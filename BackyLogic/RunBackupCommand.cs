using Newtonsoft.Json.Linq;
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
        private BackyProgress _progress = new BackyProgress();
        private bool _abort;
        private FoldersDiff _diff;

        public event Action<BackyProgress> OnProgress;


        public RunBackupCommand(IFileSystem fileSystem, string source, string target)
        {
            _fileSystem = fileSystem;
            _source = source;
            _target = target;
        }


        public void Execute()
        {
            State currentState = State.GetCurrentState(_fileSystem, _source);
            State lastBackedupState = State.GetLastBackedUpState(_fileSystem, _target);
            _diff = GetDiff(currentState, lastBackedupState);
            _diff.CalculateDiff();
            _progress.NewFilesTotal = _diff.NewFiles.Count;
            _progress.ModifiedFilesTotal = _diff.ModifiedFiles.Count;
            _progress.DeletedFilesTotal = _diff.DeletedFiles.Count;
            _progress.RenamedFilesTotal = _diff.RenamedFiles.Count;
            _progress.CalculateDiffFinished = true;
            RaiseOnProgress();

            if (NoChangesFromLastBackup(_diff))
            {
                _progress.NoChangeDetected = true;
                RaiseOnProgress();
                return;
            }

            var targetDir = GetTargetDirectory(lastBackedupState);
            CopyAllNewFiles(targetDir, _diff);
            CopyAllModifiedFiles(targetDir, _diff);
            MarkAllDeletedFiles(targetDir, _diff);
            MarkAllRenamedFiles(targetDir, _diff);
        }

        private FoldersDiff GetDiff(State currentState, State lastBackedupState)
        {
            var ret = new FoldersDiff(_fileSystem, currentState, lastBackedupState);
            ret.OnProgress += OnDiffProgress;
            return ret;
        }

        private void OnDiffProgress(DiffProgress obj)
        {
            _progress.RenameDetectionTotal = obj.RenameDetectionTotal;
            _progress.RenameDetectionFinish = obj.RenameDetectionFinished;
            RaiseOnProgress();
        }

        public void Abort()
        {
            _abort = true;
            if (_diff != null)
                _diff.Abort();
        }

        private void RaiseOnProgress()
        {
            if (OnProgress != null)
                OnProgress(_progress);
        }

        private void MarkAllRenamedFiles(string targetDir, FoldersDiff diff)
        {
            var renamedFiles = diff.RenamedFiles;
            if (renamedFiles.Any())
            {
                var renamedFilename = System.IO.Path.Combine(targetDir, "renamed.txt");
                _fileSystem.CreateFile(renamedFilename);
                foreach (var file in renamedFiles)
                {
                    if (_abort) break;
                    string renameLine = new JObject(new JProperty("oldName", file.OldName), new JProperty("newName", file.NewName)).ToString(Newtonsoft.Json.Formatting.None);
                    _fileSystem.AppendLine(renamedFilename, renameLine);
                    _progress.RenamedFilesFinished++;
                    RaiseOnProgress();
                }
            }
        }

        private void MarkAllDeletedFiles(string targetDir, FoldersDiff diff)
        {
            var deletedFiles = diff.DeletedFiles;
            if (deletedFiles.Any())
            {
                var deletedFilename = System.IO.Path.Combine(targetDir, "deleted.txt");
                _fileSystem.CreateFile(deletedFilename);
                foreach (var file in deletedFiles)
                {
                    if (_abort) break;
                    _fileSystem.AppendLine(deletedFilename, file.RelativeName);
                    _progress.DeletedFilesFinished++;
                    RaiseOnProgress();
                }
            }
        }

        private void CopyAllModifiedFiles(string targetDir, FoldersDiff diff)
        {
            var modifiedFiles = diff.ModifiedFiles;
            targetDir = Path.Combine(targetDir, "modified");
            foreach (BackyFile file in modifiedFiles)
            {
                if (_abort) break;
                try {
                    _fileSystem.Copy(file.PhysicalPath, System.IO.Path.Combine(targetDir, file.RelativeName));
                    _progress.ModifiedFilesFinished++;
                    RaiseOnProgress();
                }
                catch
                {
                    _progress.Failed.Add(file.RelativeName);
                    RaiseOnProgress();
                }
            }
        }

        private void CopyAllNewFiles(string targetDir, FoldersDiff diff)
        {
            var newFiles = diff.NewFiles;
            targetDir = Path.Combine(targetDir, "new");
            foreach (BackyFile file in newFiles)
            {
                if (_abort) break;
                try {
                    _fileSystem.Copy(file.PhysicalPath, System.IO.Path.Combine(targetDir, file.RelativeName));
                    _progress.NewFilesFinished++;
                    RaiseOnProgress();
                }
                catch
                {
                    _progress.Failed.Add(file.RelativeName);
                    RaiseOnProgress();
                }
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
            if (diff.RenamedFiles.Any())
                return false;
            return true;
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
