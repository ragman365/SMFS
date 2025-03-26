using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using DevExpress.XtraPrinting;
using DevExpress.Utils;
using GeneralLib;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using MySql.Data.MySqlClient;
using DevExpress.XtraGrid;
using DevExpress.Utils.Drawing;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class StopBreakReport : DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        public StopBreakReport()
        {
            InitializeComponent();
        }
        /***********************************************************************************************/
        private void StopBreakReport_Load(object sender, EventArgs e)
        {
        }
        /***********************************************************************************************/
        private void btnRun_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            DateTime startDate = new DateTime(2015, 1, 1);
            string cmd = "select * from `contracts` where `lapsed` = 'Y' and `lapseDate8` >= '2015-01-01' and `deceasedDate` < '1000-01-01';";
            DataTable dx = G1.get_db_data(cmd);
            dx.Columns.Add("export");
            DataTable dt = dx.Copy();

            PullInsuranceLapsed(dt);
            CleanupExport(dt);
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void CleanupExport ( DataTable dt)
        {
            string contractNumber = "";
            string export = "";
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                export = contractNumber.PadLeft(10, '0');
                dt.Rows[i]["export"] = export;
            }
            dt.AcceptChanges();
        }
        /***********************************************************************************************/
        private void PullInsuranceLapsed ( DataTable dt )
        {
            string contractNumber = "";
            string payer = "";
            string cmd = "Select * from `icontracts` x JOIN `icustomers` c ON x.`contractNumber` = c.`contractNumber` where x.`lapsed` = 'Y' and x.`lapseDate8` >= '2015-01-01' and x.`deceasedDate` < '1000-01-01';";
            DataTable dx = G1.get_db_data(cmd);
            for ( int i=0; i<dx.Rows.Count; i++)
            {
                contractNumber = dx.Rows[i]["contractNumber"].ObjToString();
                payer = dx.Rows[i]["payer"].ObjToString();
                DataRow dR = dt.NewRow();
                dR["contractNumber"] = payer;
                dR["lapsed"] = dx.Rows[i]["lapsed"].ObjToString();
                dR["lapseDate8"] = G1.DTtoMySQLDT(dx.Rows[i]["lapseDate8"]);
                dt.Rows.Add(dR);
            }
            dt.AcceptChanges();
        }
        /***********************************************************************************************/
        private void chkPrepare_CheckedChanged(object sender, EventArgs e)
        {
            if ( chkPrepare.Checked )
            {
                gridMain.Columns["num"].Visible = false;
                gridMain.Columns["lapsed"].Visible = false;
                gridMain.Columns["lapseDate8"].Visible = false;
                gridMain.Columns["contractNumber"].Visible = false;
            }
            else
            {
                gridMain.Columns["num"].Visible = true;
                gridMain.Columns["lapsed"].Visible = true;
                gridMain.Columns["lapseDate8"].Visible = true;
                gridMain.Columns["contractNumber"].Visible = true;
            }
            gridMain.RefreshData();
            dgv.Refresh();
        }
        /***********************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string contract = dr["contractNumber"].ObjToString();
            string cmd = "Select * from `contracts` where `contractNumber` = '" + contract + "';";
            DataTable ddx = G1.get_db_data(cmd);
            if ( ddx.Rows.Count <= 0 ) // Maybe Insurance
            {
                cmd = "Select * from `icustomers` where `payer` = '" + contract + "';";
                ddx = G1.get_db_data(cmd);
                if ( ddx.Rows.Count <= 0)
                {
                    MessageBox.Show("***ERROR*** Cannot find Contract or Payer!");
                    return;
                }
                contract = ddx.Rows[0]["contractNumber"].ObjToString();
            }
            if (!String.IsNullOrWhiteSpace(contract))
            {
                this.Cursor = Cursors.WaitCursor;
                DataTable dt = (DataTable)dgv.DataSource;
                bool insurance = false;
                if (contract.ToUpper().IndexOf("ZZ") == 0)
                    insurance = true;
                if (contract.ToUpper().IndexOf("MM") == 0)
                    insurance = true;
                if (contract.ToUpper().IndexOf("OO") == 0)
                    insurance = true;
                if (insurance)
                {
                    cmd = "Select * from `policies` p JOIN `icustomers` d ON p.`contractNumber` = d.`contractNumber` JOIN `icontracts` x ON p.`contractNumber` = x.`contractNumber` ";
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
        private void btnExport_Click(object sender, EventArgs e)
        {
            if (!chkPrepare.Checked)
                chkPrepare.Checked = true;

            string filter = "CSV files (*.csv)|*.csv";
            saveFileDialog1.Filter += filter;
            saveFileDialog1.FilterIndex = 0;
            saveFileDialog1.FileName = "";
            if (saveFileDialog1.ShowDialog() != DialogResult.OK)
                return;

            string fileName = saveFileDialog1.FileName;

            ((GridView)dgv.MainView).ExportToCsv(fileName, new CsvExportOptionsEx()
            {
                ExportType = DevExpress.Export.ExportType.WYSIWYG
            });
        }
        /***********************************************************************************************/
        private void exportOptions_CustomizeCell(DevExpress.Export.CustomizeCellEventArgs e)
        {
            e.Value = String.Format("\"{0}\"", e.Value);
            e.Handled = true;
        }
        /***********************************************************************************************/
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        /***********************************************************************************************/
    }
}