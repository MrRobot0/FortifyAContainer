# S7_SecureContainer

## Installation

Docker compose
```docker
version: '3.8'
services:
    secure_container:
        image: ghcr.io/mrrobot0/s7_securecontainer:master
        ports:
            - 8080:80
        volumes:
            - /var/run/docker.sock:/var/run/docker.sock
        restart: unless-stopped
```
