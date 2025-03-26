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
using DevExpress.XtraGrid.Views.Grid;
using MySql.Data.Types;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class DailyReport : DevExpress.XtraEditors.XtraForm
    {
        private bool loading = true;
        private DataTable originalDt = null;
        /****************************************************************************************/
        public DailyReport()
        {
            InitializeComponent();
            SetupTotalsSummary();
        }
        /****************************************************************************************/
        private void DailyReport_Load(object sender, EventArgs e)
        {
            LoadComboWhat();
            LoadComboWho();
            LoadLockBoxes();
            loading = false;
            GetMostRecentImport();
            if (!LoginForm.administrator)
                miscToolStripMenuItem.Enabled = false;
        }
        /****************************************************************************************/
        private void SetupTotalsSummary()
        {
            AddSummaryColumn("expected", null);
            AddSummaryColumn("payment", null);
            AddSummaryColumn("principal", null);
            AddSummaryColumn("interest", null);
        }
        /****************************************************************************************/
        private void AddSummaryColumn(string columnName, DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gMain = null, string format = "")
        {
            if (gMain == null)
                gMain = gridMain;
            if (String.IsNullOrWhiteSpace(format))
                format = "${0:0,0.00}";
            gMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            gMain.Columns[columnName].SummaryItem.DisplayFormat = format;
        }
        /****************************************************************************************/
        private void AddSummaryColumn(string columnName, string format = "")
        {
            if (String.IsNullOrWhiteSpace(format))
                format = "${0:0,0.00}";
            gridMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            gridMain.Columns[columnName].SummaryItem.DisplayFormat = format;
        }
        /***********************************************************************************************/
        private void GetMostRecentImport()
        {
            DateTime date = DateTime.Now;
            string cmd = "Select * from `bank_file` ORDER BY `tmstamp` DESC LIMIT 1";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                string tempfile = dt.Rows[0]["filename"].ObjToString();
                date = dt.Rows[0]["tmstamp"].ObjToDateTime();
                cmd = "Select * from `payments` where `edited` = 'manual' ORDER BY `tmstamp` DESC LIMIT 1;";
                dt = G1.get_db_data(cmd);
                if ( dt.Rows.Count > 0 )
                {
                    DateTime date1 = dt.Rows[0]["tmstamp"].ObjToDateTime();
                    if (date1 > date)
                        date = date1;
                }
                cmd = "Select * from `ipayments` where `edited` = 'manual' ORDER BY `tmstamp` DESC LIMIT 1;";
                dt = G1.get_db_data(cmd);
                if (dt.Rows.Count > 0)
                {
                    DateTime date1 = dt.Rows[0]["tmstamp"].ObjToDateTime();
                    if (date1 > date)
                        date = date1;
                }
                this.dateTimePicker1.Value = date;
            }
        }
        /***********************************************************************************************/
        private void LoadComboWhat(DataTable dx = null)
        {
            loading = true;
            DataTable dt = new DataTable();
            dt.Columns.Add("keycode");

            DataRow dRow = dt.NewRow();
            dRow["keycode"] = "ALL";
            dt.Rows.Add(dRow);

            if (dx != null)
            {
                string oldWhat = "";
                string what = "";
                for (int i = 0; i < dx.Rows.Count; i++)
                {
                    what = dx.Rows[i]["location"].ObjToString();
                    if (!oldWhat.Contains(what))
                        oldWhat += what + ",";
                }
                oldWhat = oldWhat.TrimEnd(',');
                string[] Lines = oldWhat.Split(',');
                for (int i = 0; i < Lines.Length; i++)
                {
                    what = Lines[i].Trim();
                    if (!String.IsNullOrWhiteSpace(what))
                    {
                        DataRow dR = dt.NewRow();
                        dR["keycode"] = what;
                        dt.Rows.Add(dR);
                    }
                }
            }
            cmbWhat.Properties.DataSource = dt;
            cmbWhat.Properties.DropDownRows = dt.Rows.Count + 1;
            cmbWhat.EditValue = "ALL";
            cmbWhat.Text = "ALL";
            loading = false;
            cmbWhat.Refresh();
        }
        /***********************************************************************************************/
        private void LoadComboWho( DataTable dx = null )
        {
            loading = true;
            DataTable dt = new DataTable();
            dt.Columns.Add("keycode");

            DataRow dRow = dt.NewRow();
            dRow["keycode"] = "ALL";
            dt.Rows.Add(dRow);

            if (dx != null)
            {
                string oldWho = "";
                string who = "";
                for ( int i=0; i<dx.Rows.Count; i++)
                {
                    who = dx.Rows[i]["code"].ObjToString();
                    if (!oldWho.Contains(who))
                        oldWho += who + ",";
                }
                oldWho = oldWho.TrimEnd(',');
                string[] Lines = oldWho.Split(',');
                for ( int i=0; i<Lines.Length; i++)
                {
                    who = Lines[i].Trim();
                    if ( !String.IsNullOrWhiteSpace ( who))
                    {
                        DataRow dR = dt.NewRow();
                        if (who == "01")
                            dR["keycode"] = "TRUSTS";
                        else if (who == "02")
                            dR["keycode"] = "INSURANCE";
                        else
                            dR["keycode"] = who;
                        dt.Rows.Add(dR);
                    }
                }
            }

            cmbWho.Properties.DataSource = dt;
            cmbWho.Properties.DropDownRows = dt.Rows.Count + 1;
            cmbWho.EditValue = "ALL";
            cmbWho.Text = "ALL";
            loading = false;
            cmbWho.Refresh();
        }
        /***********************************************************************************************/
        private void LoadComboBox(DataTable dx = null)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("keycode");

            DataRow dRow = dt.NewRow();
            dRow["keycode"] = "ALL";
            dt.Rows.Add(dRow);

            if (dx != null)
            {

                DataView tempview = dx.DefaultView;
                tempview.Sort = "box asc";
                dx = tempview.ToTable();

                string box = "";
                string oldBox = "";

                for (int i = 0; i < dx.Rows.Count; i++)
                {
                    box = dx.Rows[i]["box"].ObjToString();
                    if (oldBox != box)
                    {
                        dRow = dt.NewRow();
                        dRow["keycode"] = box;
                        dt.Rows.Add(dRow);
                        oldBox = box;
                    }
                }
            }
            cmbBox.Properties.DataSource = dt;
            cmbBox.Properties.DropDownRows = dt.Rows.Count + 1;
            cmbBox.EditValue = "ALL";
            cmbBox.Text = "ALL";
            cmbBox.Refresh();
        }
        /***********************************************************************************************/
        private void LoadComboDeposits( DataTable dx = null)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("keycode");

            DataRow dRow = dt.NewRow();
            dRow["keycode"] = "ALL";
            dt.Rows.Add(dRow);

            if (dx != null)
            {

                DataTable cmbDt = dx.Copy();
                DataView tempview = cmbDt.DefaultView;
                tempview.Sort = "depositNumber asc";
                cmbDt = tempview.ToTable();

                string depositNumber = "";
                string oldDeposit = "";

                for (int i = 0; i < cmbDt.Rows.Count; i++)
                {
                    depositNumber = cmbDt.Rows[i]["depositNumber"].ObjToString();
                    if (oldDeposit != depositNumber)
                    {
                        dRow = dt.NewRow();
                        dRow["keycode"] = depositNumber;
                        dt.Rows.Add(dRow);
                        oldDeposit = depositNumber;
                    }
                }
            }
            cmbDeposits.Properties.DataSource = dt;
            cmbDeposits.Properties.DropDownRows = dt.Rows.Count + 1;
            cmbDeposits.EditValue = "ALL";
            cmbDeposits.Text = "ALL";
            cmbDeposits.Refresh();
        }
        /****************************************************************************************/
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
        /****************************************************************************************/
        private void btnLeft_Click(object sender, EventArgs e)
        {
            DateTime now = this.dateTimePicker1.Value;
            now = now.AddDays(-1);
            this.dateTimePicker1.Value = now;
        }
        /****************************************************************************************/
        private void btnRight_Click(object sender, EventArgs e)
        {
            DateTime now = this.dateTimePicker1.Value;
            now = now.AddDays(1);
            this.dateTimePicker1.Value = now;
        }
        /****************************************************************************************/
        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            LoadLockBoxes();
        }
        /****************************************************************************************/
        private bool CheckForManualPayments ( string date1, string date2 )
        {
            string cmd = "Select * from `payments` where `tmstamp` >= '" + date1 + "' AND `tmstamp` <= '" + date2 + "' AND `edited` = 'MANUAL' LIMIT 1;";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
                return true;
            cmd = "Select * from `ipayments` where `tmstamp` >= '" + date1 + "' AND `tmstamp` <= '" + date2 + "' AND `edited` = 'MANUAL' LIMIT 1;";
            dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
                return true;
            return false;
        }
        /****************************************************************************************/
        private void LoadLockBoxes ()
        {
            if (loading)
                return;
            DateTime date = dateTimePicker1.Value;
            int year = (date.Year % 100);
            string date1 = date.ToString("yyyy-MM-dd") + " 00:00:00";
            string date2 = date.ToString("yyyy-MM-dd") + " 23:59:59";
            string cmd = "Select * from `bank_file` where `tmstamp` >= '" + date1 + "' AND `tmstamp` <= '" + date2 + "' ORDER BY `tmstamp` DESC;";
            DataTable dt = G1.get_db_data(cmd);
            cmbLockBox.Items.Clear();
            cmbLockBox.Text = "";
            string oldFile = "";
            string firstFile = "";
            string filename = "";
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                filename = dt.Rows[i]["filename"].ObjToString();
                if (filename != oldFile)
                    cmbLockBox.Items.Add(filename);
                if (String.IsNullOrWhiteSpace(firstFile))
                    firstFile = filename;
                oldFile = filename;
            }

            if (!String.IsNullOrWhiteSpace(firstFile))
            {
                cmbLockBox.Text = firstFile;
                cmd = "Select * from `bank_file` where `filename` = '" + firstFile + "' ORDER BY `tmstamp` LIMIT 1;";
                dt = G1.get_db_data(cmd);
            }
            else
            {
                dt.Clear();
            }

            dt.Columns.Add("num");
            dt.Columns.Add("name");
            dt.Columns.Add("edited");
            dt.Columns.Add("box");
            dt.Rows.Clear();

            dgv.DataSource = dt;

            if (CheckForManualPayments(date1, date2))
            {
                cmbLockBox.Items.Add("Manual Available");
                if ( String.IsNullOrWhiteSpace ( cmbLockBox.Text ))
                    cmbLockBox.Text = "Manual Available";
            }
        }
        /***********************************************************************************************/
        private void CalculateGoodBadUgly(DataTable dt)
        {
            double total = 0D;
            double value = 0D;
            double badValue = 0D;
            double trustTotal = 0D;
            double insuranceTotal = 0D;
            double trustBad = 0D;
            double insuranceBad = 0D;
            double miscBad = 0D;
            string str = "";
            string cnum = "";
            string code = "";
            string location = "";
            if (dt != null && dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    code = dt.Rows[i]["code"].ObjToString();
                    location = dt.Rows[i]["location"].ObjToString();
                    value = dt.Rows[i]["payment"].ObjToDouble();
                    total += value;
                    if (code != "01")
                    {
                        if (code == "02")
                        {
                            insuranceTotal += value;
                        }
                        else
                        {
                            trustTotal += value;
                        }
                    }
                    else
                    {
                        trustTotal += value;
                    }

                }
            }
            double totalTrust = trustTotal + trustBad;
            double totalInsurance = insuranceTotal + insuranceBad;
            double goodValue = total - badValue;
            gridBand1.Caption = "Total Trust : $" + G1.ReformatMoney(trustTotal + trustBad);
            gridBand4.Caption = "Total Ins   : $" + G1.ReformatMoney(insuranceTotal + insuranceBad);
            gridBand5.Caption = " Total Deposit : $" + G1.ReformatMoney(total);
        }
        /****************************************************************************************/
        private void btnRun_Click(object sender, EventArgs e)
        {
            LoadData();
        }
        /****************************************************************************************/
        private void LoadData()
        {
            this.Cursor = Cursors.WaitCursor;

            LoadComboDeposits();
            LoadComboBox();
            LoadComboWho();
            LoadComboWhat();


            DateTime date = dateTimePicker1.Value;
            string filename = cmbLockBox.Text.Trim();
            string cmd = "Select * from `bank_file` where `filename` = '" + filename + "' ORDER BY `tmstamp`;";
            DataTable dt = G1.get_db_data(cmd);

            dt.Columns.Add("num");
            dt.Columns.Add("name");
            dt.Columns.Add("edited");
            dt.Columns.Add("box");
            dt.Columns.Add("depositNumber");
            dt.Columns.Add("printDate");
            dt.Columns.Add("printDueDate");
            dt.Columns.Add("payer");

            string code = "";
            string contractFile = "";
            string customerFile = "";
            string paymentFile = "";
            string contractNumber = "";
            string name = "";
            double apr = 0D;
            double expected = 0D;
            string paymentRecord = "";
            string location = "";
            string depositNumber = "";
            string payer = "";
            string str = "";

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                code = dt.Rows[0]["code"].ObjToString();
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                contractFile = "contracts";
                customerFile = "customers";
                paymentFile = "payments";
                if (DailyHistory.isInsurance(contractNumber))
                {
                    contractFile = "icontracts";
                    customerFile = "icustomers";
                    paymentFile = "ipayments";
                }
                name = dt.Rows[i]["firstName"] + " " + dt.Rows[i]["lastName"].ObjToString();
                dt.Rows[i]["name"] = name;
                apr = dt.Rows[i]["apr"].ObjToDouble() / 100D;
                dt.Rows[i]["apr"] = apr;
                if (contractNumber == "ZZ0000229")
                {

                }

                location = dt.Rows[i]["location"].ObjToString();
                if (String.IsNullOrWhiteSpace(location))
                    dt.Rows[i]["box"] = "LKBX";
                else if (location == "ACH")
                    dt.Rows[i]["box"] = "ACH";
                else if (location == "CC")
                    dt.Rows[i]["box"] = "CC";
                else if (location == "TFBX")
                    dt.Rows[i]["box"] = "TFBX";
                else
                    dt.Rows[i]["box"] = "LKBX";

                paymentRecord = dt.Rows[i]["!paymentRecord"].ObjToString();
                cmd = "Select * from `" + paymentFile + "` where `record` = '" + paymentRecord + "';";
                DataTable dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    depositNumber = dx.Rows[0]["depositNumber"].ObjToString();
                    if (String.IsNullOrWhiteSpace(depositNumber))
                    {

                    }
                    dt.Rows[i]["edited"] = dx.Rows[0]["edited"].ObjToString();
                    dt.Rows[i]["depositNumber"] = depositNumber;
                    if ( !String.IsNullOrWhiteSpace ( depositNumber))
                    {
                        str = depositNumber.Substring(0, 1).ToUpper();
                        if ( str == "A")
                            dt.Rows[i]["box"] = "ACH" +
                                "";
                    }
                }
                expected = dt.Rows[i]["expected"].ObjToDouble();
                cmd = "Select * from `" + contractFile + "`c JOIN `" + customerFile + "` p ON c.`contractNumber` = p.`contractNumber` where c.`contractNumber` = '" + contractNumber + "';";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    if (expected <= 0D)
                    {
                        expected = dx.Rows[0]["amtOfMonthlyPayt"].ObjToDouble();
                        if ( DailyHistory.isInsurance ( contractNumber ) && expected > 500D)
                        {
                            payer = dx.Rows[0]["payer"].ObjToString();
                            expected = Policies.CalcMonthlyPremium(payer, date );
                        }
                    }
                    dt.Rows[i]["payer"] = dx.Rows[0]["payer"].ObjToString();
                }
                dt.Rows[i]["expected"] = expected;
            }

            if (cmbLockBox.Text.ToUpper() == "MANUAL AVAILABLE")
            {
                LoadManualPayments(dt, false);
                LoadManualPayments(dt, true);
            }

            CalcMonthsPaid(dt);
            FixTheDates(dt);

            originalDt = dt;

            LoadComboDeposits(originalDt);
            LoadComboBox(originalDt);
            LoadComboWho(originalDt);
            LoadComboWhat(originalDt);

            DetermineView();
            ScaleCells();


            //            dgv.DataSource = dt;
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void FixTheDates ( DataTable dt )
        {
            DateTime date = DateTime.Now;
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                date = dt.Rows[i]["contractDate"].ObjToDateTime();
                dt.Rows[i]["printDate"] = date.ToString("MM/dd/yyyy");
                date = dt.Rows[i]["dueDate8"].ObjToDateTime();
                dt.Rows[i]["printDueDate"] = date.ToString("MM/dd/yyyy");
            }
        }
        /****************************************************************************************/
        private void CalcMonthsPaid(DataTable dt)
        {
            if (G1.get_column_number(dt, "monthsPaid") < 0)
                dt.Columns.Add("monthsPaid", Type.GetType("System.Double"));
            double expected = 0D;
            double paid = 0D;
            double months = 0D;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                expected = dt.Rows[i]["expected"].ObjToDouble();
                paid = dt.Rows[i]["payment"].ObjToDouble();
                months = 0D;
                if (expected > 0D)
                    months = paid / expected;
                dt.Rows[i]["monthsPaid"] = months;
            }
        }
        /****************************************************************************************/
        private void LoadManualPayments ( DataTable dt, bool insurance )
        {
            DateTime date = dateTimePicker1.Value;
            string date1 = date.ToString("yyyy-MM-dd") + " 00:00:00";
            string date2 = date.ToString("yyyy-MM-dd") + " 23:59:59";
            string cmd = "Select * from `payments` p JOIN `contracts` c ON p.`contractNumber` = c.`contractNumber` JOIN `customers` m ON p.`contractNumber` = m.`contractNumber` where p.`tmstamp` >= '" + date1 + "' AND p.`tmstamp` <= '" + date2 + "' AND `edited` = 'MANUAL' ORDER BY p.`tmstamp` DESC;";
            if ( insurance )
                cmd = "Select * from `ipayments` p JOIN `icontracts` c ON p.`contractNumber` = c.`contractNumber` JOIN `icustomers` m ON p.`contractNumber` = m.`contractNumber` where p.`tmstamp` >= '" + date1 + "' AND p.`tmstamp` <= '" + date2 + "' AND `edited` = 'MANUAL' ORDER BY p.`tmstamp` DESC;";
            DataTable dx = G1.get_db_data(cmd);
            string name = "";
            double apr = 0D;
            double expected = 0D;
            double payment = 0D;
            double interest = 0D;
            double principal = 0D;
            string contractNumber = "";
            string payer = "";
            int row = 0;
            for ( int i=0; i<dx.Rows.Count; i++)
            {
                contractNumber = dx.Rows[i]["contractNumber"].ObjToString();
                if (contractNumber == "ZZ0022817")
                {

                }
                dt.ImportRow(dx.Rows[i]);

                payer = "";
                if (insurance)
                {
                    payer = dx.Rows[i]["payer"].ObjToString();
                    if ( String.IsNullOrWhiteSpace ( payer))
                        payer = dx.Rows[i]["payer1"].ObjToString();
                }
                DateTime iDate = DailyHistory.GetIssueDate(dx.Rows[i]["issueDate8"].ObjToDateTime(), contractNumber, null);

                row = dt.Rows.Count - 1;
                name = dx.Rows[i]["firstName"] + " " + dx.Rows[i]["lastName"].ObjToString();
                dt.Rows[row]["name"] = name;
                apr = dx.Rows[i]["apr"].ObjToDouble() / 100D;
                dt.Rows[row]["apr"] = apr;
                expected = dx.Rows[i]["amtOfMonthlyPayt"].ObjToDouble();
                if (insurance && expected > 500D)
                    expected = Policies.CalcMonthlyPremium(contractNumber, "", expected);
                dt.Rows[row]["expected"] = expected;
                payment = dx.Rows[i]["paymentAmount"].ObjToDouble();
                interest = dx.Rows[i]["interestPaid"].ObjToDouble();
                principal = payment - interest;
                dt.Rows[row]["payment"] = payment;
                dt.Rows[row]["interest"] = interest;
                dt.Rows[row]["principal"] = principal;
                MySqlDateTime myDate = (MySqlDateTime)G1.DTtoMySQLDT(iDate.ToString("MM/dd/yyyy"));
                dt.Rows[row]["contractDate"] = myDate;
                iDate = dx.Rows[i]["dueDate8"].ObjToDateTime();
                if (iDate.Year < 1850)
                    iDate = dx.Rows[i]["payDate8"].ObjToDateTime();
                myDate = (MySqlDateTime)G1.DTtoMySQLDT(iDate.ToString("MM/dd/yyyy"));
                dt.Rows[row]["dueDate8"] = myDate;
                dt.Rows[row]["box"] = "MANUAL";
//                dt.Rows[row]["location"] = "MANUAL";
                if (insurance)
                    dt.Rows[row]["code"] = "02";
                else
                    dt.Rows[row]["code"] = "01";
                if (insurance)
                    dt.Rows[row]["payer"] = payer;
            }
        }
        /****************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string cnum = dr["contractNumber"].ObjToString();
            this.Cursor = Cursors.WaitCursor;
            CustomerDetails clientForm = new CustomerDetails(cnum);
            clientForm.Show();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void removePaymentsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!LoginForm.administrator)
            {
                MessageBox.Show("***ERROR*** You do not have permission to remove this payment!");
                return;
            }

            DialogResult result = MessageBox.Show("Are you sure you want to Eliminate All THESE Payments?", "Eliminate Payment Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
            if (result == DialogResult.No)
                return;

            result = MessageBox.Show("Are you REALLY SURE you want to Eliminate All THESE Payments?", "Eliminate Payment Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
            if (result == DialogResult.No)
                return;

            this.Cursor = Cursors.WaitCursor;

            DataTable dt = (DataTable)dgv.DataSource;

            string record = "";
            string paymentRecord = "";
            string contractRecord = "";
            string code = "";
            string contractFile = "";
            string customerFile = "";
            string paymentFile = "";
            string contractNumber = "";
            string oldBalance = "";
            string oldDueDate8 = "";
            string oldDOLP = "";
            string cmd = "";

            for ( int i=0; i<dt.Rows.Count; i++)
            {
                code = dt.Rows[0]["code"].ObjToString();
                contractFile = "contracts";
                customerFile = "customers";
                paymentFile = "payments";
                if (code == "02")
                {
                    contractFile = "icontracts";
                    customerFile = "icustomers";
                    paymentFile = "ipayments";
                }
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                record = dt.Rows[i]["record"].ObjToString();
                paymentRecord = dt.Rows[i]["!paymentRecord"].ObjToString();

                cmd = "Select * from `" + paymentFile + "` where `record` = '" + paymentRecord + "';";
                DataTable dx = G1.get_db_data(cmd);
                if (dx.Rows.Count <= 0)
                    continue;

                oldBalance = dx.Rows[0]["oldBalance"].ObjToString();
                oldDueDate8 = dx.Rows[0]["oldDueDate8"].ObjToString();
                oldDOLP = dx.Rows[0]["oldDOLP"].ObjToString();

                G1.delete_db_table(paymentFile, "record", paymentRecord);

                cmd = "Select * from `" + contractFile + "` where `contractNumber` = '" + contractNumber + "';";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    contractRecord = dx.Rows[0]["record"].ObjToString();
                    G1.update_db_table(contractFile, "record", contractRecord, new string[] { "balanceDue", oldBalance, "dueDate8", oldDueDate8, "lastDatePaid8", oldDOLP });
                }

                G1.delete_db_table("bank_file", "record", record);
            }

            GetMostRecentImport();

            dt.Rows.Clear();
            dgv.DataSource = dt;
            dgv.Refresh();
            this.Cursor = Cursors.Default;
        }
        /*******************************************************************************************/
        private string getWhatQuery()
        {
            string procLoc = "";
            string[] locIDs = this.cmbWhat.EditValue.ToString().Split('|');
            string what = "";
            for (int i = 0; i < locIDs.Length; i++)
            {
                if (!String.IsNullOrWhiteSpace(locIDs[i]))
                {
                    if (procLoc.Trim().Length > 0)
                        procLoc += ",";
                    what = locIDs[i].Trim();
                    if ( what == "ALL")
                    {
                        procLoc = "";
                        break;
                    }
                    procLoc += "'" + what + "'";
                }
            }
            return procLoc.Length > 0 ? " `location` IN (" + procLoc + ") " : "";
        }
        /*******************************************************************************************/
        private string getBoxQuery()
        {
            string procLoc = "";
            string[] locIDs = this.cmbBox.EditValue.ToString().Split('|');
            string what = "";
            for (int i = 0; i < locIDs.Length; i++)
            {
                if (!String.IsNullOrWhiteSpace(locIDs[i]))
                {
                    if (procLoc.Trim().Length > 0)
                        procLoc += ",";
                    what = locIDs[i].Trim();
                    if (what == "ALL")
                    {
                        procLoc = "";
                        break;
                    }
                    procLoc += "'" + what + "'";
                }
            }
            return procLoc.Length > 0 ? " `box` IN (" + procLoc + ") " : "";
        }
        /****************************************************************************************/
        private void cmbWhat_EditValueChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            if (originalDt == null)
                return;
            DetermineView();
        }
        /*******************************************************************************************/
        private string getWhoQuery()
        {
            string procLoc = "";
            string who = "";
            string[] locIDs = this.cmbWho.EditValue.ToString().Split('|');
            for (int i = 0; i < locIDs.Length; i++)
            {
                if (!String.IsNullOrWhiteSpace(locIDs[i]))
                {
                    if (procLoc.Trim().Length > 0)
                        procLoc += ",";
                    who = locIDs[i].Trim();
                    if ( who == "ALL")
                    {
                        procLoc = "";
                        break;
                    }
                    if (who == "INSURANCE")
                        who = "02";
                    else if ( who == "TRUSTS")
                        who = "01";
                    procLoc += "'" + who + "'";
                }
            }
            return procLoc.Length > 0 ? " `code` IN (" + procLoc + ") " : "";
        }
        /****************************************************************************************/
        private void cmbWho_EditValueChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            if (originalDt == null)
                return;
            DetermineView();
        }
        /****************************************************************************************/
        private void cmbDeposits_EditValueChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            if (originalDt == null)
                return;
            DetermineView();
        }
        /*******************************************************************************************/
        private string getDepositQuery()
        {
            string procLoc = "";
            string who = "";
//            string[] locIDs = this.cmbDeposits.EditValue.ToString().Split('|');
            string[] locIDs = this.cmbDeposits.Text.Split('|');
            for (int i = 0; i < locIDs.Length; i++)
            {
                if (!String.IsNullOrWhiteSpace(locIDs[i]))
                {
                    if (procLoc.Trim().Length > 0)
                        procLoc += ",";
                    who = locIDs[i].Trim();
                    if (who == "ALL")
                    {
                        procLoc = "";
                        break;
                    }
                    procLoc += "'" + who + "'";
                }
            }
            return procLoc.Length > 0 ? " `depositNumber` IN (" + procLoc + ") " : "";
        }
        /***********************************************************************************************/
        private DataTable DetermineManual( DataTable originalDt)
        {
            if (loading)
                return originalDt;
            if (originalDt == null)
                return originalDt;

            string names = getDepositQuery();
            DataRow[] dRows = originalDt.Select(names);
            DataTable dt = originalDt.Clone();
            for (int i = 0; i < dRows.Length; i++)
                dt.ImportRow(dRows[i]);
            return dt;
        }
        /***********************************************************************************************/
        private void SetupDepositCombo()
        {

        }
        /***********************************************************************************************/
        private void DetermineView ()
        {
            if (loading)
                return;
            if (originalDt == null)
                return;
            string names = getWhoQuery();
            DataRow[] dRows = originalDt.Select(names);
            DataTable dt = originalDt.Clone();
            for (int i = 0; i < dRows.Length; i++)
                dt.ImportRow(dRows[i]);

            names = getWhatQuery();
            dRows = dt.Select(names);
            dt = originalDt.Clone();
            for (int i = 0; i < dRows.Length; i++)
                dt.ImportRow(dRows[i]);

            names = getBoxQuery();
            dRows = dt.Select(names);
            dt = originalDt.Clone();
            for (int i = 0; i < dRows.Length; i++)
                dt.ImportRow(dRows[i]);

            dt = DetermineManual(dt);

            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            CalculateGoodBadUgly(dt);
            this.gridMain.ExpandAllGroups();
        }
        /***********************************************************************************************/
        private int pageMarginLeft = 0;
        private int pageMarginRight = 0;
        private int pageMarginTop = 0;
        private int pageMarginBottom = 0;
        /****************************************************************************************/
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

            G1.AdjustColumnWidths(gridMain, 0.65D, true);

            printingSystem1.Document.AutoFitToPagesWidth = 1;

            printableComponentLink1.CreateDocument();
            printableComponentLink1.ShowPreview();

            G1.AdjustColumnWidths(gridMain, 0.65D, false );
        }
        /****************************************************************************************/
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

            G1.AdjustColumnWidths(gridMain, 0.65D, true);

            printingSystem1.Document.AutoFitToPagesWidth = 1;

            printableComponentLink1.CreateDocument();
            printableComponentLink1.PrintDlg();

            G1.AdjustColumnWidths(gridMain, 0.65D, false);
        }
        /***********************************************************************************************/
        private void printableComponentLink1_BeforeCreateAreas(object sender, EventArgs e)
        {
            footerCount = 0;
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
            string report = cmbLockBox.Text;
            Printer.DrawQuad(5, 8, 8, 4, report, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            font = new Font("Ariel", 8, FontStyle.Regular);
            report = cmbWhat.Text + " / " + cmbWho.Text + " / " + cmbDeposits.Text;
            Printer.DrawQuad(10, 8, 2, 4, report, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            //            Printer.DrawQuadTicks();
        }
        /****************************************************************************************/
        private void chkGroupData_CheckedChanged(object sender, EventArgs e)
        {
            if (chkGroupData.Checked)
            {
                gridMain.Columns["code"].GroupIndex = 0;
                gridMain.Columns["location"].GroupIndex = 1;
                gridMain.OptionsBehavior.AutoExpandAllGroups = true;
                gridMain.ExpandAllGroups();
                gridMain.OptionsPrint.ExpandAllGroups = true;
                gridMain.OptionsPrint.PrintGroupFooter = true;
            }
            else
            {
                gridMain.Columns["code"].GroupIndex = -1;
                gridMain.Columns["location"].GroupIndex = -1;
                gridMain.OptionsBehavior.AutoExpandAllGroups = false;
                gridMain.CollapseAllGroups();
                gridMain.OptionsPrint.ExpandAllGroups = false;
                gridMain.OptionsPrint.PrintGroupFooter = true;
            }
            gridMain.RefreshData();
            dgv.Refresh();
        }
        /****************************************************************************************/
        private void chkDeposits_CheckedChanged(object sender, EventArgs e)
        {
            if (chkDeposits.Checked)
            {
                gridMain.Columns["location"].GroupIndex = 0;
                gridMain.Columns["depositNumber"].GroupIndex = 1;
                gridMain.OptionsBehavior.AutoExpandAllGroups = true;
                gridMain.ExpandAllGroups();
                gridMain.OptionsPrint.ExpandAllGroups = true;
                gridMain.OptionsPrint.PrintGroupFooter = true;
            }
            else
            {
                gridMain.Columns["location"].GroupIndex = -1;
                gridMain.Columns["depositNumber"].GroupIndex = -1;
                gridMain.OptionsBehavior.AutoExpandAllGroups = false;
                gridMain.CollapseAllGroups();
                gridMain.OptionsPrint.ExpandAllGroups = false;
                gridMain.OptionsPrint.PrintGroupFooter = true;
            }
            gridMain.RefreshData();
            dgv.Refresh();
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
            if (!chkDeposits.Checked)
                return;
            if (e.HasFooter)
            {
                footerCount++;
                if (footerCount >= 2)
                {
                    footerCount = 0;
                    e.PS.InsertPageBreak(e.Y);
                }
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
                originalSize = gridMain.Columns["name"].AppearanceCell.Font.Size;
                mainFont = gridMain.Columns["name"].AppearanceCell.Font;
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
        private void cmbBox_EditValueChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            if (originalDt == null)
                return;
            DetermineView();
        }
        /****************************************************************************************/
        private void deleteDepositToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            DialogResult result = MessageBox.Show("***Question*** Are you sure you want to delete this Row ?", "Delete Row Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;

            DataTable dt = (DataTable)dgv.DataSource;
            int[] rows = gridMain.GetSelectedRows();
            int dtRow = 0;
            int firstRow = 0;
            if (rows.Length > 0)
                firstRow = rows[0];
            int row = 0;
            try
            {
                loading = true;
                for (int i = 0; i < rows.Length; i++)
                {
                    row = rows[i];
                    dtRow = gridMain.GetDataSourceRowIndex(row);
                    if (dtRow < 0 || dtRow > (dt.Rows.Count - 1))
                    {
                        continue;
                    }
                    dt.Rows.RemoveAt(dtRow);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("*ERROR*** " + ex.Message.ToString());
            }

            loading = false;
            if (firstRow > (dt.Rows.Count - 1))
                firstRow = (dt.Rows.Count - 1);
            dgv.DataSource = dt;
            gridMain.RefreshData();
            dgv.Refresh();

            gridMain.FocusedRowHandle = firstRow;
            gridMain.SelectRow(firstRow);
        }
        /****************************************************************************************/
    }
}