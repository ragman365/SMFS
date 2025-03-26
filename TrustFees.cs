using System;
using System.Windows.Forms;
using System.Data;
using System.Drawing;
using System.IO;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraPrinting;
using DevExpress.Utils;
using MySql.Data.MySqlClient;
using MySql.Data.Types;

using GeneralLib;
using DevExpress.XtraGrid.Views.Base;
using DocumentFormat.OpenXml.Office2013.PowerPoint.Roaming;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class TrustsFees : DevExpress.XtraEditors.XtraForm
    {
        private bool loading = true;
        private DataTable originalDt = null;
        /****************************************************************************************/
        public TrustsFees()
        {
            InitializeComponent();
            SetupTotalsSummary();
        }
        /****************************************************************************************/
        private void TrustsFees_Load(object sender, EventArgs e)
        {
            btnSave.Hide();
            lblTotal.Hide();
            barImport.Hide();
            labelMaximum.Hide();
            btnSaveDetail.Hide();

            DateTime now = DateTime.Now;
            now = now.AddMonths(-1);
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            this.dateTimePicker1.Value = new DateTime(now.Year, now.Month, days);

            string cmd = "Select * from `fees`;";
            DataTable dx = G1.get_db_data(cmd);
            dx.Columns.Add("salaryPerIssue", Type.GetType("System.Double"));
            dx.Columns.Add("trustFeePerContract", Type.GetType("System.Double"));
            dx.Columns.Add("displayDate");
            dx.Columns.Add("num");
            dx.Columns.Add("MOD");
            DateTime date = DateTime.Now;
            string str = "";
            double salary = 0D;
            double contractsIssued = 0D;
            double salaryPerIssue = 0D;
            double trustFee = 0D;
            double totalContracts = 0D;
            double trustFeePerContract = 0D;
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                salary = dx.Rows[i]["salary"].ObjToDouble();
                contractsIssued = dx.Rows[i]["contractsIssued"].ObjToDouble();
                if (contractsIssued > 0)
                {
                    salaryPerIssue = salary / contractsIssued;
                    salaryPerIssue = G1.RoundDown(salaryPerIssue);
                    if (i == dx.Rows.Count - 1)
                    {

                    }
                    //dx.Rows[i]["salaryPerIssue"] = salaryPerIssue * 100D;
                    dx.Rows[i]["salaryPerIssue"] = salaryPerIssue;
                }
                trustFee = dx.Rows[i]["trustFee"].ObjToDouble();
                totalContracts = dx.Rows[i]["totalContracts"].ObjToDouble();
                if (totalContracts > 0)
                {
                    trustFeePerContract = trustFee / totalContracts;
                    trustFeePerContract = G1.RoundDown(trustFeePerContract);
                    //                    dx.Rows[i]["trustFeePerContract"] = trustFeePerContract * 100D;
                    dx.Rows[i]["trustFeePerContract"] = trustFeePerContract;
                }
                date = dx.Rows[i]["date"].ObjToDateTime();
                str = date.ToString("MM/dd/yyyy");
                dx.Rows[i]["displayDate"] = str;
                dx.Rows[i]["MOD"] = "P";
            }
            if (dx.Rows.Count > 0)
                btnImport.Hide();
            originalDt = dx.Copy();
            G1.NumberDataTable(dx);
            dgv2.DataSource = dx;
            loading = false;
        }
        /****************************************************************************************/
        private void SetupTotalsSummary()
        {
            AddSummaryColumn("commission");
            AddSummaryColumn("salary");
            AddSummaryColumn("fees");
            AddSummaryColumn("salary", gridMain2);
            AddSummaryColumn("trustFee", gridMain2);
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
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Column " + columnName + " " + ex.Message.ToString());
            }
        }
        /****************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (dgv.Visible)
            {
                if (gridMain.OptionsFind.AlwaysVisible == true)
                    gridMain.OptionsFind.AlwaysVisible = false;
                else
                    gridMain.OptionsFind.AlwaysVisible = true;
            }
            else if ( dgv3.Visible )
            {
                if (gridMain3.OptionsFind.AlwaysVisible == true)
                    gridMain3.OptionsFind.AlwaysVisible = false;
                else
                    gridMain3.OptionsFind.AlwaysVisible = true;
            }
        }
        /****************************************************************************************/
        private void btnImport_Click(object sender, EventArgs e)
        {
            loading = true;
            DataTable dx = (DataTable)dgv2.DataSource;
            dx.Rows.Clear();
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string file = ofd.FileName;
                    dgv2.DataSource = null;
                    DataTable dt = Import.ImportCSVfile(file);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        string str = "";
                        string year = "";
                        string month = "";
                        int idays = 0;
                        int iYear = 0;
                        int iMonth = 0;
                        DateTime date = DateTime.Now;
                        string salary = "";
                        string contractsIssued = "";
                        string salaryPerIssue = "";
                        string trustFee = "";
                        string totalContracts = "";
                        string trustFeePerContract = "";
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            str = dt.Rows[i]["Year-month (YYYYMM)"].ObjToString();
                            if (str.Length < 6)
                                continue;
                            year = str.Substring(0, 4);
                            month = str.Substring(4, 2);
                            iYear = year.ObjToInt32();
                            iMonth = month.ObjToInt32();
                            idays = DateTime.DaysInMonth(iYear, iMonth);
                            date = new DateTime(iYear, iMonth, idays);
                            salary = dt.Rows[i]["Salary"].ObjToString();
                            contractsIssued = dt.Rows[i]["Contracts Issued"].ObjToString();
                            salaryPerIssue = dt.Rows[i]["Salary/Issued"].ObjToString();
                            trustFee = dt.Rows[i]["Trust Fee"].ObjToString();
                            totalContracts = dt.Rows[i]["Total Contracts"].ObjToString();
                            trustFeePerContract = dt.Rows[i]["Trust Fee/Contracts"].ObjToString();
                            DataRow dR = dx.NewRow();
                            dR["date"] = G1.DTtoMySQLDT(date);
                            dR["displayDate"] = date.ToString("MM/dd/yyyy");
                            dR["salary"] = salary.ObjToDouble();
                            dR["contractsIssued"] = contractsIssued.ObjToDouble();
                            dR["salaryPerIssue"] = salaryPerIssue.ObjToDouble();
                            dR["trustFee"] = trustFee.ObjToDouble();
                            dR["totalContracts"] = totalContracts.ObjToDouble();
                            dR["trustFeePerContract"] = trustFeePerContract.ObjToDouble();
                            dR["MOD"] = "P";
                            dx.Rows.Add(dR);
                        }
                        G1.NumberDataTable(dx);
                        dgv2.DataSource = dx;
                        originalDt = dx.Copy();
                        btnSave.Show();
                        btnImport.Hide();
                        panelDataTop.Refresh();
                    }
                }
            }
            loading = false;
        }
        /****************************************************************************************/
        private void btnLeft_Click(object sender, EventArgs e)
        {
            DateTime now = this.dateTimePicker1.Value;
            DateTime date = new DateTime(now.Year, now.Month, 1);
            date = date.AddMonths(-1);
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            date = new DateTime(date.Year, date.Month, days);
            this.dateTimePicker1.Value = date;
            this.dateTimePicker1.Refresh();
        }
        /****************************************************************************************/
        private void btnRight_Click(object sender, EventArgs e)
        {
            DateTime date = this.dateTimePicker1.Value;
            date = date.AddMonths(1);
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            date = new DateTime(date.Year, date.Month, days);
            this.dateTimePicker1.Value = date;
            this.dateTimePicker1.Refresh();
        }
        /****************************************************************************************/
        private void gridMain2_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
        {
            if (loading)
                return;
            if (e.RowHandle < 0)
                return;
            int row = gridMain2.GetDataSourceRowIndex(e.RowHandle);
            if (row < 0)
                return;
            DataTable dt = (DataTable)dgv2.DataSource;
            if (e.Column.FieldName.ToUpper() == "DISPLAYDATE")
            {
                if (e.RowHandle >= 0)
                {
                    DateTime date = e.DisplayText.ObjToDateTime();
                    e.DisplayText = date.Month.ToString("D2") + "/" + date.Day.ToString("D2") + "/" + date.Year.ToString("D4");
                    if (date.Year == 1)
                        e.DisplayText = "";
                }
            }
            else if (e.Column.FieldName.ToUpper() == "SALARYPERISSUE")
            {
                if (e.RowHandle >= 0)
                {
                    double salary = dt.Rows[row]["salary"].ObjToDouble();
                    double contractsIssued = dt.Rows[row]["contractsIssued"].ObjToDouble();
                    if (contractsIssued > 0)
                    {
                        double salaryPerIssue = salary / contractsIssued;
                        salaryPerIssue = G1.RoundDown(salaryPerIssue);
                        dt.Rows[row]["salaryPerIssue"] = salaryPerIssue;
                    }
                    else
                        dt.Rows[row]["salaryPerIssue"] = 0D;
                }
            }
            else if (e.Column.FieldName.ToUpper() == "TRUSTFEEPERCONTRACT")
            {
                if (e.RowHandle >= 0)
                {
                    double trustFee = dt.Rows[row]["trustFee"].ObjToDouble();
                    double totalContracts = dt.Rows[row]["totalContracts"].ObjToDouble();
                    if (totalContracts > 0)
                    {
                        double trustFeePerContract = trustFee / totalContracts;
                        trustFeePerContract = G1.RoundDown(trustFeePerContract);
                        dt.Rows[row]["trustFeePerContract"] = trustFeePerContract;
                    }
                    else
                        dt.Rows[row]["trustFeePerContract"] = 0D;
                }
            }
        }
        /****************************************************************************************/
        private void pictureAdd_Click(object sender, EventArgs e)
        {
            DataTable dx = (DataTable)dgv2.DataSource;
            DataRow dR = dx.NewRow();
            dR["salaryPerIssue"] = 0D;
            dR["trustFeePerContract"] = 0D;
            dx.Rows.Add(dR);
            G1.NumberDataTable(dx);
            int count = dx.Rows.Count - 1;
            dgv2.RefreshDataSource();
            dgv2.Refresh();
            gridMain2.SelectRow(count);
            gridMain2.FocusedRowHandle = count;
        }
        /****************************************************************************************/
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            TabPage current = (sender as TabControl).SelectedTab;
            if (current.Name.Trim().ToUpper() == "TABSALARY")
            {
                panelDataTop.Refresh();
                pictureAdd.Refresh();
                btnSave.Refresh();
            }
        }
        /****************************************************************************************/
        private void btnSave_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            DataTable dt = (DataTable)dgv2.DataSource;
            DateTime date = DateTime.Now;
            string record = "";
            double salary = 0D;
            double contractsIssued = 0D;
            double salaryPerIssue = 0D;
            double trustFee = 0D;
            double totalContracts = 0D;
            double trustFeePerContract = 0D;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                record = dt.Rows[i]["record"].ObjToString();
                date = dt.Rows[i]["displayDate"].ObjToDateTime();
                MySqlDateTime myDate = (MySqlDateTime)G1.DTtoMySQLDT(date);
                salary = dt.Rows[i]["salary"].ObjToDouble();
                contractsIssued = dt.Rows[i]["contractsIssued"].ObjToDouble();
                salaryPerIssue = dt.Rows[i]["salaryPerIssue"].ObjToDouble();
                trustFee = dt.Rows[i]["trustFee"].ObjToDouble();
                totalContracts = dt.Rows[i]["totalContracts"].ObjToDouble();
                trustFeePerContract = dt.Rows[i]["trustFeePerContract"].ObjToDouble();
                if (String.IsNullOrWhiteSpace(record))
                {
                    record = G1.create_record("fees", "salary", "-1");
                    if (G1.BadRecord("fees", record))
                        break;
                    dt.Rows[i]["record"] = record;
                    dt.Rows[i]["MOD"] = "P";
                    G1.update_db_table("fees", "record", record, new string[] { "date", myDate.ToString(), "salary", salary.ToString(), "contractsIssued", contractsIssued.ToString(), "trustFee", trustFee.ToString(), "totalContracts", totalContracts.ToString() });
                }
                else
                {
                    if (LoginForm.administrator)
                        G1.update_db_table("fees", "record", record, new string[] { "date", myDate.ToString(), "salary", salary.ToString(), "contractsIssued", contractsIssued.ToString(), "trustFee", trustFee.ToString(), "totalContracts", totalContracts.ToString() });
                }
            }
            btnSave.Hide();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void gridMain2_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            int row = gridMain2.GetDataSourceRowIndex(e.RowHandle);
            if (row < 0)
                return;
            DataTable dx = (DataTable)dgv2.DataSource;
            string mod = dx.Rows[row]["MOD"].ObjToString();
            if (mod == "P" && !LoginForm.administrator)
            {
                //                MessageBox.Show("***ERROR*** You CANNOT modify old Data!");
                dx.Rows[row][e.Column.FieldName] = originalDt.Rows[row][e.Column.FieldName];
                dgv2.RefreshDataSource();
                dgv2.Refresh();
                return;
            }
            btnSave.Show();
            btnSave.Refresh();
            if (e.Column.FieldName.Trim().ToUpper() == "DISPLAYDATE")
            {
                string sDate = dx.Rows[row]["displayDate"].ObjToString();
                if (G1.validate_date(sDate))
                {
                    this.Cursor = Cursors.WaitCursor;
                    DateTime date = sDate.ObjToDateTime();
                    DateTime startDate = new DateTime(date.Year, date.Month, 1);
                    int days = DateTime.DaysInMonth(date.Year, date.Month);
                    DateTime stopDate = new DateTime(date.Year, date.Month, days);
                    string start = G1.date_to_sql(startDate.ToString("MM/dd/yyyy"));
                    string stop = G1.date_to_sql(stopDate.ToString("MM/dd/yyyy"));

                    string cmd = "Select * from `contracts` where `issueDate8` >= '" + start + "' AND `issueDate8` <= '" + stop + "';";
                    DataTable dt = G1.get_db_data(cmd);

                    //int totalContracts = GetTotalActiveContracts(stopDate, start, stop);

                    int lastTotalContracts = getLastMonthTotalContracts();
                    int contractIssued = dt.Rows.Count;
                    int deceased = getRecentDeceased();
                    int lapsed = getRecentLapsed();
                    int reinstated = getRecentReinstated();
                    int totalContracts = lastTotalContracts + contractIssued - deceased - lapsed + reinstated;

                    dx.Rows[row]["contractsIssued"] = (double)(contractIssued);
                    dx.Rows[row]["totalContracts"] = (double)(totalContracts);
                    dgv2.RefreshDataSource();
                    dgv2.Refresh();
                    this.Cursor = Cursors.Default;
                }
            }
            else if (e.Column.FieldName.Trim().ToUpper() == "SALARY")
            {
                double salary = dx.Rows[row]["salary"].ObjToDouble();
                double contractsIssued = dx.Rows[row]["contractsIssued"].ObjToDouble();
                if (contractsIssued > 0)
                {
                    double salaryPerIssue = salary / contractsIssued;
                    dx.Rows[row]["salaryPerIssue"] = salaryPerIssue;
                }
                else
                    dx.Rows[row]["salaryPerIssue"] = 0D;
                dgv2.RefreshDataSource();
                dgv2.Refresh();
            }
            else if (e.Column.FieldName.Trim().ToUpper() == "TRUSTFEE")
            {
                double trustFee = dx.Rows[row]["trustFee"].ObjToDouble();
                double totalContracts = dx.Rows[row]["totalContracts"].ObjToDouble();
                if (totalContracts > 0)
                {
                    double trustFeePerContract = trustFee / totalContracts;
                    dx.Rows[row]["trustFeePerContract"] = trustFeePerContract;
                }
                else
                    dx.Rows[row]["trustFeePerContract"] = 0D;
                dgv2.RefreshDataSource();
                dgv2.Refresh();
            }
        }
        /****************************************************************************************/
        private int GetTotalActiveContracts(DateTime dateIn, string start, string stop)
        {
            //string date = dateIn.ToString("yyyyMMdd");

            //string cmd = "SELECT * FROM contracts WHERE (`serviceTotal` + `merchandiseTotal` - `allowMerchandise` - `allowInsurance`) > '0' ";
            //cmd += " AND ( `serviceTotal` > '0' OR `merchandiseTotal` > '0') ";
            //cmd += " AND `issueDate8` <= '" + date + "' AND `deceasedDate` < '1850-01-01' ";
            //cmd += " AND `lapsed` <> 'Y' ";
            //cmd += " AND `contractNumber` NOT LIKE 'A%' ";
            //cmd += ";";

            //DataTable dt = G1.get_db_data(cmd);

            //int totalContracts = dt.Rows.Count;

            //cmd = "Select * from `contracts` where `deceasedDate` >= '" + start + "' AND `deceasedDate` <= '" + stop + "';";
            //dt = G1.get_db_data(cmd);

            //totalContracts = totalContracts - dt.Rows.Count;

            int totalContracts = getLastMonthTotalContracts();
            int deceased = getRecentDeceased();
            int lapsed = getRecentLapsed();
            int reinstated = getRecentReinstated();

            totalContracts += 562 + 174;

            return totalContracts;
        }
        /****************************************************************************************/
        private int getLastMonthTotalContracts()
        {
            int totalContracts = 0;
            DateTime date = this.dateTimePicker1.Value;
            date = date.AddMonths(-1);
            DateTime startDate = new DateTime(date.Year, date.Month, 1);
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            DateTime stopDate = new DateTime(date.Year, date.Month, days);
            string start = G1.date_to_sql(startDate.ToString("MM/dd/yyyy"));
            string stop = G1.date_to_sql(stopDate.ToString("MM/dd/yyyy"));

            string cmd = "Select * from `fees` where `date` = '" + stop + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                double totalContract = dt.Rows[0]["totalContracts"].ObjToDouble();
                totalContracts = Convert.ToInt32(totalContract);
            }
            return totalContracts;
        }
        /****************************************************************************************/
        private bool getRecentDetails(ref double salary, ref double fees)
        {
            bool rtn = true;
            salary = 0D;
            fees = 0D;
            DateTime date = this.dateTimePicker1.Value;
            DateTime startDate = new DateTime(date.Year, date.Month, 1);
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            DateTime stopDate = new DateTime(date.Year, date.Month, days);
            string start = G1.date_to_sql(startDate.ToString("MM/dd/yyyy"));
            string stop = G1.date_to_sql(stopDate.ToString("MM/dd/yyyy"));

            stop = stopDate.ToString("MM/dd/yyyy");
            //int m = stopDate.Month;
            //int y = stopDate.Year;

            //stop = m.ToString() + "/" + days.ToString() + "/" + y.ToString("D4");

            DataTable dt = (DataTable)dgv2.DataSource;
            if (dt == null)
                return false;
            if (dt.Rows.Count <= 0)
                return false;
            try
            {
                DataRow[] dRow = dt.Select("displayDate='" + stop + "'");
                if (dRow.Length > 0)
                {
                    salary = dRow[0]["salaryPerIssue"].ObjToDouble();
                    salary = G1.RoundDown(salary);
                    fees = dRow[0]["trustFeePerContract"].ObjToDouble();
                    fees = G1.RoundDown(fees);
                }
            }
            catch (Exception ex)
            {

            }
            return rtn;
        }
        /****************************************************************************************/
        private int getRecentDeceased()
        {
            int deceased = 0;
            DateTime date = this.dateTimePicker1.Value;
            DateTime startDate = new DateTime(date.Year, date.Month, 1);
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            DateTime stopDate = new DateTime(date.Year, date.Month, days);
            string start = G1.date_to_sql(startDate.ToString("MM/dd/yyyy"));
            string stop = G1.date_to_sql(stopDate.ToString("MM/dd/yyyy"));

            string cmd = "Select * from `contracts` where `deceasedDate` >= '" + start + "' AND `deceasedDate` <= '" + stop + "';";
            DataTable dt = G1.get_db_data(cmd);
            deceased = dt.Rows.Count;
            return deceased;
        }
        /****************************************************************************************/
        private int getRecentLapsed()
        {
            int lapsed = 0;
            DateTime date = this.dateTimePicker1.Value;
            date = date.AddMonths(1);
            DateTime startDate = new DateTime(date.Year, date.Month, 1);
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            DateTime stopDate = new DateTime(date.Year, date.Month, days);
            string start = G1.date_to_sql(startDate.ToString("MM/dd/yyyy"));
            string stop = G1.date_to_sql(stopDate.ToString("MM/dd/yyyy"));

            string cmd = "Select * from `contracts` where `lapseDate8` >= '" + start + "' AND `lapseDate8` <= '" + stop + "';";
            DataTable dt = G1.get_db_data(cmd);
            lapsed = dt.Rows.Count;
            return lapsed;
        }
        /****************************************************************************************/
        private int getRecentReinstated()
        {
            int reinstated = 0;
            DateTime date = this.dateTimePicker1.Value;
            date = date.AddMonths(1);
            DateTime startDate = new DateTime(date.Year, date.Month, 1);
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            DateTime stopDate = new DateTime(date.Year, date.Month, days);
            string start = G1.date_to_sql(startDate.ToString("MM/dd/yyyy"));
            string stop = G1.date_to_sql(stopDate.ToString("MM/dd/yyyy"));

            string cmd = "Select * from `contracts` where `reinstateDate8` >= '" + start + "' AND `reinstateDate8` <= '" + stop + "';";
            DataTable dt = G1.get_db_data(cmd);
            reinstated = dt.Rows.Count;
            return reinstated;
        }
        /****************************************************************************************/
        private void GetTotalExpenseInfo(ref double totalSalary, ref double totalFee, ref double contractsIssued, ref double salaryPerIssue, ref double totalContracts, ref double trustFeePerContract)
        {
            DataTable dt = (DataTable)dgv2.DataSource;
            DateTime date = DateTime.Now;
            double salary = 0D;
            contractsIssued = 0D;
            salaryPerIssue = 0D;
            double trustFee = 0D;
            totalContracts = 0D;
            trustFeePerContract = 0D;

            totalSalary = 0D;
            totalFee = 0D;

            DateTime reportDate = this.dateTimePicker1.Value;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                date = dt.Rows[i]["displayDate"].ObjToDateTime();
                salary = dt.Rows[i]["salary"].ObjToDouble();
                trustFee = dt.Rows[i]["trustFee"].ObjToDouble();
                totalSalary += salary;
                totalFee += trustFee;
                if (date == reportDate)
                {
                    contractsIssued = dt.Rows[i]["contractsIssued"].ObjToDouble();
                    salaryPerIssue = dt.Rows[i]["salaryPerIssue"].ObjToDouble();
                    totalContracts = dt.Rows[i]["totalContracts"].ObjToDouble();
                    trustFeePerContract = dt.Rows[i]["trustFeePerContract"].ObjToDouble();
                    break;
                }
            }
            totalSalary = G1.RoundDown(totalSalary);
            totalFee = G1.RoundDown(totalFee);
            contractsIssued = G1.RoundDown(contractsIssued);
            salaryPerIssue = G1.RoundDown(salaryPerIssue);
            totalContracts = G1.RoundDown(totalContracts);
            trustFeePerContract = G1.RoundDown(trustFeePerContract);
        }
        /****************************************************************************************/
        private DataTable GetLastMonthData()
        {
            DateTime date = dateTimePicker1.Value;
            date = date.AddMonths(-1);
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            string date1 = date.Year.ToString("D4") + "-" + date.Month.ToString("D2") + "-" + days.ToString("D2") + " 00:00:00 ";
            string cmd = "Select * from `fee_contracts` where `date` = '" + date1 + "' ";
            cmd += ";";

            DataTable dt = G1.get_db_data(cmd);
            if ( dt.Rows.Count <= 0 )
            {
                MessageBox.Show("It appears data from the previous month was not saved!\nGo back to the previous month, run it again, and save the data.", "Data Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return dt;
            }

            //Trust85.FindContract(dt, "HT20003L");

            AddNewContracts(dt);

            G1.NumberDataTable(dt);
            return dt;
        }
        /****************************************************************************************/
        private void runNew()
        {
            this.Cursor = Cursors.WaitCursor;
            DataTable dx = GetLastMonthData();

            string startDate = this.dateTimePicker1.Value.ToString("yyyyMMdd");
            string cmd = "Select * from `contracts` c LEFT JOIN `customers` d on c.`contractNumber` = d.`contractNumber` where c.`contractNumber` NOT LIKE 'A%' AND `issueDate8` <= '" + startDate + "' ORDER BY c.`contractNumber`;";
            DataTable dt = G1.get_db_data(cmd);

            double contractsIssued = 0D;
            double salaryPerIssue = 0D;
            double totalContracts = 0D;
            double trustFeePerContract = 0D;

            double totalSalary = 0D;
            double totalFee = 0D;

            DateTime paidOffDate = new DateTime(2039, 12, 31);

            getRecentDetails(ref salaryPerIssue, ref trustFeePerContract);

            string contractNumber = "";
            double commission = 0D;
            double totalCommission = 0D;
            double contractValue = 0D;
            string status = "";

            string reportDate = this.dateTimePicker1.Value.ToString("MM/dd/yyyy");



            DateTime lapseDate = DateTime.Now;
            DateTime reinstateDate = DateTime.Now;
            DateTime deceasedDate = DateTime.Now;
            DateTime dueDate8 = DateTime.Now;

            DataTable paymentDt = null;
            double oldPaidIn = 0D;
            double oldCommission = 0D;
            double oldContractValue = 0D;
            string oldStatus = "";

            double totalPaid = 0D;
            double percentage = 0D;
            double payment = 0D;
            double interest = 0D;
            double debit = 0D;
            double credit = 0D;
            double principal = 0D;
            double downPayment = 0D;
            string lname = "";
            string lapsed = "";
            string delFlag = "";
            string contractNumber1 = "";

            double fees = 0D;

            int lastrow = dx.Rows.Count;

            lblTotal.Show();

            lblTotal.Text = "of " + lastrow.ToString();
            lblTotal.Refresh();

            barImport.Show();
            barImport.Minimum = 0;
            barImport.Maximum = lastrow;
            labelMaximum.Show();

            DataRow[] dRows = null;

            for (int i = 0; i < lastrow; i++)
            {
                try
                {
                    Application.DoEvents();

                    barImport.Value = i;
                    barImport.Refresh();
                    labelMaximum.Text = i.ToString();
                    labelMaximum.Refresh();
                    //if (dx.Rows[i]["NewContract"].ObjToString() != "Y")
                    //    continue;

                    contractNumber = dx.Rows[i]["contractNumber"].ObjToString();
                    if (contractNumber == "HT20003L")
                    {
                    }
                    lname = dx.Rows[i]["lastName"].ObjToString();
                    if (String.IsNullOrWhiteSpace(lname))
                        dx.Rows[i]["lastName"] = "NAME NOT FOUND";

                    if (dx.Rows[i]["NewContract"].ObjToString() == "Y")
                        dx.Rows[i]["salary"] = salaryPerIssue;

                    dx.Rows[i]["date"] = G1.DTtoMySQLDT(reportDate);

                    contractNumber = dx.Rows[i]["contractNumber"].ObjToString();
                    if (contractNumber == "L14082UI")
                    {
                    }
                    if (contractNumber == "39360")
                    {
                    }

                    oldPaidIn = dx.Rows[i]["paidIn"].ObjToDouble();
                    oldCommission = dx.Rows[i]["commission"].ObjToDouble();
                    oldContractValue = dx.Rows[i]["contractValue"].ObjToDouble();
                    oldStatus = dx.Rows[i]["status"].ObjToString();

                    dRows = dt.Select("contractNumber='" + contractNumber + "'");
                    if (dRows.Length <= 0)
                    { // Just let it go as is
                        contractValue = dx.Rows[i]["contractValue"].ObjToDouble();
                        totalPaid = dx.Rows[i]["paidIn"].ObjToDouble();
                        if (contractValue > 0D)
                        {
                            percentage = totalPaid / contractValue;
                            if (percentage > 1D)
                                percentage = 1D;
                            dx.Rows[i]["paidInPercent"] = percentage;
                            //commission = totalPaid * 0.05D;
                            //dx.Rows[i]["commission"] = commission;

                            if (oldStatus.ToUpper() == "ACTIVE")
                            {
                                fees = dx.Rows[i]["fees"].ObjToDouble();
                                fees += trustFeePerContract;
                                dx.Rows[i]["fees"] = fees;
                            }
                        }
                        continue;
                    }

                    delFlag = dRows[0]["deleteFlag"].ObjToString();
                    contractNumber1 = dRows[0]["contractNumber1"].ObjToString();
                    dueDate8 = dRows[0]["dueDate8"].ObjToDateTime();
                    if (dueDate8 > paidOffDate)
                        dueDate8 = paidOffDate;
                    status = "ACTIVE";
                    lapsed = dRows[0]["lapsed"].ObjToString();
                    lapseDate = dRows[0]["lapseDate8"].ObjToDateTime();
                    reinstateDate = dRows[0]["reinstateDate8"].ObjToDateTime();
                    deceasedDate = dRows[0]["deceasedDate"].ObjToDateTime();
                    if (deceasedDate.Year > 1900)
                    {
                        status = "DECEASED";
                        dx.Rows[i]["lapseDate"] = G1.DTtoMySQLDT(deceasedDate);
                        dx.Rows[i]["status"] = status;
                        if (oldStatus.ToUpper() == "ACTIVE" && status.ToUpper() == "DECEASED")
                        {
                            fees = dx.Rows[i]["fees"].ObjToDouble();
                            fees -= trustFeePerContract;
                            if (fees < 0D)
                                fees = 0D;
                            dx.Rows[i]["fees"] = fees;
                            dx.Rows[i]["status"] = status;
                            continue;
                        }
                    }

                    if ( dueDate8 == paidOffDate && dx.Rows[i]["NewContract"].ObjToString() != "Y")
                    {
                        if ( oldPaidIn == oldContractValue )
                        { // Just let it go as is
                            contractValue = dx.Rows[i]["contractValue"].ObjToDouble();
                            totalPaid = dx.Rows[i]["paidIn"].ObjToDouble();
                            if (contractValue > 0D)
                            {
                                percentage = totalPaid / contractValue;
                                if (percentage > 1D)
                                    percentage = 1D;
                                dx.Rows[i]["paidInPercent"] = percentage;
                                //commission = totalPaid * 0.05D;
                                //dx.Rows[i]["commission"] = commission;
                                if ( oldStatus.ToUpper() == "ACTIVE" && status.ToUpper() == "DECEASED")
                                {
                                    fees = dx.Rows[i]["fees"].ObjToDouble();
                                    fees -= trustFeePerContract;
                                    if (fees < 0D)
                                        fees = 0D;
                                    dx.Rows[i]["fees"] = fees;
                                    dx.Rows[i]["status"] = status;
                                    continue;
                                }

                                if (oldStatus.ToUpper() == "ACTIVE")
                                {
                                    fees = dx.Rows[i]["fees"].ObjToDouble();
                                    fees += trustFeePerContract;
                                    dx.Rows[i]["fees"] = fees;
                                }
                            }
                            continue;
                        }
                    }
                    //if (1 == 1)
                    //    continue;

                    status = "ACTIVE";
                    lapsed = dRows[0]["lapsed"].ObjToString();
                    lapseDate = dRows[0]["lapseDate8"].ObjToDateTime();
                    reinstateDate = dRows[0]["reinstateDate8"].ObjToDateTime();
                    deceasedDate = dRows[0]["deceasedDate"].ObjToDateTime();
                    if (deceasedDate.Year > 1900)
                    {
                        status = "DECEASED";
                        //dx.Rows[i]["lapsedate"] = deceasedDate.ToString("MM/dd/yyyy");
                        dx.Rows[i]["lapseDate"] = G1.DTtoMySQLDT(deceasedDate);
                    }
                    else
                    {
                        if (lapsed.ToUpper() == "Y")
                        {
                            if (lapseDate.Year > 1900)
                            {
                                if (reinstateDate.Year > 1900)
                                {
                                    if (lapseDate > reinstateDate)
                                        status = "LAPSED";
                                }
                                else
                                    status = "LAPSED";
                                if (status == "LAPSED")
                                {
                                    //dx.Rows[i]["lapsedate"] = lapseDate.ToString("MM/dd/yyyy");
                                    dx.Rows[i]["lapseDate"] = G1.DTtoMySQLDT(lapseDate);
                                }
                            }
                        }
                    }
                    dx.Rows[i]["status"] = status;

                    //                    contractNumber = dx.Rows[i]["contractNumber"].ObjToString();
                    contractValue = DailyHistory.GetContractValuePlus(contractNumber);
                    dx.Rows[i]["contractValue"] = contractValue;
                    if (contractValue > 0D)
                    {
                        if (status.ToUpper() == "ACTIVE" )
                        {
                            fees = dx.Rows[i]["fees"].ObjToDouble();
                            fees += trustFeePerContract;
                            dx.Rows[i]["fees"] = fees;
                        }
                    }

                    cmd = "Select * from `payments` where `contractNumber` = '" + contractNumber + "' AND `payDate8` <= '" + startDate + "' order by `payDate8`;";
                    paymentDt = G1.get_db_data(cmd);

                    downPayment = dRows[0]["downPayment"].ObjToDouble();
                    totalPaid = downPayment;

                    for (int j = 0; j < paymentDt.Rows.Count; j++)
                    {
                        status = paymentDt.Rows[j]["fill"].ObjToString();
                        if (status.ToUpper() == "D")
                            continue;
                        downPayment = paymentDt.Rows[j]["downPayment"].ObjToDouble();
                        if (downPayment > 0D)
                            continue;
                        payment = paymentDt.Rows[j]["paymentAmount"].ObjToDouble();
                        debit = paymentDt.Rows[j]["debitAdjustment"].ObjToDouble();
                        credit = paymentDt.Rows[j]["creditAdjustment"].ObjToDouble();
                        interest = paymentDt.Rows[j]["interestPaid"].ObjToDouble();
                        principal = payment - interest + credit - debit;
                        totalPaid += principal;
                    }

                    if (String.IsNullOrWhiteSpace(contractNumber1))
                    { // Must have a contract without a customer record. So, just let it go through as is.
                        continue;
                    }
                    dx.Rows[i]["paidIn"] = totalPaid;
                    if (contractValue > 0D)
                    {
                        percentage = totalPaid / contractValue;
                        dx.Rows[i]["paidInPercent"] = percentage;

                        commission = totalPaid * 0.05D;
                        dx.Rows[i]["commission"] = commission;
                    }
                }
                catch (Exception ex)
                {

                }
            }
            barImport.Value = lastrow;
            barImport.Refresh();
            labelMaximum.Text = lastrow.ToString();
            labelMaximum.Refresh();

            DataView tempview = dx.DefaultView;
            tempview.Sort = "status asc, contractNumber asc";
            dx = tempview.ToTable();


            G1.NumberDataTable(dx);
            dgv.DataSource = dx;
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void btnRun_Click(object sender, EventArgs e)
        {
            if (1 == 1)
            {
                this.tabControl1.SelectedIndex = 0;
                dgv2.Visible = false;
                dgv.Visible = true;
                this.Refresh();

                runNew();
                btnSaveDetail.Show();
                return;
            }

            this.Cursor = Cursors.WaitCursor;

            DataTable dx = GetLastMonthData();

            string startDate = this.dateTimePicker1.Value.ToString("yyyyMMdd");
            string cmd = "Select * from `contracts` c LEFT JOIN `customers` d on c.`contractNumber` = d.`contractNumber` where c.`contractNumber` NOT LIKE 'A%' AND `issueDate8` <= '" + startDate + "' ORDER BY c.`contractNumber`;";
            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("num");
            dt.Columns.Add("date");
            dt.Columns.Add("status");
            dt.Columns.Add("commission", Type.GetType("System.Double"));
            dt.Columns.Add("salary", Type.GetType("System.Double"));
            dt.Columns.Add("fees", Type.GetType("System.Double"));
            dt.Columns.Add("lapsedate");
            dt.Columns.Add("contractValue", Type.GetType("System.Double"));
            dt.Columns.Add("paidIn", Type.GetType("System.Double"));
            dt.Columns.Add("paidInPercent", Type.GetType("System.Double"));

            double contractsIssued = 0D;
            double salaryPerIssue = 0D;
            double totalContracts = 0D;
            double trustFeePerContract = 0D;

            double totalSalary = 0D;
            double totalFee = 0D;

            // GetTotalExpenseInfo(ref totalSalary, ref totalFee, ref contractsIssued, ref salaryPerIssue, ref totalContracts, ref trustFeePerContract);

            getRecentDetails(ref salaryPerIssue, ref trustFeePerContract);

            string contractNumber = "";
            double commission = 0D;
            double totalCommission = 0D;
            double contractValue = 0D;
            string status = "";

            string reportDate = this.dateTimePicker1.Value.ToString("MM/dd/yyyy");

            DateTime lapseDate = DateTime.Now;
            DateTime reinstateDate = DateTime.Now;
            DateTime deceasedDate = DateTime.Now;

            DataTable paymentDt = null;
            double totalPaid = 0D;
            double percentage = 0D;
            double payment = 0D;
            double interest = 0D;
            double debit = 0D;
            double credit = 0D;
            double principal = 0D;
            double downPayment = 0D;
            string lname = "";

            double fees = 0D;

            int lastrow = dt.Rows.Count;

            lblTotal.Show();

            lblTotal.Text = "of " + lastrow.ToString();
            lblTotal.Refresh();

            barImport.Show();
            barImport.Minimum = 0;
            barImport.Maximum = lastrow;
            labelMaximum.Show();

            for (int i = 0; i < lastrow; i++)
            {
                try
                {
                    barImport.Value = i;
                    barImport.Refresh();
                    labelMaximum.Text = i.ToString();
                    labelMaximum.Refresh();

                    lname = dt.Rows[i]["lastName"].ObjToString();
                    if (String.IsNullOrWhiteSpace(lname))
                        dt.Rows[i]["lastName"] = "NAME NOT FOUND";

                    if (dt.Rows[i]["NewContract"].ObjToString() == "Y")
                        dt.Rows[i]["salary"] = salaryPerIssue;

                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                    contractValue = DailyHistory.GetContractValue(dt.Rows[i]);
                    dt.Rows[i]["contractValue"] = contractValue;
                    if (contractValue > 0D)
                    {
                        fees = dt.Rows[i]["fees"].ObjToDouble();
                        fees += trustFeePerContract;
                        dt.Rows[i]["fees"] = fees;
                    }
                    dt.Rows[i]["date"] = reportDate;

                    status = "ACTIVE";
                    lapseDate = dt.Rows[i]["lapseDate8"].ObjToDateTime();
                    reinstateDate = dt.Rows[i]["reinstateDate8"].ObjToDateTime();
                    deceasedDate = dt.Rows[i]["deceasedDate"].ObjToDateTime();
                    if (deceasedDate.Year > 1900)
                    {
                        status = "DECEASED";
                        dt.Rows[i]["lapsedate"] = deceasedDate.ToString("MM/dd/yyyy");
                    }
                    else
                    {
                        if (lapseDate.Year > 1900)
                        {
                            if (reinstateDate.Year > 1900)
                            {
                                if (lapseDate > reinstateDate)
                                    status = "LAPSED";
                            }
                            else
                                status = "LAPSED";
                            if (status == "LAPSED")
                                dt.Rows[i]["lapsedate"] = lapseDate.ToString("MM/dd/yyyy");
                        }
                    }
                    dt.Rows[i]["status"] = status;

                    cmd = "Select * from `payments` where `contractNumber` = '" + contractNumber + "' order by `payDate8`;";
                    paymentDt = G1.get_db_data(cmd);

                    downPayment = dt.Rows[i]["downPayment"].ObjToDouble();
                    totalPaid = downPayment;

                    for (int j = 0; j < paymentDt.Rows.Count; j++)
                    {
                        status = paymentDt.Rows[j]["fill"].ObjToString();
                        if (status.ToUpper() == "D")
                            continue;
                        downPayment = paymentDt.Rows[j]["downPayment"].ObjToDouble();
                        if (downPayment > 0D)
                            continue;
                        payment = paymentDt.Rows[j]["paymentAmount"].ObjToDouble();
                        debit = paymentDt.Rows[j]["debitAdjustment"].ObjToDouble();
                        credit = paymentDt.Rows[j]["creditAdjustment"].ObjToDouble();
                        interest = paymentDt.Rows[j]["interestPaid"].ObjToDouble();
                        principal = payment - interest + credit - debit;
                        totalPaid += principal;
                    }

                    dt.Rows[i]["paidIn"] = totalPaid;
                    if (contractValue > 0D)
                    {
                        percentage = totalPaid / contractValue;
                        dt.Rows[i]["paidInPercent"] = percentage;

                        commission = totalPaid * 0.05D;
                        dt.Rows[i]["commission"] = commission;
                    }
                }
                catch (Exception ex)
                {

                }
            }
            barImport.Value = lastrow;
            barImport.Refresh();
            labelMaximum.Text = lastrow.ToString();
            labelMaximum.Refresh();

            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void AddNewContracts(DataTable dt)
        {
            string contractNumber = "";
            double contractValue = 0D;

            DateTime date = this.dateTimePicker1.Value;

            string date1 = date.Year.ToString("D4") + "-" + date.Month.ToString("D2") + "-01" + " 00:00:00 ";

            int days = DateTime.DaysInMonth(date.Year, date.Month);
            string date2 = date.Year.ToString("D4") + "-" + date.Month.ToString("D2") + "-" + days.ToString("D2") + " 00:00:00 ";


            string cmd = "Select * from `contracts` p ";
            cmd += " JOIN `customers` c ON p.`contractNumber` = c.`contractNumber` ";
            cmd += " where p.`issueDate8` >= '" + date1 + "' ";
            cmd += " and   p.`issueDate8` <= '" + date2 + "' ";
            cmd += " ORDER by p.`issueDate8` ";
            cmd += ";";
            DataTable dx = G1.get_db_data(cmd);

            DataRow[] dRows = null;
            DataRow dR = null;

            if (G1.get_column_number(dt, "NewContract") < 0)
                dt.Columns.Add("NewContract");
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                dt.Rows[i]["NewContract"] = "";
            }

            for (int i = 0; i < dx.Rows.Count; i++)
            {
                contractNumber = dx.Rows[i]["contractNumber"].ObjToString();
                dRows = dt.Select("contractNumber='" + contractNumber + "'");
                if (dRows.Length > 0)
                    continue;
                dR = dt.NewRow();
                dR["contractNumber"] = contractNumber;
                dR["firstName"] = dx.Rows[i]["firstName"].ObjToString();
                dR["lastName"] = dx.Rows[i]["lastName"].ObjToString();
                dR["NewContract"] = "Y";
                dt.Rows.Add(dR);
            }
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
            if ( dgv2.Visible )
                printableComponentLink1.Component = dgv2;
            else if (dgv3.Visible)
                printableComponentLink1.Component = dgv3;

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

            if ( dgv.Visible )
                G1.AdjustColumnWidths(gridMain, 0.65D, true);
            else if (dgv2.Visible)
                G1.AdjustColumnWidths(gridMain, 0.65D, true);
            else if (dgv3.Visible)
                G1.AdjustColumnWidths(gridMain, 0.65D, true);

            printableComponentLink1.CreateDocument();
            printableComponentLink1.ShowPreview();

            if (dgv.Visible)
                G1.AdjustColumnWidths(gridMain, 0.65D, false );
            else if (dgv2.Visible)
                G1.AdjustColumnWidths(gridMain, 0.65D, false );
            else if (dgv3.Visible)
                G1.AdjustColumnWidths(gridMain, 0.65D, false );

            isPrinting = false;
        }
        /***********************************************************************************************/
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


            font = new Font("Ariel", 10, FontStyle.Bold);
            if ( dgv.Visible )
                Printer.DrawQuad(5, 8, 4, 4, "Trust Fee Allocation Report", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            else if (dgv2.Visible)
                Printer.DrawQuad(5, 8, 4, 4, "Trust Salary/Fee Report", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            else if (dgv3.Visible)
                Printer.DrawQuad(5, 8, 4, 4, "Trust Allocation Mismatch", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            DateTime date = DateTime.Now;
            string workDate = date.Month.ToString("D2") + "/" + date.Day.ToString("D2") + "/" + (date.Year % 100).ToString("D2");

            string str = "Report : " + workDate;

            Printer.SetQuadSize(24, 12);
            font = new Font("Ariel", 9, FontStyle.Regular);
            Printer.DrawQuad(19, 8, 5, 4, str, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(16, 8, 3, 4, lblPayment.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(19, 8, 3, 4, lblTrust85.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(22, 8, 3, 4, lblTrust100.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /****************************************************************************************/
        private void importOldDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string file = ofd.FileName;
                    if (String.IsNullOrWhiteSpace(file))
                        return;
                    if (!File.Exists(file))
                        return;
                    DataTable dx = Import.ImportCSVfile(file);
                    CleanUpTable(dx);
                    G1.NumberDataTable(dx);
                    dgv.DataSource = dx;
                }
            }
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void CleanUpTable(DataTable dt)
        {
            for (int i = (dt.Rows.Count - 1); i >= 0; i--)
            {
                if (String.IsNullOrWhiteSpace(dt.Rows[i]["contractNumber"].ObjToString()))
                    dt.Rows.RemoveAt(i);
            }
        }
        /****************************************************************************************/
        private void btnSaveDetail_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Do you really want to SAVE this Trust Allocations?", "Save Trust Allocation Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;

            this.Cursor = Cursors.WaitCursor;

            DataTable dt = (DataTable)dgv.DataSource;
            DataTable orderDetail = dt.Copy();
            if (G1.get_column_number(orderDetail, "tmstamp") >= 0)
                orderDetail.Columns.Remove("tmstamp");
            if (G1.get_column_number(orderDetail, "record") >= 0)
                orderDetail.Columns.Remove("record");
            DataColumn Col = orderDetail.Columns.Add("record", System.Type.GetType("System.String"));
            Col.SetOrdinal(0);// to put the column in position 0;
            DataColumn Col1 = orderDetail.Columns.Add("tmstamp", System.Type.GetType("System.String"));
            Col1.SetOrdinal(0);// to put the column in position 0;

            DateTime date = DateTime.Now;
            DateTime badDate = new DateTime(1500, 1, 1);
            double dValue = 0D;

            try
            {
                for (int i = 0; i < orderDetail.Rows.Count; i++)
                {
                    orderDetail.Rows[i]["tmstamp"] = "0000-00-00";
                    date = orderDetail.Rows[i]["lapsedate"].ObjToDateTime();
                    if (date.Year < 100)
                        orderDetail.Rows[i]["lapsedate"] = G1.DTtoMySQLDT(badDate);
                    else
                        orderDetail.Rows[i]["lapsedate"] = G1.DTtoMySQLDT(date);

                    date = orderDetail.Rows[i]["date"].ObjToDateTime();
                    if (date.Year < 100)
                        orderDetail.Rows[i]["date"] = G1.DTtoMySQLDT(badDate);
                    else
                        orderDetail.Rows[i]["date"] = G1.DTtoMySQLDT(date);
                    if (String.IsNullOrWhiteSpace(orderDetail.Rows[i]["comm2"].ObjToString()))
                        orderDetail.Rows[i]["comm2"] = 0D;
                    if (String.IsNullOrWhiteSpace(orderDetail.Rows[i]["fee2"].ObjToString()))
                        orderDetail.Rows[i]["fee2"] = 0D;
                    if (String.IsNullOrWhiteSpace(orderDetail.Rows[i]["contractValue2"].ObjToString()))
                        orderDetail.Rows[i]["contractValue2"] = 0D;
                    if (String.IsNullOrWhiteSpace(orderDetail.Rows[i]["paidIn2"].ObjToString()))
                        orderDetail.Rows[i]["paidIn2"] = 0D;
                    if (String.IsNullOrWhiteSpace(orderDetail.Rows[i]["commission"].ObjToString()))
                        orderDetail.Rows[i]["commission"] = 0D;
                    if (String.IsNullOrWhiteSpace(orderDetail.Rows[i]["fees"].ObjToString()))
                        orderDetail.Rows[i]["fees"] = 0D;
                    if (String.IsNullOrWhiteSpace(orderDetail.Rows[i]["paidInPercent"].ObjToString()))
                        orderDetail.Rows[i]["paidInPercent"] = 0D;

                    orderDetail.Rows[i]["record"] = "0";
                    orderDetail.Rows[i]["firstName"] = G1.try_protect_data(orderDetail.Rows[i]["firstName"].ObjToString());
                    orderDetail.Rows[i]["lastName"] = G1.try_protect_data(orderDetail.Rows[i]["lastName"].ObjToString());
                    orderDetail.Rows[i]["firstName"] = G1.Truncate(orderDetail.Rows[i]["firstName"].ObjToString(), 80);
                    orderDetail.Rows[i]["lastName"] = G1.Truncate(orderDetail.Rows[i]["lastName"].ObjToString(), 80);
                }
            }
            catch ( Exception ex)
            {
            }


            string tableName = "fee_contracts";

            DeletePreviousData();

            Structures.TieDbTable(tableName, orderDetail);

            //string connectMySQL = "Server=localhost;Database=test;Uid=username;Pwd=password;";
            string strFile = "/TempFolder/MySQL" + DateTime.Now.Ticks.ToString() + ".csv";
            string Server = "C:/rag";
            //Create directory if not exist... Make sure directory has required rights..
            if (!Directory.Exists(Server + "/TempFolder/"))
                Directory.CreateDirectory(Server + "/TempFolder/");

            //If file does not exist then create it and right data into it..
            if (!File.Exists(Server + strFile))
            {
                FileStream fs = new FileStream(Server + strFile, FileMode.Create, FileAccess.Write);
                fs.Close();
                fs.Dispose();
            }

            //Generate csv file from where data read
            MySQL.CreateCSVfile(orderDetail, Server + strFile);
            //using (MySqlConnection cn1 = new MySqlConnection(connectMySQL))
            //{
            try
            {
                G1.conn1.Open();
                MySqlBulkLoader bcp1 = new MySqlBulkLoader(G1.conn1);
                bcp1.TableName = tableName; //Create ProductOrder table into MYSQL database...
                bcp1.FieldTerminator = ",";

                bcp1.LineTerminator = "\r\n";
                bcp1.FileName = Server + strFile;
                bcp1.NumberOfLinesToSkip = 0;
                bcp1.Load();
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** " + ex.Message.ToString(), "Save Data Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                this.Cursor = Cursors.Default;
                return;
            }

            //Once data write into db then delete file..
            try
            {
                File.Delete(Server + strFile);
            }
            catch (Exception ex)
            {
                string str = ex.Message;
            }

            this.Cursor = Cursors.Default;
            //}
        }
        /***********************************************************************************************/
        private void DeletePreviousData()
        {
            DateTime date = this.dateTimePicker1.Value;
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            string date1 = date.Year.ToString("D4") + "-" + date.Month.ToString("D2") + "-" + days.ToString("D2") + " 00:00:00 ";
            string cmd = "DELETE from `fee_contracts` where `date` = '" + date1 + "' ";
            cmd += ";";
            try
            {
                G1.get_db_data(cmd);
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Delete Previous Data " + ex.Message.ToString());
            }
        }
        /****************************************************************************************/
        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            DateTime date = dateTimePicker1.Value;

            DialogResult result = MessageBox.Show("Are you sure you want to READ OLD Trust Data for " + date.ToString("MM/dd/yyyy") + "?", "Pull(READ) Trust Report Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;

            int days = DateTime.DaysInMonth(date.Year, date.Month);
            string date1 = date.Year.ToString("D4") + "-" + date.Month.ToString("D2") + "-" + days.ToString("D2") + " 00:00:00 ";
            string cmd = "Select * from `fee_contracts` where `date` = '" + date1 + "' ";
            cmd += ";";

            try
            {
                DataTable dt = G1.get_db_data(cmd);
                G1.NumberDataTable(dt);
                dgv.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Delete Previous Data " + ex.Message.ToString());
            }
            btnSaveDetail.Hide();
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
                    if (date.Year > 1600)
                        e.DisplayText = date.ToString("MM/dd/yyyy");
                    else
                        e.DisplayText = "";
                }
            }
        }
        /****************************************************************************************/
        private void btnMatch_Click(object sender, EventArgs e)
        {
            string actualFile = "";
            string file = "";
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    file = ofd.FileName;
                    int idx = file.LastIndexOf("\\");
                    if (idx > 0)
                    {
                        actualFile = file.Substring(idx);
                        actualFile = actualFile.Replace("\\", "");
                    }
                }
            }
            if (String.IsNullOrWhiteSpace(file))
                return;
            DataTable mDt = Import.ImportCSVfile(file);
            if (mDt.Rows.Count <= 0)
                return;
            DataTable dt = (DataTable)dgv.DataSource;
            if (G1.get_column_number(dt, "mismatch") < 0)
                dt.Columns.Add("mismatch");
            if (G1.get_column_number(dt, "status2") < 0)
                dt.Columns.Add("status2");
            if (G1.get_column_number(dt, "comm2") < 0)
                dt.Columns.Add("comm2", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "fee2") < 0)
                dt.Columns.Add("fee2", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "contractValue2") < 0)
                dt.Columns.Add("contractValue2", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "paidIn2") < 0)
                dt.Columns.Add("paidIn2", Type.GetType("System.Double"));

            string status1 = "";
            string status2 = "";
            string contractNumber = "";
            double value1 = 0D;
            double value2 = 0D;
            DataRow[] dR = null;
            this.Cursor = Cursors.WaitCursor;

            int lastrow = dt.Rows.Count;

            lblTotal.Show();

            lblTotal.Text = "of " + lastrow.ToString();
            lblTotal.Refresh();

            barImport.Show();
            barImport.Minimum = 0;
            barImport.Maximum = lastrow;
            labelMaximum.Show();

            for ( int i=0; i<dt.Rows.Count; i++)
            {
                barImport.Value = i;
                barImport.Refresh();
                labelMaximum.Text = i.ToString();
                labelMaximum.Refresh();

                try
                {

                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                    dR = mDt.Select("cnum='" + contractNumber + "'");
                    if (dR.Length <= 0)
                    {
                        dt.Rows[i]["status2"] = "MISSING";
                        dt.Rows[i]["mismatch"] = "MISMATCH";
                        continue;
                    }
                    status1 = dt.Rows[i]["status"].ObjToString();
                    status2 = dR[0]["status"].ObjToString();
                    if (status1.Trim().ToUpper() != status2.Trim().ToUpper())
                    {
                        dt.Rows[i]["status2"] = status2;
                        dt.Rows[i]["mismatch"] = "MISMATCH";
                    }

                    value1 = dt.Rows[i]["commission"].ObjToDouble();
                    value2 = dR[0]["com"].ObjToDouble();
                    value1 = G1.RoundDown(value1);
                    value2 = G1.RoundDown(value2);
                    if (value1 != value2)
                    {
                        value2 = G1.RoundDown(value2);
                        dt.Rows[i]["comm2"] = value2;
                        dt.Rows[i]["mismatch"] = "MISMATCH";
                    }
                    else
                        dt.Rows[i]["comm2"] = -999.99D;

                    value1 = dt.Rows[i]["fees"].ObjToDouble();
                    value2 = dR[0]["fee"].ObjToDouble();
                    value1 = G1.RoundDown(value1);
                    value2 = G1.RoundDown(value2);
                    if (value1 != value2)
                    {
                        dt.Rows[i]["fee2"] = value2;
                        dt.Rows[i]["mismatch"] = "MISMATCH";
                    }
                    else
                        dt.Rows[i]["fee2"] = -999.99D;

                    value1 = dt.Rows[i]["contractValue"].ObjToDouble();
                    value2 = dR[0]["NETTRST"].ObjToDouble();
                    value1 = G1.RoundDown(value1);
                    value2 = G1.RoundDown(value2);
                    if (value1 != value2)
                    {
                        dt.Rows[i]["contractValue2"] = value2;
                        dt.Rows[i]["mismatch"] = "MISMATCH";
                    }
                    else
                        dt.Rows[i]["contractValue2"] = -999.99D;

                    value1 = dt.Rows[i]["paidIn"].ObjToDouble();
                    value2 = dR[0]["TOTPAIDIN"].ObjToDouble();
                    value1 = G1.RoundDown(value1);
                    value2 = G1.RoundDown(value2);
                    if (value1 != value2)
                    {
                        dt.Rows[i]["paidIn2"] = value2;
                        dt.Rows[i]["mismatch"] = "MISMATCH";
                    }
                    else
                        dt.Rows[i]["paidIn2"] = -999.99D;
                }
                catch ( Exception ex)
                {
                }
            }
            G1.NumberDataTable(dt);
            dgv3.DataSource = dt;
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void gridMain3_CustomRowFilter(object sender, RowFilterEventArgs e)
        {
            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv3.DataSource;
            ColumnView view = sender as ColumnView;
            //string mismatch = dt.Rows[row]["mismatch"].ObjToString();
            //if ( mismatch.Trim().ToUpper() != "MISMATCH")
            //{
            //    e.Visible = false;
            //    e.Handled = true;
            //}
        }
        /****************************************************************************************/
        private void gridMain3_CustomDrawCell(object sender, RowCellCustomDrawEventArgs e)
        {
            if (e.Column.FieldName.ToUpper() == "NUM")
            {
                if (e.RowHandle >= 0)
                {
                    string num = (e.RowHandle + 1).ToString();
                    e.DisplayText = num;
                }
            }
            else if (e.DisplayText.ObjToString() == "-999.99")
                e.DisplayText = "";
        }
        /****************************************************************************************/
        private void gridMain3_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain3.GetFocusedDataRow();
            string contract = dr["contractNumber"].ObjToString();
            this.Cursor = Cursors.WaitCursor;
            CustomerDetails clientForm = new CustomerDetails(contract);
            clientForm.Show();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void gridMain3_CustomColumnDisplayText(object sender, CustomColumnDisplayTextEventArgs e)
        {
            ColumnView view = sender as ColumnView;
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
    }
}