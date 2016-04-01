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
    public partial class BackupSourceView : UserControl
    {
        public BackupSourceView()
        {
            InitializeComponent();
        }

        internal void SetDirectory(string selectedPath)
        {
            var shellFile = ShellFolder.FromParsingName(selectedPath);
            shellFile.Thumbnail.FormatOption = ShellThumbnailFormatOption.Default;
            Bitmap thumbnail = shellFile.Thumbnail.MediumBitmap;
            thumbnail.MakeTransparent();
            this.pictureBox1.Image = thumbnail;
            this.lblFileName.Text = selectedPath;
        }
    }
}
