using System;
using System.Collections.Generic;
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
                RunFirstTimeBackup();
                return;
            }

            State currentState = GetCurrentState();
            State lastBackedupState = GetLastBackedUpState();

            if (NoChangesFromLastBackup(currentState, lastBackedupState))
                return;

            CopyAllNewFiles(currentState, lastBackedupState);
            CopyAllUpdatedFiles(currentState, lastBackedupState);
            MarkAllDeletedFiles(currentState, lastBackedupState);
        }

        private void MarkAllDeletedFiles(State currentState, State lastBackedupState)
        {
            throw new NotImplementedException();
        }

        private void CopyAllUpdatedFiles(State currentState, State lastBackedupState)
        {
            throw new NotImplementedException();
        }

        private void CopyAllNewFiles(State currentState, State lastBackedupState)
        {
            var newFiles = GetNewFiles(currentState, lastBackedupState);
            var targetDir = GetTargetDirectoryForNewFiles(lastBackedupState);
            _fileSystem.CreateDirectory(targetDir);
            foreach (BackyFile newFile in newFiles)
            {
                var relativeName = newFile.FullName.Replace(_source + "\\", "");
                _fileSystem.Copy(newFile.FullName, System.IO.Path.Combine(targetDir, relativeName));
            }
        }

        private string GetTargetDirectoryForNewFiles(State lastBackedupState)
        {
            var ret = System.IO.Path.Combine(_target, lastBackedupState.GetNextDirectory(_fileSystem, _target), "new");
            return ret;
        }

        private IEnumerable<BackyFile> GetNewFiles(State currentState, State lastBackedupState)
        {
            var ret = new List<BackyFile>();
            foreach (var file in currentState.Files)
            {
                if (lastBackedupState.Files.Any(x => x.FullName == file.FullName))
                    // not new
                    continue;
                ret.Add(file);
            }
            return ret;
        }

        private bool NoChangesFromLastBackup(State currentState, State lastBackedupState)
        {
            throw new NotImplementedException();
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
                    ret.Files.RemoveAll(x => x.FullName == deleted);
                foreach (var rename in backyFolder.Renamed)
                {
                    var file = ret.Files.First(x => x.FullName == rename.OldName);
                    file.FullName = rename.NewName;
                }
                // TODO: Handle modification
            }

            return ret;
        }

        private State GetCurrentState()
        {
            var files = _fileSystem.GetAllFiles(_source);

            var ret = new State();
            ret.Files = files.Select(x => BackyFile.FromFullFileName(_fileSystem, x)).ToList();
            return ret;
        }

        private void RunFirstTimeBackup()
        {
            CopyAllNewFiles(GetCurrentState(), new State());
        }

        private bool IsFirstTime()
        {
            // We expect to see folders in the target directory. If we don't see we assume it's the first time
            var dirs = _fileSystem.GetDirectories(_target);
            var ret = dirs.Any() == false;
            return ret;
        }
    }
}
