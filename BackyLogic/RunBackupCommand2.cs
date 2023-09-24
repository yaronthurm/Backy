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
    public class RunBackupCommand2 : IRunBackupCommand
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
            this.Failures.Clear();
            try
            {
                if (BackupDirectory.FromPath(_targetForSource, _fileSystem).BackupMode != "current_state")
                    throw new Exception("Target directoy is not using the 'diff' backup mode. Cannot proceed");

                this.Progress?.StartStepWithoutProgress($"\nStarted backing up '{_source}' at: { DateTime.Now }");

                MaybeCleanupDirtyPreviousBackup();

                State currentState = GetCurrentState();
                if (IsAborted()) return;

                State lastBackedupState = GetLastBackedUpState();
                if (IsAborted()) return;

                CalculateDiff(currentState, lastBackedupState);

                if (NoChangesFromLastBackup(_diff))
                {
                    return;
                }

                var historyDir = GetHistoryDirectory(lastBackedupState);

                HandleNewFiles(historyDir, _diff);
                if (IsAborted()) return;

                HandleModifiedFiles(historyDir, _diff);
                if (IsAborted()) return;

                HandleDeletedFiles(historyDir, _diff);
                if (IsAborted()) return;

                HandleRenamedFiles(historyDir, _diff);
                if (IsAborted()) return;
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

        private void Loop<T>(List<T> coll, string progressMessage, string errorMessage, Func<T, string> getRelativeName, Action<T> action)
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

        private void MaybeCleanupDirtyPreviousBackup()
        {
            var historyDir = Path.Combine(_targetForSource, "History");
            var lastVersion = _fileSystem.GetTopLevelDirectories(historyDir)
                .Select(x => x.Replace(historyDir + "\\", ""))
                .Where(x => int.TryParse(x, out var _))
                .Select(x => int.Parse(x))
                .OrderBy(x => -x)
                .FirstOrDefault();
            if (lastVersion > 0)
            {
                var versionDir = Path.Combine(historyDir, lastVersion.ToString());

                MaybeCleanupNewDirtyFiles(versionDir);

                MaybeCleanupDeletedFiles(versionDir);

                MaybeCleanupModifiedFiles(versionDir);

                MaybeCleanupRenamedFiles(versionDir);

                MaybeRemoveEmptyListingFiles(versionDir);

                // Remove the whole version if it is empty
                if (!_fileSystem.EnumerateFiles(versionDir).Any())
                    _fileSystem.DeleteDirectory(versionDir);
            }
        }

        private void MaybeCleanupNewDirtyFiles(string versionDir)
        {
            var newFilesPath = Path.Combine(versionDir, "new.txt");
            if (_fileSystem.IsFileExists(newFilesPath))
            {
                var currentStateDir = Path.Combine(_targetForSource, "CurrentState");
                var correctFilesList = _fileSystem.ReadLines(newFilesPath)
                    .Where(x => _fileSystem.IsFileExists(Path.Combine(currentStateDir, x)))
                    .ToArray();

                // Adjust list of files to allign with actual files in current state
                _fileSystem.WriteLines(newFilesPath, correctFilesList);
            }
        }

        private void MaybeRemoveEmptyListingFiles(string versionDir)
        {
            foreach (var listingFile in new[] { "new.txt", "renamed.txt" })
            {
                var path = Path.Combine(versionDir, listingFile);
                if (_fileSystem.IsFileExists(path) && !_fileSystem.ReadLines(path).Any())
                    _fileSystem.DeleteFile(path);
            }
        }

        private void MaybeCleanupDeletedFiles(string versionDir)
        {
            var currentStateDir = Path.Combine(_targetForSource, "CurrentState");
            var deleteDir = Path.Combine(versionDir, "deleted");
            foreach (var file in _fileSystem.EnumerateFiles(deleteDir).ToArray())
            {
                var relativePath = file.Replace(deleteDir + "\\", "");
                var currentStatePath = Path.Combine(currentStateDir, relativePath);
                if (_fileSystem.IsFileExists(currentStatePath) &&
                    _fileSystem.AreEqualFiles(file, currentStatePath))
                    _fileSystem.DeleteFile(file);
            }
        }

        private void MaybeCleanupModifiedFiles(string versionDir)
        {
            var currentStateDir = Path.Combine(_targetForSource, "CurrentState");
            var modifiedDir = Path.Combine(versionDir, "modified");
            foreach (var file in _fileSystem.EnumerateFiles(modifiedDir).ToArray())
            {
                var relativePath = file.Replace(modifiedDir + "\\", "");
                var currentStatePath = Path.Combine(currentStateDir, relativePath);
                if (_fileSystem.IsFileExists(currentStatePath) &&
                    _fileSystem.AreEqualFiles(file, currentStatePath))
                    _fileSystem.DeleteFile(file);
            }
        }

        private void MaybeCleanupRenamedFiles(string versionDir)
        {
            var renamedFilesPath = Path.Combine(versionDir, "renamed.txt");
            if (_fileSystem.IsFileExists(renamedFilesPath))
            {
                var currentStateDir = Path.Combine(_targetForSource, "CurrentState");
                var renamedList = _fileSystem.ReadLines(renamedFilesPath)
                    .Select(x => Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(x))
                    .Select(x => new
                    {
                        oldName = x.Value<string>("oldName"),
                        newName = x.Value<string>("newName")
                    });
                var correctFilesList = renamedList
                    .Where(x => _fileSystem.IsFileExists(Path.Combine(currentStateDir, x.newName)) && 
                                !_fileSystem.IsFileExists(Path.Combine(currentStateDir, x.oldName)))
                    .ToArray();

                // Adjust list of files to allign with actual files in current state
                _fileSystem.WriteLines(renamedFilesPath, correctFilesList
                    .Select(x => new JObject(new JProperty("oldName", x.oldName), new JProperty("newName", x.newName)))
                    .Select(x => x.ToString(Newtonsoft.Json.Formatting.None))
                    .ToArray());
            }
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

        private void HandleRenamedFiles(string historyDir, FoldersDiff diff)
        {
            if (diff.RenamedFiles.Any())
            {
                var renamedFileName = Path.Combine(historyDir, "renamed.txt");
                _fileSystem.CreateFile(renamedFileName);
                _fileSystem.WriteLines(renamedFileName, diff.RenamedFiles
                    .Select(x => new JObject(new JProperty("oldName", x.OldName), new JProperty("newName", x.NewName)))
                    .Select(x => x.ToString(Newtonsoft.Json.Formatting.None))
                    .ToArray());
                Loop(diff.RenamedFiles, "Renaming files:", "Could not rename file: ", x => x.OldName, file =>
                {
                    var oldCurrentStateName = Path.Combine(_targetForSource, "CurrentState", file.OldName);
                    var newCurrentStateName = Path.Combine(_targetForSource, "CurrentState", file.NewName);
                    _fileSystem.RenameFile(oldCurrentStateName, newCurrentStateName);
                });
            }
        }

        private void HandleDeletedFiles(string historyDir, FoldersDiff diff)
        {
            if (diff.DeletedFiles.Any())
            {
                Loop(diff.DeletedFiles, "Copying deleted files:", "Could not copy deleted file: ", x => x.RelativeName, file =>
                {
                    var currentStatePath = Path.Combine(_targetForSource, "CurrentState", file.RelativeName);
                    var histrotyName = Path.Combine(historyDir, "deleted", file.RelativeName);
                    _fileSystem.Copy(currentStatePath, histrotyName);
                    _fileSystem.DeleteFile(currentStatePath);
                });
            }
        }

        private void HandleModifiedFiles(string historyDir, FoldersDiff diff)
        {
            if (diff.ModifiedFiles.Any())
            {
                var modifiedFilesDir = Path.Combine(historyDir, "modified");
                Loop(diff.ModifiedFiles, "Copying modified files:", "Could not copy modified file: ", x => x.RelativeName, file =>
                {
                    var fullName = file.PhysicalPath;
                    var currentStatePath = Path.Combine(_targetForSource, "CurrentState", file.RelativeName);
                    var historyPath = Path.Combine(historyDir, "modified", file.RelativeName);
                    _fileSystem.Copy(currentStatePath, historyPath);
                    _fileSystem.Copy(fullName, currentStatePath);
                });
            }
        }

        private void HandleNewFiles(string historyDir, FoldersDiff diff)
        {
            if (diff.NewFiles.Any())
            {
                var newFilesPath = Path.Combine(historyDir, "new.txt");
                _fileSystem.CreateFile(newFilesPath);                
                _fileSystem.WriteLines(newFilesPath, diff.NewFiles.Select(x => x.RelativeName).ToArray());

                Loop(diff.NewFiles, "Copying new files:", "Could not copy new file: ", x => x.RelativeName, file =>
                {
                    var fullName = file.PhysicalPath;
                    var currentStatePath = Path.Combine(_targetForSource, "CurrentState", file.RelativeName);
                    _fileSystem.Copy(fullName, currentStatePath);
                });
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
                BackupDirectory.CreateIniFile(sourceGuid, target, source, fs, machineID, "current_state");
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
