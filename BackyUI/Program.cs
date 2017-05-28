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
        
        [STAThread]
        static void Main(string[] args)
        {
            if (NoOtherInstanceExists())
            {
                if (ShouldOpenAsConsoloeApp(args))
                    RunAsConsoleApp(args);
                else
                    RunAsWinFormsApp();
            }
            else {
                FocusOnActiveInstance();
            }
        }

        private static bool NoOtherInstanceExists()
        {
            return singleInstanceMutex.WaitOne(TimeSpan.Zero, true);
        }

        private static void RunAsWinFormsApp()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new PanelTest());
            Application.Run(new Main());
        }

        private static UIModes GetUIMode(string[] args)
        {
            if (args != null && args.Length == 1)
            {
                if (args[0] == "-console")
                    return UIModes.Console;
                if (args[0] == "-hidden")
                    return UIModes.Hidden;
                return UIModes.Undefined;
            }
            return UIModes.WinForm;
        }

        public enum UIModes {WinForm, Console, Hidden, Undefined }

        private static void RunAsConsoleApp(string[] args)
        {
            IMultiStepProgress progress = null;
            var mode = GetUIMode(args);            
            if (mode == UIModes.Console)
            {
                AllocConsole();
                progress = new ConsoleProgress();
            }
            else if (mode == UIModes.Undefined)
            {
                AllocConsole();
                Console.WriteLine("Unsupported UI mode, either -console or -hidden are supported");
                Environment.Exit(1);
                return;
            }
            
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
                    Progress = progress });

            if (!backupCommands.Any()) {
                new ConsoleProgress().StartStepWithoutProgress("There are no active sources");
                return;
            }

            foreach (var cmd in backupCommands)
            {
                cmd.Execute();
            }
        }

        private static void HideConsole()
        {
            var handle = GetConsoleWindow();
            ShowWindow(handle, SW_HIDE);
        }

        private static bool ShouldOpenAsConsoloeApp(string[] args)
        {
            var mode = GetUIMode(args);
            var ret = mode == UIModes.Console || mode == UIModes.Hidden || mode == UIModes.Undefined;
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

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;
    }
}