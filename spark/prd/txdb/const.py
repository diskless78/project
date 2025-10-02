""".   WHEN REMOVE ANY CONFIGURATION IN THE CLASS WHICH IS DECLARED FOR THE TABLE. THAT MEANS, IT WILL BE USED THE DEFAULT SETTING.
    
    1. MSSQL_TABLE need to be accurate with Oracle DB (UPPERCASE, lowercase or TitleCase).
    2. MSSQL_TABLE need to be matched in the const.py of the declaration (UPPERCASE, lowercase or TitleCase).
    3. ORDER BY Should be configured despite small/big table, otherwise leave it blank.
    4. Table in lakehouse will be used lowercase as our naming convention.
    5. If START_YEAR = 0 then pulling data base on min and max of the QUERY_KEY, this case need to partition with truncate
    6. If DEFAULT_START_YEAR = None then pulling all the data without QUERY_KEY
    7. If DEFAULT_START_MONTH = None which mean pull year by year - if it's number included zero which mean pull month by month per year starting with START_MONTH (START_MONTH = 0/1 are the same).
    8. If LAKEHOUSE_TABLENAME is not defined then the iceberg table will be used LAKEHOUSE_PREFIX + SOURCE_TABLENAME
    
    LAKEHOUSE_TABLENAME = "txfooter_created_by_dremio"
    PARTITION_CLAUSE = "truncate(`recid`, 1000000)"
    PARTITION_CLAUSE = months(`bizdate`), bucket(16, `lretailstoreid`)
    PARTITION_CLAUSE = "truncate(6, `szdate`)"
    PARTITION_CLAUSE = "date(`bizdate`)"
    PARTITION_CLAUSE = "days(`bizdate`)"
    PARTITION_CLAUSE = "months(`bizdate`)"
    PARTITION_CLAUSE = "years(`bizdate`)"
    PARTITION_CLAUSE = "szdate"
    
    CONVERT_COLUMNS = [
        ("bizdate", "convert(szdate)")
    ]
    
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
    
    
    FULL_PER_DAY = "N"
    INCREMENTAL_PER_DAY = "N"
    
    PULL_PERIOD_CONFIG = {          # Change ENABLE_PERIOD to N or remove this block to disable Pull Period
        "ENABLE_PERIOD": "Y",
        "START_YEAR": 2024,
        "START_MONTH": 1,
        "END_YEAR": 2025,
        "END_MONTH": 1,
        "PULL_DAY": "N"
    }
    
    
    
"""


# =============================== Global Declarations ===============================
NESSIE_BRANCH = "main"
LAKEHOUSE_CATALOG = "lakehouse_raw"
LAKEHOUSE_NAMESPACE = "txdb"
LAKEHOUSE_PREFIX = "r_"


DEFAULT_START_YEAR = None
DEFAULT_START_MONTH = None

DEFAULT_INCREMENTAL_MONTH = 1
DEFAULT_FULL_PER_DAY = "N"

DEFAUL_INCREMENTAL_PER_DAY =  "Y"
DEFAULT_INCREMENTAL_DAY = 7

DEFAULT_PARTITION_CLAUSE = ""
DEFAULT_QUERY_KEY = ""
DEFAULT_REMOVE_COLUMNS = []
DEFAULT_RENAME_COLUMNS = []
DEFAULT_ADD_COLUMNS = []
DEFAULT_CONVERT_COLUMNS = []

# ============================== TXDB-POS ===========================================
class TXFOOTER:
    QUERY_KEY = "SZDATE"
    PARTITION_CLAUSE = "szbusinessdate"
    START_YEAR = 2024
    START_MONTH = 1
    ADD_COLUMNS = [
        ("lsystemtype", "pos"),
        ("dcre", "current_timestamp")
    ]
    
class TXCMJOURNAL:
    
    ADD_COLUMNS = [
        ("lsystemtype", "pos"),
        ("dcre", "current_timestamp")
    ]
    
class TXCOUPON:
    QUERY_KEY = "SZDATE"
    START_YEAR = 2024
    START_MONTH = 1
    
    ADD_COLUMNS = [
        ("lsystemtype", "pos"),
        ("dcre", "current_timestamp")
    ]
    
class TXCUSTOMER:
    QUERY_KEY = "SZDATE"
    PARTITION_CLAUSE = "szbusinessdate"
    START_YEAR = 2024
    START_MONTH = 1
    ADD_COLUMNS = [
        ("lsystemtype", "pos"),
        ("dcre", "current_timestamp")
    ]
    
class TXDISCINFO:
    QUERY_KEY = "SZDATE"
    PARTITION_CLAUSE = "szbusinessdate"
    START_YEAR = 2024
    START_MONTH = 1
    ADD_COLUMNS = [
        ("lsystemtype", "pos"),
        ("dcre", "current_timestamp")
    ]

class TXMEDIALINE:
    QUERY_KEY = "SZDATE"
    PARTITION_CLAUSE = "szbusinessdate"
    START_YEAR = 2024
    START_MONTH = 1
    ADD_COLUMNS = [
        ("lsystemtype", "pos"),
        ("dcre", "current_timestamp")
    ]
    
class TXOLTPPROTOCOLJOURNAL:
    QUERY_KEY = "SZDATE"
    PARTITION_CLAUSE = "szbusinessdate"
    START_YEAR = 2024
    START_MONTH = 1
    ADD_COLUMNS = [
        ("lsystemtype", "pos"),
        ("dcre", "current_timestamp")
    ]
    
class TXSALELINEITEM:
    QUERY_KEY = "SZDATE"
    PARTITION_CLAUSE = "szbusinessdate"
    START_YEAR = 2024
    START_MONTH = 1
    ADD_COLUMNS = [
        ("lsystemtype", "pos"),
        ("dcre", "current_timestamp")
    ]
    
class TXSERIALIZEDMEDIA:
    QUERY_KEY = "SZDATE"
    PARTITION_CLAUSE = "szbusinessdate"
    START_YEAR = 2024
    START_MONTH = 1
    ADD_COLUMNS = [
        ("lsystemtype", "pos"),
        ("dcre", "current_timestamp")
    ]
    
class TXTOTAL:
    QUERY_KEY = "SZDATE"
    PARTITION_CLAUSE = "szbusinessdate"
    START_YEAR = 2024
    START_MONTH = 1
    
    ADD_COLUMNS = [
        ("lsystemtype", "pos"),
        ("dcre", "current_timestamp")
    ]
class TXVOIDRECEIPT:
    ADD_COLUMNS = [
        ("lsystemtype", "pos"),
        ("dcre", "current_timestamp")
    ]
    
class TXWORKSTATIONFREQUENCE:
    QUERY_KEY = "SZDATE"
    PARTITION_CLAUSE = "szbusinessdate"
    START_YEAR = 2024
    START_MONTH = 1
    ADD_COLUMNS = [
        ("lsystemtype", "pos"),
        ("dcre", "current_timestamp")
    ]
    
# ===================================================================================

