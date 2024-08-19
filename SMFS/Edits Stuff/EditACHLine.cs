using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraPrinting;
using DevExpress.Utils;

using GeneralLib;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class EditACHLine : DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        private string importLine = "";
        private bool allOkay = false;
        public static string returnLine = "";
        private DataTable workDt = null;
        private bool loading = true;
        private string CustomerName = "";
        /***********************************************************************************************/
        public EditACHLine( DataTable dt)
        {
            //importLine = line;
            workDt = dt;
            InitializeComponent();
        }
        /***********************************************************************************************/
        private void EditACHLine_Load(object sender, EventArgs e)
        {
            lblPayer.Hide();
            txtPayer.Hide();

            CustomerName = workDt.Rows[0]["name"].ObjToString();

            string location = workDt.Rows[0]["locationcode"].ObjToString();
            txtLocation.Text = location;

            double payment = workDt.Rows[0]["payment"].ObjToDouble();
            string str = G1.ReformatMoney(payment);
            txtPayment.Text = str;

            string paytype = workDt.Rows[0]["type"].ObjToString();
            string code = workDt.Rows[0]["code"].ObjToString();
            if ( code.ToUpper() == "BAD")
            {
                code = "01";
                if (paytype.ToUpper() == "ACH")
                    code = "02";
            }
            txtCode.Text = code;
            if ( code == "02")
            {
                lblPayer.Show();
                txtPayer.Show();
                string payer = workDt.Rows[0]["payer"].ObjToString();
                txtPayer.Text = payer;
            }

            string cnum = workDt.Rows[0]["cnum"].ObjToString();
            txtContract.Text = cnum;

            double expected = workDt.Rows[0]["expected"].ObjToDouble();
            str = G1.ReformatMoney(payment);
            txtExpected.Text = str;

            string date = workDt.Rows[0]["date"].ObjToString();
            txtDate.Text = date;

            loading = false;
        }
        /***********************************************************************************************/
        private void btnAccept_Click(object sender, EventArgs e)
        {
            allOkay = true;
//            workDt.Rows[workRow]["line"] = lblResultLine.Text;
            this.Close();
        }
        /***********************************************************************************************/
        private void text_TextChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            string cmd = "";
            string contractNumber = "";
            string payer = "";
            DataTable dt = null;
            TextBox tbox = (TextBox)sender;

            string code = txtCode.Text;
            if (code == "02")
            {
                if (tbox.Name.ToUpper() == "TXTCONTRACT")
                {
                    contractNumber = txtContract.Text;
                    cmd = "Select * from `icustomers` where `contractNumber` = '" + contractNumber + "';";
                    dt = G1.get_db_data(cmd);
                    if ( dt.Rows.Count > 0 )
                    {
                        payer = dt.Rows[0]["payer"].ObjToString();
                        CustomerName = dt.Rows[0]["firstName"].ObjToString() + " " + dt.Rows[0]["lastName"].ObjToString();
                        txtPayer.Text = payer;
                        txtPayer.Refresh();
                    }
                }
                else if (tbox.Name.ToUpper() == "TXTPAYER")
                {
                    payer = txtPayer.Text;
                    cmd = "Select * from `icustomers` where `payer` = '" + payer + "';";
                    dt = G1.get_db_data(cmd);
                    if (dt.Rows.Count > 0)
                    {
                        payer = dt.Rows[0]["contractNumber"].ObjToString();
                        CustomerName = dt.Rows[0]["firstName"].ObjToString() + " " + dt.Rows[0]["lastName"].ObjToString();
                        txtContract.Text = payer;
                        txtContract.Refresh();
                    }
                }
            }
            else
            {
                if (tbox.Name.ToUpper() == "TXTCONTRACT")
                {
                    contractNumber = txtContract.Text;
                    cmd = "Select * from `customers` where `contractNumber` = '" + contractNumber + "';";
                    dt = G1.get_db_data(cmd);
                    if (dt.Rows.Count > 0)
                        CustomerName = dt.Rows[0]["firstName"].ObjToString() + " " + dt.Rows[0]["lastName"].ObjToString();
                }
            }
            BuildNewLine();
        }
        /***********************************************************************************************/
        private void BuildNewLine()
        {
            workDt.Rows[0]["name"] = CustomerName;

            string code = txtCode.Text.Trim(); // 0 for 2
            workDt.Rows[0]["code"] = code;

            string location = txtLocation.Text.Trim(); // 2 for 2
            workDt.Rows[0]["locationcode"] = location;

           string contract = txtContract.Text.Trim(); //4 for 10
            if (code == "02")
                contract = txtPayer.Text.Trim();
            workDt.Rows[0]["cnum"] = contract;

            string expected = txtExpected.Text; // 14 for 7
            expected = expected.Replace("$", "");
            expected = expected.Replace(",", "");
            workDt.Rows[0]["expected"] = expected;

            string payment = txtPayment.Text; // 21 for 7
            payment = payment.Replace("$", "");
            payment = payment.Replace(",", "");
            workDt.Rows[0]["payment"] = payment;

            string date = txtDate.Text; // 28 for 8
            workDt.Rows[0]["date"] = date;
        }
        /***********************************************************************************************/
        private void EditACHLine_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (allOkay)
                this.DialogResult = DialogResult.OK;
            else
                this.DialogResult = DialogResult.Cancel;
        }
        /***********************************************************************************************/
    }
}