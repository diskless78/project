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
    
    Setting for each table for INCREMENTAL_PER_DAY and FULL_PER_DAY
    FULL_PER_DAY = "N"
    INCREMENTAL_PER_DAY = "N"
    
    PULL_PERIOD_CONFIG = {
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
LAKEHOUSE_NAMESPACE = "gold"
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
class V_COCA_SITE:
    QUERY_KEY = "ARCCINR"
    PARTITION_CLAUSE = "truncate(`arccinr`, 1000000)"
    START_YEAR = 0

class CLIDGENE:
    QUERY_KEY = ""

class STRUCOBJ:
    QUERY_KEY = ""

class ARTVL:
    QUERY_KEY = ""
         
class V_COCA_ACTIVE:
    QUERY_KEY = ""

class SITDGENE:
    QUERY_KEY = ""

class SITATTRI:
    QUERY_KEY = ""
      
class V_BRAND_SITE:
    QUERY_KEY = ""

class TRA_PARPOSTES:
    QUERY_KEY = ""

class STRUCREL:
    QUERY_KEY = ""
    
class RESEAU:
    QUERY_KEY = ""

class FOUDGENE:
    QUERY_KEY = ""
    
class ARTUVATTRI:
    QUERY_KEY = ""
    
class ARTUVUL:
    QUERY_KEY = ""
    
class ARTCAISSE:
    QUERY_KEY = "ACADMAJ"
    PARTITION_CLAUSE = "date(`acadmaj`)"
    START_YEAR = 2011
    START_MONTH = 1
    INCREMENTAL_PER_DAY = "Y"

class BLIENTBL:
    QUERY_KEY = "BLEDMAJ"
    PARTITION_CLAUSE = "date(`bledmaj`)"
    START_YEAR = 2023
    START_MONTH = 1
    INCREMENTAL_PER_DAY = "Y"

class CDEDETCDE:
    QUERY_KEY = "DCDDMAJ"
    PARTITION_CLAUSE = "date(`dcddmaj`)"
    START_YEAR = 2023
    START_MONTH = 1
    INCREMENTAL_PER_DAY = "Y"

class CDEDISTRIB:
    QUERY_KEY = "LCDDMAJ"
    PARTITION_CLAUSE = "date(`lcddmaj`)"
    START_YEAR = 2023
    START_MONTH = 1
    INCREMENTAL_PER_DAY = "Y"

class TRA_STRUCOBJ:
    QUERY_KEY = "TSOBDMAJ"
    PARTITION_CLAUSE = "date(`tsobdmaj`)"
    START_YEAR = 2007
    START_MONTH = 1
    INCREMENTAL_PER_DAY = "Y"

class STOMVT:
    QUERY_KEY = "STMDMAJ"
    PARTITION_CLAUSE = "date(`stmdmaj`)"
    START_YEAR = 2023
    START_MONTH = 1
    INCREMENTAL_PER_DAY = "Y"

class STODETRE:
    QUERY_KEY = "SDRDMAJ"
    PARTITION_CLAUSE = "date(`sdrdmaj`)"
    START_YEAR = 2023
    START_MONTH = 1
    INCREMENTAL_PER_DAY = "Y"

class STOCOUCH:
    QUERY_KEY = "STODMAJ"
    PARTITION_CLAUSE = "date(`stodmaj`)"
    START_YEAR = 2023
    START_MONTH = 1
    FULL_PER_DAY = "Y"
    INCREMENTAL_PER_DAY = "Y"
           
class CDEENTCDE:
    QUERY_KEY = "ECDDMAJ"
    PARTITION_CLAUSE = "date(`ecddmaj`)"
    START_YEAR = 2023
    START_MONTH = 1
    INCREMENTAL_PER_DAY = "Y"
          
class AVEPRIX:
    QUERY_KEY = "AVIDMAJ"
    PARTITION_CLAUSE = "date(`avidmaj`)"
    START_YEAR = 2008
    START_MONTH = 1
    FULL_PER_DAY = "Y"
    INCREMENTAL_PER_DAY = "Y"
    
class ARTCONS:
    QUERY_KEY = ""
    
class ARTUL:
    QUERY_KEY = ""

class ARTUV:
    QUERY_KEY = ""
     
class ARTRAC:
    QUERY_KEY = ""
 
class AVESCOPE:
    QUERY_KEY = ""
    
class ARTUC:
    QUERY_KEY = "ARADMAJ"
    PARTITION_CLAUSE = "date(`aradmaj`)"
    START_YEAR = 2011
    INCREMENTAL_PER_DAY = "Y"
# ===================================================================================

