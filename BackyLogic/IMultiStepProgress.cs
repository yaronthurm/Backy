using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace BackyLogic
{
    public interface IMultiStepProgress
    {
        void StartStepWithoutProgress(string text);
        void StartUnboundedStep(string text, Func<int, string> projection = null);
        void StartBoundedStep(string text, int maxValue);
        void UpdateProgress(int currentValue);
        void Increment();
    }
}
