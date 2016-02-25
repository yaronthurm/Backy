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
}
