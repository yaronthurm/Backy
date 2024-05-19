using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;


namespace BackyLogic
{

    public class StateCalculator2: IStateCalculator
    {
        public event Action OnProgress;

        private IFileSystem _fileSystem;

        public string Target { get; }

        public StateCalculator2(IFileSystem fileSystem, string target, string source, string machineID)
        {
            _fileSystem = fileSystem;
            if (source == null)
                Target = target;
            else
                Target = FindTargetForSource(source, target, fileSystem, machineID);
        }

        private static string FindTargetForSource(string source, string target, IFileSystem fs, string machineID)
        {
            string sourceGuid = FindTargetForSourceOrNull(source, target, fs, machineID);
            if (sourceGuid == null)
               throw new ApplicationException("Could not find directory for source: " + source);
            var ret = Path.Combine(target, sourceGuid);
            return ret;
        }

        private static string FindTargetForSourceOrNull(string source, string target, IFileSystem fs, string machineID)
        {
            string sourceGuid = null;
            var targetDir = fs.GetTopLevelDirectories(target);
            foreach (var innerDir in fs.GetTopLevelDirectories(target))
            {
                if (!BackupDirectory.IsBackupDirectory(innerDir, fs)) continue;
                var backupDir = BackupDirectory.FromPath(innerDir, fs);

                if (backupDir.OriginalSource.Equals(source, StringComparison.OrdinalIgnoreCase) && backupDir.MachineID == machineID)
                {
                    sourceGuid = backupDir.Guid;
                    break;
                }
            }
            if (sourceGuid == null)
                return null;
            var ret = Path.Combine(target, sourceGuid);
            return ret;
        }        

        public int MaxVersion
        {
            get {
                var historyPath = Path.Combine(Target, "History");
                var ret = _fileSystem.GetTopLevelDirectories(historyPath)
                    .Select(x => Path.GetFileName(x))
                    .Select(x => int.Parse(x))
                    .Max();
                return ret;
            }
        }

        public IState GetLastState()
        {
            var path = Path.Combine(Target, "CurrentState");
            var files = _fileSystem.IsDirectoryExist(path) ? _fileSystem.EnumerateFiles(path) : new string[0];

            var ret = new State();
            foreach (var file in files)
            {
                var backy = BackyFile.FromSourceFileName(_fileSystem, file, path);
                ret.AddFile(backy);
                OnProgress?.Invoke();
            }
            return ret;
        }

        public IState GetState(int version)
        {
            if (version > this.MaxVersion)
                throw new ApplicationException("max version exeeded");

            var ret = (State)this.GetLastState();
            if (version == this.MaxVersion)
                return ret;

            var historyPath = Path.Combine(Target, "History");

            for (int i = this.MaxVersion; i > version; i--)
            {
                // Add files that were deleted
                var deletedFolder = Path.Combine(historyPath, i.ToString(), "deleted");
                if (_fileSystem.IsDirectoryExist(deletedFolder))
                {
                    var deletedFiles = _fileSystem.EnumerateFiles(deletedFolder)
                        .Select(x => BackyFile.FromTargetFileName(_fileSystem, x, deletedFolder));
                    foreach (var deletedFile in deletedFiles)
                        ret.AddFile(deletedFile);
                }

                // Update location of files that were modified
                var modifiedFolder = Path.Combine(historyPath, i.ToString(), "modified");
                if (_fileSystem.IsDirectoryExist(modifiedFolder))
                {
                    var modifiedFiles = _fileSystem.EnumerateFiles(modifiedFolder)
                        .Select(x => BackyFile.FromTargetFileName(_fileSystem, x, modifiedFolder));
                    foreach (var modifiedFile in modifiedFiles)
                    {
                        ret.DeleteFileByPath(modifiedFile.RelativeName);
                        ret.AddFile(modifiedFile);
                    }                                                
                }


                // Remove files that were added
                var newPath = Path.Combine(historyPath, i.ToString(), "new.txt");
                if (_fileSystem.IsFileExists(newPath))
                {
                    var newFiles = _fileSystem.ReadLines(newPath).ToList();
                    foreach (var file in newFiles)
                    {
                        ret.DeleteFileByPath(file);
                    }
                }

                // Restore names of files that were renamed
                var renamedPath = Path.Combine(historyPath, i.ToString(), "renamed.txt");
                if (_fileSystem.IsFileExists(renamedPath))
                {
                    var renamedFiles = _fileSystem.ReadLines(renamedPath)
                        .Select(x => JObject.Parse(x))
                        .Select(x => new RenameInfo { OldName = x.Value<string>("oldName"), NewName = x.Value<string>("newName") });
                    foreach (var file in renamedFiles)
                    {
                        var newFile = ret.FindFile(file.NewName);
                        ret.DeleteFile(newFile);
                        var oldFile = newFile.Clone();
                        oldFile.RelativeName = file.OldName;
                        ret.AddFile(oldFile);
                    }
                }
            }

            return ret;
        }

        public IState GetDiff(int version)
        {
            if (version > this.MaxVersion)
                throw new ApplicationException("max version exeeded");

            // Get all files in backup directory
            var ret = new State();
            var rootPath = Path.Combine(Target, "History", version.ToString());
            foreach (var file in _fileSystem.EnumerateFiles(rootPath)) 
            {
                var backyFile = BackyFile.FromTargetFileName(_fileSystem, file, rootPath);
                ret.AddFile(backyFile);
            }
            return ret;
        }

        public DateTime GetDateByVersion(int currentVersion)
        {
            return DateTime.MinValue;
        }
    }
    
}
