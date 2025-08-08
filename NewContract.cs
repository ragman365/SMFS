using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using GeneralLib;
using Word = Microsoft.Office.Interop.Word;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class NewContract : DevExpress.XtraEditors.XtraForm
    {
        private string workType = "";
        private bool OpenClosingFuneral = false;
        /***********************************************************************************************/
        public NewContract( string type = "" )
        {
            InitializeComponent();
            workType = type;
        }
        /***********************************************************************************************/
        private void NewContract_Load(object sender, EventArgs e)
        {
            loadLocatons();
            if ( workType.Trim().ToUpper() == "FUNERAL")
            {
                tabControl1.TabPages.Remove(tabPage1);
                tabControl1.TabPages.Remove(tabPage2);
                tabControl1.TabPages.Remove(tabPage4);
                cmbLocFuneral.Hide();
                label16.Hide();
                txtFunService.Focus();
                txtFunService.Select();
            }
            if ( G1.isField())
            {
                try
                {
                    tabControl1.TabPages.Remove(tabPage1);
                    tabControl1.TabPages.Remove(tabPage2);
                    tabControl1.TabPages.Remove(tabPage4);
                }
                catch ( Exception ex)
                {
                }
            }
        }
        /***********************************************************************************************/
        private void loadLocatons()
        {
            string cmd = "Select * from `funeralhomes` order by `keycode`;";
            DataTable locDt = G1.get_db_data(cmd);
            cmbLoc.DataSource = locDt;
            cmbLocFuneral.DataSource = locDt;

            cmd = "Select * from `cemeteries` order by `loc`;";
            locDt = G1.get_db_data(cmd);
            cmbLocCemetery.DataSource = locDt;
        }
        /***************************************************************************************/
        private bool ValidateTrustData ( ref string contract )
        {
            contract = "";
            string location = this.cmbLoc.Text;
            string trust = this.cmbTrust.Text;
            string cNum = this.contractNumber.Text;
            string firstName = this.firstName.Text;
            string lastName = this.lastName.Text;
            if (String.IsNullOrWhiteSpace(location))
                return false;
            if (String.IsNullOrWhiteSpace(cNum))
                return false;
            if (String.IsNullOrWhiteSpace(firstName))
                return false;
            if (String.IsNullOrWhiteSpace(lastName))
                return false;
            contract = cNum;
            return true;
        }
        /***************************************************************************************/
        private bool ValidateFuneralData(ref string contract)
        {
            contract = "";
            string location = this.cmbLocFuneral.Text;
            string serviceId = this.txtFunService.Text;
            string cNum = this.txtFunContract.Text;
            string firstName = this.txtFunFirstName.Text;
            string lastName = this.txtFunLastName.Text;
            if (String.IsNullOrWhiteSpace(serviceId))
                return false;
            if (String.IsNullOrWhiteSpace(location))
                return false;
            if (String.IsNullOrWhiteSpace(cNum))
                return false;
            if (String.IsNullOrWhiteSpace(firstName))
                return false;
            if (String.IsNullOrWhiteSpace(lastName))
                return false;
            contract = cNum;
            return true;
        }
        /***************************************************************************************/
        private bool ValidateCemeteryData(ref string contract)
        {
            contract = "";
            string location = this.cmbLocCemetery.Text;
            string cNum = this.txtCemeteryContract.Text;
            string firstName = this.txtCemeteryFirstName.Text;
            string lastName = this.txtCemeteryLastName.Text;
            if (String.IsNullOrWhiteSpace(location))
                return false;
            if (String.IsNullOrWhiteSpace(cNum))
                return false;
            if (String.IsNullOrWhiteSpace(firstName))
                return false;
            if (String.IsNullOrWhiteSpace(lastName))
                return false;
            contract = cNum;
            return true;
        }
        /***************************************************************************************/
        public delegate void d_void_eventdone_string(string contract);
        public event d_void_eventdone_string SelectDone;
        protected void OnSelectDone( string contract )
        {
            SelectDone?.Invoke(contract);
        }
        /***********************************************************************************************/
        private void btnAdd_Click(object sender, EventArgs e)
        {
            string contract = "";
            if ( !ValidateTrustData ( ref contract ))
            {
                MessageBox.Show("***ERROR*** You must enter valid Data, Location, Trust, Numeric Contract#, First Name, and Last Name!");
                return;
            }
            string cmd = "Select * from `contracts` where `contractNumber` = '" + contract + "';";
            DataTable dt = G1.get_db_data(cmd);
            if ( dt.Rows.Count > 0 )
            {
                MessageBox.Show("***ERROR*** Contract " + contract + " already exists!");
                return;
            }

            string record = G1.create_record("contracts", "notes", "-1");
            if (G1.BadRecord("contracts", record))
                return;
            string serviceId = this.txtServiceID.Text;
            G1.update_db_table("contracts", "record", record, new string[] { "contractNumber", contract, "ServiceId", serviceId });

            cmd = "Select * from `customers` where `contractNumber` = '" + contract + "';";
            dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                MessageBox.Show("***ERROR*** Customer Contract " + contract + " already exists!");
                return;
            }

            record = G1.create_record("customers", "firstName", "-1");
            if (G1.BadRecord("customers", record))
                return;

            string firstName = this.firstName.Text.ToUpper();
            string lastName = this.lastName.Text.ToUpper();
            string middleName = this.middleName.Text.ToUpper();
            G1.update_db_table("customers", "record", record, new string[] { "contractNumber", contract, "firstName", firstName, "lastName", lastName, "middleName", middleName });

            G1.AddToAudit(LoginForm.username, "Customers", "New Trust Customer", "Added", contract);

            OnSelectDone(contract);
            this.Close();
        }
        /***********************************************************************************************/
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        /***********************************************************************************************/
        private string GetTitle( string location )
        {
            string rv = "";
            string cmd = "Select * from `funeralhomes` where `keycode` = '" + location + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
                rv = dt.Rows[0]["name"].ObjToString();
            return rv;
        }
        /***********************************************************************************************/
        private void FindAndReplace ( string find, string replace, ref string rtf )
        {
            int len = find.Length;
            int len2 = replace.Length;
            if (len2 < len)
                replace = replace.PadRight(len);
            rtf = rtf.Replace(find, replace);
        }
        /***********************************************************************************************/
        private void txtPayer_TextChanged(object sender, EventArgs e)
        {
            string cmd = "Select COUNT(*) from `icustomers`;";
            DataTable dx = G1.get_db_data(cmd);
            int totalCustomers = dx.Rows[0][0].ObjToInt32();
            totalCustomers++;
            string contract = "ZZ" + totalCustomers.ToString("D7");
            this.contractNumber.Text = contract;
        }
        /***********************************************************************************************/
        private void btnInsAdd_Click(object sender, EventArgs e)
        {
            string contract = "";
            string payer = "";
            if (!ValidatePayerData(ref contract))
            {
                MessageBox.Show("***ERROR*** You must enter valid Data, Payer #, First Name, and Last Name!");
                return;
            }
            string cmd = "Select * from `icontracts` where `contractNumber` = '" + contract + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                MessageBox.Show("***ERROR*** Insurance Contract " + contract + " already exists!");
                return;
            }

            string record = G1.create_record("icontracts", "notes", "-1");
            if (G1.BadRecord("icontracts", record))
                return;
            string serviceId = txtInsServiceId.Text;
            G1.update_db_table("icontracts", "record", record, new string[] { "contractNumber", contract, "ServiceId", serviceId });

            cmd = "Select * from `icustomers` where `contractNumber` = '" + contract + "';";
            dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                MessageBox.Show("***ERROR*** Insurance Customer Contract " + contract + " already exists!");
                return;
            }

            record = G1.create_record("icustomers", "firstName", "-1");
            if (G1.BadRecord("icustomers", record))
                return;

            string firstName = txtInsFirstName.Text.ToUpper();
            string lastName = txtInsLastName.Text.ToUpper();
            string middleName = txtInsMiddleName.Text.ToUpper();
            payer = txtPayer.Text.ToUpper();
            G1.update_db_table("icustomers", "record", record, new string[] { "contractNumber", contract, "firstName", firstName, "lastName", lastName, "payer", payer, "middleName", middleName });

            record = G1.create_record("payers", "empty", "-1");
            if (G1.BadRecord("payers", record))
                return;
            G1.update_db_table("payers", "record", record, new string[] { "contractNumber", contract, "firstName", firstName, "lastName", lastName, "payer", payer });

            G1.AddToAudit(LoginForm.username, "Customers", "New Insurance Customer", "Added", contract);

            OnSelectDone(contract);
            this.Close();
        }
        /***************************************************************************************/
        private bool ValidatePayerData(ref string contract)
        {
            contract = "";
            string payer = this.txtPayer.Text;
            string firstName = txtInsFirstName.Text;
            string lastName = txtInsLastName.Text;
            if (String.IsNullOrWhiteSpace(payer))
                return false;
            if (String.IsNullOrWhiteSpace(firstName))
                return false;
            if (String.IsNullOrWhiteSpace(lastName))
                return false;
            string cmd = "Select * from `icustomers` order by record DESC LIMIT 1;";
            DataTable dx = G1.get_db_data(cmd);
            string record = dx.Rows[0]["record"].ObjToString();
            cmd = "Select * from `icontracts` order by record DESC LIMIT 1;";
            dx = G1.get_db_data(cmd);
            string record2 = dx.Rows[0]["record"].ObjToString();
            int rec1 = Convert.ToInt32(record);
            int rec2 = Convert.ToInt32(record2);
            if (rec2 > rec1)
                rec1 = rec2;
            int totalCustomers = rec1.ObjToInt32();
            totalCustomers++;
            contract = "ZZ" + totalCustomers.ToString("D7");
            return true;
        }
        /***********************************************************************************************/
        private void btnInsCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        /***********************************************************************************************/
        private void btnFunAdd_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            string contract = "";
            if (!ValidateFuneralData(ref contract))
            {
                MessageBox.Show("***ERROR*** You must enter valid Data, Location, Service ID, Contract#, First Name, and Last Name!", "Bad Data Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                this.Cursor = Cursors.Default;
                return;
            }
            DateTime deceasedDate = txtDOD.Text.ObjToDateTime();
            if ( deceasedDate.Year < 1800)
            {
                MessageBox.Show("***ERROR*** You must enter valid Deceased Date!", "Bad Data Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                this.Cursor = Cursors.Default;
                return;
            }

            string record = "";
            string serviceId = this.txtFunService.Text.ToUpper();
            bool good = FunCustomer.ValidateServiceId(serviceId);
            if ( !good )
            {
                MessageBox.Show("***ERROR*** You must enter a valid Service Id\nor Merchandise Code!", "Invalid Service ID Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                this.Cursor = Cursors.Default;
                return;
            }

            if ( CheckServiceIdExists ( serviceId))
            {
                MessageBox.Show("***ERROR*** A Service ID of " + serviceId + " Already Exists Somewhere!", "Service ID EXISTS Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                this.Cursor = Cursors.Default;
                return;
            }

            if ( CheckForOtherDeceased ( contract ))
            {
                this.Cursor = Cursors.Default;
                return;
            }

            string cmd = "";
            bool badSSN = false;

            cmd = "Select * from `customers` where `contractNumber` = '" + contract + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                string ssn = dt.Rows[0]["ssn"].ObjToString();
                if (String.IsNullOrWhiteSpace(ssn))
                    badSSN = true;
                else if (ssn == "0")
                    badSSN = true;
                else if (ssn == "123456789")
                    badSSN = true;
                else if (ssn == "000000000")
                    badSSN = true;
                if ( badSSN )
                {
                    MessageBox.Show("***ERROR*** Selected PreNeed SSN is Blank or Bad (" + ssn + ") !", "Bad SSN Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    this.Cursor = Cursors.Default;
                    return;
                }
                record = dt.Rows[0]["record"].ObjToString();

                G1.update_db_table("customers", "record", record, new string[] { "ServiceId", serviceId, "deceasedDate", deceasedDate.ToString("yyyy-MM-dd") });
            }

            cmd = "Select * from `contracts` where `contractNumber` = '" + contract + "';";
            dt = G1.get_db_data(cmd);
            if ( dt.Rows.Count > 0 )
            {
                record = dt.Rows[0]["record"].ObjToString();
                G1.update_db_table("contracts", "record", record, new string[] { "ServiceId", serviceId, "deceasedDate", deceasedDate.ToString("yyyy-MM-dd") });
            }

            bool customerExists = false;

            string contractFile = "fcontracts";
            string customerFile = "fcustomers";

            cmd = "Select * from `" + contractFile + "` where `contractNumber` = '" + contract + "';";
            dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                record = G1.create_record(contractFile, "notes", "-1");
            else
            {
                record = dt.Rows[0]["record"].ObjToString();
                string oldServiceId = dt.Rows[0]["ServiceId"].ObjToString();
                if ( !String.IsNullOrWhiteSpace ( oldServiceId ))
                {
                    MessageBox.Show("***ERROR*** A Service ID of " + oldServiceId + " Already Exists for this Customer!", "Service ID EXISTS Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Cursor = Cursors.Default;
                    return;
                }
            }

            if (G1.BadRecord(contractFile, record))
            {
                this.Cursor = Cursors.Default;
                return;
            }
            G1.update_db_table(contractFile, "record", record, new string[] { "contractNumber", contract, "ServiceId", serviceId, "deceasedDate", deceasedDate.ToString("yyyy-MM-dd") });

            cmd = "Select * from `" + customerFile + "` where `contractNumber` = '" + contract + "';";
            dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                record = G1.create_record(customerFile, "firstName", "-1");
            else
            {
                record = dt.Rows[0]["record"].ObjToString();
                customerExists = true;
            }
            if (G1.BadRecord(customerFile, record))
            {
                this.Cursor = Cursors.Default;
                return;
            }

            string firstName = this.txtFunFirstName.Text;
            string lastName = this.txtFunLastName.Text;
            string middleName = this.txtFunMName.Text.Trim();
            if ( !customerExists )
                G1.update_db_table(customerFile, "record", record, new string[] { "contractNumber", contract, "firstName", firstName, "lastName", lastName, "middleName", middleName, "ServiceId", serviceId, "deceasedDate", deceasedDate.ToString("yyyy-MM-dd") });

            FunFamily.ConfirmCustExtended(contract, serviceId, "", "fcust_extended");

            G1.AddToAudit(LoginForm.username, customerFile, "New Funeral Customer", "Added", contract + "-ServiceId:" + serviceId);

            if ( contract.ToUpper().IndexOf ( "SX") < 0 )
            {
                CustomerDetails.CopyAllContractInfo(contract);
                FunFamily.ConfirmCustExtended(contract, serviceId, "", "fcust_extended");
                FunFamily.ConfirmCustExtended(contract, serviceId, "", "cust_extended");
            }

            OpenClosingFuneral = this.chkOpenClose.Checked;

            if (OpenClosingFuneral)
            {
                cmd = "Select * from `fcust_extended` WHERE `contractNumber` = '" + contract + "';";
                DataTable ddd = G1.get_db_data(cmd);
                if (ddd.Rows.Count > 0)
                {
                    string name = GetUserFirstLastName();
                    record = ddd.Rows[0]["record"].ObjToString();
                    G1.update_db_table("fcust_extended", "record", record, new string[] { "OpenCloseFuneral", "Y", "Funeral Creator", name });
                }
            }
            else
            {
                cmd = "Select * from `fcust_extended` WHERE `contractNumber` = '" + contract + "';";
                DataTable ddd = G1.get_db_data(cmd);
                if (ddd.Rows.Count > 0)
                {
                    string name = GetUserFirstLastName();
                    record = ddd.Rows[0]["record"].ObjToString();
                    G1.update_db_table("fcust_extended", "record", record, new string[] { "Funeral Creator", name });
                }
            }

            OnSelectDone(contract);
            this.Close();
        }
        /***********************************************************************************************/
        public static string GetUserFirstLastName ()
        {
            string name = "";
            string user = LoginForm.username.Trim();
            string cmd = "Select * from `users` WHERE `username` = '" + user + "';";
            DataTable dt = G1.get_db_data(cmd);
            if ( dt.Rows.Count > 0 )
            {
                name = dt.Rows[0]["firstName"].ObjToString() + " " + dt.Rows[0]["lastName"].ObjToString();
                name = name.Trim();
            }
            return name;
        }
        /***********************************************************************************************/
        public static bool CheckForOtherDeceased ( string contractNumber )
        {
            bool found = false;
            DateTime deceasedDate = DateTime.Now;
            string cmd = "Select * from `customers` where `contractNumber` = '" + contractNumber + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                string workSSN = dt.Rows[0]["ssn"].ObjToString();
                workSSN = workSSN.Replace("-", "");
                if (!String.IsNullOrWhiteSpace(workSSN))
                {
                    string cNum = "";
                    DataTable multiSsnDt = Funerals.GetMultipleSSN(workSSN);
                    for (int i = 0; i < multiSsnDt.Rows.Count; i++)
                    {
                        cNum = multiSsnDt.Rows[i]["contractNumber"].ObjToString();
                        if (cNum == contractNumber)
                            continue;
                        cmd = "Select * from `customers` where `contractNumber` = '" + cNum + "';";
                        dt = G1.get_db_data(cmd);
                        if (dt.Rows.Count > 0)
                        {
                            deceasedDate = dt.Rows[0]["deceasedDate"].ObjToDateTime();
                            if ( deceasedDate.Year > 100 )
                            {
                                found = true;
                                MessageBox.Show("***ERROR*** Contract (" + cNum + ") with same SSN (" + workSSN + ") is already deceased!\nYou must investigate!", "Deceased Contract Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                break;
                            }
                        }
                    }
                }
            }
            return found;
        }
        /***********************************************************************************************/
        public static bool CheckServiceIdExists ( string serviceId, string contractNumber = "", string ssn = "" )
        {
            if (String.IsNullOrWhiteSpace(serviceId))
                return false;
            string cmd = "Select * from `fcust_extended` where `serviceId` = '" + serviceId + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count >= 2)
                return true;
            if (dt.Rows.Count > 0)
            {
                if (!String.IsNullOrWhiteSpace(contractNumber))
                {
                    if (dt.Rows[0]["contractNumber"].ObjToString() != contractNumber)
                        return true;
                }
            }
            return false;
        }
        /***********************************************************************************************/
        private void btnFunCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        /***********************************************************************************************/
        private void pictureAdd_Click(object sender, EventArgs e)
        { // Determine Service ID

            DateTime date = DateTime.Now;
            int year = date.Year % 100;
            string sYear = year.ToString("D02");

            string loc = cmbLocFuneral.Text.Trim();
            if (String.IsNullOrWhiteSpace(loc))
            {
                MessageBox.Show("***ERROR*** You must select a Location for this funeral!");
                return;
            }
            DataTable ddx = (DataTable)cmbLocFuneral.DataSource;
            DataRow[] dR = ddx.Select("keycode='" + loc + "'");
            if ( dR.Length <= 0 )
            {
                MessageBox.Show("***ERROR*** Selecting Funeral KeyCode !");
                return;
            }

            string atNeedCode = dR[0]["atneedcode"].ObjToString();
            if (String.IsNullOrWhiteSpace(atNeedCode))
            {
                MessageBox.Show("***ERROR*** Selecting Funeral AtNeedCode !");
                return;
            }

            string serviceId = txtFunService.Text.Trim();
            if ( String.IsNullOrWhiteSpace ( serviceId))
            {
                MessageBox.Show("***ERROR*** You must enter a valid Service Id!", "Invalid Service ID Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }

            bool good = FunCustomer.ValidateServiceId(serviceId);
            if ( !good )
            {
                MessageBox.Show("***ERROR*** You must enter a valid Service Id\nor Merchandise Code!", "Invalid Service ID Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }

            string cmd = "Select * from `fcust_extended` WHERE `serviceId` = '" + serviceId + "';";
            DataTable dt = G1.get_db_data(cmd);
            if ( dt.Rows.Count > 0 )
            {
                MessageBox.Show("***ERROR*** Service ID is already being used!", "Duplicate Service ID Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }

            cmd = "Select * from `fcontracts` where `contractNumber` LIKE '%" + loc + sYear + "%' ORDER by `contractNumber` DESC LIMIT 10;";
            dt = G1.get_db_data(cmd);

            int maxContract = 1;
            string newContract = "";
            string contract = "";
            if ( dt.Rows.Count > 0 )
            {
                contract = dt.Rows[0]["contractNumber"].ObjToString();
                contract = contract.Replace(loc + sYear, "");
                if (G1.validate_numeric(contract))
                    maxContract = contract.ObjToInt32() + 1;
            }

            //cmd = "Select * from `inventory` where `ServiceID` LIKE '%" + atNeedCode + sYear + "%' ORDER BY `ServiceID` DESC LIMIT 10;";
            //DataTable dx = G1.get_db_data(cmd);

            //int maxService = 1;
            //if (dx.Rows.Count > 0)
            //{
            //    string contract = dx.Rows[0]["ServiceID"].ObjToString();
            //    contract = contract.Replace(loc + sYear, "");
            //    if (G1.validate_numeric(contract))
            //        maxService = contract.ObjToInt32() + 1;
            //}
            //string newContract = "";
            //if ( maxService > maxContract )
            //    newContract = atNeedCode + sYear + maxService.ToString("D03");
            //else
            //    newContract = atNeedCode + sYear + maxContract.ToString("D03");
            //txtFunService.Text = newContract;
            //txtFunService.Refresh();

            loc = "SX";
            //cmd = "Select * from `fcontracts` where `contractNumber` LIKE '%" + loc + sYear + "%' ORDER by `contractNumber` DESC LIMIT 10;";
            //dt = G1.get_db_data(cmd);

            //maxContract = 1;
            //contract = "";
            //if (dt.Rows.Count > 0)
            //{
            //    contract = dt.Rows[0]["contractNumber"].ObjToString();
            //    contract = contract.Replace(loc + sYear, "");
            //    if (G1.validate_numeric(contract))
            //        maxContract = contract.ObjToInt32() + 1;
            //}
            //if ( maxContract >= 1000 )
            //    newContract = loc + sYear + maxContract.ToString("D04");
            //else
            //    newContract = loc + sYear + maxContract.ToString("D03");

            string trust = "";
            string location = "";
            string middle = Trust85.decodeContractNumber(serviceId, ref trust, ref location);

            newContract = findNextFuneral(sYear);

            txtFunContract.Text = newContract;
            txtFunContract.Refresh();
        }
        /***********************************************************************************************/
        private string findNextFuneral ( string sYear )
        {
            string loc = "SX";
            string cmd = "Select `contractNumber` from `fcontracts` where `contractNumber` LIKE '%" + loc + sYear + "%' ORDER by `record`;";
            DataTable dt = G1.get_db_data(cmd);
            int maxContract = 0;
            string newContract = "";
            int num = 0;
            string contract = "";

            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    contract = dt.Rows[i]["contractNumber"].ObjToString();
                    contract = contract.Replace("SX" + sYear, "");
                    num = Convert.ToInt32(contract);
                    if (num > maxContract)
                        maxContract = num;
                }
            }
            catch ( Exception ex)
            {
            }

            maxContract += 1;
            if (maxContract >= 1000)
                newContract = loc + sYear + maxContract.ToString("D04");
            else
                newContract = loc + sYear + maxContract.ToString("D03");
            return newContract;
        }
        /***********************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        { // Determine Main Contract
            DateTime date = DateTime.Now;
            int year = date.Year % 100;
            string sYear = year.ToString("D02");

            string loc = cmbLoc.Text.Trim();
            if (String.IsNullOrWhiteSpace(loc))
                loc = "SX";

            string cmd = "Select * from `contracts` where `contractNumber` LIKE '%" + loc + sYear + "%' ORDER by `contractNumber` DESC LIMIT 10;";
            DataTable dt = G1.get_db_data(cmd);

            string trust = "";
            int maxContract = 1;
            if (dt.Rows.Count > 0)
            {
                string contract = dt.Rows[0]["contractNumber"].ObjToString();
                string location = "";
                contract = Trust85.decodeContractNumber(contract, ref trust, ref location);
                if (G1.validate_numeric(contract))
                {
                    maxContract = contract.ObjToInt32() + 1;
                    maxContract = maxContract % year;
                }
            }
            trust = cmbTrust.Text.Trim();
            string newContract = loc + sYear + maxContract.ToString("D03") + trust;
            contractNumber.Text = newContract;
            contractNumber.Refresh();
        }
        /***********************************************************************************************/
        private void pictureBox2_Click(object sender, EventArgs e)
        { // Determine Main ServiceId
            DateTime date = DateTime.Now;
            int year = date.Year % 100;
            string sYear = year.ToString("D02");

            string loc = cmbLocFuneral.Text.Trim();
            if (String.IsNullOrWhiteSpace(loc))
                loc = "SX";

            string cmd = "Select * from `contracts` where `ServiceId` LIKE '%" + loc + sYear + "%' ORDER by `ServiceId` DESC LIMIT 10;";
            DataTable dt = G1.get_db_data(cmd);

            int maxContract = 1;
            if (dt.Rows.Count > 0)
            {
                string contract = dt.Rows[0]["ServiceId"].ObjToString();
                contract = contract.Replace(loc + sYear, "");
                if (G1.validate_numeric(contract))
                {
                    maxContract = contract.ObjToInt32() + 1;
                    maxContract = maxContract % year;
                }
            }

            cmd = "Select * from `inventory` where `ServiceID` LIKE '%" + loc + sYear + "%' ORDER BY `ServiceID` DESC LIMIT 10;";
            DataTable dx = G1.get_db_data(cmd);

            int maxService = 1;
            if (dx.Rows.Count > 0)
            {
                string contract = dx.Rows[0]["ServiceID"].ObjToString();
                contract = contract.Replace(loc + sYear, "");
                if (G1.validate_numeric(contract))
                {
                    maxService = contract.ObjToInt32() + 1;
                    maxService = maxContract % year;
                }
            }
            string newContract = "";
            if (maxService > maxContract)
                newContract = loc + sYear + maxService.ToString("D03");
            else
                newContract = loc + sYear + maxContract.ToString("D03");
            txtServiceID.Text = newContract;
            txtServiceID.Refresh();
        }
        /***********************************************************************************************/
        private void pictureBox4_Click(object sender, EventArgs e)
        {
            DateTime date = DateTime.Now;
            int year = date.Year % 100;
            string sYear = year.ToString("D02");

            string loc = cmbLocCemetery.Text.Trim();
            if (String.IsNullOrWhiteSpace(loc))
            {
                MessageBox.Show("***ERROR*** You must select the Cemetery Location!");
                return;
            }

            string cmd = "Select * from `contracts` where `contractNumber` LIKE '%" + loc + sYear + "%' ORDER by `contractNumber` DESC LIMIT 10;";
            DataTable dt = G1.get_db_data(cmd);

            string trust = "";
            int maxContract = 1;
            if (dt.Rows.Count > 0)
            {
                string contract = dt.Rows[0]["contractNumber"].ObjToString();
                string location = "";
                contract = Trust85.decodeContractNumber(contract, ref trust, ref location);
                if (G1.validate_numeric(contract))
                {
                    maxContract = contract.ObjToInt32() + 1;
                    maxContract = maxContract % year;
                }
            }

            string newContract = loc + sYear + maxContract.ToString("D03");
            txtCemeteryContract.Text = newContract;
            txtCemeteryContract.Refresh();
        }
        /***********************************************************************************************/
        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        /***********************************************************************************************/
        private void button2_Click(object sender, EventArgs e)
        {
            string contractRecord = "";
            string contract = "";
            if (!ValidateCemeteryData(ref contract))
            {
                MessageBox.Show("***ERROR*** You must enter valid Data, Location, Numeric Contract#, First Name, and Last Name!");
                return;
            }
            string cmd = "Select * from `contracts` where `contractNumber` = '" + contract + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                MessageBox.Show("***ERROR*** Contract " + contract + " already exists!");
                return;
            }

            string record = G1.create_record("contracts", "notes", "-1");
            if (G1.BadRecord("contracts", record))
                return;
            contractRecord = record;
            G1.update_db_table("contracts", "record", record, new string[] { "contractNumber", contract});

            cmd = "Select * from `customers` where `contractNumber` = '" + contract + "';";
            dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                MessageBox.Show("***ERROR*** Customer Contract " + contract + " already exists!");
                return;
            }

            record = G1.create_record("customers", "firstName", "-1");
            if (G1.BadRecord("customers", record))
                return;

            string user = LoginForm.username;
            string location = cmbLocCemetery.Text.Trim().ToUpper();
            string firstName = this.txtCemeteryFirstName.Text.ToUpper();
            string lastName = this.txtCemeteryLastName.Text.ToUpper();
            string middleName = this.txtCemeteryMiddleName.Text.ToUpper();
            DateTime issueDate = txtIssueDate.Text.ObjToDateTime();
            string depositNumber = txtDepositNumber.Text.Trim();
            string bankAccount = txtBankAccount.Text.Trim();

            double payment = txtPayment.Text.ObjToDouble();
            double trust85P = payment;
            double trust100P = payment;
            if ( chkFullPayment.Checked )
            {
                trust85P = payment * 0.15D;
                trust85P = G1.RoundValue(trust85P);
                trust100P = trust85P;
            }
            G1.update_db_table("customers", "record", record, new string[] { "contractNumber", contract, "firstName", firstName, "lastName", lastName, "middleName", middleName });

            if (issueDate.Year > 100)
                G1.update_db_table("contracts", "record", contractRecord, new string[] { "issueDate8", issueDate.ToString("MM/dd/yyyy")});
            if (payment > 0D)
            {
                //G1.update_db_table("contracts", "record", contractRecord, new string[] { "downPayment", payment.ToString() });
                G1.update_db_table("contracts", "record", contractRecord, new string[] { "merchandiseTotal", payment.ToString() }); // I think this is better
            }

            record = G1.create_record("payments", "lastName", "-1");
            if (G1.BadRecord("payments", record))
                return;

            if ( String.IsNullOrWhiteSpace ( depositNumber ))
                depositNumber = "I" + issueDate.Year.ToString("D4") + issueDate.Month.ToString("D2") + issueDate.Day.ToString("D2");

            G1.update_db_table("payments", "record", record, new string[] { "contractNumber", contract, "lastName", lastName, "firstName", firstName, "paymentAmount", payment.ToString() });
            G1.update_db_table("payments", "record", record, new string[] { "dueDate8", "12/31/2039", "payDate8", issueDate.ToString("MM/dd/yyyy"), "trust85P", trust85P.ToString(), "trust100P", trust100P.ToString(), "location", location, "userId", user, "depositNumber", depositNumber, "edited", "Cemetery", "bank_account", bankAccount });

            G1.AddToAudit(LoginForm.username, "Customers", "New Cemetery Customer", "Added", contract);

            OnSelectDone(contract);
            this.Close();
        }
        /***********************************************************************************************/
        private void txtIssueDate_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                txtIssueDate_Leave(sender, e);
            else if (e.KeyCode == Keys.Tab)
                txtIssueDate_Leave(sender, e);

        }
        /***********************************************************************************************/
        private void txtIssueDate_Enter(object sender, EventArgs e)
        {
            string date = txtIssueDate.Text;
            if (G1.validate_date(date))
            {
                DateTime ddate = date.ObjToDateTime();
                txtIssueDate.Text = ddate.ToString("MM/dd/yyyy");
            }
            else
                txtIssueDate.Text = "";

        }
        /***********************************************************************************************/
        private void txtIssueDate_Leave(object sender, EventArgs e)
        {
            string date = txtIssueDate.Text;
            if (String.IsNullOrWhiteSpace(date))
                return;
            if (G1.validate_date(date))
            {
                DateTime ddate = txtIssueDate.Text.ObjToDateTime();
                if (ddate.Year < 1800)
                {
                    MessageBox.Show("***ERROR*** Date Entered Year is less than 1800!");
                    return;
                }
                if (ddate.Year > 100)
                {
                    txtIssueDate.Text = ddate.ToString("MM/dd/yyyy");
                }
            }
            else
            {
                MessageBox.Show("***ERROR*** Invalid Date!");
            }
        }
        /***********************************************************************************************/
        private void txtPayment_Leave(object sender, EventArgs e)
        {
            string pay = txtPayment.Text;
            if (String.IsNullOrWhiteSpace(pay))
                return;
            if ( !G1.validate_numeric ( pay ))
            {
                MessageBox.Show("***ERROR*** Invalid Payment!");
                return;
            }
            double payment = pay.ObjToDouble();
            pay = G1.ReformatMoney(payment);
            txtPayment.Text = pay;
        }
        /***********************************************************************************************/
        private void txtPayment_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                txtPayment_Leave(sender, e);
        }
        /****************************************************************************************/
        private void pictureBox3_Click(object sender, EventArgs e)
        { // Find Trust or Insurance as pending payment
            FastLookup fastForm = new FastLookup("", true );
            fastForm.ListDone += FastForm_ListDone;
            fastForm.Show();
        }
        /****************************************************************************************/
        private void FastForm_ListDone(string s)
        { // Trust or Policy Selected
            if (String.IsNullOrWhiteSpace(s))
                return;
            string[] Lines = s.Split(':');
            if (Lines.Length <= 1)
                return;
            string source = Lines[0].Trim();
            string amount = "";
            if (Lines.Length >= 4)
                amount = Lines[3].Trim();
            string account = Lines[1].Trim();
            string name = "";
            if (Lines.Length >= 5)
                name = Lines[4].Trim();
            txtFunContract.Text = account;
            Lines = name.Split(',');
            if (Lines.Length > 0)
            {
                txtFunLastName.Text = Lines[0].Trim();
                if (Lines.Length > 1)
                    txtFunFirstName.Text = Lines[1].Trim();
            }
            int idx = s.IndexOf(": Contract=");
            if (idx > 0)
            {
                string str = s.Substring(idx + 11);
                account = str;
                if (DailyHistory.isInsurance(account))
                {
                    MessageBox.Show("***ERROR*** Insurance Cannot Be Selected Here !", "Bad Selection Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    txtFunContract.Text = "";
                    txtFunLastName.Text = "";
                    txtFunFirstName.Text = "";
                    return;
                }
                txtFunContract.Text = account;
                string cmd = "Select * from `contracts` where `contractNumber` = '" + account + "';";
                //if (DailyHistory.isInsurance(account))
                //    cmd = "Select * from `icontracts` where `contractNumber` = '" + account + "';";
                DataTable dx = G1.get_db_data(cmd);
                if ( dx.Rows.Count > 0 )
                {
                    DateTime deceasedDate = dx.Rows[0]["deceasedDate"].ObjToDateTime();
                    if (deceasedDate.Year > 1800)
                        txtDOD.Text = deceasedDate.ToString("MM/dd/yyyy");
                }
            }
        }
        /***********************************************************************************************/
        private void txtDOD_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                txtDOD_Leave(sender, e);
            else if (e.KeyCode == Keys.Tab)
                txtDOD_Leave(sender, e);
        }
        /***********************************************************************************************/
        private void txtDOD_Leave(object sender, EventArgs e)
        {
            string date = txtDOD.Text;
            if (String.IsNullOrWhiteSpace(date))
                return;
            if (G1.validate_date(date))
            {
                DateTime ddate = txtDOD.Text.ObjToDateTime();
                if (ddate.Year <= 1800)
                {
                    MessageBox.Show("***ERROR*** Date Entered Year is less than 1800!");
                    return;
                }
                if (ddate.Year > 1800)
                    txtDOD.Text = ddate.ToString("MM/dd/yyyy");
            }
            else
            {
                MessageBox.Show("***ERROR*** Invalid Date!");
            }
        }
        /***********************************************************************************************/
        private void txtFunService_KeyDown(object sender, KeyEventArgs e)
        {
            if ( e.KeyCode == Keys.Enter )
            {
                string serviceId = txtFunService.Text.Trim();
                if ( CheckServiceIdExists ( serviceId ))
                    MessageBox.Show("***ERROR*** A Service ID of " + serviceId + " Already Exists Somewhere!", "Service ID EXISTS Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        /***********************************************************************************************/
        private void txtBankAccount_Enter(object sender, EventArgs e)
        {
            using (SelectBank bankForm = new SelectBank())
            {
                bankForm.TopMost = true;
                bankForm.ShowDialog();
                if (bankForm.DialogResult != System.Windows.Forms.DialogResult.OK)
                    return;
                string bankRecord = bankForm.Answer;
                if (String.IsNullOrWhiteSpace(bankRecord))
                    return;
                string cmd = "Select * from `bank_accounts` where `record` = '" + bankRecord + "';";
                DataTable dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    string accountTitle = dx.Rows[0]["account_title"].ObjToString();
                    string location = dx.Rows[0]["location"].ObjToString();
                    string bank_gl = dx.Rows[0]["general_ledger_no"].ObjToString();
                    string bankAccount = dx.Rows[0]["account_no"].ObjToString();
                    string data = location + "~" + bank_gl + "~" + bankAccount;
                    txtBankAccount.Text = data;
                }
            }
        }
        /***********************************************************************************************/
        private void txtBankAccount_DoubleClick(object sender, EventArgs e)
        {
            txtBankAccount_Enter(null, null);
        }
        /***********************************************************************************************/
    }
}