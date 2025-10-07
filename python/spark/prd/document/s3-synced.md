aws configure set aws_access_key_id "$SIT_LAKEHOUSE_RAW_ACCESSKEY" --profile sit
aws configure set aws_secret_access_key "$SIT_LAKEHOUSE_RAW_SECRETKEY" --profile sit

aws s3 sync s3://group-lakehouse-raw-sit/ /orphaned_backup \
    --endpoint-url https://$S3_ENDPOINT --profile sit


aws s3 sync s3://group-lakehouse-raw-prd/ s3://group-lakehouse-raw-sit/ \
    --endpoint-url https://$S3_ENDPOINT --profile sit
