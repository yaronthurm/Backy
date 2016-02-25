using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackyLogic
{
    public class State
    {
        public List<BackyFile> Files = new List<BackyFile>();

        internal string GetNextDirectory(IFileSystem fileSystem, string targetDir)
        {
            // Get all directories in target
            var dirs = fileSystem.GetDirectories(targetDir);

            // Get just first level directories
            var firstLevel = dirs.Select(x => x.Replace(targetDir, "")).Select(x => System.IO.Path.GetPathRoot(x));

            // Get highest number
            var max = firstLevel.Union(new[] { "0" }).Select(int.Parse).Max();
            return (max + 1).ToString();
        }
    }


    public class BackyFolder
    {
        public int SerialNumber;
        public List<BackyFile> New;
        public List<BackyFile> Modified;
        public List<string> Deleted;
        public List<RenameInfo> Renamed;


        /// <summary>
        /// we expect a structuor like this:
        /// d:\
        ///     target\
        ///         1\
        ///             new\
        ///                 file1, file2, file3
        ///                 subfolder\
        ///                     file1, file2
        ///             modified\
        ///                 file4, file5
        ///                 subfolder\
        ///                     file6, file7
        ///             deleted.txt (each row looks like that: {filename: "full name of deleted file"})
        ///             renamed.txt (each row looks like that: {oldname: "full name of file before rename", newname: "full name of file after rename"})
        /// </summary>
        /// <returns></returns>
        public static BackyFolder FromFileNames(IFileSystem fileSystem, IEnumerable<string> fileNames, string rootDir)
        {
            // rootDir is expected to be in the format d:\target\1
            var ret = new BackyFolder();
            ret.New = fileNames
                .Where(x => x.StartsWith(System.IO.Path.Combine(rootDir, "new")))
                .Select(x => BackyFile.FromFullFileName(fileSystem, x)).ToList();
            ret.New = fileNames
                .Where(x => x.StartsWith(System.IO.Path.Combine(rootDir, "modified")))
                .Select(x => BackyFile.FromFullFileName(fileSystem, x)).ToList();
            return null;
        }
    }

    public class RenameInfo
    {
        public string OldName;
        public string NewName;
    }
}
