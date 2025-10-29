# Step 1: Install and Set Permissions for the Script

First, move the script to a standard location and ensure it is executable by the necessary users (usually root, as PAM executes sessions under elevated privileges).

Create the script file: If you are copying the content into a new file on the server:

sudo mkdir -p /usr/local/bin/
sudo nano /usr/local/bin/ssh_alert.sh

Make it executable:

sudo chmod +x /usr/local/bin/ssh_alert.sh


# Step 2: Configure PAM for SSHD

You need to tell the sshd process to run this script whenever a new session is established. This is done in the PAM configuration file for SSH.

Edit the PAM configuration file:

sudo nano /etc/pam.d/sshd


Add the Execution Line: Add the following line to the bottom of the file (or near the end of the existing session stack), before any lines that start with @include (though placing it at the very bottom often works fine):

Add this line:

session optional pam_exec.so /usr/local/bin/ssh_alert.sh


session: Defines that this module runs during session setup/teardown.

optional: Ensures that if the script fails (e.g., if curl isn't installed or the Telegram API is down), the user's login session is not denied. This is crucial for security stability.

pam_exec.so: The module responsible for executing an external command.

/usr/local/bin/ssh_alert.sh: The path to your script.

# Step 3: Test and Verification

Restart SSHD (Recommended, but sometimes not required by PAM):

sudo systemctl restart sshd
