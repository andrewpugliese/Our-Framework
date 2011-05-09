using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Data.Common;
using System.Diagnostics;
using System.Data;

using Microsoft.Practices.EnterpriseLibrary.Data;

using B1.Core;
using B1.DataAccess;

namespace B1.Utility.DatabaseSetup
{
    class TestAppSessionControl
    {
        DataAccessMgr _daMgr = null;
        bool _stop = false;

        internal TestAppSessionControl(DataAccessMgr daMgr)
        {
            _daMgr = daMgr;
        }

        internal DataAccessMgr DaMgr
        {
            get { return _daMgr; }
        }

        internal void Stop()
        {
            _stop = true;
        }

        internal void Start()
        {
            while (!_stop)
                Thread.Sleep(1000);
        }

        DataTable GetSessionControlData()
        {
            return _daMgr.ExecuteDataSet(_daMgr.BuildSelectDbCommand("select * from B1.AppSessionControl", null), null, null).Tables[0];
        }

    }
}
