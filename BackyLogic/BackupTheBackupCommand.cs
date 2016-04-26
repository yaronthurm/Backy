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

                var missingSources = FindMissingSources();
                var existingSources = FindExistingSources();

                CopyEntireSources(missingSources);
                CopyMissingContentForExistingSources(existingSources);
            }
            finally
            {
                sw.Stop();
                this.Progress?.StartStepWithoutProgress($"Finished backing up the backup'{_source}' at: { DateTime.Now }");
                this.Progress?.StartStepWithoutProgress("Total time: " + sw.Elapsed);
            }
        }


        private void CopyMissingContentForExistingSources(IEnumerable<string> existingSources)
        {
            foreach (var existingSource in existingSources)
            {
                var missingDirectories = FindMissingDirectories(Path.Combine(_source, existingSource), Path.Combine(_target, existingSource));
                foreach (var missingDir in missingDirectories)
                {
                    var dirToCopy = Path.Combine(_source, existingSource, missingDir);
                    var destination = Path.Combine(_target, existingSource, missingDir);
                    CopyEntireDirectory(dirToCopy, destination);
                    MakeReadOnly(destination);
                }
            }
        }

        private IEnumerable<string> FindMissingDirectories(string sourceDirectory, string targetDirectory)
        {
            var dirsInSource = _fileSystem.GetTopLevelDirectories(sourceDirectory).Select(x => Path.GetFileName(x));
            var dirsInTarget = _fileSystem.GetTopLevelDirectories(targetDirectory).Select(x => Path.GetFileName(x));
            var ret = dirsInSource.Except(dirsInTarget);
            return ret;
        }

        private IEnumerable<string> FindExistingSources()
        {
            var sourcesInSource = FindSources(_source).Select(x => x.Guid);
            var sourcesInTarget = FindSources(_target).Select(x => x.Guid);
            var ret = sourcesInSource.Intersect(sourcesInTarget);
            return ret;
        }

        private void CopyEntireSources(IEnumerable<string> missingSources)
        {
            foreach (var missingSource in missingSources)
            {
                var dirToCopy = Path.Combine(_source, missingSource);
                var destination = Path.Combine(_target, missingSource);
                CopyEntireDirectory(dirToCopy, destination);
                foreach (var innerDirectory in _fileSystem.GetTopLevelDirectories(destination))
                    MakeReadOnly(innerDirectory);
            }
        }
        
        private void CopyEntireDirectory(string dirToCopy, string destination)
        {
            var filesToCopy = _fileSystem.EnumerateFiles(dirToCopy).ToArray();
            foreach (var file in filesToCopy)
            {
                var destinationFileName = Path.Combine(destination, file.Replace(dirToCopy + "\\", ""));
                _fileSystem.Copy(file, destinationFileName);
            }
        }

        private IEnumerable<BackupDirectory> FindSources(string path)
        {
            var ret = _fileSystem.GetTopLevelDirectories(path)
                .Where(x => BackupDirectory.IsBackupDirectory(x, _fileSystem))
                .Select(x => BackupDirectory.FromPath(x, _fileSystem));
            return ret;
        }

        private IEnumerable<string> FindMissingSources()
        {
            var dirsInSource = FindSources(_source).Select(x => x.Guid);
            var dirsInTarget = FindSources(_target).Select(x => x.Guid);
            var ret = dirsInSource.Except(dirsInTarget);
            return ret;
        }

        private void MakeReadOnly(string targetDir)
        {
            _fileSystem.MakeDirectoryReadOnly(targetDir);
        }
    }

    public class BackupDirectory
    {
        public string FullPath;
        public string Guid;
        public string OriginalSource;

        public static BackupDirectory FromPath(string path, IFileSystem fs)
        {
            var ret = new BackupDirectory();
            var iniFile = fs.FindFile(path, "Backy.ini");
            var iniLines = fs.ReadLines(iniFile).ToArray();
            ret.FullPath = path;
            ret.OriginalSource = iniLines.First();
            ret.Guid = iniLines.Skip(1).First();
            return ret;
        }

        public static bool IsBackupDirectory(string path, IFileSystem fs)
        {
            // Should contain a Backy.ini file
            var ret = fs.FindFile(path, "Backy.ini") != null;
            return ret;
        }

        internal static void CreateIniFile(string sourceGuid, string target, string source, IFileSystem fs)
        {
            fs.CreateFile(Path.Combine(target, sourceGuid, "backy.ini"));
            fs.AppendLine(Path.Combine(target, sourceGuid, "backy.ini"), source);
            fs.AppendLine(Path.Combine(target, sourceGuid, "backy.ini"), sourceGuid);
        }
    }
}
