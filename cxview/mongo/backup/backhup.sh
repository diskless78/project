#!/bin/bash
BACKUP_DIR="/backups/mongodb_$(date +%F)"
ARCHIVE="/backups/mongodb_$(date +%F).gz"
LOGFILE="/backups/mongo_backup.log"

export MONGO_URI="mongodb://admin:StrongPass123@localhost:27017/admin"

echo "$(date '+%F %T') Starting MongoDB backup..." >> $LOGFILE
mongodump --uri="$MONGO_URI" --gzip --archive="$ARCHIVE"

if [ $? -eq 0 ]; then
  echo "$(date '+%F %T') Backup completed successfully." >> $LOGFILE
  mongorestore --dryRun --gzip --archive="$ARCHIVE" >/dev/null 2>&1
  if [ $? -eq 0 ]; then
    echo "$(date '+%F %T') Backup verified successfully." >> $LOGFILE
  else
    echo "$(date '+%F %T') Backup verification failed!" >> $LOGFILE
  fi
else
  echo "$(date '+%F %T') Backup failed!" >> $LOGFILE
fi
