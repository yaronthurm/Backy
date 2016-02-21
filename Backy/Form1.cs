using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Backy
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            var backupCommand = new RunBackupCommand(this.txtSource.Text, this.txtTarget.Text);
            backupCommand.Execute();
        }
    }


    public class RunBackupCommand
    {
        private string _source;
        private string _target;

        public RunBackupCommand(string source, string target)
        {
            _source = source;
            _target = target;
        }


        public void Execute()
        {
            if (IsFirstTime())
            {
                RunFirstTimeBackup();
                return;
            }

            State currentState = GetCurrentState();
            State lastBackedupState = GetLastBackedUpState();

            if (NoChangesFromLastBackup(currentState, lastBackedupState))
                return;

            CopyAllNewFiles(currentState, lastBackedupState);
            CopyAllUpdatedFiles(currentState, lastBackedupState);
            MarkAllDeletedFiles(currentState, lastBackedupState);
        }

        private void MarkAllDeletedFiles(State currentState, State lastBackedupState)
        {
            throw new NotImplementedException();
        }

        private void CopyAllUpdatedFiles(State currentState, State lastBackedupState)
        {
            throw new NotImplementedException();
        }

        private void CopyAllNewFiles(State currentState, State lastBackedupState)
        {
            var newFiles = GetNewFiles(currentState, lastBackedupState);
            var targetDir = GetTargetDirectoryForNewFiles(lastBackedupState);
            Directory.CreateDirectory(targetDir);
            foreach (var newFile in newFiles)
            {
                File.Copy(newFile.FullName, Path.Combine(targetDir, newFile.Name));
            }
        }

        private string GetTargetDirectoryForNewFiles(State lastBackedupState)
        {
            var ret = Path.Combine(_target, lastBackedupState.GetNextDirectory(), "new");
            return ret;
        }

        private IEnumerable<FileForBackup> GetNewFiles(State currentState, State lastBackedupState)
        {
            var ret = new List<FileForBackup>();
            foreach (var file in currentState.Files)
            {
                if (lastBackedupState.Files.Any(x => x.FullName == file.FullName))
                    // not new
                    continue;
                ret.Add(file);
            }
            return ret;
        }

        private bool NoChangesFromLastBackup(State currentState, State lastBackedupState)
        {
            throw new NotImplementedException();
        }

        private State GetLastBackedUpState()
        {
            throw new NotImplementedException();
        }

        private State GetCurrentState()
        {
            var files = Directory.GetFiles(_source, "*", SearchOption.AllDirectories);
            var ret = new State();
            ret.Files = files.Select(FileForBackup.FromFullFileName).ToList();
            return ret;
        }

        private void RunFirstTimeBackup()
        {
            CopyAllNewFiles(GetCurrentState(), new State());
        }

        private bool IsFirstTime()
        {
            // We expect to see folders in the target directory. If we don't see we assume it's the first time
            var dirs = Directory.GetDirectories(_target);
            var ret = dirs.Length == 0;
            return ret;
        }
    }
        

    public class State
    {
        public List<FileForBackup> Files = new List<FileForBackup>();

        internal string GetNextDirectory()
        {
            return "2";
        }
    }

    public class FileForBackup
    {
        public string Name;
        public string FullName;
        public DateTime LastWriteTime;

        private FileForBackup() { }

        public static FileForBackup FromFullFileName(string fullname)
        {
            var ret = new FileForBackup();
            ret.FullName = fullname;
            ret.LastWriteTime = File.GetLastWriteTime(fullname);
            ret.Name = Path.GetFileName(fullname);
            return ret;
        }
    }
}
