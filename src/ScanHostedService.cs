
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FileWatcher
{
    public class ScanHostedService : BackgroundService
    {

        private SHA256 HashProvider { get; set; }

        private Dictionary<string, Tuple<long, DateTime>> HashSet { get; set; }


        private IConfigInfo _configInfo;

        private readonly ILogger<ScanHostedService> _logger;


        public ScanHostedService(ILogger<ScanHostedService> logger, IConfigInfo config)
        {
            _logger = logger;
            _configInfo = config;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

#if DEBUG

            StringBuilder sb = new();


            foreach (string argitem in _configInfo.Arguments)
            {
                sb.AppendFormat(" {0}  ", argitem);
            }


            _logger.LogDebug($"Start monitoring directory :{_configInfo.RootDir}");

            _logger.LogDebug($"  Recurse     : {_configInfo.RecurseFlag}  IntervalTime: {_configInfo.IntervalTime / 1000 }  RunCommand  : {_configInfo.RunCommand}   Arguments   : {sb.ToString()} ");


#endif

            _logger.LogInformation("Scan Service running.");


            HashSet = await GetHashSet(_configInfo.RootDir, _configInfo.RecurseFlag, stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {

                _logger.LogInformation("Start Scan Files at: {time:yyyy-MM-dd hh:mm:ss}", DateTimeOffset.Now);


                var hset = await GetHashSet(_configInfo.RootDir, _configInfo.RecurseFlag, stoppingToken);

                var cpkeys = new List<string>(HashSet.Keys);

                var flagHasChange = false;

                foreach (var kp in hset)
                {
                    if (stoppingToken.IsCancellationRequested)
                    {
                        stoppingToken.ThrowIfCancellationRequested();
                    }

                    if (HashSet.ContainsKey(kp.Key))
                    {
                        Tuple<long, DateTime> rdata = HashSet[kp.Key];

                        if (rdata.Item1 == kp.Value.Item1 && rdata.Item2 == kp.Value.Item2)
                        {
                            //not change
                        }
                        else
                        {
                            flagHasChange = true;
                            _logger.LogInformation($"File has change: {kp.Key}");
                            HashSet[kp.Key] = kp.Value;
                        }
                        cpkeys.Remove(kp.Key);
                    }
                    else
                    {
                        //new file
                        flagHasChange = true;
                        _logger.LogInformation($"New File : {kp.Key}");
                        HashSet.Add(kp.Key, kp.Value);
                    }
                }

                if (cpkeys.Any())
                {
                    foreach (var file in cpkeys)
                    {
                        if (stoppingToken.IsCancellationRequested)
                        {
                            stoppingToken.ThrowIfCancellationRequested();
                        }
                        //Delete file
                        flagHasChange = true;
                        _logger.LogInformation($"Delete File : {file}");
                        HashSet.Remove(file);
                    }
                }

                if (flagHasChange && !string.IsNullOrEmpty(_configInfo.RunCommand))
                {
                    var p = System.Diagnostics.Process.Start(_configInfo.RunCommand, _configInfo.Arguments);
                    p.WaitForExit(5000);
                }

                if (stoppingToken.IsCancellationRequested)
                {
                    stoppingToken.ThrowIfCancellationRequested();
                }

                await Task.Delay(_configInfo.IntervalTime, stoppingToken);
            }
        }



        private Task<Dictionary<string, Tuple<long, DateTime>>> GetHashSet(string rootDir, bool recurseFlag, CancellationToken token)
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
                                    _logger.LogError($"File Not Found : { e.FileName }");
                                }
                                catch (IOException e)
                                {
                                    _logger.LogError($"I/O Exception: {e.Message}");
                                }
                                catch (UnauthorizedAccessException e)
                                {
                                    _logger.LogError($"Access Exception: {e.Message}");
                                }
                                catch (System.Exception e)
                                {
                                    _logger.LogError($"Exception: {e.Message}");
                                }
                            }

                        }
                        catch (PathTooLongException e)
                        {
                            _logger.LogError($"Path Too Long : {e.Message}");
                        }
                        catch (IOException e)
                        {
                            _logger.LogError($"I/O Exception: {e.Message}");
                        }
                        catch (UnauthorizedAccessException e)
                        {
                            _logger.LogError($"Access Exception: {e.Message}");
                        }
                        catch (System.ArgumentException e)
                        {
                            _logger.LogError($"Argument Exception: {e.Message}");
                        }
                        catch (System.Security.SecurityException e)
                        {
                            _logger.LogError($"Security Exception: {e.Message}");
                        }
                        catch (System.Exception e)
                        {
                            _logger.LogError($"Exception: {e.Message}");
                        }
                    }
                    return dict;
                }, token);
        }


        /// <summary>
        /// Get File Hash 
        /// </summary>
        /// <param name="path">File Path</param>
        /// <returns>
        /// 
        /// If the path does not exist or is null, an empty string is returned
        /// </returns>
        private string GetFileHash(string path)
        {

            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                return string.Empty;
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
                _logger.LogError($"File Not Found : { e.FileName }");
            }
            catch (IOException e)
            {
                _logger.LogError($"I/O Exception:  {e.Message}");
            }
            catch (UnauthorizedAccessException e)
            {
                _logger.LogError($"Access Exception: {e.Message}");
            }

            return string.Empty;
        }
    }
}
