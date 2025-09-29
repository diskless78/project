from pyspark.sql import SparkSession
from lib.spark.config import SparkConfig
import logging
import json

logger = logging.getLogger("SparkUtil")
logger.setLevel(logging.INFO)


class SparkUtil:
    @staticmethod
    def get_spark_session(app_name: str, catalog_name: str, nessie_branch: str) -> SparkSession:
        logger.info(f"Initializing SparkSession for: {app_name}")

        # Spark config map
        config_map = SparkConfig.get_config_map(catalog_name, nessie_branch)
        logger.info("Using Spark Config Map:")
        logger.info(json.dumps(config_map, indent=2))

        # Start builder
        builder = SparkSession.builder.appName(app_name)

        # Apply configs
        for key, value in config_map.items():
            if value is not None and str(value).strip() != "":
                builder = builder.config(key, value)
                logger.debug(f"Applied config {key}={value}")
            else:
                logger.warning(f"Skipped empty Spark config key: {key}")

        # Create Spark session
        spark = builder.getOrCreate()

        # Log Spark runtime info
        logger.info("✨ SparkSession created successfully")
        logger.info(f"Spark version: {spark.version}")
        logger.info(f"App name: {spark.sparkContext.appName}")
        logger.info(f"Master: {spark.sparkContext.master}")

        return spark
