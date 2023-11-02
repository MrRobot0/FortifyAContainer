# S7_SecureContainer

## Installation

### Docker compose:
#### Using Docker Socket Proxy (Recommended)
```docker
version: '3.8'
services:
    secure_container:
        image: ghcr.io/mrrobot0/s7_securecontainer:master
        ports:
            - 8080:80
        restart: unless-stopped
        networks:
            - securecontainer
    dockerproxy:
        image: ghcr.io/tecnativa/docker-socket-proxy:latest
        environment:
            - CONTAINERS=1 # Allow access to viewing containers
            - CONFIGS=1 # Allow access to viewing config of containers
            - POST=0 # Disallow any POST operations (effectively read-only)
        volumes:
            - /var/run/docker.sock:/var/run/docker.sock:ro # Mounted as read-only
        restart: unless-stopped
        networks:
            - securecontainer

networks:
  securecontainer:
```

#### Using config.json
When using this option make sure your connection to docker is secure, you can't directly use the docker socket for security reasons.

Create a config.json with the following setting:
```yaml
{
  "DockerHost":  "tcp://exampleConnection:2375"
}
```
Then link the file in the compose file:
```docker
version: '3.8'
services:
    secure_container:
        image: ghcr.io/mrrobot0/s7_securecontainer:master
        ports:
            - 8080:80
        volume:
            - /path/to/your/config.json:/app/config.json
        restart: unless-stopped
```