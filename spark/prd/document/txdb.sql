
select *
from lakehouse_raw.txdb.r_txfooter
where szbusinessdate = '20250910'
  and lretailstoreid in (102)
  and sztatype in ('RT','RR','SA','VR');

SHOW PARTITIONS lakehouse_raw.txdb.r_txfooter;
SHOW TABLES IN lakehouse_raw.txdb;