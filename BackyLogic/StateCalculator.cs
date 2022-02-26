using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackyLogic
{

    public class StateCalculator
    {
        public event Action OnProgress;

        private IFileSystem _fileSystem;        
        private Lazy<List<BackyFolder>> _backyFolders;

        public string Target { get; }

        public StateCalculator(IFileSystem fileSystem, string target, string source, string machineID)
        {
            _fileSystem = fileSystem;
            if (source == null)
                Target = target;
            else
                Target = FindTargetForSource(source, target, fileSystem, machineID);
            _backyFolders = new Lazy<List<BackyFolder>>(this.GetFolders);
        }

        private static string FindTargetForSource(string source, string target, IFileSystem fs, string machineID)
        {
            string sourceGuid = FindTargetForSourceOrNull(source, target, fs, machineID);
            if (sourceGuid == null)
               throw new ApplicationException("Could not find directory for source: " + source);
            var ret = Path.Combine(target, sourceGuid);
            return ret;
        }

        public static string FindTargetForSourceOrNull(string source, string target, IFileSystem fs, string machineID)
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

        public static bool IsTargetForSourceExist(string source, string target, IFileSystem fs, string machineID)
        {
            string sourceGuid = FindTargetForSourceOrNull(source, target, fs, machineID);
            return sourceGuid != null;
        }

        public int MaxVersion
        {
            get { return _backyFolders.Value.Count; }
        }

        public State GetLastState()
        {
            return this.GetState(this.MaxVersion);
        }

        public State GetState(int version)
        {
            if (version > this.MaxVersion)
                throw new ApplicationException("max version exeeded");

            var ret = new State();
            foreach (BackyFolder backyFolder in _backyFolders.Value.OrderBy(x => x.SerialNumber).Take(version))
            {
                // Add new files
                backyFolder.New.ForEach(x => ret.AddFile(x));

                // Remove deleted files
                backyFolder.Deleted.ForEach(x => ret.DeleteFileByPath(x));

                // Handle renamed files
                foreach (var rename in backyFolder.Renamed)
                {
                    // In order to not touching the refernce, we will clone the file and modify it.
                    var file = ret.FindFile(rename.OldName);
                    ret.DeleteFile(file);
                    var renamedFile = file.Clone();
                    renamedFile.RelativeName = rename.NewName;
                    ret.AddFile(renamedFile);
                }

                // Handle modified files
                foreach (var modified in backyFolder.Modified)
                {
                    ret.DeleteFileByPath(modified.RelativeName);
                    ret.AddFile(modified);
                }
            }

            return ret;
        }

        public State GetDiff(int version)
        {
            if (version > this.MaxVersion)
                throw new ApplicationException("max version exeeded");

            // Get all files in backup directory
            var ret = new State();
            var rootPath = Path.Combine(Target, version.ToString());
            foreach (var file in _fileSystem.EnumerateFiles(rootPath)) 
            {
                var backyFile = BackyFile.FromTargetFileName(_fileSystem, file, rootPath);
                ret.AddFile(backyFile);
            }
            return ret;
        }


        private List<BackyFolder> GetFolders()
        {
            var tree = new HierarchicalDictionary<string, string>();
            
            // Get all backup files
            foreach (var file in _fileSystem.EnumerateFiles(Target))
            {
                tree.Add(file, file.Split('\\'));
                this.OnProgress?.Invoke();
            }

            var ret = new List<BackyFolder>();
            foreach (string dir in tree.GetFirstLevelContainers(Target.Split('\\')))
            {
                var fullDirectoryPath = System.IO.Path.Combine(Target, dir);
                var allFilesForThisDirectory = tree.GetAllDescendantsItems(fullDirectoryPath.Split('\\'));
                var newFolder = BackyFolder.FromFileNames(_fileSystem, allFilesForThisDirectory, fullDirectoryPath);
                ret.Add(newFolder);

            }
            return ret;
        }

        public DateTime GetDateByVersion(int currentVersion)
        {
            var backyFolder = _backyFolders.Value.First(x => x.SerialNumber == currentVersion);
            return backyFolder.DateCreated;
        }
    }

    public class ShallowFoldersMaker
    {
        public static void MakeFolderShallow(IFileSystem fs, string target, string source, string machineID, int version)
        {
            var stateCalculator = new StateCalculator(fs, target, source, machineID);
            var state = stateCalculator.GetDiff(version);
            var files = state.GetFiles()
                .Select(x => new { nameParts = x.RelativeName.Split(new[] { '\\' }, 2), orig = x })
                .Where(x => x.nameParts.Length == 2)
                .Select(x => new { type = x.nameParts[0], name = x.nameParts[1], x.orig})
                .GroupBy(x => x.type);

            var rootFolder = files.FirstOrDefault()?.FirstOrDefault()?.orig.Root;
            if (rootFolder == null) return;
            fs.MarkDirectoryAsFullControl(rootFolder);
            foreach (var group in files)
            {
                var tmpFile = rootFolder + "\\" + group.Key + ".tmp";
                var finalFile = rootFolder + "\\" + group.Key + ".txt";
                if (fs.FindFile(rootFolder, group.Key + ".txt") != null)
                    continue; // Shallow file already exists
                fs.CreateFile(tmpFile);
                var lines = group.Select(file => new JObject(new JProperty("name", file.name), new JProperty("lastWrite", file.orig.LastWriteTime)).ToString(Newtonsoft.Json.Formatting.None));
                fs.AppendLines(tmpFile, lines.ToArray());
                fs.Copy(tmpFile, finalFile);
                fs.DeleteFile(tmpFile);
                fs.RenameDirectory(rootFolder + "\\" + group.Key, rootFolder + "\\_" + group.Key);
                fs.DeleteDirectory(rootFolder + "\\_" + group.Key);
            }
            fs.MakeDirectoryReadOnly(rootFolder);
        }
    }
}
