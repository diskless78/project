#!/bin/bash


# === Telegram Config ===
BOT_TOKEN="8457927004:AAEsyZQfJAztHXvkA830j4kLQYINVJ7ee_s"
CHAT_ID="-1003262590904"

# === File Paths ===
AUTHORIZED_KEYS="/root/.ssh/authorized_keys"
LOG_FILE="/var/log/auth.log"
LOGOUT_SOURCE="/var/log/auth.log"
TMP_DIR="/tmp/ssh_sessions"

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

# Function to extract data from the session log file
get_session_data() {
    local session_id=$1
    local session_file="$TMP_DIR/${session_id}.log"
    
    if [ ! -f "$session_file" ]; then
        echo "status=ERROR message='Session file $session_file not found or incomplete.'"
        return 1
    fi

    # Read the log file content
    local content=$(cat "$session_file")

    # Extract user, ip, and fingerprint
    local user=$(echo "$content" | grep -oP 'user=\K[^[:space:]]+')
    local ip=$(echo "$content" | grep -oP 'ip=\K[^[:space:]]+')
    local fingerprint=$(echo "$content" | grep -oP 'fingerprint=\K[^[:space:]]+')
    
    if [[ -z "$user" || -z "$ip" ]]; then
        echo "status=ERROR message='Failed to parse user or IP from session file.'"
        return 1
    fi

    # Get key comment using the fingerprint
    local key_comment=$(find_key_comment "$fingerprint" "$AUTHORIZED_KEYS")

    echo "status=OK user=$user ip=$ip fingerprint=$fingerprint comment='$key_comment'"
    return 0
}


# === Main Monitor Loop ===

tail -Fn0 "$LOGOUT_SOURCE" | while read -r line; do
    
    if [[ "$line" =~ Removed[[:space:]]session[[:space:]]([0-9]+) ]]; then
        session_id="${BASH_REMATCH[1]}"
        server_name=$(hostname)

        # Get session details from the session file
        session_data=$(get_session_data "$session_id")
        status=$(echo "$session_data" | grep -oP 'status=\K[^[:space:]]+')

        if [ "$status" == "OK" ]; then
            user=$(echo "$session_data" | grep -oP 'user=\K[^[:space:]]+')
            ip=$(echo "$session_data" | grep -oP 'ip=\K[^[:space:]]+')
            key_comment=$(echo "$session_data" | grep -oP "comment='(.*?)'" | sed "s/comment='//; s/'$//")

            msg="🟢 SSH Logout Detected 🟢
🖥️ Server: *$server_name*
👤 User: *$user*
🌐 IP: *$ip*
🔐 Logged-Out By: *$key_comment*
🆔 Session: *$session_id*
🕒 Time: $(date '+%Y-%m-%d %H:%M:%S')"

            send_telegram "$msg"
            rm -f "$TMP_DIR/${session_id}.log"
            
        else
            error_msg=$(echo "$session_data" | grep -oP "message='(.*?)'" | sed "s/message='//; s/'$//")
            msg="🟢 SSH Logout Detected (Data Missing) 🟢
🖥️ Server: *$server_name*
🆔 Session: *$session_id*
⚠️ Error: $error_msg
🕒 Time: $(date '+%Y-%m-%d %H:%M:%S')"

            send_telegram "$msg"
        fi
    fi
done
