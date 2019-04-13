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
            file.Content += line + Environment.NewLine;
        }

        public void Copy(string sourceFileName, string destFileName)
        {
            var source = _files.First(x => x.Name == sourceFileName);
            var clone = source.Clone();
            clone.Name = destFileName;
            _files.Add(clone);
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
                file1.Content == file2.Content;
            return ret;
        }

        public IEnumerable<string> GetTopLevelDirectories(string directory)
        {
            var files = this.EnumerateFiles(directory);
            var directories = files.Select(x => Path.GetDirectoryName(x));
            var depth = directory.Split('\\').Length;
            // Only take the next depth
            var ret = directories
                .Select(x => x.Split('\\'))
                .Where(x => x.Length > depth)
                .Select(x => x[depth])
                .Distinct()    
                .Select(x => Path.Combine(directory, x))            
                .ToList();
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
            var ret = file.Content.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            return ret;
        }

        public void AddFiles(EmulatorFile[] newFiles)
        {
            _files.AddRange(newFiles);
        }

        public void DeleteFile(string fullname)
        {
            var file = _files.First(x => x.Name == fullname);
            _files.Remove(file);
        }

        public void UpdateFile(string fullname, DateTime newLastModified, string newContent)
        {
            var file = _files.First(x => x.Name == fullname);
            file.LastModified = newLastModified;
            file.Content = newContent;
        }

        internal void RenameFile(string currentName, string newName)
        {
            var file = _files.First(x => x.Name == currentName);
            file.Name = newName;
        }

        internal void RenameDirectory(string currentName, string newName)
        {
            var files = _files.Where(x => x.Name.StartsWith(currentName));
            foreach (var file in files)
                file.Name = file.Name.Replace(currentName, newName);
        }

        public string FindFile(string dirName, string fileName)
        {
            var ret = _files.FirstOrDefault(x => Path.Combine(dirName, fileName).Equals(x.Name, StringComparison.OrdinalIgnoreCase));
            return ret?.Name;
        }

        public DateTime GetCreateTime(string rootDir)
        {
            return DateTime.MinValue;
        }

        public void SetCreateTime(string destination, DateTime dateTime)
        {
            // TODO: add logic when test are written for this functionality.
        }

        public void MarkDirectoryAsFullControl(string dirName)
        {
            //
        }

        public void DeleteDirectory(string dirName)
        {
            var files = _files.Where(x => x.Name.StartsWith(dirName)).ToList();
            foreach (var file in files)
                _files.Remove(file);
        }

        public bool IsDirectoryExist(string dirName)
        {
            throw new NotImplementedException();
        }
    }

    public class EmulatorFile
    {
        public string Name;
        public string Content;
        public DateTime LastModified;

        public EmulatorFile(string name, DateTime lastModified = new DateTime(), string content = "")
        {
            this.Name = name;
            this.LastModified = lastModified;
            this.Content = content;
        }

        public EmulatorFile Clone()
        {
            var ret = new EmulatorFile(this.Name, this.LastModified);
            ret.Content = this.Content;
            return ret;
        }


        public static EmulatorFile FromlFileName(string fullName, string relativeName, BackyLogic.IFileSystem fs)
        {
            var ret = new EmulatorFile(
                relativeName, 
                fs.GetLastWriteTime(fullName), 
                string.Join(Environment.NewLine, fs.ReadLines(fullName)));
            return ret;
        }
    }
}
