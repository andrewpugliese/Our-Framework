using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace B1.TraceViewer
{
    /// <summary>
    /// Winform class for searching the trace viewer.
    /// </summary>
    public partial class Find : Form
    {
        TraceViewer _traceViewer = null;
        int _lastFindOffset = 0;

        IAsyncResult _endFindResult = null;
        Func<int> _endFind = null;


        /// <summary>
        /// Instantiates a Find Winforms class.
        /// </summary>
        /// <param name="traceViewer">reference to TraceViewer form</param>
        public Find(TraceViewer traceViewer)
        {
            _traceViewer = traceViewer;

            InitializeComponent();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(CancelEventArgs e)
        {
            _traceViewer.StopFind();

            if(_endFindResult != null)
                _endFindResult.AsyncWaitHandle.WaitOne();

            base.OnClosing(e);
        }

        private void btnFindNext_Click(object sender, EventArgs e)
        {
            
            btnFindNext.Enabled = false;
            btnStopFind.Enabled = true;
            FindNext();
        }

        private void btnStopFind_Click(object sender, EventArgs e)
        {
            _traceViewer.StopFind();
            _endFindResult.AsyncWaitHandle.WaitOne();
            btnFindNext.Enabled = true;
            btnStopFind.Enabled = false;
        }

        private void FindNext()
        {
            int startingOffset = _lastFindOffset;
            Func<int> findFunc = _traceViewer.FindNext(_lastFindOffset, tbContext.Text, tbMessage.Text);
            IAsyncResult findResult = findFunc.BeginInvoke(null, null);

            if(_endFind != null && _endFindResult != null)
                _endFindResult.AsyncWaitHandle.WaitOne();

            _endFind = new Func<int>( () =>
                    {
                        findResult.AsyncWaitHandle.WaitOne();

                        int offset = findFunc.EndInvoke(findResult);
                        if(offset >= 0)
                            _lastFindOffset = offset;

                        BeginInvoke(new Action( () =>
                                { 
                                    btnFindNext.Enabled = true;
                                    btnStopFind.Enabled = false;
                                }));
                        return offset;


                    });

            _endFindResult = _endFind.BeginInvoke(new AsyncCallback(
                    r => 
                    {
                        int offset = _endFind.EndInvoke(r);

                        if(startingOffset == 0 && offset < 0)
                        {
                            BeginInvoke(new Action( () =>
                                    MessageBox.Show(this, "No results matched search criteria.", "No results", 
                                        MessageBoxButtons.OK, 
                                        MessageBoxIcon.Information)));
                        }
                        else if(startingOffset > 0 && offset < 0)
                        {
                            IAsyncResult askSearchAgain = BeginInvoke(new Func<DialogResult>( () =>
                                    MessageBox.Show(this, "Reached end of file. Would you like to search from beginning.", 
                                            "No results", MessageBoxButtons.YesNo, 
                                            MessageBoxIcon.Question)));

                            DialogResult result = (DialogResult)EndInvoke(askSearchAgain);
                            if(result == System.Windows.Forms.DialogResult.Yes)
                            {
                                BeginInvoke(new Action( () =>
                                { 
                                    btnFindNext.Enabled = false;
                                    btnStopFind.Enabled = true;
                                }));

                                _lastFindOffset = 0;
                                FindNext();
                            }
                        }
                    }), null);
        }
    }
}
