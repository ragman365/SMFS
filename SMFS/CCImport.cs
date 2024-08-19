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
using DevExpress.XtraGrid.Views.Base;
using DevExpress.Xpo.Helpers;
using System.IO;
using ExcelLibrary.BinaryFileFormat;
using System.Security.Cryptography;
using DevExpress.XtraPrinting;
using DevExpress.Utils;
using DevExpress.Office.Utils;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using Excel = Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;

//using java.awt;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class CCImport : DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        private bool saveChangedData = false;
        private int CCT = 0;
        private int CCF = 0;
        private int CCI = 0;
        private int CCD = 0;
        /***********************************************************************************************/
        public CCImport()
        {
            InitializeComponent();
        }
        /***********************************************************************************************/
        private void CCImport_Load(object sender, EventArgs e)
        {
            picAdd.Hide();

            btnSaveNotPosted.Hide();
            btnSaveChangedData.Hide();

            btnPost.Hide();
            btnPost.Refresh();

            lblEffectiveDate.Hide();
            lblEffectiveDate.Refresh();

            dateTimePicker1.Hide();
            dateTimePicker1.Refresh();

            SetupTotalsSummary();

            GetBankAccounts();
        }
        /****************************************************************************************/
        private void SetupTotalsSummary()
        {
            AddSummaryColumn("authorizedAmount", null);
            AddSummaryColumn("transactionAmount", null);
            AddSummaryColumn("amount", null);
            AddSummaryColumn("salesTax", null);
            AddSummaryColumn("fee", null);
            AddSummaryColumn("surCharge", null);

            AddSummaryColumn("authorizedAmount", gridMain2 );
            AddSummaryColumn("transactionAmount", gridMain2 );
            AddSummaryColumn("amount", gridMain2);
            AddSummaryColumn("salesTax", gridMain2 );
            AddSummaryColumn("fee", gridMain2 );
            AddSummaryColumn("surCharge", gridMain2);

            AddSummaryColumn("authorizedAmount", gridMain3);
            AddSummaryColumn("transactionAmount", gridMain3);
            AddSummaryColumn("amount", gridMain3);
            AddSummaryColumn("salesTax", gridMain3);
            AddSummaryColumn("fee", gridMain3 );
            AddSummaryColumn("surCharge", gridMain3 );
        }
        /****************************************************************************************/
        private void AddSummaryColumn(string columnName, DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gMain = null, string format = "")
        {
            if (gMain == null)
                gMain = gridMain;
            //if (String.IsNullOrWhiteSpace(format))
            //    format = "${0:0,0.00}";
            if (String.IsNullOrWhiteSpace(format))
                format = "{0:0,0.00}";
            gMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            gMain.Columns[columnName].SummaryItem.DisplayFormat = format;
        }
        /****************************************************************************************/
        private void AddSummaryColumn(string columnName, string format = "")
        {
            //if (String.IsNullOrWhiteSpace(format))
            //    format = "${0:0,0.00}";
            if (String.IsNullOrWhiteSpace(format))
                format = "{0:0,0.00}";
            gridMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            gridMain.Columns[columnName].SummaryItem.DisplayFormat = format;
        }
        /***********************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (dgv.Visible)
                SetSpyGlass(gridMain);
            else if (dgv2.Visible)
                SetSpyGlass(gridMain2);
        }
        /***********************************************************************************************/
        private void SetSpyGlass(DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView grid)
        {
            if (grid.OptionsFind.AlwaysVisible == true)
                grid.OptionsFind.AlwaysVisible = false;
            else
                grid.OptionsFind.AlwaysVisible = true;
        }
        /***********************************************************************************************/
        private void gridMain_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            ColumnView view = sender as ColumnView;
            if (e.Column.FieldName.ToUpper().IndexOf("DATE") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                if (e.DisplayText.IndexOf("0000") >= 0 || e.DisplayText.IndexOf("0001") >= 0)
                    e.DisplayText = "";
                else
                {
                    DateTime date = e.DisplayText.ObjToString().ObjToDateTime();
                    e.DisplayText = date.ToString("MM/dd/yyyy");
                    if (date.Year < 30)
                        e.DisplayText = "";
                }
            }
            //else if (e.Column.FieldName.ToUpper().IndexOf("CODE") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            //{
            //    int row = e.ListSourceRowIndex;
            //    DataTable dt = (DataTable)dgv.DataSource;
            //    string payer = dt.Rows[row]["payer"].ObjToString();
            //    string contractNumber = dt.Rows[row]["contractNumber"].ObjToString();
            //    if (!String.IsNullOrWhiteSpace(payer))
            //        e.DisplayText = "02";
            //    else if (contractNumber.ToUpper().Contains("ZZ"))
            //        e.DisplayText = "02";
            //}
        }
        /***********************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string contract = dr["invoiceNumber"].ObjToString();
            string tf = dr["trustFuneral"].ObjToString();

            LookupSomeone(contract, tf);

        }
        /***********************************************************************************************/
        private void pictureDelete_Click(object sender, EventArgs e)
        {
            if (dgv.Visible)
                return;

            if (dgv2.Visible)
            {
                DataRow dr = gridMain2.GetFocusedDataRow();
                string record = dr["record"].ObjToString();
                string contractNumber = dr["invoiceNumber"].ObjToString();
                DialogResult result = MessageBox.Show("***Question*** Are you sure you want to DELETE this Bank CC (" + contractNumber + ") ?", "Delete Bank CC Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.No)
                    return;

                DataTable dt = (DataTable)dgv2.DataSource;
                int rowHandle = gridMain2.FocusedRowHandle;
                int row = gridMain2.GetDataSourceRowIndex(rowHandle);
                dt.Rows.RemoveAt(row);
                gridMain2.ClearSelection();
                G1.delete_db_table("bank_cc", "record", record);
                string who = contractNumber;
                G1.AddToAudit(LoginForm.username, "Bank CC", "CC", "CC Payment Removed for " + who, contractNumber);
            }
            else if ( dgv3.Visible )
            {
                DataRow dr = gridMain3.GetFocusedDataRow();
                string record = dr["record"].ObjToString();
                string contractNumber = dr["invoiceNumber"].ObjToString();
                DialogResult result = MessageBox.Show("***Question*** Are you sure you want to DELETE this Bank CC (" + contractNumber + ") ?", "Delete Bank CC Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.No)
                    return;

                DataTable dt = (DataTable)dgv3.DataSource;
                int rowHandle = gridMain3.FocusedRowHandle;
                int row = gridMain3.GetDataSourceRowIndex(rowHandle);
                dt.Rows.RemoveAt(row);
                gridMain3.ClearSelection();
                G1.delete_db_table("bank_cc", "record", record);
                string who = contractNumber;
                G1.AddToAudit(LoginForm.username, "Bank CC", "CC", "CC Payment Removed for " + who, contractNumber);
            }
            else if (dgv4.Visible)
            {
                DataRow dr = gridMain4.GetFocusedDataRow();
                string record = dr["record"].ObjToString();
                string contractNumber = dr["invoiceNumber"].ObjToString();
                DialogResult result = MessageBox.Show("***Question*** Are you sure you want to DELETE this Bank CC (" + contractNumber + ") ?", "Delete Bank CC Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.No)
                    return;

                DataTable dt = (DataTable)dgv4.DataSource;
                int rowHandle = gridMain4.FocusedRowHandle;
                int row = gridMain4.GetDataSourceRowIndex(rowHandle);
                dt.Rows.RemoveAt(row);
                gridMain4.ClearSelection();
                G1.delete_db_table("bank_cc", "record", record);
                string who = contractNumber;
                G1.AddToAudit(LoginForm.username, "Bank CC", "CC", "CC Payment Removed for " + who, contractNumber);
            }
            else if (dgv5.Visible)
            {
                DataRow dr = gridMain5.GetFocusedDataRow();
                string record = dr["record"].ObjToString();
                string contractNumber = dr["invoiceNumber"].ObjToString();
                DialogResult result = MessageBox.Show("***Question*** Are you sure you want to DELETE this Bank CC (" + contractNumber + ") ?", "Delete Bank CC Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.No)
                    return;

                DataTable dt = (DataTable)dgv5.DataSource;
                int rowHandle = gridMain5.FocusedRowHandle;
                int row = gridMain5.GetDataSourceRowIndex(rowHandle);
                dt.Rows.RemoveAt(row);
                gridMain5.ClearSelection();
                G1.delete_db_table("bank_cc", "record", record);
                string who = contractNumber;
                G1.AddToAudit(LoginForm.username, "Bank CC", "CC", "CC Payment Removed for " + who, contractNumber);
            }
        }
        /***********************************************************************************************/
        private int pageMarginLeft = 0;
        private int pageMarginRight = 0;
        private int pageMarginTop = 0;
        private int pageMarginBottom = 0;
        private bool isPrinting = false;
        /***********************************************************************************************/
        private void printPreviewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            isPrinting = true;
            if (this.components == null)
                this.components = new System.ComponentModel.Container();

            DevExpress.XtraPrinting.PrintingSystem printingSystem1 = new DevExpress.XtraPrinting.PrintingSystem(this.components);
            DevExpress.XtraPrinting.PrintableComponentLink printableComponentLink1 = new DevExpress.XtraPrinting.PrintableComponentLink(this.components);

            printingSystem1.Links.AddRange(new object[] {
            printableComponentLink1});


            printableComponentLink1.Component = dgv;
            if (dgv2.Visible)
                printableComponentLink1.Component = dgv2;
            else if (dgv3.Visible)
                printableComponentLink1.Component = dgv3;
            else if (dgv4.Visible)
                printableComponentLink1.Component = dgv4;

            printableComponentLink1.PrintingSystemBase = printingSystem1;

            printableComponentLink1.EnablePageDialog = true;

            printableComponentLink1.CreateDetailHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateDetailHeaderArea);
            printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);
            printableComponentLink1.BeforeCreateAreas += new System.EventHandler(this.printableComponentLink1_BeforeCreateAreas);
            printableComponentLink1.AfterCreateAreas += new System.EventHandler(this.printableComponentLink1_AfterCreateAreas);
            printableComponentLink1.Landscape = true;

            Printer.setupPrinterMargins(50, 100, 80, 50);

            pageMarginLeft = Printer.pageMarginLeft;
            pageMarginRight = Printer.pageMarginRight;
            pageMarginTop = Printer.pageMarginTop;
            pageMarginBottom = Printer.pageMarginBottom;

            printableComponentLink1.Margins.Left = pageMarginLeft;
            printableComponentLink1.Margins.Right = pageMarginRight;
            printableComponentLink1.Margins.Top = pageMarginTop;
            printableComponentLink1.Margins.Bottom = pageMarginBottom;

            printingSystem1.Document.AutoFitToPagesWidth = 1;

            printableComponentLink1.CreateDocument();
            printableComponentLink1.ShowPreview();
            isPrinting = false;
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

            font = new Font("Ariel", 12);
            string text = "Credit Card Import Data";
            if ( dgv.Visible )
            {
                Printer.DrawQuad(6, 7, 4, 4, text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);
            }
            if (dgv2.Visible)
            {
                text = "Credit Card Data Not Posted";
                Printer.DrawQuad(6, 7, 4, 4, text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);
            }
            else if (dgv3.Visible)
            {
                text = "Credit Card Posted Data";
                Printer.DrawQuad(6, 7, 4, 4, text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);
            }
            else if (dgv4.Visible)
            {
                text = "Credit Cards Declined Report";
                Printer.DrawQuad(6, 7, 4, 4, text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);
            }

            font = new Font("Ariel", 10, FontStyle.Bold);

            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /****************************************************************************************/
        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            isPrinting = true;
            if (this.components == null)
                this.components = new System.ComponentModel.Container();

            DevExpress.XtraPrinting.PrintingSystem printingSystem1 = new DevExpress.XtraPrinting.PrintingSystem(this.components);
            DevExpress.XtraPrinting.PrintableComponentLink printableComponentLink1 = new DevExpress.XtraPrinting.PrintableComponentLink(this.components);

            printingSystem1.Links.AddRange(new object[] {
            printableComponentLink1});


            printableComponentLink1.Component = dgv;
            if (dgv2.Visible)
                printableComponentLink1.Component = dgv2;
            else if (dgv3.Visible)
                printableComponentLink1.Component = dgv3;
            else if (dgv4.Visible)
                printableComponentLink1.Component = dgv4;

            printableComponentLink1.PrintingSystemBase = printingSystem1;

            printableComponentLink1.EnablePageDialog = true;

            printableComponentLink1.CreateDetailHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateDetailHeaderArea);
            printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);
            printableComponentLink1.BeforeCreateAreas += new System.EventHandler(this.printableComponentLink1_BeforeCreateAreas);
            printableComponentLink1.AfterCreateAreas += new System.EventHandler(this.printableComponentLink1_AfterCreateAreas);
            printableComponentLink1.Landscape = true;

            Printer.setupPrinterMargins(50, 100, 80, 50);

            pageMarginLeft = Printer.pageMarginLeft;
            pageMarginRight = Printer.pageMarginRight;
            pageMarginTop = Printer.pageMarginTop;
            pageMarginBottom = Printer.pageMarginBottom;

            printableComponentLink1.Margins.Left = pageMarginLeft;
            printableComponentLink1.Margins.Right = pageMarginRight;
            printableComponentLink1.Margins.Top = pageMarginTop;
            printableComponentLink1.Margins.Bottom = pageMarginBottom;

            printingSystem1.Document.AutoFitToPagesWidth = 1;

            printableComponentLink1.CreateDocument();
            printableComponentLink1.PrintDlg();
            isPrinting = false;
        }
        /***********************************************************************************************/
        private void picAdd_Click(object sender, EventArgs e)
        {
            //DataRow dr = gridMain.GetFocusedDataRow();
            //DataTable dt = (DataTable)dgv.DataSource;
            //if ( dt == null)
            //{
            //    string cmd = "Select * from `ach` where `contractNumber` = 'ABCDXXX';";
            //    dt = G1.get_db_data(cmd);
            //    dt.Columns.Add("effectiveDate");
            //    dt.Columns.Add("name");
            //    dt.Columns.Add("ID");
            //    dt.Columns.Add("DebitCredit");
            //    dt.Columns.Add("status");
            //    dt.Columns.Add("backupName");
            //}

            //DateTime effectiveDate = this.dateTimePicker1.Value;
            //using (ACHExtraPayment extraForm = new ACHExtraPayment(dt, effectiveDate))
            //{
            //    DialogResult result = extraForm.ShowDialog();
            //    if (result != DialogResult.OK)
            //        return;
            //    DataTable dx = (DataTable)extraForm.ACH_Answer;
            //    if (dx != null)
            //    {
            //        int row = 0;
            //        for (int i = 0; i < dx.Rows.Count; i++)
            //        {
            //            dt.ImportRow(dx.Rows[i]);
            //            row = dt.Rows.Count - 1;
            //            dt.Rows[row]["backupName"] = dt.Rows[row]["name"].ObjToString();
            //        }
            //        G1.NumberDataTable(dt);
            //        dgv.DataSource = dt;
            //        dgv.Refresh();
            //    }
            //}
        }
        /***********************************************************************************************/
        private void gridMain_CustomSummaryCalculate(object sender, DevExpress.Data.CustomSummaryEventArgs e)
        {
            string field = (e.Item as GridSummaryItem).FieldName.ObjToString();
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
        public static DataTable getCSVFile ( string filename )
        {
            DataTable dt = Import.ImportCSVfile(filename);
            if ( dt != null )
            {
                if ( dt.Rows.Count > 0 )
                {
                    if (G1.get_column_number(dt, "num") >= 0)
                        dt.Columns.Remove("num");
                }
            }
            return dt;
        }
        /***********************************************************************************************/
        public static DataTable getExcelFile( string filename )
        {
            DataTable dt = new DataTable();

            try
            {
                //Create COM Objects. Create a COM object for everything that is referenced
                Excel.Application xlApp = new Excel.Application();
                Excel.Workbook xlWorkbook = xlApp.Workbooks.Open(filename);
                Excel._Worksheet xlWorksheet = xlWorkbook.Sheets[1];
                Excel.Range xlRange = xlWorksheet.UsedRange;

                int rowCount = xlRange.Rows.Count;
                int colCount = xlRange.Columns.Count;

                //iterate over the rows and columns and print to the console as it appears in the file
                //excel is not zero based!!

                for (int i = 1; i <= colCount; i++)
                {
                    dt.Columns.Add("COL " + i.ToString());
                }
                for (int i = 1; i <= rowCount; i++)
                {
                    for (int j = 1; j <= colCount; j++)
                    {
                        //new line
                        if (j == 1)
                        {
                            DataRow dr = dt.NewRow();
                            dt.Rows.Add(dr);
                            //Console.Write("\r\n");
                        }

                        //write the value to the console
                        if (xlRange.Cells[i, j] != null && xlRange.Cells[i, j].Value2 != null)
                        {
                            dt.Rows[i - 1][j - 1] = xlRange.Cells[i, j].Value2.ToString();
                            //Console.Write(xlRange.Cells[i, j].Value2.ToString() + "\t");
                        }
                    }
                }

                //cleanup
                GC.Collect();
                GC.WaitForPendingFinalizers();

                //rule of thumb for releasing com objects:
                //  never use two dots, all COM objects must be referenced and released individually
                //  ex: [somthing].[something].[something] is bad

                //release com objects to fully kill excel process from running in the background
                Marshal.ReleaseComObject(xlRange);
                Marshal.ReleaseComObject(xlWorksheet);

                //close and release
                xlWorkbook.Close();
                Marshal.ReleaseComObject(xlWorkbook);

                //quit and release
                xlApp.Quit();
                Marshal.ReleaseComObject(xlApp);
            }
            finally
            {
            }

            return dt;
        }

        /***********************************************************************************************/
        private void btnPullFile_Click(object sender, EventArgs e)
        {
            DataTable dt = null;
            string str = "";
            this.Cursor = Cursors.WaitCursor;
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string filename = ofd.FileName;

                    this.Cursor = Cursors.WaitCursor;

                    int idx = filename.LastIndexOf("\\");
                    if (idx > 0)
                    {
                        actualFile = filename.Substring(idx);
                        actualFile = actualFile.Replace("\\", "");
                    }

                    FileInfo file = new FileInfo(filename);
                    string extension = file.Extension.Trim().ToUpper();

                    if ( extension == ".XLS" || extension == ".XLSX" )
                        dt = getExcelFile(filename);
                    else if ( extension == ".CSV")
                        dt = getCSVFile(filename);

                    for ( int i=0; i<dt.Rows.Count; i++)
                    {
                        str = dt.Rows[i][0].ObjToString();
                        if ( str.ToUpper() == "TRANSACTION DATE")
                        {
                            for ( int j=0; j<dt.Columns.Count; j++)
                            {
                                str = dt.Rows[i][j].ObjToString();
                                str = str.Replace(" ", "");
                                dt.Rows[i][j] = str;
                                dt.Columns[j].ColumnName = str;
                            }
                        }
                    }

                    DataTable newDt = buildActualImportTable(dt);
                    if (newDt != null)
                        dgv.DataSource = newDt;

                    this.Cursor = Cursors.Default;
                }
            }
            this.Cursor = Cursors.Default;
            btnSaveNotPosted.Show();
        }
        /***********************************************************************************************/
        private DataTable buildActualImportTable ( DataTable dx )
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("referenceNumber");
            dt.Columns.Add("transactionDate");
            dt.Columns.Add("transactionType");
            dt.Columns.Add("cardNumber");
            dt.Columns.Add("paymentType");
            dt.Columns.Add("authorizedAmount", Type.GetType("System.Double"));
            dt.Columns.Add("transactionAmount", Type.GetType("System.Double"));
            dt.Columns.Add("returnVoid");
            dt.Columns.Add("amount", Type.GetType("System.Double"));
            dt.Columns.Add("salesTax", Type.GetType("System.Double"));
            dt.Columns.Add("fee", Type.GetType("System.Double"));
            dt.Columns.Add("surCharge", Type.GetType("System.Double"));
            dt.Columns.Add("trustFuneral");
            dt.Columns.Add("invoiceNumber");

            DateTime date = DateTime.Now;
            string cardNumber = "";
            string transactionType = "";
            string transationDate = "";
            string invoiceNumber = "";
            string referenceNumber = "";
            string str = "";
            double dValue = 0D;


            for ( int i=0; i<dx.Rows.Count; i++)
            {
                try
                {
                    date = dx.Rows[i]["transactionDate"].ObjToDateTime();
                    if (date.Year < 1000)
                        continue;

                    cardNumber = dx.Rows[i]["cardNumber"].ObjToString();
                    cardNumber = cardNumber.Replace("*", "");
                    dx.Rows[i]["cardNumber"] = cardNumber;

                    transactionType = dx.Rows[i]["transactionType"].ObjToString();
                    transactionType = transactionType.Replace("Credit Card Sale", "");
                    transactionType = transactionType.Replace("Credit Card Verify", "");
                    transactionType = transactionType.Trim();

                    if (transactionType.ToUpper().IndexOf("VOID") >= 0)
                        transactionType = "Void";
                    dx.Rows[i]["transactionType"] = transactionType;

                    DataRow dRow = dt.NewRow();
                    dRow["cardNumber"] = cardNumber;
                    dRow["transactionType"] = transactionType;
                    dRow["transactionDate"] = date.ToString("yyyy-MM-dd");
                    dRow["paymentType"] = dx.Rows[i]["paymentType"].ObjToString().Trim();
                    str = dx.Rows[i]["authorizedAmount"].ObjToString();
                    dRow["authorizedAmount"] = cleanupMoney(str);
                    dRow["transactionAmount"] = cleanupMoney(dx.Rows[i]["transactionAmount"].ObjToString());
                    dRow["returnVoid"] = dx.Rows[i]["return/Void"].ObjToString().Trim();
                    dRow["amount"] = cleanupMoney(dx.Rows[i]["amount"].ObjToString());
                    dRow["salesTax"] = cleanupMoney(dx.Rows[i]["authorizedAmount"].ObjToString());
                    dRow["fee"] = cleanupMoney(dx.Rows[i]["salesTax"].ObjToString());
                    dRow["surCharge"] = cleanupMoney(dx.Rows[i]["surCharge"].ObjToString());
                    invoiceNumber = dx.Rows[i]["invoiceNumber"].ObjToString().Trim();
                    dRow["invoiceNumber"] = invoiceNumber;
                    dRow["trustFuneral"] = categorizeInvoiceNumber( ref invoiceNumber);
                    dRow["invoiceNumber"] = invoiceNumber;

                    referenceNumber = dx.Rows[i]["referenceNumber"].ObjToString().Trim();
                    if (!String.IsNullOrWhiteSpace(referenceNumber))
                    {
                        dValue = Double.Parse(referenceNumber, System.Globalization.NumberStyles.Float);
                        dRow["referenceNumber"] = dValue.ToString();
                    }
                    dt.Rows.Add(dRow);

                }
                catch ( Exception ex )
                {
                }


            }
            return dt;
        }
        /***********************************************************************************************/
        private string actualFile = "";
        /***********************************************************************************************/
        private void btnPullFile_ClickX(object sender, EventArgs e)
        {
            DataTable ddt = null;
            this.Cursor = Cursors.WaitCursor;
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string filename = ofd.FileName;

                    ddt = getExcelFile(filename);
                    int idx = filename.LastIndexOf("\\");
                    if (idx > 0)
                    {
                        actualFile = filename.Substring(idx);
                        actualFile = actualFile.Replace("\\", "");
                    }
                    dgv.DataSource = null;
                    this.Cursor = Cursors.WaitCursor;
                    DataTable dt = new DataTable();
                    dt.Columns.Add("referenceNumber");
                    dt.Columns.Add("transactionDate");
                    dt.Columns.Add("transactionType");
                    dt.Columns.Add("cardNumber");
                    dt.Columns.Add("paymentType");
                    dt.Columns.Add("authorizedAmount", Type.GetType("System.Double"));
                    dt.Columns.Add("transactionAmount", Type.GetType("System.Double"));
                    dt.Columns.Add("returnVoid");
                    dt.Columns.Add("amount", Type.GetType("System.Double"));
                    dt.Columns.Add("salesTax", Type.GetType("System.Double"));
                    dt.Columns.Add("fee", Type.GetType("System.Double"));
                    dt.Columns.Add("surCharge", Type.GetType("System.Double"));
                    dt.Columns.Add("trustFuneral");
                    dt.Columns.Add("invoiceNumber");
                    try
                    {
                        if (!File.Exists(filename))
                        {
                            DevExpress.XtraEditors.XtraMessageBox.Show("***ERROR*** File does not exist!");
                            return;
                        }
                        try
                        {
                            bool first = true;
                            string line = "";
                            int row = 0;
                            string delimiter = ",";
                            char cDelimiter = (char)delimiter[0];
                            string transactionType = "";
                            string cardNumber = "";
                            string invoiceNumber = "";

                            double dValue = 0D;
                            string referenceNumber = "";


                            FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                            using (StreamReader sr = new StreamReader(fs))
                            {
                                while ((line = sr.ReadLine()) != null)
                                {
                                    Application.DoEvents();
                                    if (line.ToUpper().IndexOf("REPORT NAME") == 0)
                                        continue;
                                    if (line.IndexOf("$") > 0)
                                        line = preprocessLine(line);
                                    string[] Lines = line.Split(cDelimiter);
                                    G1.parse_answer_data(line, delimiter);
                                    int count = G1.of_ans_count;
                                    cardNumber = Lines[4].ObjToString();
                                    cardNumber = cardNumber.Replace("*", "");
                                    if (cardNumber == "Card Number")
                                        continue;
                                    if (string.IsNullOrWhiteSpace(cardNumber))
                                        continue;

                                    transactionType = Lines[3].ObjToString().Trim();
                                    transactionType = transactionType.Replace("Credit Card Sale", "");
                                    transactionType = transactionType.Trim();

                                    if (transactionType.ToUpper().IndexOf("VOID") >= 0)
                                        transactionType = "Void";

                                    DataRow dRow = dt.NewRow();
                                    dRow["cardNumber"] = cardNumber;
                                    dRow["transactionType"] = transactionType;
                                    dRow["transactionDate"] = Lines[0];
                                    dRow["paymentType"] = Lines[5].ObjToString().Trim();
                                    dRow["authorizedAmount"] = cleanupMoney(Lines[6].ObjToString());
                                    dRow["transactionAmount"] = cleanupMoney(Lines[7].ObjToString());
                                    dRow["returnVoid"] = Lines[8].ObjToString().Trim();
                                    dRow["amount"] = cleanupMoney(Lines[9].ObjToString());
                                    dRow["salesTax"] = cleanupMoney(Lines[10].ObjToString());
                                    dRow["fee"] = cleanupMoney(Lines[11].ObjToString());
                                    dRow["surCharge"] = cleanupMoney(Lines[12].ObjToString());
                                    invoiceNumber = Lines[14].ObjToString().Trim();
                                    dRow["invoiceNumber"] = invoiceNumber;
                                    dRow["trustFuneral"] = categorizeInvoiceNumber( ref invoiceNumber);
                                    dRow["invoiceNumber"] = invoiceNumber;

                                    referenceNumber = Lines[15].ObjToString().Trim();
                                    dValue = Double.Parse(referenceNumber, System.Globalization.NumberStyles.Float);
                                    dRow["referenceNumber"] = dValue.ToString();
                                    dt.Rows.Add(dRow);
                                }
                                row++;
                                sr.Close();
                                dgv.DataSource = dt;
                            }
                        }
                        catch (Exception ex)
                        {
                            DevExpress.XtraEditors.XtraMessageBox.Show(ex.Message, "Error Occurred");
                        }
                        G1.NumberDataTable(dt);
                    }
                    catch (Exception ex)
                    {
                    }
                    this.Cursor = Cursors.Default;
                    if (dt != null && dt.Rows.Count > 0)
                    {
                    }
                }
            }
            this.Cursor = Cursors.Default;
            btnSaveNotPosted.Show();
        }
        /***********************************************************************************************/
        private string categorizeInvoiceNumber ( ref string invoiceNumber )
        {
            string rtn = "Trust";
            string[] Lines = invoiceNumber.Split(' ');
            if (Lines.Length <= 0)
                return rtn;
            string what = Lines[0].Trim();
            what = what.Replace("-", "");
            string cmd = "Select * from `contracts` where `contractNumber` = '" + what + "';";
            DataTable dx = G1.get_db_data(cmd);
            if ( dx.Rows.Count <= 0 )
            {
                cmd = "Select * from `fcust_extended` where `serviceId` = '" + what + "';";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                    rtn = "Funeral";
                else
                {
                    rtn = "N/A";
                    Lines = invoiceNumber.Split(' ');
                    if (Lines.Length <= 0)
                        return rtn;
                    what = Lines[0].Trim();
                    cmd = "Select * from `icustomers` where `payer` = '" + what + "';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                        rtn = "Insurance";
                    else
                    {
                        rtn = "N/A";
                        if (invoiceNumber.IndexOf("Down Pmt") >= 0)
                            rtn = "DownPmt";

                        invoiceNumber = invoiceNumber.Replace("Trst Down Pmt", "").Trim();
                        invoiceNumber = invoiceNumber.Replace("Trust Down Pmt", "").Trim();
                        invoiceNumber = invoiceNumber.Replace("/", " ");

                        Lines = invoiceNumber.Split(' ');
                        if ( Lines.Length >= 2 )
                        {
                            string fname = Lines[0];
                            string lname = Lines[1];
                            if (!String.IsNullOrWhiteSpace(fname) && !String.IsNullOrWhiteSpace(lname))
                            {
                                cmd = "Select * from `downpayments` where `lastName` = '" + lname + "' AND `firstName` = '" + fname + "' LIMIT 10;";
                                dx = G1.get_db_data(cmd);
                                if (dx.Rows.Count > 0)
                                    rtn = "DownPmt";
                            }
                        }
                    }
                }
            }
            return rtn;
        }
        /***********************************************************************************************/
        private double cleanupMoney ( string str)
        {
            str = str.Trim();
            str = str.Replace("\"", "");
            str = str.Replace("$", "");
            double dValue = str.ObjToDouble();
            return dValue;
        }
        /***********************************************************************************************/
        private string preprocessLine ( string line )
        {
            string newStr = "";
            string str = "";
            bool started = false;
            for ( int i=0; i<line.Length; i++)
            {
                str = line.Substring(i, 1);
                if (started)
                {
                    if (str == ",")
                        continue;
                    if (str == ".")
                        started = false;
                    newStr += str;
                    continue;
                }
                if (str == "$")
                {
                    started = true;
                    continue;
                }
                else
                    newStr += str;
            }
            return newStr;
        }
        /***********************************************************************************************/
        private void btnSaveNotPosted_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            string record = "";
            DateTime date = DateTime.Now;
            string transactionDate = "";
            string referenceNumber = "";
            string transactionType = "";
            string cardNumber = "";
            string paymentType = "";
            double authorizedAmount = 0D;
            double transactionAmount = 0D;
            string returnVoid = "";
            double amount = 0D;
            double salesTax = 0D;
            double fee = 0D;
            double surCharge = 0D;
            string invoiceNumber = "";
            string trustFuneral = "";

            string cmd = "Delete from `bank_cc` WHERE `paymentType` = '-1';";
            G1.get_db_data(cmd);

            DataTable dx = null;
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                try
                {
                    referenceNumber = dt.Rows[i]["referenceNumber"].ObjToString().Trim();
                    cmd = "Select * from `bank_cc` WHERE `referenceNumber` = '" + referenceNumber + "';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                    {
                        MessageBox.Show("*** ERROR *** Reference Number Already Imported : " + referenceNumber + "!!!", "Bank CC Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                        continue;
                    }
                    date = dt.Rows[i]["transactionDate"].ObjToDateTime();
                    transactionDate = date.ToString("MM/dd/yyyy");
                    transactionType = dt.Rows[i]["transactionType"].ObjToString();
                    cardNumber = dt.Rows[i]["cardNumber"].ObjToString();
                    paymentType = dt.Rows[i]["paymentType"].ObjToString();
                    authorizedAmount = dt.Rows[i]["authorizedAmount"].ObjToDouble();
                    transactionAmount = dt.Rows[i]["transactionAmount"].ObjToDouble();
                    returnVoid = dt.Rows[i]["returnVoid"].ObjToString();
                    amount = dt.Rows[i]["amount"].ObjToDouble();
                    salesTax = dt.Rows[i]["salesTax"].ObjToDouble();
                    fee = dt.Rows[i]["fee"].ObjToDouble();
                    surCharge = dt.Rows[i]["surCharge"].ObjToDouble();
                    trustFuneral = dt.Rows[i]["trustFuneral"].ObjToString();
                    invoiceNumber = dt.Rows[i]["invoiceNumber"].ObjToString();

                    record = G1.create_record("bank_cc", "paymentType", "-1");
                    if (G1.BadRecord("bank_cc", record))
                        continue;
                    G1.update_db_table("bank_cc", "record", record, new string[] {"referenceNumber", referenceNumber, "cardNumber", cardNumber, "transactionType", transactionType, "transactionDate", transactionDate,
                        "paymentType", paymentType, "authorizedAmount", authorizedAmount.ToString(), "transactionAmount", transactionAmount.ToString(), "returnVoid", returnVoid, "amount", amount.ToString(), "salesTax", salesTax.ToString(), "fee", fee.ToString(), "surCharge", surCharge.ToString(), "trustFuneral", trustFuneral, "invoiceNumber", invoiceNumber });
                }
                catch ( Exception ex )
                {
                    MessageBox.Show("*** ERROR *** Importing Reference Number : " + referenceNumber + "!!!", "CC Import Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                }
            }
            btnSaveNotPosted.Hide();
            btnSaveNotPosted.Refresh();
        }
        /***********************************************************************************************/
        private void btnPullNotPosted_Click(object sender, EventArgs e)
        {
            string cmd = "Select * from `bank_cc` WHERE `posted` = '' AND `transactionType` <> 'Decline' AND `transactionType` <> 'Void';";
            DataTable dt = G1.get_db_data(cmd);

            dt.Columns.Add("select");
            SetupSelection(dt);

            dt.Columns.Add("mod");

            G1.NumberDataTable(dt);
            dgv2.DataSource = dt;

            lblEffectiveDate.Show();
            lblEffectiveDate.Refresh();

            dateTimePicker1.Show();
            dateTimePicker1.Refresh();
        }
        /***********************************************************************************************/
        private void SetupSelection(DataTable dt)
        {
            DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew = this.repositoryItemCheckEdit1;
            selectnew.NullText = "";
            selectnew.ValueChecked = "1";
            selectnew.ValueUnchecked = "0";
            selectnew.ValueGrayed = "";
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["select"] = "0";
        }
        /***********************************************************************************************/
        private void repositoryItemCheckEdit1_CheckedChanged(object sender, EventArgs e)
        {
            btnPost.Show();
            btnPost.Refresh();

            lblEffectiveDate.Show();
            lblEffectiveDate.Refresh();

            dateTimePicker1.Show();
            dateTimePicker1.Refresh();

            DevExpress.XtraEditors.CheckEdit check = (DevExpress.XtraEditors.CheckEdit)sender;
            bool isChecked = true;
            if (!check.Checked)
                isChecked = false;

            DataTable dt = (DataTable)dgv2.DataSource;

            DataRow dr = gridMain2.GetFocusedDataRow();
            string what = dr["invoiceNumber"].ObjToString().ToUpper();
            string select = dr["select"].ObjToString();

            string trustFuneral = dr["trustFuneral"].ObjToString().ToUpper();
            decimal lossRecovery = 0;

            if (check.Checked)
            {
                if (trustFuneral == "DOWNPMT")
                    lossRecovery = DownPayments.GetLossRecoveryFee();

                dr["select"] = "1";
                string [] Lines = what.Split(' ');
                if (Lines.Length <= 0)
                    return;
                string account = Lines[0].Trim();
                //string trustFuneral = what;
                double amount = dr["amount"].ObjToDouble();

                DataTable feeDt = G1.LoadCCFeeTable();

                double fee = G1.GetCCFee(feeDt, account, "");
                if (fee > 0D)
                {
                    fee += 1D;
                    double ccFee = ((amount - (double) lossRecovery) ) / fee;
                    ccFee = G1.RoundValue(ccFee);
                    ccFee = (amount - (double) lossRecovery) - ccFee;
                    ccFee = G1.RoundValue(ccFee);
                    dr["ccFee"] = ccFee;
                }

            }
            else
            {
                dr["select"] = "0";
                dr["ccFee"] = 0D;
            }

            numberDeposits();
            dgv2.RefreshDataSource();
            dgv2.Refresh();
        }
        /***********************************************************************************************/
        private string trustBankCC = "";
        private string funeralBankCC = "";
        private void GetBankAccounts ()
        {
            string description = "";
            string location = "";
            string bank_gl = "";
            string bankAccount = "";
            string cc_account = "";

            trustBankCC = "";
            funeralBankCC = "";
            string cmd = "Select * from `bank_accounts` WHERE `localDescription` LIKE 'Credit Card -%';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                for (int i = 0; i < dx.Rows.Count; i++)
                {
                    description = dx.Rows[i]["localDescription"].ObjToString().ToUpper();
                    location = dx.Rows[i]["location"].ObjToString();
                    bank_gl = dx.Rows[i]["general_ledger_no"].ObjToString();
                    bankAccount = dx.Rows[i]["account_no"].ObjToString();
                    cc_account = location + "~" + bank_gl + "~" + bankAccount;
                    if (description.IndexOf("TRUST") > 0)
                        trustBankCC = cc_account;
                    else if (description.IndexOf("FUNERAL") > 0)
                        funeralBankCC = cc_account;
                }
            }
        }
        /***********************************************************************************************/
        private void numberDeposits ()
        {
            GetDepositStart();

            int cct = CCT;
            int ccf = CCF;
            int cci = CCI;
            int ccd = CCD;
            string select = "";
            string what = "";
            string depositNumber = "";
            string bankAccount  = "";
            string invoiceNumber = "";
            string record = "";
            string[] Lines = null;
            string cmd = "";
            double amount = 0D;
            DataTable dx = null;
            string account = "";

            DateTime date = this.dateTimePicker1.Value;
            string sDate = date.ToString("yyyy-MM-dd");

            DataTable dt = (DataTable)dgv2.DataSource;

            if (G1.get_column_number(dt, "trustDpRecord") < 0)
                dt.Columns.Add("trustDpRecord");

            for ( int i=0; i<dt.Rows.Count; i++)
            {
                account = "";
                depositNumber = "";
                bankAccount = "";

                amount = dt.Rows[i]["amount"].ObjToDouble();
                what = dt.Rows[i]["invoiceNumber"].ObjToString();
                Lines = what.Split(' ');
                if (Lines.Length > 0 )
                    account = Lines[0].Trim();
                what = dt.Rows[i]["trustFuneral"].ObjToString().ToUpper();
                select = dt.Rows[i]["select"].ObjToString();
                if ( select == "1" )
                {
                    if ( what == "TRUST")
                    {
                        cct++;
                        dt.Rows[i]["bankAccount"] = trustBankCC;
                        bankAccount = trustBankCC;
                        depositNumber = "CCT" + cct.ToString("D4");
                    }
                    else if (what == "INSURANCE")
                    {
                        cci++;
                        dt.Rows[i]["bankAccount"] = trustBankCC;
                        bankAccount = trustBankCC;
                        depositNumber = "CCI" + cci.ToString("D4");
                    }
                    else if (what == "FUNERAL") // RAMMA ZAMMA
                    {
                        bankAccount = "N/A";
                        cmd = "Select * from `fcust_extended` where `serviceId` = '" + account + "';";
                        dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count > 0)
                        {
                            account = dx.Rows[0]["contractNumber"].ObjToString();
                            cmd = "Select * from `cust_payments` where `contractNumber` = '" + account + "' AND `type` = 'Credit Card';";
                            dx = G1.get_db_data(cmd);
                            if (dx.Rows.Count > 0)
                            {
                                dt.Rows[i]["bankAccount"] = funeralBankCC;
                                bankAccount = funeralBankCC;
                                ccf++;
                                depositNumber = "CCF" + ccf.ToString("D4");

                                cmd = "Select * from `cust_payment_details` where `contractNumber` = '" + account + "' AND `type` = 'Credit Card';";
                                dx = G1.get_db_data(cmd);
                                if (dx.Rows.Count > 0)
                                {
                                    //dt.Rows[i]["bankAccount"] = funeralBankCC;
                                    //bankAccount = funeralBankCC;
                                    //ccf++;
                                    //depositNumber = "CCF" + ccf.ToString("D4");
                                }
                            }
                        }
                    }
                    else if(what == "DOWNPMT")
                    {
                        bankAccount = "N/A";
                        invoiceNumber = dt.Rows[i]["invoiceNumber"].ObjToString();
                        Lines = invoiceNumber.Split(' ');
                        if (Lines.Length >= 2)
                        {
                            string fname = Lines[0];
                            string lname = Lines[1];
                            if (!String.IsNullOrWhiteSpace(fname) && !String.IsNullOrWhiteSpace(lname))
                            {
                                cmd = "Select * from `downpayments` where `lastName` = '" + lname + "' AND `firstName` = '" + fname + "' LIMIT 10;";
                                dx = G1.get_db_data(cmd);
                                if (dx.Rows.Count > 0)
                                {
                                    amount = dt.Rows[i]["amount"].ObjToDouble();
                                    cmd = "Select * from `downpayments` where `lastName` = '" + lname + "' AND `firstName` = '" + fname + "' AND `downPayment` = '" + amount.ToString() + "' AND `date` = '" + sDate + "';";
                                    dx = G1.get_db_data(cmd);
                                    if (dx.Rows.Count > 0)
                                    {
                                        record = dx.Rows[0]["record"].ObjToString();
                                        dt.Rows[i]["trustDpRecord"] = record;
                                        bankAccount = dx.Rows[0]["bankAccount"].ObjToString();
                                        depositNumber = dx.Rows[0]["depositNumber"].ObjToString();
                                        if ( String.IsNullOrWhiteSpace ( depositNumber ))
                                        {
                                            ccd++;
                                            depositNumber = "CCD" + ccd.ToString("D4");
                                        }
                                    }
                                    else
                                    {
                                        bankAccount = "CREATE";
                                        ccd++;
                                        depositNumber = "CCD" + ccd.ToString("D4");
                                    }
                                }
                                else
                                {
                                    bankAccount = "CREATE";
                                    ccd++;
                                    depositNumber = "CCD" + ccd.ToString("D4");
                                }
                            }
                        }
                    }
                }
                dt.Rows[i]["depositNumber"] = depositNumber;
                dt.Rows[i]["bankAccount"] = bankAccount;
            }

            CCT = cct;
            CCF = ccf;
            CCI = cci;
            CCD = ccd;
        }
        /***************************************************************************************/
        private bool chargeCCFee (string workContract, string payer)
        {
            string lookup = workContract;
            if (!String.IsNullOrWhiteSpace(payer))
                lookup = payer;
            string cmd = "Select * from `creditcards` WHERE `contractNumber` = '" + lookup + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return true;
            return false;
        }
        /***********************************************************************************************/
        private void btnPost_Click(object sender, EventArgs e)
        {
            DateTime Importdate = dateTimePicker1.Value;
            DialogResult result = MessageBox.Show("Are you sure you want to assign " + Importdate.ToString("MM/dd/yyyy") + " as the IMPORT DATE for the Selected Rows?", "CC Import", MessageBoxButtons.YesNo);
            if (result == DialogResult.No)
                return;
            DataTable dt = (DataTable)dgv2.DataSource;

            string select = "";
            string record = "";
            string depositNumber = "";
            string bankAccount = "";
            string what = "";
            string[] Lines = null;
            string code = "";
            string location = "CC";
            string payment = "";
            string expected = "";
            string date = this.dateTimePicker1.Value.ToString("yyyyMMdd");
            string line = "";
            string account = "";
            string trustFuneral = "";
            double amount = 0D;
            double ccFee = 0D;
            double fee = 0D;

            //DataTable feeDt = G1.LoadCCFeeTable();

            DataTable dx = null;
            DataRow[] dRows = dt.Select("select='1'");
            if ( dRows.Length > 0 )
            {
                dx = dRows.CopyToDataTable();
                dx.Columns.Add("cnum");
                dx.Columns.Add("line");
                for ( int i=0; i<dx.Rows.Count; i++)
                {
                    what = dx.Rows[i]["invoiceNumber"].ObjToString();
                    Lines = what.Split(' ');
                    if (Lines.Length <= 0)
                        return;
                    account = Lines[0].Trim();
                    dx.Rows[i]["cnum"] = account;
                    trustFuneral = dx.Rows[i]["trustFuneral"].ObjToString().ToUpper();
                    code = "";
                    if (trustFuneral == "TRUST")
                        code = "01";
                    else if (trustFuneral == "INSURANCE")
                        code = "02";
                    else if (trustFuneral == "FUNERAL")
                        code = "03";
                    else if (trustFuneral == "DOWNPMT")
                    {
                        dx.Rows[i]["cnum"] = what;
                        code = "04";
                    }
                    else
                        continue;
                    location = "CC";

                    expected = "0.00";
                    amount = dx.Rows[i]["amount"].ObjToDouble();
                    payment = G1.ReformatMoney(amount);

                    line = BuildNewLine(code, account, location, payment, expected, date);
                    dx.Rows[i]["line"] = line;

                    //fee = G1.GetCCFee(feeDt, account, "");
                    //if (fee > 0D)
                    //{
                    //    fee += 1D;
                    //    ccFee = amount / fee;
                    //    ccFee = G1.RoundValue(ccFee);
                    //    ccFee = amount - ccFee;
                    //    ccFee = G1.RoundValue(ccFee);
                    //    dx.Rows[i]["fee"] = ccFee;
                    //}
                }
                ImportDailyDeposits importForm = new ImportDailyDeposits(dx, "BankCC");
                importForm.ImportDone += ImportForm_ImportDone;
                importForm.ShowDialog();
            }
        }
        /***********************************************************************************************/
        private string decodeBankAccont ( string bankAccount)
        {
            string account = "";
            string [] Lines = bankAccount.Split('~');
            if (Lines.Length > 2)
            {
                string localDescription = Lines[0];
                account = Lines[2];
            }
            return account;
        }
        /***********************************************************************************************/
        private void ImportForm_ImportDone(string s)
        {
            if (s != "SUCCESS")
                return;

            DataTable dt = (DataTable)dgv2.DataSource;

            string select = "";
            string record = "";
            string depositNumber = "";
            string bankAccount = "";
            double ccFee = 0D;
            string trustDpRecord = "";
            DateTime importDate = dateTimePicker1.Value; // RAMMA ZAMMA

            string what = "";
            string account = "";
            string cmd = "";
            double amount = 0D;
            decimal lossRecoveryFee = DownPayments.GetLossRecoveryFee();
            double totalDeposit = 0D;

            string firstName = "";
            string lastName = "";
            string location = "";
            string localDescription = "";


            DataTable dx = null;
            DataTable funDt = null;
            DataRow[] dRows = null;
            string[] Lines = null;


            for (int i = 0; i < dt.Rows.Count; i++)
            {
                depositNumber = dt.Rows[i]["depositNumber"].ObjToString();
                bankAccount = dt.Rows[i]["bankAccount"].ObjToString();
                amount = dt.Rows[i]["amount"].ObjToDouble();
                ccFee = dt.Rows[i]["ccFee"].ObjToDouble();

                firstName = "";
                lastName = "";
                account = "";

                what = dt.Rows[i]["invoiceNumber"].ObjToString();
                Lines = what.Split(' ');
                if (Lines.Length > 0)
                {
                    account = Lines[0].Trim();
                    if ( Lines.Length >= 2 )
                    {
                        firstName = Lines[0];
                        lastName = Lines[1];
                    }
                }


                what = dt.Rows[i]["trustFuneral"].ObjToString().ToUpper();
                select = dt.Rows[i]["select"].ObjToString();
                if (select == "1")
                {
                    record = dt.Rows[i]["record"].ObjToString();
                    if (bankAccount.ToUpper() == "CREATE" && what == "DOWNPMT")
                        bankAccount = decodeBankAccont(trustBankCC);

                    G1.update_db_table("bank_cc", "record", record, new string[] { "posted", "Y", "postedDate", importDate.ToString("yyyy-MM-dd"), "depositNumber", depositNumber, "bankAccount", bankAccount, "ccFee", ccFee.ToString() });

                    if (what == "DOWNPMT")
                    {
                        trustDpRecord = dt.Rows[i]["trustDpRecord"].ObjToString();
                        if (!String.IsNullOrWhiteSpace(trustDpRecord))
                            G1.update_db_table("downpayments", "record", trustDpRecord, new string[] { "depositNumber", depositNumber });
                        else
                        {
                            trustDpRecord = G1.create_record("downpayments", "firstName", "-1");
                            if (G1.BadRecord("downpayments", trustDpRecord))
                                continue;
                            totalDeposit = amount;

                            bankAccount = "";
                            localDescription = "";
                            Lines = trustBankCC.Split('~');
                            if ( Lines.Length > 2 )
                            {
                                localDescription = Lines[0];
                                bankAccount = Lines[2];
                            }

                            dt.Rows[i]["record"] = trustDpRecord;
                            G1.update_db_table("downpayments", "record", trustDpRecord, new string[] { "depositNumber", depositNumber, "date", importDate.ToString("yyyy-MM-dd"), "downPayment", amount.ToString(), "lossRecoveryFee", lossRecoveryFee.ToString(), "payment", "0.00", "totalDeposit", totalDeposit.ToString(), "firstName", firstName, "lastName", lastName, "location", location, "paymentType", "Credit Card", "bankAccount", bankAccount, "localDescription", localDescription, "ccFee", ccFee.ToString(), "user", LoginForm.username });
                        }
                    }
                    else if ( what == "FUNERAL")
                    {
                        if ( !String.IsNullOrWhiteSpace ( account ))
                        {
                            cmd = "Select * from `fcust_extended` where `serviceId` = '" + account + "';";
                            funDt = G1.get_db_data(cmd);
                            if (funDt.Rows.Count > 0)
                            {
                                account = funDt.Rows[0]["contractNumber"].ObjToString();
                                cmd = "Select * from `cust_payments` where `contractNumber` = '" + account + "' AND `type` = 'Credit Card' AND `payment` = '" + amount.ToString() + "' AND `status` <> 'Deposited';";
                                dx = G1.get_db_data(cmd);
                                if (dx.Rows.Count > 0)
                                {
                                    UpdateFuneralCC ( account, dx, depositNumber, amount, importDate, funDt ) ;
                                    //cmd = "Select * from `cust_payment_details` where `contractNumber` = '" + account + "' AND `type` = 'Credit Card';";
                                    //dx = G1.get_db_data(cmd);
                                    if (dx.Rows.Count > 0)
                                    {
                                        //dt.Rows[i]["bankAccount"] = funeralBankCC;
                                        //bankAccount = funeralBankCC;
                                        //ccf++;
                                        //depositNumber = "CCF" + ccf.ToString("D4");
                                    }
                                }
                            }
                        }
                    }
                }
            }

            dRows = dt.Select("select<>'1'");
            if (dRows.Length > 0)
            {
                dx = dRows.CopyToDataTable();
                dt = dx.Copy();
            }
            else
                dt.Rows.Clear();

            updateDepositOption("CCT", CCT);
            updateDepositOption("CCF", CCF);
            updateDepositOption("CCI", CCI);
            updateDepositOption("CCD", CCD);

            dgv2.DataSource = dt;
        }
        /***********************************************************************************************/
        private void UpdateFuneralCC ( string contractNumber, DataTable dt, string depositNumber, double amount, DateTime postedDate, DataTable funDt )
        {
            string custExtendedRecord = funDt.Rows[0]["record"].ObjToString();
            string record = "";

            string cmd = "";
            DataTable dx = null;
            DataRow[] dRows = null;
            bool found = false;
            string newRecord = "";
            string bank = "";
            string localDescription = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                record = dt.Rows[i]["record"].ObjToString();
                cmd = "Select * from `cust_payment_details` where `contractNumber` = '" + contractNumber + "' AND `type` = 'Credit Card' AND `paymentRecord` = '" + record + "' AND `status` <> 'Received';";
                dx = G1.get_db_data(cmd);
                if ( dx.Rows.Count > 0 )
                {
                    newRecord = dx.Rows[i]["record"].ObjToString();
                    bank = dx.Rows[i]["bankAccount"].ObjToString();
                    localDescription = dx.Rows[i]["localDescription"].ObjToString();
                    G1.update_db_table("cust_payment_details", "record", newRecord, new string[] { "contractNumber", contractNumber, "paymentRecord", record, "depositNumber", depositNumber, "paid", amount.ToString(), "dateReceived", postedDate.ToString("MM/dd/yyyy"), "status", "Deposited", "type", "Credit Card", "comment", "", "bankAccount", bank, "localDescription", localDescription, "dateModified", DateTime.Now.ToString("MM/dd/yyyy"), "lastUser", LoginForm.username });
                    G1.update_db_table("cust_payments", "record", record, new string[] { "depositNumber", depositNumber, "status", "Deposited", "dateModified", DateTime.Now.ToString("MM/dd/yyyy"), "amountReceived", amount.ToString() });
                    found = true;
                    break;
                }
            }
            if ( !found )
            {
                dRows = dt.Select("status<>'Deposited'");
                if (dRows.Length > 0)
                {
                    record = dRows[0]["record"].ObjToString();
                    bank = dRows[0]["bankAccount"].ObjToString();
                    localDescription = dRows[0]["localDescription"].ObjToString();
                    newRecord = G1.create_record("cust_payment_details", "comment", "-1");
                    G1.update_db_table("cust_payment_details", "record", newRecord, new string[] { "contractNumber", contractNumber, "paymentRecord", record, "depositNumber", depositNumber, "paid", amount.ToString(), "dateReceived", postedDate.ToString("MM/dd/yyyy"), "status", "Deposited", "type", "Credit Card", "comment", "", "bankAccount", bank, "localDescription", localDescription, "dateModified", DateTime.Now.ToString("MM/dd/yyyy"), "lastUser", LoginForm.username });
                    G1.update_db_table("cust_payments", "record", record, new string[] { "depositNumber", depositNumber, "status", "Deposited", "dateModified", DateTime.Now.ToString("MM/dd/yyyy"), "amountReceived", amount.ToString() });
                    found = true;
                }
            }
            if (found)
            {
                if (funDt.Rows.Count > 0)
                {
                    DataRow dR = funDt.Rows[0];
                    Funerals.CalculateCustomerDetails(contractNumber, custExtendedRecord, dR );
                }
            }
        }
        /***********************************************************************************************/
        private string BuildNewLine(string code, string contract, string location, string payment, string expected, string date)
        {
            //010900E19053LI0015619001561920201228
            //010700L19052LI0011856001185620201228
            //01160WF19163LI0007470000747020201228
            //011300N19010LI0010525001052520201228
            //02140000UC-3840000316000360020201228
            //02080000CC-1690000510000612020201228
            //01CC0HT20032LI0000000000810820220624

            //string code = txtCode.Text.Trim(); // 0 for 2
            //string location = txtLocation.Text.Trim(); // 2 for 2

            //string contract = txtContract.Text.Trim(); //4 for 10

            //string expected = txtExpected.Text; // 14 for 7
            expected = expected.Replace("$", "");
            expected = expected.Replace(".", "");
            expected = expected.Replace(",", "");

            //string payment = txtPayment.Text; // 21 for 7
            payment = payment.Replace("$", "");
            payment = payment.Replace(".", "");
            payment = payment.Replace(",", "");

            //string date = txtDate.Text; // 28 for 8

            string newline = code + location;

            if (contract.Length < 10)
                contract = "0000000000".Substring(contract.Length) + contract;
            newline += contract;

            if (expected.Length < 7)
                expected = "0000000".Substring(expected.Length) + expected;
            newline += expected;

            if (payment.Length < 7)
                payment = "0000000".Substring(payment.Length) + payment;
            newline += payment;

            newline += date;

            return newline;
        }
        /***********************************************************************************************/
        private void updateDepositOption ( string what, int value )
        {
            string cmd = "Select * from `options` where `option` = '" + what + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return;

            string record = dt.Rows[0]["record"].ObjToString();
            G1.update_db_table("options", "record", record, new string[] { "answer", value.ToString() }) ;
            return;
        }
        /***********************************************************************************************/
        private void btnPullPostedData_Click(object sender, EventArgs e)
        {
            string cmd = "Select * from `bank_cc` WHERE `posted` = 'Y' AND `transactionType` <> 'Decline' AND `transactionType` <> 'Void';";
            if ( chkHonorDates.Checked )
            {
                DateTime date = this.dateTimePicker2.Value;
                string date1 = date.ToString("yyyy-MM-dd 00:00:00");

                date = this.dateTimePicker3.Value;
                string date2 = date.ToString("yyyy-MM-dd 23:59:59");
                cmd = "Select * from `bank_cc` WHERE `posted` = 'Y' AND `postedDate` >= '" + date1 + "' AND `postedDate` <= '" + date2 + "' AND `transactionType` <> 'Decline' AND `transactionType` <> 'Void';";
            }
            DataTable dt = G1.get_db_data(cmd);

            G1.NumberDataTable(dt);
            dgv3.DataSource = dt;
        }
        /***********************************************************************************************/
        private void duplicatePaymentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string contract = dr["invoiceNumber"].ObjToString();
            if (String.IsNullOrWhiteSpace(contract))
                return;
            DataTable dt = (DataTable)dgv.DataSource;
            int rowhandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowhandle);
            try
            {
                dr["duplicate"] = "Y";
                dt.Rows[row]["duplicate"] = "Y";
                DataTable tempDt = dt.Clone();
                G1.copy_dt_row(dt, row, tempDt, 0);

                DataRow dR = dt.NewRow();
                dt.Rows.InsertAt(dR, row);
                G1.copy_dt_row(tempDt, 0, dt, row);
            }
            catch (Exception ex)
            {

            }
            dt.AcceptChanges();
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            dgv.RefreshDataSource();
            dgv.Refresh();
        }
        /***********************************************************************************************/
        private void gridMain2_CellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            DataRow dr = gridMain2.GetFocusedDataRow();

            if (e.Column.FieldName.ToUpper() != "SELECT")
            {
                dr["mod"] = "Y";

                saveChangedData = true;

                btnSaveChangedData.Show();
                btnSaveChangedData.Refresh();
            }
        }
        /***********************************************************************************************/
        private void btnSaveChangedData_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv2.DataSource;
            string record = "";
            string referenceNumber = "";
            string transactionType = "";
            string cardNumber = "";
            string paymentType = "";
            double authorizedAmount = 0D;
            double transactionAmount = 0D;
            string returnVoid = "";
            double amount = 0D;
            double salesTax = 0D;
            double fee = 0D;
            double surCharge = 0D;
            string invoiceNumber = "";
            string trustFuneral = "";
            string depositNumber = "";
            string mod = "";

            string cmd = "Delete from `bank_cc` WHERE `paymentType` = '-1';";
            G1.get_db_data(cmd);
            DataTable dx = null;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    mod = dt.Rows[i]["mod"].ObjToString();
                    if (mod != "Y")
                        continue;

                    referenceNumber = dt.Rows[i]["referenceNumber"].ObjToString().Trim();
                    transactionType = dt.Rows[i]["transactionType"].ObjToString();
                    cardNumber = dt.Rows[i]["cardNumber"].ObjToString();
                    paymentType = dt.Rows[i]["paymentType"].ObjToString();
                    authorizedAmount = dt.Rows[i]["authorizedAmount"].ObjToDouble();
                    transactionAmount = dt.Rows[i]["transactionAmount"].ObjToDouble();
                    returnVoid = dt.Rows[i]["returnVoid"].ObjToString();
                    amount = dt.Rows[i]["amount"].ObjToDouble();
                    salesTax = dt.Rows[i]["salesTax"].ObjToDouble();
                    fee = dt.Rows[i]["fee"].ObjToDouble();
                    surCharge = dt.Rows[i]["surCharge"].ObjToDouble();
                    trustFuneral = dt.Rows[i]["trustFuneral"].ObjToString();
                    invoiceNumber = dt.Rows[i]["invoiceNumber"].ObjToString();
                    depositNumber = dt.Rows[i]["depositNumber"].ObjToString();

                    record = dt.Rows[i]["record"].ObjToString();
                    if ( record == "-1")
                        record = G1.create_record("bank_cc", "paymentType", "-1");

                    if (G1.BadRecord("bank_cc", record))
                        continue;
                    G1.update_db_table("bank_cc", "record", record, new string[] {"referenceNumber", referenceNumber, "cardNumber", cardNumber, "transactionType", transactionType,
                        "paymentType", paymentType, "authorizedAmount", authorizedAmount.ToString(), "transactionAmount", transactionAmount.ToString(), "returnVoid", returnVoid, "amount", amount.ToString(), "salesTax", salesTax.ToString(), "fee", fee.ToString(), "surCharge", surCharge.ToString(), "trustFuneral", trustFuneral, "invoiceNumber", invoiceNumber });

                    dt.Rows[i]["mod"] = "";
                    dt.Rows[i]["record"] = record.ObjToInt32();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("*** ERROR *** Importing Reference Number : " + referenceNumber + "!!!", "CC Import Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                }
            }
            btnSaveChangedData.Hide();
            btnSaveChangedData.Refresh();
        }
        /***********************************************************************************************/
        private void duplicatePaymentToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain2.GetFocusedDataRow();
            string contract = dr["invoiceNumber"].ObjToString();
            if (String.IsNullOrWhiteSpace(contract))
                return;
            DataTable dt = (DataTable)dgv2.DataSource;
            int rowhandle = gridMain2.FocusedRowHandle;
            int row = gridMain2.GetDataSourceRowIndex(rowhandle);
            try
            {
                dr["duplicate"] = "Y";
                dt.Rows[row]["duplicate"] = "Y";
                dr["mod"] = "Y";
                dt.Rows[row]["mod"] = "Y";
                string referenceNumber = dr["referenceNumber"].ObjToString();
                string newReference = referenceNumber + "-A";
                dr["referenceNumber"] = newReference;
                dt.Rows[row]["referenceNumber"] = newReference;
                DataTable tempDt = dt.Clone();
                G1.copy_dt_row(dt, row, tempDt, 0);

                DataRow dR = dt.NewRow();
                dt.Rows.InsertAt(dR, row);
                G1.copy_dt_row(tempDt, 0, dt, row);

                string record = "-1";
                dt.Rows[row]["record"] = record.ObjToInt32();
                newReference = referenceNumber + "-B";
                dt.Rows[row]["referenceNumber"] = newReference;

                numberDeposits();

                btnSaveChangedData.Visible = true;
                btnSaveChangedData.Refresh();

                saveChangedData = true;
            }
            catch (Exception ex)
            {

            }
            dt.AcceptChanges();
            G1.NumberDataTable(dt);
            dgv2.DataSource = dt;
            dgv2.RefreshDataSource();
            dgv2.Refresh();
        }
        /****************************************************************************************/
        private void GetDepositStart ()
        {
            DataTable ddd = G1.get_db_data("Select * from `options`;");
            if (ddd.Rows.Count <= 0)
                return;
            CCT = 0;
            CCF = 0;
            CCI = 0;
            CCD = 0;
            DataRow[] dR = ddd.Select("option='CCT'");
            if (dR.Length > 0)
            {
                string str = dR[0]["answer"].ObjToString().Trim().ToUpper();
                if (str.Length > 0)
                    CCT = Convert.ToInt32(str);
            }

            dR = ddd.Select("option='CCF'");
            if (dR.Length > 0)
            {
                string str = dR[0]["answer"].ObjToString().Trim().ToUpper();
                if (str.Length > 0)
                    CCF = Convert.ToInt32(str);
            }

            dR = ddd.Select("option='CCI'");
            if (dR.Length > 0)
            {
                string str = dR[0]["answer"].ObjToString().Trim().ToUpper();
                if (str.Length > 0)
                    CCI = Convert.ToInt32(str);
            }

            dR = ddd.Select("option='CCD'");
            if (dR.Length > 0)
            {
                string str = dR[0]["answer"].ObjToString().Trim().ToUpper();
                if (str.Length > 0)
                    CCD = Convert.ToInt32(str);
            }
        }
        /***********************************************************************************************/
        private void gridMain3_DoubleClick(object sender, EventArgs e) // RAMM
        {
            DataRow dr = gridMain3.GetFocusedDataRow();
            string contract = dr["invoiceNumber"].ObjToString();
            string tf = dr["trustFuneral"].ObjToString();

            LookupSomeone(contract, tf);
        }
        /***********************************************************************************************/
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        /***********************************************************************************************/
        private void btnPullDeclined_Click(object sender, EventArgs e)
        {
            string cmd = "Select * from `bank_cc` WHERE `transactionType` = 'Decline';";
            if (chkHonorDates3.Checked)
            {
                DateTime date = this.dateTimePicker4.Value;
                string date1 = date.ToString("yyyy-MM-dd 00:00:00");

                date = this.dateTimePicker5.Value;
                string date2 = date.ToString("yyyy-MM-dd 23:59:59");
                cmd = "Select * from `bank_cc` WHERE transactionDate` >= '" + date1 + "' AND `transactionDate` <= '" + date2 + "' AND `transactionType` =`Decline';";
            }

            DataTable dt = G1.get_db_data(cmd);

            G1.NumberDataTable(dt);
            dgv4.DataSource = dt;
        }
        /***********************************************************************************************/
        private void gridMain2_ValidatingEditor(object sender, DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventArgs e)
        { // Not Posted Editor
            GridView view = sender as GridView;
            string column = view.FocusedColumn.FieldName.ToUpper();

            DataTable dt = (DataTable)dgv2.DataSource;
            DataRow dr = gridMain2.GetFocusedDataRow();
            int rowhandle = gridMain2.FocusedRowHandle;
            int row = gridMain2.GetDataSourceRowIndex(rowhandle);
            string str  = dr["duplicate"].ObjToString();
            bool duplicate = false;
            if (str.ToUpper() == "Y")
                duplicate = true;
        }
        /***********************************************************************************************/
        private void gridMain3_ValidatingEditor(object sender, DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventArgs e)
        { // Posted Editor
            GridView view = sender as GridView;
            string column = view.FocusedColumn.FieldName.ToUpper();

            DataTable dt = (DataTable)dgv3.DataSource;
            DataRow dr = gridMain3.GetFocusedDataRow();
            int rowhandle = gridMain3.FocusedRowHandle;
            int row = gridMain3.GetDataSourceRowIndex(rowhandle);
            string str = dr["duplicate"].ObjToString();
            bool duplicate = false;
            if (str.ToUpper() == "Y")
                duplicate = true;
        }
        /***********************************************************************************************/
        private void gridMain2_ShowingEditor(object sender, CancelEventArgs e)
        { // Not Posted Editor
            GridView view = sender as GridView;
            string column = view.FocusedColumn.FieldName.ToUpper();

            DataTable dt = (DataTable)dgv2.DataSource;
            DataRow dr = gridMain2.GetFocusedDataRow();
            int rowhandle = gridMain2.FocusedRowHandle;
            int row = gridMain2.GetDataSourceRowIndex(rowhandle);
            string str = dr["duplicate"].ObjToString();
            bool duplicate = false;
            if (str.ToUpper() == "Y")
                duplicate = true;
            if (column == "TRANSACTIONAMOUNT" && !duplicate )
                e.Cancel = true;
            else if (column == "AUTHORIZEDAMOUNT" && !duplicate)
                e.Cancel = true;
            else if (column == "AMOUNT" && !duplicate)
                e.Cancel = true;
            else if (column == "INVOICENUMBER" && !duplicate)
                e.Cancel = true;
            else
            {
                if (column == "SELECT")
                    return;
                if (column == "CCFEE")
                    return;
                if (column == "TRUSTFUNERAL")
                    return;
                if (column == "TRANSACTIONAMOUNT")
                    return;
                else if (column == "AUTHORIZEDAMOUNT")
                    return;
                else if (column == "AMOUNT")
                    return;
                else if (column == "INVOICENUMBER")
                    return;
                //if (column != "REFERENCENUMBER")
                //    return;
                e.Cancel = true;
            }
        }
        /***********************************************************************************************/
        private void gridMain3_ShowingEditor(object sender, CancelEventArgs e)
        { // Posted Editor
            GridView view = sender as GridView;
            string column = view.FocusedColumn.FieldName.ToUpper();

            DataTable dt = (DataTable)dgv3.DataSource;
            DataRow dr = gridMain3.GetFocusedDataRow();
            int rowhandle = gridMain3.FocusedRowHandle;
            int row = gridMain3.GetDataSourceRowIndex(rowhandle);
            string str = dr["duplicate"].ObjToString();
            bool duplicate = false;
            if (str.ToUpper() == "Y")
                duplicate = true;
            if (column == "TRANSACTIONAMOUNT" && !duplicate)
                e.Cancel = true;
            else if (column == "AUTHORIZEDAMOUNT" && !duplicate)
                e.Cancel = true;
            else if (column == "AMOUNT" && !duplicate)
                e.Cancel = true;
            else if (column == "TRUSTFUNERAL")
                return;
            else
            {
                if (column == "CCFEE")
                    return;
                //if (column != "REFERENCENUMBER")
                //    return;
                e.Cancel = true;
            }
        }
        /***********************************************************************************************/
        private void btnPullVoids_Click(object sender, EventArgs e)
        {
            string cmd = "Select * from `bank_cc` WHERE `transactionType` = 'Void';";
            if (chkHonorDates4.Checked)
            {
                DateTime date = this.dateTimePicker6.Value;
                string date1 = date.ToString("yyyy-MM-dd 00:00:00");

                date = this.dateTimePicker7.Value;
                string date2 = date.ToString("yyyy-MM-dd 23:59:59");
                cmd = "Select * from `bank_cc` WHERE transactionDate` >= '" + date1 + "' AND `transactionDate` <= '" + date2 + "' AND `transactionType` =`Void';";
            }

            DataTable dt = G1.get_db_data(cmd);

            G1.NumberDataTable(dt);
            dgv5.DataSource = dt;
        }
        /***********************************************************************************************/
        private void LookupSomeone ( string invoiceNumber, string tf )
        {
            string contract = invoiceNumber;
            string[] Lines = contract.Split(' ');
            if (Lines.Length <= 0)
                return;
            if (tf.ToUpper() == "DOWNPMT")
            {
                if (Lines.Length >= 2)
                {
                    int count = Lines.Length;
                    string fname = Lines[count - 2];
                    string lname = Lines[count - 1];
                    if (!String.IsNullOrWhiteSpace(fname) && !String.IsNullOrWhiteSpace(lname))
                    {
                        string cmd = "Select * from `downpayments` where `lastName` = '" + lname + "' AND `firstName` = '" + fname + "' LIMIT 10;";
                        DataTable dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count > 0)
                        {
                            this.Cursor = Cursors.WaitCursor;
                            DownPayments dpForm = new DownPayments(dx);
                            dpForm.Show();
                            this.Cursor = Cursors.Default;
                        }
                        else
                        {
                            MessageBox.Show("*** ERROR *** Cannot find any Down Payments for :\n" + fname + " " + lname + "!!!", "Down Payment Lookup Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                        }
                    }
                }
                return;
            }

            contract = Lines[0].Trim();
            if (!String.IsNullOrWhiteSpace(contract))
            {
                this.Cursor = Cursors.WaitCursor;
                string what = tf;
                if (what.ToUpper() == "TRUST")
                {
                    this.Cursor = Cursors.WaitCursor;
                    CustomerDetails clientForm = new CustomerDetails(contract);
                    clientForm.Show();
                    this.Cursor = Cursors.Default;
                }
                else if (what.ToUpper() == "INSURANCE")
                {
                    string cmd = "Select * from `payers` WHERE `payer` = '" + contract + "';";
                    DataTable dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                    {
                        this.Cursor = Cursors.WaitCursor;
                        contract = dx.Rows[0]["contractNumber"].ObjToString();
                        CustomerDetails clientForm = new CustomerDetails(contract);
                        clientForm.Show();
                        this.Cursor = Cursors.Default;
                    }
                    else
                        MessageBox.Show("*** ERROR *** Cannot find any Payer for :\n" + contract + "!!!", "Insurance Lookup Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                }
                else
                {
                    string serviceId = contract;
                    string cmd = "Select * from `fcust_extended` WHERE `serviceId` = '" + contract + "';";
                    DataTable dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                    {
                        this.Cursor = Cursors.WaitCursor;
                        contract = dx.Rows[0]["contractNumber"].ObjToString();
                        EditCust custForm = new EditCust(contract);
                        custForm.Show();
                        this.Cursor = Cursors.Default;

                    }
                    else
                        MessageBox.Show("*** ERROR *** Funeral Service ID : " + serviceId + " does not exist!!!", "Funeral Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                }
                this.Cursor = Cursors.Default;
            }
        }
        /***********************************************************************************************/
        private void gridMain2_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain2.GetFocusedDataRow();
            string contract = dr["invoiceNumber"].ObjToString();
            string tf = dr["trustFuneral"].ObjToString();

            LookupSomeone(contract, tf);
        }
        /***********************************************************************************************/
        private void gridMain2_RowCellStyle(object sender, RowCellStyleEventArgs e)
        {
            GridView View = sender as GridView;
            string str = View.GetRowCellValue(e.RowHandle, "bankAccount").ObjToString();
            if (str != null)
            {
                if (str.ToUpper() == "N/A")
                    e.Appearance.BackColor = Color.Pink;
            }
        }
        /***********************************************************************************************/
        private void unPostToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain2.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            dr["posted"] = "";
        }
        /***********************************************************************************************/
        private void editInvoiceNumberToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain2.GetFocusedDataRow();
            DataTable dt = (DataTable)dgv2.DataSource;
            int rowhandle = gridMain2.FocusedRowHandle;
            int row = gridMain2.GetDataSourceRowIndex(rowhandle);

            DataTable newDt = dt.Clone();
            newDt.ImportRow(dt.Rows[row]);


            EditInvoiceNumber editForm = new EditInvoiceNumber ( newDt );
            DialogResult result = editForm.ShowDialog();
            if (result == DialogResult.OK)
            {
                string invoiceNumber = newDt.Rows[0]["invoiceNumber"].ObjToString();

                dt.Rows[row]["trustFuneral"] = categorizeInvoiceNumber(ref invoiceNumber);
                dt.Rows[row]["invoiceNumber"] = invoiceNumber;
                dt.Rows[row]["mod"] = "Y";
                dgv2.DataSource = dt;
                dgv2.RefreshDataSource();
                dgv2.Refresh();

                newDt.Dispose();
                newDt = null;
            }
        }
        /***********************************************************************************************/
        private void editInvoiceNumberToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            DataTable dt = (DataTable)dgv.DataSource;
            int rowhandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowhandle);

            DataTable newDt = dt.Clone();
            newDt.ImportRow(dt.Rows[row]);


            EditInvoiceNumber editForm = new EditInvoiceNumber(newDt);
            DialogResult result = editForm.ShowDialog();
            if (result == DialogResult.OK)
            {
                string invoiceNumber = newDt.Rows[0]["invoiceNumber"].ObjToString();

                dt.Rows[row]["trustFuneral"] = categorizeInvoiceNumber(ref invoiceNumber);
                dt.Rows[row]["invoiceNumber"] = invoiceNumber;
                dgv.DataSource = dt;
                dgv.RefreshDataSource();
                dgv.Refresh();

                newDt.Dispose();
                newDt = null;
            }
        }
        /***********************************************************************************************/
    }
}