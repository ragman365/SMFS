using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.LookAndFeel;
using DevExpress.XtraEditors;
using DevExpress.XtraBars;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Base.ViewInfo;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraPrinting;
using GeneralLib;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class Preference : DevExpress.XtraEditors.XtraForm
    {
        private DataTable preferenceDt = null;
        private bool modified = false;
        private string preferenceModule = "";
        private string preference = "";
        /***********************************************************************************************/
        public Preference( string module, string pref )
        {
            InitializeComponent();
            preferenceModule = module;
            preference = pref;
        }
        /***********************************************************************************************/
        private void Preference_Load(object sender, EventArgs e)
        {
            this.Text = "User Preferences for Module " + preferenceModule + " Preference " + preference;
            LoadData();
        }
        /***********************************************************************************************/
        private void LoadData()
        {
            string cmd = "Select * from `users` u LEFT JOIN `preferenceUsers` p ON u.`userName` = p.`userName` ";
            cmd += "  AND `Module` = '" + preferenceModule + "' AND `Preference` = '" + preference + "';";
            preferenceDt = G1.get_db_data(cmd);
            preferenceDt.Columns.Add("num");
            preferenceDt.Columns.Add("modified");
            G1.NumberDataTable(preferenceDt);
            dgv.DataSource = preferenceDt;
        }
        /***********************************************************************************************/
        private int activeColumnIndex = -1;
        private string cellValue = "";
        private void gridMain_CellValueChanging(object sender, CellValueChangedEventArgs e)
        {
            activeColumnIndex = e.Column.VisibleIndex;
            string columnName = e.Column.FieldName.ObjToString();
            cellValue = e.Value.ObjToString();
            int row = gridMain.FocusedRowHandle;
            DataTable dt = (DataTable)dgv.DataSource;
            dt.Rows[row][columnName] = cellValue;
            gridMain_KeyPress(null, null);
        }
        /***********************************************************************************************/
        private void gridMain_KeyPress(object sender, KeyPressEventArgs e)
        {
            modified = true;
            DataRow dr = gridMain.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            dr["modified"] = "YES";
        }
        /***********************************************************************************************/
        private void Preference_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!modified)
                return;
            DialogResult result = MessageBox.Show("***Question***\nPreference List has been modified!\nWould you like to save your changes?", "Add/Edit Preferences Dialog", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (result == DialogResult.Cancel)
            {
                e.Cancel = true;
                return;
            }
            modified = false;
            if (result == DialogResult.No)
                return;
            UpdatePreferences();
        }
        /***********************************************************************************************/
        private void UpdatePreferences ()
        {
            gridMain.RefreshData();
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();
            string mod = "";
            string userName = "";
            string answer = "";
            string record1 = "";
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                try
                {
                    mod = dt.Rows[i]["modified"].ObjToString();
                    if (mod.ToUpper() != "YES")
                        continue;
                    userName = dt.Rows[i]["userName"].ObjToString();
                    answer = dt.Rows[i]["preferenceAnswer"].ObjToString();
                    record1 = dt.Rows[i]["record1"].ObjToString();
                    if (String.IsNullOrWhiteSpace(record1))
                        record1 = G1.create_record("preferenceusers", "module", "-1");

                    if (string.IsNullOrWhiteSpace(record1))
                        MessageBox.Show("***ERROR*** Creating User " + userName + " Preference Record!");
                    else if (record1 == "-1")
                        MessageBox.Show("***ERROR*** Creating User " + userName + " Preference Record!");
                    else
                    {
                        G1.update_db_table("preferenceUsers", "record", record1, new string[] { "userName", userName, "module", preferenceModule, "preference", preference, "preferenceAnswer", answer });
                    }
                }
                catch ( Exception ex)
                {
                    MessageBox.Show("***ERROR*** Creating/Updating User Preference " + preferenceModule + "/" + preference + "/" + userName + " " + ex.Message.ToString());
                }
            }
        }
        /***********************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (gridMain.OptionsFind.AlwaysVisible == true)
                gridMain.OptionsFind.AlwaysVisible = false;
            else
                gridMain.OptionsFind.AlwaysVisible = true;
        }
        /***********************************************************************************************/
    }
}