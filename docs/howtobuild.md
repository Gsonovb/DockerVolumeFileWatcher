  
#  How to build
  
  
This article will show you how to compile the code in this project.
  
##  System requirements
  
- [.net 5 sdk](https://dotnet.microsoft.com/download )
- [VS 2019](https://visualstudio.microsoft.com/ ) OR [VS Code](https://code.visualstudio.com/ )
- Linux
  - [Docker Engine](https://docs.docker.com/engine/install/ )
  - [Docker Compose](https://docs.docker.com/compose/install/ )
- Window
  - [Docker Desktop](https://docs.docker.com/docker-for-windows/install/ )
  
  
  
##  Steps
  
  
1. Use the 'git clone' command to clone the project locally.
2. Use vs code to open project folder or vs 2019 to open ` doc kerVolumeFileWatcher.sln `Project.
>Note:  The project file contains the docker compose project, so docker desktop needs to be installed in the system.
3. Modify project code file `src\Program.cs` .
4. Modify startup parameters
   1. VS Code 
   Edit `.vscode\launch.json` File, change the parameter options in "args"
   2. VS2019
   Edit `src\Properties\launchSettings.json` File, change the parameter options in "commandlineargs"
5. Press the <kbd>F5</kbd> key to start the project and test it.
  
  
  
  
##  publish
  
  
Use the following command to generate an executable file for the production environment
  
```
dotnet publish -c Release -r linux-x64 -p:PublishSingleFile=true --self-contained false
```
  
`-R` is the specified target OS and CPU type. For details, please refer to [. Net rid directory][1]
  
For more information about publishing and commands, refer to the following
- [.Net rid directory][1]
- [Single file deployment and executable][2]
- [.Net application release overview][3]
  
[1]:  https://docs.microsoft.com/en-us/dotnet/core/rid-catalog
[2]:  https://docs.microsoft.com/en-us/dotnet/core/deploying/single-file
[3]:  https://docs.microsoft.com/en-us/dotnet/core/deploying
  