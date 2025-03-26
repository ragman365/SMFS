using System;
using System.Data;
using System.Windows.Forms;
using GeneralLib;

using System.Drawing;
using DevExpress.XtraPrinting;
using DevExpress.Utils;
using DevExpress.Pdf;
using System.IO;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class CashToTrustReport : DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        public CashToTrustReport()
        {
            InitializeComponent();
        }
        /***********************************************************************************************/
        private void CashToTrustReport_Load(object sender, EventArgs e)
        {
            MySQL.SetMaxAllowedPackets();
            picLoader.Hide();
            labelMaximum.Hide();
            barImport.Hide();
        }
        /***********************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (gridMain.OptionsFind.AlwaysVisible == true)
                gridMain.OptionsFind.AlwaysVisible = false;
            else
                gridMain.OptionsFind.AlwaysVisible = true;
        }
        /***********************************************************************************************/
        private int pageMarginLeft = 0;
        private int pageMarginRight = 0;
        private int pageMarginTop = 0;
        private int pageMarginBottom = 0;
        /***********************************************************************************************/
        private void printPreviewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.components == null)
                this.components = new System.ComponentModel.Container();

            DevExpress.XtraPrinting.PrintingSystem printingSystem1 = new DevExpress.XtraPrinting.PrintingSystem(this.components);
            DevExpress.XtraPrinting.PrintableComponentLink printableComponentLink1 = new DevExpress.XtraPrinting.PrintableComponentLink(this.components);

            printingSystem1.Links.AddRange(new object[] {
            printableComponentLink1});


            printableComponentLink1.Component = dgv;
            printableComponentLink1.PrintingSystemBase = printingSystem1;

            printableComponentLink1.EnablePageDialog = true;

            printableComponentLink1.CreateDetailHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateDetailHeaderArea);
            printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);
            printableComponentLink1.BeforeCreateAreas += new System.EventHandler(this.printableComponentLink1_BeforeCreateAreas);
            printableComponentLink1.AfterCreateAreas += new System.EventHandler(this.printableComponentLink1_AfterCreateAreas);
            printableComponentLink1.Landscape = true;

            Printer.setupPrinterMargins(50, 50, 80, 50);

            pageMarginLeft = Printer.pageMarginLeft;
            pageMarginRight = Printer.pageMarginRight;
            pageMarginTop = Printer.pageMarginTop;
            pageMarginBottom = Printer.pageMarginBottom;

            printableComponentLink1.Margins.Left = pageMarginLeft;
            printableComponentLink1.Margins.Right = pageMarginRight;
            printableComponentLink1.Margins.Top = pageMarginTop;
            printableComponentLink1.Margins.Bottom = pageMarginBottom;

            printableComponentLink1.CreateDocument();
            printableComponentLink1.ShowPreview();
        }
        /***********************************************************************************************/
        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.components == null)
                this.components = new System.ComponentModel.Container();
            DevExpress.XtraPrinting.PrintingSystem printingSystem1 = new DevExpress.XtraPrinting.PrintingSystem(this.components);
            DevExpress.XtraPrinting.PrintableComponentLink printableComponentLink1 = new DevExpress.XtraPrinting.PrintableComponentLink(this.components);

            printingSystem1.Links.AddRange(new object[] {
            printableComponentLink1});

            printableComponentLink1.Component = dgv;
            printableComponentLink1.PrintingSystemBase = printingSystem1;
            printableComponentLink1.CreateDetailHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateDetailHeaderArea);
            printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);
            printableComponentLink1.BeforeCreateAreas += new System.EventHandler(this.printableComponentLink1_BeforeCreateAreas);
            printableComponentLink1.AfterCreateAreas += new System.EventHandler(this.printableComponentLink1_AfterCreateAreas);
            printableComponentLink1.Landscape = true;

            Printer.setupPrinterMargins(50, 50, 80, 50);

            pageMarginLeft = Printer.pageMarginLeft;
            pageMarginRight = Printer.pageMarginRight;
            pageMarginTop = Printer.pageMarginTop;
            pageMarginBottom = Printer.pageMarginBottom;

            printableComponentLink1.Margins.Left = pageMarginLeft;
            printableComponentLink1.Margins.Right = pageMarginRight;
            printableComponentLink1.Margins.Top = pageMarginTop;
            printableComponentLink1.Margins.Bottom = pageMarginBottom;

            printableComponentLink1.CreateDocument();
            printableComponentLink1.PrintDlg();
        }
        /***********************************************************************************************/
        private void printableComponentLink1_BeforeCreateAreas(object sender, EventArgs e)
        {
        }
        /***********************************************************************************************/
        private void printableComponentLink1_AfterCreateAreas(object sender, EventArgs e)
        {
        }
        /***********************************************************************************************/
        private void printableComponentLink1_CreateDetailHeaderArea(object sender, CreateAreaEventArgs e)
        {
        }
        /***********************************************************************************************/
        private void printableComponentLink1_CreateMarginalHeaderArea(object sender, CreateAreaEventArgs e)
        {
            Printer.setupPrinterQuads(e, 2, 3);
            Font font = new Font("Ariel", 16);
            Printer.DrawQuad(1, 1, Printer.xQuads, 2, "South Mississippi Funeral Services, LLC", Color.Black, BorderSide.Top, font, HorizontalAlignment.Center);

            Printer.SetQuadSize(12, 12);

            font = new Font("Ariel", 8);
            Printer.DrawGridDate(2, 3, 2, 3, Color.Black, BorderSide.None, font);
            Printer.DrawGridPage(11, 3, 2, 3, Color.Black, BorderSide.None, font);

            Printer.DrawQuad(1, 9, 2, 3, "User : " + LoginForm.username, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);

            font = new Font("Ariel", 10, FontStyle.Bold);
            Printer.DrawQuad(6, 8, 4, 4, "FDLIC Import Files", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);


            //            Printer.DrawQuadTicks();
            DateTime date = DateTime.Now;
            string workDate = date.ToString("MM/dd/yyyy");
            Printer.SetQuadSize(24, 12);
            font = new Font("Ariel", 9, FontStyle.Bold);
            Printer.DrawQuad(20, 8, 5, 4, "Report Date:" + workDate, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(16, 8, 3, 4, lblPayment.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(19, 8, 3, 4, lblTrust85.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(22, 8, 3, 4, lblTrust100.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /***********************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string filename = dr["filename"].ObjToString();
            if (String.IsNullOrWhiteSpace(filename))
                return;
            filename = filename.Replace('\\', '/');
            if (!String.IsNullOrWhiteSpace(filename))
            {
                DataTable ddx = Import.ImportCSVfile(filename);
                FixFDLICShow fixForm = new FixFDLICShow(ddx);
                fixForm.ShowDialog();
            }
        }
        /***********************************************************************************************/
        private void btnImport_Click(object sender, EventArgs e)
        {
            barImport.Visible = true;
            barImport.Show();
            this.Cursor = Cursors.WaitCursor;

            DataTable dt = Import.ImportCSVfile("c:/Users/robby/downloads/05-31-2021 CASH REMIT REPORT 2021.csv");
            DataTable dx = Import.ImportCSVfile("c:/Users/robby/downloads/POST TBEG MAY 2021.csv");

            DataTable newDx = null;

            string contract1 = "";
            string contract2 = "";
            string s_payment1 = "";
            string s_payment2 = "";

            double dValue = 0D;
            double payment1 = 0D;
            double payment2 = 0D;
            double diff = 0D;

            DataTable ddx = new DataTable();
            ddx.Columns.Add("contractNumber");
            ddx.Columns.Add("Cash", Type.GetType("System.Double"));
            ddx.Columns.Add("Trust", Type.GetType("System.Double"));
            ddx.Columns.Add("Diff", Type.GetType("System.Double"));

            DataRow[] dRows = null;
            DataRow dR = null;

            int lastRow = dx.Rows.Count;

            barImport.Minimum = 0;
            barImport.Maximum = lastRow;
            labelMaximum.Show();

            for ( int i=0; i<dx.Rows.Count; i++)
            {
                Application.DoEvents();
                barImport.Value = i;
                barImport.Refresh();
                labelMaximum.Text = i.ToString() + " of " + lastRow.ToString();

                contract1 = dx.Rows[i]["contract"].ObjToString();
                if (String.IsNullOrWhiteSpace(contract1))
                    continue;

                s_payment1 = dx.Rows[i]["current payments"].ObjToString();
                if (String.IsNullOrWhiteSpace(s_payment1))
                    continue;

                payment1 = s_payment1.ObjToDouble();

                newDx = dt.Clone();
                try
                {
                    dRows = dt.Select("LastName = '" + contract1.Trim() + "'");
                }
                catch ( Exception ex)
                {
                }
                if (dRows.Length > 0)
                    newDx = dRows.CopyToDataTable();

                payment2 = 0D;
                for ( int j=0; j<newDx.Rows.Count; j++)
                {
                    s_payment2 = newDx.Rows[j]["COL 15"].ObjToString();
                    dValue = s_payment2.ObjToDouble();
                    payment2 += dValue;
                }

                diff = payment1 - payment2;
                if (diff == 0D)
                    continue;

                dR = ddx.NewRow();
                dR["contractNumber"] = contract1;
                dR["Cash"] = payment2;
                dR["Trust"] = payment1;

                diff = G1.RoundValue(diff);
                dR["Diff"] = diff;
                ddx.Rows.Add(dR);
            }

            this.Cursor = Cursors.Default;
            barImport.Value = lastRow;
            barImport.Refresh();
            labelMaximum.Text = lastRow.ToString() + " of " + lastRow.ToString();
            this.Refresh();
        }
        /***********************************************************************************************/
    }
}