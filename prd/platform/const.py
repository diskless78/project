""".   WHEN REMOVE ANY CONFIGURATION IN THE CLASS WHICH IS DECLARED FOR THE TABLE. THAT MEANS, IT WILL BE USED THE DEFAULT SETTING.
    
    1. MSSQL_TABLE need to be accurate with Oracle DB (UPPERCASE, lowercase or TitleCase).
    2. MSSQL_TABLE need to be matched in the const.py of the declaration (UPPERCASE, lowercase or TitleCase).
    3. ORDER BY Should be configured despite small/big table, otherwise leave it blank.
    4. Table in lakehouse will be used lowercase as our naming convention.
    5. If START_YEAR = 0 then pulling data base on min and max of the QUERY_KEY, this case need to partition with truncate
    6. If DEFAULT_START_YEAR = None then pulling all the data without QUERY_KEY
    7. If DEFAULT_START_MONTH = None which mean pull year by year - if it's number included zero which mean pull month by month per year starting with START_MONTH (START_MONTH = 0/1 are the same).
    8. If LAKEHOUSE_TABLENAME is not defined then the iceberg table will be used LAKEHOUSE_PREFIX + SOURCE_TABLENAME
    9. If PULL_DAY = "Y" then pulling data day by day based on INCREMENTAL_DAY, START_YEAR, START_MONTH and ignore INCREMENTAL_MONTH
    10. If PULL_DAY = "N" then pulling data month by month based on INCREMENTAL_MONTH, START_YEAR, START_MONTH and ignore INCREMENTAL_DAY
    
    
    LAKEHOUSE_TABLENAME = "txfooter_created_by_dremio"
    PARTITION_CLAUSE = "truncate(`recid`, 1000000)" ---- Example: months(`bizdate`), bucket(16, `lretailstoreid`)
    PARTITION_CLAUSE = "truncate(6, `szdate`)"
    PARTITION_CLAUSE = "date(`bizdate`)"
    PARTITION_CLAUSE = "days(`bizdate`)"
    PARTITION_CLAUSE = "months(`bizdate`)"
    PARTITION_CLAUSE = "year(`bizdate`)"
    PARTITION_CLAUSE = "szdate"
    
    REMOVE_COLUMNS = ["trxid"]
    
    ADD_COLUMNS = [
        ("lsystemtype", "pos"),
        ("dcre", "current_timestamp")
    ]
    
    CONVERT_COLUMNS = [
        ("bizdate", "convert(szdate)")
    ]

    RENAME_COLUMNS = [
        ("OWNER", "owner_id"),
        ("partition", "partition_id")
    ]
    INCREMENTAL_MONTH = 2, DEFAULT_INCREMENTAL_MONTH = 1
    START_YEAR = 2020 - START_YEAR = 2024 (Remove this in the Class mean None)
    START_MONTH = 0 - START_MONTH = 1  - START_MONTH = 9 (Remove this in the Class mean None)
    
    PULL_DAY = "Y"
    INCREMENTAL_DAY = 3
    
"""


# =============================== Global Declarations ===============================
NESSIE_BRANCH = "audit"
LAKEHOUSE_CATALOG = "lakehouse_platform"
LAKEHOUSE_NAMESPACE = "payment_gw"
LAKEHOUSE_PREFIX = "strapi_"

DEFAULT_START_YEAR = None
DEFAULT_START_MONTH = None
DEFAULT_INCREMENTAL_MONTH = 1
DEFAULT_PULL_DAY = "N"
DEFAULT_INCREMENTAL_DAY = 1
DEFAULT_PARTITION_CLAUSE = ""
DEFAULT_QUERY_KEY = ""
DEFAULT_REMOVE_COLUMNS = []
DEFAULT_RENAME_COLUMNS = []
DEFAULT_ADD_COLUMNS = []
DEFAULT_CONVERT_COLUMNS = []
# ============================== TXDB-POS ===========================================
class REQUEST_LOGS:
    QUERY_KEY = "created_at"
    PARTITION_CLAUSE = "date(`created_at`)"
    START_YEAR = 2024
    START_MONTH = 1
    PULL_DAY = "Y"
# ===================================================================================

