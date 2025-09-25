# Lakehouse Namespace: lakehouse_raw.gold                                        
--- Small Job
SELECT COUNT(*) FROM lakehouse_raw.gold.r_artrac;
SELECT COUNT(*) FROM lakehouse_raw.gold.r_artuvul;
SELECT COUNT(*) FROM lakehouse_raw.gold.r_artvl;
SELECT COUNT(*) FROM lakehouse_raw.gold.r_avescope;
SELECT COUNT(*) FROM lakehouse_raw.gold.r_clidgene;
SELECT COUNT(*) FROM lakehouse_raw.gold.r_foudgene;
SELECT COUNT(*) FROM lakehouse_raw.gold.r_reseau;
SELECT COUNT(*) FROM lakehouse_raw.gold.r_sitattri;
SELECT COUNT(*) FROM lakehouse_raw.gold.r_sitdgene;
SELECT COUNT(*) FROM lakehouse_raw.gold.r_artuv;
SELECT COUNT(*) FROM lakehouse_raw.gold.r_artcons;
SELECT COUNT(*) FROM lakehouse_raw.gold.r_artul;
SELECT COUNT(*) FROM lakehouse_raw.gold.r_blientbl;
SELECT COUNT(*) FROM lakehouse_raw.gold.r_artuvattri;
SELECT COUNT(*) FROM lakehouse_raw.gold.r_cdedistrib;
SELECT COUNT(*) FROM lakehouse_raw.gold.r_cdeentcde;
SELECT COUNT(*) FROM lakehouse_raw.gold.r_strucobj;
SELECT COUNT(*) FROM lakehouse_raw.gold.r_strucrel;
SELECT COUNT(*) FROM lakehouse_raw.gold.r_tra_parpostes;
SELECT COUNT(*) FROM lakehouse_raw.gold.r_v_brand_site;
SELECT COUNT(*) FROM lakehouse_raw.gold.r_v_coca_active;

--- Medium Job
SELECT COUNT(*) FROM lakehouse_raw.gold.r_artcaisse;
SELECT COUNT(*) FROM lakehouse_raw.gold.r_artuc;
SELECT COUNT(*) FROM lakehouse_raw.gold.r_aveprix;
SELECT COUNT(*) FROM lakehouse_raw.gold.r_cdedetcde;
SELECT COUNT(*) FROM lakehouse_raw.gold.r_stocouch;
SELECT COUNT(*) FROM lakehouse_raw.gold.r_stodetre;
SELECT COUNT(*) FROM lakehouse_raw.gold.r_tra_strucobj;


--- Large Job
SELECT COUNT(*) FROM lakehouse_raw.gold.r_v_coca_site;
SELECT COUNT(*) FROM lakehouse_raw.gold.r_stomvt;


# GOLD: ora-golddb-sec.centralretail.com.vn

--- Small Job
SELECT COUNT(*) FROM GCEN509.blientbl WHERE EXTRACT(YEAR FROM bledmaj) >= 2023 AND EXTRACT(YEAR FROM bledmaj) <=2025;
SELECT COUNT(*) FROM GCEN509.cdedistrib WHERE EXTRACT(YEAR FROM lcddmaj) >= 2023 AND EXTRACT(YEAR FROM lcddmaj) <=2025;
SELECT COUNT(*) FROM GCEN509.cdeentcde WHERE EXTRACT(YEAR FROM ecddmaj) >= 2023 AND EXTRACT(YEAR FROM ecddmaj) <=2025;
SELECT COUNT(*) FROM GCEN509.artrac;
SELECT COUNT(*) FROM GCEN509.artuvul;
SELECT COUNT(*) FROM GCEN509.artvl;
SELECT COUNT(*) FROM GCEN509.avescope;
SELECT COUNT(*) FROM GCEN509.clidgene;
SELECT COUNT(*) FROM GCEN509.foudgene;
SELECT COUNT(*) FROM GCEN509.reseau;
SELECT COUNT(*) FROM GCEN509.sitattri;
SELECT COUNT(*) FROM GCEN509.sitdgene;
SELECT COUNT(*) FROM GCEN509.artuv;
SELECT COUNT(*) FROM GCEN509.artcons;
SELECT COUNT(*) FROM GCEN509.artul;
SELECT COUNT(*) FROM GCEN509.artuvattri;
SELECT COUNT(*) FROM GCEN509.strucobj;
SELECT COUNT(*) FROM GCEN509.strucrel;
SELECT COUNT(*) FROM GCEN509.tra_parpostes;
SELECT COUNT(*) FROM GCEN509.v_brand_site;
SELECT COUNT(*) FROM GCEN509.v_brand_site;
SELECT COUNT(*) FROM GDBIT.v_coca_active;

--- Medium Job
SELECT COUNT(*) FROM GCEN509.cdedetcde WHERE EXTRACT(YEAR FROM dcddmaj) >= 2023 AND EXTRACT(YEAR FROM dcddmaj) <=2025;
SELECT COUNT(*) FROM GCEN509.stocouch WHERE EXTRACT(YEAR FROM stodmaj) >= 2023 AND EXTRACT(YEAR FROM stodmaj) <=2025;
SELECT COUNT(*) FROM GCEN509.stodetre WHERE EXTRACT(YEAR FROM sdrdmaj) >= 2023 AND EXTRACT(YEAR FROM sdrdmaj) <=2025;
SELECT COUNT(*) FROM GCEN509.tra_strucobj;
SELECT COUNT(*) FROM GCEN509.artcaisse;
SELECT COUNT(*) FROM GCEN509.artuc;
SELECT COUNT(*) FROM GCEN509.aveprix;

--- Large Job
SELECT COUNT(*) FROM GCEN509.v_coca_site;
SELECT COUNT(*) FROM GCEN509.stomvt WHERE EXTRACT (YEAR FROM stmdmaj) >= 2023 AND EXTRACT(YEAR FROM stmdmaj) <= 2025; -- 455,279,619