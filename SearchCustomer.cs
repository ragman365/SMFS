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
    public partial class SearchCustomer : DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        public string searchContract = "";
        public string searchLastName = "";
        public string searchFirstName = "";
        private bool allOkay = false;
        /***********************************************************************************************/
        public SearchCustomer()
        {
            InitializeComponent();
            searchContract = "";
            searchFirstName = "";
            searchLastName = "";
        }
        /***********************************************************************************************/
        private void SearchCustomer_Load(object sender, EventArgs e)
        {
        }
        /***********************************************************************************************/
        private void txtName_KeyUp(object sender, KeyEventArgs e)
        {
            LookupCustomer();
        }
        /***********************************************************************************************/
        private void txtFirstName_KeyUp(object sender, KeyEventArgs e)
        {
            LookupCustomer();
        }
        /***********************************************************************************************/
        private void LookupCustomer()
        {
            string lastName = txtLastName.Text.Trim().ToUpper();
            if (lastName.Length < 2)
                return;
            string firstName = txtFirstName.Text.Trim().ToUpper();
            string cmd = "Select * from `customers` where `lastName` LIKE '" + lastName + "%' ";
            if ( !String.IsNullOrWhiteSpace ( firstName ))
                cmd += " AND `firstName` like '" + firstName + "%' ";
            cmd += ";";
            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("num");
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
        }
        /***********************************************************************************************/
        private void repositoryItemCheckEdit2_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string contract = dr["contractNumber"].ObjToString();
            string firstName = dr["firstName"].ObjToString();
            string lastName = dr["lastName"].ObjToString();
            searchContract = contract;
            searchFirstName = firstName;
            searchLastName = lastName;
            allOkay = true;
            this.Close();
        }
        /***********************************************************************************************/
        private void SearchCustomer_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (allOkay)
                this.DialogResult = DialogResult.OK;
            else
                this.DialogResult = DialogResult.Cancel;
        }
        /***********************************************************************************************/
    }
}