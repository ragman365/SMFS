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
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraEditors.Repository;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class EditPolicyTrusts : DevExpress.XtraEditors.XtraForm
    {
        private bool modified = false;
        private DataTable originalDt = null;
        private string _answer = "";
        private bool loading = true;
        public string A_Answer { get { return _answer; } }
        /****************************************************************************************/
        public EditPolicyTrusts()
        {
            InitializeComponent();
        }
        /****************************************************************************************/
        private void EditPolicyTrusts_Load(object sender, EventArgs e)
        {
            this.btnSave.Hide();
            _answer = "";
            string cmd = "Select * from `policyTrusts` p LEFT JOIN `contracts` c ON p.`contractNumber` = c.`contractNumber`;";
            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("mod");
            G1.NumberDataTable(dt);
            originalDt = dt;
            dgv.DataSource = dt;
            loading = false;
        }
        /****************************************************************************************/
        private void btnSave_Click(object sender, EventArgs e)
        {
            string record = "";
            string policyNumber = "";
            string contractNumber = "";
            string badContractNumber = "";
            string mod = "";
            string type = "";
            string cmd = "";
            DataTable dx = null;

            string company = "";

            DataTable dt = (DataTable)dgv.DataSource;

            bool gotBadTrustNumber = false;
            if (G1.get_column_number(dt, "badTrustNumber") >= 0)
                gotBadTrustNumber = true;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    mod = dt.Rows[i]["mod"].ObjToString();
                    record = dt.Rows[i]["record"].ObjToString();
                    if (mod == "D" && String.IsNullOrWhiteSpace(record))
                        continue;
                    if (mod == "D")
                    {
                        G1.delete_db_table("policyTrusts", "record", record);
                        continue;
                    }
                    if (String.IsNullOrWhiteSpace(mod))
                        continue;
                    if (mod.ToUpper() != "Y")
                        continue;

                    policyNumber = dt.Rows[i]["policyNumber"].ObjToString();

                    type = dt.Rows[i]["type"].ObjToString();
                    if ( type.ToUpper() == "PB")
                    {
                        policyNumber = dt.Rows[i]["policyNumber"].ObjToString();
                        if (!String.IsNullOrWhiteSpace(policyNumber))
                        {
                            cmd = "Select * from `policytrusts` WHERE `policyNumber` = '" + policyNumber + "';";
                        }
                        else
                        {
                            contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                            if (String.IsNullOrWhiteSpace(contractNumber))
                                continue;
                            cmd = "Select * from `policytrusts` WHERE `contractNumber` = '" + contractNumber + "';";
                        }
                        dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count > 0)
                            record = dx.Rows[0]["record"].ObjToString();
                    }
                    else
                    {
                        if (String.IsNullOrWhiteSpace(policyNumber))
                            continue;
                    }

                    if (String.IsNullOrWhiteSpace(record))
                        record = "-1";
                    if (record == "-1")
                        record = G1.create_record("policyTrusts", "contractNumber", "-1");
                    if (G1.BadRecord("policyTrusts", record))
                        continue;
                    policyNumber = dt.Rows[i]["policyNumber"].ObjToString();
                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                    badContractNumber = "";
                    if ( gotBadTrustNumber )
                        badContractNumber = dt.Rows[i]["badTrustNumber"].ObjToString();
                    company = dt.Rows[i]["Company"].ObjToString();
                    G1.update_db_table("policyTrusts", "record", record, new string[] { "policyNumber", policyNumber, "contractNumber", contractNumber, "type", type, "Company", company, "badTrustNumber", badContractNumber });
                    dt.Rows[i]["mod"] = "";
                    dt.Rows[i]["record"] = record;
                }
                catch ( Exception ex)
                {
                }
            }
            modified = false;
            btnSave.Hide();
        }
        /****************************************************************************************/
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            dr["mod"] = "Y";
            modified = true;
            btnSave.Show();
        }
        /****************************************************************************************/
        private void picDelete_Click(object sender, EventArgs e)
        {
            int row = 0;
            int[] Rows = gridMain.GetSelectedRows();
            for (int i = 0; i < Rows.Length; i++)
            {
                modified = true;
                row = Rows[i];
                DataRow dr = gridMain.GetDataRow(row);
                dr["mod"] = "D";
                btnSave.Show();
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
        private void gridMain_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv.DataSource;
            string delete = dt.Rows[row]["mod"].ObjToString();
            if (delete.ToUpper() == "D")
            {
                e.Visible = false;
                e.Handled = true;
            }
        }
        /****************************************************************************************/
        private void picAdd_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dRow = dt.NewRow();
            dt.Rows.Add(dRow);
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;

            G1.GoToLastRow(gridMain);

            modified = true;
        }
        /****************************************************************************************/
        private void EditTracking_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!modified)
                return;
            DialogResult result = MessageBox.Show("***Question***\nData has been modified!\nWould you like to save your changes?", "Add/Edit Policy/Trusts Dialog", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (result == DialogResult.Cancel)
            {
                e.Cancel = true;
                return;
            }
            modified = false;
            if (result == DialogResult.No)
                return;
            btnSave_Click(null, null);
        }
        /****************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string contract = dr["contractNumber"].ObjToString();
            if (!String.IsNullOrWhiteSpace(contract))
            {
                this.Cursor = Cursors.WaitCursor;
                CustomerDetails clientForm = new CustomerDetails(contract);
                clientForm.Show();
                this.Cursor = Cursors.Default;
            }
        }
        /***********************************************************************************************/
        private string actualFile = "";
        private DataTable workDt = null;
        private void btnImport_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string file = ofd.FileName;
                    int idx = file.LastIndexOf("\\");
                    if (idx > 0)
                    {
                        actualFile = file.Substring(idx);
                        actualFile = actualFile.Replace("\\", "");
                    }
                    dgv.DataSource = null;
                    this.Cursor = Cursors.WaitCursor;
                    workDt = null;
                    try
                    {
                        workDt = Import.ImportCSVfile(file);

                        if (workDt != null)
                        {
                            if (workDt.Rows.Count > 0)
                            {
                                workDt.Columns.Add("record");
                                workDt.Columns.Add("mod");

                                int columnNumber = G1.get_column_number(workDt, "Policy Number");
                                if (columnNumber >= 0)
                                {
                                    workDt.Columns[columnNumber].ColumnName = "policyNumber";
                                    workDt.Columns[columnNumber].Caption = "policyNumber";
                                }
                                columnNumber = G1.get_column_number(workDt, "Policy ID");
                                if (columnNumber >= 0)
                                {
                                    workDt.Columns[columnNumber].ColumnName = "policyNumber";
                                    workDt.Columns[columnNumber].Caption = "policyNumber";
                                }
                                columnNumber = G1.get_column_number(workDt, "Trust Number");
                                if (columnNumber >= 0)
                                {
                                    workDt.Columns[columnNumber].ColumnName = "contractNumber";
                                    workDt.Columns[columnNumber].Caption = "contractNumber";
                                }
                                columnNumber = G1.get_column_number(workDt, "CONTRACT NUMBER");
                                if (columnNumber >= 0)
                                {
                                    workDt.Columns[columnNumber].ColumnName = "contractNumber";
                                    workDt.Columns[columnNumber].Caption = "contractNumber";
                                }
                                for ( int i=0; i<workDt.Rows.Count; i++)
                                {
                                    workDt.Rows[i]["mod"] = "Y";
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                    }

                    workDt.TableName = actualFile;

                    string company = "";
                    if (actualFile.ToUpper().IndexOf("FORETHOUGHT") >= 0)
                    {
                        company = "FORETHOUGHT";
                        workDt.Columns.Add("Company");
                        workDt.Columns.Add("type");
                        for (int i = 0; i < workDt.Rows.Count; i++)
                            workDt.Rows[i]["Company"] = company;
                    }
                    else if (actualFile.ToUpper().IndexOf("UNITY") >= 0)
                    {
                        company = "UNITY";
                        workDt.Columns.Add("Company");
                        workDt.Columns.Add("type");
                        for (int i = 0; i < workDt.Rows.Count; i++)
                            workDt.Rows[i]["Company"] = company;
                    }
                    else if (actualFile.ToUpper().IndexOf("FDLIC") >= 0)
                    {
                        company = "FDLIC";
                        workDt.Columns.Add("Company");
                        workDt.Columns.Add("type");
                        for (int i = 0; i < workDt.Rows.Count; i++)
                            workDt.Rows[i]["Company"] = company;
                    }


                    G1.NumberDataTable(workDt);
                    dgv.DataSource = workDt;

                    btnSave.Show();
                }
            }
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void gridMain_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            if (e.ListSourceRowIndex == DevExpress.XtraGrid.GridControl.InvalidRowHandle)
                return;
            double dValue = 0D;
            if (e.Column.FieldName.ToUpper().IndexOf("DATE") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                if (e.DisplayText.IndexOf("0000") >= 0 || e.DisplayText.IndexOf("0001") >= 0)
                    e.DisplayText = "";
                else
                {
                    DateTime date = e.DisplayText.ObjToString().ObjToDateTime();
                    e.DisplayText = date.ToString("MM/dd/yyyy");
                    if (date.Year < 30)
                        e.DisplayText = "";
                }
            }

            if (e.DisplayText.Trim() == "0.00")
                e.DisplayText = "";
        }
        /****************************************************************************************/
        private void btnImportPB_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string file = ofd.FileName;
                    int idx = file.LastIndexOf("\\");
                    if (idx > 0)
                    {
                        actualFile = file.Substring(idx);
                        actualFile = actualFile.Replace("\\", "");
                    }
                    dgv.DataSource = null;
                    this.Cursor = Cursors.WaitCursor;
                    workDt = null;
                    try
                    {
                        workDt = Import.ImportCSVfile(file);

                        if (workDt != null)
                        {
                            if (workDt.Rows.Count > 0)
                            {
                                workDt.Columns.Add("record");
                                workDt.Columns.Add("mod");
                                workDt.Columns.Add("type");

                                int columnNumber = G1.get_column_number(workDt, "Policy #");
                                if (columnNumber >= 0)
                                {
                                    workDt.Columns[columnNumber].ColumnName = "policyNumber";
                                    workDt.Columns[columnNumber].Caption = "policyNumber";
                                }
                                columnNumber = G1.get_column_number(workDt, "Trust Number");
                                if (columnNumber >= 0)
                                {
                                    workDt.Columns[columnNumber].ColumnName = "contractNumber";
                                    workDt.Columns[columnNumber].Caption = "contractNumber";
                                }
                                for (int i = 0; i < workDt.Rows.Count; i++)
                                {
                                    workDt.Rows[i]["mod"] = "Y";
                                    workDt.Rows[i]["type"] = "PB";
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                    }

                    workDt.TableName = actualFile;


                    G1.NumberDataTable(workDt);
                    dgv.DataSource = workDt;

                    btnSave.Show();
                }
            }
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            G1.SpyGlass(gridMain);
        }
        /****************************************************************************************/
    }
}