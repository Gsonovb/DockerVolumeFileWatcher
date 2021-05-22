# Docker Volume File Watcher

[![cicr](https://github.com/Gsonovb/DockerVolumeFileWatcher/actions/workflows/build-ci-cr.yml/badge.svg)](https://github.com/Gsonovb/DockerVolumeFileWatcher/actions/workflows/build-ci-cr.yml)

---

[English](README.md) | [简体中文](README.zh-cn.md)



This tool is used to monitor the modification of docker volume mapping file.
A volume mapping file is used to share files multiple containers.
However, if one container modifies the contents of the file, other containers will not receive notification.
This tool checks the file attributes to determine whether the file is modified.

## How to Using

To use this tool, you need to build a docker image to run the program when starting the container.
Please refer to [how to use](./docs/howtouse.md).

## Building

To compile this project from source code, please refer to [how to compile](./docs/howtobuild.md).


## Demonstration

The demo program shows the main functions of the tool. Please refer to [run demo](./docs/demo.md).


## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for information on contributing to this project.

This project has adopted the code of conduct defined by the [Contributor Covenant](http://contributor-covenant.org/) to clarify expected behavior in our community.

## License

This project is licensed with the [MIT license](LICENSE).

