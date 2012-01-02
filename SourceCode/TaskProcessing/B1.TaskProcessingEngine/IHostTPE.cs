using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace B1.TaskProcessing
{
    /// <summary>
    /// Interface supported by a Task Processing Engine (TPE) Host Application or Service
    /// </summary>
    interface IHostTPE
    {
        void Start();
        void Stop();
        void Pause();
        void Resume();
        string Status();
        int SetMaxTasks(int delta);
    }
}
