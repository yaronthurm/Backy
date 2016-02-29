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

        public IEnumerable<string> GetAllFiles(string directory)
        {
            return _files.Where(x => x.Name.StartsWith(directory)).Select(x => x.Name);
        }

        public byte[] GetContent(string physicalPath)
        {
            var file = _files.First(x => x.Name == physicalPath);
            var ret = string.Join(Environment.NewLine, file.Lines) ?? "";
            return Encoding.UTF8.GetBytes(ret);
        }

        public IEnumerable<string> GetDirectories(string directory)
        {
            var ret = this.GetAllFiles(directory).Select(Path.GetDirectoryName).Distinct().ToList();
            return ret;
        }

        public DateTime GetLastWriteTime(string fullname)
        {
            var file = _files.First(x => x.Name == fullname);
            return file.LastModified;
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
