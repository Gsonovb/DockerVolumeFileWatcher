# Docker Volume File Watcher

---

这是用来监控Docker卷映射文件修改的工具。

通过卷映射文件在多个容器中共享文件，但如果某个容器修改了文件内容，其他的容器并不会收到通知（文件修改事件）。
此工具通过定时检查文件属性，来确定文件是否修改。

