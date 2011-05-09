//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace B1.Data.Models
{
    public partial class SignonControl
    {
        #region Primitive Properties
    
        public virtual byte ControlCode
        {
            get;
            set;
        }
    
        public virtual bool RestrictSignon
        {
            get;
            set;
        }
    
        public virtual bool ForceSignoff
        {
            get;
            set;
        }
    
        public virtual short StatusSeconds
        {
            get;
            set;
        }
    
        public virtual short TimeoutSeconds
        {
            get;
            set;
        }
    
        public virtual byte FailedAttemptLimit
        {
            get;
            set;
        }
    
        public virtual string RestrictSignonMsg
        {
            get;
            set;
        }
    
        public virtual string SignoffWarningMsg
        {
            get;
            set;
        }
    
        public virtual Nullable<int> LastModifiedUserCode
        {
            get { return _lastModifiedUserCode; }
            set
            {
                try
                {
                    _settingFK = true;
                    if (_lastModifiedUserCode != value)
                    {
                        if (UserMaster != null && UserMaster.UserCode != value)
                        {
                            UserMaster = null;
                        }
                        _lastModifiedUserCode = value;
                    }
                }
                finally
                {
                    _settingFK = false;
                }
            }
        }
        private Nullable<int> _lastModifiedUserCode;
    
        public virtual Nullable<System.DateTime> LastModifiedDateTime
        {
            get;
            set;
        }

        #endregion
        #region Navigation Properties
    
        public virtual UserMaster UserMaster
        {
            get { return _userMaster; }
            set
            {
                if (!ReferenceEquals(_userMaster, value))
                {
                    var previousValue = _userMaster;
                    _userMaster = value;
                    FixupUserMaster(previousValue);
                }
            }
        }
        private UserMaster _userMaster;

        #endregion
        #region Association Fixup
    
        private bool _settingFK = false;
    
        private void FixupUserMaster(UserMaster previousValue)
        {
            if (previousValue != null && previousValue.SignonControls.Contains(this))
            {
                previousValue.SignonControls.Remove(this);
            }
    
            if (UserMaster != null)
            {
                if (!UserMaster.SignonControls.Contains(this))
                {
                    UserMaster.SignonControls.Add(this);
                }
                if (LastModifiedUserCode != UserMaster.UserCode)
                {
                    LastModifiedUserCode = UserMaster.UserCode;
                }
            }
            else if (!_settingFK)
            {
                LastModifiedUserCode = null;
            }
        }

        #endregion
    }
}