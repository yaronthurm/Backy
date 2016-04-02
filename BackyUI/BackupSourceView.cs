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
        private string _path;

        public event Action<BackupSourceView> OnRemoveClick;

        public BackupSourceView()
        {
            InitializeComponent();
        }

        internal void SetDirectory(string selectedPath)
        {
            _path = selectedPath;
            var shellFile = ShellFolder.FromParsingName(selectedPath);
            shellFile.Thumbnail.FormatOption = ShellThumbnailFormatOption.Default;
            Bitmap thumbnail = shellFile.Thumbnail.MediumBitmap;
            thumbnail.MakeTransparent();
            this.pictureBox1.Image = thumbnail;
            this.lblFileName.Text = selectedPath;
        }

        public string GetPath()
        {
            return _path;
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (this.OnRemoveClick != null)
                this.OnRemoveClick(this);
        }
    }
}
