using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private bool _abort;
        private FoldersDiff _diff;
        public IMultiStepProgress Progress;

        public RunBackupCommand(IFileSystem fileSystem, string source, string target)
        {
            _fileSystem = fileSystem;
            _source = source;
            _target = target;
        }


        public void Execute()
        {
            var sw = Stopwatch.StartNew();
            try {
                this.Progress?.StartStepWithoutProgress("Started: " + DateTime.Now);

                int count = 0;

                this.Progress?.StartUnboundedStep("Scanning source files. Files scanned:");
                State currentState = State.GetCurrentState(_fileSystem, _source, () =>
                {
                    count++;
                    if (count % 100 == 0) this.Progress?.UpdateProgress(count);
                });
                this.Progress?.UpdateProgress(count);
                if (IsAborted()) return;

                this.Progress?.StartUnboundedStep("Scanning backup files. Files scanned:");
                count = 0;
                State lastBackedupState = State.GetLastBackedUpState(_fileSystem, _target, () =>
                {
                    count++;
                    if (count % 100 == 0) this.Progress?.UpdateProgress(count);
                });
                this.Progress?.UpdateProgress(count);
                if (IsAborted()) return;

                this.Progress?.StartStepWithoutProgress("Calculating diff");
                _diff = GetDiff(currentState, lastBackedupState);
                _diff.CalculateDiff();
                if (IsAborted()) return;
                this.Progress?.StartStepWithoutProgress(
                    "Finished calculating diff:\n" +
                    $"  New files: {_diff.NewFiles.Count}\n" +
                    $"  Modified files: {_diff.ModifiedFiles.Count}\n" +
                    $"  Deleted files: {_diff.DeletedFiles.Count}\n" +
                    $"  Renamed files: {_diff.RenamedFiles.Count}");

                if (NoChangesFromLastBackup(_diff))
                {
                    return;
                }

                var targetDir = GetTargetDirectory(lastBackedupState);
                CopyAllNewFiles(targetDir, _diff);
                if (IsAborted()) return;
                CopyAllModifiedFiles(targetDir, _diff);
                if (IsAborted()) return;
                MarkAllDeletedFiles(targetDir, _diff);
                if (IsAborted()) return;
                MarkAllRenamedFiles(targetDir, _diff);

                _fileSystem.MakeDirectoryReadOnly(targetDir);
            }
            finally
            {
                sw.Stop();
                this.Progress?.StartStepWithoutProgress("Finished: " + DateTime.Now);
                this.Progress?.StartStepWithoutProgress("Total time: " + sw.Elapsed);
            }

        }

        private bool IsAborted()
        {
            return _abort;
        }

        private FoldersDiff GetDiff(State currentState, State lastBackedupState)
        {
            var ret = new FoldersDiff(_fileSystem, currentState, lastBackedupState);
            ret.Progress = this.Progress;
            return ret;
        }

        public void Abort()
        {
            _abort = true;
            if (_diff != null)
                _diff.Abort();
        }

        private void MarkAllRenamedFiles(string targetDir, FoldersDiff diff)
        {
            var renamedFiles = diff.RenamedFiles;
            if (renamedFiles.Any())
            {
                this.Progress?.StartBoundedStep("Marking renamed files:", renamedFiles.Count);
                var renamedFilename = System.IO.Path.Combine(targetDir, "renamed.txt");
                _fileSystem.CreateFile(renamedFilename);
                foreach (var file in renamedFiles)
                {
                    if (_abort) break;
                    string renameLine = new JObject(new JProperty("oldName", file.OldName), new JProperty("newName", file.NewName)).ToString(Newtonsoft.Json.Formatting.None);
                    _fileSystem.AppendLine(renamedFilename, renameLine);
                    this.Progress?.Increment();
                }
            }
        }

        private void MarkAllDeletedFiles(string targetDir, FoldersDiff diff)
        {
            var deletedFiles = diff.DeletedFiles;
            if (deletedFiles.Any())
            {
                this.Progress?.StartBoundedStep("Marking deleted files:", deletedFiles.Count);
                var deletedFilename = System.IO.Path.Combine(targetDir, "deleted.txt");
                _fileSystem.CreateFile(deletedFilename);
                foreach (var file in deletedFiles)
                {
                    if (_abort) break;
                    _fileSystem.AppendLine(deletedFilename, file.RelativeName);
                    this.Progress?.Increment();
                }
            }
        }

        private void CopyAllModifiedFiles(string targetDir, FoldersDiff diff)
        {
            var modifiedFiles = diff.ModifiedFiles;
            targetDir = Path.Combine(targetDir, "modified");
            if (modifiedFiles.Any())
                this.Progress?.StartBoundedStep("Copy modified files", modifiedFiles.Count);
            foreach (BackyFile file in modifiedFiles)
            {
                if (_abort) break;
                try {
                    _fileSystem.Copy(file.PhysicalPath, System.IO.Path.Combine(targetDir, file.RelativeName));                    
                    this.Progress?.Increment();
                }
                catch
                {
                    //_progress.Failed.Add(file.RelativeName);
                }
            }
        }

        private void CopyAllNewFiles(string targetDir, FoldersDiff diff)
        {
            var newFiles = diff.NewFiles;
            targetDir = Path.Combine(targetDir, "new");
            if (newFiles.Count > 0)
                this.Progress?.StartBoundedStep("Copy new files", newFiles.Count);
            foreach (BackyFile file in newFiles)
            {
                if (_abort) break;
                try {
                    _fileSystem.Copy(file.PhysicalPath, System.IO.Path.Combine(targetDir, file.RelativeName));
                    this.Progress?.Increment();
                }
                catch
                {
                    //_progress.Failed.Add(file.RelativeName);
                    //RaiseOnProgress();
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
