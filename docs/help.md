  
#  Help
  
  
Use the following command to run
  
```bash
filewatcher  [--path|-p] <path>  [-r|--recurse] [[-t|--time] <time>] [[-c|--command] <command>]
 [[-a|--arguments] <arguments>]
```
  
##  Option description
  
  
>You can use environment variables to set the default configuration, Environment variable names are not case sensitive
>Note: the command line option specifies that the configuration priority is higher.
  
| Option                      | Environmental variables name | type   | Default value | Explanation                                                              |
| --------------------------- | ---------------------------- | ------ | ------------- | ------------------------------------------------------------------------ |
| -p, --path <path>           | filewatcher_path             | string | ''            | (required) monitored folder path                                         |
| -r, --recurse               | filewatcher_recurse          | bool   | false         | Recursive subfolder                                                      |
| -t, --time <time>           | filewatcher_time             | int    | 60            | Used to indicate the time interval between checking files, in seconds    |
| -c, --command <command>     | filewatcher_command          | string | ''            | The command to execute when a file modification is detected              |
| -a, --arguments <arguments> | filewatcher_arguments        | string | ''            | The command parameters to be executed when file modification is detected |
| --version                   |                              |        |               | Display version information                                              |
| -?, -h, --help              |                              |        |               | Display usage help                                                       |
##  Environmental variables
  
  
This tool supports the configuration of environment variables, which is convenient to use in the docker environment.
  
Refer to the table above to set environment variables.
  
>If you use environment variables, you can run without specifying parameters
>Note: the command line option specifies that the configuration priority is higher.
  