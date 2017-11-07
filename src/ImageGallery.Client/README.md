## Image Gallery Client

[![This image on DockerHub](https://img.shields.io/docker/pulls/stuartshay/imagegallery-client.svg)](https://hub.docker.com/r/stuartshay/imagegallery-client/)     

### Multi-Build Container

```
cd  NavigatorIdentity/
docker build -f docker/client-multi-build.dockerfile/Dockerfile -t imagegallery-multi-client .
```


### Push Multi-Build Container

```
docker tag <imageid> stuartshay/imagegallery-client:1.1.1-multi   
docker push stuartshay/imagegallery-client:1.1.1-multi
```
