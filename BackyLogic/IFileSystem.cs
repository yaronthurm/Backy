using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace BackyLogic
{
    public interface IFileSystem
    {
        void CreateFile(string filename);

        void Copy(string sourceFileName, string destFileName);

        IEnumerable<string> EnumerateFiles(string dirName);

        IEnumerable<string> GetDirectories(string dirName);

        DateTime GetLastWriteTime(string fullname);

        IEnumerable<string> ReadLines(string fullname);

        void AppendLine(string filename, string line);

        bool AreEqualFiles(string pathToFile1, string pathToFile2);

        void MakeDirectoryReadOnly(string dirName);
    }


    public class FileSystem : IFileSystem
    {
        public void AppendLine(string filename, string line)
        {
            File.AppendAllLines(filename, new[] { line });
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

        public IEnumerable<string> GetDirectories(string target)
        {
            return Directory.GetDirectories(target);
        }

        public DateTime GetLastWriteTime(string fullname)
        {
            return File.GetLastWriteTime(fullname);
        }

        public void MakeDirectoryReadOnly(string dirName)
        {
            DirectoryInfo dInfo = new DirectoryInfo(dirName);
            DirectorySecurity dSecurity = dInfo.GetAccessControl();
            dSecurity.SetAccessRuleProtection(true, false); // Disable inheritance
            dSecurity.AddAccessRule(
                new FileSystemAccessRule(
                    "Everyone",
                    FileSystemRights.ReadAndExecute | FileSystemRights.Traverse,
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
    }


    public interface IMultiStepProgress
    {
        void StartStepWithoutProgress(string text);
        void StartUnboundedStep(string text, Func<int, string> projection = null);
        void StartBoundedStep(string text, int maxValue);
        void UpdateProgress(int currentValue);
        void Increment();
    }
}
