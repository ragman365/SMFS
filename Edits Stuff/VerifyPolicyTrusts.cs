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
using DevExpress.XtraPrinting;
using DevExpress.Utils;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class VerifyPolicyTrusts : DevExpress.XtraEditors.XtraForm
    {
        private bool modified = false;
        private DataTable originalDt = null;
        private string _answer = "";
        private bool loading = true;
        public string A_Answer { get { return _answer; } }
        /****************************************************************************************/
        public VerifyPolicyTrusts()
        {
            InitializeComponent();
        }
        /****************************************************************************************/
        private void VerifyPolicyTrusts_Load(object sender, EventArgs e)
        {
            this.btnSave.Hide();
            _answer = "";
            string cmd = "Select * from `policyTrusts` p LEFT JOIN `contracts` c ON p.`contractNumber` = c.`contractNumber` LEFT JOIN `customers` x ON p.`contractNumber` = x.`contractNumber`;";
             //cmd = "Select * from `trust_data` WHERE `trustCompany` = 'FDLIC PB';";
            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("mod");
            //dt.Columns.Add("type");
            G1.NumberDataTable(dt);
            originalDt = dt;
            dgv.DataSource = dt;
            loading = false;
            barImport.Hide();
            lblTotal.Hide();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void btnSave_Click(object sender, EventArgs e)
        {
            string record = "";
            string policyNumber = "";
            string contractNumber = "";
            string mod = "";
            string type = "";
            string company = "";
            string cmd = "";
            DataTable dx = null;

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
                        G1.delete_db_table("policyTrusts", "record", record);
                        continue;
                    }
                    if (String.IsNullOrWhiteSpace(mod))
                        continue;
                    if (mod.ToUpper() != "Y")
                        continue;

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

                    if (String.IsNullOrWhiteSpace(record))
                        record = "-1";
                    if (record == "-1")
                        record = G1.create_record("policyTrusts", "contractNumber", "-1");
                    if (G1.BadRecord("policyTrusts", record))
                        continue;
                    policyNumber = dt.Rows[i]["policyNumber"].ObjToString();
                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                    company = dt.Rows[i]["company"].ObjToString();
                    G1.update_db_table("policyTrusts", "record", record, new string[] { "policyNumber", policyNumber, "contractNumber", contractNumber, "type", type, "company", company });
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
            string record = "";
            DataRow dr = gridMain.GetFocusedDataRow();
            string contract = dr["contractNumber"].ObjToString();
            if (!String.IsNullOrWhiteSpace(contract))
            {
                this.Cursor = Cursors.WaitCursor;
                string cmd = "";
                cmd = "Select * from `contracts` WHERE `contractNumber` = '" + contract + "';";
                DataTable dx = G1.get_db_data(cmd);
                if (dx.Rows.Count <= 0)
                {
                    DialogResult result = MessageBox.Show("***ERROR*** Customer Contract Does Not Exist!\nWould you like to create it anyway?", "Contract Error Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    if (result == DialogResult.No)
                    {
                        this.Cursor = Cursors.Default;
                        return;
                    }
                    record = G1.create_record("contracts", "contractNumber", "-1");
                    if ( G1.BadRecord ( "contracts", record ))
                    {
                        this.Cursor = Cursors.Default;
                        return;
                    }
                    G1.update_db_table("contracts", "record", record, new string[] {"contractNumber", contract });
                }

                cmd = "Select * from `customers` WHERE `contractNumber` = '" + contract + "';";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count <= 0)
                {
                    DialogResult result = MessageBox.Show("***ERROR*** Customer Does Not Exist!\nDo you want to create it\nand then edit ?", "Customer Error Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    if (result == DialogResult.No)
                    {
                        this.Cursor = Cursors.Default;
                        return;
                    }
                    record = G1.create_record("customers", "contractNumber", "-1");
                    if (G1.BadRecord("customers", record))
                    {
                        this.Cursor = Cursors.Default;
                        return;
                    }
                    G1.update_db_table("customers", "record", record, new string[] { "contractNumber", contract });
                }

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
            string name = "";
            string firstName = "";
            string middleName = "";
            string lastName = "";
            string contractNumber = "";
            double premium = 0D;
            double oldPremium = 0D;
            string[] Lines = null;
            DataRow[] dRows = null;
            DataTable dx = null;
            string cmd = "";

            DataTable dt = (DataTable)dgv.DataSource;

            if (G1.get_column_number(dt, "Premium") < 0)
                dt.Columns.Add("Premium", Type.GetType("System.Double"));

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
                    //dgv.DataSource = null;
                    this.Cursor = Cursors.WaitCursor;
                    workDt = null;
                    try
                    {
                        workDt = Import.ImportCSVfile(file);
                        if ( 1 == 1 )
                        {
                            WorkNewImport(workDt);
                            this.Cursor = Cursors.Default;
                            return;
                        }

                        if (G1.get_column_number(workDt, "lastName") < 0)
                        {
                            try
                            {
                                workDt.Columns.Add("lastName");
                                workDt.Columns.Add("firstName");
                                workDt.Columns.Add("middleName");
                            }
                            catch (Exception ex)
                            {
                            }
                        }

                        if (workDt != null)
                        {
                            if (workDt.Rows.Count > 0)
                            {
                                workDt.Columns.Add("record");
                                workDt.Columns.Add("mod");
                            }
                            for (int i = 0; i < workDt.Rows.Count; i++)
                            {
                                name = workDt.Rows[i]["insured Name"].ObjToString();
                                Lines = name.Split('/');
                                lastName = "";
                                firstName = "";
                                middleName = "";
                                if (Lines.Length > 0)
                                    lastName = Lines[0].Trim();
                                if (Lines.Length > 1)
                                    firstName = Lines[1].Trim();
                                if (Lines.Length > 2)
                                    middleName = Lines[2].Trim();
                                workDt.Rows[i]["lastName"] = lastName;
                                Lines = firstName.Split(' ');
                                if (Lines.Length > 0)
                                    firstName = Lines[0].Trim();
                                if (Lines.Length > 1)
                                    middleName = Lines[1].Trim();

                                workDt.Rows[i]["firstName"] = firstName;
                                workDt.Rows[i]["middleName"] = middleName;
                            }

                            for ( int i=0; i<workDt.Rows.Count; i++)
                            {
                                lastName = workDt.Rows[i]["lastName"].ObjToString();
                                firstName = workDt.Rows[i]["firstName"].ObjToString();
                                middleName = workDt.Rows[i]["middleName"].ObjToString();
                                contractNumber = workDt.Rows[i]["Trust Number"].ObjToString();
                                if (String.IsNullOrWhiteSpace(contractNumber))
                                    continue;
                                if ( lastName == "Fortenberry")
                                {
                                }

                                premium = workDt.Rows[i]["Premium"].ObjToDouble();

                                dRows = dt.Select("contractNumber='" + contractNumber + "'");
                                if (dRows.Length > 0)
                                    continue;
                                if ( !String.IsNullOrWhiteSpace ( middleName ))
                                    dRows = dt.Select("lastName='" + lastName + "' AND firstName='" + firstName + "' AND middleName='" + middleName + "' AND trustCompany='FDLIC PB'");
                                else
                                    dRows = dt.Select("lastName='" + lastName + "' AND firstName='" + firstName + "' AND trustCompany='FDLIC PB'");
                                if ( dRows.Length > 0 )
                                {
                                    dx = dRows.CopyToDataTable();
                                    for ( int j=0; j<dRows.Length; j++)
                                    {
                                        dRows[j]["contractNumber"] = contractNumber;
                                        dRows[j]["Premium"] = premium;
                                    }
                                }
                            }
                        }

                        FindRemainder(dt);
                    }
                    catch (Exception ex)
                    {
                    }

                    workDt.TableName = actualFile;


                    G1.NumberDataTable(workDt);
                    //dgv.DataSource = workDt;

                    //btnSave.Show();
                }
            }
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void FindRemainder ( DataTable dt )
        {
            string firstName = "";
            string lastName = "";
            string middleName = "";
            string contractNumber = "";
            string cmd = "";
            DataTable dx = null;
            DataRow[] dRows = dt.Select("contractNumber=''");
            for ( int i=0; i<dRows.Length; i++)
            {
                lastName = dRows[i]["lastName"].ObjToString();
                if ( lastName.ToUpper() == "AMIS")
                {
                }
                firstName = dRows[i]["firstName"].ObjToString();
                middleName = dRows[i]["middleName"].ObjToString();
                if (!String.IsNullOrWhiteSpace(middleName))
                    firstName += " " + middleName;
                cmd = "Select * from `customers` where `lastName` = '" + lastName + "' AND `firstName` = '" + firstName + "';";
                dx = G1.get_db_data(cmd);
                if ( dx.Rows.Count <= 0 )
                {
                    cmd = "Select * from `customers` where `lastName` = '" + lastName + "' AND `firstName` LIKE '" + firstName + "%';";
                    dx = G1.get_db_data(cmd);
                }
                if ( dx.Rows.Count > 0 )
                {
                    contractNumber = dx.Rows[0]["contractNumber"].ObjToString();
                    dRows[i]["contractNumber"] = contractNumber;
                    dRows[i]["type"] = "CustomerFile";
                    cmd = "Select * from `trust2013r` where `contractNumber` = '" + contractNumber + "' AND `endingBalance` > '0' ORDER by `payDate8` DESC LIMIT 1";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                        dRows[i]["premium"] = dx.Rows[0]["endingBalance"].ObjToDouble();
                }
            }
            return;
        }
        /****************************************************************************************/
        private void WorkNewImport ( DataTable workDt )
        {
            DataTable dt = (DataTable)dgv.DataSource;

            if (G1.get_column_number(dt, "lastName") < 0)
            {
                try
                {
                    dt.Columns.Add("lastName");
                    dt.Columns.Add("firstName");
                    dt.Columns.Add("middleName");
                }
                catch (Exception ex)
                {
                }
            }
            if (G1.get_column_number(dt, "insuredName") < 0)
                dt.Columns.Add("insuredName");


                string contractNumber = "";
            string newContract = "";
            DataRow[] dRows = null;
            DataRow dR = null;
            string cmd = "";
            DataTable dx = null;
            string firstName = "";
            string lastName = "";
            string middleName = "";
            string fromWhere = "";
            double premium = 0D;
            double endingBalance = 0D;
            DateTime deceasedDate = DateTime.Now;
            DateTime lapsedDate8 = DateTime.Now;
            DateTime dueDate8 = DateTime.Now;
            string insuredName = "";
            string possible = "";
            char c;
            string oldType = "";

            if (G1.get_column_number(dt, "endingBalance") < 0)
                dt.Columns.Add("endingBalance", Type.GetType("System.Double"));

            barImport.Minimum = 0;
            barImport.Maximum = workDt.Rows.Count;
            barImport.Show();
            lblTotal.Text = workDt.Rows.Count.ToString();
            lblTotal.Show();
            lblTotal.Refresh();

            for ( int i=0; i<workDt.Rows.Count; i++)
            {
                Application.DoEvents();

                barImport.Value = i + 1;
                barImport.Refresh();

                try
                {
                    this.Cursor = Cursors.WaitCursor;
                    contractNumber = workDt.Rows[i]["Trust Number"].ObjToString();
                    if (String.IsNullOrWhiteSpace(contractNumber))
                        continue;
                    premium = workDt.Rows[i]["Premium"].ObjToDouble();
                    insuredName = workDt.Rows[i]["Insured Name"].ObjToString();

                    firstName = "";
                    lastName = "";
                    middleName = "";
                    endingBalance = 0D;
                    fromWhere = "";
                    deceasedDate = DateTime.MinValue;
                    dueDate8 = DateTime.MinValue;
                    lapsedDate8 = DateTime.MinValue;

                    dRows = dt.Select("contractNumber='" + contractNumber + "'");
                    if (dRows.Length <= 0)
                    {
                        cmd = "Select * from `contracts` WHERE `contractNumber` = '" + contractNumber + "';";
                        dx = G1.get_db_data(cmd);

                        if ( dx.Rows.Count <= 0 )
                        {
                            if (Char.IsLetter(contractNumber, contractNumber.Length - 1))
                            {
                                newContract = contractNumber.Substring(0, contractNumber.Length - 1);
                                cmd = "Select * from `contracts` WHERE `contractNumber` = '" + newContract + "';";
                                dx = G1.get_db_data(cmd);
                            }
                        }

                        if (dx.Rows.Count > 0)
                        {
                            dueDate8 = dx.Rows[0]["dueDate8"].ObjToDateTime();
                            deceasedDate = dx.Rows[0]["deceasedDate"].ObjToDateTime();
                            lapsedDate8 = dx.Rows[0]["lapseDate8"].ObjToDateTime();

                            cmd = "Select * from `customers` WHERE `contractNumber` = '" + contractNumber + "';";
                            dx = G1.get_db_data(cmd);
                            if (dx.Rows.Count > 0)
                            {
                                firstName = dx.Rows[0]["firstName"].ObjToString();
                                lastName = dx.Rows[0]["lastName"].ObjToString();
                                middleName = dx.Rows[0]["middleName"].ObjToString();

                                cmd = "Select * from `trust2013r` where `contractNumber` = '" + contractNumber + "' AND `endingBalance` > '0' ORDER by `payDate8` DESC LIMIT 1";
                                dx = G1.get_db_data(cmd);
                                if (dx.Rows.Count > 0)
                                    endingBalance = dx.Rows[0]["endingBalance"].ObjToDouble();
                            }
                            else
                                CheckCustomer(contractNumber, ref fromWhere, ref lastName, ref firstName, ref endingBalance);
                        }
                        else
                        {
                            CheckCustomer(contractNumber, ref fromWhere, ref lastName, ref firstName, ref endingBalance);
                        }

                        try
                        {
                            dR = dt.NewRow();
                            dR["contractNumber"] = contractNumber;
                            dR["mod"] = "Y";
                            dR["type"] = "PB" + " " + fromWhere;
                            dR["firstName"] = firstName;
                            dR["lastName"] = lastName;
                            dR["middleName"] = middleName;
                            dR["endingBalance"] = endingBalance;
                            dR["Premium"] = premium;
                            dR["lapseDate8"] = G1.DTtoMySQLDT(lapsedDate8);
                            dR["dueDate8"] = G1.DTtoMySQLDT(dueDate8);
                            dR["deceasedDate"] = G1.DTtoMySQLDT(deceasedDate);
                            dR["insuredName"] = insuredName;
                            dt.Rows.Add(dR);
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                    else
                    {
                        cmd = "Select * from `contracts` WHERE `contractNumber` = '" + contractNumber + "';";
                        dx = G1.get_db_data(cmd);

                        if (dx.Rows.Count <= 0)
                        {
                            newContract = contractNumber.Substring(0, contractNumber.Length - 1);
                            cmd = "Select * from `contracts` WHERE `contractNumber` = '" + newContract + "';";
                            dx = G1.get_db_data(cmd);
                        }

                        if ( dx.Rows.Count > 0 )
                        {
                            dueDate8 = dx.Rows[0]["dueDate8"].ObjToDateTime();
                            deceasedDate = dx.Rows[0]["deceasedDate"].ObjToDateTime();
                            lapsedDate8 = dx.Rows[0]["lapseDate8"].ObjToDateTime();
                        }

                        CheckCustomer(contractNumber, ref fromWhere, ref lastName, ref firstName, ref endingBalance);

                        for (int j = 0; j < dRows.Length; j++)
                        {
                            if (j == 0)
                                dRows[j]["type"] = "PB " + fromWhere + " FFF";
                            else
                                dRows[j]["type"] = "PB " + fromWhere + " DUP";
                            dRows[j]["lastName"] = lastName;
                            dRows[j]["firstName"] = firstName;
                            dRows[j]["endingBalance"] = endingBalance;
                            dRows[j]["Premium"] = premium;
                            dRows[j]["lapseDate8"] = G1.DTtoMySQLDT(lapsedDate8);
                            dRows[j]["dueDate8"] = G1.DTtoMySQLDT(dueDate8);
                            dRows[j]["deceasedDate"] = G1.DTtoMySQLDT(deceasedDate);
                            dRows[j]["insuredName"] = insuredName;
                            dRows[j]["mod"] = "Y";
                        }
                    }
                }
                catch ( Exception ex )
                {
                }
            }
            this.Cursor = Cursors.Default;
            dgv.DataSource = dt;
            gridMain.RefreshData();
            gridMain.RefreshEditor(true);
            dgv.Refresh();
        }
        /****************************************************************************************/
        private void CheckCustomer ( string contractNumber, ref string fromWhere, ref string lastName, ref string firstName, ref double premium )
        {
            fromWhere = "";
            firstName = "";
            lastName = "";
            premium = 0D;

            string trustNumber = "";
            string cmd = "Select * from `customers` where `contractNumber` = '" + contractNumber + "';";
            DataTable dx = G1.get_db_data(cmd);
            if ( dx.Rows.Count <= 0 )
            {
                if (Char.IsLetter(contractNumber, contractNumber.Length - 1))
                {
                    trustNumber = contractNumber.Substring(0, contractNumber.Length - 1);
                    cmd = "Select * from `customers` where `contractNumber` = '" + trustNumber + "';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                        fromWhere = "M";
                }
            }
            else
            {
                fromWhere = "C";
            }

            if ( dx.Rows.Count > 0 )
            {
                lastName = dx.Rows[0]["lastName"].ObjToString();
                firstName = dx.Rows[0]["firstName"].ObjToString();
            }
            else
            {
                cmd = "Select * from `trust2013r` WHERE `contractNumber` = '" + contractNumber + "' LIMIT 1;";
                dx = G1.get_db_data(cmd);
                if ( dx.Rows.Count <= 0 && !String.IsNullOrWhiteSpace ( trustNumber))
                {
                    cmd = "Select * from `trust2013r` WHERE `contractNumber` = '" + trustNumber + "' LIMIT 1;";
                    dx = G1.get_db_data(cmd);
                }
                if ( dx.Rows.Count > 0 )
                {
                    fromWhere = "T";
                    lastName = dx.Rows[0]["lastName"].ObjToString();
                    firstName = dx.Rows[0]["firstName"].ObjToString();
                }
            }

            cmd = "Select * from `trust2013r` where `contractNumber` = '" + contractNumber + "' AND `endingBalance` > '0' ORDER by `payDate8` DESC LIMIT 1";
            dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0 && !String.IsNullOrWhiteSpace(trustNumber))
            {
                cmd = "Select * from `trust2013r` where `contractNumber` = '" + trustNumber + "' AND `endingBalance` > '0' ORDER by `payDate8` DESC LIMIT 1";
                dx = G1.get_db_data(cmd);
            }
            if (dx.Rows.Count > 0)
                premium = dx.Rows[0]["endingBalance"].ObjToDouble();
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
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }
        /***********************************************************************************************/
        private int pageMarginLeft = 0;
        private int pageMarginRight = 0;
        private int pageMarginTop = 0;
        private int pageMarginBottom = 0;
        /***********************************************************************************************/
        private void printPreviewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            gridMain.OptionsPrint.ExpandAllGroups = false;

            if (this.components == null)
                this.components = new System.ComponentModel.Container();

            DevExpress.XtraPrinting.PrintingSystem printingSystem1 = new DevExpress.XtraPrinting.PrintingSystem(this.components);
            DevExpress.XtraPrinting.PrintableComponentLink printableComponentLink1 = new DevExpress.XtraPrinting.PrintableComponentLink(this.components);

            printingSystem1.Links.AddRange(new object[] {
            printableComponentLink1});


            printableComponentLink1.Component = dgv;
            printableComponentLink1.PrintingSystemBase = printingSystem1;

            printableComponentLink1.EnablePageDialog = true;

            printableComponentLink1.CreateDetailHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateDetailHeaderArea);
            printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);
            printableComponentLink1.BeforeCreateAreas += new System.EventHandler(this.printableComponentLink1_BeforeCreateAreas);
            printableComponentLink1.AfterCreateAreas += new System.EventHandler(this.printableComponentLink1_AfterCreateAreas);
            printableComponentLink1.Landscape = true;

            Printer.setupPrinterMargins(10, 5, 80, 50);

            pageMarginLeft = Printer.pageMarginLeft;
            pageMarginRight = Printer.pageMarginRight;
            pageMarginTop = Printer.pageMarginTop;
            pageMarginBottom = Printer.pageMarginBottom;

            printableComponentLink1.Margins.Left = pageMarginLeft;
            printableComponentLink1.Margins.Right = pageMarginRight;
            printableComponentLink1.Margins.Top = pageMarginTop;
            printableComponentLink1.Margins.Bottom = pageMarginBottom;

            printingSystem1.Document.AutoFitToPagesWidth = 1;

            Font saveFont = gridMain.AppearancePrint.Row.Font;

            printableComponentLink1.CreateDocument();
            printableComponentLink1.ShowPreview();

            gridMain.Appearance.Row.Font = saveFont;
        }
        /***********************************************************************************************/
        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            gridMain.OptionsPrint.ExpandAllGroups = false;

            if (this.components == null)
                this.components = new System.ComponentModel.Container();
            DevExpress.XtraPrinting.PrintingSystem printingSystem1 = new DevExpress.XtraPrinting.PrintingSystem(this.components);
            DevExpress.XtraPrinting.PrintableComponentLink printableComponentLink1 = new DevExpress.XtraPrinting.PrintableComponentLink(this.components);

            printingSystem1.Links.AddRange(new object[] {
            printableComponentLink1});

            printableComponentLink1.Component = dgv;
            printableComponentLink1.PrintingSystemBase = printingSystem1;
            printableComponentLink1.CreateDetailHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateDetailHeaderArea);
            printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);
            printableComponentLink1.BeforeCreateAreas += new System.EventHandler(this.printableComponentLink1_BeforeCreateAreas);
            printableComponentLink1.AfterCreateAreas += new System.EventHandler(this.printableComponentLink1_AfterCreateAreas);
            printableComponentLink1.Landscape = true;

            Printer.setupPrinterMargins(10, 5, 80, 50);

            pageMarginLeft = Printer.pageMarginLeft;
            pageMarginRight = Printer.pageMarginRight;
            pageMarginTop = Printer.pageMarginTop;
            pageMarginBottom = Printer.pageMarginBottom;

            printableComponentLink1.Margins.Left = pageMarginLeft;
            printableComponentLink1.Margins.Right = pageMarginRight;
            printableComponentLink1.Margins.Top = pageMarginTop;
            printableComponentLink1.Margins.Bottom = pageMarginBottom;

            printingSystem1.Document.AutoFitToPagesWidth = 1;

            printableComponentLink1.CreateDocument();
            printableComponentLink1.PrintDlg();
        }
        /***********************************************************************************************/
        private void printableComponentLink1_BeforeCreateAreas(object sender, EventArgs e)
        {
        }
        /***********************************************************************************************/
        private void printableComponentLink1_AfterCreateAreas(object sender, EventArgs e)
        {
        }
        /***********************************************************************************************/
        private void printableComponentLink1_CreateDetailHeaderArea(object sender, CreateAreaEventArgs e)
        {
        }
        /***********************************************************************************************/
        private void printableComponentLink1_CreateMarginalHeaderArea(object sender, CreateAreaEventArgs e)
        {
            Printer.setupPrinterQuads(e, 2, 3);
            Font font = new Font("Ariel", 16);
            Printer.DrawQuad(1, 1, Printer.xQuads, 2, "South Mississippi Funeral Services, LLC", Color.Black, BorderSide.Top, font, HorizontalAlignment.Center);

            Printer.SetQuadSize(12, 12);

            font = new Font("Ariel", 8);
            Printer.DrawGridDate(2, 3, 2, 3, Color.Black, BorderSide.None, font);
            Printer.DrawGridPage(11, 3, 2, 3, Color.Black, BorderSide.None, font);

            Printer.DrawQuad(1, 9, 2, 3, "User : " + LoginForm.username, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);

            font = new Font("Ariel", 10, FontStyle.Regular);
            //string title = "Contract Activity Report";
            //Printer.DrawQuad(6, 8, 4, 4, title, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            string reportName = this.Text;
            string report = reportName;
            Printer.DrawQuad(5, 8, 8, 4, report, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);



            //DateTime date = this.dateTimePicker1.Value;
            //string workDate = date.Month.ToString("D2") + "/" + date.Year.ToString("D4");
            Printer.SetQuadSize(24, 12);
            font = new Font("Ariel", 9, FontStyle.Regular);
            //Printer.DrawQuad(20, 8, 5, 4, "Month Closing - " + workDate, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            //Printer.DrawQuad(16, 8, 3, 4, lblPayment.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(19, 8, 3, 4, lblTrust85.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(22, 8, 3, 4, lblTrust100.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /****************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            G1.SpyGlass(gridMain);
        }
        /****************`************************************************************************/
        private void btnVerifyContracts_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;

            string contractNumber = "";
            string cmd = "";
            DataTable dx = null;
            string type = "";

            barImport.Show();
            barImport.Refresh();

            barImport.Minimum = 0;
            barImport.Maximum = dt.Rows.Count;
            barImport.Show();
            lblTotal.Text = dt.Rows.Count.ToString();
            lblTotal.Show();
            lblTotal.Refresh();


            for (int i = 0; i < dt.Rows.Count; i++)
            {
                Application.DoEvents();

                barImport.Value = i + 1;
                barImport.Refresh();

                try
                {
                    type = dt.Rows[i]["type"].ObjToString();
                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString().Trim();
                    if ( contractNumber.ToUpper() == "M13170U")
                    {

                    }
                    cmd = "Select * from `contracts` WHERE `contractNumber` = '" + contractNumber + "';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count <= 0)
                    {
                        if (type.ToUpper().IndexOf("PB") >= 0)
                            dt.Rows[i]["type"] = "PB T FFF";
                        else
                            dt.Rows[i]["type"] = "T FFF";
                        continue;
                    }
                    cmd = "Select * from `customers` WHERE `contractNumber` = '" + contractNumber + "';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count <= 0)
                    {
                        if (type.ToUpper().IndexOf("PB") >= 0)
                            dt.Rows[i]["type"] = "PB C FFF";
                        else
                            dt.Rows[i]["type"] = "C FFF";
                        continue;
                    }
                    if (type.ToUpper().IndexOf("PB") >= 0)
                        dt.Rows[i]["type"] = "PB";
                    else
                        dt.Rows[i]["type"] = "";
                }
                catch ( Exception ex)
                {

                }
            }
        }
        /****************************************************************************************/
    }
}