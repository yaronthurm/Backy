using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace BackyLogic
{

    public class OSFileSystem : IFileSystem
    {
        public void AppendLines(string filename, params string[] lines)
        {
            File.AppendAllLines(filename, lines);
        }

        public void WriteLines(string filename, params string[] lines)
        {
            File.WriteAllLines(filename, lines);
        }

        public void Copy(string sourceFileName, string destFileName)
        {
            if (!Directory.Exists(Path.GetDirectoryName(destFileName)))
                Directory.CreateDirectory(Path.GetDirectoryName(destFileName));

            File.Copy(sourceFileName, destFileName);
        }

        public void CreateFile(string filename)
        {
            if (!Directory.Exists(Path.GetDirectoryName(filename)))
                Directory.CreateDirectory(Path.GetDirectoryName(filename));

            File.WriteAllText(filename, "");
        }

        public IEnumerable<string> EnumerateFiles(string source)
        {
            return Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories);
        }

        public IEnumerable<string> GetTopLevelDirectories(string target)
        {
            return Directory.GetDirectories(target);
        }

        public DateTime GetLastWriteTime(string fullname)
        {
            var fileInfo = SafeGetFileInfo(fullname);
            return fileInfo.LastWriteTime;
        }

        private static FileInfo SafeGetFileInfo(string fullname)
        {
            if (fullname.Length < 260)
                return new FileInfo(fullname);

            int indexOf = fullname.LastIndexOf("\\");
            var dirname = fullname.Substring(0, indexOf);
            var filename = fullname.Substring(indexOf + 1, fullname.Length - indexOf - 1);
            DirectoryInfo dirInfo = SafeGetDirectoryInfo(dirname);
            var ret = dirInfo.EnumerateFiles().First(x => x.Name == filename);
            return ret;
        }

        private static DirectoryInfo SafeGetDirectoryInfo(string fullname)
        {
            if (fullname.Length < 260)
                return new DirectoryInfo(fullname);

            int indexOf = fullname.LastIndexOf("\\");
            var parentDirName = fullname.Substring(0, indexOf);
            var dirname = fullname.Substring(indexOf + 1, fullname.Length - indexOf - 1);
            DirectoryInfo dirInfo = SafeGetDirectoryInfo(dirname);
            var ret = dirInfo.EnumerateDirectories().First(x => x.Name == dirname);
            return ret;
        }

        public void MakeDirectoryReadOnly(string dirName)
        {
            if (!Directory.Exists(dirName)) return;

            DirectoryInfo dInfo = new DirectoryInfo(dirName);
            DirectorySecurity dSecurity = dInfo.GetAccessControl();
            dSecurity.SetAccessRuleProtection(true, false); // Disable inheritance
            dSecurity.ResetAccessRule(
                new FileSystemAccessRule(
                    "Everyone",
                    FileSystemRights.ReadAndExecute | FileSystemRights.Traverse,
                    InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit,
                    PropagationFlags.None, AccessControlType.Allow));
            dInfo.SetAccessControl(dSecurity);
        }

        public void MarkDirectoryAsFullControl(string dirName)
        {
            if (!Directory.Exists(dirName)) return;

            DirectoryInfo dInfo = new DirectoryInfo(dirName);
            DirectorySecurity dSecurity = dInfo.GetAccessControl();
            dSecurity.SetAccessRuleProtection(true, false); // Disable inheritance
            dSecurity.ResetAccessRule(
                new FileSystemAccessRule(
                    "Everyone",
                    FileSystemRights.FullControl,
                    InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit,
                    PropagationFlags.None, AccessControlType.Allow));
            dInfo.SetAccessControl(dSecurity);
        }

        public IEnumerable<string> ReadLines(string filename)
        {
            var ret = File.ReadAllLines(filename);
            return ret;
        }

        public bool AreEqualFiles(string pathToFile1, string pathToFile2)
        {
            var file1 = new FileInfo(pathToFile1);
            var file2 = new FileInfo(pathToFile2);

            if (!file1.Exists || !file2.Exists)
                return false;

            // Check size
            if (file1.Length != file2.Length)
                return false;

            // Check last write
            if (file1.LastWriteTime != file2.LastWriteTime)
                return false;

            // Check content
            using (var fs1 = file1.OpenRead())
            using (var fs2 = file2.OpenRead())
            {
                while (true)
                {
                    int data1 = fs1.ReadByte();
                    int data2 = fs2.ReadByte();
                    if (data1 != data2)
                        return false;
                    if (data1 == -1) // End of stream reached
                        return true;
                    // else - continue
                }
            }
        }

        public string FindFile(string dirName, string fileName)
        {
            var dir = new DirectoryInfo(dirName);
            var file = dir.GetFiles(fileName).FirstOrDefault();
            return file?.FullName;
        }

        public DateTime GetCreateTime(string rootDir)
        {
            var ret = Directory.GetCreationTimeUtc(rootDir);
            return ret;
        }

        public void SetCreateTime(string destination, DateTime dateTime)
        {
            Directory.SetCreationTimeUtc(destination, dateTime);
        }

        public void DeleteFile(string filename)
        {
            File.Delete(filename);
        }

        public void DeleteDirectory(string dirName)
        {
            Directory.Delete(dirName, true);
        }

        public void RenameDirectory(string currName, string newName)
        {
            Directory.Move(currName, newName);
        }

        public void RenameFile(string currName, string newName)
        {
            File.Move(currName, newName);
        }

        public bool IsDirectoryExist(string dirName)
        {
            return Directory.Exists(dirName);
        }

        public bool IsFileExists(string filename)
        {
            return File.Exists(filename);
        }
    }


}
