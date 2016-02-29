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

        public static FoldersDiff GetDiff(State currentState, State lastBackedupState)
        {
            var ret = new FoldersDiff();
            ret.NewFiles = GetNewFiles(currentState, lastBackedupState);
            ret.ModifiedFiles = GetModifiedFiles(currentState, lastBackedupState);
            ret.DeletedFiles = GetDeletedFiles(currentState, lastBackedupState);
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
