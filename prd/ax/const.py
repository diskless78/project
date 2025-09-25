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
LAKEHOUSE_NAMESPACE = "ax"
LAKEHOUSE_PREFIX = "r_"
DEFAULT_START_YEAR = None
DEFAULT_START_MONTH = None
DEFAULT_INCREMENTAL_MONTH = 1
DEFAULT_PARTITION_CLAUSE = ""
DEFAULT_QUERY_KEY = ""
DEFAULT_SORT_BY = ""
DEFAULT_RENAME_COLUMNS = []
DEFAULT_ADD_COLUMNS = []
DEFAULT_REMOVE_COLUMNS = []

# ============================== AXDB-CBS ===========================================
class INVENTTRANS:
    LAKEHOUSE_TABLENAME = "ax_invent_trans"
    QUERY_KEY = "MODIFIEDDATETIME"
    PARTITION_CLAUSE = "months(`modifieddatetime`)"
    SORT_BY = ["costamountposted", "itemid"]
    START_YEAR = 2020
    RENAME_COLUMNS = [
        ("partition", "partition_id")
    ]

class INVENTTRANSORIGIN:
    LAKEHOUSE_TABLENAME = "ax_invent_trans_origin"
    QUERY_KEY = "RECID"
    PARTITION_CLAUSE = "truncate(`recid`, 10000000)"
    SORT_BY = ["inventtransid", "itemid"]
    START_YEAR = 0
    RENAME_COLUMNS = [
        ("partition", "partition_id")
    ]

class RETAILCHANNELTABLE:
    LAKEHOUSE_TABLENAME = "ax_sales_retail_channel"
    SORT_BY = ["recid"]
    RENAME_COLUMNS = [
        ("password", "password_char"),
        ("partition", "partition_id")
    ]
    
class CUSTTABLE:
    LAKEHOUSE_TABLENAME = "ax_custtable"
    SORT_BY = ["accountnum", "recid"]
    RENAME_COLUMNS = [
        ("partition", "partition_id")
    ]
    
class SALESLINE:
    LAKEHOUSE_TABLENAME = "ax_sales_retail_salesline"
    QUERY_KEY = "MODIFIEDDATETIME"
    PARTITION_CLAUSE = "months(`modifieddatetime`)"
    SORT_BY = ["salesid", "itemid"]
    START_YEAR = 2020
    RENAME_COLUMNS = [
        ("name", "display_name"),
        ("partition", "partition_id")
    ]
    
class RETAILTRANSACTIONSALESTRANS:
    LAKEHOUSE_TABLENAME = "ax_retail_transaction_salestrans"
    QUERY_KEY = "MODIFIEDDATETIME"
    PARTITION_CLAUSE = "months(`modifieddatetime`)"
    SORT_BY = ["linenum", "itemid"]
    START_YEAR = 2020
    START_MONTH = 1
    RENAME_COLUMNS = [
        ("counter", "counter_id"),
        ("replicated", "replicated_id"),
        ("catalog", "catalog_id"),
        ("partition", "partition_id")
    ]

class RETAILTRANSACTIONTABLE:
    LAKEHOUSE_TABLENAME = "ax_sales_retail_transaction"
    QUERY_KEY = "MODIFIEDDATETIME"
    PARTITION_CLAUSE = "months(`modifieddatetime`)"
    SORT_BY = ["invoiceid", "custaccount"]
    SORT_BY = ["invoiceid", "custaccount"]
    START_YEAR = 2020
    RENAME_COLUMNS = [
        ("type", "type_id"),
        ("counter", "counter_id"),
        ("replicated", "replicated_id"),
        ("description", "description_name"),
        ("partition", "partition_id")
    ]
    
class DIMENSIONATTRIBUTEVALUESETITEM:
    LAKEHOUSE_TABLENAME = "ax_dimension_attribute_value_set_item"
    SORT_BY = ["recid"]
    RENAME_COLUMNS = [
        ("partition", "partition_id")
    ]

class DIMENSIONATTRIBUTEVALUE:
    LAKEHOUSE_TABLENAME = "ax_dimension_attribute_value"
    SORT_BY = ["recid"]
    RENAME_COLUMNS = [
        ("owner", "owner_id"),
        ("partition", "partition_id")
    ]
    
# ==================================================================================

