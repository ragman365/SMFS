using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GeneralLib;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class EditManualBank : DevExpress.XtraEditors.XtraForm
    {
        private DataTable workDt = null;
        private DataTable originalDt = null;
        private string workFields = "";
        private bool workMulti = false;
        private DateTime workDate = DateTime.Now;
        /***********************************************************************************************/
        public EditManualBank( DataTable dt, string fields = "" )
        {
            InitializeComponent();
            workDt = dt;
            originalDt = workDt.Copy();
            workFields = fields;
        }
        /***********************************************************************************************/
        private void EditManualBank_Load(object sender, EventArgs e)
        {
            if ( 1 == 1)
            {
                btnFinished.Hide();
                btnFinished.Refresh();
                workDt.Columns.Add("mod");
                G1.NumberDataTable(workDt);
                if (workDt.Rows.Count > 0)
                    workDate = workDt.Rows[0]["date"].ObjToDateTime();
                gridMain.Columns["NOX"].Visible = true;
                if ( G1.get_column_number ( workDt, "NOX") >= 0 )
                {
                    DataRow[] dRows = workDt.Select("NOX>'0'");
                    if (dRows.Length > 0D)
                        gridMain.Columns["NOX"].Visible = true;
                }
                dgv.DataSource = workDt;
                return;
            }
            // This was copied from ViewDataTable. Uncomment this if it becomes useful
            if (String.IsNullOrWhiteSpace(workFields))
            {
                dgv.DataSource = workDt;
                return;
            }

            //if (workMulti)
            //    dgv.ContextMenuStrip = this.contextMenuStrip1;

            string[] Lines = workFields.Split(',');

            string field = "";
            string toType = "";
            DataRow dR = null;
            DataTable dt = new DataTable();

            if ( workMulti )
            {
                dt.Columns.Add("Select");
            }

            for ( int i=0; i<workDt.Rows.Count; i++)
            {
                dR = dt.NewRow();
                dt.Rows.Add(dR);
            }

            G1.NumberDataTable(dt);

            for (int i = 0; i < Lines.Length; i++)
            {
                try
                {
                    field = Lines[i].Trim();
                    field = field.Replace("(", "");
                    field = field.Replace(")", "");
                    if (G1.get_column_number(workDt, field) < 0)
                        continue;
                    toType = workDt.Columns[field].DataType.ToString().ToUpper();

                    if (toType.IndexOf("MYSQLDATETIME") >= 0)
                        dt.Columns.Add(field, Type.GetType("System.DateTime"));
                    else if (toType.IndexOf("DOUBLE") >= 0)
                        dt.Columns.Add(field, Type.GetType("System.Double"));
                    else if (toType.IndexOf("DECIMAL") >= 0)
                        dt.Columns.Add(field, Type.GetType("System.Decimal"));
                    else if (toType.IndexOf("INT32") >= 0)
                        dt.Columns.Add(field, Type.GetType("System.Int32"));
                    else if (toType.IndexOf("INT64") >= 0)
                        dt.Columns.Add(field, Type.GetType("System.Double"));
                    else if (toType.ToUpper() == "SYSTEM.BYTE[]")
                        continue;
                    else
                        dt.Columns.Add(field, Type.GetType("System.String"));

                    G1.copy_dt_column(workDt, field, dt, field);
                }
                catch ( Exception ex)
                {
                }
            }

            dgv.DataSource = dt;

            for (int i = 0; i < Lines.Length; i++)
            {
                try
                {
                    field = Lines[i].Trim();
                    if ( field.IndexOf( "(") >= 0 )
                    {
                        field = field.Replace("(", "");
                        field = field.Replace(")", "");
                        gridMain.Columns[field].Visible = false;
                    }
                }
                catch (Exception ex)
                {
                }
            }
            if (workMulti)
            {
                //gridMain.Columns["Select"].ColumnEdit = this.repositoryItemCheckEdit4;
                //SetupSelection(dt);
                //btnFinished.Show();
                //if (G1.get_column_number(dt, "SelectedRow") < 0)
                //    dt.Columns.Add("SelectedRow");
            }
        }
        /***********************************************************************************************/
        private void SetupSelection(DataTable dt)
        {
            //DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew = this.repositoryItemCheckEdit4;
            //selectnew.NullText = "";
            //selectnew.ValueChecked = "1";
            //selectnew.ValueUnchecked = "0";
            //selectnew.ValueGrayed = "";
            //for (int i = 0; i < dt.Rows.Count; i++)
            //    dt.Rows[i]["select"] = "0";
        }
        /***************************************************************************************/
        public delegate void d_void_eventdone_datarow(DataTable dd, DateTime date, DataTable originalDt );
        public event d_void_eventdone_datarow ManualDone;
        protected void OnManualDone(DataTable dd )
        {
            if (ManualDone != null)
                ManualDone.Invoke ( dd, workDate, originalDt );
        }
        /***********************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();
            if (dr != null)
                OnManualDone( dt );
        }
        /***********************************************************************************************/
        private void selectRowsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (G1.get_column_number(dt, "SelectedRow") < 0)
                dt.Columns.Add("SelectedRow");
            int row = 0;
            int rowIndex = 0;
            int[] rows = gridMain.GetSelectedRows();
            try
            {
                for (int i = 0; i < rows.Length; i++)
                {
                    row = rows[i];
                    rowIndex = gridMain.GetDataSourceRowIndex(row);
                    dt.Rows[rowIndex]["SelectedRow"] = "Y";
                }
                OnManualDone(dt);
            }
            catch (Exception ex)
            {
                MessageBox.Show("*ERROR*** " + ex.Message.ToString());
            }

        }
        /***********************************************************************************************/
        private void btnFinished_Click(object sender, EventArgs e)
        {

            string mod = "";
            string record = "";

            double tda = 0D;
            double ida = 0D;
            double nda = 0D;
            double dda = 0D;
            double misc = 0D;
            double returns = 0D;
            double transfer = 0D;
            string comment = "";

            string depositNumber = "";
            string serviceId = "";
            string bankAccount = "";
            string location = "";

            DataTable dt = (DataTable)dgv.DataSource;

            for ( int i=0; i<dt.Rows.Count; i++)
            {
                record = dt.Rows[i]["record"].ObjToString();
                mod = dt.Rows[i]["mod"].ObjToString();
                if ( mod == "D")
                {
                    G1.delete_db_table("lockboxdeposits", "record", record);
                    continue;
                }
                if (mod == "Y")
                {
                    bankAccount = dt.Rows[i]["bank_account"].ObjToString();
                    serviceId = dt.Rows[i]["serviceId"].ObjToString();
                    location = dt.Rows[i]["location"].ObjToString();
                    depositNumber = dt.Rows[i]["depositNumber"].ObjToString();
                    tda = dt.Rows[i]["TDA"].ObjToDouble();
                    ida = dt.Rows[i]["IDA"].ObjToDouble();
                    nda = dt.Rows[i]["NDA"].ObjToDouble();
                    dda = dt.Rows[i]["DDA"].ObjToDouble();
                    misc = dt.Rows[i]["misc"].ObjToDouble();
                    returns = dt.Rows[i]["returns"].ObjToDouble();
                    transfer = dt.Rows[i]["transfers"].ObjToDouble();
                    comment = dt.Rows[i]["comment"].ObjToString();

                    tda = G1.RoundValue(tda);
                    ida = G1.RoundValue(ida);
                    nda = G1.RoundValue(nda);
                    dda = G1.RoundValue(dda);
                    misc = G1.RoundValue(misc);
                    returns = G1.RoundValue(returns);
                    transfer = G1.RoundValue(transfer);

                    G1.update_db_table("lockboxdeposits", "record", record, new string[] { "comment", comment, "TDA", tda.ToString(), "IDA", ida.ToString(), "NDA", nda.ToString(), "dda", dda.ToString(), "misc", misc.ToString(), "returns", returns.ToString(), "transfers", transfer.ToString(), "bank_account", bankAccount, "depositNumber", depositNumber, "serviceId", serviceId, "location", location });
                }
            }
            btnFinished.Hide();
            btnFinished.Refresh();

            OnManualDone(dt);

            this.Close();
        }
        /***********************************************************************************************/
        private void pictureDelete_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt.Rows.Count <= 0)
                return;
            DataRow dr = gridMain.GetFocusedDataRow();
            dr["mod"] = "D";
            gridMain.RefreshEditor(true);
            btnFinished.Show();
            btnFinished.Refresh();
        }
        /***********************************************************************************************/
        private void gridMain_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv.DataSource;
            string status = dt.Rows[row]["mod"].ObjToString().ToUpper();
            if (status.ToUpper() == "D" )
            {
                e.Visible = false;
                e.Handled = true;
            }
        }
        /***********************************************************************************************/
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt.Rows.Count <= 0)
                return;
            DataRow dr = gridMain.GetFocusedDataRow();
            dr["mod"] = "Y";
            btnFinished.Show();
            btnFinished.Refresh();
        }
        /***********************************************************************************************/
        private void EditManualBank_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (btnFinished.Visible)
            {
                DialogResult result = MessageBox.Show("Changes Have BeenMade!\nDo you want to save these changes?", "Data Modified Dialog", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation);
                if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
                else if (result == DialogResult.No)
                    return;
                btnFinished_Click(null, null);
            }
        }
        /***********************************************************************************************/
    }
}