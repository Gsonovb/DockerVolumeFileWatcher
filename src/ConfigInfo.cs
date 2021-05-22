using System.Collections.Generic;

namespace FileWatcher
{
    public class ConfigInfo : IConfigInfo
    {
        public ConfigInfo()
        {
            Arguments = new List<string>();
            RootDir = string.Empty;
            RunCommand = string.Empty;
        }

        public string RootDir { get; set; }
        public bool RecurseFlag { get; set; }

        public int IntervalTime { get; set; }

        public string RunCommand { get; set; }
        public List<string> Arguments { get; set; }
    }
}
