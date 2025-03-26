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
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class FuneralDemo : DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        private string workAnswer = "";
        private string workField = "";
        /***********************************************************************************************/
        private string workName = "";
        private string _name = "";
        private string _address = "";
        private string _city = "";
        private string _county = "";
        private string _state = "";
        private string _zip = "";
        private string _phone = "";

        private string whichTab = "";
        /***********************************************************************************************/
        public FuneralDemo( string field, string answer )
        {
            InitializeComponent();
            workField = field;
            workAnswer = answer;
        }
        /***********************************************************************************************/
        public FuneralDemo( string what, string name, string address, string city, string state, string zip, string phone, string location )
        {
            InitializeComponent();

            tabPage1.Text = what;
            txtName.Text = name;
            workName = name;
            txtAddress.Text = address;
            txtCity.Text = city;
            txtState.Text = state;
            txtZip.Text = zip;
            txtPhone.Text = phone;
            txtLocation.Text = location;
            txtLocationPlace.Text = location;

            whichTab = "PLACE";

            tabControl1.TabPages.Remove(tabPage2);
        }
        /***********************************************************************************************/
        public FuneralDemo(string what, string title, string firstName, string middleName, string lastName, string suffix, string address, string city, string state, string zip, string phone, string location )
        {
            InitializeComponent();

            tabPage2.Text = what;
            txtTitle.Text = title;
            txtFirstName.Text = firstName;
            txtMiddleName.Text = middleName;
            txtLastName.Text = lastName;
            txtSuffix.Text = suffix;
            txtAddress.Text = address;
            txtCity.Text = city;
            txtState.Text = state;
            txtZip.Text = zip;
            txtPhone.Text = phone;
            txtLocation.Text = location;
            txtLocationPlace.Text = location;

            whichTab = "PERSON";

            tabControl1.TabPages.Remove(tabPage1);

            this.Text = "Demographic Details for " + what;
        }
        /***********************************************************************************************/
        private void FuneralDemo_Load(object sender, EventArgs e)
        {
            txtTitle.Text = workAnswer;
            txtName.Text = workAnswer;
            _address = "";
            _city = "";
            _county = "";
            _state = "";
            _zip = "";
            _phone = "";
            this.Text = "Details for " + workField;
        }
        /****************************************************************************************/
        public void FireEventFunDemoLoad ( string what, string tabTitle, string title, string firstName, string middleName, string lastName, string suffix, string name, string address, string city, string county, string state, string zip, string phone, string location, string column = "" )
        {
            txtName.Text = name;
            if ( what.ToUpper() == "PERSON")
            {
                whichTab = what;
                txtTitle.Text = title;
                txtFirstName.Text = firstName;
                txtMiddleName.Text = middleName;
                txtLastName.Text = lastName;
                txtSuffix.Text = suffix;
                txtAddress2.Text = address;
                txtCity2.Text = city;
                txtCounty2.Text = county;
                txtState2.Text = state;
                txtZip2.Text = zip;
                txtPhone2.Text = phone;
                txtLocation.Text = location;
                txtLocationPlace.Text = location;

                bool found = false;
                foreach (TabPage tp in tabControl1.TabPages)
                {
                    if (tp.Name.ToUpper() == "TABPAGE2")
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    tabControl1.TabPages.Add(tabPage2);
                    tabControl1.TabPages.Remove(tabPage1);
                }
                tabPage2.Text = tabTitle;
                this.Text = "Demographic Details for " + tabTitle;
            }
            else
            {
                whichTab = what;
                txtAddress.Text = address;
                txtCity.Text = city;
                txtCounty.Text = county;
                txtState.Text = state;
                txtZip.Text = zip;
                txtPhone.Text = phone;

                bool found = false;
                foreach (TabPage tp in tabControl1.TabPages)
                {
                    if (tp.Name.ToUpper() == "TABPAGE1")
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    tabControl1.TabPages.Add(tabPage1);
                    tabControl1.TabPages.Remove(tabPage2);
                }
                tabPage1.Text = tabTitle;
                this.Text = "Demographic Details for " + tabTitle;
            }
            //if ( !String.IsNullOrWhiteSpace ( column ))
            //{
            //    if (column.ToUpper() == "DEPFIRSTNAME")
            //        txtFirstName.Focus();
            //    else if (column.ToUpper() == "DEPLASTNAME")
            //        txtLastName.Focus();
            //}
        }
        /****************************************************************************************/
        public void FireEventFunDemoLoad(string title, string firstName, string middleName, string lastName, string suffix, string address, string city, string county, string state, string zip, string phone)
        {
            txtTitle.Text = title;
            txtFirstName.Text = firstName;
            txtMiddleName.Text = middleName;
            txtLastName.Text = lastName;
            txtSuffix.Text = suffix;
            txtAddress2.Text = address;
            txtCity2.Text = city;
            txtCounty2.Text = county;
            txtState2.Text = state;
            txtZip2.Text = zip;
            txtPhone2.Text = phone;
        }
        /****************************************************************************************/
        public string FireEventFunDemo(string what )
        {
            if (what.ToUpper() == "ADDRESS")
                return _address;
            else if (what.ToUpper() == "CITY")
                return _city;
            else if (what.ToUpper() == "COUNTY")
                return _county;
            else if (what.ToUpper() == "STATE")
                return _state;
            else if (what.ToUpper() == "ZIP")
                return _zip;
            else if (what.ToUpper() == "PHONE")
                return _phone;
            return "";
        }
        /***********************************************************************************************/
        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (FunDemoDone == null)
            {
                this.Close();
                return;
            }
            FunDemoDone.Invoke("Cancel", "", "", "", "", "", "", "", "", "", "", "");
            this.Hide();
            //this.Close();
            return;
        }
        /***********************************************************************************************/
        private void btnAccept_Click(object sender, EventArgs e)
        {
            _address = txtAddress.Text;
            _city = txtCity.Text;
            _county = txtCounty.Text;
            _state = txtState.Text;
            _zip = txtZip.Text;
            _phone = txtPhone.Text;
            this.DialogResult = DialogResult.OK;
            OnDone();
            return;
        }
        /***********************************************************************************************/
        private void txtName_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                txtAddress.Focus();
        }
        /***********************************************************************************************/
        private void txtAddress_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                txtCity.Focus();
        }
        /***********************************************************************************************/
        private void txtCity_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                txtCounty.Focus();
        }
        /***********************************************************************************************/
        private void txtState_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                txtZip.Focus();
        }
        /***********************************************************************************************/
        private void txtZip_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                txtPhone.Focus();
        }
        /***********************************************************************************************/
        private void txtCounty_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                txtState.Focus();
        }
        /***********************************************************************************************/
        private void txtPhone_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                btnAccept.Focus();
        }
        /***************************************************************************************/
        public void fireDemoDone ()
        {
            OnDone();
        }
        /***************************************************************************************/
        public delegate void d_void_eventdone_string(string title, string firstName, string middleName, string lastName, string suffix, string name, string address, string city, string county, string state, string zip, string phone );
        public event d_void_eventdone_string FunDemoDone;
        protected void OnDone()
        {
            if (FunDemoDone != null)
            {
                string address = "";
                string city = "";
                string county = "";
                string state = "";
                string zip = "";
                string phone = "";

                string title = txtTitle.Text.Trim();
                string firstName = txtFirstName.Text.Trim();
                string middleName = txtMiddleName.Text.Trim();
                string lastName = txtLastName.Text.Trim();
                string suffix = txtSuffix.Text.Trim();
                string name = txtName.Text.Trim();
                if ( whichTab.ToUpper() == "PLACE")
                {
                    address = txtAddress.Text.Trim();
                    city = txtCity.Text.Trim();
                    county = txtCounty.Text.Trim();
                    state = txtState.Text.Trim();
                    zip = txtZip.Text.Trim();
                    phone = txtPhone.Text.Trim();
                }
                else
                {
                    address = txtAddress2.Text.Trim();
                    city = txtCity2.Text.Trim();
                    county = txtCounty2.Text.Trim();
                    state = txtState2.Text.Trim();
                    zip = txtZip2.Text.Trim();
                    phone = txtPhone2.Text.Trim();

                    name = G1.BuildFullName(title, firstName, middleName, lastName, suffix);
                }

                FunDemoDone.Invoke( title, firstName, middleName, lastName, suffix, name, address, city, county, state, zip, phone );
                this.Hide();
                //this.Close();
            }
        }
        /***********************************************************************************************/
        private void button1_Click(object sender, EventArgs e)
        {
            FunDemoDone.Invoke("Cancel", "", "", "", "", "", "", "", "", "", "", "");
            this.Hide();
            //this.Close();
            return;
        }
        /***********************************************************************************************/
        private void button2_Click(object sender, EventArgs e)
        {
            _address = txtAddress.Text;
            _city = txtCity.Text;
            _county = txtCounty.Text;
            _state = txtState.Text;
            _zip = txtZip.Text;
            _phone = txtPhone.Text;
            this.DialogResult = DialogResult.OK;
            OnDone();
            return;
        }
        /***********************************************************************************************/
        private void txtTitle_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                txtFirstName.Focus();
        }
        /***********************************************************************************************/
        private void txtFirstName_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                txtMiddleName.Focus();
        }
        /***********************************************************************************************/
        private void txtMiddleName_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                txtLastName.Focus();
        }
        /***********************************************************************************************/
        private void txtLastName_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                txtSuffix.Focus();
        }
        /***********************************************************************************************/
        private void txtSuffix_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                txtAddress2.Focus();
        }
        /***********************************************************************************************/
        private void txtAddress2_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                txtCity2.Focus();
        }
        /***********************************************************************************************/
        private void txtCity2_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                txtCounty2.Focus();
        }
        /***********************************************************************************************/
        private void txtCounty2_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                txtState2.Focus();
        }
        /***********************************************************************************************/
        private void txtState2_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                txtZip2.Focus();
        }
        /***********************************************************************************************/
        private void txtZip2_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                txtPhone2.Focus();
        }
        /***********************************************************************************************/
        private void txtPhone2_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                button2.Focus();
        }
        /***********************************************************************************************/
        private void FuneralDemo_FormClosed(object sender, FormClosedEventArgs e)
        {
            OnDone();
        }
        /***********************************************************************************************/
    }
}