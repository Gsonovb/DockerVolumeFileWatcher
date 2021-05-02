  
  
#  如何使用
  
---
  
  
> 此工具是辅助工具，通常需要跟其他程序同时启动。
  
这里使用nginx演示如何使用本工具。
  
> nginx 是一个流行的Web服务端程序
> 当配置文件或者其他依赖文件（比如证书文件）更改时，需要重新加载配置才能生效。
  
  
##  自定义镜像
  
  
  
要在现有镜像中添加自动启动程序，需要自定义镜像。
本例中使用[nginx官方提供容器镜像](https://hub.docker.com/_/nginx )
可以在这里查看不同标记对应的Dockerfile文件。
  
这里使用最新的镜像标记`nginx:latest`，你可以根据自己需要替换其他镜像。
  
  
通过查看[Dockerfile文件](https://github.com/nginxinc/docker-nginx/blob/464886ab21ebe4b036ceb36d7557bf491f6d9320/mainline/debian/Dockerfile ) 最末尾的 `CMD` 字段，来确定镜像默认启动程序。你可以在自定义的Dockerfile中使用这个命令。
  
```Dockerfile
  
ENTRYPOINT ["/docker-entrypoint.sh"]
  
EXPOSE 80
  
STOPSIGNAL SIGQUIT
  
CMD ["nginx", "-g", "daemon off;"]
```
  
  
自定义镜像可以通过以下两种方法来制作
  
###  使用脚本
  
  
> 本过程的代码文件在 `usecase\install` 文件夹下
  
安装脚本可以自动下载最新的程序包并设置权限。
> 注：安装脚本 需要 bash 环境
  
1. 新建一个工作文件夹
2. 创建 `docker-entrypoint.sh` 文件，用作容器的执行入口
```sh
#!/bin/bash
###########
  
sh -c "filewatcher &"
  
exec "$@"
  
```  
> 说明：这里通过在命令末尾添加 `&` 方式，使程序在后台运行。
> 同时第六行代码用来继续执行之后的传入的命令
  
3. 创建 `Dockerfile` 文件
  
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
  
| 代码行 | 说明                                                                                                                                         |
| ------ | -------------------------------------------------------------------------------------------------------------------------------------------- |
| 1      | `FROM nginx:latest ` 表示使用哪个镜像作为基础镜像， 这里用 `nginx:latest` ，你可以替换成其他需要的镜像                                       |
| 4      | 使用用来安装 脚本需要 curl 和 bash。 这里被注释了，是因为镜像中已经包含了。 如果你不确定，可以启动一个容器实例并附加Shell 查看镜像内部情况。 |
| 8-9    | 添加 `docker-entrypoint.sh`文件并设置执行权限                                                                                                |
| 11-15  | 设置filewatcher环境变量用作默认配置                                                                                                          |
| 17     | 设置容器启动入口，这里使用自定义的`docker-entrypoint.sh`文件                                                                                 |
| 19     | 设置容器启动命令，这个命令可以从官方的Dockerfile中获取到                                                                                     |
  
  
4. 使用`docker build -t <tagname> .` 命令来生成镜像文件（将`<tagname>`替换成需要的名字） 
> 注意：可以添加 `--no-cache `  参数可以不是缓存内容来生成镜像  `docker build -t <tagname> --no-cache . `
如果生成镜像遇到问题，请根据错误提示检查Dockerfile文件
  
5. 使用`docker run -it --rm -p 8080:80 <tagname> `（将`<tagname>`替换成上一步中的名字）命令来测试镜像是否能正常工作.
> 建议测试以下生成镜像，以确保程序可以正常运行。
执行完命令后，使用浏览里访问 本地 8080端口，确认服务是否正常工作。测试完后停止并删除容器。
  
  
###  从源代码或每日构建
  
  
> 本过程的代码文件在 `usecase\manual` 文件夹下
  
如果你需要从源代码或者预发布版本来创建镜像时候，可以使用下面方法来生成镜像
  
1. 新建一个工作文件夹
2. 将FileWatcher工具复制到文件夹，可以从以下两个地方获取
- 从GitHub上获取最新的发布， https://github.com/Gsonovb/DockerVolumeFileWatcher/releases/latest
- 参考[如何编译] 生成发布文件
  
3. 创建 `docker-entrypoint.sh` 文件，内容如下
```sh
#!/bin/bash
###########
  
sh -c "filewatcher &"
  
exec "$@"
  
```  
  
4. 创建 `Dockerfile` 文件
  
> 注意：程序运行需要一些系统组件，根据[依赖项](https://docs.microsoft.com/zh-cn/dotnet/core/install/linux-scripted-manual#dependencies )来手动添加
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
  
  
5. 使用`docker build -t <tagname> .` 命令来生成镜像文件（将`<tagname>`替换成需要的名字） 
> 注意：可以添加 `--no-cache `  参数可以不是缓存内容来生成镜像  `docker build -t <tagname> --no-cache . `
如果生成镜像遇到问题，请根据错误提示检查Dockerfile文件
  
6. 使用`docker run -it --rm -p 8080:80 <tagname> `（将`<tagname>`替换成上一步中的名字）命令来测试镜像是否能正常工作.
> 建议测试以下生成镜像，以确保程序可以正常运行。
执行完命令后，使用浏览里访问 本地 8080端口，确认服务是否正常工作。测试完后停止并删除容器。
  
  
  
##  运行自定义镜像
  
  
  
此工具主要是配合 docker-compose 方式来使用的
> 如果使用 `docker run` 方式运行，这需要指定 `-v` 参数来挂在文件， 可能还需要指定 `-e` 参数来设置环境变量
  
在yaml 配置中，可以配置服务的共享卷和配置参数。
可以指定`build`字段来使用特定dockerfile文件生成镜像
  
通过使用 `docker-compose build .`命令来生成需要镜像
> 注意：通过docker-compose生成镜像会自动根据服务名称来命令镜像名称
  
使用  `docker-compose run -d` 命令来运行服务编排
使用  `docker-compose down`  命令来删除服务编排
  
  
  
  
Docker-compose 示例
---
  
以下内容是简单Docker-compose配置示例
  
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
  