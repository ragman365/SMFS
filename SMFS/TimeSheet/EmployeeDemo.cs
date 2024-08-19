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
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraEditors.Repository;
using System.Text.RegularExpressions;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using System.Globalization;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class EmployeeDemo : DevExpress.XtraEditors.XtraForm
    {
        private bool funModified = false;
        private bool customerModified = false;
        private bool loading = true;
        private bool isProtected = false;
        private DataTable directorsDt = null;
        private DataTable arrangersDt = null;
        private string unfilteredSSN = "";
        private string filteredSSN = "";
        private string mainSSN = "";
        private string myGender = "";
        private bool avoidSSN = false;
        private bool workNewEmployee = false;
        private string workEmployeeId = "";
        private string workRecord = "";
        private string workTcRecord = "";
        private string workUserName = "";
        private string workFirstName = "";
        private string workLastName = "";
        private string workPassword = "";
        /****************************************************************************************/
        public EmployeeDemo()
        {
            InitializeComponent();
            workNewEmployee = true;
            workEmployeeId = "";
            workRecord = "";
        }
        /****************************************************************************************/
        public EmployeeDemo( string employeeId )
        {
            InitializeComponent();
            workNewEmployee = false;
            workEmployeeId = employeeId;
        }
        /****************************************************************************************/
        private void EmployeeDemo_Load(object sender, EventArgs e)
        {
            funModified = false;
            if (!workNewEmployee)
            {
                LoadEmployee();
                txtUsername.Enabled = false;
                btnPassword.Enabled = false;
            }

            //LoadSupervisors();
            LoadTimeKeepers();

            loading = false;
        }
        /***********************************************************************************************/
        private void LoadEmployee()
        {
            btnSaveAll.Hide();
            string cmd = "Select * from `users` where `record` = '" + workEmployeeId + "';";
            DataTable dt = G1.get_db_data(cmd);
            if ( dt.Rows.Count <= 0 )
            {
                MessageBox.Show("***ERROR*** Reading Employee Record " + workEmployeeId + "!", "Reading Employee Problem Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }

            workRecord = workEmployeeId;

            loading = true;

            dt.Columns.Add("bDate");
            dt.Columns.Add("hDate");
            dt.Columns.Add("tDate");

            workUserName = dt.Rows[0]["UserName"].ObjToString();
            workFirstName = dt.Rows[0]["firstName"].ObjToString();
            workLastName = dt.Rows[0]["lastName"].ObjToString();
            string pwd = dt.Rows[0]["pwd"].ObjToString();
            if (!String.IsNullOrWhiteSpace(pwd))
            {
                txtPassword.Text = "*********";
                btnPassword.Text = "Reset Password";
            }
            txtUsername.Text = workUserName;
            txtFirstName.Text = workFirstName;
            txtLastName.Text = workLastName;

            string noTimeSheet = dt.Rows[0]["noTimeSheet"].ObjToString();
            if (noTimeSheet.ToUpper() == "Y")
                chkNoTimeSheet.Checked = true;
            cmbStatus.Text = dt.Rows[0]["status"].ObjToString();
            cmbClassification.Text = dt.Rows[0]["classification"].ObjToString();

            LoadAssignedLocations(dt);

            workTcRecord = "";
            cmd = "Select * from `tc_er` where `username` = '" + workUserName + "';";
            DataTable dx = G1.get_db_data(cmd);
            if ( dx.Rows.Count <= 0 )
            {
                cmd = "Delete from `tc_er` WHERE `prefix` = '-1';";
                G1.get_db_data(cmd);

                workTcRecord = G1.create_record("tc_er", "prefix", "-1");
                if (G1.BadRecord("tc_er", workTcRecord))
                    return;
                G1.update_db_table("tc_er", "record", workTcRecord, new string[] { "username", workUserName, "prefix", "" });
            }

            cmd = "Select * from `tc_er` where `username` = '" + workUserName + "';";
            dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
                return;

            workTcRecord = dx.Rows[0]["record"].ObjToString();

            dx.Columns.Add("ssno");
            dx.Columns.Add("gender");
            //dx.Rows[0]["gender"] = "1";

            if (!G1.isHR())
                txtSSN.Enabled = false;



            txtMiddleName.Text = dx.Rows[0]["middleName"].ObjToString();

            txtPrefix.Text = dx.Rows[0]["prefix"].ObjToString();
            txtSuffix.Text = dx.Rows[0]["suffix"].ObjToString();
            txtFullLegalName.Text = dx.Rows[0]["legalName"].ObjToString();
            txtPreferedName.Text = dx.Rows[0]["preferredName"].ObjToString();
            txtMaidenName.Text = dx.Rows[0]["maidenName"].ObjToString();
            txtEmail.Text = dx.Rows[0]["emailAddress"].ObjToString();
            cmbDelivery.Text = dx.Rows[0]["delivery"].ObjToString();
            string EmpStatus = dx.Rows[0]["EmpStatus"].ObjToString();
            cmbEmpStatus.Text = EmpStatus;
            //if (EmpStatus.ToUpper().IndexOf("PARTTIME") < 0)
            //{
            //    btnSetupServices.Hide();
            //    btnSetupServices.Refresh();
            //}
            //else
            //{
            //    btnSetupServices.Show();
            //    btnSetupServices.Refresh();
            //}

            btnSetupServices.Show();
            btnSetupServices.Refresh();

            cmbExemptStatus.Text = dx.Rows[0]["EmpType"].ObjToString();

            CustomerDetails.FormatSSN(dx, "ssn", "ssno");
            CustomerDetails.FixDates(dt, "hireDate", "hDate");
            CustomerDetails.FixDates(dt, "termDate", "tDate");
            CustomerDetails.FixDates(dt, "birthDate", "bDate");

            string bdate = dt.Rows[0]["bDate"].ObjToString();
            string age = G1.CalcAge(bdate);
            txtAge.Text = age;

            unfilteredSSN = dx.Rows[0]["ssn"].ObjToString();
            unfilteredSSN = FixSSN(unfilteredSSN);
            mainSSN = unfilteredSSN.Replace("-", "");


            if (G1.isHR())
            {
                txtSSN.Text = dx.Rows[0]["ssno"].ObjToString();
                filteredSSN = txtSSN.Text;

                mainSSN = unfilteredSSN;
            }
            else
                txtSSN.Text = "XXX-XX-XXXX";


            DateTime ddate = dt.Rows[0]["birthDate"].ObjToDateTime();
            if (ddate.Year > 1875)
            {
                dateDOB.Text = ddate.ToString("MM/dd/yyyy");
                txtBday.Text = ddate.ToString("MM/dd/yyyy");
            }

            ddate = dt.Rows[0]["hireDate"].ObjToDateTime();
            if (ddate.Year > 1875)
            {
                //dateDOB.Text = ddate.ToString("MM/dd/yyyy");
                txtHireDate.Text = ddate.ToString("MM/dd/yyyy");
            }

            ddate = dt.Rows[0]["termDate"].ObjToDateTime();
            if (ddate.Year > 1875)
            {
                //dateDOB.Text = ddate.ToString("MM/dd/yyyy");
                txtTermDate.Text = ddate.ToString("MM/dd/yyyy");
            }

            ddate = dx.Rows[0]["deceasedDate"].ObjToDateTime();
            dateDeceased.Text = ddate.ToString("MM/dd/yyyy");
            if (ddate.Year > 1800)
                txtDOD.Text = ddate.ToString("MM/dd/yyyy");

            ddate = dx.Rows[0]["effectiveDate"].ObjToDateTime();
            if (ddate.Year > 1875)
                txtEffectiveDate.Text = ddate.ToString("MM/dd/yyyy");

            groupBoxMailing.Show();
            lblEmail.Show();
            txtEmail.Show();
            lblDelivery.Show();
            cmbDelivery.Show();


            string sex = dx.Rows[0]["sex"].ObjToString();

            string gender = CustomerDetails.ValidateGender(dx.Rows[0]["sex"].ObjToString());
            if (!String.IsNullOrWhiteSpace(gender))
            {
                if (gender.ToUpper() != "UNKNOWN")
                {
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
                }
            }
            string maritalStatus = dx.Rows[0]["maritalstatus"].ObjToString();

            textEdit_patientAddressLine1.Text = dx.Rows[0]["address1"].ObjToString();
            textEdit_patientAddressLine2.Text = dx.Rows[0]["address2"].ObjToString();
            textEdit_patientCity.Text = dx.Rows[0]["city"].ObjToString();
            textEdit_patientZipCode.Text = dx.Rows[0]["zip1"].ObjToString();

            textEdit2.Text = dx.Rows[0]["mailAddress1"].ObjToString();
            textEdit3.Text = dx.Rows[0]["mailAddress2"].ObjToString();
            textEdit4.Text = dx.Rows[0]["mailCity"].ObjToString();
            textEdit1.Text = dx.Rows[0]["mailZip1"].ObjToString();


            string areaCode = dx.Rows[0]["areaCode"].ObjToString();
            string phone = dx.Rows[0]["phoneNumber"].ObjToString();

            string phone1 = dx.Rows[0]["phoneType1"].ObjToString();
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

            string phone2 = dx.Rows[0]["phoneType2"].ObjToString();
            if (phone2.ToUpper() != "CELL" && phone2.ToUpper() != "HOME" && phone2 != "WORK")
                phone2 = "";
            cmbPhoneQualifier2.Text = phone2;

            txtPhone1.Text = dx.Rows[0]["phoneNumber1"].ObjToString();
            txtPhone2.Text = dx.Rows[0]["phoneNumber2"].ObjToString();

            string state = dx.Rows[0]["state"].ObjToString();
            CustomerDetails.SetupComboTable(this.comboStates, "ref_states", "abbrev", state);

            state = dx.Rows[0]["mailState"].ObjToString();
            CustomerDetails.SetupComboTable(this.comboBox1, "ref_states", "abbrev", state);

            LoadCustomerPicture(dx);

            string sameAsHome = dx.Rows[0]["sameAsHome"].ObjToString();
            if (sameAsHome == "Y")
                chkSameAsHome.Checked = true;

            string isSupervisor = dx.Rows[0]["isSupervisor"].ObjToString();
            if (isSupervisor == "Y")
                chkIsSuper.Checked = true;

            string isTimeKeeper = dx.Rows[0]["isTimeKeeper"].ObjToString();
            if (isTimeKeeper == "Y")
                chkIsTimeKeeper.Checked = true;

            string isManager = dx.Rows[0]["isManager"].ObjToString();
            if (isManager == "Y")
                chkIsManager.Checked = true;

            string timekeeper = dx.Rows[0]["TimeKeeper"].ObjToString();
            if (!String.IsNullOrWhiteSpace(timekeeper))
                cmbTimeKeeper.Text = timekeeper;

            string salaried = dx.Rows[0]["salaried"].ObjToString();
            if (salaried == "Y")
                chkSalaried.Checked = true;

            string isBPM = dx.Rows[0]["isBPM"].ObjToString();
            if (isBPM == "Y")
                chkBPM.Checked = true;

            string splitBPM = dx.Rows[0]["splitBPM"].ObjToString();
            if (splitBPM == "Y")
                chkSplitBPM.Checked = true;

            string flux = dx.Rows[0]["flux"].ObjToString();
            if (flux == "Y")
                chkFlux.Checked = true;

            string str = "";
            double vacationOverride = dx.Rows[0]["vacationOverride"].ObjToDouble();
            if ( vacationOverride != 0D )
            {
                str = G1.ReformatMoney(vacationOverride);
                txtVacationOverride.Text = str;
            }

            double fullTimeHours = dx.Rows[0]["fullTimeHours"].ObjToDouble();
            str = G1.ReformatMoney(fullTimeHours);
            txtFullHours.Text = str;

            double rate = dx.Rows[0]["rate"].ObjToDouble();
            str = G1.ReformatMoney(rate);
            str = str.Replace(",", "");
            if (!G1.isHR())
            {
                txtRate.Text = "**.**";
                txtRate.Enabled = false;
            }
            else
            {
                //str = dx.Rows[0]["erate"].ObjToString();
                //str = G1.GetDecriptedWord ( str );
                if ( !Employees.showRates )
                    txtRate.Text = "**.**";
                else
                    txtRate.Text = str;
            }

            double biWeekly = dx.Rows[0]["biWeekly"].ObjToDouble();
            string BsalStr = G1.ReformatMoney(biWeekly);
            //BsalStr = BsalStr.Replace(",", "");

            double futureBiWeekly = dx.Rows[0]["futureBiWeekly"].ObjToDouble();
            string FBsalStr = G1.ReformatMoney(futureBiWeekly);
            //FBsalStr = FBsalStr.Replace(",", "");


            double salary = dx.Rows[0]["salary"].ObjToDouble();
            string salaryStr = G1.ReformatMoney(salary);
            //salaryStr = salaryStr.Replace(",", "");

            double futureSalary = dx.Rows[0]["futureSalary"].ObjToDouble();
            string salStr = G1.ReformatMoney(futureSalary);
            //salStr = salStr.Replace(",", "");

            double futureRate = dx.Rows[0]["futureRate"].ObjToDouble();
            if (futureSalary > 0D)
                futureRate = futureSalary / 2080D;

            str = G1.ReformatMoney(futureRate);
            str = str.Replace(",", "");
            if (!G1.isHR())
            {
                txtFutureBiWeekly.Text = "**.**";
                txtBiWeekly.Text = "**.**";
                txtFutureSalary.Text = "**.**";
                txtSalary.Text = "**.**";
                txtFutureRate.Text = "**.**";
                txtBiWeekly.Enabled = false;
                txtFutureBiWeekly.Enabled = false;
                txtSalary.Enabled = false;
                txtFutureSalary.Enabled = false;
                txtFutureRate.Enabled = false;
                txtEffectiveDate.Text = "**/**/****";
                txtEffectiveDate.Enabled = false;
                pictureBox1.Hide();
            }
            else
            {
                if (!Employees.showRates)
                {
                    txtFutureBiWeekly.Text = "**.**";
                    txtBiWeekly.Text = "**.**";
                    txtFutureSalary.Text = "**.**";
                    txtSalary.Text = "**.**";
                    txtFutureRate.Text = "**.**";
                    txtBiWeekly.Enabled = false;
                    txtFutureBiWeekly.Enabled = false;
                    txtSalary.Enabled = false;
                    txtFutureSalary.Enabled = false;
                    txtFutureRate.Enabled = false;
                    txtBiWeekly.Enabled = false;
                    txtFutureRate.Text = "**.**";
                    txtEffectiveDate.Text = "**/**/****";
                }
                else
                {
                    txtFutureRate.Text = str;

                    txtSalary.Text = salaryStr;
                    txtFutureSalary.Text = salStr;

                    txtBiWeekly.Text = BsalStr;
                    txtFutureBiWeekly.Text = FBsalStr;
                }
            }
            string location = dx.Rows[0]["Location"].ObjToString();
            cmbHomeLocation.Text = location;

            GetLicenseInfo ( dx );

            SetupBenefits(dx);

            dateDeceased.Hide();
            dateDOB.Hide();
            funModified = false;
            btnSaveAll.Hide();

            //this.TopMost = true;
            //this.FormBorderStyle = FormBorderStyle.SizableToolWindow;

            panelAll.Refresh();
            this.Refresh();
        }
        /***************************************************************************************/
        private void SetupBenefits ( DataTable dx )
        {
            DateTime hireDate = txtHireDate.Text.Trim().ObjToDateTime();
            DateTime now = DateTime.Now;
            double yearlyVacation = 0D;
            double vacationTaken = 0D;
            double yearlySick = 0D;
            double sickTaken = 0D;

            string str = "0.00";
            txtYearlyVacatiion.Text = str;
            txtVacationTaken.Text = str;
            txtYearlySick.Text = str;
            txtSickTaken.Text = str;

            if (hireDate.Year < 100)
                return;

            TimeSpan ts = now - hireDate;
            if (ts.TotalDays <= 0)
                return;
            double years = ts.TotalDays / 365D;
            if (years >= 11D)
                yearlyVacation = 15D;
            else if (years >= 2D)
                yearlyVacation = 10D;
            else if (years >= 1D)
                yearlyVacation = 5D;

            str = G1.ReformatMoney(yearlyVacation);
            txtYearlyVacatiion.Text = str;

            if (years >= 10D)
                yearlySick = 10D;
            else if (years >= 6D)
                yearlySick = 6D;
            else if (years >= 2D)
                yearlySick = 4D;
            else if (years >= 1D)
                yearlySick = 2D;

            str = G1.ReformatMoney(yearlySick);
            txtYearlySick.Text = str;
        }
        /***************************************************************************************/
        private string originalDirectorRecord = "";
        private string originalCrematoryRecord = "";
        private string originalArrangerRecord = "";
        private void GetLicenseInfo ( DataTable dx )
        {
            originalDirectorRecord = "";
            originalCrematoryRecord = "";
            originalArrangerRecord = "";

            string firstName = txtFirstName.Text.Trim();
            string lastName = txtLastName.Text.Trim();
            string license = "";
            string location = "";
            string homeLocation = cmbHomeLocation.Text.Trim();

            string cmd = "Select * from `directors` where `lastName` = '" + lastName + "' and `firstName` = '" + firstName + "';";
            DataTable dt = G1.get_db_data(cmd);
            if ( dt.Rows.Count > 0 )
            {
                for ( int i=0; i<dt.Rows.Count; i++)
                {
                    license = dt.Rows[i]["license"].ObjToString();
                    if (String.IsNullOrWhiteSpace(license))
                        continue;
                    if ( license.ToUpper().IndexOf ( "F" ) == 0 )
                    {
                        originalDirectorRecord = dt.Rows[i]["record"].ObjToString();
                        txtFuneralLicense.Text = license;
                        homeLocation = dt.Rows[i]["location"].ObjToString();
                        cmbHomeLocation.Text = homeLocation;
                    }
                    else if ( license.ToUpper().IndexOf ( "C") == 0 )
                    {
                        originalCrematoryRecord = dt.Rows[i]["record"].ObjToString();
                        txtCrematory.Text = license;
                    }
                }
            }

            cmd = "Select * from `arrangers` where `lastName` = '" + lastName + "' and `firstName` = '" + firstName + "';";
            dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                cmbArranger.Text = "Yes";
                originalArrangerRecord = dt.Rows[0]["record"].ObjToString();
            }
            else
                cmbArranger.Text = "No";
        }
        /***************************************************************************************/
        private void LoadAssignedLocations ( DataTable dt )
        {
            string locations = dt.Rows[0]["assignedLocations"].ObjToString();
            string[] Lines = locations.Split('~');

            string cmd = "Select * from `funeralhomes`;";
            DataTable dd = G1.get_db_data(cmd);

            DataView tempview = dd.DefaultView;
            tempview.Sort = "LocationCode asc";
            dd = tempview.ToTable();

            string location = "";
            string name = "";
            DataRow[] dRows = null;
            cmbAssignedLocations.Properties.DataSource = dd;

            cmbAssignedLocations.EditValue = locations;

            cmbHomeLocation.Items.Clear();
            for (int i = 0; i < dd.Rows.Count; i++)
                cmbHomeLocation.Items.Add(dd.Rows[i]["LocationCode"].ObjToString());
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
        /***********************************************************************************************/
        private void LoadCustomerPicture ( DataTable dt )
        {
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
                                if (!String.IsNullOrWhiteSpace(workTcRecord))
                                    G1.update_blob("rc_er", "record", workTcRecord, "picture", bytes);
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
                if (!String.IsNullOrWhiteSpace(workTcRecord))
                    G1.update_blob("tc_er", "record", workTcRecord, "picture", bytes);
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
        public delegate void d_EmployeeDone(string empNo);
        public event d_EmployeeDone EmployeeDone;
        protected void OnDone()
        {
            if (EmployeeDone != null)
            {
                if (dataSaved)
                    EmployeeDone(workEmployeeId);
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
            btnSaveAll.Refresh();
        }
        /***********************************************************************************************/
        private void DeceasedTextChanged(object sender, EventArgs e)
        {
            string date = dateDeceased.Text;
            if (G1.validate_date(date))
            {
                DateTime ddate = date.ObjToDateTime();
                //txtHireDate.Enabled = true;
                //txtTermDate.Enabled = true;
            }
            else
            {
                //txtHireDate.Enabled = false;
                //txtHireDate.Text = "";
                //txtTermDate.Enabled = false;
                //txtTermDate.Text = "";
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
                //txtHireDate.Enabled = true;
                //txtTermDate.Enabled = true;
            }
            else
            {
                //txtHireDate.Enabled = false;
                //txtHireDate.Text = "";
                //txtTermDate.Enabled = false;
                //txtTermDate.Text = "";
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
                //txtHireDate.Enabled = true;
                //txtTermDate.Enabled = true;
            }
            else
            {
                //txtHireDate.Enabled = false;
                //txtHireDate.Text = "";
                //txtTermDate.Enabled = false;
                //txtTermDate.Text = "";
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
                //txtHireDate.Enabled = true;
                //txtTermDate.Enabled = true;
            }
            else
            {
                //txtHireDate.Enabled = false;
                //txtHireDate.Text = "";
                //txtTermDate.Enabled = false;
                //txtTermDate.Text = "";
            }
        }
        /****************************************************************************************/
        private void FunCustomer_FormClosed(object sender, FormClosedEventArgs e)
        {
            if ( dataSaved)
            {
                OnDone();
            }
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
            //string cmd = "Select * from `" + customersFile + "` where `contractNumber` = '" + workContract + "';";
            //DataTable dx = G1.get_db_data(cmd);
            //DateTime ddate = dx.Rows[0]["deceasedDate"].ObjToDateTime();
            //if (ddate.Year < 1875)
            //    ddate = dx.Rows[0]["deceasedDate"].ObjToDateTime();
            //dateDeceased.Text = ddate.ToString("MM/dd/yyyy");
            //if (ddate.Year > 1800)
            //{
            //    txtDOD.Text = ddate.ToString("MM/dd/yyyy");
            //    txtDOD.Refresh();
            //}
        }
        /****************************************************************************************/
        private void txtDOD_Leave(object sender, EventArgs e)
        {
            string date = txtDOD.Text;
            if (String.IsNullOrWhiteSpace(date))
            {
                //if (workFuneral)
                //{
                //    MessageBox.Show("***ERROR*** You cannot Blank Out the Date of Death of an Existing Funeral!", "Date Problem Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                //    ResetDOD();
                //    return;
                //}
                //txtHireDate.Enabled = false;
                //txtHireDate.Text = "";
                //txtTermDate.Enabled = false;
                //txtTermDate.Text = "";
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
                    dateDeceased.Text = txtDOD.Text;
                    //txtHireDate.Enabled = true;
                    //txtTermDate.Enabled = true;
                }
                else
                {
                    //txtHireDate.Enabled = false;
                    //txtHireDate.Text = "";
                    //txtTermDate.Enabled = false;
                    //txtTermDate.Text = "";
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
                //txtHireDate.Enabled = true;
                //txtTermDate.Enabled = true;
            }
            else
            {
                bool save = false;
                if (btnSaveAll.Visible)
                    save = true;
                //txtDOD.Text = "";
                //txtHireDate.Enabled = false;
                //txtHireDate.Text = "";
                //txtTermDate.Enabled = false;
                //txtTermDate.Text = "";
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
            if (bDay.Year > 1500)
            {
                DateTime dod = txtDOD.Text.ObjToDateTime();
                if (dod.Year < 100)
                    dod = DateTime.Now;
                txtAge.Text = G1.GetAge(bDay, dod).ToString();
            }
        }
        /****************************************************************************************/
        private bool specialLoading = false;
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
                        txtCounty.Text = county;
                    }
                }
            }
        }
        /****************************************************************************************/
        private void txtServiceDate_Enter(object sender, EventArgs e)
        {
            string date = txtHireDate.Text;
            if (G1.validate_date(date))
            {
                DateTime ddate = date.ObjToDateTime();
                txtHireDate.Text = ddate.ToString("MM/dd/yyyy");
            }
            else
                txtHireDate.Text = "";
        }
        /****************************************************************************************/
        private bool ServiceDateChanged = false;
        private void txtServiceDate_Leave(object sender, EventArgs e)
        {
            string date = txtHireDate.Text;
            if (String.IsNullOrWhiteSpace(date))
                return;
            if (G1.validate_date(date))
            {
                DateTime ddate = txtHireDate.Text.ObjToDateTime();
                if (ddate.Year < 1800)
                {
                    MessageBox.Show("***ERROR*** Date Entered Year is less than 1800!", "Date Problem Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    return;
                }
                if (ddate.Year > 100)
                {
                    txtHireDate.Text = ddate.ToString("MM/dd/yyyy");
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
        private void txtArrangementDate_Enter(object sender, EventArgs e)
        {
            string date = txtTermDate.Text;
            if (G1.validate_date(date))
            {
                DateTime ddate = date.ObjToDateTime();
                txtTermDate.Text = ddate.ToString("MM/dd/yyyy");
            }
            else
                txtTermDate.Text = "";
        }
        /****************************************************************************************/
        private void txtArrangementDate_Leave(object sender, EventArgs e)
        {
            string date = txtTermDate.Text;
            if (String.IsNullOrWhiteSpace(date))
                return;
            if (G1.validate_date(date))
            {
                DateTime ddate = txtTermDate.Text.ObjToDateTime();
                if (ddate.Year < 1800)
                {
                    MessageBox.Show("***ERROR*** Date Entered Year is less than 1800!", "Date Problem Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    return;
                }
                if (ddate.Year > 100)
                    txtTermDate.Text = ddate.ToString("MM/dd/yyyy");
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
                    string cmd = "Select * from `tc_er` where `ssn` = '" + ssn + "';";
                    DataTable dt = G1.get_db_data(cmd);
                    if (dt.Rows.Count > 0)
                    {
                        string list = "";
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            contractNumber = dt.Rows[i]["username"].ObjToString();
                            //if (contractNumber != workContract)
                            //    list += contractNumber + ",";
                        }
                        list = list.TrimEnd(',');
                        if (!String.IsNullOrWhiteSpace(list))
                        {
                            if (G1.isAdminOrSuper())
                            {
                                DialogResult result = MessageBox.Show("***ERROR*** Other Employees have the same SSN!\nDo you still want to use this SSN?\n" + list, "Changing SSN Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
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
                                MessageBox.Show("***ERROR*** Other Employees have the same SSN!\nPlease contact Admin to use this SSN\n" + list, "Changing SSN Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
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
        private void OnCustomerModified()
        {
        }
        /****************************************************************************************/
        private void FunCustomer_FormClosing(object sender, FormClosingEventArgs e)
        {
            OnCustomerModified();
        }
        /****************************************************************************************/
        private void btnPassword_Click(object sender, EventArgs e)
        {
            using (addPassword passForm = new addPassword(txtPassword.Text))
            {
                DialogResult result = passForm.ShowDialog();
                if (result == DialogResult.OK)
                {
                    workPassword = passForm.Answer;
                    btnPassword.BackColor = Color.Lime;
                    btnPassword.Text = "Reset Password";
                    txtPassword.Text = "*********";
                    txtPassword.Refresh();


                    btnSaveAll.Show();
                    btnSaveAll.Refresh();
                }
            }
        }
        /****************************************************************************************/
        private bool dataSaved = false;
        private void btnSaveAll_Click(object sender, EventArgs e)
        {
            SaveEmployee();

            customerModified = true;

            //LoadEmployee();

            loading = false;
            string dod = this.txtDOD.Text;
            DateTime date = dod.ObjToDateTime();

            groupBoxMailing.Show();
            lblEmail.Show();
            txtEmail.Show();
            lblDelivery.Show();
            cmbDelivery.Show();
            panelAll.Refresh();
            this.Refresh();

            ServiceDateChanged = false;
        }
        /***************************************************************************************/
        private bool SaveEmployee ()
        {
            if (!btnSaveAll.Visible)
                return true;
            string userName = txtUsername.Text.Trim();
            if ( String.IsNullOrWhiteSpace ( userName ))
            {
                MessageBox.Show("***ERROR*** Username CANNOT be EMPTY! ", "Save Employee Problem Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return false;
            }
            if ( !String.IsNullOrWhiteSpace ( workUserName ))
            {
                if ( userName.Trim() != workUserName )
                {
                    DialogResult result = MessageBox.Show("*** Question *** Are you changing an existing Username?", "Changing Username Dialog", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    if (result == DialogResult.Cancel)
                        return false;
                    if ( result == DialogResult.No )
                    {
                        MessageBox.Show("Then you may not save this data!\nGet out and try again without changing the Username!", "Save Employee Problem Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                        return false;
                    }
                    string cmd = "Select * from `users` where `userName` = '" + workUserName + "';";
                    DataTable ddd = G1.get_db_data(cmd);
                    if (ddd.Rows.Count <= 0)
                        return false;
                    string rec = ddd.Rows[0]["record"].ObjToString();
                    if ( rec != workRecord )
                    {
                        MessageBox.Show("Something went wrong!\nGet out and try again without changing the Username!", "Save Employee Problem Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                        return false;
                    }
                    cmd = "Select * from `tc_er` where `username` = '" + workUserName + "';";
                    ddd = G1.get_db_data(cmd);
                    if (ddd.Rows.Count <= 0)
                        return false;
                    string tc_rec = ddd.Rows[0]["record"].ObjToString();
                    if ( tc_rec != workTcRecord )
                    {
                        MessageBox.Show("Something went wrong!\nGet out and try again without changing the Username!", "Save Employee Problem Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                        return false;
                    }

                    G1.update_db_table("users", "record", workRecord, new string[] {"userName", userName });
                    if ( !String.IsNullOrWhiteSpace ( workTcRecord ))
                        G1.update_db_table("tc_er", "record", workTcRecord, new string[] { "userName", userName });

                    workUserName = userName;
                }
            }
            if ( String.IsNullOrWhiteSpace ( workRecord ))
            {
                string cmd = "Delete from `users` WHERE `firstName` = '-1';";
                G1.get_db_data(cmd);
                cmd = "Select * from `users` where `userName` = '" + userName + "';";
                DataTable ddd = G1.get_db_data(cmd);
                if (ddd.Rows.Count > 0)
                    workRecord = ddd.Rows[0]["record"].ObjToString();
                else
                {
                    workRecord = G1.create_record("users", "firstName", "-1");
                    if (G1.BadRecord("users", workRecord))
                        return false;
                    G1.update_db_table("users", "record", workRecord, new string[] { "userName", userName });
                }
                workUserName = userName;
            }

            if (String.IsNullOrWhiteSpace(workTcRecord))
            {
                string cmd = "Delete from `tc_er` WHERE `prefix` = '-1';";
                G1.get_db_data(cmd);
                workTcRecord = G1.create_record("tc_er", "prefix", "-1");
                if (G1.BadRecord("tc_er", workTcRecord))
                    return false;
                G1.update_db_table("tc_er", "record", workTcRecord, new string[] { "username", userName });
            }

            string fname = txtFirstName.Text;
            string lname = txtLastName.Text;
            G1.update_db_table("users", "record", workRecord, new string[] { "firstName", fname, "lastName", lname });

            string noTimeSheet = "N";
            if (chkNoTimeSheet.Checked)
                noTimeSheet = "Y";

            DateTime hireDate = txtHireDate.Text.ObjToDateTime();
            DateTime termDate = txtTermDate.Text.ObjToDateTime();
            DateTime birthDate = txtBday.Text.ObjToDateTime();
            string email = txtEmail.Text;
            string status = cmbStatus.Text;
            string assignedLocations = cmbAssignedLocations.EditValue.ObjToString().Trim();
            string classification = cmbClassification.Text.Trim();

            G1.update_db_table("users", "record", workRecord, new string[] { "noTimeSheet", noTimeSheet, "hireDate", hireDate.ToString("MM/dd/yyyy"), "termDate", termDate.ToString("MM/dd/yyyy"), "birthDate", birthDate.ToString("MM/dd/yyyy"), "email", email, "status", status, "assignedLocations", assignedLocations, "classification", classification });

            if ( !String.IsNullOrWhiteSpace ( workPassword ))
            {
                string PasswordHash = LoginForm.Hash ( workPassword );

                G1.update_db_table("users", "record", workRecord, new string[] { "pwd", PasswordHash });
            }

            SaveTc_Er();

            dataSaved = true;

            btnSaveAll.Hide();
            btnSaveAll.Refresh();
            return true;
        }
        /***************************************************************************************/
        public void SaveTc_Er ()
        {
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
            string isSupervisor = "N";
            if (chkIsSuper.Checked)
                isSupervisor = "Y";
            string isTimeKeeper = "N";
            if (chkIsTimeKeeper.Checked)
                isTimeKeeper = "Y";
            string isManager = "N";
            if (chkIsManager.Checked)
                isManager = "Y";
            string salaried = "N";
            if (chkSalaried.Checked)
                salaried = "Y";
            string flux = "N";
            if (chkFlux.Checked)
                flux = "Y";
            string isBPM = "N";
            if (chkBPM.Checked)
                isBPM = "Y";

            string splitBPM = "N";
            if (chkSplitBPM.Checked)
                splitBPM = "Y";

            string location = cmbHomeLocation.Text.Trim();

            string cmd = "";
            string ssn = txtSSN.Text;
            ssn = ssn.Replace("-", "");
            if ( String.IsNullOrWhiteSpace ( workTcRecord ))
            {
                cmd = "Delete from `tc_er` WHERE `prefix` = '-1';";
                G1.get_db_data(cmd);

                workTcRecord = G1.create_record("tc_er", "prefix", "-1");
                if ( G1.BadRecord ( "tc_er", workTcRecord ))
                    return;
                G1.update_db_table("tc_er", "record", workTcRecord, new string[] { "username", workUserName });
            }

            G1.update_db_table("tc_er", "record", workTcRecord, new string[] { "middleName", mname, "suffix", suffix, "prefix", prefix, "preferredName", preferredName, "legalName", legalName, "maidenName", maidenName, "emailAddress", email, "delivery", delivery });
            if (G1.isHR())
            {
                if (Employees.showRates)
                {
                    decimal rate = txtRate.Text.ObjToDecimal();
                    decimal futureRate = txtFutureRate.Text.ObjToDecimal();

                    decimal salary = txtSalary.Text.ObjToDecimal();
                    decimal futureSalary = txtFutureSalary.Text.ObjToDecimal();

                    decimal biWeekly = txtBiWeekly.Text.ObjToDecimal();
                    decimal futureBiWeekly = txtFutureBiWeekly.Text.ObjToDecimal();

                    decimal fullTimeHours = txtFullHours.Text.ObjToDecimal();

                    DateTime effectiveDate = txtEffectiveDate.Text.ObjToDateTime();
                    string str = G1.GetEncriptedWord(rate.ToString());
                    G1.update_db_table("tc_er", "record", workTcRecord, new string[] { "rate", rate.ToString(), "futureRate", futureRate.ToString(), "effectiveDate", effectiveDate.ToString("yyyy-MM-dd"), "futureSalary", futureSalary.ToString() });
                    G1.update_db_table("tc_er", "record", workTcRecord, new string[] { "salary", salary.ToString(), "biWeekly", biWeekly.ToString(), "futureBiWeekly", futureBiWeekly.ToString(), "fullTimeHours", fullTimeHours.ToString() });
                }
                string vacationOverride = txtVacationOverride.Text.Trim();
                if (!String.IsNullOrWhiteSpace(vacationOverride))
                {
                    vacationOverride = vacationOverride.Replace(",", "");
                    if (!G1.validate_numeric(vacationOverride))
                        vacationOverride = "";
                    G1.update_db_table("tc_er", "record", workTcRecord, new string[] { "vacationOverride", vacationOverride });
                }
            }

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
                if (!String.IsNullOrWhiteSpace(deceasedDate))
                {
                    deceasedDate = G1.date_to_sql(deceasedDate);
                    deceasedDate = deceasedDate.Replace("-", "");
                    gotDeceased = true;
                }
            }

            string ethnicity = "";
            string language = "";
            string serviceDate = txtHireDate.Text;
            string arrangementDate = txtTermDate.Text;
            if (!gotDeceased)
            {
                serviceDate = "";
                arrangementDate = "";
            }
            G1.update_db_table("tc_er", "record", workTcRecord, new string[] { "sex", gender, "maritalstatus", maritalStatus, "deceasedDate", deceasedDate, "isSupervisor", isSupervisor, "isTimeKeeper", isTimeKeeper, "isManager", isManager, "salaried", salaried, "flux", flux, "isBPM", isBPM, "splitBPM", splitBPM });

            string address1 = textEdit_patientAddressLine1.Text;
            string address2 = textEdit_patientAddressLine2.Text;
            string city = textEdit_patientCity.Text;
            string state = comboStates.Text;
            string zip = textEdit_patientZipCode.Text;
            G1.update_db_table ( "tc_er", "record", workTcRecord, new string[] { "address1", address1, "address2", address2, "city", city, "state", state, "zip1", zip, "Location", location });

            string mailAddress1 = textEdit2.Text;
            string mailAddress2 = textEdit3.Text;
            string mailCity = textEdit4.Text;
            string mailState = comboBox1.Text;
            string mailZip = textEdit1.Text;
            string empStatus = cmbEmpStatus.Text;
            string exemptStatus = cmbExemptStatus.Text;
            G1.update_db_table ( "tc_er", "record", workTcRecord, new string[] { "mailAddress1", mailAddress1, "mailAddress2", mailAddress2, "mailCity", mailCity, "mailState", mailState, "mailZip1", mailZip, "EmpStatus", empStatus, "EmpType", exemptStatus });

            string phoneType1 = cmbPhoneQualifier1.Text;
            string phoneType2 = cmbPhoneQualifier2.Text;
            string phone1 = txtPhone1.Text;
            string phone2 = txtPhone2.Text;
            string sameAsHome = "";
            if (chkSameAsHome.Checked)
                sameAsHome = "Y";
            string supervisor = cmbTimeKeeper.Text.Trim();
            G1.update_db_table ( "tc_er", "record", workTcRecord, new string[] { "phoneType1", phoneType1, "phoneType2", phoneType2, "phoneNumber1", phone1, "phoneNumber2", phone2, "sameAsHome", sameAsHome, "TimeKeeper", supervisor });


            if (G1.isHR())
            {
                if (!String.IsNullOrWhiteSpace(ssn))
                {
                    if (ssn.Length == 4)
                        ssn = "00000" + ssn;
                    int rv = ValidateSSN(ssn);
                    if (rv == 0)
                        G1.update_db_table("tc_er", "record", workTcRecord, new string[] { "ssn", ssn });
                }
            }

            SaveLicenseInfo(workTcRecord);

            btnSaveAll.Hide();
            btnSaveAll.Refresh();
        }
        /***********************************************************************************************/
        private void SaveLicenseInfo ( string workTcRecord )
        {
            string directorLicense = txtFuneralLicense.Text.Trim();
            string arranger = cmbArranger.Text.Trim();
            string crematoryLicense = txtCrematory.Text.Trim();

            string location = cmbHomeLocation.Text.Trim();

            string firstName = txtFirstName.Text.Trim();
            string lastName = txtLastName.Text.Trim();
            string middleName = txtMiddleName.Text.Trim();

            string record = "";

            if (!String.IsNullOrWhiteSpace(directorLicense))
            {
                if (!String.IsNullOrWhiteSpace(originalDirectorRecord))
                    G1.update_db_table("directors", "record", originalDirectorRecord, new string[] { "firstName", firstName, "lastName", lastName, "middleName", middleName, "license", directorLicense, "location", location });
                else
                {
                    record = G1.create_record("directors", "license", "-1");
                    if (G1.BadRecord("directors", record))
                        return;
                    G1.update_db_table("directors", "record", record, new string[] { "firstName", firstName, "lastName", lastName, "middleName", middleName, "license", directorLicense, "location", location });
                }
            }
            if ( !String.IsNullOrWhiteSpace ( crematoryLicense ))
            {
                if (!String.IsNullOrWhiteSpace(originalCrematoryRecord))
                    G1.update_db_table("directors", "record", originalCrematoryRecord, new string[] { "firstName", firstName, "lastName", lastName, "middleName", middleName, "license", crematoryLicense } );
                else
                {
                    record = G1.create_record("directors", "license", "-1");
                    if (G1.BadRecord("directors", record))
                        return;
                    G1.update_db_table("directors", "record", record, new string[] { "firstName", firstName, "lastName", lastName, "middleName", middleName, "license", crematoryLicense });
                }
            }


            G1.update_db_table("tc_er", "record", workTcRecord, new string[] { "crematoryLicense", crematoryLicense });

            if (arranger.Trim().ToUpper() == "YES")
            {
                if (!String.IsNullOrWhiteSpace(originalArrangerRecord))
                    G1.update_db_table("arrangers", "record", originalArrangerRecord, new string[] { "firstName", firstName, "lastName", lastName, "middleName", middleName, "license", directorLicense, "location", location });
                else
                {
                    record = G1.create_record("arrangers", "license", "-1");
                    if (G1.BadRecord("arrangers", record))
                        return;
                    G1.update_db_table("arrangers", "record", record, new string[] { "firstName", firstName, "lastName", lastName, "middleName", middleName, "license", directorLicense, "location", location });
                }
            }
            else
            {
                if ( !String.IsNullOrWhiteSpace ( originalArrangerRecord ))
                {
                }
            }
        }
        /***********************************************************************************************/
        private void LoadTimeKeepers()
        {
            string cmd = "Select * from `tc_er` WHERE `isTimeKeeper` = 'Y';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
                return;

            cmd = "Select * from `users`;";
            DataTable dt = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
                return;

            DataRow[] dRows = null;
            string userName = "";

            this.cmbTimeKeeper.Items.Clear();
            string timeKeeper = "";
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                userName = dx.Rows[i]["username"].ObjToString();
                dRows = dt.Select("userName='" + userName + "'");
                if (dRows.Length > 0)
                {
                    timeKeeper = dRows[0]["lastName"].ObjToString() + ", " + dRows[0]["firstName"].ObjToString();
                    this.cmbTimeKeeper.Items.Add(timeKeeper);
                }
            }
        }
        /****************************************************************************************/
        private void btnSetupServices_Click(object sender, EventArgs e)
        {
            string firstName = this.txtFirstName.Text.Trim();
            string lastName = this.txtLastName.Text.Trim();
            string name = firstName + " " + lastName;
            string EmpStatus = cmbEmpStatus.Text.Trim();
            //if (EmpStatus.ToUpper().IndexOf("PARTTIME") < 0)
            //{
            //    MessageBox.Show("*** Sorry *** This Employee is not setup for PartTime Labor!!", "PartTime Labor Services Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            //    return;
            //}

            this.Cursor = Cursors.WaitCursor;
            EditEmpContract empForm = new EditEmpContract("PartTime", workEmployeeId, workUserName, name);
            empForm.Show();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void cmbEmpStatus_SelectedValueChanged(object sender, EventArgs e)
        {
            System.Windows.Forms.ComboBox combo = (System.Windows.Forms.ComboBox)sender;
            string EmpStatus = combo.Text.Trim().ToUpper();
            if (EmpStatus.ToUpper().IndexOf("PARTTIME") < 0)
            {
                btnSetupServices.Hide();
                btnSetupServices.Refresh();
            }
            else
            {
                btnSetupServices.Show();
                btnSetupServices.Refresh();
            }
        }
        /****************************************************************************************/
        private void something_KeyUp(object sender, KeyEventArgs e)
        {
            if (loading)
                return;
            string str = "";
            double dValue = 0D;
            Type type = sender.GetType();
            string name = type.Name;
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Tab)
            {
                if (name.ToUpper() == "TEXTBOX")
                {
                    TextBox textbox = (TextBox)sender;
                    string field = textbox.Name.ToUpper();
                    bool isSalaried = chkSalaried.Checked;
                    string empStatus = cmbEmpStatus.Text.Trim().ToUpper();
                    double biWeeklyHours = 80D;
                    if (empStatus == "PARTTIME")
                        biWeeklyHours = 48D;
                    if (field == "TXTVACATIONOVERRIDE")
                    {
                        dValue = textbox.Text.ObjToDouble();
                        str = G1.ReformatMoney(dValue);
                        textbox.Text = str;
                        textbox.Refresh();

                        funModified = true;
                        OnCustomerModified();
                        btnSaveAll.Show();
                        btnSaveAll.Refresh();
                        return;
                    }

                    if (field == "TXTRATE" || field == "TXTFUTURERATE" )
                    {
                        dValue = textbox.Text.ObjToDouble();
                        str = G1.ReformatMoney(dValue);
                        textbox.Text = str;
                        textbox.Refresh();

                        funModified = true;
                        OnCustomerModified();
                        btnSaveAll.Show();
                        btnSaveAll.Refresh();
                    }
                    else if (field == "TXTBIWEEKLY" || field == "TXTFUTUREBIWEEKLY" )
                    {
                        dValue = textbox.Text.ObjToDouble();
                        str = G1.ReformatMoney(dValue);
                        textbox.Text = str;
                        textbox.Refresh();

                        funModified = true;
                        OnCustomerModified();
                        btnSaveAll.Show();
                        btnSaveAll.Refresh();

                        if (field == "TXTFUTUREBIWEEKLY")
                        {
                            loading = true;
                            double futureRate = dValue * 26D;
                            str = G1.ReformatMoney(futureRate);
                            txtFutureSalary.Text = str;
                            futureRate = dValue / biWeeklyHours;
                            str = G1.ReformatMoney(futureRate);
                            txtFutureRate.Text = str;
                            txtFutureRate.Refresh();
                            loading = false;
                            return;
                        }
                        else if ( field == "TXTBIWEEKLY")
                        {
                            loading = true;
                            double futureRate = dValue * 26D;
                            str = G1.ReformatMoney(futureRate);
                            txtSalary.Text = str;
                            futureRate = dValue / biWeeklyHours;
                            str = G1.ReformatMoney(futureRate);
                            txtRate.Text = str;
                            txtRate.Refresh();
                            loading = false;
                            return;
                        }
                    }
                    else if (field == "TXTSALARY" || field == "TXTFUTURESALARY")
                    {
                        dValue = textbox.Text.ObjToDouble();
                        str = G1.ReformatMoney(dValue);
                        textbox.Text = str;
                        textbox.Refresh();

                        funModified = true;
                        OnCustomerModified();
                        btnSaveAll.Show();
                        btnSaveAll.Refresh();

                        if (field == "TXTFUTURESALARY")
                        {
                            loading = true;
                            double futureRate = dValue / 26D;
                            futureRate = G1.RoundValue(futureRate);
                            str = G1.ReformatMoney(futureRate);
                            txtFutureBiWeekly.Text = str;
                            txtFutureBiWeekly.Refresh();

                            futureRate = futureRate / biWeeklyHours;
                            futureRate = G1.RoundValue(futureRate);
                            str = G1.ReformatMoney(futureRate);
                            txtFutureRate.Text = str;
                            txtFutureRate.Refresh();
                            loading = false;
                            return;
                        }
                        else if (field == "TXTSALARY")
                        {
                            loading = true;
                            double futureRate = dValue / 26D;
                            futureRate = G1.RoundValue(futureRate);
                            str = G1.ReformatMoney(futureRate);
                            txtBiWeekly.Text = str;
                            txtBiWeekly.Refresh();

                            futureRate = futureRate / biWeeklyHours;
                            futureRate = G1.RoundValue(futureRate);
                            str = G1.ReformatMoney(futureRate);
                            txtRate.Text = str;
                            txtRate.Refresh();
                            loading = false;
                            return;
                        }
                    }
                    else if (field == "TXTFULLHOURS" )
                    {
                        dValue = textbox.Text.ObjToDouble();
                        str = G1.ReformatMoney(dValue);
                        textbox.Text = str;
                        textbox.Refresh();

                        funModified = true;
                        OnCustomerModified();
                        btnSaveAll.Show();
                        btnSaveAll.Refresh();
                        return;
                    }
                }
                SendKeys.Send("{TAB}");
            }
        }
        /****************************************************************************************/
        private void txtEffectiveDate_Leave(object sender, EventArgs e)
        {
            string date = txtEffectiveDate.Text;
            DateTime ddate = date.ObjToDateTime();
            if (ddate.Year > 100)
                txtEffectiveDate.Text = ddate.ToString("MM/dd/yyyy");
        }
        /****************************************************************************************/
        private void txtEffectiveDate_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                txtEffectiveDate_Leave(sender, e);
            else if (e.KeyCode == Keys.Tab)
                txtEffectiveDate_Leave(sender, e);
        }
        /****************************************************************************************/
        private void txtEffectiveDate_Enter(object sender, EventArgs e)
        {
            string date = txtEffectiveDate.Text;
            if (G1.validate_date(date))
            {
                DateTime ddate = date.ObjToDateTime();
                txtEffectiveDate.Text = ddate.ToString("MM/dd/yyyy");
            }
            else
                txtEffectiveDate.Text = "";
        }
        /****************************************************************************************/
        private void txtUsername_TextChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            string user = txtUsername.Text.Trim();
            string cmd = "Select * from `users` where `userName` = '" + user + "';";
            DataTable dx = G1.get_db_data(cmd);
            if ( dx.Rows.Count > 0 )
            {
                txtFirstName.Text = dx.Rows[0]["firstName"].ObjToString();
                txtFirstName.Refresh();
                txtLastName.Text = dx.Rows[0]["lastName"].ObjToString();
                txtLastName.Refresh();
                DialogResult result = MessageBox.Show("***ERROR*** Username already exists!\nLocate user and try adding again.", "Create User Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                this.Close();
                return;
            }
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
        private void button1_Click(object sender, EventArgs e)
        {
            string firstName = this.txtFirstName.Text.Trim();
            string lastName = this.txtLastName.Text.Trim();
            string name = firstName + " " + lastName;
            string EmpStatus = cmbEmpStatus.Text.Trim();
            //if (EmpStatus.ToUpper().IndexOf("PARTTIME") < 0)
            //{
            //    MessageBox.Show("*** Sorry *** This Employee is not setup for PartTime Labor!!", "PartTime Labor Services Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            //    return;
            //}

            this.Cursor = Cursors.WaitCursor;
            EditEmpContract empForm = new EditEmpContract("Other", workEmployeeId, workUserName, name);
            empForm.Show();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void btnRateChange_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            EmployeeRate empForm = new EmployeeRate ( workEmployeeId, workUserName );
            empForm.Show();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
    }
} 