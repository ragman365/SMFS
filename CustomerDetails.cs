using System;
using System.Data;
using System.Windows.Forms;
using GeneralLib;
using MySql.Data.MySqlClient;
using DevExpress.Utils;
using DevExpress.XtraGrid.Views.Grid;
using System.Drawing;
using DevExpress.XtraGrid.Columns;
using MySql.Data.Types;

using DevExpress.XtraPrinting;
using System.Collections.Generic;
using System.Text;
using System.IO;
using DevExpress.XtraTab;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.Utils.Drawing;
using DevExpress.XtraGrid;
using DevExpress.Data;
using System.Linq;
using DevExpress.XtraCharts.UI;
using DevExpress.XtraRichEdit.API.Native;
using System.Windows.Forms.DataVisualization.Charting;
using DevExpress.XtraCharts.Designer;
using sun.security.jca;
using DevExpress.XtraRichEdit.Import.Rtf;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class CustomerDetails : DevExpress.XtraEditors.XtraForm
    {
        // !customer_record
        private string workRecord = "";
        private string workContract = "";
        private string workPayer = "";
        private bool workPolicy = false;
        private string workPolicyRecord = "";
        private DataTable policyDt = null;
        private bool workAdding = false;
        private bool foundLocalPreference = false;
        private bool saveServices = false;
        private bool loading = false;
        private bool relativesChanged = false;
        private string contractsFile = "contracts";
        private string customersFile = "customers";
        private string paymentsFile = "payments";
        private DateTime policyDueDate8 = DateTime.Now;

        private string txtFirstName;
        private string txtLastName;
        private string txtMiddleName;
        private string txtPrefix;
        private string txtSuffix;
        private int saveRow = -1;
        private bool policiesModified = false;

        private bool workPDF = false;
        private string workPDFfile = "";
        private string workWhat = "";


        //public object MySQLDatetime { get; private set; }
        //public object MySQLFormatDate { get; private set; }

        /***********************************************************************************************/
        public CustomerDetails(string contract)
        {
            InitializeComponent();
            workContract = contract;
        }
        /***********************************************************************************************/
        public CustomerDetails(string record, bool adding = false)
        {
            InitializeComponent();
            workAdding = adding;
            workRecord = record;
            saveServices = false;
            btnSaveServices.Hide();
            btnSave.Hide();
        }
        /***********************************************************************************************/
        public CustomerDetails(string contract, string policyRecord)
        {
            InitializeComponent();
            workContract = contract;
            workPolicyRecord = policyRecord;
            workPolicy = true;
            contractsFile = "icontracts";
            customersFile = "icustomers";
            paymentsFile = "ipayments";
            saveServices = false;
            btnSaveServices.Hide();
            btnSave.Hide();
        }
        /***********************************************************************************************/
        public CustomerDetails(string payer, string pdfFileName, bool generatePDF )
        {
            InitializeComponent();
            workPayer = payer;
            workPolicy = true;
            contractsFile = "icontracts";
            customersFile = "icustomers";
            paymentsFile = "ipayments";

            workPDFfile = pdfFileName;
            workPDF = generatePDF;
            workWhat = "Policies";

            //SetupTotalsSummary();

            ClientDetails_Load(null, null);
            this.Close();
        }
        /***********************************************************************************************/
        public static string BuildClientTitle(DataRow dRow)
        {
            string fname = dRow["firstName"].ObjToString();
            string lname = dRow["lastName"].ObjToString();
            string mname = dRow["middleName"].ObjToString();
            string prefix = dRow["prefix"].ObjToString();
            string suffix = dRow["suffix"].ObjToString();
            string contractNumber = dRow["contractNumber"].ObjToString();
            string payer = dRow["payer"].ObjToString();
            string str = "( " + contractNumber + " ) ";
            if (!String.IsNullOrWhiteSpace(payer))
                str = "( " + contractNumber + "-" + payer + " ) ";
            if (!String.IsNullOrWhiteSpace(prefix))
                str += prefix + " ";
            if (!String.IsNullOrWhiteSpace(fname))
                str += fname + " ";
            if (!String.IsNullOrWhiteSpace(mname))
                str += mname + " ";
            if (!String.IsNullOrWhiteSpace(lname))
                str += lname + " ";
            if (!String.IsNullOrWhiteSpace(suffix))
                str += suffix + " ";
            try
            {
                string serviceId = dRow["ServiceId"].ObjToString();
                if (!String.IsNullOrWhiteSpace(serviceId))
                    str += "  SERVICE ID : " + serviceId;
            }
            catch
            { }
            return str;
        }
        /***********************************************************************************************/
        private string BuildClientTitle()
        {
            string fname = txtFirstName;
            string lname = txtLastName;
            string mname = txtMiddleName;
            string prefix = txtPrefix;
            string suffix = txtSuffix;
            string clientNumber = txtClientNumber.Text;
            clientNumber = workContract;
            string str = "( " + clientNumber + " ) ";
            if (!String.IsNullOrWhiteSpace(prefix))
                str += prefix + " ";
            if (!String.IsNullOrWhiteSpace(fname))
                str += fname + " ";
            if (!String.IsNullOrWhiteSpace(mname))
                str += mname + " ";
            if (!String.IsNullOrWhiteSpace(lname))
                str += lname + " ";
            if (!String.IsNullOrWhiteSpace(suffix))
                str += suffix + " ";

            if (!String.IsNullOrWhiteSpace(lblPayer.Text))
                str += " (" + lblPayer.Text + ")";
            return str;
        }
        /***********************************************************************************************/
        private bool majorError = false;
        private void ClientDetails_Load(object sender, EventArgs e)
        {
            if ( workWhat == "Policies")
            {
                LoadPolicies ( DateTime.Now );

                printPreviewToolStripMenuItem_Click(null, null);
                return;
            }
            policiesModified = false;
            btnSavePolicies.Hide();
            DateTime now = DateTime.Now;
            ShowTiedCustomer();
            string skinName = "";
            foundLocalPreference = G1.RestoreGridLayout(this, this.dgv, gridMain, LoginForm.username, "ClientDetailLayout", ref skinName);
            if (!String.IsNullOrWhiteSpace(skinName))
            {
                if (skinName != "DevExpress Style")
                    DailyForm_SkinChange(skinName);
            }

            SetupToolTips();
            if (workContract.ToUpper().IndexOf("ZZ") == 0)
                workPolicy = true;
            else if (workContract.ToUpper().IndexOf("MM") == 0)
                workPolicy = true;
            else if (workContract.ToUpper().IndexOf("OO") == 0)
                workPolicy = true;
            if (workPolicy)
            {
                customersFile = "icustomers";
                contractsFile = "icontracts";
                paymentsFile = "ipayments";
                string cmd = "Select * from `policies` where `record` = '" + workPolicyRecord + "';";
                policyDt = G1.get_db_data(cmd);
            }
            if (workAdding)
                LoadForAdding();
            else
                LoadData();
            if (majorError)
                return;
            if (!String.IsNullOrWhiteSpace(workRecord))
            {
                string cmd = "Select * from `" + customersFile + "` where `record` = '" + workRecord + "';";
                DataTable dt = G1.get_db_data(cmd);
                if (dt.Rows.Count > 0)
                {
                    workContract = dt.Rows[0]["contractNumber"].ObjToString();
                    string str = BuildClientTitle();
                    this.Text = str;
                }
                //LoadServices();
            }
            AddSummaryColumn("data");
            btnSave.Hide();
            DateTime loadNow = DateTime.Now;
            TimeSpan ts = loadNow - now;

            bool gotContract = CheckForContract();
            if ( gotContract )
                tabNotices.Text = "Notices/Goods and Services";
            if (!LoginForm.administrator)
                btnFuneral.Hide();

            if (G1.isField() )
            {
                xtraTabControl2.TabPages.Remove(tabACH);
                xtraTabControl1.TabPages.Remove(tabRequests);
                xtraTabControl1.TabPages.Remove(tabDailyHistory);
                xtraTabControl1.TabPages.Remove(tabContract);
                xtraTabControl2.TabPages.Remove(tabTrusts);

                if ( !gotContract )
                    xtraTabControl1.TabPages.Remove(tabNotices);

                miscToolStripMenuItem.Visible = false;
                btnTie.Hide();
            }
            else
            {
                if (!LoginForm.administrator && !G1.isHR() )
                    xtraTabControl1.TabPages.Remove(tabContract);
                string preference = G1.getPreference(LoginForm.username, "DailyHistory", "View Daily History");
                if (G1.RobbyServer)
                    preference = "YES";
                if (preference != "YES")
                    xtraTabControl1.TabPages.Remove(tabDailyHistory);

                preference = G1.getPreference(LoginForm.username, "DailyHistory", "Add Manual Payment");
                if (G1.RobbyServer)
                    preference = "YES";
                if (preference != "YES")
                {
                    xtraTabControl2.TabPages.Remove(tabACH);
                    xtraTabControl1.TabPages.Remove(tabNotices);
                    xtraTabControl1.TabPages.Remove(tabRequests);
                }
            }
            CheckForAgreement(workContract);
        }
        /****************************************************************************************/
        private void CheckForAgreement(string contractNumber)
        {
            string cmd = "Select * from `pdfimages` where `contractNumber` = '" + contractNumber + "';";
            DataTable picDt = G1.get_db_data(cmd);
            if (picDt.Rows.Count > 0)
            {
                this.picAgreement.Tag = picDt.Rows[0]["record"].ObjToString();
                this.picAgreement.Show();
            }
            else
            {
                this.picAgreement.Tag = "";
                this.picAgreement.Hide();
            }
        }
        /***********************************************************************************************/
        private void UpdateHeaderInfo()
        {
            string cmd = "Select * from `" + customersFile + "` where `record` = '" + workRecord + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return;

            dt.Columns.Add("ssno");

            DateTime ddate = DateTime.Now;

            string contractNumber = dt.Rows[0]["contractNumber"].ObjToString();

            txtFirstName = dt.Rows[0]["firstName"].ObjToString();
            txtLastName = dt.Rows[0]["lastName"].ObjToString();
            txtMiddleName = dt.Rows[0]["middleName"].ObjToString();
            txtPrefix = dt.Rows[0]["prefix"].ObjToString();
            txtSuffix = dt.Rows[0]["suffix"].ObjToString();
            //txtClientNumber.Text = workRecord.ToString();
            txtClientNumber.Text = workContract;
            //FormatSSN(dt, "ssn", "ssno");
            //FixDates(dt, "birthDate", "bDate");

            string agentCode = dt.Rows[0]["agentCode"].ObjToString();
            string name = GetAgentName(agentCode);

            txtAgentCode.Text = agentCode;
            txtAgentName.Text = name;

            string meetingNumber = dt.Rows[0]["meetingNumber"].ObjToString();
            txtMeetingNumber.Text = meetingNumber;

            string bdate = dt.Rows[0]["birthDate"].ObjToString();
            //if (workPolicy)
            //{
            //    DateTime bbDate = policyDt.Rows[0]["birthDate"].ObjToDateTime();
            //    if (bbDate.Year > 1850)
            //        bdate = bbDate.ToString("MM/dd/yyyy");
            //}
            string age = G1.CalcAge(bdate);
            txtAge.Text = age;

            lblName.Text = "Name: " + txtFirstName + " " + txtLastName;

            FormatSSN(dt, "ssn", "ssno");
            lblSSN.Text = "SSN: " + dt.Rows[0]["ssno"].ObjToString();

            //txtSSN.Text = dt.Rows[0]["ssno"].ObjToString();
            if (DailyHistory.isInsurance(workContract))
            {
                lblPayer.Text = "Payer: " + dt.Rows[0]["payer"].ObjToString();
                workPayer = dt.Rows[0]["payer"].ObjToString();
                LoadBurialAssociation(workPayer);
            }
            else
            {
                lblPayer.Text = "";
                lblBurialAss.Text = "";
            }

            ddate = dt.Rows[0]["birthDate"].ObjToDateTime();
            //if (workPolicy)
            //    ddate = policyDt.Rows[0]["birthDate"].ObjToDateTime();
            if (ddate.Year > 1875)
                txtDOB.Text = ddate.ToString("MM/dd/yyyy");

            ddate = dt.Rows[0]["deceasedDate"].ObjToDateTime();
            if (ddate.Year > 1875)
                lblDeceased.Text = "Deceased Date : " + ddate.ToString("MM/dd/yyyy");
            else
                lblDeceased.Text = "";
            string str = BuildClientTitle();
            this.Text = str;
            LoadContractExtras();
            btnFuneral.Hide();
            if ( ddate.Year > 1000 )
            {
                cmd = "Select * from `" + contractsFile + "` where `contractNumber` = '" + contractNumber + "';";
                dt = G1.get_db_data(cmd);
                if (dt.Rows.Count <= 0)
                    return;
                string serviceId = dt.Rows[0]["serviceId"].ObjToString();
                if (!String.IsNullOrWhiteSpace(serviceId))
                    btnFuneral.Show();
            }
        }
        /***********************************************************************************************/
        private void LoadBurialAssociation ( string payer )
        {
            if (String.IsNullOrWhiteSpace(payer))
                return;

            string SDI_Key_Code = "";

            DataTable dt = G1.get_db_data("Select * from `payers` WHERE `payer` = '" + payer + "';");
            if (dt.Rows.Count <= 0)
                return;

            try
            {
                DataTable burialDt = G1.get_db_data("Select * from `burial_association`");

                DataTable payerDt = InsuranceCoupons.BuildPayerTable(burialDt);
                SDI_Key_Code = dt.Rows[0]["SDICode"].ObjToString();
                if (String.IsNullOrWhiteSpace(SDI_Key_Code))
                    SDI_Key_Code = "YY";

                DataTable prefixDt = InsuranceCoupons.Locate_SDI_Key_Code(payerDt, payer);
                if (prefixDt.Rows.Count == 1)
                    SDI_Key_Code = prefixDt.Rows[0]["SDI_Key_Code"].ObjToString();
                else if (prefixDt.Rows.Count > 1)
                    SDI_Key_Code = "YY";

                DataRow[] dRows = burialDt.Select("SDI_Key_Code='" + SDI_Key_Code + "'");
                if (dRows.Length > 0)
                    lblBurialAss.Text = "BA : " + dRows[0]["burial_association"].ObjToString();
                else
                    lblBurialAss.Text = "";
            }
            catch ( Exception ex)
            {
            }
        }
        /***********************************************************************************************/
        private FunServices editFunServices = null;
        private bool funModified = false;
        private void InitializeServicePanel()
        {
            if (editFunServices != null)
                editFunServices.Close();
            this.panelClaimBottom.Hide();
            this.panelClaimTop.Hide();
            editFunServices = null;
            funModified = false;
            G1.ClearPanelControls(this.panelClaimAll);

            editFunServices = new FunServices(this, workContract, false );
            if (!this.LookAndFeel.UseDefaultLookAndFeel)
            {
                editFunServices.LookAndFeel.UseDefaultLookAndFeel = false;
                editFunServices.LookAndFeel.SetSkinStyle(this.LookAndFeel.SkinName);
            }
            G1.LoadFormInPanel(editFunServices, this.panelClaimAll);

            this.Refresh();
        }
        /***********************************************************************************************/
        private void LoadServices()
        {
            InitializeServicePanel();
        }
        /***********************************************************************************************/
        private void LoadServicesx()
        {
            string cmd = "Select * from `cust_services` where `contractNumber` = '" + workContract + "';";
            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("num");
            G1.NumberDataTable(dt);
            DetermineServices(dt);
            dgv.DataSource = dt;
            btnSaveServices.Hide();
        }
        /***********************************************************************************************/
        private void DetermineServices(DataTable dt)
        {
            double service = 0D;
            double merchandise = 0D;
            double cash = 0D;
            string str = "";
            string data = "";
            string type = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                type = dt.Rows[i]["type"].ObjToString();
                data = dt.Rows[i]["data"].ObjToString();
                if (type.ToUpper() == "MERCHANDISE")
                {
                    if (G1.validate_numeric(data))
                        merchandise += data.ObjToDouble();
                }
                else if (type.ToUpper() == "CASH ADVANCE")
                {
                    if (G1.validate_numeric(data))
                        cash += data.ObjToDouble();
                }
                else if (type.ToUpper() == "OTHER")
                    continue;
                else
                {
                    if (G1.validate_numeric(data))
                        service += data.ObjToDouble();
                }
            }
            lblCash.Text = "Cash Advance : " + G1.ReformatMoney(cash);
            lblService.Text = "Services : " + G1.ReformatMoney(service);
            lblMerchandise.Text = "Merchandise : " + G1.ReformatMoney(merchandise);
        }
        /***********************************************************************************************/
        private void SetupToolTips()
        {
            ToolTip tt = new ToolTip();
            tt.SetToolTip(this.pictureDelete, "Remove Medical Information");
            ToolTip tt1 = new ToolTip();
            tt.SetToolTip(this.pictureAdd, "Add Medical Information");
            ToolTip tt2 = new ToolTip();
            tt.SetToolTip(this.pictureBox1, "Search Medical Information");

            ToolTip tt3 = new ToolTip();
            tt.SetToolTip(this.pictureBox4, "Search Claim Information");
            ToolTip tt4 = new ToolTip();
            tt.SetToolTip(this.pictureBox3, "Add New Claim");
            ToolTip tt5 = new ToolTip();
            tt.SetToolTip(this.pictureBox2, "Remove Claim");
        }
        /***********************************************************************************************/
        public static string GetAgentName(string agentCode)
        {
            string name = "";
            if (String.IsNullOrWhiteSpace(agentCode))
                return name;
            string cmd = "Select * from `agents` where `agentCode` = '" + agentCode + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
                name = dx.Rows[0]["firstName"] + " " + dx.Rows[0]["lastName"].ObjToString();
            return name;
        }
        /***********************************************************************************************/
        private void LoadData()
        {
            SetupTrustPaidCombo(cmbTrustPaid1);
            SetupTrustPaidCombo(cmbTrustPaid2);
            SetupTrustPaidCombo(cmbTrustPaid3);
            SetupTrustPaidCombo(cmbTrustPaid4);
            SetupTrustPaidCombo(cmbTrustPaid5);

            string cmd = "Select * from `" + customersFile + "` where `contractNumber` = '" + workContract + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
            {
                if (workContract.ToUpper().IndexOf("SX") == 0)
                {
                    MessageBox.Show("***ERROR*** Reading Customer Data Record " + workContract.ToString() + "!");
                    this.Cursor = Cursors.Default;
                    return;
                }
                if (!G1.isAdmin() && !G1.isHR())
                {
                    MessageBox.Show("***ERROR*** Reading Customer Data Record " + workContract.ToString() + "!!");
                    this.Close();
                    majorError = true;
                    return;
                }
                DialogResult result = MessageBox.Show("***ERROR*** Customer Does Not Exist!\nDo you want to create it\nand then edit ?", "Customer Error Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                if (result == DialogResult.No)
                {
                    this.Cursor = Cursors.Default;
                    this.Close();
                    majorError = true;
                    return;
                }

                //cmd = "Select * from `" + customersFile + "` where `contractNumber` = 'XXXXY';";
                //dt = G1.get_db_data(cmd);
                //if (dt.Rows.Count <= 0)
                //    return;

                string record = G1.create_record(customersFile, "contractNumber", workContract);
                if (G1.BadRecord(customersFile, record))
                {
                    this.Cursor = Cursors.Default;
                    this.Close();
                    majorError = true;
                    return;
                }
                dt = G1.get_db_data(cmd);
                if (dt.Rows.Count <= 0)
                {
                    this.Cursor = Cursors.Default;
                    this.Close();
                    majorError = true;
                    return;
                }
            }

            cmd = "Select * from `" + contractsFile + "` where `contractNumber` = '" + workContract + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
            {
                if ( !G1.isAdmin() && !G1.isHR() )
                {
                    MessageBox.Show("***ERROR*** Reading Customer Contract " + workContract.ToString() + "!!");
                    if (dx.Rows.Count <= 0)
                    {
                        this.Close();
                        majorError = true;
                        return;
                    }
                }
                DialogResult result = MessageBox.Show("***ERROR*** CONTRACT Does Not Exist!\nDo you want to create it\nand then edit ?", "CONTRACT Error Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                if (result == DialogResult.No)
                {
                    this.Cursor = Cursors.Default;
                    this.Close();
                    majorError = true;
                    return;
                }
                string record = G1.create_record(contractsFile, "contractNumber", workContract);
                if (G1.BadRecord(contractsFile, record))
                {
                    this.Cursor = Cursors.Default;
                    this.Close();
                    majorError = true;
                    return;
                }
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count <= 0)
                {
                    this.Cursor = Cursors.Default;
                    this.Close();
                    majorError = true;
                    return;
                }
                //MessageBox.Show("***ERROR*** Reading Customer Contract " + workContract.ToString() + "!!");
                //if (dx.Rows.Count <= 0)
                //{
                //    this.Close();
                //    majorError = true;
                //    return;
                //}
                //cmd = "Select * from `" + contractsFile + "` where `contractNumber` = 'XXXXY';";
                //dx = G1.get_db_data(cmd);
                //if (dx.Rows.Count <= 0)
                //    return;
            }
            else
            {
                if ( DailyHistory.isInsurance ( workContract ) )
                {
                    DataTable payDt = G1.get_db_data("Select * from `icustomers` WHERE `contractNumber` = '" + workContract + "';");
                    if (payDt.Rows.Count > 0)
                    {
                        string version = payDt.Rows[0]["version"].ObjToString();
                        workPayer = payDt.Rows[0]["payer"].ObjToString();
                        payDt = G1.get_db_data("Select * from `payers` WHERE `payer` = '" + workPayer + "' AND `version` = '" + version + "';");
                        if (payDt.Rows.Count > 0)
                        {
                            string tempStr = payDt.Rows[0]["contractNumber"].ObjToString();
                            if (!String.IsNullOrWhiteSpace(tempStr))
                                workContract = tempStr;
                            cmd = "Select * from `" + contractsFile + "` where `contractNumber` = '" + workContract + "';";
                            dx = G1.get_db_data(cmd);
                        }
                    }
                }
            }
            loading = true;
            DateTime dueDate8 = dx.Rows[0]["dueDate8"].ObjToDateTime();
            double payment = dx.Rows[0]["amtOfMonthlyPayt"].ObjToDouble();
            if (!String.IsNullOrWhiteSpace(workPayer))
                payment = Policies.CalcMonthlyPremium(workPayer);

            workRecord = dt.Rows[0]["record"].ObjToString();

            dt.Columns.Add("bDate");
            dt.Columns.Add("ssno");
            dt.Columns.Add("agreement");
            dt.Columns.Add("gender");
            dt.Rows[0]["gender"] = "1";

            InitializeCustomerPanel();

            DateTime ddate = DateTime.Now;

            txtFirstName = dt.Rows[0]["firstName"].ObjToString();
            txtLastName = dt.Rows[0]["lastName"].ObjToString();
            txtMiddleName = dt.Rows[0]["middleName"].ObjToString();
            txtPrefix = dt.Rows[0]["prefix"].ObjToString();
            txtSuffix = dt.Rows[0]["suffix"].ObjToString();
            //txtClientNumber.Text = workRecord.ToString();
            //txtClientNumber.Text = workContract;
            //FormatSSN(dt, "ssn", "ssno");
            //FixDates(dt, "birthDate", "bDate");

            string agentCode = dt.Rows[0]["agentCode"].ObjToString();
            string name = GetAgentName(agentCode);

            txtAgentCode.Text = agentCode;
            txtAgentName.Text = name;


            //string bdate = dt.Rows[0]["bDate"].ObjToString();
            ////if (workPolicy)
            ////{
            ////    DateTime bbDate = policyDt.Rows[0]["birthDate"].ObjToDateTime();
            ////    if (bbDate.Year > 1850)
            ////        bdate = bbDate.ToString("MM/dd/yyyy");
            ////}
            //string age = G1.CalcAge(bdate);
            //txtAge.Text = age;

            //lblName.Text = "Name: " + txtFirstName.Text + " " + txtLastName.Text;
            //txtSSN.Text = dt.Rows[0]["ssno"].ObjToString();
            //if (DailyHistory.isInsurance(workContract))
            //{
            //    lblPayer.Text = "Payer: " + dt.Rows[0]["payer"].ObjToString();
            //    workPayer = dt.Rows[0]["payer"].ObjToString();
            //}
            //else
            //    lblPayer.Text = "";

            //DateTime ddate = dt.Rows[0]["birthDate"].ObjToDateTime();
            ////if (workPolicy)
            ////    ddate = policyDt.Rows[0]["birthDate"].ObjToDateTime();
            //if (ddate.Year > 1875)
            //{
            //    dateDOB.Text = ddate.ToString("MM/dd/yyyy");
            //    txtDOB.Text = ddate.ToString("MM/dd/yyyy");
            //}

            //ddate = dt.Rows[0]["deceasedDate"].ObjToDateTime();
            //if (ddate.Year < 1875)
            //    ddate = dx.Rows[0]["deceasedDate"].ObjToDateTime();
            //dateDeceased.Text = ddate.ToString("MM/dd/yyyy");

            ddate = dx.Rows[0]["trustPaidDate1"].ObjToDateTime();
            if (ddate.Year > 1850)
                dateMoneyPaid1.Text = ddate.ToString("MM/dd/yyyy");

            ddate = dx.Rows[0]["trustPaidDate2"].ObjToDateTime();
            if (ddate.Year > 1850)
                dateMoneyPaid2.Text = ddate.ToString("MM/dd/yyyy");

            ddate = dx.Rows[0]["trustPaidDate3"].ObjToDateTime();
            if (ddate.Year > 1850)
                dateMoneyPaid3.Text = ddate.ToString("MM/dd/yyyy");
            ddate = dx.Rows[0]["trustPaidDate4"].ObjToDateTime();
            if (ddate.Year > 1850)
                dateMoneyPaid4.Text = ddate.ToString("MM/dd/yyyy");
            ddate = dx.Rows[0]["trustPaidDate5"].ObjToDateTime();
            if (ddate.Year > 1850)
                dateMoneyPaid5.Text = ddate.ToString("MM/dd/yyyy");

            ddate = dx.Rows[0]["dateDPPaid"].ObjToDateTime();
            if (ddate.Year > 1850)
                dateDPPaid.Text = ddate.ToString("MM/dd/yyyy");

            string trustPaid = dx.Rows[0]["trustPaid1"].ObjToString();
            if (!String.IsNullOrWhiteSpace(trustPaid))
                cmbTrustPaid1.Text = trustPaid;

            trustPaid = dx.Rows[0]["trustPaid2"].ObjToString();
            if (!String.IsNullOrWhiteSpace(trustPaid))
                cmbTrustPaid2.Text = trustPaid;

            trustPaid = dx.Rows[0]["trustPaid3"].ObjToString();
            if (!String.IsNullOrWhiteSpace(trustPaid))
                cmbTrustPaid3.Text = trustPaid;

            trustPaid = dx.Rows[0]["trustPaid4"].ObjToString();
            if (!String.IsNullOrWhiteSpace(trustPaid))
                cmbTrustPaid4.Text = trustPaid;

            trustPaid = dx.Rows[0]["trustPaid5"].ObjToString();
            if (!String.IsNullOrWhiteSpace(trustPaid))
                cmbTrustPaid5.Text = trustPaid;

            string trustRemoved = dx.Rows[0]["trustRemoved"].ObjToString();
            if (!String.IsNullOrWhiteSpace(trustRemoved))
                cmbRemove.Text = trustRemoved;
            string trustRefunded = dx.Rows[0]["trustRefunded"].ObjToString();
            if (!String.IsNullOrWhiteSpace(trustRefunded))
                cmbRefund.Text = trustRefunded;
            string removePaid = dx.Rows[0]["dateRemoved"].ObjToString();
            if (!String.IsNullOrWhiteSpace(removePaid))
                txtDOR.Text = removePaid;
            labelServiceId.Hide();
            //            btnFuneral.Hide();
            lblServiceId.Hide();
            txtServiceId.Hide();
            string serviceId = "";
            if (!String.IsNullOrWhiteSpace(serviceId))
            {
                labelServiceId.Text = "Service ID : " + serviceId;
                labelServiceId.Show();
                btnFuneral.Show();
            }

            txtSex.Text = dt.Rows[0]["sex"].ObjToString();
            //            lblClient.Text = "Customer #:" + dt.Rows[0]["record"].ObjToString();
            lblClient.Text = "Contract #:" + workContract;
            lblSSN.Text = "SSN: " + dt.Rows[0]["ssno"].ObjToString();

            string gender = ValidateGender(dt.Rows[0]["sex"].ObjToString());
            //            SetupComboTable(this.comboGender, "ref_gender", "gender", gender);

            string maritalStatus = dt.Rows[0]["maritalstatus"].ObjToString();
            //            SetupComboTable(this.comboMaritalStatus, "ref_marital_status", "maritalstatus", maritalStatus);

            string race = dt.Rows[0]["race"].ObjToString();
            //            SetupComboTable(this.comboRace, "ref_race", "race", race);

            string ethnicity = dt.Rows[0]["ethnicity"].ObjToString();
            //            SetupComboTable(this.comboEthnicity, "ref_ethnicity", "ethnicity", ethnicity);

            string language = dt.Rows[0]["language"].ObjToString();
            //            SetupComboTable(this.comboLanguage, "ref_language", "language", language);

            textEdit_patientAddressLine1.Text = dt.Rows[0]["address1"].ObjToString();
            textEdit_patientAddressLine2.Text = dt.Rows[0]["address2"].ObjToString();
            textEdit_patientCity.Text = dt.Rows[0]["city"].ObjToString();
            textEdit_patientZipCode.Text = dt.Rows[0]["zip1"].ObjToString();

            string areaCode = dt.Rows[0]["areaCode"].ObjToString();
            string phone = dt.Rows[0]["phoneNumber"].ObjToString();

            string phone1 = dt.Rows[0]["phoneType1"].ObjToString();
            if (String.IsNullOrWhiteSpace(phone1))
            {
                if (!String.IsNullOrWhiteSpace(phone))
                {
                    if (!String.IsNullOrWhiteSpace(areaCode))
                    {
                        if (areaCode.IndexOf("(") < 0)
                        {
                            areaCode = "(" + areaCode + ")";
                            phone = phone.Replace(areaCode, "");
                        }
                        if (areaCode.IndexOf("(") >= 0)
                        {
                            phone = phone.Replace(areaCode, "");
                            phone1 = areaCode;
                        }
                        else
                            phone1 = "(" + areaCode + ") ";
                    }
                    phone1 += phone;
                }
            }

            cmbPhoneQualifier1.Text = phone1;
            cmbPhoneQualifier2.Text = dt.Rows[0]["phoneType2"].ObjToString();
            cmbPhoneQualifier3.Text = dt.Rows[0]["phoneType3"].ObjToString();

            txtPhone1.Text = dt.Rows[0]["phoneNumber1"].ObjToString();
            txtPhone2.Text = dt.Rows[0]["phoneNumber2"].ObjToString();
            txtPhone3.Text = dt.Rows[0]["phoneNumber3"].ObjToString();

            string state = dt.Rows[0]["state"].ObjToString();
            SetupComboTable(this.comboStates, "ref_states", "abbrev", state);
            for (int i = 0; i < xtraTabControl2.TabPages.Count; i++)
            {
                string tabName = xtraTabControl2.TabPages[i].Name.ObjToString().ToUpper();
                DevExpress.XtraTab.XtraTabPage page = xtraTabControl2.TabPages[i];
                if (tabName.ToUpper() == "TABCONTACT")
                    xtraTabControl2.TabPages[i].PageVisible = false;

            }

            for (int i = 0; i < xtraTabControl1.TabPages.Count; i++)
            {
                string tabName = xtraTabControl1.TabPages[i].Name.ObjToString().ToUpper();
                DevExpress.XtraTab.XtraTabPage page = xtraTabControl1.TabPages[i];
                if (tabName != "TABDEMOGRAPHICS")
                {
                    xtraTabControl1.FirstVisiblePageIndex = i;
                    xtraTabControl1.SelectedTabPageIndex = i;
                    break;
                }
            }
            //LoadAttachments();
            //LoadClaims();
            //LoadSites();

            UpdateHeaderInfo();

            InitializeDailyHistoryTabPage();
            //LoadRelatives();
            //LoadAgreements();
            LoadCustomerPicture();
            policyDueDate8 = dueDate8;
            LoadPolicies(dueDate8);
            //            LoadACH(payment);
            LoadACH(0D);
            LoadCC(0D);
            loading = false;
            btnSave.Enabled = false;
            btnSave.Show();
        }
        /***********************************************************************************************/
        private void SaveACH()
        {
            string dom = txtDayOfMonth.Text.Trim();
            string frequency = txtFrequency.Text.Trim();
            string routingNumber = txtRouting.Text.Trim();
            string accountNumber = txtAccount.Text.Trim();
            string payment = txtPayment.Text.Trim();
            string acctType = cmbAcctType.Text;
            string numPayments = txtNumPayments.Text;
            string leftPayments = txtLeftPayments.Text;
            DateTime dateBeginning = dateBeginACH.Text.ObjToDateTime();
            if (String.IsNullOrWhiteSpace(acctType))
                acctType = "Checking";
            if (acctType.ToUpper() != "CHECKING" && acctType.ToUpper() != "SAVINGS")
                acctType = "Checking";
            string record = "";
            if (!String.IsNullOrWhiteSpace(routingNumber) || !String.IsNullOrWhiteSpace(accountNumber))
            {
                string cmd = "Select * from `ach` where `contractNumber` = '" + workContract + "';";
                DataTable dx = G1.get_db_data(cmd);
                if (dx.Rows.Count <= 0)
                    record = G1.create_record("ach", "code", "-1");
                else
                    record = dx.Rows[0]["record"].ObjToString();
                if (G1.BadRecord("ach", record))
                    return;
                string code = "01";
                if (!String.IsNullOrWhiteSpace(workPayer))
                    code = "02";
                if (acctType.ToUpper() == "CHECKING")
                    code = "01";
                else if (acctType.ToUpper() == "SAVINGS")
                    code = "02";
                G1.update_db_table("ach", "record", record, new string[] { "contractNumber", workContract, "payer", workPayer, "code", code, "routingNumber", routingNumber, "accountNumber", accountNumber, "frequencyInMonths", frequency, "dayOfMonth", dom, "acctType", acctType, "payment", payment, "dateBeginning", dateBeginning.ToString("yyyy-MM-dd"), "numPayments", numPayments, "leftPayments", leftPayments });
            }
        }
        /***********************************************************************************************/
        private double achMonthlyPayment = 0D;
        private void LoadACH(double payment)
        {
            payment = 0D; // Force this to zero and pickup whatever is in the ACH table
            lblRouting.Text = "";
            achMonthlyPayment = payment;
            string cmd = "Select * from `ach` where `contractNumber` = '" + workContract + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
                return;
            string dom = dx.Rows[0]["dayOfMonth"].ObjToString();
            string spayment = dx.Rows[0]["payment"].ObjToString();
            achMonthlyPayment = spayment.ObjToDouble();
            string frequency = dx.Rows[0]["frequencyInMonths"].ObjToString();
            string routingNumber = dx.Rows[0]["routingNumber"].ObjToString();
            string accountNumber = dx.Rows[0]["accountNumber"].ObjToString();
            string acctType = dx.Rows[0]["acctType"].ObjToString();
            string numPayments = dx.Rows[0]["numPayments"].ObjToString();
            string leftPayments = dx.Rows[0]["leftPayments"].ObjToString();
            string code = dx.Rows[0]["code"].ObjToString();

            DateTime dateBeginning = dx.Rows[0]["dateBeginning"].ObjToDateTime();

            //payment = payment * Convert.ToInt32(frequency);

            txtFrequency.Text = frequency;
            txtDayOfMonth.Text = dom;
            double pay = spayment.ObjToDouble();
            spayment = G1.ReformatMoney(pay);
            if (pay <= 0D)
                spayment = G1.ReformatMoney(payment);
            txtPayment.Text = spayment;
            txtRouting.Text = routingNumber;
            txtAccount.Text = accountNumber;
            txtNumPayments.Text = numPayments;
            txtLeftPayments.Text = leftPayments;
            if (String.IsNullOrWhiteSpace(acctType))
                acctType = "Checking";
            if (acctType.ToUpper() != "CHECKING" && acctType != "SAVINGS")
                acctType = "Checking";
            cmbAcctType.Text = acctType;
            if ( code == "02")
                cmbAcctType.Text = "Savings";
            dateBeginACH.Text = dateBeginning.ToString("MM/dd/yyyy");
        }
        /***********************************************************************************************/
        private void SaveCC()
        {
            try
            {
                string dom = txtCCDayOfMonth.Text;
                string spayment = txtCCPayment.Text;
                string ccNumber = txtCCAccount.Text;
                string expirationDate = txtCCExpirationDate.Text;
                string numPayments = txtCCNumPayments.Text;
                string leftPayments = txtCCLeftPayments.Text;
                string dateBeginning = dateBeginCC.Text;
                string draftStartDate = dateBeginCC.Text;

                string insFirstName = txtInsFirstName.Text;
                string insMiddleName = txtInsMiddleName.Text;
                string insLastName = txtInsLastName.Text;

                string cardFirstName = txtCardFirstName.Text;
                string cardMiddleName = txtCardMiddleName.Text;
                string cardLastName = txtCardLastName.Text;

                string billingZip = txtBillingZip.Text;

                string allowFee = txtAllowFees.Text.Trim().ToUpper();

                string str = expirationDate;

                if (str.IndexOf("/") < 0)
                    return;
                string[] Lines = str.Split('/');
                if (Lines.Length < 2)
                    return;
                str = Lines[0].Trim();
                int month = str.ObjToInt32();
                if (month <= 0 || month > 12)
                    return;

                string record = "";

                string lookup = workContract;
                if (!String.IsNullOrWhiteSpace(workPayer))
                    lookup = workPayer;

                string cmd = "Select * from `creditcards` WHERE `contractNumber` = '" + lookup + "';";
                DataTable dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                    record = dx.Rows[0]["record"].ObjToString();
                else
                    record = G1.create_record("creditcards", "cardMiddleName", "-1");
                if (G1.BadRecord("creditcards", record))
                    return;
                if (String.IsNullOrWhiteSpace(workPayer))
                    G1.update_db_table("creditcards", "record", record, new string[] { "contractNumber", lookup, "payer", "", "insFirstName", insFirstName, "insMiddleName", insMiddleName, "insLastName", insLastName, "cardFirstName", cardFirstName, "cardMiddleName", cardMiddleName, "cardLastName", cardLastName, "allowFee", allowFee });
                else
                    G1.update_db_table("creditcards", "record", record, new string[] { "contractNumber", workPayer, "payer", workPayer, "insFirstName", insFirstName, "insMiddleName", insMiddleName, "insLastName", insLastName, "cardFirstName", cardFirstName, "cardMiddleName", cardMiddleName, "cardLastName", cardLastName });

                G1.update_db_table("creditcards", "record", record, new string[] { "draftAmount", spayment, "ccNumber", ccNumber, "expirationDate", expirationDate, "draftStartDate", draftStartDate, "numPayments", numPayments.ToString(), "remainingPayments", leftPayments.ToString(), "draftStartDay", dom, "billingZip", billingZip });
            }
            catch ( Exception ex )
            {
            }
        }
        /***********************************************************************************************/
        private double ccMonthlyPayment = 0D;
        private bool gotCC = false;
        private void LoadCC(double payment)
        {
            payment = 0D; // Force this to zero and pickup whatever is in the ACH table
            ccMonthlyPayment = payment;
            string lookup = workContract;
            if (!String.IsNullOrWhiteSpace(workPayer))
                lookup = workPayer;
            string cmd = "Select * from `creditcards` where `contractNumber` = '" + lookup + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
                return;
            string dom = dx.Rows[0]["draftStartDay"].ObjToString();
            string spayment = dx.Rows[0]["draftAmount"].ObjToString();
            ccMonthlyPayment = spayment.ObjToDouble();
            string accountNumber = dx.Rows[0]["ccNumber"].ObjToString();
            string numPayments = dx.Rows[0]["numPayments"].ObjToString();
            string leftPayments = dx.Rows[0]["remainingPayments"].ObjToString();
            DateTime dateBeginning = dx.Rows[0]["draftStartDate"].ObjToDateTime();
            string expirationDate = dx.Rows[0]["expirationDate"].ObjToString();
            string billingZip = dx.Rows[0]["billingZip"].ObjToString();

            string allowFee = dx.Rows[0]["allowFee"].ObjToString();

            txtCCDayOfMonth.Text = dom;
            double pay = spayment.ObjToDouble();
            spayment = G1.ReformatMoney(pay);
            if (pay <= 0D)
                spayment = G1.ReformatMoney(payment);
            txtCCPayment.Text = spayment;
            txtCCAccount.Text = accountNumber;
            txtCCExpirationDate.Text = expirationDate;
            txtCCNumPayments.Text = numPayments;
            txtCCLeftPayments.Text = leftPayments;
            dateBeginCC.Text = dateBeginning.ToString("MM/dd/yyyy");

            string insFirstName = dx.Rows[0]["insFirstName"].ObjToString();
            string insMiddleName = dx.Rows[0]["insMiddleName"].ObjToString();
            string insLastName = dx.Rows[0]["insLastName"].ObjToString();

            string cardFirstName = dx.Rows[0]["cardFirstName"].ObjToString();
            string cardMiddleName = dx.Rows[0]["cardMiddleName"].ObjToString();
            string cardLastName = dx.Rows[0]["cardLastName"].ObjToString();

            txtInsFirstName.Text = insFirstName;
            txtInsMiddleName.Text = insMiddleName;
            txtInsLastName.Text = insLastName;

            txtCardFirstName.Text = cardFirstName;
            txtCardMiddleName.Text = cardMiddleName;
            txtCardLastName.Text = cardLastName;

            txtAllowFees.Text = allowFee;

            txtBillingZip.Text = billingZip;
            gotCC = true;
        }
        /***********************************************************************************************/
        private double policyOldPremium = 0D;
        private void LoadPolicies(DateTime dueDate8)
        {
            //            if (workContract.ToUpper().IndexOf("ZZ") != 0)
            if (!workPolicy && workWhat != "Policies" )
            {
                RemoveMainTabPage("POLICIES");
                return;
            }

            chkFilterInactive.Checked = true;
            string preference = G1.getPreference(LoginForm.username, "Allow Filter Deceased in Lookups", "Allow Access");
            if (preference != "YES")
                chkFilterInactive.Hide();


            //AddSummaryColumn("premium", gridMain5);
            gridMain5.Columns["premium"].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Custom;
            gridMain5.Columns["premium"].SummaryItem.DisplayFormat = "{0:C2}";

            AddSummaryColumn("myPremium", gridMain5);
            //AddSummaryColumn("liability", gridMain5);
            gridMain5.Columns["liability"].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Custom;
            gridMain5.Columns["liability"].SummaryItem.DisplayFormat = "{0:C2}";

            gridMain5.Columns["historicPremium"].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Custom;
            gridMain5.Columns["historicPremium"].SummaryItem.DisplayFormat = "{0:C2}";

            G1.loadGroupCombo(cmbSelectColumns, "Policies", "Primary");

            string cmd = "Select * from `policies` p JOIN `icustomers` d ON p.`contractNumber` = d.`contractNumber` JOIN `icontracts` x ON p.`contractNumber` = x.`contractNumber` ";
            cmd += " LEFT JOIN `payers` c ON p.`payer` = c.`payer` ";
            if (String.IsNullOrWhiteSpace(workPayer))
                return;
            cmd += " WHERE p.`payer` = '" + workPayer + "' ";
            cmd += ";";

            string report = "";
            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("mod");

            if ( dt.Rows.Count > 0 && G1.isAdmin() )
                dt = CheckPolicyPayerName(dt);

            double oldPremium = 0D;
            double newPremium = 0D;
            double monthlyPremium = 0D;
            double historicPremium = 0D;
            double monthlySecNat = 0D;
            double monthly3rdParty = 0D;

            CustomerDetails.CalcMonthlyPremium ( workPayer, ref monthlyPremium, ref historicPremium, ref monthlySecNat, ref monthly3rdParty);
            oldPremium = monthlyPremium - monthlySecNat;
            policyOldPremium = oldPremium;


            FixOrphanPolicies(dt);

            if (chkHonor.Checked || workWhat == "Policies")
            {
                DataTable testDt = filterSecNat(chkSecNat.Checked, dt);
                dt = testDt.Copy();

                if (chkHonor3rdParty.Checked)
                {
                    for (int i = dt.Rows.Count - 1; i >= 0; i--)
                    {
                        report = dt.Rows[i]["report"].ObjToString().ToUpper();
                        if (report == "NOT THIRD PARTY" || String.IsNullOrWhiteSpace(report))
                            dt.Rows.RemoveAt(i);
                    }
                }
                else if (!chkSecNat.Checked)
                {
                    if (DateTime.Now > DailyHistory.kill3rdPartyDate)
                    {
                        for (int i = dt.Rows.Count - 1; i >= 0; i--)
                        {
                            report = dt.Rows[i]["report"].ObjToString().ToUpper();
                            if (report != "NOT THIRD PARTY")
                                dt.Rows.RemoveAt(i);
                        }
                    }
                }
            }

            dt.Columns.Add("num");
            dt.Columns.Add("bDate");
            dt.Columns.Add("ssno");
            dt.Columns.Add("select");
            dt.Columns.Add("fullname");
            dt.Columns.Add("policyfullname");
            dt.Columns.Add("myDeceasedDate");
            dt.Columns.Add("myPremium", Type.GetType("System.Double"));
            DateTime date = DateTime.Now;
            double premium = 0D;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                date = dt.Rows[i]["deceasedDate"].ObjToDateTime();
                if (date.Year > 900)
                    dt.Rows[i]["myDeceasedDate"] = date.ToString("MM/dd/yyyy");
                premium = dt.Rows[i]["premium"].ObjToDouble();
                dt.Rows[i]["myPremium"] = premium;
                dt.Rows[i]["dueDate8"] = G1.DTtoMySQLDT(dueDate8);
            }

            G1.NumberDataTable(dt);
            FixDates(dt, "birthDate", "bDate");
            FormatSSN(dt, "ssn", "ssno");
            SetupFullNames(dt);
            FixDeceasedDate(dt);

            FastLookup.FilterPolicies(dt);

            if ( workWhat == "Policies")
            {
                DataView tempview1 = dt.DefaultView;
                tempview1.Sort = "policyfullname";
                dt = tempview1.ToTable();

            }

            if ( dt.Rows.Count > 0 )
                dt = G1.RemoveDuplicates(dt, "record");

            dgv5.DataSource = dt;
            //gridMain5.Columns["num"].Fixed = DevExpress.XtraGrid.Columns.FixedStyle.Left;
            //gridMain5.Columns["contractNumber"].Fixed = DevExpress.XtraGrid.Columns.FixedStyle.Left;
            if (saveRow == -2)
            {
                gridMain5.FocusedRowHandle = dt.Rows.Count - 1;
                gridMain5.SelectRow(dt.Rows.Count - 1);
                gridMain5.RefreshData();
                dgv5.Refresh();
            }
            else if (saveRow >= 0)
            {
                gridMain5.FocusedRowHandle = saveRow;
                gridMain5.SelectRow(saveRow);
                gridMain5.RefreshData();
                dgv5.Refresh();
            }
            if (G1.isField() || workWhat == "Policies" )
            {
                chkUseLockPositions.Hide();
                chkSecNat.Hide();
                chkHonor3rdParty.Hide();
                chkHonor.Hide();
                chkFilterInactive.Hide();
            }
            saveRow = -1;
            if (G1.isField() || workWhat == "Policies" )
                cmbSelectColumns.Text = "Policy Summary 2A";
        }
        /****************************************************************************************/
        private DataTable CheckPolicyPayerName ( DataTable dt )
        {
            string policyPayerFname = dt.Rows[0]["firstName"].ObjToString();
            string policyPayerLname = dt.Rows[0]["lastName"].ObjToString();
            string payerFirstName = dt.Rows[0]["firstName1"].ObjToString();
            string payerLastName = dt.Rows[0]["lastName1"].ObjToString();

            if (policyPayerFname == payerFirstName && policyPayerLname == payerLastName)
                return dt;

            string policyPayerFullName = policyPayerFname + " " + policyPayerLname;
            string payerFullName = payerFirstName + " " + payerLastName;

            DialogResult result = MessageBox.Show("Primary Payer Name (" + payerFullName + ")\nDoes not match Policy Payer Name (" + policyPayerFullName + ")!\nDo you want to change the Policy Payer to match Primary Payer?", "Name Mismatch Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            if (result == DialogResult.No)
                return dt;

            string record = "";

            for ( int i=0; i<dt.Rows.Count; i++)
            {
                record = dt.Rows[i]["record"].ObjToString();
                G1.update_db_table("policies", "record", record, new string[] { "firstName", payerFirstName, "lastName", payerLastName });
                dt.Rows[i]["firstName"] = payerFirstName;
                dt.Rows[i]["lastName"] = payerLastName;
            }
            return dt;
        }
        /****************************************************************************************/
        public static void FixOrphanPolicies(DataTable dt)
        {
            DateTime date = DateTime.Now;
            DateTime payerDueDate = DateTime.Now;
            DateTime dolp = DateTime.Now;
            DateTime payerDolp = DateTime.Now;
            string payer = "";
            string orphanContract = "";
            string contractNumber = "";
            DateTime payerDeceasedDate = DateTime.Now;

            int customerColumn = G1.get_column_number(dt, "deceasedDate");
            int policyColumn = G1.get_column_number(dt, "deceasedDate2");
            int payerColumn = G1.get_column_number(dt, "deceasedDate3");
            if (customerColumn > policyColumn)
                return;
            if (policyColumn > payerColumn)
                return;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                payer = dt.Rows[i]["payer"].ObjToString();
                payerDeceasedDate = dt.Rows[i]["deceasedDate3"].ObjToDateTime();
                date = dt.Rows[i]["deceasedDate2"].ObjToDateTime();
                date = dt.Rows[i]["dueDate8"].ObjToDateTime();
                payerDueDate = dt.Rows[i]["dueDate81"].ObjToDateTime();
                if (payerDueDate >= date)
                {
                    orphanContract = dt.Rows[i]["contractNumber"].ObjToString();
                    if (orphanContract.IndexOf("OO") == 0 || orphanContract.IndexOf("MM") == 0 )
                    {
                        contractNumber = dt.Rows[i]["contractNumber3"].ObjToString();
                        if (contractNumber.IndexOf("ZZ") == 0)
                        {
                            dt.Rows[i]["contractNumber"] = contractNumber;
                            if ( payerDeceasedDate.Year > 1000 )
                                dt.Rows[i]["deceasedDate"] = G1.DTtoMySQLDT(dt.Rows[i]["deceasedDate3"].ObjToDateTime());
                            dt.Rows[i]["deceasedDate1"] = G1.DTtoMySQLDT(dt.Rows[i]["deceasedDate3"].ObjToDateTime());
                            dt.Rows[i]["deceasedDate2"] = G1.DTtoMySQLDT(dt.Rows[i]["deceasedDate3"].ObjToDateTime());
                        }
                    }
                    date = payerDueDate;
                    dt.Rows[i]["dueDate8"] = G1.DTtoMySQLDT(date.ToString("yyyy-MM-dd"));
                    dolp = dt.Rows[i]["lastDatePaid8"].ObjToDateTime();
                    payerDolp = dt.Rows[i]["lastDatePaid81"].ObjToDateTime();
                    if (payerDolp > dolp)
                        dt.Rows[i]["lastDatePaid8"] = G1.DTtoMySQLDT(payerDolp.ToString("yyyy-MM-dd"));
                }
                //dt.Rows[i]["dueDate"] = date.ToString("yyyy-MM-dd");
            }
        }
        /****************************************************************************************/
        public static void FixOrphanPolicies2(DataTable dt)
        {
            DateTime date = DateTime.Now;
            DateTime payerDueDate = DateTime.Now;
            DateTime dolp = DateTime.Now;
            DateTime payerDolp = DateTime.Now;
            string payer = "";
            string orphanContract = "";
            string contractNumber = "";
            DateTime payerDeceasedDate = DateTime.Now;
            bool doOrphan = false;

            //DataRow[] dRows = dt.Select("payer='CC-719'");
            //if ( dRows.Length > 0 )
            //{
            //    DataTable dddd = dRows.CopyToDataTable();
            //}

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    payer = dt.Rows[i]["payer"].ObjToString();
                    if (payer == "181670")
                    {
                    }
                    payerDeceasedDate = dt.Rows[i]["deceasedDate3"].ObjToDateTime();
                    date = dt.Rows[i]["deceasedDate2"].ObjToDateTime();
                    date = dt.Rows[i]["dueDate8"].ObjToDateTime();
                    payerDueDate = dt.Rows[i]["dueDate81"].ObjToDateTime();
                    if (payerDueDate >= date)
                    {
                        orphanContract = dt.Rows[i]["contractNumber"].ObjToString();
                        if (orphanContract.IndexOf("OO") == 0 || orphanContract.IndexOf("MM") == 0)
                        {
                            contractNumber = dt.Rows[i]["contractNumber3"].ObjToString();
                            if (contractNumber.IndexOf("ZZ") == 0)
                            {
                                dt.Rows[i]["contractNumber"] = contractNumber;
                                if (payerDeceasedDate.Year > 1000)
                                    dt.Rows[i]["deceasedDate2"] = G1.DTtoMySQLDT(dt.Rows[i]["deceasedDate3"].ObjToDateTime());
                                dt.Rows[i]["deceasedDate1"] = G1.DTtoMySQLDT(dt.Rows[i]["deceasedDate3"].ObjToDateTime());
                                dt.Rows[i]["deceasedDate"] = G1.DTtoMySQLDT(dt.Rows[i]["deceasedDate3"].ObjToDateTime());
                            }
                        }
                        else
                        {
                            contractNumber = dt.Rows[i]["contractNumber3"].ObjToString();
                            if (contractNumber.IndexOf("ZZ") == 0)
                            {
                                dt.Rows[i]["contractNumber"] = contractNumber;
                                if (payerDeceasedDate.Year > 1000)
                                    dt.Rows[i]["deceasedDate2"] = G1.DTtoMySQLDT(dt.Rows[i]["deceasedDate3"].ObjToDateTime());
                                dt.Rows[i]["deceasedDate1"] = G1.DTtoMySQLDT(dt.Rows[i]["deceasedDate3"].ObjToDateTime());
                                dt.Rows[i]["deceasedDate"] = G1.DTtoMySQLDT(dt.Rows[i]["deceasedDate3"].ObjToDateTime());
                            }
                        }
                        date = payerDueDate;
                        dt.Rows[i]["dueDate8"] = G1.DTtoMySQLDT(date.ToString("yyyy-MM-dd"));
                        dolp = dt.Rows[i]["lastDatePaid8"].ObjToDateTime();
                        payerDolp = dt.Rows[i]["lastDatePaid81"].ObjToDateTime();
                        if (payerDolp > dolp)
                            dt.Rows[i]["lastDatePaid8"] = G1.DTtoMySQLDT(payerDolp.ToString("yyyy-MM-dd"));
                        dt.Rows[i]["lapsed"] = dt.Rows[i]["lapsed3"].ObjToString();
                        dt.Rows[i]["lapseDate8"] = G1.DTtoMySQLDT(dt.Rows[i]["lapseDate81"].ObjToDateTime());
                    }
                    dt.Rows[i]["dueDate"] = date.ToString("yyyy-MM-dd");
                    dt.Rows[i]["ddate"] = date.ToString("yyyy-MM-dd");
                }
                catch ( Exception ex)
                {
                }
            }
            //dRows = dt.Select("payer='CC-719'");
            //if (dRows.Length > 0)
            //{
            //    DataTable dddd = dRows.CopyToDataTable();
            //}
            //dRows = dt.Select("payer='CC-719J'");
            //if (dRows.Length > 0)
            //{
            //    DataTable dddd = dRows.CopyToDataTable();
            //}
        }
        /***********************************************************************************************/
        private void FixDeceasedDate(DataTable dt)
        {
            string date1 = "";
            string date2 = "";
            if (G1.get_column_number(dt, "deceasedDate") < 0)
                return;
            if (G1.get_column_number(dt, "deceasedDate1") < 0)
                return;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                date1 = dt.Rows[i]["deceasedDate"].ObjToString();
                if (date1.IndexOf("0000") >= 0)
                {
                    date2 = dt.Rows[i]["deceasedDate1"].ObjToString();
                    if (date2.IndexOf("0000") < 0)
                        dt.Rows[i]["deceasedDate"] = dt.Rows[i]["deceasedDate1"];
                }
            }
        }
        /***********************************************************************************************/
        private void SetupFullNames(DataTable dt)
        {
            string fullname = "";
            string fname = "";
            string lname = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                fname = dt.Rows[i]["firstName"].ObjToString();
                lname = dt.Rows[i]["lastName"].ObjToString();
                fullname = fname + " " + lname;
                dt.Rows[i]["fullname"] = fullname;
                fname = dt.Rows[i]["policyFirstName"].ObjToString();
                lname = dt.Rows[i]["policyLastName"].ObjToString();
                fullname = fname + " " + lname;
                dt.Rows[i]["policyfullname"] = fullname;
            }
        }
        /****************************************************************************************/
        private void RemoveMainTabPage(string tabName)
        {
            for (int i = (xtraTabControl1.TabPages.Count - 1); i >= 0; i--)
            {
                XtraTabPage tp = xtraTabControl1.TabPages[i];
                if (tp.Text.ToUpper() == tabName.ToUpper())
                    xtraTabControl1.TabPages.RemoveAt(i);
            }
        }
        /***********************************************************************************************/
        private void LoadCustomerPicture()
        {
            string cmd = "Select * from `" + customersFile + "` where `record` = '" + workRecord + "';";
            DataTable dt = G1.get_db_data(cmd);
            Byte[] bytes = dt.Rows[0]["picture"].ObjToBytes();
            Image myImage = new Bitmap(1, 1);
            if (bytes != null)
            {
                myImage = G1.byteArrayToImage(bytes);
                this.PatientPicture.Image = (Bitmap)myImage;
            }
        }
        /***********************************************************************************************/
        private DailyHistory dailyForm = null;
        /***********************************************************************************************/
        private void InitializeDailyHistoryTabPage()
        {
            G1.ClearTabPageControls(tabDailyHistory);
            if (String.IsNullOrWhiteSpace(workContract))
                return;
            dailyForm = null;
            //if (!String.IsNullOrWhiteSpace(workPolicyRecord))
            //{
            //    dailyForm = new DailyHistory(workContract, workPolicyRecord);
            //    dailyForm.ManualDone += DailyForm_ManualDone;
            //}
            //else
            //{
            dailyForm = new DailyHistory(workContract);
            dailyForm.ManualDone += DailyForm_ManualDone;
            dailyForm.SkinChange += DailyForm_SkinChange;
            //            }
            if (!this.LookAndFeel.UseDefaultLookAndFeel)
            {
                tabDailyHistory.LookAndFeel.UseDefaultLookAndFeel = false;
                tabDailyHistory.LookAndFeel.SetSkinStyle(this.LookAndFeel.SkinName);
            }
            G1.LoadFormInTab(dailyForm, tabDailyHistory);
        }
        /***********************************************************************************************/
        private void DailyForm_SkinChange(string skin)
        {
            this.panelTop.BackColor = Color.Transparent;
            this.menuStrip1.BackColor = Color.Transparent;
            this.gridMain.PaintStyleName = "Skin";
            DevExpress.Skins.SkinManager.EnableFormSkins();
            this.LookAndFeel.UseDefaultLookAndFeel = true;
            DevExpress.LookAndFeel.UserLookAndFeel.Default.SetSkinStyle(skin);
            this.LookAndFeel.SetSkinStyle(skin);
        }
        /***********************************************************************************************/
        private void DailyForm_ManualDone(string s)
        {

        }
        /***********************************************************************************************/
        public static string ValidateGender(string genderIn)
        {
            string genderOut = "";
            if (String.IsNullOrWhiteSpace(genderIn))
                return ("unknown");
            string data = genderIn.Substring(0, 1).ToUpper();
            if (data == "M")
                genderOut = "male";
            if (data == "F")
                genderOut = "female";
            if (data == "U")
                genderOut = "unknown";
            return genderOut;
        }
        /***********************************************************************************************/
        public static void SetupComboTable(System.Windows.Forms.ComboBox box, string db, string field, string answer)
        {
            string cmd = "Select * from `" + db + "`;";
            DataTable dt = G1.get_db_data(cmd);

            DataSet myDataSet = new DataSet();
            myDataSet.Tables.Add(dt);

            box.DataSource = myDataSet.Tables[0].DefaultView;
            box.DisplayMember = field;
            box.Text = answer;
        }
        /***********************************************************************************************/
        private void LoadForAdding()
        {
            this.panelTop.Hide();
            this.panelTabBottom.Hide();
            this.panelTabTop.Hide();
            this.panelTabAll.Hide();
            this.panelDemoBottom.Hide();
            for (int i = 0; i < xtraTabControl1.TabPages.Count; i++)
            {
                string tabName = xtraTabControl1.TabPages[i].Name.ObjToString().ToUpper();
                DevExpress.XtraTab.XtraTabPage page = xtraTabControl1.TabPages[i];
                if (tabName != "TABDEMOGRAPHICS")
                {
                    xtraTabControl1.TabPages[i].Hide();
                    page.Hide();
                    xtraTabControl1.TabPages[i].PageEnabled = false;
                }
            }
            txtClientNumber.Text = "To Be Determined";
            btnSave.Enabled = false;
            btnSave.Show();
        }
        /***********************************************************************************************/
        public static void FormatSSN(DataTable dt, string columnName, string newColumn)
        {
            string ssn = "";
            string ssno = "";
            int row = 0;
            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    row = i;
                    ssn = dt.Rows[i][columnName].ObjToString();
                    ssno = ssn;
                    if (ssn.Trim().Length >= 9)
                    {
                        //ssno = "XXX-XX-" + ssn.Substring(5, 4);
                        ssno = ssn.Substring ( 0, 3 ) + "-" + ssn.Substring ( 3,2 ) + "-" + ssn.Substring(5, 4);
                    }
                    dt.Rows[i][newColumn] = ssno;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Fixing Date Field " + columnName + " Row= " + row + "SSN= " + ssn + " " + ex.Message.ObjToString());
            }
        }
        /***********************************************************************************************/
        public static void FixDates(DataTable dt, string columnName, string newColumn)
        {
            string date = "";
            long ldate = 0L;
            int row = 0;
            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    row = i;
                    date = dt.Rows[i][columnName].ObjToString();
                    if (String.IsNullOrWhiteSpace(date))
                        continue;
                    if (date == "0000-00-00")
                    {
                        date = "";
                        dt.Rows[i][columnName] = date;
                    }
                    else
                    {
                        ldate = G1.date_to_days(date);
                        date = G1.days_to_date(ldate);
                        dt.Rows[i][newColumn] = date;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Fixing Date Field " + columnName + " Row= " + row + "Date= " + date + " " + ex.Message.ObjToString());
            }
        }
        /***********************************************************************************************/
        private void CheckEnableSave()
        {
            bool allow = true;
            string firstName = txtFirstName.Trim();
            string lastName = txtLastName.Trim();
            if (String.IsNullOrWhiteSpace(firstName) || String.IsNullOrWhiteSpace(lastName))
                allow = false;
            if (allow)
            {
                btnSave.Enabled = true;
                btnSave.Show();
            }
        }
        /***********************************************************************************************/
        private bool ValidateRemoval()
        {
            if (1 == 1)
                return true;
            string removal = cmbRemove.Text.Trim().ToUpper();
            string serviceId = txtServiceId.Text.Trim();
            if (removal == "YES" && String.IsNullOrWhiteSpace(serviceId))
            {
                MessageBox.Show("***ERROR*** Removal MUST HAVE Service ID (Funeral #)");
                return false;
            }
            return true;
        }
        /***********************************************************************************************/
        private void SaveAll(bool closing = false)
        {
            if (!btnSave.Enabled)
            {
                MessageBox.Show("***ERROR*** Customer cannot be saved because of missing data or data problems.");
                return;
            }
            if (!ValidateRemoval())
                return;
            string record = workRecord;
            if (workAdding)
                record = G1.create_record(customersFile, "firstName", "-1");
            if (G1.BadRecord(customersFile, record))
                return;

            if (custModified)
            {
                editFunCustomer.FireEventSaveFunServices(true);
                custModified = false;
            }

            SaveAllOtherData(record);

            if (familyModified)
            {
                bool stayOpen = true;
                if (closing)
                    stayOpen = false;
                editFunFamily.FireEventSaveFunServices(true, stayOpen);
                familyModified = false;
            }

            bool showingFuneral = btnFuneral.Visible;
            this.panelTop.Show();
            this.panelTabBottom.Show();
            this.panelTabTop.Show();
            this.panelTabAll.Show();
            this.panelDemoBottom.Show();
            UpdateHeaderInfo();
            custModified = false;
            btnSave.Enabled = false;
            btnSave.Hide();
            //if ( btnFuneral.Visible && !showingFuneral)
            //{
            //    string ssn = lblSSN.Text.Trim();
            //    ssn = ssn.Replace("SSN # :", "");
            //    string cmd = "Select * from `fcustomers` WHERE `ssn` = '" + ssn + "';";
            //    DataTable dx = G1.get_db_data(cmd);
            //    if (dx.Rows.Count <= 0)
            //    {
            //        DialogResult result = MessageBox.Show("This looks like a new Funeral.\nAre you wanting to setup a new Funeral from this Pre-Need Contract?", "New Funeral Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            //        if (result == DialogResult.Yes)
            //        {
            //            CopyAllContractInfo(workContract);
            //            this.Cursor = Cursors.WaitCursor;
            //            EditCust editForm = new EditCust(workContract);
            //            editForm.Show();
            //            this.Cursor = Cursors.Default;
            //        }
            //    }
            //}
            if (editFunServices != null)
                editFunServices.FireEventFunServicesProtection(workContract);
        }
        /***********************************************************************************************/
        public static void CopyServiceId ( string contractIn )
        {
            string contractNumber = "";
            string record = "";
            string cmd = "Select * from `customers` WHERE `contractNumber` = '" + contractIn + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
                return;
            string ssn = dx.Rows[0]["ssn"].ObjToString();
            if (String.IsNullOrWhiteSpace(ssn))
                return;

            ssn = ssn.Replace("-", "");
            if (ssn.Length < 9)
                return;

            cmd = "Select * from `customers` WHERE `ssn` = '" + ssn + "';";
            dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 1)
                return;

            cmd = "Select * from `contracts` where `contractNumber` = '" + contractIn + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return;
            DateTime deceasedDate = dt.Rows[0]["deceasedDate"].ObjToDateTime();
            string serviceId = dt.Rows[0]["serviceId"].ObjToString();
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                contractNumber = dx.Rows[i]["contractNumber"].ObjToString();
                if (contractNumber == contractIn)
                {
                    continue;
                }
                cmd = "Select * from `contracts` where `contractNumber` = '" + contractNumber + "';";
                dt = G1.get_db_data(cmd);
                if (dt.Rows.Count > 0)
                {
                    record = dt.Rows[0]["record"].ObjToString();
                    G1.update_db_table("contracts", "record", record, new string[] { "deceasedDate", deceasedDate.ToString("MM/dd/yyyy"), "serviceId", serviceId });
                }

                cmd = "Select * from `customers` where `contractNumber` = '" + contractNumber + "';";
                dt = G1.get_db_data(cmd);
                if (dt.Rows.Count > 0)
                {
                    record = dt.Rows[0]["record"].ObjToString();
                    G1.update_db_table("customers", "record", record, new string[] { "deceasedDate", deceasedDate.ToString("MM/dd/yyyy"), "serviceId", serviceId });
                }

                cmd = "Select * from `cust_extended` where `contractNumber` = '" + contractNumber + "';";
                dt = G1.get_db_data(cmd);
                if (dt.Rows.Count > 0)
                {
                    record = dt.Rows[0]["record"].ObjToString();
                    G1.update_db_table("cust_extended", "record", record, new string[] { "serviceId", serviceId });
                }
                else
                {
                    record = G1.create_record("cust_extended", "serviceId", "-1");
                    if ( G1.BadRecord ( "cust_extended", record ))
                    {
                        MessageBox.Show("***ERROR*** Creating CustExtended for Contract " + contractNumber + "!", "Create Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                        return;
                    }
                    G1.update_db_table("cust_extended", "record", record, new string[] { "contractNumber", contractNumber, "serviceId", serviceId });
                }
            }
        }
        /***********************************************************************************************/
        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveAll(false);
        }
        /***********************************************************************************************/
        private void SaveAllOtherData(string record)
        {
            //string fname = txtFirstName.Text;
            //string lname = txtLastName.Text;
            //string mname = txtMiddleName.Text;
            //string suffix = txtSuffix.Text;
            //string prefix = txtPrefix.Text;
            //string ssn = txtSSN.Text;
            //ssn = ssn.Replace("-", "");
            //if (!String.IsNullOrWhiteSpace(ssn))
            //{
            //    if (ssn.Length == 4)
            //        ssn = "00000" + ssn;
            //    int rv = ValidateSSN(ssn);
            //    if (rv == 0)
            //        G1.update_db_table(customersFile, "record", record, new string[] { "ssn", ssn });
            //}
            //G1.update_db_table(customersFile, "record", record, new string[] { "firstName", fname, "lastName", lname, "middleName", mname, "suffix", suffix, "prefix", prefix });

            //string gender = comboGender.Text;
            //string dob = dateDOB.Text;
            //dob = G1.date_to_sql(dob);
            //dob = dob.Replace("-", "");

            //string race = comboRace.Text;
            //string maritalStatus = comboMaritalStatus.Text;
            //bool gotDeceased = false;
            //string deceasedDate = dateDeceased.Text;
            //if (deceasedDate == "0/0/0000")
            //{
            //    deceasedDate = "01/01/0001 12:01 AM";
            //    gotDeceased = true;
            //}
            //else
            //{
            //    deceasedDate = G1.date_to_sql(deceasedDate);
            //    deceasedDate = deceasedDate.Replace("-", "");
            //    gotDeceased = true;
            //}

            //string ethnicity = comboEthnicity.Text;
            //string language = comboLanguage.Text;
            //G1.update_db_table(customersFile, "record", record, new string[] { "birthDate", dob, "sex", gender, "ethnicity", ethnicity, "maritalstatus", maritalStatus, "race", race, "language", language, "deceasedDate", deceasedDate });

            //string address1 = textEdit_patientAddressLine1.Text;
            //string address2 = textEdit_patientAddressLine2.Text;
            //string city = textEdit_patientCity.Text;
            //string state = comboStates.Text;
            //string zip = textEdit_patientZipCode.Text;
            //G1.update_db_table(customersFile, "record", record, new string[] { "address1", address1, "address2", address2, "city", city, "state", state, "zip1", zip });

            //string phoneType1 = cmbPhoneQualifier1.Text;
            //string phoneType2 = cmbPhoneQualifier2.Text;
            //string phoneType3 = cmbPhoneQualifier3.Text;
            //string phone1 = txtPhone1.Text;
            //string phone2 = txtPhone2.Text;
            //string phone3 = txtPhone3.Text;
            //G1.update_db_table(customersFile, "record", record, new string[] { "phoneType1", phoneType1, "phoneType2", phoneType2, "phoneType3", phoneType3, "phoneNumber1", phone1, "phoneNumber2", phone2, "phoneNumber3", phone3 });

            string agentCode = txtAgentCode.Text;
            string name = txtAgentName.Text.Trim();
            if (!String.IsNullOrWhiteSpace(agentCode) && !String.IsNullOrWhiteSpace(name))
                G1.update_db_table(customersFile, "record", record, new string[] { "agentCode", agentCode });

            string meetingNumber = txtMeetingNumber.Text.Trim();
            if (!String.IsNullOrWhiteSpace(meetingNumber))
                G1.update_db_table(customersFile, "record", record, new string[] { "meetingNumber", meetingNumber });


            SaveContractExtras();

            //DataTable ddt = null;
            //if (gotDeceased)
            //{
            //    ddt = G1.get_db_data("Select * from `" + contractsFile + "` where `contractNumber` = '" + workContract + "';");
            //    if (ddt.Rows.Count > 0)
            //    {
            //        string contractRecord = ddt.Rows[0]["record"].ObjToString();
            //        G1.update_db_table(contractsFile, "record", contractRecord, new string[] { "deceasedDate", deceasedDate });
            //        ddt.Dispose();
            //        ddt = null;
            //    }
            //}

            DataTable ddt = G1.get_db_data("Select * from `" + contractsFile + "` where `contractNumber` = '" + workContract + "';");
            if (ddt.Rows.Count > 0)
            {
                string contractRecord = ddt.Rows[0]["record"].ObjToString();
                UpdateTrustPaid(contractRecord, dateMoneyPaid1.Text, cmbTrustPaid1.Text, "1");
                UpdateTrustPaid(contractRecord, dateMoneyPaid2.Text, cmbTrustPaid2.Text, "2");
                UpdateTrustPaid(contractRecord, dateMoneyPaid3.Text, cmbTrustPaid3.Text, "3");
                UpdateTrustPaid(contractRecord, dateMoneyPaid4.Text, cmbTrustPaid4.Text, "4");
                UpdateTrustPaid(contractRecord, dateMoneyPaid5.Text, cmbTrustPaid5.Text, "5");

                UpdateDatePaid(contractRecord, dateDPPaid.Text, "dateDPPaid");

                string trustRemoved = cmbRemove.Text;
                string trustRefunded = cmbRefund.Text;
                string removedDate = txtDOR.Text;
                if (removedDate == "0/0/0000")
                    removedDate = "01/01/0001 12:01 AM";
                else
                {
                    removedDate = G1.date_to_sql(removedDate);
                    removedDate = removedDate.Replace("-", "");
                }
                G1.update_db_table(contractsFile, "record", contractRecord, new string[] { "trustRemoved", trustRemoved, "trustRefunded", trustRefunded, "dateRemoved", removedDate });

                ddt.Dispose();
                ddt = null;
            }
            SaveACH();
            SaveCC();
        }
        /***********************************************************************************************/
        private void UpdateDatePaid(string contractRecord, string PaidDate, string PaidField)
        {
            if (PaidDate == "0/0/0000")
            {
                PaidDate = "01/01/0001 12:01 AM";
                G1.update_db_table(contractsFile, "record", contractRecord, new string[] { PaidField, PaidDate });
            }
            else
            {
                PaidDate = G1.date_to_sql(PaidDate);
                PaidDate = PaidDate.Replace("-", "");
                G1.update_db_table(contractsFile, "record", contractRecord, new string[] { PaidField, PaidDate });
            }
        }
        /***********************************************************************************************/
        private void UpdateTrustPaid(string contractRecord, string trustPaidDate, string trustPaid, string index)
        {
            //            string trustPaidDate = dateMoneyPaid1.Text;
            if (trustPaidDate == "0/0/0000")
            {
                trustPaidDate = "01/01/0001 12:01 AM";
                G1.update_db_table(contractsFile, "record", contractRecord, new string[] { "trustPaidDate" + index, trustPaidDate });
            }
            else
            {
                trustPaidDate = G1.date_to_sql(trustPaidDate);
                trustPaidDate = trustPaidDate.Replace("-", "");
                G1.update_db_table(contractsFile, "record", contractRecord, new string[] { "trustPaidDate" + index, trustPaidDate });
            }
            G1.update_db_table(contractsFile, "record", contractRecord, new string[] { "trustPaid" + index, trustPaid });
        }
        /***********************************************************************************************/
        private void txtFirstName_TextChanged(object sender, EventArgs e)
        {
            string firstName = txtFirstName.Trim();
            if (String.IsNullOrWhiteSpace(firstName))
            {
                btnSave.Enabled = false;
                btnSave.Hide();
            }
            else
                CheckEnableSave();
        }
        /***********************************************************************************************/
        private void txtLastName_TextChanged(object sender, EventArgs e)
        {
            string lastName = txtLastName.Trim();
            if (String.IsNullOrWhiteSpace(lastName))
            {
                btnSave.Enabled = false;
                btnSave.Hide();
            }
            else
                CheckEnableSave();
        }
        /***********************************************************************************************/
        private void pictureAdd_Click(object sender, EventArgs e)
        {
        }
        /***********************************************************************************************/
        private void attachFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }
        /***********************************************************************************************/
        private void repositoryItemCheckEdit1_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)(dgv.DataSource);
            DataRow dr = gridMain.GetFocusedDataRow();
            //            string contract = dr["claimNumber"].ObjToString();
            string value = dr["agreement"].ObjToString();
            if (value == "1")
            {
                string type = dr["type"].ObjToString();
                string date = dr["date"].ObjToString();
                string client = txtClientNumber.Text;
                string lname = txtLastName;

                string title = "( " + client + "-" + lname + ") " + date + " " + type;
                string record = dr["!imagesRecord"].ObjToString();
                if (record != "-1")
                    ShowPDfImage(record, title, title, workContract );
            }
        }
        /***********************************************************************************************/
        public static void ShowPDfImage(string record, string title, string filename, string workContract )
        {
            string command = "Select `image` from `pdfimages` where `Record` = '" + record + "';";
            MySqlCommand cmd1 = new MySqlCommand(command, G1.conn1);
            cmd1.Connection.Open();
            try
            {
                using (MySqlDataReader dr = cmd1.ExecuteReader(System.Data.CommandBehavior.Default))
                {
                    if (dr.Read())
                    {
                        byte[] fileData = (byte[])dr.GetValue(0);
                        ViewPDF pdfForm1 = new ViewPDF(title, record, workContract, fileData);
                        pdfForm1.Show();
                    }

                    dr.Close();
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                if (cmd1.Connection.State == ConnectionState.Open)
                    cmd1.Connection.Close();
            }
        }
        /***********************************************************************************************/
        private void clearAttachmentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            G1.update_db_table("client_attachments", "record", record, new string[] { "attachment", "", "!imagesRecord", "-1" });
            dr["agreement"] = "";
            dr["!imagesRecord"] = "-1";
            dgv.RefreshDataSource();
        }
        /***********************************************************************************************/
        private void attachFileToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)(dgv.DataSource);
            DataRow dr = gridMain.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            using (OpenFileDialog ofdImage = new OpenFileDialog())
            {
                ofdImage.Multiselect = false;

                //string file = "C:/Users/Robby/Documents/AMS/SMFS Robby Graham Agreement.pdf";
                //string record1 = "13";
                //ReadAndStorePDF("pdfimages", record1, file);

                if (ofdImage.ShowDialog() == DialogResult.OK)
                {
                    string filename = ofdImage.FileName;
                    filename = filename.Replace('\\', '/');
                    if (!String.IsNullOrWhiteSpace(filename))
                    {
                        G1.update_db_table("client_attachments", "record", record, new string[] { "attachment", filename });
                        string record1 = G1.create_record("pdfimages", "filename", "-1");
                        G1.update_db_table("pdfimages", "record", record1, new string[] { "filename", filename });
                        G1.ReadAndStorePDF("pdfimages", record1, filename);
                        G1.update_db_table("client_attachments", "record", record, new string[] { "!imagesRecord", record1 });
                        dr["agreement"] = "1";
                        dr["!imagesRecord"] = record1;
                        dr["attachment"] = filename;
                    }
                }
                dgv.Refresh();
                this.Refresh();
            }
        }
        /***********************************************************************************************/
        //private void ReadAndStorePDF(string table, string record, string filename)
        //{
        //    FileStream fStream = File.OpenRead(filename);
        //    byte[] contents = new byte[fStream.Length];
        //    fStream.Read(contents, 0, (int)fStream.Length);
        //    fStream.Close();
        //    G1.update_blob("pdfimages", "record", record, "image", contents);
        //}
        /***********************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            string type = dr["type"].ObjToString();
            string date = dr["date"].ObjToString();
            string client = txtClientNumber.Text;
            string lname = txtLastName;

            string title = "( " + client + "-" + lname + ") " + date + " " + type;
            //ClientDetailAttachments detailForm = new ClientDetailAttachments(workRecord, record, title);
            //detailForm.DetailDone += DetailForm_DetailDone;
            //detailForm.ShowDialog();
        }
        private void DetailForm_DetailDone(string s)
        {
            //            LoadAttachments();
        }
        /***********************************************************************************************/
        private void chartDelete_Click(object sender, EventArgs e)
        {
            //string answer = G1.getPreference(LoginForm.username, "CustomerDetails", "Delete");
            //if (answer != "YES")
            //{
            //    MessageBox.Show("***WARNING*** You do not have permission to Delete Customer detail information!");
            //    return;
            //}
            DataRow dr = gridMain.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            string imageRecord = dr["!imagesRecord"].ObjToString();
            string type = dr["type"].ObjToString();
            DialogResult result = MessageBox.Show("***Question*** Are you sure you want to delete this " + type + "?", "Delete Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;
            DeletePDFImage(imageRecord);
            G1.delete_db_table("client_attachments", "record", record);
            LoadData();
        }
        /***********************************************************************************************/
        public static void DeletePDFImage(string record)
        {
            if (String.IsNullOrWhiteSpace(record))
                return;
            if (!G1.validate_numeric(record))
                return;
            if (record == "-1")
                return;
            try
            {
                G1.delete_db_table("pdfimages", "record", record);
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Deleting pdfimage Record " + record + " Exception= " + ex.Message.ToString());
            }

        }
        /***********************************************************************************************/
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            //            LoadAttachments();
        }
        /***********************************************************************************************/
        private void pictureBox3_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            Services serviceForm = new Services(true, dt, workContract);
            serviceForm.SelectDone += ServiceForm_SelectDone;
            serviceForm.Show();
        }
        /***********************************************************************************************/
        private void ServiceForm_SelectDone(DataTable dt, string what )
        {
            SaveServices(dt);
            if (G1.get_column_number(dt, "num") < 0)
                dt.Columns.Add("num");
            G1.NumberDataTable(dt);
            DetermineServices(dt);
            dgv.DataSource = dt;
        }
        /***********************************************************************************************/
        private void pictureBox4_Click(object sender, EventArgs e)
        {
            if (gridMain.OptionsFind.AlwaysVisible == true)
                gridMain.OptionsFind.AlwaysVisible = false;
            else
                gridMain.OptionsFind.AlwaysVisible = true;
        }
        /***********************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (gridMain.OptionsFind.AlwaysVisible == true)
                gridMain.OptionsFind.AlwaysVisible = false;
            else
                gridMain.OptionsFind.AlwaysVisible = true;
        }
        /***********************************************************************************************/
        private void repositoryItemDateEdit1_CalendarTimeProperties_EditValueChanged(object sender, EventArgs e)
        {

        }
        /***********************************************************************************************/
        private void pictureBox2_Click(object sender, EventArgs e)
        {
            //string answer = G1.getPreference(LoginForm.username, "ClientDetails", "DeleteClaims");
            //if (answer != "YES")
            //{
            //    MessageBox.Show("***WARNING*** You do not have permission to Delete client claims information!");
            //    return;
            //}
            DataRow dr = gridMain.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            string desc = dr["service"].ObjToString();
            DialogResult result = MessageBox.Show("***Question*** Are you sure you want to delete this " + desc + "?", "Delete Claim Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;
            int rowHandle = gridMain.FocusedRowHandle;
            DataTable dt = (DataTable)dgv.DataSource;
            dt.Rows.RemoveAt(rowHandle);
            dgv.DataSource = dt;
        }
        /***********************************************************************************************/
        private void lockScreenPositionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            G1.SaveLocalPreferences(this, gridMain, LoginForm.username, "ClientDetailLayout");
            foundLocalPreference = true;
        }
        /***********************************************************************************************/
        private void unlockScreenPositionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            G1.RemoveLocalPreferences(LoginForm.username, "ClientDetailLayout");
            foundLocalPreference = false;
        }
        /****************************************************************************************/
        private bool checkForModified()
        {
            bool modified = false;
            if (editFunFamily != null)
            {
                familyModified = editFunFamily.FireEventFunServicesModified();
                if (familyModified)
                    modified = true;
            }
            if (editFunServices != null)
            {
                funModified = editFunServices.FireEventFunServicesModified();
                if (funModified)
                    modified = true;
            }
            if (editFunCustomer != null)
            {
                custModified = editFunCustomer.FireEventFunServicesModified();
                if (custModified)
                    modified = true;
            }
            if (btnSave.Visible)
            {
                modified = true;
                btnSave.Enabled = true;
            }
            return modified;
        }
        /***********************************************************************************************/
        private void CloseAllSubProcesses()
        {
            if (editFunFamily != null)
                editFunFamily.Close();
            if (editFunServices != null)
                editFunServices.Close();
            if (editFunCustomer != null)
                editFunCustomer.Close();
            editFunFamily = null;
            editFunServices = null;
            editFunCustomer = null;
        }
        /***********************************************************************************************/
        private void ClientDetails_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (checkForModified())
            {
                DialogResult result = MessageBox.Show("***Question***\nData has been modified!\nWould you like to save your changes?", "Data Modified Dialog", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
                if (result == DialogResult.No)
                {
                    CloseAllSubProcesses();
                    return;
                }
            }

            if (btnSave.Visible)
            {
                if (!btnSave.Enabled || CCDateError )
                {
                    MessageBox.Show("***ERROR*** Customer cannot be saved because of missing data or data error.");
                    return;
                }
                SaveAll(true);
                //btnSave_Click(null, null);
            }

            if (editFunCustomer != null)
            {
                funModified = editFunCustomer.FireEventFunServicesModified();
                if (funModified)
                {
                    editFunCustomer.FireEventSaveFunServices(true);
                    btnSave.Hide();
                }
            }
            if (editFunServices != null)
            {
                funModified = editFunServices.FireEventFunServicesModified();
                if (funModified)
                {
                    editFunServices.FireEventSaveFunServices(true);
                    btnSaveServices.Hide();
                }
            }

            if (editFunFamily != null)
            {
                familyModified = editFunFamily.FireEventFunServicesModified();
                if (familyModified)
                {
                    editFunFamily.FireEventSaveFunServices(true);
                    familyModified = false;
                }
            }

            CloseAllSubProcesses();

            if (foundLocalPreference)
                G1.SaveLocalPreferences(this, gridMain, LoginForm.username, "ClientDetailLayout");
            if (dailyForm != null)
                dailyForm.Close();
            dailyForm = null;

            G1.CleanupDataGrid(ref dgv);
            GC.Collect();
            G1.CleanupDataGrid(ref dgv2);
            GC.Collect();
            G1.CleanupDataGrid(ref dgv3);
            GC.Collect();
            G1.CleanupDataGrid(ref dgv4);
            GC.Collect();
            G1.CleanupDataGrid(ref dgv5);
            GC.Collect();
            G1.CleanupDataGrid(ref dgv7);
            GC.Collect();

            if ( this.PatientPicture.Image != null )
            {
                this.PatientPicture.Image.Dispose();
                this.PatientPicture.Image = null;
                GC.Collect();
            }
        }
        /***********************************************************************************************/
        private void btnSelectMerchandise_Click(object sender, EventArgs e)
        {
            InventoryList inventForm = new InventoryList(true);
            inventForm.ModuleDone += InventForm_ModuleDone;
            inventForm.Show();
        }
        /***********************************************************************************************/
        private void InventForm_ModuleDone(string s)
        {
            string merchandiseRecord = s;
            if (String.IsNullOrWhiteSpace(merchandiseRecord))
                return;
            string cmd = "Select * from `inventorylist` where `record` = '" + merchandiseRecord + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count < 0)
                return;
            string casketDescription = dt.Rows[0]["casketdesc"].ObjToString();
            string casketCost = dt.Rows[0]["casketcost"].ObjToString();
            string casketType = dt.Rows[0]["caskettype"].ObjToString();
            string record = "";

            if (btnSaveServices.Visible)
            {
                dt = (DataTable)dgv.DataSource;
                SaveServices(dt);
            }

            cmd = "Select * from `cust_services` where `service` = 'Casket Name' and `contractNumber` = '" + workContract + "';";
            dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
                record = dt.Rows[0]["record"].ObjToString();
            else
                record = G1.create_record("cust_services", "type", "-1");
            if (!G1.BadRecord("cust_services", record))
                G1.update_db_table("cust_services", "record", record, new string[] { "service", "Casket Name", "data", casketDescription, "type", "Merchandise", "contractNumber", workContract });

            cmd = "Select * from `cust_services` where `service` = 'Casket Price' and `contractNumber` = '" + workContract + "';";
            dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
                record = dt.Rows[0]["record"].ObjToString();
            else
                record = G1.create_record("cust_services", "type", "-1");
            if (!G1.BadRecord("cust_services", record))
                G1.update_db_table("cust_services", "record", record, new string[] { "service", "Casket Price", "data", casketCost, "type", "Merchandise", "contractNumber", workContract });

            cmd = "Select * from `cust_services` where `service` = 'Casket Description' and `contractNumber` = '" + workContract + "';";
            dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
                record = dt.Rows[0]["record"].ObjToString();
            else
                record = G1.create_record("cust_services", "type", "-1");
            if (!G1.BadRecord("cust_services", record))
                G1.update_db_table("cust_services", "record", record, new string[] { "service", "Casket Description", "data", casketType, "type", "Merchandise", "contractNumber", workContract });

            LoadServices();
        }
        /***********************************************************************************************/
        private void pictureBox2_Click_1(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            DialogResult result = MessageBox.Show("***Question***\nAre you sure you want to DELETE this service?", "Delete Services Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;
            G1.delete_db_table("cust_services", "record", record);

            int row = gridMain.FocusedRowHandle;
            int dtRow = gridMain.GetDataSourceRowIndex(row);
            DataTable dt = (DataTable)dgv.DataSource;
            dt.Rows.RemoveAt(dtRow);
            G1.NumberDataTable(dt);
            DetermineServices(dt);
            dgv.DataSource = dt;
            dgv.RefreshDataSource();
            dgv.Refresh();
        }
        /***********************************************************************************************/
        private void btnTie_Click(object sender, EventArgs e)
        {
            using (TieCustomers searchForm = new TieCustomers(workContract, workPayer))
            {
                searchForm.ShowDialog();
                //if (searchForm.DialogResult != DialogResult.OK)
                //    return;
            }
        }
        /***********************************************************************************************/
        private void ShowTiedCustomer()
        {
            lblTiedCustomer.Hide();
            string cmd = "Select * from `tied_customers` where `contractNumber` = '" + workContract + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                lblTiedCustomer.Text = "There appears to be " + (dt.Rows.Count - 1).ToString() + " Customers Tied to this one!";
                lblTiedCustomer.Show();
                return;
            }
            else
                btnTie.Show();
        }
        /***********************************************************************************************/
        private void untieCustomerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string cmd = "Select * from `tied_customers` where `contractNumber` = '" + workContract + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                string record = dt.Rows[0]["record"].ObjToString();
                G1.delete_db_table("tied_customers", "record", record);
                ShowTiedCustomer();
            }
        }
        /***********************************************************************************************/
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        /***********************************************************************************************/
        private void xtraTabControl1_SelectedPageChanged(object sender, DevExpress.XtraTab.TabPageChangedEventArgs e)
        {
            DevExpress.XtraTab.XtraTabControl tabControl = (DevExpress.XtraTab.XtraTabControl)sender;
            DevExpress.XtraTab.XtraTabPage page = tabControl.SelectedTabPage;
            if (page.Name.ObjToString().ToUpper() == "TABCONTRACT")
            {
                LoadContract();
            }
            else if (page.Name.ObjToString().ToUpper() == "TABEXTENDED")
            {
                LoadExtended();
            }
            else if (page.Name.ObjToString().ToUpper() == "TABDAILYHISTORY")
            {
                if (dailyForm != null)
                    dailyForm.FireEventReloadHeader();
            }
            else if (page.Name.ObjToString().ToUpper() == "TABMERCHANDISE")
            {
                this.Cursor = Cursors.WaitCursor;
                if (editFunServices == null)
                    InitializeServicePanel();
                this.Cursor = Cursors.Default;
            }
            else if (page.Name.ObjToString().ToUpper() == "TABAGREEMENTS")
            {
                this.Cursor = Cursors.WaitCursor;
                if (dgv3.DataSource == null)
                    LoadAgreements();
                this.Cursor = Cursors.Default;
            }
            else if (page.Name.ObjToString().ToUpper() == "TABNOTICES")
            {
                this.Cursor = Cursors.WaitCursor;
                if (dgv6.DataSource == null)
                    LoadNotices();
                this.Cursor = Cursors.Default;
            }
            else if (page.Name.ObjToString().ToUpper() == "TABREQUESTS")
            {
                this.Cursor = Cursors.WaitCursor;
                LoadRequests();
                this.Cursor = Cursors.Default;
            }
        }
        /***********************************************************************************************/
        private bool contractModified = false;
        private bool loadingContract = true;
        private void LoadContract()
        {
            btnSaveContract.Hide();
            if (String.IsNullOrWhiteSpace(workContract))
                return;
            DataTable dx = new DataTable();
            dx.Columns.Add("num");
            dx.Columns.Add("mod");
            dx.Columns.Add("parameter");
            dx.Columns.Add("type");
            dx.Columns.Add("data");
            dx.Columns.Add("customer");
            dx.Columns.Add("customerData");
            dx.Columns.Add("mod2");

            contractModified = false;

            string parameter = "";

            try
            {
                DataTable dt = G1.get_db_data("Select * from `" + contractsFile + "` where `contractNumber` = '" + workContract + "';");
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    parameter = dt.Columns[i].ColumnName.ObjToString();
                    if (parameter.ToUpper() == "TMSTAMP")
                        continue;
                    if (parameter.ToUpper() == "RECORD")
                        continue;
                    if (parameter.ToUpper() == "CONTRACTNUMBER")
                        continue;
                    DataRow dRow = dx.NewRow();
                    dRow["parameter"] = dt.Columns[i].ColumnName.ObjToString();
                    dRow["type"] = dt.Columns[i].DataType.ObjToString();
                    dRow["data"] = dt.Rows[0][i].ObjToString();
                    dRow["customer"] = "";
                    dRow["customerData"] = "";
                    dx.Rows.Add(dRow);
                }
            }
            catch ( Exception ex)
            {
            }

            try
            {
                DataTable ddt = G1.get_db_data("Select * from `" + customersFile + "` where `contractNumber` = '" + workContract + "';");
                if (ddt.Rows.Count > 0)
                {
                    string oldCasket = ddt.Rows[0]["extraItemAmtMI1"].ObjToString();
                    string oldVault = ddt.Rows[0]["extraItemAmtMI2"].ObjToString();
                    string oldCasketPrice = ddt.Rows[0]["extraItemAmtMR1"].ObjToString();
                    string oldVaultPrice = ddt.Rows[0]["extraItemAmtMR2"].ObjToString();

                    dx.Rows[0]["customer"] = "Imported Casket";
                    dx.Rows[0]["customerData"] = oldCasket;
                    dx.Rows[1]["customer"] = "Imported Casket Price";
                    dx.Rows[1]["customerData"] = oldCasketPrice;

                    dx.Rows[2]["customer"] = "Imported Vault";
                    dx.Rows[2]["customerData"] = oldVault;
                    dx.Rows[3]["customer"] = "Imported Vault Price";
                    dx.Rows[3]["customerData"] = oldVaultPrice;
                }
            }
            catch ( Exception ex)
            {
            }

            if ( DailyHistory.isInsurance ( workContract ))
            {
                double premium = Policies.CalcMonthlyPremium(workPayer, DateTime.Now);
                DataRow[] dRows = dx.Select("parameter='amtOfMonthlyPayt'");
                if (dRows.Length > 0)
                {
                    string money = G1.ReformatMoney(premium);
                    dRows[0]["data"] = money;
                }

                double balanceDue = DailyHistory.GetDueNow(workPayer, premium);
                dRows = dx.Select("parameter='BalanceDue'");
                if ( dRows.Length > 0 )
                {
                    string money = G1.ReformatMoney(balanceDue);
                    dRows[0]["data"] = money;
                }
                dRows = dx.Select("parameter='nowDue'");
                if (dRows.Length > 0)
                {
                    string money = G1.ReformatMoney(balanceDue);
                    dRows[0]["data"] = money;
                }
            }
            G1.NumberDataTable(dx);
            dgv2.DataSource = dx;
            contractModified = false;
            loadingContract = false;
        }
        /***********************************************************************************************/
        private void gridMain2_KeyUp(object sender, KeyEventArgs e)
        {
            if (loadingContract)
                return;
            //contractModified = true;
            //DataRow dr = gridMain2.GetFocusedDataRow();
            //dr["mod"] = "M";
            //string type = dr["type"].ObjToString();
            //string data = dr["data"].ObjToString();
            //if (type.ToUpper().IndexOf("DECIMAL") >= 0)
            //{
            //    double dvalue = data.ObjToDouble();
            //    dr["data"] = dvalue.ToString("###.00");
            //}
            //else if (type.ToUpper().IndexOf("MYSQLDATETIME") >= 0)
            //{
            //    if (data.IndexOf("0000") >= 0)
            //    {
            //        dr["data"] = "0/0/0000";
            //    }
            //    else
            //    {
            //        if (G1.validate_date(data))
            //        {
            //            DateTime date = data.ObjToDateTime();
            //            try
            //            {
            //                MySqlDateTime myDate = (MySqlDateTime)G1.DTtoMySQLDT(date);
            //                dr["data"] = myDate.Month.ToString("D2") + "/" + myDate.Day.ToString("D2") + "/" + myDate.Year.ToString("D4");
            //            }
            //            catch
            //            {

            //            }
            //        }
            //    }
            //}
            //btnSaveContract.Show();
        }
        /***********************************************************************************************/
        private void btnSaveContract_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv2.DataSource;
            if (dt == null)
                return;
            this.Cursor = Cursors.WaitCursor;
            string saveItems = "";
            string mod = "";
            string parameter = "";
            string data = "";
            string str = "";
            DataTable ddt = G1.get_db_data("Select * from `" + contractsFile + "` where `contractNumber` = '" + workContract + "';");
            string oldData = "";
            string oldAPR = "";
            string newAPR = "";

            string mod2 = "";

            var myList = new List<string>();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                mod = dt.Rows[i]["mod2"].ObjToString();
                if (mod == "M")
                    mod2 = "Y";

                mod = dt.Rows[i]["mod"].ObjToString();
                if (mod != "M")
                    continue;
                parameter = dt.Rows[i]["parameter"].ObjToString();
                data = dt.Rows[i]["data"].ObjToString();
                oldData = "";
                if (ddt.Rows.Count > 0)
                {
                    oldData = ddt.Rows[0][parameter].ObjToString();
                    str = "Changed Contract " + workContract + " " + parameter + " from " + oldData + " to " + data + ".";
                    G1.AddToAudit(LoginForm.username, "CustomerDetails", parameter, str, workContract);
                    if ( parameter.ToUpper() == "APR")
                    {
                        oldAPR = oldData;
                        newAPR = data;
                    }
                }
                if (!String.IsNullOrWhiteSpace(parameter))
                {
                    saveItems += parameter + "," + data + ",";
                    myList.Add(parameter);
                    myList.Add(data);
                }
            }
            saveItems = saveItems.TrimEnd(',');
            if (ddt.Rows.Count > 0)
            {
                // Create list

                // Add items to the list

                // Convert to array
                var myArray = myList.ToArray();
                string contractRecord = ddt.Rows[0]["record"].ObjToString();
                try
                {
                    G1.update_db_table(contractsFile, "record", contractRecord, myArray);
                    UpdatePayersDetail(workContract);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("**ERRROR*** Updaing Contract!\nCall I/T");
                }
                ddt.Dispose();
                ddt = null;
            }

            if ( mod2 == "Y" )
            {
                ddt = G1.get_db_data("Select * from `" + customersFile + "` where `contractNumber` = '" + workContract + "';");
                if ( ddt.Rows.Count > 0 )
                {
                    string record = ddt.Rows[0]["record"].ObjToString();
                    string oldCasket = dt.Rows[0]["customerData"].ObjToString();
                    string oldCasketPrice = dt.Rows[1]["customerData"].ObjToString();
                    string oldVault = dt.Rows[2]["customerData"].ObjToString();
                    string oldVaultPrice = dt.Rows[3]["customerData"].ObjToString();

                    G1.update_db_table(customersFile, "record", record, new string[] { "extraItemAmtMI1", oldCasket, "extraItemAmtMR1", oldCasketPrice, "extraItemAmtMI2", oldVault, "extraItemAmtMR2", oldVaultPrice });
                }
            }
            if ( contractsFile.ToUpper() == "CONTRACTS")
            {
                LockPaymentHistory(workContract, oldData );
            }

            btnSaveContract.Hide();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void LockPaymentHistory ( string contractNumber, string oldAPR )
        {
            string record = "";
            //string cmd = "Select * from `payments` where `contractNumber` = '" + contractNumber + "';";
            //DataTable dt = G1.get_db_data(cmd);

            //decimal rate = oldAPR.ObjToDecimal() / (decimal)100D;
            //for ( int i=0; i<dt.Rows.Count; i++)
            //{
            //    record = dt.Rows[i]["record"].ObjToString();
            //    if (String.IsNullOrWhiteSpace(record))
            //        continue;
            //    G1.update_db_table("payments", "record", record, new string[] {"apr", rate.ToString(), "lockInterest", "Y" });
            //}
        }
        /***********************************************************************************************/
        public static void UpdatePayersDetail(string contractNumber)
        {
            bool insurance = DailyHistory.isInsurance(contractNumber);
            if (!insurance)
                return;
            string cmd = "Select * from `icontracts` where `contractNumber` = '" + contractNumber + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                DateTime lapsedDate = dx.Rows[0]["lapseDate8"].ObjToDateTime();
                DateTime reinstateDate = dx.Rows[0]["reinstateDate8"].ObjToDateTime();
                DateTime deceasedDate = dx.Rows[0]["deceasedDate"].ObjToDateTime();
                DateTime dueDate = dx.Rows[0]["dueDate8"].ObjToDateTime();
                DateTime DOLP = dx.Rows[0]["lastDatePaid8"].ObjToDateTime();
                double premium = dx.Rows[0]["amtOfMonthlyPayt"].ObjToDouble();
                double annualPremium = dx.Rows[0]["annualPremium"].ObjToDouble();
                string lapsed = dx.Rows[0]["lapsed"].ObjToString();
                cmd = "Select * from `payers` where `contractNumber` = '" + contractNumber + "';";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    string record = dx.Rows[0]["record"].ObjToString();
                    G1.update_db_table("payers", "record", record, new string[] { "amtOfMonthlyPayt", premium.ToString(), "annualPremium", annualPremium.ToString(), "dueDate8", dueDate.ToString("MM/dd/yyyy"), "lastDatePaid8", DOLP.ToString("MM/dd/yyyy"), "lapsed", lapsed, "empty", "", "lapseDate8", lapsedDate.ToString("MM/dd/yyyy"), "reinstateDate8", reinstateDate.ToString("MM/dd/yyyy"), "deceasedDate", deceasedDate.ToString("MM/dd/yyyy") });
                }
                cmd = "Select * from `icustomers` where `contractNumber` = '" + contractNumber + "';";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    string record = dx.Rows[0]["record"].ObjToString();
                    G1.update_db_table("icustomers", "record", record, new string[] { "lapsed", lapsed, "deceasedDate", deceasedDate.ToString("MM/dd/yyyy") });
                }
            }
        }
        /***********************************************************************************************/
        private void UpdatePayersName(string contractNumber)
        {
            bool insurance = DailyHistory.isInsurance(contractNumber);
            if (!insurance)
                return;
            string cmd = "Select * from `icustomers` where `contractNumber` = '" + contractNumber + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                string firstName = dx.Rows[0]["firstName"].ObjToString();
                string lastName = dx.Rows[0]["lastName"].ObjToString();
                string payer = dx.Rows[0]["payer"].ObjToString();
                if (!String.IsNullOrWhiteSpace(payer))
                {
                    cmd = "Select * from `payers` where `payer` = '" + payer + "';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                    {
                        string record = dx.Rows[0]["record"].ObjToString();
                        G1.update_db_table("payers", "record", record, new string[] { "firstName", firstName, "lastName", lastName });
                    }
                }
            }
        }
        /***********************************************************************************************/
        private void gridMain_RowCellStyle(object sender, RowCellStyleEventArgs e)
        {
            GridView View = sender as GridView;
            if (e.Column.FieldName.ToUpper() == "DATA")
            {
                string str = View.GetRowCellValue(e.RowHandle, "data").ObjToString();
                if (str != null)
                {
                    if (G1.validate_numeric(str))
                        e.Appearance.TextOptions.HAlignment = HorzAlignment.Far;
                }
            }
        }
        /***********************************************************************************************/
        private void gridMain_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
        {
            if (e.Column.FieldName.ToUpper() == "DATA")
            {
                if (e.RowHandle >= 0)
                {
                    string data = e.DisplayText.ObjToString();
                    if (G1.validate_numeric(data))
                    {
                        double dvalue = data.ObjToDouble();
                        e.DisplayText = G1.ReformatMoney(dvalue);
                    }
                }
            }
        }
        /***********************************************************************************************/
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            btnSaveServices.Show();
        }
        /***********************************************************************************************/
        private void SaveServices(DataTable dt)
        {
            string service = "";
            string data = "";
            string type = "";
            string record = "";
            string cmd = "Delete from `cust_services` where `contractNumber` = '" + workContract + "';";
            G1.get_db_data(cmd);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dt.Rows[i]["record"] = "0";
                service = dt.Rows[i]["service"].ObjToString();
                data = dt.Rows[i]["data"].ObjToString();
                type = dt.Rows[i]["type"].ObjToString();
                //                double price = dt.Rows[i]["price"].ObjToDouble();
                record = G1.create_record("cust_services", "service", "-1");
                if (String.IsNullOrWhiteSpace(record))
                    continue;
                if (record == "0" || record == "-1")
                    continue;
                dt.Rows[i]["record"] = record;
                G1.update_db_table("cust_services", "record", record, new string[] { "service", service, "data", data, "type", type, "contractNumber", workContract });
            }
            if (G1.get_column_number(dt, "num") < 0)
                dt.Columns.Add("num");
            G1.NumberDataTable(dt);
        }
        /***********************************************************************************************/
        private void btnSaveServices_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            SaveServices(dt);
            btnSaveServices.Hide();
        }
        /***********************************************************************************************/
        private void simpleButton2_Click(object sender, EventArgs e)
        {
            //            dateDeceased.Text = "0/0/0000";
            btnSave.Enabled = true;
            btnSave.Show();
        }
        /***********************************************************************************************/
        private int ValidateSSN(string ssn)
        {
            ssn = ssn.Replace("-", "");
            if (!G1.validate_numeric(ssn))
                return 1;
            if (ssn.Length != 9)
                return 2;
            return 0;
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
            if (dgv2.Visible)
                printableComponentLink1.Component = dgv2;
            if (dgv5.Visible)
                printableComponentLink1.Component = dgv5;
            if (dgv7.Visible)
                printableComponentLink1.Component = dgv7;

            if ( workWhat == "Policies")
                printableComponentLink1.Component = dgv5;

            printableComponentLink1.PrintingSystemBase = printingSystem1;

            printableComponentLink1.EnablePageDialog = true;

            printableComponentLink1.CreateDetailHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateDetailHeaderArea);
            printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);
            printableComponentLink1.BeforeCreateAreas += new System.EventHandler(this.printableComponentLink1_BeforeCreateAreas);
            printableComponentLink1.AfterCreateAreas += new System.EventHandler(this.printableComponentLink1_AfterCreateAreas);
            printableComponentLink1.Landscape = false;
            if (dgv5.Visible || dgv7.Visible|| workWhat == "Policies" )
            {
                //printingSystem1.Document.AutoFitToPagesWidth = 1;
                printableComponentLink1.Landscape = true;
            }

            Printer.setupPrinterMargins(10, 5, 80, 50);

            pageMarginLeft = Printer.pageMarginLeft;
            pageMarginRight = Printer.pageMarginRight;
            pageMarginTop = Printer.pageMarginTop;
            pageMarginBottom = Printer.pageMarginBottom;

            printableComponentLink1.Margins.Left = pageMarginLeft;
            printableComponentLink1.Margins.Right = pageMarginRight;
            printableComponentLink1.Margins.Top = pageMarginTop;
            printableComponentLink1.Margins.Bottom = pageMarginBottom;

            if (dgv5.Visible)
                G1.AdjustColumnWidths(gridMain5, 0.65D, true);
            else if (dgv7.Visible)
                G1.AdjustColumnWidths(gridMain7, 0.65D, true);

            printableComponentLink1.CreateDocument();

            if (workPDF)
            {
                string filename = "";
                //string filename = path + @"\" + workReport + "_" + today.Year.ToString("D4") + today.Month.ToString("D2") + today.Day.ToString("D2") + ".pdf";
                filename = workPDFfile;
                if (File.Exists(filename))
                {
                    File.SetAttributes(filename, FileAttributes.Normal);
                    File.Delete(filename);
                }
                printableComponentLink1.ExportToPdf(filename);
                return;
            }
            else
                printableComponentLink1.ShowPreview();

            if (dgv5.Visible)
                G1.AdjustColumnWidths(gridMain5, 0.65D, false );
            else if (dgv7.Visible) 
                G1.AdjustColumnWidths(gridMain7, 0.65D, false );
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
            if (dgv2.Visible)
                printableComponentLink1.Component = dgv2;
            if (dgv5.Visible)
                printableComponentLink1.Component = dgv5;
            if (dgv7.Visible)
                printableComponentLink1.Component = dgv7;

            printableComponentLink1.PrintingSystemBase = printingSystem1;
            printableComponentLink1.CreateDetailHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateDetailHeaderArea);
            printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);
            printableComponentLink1.BeforeCreateAreas += new System.EventHandler(this.printableComponentLink1_BeforeCreateAreas);
            printableComponentLink1.AfterCreateAreas += new System.EventHandler(this.printableComponentLink1_AfterCreateAreas);
            printableComponentLink1.Landscape = false;
            if (dgv5.Visible || dgv7.Visible)
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

            font = new Font("Ariel", 10, FontStyle.Regular);
            if (dgv.Visible)
                Printer.DrawQuad(6, 8, 4, 4, "Services Report for (" + workContract + ")", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            else if (dgv2.Visible)
                Printer.DrawQuad(6, 8, 4, 4, "Contract Report for (" + workContract + ")", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            else if (dgv5.Visible)
                Printer.DrawQuad(5, 8, 5, 4, "Payer/Policy Summary for (" + workPayer + " / " + workContract + ")", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            else if (dgv7.Visible)
                Printer.DrawQuad(5, 8, 5, 4, "Reinstatement Request for (" + workPayer + " / " + workContract + ")", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);


            //            Printer.DrawQuadTicks();
            DateTime date = DateTime.Now;
            string workDate = date.Month.ToString("D2") + "/" + date.Year.ToString("D4");
            Printer.SetQuadSize(24, 12);
            font = new Font("Ariel", 9, FontStyle.Bold);
            //            Printer.DrawQuad(20, 8, 5, 4, "Report Month:" + workDate, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(16, 8, 3, 4, lblPayment.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(19, 8, 3, 4, lblTrust85.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(22, 8, 3, 4, lblTrust100.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /***********************************************************************************************/
        private void txtAgentCode_EditValueChanged(object sender, EventArgs e)
        {
            bool askForChange = false;
            string name = "";
            string agentCode = txtAgentCode.Text.Trim();
            if (!String.IsNullOrWhiteSpace(agentCode))
            {
                name = GetAgentName(agentCode);
                if (!String.IsNullOrWhiteSpace(name))
                {
                    txtAgentName.Text = name;
                    askForChange = true;
                }
            }
            if (loading)
                return;
            if (askForChange)
            {
                DialogResult result = MessageBox.Show("Do you want to change all previous payment to this agent (" + name + ") ???", "Update Old Payment Agent Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
                if (result == DialogResult.No)
                    return;
                string record = "";
                string data = "agentNumber," + agentCode;
                string cmd = "Select * from `payments` where `contractNumber` = '" + workContract + "';";
                DataTable dx = G1.get_db_data(cmd);
                for (int i = 0; i < dx.Rows.Count; i++)
                {
                    record = dx.Rows[i]["record"].ObjToString();
                    G1.update_db_table(paymentsFile, "record", record, data);
                }
            }
        }
        /***********************************************************************************************/
        private void LoadRelatives()
        {
            funModified = false;
            InitializeFamilyPanel();
            //string cmd = "Select * from `relatives` where `contractNumber` = '" + workContract + "';";
            //DataTable dt = G1.get_db_data(cmd);
            //dt.Columns.Add("num");
            //dt.Columns.Add("mod");
            //G1.NumberDataTable(dt);
            //dgvDependent.DataSource = dt;
        }
        /***********************************************************************************************/
        private FunCustomer editFunCustomer = null;
        private bool custModified = false;
        private void InitializeCustomerPanel()
        {
            if (editFunCustomer != null)
                editFunCustomer.Close();
            editFunCustomer = null;
            custModified = false;
            G1.ClearPanelControls(this.panelCustomer);

            editFunCustomer = new FunCustomer(workContract, "", true );
            editFunCustomer.CustomerModifiedDone += EditFunCustomer_CustomerModifiedDone;
            if (!this.LookAndFeel.UseDefaultLookAndFeel)
            {
                editFunCustomer.LookAndFeel.UseDefaultLookAndFeel = false;
                editFunCustomer.LookAndFeel.SetSkinStyle(this.LookAndFeel.SkinName);
            }
            G1.LoadFormInPanel(editFunCustomer, this.panelCustomer);
        }
        /***********************************************************************************************/
        private void EditFunCustomer_CustomerModifiedDone(string s)
        { // Customer Demographics Changed / Update Header
            string cmd = "Select * from `contracts` WHERE `contractNumber` = '" + workContract + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                DateTime deceasedDate = dt.Rows[0]["deceasedDate"].ObjToDateTime();
                string serviceId = dt.Rows[0]["serviceId"].ObjToString();
                if (deceasedDate.Year > 1000 && !String.IsNullOrWhiteSpace(serviceId))
                {
                    btnFuneral.Show();
                    string ssn = lblSSN.Text.Trim();
                    ssn = ssn.Replace("SSN # :", "");
                    cmd = "Select * from `fcustomers` WHERE `ssn` = '" + ssn + "';";
                    DataTable dx = G1.get_db_data(cmd);
                    if (dt.Rows.Count <= 0)
                    {
                        DialogResult result = MessageBox.Show("This looks like a new Funeral.\nAre you wanting to setup a new Funeral from this Pre-Need Contract?", "New Funeral Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                        if (result == DialogResult.No)
                            return;
                        CopyAllContractInfo(workContract);
                        this.Cursor = Cursors.WaitCursor;
                        EditCust editForm = new EditCust(workContract);
                        editForm.Show();
                        this.Cursor = Cursors.Default;
                    }
                }
                else
                    btnFuneral.Hide();
            }
            else
                btnFuneral.Hide();
            custModified = true;
            btnSave.Enabled = true;
            btnSave.Visible = true;
        }
        /***********************************************************************************************/
        private FunFamily editFunFamily = null;
        private bool familyModified = false;
        private void InitializeFamilyPanel()
        {
            if (editFunFamily != null)
                editFunFamily.Close();
            editFunFamily = null;
            familyModified = false;
            G1.ClearPanelControls(this.panelFamilyAll);

            editFunFamily = new FunFamily(workContract, "ALL", false);
            editFunFamily.FamilyModifiedDone += EditFunFamily_FamilyModifiedDone;
            if (!this.LookAndFeel.UseDefaultLookAndFeel)
            {
                editFunFamily.LookAndFeel.UseDefaultLookAndFeel = false;
                editFunFamily.LookAndFeel.SetSkinStyle(this.LookAndFeel.SkinName);
            }
            G1.LoadFormInPanel(editFunFamily, this.panelFamilyAll);
            //this.panelFamilyAll.Dock = DockStyle.Top;
            this.panelFamilyAll.Dock = DockStyle.Fill;
        }
        /***********************************************************************************************/
        private void EditFunFamily_FamilyModifiedDone(string s)
        {
            familyModified = true;
            btnSave.Enabled = true;
            btnSave.Visible = true;
        }
        /***********************************************************************************************/
        private void LoadAgreements()
        {
            relativesChanged = false;
            string cmd = "Select * from `agreements` where `contractNumber` = '" + workContract + "';";
            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("num");
            dt.Columns.Add("mod");
            G1.NumberDataTable(dt);
            dgv3.DataSource = dt;
        }
        /***********************************************************************************************/
        private bool CheckForContract ()
        {
            string cmd = "Select * from `lapse_list` where `contractNumber` = '" + workContract + "' AND `detail` = 'Goods and Services';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
                return true;
            return false;
        }
        /***********************************************************************************************/
        private void LoadNotices()
        {
            string cmd = "Select * from `lapse_list` where `contractNumber` = '" + workContract + "' order by noticeDate desc;";
            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("num");
            dt.Columns.Add("sDate");
            string date = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                date = dt.Rows[i]["noticeDate"].ObjToDateTime().ToString("MM/dd/yyyy");
                date = dt.Rows[i]["tmstamp"].ObjToDateTime().ToString("MM/dd/yyyy");
                dt.Rows[i]["sDate"] = date;
            }
            G1.NumberDataTable(dt);
            dgv6.DataSource = dt;
        }
        /***********************************************************************************************/
        private void LoadRequests()
        {
            string cmd = "Select * from `reinstate_requests` where `contractNumber` = '" + workContract + "' order by requestNumber desc;";
            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("num");
            G1.NumberDataTable(dt);
            dgv7.DataSource = dt;
        }
        /***********************************************************************************************/
        private bool removeAutodraft = false;
        private string autoDraftRecord = "";
        //private void SaveAutoDraft()
        //{
        //    int day = textDOM.Text.ObjToInt32();
        //    double money = textAutoAmt.Text.ObjToDouble();
        //    if (day <= 0 || money <= 0D)
        //    {
        //        if (!String.IsNullOrWhiteSpace(autoDraftRecord))
        //            G1.delete_db_table("autodraft", "record", autoDraftRecord);
        //    }
        //    else
        //    {
        //        if (String.IsNullOrWhiteSpace(autoDraftRecord))
        //            autoDraftRecord = G1.create_record("autodraft", "contractNumber", "-1");
        //        if (G1.BadRecord("autodraft", autoDraftRecord))
        //            return;
        //        G1.update_db_table("autodraft", "record", autoDraftRecord, new string[] { "contractNumber", workContract, "autodraftDay", day.ToString(), "autodraftAmount", money.ToString() });
        //    }
        //}
        /***********************************************************************************************/
        private void SaveRelatives()
        {
            //string record = "";
            //string mod = "";
            //string firstName = "";
            //string lastName = "";
            //string mi = "";
            //string suffix = "";
            //string dob = "";
            //string dod = "";
            //string relationship = "";
            //string maidenName = "";
            //string city = "";
            //string spouseFirstName = "";
            //DataTable dt = (DataTable)dgvDependent.DataSource;
            //for (int i = 0; i < dt.Rows.Count; i++)
            //{
            //    record = dt.Rows[i]["record"].ObjToString();
            //    if (String.IsNullOrWhiteSpace(record))
            //        record = G1.create_record("relatives", "depFirstName", "-1");
            //    if (G1.BadRecord("relatives", record))
            //        return;
            //    firstName = dt.Rows[i]["depFirstName"].ObjToString();
            //    lastName = dt.Rows[i]["depLastName"].ObjToString();
            //    mi = dt.Rows[i]["depMI"].ObjToString();
            //    suffix = dt.Rows[i]["depSuffix"].ObjToString();
            //    dob = dt.Rows[i]["depDOB"].ObjToString();
            //    dod = dt.Rows[i]["depDOD"].ObjToString();
            //    maidenName = dt.Rows[i]["maidenName"].ObjToString();
            //    relationship = dt.Rows[i]["depRelationship"].ObjToString();
            //    city = dt.Rows[i]["city"].ObjToString();
            //    spouseFirstName = dt.Rows[i]["spouseFirstName"].ObjToString();

            //    G1.update_db_table("relatives", "record", record, new string[] { "depFirstName", firstName, "depLastName", lastName, "contractNumber", workContract, "depMI", mi, "depSuffix", suffix, "depDOB", dob, "depRelationship", relationship, "maidenName", maidenName });
            //    G1.update_db_table("relatives", "record", record, new string[] { "city", city, "spouseFirstName", spouseFirstName, "depDOD", dod });
            //}
        }
        /***********************************************************************************************/
        private void AddMod(DataTable dt, DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView grid)
        {
            if (G1.get_column_number(dt, "mod") < 0)
            {
                dt.Columns.Add("mod");
            }
        }
        /***********************************************************************************************/
        private void pictureBox12_Click(object sender, EventArgs e)
        {
            //DataTable dt = (DataTable)dgvDependent.DataSource;
            //AddMod(dt, gridMainDep);
            //DataRow dRow = dt.NewRow();
            //dt.Rows.Add(dRow);
            //dgvDependent.DataSource = dt;
            //dgvDependent.Refresh();
            //gridMainDep_CellValueChanged(null, null);
        }
        /***********************************************************************************************/
        private void gridMainDep_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            //DataTable dt = (DataTable)dgvDependent.DataSource;
            //if (G1.get_column_number(dt, "mod") < 0)
            //{
            //    dt.Columns.Add("mod");
            //}
            //DataRow dr = gridMainDep.GetFocusedDataRow();
            //if (e != null)
            //{
            //    if (e.Column.FieldName.ToUpper() == "FINANCIALLYDEPENDENT")
            //    {
            //        string data = dr["financiallyDependent"].ObjToString();
            //        if (!String.IsNullOrWhiteSpace(data))
            //        {
            //            string what = data.Substring(0, 1).ToUpper();
            //            if (what == "Y")
            //                what = "YES";
            //            else if (what == "N")
            //                what = "NO";
            //            else
            //            {
            //                MessageBox.Show("***ERROR*** Must be 'Yes' or 'No!");
            //                what = "";
            //            }
            //            dr["financiallyDependent"] = what;
            //        }
            //    }
            //}
            //dr["mod"] = "Y";
            //btnSave.Enabled = true;
            //btnSave.Show();
            //relativesChanged = true;
        }
        /***********************************************************************************************/
        private void pictureBox5_Click(object sender, EventArgs e)
        {
            using (Arrangements arrForm = new Arrangements(true))
            {
                arrForm.Text = "Arrangement List";
                arrForm.ListDone += ArrForm_ListDone;
                arrForm.ShowDialog();
            }
        }
        /***********************************************************************************************/
        private void ArrForm_ListDone(string s)
        {
            if (String.IsNullOrWhiteSpace(s))
                return;
            string arrangementRecord = s;
            string cmd = "Select * from `arrangementForms` where `record` = '" + arrangementRecord + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                string type = dt.Rows[0]["type"].ObjToString();
                string form = dt.Rows[0]["formName"].ObjToString();
                string record = G1.create_record("agreements", "formName", "-1");
                if (G1.BadRecord("agreements", record))
                    return;
                G1.update_db_table("agreements", "record", record, new string[] { "contractNumber", workContract, "formName", form });
                LoadAgreements();
            }
        }
        /***********************************************************************************************/
        private void gridMain3_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain3.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            string form = dr["formName"].ObjToString();
            LoadUpForm(form, record);
        }
        /***********************************************************************************************/
        private void LoadUpForm(string form, string record, bool reload = false)
        {
            string record1 = "";
            string str = G1.get_db_blob("agreements", record, "image");
            if (String.IsNullOrWhiteSpace(str))
            {
                string cmd = "Select * from `arrangementforms` where `formName` = '" + form + "';";
                DataTable dt = G1.get_db_data(cmd);
                if (dt.Rows.Count > 0)
                {
                    record1 = dt.Rows[0]["record"].ObjToString();
                    str = G1.get_db_blob("arrangementforms", record1, "image");
                }
            }
            if (str.IndexOf("rtf1") > 0)
            {
                if (reload)
                {
                    string cmd = "Select * from `arrangementforms` where `formName` = '" + form + "';";
                    DataTable dt = G1.get_db_data(cmd);
                    if (dt.Rows.Count > 0)
                    {
                        byte[] b = Encoding.UTF8.GetBytes(str);
                        G1.update_blob("agreements", "record", record, "image", b);
                        this.Cursor = Cursors.WaitCursor;
                        ArrangementForms arrangementForm = new ArrangementForms(form, "", record, workContract, b);
                        arrangementForm.RtfFinished += ArrangementForm_RtfDone;
                        arrangementForm.Show();
                        this.Cursor = Cursors.Default;
                    }
                }
                else
                {
                    byte[] b = Encoding.UTF8.GetBytes(str);
                    this.Cursor = Cursors.WaitCursor;
                    ArrangementForms arrangementForm = new ArrangementForms(form, "", record, workContract, b);
                    arrangementForm.RtfFinished += ArrangementForm_RtfDone;
                    arrangementForm.Show();
                    this.Cursor = Cursors.Default;

                }
            }
            else if (str.IndexOf("PDF") > 0)
            {
                string command = "Select `image` from `agreements` where `record` = '" + record + "';";
                if (!String.IsNullOrWhiteSpace(record1))
                    command = "Select `image` from `arrangementforms` where `record` = '" + record1 + "';";
                MySqlCommand cmd1 = new MySqlCommand(command, G1.conn1);
                cmd1.Connection.Open();
                try
                {
                    using (MySqlDataReader dR = cmd1.ExecuteReader(System.Data.CommandBehavior.Default))
                    {
                        if (dR.Read())
                        {
                            byte[] fileData = (byte[])dR.GetValue(0);
                            byte[] results = ReplaceFields(fileData, form);
                            this.Cursor = Cursors.WaitCursor;
                            ViewPDF viewForm = new ViewPDF(form, record, workContract, results);
                            viewForm.PdfDone += ViewForm_PdfDone;
                            viewForm.Show();
                            this.Cursor = Cursors.Default;
                        }
                        dR.Close();
                    }
                }
                catch (Exception ex)
                {
                }
                finally
                {
                    if (cmd1.Connection.State == ConnectionState.Open)
                        cmd1.Connection.Close();
                }
            }
        }
        /***********************************************************************************************/
        //private void LoadUpFormx(string form, string record, bool reload = false )
        //{
        //    string workFormName = form;
        //    if (!String.IsNullOrWhiteSpace(workContract) && !reload )
        //        form = workContract + " " + form;

        //    string cmd = "Select * from `pdfimages` where `filename` = '" + form + "';";
        //    DataTable dt = G1.get_db_data(cmd);
        //    if (dt.Rows.Count <= 0)
        //    {
        //        cmd = "Select * from `pdfimages` where `filename` = '" + workFormName + "';";
        //        dt = G1.get_db_data(cmd);
        //        if (dt.Rows.Count > 0)
        //            form = workFormName;
        //        record = "";
        //    }
        //    else
        //        record = dt.Rows[0]["record"].ObjToString();
        //    if (dt.Rows.Count > 0)
        //    {
        //        string pdfRecord = dt.Rows[0]["record"].ObjToString();
        //        string str = G1.get_db_blob("pdfimages", pdfRecord, "image");
        //        if (str.IndexOf("rtf1") > 0)
        //        {
        //            if ( reload )
        //            {
        //                string oldForm = workContract + " " + form;
        //                cmd = "Select * from `pdfimages` where `filename` = '" + oldForm + "';";
        //                dt = G1.get_db_data(cmd);
        //                if ( dt.Rows.Count > 0 )
        //                {
        //                    string oldRecord = dt.Rows[0]["record"].ObjToString();
        //                    byte[] b = Encoding.UTF8.GetBytes(str);
        //                    G1.update_blob("pdfimages", "record", oldRecord, "image", b);
        //                }
        //            }
        //            this.Cursor = Cursors.WaitCursor;
        //            ArrangementForms arrangementForm = new ArrangementForms(record, workFormName, workContract);
        //            arrangementForm.RtfFinished += ArrangementForm_RtfDone;
        //            arrangementForm.Show();
        //            this.Cursor = Cursors.Default;
        //        }
        //        else if ( str.IndexOf ("PDF") > 0 )
        //        {
        //            if (reload)
        //                form = workFormName;
        //            string command = "Select `image` from `pdfimages` where `filename` = '" + form + "';";
        //            MySqlCommand cmd1 = new MySqlCommand(command, G1.conn1);
        //            cmd1.Connection.Open();
        //            try
        //            {
        //                using (MySqlDataReader dR = cmd1.ExecuteReader(System.Data.CommandBehavior.Default))
        //                {
        //                    if (dR.Read())
        //                    {
        //                        byte[] fileData = (byte[])dR.GetValue(0);
        //                        byte[] results = ReplaceFields(fileData, workFormName);
        //                        this.Cursor = Cursors.WaitCursor;
        //                        ViewPDF viewForm = new ViewPDF(form, workContract, results);
        //                        viewForm.PdfDone += ViewForm_PdfDone;
        //                        viewForm.Show();
        //                        this.Cursor = Cursors.Default;
        //                    }
        //                    dR.Close();
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //            }
        //            finally
        //            {
        //                if (cmd1.Connection.State == ConnectionState.Open)
        //                    cmd1.Connection.Close();
        //            }
        //        }
        //    }
        //}
        /***********************************************************************************************/
        private void ArrangementForm_RtfDone(string workFormName, string record, string contractNumber, string rtfText, bool dontAsk, bool force )
        {
            if (String.IsNullOrWhiteSpace(record))
            {
                return;
            }

            string form = workFormName;
            byte[] b = Encoding.UTF8.GetBytes(rtfText);
            G1.update_blob("agreements", "record", record, "image", b);
            //            s.Close();
            //            btnSave.Hide();

            //string str = G1.ConvertToString(b);
            //G1.update_db_blob("pdfimages", record, "image", str);


            //FileStream fStream = File.OpenRead(filename);
            //byte[] contents = new byte[fStream.Length];
            //fStream.Read(contents, 0, (int)fStream.Length);
            //fStream.Close();
            //G1.update_blob("pdfimages", "record", record, "image", contents);

        }
        /***********************************************************************************************/
        private byte[] ReplaceFields(byte[] bytes, string form)
        {
            byte[] result = null;
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                //creating a sample Document
                iTextSharp.text.Document doc = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4, 30f, 30f, 30f, 30f);

                System.IO.MemoryStream mo = new System.IO.MemoryStream();

                iTextSharp.text.pdf.PdfReader reader = new iTextSharp.text.pdf.PdfReader(bytes);
                iTextSharp.text.pdf.PdfStamper pdfStamper = new iTextSharp.text.pdf.PdfStamper(reader, mo);

                iTextSharp.text.pdf.AcroFields fields = pdfStamper.AcroFields;
                fields.SetField("First Name", "Robby");
                string str = fields.GetField("First Name");

                DataTable dt = setupData(form);

                //StringBuilder sb = new StringBuilder();
                //foreach (var de in reader.AcroFields.Fields)
                //{
                //    sb.Append(de.Key.ToString() + Environment.NewLine);
                //}
                //str = sb.ToString();
                //str = str.Replace("\r", "");
                //string[] Lines = str.Split('\n');
                //string field = "";
                //string data = "";

                //string cmd = "Select * from `structures` where `form` = '" + form + "' order by `order`;";
                //DataTable dt = G1.get_db_data(cmd);
                //dt.Columns.Add("num");
                //dt.Columns.Add("mod");
                //dt.Columns.Add("F1");
                //dt.Columns.Add("F2");

                //for (int i = 0; i < Lines.Length; i++)
                //{
                //    field = Lines[i].Trim();
                //    if (!String.IsNullOrWhiteSpace(field))
                //    {
                //        DataRow dRow = dt.NewRow();
                //        dRow["field"] = field;
                //        dt.Rows.Add(dRow);
                //    }
                //}
                //string returnData = "";
                //LoadDbFields(dt);
                string field = "";
                string data = "";
                string returnData = "";
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    field = dt.Rows[i]["field"].ObjToString();
                    data = dt.Rows[i]["F2"].ObjToString();
                    fields.SetField(field, data);
                    returnData = fields.GetField(field);
                }
                pdfStamper.Close();
                reader.Close();
                result = mo.ToArray();
            }
            return result;
        }
        /***********************************************************************************************/
        private DataTable setupData(string form)
        {
            string cmd = "Select * from `structures` where `form` = '" + form + "' order by `order`;";
            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("num");
            dt.Columns.Add("mod");
            dt.Columns.Add("F1");
            dt.Columns.Add("F2");
            LoadDbFields(dt);
            return dt;
        }
        /***********************************************************************************************/
        private DataTable LoadDbFields(DataTable dt)
        {
            if (String.IsNullOrWhiteSpace(workContract))
                return dt;
            string table = "";
            string dbfield = "";
            string data = "";
            string cmd = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                table = dt.Rows[i]["table"].ObjToString();
                dbfield = dt.Rows[i]["dbfield"].ObjToString();
                if (String.IsNullOrWhiteSpace(table))
                    continue;
                if (String.IsNullOrWhiteSpace(dbfield))
                    continue;
                data = GetDbField(table, dbfield, workContract);
                dt.Rows[i]["F2"] = data;
            }
            return dt;
        }
        /***********************************************************************************************/
        private string GetDbField(string table, string field, string contractNumber)
        {
            string data = "";
            string cmd = "";
            string str = "";
            if (table.ToUpper() == "RELATIVES")
                return "";

            try
            {
                cmd = "Select * from `" + table + "` where `contractNumber` = '" + contractNumber + "';";
                DataTable dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    string strDelimitor = " +";
                    string[] Lines = field.Split(new[] { strDelimitor }, StringSplitOptions.None);
                    for (int i = 0; i < Lines.Length; i++)
                    {
                        field = Lines[i].Trim();
                        try
                        {
                            if (field.Trim() != "+")
                            {
                                str = dx.Rows[0][field].ObjToString();
                                data += str;
                            }
                        }
                        catch
                        {
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Looking up Table " + table + " Field " + field + " For ContractNumber " + contractNumber + "!!");
            }
            return data;
        }
        /***********************************************************************************************/
        private void ViewForm_PdfDone(string filename, string record, string contractNumber, byte[] b)
        {
            string formname = G1.DecodeFilename(filename);
            byte[] result = null; // Save Customer PDF Form
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                //creating a sample Document
                iTextSharp.text.Document doc = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4, 30f, 30f, 30f, 30f);

                System.IO.MemoryStream mo = new System.IO.MemoryStream();

                iTextSharp.text.pdf.PdfReader reader = new iTextSharp.text.pdf.PdfReader(b);

                StringBuilder sb = new StringBuilder();
                foreach (var de in reader.AcroFields.Fields)
                {
                    sb.Append(de.Key.ToString() + Environment.NewLine);
                }
                string str = sb.ToString();
                str = str.Replace("\r", "");
                string[] Lines = str.Split('\n');

                iTextSharp.text.pdf.PdfStamper pdfStamper = new iTextSharp.text.pdf.PdfStamper(reader, mo);
                iTextSharp.text.pdf.AcroFields fields = pdfStamper.AcroFields;

                string data = "";
                for (int i = 0; i < Lines.Length; i++)
                {
                    string name = Lines[i].Trim();
                    str = fields.GetField(name);
                    data += name + "\n" + str + "\n";
                }
                pdfStamper.Close();
                reader.Close();
                result = mo.ToArray();
            }
            return;
        }
        ///***********************************************************************************************/
        //private void textAutoAmt_EditValueChanged(object sender, EventArgs e)
        //{
        //    if (loading)
        //        return;
        //    string str = textAutoAmt.Text;
        //    if (!G1.validate_numeric(str))
        //    {
        //        MessageBox.Show("***ERROR*** $ Amount is INVALID!");
        //        loading = true;
        //        textAutoAmt.Text = "0.00";
        //        loading = false;
        //        return;
        //    }
        //    double money = textAutoAmt.Text.ObjToDouble();
        //    str = G1.ReformatMoney(money);
        //    loading = true;
        //    textAutoAmt.Text = str;
        //    loading = false;
        //    btnSave.Show();
        //    btnSave.Enabled = true;
        //}
        ///***********************************************************************************************/
        //private void textDOM_EditValueChanged(object sender, EventArgs e)
        //{
        //    if (loading)
        //        return;
        //    string str = textDOM.Text;
        //    if (!G1.validate_numeric(str))
        //    {
        //        MessageBox.Show("***ERROR*** $ Amount is INVALID!");
        //        loading = true;
        //        textDOM.Text = "1";
        //        loading = false;
        //        return;
        //    }
        //    int day = str.ObjToInt32();
        //    if (day == 0 || day > 28)
        //    {
        //        loading = true;
        //        textAutoAmt.Text = "0.00";
        //        loading = false;
        //        return;
        //    }
        //    btnSave.Show();
        //    btnSave.Enabled = true;
        //}
        /***********************************************************************************************/
        private void pictureBox6_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain3.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            string form = dr["formName"].ObjToString();
            G1.delete_db_table("agreements", "record", record);
            LoadAgreements();
        }
        /***********************************************************************************************/
        private void btnSaveExtended_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv4.DataSource;
            if (dt == null)
                return;
            string saveItems = "";
            string mod = "";
            string parameter = "";
            string data = "";
            var myList = new List<string>();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                mod = dt.Rows[i]["mod"].ObjToString();
                if (mod != "M")
                    continue;
                parameter = dt.Rows[i]["parameter"].ObjToString();
                data = dt.Rows[i]["data"].ObjToString();
                if (!String.IsNullOrWhiteSpace(parameter))
                {
                    saveItems += parameter + "," + data + ",";
                    myList.Add(parameter);
                    myList.Add(data);
                }
            }
            saveItems = saveItems.TrimEnd(',');
            string record = "";
            DataTable ddt = G1.get_db_data("Select * from `cust_extended` where `contractNumber` = '" + workContract + "';");
            if (ddt.Rows.Count <= 0)
            {
                record = G1.create_record("cust_extended", "ServiceId", "-1");
                if (G1.BadRecord("cust_extended", record))
                    return;
                G1.update_db_table("cust_extended", "record", record, new string[] { "contractNumber", workContract });
            }
            else
                record = ddt.Rows[0]["record"].ObjToString();

            if (!G1.BadRecord("cust_extended", record))
            {
                // Create list

                // Add items to the list

                // Convert to array
                var myArray = myList.ToArray();
                try
                {
                    G1.update_db_table("cust_extended", "record", record, myArray);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("**ERRROR*** Updaing Extended Customer Information!\nCall I/T");
                }
                ddt.Dispose();
                ddt = null;
            }
        }
        /***********************************************************************************************/
        private bool extendedModified = false;
        private bool loadingExtended = true;
        private void LoadExtended()
        {
            btnSaveExtended.Hide();
            if (String.IsNullOrWhiteSpace(workContract))
                return;
            DataTable dx = new DataTable();
            dx.Columns.Add("num");
            dx.Columns.Add("mod");
            dx.Columns.Add("parameter");
            dx.Columns.Add("type");
            dx.Columns.Add("data");

            extendedModified = false;

            string parameter = "";

            DataTable dt = G1.get_db_data("Select * from `cust_extended` where `contractNumber` = '" + workContract + "';");
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                parameter = dt.Columns[i].ColumnName.ObjToString();
                if (parameter.ToUpper() == "TMSTAMP")
                    continue;
                if (parameter.ToUpper() == "RECORD")
                    continue;
                if (parameter.ToUpper() == "CONTRACTNUMBER")
                    continue;
                if (parameter.ToUpper() == "SERVICEID")
                    continue;
                DataRow dRow = dx.NewRow();
                dRow["parameter"] = dt.Columns[i].ColumnName.ObjToString();
                dRow["type"] = dt.Columns[i].DataType.ObjToString();
                if (dt.Rows.Count > 0)
                    dRow["data"] = dt.Rows[0][i].ObjToString();
                dx.Rows.Add(dRow);
            }
            G1.NumberDataTable(dx);
            dgv4.DataSource = dx;
            extendedModified = false;
            loadingExtended = false;
        }
        /***********************************************************************************************/
        private void gridMain4_KeyUp(object sender, KeyEventArgs e)
        {
            if (loadingExtended)
                return;
            extendedModified = true;
            DataRow dr = gridMain4.GetFocusedDataRow();
            dr["mod"] = "M";
            string type = dr["type"].ObjToString();
            string data = dr["data"].ObjToString();
            if (type.ToUpper().IndexOf("DECIMAL") >= 0)
            {
                double dvalue = data.ObjToDouble();
                dr["data"] = dvalue.ToString("###.00");
            }
            else if (type.ToUpper().IndexOf("MYSQLDATETIME") >= 0)
            {
                if (data.IndexOf("0000") >= 0)
                {
                    dr["data"] = "0/0/0000";
                }
                else
                {
                    if (G1.validate_date(data))
                    {
                        DateTime date = data.ObjToDateTime();
                        try
                        {
                            MySqlDateTime myDate = (MySqlDateTime)G1.DTtoMySQLDT(date);
                            dr["data"] = myDate.Month.ToString("D2") + "/" + myDate.Day.ToString("D2") + "/" + myDate.Year.ToString("D4");
                        }
                        catch
                        {

                        }
                    }
                }
            }
            btnSaveExtended.Show();
        }
        /***********************************************************************************************/
        private void editDataToolStripMenuItem_Click(object sender, EventArgs e)
        { // NOT USED ANYMORE
            //DataRow dr = gridMain3.GetFocusedDataRow();
            //string record = dr["record"].ObjToString();
            //string form = dr["formName"].ObjToString();
            //EditFormData editForm = new EditFormData(workContract, form, "", record);
            //editForm.Show();
        }
        /***********************************************************************************************/
        private void reloadOriginalDocumentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain3.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            string form = dr["formName"].ObjToString();
            LoadUpForm(form, record, true);
        }
        /***********************************************************************************************/
        private void TextChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            btnSave.Show();
            btnSave.Enabled = true;
        }
        /***********************************************************************************************/
        private void picDec_Click(object sender, EventArgs e)
        {
            string lines = "Add Picture\nClear Picture\nDo Nothing\n";
            using (SelectFromList listForm = new SelectFromList(lines, false))
            {
                listForm.Text = "Select Picture Option";
                listForm.ListDone += ListForm_ListDone;
                listForm.ShowDialog();
            }

            //using (ListSelect listForm = new ListSelect(lines, false))
            //{
            //    listForm.Text = "Select Picture Option";
            //    listForm.ListDone += ListForm_ListDone;
            //    listForm.ShowDialog();
            //}
        }
        /***********************************************************************************************/
        private void ListForm_ListDone(string s)
        {
            if (s == "Do Nothing")
                return;
            if (s == "Add Picture")
            {
                using (OpenFileDialog ofdImage = new OpenFileDialog())
                {
                    ofdImage.Multiselect = false;

                    if (ofdImage.ShowDialog() == DialogResult.OK)
                    {
                        string filename = ofdImage.FileName;
                        filename = filename.Replace('\\', '/');
                        if (!String.IsNullOrWhiteSpace(filename))
                        {
                            try
                            {
                                Bitmap myNewImage = new Bitmap(filename);
                                ImageConverter converter = new ImageConverter();
                                var bytes = (byte[])converter.ConvertTo(myNewImage, typeof(byte[]));
                                G1.update_blob("customers", "record", workRecord, "picture", bytes);
                                this.PatientPicture.Image = (Bitmap)myNewImage;
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("***ERROR*** Storing Image " + ex.ToString());
                            }
                        }
                    }
                    dgv.Refresh();
                    this.Refresh();
                }
            }
            else if (s == "Clear Picture")
            {
                Bitmap emptyImage = new Bitmap(1, 1);
                ImageConverter converter = new ImageConverter();
                var bytes = (byte[])converter.ConvertTo(emptyImage, typeof(byte[]));
                G1.update_blob("customers", "record", workRecord, "picture", bytes);
                this.PatientPicture.Image = (Bitmap)emptyImage;
            }
        }
        /***********************************************************************************************/
        private void gridMainDep_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
        {
            if (e.Column.FieldName.ToUpper() == "DEPDOB" || e.Column.FieldName.ToUpper() == "DEPDOD")
            {
                if (!String.IsNullOrWhiteSpace(e.DisplayText))
                {
                    if (!G1.validate_date(e.DisplayText))
                        e.DisplayText = "";
                    else
                    {
                        DateTime date = e.DisplayText.ObjToDateTime();
                        if (date.Year < 1875)
                            e.DisplayText = "";
                        else
                            e.DisplayText = date.ToString("MM/dd/yyyy");
                    }
                }
            }
        }
        ///***********************************************************************************************/
        //private void DeceasedTextChanged(object sender, EventArgs e)
        //{
        //    string date = dateDeceased.Text;
        //    if (G1.validate_date(date))
        //    {
        //        DateTime ddate = date.ObjToDateTime();
        //    }
        //    btnSave.Show();
        //    btnSave.Enabled = true;
        //}
        ///***********************************************************************************************/
        //private void dateDeceased_EditValueChanged(object sender, EventArgs e)
        //{
        //    string date = dateDeceased.Text;
        //    if (G1.validate_date(date))
        //    {
        //        DateTime ddate = date.ObjToDateTime();
        //        btnSave.Enabled = true;
        //        btnSave.Show();
        //    }
        //}
        ///***********************************************************************************************/
        //private void dateDeceased_Leave(object sender, EventArgs e)
        //{
        //    string date = dateDeceased.Text;
        //    if (G1.validate_date(date))
        //    {
        //        DateTime ddate = date.ObjToDateTime();
        //        dateDeceased.Text = ddate.ToString("MM/dd/yyyy");
        //    }
        //}
        ///***********************************************************************************************/
        //private void dateDeceased_Enter(object sender, EventArgs e)
        //{
        //    string date = dateDeceased.Text;
        //    if (G1.validate_date(date))
        //    {
        //        DateTime ddate = date.ObjToDateTime();
        //        dateDeceased.EditValue = ddate.ToString("MM/dd/yyyy");
        //        date = dateDeceased.Text;
        //    }
        //}
        ///***********************************************************************************************/
        //private void dateDOB_EditValueChanged(object sender, EventArgs e)
        //{
        //    string date = dateDOB.Text;
        //    if (G1.validate_date(date))
        //    {
        //        DateTime ddate = date.ObjToDateTime();
        //        btnSave.Enabled = true;
        //        btnSave.Show();
        //    }
        //}
        ///***********************************************************************************************/
        //private void dateDOB_Enter(object sender, EventArgs e)
        //{
        //    string date = dateDOB.Text;
        //    if (G1.validate_date(date))
        //    {
        //        DateTime ddate = date.ObjToDateTime();
        //        dateDOB.EditValue = ddate.ToString("MM/dd/yyyy");
        //        date = dateDOB.Text;
        //    }
        //}
        ///***********************************************************************************************/
        //private void dateDOB_Leave(object sender, EventArgs e)
        //{
        //    string date = dateDOB.Text;
        //    if (G1.validate_date(date))
        //    {
        //        DateTime ddate = date.ObjToDateTime();
        //        dateDOB.Text = ddate.ToString("MM/dd/yyyy");
        //    }
        //}
        /***********************************************************************************************/
        private void dateMoneyPaid_EditValueChanged(object sender, EventArgs e)
        {
            string date = dateMoneyPaid1.Text;
            DevExpress.XtraEditors.DateEdit date1 = (DevExpress.XtraEditors.DateEdit)sender;
            date = date1.Text;
            if (G1.validate_date(date))
            {
                DateTime ddate = date.ObjToDateTime();
                btnSave.Enabled = true;
                btnSave.Show();
            }
        }
        /***********************************************************************************************/
        private void dateMoneyPaid_Leave(object sender, EventArgs e)
        {
            string date = dateMoneyPaid1.Text;
            DevExpress.XtraEditors.DateEdit date1 = (DevExpress.XtraEditors.DateEdit)sender;
            date = date1.Text;
            if (G1.validate_date(date))
            {
                DateTime ddate = date.ObjToDateTime();
                date1.Text = ddate.ToString("MM/dd/yyyy");
                btnSave.Enabled = true;
                btnSave.Show();
            }
        }
        /***********************************************************************************************/
        private void dateMoneyPaid_Enter(object sender, EventArgs e)
        {
            string date = dateMoneyPaid1.Text;
            DevExpress.XtraEditors.DateEdit date1 = (DevExpress.XtraEditors.DateEdit)sender;
            date = date1.Text;
            if (G1.validate_date(date))
            {
                DateTime ddate = date.ObjToDateTime();
                date1.EditValue = ddate.ToString("MM/dd/yyyy");
                date = date1.Text;
                btnSave.Enabled = true;
                btnSave.Show();
            }
        }
        /***********************************************************************************************/
        private void btnClearMoneyPaid_Click(object sender, EventArgs e)
        {
            DevExpress.XtraEditors.SimpleButton button = (DevExpress.XtraEditors.SimpleButton)sender;
            string name = button.Name.Trim().ToUpper();
            if (name.IndexOf("PAID1") > 0)
                dateMoneyPaid1.Text = "0/0/0000";
            else if (name.IndexOf("PAID2") > 0)
                dateMoneyPaid2.Text = "0/0/0000";
            else if (name.IndexOf("PAID3") > 0)
                dateMoneyPaid3.Text = "0/0/0000";
            else if (name.IndexOf("PAID4") > 0)
                dateMoneyPaid4.Text = "0/0/0000";
            else if (name.IndexOf("PAID5") > 0)
                dateMoneyPaid5.Text = "0/0/0000";
            else if (name.ToUpper() == "BTNCLEARDPDATE")
                dateDPPaid.Text = "0/0/0000";
            btnSave.Enabled = true;
            btnSave.Show();
        }
        /***********************************************************************************************/
        private void SetupTrustPaidCombo(ComboBox combo)
        {
            combo.Items.Clear();
            string cmd = "Select * from `ref_trust_assignments`;";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                string trust = "";
                combo.Items.Add("Clear");
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    trust = dt.Rows[i]["trust_assignments"].ObjToString();
                    combo.Items.Add(trust);
                }
            }
        }
        /***********************************************************************************************/
        private void cmbTrustPaid_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox combo = (ComboBox)sender;
            string text = combo.Text.Trim().ToUpper();
            if (text == "CLEAR")
                combo.SelectedIndex = -1;
            btnSave.Enabled = true;
            btnSave.Show();
        }
        /***********************************************************************************************/
        private void cmbRemove_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            btnSave.Enabled = true;
            btnSave.Show();
            string removal = cmbRemove.Text.Trim().ToUpper();
            setDateRemoved(removal);
        }
        /***********************************************************************************************/
        private void cmbRefund_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            btnSave.Enabled = true;
            btnSave.Show();
            string removal = cmbRefund.Text.Trim().ToUpper();
            setDateRemoved(removal);
        }
        /***********************************************************************************************/
        private void setDateRemoved(string removal)
        {
            if (removal.ToUpper() == "NO")
            {
                txtDOR.Text = "";
                txtDOR.Refresh();
                return;
            }
            string dateRemoved = txtDOR.Text.Trim();
            bool calcNewDate = false;
            if (String.IsNullOrWhiteSpace(dateRemoved))
                calcNewDate = true;
            else
            {
                if (!G1.validate_date(dateRemoved))
                    calcNewDate = true;
            }
            if (!calcNewDate)
                return;
            DateTime now = DateTime.Now;
            txtDOR.Text = now.ToString("MM/dd/yyyy");
            txtDOR.Refresh();
        }
        /***********************************************************************************************/
        private void gridMain5_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            DataTable dt = (DataTable)dgv5.DataSource;
            DataRow dr = gridMain5.GetFocusedDataRow();
            if (e != null)
            {
                string user = LoginForm.username.ToUpper();
                if (e.Column.FieldName.ToUpper() == "REPORT")
                {
                    if ( user == "ROBBY" || user == "CJENKINS")
                    {
                        btnSavePolicies.Show();
                        policiesModified = true;
                    }
                }
            }
        }
        /***********************************************************************************************/
        private void gridMain5_ValidatingEditor(object sender, DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventArgs e)
        {
            GridView view = sender as GridView;
            int row = gridMain5.FocusedRowHandle;
            row = gridMain5.GetDataSourceRowIndex(row);
            DataTable dt = (DataTable)dgv5.DataSource;
            string field = view.FocusedColumn.FieldName.ToUpper();
            if (field == "MYDECEASEDDATE")
            {
                DataRow dr = gridMain5.GetFocusedDataRow();
                string deceasedDate = e.Value.ObjToString();
                dr["mod"] = "Y";
                btnSavePolicies.Show();
                btnSavePolicies.Refresh();
                //if (String.IsNullOrWhiteSpace(deceasedDate))
                //{
                //    DateTime oldDate = dr["myDeceasedDate"].ObjToDateTime();
                //    if (oldDate.Year > 900)
                //    {
                //        double premium = dr["premium"].ObjToDouble();
                //        dr["myPremium"] = premium;
                //        premium = premium * -1D;
                //        UpdatePolicyDeceased(premium);
                //    }
                //    return;
                //}
                //DateTime date = deceasedDate.ObjToDateTime();
                //if (date.Year > 900)
                //{
                //    MySqlDateTime myDate = (MySqlDateTime)G1.DTtoMySQLDT(deceasedDate);
                //    deceasedDate = date.ToString("MM/dd/yyyy");
                //    dt.Rows[row]["myDeceasedDate"] = deceasedDate;
                //    dr["myDeceasedDate"] = deceasedDate;
                //    double premium = dr["premium"].ObjToDouble();
                //    string record = dr["record"].ObjToString();
                //    UpdatePolicyDeceased(premium);
                //    dr["myPremium"] = 0D;
                //    G1.update_db_table("policies", "record", record, new string[] { "deceasedDate", deceasedDate, "premium", "0.00" });
                //}
                //else
                //    e.Valid = false;
            }
        }
        /***********************************************************************************************/
        private bool UpdatePolicyPremium(double premium)
        {
            string contract = workContract;
            string record = workRecord;
            string cmd = "Select * from `" + contractsFile + "` where `contractNumber` = '" + workContract + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return false;
            record = dt.Rows[0]["record"].ObjToString();
            double annualPremium = dt.Rows[0]["annualPremium"].ObjToDouble();
            double balanceDue = premium;
            double nowDue = premium;

            string rule = DetermineAnnual(premium, annualPremium);

            if (rule == "12")
                annualPremium = premium * 12D;
            else if (rule == "11")
                annualPremium = premium * 11D;
            else if (rule == "95%")
                annualPremium = premium * 0.95D * 12D;

            //G1.update_db_table("icontracts", "record", record, new string[] { "amtOfMonthlyPayt", premium.ToString(), "balanceDue", balanceDue.ToString(), "nowDue", nowDue.ToString(), "annualPremium", annualPremium.ToString() });

            //ManualPayment.UpdatePayer(workContract, premium, annualPremium, true);
            return true;
        }
        /***********************************************************************************************/
        private bool UpdatePolicyDeceased(double premium)
        {
            string contract = workContract;
            string record = workRecord;
            string cmd = "Select * from `" + contractsFile + "` where `contractNumber` = '" + workContract + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return false;
            double payment = dt.Rows[0]["amtOfMonthlyPayt"].ObjToDouble();
            double originalPayment = payment;
            double annualPremium = dt.Rows[0]["annualPremium"].ObjToDouble();
            payment = Policies.CalcMonthlyPremium(workContract, "", payment);
            premium = G1.RoundValue(premium);
            payment = payment - premium;
            payment = G1.RoundValue(payment);
            record = dt.Rows[0]["record"].ObjToString();

            string rule = DetermineAnnual(originalPayment, annualPremium);

            if (rule == "12")
                annualPremium = payment * 12D;
            else if (rule == "11")
                annualPremium = payment * 11D;
            else if (rule == "95%")
                annualPremium = payment * 0.95D * 12D;

            //G1.update_db_table(contractsFile, "record", record, new string[] { "amtOfMonthlyPayt", payment.ToString(), "annualPremium", annualPremium.ToString() });

            //ManualPayment.UpdatePayer(workContract, payment, annualPremium, true);

            InitializeDailyHistoryTabPage();
            return true;
        }
        /***********************************************************************************************/
        private string DetermineAnnual(double totalPremium, double annual)
        {
            string rule = "";
            double percent = (totalPremium * 12D) * .95D;
            double eleven = totalPremium * 11D;
            double twelve = totalPremium * 12D;

            if (percent >= (annual - 0.02D) && percent <= (annual + 0.02D))
                rule = "95%";
            if (annual == eleven)
                rule = "11";
            if (String.IsNullOrWhiteSpace(rule))
                rule = "12";

            return rule;
        }
        /***********************************************************************************************/
        private void gridMain5_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
        {
            if (e.Column.FieldName.ToUpper().IndexOf("DATE") >= 0)
            {
                if (e.RowHandle >= 0)
                {
                    string data = e.DisplayText.ObjToString();
                    DateTime date = data.ObjToDateTime();
                    if (date.Year > 900)
                        e.DisplayText = date.ToString("MM/dd/yyyy");
                    else
                        e.DisplayText = "";
                }
            }
        }
        /***********************************************************************************************/
        private void gridMain4_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain4.GetFocusedDataRow();
            //            string record = dr["record"].ObjToString();
            string data = dr["data"].ObjToString();
            string parameter = dr["parameter"].ObjToString();
            textEdit textForm = new textEdit(workContract, parameter, data);
            textForm.RtfDone += TextForm_RtfDone;
            textForm.Show();
        }
        /***********************************************************************************************/
        private void TextForm_RtfDone(string contractNumber, string parameter, string rtfText)
        {
            int count = rtfText.Length;
        }
        /***********************************************************************************************/
        private void gridMain2_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (loadingContract)
                return;
            contractModified = true;
            DataRow dr = gridMain2.GetFocusedDataRow();

            DataTable dt = (DataTable)dgv2.DataSource;
            int rowHandle = gridMain2.FocusedRowHandle;
            int row = gridMain2.GetDataSourceRowIndex(rowHandle);
            string field = e.Column.FieldName.Trim();
            if ( field.ToUpper() == "CUSTOMERDATA")
            {
                dr["mod2"] = "M";
                btnSaveContract.Show();
                return;
            }


            dr["mod"] = "M";
            string type = dr["type"].ObjToString();
            string data = dr["data"].ObjToString();
            if (type.ToUpper().IndexOf("DECIMAL") >= 0)
            {
                double dvalue = data.ObjToDouble();
                dr["data"] = dvalue.ToString("###.00");
            }
            else if (type.ToUpper().IndexOf("MYSQLDATETIME") >= 0)
            {
                if (data.IndexOf("0000") >= 0)
                {
                    dr["data"] = "0/0/0000";
                }
                else
                {
                    if (G1.validate_date(data))
                    {
                        DateTime date = data.ObjToDateTime();
                        try
                        {
                            MySqlDateTime myDate = (MySqlDateTime)G1.DTtoMySQLDT(date);
                            dr["data"] = myDate.Month.ToString("D2") + "/" + myDate.Day.ToString("D2") + "/" + myDate.Year.ToString("D4");
                        }
                        catch
                        {

                        }
                    }
                }
            }
            btnSaveContract.Show();
        }
        /***********************************************************************************************/
        private void menuSetLapsed_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain5.GetFocusedDataRow();
            int rowHandle = gridMain5.FocusedRowHandle;
            int row = gridMain5.GetDataSourceRowIndex(rowHandle);
            string contract = dr["contractNumber"].ObjToString();
            string payer = dr["payer"].ObjToString();
            string name = dr["policyfullname"].ObjToString();
            DialogResult result = MessageBox.Show("Are you sure you want to MARK Insurance Customer (" + name + ")  as Lapsed???", "Mark Lapse Accounts Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
            if (result == DialogResult.No)
                return;
            string record = "";
            DateTime today = DateTime.Now;
            string lapseDate = today.ToString("yyyy-MM-dd");
            record = dr["record"].ObjToString();
            G1.update_db_table("policies", "record", record, new string[] { "lapsed", "Y", "lapsedDate8", lapseDate });
            G1.AddToAudit(LoginForm.username, "PastDue", "Lapse", "Set", name);
            LoadPolicies(policyDueDate8);
        }
        /***********************************************************************************************/
        private void menuClearLapsed_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain5.GetFocusedDataRow();
            int rowHandle = gridMain5.FocusedRowHandle;
            int row = gridMain5.GetDataSourceRowIndex(rowHandle);
            string contract = dr["contractNumber"].ObjToString();
            string payer = dr["payer"].ObjToString();
            string name = dr["policyfullname"].ObjToString();
            DialogResult result = MessageBox.Show("Are you sure you want to CLEAR Insurance Customer (" + name + ")  Lapsed???", "Clear Lapse Accounts Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
            if (result == DialogResult.No)
                return;
            string record = "";
            DateTime today = DateTime.Now;
            string lapseDate = "0000-00-00";
            record = dr["record"].ObjToString();
            G1.update_db_table("policies", "record", record, new string[] { "lapsed", "", "lapsedDate8", lapseDate });
            G1.AddToAudit(LoginForm.username, "PastDue", "Clear Lapse", "ReSet", name);
            LoadPolicies(policyDueDate8);
        }
        /***********************************************************************************************/
        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        { // Recalc Premium
            GridView view = sender as GridView;
            DataRow dr = gridMain5.GetFocusedDataRow();
            int row = gridMain5.FocusedRowHandle;
            row = gridMain5.GetDataSourceRowIndex(row);
            string contract = dr["contractNumber"].ObjToString();
            string payer = dr["payer"].ObjToString();
            string name = dr["policyfullname"].ObjToString();

            DialogResult result = MessageBox.Show("Are you sure you want to RECALCULATE the Total Premium for (" + name + ")???", "Recalculate Premium Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
            if (result == DialogResult.No)
                return;


            DataTable dt = (DataTable)dgv5.DataSource;
            DateTime deceasedDate = DateTime.Now;
            DateTime lapseDate8 = DateTime.Now;
            double premium = 0D;
            double totalPremium = 0D;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                deceasedDate = dt.Rows[i]["MYDECEASEDDATE"].ObjToDateTime();
                if (deceasedDate.Year > 1800)
                    continue;
                lapseDate8 = dt.Rows[i]["lapsedDate8"].ObjToDateTime();
                if (lapseDate8.Year > 1800)
                    continue;
                premium = dt.Rows[i]["premium"].ObjToDouble();
                totalPremium += premium;
            }
            UpdatePolicyPremium(totalPremium);
            G1.AddToAudit(LoginForm.username, "Policies", "Recalc Total Premium", "ReCalc", this.Text);
            if (dailyForm != null)
                dailyForm.FireEventReloadHeader();
            LoadContract();
            string str = G1.ReformatMoney(totalPremium);
            MessageBox.Show("***Information*** Insurance Premium Set to " + str, "Reset Premium Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        /***********************************************************************************************/
        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        { // Reinstate
            DataRow dr = gridMain5.GetFocusedDataRow();
            int rowHandle = gridMain5.FocusedRowHandle;
            int row = gridMain5.GetDataSourceRowIndex(rowHandle);
            string contract = dr["contractNumber"].ObjToString();
            string payer = dr["payer"].ObjToString();
            string name = dr["policyfullname"].ObjToString();
            DialogResult result = MessageBox.Show("Are you sure you want to Reinstate Insurance Policy (" + name + ")???", "Reinstate Accounts Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
            if (result == DialogResult.No)
                return;
            string record = "";
            DateTime today = DateTime.Now;

            string lapseDate = "0000-00-00";
            record = dr["record"].ObjToString();
            G1.update_db_table("policies", "record", record, new string[] { "lapsed", "", "lapsedDate8", lapseDate });
            G1.AddToAudit(LoginForm.username, "PastDue", "Reinstate", "ReSet", name);
            LoadPolicies(policyDueDate8);
        }
        /***********************************************************************************************/
        private void btnFuneral_Click(object sender, EventArgs e)
        {
            string cmd = "select * from `funeralHomes` where `assignedAgents` LIKE ('%" + LoginForm.activeFuneralHomeAgent + "%');";
            //DataTable dt = G1.get_db_data(cmd);
            //if (dt.Rows.Count <= 0)
            //{
            //    if (!checkForActiveFuneralHome())
            //        return;
            //}
            //else if (dt.Rows.Count > 1)
            //{
            //    if (!checkForActiveFuneralHome(dt))
            //        return;
            //}
            //else
            //{
            //    LoginForm.activeFuneralHomeKeyCode = dt.Rows[0]["keycode"].ObjToString();
            //}
            string ssn = lblSSN.Text.Trim();
            ssn = ssn.Replace("SSN # :", "");
            cmd = "Select * from `fcustomers` WHERE `ssn` = '" + ssn + "';";
            DataTable dt = G1.get_db_data(cmd);
            if ( dt.Rows.Count <= 0 )
            {
                DialogResult result = MessageBox.Show("This looks like a new Funeral.\nAre you wanting to setup a new Funeral from this Pre-Need Contract?", "New Funeral Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                if (result == DialogResult.No)
                    return;
                CopyAllContractInfo(workContract);
            }
            this.Cursor = Cursors.WaitCursor;
            //            EditCustomer editForm = new EditCustomer(workContract);
            EditCust editForm = new EditCust(workContract);
            editForm.Show();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        public static void CopyAllContractInfo ( string contractNumber )
        {
            CopyServiceId(contractNumber);

            string customerFile = "customers";
            string contractFile = "contracts";
            if (DailyHistory.isInsurance(contractNumber))
            {
                customerFile = "icustomers";
                contractFile = "icontracts";
            }

            DateTime deceasedDate = DateTime.Now;
            string serviceId = "";

            string cmd = "Select * from `" + contractFile + "` where `contractNumber` = '" + contractNumber + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return;

            cmd = "Select * from `" + customerFile + "` where `contractNumber` = '" + contractNumber + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
                return;
            string ssn = dx.Rows[0]["ssn"].ObjToString();
            if (String.IsNullOrWhiteSpace(ssn))
            {
                MessageBox.Show("***ERROR*** Pre-Need SSN is EMPTY!", "Empty SSN Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }

            CopyFromTableToTable(customerFile, "fcustomers", contractNumber);
            CopyFromTableToTable(contractFile, "fcontracts", contractNumber);
            CopyFromTableToTable("cust_services", "fcust_services", contractNumber);
            CopyFromTableToTable("contracts_extended", "fcontracts_extended", contractNumber);
            CopyFromTableToTable("cust_extended", "fcust_extended", contractNumber);

            PreProcessServices(contractNumber);

            //PreProcessBadCasket(contractNumber);

            SaveCustExtended(contractNumber);

            cmd = "UPDATE `fcust_services` SET `who` = '" + LoginForm.username + "' WHERE `contractNumber` = '" + contractNumber + "';";
            G1.update_db_data(cmd);
        }
        /****************************************************************************************/
        public static void PreProcessBadCasket ( string contractNumber )
        {
            DataTable ddd = null;
            double price = 0D;
            double data = 0D;
            string casketRecord = "";
            string priceRecord = "";
            string cmd = "Select * from `fcust_services` where `contractNumber` = '" + contractNumber + "';";
            DataTable dt = G1.get_db_data(cmd);

            DataRow[] dRows = dt.Select("service='Casket Name'");
            if (dRows.Length > 0)
            {
                casketRecord = dRows[0]["record"].ObjToString();
                dRows = dt.Select("service='Casket Price'");
                if ( dRows.Length > 0 )
                {
                    ddd = dRows.CopyToDataTable();
                    price = dRows[0]["price"].ObjToDouble();
                    data = dRows[0]["data"].ObjToDouble();
                    if ( price == 0D && data == 0D )
                    {
                        priceRecord = dRows[0]["record"].ObjToString();
                        G1.delete_db_table("fcust_services", "record", casketRecord);
                        G1.delete_db_table("fcust_services", "record", priceRecord);
                    }
                }
            }
            dRows = dt.Select("service='Outer Container Name'");
            if (dRows.Length > 0)
            {
                casketRecord = dRows[0]["record"].ObjToString();
                dRows = dt.Select("service='Outer Container Price'");
                if (dRows.Length > 0)
                {
                    ddd = dRows.CopyToDataTable();
                    price = dRows[0]["price"].ObjToDouble();
                    data = dRows[0]["data"].ObjToDouble();
                    if (price == 0D && data == 0D)
                    {
                        priceRecord = dRows[0]["record"].ObjToString();
                        G1.delete_db_table("fcust_services", "record", casketRecord);
                        G1.delete_db_table("fcust_services", "record", priceRecord);
                    }
                }
            }
        }
        /****************************************************************************************/
        public static  void PreProcessServices( string contractNumber )
        {
            string cmd = "Select * from `fcust_services` where `contractNumber` = '" + contractNumber + "';";
            DataTable dt = G1.get_db_data(cmd);

            DataRow[] dRows = dt.Select("service='Committal Equipment'");
            if (dRows.Length <= 0)
                return;
            string record = dRows[0]["record"].ObjToString();

            dRows = dt.Select("service='Staff And Equipment For Graveside Service'");
            if (dRows.Length <= 0)
                G1.update_db_table("fcust_services", "record", record, new string[] {"service", "Staff And Equipment For Graveside Service" });

            //double price = 0D;

            //cmd = "Select * from `fcust_services` where `contractNumber` = '" + contractNumber + "';";
            //dt = G1.get_db_data(cmd);
            //for ( int i=0; i<dt.Rows.Count; i++)
            //{
            //    price = dt.Rows[i]["price"].ObjToDouble();
            //    if ( price <= 0D)
            //    {
            //        record = dt.Rows[i]["record"].ObjToString();
            //        if ( !String.IsNullOrWhiteSpace ( record ))
            //        {
            //            if (record != "0")
            //                G1.delete_db_table("fcust_services", "record", record);
            //        }
            //    }
            //}
        }
        /***********************************************************************************************/
        public static void PreprocessOtherInventory ( DataTable dt )
        {
            string desc = "";
            DataRow[] dRows = null;
            string cmd = "Select DISTINCT type,description from `inventory_other` ORDER BY `description`;";
            DataTable dx = G1.get_db_data(cmd);
            for ( int i=0; i<dx.Rows.Count; i++)
            {
                desc = dx.Rows[i]["description"].ObjToString();
                if (String.IsNullOrWhiteSpace(desc))
                    continue;
                dRows = dt.Select("service LIKE '%" + desc + "%'");
                if (dRows.Length > 0)
                    dRows[0]["service"] = desc;
            }
        }
        /***********************************************************************************************/
        public static void SaveCustExtended( string contractNumber )
        {
            double totalMerchandise = 0D;
            double totalServices = 0D;
            double totalCashAdvance = 0D;
            double merchandiseDifference = 0D;
            double serviceDifference = 0D;
            double price = 0D;
            double diff = 0D;
            string record = "";

            FunServices funForm = new FunServices(contractNumber);
            DataTable dx = funForm.funServicesDT;

            string cmd = "Select * from `fcust_extended` where `contractNumber` = '" + contractNumber + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
            {
                record = G1.create_record("fcust_extended", "pendingComment", "-1");
                if (G1.BadRecord("fcust_extended", record))
                    return;
                G1.update_db_table("fcust_extended", "record", record, new string[] { "contractNumber", contractNumber, "pendingComment", "" });
            }
            else
                record = dt.Rows[0]["record"].ObjToString();
            string custExtendedRecord = record;
            string type = "";
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                type = dx.Rows[i]["type"].ObjToString().ToUpper();
                price = dx.Rows[i]["currentPrice"].ObjToDouble();
                diff = dx.Rows[i]["difference"].ObjToDouble();
                if (type == "MERCHANDISE")
                {
                    totalMerchandise += price;
                    merchandiseDifference += diff;
                }
                else if (type == "SERVICE")
                {
                    totalServices += price;
                    serviceDifference += diff;
                }
                else if (type == "CASH ADVANCE")
                    totalCashAdvance += price;
            }
            double custMerchandise = totalMerchandise - merchandiseDifference;
            double custServices = totalServices - serviceDifference;
            double custPrice = custMerchandise + custServices;
            double currentPrice = totalMerchandise + totalServices;
            double totalDiscount = merchandiseDifference + serviceDifference;

            string caseCreatedDate = DateTime.Now.ToString("yyyy-MM-dd");

            G1.update_db_table("fcust_extended", "record", record, new string[] { "custPrice", custPrice.ToString(), "custMerchandise", custMerchandise.ToString(), "custServices", custServices.ToString(), "cashAdvance", totalCashAdvance.ToString() });
            G1.update_db_table("fcust_extended", "record", record, new string[] { "currentPrice", currentPrice.ToString(), "currentMerchandise", totalMerchandise.ToString(), "currentServices", totalServices.ToString() });
            G1.update_db_table("fcust_extended", "record", record, new string[] { "totalDiscount", totalDiscount.ToString(), "merchandiseDiscount", merchandiseDifference.ToString(), "servicesDiscount", serviceDifference.ToString(), "caseCreatedDate", caseCreatedDate });
        }
        /***********************************************************************************************/
        public static void CopyFromTableToTable ( string from, string to, string contractNumber)
        {

            string cmd = "Select * from `" + from + "` WHERE `contractNumber` = '" + contractNumber + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
                return;

            string fields = "";
            string newFields = "";
            string name = "";
            for (int i = 0; i < dx.Columns.Count; i++)
            {
                name = dx.Columns[i].ColumnName;
                if (name.ToUpper() == "RECORD")
                    continue;
                else if (name.ToUpper() == "TMSTAMP")
                    continue;
                fields += "`" + name + "`,";
                newFields += "`" + name + "`,";
            }

            fields = fields.TrimEnd(',');
            newFields = newFields.TrimEnd(',');

            try
            {
                cmd = "Select * from `" + to + "` WHERE `contractNumber` = '" + contractNumber + "'; ";
                DataTable ddd = G1.get_db_data(cmd);
                if ( ddd.Rows.Count > 0 )
                {
                    string record = "";
                    for (int i = 0; i < ddd.Rows.Count; i++)
                    {
                        record = ddd.Rows[i]["record"].ObjToString();
                        G1.delete_db_table(to, "record", record);
                    }
                }
                cmd = "INSERT INTO `" + to + "` (" + fields + " ) SELECT " + newFields + " FROM `" + from + "` WHERE `contractNumber` = '" + contractNumber + "'; ";
                ddd = G1.get_db_data(cmd);
                int rows = ddd.Rows.Count;
            }
            catch (Exception ex)
            {
            }
        }
        /***********************************************************************************************/
        private bool checkForActiveFuneralHome(DataTable dt = null)
        {
            bool gotit = false;
            if (String.IsNullOrWhiteSpace(LoginForm.activeFuneralHomeKeyCode))
            {
                for (; ; )
                {
                    using (FuneralHomeSelect funSelect = new FuneralHomeSelect(dt))
                    {
                        funSelect.ShowDialog();
                    }
                    if (String.IsNullOrWhiteSpace(LoginForm.activeFuneralHomeKeyCode))
                    {
                        DialogResult result = MessageBox.Show("***Warning*** Are you sure you DO NOT WANT to select an Active Funeral Home?", "Active Funeral Home Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                        if (result == DialogResult.Yes)
                            break;
                    }
                    gotit = true;
                    break;
                }
            }
            else gotit = true;
            return gotit;
        }
        /***********************************************************************************************/
        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        { // Attach Agreement
            string contract = workContract;
            string record = workRecord;
            string record1 = "";
            using (OpenFileDialog ofdImage = new OpenFileDialog())
            {
                ofdImage.Multiselect = false;

                if (ofdImage.ShowDialog() == DialogResult.OK)
                {
                    string filename = ofdImage.FileName;
                    filename = filename.Replace('\\', '/');
                    if (!String.IsNullOrWhiteSpace(filename))
                    {
                        string cmd = "Select `record`, `contractNumber` from `pdfimages` where `contractNumber` = '" + workContract + "';";
                        DataTable ddx = G1.get_db_data(cmd);
                        if (ddx.Rows.Count > 0)
                        {
                            record1 = ddx.Rows[0]["record"].ObjToString();
                            if (!String.IsNullOrWhiteSpace(record1))
                                G1.delete_db_table("pdfimages", "record", record1);
                        }
                        G1.update_db_table(customersFile, "record", record, new string[] { "agreementFile", filename });
                        cmd = "Select * from `pdfimages` where `contractNumber` = '-1';";
                        ddx = G1.get_db_data(cmd);
                        if (ddx.Rows.Count > 0)
                        {
                            for ( int i=0; i<ddx.Rows.Count; i++ )
                            {
                                record1 = ddx.Rows[i]["record"].ObjToString();
                                if (!String.IsNullOrWhiteSpace(record1))
                                    G1.delete_db_table("pdfimages", "record", record1);
                            }
                        }
//                        record1 = G1.create_record("pdfimages", "filename", "-1");
                        record1 = G1.create_record("pdfimages", "contractNumber", "-1");
                        if (!G1.BadRecord("pdfimages", record1))
                        {
                            G1.update_db_table("pdfimages", "record", record1, new string[] { "filename", filename, "contractNumber", workContract });
                            G1.ReadAndStorePDF("pdfimages", record1, filename);
                            G1.update_db_table(customersFile, "record", record, new string[] { "!imagesRecord", record1 });
                        }
                    }
                }
                if (dailyForm != null)
                    dailyForm.FireEventReloadHeader();
                this.Refresh();
            }
        }
        /***********************************************************************************************/
        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        { // Detach Agreement
            string record = workRecord;
            G1.update_db_table(customersFile, "record", record, new string[] { "agreementFile", "", "!imagesRecord", "-1" });
            string cmd = "Select `record`, `contractNumber` from `pdfimages` where `contractNumber` = '" + workContract + "';";
            DataTable ddx = G1.get_db_data(cmd);
            if (ddx.Rows.Count > 0)
            {
                string record1 = ddx.Rows[0]["record"].ObjToString();
                if (!String.IsNullOrWhiteSpace(record1))
                    G1.delete_db_table("pdfimages", "record", record1);
                if (dailyForm != null)
                    dailyForm.FireEventReloadHeader();
            }
        }
        /***********************************************************************************************/
        private void xtraTabControl2_SelectedPageChanged(object sender, TabPageChangedEventArgs e)
        {
            DevExpress.XtraTab.XtraTabControl tabControl = (DevExpress.XtraTab.XtraTabControl)sender;
            DevExpress.XtraTab.XtraTabPage page = tabControl.SelectedTabPage;
            if (page.Name.ObjToString().ToUpper() == "TABFAMILY")
            {
                if (editFunFamily == null)
                {
                    this.Cursor = Cursors.WaitCursor;
                    LoadRelatives();
                    this.Cursor = Cursors.Default;
                }
            }
            else if (page.Name.ObjToString().ToUpper() == "TABTRUSTS")
            {
                string statusReason = "";
                string cmd = "Select * from `trust_data` where `contractNumber` = '" + workContract + "' ORDER by `date` DESC;";
                DataTable dt = G1.get_db_data(cmd);
                if (dt.Rows.Count > 0)
                {
                    DataRow[] dRows = dt.Select("reducedPaidUpAmount>'0'");
                    if ( dRows.Length > 0 )
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                            dt.Rows[i]["endingDeathBenefit"] = 0D;
                    }

                    for ( int i=0; i<dt.Rows.Count; i++)
                    {
                        statusReason = dt.Rows[i]["statusReason"].ObjToString();
                        if (statusReason.ToUpper() == "DC")
                            dt.Rows[i]["deathClaimAmount"] = 0D;
                    }

                    dt = LoadTrustDetails(dt);
                    G1.NumberDataTable(dt);
                }
                else
                {
                    cmd = "Select * from `trust2013r` WHERE `contractNumber` = '" + workContract + "' AND `endingBalance` > '0.00' ORDER by `payDate8` DESC LIMIT 1;";
                    DataTable dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                    {
                        double total = dx.Rows[0]["endingBalance"].ObjToDouble();
                        string str = G1.ReformatMoney(total);
                        txtTBB.Text = str;
                    }
                }

                dgvTrust.DataSource = dt;
            }
        }
        /***********************************************************************************************/
        private DataTable LoadTrustDetails ( DataTable dt)
        {
            DateTime lastDate = DateTime.Now;
            DateTime date = DateTime.Now;

            bool saveVisible = false;
            if (btnSave.Visible)
                saveVisible = true;

            int count = 1;
            string str = "";
            string field = "";

            double tbb = 0D;
            double teb = 0D;
            double total = 0D;
            double payments = 0D;
            double endingDeathBenefit = 0D;
            double reducedPaidUpAmount = 0D;
            string trustCompany = "";
            string datePaid = "";
            double fdlic = 0D;
            DateTime fdlicDatePaid = DateTime.Now;
            double unity = 0D;
            DateTime unityDatePaid = DateTime.Now;
            double cd = 0D;
            DateTime cdDatePaid = DateTime.Now;
            double forethought = 0D;
            DateTime forethoughtDatePaid = DateTime.Now;
            double secnat = 0D;
            DateTime secnatDatePaid = DateTime.Now;
            string cmd = "";
            DataTable dx = null;
            string paidFrom = "";
            double paidFromAmount = 0D;
            DataRow dr = null;

            DateTime maxDate = DateTime.Now;
            DateTime extraDate = DateTime.Now;
            if (dt.Rows.Count > 0)
            {
                extraDate = dt.Rows[0]["date"].ObjToDateTime();
                if ( maxDate.Year > 1000 )
                {
                    cmd = "Select * from `cust_payment_details` WHERE `contractNumber` = '" + workContract + "' AND `dateReceived` > '" + extraDate.ToString("yyyy-MM-dd") + "' AND `type` = 'Trust';";
                    dx = G1.get_db_data(cmd);
                    if ( dx.Rows.Count > 0 )
                    {
                        for ( int i=0; i<dx.Rows.Count; i++)
                        {
                            paidFrom = dx.Rows[i]["paidFrom"].ObjToString();
                            maxDate = dx.Rows[i]["dateReceived"].ObjToDateTime();
                            if ( !String.IsNullOrWhiteSpace ( paidFrom) )
                            {
                                paidFromAmount = dx.Rows[i]["amtActuallyReceived"].ObjToDouble();
                                if ( paidFromAmount > 0D)
                                {
                                    dr = dt.NewRow();
                                    dr["date"] = G1.DTtoMySQLDT(maxDate);
                                    dr["deathPaidDate"] = G1.DTtoMySQLDT(maxDate);
                                    dr["deathClaimAmount"] = paidFromAmount;
                                    dr["trustCompany"] = paidFrom;
                                    dt.Rows.Add(dr);
                                    if (maxDate > extraDate)
                                        extraDate = maxDate;
                                }
                            }
                        }
                        DataView tempview1 = dt.DefaultView;
                        tempview1.Sort = "date DESC";
                        dt = tempview1.ToTable();
                    }
                }
            }

            DataTable newdt = new DataTable();
            newdt.Columns.Add("trustee");
            newdt.Columns.Add("datePaid");
            newdt.Columns.Add("deathClaimPaid", Type.GetType("System.Double"));

            try
            {
                dr = newdt.NewRow();
                dr["trustee"] = "FDLIC";
                dr["deathClaimPaid"] = 0D;
                newdt.Rows.Add(dr);
                dr = newdt.NewRow();
                dr["trustee"] = "Unity";
                dr["deathClaimPaid"] = 0D;
                newdt.Rows.Add(dr);
                dr = newdt.NewRow();
                dr["trustee"] = "Security National";
                dr["deathClaimPaid"] = 0D;
                newdt.Rows.Add(dr);
                dr = newdt.NewRow();
                dr["trustee"] = "Forethought";
                dr["deathClaimPaid"] = 0D;
                newdt.Rows.Add(dr);
                dr = newdt.NewRow();
                dr["trustee"] = "CD";
                dr["deathClaimPaid"] = 0D;
                newdt.Rows.Add(dr);
            }
            catch ( Exception ex)
            {
            }

            DataRow[] dRows = null;
            double dValue = 0D;
            string statusReason = "";

            DateTime datePaidDate = DateTime.Now;
            if (dt.Rows.Count > 0)
            {
                try
                {
                    lastDate = dt.Rows[0]["date"].ObjToDateTime();
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        date = dt.Rows[i]["date"].ObjToDateTime();
                        if (date < extraDate)
                            break;
                        datePaidDate = dt.Rows[i]["deathPaidDate"].ObjToDateTime();
                        datePaid = "";
                        if (datePaidDate.Year > 1000)
                            datePaid = datePaidDate.ToString("MM/dd/yyyy");
                        statusReason = dt.Rows[i]["statusReason"].ObjToString();
                        endingDeathBenefit = dt.Rows[i]["endingDeathBenefit"].ObjToDouble();
                        reducedPaidUpAmount = dt.Rows[i]["reducedPaidUpAmount"].ObjToDouble();
                        tbb = dt.Rows[i]["beginningPaymentBalance"].ObjToDouble();
                        payments = dt.Rows[i]["payments"].ObjToDouble();
                        teb = dt.Rows[i]["endingPaymentBalance"].ObjToDouble();
                        teb = dt.Rows[i]["beginningDeathBenefit"].ObjToDouble();
                        if (datePaidDate.Year > 1000)
                        {
                            teb = dt.Rows[i]["deathClaimAmount"].ObjToDouble();
                            if ( teb <= 0D )
                                teb = dt.Rows[i]["endingPaymentBalance"].ObjToDouble();
                        }
                        if (reducedPaidUpAmount > 0D)
                        {
                            teb = reducedPaidUpAmount;
                            endingDeathBenefit = reducedPaidUpAmount;
                        }
                        if (teb < tbb && payments <= 0D)
                        {

                        }
                        else if (teb < (tbb + payments))
                            teb = tbb + payments;
                        if (statusReason.ToUpper() == "DC")
                        {
                            teb = 0D;
                            dt.Rows[i]["beginningDeathBenefit"] = 0D;
                            dt.Rows[i]["endingPaymentBalance"] = 0D;
                        }
                        else if ( datePaidDate.Year > 1000 )
                        {
                            teb = 0D;
                            dt.Rows[i]["beginningDeathBenefit"] = 0D;
                            dt.Rows[i]["endingPaymentBalance"] = 0D;
                        }
                        total += teb;
                        trustCompany = dt.Rows[i]["trustCompany"].ObjToString();
                        str = G1.ReformatMoney(teb);
                        if (trustCompany.ToUpper().IndexOf("FDLIC") >= 0)
                        {
                            dRows = newdt.Select("trustee='FDLIC'");
                            if (dRows.Length > 0)
                            {
                                dValue = dRows[0]["deathClaimPaid"].ObjToDouble();
                                dValue += teb;
                                dRows[0]["deathClaimPaid"] = dValue;
                                dRows[0]["datePaid"] = datePaid;
                            }
                        }
                        else if (trustCompany.ToUpper().IndexOf("SECURITY NATIONAL") >= 0)
                        {
                            dRows = newdt.Select("trustee='Security National'");
                            if (dRows.Length > 0)
                            {
                                dValue = dRows[0]["deathClaimPaid"].ObjToDouble();
                                dValue += teb;
                                dRows[0]["deathClaimPaid"] = dValue;
                                dRows[0]["datePaid"] = datePaid;
                            }
                        }
                        else if (trustCompany.ToUpper().IndexOf("FORETHOUGHT") >= 0)
                        {
                            dRows = newdt.Select("trustee='Forethought'");
                            if (dRows.Length > 0)
                            {
                                dValue = dRows[0]["deathClaimPaid"].ObjToDouble();
                                dValue += teb;
                                dRows[0]["deathClaimPaid"] = dValue;
                                dRows[0]["datePaid"] = datePaid;
                            }
                        }
                        else if (trustCompany.ToUpper().IndexOf("UNITY") >= 0)
                        {
                            dRows = newdt.Select("trustee='Unity'");
                            if (dRows.Length > 0)
                            {
                                dValue = dRows[0]["deathClaimPaid"].ObjToDouble();
                                dValue += teb;
                                dRows[0]["deathClaimPaid"] = dValue;
                                dRows[0]["datePaid"] = datePaid;
                            }
                        }
                        else if (trustCompany.ToUpper().IndexOf("CD") >= 0)
                        {
                            dRows = newdt.Select("trustee='CD'");
                            if (dRows.Length > 0)
                            {
                                dValue = dRows[0]["deathClaimPaid"].ObjToDouble();
                                dValue += teb;
                                dRows[0]["deathClaimPaid"] = dValue;
                                dRows[0]["datePaid"] = datePaid;
                            }
                        }
                    }
                }
                catch ( Exception ex )
                {
                }
            }

            DataView tempview = newdt.DefaultView;
            tempview.Sort = "deathClaimPaid DESC";
            newdt = tempview.ToTable();

            count = 1;
            total = 0D;
            for ( int i=0; i<newdt.Rows.Count; i++)
            {
                trustCompany = newdt.Rows[i]["trustee"].ObjToString();
                datePaid = newdt.Rows[i]["datePaid"].ObjToString();
                dValue = newdt.Rows[i]["deathClaimPaid"].ObjToDouble();
                if (dValue > 0D)
                {
                    total += dValue;
                    str = G1.ReformatMoney(dValue);
                    if (count == 1)
                    {
                        txtMoney1.Text = str;
                        cmbTrustPaid1.Text = trustCompany;
                        dateMoneyPaid1.Text = datePaid;
                    }
                    else if (count == 2)
                    {
                        txtMoney2.Text = str;
                        cmbTrustPaid2.Text = trustCompany;
                        dateMoneyPaid2.Text = datePaid;
                    }
                    else if (count == 3)
                    {
                        txtMoney3.Text = str;
                        cmbTrustPaid3.Text = trustCompany;
                        dateMoneyPaid3.Text = datePaid;
                    }
                    else if (count == 4)
                    {
                        txtMoney4.Text = str;
                        cmbTrustPaid4.Text = trustCompany;
                        dateMoneyPaid4.Text = datePaid;
                    }
                    else if (count == 5)
                    {
                        txtMoney5.Text = str;
                        cmbTrustPaid5.Text = trustCompany;
                        dateMoneyPaid5.Text = datePaid;
                    }
                    count++;
                }
            }


            str = G1.ReformatMoney(total);
            txtTotalTrust.Text = str;

            DateTime firstDate = DateTime.MinValue;
            total = 0D;
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                date = dt.Rows[i]["date"].ObjToDateTime();
                if (firstDate == DateTime.MinValue)
                    firstDate = date;
                if (date != firstDate)
                    break;
                dValue = dt.Rows[i]["beginningPaymentBalance"].ObjToDouble();
                if ( dValue <= 0D )
                    dValue = dt.Rows[i]["endingPaymentBalance"].ObjToDouble();
                total += dValue;
            }

            str = G1.ReformatMoney(total);
            txtTotalTrust.Text = str;

            cmd = "Select * from `trust2013r` WHERE `contractNumber` = '" + workContract + "' AND `endingBalance` > '0.00' ORDER by `payDate8` DESC LIMIT 1;";
            dx = G1.get_db_data(cmd);
            if ( dx.Rows.Count > 0 )
            {
                total = dx.Rows[0]["endingBalance"].ObjToDouble();
                str = G1.ReformatMoney(total);
                txtTBB.Text = str;
            }

            FindFuneralPayments();

            btnSave.Visible = saveVisible;
            return dt;
        }
        /***********************************************************************************************/
        //private void OldLoadTrustDetails(DataTable dt)
        //{
        //    DateTime lastDate = DateTime.Now;
        //    DateTime date = DateTime.Now;

        //    bool saveVisible = false;
        //    if (btnSave.Visible)
        //        saveVisible = true;

        //    int count = 1;
        //    string str = "";
        //    string field = "";

        //    double tbb = 0D;
        //    double teb = 0D;
        //    double total = 0D;
        //    double payments = 0D;
        //    double endingDeathBenefit = 0D;
        //    double reducedPaidUpAmount = 0D;
        //    string trustCompany = "";
        //    string datePaid = "";
        //    double fdlic = 0D;
        //    double unity = 0D;
        //    double cd = 0D;
        //    double forethought = 0D;
        //    double secnat = 0D;
        //    DateTime datePaidDate = DateTime.Now;
        //    if (dt.Rows.Count > 0)
        //    {
        //        lastDate = dt.Rows[0]["date"].ObjToDateTime();
        //        for (int i = 0; i < dt.Rows.Count; i++)
        //        {
        //            date = dt.Rows[i]["date"].ObjToDateTime();
        //            if (date < lastDate)
        //                break;
        //            datePaidDate = dt.Rows[i]["deathPaidDate"].ObjToDateTime();
        //            datePaid = "";
        //            if (datePaidDate.Year > 1000)
        //                datePaid = datePaidDate.ToString("MM/dd/yyyy");
        //            endingDeathBenefit = dt.Rows[i]["endingDeathBenefit"].ObjToDouble();
        //            reducedPaidUpAmount = dt.Rows[i]["reducedPaidUpAmount"].ObjToDouble();
        //            tbb = dt.Rows[i]["beginningPaymentBalance"].ObjToDouble();
        //            payments = dt.Rows[i]["payments"].ObjToDouble();
        //            teb = dt.Rows[i]["endingPaymentBalance"].ObjToDouble();
        //            if (datePaidDate.Year > 1000)
        //                teb = dt.Rows[i]["deathClaimAmount"].ObjToDouble();
        //            if (reducedPaidUpAmount > 0D)
        //            {
        //                teb = reducedPaidUpAmount;
        //                endingDeathBenefit = reducedPaidUpAmount;
        //            }
        //            if (teb < tbb && payments <= 0D)
        //            {

        //            }
        //            else if (teb < (tbb + payments))
        //                teb = tbb + payments;
        //            total += teb;
        //            trustCompany = dt.Rows[i]["trustCompany"].ObjToString();
        //            str = G1.ReformatMoney(teb);
        //            if (count == 1)
        //            {
        //                txtMoney1.Text = str;
        //                cmbTrustPaid1.Text = trustCompany;
        //                dateMoneyPaid1.Text = datePaid;
        //            }
        //            else if (count == 2)
        //            {
        //                txtMoney2.Text = str;
        //                cmbTrustPaid2.Text = trustCompany;
        //                dateMoneyPaid2.Text = datePaid;
        //            }
        //            else if (count == 3)
        //            {
        //                txtMoney3.Text = str;
        //                cmbTrustPaid3.Text = trustCompany;
        //                dateMoneyPaid3.Text = datePaid;
        //            }
        //            else if (count == 4)
        //            {
        //                txtMoney4.Text = str;
        //                cmbTrustPaid4.Text = trustCompany;
        //                dateMoneyPaid4.Text = datePaid;
        //            }
        //            else if (count == 5)
        //            {
        //                txtMoney5.Text = str;
        //                cmbTrustPaid5.Text = trustCompany;
        //                dateMoneyPaid5.Text = datePaid;
        //            }
        //            count++;
        //        }
        //    }
        //    str = G1.ReformatMoney(total);
        //    txtTotalTrust.Text = str;

        //    string cmd = "Select * from `trust2013r` WHERE `contractNumber` = '" + workContract + "' AND `endingBalance` > '0.00' ORDER by `payDate8` DESC LIMIT 1;";
        //    DataTable dx = G1.get_db_data(cmd);
        //    if (dx.Rows.Count > 0)
        //    {
        //        total = dx.Rows[0]["endingBalance"].ObjToDouble();
        //        str = G1.ReformatMoney(total);
        //        txtTBB.Text = str;
        //    }

        //    btnSave.Visible = saveVisible;
        //}
        /***********************************************************************************************/
        private void FindFuneralPayments ()
        {
            txtMoney1.Text = "";
            cmbTrustPaid1.Text = "";
            dateMoneyPaid1.Text = "";

            txtMoney2.Text = "";
            cmbTrustPaid2.Text = "";
            dateMoneyPaid2.Text = "";

            txtMoney3.Text = "";
            cmbTrustPaid3.Text = "";
            dateMoneyPaid3.Text = "";

            txtMoney4.Text = "";
            cmbTrustPaid4.Text = "";
            dateMoneyPaid4.Text = "";

            txtMoney5.Text = "";
            cmbTrustPaid5.Text = "";
            dateMoneyPaid5.Text = "";

            string cmd = "Select * from `fcustomers` where `contractNumber` = '" + workContract + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return;

            string serviceId = dt.Rows[0]["serviceId"].ObjToString();

            if (String.IsNullOrWhiteSpace(serviceId))
                return;

            DataTable dx = null;
            string record = "";
            string paidFrom = "";
            string status = "";
            DateTime dateFiled = DateTime.Now;
            DateTime dateReceived = DateTime.Now;
            double amtActuallyReceived = 0D;
            double trustAmtFiled = 0D;
            string str = "";
            string paidMoney = "";
            string paidDate = "";

            try
            {
                cmd = "Select * from `cust_payments` where `contractNumber` = '" + workContract + "' and `type` = 'Trust';";
                dt = G1.get_db_data(cmd);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    record = dt.Rows[i]["record"].ObjToString();
                    cmd = "Select * from `cust_payment_details` WHERE `contractNumber` = '" + workContract + "' AND `paymentRecord` = '" + record + "' AND `type` = 'Trust';";
                    dx = G1.get_db_data(cmd);
                    for (int j = 0; j < dx.Rows.Count; j++)
                    {
                        paidFrom = dx.Rows[j]["paidFrom"].ObjToString();
                        status = dx.Rows[j]["status"].ObjToString();
                        dateFiled = dx.Rows[j]["dateFiled"].ObjToDateTime();
                        dateReceived = dx.Rows[j]["dateReceived"].ObjToDateTime();
                        amtActuallyReceived = dx.Rows[j]["amtActuallyReceived"].ObjToDouble();
                        trustAmtFiled = dx.Rows[j]["trustAmtFiled"].ObjToDouble();

                        str = G1.ReformatMoney(trustAmtFiled);
                        if (dateReceived.Year > 1000)
                            str = G1.ReformatMoney(amtActuallyReceived);
                        paidMoney = str;

                        paidDate = dateFiled.ToString("MM/dd/yyyy");
                        if (dateReceived.Year > 1000)
                            paidDate = dateReceived.ToString("MM/dd/yyyy");

                        if (j == 0)
                        {
                            txtMoney1.Text = paidMoney;
                            cmbTrustPaid1.Text = paidFrom;
                            dateMoneyPaid1.Text = paidDate;
                        }
                        else if (j == 1)
                        {
                            txtMoney2.Text = paidMoney;
                            cmbTrustPaid2.Text = paidFrom;
                            dateMoneyPaid2.Text = paidDate;
                        }
                        else if (j == 2)
                        {
                            txtMoney3.Text = paidMoney;
                            cmbTrustPaid3.Text = paidFrom;
                            dateMoneyPaid3.Text = paidDate;
                        }
                        else if (j == 3)
                        {
                            txtMoney4.Text = paidMoney;
                            cmbTrustPaid4.Text = paidFrom;
                            dateMoneyPaid4.Text = paidDate;
                        }
                        else if (j == 4)
                        {
                            txtMoney5.Text = paidMoney;
                            cmbTrustPaid5.Text = paidFrom;
                            dateMoneyPaid5.Text = paidDate;
                        }
                    }
                }
            }
            catch ( Exception ex)
            {
            }
        }
        /***********************************************************************************************/
        private void btnAdd_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv5.DataSource;
            DataRow dr = gridMain5.GetFocusedDataRow();
            int rowhandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowhandle);
            string contractNumber = dr["contractNumber"].ObjToString();
            string payer = dr["payer"].ObjToString();
            string pcode = dr["pCode"].ObjToString();
            string ucode = dr["ucode"].ObjToString();
            string report = dr["report"].ObjToString();
            string address1 = dr["address1"].ObjToString();
            string address2 = dr["address2"].ObjToString();
            string city = dr["city"].ObjToString();
            string state = dr["state"].ObjToString();
            string zip1 = dr["zip1"].ObjToString();
            string zip2 = dr["zip2"].ObjToString();
            string agent = dr["agentCode"].ObjToString();
            string agent1 = dr["agentCode1"].ObjToString();
            string oldagent = dr["oldAgentInfo"].ObjToString();
            string coverageType = dr["coverageType"].ObjToString();

            DataTable cloneDt = dt.Clone();

            DataRow dRow = cloneDt.NewRow();
            dRow["contractNumber"] = contractNumber;
            dRow["payer"] = payer;
            dRow["pCode"] = pcode;
            dRow["ucode"] = ucode;
            dRow["report"] = report;
            dRow["address1"] = address1;
            dRow["address2"] = address2;
            dRow["city"] = city;
            dRow["state"] = state;
            dRow["zip1"] = zip1;
            dRow["zip2"] = zip2;
            dRow["agentCode"] = agent;
            dRow["agentCode1"] = agent1;
            dRow["oldAgentInfo"] = oldagent;
            dRow["coverageType"] = coverageType;
            cloneDt.Rows.Add(dRow);

            using (AddEditPolicy addEditForm = new AddEditPolicy(this.Text, cloneDt))
            {
                DialogResult result = addEditForm.ShowDialog();
                if (result == DialogResult.OK)
                {
                    saveRow = -2;
                    LoadPolicies(policyDueDate8);

                    FunPayments.DeterminePayerDead(payer);

                    if (editFunCustomer != null)
                        InitializeCustomerPanel();
                }
            }
        }
        /***********************************************************************************************/
        private void btnEdit_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain5.GetFocusedDataRow();
            int rowHandle = gridMain5.FocusedRowHandle;
            int row = gridMain5.GetDataSourceRowIndex(rowHandle);
            string contractNumber = dr["contractNumber"].ObjToString();
            string payer = dr["payer"].ObjToString();

            DataTable dt = (DataTable)dgv5.DataSource;
            DataTable cloneDt = dt.Clone();

            G1.copy_dt_row(dt, row, cloneDt, cloneDt.Rows.Count);

            using (AddEditPolicy addEditForm = new AddEditPolicy(this.Text, cloneDt))
            {
                DialogResult result = addEditForm.ShowDialog();
                if (result == DialogResult.OK)
                {
                    saveRow = row;
                    LoadPolicies(policyDueDate8);

                    FunPayments.DeterminePayerDead(payer);

                    if (editFunCustomer != null)
                        InitializeCustomerPanel();
                }
            }
        }
        /***********************************************************************************************/
        private void btnDelete_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv5.DataSource;
            DataRow dr = gridMain5.GetFocusedDataRow();
            int rowHandle = gridMain5.FocusedRowHandle;
            int row = gridMain5.GetDataSourceRowIndex(rowHandle);
            string record = dr["record"].ObjToString();
            string policy = dr["policyNumber"].ObjToString();
            string payer = dr["payer"].ObjToString();

            DialogResult result = MessageBox.Show("***Warning*** Are you SURE you want to DELETE policy (" + policy + ")?", "Delete Policy Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;
            G1.delete_db_table("policies", "record", record);

            FunPayments.DeterminePayerDead(payer);

            dt.Rows.RemoveAt(row);
            dt.AcceptChanges();
            G1.NumberDataTable(dt);
            dgv5.DataSource = dt;
            if (rowHandle > (dt.Rows.Count - 1))
            {
                gridMain5.FocusedRowHandle = rowHandle - 1;
                gridMain5.RefreshData();
                dgv5.Refresh();
            }
        }
        /***********************************************************************************************/
        private void txtDOR_Enter(object sender, EventArgs e)
        {
            string date = txtDOR.Text;
            if (G1.validate_date(date))
            {
                DateTime ddate = date.ObjToDateTime();
                //                dateDeceased.EditValue = ddate.ToString("MM/dd/yyyy");
                txtDOR.Text = ddate.ToString("MM/dd/yyyy");
                //date = dateDeceased.Text;
                //txtServiceId.Enabled = true;
            }
            else
            {
                txtDOR.Text = "";
                //txtServiceId.Enabled = false;
                //txtServiceId.Text = "";
            }
        }
        /***********************************************************************************************/
        private void txtDOR_Leave(object sender, EventArgs e)
        {
            string date = txtDOR.Text;
            if (String.IsNullOrWhiteSpace(date))
            {
                //txtServiceId.Enabled = false;
                //txtServiceId.Text = "";
                return;
            }
            //            DateTime ddate = dateDeceased.DateTime;
            if (G1.validate_date(date))
            {
                DateTime ddate = txtDOR.Text.ObjToDateTime();
                if (ddate.Year < 1800)
                {
                    MessageBox.Show("***ERROR*** Date Entered Year is less than 1800!");
                    //txtServiceId.Enabled = false;
                    //txtServiceId.Text = "";
                    return;
                }
                if (ddate.Year > 100)
                {
                    txtDOR.Text = ddate.ToString("MM/dd/yyyy");
                    //CalculateAge();
                    //txtServiceId.Enabled = true;
                    //dateDeceased.Text = txtDOD.Text;
                }
                else
                {
                }
            }
            else
            {
                MessageBox.Show("***ERROR*** Invalid Date!");
            }
        }
        /***********************************************************************************************/
        private void txtDOR_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                txtDOR_Leave(sender, e);
            else if (e.KeyCode == Keys.Tab)
                txtDOR_Leave(sender, e);
        }
        /***********************************************************************************************/
        private void SetupSelectedColumns(string procType, string group, DevExpress.XtraGrid.GridControl dgv)
        {
            if (String.IsNullOrWhiteSpace(group))
                return;
            if (String.IsNullOrWhiteSpace(procType))
                procType = "Customers";
            string cmd = "Select * from procfiles where name = '" + group + "' and ProcType = '" + procType + "' order by seq";
            DataTable dt = G1.get_db_data(cmd);
            DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gridMain = (DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView)dgv5.MainView;
            for (int i = 0; i < gridMain.Columns.Count; i++)
                gridMain.Columns[i].Visible = false;
            DataTable ddx = (DataTable)dgv5.DataSource;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string name = dt.Rows[i]["Description"].ToString();
                int index = dt.Rows[i]["seq"].ObjToInt32();
                try
                {
                    if (G1.get_column_number(gridMain, name) >= 0)
                        gridMain.Columns[name].Visible = true;
                }
                catch
                {
                }
            }
        }
        /***********************************************************************************************/
        private void cmbSelectColumns_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox combo = (ComboBox)sender;
            string comboName = combo.Text;
            string skinName = "";
            if (!String.IsNullOrWhiteSpace(comboName))
            {
                SetupSelectedColumns("Policies", comboName, dgv5);
                foundLocalPreference = G1.RestoreGridLayout(this, this.dgv5, gridMain5, LoginForm.username, comboName, ref skinName);
                if (!String.IsNullOrWhiteSpace(skinName))
                {
                    //if (skinName != "DevExpress Style")
                    //    skinForm_SkinSelected("Skin : " + skinName);
                }
            }
            else
                SetupSelectedColumns("Policies", "Primary", dgv5);
        }
        /***********************************************************************************************/
        private void btnSelectColumns_Click(object sender, EventArgs e)
        {
            string actualName = cmbSelectColumns.Text;
            SelectColumns sform = new SelectColumns(dgv5, "Policies", "Primary", actualName);
            sform.Done += new SelectColumns.d_void_eventdone(sform_Done);
            sform.Show();
        }
        /***********************************************************************************************/
        void sform_Done()
        {
            dgv5.Refresh();
            this.Refresh();
        }
        /***********************************************************************************************/
        private bool foundPolicyPreference = false;
        private void btnLockPolicies_Click(object sender, EventArgs e)
        {
            string comboName = cmbSelectColumns.Text;
            if (String.IsNullOrWhiteSpace(comboName))
                comboName = "PolicyDetailLayout";

            G1.SaveLocalPreferences(this, gridMain5, LoginForm.username, comboName );
            foundPolicyPreference = true;
        }
        /***********************************************************************************************/
        private void chkUseLockPositions_CheckedChanged(object sender, EventArgs e)
        {
            string skinName = "";
            string comboName = cmbSelectColumns.Text;
            if (String.IsNullOrWhiteSpace(comboName))
                comboName = "PolicyDetailLayout";

            foundLocalPreference = G1.RestoreGridLayout(this, this.dgv5, gridMain5, LoginForm.username, comboName, ref skinName);

            //if (!String.IsNullOrWhiteSpace(skinName))
            //{
            //    if (skinName != "DevExpress Style")
            //        DailyForm_SkinChange(skinName);
            //}
        }
        /***********************************************************************************************/
        private void btnUnlockPositions_Click(object sender, EventArgs e)
        {
            string comboName = cmbSelectColumns.Text;
            if (String.IsNullOrWhiteSpace(comboName))
                comboName = "PolicyDetailLayout";

            G1.RemoveLocalPreferences(LoginForm.username, comboName );
            foundPolicyPreference = false;
        }
        /***********************************************************************************************/
        private void gridMain6_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain6.GetFocusedDataRow();
            string record = dr["noticeRecord"].ObjToString();
            string type = dr["type"].ObjToString();
            string date = dr["noticeDate"].ObjToString();
            string detail = dr["detail"].ObjToString();
            string str = G1.get_db_blob("lapse_notices", record, "image");
            if (str.IndexOf("PDF") > 0)
            {
                try
                {
                    string command = "Select `image` from `lapse_notices` where `record` = '" + record + "';";
                    MySqlCommand cmd1 = new MySqlCommand(command, G1.conn1);
                    cmd1.Connection.Open();
                    try
                    {
                        using (MySqlDataReader dR = cmd1.ExecuteReader(System.Data.CommandBehavior.Default))
                        {
                            if (dR.Read())
                            {
                                byte[] fileData = (byte[])dR.GetValue(0);
                                this.Cursor = Cursors.WaitCursor;
                                ViewPDF viewForm = new ViewPDF(detail, record, workContract, fileData);
                                viewForm.PdfDone += ViewForm_PdfDone;
                                viewForm.Show();
                                this.Cursor = Cursors.Default;
                            }
                            dR.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                    finally
                    {
                        if (cmd1.Connection.State == ConnectionState.Open)
                            cmd1.Connection.Close();
                    }

                    //byte[] b = Encoding.ASCII.GetBytes(str);
                    //this.Cursor = Cursors.WaitCursor;
                    //ViewPDF viewForm = new ViewPDF(detail, record, workContract, b);
                    //viewForm.PdfDone += ViewForm_PdfDone;
                    //viewForm.Show();
                    //this.Cursor = Cursors.Default;
                }
                catch (Exception ex)
                {
                }
            }
            else
            {
                byte[] b = Encoding.ASCII.GetBytes(str);
                byte[] bytes = Encoding.ASCII.GetBytes(str);

                MemoryStream stream = new MemoryStream(bytes);

                RichTextBox rtb = new RichTextBox();
                rtb.LoadFile(stream, RichTextBoxStreamType.RichText);
                if (detail.ToUpper().IndexOf("LAPSE") >= 0)
                    rtb.Rtf = FindNotice(workContract, rtb);

                ArrangementForms aForm = new ArrangementForms("", "", record, workContract, b, false, true);
                aForm.RtfDone += AForm_RtfDone;
                aForm.Show();
            }


            //ViewRTF aForm = new ViewRTF(rtb.Rtf);
            //aForm.Text = detail + " for " + workContract;
            //aForm.Show();
        }
        /***********************************************************************************************/
        private void AForm_RtfDone(string filename, string contractNumber, string rtfText)
        {
            string record = filename;
            if (!G1.validate_numeric(record))
                return;
            byte[] b = Encoding.UTF8.GetBytes(rtfText);
            G1.update_blob("lapse_notices", "record", record, "image", b);
        }
        /***********************************************************************************************/
        private string FindNotice(string contractNumber, RichTextBox rtb)
        {
            string result = "";
            RichTextBox rtb3 = new RichTextBox();
            rtb3.Font = new Font("Lucida Console", 9);
            string[] Lines = rtb.Text.Split('\n');
            string text = "";
            string contract = "";
            for (int i = 0; i < Lines.Length; i++)
            {
                text = Lines[i];
                if (text.ToUpper().IndexOf("OUR RECORDS SHOW") >= 0)
                {
                    if (contract == contractNumber)
                        break;
                    rtb3.Clear();
                    rtb3.AppendText(text + "\n");
                    contract = "";
                }
                else
                {
                    if (String.IsNullOrWhiteSpace(contract))
                    {
                        if (text.ToUpper().IndexOf("ACCNT#") >= 0)
                        {
                            if (text.ToUpper().IndexOf(contractNumber) >= 0)
                                contract = contractNumber;
                            else
                                contract = "XXXX";
                            rtb3.AppendText(text + "\n");
                        }
                        else
                            rtb3.AppendText(text + "\n");
                    }
                    else
                        rtb3.AppendText(text + "\n");
                }
            }
            result = rtb3.Rtf;
            return result;
        }
        /***********************************************************************************************/
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        { // Run Print Contract Details
            if (dgv5.Visible)
                printPreviewToolStripMenuItem_Click(null, null);
            else if (dgv.Visible)
                printPreviewToolStripMenuItem_Click(null, null);
            else if (dgv2.Visible)
                printPreviewToolStripMenuItem_Click(null, null);
            else
            {
                this.Cursor = Cursors.WaitCursor;
                PrintContractDetails printForm = new PrintContractDetails(workContract);
                printForm.Show();
                this.Cursor = Cursors.Default;
            }
        }
        /***********************************************************************************************/
        private void setRequestAcceptedDateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)(dgv7.DataSource);
            DataRow dr = gridMain7.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            string date = DateTime.Now.ToString("MM/dd/yyyy");
            G1.update_db_table("reinstate_requests", "record", record, new string[] { "date_approved", date });
            var date3 = G1.DTtoMySQLDT(date);
            dr["date_approved"] = date3;
            int rowHandle = gridMain7.FocusedRowHandle;
            int row = gridMain7.GetDataSourceRowIndex(rowHandle);
            dt.Rows[row]["date_approved"] = date3;
            dt.AcceptChanges();
            gridMain7.RefreshData();
            dgv7.RefreshDataSource();
            dgv7.Refresh();
        }
        /***********************************************************************************************/
        private void gridMain7_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            ColumnView view = sender as ColumnView;
            if (e.Column.FieldName.ToUpper().IndexOf("DATE") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                if (e.DisplayText.IndexOf("0000") >= 0 || e.DisplayText.IndexOf("0001") >= 0)
                    e.DisplayText = "";
                else
                {
                    DateTime date = e.DisplayText.ObjToDateTime();
                    if (e.DisplayText.IndexOf("0000") >= 0 || e.DisplayText.IndexOf("0001") >= 0)
                        e.DisplayText = "";
                    else
                        e.DisplayText = date.ToString("MM/dd/yyyy");
                }
            }
        }
        /***********************************************************************************************/
        private void setRequestAsVOIDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)(dgv7.DataSource);
            DataRow dr = gridMain7.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            string status = "VOID-" + LoginForm.username;
            G1.update_db_table("reinstate_requests", "record", record, new string[] { "status", status });
            string date = DateTime.Now.ToString("MM/dd/yyyy");
            G1.update_db_table("reinstate_requests", "record", record, new string[] { "date_approved", date });
            dr["status"] = status;
            var date3 = G1.DTtoMySQLDT(date);
            dr["date_approved"] = date3;
            int rowHandle = gridMain7.FocusedRowHandle;
            int row = gridMain7.GetDataSourceRowIndex(rowHandle);
            dt.Rows[row]["status"] = status;
            dt.Rows[row]["date_approved"] = date3;
            dt.AcceptChanges();
            gridMain7.RefreshData();
            dgv7.RefreshDataSource();
            dgv7.Refresh();
        }
        /***************************************************************************************/
        private void SaveContractExtras()
        {
            string text = this.rtb1.Text;
            string cmd = "Select * from `contracts_extended` where `contractNumber` = '" + workContract + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0 && String.IsNullOrWhiteSpace(text))
                return;
            string record = "";
            if (dx.Rows.Count <= 0 && !String.IsNullOrWhiteSpace(text))
                record = G1.create_record("contracts_extended", "thirdPartyComment", "-1");
            else
                record = dx.Rows[0]["record"].ObjToString();
            if (G1.BadRecord("contracts_extended", record))
                return;
            G1.update_db_table("contracts_extended", "record", record, new string[] { "contractNumber", workContract, "thirdPartyComment", text });
        }
        /***************************************************************************************/
        private void LoadContractExtras()
        {
            string text = "";
            string cmd = "Select * from `contracts_extended` where `contractNumber` = '" + workContract + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
                text = dx.Rows[0]["thirdPartyComment"].ObjToString();
            this.rtb1.Text = text;
        }
        /***********************************************************************************************/
        private void rtb1_TextChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            funModified = true;
            btnSave.Enabled = true;
            btnSave.Show();
        }
        /***********************************************************************************************/
        private void changePayerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string newPayer = "";
            using (Ask askForm = new Ask("Enter Payer # (Must Already Exist)?"))
            {
                askForm.Text = "";
                askForm.ShowDialog();
                if (askForm.DialogResult != System.Windows.Forms.DialogResult.OK)
                    return;
                newPayer = askForm.Answer;
                if (String.IsNullOrWhiteSpace(newPayer))
                    return;
            }
            if (String.IsNullOrWhiteSpace(newPayer))
                return;

            DataRow dr = gridMain5.GetFocusedDataRow();
            string oldPolicyRecord = dr["record"].ObjToString();
            string cmd = "Select * from `icustomers` where `payer` = '" + newPayer + "' ORDER BY `contractNumber` DESC;";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
            {
                MessageBox.Show("***ERROR*** New Payer Does Not Exist yet! You must first create a new Insurance Payer!");
                return;
            }
            string contractNumber = dt.Rows[0]["contractNumber"].ObjToString();
            string firstName = dt.Rows[0]["firstName"].ObjToString();
            string lastName = dt.Rows[0]["lastName"].ObjToString();
            G1.update_db_table("policies", "record", oldPolicyRecord, new string[] { "contractNumber", contractNumber, "payer", newPayer, "lastName", lastName, "firstName", firstName });
            LoadPolicies(policyDueDate8);
        }
        /***********************************************************************************************/
        private void clearDeceasedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain5.GetFocusedDataRow();
            int rowHandle = gridMain5.FocusedRowHandle;
            int row = gridMain5.GetDataSourceRowIndex(rowHandle);
            string contract = dr["contractNumber"].ObjToString();
            string payer = dr["payer"].ObjToString();
            string name = dr["policyfullname"].ObjToString();
            DialogResult result = MessageBox.Show("Are you sure you want to CLEAR Insurance Customer (" + name + ")  Deceased???", "Clear Deceased Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
            if (result == DialogResult.No)
                return;
            string record = "";
            DateTime today = DateTime.Now;
            string lapseDate = "0000-00-00";
            record = dr["record"].ObjToString();
            string premium = dr["beneficiary"].ObjToString();
            G1.update_db_table("policies", "record", record, new string[] { "deceasedDate", lapseDate, "premium", premium });
            G1.AddToAudit(LoginForm.username, "Policies", "Clear Deceased", "ReSet", name);

            LoadPolicies(policyDueDate8);
        }
        /***********************************************************************************************/
        private void gridMain5_CustomDrawFooterCell(object sender, FooterCellCustomDrawEventArgs e)
        {
            if (e.Column.FieldName.ToUpper() != "MYPREMIUM")
                return;
            int dx = e.Bounds.Height;
            Brush brush = new System.Drawing.SolidBrush(this.gridMain5.Appearance.BandPanelBackground.BackColor);
            //            Brush brush = e.Cache.GetGradientBrush(e.Bounds, this.gridMain.Appearance.BandPanelBackground.BackColor, Color.FloralWhite, );
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
            double total = calculateTotalPremiums();
            total = Policies.CalcMonthlyPremium(workPayer);
            string text = G1.ReformatMoney(total);
            e.Appearance.DrawString(e.Cache, text, r);
            //Prevent default drawing of the cell 
            e.Handled = true;
        }
        /****************************************************************************************/
        private double calculateTotalPremiums()
        {
            DataTable dt = (DataTable)dgv5.DataSource;
            double price = 0D;
            double total = 0D;
            DateTime date = DateTime.Now;
            string lapsed = "";
            bool gotPremium = false;
            if (G1.get_column_number(dt, "myPremium") >= 0)
                gotPremium = true;
            bool doit = true;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                doit = true;
                date = dt.Rows[i]["deceasedDate"].ObjToDateTime();
                if (date.Year > 100)
                    doit = false;

                date = dt.Rows[i]["lapseDate8"].ObjToDateTime();
                if (date.Year > 100)
                    doit = false;

                lapsed = dt.Rows[i]["lapsed"].ObjToString().ToUpper();
                if (lapsed == "Y")
                    doit = false;

                if (gotPremium && !doit)
                {
                    //dt.Rows[i]["myPremium"] = 0D;
                    continue;
                }
                else if (!doit)
                    continue;

                price = dt.Rows[i]["premium"].ObjToDouble();
                total += price;
            }
            total = G1.RoundDown(total);
            return total;
        }
        /***********************************************************************************************/
        private void button1_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv5.DataSource;
            SplitPolicies splitForm = new SplitPolicies(workContract, workPayer, dt);
            splitForm.Show();
        }
        /***********************************************************************************************/
        private void gridMain5_CustomSummaryCalculate(object sender, DevExpress.Data.CustomSummaryEventArgs e)
        {
            double value = e.TotalValue.ObjToDouble();
            string field = (e.Item as GridSummaryItem).FieldName.ObjToString();
            double monthlyPremium = 0D;
            double historicPremium = 0D;
            double monthlySecNat = 0D;
            double monthly3rdParty = 0D;
            double premium = 0D;
            CustomerDetails.CalcMonthlyPremium(workPayer, ref monthlyPremium, ref historicPremium, ref monthlySecNat, ref monthly3rdParty );

            if (field.ToUpper() == "PREMIUM")
            {
                e.TotalValueReady = true;
                premium = monthlyPremium;
                if (chkHonor.Checked)
                {
                    if (chkSecNat.Checked)
                        premium = monthlySecNat;
                    else if (chkHonor3rdParty.Checked)
                        premium = monthly3rdParty;
                    else
                    {
                        if ( DateTime.Now > DailyHistory.kill3rdPartyDate )
                            premium = premium - monthlySecNat - monthly3rdParty;
                        else
                            premium = premium - monthlySecNat;
                    }
                }
                e.TotalValue = premium;
            }
            if (field.ToUpper() == "HISTORICPREMIUM")
            {
                e.TotalValueReady = true;
                e.TotalValue = historicPremium;
            }
            else if (field.ToUpper() == "LIABILITY")
            {
                e.TotalValueReady = true;
                premium = Policies.CalcTotalLiability(workPayer);
                e.TotalValue = premium;
            }
        }
        /***********************************************************************************************/
        private void gridMain5_CustomColumnDisplayText(object sender, CustomColumnDisplayTextEventArgs e)
        {
            ColumnView view = sender as ColumnView;
            if (e.Column.FieldName.ToUpper() == "DUEDATE8" && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                DataTable dt = (DataTable)dgv5.DataSource;
                int row = e.ListSourceRowIndex;
                string lapsed = dt.Rows[row]["lapsed"].ObjToString().ToUpper();

                if (lapsed == "Y" )
                {
                    DateTime lapsedDate = dt.Rows[row]["lapseDate8"].ObjToDateTime();
                    DateTime reinstateDate = dt.Rows[row]["reinstateDate8"].ObjToDateTime();
                    if (lapsedDate.Year == 0 || lapsedDate.Year == 1)
                    {
                        e.DisplayText = "00/00/0000";
                        return;
                    }
                    else
                        e.DisplayText = lapsedDate.ToString("MM/dd/yyyy");
                }
                else
                {
                    DateTime deceasedDate = dt.Rows[row]["myDeceasedDate"].ObjToDateTime();
                    if (deceasedDate.Year > 1900)
                        e.DisplayText = "";
                }
                string report = dt.Rows[row]["report"].ObjToString().ToUpper();
                if (report != "NOT THIRD PARTY")
                    e.DisplayText = "";
            }
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
        /***********************************************************************************************/
        private void printReinstateReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            printPreviewToolStripMenuItem_Click(null, null);
        }
        /***********************************************************************************************/
        private void gridMain7_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain7.GetFocusedDataRow();
            int rowHandle = gridMain7.FocusedRowHandle;
            int row = gridMain7.GetDataSourceRowIndex(rowHandle);
            DataTable dt = (DataTable)dgv7.DataSource;
            ReinstateRequest reinstateForm = new ReinstateRequest(workContract, 0D, dt.Rows[row]);
            reinstateForm.Show();
        }
        /****************************************************************************************/
        public static bool isSecNat ( string companyCode )
        {
            bool gotSecNat = false;
            if (secNatDt == null)
                secNatDt = G1.get_db_data("Select * from `secnat`;");
            DataRow [] dR = secNatDt.Select ("cc='" + companyCode + "'");
            if (dR.Length > 0)
                gotSecNat = true;

            return gotSecNat;
        }
        /****************************************************************************************/
        public static DataTable secNatDt = null;
        public static DataTable filterSecNat(bool include, DataTable dt)
        {
            if (secNatDt == null)
                secNatDt = G1.get_db_data("Select * from `secnat`;");


            DataTable newDt = dt.Clone();
            try
            {
                if (!include)
                {
                    //var result = dt.AsEnumerable()
                    //       .Where(row => !secNatDt.AsEnumerable()
                    //                             .Select(r => r.Field<string>("cc"))
                    //                             .Any(x => x == row.Field<string>("companyCode"))
                    //      ).CopyToDataTable();
                    //newDt = result.Copy();
                    var result = dt.AsEnumerable()
                           .Where(row => !secNatDt.AsEnumerable()
                                                 .Select(r => r.Field<string>("cc"))
                                                 .Any(x => x == row.Field<string>("companyCode"))
                          );
                    newDt = result.CopyToDataTable();
                }
                else
                {
                    var result = dt.AsEnumerable()
                           .Where(row => secNatDt.AsEnumerable()
                                                 .Select(r => r.Field<string>("cc"))
                                                 .Any(x => x == row.Field<string>("companyCode"))
                          );
                    newDt = result.CopyToDataTable();
                }
            }
            catch (Exception ex)
            {
            }
            return newDt;
        }
        /***********************************************************************************************/
        private void chkHonor_CheckedChanged(object sender, EventArgs e)
        {
            LoadPolicies(policyDueDate8);
        }
        /***********************************************************************************************/
        private void chkSecNat_CheckedChanged(object sender, EventArgs e)
        {
            LoadPolicies(policyDueDate8);
        }
        /***********************************************************************************************/
        public static void CalcMonthlyPremium(DataTable dt, string deceasedColumnName, ref double monthlyPremium, ref double historicPremium, ref double monthlySecNat, ref double monthly3rdParty)
        {
            monthlyPremium = 0D;
            historicPremium = 0D;
            monthlySecNat = 0D;
            monthly3rdParty = 0D;
            if (String.IsNullOrWhiteSpace(deceasedColumnName))
                deceasedColumnName = "deceasedDate";

            //string cmd = "Select * from `policies` where `payer` = '" + payer + "';";
            //DataTable dt = G1.get_db_data(cmd);
            //if (dt.Rows.Count <= 0)
            //    return;

            DateTime deceasedDate = DateTime.Now;
            DateTime lapseDate8 = DateTime.Now;
            double premium = 0D;
            string lapsed = "";
            string report = "";

            bool gotPremium = false;
            if (G1.get_column_number(dt, "myPremium") >= 0)
                gotPremium = true;
            bool doit = true;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                report = dt.Rows[i]["report"].ObjToString();
                if (String.IsNullOrWhiteSpace(report))
                    continue;
                premium = dt.Rows[i]["historicPremium"].ObjToDouble();
                historicPremium += premium;

                doit = true;
                deceasedDate = dt.Rows[i][deceasedColumnName].ObjToDateTime();
                if (deceasedDate.Year > 1800)
                    doit = false;
                lapseDate8 = dt.Rows[i]["lapsedDate8"].ObjToDateTime();
                if (lapseDate8.Year > 1800)
                    doit = false;

                lapsed = dt.Rows[i]["lapsed"].ObjToString().ToUpper();
                if (lapsed == "Y")
                    doit = false;

                if (gotPremium && !doit)
                {
                    //dt.Rows[i]["myPremium"] = 0D;
                    continue;
                }
                else if (!doit)
                    continue;
                premium = dt.Rows[i]["premium"].ObjToDouble();
                monthlyPremium += premium;
                report = dt.Rows[i]["report"].ObjToString();
                if (report.ToUpper() != "NOT THIRD PARTY")
                    monthly3rdParty += premium;
            }
            monthlyPremium = G1.RoundDown(monthlyPremium);
            historicPremium = G1.RoundDown(historicPremium);

            DataTable testDt = filterSecNat(true, dt);
            dt = testDt.Copy();

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                doit = true;
                deceasedDate = dt.Rows[i]["deceasedDate"].ObjToDateTime();
                if (deceasedDate.Year > 1800)
                    doit = false;
                lapseDate8 = dt.Rows[i]["lapsedDate8"].ObjToDateTime();
                if (lapseDate8.Year > 1800)
                    doit = false;

                lapsed = dt.Rows[i]["lapsed"].ObjToString().ToUpper();
                if (lapsed == "Y")
                    doit = false;

                if (gotPremium && !doit)
                {
                    //dt.Rows[i]["myPremium"] = 0D;
                    continue;
                }
                else if (!doit)
                    continue;
                premium = dt.Rows[i]["premium"].ObjToDouble();
                monthlySecNat += premium;
            }
            monthlySecNat = G1.RoundDown(monthlySecNat);
            monthly3rdParty = G1.RoundDown(monthly3rdParty);
            monthly3rdParty = monthly3rdParty - monthlySecNat;
            monthly3rdParty = G1.RoundDown(monthly3rdParty);
            return;
        }
        /***********************************************************************************************/
        public static void CalcMonthlyPremium(string payer, ref double monthlyPremium, ref double historicPremium, ref double monthlySecNat, ref double monthly3rdParty )
        {
            monthlyPremium = 0D;
            historicPremium = 0D;
            monthlySecNat = 0D;
            monthly3rdParty = 0D;

            decimal mPremium = 0;
            decimal mTotal = 0;

            decimal hPremium = 0;
            decimal hTotal = 0;

            string cmd = "Select * from `policies` where `payer` = '" + payer + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return;

            DateTime deceasedDate = DateTime.Now;
            DateTime lapseDate8 = DateTime.Now;
            double premium = 0D;
            string lapsed = "";
            string report = "";

            bool gotPremium = false;
            if (G1.get_column_number(dt, "myPremium") >= 0)
                gotPremium = true;
            bool doit = true;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                report = dt.Rows[i]["report"].ObjToString();
                if (String.IsNullOrWhiteSpace(report))
                    continue;
                premium = dt.Rows[i]["historicPremium"].ObjToDouble();
                historicPremium += premium;
                hPremium = dt.Rows[i]["historicPremium"].ObjToDecimal();
                hTotal += hPremium;

                doit = true;
                deceasedDate = dt.Rows[i]["deceasedDate"].ObjToDateTime();
                if (deceasedDate.Year > 1800)
                    doit = false;
                lapseDate8 = dt.Rows[i]["lapsedDate8"].ObjToDateTime();
                if (lapseDate8.Year > 1800)
                    doit = false;

                lapsed = dt.Rows[i]["lapsed"].ObjToString().ToUpper();
                if (lapsed == "Y")
                    doit = false;

                if (gotPremium && !doit)
                {
                    //dt.Rows[i]["myPremium"] = 0D;
                    continue;
                }
                else if (!doit)
                    continue;
                premium = dt.Rows[i]["premium"].ObjToDouble();
                premium = G1.RoundValue(premium); // Ramma Zamma
                monthlyPremium += premium;
                mPremium = dt.Rows[i]["premium"].ObjToDecimal();
                mTotal += mPremium;
                report = dt.Rows[i]["report"].ObjToString();
                if (report.ToUpper() != "NOT THIRD PARTY")
                    monthly3rdParty += premium;
            }
            monthlyPremium = G1.RoundDown(monthlyPremium);
            monthlyPremium = mTotal.ObjToDouble();
            historicPremium = G1.RoundDown(historicPremium);
            historicPremium = hTotal.ObjToDouble();

            DataTable testDt = filterSecNat(true, dt);
            dt = testDt.Copy();

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                doit = true;
                deceasedDate = dt.Rows[i]["deceasedDate"].ObjToDateTime();
                if (deceasedDate.Year > 1800)
                    doit = false;
                lapseDate8 = dt.Rows[i]["lapsedDate8"].ObjToDateTime();
                if (lapseDate8.Year > 1800)
                    doit = false;

                lapsed = dt.Rows[i]["lapsed"].ObjToString().ToUpper();
                if (lapsed == "Y")
                    doit = false;

                if (gotPremium && !doit)
                {
                    //dt.Rows[i]["myPremium"] = 0D;
                    continue;
                }
                else if (!doit)
                    continue;
                premium = dt.Rows[i]["premium"].ObjToDouble();
                monthlySecNat += premium;
            }
            monthlySecNat = G1.RoundDown(monthlySecNat);
            monthly3rdParty = G1.RoundDown(monthly3rdParty);
            monthly3rdParty = monthly3rdParty - monthlySecNat;
            monthly3rdParty = G1.RoundDown(monthly3rdParty);
            return;
        }
        /***********************************************************************************************/
        private void txtDayOfMonth_EditValueChanged(object sender, EventArgs e)
        {
            if (String.IsNullOrWhiteSpace(txtDayOfMonth.Text))
                return;
            string dom = txtDayOfMonth.Text.Trim();
            if (String.IsNullOrWhiteSpace(dom))
                txtDayOfMonth.Text = "1";
            else
            {
                int day = dom.ObjToInt32();
                if (day < 0 || day > 32)
                    txtDayOfMonth.Text = "1";
            }
            btnSave.Show();
            btnSave.Enabled = true;
        }
        /***********************************************************************************************/
        private void txtFrequency_EditValueChanged(object sender, EventArgs e)
        {
            if (String.IsNullOrWhiteSpace(txtFrequency.Text))
                return;
            string freq = txtFrequency.Text;
            if (String.IsNullOrWhiteSpace(freq))
                txtFrequency.Text = "1";
            else
            {
                int day = freq.ObjToInt32();
                if (day < 0 || day > 12)
                    txtFrequency.Text = "1";
            }

            try
            {
                //int monthlyFrequency = Convert.ToInt32(txtFrequency.Text);
                //double payment = achMonthlyPayment * monthlyFrequency;
                //if (payment <= 0D)
                //    payment = 0D;
                //string spayment = G1.ReformatMoney(payment);
                //txtPayment.Text = spayment;

                btnSave.Show();
                btnSave.Enabled = true;
            }
            catch (Exception ex)
            {
            }
        }
        /***********************************************************************************************/
        private void txtRouting_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                txtRouting_Leave(sender, e);
            else if (e.KeyCode == Keys.Tab)
                txtRouting_Leave(sender, e);
        }
        /***********************************************************************************************/
        private void txtRouting_Leave(object sender, EventArgs e)
        {
            string routingNumber = txtRouting.Text.Trim();
            if (routingNumber.Length != 9)
            {
                MessageBox.Show("***ERROR*** Routing Number Must Be 9 Digits");
                return;
            }
            if ( !ValidateRoutingNumberDigits ( routingNumber))
            {
                lblRouting.Text = "Invalid Routing #";
                lblRouting.ForeColor = Color.Red;
                return;
            }
            lblRouting.Text = "Valid";
            lblRouting.ForeColor = Color.Green;
        }
        /***********************************************************************************************/
        private bool ValidateRoutingNumberDigits(string RoutingNumber)
        {
            long Sum = 0L;
            Sum = 3L * RoutingNumber.Substring(0, 1).ObjToInt64();
            Sum += 7L * RoutingNumber.Substring(1, 1).ObjToInt64();
            Sum += 1L * RoutingNumber.Substring(2, 1).ObjToInt64();

            Sum += 3L * RoutingNumber.Substring(3, 1).ObjToInt64();
            Sum += 7L * RoutingNumber.Substring(4, 1).ObjToInt64();
            Sum += 1L * RoutingNumber.Substring(5, 1).ObjToInt64();

            Sum += 3L * RoutingNumber.Substring(6, 1).ObjToInt64();
            Sum += 7L * RoutingNumber.Substring(7, 1).ObjToInt64();
            Sum += 1L * RoutingNumber.Substring(8, 1).ObjToInt64();

            bool valid = false;
            if ((Sum % 10) == 0)
                valid = true;
            return valid;
        }
        /***********************************************************************************************/
        private void dateBeginACH_Enter(object sender, EventArgs e)
        {
            string date = dateBeginACH.Text;
            DevExpress.XtraEditors.DateEdit date1 = (DevExpress.XtraEditors.DateEdit)sender;
            date = date1.Text;
            if (G1.validate_date(date))
            {
                DateTime ddate = date.ObjToDateTime();
                date1.EditValue = ddate.ToString("MM/dd/yyyy");
                date = date1.Text;
                btnSave.Enabled = true;
                btnSave.Show();
            }
        }
        /***********************************************************************************************/
        private void dateBeginACH_Leave(object sender, EventArgs e)
        {
            string date = dateBeginACH.Text;
            DevExpress.XtraEditors.DateEdit date1 = (DevExpress.XtraEditors.DateEdit)sender;
            date = date1.Text;
            if (G1.validate_date(date))
            {
                DateTime ddate = date.ObjToDateTime();
                date1.Text = ddate.ToString("MM/dd/yyyy");
                btnSave.Enabled = true;
                btnSave.Show();
            }
        }
        /***********************************************************************************************/
        private void dateBeginACH_EditValueChanged(object sender, EventArgs e)
        {
            string date = dateBeginACH.Text;
            DevExpress.XtraEditors.DateEdit date1 = (DevExpress.XtraEditors.DateEdit)sender;
            date = date1.Text;
            if (G1.validate_date(date))
            {
                DateTime ddate = date.ObjToDateTime();
                btnSave.Enabled = true;
                btnSave.Show();
            }
        }
        /***********************************************************************************************/
        private void chkFilterInactive_CheckedChanged(object sender, EventArgs e)
        {
            gridMain5.RefreshData();
            dgv5.Refresh();
        }
        /***********************************************************************************************/
        private void gridMain5_CustomRowFilter(object sender, RowFilterEventArgs e)
        {
            if (chkFilterInactive.Checked )
            {
                int row = e.ListSourceRow;
                DataTable dt = (DataTable)dgv5.DataSource;
                if (dt == null)
                    return;
                string contractNumber = dt.Rows[row]["contractNumber"].ObjToString();
                if (contractNumber.IndexOf("OO") >= 0 || contractNumber.IndexOf("MM") >= 0)
                {
                    e.Visible = false;
                    e.Handled = true;
                    return;
                }
            }
        }
        /***********************************************************************************************/
        private void btnSavePolicies_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv5.DataSource;
            string record = "";
            string report = "";
            string mod = "";
            this.Cursor = Cursors.WaitCursor;

            bool doMod = false;
            if (G1.get_column_number(dt, "mod") >= 0)
                doMod = true;

            double newPremium = 0D;
            double monthlyPremium = 0D;
            double historicPremium = 0D;
            double monthlySecNat = 0D;
            double monthly3rdParty = 0D;

            DateTime lastDatePaid = DateTime.Now;
            DateTime dueDate = DateTime.Now;
            double paymentAmount = 0D;
            double numMonthPaid = 0D;
            bool prepaid = false;

            CustomerDetails.CalcMonthlyPremium ( dt, "myDeceasedDate", ref monthlyPremium, ref historicPremium, ref monthlySecNat, ref monthly3rdParty);
            newPremium = monthlyPremium - monthlySecNat;
            if (newPremium != policyOldPremium)
                prepaid = FunPayments.CheckPayerPrepaid(workPayer, policyOldPremium, newPremium, ref lastDatePaid, ref dueDate, ref paymentAmount, ref numMonthPaid );

            DateTime deceasedDate = DateTime.Now;

            double creditPayment = 0D;
            double days = 0;
            string deceasedPolicies = "";
            double TotalDeceasedPremiums = 0D;
            double premium = 0D;
            double credit = 0D;

            for ( int i=0; i<dt.Rows.Count; i++)
            {
                record = dt.Rows[i]["record"].ObjToString();
                if (String.IsNullOrWhiteSpace(record))
                    continue;
                report = dt.Rows[i]["report"].ObjToString();
                if (doMod)
                {
                    mod = dt.Rows[i]["mod"].ObjToString();
                    if ( mod.ToUpper() == "Y" )
                    {
                        deceasedDate = dt.Rows[i]["myDeceasedDate"].ObjToDateTime();
                        if (deceasedDate.Year > 100)
                        {
                            //G1.update_db_table("policies", "record", record, new string[] { "report", report, "deceasedDate", deceasedDate.ToString("MM/dd/yyyy") });
                            if ( prepaid )
                            {
                                premium = dt.Rows[i]["premium"].ObjToDouble() * numMonthPaid;
                                TimeSpan ts = deceasedDate - lastDatePaid;
                                if ( ts.TotalDays > 0 )
                                {
                                    days = ts.TotalDays;
                                    ts = dueDate - deceasedDate;
                                    if (ts.TotalDays > 0)
                                    {
                                        creditPayment += (ts.TotalDays - days) * (paymentAmount / ts.TotalDays);
                                        deceasedPolicies += dt.Rows[i]["policyNumber"].ObjToString() + "~";
                                    }
                                }
                            }
                        }
                        else
                            G1.update_db_table("policies", "record", record, new string[] { "report", report, "deceasedDate", "" });
                    }
                    else
                        G1.update_db_table("policies", "record", record, new string[] { "report", report });
                }
                else
                    G1.update_db_table("policies", "record", record, new string[] {"report", report });
            }


            if (newPremium != policyOldPremium)
            {
                if ( creditPayment > 0D)
                {
                    string message = "Payer " + workPayer + " has Deceased Policies with Payment Credit of " + G1.ReformatMoney ( creditPayment ) + " !\n";
                    if ( !String.IsNullOrWhiteSpace ( deceasedPolicies ))
                        message += deceasedPolicies + "\n";
                    //Messages.SendTheMessage(LoginForm.username, "cjenkins", "Policy Deceased Information", message);
                    Messages.SendTheMessage(LoginForm.username, "robby", "Policy Deceased Information", message);
                }
            }

            policiesModified = false;
            btnSavePolicies.Hide();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void chkHonor3rdParty_CheckedChanged(object sender, EventArgs e)
        {
            LoadPolicies(policyDueDate8);
        }
        /***********************************************************************************************/
        private void gridMain5_RowCellStyle(object sender, RowCellStyleEventArgs e)
        {
            GridView View = sender as GridView;
            if (e.Column.FieldName.ToUpper() == "PREMIUM")
            {
                bool doColor = false;
                DataTable dt = (DataTable)dgv5.DataSource;
                DateTime date = View.GetRowCellValue(e.RowHandle, "myDeceasedDate").ObjToDateTime();
                if (date.Year > 100)
                    doColor = true;
                string str = View.GetRowCellValue(e.RowHandle, "lapsed").ObjToString();
                if (!String.IsNullOrWhiteSpace(str))
                {
                    if (str.ToUpper() == "Y")
                        doColor = true;
                }
                if (doColor)
                {
                    e.Appearance.ForeColor = Color.Red;
                }
            }
            if (!chkHonor.Checked)
            {
                if (e.Column.FieldName.ToUpper() == "COMPANYCODE")
                {
                    if (secNatDt == null)
                        secNatDt = G1.get_db_data("Select * from `secnat`;");
                    string companyCode = View.GetRowCellValue(e.RowHandle, "companyCode").ObjToString();
                    DataRow[] dRows = secNatDt.Select("cc='" + companyCode + "'");
                    if (dRows.Length > 0)
                        e.Appearance.ForeColor = Color.Red;
                    else
                    {
                        string report = View.GetRowCellValue(e.RowHandle, "report").ObjToString();
                        if (report.ToUpper() != "NOT THIRD PARTY")
                            e.Appearance.ForeColor = Color.Blue;
                    }
                }
            }
        }
        /***********************************************************************************************/
        private void gridMain2_CustomDrawCell(object sender, RowCellCustomDrawEventArgs e)
        {
            if (e.Column.FieldName.ToUpper() == "PARAMETER")
                return;
            if (e.Column.FieldName.ToUpper() == "CUSTOMER")
                return;
            if (e.Column.FieldName.ToUpper() == "CUSTOMERDATA")
                return;
            if (e.Column.FieldName.ToUpper() == "NUM")
            {
                if (e.RowHandle >= 0)
                {
                    string num = (e.RowHandle + 1).ToString();
                    e.DisplayText = num;
                }
                return;
            }

            int rowHandle = e.RowHandle;
            int row = gridMain2.GetDataSourceRowIndex(rowHandle);
            DataTable dt = (DataTable)dgv2.DataSource;
            string type = dt.Rows[row]["type"].ObjToString();
            string data = dt.Rows[row]["data"].ObjToString();
            if (type.ToUpper().IndexOf("DECIMAL") >= 0)
            {
                double dvalue = data.ObjToDouble();
                //dr["data"] = dvalue.ToString("###.00");
                e.DisplayText = dvalue.ToString("###.00");
            }
            else if (type.ToUpper().IndexOf("MYSQLDATETIME") >= 0)
            {
                if (data.IndexOf("0000") >= 0)
                {
                    //dr["data"] = "0/0/0000";
                    e.DisplayText = "0/0/0000";
                }
                else
                {
                    if (G1.validate_date(data))
                    {
                        DateTime date = data.ObjToDateTime();
                        try
                        {
                            MySqlDateTime myDate = (MySqlDateTime)G1.DTtoMySQLDT(date);
                            //dr["data"] = myDate.Month.ToString("D2") + "/" + myDate.Day.ToString("D2") + "/" + myDate.Year.ToString("D4");
                            e.DisplayText = myDate.Month.ToString("D2") + "/" + myDate.Day.ToString("D2") + "/" + myDate.Year.ToString("D4");
                        }
                        catch
                        {

                        }
                    }
                }
            }
        }
        /***********************************************************************************************/
        private void gridMain6_CustomRowFilter(object sender, RowFilterEventArgs e)
        {
            if (!G1.isField())
                return;
            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv6.DataSource;
            string detail = dt.Rows[row]["detail"].ObjToString().ToUpper();
            if ( detail != "GOODS AND SERVICES")
            {
                e.Visible = false;
                e.Handled = true;
                return;
            }
        }
        /***********************************************************************************************/
        private void dateBeginCC_EditValueChanged(object sender, EventArgs e)
        {
            string date = dateBeginCC.Text;
            DevExpress.XtraEditors.DateEdit date1 = (DevExpress.XtraEditors.DateEdit)sender;
            date = date1.Text;
            if (G1.validate_date(date))
            {
                DateTime ddate = date.ObjToDateTime();
                btnSave.Enabled = true;
                btnSave.Show();
            }
        }
        /***********************************************************************************************/
        private void dateBeginCC_Enter(object sender, EventArgs e)
        {
            string date = dateBeginCC.Text;
            DevExpress.XtraEditors.DateEdit date1 = (DevExpress.XtraEditors.DateEdit)sender;
            date = date1.Text;
            if (G1.validate_date(date))
            {
                DateTime ddate = date.ObjToDateTime();
                date1.EditValue = ddate.ToString("MM/dd/yyyy");
                date = date1.Text;
                btnSave.Enabled = true;
                btnSave.Show();
            }

        }
        /***********************************************************************************************/
        private void dateBeginCC_Leave(object sender, EventArgs e)
        {
            string date = dateBeginCC.Text;
            DevExpress.XtraEditors.DateEdit date1 = (DevExpress.XtraEditors.DateEdit)sender;
            date = date1.Text;
            if (G1.validate_date(date))
            {
                DateTime ddate = date.ObjToDateTime();
                date1.Text = ddate.ToString("MM/dd/yyyy");
                btnSave.Enabled = true;
                btnSave.Show();
            }
        }
        /***********************************************************************************************/
        private void txtCCExpirationDate_EditValueChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            btnSave.Enabled = true;
            btnSave.Show();
        }
        /***********************************************************************************************/
        private bool CCDateError = false;
        private void txtCCExpirationDate_Leave(object sender, EventArgs e)
        {
            CCDateError = false;
            string str = txtCCExpirationDate.Text;
            if ( str.IndexOf ( "/" ) < 0 )
            {
                MessageBox.Show("*** ERROR *** Expiration Date Appears to be Invalid?", "Expiration Date Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                CCDateError = true;
                return;
            }
            string[] Lines = str.Split('/');
            if ( Lines.Length < 2 )
            {
                MessageBox.Show("*** ERROR *** Expiration Date Appears to be Invalid?", "Expiration Date Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                CCDateError = true;
                return;
            }
            str = Lines[0].Trim();
            int month = str.ObjToInt32();
            if ( month <= 0 || month > 12 )
            {
                MessageBox.Show("*** ERROR *** Expiration Date Appears To Have An Invalid Month?", "Expiration Date Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                CCDateError = true;
                return;
            }
        }
        /***********************************************************************************************/
        private void xtraTabControl2_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            CCDateError = false;
            if (!gotCC)
                return;

            string str = txtCCExpirationDate.Text;
            if (str.IndexOf("/") < 0)
            {
                MessageBox.Show("*** ERROR *** Expiration Date Appears to be Invalid?", "Expiration Date Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                e.Cancel = true;
                return;
            }
            string[] Lines = str.Split('/');
            if (Lines.Length < 2)
            {
                MessageBox.Show("*** ERROR *** Expiration Date Appears to be Invalid?", "Expiration Date Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                e.Cancel = true;
                return;
            }
            str = Lines[0].Trim();
            int month = str.ObjToInt32();
            if (month <= 0 || month > 12)
            {
                MessageBox.Show("*** ERROR *** Expiration Date Appears To Have An Invalid Month?", "Expiration Date Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                e.Cancel = true;
                return;
            }
        }
        /***********************************************************************************************/
        private void txtMeetingNumber_EditValueChanged(object sender, EventArgs e)
        {
            CheckEnableSave();
            //btnSave.Show();
            //btnSave.Refresh();
        }
        /***********************************************************************************************/
        private void picAgreement_Click(object sender, EventArgs e)
        {
            string filename = this.picAgreement.Tag.ObjToString();
            if (!String.IsNullOrWhiteSpace(filename))
            {
                string title = "Agreement for (" + workContract + ") ";
                string cmd = "Select * from `" + customersFile + "` where `contractNumber` = '" + workContract + "';";
                DataTable dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    filename = dx.Rows[0]["agreementFile"].ObjToString();
                    string firstName = dx.Rows[0]["firstName"].ObjToString();
                    string lastName = dx.Rows[0]["lastName"].ObjToString();
                    title = "Agreement for (" + workContract + ") " + firstName + " " + lastName;
                    string record = this.picAgreement.Tag.ObjToString();
                    if (record != "-1")
                    {
                        //CustomerDetails.ShowPDfImage(record, title, title, workContract);
                        Customers.ShowPDfImage(record, title, filename);
                    }
                }
            }
        }
        /***********************************************************************************************/
        private void gridTrust_RowStyle(object sender, RowStyleEventArgs e)
        {
            GridView view = sender as GridView;
            if (e.RowHandle > 0)
            {
                string trust = view.GetRowCellDisplayText(e.RowHandle, view.Columns["trustCompany"]);
                if (trust != null)
                {
                    if (trust.ToUpper().IndexOf ( "FDLIC" ) >= 0  )
                    {
                        e.Appearance.BackColor = Color.Pink;
                        e.Appearance.BackColor2 = Color.Pink;
                        e.HighPriority = true;
                    }
                    else if (trust.ToUpper().IndexOf("UNITY") >= 0)
                    {
                        e.Appearance.BackColor = Color.LightGreen;
                        e.Appearance.BackColor2 = Color.LightGreen;
                        e.HighPriority = true;
                    }
                    else if (trust.ToUpper().IndexOf("SECURITY") >= 0)
                    {
                        e.Appearance.BackColor = Color.LightBlue;
                        e.Appearance.BackColor2 = Color.LightBlue;
                        e.HighPriority = true;
                    }
                    else if (trust.ToUpper().IndexOf("FORETHOUGHT") >= 0)
                    {
                        e.Appearance.BackColor = Color.LightYellow;
                        e.Appearance.BackColor2 = Color.LightYellow;
                        e.Appearance.ForeColor = Color.Black;
                        e.HighPriority = true;
                    }
                }
            }
        }
        /***********************************************************************************************/
        private void viewFamilyMembersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain5.GetFocusedDataRow();
            int rowHandle = gridMain5.FocusedRowHandle;
            int row = gridMain5.GetDataSourceRowIndex(rowHandle);
            string contract = dr["contractNumber"].ObjToString();
            string payer = dr["payer"].ObjToString();
            string name = dr["policyfullname"].ObjToString();
            string serviceId = dr["ServiceId"].ObjToString();
            if ( String.IsNullOrWhiteSpace ( serviceId ))
            {
                MessageBox.Show("***ERROR*** This Policy is not in a Funeral!", "Empty ServiceId Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }

            string cmd = "Select * from `fcust_extended` WHERE `ServiceId` = '" + serviceId + "';";
            DataTable dt = G1.get_db_data(cmd);
            if ( dt.Rows.Count <= 0 )
            {
                MessageBox.Show("***ERROR*** Cannot locate for Funeral for Service Id!", "Funeral ServiceId Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }

            contract = dt.Rows[0]["contractNumber"].ObjToString();

            this.Cursor = Cursors.WaitCursor;

            FunFamilyNew funForm = new FunFamilyNew (contract, "Family Members", true);
            funForm.StartPosition = FormStartPosition.CenterScreen;
            funForm.Show();

            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
    }
}