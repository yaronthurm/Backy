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
        private List<EmulatorFile> _files;

        public FileSystemEmulator(IEnumerable<string> files)
        {
            _files = files.Select(x => new EmulatorFile(x)).ToList();
        }


        public void AppendLine(string filename, string line)
        {
            var file = _files.First(x => x.Name == filename);
            file.Lines.Add(line);
        }

        public void Copy(string sourceFileName, string destFileName)
        {
            _files.Add(new EmulatorFile(destFileName));
        }

        public void CreateFile(string filename)
        {
            _files.Add(new EmulatorFile(filename));
        }

        public IEnumerable<string> GetAllFiles(string directory)
        {
            return _files.Where(x => x.Name.StartsWith(directory)).Select(x => x.Name);
        }

        public IEnumerable<string> GetDirectories(string directory)
        {
            var ret = this.GetAllFiles(directory).Select(Path.GetDirectoryName).Distinct().ToList();
            return ret;
        }

        public DateTime GetLastWriteTime(string fullname)
        {
            return new DateTime(2016, 1, 13, 13, 0, 0);
        }

        public IEnumerable<string> ReadLines(string filename)
        {
            var file = _files.First(x => x.Name == filename);
            return file.Lines;
        }



        class EmulatorFile
        {
            public string Name;
            public List<string> Lines = new List<string>();

            public EmulatorFile(string name)
            {
                this.Name = name;
            }
        }
    }
}
