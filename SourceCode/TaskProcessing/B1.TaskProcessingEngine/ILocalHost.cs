using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace B1.TaskProcessing
{
    interface ILocalHost
    {
        void Start();
        void Stop();
        void Pause();
        void Resume();
        string Status();
        int SetMaxTasks(int delta);
    }
}
