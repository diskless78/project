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
TAG_NAME = ["PIC:DevOps", "Src:ora-golddb-archived", "Des:lakehouse_archive.gold_archive", "Todo:NONE"]
startDate = datetime(year=2025, month=9, day=1)
cron_regex = "10 1 * * *"  # run daily at 01:10 AM

# Config
git_repo_url = "git@repository.centralretail.com.vn:platform/automation/spark.git"
JOB_PATH = "/opt/spark/prd/gold_archive"

TABLE_NAME = [
    "GCEN509.TARCRIT",
    "GCEN509.AVESCOPE",
    "GCEN509.AVETAR",
    "GCEN509.TRA_AVETAR",
    "GCEN509.CDEENTRE",
    "GCEN509.STODETTRA",
    "GCEN509.STOENTTRA",
    "GCEN509.STOLITIGE",
    "GCEN509.CNSMVT",
    "GCEN509.INVCORREC",
    "GCEN509.INVCRITINV",
    "GCEN509.INVENTINV",
    "GCEN509.INVETAPE",
    "GCEN509.INVSUIVI",
    "GCEN509.STOMVSEMAINE",
    "GCEN509.ARTUC",
    "GCEN509.TARPRIX",
    "GCEN509.TAREMISE",
    "GCEN509.TAREXAR",
    "GCEN509.TARGRAR",
    "GCEN509.TARGRAT",
    "GCEN509.TARINCO",
    "GCEN509.TARGRUL",
    "GCEN509.AVEPRIX",
    "GCEN509.RAOVENTTHEO",
    "GCEN509.RAOPARAM",
    "GCEN509.ARTPERIODE",
    "GCEN509.STODETRE",
    "GCEN509.STOENTRE",
    "GCEN509.STOPIERE",
    "GCEN509.STOREMRE",
    "GCEN509.CDEDETRE",
    "GCEN509.STOMAVOY",
    "GCEN509.STOMVT",
    "GCEN509.STODEPR",
    "GCEN509.STODEPV",
    "GCEN509.STODEPA",
    "GCEN509.CNSMVTERR",
    "GCEN509.INVDETINV",
    "GCEN509.INVSAISIE",
    "GCEN509.STOMVPERIODE",
    "GCEN509.STOMVMOIS",
    "GCEN509.VTEREMART"
]

# DAG
with DAG(
    dag_id="spark-goldb_archive-lakehouse",
    default_args=default_args,
    start_date=pendulum.datetime(startDate.year, startDate.month, startDate.day, tz="Asia/Ho_Chi_Minh"),
    catchup=False,
    schedule_interval=None,
    # schedule_interval=cron_regex,
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
            name=f"gold-archive-{table_name}",
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
            # node_selector=lakehouse_node_selector,
            # tolerations=[lakehouse_toleration],
            get_logs=True,
            is_delete_operator_pod=True
        )

    # Task parallelism
    with TaskGroup("spark_job") as spark_job:
        tasks = []
        for table in TABLE_NAME:
            task = spark_run_task(f"{table}_run", table)
            tasks.append(task)