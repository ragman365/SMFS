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
    public partial class InventoryMatch : DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        private string workSerialNumber = "";
        private string workDescription= "";
        private string workLocation = "";
        private string workRecord = "";
        /***********************************************************************************************/
        public InventoryMatch( string serialNumber, string description, string location, string record = "" )
        {
            InitializeComponent();
            workSerialNumber = serialNumber;
            workDescription = description;
            workLocation = location;
            workRecord = record;
        }
        /***********************************************************************************************/
        private void InventoryMatch_Load(object sender, EventArgs e)
        {
            txtSerialNumber.Text = workSerialNumber;
            txtDescription.Text = workDescription;
            cmbLocation.Text = workLocation;

            if (String.IsNullOrWhiteSpace(workDescription) || !String.IsNullOrWhiteSpace ( workRecord ))
            {
                getLocations();
                if ( String.IsNullOrWhiteSpace ( workRecord ))
                    btnAdd.Text = "Add";
                //lblMatchStatus.Text = "Inventory MISMATCHED!";
                lblMatchStatus.Text = "Order NOT FOUND!";
                if (!String.IsNullOrWhiteSpace(workRecord))
                    txtSerialNumber.Enabled = true;
                else
                    txtSerialNumber.Enabled = false;
            }
            else
            {
                cmbLocation.Enabled = false;
                txtDescription.Enabled = false;
                //lblMatchStatus.Text = "Inventory MATCHED!";
                lblMatchStatus.Text = "Order FOUND!";
                txtSerialNumber.Enabled = false;
            }
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

            for (int i = 0; i < _LocationList.Rows.Count; i++ )
            {
                str = _LocationList.Rows[i]["LocationCode"].ObjToString();
                cmbLocation.Items.Add(str);
            }

            if (!String.IsNullOrWhiteSpace(workLocation))
                cmbLocation.Text = workLocation;
        }
        /***************************************************************************************/
        public delegate void d_void_eventdone_string(string status, string serialNumber, string description, string location, string record );
        public event d_void_eventdone_string SelectDone;
        protected void OnSelectDone( string status, string serialNumber, string description, string location, string record )
        {
            SelectDone?.Invoke(status, serialNumber, description, location, workRecord );
        }
        /***********************************************************************************************/
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            OnSelectDone("CANCEL", workSerialNumber, "", "", workRecord );
            this.Close();
        }
        /***********************************************************************************************/
        private void btnAdd_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            string status = btnAdd.Text.ToUpper();
            string description = txtDescription.Text.Trim();
            string location = cmbLocation.Text.Trim();
            if (!String.IsNullOrWhiteSpace(workRecord))
                workSerialNumber = txtSerialNumber.Text.Trim();

            if ( status == "ADD" )
            {
                if ( String.IsNullOrWhiteSpace ( description ))
                {
                    MessageBox.Show("***ERROR*** Delivered Merchandise CANNOT be added without a Description!!", "Merchandise MisMatch Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    return;
                }
                else if ( String.IsNullOrWhiteSpace ( location ))
                {
                    MessageBox.Show("***ERROR*** Delivered Merchandise CANNOT be added without a Location!!", "Merchandise MisMatch Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    return;
                }
            }
            OnSelectDone(status, workSerialNumber, description, location, workRecord );
            this.Close();
        }
        /***********************************************************************************************/
        private void SomethingChanged ()
        {
        }
        /***********************************************************************************************/
        private void txtService_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                cmbLocation.Focus();
            }
        }
        /***********************************************************************************************/
        private void InventoryMatch_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            OnSelectDone("CANCEL", workSerialNumber, "", "", workRecord );
        }
        /***********************************************************************************************/
    }
}