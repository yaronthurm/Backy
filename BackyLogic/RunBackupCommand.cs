using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackyLogic
{
    public class RunBackupCommand
    {
        private string _source;
        private string _target;
        private IFileSystem _fileSystem;

        public RunBackupCommand(IFileSystem fileSystem, string source, string target)
        {
            _fileSystem = fileSystem;
            _source = source;
            _target = target;
        }


        public void Execute()
        {
            if (IsFirstTime())
            {
                RunFirstTimeBackup(Path.Combine(_target, "1"));
                return;
            }

            State currentState = GetCurrentState();
            State lastBackedupState = GetLastBackedUpState();

            if (NoChangesFromLastBackup(currentState, lastBackedupState))
                return;

            var targetDir = GetTargetDirectory(lastBackedupState);
            CopyAllNewFiles(targetDir, currentState, lastBackedupState);
            CopyAllUpdatedFiles(targetDir, currentState, lastBackedupState);
            MarkAllDeletedFiles(targetDir, currentState, lastBackedupState);
        }

        private void MarkAllDeletedFiles(string targetDir, State currentState, State lastBackedupState)
        {
            var deletedFiles = GetDeletedFiles(currentState, lastBackedupState);
            if (deletedFiles.Any())
            {
                var deletedFilename = System.IO.Path.Combine(targetDir, "deleted.txt");
                _fileSystem.CreateFile(deletedFilename);
                foreach (var file in deletedFiles)
                    _fileSystem.AppendLine(deletedFilename, file.RelativeName);
            }
        }

        private void CopyAllUpdatedFiles(string targetDir, State currentState, State lastBackedupState)
        {
            // TODO
        }

        private void CopyAllNewFiles(string targetDir, State currentState, State lastBackedupState)
        {
            var newFiles = GetNewFiles(currentState, lastBackedupState);
            targetDir = Path.Combine(targetDir, "new");
            foreach (BackyFile newFile in newFiles)
            {
                _fileSystem.Copy(newFile.PhysicalPath, System.IO.Path.Combine(targetDir, newFile.RelativeName));
            }
        }

        private string GetTargetDirectory(State lastBackedupState)
        {
            var ret = System.IO.Path.Combine(_target, lastBackedupState.GetNextDirectory(_fileSystem, _target));
            return ret;
        }

        private IEnumerable<BackyFile> GetNewFiles(State currentState, State lastBackedupState)
        {
            var ret = new List<BackyFile>();
            foreach (var file in currentState.Files)
            {
                if (lastBackedupState.Files.Any(x => x.RelativeName == file.RelativeName))
                    // not new
                    continue;
                ret.Add(file);
            }
            return ret;
        }

        private IEnumerable<BackyFile> GetModifiedFiles(State currentState, State lastBackedupState)
        {
            var ret = new List<BackyFile>();
            foreach (var file in currentState.Files)
            {
                // look for file in backup
                var backupfile = lastBackedupState.Files.FirstOrDefault(x => x.RelativeName == file.RelativeName);
                if (backupfile == null || backupfile.LastWriteTime == file.LastWriteTime)
                    continue;
                
                ret.Add(file);
            }
            return ret;
        }

        private IEnumerable<BackyFile> GetDeletedFiles(State currentState, State lastBackedupState)
        {
            var ret = new List<BackyFile>();
            foreach (var file in lastBackedupState.Files)
            {
                // look for file in current
                var currentFile = currentState.Files.FirstOrDefault(x => x.RelativeName == file.RelativeName);
                if (currentFile == null)
                    ret.Add(file);
            }
            return ret;
        }

        private bool NoChangesFromLastBackup(State currentState, State lastBackedupState)
        {
            if (GetNewFiles(currentState, lastBackedupState).Any())
                return false;
            if (GetModifiedFiles(currentState, lastBackedupState).Any())
                return false;
            if (GetDeletedFiles(currentState, lastBackedupState).Any())
                return false;
            return true;
        }

        private State GetLastBackedUpState()
        {
            var ret = new State();

            // Get all backup files
            var allBackupFiles = _fileSystem.GetAllFiles(_target);
            var allBackupDirectories = State.GetFirstLevelDirectories(_fileSystem, _target).Select(x => System.IO.Path.Combine(_target, x));

            var backyFolders = new List<BackyFolder>();
            foreach (string dir in allBackupDirectories)
            {
                var allFilesForThisDirectory = allBackupFiles.Where(x => x.StartsWith(dir));
                var newFolder = BackyFolder.FromFileNames(_fileSystem, allFilesForThisDirectory, dir);
                backyFolders.Add(newFolder);
            }

            foreach (BackyFolder backyFolder in backyFolders.OrderBy(x => x.SerialNumber))
            {
                ret.Files.AddRange(backyFolder.New);
                foreach (var deleted in backyFolder.Deleted)
                    ret.Files.RemoveAll(x => x.RelativeName == deleted);
                foreach (var rename in backyFolder.Renamed)
                {
                    var file = ret.Files.First(x => x.RelativeName == rename.OldName);
                    file.RelativeName = rename.NewName;
                }
                // TODO: Handle modification
            }

            return ret;
        }

        private State GetCurrentState()
        {
            var files = _fileSystem.GetAllFiles(_source);

            var ret = new State();
            ret.Files = files.Select(x => BackyFile.FromSourceFileName(_fileSystem, x, _source)).ToList();
            return ret;
        }

        private void RunFirstTimeBackup(string targetDir)
        {
            CopyAllNewFiles(targetDir, GetCurrentState(), new State());
        }

        private bool IsFirstTime()
        {
            // We expect to see folders in the target directory. If we don't see we assume it's the first time
            var dirs = State.GetFirstLevelDirectories(_fileSystem, _target);
            var ret = dirs.Any() == false;
            return ret;
        }
    }
}
