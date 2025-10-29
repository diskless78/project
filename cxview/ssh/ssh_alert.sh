#!/bin/bash

# --- CONFIGURATION ---
TELEGRAM_BOT_TOKEN="8457927004:AAEsyZQfJAztHXvkA830j4kLQYINVJ7ee_s"
TELEGRAM_CHAT_ID="-5013802399"
BASE_URL="https://api.telegram.org/bot${TELEGRAM_BOT_TOKEN}/sendMessage"
LOG_FILE="/var/log/auth.log"

# Get current date and time
LOGIN_TIME=$(date '+%Y-%m-%d %H:%M:%S %Z')

if [ -z "$PAM_USER" ] || [ -z "$PAM_RHOST" ]; then
    echo "ERROR: PAM environment variables not available. Script aborted." | logger -t ssh_alert
    exit 1
fi

USER_NAME="$PAM_USER"
REMOTE_IP="$PAM_RHOST"
SERVER_NAME=$(hostname)
USER_HOME=$(getent passwd "$USER_NAME" | cut -d: -f6)
AUTHORIZED_KEYS_PATH="$USER_HOME/.ssh/authorized_keys"

if [ ! -z "$SSH_CONNECTION" ]; then
    CONNECTION_DETAILS=$(echo "$SSH_CONNECTION" | awk '{print $1":"$2 " -> " $3":"$4}')
else
    CONNECTION_DETAILS="Unknown Ports"
fi

find_key_comment() {
    local fingerprint=$1
    local user=$2
    local authorized_keys_file="$3"
    local temp_file="/tmp/key_check_$(date +%s%N).pub"
    local comment="Comment not found"
    
    if [[ "$fingerprint" != "Not found/password login" ]] && [ -f "$authorized_keys_file" ]; then
        if ! [ -r "$authorized_keys_file" ]; then
            comment="Access denied to $authorized_keys_file"
            echo "$comment"
            return
        fi

        while IFS= read -r key_line || [[ -n "$key_line" ]]; do
            # Skip empty lines and comments
            if [[ -z "$key_line" || "$key_line" =~ ^[[:space:]]*# ]]; then
                continue
            fi

            KEY_STRING=$(echo "$key_line" | awk '{print $1, $2}')
            echo "$KEY_STRING" > "$temp_file"
            
            KEY_FILE_FINGERPRINT=$(ssh-keygen -l -E sha256 -f "$temp_file" 2>/dev/null | awk '{print $2}')

            # If fingerprints match, extract comment
            if [[ "$KEY_FILE_FINGERPRINT" == "$fingerprint" ]]; then
                COMMENT_DATA=$(echo "$key_line" | awk '{$1=$2=$3=""; sub(/^ * /, ""); print}')
                if [ -z "$COMMENT_DATA" ]; then
                    comment="No comment in key file"
                else
                    comment="$COMMENT_DATA"
                fi
                break
            fi
        done < "$authorized_keys_file"
    elif [[ "$fingerprint" == "Not found/password login" ]]; then
        comment="N/A (Password/Log not found)"
    else
        comment="Key file not found: $authorized_keys_file"
    fi

    echo "$comment"
}

sleep 1

KEY_FINGERPRINT="Not found/password login"
FINGERPRINT_LINE=""

if command -v journalctl &> /dev/null; then
    FINGERPRINT_LINE=$(journalctl _SYSTEMD_UNIT=ssh.service \
        --since "1 minute ago" \
        -o cat | grep "Accepted publickey for ${USER_NAME} from ${REMOTE_IP}" | tail -n 1)
elif [ -f "$LOG_FILE" ]; then
    FINGERPRINT_LINE=$(tail -n 50 "$LOG_FILE" | grep "Accepted publickey for $USER_NAME from $REMOTE_IP" | tail -n 1)
fi

if [ ! -z "$FINGERPRINT_LINE" ]; then
    if [[ "$FINGERPRINT_LINE" =~ 'key fingerprint' ]]; then
        KEY_FINGERPRINT=$(echo "$FINGERPRINT_LINE" | sed -E 's/.*(SHA256:[^[:space:]]+).*/\1/')
    elif [[ "$FINGERPRINT_LINE" =~ 'RSA' || "$FINGERPRINT_LINE" =~ 'ED25519' ]]; then
        KEY_FINGERPRINT=$(echo "$FINGERPRINT_LINE" | sed -E 's/.*(SHA256:[^[:space:]]+).*/\1/')
    fi
fi

# --- KEY COMMENT LOOKUP ---
KEY_COMMENT=$(find_key_comment "$KEY_FINGERPRINT" "$USER_NAME" "$AUTHORIZED_KEYS_PATH")

# --- AUTH LOG PRE-SEND MESSAGE ---
echo "SSH ALERT: Server=$SERVER_NAME, User=$USER_NAME, IP=$REMOTE_IP, KeyComment=$KEY_COMMENT, FP=$KEY_FINGERPRINT" | logger -t ssh_alert

escape_markdown() {
    local text="$1"
    text="${text//./\\.}"
    text="${text//-/\\-}"
    text="${text//_/\\_}"
    text="${text//(/\\(}"
    text="${text//)/\\)}"
    text="${text//\*/\\*}"
    text="${text//\`/\\\`}"
    text="${text//\#/\\#}"
    text="${text//+/\\+}"
    text="${text//\{/\\{}"
    text="${text//\}/\\}}"
    text="${text//=/\\=}"
    text="${text//|/\\|}"
    text="${text//!/\\!}"
    text="${text//>/\\>}"
    text="${text//\&/\\&}"
    echo "$text"
}

ESCAPED_USER=$(escape_markdown "$USER_NAME")
ESCAPED_REMOTE_IP=$(escape_markdown "$REMOTE_IP")
ESCAPED_SERVER_NAME=$(escape_markdown "$SERVER_NAME")
ESCAPED_LOGIN_TIME=$(escape_markdown "$LOGIN_TIME")
ESCAPED_CONN_DETAILS=$(escape_markdown "$CONNECTION_DETAILS")
ESCAPED_FINGERPRINT=$(escape_markdown "$KEY_FINGERPRINT") 
ESCAPED_KEY_COMMENT=$(escape_markdown "$KEY_COMMENT") 

MESSAGE="🚨 *SSH Login Alert* 🚨

*Server*: ${ESCAPED_SERVER_NAME}
*User*: \`${ESCAPED_USER}\`
*IP Address*: ${ESCAPED_REMOTE_IP}
*Time (UTC)*: ${ESCAPED_LOGIN_TIME}

\`Connection\`: ${ESCAPED_CONN_DETAILS}
*Key Fingerprint*: \`${ESCAPED_FINGERPRINT}\`
*Key Comment*: \`${ESCAPED_KEY_COMMENT}\`"

RESPONSE=$(curl -s -X POST "${BASE_URL}" \
     -d chat_id="${TELEGRAM_CHAT_ID}" \
     -d parse_mode="MarkdownV2" \
     --data-urlencode "text=${MESSAGE}")

if echo "$RESPONSE" | grep -q '"ok":true'; then
    echo "SSH login alert sent successfully for user $USER_NAME from $REMOTE_IP (Key: $KEY_FINGERPRINT)." | logger -t ssh_alert
else
    echo "Failed to send SSH login alert for user $USER_NAME. Response: $RESPONSE" | logger -t ssh_alert
fi

exit 0
