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
        restart: unless-stopped
    dockerproxy:
        image: ghcr.io/tecnativa/docker-socket-proxy:latest
        environment:
            - CONTAINERS=1 # Allow access to viewing containers
            - CONFIGS=1 # Allow access to viewing config of containers
            - POST=0 # Disallow any POST operations (effectively read-only)
        volumes:
            - /var/run/docker.sock:/var/run/docker.sock:ro # Mounted as read-only
        restart: unless-stopped
```
