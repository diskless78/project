import sys
from pathlib import Path
from pyspark.sql.functions import col

ROOT_DIR = Path(__file__).resolve().parents[2]
sys.path.append(str(ROOT_DIR))

from lib.spark.session import SparkUtil

source_table_name = "lakehouse_raw.txdb.r_txfooter"
bizdate_filter = "2025-09-10"
retailstoreid_filter = [102]
sztatype_filter = ["RT", "RR", "SA", "VR"]


spark = None

try:
    spark = SparkUtil.get_spark_session(source_table_name)

    df = spark.table(source_table_name)

    exec_query = df.filter(
        (col("bizdate") == bizdate_filter) &
        (col("lretailstoreid").isin(retailstoreid_filter)) &
        (col("sztatype").isin(sztatype_filter))
    )

    input_files = exec_query.inputFiles()
    print("\n=== Files accessed to partition ===")
    for f in input_files:
        print(f)

    print("\n=== Physical Plan ===")
    exec_query.explain(True)
    
    print("\n=== Query Results  ===")
    exec_query.show(exec_query.count(), truncate=False) 
    
    row_count = exec_query.count()
    print(f"\n=== Total Rows: {row_count} ===\n")

finally:
    if spark:
        spark.stop()
