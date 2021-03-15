using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FileWatcher
{
    class Program
    {

        static string[] optionPathAlias = new string[] { "--path", "-p" };
        static string[] optionRecurseAlias = new string[] { "--recurse", "-r" };
        static string[] optionTimeAlias = new string[] { "--time", "-t" };
        static string[] optionCommandAlias = new string[] { "--command", "-c" };
        static string[] optionArgumentsAlias = new string[] { "--arguments", "-a" };


        static string RootDir { get; set; }
        static bool RecurseFlag { get; set; }

        static int IntervalTime { get; set; }

        static string RunCommand { get; set; }
        static List<string> Arguments { get; set; }

        static SHA256 HashProvider { get; set; }

        static Dictionary<string, Tuple<long, DateTime>> HashSet { get; set; }

        static int Main(string[] args)
        {

            var rootCommand = new RootCommand
                {
                    new Option<DirectoryInfo>(optionPathAlias,description: "Specify the monitored folder"),
                    new Option<bool>(optionRecurseAlias,"An option to indicate whether to recurse all files in the directory"),
                    new Option<int>(optionTimeAlias,getDefaultValue:() => 60,"An option is used to indicate the interval time in seconds."),
                    new Option<String>(optionCommandAlias,"An option to run the command when a change is detected"),
                    new Option<String[]>(optionArgumentsAlias,"An option indicates the parameters to run the command when changes are detected")
                };

            rootCommand.Description = "A tool for monitoring file modification";

            rootCommand.Handler = CommandHandler.Create<IConsole, CancellationToken, DirectoryInfo, bool, int, String, String[]>(
                async (IConsole console, CancellationToken token, DirectoryInfo path, bool recurse, int time, string command, string[] arguments) =>
                {
#if DEBUG
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
                    Console.WriteLine("");
#endif

                    try
                    {

                        if (null == path || !path.Exists)
                        {
                            Console.WriteLine($"The specified directory does not exist :{path?.FullName}");
                            return 1;
                        }



                        RootDir = path.FullName;
                        RecurseFlag = recurse;
                        IntervalTime = time * 1000;
                        RunCommand = command ?? "";
                        Arguments = new List<string>();

                        if (null != arguments)
                        {
                            Arguments.AddRange(arguments);
                        }


                        HashSet = await GetHashSet(RootDir, RecurseFlag, token);

                        Task task = GetExectingTask(token);


                        Console.WriteLine("Press any key to quit.");
                        Console.ReadLine();

                        return 0;
                    }
                    catch (OperationCanceledException)
                    {
                        console.Error.WriteLine("The operation was aborted");
                        return 1;
                    }
                });


            return rootCommand.InvokeAsync(args).Result;
        }

        private static Task GetExectingTask(CancellationToken token)
        {
            return Task.Run(async () =>
           {
               do
               {
#if DEBUG
                   Console.WriteLine("Start Scan Files.");
#endif                   
                   var hset = await GetHashSet(RootDir, RecurseFlag, token);

                   var cpkeys = new List<string>(HashSet.Keys);

                   var flagHasChange = false;

                   foreach (var kp in hset)
                   {
                       if (token.IsCancellationRequested)
                           token.ThrowIfCancellationRequested();

                       if (HashSet.ContainsKey(kp.Key))
                       {
                           Tuple<long, DateTime> rdata = HashSet[kp.Key];

                           if (rdata.Item1 == kp.Value.Item1 &&
                               rdata.Item2 == kp.Value.Item2)
                           {
                               //not change
                           }
                           else
                           {
                               flagHasChange = true;
                               Console.WriteLine($"File has change: {kp.Key}");
                               HashSet[kp.Key] = kp.Value;
                           }
                           cpkeys.Remove(kp.Key);
                       }
                       else
                       {
                           //new file
                           flagHasChange = true;
                           Console.WriteLine($"New File : {kp.Key}");
                           HashSet.Add(kp.Key, kp.Value);
                       }
                   }

                   if (cpkeys.Any())
                   {
                       foreach (var file in cpkeys)
                       {
                           if (token.IsCancellationRequested)
                               token.ThrowIfCancellationRequested();
                           //Delete file
                           flagHasChange = true;
                           Console.WriteLine($"Delete File : {file}");
                           HashSet.Remove(file);
                       }
                   }

                   if (flagHasChange && !string.IsNullOrEmpty(RunCommand))
                   {
                       var p = System.Diagnostics.Process.Start(RunCommand, Arguments);
                       p.WaitForExit(2000);
                   }

                   if (token.IsCancellationRequested)
                       token.ThrowIfCancellationRequested();

                   Thread.Sleep(IntervalTime);

               } while (!token.IsCancellationRequested);
           }, token);


        }

        private static Task<Dictionary<string, Tuple<long, DateTime>>> GetHashSet(string rootDir, bool recurseFlag, CancellationToken token)
        {

            return Task.Run(() =>
                {
                    Dictionary<string, Tuple<long, DateTime>> dict = new();

                    if (Directory.Exists(rootDir))
                    {
                        try
                        {
                            var dir = new DirectoryInfo(rootDir);
                            var files = dir.EnumerateFiles("*", recurseFlag ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

                            foreach (var file in files)
                            {
                                var filepath = file.FullName;

                                try
                                {
                                    if (!dict.ContainsKey(filepath))
                                    {
                                        var info = Tuple.Create<long, DateTime>(file.Length, file.LastWriteTimeUtc);

                                        dict.Add(filepath, info);
                                    }
                                }
                                catch (FileNotFoundException e)
                                {
                                    Console.WriteLine($"File Not Found : { e.FileName }");
                                }
                                catch (IOException e)
                                {
                                    Console.WriteLine($"I/O Exception: {e.Message}");
                                }
                                catch (UnauthorizedAccessException e)
                                {
                                    Console.WriteLine($"Access Exception: {e.Message}");
                                }
                                catch (System.Exception e)
                                {
                                    Console.WriteLine($"Exception: {e.Message}");
                                }
                            }

                        }
                        catch (PathTooLongException e)
                        {
                            Console.WriteLine($"Path Too Long : {e.Message}");
                        }
                        catch (IOException e)
                        {
                            Console.WriteLine($"I/O Exception: {e.Message}");
                        }
                        catch (UnauthorizedAccessException e)
                        {
                            Console.WriteLine($"Access Exception: {e.Message}");
                        }
                        catch (System.ArgumentException e)
                        {
                            Console.WriteLine($"Argument Exception: {e.Message}");
                        }
                        catch (System.Security.SecurityException e)
                        {
                            Console.WriteLine($"Security Exception: {e.Message}");
                        }
                        catch (System.Exception e)
                        {
                            Console.WriteLine($"Exception: {e.Message}");
                        }
                    }
                    return dict;
                }, token);

        }

        private static string GetStringHash(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return String.Empty;
            }

            if (null == HashProvider)
            {
                HashProvider = SHA256.Create();
            }

            var bytes = Encoding.UTF8.GetBytes(str);

            var hashbytes = HashProvider.ComputeHash(bytes);

            return BitConverter.ToString(hashbytes);

        }


        /// <summary>
        /// Get File Hash 
        /// </summary>
        /// <param name="path">File Path</param>
        /// <returns>
        /// 
        /// If the path does not exist or is null, an empty string is returned
        /// </returns>
        private static string GetFileHash(string path)
        {

            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                return String.Empty;
            }

            if (null == HashProvider)
            {
                HashProvider = SHA256.Create();
            }

            try
            {
                using FileStream fileStream = File.Open(path, FileMode.Open);
                fileStream.Position = 0;
                byte[] hashValue = HashProvider.ComputeHash(fileStream);
                return BitConverter.ToString(hashValue);
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine($"File Not Found : { e.FileName }");
            }
            catch (IOException e)
            {
                Console.WriteLine($"I/O Exception:  {e.Message}");
            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine($"Access Exception: {e.Message}");
            }

            return String.Empty;
        }
    }
}
