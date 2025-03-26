using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Base;
using System.Globalization;
using System.IO;
using DevExpress.XtraPrinting;
using DevExpress.Utils;

using GeneralLib;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class FixInsurance : DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        public FixInsurance()
        {
            InitializeComponent();
        }
        /***********************************************************************************************/
        private void FixInsurance_Load(object sender, EventArgs e)
        {
        }
        /***********************************************************************************************/
        private void btnRun_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            string cmd = "Select * from `icustomers` where `touched` = 'A';";
            DataTable dt = G1.get_db_data(cmd);

            dt.Columns.Add ( "payments" );
            dt.Columns.Add ( "policies" );

            string payer = "";
            string ccd = "";
            string list = "";
            string firstName = "";
            string lastName = "";
            DataTable ddx = null;
            int j = 0;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                payer = dt.Rows[i]["payer"].ObjToString();

                try
                {
                    ccd = "SELECT * from `icustomers` where `payer`= '" + payer + "';";
                    ddx = G1.get_db_data(ccd);
                    if (ddx.Rows.Count > 0)
                    {
                        list = "";
                        for (j = 0; j < ddx.Rows.Count; j++)
                        {
                            string contract = ddx.Rows[j]["contractNumber"].ObjToString();
                            list += contract + ",";
                        }
                        list = list.TrimEnd(',');
                        dt.Rows[i]["payments"] = list;
                    }
                }
                catch ( Exception ex)
                {
                }

                firstName = dt.Rows[i]["firstName"].ObjToString();
                lastName = dt.Rows[i]["lastName"].ObjToString();

                try
                {
                    ccd = "SELECT * from `policies` where `payer`= '" + payer + "' AND `firstName` = '" + firstName + "' AND `lastName` = '" + lastName + "' GROUP BY `contractNumber`;";
                    ddx = G1.get_db_data(ccd);
                    if (ddx.Rows.Count > 0)
                    {
                        list = "";
                        for (j = 0; j < ddx.Rows.Count; j++)
                        {
                            string contract = ddx.Rows[j]["contractNumber"].ObjToString();
                            list += contract + ",";
                        }
                        list = list.TrimEnd(',');
                        dt.Rows[i]["policies"] = list;
                    }
                }
                catch ( Exception ex)
                {
                }
            }


            //cmd = "Select * from `icustomers` where `touched` = 'L';";
            //DataTable dx = G1.get_db_data(cmd);

            //for (int i = 0; i < dx.Rows.Count; i++)
            //    dt.ImportRow(dx.Rows[i]);

            //cmd = "Select * from `icustomers` where `touched` = 'D';";
            //dx = G1.get_db_data(cmd);

            //for (int i = 0; i < dx.Rows.Count; i++)
            //    dt.ImportRow(dx.Rows[i]);

            G1.NumberDataTable(dt);
            dgv.DataSource = dt;

            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string contract = dr["contractNumber"].ObjToString();
            if (!String.IsNullOrWhiteSpace(contract))
            {
                this.Cursor = Cursors.WaitCursor;
                string cmd = "Select * from `policies` p JOIN `icustomers` d ON p.`contractNumber` = d.`contractNumber` JOIN `icontracts` x ON p.`contractNumber` = x.`contractNumber` ";
                cmd += " WHERE p.`contractNumber` = '" + contract + "' ";

                DataTable dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    Policies policyForm = new Policies(contract);
                    policyForm.Show();
                }
                else
                {
                    CustomerDetails clientForm = new CustomerDetails(contract);
                    clientForm.Show();
                }
                this.Cursor = Cursors.Default;
            }
        }
        /***********************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.gridMain.OptionsFind.AlwaysVisible == true)
                    gridMain.OptionsFind.AlwaysVisible = false;
                else
                    gridMain.OptionsFind.AlwaysVisible = true;
            }
            catch (Exception ex)
            {
            }
        }
        /***********************************************************************************************/
    }
}