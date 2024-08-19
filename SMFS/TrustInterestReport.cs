using DevExpress.Charts.Native;
using DevExpress.Data;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraPrinting;
using GeneralLib;
using java.awt.print;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class TrustInterestReport : DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        private DataTable originalDt = null;
        private DataTable detailDt = null;
        private bool previousDateRead = false;

        private bool loading = true;
        public TrustInterestReport()
        {
            InitializeComponent();
        }
        /***********************************************************************************************/
        private void TrustInterestReport_Load(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            now = now.AddMonths(-1);
            now = new DateTime(now.Year, now.Month, 1);
            this.dateTimePicker1.Value = now;
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            this.dateTimePicker2.Value = new DateTime(now.Year, now.Month, days);

            SetupTotalsSummary();

            chkCollapse.Hide();

            if (!G1.RobbyServer)
                txtContract.Hide();

            loading = false;
        }
        /****************************************************************************************/
        private void SetupTotalsSummary()
        {
            AddSummaryColumn("totalInterest", null);
            AddSummaryColumn("total15", null);
        }
        /****************************************************************************************/
        private void AddSummaryColumn(string columnName, DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gMain = null)
        {
            if (gMain == null)
                gMain = gridMain;
            //            gMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            gMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            //            gMain.Columns[columnName].SummaryItem.DisplayFormat = "${0:0,0.00}";
            gMain.Columns[columnName].SummaryItem.DisplayFormat = "{0:N2}";
        }
        /***********************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            SetSpyGlass(gridMain);
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
        private int pageMarginLeft = 0;
        private int pageMarginRight = 0;
        private int pageMarginTop = 0;
        private int pageMarginBottom = 0;
        private bool isPrinting = false;
        /***********************************************************************************************/
        private void printPreviewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mainPrintRow = 0;

            isPrinting = true;
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

            G1.PrintPreview(printableComponentLink1, gridMain);

            //printableComponentLink1.CreateDocument();
            //printableComponentLink1.ShowPreview();

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
            Printer.DrawQuad(6, 7, 4, 4, this.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);

            font = new Font("Ariel", 10, FontStyle.Bold);

            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /****************************************************************************************/
        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mainPrintRow = 0;

            isPrinting = true;
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
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            if (!chkCollapse.Checked && chkShowDetail.Checked)
            {
                string contract = dr["contractNumber"].ObjToString();
                if (!String.IsNullOrWhiteSpace(contract))
                {
                    this.Cursor = Cursors.WaitCursor;
                    CustomerDetails clientForm = new CustomerDetails(contract);
                    clientForm.Show();
                    this.Cursor = Cursors.Default;
                }
            }
        }
        /***********************************************************************************************/
        private void btnRun_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            DataTable funDt = G1.get_db_data("Select * from `funeralhomes`;");

            DateTime date = this.dateTimePicker1.Value;
            string date1 = date.ToString("yyyy-MM-dd");
            date = this.dateTimePicker2.Value;
            string date2 = date.ToString("yyyy-MM-dd");

            string cmd = "SELECT c.`contractNumber`,j.`lastName`, j.`firstName`, c.`issueDate8`, c.`dueDate8`, c.`lapseDate8`,j.`deceasedDate`,p.`downPayment`,p.`payDate8`,SUM(p.`interestPaid`) AS total_interest, SUM(p.`trust85p`) AS total_trust85, SUM(p.`trust100p`) AS total_trust100   FROM contracts c JOIN customers j ON c.`contractNumber` = j.`contractNumber` LEFT JOIN `payments` p ON p.`contractNumber` = c.`contractNumber` ";
            cmd += " WHERE p.`payDate8` >= '" + date1 + "' AND p.`payDate8` <= '" + date2 + "' ";

            string contract = txtContract.Text.Trim();
            if (!String.IsNullOrWhiteSpace(contract))
                cmd += " AND c.`contractNumber` = '" + contract + "' ";
            cmd += " GROUP BY c.`contractNumber`;";

            DataTable dt = G1.get_db_data(cmd);

            FindNewContracts(dt, date1, date2);

            dt.Columns.Add("location");
            dt.Columns.Add("loc");
            dt.Columns.Add("totalInterest", Type.GetType("System.Double"));
            dt.Columns.Add("total15", Type.GetType("System.Double"));

            string runOn = cmbRunOn.Text.Trim().ToUpper();

            if (runOn.ToUpper() != "RILES")
                dt = SMFS.FilterForRiles(dt);

            dt = Trust85.FilterForCemetery(dt, runOn);

            dt = CleanupFutureReporting(dt, date1, date2);

            double totalInterest = 0D;
            double total15 = 0D;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                contract = dt.Rows[i]["contractNumber"].ObjToString();
                if (contract == "M23001LI")
                {
                }
                CalcTrustData(contract, dateTimePicker1.Value, dateTimePicker2.Value, ref totalInterest, ref total15);
                dt.Rows[i]["totalInterest"] = totalInterest;
                dt.Rows[i]["total15"] = total15;
                if (dt.Rows[i]["specialDP"].ObjToString().ToUpper() == "Y")
                {
                    total15 = dt.Rows[i]["total_Trust100"].ObjToDouble() - dt.Rows[i]["total_Trust85"].ObjToDouble();
                    dt.Rows[i]["total15"] = dt.Rows[i]["total_Trust100"].ObjToDouble() - dt.Rows[i]["total_Trust85"].ObjToDouble();
                }
            }

            string contractNumber = "";
            string trust = "";
            string loc = "";

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                if (contractNumber == "FF21047LI")
                {
                    totalInterest = dt.Rows[i]["totalInterest"].ObjToDouble();
                }
                contract = Trust85.decodeContractNumber(contractNumber, ref trust, ref loc);
                dt.Rows[i]["location"] = loc;
            }

            DataView tempview = dt.DefaultView;
            tempview.Sort = "location asc";
            dt = tempview.ToTable();

            double value = 0D;
            totalInterest = 0D;
            total15 = 0D;
            string oldLocation = "";

            DataRow dRow = null;
            DataRow[] dRows = null;

            string oldLoc = "";
            DataTable dx = dt.Clone();

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                loc = dt.Rows[i]["location"].ObjToString();
                if (String.IsNullOrWhiteSpace(oldLoc))
                {
                    oldLoc = loc;
                    dRows = funDt.Select("keycode='" + loc + "'");
                    if (dRows.Length > 0)
                        oldLocation = dRows[0]["LocationCode"].ObjToString();
                    else
                        oldLocation = loc;
                }

                if (loc != oldLoc)
                {
                    dRow = dx.NewRow();
                    dRow["loc"] = oldLoc;
                    dRow["location"] = oldLocation;
                    dRow["totalInterest"] = totalInterest;
                    dRow["total15"] = total15;
                    dx.Rows.Add(dRow);

                    totalInterest = 0D;
                    total15 = 0D;
                    oldLoc = loc;
                    dRows = funDt.Select("keycode='" + loc + "'");
                    if (dRows.Length > 0)
                        oldLocation = dRows[0]["LocationCode"].ObjToString();
                    else
                        oldLocation = loc;
                }

                //totalInterest += dt.Rows[i]["total_interest"].ObjToDouble();
                //total15 += dt.Rows[i]["total_trust100"].ObjToDouble() - dt.Rows[i]["total_trust85"].ObjToDouble();

                totalInterest += dt.Rows[i]["totalInterest"].ObjToDouble();
                total15 += dt.Rows[i]["total15"].ObjToDouble();

                //dt.Rows[i]["totalInterest"] = dt.Rows[i]["total_interest"].ObjToDouble();
                //dt.Rows[i]["total15"] = dt.Rows[i]["total_trust100"].ObjToDouble() - dt.Rows[i]["total_trust85"].ObjToDouble();
                dt.Rows[i]["location"] = oldLocation;
            }

            dRow = dx.NewRow();
            dRow["loc"] = oldLoc;
            dRow["totalInterest"] = totalInterest;
            dRow["total15"] = total15;
            dx.Rows.Add(dRow);

            detailDt = dt.Copy();

            //dx = RemoveCemeteries(dx);

            string location = "";

            for (int i = 0; i < dx.Rows.Count; i++)
            {
                loc = dx.Rows[i]["loc"].ObjToString();
                dRows = funDt.Select("keycode='" + loc + "'");
                if (dRows.Length > 0)
                    dx.Rows[i]["location"] = dRows[0]["LocationCode"].ObjToString();
                else
                    dx.Rows[i]["location"] = loc;
            }

            tempview = dx.DefaultView;
            tempview.Sort = "location asc";
            dx = tempview.ToTable();

            //dx = dt.Copy();

            G1.NumberDataTable(dx);
            dgv.DataSource = dx;
            originalDt = dx;
            this.Cursor = Cursors.Default;
            loading = false;
        }
        /****************************************************************************************/
        private void FindNewContracts(DataTable dt, string date1, string date2)
        {
            int lastRow = 0;
            double downPayment = 0D;
            if (G1.get_column_number(dt, "specialDP") < 0)
                dt.Columns.Add("specialDP");

            string cmd = "Select * from `contracts` p ";
            cmd += " JOIN `customers` c ON p.`contractNumber` = c.`contractNumber` ";
            cmd += " JOIN `payments` x ON p.`contractNumber` = x.`contractNumber` ";
            cmd += " where p.`issueDate8` >= '" + date1 + "' ";
            cmd += " and   p.`issueDate8` <= '" + date2 + "' ";
            cmd += " and p.`downPayment` > '0.00' ";
            cmd += " and x.`downPayment` > '0.00' ";
            cmd += " ORDER by p.`issueDate8` ";
            cmd += ";";
            DataTable dx = G1.get_db_data(cmd);

            double trust100P = 0D;
            double trust85P = 0D;
            string contract = "";
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                try
                {
                    contract = dx.Rows[i]["contractNumber"].ObjToString();
                    if (contract == "M23001LI")
                    {
                    }
                    trust100P = dx.Rows[i]["trust100P"].ObjToDouble();
                    trust85P = dx.Rows[i]["trust85P"].ObjToDouble();
                    DataRow[] dRows = dt.Select("contractNumber='" + contract + "'");
                    if (dRows.Length <= 0)
                    {
                        dt.ImportRow(dx.Rows[i]);
                        lastRow = dt.Rows.Count - 1;
                        dt.Rows[lastRow]["payDate8"] = dt.Rows[lastRow]["issueDate8"];
                        downPayment = dt.Rows[lastRow]["downPayment"].ObjToDouble();
                        //if (!chkShowAll.Checked)
                        dt.Rows[lastRow]["downPayment"] = downPayment;
                        dt.Rows[lastRow]["total_interest"] = 0D;

                        //dt.Rows[i]["totalTrust85P"] = downPayment * 0.85D;
                        //dt.Rows[i]["totalTrust100P"] = downPayment;
                        //dt.Rows[i]["totalTrust15"] = downPayment - dt.Rows[i]["totalTrust85P"].ObjToDouble();

                        dt.Rows[lastRow]["total_trust100"] = downPayment;
                        dt.Rows[lastRow]["total_trust85"] = downPayment * 0.85D;
                        dt.Rows[lastRow]["total_trust100"] = trust100P;
                        dt.Rows[lastRow]["total_trust85"] = trust85P;
                        //dt.Rows[lastRow]["debitAdjustment"] = 0D;
                        //dt.Rows[lastRow]["creditAdjustment"] = 0D;
                        dt.Rows[lastRow]["specialDP"] = "Y";
                    }
                }
                catch ( Exception ex)
                {
                }
            }
        }
        /****************************************************************************************/
        private DataTable CleanupFutureReporting(DataTable dt, string date1, string date2)
        {
            DateTime lDate1 = date1.ObjToDateTime();
            DateTime lDate2 = date2.ObjToDateTime();

            int nextMonth = lDate2.Month;
            int issueMonth = 0;

            DateTime payDate = DateTime.Now;
            DateTime issueDate = DateTime.Now;

            double downPayment = 0D;

            string contractNumber = "";

            for (int i = dt.Rows.Count - 1; i >= 0; i--)
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                if (contractNumber == "HU23006LI")
                {
                }
                payDate = dt.Rows[i]["payDate8"].ObjToDateTime();
                issueDate = dt.Rows[i]["issueDate8"].ObjToDateTime();
                issueMonth = issueDate.Month;
                if (issueDate.Year > lDate2.Year)
                    issueMonth += 12;

                if (issueMonth > nextMonth)
                {
                    if (previousDateRead)
                        downPayment = dt.Rows[i]["newBusiness"].ObjToDouble();
                    else
                        downPayment = dt.Rows[i]["downPayment"].ObjToDouble();
                    if (downPayment > 0D)
                        dt.Rows.RemoveAt(i);
                }
            }
            return dt;
        }
        /****************************************************************************************/
        //private DataTable CleanupFutureReporting(DataTable dt, string date1, string date2)
        //{
        //    DateTime lDate1 = date1.ObjToDateTime();
        //    DateTime lDate2 = date2.ObjToDateTime();

        //    int nextMonth = lDate2.Month;
        //    int issueMonth = 0;

        //    DateTime payDate = DateTime.Now;
        //    DateTime issueDate = DateTime.Now;

        //    double downPayment = 0D;

        //    string contractNumber = "";

        //    for (int i = dt.Rows.Count - 1; i >= 0; i--)
        //    {
        //        contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
        //        payDate = dt.Rows[i]["payDate8"].ObjToDateTime();
        //        issueDate = dt.Rows[i]["issueDate8"].ObjToDateTime();
        //        issueMonth = issueDate.Month;
        //        if (issueDate.Year > lDate2.Year)
        //            issueMonth += 12;
        //        if (issueMonth > nextMonth)
        //        {
        //            downPayment = dt.Rows[i]["downPayment"].ObjToDouble();
        //            if (downPayment > 0D)
        //                dt.Rows.RemoveAt(i);
        //        }
        //    }
        //    return dt;
        //}
        /****************************************************************************************/
        private void CalcTrustData(string contractNumber, DateTime workDate1, DateTime workDate2, ref double totalInterest, ref double total15)
        {
            totalInterest = 0D;
            total15 = 0D;
            string cmd = "Select * from `contracts` where `contractNumber` = '" + contractNumber + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
                return;

            DataTable contractDt = dx.Copy();

            double payment = dx.Rows[0]["amtOfMonthlyPayt"].ObjToString().ObjToDouble();
            int numPayments = dx.Rows[0]["numberOfPayments"].ObjToString().ObjToInt32();
            string dueDate = dx.Rows[0]["dueDate8"].ObjToString();
            string issueDate = dx.Rows[0]["issueDate8"].ObjToString();
            DateTime iDate = DailyHistory.GetIssueDate(dx.Rows[0]["issueDate8"].ObjToDateTime(), contractNumber, dx);
            issueDate = iDate.ToString("MM/dd/yyyy");
            DateTime lastDate = issueDate.ObjToDateTime();
            string apr = dx.Rows[0]["APR"].ObjToString();
            double dAPR = apr.ObjToDouble() / 100.0D;
            double contractValue = DailyHistory.GetContractValue(dx.Rows[0]);

            cmd = "Select * from `payments` where `contractNumber` = '" + contractNumber + "' order by `paydate8` DESC, `tmstamp` DESC;";
            dx = G1.get_db_data(cmd);

            if (numPayments <= 0 && dx.Rows.Count > 0)
                numPayments = dx.Rows.Count;
            double startBalance = DailyHistory.GetFinanceValue(contractNumber);

            DailyHistory.CalculateNewStuff(dx, dAPR, numPayments, startBalance, lastDate);

            for (int i = 0; i < dx.Rows.Count; i++)
            {
                iDate = dx.Rows[i]["payDate8"].ObjToDateTime();
                if (iDate >= workDate1 && iDate <= workDate2)
                {
                    if (iDate > workDate2)
                        break;
                    totalInterest += dx.Rows[i]["interestPaid"].ObjToDouble();
                    total15 += dx.Rows[i]["calculatedTrust100"].ObjToDouble() - dx.Rows[i]["calculatedTrust85"].ObjToDouble();
                }
            }
            return;
        }
        /****************************************************************************************/
        private DataTable RemoveCemeteries(DataTable dt)
        {
            string contractNumber = "";
            string contractHome = "";
            try
            {
                for (int i = (dt.Rows.Count - 1); i >= 0; i--)
                {
                    contractNumber = dt.Rows[i]["loc"].ObjToString();
                    if (String.IsNullOrWhiteSpace(contractNumber))
                        dt.Rows.RemoveAt(i);
                    else if (TrustReports.isCemetery(contractNumber))
                        dt.Rows.RemoveAt(i);
                }
            }
            catch (Exception ex)
            {
            }
            return dt;
        }
        /***********************************************************************************************/
        private DataTable GetGroupData(DataTable dt)
        {
            if (dt.Rows.Count <= 0)
                return dt;

            DataTable groupDt = dt.AsEnumerable().GroupBy(r => new { Col1 = r["payer"] }).Select(g => g.OrderBy(r => r["payer"]).First()).CopyToDataTable();
            return groupDt;
        }
        /***********************************************************************************************/
        private void gridMain_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column.FieldName.ToUpper().IndexOf("NUM") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                e.DisplayText = e.GroupRowHandle.ObjToString();
            }
        }
        /***********************************************************************************************/
        private void gridMain_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            //int row = e.ListSourceRow;
            //DataTable dt = (DataTable)dgv.DataSource;
            //double newPremium = dt.Rows[row]["newPremium"].ObjToDouble();
            //double oldPremium = dt.Rows[row]["oldPremium"].ObjToDouble();
            //if (newPremium == 0D && oldPremium == 0D )
            //{
            //    e.Visible = false;
            //    e.Handled = true;
            //    return;
            //}
        }
        /***********************************************************************************************/
        private void gridMain_RowCellStyle(object sender, DevExpress.XtraGrid.Views.Grid.RowCellStyleEventArgs e)
        {
            //GridView View = sender as GridView;
            //if (e.RowHandle >= 0)
            //{
            //    double payment = View.GetRowCellDisplayText(e.RowHandle, View.Columns["paymentAmount"]).ObjToDouble();
            //    double expected = View.GetRowCellDisplayText(e.RowHandle, View.Columns["premium"]).ObjToDouble();
            //    double secNat = View.GetRowCellDisplayText(e.RowHandle, View.Columns["secNatPremium"]).ObjToDouble();
            //    double thirdParty = View.GetRowCellDisplayText(e.RowHandle, View.Columns["thirdPartyPremium"]).ObjToDouble();

            //    double total = expected + secNat;
            //    if ( payment == total )
            //    {
            //        e.Appearance.BackColor = Color.Red;
            //    }
            //}
        }
        /***********************************************************************************************/
        private void chkHonor_CheckedChanged(object sender, EventArgs e)
        {
            //btnRun_Click(null, null);
        }
        /***********************************************************************************************/
        private void chkSecNat_CheckedChanged(object sender, EventArgs e)
        {
            //btnRun_Click(null, null);
        }
        /***********************************************************************************************/
        private void btnLeft_Click(object sender, EventArgs e)
        {
            DateTime date = this.dateTimePicker1.Value;
            date = date.AddMonths(-1);
            this.dateTimePicker1.Value = date;
            this.dateTimePicker1.Refresh();

            date = this.dateTimePicker2.Value;
            date = date.AddMonths(-1);
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            this.dateTimePicker2.Value = new DateTime(date.Year, date.Month, days);
            this.dateTimePicker2.Refresh();
        }
        /***********************************************************************************************/
        private void btnRight_Click(object sender, EventArgs e)
        {
            DateTime date = this.dateTimePicker1.Value;
            date = date.AddMonths(1);
            this.dateTimePicker1.Value = date;
            this.dateTimePicker1.Refresh();

            date = this.dateTimePicker2.Value;
            date = date.AddMonths(1);
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            this.dateTimePicker2.Value = new DateTime(date.Year, date.Month, days);
            this.dateTimePicker2.Refresh();
        }
        /***********************************************************************************************/
        private void gridMain_ColumnFilterChanged(object sender, EventArgs e)
        {
            //DataTable dt = (DataTable)dgv.DataSource;
            //int count = dt.Rows.Count;
            //gridMain.SelectAll();
            //int[] rows = gridMain.GetSelectedRows();
            //int row = 0;
            //for (int i = 0; i < rows.Length; i++)
            //{
            //    row = rows[i];
            //    var dRow = gridMain.GetDataRow(row);
            //    if (dRow != null)
            //        dRow["num"] = (i + 1).ToString();
            //}
            //gridMain.ClearSelection();
        }
        /***********************************************************************************************/
        private void RenumberRows()
        {

            //int row = 0;

            //DataTable dt = (DataTable)dgv.DataSource;
            //string num = "";
            //DataRow dRow = null;
            //int iRow = 0;
            //for (int i = 0; i != gridMain.RowCount; i++)
            //{
            //    if (gridMain.IsRowVisible(i) == RowVisibleState.Visible)
            //    {
            //        iRow = gridMain.GetVisibleRowHandle(i);
            //        iRow = gridMain.GetDataSourceRowIndex(iRow);

            //        dt.Rows[iRow]["num"] = (row + 1).ToString();
            //        row++;
            //    }
            //    else
            //    {
            //        num = dt.Rows[i]["num"].ObjToString();
            //    }
            //}
            //dgv.DataSource = dt;

            //row = 0;
            //int count = dt.Rows.Count;
            //gridMain.SelectAll();
            //int[] rows = gridMain.GetSelectedRows();
            //for (int i = 0; i < rows.Length; i++)
            //{
            //    row = rows[i];
            //    var dRow = gridMain.GetDataRow(row);
            //    if (dRow != null)
            //        dRow["num"] = (i + 1).ToString();
            //}
            //gridMain.ClearSelection();
        }
        /***********************************************************************************************/
        private void gridMain_CustomRowFilter_1(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            //int row = e.ListSourceRow;
            //DataTable dt = (DataTable)dgv.DataSource;
            //double secNatPremium= dt.Rows[row]["secNatPremium"].ObjToDouble();
            //double thirdPartyPremium = dt.Rows[row]["thirdPartyPremium"].ObjToDouble();
            //if (secNatPremium == 0D && thirdPartyPremium == 0D)
            //{
            //    e.Visible = false;
            //    e.Handled = true;
            //    return;
            //}
            //double payment = dt.Rows[row]["paymentAmount"].ObjToDouble();
            //double expected = dt.Rows[row]["premium"].ObjToDouble();
            //if ( payment == expected)
            //{
            //    e.Visible = false;
            //    e.Handled = true;
            //    return;
            //}
            //double difference = payment / expected;
            //difference = G1.RoundValue(difference);
            //difference = difference % 1D;
            //difference = G1.RoundValue(difference);
            //if ( difference == 0D)
            //{
            //    e.Visible = false;
            //    e.Handled = true;
            //    return;
            //}
        }
        /***********************************************************************************************/
        private void gridMain_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
        {
            if (loading)
                return;
            if (e.Column.FieldName.ToUpper() == "NUM")
            {
                if (e.RowHandle >= 0)
                {
                    string num = (e.RowHandle + 1).ToString();
                    e.DisplayText = num;
                    //int row = gridMain.GetDataSourceRowIndex(e.RowHandle);
                    //DataTable dt = (DataTable)dgv.DataSource;
                    //dt.Rows[row]["num"] = num;
                }
            }
        }
        /***********************************************************************************************/
        private int mainPrintRow = 0;
        private void gridMain_BeforePrintRow(object sender, DevExpress.XtraGrid.Views.Printing.CancelPrintRowEventArgs e)
        {
            int row = e.RowHandle;
            if (row < 0)
                return;
            //row = gridMain.GetDataSourceRowIndex(row);
            //DataTable dt = (DataTable)dgv.DataSource;
            //dt.Rows[row]["num"] = (mainPrintRow + 1).ToString();
            //mainPrintRow++;
        }
        /***********************************************************************************************/
        private void chkShowDetail_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox box = (CheckBox)sender;
            if (box.Checked)
            {
                gridMain.Columns["contractNumber"].Visible = true;
                gridMain.Columns["payDate8"].Visible = true;
                gridMain.Columns["location"].GroupIndex = 0;
                dgv.DataSource = detailDt;
                gridMain.RefreshData();
                gridMain.RefreshEditor(true);
                gridMain.ExpandAllGroups();
                dgv.Refresh();
                chkCollapse.Show();
                chkCollapse.Refresh();
            }
            else
            {
                gridMain.Columns["contractNumber"].Visible = false;
                gridMain.Columns["payDate8"].Visible = false;
                gridMain.Columns["location"].GroupIndex = -1;
                dgv.DataSource = originalDt;
                gridMain.RefreshData();
                gridMain.RefreshEditor(true);
                gridMain.CollapseAllGroups();
                dgv.Refresh();
                chkCollapse.Hide();
                chkCollapse.Refresh();
            }
        }
        /***********************************************************************************************/
        private void chkCollapse_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox box = (CheckBox)sender;
            if (box.Checked)
            {
                gridMain.OptionsView.GroupFooterShowMode = GroupFooterShowMode.VisibleAlways;
                gridMain.CollapseAllGroups();
                //gridMain.ExpandGroupLevel(0);
            }
            else
                gridMain.ExpandAllGroups();
        }
        /***********************************************************************************************/
        private void menuPreviousData_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            DateTime begin = this.dateTimePicker1.Value;
            DateTime end = this.dateTimePicker2.Value;

            int days = DateTime.DaysInMonth(begin.Year, begin.Month);
            DateTime last = new DateTime(begin.Year, begin.Month, days);
            if (last > end)
            {
                this.Cursor = Cursors.Default;
                return;
            }

            string runDate1 = begin.ToString("yyyy-MM-dd");
            string runDate2 = end.ToString("yyyy-MM-dd");
            string runWhat = cmbRunOn.Text.Trim();

            string cmd = "Select * from `cashRemitted` where `runDate1` = '" + runDate1 + "' AND `runDate2` = '" + runDate2 + "' AND `runWhat` = '" + runWhat + "';";
            DataTable dt = G1.get_db_data(cmd);

            previousDateRead = true;

            dt = ConsolidateTrustData(dt);

            dt = CleanupFutureReporting(dt, runDate1, runDate2);

            detailDt = dt.Copy();

            DataTable newDt = BuildPreviousData(dt, runDate1, runDate2);

            G1.NumberDataTable(newDt);
            dgv.DataSource = newDt;

            originalDt = newDt;

            menuStrip1.BackColor = Color.LightGreen;

            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private DataTable BuildPreviousData(DataTable dt, string date1, string date2)
        {
            dt = TrustTotals.AddColumn(dt, "num");
            dt = TrustTotals.AddColumn(dt, "Year");
            dt = TrustTotals.AddColumn(dt, "sDate");
            dt = TrustTotals.AddColumn(dt, "loc");
            dt = TrustTotals.AddColumn(dt, "trust");

            //dt.Columns.Add("totalInterest", Type.GetType("System.Double"));
            dt.Columns.Add("total15", Type.GetType("System.Double"));
            dt.Columns.Add("totalTrust85P", Type.GetType("System.Double"));
            dt.Columns.Add("totalTrust100P", Type.GetType("System.Double"));

            string contractNumber = "";
            string contract = "";
            string trust = "";
            string loc = "";

            DataTable funDt = G1.get_db_data("Select * from `funeralhomes`;");

            DataRow[] dRows = null;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                contract = Trust85.decodeContractNumber(contractNumber, ref trust, ref loc);
                dt.Rows[i]["trust"] = trust;

                dRows = funDt.Select("keycode='" + loc + "'");
                if (dRows.Length > 0)
                    loc = dRows[0]["LocationCode"].ObjToString();

                dt.Rows[i]["loc"] = loc;

                dt.Rows[i]["Year"] = dt.Rows[i]["payDate8"].ObjToDateTime().Year.ToString();
                dt.Rows[i]["sDate"] = dt.Rows[i]["payDate8"].ObjToDateTime().ToString("yyyyMMdd");
            }

            DateTime date = DateTime.Now;

            date = dt.Rows[0]["payDate8"].ObjToDateTime();
            int startYear = date.Year;
            int row = dt.Rows.Count - 1;
            date = dt.Rows[row]["payDate8"].ObjToDateTime();
            int stopYear = date.Year;
            int years = stopYear - startYear + 1;

            //int numPayments = 0;
            //DateTime lastDate = DateTime.Now;
            //string apr = "";
            //double dAPR = 0D;
            //double startBalance = 0D;
            //DataTable dp = null;
            //string cmd = "";

            double totalInterest = 0D;
            double totalTrust15 = 0D;
            double totalTrust85P = 0D;
            double totalTrust100P = 0D;

            DateTime mDate1 = date1.ObjToDateTime();
            DateTime mDate2 = date2.ObjToDateTime();
            DateTime payDate = DateTime.Now;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                if (contractNumber == "FF21047LI")
                {
                    totalInterest = dt.Rows[i]["totalInterest"].ObjToDouble();
                }
                contract = Trust85.decodeContractNumber(contractNumber, ref trust, ref loc);
                dt.Rows[i]["location"] = loc;
            }

            DataView tempview = dt.DefaultView;
            tempview.Sort = "location asc";
            dt = tempview.ToTable();
            string special = "";

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                Application.DoEvents();

                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();

                try
                {
                    totalInterest = dt.Rows[i]["interestPaid1"].ObjToDouble();
                    totalTrust85P = dt.Rows[i]["trust85P"].ObjToDouble();
                    totalTrust100P = dt.Rows[i]["trust100P"].ObjToDouble();
                    dt.Rows[i]["totalInterest"] = totalInterest;
                    dt.Rows[i]["totalTrust85P"] = totalTrust85P;
                    dt.Rows[i]["totalTrust100P"] = totalTrust100P;
                    dt.Rows[i]["total15"] = totalTrust100P - totalTrust85P;
                }
                catch (Exception ex)
                {
                }
            }

            detailDt = dt.Copy();

            string oldLoc = "";
            string oldLocation = "";
            DataTable dx = dt.Clone();
            DataRow dRow = null;
            double total15 = 0D;
            totalInterest = 0D;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    loc = dt.Rows[i]["location"].ObjToString();
                    if (String.IsNullOrWhiteSpace(oldLoc))
                    {
                        oldLoc = loc;
                        dRows = funDt.Select("keycode='" + loc + "'");
                        if (dRows.Length > 0)
                            oldLocation = dRows[0]["LocationCode"].ObjToString();
                        else
                            oldLocation = loc;
                    }

                    if (loc != oldLoc)
                    {
                        dRow = dx.NewRow();
                        dRow["loc"] = oldLoc;
                        dRow["location"] = oldLocation;
                        dRow["totalInterest"] = totalInterest;
                        dRow["total15"] = total15;
                        dx.Rows.Add(dRow);

                        totalInterest = 0D;
                        total15 = 0D;
                        oldLoc = loc;
                        dRows = funDt.Select("keycode='" + loc + "'");
                        if (dRows.Length > 0)
                            oldLocation = dRows[0]["LocationCode"].ObjToString();
                        else
                            oldLocation = loc;
                    }

                    //totalInterest += dt.Rows[i]["total_interest"].ObjToDouble();
                    //total15 += dt.Rows[i]["total_trust100"].ObjToDouble() - dt.Rows[i]["total_trust85"].ObjToDouble();

                    totalInterest += dt.Rows[i]["totalInterest"].ObjToDouble();
                    total15 += dt.Rows[i]["total15"].ObjToDouble();

                    //dt.Rows[i]["totalInterest"] = dt.Rows[i]["total_interest"].ObjToDouble();
                    //dt.Rows[i]["total15"] = dt.Rows[i]["total_trust100"].ObjToDouble() - dt.Rows[i]["total_trust85"].ObjToDouble();
                    dt.Rows[i]["location"] = oldLocation;
                }
                catch (Exception ex)
                {
                }
            }

            dRow = dx.NewRow();
            dRow["loc"] = oldLoc;
            dRow["location"] = oldLocation;
            dRow["totalInterest"] = totalInterest;
            dRow["total15"] = total15;
            dx.Rows.Add(dRow);


            //dx = RemoveCemeteries(dx);

            string location = "";

            for (int i = 0; i < dx.Rows.Count; i++)
            {
                loc = dx.Rows[i]["loc"].ObjToString();
                dRows = funDt.Select("keycode='" + loc + "'");
                if (dRows.Length > 0)
                    dx.Rows[i]["location"] = dRows[0]["LocationCode"].ObjToString();
                else
                    dx.Rows[i]["location"] = loc;
            }

            tempview = dx.DefaultView;
            tempview.Sort = "location asc";
            dx = tempview.ToTable();

            //dx = dt.Copy();

            this.Cursor = Cursors.Default;
            loading = false;
            return dx;
        }
        /****************************************************************************************/
        public static DataTable ConsolidateTrustData(DataTable dt)
        {
            try
            {
                DataView tempview = dt.DefaultView;
                tempview.Sort = "contractNumber asc";
                dt = tempview.ToTable();

                int oldrow = 0;

                string contractNumber = "";
                string oldContractNumber = "";

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                    if (String.IsNullOrWhiteSpace(oldContractNumber))
                    {
                        oldContractNumber = contractNumber;
                        oldrow = i;
                        continue;
                    }
                    if (contractNumber != oldContractNumber)
                    {
                        oldrow = i;
                        oldContractNumber = contractNumber;
                        continue;
                    }
                    if (oldrow == i)
                        continue;

                    oldContractNumber = contractNumber;

                    Commission.CombineData(dt, oldrow, i, "interestPaid1");
                    Commission.CombineData(dt, oldrow, i, "trust85P");
                    Commission.CombineData(dt, oldrow, i, "trust100P");

                    dt.Rows[i]["contractNumber"] = "";
                }
                for (int i = (dt.Rows.Count - 1); i >= 0; i--)
                {
                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                    if (String.IsNullOrWhiteSpace(contractNumber))
                        dt.Rows.RemoveAt(i);
                }
            }
            catch (Exception ex)
            {
            }

            DataView tempv = dt.DefaultView;
            tempv.Sort = "payDate8 asc";
            dt = tempv.ToTable();

            G1.NumberDataTable(dt);
            return dt;
        }
        /***********************************************************************************************/
    }
}