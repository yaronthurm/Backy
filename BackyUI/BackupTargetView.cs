using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Shell;

namespace Backy
{
    public partial class BackupTargetView : UserControl
    {
        public BackupTargetView()
        {
            InitializeComponent();

            this.lblPath.Text = null;
        }

        internal void SetDirectory(string selectedPath)
        {
            var shellFile = ShellFolder.FromParsingName(selectedPath);
            shellFile.Thumbnail.FormatOption = ShellThumbnailFormatOption.Default;
            Bitmap thumbnail = shellFile.Thumbnail.MediumBitmap;
            thumbnail.MakeTransparent();
            this.pictureBox1.Image = thumbnail;
            this.lblPath.Text = selectedPath;
        }

        public string Path
        {
            get { return this.lblPath.Text; }
        }
    }
}
