# 演示
---
[English](README.md) | [简体中文](README.zh-cn.md)


这演示是用来说明程序如何工作，以及Docker文件映射方式无法接收到修改通知。


## 系统需求

- git
- docker
- docker-compose


## 如何开始

1. 打开终端（推荐Linux环境）
2. 将本项目代码克隆到本地

```bash
git clone https://github.com/Gsonovb/DockerVolumeFileWatcher.git
```

2. 转到 demo 文件夹
```
cd DockerVolumeFileWatcher\demo
```

3. 输入以下命令，启动服务
```
docker-compose up
```
4. 使用浏览器打开网址 http://localhost:8080 和  http://localhost:8081 ，并尝试刷新页面内。你会注意到 http://localhost:8081 内容会更新但另一个不会。

5. 转会到终端， 按 <kbd>Ctrl + C</kbd>  停止服务

6. (可选) 打开 `docker-compose.yml` 文件， 将取消注释部分，并保存。 然后再次运行服务，可以在运行日志中看到 `notify_bind` 服务没有检测到文件修改，但`notify_volume`服务却检测到文件更改。

7. 在终端中输入 以下命令，删除服务
```
docker-compose down -v
```
