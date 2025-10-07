# Login DB Info

    mssql-axdb-sec.centralretail.com.vn
    DB: DAX63_PROD
    DBReadOnly/qL4TPPB3#j!3n7rd

# Table Definition
    CUSTTABLE -> r_ax_custtable
    DIMENSIONATTRIBUTEVALUE -> r_ax_dimension_attribute_value"
    DIMENSIONATTRIBUTEVALUESETITEM -> r_ax_dimension_attribute_value_set_item
    RETAILTRANSACTIONSALESTRANS -> r_ax_retail_transaction_salestrans
    RETAILCHANNELTABLE -> r_ax_sales_retail_channel
    SALESLINE -> r_ax_sales_retail_salesline
    RETAILTRANSACTIONTABLE -> r_ax_sales_retail_transaction

# Fields was being changed

    PARTITION -> PARTITION_ID
    CATALOG -> CATALOG_ID
    REPLICATED -> REPLICATED_ID
    COUNTER -> COUNTER_ID
    PASSWORD -> PASSWORD_CHAR
    NAME -> DISPLAY_NAME
    TYPE -> TYPE_ID
    DESCRIPTION -> DESCRIPTION_NAME


# Lakehouse Namespace: lakehouse_raw.ax

SELECT COUNT(*) FROM lakehouse_raw.ax.r_ax_retail_transaction_salestrans;
SELECT COUNT(*) FROM lakehouse_raw.ax.r_ax_sales_retail_salesline;
SELECT COUNT(*) FROM lakehouse_raw.ax.r_ax_sales_retail_transaction;
SELECT COUNT(*) FROM lakehouse_raw.ax.r_ax_invent_trans;
SELECT COUNT(*) FROM lakehouse_raw.ax.r_ax_invent_trans_origin;

SELECT COUNT(*) FROM lakehouse_raw.ax.r_ax_custtable;
SELECT COUNT(*) FROM lakehouse_raw.ax.r_ax_dimension_attribute_value;
SELECT COUNT(*) FROM lakehouse_raw.ax.r_ax_dimension_attribute_value_set_item;
SELECT COUNT(*) FROM lakehouse_raw.ax.r_ax_sales_retail_channel;


# AXDB: mssql-axdb-sec.centralretail.com.vn

SELECT COUNT(*) FROM RETAILTRANSACTIONSALESTRANS;
SELECT COUNT(*) FROM SALESLINE WHERE SALESSTATUS =3;
SELECT COUNT(*) FROM RETAILTRANSACTIONTABLE;
SELECT COUNT(*) FROM INVENTTRANS;
SELECT COUNT(*) FROM INVENTTRANSORIGIN;

SELECT COUNT(*) FROM CUSTTABLE;
SELECT COUNT(*) FROM DIMENSIONATTRIBUTEVALUE;
SELECT COUNT(*) FROM DIMENSIONATTRIBUTEVALUESETITEM;
SELECT COUNT(*) FROM RETAILCHANNELTABLE;


Select min(CREATEDDATETIME) , max(CREATEDDATETIME) from InventTrans WHERE RECID between 5692000000 and 5693999999;

SELECT salesorderid, createddatetime
FROM lakehouse_raw.ax.r_ax_sales_retail_transaction
WHERE salesorderid = 'RSO000258256' AND
  EXTRACT (YEAR FROM createddatetime) = 2025 AND EXTRACT (MONTH FROM createddatetime) =9 AND EXTRACT (DAY FROM createddatetime) = 3;
  
SELECT *
FROM RETAILTRANSACTIONTABLE
WHERE YEAR(CREATEDDATETIME) = 2025
  AND MONTH(CREATEDDATETIME) = 9
  AND DAY(CREATEDDATETIME) = 3
  AND SALESORDERID = 'RSO000258256'

