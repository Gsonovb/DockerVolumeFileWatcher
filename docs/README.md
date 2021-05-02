# Docker Volume File Watcher


---


This tool is used to monitor the modification of docker volume mapping file.
A volume mapping file is used to share files multiple containers.
However, if one container modifies the contents of the file, other containers will not receive notification.
This tool checks the file attributes to determine whether the file is modified.
