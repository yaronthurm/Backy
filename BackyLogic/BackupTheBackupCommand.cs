using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BackyLogic
{
    public class BackupTheBackupCommand
    {
        private string _source;
        private string _target;
        private IFileSystem _fileSystem;
        public IMultiStepProgress Progress;
        private CancellationToken _cancellationToken;

        public BackupTheBackupCommand(IFileSystem fileSystem, string source, string target, CancellationToken cancellationToken = new CancellationToken())
        {
            _fileSystem = fileSystem;
            _source = source;
            _target = target;
            _cancellationToken = cancellationToken;
        }


        public void Execute()
        {
            var sw = Stopwatch.StartNew();
            try {
                this.Progress?.StartStepWithoutProgress($"\nStarted backing up the backup at: { DateTime.Now }");
            }
            finally
            {
                sw.Stop();
                this.Progress?.StartStepWithoutProgress($"Finished backing up the backup'{_source}' at: { DateTime.Now }");
                this.Progress?.StartStepWithoutProgress("Total time: " + sw.Elapsed);
            }
        }
    }
}
