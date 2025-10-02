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
TAG_NAME = ["PIC:DevOps", "Src:mssql-txdb-sec", "Des:lakehouse-raw.txdb", "Todo:NONE"]
startDate = datetime(year=2025, month=8, day=1)
cron_regex = "30 * * * *"  # Every hour at minute 30

# Config
git_repo_url = "git@repository.centralretail.com.vn:platform/automation/spark.git"
JOB_PATH = "/opt/spark/prd/gold"

TABLE_NAME = [
    "GCEN509.STODETRE",
    "GCEN509.CDEDETCDE"
]

# DAG
with DAG(
    dag_id="spark-run-once",
    default_args=default_args,
    start_date=pendulum.datetime(startDate.year, startDate.month, startDate.day, tz="Asia/Ho_Chi_Minh"),
    catchup=False,
    schedule_interval=None,
    # schedule_interval=cron_regex,
    tags=TAG_NAME,
    max_active_runs=1,
    concurrency=3
) as dag:

    def spark_run_task(task_id, table_name):

        return KubernetesPodOperator(
            task_id=task_id,
            name=f"run-once-{table_name}",
            namespace="group-spark",
            image="harbor.centralretail.com.vn/lakehouse/lakehouse-spark:3.5.5",
            image_pull_policy="Always",
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
            # node_selector={"kind": "lakehouse"},
            get_logs=True,
            is_delete_operator_pod=True
        )

    # Task parallelism
    with TaskGroup("spark_job") as spark_job:
        tasks = []
        for table in TABLE_NAME:
            task = spark_run_task(f"{table}_run", table)
            tasks.append(task)