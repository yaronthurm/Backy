using BackyLogic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Backy
{
    static class Program
    {
        static Mutex singleInstanceMutex = new Mutex(true, "{2FA0A600-7B60-4E7B-9C58-8FA0D3D575B0}");

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (ShouldOpenAsConsoloeApp(args))
            {
                RunAsConsoleApp();
                return;
            }

            if (singleInstanceMutex.WaitOne(TimeSpan.Zero, true))
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Main());
            }
            else {
                FocusOnActiveInstance();
            }
        }

        private static void RunAsConsoleApp()
        {
            AllocConsole();
            BackyLogic.Settings _settings = BackyLogic.Settings.Load();
            IFileSystem fileSystem = new OSFileSystem();

            if (!Directory.Exists(_settings.Target))
            {
                Console.WriteLine($"Target directory '{_settings.Target}' does not exist. Please select a different target.");
                return;
            }
            var activeSources = _settings.Sources.Where(x => x.Enabled && Directory.Exists(x.Path)).ToArray();
            

            var _cancelTokenSource = new CancellationTokenSource();
            var backupCommands = activeSources
                .Select(x =>
                new RunBackupCommand(fileSystem, x.Path, _settings.Target, new MachineID { Value = _settings.MachineID },
                _cancelTokenSource.Token) {
                    Progress = new ConsoleProgress() });

            if (!backupCommands.Any()) {
                new ConsoleProgress().StartStepWithoutProgress("There are no active sources");
                return;
            }

            foreach (var cmd in backupCommands)
            {
                cmd.Execute();
            }
        }

        private static bool ShouldOpenAsConsoloeApp(string[] args)
        {
            var ret = args != null && args.Length == 1 && args[0] == "-console";
            return ret;
        }

        static void FocusOnActiveInstance()
        {
            Process current = Process.GetCurrentProcess();
            foreach (Process process in Process.GetProcessesByName(current.ProcessName))
            {
                if (process.Id != current.Id)
                {
                    SetForegroundWindow(process.MainWindowHandle);
                    break;
                }
            }
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();
    }
}