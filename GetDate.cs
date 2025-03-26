using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class GetDate : DevExpress.XtraEditors.XtraForm
    {
        /****************************************************************************************/
        private DateTime workDate = DateTime.Now;
        private string workTitle = "";
        private int workWidth = 0;
        private DateControl dc = null;
        private bool workClose = false;
        public DateTime myDateAnswer { get { return workDate; } }
        public string action = "";
        /****************************************************************************************/
        public GetDate( DateTime date, string title = "", int extraWidth = 0, bool focusClose = false )
        {
            InitializeComponent();
            workDate = date;
            workTitle = title;
            workWidth = extraWidth;
            workClose = focusClose;
        }
        /****************************************************************************************/
        private void GetDate_Load(object sender, EventArgs e)
        {
            if (String.IsNullOrWhiteSpace(workTitle))
                workTitle = "Select Date";
            this.Text = workTitle;

            dc = new DateControl();
            dc.Size = dc.CalcBestSize();
            if ( workWidth > 0 )
            {
                Rectangle rect = this.Bounds;
                this.SetBounds(rect.Left, rect.Top, dc.Width + workWidth, rect.Height);
                rect = dc.Bounds;
            }
            dc.DateTime = new DateTime(2008, 1, 1);
            dc.DateTime = workDate;
            Controls.Add(dc);

            if (workClose)
                btnClose.Focus();
            this.action = "Ignore";
        }
        /****************************************************************************************/
        private void button1_Click(object sender, EventArgs e)
        {
            workDate = dc.DateTime;
            this.DialogResult = DialogResult.OK;
            this.action = "OK";
            this.Close();
        }
        /****************************************************************************************/
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Ignore;
            this.action = "Ignore";
            this.Close();
        }
        /****************************************************************************************/
        private void btnClear_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.action = "Cancel";
            this.Close();
        }
        /****************************************************************************************/
        private void GetDate_FormClosing(object sender, FormClosingEventArgs e)
        {
        }
        /****************************************************************************************/
    }
}