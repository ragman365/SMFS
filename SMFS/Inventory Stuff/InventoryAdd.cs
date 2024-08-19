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
    public partial class InventoryAdd : DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        public InventoryAdd()
        {
            InitializeComponent();
        }
        /***********************************************************************************************/
        private void InventoryAdd_Load(object sender, EventArgs e)
        {
            getLocations();
        }
        /***********************************************************************************************/
        private void getLocations()
        {
            string cmd = "SELECT `LocationCode` FROM `inventory` GROUP BY `LocationCode` ASC;";
            DataTable _LocationList = G1.get_db_data(cmd);

            string str = "";

            for (int i = _LocationList.Rows.Count - 1; i >= 0; i--)
            {
                str = _LocationList.Rows[i]["LocationCode"].ObjToString();
                if (String.IsNullOrWhiteSpace(str))
                    _LocationList.Rows.RemoveAt(i);
            }

            for (int i = 0; i < _LocationList.Rows.Count; i++)
            {
                str = _LocationList.Rows[i]["LocationCode"].ObjToString();
                cmbLocation.Items.Add(str);
            }
        }
        /***************************************************************************************/
        public delegate void d_void_eventdone_string(string status, string poNumber, string description, string location );
        public event d_void_eventdone_string SelectDone;
        protected void OnSelectDone( string status, string poNumber, string description, string location )
        {
            SelectDone?.Invoke(status, poNumber, description, location );
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
            this.DialogResult = DialogResult.OK;
            string status = btnAdd.Text.ToUpper();
            string poNumber = txtPO.Text.Trim();
            string description = txtDescription.Text.Trim();
            string location = cmbLocation.Text.Trim();
            if ( status == "ADD" )
            {
                if ( String.IsNullOrWhiteSpace ( description ))
                {
                    MessageBox.Show("***ERROR*** New Order CANNOT be added without a Description!!", "New Order Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    return;
                }
                else if ( String.IsNullOrWhiteSpace ( location ))
                {
                    MessageBox.Show("***ERROR*** New Order CANNOT be added without a Location!!", "New Order Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    return;
                }
            }
            OnSelectDone(status, poNumber, description, location );
            this.Close();
        }
        /***********************************************************************************************/
        private void InventoryMatch_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }
        /***********************************************************************************************/
        private void txtPO_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                txtDescription.Focus();
            }
        }
        /***********************************************************************************************/
        private void txtDescription_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                cmbLocation.Focus();
            }
        }
        /***********************************************************************************************/
        private void cmbLocation_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnAdd.Focus();
            }
        }
        /***********************************************************************************************/
        private void btnSelectMerchandise_Click(object sender, EventArgs e)
        {
            InventoryList listForm = new InventoryList(true, false, true );
            listForm.ModuleDone += ListForm_ModuleDone;
            listForm.Show();
        }
        /***********************************************************************************************/
        private void ListForm_ModuleDone(string s)
        {
            string merchandiseRecord = s;
            if (String.IsNullOrWhiteSpace(merchandiseRecord))
                return;
            string cmd = "Select * from `inventorylist` where `record` = '" + s + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return;

            string casketCode = dt.Rows[0]["casketcode"].ObjToString();
            string casketDesc = dt.Rows[0]["casketdesc"].ObjToString();

            txtDescription.Text = casketDesc;
            txtDescription.Refresh();
        }
        /***********************************************************************************************/
    }
}