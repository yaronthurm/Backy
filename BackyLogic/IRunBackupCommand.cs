using System.Collections.Generic;

namespace BackyLogic
{
    public interface IRunBackupCommand
    {
        void Execute();
        List<BackupFailure> Failures { get; }
        string FailuresPretty {  get; }
    }        
}
