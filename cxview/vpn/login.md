CxViewAdmin@2025

Hướng dẫn truy cập và sử dụng hệ thống platform cxview mới:
- Sử dụng openvpn community edition download cài đặt nếu chưa có: https://openvpn.net/community/
- Import file cấu hình openvpn hcm-cxview-<username>.ovpn
- Username lấy trong tên file phần <username> & Default Password: P@ssw0rd
- Thông tin Jumpbox windows sử dụng rdp để access: 
	+ IP: 172.16.0.13
	+ username: Cùng username VPN
	+ password default: P@ssw0rd
	+ Change password windows: RDP fullscreen / press Ctrl + Alt + End => Change password 
	+ Contact Tiến NV để setup thêm phần mềm trên jumpbox nếu cần 
- Change VPN default password:
	+ mở web browser: http://172.16.1.1 
	+ Login tài khoản vpn default
	+ Change password 
- Hướng dẫn access gitlab:
	+ URL: https://gitlab.cxview.local
	+ Click login với KeyCloak
	+ Username (dùng chung với vpn/rdp)
	+ Password: P@ssw0rd => Change password để login vào gitlab 
	+ Gửi info repos cần access để cấp quyền 

- Chú ý: trên jumpbox truy cập vào xdata.cxview.ai & admin.cxview.ai đang được point vào địa chỉ IP internal của hệ thống mới.


