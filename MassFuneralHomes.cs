using System;
using System.Data;
using System.Windows.Forms;

using GeneralLib;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Columns;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class MassFuneralHomes : Form
    {
        /***********************************************************************************************/
        private DataTable workDt = null;
        public MassFuneralHomes(DataTable dt)
        {
            workDt = dt;
            InitializeComponent();
        }
        /***********************************************************************************************/
        private void MassFuneralHomes_Load(object sender, EventArgs e)
        {
            btnMassPrint.Hide();
            lblPrintWhat.Hide();
            cmbWhat.Hide();

            lblAsOfDate.Hide();
            dateTimePicker1.Hide();
            chkAsOfDate.Hide();

            LoadData();
        }
        /***********************************************************************************************/
        private void LoadData()
        {
            DataTable dt = workDt;

            DataRow[] dRows = dt.Select("locind=''" );
            if ( dRows.Length > 0 )
            {
                for (int i = 0; i < dRows.Length; i++)
                    dt.Rows.Remove(dRows[i]);
            }

            SetupSelection(dt);

            G1.NumberDataTable(dt);
            this.dgv.DataSource = dt;
        }
        /***********************************************************************************************/
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            if (e.Column.FieldName.ToUpper() == "GROUPNAME")
            {
                string record = dr["record"].ObjToString();
                if (!String.IsNullOrWhiteSpace(record))
                {
                    string groupname = dr["groupname"].ObjToString();
                    //                    G1.update_db_table("funeralhomes", "record", record, new string[] { "groupname", groupname} );
                }
            }
        }
        /***********************************************************************************************/
        private void gridMain_ShowingEditor(object sender, System.ComponentModel.CancelEventArgs e)
        {
            GridView view = sender as GridView;
        }
        /***********************************************************************************************/
        private void repositoryItemCheckedComboBoxEdit2_Popup(object sender, EventArgs e)
        {
        }
        /***********************************************************************************************/
        private void SetupSelection(DataTable dt)
        {
            if (G1.get_column_number(dt, "select") < 0)
                dt.Columns.Add("select");

            DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew = this.repositoryItemCheckEdit1;
            selectnew.NullText = "";
            selectnew.ValueChecked = "1";
            selectnew.ValueUnchecked = "0";
            selectnew.ValueGrayed = "";
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["select"] = "0";
        }
        /***********************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            PriceLists priceForm = new PriceLists("Current", record );
            priceForm.Show();
        }
        /***********************************************************************************************/
        private void btnSelect_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["select"] = "1";
            dgv.DataSource = dt;

            btnMassPrint.Show();
            btnMassPrint.Refresh();

            lblPrintWhat.Show();
            lblPrintWhat.Refresh();
            cmbWhat.Show();
            cmbWhat.Refresh();

            lblAsOfDate.Show();
            dateTimePicker1.Show();
            chkAsOfDate.Show();

        }
        /***********************************************************************************************/
        private void btnUnselectAll_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["select"] = "0";
            dgv.DataSource = dt;

            btnMassPrint.Hide();
            btnMassPrint.Refresh();

            lblPrintWhat.Hide();
            lblPrintWhat.Refresh();
            cmbWhat.Hide();
            cmbWhat.Refresh();

            lblAsOfDate.Hide();
            dateTimePicker1.Hide();
            chkAsOfDate.Hide();
        }
        /***********************************************************************************************/
        private void repositoryItemCheckEdit1_CheckedChanged(object sender, EventArgs e)
        {
            DevExpress.XtraEditors.CheckEdit check = (DevExpress.XtraEditors.CheckEdit)sender;
            bool isChecked = true;
            if (!check.Checked)
                isChecked = false;

            DataTable dt = (DataTable)dgv.DataSource;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetFocusedDataSourceRowIndex();

            DataRow dr = gridMain.GetFocusedDataRow();
            string select = dr["select"].ObjToString();
            if ( isChecked )
            {
                dr["select"] = "1";
                btnMassPrint.Show();
                btnMassPrint.Refresh();

                lblPrintWhat.Show();
                lblPrintWhat.Refresh();
                cmbWhat.Show();
                cmbWhat.Refresh();

                lblAsOfDate.Show();
                dateTimePicker1.Show();
                chkAsOfDate.Show();
            }
            else
            {
                dr["select"] = "0";
                dt.Rows[row]["select"] = "0";

                DataRow[] dRows = dt.Select("select='1'");
                if (dRows.Length <= 0)
                {
                    btnMassPrint.Hide();
                    btnMassPrint.Refresh();

                    lblPrintWhat.Hide();
                    lblPrintWhat.Refresh();
                    cmbWhat.Hide();
                    cmbWhat.Refresh();

                    lblAsOfDate.Hide();
                    dateTimePicker1.Hide();
                    chkAsOfDate.Hide();
                }
            }
        }
        /***********************************************************************************************/
        private void btnMassPrint_Click(object sender, EventArgs e)
        {
            string printWhat = cmbWhat.Text.Trim();
            DataTable dt = (DataTable)dgv.DataSource;
            string asOfDate = "";
            if ( chkAsOfDate.Checked )
            {
                DateTime date = this.dateTimePicker1.Value;
                asOfDate = date.ToString("MM/dd/yyyy");
            }
            PriceLists priceForm = new PriceLists ( printWhat, dt, asOfDate );
            priceForm.Show();
        }
        /***********************************************************************************************/
    }
}
