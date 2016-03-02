using System.Collections.Generic;

namespace BackyLogic
{
    public class BackyProgress
    {
        public List<string> Failed = new List<string>();

        public bool CalculateDiffFinished { get; internal set; }
        public bool NoChangeDetected { get; internal set; }

        public int NewFilesFinished { get; internal set; }
        public int NewFilesTotal { get; internal set; }

        public int DeletedFilesTotal { get; internal set; }
        public int DeletedFilesFinished { get; internal set; }

        public int ModifiedFilesTotal { get; internal set; }
        public int ModifiedFilesFinished { get; internal set; }

        public int RenamedFilesTotal { get; internal set; }
        public int RenamedFilesFinished { get; internal set; }
    }
}