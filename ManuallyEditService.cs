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
    public partial class ManuallyEditService : DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        private string workType = "";
        private string workService = "";
        private double workPrice = 0D;
        private double workCurrentPrice = 0D;
        private bool workSalesTax = false;
        /***********************************************************************************************/
        public string wType { get { return workType; } }
        public string wService { get { return workService; } }
        public double wPrice { get { return workPrice; } }
        public double wCurrentPrice { get { return workCurrentPrice; } }
        public bool wSalesTax { get { return workSalesTax; } }

        /***********************************************************************************************/
        public ManuallyEditService( string type, string service, double price, double currentPrice, bool salesTax )
        {
            InitializeComponent();
            workType = type;
            workService = service;
            workPrice = price;
            workCurrentPrice = currentPrice;
            workSalesTax = salesTax;
        }
        /***********************************************************************************************/
        private void ManuallyEditService_Load(object sender, EventArgs e)
        {
            //if ( workType.ToUpper() == "CASH ADVANCE" )
            //{
            //    lblCurrentPrice.Hide();
            //    txtCurrentPrice.Hide();
            //}
            txtType.Text = workType;
            txtService.Text = workService;

            string str = G1.ReformatMoney(workPrice);
            str = str.Replace("$", "");
            str = str.Replace(",", "");
            txtCustomerPrice.Text = str;

            str = G1.ReformatMoney(workCurrentPrice);
            str = str.Replace("$", "");
            str = str.Replace(",", "");
            txtCurrentPrice.Text = str;

            if (workSalesTax)
                chkSalesTax.Checked = true;
        }
        /***************************************************************************************/
        public delegate void d_void_eventdone_string(string contract);
        public event d_void_eventdone_string SelectDone;
        protected void OnSelectDone( string contract )
        {
            SelectDone?.Invoke(contract);
        }
        /***********************************************************************************************/
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
        /***********************************************************************************************/
        private void btnAdd_Click(object sender, EventArgs e)
        {
            if ( txtService.Text.ToUpper() == "PACKAGE DISCOUNT" ||
                 txtService.Text.ToUpper() == "PACKAGE PRICE" ||
                 txtService.Text.ToUpper() == "TOTAL LISTED PRICE" )
            {
                MessageBox.Show("***ERROR*** You Cannot Change the Details of " + txtService.Text + "!", "Package Details Protected Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }
            workType = txtType.Text;
            workService = txtService.Text;
            workPrice = txtCustomerPrice.Text.ObjToDouble();
            workCurrentPrice = txtCurrentPrice.Text.ObjToDouble();
            workSalesTax = chkSalesTax.Checked;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        /***********************************************************************************************/
        private void SomethingChanged ()
        {
            double money = txtCustomerPrice.Text.ObjToDouble();
            string str = G1.ReformatMoney(money);
            str = str.Replace("$", "");
            str = str.Replace(",", "");
            txtCustomerPrice.Text = str;

            money = txtCurrentPrice.Text.ObjToDouble();
            str = G1.ReformatMoney(money);
            str = str.Replace("$", "");
            str = str.Replace(",", "");
            txtCurrentPrice.Text = str;
        }
        /***********************************************************************************************/
        private void txtCustomerPrice_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                SomethingChanged();
        }
        /***********************************************************************************************/
        private void txtCurrentPrice_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                SomethingChanged();
        }
        /***********************************************************************************************/
        private void chkSalesTax_CheckedChanged(object sender, EventArgs e)
        {
            SomethingChanged();
        }
        /***********************************************************************************************/
        private void txtType_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                txtService.Focus();
            }
        }
        /***********************************************************************************************/
        private void txtService_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                txtCustomerPrice.Focus();
            }
        }
        /***********************************************************************************************/
        private void txtCustomerPrice_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                txtCurrentPrice.Focus();
            }
        }
        /***********************************************************************************************/
        private void txtCurrentPrice_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                chkSalesTax.Focus();
            }
        }
        /***********************************************************************************************/
    }
}