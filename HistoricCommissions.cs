using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraPrinting;
using DevExpress.Utils;
using MySql.Data.MySqlClient;
using MySql.Data.Types;
using GeneralLib;
using DevExpress.XtraGrid.Views.Grid;
using System.Linq;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class HistoricCommissions : DevExpress.XtraEditors.XtraForm
    {
        private DataTable searchAgents = null;
        private DataTable auditDt = null;
        private DataTable workAgents = null;
        private DataTable originalDt = null;
        /****************************************************************************************/
        public HistoricCommissions()
        {
            InitializeComponent();
            DateTime now = DateTime.Now;
            now = now.AddMonths(-1);
            now = new DateTime(now.Year, now.Month, 1);
            this.dateTimePicker1.Value = now;
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            this.dateTimePicker2.Value = new DateTime(now.Year, now.Month, days);
        }
        /****************************************************************************************/
        private void HistoricCommissions_Load(object sender, EventArgs e)
        {
            LoadAgents();
            LoadData();
        }
        /***********************************************************************************************/
        private void SetupSelection(DataTable dt)
        {
            DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew = this.repositoryItemCheckEdit1;
            selectnew.NullText = "";
            selectnew.ValueChecked = "1";
            selectnew.ValueUnchecked = "0";
            selectnew.ValueGrayed = "";
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["select"] = "0";
        }
        /****************************************************************************************/
        private void LoadAgents ()
        {
            string columnName = "agentNumber";
            columnName = "agentCode";
            string cmd = "Select * from `agents` order by `agentCode`;";
            DataTable dt = G1.get_db_data(cmd);
            SetupGoalInfo(dt);
            workAgents = dt.Copy();
            chkComboAgent.Properties.DataSource = dt;

            searchAgents = dt.Copy();
            cmd = "Select * from `agents` GROUP by `lastName`,`firstName` order by `lastName`;";
            DataTable nameList = G1.get_db_data(cmd);
            nameList.Columns.Add("agentNames");
            string fname = "";
            string lname = "";
            for (int i = 0; i < nameList.Rows.Count; i++)
            {
                fname = nameList.Rows[i]["firstName"].ObjToString();
                lname = nameList.Rows[i]["lastName"].ObjToString();
                nameList.Rows[i]["agentNames"] = fname + " " + lname;
            }
            chkComboAgentNames.Properties.DataSource = nameList;
        }
        /****************************************************************************************/
        private void SetupGoalInfo(DataTable dt )
        {
            string agentCode = "";
            DataTable goalDt = G1.get_db_data("Select * from `goals` where `status` = 'CURRENT' ORDER by `effectiveDate`;");
            DataTable gDt = goalDt.Clone();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                gDt.Rows.Clear();
                agentCode = dt.Rows[i]["agentCode"].ObjToString();
                DataRow[] dRows = goalDt.Select("agentCode='" + agentCode + "'");
                for (int j = 0; j < dRows.Length; j++)
                {
                    gDt.ImportRow(dRows[j]);
                }
                ReloadAgentCommission(gDt, dt, i);
            }
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
                else if (type.ToUpper() == "GOAL")
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
            if (row < 0)
                row = GetAgentRow(agentCode, mainDt);
            if (row >= 0)
            {
                mainDt.Rows[row]["splits"] = standardSplit;
                mainDt.Rows[row]["commission"] = standardCommission;
                mainDt.Rows[row]["goal"] = goal;
                mainDt.Rows[row]["additionalGoals"] = goalSplit;
                mainDt.Rows[row]["goalPercent"] = goalCommission;
                mainDt.Rows[row]["customGoals"] = goalFormula;
            }
        }
        /****************************************************************************************/
        private int GetAgentRow(string agent, DataTable dt)
        {
            int row = -1;
            string str = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                str = dt.Rows[i]["agentCode"].ObjToString();
                if (str == agent)
                {
                    row = i;
                    break;
                }
            }
            return row;
        }
        /****************************************************************************************/
        private void LoadData ()
        {
            string cmd = "Select * from `lapse_reinstates` Order by `endDate` DESC;";
            cmd += ";";
            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("select");
            dt.Columns.Add("fromDate");
            dt.Columns.Add("toDate");
            DateTime startDate = DateTime.Now;
            DateTime endDate = DateTime.Now;
            string date = "";
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                startDate = dt.Rows[i]["startDate"].ObjToDateTime();
                date = startDate.ToString("MM/dd/yyyy");
                dt.Rows[i]["fromDate"] = date;
                endDate = dt.Rows[i]["endDate"].ObjToDateTime();
                date = endDate.ToString("MM/dd/yyyy");
                dt.Rows[i]["toDate"] = date;
            }
            SetupSelection(dt);
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
        }
        /****************************************************************************************/
        private void btnRun_Click(object sender, EventArgs e)
        {
            RunData();
        }
        /****************************************************************************************/
        private void RunData()
        {
            this.Cursor = Cursors.WaitCursor;
            DateTime date = dateTimePicker1.Value;
            string date1 = G1.DateTimeToSQLDateTime(date);
            date = dateTimePicker2.Value;
            string date2 = G1.DateTimeToSQLDateTime(date);

            DataTable ldx = null;
            string cmd = "";

            if (chkUseDates.Checked)
            {

                cmd = "Select * from `lapse_reinstates` where `startDate` >= '" + date1 + "' and `endDate` <= '" + date2 + "' Order by `startDate`;";
                cmd += ";";
                ldx = G1.get_db_data(cmd);
            }
            else
                ldx = (DataTable)dgv.DataSource;

            string runNumber = "";

            DataTable dt = null;
            DataTable dx = null;
            DataTable dt_10 = null;
            DataTable dt_9 = null;
            DataTable dt_8 = null;
            string select = "";
            int lastRow = 0;
            for ( int i=0; i<ldx.Rows.Count; i++)
            {
                if ( !chkUseDates.Checked )
                {
                    select = ldx.Rows[i]["select"].ObjToString();
                    if (select != "1")
                        continue;
                }
                runNumber = ldx.Rows[i]["record"].ObjToString();
                date = ldx.Rows[i]["startDate"].ObjToDateTime();
                date1 = date.Year.ToString("D4") + date.Month.ToString("D2") + date.Day.ToString("D2");
                date = ldx.Rows[i]["endDate"].ObjToDateTime();
                date2 = date.Year.ToString("D4") + date.Month.ToString("D2") + date.Day.ToString("D2");

                cmd = "Select * from `historic_commissions` where `runNumber` = '" + runNumber + "';";
                dx = G1.get_db_data(cmd);
                if ( dx.Rows.Count > 0 )
                {
                    if (dt_10 == null)
                    {
                        dt_10 = dx.Clone();
                        dt_10.Columns.Add("startDate");
                        dt_10.Columns.Add("endDate");
                        dt_10.Columns.Add("stopDate");
                    }
                    for (int j = 0; j < dx.Rows.Count; j++)
                    {
                        dt_10.ImportRow(dx.Rows[j]);
                        lastRow = dt_10.Rows.Count - 1;
                        dt_10.Rows[lastRow]["startDate"] = date1;
                        dt_10.Rows[lastRow]["endDate"] = date2;
                        dt_10.Rows[lastRow]["stopDate"] = date2.ObjToDateTime().ToString("MM/dd/yyyy");
                    }

                    cmd = "Select * from `lapsetable` where `runNumber` = '" + runNumber + "';";
                    dx = G1.get_db_data(cmd);
                    if ( dx.Rows.Count > 0)
                    {
                        if (dt_8 == null)
                            dt_8 = dx.Clone();
                        for (int j = 0; j < dx.Rows.Count; j++)
                            dt_8.ImportRow(dx.Rows[j]);
                    }
                    cmd = "Select * from `reinstatetable` where `runNumber` = '" + runNumber + "';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                    {
                        if (dt_9 == null)
                            dt_9 = dx.Clone();
                        for (int j = 0; j < dx.Rows.Count; j++)
                            dt_9.ImportRow(dx.Rows[j]);
                    }
                    cmd = "Select * from `trustdetail` where `runNumber` = '" + runNumber + "';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                    {
                        if (dt == null)
                            dt = dx.Clone();
                        for (int j = 0; j < dx.Rows.Count; j++)
                            dt.ImportRow(dx.Rows[j]);
                    }
                }
            }
            RunCommissions(dt, dt_10, dt_8, dt_9);
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void RunCommissions( DataTable dt, DataTable dt10, DataTable dt8, DataTable dt9 )
        {
            string cmd = "Select * from `agents` order by `agentCode`;";
            DataTable _agentList = G1.get_db_data(cmd);
            this.Cursor = Cursors.WaitCursor;
            DateTime date1 = this.dateTimePicker1.Value;
            DateTime date2 = this.dateTimePicker2.Value;
            DateTime date = DateTime.Now;
            if ( !chkUseDates.Checked )
            {
                bool first = true;
                if (dt != null)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        date = dt.Rows[i]["payDate8"].ObjToDateTime();
                        if (date.Year > 100)
                        {
                            if (first)
                            {
                                date1 = date;
                                date2 = date;
                                first = false;
                                continue;
                            }
                            if (date < date1)
                                date1 = date;
                            if (date > date2)
                                date2 = date;
                        }
                    }
                }
            }
            Commission commForm = new Commission(date1, date2, dt, dt10, dt8, dt9, _agentList);
            commForm.Show();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void LoadDatax()
        {
            this.Cursor = Cursors.WaitCursor;
            DateTime date = dateTimePicker1.Value;
            string date1 = G1.DateTimeToSQLDateTime(date);
            date = dateTimePicker2.Value;
            string date2 = G1.DateTimeToSQLDateTime(date);

            string cmd = "Select * from `commissions` where `workDate1` >= '" + date1 + "' and `workDate2` <= '" + date2 + "' ";
            cmd += ";";
            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("num");
            dt.Columns.Add("customer");
            string name = "";
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                name = dt.Rows[i]["name"].ObjToString();
                dt.Rows[i]["customer"] = name;
            }
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            originalDt = dt;
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void ConvertToTable( DataRow [] dRows, DataTable dt )
        {
            dt.Rows.Clear();
            for (int i = 0; i < dRows.Length; i++)
                dt.ImportRow(dRows[i]);
        }
        /***********************************************************************************************/
        private int LocateAgent(DataTable dt, string agent, string columnName = "")
        {
            int row = -1;
            string str = "";
            if (String.IsNullOrWhiteSpace(columnName))
                columnName = "agentNumber";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                str = dt.Rows[i][columnName].ObjToString();
                if (str == agent)
                {
                    row = i;
                    break;
                }
            }
            return row;
        }
        /***********************************************************************************************/
        private int pageMarginLeft = 0;
        private int pageMarginRight = 0;
        private int pageMarginTop = 0;
        private int pageMarginBottom = 0;
        /***********************************************************************************************/
        private void printPreviewToolStripMenuItem_Click(object sender, EventArgs e)
        {
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

            Printer.setupPrinterMargins(50, 50, 80, 50);

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
        }
        /***********************************************************************************************/
        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
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

            Printer.setupPrinterMargins(50, 50, 80, 50);

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

            font = new Font("Ariel", 10, FontStyle.Bold);
            Printer.DrawQuad(6, 8, 2, 4, "Commission Report", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);


            //            Printer.DrawQuadTicks();
            //            DateTime date = this.dateTimePicker1.Value;
            string workDate = "";
            Printer.SetQuadSize(24, 12);
            font = new Font("Ariel", 9, FontStyle.Bold);
            Printer.DrawQuad(20, 8, 5, 4, "Report Year:" + workDate, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            //Printer.DrawQuad(16, 8, 3, 4, lblPayment.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(19, 8, 3, 4, lblTrust85.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(22, 8, 3, 4, lblTrust100.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /****************************************************************************************/
        private void gridMain_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
        {
            if (e.Column.FieldName.ToUpper() == "DUEDATE8")
            {
                if (e.RowHandle >= 0)
                {
                    DateTime date = e.DisplayText.ObjToDateTime();
                    e.DisplayText = date.Month.ToString("D2") + "/" + date.Day.ToString("D2") + "/" + date.Year.ToString("D4");
                    if (date.Year == 1)
                        e.DisplayText = "";
                }
            }
            else if (e.Column.FieldName.ToUpper() == "PAYDATE8")
            {
                if (e.RowHandle >= 0)
                {
                    DateTime date = e.DisplayText.ObjToDateTime();
                    e.DisplayText = date.Month.ToString("D2") + "/" + date.Day.ToString("D2") + "/" + date.Year.ToString("D4");
                    if (date.Year == 1)
                        e.DisplayText = "";
                }
            }
        }
        /*******************************************************************************************/
        private string getAgentQuery()
        {
            string procLoc = "";
            string[] locIDs = this.chkComboAgent.EditValue.ToString().Split('|');
            for (int i = 0; i < locIDs.Length; i++)
            {
                if (!String.IsNullOrWhiteSpace(locIDs[i]))
                {
                    if (procLoc.Trim().Length > 0)
                        procLoc += ",";
                    procLoc += "'" + locIDs[i].Trim() + "'";
                }
            }
            return procLoc.Length > 0 ? " `agentNumber` IN (" + procLoc + ") " : "";
        }
        /****************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (gridMain.OptionsFind.AlwaysVisible == true)
                gridMain.OptionsFind.AlwaysVisible = false;
            else
                gridMain.OptionsFind.AlwaysVisible = true;
        }
        /****************************************************************************************/
        private void chkComboAgent_EditValueChanged(object sender, EventArgs e)
        {
            if (originalDt == null)
                return;
            string names = getAgentQuery();
            DataRow[] dRows = originalDt.Select(names);
            DataTable dt = originalDt.Clone();
            for (int i = 0; i < dRows.Length; i++)
                dt.ImportRow(dRows[i]);
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
        }
        /****************************************************************************************/
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        /****************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataTable maindt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();
            string runNumber = dr["record"].ObjToString();
            this.Cursor = Cursors.WaitCursor;
            //DateTime date = dateTimePicker1.Value;
            //string date1 = G1.DateTimeToSQLDateTime(date);
            //date = dateTimePicker2.Value;
            //string date2 = G1.DateTimeToSQLDateTime(date);

            //string cmd = "Select * from `lapse_reinstates` where `startDate` >= '" + date1 + "' and `endDate` <= '" + date2 + "' Order by `startDate`;";
            //cmd += ";";
            //DataTable ldx = G1.get_db_data(cmd);

            //string runNumber = "";

            DataTable dt10 = null;
            DataTable dt9 = null;
            DataTable dt8 = null;

            DataTable dt = null;
            DataTable dx = null;
            DataTable dt_10 = null;
            DataTable dt_9 = null;
            DataTable dt_8 = null;
            string cmd = "";
            cmd = "Select * from `historic_commissions` where `runNumber` = '" + runNumber + "';";
            dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                if (dt_10 == null)
                    dt_10 = dx.Clone();
                for (int j = 0; j < dx.Rows.Count; j++)
                    dt_10.ImportRow(dx.Rows[j]);

                cmd = "Select * from `lapsetable` where `runNumber` = '" + runNumber + "';";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    if (dt_8 == null)
                        dt_8 = dx.Clone();
                    for (int j = 0; j < dx.Rows.Count; j++)
                        dt_8.ImportRow(dx.Rows[j]);
                }
                cmd = "Select * from `reinstatetable` where `runNumber` = '" + runNumber + "';";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    if (dt_9 == null)
                        dt_9 = dx.Clone();
                    for (int j = 0; j < dx.Rows.Count; j++)
                        dt_9.ImportRow(dx.Rows[j]);
                }
                cmd = "Select * from `trustdetail` where `runNumber` = '" + runNumber + "';";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    if (dt == null)
                        dt = dx.Clone();
                    for (int j = 0; j < dx.Rows.Count; j++)
                        dt.ImportRow(dx.Rows[j]);
                }
            }
            RunCommissions(dt, dt_10, dt_8, dt_9);
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void btnLeft_Click(object sender, EventArgs e)
        {
            DateTime now = this.dateTimePicker1.Value;
            DateTime date = new DateTime(now.Year, now.Month, 1);
            date = date.AddMonths(-1);
            this.dateTimePicker1.Value = date;
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            date = new DateTime(date.Year, date.Month, days);
            this.dateTimePicker2.Value = date;
        }
        /****************************************************************************************/
        private void btnRight_Click(object sender, EventArgs e)
        {
            DateTime now = this.dateTimePicker2.Value;
            now = now.AddMonths(1);
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            DateTime date = new DateTime(now.Year, now.Month, days);
            this.dateTimePicker2.Value = date;
        }
        /****************************************************************************************/
        private void chkComboAgentNames_EditValueChanged(object sender, EventArgs e)
        {
            if (originalDt == null)
                return;
            string names = getAgentNameQuery();
            DataRow[] dRows = originalDt.Select(names);
            DataTable dt = originalDt.Clone();
            for (int i = 0; i < dRows.Length; i++)
                dt.ImportRow(dRows[i]);
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
        }
        /*******************************************************************************************/
        private string getAgentNameQuery()
        {
            string procLoc = "";
            string[] locIDs = this.chkComboAgentNames.EditValue.ToString().Split('|');
            for (int i = 0; i < locIDs.Length; i++)
            {
                if (!String.IsNullOrWhiteSpace(locIDs[i]))
                {
                    if (procLoc.Trim().Length > 0)
                        procLoc += ",";
                    procLoc += "'" + locIDs[i].Trim() + "'";
                }
            }
            return procLoc.Length > 0 ? " `customer` IN (" + procLoc + ") " : "";
        }
        /****************************************************************************************/
        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            DateTime now = this.dateTimePicker1.Value;
            DateTime date = new DateTime(now.Year, now.Month, 1);
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            date = new DateTime(date.Year, date.Month, days);
            this.dateTimePicker2.Value = date;
        }
        /****************************************************************************************/
    }
}