{
	"AccessKey": {
		"UserName": "s3_group-lakehouse-archive",
		"AccessKeyId": "XLIWV8311LFM7Y1PAODQ",
		"SecretAccessKey": "CQcDcPBYA_9m5AkJ1gmtIOBj02ICqaL00RA1UH3w"
	}
}

show schemas in lakehouse_platform;
SHOW TABLES IN lakehouse_platform.payment_gw;
select count(*) from lakehouse_platform.payment_gw.strapi_request_logs;
select * from lakehouse_platform.payment_gw.strapi_request_logs;


select count(*) from lakehouse_platform.payment_gw.strapi_request_logs where extract(year from created_at) = 2024 
and extract(month from created_at) = 1 
and extract(day from created_at) = 1;

select * from lakehouse_platform.payment_gw.strapi_request_logs where created_at >= DATE '2024-01-01' 
and created_at < DATE '2024-12-01';

select count(*) from lakehouse_platform.payment_gw.strapi_request_logs where created_at >= '2024-01-01 00:00:00' 
and created_at < '2024-02-01 00:00:00';