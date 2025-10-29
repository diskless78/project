 CxViewAdmin@2025

docker exec -it 70432df22d0b redis-cli -a rediscxview123 --scan > ./keys.txt
cat keys.txt

iftop
nvidia-smi

git@gitlab.cxview.local:platform/cluster/prod.git
mongosh "mongodb://mongodb.cxview.local:27017/cxview" -u "minh.pham" -p --authenticationDatabase "cxview"

mongodb://minh.pham:8rj2SkarBaZ5@mongodb.cxview.local:27017/?authSource=cxview
mongodb://minh.pham:8rj2SkarBaZ5@mongodb.cxview.local:27017

.\mongosh "mongodb://minh.pham:8rj2SkarBaZ5@mongodb.cxview.local:27017/cxview?authSource=cxview"
.\mongosh "mongodb://minh.pham:8rj2SkarBaZ5@mongodb.cxview.local:27017"

db.createUser(
   {
      user: "minh.pham",
      pwd: passwordPrompt(),
      roles: [
         { role: "root", db: "admin" }
      ]
   }
)


ssh-keygen -t ed25519 -C "marcus.duong@cxview.ai"

docker exec -it ff8519f3d61e sh

systemctl list-units --type=service --all

 systemctl status k3s-server
 systemctl status k3s-api
 systemctl status k3s-central