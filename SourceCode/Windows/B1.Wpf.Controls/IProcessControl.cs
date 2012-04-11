using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace B1.Wpf.Controls
{
    /// <summary>
    /// Interface definition for the process control controls
    /// </summary>
    public interface IProcessControl
    {
        /// <summary>
        /// Initiate or begin a process
        /// </summary>
        /// <param name="context">Object provided by user during construction</param>
        void Start(object context);

        /// <summary>
        /// Permanently End a process
        /// </summary>
        /// <param name="context">Object provided by user during construction</param>
        void Stop(object context);

        /// <summary>
        /// Temporarily pause a process
        /// </summary>
        /// <param name="context">Object provided by user during construction</param>
        void Pause(object context);

        /// <summary>
        /// Resume a process from where it was paused
        /// </summary>
        /// <param name="context">Object provided by user during construction</param>
        void Resume(object context);

        /// <summary>
        /// Set control display to indicate a paused state
        /// </summary>
        void DisplayPausedState();

        /// <summary>
        /// Set control display to indicate a running state
        /// </summary>
        void DisplayResumedState();

        /// <summary>
        /// Return the status of the process
        /// </summary>
        /// <param name="context">Object provided by user during construction</param>
        /// <returns>The process's status string</returns>
        string Status(object context);
    }
}
