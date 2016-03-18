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

        byte[] GetContent(string physicalPath);

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

        public byte[] GetContent(string physicalPath)
        {
            var ret = File.ReadAllBytes(physicalPath);
            return ret;
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
