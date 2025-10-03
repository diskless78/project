import os
import time
import smtplib
from email.message import EmailMessage
from datetime import datetime, timedelta
from pyspark.sql import SparkSession

# === Configuration ===
SPARK_APP_NAME = "Expired_Snapshot_Cleanup"
SMTP_SERVER = "10.250.2.159"
SMTP_PORT = 25
EMAIL_SENDER = "crv.sys.monitor@vn.centralretail.com"
EMAIL_RECIPIENT = "CRV.IT.DevOps@vn.centralretail.com"
EMAIL_SUBJECT = "[PROD-LAKEHOUSE] Expired Snapshots Summary"
ROOT_WAREHOUSE = "warehouse"
S3_ENDPOINT = os.getenv("S3_ENDPOINT")

CATALOGS = [
    {
        "name": "lakehouse_raw",
        "bucket": os.getenv("LAKEHOUSE_RAW_BUCKET"),
        "access_key": os.getenv("LAKEHOUSE_RAW_ACCESSKEY"),
        "secret_key": os.getenv("LAKEHOUSE_RAW_SECRETKEY"),
        "nessie_uri": os.getenv("NESSIE_ENDPOINT_RAW"),
        "bearer_token": os.getenv("NESSIE_TOKEN_RAW"),
        "branch": "main"
    },
    {
        "name": "lakehouse_refined",
        "bucket": os.getenv("LAKEHOUSE_REFINED_BUCKET"),
        "access_key": os.getenv("LAKEHOUSE_REFINED_ACCESSKEY"),
        "secret_key": os.getenv("LAKEHOUSE_REFINED_SECRETKEY"),
        "nessie_uri": os.getenv("NESSIE_ENDPOINT_REFINED"),
        "bearer_token": os.getenv("NESSIE_TOKEN_REFINED"),
        "branch": "main"
    },
    {
        "name": "lakehouse_structured",
        "bucket": os.getenv("LAKEHOUSE_STRUCTURED_BUCKET"),
        "access_key": os.getenv("LAKEHOUSE_STRUCTURED_ACCESSKEY"),
        "secret_key": os.getenv("LAKEHOUSE_STRUCTURED_SECRETKEY"),
        "nessie_uri": os.getenv("NESSIE_ENDPOINT_STRUCTURED"),
        "bearer_token": os.getenv("NESSIE_TOKEN_STRUCTURED"),
        "branch": "main"
    },
    {
        "name": "lakehouse_archive",
        "bucket": os.getenv("LAKEHOUSE_ARCHIVE_BUCKET"),
        "access_key": os.getenv("LAKEHOUSE_ARCHIVE_ACCESSKEY"),
        "secret_key": os.getenv("LAKEHOUSE_ARCHIVE_SECRETKEY"),
        "nessie_uri": os.getenv("NESSIE_ENDPOINT_ARCHIVE"),
        "bearer_token": os.getenv("NESSIE_ENDPOINT_ARCHIVE"),
        "branch": "archive"
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
        .config(f"spark.sql.catalog.{catalog['name']}.ref", catalog['branch']) \
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

# === Get all namespaces ===
def get_all_namespaces(spark, catalog):
    try:
        return [row.namespace for row in spark.sql(f"SHOW NAMESPACES IN {catalog['name']}").collect()]
    except Exception as e:
        print(f"[ERROR] Failed to fetch namespaces for {catalog['name']}: {e}")
        return []
# === check and set table properties ===
def set_table_properties(spark, full_table: str, required_props: dict):
    """
    Check Iceberg table properties and only set the ones that are missing.
    """
    try:
        # Get all current properties
        props_df = spark.sql(f"SHOW TBLPROPERTIES {full_table}")
        current_props = {row['key']: row['value'] for row in props_df.collect()}

        # Filter properties that need to be updated
        updates = {
            k: v for k, v in required_props.items()
            if k not in current_props or str(current_props[k]) != str(v)
        }

        if updates:
            props_sql = ",\n    ".join([f"'{k}'='{v}'" for k, v in updates.items()])
            alter_sql = f"ALTER TABLE {full_table} SET TBLPROPERTIES ({props_sql})"
            spark.sql(alter_sql)
            print(f"[INFO] Updated {len(updates)} properties on {full_table}: {updates}")
        else:
            print(f"[INFO] All required properties already set on {full_table}")

    except Exception as e:
        print(f"[ERROR] Failed to check/alter properties for {full_table}: {e}")


# === Expire snapshots for table ===
def expire_snapshot_for_table(spark, catalog_name, namespace, table_name):
    """
        lakehouse_refined, lakehouse_structured: retain last 3 snapshots
        lakehouse_raw, lakehouse_archive: expire everything older than latest
    """
    full_table = f"{catalog_name}.{namespace}.{table_name}"

    # Required Iceberg properties
    required_props = {
        "gc.enabled": "true",
        "write.target-file-size-bytes": "536870912",
        "write.parquet.row-group-size-bytes": "134217728",
        "write.metadata.delete-after-commit.enabled": "true",
        "write.metadata.previous-versions-max": "5"
    }
    set_table_properties(spark, full_table, required_props)

    RETAIN_LAST_CATALOGS = {"lakehouse_refined", "lakehouse_structured"}

    try:
        # Count snapshot
        snapshot_count = spark.sql(f"SELECT COUNT(*) as c FROM {full_table}.snapshots").collect()[0]['c']
        if snapshot_count == 0:
            print(f"[INFO] No snapshots found for {full_table}. Skipping rewrite and expire.")
            return None

        # Rewrite manifests
        rewrite_sql = f"""
            CALL {catalog_name}.system.rewrite_manifests(
                table => '{namespace}.{table_name}'
            )
        """
        spark.sql(rewrite_sql)
        print(f"[INFO] Rewrote manifests for {full_table} before expiring snapshots.")

        if catalog_name in RETAIN_LAST_CATALOGS:
            # Retain last 3 snapshots
            call_sql = f"""
                CALL {catalog_name}.system.expire_snapshots(
                    table => '{namespace}.{table_name}',
                    retain_last => 3
                )
            """
            print(f"[INFO] Applying 'retain_last=3' policy for {full_table} in {catalog_name}.")
            spark.sql(call_sql)
            print(f"[INFO] Expired old snapshots for {full_table}, retaining the last 3.")
            return "Retained last 3 snapshots"

        else:
            # Expire everything older than latest
            latest_ts_df = spark.sql(f"""
                SELECT committed_at
                FROM {full_table}.snapshots
                ORDER BY committed_at DESC
                LIMIT 1
            """)
            latest_ts = latest_ts_df.collect()[0]['committed_at']

            call_sql = f"""
                CALL {catalog_name}.system.expire_snapshots(
                    table => '{namespace}.{table_name}',
                    older_than => TIMESTAMP '{latest_ts}'
                )
            """
            spark.sql(call_sql)
            print(f"[INFO] Expired snapshots older than {latest_ts} for {full_table} in {catalog_name}.")
            return str(latest_ts)

    except Exception as e:
        print(f"[ERROR] Failed to expire snapshots for {full_table}: {e}")
        return None

# === Explore snapshots for all tables in namespace ===
def expire_snapshots_in_namespace(spark, catalog, namespace, summary):
    try:
        tables = spark.sql(f"SHOW TABLES IN {catalog['name']}.{namespace}").collect()
        for table in tables:
            latest_ts = expire_snapshot_for_table(spark, catalog['name'], namespace, table.tableName)
            if latest_ts:
                summary.setdefault(catalog['name'], {}).setdefault(namespace, {})[table.tableName] = latest_ts
                
            print(f"[INFO] Waiting 10 seconds before processing the next table...")
            time.sleep(10)
            
    except Exception as e:
        print(f"[ERROR] Failed to list tables in namespace {namespace}: {e}")

# === Send email notification ===
def send_email_notification(summary):
    msg = EmailMessage()
    msg["Subject"] = EMAIL_SUBJECT
    msg["From"] = EMAIL_SENDER
    msg["To"] = EMAIL_RECIPIENT

    html_body = "<html><body><h3>Expired Snapshots Cleanup Summary for All PROD Lakehouses</h3>"

    for cat_name, rows in summary.items():
        html_body += f"<h4 style='color:#2d89ef;'>{cat_name}</h4>"
        html_body += "<table border='1' cellpadding='5' cellspacing='0' style='border-collapse: collapse;'>"
        html_body += "<tr><th>Namespace</th><th>Table Name</th><th>Latest Snapshot (GMT+7)</th></tr>"

        if rows:
            for namespace, table_name, latest_ts in rows:
                try:
                    # Add 7 hours
                    ts_obj = datetime.fromisoformat(latest_ts) + timedelta(hours=7)
                    display_ts = ts_obj.strftime("%Y-%m-%d %H:%M:%S")
                except Exception:
                    display_ts = latest_ts

                html_body += (
                    f"<tr>"
                    f"<td>{namespace}</td>"
                    f"<td>{table_name}</td>"
                    f"<td>{display_ts}</td>"
                    f"</tr>"
                )
        else:
            html_body += "<tr><td colspan='3'>No snapshots expired.</td></tr>"

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

# === Process catalog
def process_catalog(catalog):
    """
    Process snapshots for all tables in a catalog and flatten all table-timestamp pairs
    so that each table has its own row: namespace | table_name | latest_snapshot
    """
    spark = create_spark_session(catalog)
    catalog_summary = []

    try:
        namespaces = get_all_namespaces(spark, catalog)
        for namespace in namespaces:
            try:
                tables = spark.sql(f"SHOW TABLES IN {catalog['name']}.{namespace}").collect()
                for table in tables:
                    table_name = table.tableName
                    latest_snapshot = expire_snapshot_for_table(spark, catalog['name'], namespace, table_name)
                    if latest_snapshot:
                        catalog_summary.append((namespace, table_name, latest_snapshot))
            except Exception as e:
                print(f"[ERROR] Failed to process namespace {namespace} in catalog {catalog['name']}: {e}")

    except Exception as e:
        print(f"[ERROR] Failed to process catalog {catalog['name']}: {e}")
    finally:
        spark.stop()
        print(f"[INFO] Spark session for catalog {catalog['name']} stopped.")

    return catalog_summary


# === Main ===
def main():
    final_summary = {}
    for catalog in CATALOGS:
        print(f"\n=== Processing Catalog: {catalog['name']} ===")
        catalog_result = process_catalog(catalog)
        final_summary[catalog['name']] = catalog_result

    send_email_notification(final_summary)
    print("\n=== Expired snapshot cleanup completed successfully ===\n")

if __name__ == "__main__":
    main()