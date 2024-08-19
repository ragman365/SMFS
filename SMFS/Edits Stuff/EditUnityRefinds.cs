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
    public partial class EditUnityRefunds : DevExpress.XtraEditors.XtraForm
    {
        private bool modified = false;
        private DataTable originalDt = null;
        private string _answer = "";
        private bool loading = true;
        public string A_Answer { get { return _answer; } }
        /****************************************************************************************/
        public EditUnityRefunds()
        {
            InitializeComponent();
        }
        /****************************************************************************************/
        private void EditUnityRefunds_Load(object sender, EventArgs e)
        {
            this.btnSave.Hide();
            _answer = "";
            string cmd = "Select * from `unityrefunds` p JOIN `contracts` c ON p.`contractNumber` = c.`contractNumber`;";
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
            string contractNumber = "";
            double refund = 0D;
            string mod = "";
            string type = "";
            string cmd = "";
            DataTable dx = null;

            string company = "";

            DataTable dt = (DataTable)dgv.DataSource;
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
                        if ( !String.IsNullOrWhiteSpace ( record ))
                            G1.delete_db_table("unityrefunds", "record", record);
                        continue;
                    }
                    if (String.IsNullOrWhiteSpace(mod))
                        continue;
                    if (mod.ToUpper() != "Y")
                        continue;

                    if (String.IsNullOrWhiteSpace(record))
                        record = "-1";
                    if (record == "-1")
                        record = G1.create_record("unityrefunds", "contractNumber", "-1");
                    if (G1.BadRecord("unityrefunds", record))
                        continue;
                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                    refund = dt.Rows[i]["unityRefund"].ObjToDouble();
                    G1.update_db_table("unityrefunds", "record", record, new string[] { "contractNumber", contractNumber, "unityRefund", refund.ToString() });
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
                        workDt = ExcelWriter.ReadFile2(file, 0 );

                        workDt.TableName = actualFile;
                    }
                    catch (Exception ex)
                    {
                    }

                    if (workDt != null)
                    {
                        workDt.TableName = actualFile;

                        workDt = PreProcessUnity(workDt);

                        DataTable dt = new DataTable();
                        dt.Columns.Add("record");
                        dt.Columns.Add("contractNumber");
                        dt.Columns.Add("unityRefund", Type.GetType("System.Double"));
                        dt.Columns.Add("mod");

                        string contractNumber = "";
                        double refund = 0D;

                        DataRow dRow = null;

                        for ( int i=0; i<workDt.Rows.Count; i++)
                        {
                            contractNumber = workDt.Rows[i]["CONTRACT #"].ObjToString();
                            if (String.IsNullOrWhiteSpace(contractNumber))
                                continue;
                            if (contractNumber == "WF18055LI")
                                continue;
                            if (contractNumber == "HT18008LI")
                                continue;
                            if (contractNumber == "B17064LI")
                                continue;
                            if (contractNumber == "WF18075LI")
                                continue;
                            refund = workDt.Rows[i]["BEGINNING VALUE"].ObjToDouble();
                            if (refund <= 0D)
                                continue;
                            dRow = dt.NewRow();
                            dRow["contractNumber"] = contractNumber;
                            dRow["unityRefund"] = refund;
                            dRow["mod"] = "Y";
                            dt.Rows.Add(dRow);
                        }

                        G1.NumberDataTable(dt);
                        dgv.DataSource = dt;

                        btnSave.Show();
                    }
                }
            }
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private DataTable PreProcessUnity(DataTable dt)
        {
            int firstRow = -1;
            string search = "CONTRACT #";
            string str = "";
            DataTable newDt = dt.Clone();

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    str = dt.Rows[i][j].ObjToString().ToUpper();
                    if (str == search)
                    {
                        firstRow = i;
                        break;
                    }
                }
                if (firstRow >= 0)
                    break;
            }
            if (firstRow < 0)
                return newDt;
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                str = dt.Rows[firstRow][i].ObjToString();
                if (String.IsNullOrWhiteSpace(str))
                    continue;
                if (G1.get_column_number(dt, str) >= 0)
                {
                    for (; ; )
                    {
                        str = str + "2";
                        if (G1.get_column_number(dt, str) < 0)
                            break;
                    }
                }
                newDt.Columns[i].ColumnName = str;
                newDt.Columns[i].Caption = str;

                dt.Columns[i].ColumnName = str;
                dt.Columns[i].Caption = str;
            }
            //string policyNumber = "";
            //for (int i = (firstRow + 1); i < dt.Rows.Count; i++)
            //{
            //    str = dt.Rows[i]["POLICY NUMBER"].ObjToString().ToUpper();
            //    if (String.IsNullOrWhiteSpace(str))
            //        continue;
            //    policyNumber = str;
            //    str = dt.Rows[i]["FH NAME"].ObjToString();
            //    if (String.IsNullOrWhiteSpace(str))
            //        continue;
            //    if (str.ToUpper() == "FH NAME")
            //    {
            //        if (!G1.validate_numeric(policyNumber))
            //            continue;
            //    }
            //    //if (!G1.validate_numeric(str))
            //    //    continue;
            //    newDt.ImportRow(dt.Rows[i]);
            //}
            return dt;
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
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            G1.SpyGlass(gridMain);
        }
        /****************************************************************************************/
    }
}