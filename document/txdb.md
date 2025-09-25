select *
from lakehouse_raw.txdb.r_txfooter as s
where s.bizdate = '2025-09-10'
  and s.lretailstoreid in (102)
  and s.sztatype in ('RT','RR','SA','VR');

SHOW PARTITIONS lakehouse_raw.txdb.r_txfooter;
