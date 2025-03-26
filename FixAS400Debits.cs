using DevExpress.CodeParser;
using DevExpress.Pdf;
using DevExpress.Utils;
using DevExpress.Utils.Drawing;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraEditors.ViewInfo;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Controls;
using DevExpress.XtraGrid.Views.BandedGrid;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports.UI;
using GeneralLib;
using System;
using System.Data;
using System.Drawing;
using System.Web.UI.WebControls;
using System.Windows.Forms;
using GridView = DevExpress.XtraGrid.Views.Grid.GridView;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class FixAS400Debits : DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        private DataTable workDt = null;
        private DataTable originalDt = null;
        private string workReport = "";
        private DataTable oDt = null;
        private bool loading = false;
        private bool insurance = false;
        private bool foundLocalPreference = false;
        private GridView originalGridView = null;
        private bool showFooters = true;
        /***********************************************************************************************/
        public FixAS400Debits()
        {
            InitializeComponent();
            workReport = "";
        }
        /***********************************************************************************************/
        RepositoryItemDateEdit ri = null;
        private void FixAS400Debits_Load(object sender, EventArgs e)
        {
            this.Text = "AS400 Debit Fix Report";
            workReport = this.Text;
            btnFix.Hide();
            button1.Hide();
            barImport.Hide();
            lblTotal.Hide();

            gridMain.Columns["retained"].Visible = false;
            gridMain.Columns["principal"].Visible = false;
        }
        /***********************************************************************************************/
        private void LoadData()
        {
            this.Cursor = Cursors.WaitCursor;
            btnFix.Hide();
            button1.Hide();

            string cmd = "SELECT * FROM payments p JOIN contracts x ON p.`contractNumber` = x.`contractNumber` WHERE debitAdjustment > '0' AND trust85P > '0' AND `fill` <> 'D' ORDER BY payDate8 DESC;";

            DataTable dt = G1.get_db_data(cmd);

            dt.Columns.Add("totalTrust85", Type.GetType("System.Double"));
            dt.Columns.Add("endingBalance", Type.GetType("System.Double"));
            dt.Columns.Add("principal", Type.GetType("System.Double"));

            G1.NumberDataTable(dt);
            originalDt = dt.Copy();
            dgv.DataSource = dt;

            ScaleCells();

            if (dt.Rows.Count > 0)
            {
                //btnFix.Show();
                button1.Show();
            }

            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            G1.ShowHideFindPanel(gridMain);
        }
        /***********************************************************************************************/
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadData();
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

//            Printer.setupPrinterMargins(50, 50, 80, 50);
            Printer.setupPrinterMargins(5, 5, 80, 50);

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

            printingSystem1.Document.AutoFitToPagesWidth = 1;

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
            string title = workReport + " Report";
            Printer.DrawQuad(6, 8, 4, 4, title, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            //            Printer.DrawQuadTicks();
            DateTime date = DateTime.Now;
            string workDate = date.Month.ToString("D2") + "/" + date.Year.ToString("D4");
            Printer.SetQuadSize(24, 12);
            font = new Font("Ariel", 9, FontStyle.Regular);
            //Printer.DrawQuad(20, 8, 5, 4, "Report Month:" + workDate, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(16, 8, 3, 4, lblPayment.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(19, 8, 3, 4, lblTrust85.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(22, 8, 3, 4, lblTrust100.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /***********************************************************************************************/
        private void gridMain_DoubleClick_2(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();
            string contract = dr["contractNumber"].ObjToString();
            if (!String.IsNullOrWhiteSpace(contract))
            {

                this.Cursor = Cursors.WaitCursor;
                CustomerDetails clientForm = new CustomerDetails(contract);
                clientForm.Show();
                this.Cursor = Cursors.Default;
            }
        }
        /***********************************************************************************************/
        private void gridMain_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            ColumnView view = sender as ColumnView;

            string columnName = e.Column.FieldName.ToUpper();

            if (e.Column.FieldName.ToUpper().IndexOf("DATE") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                if (e.DisplayText.IndexOf("0000") >= 0 || e.DisplayText.IndexOf("0001") >= 0)
                    e.DisplayText = "";
                else
                {
                    DateTime date = e.DisplayText.ObjToString().ObjToDateTime();
                    if (date.Year > 100)
                        e.DisplayText = date.ToString("MM/dd/yyyy");
                    else
                        e.DisplayText = "";
                }
            }
        }
        /****************************************************************************************/
        private void txtScale_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string balance = txtScale.Text.Trim();
                if (!G1.validate_numeric(balance))
                {
                    MessageBox.Show("***ERROR*** Scale must be numeric!");
                    return;
                }
                double money = balance.ObjToDouble();
                balance = G1.ReformatMoney(money);
                txtScale.Text = balance;
                ScaleCells();
                return;
            }
            // Initialize the flag to false.
            bool nonNumberEntered = false;

            // Determine whether the keystroke is a number from the top of the keyboard.
            if (e.KeyCode < Keys.D0 || e.KeyCode > Keys.D9)
            {
                // Determine whether the keystroke is a number from the keypad.
                if (e.KeyCode < Keys.NumPad0 || e.KeyCode > Keys.NumPad9)
                {
                    // Determine whether the keystroke is a backspace.
                    if (e.KeyCode != Keys.Back)
                    {
                        // A non-numerical keystroke was pressed.
                        // Set the flag to true and evaluate in KeyPress event.
                        if (e.KeyCode != Keys.OemPeriod)
                            nonNumberEntered = true;
                    }
                }
            }
            //If shift key was pressed, it's not a number.
            if (Control.ModifierKeys == Keys.Shift)
            {
                nonNumberEntered = true;
            }
            if (nonNumberEntered)
            {
                MessageBox.Show("***ERROR*** Key entered must be a number!");
                e.Handled = true;
            }
        }
        /****************************************************************************************/
        private double originalSize = 0D;
        private Font mainFont = null;
        private void ScaleCells()
        {
            if (originalSize == 0D)
            {
                //                originalSize = gridMain.Columns["address1"].AppearanceCell.FontSizeDelta.ObjToDouble();
                originalSize = gridMain.Columns["firstName"].AppearanceCell.Font.Size;
                mainFont = gridMain.Columns["firstName"].AppearanceCell.Font;
            }
            double scale = txtScale.Text.ObjToDouble();
            double size = scale / 100D * originalSize;
            Font font = new Font(mainFont.Name, (float)size);
            for (int i = 0; i < gridMain.Columns.Count; i++)
            {
                gridMain.Columns[i].AppearanceCell.Font = font;
            }
            gridMain.RefreshData();
            dgv.Refresh();
            this.Refresh();
        }
        /***********************************************************************************************/
        private void btnFix_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();
            string contract = dr["contractNumber"].ObjToString();
            string record = dr["record"].ObjToString();

            double trust100 = dr["trust100P"].ObjToDouble();
            double trust85 = dr["trust85P"].ObjToDouble();
            if ( trust85 < 0D)
            {
                MessageBox.Show("It looks like the Trust85 has already been corrected!!", "Trust85 Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }

            double dValue = -1D * trust100;
            dr["trust100P"] = dValue;

            dValue = -1D * trust85;
            dr["trust85P"] = dValue;

            if ( chkActive.Checked )
                G1.update_db_table("payments", "record", record, new string[] { "trust85P", trust85.ToString(), "trust100P", trust100.ToString()});

            double totalTrust85 = dr["totalTrust85"].ObjToDouble();
            totalTrust85 = totalTrust85 - trust85;
            totalTrust85 = totalTrust85 + dValue;

            //dr["totalTrust85"] = totalTrust85;

            DataRow[] dRows = dt.Select("contractNumber='" + contract + "'");
            for ( int i=0; i<dRows.Length; i++)
            {
                dRows[i]["totalTrust85"] = totalTrust85;
            }

            dgv.Refresh();
        }
        /***********************************************************************************************/
        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable dt = (DataTable)dgv.DataSource;
                if (dt == null)
                    return;
                if (dt.Rows.Count <= 0)
                    return;

                string filter = "CSV files (*.csv)|*.csv";
                string f = "CSV files(*.csv) | *.csv";
                saveFileDialog1.Filter = f;
                saveFileDialog1.FilterIndex = 0;
                saveFileDialog1.FileName = "";
                if (saveFileDialog1.ShowDialog() != DialogResult.OK)
                    return;

                string filename = saveFileDialog1.FileName;

                DataTable tempDt = dt.Copy();
                //tempDt.Columns.RemoveAt(0);

                this.Cursor = Cursors.WaitCursor;
                try
                {
                    MySQL.CreateCSVfile(tempDt, filename, true, "~");
                }
                catch (Exception ex)
                {
                }
            }
            catch ( Exception ex)
            {
            }
            this.Cursor = Cursors.Arrow;
        }
        /***********************************************************************************************/
        private void ConvertColumn ( DataTable dt, string columnName, string caption )
        {
            dt.Columns.Add("tempColumn", Type.GetType("System.Double"));
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                dt.Rows[i]["tempColumn"] = dt.Rows[i][columnName].ObjToDouble();
            }
            try
            {
                dt.Columns.Remove(dt.Columns[columnName]);
            }
            catch (Exception ex)
            {
            }
            dt.Columns["tempColumn"].Caption = caption;
            dt.Columns["tempColumn"].ColumnName = columnName;
        }
        /***********************************************************************************************/
        private string oldReadFile = "";
        private void readOldFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string file = ofd.FileName;
                    DataTable dt = Import.ImportCSVfile(file, null, true, "~");
                    oldReadFile = file;
                    //dt.Columns["Num"].ColumnName = "num";

                    try
                    {
                        ConvertColumn(dt, "trust85P", "Trust85");
                        ConvertColumn(dt, "trust100P", "Trust100");
                        ConvertColumn(dt, "debitAdjustment", "debit");
                        ConvertColumn(dt, "interestPaid", "Interest");
                        ConvertColumn(dt, "principal", "Principal");
                        ConvertColumn(dt, "retained", "Retained");
                        ConvertColumn(dt, "totalTrust85", "Total DH Trust85");
                        ConvertColumn(dt, "endingBalance", "TBB");
                    }
                    catch ( Exception ex)
                    {
                    }

                    G1.NumberDataTable(dt);
                    dgv.DataSource = dt;
                    ScaleCells();
                    btnFix.Show();
                    button1.Show();
                    //G1.SetupVisibleColumns(gridMain3, this.columnsToolStripMenuItem, nmenu_Click);
                    //tabControl1.SelectTab("tabDetail");
                }
            }
        }
        /***********************************************************************************************/
        private void button1_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;

            string contractNumber = "";
            string record = "";
            string lockTrust85 = "";
            double interest = 0D;
            double contractValue = 0D;
            double rate = 0D;
            double amtOfMonthlyPayt = 0D;
            double financeMonths = 0D;
            DateTime dueDate8 = DateTime.Now;
            DateTime lastDate = DateTime.Now;
            DateTime deceasedDate = DateTime.Now;
            double originalDownPayment = 0D;
            double startBalance = 0D;
            string cmd = "";
            string apr = "";
            double dAPR = 0D;
            double payment = 0D;
            int numPayments = 0;
            double totalInterest = 0D;
            DataTable dp = null;
            string dueDate = "";
            string issueDate = "";
            DateTime iDate = DateTime.Now;

            double principal = 0D;
            double retained = 0D;
            string fill = "";

            DataRow[] dRows = null;

            int lastRow = dt.Rows.Count;
            //lastRow = 10;

            barImport.Show();
            lblTotal.Show();
            barImport.Minimum = 0;
            barImport.Maximum = lastRow;

            string testContract = "E16017UI";

            testContract = "";

            if (!String.IsNullOrWhiteSpace(testContract))
            {
                dRows = dt.Select("contractNumber='" + testContract + "'");
                if (dRows.Length > 0)
                {
                    dt = dRows.CopyToDataTable();
                    lastRow = 1;
                    dgv.DataSource = dt;
                }
            }

            for (int i = 0; i < lastRow; i++)
            {
                barImport.Value = i + 1;
                barImport.Refresh();

                lblTotal.Text = (i + 1).ToString() + " of " + lastRow.ToString();
                lblTotal.Refresh();

                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                lockTrust85 = dt.Rows[i]["lockTrust85"].ObjToString();

                try
                {
                    interest = dt.Rows[i]["interestPaid"].ObjToDouble();

                    contractValue = DailyHistory.GetContractValuePlus(dt.Rows[i]);
                    rate = dt.Rows[i]["apr1"].ObjToDouble();

                    //dt.Rows[i]["contractValue"] = contractValue;
                    //dt.Rows[i]["apr"] = rate;

                    payment = dt.Rows[i]["amtOfMonthlyPayt"].ObjToString().ObjToDouble();
                    amtOfMonthlyPayt = payment;
                    numPayments = dt.Rows[i]["numberOfPayments"].ObjToString().ObjToInt32();
                    financeMonths = (double)numPayments;
                    totalInterest = dt.Rows[i]["totalInterest"].ObjToString().ObjToDouble();
                    dueDate = dt.Rows[i]["dueDate8"].ObjToString();
                    dueDate8 = dueDate.ObjToDateTime();
                    issueDate = dt.Rows[i]["issueDate8"].ObjToString();
                    //                    DateTime iDate = DailyHistory.GetIssueDate(dt.Rows[i]["issueDate8"].ObjToDateTime(), contractNumber, dx);
                    iDate = DailyHistory.GetIssueDate(dt.Rows[i]["issueDate8"].ObjToDateTime(), contractNumber, null);
                    issueDate = iDate.ToString("MM/dd/yyyy");
                    lastDate = issueDate.ObjToDateTime();
                    originalDownPayment = dt.Rows[i]["downPayment"].ObjToDouble();
                    if (originalDownPayment <= 0D)
                        originalDownPayment = DailyHistory.GetOriginalDownPayment(dt.Rows[i]);

                    //dt.Rows[i]["issueDate"] = issueDate;
                    deceasedDate = dt.Rows[i]["deceasedDate"].ObjToDateTime();
                    //dt.Rows[i]["Pmts"] = numPayments.ToString();

                    apr = dt.Rows[i]["APR1"].ObjToString();
                    dAPR = apr.ObjToDouble() / 100.0D;

                    startBalance = DailyHistory.GetFinanceValue(contractNumber);
                    cmd = "Select * from `payments` where `contractNumber` = '" + contractNumber + "' order by `paydate8` DESC, `tmstamp` DESC;";
                    dp = G1.get_db_data(cmd);

                    DailyHistory.CalculateNewStuff(dp, dAPR, numPayments, startBalance, lastDate);

                    payment = 0D;
                    principal = 0D;
                    interest = 0D;
                    retained = 0D;
                    for ( int j=0; j<dp.Rows.Count; j++)
                    {
                        fill = dp.Rows[j]["fill"].ObjToString().ToUpper();
                        if ( fill != "D")
                            payment += dp.Rows[j]["trust85P"].ObjToDouble();
                    }
                    payment = G1.RoundValue(payment);
                    dt.Rows[i]["totalTrust85"] = payment;

                    record = dt.Rows[i]["record"].ObjToString();
                    dRows = dp.Select("record='" + record + "'");
                    if (dRows.Length > 0)
                    {
                        principal = dRows[0]["principal"].ObjToDouble();
                        retained = dRows[0]["retained"].ObjToDouble();
                        interest = dRows[0]["interestPaid"].ObjToDouble();

                        principal = G1.RoundValue(principal);
                        retained = G1.RoundValue(retained);
                        interest = G1.RoundValue(interest);
                    }
                    dt.Rows[i]["principal"] = principal;
                    //dt.Rows[i]["interestPaid"] = interest;
                    dt.Rows[i]["retained"] = retained;

                    cmd = "Select * from `trust2013r` where `contractNumber` = '" + contractNumber + "' AND ( `endingBalance` > '0' OR `beginningBalance` > '0' ) ORDER BY `payDate8` DESC LIMIT 2;";
                    dp = G1.get_db_data(cmd);
                    payment = 0D;
                    if (dp.Rows.Count > 0)
                    {
                        payment = dp.Rows[0]["endingBalance"].ObjToDouble();
                        if (payment == 0D)
                            payment = dp.Rows[0]["beginningBalance"].ObjToDouble();
                        payment = G1.RoundValue(payment);
                    }
                    dt.Rows[i]["endingBalance"] = payment;
                    //if (i >= 100)
                    //    break;
                }
                catch (Exception ex)
                {
                }
            }
            lblTotal.Text = lastRow.ToString() + " of " + lastRow.ToString();
            lblTotal.Refresh();
            btnFix.Show();
            gridMain.Columns["retained"].Visible = true;
            gridMain.Columns["principal"].Visible = true;
        }
        /***********************************************************************************************/
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if ( String.IsNullOrWhiteSpace ( oldReadFile ))
            {
                MessageBox.Show("It doesn't look like you've previously read a file into the system!", "Read File Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }
            string filename = oldReadFile;

            DataTable dt = (DataTable)dgv.DataSource;

            this.Cursor = Cursors.WaitCursor;
            try
            {
                MySQL.CreateCSVfile(dt, filename, true, "~");
            }
            catch ( Exception ex)
            {
            }
            this.Cursor = Cursors.Arrow;
        }
        /***********************************************************************************************/
    }
}
