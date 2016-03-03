using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackyLogic
{
    public class FoldersDiff
    {
        IFileSystem _fileSystem;
        State _currentState;
        State _lastBackedupState;


        public List<BackyFile> NewFiles;
        public List<BackyFile> ModifiedFiles;
        public List<BackyFile> DeletedFiles;
        public List<RenameInfo> RenamedFiles;

        public event Action<DiffProgress> OnProgress;
        private DiffProgress _progress = new DiffProgress();

        private FoldersDiff(IFileSystem fileSystem, State currentState, State lastBackedupState)
        {
            _fileSystem = fileSystem;
            _currentState = currentState;
            _lastBackedupState = lastBackedupState;
        }


        public static FoldersDiff GetDiff(IFileSystem fileSystem, State currentState, State lastBackedupState, Action<DiffProgress> OnProgress)
        {
            var ret = new FoldersDiff(fileSystem, currentState, lastBackedupState);
            ret.OnProgress += OnProgress;
            ret.CalculateDiff();
            return ret;
        }

        private void RaiseOnProgress()
        {
            if (OnProgress != null)
                OnProgress(_progress);
        }

        private void CalculateDiff()
        {
            // First we need to take all the new and deleted files and check whether those are actually renames.
            var newFiles = GetNewFiles();
            var deletedFiles = GetDeletedFiles();
            var renames = DetectRenamedFiles(newFiles, deletedFiles);

            this.RenamedFiles = renames;
            this.NewFiles = GetNewFilesMinusRenames(newFiles, renames);
            this.ModifiedFiles = GetModifiedFiles();
            this.DeletedFiles = GetDeletedFilesMinusRenames(deletedFiles, renames);
        }

        private static List<BackyFile> GetDeletedFilesMinusRenames(List<BackyFile> deletedFiles, List<RenameInfo> renames)
        {
            var deletedFilesThatAreActualyRenames = renames.Select(x => x.OldName).ToList();
            var ret = deletedFiles.Where(x => !deletedFilesThatAreActualyRenames.Contains(x.RelativeName)).ToList();
            return ret;
        }

        private static List<BackyFile> GetNewFilesMinusRenames(List<BackyFile> newFiles, List<RenameInfo> renames)
        {
            var newFilesThatAreActualyRenames = renames.Select(x => x.NewName).ToList();
            var ret = newFiles.Where(x => !newFilesThatAreActualyRenames.Contains(x.RelativeName)).ToList();
            return ret;
        }

        private List<RenameInfo> DetectRenamedFiles(List<BackyFile> newFiles, List<BackyFile> deletedFiles)
        {
            // First, for each deleted file, look for a new file with the same modified date
            var renameSuspects = new[] { new { OldFile = (BackyFile)null, NewFile = (BackyFile)null } }.Skip(1).ToList();
            foreach (var deleted in deletedFiles)
            {
                var matchingFile = newFiles.FirstOrDefault(x => x.LastWriteTime == deleted.LastWriteTime);
                if (matchingFile != null)
                {
                    renameSuspects.Add(new { OldFile = deleted, NewFile = matchingFile });

                    // Remove the matched file from the list
                    newFiles = newFiles.Where(x => x != matchingFile).ToList();
                }
            }

            _progress.RenameDetectionTotal = renameSuspects.Count;

            var ret = new List<RenameInfo>();
            // For each suspect, check the content of both files
            foreach (var suspect in renameSuspects)
            {
                byte[] deletedContent = _fileSystem.GetContent(suspect.OldFile.PhysicalPath);
                byte[] newContent = _fileSystem.GetContent(suspect.NewFile.PhysicalPath);
                if (Enumerable.SequenceEqual(deletedContent, newContent))
                    ret.Add(new RenameInfo { OldName = suspect.OldFile.RelativeName, NewName = suspect.NewFile.RelativeName });
                _progress.RenameDetectionFinished++;
                RaiseOnProgress();
            }

            return ret;
        }

        private List<BackyFile> GetNewFiles()
        {
            var ret = new List<BackyFile>();
            foreach (var file in _currentState.Files)
            {
                if (_lastBackedupState.Files.Any(x => x.RelativeName == file.RelativeName))
                    // not new
                    continue;

                ret.Add(file);
            }
            return ret;
        }
        
        private List<BackyFile> GetModifiedFiles()
        {
            var ret = new List<BackyFile>();
            foreach (var file in _currentState.Files)
            {
                // look for file in backup
                var backupfile = _lastBackedupState.Files.FirstOrDefault(x => x.RelativeName == file.RelativeName);
                if (backupfile == null || backupfile.LastWriteTime == file.LastWriteTime)
                    continue;

                ret.Add(file);
            }
            return ret;
        }

        private List<BackyFile> GetDeletedFiles()
        {
            var ret = new List<BackyFile>();
            foreach (var file in _lastBackedupState.Files)
            {
                // look for file in current
                var currentFile = _currentState.Files.FirstOrDefault(x => x.RelativeName == file.RelativeName);
                if (currentFile == null)
                    ret.Add(file);
            }
            return ret;
        }
    }
}
