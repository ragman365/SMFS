using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GeneralLib;
/************************************************************************************/
namespace SMFS
{
/************************************************************************************/
    public partial class PleaseWait : Form
    {
        private string workMessage = "";
        private bool workError = false;
/************************************************************************************/
        public PleaseWait( string message = "", bool error = false )
        {
            InitializeComponent();
            workMessage = message;
            workError = error;
        }
/************************************************************************************/
        private void PleaseWait_Load(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            if (!String.IsNullOrWhiteSpace(workMessage))
            {
                label1.Text = workMessage;
                label1.Refresh();
            }
            if (workError)
                this.BackColor = Color.Red;

            string measureString = label1.Text;
            Font stringFont = label1.Font;

            // Measure string.
            SizeF stringSize = new SizeF();
            Graphics g = label1.CreateGraphics();
            stringSize = g.MeasureString(measureString, stringFont);

            int top = this.Top;
            int left = this.Left;
            int height = this.Height;
            double dWidth = stringSize.Width;
            int width = Convert.ToInt32(dWidth);

            this.SetBounds(left, top, width+50, height);
        }
        /***************************************************************************************/
        public void FireEvent2( string message = "" )
        {
            if (String.IsNullOrWhiteSpace(message))
                message = "Please Wait!";

            string measureString = message;
            Font stringFont = label1.Font;

            // Measure string.
            SizeF stringSize = new SizeF();
            Graphics g = label1.CreateGraphics();
            stringSize = g.MeasureString(measureString, stringFont);

            int top = this.Top;
            int left = this.Left;
            int height = this.Height;
            double dWidth = stringSize.Width;
            int width = Convert.ToInt32(dWidth);

            this.SetBounds(left, top, width + 50, height);

            label1.Text = message;
            label1.Refresh();
        }
        /***************************************************************************************/
        public void FireEvent1()
        {
            label1.Text = "";
            label1.Refresh();

            int top = this.Top;
            int left = this.Left;

            this.SetBounds(left, top, 1, 1);
            this.Refresh();

            this.Cursor = Cursors.Default;
            this.Close();
        }
/************************************************************************************/
    }
}
