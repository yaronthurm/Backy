using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackyLogic
{
    public class State
    {
        public List<BackyFile> Files = new List<BackyFile>();
        private FilesAndDirectoriesTree _tree;
        public int Version;

        internal string GetNextDirectory(IFileSystem fileSystem, string targetDir)
        {
            var firstLevel = GetFirstLevelDirectories(fileSystem, targetDir);

            // Get highest number
            var max = firstLevel.Union(new[] { "0" }).Select(int.Parse).Max();
            return (max + 1).ToString();
        }

        public static IEnumerable<string> GetFirstLevelDirectories(IFileSystem fileSystem, string targetDir)
        {
            // Get all directories in target
            var dirs = fileSystem.GetDirectories(targetDir);

            // Get just first level directories
            if (!targetDir.EndsWith("\\")) targetDir += "\\";
            var ret = dirs.Select(x => x.Replace(targetDir, "")).Select(x => x.Split('\\')[0]).Distinct();

            return ret;
        }

        public static State GetLastBackedUpState(IFileSystem fileSystem, string target)
        {
            var stateCalculator = new TransientState(fileSystem, target);
            var ret = stateCalculator.GetLastState();
            return ret;
        }

        public static State GetCurrentState(IFileSystem fileSystem, string source)
        {
            var files = fileSystem.GetAllFiles(source);

            var ret = new State();
            ret.Files = files.Select(x => BackyFile.FromSourceFileName(fileSystem, x, source)).ToList();
            return ret;
        }

        
        public bool ContainsFile(string fileRelativePath)
        {
            var keys = fileRelativePath.Split('\\');
            var ret = _tree.Contains(keys);
            return ret;
        }

    }

    public class FilesAndDirectoriesTree
    {
        private Dictionary<string, IVirtualFile> _files = new Dictionary<string, IVirtualFile>();
        private Dictionary<string, FilesAndDirectoriesTree> _directories = new Dictionary<string, FilesAndDirectoriesTree>();
        

        public void Add(IVirtualFile file)
        {
            var keyPath = file.GetPath();
            var currentDirectory = this.GetByPath(keyPath, true);
            currentDirectory._files.Add(keyPath[keyPath.Length - 1], file);
        }

        public bool Contains(params string[] keyPath)
        {
            var directory = this.GetByPath(keyPath, false);
            if (directory == null)
                return false;
            var ret = directory._files.ContainsKey(keyPath.Last());
            return ret;
        }

        public IEnumerable<IVirtualFile> GetFirstLevelFiles(params string[] keyPath)
        {
            var ret = new List<IVirtualFile>();
            var directory = this.GetByPath(keyPath, false);
            if (directory != null)
                ret.AddRange(directory._files.Values);
            return ret;
        }

        public IEnumerable<string> GetFirstLevelDirectories(params string[] keyPath)
        {
            var ret = new List<string>();
            var directory = this.GetByPath(keyPath, false);
            if (directory != null)
                ret.AddRange(directory._directories.Keys);
            return ret;
        }


        private FilesAndDirectoriesTree GetByPath(string[] keyPath, bool createIfMissing)
        {
            if (keyPath.Last() != "") keyPath = keyPath.Concat(new[] { "" }).ToArray();
            var ret = this;
            for (int i = 0; i < keyPath.Length - 1; i++)
            {
                var key = keyPath[i];
                FilesAndDirectoriesTree nextDirectory;
                if (!ret._directories.TryGetValue(key, out nextDirectory))
                {
                    if (!createIfMissing)
                        return null;
                    else {
                        nextDirectory = new FilesAndDirectoriesTree();
                        ret._directories.Add(key, nextDirectory);
                    }
                }
                ret = nextDirectory;
            }

            return ret;
        }
    }

    public interface IVirtualFile {
        string LogicalName { get; }
        string PhysicalPath { get; }

        string[] GetPath();
    }


    public class TransientState
    {
        List<BackyFolder> _backyFolders;

        public TransientState(IFileSystem fileSystem, string target)
        {
            // Get all backup files
            var allBackupFiles = fileSystem.GetAllFiles(target);
            var allBackupDirectories = State.GetFirstLevelDirectories(fileSystem, target).Select(x => System.IO.Path.Combine(target, x));

            var backyFolders = new List<BackyFolder>();
            foreach (string dir in allBackupDirectories)
            {
                var allFilesForThisDirectory = allBackupFiles.Where(x => x.StartsWith(dir));
                var newFolder = BackyFolder.FromFileNames(fileSystem, allFilesForThisDirectory, dir);
                backyFolders.Add(newFolder);
            }

            _backyFolders = backyFolders;
        }

        public int MaxVersion
        {
            get { return _backyFolders.Count; }
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
            foreach (BackyFolder backyFolder in _backyFolders.OrderBy(x => x.SerialNumber).Take(version))
           {
                // Add new files
                ret.Files.AddRange(backyFolder.New);

                // Remove deleted files
                foreach (var deleted in backyFolder.Deleted)
                    ret.Files.RemoveAll(x => x.RelativeName == deleted);

                // Handle renamed files
                foreach (var rename in backyFolder.Renamed)
                {
                    // In order to not touching the refernce, we will clone the file and modify it.
                    var file = ret.Files.First(x => x.RelativeName == rename.OldName);
                    ret.Files.Remove(file);
                    var renamedFile = file.Clone();
                    renamedFile.RelativeName = rename.NewName;
                    ret.Files.Add(renamedFile);
                }

                // Handle modified files
                foreach (var modified in backyFolder.Modified)
                {
                    var file = ret.Files.First(x => x.RelativeName == modified.RelativeName);
                    ret.Files.Remove(file);
                    ret.Files.Add(modified);
                }
            }

            return ret;
        }
    }


    public class BackyFolder
    {
        public int SerialNumber;
        public List<BackyFile> New;
        public List<BackyFile> Modified;
        public List<string> Deleted;
        public List<RenameInfo> Renamed;


        /// <summary>
        /// we expect a structuor like this:
        /// d:\
        ///     target\
        ///         1\
        ///             new\
        ///                 file1, file2, file3
        ///                 subfolder\
        ///                     file1, file2
        ///             modified\
        ///                 file4, file5
        ///                 subfolder\
        ///                     file6, file7
        ///             deleted.txt (each row looks contains the name of the deleted file
        ///             renamed.txt (each row looks like that: {oldName: "relative path before rename", newName: "relative name after rename"})
        /// </summary>
        /// <returns></returns>
        public static BackyFolder FromFileNames(IFileSystem fileSystem, IEnumerable<string> fileNames, string rootDir)
        {
            // rootDir is expected to be in the format d:\target\1
            var ret = new BackyFolder();
            ret.SerialNumber = int.Parse(System.IO.Path.GetFileName(rootDir));

            ret.New = fileNames
                .Where(x => x.StartsWith(System.IO.Path.Combine(rootDir, "new")))
                .Select(x => BackyFile.FromTargetFileName(fileSystem, x, rootDir + "\\new")).ToList();
            ret.Modified = fileNames
                .Where(x => x.StartsWith(System.IO.Path.Combine(rootDir, "modified")))
                .Select(x => BackyFile.FromTargetFileName(fileSystem, x, rootDir + "\\modified")).ToList();

            var deletedName = System.IO.Path.Combine(rootDir, "deleted.txt");
            if (fileNames.Contains(deletedName)) {
                ret.Deleted = fileSystem.ReadLines(deletedName).ToList();
            }
            else
                ret.Deleted = new List<string>();

            var renamedName = System.IO.Path.Combine(rootDir, "renamed.txt");
            if (fileNames.Contains(renamedName))
            {
                ret.Renamed = fileSystem.ReadLines(renamedName)
                    .Select(x => Newtonsoft.Json.Linq.JObject.Parse(x))
                    .Select(x => new RenameInfo { OldName = x.Value<string>("oldName"), NewName = x.Value<string>("newName") }).ToList();
            }
            else
                ret.Renamed = new List<RenameInfo>();

            return ret;
        }
    }

    public class RenameInfo
    {
        public string OldName;
        public string NewName;
    }
}
