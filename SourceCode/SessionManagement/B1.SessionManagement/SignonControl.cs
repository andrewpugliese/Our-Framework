using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Data;

using B1.DataAccess;

namespace B1.SessionManagement
{
    /// <summary>
    /// Data Strcuture representing the Signon Control attributes
    /// </summary>
    public struct SignonControlStructure
    {
        /// <summary>
        /// Unique Key for the record;  (There is only 1 record)
        /// </summary>
        public byte SessionControlCode;
        /// <summary>
        /// Number of seconds before a session can be considered in active
        /// </summary>
        public int TimeOutSeconds;
        /// <summary>
        /// Number of seconds for the heartbeat status interval
        /// </summary>
        public int StatusSeconds;
        /// <summary>
        /// Max number of subsequent failed Signon attempt before account local
        /// </summary>
        public byte FailedAttemptLimit;
        /// <summary>
        /// Warning message to be displayed when there will be a forced signoff
        /// </summary>
        public string SignoffWarningMsg;
        /// <summary>
        /// Message to be displayed when signons are restricted
        /// </summary>
        public string RestrictSignonMsg;
        /// <summary>
        /// Boolean indicating that all signons must be signed off immediately
        /// </summary>
        public bool ForceSignoff;
        /// <summary>
        /// Boolean indicating that new signons must be restricted
        /// </summary>
        public bool RestrictSignon;
        /// <summary>
        /// UserCode of the user which modified the record
        /// </summary>
        public int? LastModifiedByUserCode;
        /// <summary>
        /// DateTime of the last modification
        /// </summary>
        public DateTime? LastModifiedByDateTime;
    }

    /// <summary>
    /// Class that encapsulates the maintenance of the B1.SignonControl database table
    /// This class is managed by the AppSession class so it only has 1 public method
    /// which returns the data structure of current settings.
    /// </summary>
    public class SignonControl
    {
        DataAccessMgr _daMgr = null;
        SignonControlStructure _signonControl;

        /// <summary>
        /// The only constructor.
        /// </summary>
        /// <param name="daMgr">DataAccessMgr object</param>
        internal SignonControl(DataAccessMgr daMgr)
        {
            _daMgr = daMgr;
            DataTable dt = _daMgr.ExecuteDataSet(_daMgr.BuildSelectDbCommand(
                    "select * from " + DataAccess.Constants.SCHEMA_CORE + "." + Constants.SignonControl
                    , null), null, null).Tables[0];
            Refresh(dt);
        }

        /// <summary>
        /// Updates the SignonControl data structure with the latest data from the database
        /// </summary>
        /// <param name="dt">DataTable of the SignonControl table in the database</param>
        internal void Refresh(DataTable dt)
        {
            _signonControl = new SignonControlStructure();
            _signonControl.RestrictSignonMsg = dt.Rows[0][SessionManagement.Constants.RestrictSignonMsg].ToString();
            _signonControl.SignoffWarningMsg = dt.Rows[0][SessionManagement.Constants.SignoffWarningMsg].ToString();
            _signonControl.FailedAttemptLimit = Convert.ToByte(dt.Rows[0][SessionManagement.Constants.FailedAttemptLimit]);
            _signonControl.SessionControlCode = Convert.ToByte(dt.Rows[0][SessionManagement.Constants.ControlCode]);
            _signonControl.StatusSeconds = Convert.ToInt32(dt.Rows[0][SessionManagement.Constants.StatusSeconds]);
            _signonControl.TimeOutSeconds = Convert.ToInt32(dt.Rows[0][SessionManagement.Constants.TimeoutSeconds]);
            _signonControl.ForceSignoff = Convert.ToBoolean(dt.Rows[0][SessionManagement.Constants.ForceSignOff]);
            _signonControl.RestrictSignon = Convert.ToBoolean(dt.Rows[0][SessionManagement.Constants.RestrictSignon]);
            _signonControl.LastModifiedByUserCode = null;
            if (dt.Rows[0][SessionManagement.Constants.LastModifiedUserCode] != DBNull.Value)
                _signonControl.LastModifiedByUserCode = Convert.ToInt32(dt.Rows[0][SessionManagement.Constants.LastModifiedUserCode]);
            if (dt.Rows[0][SessionManagement.Constants.LastModifiedDateTime] != DBNull.Value)
                _signonControl.LastModifiedByDateTime = Convert.ToDateTime(dt.Rows[0][SessionManagement.Constants.LastModifiedDateTime]);
        }

        /// <summary>
        /// Returns the Signon Control Data Structure
        /// </summary>
        public SignonControlStructure SignonControlData
        {
            get { return _signonControl; }
        }

        /// <summary>
        /// Method with change the settings of the SignonControl table to the given new settings only if they have not already been changed.
        /// <para>If </para>
        /// </summary>
        /// <param name="lastModifiedUser">The userCode of the user who last updated the record (or null if not changed)</param>
        /// <param name="lastModifiedDateTime">The DateTime of when the user last updated the record (or null if not changed)</param>
        /// <param name="newControlSettings">Signon Control Data Strcutre containing the new values.</param>
        /// <returns>Boolean indicating if record was changed or not.</returns>
        internal bool ChangeControl(int? lastModifiedByUserCode
                , DateTime? lastModifiedDateTime
                , SignonControlStructure newControlSettings)
        {
            DbTableDmlMgr dmlChange = _daMgr.DbCatalogGetTableDmlMgr(DataAccess.Constants.SCHEMA_CORE
                    , Constants.SignonControl);

            dmlChange.AddColumn(Constants.TimeoutSeconds
                    , _daMgr.BuildParamName(Constants.TimeoutSeconds));
            dmlChange.AddColumn(Constants.SignoffWarningMsg
                    , _daMgr.BuildParamName(Constants.SignoffWarningMsg));
            dmlChange.AddColumn(Constants.ForceSignOff
                    , _daMgr.BuildParamName(Constants.ForceSignOff));
            dmlChange.AddColumn(Constants.RestrictSignon
                    , _daMgr.BuildParamName(Constants.RestrictSignon));
            dmlChange.AddColumn(Constants.StatusSeconds
                    , Constants.StatusSeconds);
            dmlChange.AddColumn(Constants.RestrictSignonMsg
                    , Constants.RestrictSignonMsg);
            dmlChange.AddColumn(Constants.FailedAttemptLimit
                    , Constants.FailedAttemptLimit);

            dmlChange.SetWhereCondition((j) =>
                    j.Column(Constants.SignonControl, Constants.ControlCode) ==
                    j.Parameter(Constants.SignonControl, Constants.ControlCode,
                        _daMgr.BuildParamName(Constants.ControlCode)));

            DbCommandMgr dbCmdMgr = new DbCommandMgr(_daMgr);
            DbCommand cmdChangeOrig = _daMgr.BuildChangeDbCommand(dmlChange
                    , new DbQualifiedObject<string>(DataAccess.Constants.SCHEMA_CORE
                        , Constants.SignonControl, Constants.LastModifiedUserCode)
                    , new DbQualifiedObject<string>(DataAccess.Constants.SCHEMA_CORE
                        , Constants.SignonControl, Constants.LastModifiedDateTime));

            cmdChangeOrig.Parameters[_daMgr.BuildParamName(Constants.ControlCode)].Value
                    = newControlSettings.SessionControlCode;
            cmdChangeOrig.Parameters[_daMgr.BuildParamName(Constants.TimeoutSeconds)].Value
                    = Convert.ToInt16(newControlSettings.TimeOutSeconds);
            cmdChangeOrig.Parameters[_daMgr.BuildParamName(Constants.SignoffWarningMsg)].Value
                    = newControlSettings.SignoffWarningMsg;
            cmdChangeOrig.Parameters[_daMgr.BuildParamName(Constants.ForceSignOff)].Value
                    = newControlSettings.ForceSignoff;
            cmdChangeOrig.Parameters[_daMgr.BuildParamName(Constants.RestrictSignon)].Value
                    = newControlSettings.RestrictSignon;
            cmdChangeOrig.Parameters[_daMgr.BuildParamName(Constants.RestrictSignonMsg)].Value
                    = newControlSettings.RestrictSignonMsg;
            cmdChangeOrig.Parameters[_daMgr.BuildParamName(Constants.StatusSeconds)].Value
                    = newControlSettings.StatusSeconds;
            cmdChangeOrig.Parameters[_daMgr.BuildParamName(Constants.FailedAttemptLimit)].Value
                    = newControlSettings.FailedAttemptLimit;
            if (lastModifiedByUserCode.HasValue)
                cmdChangeOrig.Parameters[_daMgr.BuildParamName(Constants.LastModifiedUserCode)].Value
                        = lastModifiedByUserCode.Value;
            if (lastModifiedDateTime.HasValue)
                cmdChangeOrig.Parameters[_daMgr.BuildParamName(Constants.LastModifiedDateTime)].Value
                        = lastModifiedDateTime.Value;
            if (newControlSettings.LastModifiedByUserCode.HasValue)
                cmdChangeOrig.Parameters[_daMgr.BuildParamName(Constants.LastModifiedUserCode, true)].Value
                        = newControlSettings.LastModifiedByUserCode.Value;
            cmdChangeOrig.Parameters[_daMgr.BuildParamName(Constants.LastModifiedDateTime, true)].Value
                    = newControlSettings.LastModifiedByDateTime.Value;
            
            int rowsAffected = _daMgr.ExecuteNonQuery(cmdChangeOrig, null, null);

            if (rowsAffected == 1)
            {
                _signonControl.FailedAttemptLimit = newControlSettings.FailedAttemptLimit;
                _signonControl.ForceSignoff = newControlSettings.ForceSignoff;
                _signonControl.RestrictSignon = newControlSettings.RestrictSignon;
                _signonControl.RestrictSignonMsg = newControlSettings.RestrictSignonMsg;
                _signonControl.SignoffWarningMsg = newControlSettings.SignoffWarningMsg;
                _signonControl.StatusSeconds = newControlSettings.StatusSeconds;
                _signonControl.TimeOutSeconds = newControlSettings.TimeOutSeconds;
                _signonControl.LastModifiedByUserCode = newControlSettings.LastModifiedByUserCode;
                _signonControl.LastModifiedByDateTime = newControlSettings.LastModifiedByDateTime;

                return true;
            }
            else return false;
        }
    }
}
