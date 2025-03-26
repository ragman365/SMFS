using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using GeneralLib;
using DevExpress.XtraPrinting;
using EMRControlLib;
using DevExpress.XtraReports.Design.CodeCompletion;
using DevExpress.Utils;
using DevExpress.XtraRichEdit;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class ContractForm : DevExpress.XtraEditors.XtraForm
    {
        /****************************************************************************************/
        private DataTable[] allDt = new DataTable[8];
        private DataTable workDt = null;
        public ContractForm(DataTable[] dts, RichTextBoxEx rtb1, RichTextBoxEx rtb2, DataTable dt3, RichTextBoxEx rtbd, RichTextBoxEx rtbd1, RichTextBoxEx rtbF )
        {
            InitializeComponent();
            allDt = dts;
            rtbAddress.Rtf = rtb1.Rtf;
            rtbStatementOfFuneral.Document.AppendRtfText(rtb2.Rtf);
            workDt = dt3;
            //rtbFinale.Rtf = rtbF.Rtf;
            //rtbRight.AppendTextAsRtf(rtbd.Rtf);
            //rtbRight.AppendTextAsRtf(rtbd1.Rtf);

            //rtb3.Rtf = rtbd1.Rtf;
        }
        /****************************************************************************************/
        private void ContractForm_Load(object sender, EventArgs e)
        {
//            rtb.Document.Delete(rtb.Document.Range);
            LoadFile();
//            rtb.Show();
            this.BringToFront();
        }
        private void LoadFile()
        {
            string form = "casefiledocument_Extra";
            string cmd = "Select * from `arrangementforms` where `formName` = '" + form + "';";
            DataTable ddx = G1.get_db_data(cmd);
            if (ddx.Rows.Count <= 0)
                return;
            string record = ddx.Rows[0]["record"].ObjToString();
            string str = G1.get_db_blob("arrangementforms", record, "image");
            if (str.IndexOf("rtf1") > 0)
            {
                //rtb.Document.AppendRtfText(rtbAddress.Rtf);
                //rtb.Document.AppendRtfText(rtbStatementOfFuneral.Rtf);
                //rtb.Document.AppendRtfText(str);
                //rtb.Refresh();
            }
        }
        /***********************************************************************************************/
        private int pageMarginLeft = 0;
        private int pageMarginRight = 0;
        private int pageMarginTop = 0;
        private int pageMarginBottom = 0;
        /***********************************************************************************************/
        DevExpress.XtraPrinting.PrintableComponentLink printableComponentLink1 = null;
        /***********************************************************************************************/
        private void printPreviewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if ( 1 == 1 )
            {
                rtb.ShowPrintPreview();
                return;
            }
            if (this.components == null)
                this.components = new System.ComponentModel.Container();
            DevExpress.XtraPrinting.PrintingSystem printingSystem1 = new DevExpress.XtraPrinting.PrintingSystem(this.components);
            if (printableComponentLink1 != null)
            {
                printableComponentLink1.ClearDocument();
                printableComponentLink1.Dispose();
            }
            printableComponentLink1 = new DevExpress.XtraPrinting.PrintableComponentLink(this.components);

            printingSystem1.Links.AddRange(new object[] {
            printableComponentLink1});

            //printableComponentLink1.Component = x;

            printableComponentLink1.PrintingSystemBase = printingSystem1;
            //printableComponentLink1.CreateDetailHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateDetailHeaderArea);
            //printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);
            //printableComponentLink1.CreateMarginalFooterArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalFooterArea);
            //printableComponentLink1.BeforeCreateAreas += new System.EventHandler(this.printableComponentLink1_BeforeCreateAreas);
            //printableComponentLink1.AfterCreateAreas += new System.EventHandler(this.printableComponentLink1_AfterCreateAreas);
            //printableComponentLink1.CreateReportFooterArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateReportFooterArea);
            printableComponentLink1.Landscape = false;

            Printer.setupPrinterMargins(50, 50, 195, 50);

            pageMarginLeft = Printer.pageMarginLeft;
            pageMarginRight = Printer.pageMarginRight;
            pageMarginTop = Printer.pageMarginTop;
            pageMarginBottom = Printer.pageMarginBottom;

            printableComponentLink1.Margins.Left = pageMarginLeft;
            printableComponentLink1.Margins.Right = pageMarginRight;
            printableComponentLink1.Margins.Top = pageMarginTop;
            printableComponentLink1.Margins.Bottom = pageMarginBottom;

            printableComponentLink1.CreateDocument();
            printableComponentLink1.ShowPreviewDialog();
        }
        /****************************************************************************************/
        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }
        /****************************************************************************************/
    }
}