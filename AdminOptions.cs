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
    public partial class AdminOptions : DevExpress.XtraEditors.XtraForm
    {
        private bool modified = false;
        private bool loading = true;
        /***********************************************************************************************/
        public AdminOptions()
        {
            InitializeComponent();
        }
        /***********************************************************************************************/
        private void AdminOptions_Load(object sender, EventArgs e)
        {
            btnSave.Hide();
            LoadData();
        }
        /***********************************************************************************************/
        private void LoadData()
        {
            VerifyAllOptions();
            string cmd = "Select * from `options`;";
            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("num");
            dt.Columns.Add("mod");
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            modified = false;
            loading = false;
        }
        /***********************************************************************************************/
        private void VerifyAllOptions()
        {
            VerifyOption("Lapse Notices Top Border Lines");
            VerifyOption("Lapse Notices Bottom Border Lines");
            VerifyOption("Lapse Notices Left Border Spaces");
            VerifyOption("Lapse Notices Left Side Width");
            VerifyOption("Lapse Notices Lines Prior to Customer");
            VerifyOption("TOF after X Notices");
            VerifyOption("Use New Interest/Balance Calculations");
            VerifyOption("Trust Reinstate Number");
            VerifyOption("Insurance Reinstate Number");
            VerifyOption("Update Payment Bank Accounts (Y/N)");
        }
        /***********************************************************************************************/
        public static string VerifyOption(string option, string myAnswer = "" )
        {
            string record = "";
            DataTable dt = G1.get_db_data("Select * from `options` where `option` = '" + option + "';");
            if (dt.Rows.Count > 0)
            {
                record = dt.Rows[0]["record"].ObjToString();
                if ( !String.IsNullOrWhiteSpace ( myAnswer))
                    G1.update_db_table("options", "record", record, new string[] { "answer", myAnswer });
                return record;
            }
            record = G1.create_record("options", "answer", "-1");
            if (G1.BadRecord("options", record))
                return "";
            G1.update_db_table("options", "record", record, new string[] { "option", option, "answer", myAnswer });
            return record;
        }
        /***********************************************************************************************/
        public static string GetOptionAnswer(string option)
        {
            string rv = "";
            DataTable dt = G1.get_db_data("Select * from `options` where `option` = '" + option + "';");
            if (dt.Rows.Count <= 0)
                return rv;
            rv = dt.Rows[0]["answer"].ObjToString();
            return rv;
        }
        /***********************************************************************************************/
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            dr["mod"] = "Y";
            modified = true;
            btnSave.Show();
        }
        /***********************************************************************************************/
        private void AdminOptions_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!modified)
                return;
            DialogResult result = MessageBox.Show("***Question***\nOptions have been modified!\nWould you like to save your changes?", "Add/Edit Options Dialog", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (result == DialogResult.Cancel)
            {
                e.Cancel = true;
                return;
            }
            modified = false;
            if (result == DialogResult.No)
                return;
            btnSave_Click(null, null);
        }
        /***********************************************************************************************/
        private void btnSave_Click(object sender, EventArgs e)
        {
            string record = "";
            string option = "";
            string answer = "";
            string mod = "";
            DataTable dt = (DataTable)dgv.DataSource;
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                mod = dt.Rows[i]["mod"].ObjToString();
                if (mod != "Y")
                    continue;
                dt.Rows[i]["mod"] = "";
                record = dt.Rows[i]["record"].ObjToString();
                if (record == "-1")
                    record = G1.create_record("options", "answer", "-1");
                if (G1.BadRecord("options", record))
                    continue;
                option = dt.Rows[i]["option"].ObjToString();
                answer = dt.Rows[i]["answer"].ObjToString();
                G1.update_db_table("options", "record", record, new string[] { "option", option, "answer", answer });
                dt.Rows[i]["record"] = record.ObjToInt32();
            }
            modified = false;
            btnSave.Hide();
            LoginForm.ReadTrust85ForcePayoffOptions();
            this.Close();
        }
        /***********************************************************************************************/
        private void pictureBox3_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dRow = dt.NewRow();
            dRow["option"] = "New Option";
            dRow["mod"] = "Y";
            dRow["record"] = -1;
            dt.Rows.Add(dRow);
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            dgv.Refresh();
            gridMain.MoveLast();
            btnSave.Visible = true;
            btnSave.Refresh();
            gridMain.Columns["option"].OptionsColumn.AllowEdit = true;
        }
        /***********************************************************************************************/
        private void pictureDelete_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt.Rows.Count <= 0)
                return;
            DataRow dr = gridMain.GetFocusedDataRow();
            if (dr == null)
                return;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            string record = dr["record"].ObjToString();
            if (String.IsNullOrWhiteSpace(record))
                return;
            if ( record == "-1")
            {
                dt.Rows.RemoveAt(row);
                G1.NumberDataTable(dt);
                dgv.DataSource = dt;
                dgv.Refresh();
                return;
            }
            try
            {
                G1.delete_db_table("options", "record", record);
                dt.Rows.RemoveAt(row);
                G1.NumberDataTable(dt);
                dgv.DataSource = dt;
                dgv.Refresh();
            }
            catch ( Exception ex )
            {
            }
        }
        /***********************************************************************************************/
    }
}