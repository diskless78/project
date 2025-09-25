import logging
from pathlib import Path
from datetime import datetime
from pyspark.sql import SparkSession, DataFrame

logger = logging.getLogger(__name__)


class MSSQLReader:
    def __init__(self, spark: SparkSession, conf: dict):
       
        self.spark = spark
        self.jdbc_url = (
            f"jdbc:sqlserver://{conf['host']}:{conf['port']};"
            f"databaseName={conf['database']};"
            "encrypt=true;"
            "trustServerCertificate=true"
        )

        self.jdbc_properties = {
            "user": conf.get("username"),
            "password": conf.get("password"),
            "driver": conf.get("driver", "com.microsoft.sqlserver.jdbc.SQLServerDriver")
        }

        # Checking missing cred
        missing_keys = [k for k in ["user", "password"] if not self.jdbc_properties.get(k)]
        if missing_keys:
            raise ValueError(f"Missing required key(s) in conf: {', '.join(missing_keys)}")

# =========================================== Load login credential ============================================
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
    def query_table(
        self,
        sql_query: str
    ) -> DataFrame:
        
        logger.info(f"SQL Query:\n{sql_query}")

        # Execute query
        try:
            df = self.spark.read.jdbc(
                url=self.jdbc_url,
                table=f"({sql_query}) AS query_alias",
                properties=self.jdbc_properties
            )
            logger.info("Query executed and DataFrame loaded successfully.")
            return df
        except Exception as e:
            logger.error(f"Failed to read from MSSQL: {e}")
            raise

# ================================= Get Min & Max value  ==============================================
    def execute_query(self, sql_query: str) -> int:
            
        """
            SELECT MIN(RECID) AS min_value FROM DBO.INVENTTRANSORIGIN
            SELECT MAX(RECID) AS max_value FROM DBO.INVENTTRANSORIGIN
        """

        logger.info(f"Executing SQL Query:\n{sql_query}")

        try:
            df = self.spark.read.jdbc(
                url=self.jdbc_url,
                table=f"({sql_query}) AS query_alias",
                properties=self.jdbc_properties
            )

            value = df.first()[0]
            result = int(value)
            
            logger.info(f"Query executed successfully. Result: {result}")
            return result
        except Exception as e:
            logger.error(f"Failed to execute the query against MSSQL: {e}")
            raise
            