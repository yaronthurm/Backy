using System;
using System.Collections.Generic;

namespace BackyLogic
{
    public interface IFileSystem
    {
        void CreateFile(string filename);

        void DeleteFile(string filename);

        void Copy(string sourceFileName, string destFileName);

        IEnumerable<string> EnumerateFiles(string dirName);

        string FindFile(string dirName, string fileName);

        IEnumerable<string> GetTopLevelDirectories(string dirName);

        DateTime GetLastWriteTime(string fullname);

        IEnumerable<string> ReadLines(string fullname);

        void AppendLine(string filename, string line);

        bool AreEqualFiles(string pathToFile1, string pathToFile2);

        void MakeDirectoryReadOnly(string dirName);

        void MarkDirectoryAsFullControl(string dirName);

        void DeleteDirectory(string dirName);

        DateTime GetCreateTime(string rootDir);

        void SetCreateTime(string destination, DateTime dateTime);

        bool IsDirectoryExist(string dirName);
    }

}
