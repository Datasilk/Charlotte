# Charlotte's Web Router
#### A load balancer for handling requests to multiple instances of Charlotte (running in docker)

To build a Docker image, open a command window in Charlotte's root project folder
```
docker build -f Router\Dockerfile --force-rm -t charlotte-router .	
```

To run in a Docker container:
```
docker run -dit --rm -p 7007:80 -e "ASPNETCORE_ENVIRONMENT=Production" -e "ASPNETCORE_URLS=http://+:80" --name charlottes-web-router charlotte-router
```