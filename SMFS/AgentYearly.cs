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
using DevExpress.Utils;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid;
using DevExpress.XtraPrinting;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class AgentYearly : DevExpress.XtraEditors.XtraForm
    {
        private string workTable = "";
        private string workColumns = "";
        private bool modified = false;
        /****************************************************************************************/
        public AgentYearly()
        {
            InitializeComponent();
        }
        /****************************************************************************************/
        private void AgentYearly_Load(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            now = now.AddMonths(-1);
            now = new DateTime(now.Year, now.Month, 1);
            this.dateTimePicker1.Value = now;
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            this.dateTimePicker2.Value = new DateTime(now.Year, now.Month, days);

            btnSave.Hide();

            modified = false;
            SetupTotalsSummary();
            LoadData();
        }
        /****************************************************************************************/
        private void SetupTotalsSummary()
        {
            AddSummaryColumn("mycommission", null);
        }
        /****************************************************************************************/
        private void AddSummaryColumn(string columnName, DevExpress.XtraGrid.Views.Grid.GridView gMain = null)
        {
            if (gMain == null)
                gMain = gridMain;
            gMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            gMain.Columns[columnName].SummaryItem.DisplayFormat = "{0:N2}";
        }
        /****************************************************************************************/
        private void LoadData ()
        {

            string cmd = "Select * from `agents` order by `agentCode`";
            DataTable dt = G1.get_db_data(cmd);

            dt.Columns.Add("num");
            dt.Columns.Add("mod");
            dt.Columns.Add("agentCodes");
            dt.Columns.Add("mycommission", Type.GetType("System.Decimal"));


            dt = LoadNames(dt);

            DataTable dx = dt.Copy();

            dt = G1.RemoveDuplicates(dt, "name");

            string firstName = "";
            string lastName = "";
            string prefix = "";
            string suffix = "";
            string mi = "";
            string name = "";
            DataRow[] dRows = null;
            string agentCodes = "";
            string agentCode = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                name = dt.Rows[i]["name"].ObjToString();
                if ( name.ToUpper().IndexOf ( "GAMMILL") >= 0 )
                {
                }
                firstName = dt.Rows[i]["firstName"].ObjToString();
                lastName = dt.Rows[i]["lastName"].ObjToString();
                //G1.ParseOutName(name, ref prefix, ref firstName, ref lastName, ref mi, ref suffix);
                if ( !String.IsNullOrWhiteSpace ( firstName ) && !String.IsNullOrWhiteSpace ( lastName ))
                {
                    dRows = dx.Select("firstName='" + firstName + "' AND lastName='" + lastName + "'");
                    if ( dRows.Length > 0 )
                    {
                        agentCodes = "";
                        for ( int j=0; j<dRows.Length; j++)
                        {
                            agentCode = dRows[j]["agentCode"].ObjToString();
                            agentCodes += agentCode + ",";
                        }
                        agentCodes = agentCodes.TrimEnd(',');
                        dt.Rows[i]["agentCodes"] = agentCodes;
                    }
                }
            }

            //LoadCommissions(dt);

            //try
            //{
            //    string activeStatus = cmbActiveStatus.Text.Trim().ToUpper();
            //    if (activeStatus != "ALL")
            //    {
            //        if (activeStatus == "ACTIVE")
            //        {
            //            dRows = dt.Select("activeStatus='Active'");
            //            if (dRows.Length > 0)
            //                dt = dRows.CopyToDataTable();
            //            dRows = dt.Select("activeStatus='Inactive' AND mycommission > '0'");
            //            if (dRows.Length > 0)
            //            {
            //                for (int j = 0; j < dRows.Length; j++)
            //                    dt.ImportRow(dRows[j]);
            //            }
            //        }
            //        else if ( activeStatus == "GONE")
            //        {
            //            dRows = dt.Select("activeStatus='Gone'");
            //            if (dRows.Length > 0)
            //                dt = dRows.CopyToDataTable();
            //        }
            //        else
            //        {
            //            dRows = dt.Select("activeStatus='Inactive'");
            //            if (dRows.Length > 0)
            //                dt = dRows.CopyToDataTable();
            //        }
            //    }

            //    string employeeStatus = cmbEmployeeStatus.Text.Trim().ToUpper();
            //    if (employeeStatus != "ALL")
            //    {
            //        if (employeeStatus == "FULL TIME")
            //        {
            //            dRows = dt.Select("employeeStatus<>'Part TIme'");
            //            if (dRows.Length > 0)
            //                dt = dRows.CopyToDataTable();
            //        }
            //        else
            //        {
            //            dRows = dt.Select("employeeStatus='Part Time'");
            //            if (dRows.Length > 0)
            //                dt = dRows.CopyToDataTable();
            //        }
            //    }

            //    if ( activeStatus == "ALL" )
            //    {
            //        DataView tempview = dt.DefaultView;
            //        tempview.Sort = "activeStatus asc, name asc, employeeStatus asc";
            //        dt = tempview.ToTable();
            //    }
            //    if (employeeStatus == "ALL")
            //    {
            //        DataView tempview = dt.DefaultView;
            //        tempview.Sort = "activeStatus asc, name asc, employeeStatus asc";
            //        dt = tempview.ToTable();
            //    }
            //}
            //catch ( Exception ex)
            //{
            //}

            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
        }
        /****************************************************************************************/
        private DataTable CleanupAgents ( DataTable dt )
        {
            DataRow[] dRows = null;
            DataTable dx = dt.Copy();

            try
            {
                string activeStatus = cmbActiveStatus.Text.Trim().ToUpper();
                if (activeStatus != "ALL")
                {
                    if (activeStatus == "ACTIVE")
                    {
                        dRows = dt.Select("activeStatus='Active'");
                        if (dRows.Length > 0)
                            dt = dRows.CopyToDataTable();
                        dRows = dx.Select("activeStatus='Inactive' AND mycommission > '0'");
                        if (dRows.Length > 0)
                        {
                            for (int j = 0; j < dRows.Length; j++)
                                dt.ImportRow(dRows[j]);
                        }
                    }
                    else if (activeStatus == "GONE")
                    {
                        dRows = dt.Select("activeStatus='Gone'");
                        if (dRows.Length > 0)
                            dt = dRows.CopyToDataTable();
                    }
                    else
                    {
                        dRows = dt.Select("activeStatus='Inactive'");
                        if (dRows.Length > 0)
                            dt = dRows.CopyToDataTable();
                    }
                }

                string employeeStatus = cmbEmployeeStatus.Text.Trim().ToUpper();
                if (employeeStatus != "ALL")
                {
                    if (employeeStatus == "FULL TIME")
                    {
                        //dRows = dt.Select("employeeStatus<>'Part TIme'");
                        dRows = dt.Select("employeeStatus<>'Contract Labor'");
                        if (dRows.Length > 0)
                            dt = dRows.CopyToDataTable();
                    }
                    else
                    {
                        //dRows = dt.Select("employeeStatus='Part Time'");
                        dRows = dt.Select("employeeStatus='Contract Labor'");
                        if (dRows.Length > 0)
                            dt = dRows.CopyToDataTable();
                    }
                }

                if (activeStatus == "ALL")
                {
                    DataView tempview = dt.DefaultView;
                    tempview.Sort = "activeStatus asc, lname asc, employeeStatus asc";
                    dt = tempview.ToTable();
                }
                if (employeeStatus == "ALL")
                {
                    DataView tempview = dt.DefaultView;
                    tempview.Sort = "activeStatus asc, lname asc, employeeStatus asc";
                    dt = tempview.ToTable();
                }
            }
            catch (Exception ex)
            {
            }

            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            return dt;
        }
        /****************************************************************************************/
        private void RunTheData ()
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt == null)
                return;

            if (cmbActiveStatus.Text.Trim().ToUpper() != "GONE")
            {
                DataRow[] dRows = dt.Select("activeStatus<>'Gone'");
                if (dRows.Length > 0)
                    dt = dRows.CopyToDataTable();
            }

            this.Cursor = Cursors.WaitCursor;
            LoadCommissions(dt);

            G1.NumberDataTable(dt);
            dgv.DataSource = dt;

            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void LoadCommissions(DataTable dt)
        {
            DateTime last = DateTime.Now;
            int days = 0;

            int monthCol = 0;

            string cmd = "";
            string splits = "";
            DataTable dx = null;

            string commissionType = cmbType.Text.Trim();


            string runNumber = "";

            string firstName = "";
            string lastName = "";

            string workAgentCodes = "";
            string workAgent = "";
            double totalCommission = 0D;
            double editedCommission = 0D;

            double splitCommission = 0D;
            double splitBaseCommission = 0D;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                workAgentCodes = dt.Rows[i]["agentCodes"].ObjToString();
                if ( workAgentCodes.IndexOf ( "B12") >= 0 )
                {
                }
                workAgent = dt.Rows[i]["name"].ObjToString();

                firstName = dt.Rows[i]["firstName"].ObjToString();
                lastName = dt.Rows[i]["lastName"].ObjToString();

                //decodeAgentName(workAgent, ref firstName, ref lastName);

                if ( lastName.ToUpper() == "GAMMILL")
                {
                }


                DateTime begin = this.dateTimePicker1.Value;
                DateTime end = this.dateTimePicker2.Value;
                begin = begin.AddMonths(-1);

                DataRow[] dRows = null;
                totalCommission = 0D;


                for (; ; )
                {
                    begin = begin.AddMonths(1);
                    days = DateTime.DaysInMonth(begin.Year, begin.Month);
                    last = new DateTime(begin.Year, begin.Month, days);
                    if (last > end)
                        break;

                    cmd = "Select * from `lapse_reinstates` where `startDate` = '" + begin.ToString("yyyy-MM-dd") + "' AND `endDate` = '" + last.ToString("yyyy-MM-dd") + "';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count <= 0)
                        continue;
                    runNumber = dx.Rows[0]["record"].ObjToString();

                    cmd = "Select * from `historic_commissions` where `runNumber` = '" + runNumber + "';";
                    dx = G1.get_db_data(cmd);

                    if (dx.Rows.Count <= 0)
                        continue;

                    if (commissionType.ToUpper().IndexOf("STANDARD") >= 0)
                    {
                        //dRows = dx.Select("firstName='" + firstName + "' AND lastName='" + lastName + "' AND type='Standard'");
                        dRows = dx.Select("firstName='" + firstName + "' AND lastName='" + lastName + "' ");
                    }
                    else
                        dRows = dx.Select("firstName='" + firstName + "' AND lastName='" + lastName + "' AND type='Goal'");
                    if (dRows.Length <= 0)
                        continue;
                    dx = dRows.CopyToDataTable();

                    double commission = 0D;
                    string commType = "";

                    for (int j = 0; j < dx.Rows.Count; j++)
                    {
                        commType = dx.Rows[j]["type"].ObjToString().ToUpper();
                        splits = dx.Rows[j]["splits"].ObjToString();

                        splitBaseCommission = dx.Rows[j]["splitBaseCommission"].ObjToDouble();
                        splitCommission = dx.Rows[j]["splitCommission"].ObjToDouble();

                        if (commissionType.ToUpper().IndexOf("STANDARD") >= 0)
                        {
                            if (commType == "GOAL")
                            {
                                if (splitBaseCommission > 0D )
                                    commission += splitBaseCommission;
                                else
                                    commission += dx.Rows[j]["totalCommission"].ObjToDouble() - dx.Rows[j]["contractCommission"].ObjToDouble();
                            }
                            else if (!String.IsNullOrWhiteSpace(splits))
                                commission += dx.Rows[j]["splitBaseCommission"].ObjToDouble();
                            else
                                commission += dx.Rows[j]["totalCommission"].ObjToDouble();
                        }
                        else
                        {
                            commission += dx.Rows[j]["totalCommission"].ObjToDouble();
                        }
                    }

                    editedCommission = 0D;
                    if (LoadAgentCustom(workAgent, last, commissionType, ref editedCommission))
                        commission = editedCommission;

                    commission = G1.RoundValue(commission);

                    totalCommission += commission;

                }
                dt.Rows[i]["mycommission"] = totalCommission;
            }
        }
        /****************************************************************************************/
        private void decodeAgentName(string name, ref string firstName, ref string lastName)
        {
            firstName = "";
            lastName = "";
            string prefix = "";
            string suffix = "";
            string mi = "";

            G1.ParseOutName(name, ref prefix, ref firstName, ref lastName, ref mi, ref suffix);
        }
        /****************************************************************************************/
        private DataTable LoadNames ( DataTable dt)
        {
            if ( G1.get_column_number ( dt, "name") < 0 )
                dt.Columns.Add("name");
            if (G1.get_column_number(dt, "lname") < 0)
                dt.Columns.Add("lname");
            string firstName = "";
            string lastName = "";
            string name = "";
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                firstName = dt.Rows[i]["firstName"].ObjToString();
                lastName = dt.Rows[i]["lastName"].ObjToString();
                name = firstName + " " + lastName;
                dt.Rows[i]["name"] = name;
                dt.Rows[i]["lname"] = lastName;
            }
            DataView tempview = dt.DefaultView;
            tempview.Sort = "lname";
            dt = tempview.ToTable();

            return dt;
        }
        /****************************************************************************************/
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            DataTable dt = (DataTable)dgv.DataSource;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            dt.Rows[row]["mod"] = "Y";
            btnSave.Show();
            btnSave.Refresh();
            modified = true;
        }
        /****************************************************************************************/
        private void gridMain_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
        }
        /****************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        { // Spy Glass
            G1.ShowHideFindPanel(gridMain);
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
            if (!modified)
                return;
            DialogResult result = MessageBox.Show("***Question***\nData has been modified!\nWould you like to save your changes?", "Options Changed Dialog", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
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
        private void repositoryItemComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            modified = true;
        }
        /****************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            string commissionType = cmbType.Text.Trim();
            DataRow dr = gridMain.GetFocusedDataRow();
            string agent = dr["name"].ObjToString();
            if (String.IsNullOrWhiteSpace(agent))
                return;
            string agentCodes = dr["agentCodes"].ObjToString();
            DateTime begin = this.dateTimePicker1.Value;
            DateTime end = this.dateTimePicker2.Value;

            this.Cursor = Cursors.WaitCursor;

            AgentYearlyCommissions yearlyForm = new AgentYearlyCommissions(dt, commissionType, agent, agentCodes, begin, end);
            yearlyForm.Show();
            this.Cursor = Cursors.Default;

            //DateTime last = DateTime.Now;
            //int days = 0;

            //string cmd = "";
            //DataTable dx = null;

            //string runNumber = "";

            //for (; ;)
            //{
            //    days = DateTime.DaysInMonth(begin.Year, begin.Month);
            //    last = new DateTime(begin.Year, begin.Month, days);

            //    cmd = "Select * from `lapse_reinstates` where `startDate` = '" + begin.ToString("yyyy-MM-dd") + "' AND `endDate` = '" + last.ToString("yyyy-MM-dd") + "';";
            //    dx = G1.get_db_data(cmd);
            //    if (dx.Rows.Count <= 0)
            //        break;
            //    runNumber = dx.Rows[0]["record"].ObjToString();

            //    cmd = "Select * from `historic_commissions` where `runNumber` = '" + runNumber + "';";
            //    dx = G1.get_db_data(cmd);
            //    break;
            //}

            //this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void btnSave_Click(object sender, EventArgs e)
        {
            //string mod = "";
            //string name = "";
            //string who = "";
            //string option = "";
            //string answer = "";
            //string record = "";
            //DataTable dx = null;
            //DataRow[] dRows = null;
            //string cmd = "";
            //DataTable dt = (DataTable)dgv.DataSource;

            //int startColumn = G1.get_column_number(dt, "name");
            //startColumn = startColumn + 1;

            //this.Cursor = Cursors.WaitCursor;

            //for ( int i=0; i<dt.Rows.Count; i++)
            //{
            //    mod = dt.Rows[i]["mod"].ObjToString();
            //    if (mod != "Y")
            //        continue;
            //    name = dt.Rows[i]["name"].ObjToString();
            //    who = dt.Rows[i]["ma"].ObjToString();
            //    cmd = "Select * from `funcommissiondata` where `name` = '" + name + "' AND `ma` = '" + who + "';";
            //    dx = G1.get_db_data(cmd);

            //    for ( int j=startColumn; j<dt.Columns.Count; j++)
            //    {
            //        option = dt.Columns[j].ColumnName.ObjToString().Trim();
            //        if (option.ToUpper() == "MOD") // Don't Save This as an option
            //            continue;
            //        dRows = optionDt.Select("option='" + option + "'");
            //        if (dRows.Length > 0)
            //            who = dRows[0]["who"].ObjToString();
            //        answer = dt.Rows[i][j].ObjToString();
            //        dRows = dx.Select("option='" + option + "'");
            //        try
            //        {
            //            if (dRows.Length > 0)
            //            {
            //                record = dRows[0]["record"].ObjToString();
            //                G1.update_db_table("funcommissiondata", "record", record, new string[] { "name", name, "ma", who, "option", option, "answer", answer });
            //            }
            //            else
            //            {
            //                record = G1.create_record("funcommissiondata", "option", "-1");
            //                if (G1.BadRecord("funcommissiondata", record))
            //                    continue;
            //                G1.update_db_table("funcommissiondata", "record", record, new string[] { "name", name, "ma", who, "option", option, "answer", answer });
            //            }
            //        }
            //        catch ( Exception ex)
            //        {
            //        }
            //    }
            //}

            //btnSave.Hide();
            //btnSave.Refresh();
            //modified = false;
            //this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void BtnRunAll_Click(object sender, EventArgs e)
        {
            LoadData();

            DataTable dt = (DataTable)dgv.DataSource;

            if (cmbActiveStatus.Text.Trim().ToUpper() != "GONE")
            {
                DataRow[] dRows = dt.Select("activeStatus<>'Gone'");
                if (dRows.Length > 0)
                    dt = dRows.CopyToDataTable();
            }

            this.Cursor = Cursors.WaitCursor;
            LoadCommissions(dt);


            dt = CleanupAgents(dt);

            this.Cursor = Cursors.Default;

            DataTable ddd = dt.Clone();

            DataRow[] rows = new DataRow[gridMain.DataRowCount];
            for (int i = 0; i < gridMain.DataRowCount; i++)
            {
                rows[i] = gridMain.GetDataRow(i);
                ddd.ImportRow(gridMain.GetDataRow(i));
            }

            dt = ddd.Copy();

            string agent = "";
            string agentCodes = "";
            DateTime begin = this.dateTimePicker1.Value;
            DateTime end = this.dateTimePicker2.Value;
            string commissionType = cmbType.Text.Trim();

            this.Cursor = Cursors.WaitCursor;

            AgentYearlyCommissions yearlyForm = new AgentYearlyCommissions(dt, commissionType, agent, agentCodes, begin, end);
            yearlyForm.Show();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void btnRight_Click(object sender, EventArgs e)
        {
            DateTime now = this.dateTimePicker2.Value;
            now = now.AddMonths(1);
            now = new DateTime(now.Year, now.Month, 1);
            this.dateTimePicker1.Value = now;
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            this.dateTimePicker2.Value = new DateTime(now.Year, now.Month, days);
            //RunTheData();
        }
        /****************************************************************************************/
        private void btnLeft_Click(object sender, EventArgs e)
        {
            DateTime now = this.dateTimePicker1.Value;
            now = now.AddMonths(-1);
            now = new DateTime(now.Year, now.Month, 1);
            this.dateTimePicker1.Value = now;
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            this.dateTimePicker2.Value = new DateTime(now.Year, now.Month, days);

            //RunTheData();
        }
        /****************************************************************************************/
        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            DateTime date = this.dateTimePicker1.Value;
            date = new DateTime(date.Year, date.Month, 1);
            this.dateTimePicker1.Value = date;

            //RunTheData();
        }
        /****************************************************************************************/
        private void dateTimePicker2_ValueChanged(object sender, EventArgs e)
        {
            DateTime date = this.dateTimePicker2.Value;
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            date = new DateTime(date.Year, date.Month, days);
            this.dateTimePicker2.Value = date;

           // RunTheData();
        }
        /****************************************************************************************/
        private void button1_Click(object sender, EventArgs e)
        {
            LoadData();
            RunTheData();
            DataTable dt = (DataTable)dgv.DataSource;
            dt = CleanupAgents(dt);
        }
        /****************************************************************************************/
        private void cmbType_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadData();
            RunTheData();
            DataTable dt = (DataTable)dgv.DataSource;
            dt = CleanupAgents(dt);
        }
        /****************************************************************************************/
        private bool LoadAgentCustom(string agent, DateTime date, string commissionType, ref double editedCommission )
        {
            bool rtn = false;
            editedCommission = 0D;
            try
            {
                string cmd = "Select * from `historic_commission_custom` where `agentName` = '" + agent + "' and `date` = '" + date.ToString("yyyy-MM-dd") + "' AND `commissionType` = '" + commissionType + "' AND `row` = '2';";
                DataTable dx = G1.get_db_data(cmd);
                if (dx.Rows.Count <= 0)
                    return rtn;
                string data = dx.Rows[0]["data"].ObjToString();
                if (G1.validate_numeric(data))
                {
                    editedCommission = data.ObjToDouble();
                    rtn = true;
                }
            }
            catch (Exception ex)
            {
            }
            return rtn;
        }
        /****************************************************************************************/
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            RunTheData();
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

            printableComponentLink1.EnablePageDialog = false;

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

            G1.PrintPreview(printableComponentLink1, gridMain);

            //printableComponentLink1.CreateDocument();
            //printableComponentLink1.ShowPreview();

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
            string title = "Agent Historic Commission Report";
            Printer.DrawQuad(6, 8, 4, 4, title, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);


            DateTime date = this.dateTimePicker1.Value;
            string workDate1 = date.ToString("MM/dd/yyyy");

            date = this.dateTimePicker2.Value;
            string workDate2 = date.ToString("MM/dd/yyyy");

            Printer.SetQuadSize(24, 12);
            font = new Font("Ariel", 9, FontStyle.Regular);
            Printer.DrawQuad(19, 8, 6, 4, "Date Range - " + workDate1 + " - " + workDate2, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            //Printer.DrawQuad(16, 8, 3, 4, lblPayment.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(19, 8, 3, 4, lblTrust85.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(22, 8, 3, 4, lblTrust100.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /****************************************************************************************/
        private bool pageBreak = false;
        private void gridMain_BeforePrintRow(object sender, DevExpress.XtraGrid.Views.Printing.CancelPrintRowEventArgs e)
        {
            int rowHandle = e.RowHandle;
            if (gridMain.IsDataRow(rowHandle))
            {
                try
                {
                    DataTable dt = (DataTable)dgv.DataSource;
                    int row = gridMain.GetDataSourceRowIndex(rowHandle);

                    string newPage = dt.Rows[row]["contracts"].ObjToString();
                    if (newPage.ToUpper() == "BREAK")
                    {
                        pageBreak = true;
                        e.Cancel = true;
                    }
                }
                catch (Exception ex)
                {
                }
            }
            if (e.HasFooter)
            {
                //if (chkPageBreaks.Checked)
                //    pageBreak = true;
            }
        }
        /****************************************************************************************/
        private void gridMain_AfterPrintRow(object sender, DevExpress.XtraGrid.Views.Printing.PrintRowEventArgs e)
        {
            if (pageBreak)
                e.PS.InsertPageBreak(e.Y);
            pageBreak = false;
        }
        /****************************************************************************************/
    }
}