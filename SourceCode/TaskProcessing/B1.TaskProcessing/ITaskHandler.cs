using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using B1.DataAccess;

namespace B1.TaskProcessing
{
    /// <summary>
    /// Interface for processing long and short running tasks
    /// </summary>
    public interface ITaskHandler
    {
        void Initialize(DataAccessMgr daMgr, string payload);
        string Start(); // if returns null, then will be long running and status will be called
                        // if returns non null, then that is result; function has ended; call stop.
        void Stop();
        string Status();
    }
}
