using System;
using System.Data;
using System.Windows.Forms;
using GeneralLib;

using System.Drawing;
using DevExpress.XtraPrinting;
using DevExpress.Utils;
using DevExpress.Pdf;
using MySql.Data.MySqlClient;
using System.Text;
using System.IO;
using DevExpress.XtraRichEdit;
using System.Drawing.Printing;
using EMRControlLib;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class ViewRTF : DevExpress.XtraEditors.XtraForm
    {
        private string rtfText = "";
        private string workRecord = "";
        /***********************************************************************************************/
        public ViewRTF( string text )
        {
            InitializeComponent();
            rtfText = text;
        }
        /***********************************************************************************************/
        public ViewRTF(string record, string text)
        {
            InitializeComponent();
            rtfText = text;
            workRecord = record;
        }
        /***********************************************************************************************/
        private void ViewRTF_Load(object sender, EventArgs e)
        {
            rtb.RichTextBox.AppendRtf(rtfText);
            rtb.RichTextBox.Modified = false;
            rtb.RichTextBox.ScrollToCaret();
        }
        /***********************************************************************************************/
        private void BuildPrintReport(bool clear = true)
        {
            if (clear)
                rtbPrint.RichTextBox.Clear();
            rtbPrint.RichTextBox.AppendRtf(rtb.RichTextBox.Rtf);
        }
        /***********************************************************************************************/
        private EMRRichTextBox rtbPrint = new EMRRichTextBox();
        private int checkPrint;
        private int pageNumber;
        /***********************************************************************************************/
        private void printPreviewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BuildPrintReport();
            printPreviewDialog1.BringToFront();
            printPreviewDialog1.TopLevel = true;
            printPreviewDialog1.SetBounds(0, 500, 500, 500);
            printPreviewDialog1.ShowDialog();
        }
        /***********************************************************************************************/
        private void printDocument1_BeginPrint(object sender, PrintEventArgs e)
        {
            pageNumber = 1;
            checkPrint = 0;
        }
        /***********************************************************************************************/
        private void printDocument1_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            e.PageSettings.Margins.Bottom = 10;
            checkPrint = rtbPrint.RichTextBox.Print(checkPrint, rtbPrint.RichTextBox.TextLength, e);

            string page = "Page - " + pageNumber.ToString();

            int x = (e.PageBounds.Width / 2) - 30;
            Point drawPoint = new Point(x, e.PageBounds.Height - 55);
            var topFont = new Font("Times New Roman", 12);
            e.Graphics.DrawString(page, topFont, Brushes.Black, drawPoint);

            if (checkPrint < rtbPrint.RichTextBox.TextLength)
                e.HasMorePages = true;
            else
                e.HasMorePages = false;
            pageNumber++;
        }
        /***********************************************************************************************/
        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BuildPrintReport();
            using (PrintDialog pd = new PrintDialog())
            {
                pd.Document = printDocument1;
                if (pd.ShowDialog() == DialogResult.OK)
                    printDocument1.Print();
            }
        }
        /***************************************************************************************/
        public delegate void d_void_eventdone_bytes(string record, string rtfText);
        public event d_void_eventdone_bytes RtfDone;
        protected void OnDone()
        {
            if (RtfDone != null)
            {
                RtfDone.Invoke(workRecord, this.rtb.RichTextBox.Rtf);
            }
        }
        /***********************************************************************************************/
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        /***********************************************************************************************/
        private void ViewRTF_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (rtb.RichTextBox.Modified)
                OnDone();
        }
        /***********************************************************************************************/
    }
}