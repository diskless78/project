import sys
import time
import logging
import re
from datetime import datetime, timedelta, date
from pyspark.sql import functions as F
from pyspark.sql import DataFrame
from pathlib import Path

ROOT_DIR = Path(__file__).resolve().parents[2]
sys.path.append(str(ROOT_DIR))
JOB_PATH = Path(__file__).resolve().parent

from lib.spark.session import SparkUtil
from lib.database.mssql_reader import MSSQLReader
from lib.iceberg.writer import IcebergTable
import const

# Logging
logging.basicConfig(level=logging.INFO, format="%(asctime)s [%(levelname)s] %(name)s - %(message)s")
logger = logging.getLogger(__name__)
CRED_PATH = Path(__file__).resolve().parent / ".cred"

# ================================= Read data from source database ==================================
def read_data(spark, mssql_cred: dict, sql_query: str, remove_columns: list, rename_columns: list, add_columns: list, sort_by: list, convert_columns: list) -> DataFrame:
    """Reads data from MSSQL and applies transformations."""
    try:
        
        mssql_reader = MSSQLReader(spark=spark, conf=mssql_cred)
        df = mssql_reader.query_table(sql_query)
        
        if df.count() > 0:
            df = df.toDF(*[c.lower() for c in df.columns])
            
            # Remove columns
            for col_name in remove_columns:
                if col_name in df.columns:
                    df = df.drop(col_name)
                else:
                    logger.warning(f"Column '{col_name}' not found in source DataFrame. Skipping removal.")
        
            # Rename column
            for col_from, col_to in rename_columns:
                if col_from in df.columns:
                    df = df.withColumnRenamed(col_from, col_to)
                else:
                    logger.warning(f"Column '{col_from}' not found in source DataFrame. Skipping rename to '{col_to}'.")
                    
            # Add column with value
            for col_name, value in add_columns:
                if value.lower() == "current_timestamp":
                    df = df.withColumn(col_name, F.current_timestamp())
                else:
                    df = df.withColumn(col_name, F.lit(value))
            
            # Convert source column yyyyMMdd to Date column
            for col_name, value in convert_columns:
                if isinstance(value, str):
                    convert_match = re.search(r'convert\((.*?)\)', value.lower())
                    if convert_match:
                        source_col = convert_match.group(1)
                        if source_col in df.columns:
                            df = df.withColumn(col_name, F.to_date(F.col(source_col), "yyyyMMdd"))
                        else:
                            logger.error(f"Source column '{source_col}' not found for conversion. Aborting.")
                            raise ValueError(f"Source column '{source_col}' not found.")
                    elif value.lower() == "current_timestamp":
                        df = df.withColumn(col_name, F.current_timestamp())
                    else:
                        df = df.withColumn(col_name, F.lit(value))
                else:
                    logger.error(f"Unsupported value type for column '{col_name}'. Aborting.")
                    raise ValueError(f"Unsupported value type for column '{col_name}'")
                     
            # order by the table
            if sort_by:
                logger.info("Ordering DataFrame by columns: %s", sort_by)
                df = df.orderBy(sort_by)
            
            # Shuftle the columns into each partition
            if sort_by:
                df = df.repartition(*sort_by)
                
        logger.info("Successfully read data from MSSQL. Row count: %s", df.count())
        return df
    except Exception as e:
        logger.error("Failed to read data from MSSQL: %s", e)
        raise

# ===================================== Main Job Logic ==============================================
def run_job(source_table_name: str):
    
    start_time = time.time()
    logger.info("Starting job: %s", source_table_name)
    spark = None
    
    try:
        logger.info("Initializing Spark session...")
        spark = SparkUtil.get_spark_session(source_table_name)
        logger.info("Spark session initialized.")

        # Configure mssql_reader
        creds = MSSQLReader.load_credentials(CRED_PATH)
        mssql_cred = {
            "host": creds["MSSQL_HOST"],
            "port": int(creds.get("MSSQL_PORT", 1433)),
            "username": creds["MSSQL_USERNAME"],
            "password": creds["MSSQL_PASSWORD"],
            "database": creds["MSSQL_DB"]
        }
        
        # ====================================== Get value from the txdb config =================================================
        table_config = getattr(const, source_table_name.split('.')[1].upper())
        load_start_year = getattr(table_config, 'START_YEAR', const.DEFAULT_START_YEAR)
        load_start_month = getattr(table_config, 'START_MONTH', const.DEFAULT_START_MONTH)
        incremental_month = getattr(table_config, "INCREMENTAL_MONTH", const.DEFAULT_INCREMENTAL_MONTH)
        partition_clause = getattr(table_config, 'PARTITION_CLAUSE', const.DEFAULT_PARTITION_CLAUSE)
        rename_columns = getattr(table_config, 'RENAME_COLUMNS', const.DEFAULT_RENAME_COLUMNS)
        remove_columns = getattr(table_config, 'REMOVE_COLUMNS', const.DEFAULT_REMOVE_COLUMNS)
        add_columns = getattr(table_config, "ADD_COLUMNS", const.DEFAULT_ADD_COLUMNS)
        query_key = getattr(table_config, "QUERY_KEY", const.DEFAULT_QUERY_KEY)
        sort_by = getattr(table_config, "SORT_BY", const.DEFAULT_SORT_BY)
        convert_columns = getattr(table_config, "CONVERT_COLUMNS", const.DEFAULT_CONVERT_COLUMNS)
        
        ICEBERG_TABLE = f"{const.LAKEHOUSE_CATALOG}.{const.LAKEHOUSE_NAMESPACE}.{const.LAKEHOUSE_PREFIX}{getattr(table_config, 'LAKEHOUSE_TABLENAME', source_table_name.split('.')[1].lower())}"
        # ====================================== Get value from the txdb config =================================================
        
        writer = IcebergTable(spark)
        existing_table = spark.catalog.tableExists(ICEBERG_TABLE)
        
        if load_start_year is None:  # Case: Table < 5M records
           
            logger.info("Configuration is set for 'Always Full Load'.")
            if not existing_table:
                schema_query = f"SELECT TOP 1 * FROM {source_table_name}"
                df_schema = read_data(spark, mssql_cred, schema_query, remove_columns, rename_columns, add_columns, sort_by, convert_columns)
                logger.info("Table does not exist. Creating table for full load...")
                writer.create_table(
                    df=df_schema,
                    table_name=ICEBERG_TABLE,
                    partition_clause=partition_clause,
                    order_cols=sort_by
                )
            
            logger.info("Performing full load...")
            sql_query = f"SELECT * FROM {source_table_name}"
            df = read_data(spark, mssql_cred, sql_query, remove_columns, rename_columns, add_columns, sort_by, convert_columns)
            if df.count() > 0: writer.write(df, table_name=ICEBERG_TABLE, mode = 1)
            
        elif load_start_year == 0: # Case: Table With No Timstamp Field
            
            try: # Extract batch_step from PARTITION_CLAUSE
                batch_step = int(re.search(r'\d+', partition_clause).group())
            except (AttributeError, ValueError):
                logger.error("Failed to extract a valid batch_step from PARTITION_CLAUSE. Aborting.")
                sys.exit(1)
            
            mssql_reader = MSSQLReader(spark=spark, conf=mssql_cred)
            min_value_query = f"SELECT MIN({query_key}) AS min_value FROM {source_table_name}"
            max_value_query = f"SELECT MAX({query_key}) AS max_value FROM {source_table_name}"
            
            logger.info(f"Batch step extracted: {batch_step}. Starting batched load...")
            if not existing_table:
                schema_query = f"SELECT TOP 1 * FROM {source_table_name}"
                df_schema = read_data(spark, mssql_cred, schema_query, remove_columns, rename_columns, add_columns, sort_by, convert_columns)
                logger.info("Table does not exist. Creating table for full load...")
                writer.create_table(
                    df=df_schema,
                    table_name=ICEBERG_TABLE,
                    partition_clause=partition_clause,
                    order_cols=sort_by
                )
                
                # Get min and max base on query_key
                logger.info(f"Getting MIN and MAX {query_key} to split base on batch_step.")
                
                min_value = mssql_reader.execute_query(min_value_query)
                max_value = mssql_reader.execute_query(max_value_query)
                
                while True:
                    current_max_value = min_value + batch_step

                    if current_max_value >= max_value:
                        sql_query = f"SELECT * FROM {source_table_name} WHERE {query_key} BETWEEN {min_value} AND {max_value}"
                        
                        df = read_data(spark, mssql_cred, sql_query, remove_columns, rename_columns, add_columns, sort_by, convert_columns)
                        if df.count() > 0: writer.write(df, table_name=ICEBERG_TABLE)
                        break
                    else:
                        sql_query = f"SELECT * FROM {source_table_name} WHERE {query_key} BETWEEN {min_value} AND {current_max_value}"

                        df = read_data(spark, mssql_cred, sql_query, remove_columns, rename_columns, add_columns, sort_by, convert_columns)
                        if df.count() > 0: writer.write(df, table_name=ICEBERG_TABLE)

                        min_value = current_max_value + 1
                        
            else: # Schedule: Incremental get max & min = max- batch_step
                
                max_value = mssql_reader.execute_query(max_value_query)
                min_value = (max_value // batch_step) * batch_step
                sql_query = f"SELECT * FROM {source_table_name} WHERE {query_key} BETWEEN {min_value} AND {max_value}"
                
                df = read_data(spark, mssql_cred, sql_query, remove_columns, rename_columns, add_columns, sort_by, convert_columns)
                if df.count() > 0: writer.write(df, table_name=ICEBERG_TABLE, mode = 1)
                 
        else: # Case: Normal Table -> Timestamp as varchar
            if not existing_table:
                # First run: Pulling each month per year
                schema_query = f"SELECT TOP 1 * FROM {source_table_name}"
                df_schema = read_data(spark, mssql_cred, schema_query, remove_columns, rename_columns, add_columns, sort_by, convert_columns)
                logger.info("Table does not exist. Performing full load from %s.", load_start_year)
                writer.create_table(
                    df=df_schema,
                    table_name=ICEBERG_TABLE,
                    partition_clause=partition_clause,
                    order_cols=sort_by
                )
                if load_start_month is None: # Pull year by year
                    current_year_start = load_start_year
                    current_year_end = date.today().year
                    
                    for year in range(current_year_start, current_year_end + 1):
                        sql_query = f"SELECT * FROM {source_table_name} WHERE {query_key} LIKE '{year}%'"
                        logger.info("Processing data for year: %s", year)
                        df = read_data(spark, mssql_cred, sql_query, remove_columns, rename_columns, add_columns, sort_by, convert_columns)
                        if df.count() > 0: writer.write(df, table_name=ICEBERG_TABLE)
                        
                else: # Pull month by month per year
                    current_year = date.today().year
                    current_month = date.today().month

                    start_month = load_start_month if load_start_month != 0 else 1

                    for year in range(load_start_year, current_year + 1):
                        end_month_range = 13
                        if year == current_year:
                            end_month_range = current_month + 1

                        start_month_range = start_month
                        if year > load_start_year:
                            start_month_range = 1
                            
                        for month in range(start_month_range, end_month_range):
                            month_str = f"{year}{month:02d}"
                            sql_query = f"SELECT * FROM {source_table_name} WHERE {query_key} LIKE '{month_str}%'"
                            logger.info("Processing data for year-month: %s-%s", year, month)

                            df = read_data(spark, mssql_cred, sql_query, remove_columns, rename_columns, add_columns, sort_by, convert_columns)
                            if df.count() > 0: writer.write(df, table_name=ICEBERG_TABLE)
                        
                logger.info("Finished ingesting data up to the current month of the current year.")
                
            else: # Schedule: Incremental = current - 1 to current month
                
                previous_month_date = date.today().replace(day=1) - timedelta(days=1)
                start_year = previous_month_date.year
                start_month = previous_month_date.month
                end_year = date.today().year
                end_month = date.today().month
                
                if incremental_month - 1 == 0:
                    if 1 <= date.today().day <= 7:
                        if start_year != end_year:
                            sz_last_mon = f"{start_year}{start_month:02d}"
                            sz_current_mon = f"{end_year}{end_month:02d}"
                        else:
                            sz_last_mon = f"{start_year}{start_month:02d}"
                            sz_current_mon = f"{start_year}{end_month:02d}"
                            
                        sql_query = f"SELECT * FROM {source_table_name} WHERE {query_key} LIKE '{sz_last_mon}' OR {query_key} LIKE '{sz_current_mon}%'"
                        logger.info(f"Table already exists. Performing incremental from month: {sz_last_mon}/{sz_current_mon}")
                        
                    else:
                        sz_current_mon = f"{end_year}{end_month:02d}"
                        sql_query = f"SELECT * FROM {source_table_name} WHERE {query_key} LIKE '{sz_current_mon}%'"
                        logger.info(f"Table already exists. Performing incremental from month: {sz_current_mon}")
                else: 
                    if start_year != end_year:
                        sz_last_mon = f"{start_year}{start_month:02d}"
                        sz_current_mon = f"{end_year}{end_month:02d}"
                    else:
                        sz_last_mon = f"{start_year}{start_month:02d}"
                        sz_current_mon = f"{start_year}{end_month:02d}"
                        
                    sql_query = f"SELECT * FROM {source_table_name} WHERE {query_key} LIKE '{sz_last_mon}' OR {query_key} LIKE '{sz_current_mon}%'"
                    logger.info(f"Table already exists. Performing incremental from month: {sz_last_mon}/{sz_current_mon}")
                    
                df = read_data(spark, mssql_cred, sql_query, remove_columns, rename_columns, add_columns, sort_by, convert_columns)
                if df.count() > 0: writer.write(df, table_name=ICEBERG_TABLE, mode = 1)

        logger.info("Job completed in %.2f seconds.", time.time() - start_time)
    except Exception as e:
        logger.error("Error job execution: %s", e, exc_info=True)
        sys.exit(1)
    finally:
        if spark:
            spark.stop()
            logger.info("Spark session stopped.")
