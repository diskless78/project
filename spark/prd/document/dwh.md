# Lakehouse Namespace: lakehouse_raw.dwh

SELECT COUNT(*) FROM lakehouse_raw.dwh.r_mv_merstruc;
SELECT COUNT(*) FROM lakehouse_raw.dwh.r_mercstr;
SELECT COUNT(*) FROM lakehouse_raw.dwh.r_mercstr_all;
SELECT COUNT(*) FROM lakehouse_raw.dwh.r_adm_lcm_budgday;
SELECT COUNT(*) FROM lakehouse_raw.dwh.r_adm_network;
SELECT COUNT(*) FROM lakehouse_raw.dwh.r_adm_notcomp;
SELECT COUNT(*) FROM lakehouse_raw.dwh.r_article;
SELECT COUNT(*) FROM lakehouse_raw.dwh.r_bi_inf_bdv_budgetversion;
SELECT COUNT(*) FROM lakehouse_raw.dwh.r_sitemappos;
SELECT COUNT(*) FROM lakehouse_raw.dwh.r_stockday_article;

# DWHDB: ora-dwhdb-sec.centralretail.com.vn

SELECT COUNT(*) FROM DWH.MV_MERSTRUC;
SELECT COUNT(*) FROM DWH.MERCSTR;
SELECT COUNT(*) FROM DWH.MERCSTR_ALL;
SELECT COUNT(*) FROM ADM_LCM_BUDGDAY;
SELECT COUNT(*) FROM ADM.ADM_NETWORK;
SELECT COUNT(*) FROM ADM.ADM_NOTCOMP;
SELECT COUNT(*) FROM DWH.ARTICLE;
SELECT COUNT(*) FROM DWHBO.BI_INF_BDV_BUDGETVERSION;
SELECT COUNT(*) FROM CMS.SITEMAPPOS;
SELECT COUNT(*) FROM DWH.STOCKDAY_ARTICLE;
SELECT COUNT (*) FROM DATAVLP WHERE EXTRACT (YEAR FROM VLPDATE) >= 2023 AND EXTRACT(YEAR FROM VLPDATE) <= 2025;
SELECT COUNT (*) FROM TICKRAT WHERE EXTRACT (YEAR FROM RATDATE) >= 2023 AND EXTRACT(YEAR FROM RATDATE) <= 2025;