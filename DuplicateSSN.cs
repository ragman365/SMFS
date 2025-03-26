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
using System.IO;
using DevExpress.XtraPrinting;
using DevExpress.Utils;
using System.Drawing.Text;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid;
using System.Configuration;
using DevExpress.RichEdit.Export;
using DevExpress.XtraGrid.Columns;
using DevExpress.CodeParser;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class DuplicateSSN : DevExpress.XtraEditors.XtraForm
    {
        /****************************************************************************************/
        private string workSSN = "";
        private string workContract = "";
        public DuplicateSSN( string contract, string ssn )
        {
            InitializeComponent();
            workSSN = ssn;
            workContract = contract;
        }
        /****************************************************************************************/
        private void DuplicateSSN_Load(object sender, EventArgs e)
        {
            btnSave.Hide();
            string cmd = "Select * from `customers` where `ssn` = '" + workSSN + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
                return;
            string contract = "";
            string service = "";
            DataTable ddx = null;
            DataTable cDt = null;
            DataRow dR = null;
            string str = "";
            string type = "";
            string price = "";
            string name = "";
            string cName = "";
            string issueDate = "";
            double dValue = 0D;
            DateTime contractDate = DateTime.Now;
            DateTime date = DateTime.Now;
            DataTable dt = new DataTable();
            for ( int i=0; i<dx.Rows.Count; i++)
            {
                contract = dx.Rows[i]["contractNumber"].ObjToString();
                contractDate = dx.Rows[i]["contractDate"].ObjToDateTime();
                cmd = "Select * from `contracts` where `contractNumber` = '" + contract + "';";
                ddx = G1.get_db_data(cmd);
                issueDate = "IssueDate = NONE";
                if ( ddx.Rows.Count > 0 )
                {
                    date = ddx.Rows[0]["issueDate8"].ObjToDateTime();
                    if (date.Year > 1000)
                        issueDate = "IssueDate = " + date.ToString("MM/dd/yyyy");
                    else if (contractDate.Year > 0)
                        issueDate = "Contract Date = " + contractDate.ToString("MM/dd/yyyy");
                }
                cmd = "Select * from `cust_services` WHERE `contractNumber` = '" + contract + "';";
                ddx = G1.get_db_data(cmd);
                if (ddx.Rows.Count <= 0)
                    continue;
                cName = "C" + (i + 1).ToString();
                dt.Columns.Add(cName);
                name = "S" + (i + 1).ToString();
                dt.Columns.Add(name);

                if (dt.Rows.Count <= 0)
                {
                    dR = dt.NewRow();
                    dt.Rows.Add(dR);
                    dR = dt.NewRow();
                    dt.Rows.Add(dR);
                    dR = dt.NewRow();
                    dt.Rows.Add(dR);
                    dR = dt.NewRow();
                    dt.Rows.Add(dR);
                    dR = dt.NewRow();
                    dt.Rows.Add(dR);
                }
                dt.Rows[0][name] = "Primary-" + contract;
                dt.Rows[1][name] = issueDate;
                dt.Rows[2][name] = "Select Merchandise";
                dt.Rows[3][name] = "Select Services";
                dt.Rows[4][name] = "Select Cash Advance";
                for ( int j=0; j<ddx.Rows.Count; j++)
                {
                    type = ddx.Rows[j]["type"].ObjToString();
                    price = ddx.Rows[j]["data"].ObjToString();
                    if (String.IsNullOrWhiteSpace(type))
                        type = "S";
                    else
                    {
                        str = type.Substring(0, 1);
                        type = str.ToUpper();
                    }
                    service = ddx.Rows[j]["service"].ObjToString();
                    if ( dt.Rows.Count <= (j+5))
                    {
                        dR = dt.NewRow();
                        dt.Rows.Add(dR);
                    }
                    if ( type == "M" && service.Trim().ToUpper() == "CASKET NAME" && !String.IsNullOrEmpty ( price))
                    {
                        cmd = "Select * from `casket_master` where `casketcode` = '" + price + "';";
                        cDt = G1.get_db_data(cmd);
                        if (cDt.Rows.Count > 0)
                            service = cDt.Rows[0]["casketdesc"].ObjToString();
                    }
                    else if (type == "M" && service.Trim().ToUpper() == "OUTER CONTAINER NAME" && !String.IsNullOrEmpty(price))
                    {
                        cmd = "Select * from `casket_master` where `casketcode` = '" + price + "';";
                        cDt = G1.get_db_data(cmd);
                        if (cDt.Rows.Count > 0)
                            service = cDt.Rows[0]["casketdesc"].ObjToString();
                    }
                    str = "(" + type;
                    if (!String.IsNullOrWhiteSpace(price))
                    {
                        if ( G1.validate_numeric ( price))
                        {
                            dValue = price.ObjToDouble();
                            price = G1.ReformatMoney(dValue);
                            price = price.Replace(".00", "");
                        }
                        str += "-$" + price;
                    }
                    str += ") " + service;
                    dt.Rows[j+5][name] = str;
                }
            }
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                contract = dx.Rows[i]["contractNumber"].ObjToString();
                name = "S" + (i + 1).ToString();
                gridMain.Columns[name].Caption = contract;
            }
            SetupCheckColumn(this.repositoryItemCheckEdit1, dt, "C1");
            SetupCheckColumn(this.repositoryItemCheckEdit2, dt, "C2");
            if ( dx.Rows.Count > 2 )
                SetupCheckColumn(this.repositoryItemCheckEdit3, dt, "C3");
            else
            {
                gridMain.Columns["C3"].Visible = false;
                gridMain.Columns["S3"].Visible = false;
            }
            if (dx.Rows.Count > 3)
                SetupCheckColumn(this.repositoryItemCheckEdit4, dt, "C4");
            else
            {
                gridMain.Columns["C4"].Visible = false;
                gridMain.Columns["S4"].Visible = false;
            }
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                contract = dx.Rows[i]["contractNumber"].ObjToString();
                if (contract == workContract)
                {
                    name = "C" + (i + 1).ToString();
                    dt.Rows[0][name] = "1";
                }
            }

            //CleanupServices(dt);

            dgv.DataSource = dt;
            lblCurrentPrimary.Text = "Current Primary Contract Number : " + workContract;
            dgv.Refresh();
        }
        /***********************************************************************************************/
        private DataTable CleanupServices ( DataTable dt)
        {
            string column = "";
            int col = 0;
            string service = "";
            int lastRow = 0;
            for ( int i=1; i<=4; i++)
            {
                column = "C" + i.ToString();
                if (G1.get_column_number(dt, column) < 0)
                    break;
                col = G1.get_column_number(dt, column);
                col++;
                if (col >= dt.Columns.Count)
                    continue;
                lastRow = 0;
                for ( int j=0; j<dt.Rows.Count; j++)
                {
                    service = dt.Rows[j][col].ObjToString();
                    if (service.IndexOf("-$0") > 0)
                        dt.Rows[j][col] = "";
                }
            }
            return dt;
        }
        /***********************************************************************************************/
        private void SetupCheckColumn(DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew, DataTable dt, string columnName )
        {
            //DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew = this.repositoryItemCheckEdit1;
            selectnew.NullText = "";
            selectnew.ValueChecked = "1";
            selectnew.ValueUnchecked = "0";
            selectnew.ValueGrayed = "";
            if (G1.get_column_number(dt, columnName) < 0)
                dt.Columns.Add(columnName);
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i][columnName] = "0";
        }
        /****************************************************************************************/
        private void btnSave_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            OnSelectDone( dt );
            btnSave.Visible = false;
            this.Close();
        }
        /****************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            GridColumn currCol = gridMain.FocusedColumn;
            string contract = currCol.Caption;
            if ( !String.IsNullOrWhiteSpace ( contract ))
            {
                this.Cursor = Cursors.WaitCursor;
                CustomerDetails clientForm = new CustomerDetails(contract);
                clientForm.Show();
                this.Cursor = Cursors.Default;
            }
        }
        /***************************************************************************************/
        public delegate void d_void_eventdone_dt(DataTable dt);
        public event d_void_eventdone_dt SelectDone;
        protected void OnSelectDone(DataTable dt)
        {
            SelectDone?.Invoke(dt);
        }
        /****************************************************************************************/
        private void repositoryItemCheckEdit1_Click(object sender, EventArgs e)
        {
            btnSave.Show();
            try
            {
                bool isChecked = true;
                DevExpress.XtraEditors.CheckEdit select = (DevExpress.XtraEditors.CheckEdit)sender;
                if (select.Checked)
                    isChecked = false;

                GridColumn currCol = gridMain.FocusedColumn;
                string currentColumn = currCol.FieldName;
                string column = currentColumn.Replace("C", "S");

                DataTable dt = (DataTable)dgv.DataSource;
                DataRow dr = gridMain.GetFocusedDataRow();
                string service = dr[column].ObjToString();
                if ( service.IndexOf ( "Primary-") >= 0 && isChecked )
                {
                    int rowHandle = gridMain.FocusedRowHandle;
                    int row = gridMain.GetDataSourceRowIndex(rowHandle);
                    if (column != "S1")
                        dt.Rows[row]["C1"] = 0;
                    if (column != "S2")
                        dt.Rows[row]["C2"] = 0;
                    if (column != "S3")
                        dt.Rows[row]["C3"] = 0;
                    if (column != "S4")
                        dt.Rows[row]["C4"] = 0;
                    gridMain.RefreshData();
                }
                if (service == "Select Merchandise")
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        service = dt.Rows[i][column].ObjToString();
                        if (service.IndexOf("(M") == 0)
                        {
                            if (isChecked)
                                dt.Rows[i][currentColumn] = 1;
                            else
                                dt.Rows[i][currentColumn] = 0;
                        }
                    }
                }
                else if (service == "Select Services")
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        service = dt.Rows[i][column].ObjToString();
                        if (service.IndexOf("(S") == 0)
                        {
                            if (isChecked)
                                dt.Rows[i][currentColumn] = 1;
                            else
                                dt.Rows[i][currentColumn] = 0;
                        }
                    }
                }
                else if (service == "Select Cash Advance")
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        service = dt.Rows[i][column].ObjToString();
                        if (service.IndexOf("(C") == 0)
                        {
                            if (isChecked)
                                dt.Rows[i][currentColumn] = 1;
                            else
                                dt.Rows[i][currentColumn] = 0;
                        }
                    }
                }
            }
            catch ( Exception ex )
            {
            }
        }
        /****************************************************************************************/
        private void DuplicateSSN_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!btnSave.Visible)
                return;
            DialogResult result = DevExpress.XtraEditors.XtraMessageBox.Show("***Question***\nData has been modified!\nWould you like to save your changes?", "Data Modified Dialog", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if ( result == DialogResult.Cancel )
            {
                e.Cancel = true;
                return;
            }
            if (result == DialogResult.Yes)
            {
                btnSave.Visible = false;
                btnSave_Click(null, null);
            }
        }
        /****************************************************************************************/
    }
}