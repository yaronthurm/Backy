using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.WindowsAPICodePack.Shell;
using System.Threading.Tasks;
using System.Threading;

namespace Backy
{
    public partial class LargeFileView : UserControl
    {
        private FileView _file;
        private bool _isLoaded;
        public event Action OnChange;
        public event Action<FileView> DoubleClick;

        public LargeFileView()
        {
            InitializeComponent();
            
            foreach (Control ctrl in this.GetAllDecendentsContorls(this))
            {
                this.BindCtrlToMouseEnterAndMouseLeave(ctrl);
                this.BindCtrlToMouseDoubleClick(ctrl);
            }
        }

        private IEnumerable<Control> GetAllDecendentsContorls(Control parent)
        {
            var ret = new List<Control>();
            var queue = new Queue<Control>();
            queue.Enqueue(parent);
            while (queue.Count > 0)
            {
                var item = queue.Dequeue();
                ret.Add(item);
                foreach (Control innerControl in item.Controls)
                    queue.Enqueue(innerControl);
            }
            return ret;
        }

        private void BindCtrlToMouseEnterAndMouseLeave(Control ctrl)
        {
            ctrl.MouseEnter += (s, e) => this.BackColor = Color.LightSkyBlue; 
            ctrl.MouseLeave += (s, e) => this.BackColor = Color.Transparent; 
        }

        private void BindCtrlToMouseDoubleClick(Control ctrl)
        {
            ctrl.DoubleClick += (s, e) =>
            {
                if (this.DoubleClick != null)
                    this.DoubleClick(_file);
            };
        }

        public void SetData(Bitmap thumbnail, FileView file)
        {
            _isLoaded = true;
            _file = file;
            if (ThumbnailShouldBeMadeTransparent(_file.PhysicalPath))
                thumbnail.MakeTransparent();
            this.pictureBox1.Image = thumbnail;
            this.lblFileName.Text = Path.GetFileName(_file.LogicalPath);
        }

        public void SetDataForLazyLoading(FileView file)
        {
            _file = file;
        }

        private bool ThumbnailShouldBeMadeTransparent(string physicalPath)
        {
            var dontMakeTransparentExtentions = new[] { ".jpg", ".mpeg", ".avi", ".png" };
            var ext = Path.GetExtension(physicalPath).ToLower();
            var ret = !dontMakeTransparentExtentions.Contains(ext);
            return ret;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            //if (IsVisible(e) && !WasAlreadyLoaded())            
                //LoadDataForContol();
            base.OnPaint(e);
        }

        private void LoadDataForContol()
        {
            _isLoaded = true;
            this.lblFileName.Text = "Loading...";
            Task.Run(() =>
            {
                var shellFile = ShellFolder.FromParsingName(_file.PhysicalPath);
                shellFile.Thumbnail.FormatOption = ShellThumbnailFormatOption.Default;
                var bitmap = shellFile.Thumbnail.MediumBitmap;
                this.BeginInvoke((Action)(() => this.SetData(bitmap, _file)));
            });
        }

        private bool WasAlreadyLoaded()
        {
            return _isLoaded;
        }

        private bool IsVisible(PaintEventArgs e)
        {
            return !e.ClipRectangle.IsEmpty;
        }
    }
}