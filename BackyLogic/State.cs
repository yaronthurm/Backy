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
            var firstLevel = GetFirstLevelDirectories(fileSystem, targetDir);

            // Get highest number
            var max = firstLevel.Union(new[] { "0" }).Select(int.Parse).Max();
            return (max + 1).ToString();
        }

        public static IEnumerable<string> GetFirstLevelDirectories(IFileSystem fileSystem, string targetDir)
        {
            // Get all directories in target
            var dirs = fileSystem.GetDirectories(targetDir);

            // Get just first level directories
            if (!targetDir.EndsWith("\\")) targetDir += "\\";
            var ret = dirs.Select(x => x.Replace(targetDir, "")).Select(x => x.Split('\\')[0]).Distinct();

            return ret;
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
            ret.SerialNumber = int.Parse(System.IO.Path.GetFileName(rootDir));

            ret.New = fileNames
                .Where(x => x.StartsWith(System.IO.Path.Combine(rootDir, "new")))
                .Select(x => BackyFile.FromTargetFileName(fileSystem, x, rootDir + "\\new")).ToList();
            ret.Modified = fileNames
                .Where(x => x.StartsWith(System.IO.Path.Combine(rootDir, "modified")))
                .Select(x => BackyFile.FromTargetFileName(fileSystem, x, rootDir + "\\modified")).ToList();

            var deletedName = System.IO.Path.Combine(rootDir, "deleted.txt");
            if (fileNames.Contains(deletedName)) {
                ret.Deleted = fileSystem.ReadLines(deletedName)
                    .Select(x => Newtonsoft.Json.Linq.JObject.Parse(x))
                    .Select(x => x.Value<string>("filename")).ToList();
            }
            else
                ret.Deleted = new List<string>();

            var renamedName = System.IO.Path.Combine(rootDir, "renamed.txt");
            if (fileNames.Contains(renamedName))
            {
                ret.Renamed = fileSystem.ReadLines(renamedName)
                    .Select(x => Newtonsoft.Json.Linq.JObject.Parse(x))
                    .Select(x => new RenameInfo { OldName = x.Value<string>("oldname"), NewName = x.Value<string>("newname") }).ToList();
            }
            else
                ret.Renamed = new List<RenameInfo>();

            return ret;
        }
    }

    public class RenameInfo
    {
        public string OldName;
        public string NewName;
    }
}
