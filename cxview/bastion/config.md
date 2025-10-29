# Jump Host - create config
Host bastion
    HostName 172.16.0.13
    User thanh.duong

# 172.16.*
Host 172.16.*
    ProxyJump bastion
