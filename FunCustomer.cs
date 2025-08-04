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
using Tracking;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraEditors.Repository;
using System.Text.RegularExpressions;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using System.Globalization;

using DevExpress.XtraEditors.Popup;
using DevExpress.Utils.Win;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class FunCustomer : DevExpress.XtraEditors.XtraForm
    {
        private string workContract = "";
        private string workRecord = "";
        private string customersFile = "customers";
        private string contractsFile = "contracts";
        private string custExtendedFile = "cust_extended";
        private bool insurance = false;
        private string workPayer = "";
        private bool funModified = false;
        private bool customerModified = false;
        private bool loading = true;
        private bool isProtected = false;
        private DataTable directorsDt = null;
        private DataTable arrangersDt = null;
        private bool workFuneral = false;
        private string unfilteredSSN = "";
        private string filteredSSN = "";
        private string mainSSN = "";
        private string firstCallInformation = "";
        private bool firstCallFamily = false;
        private string hospiceInformation = "";
        private bool hospiceFamily = false;
        private DataTable workDt6 = null;
        private bool fromPreneed = true;
        private string myGender = "";
        private bool avoidSSN = false;
        private string originalServiceId = "";
        /****************************************************************************************/
        public FunCustomer(string contract, bool funeral = false)
        {
            InitializeComponent();
            workContract = contract;
            workFuneral = funeral;
        }
        /****************************************************************************************/
        public FunCustomer(string contract, string junk = "", bool preneed = true)
        {
            InitializeComponent();
            workContract = contract;
            fromPreneed = preneed;
        }
        /****************************************************************************************/
        private string oldServiceId = "";
        private void FunCustomer_Load(object sender, EventArgs e)
        {
            funModified = false;
            LoadCustomer();
            oldServiceId = txtServiceId.Text.Trim();
            loading = false;
            if (workFuneral)
            {
                customersFile = "fcustomers";
                contractsFile = "fcontracts";
                custExtendedFile = "fcust_extended";
            }
            //funDemo = new FuneralDemo("Place", "", "", "", "", "", "", "", "", "", "", "");
            //funDemo.FunDemoDone += FunDemo_FunDemoDone;
            //Rectangle rect = funDemo.Bounds;
            //int top = rect.Y;
            //int left = rect.X;
            //int height = rect.Height;
            //int width = rect.Width;
            //top = this.Bounds.Y;
            //left = this.Bounds.Width - width;
            //funDemo.StartPosition = FormStartPosition.Manual;
            //funDemo.SetBounds(left, top, width, height);

            //funDemo.Show();
            //funDemo.Hide();
        }
        /***********************************************************************************************/
        private void LoadCustomer()
        {
            btnSaveAll.Hide();
            workPayer = "";
            if (String.IsNullOrWhiteSpace(workContract))
                return;
            if (DailyHistory.isInsurance(workContract))
            {
                customersFile = "icustomers";
                contractsFile = "icontracts";
                insurance = true;
            }
            if (workFuneral)
            {
                customersFile = "fcustomers";
                contractsFile = "fcontracts";
                custExtendedFile = "fcust_extended";
            }
            string cmd = "Select * from `" + customersFile + "` where `contractNumber` = '" + workContract + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
            {
                cmd = "Select * from `fcustomers` where `contractNumber` = '" + workContract + "';";
                if (customersFile == "fcustomers")
                    cmd = "Select * from `customers` where `contractNumber` = '" + workContract + "';";
                dt = G1.get_db_data(cmd);
                if (dt.Rows.Count <= 0)
                {
                    cmd = "Select * from `fcustomers` where `payer` = '" + workContract + "';";
                    dt = G1.get_db_data(cmd);
                    if (dt.Rows.Count > 0)
                    {
                        workContract = dt.Rows[0]["contractNumber"].ObjToString();
                        customersFile = "icustomers";
                        contractsFile = "icontracts";
                        insurance = true;
                    }
                }
            }
            if (dt.Rows.Count <= 0)
            {
                MessageBox.Show("***ERROR*** Reading Customer Contract " + workContract.ToString() + "!", "Reading Customer Problem Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                //this.Close();
                return;
            }
            if (insurance)
                workPayer = dt.Rows[0]["payer"].ObjToString();

            cmd = "Select * from `" + contractsFile + "` where `contractNumber` = '" + workContract + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
            {
                cmd = "Select * from `fcontracts` where `contractNumber` = '" + workContract + "';";
                if (contractsFile == "fcontracts")
                    cmd = "Select * from `contracts` where `contractNumber` = '" + workContract + "';";
                dx = G1.get_db_data(cmd);
            }
            else
            {
                if (insurance)
                {
                    cmd = "Select * from `payers` WHERE `payer` = '" + workPayer + "';";
                    DataTable payDt = G1.get_db_data(cmd);
                    if (payDt.Rows.Count > 0)
                    {
                        workContract = payDt.Rows[0]["contractNumber"].ObjToString();
                        cmd = "Select * from `" + contractsFile + "` where `contractNumber` = '" + workContract + "';";
                        dx = G1.get_db_data(cmd);
                    }
                }
            }
            if (dx.Rows.Count <= 0)
            {
                MessageBox.Show("***ERROR*** Reading Customer Contract " + workContract.ToString() + "!", "Reading Customer Problem Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                this.Close();
            }
            loading = true;

            workRecord = dt.Rows[0]["record"].ObjToString();

            dt.Columns.Add("bDate");
            dt.Columns.Add("ssno");
            dt.Columns.Add("agreement");
            dt.Columns.Add("gender");
            dt.Rows[0]["gender"] = "1";

            //if (workPolicy)
            //{
            //    txtFirstName.Text = policyDt.Rows[0]["policyFirstName"].ObjToString();
            //    txtLastName.Text = policyDt.Rows[0]["policyLastName"].ObjToString();
            //}
            //else
            //{
            txtFirstName.Text = dt.Rows[0]["firstName"].ObjToString();
            txtFirstName.Text = FixUpperLowerNames(txtFirstName.Text);

            txtLastName.Text = dt.Rows[0]["lastName"].ObjToString();
            txtLastName.Text = FixUpperLowerNames(txtLastName.Text);

            txtMiddleName.Text = dt.Rows[0]["middleName"].ObjToString();
            txtMiddleName.Text = FixUpperLowerNames(txtMiddleName.Text);

            txtPrefix.Text = dt.Rows[0]["prefix"].ObjToString();
            txtSuffix.Text = dt.Rows[0]["suffix"].ObjToString();
            txtFullLegalName.Text = dt.Rows[0]["legalName"].ObjToString();
            txtPreferedName.Text = dt.Rows[0]["preferredName"].ObjToString();
            txtMaidenName.Text = dt.Rows[0]["maidenName"].ObjToString();
            txtEmail.Text = dt.Rows[0]["emailAddress"].ObjToString();
            cmbDelivery.Text = dt.Rows[0]["delivery"].ObjToString();
            txtFirstPayDate.Text = dt.Rows[0]["firstPayDate"].ObjToDateTime().ToString("MM/dd/yyyy");
            //            }
            CustomerDetails.FormatSSN(dt, "ssn", "ssno");
            CustomerDetails.FixDates(dt, "birthDate", "bDate");

            //string agentCode = dt.Rows[0]["agentCode"].ObjToString();
            //string name = GetAgentName(agentCode);

            //txtAgentCode.Text = agentCode;
            //txtAgentName.Text = name;


            string bdate = dt.Rows[0]["bDate"].ObjToString();
            //if (workPolicy)
            //{
            //    DateTime bbDate = policyDt.Rows[0]["birthDate"].ObjToDateTime();
            //    if (bbDate.Year > 1850)
            //        bdate = bbDate.ToString("MM/dd/yyyy");
            //}
            string age = G1.CalcAge(bdate);
            txtAge.Text = age;

            unfilteredSSN = dt.Rows[0]["ssn"].ObjToString();
            unfilteredSSN = FixSSN(unfilteredSSN);
            mainSSN = unfilteredSSN.Replace("-", "");

            avoidSSN = false;
            if (mainSSN.Length == 9)
            {
                if (workContract.IndexOf("SX") != 0)
                {
                    avoidSSN = true;
                    txtSSN.Enabled = false;
                }
            }

            if (G1.isAdmin() || G1.isHR())
                txtSSN.Enabled = true;

            txtSSN.Text = dt.Rows[0]["ssno"].ObjToString();
            filteredSSN = txtSSN.Text;

            mainSSN = unfilteredSSN;

            //if (DailyHistory.isInsurance(workContract))
            //    lblPayer.Text = "Payer: " + dt.Rows[0]["payer"].ObjToString();
            //else
            //    lblPayer.Text = "";

            DateTime ddate = dt.Rows[0]["birthDate"].ObjToDateTime();
            //if (workPolicy)
            //    ddate = policyDt.Rows[0]["birthDate"].ObjToDateTime();
            if (ddate.Year > 1875)
            {
                dateDOB.Text = ddate.ToString("MM/dd/yyyy");
                txtBday.Text = ddate.ToString("MM/dd/yyyy");
            }

            ddate = dt.Rows[0]["deceasedDate"].ObjToDateTime();
            if (ddate.Year < 1875)
                ddate = dx.Rows[0]["deceasedDate"].ObjToDateTime();
            dateDeceased.Text = ddate.ToString("MM/dd/yyyy");
            if (ddate.Year > 1800)
                txtDOD.Text = ddate.ToString("MM/dd/yyyy");

            groupBoxMailing.Show();
            lblEmail.Show();
            txtEmail.Show();
            lblDelivery.Show();
            cmbDelivery.Show();

            if (ddate.Year > 100)
            {
                //groupBoxMailing.Hide();
                //lblEmail.Hide();
                //txtEmail.Hide();
                //lblDelivery.Hide();
                //cmbDelivery.Hide();
                string serviceId = dx.Rows[0]["ServiceId"].ObjToString();
                originalServiceId = serviceId;
                if (!String.IsNullOrWhiteSpace(serviceId))
                {
                    txtServiceId.Text = serviceId;
                    if ( serviceId.Length >= 2 )
                    {
                        string merchCode = serviceId.Substring(0, 2);
                        cmd = "Select * from `funeralhomes` where `merchandiseCode` = '" + merchCode + "';";
                        DataTable ddx = G1.get_db_data(cmd);
                        if ( ddx.Rows.Count > 0 )
                        {
                            this.txtSSN.Enabled = false;
                            labelControl14.Text = "Date of Sale";
                        }
                    }
                }
                txtServiceId.Enabled = true;

                //string serviceDate = dx.Rows[0]["ServiceDate"].ObjToString();
                //if (!String.IsNullOrWhiteSpace(serviceDate))
                //    txtServiceDate.Text = serviceDate;
                txtServiceDate.Enabled = true;
                txtArrangementDate.Enabled = true;
                txtArrangementTime.Enabled = true;
            }

            else
            {
                txtServiceId.Enabled = false;
                txtServiceDate.Enabled = false;
                txtArrangementDate.Enabled = false;
                txtArrangementTime.Enabled = false;
            }
            if (workFuneral)
            {
                txtFirstPayDate.Hide();
                txtFirstPayDate.Refresh();
                //lblFirstPayDate.Hide();
                this.lblFirstPayDate.Visible = false;
                this.lblFirstPayDate.Refresh();
            }

            string sex = dt.Rows[0]["sex"].ObjToString();

            string gender = CustomerDetails.ValidateGender(dt.Rows[0]["sex"].ObjToString());
            if (gender.ToUpper() == "MALE")
            {
                radioMale.Checked = true;
                myGender = "Male";
            }
            else
            {
                radioFemale.Checked = true;
                myGender = "Female";
            }
            string maritalStatus = dt.Rows[0]["maritalstatus"].ObjToString();
            string race = dt.Rows[0]["race"].ObjToString();
            string ethnicity = dt.Rows[0]["ethnicity"].ObjToString();
            string language = dt.Rows[0]["language"].ObjToString();

            textEdit_patientAddressLine1.Text = dt.Rows[0]["address1"].ObjToString();
            textEdit_patientAddressLine2.Text = dt.Rows[0]["address2"].ObjToString();
            textEdit_patientCity.Text = dt.Rows[0]["city"].ObjToString();
            textEdit_patientZipCode.Text = dt.Rows[0]["zip1"].ObjToString();

            textEdit2.Text = dt.Rows[0]["mailAddress1"].ObjToString();
            textEdit3.Text = dt.Rows[0]["mailAddress2"].ObjToString();
            textEdit4.Text = dt.Rows[0]["mailCity"].ObjToString();
            textEdit1.Text = dt.Rows[0]["mailZip1"].ObjToString();


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

            if (phone1.ToUpper() != "CELL" && phone1.ToUpper() != "HOME" && phone1 != "WORK")
                phone1 = "";

            cmbPhoneQualifier1.Text = phone1;

            string phone2 = dt.Rows[0]["phoneType2"].ObjToString();
            if (phone2.ToUpper() != "CELL" && phone2.ToUpper() != "HOME" && phone2 != "WORK")
                phone2 = "";
            cmbPhoneQualifier2.Text = phone2;

            txtPhone1.Text = dt.Rows[0]["phoneNumber1"].ObjToString();
            txtPhone2.Text = dt.Rows[0]["phoneNumber2"].ObjToString();

            string state = dt.Rows[0]["state"].ObjToString();
            CustomerDetails.SetupComboTable(this.comboStates, "ref_states", "abbrev", state);

            state = dt.Rows[0]["mailState"].ObjToString();
            CustomerDetails.SetupComboTable(this.comboBox1, "ref_states", "abbrev", state);

            LoadCustomerPicture();
            LoadVitals();
            CalculateAge();
            dateDeceased.Hide();
            dateDOB.Hide();
            funModified = false;
            btnSaveAll.Hide();

            DetermineView();

            //textBox1.Hide();

            panelAll.Refresh();
            this.Refresh();
        }
        /***************************************************************************************/
        public static string FixUpperLowerNames(string name)
        {
            int cLower = 0;
            int cUpper = 0;

            char c;

            bool allUpper = name.All(char.IsUpper);     //returns true
            bool allLower = name.All(char.IsLower);     //returns false

            for (int i = 0; i < name.Length; i++)
            {
                c = (char)name[i];
                if (char.IsLower(c))
                    cLower++;
                else if (char.IsUpper(c))
                    cUpper++;
            }

            string rtnName = "";

            if (allUpper)
                rtnName = G1.force_lower_line(name);
            else if (allLower)
                rtnName = G1.force_lower_line(name);
            else
                rtnName = name;

            return rtnName;
        }
        /***************************************************************************************/
        private void DetermineView()
        {
            DateTime ddate = txtDOD.Text.ObjToDateTime();

            if (ddate.Year > 100)
            {
                panelVitals.Show();
            }
            else
            {
                panelVitals.Hide();
            }
        }
        /***************************************************************************************/
        private void LoadVitals()
        {
            InitializeVitalsPanel();
        }
        /***********************************************************************************************/
        private bool otherModified = false;
        private void InitializeVitalsPanel()
        {
            dgv6.Visible = false;
            otherModified = false;

            string arrangementDate = "";
            string arrangementTime = "";
            string serviceDate = LoadOtherData(ref arrangementDate, ref arrangementTime);
            if (!String.IsNullOrWhiteSpace(serviceDate))
                txtServiceDate.Text = serviceDate;
            if (!String.IsNullOrWhiteSpace(arrangementDate))
            {
                txtArrangementDate.Text = arrangementDate;
                if (!String.IsNullOrWhiteSpace(arrangementTime))
                    txtArrangementTime.Text = arrangementTime;
            }

            string deceasedDOD = this.txtDOD.Text;
            if (String.IsNullOrWhiteSpace(deceasedDOD))
                return;
            DateTime deceasedDate = deceasedDOD.ObjToDateTime();

            if (deceasedDate.Year < 100)
                return;
            dgv6.Visible = true;
        }
        /****************************************************************************************/
        private DataTable trackingDt = null;
        private DataTable trackDt = null;
        RepositoryItemComboBox ciLookup = new RepositoryItemComboBox();
        RepositoryItemComboBox ciLookupSave = new RepositoryItemComboBox();
        /****************************************************************************************/
        private void ReloadTrack()
        {
            if (trackDt != null)
            {
                trackDt.Rows.Clear();
                trackDt.Dispose();
                trackDt = null;
            }
            trackDt = G1.get_db_data("Select * from `track`;");
        }
        /****************************************************************************************/
        private string FixUsingFieldData(string field)
        {
            string newField = field;
            string cmd = "Select * from `tracking` where `tracking` = '" + field + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                string useData = dx.Rows[0]["using"].ObjToString();
                if (!String.IsNullOrWhiteSpace(useData))
                    newField = useData;
            }
            return newField;
        }
        /****************************************************************************************/
        private string LoadOtherData(ref string arrangementDate, ref string arrangementTime)
        {
            string serviceDate = "";
            arrangementDate = "";
            arrangementTime = "";
            trackingDt = G1.get_db_data("Select * from `tracking`;");
            trackDt = G1.get_db_data("Select * from `track`;");
            ciLookup.SelectedIndexChanged += CiLookup_SelectedIndexChanged;
            ciLookup.KeyPress += CiLookup_KeyPress;
            ciLookup.Popup += CiLookup_Popup;

            ciLookupSave.Popup += CiLookup_Popup;
            ciLookupSave.SelectedIndexChanged += CiLookup_SelectedIndexChanged;
            ciLookupSave.KeyPress += CiLookup_KeyPress;

            string dbfield = "";
            string data = "";
            DataRow[] dR = null;
            string cmd = "Select * from `cust_extended_layout` WHERE `group` = 'Vital Statistics' ORDER BY `order`;";
            DataTable dx = G1.get_db_data(cmd);
            dx.Columns.Add("num");
            dx.Columns.Add("mod");
            dx.Columns.Add("data");
            dx.Columns.Add("add");
            dx.Columns.Add("edit");
            dx.Columns.Add("tracking");
            dx.Columns.Add("dropOnly");
            dx.Columns.Add("addContact");
            cmd = "Select * from `" + custExtendedFile + "` where `contractNumber` = '" + workContract + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                DateTime sDate = dt.Rows[0]["serviceDate"].ObjToDateTime();
                if (sDate.Year > 100)
                    serviceDate = sDate.ToString("MM/dd/yyyy");

                sDate = dt.Rows[0]["arrangementDate"].ObjToDateTime();
                if (sDate.Year > 100)
                {
                    arrangementDate = sDate.ToString("MM/dd/yyyy");

                    sDate = dt.Rows[0]["arrangementTime"].ObjToDateTime();
                    arrangementTime = sDate.ToString("HH:mm");
                }

                string county = dt.Rows[0]["DECCOUNTY"].ObjToString();
                txtCounty.Text = county;
                string insideCity = dt.Rows[0]["IN CITY LIMITS"].ObjToString();
                cmbInsideCity.Text = insideCity;

                int groupNumber = 1;
                string oldGroup = "";
                string group = "";
                string help = "";
                string type = "";
                string dropOnly = "";
                string addContact = "";
                for (int i = 0; i < dx.Rows.Count; i++)
                {
                    group = dx.Rows[i]["group"].ObjToString();
                    if (String.IsNullOrWhiteSpace(oldGroup))
                        oldGroup = group;
                    if (String.IsNullOrWhiteSpace(group))
                        group = oldGroup;
                    if (group != oldGroup)
                        groupNumber++;
                    oldGroup = group;
                    group = groupNumber.ToString() + ". " + group;
                    dx.Rows[i]["group"] = group;
                    dbfield = dx.Rows[i]["dbfield"].ObjToString();
                    if (dbfield.Trim().ToUpper() == "FIRSTCALLDETAIL")
                    {
                        firstCallFamily = false;
                        firstCallInformation = dt.Rows[0]["FirstCallDetail"].ObjToString();
                        if (dt.Rows[0]["FirstCallFamilyPresent"].ObjToString() == "YES")
                            firstCallFamily = true;
                        continue;
                    }
                    if (dbfield.Trim().ToUpper() == "HOSPICEDETAIL")
                    {
                        //hospiceFamily = false;
                        //if (G1.get_column_number(dt, "HospiceFamilyPresent") >= 0)
                        //{
                        //    hospiceInformation = dt.Rows[0]["HospiceDetail"].ObjToString();
                        //    if (dt.Rows[0]["HospiceFamilyPresent"].ObjToString() == "YES")
                        //        hospiceFamily = true;
                        //}
                        continue;
                    }
                    if (!String.IsNullOrWhiteSpace(dbfield))
                    {
                        if (G1.get_column_number(dt, dbfield) >= 0)
                        {
                            data = dt.Rows[0][dbfield].ObjToString();
                            dx.Rows[i]["data"] = data;
                        }
                    }
                    dR = trackingDt.Select("tracking='" + dbfield + "'");
                    if (dR.Length > 0)
                    {
                        dropOnly = dR[0]["dropOnly"].ObjToString();
                        addContact = dR[0]["addContact"].ObjToString();
                        if (dropOnly != "1")
                        {
                            dx.Rows[i]["help"] = "Tracking";
                            dx.Rows[i]["tracking"] = "T";
                            dx.Rows[i]["dropOnly"] = dR[0]["dropOnly"].ObjToString();
                            dx.Rows[i]["addContact"] = dR[0]["addContact"].ObjToString();
                        }
                        else
                        {
                            if (dropOnly == "1")
                                dx.Rows[i]["dropOnly"] = dropOnly;
                            if (addContact == "1")
                                dx.Rows[i]["addContact"] = addContact;
                        }
                    }
                    else
                    {
                        help = dx.Rows[i]["help"].ObjToString();
                        type = dx.Rows[i]["type"].ObjToString();
                        if (type.ToUpper() == "DATE" && String.IsNullOrWhiteSpace(help))
                            dx.Rows[i]["help"] = "Select Date";
                        else if (type.ToUpper() == "FULLDATE" && String.IsNullOrWhiteSpace(help))
                            dx.Rows[i]["help"] = "Select Date";
                        else if (type.ToUpper() == "DAY" && String.IsNullOrWhiteSpace(help))
                            dx.Rows[i]["help"] = "Select the Day of the Week";
                    }
                }
            }

            bool mod = CheckFuneralHomeCustody(dt, dx);

            gridMain6.Columns["num"].Visible = false;
            G1.NumberDataTable(dx);
            dgv6.DataSource = dx;
            otherModified = false;
            gridMain6.ExpandAllGroups();
            SetupDirectors();
            SetupArrangers();
            if (mod)
            {
                otherModified = true;
                btnSaveAll.Show();
            }
            return serviceDate;
        }
        /***************************************************************************************/
        private bool CheckFuneralHomeCustody(DataTable dt, DataTable dx)
        {
            bool modified = false;
            if (dt.Rows.Count <= 0)
                return modified;
            string serviceId = dt.Rows[0]["serviceId"].ObjToString();
            if (String.IsNullOrWhiteSpace(serviceId))
                return modified;
            string contract = "";
            string trust = "";
            string loc = "";
            contract = Trust85.decodeContractNumber(serviceId, true, ref trust, ref loc);
            if (String.IsNullOrWhiteSpace(loc))
                return modified;
            string cmd = "Select * from `funeralhomes` where `atneedcode` = '" + loc + "';";
            DataTable dd = G1.get_db_data(cmd);
            if (dd.Rows.Count <= 0)
                return modified;
            DataRow[] dR = dx.Select("dbfield='FunHome 1st Cust'");
            if (dR.Length <= 0)
                return modified;
            string str = dR[0]["data"].ObjToString();
            if (String.IsNullOrWhiteSpace(str))
            {
                AddFunHomeCustody(dx, "FunHome 1st Cust", dd.Rows[0]["name"].ObjToString());
                AddFunHomeCustody(dx, "FunHome 1st Cust License No", dd.Rows[0]["licenseNo"].ObjToString());
                AddFunHomeCustody(dx, "FunHome 1st Cust Address", dd.Rows[0]["address"].ObjToString());
                AddFunHomeCustody(dx, "FunHome 1st Cust City", dd.Rows[0]["city"].ObjToString());
                AddFunHomeCustody(dx, "FunHome 1st Cust State", dd.Rows[0]["state"].ObjToString());
                AddFunHomeCustody(dx, "FunHome 1st Cust Zip", dd.Rows[0]["zip"].ObjToString());
            }
            return modified;
        }
        /***************************************************************************************/
        private void AddFunHomeCustody(DataTable dx, string dbField, string data)
        {
            DataRow[] dR = dx.Select("dbfield='" + dbField + "'");
            if (dR.Length <= 0)
                return;
            dR[0]["data"] = data;
            dR[0]["mod"] = "Y";
        }
        /***************************************************************************************/
        private void SetupDirectors()
        {
            string location = "";
            string cmd = "Select * from `directors`";
            DataTable dx = G1.get_db_data(cmd);
            string find = EditCust.activeFuneralHomeName;

            directorsDt = dx.Clone();

            cmd = "Select * from `funeralHomes` where `LocationCode` = '" + find + "';";
            DataTable ddt = G1.get_db_data(cmd);

            DataRow dRow = null;

            DataRow[] dr = dx.Select("location='" + find + "'");
            DataTable directDt = dx.Clone();
            if (dr.Length > 0)
                directDt = dr.CopyToDataTable();

            string assignedDirectors = "";

            if (ddt.Rows.Count > 0)
                assignedDirectors = ddt.Rows[0]["assignedDirectors"].ObjToString();

            string[] Lines = assignedDirectors.Split('~');
            string name = "";
            string lastName = "";
            string firstName = "";
            string middleName = "";
            string license = "";

            string[] oLines = null;
            DataRow[] ddr = null;

            for (int k = 0; k < Lines.Length; k++)
            {
                lastName = "";
                firstName = "";
                middleName = "";
                license = "";
                name = Lines[k].Trim();
                oLines = name.Split(',');
                if (oLines.Length > 0)
                {
                    lastName = oLines[0].Trim();
                    if (oLines.Length > 1)
                    {
                        firstName = oLines[1].Trim();
                        if (oLines.Length > 2)
                            license = oLines[2].Trim();
                    }
                    if (!String.IsNullOrWhiteSpace(license))
                        ddr = directDt.Select("license='" + license + "'");
                    else
                        ddr = directDt.Select("lastName='" + lastName + "' AND firstName='" + firstName + "'");
                    if (ddr.Length > 0)
                        directorsDt.ImportRow(ddr[0]);
                }
            }

            dr = dx.Select("location<>'" + find + "'");
            directDt = dx.Clone();
            if (dr.Length > 0)
                directDt = dr.CopyToDataTable();

            for (int k = 0; k < Lines.Length; k++)
            {
                lastName = "";
                firstName = "";
                middleName = "";
                license = "";
                name = Lines[k].Trim();
                oLines = name.Split(',');
                if (oLines.Length > 0)
                {
                    lastName = oLines[0].Trim();
                    if (oLines.Length > 1)
                    {
                        firstName = oLines[1].Trim();
                        if (oLines.Length > 2)
                            license = oLines[2].Trim();
                    }
                    if (!String.IsNullOrWhiteSpace(license))
                        ddr = directDt.Select("license='" + license + "'");
                    else
                        ddr = directDt.Select("lastName='" + lastName + "' AND firstName='" + firstName + "'");
                    if (ddr.Length > 0)
                        directorsDt.ImportRow(ddr[0]);
                }
            }
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                lastName = dx.Rows[i]["lastName"].ObjToString();
                firstName = dx.Rows[i]["firstName"].ObjToString();
                license = dx.Rows[i]["license"].ObjToString();
                if (!String.IsNullOrWhiteSpace(license))
                    ddr = directorsDt.Select("license='" + license + "'");
                else
                    ddr = directorsDt.Select("lastName='" + lastName + "' AND firstName='" + firstName + "'");
                if (ddr.Length <= 0)
                    directorsDt.ImportRow(dx.Rows[i]);
            }

            for ( int i=directorsDt.Rows.Count-1; i>=0; i--)
            {
                license = directorsDt.Rows[i]["license"].ObjToString();
                if (license.ToUpper().IndexOf("FD-") < 0 && license.ToUpper().IndexOf("FS-") < 0)
                    directorsDt.Rows.RemoveAt(i);
            }
        }
        /***************************************************************************************/
        private void SetupArrangers()
        {
            string location = "";
            string cmd = "Select * from `arrangers`";
            DataTable dx = G1.get_db_data(cmd);
            string find = EditCust.activeFuneralHomeName;

            arrangersDt = dx.Clone();

            cmd = "Select * from `funeralHomes` where `LocationCode` = '" + find + "';";
            DataTable ddt = G1.get_db_data(cmd);

            DataRow dRow = null;

            DataRow[] dr = dx.Select("location='" + find + "'");
            DataTable directDt = dx.Clone();
            if (dr.Length > 0)
                directDt = dr.CopyToDataTable();

            string assignedArrangers = "";

            if (ddt.Rows.Count > 0)
                assignedArrangers = ddt.Rows[0]["assignedArrangers"].ObjToString();

            string[] Lines = assignedArrangers.Split('~');
            string name = "";
            string lastName = "";
            string firstName = "";
            string middleName = "";
            string license = "";

            string[] oLines = null;
            DataRow[] ddr = null;

            for (int k = 0; k < Lines.Length; k++)
            {
                lastName = "";
                firstName = "";
                middleName = "";
                license = "";
                name = Lines[k].Trim();
                oLines = name.Split(',');
                if (oLines.Length > 0)
                {
                    lastName = oLines[0].Trim();
                    if (oLines.Length > 1)
                    {
                        firstName = oLines[1].Trim();
                        if (oLines.Length > 2)
                            license = oLines[2].Trim();
                    }
                    if (!String.IsNullOrWhiteSpace(license))
                        ddr = directDt.Select("license='" + license + "'");
                    else
                        ddr = directDt.Select("lastName='" + lastName + "' AND firstName='" + firstName + "'");
                    if (ddr.Length > 0)
                        arrangersDt.ImportRow(ddr[0]);
                }
            }

            dr = dx.Select("location<>'" + find + "'");
            directDt = dx.Clone();
            if (dr.Length > 0)
                directDt = dr.CopyToDataTable();

            for (int k = 0; k < Lines.Length; k++)
            {
                lastName = "";
                firstName = "";
                middleName = "";
                license = "";
                name = Lines[k].Trim();
                oLines = name.Split(',');
                if (oLines.Length > 0)
                {
                    lastName = oLines[0].Trim();
                    if (oLines.Length > 1)
                    {
                        firstName = oLines[1].Trim();
                        if (oLines.Length > 2)
                            license = oLines[2].Trim();
                    }
                    if (!String.IsNullOrWhiteSpace(license))
                        ddr = directDt.Select("license='" + license + "'");
                    else
                        ddr = directDt.Select("lastName='" + lastName + "' AND firstName='" + firstName + "'");
                    if (ddr.Length > 0)
                        arrangersDt.ImportRow(ddr[0]);
                }
            }
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                lastName = dx.Rows[i]["lastName"].ObjToString();
                firstName = dx.Rows[i]["firstName"].ObjToString();
                license = dx.Rows[i]["license"].ObjToString();
                if (!String.IsNullOrWhiteSpace(license))
                    ddr = arrangersDt.Select("license='" + license + "'");
                else
                    ddr = arrangersDt.Select("lastName='" + lastName + "' AND firstName='" + firstName + "'");
                if (ddr.Length <= 0)
                    arrangersDt.ImportRow(dx.Rows[i]);
            }
        }
        /***************************************************************************************/
        private string keyboard = "";
        private void CiLookup_KeyPress(object sender, KeyPressEventArgs e)
        {
            //if ( isProtected )
            //{
            //    e.Handled = true;
            //}
        }
        /***************************************************************************************/
        private void CiLookup_SelectedIndexChanged(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv6.DataSource;
            DataRow dr = gridMain6.GetFocusedDataRow();
            int rowhandle = gridMain6.FocusedRowHandle;
            int row = gridMain6.GetDataSourceRowIndex(rowhandle);
            string small = dt.Rows[row]["data"].ObjToString();

            string help = dt.Rows[row]["help"].ObjToString();
            string dbField = dt.Rows[row]["dbField"].ObjToString();

            ComboBoxEdit combo = (ComboBoxEdit)sender;
            string what = combo.Text.Trim().ToUpper();
            what = combo.Text.Trim();
            string text = textBox1.Text.Trim();
            //if ( !String.IsNullOrWhiteSpace ( what ) && String.IsNullOrWhiteSpace ( text ))
            //{
            //    textBox1.Text = text.Trim();
            //    return;
            //}
            dr["data"] = what;

            if (dbField.ToUpper() == "FUNERAL DIRECTOR")
            {
                string[] Lines = what.Split(' ');
                for (int i = 0; i < Lines.Length; i++)
                {
                    if (Lines[i].IndexOf("[") >= 0 && Lines[i].IndexOf("]") >= 0)
                    {
                        string license = Lines[i].Trim();
                        license = license.Replace("[", "");
                        license = license.Replace("]", "");
                        license = license.Trim();
                        DataRow[] dR = dt.Select("field='Funeral Director - License # '");
                        if (dR.Length > 0)
                        {
                            dR[0]["data"] = license;
                            dR[0]["mod"] = "Y";
                        }
                        break;
                    }
                }
            }

            funModified = true;
            btnSaveAll.Show();

            if (help.ToUpper() == "TRACKING")
            {
                DataRow[] dR = null;
                string cmd = "reference LIKE '" + dbField + "~%'";
                DataRow[] dRows = dt.Select(cmd);
                if (dRows.Length > 0)
                {
                    string[] Lines = null;
                    string field = "";
                    string answer = "";
                    for (int i = 0; i < dRows.Length; i++)
                    {
                        Lines = dRows[i]["reference"].ObjToString().Split('~');
                        if (Lines.Length <= 1)
                            continue;
                        field = Lines[1].Trim();
                        dR = trackDt.Select("answer='" + what + "' AND location='" + EditCust.activeFuneralHomeName + "'");
                        if (dR.Length > 0)
                        {
                            answer = dR[0][field].ObjToString();
                            dRows[i]["data"] = answer;
                            dRows[i]["mod"] = "Y";
                        }
                        else
                        {
                            dRows[i]["data"] = "";
                            dRows[i]["mod"] = "";
                        }
                    }
                }
                dt.AcceptChanges();
            }
        }
        /***************************************************************************************/
        private void CiLookup_SelectedIndexChangedAgain ( string what )
        {
            DataTable dt = (DataTable)dgv6.DataSource;
            DataRow dr = gridMain6.GetFocusedDataRow();
            int rowhandle = gridMain6.FocusedRowHandle;
            int row = gridMain6.GetDataSourceRowIndex(rowhandle);
            string small = dt.Rows[row]["data"].ObjToString();

            string help = dt.Rows[row]["help"].ObjToString();
            string dbField = dt.Rows[row]["dbField"].ObjToString();

            //ComboBoxEdit combo = (ComboBoxEdit)sender;
            //string what = combo.Text.Trim().ToUpper();
            //what = combo.Text.Trim();
            //dr["data"] = what;

            if (dbField.ToUpper() == "FUNERAL DIRECTOR")
            {
                string[] Lines = what.Split(' ');
                for (int i = 0; i < Lines.Length; i++)
                {
                    if (Lines[i].IndexOf("[") >= 0 && Lines[i].IndexOf("]") >= 0)
                    {
                        string license = Lines[i].Trim();
                        license = license.Replace("[", "");
                        license = license.Replace("]", "");
                        license = license.Trim();
                        DataRow[] dR = dt.Select("field='Funeral Director - License # '");
                        if (dR.Length > 0)
                        {
                            dR[0]["data"] = license;
                            dR[0]["mod"] = "Y";
                        }
                        break;
                    }
                }
            }

            funModified = true;
            btnSaveAll.Show();

            if (help.ToUpper() == "TRACKING")
            {
                DataRow[] dR = null;
                string cmd = "reference LIKE '" + dbField + "~%'";
                DataRow[] dRows = dt.Select(cmd);
                if (dRows.Length > 0)
                {
                    string[] Lines = null;
                    string field = "";
                    string answer = "";
                    for (int i = 0; i < dRows.Length; i++)
                    {
                        Lines = dRows[i]["reference"].ObjToString().Split('~');
                        if (Lines.Length <= 1)
                            continue;
                        field = Lines[1].Trim();
                        dR = trackDt.Select("answer='" + what + "' AND location='" + EditCust.activeFuneralHomeName + "'");
                        if (dR.Length > 0)
                        {
                            answer = dR[0][field].ObjToString();
                            dRows[i]["data"] = answer;
                            dRows[i]["mod"] = "Y";
                        }
                        else
                        {
                            dRows[i]["data"] = "";
                            dRows[i]["mod"] = "";
                        }
                    }
                }
                dt.AcceptChanges();
            }
        }
        /***************************************************************************************/
        private void CiLookup_DataChanged(string what)
        {
            DataTable dt = (DataTable)dgv6.DataSource;
            DataRow dr = gridMain6.GetFocusedDataRow();
            int rowhandle = gridMain6.FocusedRowHandle;
            int row = gridMain6.GetDataSourceRowIndex(rowhandle);

            string help = dt.Rows[row]["help"].ObjToString();
            string dbField = dt.Rows[row]["dbField"].ObjToString();

            //ComboBoxEdit combo = (ComboBoxEdit)sender;
            //string what = combo.Text.Trim().ToUpper();
            dr["data"] = what;

            if (dbField.ToUpper() == "FUNERAL DIRECTOR")
            {
                string[] Lines = what.Split(' ');
                for (int i = 0; i < Lines.Length; i++)
                {
                    if (Lines[i].IndexOf("[") >= 0 && Lines[i].IndexOf("]") >= 0)
                    {
                        string license = Lines[i].Trim();
                        license = license.Replace("[", "");
                        license = license.Replace("]", "");
                        license = license.Trim();
                        DataRow[] dR = dt.Select("field='Funeral Director - License # '");
                        if (dR.Length > 0)
                        {
                            dR[0]["data"] = license;
                            dR[0]["mod"] = "Y";
                        }
                        break;
                    }
                }
            }

            funModified = true;
            btnSaveAll.Show();

            if (help.ToUpper() == "TRACKING")
            {
                DataRow[] dR = null;
                string cmd = "reference LIKE '" + dbField + "~%'";
                DataRow[] dRows = dt.Select(cmd);
                if (dRows.Length > 0)
                {
                    string[] Lines = null;
                    string field = "";
                    string answer = "";
                    string reference = "";
                    for (int i = 0; i < dRows.Length; i++)
                    {
                        dR = trackDt.Select("tracking='" + dbField + "' AND answer='" + what + "' AND location='" + EditCust.activeFuneralHomeName + "'");
                        if (dR.Length > 0)
                        {
                            reference = dRows[i]["reference"].ObjToString();
                            answer = ProcessReference(dR, reference);
                            dRows[i]["data"] = answer;
                            dRows[i]["mod"] = "Y";
                        }
                    }
                }
                dt.AcceptChanges();
            }
        }
        /***************************************************************************************/
        public void FireEventServiceDateChanged()
        {
            string cmd = "Select * from `fcust_extended` where `contractNumber` = '" + workContract + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                DateTime date = dt.Rows[0]["serviceDate"].ObjToDateTime();
                txtServiceDate.Text = date.ToString("MM/dd/yyyy");
                txtServiceDate.Refresh();
            }
        }
        /***************************************************************************************/
        public bool FireEventFunServicesModified()
        {
            if (funModified || otherModified || customerModified)
                return true;
            return false;
        }
        /***************************************************************************************/
        public void FireEventFunServicesSetModified()
        {
            this.btnSaveAll.Show();
            this.btnSaveAll.Refresh();
        }
        /***************************************************************************************/
        public DevExpress.XtraGrid.GridControl FireEventPrintPreview()
        {
            return dgv6;
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
        /***************************************************************************************/
        public static bool ValidateServiceId ( string serviceId )
        {
            if (serviceId.ToUpper().IndexOf("O/S ") == 0)
                return true;

            if (serviceId.ToUpper().IndexOf("OS ") == 0)
                return true;

            string cmd = "Select * from `funeralhomes`;";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
                return false;
            string contract = "";
            string loc = "";
            string trust = "";
            contract = Trust85.decodeContractNumber(serviceId, ref trust, ref loc);

            DataRow[] dRows = dx.Select("atneedcode='" + loc + "'");
            if (dRows.Length <= 0)
                dRows = dx.Select("merchandisecode='" + loc + "'");
            if (dRows.Length <= 0)
                return false;
            return true;
        }
        /***************************************************************************************/
        public void FireEventSaveFunServices(bool save = false)
        {
            if ((save && funModified) || (save && otherModified))
            {
                string serviceId = txtServiceId.Text;
                if ( !String.IsNullOrWhiteSpace ( oldServiceId) && String.IsNullOrWhiteSpace ( serviceId))
                {
                    MessageBox.Show("***ERROR*** Old Service ID is " + oldServiceId + "!\nNew Service ID is BLANK!", "Service ID ERROR Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    return; // Exit if came in with Service Id and is now blank
                }
                if (NewContract.CheckServiceIdExists(serviceId, workContract))
                {
                    MessageBox.Show("***ERROR*** A Service ID of " + serviceId + " Already Exists Somewhere!", "Service ID EXISTS Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    return;
                }

                string dodd = txtDOD.Text;
                if (G1.validate_date(dodd)) // If Valid Deceased Date
                {
                    bool good = ValidateServiceId(serviceId); // Check for Valid Service Id
                    if (!good)
                    {
                        MessageBox.Show("***ERROR*** A Service ID of " + serviceId + "\nis not a value At Need Code\nor Merchandise Code!", "Service ID Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                        return;
                    }
                }

                this.Cursor = Cursors.WaitCursor;
                string record = workRecord;
                string fname = txtFirstName.Text;
                string lname = txtLastName.Text;
                string mname = txtMiddleName.Text;
                string suffix = txtSuffix.Text;
                string prefix = txtPrefix.Text;
                string legalName = txtFullLegalName.Text;
                string preferredName = txtPreferedName.Text;
                string maidenName = txtMaidenName.Text;
                string email = txtEmail.Text;
                string delivery = cmbDelivery.Text;
                string ssn = txtSSN.Text;
                ssn = ssn.Replace("-", "");
                if (!String.IsNullOrWhiteSpace(ssn))
                {
                    if (ssn.Length == 4)
                        ssn = "00000" + ssn;
                    int rv = ValidateSSN(ssn);
                    if (rv == 0)
                        G1.update_db_table(customersFile, "record", record, new string[] { "ssn", ssn });
                }
                G1.update_db_table(customersFile, "record", record, new string[] { "firstName", fname, "lastName", lname, "middleName", mname, "suffix", suffix, "prefix", prefix, "preferredName", preferredName, "legalName", legalName, "maidenName", maidenName, "emailAddress", email, "delivery", delivery });
                if (!String.IsNullOrWhiteSpace(workPayer))
                    ManualPayment.UpdatePayer(workPayer, fname, lname);

                string gender = "male";
                if (radioFemale.Checked)
                    gender = "female";
                string dob = dateDOB.Text;
                dob = G1.date_to_sql(dob);
                dob = dob.Replace("-", "");

                string race = "";
                string maritalStatus = "";
                //race = comboRace.Text;
                //maritalStatus = comboMaritalStatus.Text;
                string dod = txtDOD.Text;
                if (String.IsNullOrWhiteSpace(dod))
                    dateDeceased.Text = "";
                else
                    dateDeceased.Text = dod.ObjToDateTime().ToString("MM/dd/yyyy");
                bool gotDeceased = false;
                string deceasedDate = dateDeceased.Text;
                if (deceasedDate == "0/0/0000")
                {
                    deceasedDate = "01/01/0001 12:01 AM";
                    gotDeceased = true;
                }
                else
                {
                    deceasedDate = G1.date_to_sql(deceasedDate);
                    deceasedDate = deceasedDate.Replace("-", "");
                    gotDeceased = true;
                }

                string ethnicity = "";
                string language = "";
                serviceId = txtServiceId.Text.Trim();
                if ( serviceId != originalServiceId )
                {
                    ChangeServiceId(serviceId);
                    originalServiceId = serviceId;
                }
                string serviceDate = txtServiceDate.Text;
                string arrangementDate = txtArrangementDate.Text;
                string arrangementTime = txtArrangementTime.Text;
                if (!gotDeceased)
                {
                    serviceId = "";
                    serviceDate = "";
                    arrangementDate = "";
                    arrangementTime = "";
                }
                DateTime firstPayDate = txtFirstPayDate.Text.Trim().ObjToDateTime();
                //ethnicity = comboEthnicity.Text;
                //language = comboLanguage.Text;
                G1.update_db_table(customersFile, "record", record, new string[] { "birthDate", dob, "sex", gender, "ethnicity", ethnicity, "maritalstatus", maritalStatus, "race", race, "language", language, "deceasedDate", deceasedDate, "firstPayDate", firstPayDate.ToString("MM/dd/yyyy") });

                string address1 = textEdit_patientAddressLine1.Text;
                string address2 = textEdit_patientAddressLine2.Text;
                string city = textEdit_patientCity.Text;
                string state = comboStates.Text;
                string zip = textEdit_patientZipCode.Text;
                G1.update_db_table(customersFile, "record", record, new string[] { "address1", address1, "address2", address2, "city", city, "state", state, "zip1", zip });

                string mailAddress1 = textEdit2.Text;
                string mailAddress2 = textEdit3.Text;
                string mailCity = textEdit4.Text;
                string mailState = comboBox1.Text;
                string mailZip = textEdit1.Text;
                G1.update_db_table(customersFile, "record", record, new string[] { "mailAddress1", mailAddress1, "mailAddress2", mailAddress2, "mailCity", mailCity, "mailState", mailState, "mailZip1", mailZip });

                string phoneType1 = cmbPhoneQualifier1.Text;
                string phoneType2 = cmbPhoneQualifier2.Text;
                string phone1 = txtPhone1.Text;
                string phone2 = txtPhone2.Text;
                G1.update_db_table(customersFile, "record", record, new string[] { "phoneType1", phoneType1, "phoneType2", phoneType2, "phoneNumber1", phone1, "phoneNumber2", phone2, "serviceId", serviceId });

                if (gotDeceased)
                {
                    DataTable ddt = G1.get_db_data("Select * from `" + contractsFile + "` where `contractNumber` = '" + workContract + "';");
                    if (ddt.Rows.Count > 0)
                    {
                        string contractRecord = ddt.Rows[0]["record"].ObjToString();
                        G1.update_db_table(contractsFile, "record", contractRecord, new string[] { "deceasedDate", deceasedDate, "ServiceId", serviceId });
                        ddt.Dispose();
                        ddt = null;
                        if (!String.IsNullOrWhiteSpace(workPayer))
                            ManualPayment.UpdatePayer(workPayer, deceasedDate.ObjToDateTime());
                    }
                }
                string county = txtCounty.Text.Trim();
                string insideCity = cmbInsideCity.Text.Trim();
                FunFamily.ConfirmCustExtended(workContract, serviceId, serviceDate, custExtendedFile, county, insideCity, arrangementDate, arrangementTime);

                SaveFirstCallInfo();
                SaveHospiceInfo();

                ChangeAllPreNeeds(workContract, serviceId, deceasedDate);

                DateTime dDate = deceasedDate.ObjToDateTime();
                if (dDate.Year > 100)
                {
                    string cmd = "";
                    if (!String.IsNullOrWhiteSpace(workPayer))
                        cmd = "Select * from `creditcards` where `payer` = '" + workPayer + "';";
                    else
                        cmd = "Select * from `creditcards` where `contractNumber` = '" + workContract + "';";
                    DataTable dd = G1.get_db_data(cmd);
                    if (dd.Rows.Count > 0)
                    {
                        record = dd.Rows[0]["record"].ObjToString();
                        G1.update_db_table("creditcards", "record", record, new string[] { "status", "Pause" });
                    }
                }
                this.Cursor = Cursors.Default;
            }

            funModified = false;
            if (save && otherModified)
            {
                DataTable dt = (DataTable)dgv6.DataSource;
                //FunFamily.SaveOtherData(workContract, dt, workFuneral);
                T1.SaveOtherData(workContract, dt, workFuneral);
                otherModified = false;
            }
            if (DailyHistory.isInsurance(workContract))
            {
                if (!String.IsNullOrWhiteSpace(workPayer))
                    FunPayments.DeterminePayerDead(workPayer);
            }

            btnSaveAll.Hide();
        }
        /***********************************************************************************************/
        private void ChangeServiceId ( string newServiceId )
        {
            string record = "";
            string cmd = "Select * from `cust_payment_ins_checklist` WHERE `contractNumber` = '" + workContract + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                for (int i = 0; i < dx.Rows.Count; i++)
                {
                    record = dx.Rows[i]["record"].ObjToString();
                    G1.update_db_table("cust_payment_ins_checklist", "record", record, new string[] { "ServiceId", newServiceId });
                }
            }

            cmd = "Select * from `fcust_extended` WHERE `contractNumber` = '" + workContract + "';";
            dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                record = dx.Rows[0]["record"].ObjToString();
                G1.update_db_table("fcust_extended", "record", record, new string[] { "ServiceId", newServiceId });
            }

            FunPayments form = (FunPayments) G1.IsFormOpen("FunPayments", workContract );
            if (form != null)
            {
                form.FireEventServiceIdChanged(newServiceId);
            }

        }
        /***********************************************************************************************/
        private void ChangeAllPreNeeds(string contractNumber, string serviceId, string deceasedDate)
        {
            if (!workFuneral)
                return; // Not a Funeral
            if (contractNumber.ToUpper().IndexOf("SX") >= 0)
            {
                CleanoutPolicyPayments(contractNumber, deceasedDate, serviceId);
                return; // Not a Preneed Customer
            }

            string contractList = "";

            string cmd = "Select * from `customers` where `contractNumber` = '" + contractNumber + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return;

            string oldServiceId = dt.Rows[0]["serviceId"].ObjToString();
            if (String.IsNullOrWhiteSpace(oldServiceId))
                return;

            string oldDeceasedDate = dt.Rows[0]["deceasedDate"].ObjToString();
            if (oldDeceasedDate == "0/0/0000")
                oldDeceasedDate = "01/01/0001 12:01 AM";
            else
            {
                oldDeceasedDate = G1.date_to_sql(oldDeceasedDate);
                oldDeceasedDate = oldDeceasedDate.Replace("-", "");
            }

            string ssn = dt.Rows[0]["ssn"].ObjToString();
            ssn = ssn.Replace("-", "");
            if (String.IsNullOrWhiteSpace(ssn))
                return;
            if (ssn == "0" || ssn == "1")
                return;

            bool deceasedChanged = false;
            bool serviceIdChanged = false;

            if (deceasedDate != oldDeceasedDate)
                deceasedChanged = true;
            if (serviceId != oldServiceId)
                serviceIdChanged = true;

            if (!deceasedChanged && !serviceIdChanged)
                return;

            string record = "";
            string oldSSN = "";
            string cNum = "";
            DataTable dx = null;

            cmd = "Select * from `customers` where `serviceId` = '" + oldServiceId + "';";
            dt = G1.get_db_data(cmd);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                record = dt.Rows[i]["record"].ObjToString();
                oldSSN = dt.Rows[i]["ssn"].ObjToString();
                oldSSN = oldSSN.Replace("-", "");
                if (String.IsNullOrWhiteSpace(oldSSN))
                    continue;
                if (oldSSN == "0" || oldSSN == "1")
                    continue;
                if (oldSSN != ssn)
                    continue;
                G1.update_db_table("customers", "record", record, new string[] { "deceasedDate", deceasedDate, "serviceId", serviceId });

                cNum = dt.Rows[i]["contractNumber"].ObjToString();

                if (!String.IsNullOrWhiteSpace(contractList))
                    contractList += ",";
                contractList += cNum;

                cmd = "Select * from `contracts` where `contractNumber` = '" + cNum + "';";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    record = dx.Rows[0]["record"].ObjToString();
                    G1.update_db_table("contracts", "record", record, new string[] { "deceasedDate", deceasedDate, "serviceId", serviceId });
                }

                cmd = "Select * from `cust_extended` where `contractNumber` = '" + cNum + "';";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    record = dx.Rows[0]["record"].ObjToString();
                    G1.update_db_table("cust_extended", "record", record, new string[] { "serviceId", serviceId });
                }
            }

            CleanoutPolicyPayments(contractNumber, deceasedDate, serviceId);
        }
        /***********************************************************************************************/
        private void CleanoutPolicyPayments(string contractNumber, string deceasedDate, string serviceId)
        {
            if (String.IsNullOrWhiteSpace(contractNumber))
                return;
            string record = "";
            string cmd = "Select * from `cust_payments` where `contractNumber` = '" + contractNumber + "';";
            DataTable dt = G1.get_db_data(cmd);
            for (int i = 0; i < dt.Rows.Count; i++)
                CleanupPossibleInsurance(dt.Rows[i], deceasedDate, serviceId);
        }
        /****************************************************************************************/
        private void CleanupPossibleInsurance(DataRow dr, string deceasedDate, string serviceId)
        {
            try
            {
                string type = dr["type"].ObjToString().ToUpper();
                if (type.IndexOf("INSURANCE") >= 0 || type == "3RD PARTY" || type == "CLASS A")
                {
                    string names = dr["names"].ObjToString();
                    string trustOrPolicy = dr["trust_policy"].ObjToString();
                    string[] Lines = trustOrPolicy.Split('/');
                    if (Lines.Length >= 2)
                    {
                        string payer = Lines[0];
                        string policyNumber = Lines[1];
                        Lines = names.Split(',');
                        if (Lines.Length >= 2)
                        {
                            string lName = Lines[0].Trim();
                            string fName = Lines[1].Trim();
                            if (!String.IsNullOrWhiteSpace(policyNumber) && !String.IsNullOrWhiteSpace(payer) && !String.IsNullOrWhiteSpace(lName) && !String.IsNullOrWhiteSpace(fName))
                                FunPayments.UpdatePayerPolicies(payer, policyNumber, fName, lName, deceasedDate, serviceId, false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
        /***********************************************************************************************/
        private void SaveFirstCallInfo()
        {
            string cmd = "Select * from `fcust_extended` where `contractNumber` = '" + workContract + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return;
            string record = dt.Rows[0]["record"].ObjToString();
            string familyPresent = "NO";
            if (firstCallFamily)
                familyPresent = "YES";
            G1.update_db_table("fcust_extended", "record", record, new string[] { "FirstCallDetail", firstCallInformation, "FirstCallFamilyPresent", familyPresent });
        }
        /***********************************************************************************************/
        private void SaveHospiceInfo()
        {
            //string cmd = "Select * from `fcust_extended` where `contractNumber` = '" + workContract + "';";
            //DataTable dt = G1.get_db_data(cmd);
            //if (dt.Rows.Count <= 0)
            //    return;
            //string record = dt.Rows[0]["record"].ObjToString();
            //string familyPresent = "NO";
            //if (hospiceFamily)
            //    familyPresent = "YES";
            //G1.update_db_table("fcust_extended", "record", record, new string[] { "HospiceDetail", hospiceInformation, "HospiceFamilyPresent", familyPresent });
        }
        /***********************************************************************************************/
        private void LoadCustomerPicture()
        {
            string cmd = "Select * from `" + customersFile + "` where `record` = '" + workRecord + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
            {
                if (customersFile == "fcustomers")
                {
                    cmd = "Select * from `customers` where `contractNumber` = '" + workContract + "';";
                    dt = G1.get_db_data(cmd);
                    if (dt.Rows.Count <= 0)
                        return;
                }
            }
            Byte[] bytes = dt.Rows[0]["picture"].ObjToBytes();
            Image myImage = new Bitmap(1, 1);
            if (bytes != null)
            {
                myImage = G1.byteArrayToImage(bytes);
                this.picDec.Image = (Bitmap)myImage;
            }
        }
        /****************************************************************************************/
        private void picDec_Click(object sender, EventArgs e)
        {
            string lines = "Add Picture\nClear Picture\nDo Nothing\n";
            using (SelectFromList listForm = new SelectFromList(lines, false))
            {
                listForm.Text = "Select Picture Option";
                listForm.ListDone += ListForm_PictureDone;
                listForm.ShowDialog();
            }
            //using (ListSelect listForm = new ListSelect(lines, false))
            //{
            //    listForm.Text = "Select Picture Option";
            //    listForm.ListDone += ListForm_PictureDone;
            //    listForm.ShowDialog();
            //}
        }
        /***********************************************************************************************/
        private void ListForm_PictureDone(string s)
        {
            if (s == "Do Nothing")
                return;
            if (s == "Add Picture")
            {
                //Image myImage = new Bitmap(1, 1);
                //ImageConverter conver = new ImageConverter();
                //var b = (byte[])conver.ConvertTo(myImage, typeof(byte[]));
                //ViewImage view = (ViewImage) new ViewImage(b);
                //view.ShowDialog();

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
                                if (workFuneral)
                                    G1.update_blob("fcustomers", "record", workRecord, "picture", bytes);
                                else
                                    G1.update_blob("customers", "record", workRecord, "picture", bytes);
                                this.picDec.Image = (Bitmap)myNewImage;
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("***ERROR*** Storing Image " + ex.ToString(), "Storing Image Problem Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                            }
                        }
                    }
                    this.Refresh();
                }
            }
            else if (s == "Clear Picture")
            {
                Bitmap emptyImage = new Bitmap(1, 1);
                ImageConverter converter = new ImageConverter();
                var bytes = (byte[])converter.ConvertTo(emptyImage, typeof(byte[]));
                G1.update_blob("customers", "record", workRecord, "picture", bytes);
                this.picDec.Image = (Bitmap)emptyImage;
            }
        }
        /****************************************************************************************/
        private void panelAll_Paint(object sender, PaintEventArgs e)
        {
            Rectangle rect = panelAll.Bounds;
            Graphics g = panelAll.CreateGraphics();
            Pen pen = new Pen(Brushes.Black);
            int left = rect.Left;
            int top = rect.Top;
            int width = rect.Width - 1;
            int high = rect.Height - 1;
            g.DrawRectangle(pen, left, top, width, high);
        }
        /***************************************************************************************/
        public delegate void d_void_eventdone_string(string s);
        public event d_void_eventdone_string CustomerModifiedDone;
        protected void OnCustomerModified()
        {
            if (loading)
                return;
            if (CustomerModifiedDone != null)
            {
                CustomerModifiedDone.Invoke("YES");
            }
        }
        /****************************************************************************************/
        private void ssnChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            string ssn = txtSSN.Text;
            if (ssn.Length == 3)
            {
                ssn += "-";
                txtSSN.Text = ssn;
            }
            else if (ssn.Length == 6)
            {
                ssn += "-";
                txtSSN.Text = ssn;
            }
            txtSSN.Refresh();
            txtSSN.Select(txtSSN.Text.Length, 0);

            funModified = true;
            OnCustomerModified();
            btnSaveAll.Show();
        }
        /****************************************************************************************/
        private void somethingChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            funModified = true;
            OnCustomerModified();
            btnSaveAll.Show();
        }
        /***********************************************************************************************/
        private void DeceasedTextChanged(object sender, EventArgs e)
        {
            string date = dateDeceased.Text;
            if (G1.validate_date(date))
            {
                DateTime ddate = date.ObjToDateTime();
                txtServiceId.Enabled = true;
                txtServiceDate.Enabled = true;
                txtArrangementDate.Enabled = true;
                txtArrangementTime.Enabled = true;
            }
            else
            {
                txtServiceId.Enabled = false;
                txtServiceId.Text = "";
                txtServiceDate.Enabled = false;
                txtServiceDate.Text = "";
                txtArrangementDate.Enabled = false;
                txtArrangementDate.Text = "";
                txtArrangementTime.Enabled = false;
                txtArrangementTime.Text = "";
            }
            funModified = true;
            OnCustomerModified();
            btnSaveAll.Show();
        }
        /***********************************************************************************************/
        private void dateDeceased_EditValueChanged(object sender, EventArgs e)
        {
            string date = dateDeceased.Text;
            if (G1.validate_date(date))
            {
                DateTime ddate = date.ObjToDateTime();
                txtServiceId.Enabled = true;
                txtServiceDate.Enabled = true;
                txtArrangementDate.Enabled = true;
                txtArrangementTime.Enabled = true;
            }
            else
            {
                txtServiceId.Enabled = false;
                txtServiceId.Text = "";
                txtServiceDate.Enabled = false;
                txtServiceDate.Text = "";
                txtArrangementDate.Enabled = false;
                txtArrangementDate.Text = "";
                txtArrangementTime.Enabled = false;
                txtArrangementTime.Text = "";
            }
            funModified = true;
            OnCustomerModified();
            btnSaveAll.Show();
        }
        /***********************************************************************************************/
        private void dateDeceased_Leave(object sender, EventArgs e)
        {
            string date = dateDeceased.Text;
            DateTime ddate = dateDeceased.DateTime;
            if (ddate.Year > 100)
            {
                dateDeceased.Text = ddate.ToString("MM/dd/yyyy");
                txtServiceId.Enabled = true;
                txtServiceDate.Enabled = true;
                txtArrangementDate.Enabled = true;
                txtArrangementTime.Enabled = true;
            }
            else
            {
                txtServiceId.Enabled = false;
                txtServiceId.Text = "";
                txtServiceDate.Enabled = false;
                txtServiceDate.Text = "";
                txtArrangementDate.Enabled = false;
                txtArrangementDate.Text = "";
                txtArrangementTime.Enabled = false;
                txtArrangementTime.Text = "";
            }
        }
        /***********************************************************************************************/
        private void dateDeceased_Enter(object sender, EventArgs e)
        {
            string date = dateDeceased.Text;
            if (G1.validate_date(date))
            {
                DateTime ddate = date.ObjToDateTime();
                dateDeceased.EditValue = ddate.ToString("MM/dd/yyyy");
                date = dateDeceased.Text;
                txtServiceId.Enabled = true;
                txtServiceDate.Enabled = true;
                txtArrangementDate.Enabled = true;
                txtArrangementTime.Enabled = true;
            }
            else
            {
                txtServiceId.Enabled = false;
                txtServiceId.Text = "";
                txtServiceDate.Enabled = false;
                txtServiceDate.Text = "";
                txtArrangementDate.Enabled = false;
                txtArrangementDate.Text = "";
                txtArrangementTime.Enabled = false;
                txtArrangementTime.Text = "";
            }
        }
        /****************************************************************************************/
        private void FunCustomer_FormClosed(object sender, FormClosedEventArgs e)
        {
        }
        /****************************************************************************************/
        private void dateDOB_Enter(object sender, EventArgs e)
        {
            string date = dateDOB.Text;
            if (G1.validate_date(date))
            {
                DateTime ddate = date.ObjToDateTime();
                dateDOB.EditValue = ddate.ToString("MM/dd/yyyy");
                date = dateDOB.Text;
            }
        }
        /****************************************************************************************/
        private void dateDOB_Leave(object sender, EventArgs e)
        {
            string date = dateDOB.Text;
            if (G1.validate_date(date))
            {
                DateTime ddate = date.ObjToDateTime();
                dateDOB.Text = ddate.ToString("MM/dd/yyyy");
            }
        }
        /***********************************************************************************************/
        private void dateDOB_EditValueChanged(object sender, EventArgs e)
        {
            string date = dateDOB.Text;
            if (G1.validate_date(date))
            {
                DateTime ddate = date.ObjToDateTime();
            }
            funModified = true;
            OnCustomerModified();
            btnSaveAll.Show();
        }
        /****************************************************************************************/
        private void dateDeceased_TextChanged(object sender, EventArgs e)
        {
            string date = dateDeceased.Text;
            //DateTime ddate = dateDeceased.DateTime;
            //if (ddate.Year > 100)
            //{
            //    dateDeceased.Text = ddate.ToString("MM/dd/yyyy");
            //    txtServiceId.Enabled = true;
            //}
            //else
            //{
            //    txtServiceId.Enabled = false;
            //    txtServiceId.Text = "";
            //}
        }
        /****************************************************************************************/
        private void ResetDOD()
        {
            string cmd = "Select * from `" + customersFile + "` where `contractNumber` = '" + workContract + "';";
            DataTable dx = G1.get_db_data(cmd);
            DateTime ddate = dx.Rows[0]["deceasedDate"].ObjToDateTime();
            if (ddate.Year < 1875)
                ddate = dx.Rows[0]["deceasedDate"].ObjToDateTime();
            dateDeceased.Text = ddate.ToString("MM/dd/yyyy");
            if (ddate.Year > 1800)
            {
                txtDOD.Text = ddate.ToString("MM/dd/yyyy");
                txtDOD.Refresh();
            }
        }
        /****************************************************************************************/
        private void txtDOD_Leave(object sender, EventArgs e)
        {
            string date = txtDOD.Text;
            if (String.IsNullOrWhiteSpace(date))
            {
                if (workFuneral)
                {
                    MessageBox.Show("***ERROR*** You cannot Blank Out the Date of Death of an Existing Funeral!", "Date Problem Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    ResetDOD();
                    return;
                }
                if (txtServiceId.Enabled)
                    btnSaveAll.Show();
                txtServiceId.Enabled = false;
                txtServiceId.Text = "";
                txtServiceDate.Enabled = false;
                txtServiceDate.Text = "";
                txtArrangementDate.Enabled = false;
                txtArrangementDate.Text = "";
                txtArrangementTime.Enabled = false;
                txtArrangementTime.Text = "";
                return;
            }
            //            DateTime ddate = dateDeceased.DateTime;
            if (G1.validate_date(date))
            {
                DateTime ddate = txtDOD.Text.ObjToDateTime();
                if (ddate.Year < 1800)
                {
                    MessageBox.Show("***ERROR*** Date Entered Year is less than 1800!", "Date Problem Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    ResetDOD();
                    //txtServiceId.Enabled = false;
                    //txtServiceId.Text = "";
                    //txtServiceDate.Enabled = false;
                    //txtServiceDate.Text = "";
                    //txtArrangementDate.Enabled = false;
                    //txtArrangementDate.Text = "";
                    return;
                }
                if (ddate.Year > 100)
                {
                    txtDOD.Text = ddate.ToString("MM/dd/yyyy");
                    CalculateAge();
                    txtServiceId.Enabled = true;
                    dateDeceased.Text = txtDOD.Text;
                    txtServiceDate.Enabled = true;
                    txtArrangementDate.Enabled = true;
                    txtArrangementTime.Enabled = true;
                }
                else
                {
                    txtServiceId.Enabled = false;
                    txtServiceId.Text = "";
                    txtServiceDate.Enabled = false;
                    txtServiceDate.Text = "";
                    txtArrangementDate.Enabled = false;
                    txtArrangementDate.Text = "";
                    txtArrangementTime.Enabled = false;
                    txtArrangementTime.Text = "";
                }
            }
            else
            {
                MessageBox.Show("***ERROR*** Invalid Date!", "Date Problem Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            }
        }
        /****************************************************************************************/
        private void txtDOD_Enter(object sender, EventArgs e)
        {
            string date = txtDOD.Text;
            if (G1.validate_date(date))
            {
                DateTime ddate = date.ObjToDateTime();
                //                dateDeceased.EditValue = ddate.ToString("MM/dd/yyyy");
                txtDOD.Text = ddate.ToString("MM/dd/yyyy");
                date = dateDeceased.Text;
                txtServiceId.Enabled = true;
                txtServiceDate.Enabled = true;
                txtArrangementDate.Enabled = true;
                txtArrangementTime.Enabled = true;
            }
            else
            {
                bool save = false;
                if (btnSaveAll.Visible)
                    save = true;
                txtDOD.Text = "";
                txtServiceId.Enabled = false;
                txtServiceId.Text = "";
                txtServiceDate.Enabled = false;
                txtServiceDate.Text = "";
                txtArrangementDate.Enabled = false;
                txtArrangementDate.Text = "";
                txtArrangementTime.Enabled = false;
                txtArrangementTime.Text = "";
                if (!save)
                {
                    btnSaveAll.Visible = false;
                    funModified = false;
                }
            }
        }
        /****************************************************************************************/
        private void txtDOD_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                txtDOD_Leave(sender, e);
            else if (e.KeyCode == Keys.Tab)
                txtDOD_Leave(sender, e);
        }
        /****************************************************************************************/
        private void txtBday_Leave(object sender, EventArgs e)
        {
            string date = txtBday.Text;
            if (String.IsNullOrWhiteSpace(date))
                return;
            if (G1.validate_date(date))
            {
                DateTime ddate = txtBday.Text.ObjToDateTime();
                if (ddate.Year < 1800)
                {
                    MessageBox.Show("***ERROR*** Date Entered Year is less than 1800!", "DateProblem Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    return;
                }
                if (ddate.Year > 100)
                {
                    txtBday.Text = ddate.ToString("MM/dd/yyyy");
                    dateDOB.Text = txtBday.Text;
                    CalculateAge();
                }
            }
            else
            {
                MessageBox.Show("***ERROR*** Invalid Date!", "DateProblem Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            }
        }
        /****************************************************************************************/
        private void txtBday_Enter(object sender, EventArgs e)
        {
            string date = txtBday.Text;
            if (G1.validate_date(date))
            {
                DateTime ddate = date.ObjToDateTime();
                txtBday.Text = ddate.ToString("MM/dd/yyyy");
            }
            else
                txtBday.Text = "";
        }
        /****************************************************************************************/
        private void txtBday_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                txtBday_Leave(sender, e);
            else if (e.KeyCode == Keys.Tab)
                txtBday_Leave(sender, e);
        }
        /****************************************************************************************/
        private void CalculateAge()
        {
            DateTime bDay = txtBday.Text.ObjToDateTime();
            if (bDay.Year > 500)
            {
                DateTime dod = txtDOD.Text.ObjToDateTime();
                if (dod.Year < 100)
                    dod = DateTime.Now;
                txtAge.Text = G1.GetAge(bDay, dod).ToString();
            }
        }
        /****************************************************************************************/
        private bool specialLoading = false;
        /****************************************************************************************/
        private void AutoPopulatePOD(DataTable dt6, string field, string what)
        {
            string cmd = "";
            DataTable dt = null;
            DataRow[] dRows = null;
            if (what.ToUpper().IndexOf("DECEDENT") >= 0 || String.IsNullOrWhiteSpace ( what ))
            {
                string address = textEdit_patientAddressLine1.Text;
                address = G1.force_lower_line(address);

                string city = textEdit_patientCity.Text;
                string state = comboStates.Text;
                string zip = textEdit_patientZipCode.Text;
                string county = txtCounty.Text;
                string phone = txtPhone1.Text;
                string reference = "";

                if ( String.IsNullOrWhiteSpace ( what ))
                {
                    address = "";
                    city = "";
                    state = "";
                    zip = "";
                    county = "";
                    phone = "";
                }

                dRows = dt6.Select("field='Place of Death - Name'");
                if (dRows.Length > 0)
                {
                    dRows[0]["data"] = "";
                    reference = dRows[0]["reference"].ObjToString();
                    dRows[0]["reference"] = "/PROTECT";
                    dRows[0]["mod"] = "Y";
                }
                dRows = dt6.Select("field='Place of Death - Address'");
                if (dRows.Length > 0)
                {
                    dRows[0]["data"] = address;
                    reference = dRows[0]["reference"].ObjToString();
                    dRows[0]["reference"] = "/PROTECT";
                    dRows[0]["mod"] = "Y";
                }
                dRows = dt6.Select("field='Place of Death - City'");
                if (dRows.Length > 0)
                {
                    dRows[0]["data"] = city;
                    dRows[0]["reference"] = "/PROTECT";
                    dRows[0]["mod"] = "Y";
                }
                dRows = dt6.Select("field='Place of Death - State'");
                if (dRows.Length > 0)
                {
                    dRows[0]["data"] = state;
                    dRows[0]["reference"] = "/PROTECT";
                    dRows[0]["mod"] = "Y";
                }
                dRows = dt6.Select("field='Place of Death - Zip Code'");
                if (dRows.Length > 0)
                {
                    dRows[0]["data"] = zip;
                    dRows[0]["reference"] = "/PROTECT";
                    dRows[0]["mod"] = "Y";
                }
                dRows = dt6.Select("field='Place of Death - County'");
                if (dRows.Length > 0)
                {
                    dRows[0]["data"] = county;
                    dRows[0]["reference"] = "/PROTECT";
                    dRows[0]["mod"] = "Y";
                }
                dRows = dt6.Select("field='Place of Death - Phone'");
                if (dRows.Length > 0)
                {
                    dRows[0]["data"] = phone;
                    dRows[0]["reference"] = "/PROTECT";
                    dRows[0]["mod"] = "Y";
                }
                dRows = dt6.Select("field='Place of Death - Name'");
                if (dRows.Length > 0 && !String.IsNullOrWhiteSpace ( what ))
                {
                    dRows[0]["reference"] = "/PROTECT";
                    string residence = "";

                    bool female = this.radioFemale.Checked;
                    bool male = this.radioMale.Checked;

                    if ( male )
                        residence = "his Residence";
                    else
                        residence = "her Residence";
                    dRows[0]["data"] = residence;
                    dRows[0]["tracking"] = "";
                    dRows[0]["help"] = "";
                    dRows[0]["mod"] = "Y";
                    ciLookupSave = ciLookup;
                    ciLookup.Items.Clear();
                    gridMain6.Columns["data"].ColumnEdit = ciLookup;
                    gridMain6.RefreshData();
                    gridMain6.RefreshEditor(true);
                    DataRow[] dR = trackingDt.Select("tracking='POD'");
                    if (dR.Length > 0)
                    {
                        dR[0]["tracking"] = "XPOD";
                        dR[0]["using"] = "XPOD";
                    }
                }
            }
            else
            {
                dRows = dt6.Select("field='Place of Death - Name'");
                if (dRows.Length > 0)
                {
                    string saveData = dRows[0]["data"].ObjToString();
                    dRows[0]["data"] = "";
                    dRows[0]["reference"] = "";
                    dRows[0]["tracking"] = "T";
                    dRows[0]["help"] = "Tracking";
                    ciLookup = ciLookupSave;
                    gridMain6.Columns["data"].ColumnEdit = ciLookup;
                    gridMain6.RefreshData();
                    gridMain6.RefreshEditor(true);
                    DataRow[] dR = trackingDt.Select("tracking='XPOD'");
                    if (dR.Length > 0)
                    {
                        dR[0]["tracking"] = "POD";
                        dR[0]["using"] = "POD";
                    }
                    DataRow[] xRow = trackDt.Select("tracking='POD' AND answer='" + what + "' AND location='ALL'");
                    if (xRow.Length <= 0)
                        xRow = trackDt.Select("tracking='POD' AND answer='" + what + "' AND location='" + EditCust.activeFuneralHomeName + "'");

                    dRows = dt6.Select("field='Place of Death - Address'");
                    if (dRows.Length > 0)
                    {
                        dRows[0]["reference"] = "POD~Address";
                        if (xRow.Length > 0)
                        {
                            dRows[0]["data"] = xRow[0]["address"].ObjToString();
                            dRows[0]["mod"] = "Y";
                        }
                    }
                    dRows = dt6.Select("field='Place of Death - City'");
                    if (dRows.Length > 0)
                    {
                        dRows[0]["reference"] = "POD~City";
                        if (xRow.Length > 0)
                        {
                            dRows[0]["data"] = xRow[0]["city"].ObjToString();
                            dRows[0]["mod"] = "Y";
                        }
                    }
                    dRows = dt6.Select("field='Place of Death - State'");
                    if (dRows.Length > 0)
                    {
                        dRows[0]["reference"] = "POD~State";
                        if (xRow.Length > 0)
                        {
                            dRows[0]["data"] = xRow[0]["state"].ObjToString();
                            dRows[0]["mod"] = "Y";
                        }
                    }
                    dRows = dt6.Select("field='Place of Death - Zip Code'");
                    if (dRows.Length > 0)
                    {
                        dRows[0]["reference"] = "POD~Zip";
                        if (xRow.Length > 0)
                        {
                            dRows[0]["data"] = xRow[0]["zip"].ObjToString();
                            dRows[0]["mod"] = "Y";
                        }
                    }
                    dRows = dt6.Select("field='Place of Death - County'");
                    if (dRows.Length > 0)
                    {
                        dRows[0]["reference"] = "POD~County";
                        if (xRow.Length > 0)
                        {
                            dRows[0]["data"] = xRow[0]["county"].ObjToString();
                            dRows[0]["mod"] = "Y";
                        }
                    }
                    dRows = dt6.Select("field='Place of Death - Phone'");
                    if (dRows.Length > 0)
                    {
                        dRows[0]["reference"] = "POD~Phone";
                        if (xRow.Length > 0)
                        {
                            dRows[0]["data"] = xRow[0]["phone"].ObjToString();
                            dRows[0]["mod"] = "Y";
                        }
                    }
                    if (!String.IsNullOrWhiteSpace(saveData))
                    {
                        dRows = dt6.Select("field='Place of Death - Name'");
                        if (dRows.Length > 0)
                            dRows[0]["data"] = saveData;
                    }
                }
            }
        }
        /****************************************************************************************/
        private void ProcessDataChanged(DataTable dt6, string field, string what)
        {
            if (field.ToUpper().IndexOf("LOCATION") < 0)
            {
                if (field.ToUpper().IndexOf("PLACE OF DEATH") >= 0)
                {
                    AutoPopulatePOD(dt6, field, what);
                }
                return;
            }
            string cmd = "";
            DataTable dx = null;
            DataRow[] dRows = null;
            DataRow[] dR = null;
            string reference = "";

            if (trackDt == null)
                trackDt = G1.get_db_data("Select * from `track`;");
            if (workDt6 == null)
                workDt6 = trackDt.Clone();

            dR = trackDt.Select("answer='" + what + "' AND location='" + EditCust.activeFuneralHomeName + "'");
            if (dR.Length <= 0)
                dR = workDt6.Select("answer='" + what + "' AND location='" + EditCust.activeFuneralHomeName + "'");
            if (dR.Length <= 0)
                return;

            specialLoading = true;
            string answer = "";
            string dbField = "";
            string newField = field.ToUpper().Replace("LOCATION", "ADDRESS");
            dRows = dt6.Select("field='" + newField + "'");
            if (dRows.Length > 0)
            {
                reference = dRows[0]["reference"].ObjToString();
                dbField = dRows[0]["dbField"].ObjToString();
                answer = ProcessReference(dR, reference);
                dRows[0]["data"] = answer;
                //                dRows[0]["data"] = dR[0]["address"].ObjToString();
            }

            newField = field.ToUpper().Replace("LOCATION", "CITY");
            dRows = dt6.Select("field='" + newField + "'");
            if (dRows.Length > 0)
            {
                reference = dRows[0]["reference"].ObjToString();
                dbField = dRows[0]["dbField"].ObjToString();
                answer = ProcessReference(dR, reference);
                dRows[0]["data"] = answer;
                //dRows[0]["data"] = dR[0]["city"].ObjToString();
            }

            newField = field.ToUpper().Replace("LOCATION", "STATE");
            dRows = dt6.Select("field='" + newField + "'");
            if (dRows.Length > 0)
            {
                reference = dRows[0]["reference"].ObjToString();
                dbField = dRows[0]["dbField"].ObjToString();
                answer = ProcessReference(dR, reference);
                dRows[0]["data"] = answer;
                //                dRows[0]["data"] = dR[0]["state"].ObjToString();
            }

            newField = field.ToUpper().Replace("LOCATION", "ZIP");
            dRows = dt6.Select("field='" + newField + "'");
            if (dRows.Length > 0)
            {
                reference = dRows[0]["reference"].ObjToString();
                dbField = dRows[0]["dbField"].ObjToString();
                answer = ProcessReference(dR, reference);
                dRows[0]["data"] = answer;
                //                dRows[0]["data"] = dR[0]["zip"].ObjToString();
            }

            specialLoading = false;
        }
        /***************************************************************************************/
        private string ProcessReference(DataRow[] dR, string field)
        {
            if (dR.Length <= 0)
                return "";
            string answer = "";
            if (String.IsNullOrWhiteSpace(field))
                return answer;
            try
            {
                string[] Lines = null;
                if (field.IndexOf("~") >= 0)
                {
                    Lines = field.Split('~');
                    if (Lines.Length <= 1)
                        return answer;
                    field = Lines[1];
                }

                if (field.IndexOf("+") < 0)
                    answer = dR[0][field].ObjToString();
                else
                {
                    Lines = field.Split('+');
                    string str = "";
                    for (int j = 0; j < Lines.Length; j++)
                    {
                        field = Lines[j].Trim();
                        try
                        {
                            if (!String.IsNullOrWhiteSpace(field))
                            {
                                str = dR[0][field].ObjToString();
                                answer += str + " ";
                            }
                        }
                        catch (Exception ex)
                        {
                            if (field == ",")
                                answer = answer.Trim();
                            answer += field;
                            if (field == ",")
                                answer += " ";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return answer;
        }
        /****************************************************************************************/
        private void ProcessDataChangedx(DataTable dt6, string field, string what)
        {
            if (field.ToUpper().IndexOf("LOCATION") < 0)
                return;
            string cmd = "";
            DataTable dx = null;
            DataRow[] dRows = null;

            cmd = "Select * from `track` where `answer` = '" + what + "' ";
            if (!String.IsNullOrWhiteSpace(EditCust.activeFuneralHomeName))
                cmd += " and `location` = '" + EditCust.activeFuneralHomeName + "' ";
            cmd += ";";
            dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                specialLoading = true;
                string newField = field.ToUpper().Replace("LOCATION", "ADDRESS");
                dRows = dt6.Select("field='" + newField + "'");
                if (dRows.Length > 0)
                    dRows[0]["data"] = dx.Rows[0]["address"].ObjToString();

                newField = field.ToUpper().Replace("LOCATION", "CITY");
                dRows = dt6.Select("field='" + newField + "'");
                if (dRows.Length > 0)
                    dRows[0]["data"] = dx.Rows[0]["city"].ObjToString();

                newField = field.ToUpper().Replace("LOCATION", "STATE");
                dRows = dt6.Select("field='" + newField + "'");
                if (dRows.Length > 0)
                    dRows[0]["data"] = dx.Rows[0]["state"].ObjToString();

                newField = field.ToUpper().Replace("LOCATION", "ZIP");
                dRows = dt6.Select("field='" + newField + "'");
                if (dRows.Length > 0)
                    dRows[0]["data"] = dx.Rows[0]["zip"].ObjToString();


                specialLoading = false;
            }
        }
        /****************************************************************************************/
        private void gridMain6_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (specialLoading)
                return;
            DataTable dt = (DataTable)dgv6.DataSource;
            DataRow dr = gridMain6.GetFocusedDataRow();
            dr["mod"] = "Y";
            otherModified = true;
            funModified = true;
            btnSaveAll.Show();
            int rowHandle = gridMain6.FocusedRowHandle;
            int row = gridMain6.GetDataSourceRowIndex(rowHandle);
            DataTable dt6 = (DataTable)dgv6.DataSource;
            DataRow[] dR = null;

            GridColumn currCol = gridMain6.FocusedColumn;
            currentColumn = currCol.FieldName;
            string dbField = "";
            string field = dt6.Rows[row]["field"].ObjToString();
            string what = dr["data"].ObjToString();
            if (String.IsNullOrWhiteSpace(what))
            {
                dr["add"] = "";
                dr["edit"] = "";
                dbField = dt6.Rows[row]["dbField"].ObjToString();
                if (dbField.ToUpper() == "FUNERAL DIRECTOR")
                {
                    dR = dt.Select("field='Funeral Director - License # '");
                    if (dR.Length > 0)
                    {
                        dR[0]["data"] = "";
                        dR[0]["mod"] = "Y";
                    }
                }
                if ( field.ToUpper() == "PLACE OF DEATH")
                {
                    AutoPopulatePOD(dt6, field, what);
                }
                return;
            }

            string record = dt6.Rows[row]["record"].ObjToString();
            dbField = dt6.Rows[row]["dbField"].ObjToString();
            string tract = dt6.Rows[row]["help"].ObjToString().ToUpper();
            string reference = dt6.Rows[row]["reference"].ObjToString();
            //if ( field == "Service Clergy" )
            //{
            //}

            DataRow[] dRows = null;
            dR = null;
            DataTable dx = null;
            string cmd = "";

            try
            {
                if (gridMain6.Columns[currentColumn].ColumnEdit != null || tract.ToUpper() == "TRACKING")
                {
                    string answers = "";
                    bool found = false;
                    for (int i = 0; i < ciLookup.Items.Count; i++)
                    {
                        answers = ciLookup.Items[i].ObjToString();
                        if (what.Trim().ToUpper() == answers.Trim().ToUpper())
                        {
                            found = true;
                        }
                    }
                    if (!found)
                    {
                        if (dbField.ToUpper() != "FUNERAL ARRANGER"  && dbField.ToUpper() != "FUNERAL DIRECTOR")
                        {
                            dr["add"] = "+";
                            dr["edit"] = "E";
                        }
                    }
                    else
                    {
                        dr["add"] = "";
                        dr["edit"] = "";
                    }
                    bool accepted = false;
                    if (!found)
                    {
                        if (isProtected)
                        {
                            MessageBox.Show("***SORRY*** Field is protected.\nYou must choose from the dropdown!", "Data Entry Problem Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                            dr["data"] = "";
                            dr["add"] = "";
                            dr["edit"] = "";
                            return;
                        }
                        using (FuneralDemo funDemo = new FuneralDemo(field, what))
                        {
                            funDemo.ShowDialog();
                            if (funDemo.DialogResult == System.Windows.Forms.DialogResult.OK)
                            {
                                string address = funDemo.FireEventFunDemo("address");
                                string city = funDemo.FireEventFunDemo("city");
                                string county = funDemo.FireEventFunDemo("county");
                                string state = funDemo.FireEventFunDemo("state");
                                string zip = funDemo.FireEventFunDemo("zip");
                                string phone = funDemo.FireEventFunDemo("phone");
                                if (workDt6 == null)
                                {
                                    if (trackDt == null)
                                        trackDt = G1.get_db_data("Select * from `track`;");
                                    workDt6 = trackDt.Clone();
                                }
                                dR = workDt6.Select("tracking='" + field + "' and location='" + EditCust.activeFuneralHomeName + "'");
                                if (dR.Length <= 0)
                                {
                                    DataRow d = workDt6.NewRow();
                                    d["tracking"] = field;
                                    d["answer"] = what;
                                    d["address"] = address;
                                    d["city"] = city;
                                    d["county"] = county;
                                    d["state"] = state;
                                    d["zip"] = zip;
                                    d["phone"] = phone;
                                    d["location"] = EditCust.activeFuneralHomeName;
                                    workDt6.Rows.Add(d);
                                    dR = workDt6.Select("tracking='" + field + "' and location='" + EditCust.activeFuneralHomeName + "'");
                                }
                                else
                                {
                                    dR[0]["answer"] = what;
                                    dR[0]["address"] = address;
                                    dR[0]["city"] = city;
                                    dR[0]["county"] = county;
                                    dR[0]["state"] = state;
                                    dR[0]["zip"] = zip;
                                    dR[0]["phone"] = phone;
                                    dR[0]["location"] = EditCust.activeFuneralHomeName;
                                }

                                dbField = FixUsingFieldData(dbField);
                                dRows = dt.Select("reference LIKE '" + dbField + "~%'");
                                if (dRows.Length > 0)
                                {
                                    reference = "";
                                    for (int i = 0; i < dRows.Length; i++)
                                    {
                                        reference = dRows[i]["reference"].ObjToString();
                                        string answer = ProcessReference(dR, reference);
                                        dRows[i]["data"] = answer;
                                        dRows[i]["mod"] = "Y";
                                    }
                                }

                                //ProcessDataChanged(dt6, field, what);
                                ReloadTrack();
                                gridMain6_ShownEditor(null, null);
                                accepted = true;
                            }
                        }
                        if (!accepted)
                        {
                            if (reference.IndexOf("~") < 0)
                                return;
                            string[] Lines = reference.Split('~');
                            if (Lines.Length >= 2)
                            {
                                try
                                {
                                    string majorField = Lines[0].Trim();
                                    string myfield = Lines[1].Trim();
                                    dR = dt.Select("dbField = '" + majorField + "'");
                                    if (dR.Length > 0)
                                    {
                                        string answer = dR[0]["data"].ObjToString();
                                        dR = trackDt.Select("tracking='" + majorField + "' AND answer='" + answer + "' AND location='" + EditCust.activeFuneralHomeName + "'");
                                        if (dR.Length > 0)
                                        {
                                            record = dR[0]["record"].ObjToString();
                                            dR[0][field] = what;
                                            G1.update_db_table("track", "record", record, new string[] { field, what });
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                }
                            }
                        }
                    }
                    else
                        ProcessDataChanged(dt6, field, what);
                }
                else
                {
                    if (reference.IndexOf("~") < 0)
                    {
                        if (reference.ToUpper() == "/PROTECT")
                        {
                            MessageBox.Show("***SORRY*** Field is protected.\nYou cannot change at this time!", "Data Entry Problem Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                            dr["data"] = oldWhat;
                            dr["add"] = "";
                            dr["edit"] = "";
                            return;
                        }
                        return;
                    }
                    string[] Lines = reference.Split('~');
                    if (Lines.Length >= 2)
                    {
                        try
                        {
                            string majorField = Lines[0].Trim();
                            string myfield = Lines[1].Trim();
                            dR = dt.Select("dbField = '" + majorField + "'");
                            if (dR.Length > 0)
                            {
                                string answer = dR[0]["data"].ObjToString();
                                dR = trackDt.Select("tracking='" + majorField + "' AND answer='" + answer + "' AND location='" + EditCust.activeFuneralHomeName + "'");
                                if ( dR.Length <= 0 )
                                    dR = trackDt.Select("tracking='" + majorField + "' AND answer='" + answer + "'");
                                if (dR.Length > 0)
                                {
                                    record = dR[0]["record"].ObjToString();
                                    dR[0][myfield] = what;
                                    G1.update_db_table("track", "record", record, new string[] { myfield, what });
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
            //if (funDemo != null)
            //{
            //    if (!funDemo.IsDisposed)
            //    {
            //        if (funDemo.Visible)
            //            funDemo.Hide();
            //    }
            //}
        }
        /****************************************************************************************/
        private void gridMain6_CellValueChangedx(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            DataTable dt = (DataTable)dgv6.DataSource;
            DataRow dr = gridMain6.GetFocusedDataRow();
            dr["mod"] = "Y";
            otherModified = true;
            btnSaveAll.Show();
            GridColumn currCol = gridMain6.FocusedColumn;
            currentColumn = currCol.FieldName;

            string what = dr["data"].ObjToString();
            if (String.IsNullOrWhiteSpace(what))
                return;

            string tract = dr["tracking"].ObjToString().ToUpper();
            string reference = dr["reference"].ObjToString();
            if (gridMain6.Columns[currentColumn].ColumnEdit != null || tract == "T")
            {
                string answers = "";
                bool found = false;
                for (int i = 0; i < ciLookup.Items.Count; i++)
                {
                    answers = ciLookup.Items[i].ObjToString();
                    if (what.Trim().ToUpper() == answers.Trim().ToUpper())
                    {
                        found = true;
                    }
                }
                if (!found)
                {
                    dr["add"] = "+";
                    dr["edit"] = "E";
                }
                else
                {
                    dr["add"] = "";
                    dr["edit"] = "";
                }
                gridMain6.Columns[currentColumn].ColumnEdit = null;
            }
            else
            {
                if (reference.IndexOf("~") < 0)
                    return;
                string[] Lines = reference.Split('~');
                if (Lines.Length >= 2)
                {
                    try
                    {
                        string majorField = Lines[0].Trim();
                        string field = Lines[1].Trim();
                        DataRow[] dR = dt.Select("dbField = '" + majorField + "'");
                        if (dR.Length > 0)
                        {
                            string answer = dR[0]["data"].ObjToString();
                            dR = trackDt.Select("tracking='" + majorField + "' AND answer='" + answer + "' AND location='" + EditCust.activeFuneralHomeName + "'");
                            if (dR.Length > 0)
                            {
                                string record = dR[0]["record"].ObjToString();
                                dR[0][field] = what;
                                G1.update_db_table("track", "record", record, new string[] { field, what });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
        }
        /****************************************************************************************/
        private void btnSaveAll_Click(object sender, EventArgs e)
        {
            FireEventSaveFunServices(true);
            customerModified = false;
            LoadCustomer();
            loading = false;
            string dod = this.txtDOD.Text;
            DateTime date = dod.ObjToDateTime();
            if (date.Year < 500)
            {
                groupBoxMailing.Show();
                lblEmail.Show();
                txtEmail.Show();
                lblDelivery.Show();
                cmbDelivery.Show();
                panelAll.Refresh();
                this.Refresh();
            }
            if (ServiceDateChanged)
                OnSomethingChanged("SRVDATE");
            ServiceDateChanged = false;
        }
        /****************************************************************************************/
        private void textEdit_patientZipCode_EditValueChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            string zipCode = textEdit_patientZipCode.Text.Trim();
            if (!String.IsNullOrWhiteSpace(zipCode))
            {
                string city = "";
                string state = "";
                string county = "";
                bool rv = FunFamily.LookupZipcode(zipCode, ref city, ref state, ref county);
                if (rv)
                {
                    if (!String.IsNullOrWhiteSpace(state))
                    {
                        string cmd = "Select * from `ref_states` where `state` = '" + state + "';";
                        DataTable dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count > 0)
                            state = dx.Rows[0]["abbrev"].ObjToString();
                    }
                    if (!String.IsNullOrWhiteSpace(city))
                        textEdit_patientCity.Text = city;
                    if (!String.IsNullOrWhiteSpace(state))
                        comboStates.Text = state;
                    if (!String.IsNullOrWhiteSpace(county))
                    {
                        ChangeVitalsField("deccounty", county);
                        txtCounty.Text = county;
                    }
                }
            }
        }
        /****************************************************************************************/
        private void ChangeVitalsField(string field, string answer)
        {
            DataTable dt = (DataTable)dgv6.DataSource;
            if (dt == null)
                return;
            DataRow[] dR = dt.Select("field='" + field + "'");
            if (dR.Length > 0)
            {
                dR[0]["data"] = answer;
                dR[0]["mod"] = "Y";
                otherModified = true;
            }
        }
        /****************************************************************************************/
        private string currentColumn = "";
        private void gridMain6_MouseDown(object sender, MouseEventArgs e)
        {
            var hitInfo = gridMain6.CalcHitInfo(e.Location);
            if (hitInfo.InRowCell)
            {
                int rowHandle = hitInfo.RowHandle;
                dgv6.RefreshDataSource();

                DataTable dt = (DataTable)dgv6.DataSource;

                string field = dt.Rows[rowHandle]["dbfield"].ObjToString();
                if (field.Trim().ToUpper() == "FIRSTCALLDETAIL")
                {
                    string fstr = dt.Rows[rowHandle]["data"].ObjToString();
                    using (FirstCall firstForm = new FirstCall(firstCallInformation, firstCallFamily))
                    {
                        firstForm.ShowDialog();
                        if (firstForm.DialogResult == System.Windows.Forms.DialogResult.OK)
                        {
                            firstCallFamily = false;
                            fstr = firstForm.myTextAnswer;
                            firstCallInformation = fstr;
                            if (firstForm.myCheckFamily)
                                firstCallFamily = true;
                            otherModified = true;
                            btnSaveAll.Show();
                            btnSaveAll.Refresh();
                        }
                    }
                    return;
                }

                GridColumn column = hitInfo.Column;
                currentColumn = column.FieldName.Trim();
                string data = dt.Rows[rowHandle][currentColumn].ObjToString();
                if (data.Trim() == "+")
                {
                    data = dt.Rows[rowHandle]["data"].ObjToString();
                    if (String.IsNullOrWhiteSpace(data))
                        return;
                    DataRow[] dR = trackingDt.Select("tracking='" + field + "'");
                    if (dR.Length > 0)
                    {
                        string u = dR[0]["using"].ObjToString();
                        if (!String.IsNullOrWhiteSpace(u))
                            field = u;
                    }
                    dR = trackDt.Select("tracking='" + field + "' AND answer='" + data + "' AND location='" + EditCust.activeFuneralHomeName + "'");
                    if (dR.Length <= 0)
                    {
                        if (workDt6 == null)
                            return;
                        string record = G1.create_record("track", "answer", "-1");
                        if (G1.BadRecord("track", record))
                            return;
                        G1.update_db_table("track", "record", record, new string[] { "tracking", field, "answer", data, "location", EditCust.activeFuneralHomeName });
                        string trackField = dt.Rows[rowHandle]["field"].ObjToString();
                        dR = workDt6.Select("tracking='" + trackField + "' and answer = '" + data + "' and location = '" + EditCust.activeFuneralHomeName + "'");
                        DataRow dRow = trackDt.NewRow();
                        dRow["tracking"] = field;
                        dRow["answer"] = data;
                        dRow["location"] = EditCust.activeFuneralHomeName;
                        dRow["record"] = record;
                        if (dR.Length > 0)
                        {
                            dRow["address"] = dR[0]["address"].ObjToString();
                            dRow["city"] = dR[0]["city"].ObjToString();
                            dRow["county"] = dR[0]["county"].ObjToString();
                            dRow["state"] = dR[0]["state"].ObjToString();
                            dRow["zip"] = dR[0]["zip"].ObjToString();
                            dRow["phone"] = dR[0]["phone"].ObjToString();
                            G1.update_db_table("track", "record", record, new string[] { "address", dR[0]["address"].ObjToString(), "city", dR[0]["city"].ObjToString(), "county", dR[0]["county"].ObjToString(), "state", dR[0]["state"].ObjToString(), "zip", dR[0]["zip"].ObjToString(), "phone", dR[0]["phone"].ObjToString() });
                        }
                        trackDt.Rows.Add(dRow);

                        //DataRow dRow = trackDt.NewRow();
                        //dRow["tracking"] = field;
                        //dRow["answer"] = data;
                        //dRow["location"] = EditCust.activeFuneralHomeName;
                        //dRow["record"] = record;
                        //trackDt.Rows.Add(dRow);
                        lastDb = "";


                        field = dt.Rows[rowHandle]["dbfield"].ObjToString();
                        dR = trackingDt.Select("tracking='" + field + "'");
                        if (dR.Length > 0)
                        {
                            string u = dR[0]["using"].ObjToString();
                            if (!String.IsNullOrWhiteSpace(u))
                                field = u;
                        }
                        EditTracking trackForm = new EditTracking(field, EditCust.activeFuneralHomeName, record);
                        trackForm.ShowDialog();
                        trackDt = G1.get_db_data("Select * from `track`");
                        string cmd = "Select * from `track` WHERE `record` = '" + record + "';";
                        DataTable dx = G1.get_db_data(cmd);
                        gridMain6_ShownEditor(null, null);
                        string what = data;
                        CiLookup_DataChanged(what);
                        string newAnswer = what;
                        if (!String.IsNullOrWhiteSpace(newAnswer))
                        {
                            dt.Rows[rowHandle]["data"] = newAnswer;
                            cmd = "Select * from `track` where `tracking` = '" + field + "' and `location` = '" + EditCust.activeFuneralHomeName + "' AND `answer` = '" + newAnswer + "';";
                            dx = G1.get_db_data(cmd);
                            if (dx.Rows.Count <= 0)
                            {
                                dt.Rows[rowHandle]["add"] = "+";
                                dt.Rows[rowHandle]["edit"] = "E";
                            }
                            dgv6.RefreshDataSource();
                        }
                    }
                    else
                        dt.Rows[rowHandle][currentColumn] = "";
                    dgv6.RefreshDataSource();
                }
                else if (data.Trim() == "E")
                {
                    gridMain6_DoubleClick(null, null);
                    //field = dt.Rows[rowHandle]["dbfield"].ObjToString();
                    //DataRow[] dR = trackingDt.Select("tracking='" + field + "'");
                    //if (dR.Length > 0)
                    //{
                    //    string u = dR[0]["using"].ObjToString();
                    //    if (!String.IsNullOrWhiteSpace(u))
                    //        field = u;
                    //}
                    //EditTracking trackForm = new EditTracking(field, EditCust.activeFuneralHomeName );
                    //trackForm.ShowDialog();
                    //trackDt = G1.get_db_data("Select * from `track`");
                    //gridMain6_ShownEditor(null, null);
                    //string newAnswer = EditTracking.trackingSelection;
                    //if (!String.IsNullOrWhiteSpace(newAnswer))
                    //{
                    //    dt.Rows[rowHandle]["data"] = newAnswer;
                    //    string cmd = "Select * from `track` where `tracking` = '" + field + "' and `location` = '" + EditCust.activeFuneralHomeName + "' AND `answer` = '" + newAnswer + "';";
                    //    DataTable dx = G1.get_db_data(cmd);
                    //    if (dx.Rows.Count <= 0)
                    //    {
                    //        dt.Rows[rowHandle]["add"] = "+";
                    //        dt.Rows[rowHandle]["edit"] = "E";
                    //    }
                    //    dgv6.RefreshDataSource();
                    //}
                }
                else
                {
                }
            }
            if (funDemo != null)
            {
                if (!funDemo.IsDisposed)
                {
                    if (funDemo.Visible)
                    {
                        if (!popupForm.ListBox.Visible)
                            funDemo.Close();
                    }
                }
            }
            this.TopMost = true;
            this.Focus();
        }
        /****************************************************************************************/
        private string lastDb = "";
        private DataTable myDt = null;
        private DataTable itemDt = null;
        private string editingWhat = "";
        private bool isTracking = false;
        private void gridMain6_ShownEditor(object sender, EventArgs e)
        {
            isProtected = false;
            GridColumn currCol = gridMain6.FocusedColumn;
            currentColumn = currCol.FieldName;
            if (currentColumn.ToUpper() != "DATA")
                return;

            int focusedRow = gridMain6.FocusedRowHandle;
            int row = gridMain6.GetDataSourceRowIndex(focusedRow);

            DataRow dr = gridMain6.GetFocusedDataRow();
            string field = dr["field"].ObjToString();
            if (field == "Decedent’s Church Affiliation")
            {
            }
            editingWhat = field;

            string dbField = dr["dbfield"].ObjToString();
            string help = dr["reference"].ObjToString();
            string ddData = dr["data"].ObjToString();
            if ( String.IsNullOrWhiteSpace ( ddData ))
            {
            }

            isProtected = false;

            try
            {
                ciLookup.Items.Clear();
                if (myDt == null)
                {
                    myDt = new DataTable();
                    myDt.Columns.Add("stuff");
                }
                myDt.Rows.Clear();

                string[] Lines = null;
                string cmd = "";
                if (help.Length > 0)
                    cmd = help.Substring(0, 1);
                if (cmd == "$")
                {
                    Lines = help.Split('=');
                    if (Lines.Length < 2)
                        return;
                }

                if (field.ToUpper().Trim() == "FUNERAL DIRECTOR")
                {
                    string name = "";
                    string license = "";
                    string location = "";
                    for (int i = 0; i < directorsDt.Rows.Count; i++)
                    {
                        try
                        {
                            license = directorsDt.Rows[i]["license"].ObjToString();
                            location = directorsDt.Rows[i]["location"].ObjToString();
                            //name = directorsDt.Rows[i]["lastName"].ObjToString() + ", " + directorsDt.Rows[i]["firstName"].ObjToString() + " " + directorsDt.Rows[i]["middleName"].ObjToString() + " [" + license + "]";
                            string firstName = directorsDt.Rows[i]["firstName"].ObjToString().Trim();
                            string middleName = directorsDt.Rows[i]["middleName"].ObjToString().Trim();
                            string lastName = directorsDt.Rows[i]["lastName"].ObjToString().Trim();
                            name = firstName;
                            if (!String.IsNullOrWhiteSpace(middleName))
                                name += " " + middleName;
                            if (!String.IsNullOrWhiteSpace(lastName))
                                name += " " + lastName;
                            name = name.Trim();
                            name = name + " [" + license + "]";
                            AddToMyDt(name);
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }
                else if (field.ToUpper().Trim() == "FUNERAL ARRANGER - NAME")
                {
                    string name = "";
                    string license = "";
                    string location = "";
                    for (int i = 0; i < arrangersDt.Rows.Count; i++)
                    {
                        try
                        {
                            license = arrangersDt.Rows[i]["license"].ObjToString();
                            location = arrangersDt.Rows[i]["location"].ObjToString();
                            //                            name = arrangersDt.Rows[i]["lastName"].ObjToString() + ", " + arrangersDt.Rows[i]["firstName"].ObjToString() + " " + arrangersDt.Rows[i]["middleName"].ObjToString() + " [" + license + "]";
                            string firstName = arrangersDt.Rows[i]["firstName"].ObjToString().Trim();
                            string middleName = arrangersDt.Rows[i]["middleName"].ObjToString().Trim();
                            string lastName = arrangersDt.Rows[i]["lastName"].ObjToString().Trim();
                            name = firstName;
                            if (!String.IsNullOrWhiteSpace(middleName))
                                name += " " + middleName;
                            if (!String.IsNullOrWhiteSpace(lastName))
                                name += " " + lastName;
                            name = name.Trim();

                            name += " [" + license + "]";
                            AddToMyDt(name);
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }
                else if (!String.IsNullOrWhiteSpace(help))
                {
                    try
                    {
                        if (cmd == "$")
                        {
                            string db = Lines[0];
                            db = db.Replace("$", "");
                            cmd = "Select * from `" + db + "`;";
                            field = Lines[1];
                            if (field.ToUpper().IndexOf("/PROTECT") > 0)
                                isProtected = true;
                            field = Regex.Replace(field, "/Protect", "", RegexOptions.IgnoreCase);

                            DataTable dd = G1.get_db_data(cmd);

                            //if ( dd.Rows.Count > 0 )
                            //    itemDt = dd.Copy();

                            if (itemDt != null)
                                itemDt.Rows.Clear();

                            for (int i = 0; i < dd.Rows.Count; i++)
                                AddToMyDt(dd.Rows[i][field].ObjToString());
                        }
                        if (help.Trim().ToUpper() == "/PROTECT")
                            isProtected = true;
                        isTracking = false;

                        //DataRow[] dR = trackingDt.Select("tracking='" + dbField + "'");
                        //if (dR.Length > 0)
                        //{
                        //    string substitute = dR[0]["using"].ObjToString();
                        //    if (!String.IsNullOrWhiteSpace(substitute))
                        //        dbField = substitute;
                        //    if (!String.IsNullOrWhiteSpace(EditCust.activeFuneralHomeName))
                        //    {
                        //        string locations = FunFamilyNew.findLocationAssociation();
                        //        //dR = trackDt.Select("tracking='" + dbField + "' AND ( location='" + EditCust.activeFuneralHomeName + "' ) ");
                        //        dR = trackDt.Select("tracking='" + dbField + "' AND (" + locations + ")");
                        //    }
                        //    else
                        //        dR = trackDt.Select("tracking='" + dbField + "'");
                        //    if (dR.Length > 0)
                        //        itemDt = dR.CopyToDataTable();
                        //    //for (int i = 0; i < dR.Length; i++)
                        //    //    AddToMyDt(dR[i]["answer"].ObjToString());
                        //    dR = trackDt.Select("tracking='" + dbField + "' AND ( location='All' ) ");
                        //    if (dR.Length > 0)
                        //    {
                        //        DataTable mergeDt = dR.CopyToDataTable();
                        //        if (itemDt == null)
                        //            itemDt = mergeDt.Copy();
                        //        else
                        //            itemDt.Merge(mergeDt);
                        //        //for (int i = 0; i < dR.Length; i++)
                        //        //    AddToMyDt(dR[i]["answer"].ObjToString());
                        //    }
                        //}
                    }
                    catch (Exception ex)
                    {
                    }
                }
                else
                {
                    if (help.Trim().ToUpper() == "/PROTECT")
                        isProtected = true;
                    DataRow[] dR = trackingDt.Select("tracking='" + dbField + "'");
                    if (dR.Length > 0)
                    {
                        string substitute = dR[0]["using"].ObjToString();
                        if (!String.IsNullOrWhiteSpace(substitute))
                            dbField = substitute;
                        if (!String.IsNullOrWhiteSpace(EditCust.activeFuneralHomeName))
                        {
                            string locations = FunFamilyNew.findLocationAssociation();
                            //dR = trackDt.Select("tracking='" + dbField + "' AND ( location='" + EditCust.activeFuneralHomeName + "' ) ");
                            dR = trackDt.Select("tracking='" + dbField + "' AND (" + locations + ")");
                        }
                        else
                            dR = trackDt.Select("tracking='" + dbField + "'");
                        isTracking = true;
                        if (dR.Length > 0)
                            itemDt = dR.CopyToDataTable();
                        for (int i = 0; i < dR.Length; i++)
                            AddToMyDt(dR[i]["answer"].ObjToString());
                        dR = trackDt.Select("tracking='" + dbField + "' AND ( location='All' ) ");
                        if (dR.Length > 0)
                        {
                            DataTable mergeDt = dR.CopyToDataTable();
                            if (itemDt == null)
                                itemDt = mergeDt.Copy();
                            else
                                itemDt.Merge(mergeDt);
                            for (int i = 0; i < dR.Length; i++)
                                AddToMyDt(dR[i]["answer"].ObjToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }

            if (myDt.Rows.Count <= 0)
            {
                gridMain6.Columns["data"].ColumnEdit = null;
                string type = dr["type"].ObjToString();
                if (type.ToUpper() == "DATE" || type.ToUpper() == "DAY" || type.ToUpper() == "FULLDATE")
                {
                    DataTable dt = (DataTable)dgv6.DataSource;

                    string str = dt.Rows[row]["data"].ObjToString();
                    DateTime myDate = DateTime.Now;
                    if (!String.IsNullOrWhiteSpace(str))
                        myDate = str.ObjToDateTime();
                    string title = dt.Rows[row]["field"].ObjToString();
                    using (GetDate dateForm = new GetDate(myDate, title))
                    {
                        dateForm.ShowDialog();
                        if (dateForm.DialogResult == System.Windows.Forms.DialogResult.OK)
                        {
                            myDate = dateForm.myDateAnswer;
                            if (type.ToUpper() == "DAY")
                                dt.Rows[row]["data"] = G1.DayOfWeekText(myDate);
                            else if (type == "FULLDATE")
                                dt.Rows[row]["data"] = myDate.ToString("MMMM d, yyyy");
                            else
                                dt.Rows[row]["data"] = myDate.ToString("MM/dd/yyyy");
                            dt.Rows[row]["mod"] = "Y";

                            string dateField = dt.Rows[row]["field"].ObjToString();
                            dateField = dateField.ToUpper().Replace("DATE", "Day");
                            DataRow[] ddR = dt.Select("field='" + dateField + "'");
                            if (ddR.Length <= 0)
                            {
                                dateField = dateField.ToUpper().Replace("DAY", "DayDate");
                                ddR = dt.Select("field='" + dateField + "'");
                            }
                            if (ddR.Length > 0)
                            {
                                type = ddR[0]["type"].ObjToString();
                                if (type.ToUpper() == "DAY")
                                {
                                    ddR[0]["data"] = G1.DayOfWeekText(myDate);
                                    ddR[0]["mod"] = "Y";
                                }
                            }

                            gridMain6.RefreshData();
                            gridMain6.RefreshEditor(true);
                            funModified = true;
                            otherModified = true;
                            btnSaveAll.Show();
                            btnSaveAll.Refresh();
                        }
                    }
                }
                gridMain6.RefreshData();
                gridMain6.RefreshEditor(true);
            }
            else
            {
                if (lastDb != dbField)
                {
                    lastDb = dbField;
                    if (gridMain6.Columns["data"].ColumnEdit != null)
                    {
                        gridMain6.Columns["data"].ColumnEdit = null;
                    }
                    //textBox1.Text = "Junk";
                    if ( funDemo != null )
                    {
                        if (funDemo.IsDisposed)
                        {
                            string currentData = dr["data"].ObjToString();
                            BringFunDemoUp( currentData );
                        }
                        else if (!funDemo.Visible)
                        {
                            string currentData = dr["data"].ObjToString();
                            //funDemo.FireEventFunDemoLoad("Place", editingWhat, "", "", "", "", "", currentData.Trim(), "", "", "", "", "", "", "");
                            //if (isTracking)
                            //{
                            //    funDemo.Visible = true;
                            //    funDemo.Show();
                            //    funDemo.Refresh();
                            //}
                        }
                    }
                }

                DataView tempview = myDt.DefaultView;
                tempview.Sort = "stuff asc";
                myDt = tempview.ToTable();

                myDt = FunFamilyNew.RemoveDuplicates(myDt, "stuff");

                for (int i = 0; i < myDt.Rows.Count; i++)
                    ciLookup.Items.Add(myDt.Rows[i]["stuff"].ObjToString());

                gridMain6.Columns["data"].ColumnEdit = ciLookup;
                gridMain6.RefreshData();
                gridMain6.RefreshEditor(true);
            }
        }
        /****************************************************************************************/
        //private DevExpress.XtraEditors.Popup.ComboBoxPopupListBoxForm popupForm = null;
        private PopupListBoxForm popupForm = null;
        /***************************************************************************************/
        private void CiLookup_Popup(object sender, EventArgs e)
        {
            popupForm = (sender as IPopupControl).PopupWindow as PopupListBoxForm;
            popupForm.ListBox.MouseMove += ListBox_MouseMove;
            popupForm.ListBox.MouseDown += ListBox_MouseDown;
            popupForm.ListBox.SelectedValueChanged += ListBox_SelectedValueChanged;
        }
        /****************************************************************************************/
        private void ListBox_SelectedValueChanged(object sender, EventArgs e)
        {
            //if (1 == 1)
            //    return;
            DataRow dr = gridMain6.GetFocusedDataRow();
            string item = dr["data"].ObjToString();
            item = popupForm.ListBox.SelectedValue.ObjToString();
            //dr["data"] = item;
            //gridMain6.RefreshData();
            //gridMain6.RefreshEditor(true);
            if (isTracking)
            {
                textBox1.Text = item;
                textBox1.Refresh();
            }
            int index = popupForm.ListBox.SelectedIndex;
            if (index == lastIndex)
                return;
            if (1 == 1)
                return;

            whichRowChanged = gridMain6.FocusedRowHandle;

            string columnName = gridMain6.FocusedColumn.FieldName.ToUpper();


            lastIndex = index;

            string answer = item;
            string address = "";
            string city = "";
            string county = "";
            string state = "";
            string zip = "";
            string phone = "";
            string location = "";

            try
            {
                DataRow[] dRows = null;
                dRows = itemDt.Select("answer='" + item + "'");
                if (dRows.Length > 0)
                {
                    location = dRows[0]["location"].ObjToString();
                    address = dRows[0]["address"].ObjToString();
                    city = dRows[0]["city"].ObjToString();
                    county = dRows[0]["county"].ObjToString();
                    state = dRows[0]["state"].ObjToString();
                    zip = dRows[0]["zip"].ObjToString();
                    phone = dRows[0]["phone"].ObjToString();
                }
                else
                {
                }
            }
            catch (Exception ex)
            {
            }
            if (funDemo == null)
            {
                funDemo = new FuneralDemo("Place", "", "", "", "", "", "", "", "", "", "", "");
                funDemo.FunDemoDone += FunDemo_FunDemoDone;
                Rectangle rect = funDemo.Bounds;
                int top = rect.Y;
                int left = rect.X;
                int height = rect.Height;
                int width = rect.Width;
                top = this.Bounds.Y;
                left = this.Bounds.Width - width;
                funDemo.StartPosition = FormStartPosition.Manual;
                funDemo.SetBounds(left, top, width, height);

                funDemo.Show();
            }

            if (funDemo != null)
            {
                try
                {
                    if ( String.IsNullOrWhiteSpace ( address ))
                    {
                    }
                    funDemo.FireEventFunDemoLoad("Place", editingWhat, "", "", "", "", "", item, address, city, county, state, zip, phone, location);
                    funDemo.TopMost = true;
                    if (!funDemo.Visible && !funDemo.IsDisposed)
                    {
                        funDemo.Visible = true;
                        funDemo.Refresh();
                    }
                    dr = gridMain6.GetFocusedDataRow();
                    if (dr != null)
                    {
                        dr["data"] = item;
                        gridMain6.RefreshData();
                        gridMain6.RefreshEditor(true);
                    }

                    gridMain6.Focus();
                    popupForm.ListBox.Focus();
                }
                catch (Exception ex)
                {
                    if (funDemo.IsDisposed)
                    {
                        funDemo = new FuneralDemo("Place", "", "", "", "", "", "", "", "", "", "", "");
                        funDemo.FunDemoDone += FunDemo_FunDemoDone;
                        funDemo.Show();
                        funDemo.Hide();
                    }
                }
            }
            popupForm.ListBox.Show();
            //cmb.ShowPopup();
        }
    /****************************************************************************************/
        private void ListBox_MouseDown(object sender, MouseEventArgs e)
        {
            DevExpress.XtraEditors.Popup.ComboBoxPopupListBox box = (DevExpress.XtraEditors.Popup.ComboBoxPopupListBox)sender;
            int selectedRowIndex = box.SelectedIndex;
            if (funDemo != null)
            {
                if (!funDemo.IsDisposed)
                {
                    if (!funDemo.Visible)
                        return;
                    if (funDemo.Visible)
                    {
                        funDemo.Hide();
                        popupForm.Close();
                        string item = textBox1.Text.Trim();
                        DataRow dr = gridMain6.GetFocusedDataRow();
                        dr = gridMain6.GetDataRow(whichRowChanged);
                        //dr["data"] = item;
                        //dr["mod"] = "Y";
                        gridMain6.RefreshData();
                        gridMain6.RefreshEditor(true);
                        this.Focus();
                        this.TopMost = true;

                        DataTable tempDt = dr.Table.Copy();
                        string dbField = dr["dbField"].ObjToString();

                        DataTable dx = (DataTable)dgv6.DataSource;
                        DataRow[] dRows = null;
                        dRows = dx.Select("dbfield='" + dbField + "'");
                        if ( dRows.Length > 0 )
                        {
                            dRows[0]["data"] = item;
                            dRows[0]["mod"] = "Y";
                            gridMain6.RefreshData();
                        }
                        dRows = itemDt.Select("answer='" + item + "' and location='" + EditCust.activeFuneralHomeName + "'");
                        if ( dRows.Length <= 0 )
                            dRows = itemDt.Select("answer='" + item + "'");
                        if (dRows.Length > 0)
                        {
                            tempDt = dRows.CopyToDataTable();
                            string location = dRows[0]["location"].ObjToString();
                            string address = dRows[0]["address"].ObjToString();
                            string city = dRows[0]["city"].ObjToString();
                            string county = dRows[0]["county"].ObjToString();
                            string state = dRows[0]["state"].ObjToString();
                            string zip = dRows[0]["zip"].ObjToString();
                            string phone = dRows[0]["phone"].ObjToString();

                            string reference = "";

                            dRows = dx.Select("reference LIKE '" + dbField + "~%'");
                            if (dRows.Length > 0 )
                            {
                                for ( int i=0; i<dRows.Length; i++)
                                {
                                    reference = dRows[i]["reference"].ObjToString();
                                    if (reference.ToUpper().IndexOf("ADDRESS") > 0)
                                    {
                                        dRows[i]["data"] = address;
                                        dRows[i]["mod"] = "Y";
                                    }
                                    else if (reference.ToUpper().IndexOf("CITY") > 0)
                                    {
                                        dRows[i]["data"] = city;
                                        dRows[i]["mod"] = "Y";
                                    }
                                    else if (reference.ToUpper().IndexOf("COUNTY") > 0)
                                    {
                                        dRows[i]["data"] = county;
                                        dRows[i]["mod"] = "Y";
                                    }
                                    else if (reference.ToUpper().IndexOf("STATE") > 0)
                                    {
                                        dRows[i]["data"] = state;
                                        dRows[i]["mod"] = "Y";
                                    }
                                    else if (reference.ToUpper().IndexOf("ZIP") > 0)
                                    {
                                        dRows[i]["data"] = zip;
                                        dRows[i]["mod"] = "Y";
                                    }
                                    else if (reference.ToUpper().IndexOf("PHONE") > 0)
                                    {
                                        dRows[i]["data"] = phone;
                                        dRows[i]["mod"] = "Y";
                                    }

                                }
                            }
                            gridMain6.RefreshData();
                            gridMain6.RefreshEditor(true);
                        }
                        funModified = true;
                        otherModified = true;
                        btnSaveAll.Show();
                        btnSaveAll.Refresh();
                    }
                }
            }
        }
        /****************************************************************************************/
        private int lastIndex = -1;
        private int whichRowChanged = -1;
        private FuneralDemo funDemo = null;
        private void ListBox_MouseMove(object sender, MouseEventArgs e)
        {
            PopupListBox listBoxControl = sender as PopupListBox;
            ComboBoxEdit cmb = listBoxControl.OwnerEdit as ComboBoxEdit;
            int index = listBoxControl.IndexFromPoint(new Point(e.X, e.Y));
            if (index < 0)
            {
                if ( e.Y > listBoxControl.Height )
                {
                }
            }
            else
            {
                if (index == lastIndex)
                    return;

                whichRowChanged = gridMain6.FocusedRowHandle;
                DataRow dr = gridMain6.GetFocusedDataRow();

                string columnName = gridMain6.FocusedColumn.FieldName.ToUpper();


                string item = cmb.Properties.Items[index].ToString();
                if (isTracking)
                {
                    textBox1.Text = item;
                    textBox1.Refresh();
                }
                lastIndex = index;

                //dr["data"] = item; // ramma zamma

                //gridMain6.RefreshData();
                //gridMain6.RefreshEditor(true);
                //popupForm.ListBox.Refresh();
                popupForm.ListBox.Focus();

                if (1 == 1)
                {
                    cmb.ShowPopup();
                    if ( funDemo != null && !funDemo.IsDisposed )
                    {
                        if ( !funDemo.Visible)
                        {
                            if (isTracking)
                            {
                                funDemo.Visible = true;
                                funDemo.Show();
                            }
                        }
                    }
                    return;
                }

                string answer = item;
                string address = "";
                string city = "";
                string county = "";
                string state = "";
                string zip = "";
                string phone = "";
                string location = "";

                try
                {
                    DataRow[] dRows = null;
                    dRows = itemDt.Select("answer='" + item + "'");
                    if (dRows.Length > 0)
                    {
                        location = dRows[0]["location"].ObjToString();
                        address = dRows[0]["address"].ObjToString();
                        city = dRows[0]["city"].ObjToString();
                        county = dRows[0]["county"].ObjToString();
                        state = dRows[0]["state"].ObjToString();
                        zip = dRows[0]["zip"].ObjToString();
                        phone = dRows[0]["phone"].ObjToString();
                    }
                }
                catch (Exception ex)
                {
                }
                if ( funDemo == null )
                {
                    funDemo = new FuneralDemo("Place", "", "", "", "", "", "", "", "", "", "", "");
                    funDemo.FunDemoDone += FunDemo_FunDemoDone;
                    Rectangle rect = funDemo.Bounds;
                    int top = rect.Y;
                    int left = rect.X;
                    int height = rect.Height;
                    int width = rect.Width;
                    top = this.Bounds.Y;
                    left = this.Bounds.Width - width;
                    funDemo.StartPosition = FormStartPosition.Manual;
                    funDemo.SetBounds(left, top, width, height);

                    funDemo.Show();
                }

                if (funDemo != null)
                {
                    try
                    {
                        funDemo.FireEventFunDemoLoad("Place", editingWhat, "", "", "", "", "", item, address, city, county, state, zip, phone, location);
                        funDemo.TopMost = true;
                        if (!funDemo.Visible && !funDemo.IsDisposed )
                        {
                            funDemo.Visible = true;
                            funDemo.Refresh();
                        }
                        dr = gridMain6.GetFocusedDataRow();
                        if (dr != null)
                        {
                            dr["data"] = item;
                            //gridMain6.RefreshData();
                            //gridMain6.RefreshEditor(true);
                        }

                        //gridMain6.Focus();
                        popupForm.ListBox.Focus();
                        //this.Focus();
                    }
                    catch (Exception ex)
                    {
                        if (funDemo.IsDisposed)
                        {
                            funDemo = new FuneralDemo("Place", "", "", "", "", "", "", "", "", "", "", "");
                            funDemo.FunDemoDone += FunDemo_FunDemoDone;
                            funDemo.Show();
                            funDemo.Hide();
                        }
                    }
                }
                cmb.ShowPopup();
            }
        }
        /****************************************************************************************/
        private void FunDemo_FunDemoDone(string title, string firstName, string middleName, string lastName, string suffix, string name, string address, string city, string county, string state, string zip, string phone)
        {
            DataRow dr = gridMain6.GetFocusedDataRow();
            if (dr == null)
                return;
            if (whichRowChanged < 0)
                return;

            int rowHandle = whichRowChanged;
            //if (rowHandle >= 0)
            //    dr = gridMain6.GetDataRow(rowHandle);

            int row = gridMain6.GetFocusedDataSourceRowIndex();

            DataTable tempDt = dr.Table;

            string dbField = dr["dbField"].ObjToString();

            //DataTable dt = (DataTable)dgv6.DataSource;

            if (!String.IsNullOrWhiteSpace(name))
            {
                dr["data"] = name;
                dr["mod"] = "Y";
            }

            DataTable dx = (DataTable)dgv6.DataSource;
            DataRow[] dRows = null;
            string reference = "";

            dRows = dx.Select("reference LIKE '" + dbField + "~%'");
            if (dRows.Length > 0)
            {
                for (int i = 0; i < dRows.Length; i++)
                {
                    reference = dRows[i]["reference"].ObjToString();
                    if (reference.ToUpper().IndexOf("ADDRESS") > 0)
                    {
                        dRows[i]["data"] = address;
                        dRows[i]["mod"] = "Y";
                    }
                    else if (reference.ToUpper().IndexOf("CITY") > 0)
                    {
                        dRows[i]["data"] = city;
                        dRows[i]["mod"] = "Y";
                    }
                    else if (reference.ToUpper().IndexOf("COUNTY") > 0)
                    {
                        dRows[i]["data"] = county;
                        dRows[i]["mod"] = "Y";
                    }
                    else if (reference.ToUpper().IndexOf("STATE") > 0)
                    {
                        dRows[i]["data"] = state;
                        dRows[i]["mod"] = "Y";
                    }
                    else if (reference.ToUpper().IndexOf("ZIP") > 0)
                    {
                        dRows[i]["data"] = zip;
                        dRows[i]["mod"] = "Y";
                    }
                    else if (reference.ToUpper().IndexOf("PHONE") > 0)
                    {
                        dRows[i]["data"] = phone;
                        dRows[i]["mod"] = "Y";
                    }
                }
            }
            //dr["depPrefix"] = title;
            //dr["depFirstName"] = firstName;
            //dr["depMI"] = middleName;
            //dr["depLastName"] = lastName;
            //dr["depSuffix"] = suffix;
            //dr["address"] = address;
            //dr["city"] = city;
            //dr["county"] = county;
            //dr["state"] = state;
            //dr["zip"] = zip;
            //dr["phone"] = phone;
            //dr["mod"] = "Y";

            funModified = true;
            otherModified = true;
            btnSaveAll.Show();
            btnSaveAll.Refresh();
            dgv6.Refresh();

            funDemo.Hide();
        }
        /****************************************************************************************/
        private void AddToMyDt(string data)
        {
            if (String.IsNullOrWhiteSpace(data))
                return;
            if (myDt == null)
            {
                myDt = new DataTable();
                myDt.Columns.Add("stuff");
            }
            DataRow dRow = myDt.NewRow();
            dRow["stuff"] = data;
            myDt.Rows.Add(dRow);
        }
        /****************************************************************************************/
        private void gridMain6_DoubleClick(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv6.DataSource;
            DataRow dr = gridMain6.GetFocusedDataRow();
            int rowHandle = gridMain6.FocusedRowHandle;
            GridColumn column = gridMain6.FocusedColumn;
            string columnName = column.FieldName.ObjToString();
            //if (columnName.ToUpper() == "DATA")
            //    return;
            string field = dr["dbfield"].ObjToString();
            if (String.IsNullOrWhiteSpace(field))
                return;
            string cmd = "Select * from `tracking` where `tracking` = '" + field + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
                return;
            string trackUsing = dx.Rows[0]["using"].ObjToString();
            if (!String.IsNullOrWhiteSpace(trackUsing))
                field = trackUsing;
            EditTracking trackForm = new EditTracking(field, EditCust.activeFuneralHomeName);
            trackForm.ShowDialog();
            trackDt = G1.get_db_data("Select * from `track`");
            gridMain6_ShownEditor(null, null);
            string newAnswer = EditTracking.trackingSelection;
            if (!String.IsNullOrWhiteSpace(newAnswer))
            {
                string dbField = dr["dbField"].ObjToString().Trim();
                dt.Rows[rowHandle]["data"] = newAnswer;
                dt.Rows[rowHandle]["add"] = "";
                cmd = "Select * from `track` where `tracking` = '" + field + "' and `location` = '" + EditCust.activeFuneralHomeName + "' AND `answer` = '" + newAnswer + "';";
                dx = G1.get_db_data(cmd);
                //if (dx.Rows.Count <= 0)
                //{
                //    dt.Rows[rowHandle]["add"] = "+";
                //    dt.Rows[rowHandle]["edit"] = "E";
                //}
                dt.Rows[rowHandle]["mod"] = "Y";
                otherModified = true;
                if (!String.IsNullOrWhiteSpace(dbField))
                {
                    try
                    {
                        DataRow[] dR = trackDt.Select("tracking='" + dbField + "' AND answer='" + newAnswer + "' AND location='" + EditCust.activeFuneralHomeName + "'");
                        if (dR.Length > 0)
                        {
                            DataRow[] dRows = dt.Select("reference LIKE '" + dbField + "~%'");
                            if (dRows.Length > 0)
                            {
                                string reference = "";
                                for (int i = 0; i < dRows.Length; i++)
                                {
                                    reference = dRows[i]["reference"].ObjToString();
                                    string answer = ProcessReference(dR, reference);
                                    dRows[i]["data"] = answer;
                                    dRows[i]["mod"] = "Y";

                                    //reference = dRows[i]["reference"].ObjToString();
                                    //reference = reference.Replace(dbField + "~", "");
                                    //dRows[i]["data"] = dR[0][reference].ObjToString();
                                    //dRows[i]["mod"] = "Y";
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
                dgv6.RefreshDataSource();
            }
        }
        /****************************************************************************************/
        private void txtServiceDate_Enter(object sender, EventArgs e)
        {
            string date = txtServiceDate.Text;
            if (G1.validate_date(date))
            {
                DateTime ddate = date.ObjToDateTime();
                txtServiceDate.Text = ddate.ToString("MM/dd/yyyy");
            }
            else
                txtServiceDate.Text = "";
        }
        /****************************************************************************************/
        private bool ServiceDateChanged = false;
        private void txtServiceDate_Leave(object sender, EventArgs e)
        {
            string date = txtServiceDate.Text;
            if (String.IsNullOrWhiteSpace(date))
                return;
            if (G1.validate_date(date))
            {
                DateTime ddate = txtServiceDate.Text.ObjToDateTime();
                if (ddate.Year < 1800)
                {
                    MessageBox.Show("***ERROR*** Date Entered Year is less than 1800!", "Date Problem Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    return;
                }
                if (ddate.Year > 100)
                {
                    txtServiceDate.Text = ddate.ToString("MM/dd/yyyy");
                    ServiceDateChanged = true;
                }
            }
            else
            {
                MessageBox.Show("***ERROR*** Invalid Date!", "Date Problem Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            }
        }
        /****************************************************************************************/
        private void txtServiceDate_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                txtServiceDate_Leave(sender, e);
            else if (e.KeyCode == Keys.Tab)
                txtServiceDate_Leave(sender, e);
        }
        /****************************************************************************************/
        public static string FixSSN(string ssn)
        {
            string originalSSN = ssn;
            ssn = ssn.Replace("-", "");
            if (ssn.Length != 9)
                return originalSSN;
            string newSSN = ssn.Substring(0, 3);
            newSSN += "-";
            newSSN += ssn.Substring(3, 2);
            newSSN += "-";
            newSSN += ssn.Substring(5);
            return newSSN;
        }
        /****************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            string ssn = txtSSN.Text.Trim();
            if (String.IsNullOrWhiteSpace(filteredSSN))
            {
                unfilteredSSN = ssn;
                filteredSSN = codeFilteredSSN(ssn);
            }
            if (ssn == filteredSSN)
                txtSSN.Text = unfilteredSSN;
            else
                txtSSN.Text = filteredSSN;
            txtSSN.Refresh();
        }
        /****************************************************************************************/
        private void txtSSN_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string trimmedSSN = txtSSN.Text.Trim().Replace("-", "");
                if (trimmedSSN.Length >= 9)
                {
                    unfilteredSSN = txtSSN.Text;
                    filteredSSN = "XXX-XX-";
                    filteredSSN += trimmedSSN.Substring(5);
                    txtSSN.Text = filteredSSN;
                }
            }
        }
        /****************************************************************************************/
        private string codeFilteredSSN(string ssn)
        {
            string FilteredSSN = "";
            string trimmedSSN = ssn.Trim().Replace("-", "");
            if (trimmedSSN.Length >= 9)
            {
                //unfilteredSSN = txtSSN.Text;
                FilteredSSN = "XXX-XX-";
                FilteredSSN += trimmedSSN.Substring(5);
            }
            return FilteredSSN;
        }
        /****************************************************************************************/
        private void txtServiceId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string serviceId = txtServiceId.Text.Trim();
                if (NewContract.CheckServiceIdExists(serviceId, workContract))
                    MessageBox.Show("***ERROR*** A Service ID of " + serviceId + " Already Exists Somewhere!", "Service ID EXISTS Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            }
        }
        /****************************************************************************************/
        private void txtArrangementDate_Enter(object sender, EventArgs e)
        {
            string date = txtArrangementDate.Text;
            if (G1.validate_date(date))
            {
                DateTime ddate = date.ObjToDateTime();
                txtArrangementDate.Text = ddate.ToString("MM/dd/yyyy");
            }
            else
                txtArrangementDate.Text = "";
        }
        /****************************************************************************************/
        private void txtArrangementDate_Leave(object sender, EventArgs e)
        {
            string date = txtArrangementDate.Text;
            if (String.IsNullOrWhiteSpace(date))
                return;
            if (G1.validate_date(date))
            {
                DateTime ddate = txtArrangementDate.Text.ObjToDateTime();
                if (ddate.Year < 1800)
                {
                    MessageBox.Show("***ERROR*** Date Entered Year is less than 1800!", "Date Problem Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    return;
                }
                if (ddate.Year > 100)
                    txtArrangementDate.Text = ddate.ToString("MM/dd/yyyy");
            }
            else
            {
                MessageBox.Show("***ERROR*** Invalid Date!", "Date Problem Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            }
        }
        /****************************************************************************************/
        private void txtArrangementDate_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                txtArrangementDate_Leave(sender, e);
            else if (e.KeyCode == Keys.Tab)
                txtArrangementDate_Leave(sender, e);
        }
        /****************************************************************************************/
        private string oldWhat = "";
        private int oldWhatRow = -1;
        private void gridMain6_ValidatingEditor(object sender, DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventArgs e)
        {
            GridView view = sender as GridView;
            if (view.FocusedColumn.FieldName.ToUpper() == "DATA")
            {
                DataTable dt = (DataTable)dgv6.DataSource;
                DataRow dr = gridMain6.GetFocusedDataRow();
                int rowhandle = gridMain6.FocusedRowHandle;
                int row = gridMain6.GetDataSourceRowIndex(rowhandle);
                oldWhat = dt.Rows[row]["data"].ObjToString();
                oldWhatRow = row;
            }
        }
        /****************************************************************************************/
        private void txtFirstPayDate_Leave(object sender, EventArgs e)
        {
            string date = txtFirstPayDate.Text;
            if (String.IsNullOrWhiteSpace(date))
                return;
            if (G1.validate_date(date))
            {
                DateTime ddate = txtFirstPayDate.Text.ObjToDateTime();
                if (ddate.Year < 1800)
                {
                    MessageBox.Show("***ERROR*** Date Entered Year is less than 1800!", "Date Problem Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    return;
                }
                if (ddate.Year > 100)
                    txtFirstPayDate.Text = ddate.ToString("MM/dd/yyyy");
            }
            else
            {
                MessageBox.Show("***ERROR*** Invalid Date!", "Date Problem Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            }
        }
        /****************************************************************************************/
        private void txtFirstPayDate_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                txtFirstPayDate_Leave(sender, e);
            else if (e.KeyCode == Keys.Tab)
                txtFirstPayDate_Leave(sender, e);

        }
        /****************************************************************************************/
        private void txtFirstPayDate_Enter(object sender, EventArgs e)
        {
            string date = txtFirstPayDate.Text;
            if (G1.validate_date(date))
            {
                DateTime ddate = date.ObjToDateTime();
                txtFirstPayDate.Text = ddate.ToString("MM/dd/yyyy");
            }
            else
                txtFirstPayDate.Text = "";

        }
        /****************************************************************************************/
        private void txtSSN_Leave(object sender, EventArgs e)
        {
            string ssn = txtSSN.Text;
            if (ssn.IndexOf("XX") >= 0)
                ssn = unfilteredSSN;
            string xyxxy = txtSSN.Text;
            ssn = ssn.Replace("-", "");
            if (!String.IsNullOrWhiteSpace(ssn))
            {
                if (ssn.Length == 4)
                    ssn = "00000" + ssn;
                int rv = ValidateSSN(ssn);
                if (rv == 0)
                {
                    string contractNumber = "";
                    string cmd = "Select * from `customers` where `ssn` = '" + ssn + "';";
                    DataTable dt = G1.get_db_data(cmd);
                    if (dt.Rows.Count > 0)
                    {
                        string list = "";
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                            if (contractNumber != workContract)
                                list += contractNumber + ",";
                        }
                        list = list.TrimEnd(',');
                        if (!String.IsNullOrWhiteSpace(list))
                        {
                            if (G1.isAdminOrSuper())
                            {
                                DialogResult result = MessageBox.Show("***ERROR*** Other Contracts have the same SSN!\nDo you still want to use this SSN?\n" + list, "Changing SSN Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                                if (result == DialogResult.No)
                                {
                                    filteredSSN = codeFilteredSSN(mainSSN);
                                    unfilteredSSN = mainSSN;
                                    txtSSN.Text = filteredSSN;
                                    txtSSN.Refresh();
                                }
                            }
                            else
                            {
                                MessageBox.Show("***ERROR*** Other Contracts have the same SSN!\nPlease contact Admin to use this SSN\n" + list, "Changing SSN Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                                filteredSSN = codeFilteredSSN(mainSSN);
                                unfilteredSSN = mainSSN;
                                txtSSN.Text = filteredSSN;
                                txtSSN.Refresh();
                            }
                        }
                    }
                }
            }
        }
        /***************************************************************************************/
        public delegate void d_void_SomethingChanged(string what);
        public event d_void_SomethingChanged SomethingChanged;
        protected void OnSomethingChanged(string what)
        {
            SomethingChanged?.Invoke(what);
        }
        /***************************************************************************************/
        private void txtArrangementTime_TextChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            //somethingChanged(null, null);
        }
        /****************************************************************************************/
        private void txtArrangementTime_Leave(object sender, EventArgs e)
        {
            string time = txtArrangementTime.Text.Trim();
            if (time.IndexOf(":") < 0)
                time += ":00";
            string[] Lines = time.Split(':');
            if (Lines.Length <= 0)
                return;
            string sHour = Lines[0].Trim();
            int hour = sHour.ObjToInt32();
            if (hour < 0 || hour > 23)
            {
                txtArrangementTime.Text = "00:00";
                txtArrangementTime.Refresh();
                MessageBox.Show("***ERROR*** Invalid Hour (" + sHour + ") entered!", "Invalid Hour Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }
            string sMinute = Lines[1].Trim();
            int minute = sMinute.ObjToInt32();
            if (minute < 0 || minute > 59)
            {
                txtArrangementTime.Text = "00:00";
                txtArrangementTime.Refresh();
                MessageBox.Show("***ERROR*** Invalid Minute (" + sMinute + ") entered!", "Invalid Minute Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }
            string str = hour.ToString("D2") + ":" + minute.ToString("D2");
            txtArrangementTime.Text = str;
            txtArrangementTime.Refresh();
            somethingChanged(null, null);
        }
        /****************************************************************************************/
        private bool isValidTime ( string time )
        {
            if (time.IndexOf(":") < 0)
                time += ":00";
            string[] Lines = time.Split(':');
            if (Lines.Length <= 0)
                return false;
            string sHour = Lines[0].Trim();
            int hour = sHour.ObjToInt32();
            if (hour < 0 || hour > 23)
            {
                txtArrangementTime.Text = "00:00";
                txtArrangementTime.Refresh();
                MessageBox.Show("***ERROR*** Invalid Hour (" + sHour + ") entered!", "Invalid Hour Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return false;
            }
            string sMinute = Lines[1].Trim();
            int minute = sMinute.ObjToInt32();
            if (minute < 0 || minute > 59)
            {
                txtArrangementTime.Text = "00:00";
                txtArrangementTime.Refresh();
                MessageBox.Show("***ERROR*** Invalid Minute (" + sMinute + ") entered!", "Invalid Minute Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return false;
            }
            string str = hour.ToString("D2") + ":" + minute.ToString("D2");
            txtArrangementTime.Text = str;
            txtArrangementTime.Refresh();
            somethingChanged(null, null);
            return true;
        }
        /****************************************************************************************/
        private void FunCustomer_FormClosing(object sender, FormClosingEventArgs e)
        {
            if ( txtArrangementTime.Focused )
            {
                string time = txtArrangementTime.Text.Trim();
                if (isValidTime(time))
                    return;
                e.Cancel = true;
            }
        }
        /****************************************************************************************/
        private void gridMain6_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter || e.KeyData == Keys.Tab || e.KeyData == Keys.Up || e.KeyData == Keys.Down )
            {
                string text = textBox1.Text.Trim();
                if (!String.IsNullOrWhiteSpace(text))
                    textBox1_TextChanged(null, null);

                DataTable dt = (DataTable)dgv6.DataSource;
                int rowHandle = gridMain6.FocusedRowHandle;

                DataRow dr = gridMain6.GetFocusedDataRow();
                string ddata = dr["data"].ObjToString();
                //if (!String.IsNullOrWhiteSpace(text) && String.IsNullOrWhiteSpace(ddata))
                //    return;

                int row = gridMain6.FocusedRowHandle;
                GridColumn currCol = gridMain6.FocusedColumn;
                string currentColumn = currCol.FieldName;
                if ( e.KeyData == Keys.Up )
                    rowHandle--;
                else
                    rowHandle++;
                if (currentColumn.ToUpper() == "DATA")
                {
                    if ( rowHandle < 0 )
                    {
                        gridMain6.FocusedColumn = gridMain6.Columns["data"];
                        rowHandle = 0;
                    }
                    if (rowHandle > (dt.Rows.Count - 1))
                    {
                        gridMain6.FocusedColumn = gridMain6.Columns["data"];
                        rowHandle = 0;
                    }
                    else
                    {
                        string data = dt.Rows[row]["data"].ObjToString();
                        if ( isTracking && !String.IsNullOrWhiteSpace ( data ))
                        {
                            whichRowChanged = row;
                            textBox1.Text = data;
                            textBox1.Refresh();
                            //if (funDemo != null && funDemo.Visible)
                            if (funDemo != null )
                                funDemo.fireDemoDone();
                        }    
                    }
                }
                gridMain6.SelectRow(rowHandle);
                gridMain6.FocusedRowHandle = rowHandle;
                e.Handled = true;

                gridMain6_ShownEditor(null, null);
                textBox1.Text = "";
            }
            else
            {
                try
                {
                    e.Handled = false;
                    int row = gridMain6.FocusedRowHandle;
                    row = gridMain6.GetDataSourceRowIndex(row);
                    //oldWhatRow = row;
                    if (e.KeyData == Keys.Escape)
                    {
                        DataTable dt = (DataTable)dgv6.DataSource;
                        int rowHandle = gridMain6.FocusedRowHandle;

                        row = gridMain6.FocusedRowHandle;
                        if (row >= 0)
                        {
                            DataRow dr = gridMain6.GetFocusedDataRow();
                            string drStr = dr["data"].ObjToString();

                            string str = dt.Rows[row]["data"].ObjToString();
                            if (oldWhatRow != row)
                            {
                                oldWhatRow = row;
                                oldWhat = str;
                                if (!String.IsNullOrWhiteSpace(str))
                                {
                                    dr["data"] = str;
                                    CiLookup_SelectedIndexChangedAgain(str);
                                    textBox1.Text = "";
                                }
                            }
                            else
                            {
                                if (!String.IsNullOrWhiteSpace(oldWhat))
                                {
                                    dr["data"] = oldWhat;
                                    CiLookup_SelectedIndexChangedAgain(oldWhat);
                                    textBox1.Text = "";
                                }
                            }
                        }
                        gridMain6_ShownEditor(null, null);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("***ERROR*** Data Error.\n" + ex.Message.ToString(), "Data Entry Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                }
            }
        }
        /****************************************************************************************/
        private void changeSSNToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if ( !LoginForm.administrator)
            {
                MessageBox.Show ( "***ERROR*** You do not have permission to change the SSN!", "Change SSN Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }
            string ssn = unfilteredSSN;
            ChangeSSN ssnForm = new ChangeSSN(workContract, ssn );
            ssnForm.SelectDone += SsnForm_SelectDone;
            ssnForm.ShowDialog();
        }
        /****************************************************************************************/
        private void SsnForm_SelectDone ( string newSSN )
        {
            unfilteredSSN = FixSSN(newSSN);
            filteredSSN = codeFilteredSSN(newSSN);

            txtSSN.Text = filteredSSN;
            txtSSN.Refresh();

        }
        /****************************************************************************************/
        private bool popupCreated = false;
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            string item = textBox1.Text.Trim();
            if (String.IsNullOrWhiteSpace(item))
            {
                gridMain6.Focus();
                return;
            }
            string answer = item;
            string address = "";
            string city = "";
            string county = "";
            string state = "";
            string zip = "";
            string phone = "";
            string location = "";
            if (itemDt == null)
                return;

            try
            {
                DataRow[] dRows = null;
                dRows = itemDt.Select("answer='" + item + "' AND location='" + EditCust.activeFuneralHomeName + "'");
                if (dRows.Length <= 0)
                {
                    dRows = itemDt.Select("answer='" + item + "' AND location='All'");
                    if ( dRows.Length <= 0 )
                        dRows = itemDt.Select("answer='" + item + "'");
                }
                if (dRows.Length > 0)
                {
                    location = dRows[0]["location"].ObjToString();
                    address = dRows[0]["address"].ObjToString();
                    city = dRows[0]["city"].ObjToString();
                    county = dRows[0]["county"].ObjToString();
                    state = dRows[0]["state"].ObjToString();
                    zip = dRows[0]["zip"].ObjToString();
                    phone = dRows[0]["phone"].ObjToString();
                }
            }
            catch (Exception ex)
            {
            }
            if (funDemo == null || funDemo.IsDisposed )
            {
                if (!isTracking)
                    return;
                funDemo = new FuneralDemo("Place", editingWhat, item, "", "", "", "","");
                funDemo.FunDemoDone += FunDemo_FunDemoDone;
                Rectangle rect = funDemo.Bounds;
                int top = rect.Y;
                int left = rect.X;
                int height = rect.Height;
                int width = rect.Width;
                top = this.Bounds.Y;
                left = this.Bounds.Width - width;
                funDemo.StartPosition = FormStartPosition.Manual;
                funDemo.SetBounds(left, top, width, height);

                funDemo.Show();
                popupCreated = true;
            }

            if (funDemo != null && !funDemo.IsDisposed )
            {
                try
                {
                    if (isTracking)
                    {
                        funDemo.FireEventFunDemoLoad("Place", editingWhat, "", "", "", "", "", item, address, city, county, state, zip, phone, location);
                        funDemo.TopMost = true;
                        if (!funDemo.Visible && !funDemo.IsDisposed)
                        {
                            funDemo.Visible = true;
                            funDemo.Refresh();
                        }
                        //if (created)
                        if (popupForm != null)
                        {
                            popupForm.ListBox.Show();
                            popupForm.ListBox.Focus();
                            popupForm.ListBox.Refresh();
                            popupForm.ListBox.Visible = true;
                        }
                    }
                    //this.Focus();
                }
                catch (Exception ex)
                {
                    if (funDemo.IsDisposed)
                    {
                        funDemo = new FuneralDemo("Place", "", "", "", "", "", "", "", "", "", "", "");
                        funDemo.FunDemoDone += FunDemo_FunDemoDone;
                        funDemo.Show();
                        funDemo.Hide();
                    }
                }
            }

            DataRow dr = gridMain6.GetFocusedDataRow();
            //dr["data"] = item;
            //gridMain6.RefreshEditor(true);
            //popupForm.Focus();
            //cmb.ShowPopup();
        }
        /****************************************************************************************/
        private void BringFunDemoUp( string currentData )
        {
            //funDemo.FireEventFunDemoLoad("Place", editingWhat, "", "", "", "", "", currentData.Trim(), "", "", "", "", "", "", "");

            if (!isTracking)
                return;

            funDemo = new FuneralDemo("Place", currentData, "", "", "", "", "", "" );
            this.Text = "Demographic Details for " + editingWhat;
            funDemo.FunDemoDone += FunDemo_FunDemoDone;
            Rectangle rect = funDemo.Bounds;
            int top = rect.Y;
            int left = rect.X;
            int height = rect.Height;
            int width = rect.Width;
            top = this.Bounds.Y;
            left = this.Bounds.Width - width;
            funDemo.StartPosition = FormStartPosition.Manual;
            funDemo.SetBounds(left, top, width, height);

            funDemo.Show();
            popupCreated = true;
        }
        /****************************************************************************************/
        private void gridMain6_MouseMove(object sender, MouseEventArgs e)
        {
            if (funDemo == null)
                return;
            if (funDemo.Visible)
            {
                funDemo.Hide();
                textBox1.Text = "";
                textBox1.Refresh();
            }
        }
        /****************************************************************************************/
        private void FunCustomer_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 39)
            {
                e.KeyChar = '`';
                e.Handled = false;
            }
        }
    }
    /****************************************************************************************/
}