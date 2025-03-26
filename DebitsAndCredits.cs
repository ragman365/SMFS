using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraPrinting;
using DevExpress.Utils;
using MySql.Data.MySqlClient;
using GeneralLib;
using DevExpress.XtraRichEdit.API.Native;
using DevExpress.XtraReports.UI;
using DevExpress.XtraReports.UserDesigner;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class DebitsAndCredits : DevExpress.XtraEditors.XtraForm
    {
        /****************************************************************************************/
        private bool autoRun = false;
        private bool autoForce = false;
        private string sendTo = "";
        private string sendWhere = "";
        /****************************************************************************************/
        private bool loading = true;
        private bool first = true;
        private bool continuousPrint = false;
        private string workTitle = "";
        private bool workInsurance = false;
        private string paymentsFile = "payments";
        private string contractsFile = "contracts";
        private string customersFile = "customers";
        private DevExpress.XtraRichEdit.RichEditControl rtb2 = new DevExpress.XtraRichEdit.RichEditControl();
        /****************************************************************************************/
        public DebitsAndCredits( string title = "")
        {
            InitializeComponent();
            workTitle = title;
            if (String.IsNullOrWhiteSpace(workTitle))
                workTitle = "Weekly Debits and Credits Report";
        }
        /****************************************************************************************/
        public DebitsAndCredits( bool auto, bool force, string title = "")
        {
            autoRun = auto;
            autoForce = force;
            InitializeComponent();
            workTitle = title;
            if (String.IsNullOrWhiteSpace(workTitle))
                workTitle = "Weekly Debits and Credits Report";
            RunAutoReports();
        }
        /****************************************************************************************/
        private void RunAutoReports()
        {
            string cmd = "Select * from `remote_processing`;";
            DataTable dt = G1.get_db_data(cmd);
            string report = "";
            DateTime date = DateTime.Now;
            int presentDay = date.Day;
            int dayToRun = 0;
            string status = "";
            string frequency = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                status = dt.Rows[i]["status"].ObjToString();
                if (status.ToUpper() == "INACTIVE")
                    continue;

                if (!autoForce)
                {
                    dayToRun = dt.Rows[i]["day_to_run"].ObjToInt32();
                    frequency = dt.Rows[i]["dateIncrement"].ObjToString();
                    if (!AutoRunSetup.CheckOkToRun(dayToRun, frequency))
                        return;
                }

                report = dt.Rows[i]["report"].ObjToString();
                sendTo = dt.Rows[i]["sendTo"].ObjToString();
                sendWhere = dt.Rows[i]["sendWhere"].ObjToString();
                if (report.ToUpper() == "DEBITS AND CREDITS REPORT")
                {
                    this.dateTimePicker1.Value = DateTime.Now;
                    this.dateTimePicker2.Value = DateTime.Now;
                    cmbDateType_SelectedIndexChanged(null, null);
                    gridMain.OptionsView.ShowFooter = true;
                    SetupTotalsSummary();
                    loading = false;
                    LoadData();
                    this.Text = workTitle;
                    btnRun_Click(null, null);
                    this.Close();
                }
            }
        }
        /****************************************************************************************/
        private void DebitsAndCredits_Load(object sender, EventArgs e)
        {
            this.dateTimePicker1.Value = DateTime.Now;
            this.dateTimePicker2.Value = DateTime.Now;
            cmbDateType_SelectedIndexChanged(null, null);
            workInsurance = false;
            if (workTitle.ToUpper().IndexOf("INSURANCE") == 0)
            {
                workInsurance = true;
                paymentsFile = "ipayments";
                customersFile = "icustomers";
                contractsFile = "icontracts";
            }
            gridMain.OptionsView.ShowFooter = true;
            //gridMain.Columns["calcDueDate"].Visible = false;
            //gridMain.Columns["location"].Visible = false;
            //gridMain.Columns["balanceDue"].Visible = false;
            SetupTotalsSummary();
            loading = false;
            LoadData();
            this.Text = workTitle;
        }
        /****************************************************************************************/
        private void SetupTotalsSummary()
        {
            //AddSummaryColumn("amtOfMonthlyPayt");
            //AddSummaryColumn("balanceDue");
            AddSummaryColumn("totalContract");
            AddSummaryColumn("creditAdjustment");
            AddSummaryColumn("debitAdjustment");
        }
        /****************************************************************************************/
        private void AddSummaryColumn(string columnName)
        {
            gridMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            gridMain.Columns[columnName].SummaryItem.DisplayFormat = "${0:0,0.00}";
        }
        /****************************************************************************************/
        private void LoadData()
        {
            G1.SetupVisibleColumns(gridMain, this.columnsToolStripMenuItem, nmenu_Click);
            string cmd = "Select * from `payments` where `contractNumber` = 'XYZZYAAA';";
            DataTable dt = G1.get_db_data(cmd);
            G1.NumberDataTable(dt);
//            dgv.DataSource = dt;
        }
        /***********************************************************************************************/
        void nmenu_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menu = (ToolStripMenuItem)sender;
            string name = menu.Name;
            int index = getGridColumnIndex(name);
            if (index < 0)
                return;
            if (menu.Checked)
            {
                menu.Checked = false;
                gridMain.Columns[index].Visible = false;
            }
            else
            {
                menu.Checked = true;
                gridMain.Columns[index].Visible = true;
            }
            gridMain.RefreshData();
            dgv.Refresh();
            ToolStripMenuItem xmenu = this.columnsToolStripMenuItem;
            xmenu.ShowDropDown();
        }
        /***********************************************************************************************/
        private int getGridColumnIndex(string columnName)
        {
            int index = -1;
            for (int i = 0; i < gridMain.Columns.Count; i++)
            {
                string name = gridMain.Columns[i].Name;
                if (name == columnName)
                {
                    index = i;
                    break;
                }
            }
            return index;
        }
        /***********************************************************************************************/
        private int pageMarginLeft = 0;
        private int pageMarginRight = 0;
        private int pageMarginTop = 0;
        private int pageMarginBottom = 0;
        /***********************************************************************************************/
        private void printPreviewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //if (1 == 1)
            //{
            //    DataTable dt = (DataTable)dgv.DataSource;
            //    XtraReport1 xForm = new XtraReport1(dt, dgv);
            //    xForm.ShowPreview();
            //    //                MyReport myForm = new MyReport(dt, dgv);
            //    return;
            //}
            if (this.components == null)
                this.components = new System.ComponentModel.Container();
            DevExpress.XtraPrinting.PrintingSystem printingSystem1 = new DevExpress.XtraPrinting.PrintingSystem(this.components);
            DevExpress.XtraPrinting.PrintableComponentLink printableComponentLink1 = new DevExpress.XtraPrinting.PrintableComponentLink(this.components);

            printingSystem1.Links.AddRange(new object[] {
            printableComponentLink1});

            printableComponentLink1.Component = dgv;
            if (continuousPrint)
            {
                SectionMargins margins = rtb2.Document.Sections[0].Margins;
                margins.Left = 0;
                margins.Right = 0;
                margins.Top = 0;
                margins.Bottom = 0;
                printableComponentLink1.Component = rtb2;
            }

            printableComponentLink1.PrintingSystemBase = printingSystem1;
            printableComponentLink1.CreateDetailHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateDetailHeaderArea);
            printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);
            printableComponentLink1.BeforeCreateAreas += new System.EventHandler(this.printableComponentLink1_BeforeCreateAreas);
            printableComponentLink1.AfterCreateAreas += new System.EventHandler(this.printableComponentLink1_AfterCreateAreas);
            this.gridMain.BeforePrintRow += new DevExpress.XtraGrid.Views.Base.BeforePrintRowEventHandler(this.gridMain_BeforePrintRow);
            this.gridMain.AfterPrintRow += new DevExpress.XtraGrid.Views.Base.AfterPrintRowEventHandler(this.gridMain_AfterPrintRow);

            printableComponentLink1.Landscape = true;

            Printer.setupPrinterMargins(50, 100, 80, 50);
            if ( continuousPrint )
                Printer.setupPrinterMargins(0, 0, 0, 0);

            pageMarginLeft = Printer.pageMarginLeft;
            pageMarginRight = Printer.pageMarginRight;
            pageMarginTop = Printer.pageMarginTop;
            pageMarginBottom = Printer.pageMarginBottom;


            printableComponentLink1.Margins.Left = pageMarginLeft;
            printableComponentLink1.Margins.Right = pageMarginRight;
            printableComponentLink1.Margins.Top = pageMarginTop;
            printableComponentLink1.Margins.Bottom = pageMarginBottom;
            if ( continuousPrint )
            {
                printableComponentLink1.MinMargins.Left = pageMarginLeft;
                printableComponentLink1.MinMargins.Right = pageMarginRight;
                printableComponentLink1.MinMargins.Top = pageMarginTop;
                printableComponentLink1.MinMargins.Bottom = pageMarginBottom;
            }

            G1.AdjustColumnWidths(gridMain, 0.65D, true);

            printableComponentLink1.CreateDocument();
            if (autoRun)
            {
                DataTable dt = (DataTable)dgv.DataSource;
                string emailLocations = DailyHistory.ParseOutLocations(dt);

                string path = G1.GetReportPath();
                DateTime today = DateTime.Now;

                string filename = path + @"\" + workTitle + "_" + today.Year.ToString("D4") + today.Month.ToString("D2") + today.Day.ToString("D2") + ".pdf";
                if (File.Exists(filename))
                    File.Delete(filename);
                printableComponentLink1.ExportToPdf(filename);
                RemoteProcessing.AutoRunSendTo(workTitle, filename, sendTo, sendWhere, emailLocations);
            }
            else
                printableComponentLink1.ShowPreview();

            G1.AdjustColumnWidths(gridMain, 0.65D, false );
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
            this.gridMain.BeforePrintRow += new DevExpress.XtraGrid.Views.Base.BeforePrintRowEventHandler(this.gridMain_BeforePrintRow);
            this.gridMain.AfterPrintRow += new DevExpress.XtraGrid.Views.Base.AfterPrintRowEventHandler(this.gridMain_AfterPrintRow);

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

            printableComponentLink1.CreateDocument();
            if (LoginForm.doLapseReport)
                printableComponentLink1.Print();
            else
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

            font = new Font("Ariel", 6);
            Printer.DrawGridDate(2, 3, 2, 3, Color.Black, BorderSide.None, font);
            Printer.DrawGridPage(11, 3, 2, 3, Color.Black, BorderSide.None, font);

//            Printer.DrawQuad(1, 9, 2, 3, "User : " + LoginForm.username, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);

            font = new Font("Ariel", 10, FontStyle.Regular);
            string title = this.Text;
            if (!String.IsNullOrWhiteSpace(this.Text))
                title = this.Text;
            string startDate = this.dateTimePicker1.Value.ToString("MM/dd/yyyy");
            string stopDate = this.dateTimePicker2.Value.ToString("MM/dd/yyyy");
            title += " (" + startDate + " - " + stopDate + ")";
            Printer.DrawQuad(5, 8, 6, 4, title, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);


            //            Printer.DrawQuadTicks();
            Printer.SetQuadSize(24, 12);
            font = new Font("Ariel", 7, FontStyle.Regular);
            //Printer.DrawQuad(16, 7, 5, 2, lblBalance.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Top);
            //Printer.DrawQuad(16, 10, 5, 2, lblPayment.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            font = new Font("Ariel", 8);
            string search = "Agents : All";
            Printer.DrawQuad(1, 6, 6, 3, search, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Top);
            Printer.DrawQuad(1, 9, 6, 3, "User : " + LoginForm.username, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);


            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /****************************************************************************************/
        private int footerCount = 0;
        private void gridMain_BeforePrintRow(object sender, DevExpress.XtraGrid.Views.Printing.CancelPrintRowEventArgs e)
        {
            if (e.HasFooter)
            {
                footerCount++;
            }
        }
        /****************************************************************************************/
        private void gridMain_AfterPrintRow(object sender, DevExpress.XtraGrid.Views.Printing.PrintRowEventArgs e)
        {
            if (e.HasFooter)
            {
                footerCount++;
                if (footerCount >= 1)
                {
                    footerCount = 0;
                    //if ( chkSort.Checked )
                    //    e.PS.InsertPageBreak(e.Y);
                }
            }
        }
        /****************************************************************************************/
        private void gridMain_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
        {
            if (e.Column.FieldName.ToUpper() == "DUEDATE8")
            {
                if (e.RowHandle >= 0)
                {
                    DateTime date = e.DisplayText.ObjToDateTime();
                    e.DisplayText = date.Month.ToString("D2") + "/" + date.Day.ToString("D2") + "/" + date.Year.ToString("D4");
                }
            }
            else if (e.Column.FieldName.ToUpper() == "PAYDATE8")
            {
                if (e.RowHandle >= 0)
                {
                    DateTime date = e.DisplayText.ObjToDateTime();
                    e.DisplayText = date.Month.ToString("D2") + "/" + date.Day.ToString("D2") + "/" + date.Year.ToString("D4");
                }
            }
            else if (e.Column.FieldName.ToUpper() == "LASTDATEPAID8")
            {
                if (e.RowHandle >= 0)
                {
                    DateTime date = e.DisplayText.ObjToDateTime();
                    e.DisplayText = date.Month.ToString("D2") + "/" + date.Day.ToString("D2") + "/" + date.Year.ToString("D4");
                }
            }
            else if (e.Column.FieldName.ToUpper() == "DAYSLATE")
            {
                if (e.RowHandle >= 0)
                {
                    DataTable dx = (DataTable)dgv.DataSource;
                    string contract = dx.Rows[e.RowHandle]["contractNumber"].ObjToString();
                    if ( contract == "C12032U" )
                    {

                    }
                    string days = e.DisplayText.ObjToString().Replace(",", "");
                    int daysLate = days.ObjToInt32();
                    //int limit = this.txtPastDue.Text.ObjToInt32();
                    //if (limit > 0 && daysLate >= limit)
                    //{
                    //    e.Appearance.BackColor = Color.Red;
                    //    e.Appearance.ForeColor = Color.Yellow;
                    //}
                }
            }
        }
        /****************************************************************************************/
        private void btnRun_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            DateTime date = dateTimePicker1.Value;
            string date1 = G1.DateTimeToSQLDateTime(date);
            date = dateTimePicker2.Value;
            string date2 = G1.DateTimeToSQLDateTime(date);

            DateTime now = DateTime.Now;
            DateTime paidout = new DateTime(2039, 12, 31);
            string date3 = G1.DateTimeToSQLDateTime(paidout);

            string cmd = "Select * from `contracts` p ";
            cmd += " JOIN `payments` d ON p.`contractNumber` = d.`contractNumber` ";
            cmd += " JOIN `customers` c ON p.`contractNumber` = c.`contractNumber` ";
            cmd += " WHERE ( d.`debitAdjustment` <> '0' OR d.`creditAdjustment` <> '0' ) ";
            cmd += " AND d.`payDate8` >= '" + date1 + "' ";
            cmd += " AND d.`payDate8` <= '" + date2 + "' ";
            cmd += " ORDER BY d.`payDate8` ";
            cmd += ";";

            DataTable dt = G1.get_db_data(cmd);

            dt.Columns.Add("num");
            dt.Columns.Add("customer");
            dt.Columns.Add("daysLate", Type.GetType("System.Int32"));
            dt.Columns.Add("phone");
            dt.Columns.Add("totalContract", Type.GetType("System.Double"));
            dt.Columns.Add("calcDueDate");
            dt.Columns.Add("dueDate");
            dt.Columns.Add("issueDate");
            dt.Columns.Add("lastPaidDate");
            dt.Columns.Add("lapseDate");
            dt.Columns.Add("reinstateDate");
            dt.Columns.Add("reason");

            string fname = "";
            string lname = "";
            string name = "";
            string area = "";
            string phone = "";
            double totalContract = 0D;

            double totalPayments = 0D;
            double totalBalance = 0D;

            double payment = 0D;
            double balance = 0D;
            string location = "";
            string str = "";
            string reason = "";

            for ( int i=0; i<dt.Rows.Count; i++)
            {
                try
                {
                    fname = dt.Rows[i]["firstName1"].ObjToString();
                    lname = dt.Rows[i]["lastName1"].ObjToString();
                    name = fname + " " + lname;
                    dt.Rows[i]["customer"] = name;
                    payment = dt.Rows[i]["amtOfMonthlyPayt"].ObjToDouble();
                    totalPayments += payment;
                    balance = dt.Rows[i]["balanceDue"].ObjToDouble();
                    totalBalance += balance;
                    date = dt.Rows[i]["dueDate8"].ObjToDateTime();
                    TimeSpan ts = now - date;
                    dt.Rows[i]["daysLate"] = (int)ts.Days;
                    area = dt.Rows[i]["areaCode"].ObjToString();
                    phone = dt.Rows[i]["phoneNumber"].ObjToString();
                    if (String.IsNullOrWhiteSpace(phone))
                        phone = dt.Rows[i]["phoneNumber1"].ObjToString();
                    else
                        phone = "(" + area + ") " + phone;
                    dt.Rows[i]["phone"] = phone;
                    totalContract = DailyHistory.GetFinanceValue(dt.Rows[i]);
                    dt.Rows[i]["totalContract"] = totalContract;
                    location = dt.Rows[i]["location"].ObjToString();

                    date = dt.Rows[i]["dueDate8"].ObjToDateTime();
                    str = date.ToString("MM/dd/yyyy");
                    dt.Rows[i]["dueDate"] = str;

                    date = dt.Rows[i]["lapseDate8"].ObjToDateTime();
                    if (date.Year > 1900)
                    {
                        str = date.ToString("MM/dd/yyyy");
                        dt.Rows[i]["lapseDate"] = str;
                    }

                    date = dt.Rows[i]["reinstateDate8"].ObjToDateTime();
                    if (date.Year > 1900)
                    {
                        str = date.ToString("MM/dd/yyyy");
                        dt.Rows[i]["reinstateDate"] = str;
                    }

                    date = dt.Rows[i]["lastDatePaid8"].ObjToDateTime();
                    if (date.Year > 1900)
                    {
                        str = date.ToString("MM/dd/yyyy");
                        dt.Rows[i]["lastPaidDate"] = str;
                    }

                    date = dt.Rows[i]["issueDate8"].ObjToDateTime();
                    if (date.Year > 1900)
                    {
                        str = date.ToString("MM/dd/yyyy");
                        dt.Rows[i]["issueDate"] = str;
                    }
                    reason = "";
                    str = dt.Rows[i]["creditReason"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(str))
                        reason = str;
                    else
                    {
                        str = dt.Rows[i]["debitReason"].ObjToString();
                        if (!String.IsNullOrWhiteSpace(str))
                            reason = str;
                    }
                    dt.Rows[i]["reason"] = reason;
                }
                catch ( Exception ex )
                {
                    MessageBox.Show("***ERROR*** Loading Debit and Credit Contracts! " + ex.Message.ToString());
                }
            }
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (gridMain.OptionsFind.AlwaysVisible == true)
                gridMain.OptionsFind.AlwaysVisible = false;
            else
                gridMain.OptionsFind.AlwaysVisible = true;
        }
        /****************************************************************************************/
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        /****************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
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
        private void cmbDateType_SelectedIndexChanged(object sender, EventArgs e)
        {
            string dateType = cmbDateType.Text;
            if (dateType.ToUpper() == "MONTHLY")
            {
                DateTime now = DateTime.Now;
                now = now.AddMonths(-1);
                now = new DateTime(now.Year, now.Month, 1);
                DateTime start = now;
                int days = DateTime.DaysInMonth(now.Year, now.Month);
                DateTime stop = new DateTime(now.Year, now.Month, days);
                this.dateTimePicker1.Value = start;
                this.dateTimePicker2.Value = stop;
                this.Text = this.Text.Replace("Weekly", "Monthly");
            }
            else
            {
                this.Text = this.Text.Replace("Monthly", "Weekly");
                DateTime now = this.dateTimePicker2.Value;
                for (;;)
                {
                    if (now.DayOfWeek == DayOfWeek.Friday)
                    {
                        this.dateTimePicker2.Value = now;
                        this.dateTimePicker1.Value = now.AddDays(-6);
                        break;
                    }
                    now = now.AddDays(-1);
                }
            }
            this.Refresh();
        }
        /***********************************************************************************************/
        private void GetWeeklyDate(DateTime date, string direction)
        {
            loading = true;
            DateTime idate = date;
            if (direction == "BACK")
            {
                date = date.AddDays(-7);
                this.dateTimePicker2.Value = date;
                date = date.AddDays(-6);
                this.dateTimePicker1.Value = date;
            }
            else
            {
                date = date.AddDays(7);
                this.dateTimePicker2.Value = date;
                date = date.AddDays(-6);
                this.dateTimePicker1.Value = date;

            }
            loading = false;
        }
        /****************************************************************************************/
        private void btnLeft_Click(object sender, EventArgs e)
        {
            DateTime now = this.dateTimePicker2.Value;
            if (cmbDateType.Text.ToUpper() == "WEEKLY")
            {
                GetWeeklyDate(this.dateTimePicker2.Value, "BACK");
                return;
            }
            else if (cmbDateType.Text.ToUpper() == "DAILY")
            {
                now = now.AddDays(-1);
                this.dateTimePicker1.Value = now;
                this.dateTimePicker2.Value = now;
                return;
            }
            now = now.AddMonths(-1);
            now = new DateTime(now.Year, now.Month, 1);
            DateTime start = now;
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            DateTime stop = new DateTime(now.Year, now.Month, days);
            this.dateTimePicker1.Value = start;
            this.dateTimePicker2.Value = stop;
        }
        /****************************************************************************************/
        private void btnRight_Click(object sender, EventArgs e)
        {
            DateTime now = this.dateTimePicker2.Value;
            if (cmbDateType.Text.ToUpper() == "WEEKLY")
            {
                GetWeeklyDate(this.dateTimePicker2.Value, "FORWARD");
                return;
            }
            else if (cmbDateType.Text.ToUpper() == "DAILY")
            {
                now = now.AddDays(1);
                this.dateTimePicker1.Value = now;
                this.dateTimePicker2.Value = now;
                return;
            }
            now = now.AddMonths(1);
            now = new DateTime(now.Year, now.Month, 1);
            DateTime start = now;
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            DateTime stop = new DateTime(now.Year, now.Month, days);
            this.dateTimePicker1.Value = start;
            this.dateTimePicker2.Value = stop;
        }
        /****************************************************************************************/
    }
}