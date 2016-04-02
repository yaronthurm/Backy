using System;
using System.Collections.Generic;
using System.IO;
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




        public static string ChooseDirectoryConditionaly(FolderBrowserDialog folderDialog, Func<string, bool> condition, string errorMessage)
        {
            string ret = "";
            while (true)
            {
                folderDialog.SelectedPath = ret;
                var result = folderDialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    ret = folderDialog.SelectedPath;
                    if (condition(ret))
                        return ret;
                    else
                        MessageBox.Show(errorMessage);
                }
                else
                {
                    return null;
                }
            }
        }


        public static string ChooseEmptyFolder()
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.ShowNewFolderButton = true;
            var ret = ChooseDirectoryConditionaly(dialog, x => !Directory.GetDirectories(x).Any() && !Directory.GetFiles(x).Any(), "Please choose an empty directory");
            return ret;
        }
    }
}
