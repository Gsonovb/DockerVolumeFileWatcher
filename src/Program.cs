using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.IO;

namespace FileWatcher
{
    internal class Program
    {
        private const string STR_ConsoleLine = "================================================================";
        private static string[] optionPathAlias = new string[] { "--path", "-p" };
        private static string[] optionRecurseAlias = new string[] { "--recurse", "-r" };
        private static string[] optionTimeAlias = new string[] { "--time", "-t" };
        private static string[] optionCommandAlias = new string[] { "--command", "-c" };
        private static string[] optionArgumentsAlias = new string[] { "--arguments", "-a" };

        public static int Main(string[] args)
        {

            var rootCommand = new RootCommand
                {
                    new Option<DirectoryInfo>(optionPathAlias,description: "Specify the monitored folder"),
                    new Option<bool>(optionRecurseAlias,"An option to indicate whether to recurse all files in the directory"),
                    new Option<int>(optionTimeAlias,getDefaultValue:() => 60,"An option is used to indicate the interval time in seconds."),
                    new Option<string>(optionCommandAlias,"An option to run the command when a change is detected"),
                    new Option<string[]>(optionArgumentsAlias,"An option indicates the parameters to run the command when changes are detected")
                };

            rootCommand.Description = "A tool for monitoring file modification";

            rootCommand.Handler = CommandHandler.Create<DirectoryInfo, bool, int, String, String[]>(
                 (DirectoryInfo path, bool recurse, int time, string command, string[] arguments) =>
                 {
#if DEBUG

                     WriteTitleLine("Debug Info");

                     Console.Write($"Option value for --path: {path?.FullName ?? "null"}  , --recurse: {recurse} ，--time:{time}, --command: {command ?? "null"} , --arguments: ");

                     if (null != arguments && arguments.Length > 0)
                     {
                         foreach (string argitem in arguments)
                         {
                             Console.Write(argitem);
                             Console.Write(" ");
                         }
                     }
                     else
                     {
                         Console.Write("null");
                     }

                     Console.WriteLine();
#endif



                     DirectoryInfo tpath = path;
                     bool trecurse = recurse;
                     int ttime = time;
                     string tcommand = command;
                     string[] targuments = arguments;

                     try
                     {
#if DEBUG
                         Console.WriteLine();
                         WriteTitleLine("System Info");
                         Console.WriteLine("Environment Info:");
#endif
                         var envars = Environment.GetEnvironmentVariables();

                         foreach (DictionaryEntry item in envars)
                         {
                             var name = item.Key.ToString().ToLower();
                             var value = item.Value as string;
#if DEBUG
                             Console.WriteLine($"   {name}:{value}");
#endif

                             if (string.Equals(name, "FileWatcher_path", StringComparison.InvariantCultureIgnoreCase))
                             {
                                 if (null == path && !string.IsNullOrEmpty(value) && Directory.Exists(value))
                                 {
                                     tpath = new DirectoryInfo(value);
                                 }
                             }
                             else if (string.Equals(name, "FileWatcher_recurse", StringComparison.InvariantCultureIgnoreCase))
                             {
                                 if (bool.TryParse(value, out trecurse))
                                 {
                                 }
                                 else
                                 {
                                     trecurse = recurse;
                                 }
                             }
                             else if (string.Equals(name, "FileWatcher_time", StringComparison.InvariantCultureIgnoreCase))
                             {
                                 if (int.TryParse(value, out ttime))
                                 {

                                 }
                                 else
                                 {
                                     ttime = time;
                                 }
                             }
                             else if (string.Equals(name, "FileWatcher_command", StringComparison.InvariantCultureIgnoreCase))
                             {
                                 if (string.IsNullOrEmpty(tcommand) && !string.IsNullOrEmpty(value))
                                 {
                                     tcommand = value;
                                 }
                             }
                             else if (string.Equals(name, "FileWatcher_arguments", StringComparison.InvariantCultureIgnoreCase))
                             {
                                 if ((null == targuments || targuments.Length == 0) && !string.IsNullOrEmpty(value))
                                 {
                                     targuments = value.Split(",", StringSplitOptions.RemoveEmptyEntries);

                                 }
                             }
                         }
                     }
                     catch (Exception ex)
                     {
                         Console.Error.WriteLine($"Can't Get Environment Variables : {ex.Message}");
                     }

                     try
                     {

#if DEBUG
                         Console.WriteLine();
#endif

                         WriteTitleLine("Application parameters");

                         Console.WriteLine($"  Directory :{tpath?.FullName}");
                         Console.WriteLine($"  Recurse     : {trecurse}");
                         Console.WriteLine($"  IntervalTime: {ttime}");
                         Console.WriteLine($"  RunCommand  : {tcommand}");
                         Console.Write($"  Arguments   : ");

                         if (null != targuments)
                         {
                             foreach (string argitem in targuments)
                             {
                                 Console.Write(argitem);
                                 Console.Write(" ");
                             }
                         }

                         Console.WriteLine();
                         Console.WriteLine();


                         if (null == tpath || !tpath.Exists)
                         {
                             Console.WriteLine($"The specified directory does not exist :{tpath?.FullName}");
                             return 1;
                         }

                         ConfigInfo ci = new ConfigInfo
                         {
                             RootDir = tpath.FullName,
                             RecurseFlag = trecurse,
                             IntervalTime = ttime * 1000,
                             RunCommand = tcommand ?? ""
                         };

                         if (null != targuments)
                         {
                             ci.Arguments.AddRange(targuments);
                         }

                         CreateHostBuilder(ci).Build().Run();
                         return 0;
                     }
                     catch (OperationCanceledException)
                     {
                         Console.WriteLine("The operation was aborted");
                         return 1;
                     }
                 });

            return rootCommand.InvokeAsync(args).Result;
        }

        /// <summary>
        /// Output Title  to Console
        /// </summary>
        /// <param name="name"></param>
        private static void WriteTitleLine(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                Console.WriteLine(STR_ConsoleLine);
            }
            else
            {
                Console.WriteLine(STR_ConsoleLine);


                var len = STR_ConsoleLine.Length - name.Length;
                var title = name;

                if (len > 0)
                {
                    len = len / 2;
                    title = title.PadLeft(len + name.Length);
                }

                Console.WriteLine(title);
                Console.WriteLine(STR_ConsoleLine);
            }
        }

        public static IHostBuilder CreateHostBuilder(IConfigInfo config)
        {
            return Host.CreateDefaultBuilder()
                    .UseContentRoot(config.RootDir)
                    .ConfigureServices((hostContext, services) =>
                    {
                        services.AddSingleton<IConfigInfo>(config);
                        services.AddHostedService<ScanHostedService>();
                    });
        }
    }
}
