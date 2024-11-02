using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraPrinting;
using DevExpress.Utils;
using MySql.Data.MySqlClient;
using GeneralLib;
using DevExpress.XtraGrid.Views.Grid;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class Commission : DevExpress.XtraEditors.XtraForm
    {
        private DataTable workTable = null;
        private DataTable initDt8 = null;
        private DataTable initDt9 = null;
        private DataTable workDt8 = null;
        private DataTable workDt9 = null;
        private DataTable workDt10 = null;
        private DataTable workAgents = null;
        private DataTable mDt = null;
        private DataTable searchAgents = null;
        private DataTable auditDt = null;
        private string workDate = "";
        private DateTime workDate1;
        private DateTime workDate2;
        private DateTime wDate;
        private bool first = true;
        private bool doBatch = false;
        private bool doSplits = false;
        private bool allowDebits = false;
        public static DataTable commissionDt = null;
        public static DataTable splitDt = null;
        /****************************************************************************************/
        public Commission( bool batch, bool splits, DateTime date, DateTime Date2, DataTable dt, DataTable dt8, DataTable dt9, DataTable agents)
        {
            InitializeComponent();
            doBatch = batch;
            doSplits = splits;
            workTable = dt;
            workDt8 = dt8;
            initDt8 = dt8;
            workDt9 = dt9;
            initDt9 = dt9;
            workAgents = agents;
            workDate = date.Month.ToString("D2") + "/" + date.Year.ToString("D4");
            wDate = date;
            workDate1 = date;
            workDate2 = Date2;
            SetupTotalsSummary();
            if ( doBatch )
            {
                chkDoSplits.Checked = doSplits;
                workDt8 = ConsolidateLapse(workDt8);
                workDt9 = ConsolidateLapse(workDt9);
//                G1.SetupVisibleColumns(gridMain, this.columnsToolStripMenuItem, nmenu_Click);
                LoadAgents();
                LoadData();
                btnRun_Click(null, null);
                commissionDt = (DataTable)dgv.DataSource;
                this.Close();
            }
        }
        /****************************************************************************************/
        private bool historic = false;
        public Commission(DateTime date, DateTime Date2, DataTable dt, DataTable dt10, DataTable dt8, DataTable dt9, DataTable agents)
        {
            InitializeComponent();
            btnLock.Hide();
            btnRun.Hide();
            btnCreateTabs.Hide();
            chkLoadAll.Hide();
            chkDoSplits.Hide();
            historic = true;
            doBatch = false;
            doSplits = false;
            workTable = dt;
            workDt8 = dt8;
            initDt8 = dt8;
            workDt9 = dt9;
            initDt9 = dt9;
            workDt10 = dt10;
            workAgents = agents;
            workDate = date.Month.ToString("D2") + "/" + date.Year.ToString("D4");
            wDate = date;
            workDate1 = date;
            workDate2 = Date2;
            SetupTotalsSummary();
            dgv.DataSource = dt10;
            gridMain.Columns["splitCommission"].Visible = true;
            gridMain.Columns["splitBaseCommission"].Visible = true;
            gridMain.Columns["splitGoalCommission"].Visible = true;
            dgv.Refresh();
            this.Refresh();
            //if (doBatch)
            //{
            //    chkDoSplits.Checked = doSplits;
            //    workDt8 = ConsolidateLapse(workDt8);
            //    workDt9 = ConsolidateLapse(workDt9);
            //    //                G1.SetupVisibleColumns(gridMain, this.columnsToolStripMenuItem, nmenu_Click);
            //    LoadAgents();
            //    LoadData();
            //    btnRun_Click(null, null);
            //    commissionDt = (DataTable)dgv.DataSource;
            //    this.Close();
            //}
        }
        /****************************************************************************************/
        private void Commission_Load(object sender, EventArgs e)
        {
            if (!historic)
            {
                workDt8 = ConsolidateLapse(workDt8);
                workDt9 = ConsolidateLapse(workDt9);
                G1.SetupVisibleColumns(gridMain, this.columnsToolStripMenuItem, nmenu_Click);
                LoadAgents();
                LoadData();
            }
            else
                LoadAgents();
        }
        /****************************************************************************************/
        private void SetupTotalsSummary()
        {
            AddSummaryColumn("totalCommission");
            AddSummaryColumn("totalPayments");
            AddSummaryColumn("commission");
            AddSummaryColumn("dbcMoney");
            AddSummaryColumn("splitCommission");
            AddSummaryColumn("splitBaseCommission");
            AddSummaryColumn("splitGoalCommission");
            AddSummaryColumn("goalCommission");
            AddSummaryColumn("mainCommission");
            AddSummaryColumn("contractValue");
            AddSummaryColumn("Formula Sales");
            AddSummaryColumn("Location Sales");
            AddSummaryColumn("dbrValue");
            AddSummaryColumn("Recap");
            AddSummaryColumn("Reins");
            AddSummaryColumn("pastRecap");
            AddSummaryColumn("pastFailures");
            AddSummaryColumn("totalContracts");
            AddSummaryColumn("contractCommission");
            AddSummaryColumn("fbi", null, "{0}");
            AddSummaryColumn("fbi$");
            AddSummaryColumn("MR");
            AddSummaryColumn("MC");
        }
        /****************************************************************************************/
        private void AddSummaryColumn ( string columnName)
        {
            gridMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            gridMain.Columns[columnName].SummaryItem.DisplayFormat = "${0:0,0.00}";
        }
        /****************************************************************************************/
        private void AddSummaryColumn(string columnName, DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gMain = null, string format = "")
        {
            if (gMain == null)
                gMain = gridMain;
            if (String.IsNullOrWhiteSpace(format))
                format = "${0:0,0.00}";
            gMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            gMain.Columns[columnName].SummaryItem.DisplayFormat = format;
        }
        /***********************************************************************************************/
        void nmenu_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menu = (ToolStripMenuItem)sender;
            string name = menu.Name;
            int index = getGridColumnIndex(name);
            if (index < 0)
                return;
            if (menu.Checked)
            {
                menu.Checked = false;
                gridMain.Columns[index].Visible = false;
            }
            else
            {
                menu.Checked = true;
                gridMain.Columns[index].Visible = true;
            }
            gridMain.RefreshData();
            dgv.Refresh();
            ToolStripMenuItem xmenu = this.columnsToolStripMenuItem;
            xmenu.ShowDropDown();
        }
        /***********************************************************************************************/
        private int getGridColumnIndex(string columnName)
        {
            int index = -1;
            for (int i = 0; i < gridMain.Columns.Count; i++)
            {
                string name = gridMain.Columns[i].Name;
                if (name == columnName)
                {
                    index = i;
                    break;
                }
            }
            return index;
        }
        /****************************************************************************************/
        private void LoadAgents ()
        {
            string columnName = "agentNumber";
            columnName = "agentCode";
            first = false;
            string list = "";
            for ( int i=0; i<workAgents.Rows.Count; i++)
                list += "'" + workAgents.Rows[i][columnName].ObjToString() + "',";
            list = list.TrimEnd(',');
            string cmd = "Select * from `agents` order by `agentCode`;";
//            string cmd = "Select * from `agents` where `agentCode` in (" + list + ") order by `agentCode`;";

            DataTable dt = G1.get_db_data(cmd);

            SetupGoalInfo(dt);
            chkComboAgent.Properties.DataSource = dt;
            searchAgents = dt.Copy();
            VerifyAgents(searchAgents);

            LoadAgentNames( dt );
        }
        /****************************************************************************************/
        private void LoadAgentNames( DataTable dt )
        {
            //string columnName = "agentNumber";
            //columnName = "agentCode";
            //string cmd = "Select * from `agents` order by `agentCode`;";
            //DataTable dt = G1.get_db_data(cmd);
            //SetupGoalInfo(dt);
            //workAgents = dt.Copy();
            //chkComboAgent.Properties.DataSource = dt;

            searchAgents = dt.Copy();
            string cmd = "Select * from `agents` GROUP by `lastName`,`firstName` order by `lastName`;";
            DataTable nameList = G1.get_db_data(cmd);
            nameList.Columns.Add("agentNames");
            string fname = "";
            string lname = "";
            for (int i = 0; i < nameList.Rows.Count; i++)
            {
                fname = nameList.Rows[i]["firstName"].ObjToString().Trim();
                lname = nameList.Rows[i]["lastName"].ObjToString().Trim();
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
        private void LoadData()
        {
        }
        ///****************************************************************************************/
        //private void btnRun_Clickx(object sender, EventArgs e)
        //{
        //    this.Cursor = Cursors.WaitCursor;
        //    DataTable dt = workTable.Clone();
        //    DataTable gDx = G1.get_db_data("Select * from `goals` where `status` = 'Current' order by `agentCode`,`effectiveDate`;");
        //    dt.Columns.Add("splits");
        //    dt.Columns.Add("additionalGoals");
        //    dt.Columns.Add("goal", Type.GetType("System.Double"));
        //    dt.Columns.Add("goalPercent", Type.GetType("System.Double"));
        //    dt.Columns.Add("mainCommission", Type.GetType("System.Double"));
        //    dt.Columns.Add("splitCommission", Type.GetType("System.Double"));
        //    dt.Columns.Add("goalCommission", Type.GetType("System.Double"));
        //    dt.Columns.Add("totalCommission", Type.GetType("System.Double"));
        //    dt.Columns.Add("dbrValue", Type.GetType("System.Double"));
        //    dt.Columns.Add("customGoals");
        //    dt.Columns.Add("totalContracts", Type.GetType("System.Double"));
        //    dt.Columns.Add("pastFailure", Type.GetType("System.Double"));

        //    gDx.Columns.Add("totalContracts", Type.GetType("System.Double"));
        //    gDx.Columns.Add("totalCommission", Type.GetType("System.Double"));

        //    gDx.Columns.Add("commission", Type.GetType("System.Double"));
        //    gDx.Columns.Add("Formula Sales", Type.GetType("System.Double"));
        //    gDx.Columns.Add("Location Sales", Type.GetType("System.Double"));
        //    gDx.Columns.Add("Total Sales", Type.GetType("System.Double"));
        //    gDx.Columns.Add("TCommission", Type.GetType("System.Double"));
        //    gDx.Columns.Add("dbrValue", Type.GetType("System.Double"));
        //    gDx.Columns.Add("Recap", Type.GetType("System.Double"));
        //    gDx.Columns.Add("Reins", Type.GetType("System.Double"));
        //    gDx.Columns.Add("ResultCommission", Type.GetType("System.Double"));
        //    gDx.Columns.Add("totalPayments", Type.GetType("System.Double"));
        //    gDx.Columns.Add("pastFailures", Type.GetType("System.Double"));
        //    gDx.Columns.Add("customer");
        //    gDx.Columns.Add("agentNumber");
        //    gDx.Columns.Add("fbi", Type.GetType("System.Double"));

        //    // VerifyAgents(gDx);

        //    double payment = 0D;
        //    double agentPayments = 0D;
        //    double totalPayments = 0D;
        //    double totalCommission = 0D;
        //    double agentCommission = 0D;
        //    double agentRecap = 0D;
        //    double agentReins = 0D;
        //    double recap = 0D;
        //    double reins = 0D;
        //    double pastRecaps = 0D;
        //    double pastFailure = 0D;
        //    double commission = 0D;
        //    double contractValue = 0D;
        //    double agentContracts = 0D;
        //    double goal = 0D;
        //    double goalPercent = 0D;
        //    double dbrValue = 0D;
        //    double dbrTotal = 0D;
        //    double totalContracts = 0D;
        //    double totalRecap = 0D;
        //    double totalReins = 0D;
        //    string dbr = "";
        //    string fname = "";
        //    string lname = "";
        //    string name = "";
        //    string additional = "";
        //    string splits = "";
        //    string agent = "";
        //    string type = "";
        //    string agentCode = "";
        //    string formula = "";
        //    string status = "";
        //    for ( int i=0; i<gDx.Rows.Count; i++)
        //    {
        //        type = gDx.Rows[i]["type"].ObjToString();
        //        status = gDx.Rows[i]["status"].ObjToString();
        //        formula = gDx.Rows[i]["formula"].ObjToString();
        //        agentCode = gDx.Rows[i]["agentCode"].ObjToString();
        //        if ( agentCode == "N40")
        //        {

        //        }
        //        DataRow[] dRows = workTable.Select("agentNumber='" + agentCode + "'");
        //        DataTable ddx = workTable.Clone();
        //        for (int j = 0; j < dRows.Length; j++)
        //            ddx.ImportRow(dRows[j]);
        //        totalPayments = 0D;
        //        totalContracts = 0D;
        //        totalCommission = 0D;
        //        totalRecap = 0D;
        //        totalReins = 0D;
        //        for ( int j=0; j<dRows.Length; j++)
        //        {
        //            totalPayments += dRows[j]["totalPayments"].ObjToDouble();
        //            totalContracts += dRows[j]["contractValue"].ObjToDouble();
        //            totalCommission += dRows[j]["commission"].ObjToDouble();
        //            totalRecap += dRows[j]["Recap"].ObjToDouble();
        //            totalReins += dRows[j]["Reins"].ObjToDouble();
        //        }
        //        if (type.Trim().ToUpper() == "GOAL")
        //            gDx.Rows[i]["totalContracts"] = totalContracts;
        //        else
        //        {
        //            gDx.Rows[i]["totalPayments"] = totalPayments;
        //            gDx.Rows[i]["commission"] = totalCommission;
        //            gDx.Rows[i]["Recap"] = totalRecap;
        //            gDx.Rows[i]["Reins"] = totalReins;
        //        }
        //        dRows = searchAgents.Select("agentCode='" + agentCode + "'");
        //        if ( dRows.Length > 0 )
        //        {
        //            gDx.Rows[i]["agentNumber"] = agentCode;
        //            fname = dRows[0]["firstName"].ObjToString();
        //            lname = dRows[0]["lastName"].ObjToString();
        //            name = fname.Trim() + " " + lname.Trim();
        //            gDx.Rows[i]["customer"] = name;
        //        }
        //    }
        //    G1.NumberDataTable(gDx);
        //    dgv.DataSource = gDx;
        //    this.Cursor = Cursors.Default;
        //}
        /****************************************************************************************/
        private void VerifyAgents ( DataTable dx )
        {
            if (workTable == null)
                return;
            bool doType = false;
            bool doName = false;
            if (G1.get_column_number(dx, "type") >= 0)
                doType = true;
            if (G1.get_column_number(dx, "firstName") >= 0)
                doName = true;
            DateTime date = new DateTime(2012, 1, 1);
            string agent = "";
            for ( int i=0; i<workTable.Rows.Count; i++)
            {
                agent = workTable.Rows[i]["agentNumber"].ObjToString();
                DataRow[] dRows = dx.Select("agentCode='" + agent + "'");
                if ( dRows.Length <= 0)
                {
                    DataRow dR = dx.NewRow();
                    dR["agentCode"] = agent;
                    if (doType)
                    {
                        dR["type"] = "Standard";
                        dR["status"] = "Current";
                        dR["Percent"] = 0D;
                        dR["effectiveDate"] = G1.DTtoMySQLDT(date);
                    }

                    if ( doName )
                    {
                        dR["firstName"] = "Bad";
                        dR["lastName"] = "Name";
                    }
                    dx.Rows.Add(dR);
                }
            }
        }
        /****************************************************************************************/
        public static bool isMeetingPaid ( string contractNumber, DataTable mDt)
        {
            bool isPaid = false;
            DataRow[] dRows = mDt.Select("contractNumber='" + contractNumber + "'");
            if (dRows.Length > 0)
                isPaid = true;
            return isPaid;
        }
        /****************************************************************************************/
        private void btnRun_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            DataTable dt = workTable.Clone();

            DataRow[] dxxx = workTable.Select("agentNumber='V22'");
            DataTable dxxxt = workTable.Clone();
            G1.ConvertToTable(dxxx, dxxxt);
            int xyxx = dxxxt.Rows.Count;

            DataTable gDx = G1.get_db_data("Select * from `goals` where `status` = 'Current' order by `agentCode`,`effectiveDate`;");
            VerifyAgents(gDx);
            DataTable tempDt = null;
            dt.Columns.Add("splits");
            dt.Columns.Add("additionalGoals");
            dt.Columns.Add("goal", Type.GetType("System.Double"));
            dt.Columns.Add("goalPercent", Type.GetType("System.Double"));
            dt.Columns.Add("mainCommission", Type.GetType("System.Double"));
            dt.Columns.Add("splitCommission", Type.GetType("System.Double"));
            dt.Columns.Add("splitBaseCommission", Type.GetType("System.Double"));
            dt.Columns.Add("splitGoalCommission", Type.GetType("System.Double"));
            dt.Columns.Add("goalCommission", Type.GetType("System.Double"));
            dt.Columns.Add("totalCommission", Type.GetType("System.Double"));
            dt.Columns.Add("dbrValue", Type.GetType("System.Double"));
//            dt.Columns.Add("dbcMoney", Type.GetType("System.Double"));
            dt.Columns.Add("customGoals");
            dt.Columns.Add("totalContracts", Type.GetType("System.Double"));
            dt.Columns.Add("pastFailures", Type.GetType("System.Double"));

            string contractNumber = "";
            double fbi = 0D;
            double agentFbi = 0D;
            double payment = 0D;
            double agentPayments = 0D;
            double totalPayments = 0D;
            double totalCommission = 0D;
            double agentCommission = 0D;
            double agentRecap = 0D;
            double agentReins = 0D;
            double agentDBC = 0D;
            double agentDBCMoney = 0D;
            double recap = 0D;
            double reins = 0D;
            double pastRecaps = 0D;
            double pastFailures = 0D;
            double commission = 0D;
            double contractValue = 0D;
            double agentContracts = 0D;
            double goal = 0D;
            double goalPercent = 0D;
            double dbrValue = 0D;
            double dbrTotal = 0D;
            double dbcMoney = 0D;
            double dbc = 0D;
            string dbr = "";
            string fname = "";
            string lname = "";
            string name = "";
            string additional = "";
            string splits = "";
            string agent = "";
            double debit = 0D;
            double credit = 0D;

            mDt = workTable.Clone();
            DataRow[] dRows = workTable.Select("meetingNumber <> '' AND meetingNumber <> '0'");
            if (dRows.Length > 0)
                mDt = dRows.CopyToDataTable();


            try
            {
                //CalculateMeetingCommissions(dt);

                for (int i = 0; i < searchAgents.Rows.Count; i++)
                {
                    agent = searchAgents.Rows[i]["agentCode"].ObjToString();
                    if ( agent == "N30")
                    {
                    }
                    fname = searchAgents.Rows[i]["firstName"].ObjToString();
                    lname = searchAgents.Rows[i]["lastName"].ObjToString();
                    name = fname.Trim() + " " + lname.Trim();
                    if (lname == "Chaney")
                    {

                    }
                    //if (agent != "V25")
                    //{
                    //    continue;
                    //}
                    //    continue;
                    dRows = workTable.Select("agentNumber='" + agent + "'");
                    DataTable ddd = workTable.Clone();
                    for (int j = 0; j < dRows.Length; j++)
                        ddd.ImportRow(dRows[j]);

                    DataRow[] xRows = ddd.Select("Recap>'0'");
                    int xxi = xRows.Length;
                    //if (dRows.Length <= 0)
                    //    continue;
                    agentCommission = 0D;
                    agentPayments = 0D;
                    agentContracts = 0D;
                    dbrTotal = 0D;
                    agentRecap = 0D;
                    agentReins = 0D;
                    agentFbi = 0D;
                    agentDBC = 0D;
                    agentDBCMoney = 0D;
                    for (int j = 0; j < dRows.Length; j++)
                    {
                        contractNumber = dRows[j]["contractNumber"].ObjToString();
                        if ( contractNumber == "L24808")
                        {
                        }
                        commission = dRows[j]["commission"].ObjToDouble();
                        payment = dRows[j]["totalPayments"].ObjToDouble();
                        debit = dRows[j]["debitAdjustment"].ObjToDouble();
                        credit = dRows[j]["creditAdjustment"].ObjToDouble();
                        if (credit != 0D) // Ramma Zamma
                        {
                            if ( agent.ToUpper() == "XXX" )
                                payment = 0D;
                        }
                        //if ( debit != 0D && !allowDebits ) // Ramma Zamma
                        //    payment = 0D;
                        contractValue = dRows[j]["contractValue"].ObjToDouble();
                        recap = dRows[j]["Recap"].ObjToDouble();
                        reins = dRows[j]["Reins"].ObjToDouble();
                        fbi = dRows[j]["fbi"].ObjToDouble();
                        payment = G1.RoundValue(payment);
                        dbc = dRows[j]["dbc"].ObjToDouble();
                        commission = G1.RoundValue(commission);
                        dbcMoney = dRows[j]["dbcMoney"].ObjToDouble();
                        dbcMoney = G1.RoundValue(dbcMoney);
                        if ( dbcMoney > 0D)
                        {
                        }
                        //                    contractValue = G1.RoundValue(contractValue);
                        dbr = dRows[j]["dbr"].ObjToString();
                        if (dbr.Trim().ToUpper() == "DBR")
                        {
                            //                        if ( !CheckDeathDateCommission ( ddd, j ))
                            //dbrTotal += contractValue;
                        }
                        if (!ShouldCommissionBePaid(ddd, j))
                            agentDBC += contractValue;
                        agentCommission += commission;
                        agentDBCMoney += dbcMoney;
                        agentPayments += payment;
                        //if (!isMeetingPaid(contractNumber, mDt))
                        //    agentContracts += contractValue;
                        agentRecap += recap;
                        agentReins += reins;
                        agentFbi += fbi;
                        totalCommission += commission;
                        totalPayments += payment;
                    }

                    DataRow[] gRows = gDx.Select("agentCode='" + agent + "'");
                    if (gRows.Length <= 0)
                        continue;
                    if ( agent == "N30")
                    {

                    }
                    //agentRecap = 0D;
                    //gRows = workDt8.Select("agentNumber='" + agent + "'");
                    //tempDt = workDt8.Clone();
                    //G1.ConvertToTable(gRows, tempDt);
                    //for (int kk = 0; kk < gRows.Length; kk++)
                    //{
                    //    agentRecap += gRows[kk]["Recap"].ObjToDouble();
                    //}

                    DataRow dr = dt.NewRow();
                    dr["agentNumber"] = agent;
                    fname = searchAgents.Rows[i]["firstName"].ObjToString();
                    lname = searchAgents.Rows[i]["lastName"].ObjToString();
                    name = fname + " " + lname;
                    dr["customer"] = name;
                    //dr["lname"] = lname;
                    dr["commission"] = G1.RoundValue(agentCommission);
                    dr["dbcMoney"] = G1.RoundValue(agentDBCMoney);
                    //                dr["paymentAmount"] = G1.RoundDown(agentPayments);
                    dr["totalPayments"] = G1.RoundValue(agentPayments);
                    dr["contractValue"] = G1.RoundValue(agentContracts);
                    dr["Recap"] = G1.RoundValue(agentRecap);
                    dr["Reins"] = G1.RoundValue(agentReins);
                    dr["dbc"] = G1.RoundValue(agentDBC);
                    dr["fbi"] = agentFbi;
                    splits = searchAgents.Rows[i]["splits"].ObjToString();
                    additional = searchAgents.Rows[i]["additionalGoals"].ObjToString();
                    goal = searchAgents.Rows[i]["goal"].ObjToDouble();
                    goalPercent = searchAgents.Rows[i]["goalPercent"].ObjToDouble();
                    dr["splits"] = splits;
                    dr["additionalGoals"] = additional;
                    dr["goal"] = goal;
                    dr["goalPercent"] = goalPercent;
                    dr["customGoals"] = searchAgents.Rows[i]["customGoals"].ObjToString();
                    dr["dbrValue"] = G1.RoundValue(dbrTotal);
                    dr["pastFailures"] = searchAgents.Rows[i]["recapAmount"].ObjToDouble();
                    searchAgents.Rows[i]["recapAmount"] = 0D; // Zero out so it's not used again.

                    dt.Rows.Add(dr);
                }
            }
            catch ( Exception ex )
            {
                MessageBox.Show("***ERROR*** " + ex.Message.ToString());
            }

            DataTable extraDt = CalcAgentExtraCommission( dt );

//            DataTable mainDt = dt.Copy();

            dt = extraDt.Copy();

//            CalculateSpecialGoals(dt);


//            CalculatePastRecap(dt, workDt8 );

//            CalculatePastRecap(dt, workDt9);

            double locationSales = 0D;
            double formulaSales = 0D;
            double totalContracts = 0D;
            double dbrSales = 0D;
            if ( G1.get_column_number ( dt, "contractValue") < 0 )
                dt.Columns.Add("contractValue", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "totalContracts") < 0)
                dt.Columns.Add("totalContracts", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "pastDBR") < 0)
                dt.Columns.Add("pastDBR", Type.GetType("System.Double"));

            string type = "";
            double pastDBR = 0D;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                pastDBR = 0D;
                agent = dt.Rows[i]["agentCode"].ObjToString();
                if ( agent == "N30")
                {

                }
                type = dt.Rows[i]["type"].ObjToString();
                if ( type.ToUpper() == "GOAL")
                {
                    DataRow[] dR = workDt8.Select("agentNumber='" + agent + "' AND dbrSales > '0'");
                    for ( int k=0; k<dR.Length; k++)
                        pastDBR += dR[k]["dbrSales"].ObjToDouble();
                    //dt.Rows[i]["pastDBR"] = pastDBR;
                }
                locationSales = dt.Rows[i]["location sales"].ObjToDouble();
                locationSales = G1.RoundValue(locationSales);
                dt.Rows[i]["location sales"] = locationSales;
                formulaSales = dt.Rows[i]["formula sales"].ObjToDouble();
                formulaSales = G1.RoundValue(formulaSales);
                dt.Rows[i]["formula sales"] = formulaSales;
                totalContracts = locationSales + formulaSales;
                dt.Rows[i]["contractValue"] = totalContracts;
                dbrSales = dt.Rows[i]["dbrValue"].ObjToDouble();
                dbrSales = G1.RoundValue(dbrSales);
                dt.Rows[i]["dbrValue"] = dbrSales;
                totalContracts = totalContracts - dbrSales;
                dt.Rows[i]["totalContracts"] = totalContracts;
            }

            if (G1.get_column_number(dt, "contractCommission") < 0 )
                dt.Columns.Add("contractCommission", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "goalCommission") < 0)
                dt.Columns.Add("goalCommission", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "totalCommission") < 0)
                dt.Columns.Add("totalCommission", Type.GetType("System.Double"));


            double goalCommission = 0D;
            totalCommission = 0D;
            double baseCommission = 0D;
            double contractCommission = 0D;
            pastRecaps = 0D;
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                agent = dt.Rows[i]["agentCode"].ObjToString();
                if (agent == "V25")
                {

                }
                commission = dt.Rows[i]["commission"].ObjToDouble();
                totalCommission = dt.Rows[i]["ResultCommission"].ObjToDouble();
                totalCommission = dt.Rows[i]["TCommission"].ObjToDouble();
                recap = dt.Rows[i]["Recap"].ObjToDouble();
                reins = dt.Rows[i]["Reins"].ObjToDouble();
//                pastRecaps = dt.Rows[i]["pastRecap"].ObjToDouble();
                pastFailures = dt.Rows[i]["pastFailures"].ObjToDouble();
                if ( pastFailures != 0D)
                {

                }
                goalCommission = totalCommission - commission;

                if ( goalCommission > 0D)
                    dt.Rows[i]["goalCommission"] = totalCommission - commission;
                else
                    dt.Rows[i]["goalCommission"] = 0D;
                contractCommission = totalCommission - commission - recap + reins - pastRecaps;
                if ( contractCommission > 0D)
                    dt.Rows[i]["contractCommission"] = contractCommission;
                else
                    dt.Rows[i]["contractCommission"] = 0D;
                baseCommission = dt.Rows[i]["commission"].ObjToDouble();
                dt.Rows[i]["totalCommission"] = contractCommission + baseCommission;
            }

            if (chkDoSplits.Checked)
            {
                CalculateAllSplits(dt);
                gridMain.Columns["splitCommission"].Visible = true;
                gridMain.Columns["splitBaseCommission"].Visible = true;
                gridMain.Columns["splitGoalCommission"].Visible = true;
            }
            else
            {
                gridMain.Columns["splitCommission"].Visible = false;
                gridMain.Columns["splitBaseCommission"].Visible = false;
                gridMain.Columns["splitGoalCommission"].Visible = false;
            }


            dt.Columns.Add("num");
            dt.Columns.Add("agentNumber");
            dt.Columns.Add("customer");
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                dt.Rows[i]["agentNumber"] = dt.Rows[i]["agentCode"].ObjToString();
                dt.Rows[i]["customer"] = dt.Rows[i]["name"].ObjToString();
            }

            CalculateMeetingCommissions(dt);

            TheFinale(dt);

            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void CalculateMeetingCommissions ( DataTable dt )
        {
            DataRow [] dRows = workTable.Select("meetingNumber <> '' AND meetingNumber <> '0'");

            //DataRow[] dRows = workTable.Select("meetingNumber>'0'");
            if (dRows.Length <= 0)
                return;
            DataTable mDt = dRows.CopyToDataTable();

            string cmd = "";
            DataTable meetingDt = null;
            string meetingNumber = "";
            string contractNumber = "";
            double contractValue = 0D;
            double cashAdvance = 0D;
            string dbc = "";
            DateTime effectiveFromDate = DateTime.Now;
            DateTime effectiveToDate = DateTime.Now;
            string agent = "";
            string agentLastName = "";
            string agentFirstName = "";
            double commissionPercent = 0D;
            double splitCommissionPercent = 0D;

            DateTime issueDate8 = DateTime.Now;

            string name = "";
            DataRow[] dddRow = null;

            string firstName1 = "";
            string lastName1 = "";

            double commission = 0D;
            double finalCommission = 0D;
            double dValue = 0D;

            for ( int i=0; i<mDt.Rows.Count; i++)
            {
                try
                {
                    meetingNumber = mDt.Rows[i]["meetingNumber"].ObjToString();
                    contractNumber = mDt.Rows[i]["contractNumber"].ObjToString();
                    dbc = mDt.Rows[i]["dbc"].ObjToString();
                    contractValue = mDt.Rows[i]["contractValue"].ObjToDouble();
                    issueDate8 = mDt.Rows[i]["issueDate8"].ObjToDateTime();
                    firstName1 = mDt.Rows[i]["firstName1"].ObjToString();
                    lastName1 = mDt.Rows[i]["lastName1"].ObjToString();

                    cmd = "Select * from `agent_meetings` WHERE `meetingNumber` = '" + meetingNumber + "';";
                    meetingDt = G1.get_db_data(cmd);
                    if (meetingDt.Rows.Count <= 0)
                        continue;
                    for (int j = 0; j < meetingDt.Rows.Count; j++)
                    {
                        agent = meetingDt.Rows[j]["agent"].ObjToString();
                        agentLastName = meetingDt.Rows[j]["agentLastName"].ObjToString();
                        agentFirstName = meetingDt.Rows[j]["agentFirstName"].ObjToString();

                        commissionPercent = meetingDt.Rows[j]["commissionPercent"].ObjToDouble();
                        splitCommissionPercent = meetingDt.Rows[j]["splitCommissionPercent"].ObjToDouble();

                        effectiveFromDate = meetingDt.Rows[j]["effectiveFromDate"].ObjToDateTime();
                        effectiveToDate = meetingDt.Rows[j]["effectiveToDate"].ObjToDateTime();

                        if ( issueDate8 < effectiveFromDate || issueDate8 > effectiveToDate )
                            continue;

                        name = agentFirstName + " " + agentLastName;
                        dddRow = dt.Select("name='" + name + "' AND contractValue>'0'");
                        dddRow = dt.Select("name='" + name + "' AND type='Goal'");
                        if ( dddRow.Length <= 0 )
                            dddRow = dt.Select("name='" + name + "'");
                        if ( dddRow.Length > 0 )
                        {
                            DataTable tempDt = dddRow.CopyToDataTable();
                            commission = contractValue * commissionPercent / 100D;
                            if (splitCommissionPercent > 0D)
                                commission = commission * splitCommissionPercent;
                            dValue = dddRow[0]["MC"].ObjToDouble();
                            dValue += commission;
                            dValue = G1.RoundValue(dValue);
                            dddRow[0]["MC"] = dValue;
                        }
                        name = firstName1 + " " + lastName1;
                        dddRow = dt.Select("name='" + name + "' AND type='Goal'");
                        if (dddRow.Length <= 0)
                            dddRow = dt.Select("name='" + name + "'");
                        if (dddRow.Length > 0)
                        {
                            DataTable tempDt = dddRow.CopyToDataTable();
                            commission = contractValue * commissionPercent / 100D;
                            if (splitCommissionPercent > 0D)
                                commission = commission * splitCommissionPercent;
                            dValue = dddRow[0]["MR"].ObjToDouble();
                            dValue += commission;
                            dValue = G1.RoundValue(dValue);
                            dddRow[0]["MR"] = dValue;
                        }
                    }
                }
                catch ( Exception ex)
                {
                }
            }
        }
        /***********************************************************************************************/
        private void TheFinale( DataTable dt )
        {
            string agent = "";
            double totalCommission = 0D;
            double baseCommission = 0D;
            double goalCommission = 0D;
            double contractCommission = 0D;
            double splitCommission = 0D;
            double splitBaseCommission = 0D;
            double splitGoalCommission = 0D;
            double fbiCommission = 0D;
            double pastRecap = 0D;
            double pastFailures = 0D;
            double Recap = 0D;
            double Reins = 0D;
            double fbi = 0D;
            double fbiMoney = 0D;
            double MC = 0D;
            double MR = 0D;
            string splits = "";
            string type = "";
            if (G1.get_column_number(dt, "MR") < 0)
                dt.Columns.Add("MR", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "MC") < 0)
                dt.Columns.Add("MC", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "fbi$") < 0)
                dt.Columns.Add("fbi$", Type.GetType("System.Double"));
                bool doSplits = false;
            if (G1.get_column_number(dt, "splitCommission") >= 0)
                doSplits = true;
            if (G1.get_column_number(dt, "splitBaseCommission") >= 0)
                doSplits = true;
            if (G1.get_column_number(dt, "splitGoalCommission") >= 0)
                doSplits = true;
            bool doPastRecap = false;
            if (G1.get_column_number(dt, "pastRecap") >= 0)
                doPastRecap = true;
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                agent = dt.Rows[i]["agentNumber"].ObjToString();
                if (agent == "L15")
                {
                }
                type = dt.Rows[i]["type"].ObjToString();
                if (type.ToUpper() == "GOAL")
                {
                    Recap = dt.Rows[i]["Recap"].ObjToDouble();
                    Reins = dt.Rows[i]["Reins"].ObjToDouble();
                    //baseCommission = dt.Rows[i]["commission"].ObjToDouble();
                    //baseCommission = baseCommission - Recap + Reins;
                    //dt.Rows[i]["commission"] = baseCommission;
                    //dt.Rows[i]["Recap"] = 0D;
                    //dt.Rows[i]["Reins"] = 0D;
                }
                else
                {
                    Recap = 0D;
                    Reins = 0D;
                    dt.Rows[i]["Recap"] = 0D;
                    dt.Rows[i]["Reins"] = 0D;
                }
                splits = dt.Rows[i]["splits"].ObjToString();
                splitCommission = 0D;
                splitBaseCommission = 0D;
                splitGoalCommission = 0D;
                baseCommission = dt.Rows[i]["commission"].ObjToDouble();
                goalCommission = dt.Rows[i]["goalCommission"].ObjToDouble();
                MC = dt.Rows[i]["MC"].ObjToDouble();
                if (doSplits)
                {
                    splitCommission = dt.Rows[i]["splitCommission"].ObjToDouble();
                    splitBaseCommission = dt.Rows[i]["splitBaseCommission"].ObjToDouble();
                    splitGoalCommission = dt.Rows[i]["splitGoalCommission"].ObjToDouble();
                    if (type.ToUpper() == "GOAL" && !String.IsNullOrWhiteSpace(splits))
                        goalCommission = 0D;
                    if (type.ToUpper() == "GOAL")
                        dt.Rows[i]["contractCommission"] = dt.Rows[i]["contractCommission"].ObjToDouble() - dt.Rows[i]["pastFailures"].ObjToDouble() - Recap + Reins;
                    else
                        dt.Rows[i]["contractCommission"] = 0D;
                }
                else if (type.ToUpper() == "GOAL")
                {
                    dt.Rows[i]["contractCommission"] = dt.Rows[i]["contractCommission"].ObjToDouble() - dt.Rows[i]["pastFailures"].ObjToDouble() - Recap + Reins;
                }
                else
                    dt.Rows[i]["contractCommission"] = 0D;

                if ( agent == "E30")
                {
                }

                pastFailures = dt.Rows[i]["pastFailures"].ObjToDouble();
                //contractCommission = goalCommission - Recap + Reins - pastFailures + MC;
                contractCommission = goalCommission - Recap + Reins - pastFailures;
                dt.Rows[i]["contractCommission"] = contractCommission;

//                contractCommission = dt.Rows[i]["contractCommission"].ObjToDouble();
                pastFailures = dt.Rows[i]["pastFailures"].ObjToDouble();
                pastRecap = 0D;
                if ( doPastRecap )
                    pastRecap = dt.Rows[i]["pastRecap"].ObjToDouble();
                Recap = dt.Rows[i]["Recap"].ObjToDouble();
                Reins = dt.Rows[i]["Reins"].ObjToDouble();
                fbi = dt.Rows[i]["fbi"].ObjToDouble();
                fbiCommission = 0D;
                if ( fbi > 0D)
                {
                    agent = dt.Rows[i]["agentNumber"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(agent))
                    {
                        DataRow[] dR = searchAgents.Select("agentCode = '" + agent + "'");
                        if (dR.Length > 0)
                        {
                            fbiMoney = dR[0]["fbiCommission"].ObjToDouble();
                            fbiCommission = fbi * fbiMoney;
                            dt.Rows[i]["fbi$"] = fbiCommission;
                        }
                    }
                }
                totalCommission = baseCommission + contractCommission + splitCommission + splitBaseCommission + splitGoalCommission;
                //totalCommission = baseCommission + contractCommission + splitCommission + splitBaseCommission + splitGoalCommission + fbiCommission;
                //totalCommission = baseCommission + contractCommission + splitCommission + splitBaseCommission + splitGoalCommission + fbiCommission;
                //totalCommission = baseCommission + contractCommission + splitCommission + splitBaseCommission + splitGoalCommission + fbiCommission - pastFailures;
                //totalCommission = baseCommission + contractCommission + splitCommission + splitBaseCommission + splitGoalCommission + fbiCommission - pastFailures - Recap + Reins;
                //totalCommission = baseCommission + goalCommission + contractCommission - pastFailures - pastRecap - Recap + Reins + splitCommission + splitBaseCommission + splitGoalCommission + fbiCommission;
                dt.Rows[i]["totalCommission"] = totalCommission;
            }
        }
        ///***********************************************************************************************/
        //private void CalculateSpecialGoals ( DataTable dt )
        //{

        //    int startColumn = G1.get_column_number(dt, "ResultCommission");
        //    string str = "";
        //    string type = "";
        //    double amount = 0D;
        //    double goal = 0D;
        //    bool foundGoal = false;
        //    bool foundSales = false;
        //    double locationSales = 0D;
        //    double formulaSales = 0D;
        //    double dbrValue = 0D;
        //    double goalCommission = 0D;
        //    double salesCommission = 0D;
        //    double commission = 0D;
        //    double goalPercent = 0D;
        //    for ( int i=0; i<dt.Rows.Count; i++)
        //    {
        //        foundGoal = false;
        //        foundSales = false;
        //        goalCommission = 0D;
        //        salesCommission = 0D;
        //        locationSales = 0D;
        //        formulaSales = 0D;
        //        dbrValue = 0D;
        //        goalPercent = dt.Rows[i]["goalPercent"].ObjToDouble();
        //        if (goalPercent >= 1.0D)
        //            goalPercent = goalPercent / 100D;
        //        for ( int j=(startColumn+1); j<dt.Columns.Count; j++)
        //        {
        //            str = dt.Rows[i][j].ObjToString();
        //            if (str.IndexOf("~") < 0)
        //                continue;
        //            string[] Lines = str.Split('~');
        //            if (Lines.Length < 3)
        //                continue;
        //            type = Lines[0].Trim();
        //            if (type.ToUpper() == "L")
        //                foundGoal = true;
        //            else if (type.ToUpper() == "A")
        //                foundSales = true;
        //            amount = Lines[1].ObjToDouble();
        //            goal = Lines[2].ObjToDouble();
        //            if (amount > goal )
        //            {
        //                if (type.ToUpper() == "L")
        //                {
        //                    commission = amount * goalPercent;
        //                    commission = G1.RoundValue(commission);
        //                    locationSales += amount;
        //                    goalCommission += commission;
        //                }
        //                else
        //                {
        //                    commission = amount * 0.05D;
        //                    commission = G1.RoundValue(commission);
        //                    formulaSales += amount;
        //                    salesCommission += commission;
        //                }
        //            }
        //        }
        //        if (!foundGoal && !foundSales)
        //            continue;
        //        if ( foundGoal )
        //        {
        //            dt.Rows[i]["Location Sales"] = locationSales;
        //            dbrValue = dt.Rows[i]["dbrValue"].ObjToDouble();
        //            goalCommission = goalCommission - (dbrValue * goalPercent);
        //            goalCommission = G1.RoundValue(goalCommission);
        //            dt.Rows[i]["goalCommission"] = goalCommission;
        //        }
        //        if ( foundSales )
        //        {
        //            dt.Rows[i]["Formula Sales"] = formulaSales;
        //            salesCommission = G1.RoundDown(salesCommission);
        //            dt.Rows[i]["commission"] = salesCommission;
        //        }
        //        goalCommission = dt.Rows[i]["goalCommission"].ObjToDouble();
        //        salesCommission = dt.Rows[i]["commission"].ObjToDouble();
        //        commission = goalCommission + salesCommission;
        //        dt.Rows[i]["ResultCommission"] = commission;
        //    }
        //}
        ///***********************************************************************************************/
        //private void CalculatePastRecap ( DataTable dt, DataTable dt8 )
        //{
        //    DataTable allAgentsDt = G1.get_db_data("Select * from `agents`;");

        //    if (G1.get_column_number(dt, "pastRecap") < 0)
        //        dt.Columns.Add("pastRecap", Type.GetType("System.Double"));
        //    if (G1.get_column_number(dt, "pastFailures") < 0)
        //        dt.Columns.Add("pastFailures", Type.GetType("System.Double"));
        //    DataTable tempTable = dt8.Clone();
        //    double goal = 0D;
        //    double percent = 0D;
        //    string agentNumber = "";
        //    string formula = "";
        //    string[,] calc = new string[100, 2];
        //    int count = 0;
        //    double totalContracts = 0D;
        //    double lapseRecaps = 0D;
        //    double dbrSales = 0D;
        //    double lostSales = 0D;
        //    double pastFailures = 0D;
        //    double pastLapse = 0D;
        //    double recaps = 0D;
        //    double lapseContracts = 0D;
        //    string delimiter = "";
        //    string str = "";
        //    double sepGoal = 0D;
        //    for ( int i=0; i<dt.Rows.Count; i++)
        //    {
        //        agentNumber = dt.Rows[i]["agentCode"].ObjToString();
        //        goal = dt.Rows[i]["goal"].ObjToDouble();
        //        percent = dt.Rows[i]["percent"].ObjToDouble();
        //        if (percent >= 1.0)
        //            percent = percent / 100D;
        //        formula = dt.Rows[i]["formula"].ObjToString();
        //        if (goal > 0D && percent > 0D )
        //        {
        //            count = ParseOutFormula(agentNumber, goal, percent, formula, ref calc);
        //            for (int j = 0; j < count; j++)
        //            {
        //                agentNumber = calc[j, 0];
        //                if (delimiter == "=")
        //                {
        //                    str = calc[j + 1, 0].ObjToString().Trim();
        //                    str = str.Replace(",", "");
        //                    sepGoal = str.ObjToDouble();
        //                    delimiter = calc[j + 1, 1];
        //                    j = j + 1;
        //                }
        //                if (isAgent(agentNumber, allAgentsDt))
        //                {
        //                    DataRow[] dRows = dt8.Select("agentNumber='" + agentNumber + "'");
        //                    ConvertToTable(dRows, tempTable);
        //                    lapseRecaps = 0D;
        //                    pastFailures = 0D;
        //                    for (int k = 0; k < dRows.Length; k++)
        //                    {
        //                        goal = dRows[k]["goal"].ObjToDouble();
        //                        totalContracts = dRows[k]["totalContracts"].ObjToDouble();
        //                        lapseRecaps += dRows[k]["lapseRecaps"].ObjToDouble();
        //                        pastLapse = dRows[k]["lapseRecaps"].ObjToDouble();
        //                        dbrSales = dRows[k]["dbrSales"].ObjToDouble();
        //                        recaps = dRows[k]["Recap"].ObjToDouble();
        //                        lapseContracts = totalContracts - dbrSales;
        //                        lapseContracts -= (pastLapse / percent);
        //                        if (lapseContracts < goal)
        //                            pastFailures += goal * percent;
        //                    }
        //                    dt.Rows[i]["pastRecap"] = lapseRecaps;
        //                    dt.Rows[i]["pastFailures"] = pastFailures;
        //                }
        //                else
        //                {
        //                    DataRow[] dRows = dt8.Select("loc='" + agentNumber + "'");
        //                    ConvertToTable(dRows, tempTable);
        //                    lapseRecaps = 0D;
        //                    for (int k = 0; k < dRows.Length; k++)
        //                    {
        //                        goal = dRows[k]["goal"].ObjToDouble();
        //                        totalContracts = dRows[k]["totalContracts"].ObjToDouble();
        //                        lapseRecaps += dRows[k]["lapseRecaps"].ObjToDouble();
        //                        pastLapse = dRows[k]["lapseRecaps"].ObjToDouble();
        //                        dbrSales = dRows[k]["dbrSales"].ObjToDouble();
        //                        recaps = dRows[k]["Recap"].ObjToDouble();
        //                        lapseContracts = totalContracts - dbrSales;
        //                        lapseContracts -= (pastLapse / percent);
        //                        if (lapseContracts < goal)
        //                            pastFailures += goal * percent;
        //                    }
        //                    dt.Rows[i]["pastRecap"] = lapseRecaps;
        //                    dt.Rows[i]["pastFailures"] = pastFailures;
        //                }
        //            }
        //        }
        //    }
        //}
        /***********************************************************************************************/
        private void ConvertToTable( DataRow [] dRows, DataTable dt )
        {
            dt.Rows.Clear();
            for (int i = 0; i < dRows.Length; i++)
                dt.ImportRow(dRows[i]);
        }
        /***********************************************************************************************/
//        private void CalculateExtraCommissions ( bool dosplits, DataTable dt, string columnName, string resultColumn )
//        {
//            string splits = "";
//            double goal = 0D;
//            double baseCommission = 0D;
//            double commission = 0D;
//            double payment = 0D;
//            string agent = "";
//            double percent = 0D;
//            double amount = 0D;
//            double totalAmount = 0D;
//            double diff = 0D;
//            string str = "";
//            int row = 0;
//            for ( int i=0; i<dt.Rows.Count; i++)
//            {
//                goal = dt.Rows[i]["goal"].ObjToDouble();
//                splits = dt.Rows[i][columnName].ObjToString();
//                payment = dt.Rows[i]["totalPayments"].ObjToDouble();
//                if ( !dosplits)
//                    payment = dt.Rows[i]["contractValue"].ObjToDouble();
//                baseCommission = dt.Rows[i]["commission"].ObjToDouble();
//                if (dosplits)
//                    dt.Rows[i]["mainCommission"] = baseCommission;
//                if (!dosplits)
//                {
//                    if (payment < goal)
//                        continue;
//                    percent = dt.Rows[i]["goalPercent"].ObjToDouble();
//                    baseCommission = payment * (percent / 100D);
//                    baseCommission = G1.RoundValue(baseCommission);
//                    if (splits.IndexOf("~") < 0)
//                    {
//                        dt.Rows[i][resultColumn] = baseCommission;
//                        continue;
//                    }
//                }
//                totalAmount = 0D;
//                if (splits.IndexOf("~") >= 0)
//                {
//                    string[] Lines = splits.Split('~');
//                    for (int j = 0; j < Lines.Length; j = j + 2)
//                    {
//                        try
//                        {
//                            agent = Lines[j].Trim();
//                            if (String.IsNullOrWhiteSpace(agent))
//                                continue;
//                            str = Lines[j + 1].ObjToString();
//                            if (G1.validate_numeric(str))
//                            {
//                                percent = str.ObjToDouble() / 100D;
//                                commission = payment * percent;
//                                commission = G1.RoundDown(commission);
//                                row = LocateAgent(dt, agent);
//                                if (row >= 0)
//                                {
//                                    amount = dt.Rows[row][resultColumn].ObjToDouble();
//                                    amount += commission;
//                                    dt.Rows[row][resultColumn] = amount;
//                                    totalAmount += amount;
//                                }
//                            }
//                        }
//                        catch (Exception ex)
//                        {

//                        }
//                    }
//                    if (dosplits)
//                    {
//                        dt.Rows[i]["mainCommission"] = 0D;
//                        if (totalAmount != baseCommission)
//                        {
//                            diff = baseCommission - totalAmount;
////                            diff = G1.RoundValue(diff);
//                            amount = dt.Rows[i][resultColumn].ObjToDouble();
//                            amount += diff;
////                            amount = G1.RoundDown(amount);
//                            dt.Rows[i][resultColumn] = amount;
//                            //diff = dt.Rows[i]["mainCommission"].ObjToDouble();
//                            //diff = diff - amount;
//                            //dt.Rows[i]["mainCommission"] = diff;
//                        }
//                    }
//                }
//            }
//        }
        /***********************************************************************************************/
        private int LocateAgent(DataTable dt, string agent, string columnName = "", bool goal = false )
        {
            int row = -1;
            string str = "";
            if (String.IsNullOrWhiteSpace(columnName))
                columnName = "agentNumber";
            double goalPercent = 0D;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                str = dt.Rows[i][columnName].ObjToString();
                if (str == agent)
                {
                    if ( goal )
                    {
                        goalPercent = dt.Rows[i]["goalPercent"].ObjToDouble();
                        if (goalPercent == 0D)
                            continue;
                    }
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
            string workDate = cmbYear.Text;
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
        /***********************************************************************************************/
        //private void printableComponentLink1_CreateMarginalHeaderArea(object sender, CreateAreaEventArgs e)
        //{
        //    Printer.setupPrinterQuads(e, 2, 3);
        //    Font font = new Font("Ariel", 16);
        //    Printer.DrawQuad(1, 1, Printer.xQuads, 2, "South Mississippi Funeral Services, LLC", Color.Black, BorderSide.Top, font, HorizontalAlignment.Center);

        //    Printer.SetQuadSize(12, 12);

        //    font = new Font("Ariel", 8);
        //    Printer.DrawGridDate(2, 3, 2, 3, Color.Black, BorderSide.None, font);
        //    Printer.DrawGridPage(11, 3, 2, 3, Color.Black, BorderSide.None, font);

        //    Printer.DrawQuad(1, 9, 2, 3, "User : " + LoginForm.username, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);

        //    font = new Font("Ariel", 10, FontStyle.Bold);
        //    Printer.DrawQuad(6, 8, 4, 4, "Agent Commission Report", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);


        //    //            Printer.DrawQuadTicks();
        //    Printer.SetQuadSize(24, 12);
        //    font = new Font("Ariel", 9, FontStyle.Bold);
        //    Printer.DrawQuad(20, 8, 5, 4, "Report Month:" + workDate, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
        //    //Printer.DrawQuad(19, 8, 3, 4, lblTrust85.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
        //    //Printer.DrawQuad(22, 8, 3, 4, lblTrust100.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

        //    Printer.SetQuadSize(12, 12);
        //    Printer.DrawQuadBorder(1, 1, 12, 11, BorderSide.All, 1, Color.Black);
        //    Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
        //}
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
        private void CheckForRerun()
        {
            string wd1 = workDate1.ToString("yyyyMMdd");
            string wd2 = workDate2.ToString("yyyyMMdd");

            string cmd = "Select * from `commissions` where `workDate1` >= '" + wd1 + "' and `workDate2` <= '" + wd2 + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
                return;
            string agent = "";
            string agentCode = "";
            double pastFailures = 0D;
            for ( int i=0; i<dx.Rows.Count; i++)
            {
                agentCode = dx.Rows[i]["agentCode"].ObjToString();
                pastFailures = dx.Rows[i]["pastFailures"].ObjToDouble();
                for ( int j=0; j<searchAgents.Rows.Count; j++)
                {
                    agent = searchAgents.Rows[j]["agentCode"].ObjToString();
                    if (agent == agentCode )
                    {
                        searchAgents.Rows[j]["recapAmount"] = pastFailures;
                        break;
                    }
                }
            }
        }
        /****************************************************************************************/
        private void chkLoadAll_CheckedChanged(object sender, EventArgs e)
        {
            string cmd = "Select * from `agents` order by `agentCode`;";
            searchAgents = G1.get_db_data(cmd);
            LoadData();
            CheckForRerun();
        }
        /****************************************************************************************/
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        /****************************************************************************************/
        private void PreProcessGoals( DataTable dt )
        {
            string lastAgent = "";
            string agent = "";
            string type = "";

            DateTime lastGoalDate = DateTime.Now;
            DateTime lastStandardDate = DateTime.Now;
            DateTime eDate = DateTime.Now;
            bool gotGoal = false;
            bool gotStandard = false;

            for (int i=0; i<dt.Rows.Count; i++)
            {
                agent = dt.Rows[i]["agentCode"].ObjToString();
                if (String.IsNullOrWhiteSpace(lastAgent))
                    lastAgent = agent;
                if ( agent == "WF9")
                {

                }
                if ( agent != lastAgent)
                {
                    gotGoal = false;
                    gotStandard = false;
                    lastAgent = agent;
                }
                eDate = dt.Rows[i]["effectiveDate"].ObjToDateTime();
                if ( eDate > wDate )
                {
                    if (eDate >= workDate1 && eDate <= workDate2)
                    { // This means the effective date is within the month we are running
                    }
                    else
                    {
                        dt.Rows[i]["agentCode"] = "";
                        continue;
                    }
                }
                type = dt.Rows[i]["type"].ObjToString();
                if ( type.ToUpper() == "GOAL")
                {
                    if (!gotGoal)
                    {
                        gotGoal = true;
                        lastGoalDate = eDate;
                    }
                    else
                    {
                        if (eDate == lastGoalDate)
                            continue;
                        dt.Rows[i]["agentCode"] = "";
                    }
                }
                else // Must be Standard
                {
                    dt.Rows[i]["goal1"] = 0D;
                    dt.Rows[i]["goalPercent"] = 0D;
                    if ( !gotStandard)
                    {
                        gotStandard = true;
                        lastStandardDate = eDate;
                    }
                    else
                    {
                        if (eDate == lastStandardDate)
                            continue;
                        dt.Rows[i]["agentCode"] = "";
                    }
                }
            }
            for ( int i=(dt.Rows.Count-1); i>=0; i--)
            {
                eDate = dt.Rows[i]["effectiveDate"].ObjToDateTime();
                if (eDate > workDate2)
                    dt.Rows.RemoveAt(i);
                else
                    dt.Rows[i]["status"] = "Current";
            }
            dt.AcceptChanges();
        }
        /****************************************************************************************/
        private void AddRowToAudit ( DataTable dt, int row )
        {
            //DataRow dRow = auditDt.NewRow();
            //auditDt.Rows.Add(dRow);
            int last = auditDt.Rows.Count;
            G1.copy_dt_row(dt, row, auditDt, last);
        }
        /****************************************************************************************/
        private DataTable CalcAgentExtraCommission( DataTable mainDt )
        {
            // For Individual Combined goals type this under custom goals for each agent participating in the formula 
            // Example : (P44+P23) = 30000I 

            //            string cmd = "Select * from `goals` g JOIN `agents` a ON g.`agentCode` = a.`agentCode` where g.`status` = 'Current' ORDER by g.`agentCode`, `effectiveDate` DESC;";
            string cmd = "Select * from `goals` g JOIN `agents` a ON g.`agentCode` = a.`agentCode` ORDER by g.`agentCode`, `effectiveDate` DESC;";
            DataTable agents = G1.get_db_data(cmd);
            if (agents.Rows.Count <= 0)
                return agents;
            VerifyAgents(agents);

            PreProcessGoals(agents);

            agents.Columns.Add("Formula Sales", Type.GetType("System.Double"));
            agents.Columns.Add("Location Sales", Type.GetType("System.Double"));
            agents.Columns.Add("Total Sales", Type.GetType("System.Double"));
            agents.Columns.Add("TCommission", Type.GetType("System.Double"));
            agents.Columns.Add("dbrValue", Type.GetType("System.Double"));
            agents.Columns.Add("dbcMoney", Type.GetType("System.Double"));
            agents.Columns.Add("Recap", Type.GetType("System.Double"));
            agents.Columns.Add("Reins", Type.GetType("System.Double"));
            agents.Columns.Add("dbc", Type.GetType("System.Double"));
            agents.Columns.Add("ResultCommission", Type.GetType("System.Double"));
            agents.Columns.Add("totalPayments", Type.GetType("System.Double"));
            agents.Columns.Add("pastFailures", Type.GetType("System.Double"));
            agents.Columns.Add("MR", Type.GetType("System.Double"));
            agents.Columns.Add("MC", Type.GetType("System.Double"));
            agents.Columns.Add("fbi", Type.GetType("System.Double"));
            agents.Columns.Add("name");

            string name = "";
            int i = 0;

            for (i = 0; i < agents.Rows.Count; i++)
            {
                name = agents.Rows[i]["firstName"].ObjToString() + " " + agents.Rows[i]["lastName"].ObjToString();
                agents.Rows[i]["name"] = name;
            }

            auditDt = agents.Clone();

            DataTable allAgentsDt = G1.get_db_data("Select * from `agents`;");
            VerifyAgents(allAgentsDt);

            DataTable dt = workTable;
            string agentCode = "";
            string formula = "";
            string agent = "";
            string type = "";
            double fbi = 0D;
            double dbc = 0D;
            double percent = 0D;
            double goal = 0D;
            double commission = 0D;
            double dbcMoney = 0D;
            double baseCommission = 0D;
            double salesCommission = 0D;
            double totalPayments = 0D;
            double recap = 0D;
            double reins = 0D;
            int count = 0;
            bool rv = false;
            int position = -1;
            int row = 0;
            string fname = "";
            string lname = "";
            string str = "";
            DataTable goalDt = agents.Clone();
            int goalCount = 0;
            DateTime eDate = DateTime.Now;
            string status = "";
            string lastDelimiter = "";
            string parameter = "";
            string delimiter = "";
            string delimiters = @"(?<=[.,;])+->";
            delimiters = @"(?<=[;])+->";
            string[,] calc = new string[100, 2];
            string saveFormula = "";
            string lastAgent = "";
            bool historical = false;
            try
            {
                for (i = 0; i < agents.Rows.Count; i++)
                {
                    historical = false;
                    agentCode = agents.Rows[i]["agentCode"].ObjToString();
                    if (String.IsNullOrWhiteSpace(agentCode))
                    {
                        //agentCode = lastAgent;
                        //historical = true;
                        //if (agentCode != "N07")
                            continue;
                        //continue;
                    }
                    if (agentCode == "N30")
                    {
                    }
                    lastAgent = agentCode;
                    name = agents.Rows[i]["name"].ObjToString();
                    //if (name.ToUpper() != "THOMAS LAIRD")
                    //{
                    //    continue;
                    //}
                    goalCount = 0;
                    status = agents.Rows[i]["status"].ObjToString();
                    if (status.Trim().ToUpper() == "HISTORIC")
                    {
                        agents.Rows[i]["commission"] = 0D;
                        agents.Rows[i]["Tcommission"] = 0D;
                        //continue;
                    }
                    type = agents.Rows[i]["type"].ObjToString();
                    formula = agents.Rows[i]["formula"].ObjToString();
                    saveFormula = formula;
                    percent = agents.Rows[i]["percent"].ObjToDouble();
                    goal = agents.Rows[i]["goal"].ObjToDouble();
                    eDate = agents.Rows[i]["effectiveDate"].ObjToDateTime();
                    count = 0;
                    row = LocateAgent(mainDt, agentCode, "agentNumber");
                    if (row >= 0 && !historical )
                    {
                        commission = mainDt.Rows[row]["commission"].ObjToDouble();
                        dbcMoney = mainDt.Rows[row]["dbcMoney"].ObjToDouble();
                        fbi = mainDt.Rows[row]["fbi"].ObjToDouble();
                        recap = mainDt.Rows[row]["recap"].ObjToDouble();
                        reins = mainDt.Rows[row]["reins"].ObjToDouble();
                        dbc = mainDt.Rows[row]["dbc"].ObjToDouble();
                        agents.Rows[i]["commission"] = commission;
                        agents.Rows[i]["dbcMoney"] = dbcMoney;
                        agents.Rows[i]["TCommission"] = commission;
                        totalPayments = mainDt.Rows[row]["totalPayments"].ObjToDouble();
                        totalPayments = G1.RoundValue(totalPayments);
                        agents.Rows[i]["Total Sales"] = totalPayments;
                        agents.Rows[i]["totalPayments"] = totalPayments;
                        mainDt.Rows[row]["commission"] = 0D;
                        mainDt.Rows[row]["dbcMoney"] = 0D;
                        mainDt.Rows[row]["totalPayments"] = 0D;
                        agents.Rows[i]["Recap"] = recap;
                        agents.Rows[i]["Reins"] = reins;
                        agents.Rows[i]["fbi"] = fbi;
                        if (type.ToUpper() == "GOAL")
                        {
                            agents.Rows[i]["dbrValue"] = dbc;
                            agents.Rows[i]["dbc"] = dbc;
                        }
                        else
                        {
                            agents.Rows[i]["dbc"] = dbc;
                        }
                        mainDt.Rows[row]["Recap"] = 0D;
                        mainDt.Rows[row]["Reins"] = 0D;
                        mainDt.Rows[row]["fbi"] = 0D;
                    }
                    else
                    {
                        agents.Rows[i]["commission"] = 0D;
                        agents.Rows[i]["Tcommission"] = 0D;
                    }
                    if (type.ToUpper() == "STANDARD" && String.IsNullOrWhiteSpace(formula))
                    {
                        row = LocateAgent(mainDt, agentCode, "agentNumber");
                        if (row >= 0)
                        {
                            AddRowToAudit(agents, i);
                            goalDt.ImportRow(agents.Rows[i]);
                        }
                        //row = LocateAgent(mainDt, agentCode, "agentNumber");
                        //if (row >= 0)
                        //{
                        //    commission = mainDt.Rows[row]["commission"].ObjToDouble();
                        //    agents.Rows[i]["commission"] = commission;
                        //    agents.Rows[i]["TCommission"] = commission;
                        //    commission = mainDt.Rows[row]["totalPayments"].ObjToDouble();
                        //    commission = G1.RoundValue(commission);
                        //    agents.Rows[i]["Total Sales"] = commission;
                        //    agents.Rows[i]["totalPayments"] = commission;
                        //    AddRowToAudit(agents, i);
                        //}
                        continue;
                    }
                    for (;;)
                    {
                        try
                        {
                            if (String.IsNullOrWhiteSpace(formula))
                            {
                                if (goal > 0D && percent > 0D)
                                    formula = agentCode;
                            }
                            if ( agentCode == "N07" && !String.IsNullOrWhiteSpace ( formula))
                            {

                            }
                            rv = GetParameter(formula, delimiters, ref parameter, ref delimiter, ref position);
                            if (!rv)
                                break;
                            calc[count, 0] = parameter;
                            calc[count, 1] = delimiter;
                            count++;
                            if (String.IsNullOrWhiteSpace(delimiter))
                                break;
                            formula = formula.Substring((position + 1));
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("***ERROR*** Parsing Formula! " + ex.Message.ToString());
                        }
                    }
                    double formulaSales = 0D;
                    double locationSales = 0D;
                    bool grouping = false;
                    double groupLocationSales = 0D;
                    double groupFormulaSales = 0D;
                    double group_dbrValue = 0D;
                    double group_recap = 0D;
                    double group_reins = 0D;
                    double dbrSales = 0D;
                    double recapSales = 0D;
                    double reinsSales = 0D;
                    double totalSales = 0D;
                    double locationRecap = 0D;
                    double locationReins = 0D;
                    commission = 0D;
                    lastDelimiter = "";
                    double value = 0D;
                    double dbrValue = 0D;
                    recap = 0D;
                    reins = 0D;
                    dbc = 0D;
                    double sepGoal = 0D;
                    string formType = "";
                    string hold = "";
                    bool individual = false;
                    double divisor = 0D;
                    string dStr = "";
                    for (int j = 0; j < count; j++)
                    {
                        try
                        {
                            sepGoal = 0D;
                            parameter = calc[j, 0];
                            if ( parameter.IndexOf ( "/" ) > 0 )
                            {
                                int jj = parameter.IndexOf("/");
                                dStr = parameter.Substring(jj);
                                dStr = dStr.Replace( "/", "").Trim();
                                divisor = dStr.ObjToDouble();
                                parameter = parameter.Substring(0,jj);
                                parameter = parameter.Replace("/", "").Trim();
                            }
                            delimiter = calc[j, 1];
                            if ( parameter == "WF")
                            {

                            }
                            //if ( count == 1 && String.IsNullOrWhiteSpace ( delimiter))
                            //{
                            //    recapSales = agents.Rows[i]["Recap"].ObjToDouble();
                            //    reinsSales = agents.Rows[i]["Reins"].ObjToDouble();
                            //    continue;
                            //}
                            //if (String.IsNullOrWhiteSpace(parameter))
                            //    continue;
                            if (delimiter == "=")
                            {
                                str = calc[j + 1, 0].ObjToString().Trim().ToUpper();
                                str = str.Replace(",", "");
                                if ( str.IndexOf ( "I") > 0 )
                                {
                                    individual = true;
                                    str = str.Replace("I", "");
                                }
                                sepGoal = str.ObjToDouble();
                                delimiter = calc[j + 1, 1];
                                j = j + 1;
                            }
                            if (isAgent(parameter, allAgentsDt))
                            {
                                formType = "A";
                                value = GetAgentSales(mDt, parameter, dt, eDate, ref dbrValue, ref recap, ref reins, ref dbc);
                                if (divisor > 0D)
                                    value = value / divisor;
                                if (lastDelimiter == "+")
                                {
                                    formulaSales += value;
                                    dbrSales += dbrValue;
                                    recapSales += recap;
                                    reinsSales += reins;
                                }
                                else if (String.IsNullOrWhiteSpace(lastDelimiter))
                                {
                                    formulaSales = value;
                                    dbrSales = dbrValue;
                                    //                                locationSales = value;
                                    recapSales = recap;
                                    reinsSales = reins;
                                    if (grouping)
                                    {
                                        groupLocationSales += value;
                                        group_dbrValue += dbrValue;
                                        group_recap += recap;
                                        group_reins += reins;
                                        hold += parameter + delimiter;
                                    }
                                }
                            }
                            else
                            {
                                formType = "L";
                                if (delimiter == "(")
                                {
                                    grouping = true;
                                    groupLocationSales = 0D;
                                    group_dbrValue = 0D;
                                    group_recap = 0D;
                                    continue;
                                }
                                if (delimiter == ")" && String.IsNullOrWhiteSpace(parameter))
                                {
                                    hold += parameter;
                                    value = groupLocationSales;
                                    group_dbrValue = dbrValue;
                                    group_recap = recap;
                                    grouping = false;
                                    locationSales = groupLocationSales;
                                    dbrValue = group_dbrValue;
                                    recapSales = group_recap;
                                }
                                else
                                {
                                    if ( agentCode == "V25" && (parameter == "C" || parameter == "L" || parameter == "E"))
                                    {
                                        DataRow [] dddR = initDt8.Select("agentNumber='V25' AND (loc='C' OR loc='L' OR loc='E')");
                                        DataTable tempDt = initDt8.Clone();
                                        G1.ConvertToTable(dddR, tempDt);
                                    }
                                    value = GetLocationSales(parameter, dt, workDt8, workDt9, eDate, ref dbrValue, ref recap, ref reins, ref dbc );
//                                    locationRecap = GetLocationRecap(agentCode, parameter, workDt8);
                                    locationRecap = GetLocationRecap(agentCode, parameter, initDt8);
                                    recap = locationRecap;
                                    locationReins = GetLocationReins(agentCode, parameter, initDt9);
                                    reins = locationReins;
                                    if (String.IsNullOrWhiteSpace(parameter) && grouping == false && lastDelimiter == ")")
                                    {
                                        value = groupLocationSales;
                                        group_dbrValue = dbrValue;
                                        group_recap = recap;
                                        group_reins = reins;
                                        locationSales = groupLocationSales;
                                        dbrValue = group_dbrValue;
                                        //recapSales = group_recap;
                                    }
                                    if (lastDelimiter == "+")
                                    {
                                        locationSales += value;
                                        recapSales += recap;
                                        reinsSales += reins;
                                        dbrSales += dbrValue;
                                        if (grouping)
                                        {
                                            groupLocationSales += value;
                                            group_dbrValue += dbrValue;
                                            group_recap += recap;
                                            group_reins += reins;
                                            hold += parameter + delimiter;
                                        }
                                    }
                                    else if (String.IsNullOrWhiteSpace(lastDelimiter))
                                    {
                                        locationSales = value;
                                        recapSales = recap;
                                        reinsSales = reins;
                                        dbrSales = dbrValue;
                                        if (grouping)
                                        {
                                            groupLocationSales += value;
                                            group_dbrValue += dbrValue;
                                            group_recap += recap;
                                            group_reins += reins;
                                            hold += parameter + delimiter;
                                        }
                                    }
                                    if (delimiter == ")" && !String.IsNullOrWhiteSpace(parameter))
                                    {
                                        //hold += parameter;
                                        value = groupLocationSales;
                                        //group_dbrValue = dbrValue;
                                        //group_recap = recap;
                                        //group_reins = reins;
                                        grouping = false;
                                        locationSales = groupLocationSales;
                                        dbrValue = group_dbrValue;
                                        recapSales = group_recap;
                                        reinsSales = group_reins;
                                    }
                                }
                            }
                            lastDelimiter = delimiter;
                            if (sepGoal > 0D)
                            {
                                if ( agentCode == "V25")
                                {
                                }
                                if (String.IsNullOrWhiteSpace(parameter))
                                    parameter = hold;
                                if (G1.get_column_number(agents, parameter) < 0)
                                    agents.Columns.Add(parameter);
                                if (locationSales <= 0D && formulaSales > 0D)
                                {
                                    if (value <= 0D)
                                        value = formulaSales;
                                    formType = "A";
                                }
                                if ( dbrValue > 0D)
                                {
                                }
                                agents.Rows[i][parameter] = formType + "~" + value.ToString() + "~" + sepGoal.ToString() + "~" + dbrValue.ToString() + "~" + recapSales.ToString() + "~" + reinsSales.ToString();
                                hold = "";
                                goalCount++;
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("***ERROR*** Gathering Sales and Location Data " + ex.Message.ToString());
                        }
                    }
                    if (goalCount > 0)
                    {
                        if (agentCode == "V25")
                        {
                        }
                        str = "";
                        int col = G1.get_column_number(agents, "name");
                        for (int j = 1; j <= goalCount; j++)
                        {
                            str = agents.Rows[i][col + j].ObjToString();
                            string[] Lines = str.Split('~');
                            if (Lines.Length >= 5)
                            {
                                locationSales = 0D;
                                formulaSales = 0D;
                                if (Lines[0].Trim().ToUpper() == "L")
                                    locationSales = Lines[1].ObjToDouble();
                                else
                                    formulaSales = Lines[1].ObjToDouble();
                                goal = Lines[2].ObjToDouble();
                                dbrValue = Lines[3].ObjToDouble();
                                recapSales = Lines[4].ObjToDouble();
                                reinsSales = Lines[5].ObjToDouble();
                                formulaSales = G1.RoundValue(formulaSales);
                                locationSales = G1.RoundValue(locationSales);
                                dbrValue = G1.RoundValue(dbrValue);
                                baseCommission = agents.Rows[i]["commission"].ObjToDouble();
                                agents.Rows[i]["Formula Sales"] = formulaSales;
                                agents.Rows[i]["Location Sales"] = locationSales;
                                agents.Rows[i]["dbrValue"] = dbrValue;
                                agents.Rows[i]["Recap"] = recapSales;
                                agents.Rows[i]["Reins"] = reinsSales;
                                agents.Rows[i]["goal"] = goal;
                                agents.Rows[i]["dbc"] = dbc;
                                str = agents.Columns[col + j].ColumnName;
                                str = str.Replace(")", "");
                                agents.Rows[i]["formula"] = str;
                                totalSales = (formulaSales + locationSales) - dbrValue;

//                                totalSales = totalSales - recapSales + reinsSales; // Maybe
//                                totalSales = totalSales - recapSales + reinsSales - dbc; // Maybe now 4/16/2019
                                totalSales = totalSales - dbc;

                                agents.Rows[i]["Total Sales"] = G1.RoundValue(totalSales);
                                commission = totalSales * (percent / 100D);
                                if (totalSales > goal)
                                {
                                    if (individual)
                                    {
                                        value = GetAgentSales(mDt, agentCode, dt, eDate, ref dbrValue, ref recap, ref reins, ref dbc);
                                        value = value - dbrValue - dbc;
                                        if (value <= 0D)
                                            value = 0D;
                                        if (percent >= 1D)
                                            percent = percent / 100D;
                                        commission = value * percent;
                                        agents.Rows[i]["TCommission"] = commission + baseCommission;
                                        agents.Rows[i]["Reins"] = reins;
                                        agents.Rows[i]["Recap"] = recap;
                                        agents.Rows[i]["dbrValue"] = dbrValue;
                                        agents.Rows[i]["dbc"] = dbc;
                                        agents.Rows[i]["Formula Sales"] = value;
                                        agents.Rows[i]["formula"] = saveFormula;
                                    }
                                    else
                                        agents.Rows[i]["TCommission"] = commission + baseCommission;
                                }
                                else
                                    agents.Rows[i]["TCommission"] = baseCommission;
                                AddRowToAudit(agents, i);
                                goalDt.ImportRow(agents.Rows[i]);
                                agents.Rows[i]["totalPayments"] = 0D;
                                agents.Rows[i]["commission"] = 0D;
                            }
                        }
                        for (int j = agents.Columns.Count - 1; j > col; j--)
                            agents.Columns.RemoveAt(j);
                    }
                    else
                    { // This is the 1% Goal Commission Calculation
                        if (agentCode == "N07")
                        {
                        }
                        formulaSales = G1.RoundValue(formulaSales);
                        locationSales = G1.RoundValue(locationSales);
                        dbrValue = G1.RoundValue(dbrValue);
                        agents.Rows[i]["Formula Sales"] = formulaSales;
                        agents.Rows[i]["Location Sales"] = locationSales;
                        agents.Rows[i]["dbrValue"] = dbrSales;
                        agents.Rows[i]["Recap"] = recapSales;
                        agents.Rows[i]["Reins"] = reinsSales;
                        agents.Rows[i]["dbc"] = dbc;
                        totalSales = (formulaSales + locationSales) - dbrSales;

//                        totalSales = totalSales - recapSales + reinsSales; // Maybe

                        agents.Rows[i]["Total Sales"] = G1.RoundValue(totalSales);
                        salesCommission = 0D;
                        if (totalSales > goal)
                            salesCommission = totalSales * (percent / 100D);
                        commission = agents.Rows[i]["TCommission"].ObjToDouble();
                        baseCommission = commission;
                        //                    commission = agents.Rows[i]["commission"].ObjToDouble() + agents.Rows[i]["TCommission"].ObjToDouble();
                        agents.Rows[i]["TCommission"] = salesCommission + baseCommission;

                        AddRowToAudit(agents, i);
                        goalDt.ImportRow(agents.Rows[i]);
                    }
                }
            }
            catch ( Exception ex )
            {
                MessageBox.Show("Error i=" + i.ToString() + " Agent = " + agentCode + " " + ex.ToString());
            }
//            CalculateSplits(agents);
 //           CalculateSplits(goalDt);

            if ( G1.get_column_number ( mainDt, "Formula Sales") < 0 )
            {
                mainDt.Columns.Add("Formula Sales", Type.GetType("System.Double"));
                mainDt.Columns.Add("Location Sales", Type.GetType("System.Double"));
                mainDt.Columns.Add("Total Sales", Type.GetType("System.Double"));
                mainDt.Columns.Add("TCommission", Type.GetType("System.Double"));
                mainDt.Columns.Add("ResultCommission", Type.GetType("System.Double"));
            }

            //DataTable tempDt = null;
            //DataTable ttDt = null;
            //string tDate = "";
            //DataRow[] dddR = null;
            //for (i = 0; i < goalDt.Rows.Count; i++)
            //{
            //    agent = goalDt.Rows[i]["agentCode"].ObjToString();
            //    formula = goalDt.Rows[i]["formula"].ObjToString();
            //    eDate = goalDt.Rows[i]["effectiveDate"].ObjToDateTime();
            //    cmd = "Select * from `goals` where `agentCode` = '" + agent + "' AND `type` = 'GOAL' ORDER BY `effectiveDate` DESC;";
            //    tempDt = G1.get_db_data(cmd);
            //    if (tempDt.Rows.Count <= 0)
            //        continue;
            //    for ( int j=(tempDt.Rows.Count-1); j>=0;  j--)
            //    {
            //        eDate = tempDt.Rows[j]["effectiveDate"].ObjToDateTime();
            //        tDate = eDate.Year.ToString("D4") + eDate.Month.ToString("D2");
            //        dddR = workDt8.Select("YerMonth < '" + tDate + "'");
            //        ttDt = tempDt.Clone();
            //        G1.ConvertToTable(dddR, ttDt);
            //    }
            //}

            agents = goalDt.Copy();
            double pastFailures = 0D;
            for ( i=0; i<agents.Rows.Count; i++)
            {
                pastFailures = 0D;
                agent = agents.Rows[i]["agentCode"].ObjToString();
                row = LocateAgent(mainDt, agent, "agentNumber");
                if (row >= 0)
                {
                    AddToTable(mainDt, row, agents, i, "Recap");
                    AddToTable(mainDt, row, agents, i, "Reins");
                    AddToTable(mainDt, row, agents, i, "dbrValue");
                    AddToTable(mainDt, row, agents, i, "Formula Sales");
                    AddToTable(mainDt, row, agents, i, "Location Sales");
                    AddToTable(mainDt, row, agents, i, "Total Sales");
                    AddToTable(mainDt, row, agents, i, "TCommission");
                    AddToTable(mainDt, row, agents, i, "ResultCommission");
                    AddToTable(mainDt, row, agents, i, "ResultCommission", "totalCommission");
                    //                    AddToTable(mainDt, row, agents, i, "pastFailure");
                    //pastFailures = mainDt.Rows[row]["pastFailures"].ObjToDouble();
                    //agents.Rows[i]["pastFailures"] = mainDt.Rows[row]["pastFailures"].ObjToDouble();
                    //mainDt.Rows[row]["pastFailures"] = 0D; // Zero out so it will not be used again.
                }
                //if ( pastFailures > 0D)
                //{
                //    if ( agent == "N30")
                //    {

                //    }
                //    row = LocateAgent(mainDt, agent, "agentNumber", true );
                //    if (row >= 0)
                //        agents.Rows[i]["pastFailures"] = pastFailures;
                //}
            }
            DataRow[] dR = mainDt.Select("pastFailures>'0.00'");
            DataRow[] ddR = null;
            for ( i=0; i<dR.Length; i++)
            {
                pastFailures = dR[i]["pastFailures"].ObjToDouble();
                agent = dR[i]["agentNumber"].ObjToString();
                ddR = agents.Select("agentCode='" + agent + "' AND type='GOAL'");
                if ( ddR.Length > 0 )
                {
                    ddR[0]["pastFailures"] = pastFailures;
                    dR[i]["pastFailures"] = 0D;
                }
            }
            int startColumn = G1.get_column_number(agents, "name");
            type = "";
            for (i = (startColumn + 1); i < agents.Columns.Count; i++)
            {
                name = agents.Columns[i].ColumnName.ObjToString();
                if (G1.get_column_number(mainDt, name) < 0)
                {
                    type = agents.Columns[i].DataType.ToString();
                    mainDt.Columns.Add(name, Type.GetType(type));
                    for (int j = 0; j < agents.Rows.Count; j++)
                    {
                        agent = agents.Rows[j]["agentCode"].ObjToString();
                        row = LocateAgent(mainDt, agent, "agentNumber");
                        if (row >= 0)
                            mainDt.Rows[row][name] = agents.Rows[j][name].ObjToString();
                    }
                }
            }
//            return mainDt;
            return agents;
        }
        /****************************************************************************************/
        public static int ParseFormula ( string formula, ref string[,] calc )
        {
            if (String.IsNullOrWhiteSpace(formula))
                return 0;
            string goal = "";
            string[] Lines = formula.Split('=');
            int count = 0;
            for ( int i=0; i<Lines.Length; i=i+2)
            {
                formula = Lines[i];
                calc[count, 0] = formula;
                calc[count, 1] = "0.00";
                if (i < (Lines.Length - 1))
                {
                    goal = Lines[i + 1];
                    calc[count, 1] = goal;
                }
                count++;
                if (i >= (Lines.Length - 1))
                    break;
            }
            return count;
        }
        /****************************************************************************************/
        public static int ParseOutFormula( string agentCode, double goal, double percent, string formula, ref string[,] calc )
        {
            if (String.IsNullOrWhiteSpace(formula))
            {
                if (goal > 0D && percent > 0D)
                    formula = agentCode;
            }
            string parameter = "";
            string delimiter = "";
            string delimiters = @"(?<=[.,;])+->";
            delimiters = @"(?<=[;])+->";
            //            string[,] calc = new string[50, 2];
            bool rv = false;
            int count = 0;
            int position = -1;
            for (;;)
            {
                try
                {
                    rv = GetParameter(formula, delimiters, ref parameter, ref delimiter, ref position);
                    if (!rv)
                        break;
                    calc[count, 0] = parameter;
                    calc[count, 1] = delimiter;
                    count++;
                    if (String.IsNullOrWhiteSpace(delimiter))
                        break;
                    formula = formula.Substring((position + 1));
                }
                catch (Exception ex)
                {
                    MessageBox.Show("***ERROR*** Parsing Formula! " + ex.Message.ToString());
                }
            }
            return count;
        }
        /****************************************************************************************/
        private void AddToTable ( DataTable mainDt, int row, DataTable agents, int i, string column, string toColumn = "" )
        {
            if (String.IsNullOrWhiteSpace(toColumn))
                toColumn = column;
            double amount = agents.Rows[i][column].ObjToDouble();
            amount = G1.RoundValue(amount);
            double total = mainDt.Rows[row][toColumn].ObjToDouble();
            mainDt.Rows[row][toColumn] = total + amount;
        }
        /****************************************************************************************/
        public static bool CheckDeathDateCommission(string deceasedDate, string issueDate )
        {
            bool payCommission = true;
//            string deceasedDate = G1.GetSQLDate(dt, row, "deceasedDate");
            string str = "";
            int businessDays = 0;
            businessDays = 1;  // Changed 6/6/2023 as per CP
            if (G1.validate_date(deceasedDate))
            {
                DateTime ddate = deceasedDate.ObjToDateTime();
//                str = G1.GetSQLDate(dt, row, "issueDate8");
                DateTime idate = issueDate.ObjToDateTime();
                if (ddate <= idate)
                    payCommission = false;
                else
                {
                    for (;;)
                    {
                        idate = idate.AddDays(1);
                        if (idate.DayOfWeek == DayOfWeek.Saturday)
                            continue;
                        if (idate.DayOfWeek == DayOfWeek.Sunday)
                            continue;
                        if (ddate <= idate)
                        {
                            payCommission = false;
                            break;
                        }
                        businessDays++;
                        if (businessDays >= 10)
                            break;
                    }
                }
            }
            if (businessDays > 0 && !payCommission)
            {

            }
            return payCommission;
        }
        /****************************************************************************************/
        public static int CalcBusinessDays(string deceasedDate, string issueDate)
        {
            bool payCommission = true;
            string str = "";
            int businessDays = 0;
            businessDays = 1;  // Changed 6/6/2023 as per CP
            if (G1.validate_date(deceasedDate))
            {
                DateTime ddate = deceasedDate.ObjToDateTime();
                DateTime idate = issueDate.ObjToDateTime();
                if (ddate <= idate)
                {
                    for (;;)
                    {
                        idate = idate.AddDays(-1);
                        if (idate.DayOfWeek == DayOfWeek.Saturday)
                            continue;
                        if (idate.DayOfWeek == DayOfWeek.Sunday)
                            continue;
                        if (idate <= ddate)
                        {
                            payCommission = false;
                            break;
                        }
                        businessDays--;
                    }
                    payCommission = false;
                }
                else
                {
                    for (;;)
                    {
                        idate = idate.AddDays(1);
                        if (idate.DayOfWeek == DayOfWeek.Saturday)
                            continue;
                        if (idate.DayOfWeek == DayOfWeek.Sunday)
                            continue;
                        if (ddate <= idate)
                        {
                            payCommission = false;
                            break;
                        }
                        businessDays++;
                        if (businessDays >= 10)
                            break;
                    }
                }
            }
            return businessDays;
        }
        /****************************************************************************************/
        public static bool ShouldCommissionBePaid(DataTable dt, int row)
        {
            bool payCommission = true;
            string deceasedDate = G1.GetSQLDate(dt, row, "deceasedDate");
            DateTime ddate = deceasedDate.ObjToDateTime();
            if (ddate.Year < 1850)
                return true;
            string cnum = dt.Rows[row]["contractNumber"].ObjToString();
            if ( cnum == "L23119L")
            {

            }

            string str = "";
            int businessDays = 0;
            //str = dt.Rows[row]["dateDPPaid"].ObjToString();
            //if (String.IsNullOrWhiteSpace(str))
            //    return false;
            DateTime dateDPPaid = dt.Rows[row]["dateDPPaid"].ObjToDateTime();
//            DateTime dateDPPaid = dt.Rows[row]["issueDate8"].ObjToDateTime();
            if ( dateDPPaid.Year < 1850)
            {
                dateDPPaid = dt.Rows[row]["issueDate8"].ObjToDateTime();
                if (dateDPPaid.Year < 1850)
                    return true;
            }
            //            str = G1.GetSQLDate(dt, row, "issueDate8");
            businessDays = 1; // Changed 6/6/2023 as per CP
            DateTime idate = dateDPPaid;
            if (ddate <= idate)
                payCommission = false;
            else
            {
                for (;;)
                {
                    idate = idate.AddDays(1);
                    if (idate.DayOfWeek == DayOfWeek.Saturday)
                        continue;
                    if (idate.DayOfWeek == DayOfWeek.Sunday)
                        continue;
                    if (ddate <= idate)
                    {
                        payCommission = false;
                        break;
                    }
                    businessDays++;
                    if (businessDays >= 10)
                        break;
                }
            }
            if (businessDays > 0 && !payCommission)
            {

            }
            return payCommission;
        }
        /****************************************************************************************/
        public static bool CheckDeathDateCommission ( DataTable dt, int row, DateTime date1, DateTime date2 )
        {
            bool payCommission = true;
            string deceasedDate = G1.GetSQLDate(dt, row, "deceasedDate");
            string cnum = dt.Rows[row]["contractNumber"].ObjToString();

            DateTime dDate = dt.Rows[row]["deceasedDate"].ObjToDateTime();
            if (dDate.Year > 1850)
            {
//                if (dt.Rows[row]["SetAsDBR"].ObjToString().ToUpper() == "Y")
                if ( dDate >= date1 && dDate <= date2 )
                    return false;
            }


            //if ( cnum == "WF18236L")
            //{

            //}
            //string str = "";
            //int businessDays = 0;
            //if (G1.validate_date(deceasedDate))
            //{
            //    DateTime ddate = deceasedDate.ObjToDateTime();
            //    str = G1.GetSQLDate(dt, row, "issueDate8");
            //    DateTime idate = str.ObjToDateTime();
            //    if (ddate <= idate)
            //        payCommission = false;
            //    else
            //    {
            //        for (;;)
            //        {
            //            idate = idate.AddDays(1);
            //            if (idate.DayOfWeek == DayOfWeek.Saturday)
            //                continue;
            //            if (idate.DayOfWeek == DayOfWeek.Sunday)
            //                continue;
            //            if ( ddate <= idate )
            //            {
            //                payCommission = false;
            //                break;
            //            }
            //            businessDays++;
            //            if ( businessDays >= 10)
            //                break;
            //        }
            //    }
            //}
            //if ( businessDays > 0 && !payCommission )
            //{

            //}
            return payCommission;
        }
        /****************************************************************************************/
        public static double GetAgentSales(DataTable mDt, string parameter, DataTable dt, DateTime eDate, ref double dbrValue, ref double recap, ref double reins, ref double dbc )
        {
            string contract = "";
            double total = 0D;
            double value = 0D;
            double recapValue = 0D;
            dbrValue = 0D;
            recap = 0D;
            reins = 0D;
            dbc = 0D;
            if (String.IsNullOrWhiteSpace(parameter))
                return total;
            DateTime now = DateTime.Now;
            DataRow[] dRows = dt.Select("agentNumber='" + parameter.Trim() + "'");
            DataTable dx = dt.Clone();
            G1.ConvertToTable(dRows, dx);
            for (int i = 0; i < dRows.Length; i++)
            {
                now = dRows[i]["issueDate8"].ObjToDateTime();
                if (now >= eDate)
                {
                    contract = dRows[i]["contractNumber"].ObjToString();
                    if ( contract == "L23119LI")
                    {
                    }
                    if (isMeetingPaid(contract, mDt))
                        continue;
                    value = dRows[i]["contractValue"].ObjToDouble();
                    recapValue = dRows[i]["recap"].ObjToDouble();
//                    if (CheckDeathDateCommission(dx, i))
                    if (ShouldCommissionBePaid(dx, i))
                    {
                        total += value;
                        recap += recapValue;
                    }
                    else
                    {
                        total += value;
                        dbrValue += value;
                        dbc += value;
                    }
                    recapValue = dRows[i]["reins"].ObjToDouble();
                    reins += recapValue;
                }
            }
            dbrValue = dbc;
            return total;
        }
        /****************************************************************************************/
        public static double GetLocationRecap ( string agent, string parameter, DataTable workDt8)
        {
            double locRecap = 0D;
            if (workDt8 != null)
            {
                DataRow [] dRows = workDt8.Select("agentNumber='" + agent + "' AND loc='" + parameter + "'");
                for (int i = 0; i < dRows.Length; i++)
                    locRecap += dRows[i]["recap"].ObjToDouble();
            }
            return locRecap;
        }
        /****************************************************************************************/
        public static double GetLocationReins(string agent, string parameter, DataTable workDt8)
        {
            double locRecap = 0D;
            if (workDt8 != null)
            {
                DataRow[] dRows = workDt8.Select("agentNumber='" + agent + "' AND loc='" + parameter + "'");
                for (int i = 0; i < dRows.Length; i++)
                    locRecap += dRows[i]["reins"].ObjToDouble();
            }
            return locRecap;
        }
        /****************************************************************************************/
        public static double GetLocationSales(string parameter, DataTable dt, DataTable workDt8, DataTable workDt9, DateTime eDate, ref double dbrValue, ref double recap, ref double reins, ref double dbc )
        {
            double total = 0D;
            double value = 0D;
            double recapValue = 0D;
            dbrValue = 0D;
            recap = 0D;
            reins = 0D;
            dbc = 0D;
            if (String.IsNullOrWhiteSpace(parameter))
                return total;
            if ( parameter.ToUpper() == "C" || parameter.ToUpper() == "L" || parameter.ToUpper() == "E")
            {

            }
            string contractNumber = "";
            DateTime now = DateTime.Now;
            DataRow[] dRows = null;
            DataTable dx = null;
            double locRecap = 0D;
            if (workDt8 != null)
            {
                dRows = workDt8.Select("loc='" + parameter + "'");
                dx = dt.Clone();
                for (int i = 0; i < dRows.Length; i++)
                {
                    locRecap += dRows[i]["recap"].ObjToDouble();
                    dx.ImportRow(dRows[i]);
                }
            }
            //double locReins = 0D;
            //if (workDt9 != null)
            //{
            //    dRows = workDt9.Select("loc='" + parameter + "'");
            //    dx = dt.Clone();
            //    for (int i = 0; i < dRows.Length; i++)
            //    {
            //        locReins += dRows[i]["reins"].ObjToDouble();
            //        dx.ImportRow(dRows[i]);
            //    }
            //}

            dRows = dt.Select("loc='" + parameter + "'");
            dx = dt.Clone();
            for (int i = 0; i < dRows.Length; i++)
                dx.ImportRow(dRows[i]);
            for (int i = 0; i < dRows.Length; i++)
            {
                now = dRows[i]["issueDate8"].ObjToDateTime();
                if (now.Year < 1900)
                {
                    contractNumber = dRows[i]["contractNumber"].ObjToString();
                    now = DailyHistory.GetIssueDate(now, contractNumber, null);
                }
                if (now >= eDate)
                {
                    value = dRows[i]["contractValue"].ObjToDouble();
                    recapValue = dRows[i]["recap"].ObjToDouble();
                    if ( recapValue > 0D)
                    {
                    }
//                    if (CheckDeathDateCommission(dx, i))
                    if ( ShouldCommissionBePaid ( dx, i ))
                    {
                        total += value;
                        recap += recapValue;
                    }
                    else
                    {
                        total += value;
                        dbrValue += value;
                        dbc += value;
                    }
                    recapValue = dRows[i]["reins"].ObjToDouble();
                    reins += recapValue;
                }
                else
                {
                }
            }
            recap = locRecap;
            //reins = locReins;
            dbrValue = dbc;
            return total;
        }
        /****************************************************************************************/
        public static bool isAgent(string parameter, DataTable agents)
        {
            bool rv = false;
            string agentCode = "";
            for (int i = 0; i < agents.Rows.Count; i++)
            {
                agentCode = agents.Rows[i]["agentCode"].ObjToString().ToUpper();
                if (String.IsNullOrWhiteSpace(agentCode))
                    continue;
                if (parameter.Trim().ToUpper() == agents.Rows[i]["agentCode"].ObjToString().ToUpper())
                {
                    rv = true;
                    break;
                }
            }
            return rv;
        }
        /****************************************************************************************/
        public static bool GetParameter(string formula, string delimiters, ref string parameter, ref string delimiter, ref int position)
        {
            bool rv = false;
            position = -1;
            parameter = "";
            string hold = "";
            delimiter = "";
            string c = "";
            for (int i = 0; i < formula.Length; i++)
            {
                c = formula.Substring(i, 1);
                if (delimiters.Contains(c))
                {
                    if (c != "(" && c != ")")
                    {
                        parameter = formula.Substring(0, i);
                        hold += parameter;
                    }
                    else
                    {
                        hold = formula.Substring(0, i);
                        parameter = hold;
                        //parameter = "XXX";
                        //if (c == ")")
                        //    parameter = "YYY";
                    }
                    delimiter = c;
                    position = i;
                    rv = true;
                    break;
                }
            }
            if (!rv && formula.Length > 0)
            {
                parameter = formula;
                delimiter = "";
                position = formula.Length;
                rv = true;
            }
            return rv;
        }
        /***********************************************************************************************/
        //private void CalculateSplits(DataTable dt)
        //{
        //    double commission = 0D;
        //    double mainCommission = 0D;
        //    string type = "";
        //    string splits = "";
        //    string agent = "";
        //    string str = "";
        //    double percent = 0D;
        //    double payment = 0D;
        //    double amount = 0D;
        //    double totalAmount = 0D;
        //    double mainPercent = 0D;
        //    int row = 0;
        //    if (G1.get_column_number(dt, "ResultCommission") < 0)
        //        dt.Columns.Add("ResultCommission", Type.GetType("System.Double"));

        //    for (int i = 0; i < dt.Rows.Count; i++)
        //    {
        //        commission = dt.Rows[i]["TCommission"].ObjToDouble();
        //        if (commission <= 0D)
        //            continue;
        //        type = dt.Rows[i]["type"].ObjToString();
        //        splits = dt.Rows[i]["splits"].ObjToString();
        //        mainPercent = dt.Rows[i]["percent"].ObjToDouble();
        //        if (mainPercent >= 1D)
        //            mainPercent = mainPercent / 100D;
        //        if (String.IsNullOrWhiteSpace(splits))
        //        {
        //            amount = dt.Rows[i]["ResultCommission"].ObjToDouble();
        //            amount += commission;
        //            dt.Rows[i]["ResultCommission"] = amount;
        //            continue;
        //        }
        //        if (splits.IndexOf("~") >= 0)
        //        {
        //            payment = commission;
        //            string[] Lines = splits.Split('~');
        //            for (int j = 0; j < Lines.Length; j = j + 2)
        //            {
        //                try
        //                {
        //                    agent = Lines[j].Trim();
        //                    if (String.IsNullOrWhiteSpace(agent))
        //                        continue;
        //                    str = Lines[j + 1].ObjToString();
        //                    if (G1.validate_numeric(str))
        //                    {
        //                        if (type.ToUpper() == "GOAL")
        //                            percent = str.ObjToDouble();
        //                        else
        //                            percent = str.ObjToDouble() / 100D;
        //                        commission = payment * percent / mainPercent;
        //                        commission = G1.RoundDown(commission);
        //                        row = LocateAgent(dt, agent, "agentCode");
        //                        if (row >= 0)
        //                        {
        //                            amount = dt.Rows[row]["ResultCommission"].ObjToDouble();
        //                            amount += commission;
        //                            dt.Rows[row]["ResultCommission"] = amount;
        //                            totalAmount += amount;
        //                        }
        //                    }
        //                }
        //                catch (Exception ex)
        //                {

        //                }
        //            }
        //            //if (dosplits)
        //            //{
        //            //    dt.Rows[i]["mainCommission"] = 0D;
        //            //    if (totalAmount != baseCommission)
        //            //    {
        //            //        diff = baseCommission - totalAmount;
        //            //        //                            diff = G1.RoundValue(diff);
        //            //        amount = dt.Rows[i][resultColumn].ObjToDouble();
        //            //        amount += diff;
        //            //        dt.Rows[i][resultColumn] = amount;
        //            //    }
        //            //}
        //        }
        //    }
        //    for (int i = 0; i < dt.Rows.Count; i++)
        //    {
        //        commission = dt.Rows[i]["ResultCommission"].ObjToDouble();
        //        if (commission <= 0D)
        //            continue;
        //        dt.Rows[i]["TCommission"] = commission;
        //    }
        //}
        /***********************************************************************************************/
        private void CalculateAllSplits(DataTable dt)
        {
            double commission = 0D;
            double mainCommission = 0D;
            double goalCommission = 0D;
            double splitBaseCommission = 0D;
            double splitGoalCommission = 0D;
            double contractCommission = 0D;
            double recaps = 0D;
            double newRecaps = 0D;
            double newReins = 0D;
            double reins = 0D;
            bool individual = false;
            string type = "";
            string splits = "";
            string agent = "";
            string str = "";
            double percent = 0D;
            double payment = 0D;
            double amount = 0D;
            double totalAmount = 0D;
            double mainPercent = 0D;
            string thisAgent = "";
            double fbi = 0D;
            double fbiMoney = 0D;
            double fbiCommission = 0D;
            int row = 0;
            if (G1.get_column_number(dt, "splitCommission") < 0)
                dt.Columns.Add("splitCommission", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "splitBaseCommission") < 0)
                dt.Columns.Add("splitBaseCommission", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "splitGoalCommission") < 0)
                dt.Columns.Add("splitGoalCommission", Type.GetType("System.Double"));

            DataRow[] dRows = null;
            double splitBase = 0D;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                thisAgent = dt.Rows[i]["agentCode"].ObjToString();
                if ( thisAgent == "L15")
                {
                }
                commission = dt.Rows[i]["commission"].ObjToDouble();
                goalCommission = dt.Rows[i]["goalCommission"].ObjToDouble();
                recaps = dt.Rows[i]["Recap"].ObjToDouble();
                reins = dt.Rows[i]["reins"].ObjToDouble();
                contractCommission = dt.Rows[i]["contractCommission"].ObjToDouble();
                type = dt.Rows[i]["type"].ObjToString();
                splits = dt.Rows[i]["splits"].ObjToString();
                mainPercent = dt.Rows[i]["percent"].ObjToDouble();
                fbi = dt.Rows[i]["fbi"].ObjToDouble();
                fbiCommission = dt.Rows[i]["fbiCommission"].ObjToDouble();
                fbiMoney = fbi * fbiCommission;
                if (mainPercent >= 1D)
                    mainPercent = mainPercent / 100D;
                if (String.IsNullOrWhiteSpace(splits))
                    continue;
                if (splits.IndexOf("~") >= 0)
                {
                    individual = false;
                    dt.Rows[i]["commission"] = 0D;
                    dt.Rows[i]["goalCommission"] = 0D;
                    //dt.Rows[i]["Recap"] = 0D;
                    //dt.Rows[i]["reins"] = 0D;
                    payment = commission;
                    if (type.ToUpper() == "GOAL")
                    {
                        payment = goalCommission;
                        contractCommission -= goalCommission;
                        dt.Rows[i]["contractCommission"] = contractCommission;
                    }
                    payment = G1.RoundValue(payment);
                    mainCommission = dt.Rows[i]["totalCommission"].ObjToDouble();
                    mainCommission = G1.RoundValue(mainCommission);
                    mainCommission = mainCommission - payment;
                    mainCommission = G1.RoundValue(mainCommission);
                    dt.Rows[i]["totalCommission"] = mainCommission;
                    double startCommission = payment * mainPercent;
                    string[] Lines = splits.Split('~');
                    for (int j = 0; j < Lines.Length; j = j + 2)
                    {
                        try
                        {
                            newRecaps = 0D;
                            newReins = 0D;
                            agent = Lines[j].Trim();
                            if (String.IsNullOrWhiteSpace(agent))
                                continue;
                            if ( agent == "L15")
                            {

                            }
                            str = Lines[j + 1].ObjToString();
                            if (G1.validate_numeric(str))
                            {
                                if (type.ToUpper() == "GOAL")
                                {
                                    percent = str.ObjToDouble();
                                    if ( percent > 100D)
                                    {
                                        individual = true;
                                        percent = dt.Rows[i]["percent"].ObjToDouble();
                                        percent = percent / 100D;
                                    }
                                    mainPercent = 1D;
                                }
                                else
                                    percent = str.ObjToDouble() / 100D;
                                commission = payment * percent / mainPercent;
                                fbiMoney = fbi * percent / mainPercent;
                                //if ( recaps > 0D)
                                //    newRecaps = recaps * percent / mainPercent;
                                //if ( reins > 0D)
                                //    newReins = reins * percent / mainPercent;
                                commission = G1.RoundDown(commission);
                                row = LocateAgent(dt, agent, "agentCode");
                                if (thisAgent == agent)
                                    row = i;
                                if (row >= 0)
                                {
                                    if (type.ToUpper() == "GOAL")
                                    {
                                        //ToDo
                                        splitGoalCommission = dt.Rows[row]["splitGoalCommission"].ObjToDouble();
                                        splitGoalCommission = G1.RoundValue(splitGoalCommission);
                                        splitGoalCommission += commission;
                                        splitGoalCommission = G1.RoundValue(splitGoalCommission);
                                        dt.Rows[row]["splitGoalCommission"] = splitGoalCommission;

                                        //dt.Rows[row]["recap"] = newRecaps;
                                        //dt.Rows[row]["reins"] = newReins;

                                        mainCommission = dt.Rows[row]["totalCommission"].ObjToDouble();
                                        mainCommission = G1.RoundValue(mainCommission);
                                        mainCommission = mainCommission + splitGoalCommission;
                                        dt.Rows[row]["totalCommission"] = mainCommission;
                                        dt.Rows[row]["fbi"] = fbiMoney;
                                    }
                                    else
                                    {
                                        splitBaseCommission = dt.Rows[row]["splitBaseCommission"].ObjToDouble();
                                        splitBaseCommission = G1.RoundValue(splitBaseCommission);
                                        splitBaseCommission += commission;
                                        dt.Rows[row]["splitBaseCommission"] = splitBaseCommission;
                                        dt.Rows[row]["fbi"] = fbiMoney;

                                        //dt.Rows[row]["recap"] = newRecaps;
                                        //dt.Rows[row]["reins"] = newReins;

                                        mainCommission = dt.Rows[row]["totalCommission"].ObjToDouble();
                                        mainCommission = G1.RoundValue(mainCommission);
                                        mainCommission = mainCommission + splitBaseCommission;
                                        dt.Rows[row]["totalCommission"] = mainCommission;
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
            }

        }
        /***********************************************************************************************/
        private void CalculateSplitCommissions(bool dosplits, DataTable dt, string columnName, string resultColumn)
        { // It doesn't really matter here for standard splits because it's forced to match the Agent Totals back in Trust85 after running Commissions
            string splits = "";
            double goal = 0D;
            double baseCommission = 0D;
            double commission = 0D;
            double payment = 0D;
            string agent = "";
            double percent = 0D;
            double amount = 0D;
            double totalAmount = 0D;
            double diff = 0D;
            string str = "";
            int row = 0;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                goal = dt.Rows[i]["goal"].ObjToDouble();
                splits = dt.Rows[i][columnName].ObjToString();
                payment = dt.Rows[i]["totalPayments"].ObjToDouble();
                if (!dosplits)
                    payment = dt.Rows[i]["contractValue"].ObjToDouble();
                baseCommission = dt.Rows[i]["commission"].ObjToDouble();
                if (dosplits)
                    dt.Rows[i]["mainCommission"] = baseCommission;
                if (!dosplits)
                {
                    if (payment < goal)
                        continue;
                    percent = dt.Rows[i]["goalPercent"].ObjToDouble();
                    baseCommission = payment * (percent / 100D);
                    baseCommission = G1.RoundValue(baseCommission);
                    if (splits.IndexOf("~") < 0)
                    {
                        dt.Rows[i][resultColumn] = baseCommission;
                        continue;
                    }
                }
                totalAmount = 0D;
                if (splits.IndexOf("~") >= 0)
                {
                    string[] Lines = splits.Split('~');
                    for (int j = 0; j < Lines.Length; j = j + 2)
                    {
                        try
                        {
                            agent = Lines[j].Trim();
                            if (String.IsNullOrWhiteSpace(agent))
                                continue;
                            str = Lines[j + 1].ObjToString();
                            if (G1.validate_numeric(str))
                            {
                                percent = str.ObjToDouble() / 100D;
                                commission = payment * percent;
                                commission = G1.RoundDown(commission);
                                row = LocateAgent(dt, agent);
                                if (row >= 0)
                                {
                                    amount = dt.Rows[row][resultColumn].ObjToDouble();
                                    amount += commission;
                                    dt.Rows[row][resultColumn] = amount;
                                    totalAmount += amount;
                                }
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                    if (dosplits)
                    {
                        dt.Rows[i]["mainCommission"] = 0D;
                        if (totalAmount != baseCommission)
                        {
                            diff = baseCommission - totalAmount;
                            //                            diff = G1.RoundValue(diff);
                            amount = dt.Rows[i][resultColumn].ObjToDouble();
                            amount += diff;
                            //                            amount = G1.RoundDown(amount);
                            dt.Rows[i][resultColumn] = amount;
                            //diff = dt.Rows[i]["mainCommission"].ObjToDouble();
                            //diff = diff - amount;
                            //dt.Rows[i]["mainCommission"] = diff;
                        }
                    }
                }
            }
        }
        /****************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataTable dt = workTable;
            DataTable dt8 = workDt8;
            DataTable dt9 = workDt9;
            DataTable dt10 = (DataTable)dgv.DataSource;
            DateTime startDate = workDate1;
            DateTime stopDate = workDate2;

            DataRow dr = gridMain.GetFocusedDataRow();
            string agentName = dr["customer"].ObjToString();
            string agentNumber = dr["agentNumber"].ObjToString();

            this.Cursor = Cursors.WaitCursor;
            CommissionDetail commForm = new CommissionDetail(startDate, stopDate, agentNumber, agentName, dt, dt8, dt9, dt10 );
            commForm.Show();
            this.Cursor = Cursors.Default;

            //DataTable maindt = (DataTable)dgv.DataSource;
            //DataRow dr = gridMain.GetFocusedDataRow();
            //string agent = dr["customer"].ObjToString();
            //DataRow[] dR = auditDt.Select("name='" + agent + "'");

            //DataTable dt = auditDt.Clone();
            //for (int i = 0; i < dR.Length; i++)
            //{
            //    dt.ImportRow(dR[i]);
            //}

            //CommAudit CommForm = new CommAudit(dt, workTable, workDate1, workDate2);
            //CommForm.Show();
        }
        /****************************************************************************************/
        private void btnCreateTabs_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            DataTable mainDt = (DataTable)dgv.DataSource;
            string agent = dr["customer"].ObjToString();
            DataRow[] dR = mainDt.Select("customer='" + agent + "'");

            DataTable dt = mainDt.Clone();
            for (int i = 0; i < dR.Length; i++)
            {
                dt.ImportRow(dR[i]);
            }

            int count = this.tabControl1.TabPages.Count;
            TabPage tabPage2 = new System.Windows.Forms.TabPage();
            tabPage2.SuspendLayout();
            this.tabControl1.Controls.Add(tabPage2);
//            tabPage2.Controls.Add(dgv);
            tabPage2.Location = new System.Drawing.Point(4, 22);
            tabPage2.Name = "tabPage" + (count + 1).ToString();
            tabPage2.Padding = new System.Windows.Forms.Padding(3);
            tabPage2.Size = new System.Drawing.Size(1195, 271);
            tabPage2.TabIndex = 0;
            tabPage2.Text = agent;
            tabPage2.UseVisualStyleBackColor = true;
            tabPage2.ResumeLayout(false);

//            DataTable dt = (DataTable)dgv.DataSource;

//            G1.ClearTabPageControls(tabPage2);
            DupCommission dupForm = new DupCommission(dt, agent);
            //if (!this.LookAndFeel.UseDefaultLookAndFeel)
            //{
            //    tabDailyHistory.LookAndFeel.UseDefaultLookAndFeel = false;
            //    tabDailyHistory.LookAndFeel.SetSkinStyle(this.LookAndFeel.SkinName);
            //}
//            G1.LoadFormInTab(dailyForm, tabDailyHistory);
            G1.LoadFormInControl(dupForm, tabPage2);

        }
        /****************************************************************************************/
        private void chkConsolidate_CheckedChanged(object sender, EventArgs e)
        {
            if (!chkConsolidate.Checked)
            {
                dgv.DataSource = workDt10;
                dgv.Refresh();
                return;
            }
            DataTable dt = (DataTable)dgv.DataSource;
            dt = ConsolidateCommissions(dt);
            dgv.DataSource = dt;
        }
        /****************************************************************************************/
        public static DataTable ConsolidateCommissions(DataTable dt )
        {
            try
            {
                if (G1.get_column_number(dt, "endDate") < 0)
                    dt.Columns.Add("endDate");
                if (G1.get_column_number(dt, "standardFormula") < 0)
                    dt.Columns.Add("standardFormula");
                DataView tempview = dt.DefaultView;
                tempview.Sort = "customer asc, endDate asc";
                dt = tempview.ToTable();
                DataTable dx = dt.Clone();
                string oldName = "";
                string oldStandard = "";
                string standard = "";
                string agentCode = "";
                string oldDate = "";
                DataRow[] dRows = null;
                double baseCommission = 0D;
                double splitBase = 0D;
                int oldrow = 0;
                string name = "";
                string date = "";
                string type = ""; // TBelow was an attempt to fix a problem that caused some of the commissions to be doubled. The real fix is in Trust85

                //for (int i = 0; i < dt.Rows.Count; i++)
                //{
                //    name = dt.Rows[i]["customer"].ObjToString();
                //    splitBase = dt.Rows[i]["splitBaseCommission"].ObjToDouble();
                //    if (splitBase > 0D)
                //    {
                //        dRows = dt.Select("customer='" + name + "' AND commission='" + splitBase.ToString() + "'");
                //        if (dRows.Length > 0)
                //        {
                //            baseCommission = dRows[0]["commission"].ObjToDouble();
                //            if (baseCommission == splitBase)
                //                dRows[0]["commission"] = 0D;
                //        }
                //    }
                //}

                if (G1.get_column_number(dt, "lname") < 0)
                    dt.Columns.Add("lname");

                for ( int i=0; i<dt.Rows.Count; i++)
                {
                    dt.Rows[i]["lname"] = dt.Rows[i]["lastName"].ObjToString();
                }

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    name = dt.Rows[i]["customer"].ObjToString();
                    date = dt.Rows[i]["endDate"].ObjToString();
                    type = dt.Rows[i]["type"].ObjToString();
                    agentCode = dt.Rows[i]["agentCode"].ObjToString();
                    if ( type.ToUpper() == "GOAL") //RAMMA ZAMMA
                    {
                        //dt.Rows[i]["commission"] = 0D; // Added 01/10/2022 Because this caused Splits to be doubled
                        //dt.Rows[i]["splitBaseCommission"] = 0D;
                    }
                    if (String.IsNullOrWhiteSpace(oldName))
                    {
                        if (!String.IsNullOrWhiteSpace(name))
                        {
                            oldName = name;
                            oldDate = date;
                            oldrow = i;
                            oldStandard = "";
                            if (type.ToUpper() != "GOAL")
                                oldStandard = agentCode;
                            continue;
                        }
                    }
                    if (oldName != name)
                    {
                        oldName = name;
                        oldDate = date;
                        oldrow = i;
                        oldStandard = "";
                        if (type.ToUpper() != "GOAL")
                            oldStandard = agentCode;
                        continue;
                    }
                    if (oldDate != date)
                    {
                        oldName = name;
                        oldDate = date;
                        oldrow = i;
                        continue;
                    }
                    if (oldrow == i)
                        continue;
                    if (!oldStandard.Contains(agentCode))
                    {
                        if (!String.IsNullOrWhiteSpace(oldStandard))
                            oldStandard += ",";
                        oldStandard += agentCode;
                    }
                    dt.Rows[oldrow]["standardFormula"] = oldStandard;
                    CombineData(dt, oldrow, i, "contractValue");
                    CombineData(dt, oldrow, i, "goal");
                    CombineData(dt, oldrow, i, "totalPayments");
                    CombineData(dt, oldrow, i, "commission");
                    CombineData(dt, oldrow, i, "Tcommission");
                    CombineData(dt, oldrow, i, "dbcMoney");
                    CombineData(dt, oldrow, i, "mainCommission");
                    CombineData(dt, oldrow, i, "goalCommission");
                    CombineData(dt, oldrow, i, "splitCommission");
                    CombineData(dt, oldrow, i, "splitBaseCommission");
                    CombineData(dt, oldrow, i, "splitGoalCommission");
                    CombineData(dt, oldrow, i, "totalCommission");
                    CombineData(dt, oldrow, i, "Formula Sales");
                    CombineData(dt, oldrow, i, "Location Sales");
                    CombineData(dt, oldrow, i, "dbrValue");
                    CombineData(dt, oldrow, i, "pastDBR");
                    CombineData(dt, oldrow, i, "Recap");
                    CombineData(dt, oldrow, i, "Reins");
                    CombineData(dt, oldrow, i, "splits");
                    CombineData(dt, oldrow, i, "additionalGoals");
                    CombineData(dt, oldrow, i, "customGoals");
                    //                CombineData(dt, oldrow, i, "pastRecap");
                    CombineData(dt, oldrow, i, "pastFailures");
                    CombineData(dt, oldrow, i, "totalContracts");
                    CombineData(dt, oldrow, i, "contractCommission");
                    CombineData(dt, oldrow, i, "fbi");
                    CombineData(dt, oldrow, i, "MR");
                    CombineData(dt, oldrow, i, "MC");
                    CombineData(dt, oldrow, i, "fbi$");
                    CombineData(dt, oldrow, i, "lname");

                    dt.Rows[i]["contractValue"] = 0D;
                    dt.Rows[i]["goal"] = 0D;
                    dt.Rows[i]["totalPayments"] = 0D;
                    dt.Rows[i]["commission"] = 0D;
                    dt.Rows[i]["dbcMoney"] = 0D;
                    //                dt.Rows[i]["mainCommission"] = 0D;
                    dt.Rows[i]["goalCommission"] = 0D;
                    //                dt.Rows[i]["splitCommission"] = 0D;
                    dt.Rows[i]["totalCommission"] = 0D;
                    dt.Rows[i]["Formula Sales"] = 0D;
                    dt.Rows[i]["Location Sales"] = 0D;
                    dt.Rows[i]["dbrValue"] = 0D;
                    dt.Rows[i]["pastDBR"] = 0D;
                    dt.Rows[i]["Recap"] = 0D;
                    dt.Rows[i]["Reins"] = 0D;
                    //                dt.Rows[i]["pastRecap"] = 0D;
                    dt.Rows[i]["pastFailures"] = 0D;
                    dt.Rows[i]["customer"] = "";
                    dt.Rows[i]["totalContracts"] = 0D;
                    dt.Rows[i]["contractCommission"] = 0D;
                    dt.Rows[i]["totalCommission"] = 0D;
                    dt.Rows[i]["fbi"] = 0D;
                    dt.Rows[i]["fbi$"] = 0D;
                    dt.Rows[i]["MR"] = 0D;
                    dt.Rows[i]["MC"] = 0D;
                }
                for (int i = (dt.Rows.Count - 1); i >= 0; i--)
                {
                    name = dt.Rows[i]["customer"].ObjToString();
                    if (String.IsNullOrWhiteSpace(name))
                        dt.Rows.RemoveAt(i);
                }
            }
            catch ( Exception ex)
            {
            }

            DataView tempv = dt.DefaultView;
            tempv.Sort = "lname asc";
            dt = tempv.ToTable();

            G1.NumberDataTable(dt);
            return dt;
        }
        /****************************************************************************************/
        private DataTable ConsolidateLapse(DataTable dx )
        {
            DataView tempview = dx.DefaultView;
            tempview.Sort = "agentName asc, YearMonth asc";
            dx = tempview.ToTable();
            DataTable dt = dx.Copy();
            string oldName = "";
            int oldrow = 0;
            string name = "";
            string oldYearMonth = "";
            string yearMonth;
            string oldFormula = "";
            string newFormula = "";
            dt.Columns.Add("Subtract");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                name = dt.Rows[i]["agentName"].ObjToString();
                yearMonth = dt.Rows[i]["YearMonth"].ObjToString();
                if (String.IsNullOrWhiteSpace(oldName))
                {
                    if (!String.IsNullOrWhiteSpace(name))
                    {
                        oldName = name;
                        oldYearMonth = yearMonth;
                        oldrow = i;
                        continue;
                    }
                }
                if (oldName != name)
                {
                    oldName = name;
                    oldYearMonth = yearMonth;
                    oldrow = i;
                    continue;
                }
                if ( oldYearMonth != yearMonth)
                {
                    oldName = name;
                    oldYearMonth = yearMonth;
                    oldrow = i;
                    continue;
                }
                if (oldrow == i)
                    continue;
                //for ( int j=0; j<dt.Columns.Count; j++)
                //{
                //    columnName = dt.Columns[j].ColumnName.Trim();
                //    if ( columnName.ToUpper() == "FORMULA")
                //    {
                //        if (!String.IsNullOrWhiteSpace(dt.Rows[oldrow]["formula"].ObjToString()))
                //            continue;
                //    }
                //    CombineData(dt, oldrow, i, columnName);
                //    dt.Rows[i]["subtract"] = "Y";
                //}
                CombineData(dt, oldrow, i, "contractValue");
                CombineData(dt, oldrow, i, "totalContracts");
                CombineData(dt, oldrow, i, "commission");
                CombineData(dt, oldrow, i, "dbcMoney");
                CombineData(dt, oldrow, i, "dbrSales");
                CombineData(dt, oldrow, i, "Recap");
                CombineData(dt, oldrow, i, "lapseRecaps");
                CombineData(dt, oldrow, i, "contractNumber");
                CombineData(dt, oldrow, i, "cashAdvance");
                CombineData(dt, oldrow, i, "contractValue");
                oldFormula = dt.Rows[oldrow]["formula"].ObjToString();
                if ( String.IsNullOrWhiteSpace ( oldFormula))
                {
                    newFormula = dt.Rows[i]["formula"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(newFormula))
                        dt.Rows[oldrow]["formula"] = newFormula;
                }
                dt.Rows[i]["subtract"] = "Y";
            }
            for (int i = (dt.Rows.Count - 1); i >= 0; i--)
            {
                name = dt.Rows[i]["subtract"].ObjToString();
                if (name.ToUpper() =="Y")
                    dt.Rows.RemoveAt(i);
            }
            dt.Columns.Remove("subtract");

            DataTable newDt = dt.Copy();
            bool remove = false;
            for ( int i=(newDt.Columns.Count-1); i>= 0; i--)
            {
                remove = true;
                name = newDt.Columns[i].ColumnName.ToUpper();
                if (name == "AGENTNUMBER")
                    remove = false;
                else if (name == "AGENTNAME")
                    remove = false;
                else if (name == "FORMULA")
                    remove = false;
                else if (name == "CONTRACTVALUE")
                    remove = false;
                else if (name == "TOTALCONTRACTS")
                    remove = false;
                else if (name == "COMMISSION")
                    remove = false;
                else if (name == "DBCMONEY")
                    remove = false;
                else if (name == "DBRSALES")
                    remove = false;
                else if (name == "RECAP")
                    remove = false;
                else if (name == "LAPSERECAPS")
                    remove = false;
                else if (name == "CONTRACTNUMBER")
                    remove = false;
                else if (name == "TOTALCONTRACTS")
                    remove = false;
                else if (name == "CONTRACTCOMMISSION")
                    remove = false;
                else if (name == "CASHADVANCE")
                    remove = false;
                else if (name == "CONTRACTVALUE")
                    remove = false;
                else if (name == "LOC")
                    remove = false;
                else if (name == "GOAL")
                    remove = false;
                else if (name == "SPLITS")
                    remove = false;
                else if (name == "YEARMONTH")
                    remove = false;
                if (remove)
                    newDt.Columns.RemoveAt(i);
            }
            return dt;
        }
        /****************************************************************************************/
        public static void CombineData ( DataTable dt, int oldrow, int row, string field )
        {
            if (G1.get_column_number(dt, field) < 0)
                return;
            string type = "";
            try
            {
                type = dt.Columns[field].DataType.ToString().ToUpper();
                if (type.ToUpper() == "MYSQL.DATA.TYPES.MYSQLDATETIME")
                    return;
                if (type.ToUpper().IndexOf("DATETIME") >= 0)
                    return;
                if (type.ToUpper().IndexOf("INT64") >= 0)
                    return;
                if (type.ToUpper().IndexOf("INT32") >= 0)
                    return;
                if (type.IndexOf("DOUBLE") >= 0 || type.IndexOf("DECIMAL") >= 0)
                {
                    double oldValue = dt.Rows[oldrow][field].ObjToDouble();
                    double newValue = dt.Rows[row][field].ObjToDouble();
                    dt.Rows[oldrow][field] = oldValue + newValue;
                }
                else
                {
                    string oldValue = dt.Rows[oldrow][field].ObjToString();
                    string newValue = dt.Rows[row][field].ObjToString();
                    if (!String.IsNullOrWhiteSpace(oldValue))
                        oldValue += "+";
                    dt.Rows[oldrow][field] = oldValue + newValue;
                }
            }
            catch ( Exception ex )
            {
                MessageBox.Show("***ERROR*** Combining Data");
            }
        }
        /****************************************************************************************/
        private void btnLock_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            DataTable dt = (DataTable)dgv.DataSource;
            LockCommissions(dt, workDate1, workDate2);
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        public static void LockCommissions( DataTable dt, DateTime workDate1, DateTime workDate2 )
        {

            string wd1 = workDate1.ToString("yyyyMMdd");
            string wd2 = workDate2.ToString("yyyyMMdd");

            string cmd = "Select * from `commissions` where `workDate1` >= '" + wd1 + "' and `workDate2` <= '" + wd2 + "';";
            DataTable dx = G1.get_db_data(cmd);
            if ( dx.Rows.Count > 0 )
            {
                DialogResult result = MessageBox.Show("Previous Commissions have already been saved!\nAre you sure you want to OVER-WRITE these commissions?", "Over-Write Commissions Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
                if (result == DialogResult.No)
                    return;
                cmd = "Delete from `commissions` where `workDate1` >= '" + wd1 + "' and `workDate2` <= '" + wd2 + "';";
                G1.get_db_data( cmd );
            }
            string record = "";
            string agentCode = "";
            string effectiveDate = "";
            string type = "";
            string formula = "";
            string status = "";
            string percent = "";
            string goal = "";
            string splits = "";
            string firstName = "";
            string lastName = "";
            string status1 = "";
            string commission = "";
            string goal1 = "";
            string goalpercent = "";
            string splits1 = "";
            string additionalGoals = "";
            string customGoals = "";
            string recapAmount = "";
            string agentIncoming = "";
            string locCode = "";
            string Formula_Sales = "";
            string Location_Sales = "";
            string Total_Sales = "";
            string TCommission = "";
            string dbrValue = "";
            string Recap = "";
            string Reins = "";
            string ResultCommission = "";
            string totalPayments = "";
            string name = "";
            string pastRecap = "";
            string pastFailures = "";
            string contractValue = "";
            string totalContracts = "";
            string contractCommission = "";
            string goalCommission = "";
            string splitCommission = "";
            string splitBaseCommission = "";
            string splitGoalCommission = "";
            string totalCommission = "";
            string agentNumber = "";
            string fbi = "";
            string fbiCommission = "";
            double tCommission = 0D;
            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    record = G1.create_record("commissions", "agentCode", "-1");
                    if (G1.BadRecord("commissions", record))
                        break;
                    agentCode = dt.Rows[i]["agentCode"].ObjToString();
                    effectiveDate = dt.Rows[i]["effectiveDate"].ObjToString();
                    G1.update_db_table("commissions", "record", record, new string[] { "workDate1", wd1, "workDate2", wd2, "agentCode", agentCode, "effectiveDate", effectiveDate });
                    type = dt.Rows[i]["type"].ObjToString();
                    formula = dt.Rows[i]["formula"].ObjToString();
                    status = dt.Rows[i]["status"].ObjToString();
                    percent = dt.Rows[i]["percent"].ObjToString();
                    goal = dt.Rows[i]["goal"].ObjToString();
                    splits = dt.Rows[i]["splits"].ObjToString();
                    firstName = dt.Rows[i]["firstName"].ObjToString();
                    lastName = dt.Rows[i]["lastName"].ObjToString();
                    G1.update_db_table("commissions", "record", record, new string[] { "type", type, "formula", formula, "status", status, "percent", percent, "goal", goal, "splits", splits, "firstName", firstName, "lastName", lastName });
                    status1 = dt.Rows[i]["status1"].ObjToString();
                    commission = dt.Rows[i]["commission"].ObjToString();
                    goal1 = dt.Rows[i]["goal1"].ObjToString();
                    goalpercent = dt.Rows[i]["goalpercent"].ObjToString();
                    splits1 = dt.Rows[i]["splits1"].ObjToString();
                    additionalGoals = dt.Rows[i]["additionalGoals"].ObjToString();
                    customGoals = dt.Rows[i]["customGoals"].ObjToString();
                    recapAmount = dt.Rows[i]["recapAmount"].ObjToString();
                    G1.update_db_table("commissions", "record", record, new string[] { "status1", status1, "commission", commission, "goal1", goal1, "goalpercent", goalpercent, "splits1", splits1, "additionalGoals", additionalGoals, "customGoals", customGoals, "recapAmount", recapAmount });
                    agentIncoming = dt.Rows[i]["agentIncoming"].ObjToString();
                    locCode = dt.Rows[i]["locCode"].ObjToString();
                    Formula_Sales = dt.Rows[i]["Formula Sales"].ObjToString();
                    Location_Sales = dt.Rows[i]["Location Sales"].ObjToString();
                    Total_Sales = dt.Rows[i]["Total Sales"].ObjToString();
                    TCommission = dt.Rows[i]["TCommission"].ObjToString();
                    dbrValue = dt.Rows[i]["dbrValue"].ObjToString();
                    G1.update_db_table("commissions", "record", record, new string[] { "agentIncoming", agentIncoming, "locCode", locCode, "Formula Sales", Formula_Sales, "Location Sales", Location_Sales, "Total Sales", Total_Sales, "TCommission", TCommission, "dbrValue", dbrValue });
                    Recap = dt.Rows[i]["Recap"].ObjToString();
                    Reins = dt.Rows[i]["Reins"].ObjToString();
                    ResultCommission = dt.Rows[i]["ResultCommission"].ObjToString();
                    totalPayments = dt.Rows[i]["totalPayments"].ObjToString();
                    name = dt.Rows[i]["name"].ObjToString();
                    //                pastRecap = dt.Rows[i]["pastRecap"].ObjToString();
                    G1.update_db_table("commissions", "record", record, new string[] { "Recap", Recap, "Reins", Reins, "ResultCommission", ResultCommission, "totalPayments", totalPayments, "name", name });
                    pastFailures = dt.Rows[i]["pastFailures"].ObjToString();
                    contractValue = dt.Rows[i]["contractValue"].ObjToString();
                    totalContracts = dt.Rows[i]["totalContracts"].ObjToString();
                    contractCommission = dt.Rows[i]["contractCommission"].ObjToString();
                    goalCommission = dt.Rows[i]["goalCommission"].ObjToString();
                    splitCommission = dt.Rows[i]["splitCommission"].ObjToString();
                    splitBaseCommission = dt.Rows[i]["splitBaseCommission"].ObjToString();
                    splitGoalCommission = dt.Rows[i]["splitGoalCommission"].ObjToString();
                    totalCommission = dt.Rows[i]["totalCommission"].ObjToString();
                    agentNumber = dt.Rows[i]["agentNumber"].ObjToString();
                    fbi = dt.Rows[i]["fbi"].ObjToString();
                    fbiCommission = dt.Rows[i]["fbi$"].ObjToString();
                    G1.update_db_table("commissions", "record", record, new string[] { "pastFailures", pastFailures, "contractValue", contractValue, "totalContracts", totalContracts, "contractCommission", contractCommission, "goalCommission", goalCommission, "totalCommission", totalCommission, "agentNumber", agentNumber, "fbi", fbi, "fbi$", fbiCommission, "splitCommission", splitCommission, "splitBaseCommission", splitBaseCommission, "splitGoalCommission", splitGoalCommission });
                    tCommission = totalCommission.ObjToDouble();
                    if (tCommission >= 0D)
                        tCommission = 0D;
                    else
                        tCommission = tCommission * -1D;
                    cmd = "Select * from `agents` where `agentCode` = '" + agentCode + "';";
                    DataTable agentDt = G1.get_db_data(cmd);
                    if (agentDt.Rows.Count > 0)
                    {
                        record = agentDt.Rows[0]["record"].ObjToString();
                        G1.update_db_table("agents", "record", record, new string[] { "recapAmount", tCommission.ToString() });
                    }
                }
            }
            catch ( Exception ex )
            {
                MessageBox.Show("***ERROR*** " + ex.Message.ToString());
            }
        }
        /****************************************************************************************/
        private void pieChartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;

            DataView tempview = dt.DefaultView;
            tempview.Sort = "name asc, agentCode asc";
            dt = tempview.ToTable();

            DataTable dx = new DataTable();
            dx.Columns.Add("agentNumber");
            dx.Columns.Add("agentName");
            dx.Columns.Add("Contract Commission", Type.GetType("System.Double"));
            dx.Columns.Add("Base Commission", Type.GetType("System.Double"));
            dx.Columns.Add("Total Commission", Type.GetType("System.Double"));

            string oldAgentNumber = "";
            string oldAgentName = "";

            string agentNumber = "";
            string agentName = "";

            double T_TotalBase = 0D;
            double T_TotalCommissions = 0D;
            double T_TotalContracts = 0D;

            double totalPayments = 0D;
            double commission = 0D;
            double contractValue = 0D;

            DataRow dRow = null;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                agentName = dt.Rows[i]["name"].ObjToString();
                if (String.IsNullOrWhiteSpace(oldAgentName))
                    oldAgentName = agentName;
                if (oldAgentName != agentName)
                {
                    dRow = dx.NewRow();
                    dRow["agentNumber"] = oldAgentNumber;
                    dRow["agentName"] = oldAgentName;
                    if (T_TotalBase <= 0D)
                        T_TotalBase = 0D;
                    if (T_TotalCommissions <= 0D)
                        T_TotalCommissions = 0D;
                    if (T_TotalContracts <= 0D)
                        T_TotalContracts = 0D;
                    dRow["Base Commission"] = T_TotalBase;
                    dRow["Total Commission"] = T_TotalCommissions;
                    dRow["Contract Commission"] = T_TotalContracts;
                    dx.Rows.Add(dRow);
                    T_TotalBase = 0D;
                    T_TotalCommissions = 0D;
                    T_TotalContracts = 0D;
                }
                oldAgentName = agentName;
                oldAgentNumber = dt.Rows[i]["agentCode"].ObjToString();
                commission = dt.Rows[i]["commission"].ObjToDouble();
                contractValue = dt.Rows[i]["totalCommission"].ObjToDouble();
                totalPayments = dt.Rows[i]["contractCommission"].ObjToDouble();

                T_TotalCommissions += contractValue;
                T_TotalContracts += totalPayments;
                T_TotalBase += commission;
            }
            dRow = dx.NewRow();
            dRow["agentNumber"] = oldAgentNumber;
            dRow["agentName"] = oldAgentName;
            dRow["Base Commission"] = T_TotalBase;
            dRow["Total Commission"] = T_TotalCommissions;
            dRow["Contract Commission"] = T_TotalContracts;
            dx.Rows.Add(dRow);

            PieChart pieForm = new PieChart("", dx, workDate1, workDate2);
            pieForm.Show();
        }
        /****************************************************************************************/
        private void menuBarChart_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;

            DataView tempview = dt.DefaultView;
            tempview.Sort = "name asc, agentCode asc";
            dt = tempview.ToTable();

            DataTable dx = new DataTable();
            dx.Columns.Add("agentNumber");
            dx.Columns.Add("agentName");
            dx.Columns.Add("Contract Commission", Type.GetType("System.Double"));
            dx.Columns.Add("Base Commission", Type.GetType("System.Double"));
            dx.Columns.Add("Total Commission", Type.GetType("System.Double"));

            string oldAgentNumber = "";
            string oldAgentName = "";

            string agentNumber = "";
            string agentName = "";

            double T_TotalBase = 0D;
            double T_TotalCommissions = 0D;
            double T_TotalContracts = 0D;

            double totalPayments = 0D;
            double commission = 0D;
            double contractValue = 0D;

            DataRow dRow = null;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                agentName = dt.Rows[i]["name"].ObjToString();
                if (String.IsNullOrWhiteSpace(oldAgentName))
                    oldAgentName = agentName;
                if (oldAgentName != agentName)
                {
                    dRow = dx.NewRow();
                    dRow["agentNumber"] = oldAgentNumber;
                    dRow["agentName"] = oldAgentName;
                    if (T_TotalBase <= 0D)
                        T_TotalBase = 0D;
                    if (T_TotalCommissions <= 0D)
                        T_TotalCommissions = 0D;
                    if (T_TotalContracts <= 0D)
                        T_TotalContracts = 0D;
                    dRow["Base Commission"] = T_TotalBase;
                    dRow["Total Commission"] = T_TotalCommissions;
                    dRow["Contract Commission"] = T_TotalContracts;
                    dx.Rows.Add(dRow);
                    T_TotalBase = 0D;
                    T_TotalCommissions = 0D;
                    T_TotalContracts = 0D;
                }
                oldAgentName = agentName;
                oldAgentNumber = dt.Rows[i]["agentCode"].ObjToString();
                commission = dt.Rows[i]["commission"].ObjToDouble();
                contractValue = dt.Rows[i]["totalCommission"].ObjToDouble();
                totalPayments = dt.Rows[i]["contractCommission"].ObjToDouble();

                T_TotalCommissions += contractValue;
                T_TotalContracts += totalPayments;
                T_TotalBase += commission;
            }
            dRow = dx.NewRow();
            dRow["agentNumber"] = oldAgentNumber;
            dRow["agentName"] = oldAgentName;
            dRow["Base Commission"] = T_TotalBase;
            dRow["Total Commission"] = T_TotalCommissions;
            dRow["Contract Commission"] = T_TotalContracts;
            dx.Rows.Add(dRow);

            BarChart barForm = new BarChart("", dx, workDate1, workDate2);
            barForm.Show();
        }
        /****************************************************************************************/
        private void chkComboAgent_EditValueChanged(object sender, EventArgs e)
        {
            string names = getAgentQuery();
            DataRow[] dRows = workDt10.Select(names);
            DataTable dt = workDt10.Clone();
            for (int i = 0; i < dRows.Length; i++)
                dt.ImportRow(dRows[i]);
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
        }
        /*******************************************************************************************/
        private string getAgentQuery(string agent = "")
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
            if (String.IsNullOrWhiteSpace(agent))
                agent = "agentCode";
            return procLoc.Length > 0 ? " `" + agent + "` IN (" + procLoc + ") " : "";
        }
        /****************************************************************************************/
        private void chkComboAgentNames_EditValueChanged(object sender, EventArgs e)
        {
            string names = getAgentNameQuery();
            DataRow[] dRows = workDt10.Select(names);
            DataTable dt = workDt10.Clone();
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
        private void agentMonthlyHistoryReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }
        /****************************************************************************************/
    }
}