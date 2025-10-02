import sys
import time
import logging
import re
from datetime import datetime, timedelta, date
import calendar
from pyspark.sql import functions as F
from pyspark.sql import DataFrame
from pathlib import Path

ROOT_DIR = Path(__file__).resolve().parents[2]
sys.path.append(str(ROOT_DIR))

from lib.spark.session import SparkUtil
from lib.database.mssql_reader import MSSQLReader
from lib.iceberg.writer import IcebergTable
import const

# Logging setup
logging.basicConfig(level=logging.INFO, format="%(asctime)s [%(levelname)s] %(name)s - %(message)s")
logger = logging.getLogger(__name__)
CRED_PATH = Path(__file__).resolve().parent / ".cred"

# ===================================== Datetime calculator  ========================================
def get_date_range_strings(start_dt: datetime, end_dt: datetime):
    """Formats 'YYYY-MM-DD 00:00:00' strings for SQL queries."""
    
    start_date_str = start_dt.strftime('%Y-%m-%d 00:00:00')
    end_date_str = end_dt.strftime('%Y-%m-%d 00:00:00')
    
    return start_date_str, end_date_str
# ======================================== Write sql_query  ========================================
def write_sqlquery(table_name, query_key, start_dt, end_dt):
    start_date_str, end_date_str = get_date_range_strings(start_dt, end_dt)
    
    sql_query = (
        f"SELECT * FROM {table_name} "
        f"WHERE {query_key} >= '{start_date_str}' AND {query_key} < '{end_date_str}'"
    )
    
    logger.info("  Processing range: %s to %s ", start_date_str, end_date_str)
    return sql_query
# ================================= Read data from source database ==================================
def read_data(spark, mssql_cred: dict, sql_query: str, remove_columns: list, rename_columns: list, add_columns: list, convert_columns: list) -> DataFrame:
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
            
            # Convert datetime as varchar such as yyyyMMdd and yyyy-MM-dd
            for col_name, value in convert_columns:
                if isinstance(value, str):
                    convert_match = re.search(r'convert\((.*?)\)', value.lower())
                    if convert_match:
                        source_col = convert_match.group(1)
                        if source_col in df.columns:
                            df = df.withColumn(
                                col_name,
                                F.coalesce(
                                    F.to_date(F.col(source_col), "yyyyMMdd"),
                                    F.to_date(F.col(source_col), "yyyy-MM-dd")
                                )
                            )
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
                
        logger.info("Successfully read data from MSSQL. Row count: %s", df.count())
        return df
    except Exception as e:
        logger.error("Failed to read data from MSSQL: %s", e)
        raise

# ===================================== Main Job Logic ==============================================
def run_job(source_table_name: str):
    
    start_time = time.time()
    logger.info("Starting job for table: %s", source_table_name)
    spark = None
    
    try:
        # Configure mssql_reader
        creds = MSSQLReader.load_credentials(CRED_PATH)
        mssql_cred = {
            "host": creds["MSSQL_HOST"],
            "port": int(creds.get("MSSQL_PORT")),
            "username": creds["MSSQL_USERNAME"],
            "password": creds["MSSQL_PASSWORD"],
            "database": creds["MSSQL_DB"]
        }
        
        # ====================================== Get value from the axdb config =================================================
        table_config = getattr(const, source_table_name.split('.')[1].upper())
        load_start_year = getattr(table_config, 'START_YEAR', const.DEFAULT_START_YEAR)
        load_start_month = getattr(table_config, 'START_MONTH', const.DEFAULT_START_MONTH)
        incremental_month = getattr(table_config, "INCREMENTAL_MONTH", const.DEFAULT_INCREMENTAL_MONTH)
        incremental_per_day = getattr(table_config, "INCREMENTAL_PER_DAY", const.DEFAUL_INCREMENTAL_PER_DAY)
        full_per_day = getattr(table_config, "FULL_PER_DAY", const.DEFAULT_FULL_PER_DAY)
        incremental_day = getattr(table_config, "INCREMENTAL_DAY", const.DEFAULT_INCREMENTAL_DAY)
        partition_clause = getattr(table_config, 'PARTITION_CLAUSE', const.DEFAULT_PARTITION_CLAUSE)
        remove_columns = getattr(table_config, 'REMOVE_COLUMNS', const.DEFAULT_REMOVE_COLUMNS)
        rename_columns = getattr(table_config, 'RENAME_COLUMNS', const.DEFAULT_RENAME_COLUMNS)
        add_columns = getattr(table_config, "ADD_COLUMNS", const.DEFAULT_ADD_COLUMNS)
        convert_columns = getattr(table_config, "CONVERT_COLUMNS", const.DEFAULT_CONVERT_COLUMNS)
        query_key = getattr(table_config, "QUERY_KEY", const.DEFAULT_QUERY_KEY)
        
          # This is used to pull for a period year/month
        period_config = getattr(table_config, 'PULL_PERIOD_CONFIG', {})
        enable_period = period_config.get("ENABLE_PERIOD", "N")
        
        ICEBERG_TABLE = f"{const.LAKEHOUSE_CATALOG}.{const.LAKEHOUSE_NAMESPACE}.{const.LAKEHOUSE_PREFIX}{getattr(table_config, 'LAKEHOUSE_TABLENAME', source_table_name.split('.')[1].lower())}"
        SPARK_APP = source_table_name.split('.')[1]
        # ====================================== Get value from the axdb config =================================================
        logger.info("Initializing Spark session...")
        spark = SparkUtil.get_spark_session(SPARK_APP, const.LAKEHOUSE_CATALOG, const.NESSIE_BRANCH)
        
        writer = IcebergTable(spark)
        existing_table = spark.catalog.tableExists(ICEBERG_TABLE)
        
        if load_start_year is None:  # Case: Table < 5M records
           
            logger.info("Configuration is set for 'Always Full Load'.")
            if not existing_table:
                schema_query = f"SELECT TOP 1 * FROM {source_table_name}"
                df_schema = read_data(spark, mssql_cred, schema_query, remove_columns, rename_columns, add_columns, convert_columns)
                logger.info("Table does not exist. Creating table for full load...")
                writer.create_table(
                    df=df_schema,
                    table_name=ICEBERG_TABLE,
                    partition_clause=partition_clause
                )
            
            logger.info("Performing full load...")
            sql_query = f"SELECT * FROM {source_table_name}"
            df = read_data(spark, mssql_cred, sql_query, remove_columns, rename_columns, add_columns, convert_columns)
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
                df_schema = read_data(spark, mssql_cred, schema_query, remove_columns, rename_columns, add_columns, convert_columns)
                logger.info("Table does not exist. Creating table for full load...")
                writer.create_table(
                    df=df_schema,
                    table_name=ICEBERG_TABLE,
                    partition_clause=partition_clause
                )
                # Get min and max base on query_key
                logger.info(f"Getting MIN and MAX {query_key} to split base on batch_step.")
                
                min_value = mssql_reader.execute_query(min_value_query)
                max_value = mssql_reader.execute_query(max_value_query)
                
                while True:
                    current_max_value = min_value + batch_step

                    if current_max_value >= max_value:
                        sql_query = f"SELECT * FROM {source_table_name} WHERE {query_key} BETWEEN {min_value} AND {max_value}"
                        
                        df = read_data(spark, mssql_cred, sql_query, remove_columns, rename_columns, add_columns, convert_columns)
                        if df.count() > 0: writer.write(df, table_name=ICEBERG_TABLE)
                        break
                    else:
                        sql_query = f"SELECT * FROM {source_table_name} WHERE {query_key} BETWEEN {min_value} AND {current_max_value}"

                        df = read_data(spark, mssql_cred, sql_query, remove_columns, rename_columns, add_columns, convert_columns)
                        if df.count() > 0: writer.write(df, table_name=ICEBERG_TABLE)

                        min_value = current_max_value + 1
                        
            else: # Schedule: Incremental get max & min = max- batch_step
                
                max_value = mssql_reader.execute_query(max_value_query)
                min_value = (max_value // batch_step) * batch_step
                sql_query = f"SELECT * FROM {source_table_name} WHERE {query_key} BETWEEN {min_value} AND {max_value}"
                
                df = read_data(spark, mssql_cred, sql_query, remove_columns, rename_columns, add_columns, convert_columns)
                if df.count() > 0: writer.write(df, table_name=ICEBERG_TABLE, mode = 1)
                 
        else: # Case: Normal Table -> Timestamp Field
            if not existing_table:
                # First run: Pulling each year
                schema_query = f"SELECT TOP 1 * FROM {source_table_name}"
                df_schema = read_data(spark, mssql_cred, schema_query, remove_columns, rename_columns, add_columns, convert_columns)
                logger.info("Table does not exist. Performing full load from %s.", load_start_year)
                writer.create_table(
                    df=df_schema,
                    table_name=ICEBERG_TABLE,
                    partition_clause=partition_clause
                )
                
                if load_start_month is None: # Pull year by year
                    current_year_start = load_start_year
                    current_year_end = date.today().year
                    
                    for year in range(current_year_start, current_year_end + 1):
                        sql_query = f"SELECT * FROM {source_table_name} WHERE YEAR({query_key}) = {year}"
                        logger.info("Processing data for year: %s", year)
                        df = read_data(spark, mssql_cred, sql_query, remove_columns, rename_columns, add_columns, convert_columns)
                        if df.count() > 0: writer.write(df, table_name=ICEBERG_TABLE)
                        
                else: # Pull month by month per year
                    current_date = date.today()
                    current_year = current_date.year
                    current_month = current_date.month

                    start_month = load_start_month if load_start_month != 0 else 1

                    for year in range(load_start_year, current_year + 1):
                        end_month_range = 13
                        if year == current_year:
                            end_month_range = current_month + 1

                        start_month_range = start_month
                        if year > load_start_year:
                            start_month_range = 1
                            
                        for month in range(start_month_range, end_month_range):
                            if full_per_day.upper() == "N": 
                                # Monthly Pull
                                start_dt = datetime(year, month, 1)
                                if month == 12:
                                    end_dt = datetime(year + 1, 1, 1)
                                else:
                                    end_dt = datetime(year, month + 1, 1)

                                logger.info("Processing monthly full load: %s-%s", year, month)
                                sql_query = write_sqlquery(source_table_name, query_key, start_dt, end_dt)
                                
                                df = read_data(spark, mssql_cred, sql_query, remove_columns, rename_columns, add_columns, convert_columns)
                                if df.count() > 0: writer.write(df, table_name=ICEBERG_TABLE)
                                
                            elif full_per_day.upper() == "Y":
                                # Daily Pull
                                num_days = calendar.monthrange(year, month)[1]
                                logger.info("Processing daily full load: %s-%s (%d total days).", year, month, num_days)
                                
                                for day in range(1, num_days + 1):
                                    start_dt = datetime(year, month, day)
                                    end_dt = start_dt + timedelta(days=1)

                                    if end_dt.date() > current_date:
                                        logger.info("  Skipping future date: %s", start_dt.date())
                                        continue 

                                    sql_query = write_sqlquery(source_table_name, query_key, start_dt, end_dt)
                                    
                                    df = read_data(spark, mssql_cred, sql_query, remove_columns, rename_columns, add_columns, convert_columns)
                                    if df.count() > 0: writer.write(df, table_name=ICEBERG_TABLE)

                            else:
                                logger.error("Invalid value for 'pull_day' in full load mode. Must be 'Y' or 'N'. Aborting month %s-%s.", month, year)
                                sys.exit(1)
                
            else: # Schedule: Incremental = current - 1 to current month
                
                logger.info("Table exists. Performing Incremental Load.")
                
                # If Pull Period is enabled
                if isinstance(enable_period, str) and enable_period.upper() == "Y":
                    period_start_year = period_config.get("START_YEAR")
                    period_start_month = period_config.get("START_MONTH")
                    period_end_year = period_config.get("END_YEAR")
                    period_end_month = period_config.get("END_MONTH")
                    period_pull_day = period_config.get("PULL_DAY")
                    
                    for year in range(period_start_year, period_end_year):
                        end_month_range = 13
                        if year == period_end_year:
                            end_month_range = period_end_month + 1

                        start_month_range = period_start_month if year == period_start_year else 1

                        for month in range(start_month_range, end_month_range):
                            if period_pull_day.upper() == "N": 
                                # Monthly Pull
                                start_dt = datetime(year, month, 1)
                                if month == 12:
                                    end_dt = datetime(year + 1, 1, 1)
                                else:
                                    end_dt = datetime(year, month + 1, 1)

                                logger.info("Processing monthly full load: %s-%s", year, month)
                                sql_query = write_sqlquery(source_table_name, query_key, start_dt, end_dt)
                                
                                df = read_data(spark, mssql_cred, sql_query, remove_columns, rename_columns, add_columns, convert_columns)
                                if df.count() > 0: writer.write(df, table_name=ICEBERG_TABLE, mode = 1)
                                
                            elif period_pull_day.upper() == "Y":
                                # Daily Pull
                                num_days = calendar.monthrange(year, month)[1]
                                logger.info("Processing daily full load: %s-%s (%d total days).", year, month, num_days)
                                
                                for day in range(1, num_days + 1):
                                    start_dt = datetime(year, month, day)
                                    end_dt = start_dt + timedelta(days=1)

                                    if end_dt.date() > date.today():
                                        logger.info("  Skipping future date: %s", start_dt.date())
                                        continue 

                                    sql_query = write_sqlquery(source_table_name, query_key, start_dt, end_dt)
                                    
                                    df = read_data(spark, mssql_cred, sql_query, remove_columns, rename_columns, add_columns, convert_columns)
                                    if df.count() > 0: writer.write(df, table_name=ICEBERG_TABLE, mode = 1)

                            else:
                                logger.error("Invalid value for 'pull_day' in full load mode. Must be 'Y' or 'N'. Aborting month %s-%s.", month, year)
                                sys.exit(1)

                if incremental_per_day.upper() == "N": 
                    # Monthly Incremental
                    
                    current_month_start = date.today().replace(day=1)
                    previous_month_start = current_month_start - timedelta(days=1)
                    previous_month_start = previous_month_start.replace(day=1)
                    current_day = date.today().day
                    end_date_sql = (date.today().replace(day=28) + timedelta(days=4)).replace(day=1)
                    start_date_sql = None
                    
                    if incremental_month == 1:
                        previous_month_start = current_month_start - timedelta(days=1)
                        previous_month_start = previous_month_start.replace(day=1)

                        if 1 <= current_day <= 7:
                            start_date_sql = previous_month_start
                            logger.info(f"Table exists. Monthly Incremental from Previous Month ({previous_month_start}) up to Current Month")
                        else:
                            start_date_sql = current_month_start
                            logger.info(f"Table exists. Monthly Incremental for Current Month ({current_month_start})")
                        
                        sql_query = write_sqlquery(source_table_name, query_key, datetime.combine(start_date_sql, datetime.min.time()), datetime.combine(end_date_sql, datetime.min.time()))
                        
                        df = read_data(spark, mssql_cred, sql_query, remove_columns, rename_columns, add_columns, convert_columns)
                        if df.count() > 0: writer.write(df, table_name=ICEBERG_TABLE, mode = 1)
                              
                    elif incremental_month > 1:
                        months_to_go_back = incremental_month - 1
                        
                        temp_dt = current_month_start 
                        
                        for _ in range(months_to_go_back):
                            temp_dt = temp_dt - timedelta(days=1)
                            temp_dt = temp_dt.replace(day=1)
                        
                        start_date_sql = temp_dt
                        logger.info(f"Table exists. Monthly Incremental from {incremental_month} months ago ({start_date_sql}) up to Current Month.")
                        sql_query = write_sqlquery(source_table_name, query_key, datetime.combine(start_date_sql, datetime.min.time()), datetime.combine(end_date_sql, datetime.min.time()))
                        
                        df = read_data(spark, mssql_cred, sql_query, remove_columns, rename_columns, add_columns, convert_columns)
                        if df.count() > 0: writer.write(df, table_name=ICEBERG_TABLE, mode = 1)
                        
                    else:
                        logger.error("Invalid setting: incremental_month must be >=1. Aborting.")
                        sys.exit(1)
                        
                elif incremental_per_day.upper() == "Y":
                    
                    # Start date: N days ago at 00:00:00
                    start_date_sql = date.today() - timedelta(days=incremental_day)
                    # End date: Tomorrow at 00:00:00
                    end_date_sql = date.today() + timedelta(days=1)
                    
                    logger.info(f"Monthly Incremental from {start_date_sql} up to {end_date_sql}")
                    sql_query = write_sqlquery(source_table_name, query_key, datetime.combine(start_date_sql, datetime.min.time()), datetime.combine(end_date_sql, datetime.min.time()))
                    
                    df = read_data(spark, mssql_cred, sql_query, remove_columns, rename_columns, add_columns, convert_columns)
                    if df.count() > 0: writer.write(df, table_name=ICEBERG_TABLE, mode = 1)
                    
                else:
                    logger.error("Invalid value for 'pull_day' in incremental mode. Must be 'Y' or 'N'. Aborting.")
                    sys.exit(1)

        logger.info("Job completed in %.2f seconds.", time.time() - start_time)
        
    except Exception as e:
        logger.error("Error job execution: %s", e, exc_info=True)
        sys.exit(1)
    finally:
        if spark:
            spark.stop()
            logger.info("Spark session stopped.")
