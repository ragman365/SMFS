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
    public partial class EditEmpContract : DevExpress.XtraEditors.XtraForm
    {
        private bool modified = false;
        private bool loading = true;
        private string workEmpNo = "";
        private string workUser = "";
        private string workName = "";
        private string workWhat = "";
        /****************************************************************************************/
        public EditEmpContract( string what, string empNo, string user, string name )
        {
            InitializeComponent();
            workEmpNo = empNo;
            workUser = user;
            workName = name;
            workWhat = what;
        }
        /****************************************************************************************/
        private void EditEmpContract_Load(object sender, EventArgs e)
        {
            this.TopMost = true;
            btnSaveAll.Hide();
            this.Text = "Edit Employee PartTime Services for " + workName;
            if ( workWhat.ToUpper() == "OTHER")
                this.Text = "Edit Employee Other Services for " + workName;

            string cmd   = "Select * from `tc_contract_labor_services` ORDER BY `order`;";
            if (workWhat.ToUpper() == "OTHER")
                cmd = "Select * from `tc_other_labor_services` ORDER BY `order`;";
            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("rate", Type.GetType("System.Decimal"));
            dt.Columns.Add("myrecord");
            dt.Columns.Add("num");
            dt.Columns.Add("mod");

            string service = "";
            string myRecord = "";
            DataRow[] dRows = null;
            cmd = "Select * from `tc_contract_labor_setup` WHERE `employee` = '" + workUser + "' ORDER BY `order`;";
            if (workWhat.ToUpper() == "OTHER")
                cmd = "Select * from `tc_other_labor_setup` WHERE `employee` = '" + workUser + "' ORDER BY `order`;";
            DataTable dx = G1.get_db_data(cmd);
            for ( int i=0; i<dx.Rows.Count; i++)
            {
                service = dx.Rows[i]["laborService"].ObjToString();
                dRows = dt.Select ( "laborService='" + service + "'");
                if (dRows.Length > 0)
                {
                    dRows[0]["rate"] = dx.Rows[i]["rate"].ObjToDecimal();
                    myRecord = dx.Rows[i]["record"].ObjToString();
                    dRows[0]["myrecord"] = myRecord;
                }
            }

            if (!G1.isHR())
                gridMain.Columns["rate"].Visible = false;

            G1.NumberDataTable(dt);
            dgv.DataSource = dt;

            loading = false;

            //int top = this.Top + 20;
            //int left = this.Left + 20;
            //this.SetBounds(left, top, this.Width, this.Height);
        }
        /****************************************************************************************/
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (loading)
                return;
            modified = true;
            btnSaveAll.Show();
            btnSaveAll.Refresh();
            DataRow dr = gridMain.GetFocusedDataRow();
            dr["mod"] = "Y";
        }
        /****************************************************************************************/
        private void gridMain_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt == null)
                return;
            string delete = dt.Rows[row]["mod"].ObjToString();
            if (delete.ToUpper() == "D")
            {
                e.Visible = false;
                e.Handled = true;
            }
        }
        /****************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        { // Spy Glass
            G1.SpyGlass(gridMain);
        }
        /****************************************************************************************/
        private void btnSaveAll_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            string record = "";
            string mod = "";
            string service = "";
            decimal rate = 0;


            string cmd = "DELETE from `tc_contract_labor_setup` WHERE `laborService` = '-1'";
            if ( workWhat.ToUpper() == "OTHER")
                cmd = "DELETE from `tc_other_labor_setup` WHERE `laborService` = '-1'";
            G1.get_db_data(cmd);

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                record = dt.Rows[i]["myrecord"].ObjToString();
                mod = dt.Rows[i]["mod"].ObjToString();
                if (mod == "D")
                {
                    if (!String.IsNullOrWhiteSpace(record))
                    {
                        if (workWhat.ToUpper() == "OTHER")
                            G1.delete_db_table("tc_other_labor_setup", "record", record);
                        else
                            G1.delete_db_table("tc_contract_labor_setup", "record", record);
                    }
                    continue;
                }
                if (mod != "Y")
                    continue;
                if (workWhat.ToUpper() == "OTHER")
                {
                    if (String.IsNullOrWhiteSpace(record))
                        record = G1.create_record("tc_other_labor_setup", "laborService", "-1");
                    if (G1.BadRecord("tc_other_labor_setup", record))
                        return;

                    service = dt.Rows[i]["laborService"].ObjToString();
                    rate = dt.Rows[i]["rate"].ObjToDecimal();
                    G1.update_db_table("tc_other_labor_setup", "record", record, new string[] { "laborService", service, "rate", rate.ToString(), "employee", workUser, "user", LoginForm.username });
                }
                else
                {
                    if (String.IsNullOrWhiteSpace(record))
                        record = G1.create_record("tc_contract_labor_setup", "laborService", "-1");
                    if (G1.BadRecord("tc_contract_labor_setup", record))
                        return;

                    service = dt.Rows[i]["laborService"].ObjToString();
                    rate = dt.Rows[i]["rate"].ObjToDecimal();
                    G1.update_db_table("tc_contract_labor_setup", "record", record, new string[] { "laborService", service, "rate", rate.ToString(), "employee", workUser, "user", LoginForm.username });
                }
            }
            modified = false;
            btnSaveAll.Hide();
        }
        /***********************************************************************************************/
        private void RecheckOrder ( DataTable dt )
        {
            int order = 0;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                order = dt.Rows[i]["order"].ObjToInt32();
                if (order != i)
                    dt.Rows[i]["mod"] = "Y";
            }
        }
        /****************************************************************************************/
        private void gridMain_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
        {
            if (e.Column.FieldName.ToUpper() == "NUM")
            {
                if (e.RowHandle >= 0)
                {
                    string num = (e.RowHandle + 1).ToString();
                    e.DisplayText = num;
                }
            }
        }
        /****************************************************************************************/
        private void EditTable_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!btnSaveAll.Visible)
                return;
            DialogResult result = MessageBox.Show("***Question*** Data has been modified.\nDo you really want to exit WITHOUT saving your data?", "Data Modified Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
                return;
            e.Cancel = true;
        }
        /****************************************************************************************/
        private void gridMain_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column.FieldName.ToUpper() == "YEAR" && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                string year = e.DisplayText;
                year = year.Replace(",", "");
                e.DisplayText = year;
            }
            else if (e.Column.FieldName.ToUpper() == "RATE" && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                double rate = e.DisplayText.ObjToDouble();
                if (rate == 0D)
                    e.DisplayText = "";
            }
        }
        /****************************************************************************************/
        private void pictureDelete_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string myRecord = dr["myRecord"].ObjToString();
            if ( !String.IsNullOrWhiteSpace ( myRecord ))
            {
                if ( workWhat.ToUpper() == "OTHER" )
                    G1.delete_db_table("tc_other_labor_setup", "record", myRecord);
                else
                    G1.delete_db_table("tc_contract_labor_setup", "record", myRecord);
                dr["myRecord"] = "";
                dr["rate"] = 0D;
            }
        }
        /****************************************************************************************/
    }
}