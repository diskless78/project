import os
import smtplib
from email.message import EmailMessage
import boto3
from pyspark.sql import SparkSession

SPARK_APP_NAME = "Clean_Orphaned_Table"
SMTP_SERVER = "10.250.2.159"
SMTP_PORT = 25
EMAIL_SENDER = "crv.sys.monitor@vn.centralretail.com"
EMAIL_RECIPIENT = "CRV.IT.DevOps@vn.centralretail.com"
EMAIL_SUBJECT = "[PROD-LAKEHOUSE] Orphaned and Unknown Table and Folder Cleanup"

ORPHANED_BACKUP_DIR = "/orphaned_backup"
ROOT_WAREHOUSE = "warehouse"
S3_ENDPOINT = os.getenv("S3_ENDPOINT")

# === Catalog Configurations ===
CATALOGS = [
    {
        "name": "lakehouse_raw",
        "bucket": os.getenv("LAKEHOUSE_RAW_BUCKET"),
        "access_key": os.getenv("LAKEHOUSE_RAW_ACCESSKEY"),
        "secret_key": os.getenv("LAKEHOUSE_RAW_SECRETKEY"),
        "nessie_uri": os.getenv("NESSIE_ENDPOINT_RAW"),
        "bearer_token": os.getenv("NESSIE_TOKEN_RAW")
    },
    {
        "name": "lakehouse_refined",
        "bucket": os.getenv("LAKEHOUSE_REFINED_BUCKET"),
        "access_key": os.getenv("LAKEHOUSE_REFINED_ACCESSKEY"),
        "secret_key": os.getenv("LAKEHOUSE_REFINED_SECRETKEY"),
        "nessie_uri": os.getenv("NESSIE_ENDPOINT_REFINED"),
        "bearer_token": os.getenv("NESSIE_TOKEN_REFINED")
    },
    {
        "name": "lakehouse_structured",
        "bucket": os.getenv("LAKEHOUSE_STRUCTURED_BUCKET"),
        "access_key": os.getenv("LAKEHOUSE_STRUCTURED_ACCESSKEY"),
        "secret_key": os.getenv("LAKEHOUSE_STRUCTURED_SECRETKEY"),
        "nessie_uri": os.getenv("NESSIE_ENDPOINT_STRUCTURED"),
        "bearer_token": os.getenv("NESSIE_TOKEN_STRUCTURED")
    },
    {
        "name": "lakehouse_platform",
        "bucket": os.getenv("LAKEHOUSE_PLATFORM_BUCKET"),
        "access_key": os.getenv("LAKEHOUSE_PLATFORM_ACCESSKEY"),
        "secret_key": os.getenv("LAKEHOUSE_PLATFORM_SECRETKEY"),
        "nessie_uri": os.getenv("NESSIE_ENDPOINT_PLATFORM"),
        "bearer_token": os.getenv("NESSIE_TOKEN_PLATFORM")
    }
]

# === Spark Session ===
def create_spark_session(catalog):
    return SparkSession.builder \
        .appName(f"{SPARK_APP_NAME}_{catalog['name']}") \
        .config(f"spark.sql.catalog.{catalog['name']}", "org.apache.iceberg.spark.SparkCatalog") \
        .config("spark.sql.extensions", "org.apache.iceberg.spark.extensions.IcebergSparkSessionExtensions") \
        .config(f"spark.sql.catalog.{catalog['name']}.catalog-impl", "org.apache.iceberg.nessie.NessieCatalog") \
        .config(f"spark.sql.catalog.{catalog['name']}.warehouse", f"s3a://{catalog['bucket']}/{ROOT_WAREHOUSE}") \
        .config(f"spark.sql.catalog.{catalog['name']}.uri", catalog['nessie_uri']) \
        .config(f"spark.sql.catalog.{catalog['name']}.ref", "main") \
        .config(f"spark.sql.catalog.{catalog['name']}.authentication.type", "BEARER") \
        .config(f"spark.sql.catalog.{catalog['name']}.authentication.token", catalog['bearer_token']) \
        .config("spark.hadoop.fs.s3a.endpoint", S3_ENDPOINT) \
        .config("spark.hadoop.fs.s3a.access.key", catalog['access_key']) \
        .config("spark.hadoop.fs.s3a.secret.key", catalog['secret_key']) \
        .config("spark.hadoop.fs.s3a.path.style.access", "true") \
        .config("spark.hadoop.fs.s3a.connection.ssl.enabled", "true") \
        .config("spark.hadoop.fs.s3a.impl", "org.apache.hadoop.fs.s3a.S3AFileSystem") \
        .config("spark.hadoop.fs.s3.impl", "org.apache.hadoop.fs.s3a.S3AFileSystem") \
        .getOrCreate()

# === Get all namespaces in lakehouse catalog ===
def get_all_namespaces(spark, catalog):
    try:
        rows = spark.sql(f"SHOW NAMESPACES IN {catalog['name']}").collect()
        return [row.namespace for row in rows]
    except Exception as e:
        print(f"[ERROR] Failed to fetch namespaces for {catalog['name']}: {e}")
        return []

# === Get used table location for each namespace ===
def get_used_locations(spark, namespace, catalog):
    used_locations = set()
    try:
        tables = spark.sql(f"SHOW TABLES IN {catalog['name']}.{namespace}").collect()
        for table in tables:
            full_table = f"{catalog['name']}.{namespace}.{table.tableName}"
            try:
                desc = spark.sql(f"DESCRIBE TABLE EXTENDED {full_table}").collect()
                for row in desc:
                    if row.col_name.strip().lower() == "location":
                        normalized_path = (
                            row.data_type.strip()
                            .replace("s3://", "s3a://")
                            .split("/metadata/")[0]
                            .rstrip("/") + "/"
                        )
                        used_locations.add(normalized_path)
            except Exception as e:
                print(f"[ERROR] Failed to describe table {full_table}: {e}")
    except Exception as e:
        print(f"[ERROR] Failed to list tables in namespace {namespace}: {e}")
    return used_locations

# === List all folders in a namespace ===
def list_s3_folders_for_namespace(namespace, catalog):
    s3 = boto3.client(
        "s3",
        aws_access_key_id=catalog['access_key'],
        aws_secret_access_key=catalog['secret_key'],
        endpoint_url=f"https://{S3_ENDPOINT}"
    )
    prefix = f"{ROOT_WAREHOUSE}/{namespace}/"
    paginator = s3.get_paginator("list_objects_v2")
    folders = []
    for page in paginator.paginate(Bucket=catalog['bucket'], Prefix=prefix, Delimiter='/'):
        for common_prefix in page.get('CommonPrefixes', []):
            folders.append(f"s3a://{catalog['bucket']}/{common_prefix['Prefix']}")
    return folders

# === Find orphaned folders in a namespace ===
def find_orphan_folders_for_namespace(namespace, used_locations, catalog):
    all_folders = list_s3_folders_for_namespace(namespace, catalog)
    normalized_used = {loc.rstrip("/") + "/" for loc in used_locations}
    return [folder for folder in all_folders if folder.rstrip("/") + "/" not in normalized_used]

# === ind and delete orphaned table in each namespace ===
def clean_orphaned_table_in_namespace(namespace, folder, catalog):
    s3 = boto3.client(
        "s3",
        aws_access_key_id=catalog['access_key'],
        aws_secret_access_key=catalog['secret_key'],
        endpoint_url=f"https://{S3_ENDPOINT}"
    )
    source_prefix = folder.replace("s3a://", "").split("/", 1)[1]
    
    # Backup the folder before deletion
    backup_s3_folder_to_nfs(catalog, source_prefix)
    
    paginator = s3.get_paginator("list_objects_v2")
    for page in paginator.paginate(Bucket=catalog['bucket'], Prefix=source_prefix):
        for obj in page.get("Contents", []):
            try:
                s3.delete_object(Bucket=catalog['bucket'], Key=obj['Key'])
            except Exception as e:
                print(f"[ERROR] Failed to delete {obj['Key']}: {e}")

# === Find and delete orphaned table in each lakehouse catalog ===
def clean_orphaned_table_in_catalog(namespaces, catalog):
    s3 = boto3.client(
        "s3",
        aws_access_key_id=catalog['access_key'],
        aws_secret_access_key=catalog['secret_key'],
        endpoint_url=f"https://{S3_ENDPOINT}"
    )
    prefix = f"{ROOT_WAREHOUSE}/"
    paginator = s3.get_paginator("list_objects_v2")
    deleted_folders = []

    for page in paginator.paginate(Bucket=catalog['bucket'], Prefix=prefix, Delimiter='/'):
        for common_prefix in page.get('CommonPrefixes', []):
            folder_name = common_prefix['Prefix'].split('/')[1]
            if folder_name not in namespaces:
                full_path = f"s3a://{catalog['bucket']}/{common_prefix['Prefix']}"

                # Backup the folder before deletion
                source_prefix = common_prefix['Prefix']
                backup_s3_folder_to_nfs(catalog, source_prefix)
                
                # Delete all objects in that folder
                for sub_page in paginator.paginate(Bucket=catalog['bucket'], Prefix=common_prefix['Prefix']):
                    for obj in sub_page.get("Contents", []):
                        try:
                            s3.delete_object(Bucket=catalog['bucket'], Key=obj['Key'])
                        except Exception as e:
                            print(f"[ERROR] Failed to delete {obj['Key']}: {e}")

                deleted_folders.append(full_path)

    return deleted_folders

# ===  Backup folder from S3 to NFS ===
def backup_s3_folder_to_nfs(catalog, source_prefix):
    s3 = boto3.client(
        "s3",
        aws_access_key_id=catalog['access_key'],
        aws_secret_access_key=catalog['secret_key'],
        endpoint_url=f"https://{S3_ENDPOINT}"
    )
    
    backup_folder = os.path.join(ORPHANED_BACKUP_DIR, catalog['name'], source_prefix)
    os.makedirs(backup_folder, exist_ok=True)

    paginator = s3.get_paginator("list_objects_v2")
    for page in paginator.paginate(Bucket=catalog['bucket'], Prefix=source_prefix):
        for obj in page.get("Contents", []):
            key = obj['Key']
            local_path = os.path.join(backup_folder, os.path.relpath(key, source_prefix))
            os.makedirs(os.path.dirname(local_path), exist_ok=True)
            try:
                s3.download_file(catalog['bucket'], key, local_path)
                print(f"[INFO] Backed up: {key} -> {local_path}")
            except Exception as e:
                print(f"[ERROR] Failed to backup {key}: {e}")
                
# === Send email report ===
def send_email_notification(summary):
    msg = EmailMessage()
    msg["Subject"] = EMAIL_SUBJECT
    msg["From"] = EMAIL_SENDER
    msg["To"] = EMAIL_RECIPIENT

    html_body = "<html><body><h3>Cleanup Summary for All PROD Lakehouses</h3>"
    for cat_name, deleted in summary.items():
        html_body += f"<h4 style='color:#2d89ef;'>{cat_name}</h4>"
        html_body += "<table border='1' cellpadding='5' cellspacing='0' style='border-collapse: collapse;'>"
        html_body += "<tr><th>Namespace</th><th>Orphaned Folder</th><th>Moved To NFS</th></tr>"
        if deleted:
            for ns, folders in deleted.items():
                if ns == "__unknown_root_folders__":
                    ns_label = "root warehouse"
                else:
                    ns_label = ns

                for folder in folders:
                    source_prefix = folder.replace("s3a://", "").split("/", 1)[1]
                    backup_path = os.path.join(ORPHANED_BACKUP_DIR, cat_name, source_prefix)
                    html_body += (
                        f"<tr>"
                        f"<td>{ns_label}</td>"
                        f"<td>{folder}</td>"
                        f"<td>{backup_path}</td>"
                        f"</tr>"
                    )
        else:
            html_body += "<tr><td colspan='3'>No orphaned or unknown folder found.</td></tr>"
        html_body += "</table><br>"
    html_body += "</body></html>"

    msg.set_content("This email requires an HTML-compatible email client.")
    msg.add_alternative(html_body, subtype='html')

    try:
        with smtplib.SMTP(SMTP_SERVER, SMTP_PORT) as server:
            server.send_message(msg)
        print("[INFO] Email notification sent successfully.")
    except Exception as e:
        print(f"[ERROR] Failed to send email notification: {e}")

# === Main catalog processing ===
def process_catalog(catalog):
    spark = create_spark_session(catalog)
    overall_deleted = {}

    namespaces = get_all_namespaces(spark, catalog)
    for namespace in namespaces:
        used_locations = get_used_locations(spark, namespace, catalog)
        orphan_folders = find_orphan_folders_for_namespace(namespace, used_locations, catalog)
        if orphan_folders:
            for folder in orphan_folders:
                clean_orphaned_table_in_namespace(namespace, folder, catalog)
            overall_deleted[namespace] = orphan_folders

    unknown_deleted = clean_orphaned_table_in_catalog(namespaces, catalog)
    if unknown_deleted:
        overall_deleted["__unknown_root_folders__"] = unknown_deleted

    spark.stop()
    return overall_deleted

# === Main  ===
def main():
    final_summary = {}
    for catalog in CATALOGS:
        print(f"\n=== Processing Catalog: {catalog['name']} ===")
        deleted_items = process_catalog(catalog)
        final_summary[catalog['name']] = deleted_items

    send_email_notification(final_summary)
    print("\n=== Cleanup completed successfully ===\n")

if __name__ == "__main__":
    main()
