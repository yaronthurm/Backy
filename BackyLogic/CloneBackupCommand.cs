using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace BackyLogic
{
    public class CloneBackupCommand
    {
        private string _source;
        private string _target;
        private IFileSystem _fileSystem;
        private bool _abort;
        public IMultiStepProgress Progress;

        public CloneBackupCommand(IFileSystem fileSystem, string source, string target)
        {
            _fileSystem = fileSystem;
            _source = source;
            _target = target;
        }


        public void Execute()
        {
            var sw = Stopwatch.StartNew();
            try
            {
                this.Progress?.StartStepWithoutProgress("Started: " + DateTime.Now);
            }
            finally
            {
                sw.Stop();
                this.Progress?.StartStepWithoutProgress("Finished: " + DateTime.Now);
                this.Progress?.StartStepWithoutProgress("Total time: " + sw.Elapsed);
            }
        }
    }
}