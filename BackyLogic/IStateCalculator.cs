using System;
using System.Collections.Generic;

namespace BackyLogic
{
    public interface IStateCalculator
    {
        event Action OnProgress;
        int MaxVersion {  get; }
        IState GetState(int version);
        IState GetDiff(int version);
        DateTime GetDateByVersion(int currentVersion);
        IState GetLastState();

    }        
}
