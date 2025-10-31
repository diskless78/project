# Login Alert /etc/systemd/system/ssh_login_monitor.service

[Unit]
Description=SSH Login Alert Monitor
After=network.target

[Service]
Type=simple
ExecStart=/usr/local/bin/ssh_login_monitor
Restart=always
StandardOutput=journal

[Install]
WantedBy=multi-user.target


systemctl daemon-reload
systemctl enable ssh_login_monitor.service
systemctl start ssh_login_monitor.service


# Logout Alert by create the file /etc/systemd/system/ssh_logout_monitor.service

[Unit]
Description=SSH Logout Monitor Service
After=network.target

[Service]
Type=simple
User=root
ExecStart=/bin/bash  /usr/local/bin/ssh_logout_monitor
Restart=on-failure

[Install]
WantedBy=multi-user.target


# Enable and start the service
systemctl daemon-reload
systemctl enable ssh_logout_monitor.service
systemctl start ssh_logout_monitor.service