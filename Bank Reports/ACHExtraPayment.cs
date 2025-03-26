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
using DevExpress.XtraGrid.Views.Base;
using DevExpress.Xpo.Helpers;
using System.IO;
using ExcelLibrary.BinaryFileFormat;
using System.Security.Cryptography;
using DevExpress.XtraPrinting;
using DevExpress.Utils;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class ACHExtraPayment : DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        private DataTable workDt = null;
        private DateTime workDate = DateTime.Now;
        private DataTable _saveDt = null;
        public DataTable ACH_Answer { get { return _saveDt; } }

        /***********************************************************************************************/
        public ACHExtraPayment( DataTable dt, DateTime date )
        {
            InitializeComponent();
            workDt = dt;
            workDate = date;
        }
        /***********************************************************************************************/
        private void ACHExtraPayment_Load(object sender, EventArgs e)
        {
            string cmd = "Select * from `ach` where `contractNumber` = 'XyzzY';";
            DataTable dx = G1.get_db_data(cmd);
            dx.Columns.Add("effectiveDate");
            dx.Columns.Add("name");
            dx.Columns.Add("ID");
            dx.Columns.Add("DebitCredit");
            if ( G1.get_column_number ( dx, "status") < 0 )
                dx.Columns.Add("status");

            dgv.DataSource = dx;
            btnSavePayments.Hide();
            G1.SetupToolTip(picAdd, "Find Customer to Add");
        }
        /***********************************************************************************************/
        private void picAdd_Click(object sender, EventArgs e)
        {
            FastLookup fastForm = new FastLookup("");
            fastForm.ListDone += FastForm_ListDone;
            fastForm.Show();
        }
        /****************************************************************************************/
        private void FastForm_ListDone(string s)
        { // Trust or Policy Selected
            if (String.IsNullOrWhiteSpace(s))
                return;

            string source = "";
            string amount = "";
            string account = "";
            string name = "";
            string contractNumber = "";
            string payer = "";
            string policyRecord = "";

            FunPayments.DecodeFastLookup(s, ref contractNumber, ref payer, ref name, ref source, ref amount, ref policyRecord);

            if (source.ToUpper() == "TRUST")
            {
            }
            else
            {
            }

            string cmd = "";
            DataTable dx = null;
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dRow = dt.NewRow();
            if (source.ToUpper() == "TRUST")
            {
                dRow["contractNumber"] = contractNumber;
                dRow["name"] = name;
            }
            else
            {
                dRow["contractNumber"] = contractNumber;
                dRow["name"] = name;
                cmd = "Select * from `icustomers` where `contractNumber` = '" + contractNumber + "';";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                    dRow["payer"] = dx.Rows[0]["payer"].ObjToString();
            }

            cmd = "Select * from `ach` where `contractNumber` = '" + contractNumber + "';";
            dx = G1.get_db_data(cmd);

            if (dx.Rows.Count > 0)
            {
                dRow["routingNumber"] = dx.Rows[0]["routingNumber"].ObjToString();
                dRow["accountNumber"] = dx.Rows[0]["accountNumber"].ObjToString();
                dRow["acctType"] = dx.Rows[0]["acctType"].ObjToString();
                string str = dx.Rows[0]["payment"].ObjToString();
                str = str.Replace("$", "");
                str = str.Replace(",", "");
                if (G1.validate_numeric(str))
                {
                    double payment = str.ObjToDouble();
                    str = G1.ReformatMoney(payment);
                    dRow["payment"] = str;
                }
                dRow["code"] = dx.Rows[0]["code"].ObjToString();
                dRow["frequencyInMonths"] = dx.Rows[0]["frequencyInMonths"].ObjToString();
                dRow["dayOfMonth"] = dx.Rows[0]["dayOfMonth"].ObjToString();
                dRow["DebitCredit"] = "Debit";
                dRow["effectiveDate"] = workDate.ToString("MM/dd/yyyy");
                dRow["ID"] = GenerateACH.GenerateRandomId(10);
            }

            dt.Rows.Add(dRow);
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            dgv.Refresh();
            gridMain_CellValueChanged(null, null);
            int row = dt.Rows.Count - 1;
            gridMain.FocusedRowHandle = row;
            btnSavePayments.Show();
        }
        /***********************************************************************************************/
        private void gridMain_CellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            if (e == null)
                return;
            int rowHandle = gridMain.FocusedRowHandle;
            DataRow dr = gridMain.GetFocusedDataRow();
            if (dr == null)
                return;

            if ( e.Column.FieldName.ToUpper() == "PAYMENT")
            {
                string str = e.Value.ObjToString();
                str = str.Replace("$", "");
                str = str.Replace(",", "");
                if ( G1.validate_numeric ( str))
                {
                    double payment = str.ObjToDouble();
                    str = "$" + G1.ReformatMoney(payment);
                    dr["payment"] = str;
                }
            }

            gridMain.RefreshData();
            btnSavePayments.Show();
        }
        /***********************************************************************************************/
        private bool closeOk = false;
        private void btnSavePayments_Click(object sender, EventArgs e)
        {
            closeOk = true;
            SavePayments();
            this.Close();
        }
        /***********************************************************************************************/
        private void SavePayments ()
        {
            _saveDt = (DataTable)dgv.DataSource;
            btnSavePayments.Hide();
        }
        /***********************************************************************************************/
        private void ACHExtraPayment_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (btnSavePayments.Visible)
            {
                DialogResult result = MessageBox.Show("Changes Made! Do you want to honor these changes?", "Data Modified Dialog", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation);
                if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
                else if (result == DialogResult.No)
                    return;
                SavePayments();
                this.DialogResult = DialogResult.OK;
            }
            else if (closeOk)
                this.DialogResult = DialogResult.OK;
        }
        /***********************************************************************************************/
    }
}