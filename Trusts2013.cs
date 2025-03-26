using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using GeneralLib;
using DevExpress.XtraEditors;
using System.Runtime.InteropServices;
using Excel = Microsoft.Office.Interop.Excel;
using DevExpress.Utils;
using DevExpress.XtraPrinting;
using DevExpress.XtraGrid.Views.Grid;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class Trusts2013 : DevExpress.XtraEditors.XtraForm
    {
        /****************************************************************************************/
        public Trusts2013()
        {
            InitializeComponent();
            SetupTotalsSummary();
        }
        /****************************************************************************************/
        private void Trusts2013_Load(object sender, EventArgs e)
        {
            btnSave.Hide();
        }
        /****************************************************************************************/
        private void SetupTotalsSummary()
        {
            AddSummaryColumn("beginningBalance");
            AddSummaryColumn("interest");
            AddSummaryColumn("ytdPrevious");
            AddSummaryColumn("paymentCurrMonth");
            AddSummaryColumn("currentPayments");
            AddSummaryColumn("deathRemYTDprevious");
            AddSummaryColumn("deathRemCurrMonth");
            AddSummaryColumn("refundRemYTDprevious");
            AddSummaryColumn("refundRemCurrMonth");
            AddSummaryColumn("currentRemovals");
            AddSummaryColumn("endingBalance");
            AddSummaryColumn("myBalance");
            AddSummaryColumn("myDiff");
            AddSummaryColumn("paulCurrentMonth");
            AddSummaryColumn("ragCurrentMonth");
        }
        /****************************************************************************************/
        private void AddSummaryColumn(string columnName, DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gMain = null)
        {
            try
            {
                if (gMain == null)
                    gMain = gridMain;
                //            gMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
                gMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
                //            gMain.Columns[columnName].SummaryItem.DisplayFormat = "${0:0,0.00}";
                gMain.Columns[columnName].SummaryItem.DisplayFormat = "{0:N2}";
            }
            catch ( Exception ex)
            {
                MessageBox.Show("***ERROR*** Column " + columnName + " " + ex.Message.ToString());
            }
        }
        /****************************************************************************************/
        private void btnOpen_Click(object sender, EventArgs e)
        {
            DataTable dt = new DataTable();
            //dt.Columns.Add("payDate8");
            //dt.Columns.Add("contractNumber");
            //dt.Columns.Add("firstName");
            //dt.Columns.Add("lastName");
            //dt.Columns.Add("beginningBalance", Type.GetType("System.Double"));
            //dt.Columns.Add("currentPayments", Type.GetType("System.Double"));
            //dt.Columns.Add("currentRemovals", Type.GetType("System.Double"));
            //dt.Columns.Add("endingBalance", Type.GetType("System.Double"));
            //dt.Columns.Add("location");
            string cmd = "Select * from `trust2013` where `lastName` = 'XYZZY444';";
            dt = G1.get_db_data(cmd);
            string file = "";
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    file = ofd.FileName;
            }
            if (String.IsNullOrWhiteSpace(file))
                return;
            bool isExcel = false;
            if (file.ToUpper().IndexOf(".XLS") > 0)
                isExcel = true;
            else if (file.ToUpper().IndexOf(".XLSX") > 0)
                isExcel = true;
            if (isExcel)
                PullExcel(dt, file);
            else
                PullCSV(dt, file);
        }
        /****************************************************************************************/
        private void PullPaulCSV ( DataTable dt, DataTable dx, string location )
        {
            string contractNumber = "";
            string lastName = "";
            string firstName = "";
            string address = "";
            string city = "";
            string state = "";
            string zip = "";
            string ssn = "";
            string serviceId = "";

            double beginningBalance = 0D;
            double currentPayments = 0D;
            double currentRemovals = 0D;
            double endingBalance = 0D;
            double PRIORPPIT = 0D;
            double interest = 0D;
            double YTDPREV = 0D;
            double CURRPPIT = 0D;
            dt.Columns.Add("dup");
            dt.Columns.Add("myBalance", Type.GetType("System.Double"));
            dt.Columns.Add("myDiff", Type.GetType("System.Double"));
            dt.Columns.Add("trust85Max", Type.GetType("System.Double"));
            dt.Columns.Add("trust85Remaining", Type.GetType("System.Double"));

            string loc = "";
            string locind = "";
            string trust = "";
            string contract = "";

            double PREVFUNRL = 0D;
            double CURRFUNRL = 0D;
            double PREVREFND = 0D;
            double CURRREFND = 0D;
            double TOTPPITIN = 0D;

            DateTime date = this.dateTimePicker1.Value;
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            date = new DateTime(date.Year, date.Month, days);

            string str = "";
            int lastRow = dx.Rows.Count;
//            lastRow = 30;

            for (int i = 0; i < lastRow; i++)
            {
                str = dx.Rows[i][1].ObjToString();
                if (String.IsNullOrWhiteSpace(str))
                    continue;
                if (str.Trim().ToUpper() == "CNUM")
                    continue;
                if (str.Trim().ToUpper() == "LAST NAME")
                    continue;
                if (str.Trim().ToUpper() == "FIRST NAME")
                    continue;
                try
                {
                    contractNumber = "";
                    lastName = "";
                    firstName = "";
                    beginningBalance = 0D;
                    currentPayments = 0D;
                    currentRemovals = 0D;
                    endingBalance = 0D;

                    contractNumber = dx.Rows[i]["CNUM"].ObjToString();
                    if ( contractNumber == "P457")
                    {
                    }
                    contract = Trust85.decodeContractNumber(contractNumber, ref trust, ref loc);


                    lastName = dx.Rows[i]["LNAME"].ObjToString();
                    firstName = dx.Rows[i]["FNAME"].ObjToString();
                    address = dx.Rows[i]["ADDRESS"].ObjToString();
                    city = dx.Rows[i]["CITY"].ObjToString();
                    state = dx.Rows[i]["STATE"].ObjToString();
                    zip = dx.Rows[i]["ZIP"].ObjToString();
                    ssn = dx.Rows[i]["SSNO"].ObjToString();
                    serviceId = dx.Rows[i]["FUNERAL#"].ObjToString();

                    PRIORPPIT = dx.Rows[i]["PRIORPPIT"].ObjToDouble(); // Payments Total @ End of Last Year
                    interest = dx.Rows[i]["INTEREST"].ObjToDouble(); // Interest
                    YTDPREV = dx.Rows[i]["YTDPREV"].ObjToDouble(); // Payments Total Current Year Prior to Current Month
                    CURRPPIT = dx.Rows[i]["CURRPPIT"].ObjToDouble(); // Payments Current Month

                    PREVFUNRL = dx.Rows[i]["PREVFUNRL"].ObjToDouble(); // Death Removals in Current Year Before Current Month
                    CURRFUNRL = dx.Rows[i]["CURRFUNRL"].ObjToDouble(); // Death Removals in Current Month
                    PREVREFND = dx.Rows[i]["PREVREFND"].ObjToDouble(); // Refund Removals in Current Year Before Current Month
                    CURRREFND = dx.Rows[i]["CURRREFND"].ObjToDouble(); // Refund Removals in Current Month
                    TOTPPITIN = dx.Rows[i]["TOTPPITIN"].ObjToDouble(); // Ending Balance
                    locind = dx.Rows[i]["LOCIND"].ObjToString();
                    location = "";
                    if (locind.IndexOf("02") > 0)
                        location = "2002";

                    beginningBalance = PRIORPPIT;
                    currentPayments = YTDPREV + CURRPPIT;
                    currentRemovals = PREVFUNRL + CURRFUNRL + PREVREFND + CURRREFND;
                    endingBalance = TOTPPITIN;

                    endingBalance = beginningBalance + interest + currentPayments - currentRemovals;

                    DataRow dRow = dt.NewRow();

                    dRow["payDate8"] = G1.DTtoMySQLDT(date.ToString("MM/dd/yyyy"));

                    dRow["contractNumber"] = contractNumber;
                    dRow["firstName"] = firstName;
                    dRow["lastName"] = lastName;
                    dRow["address2013"] = address;
                    dRow["city2013"] = city;
                    dRow["state2013"] = state;
                    dRow["zip2013"] = zip;
                    dRow["ssn2013"] = ssn;
                    dRow["ServiceId"] = serviceId;

                    dRow["beginningBalance"] = PRIORPPIT;
                    dRow["interest"] = interest;
                    dRow["ytdPrevious"] = YTDPREV;
                    dRow["paymentCurrMonth"] = CURRPPIT;
                    dRow["currentPayments"] = currentPayments;
                    dRow["deathRemYTDprevious"] = PREVFUNRL;
                    dRow["deathRemCurrMonth"] = CURRFUNRL;
                    dRow["refundRemYTDprevious"] = PREVREFND;
                    dRow["refundRemCurrMonth"] = CURRREFND;
                    dRow["currentRemovals"] = currentRemovals;
                    dRow["endingBalance"] = endingBalance;

                    dRow["location"] = loc;
                    dRow["filename"] = location;
                    dRow["locind"] = locind;
                    dt.Rows.Add(dRow);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("***Error*** " + ex.Message.ToString());
                }
            }
            btnSave.Show();
            dt.Columns.Add("num");

            if (G1.get_column_number(dt, "paulCurrentMonth") < 0)
                dt.Columns.Add("paulCurrentMonth", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "ragCurrentMonth") < 0)
                dt.Columns.Add("ragCurrentMonth", Type.GetType("System.Double"));
            //dt.Rows[0]["paulCurrentMonth"] = 1.23D;


            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            gridMain.Columns["payDate8"].Visible = false;
            gridMain.Columns["payDate8"].GroupIndex = -1;
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void PullCSV(DataTable dt, string file)
        {
            DateTime date = DateTime.Now;
            string contractNumber = "";
            string lastName = "";
            string firstName = "";
            double beginningBalance = 0D;
            double currentPayments = 0D;
            double currentRemovals = 0D;
            double endingBalance = 0D;

            string str = "";
            string location = G1.DecodeFilename(file, true );
            if (location.IndexOf("2002") > 0)
                txt2002.Text = "2002";
            else
                txt2002.Text = "";


            this.Cursor = Cursors.WaitCursor;

            DataTable dx = Import.ImportCSVfile(file, null);
            if ( G1.get_column_number ( dx, "REMOVECODE") > 0 )
            {
                PullPaulCSV(dt, dx, location);
                return;
            }
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                str = dx.Rows[i][1].ObjToString();
                if (String.IsNullOrWhiteSpace(str))
                    continue;
                if (str.Trim().ToUpper() == "CNUM")
                    continue;
                if (str.Trim().ToUpper() == "LAST NAME")
                    continue;
                if (str.Trim().ToUpper() == "FIRST NAME")
                    continue;
                try
                {
                    contractNumber = "";
                    lastName = "";
                    firstName = "";
                    beginningBalance = 0D;
                    currentPayments = 0D;
                    currentRemovals = 0D;
                    endingBalance = 0D;
                    DataRow dRow = dt.NewRow();
                    date = dx.Rows[i]["date"].ObjToDateTime();
                    dRow["payDate8"] = date.ToString("MM/dd/yyyy");
                    contractNumber = dx.Rows[i]["cnum"].ObjToString();
                    lastName = dx.Rows[i]["Last Name"].ObjToString();
                    firstName = dx.Rows[i]["First Name"].ObjToString();
                    beginningBalance = dx.Rows[i]["Beginning Balance"].ObjToDouble();
                    currentPayments = dx.Rows[i]["Current Payments"].ObjToDouble();
                    currentRemovals = dx.Rows[i]["Current Removals"].ObjToDouble();
                    endingBalance = dx.Rows[i]["Ending Balance"].ObjToDouble();
                    dRow["contractNumber"] = contractNumber;
                    dRow["firstName"] = firstName;
                    dRow["lastName"] = lastName;
                    dRow["beginningBalance"] = beginningBalance;
                    dRow["currentPayments"] = currentPayments;
                    dRow["currentRemovals"] = currentRemovals;
                    dRow["endingBalance"] = endingBalance;
                    dRow["location"] = location;
                    dt.Rows.Add(dRow);
                }
                catch (Exception ex)
                {
                }
            }
            btnSave.Show();
            dt.Columns.Add("num");
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void PullExcel(DataTable dt, string file)
        {
            Excel.Application xlApp;
            Excel.Workbook xlWorkBook;
            Excel.Worksheet xlWorkSheet;
            Excel.Range range;

            DateTime date = DateTime.Now;
            string location = G1.DecodeFilename(file, true);
            if (location.IndexOf("2002") > 0)
                txt2002.Text = "2002";
            else
                txt2002.Text = "";
            string str;
            int rCnt;
            int cCnt;
            int rw = 0;
            int cl = 0;

            this.Cursor = Cursors.WaitCursor;
            xlApp = new Excel.Application();
            xlWorkBook = xlApp.Workbooks.Open(file, 0, true, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);

            int lastRow = xlWorkBook.Worksheets.Count;
            barImport.Minimum = 0;
            barImport.Maximum = lastRow;
            lblTotal.Text = "of " + lastRow.ToString();
            lblTotal.Refresh();

            for (int i = 0; i < xlWorkBook.Worksheets.Count; i++)
            {
                barImport.Value = i;
                barImport.Refresh();
                lblMax.Text = i.ToString();
                lblMax.Refresh();

                xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(i + 1);
                string name = xlWorkSheet.Name.ObjToString();
                string[] Lines = name.Split(' ');
                if (Lines.Length > 1)
                    name = Lines[0].Trim();
                date = name.ObjToDateTime();
                name = date.ToString("MM/dd/yyyy");

                range = xlWorkSheet.UsedRange;
                rw = range.Rows.Count;
                cl = range.Columns.Count;
                string contractNumber = "";
                string lastName = "";
                string firstName = "";
                double beginningBalance = 0D;
                double currentPayments = 0D;
                double currentRemovals = 0D;
                double endingBalance = 0D;

                int lastRow2 = rw;
                barImport2.Minimum = 0;
                barImport2.Maximum = lastRow2;
                lblTotal2.Text = "of " + lastRow2.ToString();
                lblTotal2.Refresh();

                for (rCnt = 1; rCnt <= rw; rCnt++)
                {
                    barImport2.Value = rCnt;
                    barImport2.Refresh();
                    lblMax2.Text = rCnt.ToString();
                    lblMax2.Refresh();

                    str = (string)(range.Cells[rCnt, 1] as Excel.Range).Value2;
                    if (String.IsNullOrWhiteSpace(str))
                        continue;
                    if (str.Trim().ToUpper() == "CNUM")
                        continue;
                    if (str.Trim().ToUpper() == "LAST NAME")
                        continue;
                    if (str.Trim().ToUpper() == "FIRST NAME")
                        continue;
                    try
                    {
                        contractNumber = "";
                        lastName = "";
                        firstName = "";
                        beginningBalance = 0D;
                        currentPayments = 0D;
                        currentRemovals = 0D;
                        endingBalance = 0D;
                        DataRow dRow = dt.NewRow();
                        dRow["payDate8"] = name;
                        if ((range.Cells[rCnt, 1] as Excel.Range).Value2 != null)
                            contractNumber = (string)(range.Cells[rCnt, 1] as Excel.Range).Value2;
                        if ((range.Cells[rCnt, 2] as Excel.Range).Value2 != null)
                            lastName = (string)(range.Cells[rCnt, 2] as Excel.Range).Value2;
                        if ((range.Cells[rCnt, 3] as Excel.Range).Value2 != null)
                            firstName = (string)(range.Cells[rCnt, 3] as Excel.Range).Value2;
                        if ((range.Cells[rCnt, 4] as Excel.Range).Value2 != null)
                            beginningBalance = (double)(range.Cells[rCnt, 4] as Excel.Range).Value2;
                        if ((range.Cells[rCnt, 5] as Excel.Range).Value2 != null)
                            currentPayments = (double)(range.Cells[rCnt, 5] as Excel.Range).Value2;
                        if ((range.Cells[rCnt, 6] as Excel.Range).Value2 != null)
                            currentRemovals = (double)(range.Cells[rCnt, 6] as Excel.Range).Value2;
                        if ((range.Cells[rCnt, 7] as Excel.Range).Value2 != null)
                            endingBalance = (double)(range.Cells[rCnt, 7] as Excel.Range).Value2;
                        dRow["contractNumber"] = contractNumber;
                        dRow["firstName"] = firstName;
                        dRow["lastName"] = lastName;
                        dRow["beginningBalance"] = beginningBalance;
                        dRow["currentPayments"] = currentPayments;
                        dRow["currentRemovals"] = currentRemovals;
                        dRow["endingBalance"] = endingBalance;
                        dRow["location"] = location;
                        dt.Rows.Add(dRow);
                    }
                    catch (Exception ex)
                    {
                    }
                }

                Marshal.ReleaseComObject(xlWorkSheet);
            }
            xlWorkBook.Close(true, null, null);
            xlApp.Quit();
            Marshal.ReleaseComObject(xlWorkBook);
            Marshal.ReleaseComObject(xlApp);
            btnSave.Show();
            dt.Columns.Add("num");
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void btnSave_Click(object sender, EventArgs e)
        {
            DateTime date = DateTime.Now;
            string contractNumber = "";
            string lastName = "";
            string firstName = "";
            string address = "";
            string city = "";
            string state = "";
            string zip = "";
            string ssn = "";
            string serviceId = "";

            double beginningBalance = 0D;
            double currentPayments = 0D;
            double currentRemovals = 0D;
            double endingBalance = 0D;

            double interest = 0D;
            double ytdPrevious = 0D;
            double paymentCurrMonth = 0D;
            double deathRemYTDprevious = 0D;
            double deathRemCurrMonth = 0D;
            double refundRemYTDprevious = 0D;
            double refundRemCurrMonth = 0D;

            string text2002 = this.txt2002.Text;
            string location = "";
            string filename = "";
            string locind = "";

            string cmd = "";

            this.Cursor = Cursors.WaitCursor;
            lblReplace.Text = "0";
            int replaced = 0;
            bool dup = false;

            DataTable dt = (DataTable)dgv.DataSource;
            try
            {
                int lastRow = dt.Rows.Count;
                barImport.Minimum = 0;
                barImport.Maximum = lastRow;
                lblTotal.Text = "of " + lastRow.ToString();
                lblTotal.Refresh();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    barImport.Value = i;
                    barImport.Refresh();
                    lblMax.Text = i.ToString();
                    lblMax.Refresh();

                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                    location = dt.Rows[i]["location"].ObjToString();
                    locind = dt.Rows[i]["locind"].ObjToString();
                    text2002 = "";
                    if (locind.IndexOf("02") >= 0)
                        text2002 = "2002";

                    beginningBalance = dt.Rows[i]["beginningBalance"].ObjToDouble();
                    currentPayments = dt.Rows[i]["currentPayments"].ObjToDouble();
                    currentRemovals = dt.Rows[i]["currentRemovals"].ObjToDouble();
                    endingBalance = dt.Rows[i]["endingBalance"].ObjToDouble();

                    interest = dt.Rows[i]["interest"].ObjToDouble();
                    ytdPrevious = dt.Rows[i]["ytdPrevious"].ObjToDouble();
                    paymentCurrMonth = dt.Rows[i]["paymentCurrMonth"].ObjToDouble();
                    deathRemYTDprevious = dt.Rows[i]["deathRemYTDprevious"].ObjToDouble();
                    deathRemCurrMonth = dt.Rows[i]["deathRemCurrMonth"].ObjToDouble();
                    refundRemYTDprevious = dt.Rows[i]["refundRemYTDprevious"].ObjToDouble();
                    refundRemCurrMonth = dt.Rows[i]["refundRemCurrMonth"].ObjToDouble();
                    if ( beginningBalance == 0D && currentPayments == 0D && currentRemovals == 0D && endingBalance == 0D && interest == 0D)
                    {
                        if ( ytdPrevious == 0D && paymentCurrMonth == 0D && deathRemYTDprevious == 0D && deathRemCurrMonth == 0D)
                        {
                            if (refundRemYTDprevious == 0D && refundRemCurrMonth == 0D)
                                continue;
                        }
                    }

                    dup = false;

                    date = dt.Rows[i]["payDate8"].ObjToDateTime();
                    cmd = "Select * from `Trust2013` where `contractNumber` = '" + contractNumber + "' and `payDate8` = '" + date.ToString("yyyyMMdd") + "' ";
                    cmd += " and `locind` = '" + locind + "' ";
                    cmd += ";";
                    DataTable dx = G1.get_db_data(cmd);
                    string record = "";
                    if (dx.Rows.Count > 0)
                    {
                        record = dx.Rows[0]["record"].ObjToString();
                        replaced++;
                        dt.Rows[i]["dup"] = "Y";
                        dup = true;
                    }
                    else
                    {
                        record = G1.create_record("Trust2013", "firstName", "-1");
                        if (G1.BadRecord("Trust2013", record))
                            break;
                    }
                    firstName = dt.Rows[i]["firstName"].ObjToString();
                    lastName = dt.Rows[i]["lastName"].ObjToString();
                    address = dt.Rows[i]["address2013"].ObjToString();
                    city = dt.Rows[i]["city2013"].ObjToString();
                    state = dt.Rows[i]["state2013"].ObjToString();
                    zip = dt.Rows[i]["zip2013"].ObjToString();
                    ssn = dt.Rows[i]["ssn2013"].ObjToString();
                    serviceId = dt.Rows[i]["ServiceId"].ObjToString();

                    beginningBalance = dt.Rows[i]["beginningBalance"].ObjToDouble();
                    currentPayments = dt.Rows[i]["currentPayments"].ObjToDouble();
                    currentRemovals = dt.Rows[i]["currentRemovals"].ObjToDouble();
                    endingBalance = dt.Rows[i]["endingBalance"].ObjToDouble();

                    interest = dt.Rows[i]["interest"].ObjToDouble();
                    ytdPrevious = dt.Rows[i]["ytdPrevious"].ObjToDouble();
                    paymentCurrMonth = dt.Rows[i]["paymentCurrMonth"].ObjToDouble();
                    deathRemYTDprevious = dt.Rows[i]["deathRemYTDprevious"].ObjToDouble();
                    deathRemCurrMonth = dt.Rows[i]["deathRemCurrMonth"].ObjToDouble();
                    refundRemYTDprevious = dt.Rows[i]["refundRemYTDprevious"].ObjToDouble();
                    refundRemCurrMonth = dt.Rows[i]["refundRemCurrMonth"].ObjToDouble();

                    if ( dup)
                    {
                        beginningBalance += dx.Rows[0]["beginningBalance"].ObjToDouble();
                        currentPayments += dx.Rows[0]["currentPayments"].ObjToDouble();
                        currentRemovals += dx.Rows[0]["currentRemovals"].ObjToDouble();
                        endingBalance += dx.Rows[0]["endingBalance"].ObjToDouble();

                        interest += dx.Rows[0]["interest"].ObjToDouble();
                        ytdPrevious += dx.Rows[0]["ytdPrevious"].ObjToDouble();
                        paymentCurrMonth += dx.Rows[0]["paymentCurrMonth"].ObjToDouble();
                        deathRemYTDprevious += dx.Rows[0]["deathRemYTDprevious"].ObjToDouble();
                        deathRemCurrMonth += dx.Rows[0]["deathRemCurrMonth"].ObjToDouble();
                        refundRemYTDprevious += dx.Rows[0]["refundRemYTDprevious"].ObjToDouble();
                        refundRemCurrMonth += dx.Rows[0]["refundRemCurrMonth"].ObjToDouble();
                    }

                    filename = dt.Rows[i]["filename"].ObjToString();

                    G1.update_db_table("Trust2013", "record", record, new string[] { "contractNumber", contractNumber, "firstName", firstName, "lastName", lastName, "payDate8", date.ToString("yyyyMMdd"), "beginningBalance", beginningBalance.ToString(), "currentPayments", currentPayments.ToString(), "currentRemovals", currentRemovals.ToString(), "endingBalance", endingBalance.ToString(), "Is2002", text2002, "location", location, "filename", filename, "locind", locind });
                    G1.update_db_table("Trust2013", "record", record, new string[] { "interest", interest.ToString(), "ytdPrevious", ytdPrevious.ToString(), "paymentCurrMonth", paymentCurrMonth.ToString(), "deathRemYTDprevious", deathRemYTDprevious.ToString(), "deathRemCurrMonth", deathRemCurrMonth.ToString(), "refundRemYTDprevious", refundRemYTDprevious.ToString(), "refundRemCurrMonth", refundRemCurrMonth.ToString() });
                    G1.update_db_table("Trust2013", "record", record, new string[] { "address2013", address, "city2013", city, "state2013", state, "zip2013", zip, "ssn2013", ssn, "ServiceId", serviceId });
                }
                lblReplace.Text = replaced.ToString();
                MessageBox.Show("Import Finished");
            }
            catch ( Exception ex )
            {
                MessageBox.Show("***ERROR*** " + ex.Message.ToString());
            }
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
        private void btnCalcDiff_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            int lastRow = dt.Rows.Count;
            string contractNumber = "";
            barImport.Minimum = 0;
            barImport.Maximum = lastRow;
            lblTotal.Text = "of " + lastRow.ToString();
            lblTotal.Refresh();
            DateTime date = this.dateTimePicker1.Value;
            double Trust85Real = 0D;
            double Trust85Max = 0D;
            double Trust85Remaining = 0D;
            double myDiff = 0D;
            double endingBalance = 0D;
            double removals = 0D;
            double balanceDue = 0D;
            this.Cursor = Cursors.WaitCursor;
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                barImport.Value = i;
                barImport.Refresh();
                lblMax.Text = i.ToString();
                lblMax.Refresh();

                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                endingBalance = dt.Rows[i]["endingBalance"].ObjToDouble();
                if (endingBalance == 0D)
                    continue;
                removals = dt.Rows[i]["currentRemovals"].ObjToDouble();

                Trust85.CalcTrust85Total(contractNumber, date, ref Trust85Real, ref Trust85Max, ref balanceDue );

                Trust85Real -= removals;
                dt.Rows[i]["myBalance"] = Trust85Real;
                myDiff = endingBalance - Trust85Real;
                dt.Rows[i]["myDiff"] = myDiff;
                dt.Rows[i]["trust85Max"] = Trust85Max;
                Trust85Remaining = 0D;
                if ( endingBalance > 0D)
                    Trust85Remaining = Trust85Max - Trust85Real;
                dt.Rows[i]["trust85Remaining"] = Trust85Remaining;
            }
            barImport.Value = lastRow;
            barImport.Refresh();
            lblMax.Text = lastRow.ToString();
            lblMax.Refresh();

            dgv.DataSource = dt;
            dgv.Refresh();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string contract = dr["contractNumber"].ObjToString();
            this.Cursor = Cursors.WaitCursor;
            CustomerDetails clientForm = new CustomerDetails(contract);
            clientForm.Show();
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

            font = new Font("Ariel", 8);
            Printer.DrawGridDate(2, 3, 2, 3, Color.Black, BorderSide.None, font);
            Printer.DrawGridPage(11, 3, 2, 3, Color.Black, BorderSide.None, font);

            Printer.DrawQuad(1, 9, 2, 3, "User : " + LoginForm.username, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);

            font = new Font("Ariel", 10, FontStyle.Bold);
            string title = "Trust85P Totals Data from AS400";
            Printer.DrawQuad(6, 8, 4, 4, title, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);


            //            Printer.DrawQuad(20, 8, 5, 4, title + workDate, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(16, 8, 3, 4, lblPayment.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(19, 8, 3, 4, lblTrust85.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(22, 8, 3, 4, lblTrust100.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /****************************************************************************************/
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        /****************************************************************************************/
        private void gridMain_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
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
        /****************************************************************************************/
        private void gridMain_RowCellStyle(object sender, DevExpress.XtraGrid.Views.Grid.RowCellStyleEventArgs e)
        {
            //if (e.RowHandle < 0)
            //    return;
            //GridView View = sender as GridView;
            //if (e.Column.FieldName.ToUpper() == "NUM")
            //{
            //}
        }
        /****************************************************************************************/
        private void btnPaul_Click(object sender, EventArgs e)
        {
            Import paulImport = new Import("Paul's Data");
            paulImport.SelectDone += PaulImport_SelectDone;
            paulImport.Show();
        }
        /****************************************************************************************/
        private void PaulImport_SelectDone(DataTable dt)
        {
            if (dt == null)
                return;
            DataRow[] dRows = null;
            DataTable mDt = (DataTable)dgv.DataSource;
            if ( G1.get_column_number ( mDt, "paulCurrentMonth") < 0 )
                mDt.Columns.Add("paulCurrentMonth", Type.GetType("System.Double"));

            string cnum = "";
            double trust85 = 0D;
            double oldTrust85 = 0D;
            this.Cursor = Cursors.WaitCursor;
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                cnum = dt.Rows[i]["cnum"].ObjToString();
                if (String.IsNullOrWhiteSpace(cnum))
                    continue;
                try
                {
                    dRows = mDt.Select("contractNumber='" + cnum + "'");
                    if (dRows.Length > 0)
                    {
                        oldTrust85 = dRows[0]["paymentCurrMonth"].ObjToDouble();
                        trust85 = dt.Rows[i]["CURRPPIT"].ObjToDouble();
                        if (oldTrust85 != trust85)
                            dRows[0]["myDiff"] = trust85;
                    }
                }
                catch ( Exception ex)
                {
                }
            }
            dgv.DataSource = mDt;
            dgv.RefreshDataSource();
            gridMain.RefreshData();
            dgv.Refresh();
            gridMain.RefreshData();

            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void btnRAG_Click(object sender, EventArgs e)
        {

        }
        /****************************************************************************************/
    }
}