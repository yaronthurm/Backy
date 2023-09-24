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

                var diff = BackupTheBackupDiff.Calculate(_source, _target, _fileSystem);

                CopyMissingSources(diff.Missing);
                CopyMissingDirectoriesForExistingSources(diff.Existing);
            }
            finally
            {
                sw.Stop();
                this.Progress?.StartStepWithoutProgress($"Finished backing up the backup'{_source}' at: { DateTime.Now }");
                this.Progress?.StartStepWithoutProgress("Total time: " + sw.Elapsed);
            }
        }


        private void CopyMissingDirectoriesForExistingSources(IEnumerable<BackupTheBackupDiff.BackupDiff> existingSources)
        {
            foreach (var existingSource in existingSources.Where(x => x.MissingDirectories.Any()))
            {
                if (DirtyFlagExists(existingSource.Directory.Guid))
                    CleanupDirtyDirectory(existingSource.Directory.Guid);

                var missingDirectories = existingSource.MissingDirectories;
                foreach (var missingDir in GetNumericalOrdered(missingDirectories))
                {
                    var dirToCopy = Path.Combine(_source, existingSource.Directory.Guid, missingDir);
                    var destination = Path.Combine(_target, existingSource.Directory.Guid, missingDir);

                    SetDirtyFlag(existingSource.Directory.Guid, destination);
                    CopyEntireDirectory(dirToCopy, destination);
                    AdjustCreateTime(dirToCopy, destination);
                    MakeReadOnly(destination);
                    ClearDirtyFlag(existingSource.Directory.Guid);
                }
            }
        }

        private List<string> GetNumericalOrdered(List<string> source)
        {
            var ret = source.Select(int.Parse).OrderBy(x => x).Select(x => x.ToString());
            return ret.ToList();
        }

        private void CleanupDirtyDirectory(string guid)
        {
            var dirtyFileName = Path.Combine(_target, guid, "dirty.txt");
            var dirtyDirectoryFullPath = _fileSystem.ReadLines(dirtyFileName).First();

            this.Progress?.StartStepWithoutProgress($"Found dirty directory. Performing cleanup");
            if (_fileSystem.IsDirectoryExist(dirtyDirectoryFullPath))
            {
                _fileSystem.MarkDirectoryAsFullControl(dirtyDirectoryFullPath);
                _fileSystem.DeleteDirectory(dirtyDirectoryFullPath);
            }
            _fileSystem.DeleteFile(dirtyFileName);
            this.Progress?.StartStepWithoutProgress($"Cleanup completed succesfully");
        }

        private bool DirtyFlagExists(string guid)
        {
            return _fileSystem.FindFile(Path.Combine(_target, guid), "dirty.txt") != null;
        }
        private void ClearDirtyFlag(string guid)
        {
            var dirtyFilePath = Path.Combine(_target, guid, "dirty.txt");
            _fileSystem.DeleteFile(dirtyFilePath);
        }

        private void SetDirtyFlag(string guid, string destination)
        {
            var dirtyFilePath = Path.Combine(_target, guid, "dirty.txt");
            _fileSystem.CreateFile(dirtyFilePath);
            _fileSystem.AppendLines(dirtyFilePath, destination);
        }

        
        private void AdjustCreateTime(string dirToCopy, string destination)
        {
            _fileSystem.SetCreateTime(destination, _fileSystem.GetCreateTime(dirToCopy));
        }

        private void CopyMissingSources(IEnumerable<BackupDirectory> missingSources)
        {
            foreach (var missingSource in missingSources)
            {
                var dirToCopy = Path.Combine(_source, missingSource.Guid);
                var destination = Path.Combine(_target, missingSource.Guid);
                CopyEntireDirectory(dirToCopy, destination);
                foreach (var innerDirectory in _fileSystem.GetTopLevelDirectories(destination))
                    MakeReadOnly(innerDirectory);                
            }
        }
        
        private void CopyEntireDirectory(string dirToCopy, string destination)
        {
            var filesToCopy = _fileSystem.EnumerateFiles(dirToCopy).ToArray();
            if (filesToCopy.Any())
                this.Progress?.StartBoundedStep($"Copy files from {dirToCopy}", filesToCopy.Length);
            foreach (var file in filesToCopy)
            {
                var destinationFileName = Path.Combine(destination, file.Replace(dirToCopy + "\\", ""));
                _fileSystem.Copy(file, destinationFileName);
                this.Progress?.Increment();
            }
        }

        private void MakeReadOnly(string targetDir)
        {
            _fileSystem.MakeDirectoryReadOnly(targetDir);
        }
    }

    public class BackupTheBackupDiff
    {
        public List<BackupDirectory> Missing;
        public List<BackupDiff> Existing;

        public class BackupDiff
        {
            public BackupDirectory Directory;
            public List<string> MissingDirectories;
        }

        public static BackupTheBackupDiff Calculate(string source, string target, IFileSystem fs)
        {
            var ret = new BackupTheBackupDiff();
            ret.Missing = FindMissingSources(source, target, fs).ToList();
            ret.Existing = FindExistingSources(source, target, fs).ToList();
            return ret;
        }
        

        private static IEnumerable<BackupDiff> FindExistingSources(string source, string target, IFileSystem fs)
        {
            var sourcesInSource = FindSources(source, fs);
            var sourcesInTarget = FindSources(target, fs);
            var existingSources = sourcesInSource.Intersect(sourcesInTarget, new GenericEqualityComparer<BackupDirectory>(x => x.Guid));

            foreach (var existingSource in existingSources)
            {
                var sourcePath = Path.Combine(source, existingSource.Guid);
                var targeyPath = Path.Combine(target, existingSource.Guid);
                var missingDontent = FindMissingDirectories(sourcePath, targeyPath, fs);
                var ret = new BackupDiff();
                ret.Directory = existingSource;
                ret.MissingDirectories = missingDontent.ToList();
                yield return ret;                
            }
        }

        private static IEnumerable<BackupDirectory> FindSources(string path, IFileSystem fs)
        {
            var ret = fs.GetTopLevelDirectories(path)
                .Where(x => BackupDirectory.IsBackupDirectory(x, fs))
                .Select(x => BackupDirectory.FromPath(x, fs));
            return ret;
        }

        private static IEnumerable<string> FindMissingDirectories(string source, string target, IFileSystem fs)
        {
            var dirsInSource = fs.GetTopLevelDirectories(source).Select(x => Path.GetFileName(x));
            var dirsInTarget = fs.GetTopLevelDirectories(target).Select(x => Path.GetFileName(x));
            var dirtyDirectories = FindDirtyDirectories(target, fs).Select(x => Path.GetFileName(x));
            var ret = dirsInSource.Except(dirsInTarget.Except(dirtyDirectories));
            return ret;
        }

        private static IEnumerable<string> FindDirtyDirectories(string target, IFileSystem fs)
        {
            var dirtyFile = fs.FindFile(target, "dirty.txt");
            if (dirtyFile == null) yield break;

            var ret = fs.ReadLines(dirtyFile).First();
            yield return ret;
        }

        private static IEnumerable<BackupDirectory> FindMissingSources(string source, string target, IFileSystem fs)
        {
            var dirsInSource = FindSources(source, fs);
            var dirsInTarget = FindSources(target, fs);
            var ret = dirsInSource.Except(dirsInTarget, new GenericEqualityComparer<BackupDirectory>(x => x.Guid));
            return ret;
        }
    }    


    public class BackupDirectory
    {
        private const string IniFileName = "backy.ini";

        public string FullPath;
        public string Guid;
        public string OriginalSource;
        public string MachineID;
        public string BackupMode;

        public static BackupDirectory FromPath(string path, IFileSystem fs)
        {
            var ret = new BackupDirectory();
            var iniFile = fs.FindFile(path, IniFileName);
            var iniLines = fs.ReadLines(iniFile).ToArray();
            ret.FullPath = path;
            ret.OriginalSource = iniLines.First();
            ret.Guid = iniLines.Skip(1).First();
            ret.MachineID = iniLines.Skip(2).First();
            ret.BackupMode = iniLines.Skip(3).FirstOrDefault() ?? "diff";
            return ret;
        }

        public static bool IsBackupDirectory(string path, IFileSystem fs)
        {
            // Should contain a Backy.ini file
            var ret = fs.FindFile(path, IniFileName) != null;
            return ret;
        }

        public static void CreateIniFile(string sourceGuid, string target, string source, IFileSystem fs, string machineID, string backupMode)
        {
            fs.CreateFile(Path.Combine(target, sourceGuid, IniFileName));
            fs.AppendLines(Path.Combine(target, sourceGuid, IniFileName), source);
            fs.AppendLines(Path.Combine(target, sourceGuid, IniFileName), sourceGuid);
            fs.AppendLines(Path.Combine(target, sourceGuid, IniFileName), machineID);
            fs.AppendLines(Path.Combine(target, sourceGuid, IniFileName), backupMode);
        }
    }


    public class GenericEqualityComparer<T> : IEqualityComparer<T>
    {
        private Func<T, IComparable> _comparableExtractor;

        public GenericEqualityComparer(Func<T, string> comparableExtractor)
        {
            _comparableExtractor = comparableExtractor;
        }

        public bool Equals(T x, T y)
        {
            var xComparable = _comparableExtractor(x);
            var yComparable = _comparableExtractor(y);
            var ret = xComparable.Equals(yComparable);
            return ret;
        }

        public int GetHashCode(T obj)
        {
            var objComparable = _comparableExtractor(obj);
            return objComparable.GetHashCode();
        }
    }
}
