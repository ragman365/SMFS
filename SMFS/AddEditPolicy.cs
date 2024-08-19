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
/***************************************************************************************/
namespace SMFS
{
    /***************************************************************************************/
    public partial class AddEditPolicy : DevExpress.XtraEditors.XtraForm
    {
        private DataTable workDt = null;
        private DataTable saveDt = null;
        private string workTitle = "";
        private string workContract = "";
        private string workPayer = "";
        private bool workEditing = false;
        private bool modified = false;
        private bool loading = true;
        /***************************************************************************************/
        public AddEditPolicy( string title, DataTable dt )
        {
            InitializeComponent();
            workDt = dt;
            workTitle = title;
        }
        /***************************************************************************************/
        private void AddEditPolicy_Load(object sender, EventArgs e)
        {
            string record = workDt.Rows[0]["record"].ObjToString();
            if (!String.IsNullOrWhiteSpace(record))
            {
                workEditing = true;
                this.Text = "Update Policy for " + workTitle;
            }
            else
                this.Text = "Add NEW Policy for " + workTitle;

            btnAdd.Text = this.Text;

            workContract = workDt.Rows[0]["contractNumber"].ObjToString();
            workPayer = workDt.Rows[0]["payer"].ObjToString();
            workDt.Columns.Add("issueDate");
            if ( workEditing)
            {
                DateTime date = workDt.Rows[0]["issueDate8"].ObjToDateTime();
                string str = date.ToString("MM/dd/yyyy");
                workDt.Rows[0]["issueDate"] = str;
            }
            if ( G1.get_column_number ( workDt, "myDeceasedDate") >= 0 )
            {
                if ( G1.get_column_number ( workDt, "ddate") < 0 )
                {
                    workDt.Columns.Add("ddate");
                    if (workEditing)
                    {
                        DateTime date = workDt.Rows[0]["myDeceasedDate"].ObjToDateTime();
                        if (date.Year > 100)
                        {
                            string str = date.ToString("MM/dd/yyyy");
                            workDt.Rows[0]["ddate"] = str;
                        }
                    }
                }
            }
            dgv.DataSource = workDt;
            loading = false;
            saveDt = workDt.Copy();
        }
        /***************************************************************************************/
        private void btnAdd_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            ImportPolicyData(dt);
        }
        /***********************************************************************************************/
        private void ImportPolicyData(DataTable dt )
        {
            DataTable dx = null;
            string cmd = "";
            string record = "";
            string payer = "";
            string contract = "";
            string firstName = "";
            string lastName = "";
            string ssn = "";
            string agentCode = "";
            DateTime date = DateTime.Now;

            string lapsed = "";
            string deceasedDate = "";
            string deleteFlag = "";
            string pCode = "";

            string birthDate = "";
            string issueDate8 = "";
            string premium = "";

            string beneficiary = "";
            string liability = "";
            string companyCode = "";
            string issueAge = "";
            string type = "";
            string oldAgentInfo = "";
            string groupNumber = "";
            string policyFirstName = "";
            string policyLastName = "";
            string policyNumber = "";
            string report = "";
            string pcode = "";
            string ucode = "";
            string serviceId = "";

            int lastrow = dt.Rows.Count;
            int tableRow = 0;
            try
            {
                int start = 0;
                for (int i = start; i < lastrow; i++)
                {
                    tableRow = i;
                    record = "";
                    try
                    {
                        policyNumber = dt.Rows[i]["policyNumber"].ObjToString();
                        if (String.IsNullOrWhiteSpace(policyNumber))
                        {
                            MessageBox.Show("***ERROR*** You must enter a policy number!");
                            break;
                        }
                        payer = workPayer;
                        policyFirstName = dt.Rows[i]["policyFirstName"].ObjToString();
                        policyFirstName = G1.protect_data(policyFirstName);
                        policyLastName = dt.Rows[i]["policyLastName"].ObjToString();
                        policyLastName = G1.protect_data(policyLastName);
                        if (workEditing == false)
                        {
                            cmd = "Select * from `policies` where `payer` = '" + payer + "' AND `policyNumber` = '" + policyNumber + "' AND `policyLastName` = '" + policyLastName + "' and `policyFirstName` = '" + policyFirstName + "';";
                            dx = G1.get_db_data(cmd);
                            if (dx.Rows.Count > 0)
                            {
                                MessageBox.Show("***ERROR*** Payer/Policy Information is already assigned to someone in the database!");
                                break;
                            }
                        }

                        firstName = dt.Rows[i]["firstName"].ObjToString();
                        lastName = dt.Rows[i]["lastName"].ObjToString();
                        firstName = G1.protect_data(firstName);
                        lastName = G1.protect_data(lastName);

                        record = dt.Rows[i]["record"].ObjToString();
                        if ( !workEditing )
                            record = G1.create_record("policies", "contractNumber", "-1");
                        if (G1.BadRecord("policies", record))
                        {
                            MessageBox.Show("***ERROR*** Bad Policy Record! Record = " + record + "!");
                            break;
                        }

                        G1.update_db_table("policies", "record", record, new string[] { "contractNumber", workContract, "payer", payer, "firstName", firstName, "lastName", lastName, "policyNumber", policyNumber, "policyFirstName", policyFirstName, "policyLastName", policyLastName });

                        ssn = dt.Rows[i]["ssno"].ObjToString();

                        agentCode = dt.Rows[i]["agentCode"].ObjToString();

                        //deleteFlag = dt.Rows[i]["delete"].ObjToString();
                        pCode = dt.Rows[i]["pcode"].ObjToString();

                        birthDate = Import.GetSQLDate(dt, i, "bDate");
                        birthDate = ReturnMySqlDate(birthDate);

                        issueDate8 = Import.GetSQLDate(dt, i, "issuedate");
                        issueDate8 = ReturnMySqlDate(issueDate8);

                        deceasedDate = Import.GetSQLDate(dt, i, "dDate");
                        deceasedDate = ReturnMySqlDate(deceasedDate);

                        premium = dt.Rows[i]["premium"].ObjToString();

                        beneficiary = dt.Rows[i]["beneficiary"].ObjToString();
                        liability = dt.Rows[i]["liability"].ObjToString();
                        companyCode = dt.Rows[i]["companyCode"].ObjToString();
                        issueAge = dt.Rows[i]["issueAge"].ObjToString();
                        type = dt.Rows[i]["type"].ObjToString();
                        oldAgentInfo = dt.Rows[i]["oldAgentInfo"].ObjToString();
                        groupNumber = dt.Rows[i]["groupNumber"].ObjToString();
                        report = dt.Rows[i]["report"].ObjToString();
                        pcode = dt.Rows[i]["pcode"].ObjToString();
                        ucode = dt.Rows[i]["ucode"].ObjToString();
                        serviceId = dt.Rows[i]["ServiceId"].ObjToString();

                        G1.update_db_table("policies", "record", record, new string[] { "ssn", ssn, "pCode", pCode, "birthDate", birthDate,
                        "issueDate8", issueDate8, "premium", premium, "beneficiary", beneficiary, "liability", liability, "companyCode", companyCode, "issueAge", issueAge,
                        "type", type, "oldAgentInfo", oldAgentInfo, "groupNumber", groupNumber, "report", report, "pcode", pcode, "ucode", ucode, "agentCode", agentCode });

                        G1.update_db_table("policies", "record", record, new string[] { "deceasedDate", deceasedDate, "ServiceId", serviceId });
                        modified = false;
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                        return;
                    }
                    catch (Exception ex)
                    {
                        dt.Rows[i]["num"] = "*ERROR*";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Creating Policy Record/Row! " + contract + "/" + tableRow.ToString() + " Stopping! " + ex.Message.ObjToString());
            }
        }
        /***************************************************************************************/
        private string ReturnMySqlDate ( string dod )
        {
            string dateDeceased = "";
            if (String.IsNullOrWhiteSpace(dod))
                dateDeceased = "";
            else
                dateDeceased = dod.ObjToDateTime().ToString("MM/dd/yyyy");
            string deceasedDate = dateDeceased;
            if (deceasedDate == "0/0/0000")
                deceasedDate = "01/01/0001 12:01 AM";
            else
            {
                deceasedDate = G1.date_to_sql(deceasedDate);
                deceasedDate = deceasedDate.Replace("-", "");
            }
            return deceasedDate;
        }
        /***************************************************************************************/
        private void gridMain_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
        {
            if (e.Column.FieldName.ToUpper().IndexOf("DATE") >= 0)
            {
                if (e.RowHandle >= 0)
                {
                    if (!String.IsNullOrWhiteSpace(e.DisplayText))
                    {
                        DateTime date = e.DisplayText.ObjToDateTime();
                        if (date.Year < 1850)
                            e.DisplayText = "";
                        else
                            e.DisplayText = date.ToString("MM/dd/yyyy");
                    }
                }
            }
        }
        /***************************************************************************************/
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (loading)
                return;
            DataRow dr = gridMain.GetFocusedDataRow();
            DataTable dt = (DataTable)dgv.DataSource;
            if (e.Column.FieldName.ToUpper().IndexOf("DATE") >= 0)
            {
                string columnName = e.Column.FieldName;
                int rowhandle = gridMain.FocusedRowHandle;
                int row = gridMain.GetDataSourceRowIndex(rowhandle);
                string date = e.Value.ObjToString();
                date = dr[columnName].ObjToString();
                if (!String.IsNullOrWhiteSpace(date))
                {
                    if (!G1.validate_date(date))
                    {
                        MessageBox.Show("***ERROR*** Invalid Date Entered!");
                        dr[columnName] = saveDt.Rows[row][columnName];
                    }
                }
            }
            modified = true;
        }
        /***************************************************************************************/
        private void AddEditPolicy_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (modified)
            {
                DialogResult result = MessageBox.Show("Changes Made! Do you want to honor these changes?", "Data Modified Dialog", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation);
                if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
                else if (result == DialogResult.No)
                    return;
                DataTable dt = (DataTable)dgv.DataSource;
                ImportPolicyData(dt);
            }
        }
        /***************************************************************************************/
    }
}