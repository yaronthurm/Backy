using Microsoft.WindowsAPICodePack.Shell;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Backy
{
    public partial class PanelTest : Form
    {
        private FileView[] _files;

        public PanelTest()
        {
            InitializeComponent();
            var lines = File.ReadAllLines("D:\\back_test_file1.txt");
            var fileViewsList = new List<FileView>();
            for (int i = 0; i < lines.Length; i+=2)            
                fileViewsList.Add(new FileView { LogicalPath = lines[i], PhysicalPath = lines[i + 1] });
            _files = fileViewsList.ToArray();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var sw = Stopwatch.StartNew();
            this.flowPanelTest1.SetData((int)this.numItems.Value, TimeSpan.FromMilliseconds((int)this.loadTime.Value));
            sw.Stop();
            this.lblTime.Text = sw.Elapsed.ToString();
        }

        private void btnLoadPictures_Click(object sender, EventArgs e)
        {
            var sw = Stopwatch.StartNew();
            this.flowPanelTest1.SetRealData(_files);
            sw.Stop();
            this.lblTime.Text = sw.Elapsed.ToString();
        }        
    }
}
