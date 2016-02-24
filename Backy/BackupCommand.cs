using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backy
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
            foreach (var newFile in newFiles)
            {
                _fileSystem.Copy(newFile.FullName, System.IO.Path.Combine(targetDir, newFile.Name));
            }
        }

        private string GetTargetDirectoryForNewFiles(State lastBackedupState)
        {
            var ret = System.IO.Path.Combine(_target, lastBackedupState.GetNextDirectory(), "new");
            return ret;
        }

        private IEnumerable<FileForBackup> GetNewFiles(State currentState, State lastBackedupState)
        {
            var ret = new List<FileForBackup>();
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
            throw new NotImplementedException();
        }

        private State GetCurrentState()
        {
            var files = _fileSystem.GetAllFiles(_source);
                
            var ret = new State();
            ret.Files = files.Select(FileForBackup.FromFullFileName).ToList();
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
