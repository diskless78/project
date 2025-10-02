from airflow import DAG
import pendulum
from datetime import datetime
from airflow.providers.cncf.kubernetes.operators.kubernetes_pod import KubernetesPodOperator
from airflow.utils.task_group import TaskGroup
from kubernetes.client import models as k8s

# Default args
default_args = {
    "owner": "DevOps",
    "retries": 0,
}

# Repo URL
git_repo_url = "git@repository.centralretail.com.vn:platform/automation/spark.git"

CRON_SCHEDULE = "59 23 * * *"
TAG_NAME = ["PIC:DevOps", "Todo:Cleansing", "Src:Iceberg"]

# Volume and PVC
PVC_NAME = "lakehouse-spark-orphaned-pvc"
MOUNT_PATH = "/orphaned_backup"
VOLUME_NAME = "orphaned-backup-volume"

# --- Spark Configuration ---
SPARK_CONF = (
    "--driver-memory 2g "
    "--conf spark.driver.cores=1 "
    "--executor-memory 4g "
    "--conf spark.executor.memoryOverhead=1g "
    "--conf spark.executor.cores=1 "
    "--conf spark.executor.instances=1 "
    "--conf spark.kubernetes.executor.request.cores=1 "
    "--conf spark.kubernetes.executor.limit.cores=1 "
)

# Define the DAG
with DAG(
    dag_id="spark-maintenance-lakehouse",
    default_args=default_args,
    start_date=pendulum.datetime(2025, 8, 1, tz="Asia/Ho_Chi_Minh"),
    schedule_interval=CRON_SCHEDULE,
    # schedule_interval=None,
    catchup=False,
    tags=TAG_NAME,
    max_active_runs=1,
) as dag:

    # 1. K8s Volume configuration
    volume = k8s.V1Volume(
        name=VOLUME_NAME,
        persistent_volume_claim=k8s.V1PersistentVolumeClaimVolumeSource(claim_name=PVC_NAME)
    )

    # 2. K8s Volume Mount configuration
    volume_mount = k8s.V1VolumeMount(
        name=VOLUME_NAME,
        mount_path=MOUNT_PATH,
        read_only=False
    )

    MAINTENANCE_SCRIPTS = [
        ("expired_snapshot", "/opt/spark/prd/maintenance/expired_snapshot.py", "spark"),
        ("clean_orphaned_table", "/opt/spark/prd/maintenance/clean_orphaned_table.py", "spark"),
        ("clean_old_backup_folder", "/opt/spark/prd/maintenance/clean_old_backup_folder.py", "python"),
    ]

    def create_maintenance_task(task_suffix, script_path, run_type):
        """Creates a KubernetesPodOperator for a single maintenance script."""
        
        if run_type == "spark":
            execution_command = f"/opt/bitnami/spark/bin/spark-submit {SPARK_CONF} {script_path}"
        elif run_type == "python":
            execution_command = f"/opt/bitnami/python/bin/python3 {script_path}"
        else:
            raise ValueError(f"Unknown run_type: {run_type}")

        full_command = (
            "set -e && "
            "echo 'Cloning spark repository...' && "
            "cd /opt && rm -rf spark && "
            "if GIT_SSH_COMMAND='ssh -o StrictHostKeyChecking=no' git clone {git_repo_url}; then "
            "  echo 'Git cloned successfully'; "
            "else "
            "  echo 'Git clone failed!' && exit 1; "
            "fi && "
            f"echo 'Running script: {script_path}...' && "
            f"{execution_command} && "
            "echo 'Script completed.' "
        ).format(git_repo_url=git_repo_url)

        return KubernetesPodOperator(
            task_id=f"run_{task_suffix}_job",
            name=f"iceberg-maintenance-{task_suffix}",
            namespace="group-spark",
            image="harbor.centralretail.com.vn/lakehouse/lakehouse-spark:3.5.5",
            image_pull_policy="IfNotPresent",
            image_pull_secrets=[k8s.V1LocalObjectReference(name="harbor-registry-secret")],

            # Define resources (kept the same as original)
            container_resources=k8s.V1ResourceRequirements(
                limits={"memory": "8Gi", "cpu": "4"}
            ),

            cmds=["/bin/bash", "-c"],
            arguments=[full_command],

            volumes=[volume],
            volume_mounts=[volume_mount],

            # Pod settings
            get_logs=True,
            is_delete_operator_pod=True
        )

    with TaskGroup("iceberg_maintenance_jobs") as maintenance_group:
        
        # Create tasks based on the list
        snapshot_task = create_maintenance_task(*MAINTENANCE_SCRIPTS[0])
        orphaned_task = create_maintenance_task(*MAINTENANCE_SCRIPTS[1])
        backup_clean_task = create_maintenance_task(*MAINTENANCE_SCRIPTS[2])

        snapshot_task >> orphaned_task >> backup_clean_task