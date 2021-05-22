# Docker Volume File Watcher

[![cicr](https://github.com/Gsonovb/DockerVolumeFileWatcher/actions/workflows/build-ci-cr.yml/badge.svg)](https://github.com/Gsonovb/DockerVolumeFileWatcher/actions/workflows/build-ci-cr.yml)

---

[English](README.md) | [简体中文](README.zh-cn.md)



这是用来监控Docker卷映射文件修改的工具。

通过卷映射文件在多个容器中共享文件，但如果某个容器修改了文件内容，其他的容器并不会收到通知（文件修改事件）。
此工具通过定时检查文件属性，来确定文件是否修改。


## 帮助文档

你可以访问这里查看[帮助文档](https://gsonovb.github.io/DockerVolumeFileWatcher)。


## 使用方法

要使用本工具需要构建Docker镜像，实现启动容器时运行程序。
具体过程请参考[如何使用](./docs/zh-cn/howtouse.md)。


## 演示

演示程序说明本工具的主要功能。请参考[运行演示](./docs/zh-cn/demo.md)。

## 构建

要从源代码编译本项目请参考[如何编译](./docs/zh-cn/howtobuild.md)。

## 贡献

有关对该项目做出贡献的信息，请参见[CONTRIBUTING](CONTRIBUTING.zh-cn.md)。

该项目采用了[贡献者契约](http://contributor-covenant.org/)定义的行为准则。


## 许可

此项目使用 [MIT license](LICENSE).

