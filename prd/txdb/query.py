import sys
from pathlib import Path
from pyspark.sql import SparkSession
from pyspark.sql.functions import col

ROOT_DIR = Path(__file__).resolve().parents[2]
sys.path.append(str(ROOT_DIR))
JOB_PATH = Path(__file__).resolve().parent

from lib.spark.session import SparkUtil

# Table and filter info
source_table_name = "lakehouse_raw.txdb.r_txfooter"
bizdate_filter = "2025-09-10"
retail_store_ids = [102]
ta_types = ["RT", "RR", "SA", "VR"]


spark = None

try:
    # Get Spark session
    spark = SparkUtil.get_spark_session(source_table_name)

    df = spark.table(source_table_name)

    filtered_df = df.filter(
        (col("bizdate") == bizdate_filter) &
        (col("lRetailStoreID").isin(retail_store_ids)) &
        (col("szTaType").isin(ta_types))
    )

    input_files = filtered_df.inputFiles()
    print("\n=== Files accessed for this query (pruned partitions) ===")
    for f in input_files:
        print(f)

    print("\n=== Optimized Physical Plan ===")
    filtered_df.explain(True)

    print("\n=== Query Results Preview  ===")
    filtered_df.show(filtered_df.count(), truncate=False) 


finally:
    if spark:
        spark.stop()
