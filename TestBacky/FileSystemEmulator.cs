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

        public FileSystemEmulator(IEnumerable<EmulatorFile> files)
        {
            _files = files.ToList();
        }


        public void AppendLine(string filename, string line)
        {
            var file = _files.First(x => x.Name == filename);
            file.Lines.Add(line);
        }

        public void Copy(string sourceFileName, string destFileName)
        {
            var source = _files.First(x => x.Name == sourceFileName);
            _files.Add(new EmulatorFile(destFileName, source.LastModified));
        }

        public void CreateFile(string filename)
        {
            _files.Add(new EmulatorFile(filename));
        }

        public IEnumerable<string> EnumerateFiles(string directory)
        {
            return _files.Where(x => x.Name.StartsWith(directory)).Select(x => x.Name);
        }

        public bool AreEqualFiles(string pathToFile1, string pathToFile2)
        {
            var file1 = _files.First(x => x.Name == pathToFile1);
            var file2 = _files.First(x => x.Name == pathToFile2);
            var ret =
                file1.LastModified == file2.LastModified &&
                string.Join("\n", file1.Lines) == string.Join("\n", file2.Lines);
            return ret;
        }

        public IEnumerable<string> GetDirectories(string directory)
        {
            var ret = this.EnumerateFiles(directory).Select(Path.GetDirectoryName).Distinct().ToList();
            return ret;
        }

        public DateTime GetLastWriteTime(string fullname)
        {
            var file = _files.First(x => x.Name == fullname);
            return file.LastModified;
        }

        public void MakeDirectoryReadOnly(string dirName)
        {
            // TODO:
        }

        public IEnumerable<string> ReadLines(string filename)
        {
            var file = _files.First(x => x.Name == filename);
            return file.Lines;
        }        
    }

    public class EmulatorFile
    {
        public string Name;
        public List<string> Lines = new List<string>();
        public DateTime LastModified;

        public EmulatorFile(string name)
        {
            this.Name = name;
        }

        public EmulatorFile(string name, DateTime lastModified): this (name)
        {
            this.LastModified = lastModified;
        }
    }
}
