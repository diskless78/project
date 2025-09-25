""".   WHEN REMOVE ANY CONFIGURATION IN THE CLASS WHICH IS DECLARED FOR THE TABLE. THAT MEANS, IT WILL BE USED THE DEFAULT SETTING.
    
    1. MSSQL_TABLE need to be accurate with Oracle DB (UPPERCASE, lowercase or TitleCase).
    2. MSSQL_TABLE need to be matched in the const.py of the declaration (UPPERCASE, lowercase or TitleCase).
    3. ORDER BY Should be configured despite small/big table, otherwise leave it blank.
    4. Table in lakehouse will be used lowercase as our naming convention.
    5. If START_YEAR = 0 then pulling data base on min and max of the QUERY_KEY, this case need to partition with truncate
    6. If DEFAULT_START_YEAR = None then pulling all the data without QUERY_KEY
    7. If DEFAULT_START_MONTH = None which mean pull year by year - if it's number included zero which mean pull month by month per year starting with START_MONTH (START_MONTH = 0/1 are the same).

    PARTITION_CLAUSE = "truncate(`recid`, 1000000)" ---- Example: months(`bizdate`), bucket(16, `lretailstoreid`)
    PARTITION_CLAUSE = "truncate(6, `szdate`)"
    PARTITION_CLAUSE = "date(`bizdate`)"
    PARTITION_CLAUSE = "days(`bizdate`)"
    PARTITION_CLAUSE = "months(`bizdate`)"
    PARTITION_CLAUSE = "year(`bizdate`)"
    PARTITION_CLAUSE = "szdate"
    SORT_BY = ["inventtransid", "itemid"]
    
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
"""


# =============================== Global Declarations ===============================
LAKEHOUSE_CATALOG = "lakehouse_raw"
LAKEHOUSE_NAMESPACE = "dwh"
LAKEHOUSE_PREFIX = "r_"
DEFAULT_START_YEAR = None
DEFAULT_START_MONTH = None
DEFAULT_INCREMENTAL_MONTH = 1
DEFAULT_PARTITION_CLAUSE = ""
DEFAULT_QUERY_KEY = ""
DEFAULT_SORT_BY = ""
DEFAULT_REMOVE_COLUMNS = []
DEFAULT_RENAME_COLUMNS = []
DEFAULT_ADD_COLUMNS = []
DEFAULT_REMOVE_COLUMNS = []
# ============================== TXDB-POS ===========================================
class ADM_LCM_BUDGDAY:
    SORT_BY = ["adbsite"]

class ADM_NETWORK:
    SORT_BY = ["andcode"]

class ADM_NOTCOMP:
    SORT_BY = ["adcsite"]
    
class ARTICLE:
    SORT_BY = ["artcexr"]
    
class BI_INF_BDV_BUDGETVERSION:
    SORT_BY = ["store_cd"]

class MV_MERSTRUC:
    SORT_BY = ["mercode"]

class SITEMAPPOS:
    SORT_BY = ["smpvsite"]
    
class ADM_BUDGDAY:
    QUERY_KEY = "ADBUDPDATE"
    PARTITION_CLAUSE = "months(`adbudpdate`)"
    START_YEAR = 2023


""" The below one is being run at 4h30 """


class MERCSTR_ALL:
    SORT_BY = ["mercode"]
    
class MERCSTR:
    SORT_BY = ["mercode"]

class STOCKDAY_ARTICLE:
    SORT_BY = ["mindmvt"]

class RECEYEAR:
    SORT_BY = ["sysdcre"]

class STOCKDAT:
    SORT_BY = ["stodcre"]
   
class TICKRATLCM:
    SORT_BY = ["ratdate"]

class DATAVLPLCM:
    SORT_BY = ["create_date"]
           
class TICKRAT:
    QUERY_KEY = "RATDATE"
    PARTITION_CLAUSE = "months(`ratdate`)"
    START_YEAR = 2023

class DATAVLP:
    QUERY_KEY = "VLPDATE"
    PARTITION_CLAUSE = "months(`vlpdate`)"
    START_YEAR = 2023

class SHIPMVT:
    QUERY_KEY = "SYSDCRE"
    PARTITION_CLAUSE = "months(`sysdcre`)"
    START_YEAR = 2023
    
class RECEMVT:
    QUERY_KEY = "SYSDCRE"
    PARTITION_CLAUSE = "months(`sysdcre`)"
    START_YEAR = 2023

class SALEMVT:
    QUERY_KEY = "SYSDCRE"
    PARTITION_CLAUSE = "months(`sysdcre`)"
    START_YEAR = 2023
      
# ===================================================================================

