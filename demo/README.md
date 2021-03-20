# Demonstration

---
[English](README.md) | [简体中文](README.zh-cn.md)


This demo is used to show how the program works, and the docker file mapping method cannot receive modification notification.


## System requirements
- git
- docker
- docker-compose


## How to start

1. Open the terminal (Linux is recommended)

2. Clone the code of this project to local
```bash
git clone  https://github.com/Gsonovb/DockerVolumeFileWatcher.git
```

2. Go to the demo folder
```
cd DockerVolumeFileWatcher\demo
```

3. Enter the following command to start the service
```
docker-compose up
```

4. Use the browser to open the web address http://localhost:8080 and http://localhost:8081 and try to refresh the page. You'll notice http://localhost:8081 content will be updated, but another will not.

5. Transfer to the terminal and press <KBD>Ctrl+C</KBD> to stop the service

6. (optional) open `docker-compose.yml` File, the comment section will be cancelled and saved. Then run the service again, and you can see `notify_bind` service  did not detect a file modification, but `notify_volume` service detected a file change.

7. Enter the following command in the terminal to delete the service
```
docker-compose down -v
```