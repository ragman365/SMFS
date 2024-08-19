using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using GeneralLib;
using DevExpress.XtraPrinting;
using DevExpress.Utils;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;


using System.Collections.Generic;
using System.ComponentModel;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraPrinting;


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
using System.Drawing.Drawing2D;
using DevExpress.XtraPrintingLinks;
using DevExpress.XtraGrid.Views.BandedGrid;
using ExcelLibrary.BinaryFileFormat;
using DevExpress.XtraBars.ViewInfo;
using System.Text;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraEditors.ViewInfo;
using DevExpress.XtraGrid.Columns;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Net.Mail;

/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class DoubleCheckPayments : DevExpress.XtraEditors.XtraForm
    {

        private DataTable originalDt = null;
        /****************************************************************************************/
        public DoubleCheckPayments()
        {
            InitializeComponent();
        }
        /****************************************************************************************/
        private void DoubleCheckPayments_Load(object sender, EventArgs e)
        {

            DateTime now = DateTime.Now;

            gridMain.Columns["num"].Visible = true;

            gridMain.OptionsPrint.PrintBandHeader = false;
            gridMain.OptionsPrint.PrintFooter = false;

        }
        /****************************************************************************************/
        private void btnRun_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            DataTable dx = null;
            DataRow dRows = null;
            DataTable dp = null;
            DataRow dR = null;
            DataTable cDt = null;

            DataTable ddx = new DataTable();
            ddx.Columns.Add("num");
            ddx.Columns.Add("contractNumber");
            ddx.Columns.Add("currentDueDate");
            ddx.Columns.Add("correctDueDate");
            ddx.Columns.Add("lastPaidDate");
            ddx.Columns.Add("lapseDate");
            ddx.Columns.Add("actualDueDate");
            string contractNumber = "";
            string currentDueDate = "";
            string correctDueDate = "";
            string str = "";

            int row = 0;
            string contract = "";
            double balanceDue = 0D;
            double payment = 0D;
            double originalDownPayment = 0D;
            int numPayments = 0;
            double startBalance = 0D;
            DateTime lastDate = DateTime.Now;
            double dAPR = 0D;
            double newBalance = 0D;
            DateTime trustDueDate = DateTime.Now;
            DateTime oldDueDate = DateTime.Now;
            DateTime nextDueDate = DateTime.Now;
            DateTime newNextDueDate = DateTime.Now;
            DateTime date = DateTime.Now;
            DateTime dolp = DateTime.Now;
            DateTime dueDate8 = DateTime.Now;
            DateTime deceasedDate = DateTime.Now;
            DateTime curDueDate = DateTime.Now;
            DateTime lapseDate = DateTime.Now;
            DateTime reinstateDate = DateTime.Now;
            DateTime actualDueDate = DateTime.Now;
            DateTime firstPayDate = DateTime.Now;
            string sLapseDate = "";

            double creditBalance = 0D;
            double months = 0D;
            int idx = 0;
            bool adding = true;
            DataTable ddd = null;
            DailyHistory dailyForm = null;
            int lastRow = 0;

            string cmd = "Select * from `contracts` where `dueDate8` <> '2039-12-31' ";
            contract = txtContract.Text.Trim();
            if ( !String.IsNullOrWhiteSpace ( contract ))
                cmd += " AND `contractNumber` = '" + contract + "' ";
            cmd += ";";
            DataTable dt = G1.get_db_data(cmd);

            barImport.Minimum = 0;
            barImport.Maximum = dt.Rows.Count;
            barImport.Value = 0;
            barImport.Refresh();

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    Application.DoEvents();

                    barImport.Value = (i + 1);
                    barImport.Refresh();
                    row = i;

                    //if (i >= 100)
                    //    break;

                    date = dt.Rows[row]["issueDate8"].ObjToDateTime();
                    trustDueDate = dt.Rows[row]["dueDate8"].ObjToDateTime();
                    if (trustDueDate.Year < 1000)
                        continue;
                    dolp = dt.Rows[row]["lastDatePaid8"].ObjToDateTime();
                    if (dolp.Year < 1000)
                        continue;
                    deceasedDate = dt.Rows[row]["deceasedDate"].ObjToDateTime();
                    if (deceasedDate.Year > 1000)
                        continue;
                    actualDueDate = trustDueDate;
                    trustDueDate = new DateTime(trustDueDate.Year, trustDueDate.Month, 1);
                    lapseDate = dt.Rows[row]["lapseDate8"].ObjToDateTime();
                    sLapseDate = lapseDate.ToString("MM/dd/yyyy");
                    reinstateDate = dt.Rows[row]["reinstateDate8"].ObjToDateTime();
                    if (reinstateDate > lapseDate)
                        sLapseDate = "";
                    balanceDue = dt.Rows[row]["balanceDue"].ObjToDouble();
                    contract = dt.Rows[row]["contractNumber"].ObjToString();
                    payment = dt.Rows[row]["amtOfMonthlyPayt"].ObjToString().ObjToDouble();
                    numPayments = dt.Rows[row]["numberOfPayments"].ObjToString().ObjToInt32();

                    nextDueDate = trustDueDate;
                    try
                    {
                        cmd = "Select * from `customers` where `contractNumber` = '" + contract + "';";
                        cmd += ";";
                        cDt = G1.get_db_data(cmd);

                        if (cDt.Rows.Count <= 0)
                            continue;

                        firstPayDate = cDt.Rows[0]["firstPayDate"].ObjToDateTime();

                        dailyForm = new DailyHistory(contract, true);
                        if (dailyForm == null)
                            continue;
                        ddd = dailyForm.FireEventDailyReturn();
                        if (ddd == null)
                            continue;
                    }
                    catch ( Exception ex )
                    {
                        if (dailyForm != null)
                            dailyForm.Close();
                        dailyForm = null;
                        continue;
                    }
                    if ( ddd.Rows.Count > 0 )
                    {
                        lastRow = ddd.Rows.Count - 1;
                        nextDueDate = ddd.Rows[lastRow]["nextDueDate"].ObjToDateTime();
                        nextDueDate = ddd.Rows[lastRow]["currentDueDate8"].ObjToDateTime();
                        if (firstPayDate.Year < 1000 && ddd.Rows.Count > 1)
                            firstPayDate = ddd.Rows[1]["dueDate8"].ObjToDateTime();
                    }
                    if (dailyForm != null)
                        dailyForm.Close();
                    dailyForm = null;

                    if (nextDueDate.Year < 1000)
                        continue;
                    if (nextDueDate <= DailyHistory.majorDate)
                        continue;
                    if ( firstPayDate.Year > 1000 )
                    {
                        if (firstPayDate > nextDueDate)
                            nextDueDate = firstPayDate;
                    }
                    if ( nextDueDate != trustDueDate )
                    {
                        dRows = ddx.NewRow();
                        dRows["contractNumber"] = contract;
                        dRows["currentDueDate"] = trustDueDate.ToString("MM/dd/yyyy");
                        dRows["correctDueDate"] = nextDueDate.ToString("MM/dd/yyyy");
                        dRows["lastPaidDate"] = dolp.ToString("MM/dd/yyyy");
                        dRows["lapseDate"] = sLapseDate;
                        dRows["actualDueDate"] = actualDueDate.ToString("MM/dd/yyyy");
                        ddx.Rows.Add(dRows);
                    }
                    if (1 == 1)
                        continue;


                    DailyHistory.DetermineDueDate(contract, dolp, payment, payment, 0D, 0D, ref nextDueDate, ref creditBalance, ref months, ref newBalance, ref curDueDate);

                    DailyHistory.CalculateNewStuff(dp, dAPR, numPayments, startBalance, lastDate);

                    dx = LoadMainData(contract, startBalance, payment);

                    double newbalance = 0D;
                    newNextDueDate = DailyHistory.getNextDueDate(dp, payment, ref newbalance);


                    if (dx.Rows.Count > 0 && dp.Rows.Count > 0 )
                    {
                        str = dx.Rows[0]["NumPayments"].ObjToString();
                        idx = str.IndexOf(".");
                        if (idx > 0)
                            str = str.Substring(0, idx);
                        numPayments = Convert.ToInt32(Convert.ToDouble(str));
                        oldDueDate = dx.Rows[0]["oldDueDate8"].ObjToDateTime();

                        //DataView tempview = dx.DefaultView;
                        //tempview.Sort = "dueDate8 DESC";
                        //dx = tempview.ToTable();

                        adding = true;
                        //oldDueDate = dx.Rows[0]["oldDueDate8"].ObjToDateTime();
                        nextDueDate = dp.Rows[0]["nextDueDate"].ObjToDateTime();
                        dueDate8 = dp.Rows[0]["dueDate8"].ObjToDateTime();
                        if (dueDate8 < new DateTime(2039, 12, 31))
                            nextDueDate = dueDate8.AddMonths(numPayments);
                        else
                            nextDueDate = dueDate8;
                        if (dolp < DailyHistory.majorDate )
                            continue;
                        if (!String.IsNullOrWhiteSpace(txtContract.Text.Trim()))
                        {
                        }
                        if ( nextDueDate > oldDueDate )
                        {
                            oldDueDate = nextDueDate;
                            adding = false;
                        }
                        if (oldDueDate.Year > 1000)
                        {
                            //str = dx.Rows[0]["NumPayments"].ObjToString();
                            //idx = str.IndexOf(".");
                            //if (idx > 0)
                            //    str = str.Substring(0, idx);
                            //numPayments = Convert.ToInt32(Convert.ToDouble(str));
                            if ( !adding )
                                numPayments = 0;
                            try
                            {
                                if (numPayments != 0)
                                    oldDueDate = oldDueDate.AddMonths(numPayments);
                                if (oldDueDate != trustDueDate)
                                {
                                    if (oldDueDate > DailyHistory.majorDate )
                                     {
                                        dRows = ddx.NewRow();
                                        dRows["contractNumber"] = contract;
                                        dRows["currentDueDate"] = trustDueDate.ToString("MM/dd/yyyy");
                                        dRows["correctDueDate"] = nextDueDate.ToString("MM/dd/yyyy");
                                        ddx.Rows.Add(dRows);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    //MessageBox.Show("***ERROR*** Customer i=" + i.ToString() + " " + ex.Message.ToString());
                }
            }


            G1.NumberDataTable(ddx);

            dgv.DataSource = ddx;

            barImport.Value = lastRow;
            barImport.Refresh();

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
            gridMain.OptionsPrint.ExpandAllGroups = false;

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

            Printer.setupPrinterMargins(10, 5, 80, 50);

            pageMarginLeft = Printer.pageMarginLeft;
            pageMarginRight = Printer.pageMarginRight;
            pageMarginTop = Printer.pageMarginTop;
            pageMarginBottom = Printer.pageMarginBottom;

            printableComponentLink1.Margins.Left = pageMarginLeft;
            printableComponentLink1.Margins.Right = pageMarginRight;
            printableComponentLink1.Margins.Top = pageMarginTop;
            printableComponentLink1.Margins.Bottom = pageMarginBottom;

            printingSystem1.Document.AutoFitToPagesWidth = 1;

            Font saveFont = gridMain.AppearancePrint.Row.Font;

            G1.PrintPreview(printableComponentLink1, gridMain);

            //printableComponentLink1.CreateDocument();
            //printableComponentLink1.ShowPreview();

            gridMain.Appearance.Row.Font = saveFont;
        }
        /***********************************************************************************************/
        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            gridMain.OptionsPrint.ExpandAllGroups = false;

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

            Printer.setupPrinterMargins(10, 5, 80, 50);

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
            //string title = "Contract Activity Report";
            //Printer.DrawQuad(6, 8, 4, 4, title, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            string reportName = "Duedate Issues Report";
            string report = reportName + " Report for " + DateTime.Now.ToString("MM/dd/yyyy");
            Printer.DrawQuad(5, 8, 8, 4, report, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);



            DateTime date = DateTime.Now;
            string workDate = date.Month.ToString("D2") + "/" + date.Year.ToString("D4");
            Printer.SetQuadSize(24, 12);
            font = new Font("Ariel", 9, FontStyle.Regular);
            //Printer.DrawQuad(20, 8, 5, 4, "Month Closing - " + workDate, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            //Printer.DrawQuad(16, 8, 3, 4, lblPayment.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(19, 8, 3, 4, lblTrust85.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(22, 8, 3, 4, lblTrust100.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /****************************************************************************************/
        private bool pageBreak = false;
        private void gridMain_BeforePrintRow(object sender, DevExpress.XtraGrid.Views.Printing.CancelPrintRowEventArgs e)
        {
            int rowHandle = e.RowHandle;
            if (gridMain.IsDataRow(rowHandle))
            {
                try
                {
                    DataTable dt = (DataTable)dgv.DataSource;
                    int row = gridMain.GetDataSourceRowIndex(rowHandle);

                    string newPage = dt.Rows[row]["C5"].ObjToString();
                    if (newPage.ToUpper() == "BREAK")
                    {
                        pageBreak = true;
                        e.Cancel = true;
                    }
                }
                catch (Exception ex)
                {
                }
            }
            if (e.HasFooter)
            {
            }
        }
        /****************************************************************************************/
        private void gridMain_AfterPrintRow(object sender, DevExpress.XtraGrid.Views.Printing.PrintRowEventArgs e)
        {
            if (pageBreak)
                e.PS.InsertPageBreak(e.Y);
            pageBreak = false;
        }
        /****************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        { // Spy Glass
            G1.ShowHideFindPanel(gridMain);
        }
        /****************************************************************************************/
        private double originalSize = 0D;
        private Font mainFont = null;
        private Font newFont = null;
        private Font HeaderFont = null;
        private double originalHeaderSize = 0D;
        private void ScaleCells()
        {
            if (1 == 1)
                return;
            if (originalSize == 0D)
            {
                //                originalSize = gridMain.Columns["address1"].AppearanceCell.FontSizeDelta.ObjToDouble();
                originalSize = gridMain.Columns["Location Name"].AppearanceCell.Font.Size;
                mainFont = gridMain.Columns["Location Name"].AppearanceCell.Font;
                HeaderFont = gridMain.Appearance.HeaderPanel.Font;
                originalHeaderSize = gridMain.Appearance.HeaderPanel.Font.Size;
            }
            double scale = txtScale.Text.ObjToDouble();
            double size = scale / 100D * originalSize;
            Font font = new Font(mainFont.Name, (float)size);

            for (int i = 0; i < gridMain.Columns.Count; i++)
            {
                gridMain.Columns[i].AppearanceCell.Font = font;
            }
            gridMain.Appearance.GroupFooter.Font = font;
            gridMain.Appearance.FooterPanel.Font = font;
            gridMain.AppearancePrint.FooterPanel.Font = font;
            gridMain.AppearancePrint.GroupFooter.Font = font;
            newFont = font;
            size = scale / 100D * originalHeaderSize;
            font = new Font(HeaderFont.Name, (float)size, FontStyle.Bold);
            for (int i = 0; i < gridMain.Columns.Count; i++)
            {
                gridMain.Columns[i].AppearanceHeader.Font = font;
            }
            //gridMain.Appearance.HeaderPanel.Font = font;
            //gridMain.AppearancePrint.HeaderPanel.Font = font;
            gridMain.RefreshData();
            gridMain.RefreshEditor(true);

            dgv.Refresh();
            this.Refresh();
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
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
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
        /****************************************************************************************/
        private void gridMain_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            if (dr == null)
                return;
            string contract = dr["contractNumber"].ObjToString();
            if (!String.IsNullOrWhiteSpace(contract))
            {
                this.Cursor = Cursors.WaitCursor;
                DataTable dt = (DataTable)dgv.DataSource;
                CustomerDetails clientForm = new CustomerDetails(contract);
                clientForm.Show();
                this.Cursor = Cursors.Default;
            }
        }
        /****************************************************************************************/
        private void gridMain_CalcRowHeight(object sender, RowHeightEventArgs e)
        {
            GridView View = sender as GridView;
            if (e.RowHandle >= 0)
            {
                int maxHeight = 0;
                int newHeight = 0;
                string name = "";
                bool doit = false;
                foreach (GridColumn column in gridMain.Columns)
                {
                    doit = true;
                    doit = false;
                    name = column.FieldName.ToUpper();
                    if (name == "C1")
                        doit = true;
                    if (doit)
                    {
                        DataTable dt = (DataTable)dgv.DataSource;
                        int row = gridMain.GetDataSourceRowIndex(e.RowHandle);
                        string data = dt.Rows[row]["C1"].ObjToString().ToUpper();
                        using (RepositoryItemMemoEdit edit = new RepositoryItemMemoEdit())
                        {
                            using (MemoEditViewInfo viewInfo = edit.CreateViewInfo() as MemoEditViewInfo)
                            {
                                viewInfo.EditValue = gridMain.GetRowCellValue(e.RowHandle, column.FieldName);
                                viewInfo.Bounds = new Rectangle(0, 0, column.VisibleWidth, dgv.Height);
                                using (Graphics graphics = dgv.CreateGraphics())
                                using (GraphicsCache cache = new GraphicsCache(graphics))
                                {
                                    viewInfo.CalcViewInfo(graphics);
                                    var height = ((IHeightAdaptable)viewInfo).CalcHeight(cache, column.VisibleWidth);
                                    newHeight = Math.Max(height, maxHeight);
                                    if (newHeight > maxHeight)
                                        maxHeight = newHeight;
                                    if ( data.IndexOf ( "FUNERAL") > 0 )
                                        maxHeight = 35;
                                }
                            }
                        }
                    }
                }

                if (maxHeight > 0)
                    e.RowHeight = maxHeight;
            }
        }
        /****************************************************************************************/
        private void gridMain_RowCellStyle_1(object sender, RowCellStyleEventArgs e)
        {
        }
        /****************************************************************************************/
        private DataTable LoadMainData( string workContract, double startBalance, double ExpectedPayment )
        {
            bool insurance = false;
            string cmd = "Select * from `payments` where `contractNumber` = '" + workContract + "' order by `payDate8` DESC, `tmstamp` DESC;";
            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("num");
            dt.Columns.Add("balance", Type.GetType("System.Double"));
            dt.Columns.Add("debit", Type.GetType("System.Double"));
            dt.Columns.Add("credit", Type.GetType("System.Double"));
            dt.Columns.Add("prince", Type.GetType("System.Double"));
            dt.Columns.Add("nextDueDate");
            dt.Columns.Add("creditBalance", Type.GetType("System.Double"));
            dt.Columns.Add("reason");
            dt.Columns.Add("NumPayments", Type.GetType("System.Double"));
            DateTime dueDate = DateTime.Now;
            DateTime payDate = DateTime.Now;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dueDate = dt.Rows[i]["dueDate8"].ObjToDateTime();
                if (dueDate.Year.ToString("D4").IndexOf("000") >= 0)
                    dt.Rows[i]["dueDate8"] = dt.Rows[i]["payDate8"];
            }

            double sBalance = startBalance;
            string status = "";
            bool deleted = false;
            double NumPayments = 0D;
            double numMonthPaid = 0D;
            double payment = 0D;
            double debit = 0D;
            double credit = 0D;
            double interest = 0D;
            double principal = 0D;
            double balance = 0D;
            string reason = "";
            string edited = "";

            DateTime insPayDate8 = DateTime.Now;
            DateTime insDueDate8 = DateTime.Now;

            DateTime pDate = DateTime.Now;
            DateTime dDate = DateTime.Now;
            for (int i = (dt.Rows.Count - 1); i >= 0; i--)
            {
                payment = dt.Rows[i]["paymentAmount"].ObjToString().ObjToDouble();
                debit = dt.Rows[i]["debitAdjustment"].ObjToDouble();
                credit = dt.Rows[i]["creditAdjustment"].ObjToDouble();
                if (payment == 0D)
                {
                    if (credit > 0D)
                        payment = credit;
                    else if (debit > 0D)
                        payment = debit;
                }
                NumPayments = 0D;
                if (ExpectedPayment > 0D)
                {
                    NumPayments = payment / ExpectedPayment;
                    if (debit > 0D)
                        NumPayments = NumPayments * -1D;
                }
                dt.Rows[i]["NumPayments"] = NumPayments;
                if (insurance)
                {
                    numMonthPaid = dt.Rows[i]["numMonthPaid"].ObjToDouble();
                    if (numMonthPaid != 0D)
                        dt.Rows[i]["NumPayments"] = numMonthPaid;
                }
                debit = dt.Rows[i]["debitAdjustment"].ObjToString().ObjToDouble();
                credit = dt.Rows[i]["creditAdjustment"].ObjToString().ObjToDouble();
                status = dt.Rows[i]["fill"].ObjToString();
                if (status.ToUpper() == "D")
                    deleted = true;
                //if (payment == 0D && debit == 0D && credit == 0D)
                //    dt.Rows.RemoveAt(i);
            }
            string location = "";
            string userId = "";
            int imonths = 0;
            DateTime lastDueDate = DateTime.Now;
            DateTime nextDueDate = DateTime.Now;
            string depositNumber = "";
            string fill1 = "";

            for (int i = (dt.Rows.Count - 1); i >= 0; i--)
            {
                status = dt.Rows[i]["fill"].ObjToString();
                //if (status.ToUpper() == "D" && !chkLoadAll.Checked)
                //{
                //    dt.Rows.RemoveAt(i);
                //    continue;
                //}
                location = dt.Rows[i]["location"].ObjToString();
                userId = dt.Rows[i]["userId"].ObjToString();
                payment = dt.Rows[i]["paymentAmount"].ObjToString().ObjToDouble();
                debit = dt.Rows[i]["debitAdjustment"].ObjToString().ObjToDouble();
                credit = dt.Rows[i]["creditAdjustment"].ObjToString().ObjToDouble();
                interest = dt.Rows[i]["interestPaid"].ObjToString().ObjToDouble();
                edited = dt.Rows[i]["edited"].ObjToString();
                principal = payment - interest;
                balance = sBalance - principal + debit - credit;
                if (status.ToUpper() == "D")
                    balance = sBalance;
                reason = dt.Rows[i]["debitReason"].ObjToString() + " " + dt.Rows[i]["creditReason"].ObjToString();
                if (edited.ToUpper() == "MANUAL")
                    reason = "* " + userId + " " + reason;
                else if (edited.ToUpper() == "TRUSTADJ")
                    reason = "TA-" + userId + " " + reason;
                else if (edited.ToUpper() == "CEMETERY")
                    reason = "CE-" + userId + " " + reason;
                else
                {
                    depositNumber = dt.Rows[i]["depositNumber"].ObjToString();
                    fill1 = dt.Rows[i]["fill1"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(depositNumber))
                    {
                        if (depositNumber.Substring(0, 1).ToUpper() == "T")
                        {
                            if (fill1.ToUpper() == "TFBX")
                                reason = "TFBX-" + userId;
                            else
                                reason = "LKBX-" + userId;
                        }
                        else if (depositNumber.Substring(0, 1).ToUpper() == "A")
                            reason = "ACH-" + userId;
                    }
                }
                dt.Rows[i]["balance"] = balance;
                dt.Rows[i]["prince"] = G1.RoundValue(principal);
                dt.Rows[i]["debit"] = debit;
                dt.Rows[i]["credit"] = credit;
                dt.Rows[i]["reason"] = reason.Trim();
                sBalance = balance;
            }

            G1.NumberDataTable(dt);
            return dt;
        }
        /****************************************************************************************/
        private void gridMain_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
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
        }
        /****************************************************************************************/
        private void fixMarkedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            int[] rows = gridMain.GetSelectedRows();
            int lastRow = dt.Rows.Count;
            lastRow = rows.Length;

            barImport.Maximum = lastRow;
            barImport.Minimum = 0;
            barImport.Value = 0;
            barImport.Refresh();

            int count = 0;
            int row = 0;
            DataRow dr = null;
            string contractNumber = "";
            string record = "";
            DataTable dx = null;

            string cmd = "";
            DateTime dueDate = DateTime.Now;

            for (int i = 0; i < lastRow; i++)
            {
                Application.DoEvents();
                try
                {
                    barImport.Value = i + 1;
                    barImport.Refresh();

                    row = rows[i];
                    row = gridMain.GetDataSourceRowIndex(row);

                    dr = dt.Rows[row];
                    contractNumber = dr["contractNumber"].ObjToString();
                    dueDate = dr["correctDueDate"].ObjToDateTime();

                    cmd = "Select * from `contracts` where `contractNumber` = '" + contractNumber + "';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count <= 0)
                        continue;

                    record = dx.Rows[0]["record"].ObjToString();

                    G1.update_db_table("contracts", "record", record, new string[] { "dueDate8", dueDate.ToString("yyyy-MM-dd")});

                    count++;
                }
                catch (Exception ex)
                {
                }
            }
            barImport.Value = lastRow;
            barImport.Refresh();
        }
        /****************************************************************************************/
    }
}