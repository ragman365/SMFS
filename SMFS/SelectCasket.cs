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
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class SelectCasket : DevExpress.XtraEditors.XtraForm
    {
        private DataTable workDt = null;
        private DataTable workSelectDt = null;
        private bool workMulti = false;
        private string _answer = "";
        public string Answer { get { return _answer; } }
        /***********************************************************************************************/
        public SelectCasket( DataTable dt, DataTable selectDt=null )
        {
            InitializeComponent();
            workDt = dt;
            workSelectDt = selectDt;
            workMulti = false;
        }
        /***********************************************************************************************/
        private void SelectCasket_Load(object sender, EventArgs e)
        {
            lblManual.Hide();
            btnManual.Hide();
            if ( workSelectDt != null )
            {
                lblManual.Show();
                btnManual.Show();
            }
            DataTable dt = new DataTable();
            dt.Columns.Add("num");
            dt.Columns.Add("cost");
            dt.Columns.Add("casket");
            dt.Columns.Add("newcost");
            if (workSelectDt == null)
                gridMain.Columns["newcost"].Visible = false;
            string str = "";
            for ( int i=0; i<workDt.Rows.Count; i++)
            {
                str = workDt.Rows[i]["COL4"].ObjToString();
                if (!String.IsNullOrWhiteSpace(str))
                {
                    DataRow dR = dt.NewRow();
                    dR["casket"] = str;
                    str = workDt.Rows[i]["Col 17"].ObjToString();
                    dR["cost"] = str;
                    dt.Rows.Add(dR);
                }
            }
            if (workMulti)
                SetupSelection(dt);
            else
                gridMain.Columns["select"].Visible = false;

            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
        }
        /***********************************************************************************************/
        private void SetupSelection(DataTable dt)
        {
            if ( G1.get_column_number ( dt, "select") < 0 )
                dt.Columns.Add("select");
            DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew = this.repositoryItemCheckEdit4;
            selectnew.NullText = "";
            selectnew.ValueChecked = "1";
            selectnew.ValueUnchecked = "0";
            selectnew.ValueGrayed = "";
            if (G1.get_column_number(dt, "select") < 0)
                dt.Columns.Add("select");
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["select"] = "0";
        }
        /***********************************************************************************************/
        private void btnExit_Click(object sender, EventArgs e)
        {
            OnListDone();
            this.Close();
        }
        /***************************************************************************************/
        public delegate void d_void_eventdone_string(DataTable dt );
        public event d_void_eventdone_string ListDone;
        protected void OnListDone()
        {
            _answer = "";
            if (workSelectDt == null)
            {
                DataRow dr = gridMain.GetFocusedDataRow();
                string selection = dr["cost"].ObjToString();
                if (String.IsNullOrWhiteSpace(selection))
                    this.Close();
                _answer = selection;
            }
            this.DialogResult = DialogResult.OK;
            if (ListDone != null)
            {
                DataTable dt = (DataTable)dgv.DataSource;
                ListDone.Invoke(dt);
            }
        }
        /***********************************************************************************************/
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
        /***********************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            if (workSelectDt == null)
            {
                OnListDone();
                this.Close();
                return;
            }

            DataTable ddx = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();
            string desc = dr["casket"].ObjToString();
            string cost = dr["cost"].ObjToString();

            using (SelectCasket listForm = new SelectCasket(workSelectDt))
            {
                listForm.Text = "Looking for " + desc + " Old Price " + cost;
                listForm.ShowDialog();
                if (listForm.DialogResult == System.Windows.Forms.DialogResult.OK)
                {
                    string price = listForm.Answer;
                    if (!String.IsNullOrWhiteSpace(price))
                    {
                        price = price.Replace("$", "");
                        price = price.Replace(",", "");
                        if (G1.validate_numeric(price))
                        {
                            double payment = price.ObjToDouble();
                            price = "$" + G1.ReformatMoney(payment);
                            dr["newcost"] = price;
                        }
                    }
                }
            }
        }
        /***********************************************************************************************/
        private void SelectFromList_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!workMulti)
            {
                if (workSelectDt != null)
                    OnListDone();
                return;
            }
            bool modified = false;
            string select = "";
            DataTable dt = (DataTable)dgv.DataSource;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                select = dt.Rows[i]["select"].ObjToString();
                if (select == "1")
                {
                    modified = true;
                    break;
                }
            }
            if ( modified )
            {
                DialogResult result = MessageBox.Show("***Question***\nData has been selected!\nDo you want to exit with the selected data?", "Data Selected Dialog", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
                if (result == DialogResult.Yes)
                {
                    OnListDone();
                    return;
                }
            }
        }
        /***********************************************************************************************/
        private void repositoryItemCheckEdit4_CheckedChanged(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            string select = dr["select"].ObjToString();
            select = dt.Rows[row]["select"].ObjToString();

            string doit = "0";
            if (select == "0")
                doit = "1";
            dr["select"] = doit;
            dt.Rows[row]["select"] = doit;
            dgv.DataSource = dt;
            dgv.RefreshDataSource();
            gridMain.RefreshData();
            dgv.Refresh();
        }
        /***********************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        {
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
        /***********************************************************************************************/
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (e == null)
                return;
            int rowHandle = gridMain.FocusedRowHandle;
            DataRow dr = gridMain.GetFocusedDataRow();
            if (dr == null)
                return;

            if (e.Column.FieldName.ToUpper() == "NEWCOST")
            {
                string str = e.Value.ObjToString();
                str = str.Replace("$", "");
                str = str.Replace(",", "");
                if (G1.validate_numeric(str))
                {
                    double payment = str.ObjToDouble();
                    str = "$" + G1.ReformatMoney(payment);
                    dr["newcost"] = str;
                }
            }
        }
        /***********************************************************************************************/
        private void gridMain_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (!gridMain.IsLastVisibleRow)
                    gridMain.MoveNext();
                else
                    gridMain.MoveFirst();
            }
        }
        /***********************************************************************************************/
        private void btnManual_Click(object sender, EventArgs e)
        {
            gridMain_DoubleClick( null, null );
        }
        /***********************************************************************************************/
    }
}