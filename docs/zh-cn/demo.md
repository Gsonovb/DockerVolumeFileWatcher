  
  
#  演示
  
---
  
  
  
  
  
##  说明
  
  
这个演示是用来说明在多个容器中映射同一个文件后，当其中某个容器对文件进行写入操作后，其他容器无法收取到文件修改通知，从而无法得知文件已经改变。
  
这里使用nginx作为示例，通过脚本自动更新**配置文件**来改变输出内容。
  
>要让nginx重新加载**配置文件**需要通过外部工具来监听文件更改，并在发现文件修改后，执行 重新加载命令。
  
包含的服务如下
  
| 服务名        | 端口 | 说明                                                       |
| ------------- | ---- | ---------------------------------------------------------- |
| worker        | -    | 用来自动更新配置文件                                       |
| web1          | 8080 | web服务1，不会自动重载配置文件                             |
| web2          | 8081 | web服务2，自定义镜像通过此工具自动重载配置文件             |
| notify_volume | -    | (默认注释) 使用inotifywait工具监视文件修改（磁盘卷方式）   |
| notify_bind   | -    | (默认注释) 使用inotifywait工具监视文件修改（文件绑定方式） |
---
所有容器都绑定如下磁盘卷
- 文件`./nginx/default.conf`到`/etc/nginx/conf.d/default.conf`
- 磁盘卷`share`到`/sharedata`
  
  
##  系统需求
  
  
要运行演示程序需要系统中安装以下组件
  
- git
- docker
- docker-compose
  
  
##  如何开始
  
  
1. 打开终端
2. 克隆仓库  `git clone https://github.com/Gsonovb/DockerVolumeFileWatcher.git`
3. 转到 demo 文件夹 ```cd DockerVolumeFileWatcher\demo```
4. 输入命令 ```docker-compose up``` ，启动服务
5. 使用浏览器打开网址 http://localhost:8080 和 http://localhost:8081 ，并尝试刷新页面内。你会注意到 http://localhost:8081 内容会更新但另一个不会更新。
6. 回到终端， 按 <kbd>Ctrl + C</kbd>  停止服务
7. (可选) 打开 `docker-compose.yml` 文件， 取消注释部分并保存。 然后再次运行服务，可以在运行日志中看到 `notify_bind` 服务没有检测到文件修改，但`notify_volume`服务却检测到文件更改。
8. 输入 ```docker-compose down -v``` 命令，删除服务
  