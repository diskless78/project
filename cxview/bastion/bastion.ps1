# Install OpenSSH Server on Windows Bastion Host: 172.16.0.13

Add-WindowsCapability -Online -Name OpenSSH.Server~~~~0.0.1.0
Start-Service sshd
Set-Service -Name sshd -StartupType Automatic
New-NetFirewallRule -Name sshd -DisplayName "OpenSSH Server (sshd)" -Enabled True -Direction Inbound -Action Allow -Protocol TCP -LocalPort 22


ssh -J thanh.duong@172.16.0.13 root@172.16.0.23

# Jump Host - create config
Host bastion
    HostName 172.16.0.13
    User thanh.duong

# 172.16.*
Host 172.16.*
    ProxyJump bastion

ssh root@172.16.0.23
