  
#  帮助
  
  
你可以通过以下命令来使用此工具
  
```bash
filewatcher  [--path|-p] <path>  [-r|--recurse] [[-t|--time] <time>] [[-c|--command] <command>]
 [[-a|--arguments] <arguments>]
```
  
##  选项说明
  
  
> 可以使用环境变量设置默认配置，环境变量名不区分大小写
> 注意：通过命令行选项指定配置优先级更高。
  
  
| 选项参数                    | 环境变量名            | 参数类型 | 默认值 | 说明                                 |
| --------------------------- | --------------------- | -------- | ------ | ------------------------------------ |
| -p, --path <path>           | filewatcher_path      | 字符串   | ''     | （必须） 监视的文件夹路径            |
| -r, --recurse               | filewatcher_recurse   | 布尔     | false  | 是否递归子文件夹                     |
| -t, --time <time>           | filewatcher_time      | 整数     | 60     | 用于指示检查文件的间隔时间，单位秒   |
| -c, --command <command>     | filewatcher_command   | 字符串   | ''     | 当检测到文件修改后，要执行的命令     |
| -a, --arguments <arguments> | filewatcher_arguments | 字符串   | ''     | 当检测到文件修改后，要执行的命令参数 |
| --version                   |                       |          |        | 显示版本信息                         |
| -?, -h, --help              |                       |          |        | 显示使用帮助                         |
##  环境变量
  
  
此工具支持在环境变量配置，方便在Docker环境中使用。 
  
参考上边的表格中的环境变量名称来设置环境变量。
  
> 如果使用环境变量进行配置，可以不指定运行参数直接运行
> 注意：通过命令行选项指定配置优先级更高。
  
  