using BackyLogic;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;

namespace BackyListener
{
    public partial class BackyListenerService : ServiceBase
    {
        private DriveWatcher _driveWatcher = new DriveWatcher();
        

        public BackyListenerService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            _driveWatcher.NewDrivesAdded += (newDrives) =>
            {
                var backyDriveOrNull = newDrives.FirstOrDefault(DriveWatcher.IsBackyDrive);
                if (backyDriveOrNull != null)
                {
                    MarkNewBackyDrive(backyDriveOrNull);
                    InvokeBackyUI();
                }
            };
            _driveWatcher.Start();
        }

        private void InvokeBackyUI()
        {
            var lastBackyExecutbleFile = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "Backy",
                "LastExecutable.txt");
            var pathToExecutable = File.ReadAllText(lastBackyExecutbleFile);
            Process.Start(pathToExecutable);
        }

        private void MarkNewBackyDrive(DriveInfo drive)
        {
            var fileForMarking = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "Backy",
                "NewDriveDetection.txt");
            File.WriteAllText(fileForMarking, drive.RootDirectory.FullName);
        }

        protected override void OnStop()
        {
        }
    }
}
