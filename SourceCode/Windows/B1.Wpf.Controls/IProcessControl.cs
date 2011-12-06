using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace B1.Wpf.Controls
{
    public interface IProcessControl
    {
        void Start(object context);
        void Stop(object context);
        void Pause(object context);
        void Resume(object context);
        void DisplayPausedState();
        void DisplayResumedState();
        string Status(object context);
    }
}
