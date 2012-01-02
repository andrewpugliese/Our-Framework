using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace B1.TaskProcessingEngine.App
{
    /// <summary>
    /// The Task Processing Engine (TPE) Application is a WPF Host Application for the Task Processing Engine Class.
    /// <para>There can be 1 or more instances of this application running simulataneously provided that</para>
    /// <para>each instance of this application is assigned its own unique identifier (found in configuration key: EngineId)</para>
    /// <para>The value associated with this key needs to be found in the AppMaster table of the Core schema. </para>
    /// <para>This application behaves like the Windows Service version accept that it has a fulling functioning UI
    /// for monitoring the processes and adjusting runtime settings.</para>
    /// <para>Each TPE instance (application or service) will retrieve tasks from a logical queue stored in the database and host 
    /// the processing of them by user defined application functions which were loaded via reflection.</para>
    /// <para>Every task found in the queue also has a record in the TaskRegistrations table which contains the assembly in which
    /// the function is defined.  Each function is derived from the TaskProcess abstract class which allows it to be processed by
    /// the TPE.</para>
    /// <see cref="B1.TaskProcessing.TaskProcess"/>
    /// <para>The TPE is configured with a TaskAssemblyPath key which directs the engine where to find the assembly for the tasks.</para>
    /// <para>A sample assembly of sample tasks can be found in: <see cref="B1.Test.SampleTasks"/></para>
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
        // This class is only needed for SandCastle and its ability to create HTML documenatation for
        // a given namespace.
    }
}
