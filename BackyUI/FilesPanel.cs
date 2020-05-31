using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Shell;
using System.IO;
using System.Diagnostics;
using BackyLogic;
using System.Threading.Tasks;

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
        private List<ContextMenuItem> _contextMenuItems = new List<ContextMenuItem>();

        public bool EnableContextMenu { get; set; } = true;


        public FilesPanel()
        {
            InitializeComponent();

            this.lblCount.Text = null;
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
            var filesControls = GetFilesControls_ParllelWithDifferedLoading(firstLevelFiles);                    // we do differ execution

            int start = (_currentPage - 1) * PageSize + 1;
            
            var pageControls = directoeiresControls.Union(filesControls)
                .Skip(start - 1).Take(PageSize).ToArray();

            _currentViewTotalItems = firstLevelDirectories.Length + firstLevelFiles.Length;
            int end = Math.Min(start + PageSize - 1, _currentViewTotalItems);
            lblCount.Text = $"{start}-{end}/{_currentViewTotalItems}";

            this.flowLayoutPanel1.Controls.AddRange(pageControls);
            EnableDisabledNavigationButtons();
        }

        private LargeFileView[] GetFilesControls_Serial(IEnumerable<FileView> firstLevelFiles)
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
            return ret.ToArray();
        }

        private LargeFileView[] GetFilesControls_Parllel(IEnumerable<FileView> firstLevelFiles)
        {
            var source = firstLevelFiles.Select((x, i) => new { seqNum = i, value = x }).ToArray();
            var ret = new LargeFileView[source.Length];
            Parallel.ForEach(source, x =>
            {
                var item = new LargeFileView();
                var shellFile = ShellFolder.FromParsingName(x.value.PhysicalPath);
                shellFile.Thumbnail.FormatOption = ShellThumbnailFormatOption.Default;
                if (this.InvokeRequired)
                    this.BeginInvoke((Action)(() => item.SetData(shellFile.Thumbnail.MediumBitmap, x.value)));
                else
                    item.SetData(shellFile.Thumbnail.MediumBitmap, x.value);
                item.ContextMenuStrip = this.GetContextMenuForFileView(x.value);
                item.DoubleClick += fileView => Process.Start(fileView.PhysicalPath);
                ret[x.seqNum] = item;
            });
            return ret;
        }

        private LargeFileView[] GetFilesControls_ParllelWithDifferedLoading(IEnumerable<FileView> firstLevelFiles)
        {
            var source = firstLevelFiles.Select((x, i) => new { seqNum = i, value = x }).ToArray();
            var ret = new LargeFileView[source.Length];
            Parallel.ForEach(source, x =>
            {
                var item = new LargeFileView();
                item.ContextMenuStrip = this.GetContextMenuForFileView(x.value);
                item.DoubleClick += fileView => Process.Start(fileView.PhysicalPath);
                item.Tag = x.value;
                ret[x.seqNum] = item;
            });

            Task.Run(() =>
            {
                Parallel.ForEach(ret, x =>
                {
                    System.Drawing.Bitmap bitmap = null;                   
                    FileView itemSource = (FileView)x.Tag;

                    if (itemSource.PhysicalPath != null)
                    {
                        var shellFile = ShellFolder.FromParsingName(itemSource.PhysicalPath);
                        shellFile.Thumbnail.FormatOption = ShellThumbnailFormatOption.Default;
                        bitmap = FuncWithRetry(() => shellFile.Thumbnail.MediumBitmap);
                    }

                    if (this.InvokeRequired)
                        this.Invoke((Action)(() => x.SetData(bitmap, itemSource)));
                    else
                        x.SetData(bitmap, itemSource);
                });
            });

            return ret;
        }

        private static T FuncWithRetry<T>(Func<T> func)
        {
            try
            {
                return func();
            }
            catch
            {
                return func();
            }
        }

        private ContextMenuStrip GetContextMenuForFileView(FileView file)
        {
            var ret = new ContextMenuStrip();
            if (!this.EnableContextMenu)
                return ret;
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


    public class ContextMenuItem
    {
        public string Text;
        public Action<FileView> OnClick;
    }
}
