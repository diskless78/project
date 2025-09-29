import os
import shutil
from datetime import datetime, timezone
from email.message import EmailMessage
import smtplib

# === Configuration ===
PROD_ORPHANED_TABLES = "/orphaned_backup/"
days_threshold = 7

# Email configuration
SMTP_SERVER = "10.250.2.159"
SMTP_PORT = 25
EMAIL_SENDER = "crv.sys.monitor@vn.centralretail.com"
EMAIL_RECIPIENT = "CRV.IT.DevOps@vn.centralretail.com"
EMAIL_SUBJECT = "[PROD] Clean Old Folders in NFS Orphaned Backup"


def send_deletion_report_email(deleted_folders_info, remaining_folders_info):
    rows = []
    for folder, info in deleted_folders_info.items():
        rows.append({
            "status": "🗑️ Deleted",
            "folder": folder,
            "last_modified": info['last_modified'],
            "age": info['age_days']
        })
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
                    <th>Last Modified</th>
                    <th>Age (days)</th>
                </tr>
            </thead>
            <tbody>{table_rows}</tbody>
        </table>
        """
    else:
        table_html = "<p><i>No folders found.</i></p>"

    html_body = f"""
    <html>
      <body>
        <h2>Local NFS Path: {PROD_ORPHANED_TABLES} - Current Folder Status</h2>
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
        print("✅ Email report sent successfully.")
    except Exception as e:
        print(f"❌ Failed to send email: {e}")


def delete_old_orphaned_nfs(days_threshold):
    print(f"🔍 Scanning {PROD_ORPHANED_TABLES} recursively for orphaned *leaf* folders older than {days_threshold} days...")

    deleted_folders_info = {}
    remaining_folders_info = {}

    if not os.path.exists(PROD_ORPHANED_TABLES):
        print(f"❌ Path does not exist: {PROD_ORPHANED_TABLES}")
        return deleted_folders_info, remaining_folders_info

    # Walk top-down so we can check "leaf" dirs
    for root, dirs, files in os.walk(PROD_ORPHANED_TABLES, topdown=True):
        for folder in dirs:
            folder_path = os.path.join(root, folder)

            # Check if this is a leaf folder (no subdirs inside)
            if any(os.path.isdir(os.path.join(folder_path, d)) for d in os.listdir(folder_path)):
                continue  # skip non-leaf folders

            folder_mtime = os.path.getmtime(folder_path)
            last_modified = datetime.fromtimestamp(folder_mtime, timezone.utc)
            age = datetime.now(timezone.utc) - last_modified

            if age.days >= days_threshold:
                print(f"🧹 Deleting leaf folder: {folder_path} "
                      f"(LastModified: {last_modified}, Age: {age.days} days)")
                deleted_folders_info[folder_path] = {
                    "last_modified": last_modified.strftime("%Y-%m-%d %H:%M:%S %Z"),
                    "age_days": age.days
                }
                try:
                    shutil.rmtree(folder_path)
                    print(f"   ✅ Deleted {folder_path}")
                except Exception as e:
                    print(f"   ❌ Failed to delete {folder_path}: {e}")
            else:
                remaining_folders_info[folder_path] = {
                    "last_modified": last_modified.strftime("%Y-%m-%d %H:%M:%S %Z"),
                    "age_days": age.days
                }

    return deleted_folders_info, remaining_folders_info


def main():
    deleted_folders_info, remaining_folders_info = delete_old_orphaned_nfs(days_threshold)
    send_deletion_report_email(deleted_folders_info, remaining_folders_info)


if __name__ == "__main__":
    main()