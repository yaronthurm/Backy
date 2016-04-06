using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Shell;
using System.IO;
using System.Diagnostics;
using BackyLogic;

namespace Backy
{
    public partial class FilesPanel : UserControl
    {
        private HierarchicalDictionary<string, FileView> _tree = new HierarchicalDictionary<string, FileView>();
        private string _rootDirectory;
        private int _currentPage;
        private string _currentDirectory = "";
        private int _currentViewTotalItems;
        private Dictionary<string, int> _pageNumberPerDirectory = new Dictionary<string, int>();

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
                if (_currentViewTotalItems % PageSize == 0)
                    return _currentViewTotalItems / PageSize;
                else
                    return _currentViewTotalItems / PageSize + 1;
            }
        }

        public void SetCurrentDirectoty(string value)
        {
            _currentDirectory = value;
        }

        public string GetCurrentDirectory()
        {
            return _currentDirectory;
        }

        public void PopulateFiles(IEnumerable<FileView> files, string rootDirectory)
        {
            _tree = new HierarchicalDictionary<string, FileView>();
            foreach (var file in files)
                 _tree.Add(file, file.LogicalPath.Split('\\'));
            _rootDirectory = rootDirectory;
            this.Clear();
            this.CaptureCurrentPage();
            this.FillPanel();
        }

        public void Clear()
        {
            _currentPage = 1;
            _pageNumberPerDirectory.Clear();
            this.flowLayoutPanel1.Controls.Clear();
        }

        public class ContextMenuItem {
            public string Text;
            public Action<FileView> OnClick;
        }
        private List<ContextMenuItem> _contextMenuItems = new List<ContextMenuItem>();

        public void AddContextMenuItem(string text, Action<FileView> onClick)
        {
            _contextMenuItems.Add(new ContextMenuItem { Text = text, OnClick = onClick });
        }

        private void EnableDisabledNavigationButtons()
        {
            this.btnNext.Enabled = _currentPage < TotalPages;
            this.btnPrev.Enabled = _currentPage > 1;
            this.btnUp.Enabled = _currentDirectory != "";
        }

        private void btnNext_Click(object sender, EventArgs e)
        {            
            this._currentPage++;
            this.CaptureCurrentPage();
            this.FillPanel();
        }

        private void btnPrev_Click(object sender, EventArgs e)
        {
            this._currentPage--;
            this.CaptureCurrentPage();
            this.FillPanel();
        }

        private void btnUp_Click(object sender, EventArgs e)
        {
            this.CaptureCurrentPage();
            _currentDirectory = Path.Combine(_currentDirectory.Split(new[] { "\\" }, StringSplitOptions.RemoveEmptyEntries).Reverse().Skip(1).Reverse().ToArray());
            this.FillPanel();
        }

        private void CaptureCurrentPage()
        {
            _pageNumberPerDirectory[_currentDirectory] = _currentPage;

        }

        private void FillPanel()
        {
            if (_pageNumberPerDirectory.ContainsKey(_currentDirectory))
                _currentPage = _pageNumberPerDirectory[_currentDirectory];
            else
                _currentPage = 1;

            this.lblCurrentDirectory.Text = _currentDirectory;

            this.flowLayoutPanel1.Controls.Clear();

            var firstLevelDirectories = GetFirstLevelDirectories().ToArray();
            var firstLevelFiles = GetFirstLevelFiles().ToArray();

            var directoeiresControls = GetDirectoriesControls(firstLevelDirectories); // This is heavy job so 
            var filesControls = GetFilesControls(firstLevelFiles);                    // we do differ execution

            int start = (_currentPage - 1) * PageSize + 1;

            var pageControls = directoeiresControls.Union(filesControls)
                .Skip(start - 1).Take(PageSize).ToArray();

            _currentViewTotalItems = firstLevelDirectories.Length + firstLevelFiles.Length;
            int end = Math.Min(start + PageSize - 1, _currentViewTotalItems);
            lblCount.Text = $"{start}-{end}/{_currentViewTotalItems}";

            this.flowLayoutPanel1.Controls.AddRange(pageControls);
            EnableDisabledNavigationButtons();
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
                    item.ContextMenuStrip = this.GetContextMenuForFileView(x);
                    item.DoubleClick += fileView => Process.Start(fileView.PhysicalPath);
                    return item;
                });
            return ret;
        }

        private ContextMenuStrip GetContextMenuForFileView(FileView file)
        {
            var ret = new ContextMenuStrip();
            foreach (var item in _contextMenuItems)
            {
                var stripItem = new ToolStripMenuItem(item.Text);
                stripItem.Click += (s, e) => item.OnClick(file);
                ret.Items.Add(stripItem);
            }
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
                        this.CaptureCurrentPage();
                        _currentDirectory = Path.Combine(_currentDirectory, fileView.LogicalPath);
                        this.FillPanel();
                    };
                    return item;
                });
            return ret;
        }

        private IEnumerable<FileView> GetFirstLevelFiles()
        {
            return _tree.GetFirstLevelItems(_currentDirectory.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries));
        }

        public IEnumerable<string> GetFirstLevelDirectories()
        {
            return _tree.GetFirstLevelContainers(_currentDirectory.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries));
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

        public override string ToString()
        {
            return LogicalPath;
        }
    }
}
