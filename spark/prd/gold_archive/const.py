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
NESSIE_BRANCH = "archive"
LAKEHOUSE_CATALOG = "lakehouse_archive"
LAKEHOUSE_NAMESPACE = "gold_archive"
LAKEHOUSE_PREFIX = ""

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

# ============================== Oracle GOLD ARCHIVE =====================================
class TARCRIT:
    QUERY_KEY = ""

class AVESCOPE:
    QUERY_KEY = ""

class AVETAR:
    QUERY_KEY = ""

class TRA_AVETAR:
    QUERY_KEY = ""

class CDEENTRE:
    QUERY_KEY = ""

class STODETTRA:
    QUERY_KEY = ""

class STOENTTRA:
    QUERY_KEY = ""

class STOLITIGE:
    QUERY_KEY = ""

class CNSMVT:
    QUERY_KEY = ""

class INVCORREC:
    QUERY_KEY = ""

class INVCRITINV:
    QUERY_KEY = ""

class INVENTINV:
    QUERY_KEY = ""

class INVETAPE:
    QUERY_KEY = ""

class INVSUIVI:
    QUERY_KEY = ""

class STOMVSEMAINE:   # 1,5 billion rows
    QUERY_KEY = "SMSDMAJ"
    PARTITION_CLAUSE = "date(`smsdmaj`)"
    START_YEAR = 2000
    START_MONTH = 1
    INCREMENTAL_PER_DAY = "Y"
    FULL_PER_DAY = "Y"
    
class ARTUC:
    QUERY_KEY = "ARADMAJ"
    PARTITION_CLAUSE = "date(`aradmaj`)"
    START_YEAR = 2000
    START_MONTH = 1
    INCREMENTAL_PER_DAY = "Y"

class TARPRIX:
    QUERY_KEY = "TAPDMAJ"
    PARTITION_CLAUSE = "date(`tapdmaj`)"
    START_YEAR = 2000
    START_MONTH = 1
    INCREMENTAL_PER_DAY = "Y"

class TAREMISE:
    QUERY_KEY = "TREDMAJ"
    PARTITION_CLAUSE = "date(`tredmaj`)"
    START_YEAR = 2000
    START_MONTH = 1
    INCREMENTAL_PER_DAY = "Y"

class TAREXAR:
    QUERY_KEY = "TRXDMAJ"
    PARTITION_CLAUSE = "date(`trxdmaj`)"
    START_YEAR = 2000
    START_MONTH = 1
    INCREMENTAL_PER_DAY = "Y"
    
class TARGRAR:
    QUERY_KEY = "TGADMAJ"
    PARTITION_CLAUSE = "date(`tgadmaj`)"
    START_YEAR = 2000
    START_MONTH = 1
    INCREMENTAL_PER_DAY = "Y"

class TARGRAT:
    QUERY_KEY = "TGRDMAJ"
    PARTITION_CLAUSE = "date(`tgrdmaj`)"
    START_YEAR = 2000
    START_MONTH = 1
    INCREMENTAL_PER_DAY = "Y"
    
class TARINCO:
    QUERY_KEY = "TICDMAJ"
    PARTITION_CLAUSE = "date(`ticdmaj`)"
    START_YEAR = 2000
    START_MONTH = 1
    INCREMENTAL_PER_DAY = "Y"
    
class TARGRUL:
    QUERY_KEY = "TGUDMAJ"
    PARTITION_CLAUSE = "date(`tgudmaj`)"
    START_YEAR = 2000
    START_MONTH = 1
    INCREMENTAL_PER_DAY = "Y"
    FULL_PER_DAY = "Y"
    
class AVEPRIX:
    QUERY_KEY = "AVIDMAJ"
    PARTITION_CLAUSE = "date(`avidmaj`)"
    START_YEAR = 2000
    START_MONTH = 1
    INCREMENTAL_PER_DAY = "Y"
    FULL_PER_DAY = "Y"

class RAOVENTTHEO:
    QUERY_KEY = "RVTDMAJ"
    PARTITION_CLAUSE = "date(`rvtdmaj`)"
    START_YEAR = 2000
    START_MONTH = 1
    INCREMENTAL_PER_DAY = "Y"
    FULL_PER_DAY = "Y"
    
class RAOPARAM:
    QUERY_KEY = "RPADMAJ"
    PARTITION_CLAUSE = "date(`rpadmaj`)"
    START_YEAR = 2000
    START_MONTH = 1
    INCREMENTAL_PER_DAY = "Y"
    FULL_PER_DAY = "Y"

class ARTPERIODE:
    QUERY_KEY = "ARPDMAJ"
    PARTITION_CLAUSE = "date(`arpdmaj`)"
    START_YEAR = 2000
    START_MONTH = 1
    INCREMENTAL_PER_DAY = "Y"
    FULL_PER_DAY = "Y"
 
class STODETRE:
    QUERY_KEY = "SDRDMAJ"
    PARTITION_CLAUSE = "date(`sdrdmaj`)"
    START_YEAR = 2000
    START_MONTH = 1
    INCREMENTAL_PER_DAY = "Y"
    FULL_PER_DAY = "Y"
      
class STOENTRE:
    QUERY_KEY = "SERDMAJ"
    PARTITION_CLAUSE = "date(`serdmaj`)"
    START_YEAR = 2000
    START_MONTH = 1
    INCREMENTAL_PER_DAY = "Y"
    FULL_PER_DAY = "Y"

class STOPIERE:
    QUERY_KEY = "SPRDMAJ"
    PARTITION_CLAUSE = "date(`sprdmaj`)"
    START_YEAR = 2000
    START_MONTH = 1
    INCREMENTAL_PER_DAY = "Y"
    FULL_PER_DAY = "Y"    

class STOREMRE:
    QUERY_KEY = "SRRDMAJ"
    PARTITION_CLAUSE = "date(`srrdmaj`)"
    START_YEAR = 2000
    START_MONTH = 1
    INCREMENTAL_PER_DAY = "Y"
    FULL_PER_DAY = "Y"            

class CDEDETRE:
    QUERY_KEY = "CDRDMAJ"
    PARTITION_CLAUSE = "date(`cdrdmaj`)"
    START_YEAR = 2000
    START_MONTH = 1
    INCREMENTAL_PER_DAY = "Y"
    FULL_PER_DAY = "Y"     

class STOMAVOY:
    QUERY_KEY = "SMVDMAJ"
    PARTITION_CLAUSE = "date(`smvdmaj`)"
    START_YEAR = 2000
    START_MONTH = 1
    INCREMENTAL_PER_DAY = "Y"

class STOMVT:
    QUERY_KEY = "STMDMAJ"
    PARTITION_CLAUSE = "date(`stmdmaj`)"
    START_YEAR = 2000
    START_MONTH = 1
    INCREMENTAL_PER_DAY = "Y"
    FULL_PER_DAY = "Y"
    
class STODEPR:
    QUERY_KEY = "SDRDATE"
    PARTITION_CLAUSE = "date(`sdrdate`)"
    START_YEAR = 2000
    START_MONTH = 1
    INCREMENTAL_PER_DAY = "Y"
    FULL_PER_DAY = "Y"

class STODEPV:
    QUERY_KEY = "SDVDDEM"
    PARTITION_CLAUSE = "date(`sdvddem`)"
    START_YEAR = 2000
    START_MONTH = 1
    INCREMENTAL_PER_DAY = "Y"
    FULL_PER_DAY = "Y" 

class STODEPA:
    QUERY_KEY = "SDADATE"
    PARTITION_CLAUSE = "date(`sdadate`)"
    START_YEAR = 2000
    START_MONTH = 1
    INCREMENTAL_PER_DAY = "Y"
    FULL_PER_DAY = "Y" 

class CNSMVTERR:
    QUERY_KEY = "CMEDMAJ"
    PARTITION_CLAUSE = "date(`cmedmaj`)"
    START_YEAR = 2000
    START_MONTH = 1
    INCREMENTAL_PER_DAY = "Y"
    FULL_PER_DAY = "Y"

class INVDETINV:
    QUERY_KEY = "DINDMAJ"
    PARTITION_CLAUSE = "date(`dindmaj`)"
    START_YEAR = 2000
    START_MONTH = 1
    INCREMENTAL_PER_DAY = "Y"
    FULL_PER_DAY = "Y" 

class INVSAISIE:
    QUERY_KEY = "ISADMAJ"
    PARTITION_CLAUSE = "date(`isadmaj`)"
    START_YEAR = 2000
    START_MONTH = 1
    INCREMENTAL_PER_DAY = "Y"
    FULL_PER_DAY = "Y" 

class STOMVPERIODE:
    QUERY_KEY = "SMPDMAJ"
    PARTITION_CLAUSE = "date(`smpdmaj`)"
    START_YEAR = 2000
    START_MONTH = 1
    INCREMENTAL_PER_DAY = "Y"
    FULL_PER_DAY = "Y"

class STOMVMOIS:
    QUERY_KEY = "SMMDMAJ"
    PARTITION_CLAUSE = "date(`smmdmaj`)"
    START_YEAR = 2000
    START_MONTH = 1
    INCREMENTAL_PER_DAY = "Y"
    FULL_PER_DAY = "Y"

class VTEREMART:
    QUERY_KEY = "VTADMAJ"
    PARTITION_CLAUSE = "date(`vtadmaj`)"
    START_YEAR = 2000
    START_MONTH = 1
    INCREMENTAL_PER_DAY = "Y"
    FULL_PER_DAY = "Y" 
# ===================================================================================

