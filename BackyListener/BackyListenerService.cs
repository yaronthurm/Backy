using BackyLogic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

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
            
        }

        private void MarkNewBackyDrive(DriveInfo drive)
        {
            
        }

        protected override void OnStop()
        {
        }
    }
}
