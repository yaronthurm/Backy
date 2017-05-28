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
                Parallel.ForEach(ret, x =>
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
    }
}

