#!/bin/bash

# === Telegram Config ===
BOT_TOKEN="8457927004:AAEsyZQfJAztHXvkA830j4kLQYINVJ7ee_s"
CHAT_ID="-1003262590904"

# === File Paths ===
AUTHORIZED_KEYS="/root/.ssh/authorized_keys"
LOG_FILE="/var/log/auth.log"
SYSLOG_FILE="/var/log/syslog"
TMP_DIR="/tmp/ssh_sessions"
mkdir -p "$TMP_DIR"

LAST_USER=""
LAST_IP=""
LAST_FINGERPRINT=""
SESSION_DETECTED=0

# Telegram send function
send_telegram() {
    local message="$1"
    curl -s -X POST "https://api.telegram.org/bot${BOT_TOKEN}/sendMessage" \
         -d "chat_id=${CHAT_ID}" \
         -d "text=${message}" \
         -d "parse_mode=Markdown" >/dev/null 2>&1
}

# SSH Key finding base on fingerprint
find_key_comment() {
    local fingerprint=$1
    local authorized_keys_file=$2
    local temp_file="/tmp/key_check_$(date +%s%N).pub"
    local comment="Key Comment not found"

    if [[ "$fingerprint" != "Not found/password login" ]] && [ -f "$authorized_keys_file" ]; then
        while IFS= read -r key_line || [[ -n "$key_line" ]]; do
            if [[ -z "$key_line" || "$key_line" =~ ^[[:space:]]*# ]]; then
                continue
            fi
            key_part=$(echo "$key_line" | sed -E 's/^(.*)(ssh-(rsa|dss|ed25519|ecdsa-sha2-nistp)[[:space:]]+[^[:space:]]+).*$/\2/')
            if [[ -n "$key_part" ]]; then
                echo "$key_part" > "$temp_file"
                KEY_FILE_FINGERPRINT=$(ssh-keygen -l -E sha256 -f "$temp_file" 2>/dev/null | awk '{print $2}')
                if [[ "$KEY_FILE_FINGERPRINT" == "$fingerprint" ]]; then
                    COMMENT_DATA=$(echo "$key_line" | sed -E 's/.*(ssh-(rsa|dss|ed25519|ecdsa-sha2-nistp)[[:space:]]+[^[:space:]]+)[[:space:]]*(.*)/\3/i')
                    comment="${COMMENT_DATA:-No comment in key file}"
                    break
                fi
            fi
        done < "$authorized_keys_file"
    fi
    [ -f "$temp_file" ] && rm -f "$temp_file"
    echo "$comment"
}

tail -Fn0 "$LOG_FILE" "$SYSLOG_FILE" | while read -r line; do
    
    # 1. Detect SSH login 
    if [[ "$line" =~ Accepted[[:space:]]publickey[[:space:]]for[[:space:]]([a-zA-Z0-9._-]+)[[:space:]]from[[:space:]]([0-9.]+).*SHA256:([A-Za-z0-9+/=]+) ]]; then
        LAST_USER="${BASH_REMATCH[1]}"
        LAST_IP="${BASH_REMATCH[2]}"
        LAST_FINGERPRINT="SHA256:${BASH_REMATCH[3]}"
        SESSION_DETECTED=1 
        continue

    # 2. Look for the Session ID
    elif [[ "$SESSION_DETECTED" -eq 1 ]] && [[ "$line" =~ New[[:space:]]session[[:space:]]([0-9]+)[[:space:]]of[[:space:]]user[[:space:]]($LAST_USER) ]]; then
        session_id="${BASH_REMATCH[1]}"
        server_name=$(hostname)
        user="$LAST_USER"
        ip="$LAST_IP"
        fingerprint="$LAST_FINGERPRINT"

        # Reset flag immediately to allow next detection
        SESSION_DETECTED=0 
        LAST_USER=""
        LAST_IP=""
        LAST_FINGERPRINT=""

        # Check for duplicate alerts using the session ID file
        session_file="$TMP_DIR/${session_id}.log"
        if [[ ! -f "$session_file" ]]; then
            echo "user=$user ip=$ip fingerprint=$fingerprint" > "$session_file"
            key_comment=$(find_key_comment "$fingerprint" "$AUTHORIZED_KEYS")

            msg="🟢 SSH Login Detected 🟢
🖥️ Server: *$server_name*
👤 User: *$user*
🌐 IP: *$ip*
🔐 Logged-In By: *$key_comment*
🆔 Session: *$session_id*
🕒 Time: $(date '+%Y-%m-%d %H:%M:%S')"

            send_telegram "$msg"
        fi
        
    # 3. Detect session failure/timeout
    elif [[ "$SESSION_DETECTED" -eq 1 ]] && [[ "$line" =~ ^(.+\s+)?(error|failed|disconnect) ]]; then
        SESSION_DETECTED=0
        LAST_USER=""
        LAST_IP=""
        LAST_FINGERPRINT=""
    fi
done