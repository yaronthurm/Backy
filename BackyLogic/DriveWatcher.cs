using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackyLogic
{
    public class DriveWatcher
    {
        public event Action<IEnumerable<DriveInfo>> NewDrivesAdded;

        public void Start()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    CheckForChanges();
                    await Task.Delay(2000);
                }
            });
        }


        private List<string> _lastDrivesList = DriveInfo.GetDrives().Where(x => x.IsReady).Select(x => x.Name).ToList();

        private void CheckForChanges()
        {
            var currentDrives = DriveInfo.GetDrives().Where(x => x.IsReady).Select(x => x.Name).ToList();
            var addedDrives = currentDrives.Except(_lastDrivesList).ToList();
            if (addedDrives.Any())
            {
                Task.Run(() => this.NewDrivesAdded?.Invoke(addedDrives.Select(x => new DriveInfo(x))));
            }
            _lastDrivesList = currentDrives;

        }


        public static bool IsBackyDrive(DriveInfo drive)
        {
            var backyFile = new FileInfo(Path.Combine(drive.RootDirectory.FullName, "backy_drive.ini"));
            if (!backyFile.Exists)
                return false;
            var pathToBackyFolder = Path.Combine(drive.RootDirectory.FullName, File.ReadAllText(backyFile.FullName));
            if (!Directory.Exists(pathToBackyFolder))
                return false;
            return true;
        }
    }
}
