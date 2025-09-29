# Copy table from NFS to S3

    s3cmd sync /orphaned_backup/lakehouse_raw/cbs/test_eacb2ae3-7d7c-4edf-b3b3-3d849e081b62 s3://group-lakehouse-raw-sit/warehouse/cbs

    s3cmd put --recursive /orphaned_backup/lakehouse_raw/cbs/test_eacb2ae3-7d7c-4edf-b3b3-3d849e081b62 s3://group-lakehouse-raw-sit/warehouse/cbs


