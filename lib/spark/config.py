import os
import logging
logger = logging.getLogger("SparkConfig")

class SparkConfig:
    """Provides SparkSession configuration for Iceberg + Nessie + S3."""

    @staticmethod
    def get_env(key: str, required: bool = True) -> str:
        val = os.environ.get(key)
        if val is None and required:
            logger.warning(f"Missing required env var: {key}")
        return val

    @staticmethod
    def get_config_map() -> dict:
        lakehouse_bucket = SparkConfig.get_env('LAKEHOUSE_RAW_BUCKET')
        nessie_endpoint = SparkConfig.get_env('NESSIE_ENDPOINT_RAW')
        nessie_token = SparkConfig.get_env('NESSIE_TOKEN_RAW')
        s3_accesskey = SparkConfig.get_env('LAKEHOUSE_RAW_ACCESSKEY')
        s3_secretkey = SparkConfig.get_env('LAKEHOUSE_RAW_SECRETKEY')
        s3_endpoint = SparkConfig.get_env('S3_ENDPOINT')
        
        return {
            # Iceberg with Nessie
            "spark.sql.catalog.lakehouse_raw": "org.apache.iceberg.spark.SparkCatalog",
            "spark.sql.catalog.lakehouse_raw.catalog-impl": "org.apache.iceberg.nessie.NessieCatalog",
            "spark.sql.catalog.lakehouse_raw.warehouse": f"s3a://{lakehouse_bucket}/warehouse",
            "spark.sql.catalog.lakehouse_raw.uri": f"{nessie_endpoint}",
            "spark.sql.catalog.lakehouse_raw.ref": "main",
            "spark.sql.catalog.lakehouse_raw.authentication.type": "BEARER",
            "spark.sql.catalog.lakehouse_raw.authentication.token": f"{nessie_token}",
            
            # S3
            "spark.hadoop.fs.s3a.endpoint": f"{s3_endpoint}:443",
            f"spark.hadoop.fs.s3a.bucket.{lakehouse_bucket}.access.key": f"{s3_accesskey}",
            f"spark.hadoop.fs.s3a.bucket.{lakehouse_bucket}.secret.key": f"{s3_secretkey}",
            f"spark.hadoop.fs.s3a.bucket.{lakehouse_bucket}.aws.credentials.provider": "org.apache.hadoop.fs.s3a.SimpleAWSCredentialsProvider",

            "spark.hadoop.fs.s3a.path.style.access": "true",
            "spark.hadoop.fs.s3a.connection.ssl.enabled": "true",
            "spark.hadoop.fs.s3a.impl": "org.apache.hadoop.fs.s3a.S3AFileSystem",
            "spark.hadoop.fs.s3.impl": "org.apache.hadoop.fs.s3a.S3AFileSystem",
        }