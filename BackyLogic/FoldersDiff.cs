using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackyLogic
{
    public class FoldersDiff
    {
        public List<BackyFile> NewFiles;
        public List<BackyFile> ModifiedFiles;
        public List<BackyFile> DeletedFiles;
        public List<RenameInfo> RenamedFiles;

        public static FoldersDiff GetDiff(IFileSystem fileSystem, State currentState, State lastBackedupState)
        {
            var ret = new FoldersDiff();

            // First we need to take all the new and deleted files and check whether those are actually renames.
            var newFiles = GetNewFiles(currentState, lastBackedupState);
            var deletedFiles = GetDeletedFiles(currentState, lastBackedupState);
            var renames = GetRenamedFiles(fileSystem, newFiles, deletedFiles);

            ret.RenamedFiles = renames;
            ret.NewFiles = GetNewFilesMinusRenames(newFiles, renames);
            ret.ModifiedFiles = GetModifiedFiles(currentState, lastBackedupState);
            ret.DeletedFiles = GetDeletedFilesMinusRenames(deletedFiles, renames);
            return ret;
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

        private static List<RenameInfo> GetRenamedFiles(IFileSystem fileSystem, List<BackyFile> newFiles, List<BackyFile> deletedFiles)
        {
            // First, for each deleted file, look for a new file with the same modified date
            var renameSuspects = new[] { new { OldFile = (BackyFile)null, NewFile = (BackyFile)null } }.Skip(1).ToList();
            foreach (var deleted in deletedFiles)
            {
                var matchingFile = newFiles.FirstOrDefault(x => x.LastWriteTime == deleted.LastWriteTime);
                if (matchingFile != null)
                {
                    renameSuspects.Add(new { OldFile = deleted, NewFile = matchingFile });
                }
            }

            var ret = new List<RenameInfo>();
            // For each suspect, check the content of both files
            foreach (var suspect in renameSuspects)
            {
                byte[] deletedContent = fileSystem.GetContent(suspect.OldFile.PhysicalPath);
                byte[] newContent = fileSystem.GetContent(suspect.NewFile.PhysicalPath);
                if (Enumerable.SequenceEqual(deletedContent, newContent))
                    ret.Add(new RenameInfo { OldName = suspect.OldFile.RelativeName, NewName = suspect.NewFile.RelativeName });
            }

            return ret;
        }

        private static List<BackyFile> GetNewFiles(State currentState, State lastBackedupState)
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
        
        private static List<BackyFile> GetModifiedFiles(State currentState, State lastBackedupState)
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

        private static List<BackyFile> GetDeletedFiles(State currentState, State lastBackedupState)
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
    }
}
