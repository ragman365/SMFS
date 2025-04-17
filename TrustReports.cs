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
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class TrustReports : DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        private PleaseWait pleaseForm = null;
        private DataTable original_SMFS_dt = null;
        private DataTable paymentsDt = null;
        private DataTable newContractsDt = null;
        private DataTable trust85Dt = null;
        private DataTable customerDt = null;
        private DataTable originalDt = null;
        private DateTime workDate2 = DateTime.Now;
        private bool RunClicked = false;
        private DateTime lastSaveDate = DateTime.Now;

        private bool foundLocalPref6 = false;
        private bool foundLocalPref2 = false;
        private bool foundLocalPref7 = false;
        private bool foundLocalPref8 = false;
        private DataTable pre2002Dt = null;
        public static DataTable trustReportDt = null;

        //        private DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gridMain8 = new DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView();
        //        private DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gridMain8 = new DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView();
        /***********************************************************************************************/
        public TrustReports(DataTable custDt, DataTable t85, DateTime date2)
        {
            InitializeComponent();
            //            trust85Dt = t85.Copy();
            trust85Dt = t85;
            customerDt = custDt;
            workDate2 = date2;
            //            originalDt = customerDt.Copy();
        }
        /****************************************************************************************/
        private void ImportAS400(ref DataTable paymentsDt, ref DataTable newContractsDt)
        {
            string newContracts = "SEPTEMBER_2018_DP_AND_MONTHLY";
            string payments = "SEPTEMBER_2018_MONTHLY";

            string file = "C:/Users/Robby/Downloads/" + payments + ".csv";
            if (File.Exists(file))
                paymentsDt = Import.ImportCSVfile(file);

            file = "C:/Users/Robby/Downloads/" + newContracts + ".csv";
            if (File.Exists(file))
                newContractsDt = Import.ImportCSVfile(file);
        }
        /***********************************************************************************************/
        private void TrustReports_Load(object sender, EventArgs e)
        {
            tabControl1.TabPages.Remove(tabCompare);
            tabControl1.TabPages.Remove(tabDiff);
            tabControl1.TabPages.Remove(tabCompareBalance);
            btnVerify.Hide();
            progressBar2.Hide();
            progressBar1.Hide();
            lblStatus.Hide();
            myStatus.Hide();
            label2.Hide();
            panel1Top.Hide();
            btnVerify.Hide();
            if (SMFS.activeSystem.ToUpper() != "RILES")
            {
                for ( int i=0; i<menuStrip1.Items.Count; i++)
                {
                    if ( menuStrip1.Items[i].Name.Trim()  == "rilesToolStripMenuItem")
                    {
                        menuStrip1.Items.RemoveAt(i);
                        break;
                    }
                }
            }

            string cmd = "Select * from `pre2002`;";
            pre2002Dt = G1.get_db_data(cmd);

            string skinName = "";
            //foundLocalPref2 = G1.RestoreGridLayout(this, this.dgv2, gridMain2, LoginForm.username, "TrustReportLayout2", ref skinName);

            DateTime now = DateTime.Now;
            now = now.AddMonths(-1);
            now = new DateTime(now.Year, now.Month, 1);
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            DateTime stop = new DateTime(now.Year, now.Month, days);
            this.dateTimePicker2.Value = stop;

            //ImportAS400(ref paymentsDt, ref newContractsDt);
            if (!LoginForm.administrator)
            {
                lblContract.Hide();
                txtContract.Hide();
            }
            lblTotal.Hide();
            barImport.Hide();

            //            this.dateTimePicker2.Value = workDate2;

            AddSummaryColumn("beginningBalance", gridMain6);
            AddSummaryColumn("ytdPrevious", gridMain6);
            AddSummaryColumn("paymentCurrMonth", gridMain6);
            AddSummaryColumn("currentPayments", gridMain6);
            AddSummaryColumn("currentRemovals", gridMain6);
            AddSummaryColumn("endingBalance", gridMain6);
            AddSummaryColumn("dbr", gridMain6);
            AddSummaryColumn("original", gridMain6);
            AddSummaryColumn("as400", gridMain6);
            AddSummaryColumn("difference", gridMain6);
            AddSummaryColumn("ragCurrentMonth", gridMain6);
            AddSummaryColumn("deathRemYTDprevious", gridMain6);
            AddSummaryColumn("deathRemCurrMonth", gridMain6);
            AddSummaryColumn("refundRemYTDprevious", gridMain6);
            AddSummaryColumn("refundRemCurrMonth", gridMain6);
            AddSummaryColumn("interest", gridMain6);

            AddSummaryColumn("beginningBalance", gridMain7);
            AddSummaryColumn("ytdPrevious", gridMain7);
            AddSummaryColumn("paymentCurrMonth", gridMain7);
            AddSummaryColumn("currentRemovals", gridMain7);
            AddSummaryColumn("endingBalance", gridMain7);
            AddSummaryColumn("interest", gridMain7);

            AddSummaryColumn("beginningBalance", gridMain2);
            AddSummaryColumn("ytdPrevious", gridMain2);
            AddSummaryColumn("paymentCurrMonth", gridMain2);
            AddSummaryColumn("currentPayments", gridMain2);
            AddSummaryColumn("endingBalance", gridMain2);

            AddSummaryColumn("beginningBalance", gridMain8);
            //AddSummaryColumn("paymentCurrMonth", gridMain8);
            AddSummaryColumn("currentRefunds", gridMain8);
            AddSummaryColumn("currentDeathClaims", gridMain8);
            AddSummaryColumn("interest", gridMain8);
            AddSummaryColumn("currentInterest", gridMain8);
            AddSummaryColumn("endingBalance", gridMain8);

            AddSummaryColumn("endingBalance", gridMain9);
            AddSummaryColumn("calcTrust85", gridMain9);
            AddSummaryColumn("difference", gridMain9);

            AddSummaryColumn("paymentCurrMonth", gridMain10);
            AddSummaryColumn("cashRemitted", gridMain10);
            AddSummaryColumn("difference", gridMain10);

            //G1.SetupVisibleColumns(gridMain6, this.columnsToolStripMenuItem, nmenu_Click);
            loadLocatons();
        }
        /***********************************************************************************************/
        private void loadLocatons()
        {
            string cmd = "Select * from `funeralhomes` order by `keycode`;";
            DataTable locDt = G1.get_db_data(cmd);
            chkComboLocNames.Properties.DataSource = locDt;
            chkComboLocation.Properties.DataSource = locDt;
        }
        /****************************************************************************************/
        private void AddSummaryColumn(string columnName, DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gMain = null)
        {
            gMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            gMain.Columns[columnName].SummaryItem.DisplayFormat = "{0:N2}";
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
                gridMain6.Columns[index].Visible = false;
            }
            else
            {
                menu.Checked = true;
                gridMain6.Columns[index].Visible = true;
            }
            gridMain6.RefreshData();
            dgv6.Refresh();
            ToolStripMenuItem xmenu = this.columnsToolStripMenuItem;
            xmenu.ShowDropDown();
        }
        /***********************************************************************************************/
        private int getGridColumnIndex(string columnName)
        {
            int index = -1;
            for (int i = 0; i < gridMain6.Columns.Count; i++)
            {
                string name = gridMain6.Columns[i].Name;
                if (name == columnName)
                {
                    index = i;
                    break;
                }
            }
            return index;
        }
        /***********************************************************************************************/
        private void LoadLocations(DataTable lDt)
        {
            string contract = "";
            string trust = "";
            string loc = "";
            if (G1.get_column_number(lDt, "loc") < 0)
                lDt.Columns.Add("loc");
            if (G1.get_column_number(lDt, "Location Name") < 0)
                lDt.Columns.Add("Location Name");

            for (int i = 0; i < lDt.Rows.Count; i++)
            {
                contract = lDt.Rows[i]["contractNumber"].ObjToString();
                contract = Trust85.decodeContractNumber(contract, ref trust, ref loc);
                lDt.Rows[i]["loc"] = loc;
            }

            string cmd = "Select * from `funeralhomes` order by `keycode`;";
            DataTable dd = G1.get_db_data(cmd);
            string location = "";
            for (int i = 0; i < lDt.Rows.Count; i++)
            {
                location = lDt.Rows[i]["loc"].ObjToString();
                DataRow[] dr = dd.Select("keycode='" + location + "'");
                if (dr.Length > 0)
                    lDt.Rows[i]["Location Name"] = dr[0]["name"].ObjToString();
                else
                    lDt.Rows[i]["Location Name"] = location;
            }
        }
        /*******************************************************************************************/
        private string getLocationQuery()
        {
            string procLoc = "";
            string[] locIDs = this.chkComboLocation.EditValue.ToString().Split('|');
            string location = "";
            for (int i = 0; i < locIDs.Length; i++)
            {
                if (!String.IsNullOrWhiteSpace(locIDs[i]))
                {
                    if (procLoc.Trim().Length > 0)
                        procLoc += ",";
                    location = locIDs[i].Trim().ToUpper();
                    if (location.IndexOf("02") < 0)
                    {
                        if (location.IndexOf("WF") >= 0)
                            location = "WF";
                        location += "12345LI"; // Force any contract Number and Trust LI just to get the location
                        location = getProperLocation(location, chk2002.Checked);
                    }
                    procLoc += "'" + location + "'";
                }
            }
            return procLoc.Length > 0 ? " `locind` IN (" + procLoc + ") " : "";
            //            return procLoc.Length > 0 ? " `location` IN (" + procLoc + ") " : "";
        }
        /***********************************************************************************************/
        private void ClearCustomerTable()
        {
            if (trust85Dt == null)
                return;

            if (G1.get_column_number(trust85Dt, "DoneDone") < 0)
                trust85Dt.Columns.Add("DoneDone");
            for (int i = 0; i < trust85Dt.Rows.Count; i++)
                trust85Dt.Rows[i]["DoneDone"] = "";
        }
        /***********************************************************************************************/
        private DataTable PullAllData(bool allData = false)
        {
            string contract = this.txtContract.Text;
            string Y2002 = "";
            if (chk2002.Checked)
                Y2002 = "2002";

            string cmd = "Select * from `trust2013` a JOIN `customers` c ON a.`contractNumber` = c.`contractNumber` ";
            cmd += " JOIN `contracts` x ON a.`contractNumber` = x.`contractNumber` ";
            if (!allData)
            {
                if (chk2002.Checked)
                    cmd += " where `Is2002` = '2002' ";
                else
                    cmd += " where `Is2002` <> '2002' ";
                if (!String.IsNullOrWhiteSpace(contract))
                    cmd += " AND a.`contractNumber` = '" + contract + "' ";
            }

            if (SMFS.activeSystem.ToUpper() == "RILES")
                cmd += " AND a.`riles` = 'Y' ";
            else
                cmd += " AND a.`riles` <> 'Y' ";

            cmd += ";";

            DataTable dt = G1.get_db_data(cmd);
            if (!allData)
            {
                if (chkLiveOnly.Checked)
                    return dt;
            }

            cmd = "Select * from `trust2013` a ";
            if (!allData)
            {
                if (chk2002.Checked)
                    cmd += " where `Is2002` = '2002' ";
                else
                    cmd += " where `Is2002` <> '2002' ";

                if (!String.IsNullOrWhiteSpace(contract))
                    cmd += " AND a.`contractNumber` = '" + contract + "' ";
            }

            if (SMFS.activeSystem.ToUpper() == "RILES")
                cmd += " AND a.`riles` = 'Y' ";
            else
                cmd += " AND a.`riles` <> 'Y' ";

            cmd += ";";

            DataTable dx = G1.get_db_data(cmd); // This is because not all that is in trust2013 is in the SMFS database

            string contractNumber = "";
            DataRow[] dR = null;
            try
            {
                for (int i = 0; i < dx.Rows.Count; i++)
                {
                    contractNumber = dx.Rows[i]["contractNumber"].ObjToString();
                    dR = dt.Select("contractNumber = '" + contractNumber + "'");
                    if (dR.Length <= 0)
                        dt.ImportRow(dx.Rows[i]);
                }
            }
            catch (Exception ex)
            {

            }
            //CleanupYearEnd(dt);
            return dt;
        }
        /***********************************************************************************************/
        private void CleanupYearEnd(DataTable dt)
        {
            if (this.dateTimePicker2.Value.Month != 1)
                return;
            string str = "";
            double originalBeginningBalance = 0D;
            double beginningBalance = 0D;
            double endingBalance = 0D;
            double currentPayments = 0D;
            double currentRemovals = 0D;
            double interest = 0D;
            DateTime runDate = this.dateTimePicker2.Value;
            DateTime date = DateTime.Now;
            DateTime thisMonth = new DateTime(runDate.Year, runDate.Month, 1);
            int removeCount = 0;
            double totalRemovals = 0D;
            bool remove = false;
            string contractNumber = "";

            //DataTable rDt = dt.Clone();

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                if (contractNumber == "M23001LI")
                {
                }
                remove = false;
                beginningBalance = dt.Rows[i]["beginningBalance"].ObjToDouble();
                originalBeginningBalance = beginningBalance;
                endingBalance = dt.Rows[i]["endingBalance"].ObjToDouble();
                endingBalance = G1.RoundValue(endingBalance);
                currentPayments = dt.Rows[i]["currentPayments"].ObjToDouble();
                currentRemovals = dt.Rows[i]["deathRemYTDPrevious"].ObjToDouble();
                currentRemovals += dt.Rows[i]["refundRemYTDPrevious"].ObjToDouble();
                currentRemovals += dt.Rows[i]["deathRemCurrMonth"].ObjToDouble();
                currentRemovals += dt.Rows[i]["refundRemCurrMonth"].ObjToDouble();
                if (currentRemovals > 0D)
                {
                    totalRemovals += beginningBalance;
                    remove = true;
                    //G1.copy_dt_row(dt, i, rDt, rDt.Rows.Count);
                }
                interest = dt.Rows[i]["interest"].ObjToDouble();
                currentRemovals = currentRemovals - interest;
                beginningBalance = beginningBalance + currentPayments;
                beginningBalance = G1.RoundValue(beginningBalance);
                if (remove && endingBalance != 0D)
                {
                    beginningBalance = endingBalance;
                    remove = false;
                }
                str = dt.Rows[i]["trustRemoved"].ObjToString().Trim().ToUpper();
                if (str == "YES")
                {
                    date = dt.Rows[i]["dateRemoved"].ObjToDateTime();
                    if (date < thisMonth.AddMonths(-1))
                    {
                        if (currentRemovals != 0D)
                            beginningBalance = 0D;
                        if (endingBalance != 0D)
                        {
                            remove = false;
                            beginningBalance = endingBalance;
                        }
                    }
                }
                str = dt.Rows[i]["trustRefunded"].ObjToString().Trim().ToUpper();
                if (str == "YES")
                {
                    date = dt.Rows[i]["dateRemoved"].ObjToDateTime();
                    if (date < thisMonth.AddMonths(-1))
                    {
                        if (currentRemovals != 0D)
                            beginningBalance = 0D;
                        if (endingBalance != 0D)
                        {
                            remove = false;
                            beginningBalance = endingBalance;
                        }
                    }
                }
                if (remove)
                {
                    beginningBalance = 0D;
                    dt.Rows[i]["interest"] = 0D;
                }
                dt.Rows[i]["beginningBalance"] = beginningBalance;
                dt.Rows[i]["deathRemYTDPrevious"] = 0D;
                dt.Rows[i]["deathRemCurrMonth"] = 0D;
                dt.Rows[i]["refundRemYTDPrevious"] = 0D;
                dt.Rows[i]["refundRemCurrMonth"] = 0D;
                dt.Rows[i]["ytdPrevious"] = 0D;
                dt.Rows[i]["endingBalance"] = endingBalance;
            }
        }
        /***********************************************************************************************/
        private string FixSpecial(string data)
        {
            string apostraphe = "ÃƒÆ’Ã";
            //if (1 == 1)
            //    return data;
            int idx = data.IndexOf(apostraphe);
            if (idx > 0)
                data = G1.Truncate(data, idx);

            data = G1.try_protect_data(data);
            data = G1.Truncate(data, 50);

            return data;
        }
        /***********************************************************************************************/
        private DataTable PullTheData(bool PerformYearEnd = true)
        {
            this.Cursor = Cursors.WaitCursor;
            string contract = this.txtContract.Text;
            string Y2002 = "";
            if (chk2002.Checked)
                Y2002 = "2002";

            testNewDone = false;

            string cmd = "Select * from `trust2013r` a JOIN `customers` c ON a.`contractNumber` = c.`contractNumber` ";
            cmd += " JOIN `contracts` x ON a.`contractNumber` = x.`contractNumber` ";
            if (chk2002.Checked)
                cmd += " where `Is2002` = '2002' ";
            else
                cmd += " where `Is2002` <> '2002' ";
            if (!String.IsNullOrWhiteSpace(contract))
                cmd += " AND a.`contractNumber` = '" + contract + "' ";
            if (!String.IsNullOrWhiteSpace(chkComboLocation.Text))
                cmd += " AND a.`locind` = '" + chkComboLocation.Text + "' ";

            DateTime date = this.dateTimePicker2.Value;

            if (!oldData)
                date = date.AddMonths(-1);

            string date1 = date.Year.ToString("D4") + "-" + date.Month.ToString("D2") + "-01 00:00:00";
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            string date2 = date.Year.ToString("D4") + "-" + date.Month.ToString("D2") + "-" + days.ToString("D2") + " 23:59:59";

            cmd += " AND `payDate8` >= '" + date1 + "' AND `payDate8` <= '" + date2 + "' ";

            if (SMFS.activeSystem.ToUpper() == "RILES")
                cmd += " AND a.`riles` = 'Y' ";
            else
                cmd += " AND a.`riles` <> 'Y' ";

            cmd += ";";

            DataTable dt = G1.get_db_data(cmd);

            if (dt.Rows.Count <= 0)
            {
                string system = SMFS.activeSystem;
                MessageBox.Show("***WARNING*** It looks like the previous months data was not saved. Go back to the previous month and save the data for " + system + " System.");
                if (SMFS.activeSystem.ToUpper() == "RILES")
                {
                    SetupRilesContracts(dt);
                }
            }
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dt.Rows[i]["firstName"] = FixSpecial(dt.Rows[i]["firstName"].ObjToString());
                dt.Rows[i]["lastName"] = FixSpecial(dt.Rows[i]["lastName"].ObjToString());
                dt.Rows[i]["address2013"] = FixSpecial(dt.Rows[i]["address2013"].ObjToString());
                dt.Rows[i]["city2013"] = FixSpecial(dt.Rows[i]["city2013"].ObjToString());
                dt.Rows[i]["state2013"] = FixSpecial(dt.Rows[i]["state2013"].ObjToString());
                dt.Rows[i]["zip2013"] = FixSpecial(dt.Rows[i]["zip2013"].ObjToString());
                dt.Rows[i]["ssn2013"] = FixSpecial(dt.Rows[i]["ssn2013"].ObjToString());
            }


            //dt = SMFS.FilterForRiles( dt );

            if (chkLiveOnly.Checked)
                return dt;

            //            Trust85.FindContract(dt, "WM01041");

            cmd = "Select * from `trust2013r` a ";
            if (chk2002.Checked)
                cmd += " where `Is2002` = '2002' ";
            else
                cmd += " where `Is2002` <> '2002' ";

            if (!String.IsNullOrWhiteSpace(contract))
                cmd += " AND a.`contractNumber` = '" + contract + "' ";
            if (!String.IsNullOrWhiteSpace(chkComboLocation.Text))
                cmd += " AND a.`locind` = '" + chkComboLocation.Text + "' ";

            cmd += " AND `payDate8` >= '" + date1 + "' AND `payDate8` <= '" + date2 + "' ";

            if (SMFS.activeSystem.ToUpper() == "RILES")
                cmd += " AND a.`riles` = 'Y' ";
            else
                cmd += " AND a.`riles` <> 'Y' ";

            cmd += ";";
            //if (1 == 1)
            //    return dt;

            DataTable dx = G1.get_db_data(cmd); // This is because not all that is in trust2013 is in the SMFS database

            //dx = SMFS.FilterForRiles(dx);

            string contractNumber = "";
            DataRow[] dRows = null;
            DataRow dR = null;
            double removed = 0D;
            double refunded = 0D;

            try
            {
                this.Cursor = Cursors.WaitCursor;
                for (int i = 0; i < dx.Rows.Count; i++)
                {
                    Application.DoEvents();
                    contractNumber = dx.Rows[i]["contractNumber"].ObjToString();
                    dRows = dt.Select("contractNumber = '" + contractNumber + "'"); // Avoid Duplicate Records
                    if (dRows.Length <= 0)
                    {
                        dx.Rows[i]["firstName"] = FixSpecial(dx.Rows[i]["firstName"].ObjToString());
                        dx.Rows[i]["lastName"] = FixSpecial(dx.Rows[i]["lastName"].ObjToString());
                        dx.Rows[i]["address2013"] = FixSpecial(dx.Rows[i]["address2013"].ObjToString());
                        dx.Rows[i]["city2013"] = FixSpecial(dx.Rows[i]["city2013"].ObjToString());
                        dx.Rows[i]["state2013"] = FixSpecial(dx.Rows[i]["state2013"].ObjToString());
                        dx.Rows[i]["zip2013"] = FixSpecial(dx.Rows[i]["zip2013"].ObjToString());
                        dx.Rows[i]["ssn2013"] = FixSpecial(dx.Rows[i]["ssn2013"].ObjToString());

                        dR = dt.NewRow();
                        dR["contractNumber"] = contractNumber;
                        dR["firstName"] = dx.Rows[i]["firstName"].ObjToString();
                        dR["lastName"] = dx.Rows[i]["lastName"].ObjToString();
                        dR["address1"] = dx.Rows[i]["address2013"].ObjToString();
                        dR["address2"] = "";
                        dR["city"] = dx.Rows[i]["city2013"].ObjToString();
                        dR["state"] = dx.Rows[i]["state2013"].ObjToString();
                        dR["zip1"] = dx.Rows[i]["zip2013"].ObjToString();
                        dR["zip2"] = "";
                        dR["ssn"] = dx.Rows[i]["ssn2013"].ObjToString();
                        dR["payDate8"] = dx.Rows[i]["payDate8"];
                        dR["beginningBalance"] = dx.Rows[i]["beginningBalance"].ObjToDouble();
                        dR["paymentCurrMonth"] = dx.Rows[i]["paymentCurrMonth"].ObjToDouble();
                        dR["ytdPrevious"] = dx.Rows[i]["ytdPrevious"].ObjToDouble();
                        dR["currentPayments"] = dx.Rows[i]["currentPayments"].ObjToDouble();
                        dR["interest"] = dx.Rows[i]["interest"].ObjToDouble();
                        dR["locind"] = dx.Rows[i]["locind"].ObjToString();
                        dR["deathRemYTDPrevious"] = dx.Rows[i]["deathRemYTDPrevious"].ObjToDouble();
                        dR["deathRemCurrMonth"] = dx.Rows[i]["deathRemCurrMonth"].ObjToDouble();
                        dR["refundRemYTDPrevious"] = dx.Rows[i]["refundRemYTDPrevious"].ObjToDouble();
                        dR["refundRemCurrMonth"] = dx.Rows[i]["refundRemCurrMonth"].ObjToDouble();
                        dR["currentRemovals"] = dx.Rows[i]["currentRemovals"].ObjToDouble();
                        dR["endingBalance"] = dx.Rows[i]["endingBalance"].ObjToDouble();
                        dR["location"] = dx.Rows[i]["location"].ObjToString();

                        removed = dx.Rows[i]["deathRemYTDPrevious"].ObjToDouble() + dx.Rows[i]["deathRemCurrMonth"].ObjToDouble();
                        refunded = dx.Rows[i]["refundRemYTDPrevious"].ObjToDouble() + dx.Rows[i]["refundRemCurrMonth"].ObjToDouble();

                        if (removed > 0D)
                            dR["trustRemoved"] = "Y";
                        if (refunded > 0D)
                            dR["trustRefunded"] = "Y";
                        //dR["dateRemoved"] = G1.DTtoMySQLDT(dx.Rows[i]["dateRemoved"].ObjToDateTime());
                        dR["ServiceID"] = dx.Rows[i]["ServiceID"].ObjToString();
                        if (chk2002.Checked)
                            dR["Is2002"] = "2002";
                        dt.Rows.Add(dR);

                    }
                    //dt.ImportRow(dx.Rows[i]);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** " + ex.Message.ToString());
            }

            //Trust85.FindContract(dt, "FR20011LI");

            if (PerformYearEnd)
                CleanupYearEnd(dt);

            return dt;
        }
        /***********************************************************************************************/
        private void btnRun_Click(object sender, EventArgs e)
        {
            if (checkForChanged())
                return;

            panel1Top.Hide();
            DateTime date = dateTimePicker2.Value;

            DialogResult result = MessageBox.Show("Are you sure you want to RUN the Trust Report for " + date.ToString("MM/dd/yyyy") + "?", "Run Trust Report Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;
            workDate2 = date.AddDays(1);
            oldData = false;
            this.Cursor = Cursors.WaitCursor;
            ClearCustomerTable();
            RunClicked = true;
            //            customerDt = originalDt.Copy();

            date = dateTimePicker2.Value;
            string date2 = G1.DateTimeToSQLDateTime(date);

            DateTime priorYearDate = new DateTime((date.Year - 1), 12, 31);

            int year = date.Year - 1;
            string columnName = year.ToString() + " & Prior Bal PD";
            gridMain6.Columns["beginningBalance"].Caption = columnName;
            gridMain2.Columns["beginningBalance"].Caption = columnName;

            string Y2002 = "";
            if (chk2002.Checked)
                Y2002 = "2002";

            string cmd = "Select * from `trust2013r` a JOIN `customers` c ON a.`contractNumber` = c.`contractNumber` ";
            cmd += " JOIN `contracts` x ON a.`contractNumber` = x.`contractNumber` ";
            if (chk2002.Checked)
                cmd += " where `Is2002` = '2002' ";
            else
                cmd += " where `Is2002` <> '2002' ";

            lastSaveDate = this.dateTimePicker2.Value;
            lastSaveDate = new DateTime(lastSaveDate.Year, lastSaveDate.Month, 1);
            lastSaveDate = lastSaveDate.AddDays(-1);

            string lDate1 = lastSaveDate.ToString("yyyy-MM-dd");
            cmd += " AND `payDate8` <= '" + lDate1 + "' ";

            DataTable dt = G1.get_db_data(cmd + " ORDER BY `payDate8` DESC LIMIT 1;");

            //lastSaveDate = DateTime.Now;
            if (dt.Rows.Count > 0)
                lastSaveDate = dt.Rows[0]["payDate8"].ObjToDateTime();
            dt.Rows.Clear();
            dt.Dispose();
            dt = null;


            bool debug = false;
            string contract = this.txtContract.Text;
            //if (!String.IsNullOrWhiteSpace(contract))
            //{
            //    debug = true;
            //    cmd += " AND a.`contractNumber` = '" + contract + "' ";
            //}
            //cmd += ";";

            //if ( debug )
            //    dt = G1.get_db_data(cmd);
            //else

            dt = PullTheData(); //Some of the data in Trust2013 is not in customer file

            if (chk2002.Checked)
                dt = FindNewContracts(dt);
            else
            {
                if (!chk2002.Checked)
                    FindNewCemeteries(dt);
            }

            //Trust85.FindContract(dt, "C0214");
            //if (1 == 1)
            //{
            //    MessageBox.Show("DONE");
            //    //return;
            //}

            //Trust85.FindContract(dt, "WT023");

            //if (1 != 1)
            //{
            //    DataRow[] dRows = null;
            //    try
            //    {
            //        dRows = dt.Select("contractNumber='L17035UI'");
            //        if (dRows.Length <= 0)
            //            return;
            //        DataTable ddx = dt.Clone();
            //        G1.ConvertToTable(dRows, ddx);
            //        dt = ddx.Copy();
            //    }
            //    catch (Exception ex)
            //    {

            //    }
            //}


            int numberRows = dt.Rows.Count;

            DateTime currentMonthDate = date;
            DateTime priorMonthsDate = date;
            DateTime payDate8 = DateTime.Now;

            bool doPriorMonths = false;
            if (date.Month > 1)
            {
                doPriorMonths = true;
                priorMonthsDate = date.AddMonths(-1);
            }

            double priorYear = 0D;
            double ytdNow = 0D;
            double currentMonth = 0D;
            double currentRemoval = 0D;
            double currentPayments = 0D;
            double trust85P = 0D;
            double trust100P = 0D;
            double t85P = 0D;
            double t100P = 0D;
            double tt85P = 0D;

            DateTime issueDate = DateTime.Now;

            DataView tempview = dt.DefaultView;
            tempview.Sort = "location asc, contractNumber asc";
            dt = tempview.ToTable();

            string address = "";
            string str = "";
            string cnum = "";
            DataTable dx = null;
            int lastrow = dt.Rows.Count;

            //if ( LoginForm.administrator )
            //{
            //    if ( debug )
            //    {
            //        DataRow[] dRow = trust85Dt.Select("contractNumber='" + contract + "'");
            //        DataTable ddx = trust85Dt.Clone();
            //        for (int i = 0; i < dRow.Length; i++)
            //            ddx.ImportRow(dRow[i]);
            //        lastrow = ddx.Rows.Count;
            //        dt = ddx.Copy();
            //    }
            //}

            bool doHalf = false;
            DateTime halfDate = new DateTime(2006, 7, 1);
            bool deceased = false;
            DateTime deceasedDate = DateTime.Now;
            bool trustPaid = false;
            DateTime trustPaidDate = DateTime.Now;

            DateTime date3 = DateTime.Now;
            string startDate = "";

            if (G1.get_column_number(dt, "num") < 0)
                dt.Columns.Add("num");
            if (G1.get_column_number(dt, "fullname") < 0)
                dt.Columns.Add("fullname");
            if (G1.get_column_number(dt, "dd") < 0)
                dt.Columns.Add("dd");
            if (G1.get_column_number(dt, "found") < 0)
                dt.Columns.Add("found");

            string fname = "";
            string lname = "";
            string name = "";
            string dbr = "";
            double contractValue = 0D;
            double financeMonths = 0D;
            double rate = 0D;
            double downPayment = 0D;
            double principal = 0D;
            double payment = 0D;
            double credit = 0D;
            double debit = 0D;
            double interest = 0D;
            double downpayment = 0D;
            double lower = 0D;
            double higher = 0D;
            double as400Trust85 = 0D;
            double difference = 0D;

            if (G1.get_column_number(dt, "loc") < 0)
                dt.Columns.Add("loc");
            if (G1.get_column_number(dt, "Location Name") < 0)
                dt.Columns.Add("Location Name");
            if (G1.get_column_number(dt, "dbr") < 0)
                dt.Columns.Add("dbr", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "original") < 0)
                dt.Columns.Add("original", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "as400") < 0)
                dt.Columns.Add("as400", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "difference") < 0)
                dt.Columns.Add("difference", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "ragCurrentMonth") < 0)
                dt.Columns.Add("ragCurrentMonth", Type.GetType("System.Double"));

            //if ( !debug)
            //    AddNewContracts(dt);

            LoadLocations(dt);

            //DataTable ddx = dt.DefaultView.ToTable(true, "locind", "Location Name");

            int lastRow = dt.Rows.Count;
            //            lastRow = 0;
            lblTotal.Show();

            lblTotal.Text = "of " + lastRow.ToString();
            lblTotal.Refresh();

            barImport.Show();
            barImport.Minimum = 0;
            barImport.Maximum = lastRow;

            string found = "";

            //Trust85.FindContract(dt, "FF23070LI");

            //original_SMFS_dt = dt.Copy();

            try
            {

                RunSMFS(dt, lastSaveDate);
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** " + ex.Message.ToString());
            }

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                trust85P = dt.Rows[i]["paymentCurrMonth"].ObjToDouble();
                as400Trust85 = dt.Rows[i]["as400"].ObjToDouble();
                difference = trust85P - as400Trust85;
                difference = G1.RoundValue(difference);
                dt.Rows[i]["difference"] = difference;
            }

            FixStuff(dt);

            confirmLocations(dt);

            CleanupYearEndRemovals(dt);

            dt = SMFS.FilterForRiles(dt);

            FixSpecialLocations(dt);

            LoadPre2002(dt);

            SetupCemeteries(dt, gridMain6);

            originalDt = dt;
            G1.NumberDataTable(dt);
            dgv6.DataSource = dt;
            trustReportDt = dt;


            LoadUpLocations();

            if (chkExpand.Checked)
            {
                gridMain6.OptionsBehavior.AutoExpandAllGroups = true;
                gridMain6.ExpandAllGroups();
            }
            else
            {
                gridMain6.OptionsBehavior.AutoExpandAllGroups = false;
                gridMain6.CollapseAllGroups();
            }
            //            gridMain6.Columns["Location Name"].Visible = true;
            gridMain6.Columns["loc"].Visible = true;

            if (chk2002.Checked)
            {
                gridMain6.Columns["interest"].Visible = true;
                gridMain7.Columns["interest"].Visible = false;
                gridMain8.Columns["interest"].Visible = false;
                gridMain8.Columns["currentInterest"].Visible = false;
            }
            else
            {
                gridMain6.Columns["interest"].Visible = true;
                gridMain7.Columns["interest"].Visible = false;
                gridMain8.Columns["interest"].Visible = true;
                gridMain8.Columns["currentInterest"].Visible = true;
            }

            if (String.IsNullOrWhiteSpace(txtContract.Text))
                panel1Top.Show();
            this.Cursor = Cursors.Default;
            //if (chk2002.Checked)
            //    btnVerify.Show();
        }
        /**************************************************************************************/
        private void FixSpecialLocations(DataTable dt)
        {
            if (!chk2002.Checked)
                return;
            string locind = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                locind = dt.Rows[i]["locind"].ObjToString();
                if (locind == "FO")
                    dt.Rows[i]["locind"] = "BRF02";
                else if (locind == "WC")
                    dt.Rows[i]["locind"] = "WF02";
            }
        }
        /****************************************************************************************/
        private void CleanupYearEndRemovals(DataTable dt)
        {
            this.Cursor = Cursors.WaitCursor;
            DateTime runDate = this.dateTimePicker2.Value;
            if (runDate.Month > 1)
                return;

            DateTime startDate = new DateTime(runDate.Year, runDate.Month, 1);
            string contractNumber = "";
            double beginningBalance = 0D;
            double endingBalance = 0D;
            double paymentCurrMonth = 0D;
            double death = 0D;
            double dValue = 0D;
            double interest = 0D;
            DateTime issueDate = DateTime.Now;
            bool gotIssueDate = false;
            if (G1.get_column_number(dt, "issueDate") >= 0)
                gotIssueDate = true;
            for (int i = dt.Rows.Count - 1; i >= 0; i--)
            {
                death = 0D;
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                if (contractNumber == "C17007U")
                {
                }
                if ( gotIssueDate )
                    issueDate = dt.Rows[i]["issueDate"].ObjToDateTime();
                endingBalance = dt.Rows[i]["endingBalance"].ObjToDouble();
                beginningBalance = dt.Rows[i]["beginningBalance"].ObjToDouble();
                paymentCurrMonth = dt.Rows[i]["paymentCurrMonth"].ObjToDouble();

                dValue = dt.Rows[i]["deathRemCurrMonth"].ObjToDouble();
                if (dValue > 0D)
                    death = dValue;

                dValue = dt.Rows[i]["refundRemCurrMonth"].ObjToDouble();
                if (dValue > 0D)
                    death = dValue;

                if (beginningBalance == 0D && endingBalance == 0D && paymentCurrMonth == 0D && death == 0D)
                {
                    if (gotIssueDate)
                    {
                        if (issueDate >= startDate && issueDate <= runDate)
                            continue;
                    }
                    if (!chk2002.Checked)
                    {
                        interest = dt.Rows[i]["interest"].ObjToDouble();
                        if (interest <= 0D)
                            dt.Rows.RemoveAt(i);
                    }
                    else
                        dt.Rows.RemoveAt(i);
                }
            }
        }
        /****************************************************************************************/
        private DataTable FindNewContracts(DataTable dt)
        {
            int lastRow = 0;
            double downPayment = 0D;

            DateTime date = this.dateTimePicker2.Value;
            string date2 = G1.DateTimeToSQLDateTime(date);

            date = new DateTime(date.Year, date.Month, 1);
            string date1 = G1.DateTimeToSQLDateTime(date);

            string cmd = "Select * from `contracts` p ";
            cmd += " JOIN `customers` c ON p.`contractNumber` = c.`contractNumber` ";
            cmd += " where p.`issueDate8` >= '" + date1 + "' ";
            cmd += " and   p.`issueDate8` <= '" + date2 + "' ";
            //cmd += " and p.`downPayment` > '0.00' ";
            cmd += " ORDER by p.`issueDate8` ";
            cmd += ";";
            DataTable dx = G1.get_db_data(cmd);
            dx = SMFS.FilterForRiles(dx);
            string contract = "";
            DataRow dR = null;
            DateTime deceasedDate = DateTime.Now;
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                contract = dx.Rows[i]["contractNumber"].ObjToString();
                if (contract == "P24002L")
                {
                }
                DataRow[] dRows = dt.Select("contractNumber='" + contract + "'");
                if (dRows.Length <= 0)
                {
                    dR = dt.NewRow();
                    dR["contractNumber"] = contract;
                    dR["firstName"] = dx.Rows[i]["firstName"].ObjToString();
                    dR["lastName"] = dx.Rows[i]["lastName"].ObjToString();
                    dR["address1"] = dx.Rows[i]["address1"].ObjToString();
                    dR["address2"] = dx.Rows[i]["address2"].ObjToString();
                    dR["city"] = dx.Rows[i]["city"].ObjToString();
                    dR["state"] = dx.Rows[i]["state"].ObjToString();
                    dR["zip1"] = dx.Rows[i]["zip1"].ObjToString();
                    dR["zip2"] = dx.Rows[i]["zip2"].ObjToString();
                    dR["ssn"] = dx.Rows[i]["ssn"].ObjToString();
                    dR["payDate8"] = dx.Rows[i]["issueDate8"];
                    dR["apr"] = dx.Rows[i]["apr"].ObjToDouble();
                    downPayment = dx.Rows[i]["downPayment"].ObjToDouble();
                    //dR["paymentCurrMonth"] = downPayment; // This was commented out for some reason
                    dR["paymentCurrMonth"] = 0D; // Commented this out for some reason
                    dR["currentPayments"] = downPayment;
                    dR["interest"] = 0D;
                    dR["locind"] = getProperLocation(contract, chk2002.Checked);
                    dR["trustRemoved"] = dx.Rows[i]["trustRemoved"].ObjToString();
                    dR["trustRefunded"] = dx.Rows[i]["trustRefunded"].ObjToString();
                    dR["dateRemoved"] = G1.DTtoMySQLDT(dx.Rows[i]["dateRemoved"].ObjToDateTime());
                    dR["ServiceID"] = dx.Rows[i]["ServiceID"].ObjToString();
                    dR["Is2002"] = "2002";
                    deceasedDate = dx.Rows[i]["deceasedDate"].ObjToDateTime();
                    if (deceasedDate.Year > 100)
                        dR["deceasedDate"] = G1.DTtoMySQLDT(deceasedDate);
                    deceasedDate = dx.Rows[i]["issueDate8"].ObjToDateTime();
                    if (deceasedDate.Year > 100)
                        dR["issueDate8"] = G1.DTtoMySQLDT(deceasedDate);
                    dt.Rows.Add(dR);
                }
            }
            LoadTrustAdjustments(dt, date1, date2);

            contract = this.txtContract.Text;
            if (!String.IsNullOrWhiteSpace(contract))
            {
                DataRow[] dRows = dt.Select("contractNumber='" + contract + "'");
                DataTable ddx = dt.Clone();
                G1.ConvertToTable(dRows, ddx);
                dt.Rows.Clear();
                dt = ddx.Copy();
                dt.AcceptChanges();
            }
            dt = SMFS.FilterForRiles(dt);
            return dt;
        }
        /****************************************************************************************/
        private void FindNewCemeteries(DataTable dt)
        {
            double downPayment = 0D;

            DateTime date = this.dateTimePicker2.Value;
            string date2 = G1.DateTimeToSQLDateTime(date);

            date = new DateTime(date.Year, date.Month, 1);
            string date1 = G1.DateTimeToSQLDateTime(date);

            string cmd = "Select * from payments where `edited` = 'Cemetery' AND `payDate8` >= '" + date1 + "' ";
            cmd += " and   `payDate8` <= '" + date2 + "' ";
            cmd += ";";

            try
            {

                DataTable ddt = G1.get_db_data(cmd);

                string contract = "";
                DataRow dR = null;
                for (int i = 0; i < ddt.Rows.Count; i++)
                {
                    contract = ddt.Rows[i]["contractNumber"].ObjToString();
                    cmd = "Select * from `contracts` p ";
                    cmd += " JOIN `customers` c ON p.`contractNumber` = c.`contractNumber` ";
                    cmd += " where p.`contractNumber` = '" + contract + "' ";
                    cmd += ";";
                    DataTable dx = G1.get_db_data(cmd);
                    DataRow[] dRows = dt.Select("contractNumber='" + contract + "'");
                    if (dRows.Length <= 0)
                    {
                        dR = dt.NewRow();
                        dR["contractNumber"] = contract;
                        dR["firstName"] = dx.Rows[0]["firstName"].ObjToString();
                        dR["lastName"] = dx.Rows[0]["lastName"].ObjToString();
                        dR["address1"] = dx.Rows[0]["address1"].ObjToString();
                        dR["address2"] = dx.Rows[0]["address2"].ObjToString();
                        dR["city"] = dx.Rows[0]["city"].ObjToString();
                        dR["state"] = dx.Rows[0]["state"].ObjToString();
                        dR["zip1"] = dx.Rows[0]["zip1"].ObjToString();
                        dR["zip2"] = dx.Rows[0]["zip2"].ObjToString();
                        dR["ssn"] = dx.Rows[0]["ssn"].ObjToString();
                        dR["payDate8"] = dx.Rows[0]["issueDate8"];
                        downPayment = dx.Rows[0]["downPayment"].ObjToDouble();
                        //dR["paymentCurrMonth"] = downPayment;
                        dR["paymentCurrMonth"] = 0D;
                        dR["currentPayments"] = downPayment;
                        dR["interest"] = 0D;
                        dR["locind"] = getProperLocation(contract, chk2002.Checked);
                        dR["trustRemoved"] = dx.Rows[0]["trustRemoved"].ObjToString();
                        dR["trustRefunded"] = dx.Rows[0]["trustRefunded"].ObjToString();
                        dR["dateRemoved"] = G1.DTtoMySQLDT(dx.Rows[0]["dateRemoved"].ObjToDateTime());
                        dR["ServiceID"] = dx.Rows[0]["ServiceID"].ObjToString();
                        dR["Is2002"] = "";
                        dt.Rows.Add(dR);
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
        /****************************************************************************************/
        private void LoadTrustAdjustments(DataTable dt, string date1, string date2)
        {
            double trust85 = 0D;

            DateTime newStop = date2.ObjToDateTime();
            date2 = newStop.ToString("yyyy-MM-dd") + " 23:59:59";

            //            string cmd = "Select * from `payments` p JOIN `contracts` c ON p.`contractNumber` = c.`contractNumber` JOIN `customers` s ON p.`contractNumber` = s.`contractNumber` WHERE p.`tmstamp` >='" + date1 + "' AND p.`tmstamp` <= '" + date2 + "' AND p.`edited` = 'TRUSTADJ';";
            string cmd = "Select * from `payments` p JOIN `contracts` c ON p.`contractNumber` = c.`contractNumber` JOIN `customers` s ON p.`contractNumber` = s.`contractNumber` WHERE p.`payDate8` >='" + date1 + "' AND p.`payDate8` <= '" + date2 + "' AND p.`edited` = 'TRUSTADJ';";
            DataTable dx = G1.get_db_data(cmd);
            string contract = "";
            DataRow dR = null;
            string record1 = "";
            string record2 = "";
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                contract = dx.Rows[i]["contractNumber"].ObjToString();
                if (contract == "B17059UI")
                {
                }
                record1 = dx.Rows[i]["record"].ObjToString();
                DataRow[] dRows = dt.Select("contractNumber='" + contract + "'");
                if (dRows.Length <= 0)
                { // Only add if contract is not included because once added, the runSMFS will do the rest
                    dR = dt.NewRow();
                    dR["contractNumber"] = contract;
                    dR["firstName"] = dx.Rows[i]["firstName"].ObjToString();
                    dR["lastName"] = dx.Rows[i]["lastName"].ObjToString();
                    dR["address1"] = dx.Rows[i]["address1"].ObjToString();
                    dR["address2"] = dx.Rows[i]["address2"].ObjToString();
                    dR["city"] = dx.Rows[i]["city"].ObjToString();
                    dR["state"] = dx.Rows[i]["state"].ObjToString();
                    dR["zip1"] = dx.Rows[i]["zip1"].ObjToString();
                    dR["zip2"] = dx.Rows[i]["zip2"].ObjToString();
                    dR["ssn"] = dx.Rows[i]["ssn"].ObjToString();
                    dR["payDate8"] = dx.Rows[i]["issueDate8"];
                    trust85 = dx.Rows[i]["trust85P"].ObjToDouble();
                    dR["paymentCurrMonth"] = 0D;
                    dR["currentPayments"] = 0D;
                    dR["interest"] = 0D;
                    dR["locind"] = getProperLocation(contract, chk2002.Checked);
                    dR["trustRemoved"] = dx.Rows[i]["trustRemoved"].ObjToString();
                    dR["trustRefunded"] = dx.Rows[i]["trustRefunded"].ObjToString();
                    dR["dateRemoved"] = G1.DTtoMySQLDT(dx.Rows[i]["dateRemoved"].ObjToDateTime());
                    dR["ServiceID"] = dx.Rows[i]["ServiceID"].ObjToString();
                    dR["Is2002"] = "2002";
                    dt.Rows.Add(dR);
                }
            }
        }
        /***********************************************************************************************/
        public static string getProperLocation(string contract, bool chk2002Checked)
        {
            string locind = "";
            string trust = "";
            string loc = "";
            contract = Trust85.decodeContractNumber(contract, ref trust, ref loc);
            if (loc == "L" || loc == "C")
                locind = "JCC02";
            else if (loc == "WM")
                locind = "WM02";
            else if (loc == "WF")
                locind = "WF02";
            else if (loc == "T")
                locind = "TY02";
            else if (loc == "HU")
                locind = "HU02";
            else if (loc == "HT")
                locind = "HT02";
            else if (loc == "FF")
                locind = "FF02";
            else if (loc == "E")
                locind = "E02";
            else if (loc == "CT")
                locind = "CT02";
            else if (loc == "M")
                locind = "CC02";
            else if (loc == "B" || loc == "N" || loc == "MC" || loc == "F")
                locind = "BRF02";
            else if (loc == "P")
                locind = "BH02";
            else if (loc == "FR")
                locind = "BH02";
            //if (!chk2002.Checked)
            //    locind = locind.Replace("02", "");
            if (!chk2002Checked)
                locind = locind.Replace("02", "");
            if (String.IsNullOrWhiteSpace(locind))
                locind = loc;
            return locind;
        }
        /***********************************************************************************************/
        private void VerifyRilesContracts(DataTable dt)
        {
            if (SMFS.activeSystem.ToUpper() != "RILES")
                return;
            string cmd = "";
            DateTime runDate = this.dateTimePicker2.Value;
            int days = DateTime.DaysInMonth(runDate.Year, runDate.Month);
            runDate = new DateTime(runDate.Year, runDate.Month, days);
            DateTime January = new DateTime(runDate.Year, 1, 1);
            DateTime ytdDate = runDate.AddMonths(-1);
            days = DateTime.DaysInMonth(ytdDate.Year, ytdDate.Month);
            ytdDate = new DateTime(ytdDate.Year, ytdDate.Month, days);
            DateTime thisMonth = new DateTime(runDate.Year, runDate.Month, 1);
            days = DateTime.DaysInMonth(thisMonth.Year, thisMonth.Month);
            DateTime endMonth = new DateTime(thisMonth.Year, thisMonth.Month, days);

            string startDate = thisMonth.ToString("yyyy-MM-dd");
            string endDate = endMonth.ToString("yyyy-MM-dd");
            string contractNumber = "";

            DataRow[] dRows = null;
            DataRow dR = null;
            double downPayment = 0D;
            double trustPercent = 0D;
            double beginningBalance = 0D;

            cmd = "Select * from `payments` p JOIN `customers` c ON p.`contractNumber` = c.`contractNumber` ";
            cmd += " JOIN `contracts` x ON p.`contractNumber` = x.`contractNumber` ";
            cmd += "where p.`contractNumber` LIKE 'RF%' AND `payDate8` >= '" + startDate + "' AND `payDate8` <= '" + endDate + "' GROUP BY p.`contractNumber` ";

            DataTable dx = G1.get_db_data(cmd);

            for (int i = 0; i < dx.Rows.Count; i++)
            {
                contractNumber = dx.Rows[i]["contractNumber"].ObjToString();
                dRows = dt.Select("contractNumber = '" + contractNumber + "'"); // Avoid Duplicate Records
                if (dRows.Length <= 0)
                {
                    dR = dt.NewRow();
                    dR["contractNumber"] = contractNumber;
                    dR["firstName"] = dx.Rows[i]["firstName"].ObjToString();
                    dR["lastName"] = dx.Rows[i]["lastName"].ObjToString();
                    dR["address2013"] = dx.Rows[i]["address1"].ObjToString() + " " + dx.Rows[i]["address2"].ObjToString();
                    dR["address2013"] = dR["address2013"].ObjToString().Trim();
                    dR["city2013"] = dx.Rows[i]["city"].ObjToString();
                    dR["state2013"] = dx.Rows[i]["state"].ObjToString();
                    dR["zip2013"] = dx.Rows[i]["zip1"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(dx.Rows[i]["zip2"].ObjToString()))
                        dR["zip2013"] += "-" + dx.Rows[i]["zip2"].ObjToString();
                    dR["ssn2013"] = dx.Rows[i]["ssn"].ObjToString();
                    dR["payDate8"] = dx.Rows[i]["issueDate8"];
                    downPayment = dx.Rows[i]["downPayment"].ObjToDouble();
                    //dR["paymentCurrMonth"] = downPayment;
                    dR["beginningBalance"] = dx.Rows[i]["beginningBalance"].ObjToDouble();
                    dR["beginningBalance"] = GetRilesBB(contractNumber);
                    dR["paymentCurrMonth"] = 0D;
                    dR["currentPayments"] = 0D;
                    dR["interest"] = 0D;
                    dR["locind"] = getProperLocation(contractNumber, chk2002.Checked);
                    dR["trustRemoved"] = dx.Rows[i]["trustRemoved"].ObjToString();
                    dR["trustRefunded"] = dx.Rows[i]["trustRefunded"].ObjToString();
                    dR["dateRemoved"] = G1.DTtoMySQLDT(dx.Rows[i]["dateRemoved"].ObjToDateTime());
                    dR["ServiceID"] = dx.Rows[i]["ServiceID"].ObjToString();
                    dR["Is2002"] = "2002";
                    dR["riles"] = "Y";
                    dt.Rows.Add(dR);
                }
            }
        }
        /***********************************************************************************************/
        private void SetupRilesContracts(DataTable dt)
        {
            if (SMFS.activeSystem.ToUpper() != "RILES")
                return;
            string cmd = "";
            DateTime runDate = this.dateTimePicker2.Value;
            int days = DateTime.DaysInMonth(runDate.Year, runDate.Month);
            runDate = new DateTime(runDate.Year, runDate.Month, days);
            DateTime January = new DateTime(runDate.Year, 1, 1);
            DateTime ytdDate = runDate.AddMonths(-1);
            days = DateTime.DaysInMonth(ytdDate.Year, ytdDate.Month);
            ytdDate = new DateTime(ytdDate.Year, ytdDate.Month, days);
            DateTime thisMonth = new DateTime(runDate.Year, runDate.Month, 1);
            days = DateTime.DaysInMonth(thisMonth.Year, thisMonth.Month);
            DateTime endMonth = new DateTime(thisMonth.Year, thisMonth.Month, days);

            string startDate = thisMonth.ToString("yyyy-MM-dd");
            string endDate = endMonth.ToString("yyyy-MM-dd");
            string contractNumber = "";

            DataRow[] dRows = null;
            DataRow dR = null;
            double downPayment = 0D;
            double trustPercent = 0D;
            double beginningBalance = 0D;

            cmd = "Select * from `payments` p JOIN `customers` c ON p.`contractNumber` = c.`contractNumber` ";
            cmd += " JOIN `contracts` x ON p.`contractNumber` = x.`contractNumber` ";
            cmd += "where p.`contractNumber` LIKE 'RF%' AND `payDate8` <= '" + endDate + "' GROUP BY p.`contractNumber` ";

            DataTable dx = G1.get_db_data(cmd);

            for (int i = 0; i < dx.Rows.Count; i++)
            {
                contractNumber = dx.Rows[i]["contractNumber"].ObjToString();
                dRows = dt.Select("contractNumber = '" + contractNumber + "'"); // Avoid Duplicate Records
                if (dRows.Length <= 0)
                {
                    dR = dt.NewRow();
                    dR["contractNumber"] = contractNumber;
                    dR["firstName"] = dx.Rows[i]["firstName"].ObjToString();
                    dR["lastName"] = dx.Rows[i]["lastName"].ObjToString();
                    dR["address2013"] = dx.Rows[i]["address1"].ObjToString() + " " + dx.Rows[i]["address2"].ObjToString();
                    dR["address2013"] = dR["address2013"].ObjToString().Trim();
                    dR["city2013"] = dx.Rows[i]["city"].ObjToString();
                    dR["state2013"] = dx.Rows[i]["state"].ObjToString();
                    dR["zip2013"] = dx.Rows[i]["zip1"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(dx.Rows[i]["zip2"].ObjToString()))
                        dR["zip2013"] += "-" + dx.Rows[i]["zip2"].ObjToString();
                    dR["ssn2013"] = dx.Rows[i]["ssn"].ObjToString();
                    dR["payDate8"] = dx.Rows[i]["issueDate8"];
                    downPayment = dx.Rows[i]["downPayment"].ObjToDouble();
                    //dR["paymentCurrMonth"] = downPayment;
                    dR["beginningBalance"] = dx.Rows[i]["beginningBalance"].ObjToDouble();
                    dR["beginningBalance"] = GetRilesBB(contractNumber);
                    dR["paymentCurrMonth"] = 0D;
                    dR["currentPayments"] = 0D;
                    dR["interest"] = 0D;
                    dR["locind"] = getProperLocation(contractNumber, chk2002.Checked);
                    dR["trustRemoved"] = dx.Rows[i]["trustRemoved"].ObjToString();
                    dR["trustRefunded"] = dx.Rows[i]["trustRefunded"].ObjToString();
                    dR["dateRemoved"] = G1.DTtoMySQLDT(dx.Rows[i]["dateRemoved"].ObjToDateTime());
                    dR["ServiceID"] = dx.Rows[i]["ServiceID"].ObjToString();
                    dR["Is2002"] = "2002";
                    dR["riles"] = "Y";
                    dt.Rows.Add(dR);
                }
            }
        }
        /***********************************************************************************************/
        private double GetRilesBB(string contractNumber)
        {
            double bb = 0D;
            DateTime runDate = this.dateTimePicker2.Value;
            DateTime date = DateTime.Now;
            DateTime thisMonth = new DateTime(runDate.Year, runDate.Month, 1);
            double payment = 0D;
            double downPayment = 0D;
            double trustPercent = 1.0D;

            string cmd = "Select * from `payments` p JOIN `contracts` c ON p.`contractNumber` = c.`contractNumber` where p.`contractNumber` = '" + contractNumber + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                trustPercent = dt.Rows[0]["trustPercent"].ObjToDouble();
                if (trustPercent > 0D)
                    trustPercent = trustPercent / 100D;
            }
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                date = dt.Rows[i]["payDate8"].ObjToDateTime();
                if (date < thisMonth)
                {
                    //payment = dt.Rows[i]["paymentAmount"].ObjToDouble() * 0.85D;
                    //downPayment = dt.Rows[i]["downPayment"].ObjToDouble() * 0.85D;
                    payment = dt.Rows[i]["paymentAmount"].ObjToDouble() * trustPercent;
                    downPayment = dt.Rows[i]["downPayment"].ObjToDouble() * trustPercent;
                    bb += payment + downPayment;
                }
            }
            if (bb <= 0D)
            {
            }
            return bb;
        }
        /***********************************************************************************************/
        private void RunSMFS(DataTable dt, DateTime lastSaveDate)
        {
            VerifyRilesContracts(dt);

            if (dt.Rows.Count <= 0)
                return;

            //MessageBox.Show("Start RunSMFS");
            showLabel("1");
            GC.Collect();
            string cmd = "";
            DateTime runDate = this.dateTimePicker2.Value;
            int days = DateTime.DaysInMonth(runDate.Year, runDate.Month);
            runDate = new DateTime(runDate.Year, runDate.Month, days, 23, 59, 59);
            DateTime January = new DateTime(runDate.Year, 1, 1);
            DateTime ytdDate = runDate.AddMonths(-1);
            days = DateTime.DaysInMonth(ytdDate.Year, ytdDate.Month);
            ytdDate = new DateTime(ytdDate.Year, ytdDate.Month, days);
            DateTime thisMonth = new DateTime(runDate.Year, runDate.Month, 1);
            //thisMonth = thisMonth.AddDays(-1);

            bool doHalf = false;
            DateTime halfDate = new DateTime(2006, 7, 1);


            if (G1.get_column_number(dt, "trust85P") < 0)
                dt.Columns.Add("trust85P", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "contractValue") < 0)
                dt.Columns.Add("contractValue", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "apr") < 0)
                dt.Columns.Add("apr", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "calcTrust85") < 0)
                dt.Columns.Add("calcTrust85", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "difference") < 0)
                dt.Columns.Add("difference", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "issueDate") < 0)
                dt.Columns.Add("issueDate");
            if (G1.get_column_number(dt, "Pmts") < 0)
                dt.Columns.Add("Pmts");
            if (G1.get_column_number(dt, "DD") < 0)
                dt.Columns.Add("DD");
            if (G1.get_column_number(dt, "method") < 0)
                dt.Columns.Add("method");
            if (G1.get_column_number(dt, "currentDeathClaims") < 0)
                dt.Columns.Add("currentDeathClaims", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "currentRefunds") < 0)
                dt.Columns.Add("currentRefunds", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "currentInterest") < 0)
                dt.Columns.Add("currentInterest", Type.GetType("System.Double"));


            double trust85P = 0D;
            double trust100P = 0D;

            double ytdPrevious = 0D;
            double paymentCurrMonth = 0D;
            double currentPayments = 0D;
            double currentRemovals = 0D;
            string contractNumber = "";

            double startBalance = 0D;
            double beginningBalance = 0D;
            double deathRemYTDPrevious = 0D;
            double deathRemCurrMonth = 0D;
            double refundRemYTDPrevious = 0D;
            double refundRemCurrMonth = 0D;
            double interest = 0D;
            double endingBalance = 0D;
            double removals = 0D;
            double value = 0D;
            double difference = 0D;
            double trust85 = 0D;
            double trust100 = 0D;
            double oldTrust85 = 0D;
            double contractValue = 0D;
            double rate = 0D;

            int method = 0;
            string str = "";
            bool trustPaid = false;
            bool trustRefunded = false;
            DateTime dateRemoved = DateTime.Now;

            DataTable dx = null;
            DataTable dp = null;
            DateTime dueDate8 = DateTime.Now;
            DateTime deceasedDate = DateTime.Now;
            DateTime lastDate = DateTime.Now;
            DateTime payDate8 = DateTime.Now;
            int month = 0;
            int day = 0;
            int year = 0;
            string strRemoval = "";
            string record = "";
            string myFields = "";
            double dValue = 0D;
            double debit = 0D;
            double credit = 0D;
            double amtOfMonthlyPayt = 0;
            double financeMonths = 0D;
            double retained = 0D;
            double originalDownPayment = 0D;
            double downPayment = 0D;
            double amtPaid = 0D;
            double principal = 0D;
            double lastEndingBalance = 0D;

            double paymentAmount = 0D;
            double ccFee = 0D;

            int dbr = 0;
            //            int days = 0;

            DateTime date = DateTime.Now;

            //dx = dt.Clone();

            int lastRow = dt.Rows.Count;
            DateTime trustDate8 = DateTime.Now;
            string edited = "";
            string finale = "";
            bool honorFinale = false;
            int finaleCount = 0;
            //G1.CreateAudit("TrustReport");
            int auditCount = 0;
            bool doNewYear = false;
            if (this.dateTimePicker2.Value.Month == 1)
                doNewYear = true;

            bool doit = false;

            string force = "";
            string lockTrust85 = "";
            DataRow dR = null;
            DateTime dateDpPaid = DateTime.Now;
            DateTime iDate = DateTime.Now;
            DateTime issueDate8 = DateTime.Now;
            bool gotIssue = false;
            bool gotDBR = false;

            lastRow = dt.Rows.Count;
            //            lastRow = 0;
            lblTotal.Show();

            lblTotal.Text = "of " + lastRow.ToString();
            lblTotal.Refresh();

            barImport.Show();
            barImport.Minimum = 0;
            barImport.Maximum = lastRow;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                Application.DoEvents();
                doit = false;
                auditCount++;
                //G1.WriteAudit("Row=" + i.ToString());
                //showLabel("i " + i.ToString());
                //MessageBox.Show("Start RunSMFS PROCESSING");
                lblTotal.Text = (i + 1).ToString() + " of " + lastRow.ToString();
                lblTotal.Refresh();
                //if (1 == 1)
                //    continue;

                barImport.Value = i + 1;
                barImport.Refresh();

                trustPaid = false;
                trustRefunded = false;
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                lockTrust85 = dt.Rows[i]["lockTrust85"].ObjToString();

                deceasedDate = dt.Rows[i]["deceasedDate"].ObjToDateTime();

                issueDate8 = dt.Rows[i]["issueDate8"].ObjToDateTime();
                gotIssue = false;
                if (issueDate8 >= thisMonth && issueDate8 <= runDate)
                {
                    if (deceasedDate < thisMonth && deceasedDate.Year > 1000)
                        gotIssue = true;
                    else if ( deceasedDate.Year > 1000 )
                    {
                        payDate8 = dt.Rows[i]["payDate8"].ObjToDateTime();
                        //if (payDate8 >= thisMonth && payDate8 <= runDate)
                        //    gotIssue = true;
                    }
                }
                gotDBR = false;

                ytdPrevious = 0D;
                try
                {
                    //G1.sleep(50);
                    //GC.Collect();
                    //if (contractNumber != "WF15012UI")
                    //    continue;
                    //if (contractNumber != "M18040LI")
                    //    continue;
                    if (contractNumber == "WF23008L")
                    {
                    }
                    if (contractNumber == "M23001LI")
                    {
                    }

                    //dx.Clear();
                    //dx.ImportRow(dt.Rows[i]);

                    showLabel("After dx.Import");

                    trustDate8 = dt.Rows[i]["payDate8"].ObjToDateTime();
                    //if (doNewYear)
                    //{
                    //    beginningBalance = dt.Rows[i]["beginningBalance"].ObjToDouble();
                    //    currentPayments = dt.Rows[i]["currentPayments"].ObjToDouble();
                    //    currentRemovals = dt.Rows[i]["deathRemYTDPrevious"].ObjToDouble();
                    //    currentRemovals += dt.Rows[i]["refundRemYTDPrevious"].ObjToDouble();
                    //    currentRemovals += dt.Rows[i]["deathRemCurrMonth"].ObjToDouble();
                    //    currentRemovals += dt.Rows[i]["refundRemCurrMonth"].ObjToDouble();
                    //    beginningBalance = beginningBalance + currentPayments - currentRemovals;
                    //    //beginningBalance = dt.Rows[i]["endingBalance"].ObjToDouble();
                    //    str = dt.Rows[i]["trustRemoved"].ObjToString().Trim().ToUpper();
                    //    if (str == "YES")
                    //    {
                    //        date = dt.Rows[i]["dateRemoved"].ObjToDateTime();
                    //        if ( date < thisMonth.AddMonths ( -1) )
                    //            beginningBalance = 0D;
                    //    }
                    //    str = dt.Rows[i]["trustRefunded"].ObjToString().Trim().ToUpper();
                    //    if (str == "YES")
                    //    {
                    //        date = dt.Rows[i]["dateRemoved"].ObjToDateTime();
                    //        if (date < thisMonth.AddMonths(-1))
                    //            beginningBalance = 0D;
                    //    }
                    //    dt.Rows[i]["beginningBalance"] = beginningBalance;
                    //    dt.Rows[i]["deathRemYTDPrevious"] = 0D;
                    //    dt.Rows[i]["deathRemCurrMonth"] = 0D;
                    //    dt.Rows[i]["refundRemYTDPrevious"] = 0D;
                    //    dt.Rows[i]["refundRemCurrMonth"] = 0D;
                    //    dt.Rows[i]["ytdPrevious"] = 0D;
                    //}

                    if (contractNumber == "FF23070LI")
                    {
                    }
                    if (contractNumber == "E21044LI")
                    {
                    }
                    if (contractNumber == "P24002L")
                    {
                    }
                    beginningBalance = dt.Rows[i]["beginningBalance"].ObjToDouble();
                    lastEndingBalance = dt.Rows[i]["endingBalance"].ObjToDouble();

                    deathRemYTDPrevious = dt.Rows[i]["deathRemYTDPrevious"].ObjToDouble();
                    deathRemCurrMonth = dt.Rows[i]["deathRemCurrMonth"].ObjToDouble();
                    refundRemYTDPrevious = dt.Rows[i]["refundRemYTDPrevious"].ObjToDouble();
                    refundRemCurrMonth = dt.Rows[i]["refundRemCurrMonth"].ObjToDouble();

                    interest = dt.Rows[i]["interest"].ObjToDouble();

                    if (runDate > lastSaveDate)
                    {
                        deathRemYTDPrevious += deathRemCurrMonth;
                        refundRemYTDPrevious += refundRemCurrMonth;
                        deathRemCurrMonth = 0D;
                        refundRemCurrMonth = 0D;

                        ytdPrevious = dt.Rows[i]["ytdPrevious"].ObjToDouble();
                        paymentCurrMonth = dt.Rows[i]["paymentCurrMonth"].ObjToDouble();
                        ytdPrevious += paymentCurrMonth;
                        //if ( paymentCurrMonth != 0D)
                        //{
                        //    if (deathRemYTDPrevious > 0D)
                        //        deathRemYTDPrevious += paymentCurrMonth;
                        //    else if ( refundRemYTDPrevious > 0D)
                        //        refundRemYTDPrevious += paymentCurrMonth;
                        //}
                        dt.Rows[i]["ytdPrevious"] = ytdPrevious;
                        dt.Rows[i]["paymentCurrMonth"] = 0D;
                        currentPayments = dt.Rows[i]["currentPayments"].ObjToDouble();
                        //if (currentPayments > 0D && ytdPrevious == 0D && paymentCurrMonth == 0D)
                        //{
                        //    dt.Rows[i]["paymentCurrMonth"] = currentPayments;
                        //    doit = true;
                        //}
                    }

                    trustPaid = false;
                    trustRefunded = false;
                    str = dt.Rows[i]["trustRemoved"].ObjToString().Trim().ToUpper();
                    if (str == "YES")
                        trustPaid = true;

                    str = dt.Rows[i]["trustRefunded"].ObjToString().Trim().ToUpper();
                    if (str == "YES")
                        trustRefunded = true;


                    dateRemoved = new DateTime(1950, 1, 1);
                    if (deathRemCurrMonth > 0D || refundRemCurrMonth > 0D)
                        dateRemoved = this.dateTimePicker2.Value;
                    if (trustPaid || trustRefunded)
                    {
                        date = dt.Rows[i]["dateRemoved"].ObjToDateTime();
                        if (date.Year > 100)
                        {
                            if (date.Year >= this.dateTimePicker2.Value.Year)
                            {
                                dateRemoved = date;
                                days = DateTime.DaysInMonth(dateRemoved.Year, dateRemoved.Month);
                                dateRemoved = new DateTime(dateRemoved.Year, dateRemoved.Month, days);
                            }
                        }
                        if (dateRemoved.Year < this.dateTimePicker2.Value.Year)
                        {
                            trustPaid = false;
                            trustRefunded = false;
                        }
                    }

                    endingBalance = dt.Rows[i]["endingBalance"].ObjToDouble();
                    //removals = dt.Rows[i]["currentRemovals"].ObjToDouble();
                    //if (removals > 0D)
                    //{
                    //    strRemoval = dt.Rows[i]["trustRemoved"].ObjToString();
                    //    if (String.IsNullOrWhiteSpace(strRemoval) || strRemoval.ToUpper() == "NO")
                    //    {
                    //        record = dt.Rows[i]["record2"].ObjToString();
                    //        myFields = "trustRemoved,YES";
                    //        G1.update_db_table("contracts", "record", record, myFields);
                    //    }
                    //}

                    contractValue = DailyHistory.GetContractValuePlus(dt.Rows[i]);
                    rate = dt.Rows[i]["apr"].ObjToDouble();

                    dt.Rows[i]["contractValue"] = contractValue;
                    dt.Rows[i]["apr"] = rate;

                    double payment = dt.Rows[i]["amtOfMonthlyPayt"].ObjToString().ObjToDouble();
                    amtOfMonthlyPayt = payment;
                    int numPayments = dt.Rows[i]["numberOfPayments"].ObjToString().ObjToInt32();
                    financeMonths = (double)numPayments;
                    double totalInterest = dt.Rows[i]["totalInterest"].ObjToString().ObjToDouble();
                    string dueDate = dt.Rows[i]["dueDate8"].ObjToString();
                    dueDate8 = dueDate.ObjToDateTime();
                    string issueDate = dt.Rows[i]["issueDate8"].ObjToString();
                    //                    DateTime iDate = DailyHistory.GetIssueDate(dt.Rows[i]["issueDate8"].ObjToDateTime(), contractNumber, dx);
                    //iDate = DailyHistory.GetIssueDate(dt.Rows[i]["issueDate8"].ObjToDateTime(), contractNumber, null);
                    DailyHistory.GetIssueDate(contractNumber, ref iDate, ref dateDpPaid);
                    if (dateDpPaid.Year < 100)
                        dateDpPaid = iDate;
                    issueDate = iDate.ToString("MM/dd/yyyy");
                    lastDate = issueDate.ObjToDateTime();
                    if (issueDate.IndexOf("0000") >= 0)
                    {
                        dt.Rows[i]["calcTrust85"] = 0D;
                        dt.Rows[i]["difference"] = endingBalance;
                        continue;
                    }
                    originalDownPayment = dt.Rows[i]["downPayment"].ObjToDouble();
                    if (originalDownPayment <= 0D)
                        originalDownPayment = DailyHistory.GetOriginalDownPayment(dt.Rows[i]);
                    doHalf = false;
                    if (iDate < halfDate)
                        doHalf = true;

                    dt.Rows[i]["issueDate"] = issueDate;
                    deceasedDate = dt.Rows[i]["deceasedDate"].ObjToDateTime();
                    if (deceasedDate.Year > 1850)
                        dt.Rows[i]["DD"] = deceasedDate.ToString("MM/dd/yyyy");
                    dt.Rows[i]["Pmts"] = numPayments.ToString();

                    dbr = 0;
                    if (deceasedDate >= thisMonth && deceasedDate <= runDate)
                    {
                        dt.Rows[i]["dbr"] = 1D;
                        dbr = 1;
                        gotDBR = true;
                    }

                    string apr = dt.Rows[i]["APR"].ObjToString();
                    double dAPR = apr.ObjToDouble() / 100.0D;

                    //startBalance = DailyHistory.GetFinanceValue(dt.Rows[i]);
                    startBalance = DailyHistory.GetFinanceValue(contractNumber);
                    //if (1 == 1)
                    //    continue;
                    cmd = "Select * from `payments` where `contractNumber` = '" + contractNumber + "' order by `paydate8` DESC, `tmstamp` DESC;";
                    dp = G1.get_db_data(cmd);
                    if (dp.Rows.Count <= 0)
                    {
                        dR = dp.NewRow();
                        originalDownPayment = DailyHistory.GetDownPayment(contractNumber);
                        dR["payDate8"] = G1.DTtoMySQLDT(issueDate);
                        dR["paymentAmount"] = originalDownPayment;
                        dR["contractNumber"] = contractNumber;
                        dp.Rows.Add(dR);
                    }

                    trust85 = 0D;
                    trust100 = 0D;
                    oldTrust85 = 0D;

                    DailyHistory.CalculateNewStuff(dp, dAPR, numPayments, startBalance, lastDate);

                    //for ( int k=0; k<dp.Rows.Count; k++)
                    //{
                    //    paymentAmount = dp.Rows[k]["paymentAmount"].ObjToDouble();
                    //    ccFee = dp.Rows[k]["ccFee"].ObjToDouble();
                    //    paymentAmount -= ccFee;
                    //    dp.Rows[k]["paymentAmount"] = paymentAmount;
                    //}

                    method = 0;
                    finaleCount = 0;

                    if (dp.Rows.Count >= 0)
                    {
                        if (contractNumber == "P24002L")
                        {
                        }
                        trust85 = 0D;
                        //ytdPrevious = 0D;
                        paymentCurrMonth = 0D;
                        if (doNewYear)
                            ytdPrevious = 0D;
                        if (dp.Rows.Count > 0)
                        {
                            method = dp.Rows[0]["method"].ObjToInt32();
                            for (int j = 0; j < dp.Rows.Count; j++)
                            {
                                doit = false;
                                if (dp.Rows[j]["fill"].ObjToString().ToUpper() == "D")
                                    continue;
                                honorFinale = false;
                                finale = dp.Rows[j]["new"].ObjToString().ToUpper();
                                if (finale == "FINALE")
                                {
                                    finaleCount++;
                                    if (finaleCount == 1)
                                        honorFinale = true;
                                }
                                edited = dp.Rows[j]["edited"].ObjToString().ToUpper();
                                payDate8 = dp.Rows[j]["payDate8"].ObjToDateTime();
                                if (edited == "TRUSTADJ")
                                    payDate8 = dp.Rows[j]["payDate8"].ObjToDateTime();
                                if (!gotIssue)
                                {
                                    if (payDate8 < January)
                                    {
                                        if (edited != "TRUSTADJ" && edited != "CEMETERY")
                                        {
                                            if ( payDate8 > issueDate.ObjToDateTime() ) // This may be a problem
                                                break;
                                        }
                                    }
                                }
                                if (payDate8 < lastSaveDate)
                                {
                                    if (edited != "TRUSTADJ" && edited != "CEMETERY")
                                    {
                                        downPayment = dp.Rows[j]["downPayment"].ObjToDouble();
                                        if (downPayment <= 0D)
                                            break;
                                        if (!gotIssue)
                                        {
                                            if (iDate < thisMonth || iDate > runDate)
                                                break;
                                        }
                                    }
                                }
                                trust85P = dp.Rows[j]["calculatedTrust85"].ObjToDouble();
                                trust85P = (double)dp.Rows[j]["calculatedTrust85"].ObjToDecimal();
                                trust85P = G1.RoundDown(trust85P);
                                trust100P = dp.Rows[j]["calculatedTrust100"].ObjToDouble();
                                decimal trust85D = (decimal)(dp.Rows[j]["calculatedTrust85"].ObjToDouble());
                                trust85P = (double)(trust85D);
                                //trust85P = G1.RoundDown(trust85P);
                                if (payDate8 <= DailyHistory.secondDate)
                                {
                                    trust100 = dp.Rows[j]["trust100P"].ObjToDouble();
                                    trust85D = (decimal)(dp.Rows[j]["trust85P"].ObjToDouble());
                                    trust85P = (double)(trust85D);
                                    //trust85P = G1.RoundDown(trust85P);
                                }
                                credit = dp.Rows[j]["creditAdjustment"].ObjToDouble();
                                debit = dp.Rows[j]["debitAdjustment"].ObjToDouble();
                                interest = dp.Rows[j]["interestPaid"].ObjToDouble();
                                amtPaid = dp.Rows[j]["paymentAmount"].ObjToDouble();
                                amtPaid = DailyHistory.getPayment(dp, j);
                                downPayment = dp.Rows[j]["downPayment"].ObjToDouble();
                                if (downPayment > 0D)
                                {
                                    amtPaid = downPayment;
                                    interest = 0D;
                                }
                                principal = amtPaid - interest;
                                principal = G1.RoundDown(principal);
                                //force = dp.Rows[j]["force"].ObjToString().ToUpper();
                                force = "";
                                if (edited == "MANUAL" && trust85P < 0D)
                                    force = "Y";
                                if (debit != 0D || credit != 0D)
                                    force = "Y";
                                if (edited != "TRUSTADJ" && edited != "CEMETERY" && force != "Y" && finale.ToUpper() != "FINALE")
                                {
                                    if (principal < 0D && debit == 0D)
                                    {
                                        if (payDate8 > DailyHistory.secondDate)
                                        {
                                            interest = amtPaid;
                                            principal = 0D;
                                            amtPaid = 0D;
                                            dp.Rows[j]["trust85P"] = 0D;
                                            dp.Rows[j]["trust100P"] = 0D;
                                            dp.Rows[j]["prince"] = principal;
                                        }
                                    }

                                    if (SMFS.activeSystem.ToUpper() != "RILES")
                                    {
                                        if (lockTrust85 != "Y" && !contractNumber.ToUpper().EndsWith("LI"))
                                            method = ImportDailyDeposits.CalcTrust85P(payDate8, amtOfMonthlyPayt, issueDate, contractValue, originalDownPayment, financeMonths, amtPaid, principal, debit, credit, rate, ref trust85P, ref trust100P, ref retained);
                                    }
                                    else
                                    {
                                        trust100P = amtPaid;
                                        trust85P = amtPaid;
                                    }
                                }

                                debit = dp.Rows[j]["debitAdjustment"].ObjToDouble();
                                if (debit > 0D)
                                {
                                    if (payDate8 < DailyHistory.majorDate)
                                    {
                                        if (trust85P > 0D)
                                        {
                                            trust85P = trust85P * (-1D);
                                            trust100P = trust100P * (-1D);
                                        }
                                    }
                                }
                                //trust85P = G1.RoundDown(trust85P);
                                //trust100P = G1.RoundDown(trust100P);

                                if (doHalf)
                                {
                                    trust85P = trust100P / 2D;
                                    //trust85P = G1.RoundDown(trust85P);
                                }

                                if (payDate8 >= January && payDate8 <= ytdDate)
                                {
                                    if (payDate8 <= trustDate8)
                                        ytdPrevious = dt.Rows[i]["ytdPrevious"].ObjToDouble();
                                    else
                                        ytdPrevious += trust85P;
                                }
                                if (payDate8 >= thisMonth && payDate8 <= runDate)
                                    doit = true;
                                else
                                {
                                    if (iDate >= thisMonth && iDate <= runDate)
                                    {
                                        if (downPayment > 0D)
                                        {
                                            if (dateDpPaid < thisMonth)
                                                doit = true;
                                        }
                                    }
                                }
                                //else if (iDate >= thisMonth && iDate <= runDate)
                                //    doit = true;

                                if (doit)
                                {
                                    currentRemovals = 0D;
                                    if (trustPaid || trustRefunded)
                                    {
                                        trust85P = G1.RoundValue(trust85P);
                                        paymentCurrMonth += trust85P;
                                        dValue = dt.Rows[i]["paymentCurrMonth"].ObjToDouble(); // Probably testing runDate Month
                                        dt.Rows[i]["as400"] = dValue;
                                        dValue = dt.Rows[i]["ragCurrentMonth"].ObjToDouble();
                                        dValue += trust85P;
                                        dt.Rows[i]["ragCurrentMonth"] = dValue;
                                    }
                                    else
                                    {
                                        trust85P = G1.RoundValue(trust85P);
                                        paymentCurrMonth += trust85P;
                                        dValue = dt.Rows[i]["paymentCurrMonth"].ObjToDouble(); // Probably testing runDate Month
                                        dt.Rows[i]["as400"] = dValue;
                                        dValue = dt.Rows[i]["ragCurrentMonth"].ObjToDouble();
                                        dValue += trust85P;
                                        dt.Rows[i]["ragCurrentMonth"] = dValue;
                                    }
                                }
                                else if (iDate >= thisMonth && iDate <= runDate)
                                {
                                }
                            }
                        }
                        //paymentCurrMonth = G1.RoundDown(paymentCurrMonth);
                        dt.Rows[i]["ytdPrevious"] = ytdPrevious;
                        currentRemovals = 0D;
                        if (trustPaid || trustRefunded)
                        {
                            currentRemovals = paymentCurrMonth + deathRemCurrMonth + deathRemYTDPrevious + refundRemCurrMonth + refundRemYTDPrevious;
                        }
                        dt.Rows[i]["paymentCurrMonth"] = paymentCurrMonth;
                        currentPayments = ytdPrevious + paymentCurrMonth;
                        dt.Rows[i]["currentPayments"] = currentPayments;
                        dt.Rows[i]["currentRemovals"] = currentRemovals;
                        beginningBalance = dt.Rows[i]["beginningBalance"].ObjToDouble();
                        endingBalance = beginningBalance + currentPayments - currentRemovals;
                        dt.Rows[i]["endingBalance"] = endingBalance;
                        dt.Rows[i]["calcTrust85"] = trust85;
                        if (endingBalance == 0D && removals > 0D)
                            endingBalance = removals;
                        if (beginningBalance == 0D && endingBalance == 0D)
                            endingBalance = oldTrust85;
                        if (gotIssue)
                            endingBalance = 0D;
                        difference = endingBalance - trust85;
                        dt.Rows[i]["difference"] = difference;
                        dt.Rows[i]["method"] = method.ToString();

                        if (trustPaid || trustRefunded)
                        {
                            if (runDate > lastSaveDate)
                            {
                                if (dateRemoved < new DateTime (runDate.Year, runDate.Month, runDate.Day ))
                                {
                                    if (trustPaid)
                                    {
                                        if (deathRemYTDPrevious == 0D)
                                        {
                                            //                                            deathRemYTDPrevious = beginningBalance + ytdPrevious + paymentCurrMonth;
                                            deathRemYTDPrevious = beginningBalance + ytdPrevious;
                                            deathRemCurrMonth = paymentCurrMonth;
                                        }
                                        else if (paymentCurrMonth > 0D)
                                        {
                                            deathRemCurrMonth = paymentCurrMonth;
                                        }
                                    }
                                    else if (trustRefunded)
                                    {
                                        if (refundRemYTDPrevious == 0D)
                                        {
                                            refundRemYTDPrevious = beginningBalance + ytdPrevious;
                                            //                                            refundRemYTDPrevious = beginningBalance + ytdPrevious + paymentCurrMonth;
                                            refundRemCurrMonth = paymentCurrMonth;
                                        }
                                        else if (paymentCurrMonth > 0D)
                                        {
                                            refundRemCurrMonth = paymentCurrMonth;
                                        }
                                    }
                                }
                                else if (dateRemoved == new DateTime (runDate.Year, runDate.Month, runDate.Day ))
                                {
                                    if (trustPaid)
                                    {
                                        if (deathRemYTDPrevious == 0D)
                                        {
                                            //deathRemYTDPrevious = beginningBalance + ytdPrevious;
                                            deathRemCurrMonth = beginningBalance + ytdPrevious + paymentCurrMonth;
                                        }
                                    }
                                    else if (trustRefunded)
                                    {
                                        if (refundRemYTDPrevious == 0D)
                                        {
                                            //refundRemYTDPrevious = beginningBalance + ytdPrevious;
                                            refundRemCurrMonth = beginningBalance + ytdPrevious + paymentCurrMonth;
                                        }
                                    }
                                }
                                else // Date Removed is > runDate . . . Don't do anything . . . yet!
                                {
                                }
                            }
                            else
                            {
                                if (runDate == lastSaveDate)
                                {
                                    if (dateRemoved < runDate)
                                    {
                                        if (trustPaid)
                                        {
                                            if (deathRemYTDPrevious == 0D)
                                            {
                                                deathRemYTDPrevious = beginningBalance + ytdPrevious + paymentCurrMonth;
                                                deathRemCurrMonth = 0D;
                                            }
                                        }
                                        else if (trustRefunded)
                                        {
                                            if (refundRemYTDPrevious == 0D)
                                            {
                                                refundRemYTDPrevious = beginningBalance + ytdPrevious + paymentCurrMonth;
                                                refundRemCurrMonth = 0D;
                                            }
                                        }
                                    }
                                    else if (dateRemoved == runDate)
                                    {
                                        if (trustPaid)
                                        {
                                            if (deathRemYTDPrevious == 0D)
                                            {
                                                deathRemYTDPrevious = beginningBalance + ytdPrevious;
                                                deathRemCurrMonth = paymentCurrMonth;
                                            }
                                        }
                                        else if (trustRefunded)
                                        {
                                            if (refundRemYTDPrevious == 0D)
                                            {
                                                refundRemYTDPrevious = beginningBalance + ytdPrevious;
                                                refundRemCurrMonth = paymentCurrMonth;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (trustPaid)
                                    {
                                        if (deathRemYTDPrevious == 0D)
                                            deathRemYTDPrevious = beginningBalance;
                                    }
                                }
                            }
                        }
                        if (contractNumber == "P21003L")
                        {
                        }
                        if (gotIssue)
                            deathRemCurrMonth = currentPayments;
                        dt.Rows[i]["deathRemYTDPrevious"] = deathRemYTDPrevious;
                        dt.Rows[i]["deathRemCurrMonth"] = deathRemCurrMonth;
                        dt.Rows[i]["refundRemYTDPrevious"] = refundRemYTDPrevious;
                        dt.Rows[i]["refundRemCurrMonth"] = refundRemCurrMonth;
                        currentRemovals = deathRemCurrMonth + deathRemYTDPrevious + refundRemCurrMonth + refundRemYTDPrevious;
                        endingBalance = beginningBalance + currentPayments - currentRemovals;
                        //if (gotIssue)
                        //    endingBalance = 0D;
                        //if (dbr == 1)
                        //{
                        if (currentRemovals > 0D && !gotIssue )
                        {
                            if (runDate.ToString("yyyy-MM-dd") != dateRemoved.ToString("yyyy-MM-dd"))
                            {
                                endingBalance = endingBalance - paymentCurrMonth;
                                currentRemovals = currentRemovals - paymentCurrMonth;
                            }
                        }
                        //}
                        dt.Rows[i]["currentRemovals"] = currentRemovals;
                        dt.Rows[i]["endingBalance"] = endingBalance;
                    }
                    else
                    {
                        dt.Rows[i]["calcTrust85"] = 0D;
                        dt.Rows[i]["difference"] = endingBalance;
                        dt.Rows[i]["method"] = method.ToString();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("***ERROR*** " + ex.Message.ToString());
                }
            }
            //G1.WriteAudit("Row=" + auditCount.ToString());
            //MessageBox.Show("***Finished***");
        }
        /***********************************************************************************************/
        private void ProcessData(DataTable dt, DateTime fromDate, DateTime runDate, string LoadColumn)
        {
            /*select* from payments where payDate8 >= '20190101' and payDate8 <= '20190131' and paymentAmount > '0';
            SELECT* FROM payments WHERE payDate8 >= '20190101' AND payDate8 <= '20190131' AND downPayment > '0';
            SELECT* FROM contracts WHERE issueDate8 >= '20190101' AND issueDate8 <= '20190131';*/

            string date1 = G1.DateTimeToSQLDateTime(fromDate);
            string date2 = G1.DateTimeToSQLDateTime(runDate);

            string cmd = "Select * from payments where payDate8 >= '" + date1 + "' AND payDate8 <= '" + date2 + "' ";
            ProcessPayAmounts(dt, cmd, "paymentAmount");
        }
        /***********************************************************************************************/
        private void showLabel(string text)
        {
            //label5.Text = text;
            //label5.Refresh();
        }
        /***********************************************************************************************/
        private void ProcessPayAmounts(DataTable dt, string cmd, string what)
        {
            DataRow[] dR = null;
            string contractNumber = "";
            double payment = 0D;
            double contractValue = 0D;
            DateTime issueDate = DateTime.Now;
            double financeMonths = 0D;
            double amtOfMonthlyPayt = 0D;
            double rate = 0D;
            double downPayment1 = 0D;
            if (G1.get_column_number(dt, "trust85P") < 0)
                dt.Columns.Add("trust85P", Type.GetType("System.Double"));

            cmd += " AND " + what + " <> '0' ORDER by contractNumber ";
            cmd += ";";
            DataTable dx = G1.get_db_data(cmd);
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                contractNumber = dx.Rows[i]["contractNumber"].ObjToString();
                payment = dx.Rows[i][what].ObjToDouble();
                dR = dt.Select("contractNumber='" + contractNumber + "'");
                contractValue = DailyHistory.GetContractValuePlus(dR[0]);
                issueDate = dt.Rows[i]["issueDate8"].ObjToDateTime();
                issueDate = DailyHistory.GetIssueDate(issueDate, contractNumber, null);
                financeMonths = dR[0]["numberOfPayments"].ObjToDouble();
                amtOfMonthlyPayt = dR[0]["amtOfMonthlyPayt"].ObjToDouble();
                rate = dR[0]["apr"].ObjToDouble() / 100.0D;
                downPayment1 = dR[0]["downPayment1"].ObjToDouble();
                //if (downpayment > 0D)
                //{
                //    if (downPayment1 > downpayment)
                //        downpayment = downPayment1;
                //}

                //principal = payment + credit - debit - interest + downpayment;
                //calculateTrust100 = true;
                //if (payment == 0D && downpayment == 0D && (credit != 0D || debit != 0D))
                //{
                //    calculateTrust100 = false;
                //    trust85 = dt.Rows[i]["trust85P"].ObjToDouble();
                //    trust100 = dt.Rows[i]["trust100P"].ObjToDouble();
                //}
                //else
                //{
                //    dt.Rows[i]["trust85P"] = 0D;
                //    dt.Rows[i]["trust100P"] = 0D;
                //}
                //if (payment != 0D || downpayment != 0D || credit != 0D || debit != 0D)
                //    calculateTrust100 = true;

                //if (calculateTrust100)
                //{
                //    method = ImportDailyDeposits.CalcTrust85P(amtOfMonthlyPayt, issueDate.ToString("MM/dd/yyyy"), contractValue, downPayment1, financeMonths, payment, principal, debit, credit, rate, ref trust85, ref trust100, ref retained);
                //    //                    method = ImportDailyDeposits.CalcTrust85(issueDate.ToString("MM/dd/yyyy"), contractValue, downPayment1, financeMonths, payment, principal, rate, ref trust85, ref trust100);
                //}
            }
        }
        /***********************************************************************************************/
        private double FindAS400(string cnum, DataTable dt)
        {
            if (dt == null)
                return 0D;
            double rv = 0D;
            string str = "";
            string sMonth = "";
            string sDay = "";
            string sYear = "";

            int month = 0;
            int day = 0;
            int year = 0;

            DateTime date = DateTime.Now;
            DataRow[] dRows = dt.Select("cnum='" + cnum + "'");
            if (dRows.Length > 0)
            {
                for (int i = 0; i < dRows.Length; i++)
                {
                    str = dRows[i]["DATE"].ObjToString();
                    if (str.Length <= 5)
                        str = "0" + str;
                    sMonth = str.Substring(0, 2);
                    sDay = str.Substring(2, 2);
                    sYear = str.Substring(4, 2);

                    month = sMonth.ObjToInt32();
                    year = sYear.ObjToInt32();
                    if (month == workDate2.Month && year == (workDate2.Year % 100))
                        rv += dRows[i]["Trust85"].ObjToDouble();
                }
            }
            return rv;
        }
        /***********************************************************************************************/
        private void AddNewContracts(DataTable dt)
        {
            string contractNumber = "";
            double contractValue = 0D;
            double trust85P = 0D;
            string fullname = "";
            string fname = "";
            string lname = "";
            string address1 = "";
            string address2 = "";
            string city = "";
            string state = "";
            string zip1 = "";
            string zip2 = "";
            string ssn = "";
            string cmd = "";
            double as400Trust85 = 0D;
            string str = "";
            string contract = "";
            string trust = "";
            string loc = "";
            DateTime deceasedDate = DateTime.Now;

            cmd = "Select * from `funeralhomes` order by `keycode`;";
            DataTable dd = G1.get_db_data(cmd);

            DataTable dx = null;
            DataRow[] dRows = null;

            int count = 0;

            for (int i = 0; i < trust85Dt.Rows.Count; i++)
            {
                str = trust85Dt.Rows[i]["DoneDone"].ObjToString();
                if (str == "Done")
                    continue;

                contractNumber = trust85Dt.Rows[i]["contractNumber"].ObjToString();
                contractValue = trust85Dt.Rows[i]["contractValue"].ObjToDouble();
                if (contractValue <= 0D)
                    continue;
                if (contractNumber == "M23001LI")
                {
                }
                count++;
                //                dRows = dt.Select("contractNumber='" + contractNumber + "'");

                contract = Trust85.decodeContractNumber(contractNumber, ref trust, ref loc);
                DataRow dRow = dt.NewRow();
                dRow["loc"] = loc;
                DataRow[] dr = dd.Select("keycode='" + loc + "'");
                if (dr.Length > 0)
                    dRow["Location Name"] = dr[0]["name"].ObjToString();
                else
                    dRow["Location Name"] = loc;

                deceasedDate = trust85Dt.Rows[i]["deceasedDate"].ObjToDateTime();
                if (deceasedDate.Year > 100)
                    dRow["dd"] = deceasedDate.ToString("MM/dd/yyyy");

                dRow["contractNumber"] = contractNumber;
                dRow["payDate8"] = G1.DTtoMySQLDT(workDate2);
                dRow["found"] = "New";
                trust85P = trust85Dt.Rows[i]["trust85P"].ObjToDouble();
                dRow["beginningBalance"] = 0D;

                dRow["ytdPrevious"] = 0D;
                dRow["paymentCurrMonth"] = trust85P;
                dRow["ragCurrentMonth"] = trust85P;

                dRow["currentRemovals"] = 0D;
                dRow["currentPayments"] = trust85P;
                dRow["endingBalance"] = trust85P;

                //as400Trust85 = FindAS400(contractNumber, newContractsDt);
                //dRow["as400"] = as400Trust85;

                fname = trust85Dt.Rows[i]["firstName"].ObjToString().Trim();
                lname = trust85Dt.Rows[i]["lastName"].ObjToString().Trim();
                fullname = fname.Trim() + " " + lname;
                dRow["fullname"] = fullname;

                cmd = "Select * from`customers` where `contractNumber` = '" + contractNumber + "';";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    address1 = dx.Rows[0]["address1"].ObjToString().Trim();
                    address2 = dx.Rows[0]["address2"].ObjToString().Trim();
                    if (!String.IsNullOrWhiteSpace(address2))
                        address1 += " " + address2;
                    dRow["address1"] = address1;
                    city = dx.Rows[0]["city"].ObjToString();
                    dRow["city"] = city;
                    state = dx.Rows[0]["state"].ObjToString();
                    dRow["state"] = state;
                    zip1 = dx.Rows[0]["zip1"].ObjToString().Trim();
                    zip2 = dx.Rows[0]["zip2"].ObjToString().Trim();
                    if (!String.IsNullOrWhiteSpace(zip2) && zip2 != "0")
                        zip1 += " " + zip2;
                    dRow["zip1"] = zip1;
                    ssn = dx.Rows[0]["ssn"].ObjToString();
                    dRow["ssn"] = ssn;
                }
                dt.Rows.Add(dRow);
            }
        }
        /***********************************************************************************************/
        private void MatchContracts(DataTable dt)
        {
            if (trust85Dt == null)
                return;
            if (G1.get_column_number(dt, "found") < 0)
                dt.Columns.Add("found");
            double totalTrust85P = 0D;
            double trust85P1 = 0D;
            double lower = 0D;
            double upper = 0D;
            try
            {
                gridMain6.Columns["found"].Visible = true;
                string contractNumber = "";

                //for (int i = 0; i < dt.Rows.Count; i++)
                //{
                //    contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                //    DataRow[] dR = trust85Dt.Select("contractNumber='" + contractNumber + "'");
                //    if (dR.Length > 0)
                //    {
                //        totalTrust85P = 0D;
                //        dt.Rows[i]["found"] = "FOUND";
                //        trust85P1 = dt.Rows[i]["paymentCurrMonth"].ObjToDouble();
                //        trust85P1 = G1.RoundDown(trust85P1);
                //        for ( int j=0; j< dR.Length; j++)
                //            totalTrust85P += G1.RoundValue (dR[j]["trust85P"].ObjToDouble());
                //        totalTrust85P = G1.RoundDown(totalTrust85P);
                //        if (trust85P1 != totalTrust85P)
                //        {
                //            lower = totalTrust85P - 0.02D;
                //            upper = totalTrust85P + 0.02D;
                //            if (trust85P1 >= lower && trust85P1 <= upper)
                //                dt.Rows[i]["paymentCurrMonth"] = totalTrust85P;
                //            else
                //                dt.Rows[i]["found"] = totalTrust85P.ToString("###.00");
                //        }
                //    }
                //}
                for (int i = 0; i < trust85Dt.Rows.Count; i++)
                {
                    contractNumber = trust85Dt.Rows[i]["contractNumber"].ObjToString();
                    if (contractNumber == "CT18037L")
                    {

                    }
                    DataRow[] dR = dt.Select("contractNumber='" + contractNumber + "'");
                    if (dR.Length <= 0)
                    {
                        DataRow dRow = dt.NewRow();
                        dRow["contractNumber"] = contractNumber;
                        dRow["found"] = "New";
                        trust85P1 = trust85Dt.Rows[i]["trust85P"].ObjToDouble();
                        dRow["ytdPrevious"] = 0D;
                        dRow["paymentCurrMonth"] = trust85P1;
                        dRow["currentRemovals"] = 0D;
                        dRow["currentPayments"] = trust85P1; // YTD Total
                        dRow["endingBalance"] = trust85P1;

                        dt.Rows.Add(dRow);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** " + ex.ToString());
            }
        }
        /***********************************************************************************************/
        private void btnLeft_Click(object sender, EventArgs e)
        {
            DateTime now = this.dateTimePicker2.Value;
            DateTime date = new DateTime(now.Year, now.Month, 1);
            date = date.AddMonths(-1);
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            date = new DateTime(date.Year, date.Month, days);
            this.dateTimePicker2.Value = date;
        }
        /***********************************************************************************************/
        private void btnRight_Click(object sender, EventArgs e)
        {
            DateTime now = this.dateTimePicker2.Value;
            DateTime date = new DateTime(now.Year, now.Month, 1);
            date = date.AddMonths(1);
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            date = new DateTime(date.Year, date.Month, days);
            this.dateTimePicker2.Value = date;
        }
        /***********************************************************************************************/
        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            DateTime now = this.dateTimePicker2.Value;
            DateTime date = new DateTime(now.Year, now.Month, 1);
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            date = new DateTime(date.Year, date.Month, days);
            this.dateTimePicker2.Value = date;
        }
        ///***********************************************************************************************/
        //public AdvBandedGridView CloneDataGrid(AdvBandedGridView mainDataGridView)
        //{
        //    DataGridView cloneDataGridView = new DataGridView();
        //    AdvBandedGridView clone = new AdvBandedGridView();

        //    if (clone.Columns.Count == 0)
        //    {
        //        foreach (BandedGridColumn datagrid in mainDataGridView.Columns)
        //        {
        //            clone.Columns.Add(datagrid);
        //        }
        //    }
        //    return clone;
        //}
        /***********************************************************************************************/
        //private PrintableComponentLink SetupPrint(DataTable dt)
        //{
        //    DevExpress.XtraPrinting.PrintableComponentLink printableComponentLink1 = new DevExpress.XtraPrinting.PrintableComponentLink(this.components);
        //    AdvBandedGridView grid = CloneDataGrid(gridMain2);
        //    GridControl dgv2 = new DevExpress.XtraGrid.GridControl();
        //    dgv2.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] { grid });

        //    printableComponentLink1.CreateDetailHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateDetailHeaderArea);
        //    printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);
        //    printableComponentLink1.BeforeCreateAreas += new System.EventHandler(this.printableComponentLink1_BeforeCreateAreas);
        //    printableComponentLink1.AfterCreateAreas += new System.EventHandler(this.printableComponentLink1_AfterCreateAreas);

        //    printableComponentLink1.Landscape = true;

        //    return printableComponentLink1;
        //}
        ///***********************************************************************************************/
        //private void printPreviewDGV2(object sender, EventArgs e)
        //{
        //    isPrinting = true;
        //    footerCount = 0;
        //    printRow = 1;
        //    if (this.components == null)
        //        this.components = new System.ComponentModel.Container();

        //    DataTable dt = (DataTable)dgv2.DataSource;

        //    DevExpress.XtraPrinting.PrintingSystem printingSystem1 = new DevExpress.XtraPrinting.PrintingSystem(this.components);
        //    DevExpress.XtraPrinting.PrintableComponentLink printableComponentLink1 = new DevExpress.XtraPrinting.PrintableComponentLink(this.components);

        //    printingSystem1.Links.AddRange(new object[] {
        //    printableComponentLink1});


        //    printableComponentLink1.Component = dgv6;
        //    if (dgv2.Visible)
        //        printableComponentLink1.Component = dgv2;
        //    else if (dgv7.Visible)
        //        printableComponentLink1.Component = dgv7;
        //    else if (dgv8.Visible)
        //        printableComponentLink1.Component = dgv8;

        //    printableComponentLink1.PrintingSystemBase = printingSystem1;

        //    //printableComponentLink1.EnablePageDialog = true;

        //    printableComponentLink1.CreateDetailHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateDetailHeaderArea);
        //    printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);
        //    printableComponentLink1.BeforeCreateAreas += new System.EventHandler(this.printableComponentLink1_BeforeCreateAreas);
        //    printableComponentLink1.AfterCreateAreas += new System.EventHandler(this.printableComponentLink1_AfterCreateAreas);

        //    printableComponentLink1.Landscape = true;

        //    Printer.setupPrinterMargins(50, 100, 80, 50);
        //    //Printer.setupPrinterMargins(50, 50, 80, 50);

        //    pageMarginLeft = Printer.pageMarginLeft;
        //    pageMarginRight = Printer.pageMarginRight;
        //    pageMarginTop = Printer.pageMarginTop;
        //    pageMarginBottom = Printer.pageMarginBottom;

        //    printableComponentLink1.Margins.Left = pageMarginLeft;
        //    printableComponentLink1.Margins.Right = pageMarginRight;
        //    printableComponentLink1.Margins.Top = pageMarginTop;
        //    printableComponentLink1.Margins.Bottom = pageMarginBottom;

        //    printableComponentLink1.CreateDocument();
        //    printableComponentLink1.ShowPreview();
        //    isPrinting = false;
        //}
        /***********************************************************************************************/
        private int pageMarginLeft = 0;
        private int pageMarginRight = 0;
        private int pageMarginTop = 0;
        private int pageMarginBottom = 0;
        private bool isPrinting = false;
        private string majorHeading = "";
        /***********************************************************************************************/
        private void printPreviewToolStripMenuItemY_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv6.DataSource;
            if (dgv2.Visible)
            {
                dt = (DataTable)dgv2.DataSource;
                G1.AdjustColumnWidths(gridMain2, 0.65D, true);
            }
            else if (dgv7.Visible)
            {
                dt = (DataTable)dgv7.DataSource;
                G1.AdjustColumnWidths(gridMain7, 0.65D, true);
            }
            else if (dgv8.Visible)
            {
                dt = (DataTable)dgv8.DataSource;
                G1.AdjustColumnWidths(gridMain8, 0.65D, true);
            }

            DataTable saveDt = dt.Copy();

            if (this.components == null)
                this.components = new System.ComponentModel.Container();
            DevExpress.XtraPrinting.PrintingSystem printingSystem1 = new DevExpress.XtraPrinting.PrintingSystem(this.components);

            DataTable dxx = dt.DefaultView.ToTable(true, "locind");
            string locind = "";
            DataRow[] dRows = null;
            CompositeLink compositeLink = new CompositeLink(new PrintingSystem());
            compositeLink.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);
            for (int i = 0; i < 2; i++)
            {
                locind = dxx.Rows[i]["locind"].ObjToString();
                majorHeading = locind;
                if (!chk2002.Checked)
                {
                    dRows = pre2002Dt.Select("locind='" + locind + "'");
                    if (dRows.Length > 0)
                        majorHeading = dRows[0]["name"].ObjToString();
                }
                if (dgv2.Visible)
                {
                    try
                    {
                        dRows = saveDt.Select("locind='" + locind + "'");
                        dt = saveDt.Clone();
                        G1.ConvertToTable(dRows, dt);
                        dgv2.DataSource = dt;
                        PrintableComponentLink p1 = printSystem(sender, e, dgv2);
                        p1.PrintingSystemBase = printingSystem1;
                        printingSystem1.Links.Add(p1);
                        p1.CreateDocument();
                        try
                        {
                            compositeLink.Links.Add(p1);
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
            //compositeLink.CreateDocument();
            compositeLink.ShowPreviewDialog();
            //printingSystem1.PrintDlg();
            if (dgv2.Visible)
            {
                G1.AdjustColumnWidths(gridMain2, 0.65D, false);
            }
            else if (dgv7.Visible)
            {
                G1.AdjustColumnWidths(gridMain7, 0.65D, false);
            }
            else if (dgv8.Visible)
            {
                G1.AdjustColumnWidths(gridMain8, 0.65D, false);
            }
        }
        /***********************************************************************************************/
        private PrintableComponentLink printSystem(object sender, EventArgs e, GridControl dgv)
        {
            printFirst = true;
            isPrinting = true;
            footerCount = 0;
            printRow = 1;

            DevExpress.XtraPrinting.PrintableComponentLink printableComponentLink1 = new DevExpress.XtraPrinting.PrintableComponentLink(this.components);

            //printingSystem1.Links.AddRange(new object[] {
            //printableComponentLink1});

            //printingSystem1.Links.Add( printableComponentLink1 );

            printableComponentLink1.Component = dgv;

            //printableComponentLink1.PrintingSystemBase = printingSystem1;

            //printableComponentLink1.EnablePageDialog = true;

            printableComponentLink1.CreateDetailHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateDetailHeaderArea);
            printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);
            printableComponentLink1.BeforeCreateAreas += new System.EventHandler(this.printableComponentLink1_BeforeCreateAreas);
            printableComponentLink1.AfterCreateAreas += new System.EventHandler(this.printableComponentLink1_AfterCreateAreas);

            printableComponentLink1.Landscape = true;

            Printer.setupPrinterMargins(50, 100, 80, 50);
            //Printer.setupPrinterMargins(50, 50, 80, 50);

            pageMarginLeft = Printer.pageMarginLeft;
            pageMarginRight = Printer.pageMarginRight;
            pageMarginTop = Printer.pageMarginTop;
            pageMarginBottom = Printer.pageMarginBottom;

            printableComponentLink1.Margins.Left = pageMarginLeft;
            printableComponentLink1.Margins.Right = pageMarginRight;
            printableComponentLink1.Margins.Top = pageMarginTop;
            printableComponentLink1.Margins.Bottom = pageMarginBottom;

            //printableComponentLink1.CreateDocument();
            //            printableComponentLink1.ShowPreview();
            isPrinting = false;
            return printableComponentLink1;
        }
        /***********************************************************************************************/
        private void printPreviewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            printFirst = true;
            isPrinting = true;
            footerCount = 0;
            printRow = 1;
            if (this.components == null)
                this.components = new System.ComponentModel.Container();

            DevExpress.XtraPrinting.PrintingSystem printingSystem1 = new DevExpress.XtraPrinting.PrintingSystem(this.components);
            DevExpress.XtraPrinting.PrintableComponentLink printableComponentLink1 = new DevExpress.XtraPrinting.PrintableComponentLink(this.components);

            printingSystem1.Links.AddRange(new object[] {
            printableComponentLink1});

            printableComponentLink1.Component = dgv6;
            if (dgv2.Visible)
            {
                printableComponentLink1.Component = dgv2;
                DataTable dx = (DataTable)dgv2.DataSource;
                if (dx.Rows.Count > 0)
                {
                    majorLastLocation = dx.Rows[0]["Location Name"].ObjToString();
                }
                G1.AdjustColumnWidths(gridMain2, 0.65D, true);
            }
            else if (dgv7.Visible)
            {
                G1.AdjustColumnWidths(gridMain7, 0.65D, true);
                printableComponentLink1.Component = dgv7;
            }
            else if (dgv8.Visible)
            {
                printableComponentLink1.Component = dgv8;
                G1.AdjustColumnWidths(gridMain8, 0.65D, true);
            }

            printableComponentLink1.PrintingSystemBase = printingSystem1;

            //printableComponentLink1.EnablePageDialog = true;

            printableComponentLink1.CreateDetailHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateDetailHeaderArea);
            printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);
            printableComponentLink1.BeforeCreateAreas += new System.EventHandler(this.printableComponentLink1_BeforeCreateAreas);
            printableComponentLink1.AfterCreateAreas += new System.EventHandler(this.printableComponentLink1_AfterCreateAreas);

            printableComponentLink1.Landscape = true;

            Printer.setupPrinterMargins(50, 100, 80, 50);
            //Printer.setupPrinterMargins(50, 50, 80, 50);

            pageMarginLeft = Printer.pageMarginLeft;
            pageMarginRight = Printer.pageMarginRight;
            pageMarginTop = Printer.pageMarginTop;
            pageMarginBottom = Printer.pageMarginBottom;

            printableComponentLink1.Margins.Left = pageMarginLeft;
            printableComponentLink1.Margins.Right = pageMarginRight;
            printableComponentLink1.Margins.Top = pageMarginTop;
            printableComponentLink1.Margins.Bottom = pageMarginBottom;

            printableComponentLink1.CreateDocument();
            //ModifyDocument(printableComponentLink1.PrintingSystem.Pages, printableComponentLink1.PrintingSystem);
            if (continuousPrint)
            {
                if (File.Exists(fullPath))
                    File.Delete(fullPath);
                if (fullPath.ToUpper().IndexOf(".PDF") > 0)
                    printableComponentLink1.ExportToPdf(fullPath);
                else
                    printableComponentLink1.ExportToCsv(fullPath);
            }
            else
                printableComponentLink1.ShowPreview();
            //            printableComponentLink1.ExportToRtf("C:/rag/testdoc");
            if (dgv2.Visible)
            {
                G1.AdjustColumnWidths(gridMain2, 0.65D, false);
            }
            else if (dgv7.Visible)
            {
                G1.AdjustColumnWidths(gridMain7, 0.65D, false);
            }
            else if (dgv8.Visible)
            {
                G1.AdjustColumnWidths(gridMain8, 0.65D, false);
            }
            isPrinting = false;
        }
        /***********************************************************************************************/
        private void ModifyDocument(PageList pages, PrintingSystem printingSystem)
        {
            int count = 0;
            foreach (DevExpress.XtraPrinting.Page p in pages)
            {
                count++;
                PageInfoBrick marginalAreaBrick = new PageInfoBrick();
                TextBrick headerAreaBrick = new TextBrick();
                DevExpress.XtraPrinting.NativeBricks.XETextBrick tBrick = new DevExpress.XtraPrinting.NativeBricks.XETextBrick();

                DevExpress.XtraPrinting.Native.NestedBrickIterator iterator = new DevExpress.XtraPrinting.Native.NestedBrickIterator(p.InnerBricks);
                while (iterator.MoveNext())
                {
                    VisualBrick visualBrick = iterator.CurrentBrick as VisualBrick;
                    if (visualBrick != null)
                        if (visualBrick.Value != null)
                        {
                            if (visualBrick.Value.ObjToString().ToUpper() == "LOCATION HEADER")
                            {
                                headerAreaBrick = (TextBrick)visualBrick;
                                headerAreaBrick.Text = "Page : " + count.ToString();
                                visualBrick = headerAreaBrick;
                            }
                        }
                }
                marginalAreaBrick.Format = headerAreaBrick.Text;
            }
        }
        /***********************************************************************************************/
        void link1_CreateDetailHeaderArea(object sender, CreateAreaEventArgs e)
        {
            string s = "I am the Detail Header Area";
            e.Graph.StringFormat = new BrickStringFormat(StringAlignment.Center, StringAlignment.Center);
            e.Graph.Font = new Font("Comic Sans MS", 10);
            e.Graph.BackColor = Color.LightGreen;
            e.Graph.ForeColor = Color.Green;
            SizeF sz = e.Graph.MeasureString(s);
            sz.Width += 2;
            RectangleF r = new RectangleF(new PointF(0, 0), sz);
            e.Graph.DrawString(s, r);
        }
        /***********************************************************************************************/
        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            isPrinting = true;
            footerCount = 0;
            printRow = 1;
            if (this.components == null)
                this.components = new System.ComponentModel.Container();
            DevExpress.XtraPrinting.PrintingSystem printingSystem1 = new DevExpress.XtraPrinting.PrintingSystem(this.components);
            DevExpress.XtraPrinting.PrintableComponentLink printableComponentLink1 = new DevExpress.XtraPrinting.PrintableComponentLink(this.components);

            printingSystem1.Links.AddRange(new object[] {
            printableComponentLink1});

            printableComponentLink1.Component = dgv6;
            if (dgv2.Visible)
                printableComponentLink1.Component = dgv2;
            else if (dgv7.Visible)
                printableComponentLink1.Component = dgv7;
            else if (dgv8.Visible)
                printableComponentLink1.Component = dgv8;

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
        private CreateAreaEventArgs publicE = null;
        private bool printFirst = true;
        private bool printFirstToo = true;
        private void printableComponentLink1_CreateMarginalHeaderArea(object sender, CreateAreaEventArgs e)
        {
            if (printFirst)
                publicE = e;
            printFirst = false;
            Printer.setupPrinterQuads(e, 2, 3);
            Font font = new Font("Ariel", 16);
            Printer.DrawQuad(1, 1, Printer.xQuads, 2, "South Mississippi Funeral Services, LLC", Color.Black, BorderSide.Top, font, HorizontalAlignment.Center);
            //Printer.DrawQuad(1, 1, Printer.xQuads, 2, majorHeading, Color.Black, BorderSide.Top, font, HorizontalAlignment.Center);

            font = new Font("Ariel", 8);

            Printer.SetQuadSize(12, 12);

            Printer.DrawGridDate(2, 3, 2, 3, Color.Black, BorderSide.None, font);
            Printer.DrawGridPage(11, 3, 2, 3, Color.Black, BorderSide.None, font);

            Printer.DrawQuad(1, 9, 2, 3, "User : " + LoginForm.username, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);

            //Printer.DrawQuad(1, 6, Printer.xQuads, 2, "Location Header", Color.Black, BorderSide.None, font, HorizontalAlignment.Center);

            if (chk2002.Checked)
                Printer.DrawQuad(1, 6, 2, 3, "After 2002", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);
            else
                Printer.DrawQuad(1, 6, 2, 3, "Before 2002", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);


            //string year = workDate2.Year.ToString("D4");
            //string month = workDate2.ToString("MMMMMMMMMMMMM");
            DateTime localDate = this.dateTimePicker2.Value;
            int days = DateTime.DaysInMonth(localDate.Year, localDate.Month);
            localDate = new DateTime(localDate.Year, localDate.Month, days);
            localDate = localDate.AddDays(1);
            string year = this.dateTimePicker2.Value.Year.ToString("D4");
            string month = this.dateTimePicker2.Value.ToString("MMMMMMMMMMMMM");
            month = localDate.ToString("MMMMMMMMMMMMM");
            year = localDate.Year.ObjToString(); ;
            string dateStr = " for " + month + " " + year;
            dateStr = " by " + month + " 1, " + year;

            font = new Font("Ariel", 10, FontStyle.Regular);
            if (dgv6.Visible)
                Printer.DrawQuad(6, 8, 4, 4, "All Trust Data" + dateStr, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            else if (dgv2.Visible)
                Printer.DrawQuad(5, 8, 4, 4, "Payments Placed in Trust" + dateStr, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            else if (dgv7.Visible)
                Printer.DrawQuad(4, 8, 6, 4, "Beginning Period Trust Report-Trust Account" + dateStr, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            else if (dgv8.Visible)
                Printer.DrawQuad(5, 8, 5, 4, "Payments Removed from Trust" + dateStr, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            //            Printer.DrawQuadTicks();
            DateTime date = this.dateTimePicker2.Value;
            //            date = date.AddDays(1);
            string workDate = date.Month.ToString("D2") + "/" + date.Day.ToString("D2") + "/" + (date.Year % 100).ToString("D2");
            string workDate1 = date.Month.ToString("D2") + "/01/" + (date.Year % 100).ToString("D2");

            string str = "Report : " + workDate1 + " - " + workDate;

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
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (dgv6.Visible)
                SetSpyGlass(gridMain6);
            else if (dgv2.Visible)
                SetSpyGlass(gridMain2);
            else if (dgv7.Visible)
                SetSpyGlass(gridMain7);
            else if (dgv8.Visible)
                SetSpyGlass(gridMain8);
            else if (dgv9.Visible)
                SetSpyGlass(gridMain9);
            else if (dgv10.Visible)
                SetSpyGlass(gridMain10);
        }
        /***********************************************************************************************/
        private void SetSpyGlass(DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView grid)
        {
            G1.ShowHideFindPanel(grid);
            //if (grid.OptionsFind.AlwaysVisible == true)
            //    grid.OptionsFind.AlwaysVisible = false;
            //else
            //    grid.OptionsFind.AlwaysVisible = true;
        }
        /***********************************************************************************************/
        private void chkFilterNewContracts_CheckedChanged(object sender, EventArgs e)
        {
            if (dgv6.Visible)
            {
                dgv6.RefreshDataSource();
                gridMain6.RefreshData();
                dgv6.Refresh();
            }
            else if (dgv7.Visible)
            {
                dgv7.RefreshDataSource();
                gridMain7.RefreshData();
                dgv7.Refresh();
            }
            else if (dgv8.Visible)
            {
                dgv8.RefreshDataSource();
                gridMain8.RefreshData();
                dgv8.Refresh();
            }
            this.Refresh();
        }
        /***********************************************************************************************/
        private void gridMain6_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv6.DataSource;
            if (chkFilterNewContracts.Checked)
            {
                double beginningBalance = dt.Rows[row]["beginningBalance"].ObjToDouble();
                double endingBalance = dt.Rows[row]["endingBalance"].ObjToDouble();
                if (beginningBalance == 0D && endingBalance == 0D)
                {
                    e.Visible = false;
                    e.Handled = true;
                    return;
                }
            }
        }
        /***********************************************************************************************/
        private void confirmLocations(DataTable dt)
        {
            string contract = "";
            string locind = "";
            string trust = "";
            string loc = "";
            string locationName = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                locind = dt.Rows[i]["locind"].ObjToString();
                if (String.IsNullOrWhiteSpace(locind))
                {
                    contract = dt.Rows[i]["contractNumber"].ObjToString();
                    if (contract == "FR20002L")
                    {

                    }
                    locationName = dt.Rows[i]["Location Name"].ObjToString();
                    contract = Trust85.decodeContractNumber(contract, ref trust, ref loc);
                    if (loc == "L" || loc == "C")
                        locind = "JCC02" + " " + locationName;
                    else if (loc == "WM")
                        locind = "WM02" + " " + locationName;
                    else if (loc == "WF")
                        locind = "WF02" + " " + locationName;
                    else if (loc == "T")
                        locind = "TY02" + " " + locationName;
                    else if (loc == "HU")
                        locind = "HU02" + " " + locationName;
                    else if (loc == "HT")
                        locind = "HT02" + " " + locationName;
                    else if (loc == "FF")
                        locind = "FF02" + " " + locationName;
                    else if (loc == "E")
                        locind = "E02" + " " + locationName;
                    else if (loc == "CT")
                        locind = "CT02" + " " + locationName;
                    else if (loc == "M")
                        locind = "CC02" + " " + locationName;
                    else if (loc == "B" || loc == "N" || loc == "MC" || loc == "F")
                        locind = "BRF02" + " " + locationName;
                    else if (loc == "P")
                        locind = "BH02" + " " + locationName;
                    else if (loc == "FR")
                        locind = "BH02";
                }
                if (!chk2002.Checked)
                    locind = locind.Replace("02", "");
                dt.Rows[i]["locind"] = locind;
            }
        }
        /***********************************************************************************************/
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnVerify.Hide();
            lblWait.Text = "";
            lblWait.Refresh();
            TabPage current = (sender as TabControl).SelectedTab;
            if (current.Name.Trim().ToUpper() == "TABPLACEDINTRUST")
                LoadTabPlacedInTrust();
            else if (current.Name.Trim().ToUpper() == "TABBEGINNING")
                LoadTabBeginning();
            else if (current.Name.Trim().ToUpper() == "TABREMOVED")
                LoadTabRemoved();
            //else
            //    btnVerify.Show();
        }
        /***********************************************************************************************/
        private void LoadTabPlacedInTrust()
        {
            //if (!RunClicked)
            //    btnRun_Click(null, null);
            if (dgv6.DataSource == null)
                return;
            this.Cursor = Cursors.WaitCursor;
            DataTable dt6 = (DataTable)dgv6.DataSource;
            if (dt6 == null)
                return;
            DataTable dt2 = dt6.Copy();

            if (chkShowLocations.Checked)
            {
                DataView tempview = dt2.DefaultView;
                tempview.Sort = "locind asc, lastname asc, firstname asc";
                dt2 = tempview.ToTable();
                gridMain2.Columns["locind"].GroupIndex = 0;
                gridMain2.Columns["num"].Visible = false;
                chkExpand.Show();
            }
            else
            {
                gridMain2.Columns["locind"].GroupIndex = -1;
                gridMain2.Columns["num"].Visible = true;
                chkExpand.Checked = true;
                chkExpand.Hide();
            }


            double priorYear = 0D;
            double ytdNow = 0D;
            double currentMonth = 0D;
            double deathRemYTDPrevious = 0D;
            double deathRemCurrMonth = 0D;
            double refundRemYTDPrevious = 0D;
            double refundRemCurrMonth = 0D;
            double currentRemoval = 0D;
            double totalPaid = 0D;
            double interest = 0D;
            string contractNumber = "";


            for (int i = 0; i < dt2.Rows.Count; i++)
            {
                contractNumber = dt2.Rows[i]["contractNumber"].ObjToString();
                if (contractNumber == "WF18056LI")
                {
                }
                priorYear = dt2.Rows[i]["beginningBalance"].ObjToDouble();
                ytdNow = dt2.Rows[i]["ytdPrevious"].ObjToDouble();
                currentMonth = dt2.Rows[i]["paymentCurrMonth"].ObjToDouble();
                currentRemoval = dt2.Rows[i]["currentRemovals"].ObjToDouble();
                deathRemYTDPrevious = dt2.Rows[i]["deathRemYTDPrevious"].ObjToDouble();
                deathRemCurrMonth = dt2.Rows[i]["deathRemCurrMonth"].ObjToDouble();
                refundRemYTDPrevious = dt2.Rows[i]["refundRemYTDPrevious"].ObjToDouble();
                refundRemCurrMonth = dt2.Rows[i]["refundRemCurrMonth"].ObjToDouble();
                interest = dt2.Rows[i]["interest"].ObjToDouble();

                //if ( refundRemCurrMonth > 0D || refundRemYTDPrevious > 0D || deathRemCurrMonth > 0D || deathRemYTDPrevious > 0D)
                //{
                //    dt2.Rows[i]["contractNumber"] = "";
                //    continue;
                //}

                //                ytdNow += priorYear;
                totalPaid = ytdNow + currentMonth;

                dt2.Rows[i]["ytdPrevious"] = ytdNow;
                dt2.Rows[i]["endingBalance"] = priorYear + totalPaid;
                dt2.Rows[i]["beginningBalance"] = priorYear;
            }
            //            G1.NumberDataTable(dt7);
            for (int i = (dt2.Rows.Count - 1); i >= 0; i--)
            {
                contractNumber = dt2.Rows[i]["contractNumber"].ObjToString();
                if (String.IsNullOrWhiteSpace(contractNumber))
                    dt2.Rows.RemoveAt(i);
            }

            SetupCemeteries(dt2, gridMain2);

            dgv2.DataSource = dt2;
            //            gridMain7.Columns["Location Name"].Visible = true;
            if (chkShowLocations.Checked)
                gridMain2.Columns["loc"].Visible = true;
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void LoadTabBeginning()
        {
            //if (!RunClicked)
            //    btnRun_Click(null, null);
            DataTable dt7 = null;
            try
            {
                if (dgv6.DataSource == null)
                    return;
                this.Cursor = Cursors.WaitCursor;
                DataTable dt6 = (DataTable)dgv6.DataSource;
                if (dt6 == null)
                    return;
                dt7 = dt6.Copy();

                if (chkShowLocations.Checked)
                {
                    DataView tempview = dt7.DefaultView;
                    tempview.Sort = "locind asc, lastname asc, firstname asc";
                    dt7 = tempview.ToTable();
                    gridMain7.Columns["locind"].GroupIndex = 0;
                    gridMain7.Columns["num"].Visible = false;
                    chkExpand.Show();
                }
                else
                {
                    gridMain7.Columns["locind"].GroupIndex = -1;
                    gridMain7.Columns["num"].Visible = true;
                    chkExpand.Checked = true;
                    chkExpand.Hide();
                }
            }
            catch (Exception ex)
            {
            }



            double priorYear = 0D;
            double ytdNow = 0D;
            double deathRemYTDPrevious = 0D;
            double deathRemCurrMonth = 0D;
            double refundRemYTDPrevious = 0D;
            double refundRemCurrMonth = 0D;
            double currentMonth = 0D;
            double currentRemoval = 0D;
            double totalPaid = 0D;
            double beginningBalance = 0D;
            double interest = 0D;
            string contractNumber = "";

            try
            {
                for (int i = 0; i < dt7.Rows.Count; i++)
                {
                    contractNumber = dt7.Rows[i]["contractNumber"].ObjToString();
                    if (contractNumber == "M23001LI")
                    {
                    }
                    priorYear = dt7.Rows[i]["beginningBalance"].ObjToDouble();
                    ytdNow = dt7.Rows[i]["ytdPrevious"].ObjToDouble();
                    currentMonth = dt7.Rows[i]["paymentCurrMonth"].ObjToDouble();
                    //if (!chk2002.Checked)
                    //    currentMonth = 0D;
                    currentRemoval = dt7.Rows[i]["currentRemovals"].ObjToDouble();
                    deathRemYTDPrevious = dt7.Rows[i]["deathRemYTDPrevious"].ObjToDouble();
                    deathRemCurrMonth = dt7.Rows[i]["deathRemCurrMonth"].ObjToDouble();
                    refundRemYTDPrevious = dt7.Rows[i]["refundRemYTDPrevious"].ObjToDouble();
                    refundRemCurrMonth = dt7.Rows[i]["refundRemCurrMonth"].ObjToDouble();
                    interest = dt7.Rows[i]["interest"].ObjToDouble();
                    //interest = 0D;

                    currentRemoval = deathRemCurrMonth + refundRemCurrMonth;
                    if (currentRemoval > 0D)
                        currentRemoval += interest;

                    ytdNow += priorYear;
                    totalPaid = priorYear + currentMonth - currentRemoval;

                    dt7.Rows[i]["currentRemovals"] = currentRemoval;
                    dt7.Rows[i]["ytdPrevious"] = ytdNow + currentMonth;
                    beginningBalance = ytdNow - deathRemYTDPrevious - refundRemYTDPrevious;
                    if ((deathRemCurrMonth + deathRemYTDPrevious + refundRemCurrMonth + refundRemYTDPrevious) == 0)
                    {
                        beginningBalance += interest;
                        interest = 0D;
                    }
                    else if ((deathRemYTDPrevious + refundRemYTDPrevious) > 0D)
                    {
                        beginningBalance = 0D; // May be a Problem!
                        beginningBalance = (ytdNow + interest) - (deathRemYTDPrevious + refundRemYTDPrevious);
                        if (ytdNow == (deathRemYTDPrevious + refundRemYTDPrevious))
                            beginningBalance = 0D;
                        interest = 0D;
                    }
                    if (!chk2002.Checked)
                    {
                        if ((currentRemoval) > 0D) // Removed on 8/6/2021 from Post2002 but added back for Pre2002
                        {
                            if (currentMonth == 0D)
                                beginningBalance = currentRemoval;
                            interest = 0D;
                        }
                    }

                    if ((currentRemoval) > 0D)
                    {
                        if (currentMonth == 0D)
                        {
                            //beginningBalance = currentRemoval;
                        }
                        interest = 0D;
                    }

                    //if (beginningBalance < 0D)
                    //    beginningBalance = 0D;
                    //if ( beginningBalance >= 0D)
                    dt7.Rows[i]["beginningBalance"] = beginningBalance;
                    dt7.Rows[i]["endingBalance"] = beginningBalance + interest - currentRemoval + currentMonth;
                    if (deathRemYTDPrevious > 0D || refundRemYTDPrevious > 0D)
                    {
                        if ((deathRemYTDPrevious + refundRemYTDPrevious) == beginningBalance)
                            dt7.Rows[i]["endingBalance"] = 0D;
                    }
                    //if (!chk2002.Checked)
                    //    dt7.Rows[i]["paymentCurrMonth"] = 0D;
                }
            }
            catch (Exception ex)
            {

            }
            //            G1.NumberDataTable(dt7);

            SetupCemeteries(dt7, gridMain7);

            dgv7.DataSource = dt7;
            //            gridMain7.Columns["Location Name"].Visible = true;
            gridMain7.Columns["loc"].Visible = true;
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void LoadTabBeginningxx()
        {
            //if (!RunClicked)
            //    btnRun_Click(null, null);
            DataTable dt7 = null;
            try
            {
                if (dgv6.DataSource == null)
                    return;
                this.Cursor = Cursors.WaitCursor;
                DataTable dt6 = (DataTable)dgv6.DataSource;
                if (dt6 == null)
                    return;
                dt7 = dt6.Copy();

                DataView tempview = dt7.DefaultView;
                tempview.Sort = "locind asc, lastname asc, firstname asc";
                //            tempview.Sort = "locind asc, lastname asc";
                dt7 = tempview.ToTable();
            }
            catch (Exception ex)
            {
            }



            double priorYear = 0D;
            double ytdNow = 0D;
            double deathRemYTDPrevious = 0D;
            double deathRemCurrMonth = 0D;
            double refundRemYTDPrevious = 0D;
            double refundRemCurrMonth = 0D;
            double currentMonth = 0D;
            double currentRemoval = 0D;
            double totalPaid = 0D;
            double beginningBalance = 0D;
            double interest = 0D;
            string contractNumber = "";

            try
            {
                for (int i = 0; i < dt7.Rows.Count; i++)
                {
                    contractNumber = dt7.Rows[i]["contractNumber"].ObjToString();
                    if (contractNumber == "P19041LI")
                    {
                    }
                    priorYear = dt7.Rows[i]["beginningBalance"].ObjToDouble();
                    ytdNow = dt7.Rows[i]["ytdPrevious"].ObjToDouble();
                    currentMonth = dt7.Rows[i]["paymentCurrMonth"].ObjToDouble();
                    //if (!chk2002.Checked)
                    //    currentMonth = 0D;
                    currentRemoval = dt7.Rows[i]["currentRemovals"].ObjToDouble();
                    deathRemYTDPrevious = dt7.Rows[i]["deathRemYTDPrevious"].ObjToDouble();
                    deathRemCurrMonth = dt7.Rows[i]["deathRemCurrMonth"].ObjToDouble();
                    refundRemYTDPrevious = dt7.Rows[i]["refundRemYTDPrevious"].ObjToDouble();
                    refundRemCurrMonth = dt7.Rows[i]["refundRemCurrMonth"].ObjToDouble();
                    interest = dt7.Rows[i]["interest"].ObjToDouble();
                    //interest = 0D;

                    currentRemoval = deathRemCurrMonth + refundRemCurrMonth;
                    if (currentRemoval > 0D)
                        currentRemoval += interest;

                    ytdNow += priorYear;
                    totalPaid = priorYear + currentMonth - currentRemoval;

                    dt7.Rows[i]["currentRemovals"] = currentRemoval;
                    dt7.Rows[i]["ytdPrevious"] = ytdNow + currentMonth;
                    beginningBalance = ytdNow - deathRemYTDPrevious - refundRemYTDPrevious;
                    if ((deathRemCurrMonth + deathRemYTDPrevious + refundRemCurrMonth + refundRemYTDPrevious) == 0)
                    {
                        beginningBalance += interest;
                        interest = 0D;
                    }
                    else if ((deathRemYTDPrevious + refundRemYTDPrevious) > 0D)
                    {
                        beginningBalance = 0D; // May be a Problem!
                        beginningBalance = (ytdNow + interest) - (deathRemYTDPrevious + refundRemYTDPrevious);
                        if (ytdNow == (deathRemYTDPrevious + refundRemYTDPrevious))
                            beginningBalance = 0D;
                        interest = 0D;
                    }
                    if ((currentRemoval) > 0D)
                    {
                        if (currentMonth == 0D)
                            beginningBalance = currentRemoval;
                        interest = 0D;
                    }

                    //if (beginningBalance < 0D)
                    //    beginningBalance = 0D;
                    //if ( beginningBalance >= 0D)
                    dt7.Rows[i]["beginningBalance"] = beginningBalance;
                    dt7.Rows[i]["endingBalance"] = beginningBalance + interest - currentRemoval + currentMonth;
                    if (deathRemYTDPrevious > 0D || refundRemYTDPrevious > 0D)
                    {
                        if ((deathRemYTDPrevious + refundRemYTDPrevious) == beginningBalance)
                            dt7.Rows[i]["endingBalance"] = 0D;
                    }
                    //if (!chk2002.Checked)
                    //    dt7.Rows[i]["paymentCurrMonth"] = 0D;
                }
            }
            catch (Exception ex)
            {

            }
            //            G1.NumberDataTable(dt7);
            dgv7.DataSource = dt7;
            //            gridMain7.Columns["Location Name"].Visible = true;
            gridMain7.Columns["loc"].Visible = true;
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void gridMain7_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv7.DataSource;

            if (chkFilterNewContracts.Checked)
            {
                double beginningBalance = dt.Rows[row]["beginningBalance"].ObjToDouble();
                double endingBalance = dt.Rows[row]["endingBalance"].ObjToDouble();
                if (beginningBalance == 0D && endingBalance == 0D)
                {
                    e.Visible = false;
                    e.Handled = true;
                    return;
                }
            }
        }
        /***********************************************************************************************/
        private void gridMain6_AfterPrintRow(object sender, DevExpress.XtraGrid.Views.Printing.PrintRowEventArgs e)
        {
            int row = e.RowHandle;
            if (row < 0)
                return;
            DataTable dt = (DataTable)dgv6.DataSource;
            string location = dt.Rows[row]["loc"].ObjToString();
            string name = dt.Rows[row]["Location Name"].ObjToString();
            //if (String.IsNullOrWhiteSpace(grid6lastLocation))
            //    grid6lastLocation = location;
            //            if (location != grid6lastLocation )
            if (e.HasFooter)
            {
                if (chkExpand.Checked)
                    e.PS.InsertPageBreak(e.Y);
                //grid6lastLocation = location;
                //gridBand6.Caption = name;
                //startPrinting = true;
            }
        }
        /***********************************************************************************************/
        private void gridMain7_AfterPrintRow(object sender, DevExpress.XtraGrid.Views.Printing.PrintRowEventArgs e)
        {

        }
        /***********************************************************************************************/
        private void LoadTabRemoved()
        {
            //if (!RunClicked)
            //    btnRun_Click(null, null);
            if (dgv6.DataSource == null)
                return;
            this.Cursor = Cursors.WaitCursor;
            DataTable dt6 = (DataTable)dgv6.DataSource;
            if (dt6 == null)
                return;
            DataTable dt8 = dt6.Copy();

            bool doYearEnd = false;
            if (this.dateTimePicker2.Value.Month == 1)
                doYearEnd = true;

            DataView tempview = dt8.DefaultView;

            if (chkShowLocations.Checked)
            {
                tempview = dt8.DefaultView;
                tempview.Sort = "locind asc, lastname asc, firstname asc";
                dt8 = tempview.ToTable();
                gridMain8.Columns["loc"].GroupIndex = -1;
                gridMain8.Columns["locind"].GroupIndex = 0;
                gridMain8.Columns["num"].Visible = false;
                chkExpand.Show();
            }
            else
            {
                gridMain8.Columns["loc"].GroupIndex = -1;
                gridMain8.Columns["locind"].GroupIndex = -1;
                gridMain8.Columns["num"].Visible = true;
                chkExpand.Checked = true;
                chkExpand.Hide();
            }

            if (!chk2002.Checked)
            {
                TabRemoveOld(dt8);
                return;
            }

            double priorYear = 0D;
            double ytdNow = 0D;
            double deathRemYTDPrevious = 0D;
            double deathRemCurrMonth = 0D;
            double refundRemYTDPrevious = 0D;
            double refundRemCurrMonth = 0D;
            double interest = 0D;
            double currentMonth = 0D;
            double currentRemoval = 0D;
            double totalPaid = 0D;

            if (G1.get_column_number(dt8, "currentDeathClaims") < 0)
                dt8.Columns.Add("currentDeathClaims", Type.GetType("System.Double"));
            if (G1.get_column_number(dt8, "currentRefunds") < 0)
                dt8.Columns.Add("currentRefunds", Type.GetType("System.Double"));
            if (G1.get_column_number(dt8, "currentInterest") < 0)
                dt8.Columns.Add("currentInterest", Type.GetType("System.Double"));

            double currentDeathClaims = 0D;
            double currentRefunds = 0D;
            double currentInterest = 0D;

            string trustRemoved = "";
            string trustRefunded = "";
            DateTime dateRemoved = DateTime.Now;
            bool clearData = false;
            //int dbr = 0;
            string contractNumber = "";

            //dt.Rows[i]["ytdPrevious"] = ytdnow;
            //dt.Rows[i]["paymentCurrMonth"] = currentMonth;
            //dt.Rows[i]["currentRemovals"] = currentRemoval;
            //dt.Rows[i]["currentPayments"] = currentPayments;
            //dt.Rows[i]["endingBalance"] = currentPayments + priorYear;

            double v1 = 0D;
            double v2 = 0D;
            double v3 = 0D;
            double v4 = 0D;
            double v5 = 0D;

            for (int i = 0; i < dt8.Rows.Count; i++)
            {
                contractNumber = dt8.Rows[i]["contractNumber"].ObjToString();
                if (contractNumber == "M19082LI")
                {
                }
                //dbr = dt8.Rows[i]["dbr"].ObjToInt32();
                priorYear = dt8.Rows[i]["beginningBalance"].ObjToDouble();
                ytdNow = dt8.Rows[i]["ytdPrevious"].ObjToDouble();
                currentMonth = dt8.Rows[i]["paymentCurrMonth"].ObjToDouble();
                //                if ( dbr != 1 )
                priorYear += currentMonth;
                currentRemoval = dt8.Rows[i]["currentRemovals"].ObjToDouble();

                trustRemoved = dt8.Rows[i]["trustRemoved"].ObjToString().ToUpper();
                trustRefunded = dt8.Rows[i]["trustRefunded"].ObjToString().ToUpper();

                deathRemYTDPrevious = dt8.Rows[i]["deathRemYTDPrevious"].ObjToDouble();
                deathRemCurrMonth = dt8.Rows[i]["deathRemCurrMonth"].ObjToDouble();

                refundRemYTDPrevious = dt8.Rows[i]["refundRemYTDPrevious"].ObjToDouble();
                refundRemCurrMonth = dt8.Rows[i]["refundRemCurrMonth"].ObjToDouble();

                if (deathRemYTDPrevious == currentMonth)
                    currentMonth = 0D;
                if (refundRemYTDPrevious == currentMonth)
                    currentMonth = 0D;

                if (currentMonth > 0D)
                {
                    if (deathRemCurrMonth == 0D && deathRemYTDPrevious > 0D)
                        deathRemYTDPrevious += currentMonth;
                    else if (refundRemCurrMonth == 0D && refundRemYTDPrevious > 0D)
                        refundRemYTDPrevious += currentMonth;
                }
                if (deathRemYTDPrevious > 0D && ytdNow > deathRemYTDPrevious)
                    deathRemYTDPrevious = ytdNow;
                else if (refundRemYTDPrevious > 0D && ytdNow > refundRemYTDPrevious)
                    refundRemYTDPrevious = ytdNow;

                interest = dt8.Rows[i]["interest"].ObjToDouble();
                if (refundRemCurrMonth == 0D && refundRemYTDPrevious == 0D && deathRemCurrMonth == 0D && deathRemYTDPrevious == 0D && interest == 0D)
                {
                    clearData = true;
                    //if (trustRemoved == "YES" || trustRefunded == "YES")
                    //    clearData = false;
                    if (clearData)
                    {
                        dt8.Rows[i]["contractNumber"] = "";
                        continue;
                    }
                }

                currentDeathClaims = deathRemCurrMonth;
                dt8.Rows[i]["currentDeathClaims"] = currentDeathClaims;

                currentRefunds = refundRemCurrMonth;
                dt8.Rows[i]["currentRefunds"] = currentRefunds;

                dt8.Rows[i]["currentInterest"] = currentInterest;

                ytdNow += priorYear;
                totalPaid = ytdNow - currentDeathClaims - currentRefunds;
                totalPaid = G1.RoundValue(totalPaid);

                dt8.Rows[i]["beginningBalance"] = ytdNow;
                dt8.Rows[i]["endingBalance"] = totalPaid;

                dt8.Rows[i]["beginningBalance"] = deathRemYTDPrevious + refundRemYTDPrevious;
                v1 = dt8.Rows[i]["beginningBalance"].ObjToDouble();
                v2 = dt8.Rows[i]["interest"].ObjToDouble();
                v3 = dt8.Rows[i]["currentInterest"].ObjToDouble();
                v4 = dt8.Rows[i]["currentDeathClaims"].ObjToDouble();
                v5 = dt8.Rows[i]["currentRefunds"].ObjToDouble();
                totalPaid = v1 + v2 + v3 + v4 + v5;

                dt8.Rows[i]["endingBalance"] = totalPaid;

                // dt8.Rows[i]["endingBalance"] = deathRemYTDPrevious + refundRemYTDPrevious + currentRefunds + currentDeathClaims;
            }
            //            G1.NumberDataTable(dt7);

            for (int i = (dt8.Rows.Count - 1); i >= 0; i--)
            {
                contractNumber = dt8.Rows[i]["contractNumber"].ObjToString();
                if (String.IsNullOrWhiteSpace(contractNumber))
                    dt8.Rows.RemoveAt(i);
            }

            if (chkShowLocations.Checked)
            {
                tempview = dt8.DefaultView;
                tempview.Sort = "locind asc, lastname asc, firstname asc";
                dt8 = tempview.ToTable();
            }

            SetupCemeteries(dt8, gridMain8);

            dgv8.DataSource = dt8;
            //            gridMain8.Columns["Location Name"].Visible = true;
            gridMain8.Columns["loc"].Visible = true;
            dgv8.RefreshDataSource();
            dgv8.Refresh();
            int row = PositionTab(dt8);
            if (row >= 0)
            {
                gridMain8.FocusedRowHandle = row;
                gridMain8.RefreshData();
            }
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private int PositionTab(DataTable dx)
        {
            int rv = -1;
            if (1 == 1)
                return rv;
            if (!gridMain6.OptionsFind.AlwaysVisible)
                return rv;
            string filter = gridMain6.FindFilterText.ObjToString();
            DataTable dt = (DataTable)dgv6.DataSource;
            if (dt.Rows.Count != 1)
                return rv;
            string contractNumber = dt.Rows[0]["contractNumber"].ObjToString();
            string contract = "";
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                contract = dx.Rows[i]["contractNumber"].ObjToString();
                if (contract == contractNumber)
                {
                    rv = i;
                    break;
                }
            }
            return rv;
        }
        /***********************************************************************************************/
        private void TabRemoveOld(DataTable dt8)
        {
            double priorYear = 0D;
            double ytdNow = 0D;
            double deathRemYTDPrevious = 0D;
            double deathRemCurrMonth = 0D;
            double refundRemYTDPrevious = 0D;
            double refundRemCurrMonth = 0D;
            double interest = 0D;
            double currentMonth = 0D;
            double currentRemoval = 0D;
            double totalPaid = 0D;

            if (G1.get_column_number(dt8, "currentDeathClaims") < 0)
                dt8.Columns.Add("currentDeathClaims", Type.GetType("System.Double"));
            if (G1.get_column_number(dt8, "currentRefunds") < 0)
                dt8.Columns.Add("currentRefunds", Type.GetType("System.Double"));
            if (G1.get_column_number(dt8, "currentInterest") < 0)
                dt8.Columns.Add("currentInterest", Type.GetType("System.Double"));

            double currentDeathClaims = 0D;
            double currentRefunds = 0D;
            double currentInterest = 0D;

            string trustRemoved = "";
            string trustRefunded = "";
            DateTime dateRemoved = DateTime.Now;
            bool clearData = false;
            string contractNumber = "";

            double v1 = 0D;
            double v2 = 0D;
            double v3 = 0D;
            double v4 = 0D;
            double v5 = 0D;

            for (int i = 0; i < dt8.Rows.Count; i++)
            {
                contractNumber = dt8.Rows[i]["contractNumber"].ObjToString();
                if (contractNumber == "P604")
                {
                }
                currentInterest = 0D;
                //                dbr = dt8.Rows[i]["dbr"].ObjToInt32();
                priorYear = dt8.Rows[i]["beginningBalance"].ObjToDouble();
                ytdNow = dt8.Rows[i]["ytdPrevious"].ObjToDouble();
                currentMonth = dt8.Rows[i]["paymentCurrMonth"].ObjToDouble();
                priorYear += currentMonth;
                currentRemoval = dt8.Rows[i]["currentRemovals"].ObjToDouble();

                trustRemoved = dt8.Rows[i]["trustRemoved"].ObjToString().ToUpper();
                trustRefunded = dt8.Rows[i]["trustRefunded"].ObjToString().ToUpper();

                deathRemYTDPrevious = dt8.Rows[i]["deathRemYTDPrevious"].ObjToDouble();
                deathRemCurrMonth = dt8.Rows[i]["deathRemCurrMonth"].ObjToDouble();

                refundRemYTDPrevious = dt8.Rows[i]["refundRemYTDPrevious"].ObjToDouble();
                refundRemCurrMonth = dt8.Rows[i]["refundRemCurrMonth"].ObjToDouble();

                interest = dt8.Rows[i]["interest"].ObjToDouble();
                if (refundRemCurrMonth == 0D && refundRemYTDPrevious == 0D && deathRemCurrMonth == 0D && deathRemYTDPrevious == 0D)
                {
                    clearData = true;
                    if (trustRemoved == "YES" || trustRefunded == "YES")
                        clearData = false;
                    if (clearData)
                    {
                        dt8.Rows[i]["contractNumber"] = "";
                        continue;
                    }
                }
                if (refundRemCurrMonth > 0D || deathRemCurrMonth > 0D)
                {
                    ytdNow = 0D;
                    priorYear = 0D;
                    currentInterest = interest;
                    dt8.Rows[i]["interest"] = 0D;
                    interest = 0D;
                    //if (deathRemCurrMonth > 0D)
                    //    deathRemCurrMonth -= currentInterest;
                    //else if (refundRemCurrMonth > 0D)
                    //    refundRemCurrMonth -= currentInterest;
                }
                else if (deathRemYTDPrevious > 0D || refundRemYTDPrevious > 0D)
                {

                }

                currentDeathClaims = deathRemCurrMonth;
                dt8.Rows[i]["currentDeathClaims"] = currentDeathClaims;

                currentRefunds = refundRemCurrMonth;
                dt8.Rows[i]["currentRefunds"] = currentRefunds;

                dt8.Rows[i]["currentInterest"] = currentInterest;

                ytdNow += priorYear;
                totalPaid = ytdNow - currentDeathClaims - currentRefunds;
                totalPaid = G1.RoundValue(totalPaid);

                dt8.Rows[i]["beginningBalance"] = ytdNow;
                dt8.Rows[i]["endingBalance"] = totalPaid;

                v1 = dt8.Rows[i]["beginningBalance"].ObjToDouble();
                v2 = dt8.Rows[i]["interest"].ObjToDouble();
                v3 = dt8.Rows[i]["currentInterest"].ObjToDouble();
                v4 = dt8.Rows[i]["currentDeathClaims"].ObjToDouble();
                v5 = dt8.Rows[i]["currentRefunds"].ObjToDouble();
                totalPaid = v1 + v2 + v3 + v4 + v5;

                dt8.Rows[i]["endingBalance"] = totalPaid;

                //                dt8.Rows[i]["endingBalance"] = deathRemYTDPrevious + refundRemYTDPrevious + currentRefunds + currentDeathClaims + currentInterest;
            }
            //            G1.NumberDataTable(dt7);

            for (int i = (dt8.Rows.Count - 1); i >= 0; i--)
            {
                contractNumber = dt8.Rows[i]["contractNumber"].ObjToString();
                if (String.IsNullOrWhiteSpace(contractNumber))
                    dt8.Rows.RemoveAt(i);
            }

            dgv8.DataSource = dt8;
            //            gridMain8.Columns["Location Name"].Visible = true;
            gridMain8.Columns["loc"].Visible = true;
            this.Cursor = Cursors.Default;

        }
        /***********************************************************************************************/
        private void gridMain8_AfterPrintRow(object sender, DevExpress.XtraGrid.Views.Printing.PrintRowEventArgs e)
        {
            int row = e.RowHandle;
            if (row < 0)
                return;
            DataTable dt = (DataTable)dgv8.DataSource;
            string location = dt.Rows[row]["loc"].ObjToString();
            string name = dt.Rows[row]["Location Name"].ObjToString();
            //if (String.IsNullOrWhiteSpace(grid6lastLocation))
            //    grid6lastLocation = location;
            //            if (location != grid6lastLocation )
            if (e.HasFooter)
            {
                if (chkExpand.Checked)
                    e.PS.InsertPageBreak(e.Y);
                //grid6lastLocation = location;
                //gridBand6.Caption = name;
                //startPrinting = true;
            }
        }
        /***********************************************************************************************/
        private void gridMain8_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {

            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv8.DataSource;
            if (chkFilterNewContracts.Checked)
            {
                double beginningValue = dt.Rows[row]["beginningBalance"].ObjToDouble();
                double removal = dt.Rows[row]["currentRemovals"].ObjToDouble();
                if (beginningValue == 0D && removal == 0D)
                {
                    e.Visible = false;
                    e.Handled = true;
                    return;
                }
            }
        }
        /***********************************************************************************************/
        private void chkComboLocNames_EditValueChanged(object sender, EventArgs e)
        {
            string names = getLocationNameQuery();
            DataRow[] dRows = originalDt.Select(names);
            DataTable dt = originalDt.Clone();
            for (int i = 0; i < dRows.Length; i++)
                dt.ImportRow(dRows[i]);
            G1.NumberDataTable(dt);
            dgv6.DataSource = dt;
            tabControl1.SelectedIndex = 0;
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
                    string cmd = "Select * from `funeralhomes` where `name` = '" + locIDs[i].Trim() + "';";
                    DataTable dt = G1.get_db_data(cmd);
                    if (dt.Rows.Count > 0)
                    {
                        string id = dt.Rows[0]["keycode"].ObjToString();
                        procLoc += "'" + id.Trim() + "'";
                    }
                }
            }
            return procLoc.Length > 0 ? " `loc` IN (" + procLoc + ") " : "";
        }
        /*******************************************************************************************/
        private void getLocationNameQueryx()
        {
            string procLoc = "";
            string[] locIDs = this.chkComboLocNames.EditValue.ToString().Split('|');
            for (int i = 0; i < locIDs.Length; i++)
            {
                if (!String.IsNullOrWhiteSpace(locIDs[i]))
                {
                    if (procLoc.Trim().Length > 0)
                        procLoc += "|";
                    string cmd = "Select * from `funeralhomes` where `name` = '" + locIDs[i].Trim() + "';";
                    DataTable dt = G1.get_db_data(cmd);
                    if (dt.Rows.Count > 0)
                    {
                        string id = dt.Rows[0]["keycode"].ObjToString();
                        procLoc += id.Trim();
                    }
                }
            }
            chkComboLocation.EditValue = procLoc;
            chkComboLocation.Text = procLoc;
        }
        /***********************************************************************************************/
        private void chkComboLocation_EditValueChanged(object sender, EventArgs e)
        {
            string names = getLocationQuery();
            DataRow[] dRows = originalDt.Select(names);
            DataTable dt = originalDt.Clone();
            for (int i = 0; i < dRows.Length; i++)
                dt.ImportRow(dRows[i]);
            DataView tempview = dt.DefaultView;
            tempview.Sort = "locind asc, lastname asc, firstname asc";
            if (chkAlphaMode.Checked)
                tempview.Sort = "lastname asc, firstname asc";
            dt = tempview.ToTable();
            G1.NumberDataTable(dt);
            dgv6.DataSource = dt;
        }
        /***********************************************************************************************/
        private void chkExpand_CheckedChanged(object sender, EventArgs e)
        {
            if (dgv6.Visible)
            {
                if (chkExpand.Checked)
                {
                    gridMain6.OptionsBehavior.AutoExpandAllGroups = true;
                    gridMain6.ExpandAllGroups();
                    gridMain6.OptionsPrint.ExpandAllGroups = true;
                    gridMain6.OptionsPrint.PrintGroupFooter = true;
                }
                else
                {
                    gridMain6.OptionsBehavior.AutoExpandAllGroups = false;
                    gridMain6.CollapseAllGroups();
                    gridMain6.OptionsPrint.ExpandAllGroups = false;
                    gridMain6.OptionsPrint.PrintGroupFooter = true;
                }
            }
            else if (dgv2.Visible)
            {
                if (chkExpand.Checked)
                {
                    gridMain2.OptionsBehavior.AutoExpandAllGroups = true;
                    gridMain2.ExpandAllGroups();
                    gridMain2.OptionsPrint.ExpandAllGroups = true;
                    gridMain2.OptionsPrint.PrintGroupFooter = true;
                }
                else
                {
                    gridMain2.OptionsBehavior.AutoExpandAllGroups = false;
                    gridMain2.CollapseAllGroups();
                    gridMain2.OptionsPrint.ExpandAllGroups = false;
                    gridMain2.OptionsPrint.PrintGroupFooter = true;
                }
            }
            else if (dgv7.Visible)
            {
                if (chkExpand.Checked)
                {
                    gridMain7.OptionsBehavior.AutoExpandAllGroups = true;
                    gridMain7.ExpandAllGroups();
                    gridMain7.OptionsPrint.ExpandAllGroups = true;
                    gridMain7.OptionsPrint.PrintGroupFooter = true;
                }
                else
                {
                    gridMain7.OptionsBehavior.AutoExpandAllGroups = false;
                    gridMain7.CollapseAllGroups();
                    gridMain7.OptionsPrint.ExpandAllGroups = false;
                    gridMain7.OptionsPrint.PrintGroupFooter = true;
                }
            }
            else if (dgv8.Visible)
            {
                if (chkExpand.Checked)
                {
                    gridMain8.OptionsBehavior.AutoExpandAllGroups = true;
                    gridMain8.ExpandAllGroups();
                    gridMain8.OptionsPrint.ExpandAllGroups = true;
                    gridMain8.OptionsPrint.PrintGroupFooter = true;
                }
                else
                {
                    gridMain8.OptionsBehavior.AutoExpandAllGroups = false;
                    gridMain8.CollapseAllGroups();
                    gridMain8.OptionsPrint.ExpandAllGroups = false;
                    gridMain8.OptionsPrint.PrintGroupFooter = true;
                }
            }
        }
        /***********************************************************************************************/
        private void gridMain6_BeforePrintRow(object sender, DevExpress.XtraGrid.Views.Printing.CancelPrintRowEventArgs e)
        {
        }
        /***********************************************************************************************/
        private void FixStuff(DataTable dt)
        {
            string contractNumber = "";
            string fullname = "";
            string fname = "";
            string lname = "";
            string address1 = "";
            string address2 = "";
            string city = "";
            string state = "";
            string zip1 = "";
            string zip2 = "";
            string ssn = "";

            string address2013 = "";
            string city2013 = "";
            string state2013 = "";
            string zip2013 = "";
            string ssn2013 = "";
            DataTable dx = null;
            string cmd = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                fullname = dt.Rows[i]["fullname"].ObjToString();
                if (String.IsNullOrWhiteSpace(fullname))
                {
                    fname = dt.Rows[i]["firstName1"].ObjToString().Trim();
                    lname = dt.Rows[i]["lastName1"].ObjToString().Trim();
                    fullname = fname.Trim() + " " + lname;
                    dt.Rows[i]["fullname"] = fullname;
                    address1 = dt.Rows[i]["address1"].ObjToString().Trim();
                    address2 = dt.Rows[i]["address2"].ObjToString().Trim();
                    if (!String.IsNullOrWhiteSpace(address2))
                        address1 += " " + address2;
                    dt.Rows[i]["address1"] = address1;
                    city = dt.Rows[i]["city"].ObjToString();
                    dt.Rows[i]["city"] = city;
                    state = dt.Rows[i]["state"].ObjToString();
                    dt.Rows[i]["state"] = state;
                    zip1 = dt.Rows[i]["zip1"].ObjToString().Trim();
                    zip2 = dt.Rows[i]["zip2"].ObjToString().Trim();
                    if (!String.IsNullOrWhiteSpace(zip2) && zip2 != "0")
                        zip1 += " " + zip2;
                    dt.Rows[i]["zip1"] = zip1;
                    ssn = dt.Rows[i]["ssn"].ObjToString();
                    dt.Rows[i]["ssn"] = ssn;
                }
                address1 = dt.Rows[i]["address1"].ObjToString();
                if (String.IsNullOrWhiteSpace(address1))
                {
                    address2013 = dt.Rows[i]["address2013"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(address2013))
                    {
                        dt.Rows[i]["address1"] = address2013;
                        fullname = dt.Rows[i]["fullname"].ObjToString().Trim();
                        if (String.IsNullOrWhiteSpace(fullname))
                        {
                            fname = dt.Rows[i]["firstName"].ObjToString().Trim();
                            lname = dt.Rows[i]["lastName"].ObjToString().Trim();
                            fullname = fname + " " + lname;
                            dt.Rows[i]["fullname"] = fullname;
                        }
                        city2013 = dt.Rows[i]["city2013"].ObjToString();
                        state2013 = dt.Rows[i]["state2013"].ObjToString();
                        zip2013 = dt.Rows[i]["zip2013"].ObjToString();
                        ssn2013 = dt.Rows[i]["ssn2013"].ObjToString();
                        dt.Rows[i]["city"] = city2013;
                        dt.Rows[i]["state"] = state2013;
                        dt.Rows[i]["zip1"] = zip2013;
                        dt.Rows[i]["ssn"] = ssn2013;
                    }
                }
            }
        }
        /***********************************************************************************************/
        private void chkShowLocations_CheckedChanged(object sender, EventArgs e)
        {
            if (dgv6.Visible)
            {
                if (chkShowLocations.Checked)
                {
                    gridMain6.Columns["locind"].GroupIndex = 0;
                    gridMain6.Columns["num"].Visible = false;
                    chkExpand.Show();
                }
                else
                {
                    //                gridMain6.Columns["Location Name"].GroupIndex = -1;
                    gridMain6.Columns["locind"].GroupIndex = -1;
                    gridMain6.Columns["num"].Visible = true;
                    chkExpand.Checked = true;
                    chkExpand.Hide();
                }
            }
            else if (dgv2.Visible)
            {
                if (chkShowLocations.Checked)
                {
                    gridMain2.Columns["locind"].GroupIndex = 0;
                    gridMain2.Columns["num"].Visible = false;
                    chkExpand.Show();
                }
                else
                {
                    gridMain2.Columns["locind"].GroupIndex = -1;
                    gridMain2.Columns["num"].Visible = true;
                    chkExpand.Checked = true;
                    chkExpand.Hide();
                }
            }
            else if (dgv7.Visible)
            {
                if (chkShowLocations.Checked)
                {
                    gridMain7.Columns["locind"].GroupIndex = 0;
                    gridMain7.Columns["num"].Visible = false;
                    chkExpand.Show();
                }
                else
                {
                    gridMain7.Columns["locind"].GroupIndex = -1;
                    gridMain7.Columns["num"].Visible = true;
                    chkExpand.Checked = true;
                    chkExpand.Hide();
                }
            }
            else if (dgv8.Visible)
            {
                if (chkShowLocations.Checked)
                {
                    gridMain8.Columns["loc"].GroupIndex = -1;
                    gridMain8.Columns["locind"].GroupIndex = 0;
                    gridMain8.Columns["num"].Visible = false;
                    chkExpand.Show();
                }
                else
                {
                    gridMain8.Columns["loc"].GroupIndex = -1;
                    gridMain8.Columns["locind"].GroupIndex = -1;
                    gridMain8.Columns["num"].Visible = true;
                    chkExpand.Checked = true;
                    chkExpand.Hide();
                }
            }
        }
        /***********************************************************************************************/
        private void chkDBR_CheckedChanged(object sender, EventArgs e)
        {
            if (chkDBR.Checked)
            {
                DataRow[] dRows = originalDt.Select("dbr='1'");
                DataTable dt = originalDt.Clone();
                for (int i = 0; i < dRows.Length; i++)
                    dt.ImportRow(dRows[i]);
                G1.NumberDataTable(dt);
                dgv6.DataSource = dt;
            }
            else
                dgv6.DataSource = originalDt;
        }
        /***********************************************************************************************/
        private void gridMain6_DoubleClick(object sender, EventArgs e)
        {
            string contract = "";
            DataRow dr = null;
            if (dgv6.Visible)
                dr = gridMain6.GetFocusedDataRow();
            else if (dgv2.Visible)
                dr = gridMain2.GetFocusedDataRow();
            else if (dgv7.Visible)
                dr = gridMain7.GetFocusedDataRow();
            else if (dgv8.Visible)
                dr = gridMain8.GetFocusedDataRow();
            else if (dgv9.Visible)
                dr = gridMain9.GetFocusedDataRow();
            else if (dgv10.Visible)
                dr = gridMain10.GetFocusedDataRow();
            contract = dr["contractNumber"].ObjToString();
            this.Cursor = Cursors.WaitCursor;
            CustomerDetails clientForm = new CustomerDetails(contract);
            clientForm.Show();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void chkShowCurrentMonth_CheckedChanged(object sender, EventArgs e)
        {
            if (chkShowCurrentMonth.Checked)
            {
                DataRow[] dRows = originalDt.Select("paymentCurrMonth>'0'");
                DataTable dt = originalDt.Clone();
                for (int i = 0; i < dRows.Length; i++)
                    dt.ImportRow(dRows[i]);
                G1.NumberDataTable(dt);
                dgv6.DataSource = dt;

            }
            else
                dgv6.DataSource = originalDt;
        }
        /***********************************************************************************************/
        private void btnRunDiff_Click(object sender, EventArgs e)
        {
            //progressBar1.Show();
            //label2.Show();

            //this.Cursor = Cursors.WaitCursor;
            //string contractNumber = "";
            //string cmd = "";
            //DateTime lastDate = DateTime.Now;
            //DateTime payDate8 = DateTime.Now;
            //int month = 0;
            //int year = 0;
            //int day = 0;
            //double startBalance = 0D;
            //double endingBalance = 0D;
            //double value = 0D;
            //double difference = 0D;
            //double trust85 = 0D;
            //DataTable dx = null;

            //DataTable dt = PullXXXData(); //Some of the data in Trust2013 is not in customer file

            //dt.Columns.Add("calcTrust85", Type.GetType("System.Double"));
            //dt.Columns.Add("difference", Type.GetType("System.Double"));

            //label2.Show();

            //label2.Text = "of " + dt.Rows.Count.ToString();
            //label2.Refresh();

            //progressBar1.Show();
            //progressBar1.Minimum = 0;
            //progressBar1.Maximum = dt.Rows.Count;

            //int i = 0;

            //try
            //{
            //    for (i = 0; i < dt.Rows.Count; i++)
            //    {
            //        label2.Text = (i + 1).ToString() + " of " + dt.Rows.Count.ToString();
            //        label2.Refresh();

            //        progressBar1.Value = i + 1;
            //        progressBar1.Refresh();

            //        contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
            //        if (contractNumber == "P17901DI")
            //        {

            //        }
            //        endingBalance = dt.Rows[i]["endingBalance"].ObjToDouble();

            //        cmd = "Select * from `contracts` where `contractNumber` = '" + contractNumber + "';";
            //        dx = G1.get_db_data(cmd);
            //        if (dx.Rows.Count <= 0)
            //        {
            //            dt.Rows[i]["calcTrust85"] = 0D;
            //            dt.Rows[i]["difference"] = endingBalance;
            //            continue;
            //        }

            //        DataTable contractDt = dx.Copy();

            //        double payment = dx.Rows[0]["amtOfMonthlyPayt"].ObjToString().ObjToDouble();
            //        int numPayments = dx.Rows[0]["numberOfPayments"].ObjToString().ObjToInt32();
            //        double totalInterest = dx.Rows[0]["totalInterest"].ObjToString().ObjToDouble();
            //        string dueDate = dx.Rows[0]["dueDate8"].ObjToString();
            //        string issueDate = dx.Rows[0]["issueDate8"].ObjToString();
            //        DateTime iDate = DailyHistory.GetIssueDate(dx.Rows[0]["issueDate8"].ObjToDateTime(), contractNumber, dx);
            //        issueDate = iDate.ToString("MM/dd/yyyy");
            //        lastDate = issueDate.ObjToDateTime();
            //        if (issueDate.IndexOf("0000") >= 0)
            //        {
            //            dt.Rows[i]["calcTrust85"] = 0D;
            //            dt.Rows[i]["difference"] = endingBalance;
            //            continue;
            //        }
            //        string apr = dx.Rows[0]["APR"].ObjToString();
            //        double dAPR = apr.ObjToDouble() / 100.0D;

            //        startBalance = DailyHistory.GetFinanceValue(dx.Rows[0]);

            //        cmd = "Select * from `payments` where `contractNumber` = '" + contractNumber + "' order by `paydate8` DESC, `tmstamp` DESC;";
            //        dx = G1.get_db_data(cmd);

            //        DailyHistory.CalculateNewStuff(dx, dAPR, numPayments, startBalance, lastDate);

            //        if (dx.Rows.Count > 0)
            //        {
            //            trust85 = 0D;
            //            for (int j = 0; j < dx.Rows.Count; j++)
            //            {
            //                if (dx.Rows[j]["fill"].ObjToString().ToUpper() == "D")
            //                    continue;
            //                payDate8 = dx.Rows[j]["payDate8"].ObjToDateTime();
            //                month = payDate8.Month;
            //                year = payDate8.Year;
            //                day = DateTime.DaysInMonth(year, month);
            //                payDate8 = new DateTime(year, month, day);
            //                if (payDate8 > workDate2)
            //                    continue;

            //                value = dx.Rows[j]["calculatedTrust85"].ObjToDouble();
            //                trust85 += value;
            //                if (numPayments <= 0)
            //                    break;
            //            }
            //            dt.Rows[i]["calcTrust85"] = trust85;
            //            difference = endingBalance - trust85;
            //            dt.Rows[i]["difference"] = difference;
            //        }
            //        else
            //        {
            //            dt.Rows[i]["calcTrust85"] = 0D;
            //            dt.Rows[i]["difference"] = endingBalance;
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show("***ERROR*** " + ex.Message.ToString());
            //}
            //dgv9.DataSource = dt;
            //this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void btnPaul_Click(object sender, EventArgs e)
        {
            bool gotFile = false;
            DataTable dx = null;
            if (PaymentsReport.cashRemittedDt == null)
            {
                string fileName = "";
                using (OpenFileDialog ofd = new OpenFileDialog())
                {
                    if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        string file = G1.DecodeFilename(ofd.FileName);
                        fileName = ofd.FileName;
                    }
                }
                if (String.IsNullOrWhiteSpace(fileName))
                    return;

                gotFile = true;
                dx = TrustCSVfile(fileName);
                int lastRow = dx.Rows.Count - 1;
                if (lastRow >= 0)
                    dx.Rows.RemoveAt(lastRow);
                DataView tempview = dx.DefaultView;
                tempview.Sort = "Contract ASC";
                dx = tempview.ToTable();
            }
            else
            {
                dx = PaymentsReport.cashRemittedDt;
                DataView tempview = dx.DefaultView;
                tempview.Sort = "contractNumber ASC";
                dx = tempview.ToTable();
            }

            ComparePayments(dx, gotFile);
        }
        /****************************************************************************************/
        private void ComparePayments(DataTable dt, bool gotFile)
        {
            if (dt == null)
                return;
            DataRow[] dRows = null;
            DataRow[] nRows = null;
            DataTable dt6 = (DataTable)dgv6.DataSource;
            DataTable nDt = new DataTable();
            nDt.Columns.Add("contractNumber");
            nDt.Columns.Add("cashRemitted", Type.GetType("System.Double"));
            nDt.Columns.Add("paymentCurrMonth", Type.GetType("System.Double"));
            nDt.Columns.Add("difference", Type.GetType("System.Double"));

            string contractCol = "contract";
            string trust85Col = "85% Trust";
            if (!gotFile)
            {
                contractCol = "contractNumber";
                trust85Col = "trust85P";
            }

            string cnum = "";
            double trust85 = 0D;
            double totalTrust85 = 0D;
            double difference = 0D;
            int row = 0;
            string locind = "";
            DataRow dRow = null;
            DataRow nRow = null;
            this.Cursor = Cursors.WaitCursor;
            progressBar2.Show();
            progressBar2.Minimum = 0;
            progressBar2.Maximum = dt6.Rows.Count;
            for (int i = 0; i < dt6.Rows.Count; i++)
            {
                progressBar2.Value = i + 1;
                progressBar2.Refresh();

                cnum = dt6.Rows[i]["contractNumber"].ObjToString();
                if (String.IsNullOrWhiteSpace(cnum))
                    continue;
                try
                {
                    trust85 = dt6.Rows[i]["paymentCurrMonth"].ObjToDouble();
                    nRows = nDt.Select("contractNumber='" + cnum + "'");
                    if (nRows.Length <= 0)
                    {
                        dRow = nDt.NewRow();
                        dRow["contractNumber"] = cnum;
                        dRow["paymentCurrMonth"] = trust85;
                        nDt.Rows.Add(dRow);
                    }
                    dRows = dt.Select(contractCol + "='" + cnum + "'");
                    if (dRows.Length > 0)
                    {
                        totalTrust85 = 0D;

                        for (int j = 0; j < dRows.Length; j++)
                        {
                            trust85 = dRows[j][trust85Col].ObjToDouble();
                            totalTrust85 += trust85;
                        }
                        nRows = nDt.Select("contractNumber='" + cnum + "'");
                        if (nRows.Length > 0)
                            nRows[0]["cashRemitted"] = totalTrust85;
                    }
                }
                catch (Exception ex)
                {
                }
            }
            for (int i = 0; i < nDt.Rows.Count; i++)
            {
                totalTrust85 = nDt.Rows[i]["paymentCurrMonth"].ObjToDouble();
                trust85 = nDt.Rows[i]["cashRemitted"].ObjToDouble();
                difference = totalTrust85 - trust85;
                nDt.Rows[i]["difference"] = difference;
            }
            G1.NumberDataTable(nDt);
            dgv10.DataSource = nDt;
            dgv10.RefreshDataSource();
            gridMain10.RefreshData();
            dgv10.Refresh();
            gridMain10.RefreshData();

            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        public static DataTable TrustCSVfile(string filename, PictureBox picLoader = null, bool skipNum = false, string delimiter = ",")
        {
            int maxColumns = 0;
            if (picLoader != null)
                picLoader.Show();
            char cDelimiter = (char)delimiter[0];
            DataTable dt = new DataTable();
            if (!File.Exists(filename))
            {
                DevExpress.XtraEditors.XtraMessageBox.Show("***ERROR*** File does not exist!");
                return null;
            }
            try
            {
                bool first = true;
                string payer = "";
                string line = "";
                int row = 0;
                string str = "";
                FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using (StreamReader sr = new StreamReader(fs))

                {
                    while ((line = sr.ReadLine()) != null)
                    {
                        Application.DoEvents();
                        if (first)
                        {
                            if (line.ToUpper().IndexOf("LAST NAME") < 0)
                                continue;
                            first = false;
                            dt = Import.BuildImportDt(line, skipNum, delimiter);
                            maxColumns = (dt.Columns.Count - 1);
                            continue;
                        }
                        string[] Lines = line.Split(cDelimiter);
                        G1.parse_answer_data(line, delimiter);
                        int count = G1.of_ans_count;
                        if (G1.of_ans_count > maxColumns)
                        {
                            DataRow dRow = dt.NewRow();
                            int inc = 0;
                            if (skipNum)
                                inc = 0;
                            for (int i = 0; i < G1.of_ans_count; i++)
                            {
                                try
                                {
                                    str = G1.of_answer[i].ObjToString().Trim();
                                    str = Import.trim(str);
                                    dRow[i + inc] = str;
                                }
                                catch (Exception ex)
                                {
                                }
                            }
                            dt.Rows.Add(dRow);
                        }
                        row++;
                        //                        picLoader.Refresh();
                    }
                    sr.Close();
                }
            }
            catch (Exception ex)
            {
                DevExpress.XtraEditors.XtraMessageBox.Show(ex.Message, "Error Occurred");
            }
            G1.NumberDataTable(dt);
            if (picLoader != null)
                picLoader.Hide();
            return dt;
        }
        /***********************************************************************************************/
        private void AddHeading(DevExpress.XtraGrid.Views.Printing.CancelPrintRowEventArgs e)
        {
            TextBrick tb = e.PS.CreateTextBrick() as TextBrick;
            tb.Text = majorLastDetail;
            tb.Font = new Font(tb.Font, FontStyle.Bold);
            tb.HorzAlignment = DevExpress.Utils.HorzAlignment.Near;
            tb.Padding = new PaddingInfo(5, 0, 0, 0);
            tb.BackColor = Color.LightGray;
            tb.ForeColor = Color.Black;
            // Get the client page width. 
            SizeF clientPageSize = (e.BrickGraphics as BrickGraphics).ClientPageSize;
            float textBrickHeight = e.Graphics.MeasureString(tb.Text, tb.Font).Height + 4;
            // Calculate a rectangle for the brick and draw the brick. 
            RectangleF textBrickRect = new RectangleF(0, e.Y, (int)clientPageSize.Width, textBrickHeight);
            e.BrickGraphics.DrawBrick(tb, textBrickRect);
            // Adjust the current Y position to print the following row below the brick. 
            e.Y += (int)textBrickHeight;
        }
        /***********************************************************************************************/
        private int footerCount = 0;
        private int printRow = 0;
        private void beforePrintRow(object sender, DevExpress.XtraGrid.Views.Printing.CancelPrintRowEventArgs e)
        {
            if (chkShowLocations.Checked)
            {
                FindLastLocation(e);
                gridBand5.Caption = majorLastDetail;
            }
            if ((printRow % 14) == 0)
            {
                //Font font = new Font("Ariel", 16);
                //BorderSide border = new BorderSide();
                //Printer.localE.Graph.DrawString(majorLastDetail, Color.Red, new RectangleF(50, -e.Y, 150, 150), border);

                //Printer.DrawQuad(0, 0, Printer.xQuads, 2, majorLastDetail, Color.Black, BorderSide.Top, font, HorizontalAlignment.Center);
                e.PS.InsertPageBreak(e.Y);
                if (chkShowLocations.Checked)
                    AddHeading(e);
                printFirstToo = false;
                //ITextBrick brick = e.PS.CreateTextBrick();
                //brick.Rect = new RectangleF(50, -e.Y, 150, 150);
                //brick.Text = "HERE";
                printRow = 1;
                //printableComponentLink1_CreateMarginalHeaderArea(null, publicE);
            }
            else
            {
                printRow++;
                if (printFirstToo && publicE != null)
                {
                    if (chkShowLocations.Checked)
                        AddHeading(e);
                    printFirstToo = false;
                }
            }
            if (e.HasFooter)
            {
                footerCount++;
                if ((footerCount + 1) >= 2)
                {
                    if (chkShowLocations.Checked)
                        CustomFooter(e, "Location (" + majorLastLocation + ")");
                    printFirstToo = true;
                }
            }
        }
        /***********************************************************************************************/
        private string majorLastLocation = "";
        private string majorLastDetail = "";
        private string lastLocation = "";
        /***********************************************************************************************/
        private void FindLastLocation(DevExpress.XtraGrid.Views.Printing.PrintRowEventArgs e)
        {
            majorLastLocation = majorLastDetail;
            lastLocation = "";

            try
            {
                DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gMain = null;
                DataTable dt = null;
                if (dgv6.Visible)
                {
                    dt = (DataTable)dgv6.DataSource;
                    gMain = gridMain6;
                }
                if (dgv2.Visible)
                {
                    dt = (DataTable)dgv2.DataSource;
                    gMain = gridMain2;
                }
                else if (dgv7.Visible)
                {
                    dt = (DataTable)dgv7.DataSource;
                    gMain = gridMain7;
                }
                else if (dgv8.Visible)
                {
                    dt = (DataTable)dgv8.DataSource;
                    gMain = gridMain8;
                }
                int rowHandle = e.RowHandle;
                int row = gMain.GetDataSourceRowIndex(rowHandle);
                lastLocation = dt.Rows[row]["locind"].ObjToString();
                majorLastDetail = dt.Rows[row]["Location Name"].ObjToString();
                //if (!chk2002.Checked)
                //{
                DataRow[] dRows = pre2002Dt.Select("locind='" + lastLocation + "'");
                if (dRows.Length > 0)
                {
                    lastLocation = dRows[0]["name"].ObjToString();
                    majorLastDetail = lastLocation;
                }
                //}
            }
            catch
            {
            }
        }
        /***********************************************************************************************/
        private void afterPrintRow(object sender, DevExpress.XtraGrid.Views.Printing.PrintRowEventArgs e)
        {
            if (e.HasFooter)
            {
                footerCount++;
                if (footerCount >= 2)
                {
                    printRow = 1;
                    footerCount = 0;
                    if (chkExpand.Checked)
                        e.PS.InsertPageBreak(e.Y);
                }
            }
        }
        /***********************************************************************************************/
        private void CustomFooter(DevExpress.XtraGrid.Views.Printing.PrintRowEventArgs e, string footer)
        {
            if (String.IsNullOrWhiteSpace(footer))
                return;

            // Create a text brick and customize its appearance settings. 
            TextBrick tb = e.PS.CreateTextBrick() as TextBrick;
            tb.Text = footer;
            tb.Font = new Font(tb.Font, FontStyle.Bold);
            tb.HorzAlignment = DevExpress.Utils.HorzAlignment.Near;
            tb.Padding = new PaddingInfo(5, 0, 0, 0);
            tb.BackColor = Color.LightGray;
            tb.ForeColor = Color.Black;
            // Get the client page width. 
            SizeF clientPageSize = (e.BrickGraphics as BrickGraphics).ClientPageSize;
            float textBrickHeight = e.Graphics.MeasureString(tb.Text, tb.Font).Height + 4;
            // Calculate a rectangle for the brick and draw the brick. 
            int y = e.Y;
            //y = y - 100;
            RectangleF textBrickRect = new RectangleF(19, y, (int)clientPageSize.Width - 19, textBrickHeight);
            e.BrickGraphics.DrawBrick(tb, textBrickRect);
            // Adjust the current Y position to print the following row below the brick. 
            e.Y += (int)textBrickHeight;
        }
        /***********************************************************************************************/
        private void gridMain2_BeforePrintRow(object sender, DevExpress.XtraGrid.Views.Printing.CancelPrintRowEventArgs e)
        {

        }
        /***********************************************************************************************/
        private void gridMain2_AfterPrintRow(object sender, DevExpress.XtraGrid.Views.Printing.PrintRowEventArgs e)
        {

        }
        /***********************************************************************************************/
        private void gridMain6_CustomDrawGroupRow(object sender, DevExpress.XtraGrid.Views.Base.RowObjectCustomDrawEventArgs e)
        {
            var view = (GridView)sender;
            var info = (GridGroupRowInfo)e.Info;
            var caption = info.Column.Caption;
            if (info.Column.Caption == string.Empty)
            {
                caption = info.Column.ToString();
            }
            info.GroupText = $"{caption} : {info.GroupValueText} ({view.GetChildRowCount(e.RowHandle)})";
        }
        /***********************************************************************************************/
        private void gridMain2_CustomDrawGroupRow(object sender, DevExpress.XtraGrid.Views.Base.RowObjectCustomDrawEventArgs e)
        {
            var view = (GridView)sender;
            var info = (GridGroupRowInfo)e.Info;
            var caption = info.Column.Caption;
            if (info.Column.Caption == string.Empty)
            {
                caption = info.Column.ToString();
            }
            info.GroupText = $"{caption} : {info.GroupValueText} ({view.GetChildRowCount(e.RowHandle)})";

        }
        /***********************************************************************************************/
        private void gridMain7_CustomDrawGroupRow(object sender, DevExpress.XtraGrid.Views.Base.RowObjectCustomDrawEventArgs e)
        {
            var view = (GridView)sender;
            var info = (GridGroupRowInfo)e.Info;
            var caption = info.Column.Caption;
            if (info.Column.Caption == string.Empty)
            {
                caption = info.Column.ToString();
            }
            info.GroupText = $"{caption} : {info.GroupValueText} ({view.GetChildRowCount(e.RowHandle)})";
        }
        /***********************************************************************************************/
        private void gridMain8_CustomDrawGroupRow(object sender, DevExpress.XtraGrid.Views.Base.RowObjectCustomDrawEventArgs e)
        {
            var view = (GridView)sender;
            var info = (GridGroupRowInfo)e.Info;
            var caption = info.Column.Caption;
            if (info.Column.Caption == string.Empty)
            {
                caption = info.Column.ToString();
            }
            info.GroupText = $"{caption} : {info.GroupValueText} ({view.GetChildRowCount(e.RowHandle)})";
        }
        /***********************************************************************************************/
        private bool CheckForFutureData(bool allData)
        {
            DateTime date = this.dateTimePicker2.Value;
            date = date.AddMonths(1);
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            string date1 = date.Year.ToString("D4") + "-" + date.Month.ToString("D2") + "-01" + " 00:00:00 ";

            string cmd = "";


            if (SMFS.activeSystem.ToUpper() == "RILES")
                cmd = "Select * from `trust2013r` where `payDate8` >= '" + date1 + "' AND `contractNumber` LIKE 'RF%' ";
            else
                cmd = "Select * from `trust2013r` where `payDate8` >= '" + date1 + "' AND `contractNumber` NOT LIKE 'RF%' ";

            if (!allData)
            {
                if (chk2002.Checked)
                    cmd += " AND `Is2002` = '2002' ";
                else
                    cmd += " AND `Is2002` <> '2002' ";
            }
            cmd += " GROUP by `payDate8` ";
            cmd += ";";
            try
            {
                DataTable dx = G1.get_db_data(cmd);
                if (dx.Rows.Count <= 0)
                    return true;
                int months = dx.Rows.Count;
                DialogResult result = MessageBox.Show("*** WARNING *** Future Months (" + months.ToString() + ") Exist and Will be DESTROYED if you Save this months data!\nDo you want to Continue?", "Save Data Warning Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                if (result == DialogResult.No)
                    return false;
                if (!G1.ValidateOverridePassword())
                {
                    MessageBox.Show("*** INFO *** Data Will Not Be Removed and This Month Will Not Be Saved!", "Save Data Info Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Delete Previous Data " + ex.Message.ToString());
                return false;
            }
            return true;
        }
        /***********************************************************************************************/
        private bool DeletePreviousData(bool allData)
        {
            bool success = true;

            if (!CheckForFutureData(allData))
                return false;

            DateTime date = lastSaveDate;
            date = this.dateTimePicker2.Value;
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            string date1 = date.Year.ToString("D4") + "-" + date.Month.ToString("D2") + "-01" + " 00:00:00 ";
            //            string date1 = date.Year.ToString("D4") + "-" + date.Month.ToString("D2") + "-" + days.ToString("D2") + " 23:59:59 ";

            string cmd = "";


            if (SMFS.activeSystem.ToUpper() == "RILES")
                cmd = "DELETE from `trust2013r` where `payDate8` >= '" + date1 + "' AND `contractNumber` LIKE 'RF%' ";
            else
                cmd = "DELETE from `trust2013r` where `payDate8` >= '" + date1 + "' AND `contractNumber` NOT LIKE 'RF%' ";

            if (!allData)
            {
                if (chk2002.Checked)
                    cmd += " AND `Is2002` = '2002' ";
                else
                    cmd += " AND `Is2002` <> '2002' ";
            }
            cmd += ";";
            try
            {
                G1.get_db_data(cmd);
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Delete Previous Data " + ex.Message.ToString());
            }
            return success;
        }
        /***********************************************************************************************/
        private void btnSaveData_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to save this data to the database?", "Save Trust Data Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;
            DataTable dt = (DataTable)dgv6.DataSource;
            dt = originalDt;

            this.Cursor = Cursors.WaitCursor;

            lblStatus.Show();
            lblStatus.Visible = true;
            lblStatus.Text = "Preparing to Save data!";
            lblStatus.Refresh();
            myStatus.Show();
            myStatus.Visible = true;
            myStatus.Text = "Preparing to Save data!";
            myStatus.Refresh();

            DateTime saveDate = lastSaveDate;

            lastSaveDate = this.dateTimePicker2.Value;

            var date1 = G1.DTtoMySQLDT(lastSaveDate);

            string date2 = lastSaveDate.ToString("yyyy-MM-dd");

            date2 += " 00:00:00";


            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["payDate8"] = date1;

            if (!SaveData(dt, false))
                return;

            lastSaveDate = saveDate;
            this.panel1Top.Hide();
        }
        /***********************************************************************************************/
        private bool SaveData(DataTable dt, bool allData)
        {
            this.Cursor = Cursors.WaitCursor;
            lblStatus.Text = "Removing any necessary previous data saved!";
            lblStatus.Show();
            lblStatus.Refresh();
            myStatus.Text = "Removing any necessary previous data saved!";
            myStatus.Show();
            myStatus.Refresh();

            if (!DeletePreviousData(allData))
            {
                this.Cursor = Cursors.Default;
                return false;
            }

            DataTable saveDt = dt.Copy();
            int locindCol = G1.get_column_number(dt, "locind");
            for (int i = (saveDt.Columns.Count - 1); i > locindCol; i--)
                saveDt.Columns.RemoveAt(i);

            if (G1.get_column_number(saveDt, "num") >= 0)
                saveDt.Columns.Remove("num");
            if (G1.get_column_number(saveDt, "tmstamp") >= 0)
                saveDt.Columns.Remove("tmstamp");
            if (G1.get_column_number(saveDt, "record") >= 0)
                saveDt.Columns.Remove("record");
            DataColumn Col = saveDt.Columns.Add("record", System.Type.GetType("System.String"));
            Col.SetOrdinal(0);// to put the column in position 0;
            DataColumn Col1 = saveDt.Columns.Add("tmstamp", System.Type.GetType("System.String"));
            Col1.SetOrdinal(0);// to put the column in position 0;

            double dValue = 0D;
            string str = "";

            for (int i = 0; i < saveDt.Rows.Count; i++)
            {
                //saveDt.Rows[i]["tmstamp"] = "0000-00-00";
                saveDt.Rows[i]["tmstamp"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                saveDt.Rows[i]["record"] = "0";
                if (String.IsNullOrWhiteSpace(saveDt.Rows[i]["beginningBalance"].ObjToString()))
                    saveDt.Rows[i]["beginningBalance"] = "0";
                saveDt.Rows[i]["firstName"] = G1.try_protect_data(saveDt.Rows[i]["firstName"].ObjToString());
                saveDt.Rows[i]["lastName"] = G1.try_protect_data(saveDt.Rows[i]["lastName"].ObjToString());
                saveDt.Rows[i]["firstName"] = G1.Truncate(saveDt.Rows[i]["firstName"].ObjToString(), 80);
                saveDt.Rows[i]["lastName"] = G1.Truncate(saveDt.Rows[i]["lastName"].ObjToString(), 80);
            }

            DateTime date = lastSaveDate;

            string strFile = "/Trust2013/Trust2013_P_" + date.ToString("yyyyMMdd") + ".csv";
            if (SMFS.activeSystem.ToUpper() == "RILES")
                strFile = "/Trust2013/Trust2013_R_" + date.ToString("yyyyMMdd") + ".csv";
            string Server = "C:/rag";
            //Create directory if not exist... Make sure directory has required rights..
            if (!Directory.Exists(Server + "/Trust2013/"))
                Directory.CreateDirectory(Server + "/Trust2013/");

            //If file does not exist then create it and right data into it..
            if (!File.Exists(Server + strFile))
            {
                FileStream fs = new FileStream(Server + strFile, FileMode.Create, FileAccess.Write);
                fs.Close();
                fs.Dispose();
            }

            //Generate csv file from where data read

            try
            {
                lblStatus.Text = "Creating CSV File to Bulk Load into Database!";
                lblStatus.Refresh();
                myStatus.Text = "Creating CSV File to Bulk Load into Database!";
                myStatus.Refresh();

                DateTime saveDate = this.dateTimePicker2.Value;
                int days = DateTime.DaysInMonth(saveDate.Year, saveDate.Month);
                //                string mySaveDate = saveDate.Year.ToString("D4") + "-" + saveDate.Month.ToString("D2") + "-" + days.ToString("D2") + " 00:00:00";

                var mySaveDate = G1.DTtoMySQLDT(saveDate);

                //for ( int i=0; i<saveDt.Rows.Count; i++)
                //    saveDt.Rows[i]["payDate8"] = mySaveDate;

                MySQL.CreateCSVfile(saveDt, Server + strFile, false, "~");
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Creating CSV File to load into Database " + ex.Message.ToString());
                this.Cursor = Cursors.Default;
                return false;
            }
            try
            {
                lblStatus.Text = "Checking Database Fields!";
                lblStatus.Refresh();
                myStatus.Text = "Checking Database Fields!";
                myStatus.Refresh();
                Structures.TieDbTable("Trust2013r", saveDt);
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Tieing Trust2013r to DataTable " + ex.Message.ToString());
                this.Cursor = Cursors.Default;
                return false;
            }
            try
            {
                lblStatus.Text = "Saving Data NOW!";
                lblStatus.Refresh();
                myStatus.Text = "Saving Data NOW!";
                myStatus.Refresh();
                G1.conn1.Open();
                MySqlBulkLoader bcp1 = new MySqlBulkLoader(G1.conn1);
                bcp1.TableName = "Trust2013r"; //Create ProductOrder table into MYSQL database...
                bcp1.FieldTerminator = ",";

                bcp1.LineTerminator = "\r\n";
                bcp1.FileName = Server + strFile;
                bcp1.NumberOfLinesToSkip = 0;
                bcp1.FieldTerminator = "~";
                bcp1.Load();
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Bulk Loading Trust2013r to DataTable " + ex.Message.ToString());
            }

            saveDt.Dispose();
            saveDt = null;

            File.Delete(Server + strFile);

            lblStatus.Hide();
            myStatus.Hide();
            this.Cursor = Cursors.Default;
            return true;
        }
        /***********************************************************************************************/
        private bool checkForChanged()
        {
            bool rtn = false;
            if (btnSaveData.Visible)
            {
                string str = "It looks like you've made changes to the data!\nDo you want to save your changes now before you move on?";
                DialogResult result = MessageBox.Show(str, "Data Modifled Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    result = MessageBox.Show("Okay! Data will not be saved here.\nYou must press the green SAVE button to save!", "Data Not Save Yet Dialog", MessageBoxButtons.OK, MessageBoxIcon.Question);
                    rtn = true;
                }
            }
            return rtn;
        }
        /***********************************************************************************************/
        bool oldData = false;
        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (checkForChanged())
                return;

            DateTime date = dateTimePicker2.Value;
            workDate2 = date.AddDays(1);

            DialogResult result = MessageBox.Show("Are you sure you want to READ OLD Trust Data for " + date.ToString("MM/dd/yyyy") + "?", "Pull(READ) Trust Report Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;

            testNewDone = false;

            string date2 = G1.DateTimeToSQLDateTime(date);

            DateTime priorYearDate = new DateTime((date.Year - 1), 12, 31);

            int year = date.Year - 1;
            string columnName = year.ToString() + " & Prior Bal PD";
            gridMain6.Columns["beginningBalance"].Caption = columnName;
            gridMain2.Columns["beginningBalance"].Caption = columnName;

            pleaseForm = new PleaseWait();
            pleaseForm.Show();


            dgv6.DataSource = null;
            this.Cursor = Cursors.WaitCursor;
            myStatus.Text = "*** PLEASE WAIT ***";
            myStatus.Refresh();
            lblWait.Text = "*** PLEASE WAIT ***";
            lblWait.Refresh();
            this.Refresh();

            panel1Top.Hide();
            oldData = true;

            DataTable dt = null;

            try
            {
                dt = PullTheData(false);
            }
            catch ( Exception ex)
            {
            }

            // Trust85.FindContract(dt, "FR20011LI");


            this.Refresh();
            this.Cursor = Cursors.WaitCursor;
            //DateTime readDate = this.dateTimePicker2.Value;
            //int days = DateTime.DaysInMonth(readDate.Year, readDate.Month);
            //string date1 = readDate.Year.ToString("D2") + "-" + readDate.Month.ToString("D2") + "-" + days.ToString("D2");

            //string cmd = "Select * from `Trust2013r` where `payDate8` = '" + date1 + "';";
            //DataTable dt = G1.get_db_data(cmd);
            this.Cursor = Cursors.WaitCursor;
            LoadLocations(dt);
            this.Cursor = Cursors.WaitCursor;
            confirmLocations(dt);
            this.Cursor = Cursors.WaitCursor;

            Trust85.FindContract(dt, "C17007U");

            try
            {
                CleanupYearEndRemovals(dt);
            }
            catch ( Exception ex)
            {
            }

            G1.NumberDataTable(dt);
            originalDt = dt;
            this.Cursor = Cursors.WaitCursor;

            //dgv6.DataSource = dt;
            this.Cursor = Cursors.WaitCursor;

            Trust85.FindContract(dt, "RF901343");
            LoadUpLocations();
            gridMain6.Columns["loc"].Visible = true;
            if (chk2002.Checked)
            {
                gridMain6.Columns["interest"].Visible = true;
                gridMain7.Columns["interest"].Visible = false;
                gridMain8.Columns["interest"].Visible = false;
                gridMain8.Columns["currentInterest"].Visible = false;
            }
            else
            {
                gridMain6.Columns["interest"].Visible = true;
                gridMain7.Columns["interest"].Visible = false;
                gridMain8.Columns["interest"].Visible = true;
                gridMain8.Columns["currentInterest"].Visible = true;
            }

            this.panel1Top.Hide();

            //if (chk2002.Checked)
            //    btnVerify.Show();

            FixSpecialLocations(dt);

            LoadPre2002(dt);

            SetupCemeteries(dt, gridMain6);

            dgv6.DataSource = dt;

            trustReportDt = dt;

            myStatus.Text = "";
            myStatus.Refresh();
            lblWait.Text = "";
            lblWait.Refresh();

            this.Cursor = Cursors.Default;
            pleaseForm.FireEvent1();
        }
        /****************************************************************************************/
        public static bool isCemetery(string workContract)
        {
            bool isCemetery = false;
            if (workContract.ToUpper().IndexOf("NNM") == 0 || workContract.ToUpper().IndexOf("HC") == 0 || workContract.ToUpper().IndexOf ( "NM" ) == 0 )
                isCemetery = true;
            return isCemetery;
        }
        /***********************************************************************************************/
        private void SetupCemeteries(DataTable dt, DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView grid)
        {
            if (1 == 1)
                return;
            string contractNumber = "";
            if (G1.get_column_number(dt, "group") < 0)
                dt.Columns.Add("group");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                if (isCemetery(contractNumber))
                    dt.Rows[i]["group"] = "2";
                else
                    dt.Rows[i]["group"] = "1";
            }
            grid.Columns["group"].GroupIndex = 0;
            grid.Columns["locind"].GroupIndex = 1;
        }
        /***********************************************************************************************/
        private void ReadOldData(bool justReporting = false)
        {
            DateTime date = dateTimePicker2.Value;
            workDate2 = date.AddDays(1);

            string date2 = G1.DateTimeToSQLDateTime(date);

            DateTime priorYearDate = new DateTime((date.Year - 1), 12, 31);

            int year = date.Year - 1;
            string columnName = year.ToString() + " & Prior Bal PD";
            gridMain6.Columns["beginningBalance"].Caption = columnName;
            gridMain2.Columns["beginningBalance"].Caption = columnName;

            pleaseForm = new PleaseWait();
            pleaseForm.Show();


            dgv6.DataSource = null;
            this.Cursor = Cursors.WaitCursor;
            myStatus.Text = "*** PLEASE WAIT ***";
            myStatus.Refresh();
            lblWait.Text = "*** PLEASE WAIT ***";
            lblWait.Refresh();
            this.Refresh();

            panel1Top.Hide();
            oldData = true;

            DataTable dt = PullTheData(false);

            this.Refresh();
            this.Cursor = Cursors.WaitCursor;
            //DateTime readDate = this.dateTimePicker2.Value;
            //int days = DateTime.DaysInMonth(readDate.Year, readDate.Month);
            //string date1 = readDate.Year.ToString("D2") + "-" + readDate.Month.ToString("D2") + "-" + days.ToString("D2");

            //string cmd = "Select * from `Trust2013r` where `payDate8` = '" + date1 + "';";
            //DataTable dt = G1.get_db_data(cmd);
            this.Cursor = Cursors.WaitCursor;
            LoadLocations(dt);
            this.Cursor = Cursors.WaitCursor;
            confirmLocations(dt);
            this.Cursor = Cursors.WaitCursor;

            if (!justReporting)
                CleanupYearEndRemovals(dt);

            G1.NumberDataTable(dt);
            originalDt = dt;
            this.Cursor = Cursors.WaitCursor;

            //dgv6.DataSource = dt;
            this.Cursor = Cursors.WaitCursor;

            //Trust85.FindContract(dt, "HC1901");
            LoadUpLocations();
            gridMain6.Columns["loc"].Visible = true;
            if (chk2002.Checked)
            {
                gridMain6.Columns["interest"].Visible = true;
                gridMain7.Columns["interest"].Visible = false;
                gridMain8.Columns["interest"].Visible = false;
                gridMain8.Columns["currentInterest"].Visible = false;
            }
            else
            {
                gridMain6.Columns["interest"].Visible = true;
                gridMain7.Columns["interest"].Visible = false;
                gridMain8.Columns["interest"].Visible = true;
                gridMain8.Columns["currentInterest"].Visible = true;
            }

            this.panel1Top.Hide();

            //if (chk2002.Checked)
            //    btnVerify.Show();

            FixSpecialLocations(dt);

            LoadPre2002(dt);

            dgv6.DataSource = dt;

            trustReportDt = dt;

            myStatus.Text = "";
            myStatus.Refresh();
            lblWait.Text = "";
            lblWait.Refresh();

            this.Cursor = Cursors.Default;
            pleaseForm.FireEvent1();
        }
        /***********************************************************************************************/
        private void LoadPre2002(DataTable dt)
        {
            string cmd = "Select * from `pre2002`;";
            pre2002Dt = G1.get_db_data(cmd);
            if (!chk2002.Checked)
                return;

            DataTable locDt = (DataTable)chkComboLocNames.Properties.DataSource;
            string locind = "";
            string location = "";
            string reportHeading = "";
            pre2002Dt.Rows.Clear();
            DataRow[] dRows = null;
            DataRow dR = null;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                locind = dt.Rows[i]["locind"].ObjToString();
                dRows = pre2002Dt.Select("locind='" + locind + "'");
                if (dRows.Length <= 0)
                {
                    dR = pre2002Dt.NewRow();
                    dR["locind"] = locind;
                    location = dt.Rows[i]["Location Name"].ObjToString();
                    dRows = locDt.Select("locind='" + locind + "'");
                    if (dRows.Length > 0)
                    {
                        reportHeading = dRows[0]["locindHeading"].ObjToString();
                        if (!String.IsNullOrWhiteSpace(reportHeading))
                            location = reportHeading;
                    }
                    dR["name"] = location;
                    pre2002Dt.Rows.Add(dR);
                }

            }
        }
        /***********************************************************************************************/
        private void LoadUpLocations()
        {
            string cmd = "Select * from `trust2013r` ";
            if (chk2002.Checked)
                cmd += " where `Is2002` = '2002' ";
            else
                cmd += " where `Is2002` <> '2002' ";
            cmd += " GROUP by `locind` ORDER BY `locind`";
            cmd += ";";
            DataTable dt = G1.get_db_data(cmd);
            chkComboLocation.Properties.Items.Clear();

            dt.Columns["locind"].ColumnName = "keycode";

            chkComboLocation.Properties.DataSource = dt;
        }
        /***********************************************************************************************/
        private string LookupFile()
        {
            string foundFile = "";
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                string directory = @"C:\rag\Trust2013\";
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
                ofd.InitialDirectory = directory;
                ofd.Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*";
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    foundFile = ofd.FileName;
                }
            }
            return foundFile;
        }
        /***********************************************************************************************/
        private void importOriginalDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable dt = PullAllData(true); // Get Original Data

            dt.Columns.Add("num");
            dt.Columns.Add("fullname");
            dt.Columns.Add("dd");
            dt.Columns.Add("found");
            dt.Columns.Add("loc");
            dt.Columns.Add("Location Name");
            dt.Columns.Add("dbr", Type.GetType("System.Double"));
            dt.Columns.Add("original", Type.GetType("System.Double"));
            dt.Columns.Add("as400", Type.GetType("System.Double"));
            dt.Columns.Add("difference", Type.GetType("System.Double"));
            dt.Columns.Add("ragCurrentMonth", Type.GetType("System.Double"));

            lastSaveDate = DateTime.Now;
            if (dt.Rows.Count > 0)
                lastSaveDate = dt.Rows[0]["payDate8"].ObjToDateTime();
            int count = dt.Rows.Count;

            SaveData(dt, true);
        }
        /***********************************************************************************************/
        private void gridMain7_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            GridView view = sender as GridView;
            if (e.Column.FieldName == "locind")
            {
                int rowHandle = e.GroupRowHandle;
                if (view.IsGroupRow(rowHandle))
                {
                    DataTable dt = (DataTable)dgv7.DataSource;
                    string locind = e.DisplayText;
                    DataRow[] dRows = dt.Select("locind='" + locind + "'");
                    if (isPrinting)
                        e.DisplayText += " (" + dRows.Length.ToString() + ")";
                }
            }
        }
        /***********************************************************************************************/
        private void gridView_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            GridView view = sender as GridView;
            if (e.Column.FieldName == "locind")
            {
                int rowHandle = e.GroupRowHandle;
                if (view.IsGroupRow(rowHandle))
                {
                    DataTable dt = null;
                    if (dgv7.Visible)
                        dt = (DataTable)dgv7.DataSource;
                    else if (dgv8.Visible)
                        dt = (DataTable)dgv8.DataSource;
                    else if (dgv2.Visible)
                        dt = (DataTable)dgv2.DataSource;
                    else if (dgv6.Visible)
                        dt = (DataTable)dgv6.DataSource;
                    if (dt != null)
                    {
                        string locind = e.DisplayText;
                        DataRow[] dRows = dt.Select("locind='" + locind + "'");
                        if (isPrinting)
                            e.DisplayText += " (" + dRows.Length.ToString() + ")";
                    }
                }
            }
        }
        /***********************************************************************************************/
        //private void UpdateTrust2013Database ( DataRow dr)
        //{
        //    string record = dr["record"].ObjToString();
        //    double beginningBalance = dr["beginningBalance"].ObjToDouble();
        //    double deathRemYTDprevious = dr["deathRemYTDprevious"].ObjToDouble();
        //    double deathRemCurrMonth = dr["deathRemCurrMonth"].ObjToDouble();
        //    double refundRemYTDprevious = dr["refundRemYTDprevious"].ObjToDouble();
        //    double refundRemCurrMonth = dr["refundRemCurrMonth"].ObjToDouble();
        //    if (String.IsNullOrWhiteSpace(record))
        //        return;
        //    G1.update_db_table("trust2013r", "record", record, new string[] { "beginningBalance", beginningBalance.ToString(), "deathRemYTDprevious", deathRemYTDprevious.ToString(), "deathRemCurrMonth", deathRemCurrMonth.ToString()});
        //    G1.update_db_table("trust2013r", "record", record, new string[] { "refundRemYTDprevious", refundRemYTDprevious.ToString(), "refundRemCurrMonth", refundRemCurrMonth.ToString() });
        //}
        /***********************************************************************************************/
        private void editContractToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int rowHandle = gridMain6.FocusedRowHandle;
            int row = gridMain6.GetDataSourceRowIndex(rowHandle);

            DataTable dt = (DataTable)dgv6.DataSource;
            DataTable ddx = dt.Clone();
            DataRow[] dRows = dt.Select("location=''");
            G1.ConvertToTable(dRows, ddx);
            DataTable tempDt = dt.Clone();
            G1.copy_dt_row(dt, row, tempDt, tempDt.Rows.Count);
            DataRow dr = tempDt.Rows[0];
            string contractNumber = dr["contractNumber"].ObjToString();

            using (AddNewContract addForm = new AddNewContract(false, this.dateTimePicker2.Value, contractNumber, dr))
            {
                addForm.Done += AddForm_Done;
                addForm.ShowDialog();
            }
        }
        private void AddForm_Done(DataRow workDr)
        {
            if (workDr != null)
            {
                DataTable dt = originalDt;
                string contractNumber = workDr["contractNumber"].ObjToString();

                DataRow[] dRows = dt.Select("contractNumber='" + contractNumber + "'");
                if (dRows.Length > 0)
                {
                    int SelectedIndex = dt.Rows.IndexOf(dRows[0]);
                    DataTable tempDt = dt.Clone();
                    try
                    {
                        tempDt.Rows.Add(workDr.ItemArray);
                    }
                    catch (Exception ex)
                    {

                    }
                    G1.copy_dt_row(tempDt, 0, dt, SelectedIndex);
                    DateTime date = workDr["payDate8"].ObjToDateTime();
                    if (this.dateTimePicker2.Value == date)
                        this.panel1Top.Show();

                    UpdateSMFSRemovals(tempDt);
                }

                chkComboLocation_EditValueChanged(null, null);
                //dgv6.DataSource = dt;
                //dgv6.RefreshDataSource();
                //dgv6.Refresh();
            }
        }
        /***********************************************************************************************/
        private void UpdateSMFSRemovals(DataTable workDt)
        {
            string contractNumber = "";
            double beginningBalance = 0D;
            double ytdPrevious = 0D;
            double paymentCurrMonth = 0D;
            double deathRemYTDprevious = 0D;
            double deathRemCurrMonth = 0D;
            double refundRemYTDprevious = 0D;
            double refundRemCurrMonth = 0D;
            double endingBalance = 0D;
            string cmd = "";
            DataTable dx = null;
            string trustRemoved = "";
            string trustRefunded = "";
            DateTime now = this.dateTimePicker2.Value;
            string date = now.ToString("MM/dd/yyyy");
            string oldDate = now.AddMonths(-1).ToString("MMdd/yyyy");
            string updateDate = "";
            string record = "";
            for (int i = 0; i < workDt.Rows.Count; i++)
            {
                trustRemoved = "";
                trustRefunded = "";
                updateDate = "";
                contractNumber = workDt.Rows[i]["contractNumber"].ObjToString();
                beginningBalance = workDt.Rows[i]["beginningBalance"].ObjToDouble();
                ytdPrevious = workDt.Rows[i]["ytdPrevious"].ObjToDouble();
                paymentCurrMonth = workDt.Rows[i]["paymentCurrMonth"].ObjToDouble();
                deathRemYTDprevious = workDt.Rows[i]["deathRemYTDprevious"].ObjToDouble();
                deathRemCurrMonth = workDt.Rows[i]["deathRemCurrMonth"].ObjToDouble();
                refundRemYTDprevious = workDt.Rows[i]["refundRemYTDprevious"].ObjToDouble();
                refundRemCurrMonth = workDt.Rows[i]["refundRemCurrMonth"].ObjToDouble();
                endingBalance = workDt.Rows[i]["endingBalance"].ObjToDouble();

                cmd = "Select * from `contracts` where `contractNumber` = '" + contractNumber + "';";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    record = dx.Rows[0]["record"].ObjToString();
                    if (deathRemCurrMonth > 0D || deathRemYTDprevious > 0D)
                        trustRemoved = "YES";
                    else if ( refundRemCurrMonth > 0D || refundRemYTDprevious > 0D)
                        trustRefunded = "YES";
                    if (deathRemCurrMonth > 0D || refundRemCurrMonth > 0D)
                        updateDate = date;
                    else if (deathRemYTDprevious > 0D || refundRemYTDprevious > 0D)
                        updateDate = date;
                    if (String.IsNullOrWhiteSpace(trustRemoved) && String.IsNullOrWhiteSpace(trustRefunded))
                        date = DateTime.MinValue.ToString("yyyy-MM-dd");
                    G1.update_db_table("contracts", "record", record, new string[] { "trustRemoved", trustRemoved, "trustRefunded", trustRefunded, "dateRemoved", date });
                }
            }
        }
        /***********************************************************************************************/
        private void addNewContractToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable dt = originalDt;
            DataRow dr = dt.NewRow();
            DateTime date = this.dateTimePicker2.Value;
            string sDate = G1.DTtoMySQLDT(date).ObjToString();
            try
            {
                dr["payDate8"] = G1.DTtoMySQLDT(date);
                if (chk2002.Checked)
                    dr["Is2002"] = "2002";
            }
            catch (Exception ex)
            {

            }
            string contract = "";
            using (AddNewContract addForm = new AddNewContract(true, this.dateTimePicker2.Value, contract, dr))
            {
                addForm.Done += AddForm_Done1;
                addForm.ShowDialog();
            }
        }
        private void AddForm_Done1(DataRow workDr)
        {
            if (workDr != null)
            {
                DataTable dt = originalDt;
                dt.Rows.Add(workDr);

                DataTable tempDt = originalDt.Clone();
                G1.copy_dt_row(dt, dt.Rows.Count - 1, tempDt, 0);
                UpdateSMFSRemovals(tempDt);

                string contractNumber = workDr["contractNumber"].ObjToString();
                int SelectedIndex = dt.Rows.IndexOf(workDr);
                dgv6.DataSource = dt;
                dgv6.RefreshDataSource();
                dgv6.Refresh();
                DateTime date = workDr["payDate8"].ObjToDateTime();
                if (this.dateTimePicker2.Value == date)
                    this.panel1Top.Show();

                string contract = "";
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    contract = gridMain6.GetRowCellValue(i, "contractNumber").ObjToString();
                    if (contract == contractNumber)
                    {
                        SelectedIndex = i;
                        break;
                    }
                }

                gridMain6.FocusedRowHandle = SelectedIndex;
                gridMain6.SelectRow(SelectedIndex);
            }
        }
        /***********************************************************************************************/
        private void btnVerify_Click(object sender, EventArgs e)
        {
            string cmd = "";
            string contract = "";
            DataTable dx = null;
            DataTable dt = originalDt;
            if (G1.get_column_number(dt, "found") < 0)
                dt.Columns.Add("found");

            int lastRow = dt.Rows.Count;
            int badCount = 0;
            lblTotal.Show();
            this.Cursor = Cursors.WaitCursor;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dt.Rows[i]["found"] = "";
                contract = dt.Rows[i]["contractNumber"].ObjToString();
                if (!String.IsNullOrWhiteSpace(contract))
                {
                    cmd = "Select `contractNumber` from `contracts` where `contractNumber` = '" + contract + "';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                        dt.Rows[i]["found"] = "FOUND";
                    else
                        badCount++;
                    dx.Dispose();
                    dx = null;
                }
                lblTotal.Text = (i + 1).ToString() + " of " + lastRow.ToString();
                lblTotal.Refresh();
            }
            gridMain6.Columns["found"].Visible = true;
            this.Cursor = Cursors.Default;
            MessageBox.Show("Bad Count = " + badCount.ToString());
        }
        /***********************************************************************************************/
        private void lockScreenPositionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //if ( dgv6.Visible )
            //{
            //    G1.SaveLocalPreferences(this, gridMain6, LoginForm.username, "TrustReportLayout6");
            //    foundLocalPref6 = true;
            //}
            //else if (dgv2.Visible)
            //{
            //    G1.SaveLocalPreferences(this, gridMain2, LoginForm.username, "TrustReportLayout2");
            //    foundLocalPref2 = true;
            //}
            //else if (dgv7.Visible)
            //{
            //    G1.SaveLocalPreferences(this, gridMain7, LoginForm.username, "TrustReportLayout7");
            //    foundLocalPref7 = true;
            //}
            //else if (dgv8.Visible)
            //{
            //    G1.SaveLocalPreferences(this, gridMain8, LoginForm.username, "TrustReportLayout8");
            //    foundLocalPref8 = true;
            //}
        }
        /***********************************************************************************************/
        private void unLockScreenPositionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //if (dgv6.Visible)
            //{
            //    G1.RemoveLocalPreferences(LoginForm.username, "TrustReportLayout6");
            //    foundLocalPref6 = false;
            //}
            //else if (dgv2.Visible)
            //{
            //    G1.RemoveLocalPreferences(LoginForm.username, "TrustReportLayout2");
            //    foundLocalPref2 = false;
            //}
            //else if (dgv7.Visible)
            //{
            //    G1.RemoveLocalPreferences(LoginForm.username, "TrustReportLayout7");
            //    foundLocalPref7 = false;
            //}
            //else if (dgv8.Visible)
            //{
            //    G1.RemoveLocalPreferences(LoginForm.username, "TrustReportLayout8");
            //    foundLocalPref8 = false;
            //}
        }
        /***********************************************************************************************/
        private void TrustReports_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (checkForChanged())
            {
                e.Cancel = true;
                return;
            }

            //if (foundLocalPref6)
            //    G1.SaveLocalPreferences(this, gridMain6, LoginForm.username, "TrustReportLayout6");
            //if (foundLocalPref2)
            //    G1.SaveLocalPreferences(this, gridMain2, LoginForm.username, "TrustReportLayout2");
            //if (foundLocalPref7)
            //    G1.SaveLocalPreferences(this, gridMain7, LoginForm.username, "TrustReportLayout7");
            //if (foundLocalPref8)
            //    G1.SaveLocalPreferences(this, gridMain8, LoginForm.username, "TrustReportLayout8");
        }
        /***********************************************************************************************/
        private void gridMain8_CustomDrawRowFooterCell(object sender, FooterCellCustomDrawEventArgs e)
        {
            string columnName = e.Column.FieldName.ObjToString().ToUpper();
            if (columnName != "LOCIND")
                return;
            int rowHandle = e.RowHandle;
            int row = gridMain8.GetDataSourceRowIndex(rowHandle);
            DataTable dt = (DataTable)dgv8.DataSource;
            string location = dt.Rows[row]["locind"].ObjToString();
            bandLocation = location;

            int dx = e.Bounds.Height;
            Brush brush = e.Cache.GetGradientBrush(e.Bounds, Color.Wheat, Color.FloralWhite, LinearGradientMode.Vertical);
            Rectangle r = e.Bounds;
            //Draw a 3D border 
            BorderPainter painter = BorderHelper.GetPainter(DevExpress.XtraEditors.Controls.BorderStyles.Style3D);
            AppearanceObject borderAppearance = new AppearanceObject(e.Appearance);
            borderAppearance.BorderColor = Color.DarkGray;
            painter.DrawObject(new BorderObjectInfoArgs(e.Cache, borderAppearance, r));
            //Fill the inner region of the cell 
            r.Inflate(-1, -1);
            e.Cache.FillRectangle(brush, r);
            //Draw a summary value 
            r.Inflate(-2, 0);
            e.Appearance.DrawString(e.Cache, location, r);
            //            e.Appearance.DrawString(e.Cache, e.Info.DisplayText, r);
            //Prevent default drawing of the cell 
            e.Handled = true;
        }
        /***********************************************************************************************/
        private int bandCount = 0;
        private string bandLocation = "";
        private void gridMain2_CustomDrawBandHeader(object sender, DevExpress.XtraGrid.Views.BandedGrid.BandHeaderCustomDrawEventArgs e)
        {
            if (1 == 1)
                return;
            if (e.Band == null) return;
            bandCount++;
            e.Band.Caption = bandLocation;
            //if (e.Info.State != ObjectState.Pressed) return;
            using (Brush brushPressed = new LinearGradientBrush(e.Bounds, Color.WhiteSmoke, Color.Gray, LinearGradientMode.ForwardDiagonal))
            {
                Rectangle r = e.Bounds;
                Draw3DBorder(e.Cache, r);
                r.Inflate(-1, -1);
                //Fill the background 
                e.Cache.FillRectangle(brushPressed, r);

                //Draw a band glyph 
                foreach (DrawElementInfo info in e.Info.InnerElements)
                {
                    if (!info.Visible) continue;
                    GlyphElementInfoArgs glyphInfoArgs = info.ElementInfo as GlyphElementInfoArgs;
                    if (glyphInfoArgs == null) continue;
                    info.ElementInfo.OffsetContent(1, 1);
                    ObjectPainter.DrawObject(e.Cache, info.ElementPainter, info.ElementInfo);
                    info.ElementInfo.OffsetContent(-1, -1);
                    break;
                }

                //Draw the band's caption with a shadowed effect 
                Rectangle textRect = e.Info.CaptionRect;
                textRect.Offset(2, 2);
                e.Appearance.DrawString(e.Cache, e.Info.Caption, textRect, Brushes.White);
                textRect.Offset(-1, -1);
                e.Appearance.DrawString(e.Cache, e.Info.Caption, textRect, Brushes.Black);

                //Prevent default painting 
                e.Handled = true;
            }
        }
        /***********************************************************************************************/
        private void Draw3DBorder(GraphicsCache cache, Rectangle rect)
        {
            //Draw a 3D border 
            BorderPainter painter = BorderHelper.GetPainter(DevExpress.XtraEditors.Controls.BorderStyles.Style3D);
            AppearanceObject borderAppearance = new AppearanceObject();
            borderAppearance.BorderColor = Color.DarkGray;
            painter.DrawObject(new BorderObjectInfoArgs(cache, borderAppearance, rect));
        }
        /***********************************************************************************************/
        private void compareBeginningBalanceToLastMonthEndingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dgv7 == null)
                LoadTabBeginning();
            if (dgv7.DataSource == null)
                LoadTabBeginning();
            if (dgv7.DataSource == null)
                return;

            DateTime date = this.dateTimePicker2.Value;
            date = date.AddMonths(-1);
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            date = new DateTime(date.Year, date.Month, days);
            this.dateTimePicker2.Value = date;

            DataTable dt6 = (DataTable)dgv6.DataSource;
            //DataTable dt2 = (DataTable)dgv2.DataSource;
            DataTable dt7 = (DataTable)dgv7.DataSource;
            //DataTable dt8 = (DataTable)dgv8.DataSource;

            toolStripMenuItem2_Click(null, null);
            LoadTabBeginning();

            DataTable dt = (DataTable)dgv7.DataSource;

            DataRow[] dR = null;
            DataRow dRow = null;

            string contractNumber = "";
            double endingBalance = 0D;
            double beginningBalance = 0D;

            DataTable newDt = new DataTable();
            newDt.Columns.Add("contractNumber");
            newDt.Columns.Add("beginningBalance", Type.GetType("System.Double"));
            newDt.Columns.Add("endingBalance", Type.GetType("System.Double"));

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                dR = dt7.Select("contractNumber='" + contractNumber + "'");
                if (dR.Length <= 0)
                {
                }
                else
                {
                    endingBalance = dt.Rows[i]["endingBalance"].ObjToDouble();
                    endingBalance = G1.RoundValue(endingBalance);
                    beginningBalance = dR[0]["beginningBalance"].ObjToDouble();
                    beginningBalance = G1.RoundValue(beginningBalance);
                    if (beginningBalance != endingBalance)
                    {
                        dRow = newDt.NewRow();
                        dRow["contractNumber"] = contractNumber;
                        dRow["beginningBalance"] = beginningBalance;
                        dRow["endingBalance"] = endingBalance;
                        newDt.Rows.Add(dRow);
                    }
                }
            }
            G1.NumberDataTable(newDt);
            dgv11.DataSource = newDt;
            tabControl1.TabPages.Add(tabCompareBalance);
        }
        /***********************************************************************************************/
        private void chkAlphaMode_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                DataTable dt = (DataTable)dgv6.DataSource;
                if (dt == null)
                    return;

                if (chkAlphaMode.Checked)
                {
                    if (chkShowLocations.Checked)
                        chkShowLocations.Checked = false;
                    DataView tempview = dt.DefaultView;
                    tempview.Sort = "lastName asc, firstName asc";
                    dt = tempview.ToTable();
                    dgv6.DataSource = dt;
                    dgv6.Refresh();

                    gridMain6.Columns["locind"].GroupIndex = -1;
                    gridMain6.Columns["num"].Visible = true;
                    chkExpand.Checked = true;
                    chkExpand.Hide();
                }
                else
                {
                    if (!chkShowLocations.Checked)
                        chkShowLocations.Checked = true;

                    DataView tempview = dt.DefaultView;
                    tempview.Sort = "location asc, contractNumber asc";
                    dt = tempview.ToTable();
                    dgv6.DataSource = dt;
                    dgv6.Refresh();

                    gridMain6.Columns["locind"].GroupIndex = 0;
                    gridMain6.Columns["num"].Visible = false;
                    chkExpand.Show();
                }
            }
            catch (Exception ex)
            {
            }
        }
        /***********************************************************************************************/
        private bool LoadReportLocations(string locations)
        {
            chkComboLocation.CheckAll();

            string editValue = "";

            string[] Lines = locations.Split(',');
            string location = "";
            string loc = "";
            bool found = false;

            int count = -1;
            foreach (var item in chkComboLocation.Properties.Items.GetCheckedValues())
            {
                count++;
                chkComboLocation.Properties.Items[count].CheckState = CheckState.Unchecked;
                location = item.ObjToString().Trim();
                for (int k = 0; k < Lines.Length; k++)
                {
                    loc = Lines[k].Trim();
                    if (loc == location)
                    {
                        //editValue += loc + ",";
                        chkComboLocation.Properties.Items[count].CheckState = CheckState.Checked;
                        found = true;
                        break;
                    }
                }
            }
            //chkComboLocation.EditValue = editValue;
            chkComboLocation.Refresh();
            return found;
        }
        /***********************************************************************************************/
        private string fullPath = "";
        private string format = "";
        private bool continuousPrint = false;
        private void paymentsPlacedInTrustToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string mainReport = "";
            string report = "";
            string locations = "";
            format = "";
            string outputFilname = "";
            string outputDirectory = "";

            fullPath = "";

            string location = "";
            string loc = "";
            bool foundLocations = false;

            string lastReadOldData = "";
            string newReadOldData = "";
            string saveActive = SMFS.activeSystem;

            DataTable dt = null;

            string[] Lines = null;

            DateTime date = dateTimePicker2.Value;

            string yyyy = date.Year.ToString("D4");
            string month = G1.ToMonthName(date);

            DialogResult result = MessageBox.Show("Are you sure you want to RUN the Mass Reports for Payments Placed In Trust for " + date.ToString("MM/dd/yyyy") + "?", "Mass Report Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;
            string cmd = "Select * from `mass_reports` where `mainReport` = 'Payments Placed In Trust';";
            DataTable dx = G1.get_db_data(cmd);

            if (massReportsDt != null)
                dx = massReportsDt.Copy();

            int lastRow = dx.Rows.Count;
            //lastRow = 6;

            barImport.Minimum = 0;
            barImport.Maximum = lastRow;
            barImport.Show();

            bool gotRiles = false;
            bool lastRiles = false;

            for (int i = 0; i < lastRow; i++)
            {
                try
                {
                    Application.DoEvents();

                    barImport.Value = i + 1;
                    barImport.Refresh();

                    mainReport = dx.Rows[i]["mainReport"].ObjToString();
                    report = dx.Rows[i]["report"].ObjToString();
                    locations = dx.Rows[i]["locations"].ObjToString();
                    format = dx.Rows[i]["format"].ObjToString();
                    outputFilname = dx.Rows[i]["outputFilename"].ObjToString();
                    outputDirectory = dx.Rows[i]["outputDirectory"].ObjToString();

                    outputDirectory = outputDirectory.Replace("2021", yyyy);
                    outputFilname = outputFilname.Replace("yyyy", yyyy);
                    outputFilname = outputFilname.Replace("month", month);

                    fullPath = outputDirectory + "/" + outputFilname;

                    G1.verify_path(outputDirectory);

                    this.Text = mainReport + " " + report + " / " + locations;

                    SMFS.activeSystem = "";
                    gotRiles = false;


                    chk2002.Checked = false;
                    if (report.ToUpper().IndexOf("POST2002") >= 0)
                        chk2002.Checked = true;
                    else if (report.ToUpper().IndexOf("RILES") >= 0)
                    {
                        SMFS.activeSystem = "RILES";
                        chk2002.Checked = true;
                        gotRiles = true;
                    }

                    newReadOldData = chk2002.Checked.ToString();

                    for (int j = 0; j < chkComboLocation.Properties.Items.Count; j++)
                        chkComboLocation.Properties.Items[j].CheckState = CheckState.Unchecked;

                    if (newReadOldData != lastReadOldData || gotRiles || lastRiles)
                        ReadOldData(true);

                    lastRiles = gotRiles;

                    lastReadOldData = newReadOldData;

                    this.Cursor = Cursors.WaitCursor;

                    dt = (DataTable)dgv6.DataSource;
                    if (dt == null)
                        break;
                    if (dt.Rows.Count <= 0)
                        continue;

                    if (report.IndexOf("by location") >= 0)
                        chkAlphaMode.Checked = false;
                    else
                        chkAlphaMode.Checked = true;

                    foundLocations = LoadReportLocations(locations);
                    if (!foundLocations)
                    {
                        MessageBox.Show("***ERROR NOT FOUND*** Locations (" + locations + ") for Payments Placed in Trust!", "Mass Report ERROR Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        continue;
                    }

                    if (mainReport == "Payments Placed In Trust")
                    {
                        this.tabControl1.SelectedTab = this.tabPlacedInTrust;
                        continuousPrint = true;
                        printPreviewToolStripMenuItem_Click(null, null);
                        continuousPrint = false;
                        //if (format.ToUpper().IndexOf(".PDF") >= 0)
                        //    gridMain2.ExportToPdf ( fullPath );
                        //else if (format.ToUpper().IndexOf(".CSV") >= 0)
                        //    gridMain2.ExportToCsv(fullPath);
                        //G1.sleep(1000);
                    }

                    this.tabControl1.SelectedTab = this.tabAllData;

                    this.Cursor = Cursors.Default;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("***ERROR*** " + ex.Message.ToString());
                }
            }
            this.tabControl1.SelectedTab = this.tabAllData;
            this.Text = "Trust Reports";
            originalDt.Rows.Clear();
            dgv6.DataSource = originalDt;
            dgv6.RefreshDataSource();
            dgv6.Refresh();

            barImport.Value = lastRow;
            barImport.Refresh();

            MessageBox.Show("Mass Reports Finished for Payments Placed in Trust!", "Mass Report Finished Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            barImport.Hide();

            SMFS.activeSystem = saveActive;
        }
        /***********************************************************************************************/
        private void paymentsRemovedFromTrustToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string mainReport = "";
            string report = "";
            string locations = "";
            format = "";
            string outputFilname = "";
            string outputDirectory = "";

            fullPath = "";

            string location = "";
            string loc = "";

            string lastReadOldData = "";
            string newReadOldData = "";
            bool foundLocations = false;
            string saveActive = SMFS.activeSystem;

            DataTable dt = null;

            string[] Lines = null;

            DateTime date = dateTimePicker2.Value;

            string yyyy = date.Year.ToString("D4");
            string month = G1.ToMonthName(date);

            DialogResult result = MessageBox.Show("Are you sure you want to RUN the Mass Reports for Payments Removed From Trust for " + date.ToString("MM/dd/yyyy") + "?", "Mass Report Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;
            string cmd = "Select * from `mass_reports` where `mainReport` = 'Payments Removed From Trust';";
            DataTable dx = G1.get_db_data(cmd);

            if (massReportsDt != null)
                dx = massReportsDt.Copy();

            int lastRow = dx.Rows.Count;

            barImport.Minimum = 0;
            barImport.Maximum = lastRow;
            barImport.Show();

            for (int i = 0; i < lastRow; i++)
            {
                try
                {
                    Application.DoEvents();

                    barImport.Value = i + 1;
                    barImport.Refresh();

                    mainReport = dx.Rows[i]["mainReport"].ObjToString();
                    report = dx.Rows[i]["report"].ObjToString();

                    //if (report.ToUpper().IndexOf("RILES") < 0)
                    //    continue;

                    locations = dx.Rows[i]["locations"].ObjToString();
                    format = dx.Rows[i]["format"].ObjToString();
                    outputFilname = dx.Rows[i]["outputFilename"].ObjToString();
                    outputDirectory = dx.Rows[i]["outputDirectory"].ObjToString();

                    outputDirectory = outputDirectory.Replace("2021", yyyy);
                    outputFilname = outputFilname.Replace("yyyy", yyyy);
                    outputFilname = outputFilname.Replace("month", month);

                    fullPath = outputDirectory + "/" + outputFilname;

                    G1.verify_path(outputDirectory);

                    this.Text = mainReport + " " + report + " / " + locations;

                    SMFS.activeSystem = "";

                    chk2002.Checked = false;
                    if (report.ToUpper().IndexOf("POST2002") >= 0)
                        chk2002.Checked = true;
                    else if (report.ToUpper().IndexOf("RILES") >= 0)
                    {
                        SMFS.activeSystem = "RILES";
                        chk2002.Checked = true;
                        lastReadOldData = "";
                    }

                    newReadOldData = chk2002.Checked.ToString();

                    for (int j = 0; j < chkComboLocation.Properties.Items.Count; j++)
                        chkComboLocation.Properties.Items[j].CheckState = CheckState.Unchecked;

                    if (newReadOldData != lastReadOldData)
                        ReadOldData(true);

                    lastReadOldData = newReadOldData;

                    this.Cursor = Cursors.WaitCursor;

                    dt = (DataTable)dgv6.DataSource;
                    if (dt == null)
                        break;
                    if (dt.Rows.Count <= 0)
                        continue;

                    if (report.IndexOf("by location") >= 0)
                        chkAlphaMode.Checked = false;
                    else
                        chkAlphaMode.Checked = true;

                    foundLocations = LoadReportLocations(locations);
                    if (!foundLocations)
                    {
                        MessageBox.Show("***ERROR NOT FOUND*** Locations (" + locations + ") for Payments Removed From Trust!", "Mass Report ERROR Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        continue;
                    }

                    if (mainReport == "Payments Removed From Trust")
                    {
                        this.tabControl1.SelectedTab = this.tabRemoved;
                        continuousPrint = true;
                        printPreviewToolStripMenuItem_Click(null, null);
                        continuousPrint = false;
                    }

                    this.tabControl1.SelectedTab = this.tabAllData;

                    this.Cursor = Cursors.Default;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("***ERROR*** " + ex.Message.ToString());
                }
            }
            this.tabControl1.SelectedTab = this.tabAllData;
            this.Text = "Trust Reports";
            originalDt.Rows.Clear();
            dgv6.DataSource = originalDt;
            dgv6.RefreshDataSource();
            dgv6.Refresh();

            barImport.Value = lastRow;
            barImport.Refresh();

            MessageBox.Show("Mass Reports Finished for Payments Removed From Trust!", "Mass Report Finished Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

            barImport.Hide();

            SMFS.activeSystem = saveActive;
        }
        /***********************************************************************************************/
        private void trustBeginningBalanceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string mainReport = "";
            string report = "";
            string locations = "";
            format = "";
            string outputFilname = "";
            string outputDirectory = "";

            fullPath = "";

            string location = "";
            string loc = "";

            string lastReadOldData = "";
            string newReadOldData = "";
            bool foundLocations = false;
            string saveActive = SMFS.activeSystem;

            DataTable dt = null;

            string[] Lines = null;

            DateTime date = dateTimePicker2.Value;

            string yyyy = date.Year.ToString("D4");
            string month = G1.ToMonthName(date);

            DialogResult result = MessageBox.Show("Are you sure you want to RUN the Mass Reports for Trust Beginning Balance for " + date.ToString("MM/dd/yyyy") + "?", "Mass Report Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;
            string cmd = "Select * from `mass_reports` where `mainReport` = 'Trust Beginning Balance';";
            DataTable dx = G1.get_db_data(cmd);

            if (massReportsDt != null)
                dx = massReportsDt.Copy();

            int lastRow = dx.Rows.Count;

            barImport.Minimum = 0;
            barImport.Maximum = lastRow;
            barImport.Show();

            for (int i = 0; i < lastRow; i++)
            {
                try
                {
                    Application.DoEvents();

                    barImport.Value = i + 1;
                    barImport.Refresh();

                    mainReport = dx.Rows[i]["mainReport"].ObjToString();
                    report = dx.Rows[i]["report"].ObjToString();
                    locations = dx.Rows[i]["locations"].ObjToString();
                    format = dx.Rows[i]["format"].ObjToString();
                    outputFilname = dx.Rows[i]["outputFilename"].ObjToString();
                    outputDirectory = dx.Rows[i]["outputDirectory"].ObjToString();

                    outputDirectory = outputDirectory.Replace("2021", yyyy);
                    outputFilname = outputFilname.Replace("yyyy", yyyy);
                    outputFilname = outputFilname.Replace("month", month);

                    fullPath = outputDirectory + "/" + outputFilname;

                    G1.verify_path(outputDirectory);

                    this.Text = mainReport + " " + report + " / " + locations;

                    SMFS.activeSystem = "";

                    chk2002.Checked = false;
                    if (report.ToUpper().IndexOf("POST2002") >= 0)
                        chk2002.Checked = true;
                    else if (report.ToUpper().IndexOf("RILES") >= 0)
                    {
                        SMFS.activeSystem = "RILES";
                        chk2002.Checked = true;
                        lastReadOldData = "";
                    }

                    newReadOldData = chk2002.Checked.ToString();

                    for (int j = 0; j < chkComboLocation.Properties.Items.Count; j++)
                        chkComboLocation.Properties.Items[j].CheckState = CheckState.Unchecked;

                    if (newReadOldData != lastReadOldData)
                        ReadOldData(true);

                    lastReadOldData = newReadOldData;

                    this.Cursor = Cursors.WaitCursor;

                    dt = (DataTable)dgv6.DataSource;
                    if (dt == null)
                        break;
                    if (dt.Rows.Count <= 0)
                        continue;

                    //if (report.ToUpper().IndexOf("RILES") < 0)
                    //    continue;

                    if (report.IndexOf("by location") >= 0)
                        chkAlphaMode.Checked = false;
                    else
                        chkAlphaMode.Checked = true;

                    foundLocations = LoadReportLocations(locations);
                    if (!foundLocations)
                    {
                        MessageBox.Show("***ERROR NOT FOUND*** Locations (" + locations + ") for Trust Beginning Balance!", "Mass Report ERROR Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        continue;
                    }

                    if (mainReport == "Trust Beginning Balance")
                    {
                        this.tabControl1.SelectedTab = this.tabBeginning;
                        continuousPrint = true;
                        printPreviewToolStripMenuItem_Click(null, null);
                        continuousPrint = false;
                    }
                    if (report.ToUpper().IndexOf("RILES") >= 0)
                        lastReadOldData = "";

                    this.tabControl1.SelectedTab = this.tabAllData;

                    this.Cursor = Cursors.Default;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("***ERROR*** " + ex.Message.ToString());
                }
            }
            this.tabControl1.SelectedTab = this.tabAllData;
            this.Text = "Trust Reports";
            originalDt.Rows.Clear();
            dgv6.DataSource = originalDt;
            dgv6.RefreshDataSource();
            dgv6.Refresh();

            barImport.Value = lastRow;
            barImport.Refresh();

            MessageBox.Show("Mass Reports Finished for Trust Beginning Balance!", "Mass Report Finished Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

            barImport.Hide();

            SMFS.activeSystem = saveActive;
        }
        /***********************************************************************************************/
        private void editMassReportsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditMassReports editForm = new EditMassReports();
            editForm.SelectDone += EditForm_SelectDone;
            editForm.Show();
        }
        /***********************************************************************************************/
        private DataTable massReportsDt = null;
        private void EditForm_SelectDone(DataTable dt)
        {
            massReportsDt = null;
            if (dt == null)
                return;
            DataRow[] dRows = dt.Select("select='1'");
            if (dRows.Length > 0)
                massReportsDt = dRows.CopyToDataTable();
        }
        /***********************************************************************************************/
        private void CalculateCustomCounts()
        {
            DataTable dt = (DataTable)dgv6.DataSource;
            if (testLastRow != dt.Rows.Count)
            {
                testNewDone = false;
                testMainDone = false;
            }
            if (testNewDone && testMainDone)
                return;

            try
            {
                testLastRow = dt.Rows.Count;

                DataTable dx = dt.Copy();
                if (G1.get_column_number(dx, "TESTDATE") < 0)
                {
                    dx.Columns.Add("TESTDATE");
                    for (int i = 0; i < dx.Rows.Count; i++)
                        dx.Rows[i]["TESTDATE"] = dx.Rows[i]["issueDate8"].ObjToDateTime().ToString("yyyyMMdd");
                }

                DateTime date = this.dateTimePicker2.Value;
                DateTime testDate = new DateTime(date.Year, 1, 1);
                string str = testDate.ToString("yyyyMMdd");
                //DataRow[] dRows = dx.Select("TESTDATE >='" + str + "'");
                DataRow[] dRows = dx.Select("beginningBalance='0'");
                testNewCount = dRows.Length;

                dRows = dx.Select("TESTDATE <'" + str + "'");
                dRows = dx.Select("beginningBalance>'0'");
                testMainCount = dRows.Length;

                dRows = dx.Select("currentRemovals>'0'");
                testRemovalsCount = dRows.Length;

                testTotalsCount = testMainCount + testNewCount - testRemovalsCount;

                dx.Rows.Clear();
                dx.Dispose();
                dx = null;

                testNewDone = true;
                testMainDone = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("*** ERROR *** Major Problem Calculating Totals " + ex.Message.ToString() + " !", "Totals Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            }
        }
        /***********************************************************************************************/
        private bool testNewDone = false;
        private int testNewCount = 0;
        private bool testMainDone = false;
        private int testMainCount = 0;
        private int testRemovalsCount = 0;
        private int testTotalsCount = 0;
        private int testLastRow = 0;
        /***********************************************************************************************/
        private void gridMain6_CustomSummaryCalculate(object sender, DevExpress.Data.CustomSummaryEventArgs e)
        {
            DataTable dt = (DataTable)dgv6.DataSource;
            DataRow[] dRows = null;
            string field = (e.Item as GridSummaryItem).FieldName.ObjToString();
            if (field.ToUpper() == "FIRSTNAME")
            {
                CalculateCustomCounts();
                e.TotalValue = testNewCount;
            }
            else if (field.ToUpper() == "ADDRESS1")
            {
                CalculateCustomCounts();
                e.TotalValue = testRemovalsCount.ToString();
            }
            else if (field.ToUpper() == "LASTNAME")
            {
                CalculateCustomCounts();
                e.TotalValue = testMainCount;
            }
            else if (field.ToUpper() == "CITY")
            {
                CalculateCustomCounts();
                e.TotalValue = testTotalsCount;
            }
        }
        /***********************************************************************************************/
        private void gridMain7_CustomSummaryCalculate(object sender, DevExpress.Data.CustomSummaryEventArgs e)
        {
            DataTable dt = (DataTable)dgv6.DataSource;
            DataRow[] dRows = null;
            string field = (e.Item as GridSummaryItem).FieldName.ObjToString();
            if (field.ToUpper() == "FIRSTNAME")
            {
                CalculateCustomCounts();
                e.TotalValue = testNewCount;
            }
            else if (field.ToUpper() == "ADDRESS1")
            {
                CalculateCustomCounts();
                e.TotalValue = testRemovalsCount.ToString();
            }
            else if (field.ToUpper() == "LASTNAME")
            {
                CalculateCustomCounts();
                e.TotalValue = testMainCount;
            }
            else if (field.ToUpper() == "CITY")
            {
                CalculateCustomCounts();
                e.TotalValue = testTotalsCount;
            }
        }
        /***********************************************************************************************/
        private void runTrustSummaryReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TrustSummary2013 trustForm = new TrustSummary2013();
            trustForm.Show();
        }
        /***********************************************************************************************/
        private void importNewRilesFromCliffToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Import importForm = new Import("Import Riles Data to Reprocess");
            importForm.SelectDone += ImportForm_SelectDone;
            importForm.Show();
        }
        /***********************************************************************************************/
        private void ImportForm_SelectDone(DataTable dt)
        {
            if (dt == null)
                return;
            if (dt.Rows.Count <= 0)
                return;
            ReImportRiles rilesForm = new ReImportRiles(dt);
            rilesForm.Show();
        }
        /***********************************************************************************************/
        private void goToFuneralPaymentsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv8.DataSource;
            if (dt == null)
                return;
            if (dt.Rows.Count <= 0)
                return;

            DataRow dr = gridMain8.GetFocusedDataRow();

            string contractNumber = dr["contractNumber"].ObjToString();

            string cmd = "Select * from `cust_payments` WHERE `trust_policy` = '" + contractNumber + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
            {
                MessageBox.Show("***ERROR***\nCannot Locate Payment for " + contractNumber + "!", "Go To Funeral Payments", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }

            contractNumber = dx.Rows[0]["contractNumber"].ObjToString();

            this.Cursor = Cursors.WaitCursor;
            using (FunPayments editFunPayments = new FunPayments(null, contractNumber, "", false, false))
            {
                //editFunPayments.TopMost = true;
                editFunPayments.ShowDialog();
            }
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
    }
}