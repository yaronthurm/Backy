using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Shell;
using System.IO;

namespace Backy
{
    public partial class FilesPanel : UserControl
    {
        private List<FileView> _files = new List<FileView>();
        private string _rootDirectory;
        private int _currentPage;

        public event Action OnChange;

        public FilesPanel()
        {
            InitializeComponent();
        }

        private int PageSize
        {
            get
            {
                return (int)this.numericUpDown1.Value;
            }
        }
        private int TotalPages
        {
            get
            {
                if (_files.Count % PageSize == 0)
                    return _files.Count / PageSize;
                else
                    return _files.Count / PageSize + 1;
            }
        }

        public void PopulateFiles(IEnumerable<FileView> files, string rootDirectory)
        {
            _files = files.ToList();
            _rootDirectory = rootDirectory;
            if (_files.Count < PageSize)
            {
                this.btnNext.Enabled = false;
                this.btnPrev.Enabled = false;
            }
            else
            {
                this.btnNext.Enabled = true;
                this.btnPrev.Enabled = false;
            }
            
            _currentPage = 1;
            this.FillPanel();
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            this.btnPrev.Enabled = true;
            this._currentPage++;
            if (_currentPage == TotalPages)
                this.btnNext.Enabled = false;
            this.FillPanel();
        }

        private void btnPrev_Click(object sender, EventArgs e)
        {
            this.btnNext.Enabled = true;
            this._currentPage--;
            if (_currentPage == 1)
                this.btnPrev.Enabled = false;
            this.FillPanel();
        }

        private void FillPanel()
        {
            //foreach (LargeFileWithTag item in this.flowLayoutPanel1.Controls)
                //item.OnChange -= this.OnChangeHandler;
            this.flowLayoutPanel1.Controls.Clear();

            // Find first level folders
            var folders = GetFirstLevelDirectories(_files.Select(x => x.LogicalPath));
            var folderItems = folders
                .Select(x =>
                {
                    var item = new LargeFileView();
                    var shellFile = ShellFolder.FromParsingName(Path.Combine(_rootDirectory, x));
                    shellFile.Thumbnail.FormatOption = ShellThumbnailFormatOption.Default;
                    item.SetData(shellFile.Thumbnail.MediumBitmap, new FileView { LogicalPath = x, PhysicalPath = x });
                    item.OnChange += this.OnChangeHandler;
                    return item;
                }).ToArray();

            int start = (_currentPage - 1) * PageSize + 1;

            var filesWithoutFolders = _files.Where(x => folders.All(y => !x.LogicalPath.StartsWith(y + "\\")));

            var fileItems = filesWithoutFolders
                .Skip(start - 1)
                .Take(PageSize)
                .Select(x =>
            {
                var item = new LargeFileView();
                var shellFile = ShellFolder.FromParsingName(x.PhysicalPath);
                shellFile.Thumbnail.FormatOption = ShellThumbnailFormatOption.Default;
                item.SetData(shellFile.Thumbnail.MediumBitmap, x);
                item.OnChange += this.OnChangeHandler;
                return item;
            }).ToArray();


            var totalItem = folderItems.Length + fileItems.Length;
            int end = Math.Min(start + PageSize - 1, totalItem);
            lblCount.Text = string.Format("{0}-{1}/{2}", start, end, totalItem);

            this.flowLayoutPanel1.Controls.AddRange(folderItems);
            this.flowLayoutPanel1.Controls.AddRange(fileItems);
        }

        public static IEnumerable<string> GetFirstLevelDirectories(IEnumerable<string> paths)
        {
            // Get just first level directories
            var ret = paths.Select(x => x.Split('\\')).Where(x => x.Length > 1).Select(x => x[0]).Distinct();

            return ret;
        }

        private void OnChangeHandler()
        {
            if (this.OnChange != null)
                this.OnChange();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            //_currentPage = Math.Min(_currentPage, TotalPages - 1);
        }
    }


    public class FileView
    {
        public string LogicalPath;
        public string PhysicalPath;
    }
}
