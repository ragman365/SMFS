using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using GeneralLib;
using DevExpress.XtraPrinting;
using DevExpress.Utils;
using System.Net.Mail;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.IO;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class OrdersMatch : Form
    {
        private string workFunRec = "";
        private string workName = "";
        private bool workNeeded = false;
        private bool modified = false;
        private bool loading = true;
        private DataTable originalDt = null;
        private bool pendingModified = false;
        private Color SaveBackColor = SystemColors.Control;
        private Color NewBackColor = Color.LightGreen;
        /***********************************************************************************************/
        public OrdersMatch(string record = "", bool needed = false)
        {
            int w = this.Width;

            InitializeComponent();

            workFunRec = record;
            workNeeded = needed;
            int width = this.Width;
            width = this.Width + 100;
            int height = this.Height;
            this.SetBounds(this.Left, this.Top, width, height);
        }
        /***********************************************************************************************/
        private void OrdersMatch_Load(object sender, EventArgs e)
        {
            btnAdd.Hide();
            btnDelete.Hide();
            btnShowMisMatch.Hide();

            getLocations();

            string cmd = "Select * from `inventory_orders` where `matched` <> 'MATCHED';";
            DataTable dt = G1.get_db_data(cmd);
            G1.NumberDataTable(dt);
            originalDt = dt;
            dgv.DataSource = dt;
            //btnShowMisMatch.Text = "Show Orders";
            //btnShowMisMatch.Refresh();
            //txtSerialNumber.Hide();
            //btnMatch.Hide();
            //label3.Hide();


            //string cmd = "Select * from `inventory_orders` where `serialNumber` <> '' AND `deliveredSerialNumber` = '';";
            //DataTable dt = G1.get_db_data(cmd);
            //G1.NumberDataTable(dt);
            //originalDt = dt;
            //dgv.DataSource = dt;
        }
        /*******************************************************************************************/
        private void LoadData ()
        {
            //string cmd = "Select * from `inventory_orders` where `serialNumber` <> '' AND `deliveredSerialNumber` = '';";
            //DataTable dt = G1.get_db_data(cmd);
            //G1.NumberDataTable(dt);
            //originalDt = dt;
            //dgv.DataSource = dt;

            string cmd = "Select * from `inventory_orders` where `matched` <> 'MATCHED';";
            DataTable dt = G1.get_db_data(cmd);

            G1.NumberDataTable(dt);
            originalDt = dt;
            dgv.DataSource = dt;

        }
        /***********************************************************************************************/
        private DataTable _LocationList;
        private void getLocations()
        {
            string cmd = "SELECT `LocationCode` FROM `inventory` GROUP BY `LocationCode` ASC;";
            _LocationList = G1.get_db_data(cmd);

            string str = "";

            for (int i = 0; i < _LocationList.Rows.Count; i++ )
            {
                str = _LocationList.Rows[i]["LocationCode"].ObjToString();
                if (String.IsNullOrWhiteSpace(str))
                    _LocationList.Rows.RemoveAt(i);
            }

            chkComboLocation.Properties.DataSource = _LocationList;
            if (!String.IsNullOrWhiteSpace(workName))
            {
                chkComboLocation.EditValue = workName;
                chkComboLocation.Text = workName;
            }
        }
        /*******************************************************************************************/
        private string getLocationQuery()
        {
            string procLoc = "";
            string[] locIDs = this.chkComboLocation.EditValue.ToString().Split('|');
            for (int i = 0; i < locIDs.Length; i++)
            {
                if (!String.IsNullOrWhiteSpace(locIDs[i]))
                {
                    if (procLoc.Trim().Length > 0)
                        procLoc += ",";
                    procLoc += "'" + locIDs[i].Trim() + "'";
                }
            }
            return procLoc.Length > 0 ? " `locationCode` IN (" + procLoc + ") " : "";
        }
        /*******************************************************************************************/
        private void textBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if ( e.KeyCode == Keys.Enter )
            {
                MatchSerialNumber();
            }
        }
        /***********************************************************************************************/
        private void btnMatch_Click(object sender, EventArgs e)
        {
            MatchSerialNumber();
        }
        /***********************************************************************************************/
        private void MatchSerialNumber ()
        {
            string match = txtSerialNumber.Text.Trim();
            if (String.IsNullOrWhiteSpace(match))
                return;

            string cmd = "Select * from `inventory_orders` where `serialNumber` = '" + match + "';";
            DataTable dx = G1.get_db_data(cmd);
            if ( dx.Rows.Count > 0 )
            {
                string description = dx.Rows[0]["CasketDescription"].ObjToString();
                string location = dx.Rows[0]["LocationCode"].ObjToString();
                InventoryMatch matchForm = new InventoryMatch(match, description, location );
                matchForm.SelectDone += MatchForm_SelectDone;
                matchForm.ShowDialog();
            }
            else
            {
                InventoryMatch matchForm = new InventoryMatch(match, "", "" );
                matchForm.SelectDone += MatchForm_SelectDone;
                matchForm.ShowDialog();
            }
        }
        /***********************************************************************************************/
        private void MatchForm_SelectDone(string status, string serialNumber, string description, string location, string workRecord)
        {
            if (status.ToUpper() == "CANCEL")
                return;
            bool matched = false;
            DataTable dt = (DataTable)dgv.DataSource;
            //if (dt.Rows.Count <= 0)
            //    return;

            if ( status == "ACCEPT")
            {
                string cmd = "Select * from `inventory_orders` where `serialNumber` = '" + serialNumber + "';";
                DataTable dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    string record = dx.Rows[0]["record"].ObjToString();
                    G1.update_db_table("inventory_orders", "record", record, new string[] { "deliveredSerialNumber", serialNumber, "matched", "MATCHED", "DateDelivered", DateTime.Now.ToString("MM/dd/yyyy") });

                    DataRow[] dRows = dt.Select("serialNumber='" + serialNumber + "'");
                    if (dRows.Length > 0)
                    {
                        dRows[0]["deliveredSerialNumber"] = serialNumber;
                        dRows[0]["matched"] = "MATCHED";
                        dRows[0]["DateDelivered"] = G1.DTtoMySQLDT(DateTime.Now);
                        //dt.Rows.Remove(dRows[0]);
                        gridMain.RefreshData();
                        gridMain.RefreshEditor(true);
                        dgv.RefreshDataSource();
                        dgv.Refresh();

                        if (record == workRecord)
                            workRecord = "";

                        matched = true;
                    }
                }
                if ( !String.IsNullOrWhiteSpace ( workRecord ))
                {
                    DataRow[] dRows = dt.Select("record='" + workRecord + "'");
                    if (dRows.Length > 0)
                    {
                        if (matched)
                        {
                            G1.delete_db_table ( "inventory_orders", "record", workRecord );
                            dt.Rows.Remove(dRows[0]);
                            gridMain.RefreshData();
                            gridMain.RefreshEditor(true);
                            dgv.RefreshDataSource();
                            dgv.Refresh();
                        }
                        else
                        {
                            DateTime d = DateTime.Now;
                            string orderdate = d.ToString("yyyy-MM-dd");
                            string user = LoginForm.username.Trim();

                            G1.update_db_table("inventory_orders", "record",workRecord, new string[] { "deliveredSerialNumber", serialNumber, "qty", "1", "qtyPending", "1", "LocationCode", location, "CasketDescription", description, "CasketCode", "" });
                            G1.update_db_table("inventory_orders", "record", workRecord, new string[] { "orderedby", user, "DateOrdered", orderdate, "replacement", "", "matched", "MISMATCHED", "DateDelivered", DateTime.Now.ToString("MM/dd/yyyy") });
                            dRows[0]["deliveredSerialNumber"] = serialNumber;
                            dRows[0]["matched"] = "MISMATCHED";
                            dRows[0]["DateDelivered"] = G1.DTtoMySQLDT(DateTime.Now);
                            gridMain.RefreshData();
                            gridMain.RefreshEditor(true);
                            dgv.RefreshDataSource();
                            dgv.Refresh();
                        }
                    }
                }
            }
            else if ( status == "ADD" )
            {
                string record = G1.create_record("inventory_orders", "LocationCode", "-1");
                if (G1.BadRecord("inventory_orders", record))
                    return;
                DateTime d = DateTime.Now;
                string orderdate = d.ToString("yyyy-MM-dd");
                string user = LoginForm.username.Trim();

                G1.update_db_table("inventory_orders", "record", record, new string[] { "deliveredSerialNumber", serialNumber, "qty", "1", "qtyPending", "1", "LocationCode", location, "CasketDescription", description, "CasketCode", "" });
                G1.update_db_table("inventory_orders", "record", record, new string[] { "orderedby", user, "DateOrdered", orderdate, "replacement", "", "matched", "MISMATCHED", "DateDelivered", DateTime.Now.ToString("MM/dd/yyyy") });

                LoadData();
                chkComboLocation_EditValueChanged(null, null);
            }
            txtSerialNumber.Text = "";
            txtSerialNumber.Refresh();
        }
        /***********************************************************************************************/
        private int mainCount = 0;
        private void chkComboLocation_EditValueChanged(object sender, EventArgs e)
        {
            string buttonName = btnShowMisMatch.Text.Trim().ToUpper();
            if (String.IsNullOrWhiteSpace(buttonName))
                return;

            //string cmd = "Select * from `inventory_orders` where `serialNumber` <> '' AND `deliveredSerialNumber` = '' ";

            //if ( buttonName.ToUpper() == "SHOW ORDERS")
            //    cmd = "Select * from `inventory_orders` where `serialNumber` = '' OR `matched` = 'MISMATCH' ";

            string cmd = "Select * from `inventory_orders` where `matched` <> 'MATCHED' ";

            string locations = getLocationQuery();
            if (!String.IsNullOrWhiteSpace(locations))
                cmd += " AND " + locations;


            cmd += " ORDER by `LocationCode`, `DateOrdered` DESC; ";
            DataTable dt = G1.get_db_data(cmd);

            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
        }
        /***********************************************************************************************/
        private void btnShowMisMatch_Click(object sender, EventArgs e)
        {
            string name = btnShowMisMatch.Text.Trim().ToUpper();
            if (name == "SHOW MISMATCHES")
            {
                string cmd = "Select * from `inventory_orders` where `serialNumber` = '' AND `matched` <> 'MISMATCHED';";
                DataTable dt = G1.get_db_data(cmd);
                G1.NumberDataTable(dt);
                originalDt = dt;
                dgv.DataSource = dt;
                btnShowMisMatch.Text = "Show Orders";
                btnShowMisMatch.Refresh();
                txtSerialNumber.Hide();
                btnMatch.Hide();
                label3.Hide();
            }
            else
            {
                btnShowMisMatch.Text = "Show Mismatches";
                btnShowMisMatch.Refresh();
                txtSerialNumber.Show();
                btnMatch.Show();
                label3.Show();
                LoadData();
            }
        }
        /***********************************************************************************************/
        private void gridMain_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv.DataSource;
            try
            {
                string match = dt.Rows[row]["match"].ObjToString().ToUpper();
                if (match.ToUpper() == "MATCHED" )
                {
                    e.Visible = false;
                    e.Handled = true;
                    return;
                }
            }
            catch (Exception ex)
            {
                return;
            }

        }
        /***********************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            string replacement = dr["replacement"].ObjToString();
            if ( !String.IsNullOrWhiteSpace ( replacement) )
            {
                MessageBox.Show("***ERROR*** Orders with Replacement Number cannot be edited", "Edit Item Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }
            string description = dr["CasketDescription"].ObjToString();
            string location = dr["LocationCode"].ObjToString();
            string serialNumber = dr["deliveredSerialNumber"].ObjToString();
            InventoryMatch matchForm = new InventoryMatch(serialNumber, description, location, record );
            matchForm.SelectDone += MatchForm_SelectDone;
            matchForm.ShowDialog();
        }
        /***********************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            G1.ShowHideFindPanel(gridMain);
        }
        /***********************************************************************************************/
    }
}
