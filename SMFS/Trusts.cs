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
    public partial class Trusts : DevExpress.XtraEditors.XtraForm
    {
        /****************************************************************************************/
        public Trusts()
        {
            InitializeComponent();
            SetupTotalsSummary();
        }
        /****************************************************************************************/
        private void Trusts_Load(object sender, EventArgs e)
        {
        }
        /****************************************************************************************/
        private void btnRun_Click(object sender, EventArgs e)
        {
            DateTime date1 = this.dateTimePicker1.Value;
            DateTime date2 = this.dateTimePicker2.Value;
            date1 = new DateTime(date1.Year, date1.Month, 1);
            int days = DateTime.DaysInMonth(date2.Year, date2.Month);
            date2 = new DateTime(date2.Year, date2.Month, 1);
            this.Cursor = Cursors.WaitCursor;
            bool first = true;
            DataTable myDt = new DataTable();
            for (;;)
            {
                DateTime start = date1;
                days = DateTime.DaysInMonth(date1.Year, date1.Month);
                DateTime stop = new DateTime(date1.Year, date1.Month, days);
                using (Trust85 trustForm = new Trust85(start, stop))
                {
                    UpdateDataTableDate(Trust85.trust85_dt, start);
                    if ( first )
                    {
                        myDt = Trust85.trust85_dt.Clone();
                        first = false;
                    }
                    for (int i = 0; i < Trust85.trust85_dt.Rows.Count; i++)
                        myDt.ImportRow(Trust85.trust85_dt.Rows[i]);
                    date1 = date1.AddMonths(1);
                    if (date1 > date2)
                        break;
//                    break;
                }
            }
            dgv.DataSource = myDt;
            dgv.Refresh();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void UpdateDataTableDate ( DataTable dt, DateTime date )
        {
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                dt.Rows[i]["dueDate8"] = G1.DTtoMySQLDT(date);
            }
        }
        /****************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (dgv.Visible)
                SetSpyGlass(gridMain);
        }
        /***********************************************************************************************/
        private void SetSpyGlass(DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView grid)
        {
            if (grid.OptionsFind.AlwaysVisible == true)
                grid.OptionsFind.AlwaysVisible = false;
            else
                grid.OptionsFind.AlwaysVisible = true;
        }
        /****************************************************************************************/
        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            DateTime now = this.dateTimePicker1.Value;
            DateTime date = new DateTime(now.Year, now.Month, 1);
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            date = new DateTime(date.Year, date.Month, days);
            this.dateTimePicker2.Value = date;
        }
        /****************************************************************************************/
        private void SetupTotalsSummary()
        {
            AddSummaryColumn("Recap");
            AddSummaryColumn("Reins");
            AddSummaryColumn("downPayment");
            AddSummaryColumn("totalPayments");
            AddSummaryColumn("paymentAmount");
            AddSummaryColumn("trust85P");
            AddSummaryColumn("trust100P");
            AddSummaryColumn("commission");
            AddSummaryColumn("contractValue");
            AddSummaryColumn("cashAdvance");
            AddSummaryColumn("ibtrust");
            AddSummaryColumn("sptrust");
            AddSummaryColumn("xxtrust");
            AddSummaryColumn("debit");
            AddSummaryColumn("credit");
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
        /****************************************************************************************/
    }
}