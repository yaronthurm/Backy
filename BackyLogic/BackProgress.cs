using System;
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
        public int RenameDetectionTotal { get; internal set; }
        public int RenameDetectionFinish { get; internal set; }

        public int SourceFileScanned { get; internal set; }
        public int TargetFileScanned { get; internal set; }

        public bool Done()
        {
            var ret = CalculateDiffFinished &&
                NewFilesFinished == NewFilesTotal &&
                DeletedFilesFinished == DeletedFilesTotal &&
                ModifiedFilesFinished == ModifiedFilesTotal &&
                RenamedFilesFinished == RenamedFilesTotal;
            return ret;
        }
    }


    public class DiffProgress
    {
        public int RenameDetectionFinished { get; internal set; }
        public int RenameDetectionTotal { get; internal set; }
    }
}