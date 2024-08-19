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
    public partial class TieCustomers : DevExpress.XtraEditors.XtraForm
    {
        private string workContract = "";
        private string workPayer = "";
        private bool modified = false;
        /***********************************************************************************************/
        public TieCustomers( string contract, string payer )
        {
            InitializeComponent();
            workContract = contract;
            workPayer = payer;
            SetupTotalsSummary();
        }
        /****************************************************************************************/
        private void SetupTotalsSummary()
        {
            AddSummaryColumn("amount", null);
        }
        /****************************************************************************************/
        private void AddSummaryColumn(string columnName, DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gMain = null, string format = "")
        {
            if (gMain == null)
                gMain = gridMain;
            if (String.IsNullOrWhiteSpace(format))
                format = "${0:0,0.00}";
            gMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            gMain.Columns[columnName].SummaryItem.DisplayFormat = format;
        }
        /***********************************************************************************************/
        private void TieCustomers_Load(object sender, EventArgs e)
        {
            txtContract.Text = workContract;
            if (String.IsNullOrWhiteSpace(workPayer))
            {
                lblPayer.Hide();
                txtPayer.Hide();
                gridMain.Columns["payer"].Visible = false;
            }
            else
                txtPayer.Text = workPayer;
            LoadData();
            modified = false;
            btnSave.Hide();
        }
        /***********************************************************************************************/
        private void LoadData ()
        {
            string cmd = "Select * from `tied_customers` where `contractNumber` = '" + workContract + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0 )
            {
                DataRow dR = dt.NewRow();
                dR["contractNumber"] = workContract;
                dR["tied_cnum"] = workContract;
                dR["payer"] = workPayer;
                dR["amount"] = 0D;
                dt.Rows.Add(dR);
            }
            else
            {
                for ( int i=0; i<dt.Rows.Count; i++)
                {
                    dt.Rows[i]["contractNumber"] = dt.Rows[i]["tied_cnum"].ObjToString();
                }
            }
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
        }
        /***********************************************************************************************/
        private void btnAdd_Click(object sender, EventArgs e)
        {
            FastLookup fastForm = new FastLookup();
            fastForm.ListDone += FastForm_ListDone;
            fastForm.Show();
        }
        /****************************************************************************************/
        private void FastForm_ListDone(string s)
        { // Trust or Policy Selected
            if (String.IsNullOrWhiteSpace(s))
                return;
            string contractNumber = "";
            string payer = "";
            string str = "";
            bool insurance = false;
            bool trust = true;
            string[] Lines = s.Split(':');
            if (Lines.Length <= 1)
                return;
            string source = Lines[0].Trim();
            if ( source.ToUpper() == "INSURANCE")
            {
                trust = false;
                insurance = true;
            }
            string amount = "";
            if (Lines.Length >= 4)
                amount = Lines[3].Trim();
            string account = Lines[1].Trim();
            string name = "";
            if (Lines.Length >= 5)
                name = Lines[4].Trim();
            if ( Lines.Length >= 6 )
            {
                str = Lines[5].Trim();
                if (str.ToUpper() == "PAYER")
                    payer = Lines[6].Trim();
                string cmd = "Select * from `icustomers` where `payer` = '" + payer + "';";
                DataTable dx = G1.get_db_data(cmd);
                if ( dx.Rows.Count <= 0)
                {
                    MessageBox.Show("***ERROR*** Cannot find Contract for Payer " + payer + "!!");
                    return;
                }
                contractNumber = dx.Rows[0]["contractNumber"].ObjToString();
            }

            DataTable dt = (DataTable)dgv.DataSource;

            DataRow dRow = dt.NewRow();
            dRow["contractNumber"] = contractNumber;
            dRow["tied_cnum"] = contractNumber;
            dRow["payer"] = payer;
            dRow["amount"] = 0D;
            dt.Rows.Add(dRow);

            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            dgv.Refresh();
            gridMain_CellValueChanged(null, null);
            int row = dt.Rows.Count - 1;
            gridMain.FocusedRowHandle = row;
        }
        /***********************************************************************************************/
        private void btnSave_Click(object sender, EventArgs e)
        {
            string contractNumber = "";
            string tied_cnum = "";
            string payer = "";
            double amount = 0D;
            string record = "";
            string mainContractNumber = this.txtContract.Text.Trim();
            DataTable dt = (DataTable)dgv.DataSource;
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                contractNumber = mainContractNumber;
                tied_cnum = dt.Rows[i]["tied_cnum"].ObjToString();
                payer = dt.Rows[i]["payer"].ObjToString();
                amount = dt.Rows[i]["amount"].ObjToDouble();
                record = dt.Rows[i]["record"].ObjToString();
                if (String.IsNullOrWhiteSpace(record))
                    record = G1.create_record("tied_customers", "payer", "-1");
                if (G1.BadRecord("tied_customers", record))
                    break;
                G1.update_db_table("tied_customers", "record", record, new string[] { "contractNumber", contractNumber, "tied_cnum", tied_cnum, "payer", payer, "amount", amount.ToString()});
            }
            modified = false;
            btnSave.Hide();
        }
        /***********************************************************************************************/
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            modified = true;
            btnSave.Show();
        }
        /***********************************************************************************************/
        private void TieCustomers_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (modified)
            {
                DialogResult result = MessageBox.Show("***Question***\nData has been modified!\nWould you like to save your changes?", "Data Modified Dialog", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
                if (result == DialogResult.Yes)
                    btnSave_Click(null, null);
            }
        }
        /***********************************************************************************************/
        private void btnDelete_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            DataTable dt = (DataTable)dgv.DataSource;
            string contract = dt.Rows[row]["contractNumber"].ObjToString();
            if (String.IsNullOrWhiteSpace(contract))
                return;
            string primaryContract = this.txtContract.Text.Trim();
            if ( primaryContract == contract && dt.Rows.Count > 1 )
            {
                MessageBox.Show("***ERROR*** You cannot delete the primary contract until it's the last one available to delete!");
                return;
            }
            DialogResult result = MessageBox.Show("***Question***\nAre you sure you want to DELETE the tie for contract (" + contract + ") ?", "Delete Tied Contract Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;
            string record = dr["record"].ObjToString();
            if (!String.IsNullOrWhiteSpace(record))
                G1.delete_db_table("tied_customers", "record", record);
            dt.Rows.RemoveAt(row);
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            dgv.Refresh();
        }
        /***********************************************************************************************/
    }
}