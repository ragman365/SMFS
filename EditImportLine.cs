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
    public partial class EditImportLine : DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        private string importLine = "";
        private bool allOkay = false;
        public static string returnLine = "";
        private DataTable workDt = null;
        private int workRow = 0;
        private bool workTheFirst = false;
        private DateTime workEffectiveDate = DateTime.Now;
        private int theFirstBeginning = 0;
        private bool loading = true;
        /***********************************************************************************************/
        public EditImportLine( DataTable dt, int row, string line, DateTime effectiveDate, bool theFirst = false )
        {
            importLine = line;
            workDt = dt;
            workRow = row;
            workTheFirst = theFirst;
            workEffectiveDate = effectiveDate;
            InitializeComponent();
        }
        /***********************************************************************************************/
        private void EditImportLine_Load(object sender, EventArgs e)
        {
            if (String.IsNullOrWhiteSpace(importLine))
                return;
            lblImportLine.Text = importLine;

            string code = importLine.Substring(0, 2);
            txtCode.Text = code;

            string location = importLine.Substring(2, 2);
            txtLocation.Text = location;

            string cnum = importLine.Substring(4, 10);
            cnum = cnum.TrimStart('0');
            cnum = cnum.TrimStart('o');
            cnum = cnum.TrimStart('O');
            txtContract.Text = cnum;

            int idx = 0;
            int length = importLine.Length;

            idx = length - 8 - 7;
            string str = "";
            if ( workTheFirst )
                str = importLine.Substring(idx, 7);
            else
                str = importLine.Substring(21, 7);
            double payment = str.ObjToDouble() / 100.0D;
            payment = G1.RoundValue(payment);
            str = G1.ReformatMoney(payment);
            txtPayment.Text = str;

            idx = length - 8 - 7 - 7;
            if ( workTheFirst )
                str = importLine.Substring(idx, 7);
            else
                str = importLine.Substring(14, 7);
            theFirstBeginning = idx;
            payment = str.ObjToDouble() / 100.0D;
            payment = G1.RoundValue(payment);
            str = G1.ReformatMoney(payment);
            txtExpected.Text = str;

            string date = "";
            idx = length - 8;
            if (workTheFirst)
            {
                date = importLine.Substring(idx).Trim();
                date = workEffectiveDate.ToString("yyyyMMdd");
            }
            else
                date = importLine.Substring(28).Trim();
            txtDate.Text = date;
            loading = false;
            BuildNewLine();
        }
        /***********************************************************************************************/
        private void btnAccept_Click(object sender, EventArgs e)
        {
            allOkay = true;
//            workDt.Rows[workRow]["line"] = lblResultLine.Text;
            returnLine = lblResultLine.Text;
            this.Close();
        }
        /***********************************************************************************************/
        private void text_TextChanged(object sender, EventArgs e)
        {
            BuildNewLine();
        }
        /***********************************************************************************************/
        private void BuildNewLine()
        {
            if (loading)
                return;
            string code = txtCode.Text.Trim(); // 0 for 2
            string location = txtLocation.Text.Trim(); // 2 for 2

            string contract = txtContract.Text.Trim(); //4 for 10

            string expected = txtExpected.Text; // 14 for 7
            expected = expected.Replace("$", "");
            expected = expected.Replace(".", "");
            expected = expected.Replace(",", "");

            string payment = txtPayment.Text; // 21 for 7
            payment = payment.Replace("$", "");
            payment = payment.Replace(".", "");
            payment = payment.Replace(",", "");

            string date = txtDate.Text; // 28 for 8

            string newline = code + location;

            if (contract.Length < 10)
                contract = "0000000000".Substring(contract.Length) + contract;
            newline += contract;
            
            if ( workTheFirst )
            {
                newline += importLine.Substring(14, theFirstBeginning - 14);
            }

            if (expected.Length < 7)
                expected = "0000000".Substring(expected.Length) + expected;
            newline += expected;

            if ( payment.Length < 7)
                payment = "0000000".Substring(payment.Length) + payment;
            newline += payment;

            newline += date;

            lblResultLine.Text = newline;
        }
        /***********************************************************************************************/
        private void EditImportLine_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (allOkay)
                this.DialogResult = DialogResult.OK;
            else
                this.DialogResult = DialogResult.Cancel;
        }
        /***********************************************************************************************/
    }
}