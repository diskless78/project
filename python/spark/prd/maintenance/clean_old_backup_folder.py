import os
import shutil
from datetime import datetime, timezone
import smtplib
from email.message import EmailMessage

# === Configuration ===
LAKEHOUSE_ZONES = ["lakehouse_archive", "lakehouse_raw", "lakehouse_refined", "lakehouse_structured"]

PROD_ORPHANED_TABLES = "/orphaned_backup"
days_threshold = 7

TARGET_BASE_PATHS = [] 

# Email configuration
SMTP_SERVER = "10.250.2.159"
SMTP_PORT = 25
EMAIL_SENDER = "crv.sys.monitor@vn.centralretail.com"
EMAIL_RECIPIENT = "CRV.IT.DevOps@vn.centralretail.com"
EMAIL_SUBJECT = "[PROD] Clean Old Folders in NFS Orphaned Backup"

# === Helper ===
def discover_target_base_paths():
    discovered_paths = []
    
    print(f"\nDiscovering target base paths under {PROD_ORPHANED_TABLES}...")

    for zone in LAKEHOUSE_ZONES:
        warehouse_path = os.path.join(PROD_ORPHANED_TABLES, zone, "warehouse")
        
        if not os.path.isdir(warehouse_path):
            print(f"  - Zone skip: '{warehouse_path}' not found.")
            continue
            
        try:
            db_folders = os.listdir(warehouse_path)
        except OSError as e:
            print(f"  - Error listing '{warehouse_path}': {e}. Skipping zone.")
            continue
            
        for db_folder in db_folders:
            target_path = os.path.join(warehouse_path, db_folder)
            
            if os.path.isdir(target_path):
                discovered_paths.append(target_path)
                print(f"  + Discovered: {target_path}")

    return discovered_paths

def get_folder_age_info(folder_path):
    try:
        folder_mtime = os.path.getmtime(folder_path)
    except OSError as e:
        print(f"Cannot get modification time for {folder_path}: {e}")
        return None

    last_modified = datetime.fromtimestamp(folder_mtime, timezone.utc)
    age = datetime.now(timezone.utc) - last_modified

    return {
        "last_modified": last_modified.strftime("%Y-%m-%d %H:%M:%S %Z"),
        "age_days": age.days
    }

def send_deletion_report_email(deleted_folders_info, remaining_folders_info):
    rows = []
    
    # Process deleted folders
    for folder, info in deleted_folders_info.items():
        rows.append({
            "status": "🗑️ Deleted",
            "folder": folder,
            "last_modified": info['last_modified'],
            "age": info['age_days']
        })
        
    # Process remaining folders
    for folder, info in remaining_folders_info.items():
        rows.append({
            "status": "📁 Remaining",
            "folder": folder,
            "last_modified": info['last_modified'],
            "age": info['age_days']
        })

    if rows:
        table_rows = "".join(
            f"<tr><td>{row['status']}</td>"
            f"<td>{row['folder']}</td>"
            f"<td>{row['last_modified']}</td>"
            f"<td>{row['age']}</td></tr>"
            for row in rows
        )
        table_html = f"""
        <table border="1" cellpadding="5" cellspacing="0" style="border-collapse: collapse; width: 100%;">
            <thead style="background-color:#f2f2f2;">
                <tr>
                    <th>Status</th>
                    <th>Folder</th>
                    <th>Last Modified (UTC)</th>
                    <th>Age (days)</th>
                </tr>
            </thead>
            <tbody>{table_rows}</tbody>
        </table>
        """
    else:
        table_html = "<p><i>No table folders found in the target paths.</i></p>"

    html_body = f"""
    <html>
      <body>
        <h2>Local NFS Path: {PROD_ORPHANED_TABLES} - Current Table Folder Status</h2>
        <p>Threshold used: {days_threshold} days</p>
        {table_html}
      </body>
    </html>
    """

    msg = EmailMessage()
    msg["Subject"] = EMAIL_SUBJECT
    msg["From"] = EMAIL_SENDER
    msg["To"] = EMAIL_RECIPIENT
    msg.set_content("This email requires an HTML-compatible email client.")
    msg.add_alternative(html_body, subtype='html')

    try:
        with smtplib.SMTP(SMTP_SERVER, SMTP_PORT) as server:
            server.send_message(msg)
        print("Email report sent successfully.")
    except Exception as e:
        print(f"Failed to send email: {e}")


# === Main ===

def delete_old_orphaned_nfs(days_threshold, TARGET_BASE_PATHS: list):
    print(f"Scanning predefined base paths for table folders older than {days_threshold} days...")

    deleted_folders_info = {}
    remaining_folders_info = {}
    
    now_utc = datetime.now(timezone.utc) 

    for base_path in TARGET_BASE_PATHS:
        if not os.path.exists(base_path):
            print(f"Base path does not exist, skipping: {base_path}")
            continue
        
        print(f"\nScanning: {base_path}")
        
        try:
            entries = os.listdir(base_path)
        except OSError as e:
            print(f"Cannot list directory {base_path}: {e}")
            continue

        for entry in entries:
            folder_path = os.path.join(base_path, entry)
            
            # Only process directories (the table folders)
            if not os.path.isdir(folder_path):
                continue
            
            age_info = get_folder_age_info(folder_path)
            if age_info is None:
                continue

            age_days = age_info['age_days']
            
            if age_days >= days_threshold:
                print(f"🧹 Deleting table folder: {folder_path} "
                      f"(LastModified: {age_info['last_modified']}, Age: {age_days} days)")
                
                deleted_folders_info[folder_path] = age_info
                
                try:
                    shutil.rmtree(folder_path)
                    print(f"   Deleted {folder_path}")
                except Exception as e:
                    print(f"   Failed to delete {folder_path}: {e}")
            else:
                remaining_folders_info[folder_path] = age_info

    return deleted_folders_info, remaining_folders_info


def main():
    print("--- Starting Old Backup Folder Clean-up ---")
    TARGET_BASE_PATHS = discover_target_base_paths()
    
    if not TARGET_BASE_PATHS:
        print("No valid target database directories were found. Exiting.")
        return
    deleted_folders_info, remaining_folders_info = delete_old_orphaned_nfs(days_threshold, TARGET_BASE_PATHS)
    send_deletion_report_email(deleted_folders_info, remaining_folders_info)
    print("--- Clean-up Complete ---")


if __name__ == "__main__":
    main()