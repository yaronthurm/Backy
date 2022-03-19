using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackyLogic
{

    public class BackyFolder
    {
        public int SerialNumber;
        public List<BackyFile> New;
        public List<BackyFile> Modified;
        public List<string> Deleted;
        public List<RenameInfo> Renamed;
        public DateTime DateCreated;


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
        ///             deleted.txt (each row contains the name of the deleted file)
        ///             renamed.txt (each row looks like that: {oldName: "relative path before rename", newName: "relative name after rename"})
        /// </summary>
        /// <returns></returns>
        public static BackyFolder FromFileNames(IFileSystem fileSystem, IEnumerable<string> fileNames, string rootDir)
        {
            // rootDir is expected to be in the format d:\target\1
            var ret = new BackyFolder();
            ret.SerialNumber = int.Parse(System.IO.Path.GetFileName(rootDir));
            ret.DateCreated = fileSystem.GetCreateTime(rootDir);

            var newName = System.IO.Path.Combine(rootDir, "new.txt");
            if (fileNames.Contains(newName)) // Support for reading shallow files
            {
                ret.New = fileSystem.ReadLines(newName)
                    .Select(x => Newtonsoft.Json.Linq.JObject.Parse(x))
                    .Select(x => BackyFile.FromShallowData(x.Value<string>("name"), rootDir + "\\new", x.Value<DateTime>("lastWrite")))
                    .ToList();
            }
            else
            {
                ret.New = fileNames
                    .Where(x => x.StartsWith(System.IO.Path.Combine(rootDir, "new\\")))
                    .Select(x => BackyFile.FromTargetFileName(fileSystem, x, rootDir + "\\new")).ToList();
            }

            var modifiedName = System.IO.Path.Combine(rootDir, "modified.txt");
            if (fileNames.Contains(modifiedName)) // Support for reading shallow files
            {
                ret.Modified = fileSystem.ReadLines(modifiedName)
                    .Select(x => Newtonsoft.Json.Linq.JObject.Parse(x))
                    .Select(x => BackyFile.FromShallowData(x.Value<string>("name"), rootDir + "\\modified", x.Value<DateTime>("lastWrite")))
                    .ToList();
            }
            else
            {
                ret.Modified = fileNames
                    .Where(x => x.StartsWith(System.IO.Path.Combine(rootDir, "modified\\")))
                    .Select(x => BackyFile.FromTargetFileName(fileSystem, x, rootDir + "\\modified")).ToList();
            }

            var deletedName = System.IO.Path.Combine(rootDir, "deleted.txt");
            if (fileNames.Contains(deletedName)) {
                ret.Deleted = fileSystem.ReadLines(deletedName).ToList();
            }
            else
                ret.Deleted = new List<string>();

            var renamedName = System.IO.Path.Combine(rootDir, "renamed.txt");
            if (fileNames.Contains(renamedName))
            {
                ret.Renamed = fileSystem.ReadLines(renamedName)
                    .Select(x => Newtonsoft.Json.Linq.JObject.Parse(x))
                    .Select(x => new RenameInfo { OldName = x.Value<string>("oldName"), NewName = x.Value<string>("newName") }).ToList();
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
