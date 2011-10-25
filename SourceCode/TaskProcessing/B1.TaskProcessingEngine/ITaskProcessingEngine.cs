using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using B1.Wpf.Controls;

namespace B1.TaskProcessing
{
    interface ITaskProcessingEngine : IProcessControl
    {
        void Connect();
        void Disconnect();
        int MaxTaskProcesses(int delta);
        Dictionary<string, string> ConfigSettings();
    }
}
