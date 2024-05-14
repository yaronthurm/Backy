using System;
using System.Threading;


namespace BackyLogic
{
    public class RunBackupCommand_Factory
    {

        public static IRunBackupCommand GetRunBackupCommand(IFileSystem fileSystem, string source, string target, MachineID machineID, CancellationToken cancellationToken, IMultiStepProgress progress)
        {
            string targetForSource = StateCalculator.FindTargetForSourceOrNull(source, target, fileSystem, machineID.Value);
            if (targetForSource != null)
            {
                var backupMode = BackupDirectory.FromPath(targetForSource, fileSystem).BackupMode;
                if (backupMode == "diff")
                    return new RunBackupCommand(fileSystem, source, target, machineID, cancellationToken, progress);
                if (backupMode == "current_state")
                    return new RunBackupCommand2(fileSystem, source, target, machineID, cancellationToken, progress);
                throw new Exception("Invalid backup mode");
            }
            return new RunBackupCommand2(fileSystem, source, target, machineID, cancellationToken, progress);

        }
    }
}
