import logging
from typing import Optional, List
from pyspark.sql import DataFrame, SparkSession
from pyspark.sql.types import *

logger = logging.getLogger(__name__)

class IcebergTable:

    def __init__(self, spark: SparkSession):
        self.spark = spark

# =============================== Define data type =========================================
    def _spark_type_to_sql(self, dtype) -> str:
        if isinstance(dtype, StringType):
            return "STRING"
        elif isinstance(dtype, IntegerType):
            return "INT"
        elif isinstance(dtype, LongType):
            return "BIGINT"
        elif isinstance(dtype, FloatType):
            return "FLOAT"
        elif isinstance(dtype, DoubleType):
            return "DOUBLE"
        elif isinstance(dtype, BooleanType):
            return "BOOLEAN"
        elif isinstance(dtype, DateType):
            return "DATE"
        elif isinstance(dtype, TimestampType):
            return "TIMESTAMP"
        elif isinstance(dtype, DecimalType):
            # DecimalType stores precision and scale
            return f"DECIMAL({dtype.precision},{dtype.scale})"
        else:
            logger.warning(f"Unrecognized Spark type: {dtype}. Defaulting to STRING.")
            return "STRING"

    # =============================== Create table base on df ===============================
    def create_table(
        self,
        df: DataFrame,
        table_name: str,
        partition_clause: str = "",
        order_cols: Optional[List[str]] = None
    ):
        try:
            full_table_path = table_name.split('.')
            if len(full_table_path) == 3:
                catalog_name = full_table_path[0]
                database_name = full_table_path[1]
                
                self.spark.sql(f"CREATE NAMESPACE IF NOT EXISTS {catalog_name}.{database_name}")
                
                logger.info("Namespace %s.%s creation completed or already exists.", catalog_name, database_name)
            else:
                logger.warning("Table name %s does not contain a database. Skipping database creation.", table_name)
        except Exception as e:
            logger.error("Failed to create database. Error: %s", e)
            return
        
        # Columns from DataFrame
        columns_sql = []
        for field in df.schema.fields:
            sql_type = self._spark_type_to_sql(field.dataType)
            columns_sql.append(f"{field.name} {sql_type}")
        columns_sql_str = ",\n    ".join(columns_sql)

        # Partition clause
        partition_sql = f"\nPARTITIONED BY ({partition_clause})" if partition_clause else ""

        # Table properties
        props = {
            "format-version": "2",
            "write.format.default": "parquet",
            "gc.enabled": "true",
            "write.target-file-size-bytes": "536870912",
            "write.parquet.row-group-size-bytes": "134217728",
            "write.metadata.delete-after-commit.enabled": "true",
            "write.metadata.previous-versions-max": "5"
        }

        if order_cols:
            props["write.order-by"] = ", ".join(order_cols)

        props_sql = ",\n    ".join([f"'{k}' = '{v}'" for k, v in props.items()])

        # Transformed
        ddl = f"""CREATE TABLE IF NOT EXISTS {table_name} (
            {columns_sql_str}
        ){partition_sql}
        TBLPROPERTIES (
            {props_sql}
        )"""

        logger.info("Creating Iceberg table with DDL:\n%s", ddl)
        self.spark.sql(ddl)
        logger.info("Table %s creation completed or already exists.", table_name)

    # =========================== Write mode 0 & 1 ===============================
    def write(self, df: DataFrame, table_name: str, mode: int = 0):
        """
        mode: 0 = append, 1 = overwrite dynamic partitions
        """
        logger.info("Writing to Iceberg table: %s", table_name)
        writer = df.writeTo(table_name)
        if mode == 0:
            writer.append()
        else:
            writer.overwritePartitions()
        logger.info("Data write completed.")