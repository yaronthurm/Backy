using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Backy
{
    internal static class UIUtils
    {
        public static void ScrollToBottom(Control control)
        {
            SendMessage(control.Handle, WmVscroll, SbBottom, 0x0);
        }

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr window, int message, int wparam, int lparam);

        private const int SbBottom = 0x7;
        private const int WmVscroll = 0x115;
    }
}
