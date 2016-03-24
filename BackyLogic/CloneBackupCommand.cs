using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace BackyLogic
{
    public class CloneBackupCommand
    {
        private string _cloneSource;
        private string _cloneTarget;
        private int _version;
        private IFileSystem _fileSystem;
        private bool _abort;
        public IMultiStepProgress Progress;

        public CloneBackupCommand(IFileSystem fileSystem, string cloneSource, string cloneTarget, int version)
        {
            _fileSystem = fileSystem;
            _cloneSource = cloneSource;
            _cloneTarget = cloneTarget;
            _version = version;
        }


        public void Execute()
        {
            var sw = Stopwatch.StartNew();
            try
            {
                this.Progress?.StartStepWithoutProgress("Started: " + DateTime.Now);
                var stateCalculator = new TransientState(_fileSystem, _cloneSource);
                if (stateCalculator.MaxVersion < _version)
                    throw new ApplicationException("Invalid veriosn. max is " + stateCalculator.MaxVersion);

                State cloneState = stateCalculator.GetState(_version);
                foreach (var file in cloneState.GetFiles())
                {
                    var destName = Path.Combine(_cloneTarget, file.RelativeName);
                    _fileSystem.Copy(file.PhysicalPath, destName);
                }
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