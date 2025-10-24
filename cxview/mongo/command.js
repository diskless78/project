db.createUser(
   {
     user: "minh.pham",
     pwd: passwordPrompt(),
     roles: [
        { role: "dbOwner", db: "cxview" }
     ]
   }
)


db.createUser(
   {
     user: "len.vo",
     pwd: passwordPrompt(),
     roles: [
        { role: "dbOwner", db: "cxview" }
     ]
   }
)


db.createUser(
   {
     user: "len.vo",
     pwd: passwordPrompt(),
     roles: [
        { role: "readWrite", db: "cxview" }
     ]
   }
)
