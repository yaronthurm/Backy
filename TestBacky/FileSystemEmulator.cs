using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestBacky
{
    public class FileSystemEmulator : BackyLogic.IFileSystem
    {
        private List<string> _sourceFiles;
        private List<string> _destinationFiles;

        public FileSystemEmulator(List<string> sourceFiles, List<string> destinationFiles)
        {
            _sourceFiles = sourceFiles;
            _destinationFiles = destinationFiles;
        }

        public void Copy(string sourceFileName, string destFileName)
        {
            _destinationFiles.Add(destFileName);
        }

        public void CreateDirectory(string targetDir)
        {            
        }

        public IEnumerable<string> GetAllFiles(string source)
        {
            return _sourceFiles.Where(x => x.StartsWith(source));
        }

        public IEnumerable<string> GetDirectories(string target)
        {
            var ret = _destinationFiles.Select(Path.GetDirectoryName).Distinct().ToList();
            return ret;
        }

        public DateTime GetLastWriteTime(string fullname)
        {
            return new DateTime(2016, 1, 13, 13, 0, 0);
        }
    }
}
