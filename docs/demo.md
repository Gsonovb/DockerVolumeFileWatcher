  
  
#  Demonstration
  
  
---
  
  
  
##  Explanation
  
  
This is show that mapped file in multiple containers, when one of the containers writes to the file, other containers cannot receive the file modification notification, so they cannot know that the file has changed.
  
Using nginx as an example, the **configuration file** is automatically updated by script.
  
>To make nginx reload the **configuration file**, it is necessary to monitor the file changes through external tools, and execute the reload command after discovering the file modification.
  
  
The services included are as follows
  
| Service name   | port | description                                                                             |
| -------------- | ---- | --------------------------------------------------------------------------------------- |
| Worker         | -    | autogenerate configuration filefile                                                     |
| Web1           | 8080 | web service 1, does not automatically reload the configuration file                     |
| Web2           | 8081 | web service 2,  automatically reloads the configuration file                            |
| notify_ Volume | -    | (default comment) use inotifywait tool to monitor file modification (disk volume mode)  |
| notify_ Bind   | -    | (default comment) use inotifywait tool to monitor file modification (file binding mode) |
---
All containers are bound to the following disk volumes
- File `./nginx/default.conf` To `/etc/nginx/conf.d/default.conf`
- Disk volumes `share` to `/sharedata`
  
  
##  System requirements
  
- git
- docker
- docker-compose
  
##  How to start
  
  
1. Open the terminal (Linux is recommended)
2. Clone repository to local ```git clone  https://github.com/Gsonovb/DockerVolumeFileWatcher.git```
3. Go to the demo folder ```cd DockerVolumeFileWatcher/demo```
4. Enter ```docker-compose up``` command to start the service
5. Use the browser to open the web address http://localhost:8080 and http://localhost:8081 ,then try to refresh the page. You'll notice http://localhost:8081 content will be updated, but another will not.
6. Back to the terminal and press <KBD>Ctrl+C</KBD> to stop the service
7. (optional) open `docker-compose.yml` file,Uncomment section and save. Then run the service again, and you can see `notify_bind` service did not detect a file modification, but `notify_volume` service detected a file change.
8. Enter ```docker-compose down -v``` command to delete the service
  