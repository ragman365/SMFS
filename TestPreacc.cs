using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using DevExpress.XtraPrinting;
using DevExpress.Utils;
using GeneralLib;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using MySql.Data.MySqlClient;
using DevExpress.XtraGrid;
using DevExpress.Utils.Drawing;
using OfficeOpenXml;
using Excel = Microsoft.Office.Interop.Excel;
using System.Reflection;
using DevExpress.XtraGrid.Views.Base;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class TestPreacc : DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        public TestPreacc()
        {
            InitializeComponent();
            barImport.Hide();
            labelMaximum.Hide();
            lblTotal.Hide();
        }
        /***********************************************************************************************/
        private void btnRun_Click(object sender, EventArgs e)
        {
            string cmd = "Select * from `contracts` c JOIN `customers` x ON c.`contractNumber` = x.`contractNumber` order by `issueDate8`;";
            this.Cursor = Cursors.WaitCursor;
            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("status");
            dt.Columns.Add("pa");
            dt.Columns.Add("pl");
            dt.Columns.Add("pd");
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            this.Cursor = Cursors.Default;
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
            printableComponentLink1.CreateDetailHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateDetailHeaderArea);
            printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);
            printableComponentLink1.BeforeCreateAreas += new System.EventHandler(this.printableComponentLink1_BeforeCreateAreas);
            printableComponentLink1.AfterCreateAreas += new System.EventHandler(this.printableComponentLink1_AfterCreateAreas);
            printableComponentLink1.Landscape = false;

            Printer.setupPrinterMargins(50, 50, 150, 50);

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
            printableComponentLink1.Landscape = false;

            Printer.setupPrinterMargins(50, 50, 150, 50);

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
            //Printer.DrawQuadBorder(1, 1, 12, 6, BorderSide.All, 1, Color.Black);
            //Printer.DrawQuadBorder(12, 1, 1, 6, BorderSide.Right, 1, Color.Black);

            font = new Font("Ariel", 8);
            Printer.DrawGridDate(2, 1, 2, 3, Color.Black, BorderSide.None, font);
            Printer.DrawGridPage(11, 1, 2, 3, Color.Black, BorderSide.None, font);

            Printer.DrawQuad(1, 3, 2, 2, "User : " + LoginForm.username, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);

            font = new Font("Ariel", 10, FontStyle.Bold);
            Printer.DrawQuad(6, 3, 2, 3, "Contracts Report", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            font = new Font("Ariel", 7, FontStyle.Regular);
            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /***********************************************************************************************/
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        /***********************************************************************************************/
        private void btnFindFile_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string file = ofd.FileName;
                    this.txtFromTable.Text = file;
                }
            }
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void btnCompareFile_Click(object sender, EventArgs e)
        {
            string file = this.txtFromTable.Text;
            if (String.IsNullOrWhiteSpace(file))
                return;
            if (!File.Exists(file))
                return;
            DataTable dt = (DataTable)dgv.DataSource;
            this.Cursor = Cursors.WaitCursor;
            DataTable dx = Import.ImportCSVfile(file);
            dx.Columns.Add("ADD");
            string contractNumber = "";
            bool doLapse = false;
            bool doDeaths = false;
            if (file.ToUpper().IndexOf("PREACCLAP") > 0)
                doLapse = true;
            else if (file.ToUpper().IndexOf("DEATHS") > 0)
                doDeaths = true;
            DataRow[] dR = null;
            DateTime deceasedDate = DateTime.Now;
            int lastrow = dx.Rows.Count;

            barImport.Show();
            lblTotal.Show();

            lblTotal.Text = "of " + lastrow.ToString();
            lblTotal.Refresh();

            barImport.Minimum = 0;
            barImport.Maximum = lastrow;
            labelMaximum.Show();

            for (int i = 0; i < lastrow; i++)
            {
                barImport.Value = i;
                barImport.Refresh();
                labelMaximum.Text = i.ToString();
                labelMaximum.Refresh();

                contractNumber = dx.Rows[i]["cnum"].ObjToString();
                if (String.IsNullOrWhiteSpace(contractNumber))
                    continue;
                dR = dt.Select("contractNumber='" + contractNumber + "'");
                if ( doDeaths )
                {
                    if (dR.Length > 0)
                    {
                        dR[0]["pd"] = "Y";
                        deceasedDate = dR[0]["deceasedDate"].ObjToDateTime();
                        if ( deceasedDate.Year > 100 )
                            dR[0]["status"] = "MATCH";
                        else
                            dR[0]["status"] = "MISSED";
                    }
                    else
                        dx.Rows[i]["ADD"] = "ADD";
                }
                else if (doLapse)
                {
                    if (dR.Length > 0)
                    {
                        dR[0]["pl"] = "Y";
                        if (dR[0]["lapsed"].ObjToString() == "Y")
                            dR[0]["status"] = "MATCH";
                        else
                            dR[0]["status"] = "MISSED";
                    }
                    else
                        dx.Rows[i]["ADD"] = "ADD";
                }
                else
                {
                    if (dR.Length > 0)
                        dR[0]["status"] = "MATCH";
                    else
                        dx.Rows[i]["ADD"] = "ADD";
                }
            }
            DataRow dRow = null;
            dR = dx.Select("ADD='ADD'");
            for ( int i=0; i<dR.Length; i++)
            {
                contractNumber = dR[i]["cnum"].ObjToString();
                dRow = dt.NewRow();
                dRow["contractNumber"] = contractNumber;
                dRow["status"] = "ADD";
                if (doLapse)
                    dRow["pl"] = "Y";
                else if (doDeaths)
                    dRow["pd"] = "Y";
                else
                    dRow["pa"] = "Y";
                dt.Rows.Add(dRow);
            }
            barImport.Value = lastrow;
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void gridMain_CustomColumnDisplayText(object sender, CustomColumnDisplayTextEventArgs e)
        {
            ColumnView view = sender as ColumnView;
            if (e.Column.FieldName.ToUpper().IndexOf("DATE") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                if (e.DisplayText.IndexOf("0000") >= 0 || e.DisplayText.IndexOf("0001") >= 0)
                    e.DisplayText = "";
                else
                {
                    DateTime date = e.DisplayText.ObjToString().ObjToDateTime();
                    if ( date.Year > 100)
                        e.DisplayText = date.ToString("MM/dd/yyyy");
                    else
                        e.DisplayText = "";
                }
            }
        }
        /***********************************************************************************************/
        private void gridMain_CustomDrawCell(object sender, RowCellCustomDrawEventArgs e)
        {
            if (e.Column.FieldName.ToUpper() == "NUM")
            {
                if (e.RowHandle >= 0)
                {
                    string num = (e.RowHandle + 1).ToString();
                    e.DisplayText = num;
                }
            }
        }
        /***********************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            string contract = "";
            DataRow dr = gridMain.GetFocusedDataRow();
            contract = dr["contractNumber"].ObjToString();
            this.Cursor = Cursors.WaitCursor;
            CustomerDetails clientForm = new CustomerDetails(contract);
            clientForm.Show();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
    }
}