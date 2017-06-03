using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;
using System.Threading;
using System.Collections.Concurrent;
using Microsoft.WindowsAPICodePack.Shell;

namespace Backy
{
    public partial class FlowPanelTest : UserControl
    {
        public FlowPanelTest()
        {
            InitializeComponent();
        }

        public void SetData(int numberOfItemsToSimulate, TimeSpan itemLoadTime)
        {
            var controls = GetSimulatedControls2(Enumerable.Range(0, numberOfItemsToSimulate), itemLoadTime);
            this.flowLayoutPanel1.Controls.Clear();
            this.flowLayoutPanel1.Controls.AddRange(controls);
        }

        public void SetRealData(FileView[] files)
        {
            var controls = GetFilesControls_ParllelWithDifferedLoading(files);
            this.flowLayoutPanel1.Controls.Clear();
            this.flowLayoutPanel1.Controls.AddRange(controls);

        }

        private Control[] GetSimulatedControls(IEnumerable<int> simulatedSource, TimeSpan itemLoadTime)
        {
            var source = simulatedSource.Select((x, i) => new { seqNum = i, value = x }).ToArray();
            var ret = new Control[source.Length];
            Parallel.ForEach(source, x =>
            {
                var item = new Button();
                item.Text = x.value.ToString();
                if (itemLoadTime > TimeSpan.Zero)
                    Thread.Sleep(itemLoadTime);
                ret[x.seqNum] = item;
            });
            return ret;
        }

        private Control[] GetSimulatedControls2(IEnumerable<int> simulatedSource, TimeSpan itemLoadTime)
        {
            var source = simulatedSource.Select((x, i) => new { seqNum = i, value = x }).ToArray();
            var ret = new Control[source.Length];
            Parallel.ForEach(source, x =>
            {
                var item = new Button();
                item.Tag = x.value.ToString();
                ret[x.seqNum] = item;
            });

            Task.Run(() =>
            {
                var source2 = Partitioner.Create(ret).AsParallel().AsOrdered();
                Parallel.ForEach(source2, x =>
                {
                    if (itemLoadTime > TimeSpan.Zero)
                        Thread.Sleep(itemLoadTime);
                    if (this.InvokeRequired)
                        this.BeginInvoke((Action)(() => { x.Text = x.Tag.ToString(); }));
                    else
                        x.Text = x.Tag.ToString();
                });
                return;

                Parallel.ForEach(ret.Take(ret.Length / 5), x =>
                {
                    if (itemLoadTime > TimeSpan.Zero)
                        Thread.Sleep(itemLoadTime);
                    if (this.InvokeRequired)
                        this.BeginInvoke((Action)(() => { x.Text = x.Tag.ToString(); }));
                    else
                        x.Text = x.Tag.ToString();
                });

                Parallel.ForEach(ret.Skip(ret.Length / 5), x =>
                {
                    if (itemLoadTime > TimeSpan.Zero)
                        Thread.Sleep(itemLoadTime);
                    if (this.InvokeRequired)
                        this.BeginInvoke((Action)(() => { x.Text = x.Tag.ToString(); }));
                    else
                        x.Text = x.Tag.ToString();
                });
            });
            return ret;
        }



        private Control[] GetFilesControls_ParllelWithDifferedLoading(IEnumerable<FileView> firstLevelFiles)
        {
            var source = firstLevelFiles.Select((x, i) => new { seqNum = i, value = x }).Take(3000).ToArray();
            var ret = new Control[source.Length];
            Parallel.ForEach(source, x =>
            {
                var item = new LargeFileView();
                //item.BackColor = Color.BlueViolet;
                //item.Text = "123";
                item.Tag = x.value;
                item.SetDataForLazyLoading(x.value);
                ret[x.seqNum] = item;
            });

            Task.Run(() =>
            {
                var source2 = Partitioner.Create(ret).AsParallel().AsOrdered();
                Parallel.ForEach(ret, x =>
                {
                    FileView itemSource = (FileView)x.Tag;
                    //var shellFile = ShellFolder.FromParsingName(itemSource.PhysicalPath);
                    //shellFile.Thumbnail.FormatOption = ShellThumbnailFormatOption.Default;
                    //var bitmap = shellFile.Thumbnail.MediumBitmap;

                    //if (this.InvokeRequired)
                        //this.BeginInvoke((Action)(() => x.SetData(bitmap, itemSource)));
                    //else
                        //x.SetData(bitmap, itemSource);
                });
            });

            return ret;
        }
    }
}

