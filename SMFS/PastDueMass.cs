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
    public partial class PastDueMass : DevExpress.XtraEditors.XtraForm
    {
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
        public PastDueMass( string title = "")
        {
            InitializeComponent();
            workTitle = title;
            if (String.IsNullOrWhiteSpace(workTitle))
                workTitle = "Trust Lapse List (4.0)";
        }
        /****************************************************************************************/
        private void PastDueMass_Load(object sender, EventArgs e)
        {
            this.dateTimePicker1.Value = new DateTime(2013, 1, 1);
            workInsurance = false;
            if (workTitle.ToUpper().IndexOf("INSURANCE") == 0)
            {
                workInsurance = true;
                paymentsFile = "ipayments";
                customersFile = "icustomers";
                contractsFile = "icontracts";
            }
            gridMain.OptionsView.ShowFooter = false;
            //gridMain.Columns["calcDueDate"].Visible = false;
            //gridMain.Columns["location"].Visible = false;
            //gridMain.Columns["balanceDue"].Visible = false;
            //            SetupTotalsSummary();
            LoadData();
            this.Text = workTitle;
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

            //            string cmd = "Select * from `payments` where `lastDatePaid8` <= '" + date2 + "';";
            string cmd = "Select * from `contracts` p ";
            cmd += " JOIN `payments` d ON p.`contractNumber` = d.`contractNumber` ";
            cmd += " JOIN `customers` c ON p.`contractNumber` = c.`contractNumber` ";
            cmd += " where ( ( p.`lapseDate8` >= '" + date1 + "' AND p.`lapseDate8` <= '" + date2 + "' ) ";
            cmd += " OR ( p.`reinstateDate8` >= '" + date1 + "' AND p.`reinstateDate8` <= '" + date2 + "' ) ) ";
            cmd += " and (p.`deceasedDate` = '0000-00-00' OR p.`deceasedDate` = '0001-01-01' ) ";
            cmd += " GROUP BY d.`contractNumber` ORDER BY p.`dueDate8` ";
            cmd += ";";

            DataTable dt = G1.get_db_data(cmd);

            dt = SMFS.FilterForRiles(dt);

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
                }
                catch ( Exception ex )
                {
                    MessageBox.Show("***ERROR*** Loading Past Due Contracts! " + ex.Message.ToString());
                }
            }
//            CalcNewDueDate(dt);
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
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

                    cmd = "Select * from `" + paymentsFile + "` where `contractNumber` = '" + contract + "' order by `paydate8` DESC, `tmstamp` DESC;";
                    dx = G1.get_db_data(cmd);
                    DailyHistory.CalculateNewStuff(dx, dAPR, numPayments, startBalance, lastDate);
                    if (dx.Rows.Count > 0)
                    {
                        for (int j = 0; j < dx.Rows.Count; j++)
                        {
                            if (dx.Rows[j]["fill"].ObjToString().ToUpper() == "D")
                                continue;
                            newBalance = dx.Rows[j]["newbalance"].ObjToDouble();
                            iDate = dx.Rows[j]["nextDueDate"].ObjToDateTime();
                            dt.Rows[row]["calcDueDate"] = iDate.ToString("MM/dd/yyyy");
                            break;
                        }
                    }
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
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (gridMain.OptionsFind.AlwaysVisible == true)
                gridMain.OptionsFind.AlwaysVisible = false;
            else
                gridMain.OptionsFind.AlwaysVisible = true;
        }
        /****************************************************************************************/
        private void chkComboAgent_EditValueChanged(object sender, EventArgs e)
        {
            btnRun_Click(null, null);
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
        private void LocateFuneralHome( string loc, ref string name, ref string address, ref string city, ref string state, ref string zip )
        {
            name = "";
            address = "";
            city = "";
            state = "";
            zip = "";
            DataTable dx = G1.get_db_data("Select * from `funeralhomes` where `keycode` = '" + loc + "';");
            if ( dx.Rows.Count > 0 )
            {
                name = dx.Rows[0]["name"].ObjToString();
                address = dx.Rows[0]["address"].ObjToString();
                city = dx.Rows[0]["city"].ObjToString();
                state = dx.Rows[0]["state"].ObjToString();
                zip = dx.Rows[0]["zip"].ObjToString();
            }
        }
        /****************************************************************************************/
    }
}