from airflow import DAG
import pendulum
from datetime import datetime
from airflow.providers.cncf.kubernetes.operators.kubernetes_pod import KubernetesPodOperator
from airflow.utils.task_group import TaskGroup
from kubernetes.client import models as k8s

# Default args
default_args = {
    "owner": "DevOps",
    "retries": 0
}

# Tags & Scheduling
TAG_NAME = ["PIC:DevOps", "Src:ora-golddb-sec", "Des:lakehouse-raw.gold", "Todo:NONE"]
startDate = datetime(year=2025, month=9, day=1)
cron_regex = "05 * * * *"  # every hour at minute 05

# Config
git_repo_url = "git@repository.centralretail.com.vn:platform/automation/spark.git"
JOB_PATH = "/opt/spark/prd/gold"

TABLE_NAME = [
    "GCEN509.CLIDGENE",
    "GCEN509.STRUCOBJ",
    "GCEN509.ARTVL",
    "GCEN509.SITDGENE",
    "GCEN509.SITATTRI",
    "GCEN509.TRA_PARPOSTES",
    "GCEN509.STRUCREL",
    "GCEN509.RESEAU",
    "GCEN509.FOUDGENE",
    "GCEN509.ARTUVATTRI",
    "GCEN509.ARTUVUL",
    "GCEN509.ARTCAISSE",
    "GCEN509.BLIENTBL",
    "GCEN509.CDEDETCDE",
    "GCEN509.CDEDISTRIB",
    "GCEN509.TRA_STRUCOBJ",
    "GCEN509.STOMVT",
    "GCEN509.STODETRE",
    "GCEN509.STOCOUCH",
    "GCEN509.CDEENTCDE",
    "GCEN509.AVEPRIX",
    "GCEN509.ARTCONS",
    "GCEN509.ARTUL",
    "GCEN509.ARTUV",
    "GCEN509.ARTRAC",
    "GCEN509.AVESCOPE",
    "GCEN509.ARTUC",
    "GCEN509.V_BRAND_SITE",
    "GCEN509.V_COCA_SITE",
    "GDBIT.V_COCA_ACTIVE"
]

# DAG
with DAG(
    dag_id="spark-gold-lakehouse",
    default_args=default_args,
    start_date=pendulum.datetime(startDate.year, startDate.month, startDate.day, tz="Asia/Ho_Chi_Minh"),
    catchup=False,
    # schedule_interval=None,
    schedule_interval=cron_regex,
    tags=TAG_NAME,
    max_active_runs=1,
    concurrency=5
) as dag:

    def spark_run_task(task_id, table_name):

        # Toleration object
        lakehouse_toleration = k8s.V1Toleration(
            key="kind",
            operator="Equal",
            value="lakehouse",
            effect="NoSchedule"
        )
        
        # Node selector
        lakehouse_node_selector = {"kind": "lakehouse"}
        
        return KubernetesPodOperator(
            task_id=task_id,
            name=f"gold-db-{table_name}",
            namespace="group-spark",
            image="harbor.centralretail.com.vn/lakehouse/lakehouse-spark:3.5.5",
            image_pull_policy="IfNotPresent",
            image_pull_secrets=[k8s.V1LocalObjectReference(name="harbor-registry-secret")],
            service_account_name="group-airflow-prod",
            cmds=["/bin/bash", "-c"],
            arguments=[
                "set -e && "
                "echo 'Cloning Spark-Python repository...' && "
                "cd /opt && rm -rf spark && "
                f"GIT_SSH_COMMAND='ssh -o StrictHostKeyChecking=no' git clone {git_repo_url} spark && "
                f"echo 'Running Spark script for {table_name}...' && "
                f"cd {JOB_PATH} && "
                "/opt/bitnami/spark/bin/spark-submit "
                "--driver-memory 2g "
                "--conf spark.driver.cores=1 "
                "--executor-memory 8g "
                "--conf spark.executor.memoryOverhead=1g "
                "--conf spark.executor.cores=2 "
                "--conf spark.executor.instances=1 "
                "--conf spark.kubernetes.executor.request.cores=2 "
                "--conf spark.kubernetes.executor.limit.cores=2 "
                f"main.py \"{table_name}\" && "
                "echo 'All scripts completed.'"
            ],
            container_resources=k8s.V1ResourceRequirements(
                limits={"memory": "12Gi", "cpu": "8"}
            ),
            # Add node selector
            node_selector=lakehouse_node_selector,
            # Add tolerations
            tolerations=[lakehouse_toleration],
            get_logs=True,
            is_delete_operator_pod=True
        )

    # Task parallelism
    with TaskGroup("spark_job") as spark_job:
        tasks = []
        for table in TABLE_NAME:
            task = spark_run_task(f"{table}_run", table)
            tasks.append(task)