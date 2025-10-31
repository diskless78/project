dpkg -l | grep nvidia-driver-535

nvidia-driver-535                    535.274.02-0ubuntu0.24.04.2             amd64        NVIDIA driver metapackage

sudo apt-mark hold nvidia-driver-535

sudo apt-mark showholds


