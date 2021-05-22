  
  
#  How to use
  
---
  
This article uses nginx to demonstrate how to use this tool.
  
>Nginx is a popular web server program
  
##  Custom image
  
  
To add an autostart to an existing image, you need to customize the image.
  
This example uses [nginx officially container image](https://hub.docker.com/_/nginx )
You can view the dockerfile file here.
  
The latest image tag is used here `nginx:latest` , you can replace other images according to your own needs.
  
View [dockerfile file](https://github.com/nginxinc/docker-nginx/blob/464886ab21ebe4b036ceb36d7557bf491f6d9320/mainline/debian/Dockerfile ) The 'CMD' field at the end determines the default startup program of the image. You can use this command in the custom dockerfile.
  
  
```Dockerfile
  
ENTRYPOINT ["/docker-entrypoint.sh"]
  
EXPOSE 80
  
STOPSIGNAL SIGQUIT
  
CMD ["nginx", "-g", "daemon off;"]
```
  
  
There are two ways to create a custom image
  
###  Using installation scripts
  
  
> The code file for this procedure is in the folder `usecase\install` 
  
The installation script can automatically download the latest packages and set permissions.
>**Note**: the installation script needs bash environment
  
2. Create a new working folder
3. Create a `docker-entrypoint.sh` file to be used as the execution entry of the container
```sh
#!/bin/bash
###########
  
sh -c "filewatcher &"
  
exec "$@"
  
```  
>**Note**: here by adding '&' at the end of the command, the program runs in the background.
>At the same time, the sixth line of code is used to continue the subsequent incoming command
  
3. Create a `Dockerfile` file
```Dockerfile
FROM nginx:latest
  
#Remove the following line comments to install the curl and Bash tools you need
#RUN apt-get update && apt-get install -y apt-transport-https ca-certificates curl bash && apt-get clean
  
RUN (curl -L https://raw.githubusercontent.com/Gsonovb/DockerVolumefilewatcher/main/setup/install.sh)|bash
  
COPY ./docker-entrypoint.sh /usr/bin/docker-entrypoint.sh
RUN chmod +x /usr/bin/docker-entrypoint.sh
  
ENV filewatcher_path='/etc/nginx/conf.d'
ENV filewatcher_recurse=false
ENV filewatcher_time=60
ENV filewatcher_command='nginx'
ENV filewatcher_arguments='-s,reload'
  
ENTRYPOINT [ "/usr/bin/docker-entrypoint.sh" ]
  
CMD ["nginx", "-g", "daemon off;"]
  
```  
  
| Code line | description                                                                                                                                                                                                                          |
| --------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| 1         | `FROM nginx:latest ` It indicates which image to use as the base image `nginx:latest` You can replace it with other images you need                                                                                                  |
| 4         | curl and Bash are required to install scripts. It's annotated here because it's already included in the image. If you are not sure, you can start a container instance and attach a shell to see the internal situation of the image |
| 8-9       | add `docker-entrypoint.sh` file and set execution permission                                                                                                                                                                         |
| 11-15     | set the filewatcher environment variable as the default configuration                                                                                                                                                                |
| 17        | set the container startup entry, where the user-defined `docker-entrypoint.sh` file is used                                                                                                                                          |
| 19        | set the container startup command, which can be obtained from the official dockerfile                                                                                                                                                |
  
4. Use the `docker build -t <tagname> .` command to generate the image file (replace the `<tagname>` with the desired name)
>Note: the `--no-cache ` parameter can be added instead of the cache content to generate the image  `docker build -t <tagname> --no-cache . `
If you encounter problems in image generation, please check the dockerfile file according to the error prompt
  
5. Use the command `docker run -it --rm -p 8080:80 <tagname> `(replace `<tagname>` with the name in the previous step) to test whether the image works properly
>It is recommended to test the following generated images to ensure that the program can run normally.
After executing the command, use browse to access the local port 8080 to confirm whether the service works normally. Stop and delete the container after testing.
  
  
  
###  Build from source or daily
  
> The code file for this procedure is in the folder `usecase\manual` 
  
If you need to create an image from source code or pre release version, you can use the following method to generate an image
  
1. Create a new working folder
2. Copy the filewatcher tool to the folder, which can be obtained from the following two places
- Get the latest releases from GitHub, https://github.com/Gsonovb/DockerVolumeFileWatcher/releases/latest
- Refer to [How to build] to generate a release file
3. Create the `docker-entrypoint.sh` file
```sh
#!/bin/bash
###########
  
sh -c "filewatcher &"
  
exec "$@"
  
```  
4. Create a `Dockerfile` file
>**Note**: the program needs some system components to run, according to the [dependency](https://docs.microsoft.com/en-us/dotnet/core/install/linux-scripted-manual#dependencies )To add it manually
  
```Dockerfile
FROM nginx:latest
  
#Remove the following line comments to install the curl and Bash tools you need
RUN apt-get update && apt-get install -y --no-install-recommends apt-transport-https ca-certificates bash libc6 libgcc1 libgssapi-krb5-2 libicu63 libssl1.1 libstdc++6 zlib1g && apt-get clean
  
  
COPY ./filewatcher /usr/bin/filewatcher
RUN chmod +x /usr/bin/filewatcher
  
COPY ./docker-entrypoint.sh /usr/bin/docker-entrypoint.sh
RUN chmod +x /usr/bin/docker-entrypoint.sh
  
ENV filewatcher_path='/etc/nginx/conf.d'
ENV filewatcher_recurse=false
ENV filewatcher_time=60
ENV filewatcher_command='nginx'
ENV filewatcher_arguments='-s,reload'
  
  
ENTRYPOINT [ "/usr/bin/docker-entrypoint.sh" ]
  
CMD ["nginx", "-g", "daemon off;"]
  
```  
  
5. Use the `docker build -t <tagname> .` command to generate the image file (replace the `<tagname>` with the desired name)
>Note: the `--no-cache ` parameter can be added instead of the cache content to generate the image  `docker build -t <tagname> --no-cache . `
If you encounter problems in image generation, please check the dockerfile file according to the error prompt
  
6. Use the command `docker run -it --rm -p 8080:80 <tagname> `(replace `<tagname>` with the name in the previous step) to test whether the image works properly
>It is recommended to test the following generated images to ensure that the program can run normally.
After executing the command, use browse to access the local port 8080 to confirm whether the service works normally. Stop and delete the container after testing.
  
  
##  Run custom image
  
  
This tool is mainly used with docker compose mode
  
> If you use the `docker run` mode to run, you need to specify the `-v` parameter to hang on the file. You may also need to specify the `-e` parameter to set the environment variable
> 
In yaml configuration, the shared volume and configuration parameters of the service can be configured.
You can specify the `build` field to generate images using specific dockerfile files
  
Use the `docker-compose build .` command to generate the required image
>**Note**: image generation through docker compose will automatically command the image name according to the service name
  
Use the `docker-compose run -d` command to run service choreography
Use the `docker-compose down` command to delete the service choreography
  
  
  
  
Docker compose example
---
The following is an example of a simple docker compose configuration
  
```yaml
version: "3.4"
  
services:
  web1:
    restart: always
    image: nginx
    ports:
      - 8080:80
    volumes:
      - "./nginx/default.conf:/etc/nginx/conf.d/default.conf"
  
  web2:
    restart: always
    build:
      context: ./install
    environment:
      - filewatcher_time=15
    ports:
      - 8080:80
    volumes:
      - "./nginx/default.conf:/etc/nginx/conf.d/default.conf"
  
```  
  