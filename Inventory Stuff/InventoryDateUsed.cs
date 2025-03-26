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
    public partial class InventoryDateUsed : Form
    {
        /***********************************************************************************************/
        public InventoryDateUsed()
        {
            InitializeComponent();
        }
        /***********************************************************************************************/
        private void InventoryDateUsed_Load(object sender, EventArgs e)
        {
            LoadData();
        }
        /*******************************************************************************************/
        private void LoadData ()
        {
            this.Cursor = Cursors.WaitCursor;

            string cmd = "SELECT* FROM inventory i JOIN fcust_extended f WHERE i.`serviceId` = f.`serviceId` AND i.`DateUsed` <> f.`serviceDate` AND i.`serviceId` <> '';";
            DataTable dt = G1.get_db_data(cmd);

            G1.NumberDataTable(dt);
            dgv.DataSource = dt;

            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void btnFixMisMatch_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            DateTime date = DateTime.Now;
            string record = "";
            int count = 0;
            DataTable dt = (DataTable)dgv.DataSource;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                date = dt.Rows[i]["serviceDate"].ObjToDateTime();
                if (date.Year > 1000)
                {
                    record = dt.Rows[i]["record"].ObjToString();
                    G1.update_db_table("inventory", "record", record, new string[] { "DateUsed", date.ToString("MM/dd/yyyy") });
                    count++;
                }
            }
            this.Cursor = Cursors.Default;
            MessageBox.Show("***INFO*** " + count.ToString() + " fixed!", "Fix Date Used Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);

            LoadData();
        }
        /***********************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();

            string contractNumber = dr["contractNumber"].ObjToString();
            if (String.IsNullOrWhiteSpace(contractNumber))
                return;

            this.Cursor = Cursors.WaitCursor;

            EditCust custForm = new EditCust(contractNumber);
            custForm.Tag = contractNumber;
            custForm.Show();

            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
    }
}
