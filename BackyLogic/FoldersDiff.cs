using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackyLogic
{
    public class FoldersDiff
    {
        public IEnumerable<BackyFile> NewFiles;
        public IEnumerable<BackyFile> ModifiedFiles;
        public IEnumerable<string> DeletedFiles;
        public IEnumerable<RenameInfo> RenamedFiles;
    }
}
