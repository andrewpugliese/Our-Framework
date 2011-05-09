using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using B1.ILoggingManagement;
using B1.LoggingManagement;

namespace B1.TraceViewer
{
    /// <summary>
    /// This Windows application provides the ability to view and search this machines trace log. There can only be 
    /// one instance of this application running at once. Once this application starts up, any process calling the Write  
    /// method(s) of the <see cref="B1.LoggingManagement.TraceLog">TraceLog</see> class, will start writing to this log.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }

    #pragma warning disable 1591 // disable the xmlComments warning
    /// <summary>
    /// Winform class for viewing memory mapped file tracing.
    /// </summary>
    public partial class TraceViewer : Form
    {
        Find _find = null;

        public TraceViewer()
        {
            InitializeComponent();
        }
        
        protected override void OnClosing(CancelEventArgs e)
        {
            ShutDown();

            base.OnClosing(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            Init();
            _find = new Find(this);
            base.OnLoad(e);
        }
       
        private void btnNext_Click(object sender, EventArgs e)
        {
            PageTracing(PagingAction.Next);
        }

        private void udTracePageSize_ValueChanged(object sender, EventArgs e)
        {
            _pageSize = (int)udTracePageSize.Value;
        }

        private void btnTop_Click(object sender, EventArgs e)
        {
            FirstPage();
        }

        private void btnPrevious_Click(object sender, EventArgs e)
        {
            PageTracing(PagingAction.Previous);
        }

        private void btnFind_Click(object sender, EventArgs e)
        {
            if(_find.IsDisposed)
                _find = new Find(this);

            _find.Show();
        }

        private void btnLast_Click(object sender, EventArgs e)
        {
            LastPage();
        }
    }
    #pragma warning restore 1591 // disable the xmlComments warning
}
    