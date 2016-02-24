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
