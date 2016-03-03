using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Shell;
using System.IO;
using System.Diagnostics;

namespace Backy
{
    public partial class FilesPanel : UserControl
    {
        private List<FileView> _files = new List<FileView>();
        private string _rootDirectory;
        private int _currentPage;
        private string _currentDirectory = "";

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
            _currentPage = 1;
            this.FillPanel();
        }

        private void EnableDisabledNavigationButtons(int numberOfTotalItems)
        {
            if (numberOfTotalItems < PageSize)
            {
                this.btnNext.Enabled = false;
                this.btnPrev.Enabled = false;
            }
            else
            {
                this.btnNext.Enabled = true;
                this.btnPrev.Enabled = false;
            }
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
            this.flowLayoutPanel1.Controls.Clear();

            var includedFiles = GetFilesToInclude();
            var firstLevelDirectories = GetFirstLevelDirectories(includedFiles);
            var firstLevelFiles = GetFirstLevelFiles(includedFiles, firstLevelDirectories);

            var directoeiresControls = GetDirectoriesControls(firstLevelDirectories);
            var filesControls = GetFilesControls(firstLevelFiles);

            int start = (_currentPage - 1) * PageSize + 1;

            var allControls = directoeiresControls.Union(filesControls).ToArray();
            var pageControls = allControls.Skip(start - 1).Take(PageSize).ToArray();

            int end = Math.Min(start + PageSize - 1, pageControls.Length);
            lblCount.Text = string.Format("{0}-{1}/{2}", start, end, allControls.Length);

            this.flowLayoutPanel1.Controls.AddRange(allControls);
            EnableDisabledNavigationButtons(allControls.Length);
        }

        private IEnumerable<LargeFileView> GetFilesControls(IEnumerable<FileView> firstLevelFiles)
        {
            var ret = firstLevelFiles
                .Select(x =>
                {
                    var item = new LargeFileView();
                    var shellFile = ShellFolder.FromParsingName(x.PhysicalPath);
                    shellFile.Thumbnail.FormatOption = ShellThumbnailFormatOption.Default;
                    item.SetData(shellFile.Thumbnail.MediumBitmap, x);
                    item.DoubleClick += fileView => Process.Start(fileView.PhysicalPath);
                    return item;
                });
            return ret;
        }


        private IEnumerable<LargeFileView> GetDirectoriesControls(IEnumerable<string> firstLevelDirectories)
        {
            var ret = firstLevelDirectories
                .Select(x =>
                {
                    var item = new LargeFileView();
                    var shellFile = ShellFolder.FromParsingName(_rootDirectory);
                    shellFile.Thumbnail.FormatOption = ShellThumbnailFormatOption.Default;
                    item.SetData(shellFile.Thumbnail.MediumBitmap, new FileView { LogicalPath = x, PhysicalPath = x });
                    item.DoubleClick += fileView =>
                    {
                        _currentDirectory = Path.Combine(_currentDirectory, fileView.LogicalPath) + "\\";
                        this.FillPanel();
                    };
                    return item;
                });
            return ret;
        }

        private IEnumerable<FileView> GetFirstLevelFiles(IEnumerable<FileView> includedFiles, IEnumerable<string> firstLevelDirectories)
        {
            var ret = includedFiles
                .Where(x => firstLevelDirectories.All(y => !x.LogicalPath.StartsWith(_currentDirectory + y + "\\")));
            return ret;
        }

        private IEnumerable<FileView> GetFilesToInclude()
        {
            // Only return files that start with the current directory;
            if (string.IsNullOrEmpty(_currentDirectory))
                return this._files;

            var ret = _files.Where(x => x.LogicalPath.StartsWith(_currentDirectory));
            return ret;
        }

        public IEnumerable<string> GetFirstLevelDirectories(IEnumerable<FileView> files)
        {
            IEnumerable<string> ret;
            if (_currentDirectory == "")
            {
                ret = files
                    .Select(x => x.LogicalPath.Split('\\'))
                    .Where(x => x.Length > 1).Select(x => x[0]).Distinct();
            }
            else
            {
                ret = files
                    .Select(x => x.LogicalPath.Replace(_currentDirectory, "").Split('\\'))
                    .Where(x => x.Length > 1).Select(x => x[0]).Distinct();
            }

            return ret;
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
