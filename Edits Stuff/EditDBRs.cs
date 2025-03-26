using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using GeneralLib;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.BandedGrid;
using DevExpress.XtraGrid.Views.Base;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class EditDBRs : DevExpress.XtraEditors.XtraForm
    {
        private string workTable = "";
        private string workColumns = "";
        private bool modified = false;
        private DataTable workDt = null;
        private string actualFile = "";
        private string paymentType = "";
        private string workWhat = "";
        private DataTable originalDt = null;
        private bool importDP = false;
        private bool importPayments = false;
        /****************************************************************************************/
        public EditDBRs( DataTable dt, string filename )
        {
            InitializeComponent();
            workDt = dt;
            actualFile = filename;
            workWhat = "Import";
        }
        /****************************************************************************************/
        public EditDBRs( string importWhat, DataTable dt, string filename)
        {
            InitializeComponent();
            workDt = dt;
            actualFile = filename;
            workWhat = importWhat;
        }
        /****************************************************************************************/
        public EditDBRs()
        {
            InitializeComponent();
            workWhat = "Edit";
            SetupTotalsSummary();
        }
        /***********************************************************************************************/
        private void loadLocations()
        {
            string cmd = "Select * from `funeralhomes` order by `LocationCode`;";
            DataTable locDt = G1.get_db_data(cmd);
            chkComboLocNames.Properties.DataSource = locDt;
        }
        /****************************************************************************************/
        private void SetupTotalsSummary()
        {
            AddSummaryColumn("dbr", null);
            gridMain.OptionsView.ShowFooter = true;
        }
        /****************************************************************************************/
        private void AddSummaryColumn(string columnName)
        {
            gridMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            //gridMain.Columns[columnName].SummaryItem.DisplayFormat = "{0:0,0.00}";
            gridMain.Columns[columnName].SummaryItem.DisplayFormat = "{0:N2}";
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
        /****************************************************************************************/
        private void EditDBRs_Load(object sender, EventArgs e)
        {
            btnSaveAll.Hide();

            loadLocations();

            if ( workWhat == "Import")
            {
                btnRun.Hide();
                dateTimePicker1.Hide();
                dateTimePicker2.Hide();
                btnLeft.Hide();
                btnRight.Hide();
                label1.Hide();
                chkComboLocNames.Hide();
                label2.Hide();
                chkGroupLocation.Hide();
                chkGroupMonthStop.Hide();
            }

            DateTime now = DateTime.Now;
            now = now.AddMonths(-1);
            now = new DateTime(now.Year, now.Month, 1);
            this.dateTimePicker1.Value = now;

            int days = DateTime.DaysInMonth(now.Year, now.Month);
            DateTime stop = new DateTime(now.Year, now.Month, days);
            this.dateTimePicker2.Value = stop;

            if (workWhat == "Import")
            {
                string cmd = "Select * from `dbrs` WHERE `note` = 'XYZZY';";
                DataTable dt = G1.get_db_data(cmd);
                dt.Columns.Add("num");
                dt.Columns.Add("mod");

                LoadImportData(dt);

                G1.NumberDataTable(dt);

                originalDt = dt;
                dgv.DataSource = dt;

                btnSaveAll.Show();
                btnSaveAll.Refresh();
            }
            else if (workWhat == "Import DPs" || workWhat == "Import Payments")
            {
                string cmd = "Select * from `cashremitted` WHERE `runWhat` = 'XYZZY';";
                DataTable dt = G1.get_db_data(cmd);

                if ( workWhat == "Import DPs")
                    LoadImportRemitted(dt);
                else
                    LoadImportRemitted(dt);

                G1.NumberDataTable(dt);

                originalDt = dt;
                dgv2.DataSource = dt;

                tabControl1.TabPages[0].Hide();
                tabControl1.SelectedTab = tabPage2;

                gridMain2.Columns["ccFee"].Visible = true;
                gridMain2.Columns["letter"].Visible = false;
                gridMain2.Columns["pulled"].Visible = false;
                gridMain2.Columns["trust100P"].Visible = true;
                gridMain2.Columns["trust85P"].Visible = true;

                btnSaveAll.Show();
                btnSaveAll.Refresh();
            }
        }
        /***********************************************************************************************/
        private void LoadImportData ( DataTable dt )
        {
            bool DPs = false;
            bool payments = false;
            if (actualFile.ToUpper().IndexOf("DP") > 0)
                DPs = true;
            else
                payments = true;

            string month = DetermineMonth(actualFile);
            string year = DetermineYear(actualFile);

            DateTime startDate = DateTime.Now;
            DateTime stopDate = DateTime.Now;
            DetermineDate(month, year, ref startDate, ref stopDate);
            string date1 = startDate.ToString("yyyy-MM-dd");
            string date2 = stopDate.ToString("yyyy-MM-dd");

            paymentType = "payment";
            string contractCol = "COL 2";
            string dbrCol = "COL 7";
            if (DPs)
            {
                dbrCol = "COL 6";
                paymentType = "DP";
            }
            string dateCol = "COL 5";
            bool foundDBR = false;

            string contractNumber = "";
            DateTime deceasedDate = DateTime.Now;
            DateTime payDate8 = DateTime.Now;
            double dbr = 0D;
            string cmd = "";
            DataTable dx = null;

            DataRow dRow = null;
            string str = "";

            for ( int i=0; i<workDt.Rows.Count; i++)
            {
                if ( !foundDBR )
                {
                    for ( int j=0; j<workDt.Columns.Count; j++)
                    {
                        str = workDt.Rows[i][j].ObjToString();
                        if (!foundDBR)
                        {
                            if (str.ToUpper() == "LAST NAME")
                            {
                                foundDBR = true;
                                continue;
                            }
                        }
                        else
                        {
                            if ( str.ToUpper() == "DBR")
                            {
                                dbrCol = workDt.Columns[j].ColumnName.Trim();
                                break;
                            }
                        }
                    }
                }
                if (!foundDBR)
                    continue;
                contractNumber = workDt.Rows[i][contractCol].ObjToString();
                if (String.IsNullOrWhiteSpace(contractNumber))
                    continue;
                if ( contractNumber == "WC23034L")
                {
                }
                cmd = "Select * from `customers` WHERE `contractNumber` = '" + contractNumber + "';";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count <= 0)
                    continue;
                deceasedDate = dx.Rows[0]["deceasedDate"].ObjToDateTime();
                if (deceasedDate.Year < 1000)
                    continue;
                dbr = workDt.Rows[i][dbrCol].ObjToDouble();
                if (dbr == 0D)
                    continue;
                if ( payments )
                {
                    cmd = "Select * from payments where `contractNumber` = '" + contractNumber + "' AND `payDate8` <= '" + date2 + "' ORDER by `payDate8` DESC;";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                    {
                        payDate8 = dx.Rows[0]["payDate8"].ObjToDateTime();
                    }
                    else
                        payDate8 = stopDate;
                }
                else
                    payDate8 = workDt.Rows[i][dateCol].ObjToDateTime();

                dRow = dt.NewRow();
                dRow["contractNumber"] = contractNumber;
                dRow["dbr"] = dbr;
                dRow["paymentType"] = paymentType;
                dRow["deceasedDate"] = G1.DTtoMySQLDT(deceasedDate.ToString("yyyy-MM-dd"));
                dRow["payDate8"] = G1.DTtoMySQLDT(payDate8.ToString("yyyy-MM-dd"));
                dRow["cashRemitStartDate"] = G1.DTtoMySQLDT(startDate.ToString("yyyy-MM-dd"));
                dRow["cashRemitStopDate"] = G1.DTtoMySQLDT(stopDate.ToString("yyyy-MM-dd"));
                dt.Rows.Add(dRow);
            }
        }
        /***********************************************************************************************/
        private void LoadImportRemitted(DataTable dt)
        {
            bool DPs = false;
            bool payments = false;
            if (actualFile.ToUpper().IndexOf("DP") > 0)
                DPs = true;
            else
                payments = true;

            string month = DetermineMonth(actualFile);
            string year = DetermineYear(actualFile);

            DateTime startDate = DateTime.Now;
            DateTime stopDate = DateTime.Now;
            DetermineDate(month, year, ref startDate, ref stopDate);
            string date1 = startDate.ToString("yyyy-MM-dd");
            string date2 = stopDate.ToString("yyyy-MM-dd");

            paymentType = "payment";
            string contractCol = "COL 2";
            string dbrCol = "COL 9";
            if (DPs)
            {
                dbrCol = "COL 6";
                paymentType = "DP";
            }
            string dateCol = "COL 5";
            bool foundDBR = false;

            DataTable funDt = G1.get_db_data("Select * from `funeralhomes`;");


            string contractNumber = "";
            DateTime deceasedDate = DateTime.Now;
            DateTime birthDate = DateTime.Now;
            DateTime payDate8 = DateTime.Now;
            string depositNumber = "";
            double dbr = 0D;
            string fName = "";
            string lName = "";
            string cmd = "";
            DataTable dx = null;
            double downPayment = 0D;
            double payment = 0D;
            double trust100 = 0D;
            double trust85 = 0D;
            double ccFee = 0D;
            string trust = "";
            string loc = "";
            string edited = "";
            DataTable cemDt = null;
            string trustName = "";
            string loct = "";
            double liInterest = 0D;
            double interest = 0;
            string agent = "";
            string locationName = "";
            double debit = 0D;
            double credit = 0D;
            string debitReason = "";
            string creditReason = "";

            DataRow dRow = null;
            DataRow[] ddRx = null;
            string str = "";
            double dpp = 0D;

            string address1 = "";
            string address2 = "";
            string city = "";
            string state = "";
            string zip1 = "";
            string zip2 = "";
            string sex = "";
            string ssn = "";

            contractCol = "COL 2";
            string trust85Col = "COL 10";
            string trust100Col = "COL 9";
            if ( payments )
            {
                trust85Col = "COL 7";
                trust100Col = "COL 6";
            }

            for (int i = 0; i < workDt.Rows.Count; i++)
            {
                contractNumber = workDt.Rows[i][contractCol].ObjToString();
                if (String.IsNullOrWhiteSpace(contractNumber))
                    continue;

                Trust85.decodeContractNumber(contractNumber, ref trust, ref loc);

                downPayment = 0D;
                if ( DPs )
                    downPayment = workDt.Rows[i]["COL 8"].ObjToDouble();

                trust100 = workDt.Rows[i][trust100Col].ObjToDouble();
                trust85 = workDt.Rows[i][trust85Col].ObjToDouble();
                dbr = workDt.Rows[i][dbrCol].ObjToDouble();

                payment = 0D;
                ccFee = 0D;
                edited = "";
                interest = 0D;
                liInterest = 0D;
                dpp = 0D;
                agent = "";
                depositNumber = "";
                locationName = "";

                cmd = "Select * from `customers` WHERE `contractNumber` = '" + contractNumber + "';";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count <= 0)
                    continue;


                deceasedDate = DateTime.MinValue;
                if ( dbr > 0D )
                    deceasedDate = dx.Rows[0]["deceasedDate"].ObjToDateTime();

                fName = dx.Rows[0]["firstName"].ObjToString();
                lName = dx.Rows[0]["lastName"].ObjToString();

                address1 = dx.Rows[0]["address1"].ObjToString();
                address2 = dx.Rows[0]["address2"].ObjToString();
                city = dx.Rows[0]["city"].ObjToString();
                state = dx.Rows[0]["state"].ObjToString();
                zip1 = dx.Rows[0]["zip1"].ObjToString();
                zip2 = dx.Rows[0]["zip2"].ObjToString();
                sex = dx.Rows[0]["sex"].ObjToString();
                ssn = dx.Rows[0]["ssn"].ObjToString();
                birthDate = dx.Rows[0]["birthDate"].ObjToDateTime();

                if (!payments)
                    cmd = "Select * FROM `payments` WHERE `contractNumber` = '" + contractNumber + "' AND `payDate8` <= '" + date2 + "' AND `downPayment` > '0.00' ORDER by `payDate8` DESC;";
                else
                {
                    cmd = "Select * FROM `payments` WHERE `contractNumber` = '" + contractNumber + "' AND `payDate8` <= '" + date2 + "' AND `trust85P` = '" + trust85.ToString() + "' ORDER by `payDate8` DESC;";
                }
                dx = G1.get_db_data(cmd);
                if ( dx.Rows.Count <= 0 )
                {
                    cmd = "Select * FROM `payments` WHERE `contractNumber` = '" + contractNumber + "' AND `payDate8` <= '" + date2 + "' ORDER by `payDate8` DESC;";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0 )
                    {
                        dx.Rows[0]["trust85P"] = trust85;
                        dx.Rows[0]["trust100P"] = trust100;
                    }
                }
                if (dx.Rows.Count > 0)
                {
                    payDate8 = dx.Rows[0]["payDate8"].ObjToDateTime();
                    ccFee = dx.Rows[0]["ccFee"].ObjToDouble();
                    edited = dx.Rows[0]["edited"].ObjToString();
                    interest = dx.Rows[0]["interestPaid"].ObjToDouble();
                    if (trust.ToUpper() == "LI")
                        liInterest = interest;
                    agent = dx.Rows[0]["agentNumber"].ObjToString();
                    depositNumber = dx.Rows[0]["depositNumber"].ObjToString();
                    payment = dx.Rows[0]["paymentAmount"].ObjToDouble();

                    debit = dx.Rows[0]["debitAdjustment"].ObjToDouble();
                    credit = dx.Rows[0]["creditAdjustment"].ObjToDouble();
                    debitReason = dx.Rows[0]["debitReason"].ObjToString();
                    creditReason = dx.Rows[0]["creditReason"].ObjToString();

                    if (dbr > 0D)
                    {
                        trust85 = dx.Rows[0]["trust85P"].ObjToDouble();
                        trust100 = dx.Rows[0]["trust100P"].ObjToDouble();
                        trust85 = dbr;
                    }
                }
                else
                    payDate8 = stopDate;

                if (funDt.Rows.Count > 0 && !String.IsNullOrWhiteSpace(loc))
                {
                    if (loc == "FO")
                        loc = "B";
                    else if (loc == "WC")
                        loc = "WF";
                    DataRow[] dr = funDt.Select("keycode='" + loc + "'");
                    if (dr.Length > 0)
                        locationName = dr[0]["name"].ObjToString();
                }

                trust = trust.ToUpper();
                trustName = "";
                if (trust == "L" || trust == "LI")
                    trustName = "FDLIC";
                else if (trust == "U" || trust == "UI")
                    trustName = "UNITY";
                else if (trust == "D" || trust == "DI")
                    trustName = "BANCORPSOUTH";
                else
                {
                    if (edited.ToUpper() == "CEMETERY")
                    {
                        if (cemDt == null)
                            cemDt = G1.get_db_data("Select * from `cemeteries`;");

                        loct = dt.Rows[i]["location"].ObjToString(); // RAMMA ZAMMA
                        if (!String.IsNullOrWhiteSpace(loct))
                        {
                            loct += " ";
                            ddRx = cemDt.Select("loc='" + loct + "'");
                            if (ddRx.Length > 0)
                                loct += ddRx[0]["description"].ObjToString().Trim() + " Cemetery";
                            else
                                loct += "Cemetery";
                        }
                        else
                            loct += "Cemetery";
                        locationName = loct;
                        trustName = "PC";
                    }
                    else
                        trustName = "PB";
                }


                //payDate8 = workDt.Rows[i]["COL 5"].ObjToDateTime();
                //fName = workDt.Rows[i]["COL 4"].ObjToString();
                //lName = workDt.Rows[i]["COL 3"].ObjToString();

                dRow = dt.NewRow();
                dRow["contractNumber"] = contractNumber;
                dRow["dbr"] = dbr;
                if (dbr > 0D)
                    dRow["SetAsDBR"] = "Y";
                dRow["firstName"] = fName;
                dRow["lastName"] = lName;
                dRow["customer"] = fName + " " + lName;
                dRow["trust100P"] = trust100;
                dRow["trust85P"] = trust85; ;
                dRow["downPayment"] = downPayment;
                dRow["paymentAmount"] = payment;
                dRow["ccFee"] = ccFee;
                dRow["dpp"] = downPayment - ccFee;
                dRow["newBusiness"] = downPayment;
                dRow["interestPaid1"] = interest;
                dRow["LiInterest"] = liInterest;
                dRow["loc"] = loc;
                dRow["trust"] = trust;
                dRow["Trust Name"] = trustName;
                dRow["agentNumber"] = agent;
                dRow["depositNumber"] = depositNumber;
                dRow["Location Name"] = locationName;
                dRow["debitAdjustment"] = debit;
                dRow["creditAdjustment"] = credit;
                dRow["debitReason"] = debitReason;
                dRow["creditReason"] = creditReason;
                if ( deceasedDate.Year > 1000 )
                    dRow["deceasedDate"] = G1.DTtoMySQLDT(deceasedDate.ToString("yyyy-MM-dd"));
                dRow["address1"] = address1;
                dRow["address2"] = address2;
                dRow["city"] = city;
                dRow["state"] = state;
                dRow["zip1"] = zip1;
                dRow["zip2"] = zip2;
                dRow["sex"] = sex;
                dRow["ssn"] = ssn;
                dRow["birthDate"] = G1.DTtoMySQLDT(birthDate.ToString("yyyy-MM-dd"));
                dRow["payDate8"] = G1.DTtoMySQLDT(payDate8.ToString("yyyy-MM-dd"));
                dRow["runDate1"] = G1.DTtoMySQLDT(startDate.ToString("yyyy-MM-dd"));
                dRow["runDate2"] = G1.DTtoMySQLDT(stopDate.ToString("yyyy-MM-dd"));
                dt.Rows.Add(dRow);
            }
        }
        /***********************************************************************************************/
        private DataTable KillDuplicates ( DataTable dt)
        {
            DataView tempview = dt.DefaultView;
            tempview.Sort = "lastName asc, firstName asc";
            dt = tempview.ToTable();
            string lastName = "";
            string firstName = "";
            string license = "";
            string location = "";

            string lastLastName = "";
            string lastFirstName = "";
            string lastLicense = "";
            string lastLocation = "";

            for ( int i=dt.Rows.Count-1; i>= 0; i--)
            {
                lastName = dt.Rows[i]["lastName"].ObjToString();
                firstName = dt.Rows[i]["firstName"].ObjToString();
                license = dt.Rows[i]["license"].ObjToString();
                location = dt.Rows[i]["location"].ObjToString();

                if (String.IsNullOrWhiteSpace(lastName))
                    dt.Rows[i]["mod"] = "D";
                else
                {
                    if (lastName == lastLastName && firstName == lastFirstName && license == lastLicense && location == lastLocation)
                    {
                        dt.Rows[i]["mod"] = "D";
                        modified = true;
                    }
                    lastLastName = lastName;
                    lastFirstName = firstName;
                    lastLicense = license;
                    lastLocation = location;
                }
            }
            tempview = dt.DefaultView;
            tempview.Sort = "order asc";
            dt = tempview.ToTable();
            return dt;
        }
        /****************************************************************************************/
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            modified = true;
            btnSaveAll.Show();
        }
        /****************************************************************************************/
        private void pictureBox11_Click(object sender, EventArgs e)
        { // Delete Current Row
            DataRow dr = gridMain.GetFocusedDataRow();
            string lastName = dr["lastName"].ObjToString();
            string firstName = dr["firstName"].ObjToString();
            string middleName = dr["middleName"].ObjToString();
            string data = lastName + ", " + firstName + " " + middleName;
            DialogResult result = MessageBox.Show("***Question*** Are you sure you want to DELETE this Arranger (" + data + ") ?", "Delete Arranger Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt == null)
                return;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            dr["Mod"] = "D";
            dt.Rows[row]["mod"] = "D";
            gridMain_CellValueChanged(null, null);
        }
        /****************************************************************************************/
        private void gridMain_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt == null)
                return;
            string delete = dt.Rows[row]["mod"].ObjToString();
            if (delete.ToUpper() == "D")
            {
                e.Visible = false;
                e.Handled = true;
            }
        }
        /****************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        { // Spy Glass
            if (gridMain.OptionsFind.AlwaysVisible == true)
                gridMain.OptionsFind.AlwaysVisible = false;
            else
                gridMain.OptionsFind.AlwaysVisible = true;
        }
        /****************************************************************************************/
        private void btnSaveAll_Click(object sender, EventArgs e)
        {
            if (workWhat == "Import DPs" || workWhat == "Import Payments")
            {
                SaveCashRemitted();
                return;
            }
            DataTable dt = (DataTable)dgv.DataSource;
            string record = "";
            string mod = "";
            string data = "";
            string contractNumber = "";
            DateTime payDate8 = DateTime.Now;
            double payment = 0D;
            DateTime deceasedDate = DateTime.Now;

            string month = DetermineMonth(actualFile);
            string year = DetermineYear(actualFile);

            DateTime startDate = DateTime.Now;
            DateTime stopDate = DateTime.Now;
            DetermineDate(month, year, ref startDate, ref stopDate);

            string date1 = startDate.ToString("yyyy-MM-dd");
            string date2 = stopDate.ToString("yyyy-MM-dd");
            string cmd = "DELETE from `dbrs` where `cashRemitStartDate` >= '" + date1 + "' AND `cashRemitStopDate` <= '" + date2 + "' AND `paymentType` = '" + paymentType + "' ";
            cmd += ";";
            try
            {
                G1.get_db_data(cmd);
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Delete Previous Data " + ex.Message.ToString());
            }

            this.Cursor = Cursors.WaitCursor;

            dt = G1.GetGridViewTable(gridMain, dt);

            double dbr = 0D;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                mod = dt.Rows[i]["mod"].ObjToString();
                record = dt.Rows[i]["record"].ObjToString();
                if (mod == "D")
                {
                    if (record == "-1")
                        continue;
                    if (!String.IsNullOrWhiteSpace(record))
                    {
                        G1.delete_db_table("dbrs", "record", record);
                        dt.Rows[i]["record"] = "-1";
                    }
                    continue;
                }
                if (String.IsNullOrWhiteSpace(record))
                    record = G1.create_record("dbrs", "note", "-1");
                if (G1.BadRecord("dbrs", record))
                    return;
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                dbr = dt.Rows[i]["dbr"].ObjToDouble();
                payDate8 = dt.Rows[i]["payDate8"].ObjToDateTime();
                deceasedDate = dt.Rows[i]["deceasedDate"].ObjToDateTime();
                G1.update_db_table("dbrs", "record", record, new string[] { "note", "", "contractNumber", contractNumber, "dbr", dbr.ToString(), "payDate8", payDate8.ToString("yyyy-MM-dd"), "deceasedDate", deceasedDate.ToString("yyyy-MM-dd"), "cashRemitStartDate", date1, "cashRemitStopDate", date2, "paymentType", paymentType });
                dt.Rows[i]["record"] = record;
            }
            modified = false;
            btnSaveAll.Hide();

            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void SaveCashRemitted ()
        {
            string month = DetermineMonth(actualFile);
            string year = DetermineYear(actualFile);

            DateTime startDate = DateTime.Now;
            DateTime stopDate = DateTime.Now;
            DetermineDate(month, year, ref startDate, ref stopDate);
            string date1 = startDate.ToString("yyyy-MM-dd");
            string date2 = stopDate.ToString("yyyy-MM-dd");

            string cmd = "";

            using (Ask fmrmyform = new Ask("Do you want to delete previous data for date ending " + date2 + "? "))
            {
                fmrmyform.Text = "";
                fmrmyform.ShowDialog();
                string answer = fmrmyform.Answer.Trim().ToUpper();
                if (String.IsNullOrWhiteSpace(answer))
                {
                    MessageBox.Show("***Info*** Okay, not saving any data!!!");
                    return;
                }
                if (answer.ToUpper() != "YES" && answer.ToUpper() != "NO" )
                {
                    MessageBox.Show("***ERROR*** Invalid Answer . . . Enter Yes or No !!!");
                    return;
                }
                if ( answer.ToUpper() == "YES")
                {
                    cmd = "DELETE from `cashremitted` where `runDate1` >= '" + date1 + "' AND `runDate2` <= '" + date2 + "' ";
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

            }

            DataTable dt = (DataTable)dgv2.DataSource;
            if (dt == null)
                return;

            this.Cursor = Cursors.WaitCursor;

            string record = "";
            string firstName = "";
            string lastName = "";
            string contractNumber = "";
            DateTime deceasedDate = DateTime.Now;
            DateTime birthDate = DateTime.Now;
            DateTime payDate8 = DateTime.Now;
            string depositNumber = "";
            double dbr = 0D;
            double downPayment = 0D;
            double newBusiness = 0D;
            double payment = 0D;
            double trust100 = 0D;
            double trust85 = 0D;
            double ccFee = 0D;
            string trust = "";
            string loc = "";
            string edited = "";
            string trustName = "";
            string loct = "";
            double liInterest = 0D;
            double interest = 0;
            string agent = "";
            string locationName = "";
            double debit = 0D;
            double credit = 0D;
            string debitReason = "";
            string creditReason = "";

            string str = "";
            double dpp = 0D;

            string address1 = "";
            string address2 = "";
            string city = "";
            string state = "";
            string zip1 = "";
            string zip2 = "";
            string sex = "";
            string ssn = "";
            string setAsDBR = "";

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                dbr = dt.Rows[i]["dbr"].ObjToDouble();
                payDate8 = dt.Rows[i]["payDate8"].ObjToDateTime();
                deceasedDate = dt.Rows[i]["deceasedDate"].ObjToDateTime();
                firstName = dt.Rows[i]["firstName"].ObjToString();
                lastName = dt.Rows[i]["lastName"].ObjToString();

                birthDate = dt.Rows[i]["birthDate"].ObjToDateTime();
                address1 = dt.Rows[i]["address1"].ObjToString();
                address2 = dt.Rows[i]["address2"].ObjToString();
                city = dt.Rows[i]["city"].ObjToString();
                state = dt.Rows[i]["state"].ObjToString();
                zip1 = dt.Rows[i]["zip1"].ObjToString();
                zip2 = dt.Rows[i]["zip2"].ObjToString();
                sex = dt.Rows[i]["sex"].ObjToString();
                ssn = dt.Rows[i]["ssn"].ObjToString();
                dpp = dt.Rows[i]["dpp"].ObjToDouble();
                downPayment = dt.Rows[i]["downPayment"].ObjToDouble();
                newBusiness = dt.Rows[i]["newBusiness"].ObjToDouble();
                payment = dt.Rows[i]["paymentAmount"].ObjToDouble();
                ccFee = dt.Rows[i]["ccFee"].ObjToDouble();
                edited = dt.Rows[i]["edited"].ObjToString();
                trust100 = dt.Rows[i]["trust100P"].ObjToDouble();
                trust85 = dt.Rows[i]["trust85P"].ObjToDouble();
                setAsDBR = dt.Rows[i]["SetAsDBR"].ObjToString();

                loc = dt.Rows[i]["loc"].ObjToString();
                locationName = dt.Rows[i]["Location Name"].ObjToString();
                trust = dt.Rows[i]["trust"].ObjToString();
                trustName = dt.Rows[i]["Trust Name"].ObjToString();
                agent = dt.Rows[i]["agentNumber"].ObjToString();

                depositNumber = dt.Rows[i]["depositNumber"].ObjToString();

                debit = dt.Rows[i]["debitAdjustment"].ObjToDouble();
                credit = dt.Rows[i]["creditAdjustment"].ObjToDouble();
                debitReason = dt.Rows[i]["debitReason"].ObjToString();
                creditReason = dt.Rows[i]["creditReason"].ObjToString();

                record = dt.Rows[i]["record"].ObjToString();
                if (String.IsNullOrWhiteSpace(record))
                    record = G1.create_record("cashremitted", "firstName1", "-1");
                if (G1.BadRecord("cashremitted", record))
                    return;

                G1.update_db_table("cashremitted", "record", record, new string[] { "firstName1", firstName, "firstName", firstName, "lastName", lastName, "runWhat", "Trusts", "contractNumber", contractNumber, "dbr", dbr.ToString(), "payDate8", payDate8.ToString("yyyy-MM-dd"), "deceasedDate", deceasedDate.ToString("yyyy-MM-dd"), "runDate1", date1, "runDate2", date2 });

                G1.update_db_table("cashremitted", "record", record, new string[] { "birthDate", birthDate.ToString("yyyy-MM-dd"), "downPayment1", downPayment.ToString(), "downPayment", downPayment.ToString(), "paymentAmount", payment.ToString(), "ccFee", ccFee.ToString(), "trust100P", trust100.ToString(), "trust85P", trust85.ToString(), "edited", edited, "newBusiness", newBusiness.ToString() });
                G1.update_db_table("cashremitted", "record", record, new string[] { "address1", address1, "address2", address2, "city", city, "state", state, "zip1", zip1, "zip2", zip2, "sex", sex, "ssn", ssn, "SetAsDBR", setAsDBR });

                G1.update_db_table("cashremitted", "record", record, new string[] { "loc", loc, "Location Name", locationName, "trust", trust, "Trust Name", trustName, "agentNumber", agent, "depositNumber", depositNumber, "debitAdjustment", debit.ToString(), "creditAdjustment", credit.ToString(), "debitReason", debitReason, "creditReason", creditReason });

                dt.Rows[i]["record"] = record;
            }
            modified = false;
            btnSaveAll.Hide();

            this.Cursor = Cursors.Default;
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
        private void EditTable_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!btnSaveAll.Visible)
                return;
            DialogResult result = MessageBox.Show("***Question*** Data has been modified.\nDo you really want to exit WITHOUT saving your data?", "Data Modified Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
                return;
            e.Cancel = true;
        }
        /****************************************************************************************/
        private void repositoryItemComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            modified = true;
            btnSaveAll.Show();
        }
        /****************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            if (dr == null)
                return;
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
        private string DetermineMonth( string actualFile )
        {
            string[] Months = new string[12];

            Months[0] = "January";
            Months[1] = "February";
            Months[2] = "March";
            Months[3] = "April";
            Months[4] = "May";
            Months[5] = "June";
            Months[6] = "July";
            Months[7] = "August";
            Months[8] = "September";
            Months[9] = "October";
            Months[10] = "November";
            Months[11] = "December";

            string month = "";

            try
            {
                string name = actualFile.Trim().ToUpper();
                name = name.Trim();

                string str = "";
                string year = "";
                string[] Lines = name.Split(' ');
                month = Lines[0].Trim();
                month = G1.force_lower_line(month);
                for (int i = 0; i < Months.Length; i++)
                {
                    str = Months[i].ObjToString().Trim();
                    if (month.IndexOf(str) == 0)
                    {
                        month = month.Replace(str, "");
                        if (!String.IsNullOrWhiteSpace(month))
                        {
                            year = month.Trim();
                            month = str;
                            break;
                        }
                        else
                        {
                            month = str;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return month;
        }
        /***********************************************************************************************/
        private string DetermineYear( string actualFile )
        {
            string year = "";
            try
            {
                string name = actualFile.Trim().ToUpper();
                name = name.Trim();

                name = name.Replace(".xlsx", "");
                name = name.Replace(".xls", "");
                name = name.Replace(".XLSX", "");
                name = name.Replace(".XLS", "");
                string[] Lines = name.Split(' ');
                string str = Lines[1].Trim();
                if (G1.validate_numeric(str))
                {
                    int iyear = str.ObjToInt32();
                    if (iyear < 100)
                        iyear += 2000;
                    year = iyear.ToString();
                }
                else
                    str = year;
            }
            catch (Exception ex)
            {
            }
            return year;
        }
        /***********************************************************************************************/
        private void DetermineDate( string month, string yearStr, ref DateTime startDate, ref DateTime stopDate )
        {
            DateTime date = DateTime.Now;
            string date1 = "";
            int year = yearStr.ObjToInt32();
            if (month.ToUpper() == "JANUARY")
                date = new DateTime(year, 1, 31);
            else if (month.ToUpper() == "FEBRUARY")
                date = new DateTime(year, 2, 28);
            else if (month.ToUpper() == "MARCH")
                date = new DateTime(year, 3, 31);
            else if (month.ToUpper() == "APRIL")
                date = new DateTime(year, 4, 30);
            else if (month.ToUpper() == "MAY")
                date = new DateTime(year, 5, 31);
            else if (month.ToUpper() == "JUNE")
                date = new DateTime(year, 6, 30);
            else if (month.ToUpper() == "JULY")
                date = new DateTime(year, 7, 31);
            else if (month.ToUpper() == "AUGUST")
                date = new DateTime(year, 8, 31);
            else if (month.ToUpper() == "SEPTEMBER")
                date = new DateTime(year, 9, 30);
            else if (month.ToUpper() == "OCTOBER")
                date = new DateTime(year, 10, 31);
            else if (month.ToUpper() == "NOVEMBER")
                date = new DateTime(year, 11, 30);
            else if (month.ToUpper() == "DECEMBER")
                date = new DateTime(year, 12, 31);

            stopDate = date;
            startDate = new DateTime(date.Year, date.Month, 1);
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
                    e.DisplayText = date.ToString("MM/dd/yyyy");
                    if (date.Year < 30)
                        e.DisplayText = "";
                }
            }
        }
        /****************************************************************************************/
        private void btnLeft_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            DateTime now = this.dateTimePicker1.Value;
            now = now.AddMonths(-1);
            now = new DateTime(now.Year, now.Month, 1);
            this.dateTimePicker1.Value = now;
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            this.dateTimePicker2.Value = new DateTime(now.Year, now.Month, days);

            DateTime startDate = now;
            DateTime stopDate = this.dateTimePicker2.Value;

            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void btnRight_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            DateTime now = this.dateTimePicker1.Value;
            now = now.AddMonths(1);
            now = new DateTime(now.Year, now.Month, 1);
            this.dateTimePicker1.Value = now;
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            this.dateTimePicker2.Value = new DateTime(now.Year, now.Month, days);

            DateTime startDate = now;
            DateTime stopDate = this.dateTimePicker2.Value;

            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void btnRun_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                DateTime date = dateTimePicker1.Value;
                string date1 = G1.DateTimeToSQLDateTime(date);
                date = dateTimePicker2.Value;
                string date2 = G1.DateTimeToSQLDateTime(date);

                string cmd = "SELECT * FROM `dbrs` WHERE `payDate8` >= '" + date1 + "' and `payDate8` <= '" + date2 + "' ";

                cmd += "ORDER BY `payDate8` ";
                cmd += ";";

                //cmd = "SELECT * from `cashRemitted` WHERE `payDate8` >= '" + date1 + "' and `payDate8` <= '" + date2 + "' ";
                //cmd += " AND `setAsDBR` = 'Y' ";
                //cmd += "ORDER BY `payDate8` "; // Contract C23007LI died 10/10/2023 so rerun of Cash Remitted caused all payments before October to be DBR. Can't use this
                //cmd += ";";

                DataTable dt = G1.get_db_data(cmd);

                dt.Columns.Add("num");
                dt.Columns.Add("mod");

                dt = AddLocations(dt);

                G1.NumberDataTable(dt);

                originalDt = dt;
                dgv.DataSource = dt;

                this.Cursor = Cursors.Default;
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** " + ex.Message.ToString());
            }
        }
        /****************************************************************************************/
        private DataTable AddLocations(DataTable dt)
        {
            if (G1.get_column_number(dt, "loc") < 0)
                dt.Columns.Add("loc");
            string contractNumber = "";
            string contract = "";
            string trust = "";
            string loc = "";
            //chkComboLocNames.Properties.DataSource = locDt;

            DataRow[] dRows = null;

            DataTable locDt = (DataTable)chkComboLocNames.Properties.DataSource;
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                contract = Trust85.decodeContractNumber(contractNumber, ref trust, ref loc);
                if (!String.IsNullOrWhiteSpace(loc))
                {
                    dRows = locDt.Select("keycode='" + loc + "'");
                    if (dRows.Length > 0)
                        dt.Rows[i]["loc"] = dRows[0]["locationCode"].ObjToString();
                }
            }
            return dt;
        }
        /****************************************************************************************/
        private void chkComboLocNames_EditValueChanged(object sender, EventArgs e)
        {
            string names = getLocationNameQuery();
            DataRow[] dRows = originalDt.Select(names);
            DataTable dt = originalDt.Clone();
            for (int i = 0; i < dRows.Length; i++)
                dt.ImportRow(dRows[i]);
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
        }
        /*******************************************************************************************/
        private string getLocationNameQuery()
        {
            string procLoc = "";
            string[] locIDs = this.chkComboLocNames.EditValue.ToString().Split('|');
            for (int i = 0; i < locIDs.Length; i++)
            {
                if (!String.IsNullOrWhiteSpace(locIDs[i]))
                {
                    if (procLoc.Trim().Length > 0)
                        procLoc += ",";
                    procLoc += "'" + locIDs[i].Trim() + "'";
                }
            }
            return procLoc.Length > 0 ? " `loc` IN (" + procLoc + ") " : "";
        }
        /****************************************************************************************/
        private void chkGroupLocation_CheckedChanged(object sender, EventArgs e)
        {
            if ( chkGroupLocation.Checked )
            {
                gridMain.Columns["loc"].GroupIndex = 0;
                if (chkGroupMonthStop.Checked)
                    gridMain.Columns["cashRemitStopDate"].GroupIndex = 1;
                string locations = chkComboLocNames.Text.Trim();
                if (!String.IsNullOrWhiteSpace(locations))
                {
                    gridMain.Columns["loc"].GroupIndex = 1;
                }
                gridMain.ExpandAllGroups();
            }
            else
            {
                gridMain.Columns["loc"].GroupIndex = -1;
            }

            gridMain.RefreshData();
            gridMain.RefreshEditor(true);
            dgv.Refresh();
        }
        /****************************************************************************************/
        private void chkGroupMonthStop_CheckedChanged(object sender, EventArgs e)
        {
            if (chkGroupMonthStop.Checked)
            {
                gridMain.Columns["cashRemitStopDate"].GroupIndex = 0;
                if (chkGroupLocation.Checked)
                    gridMain.Columns["loc"].GroupIndex = 1;
                string locations = chkComboLocNames.Text.Trim();
                if (!String.IsNullOrWhiteSpace(locations))
                {
                    gridMain.Columns["loc"].GroupIndex = 1;
                }
                gridMain.ExpandAllGroups();
            }
            else
            {
                gridMain.Columns["cashRemitStopDate"].GroupIndex = -1;
            }

            gridMain.RefreshData();
            gridMain.RefreshEditor(true);
            dgv.Refresh();
        }
        /****************************************************************************************/
    }
}