using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

using B1.ILoggingManagement;
using B1.LoggingManagement;

namespace B1.TraceViewer
{
    public partial class TraceViewer
    {
        private enum PagingAction
        {
            Next,
            Previous
        }

        private static bool _stopFind = false;
        private static bool _stopTracePage = false;
        private int _pageSize = 50;
        private int _currentPageTopOffset = 0;
        private int _currentPageBottomOffset = 0;

        IAsyncResult _tracingPageResult;
        Action<PagingAction> _tracingPageDelegate;
        System.Threading.Timer _numTraceMessagesTimer;

        Font _originalFont;
        Font _boldFont;

        private void Init()
        {
            _tracingPageDelegate = new Action<PagingAction>(TracingPageThread);
            
            udTracePageSize.Value = _pageSize;
            _originalFont = rtbTrace.Font;
            _boldFont = new Font(rtbTrace.Font, FontStyle.Bold);
            PageTracing(PagingAction.Next);

            try
            {
                int n = MemoryMappedLog.TraceReader.NumMessages;
            }
            catch(IOException)
            {
                MessageBox.Show("Cannot open more than one trace viewer.", "TraceViewer already open.", MessageBoxButtons.OK, 
                        MessageBoxIcon.Stop);
                System.Windows.Forms.Application.Exit();
            }


            _numTraceMessagesTimer = new System.Threading.Timer(
                    o =>
                        tbNumTraceMessages.BeginInvoke(new Action(
                            () => tbNumTraceMessages.Text = MemoryMappedLog.TraceReader.NumMessages.ToString())),
                    null, 0, 1000);
        }

        private void ShutDown()
        {
            _stopTracePage = true;
            _stopFind = true;

            if(_tracingPageResult != null && _tracingPageDelegate != null)
                    _tracingPageDelegate.EndInvoke(_tracingPageResult);

            if(_numTraceMessagesTimer != null)
                _numTraceMessagesTimer.Dispose();
        }

        
        private void PageTracing(PagingAction action)
        {
            _stopTracePage = true;
            StopFind();

            lock(btnNext)
            {
                btnNext.Enabled = false;
                btnLast.Enabled = false;
                btnPrevious.Enabled = false;
                btnTop.Enabled = false;
                
                if(_tracingPageResult != null && _tracingPageDelegate != null)
                    _tracingPageDelegate.EndInvoke(_tracingPageResult);

                _stopTracePage = false;
                _tracingPageResult = _tracingPageDelegate.BeginInvoke(action,
                        new AsyncCallback(new Action<IAsyncResult>(
                            a =>
                            { 
                                BeginInvoke(new Action( () =>
                                    {
                                        btnNext.Enabled = true;
                                        btnLast.Enabled = true;

                                        if(_currentPageTopOffset > 0 &&
                                                MemoryMappedLog.TraceReader.NextReadOffset > 0)
                                        {
                                            btnPrevious.Enabled = true;
                                            btnTop.Enabled = true;
                                        }
                                    }));
                            })), null);
                
            }
               
        }

        private void TracingPageThread(PagingAction action)
        {
            int lastOffset = 0;

            if(action == PagingAction.Previous)
            {
                _currentPageBottomOffset = _currentPageTopOffset;
                lastOffset = _currentPageTopOffset;
            }
            else
            {
                _currentPageTopOffset = _currentPageBottomOffset;

                lastOffset = _currentPageBottomOffset;
            }

            rtbTrace.Invoke(new Action( () => rtbTrace.Text = ""));
                        
            int numMessagesOutstanding = _pageSize;// + (action == PagingAction.Previous ? 1 : 0);

            while(!_stopTracePage && numMessagesOutstanding > 0)
            {
                List<string> messages;

                if(action == PagingAction.Next)
                {
                    messages = MemoryMappedLog.TraceReader.ReadNextMessages(
                            numMessagesOutstanding, lastOffset);

                    lastOffset = MemoryMappedLog.TraceReader.NextReadOffset;

                    // For next, we wait until N number of messages have been received.
                    numMessagesOutstanding -= messages.Count;
                }
                else
                {
                    messages = MemoryMappedLog.TraceReader.ReadPreviousMessages(
                            numMessagesOutstanding, lastOffset);

                    numMessagesOutstanding -= messages.Count;

                    if(numMessagesOutstanding > 0)
                    {
                        messages.AddRange(MemoryMappedLog.TraceReader.ReadNextMessages(numMessagesOutstanding, lastOffset));

                        _currentPageBottomOffset = MemoryMappedLog.TraceReader.NextReadOffset;
                    }

                    // For previous, we only try to get N number of previous messages once
                    numMessagesOutstanding = 0; 
                }
                
                IAsyncResult appendResult = rtbTrace.BeginInvoke(new Action( () =>
                    { 
                        foreach(string message in messages)
                        {
                            if(_stopTracePage)
                                break;

                            AddTraceMessage(message);
                        }
                    }));

                Thread.Sleep(1);
            }

            if(action == PagingAction.Previous)
                _currentPageTopOffset = MemoryMappedLog.TraceReader.NextReadOffset;
            else
                _currentPageBottomOffset = MemoryMappedLog.TraceReader.NextReadOffset;
        }

        private void AddTraceMessage(string message)
        {
            AddTraceMessage(message, false);
        }

        private void AddTraceMessage(string message, bool reverse)
        {
            TraceMessage traceMessage = TraceLog.GetTraceMessage(message);

            string level = new string('-', traceMessage.Context == null ? 1 : 
                    (traceMessage.Context.Count() + 1) * 4);

            if(reverse)
                rtbTrace.SelectionStart = 0;

            rtbTrace.AppendText("\r\n");

            StringBuilder context = new StringBuilder();

            if(traceMessage.Context != null)
                foreach(string c in traceMessage.Context)
                    context.AppendFormat("{0}{1}", context.Length > 0 ? "." : "", c);

            if(context.Length == 0)
                context.Append("NO CONTEXT");

            rtbTrace.AppendText(level); 
            
            rtbTrace.SelectionFont = _boldFont;
            rtbTrace.AppendText(string.Format("{0}\r\n",context));
            rtbTrace.SelectionFont = _originalFont;

            rtbTrace.AppendText(string.Format("{0}Time: {1}, MachineName: {2}\r\n", level, traceMessage.Time, 
                    traceMessage.MachineName));
            rtbTrace.AppendText(string.Format("{0}ProcessId: {1}, ProcessName: {2}, ThreadId: {3}\r\n\r\n", 
                    level, traceMessage.ProcessId,traceMessage.ProcessName, traceMessage.ThreadId));

            rtbTrace.SelectionColor = Color.Blue;

            rtbTrace.AppendText(traceMessage.Message);

            rtbTrace.AppendText("\r\n");
        }

        private void FirstPage()
        {
            _currentPageBottomOffset = 0;
            PageTracing(PagingAction.Next);
        }

        private void LastPage()
        {
            _currentPageTopOffset = MemoryMappedLog.TraceReader.GetCurrentWriteOffset();
            PageTracing(PagingAction.Previous);
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="context"></param>
        /// <param name="message"></param>
        /// <param name="processName"></param>
        /// <param name="machineName"></param>
        /// <param name="processId"></param>
        /// <param name="threadId"></param>
        /// <returns>IAsyncResult to wait for find to finish.</returns>
        public Func<int> FindNext(int offset = 0, string context = null, string message = null, 
                string processName = null, string machineName = null, string processId = null, string threadId = null)
        {
            _stopFind = false;

            return new Func<int>(() =>
                    {
                        if(string.IsNullOrEmpty(context) && string.IsNullOrEmpty(message)
                                && string.IsNullOrEmpty(processName) && string.IsNullOrEmpty(machineName)
                                && string.IsNullOrEmpty(threadId) && string.IsNullOrEmpty(processId))
                            return -1;

                        BeginInvoke(new Action(() =>
                                {
                                    btnNext.Enabled = false;
                                    btnLast.Enabled = false;
                                    btnPrevious.Enabled = false;
                                    btnTop.Enabled = false;
                                }));

                        int previousOffset = offset;
                        int currentOffset = previousOffset;
                        bool found = false;
                        
                        while(!_stopFind && !found)
                        {
                            previousOffset = currentOffset;
                            string nextMessage = MemoryMappedLog.TraceReader.ReadNextMessage(previousOffset);
                            currentOffset = MemoryMappedLog.TraceReader.NextReadOffset;

                            if(nextMessage == null)
                            {
                                BeginInvoke(new Action(() =>
                                    {
                                        btnNext.Enabled = true;
                                        btnLast.Enabled = true;

                                        if(_currentPageTopOffset > 0 &&
                                                MemoryMappedLog.TraceReader.NextReadOffset > 0)
                                        {
                                            btnPrevious.Enabled = true;
                                            btnTop.Enabled = true;
                                        }
                                    }));
                        
                                return -1;
                            }

                            TraceMessage traceMessage = TraceLog.GetTraceMessage(nextMessage);

                            if((string.IsNullOrEmpty(context) || traceMessage.Context.Count( 
                                        c => c.ToLower().Contains(context.ToLower())) > 0)
                                    && (string.IsNullOrEmpty(message) 
                                        || traceMessage.Message.ToLower().Contains(message.ToLower()))
                                    && (string.IsNullOrEmpty(processName) 
                                        || traceMessage.ProcessName.ToLower().Contains(processName.ToLower()))
                                    && (string.IsNullOrEmpty(machineName) 
                                        || traceMessage.MachineName.ToLower().Contains(machineName.ToLower()))
                                    && (string.IsNullOrEmpty(threadId) 
                                        || traceMessage.ThreadId.ToString().ToLower().Contains(threadId.ToLower()))
                                    && (string.IsNullOrEmpty(processId) 
                                        || traceMessage.ProcessId.ToString().ToLower().Contains(processId.ToLower())))
                            {
                                found = true;
                                _currentPageBottomOffset = previousOffset;
                
                                TracingPageThread(PagingAction.Next);

                                BeginInvoke(new Action(() =>
                                        {
                                            btnNext.Enabled = true;
                                            btnLast.Enabled = true;
                                        }));

                                if(_currentPageTopOffset > 0 &&
                                        MemoryMappedLog.TraceReader.NextReadOffset > 0)
                                {
                                    
                                    BeginInvoke(new Action(() =>
                                            {
                                                btnPrevious.Enabled = true;
                                                btnTop.Enabled = true;
                                            }));

                                }
                                    
                            }
                        }
                        return currentOffset;
                    });
        }

        /// <summary>
        /// Stop find
        /// </summary>
        public void StopFind()
        {
            _stopFind = true;
        }
    }
}
