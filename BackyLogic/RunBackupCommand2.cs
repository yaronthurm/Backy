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
    public class RunBackupCommand2
    {
        private string _source;
        private string _targetForSource;
        private IFileSystem _fileSystem;
        private FoldersDiff _diff;
        public IMultiStepProgress Progress;
        private CancellationToken _cancellationToken;
        private MachineID _machineID;

        public List<BackupFailure> Failures { get; private set; } = new List<BackupFailure>();
        public string FailuresPretty
        {
            get
            {
                var ret = string.Join(
                    "\n",
                    Failures
                    .Take(20)
                    . Select(x => $"{x.ErrorMessage} (Error Details: {x.ErrorDetails})").ToArray());
                return ret;
            }
        }


        public RunBackupCommand2(IFileSystem fileSystem, string source, string target, MachineID machineID, CancellationToken cancellationToken = new CancellationToken())
        {
            _fileSystem = fileSystem;
            _source = source;
            _machineID = machineID;
            _targetForSource = FindOrCreateTargetForSource(source, target, fileSystem, _machineID.Value);            
            _cancellationToken = cancellationToken;
        }


        public void Execute()
        {
            if (IsAborted()) return;
            var sw = Stopwatch.StartNew();
            string targetDir = null;
            try {
                this.Progress?.StartStepWithoutProgress($"\nStarted backing up '{_source}' at: { DateTime.Now }");

                State currentState = GetCurrentState();
                if (IsAborted()) return;

                State lastBackedupState = GetLastBackedUpState();
                if (IsAborted()) return;

                CalculateDiff(currentState, lastBackedupState);

                if (NoChangesFromLastBackup(_diff))
                {
                    return;
                }

              
                var diffDir = GetDiffDirectory();
                PopulateDiffDirectory(diffDir);

                var historyDir = GetHistoryDirectory(lastBackedupState);
                UpdateStateAndHistory(diffDir, historyDir);
                ClearDiffDir(diffDir);
            }
            finally
            {
                if (targetDir != null)
                {
                    if (DirectoryIsEmpty(targetDir)) // Might happen if failed to copy files after creating the directory
                        DeleteDirectory(targetDir); // No need to keep it around
                    else
                        MakeReadOnly(targetDir);
                }
                sw.Stop();
                this.Progress?.StartStepWithoutProgress($"Finished backing up '{_source}' at: { DateTime.Now }");
                this.Progress?.StartStepWithoutProgress("Total time: " + sw.Elapsed);
            }
        }

        private void ClearDiffDir(string diffDir)
        {
            _fileSystem.DeleteDirectory(diffDir);
        }

        private void UpdateStateAndHistory(string diffDir, string historyDir)
        {
            var newFilesDir = Path.Combine(diffDir, "new");
            if (_fileSystem.IsDirectoryExist(newFilesDir))            
            {
                var newFilesPath = Path.Combine(historyDir, "new.txt");
                _fileSystem.CreateFile(newFilesPath);

                var newFilesRelativeNames = _fileSystem.EnumerateFiles(newFilesDir)
                    .Select(x => x.Replace(diffDir + "\\new\\", "")).ToList();
                Loop(newFilesRelativeNames, "Copying new files:", "Could not copy new file: ", x => x, file =>
                {
                    var fullName = Path.Combine(newFilesDir, file);
                    var currentStatePath = Path.Combine(_targetForSource, "CurrentState", file);
                    _fileSystem.Copy(fullName, currentStatePath);
                    _fileSystem.AppendLines(newFilesPath, file);
                    _fileSystem.DeleteFile(fullName);
                });
            }

            var deletedFilesPath = _fileSystem.FindFile(diffDir, "deleted.txt");
            if (deletedFilesPath != null)            
            {
                var deletedFilesRelativeNames = _fileSystem.ReadLines(deletedFilesPath).ToList();
                Loop(deletedFilesRelativeNames, "Copying deleted files:", "Could not copy deleted file: ", x => x, file =>
                {
                    var fullName = Path.Combine(_targetForSource, "CurrentState", file);
                    var histrotyName = Path.Combine(historyDir, "deleted", file);
                    _fileSystem.Copy(fullName, histrotyName);
                    _fileSystem.DeleteFile(fullName);
                });
            }

            var modifiedFilesDir = Path.Combine(diffDir, "modified");
            if (_fileSystem.IsDirectoryExist(modifiedFilesDir))
            {
                var modifiedFilesRelativeNames = _fileSystem.EnumerateFiles(modifiedFilesDir)
                    .Select(x => x.Replace(diffDir + "\\modified\\", "")).ToList();
                Loop(modifiedFilesRelativeNames, "Copying modified files:", "Could not copy modified file: ", x => x, file =>
                {
                    var fullName = Path.Combine(modifiedFilesDir, file);
                    var currentStatePath = Path.Combine(_targetForSource, "CurrentState", file);
                    var historyPath = Path.Combine(historyDir, "modified", file);
                    _fileSystem.Copy(currentStatePath, historyPath);
                    _fileSystem.Copy(fullName, currentStatePath);
                    _fileSystem.DeleteFile(fullName);
                });
            }

            var renamedFilesPath = _fileSystem.FindFile(diffDir, "renamed.txt");
            if (renamedFilesPath != null)
            {
                var renamedData = _fileSystem.ReadLines(renamedFilesPath)
                    .Select(x => Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(x))
                    .Select(x => new {
                        oldName = x.Value<string>("oldName"),
                        newName = x.Value<string>("newName")
                    }).ToList();
                Loop(renamedData, "Renaming files:", "Could not rename file: ", x => x.oldName, file =>
                {
                    var oldFullName = Path.Combine(_targetForSource, "CurrentState", file.oldName);
                    var newFullName = Path.Combine(_targetForSource, "CurrentState", file.newName);
                    _fileSystem.RenameFile(oldFullName, newFullName);
                });
                var historyFileName = Path.Combine(historyDir, "renamed.txt");
                _fileSystem.Copy(renamedFilesPath, historyFileName);
            }

            void Loop<T>(List<T> coll, string progressMessage, string errorMessage, Func<T, string> getRelativeName, Action<T> action)
            {
                this.Progress?.StartBoundedStep(progressMessage, coll.Count);
                foreach (var item in coll)
                {
                    if (IsAborted()) break;
                    try
                    {
                        action(item);
                        this.Progress?.Increment();
                    }
                    catch (Exception ex)
                    {
                        var name = getRelativeName(item);
                        this.Failures.Add(new BackupFailure
                        {
                            FileName = name,
                            ErrorMessage = errorMessage + name,
                            ErrorDetails = ex.Message
                        });
                    }
                }
            }
        }

        private void PopulateDiffDirectory(string diffDir)
        {
            var newFilesDir = Path.Combine(diffDir, "new");
            CopyAllNewFiles(newFilesDir, _diff);
            if (IsAborted()) return;

            var modifiedFilesDir = Path.Combine(diffDir, "modified");
            CopyAllModifiedFiles(modifiedFilesDir, _diff);
            if (IsAborted()) return;

            MarkAllDeletedFiles(diffDir, _diff);
            if (IsAborted()) return;

            MarkAllRenamedFiles(diffDir, _diff);
            if (IsAborted()) return;
        }

        private string GetDiffDirectory()
        {
            var ret = Path.Combine(_targetForSource, "_diff");
            return ret;
        }

        private void DeleteDirectory(string targetDir)
        {
            _fileSystem.DeleteDirectory(targetDir);
        }

        private bool DirectoryIsEmpty(string targetDir)
        {
            return _fileSystem.IsDirectoryExist(targetDir) && !_fileSystem.EnumerateFiles(targetDir).Any();
        }

        private void CalculateDiff(State currentState, State lastBackedupState)
        {
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
        }

        private State GetLastBackedUpState()
        {
            int count = 0;
            this.Progress?.StartUnboundedStep("Scanning backup files. Files scanned:");
            State lastBackedupState = State2.GetLastBackedUpState(_fileSystem, _targetForSource, _machineID.Value, () =>
            {
                count++;
                if (count % 100 == 0) this.Progress?.UpdateProgress(count);
            });
            this.Progress?.UpdateProgress(count);
            return lastBackedupState;
        }

        private State GetCurrentState()
        {
            int count = 0;
            this.Progress?.StartUnboundedStep("Scanning source files. Files scanned:");
            State currentState = State.GetCurrentState(_fileSystem, _source, () =>
            {
                count++;
                if (count % 100 == 0) this.Progress?.UpdateProgress(count);
            });
            this.Progress?.UpdateProgress(count);
            return currentState;
        }

        private void MakeReadOnly(string targetDir)
        {
            _fileSystem.MakeDirectoryReadOnly(targetDir);
        }

        private bool IsAborted()
        {
            return _cancellationToken.IsCancellationRequested;
        }

        private FoldersDiff GetDiff(State currentState, State lastBackedupState)
        {
            var ret = new FoldersDiff(_fileSystem, currentState, lastBackedupState);
            ret.Progress = this.Progress;
            return ret;
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
                    if (IsAborted()) break;
                    string renameLine = new JObject(new JProperty("oldName", file.OldName), new JProperty("newName", file.NewName)).ToString(Newtonsoft.Json.Formatting.None);
                    _fileSystem.AppendLines(renamedFilename, renameLine);
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
                    if (IsAborted()) break;
                    _fileSystem.AppendLines(deletedFilename, file.RelativeName);
                    this.Progress?.Increment();
                }
            }
        }

        private void CopyAllModifiedFiles(string targetDir, FoldersDiff diff)
        {
            var modifiedFiles = diff.ModifiedFiles;
            if (modifiedFiles.Any())
                this.Progress?.StartBoundedStep("Copy modified files", modifiedFiles.Count);
            foreach (BackyFile file in modifiedFiles)
            {
                if (IsAborted()) break;
                try {
                    _fileSystem.Copy(file.PhysicalPath, System.IO.Path.Combine(targetDir, file.RelativeName));                    
                    this.Progress?.Increment();
                }
                catch (Exception ex)
                {
                    this.Failures.Add(new BackupFailure
                    {
                        FileName = file.PhysicalPath,
                        ErrorMessage = "Could not copy modified file: " + file.RelativeName,
                        ErrorDetails = ex.Message
                    });

                    //_progress.Failed.Add(file.RelativeName);
                }
            }
        }

        private void CopyAllNewFiles(string targetDir, FoldersDiff diff)
        {
            var newFiles = diff.NewFiles;
            if (newFiles.Count > 0)
                this.Progress?.StartBoundedStep("Copy new files", newFiles.Count);
            foreach (BackyFile file in newFiles)
            {
                if (IsAborted()) break;
                try {
                    _fileSystem.Copy(file.PhysicalPath, System.IO.Path.Combine(targetDir, file.RelativeName));
                    this.Progress?.Increment();
                }
                catch (Exception ex)
                {
                    this.Failures.Add(new BackupFailure
                    {
                        FileName = file.PhysicalPath,
                        ErrorMessage = "Could not copy new file: " + file.RelativeName,
                        ErrorDetails = ex.Message
                    });
                    //_progress.Failed.Add(file.RelativeName);
                    //RaiseOnProgress();
                }
            }
        }

        private string GetHistoryDirectory(State lastBackedupState)
        {
            var historyDir = Path.Combine(_targetForSource, "History");
            var version = lastBackedupState.GetNextDirectory(_fileSystem, historyDir);
            var ret = Path.Combine(_targetForSource, "History", version);
            return ret;
        }

        private static string FindOrCreateTargetForSource(string source, string target, IFileSystem fs, string machineID)
        {
            string sourceGuid = StateCalculator.FindTargetForSourceOrNull(source, target, fs, machineID);
            if (sourceGuid == null)
            {
                sourceGuid = Guid.NewGuid().ToString("N");
                BackupDirectory.CreateIniFile(sourceGuid, target, source, fs, machineID);
            }

            var ret = Path.Combine(target, sourceGuid);
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
            var dirs = State.GetFirstLevelDirectories(_fileSystem, _targetForSource);
            var ret = dirs.Any() == false;
            return ret;
        }
    }
}
