using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraPrinting;
using DevExpress.Utils;
using MySql.Data.MySqlClient;
using GeneralLib;
using DevExpress.XtraGrid.Views.Grid;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class DupCommission : DevExpress.XtraEditors.XtraForm
    {
        private DataTable workDt = null;
        private string workAgentName = "";
        /****************************************************************************************/
        public DupCommission( DataTable dt, string agentName )
        {
            InitializeComponent();
            workDt = dt;
            workAgentName = agentName;
            SetupTotalsSummary();
        }
        /****************************************************************************************/
        private void DupCommission_Load(object sender, EventArgs e)
        {
            dgv.DataSource = workDt;
        }
        /****************************************************************************************/
        private void SetupTotalsSummary()
        {
            AddSummaryColumn("totalCommission");
            AddSummaryColumn("totalPayments");
            AddSummaryColumn("commission");
            AddSummaryColumn("splitCommission");
            AddSummaryColumn("goalCommission");
            AddSummaryColumn("mainCommission");
            AddSummaryColumn("contractValue");
            AddSummaryColumn("Formula Sales");
            AddSummaryColumn("Location Sales");
        }
        /****************************************************************************************/
        private void AddSummaryColumn(string columnName)
        {
            gridMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            gridMain.Columns[columnName].SummaryItem.DisplayFormat = "${0:0,0.00}";
        }
        /****************************************************************************************/
    }
}