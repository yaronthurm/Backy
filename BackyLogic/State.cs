using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackyLogic
{
    public class State
    {
        private HierarchicalDictionary<string, BackyFile> _tree = new HierarchicalDictionary<string, BackyFile>();

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

        public static State GetLastBackedUpState(IFileSystem fileSystem, string target, Action fileEnumaretedCallback)
        {
            var stateCalculator = new StateCalculator(fileSystem, target);
            stateCalculator.OnProgress += fileEnumaretedCallback;
            var ret = stateCalculator.GetLastState();
            return ret;
        }

        public static State GetCurrentState(IFileSystem fileSystem, string source, Action fileEnumaretedCallback)
        {
            var files = fileSystem.EnumerateFiles(source);

            var ret = new State();
            foreach (var file in files)
            {
                var backy = BackyFile.FromSourceFileName(fileSystem, file, source);
                ret.AddFile(backy);
                fileEnumaretedCallback();
            }
            return ret;
        }



        public IEnumerable<BackyFile> GetFiles()
        {
            var ret = _tree.GetAllItems();
            return ret;
        }

        public void AddFile(BackyFile file)
        {
            _tree.Add(file, file.RelativeName.Split('\\'));
        }

        public bool ContainsFile(string fileRelativePath)
        {
            var keys = fileRelativePath.Split('\\');
            var ret = _tree.Contains(keys);
            return ret;
        }

        public void DeleteFileByPath(string relativePath)
        {
            _tree.Remove(relativePath.Split('\\'));
        }

        public void DeleteFile(BackyFile file)
        {
            _tree.Remove(file.RelativeName.Split('\\'));
        }

        public BackyFile FindFile(string fileRelativePath)
        {
            return _tree.GetFileOrDefault(fileRelativePath.Split('\\'));
        }
    }

}
