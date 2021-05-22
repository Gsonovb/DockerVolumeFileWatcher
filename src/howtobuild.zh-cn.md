---
markdown:
  image_dir: /docs/assets
  path: /docs/zh-cn/howtobuild.md
  ignore_from_front_matter: true
  absolute_image_path: true
export_on_save:
  markdown: true
---
# 如何编译


本文将指导你如何编译此项目中的代码。


## 系统需求

- [.net 5 sdk](https://dotnet.microsoft.com/download)
- [VS 2019](https://visualstudio.microsoft.com/) OR [VS Code](https://code.visualstudio.com/)
- Linux
  - [Docker Engine](https://docs.docker.com/engine/install/)
  - [Docker Compose](https://docs.docker.com/compose/install/)
- Window
  - [Docker Desktop](https://docs.docker.com/docker-for-windows/install/)


## 编译步骤


1. 使用 `git clone` 命令将项目克隆到本地。
2. 使用 VS CODE 打开项目文件夹 或者 使用VS 2019 打开 `DockerVolumeFileWatcher.sln` 项目文件。
> 注意：`DockerVolumeFileWatcher.sln` 项目文件中包含docker-compose编排项目，因此需要系统中安装Docker Desktop。
3. 修改项目代码文件`src\Program.cs` 。
4. 修改启动参数
   1. VS Code 
       编辑 `.vscode\launch.json` 文件， 更改 "args" 中的参数选项

   2. VS2019
       编辑 `src\Properties\launchSettings.json` 文件， 更改 "commandLineArgs" 中的参数选项

5. 按 <kbd>F5</kbd> 键  ，启动项目并测试项目。



## 生成发布


使用以下命令生成用于生产环境的可执行文件

```
dotnet publish -c Release -r linux-x64 -p:PublishSingleFile=true --self-contained false
```
其中 `-r` 是 指定目标 OS 和 CPU 类型 ，具体可以参考 [.NET RID 目录][1]

有关发布和命令的详细信息请参考以下内容

- [.NET RID 目录][1]
- [单个文件部署和可执行文件][2]
- [.NET 应用程序发布概述][3]

[1]: https://docs.microsoft.com/zh-cn/dotnet/core/rid-catalog
[2]: https://docs.microsoft.com/zh-cn/dotnet/core/deploying/single-file
[3]: https://docs.microsoft.com/zh-cn/dotnet/core/deploying