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
    public partial class EditInvoiceNumber : DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        private string importLine = "";
        private bool allOkay = false;
        public static string returnLine = "";
        private DataTable workDt = null;
        private bool loading = true;
        private string CustomerName = "";
        /***********************************************************************************************/
        public EditInvoiceNumber( DataTable dt)
        {
            //importLine = line;
            workDt = dt;
            InitializeComponent();
        }
        /***********************************************************************************************/
        private void EditInvoiceNumber_Load(object sender, EventArgs e)
        {
            string invoiceNumber = workDt.Rows[0]["invoiceNumber"].ObjToString();

            txtInvoiceNumber.Text = invoiceNumber;
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
        }
        /***********************************************************************************************/
        private void EditACHLine_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (allOkay)
            {
                string part1 = txtPart1.Text.Trim();
                string part2 = txtPart2.Text.Trim();

                string invoiceNumber = part1 + " " + part2;

                workDt.Rows[0]["invoiceNumber"] = invoiceNumber;
                this.DialogResult = DialogResult.OK;
            }
            else
                this.DialogResult = DialogResult.Cancel;
        }
        /***********************************************************************************************/
    }
}