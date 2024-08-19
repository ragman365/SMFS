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
    public partial class NewInsurance : DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        public NewInsurance()
        {
            InitializeComponent();
        }
        /***********************************************************************************************/
        private void NewInsurance_Load(object sender, EventArgs e)
        {
        }
        /***************************************************************************************/
        private bool ValidateData ( ref string contract )
        {
            contract = "";
            string payer = this.txtPayer.Text;
            string firstName = this.firstName.Text;
            string lastName = this.lastName.Text;
            if (String.IsNullOrWhiteSpace(payer))
                return false;
            if (String.IsNullOrWhiteSpace(firstName))
                return false;
            if (String.IsNullOrWhiteSpace(lastName))
                return false;
            string cmd = "Select COUNT(*) from `icustomers`;";
            DataTable dx = G1.get_db_data(cmd);
            int totalCustomers = dx.Rows[0][0].ObjToInt32();
            totalCustomers++;
            contract = "ZZ" + totalCustomers.ToString("D7");
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

        }
        /***********************************************************************************************/
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
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
    }
}