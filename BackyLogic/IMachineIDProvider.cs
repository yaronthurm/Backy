using System;
using System.Collections.Generic;

namespace BackyLogic
{
    public interface IMachineIDProvider
    {
        string GetOrCreateID();
    }

}
