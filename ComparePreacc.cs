using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Base;
using System.Globalization;
using System.IO;
using DevExpress.XtraPrinting;
using DevExpress.Utils;

using GeneralLib;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class ComparePreacc : DevExpress.XtraEditors.XtraForm
    {
        private string actualFile = "";
        /***********************************************************************************************/
        public ComparePreacc()
        {
            InitializeComponent();
        }
        /***********************************************************************************************/
        private void ComparePreacc_Load(object sender, EventArgs e)
        {
            btnCompare.Hide();
            picLoader.Hide();
        }
        /***********************************************************************************************/
        private void btnSelectFile_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string file = ofd.FileName;
                    int idx = file.LastIndexOf("\\");
                    if (idx > 0)
                    {
                        actualFile = file.Substring(idx);
                        actualFile = actualFile.Replace("\\", "");
                    }
                    dgv.DataSource = null;
                    this.Cursor = Cursors.WaitCursor;
                    DataTable dt = Import.ImportCSVfile(file);
                    this.Cursor = Cursors.Default;
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        G1.NumberDataTable(dt);
                        CleanUpTable(dt);
                        dt.Columns.Add("dueDate8");
                        dt.Columns.Add("match");
                        dt.Columns.Add("customer");
                        dt.Columns.Add("payments");
                        dgv.DataSource = dt;
                        btnCompare.Show();
                    }
                }
            }
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void CleanUpTable ( DataTable dt)
        {
            string columnName = "";
            for ( int i=dt.Columns.Count-1; i>=0; i--)
            {
                columnName = dt.Columns[i].ColumnName.ToUpper();
                if (columnName == "NUM")
                    continue;
                else if (columnName == "DDUE8")
                    continue;
                else if (columnName == "CNUM")
                    continue;
                else if (columnName == "PAYMENTS")
                    continue;
                dt.Columns.RemoveAt(i);
            }
            dt.AcceptChanges();
        }
        /***********************************************************************************************/
        private void btnCompare_Click(object sender, EventArgs e)
        {
            picLoader.Show();
            string contractNumber = "";
            DataTable dt = (DataTable)dgv.DataSource;
            string cmd = "";
            DateTime date = DateTime.Now;
            DateTime oldDate = DateTime.Now;

            labelMaximum.Show();
            labelMaximum.Text = "0";
            barImport.Show();

            int lastrow = dt.Rows.Count;

            lblTotal.Show();
            barImport.Minimum = 0;
            barImport.Maximum = lastrow;
            lblTotal.Text = "of " + lastrow.ToString();
            lblTotal.Refresh();

            DataTable dx = G1.get_db_data("Select * from `contracts`;");
            DataTable dtx = G1.get_db_data("Select * from `customers`;");
            DataTable ddx = null;

            DataRow[] dRows = null;

            for ( int i=0; i<dt.Rows.Count; i++)
            {
                picLoader.Refresh();
                barImport.Value = i;
                barImport.Refresh();
                labelMaximum.Text = i.ToString();
                labelMaximum.Refresh();

                try
                {
                    contractNumber = dt.Rows[i]["cnum"].ObjToString();
                    dRows = dx.Select("contractNumber='" + contractNumber + "'");
                    if (dRows.Length > 0)
                    {
                        oldDate = dt.Rows[i]["DDUE8"].ObjToDateTime();
                        date = dRows[0]["dueDate8"].ObjToDateTime();
                        dt.Rows[i]["dueDate8"] = date.ToString("yyyyMMdd");
                        if (date != oldDate)
                            dt.Rows[i]["match"] = "MISMATCH";
                    }
                    else
                        dt.Rows[i]["match"] = "GONE";
                    dRows = dtx.Select("contractNumber='" + contractNumber + "'");
                    if (dRows.Length <= 0)
                        dt.Rows[i]["customer"] = "GONE";
                    cmd = "Select * from `payments` where `contractNumber` = '" + contractNumber + "' LIMIT 1;";
                    ddx = G1.get_db_data(cmd);
                    if (ddx.Rows.Count > 0)
                        dt.Rows[i]["payments"] = "YES";
                }
                catch ( Exception ex)
                {
                }
            }
            //CleanupRows(dt);
            G1.NumberDataTable(dt);
            barImport.Value = lastrow;
            picLoader.Hide();
        }
        /***********************************************************************************************/
        private void CleanupRows ( DataTable dt)
        {
            string data = "";
            for ( int i=dt.Rows.Count-1; i>=0; i--)
            {
                data = dt.Rows[i]["match"].ObjToString();
                if (String.IsNullOrWhiteSpace(data))
                    dt.Rows.RemoveAt(i);
            }
            dt.AcceptChanges();
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

            printingSystem1.PageSettingsChanged += PrintingSystem1_PageSettingsChanged;

            printingSystem1.Links.AddRange(new object[] {
            printableComponentLink1});


            printableComponentLink1.Component = dgv;
            printableComponentLink1.PrintingSystemBase = printingSystem1;

            printableComponentLink1.EnablePageDialog = true;

            printableComponentLink1.CreateDetailHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateDetailHeaderArea);
            printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);
            printableComponentLink1.BeforeCreateAreas += new System.EventHandler(this.printableComponentLink1_BeforeCreateAreas);
            printableComponentLink1.AfterCreateAreas += new System.EventHandler(this.printableComponentLink1_AfterCreateAreas);
            printableComponentLink1.Landscape = false;

            Printer.setupPrinterMargins(50, 100, 80, 50);

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
        private void PrintingSystem1_PageSettingsChanged(object sender, EventArgs e)
        {
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

            Printer.setupPrinterMargins(50, 100, 80, 50);

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

            font = new Font("Ariel", 10, FontStyle.Regular);
            Printer.DrawQuad(4, 8, 7, 4, "Mismatched Due Dates", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            //            Printer.DrawQuadTicks();
            DateTime date = DateTime.Now;
            string workDate = date.Month.ToString("D2") + "/" + date.Year.ToString("D4");
            Printer.SetQuadSize(24, 12);
            font = new Font("Ariel", 9, FontStyle.Bold);
            //            Printer.DrawQuad(20, 8, 5, 4, "Report Month:" + workDate, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(16, 8, 3, 4, lblPayment.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(19, 8, 3, 4, lblTrust85.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(22, 8, 3, 4, lblTrust100.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /***********************************************************************************************/
        private void btnFix_Click(object sender, EventArgs e)
        {
            string contractNumber = "";
            DataTable dt = (DataTable)dgv.DataSource;

            DateTime hisDate = DateTime.Now;
            DateTime myDate = DateTime.Now;
            DateTime paidOutDate = new DateTime(2039, 12, 31);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                contractNumber = dt.Rows[i]["cnum"].ObjToString();
                hisDate = dt.Rows[i]["DDUE8"].ObjToDateTime();
                myDate = dt.Rows[i]["dueDate8"].ObjToDateTime();
                if (myDate == paidOutDate)
                {

                    dt.Rows[i]["match"] = "OKAY";
                    continue;
                }
                else if (myDate > paidOutDate)
                {
                    dt.Rows[i]["match"] = "FIX MY BAD";
                    dt.Rows[i]["dueDate8"] = "20391231";
                    continue;
                }
                else if (hisDate > paidOutDate)
                {
                    dt.Rows[i]["match"] = "FIX HIS BAD";
                    dt.Rows[i]["dueDate8"] = "20391231";
                    continue;
                }
                else if (hisDate == paidOutDate && myDate < paidOutDate)
                {
                    dt.Rows[i]["match"] = "FIX MAX BAD";
                    dt.Rows[i]["dueDate8"] = "20391231";
                    continue;
                }
                else
                {
                    dt.Rows[i]["match"] = "FIX BAD";
                    dt.Rows[i]["dueDate8"] = hisDate.ToString("yyyyMMdd");
                }
            }

            string fix = "";
            string sDate = "";
            string record = "";
            string cmd = "";
            DataTable dx = null;
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                contractNumber = dt.Rows[i]["cnum"].ObjToString();
                fix = dt.Rows[i]["match"].ObjToString().ToUpper();
                if ( fix.IndexOf ( "FIX") >= 0)
                {
                    cmd = "Select * from `contracts` where `contractNumber` = '" + contractNumber + "';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                    {
                        record = dx.Rows[0]["record"].ObjToString();
                        myDate = dt.Rows[i]["dueDate8"].ObjToDateTime();
                        sDate = myDate.ToString("yyyy-MM-dd");
                        G1.update_db_table("contracts", "record", record, new string[] { "dueDate8", sDate });
                    }
                }
            }
            dgv.RefreshDataSource();
        }
        /***********************************************************************************************/
        private void mainGrid_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = mainGrid.GetFocusedDataRow();
            string contract = dr["cnum"].ObjToString();
            if (!String.IsNullOrWhiteSpace(contract))
            {
                string cmd = "Select * from `customers` where `contractNumber` = '" + contract + "';";
                DataTable dx = G1.get_db_data(cmd);
                if (dx.Rows.Count <= 0)
                {
                    MessageBox.Show("***ERROR*** Contract Number " + contract + " Does Not Exist");
                    return;
                }
                this.Cursor = Cursors.WaitCursor;
                DataTable dt = (DataTable)dgv.DataSource;
                CustomerDetails clientForm = new CustomerDetails(contract);
                clientForm.Show();
                this.Cursor = Cursors.Default;
            }
        }
        /***********************************************************************************************/
        private void mainGrid_CustomDrawCell(object sender, RowCellCustomDrawEventArgs e)
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
    }
}