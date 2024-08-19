using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;

using GeneralLib;
using DevExpress.XtraReports.UI;
using DevExpress.XtraReports.ReportGeneration;
using DevExpress.XtraPrinting;
using DevExpress.Utils;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class Agents : DevExpress.XtraEditors.XtraForm
    {
        /****************************************************************************************/
        private bool modified = false;
        private bool loading = true;
        private bool commModified = false;
        /****************************************************************************************/
        public Agents()
        {
            InitializeComponent();
            SetupTotalsSummary();
        }
        /****************************************************************************************/
        private void agents_Load(object sender, EventArgs e)
        {
            if (G1.isField())
                this.Text = "AtNeed Contacts/PreNeed Contacts";
            else
                this.Text = "Agents/AtNeed Contacts/PreNeed Contacts";

            LoadData();

            gridMain.FocusedRowHandle = 0;
            gridMain.SelectRow(0);
            SetActiveAgent();
            loading = false;
            modified = false;
            btnSave.Hide();
            btnSaveCommission.Hide();
            commModified = false;
            gridMain_FocusedRowChanged(null, null);
        }
        /****************************************************************************************/
        private void LoadData()
        {
            string cmd = "Select * from `agents` order by `agentCode`";
            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("num");
            dt.Columns.Add("mod");
            dt.Columns.Add("oldFirstName");
            dt.Columns.Add ( "oldLastName");
            string firstName = "";
            string lastName = "";
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                firstName = dt.Rows[i]["firstName"].ObjToString();
                lastName = dt.Rows[i]["lastName"].ObjToString();
                dt.Rows[i]["oldFirstName"] = firstName;
                dt.Rows[i]["oldLastName"] = lastName;
            }
            SetupGoalInfo(dt);
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;

            bool doPreneed = true;

            if (G1.isField())
            {
                tabControl1.TabPages.Remove(tabPageAgents);

                cmd = "Select * from `users` where `username` = '" + LoginForm.username + "';";
                DataTable dx = G1.get_db_data(cmd);
                if ( dx.Rows.Count > 0 )
                {
                    firstName = dx.Rows[0]["firstName"].ObjToString();
                    lastName = dx.Rows[0]["lastName"].ObjToString();
                    DataRow[] ddRows = dt.Select("firstName='" + firstName + "' AND lastName='" + lastName + "'");
                    if ( ddRows.Length <= 0 )
                    {
                        string preference = G1.getPreference(LoginForm.username, "PreNeed Contacts", "Allow Access", false);
                        if (preference.ToUpper() != "YES")
                        {
                            doPreneed = false;
                            tabControl1.TabPages.Remove(tabPagePreNeed);
                        }
                    }
                }
            }

            InitializeAtNeedPanel();

            if (doPreneed)
            {
                InitializePreNeedPanel();

                tabControl1.SelectTab(tabPagePreNeed);
            }
        }
        /****************************************************************************************/
        private void SetupGoalInfo ( DataTable dt )
        {
            string agentCode = "";
            DataTable goalDt = G1.get_db_data("Select * from `goals` where `status` = 'CURRENT' ORDER by `effectiveDate`;");
            DataTable gDt = goalDt.Clone();
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                gDt.Rows.Clear();
                agentCode = dt.Rows[i]["agentCode"].ObjToString();
                DataRow[] dRows = goalDt.Select("agentCode='" + agentCode + "'");
                for ( int j=0; j<dRows.Length; j++)
                {
                    gDt.ImportRow(dRows[j]);
                }
                ReloadAgentCommission(gDt, dt, i);
            }
        }
        /****************************************************************************************/
        private void SetupTotalsSummary()
        {
            AddSummaryColumn("recapAmount");
        }
        /****************************************************************************************/
        private void AddSummaryColumn(string columnName)
        {
            gridMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            gridMain.Columns[columnName].SummaryItem.DisplayFormat = "${0:0,0.00}";
        }
        /****************************************************************************************/
        private void btnAdd_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dRow = dt.NewRow();
            dRow["agentCode"] = "ZZZZ";
            dRow["mod"] = "Y";
            dt.Rows.Add(dRow);
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            int row = dt.Rows.Count - 1;
            gridMain.FocusedRowHandle = row;
            gridMain.RefreshData();
            dgv.Refresh();
            modified = true;
            btnSave.Show();
        }
        /****************************************************************************************/
        private void btnSave_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            DataTable dt = (DataTable)dgv.DataSource;

            ProcessNameChanges(dt);

            string record = "";
            string status = "";
            string super = "";
            string fname = "";
            string lname = "";
            string gender = "";
            string agentCode = "";
            double commission = 0D;
            double goal = 0D;
            double goalpercent = 0D;
            double fbiCommission = 0D;
            string agentIncoming = "";
            string licenseNumber = "";
            string activeStatus = "";
            string employeeStatus = "";
            string locCode = "";
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                agentCode = dt.Rows[i]["agentCode"].ObjToString();
                if (String.IsNullOrWhiteSpace(agentCode))
                    continue;
                record = dt.Rows[i]["record"].ObjToString();
                if (String.IsNullOrWhiteSpace(record))
                    record = G1.create_record("agents", "lastName", "-1");
                if (G1.BadRecord("agents", record))
                    break;
                super = dt.Rows[i]["super"].ObjToString();
                status = dt.Rows[i]["status"].ObjToString();
                fname = dt.Rows[i]["firstName"].ObjToString().Trim();
                fname = fname.TrimEnd('\n');
                lname = dt.Rows[i]["lastName"].ObjToString().Trim();
                lname = lname.TrimEnd('\n');
                gender = dt.Rows[i]["gender"].ObjToString();
                if (!String.IsNullOrWhiteSpace(gender))
                {
                    gender = gender.Substring(0, 1).ToUpper();
                    if (gender != "M" && gender != "F")
                        gender = "M";
                }
                else
                    gender = "M";
                commission = dt.Rows[i]["commission"].ObjToDouble();
                goal = dt.Rows[i]["goal"].ObjToDouble();
                goalpercent = dt.Rows[i]["goalpercent"].ObjToDouble();
                agentIncoming = dt.Rows[i]["agentIncoming"].ObjToString();
                locCode = dt.Rows[i]["locCode"].ObjToString();
                fbiCommission = dt.Rows[i]["fbiCommission"].ObjToDouble();
                licenseNumber = dt.Rows[i]["licenseNumber"].ObjToString();
                activeStatus = dt.Rows[i]["activeStatus"].ObjToString();
                employeeStatus = dt.Rows[i]["employeeStatus"].ObjToString();
                G1.update_db_table("agents", "record", record, new string[] { "super", super, "agentCode", agentCode, "status", status, "firstName", fname, "lastName", lname, "commission", commission.ToString(), "goal", goal.ToString(), "goalpercent", goalpercent.ToString(), "agentIncoming", agentIncoming, "locCode", locCode, "fbiCommission", fbiCommission.ToString(), "licenseNumber", licenseNumber, "gender", gender, "activeStatus", activeStatus, "employeeStatus", employeeStatus });
            }
            modified = false;
            btnSave.Hide();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void ProcessNameChanges ( DataTable dt )
        {
            string oldFirstName = "";
            string oldLastName = "";
            string firstName = "";
            string lastName = "";
            string name = "";
            string cmd = "";
            DataTable dx = null;
            string record = "";

            for ( int i=0; i<dt.Rows.Count; i++)
            {
                firstName = dt.Rows[i]["firstName"].ObjToString();
                lastName = dt.Rows[i]["lastName"].ObjToString();
                oldFirstName = dt.Rows[i]["oldFirstName"].ObjToString();
                oldLastName = dt.Rows[i]["oldLastName"].ObjToString();

                if ( firstName != oldFirstName || lastName != oldLastName )
                {
                    cmd = "Select * from `lapsetable` WHERE `firstName1` = '" + oldFirstName + "' AND `lastName1` = '" + oldLastName + "';";
                    dx = G1.get_db_data(cmd);
                    if ( dx.Rows.Count > 0 )
                    {
                        for ( int j=0; j<dx.Rows.Count; j++)
                        {
                            record = dx.Rows[j]["record"].ObjToString();
                        }
                    }

                    cmd = "Select * from `reinstatetable` WHERE `firstName1` = '" + oldFirstName + "' AND `lastName1` = '" + oldLastName + "';";
                    dx = G1.get_db_data(cmd);
                }
            }
        }
        /****************************************************************************************/
        private void editSplitsToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }
        /****************************************************************************************/
        private void editAdditionalGoalsToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }
        /****************************************************************************************/
        private void btnDelete_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            DataTable dt = (DataTable)dgv.DataSource;
            string agent = dt.Rows[row]["agentCode"].ObjToString();
            if (String.IsNullOrWhiteSpace(agent))
                return;
            string fname = dt.Rows[row]["firstName"].ObjToString();
            string lname = dt.Rows[row]["lastName"].ObjToString();
            string name = fname.Trim() + " " + lname.Trim();
            string record = dt.Rows[row]["record"].ObjToString();

            DialogResult result = MessageBox.Show("***Question***\nAre you sure you want to DELETE agent (" + agent + ") " + name + "?", "Delete Agents Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;
            //string cmd = "DELETE from `goals` where `record` = '" + record + "';";
            //G1.get_db_data(cmd);

            string cmd = "DELETE from `agents` where `record` = '" + record + "';";
            G1.get_db_data(cmd);
            commModified = false;
            LoadData();

            dt = (DataTable)dgv.DataSource;
            if (row >= dt.Rows.Count)
                row = dt.Rows.Count - 1;
            gridMain.FocusedRowHandle = row;
            gridMain_FocusedRowChanged(null, null);
        }
        /****************************************************************************************/
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (loading)
                return;
            DataRow dr = gridMain.GetFocusedDataRow();
            DataTable dt = (DataTable)dgv.DataSource;
            bool found = false;
            if (e.Column.FieldName.ToUpper() == "AGENTCODE")
            {
                int row = gridMain.FocusedRowHandle;
                string agent = "";
                string str = e.Value.ObjToString();
                if ( String.IsNullOrWhiteSpace ( str ))
                {
                    dr["agentCode"] = oldValue;
                    return;
                }
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    agent = dt.Rows[i]["agentCode"].ObjToString();
                    if (agent == str)
                    {
                        if (i != row)
                        {
                            dr["agentCode"] = oldValue;
                            oldValue = "";
                            gridMain.FocusedRowHandle = i;
                            gridMain.SelectRow(i);
                            found = true;
                            break;
                        }
                    }
                }
            }
            if (!found)
            {
                dr["mod"] = "Y";
                modified = true;
                btnSave.Show();
                oldValue = "";
            }
        }
        /****************************************************************************************/
        private void Agents_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!modified)
                return;
            DialogResult result = MessageBox.Show("***Question***\nInformation has been modified!\nWould you like to save your changes?", "Add/Edit Agents Dialog", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
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
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            loading = true;
            modified = false;
            LoadData();
            loading = false;
            modified = false;
            btnSave.Hide();
        }
        /****************************************************************************************/
        private string oldValue = "";
        private int oldAgentRow = -1;
        /****************************************************************************************/
        private void gridMain_CellValueChanging(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            string columnName = e.Column.FieldName.ToUpper();
            if (columnName != "AGENTCODE")
            {
                oldValue = "";
                return;
            }
            DataRow dr = gridMain.GetFocusedDataRow();
            DataTable dt = (DataTable)dgv.DataSource;
            int row = e.RowHandle;
            if (String.IsNullOrWhiteSpace(oldValue))
                oldValue = dt.Rows[row]["agentCode"].ObjToString();
        }
        /****************************************************************************************/
        private void gridMain_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
            if (loading)
                return;
            if (commModified)
            {
                if (CheckSaveGoals())
                {
                    if (oldAgentRow >= 0)
                    {
                        loading = true;
                        gridMain.FocusedRowHandle = oldAgentRow;
                        gridMain.SelectRow(oldAgentRow);
                        loading = false;
                        return;
                    }
                }
            }
            SetActiveAgent();
            oldAgentRow = gridMain.FocusedRowHandle;
        }
        /****************************************************************************************/
        private void SetActiveAgent()
        {
            btnSaveCommission.Hide();
            commModified = false;
            DataRow dr = gridMain.GetFocusedDataRow();
            if (dr == null)
            {
                dgv2.DataSource = null;
                return;
            }
            string agent = dr["agentCode"].ObjToString();
            string cmd = "Select * from `goals` g LEFT JOIN `agents` a ON g.`agentCode` = a.`agentCode` WHERE g.`agentCode` = '" + agent + "' ORDER by `effectiveDate` DESC;";
            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("num");
            dt.Columns.Add("mod");
            dt.Columns.Add("edate");
            if ( dt.Rows.Count <= 0 )
            {
                dgv2.DataSource = dt;
                return;
            }
            string str = "";
            long days = 0;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                str = dt.Rows[i]["effectiveDate"].ObjToString();
                days = G1.date_to_days(str);
                str = G1.days_to_date(days);
                dt.Rows[i]["edate"] = str;
            }
            G1.NumberDataTable(dt);
            dgv2.DataSource = dt;
        }
        /****************************************************************************************/
        private void gridMain2_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (loading)
                return;
            DataRow dr = gridMain2.GetFocusedDataRow();
            DataTable dt = (DataTable)dgv2.DataSource;
            dr["mod"] = "Y";
            commModified = true;
            btnSaveCommission.Show();
            if (e.Column.FieldName.ToUpper() == "EDATE")
            {
                string str = dr["edate"].ObjToString();
                dr["effectiveDate"] = G1.DTtoMySQLDT(str);
                long days = G1.date_to_days(str);
                str = G1.days_to_date(days);
                dr["edate"] = str;
            }
        }
        /****************************************************************************************/
        private bool CheckSaveGoals()
        {
            bool rv = false;
            DialogResult result = MessageBox.Show("***Question***\nAgent Commission Definitions have been modified!\nWould you like to save your changes?", "Add/Edit Agent Commissions Dialog", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (result == DialogResult.Cancel)
                rv = true;
            else if (result == DialogResult.Yes)
                SaveCommissions();
            return rv;
        }
        /****************************************************************************************/
        private void SaveCommissions()
        {
            this.Cursor = Cursors.WaitCursor;
            DataTable dt = (DataTable)dgv2.DataSource;
            string record = "";
            string status = "";
            string type = "";
            string agentCode = "";
            string customGoals = "";
            string mod = "";
            string str = "";
            string edate = "";
            string splits = "";
            double percent;
            double goal;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                agentCode = dt.Rows[i]["agentCode"].ObjToString();
                if (String.IsNullOrWhiteSpace(agentCode))
                    continue;
                edate = G1.GetSQLDate(dt, i, "effectiveDate");
                //                edate = dt.Rows[i]["edate"].ObjToString();
                //                dt.Rows[i]["effectiveDate"] = edate; // Just for Testing. Needs Work
                mod = dt.Rows[i]["mod"].ObjToString();
                if (mod.ToUpper() != "Y")
                    continue;
                record = dt.Rows[i]["record"].ObjToString();
                if (String.IsNullOrWhiteSpace(record))
                    record = G1.create_record("goals", "type", "-1");
                if (G1.BadRecord("goals", record))
                    break;
                str = dt.Rows[i]["effectiveDate"].ObjToString();
                status = dt.Rows[i]["status"].ObjToString();
                type = dt.Rows[i]["type"].ObjToString();
                customGoals = dt.Rows[i]["formula"].ObjToString();
                percent = dt.Rows[i]["percent"].ObjToDouble();
                percent = G1.RoundValue(percent);
                goal = dt.Rows[i]["goal"].ObjToDouble();
                goal = G1.RoundValue(goal);
                splits = dt.Rows[i]["splits"].ObjToString();

                G1.update_db_table("goals", "record", record, new string[] { "agentCode", agentCode, "status", status, "formula", customGoals, "type", type, "effectiveDate", edate, "percent", percent.ToString(), "goal", goal.ToString(), "splits", splits });
                dt.Rows[i]["mod"] = "";
            }
            DataTable mainDt = (DataTable)dgv.DataSource;
            ReloadAgentCommission(dt, mainDt);
            commModified = false;
            btnSaveCommission.Hide();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private int GetAgentRow(string agent, DataTable dt)
        {
            int row = -1;
            string str = "";
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                str = dt.Rows[i]["agentCode"].ObjToString();
                if ( str == agent)
                {
                    row = i;
                    break;
                }
            }
            return row;
        }
        /****************************************************************************************/
        private void ReloadAgentCommission(DataTable dt, DataTable mainDt, int actualRow = -1)
        {
            double standardCommission = 0D;
            string standardSplit = "";
            double goal = 0D;
            double goalCommission = 0D;
            string goalSplit = "";
            string goalFormula = "";
            string type = "";
            string status = "";
            bool foundStandard = false;
            string agentCode = "";
            if (actualRow >= 0)
                agentCode = mainDt.Rows[actualRow]["agentCode"].ObjToString();
            bool foundGoal = false;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                status = dt.Rows[i]["status"].ObjToString();
                if (status.ToUpper() != "CURRENT")
                    continue;
                agentCode = dt.Rows[i]["agentCode"].ObjToString();
                type = dt.Rows[i]["type"].ObjToString();
                if (type.ToUpper() == "STANDARD")
                {
                    standardCommission = dt.Rows[i]["percent"].ObjToDouble();
                    standardSplit = dt.Rows[i]["splits"].ObjToString();
                    foundStandard = true;
                }
                else if ( type.ToUpper() == "GOAL")
                {
                    goal = dt.Rows[i]["goal"].ObjToDouble();
                    goalCommission = dt.Rows[i]["percent"].ObjToDouble();
                    goalSplit = dt.Rows[i]["splits"].ObjToString();
                    goalFormula = dt.Rows[i]["formula"].ObjToString();
                    foundGoal = true;
                }
                if (foundStandard && foundGoal)
                    break;
            }
            if (String.IsNullOrWhiteSpace(agentCode))
                return;
//            DataTable mainDt = (DataTable)dgv.DataSource;
            int row = actualRow;
            if ( row < 0 )
                row = GetAgentRow(agentCode, mainDt);
            if (row >= 0)
            {
                mainDt.Rows[row]["splits"] = standardSplit;
                mainDt.Rows[row]["commission"] = standardCommission;
                mainDt.Rows[row]["goal"] = goal;
                mainDt.Rows[row]["additionalGoals"] = goalSplit;
                mainDt.Rows[row]["goalPercent"] = goalCommission;
                mainDt.Rows[row]["customGoals"] = goalFormula;
                dgv.RefreshDataSource();
            }
        }
        /****************************************************************************************/
        private void btnAddCommission_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string agent = dr["agentCode"].ObjToString();
            string fname = dr["firstName"].ObjToString();
            string lname = dr["lastName"].ObjToString();

            DataTable dt = (DataTable)dgv2.DataSource;
            DataRow dRow = dt.NewRow();
            dRow["agentCode"] = agent;
            dRow["firstName"] = fname;
            dRow["lastName"] = lname;
            dRow["mod"] = "Y";
            dt.Rows.Add(dRow);
            G1.NumberDataTable(dt);
            dgv2.DataSource = dt;
            commModified = true;
            btnSaveCommission.Show();
        }
        /****************************************************************************************/
        private void btnDeleteCommission_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain2.GetFocusedDataRow();
            int rowHandle = gridMain2.FocusedRowHandle;
            int row = gridMain2.GetDataSourceRowIndex(rowHandle);
            DataTable dt = (DataTable)dgv2.DataSource;
            string agent = dt.Rows[row]["agentCode"].ObjToString();
            if (String.IsNullOrWhiteSpace(agent))
                return;
            string fname = dt.Rows[row]["firstName"].ObjToString();
            string lname = dt.Rows[row]["lastName"].ObjToString();
            string name = fname.Trim() + " " + lname.Trim();

            DialogResult result = MessageBox.Show("***Question***\nAre you sure you want to DELETE GOAL for agent (" + agent + ") " + name + "?", "Delete Agents Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;

            string record = dt.Rows[row]["record"].ObjToString();
            G1.delete_db_table("goals", "record", record);
            SetActiveAgent();



            //dr["status"] = "Inactive";
            //dr["mod"] = "Y";
            //commModified = true;
        }
        /****************************************************************************************/
        private void btnSaveCommission_Click(object sender, EventArgs e)
        {
            SaveCommissions();
        }
        /****************************************************************************************/
        private void btnAssignSplits_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain2.GetFocusedDataRow();
            string splits = dr["splits"].ObjToString();
            string agent = dr["agentCode"].ObjToString();
            string type = dr["type"].ObjToString();
            double percent = dr["percent"].ObjToDouble();
            using (AgentSplits agentForm = new AgentSplits(agent, splits, percent))
            {
                DialogResult result = agentForm.ShowDialog();
                if ( result == DialogResult.OK )
                {
                    dr["splits"] = agentForm.agentSplits;
                    dr["mod"] = "Y";
                    commModified = true;
                    btnSaveCommission.Show();
                }
            }
        }
        /****************************************************************************************/
        private void pictureBox2_Click(object sender, EventArgs e)
        {
            G1.ShowHideFindPanel(gridMain);

            //if (gridMain.OptionsFind.AlwaysVisible == true)
            //    gridMain.OptionsFind.AlwaysVisible = false;
            //else
            //    gridMain.OptionsFind.AlwaysVisible = true;
        }
        /***********************************************************************************************/
        private Contacts editAtNeedContacts = null;
        private bool AtNeedModified = false;
        private void InitializeAtNeedPanel(bool justLoading = false)
        {
            if (editAtNeedContacts != null)
                editAtNeedContacts.Close();
            editAtNeedContacts = null;
            AtNeedModified = false;

            editAtNeedContacts = new Contacts ();
            //editFunPayments.paymentClosing += EditFunPayments_paymentClosing;
            if (!this.LookAndFeel.UseDefaultLookAndFeel)
            {
                editAtNeedContacts.LookAndFeel.UseDefaultLookAndFeel = false;
                editAtNeedContacts.LookAndFeel.SetSkinStyle(this.LookAndFeel.SkinName);
            }

            G1.LoadFormInPanel(editAtNeedContacts, this.tabPageAtNeed );
        }
        /***********************************************************************************************/
        private ContactsPreneed editPreNeedContacts = null;
        private bool PreNeedModified = false;
        private void InitializePreNeedPanel(bool justLoading = false)
        {
            if (editPreNeedContacts != null)
                editPreNeedContacts.Close();
            editPreNeedContacts = null;
            PreNeedModified = false;


            editPreNeedContacts = new ContactsPreneed();
            //editFunPayments.paymentClosing += EditFunPayments_paymentClosing;
            if (!this.LookAndFeel.UseDefaultLookAndFeel)
            {
                editPreNeedContacts.LookAndFeel.UseDefaultLookAndFeel = false;
                editPreNeedContacts.LookAndFeel.SetSkinStyle(this.LookAndFeel.SkinName);
            }

            G1.LoadFormInPanel(editPreNeedContacts, this.tabPagePreNeed);
        }
        /****************************************************************************************/
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        /***********************************************************************************************/
        private int pageMarginLeft = 0;
        private int pageMarginRight = 0;
        private int pageMarginTop = 0;
        private int pageMarginBottom = 0;
        private bool isPrinting = false;
        /***********************************************************************************************/
        private void printPreviewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            isPrinting = true;
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

            Printer.setupPrinterMargins(10, 10, 80, 50);

            pageMarginLeft = Printer.pageMarginLeft;
            pageMarginRight = Printer.pageMarginRight;
            pageMarginTop = Printer.pageMarginTop;
            pageMarginBottom = Printer.pageMarginBottom;

            printableComponentLink1.Margins.Left = pageMarginLeft;
            printableComponentLink1.Margins.Right = pageMarginRight;
            printableComponentLink1.Margins.Top = pageMarginTop;
            printableComponentLink1.Margins.Bottom = pageMarginBottom;

            printableComponentLink1.CreateDocument();
            printableComponentLink1.ShowPreview();
            isPrinting = false;
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

            font = new Font("Ariel", 12);
            string text = this.Text;
            Printer.DrawQuad(5, 7, 5, 4, text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);

            font = new Font("Ariel", 10, FontStyle.Bold);

            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /****************************************************************************************/
        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            isPrinting = true;
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

            Printer.setupPrinterMargins(10, 10, 80, 50);

            pageMarginLeft = Printer.pageMarginLeft;
            pageMarginRight = Printer.pageMarginRight;
            pageMarginTop = Printer.pageMarginTop;
            pageMarginBottom = Printer.pageMarginBottom;

            printableComponentLink1.Margins.Left = pageMarginLeft;
            printableComponentLink1.Margins.Right = pageMarginRight;
            printableComponentLink1.Margins.Top = pageMarginTop;
            printableComponentLink1.Margins.Bottom = pageMarginBottom;

            printableComponentLink1.CreateDocument();
            printableComponentLink1.PrintDlg();
            isPrinting = false;
        }
        /****************************************************************************************/
    }
}