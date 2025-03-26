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
using DevExpress.XtraGrid.Views.Base;
using DevExpress.Utils.Extensions;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class PastDueInsurance : DevExpress.XtraEditors.XtraForm
    {
        /****************************************************************************************/
        private bool first = true;
        private bool continuousPrint = false;
        private string workTitle = "";
        private bool workPotential = false;
        private DevExpress.XtraRichEdit.RichEditControl rtb2 = new DevExpress.XtraRichEdit.RichEditControl();
        private string contractsFile = "icontracts";
        private string customersFile = "icustomers";
        private string paymentsFile = "ipayments";
        /****************************************************************************************/
        public PastDueInsurance( string title = "")
        {
            InitializeComponent();
            workTitle = title;
            if (String.IsNullOrWhiteSpace(workTitle))
                workTitle = "Trust Lapse List (4.0)";
        }
        /****************************************************************************************/
        private void PastDueInsurance_Load(object sender, EventArgs e)
        {
            gridMain.OptionsView.ShowFooter = true;
            chkCollapes.Visible = false;
            barImport.Hide();

            SetupTotalsSummary();
            LoadData();
            this.Text = workTitle;
            workPotential = false;
            if (workTitle.ToUpper().IndexOf("POTENTIAL") >= 0)
            {
                workPotential = true;
                gridMain.OptionsView.ShowFooter = true;
                this.txtPastDue.Text = "30";
                SetPotentialLapse();
            }
            else
            {
                this.txtPastDue.Text = "30";
//                SetLapse();
                SetPotentialLapse();
            }
            if ( LoginForm.doLapseReport )
            {
                RunLapseReport();
                this.Close();
            }

            DateTime now = DateTime.Now;
            now = now.AddMonths(-1);
            now = new DateTime(now.Year, now.Month, 1);
//            this.dateTimePicker1.Value = now;
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            this.dateTimePicker2.Value = new DateTime(now.Year, now.Month, days);
            if (this.dateTimePickerAsOf.Visible)
                this.dateTimePickerAsOf.Value = now;
        }
        /****************************************************************************************/
        private void SetupTotalsSummary()
        {
            AddSummaryColumn("amtOfMonthlyPayt");
            AddSummaryColumn("balanceDue");
            AddSummaryColumn("totalContract");
        }
        /****************************************************************************************/
        private void AddSummaryColumn(string columnName)
        {
            gridMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            gridMain.Columns[columnName].SummaryItem.DisplayFormat = "${0:0,0.00}";
        }
        /****************************************************************************************/
        private void RunLapseReport ()
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
            
            printingSystem1.Document.AutoFitToPagesWidth = 1;

            G1.PrintPreview(printableComponentLink1, gridMain);

            //printableComponentLink1.CreateDocument();
            //printableComponentLink1.ShowPreview();
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

            font = new Font("Ariel", 10, FontStyle.Bold);
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
                    if ( chkSort.Checked )
                        e.PS.InsertPageBreak(e.Y);
                }
            }
        }
        /****************************************************************************************/
        private void gridMain_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
        {
            if (e.Column.FieldName.ToUpper().IndexOf ( "DATE") >= 0 )
            {
                if (e.RowHandle >= 0)
                {
                    DateTime date = e.DisplayText.ObjToDateTime();
                    if (date.Year > 1850)
                        e.DisplayText = date.ToString("MM/dd/yyyy");
                    else
                        e.DisplayText = "";
                }
            }
            if (workPotential)
            {
                if (e.Column.FieldName.ToUpper() == "PAYDATE8")
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
            else if (e.Column.FieldName.ToUpper() == "NUM")
            {
                if (e.RowHandle >= 0)
                {
                    string num = (e.RowHandle + 1).ToString();
                    e.DisplayText = num;
                }
            }
        }
        /****************************************************************************************/
        private void btnRun_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            DateTime date = dateTimePicker2.Value;
            string date1 = G1.DateTimeToSQLDateTime(date);
            string date2 = G1.DateTimeToSQLDateTime(date);

            int limit = this.txtPastDue.Text.ObjToInt32();
            limit = limit * (-1);
            DateTime ddate = date.AddDays(limit);
            date2 = G1.DateTimeToSQLDateTime(ddate);

            //date = dateTimePicker2.Value;
            //string date2 = G1.DateTimeToSQLDateTime(date);

            //if (workPotential )
            //{
            //    int limit = this.txtPastDue.Text.ObjToInt32();
            //    limit = limit * (-1);
            //    DateTime ddate = date.AddDays(limit);
            //    date2 = G1.DateTimeToSQLDateTime(ddate);
            //}

            string contractNumber = "";
            string agentCode = "";
            string agentName = "";
            DateTime now = DateTime.Now;

            DateTime paidout = new DateTime(2039, 12, 31);
            string date3 = G1.DateTimeToSQLDateTime(paidout);

            //            string cmd = "Select * from `payments` where `lastDatePaid8` <= '" + date2 + "';";
            string cmd = "Select * from `icontracts` p ";
            cmd += " JOIN `icustomers` c ON p.`contractNumber` = c.`contractNumber` ";
            cmd += " where p.`dueDate8` < '" + date2 + "' ";
            cmd += " and p.`lapsed` <> 'Y' AND c.`lapsed` <> 'Y' AND p.`deleteFlag` <> 'L' and `lastDatePaid8` <> '0000-00-00' and `amtOfMonthlyPayt` > '0.00' ";
            cmd += " and (p.`deceasedDate` = '0000-00-00' OR p.`deceasedDate` = '0001-01-01' ) ";
            cmd += " and p.`dueDate8` <> '" + date3 + "' ";

            if (!chkOld.Checked)
            {
                cmd = "Select * from `payers` pp ";
                cmd += " JOIN `icontracts` p ON pp.`contractNumber` = p.`contractNumber` ";
                cmd += " JOIN `icustomers` c ON pp.`contractNumber` = c.`contractNumber` ";
                cmd += " where ( pp.`dueDate8` < '" + date2 + "' AND p.`dueDate8` <= pp.`dueDate8` ) ";
//                cmd += " where pp.`dueDate8` < '" + date2 + "' ";
                cmd += " and pp.`lapsed` <> 'Y' AND c.`lapsed` <> 'Y' AND p.`deleteFlag` <> 'L' and pp.`lastDatePaid8` <> '0000-00-00' and pp.`amtOfMonthlyPayt` > '0.00' ";
                cmd += " AND (pp.`deceasedDate` = '0000-00-00' OR pp.`deceasedDate` = '0001-01-01' ) ";
                cmd += " and pp.`dueDate8` <> '" + date3 + "' ";
            }

            btnMark.Text = "Set as Lapse";
            if ( chkShowLapsed.Checked )
            {
                date = dateTimePickerAsOf.Value;
                date2 = G1.DateTimeToSQLDateTime(date);
                cmd = "Select * from `payers` pp ";
                cmd += " JOIN `icontracts` p ON pp.`contractNumber` = p.`contractNumber` ";
                cmd += " JOIN `icustomers` c ON pp.`contractNumber` = c.`contractNumber` ";
                cmd += " where pp.`lapseDate8` = '" + date2 + "' ";
                btnMark.Text = "Reverse Lapses";
            }
            if (first)
            {
                first = false;
                loadAgents(cmd);
            }

            string agents = getAgentQuery();
            if (!String.IsNullOrWhiteSpace(agents))
                cmd += " and " + agents;


            if (workPotential)
                cmd += " GROUP BY d.`contractNumber` ORDER BY p.`dueDate8` ";
            else
                cmd += " ORDER BY `agentCode` ";
            cmd += ";";


            DataTable dt = G1.get_db_data(cmd);
            Trust85.FindContract(dt, "ZZ0012851");
            dt.Columns.Add("num");
            dt.Columns.Add("customer");
            dt.Columns.Add("daysLate", Type.GetType("System.Int32"));
            dt.Columns.Add("phone");
            dt.Columns.Add("agentName");
            dt.Columns.Add("totalContract", Type.GetType("System.Double"));
            dt.Columns.Add("calcDueDate");
            //dt.Columns.Add("SDICode");
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
            int imonths = 0;
            string payerContract = "";
            string lapsed = "";
            string SDICode = "";
            string oldloc = "";
            string funeralHome = "";
            DateTime lapseDate8 = DateTime.Now;
            DateTime reinstateDate8 = DateTime.Now;
            DateTime deceasedDate = DateTime.Now;
            TimeSpan ts;

            DateTime pastDueDate = new DateTime(2020, 7, 1);
            DateTime dueDate8 = DateTime.Now;

            DateTimeSpan dateSpan;

            string payer = "";
            double beginningBalance = 0D;
            double endingBalance = 0D;
            DateTime lastPaidDate = DateTime.Now;
            int daysLate = 0;

            this.Cursor = Cursors.WaitCursor;

            barImport.Minimum = 0;
            barImport.Maximum = dt.Rows.Count;
            barImport.Show();


            for ( int i=0; i<dt.Rows.Count; i++)
            {
                Application.DoEvents();

                barImport.Value = i + 1;
                barImport.Refresh();

                try
                {
                    payer = dt.Rows[i]["payer"].ObjToString();
                    fname = dt.Rows[i]["firstName"].ObjToString();
                    lname = dt.Rows[i]["lastName"].ObjToString();
                    name = fname + " " + lname;
                    dt.Rows[i]["customer"] = name;
                    payment = dt.Rows[i]["amtOfMonthlyPayt"].ObjToDouble();
                    date = dt.Rows[i]["dueDate8"].ObjToDateTime();
                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                    payer = dt.Rows[i]["payer"].ObjToString();
                    if (payment > 500D)
                    {
                        //payment = Policies.CalcMonthlyPremium(contractNumber, "", payment);
                        payment = Policies.CalcMonthlyPremium(payer, pastDueDate);
                        dt.Rows[i]["amtOfMonthlyPayt"] = payment;
                    }
                    else
                    {
//                        payment = Policies.CalcMonthlyPremium(contractNumber, "", payment);
                        payment = Policies.CalcMonthlyPremium(payer, pastDueDate);
                        dt.Rows[i]["amtOfMonthlyPayt"] = payment;
                    }
                    totalPayments += payment;
                    balance = dt.Rows[i]["balanceDue"].ObjToDouble();
                    totalBalance += balance;
                    date = dt.Rows[i]["dueDate8"].ObjToDateTime();
                    ts = now - date;
                    dt.Rows[i]["daysLate"] = (int)ts.Days;
                    area = dt.Rows[i]["areaCode"].ObjToString();
                    phone = dt.Rows[i]["phoneNumber"].ObjToString();
                    if (String.IsNullOrWhiteSpace(phone))
                        phone = dt.Rows[i]["phoneNumber1"].ObjToString();
                    else
                    {
                        if ( phone.IndexOf ( area ) < 0 )
                            phone = "(" + area + ") " + phone;
                    }
                    dt.Rows[i]["phone"] = phone;

                    if ( payer == "UC-86" )
                    {
                    }

                    lastPaidDate = DailyHistory.GetInsuranceLastPaid( payer, ref dueDate8, ref payerContract, ref lapseDate8, ref reinstateDate8, ref lapsed, ref deceasedDate );

                    if (payerContract == "ZZ0012851")
                    {
                    }

                    imonths = G1.GetMonthsBetween( now, dueDate8);

                    dt.Rows[i]["lastDatePaid8"] = G1.DTtoMySQLDT(lastPaidDate.ToString("yyyy-MM-dd"));
                    dt.Rows[i]["dueDate8"] = G1.DTtoMySQLDT(dueDate8.ToString("yyyy-MM-dd"));
                    dt.Rows[i]["contractNumber"] = payerContract;
                    dt.Rows[i]["lapseDate8"] = G1.DTtoMySQLDT(lapseDate8.ToString("yyyy-MM-dd"));
                    dt.Rows[i]["reinstateDate8"] = G1.DTtoMySQLDT(reinstateDate8.ToString("yyyy-MM-dd"));
                    dt.Rows[i]["deceasedDate"] = G1.DTtoMySQLDT(deceasedDate.ToString("yyyy-MM-dd"));
                    dt.Rows[i]["lapsed"] = lapsed;

                    ts = now - dueDate8;
                    dt.Rows[i]["daysLate"] = (int)ts.Days;


                    dateSpan = DateTimeSpan.CompareDates( lastPaidDate, dueDate8 );
                    months = dateSpan.Months;
                    premium = imonths * payment;
                    premium += payment; // Add another month
                    dt.Rows[i]["balanceDue"] = premium;

                    agentCode = dt.Rows[i]["agentCode"].ObjToString();
                    dt.Rows[i]["agentName"] = GetAgentName(agentCode);

                    oldloc = dt.Rows[i]["oldloc1"].ObjToString();
                    SDICode = dt.Rows[i]["SDICode"].ObjToString();
                    if ( String.IsNullOrWhiteSpace ( SDICode ))
                        SDICode = InsuranceCoupons.getSDICode(agentCode, oldloc);
                    dt.Rows[i]["SDICode"] = SDICode;

                }
                catch ( Exception ex )
                {
                    MessageBox.Show("***ERROR*** Loading Past Due Contracts! " + ex.Message.ToString());
                }
            }

            CleanupSecNat(dt);

            if (chkDueDate.Checked)
                CalcNewDueDate2(dt);
            else
                CalcNewDueDate(dt);

            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private DataTable agentDt = null;
        private string GetAgentName(string agentCode)
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
        private DataTable CleanupSecNat(DataTable dt)
        {
            int fCount = dt.Rows.Count;

            DataTable newDt = dt.Clone();
            string cmd = "";
            string contractNumber = "";
            string payer = "";
            string firstName = "";
            string lastName = "";
            DataTable dx = null;
            DataTable testDt = null;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                payer = dt.Rows[i]["payer"].ObjToString();
                if ( payer == "UC-4251A")
                {
                }
                if (payer == "UC-2642")
                {
                }
                firstName = dt.Rows[i]["firstName"].ObjToString();
                lastName = dt.Rows[i]["lastName"].ObjToString();
                //cmd = "Select * from `policies` p where `payer` = '" + payer + "' and `firstName` = '" + firstName + "' AND `lastName` = '" + lastName + "' AND `tmstamp` > '2020-01-01'";
                cmd = "Select * from `policies` p where `payer` = '" + payer + "'";
                cmd += ";";

                dx = G1.get_db_data(cmd);

                testDt = CustomerDetails.filterSecNat(false, dx);
                if (testDt.Rows.Count <= 0)
                    dt.Rows[i]["payer"] = "";
                //G1.HardCopyDtRow(dt, i, newDt, newDt.Rows.Count);
            }
            double premium = 0D;
            for (int i = (dt.Rows.Count - 1); i >= 0; i--)
            {
                payer = dt.Rows[i]["payer"].ObjToString();
                if (String.IsNullOrWhiteSpace(payer))
                    dt.Rows.RemoveAt(i);
                else
                {
                    if (payer == "UC-2642")
                    {
                    }
                    premium = Policies.CalcMonthlyPremium(payer, DateTime.Now);
                    dt.Rows[i]["amtOfMonthlyPayt"] = premium;
                }
            }
            int ncount = dt.Rows.Count;
            return dt;
        }
        /***********************************************************************************************/
        private void CalcNewDueDate2 ( DataTable dt )
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
            double dPayments = 0D;
            DateTime lastDate = DateTime.Now;
            DataTable dx = new DataTable();
            string cmd = "";
            int numberPayments = 0;
            string contractNumber = "";
            string payer = "";
            DateTime testDate = new DateTime(2018, 7, 1);
            for (int j = 0; j < lastRow; j++)
            {
                try
                {
                    int row = j;
                    contractNumber = dt.Rows[row]["contractNumber"].ObjToString();
                    payer = dt.Rows[row]["payer"].ObjToString();
                    DateTime date = dt.Rows[row]["issueDate8"].ObjToDateTime();
                    balanceDue = dt.Rows[row]["balanceDue"].ObjToDouble();
                    contract = dt.Rows[row]["contractNumber"].ObjToString();
                    payment = dt.Rows[row]["amtOfMonthlyPayt"].ObjToString().ObjToDouble();
                    if (payment > 500D)
                        payment = Policies.CalcMonthlyPremium(contractNumber, "", payment);
                    numPayments = dt.Rows[row]["numberOfPayments"].ObjToString().ObjToInt32();

                    //startBalance = DailyHistory.GetFinanceValue(dt.Rows[row]);
                    startBalance = 0D;
                    //if (startBalance <= 0D)
                    //    continue;

                    DateTime iDate = DailyHistory.GetIssueDate(dt.Rows[row]["issueDate8"].ObjToDateTime(), contract, null);
                    string issueDate = iDate.ToString("MM/dd/yyyy");
                    lastDate = issueDate.ObjToDateTime();
                    string apr = dt.Rows[row]["APR"].ObjToString();
                    dAPR = apr.ObjToDouble() / 100.0D;

                    double months = 0D;
                    int imonths = 0;
                    DateTime dueDate8 = DateTime.Now;
                    DateTime datePaid = DateTime.Now;
                    DateTime nextDueDate = DateTime.Now;
                    bool first = true;

                    dx = LoadMainData2(contract, payer, payment);

                    if (contract == "ZZ0000797")
                    {
                    }
                    if ( dx.Rows.Count > 0 )
                    {
                        dueDate8 = dx.Rows[0]["dueDate8"].ObjToDateTime();
                        months = dx.Rows[0]["NumPayments"].ObjToDouble();
                        imonths = Convert.ToInt32(months);
                        dueDate8 = dueDate8.AddMonths(imonths);
                        dt.Rows[row]["dueDate8"] = G1.DTtoMySQLDT(dueDate8);
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show("***ERROR*** Customer i=" + j.ToString() + " " + ex.Message.ToString());
                }
            }
            dgv.DataSource = dt;
            dgv.Refresh();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void CalcNewDueDate ( DataTable dt )
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
            double dPayments = 0D;
            DateTime lastDate = DateTime.Now;
            DataTable dx = new DataTable();
            string cmd = "";
            int numberPayments = 0;
            string contractNumber = "";
            string payer = "";
            DateTime testDate = new DateTime(2018, 7, 1);
            for (int i = 0; i < lastRow; i++)
            {
                try
                {
                    int row = i;
                    contractNumber = dt.Rows[row]["contractNumber"].ObjToString();
                    payer = dt.Rows[row]["payer"].ObjToString();
                    if ( contractNumber == "ZZ0003718")
                    {
                    }
                    DateTime date = dt.Rows[row]["issueDate8"].ObjToDateTime();
                    balanceDue = dt.Rows[row]["balanceDue"].ObjToDouble();
                    contract = dt.Rows[row]["contractNumber"].ObjToString();
                    payment = dt.Rows[row]["amtOfMonthlyPayt"].ObjToString().ObjToDouble();
                    if ( payment > 500D)
                        payment = Policies.CalcMonthlyPremium(contractNumber, "", payment);
                    numPayments = dt.Rows[row]["numberOfPayments"].ObjToString().ObjToInt32();
                    startBalance = DailyHistory.GetFinanceValue(dt.Rows[row]);
                    //if (startBalance <= 0D)
                    //    continue;
                    DateTime iDate = DailyHistory.GetIssueDate(dt.Rows[row]["issueDate8"].ObjToDateTime(), contract, null);
                    string issueDate = iDate.ToString("MM/dd/yyyy");
                    lastDate = issueDate.ObjToDateTime();
                    string apr = dt.Rows[row]["APR"].ObjToString();
                    dAPR = apr.ObjToDouble() / 100.0D;

                    //dx = LoadMainData(contractNumber, payer, payment );
                    ////cmd = "Select * from `ipayments` where `contractNumber` = '" + contract + "' order by `paydate8` DESC, `tmstamp` DESC;";
                    ////dx = G1.get_db_data(cmd);
                    //DailyHistory.CalculateNewStuff(dx, dAPR, numPayments, startBalance, lastDate);
                    //if (dx.Rows.Count > 0)
                    //{
                    //    for (int j = 0; j < dx.Rows.Count; j++)
                    //    {
                    //        if (dx.Rows[j]["fill"].ObjToString().ToUpper() == "D")
                    //            continue;
                    //        newBalance = dx.Rows[j]["newbalance"].ObjToDouble();
                    //        iDate = dx.Rows[j]["nextDueDate"].ObjToDateTime();
                    //        dt.Rows[row]["calcDueDate"] = iDate.ToString("MM/dd/yyyy");
                    //        iDate = dx.Rows[j]["payDate8"].ObjToDateTime();
                    //        if (iDate.Year > 100)
                    //        {
                    //            dt.Rows[row]["lastDatePaid8"] = G1.DTtoMySQLDT(iDate);
                    //            dPayments = dx.Rows[j]["NumPayments"].ObjToDouble();
                    //            str = dPayments.ToString();
                    //            str = G1.TrimDecimals(str);
                    //            numberPayments = str.ObjToInt32();
                    //            lastDate = dt.Rows[row]["dueDate8"].ObjToDateTime();
                    //            iDate = lastDate.AddMonths(numberPayments);
                    //            dt.Rows[row]["dueDate8"] = G1.DTtoMySQLDT(iDate);

                    //            lastDate = this.dateTimePicker2.Value;
                    //            TimeSpan ts = lastDate - iDate;
                    //            dt.Rows[row]["daysLate"] = ts.Days;
                    //        }
                    //        break;
                    //    }
                    //}
                }
                catch (Exception ex)
                {
                    MessageBox.Show("***ERROR*** Customer i=" + i.ToString() + " " + ex.Message.ToString());
                }
            }
            dgv.DataSource = dt;
            dgv.Refresh();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private DataTable LoadMainData2(string workContract, string workPayer, double ExpectedPayment )
        {
            bool insurance = false;
            string cmd = "Select * from `" + paymentsFile + "` where `contractNumber` = '" + workContract + "' order by `payDate8` DESC, `tmstamp` DESC;";
            if (paymentsFile.Trim().ToUpper() == "IPAYMENTS" && !String.IsNullOrWhiteSpace(workPayer))
            {
                insurance = true;
                string ccd = "SELECT * from `icustomers` where `payer`= '" + workPayer + "';";
                DataTable ddx = G1.get_db_data(ccd);
                if (ddx.Rows.Count > 0)
                {
                    string list = "";
                    for (int i = 0; i < ddx.Rows.Count; i++)
                    {
                        string contract = ddx.Rows[i]["contractNumber"].ObjToString();
                        list += "'" + contract + "',";
                    }
                    list = list.TrimEnd(',');
                    list = "(" + list + ")";
                    cmd = "Select * from `" + paymentsFile + "` where `contractNumber` IN " + list + " order by `payDate8` DESC, `tmstamp` DESC;";
                }
            }
            //            string cmd = "Select * from `payments` where `contractNumber` = '" + workContract + "' order by `dueDate8` DESC;";
            //if ( isInsurance ( workContract )) // Orphans
            //{
            //    cmd = "Select * from `icustomers` where `contractNumber` = '" + workContract + "';";
            //    DataTable ddt = G1.get_db_data(cmd);
            //    if ( ddt.Rows.Count > 0 )
            //    {
            //        string payer = ddt.Rows[0]["payer"].ObjToString();
            //        string firstName = ddt.Rows[0]["firstName"].ObjToString();
            //        string lastName = ddt.Rows[0]["lastName"].ObjToString();
            //        cmd = "Select * from `ipayments` where `firstName` = '" + firstName + "' and `lastName` = '" + lastName + "' order by `payDate8` DESC, `tmstamp` DESC;";

            //    }
            //}
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

            //DataView tempview = dt.DefaultView;
            //tempview.Sort = "dueDate8 desc";
            ////            tempview.Sort = "loc asc, agentName asc";
            //dt = tempview.ToTable();

            //Double startBalance = DailyHistory.GetFinanceValue(workContract);

            double startBalance = 0D;

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
                //payment = dt.Rows[i]["paymentAmount"].ObjToString().ObjToDouble();
                payment = DailyHistory.getPayment(dt, i);

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
                    if (!String.IsNullOrWhiteSpace(workPayer))
                    {
                        pDate = dt.Rows[i]["payDate8"].ObjToDateTime();
                        dDate = dt.Rows[i]["dueDate8"].ObjToDateTime();
                        if (pDate > DailyHistory.killSecNatDate)
                            dt.Rows[i]["numMonthPaid"] = 0D;
                        double months = DailyHistory.CheckMonthsForInsuranceNew(workContract, workPayer, ExpectedPayment, payment, pDate, dDate);
                        NumPayments = months;
                        //nextDueDate = dueDate.ObjToDateTime();
                        //int imonths = (int)months;
                        //nextDueDate = nextDueDate.AddMonths(imonths);
                    }
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
            if (DailyHistory.isInsurance(workContract) && dt.Rows.Count > 1)
            {
                bool dateFirst = true;
                for (int i = (dt.Rows.Count - 1); i >= 0; i--)
                {
                    status = dt.Rows[i]["fill"].ObjToString();
                    //if (status.ToUpper() == "D" && !chkLoadAll.Checked)
                    //{
                    //    dt.Rows.RemoveAt(i);
                    //    continue;
                    //}
                    insPayDate8 = dt.Rows[i]["payDate8"].ObjToDateTime();
                    insDueDate8 = dt.Rows[i]["dueDate8"].ObjToDateTime();
                    NumPayments = dt.Rows[i]["NumPayments"].ObjToDouble();
                    if (insPayDate8 == insDueDate8)
                        continue;
                    imonths = NumPayments.ObjToInt32();
                    if (dateFirst)
                    {
                        try
                        {
                            insPayDate8 = dt.Rows[i + 1]["dueDate8"].ObjToDateTime();
                            dateFirst = false;
                            lastDueDate = insPayDate8;
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                    insDueDate8 = lastDueDate.AddMonths(imonths);
                    if (!insurance)
                        dt.Rows[i]["dueDate8"] = G1.DTtoMySQLDT(insDueDate8);
                    lastDueDate = insDueDate8;
                    nextDueDate = lastDueDate.AddMonths(imonths);
                }
                //lblCDD.Text = "CDD " + nextDueDate.ToString("MM/dd/yyyy");
            }
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

                //payment = dt.Rows[i]["paymentAmount"].ObjToString().ObjToDouble();
                payment = DailyHistory.getPayment(dt, i);

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

            double downPay = DailyHistory.GetDownPayment(workContract);

            //DailyHistory.GetTotals(dt, downPay);
            if (DailyHistory.isInsurance(workContract) )
            {
                double months = 0D;
                imonths = 0;
                DateTime dueDate8 = DateTime.Now;
                DateTime datePaid = DateTime.Now;
                bool first = true;

                for (int i = (dt.Rows.Count - 1); i >= 0; i--)
                {
                    datePaid = dt.Rows[i]["payDate8"].ObjToDateTime();
                    if (datePaid < DailyHistory.killSecNatDate)
                    {
                        dueDate8 = dt.Rows[i]["dueDate8"].ObjToDateTime();
                        months = dt.Rows[i]["NumPayments"].ObjToDouble();
                        continue;
                    }
                    if (first)
                    {
                        first = false;
                        imonths = Convert.ToInt32((months));

                        dueDate8 = dueDate8.AddMonths(imonths);
                        dt.Rows[i]["dueDate8"] = G1.DTtoMySQLDT(dueDate8);
                        months = dt.Rows[i]["NumPayments"].ObjToDouble();
                        months = G1.RoundValue(months);
                    }

                    else
                    {
                        imonths = Convert.ToInt32((months));
                        dueDate8 = dueDate8.AddMonths(imonths);

                        dt.Rows[i]["dueDate8"] = G1.DTtoMySQLDT(dueDate8);
                        months = dt.Rows[i]["NumPayments"].ObjToDouble();
                        months = G1.RoundValue(months);
                    }
                }
            }
            if (DailyHistory.isInsurance(workContract))
                DailyHistory.LoadExpectedPremiums(dt, workPayer);
            return dt;
        }
        /****************************************************************************************/
        private DataTable LoadMainData( string workContract, string workPayer, double ExpectedPayment )
        {
            string cmd = "Select * from `ipayments` where `contractNumber` = '" + workContract + "' order by `payDate8` DESC, `tmstamp` DESC;";
            if (paymentsFile.Trim().ToUpper() == "IPAYMENTS" && !String.IsNullOrWhiteSpace(workPayer))
            {
                string ccd = "SELECT * from `icustomers` where `payer`= '" + workPayer + "';";
                DataTable ddx = G1.get_db_data(ccd);
                if (ddx.Rows.Count > 0)
                {
                    string list = "";
                    for (int i = 0; i < ddx.Rows.Count; i++)
                    {
                        string contract = ddx.Rows[i]["contractNumber"].ObjToString();
                        list += "'" + contract + "',";
                    }
                    list = list.TrimEnd(',');
                    list = "(" + list + ")";
                    cmd = "Select * from `" + paymentsFile + "` where `contractNumber` IN " + list + " order by `payDate8` DESC, `tmstamp` DESC;";
                }
            }
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

//            double sBalance = startBalance;
            string status = "";
            bool deleted = false;
            double NumPayments = 0D;
            double payment = 0D;
            double debit = 0D;
            double credit = 0D;
            double interest = 0D;
            double principal = 0D;
            double balance = 0D;
            string reason = "";
            string edited = "";
            DateTime pDate = DateTime.Now;
            DateTime dDate = DateTime.Now;
            for (int i = 0; i <dt.Rows.Count; i++)
            {
                payment = dt.Rows[i]["paymentAmount"].ObjToString().ObjToDouble();
                NumPayments = 0D;
                if (ExpectedPayment > 0D)
                {
                    NumPayments = payment / ExpectedPayment;
                    if (!String.IsNullOrWhiteSpace(workPayer))
                    {
                        pDate = dt.Rows[i]["payDate8"].ObjToDateTime();
                        dDate = dt.Rows[i]["dueDate8"].ObjToDateTime();
                        double months = DailyHistory.CheckMonthsForInsurance(workContract, workPayer, ExpectedPayment, payment, pDate, dDate );
                        NumPayments = months;
                        //nextDueDate = dueDate.ObjToDateTime();
                        //int imonths = (int)months;
                        //nextDueDate = nextDueDate.AddMonths(imonths);
                    }
                }
                dt.Rows[i]["NumPayments"] = NumPayments;
                debit = dt.Rows[i]["debitAdjustment"].ObjToString().ObjToDouble();
                credit = dt.Rows[i]["creditAdjustment"].ObjToString().ObjToDouble();
                status = dt.Rows[i]["fill"].ObjToString();
                if (status.ToUpper() == "D")
                    deleted = true;
                //if (payment == 0D && debit == 0D && credit == 0D)
                //    dt.Rows.RemoveAt(i);
                break;
            }
            return dt;
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
        private void loadAgents( string cmd )
        {
            //string agent = "";
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
        private void btnLeft_Click(object sender, EventArgs e)
        {
            DateTime now = this.dateTimePicker2.Value;
            DateTime date = new DateTime(now.Year, now.Month, now.Day);
            date = date.AddMonths(-1);
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            this.dateTimePicker2.Value = new DateTime(date.Year, date.Month, days);
            //if (this.dateTimePickerAsOf.Visible)
            //    this.dateTimePickerAsOf.Value = this.dateTimePicker2.Value;
        }
        /****************************************************************************************/
        private void btnRight_Click(object sender, EventArgs e)
        {
            DateTime now = this.dateTimePicker2.Value;
            now = now.AddMonths(1);
            now = new DateTime(now.Year, now.Month, now.Day);
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            this.dateTimePicker2.Value = new DateTime(now.Year, now.Month, days);
            //this.dateTimePicker1.Value = now;
            //int days = DateTime.DaysInMonth(now.Year, now.Month);
            //this.dateTimePicker2.Value = new DateTime(now.Year, now.Month, days);
            //if ( this.dateTimePickerAsOf.Visible )
            //    this.dateTimePickerAsOf.Value = now;
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
        private void ResetLapses ()
        {
            DialogResult result = MessageBox.Show("Are you sure you want to REVERSE LAPSE ALL customers being displayed?", "Reverse Lapsed Customers Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
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
                record = dt.Rows[i]["record"].ObjToString();
                G1.update_db_table("payers", "record", record, new string[] { "lapsed", "", "lapseDate8", "0000-00-00" });
                record = dt.Rows[i]["record1"].ObjToString();
                G1.update_db_table("icontracts", "record", record, new string[] { "lapsed", "", "lapseDate8", "0000-00-00" });
                record = dt.Rows[i]["record2"].ObjToString();
                G1.update_db_table("icustomers", "record", record, new string[] { "lapsed", "" });


                //record = dt.Rows[i]["record"].ObjToString();
                //G1.update_db_table("icontracts", "record", record, new string[] { "lapsed", "Y", "lapseDate8", lapseDate });
                //record = dt.Rows[i]["record2"].ObjToString();
                //G1.update_db_table("icustomers", "record", record, new string[] { "lapsed", "Y" });
                G1.AddToAudit(LoginForm.username, "PastDue", "Reset Lapse", "Reset", contractNumber);
            }
        }
        /****************************************************************************************/
        private void btnMark_Click(object sender, EventArgs e)
        {
            if ( chkShowLapsed.Checked )
            {
                ResetLapses();
                return;
            }
            int days = txtPastDue.Text.ObjToInt32();
            if ( days <= 0 )
            {
                MessageBox.Show("***ERROR*** Days Late must be greater than zero!");
                return;
            }
            DialogResult result = MessageBox.Show("Are you sure you want to MARK customers over " +  days.ToString() + " days late as Lapsed???", "Mark Lapse Accounts Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
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
            for ( int i=0; i<lastRow; i++)
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                daysLate = dt.Rows[i]["daysLate"].ObjToInt32();
                balanceDue = dt.Rows[i]["balanceDue"].ObjToDouble();
                //                if ( daysLate >= days && balanceDue >= minimumBalance 
                if (daysLate >= days )
                {
                    record = dt.Rows[i]["record"].ObjToString();
                    G1.update_db_table("payers", "record", record, new string[] { "lapsed", "Y", "lapseDate8", lapseDate });
                    record = dt.Rows[i]["record1"].ObjToString();
                    G1.update_db_table("icontracts", "record", record, new string[] { "lapsed", "Y", "lapseDate8", lapseDate });
                    record = dt.Rows[i]["record2"].ObjToString();
                    G1.update_db_table("icustomers", "record", record, new string[] { "lapsed", "Y" });


                    //record = dt.Rows[i]["record"].ObjToString();
                    //G1.update_db_table("icontracts", "record", record, new string[] { "lapsed", "Y", "lapseDate8", lapseDate });
                    //record = dt.Rows[i]["record2"].ObjToString();
                    //G1.update_db_table("icustomers", "record", record, new string[] { "lapsed", "Y" });
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
            string record = "";
            int days = txtPastDue.Text.ObjToInt32();
            if (days <= 0)
            {
                MessageBox.Show("***ERROR*** Days Late must be greater than zero!");
                return;
            }
            if (chkShowLapsed.Checked)
            {
                DialogResult result = MessageBox.Show("Are you sure you want to REVERSE LAPSED for customer (" + contract + ") ??", "Reverse Lapse Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
                if (result == DialogResult.No)
                    return;
                DataTable dt = (DataTable)dgv.DataSource;
                record = dr["record"].ObjToString();
                G1.update_db_table("payers", "record", record, new string[] { "lapsed", "", "lapseDate8", "0000-00-00" });
                record = dr["record1"].ObjToString();
                G1.update_db_table("icontracts", "record", record, new string[] { "lapsed", "", "lapseDate8", "0000-00-00" });
                record = dr["record2"].ObjToString();
                G1.update_db_table("icustomers", "record", record, new string[] { "lapsed", "" });
                G1.AddToAudit(LoginForm.username, "PastDue", "Reset Lapse", "Reset", contract);
                if (row >= 0 && row < dt.Rows.Count)
                {
                    dt.Rows.RemoveAt(row);
                    dgv.DataSource = dt;
                    dgv.RefreshDataSource();
                    dgv.Refresh();
                }
            }
            else
            {
                DialogResult result = MessageBox.Show("Are you sure you want to MARK customer (" + contract + ") over " + days.ToString() + " days late as Lapsed???", "Mark Lapse Accounts Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
                if (result == DialogResult.No)
                    return;
                int daysLate = 0;
                DateTime today = this.dateTimePickerAsOf.Value;
                string lapseDate = today.ToString("yyyy-MM-dd");
                daysLate = dr["daysLate"].ObjToInt32();
                if (daysLate >= days)
                {
                    DataTable dt = (DataTable)dgv.DataSource;
                    record = dr["record"].ObjToString();
                    G1.update_db_table("payers", "record", record, new string[] { "lapsed", "Y", "lapseDate8", lapseDate });
                    record = dr["record1"].ObjToString();
                    G1.update_db_table("icontracts", "record", record, new string[] { "lapsed", "Y", "lapseDate8", lapseDate });
                    record = dr["record2"].ObjToString();
                    G1.update_db_table("icustomers", "record", record, new string[] { "lapsed", "Y" });
                    G1.AddToAudit(LoginForm.username, "PastDue", "Lapse", "Set", contract);
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
        }
        /****************************************************************************************/
        private void txtBalance_KeyUp(object sender, KeyEventArgs e)
        {
            if ( e.KeyCode == Keys.Enter )
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
                        if ( e.KeyCode != Keys.OemPeriod )
                            nonNumberEntered = true;
                    }
                }
            }
            //If shift key was pressed, it's not a number.
            if (Control.ModifierKeys == Keys.Shift)
            {
                nonNumberEntered = true;
            }
            if ( nonNumberEntered )
            {
                MessageBox.Show("***ERROR*** Key entered must be a number!");
                e.Handled = true;
            }
        }
        /****************************************************************************************/
        private int GetLapseNoticeValues ( string option, string defaultAnswer = "")
        {
            int rv = 0;
            if ( !String.IsNullOrWhiteSpace ( defaultAnswer ))
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
        private void GenerateNotices(DataTable dt )
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
            RichTextBox rtb3 = new RichTextBox();
            rtb3.Font = new Font("Lucida Console", 9);
            //rtb3.Font = new Font("Lucida Console", 8);

            int padright = GetLapseNoticeValues("Lapse Notices Left Side Width", "30");
            padright += 1;
            int padTop = GetLapseNoticeValues("Lapse Notices Top Border Lines");
            int padLeft = GetLapseNoticeValues("Lapse Notices Left Border Spaces");
            int padBottom = GetLapseNoticeValues("Lapse Notices Bottom Border Lines");
            int padToCustomer = GetLapseNoticeValues("Lapse Notices Lines Prior to Customer");
            int tof = GetLapseNoticeValues("TOF after X Notices");

            int newpadLeft = 0;
            int newpadDown = 1;
            int newskipRight = 2;
            //padright = padright + newskipRight;


            DateTime lapseDate = this.dateTimePickerAsOf.Value;

            rtb2.Document.Text = "";
            int noticeCount = 0;
            string contractNumber = "";
            string contractList = "";
            string SDICode = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                daysLate = dt.Rows[i]["daysLate"].ObjToInt32();
                if (limit > 0 && daysLate >= limit)
                {
                    contract = dt.Rows[i]["contractNumber"].ObjToString();
                    contractList += contract + ",";
                    contract = dt.Rows[i]["payer"].ObjToString();
                    balanceDue = dt.Rows[i]["balanceDue"].ObjToDouble();
                    payment = dt.Rows[i]["amtOfMonthlyPayt"].ObjToDouble();
                    SDICode = dt.Rows[i]["SDICode"].ObjToString();
                    if (SDICode == "06")
                        SDICode = "13";
                    if ( payment > 500D)
                    {
                        contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                        payment = Policies.CalcMonthlyPremium(contractNumber, "", payment);
                    }
                    miniContract = Trust85.decodeContractNumber(contract, ref trust, ref loc);
                    LocateFuneralHome( SDICode, ref name, ref address, ref city, ref state, ref zip);
                    if (String.IsNullOrWhiteSpace(name))
                    {
                        name = "South MS Funeral Services";
                        address = "P.O. Box 727";
                        city = "Bay Springs";
                        state = "MS";
                        zip = "39422";
                    }

                    try
                    {
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
                        line += "AT LEAST 30 DAYS\n";
                        if (padLeft > 0)
                            line = "".PadLeft(padLeft) + line;
                        rtb3.AppendText(line);

                        line = "".PadRight(padright);
                        line += "PAST DUE. YOUR\n";
                        if (padLeft > 0)
                            line = "".PadLeft(padLeft) + line;
                        rtb3.AppendText(line);

                        line = "".PadRight(padright);
                        line += "POLICY HAS LAPSED\n";
                        if (padLeft > 0)
                            line = "".PadLeft(padLeft) + line;
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
                            for (int j = 0; j < padToCustomer; j++)
                                rtb3.AppendText("\n");
                        }

                        newpadLeft = padLeft - 8;

                        name = dt.Rows[i]["firstName"].ObjToString() + " " + dt.Rows[i]["lastName"].ObjToString();
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
                            for (int j = 0; j < padBottom-1; j++)
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
                    catch ( Exception ex )
                    {

                    }
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
                string noticeRecord = PastDue.SaveToDatabase(rtb3.Rtf, "trust");
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

            this.Cursor = Cursors.Default;

            printPreviewToolStripMenuItem_Click(null, null);
        }
        /****************************************************************************************/
        private void printNoticesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            GenerateNotices( dt );
        }
        /****************************************************************************************/
        private DataTable funDt = null;
        /****************************************************************************************/
        private void LocateFuneralHome(string SDICode, ref string name, ref string address, ref string city, ref string state, ref string zip)
        {
            name = "";
            address = "";
            city = "";
            state = "";
            zip = "";

            if (funDt == null)
                funDt = G1.get_db_data("Select * from `funeralhomes`;");

            DataRow[] dRows = funDt.Select("SDICode='" + SDICode + "'");
            if (dRows.Length <= 0)
                dRows = funDt.Select("keycode='B'");
            if (dRows.Length <= 0)
                return;

            //name = dRows[0]["LocationCode"].ObjToString();
            name = dRows[0]["name"].ObjToString();
            address = dRows[0]["address"].ObjToString();
            city = dRows[0]["city"].ObjToString();
            state = dRows[0]["state"].ObjToString();
            zip = dRows[0]["zip"].ObjToString();
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
                tempview.Sort = "agentNumber";
                dt = tempview.ToTable();
                dgv.DataSource = dt;

                gridMain.Columns["agentNumber"].GroupIndex = 0;
                gridMain.OptionsView.ShowFooter = true;
                this.gridMain.ExpandAllGroups();
            }
            else
            {
                DataView tempview = dt.DefaultView;
                tempview.Sort = "dueDate8";
                dt = tempview.ToTable();
                dgv.DataSource = dt;

                gridMain.Columns["agentNumber"].GroupIndex = -1;
                gridMain.OptionsView.ShowFooter = false;
                gridMain.CollapseAllGroups();
            }
            gridMain.RefreshData();
            dgv.Refresh();
        }
        /****************************************************************************************/
        private void chkCalcBalance_CheckedChanged(object sender, EventArgs e)
        {
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
            G1.SetColumnPosition(gridMain, "payer", 4);
            G1.SetColumnPosition(gridMain, "contractNumber", 5);
            G1.SetColumnPosition(gridMain, "lastName", 6);
            G1.SetColumnPosition(gridMain, "firstName", 7);
            G1.SetColumnPosition(gridMain, "amtOfMonthlyPayt", 8);
            G1.SetColumnPosition(gridMain, "balanceDue", 9);
            G1.SetColumnPosition(gridMain, "dueDate8", 10);
            G1.SetColumnPosition(gridMain, "lastDatePaid8", 11);
            G1.SetColumnPosition(gridMain, "phone", 12);
            //G1.SetColumnPosition(gridMain, "totalContract", 13);
            G1.SetColumnPosition(gridMain, "daysLate", 13);
            G1.SetColumnPosition(gridMain, "reinstateDate8", 14);
            G1.SetColumnPosition(gridMain, "SDICode", 15);
            //G1.SetColumnPosition(gridMain, "address1", 15);
            //G1.SetColumnPosition(gridMain, "address2", 16);
            txtPastDue.Visible = true;
            btnMark.Visible = true;
            this.dateTimePickerAsOf.Visible = true;
            txtBalance.Visible = true;
            label2.Visible = true;
            label3.Visible = true;
            label4.Visible = true;
        }
        /****************************************************************************************/
        private void SetLapse()
        {
            ClearAllPositions();
            G1.SetColumnPosition(gridMain, "num", 1);
            G1.SetColumnPosition(gridMain, "payer", 2);
            G1.SetColumnPosition(gridMain, "contractNumber", 3);
            G1.SetColumnPosition(gridMain, "lastName", 4);
            G1.SetColumnPosition(gridMain, "firstName", 5);
            G1.SetColumnPosition(gridMain, "agentCode", 6);
            G1.SetColumnPosition(gridMain, "agentName", 7);
            G1.SetColumnPosition(gridMain, "amtOfMonthlyPayt", 8);
            G1.SetColumnPosition(gridMain, "lapseDate8", 9);
            G1.SetColumnPosition(gridMain, "lapsed", 10);
            //G1.SetColumnPosition(gridMain, "address1", 10);
            //G1.SetColumnPosition(gridMain, "address2", 11);
            txtPastDue.Visible = false;
            btnMark.Visible = false;
            this.dateTimePickerAsOf.Visible = false;
            txtBalance.Visible = false;
            label2.Visible = false;
            label3.Visible = false;
            label4.Visible = false;
        }
        /****************************************************************************************/
        private void chkGroupAgent_CheckedChanged(object sender, EventArgs e)
        {
            if (chkGroupAgent.Checked)
            {
                gridMain.Columns["agentCode"].GroupIndex = 0;
                gridMain.ExpandAllGroups();
                chkCollapes.Visible = true;
            }
            else
            {
                gridMain.Columns["agentCode"].GroupIndex = -1;
                chkCollapes.Visible = false;
            }
        }
        /****************************************************************************************/
        private void chkCollapes_CheckedChanged(object sender, EventArgs e)
        {
            if (chkCollapes.Checked)
                gridMain.CollapseAllGroups();
            else
                gridMain.ExpandAllGroups();
        }
        /****************************************************************************************/
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
                    if (date.Year > 50)
                        e.DisplayText = date.ToString("MM/dd/yyyy");
                    else
                        e.DisplayText = "";
                }
            }
        }
        /****************************************************************************************/
        private void chkShowLapsed_CheckedChanged(object sender, EventArgs e)
        {
            if (!chkShowLapsed.Checked)
            {
                btnMark.Text = "Set as Lapse";
                markCustomerAsLapsedToolStripMenuItem.Text = "Mark Customer as Lapsed";
            }
            else
            {
                btnMark.Text = "Reverse Lapses";
                markCustomerAsLapsedToolStripMenuItem.Text = "Reverse Customer Lapsed";
                string cmd = "Select * from `icontracts` where `lapseDate8` > '1900-01-01' order by `lapseDate8` DESC LIMIT 1;";
                DataTable dt = G1.get_db_data(cmd);
                if ( dt.Rows.Count > 0 )
                {
                    DateTime date = dt.Rows[0]["lapseDate8"].ObjToDateTime();
                    MessageBox.Show("***INFO*** The Last Lapse Date was set to " + date.ToString("MM/dd/yyyy") + " !", "Last Lapse Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                }
            }
        }
        /****************************************************************************************/
        private void gridMain_CellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            DataTable dt = (DataTable)dgv.DataSource;
            if (e.Column.FieldName.ToUpper() == "SDICODE")
            {
                string SDICode = dr["SDICode"].ObjToString();
                if ( SDICode.Length != 2 )
                {
                    MessageBox.Show("***ERROR*** SDICode must be exactly 2 characters!", "SDICode Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    return;
                }
                if ( !G1.validate_numeric ( SDICode ))
                {
                    MessageBox.Show("***ERROR*** SDICode must be numeric!", "SDICode Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    return;
                }
                string record = dr["record"].ObjToString();
                string payer = dr["payer"].ObjToString();
                G1.update_db_table("payers", "record", record, new string[] {"SDICode", SDICode });
            }
        }
        /****************************************************************************************/
    }
}