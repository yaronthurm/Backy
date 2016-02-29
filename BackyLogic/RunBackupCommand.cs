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

            var diff = FoldersDiff.GetDiff(currentState, lastBackedupState);

            if (NoChangesFromLastBackup(diff))
                return;

            var targetDir = GetTargetDirectory(lastBackedupState);
            CopyAllNewFiles(targetDir, diff);
            CopyAllModifiedFiles(targetDir, diff);
            MarkAllDeletedFiles(targetDir, diff);
        }

        private void MarkAllDeletedFiles(string targetDir, FoldersDiff diff)
        {
            var deletedFiles = diff.DeletedFiles;
            if (deletedFiles.Any())
            {
                var deletedFilename = System.IO.Path.Combine(targetDir, "deleted.txt");
                _fileSystem.CreateFile(deletedFilename);
                foreach (var file in deletedFiles)
                    _fileSystem.AppendLine(deletedFilename, file.RelativeName);
            }
        }

        private void CopyAllModifiedFiles(string targetDir, FoldersDiff diff)
        {
            var modifiedFiles = diff.ModifiedFiles;
            targetDir = Path.Combine(targetDir, "modified");
            foreach (BackyFile newFile in modifiedFiles)
            {
                _fileSystem.Copy(newFile.PhysicalPath, System.IO.Path.Combine(targetDir, newFile.RelativeName));
            }
        }

        private void CopyAllNewFiles(string targetDir, FoldersDiff diff)
        {
            var newFiles = diff.NewFiles;
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

        private bool NoChangesFromLastBackup(FoldersDiff diff)
        {
            if (diff.NewFiles.Any())
                return false;
            if (diff.ModifiedFiles.Any())
                return false;
            if (diff.DeletedFiles.Any())
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
                // Add new files
                ret.Files.AddRange(backyFolder.New);

                // Remove deleted files
                foreach (var deleted in backyFolder.Deleted)
                    ret.Files.RemoveAll(x => x.RelativeName == deleted);

                // Handle renamed files
                foreach (var rename in backyFolder.Renamed)
                {
                    var file = ret.Files.First(x => x.RelativeName == rename.OldName);
                    file.RelativeName = rename.NewName;
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

        private State GetCurrentState()
        {
            var files = _fileSystem.GetAllFiles(_source);

            var ret = new State();
            ret.Files = files.Select(x => BackyFile.FromSourceFileName(_fileSystem, x, _source)).ToList();
            return ret;
        }

        private void RunFirstTimeBackup(string targetDir)
        {
            var diff = FoldersDiff.GetDiff(GetCurrentState(), new State());
            CopyAllNewFiles(targetDir, diff);
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
