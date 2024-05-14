using System;
using System.IO;


namespace BackyLogic
{

    public class StateCalculator_Factory
    {
        public static IStateCalculator GetStateCalculator(IFileSystem fileSystem, string target, string source, string machineID)
        {
            string targetForSource = FindTargetForSourceOrNull(source, target, fileSystem, machineID);
            if (targetForSource != null)
            {
                var backupMode = BackupDirectory.FromPath(targetForSource, fileSystem).BackupMode;
                if (backupMode == "diff")
                    return new StateCalculator(fileSystem, target, source, machineID);
                if (backupMode == "current_state")
                    return new StateCalculator2(fileSystem, target, source, machineID);
                throw new Exception("Invalid backup mode");
            }
            throw new ApplicationException("Could not find directory for source: " + source);
        }     

        public static string FindTargetForSourceOrNull(string source, string target, IFileSystem fs, string machineID)
        {
            string sourceGuid = null;            
            foreach (var innerDir in fs.GetTopLevelDirectories(target))
            {
                if (!BackupDirectory.IsBackupDirectory(innerDir, fs)) continue;
                var backupDir = BackupDirectory.FromPath(innerDir, fs);

                if (backupDir.OriginalSource.Equals(source, StringComparison.OrdinalIgnoreCase) && backupDir.MachineID == machineID)
                {
                    sourceGuid = backupDir.Guid;
                    break;
                }
            }
            if (sourceGuid == null)
                return null;
            var ret = Path.Combine(target, sourceGuid);
            return ret;
        }                                                     
    }    
}
