using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Text;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraPrinting;
using DevExpress.Utils;
using MySql.Data.MySqlClient;
using GeneralLib;
using DevExpress.XtraRichEdit.API.Native;
using DevExpress.XtraReports.UI;
using DevExpress.XtraReports.UserDesigner;
using System.Net.Mail;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using DevExpress.Xpo.Helpers;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class PastDue : DevExpress.XtraEditors.XtraForm
    {
        /****************************************************************************************/
        private bool autoRun = false;
        private bool autoForce = false;
        private string sendTo = "";
        private string sendWhere = "";
        private string emailLocations = "";
        /****************************************************************************************/
        private bool first = true;
        private bool continuousPrint = false;
        private string workTitle = "";
        private bool workInsurance = false;
        private string paymentsFile = "payments";
        private string contractsFile = "contracts";
        private string customersFile = "customers";
        private DevExpress.XtraRichEdit.RichEditControl rtb2 = new DevExpress.XtraRichEdit.RichEditControl();
        /****************************************************************************************/
        public PastDue(string title = "")
        {
            InitializeComponent();
            workTitle = title;
            if (String.IsNullOrWhiteSpace(workTitle))
                workTitle = "Trust Lapse List (4.0)";
        }
        /****************************************************************************************/
        public PastDue(bool auto, bool force, string title = "")
        {
            autoRun = auto;
            autoForce = force;
            InitializeComponent();
            //            MessageBox.Show("*** Running Past Due ***");
            workTitle = title;
            if (String.IsNullOrWhiteSpace(workTitle))
                workTitle = "Trust Lapse List (4.0)";
            RunAutoReports();
        }
        /****************************************************************************************/
        private void RunAutoReports()
        {
            string cmd = "Select * from `remote_processing`;";
            DataTable dt = G1.get_db_data(cmd);
            string report = "";
            DateTime date = DateTime.Now;
            long currentDay = G1.date_to_days(date.ToString("MM/dd/yyyy"));
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
                if (report.ToUpper() == "POTENTIAL LAPSE")
                    PastDue_Load(null, null);
            }
        }
        /****************************************************************************************/
        private void PastDue_Load(object sender, EventArgs e)
        {
            workInsurance = false;
            if (workTitle.ToUpper().IndexOf("INSURANCE") == 0)
            {
                workInsurance = true;
                paymentsFile = "ipayments";
                customersFile = "icustomers";
                contractsFile = "icontracts";
            }
            gridMain.OptionsView.ShowFooter = false;
            SetupTotalsSummary();
            LoadData();
            this.Text = workTitle;
            if (workTitle.ToUpper().IndexOf("POTENTIAL") >= 0)
            {
                gridMain.OptionsView.ShowFooter = true;
                this.txtPastDue.Text = "30";
                SetPotentialLapse();
            }
            else
            {
                this.txtPastDue.Text = "60";
                SetLapse();
            }
            if (autoRun)
            {
                btnRun_Click(null, null);
                DataTable dt = (DataTable)dgv.DataSource;
                DataView tempview = dt.DefaultView;
                tempview.Sort = "agentCode";
                dt = tempview.ToTable();
                dgv.DataSource = dt;

                gridMain.Columns["agentCode"].GroupIndex = 0;
                gridMain.OptionsView.ShowFooter = true;
                this.gridMain.ExpandAllGroups();

                emailLocations = DailyHistory.ParseOutLocations(dt);

                printPreviewToolStripMenuItem_Click(null, null);
                this.Close();
            }
            //if ( LoginForm.doLapseReport )
            //{
            //    RunLapseReport();
            //    this.Close();
            //}
        }
        /****************************************************************************************/
        private void SetupTotalsSummary()
        {
            AddSummaryColumn("amtOfMonthlyPayt");
            AddSummaryColumn("balanceDue");
            AddSummaryColumn("totalContract");
            AddSummaryColumn("premiumDue");
        }
        /****************************************************************************************/
        private void AddSummaryColumn(string columnName)
        {
            gridMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            gridMain.Columns[columnName].SummaryItem.DisplayFormat = "${0:0,0.00}";
        }
        /****************************************************************************************/
        private void RunLapseReport()
        {
            btnRun_Click(null, null);
            printToolStripMenuItem_Click(null, null);
            //printPreviewToolStripMenuItem_Click(null, null);
        }
        /****************************************************************************************/
        private void LoadData()
        {
            G1.SetupVisibleColumns(gridMain, this.columnsToolStripMenuItem, nmenu_Click);
            string cmd = "Select * from `payments` where `contractNumber` = 'XYZZYAAA';";
            DataTable dt = G1.get_db_data(cmd);
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
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
            if (continuousPrint)
                Printer.setupPrinterMargins(0, 0, 0, 0);

            pageMarginLeft = Printer.pageMarginLeft;
            pageMarginRight = Printer.pageMarginRight;
            pageMarginTop = Printer.pageMarginTop;
            pageMarginBottom = Printer.pageMarginBottom;


            printableComponentLink1.Margins.Left = pageMarginLeft;
            printableComponentLink1.Margins.Right = pageMarginRight;
            printableComponentLink1.Margins.Top = pageMarginTop;
            printableComponentLink1.Margins.Bottom = pageMarginBottom;
            if (continuousPrint)
            {
                printableComponentLink1.MinMargins.Left = pageMarginLeft;
                printableComponentLink1.MinMargins.Right = pageMarginRight;
                printableComponentLink1.MinMargins.Top = pageMarginTop;
                printableComponentLink1.MinMargins.Bottom = pageMarginBottom;
            }

            printingSystem1.Document.AutoFitToPagesWidth = 1;

            G1.AdjustColumnWidths(gridMain, 0.65D, true);

            printableComponentLink1.CreateDocument();

            if (autoRun)
            {
                string path = G1.GetReportPath();
                DateTime today = DateTime.Now;

                string filename = path + @"\POTENTIAL_LAPSE_" + today.Year.ToString("D4") + today.Month.ToString("D2") + today.Day.ToString("D2") + ".pdf";
                if (File.Exists(filename))
                    File.Delete(filename);
                printableComponentLink1.ExportToPdf(filename);
                RemoteProcessing.AutoRunSendTo("Potential Lapse Report", filename, sendTo, sendWhere, emailLocations);
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

            printingSystem1.Document.AutoFitToPagesWidth = 1;

            G1.AdjustColumnWidths(gridMain, 0.65D, true);

            printableComponentLink1.CreateDocument();
            if (LoginForm.doLapseReport)
                printableComponentLink1.Print();
            else
                printableComponentLink1.PrintDlg();

            G1.AdjustColumnWidths(gridMain, 0.65D, false);
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
            string title = "Past Due Report";
            if (!String.IsNullOrWhiteSpace(this.Text))
                title = this.Text;
            Printer.DrawQuad(6, 8, 4, 4, title, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);


            //            Printer.DrawQuadTicks();
            Printer.SetQuadSize(24, 12);
            font = new Font("Ariel", 7, FontStyle.Regular);
            //Printer.DrawQuad(16, 7, 5, 2, lblBalance.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Top);
            //Printer.DrawQuad(16, 10, 5, 2, lblPayment.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            font = new Font("Ariel", 8);
            string search = "Agents : All";
            if (!String.IsNullOrWhiteSpace(chkComboAgent.Text))
                search = "Agents : " + chkComboAgent.Text;
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
                    if (chkSort.Checked || autoRun)
                        e.PS.InsertPageBreak(e.Y);
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
                    if (contract == "C12032U")
                    {

                    }
                    string days = e.DisplayText.ObjToString().Replace(",", "");
                    int daysLate = days.ObjToInt32();
                    int limit = this.txtPastDue.Text.ObjToInt32();
                    if (limit > 0 && daysLate >= limit)
                    {
                        e.Appearance.BackColor = Color.Red;
                        e.Appearance.ForeColor = Color.Yellow;
                    }
                }
            }
        }
        /****************************************************************************************/
        private void btnRun_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            DateTime date = dateTimePicker1.Value;
            string date1 = G1.DateTimeToSQLDateTime(date);
            int limit = this.txtPastDue.Text.ObjToInt32();
            limit = limit * (-1);
            DateTime ddate = date.AddDays(limit);
            string date2 = G1.DateTimeToSQLDateTime(ddate);
            DateTime now = DateTime.Now;

            DateTime paidout = new DateTime(2039, 12, 31);
            string date3 = G1.DateTimeToSQLDateTime(paidout);

            //            string cmd = "Select * from `payments` where `lastDatePaid8` <= '" + date2 + "';";
            string cmd = "Select * from `contracts` p ";
            cmd += " JOIN `payments` d ON p.`contractNumber` = d.`contractNumber` ";
            cmd += " JOIN `customers` c ON p.`contractNumber` = c.`contractNumber` ";
            if (workInsurance)
            {
                cmd = "Select * from `icontracts` p ";
                cmd += " JOIN `ipayments` d ON p.`contractNumber` = d.`contractNumber` ";
                cmd += " JOIN `icustomers` c ON p.`contractNumber` = c.`contractNumber` ";
                cmd += " where p.`dueDate8` < '" + date2 + "' ";
                cmd += " and `balanceDue` > '0.0' ";
                //            cmd += " and p.`lapsed` <> 'Y' AND p.`deleteFlag` <> 'L' and `lastDatePaid8` <> '0000-00-00' and `amtOfMonthlyPayt` > '0.00' ";
                cmd += " and p.`lapsed` <> 'Y' AND c.`lapsed` <> 'Y' AND p.`deleteFlag` <> 'L' and `lastDatePaid8` <> '0000-00-00' and `amtOfMonthlyPayt` > '0.00' ";
                cmd += " and (p.`deceasedDate` = '0000-00-00' OR p.`deceasedDate` = '0001-01-01' ) ";
                cmd += " and d.`dueDate8` <> '" + date3 + "' ";
            }
            else
            {
                cmd += " where p.`dueDate8` < '" + date2 + "' ";
                cmd += " and `balanceDue` > '0.0' ";
                //            cmd += " and p.`lapsed` <> 'Y' AND p.`deleteFlag` <> 'L' and `lastDatePaid8` <> '0000-00-00' and `amtOfMonthlyPayt` > '0.00' ";
                cmd += " and p.`lapsed` <> 'Y' AND c.`lapsed` <> 'Y' AND p.`deleteFlag` <> 'L' and `lastDatePaid8` <> '0000-00-00' and `amtOfMonthlyPayt` > '0.00' ";
                cmd += " and (p.`deceasedDate` = '0000-00-00' OR p.`deceasedDate` = '0001-01-01' ) ";
                cmd += " and d.`dueDate8` <> '" + date3 + "' ";
            }

            if (first)
            {
                first = false;
                loadAgents(cmd);
            }

            string agents = getAgentQuery();
            if (!String.IsNullOrWhiteSpace(agents))
                cmd += " and " + agents;


            cmd += " GROUP BY d.`contractNumber` ORDER BY p.`dueDate8` ";
            cmd += ";";


            DataTable dt = G1.get_db_data(cmd);

            dt = SMFS.FilterForRiles(dt);

            //Trust85.FindContract(dt, "E13053UI");

            dt.Columns.Add("num");
            dt.Columns.Add("customer");
            dt.Columns.Add("daysLate", Type.GetType("System.Int32"));
            dt.Columns.Add("phone");
            dt.Columns.Add("totalContract", Type.GetType("System.Double"));
            dt.Columns.Add("calcDueDate");
            dt.Columns.Add("premiumDue", Type.GetType("System.Double"));
            dt.Columns.Add("agentName");
            string fname = "";
            string lname = "";
            string name = "";
            string area = "";
            string phone = "";
            double serviceTotal = 0D;
            double merchandiseTotal = 0D;
            double allowMerchandise = 0D;
            double allowInsurance = 0D;
            double downpayment = 0D;
            double totalContract = 0D;

            double totalPayments = 0D;
            double totalBalance = 0D;

            double payment = 0D;
            double balance = 0D;
            string location = "";
            double premium = 0D;
            double months = 0D;
            string agentCode = "";

            DateTimeSpan dateSpan;

            for (int i = 0; i < dt.Rows.Count; i++)
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
                    dateSpan = DateTimeSpan.CompareDates(date, now);
                    months = dateSpan.Months;
                    premium = months * payment;
                    premium += payment; // Add another month
                    dt.Rows[i]["premiumDue"] = premium;
                    agentCode = dt.Rows[i]["agentCode"].ObjToString();
                    dt.Rows[i]["agentName"] = GetAgentName(agentCode);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("***ERROR*** Loading Past Due Contracts! " + ex.Message.ToString());
                }
            }
            //Trust85.FindContract(dt, "HT15024UI");

            //CalcNewDueDate(dt, date2);

            //Trust85.FindContract(dt, "HT15024UI");
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private DataTable agentDt = null;
        private string GetAgentName ( string agentCode)
        {
            if (String.IsNullOrWhiteSpace(agentCode))
                return "";
            string agentName = "";
            if (agentDt == null)
                agentDt = G1.get_db_data("Select * from `agents`;");
            if (agentDt == null)
                return "";
            if (agentDt.Rows.Count <= 0)
                return "";
            DataRow[] dR = agentDt.Select("agentCode='" + agentCode + "'");
            if (dR.Length > 0)
                agentName = dR[0]["lastName"].ObjToString() + ", " + dR[0]["firstName"].ObjToString();
            return agentName;
        }
        /***********************************************************************************************/
        private void CalcNewDueDate(DataTable dt, string date2)
        {
            this.Cursor = Cursors.WaitCursor;
            int lastRow = dt.Rows.Count;
            string str = "";
            double balanceDue = 0D;
            double newBalance = 0D;
            double startBalance = 0D;
            string contract = "";
            int numPayments = 0;
            double payment = 0D;
            double dAPR = 0D;
            DateTime lastDate = DateTime.Now;
            DataTable dx = new DataTable();
            string cmd = "";
            DateTime testDate = new DateTime(2018, 7, 1);
            for (int i = 0; i < lastRow; i++)
            {
                try
                {
                    int row = i;
                    DateTime date = dt.Rows[row]["issueDate8"].ObjToDateTime();
                    balanceDue = dt.Rows[row]["balanceDue"].ObjToDouble();
                    contract = dt.Rows[row]["contractNumber"].ObjToString();
                    payment = dt.Rows[row]["amtOfMonthlyPayt"].ObjToString().ObjToDouble();
                    numPayments = dt.Rows[row]["numberOfPayments"].ObjToString().ObjToInt32();
                    startBalance = DailyHistory.GetFinanceValue(dt.Rows[row]);
                    if (startBalance <= 0D)
                        continue;
                    DateTime iDate = DailyHistory.GetIssueDate(dt.Rows[row]["issueDate8"].ObjToDateTime(), contract, null);
                    string issueDate = iDate.ToString("MM/dd/yyyy");
                    lastDate = issueDate.ObjToDateTime();
                    string apr = dt.Rows[row]["APR"].ObjToString();
                    dAPR = apr.ObjToDouble() / 100.0D;
                    if (contract == "HT15024UI")
                    {

                    }

                    cmd = "Select * from `" + paymentsFile + "` where `contractNumber` = '" + contract + "' order by `paydate8` DESC, `tmstamp` DESC;";
                    dx = G1.get_db_data(cmd);
                    DailyHistory.CalculateNewStuff(dx, dAPR, numPayments, startBalance, lastDate);
                    if (dx.Rows.Count > 0)
                    {
                        iDate = DailyHistory.getNextDueDate(dx, payment, ref newBalance);
                        dt.Rows[row]["calcDueDate"] = iDate.ToString("MM/dd/yyyy");
                        dt.Rows[row]["dueDate8"] = G1.DTtoMySQLDT(iDate);
                        dt.Rows[row]["balanceDue"] = newBalance;
                        //for (int j = 0; j < dx.Rows.Count; j++)
                        //{
                        //    if (dx.Rows[j]["fill"].ObjToString().ToUpper() == "D")
                        //        continue;
                        //    newBalance = dx.Rows[j]["newbalance"].ObjToDouble();
                        //    newBalance = dx.Rows[j]["balance"].ObjToDouble();
                        //    iDate = dx.Rows[j]["nextDueDate"].ObjToDateTime();
                        //    iDate = dx.Rows[j]["dueDate8"].ObjToDateTime();
                        //    dt.Rows[row]["calcDueDate"] = iDate.ToString("MM/dd/yyyy");
                        //    dt.Rows[row]["dueDate8"] = G1.DTtoMySQLDT(iDate);
                        //    dt.Rows[row]["balanceDue"] = newBalance;
                        //    break;
                        //}
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("***ERROR*** Customer i=" + i.ToString() + " " + ex.Message.ToString());
                }
            }
            DateTime newDate = date2.ObjToDateTime();
            for (int i = (dt.Rows.Count - 1); i >= 0; i--)
            {
                contract = dt.Rows[i]["contractNumber"].ObjToString();
                if (contract == "HT15024UI")
                {

                }
                newBalance = dt.Rows[i]["balanceDue"].ObjToDouble();
                if (newBalance <= 0D)
                {
                    dt.Rows.RemoveAt(i);
                    continue;
                }
                lastDate = dt.Rows[i]["dueDate8"].ObjToDateTime();
                if (lastDate > newDate)
                {
                    dt.Rows.RemoveAt(i);
                }
            }
            dgv.DataSource = dt;
            dgv.Refresh();
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
        /***********************************************************************************************/
        private void loadAgents(string cmd)
        {
            if (workInsurance)
                return;
            string agent = "";
            //            cmd += " GROUP by `agentNumber` order by `agentNumber`;";
            cmd += " GROUP by `agentCode` order by `agentCode`;";
            DataTable _agentList = G1.get_db_data(cmd);
            chkComboAgent.Properties.DataSource = _agentList;
        }
        /*******************************************************************************************/
        private string getAgentQuery()
        {
            string procLoc = "";
            string[] locIDs = this.chkComboAgent.EditValue.ToString().Split('|');
            for (int i = 0; i < locIDs.Length; i++)
            {
                if (!String.IsNullOrWhiteSpace(locIDs[i]))
                {
                    if (procLoc.Trim().Length > 0)
                        procLoc += ",";
                    procLoc += "'" + locIDs[i].Trim() + "'";
                }
            }
            return procLoc.Length > 0 ? " `agentCode` IN (" + procLoc + ") " : "";
        }
        /****************************************************************************************/
        private void chkComboAgent_EditValueChanged(object sender, EventArgs e)
        {
            btnRun_Click(null, null);
        }
        /****************************************************************************************/
        private void ChangeAsOfDate()
        {
            DateTime now = this.dateTimePicker1.Value;
            int limit = this.txtPastDue.Text.ObjToInt32();
            if (limit > 0)
            {
                now = now.AddDays(limit);
                this.dateTimePickerAsOf.Value = now;
            }
        }
        /****************************************************************************************/
        private void btnLeft_Click(object sender, EventArgs e)
        {
            DateTime now = this.dateTimePicker1.Value;
            DateTime date = new DateTime(now.Year, now.Month, 1);
            date = date.AddMonths(-1);
            this.dateTimePicker1.Value = date;
            ChangeAsOfDate();
        }
        /****************************************************************************************/
        private void btnRight_Click(object sender, EventArgs e)
        {
            DateTime now = this.dateTimePicker1.Value;
            DateTime date = new DateTime(now.Year, now.Month, 1);
            date = date.AddMonths(1);
            this.dateTimePicker1.Value = date;
            ChangeAsOfDate();
        }
        /****************************************************************************************/
        private void commissionReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
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
                //DailyHistory dailyForm = new DailyHistory(contract);
                //dailyForm.Show();
                this.Cursor = Cursors.Default;
            }
        }
        /****************************************************************************************/
        private void btnMark_Click(object sender, EventArgs e)
        {
            int days = txtPastDue.Text.ObjToInt32();
            if (days <= 0)
            {
                MessageBox.Show("***ERROR*** Days Late must be greater than zero!");
                return;
            }
            DialogResult result = MessageBox.Show("Are you sure you want to MARK customers over " + days.ToString() + " days late as Lapsed???", "Mark Lapse Accounts Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
            if (result == DialogResult.No)
                return;
            string contractNumber = "";
            double minimumBalance = txtBalance.Text.ObjToDouble();
            double balanceDue = 0D;
            string record = "";
            int daysLate = 0;
            DateTime today = this.dateTimePickerAsOf.Value;
            string lapseDate = today.ToString("yyyy-MM-dd");
            DataTable dt = (DataTable)dgv.DataSource;
            int lastRow = dt.Rows.Count;
            //            lastRow = 1; //For Debug
            for (int i = 0; i < lastRow; i++)
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                daysLate = dt.Rows[i]["daysLate"].ObjToInt32();
                balanceDue = dt.Rows[i]["balanceDue"].ObjToDouble();
                if (daysLate >= days && balanceDue >= minimumBalance)
                {
                    record = dt.Rows[i]["record"].ObjToString();
                    G1.update_db_table(contractsFile, "record", record, new string[] { "lapsed", "Y", "lapseDate8", lapseDate });
                    record = dt.Rows[i]["record2"].ObjToString();
                    G1.update_db_table(customersFile, "record", record, new string[] { "lapsed", "Y" });
                    G1.AddToAudit(LoginForm.username, "PastDue", "Lapse", "Set", contractNumber);
                }
            }
        }
        /****************************************************************************************/
        private void markCustomerAsLapsedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            string contract = dr["contractNumber"].ObjToString();
            int days = txtPastDue.Text.ObjToInt32();
            if (days <= 0)
            {
                MessageBox.Show("***ERROR*** Days Late must be greater than zero!");
                return;
            }
            DialogResult result = MessageBox.Show("Are you sure you want to MARK customer (" + contract + ") over " + days.ToString() + " days late as Lapsed???", "Mark Lapse Accounts Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
            if (result == DialogResult.No)
                return;
            string record = "";
            int daysLate = 0;
            DateTime today = this.dateTimePickerAsOf.Value;
            string lapseDate = today.ToString("yyyy-MM-dd");
            daysLate = dr["daysLate"].ObjToInt32();
            if (daysLate >= days)
            {
                record = dr["record"].ObjToString();
                G1.update_db_table(contractsFile, "record", record, new string[] { "lapsed", "Y", "lapseDate8", lapseDate });
                record = dr["record2"].ObjToString();
                G1.update_db_table(customersFile, "record", record, new string[] { "lapsed", "Y" });
                G1.AddToAudit(LoginForm.username, "PastDue", "Lapse", "Set", contract);
                DataTable dt = (DataTable)dgv.DataSource;
                if (row >= 0 && row < dt.Rows.Count)
                {
                    dt.Rows.RemoveAt(row);
                    dgv.DataSource = dt;
                    dgv.RefreshDataSource();
                    dgv.Refresh();
                    this.Cursor = Cursors.WaitCursor;
                    ReinstateReport report = new ReinstateReport(contract, true);
                    report.Show();
                    this.Cursor = Cursors.Default;
                }
            }
            else
            {
                MessageBox.Show("***ERROR*** This customer is not more than " + days.ToString() + " late!");
            }
        }
        /****************************************************************************************/
        private void txtBalance_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string balance = txtBalance.Text.Trim();
                if (!G1.validate_numeric(balance))
                {
                    MessageBox.Show("***ERROR*** Minimum Balance must be numeric!");
                    return;
                }
                double money = balance.ObjToDouble();
                balance = G1.ReformatMoney(money);
                txtBalance.Text = balance;
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
        private int GetLapseNoticeValues(string option, string defaultAnswer = "")
        {
            int rv = 0;
            if (!String.IsNullOrWhiteSpace(defaultAnswer))
            {
                if (G1.validate_numeric(defaultAnswer))
                    rv = defaultAnswer.ObjToInt32();
            }
            string answer = AdminOptions.GetOptionAnswer(option);
            if (!String.IsNullOrWhiteSpace(answer))
            {
                if (G1.validate_numeric(answer))
                    rv = answer.ObjToInt32();
            }
            return rv;
        }
        /****************************************************************************************/
        private void GenerateNotices(DataTable dt)
        {
            this.Cursor = Cursors.WaitCursor;
            int limit = this.txtPastDue.Text.ObjToInt32();
            int count = 0;
            int daysLate = 0;
            string contract = "";
            string miniContract = "";
            string trust = "";
            string loc = "";
            string line = "";
            string name = "";
            string address = "";
            string address2 = "";
            string city = "";
            string state = "";
            string zip = "";
            string zip2 = "";
            double payment = 0D;
            double balanceDue = 0D;
            string money = "";

            string POBox = "";
            string POCity = "";
            string POState = "";
            string POZip = "";
            string funeralPhoneNumber = "";
            string manager = "";
            string signer = "";

            RichTextBox rtb3 = new RichTextBox();
            rtb3.Font = new Font("Lucida Console", 9);

            int padright = GetLapseNoticeValues("Lapse Notices Left Side Width", "30");
            int padTop = GetLapseNoticeValues("Lapse Notices Top Border Lines");
            int padLeft = GetLapseNoticeValues("Lapse Notices Left Border Spaces");
            int padBottom = GetLapseNoticeValues("Lapse Notices Bottom Border Lines");
            int padToCustomer = GetLapseNoticeValues("Lapse Notices Lines Prior to Customer");
            int tof = GetLapseNoticeValues("TOF after X Notices");

            int newpadLeft = 0;
            int newpadDown = 1;
            int newskipRight = 2;
            padright = padright + newskipRight;

            DateTime lapseDate = this.dateTimePickerAsOf.Value;

            rtb2.Document.Text = "";
            int noticeCount = 0;
            bool foundPO = false;
            string contractList = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                daysLate = dt.Rows[i]["daysLate"].ObjToInt32();
                if (limit > 0 && daysLate >= limit)
                {
                    contract = dt.Rows[i]["contractNumber"].ObjToString();
                    contractList += contract + ",";
                    balanceDue = dt.Rows[i]["balanceDue"].ObjToDouble();
                    balanceDue = dt.Rows[i]["premiumDue"].ObjToDouble();
                    payment = dt.Rows[i]["amtOfMonthlyPayt"].ObjToDouble();
                    miniContract = Trust85.decodeContractNumber(contract, ref trust, ref loc);
                    LocateFuneralHome(loc, ref name, ref address, ref city, ref state, ref zip, ref POBox, ref POCity, ref POState, ref POZip, ref funeralPhoneNumber, ref manager, ref signer );
                    if (!String.IsNullOrWhiteSpace(POBox))
                    {
                        foundPO = false;
                        if (POBox.ToUpper().IndexOf("PO") >= 0)
                            foundPO = true;
                        else if (POBox.ToUpper().IndexOf("P.O.") >= 0)
                            foundPO = true;
                        if (!foundPO)
                            POBox = "P.O. Box " + POBox;
                    }

                    for (int j = 0; j < padTop; j++)
                        rtb3.AppendText("\n");

                    if (name.Length > (padright - 1))
                        name = name.Substring(0, (padright - 1));
                    line = name.ToUpper().PadRight(padright);
                    line += "OUR RECORDS SHOW\n";
                    if (padLeft > 0)
                        line = "".PadLeft(padLeft) + line;
                    rtb3.AppendText(line);

                    line = address.ToUpper().PadRight(padright);
                    line += "YOUR ACCOUNT IS\n";
                    if (padLeft > 0)
                        line = "".PadLeft(padLeft) + line;
                    rtb3.AppendText(line);

                    line = (city.ToUpper() + " " + state.ToUpper() + " " + zip).PadRight(padright);
                    line += "AT LEAST 60 DAYS\n";
                    if (padLeft > 0)
                        line = "".PadLeft(padLeft) + line;
                    rtb3.AppendText(line);

                    bool doPOBox = false;
                    if (!String.IsNullOrWhiteSpace(POBox))
                    {
                        if (POBox != address)
                            doPOBox = true;
                    }

                    if (doPOBox)
                    {
                        line = (POBox).PadRight(padright);
                        line += "PAST DUE. YOUR\n";
                        if (padLeft > 0)
                            line = "".PadLeft(padLeft) + line;

                    }
                    else
                    {
                        line = "".PadRight(padright);
                        line += "PAST DUE. YOUR\n";
                        if (padLeft > 0)
                            line = "".PadLeft(padLeft) + line;
                    }
                    rtb3.AppendText(line);

                    if (doPOBox)
                    {
                        line = (POCity.ToUpper() + " " + POState.ToUpper() + " " + POZip).PadRight(padright);
                        line += "CONTRACT HAS LAPSED\n";
                        if (padLeft > 0)
                            line = "".PadLeft(padLeft) + line;
                    }
                    else
                    {
                        line = "".PadRight(padright);
                        line += "CONTRACT HAS LAPSED\n";
                        if (padLeft > 0)
                            line = "".PadLeft(padLeft) + line;
                    }
                    rtb3.AppendText(line);

                    line = ("ACCNT# " + contract).PadRight(padright);
                    line += "AS OF " + dt.Rows[i]["dueDate8"].ObjToDateTime().ToString("MM/dd/yyyy") + "\n";
                    if (padLeft > 0)
                        line = "".PadLeft(padLeft) + line;
                    rtb3.AppendText(line);

                    money = G1.ReformatMoney(payment).Trim();
                    money = "$" + money;
                    money = money.PadLeft(9);
                    line = "DUE MONTHLY    " + money;
                    line = line.PadRight(padright);
                    line += "IF YOU FEEL THERE IS AN ERROR,\n";
                    if (padLeft > 0)
                        line = "".PadLeft(padLeft) + line;
                    rtb3.AppendText(line);

                    money = G1.ReformatMoney(balanceDue).Trim();
                    money = "$" + money;
                    money = money.PadLeft(9);
                    line = "TOTAL DUE      " + money;
                    line = line.PadRight(padright);
                    line += "PLEASE NOTIFY US.\n";
                    if (padLeft > 0)
                        line = "".PadLeft(padLeft) + line;
                    rtb3.AppendText(line);

                    //line = "DATE OF NOTICE " + DateTime.Now.ToString("MM/dd/yyyy") + "\n";
                    line = "LAPSE DATE   " + dt.Rows[i]["dueDate8"].ObjToDateTime().ToString("MM/dd/yyyy") + "\n";
                    //line = "LAPSE DATE   " + lapseDate.ToString("MM/dd/yyyy") + "\n";
                    if (padLeft > 0)
                        line = "".PadLeft(padLeft) + line;
                    rtb3.AppendText(line);

                    if (padToCustomer > 0)
                    {
                        for (int j = 0; j < padToCustomer + newpadDown; j++)
                            rtb3.AppendText("\n");
                    }

                    newpadLeft = padLeft - 8;

                    name = dt.Rows[i]["firstName1"].ObjToString() + " " + dt.Rows[i]["lastName1"].ObjToString();
                    line = " ".PadRight(padright) + name + "\n";
                    if (padLeft > 0)
                        line = "".PadLeft(newpadLeft) + line;
                    rtb3.AppendText(line);

                    address = dt.Rows[i]["address1"].ObjToString();
                    //if (!String.IsNullOrWhiteSpace(dt.Rows[i]["address2"].ObjToString()))
                    //    address += "  " + dt.Rows[i]["address2"].ObjToString();
                    line = " ".PadRight(padright) + address.ToUpper() + "\n";
                    if (padLeft > 0)
                        line = "".PadLeft(newpadLeft) + line;
                    rtb3.AppendText(line);

                    address2 = dt.Rows[i]["address2"].ObjToString();
                    line = " ".PadRight(padright) + address2.ToUpper() + "\n";
                    if (padLeft > 0)
                        line = "".PadLeft(newpadLeft) + line;
                    rtb3.AppendText(line);

                    zip = dt.Rows[i]["zip1"].ObjToString();
                    zip2 = dt.Rows[i]["zip2"].ObjToString();
                    if (zip2 != "0")
                        zip += "-" + zip2;

                    city = dt.Rows[i]["city"].ObjToString();
                    state = dt.Rows[i]["state"].ObjToString();

                    line = " ".PadRight(padright) + (city.ToUpper() + " " + state.ToUpper() + " " + zip) + "\n";
                    if (padLeft > 0)
                        line = "".PadLeft(newpadLeft) + line;
                    rtb3.AppendText(line);

                    if (padBottom > 0)
                    {
                        for (int j = 0; j < padBottom - newpadDown - 1; j++)
                            rtb3.AppendText("\n");
                    }
                    noticeCount++;
                    if (tof > 0)
                    {
                        if (noticeCount >= tof)
                        {
                            rtb3.AppendText("\f");
                            noticeCount = 0;
                        }
                    }

                    //for (int j = 1; j <= 14; j++)
                    //    rtb3.AppendText("Line " + j.ToString() + "\n");
                    ////rtb3.AppendText("Line 2\n");
                    ////rtb3.AppendText("Line 3\n");
                    ////rtb3.AppendText("Line 4\n");
                    //rtb3.AppendText("______________________________________________________________________________________________________\n");
                    ////rtb3.AppendText("Line 6\n");
                    count++;
                    //if (count > limit)
                    //    break;
                }
            }
            continuousPrint = true;
            rtb2.Document.RtfText = rtb3.Rtf;

            DialogResult result = MessageBox.Show("Do you want to save these notices to the Database?", "Lapse Notices Generated Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                DateTime date = dateTimePickerAsOf.Value;
                string sDate = date.ToString("MM/dd/yyyy");
                string record = "";
                string noticeRecord = SaveToDatabase(rtb3.Rtf, "trust");
                if (!String.IsNullOrWhiteSpace(noticeRecord))
                {
                    contractList = contractList.TrimEnd(',');
                    string[] Lines = contractList.Split(',');
                    for (int i = 0; i < Lines.Length; i++)
                    {
                        contract = Lines[i].Trim();
                        if (!String.IsNullOrWhiteSpace(contract))
                        {
                            record = G1.create_record("lapse_list", "type", "-1");
                            if (!G1.BadRecord("lapse_list", record))
                            {
                                G1.update_db_table("lapse_list", "record", record, new string[] { "contractNumber", contract, "noticeDate", sDate, "type", "trust", "noticeRecord", noticeRecord, "detail", "Lapse Notice" });
                            }
                        }
                    }
                }
            }


            //ViewRTF aForm = new ViewRTF(rtb3.Rtf);
            //aForm.Show();

            this.Cursor = Cursors.Default;

            printPreviewToolStripMenuItem_Click(null, null);
        }
        /****************************************************************************************/
        public static string SaveToDatabase(string rtfText, string type )
        {
            byte[] b = Encoding.UTF8.GetBytes(rtfText);
            string record = G1.create_record("lapse_notices", "type", "-1");
            if (G1.BadRecord("lapse_notices", record))
                return "";

            G1.update_db_table("lapse_notices", "record", record, new string[] { "type", type });

            G1.update_blob("lapse_notices", "record", record, "image", b);
            return record;
        }
    /****************************************************************************************/
        private void printNoticesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            GenerateNotices( dt );
        }
        /****************************************************************************************/
        public static void LocateFuneralHome( string loc, ref string name, ref string address, ref string city, ref string state, ref string zip, ref string POBox, ref string POCity, ref string POState, ref string POZip, ref string phoneNumber, ref string manager, ref string signer )
        {
            name = "";
            address = "";
            city = "";
            state = "";
            zip = "";
            POBox = "";
            POCity = "";
            POState = "";
            POZip = "";
            phoneNumber = "";
            manager = "";
            signer = "";
            int idx = 0;

            DataTable dx = G1.get_db_data("Select * from `funeralhomes` where `keycode` = '" + loc + "';");
            if ( dx.Rows.Count <= 0 )
                dx = G1.get_db_data("Select * from `funeralhomes` where `atneedcode` = '" + loc + "';");
            if (loc.ToUpper() == "FF" && dx.Rows.Count > 1)
                idx = 1;
            if ( dx.Rows.Count > 0 )
            {
                name = dx.Rows[idx]["name"].ObjToString();
                address = dx.Rows[idx]["address"].ObjToString();
                city = dx.Rows[idx]["city"].ObjToString();
                state = dx.Rows[idx]["state"].ObjToString();
                zip = dx.Rows[idx]["zip"].ObjToString();

                POBox = dx.Rows[idx]["POBox"].ObjToString();
                POCity = dx.Rows[idx]["POCity"].ObjToString();
                POState = dx.Rows[idx]["POState"].ObjToString();
                POZip = dx.Rows[idx]["POZip"].ObjToString();
                phoneNumber = dx.Rows[idx]["phoneNumber"].ObjToString();

                manager = dx.Rows[idx]["manager"].ObjToString();
                signer = dx.Rows[idx]["signer"].ObjToString();
            }
        }
        /****************************************************************************************/
        private void generateNoticesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            int[] rows = gridMain.GetSelectedRows();
            int firstRow = 0;
            int lastRow = dt.Rows.Count;
            if (rows.Length <= 0)
            {
                GenerateNotices(dt);
                return;
            }
            int row = 0;
            DataTable dx = dt.Clone();
            DataRow dRow = null;
            for (int i = 0; i < rows.Length; i++)
            {
                row = rows[i];
                firstRow = gridMain.GetDataSourceRowIndex(row);
                dRow = dt.Rows[firstRow];
                dx.ImportRow(dRow);
            }
            GenerateNotices(dx);
            dx.Dispose();
        }
        /****************************************************************************************/
        private void chkSort_CheckedChanged(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if ( chkSort.Checked )
            {
                DataView tempview = dt.DefaultView;
                tempview.Sort = "agentCode";
                dt = tempview.ToTable();
                dgv.DataSource = dt;

                gridMain.Columns["agentCode"].GroupIndex = 0;
                gridMain.OptionsView.ShowFooter = true;
                this.gridMain.ExpandAllGroups();
            }
            else
            {
                DataView tempview = dt.DefaultView;
                tempview.Sort = "dueDate8";
                dt = tempview.ToTable();
                dgv.DataSource = dt;

                gridMain.Columns["agentCode"].GroupIndex = -1;
                gridMain.OptionsView.ShowFooter = false;
                gridMain.CollapseAllGroups();
            }
            gridMain.RefreshData();
            dgv.Refresh();
        }
        /****************************************************************************************/
        private void chkCalcBalance_CheckedChanged(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            DataTable dt = (DataTable)dgv.DataSource;

            DateTime date = dateTimePicker1.Value;
            string date1 = G1.DateTimeToSQLDateTime(date);
            int limit = this.txtPastDue.Text.ObjToInt32();
            limit = limit * (-1);
            DateTime ddate = date.AddDays(limit);
            string date2 = G1.DateTimeToSQLDateTime(ddate);
            DateTime now = DateTime.Now;

            for ( int i=0; i<dt.Rows.Count; i++)
            {
                date = dt.Rows[i]["calcDueDate"].ObjToDateTime();
                if (!chkCalcBalance.Checked)
                    date = dt.Rows[i]["dueDate8"].ObjToDateTime();
                TimeSpan ts = now - date;
                dt.Rows[i]["daysLate"] = (int)ts.Days;
            }
            dgv.RefreshDataSource();
            dgv.Refresh();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void ClearAllPositions()
        {
            for (int i = 0; i < gridMain.Columns.Count; i++)
            {
                gridMain.Columns[i].Visible = false;
            }
        }
        /****************************************************************************************/
        private void SetPotentialLapse()
        {
            ClearAllPositions();
            G1.SetColumnPosition(gridMain, "num", 1);
            G1.SetColumnPosition(gridMain, "agentCode", 2);
            G1.SetColumnPosition(gridMain, "agentName", 3);
            G1.SetColumnPosition(gridMain, "contractNumber", 4);
            G1.SetColumnPosition(gridMain, "lastName", 5);
            G1.SetColumnPosition(gridMain, "firstName", 6);
            G1.SetColumnPosition(gridMain, "amtOfMonthlyPayt", 7);
            G1.SetColumnPosition(gridMain, "premiumDue", 8);
            G1.SetColumnPosition(gridMain, "balanceDue", 9);
            G1.SetColumnPosition(gridMain, "dueDate8", 10);
            G1.SetColumnPosition(gridMain, "phone", 11);
            G1.SetColumnPosition(gridMain, "totalContract", 12);
            G1.SetColumnPosition(gridMain, "daysLate", 13);
            //G1.SetColumnPosition(gridMain, "address1", 13);
            //G1.SetColumnPosition(gridMain, "address2", 14);
        }
        /****************************************************************************************/
        private void SetLapse()
        {
            ClearAllPositions();
            G1.SetColumnPosition(gridMain, "num", 1);
            G1.SetColumnPosition(gridMain, "agentCode", 2);
            G1.SetColumnPosition(gridMain, "agentName", 3);
            G1.SetColumnPosition(gridMain, "contractNumber", 4);
            G1.SetColumnPosition(gridMain, "lastName", 5);
            G1.SetColumnPosition(gridMain, "firstName", 6);
            G1.SetColumnPosition(gridMain, "issueDate8", 7);
            G1.SetColumnPosition(gridMain, "amtOfMonthlyPayt", 8);
            G1.SetColumnPosition(gridMain, "premiumDue", 9);
            G1.SetColumnPosition(gridMain, "balanceDue", 10);
            G1.SetColumnPosition(gridMain, "dueDate8", 11);
            G1.SetColumnPosition(gridMain, "phone", 12);
            G1.SetColumnPosition(gridMain, "daysLate", 13);
            //G1.SetColumnPosition(gridMain, "address1", 12);
            //G1.SetColumnPosition(gridMain, "address2", 13);
        }
        /****************************************************************************************/
    }
}