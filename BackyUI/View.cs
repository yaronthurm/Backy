using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Backy
{
    public partial class View : Form
    {
        public View()
        {
            InitializeComponent();
        }

        public void SetFiles(IEnumerable<Backy.FileView> files, string rootDirectory)
        {
            this.filesPanel1.PopulateFiles(files, rootDirectory);
        }
    }
}
