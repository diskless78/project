
-- Enable CDC on the database
USE Sales_Trans_Prod;
GO
EXEC sys.sp_cdc_enable_db;
GO

-- Enable CDC on the TxCustomer table in SQL Server
EXEC sys.sp_cdc_enable_table
    @source_schema = N'dbo',
    @source_name = N'TxCustomer',
    @role_name = NULL;
GO

-- Verify CDC is enabled on the database and table
SELECT name, is_cdc_enabled 
FROM sys.databases
WHERE name = 'Sales_Trans_Prod';

SELECT * 
FROM cdc.change_tables
WHERE source_object_id = OBJECT_ID('dbo.TxCustomer');

-- Disable CDC on the TxCustomer table
USE Sales_Trans_Prod;
GO

EXEC sys.sp_cdc_disable_table  
    @source_schema = N'dbo',  
    @source_name = N'TxCustomer',  
    @capture_instance = N'dbo_TxCustomer';  -- must match existing instance name
GO


-- ===============================================

SELECT 
    c.name AS ColumnName,
    t.name AS DataType,
    c.max_length AS MaxLengthBytes,
    CASE 
        WHEN t.name IN ('nchar', 'nvarchar') 
            THEN c.max_length / 2 
        ELSE c.max_length
    END AS MaxLengthChars,
    c.precision,
    c.scale,
    c.is_nullable,
    c.is_identity
FROM sys.columns c
JOIN sys.types t 
    ON c.user_type_id = t.user_type_id
WHERE c.object_id = OBJECT_ID('RETAILTRANSACTIONSALESTRANS')
ORDER BY c.column_id;

-----
SELECT
    KU.TABLE_NAME,
    KU.COLUMN_NAME,
    KU.ORDINAL_POSITION,
    TC.CONSTRAINT_NAME
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS TC
JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS KU
    ON TC.CONSTRAINT_NAME = KU.CONSTRAINT_NAME
    AND TC.TABLE_SCHEMA = KU.TABLE_SCHEMA
WHERE TC.CONSTRAINT_TYPE = 'PRIMARY KEY'
  AND KU.TABLE_NAME = 'RETAILTRANSACTIONSALESTRANS';



SELECT *
FROM TxOLTPProtocolJournal
ORDER BY dcre DESC;


SELECT 
    MIN(ACSDCRE) AS Min,
    MAX(ACSDCRE) AS Max
FROM ARTCONS;


SELECT COUNT(*)
FROM lakehouse_raw.golddb.r_ARTCAISSE

WHERE EXTRACT(YEAR FROM ACADCRE) = 2025
  AND EXTRACT(MONTH FROM ACADCRE) = 9
  AND EXTRACT(DAY FROM ACADCRE) = 6;


WHERE CAST(CREATEDDATETIME AS TIMESTAMP_LTZ(3)) 
      >= TIMESTAMPADD(DAY, -7, CAST(CURRENT_DATE AS TIMESTAMP_LTZ(3)))
  AND CAST(CREATEDDATETIME AS TIMESTAMP_LTZ(3)) <= CURRENT_TIMESTAMP;
  
WHERE ACADCRE >= TIMESTAMPADD(DAY, -7, CURRENT_DATE) + INTERVAL '7' HOUR
  AND ACADCRE <= CURRENT_TIMESTAMP + INTERVAL '7' HOUR

WHERE CAST(AVIDCRE AS TIMESTAMP_LTZ(3)) 
       BETWEEN CAST(TIMESTAMP '2024-01-01 00:00:00' AS TIMESTAMP_LTZ(3))
           AND CAST(TIMESTAMP '2025-01-01 00:00:00' AS TIMESTAMP_LTZ(3))

WHERE EXTRACT(YEAR FROM BLEDMAJ) >= 2023 AND EXTRACT(YEAR FROM BLEDMAJ) <= 2025;


WHERE CAST(BLEDMAJ AS TIMESTAMP_LTZ(3))
      >= CAST(TIMESTAMPADD(DAY, -7, CAST(CURRENT_TIMESTAMP + INTERVAL '7' HOUR AS DATE)) AS TIMESTAMP_LTZ(3))
  AND CAST(BLEDMAJ AS TIMESTAMP_LTZ(3)) <= CURRENT_TIMESTAMP + INTERVAL '7' HOUR;


MIN_RECID=$(sqlcmd -S mssql-axdb-sec.centralretail.com.vn -d DAX63_PROD -U DBReadOnly -P 'qL4TPPB3#j!3n7rd' -h -1 -Q "SET NOCOUNT ON; SELECT MIN(RECID) FROM InventTransOrigin;")
MAX_RECID=$(sqlcmd -S mssql-axdb-sec.centralretail.com.vn -d DAX63_PROD -U DBReadOnly -P 'qL4TPPB3#j!3n7rd' -h -1 -Q "SET NOCOUNT ON; SELECT MAX(RECID) FROM InventTransOrigin;")

count_src_table=$(sqlcmd -S mssql-axdb-sec.centralretail.com.vn \
    -d DAX63_PROD \
    -U DBReadOnly \
    -P 'qL4TPPB3#j!3n7rd' \
    -h -1 -Q "SET NOCOUNT ON; SELECT COUNT(*) FROM InventTransOrigin;")

echo $count_src_table

count_src_table=$(sqlcmd -S "$mssql_host" \
    -d "$mssql_db" \
    -U "$mssql_username" \
    -P "$mssql_password" \
    -h -1 -W \
    -Q "SET NOCOUNT ON; SELECT COUNT(*) FROM InventTransOrigin;" | xargs)



select count(*) FROM RETAILTRANSACTIONTABLE Where recid between 5644155863 and 5644185252 ORDER by recid asc;
Select count(*) FROM RETAILTRANSACTIONTABLE WHERE CREATEDDATETIME >= DATEADD(DAY, -7, CAST(GETDATE() AS DATE))
  AND CREATEDDATETIME < DATEADD(DAY, 1, CAST(GETDATE() AS DATE)) order by recid asc;

SELECT MIN(recid) FROM RETAILTRANSACTIONTABLE
WHERE CREATEDDATETIME >= DATEADD(DAY, -7, '2025-01-01 05:15:07.000')
  AND CREATEDDATETIME < CREATEDDATETIME < DATEADD(DAY, 1, CAST(GETDATE() AS DATE));
  
SELECT MIN(recid) FROM RETAILTRANSACTIONTABLE
WHERE CREATEDDATETIME >= DATEADD(DAY, -7, '2025-01-01 05:15:07.000')
  AND CREATEDDATETIME < DATEADD(DAY, 1, '2025-01-01 05:15:07.000');

SELECT MIN(recid) FROM RETAILTRANSACTIONTABLE
WHERE CREATEDDATETIME >= DATEADD(DAY, -7, CAST(GETDATE() AS DATE))
  AND CREATEDDATETIME < DATEADD(DAY, 1, CAST(GETDATE() AS DATE));

SELECT MAX(recid) FROM RETAILTRANSACTIONTABLE

SELECT recid, CREATEDDATETIME
FROM RETAILTRANSACTIONTABLE
WHERE recid BETWEEN 5644155863 AND 5644185252
  AND NOT (
      CREATEDDATETIME >= DATEADD(DAY, -7, CAST(GETDATE() AS DATE))
      AND CREATEDDATETIME < DATEADD(DAY, 1, CAST(GETDATE() AS DATE))
  )
ORDER BY recid;

SELECT recid, COUNT(*) AS dup_count
FROM lakehouse_raw.ax.r_ax_sales_retail_transaction
GROUP BY recid
HAVING COUNT(*) > 1
ORDER BY recid;


SELECT recid, CREATEDDATETIME
FROM RETAILTRANSACTIONTABLE
WHERE CREATEDDATETIME >= '2020-01-01 00:00:00'
  AND CREATEDDATETIME < '2021-01-01 00:00:00'
  AND (recid < 5637147060 OR recid > 5638054051)
ORDER BY CREATEDDATETIME;

SELECT recid, CREATEDDATETIME
FROM RETAILTRANSACTIONTABLE
WHERE CREATEDDATETIME >= '2020-01-01 00:00:00'
  AND CREATEDDATETIME <= '2020-12-31 23:59:59.999'
  AND (recid < 5637147060 OR recid > 5638054463)
ORDER BY CREATEDDATETIME DESC;

SELECT *
FROM RETAILTRANSACTIONTABLE
WHERE CREATEDDATETIME >= '2020-01-01 00:00:00'
  AND CREATEDDATETIME <= '2020-12-31 23:59:59.999'
  AND recid IS NULL;


SELECT MIN(recid) AS min_recid, MAX(recid) AS max_recid
FROM RETAILTRANSACTIONTABLE
WHERE CREATEDDATETIME >= '2020-01-01 00:00:00'
  AND CREATEDDATETIME <= '2020-12-31 23:59:59.999';
-- min recid=5637147060 & max recid=5638054463

SELECT MIN(recid) AS min_recid, MAX(recid) AS max_recid
FROM RETAILTRANSACTIONTABLE
WHERE CREATEDDATETIME >= '2019-12-31 00:00:00'
  AND CREATEDDATETIME <= '2020-12-31 23:59:59.999';
-- min recid=5637147060 & max recid=5638054463

SELECT COUNT(*)
FROM RETAILTRANSACTIONTABLE
WHERE createddatetime >= DATEADD(DAY, -7, GETDATE())
  AND createddatetime <= GETDATE();

SELECT COUNT(*) FROM TxDiscInfo WHERE CAST(LEFT(szDate, 4) AS INT) BETWEEN 2024 AND YEAR(GETDATE());

---- Index

SELECT 
    i.name AS index_name,
    i.type_desc AS index_type,
    c.name AS column_name,
    ic.key_ordinal AS column_order,
    ic.is_included_column
FROM sys.indexes i
INNER JOIN sys.index_columns ic 
    ON i.object_id = ic.object_id 
   AND i.index_id = ic.index_id
INNER JOIN sys.columns c 
    ON ic.object_id = c.object_id 
   AND ic.column_id = c.column_id
WHERE i.object_id = OBJECT_ID('TxFooter')
ORDER BY i.name, ic.key_ordinal;


SELECT 
    lRetailStoreID,  
    szDate,
    COUNT(*) AS duplicate_count
FROM TxFooter
GROUP BY lRetailStoreID, szDate
HAVING COUNT(*) > 1;

SELECT 
    szTaType,  
    szDate,
    COUNT(*) AS duplicate_count
FROM TxFooter
GROUP BY szTaType, szDate
HAVING COUNT(*) > 1
ORDER BY duplicate_count;
