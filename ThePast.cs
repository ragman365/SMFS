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
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class ThePast : DevExpress.XtraEditors.XtraForm
    {
        /****************************************************************************************/
        public ThePast()
        {
            InitializeComponent();
        }
        /****************************************************************************************/
        private void ThePast_Load(object sender, EventArgs e)
        {
            Trust85.allAgentsDt = G1.get_db_data("Select * from `agents`");
            loadLocatons();
            SetupTotalsSummary();
        }
        /****************************************************************************************/
        private void SetupTotalsSummary()
        {
            AddSummaryColumn("commission", null);
            AddSummaryColumn("totalContracts", null);
            AddSummaryColumn("recap", null);
            AddSummaryColumn("recapContracts", null);
            AddSummaryColumn("dbrSales", null);
        }
        /****************************************************************************************/
        private void AddSummaryColumn(string columnName, DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gMain = null)
        {
            if (gMain == null)
                gMain = gridMain;
            //            gMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            gMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            //            gMain.Columns[columnName].SummaryItem.DisplayFormat = "${0:0,0.00}";
            gMain.Columns[columnName].SummaryItem.DisplayFormat = "{0:N2}";
        }
        /***********************************************************************************************/
        private void loadLocatons()
        {
            string cmd = "Select * from `funeralhomes` order by `keycode`;";
            DataTable locDt = G1.get_db_data(cmd);
            chkComboLocation.Properties.DataSource = locDt;
        }
        /****************************************************************************************/
        private void btnRun_Click(object sender, EventArgs e)
        {
            double commission = 0D;
            string agent = txtAgent.Text;
            string location = txtLocation.Text;
            string splits = "";
            double percent = 0.01D;
            double goal = 65000D;
            DateTime lapseDate8 = new DateTime(2019, 6, 6);
            DateTime iDate = this.dateTimePicker1.Value;
            double totalContracts = 0D;
            double recaps = 0D;
            double dbrSales = 0D;
            double recapContracts = 0D;
            DataTable dt = new DataTable();
            dt.Columns.Add("loc");
            dt.Columns.Add("commission", Type.GetType("System.Decimal"));
            dt.Columns.Add("totalContracts", Type.GetType("System.Decimal"));
            dt.Columns.Add("recap", Type.GetType("System.Decimal"));
            dt.Columns.Add("recapContracts", Type.GetType("System.Decimal"));
            dt.Columns.Add("dbrSales", Type.GetType("System.Decimal"));
            dt.Rows.Clear();
            try
            {
                string[] locIDs = this.chkComboLocation.EditValue.ToString().Split('|');
                for (int i = 0; i < locIDs.Length; i++)
                {
                    location = locIDs[i].Trim();

                    commission = Trust85.CalculatePastCommissions("lapseDate8", agent, location, splits, percent, goal, lapseDate8, iDate, iDate, ref totalContracts, ref recaps, ref recapContracts, ref dbrSales);
                    DataRow dRow = dt.NewRow();
                    dRow["loc"] = location;
                    dRow["commission"] = commission;
                    dRow["totalContracts"] = totalContracts;
                    dRow["recap"] = recaps;
                    dRow["recapContracts"] = recapContracts;
                    dRow["dbrSales"] = dbrSales;
                    dt.Rows.Add(dRow);
                }
                dgv.DataSource = dt;
                dgv.Refresh();
            }
            catch (Exception ex)
            {
            }
        }
        /****************************************************************************************/
    }
}