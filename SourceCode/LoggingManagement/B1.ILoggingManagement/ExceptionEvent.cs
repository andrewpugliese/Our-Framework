using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace B1.ILoggingManagement
{
    /// <summary>
    /// Main class for Exceptions or other events that encapsulate the exception data
    /// as well as a message specific to the exception instance and a reference number
    /// that can be set by the logging Manager to indicate that this event was logged
    /// and that it can be referenced by the unique number.
    /// </summary>
    public class ExceptionEvent : Exception
    {
        enumExceptionEventCodes _exceptionEventCode;
        int _exceptionCode = Convert.ToInt32(enumExceptionEventCodes.UnknownException);
        string _msg = null;
        string _description = null;
        Int64? _referenceNumber = null;

        /// <summary>
        /// Returns the enum event code
        /// </summary>
        public enumExceptionEventCodes ExceptionEventCode
        {
            get { return _exceptionEventCode; }
        }

        /// <summary>
        /// Returns the integer number for the exception event.
        /// </summary>
        public Int32 ExceptionCode
        {
            get { return _exceptionCode; }
        }

        /// <summary>
        /// Returns the reference number for this exception event indicating that it had
        /// been logged and that number can be used to find the exception details in the system
        /// that it had been logged.  If it was not logged, this field will return null.
        /// </summary>
        public Int64? ReferenceNumber
        {
            get { return _referenceNumber; }
            set { _referenceNumber = value; }
        }

        /// <summary>
        /// Constructor accepting enumeration and message.
        /// </summary>
        /// <param name="exceptionEventCode">event enumeration that was defined in resource table</param>
        /// <param name="message">Specific message containing details of event</param>
        public ExceptionEvent(enumExceptionEventCodes exceptionEventCode, string message)
            : base(string.Format("{0}{1}{2}", exceptionEventCode.ToString()
                        , message
                        , Environment.NewLine))
        {
            _exceptionEventCode = exceptionEventCode;
            _exceptionCode = Convert.ToInt32(exceptionEventCode);
            _msg = message;
            _description = LookupDescription(exceptionEventCode);
            base.HResult = _exceptionCode;
        }

        /// <summary>
        /// Constructor accepting enumeration and message and an inner exception
        /// </summary>
        /// <param name="exceptionEventCode">event enumeration that was defined in resource table</param>
        /// <param name="description">Description of event or description</param>
        /// <param name="innerException">The exception object associated with the event.</param>
        public ExceptionEvent(enumExceptionEventCodes exceptionEventCode
                    , string description
                    , Exception innerException)
            : base(string.Format("{0}{1}{2}", exceptionEventCode.ToString()
                    , description
                    , Environment.NewLine)
                    , innerException)
        {
            _exceptionEventCode = exceptionEventCode;
            _exceptionCode = Convert.ToInt32(exceptionEventCode);
            _msg = Message;
            _description = description;
            base.HResult = _exceptionCode;
        }

        /// <summary>
        /// Returns all the properties of this object instance as a string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("Code: {0}{1}Event: {2}{1}Description: {3}{1}Message: {4}{1}"
                                    + "StackTrace: {5}{1}InnerException: {6}{1}"
                , _exceptionCode
                , Environment.NewLine
                , _exceptionEventCode
                , _description
                , _msg
                , base.StackTrace
                , base.InnerException != null ? ConvertExceptionToString(base.InnerException) : string.Empty);
        }

        /// <summary>
        /// Returns the string version of an exception (including all inner exceptions)
        /// </summary>
        /// <param name="e">Exception object</param>
        /// <returns>String version of exception object information</returns>
        public static string ConvertExceptionToString(Exception e)
        {
            return e.Message + e.StackTrace + ((e.InnerException != null) 
                                ? ConvertExceptionToString(e.InnerException) : string.Empty);
        }

        string LookupDescription(enumExceptionEventCodes exceptionEventCode)
        {
            try
            {
                return EventCodes.ResourceManager.GetString(exceptionEventCode.ToString());
            }
            catch (Exception e)
            {
                return exceptionEventCode.ToString() + " was NOT found in the exception resource table." + e.Message;
            }
        }
    }


    #pragma warning restore 1591 // disable the xmlComments warning
}
