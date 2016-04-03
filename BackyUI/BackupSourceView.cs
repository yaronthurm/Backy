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
using System.Diagnostics;
using System.Drawing.Imaging;

namespace Backy
{
    public partial class BackupSourceView : UserControl
    {
        private Bitmap _originalBitmap;
        private Bitmap _grayscaleBitmap;

        public event Action<BackupSourceView> OnRemoveClick;

        public BackupSourceView()
        {
            InitializeComponent();

            this.lblPath.Text = null;
        }

        internal void SetDirectory(string selectedPath, bool isActive)
        {
            var shellFile = ShellFolder.FromParsingName(selectedPath);
            shellFile.Thumbnail.FormatOption = ShellThumbnailFormatOption.Default;
            Bitmap thumbnail = shellFile.Thumbnail.MediumBitmap;
            thumbnail.MakeTransparent();

            _originalBitmap = thumbnail;
            _grayscaleBitmap = MakeGrayscale3(thumbnail);
            this.pictureBox1.Image = thumbnail;
            this.lblPath.Text = selectedPath;
            this.chkEnabled.Checked = isActive;
        }

        public string Path
        {
            get { return this.lblPath.Text; }
        }

        public bool IsActive { get { return this.chkEnabled.Checked; } }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (this.OnRemoveClick != null)
                this.OnRemoveClick(this);
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            Process.Start(this.Path);
        }

        private void chkEnabled_CheckedChanged(object sender, EventArgs e)
        {
            this.pictureBox1.Image = this.chkEnabled.Checked ? _originalBitmap : _grayscaleBitmap;
        }




        private static Bitmap MakeGrayscale3(Bitmap original)
        {
            //create a blank bitmap the same size as original
            Bitmap newBitmap = new Bitmap(original.Width, original.Height);

            //get a graphics object from the new image
            Graphics g = Graphics.FromImage(newBitmap);

            //create the grayscale ColorMatrix
            ColorMatrix colorMatrix = new ColorMatrix(
               new float[][]
               {
         new float[] {.3f, .3f, .3f, 0, 0},
         new float[] {.59f, .59f, .59f, 0, 0},
         new float[] {.11f, .11f, .11f, 0, 0},
         new float[] {0, 0, 0, 1, 0},
         new float[] {0, 0, 0, 0, 1}
               });

            //create some image attributes
            ImageAttributes attributes = new ImageAttributes();

            //set the color matrix attribute
            attributes.SetColorMatrix(colorMatrix);

            //draw the original image on the new image
            //using the grayscale color matrix
            g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height),
               0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);

            //dispose the Graphics object
            g.Dispose();
            return newBitmap;
        }
    }


    class ResizeableCheckBox : CheckBox
    {
        public ResizeableCheckBox()
        {
            this.TextAlign = ContentAlignment.MiddleRight;
        }

        public override bool AutoSize
        {
            get { return base.AutoSize; }
            set { base.AutoSize = false; }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            int h = this.ClientSize.Height - 2;
            Rectangle rc = new Rectangle(new Point(0, 1), new Size(h, h));
            ControlPaint.DrawCheckBox(e.Graphics, rc,
                this.Checked ? ButtonState.Checked : ButtonState.Normal);
        }
    }
}
