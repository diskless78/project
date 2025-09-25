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
LAKEHOUSE_NAMESPACE = "gold"
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
class V_COCA_SITE:
    QUERY_KEY = "ARCCINR"
    PARTITION_CLAUSE = "truncate(`arccinr`, 1000000)"
    SORT_BY = ["arccode"]
    START_YEAR = 0

class CLIDGENE:
    SORT_BY = ["clincli"]

class STRUCOBJ:
    SORT_BY = ["sobcext"]

class ARTVL:
    SORT_BY = ["arlcexr"]
         
class V_COCA_ACTIVE:
    SORT_BY = ["arccode"]

class SITDGENE:
    SORT_BY = ["soczgeo"]

class SITATTRI:
    SORT_BY = ["satvaln"]
      
class V_BRAND_SITE:
    SORT_BY = ["shortsitename"]

class TRA_PARPOSTES:
    SORT_BY = ["tpardmaj"]

class STRUCREL:
    SORT_BY = ["objcint"]
    
class RESEAU:
    SORT_BY = ["resdmaj"]

class FOUDGENE:
    SORT_BY = ["foudmaj"]
    
class ARTUVATTRI:
    SORT_BY = ["uatdmaj"]
    
class ARTUVUL:
    SORT_BY = ["avldmaj"]
    
class ARTCAISSE:
    QUERY_KEY = "ACADMAJ"
    PARTITION_CLAUSE = "months(`acadmaj`)"
    SORT_BY = ["acadmaj"]
    START_YEAR = 2011

class BLIENTBL:
    QUERY_KEY = "BLEDMAJ"
    PARTITION_CLAUSE = "months(`bledmaj`)"
    SORT_BY = ["bledmaj"]
    START_YEAR = 2023

class CDEDETCDE:
    QUERY_KEY = "DCDDMAJ"
    PARTITION_CLAUSE = "months(`dcddmaj`)"
    SORT_BY = ["dcddmaj"]
    START_YEAR = 2023

class CDEDISTRIB:
    QUERY_KEY = "LCDDMAJ"
    PARTITION_CLAUSE = "months(`lcddmaj`)"
    SORT_BY = ["lcddmaj"]
    START_YEAR = 2023

class TRA_STRUCOBJ:
    QUERY_KEY = "TSOBDMAJ"
    PARTITION_CLAUSE = "months(`tsobdmaj`)"
    SORT_BY = ["tsobcint"]
    START_YEAR = 2007

class STOMVT:
    QUERY_KEY = "STMDMAJ"
    PARTITION_CLAUSE = "months(`stmdmaj`)"
    SORT_BY = ["stmseq"]
    START_YEAR = 2023
    START_MONTH = 1

class STODETRE:
    QUERY_KEY = "SDRDMAJ"
    PARTITION_CLAUSE = "months(`sdrdmaj`)"
    START_YEAR = 2023

class STOCOUCH:
    QUERY_KEY = "STODMAJ"
    PARTITION_CLAUSE = "months(`stodmaj`)"
    SORT_BY = ["stoseq"]
    START_YEAR = 2023
           
class CDEENTCDE:
    QUERY_KEY = "ECDDMAJ"
    PARTITION_CLAUSE = "months(`ecddmaj`)"
    SORT_BY = ["ecddmaj"]
    START_YEAR = 2023
          
class AVEPRIX:
    QUERY_KEY = "AVIDMAJ"
    PARTITION_CLAUSE = "months(`avidmaj`)"
    SORT_BY = ["avidmaj"]
    START_YEAR = 2008
    
class ARTCONS:
    SORT_BY = ["acsdmaj"]
    
class ARTUL:
    SORT_BY = ["arutypul"]

class ARTUV:
    SORT_BY = ["arvcexr"]
     
class ARTRAC:
    SORT_BY = ["artdmaj"]
 
class AVESCOPE:
    SORT_BY = ["avodmaj"]
    
class ARTUC:
    QUERY_KEY = "ARADMAJ"
    PARTITION_CLAUSE = "months(`aradmaj`)"
    SORT_BY = ["aradmaj"]
    START_YEAR = 2011
# ===================================================================================

