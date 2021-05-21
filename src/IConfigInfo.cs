using System.Collections.Generic;

namespace FileWatcher
{
    public interface IConfigInfo
    {
        string RootDir { get; set; }
        bool RecurseFlag { get; set; }

        int IntervalTime { get; set; }

        string RunCommand { get; set; }
        List<string> Arguments { get; set; }
    }
}
