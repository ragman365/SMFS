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
    public partial class CommAudit : DevExpress.XtraEditors.XtraForm
    {
        private DataTable workTable = null;
        private DataTable workMainTable = null;
        private DataTable workAgents = null;
        private DataTable searchAgents = null;
        private DataTable auditDt = null;
        private string workDate = "";
        private DateTime workDate1;
        private DateTime workDate2;
        private DateTime wDate;
        private bool first = true;
        /****************************************************************************************/
        public CommAudit(DataTable dt, DataTable mainDt, DateTime date1, DateTime date2 )
        {
            InitializeComponent();
            workTable = dt;
            workMainTable = mainDt;
            workDate1 = date1;
            workDate2 = date2;
            SetupTotalsSummary();
        }
        /****************************************************************************************/
        private void CommAudit_Load(object sender, EventArgs e)
        {
            G1.SetupVisibleColumns(gridMain, this.columnsToolStripMenuItem, nmenu_Click);
//            LoadAgents();
            LoadData();
        }
        /****************************************************************************************/
        private void SetupTotalsSummary()
        {
            AddSummaryColumn("TCommission");
            AddSummaryColumn("Total Sales");
            AddSummaryColumn("Location Sales");
            AddSummaryColumn("Formula Sales");
            AddSummaryColumn("contractValue");
            AddSummaryColumn("dbrValue");
            AddSummaryColumn("Recap");
        }
        /****************************************************************************************/
        private void AddSummaryColumn ( string columnName)
        {
            gridMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            gridMain.Columns[columnName].SummaryItem.DisplayFormat = "${0:0,0.00}";
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
            string cmd = "Select * from `agents` where `agentCode` in (" + list + ") order by `agentCode`;";
            DataTable dt = G1.get_db_data(cmd);
            chkComboAgent.Properties.DataSource = dt;
            searchAgents = dt.Copy();
        }
        /****************************************************************************************/
        private void LoadData()
        {
            DataTable allAgentsDt = G1.get_db_data("Select * from `agents`;");
            DateTime eDate = DateTime.Now;

            string formula = "";
            string area = "";
            string type = "";
            double value = 0D;
            double dbrValue = 0D;
            double dbr = 0D;
            double recap = 0D;
            double recapValue = 0D;
            double totalSales = 0D;
            double formulaSales = 0D;
            double locationSales = 0D;
            DataTable dt = workTable.Clone();
            dt.Columns.Add("num");
            dt.Columns.Add("agent");
            dt.Columns.Add("loc");
            dt.Columns.Add("contractNumber");
            dt.Columns.Add("contractValue", Type.GetType("System.Double"));
            string agent = "";
            for ( int i=0; i<workTable.Rows.Count; i++)
            {
//                dt.ImportRow(workTable.Rows[i]);
                agent = workTable.Rows[i]["agentCode"].ObjToString();
                formula = workTable.Rows[i]["formula"].ObjToString();
                type = workTable.Rows[i]["type"].ObjToString();
                eDate = workTable.Rows[i]["effectiveDate"].ObjToDateTime();
                if (type.ToUpper() == "GOAL")
                {
                    string[] Lines = formula.Split('+');
                    for ( int j=0; j<Lines.Length; j++)
                    {
                        area = Lines[j];
                        if (Commission.isAgent(area, allAgentsDt))
                        {
                            //DataRow dRow = dt.NewRow();
                            //dRow["agentCode"] = agent;
                            //dRow["agent"] = area;
                            //dRow["Formula Sales"] = value;
                            //dRow["dbrValue"] = dbrValue;
                            //dRow["Recap"] = recap;
                            //totalSales = value;
                            //dRow["Total Sales"] = G1.RoundValue(totalSales);
                            //dt.Rows.Add(dRow);
                            int holdRow = dt.Rows.Count;

                            value = GetAgentSales(true, agent, area, workMainTable, eDate, ref dbrValue, ref recapValue, dt, workTable, i );

                            formulaSales += value;
                            dbr += dbrValue;
                            recap += recapValue;

                            //dt.Rows[holdRow - 1]["agentCode"] = agent;
                            //dt.Rows[holdRow - 1]["agent"] = area;
                            //dt.Rows[holdRow - 1]["Formula Sales"] = value;
                            //dt.Rows[holdRow - 1]["dbrValue"] = dbrValue;
                            //dt.Rows[holdRow - 1]["Recap"] = recap;
                            //totalSales = value;
                            //dt.Rows[holdRow - 1]["Total Sales"] = G1.RoundValue(totalSales);
                        }
                        else
                        {
                            //DataRow dRow = dt.NewRow();
                            //dRow["agentCode"] = agent;
                            //dRow["loc"] = area;
                            //dRow["Location Sales"] = 0D;
                            //dRow["dbrValue"] = 0D;
                            //dRow["Recap"] = 0D;
                            //totalSales = 0D;
                            //dRow["Total Sales"] = G1.RoundValue(totalSales);
                            //dt.Rows.Add(dRow);
                            //int holdRow = dt.Rows.Count;

                            value = GetLocationSales(true, agent, area, workMainTable, eDate, ref dbrValue, ref recapValue, dt, workTable, i );

                            locationSales += value;
                            dbr += dbrValue;
                            recap += recapValue;

                            //dt.Rows[holdRow-1]["agentCode"] = agent;
                            //dt.Rows[holdRow - 1]["loc"] = area;
                            //dt.Rows[holdRow - 1]["Location Sales"] = value;
                            //dt.Rows[holdRow - 1]["dbrValue"] = dbrValue;
                            //dt.Rows[holdRow - 1]["Recap"] = recap;
                            //totalSales = value;
                            //dt.Rows[holdRow - 1]["Total Sales"] = G1.RoundValue(totalSales);
                        }
                    }
                }
                else
                {
                    if (Commission.isAgent(agent, allAgentsDt))
                    {
                        int holdRow = dt.Rows.Count;
                        value = GetAgentSales(false, agent, agent, workMainTable, eDate, ref dbrValue, ref recapValue, dt, workTable, i);
                        formulaSales += value;
                        dbr += dbrValue;
                        recap += recapValue;
                    }
                    else
                    {
                        value = GetLocationSales(false, agent, agent, workMainTable, eDate, ref dbrValue, ref recapValue, dt, workTable, i);
                        locationSales += value;
                        dbr += dbrValue;
                        recap += recapValue;
                    }
                }
            }

            CheckForMainLapse(dt, workDate1, workDate2);

//            DataRow dRow = dt.NewRow();
//            dRow["agentCode"] = agent;
//            dRow["loc"] = "Totals";
////            dRow["Location Sales"] = locationSales;
////            dRow["Formula Sales"] = formulaSales;
////            dRow["dbrValue"] = dbrValue;
//            dRow["Recap"] = recap;
//            totalSales = locationSales + formulaSales;
////            dRow["Total Sales"] = G1.RoundValue(totalSales);
//            dt.Rows.Add(dRow);

            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
        }
        /****************************************************************************************/
        private double GetAgentSales(bool add, string agent, string parameter, DataTable dt, DateTime eDate, ref double dbrValue, ref double recap, DataTable resultDt, DataTable workTable, int mainRow)
        {
            string contract = "";
            string loc = "";
            string trust = "";
            double totalSales = 0D;
            double total = 0D;
            double value = 0D;
            double recapValue = 0D;
            recap = 0D;
            dbrValue = 0D;
            double dbr = 0D;
            DateTime now = DateTime.Now;
            DataRow[] dRows = dt.Select("agentNumber='" + parameter + "'");
            DataTable dx = dt.Clone();
            for (int i = 0; i < dRows.Length; i++)
                dx.ImportRow(dRows[i]);
            for (int i = 0; i < dRows.Length; i++)
            {
                now = dRows[i]["issueDate8"].ObjToDateTime();
                contract = dRows[i]["contractNumber"].ObjToString();
                if (now >= eDate)
                {
                    dbr = 0D;
                    totalSales = 0D;
                    Trust85.decodeContractNumber(contract, ref trust, ref loc);
                    value = dRows[i]["contractValue"].ObjToDouble();
                    recapValue = dRows[i]["recap"].ObjToDouble();
                    if (Commission.CheckDeathDateCommission(dx, i, workDate1, workDate2))
                    {
                        total += value;
                        recap += recapValue;
                    }
                    else
                    {
                        dbr = value;
                        dbrValue += value;
                    }
                    if ((value > 0D || recapValue > 0D) && add )
                    {
                        DataRow dRow = resultDt.NewRow();
                        dRow["effectiveDate"] = G1.DTtoMySQLDT(workTable.Rows[mainRow]["effectiveDate"]);
                        dRow["goal"] = workTable.Rows[mainRow]["goal"].ObjToDouble();
                        dRow["percent"] = workTable.Rows[mainRow]["percent"].ObjToDouble();
                        dRow["formula"] = workTable.Rows[mainRow]["formula"].ObjToString();
                        dRow["type"] = workTable.Rows[mainRow]["type"].ObjToString();
                        dRow["splits"] = workTable.Rows[mainRow]["splits"].ObjToString();
                        dRow["contractNumber"] = contract;
                        dRow["contractValue"] = value;
                        dRow["loc"] = loc;
                        dRow["agentCode"] = agent;
                        dRow["agent"] = parameter;
                        dRow["Formula Sales"] = 0D;
                        if (dbr <= 0D)
                        {
                            dRow["Formula Sales"] = value;
                            totalSales = value;
                        }
                        dRow["dbrValue"] = dbr;
                        dRow["Recap"] = recapValue;
                        dRow["Total Sales"] = G1.RoundValue(totalSales);
                        resultDt.Rows.Add(dRow);
                    }
                }
            }
            return total;
        }
        /****************************************************************************************/
        private double GetLocationSales( bool add, string agent, string parameter, DataTable dt, DateTime eDate, ref double dbrValue, ref double recap, DataTable resultDt, DataTable workMainTable, int mainRow)
        {
            string contract = "";
            string trust = "";
            string loc = "";
            double totalSales = 0D;
            double total = 0D;
            double value = 0D;
            double recapValue = 0D;
            recap = 0D;
            dbrValue = 0D;
            double dbr = 0D;
            DateTime now = DateTime.Now;
            DataRow[] dRows = dt.Select("loc='" + parameter + "'");
            DataTable dx = dt.Clone();
            for (int i = 0; i < dRows.Length; i++)
                dx.ImportRow(dRows[i]);
            for (int i = 0; i < dRows.Length; i++)
            {
                now = dRows[i]["issueDate8"].ObjToDateTime();
                contract = dRows[i]["contractNumber"].ObjToString();
                if (now >= eDate)
                {
                    dbr = 0D;
                    totalSales = 0D;
                    Trust85.decodeContractNumber(contract, ref trust, ref loc);
                    value = dRows[i]["contractValue"].ObjToDouble();
                    recapValue = dRows[i]["recap"].ObjToDouble();
                    if (Commission.CheckDeathDateCommission(dx, i, workDate1, workDate2))
                    {
                        total += value;
                        recap += recapValue;
                    }
                    else
                    {
                        dbr = value;
                        dbrValue += value;
                    }
                    if ((value > 0D || recapValue > 0D) && add )
                    {
                        DataRow dRow = resultDt.NewRow();
                        dRow["effectiveDate"] = G1.DTtoMySQLDT(workTable.Rows[mainRow]["effectiveDate"]);
                        dRow["goal"] = workTable.Rows[mainRow]["goal"].ObjToDouble();
                        dRow["percent"] = workTable.Rows[mainRow]["percent"].ObjToDouble();
                        dRow["formula"] = workTable.Rows[mainRow]["formula"].ObjToString();
                        dRow["type"] = workTable.Rows[mainRow]["type"].ObjToString();
                        dRow["splits"] = workTable.Rows[mainRow]["splits"].ObjToString();
                        dRow["contractNumber"] = contract;
                        dRow["contractValue"] = value;
                        dRow["agentCode"] = agent;
                        dRow["agent"] = parameter;
                        dRow["loc"] = loc;
                        dRow["Location Sales"] = 0D;
                        if (dbr <= 0D)
                        {
                            dRow["Location Sales"] = value;
                            totalSales = value;
                        }
                        dRow["dbrValue"] = dbr;
                        dRow["Recap"] = recapValue;
                        dRow["Total Sales"] = G1.RoundValue(totalSales);
                        resultDt.Rows.Add(dRow);
                    }
                }
            }
            return total;
        }
        /****************************************************************************************/
        private void CheckForMainLapse(DataTable dt, DateTime date1, DateTime date2)
        {
            if (G1.get_column_number(dt, "Recap") < 0)
                dt.Columns.Add("Recap", Type.GetType("System.Decimal"));


            string agentList = "";
            string agent = "";
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                agent = dt.Rows[i]["agent"].ObjToString();
                if (agentList.Contains(agent))
                    continue;
                agentList += agent + ",";
            }
            agentList = agentList.TrimEnd(',');

            DateTime lapseDate = date1;
            //lapseDate = lapseDate.AddMonths(1);
            //int days = DateTime.DaysInMonth(lapseDate.Year, lapseDate.Month);
            string start = lapseDate.Year.ToString("D4") + "-" + lapseDate.Month.ToString("D2") + "-" + lapseDate.Day.ToString("D2");

            lapseDate = date2;
            string end = lapseDate.Year.ToString("D4") + "-" + lapseDate.Month.ToString("D2") + "-" + lapseDate.Day.ToString("D2");

            string cmd = "Select * from `contracts` c JOIN `customers` a ON c.`contractNumber` = a.`ContractNumber` where `lapsedate8` >= '" + start + "' AND `lapsedate8` <= '" + end + "';";
            DataTable lapseDt = G1.get_db_data(cmd);
            double contractValue = 0D;
            string contract = "";
            for (int i = 0; i < lapseDt.Rows.Count; i++)
            {
                contractValue = DailyHistory.GetContractValue(lapseDt.Rows[i]);
                contractValue = G1.RoundValue(contractValue);
                contract = lapseDt.Rows[i]["contractNumber"].ObjToString();
                DataRow[] dRows = dt.Select("contractNumber='" + contract + "'");
                if (dRows.Length > 0)
                    dRows[0]["Recap"] = contractValue * 0.01D;
                else
                {
                    agent = lapseDt.Rows[i]["agentCode"].ObjToString();
                    if (agentList.Contains(agent))
                    {
                        DataRow dRow = dt.NewRow();
                        dRow["contractNumber"] = contract;
                        dRow["agentCode"] = agent;
                        dRow["contractValue"] = contractValue;
                        dRow["Recap"] = contractValue * 0.01D;
                        dt.Rows.Add(dRow);
                    }
                }
            }
        }
        /****************************************************************************************/
        private void LoadDatax()
        {
            dgv.DataSource = workTable;
//            DataTable dt = workTable.Clone();
//            DataTable gDx = G1.get_db_data("Select * from `goals` order by `agentCode`,`effectiveDate`;");
//            dt.Columns.Add("splits");
//            dt.Columns.Add("additionalGoals");
//            dt.Columns.Add("goal", Type.GetType("System.Double"));
//            dt.Columns.Add("goalPercent", Type.GetType("System.Double"));
//            dt.Columns.Add("mainCommission", Type.GetType("System.Double"));
//            dt.Columns.Add("splitCommission", Type.GetType("System.Double"));
//            dt.Columns.Add("goalCommission", Type.GetType("System.Double"));
//            dt.Columns.Add("totalCommission", Type.GetType("System.Double"));
//            double payment = 0D;
//            double agentPayments = 0D;
//            double totalPayments = 0D;
//            double totalCommission = 0D;
//            double agentCommission = 0D;
//            double commission = 0D;
//            double contractValue = 0D;
//            double agentContracts = 0D;
//            double goal = 0D;
//            double goalPercent = 0D;
//            string fname = "";
//            string lname = "";
//            string name = "";
//            string additional = "";
//            string splits = "";
//            string agent = "";

//            for ( int i=0; i<searchAgents.Rows.Count; i++)
//            {
//                agent = searchAgents.Rows[i]["agentCode"].ObjToString();
//                DataRow[] dRows = workTable.Select("agentNumber='" + agent + "'");
//                if (dRows.Length <= 0)
//                    continue;
//                agentCommission = 0D;
//                agentPayments = 0D;
//                agentContracts = 0D;
//                for ( int j=0; j<dRows.Length; j++)
//                {
//                    commission = dRows[j]["commission"].ObjToDouble();
//                    payment = dRows[j]["totalPayments"].ObjToDouble();
//                    contractValue = dRows[j]["contractValue"].ObjToDouble();
//                    payment = G1.RoundValue(payment);
//                    commission = G1.RoundValue(commission);
////                    contractValue = G1.RoundValue(contractValue);
//                    agentCommission += commission;
//                    agentPayments += payment;
//                    agentContracts += contractValue;
//                    totalCommission += commission;
//                    totalPayments += payment;
//                }

//                DataRow[] gRows = gDx.Select("agentCode='" + agent + "'");
//                if (gRows.Length <= 0)
//                    continue;

//                DataRow dr = dt.NewRow();
//                dr["agentNumber"] = agent;
//                fname = searchAgents.Rows[i]["firstName"].ObjToString();
//                lname = searchAgents.Rows[i]["lastName"].ObjToString();
//                name = fname + " " + lname;
//                dr["customer"] = name;
//                dr["commission"] = G1.RoundValue(agentCommission);
////                dr["paymentAmount"] = G1.RoundDown(agentPayments);
//                dr["totalPayments"] = G1.RoundValue(agentPayments);
//                dr["contractValue"] = G1.RoundValue(agentContracts);
//                splits = searchAgents.Rows[i]["splits"].ObjToString();
//                additional = searchAgents.Rows[i]["additionalGoals"].ObjToString();
//                goal = searchAgents.Rows[i]["goal"].ObjToDouble();
//                goalPercent = searchAgents.Rows[i]["goalPercent"].ObjToDouble();
//                dr["splits"] = splits;
//                dr["additionalGoals"] = additional;
//                dr["goal"] = goal;
//                dr["goalPercent"] = goalPercent;
//                dt.Rows.Add(dr);
//            }

//            DataTable extraDt = CalcAgentExtraCommission( dt );

//            //CalculateExtraCommissions(true, dt, "splits", "splitCommission");
//            //CalculateExtraCommissions(false, dt, "additionalGoals", "goalCommission");

//            double goalCommission = 0D;
//            double mainCommission = 0D;
//            totalCommission = 0D;
//            //for (int i = 0; i < dt.Rows.Count; i++)
//            //{
//            //    mainCommission = dt.Rows[i]["mainCommission"].ObjToDouble();
//            //    commission = dt.Rows[i]["splitCommission"].ObjToDouble();
//            //    goalCommission = dt.Rows[i]["goalCommission"].ObjToDouble();
//            //    mainCommission = G1.RoundValue(mainCommission);
//            //    commission = G1.RoundValue(commission);
//            //    goalCommission = G1.RoundValue(goalCommission);
//            //    mainCommission = G1.RoundValue(mainCommission);
//            //    totalCommission = goalCommission + commission + mainCommission;
//            //    dt.Rows[i]["totalCommission"] = totalCommission;
//            //}
            //G1.NumberDataTable(dt);
            //dgv.DataSource = dt;
        }
        /***********************************************************************************************/
        private void CalculateExtraCommissions ( bool dosplits, DataTable dt, string columnName, string resultColumn )
        {
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
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                goal = dt.Rows[i]["goal"].ObjToDouble();
                splits = dt.Rows[i][columnName].ObjToString();
                payment = dt.Rows[i]["totalPayments"].ObjToDouble();
                if ( !dosplits)
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
        private void chkComboAgent_EditValueChanged(object sender, EventArgs e)
        {
            string list = "";
            string[] locIDs = this.chkComboAgent.EditValue.ToString().Split('|');
            for (int i = 0; i < locIDs.Length; i++)
            {
                if (!String.IsNullOrWhiteSpace(locIDs[i]))
                    list += "'" + locIDs[i].Trim() + "',";
            }
            list = list.TrimEnd(',');
            string cmd = "";
            if ( String.IsNullOrWhiteSpace ( list ))
            {
                string columnName = "agentNumber";
                columnName = "agentCode";
                list = "";
                for (int i = 0; i < workAgents.Rows.Count; i++)
                    list += "'" + workAgents.Rows[i][columnName].ObjToString() + "',";
                list = list.TrimEnd(',');
            }

            cmd = "Select * from `agents` where `agentCode` in (" + list + ") order by `agentCode`;";
            searchAgents = G1.get_db_data(cmd);
            LoadData();
        }
        /****************************************************************************************/
        private void chkLoadAll_CheckedChanged(object sender, EventArgs e)
        {
            string cmd = "Select * from `agents` order by `agentCode`;";
            searchAgents = G1.get_db_data(cmd);
            LoadData();
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
                if ( agent != lastAgent)
                {
                    gotGoal = false;
                    gotStandard = false;
                    lastAgent = agent;
                }
                eDate = dt.Rows[i]["effectiveDate"].ObjToDateTime();
                if ( eDate > wDate )
                {
                    dt.Rows[i]["agentCode"] = "";
                    continue;
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
            string cmd = "Select * from `goals` ORDER by `agentCode`, `effectiveDate` DESC;";
            DataTable agents = G1.get_db_data(cmd);
            if (agents.Rows.Count <= 0)
                return agents;
            PreProcessGoals(agents);
            agents.Columns.Add("Formula Sales", Type.GetType("System.Double"));
            agents.Columns.Add("Location Sales", Type.GetType("System.Double"));
            agents.Columns.Add("Total Sales", Type.GetType("System.Double"));
            agents.Columns.Add("TCommission", Type.GetType("System.Double"));

            auditDt = agents.Clone();

            DataTable allAgentsDt = G1.get_db_data("Select * from `agents`;");

            DataTable dt = workTable;
            string agentCode = "";
            string formula = "";
            string agent = "";
            string type = "";
            double percent = 0D;
            double goal = 0D;
            double commission = 0D;
            double recap = 0D;
            double dbrValue = 0D;
            int count = 0;
            bool rv = false;
            int position = -1;
            int row = 0;
            DateTime eDate = DateTime.Now;
            string lastDelimiter = "";
            string parameter = "";
            string delimiter = "";
            string delimiters = @"(?<=[.,;])+->";
            string[,] calc = new string[50, 2];
            for (int i = 0; i < agents.Rows.Count; i++)
            {
                agentCode = agents.Rows[i]["agentCode"].ObjToString();
                type = agents.Rows[i]["type"].ObjToString();
                formula = agents.Rows[i]["formula"].ObjToString();
                percent = agents.Rows[i]["percent"].ObjToDouble();
                goal = agents.Rows[i]["goal"].ObjToDouble();
                eDate = agents.Rows[i]["effectiveDate"].ObjToDateTime();
                count = 0;
                if (type.ToUpper() == "STANDARD" && String.IsNullOrWhiteSpace(formula))
                {
                    row = LocateAgent(mainDt, agentCode, "agentNumber");
                    if (row >= 0)
                    {
                        commission = mainDt.Rows[row]["commission"].ObjToDouble();
                        agents.Rows[i]["TCommission"] = commission;
                        AddRowToAudit(agents, i);
                    }
                    continue;
                }
                for (;;)
                {
                    try
                    {
                        if ( String.IsNullOrWhiteSpace(formula))
                        {
                            if (goal > 0D && percent > 0D)
                                formula = agentCode;
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
                double totalSales = 0D;
                commission = 0D;
                lastDelimiter = "";
                double value = 0D;
                for (int j = 0; j < count; j++)
                {
                    try
                    {
                        parameter = calc[j, 0];
                        delimiter = calc[j, 1];
                        if (isAgent(parameter, allAgentsDt))
                        {
//                            value = GetAgentSales(agentCode, parameter, dt, eDate, ref dbrValue, ref recap, agents );
                            if (lastDelimiter == "+")
                                formulaSales += value;
                            else if (String.IsNullOrWhiteSpace(lastDelimiter))
                                formulaSales = value;
                        }
                        else
                        {
//                            value = GetLocationSales(agentCode, parameter, dt, eDate, ref dbrValue, ref recap, agents  );
                            if (lastDelimiter == "+")
                                locationSales += value;
                            else if (String.IsNullOrWhiteSpace(lastDelimiter))
                                locationSales = value;

                        }
                        lastDelimiter = delimiter;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("***ERROR*** Gathering Sales and Location Data " + ex.Message.ToString());
                    }
                }
                formulaSales = G1.RoundValue(formulaSales);
                locationSales = G1.RoundValue(locationSales);
                agents.Rows[i]["Formula Sales"] = formulaSales;
                agents.Rows[i]["Location Sales"] = locationSales;
                totalSales = formulaSales + locationSales;
                agents.Rows[i]["Total Sales"] = G1.RoundValue(totalSales);
                commission = totalSales * (percent / 100D);
                if (totalSales > goal)
                    agents.Rows[i]["TCommission"] = commission;
                AddRowToAudit(agents, i);
            }
            CalculateSplits(agents);
            if ( G1.get_column_number ( mainDt, "Formula Sales") < 0 )
            {
                mainDt.Columns.Add("Formula Sales", Type.GetType("System.Double"));
                mainDt.Columns.Add("Location Sales", Type.GetType("System.Double"));
                mainDt.Columns.Add("Total Sales", Type.GetType("System.Double"));
                mainDt.Columns.Add("TCommission", Type.GetType("System.Double"));
                mainDt.Columns.Add("ResultCommission", Type.GetType("System.Double"));
            }


            for ( int i=0; i<agents.Rows.Count; i++)
            {
                agent = agents.Rows[i]["agentCode"].ObjToString();
                row = LocateAgent(mainDt, agent, "agentNumber");
                if (row >= 0)
                {
                    AddToTable(mainDt, row, agents, i, "Formula Sales");
                    AddToTable(mainDt, row, agents, i, "Location Sales");
                    AddToTable(mainDt, row, agents, i, "Total Sales");
                    AddToTable(mainDt, row, agents, i, "TCommission");
                    AddToTable(mainDt, row, agents, i, "ResultCommission");
                    AddToTable(mainDt, row, agents, i, "ResultCommission", "totalCommission");


                    //mainDt.Rows[row]["Formula Sales"] = agents.Rows[i]["Formula Sales"].ObjToDouble();
                    //mainDt.Rows[row]["Location Sales"] = agents.Rows[i]["Location Sales"].ObjToDouble();
                    //mainDt.Rows[row]["Total Sales"] = agents.Rows[i]["Total Sales"].ObjToDouble();
                    //mainDt.Rows[row]["TCommission"] = agents.Rows[i]["TCommission"].ObjToDouble();
                    //mainDt.Rows[row]["ResultCommission"] = agents.Rows[i]["ResultCommission"].ObjToDouble();
                    //mainDt.Rows[row]["totalCommission"] = agents.Rows[i]["ResultCommission"].ObjToDouble();
                }
            }
            return mainDt;
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
        ///****************************************************************************************/
        //private double GetAgentSales(string parameter, DataTable dt, DateTime eDate)
        //{
        //    double total = 0D;
        //    double value = 0D;
        //    DateTime now = DateTime.Now;
        //    DataRow[] dRows = dt.Select("agentNumber='" + parameter + "'");
        //    for (int i = 0; i < dRows.Length; i++)
        //    {
        //        now = dRows[i]["issueDate8"].ObjToDateTime();
        //        if (now >= eDate)
        //        {
        //            value = dRows[i]["contractValue"].ObjToDouble();
        //            total += value;
        //        }
        //    }
        //    return total;
        //}
        ///****************************************************************************************/
        //private double GetLocationSales(string parameter, DataTable dt, DateTime eDate)
        //{
        //    double total = 0D;
        //    double value = 0D;
        //    DateTime now = DateTime.Now;
        //    DataRow[] dRows = dt.Select("loc='" + parameter + "'");
        //    for (int i = 0; i < dRows.Length; i++)
        //    {
        //        now = dRows[i]["issueDate8"].ObjToDateTime();
        //        if (now >= eDate)
        //        {
        //            value = dRows[i]["contractValue"].ObjToDouble();
        //            total += value;
        //        }
        //    }
        //    return total;
        //}
        /****************************************************************************************/
        private bool isAgent(string parameter, DataTable agents)
        {
            bool rv = false;
            for (int i = 0; i < agents.Rows.Count; i++)
            {
                if (parameter.ToUpper() == agents.Rows[i]["agentCode"].ObjToString().ToUpper())
                {
                    rv = true;
                    break;
                }
            }
            return rv;
        }
        /****************************************************************************************/
        private bool GetParameter(string formula, string delimiters, ref string parameter, ref string delimiter, ref int position)
        {
            bool rv = false;
            position = -1;
            parameter = "";
            delimiter = "";
            string c = "";
            for (int i = 0; i < formula.Length; i++)
            {
                c = formula.Substring(i, 1);
                if (delimiters.Contains(c))
                {
                    parameter = formula.Substring(0, i);
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
        private void CalculateSplits(DataTable dt)
        {
            double commission = 0D;
            double mainCommission = 0D;
            string type = "";
            string splits = "";
            string agent = "";
            string str = "";
            double percent = 0D;
            double payment = 0D;
            double amount = 0D;
            double totalAmount = 0D;
            int row = 0;
            if (G1.get_column_number(dt, "ResultCommission") < 0)
                dt.Columns.Add("ResultCommission", Type.GetType("System.Double"));

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                commission = dt.Rows[i]["TCommission"].ObjToDouble();
                if (commission <= 0D)
                    continue;
                type = dt.Rows[i]["type"].ObjToString();
                splits = dt.Rows[i]["splits"].ObjToString();
                if (String.IsNullOrWhiteSpace(splits))
                {
                    amount = dt.Rows[i]["ResultCommission"].ObjToDouble();
                    amount += commission;
                    dt.Rows[i]["ResultCommission"] = amount;
                    continue;
                }
                if (splits.IndexOf("~") >= 0)
                {
                    payment = commission;
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
                                if ( type.ToUpper() == "GOAL")
                                    percent = str.ObjToDouble();
                                else
                                    percent = str.ObjToDouble() / 100D;
                                commission = payment * percent;
                                commission = G1.RoundDown(commission);
                                row = LocateAgent(dt, agent, "agentCode");
                                if (row >= 0)
                                {
                                    amount = dt.Rows[row]["ResultCommission"].ObjToDouble();
                                    amount += commission;
                                    dt.Rows[row]["ResultCommission"] = amount;
                                    totalAmount += amount;
                                }
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                    //if (dosplits)
                    //{
                    //    dt.Rows[i]["mainCommission"] = 0D;
                    //    if (totalAmount != baseCommission)
                    //    {
                    //        diff = baseCommission - totalAmount;
                    //        //                            diff = G1.RoundValue(diff);
                    //        amount = dt.Rows[i][resultColumn].ObjToDouble();
                    //        amount += diff;
                    //        dt.Rows[i][resultColumn] = amount;
                    //    }
                    //}
                }
            }
        }
        /***********************************************************************************************/
        private void CalculateSplitCommissions(bool dosplits, DataTable dt, string columnName, string resultColumn)
        {
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
        }
        /****************************************************************************************/
    }
}