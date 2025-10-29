#!/usr/bin/env python3
import os
import sys
import time
import requests
import subprocess
import re
import tempfile

# --- CONFIGURATION ---
# NOTE: These values are taken directly from your latest Bash script.
TELEGRAM_BOT_TOKEN = "8457927004:AAEsyZQfJAztHXvkA830j4kLQYINVJ7ee_s"
TELEGRAM_CHAT_ID = "-5013802399"  # Corrected to single dash
BASE_URL = f"https://api.telegram.org/bot{TELEGRAM_BOT_TOKEN}/sendMessage"
LOG_FILE = "/var/log/auth.log"
LOGGER_TAG = "ssh_alert_py"

def escape_markdown(text):
    """Escapes special characters for Telegram MarkdownV2."""
    if not text:
        return ""
    # List of characters to escape: . - _ ( ) * ` # + { } = | ! > &
    # We escape them by preceding them with a backslash
    for char in r'.!()-_{}+=|#':
        text = text.replace(char, f'\\{char}')
    # These require special handling or are reserved for code blocks
    text = text.replace('*', '\\*')
    text = text.replace('`', '\\`')
    text = text.replace('>', '\\>')
    text = text.replace('&', '\\&')
    return text

def find_key_comment(fingerprint, user_name):
    """
    Looks up the key comment in the user's authorized_keys file 
    by calculating the fingerprint of each key and matching it 
    against the log fingerprint.
    """
    if fingerprint == "Not found/password login":
        return "N/A (Password/Log not found)"

    try:
        # Use 'getent' to find the user's home directory
        user_info = subprocess.run(['getent', 'passwd', user_name], capture_output=True, text=True, check=True)
        user_home = user_info.stdout.split(':')[5]
    except (subprocess.CalledProcessError, IndexError):
        return f"Error finding home directory for {user_name}"

    authorized_keys_file = os.path.join(user_home, '.ssh', 'authorized_keys')
    if not os.path.isfile(authorized_keys_file):
        return f"Key file not found: {authorized_keys_file}"

    if not os.access(authorized_keys_file, os.R_OK):
        return f"Access denied to {authorized_keys_file}"

    # Use a secure temporary file to write keys for fingerprint calculation
    with tempfile.NamedTemporaryFile(delete=True, mode='w', suffix=".pub") as tmp_key_file:
        
        try:
            with open(authorized_keys_file, 'r') as f:
                for line in f:
                    line = line.strip()
                    if not line or line.startswith('#'):
                        continue

                    parts = line.split()
                    if len(parts) < 2:
                        continue
                    
                    # Extract the key string (type + base64 data)
                    key_string = f"{parts[0]} {parts[1]}"
                    
                    # Write key to temp file and flush immediately
                    tmp_key_file.write(key_string + '\n')
                    tmp_key_file.flush()
                    
                    # Calculate fingerprint using ssh-keygen
                    try:
                        keygen_output = subprocess.run(
                            ['ssh-keygen', '-l', '-E', 'sha256', '-f', tmp_key_file.name], 
                            capture_output=True, 
                            text=True, 
                            check=True,
                            stderr=subprocess.DEVNULL # Suppress warnings
                        )
                        # Example output: '256 SHA256:XXX no comment (...)'
                        # We only need the SHA256:XXX part (second field in awk output)
                        keygen_fields = keygen_output.stdout.split()
                        key_file_fingerprint = keygen_fields[1] if len(keygen_fields) > 1 else None

                        if key_file_fingerprint == fingerprint:
                            # Match found! Extract the comment (everything after the key data)
                            comment = ' '.join(parts[2:]) if len(parts) > 2 else "No comment in key file"
                            return comment
                            
                    except subprocess.CalledProcessError:
                        # ssh-keygen failed for this specific key line (e.g., malformed key)
                        continue
        
        except IOError as e:
            return f"IO Error reading {authorized_keys_file}: {e}"
        
    return "Comment not found" # No key matched the fingerprint

def get_log_fingerprint(user_name, remote_ip):
    """
    Scans system logs for the publickey login entry to retrieve the SHA256 fingerprint.
    """
    # Wait a moment for the log entry to be written by sshd
    time.sleep(1) 
    
    fingerprint = "Not found/password login"
    log_line = ""
    
    # 1. Try journalctl (modern systemd)
    try:
        if os.path.exists('/bin/journalctl') or os.path.exists('/usr/bin/journalctl'):
            command = [
                'journalctl', 
                '_SYSTEMD_UNIT=ssh.service', 
                '--since', '1 minute ago', 
                '-g', f"Accepted publickey for {user_name} from {remote_ip}", 
                '-o', 'cat'
            ]
            result = subprocess.run(command, capture_output=True, text=True, check=False)
            log_line = result.stdout.strip().split('\n')[-1] if result.stdout else ""
    except Exception:
        pass # Fall through to file based log if journalctl fails

    # 2. Fallback to auth.log file
    if not log_line and os.path.exists(LOG_FILE):
        try:
            command = ['tail', '-n', '50', LOG_FILE]
            result = subprocess.run(command, capture_output=True, text=True, check=True)
            
            # Use regex to find the relevant line
            pattern = re.compile(rf'Accepted publickey for {re.escape(user_name)} from {re.escape(remote_ip)}.*(SHA256:[^\s]+)')
            
            for line in reversed(result.stdout.splitlines()):
                if line and f"Accepted publickey for {user_name} from {remote_ip}" in line:
                    log_line = line
                    break
        except Exception:
            pass # Fallback failed, return default
            
    # 3. Extract the fingerprint from the log line if found
    if log_line:
        # Regex to capture the SHA256 fingerprint
        match = re.search(r'(SHA256:[^\\s]+)', log_line)
        if match:
            fingerprint = match.group(0)
    
    return fingerprint


def send_telegram_message(message):
    """Sends the formatted message to the Telegram group."""
    payload = {
        'chat_id': TELEGRAM_CHAT_ID,
        'parse_mode': 'MarkdownV2',
        'text': message
    }
    
    try:
        response = requests.post(BASE_URL, data=payload, timeout=5)
        response.raise_for_status() # Raise an exception for bad status codes
        
        if response.json().get('ok'):
            os.system(f'logger -t {LOGGER_TAG} "SSH login alert sent successfully for user {os.getenv("PAM_USER")}."')
        else:
            os.system(f'logger -t {LOGGER_TAG} "Failed to send SSH login alert. Telegram API response: {response.text}"')

    except requests.exceptions.RequestException as e:
        os.system(f'logger -t {LOGGER_TAG} "Failed to send SSH login alert (Request Error): {e}"')
    except Exception as e:
        os.system(f'logger -t {LOGGER_TAG} "Failed to send SSH login alert (General Error): {e}"')


def main():
    # --- CHECK PAM VARIABLES ---
    user_name = os.getenv('PAM_USER')
    remote_ip = os.getenv('PAM_RHOST')
    
    if not user_name or not remote_ip:
        os.system(f'logger -t {LOGGER_TAG} "ERROR: PAM environment variables not available. Script aborted."')
        sys.exit(1)

    # --- GATHERING INFORMATION ---
    login_time = subprocess.run(['date', '+%Y-%m-%d %H:%M:%S %Z'], capture_output=True, text=True, check=False).stdout.strip()
    server_name = os.uname().nodename # hostname
    
    # Try to get connection details from SSH_CONNECTION
    ssh_connection = os.getenv('SSH_CONNECTION', '')
    if ssh_connection:
        parts = ssh_connection.split()
        if len(parts) == 4:
            connection_details = f"{parts[0]}:{parts[1]} -> {parts[2]}:{parts[3]}"
        else:
            connection_details = "Unknown Ports"
    else:
        connection_details = "Unknown Ports"

    # --- LOG PARSING FOR KEY FINGERPRINT ---
    key_fingerprint = get_log_fingerprint(user_name, remote_ip)

    # --- KEY COMMENT LOOKUP ---
    key_comment = find_key_comment(key_fingerprint, user_name)

    # --- AUTH LOG PRE-SEND MESSAGE ---
    log_message = f"SSH ALERT: Server={server_name}, User={user_name}, IP={remote_ip}, KeyComment={key_comment}, FP={key_fingerprint}"
    os.system(f'logger -t {LOGGER_TAG} "{log_message}"')

    # --- MESSAGE CONSTRUCTION (Markdown V2 format) ---
    escaped_user = escape_markdown(user_name)
    escaped_remote_ip = escape_markdown(remote_ip)
    escaped_server_name = escape_markdown(server_name)
    escaped_login_time = escape_markdown(login_time)
    escaped_conn_details = escape_markdown(connection_details)
    escaped_fingerprint = escape_markdown(key_fingerprint) 
    escaped_key_comment = escape_markdown(key_comment) 

    message = (
        f"🚨 *SSH Login Alert* 🚨\n\n"
        f"*Server*: {escaped_server_name}\n"
        f"*User*: \\`{escaped_user}\\`\n"
        f"*IP Address*: {escaped_remote_ip}\n"
        f"*Time (UTC)*: {escaped_login_time}\n\n"
        f"\\`Connection\\`: {escaped_conn_details}\n"
        f"*Key Fingerprint*: \\`{escaped_fingerprint}\\`\n"
        f"*Key Comment*: \\`{escaped_key_comment}\\`"
    )

    # --- SEND THE TELEGRAM MESSAGE ---
    send_telegram_message(message)
    
    sys.exit(0)

if __name__ == "__main__":
    main()
