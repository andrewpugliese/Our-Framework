using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using B1.ILoggingManagement;

namespace B1.CacheManagement
{
    /// <summary>
    /// This class stores the calback item which needs to be called periodically.
    /// </summary>
    internal class CallbackItem
    {
        string _identifier;
        Action<string> _funcToCall;
        int _timerCycleCount;

        int _cycleCount = 0;

        public CallbackItem(string identifier, Action<string> funcToCall, int timerCycleCount)
        {
            _identifier = identifier;
            _funcToCall = funcToCall;
            _timerCycleCount = timerCycleCount;
        }

        public void TimeElapsed()
        {
            _cycleCount ++;
            if (_cycleCount == _timerCycleCount)
            {
                _cycleCount = 0;

                // Call function in a default threadpool thread to avoid long running function to hold the timer
                System.Threading.ThreadPool.QueueUserWorkItem(state => _funcToCall(_identifier));
            }
        }
    }

    /// <summary>
    /// This class manages multiple callback functions which need to be called periodically. This class lazily
    /// instantiates one server timer which sends out tick every 10 seconds. Each CallbackItem computes
    /// how many ticks are required before they need to call the registered function and calls them when that many ticks
    /// are received by them. This allows multiple periodic function with one single timer. Cache manager can use this
    /// function to update its cache values. Configuration manager can refresh the configuration values from database
    /// at certain intervals. The timings are in the increment of 10 seconds and will not be exact.
    /// </summary>
    public class RecurringCallbackMgr
    {
        // This holds the global AppConfigMgr instance - This class is a singleton
        private static RecurringCallbackMgr _defaultInstance = null;

        int _timerSecs = 10;
        System.Timers.Timer _serverTimer = null;
        Dictionary<string, CallbackItem> _callbackItems = new Dictionary<string, CallbackItem>();

        ILoggingMgr _loggingMgr = null;

        /// <summary>
        /// Constructor for creating a RecurringCallbackManager. 
        /// </summary>
        public RecurringCallbackMgr(ILoggingMgr loggingMgr = null)
        {
            _loggingMgr = loggingMgr;
        }

        /// <summary>
        /// Lazily created instance of a defualt RecurringCallbackMgr which is available to the application using this static
        /// property. 
        /// </summary>
        public static RecurringCallbackMgr Default
        {
            get
            {
                // If already instanciated then return the settings
                if (_defaultInstance != null) return _defaultInstance;

                // Lazy initialize this class - lock to make it thread-safe
                lock (typeof(RecurringCallbackMgr))
                {
                    // Check if another thread already created the instance - if so return it
                    if (_defaultInstance != null) return _defaultInstance;

                    _defaultInstance = new RecurringCallbackMgr();
                }

                return _defaultInstance;
            }
        }

        /// <summary>
        /// Add a function which will be called at the specified interval seconds. 
        /// </summary>
        public void Add(string identifier, Action<string> funcToCall, int intervalSecs)
        {
            // Calculate how many timer cycle will need to call the function
            int remainder;
            int cycleFrequency = Math.DivRem(intervalSecs, _timerSecs, out remainder);
            if (remainder > 0 || cycleFrequency == 0) ++cycleFrequency;

            // Wrap the function to call in try catch block so that unhandled exception in user function does not
            // trash the timer.
            funcToCall = id =>
            {
                try { funcToCall(id); }
                catch (Exception ex) { if (_loggingMgr != null) _loggingMgr.WriteToLog(ex); }
            };

            lock (_callbackItems)
            {
                _callbackItems.Add(identifier, new CallbackItem(identifier, funcToCall, cycleFrequency));
            }

            // Start the timer if it is NOT already started
            if (_serverTimer == null)
            {
                lock (this)
                {
                    if (_serverTimer == null)
                    {
                        _serverTimer = new System.Timers.Timer(_timerSecs * 1000);
                        _serverTimer.Elapsed += new System.Timers.ElapsedEventHandler(_serverTimer_Elapsed);
                        _serverTimer.Start();
                    }
                }
            }
        }

        /// <summary>
        /// Remove the recurring callback function. 
        /// </summary>
        public void Remove(string identifier)
        {
            lock (_callbackItems)
            {
                _callbackItems.Remove(identifier);
            }
        }

        void _serverTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            // Send TimeElapsed to all the items
            lock (_callbackItems)
            {
                _callbackItems.Values.ToList().ForEach(item => item.TimeElapsed());
            }
        }
    }
}
