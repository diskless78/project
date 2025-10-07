import logging
from pathlib import Path
from pyspark.sql import SparkSession, DataFrame

logger = logging.getLogger(__name__)

class PostgresReader:
    
    def __init__(self, spark: SparkSession, conf: dict):
        self.spark = spark

        self.jdbc_url = f"jdbc:postgresql://{conf['host']}:{conf['port']}/{conf['database']}"

        # PostgreSQL JDBC properties
        self.jdbc_properties = {
            "user": conf.get("username"),
            "password": conf.get("password"),
            "driver": "org.postgresql.Driver"
        }

        # Check for missing credentials and connection details
        missing_keys = [k for k in ["username", "password", "host", "port", "database"] if not conf.get(k)]
        if missing_keys:
            raise ValueError(f"Missing required key(s) in conf: {', '.join(missing_keys)}")
            
    # =========================================== Load login credential ============================================
    @staticmethod
    def load_credentials(file_path: Path) -> dict:
        creds = {}
        with open(file_path, "r") as f:
            for line in f:
                line = line.strip()
                if not line or line.startswith("#"):
                    continue
                if "=" not in line:
                    continue
                key, value = line.split("=", 1)
                creds[key.strip()] = value.strip().strip("'").strip('"')
        return creds

    # ============================== Query the data set to get data frame  ==========================================
    def query_table(self, sql_query: str) -> DataFrame:
        logger.info(f"SQL Query:\n{sql_query}")

        # Execute query
        try:
            df = self.spark.read.jdbc(
                url=self.jdbc_url,
                table=f"({sql_query}) query_alias",
                properties=self.jdbc_properties
            )
            logger.info("Query executed and DataFrame loaded successfully.")
            return df
        except Exception as e:
            logger.error(f"Failed to read from PostgreSQL: {e}")
            raise

    # ================================= Get Min & Max value  ==============================================
    def execute_query(self, sql_query: str) -> int:
        logger.info(f"Executing SQL Query:\n{sql_query}")

        try:
            df = self.spark.read.jdbc(
                url=self.jdbc_url,
                table=f"({sql_query}) query_alias",
                properties=self.jdbc_properties
            )

            value = df.first()[0]
            result = int(value)

            logger.info(f"Query executed successfully. Result: {result}")
            return result
        except Exception as e:
            logger.error(f"Failed to execute the query against PostgreSQL: {e}")
            raise
