using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Backy
{
    public partial class PanelTest : Form
    {
        public PanelTest()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var sw = Stopwatch.StartNew();
            this.flowPanelTest1.SetData((int)this.numItems.Value, TimeSpan.FromMilliseconds((int)this.loadTime.Value));
            sw.Stop();
            this.lblTime.Text = sw.Elapsed.ToString();
        }
    }
}
