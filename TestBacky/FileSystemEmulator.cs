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

        public FileSystemEmulator(IEnumerable<string> sourceFiles, IEnumerable<string> destinationFiles)
        {
            _sourceFiles = sourceFiles.ToList();
            _destinationFiles = destinationFiles.ToList();
        }

        public void Copy(string sourceFileName, string destFileName)
        {
            _destinationFiles.Add(destFileName);
        }

        public void CreateDirectory(string targetDir)
        {            
        }

        public IEnumerable<string> GetAllFiles(string directory)
        {
            return _sourceFiles.Union(_destinationFiles).Where(x => x.StartsWith(directory));
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
