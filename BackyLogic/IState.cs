using System;
using System.Collections.Generic;

namespace BackyLogic
{
    public interface IState
    {
        IEnumerable<BackyFile> GetFiles();
        bool ContainsFile(string fileRelativePath);
        BackyFile FindFile(string fileRelativePath);
        string GetNextDirectory(IFileSystem fileSystem, string targetDir);
    }        
}
