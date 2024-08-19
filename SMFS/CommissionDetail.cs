using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraPrinting;
using DevExpress.Utils;
using GeneralLib;
using System.Drawing.Printing;
using DevExpress.XtraPrintingLinks;
using DevExpress.XtraGrid.Views.BandedGrid;
using System.IO;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class CommissionDetail : DevExpress.XtraEditors.XtraForm
    {
        private DateTime workStart;
        private DateTime workStop;
        private string workAgent = "";
        private DataTable workDt;
        private DataTable workDt8;
        private DataTable workDt9;
        private DataTable workDt10;
        private DataTable searchAgents;
        private string agentNumber = "";
        private string workAgentNumber = "";
        private bool doSplitBase = false;
        private bool doSplitGoal = false;
        private bool loading = true;
        private bool workPrintDetail = false;
        private bool firstPrint = true;
        private string workPrinterName = "";
        private bool PrintPreview = false;
        private string workShow = "";
        private string workStatus = "";
        private bool workingAll = false;
        private bool continuousPrint = false;
        private string fullPath = "";

        public static string _Commissions_Printed = "";
        private bool disallowSave = false;
        public string Printed { get { return _Commissions_Printed; } }

        /****************************************************************************************/
        public CommissionDetail( DateTime start, DateTime stop, string agent, string agentName, DataTable dt, DataTable dt8, DataTable dt9, DataTable dt10, bool PrintDetail = false, string cmbShow = "", string cmbStatus = "" )
        {
            InitializeComponent();
            workStart = start;
            workStop = stop;
            agentNumber = agent;
            workAgent = agentName;
            workDt = dt;
            workDt8 = dt8;
            workDt9 = dt9;
            workDt10 = dt10;
            chkChronological.Hide();
            workPrintDetail = PrintDetail;
            workShow = cmbShow;
            cmbShowWhat.Text = workShow;
            workStatus = cmbStatus;
            cmbStatusWhat.Text = cmbStatus;
            workingAll = false;
        }
        /****************************************************************************************/
        private void CommissionDetail_Load(object sender, EventArgs e)
        {
            workingAll = false;

            _Commissions_Printed = "";
            disallowSave = false;

            string cmd = "Select * from `goals` where `agentCode` = '" + agentNumber + "' ORDER by `effectiveDate`;";
            searchAgents = G1.get_db_data(cmd);
            if (!workPrintDetail)
            {
                string agentStatus = "";
                LoadData ( ref agentStatus );
                menuStrip1.Items["reportsToolStripMenuItem"].Enabled = false;
                disallowSave = true;
            }
            else
            {
                loading = true;
                chkIncludeDetails.Checked = true;
                loading = false;

                cmd = "Select * from `goals` ORDER by `effectiveDate`;";
                searchAgents = G1.get_db_data(cmd);

                PleaseWait pleaseWait = new PleaseWait("Please Wait\nPreparing Commissions!");
                pleaseWait.Show();
                pleaseWait.Refresh();

                LoadAllData();

                pleaseWait.FireEvent1();
                pleaseWait.Dispose();
                pleaseWait = null;
            }
//            else
//            {

//                string printerName = String.Empty;
//                string printers = "Print Preview\n";
//                for (int i = 0; i < PrinterSettings.InstalledPrinters.Count; i++)
//                {
//                    string pName = PrinterSettings.InstalledPrinters[i];
//                    printers += pName + "\n";
//                }
//                printers = printers.TrimEnd('\n');
//                using (ListSelect listForm = new ListSelect(printers, false))
//                {
//                    listForm.Text = "Select Printer";
//                    listForm.ShowDialog();
//                    workPrinterName = ListSelect.list_detail.Trim();
//                    if (String.IsNullOrWhiteSpace(workPrinterName))
//                    {
//                        this.Close();
//                        return;
//                    }
//                }
//                if (workPrinterName.ToUpper() == "PRINT PREVIEW")
//                    PrintPreview = true;
//                chkIncludeDetails.Checked = true;
//                loading = false;
//                int lastRow = workDt10.Rows.Count;
////                lastRow = 3;
//                firstPrint = true;
//                if (PrintPreview)
//                {
//                    printContinuousPreviewTool_Click(null, null);
//                }
//                else
//                {
//                    for (int i = 0; i < lastRow; i++)
//                    {
//                        workAgent = workDt10.Rows[i]["customer"].ObjToString();
//                        LoadData();
//                    }
//                }
//            }
        }
        /****************************************************************************************/
        private void LoadAllData ()
        {
            int lastRow = workDt10.Rows.Count;
            DataTable myDataTable = null;
            DataTable agentDataTable = null;
            string agentStatus = "";

            workingAll = true;
            string oldAgent = "";

            string workingShow = cmbShowWhat.Text.Trim().ToUpper();
            string workingStatus = cmbStatusWhat.Text.Trim().ToUpper();
            string status = "";

            workStatus = cmbStatusWhat.Text.Trim().ToUpper();

            for (int i = 0; i < lastRow; i++)
            {
                workAgent = workDt10.Rows[i]["customer"].ObjToString();
                if (oldAgent == workAgent)
                    continue;
                oldAgent = workAgent;
                workAgentNumber = workDt10.Rows[i]["agentCode"].ObjToString();

                status = workDt10.Rows[i]["activeStatus"].ObjToString();
                if (workingStatus == "GONE" && status.ToUpper() != "GONE")
                    continue;
                if (workingStatus == "ALL" && status.ToUpper() == "GONE")
                    continue;
                //else if (status.ToUpper() == "GONE")
                //    continue;
                //if ( workingStatus.ToUpper() == "GONE")
                //{
                //    if (status != "GONE")
                //        continue;
                //}
                //else
                //{
                //    if (status == "GONE")
                //        continue;
                //}

                agentDataTable = LoadData ( ref agentStatus );

                if ( agentDataTable == null )
                {
                    if (workStatus != "ALL")
                        continue;
                    //if (agentStatus.ToUpper() == "GONE")
                    //    continue;
                }

                if (myDataTable == null)
                {
                    if (agentDataTable == null)
                        continue;
                    myDataTable = agentDataTable.Clone();
                }
                if ( status.ToUpper() == "INACTIVE" )
                {
                    if (workStatus == "ALL")
                        continue;
                }
                DataRow dR = myDataTable.NewRow();
                if ( i > 0 )
                    myDataTable.Rows.Add(dR);
                dR = myDataTable.NewRow();
                dR["desc"] = workAgent;
                //if ( i> 0)
                    dR["detail"] = agentStatus;
                dR["help"] = workStart.ToString("MM/dd/yyyy") + " to " + workStop.ToString("MM/dd/yyyy");

                myDataTable.Rows.Add(dR);

                if (agentDataTable != null)
                {
                    for (int k = 0; k < agentDataTable.Rows.Count; k++)
                        myDataTable.ImportRow(agentDataTable.Rows[k]);
                }
            }
            dgv.DataSource = myDataTable;
            this.Text = "Commission Details for All Agents";
            gridBand2.Caption = workStart.ToString("MM/dd/yyyy") + " to " + workStop.ToString("MM/dd/yyyy");
        }
        /****************************************************************************************/
        private DataTable LoadData( ref string agentStatus )
        {
            this.Text = "Details for " + workAgent + " for " + workStart.ToString("MM/dd/yyyy") + " to " + workStop.ToString("MM/dd/yyyy");
            gridBand2.Caption = this.Text;
            double totalContracts = 0D;
            double contractValue = 0D;
            double totalDBR = 0D;
            double downPayment = 0D;
            double payments = 0D;
            double totalPayments = 0D;
            double value = 0D;
            string str = "";
            DateTime issueDate;
            string issueDateStr = "";
            if ( workAgent == "Keith Shelby")
            {
            }
            DataRow[] dRows = workDt.Select("agentName='" + workAgent + "'");
            DataTable dt = workDt.Clone();
            G1.ConvertToTable(dRows, dt);

            dRows = workDt8.Select("agentName='" + workAgent + "'");
            DataTable dt8 = workDt8.Clone();
            G1.ConvertToTable(dRows, dt8);

            dRows = workDt9.Select("agentName='" + workAgent + "'");
            DataTable dt9 = workDt9.Clone();
            G1.ConvertToTable(dRows, dt9);

            dRows = workDt10.Select("customer='" + workAgent + "'");
            DataTable dt10 = workDt10.Clone();
            G1.ConvertToTable(dRows, dt10);
            if (dt10.Rows.Count <= 0)
                return null;

            string agentFirstName = dt10.Rows[0]["firstName"].ObjToString();
            string agentLastName = dt10.Rows[0]["lastName"].ObjToString();
            DataTable mDt = null;
            DateTime effectiveToDate = DateTime.Now;
            DateTime effectiveFromDate = DateTime.Now;
            double commissionPercent = 0D;
            double splitCommissionPercent = 0D;
            double dValue = 0D;

            DataTable bothDt = dt8.Copy();
            if ( bothDt.Rows.Count > 0 )
            {
            }
            bothDt.Columns.Add("Where");
            bothDt.Columns.Add("Done");
            for (int i = 0; i < bothDt.Rows.Count; i++)
                bothDt.Rows[i]["Where"] = "8";

            if (G1.get_column_number(dt9, "Where") < 0)
                dt9.Columns.Add("Where");
            if (G1.get_column_number(dt9, "Done") < 0)
                dt9.Columns.Add("Done");
            bothDt.Merge(dt9, true, MissingSchemaAction.Ignore);
            //for (int i = 0; i < dt9.Rows.Count; i++)
            //    bothDt.ImportRow(dt9.Rows[i]);

            DataView tempview = bothDt.DefaultView;
            tempview.Sort = "YearMonth asc,ContractNumber asc, Where asc";
            bothDt = tempview.ToTable();
            if (bothDt.Rows.Count == 0)
            {
                bothDt = determineFormulaRecapReins(workDt8, workDt9, dt10);
                DataRow[] dRs = bothDt.Select("where = '8'");
                G1.ConvertToTable(dRs, dt8);
                dRs = bothDt.Select("where = '9'");
                G1.ConvertToTable(dRs, dt9);
            }

            string oldInfo = "";
            string newInfo = "";
            string contract = "";
            string oldContract = "";

            string comboWhat = cmbShowWhat.Text.ToUpper();

            if (!chkShowAll.Checked)
            {
                if (!chkIncludeDetails.Checked)
                {
                    for (int i = 0; i < bothDt.Rows.Count; i++)
                    {
                        newInfo = bothDt.Rows[i]["YearMonth"].ObjToString();
                        contract = bothDt.Rows[i]["contractNumber"].ObjToString();
                        if (newInfo == oldInfo && contract == oldContract)
                        {
                            bothDt.Rows[i]["contractNumber"] = "REMOVE";
                            bothDt.Rows[i - 1]["contractNumber"] = "REMOVE";
                        }
                        oldInfo = newInfo;
                        oldContract = contract;
                    }
                }
            }

            string cmd = "";
            agentStatus = "";

            workStatus = cmbStatusWhat.Text.Trim().ToUpper();
            //if ( workStatus != "ALL" && workingAll )
            //{
                cmd = "Select * from `agents` WHERE `agentCode` = '" + workAgentNumber + "';";
                DataTable agentDt = G1.get_db_data(cmd);
                if (agentDt.Rows.Count > 0)
                    agentStatus = agentDt.Rows[0]["activeStatus"].ObjToString();
            //}

            if (bothDt.Rows.Count > 0)
            {
                for (int i = bothDt.Rows.Count - 1; i >= 0; i--)
                {
                    contract = bothDt.Rows[i]["contractNumber"].ObjToString();
                    if (contract == "REMOVE")
                        bothDt.Rows.RemoveAt(i);
                }
            }


            dRows = dt.Select("DBR ='DBR'");
            DataTable dtDBR = workDt.Clone();
            G1.ConvertToTable(dRows, dtDBR);

            dRows = dt.Select("FBI ='1'");
            DataTable dtFBI = workDt.Clone();
            G1.ConvertToTable(dRows, dtFBI);

            dRows = workDt.Select("meetingNumber <> '' AND meetingNumber <> '0'");
            DataTable meetingDt = workDt.Clone();
            G1.ConvertToTable(dRows, meetingDt);

            cmd = "Select * from `agent_meetings` where `effectiveFromDate` >= '" + workStart.ToString("yyyy-MM-dd") + "' and '" + workStop.ToString("yyyy-MM-dd") + "' <= `effectiveToDate`;";
            DataTable meetDt = G1.get_db_data(cmd);

            doSplitBase = false;
            if (G1.get_column_number(workDt10, "splitBaseCommission") >= 0)
                doSplitBase = true;
            doSplitGoal = false;
            if (G1.get_column_number(workDt10, "splitGoalCommission") >= 0)
                doSplitGoal = true;

            totalContracts = 0D;
            totalDBR = 0D;
            double formulaSales = 0D;
            double totalDBCMoney = 0D;
            double dbcMoney = 0D;
            double totalRecap = 0D;
            double totalReins = 0D;
            double totalRecapContracts = 0D;
            double totalBaseCommission = 0D;
            double totalContractCommission = 0D;
            double baseCommission = 0D;
            double goalCommission = 0D;
            double totalGoalCommission = 0D;
            double splitBaseCommission = 0D;
            double splitGoalCommission = 0D;
            double MC = 0D;
            double MR = 0D;
            double totalCommission = 0D;

            double pastFailures = 0D;
            double totalPastFailures = 0D;
            string lapseContracts = "";
            string reinContracts = "";
            string contractNumber = "";
            DataTable oldLapseDt = null;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                if (contractNumber == "HT19004LI")
                {
                }
                contractValue = dt.Rows[i]["contractValue"].ObjToDouble();
                downPayment = dt.Rows[i]["downPayment"].ObjToDouble();
                if (downPayment > 0D)
                    totalContracts += contractValue;
                str = dt.Rows[i]["DBR"].ObjToString();
                if (str.ToUpper() == "DBR")
                    totalDBR += contractValue;
                value = dt.Rows[i]["recap"].ObjToDouble();
                if (value > 0D)
                {
                    totalRecap += value;
                    totalRecapContracts += dt.Rows[i]["recapContracts"].ObjToDouble();
                    lapseContracts += contractNumber + ",";
                }
                value = dt.Rows[i]["reins"].ObjToDouble();
                if (value > 0D)
                {
                    totalReins += value;
                    reinContracts += contractNumber + ",";
                }
            }

            totalPayments = 0D;
            totalContracts = 0D;
            formulaSales = 0D;
            totalRecap = 0D;
            totalReins = 0D;
            totalCommission = 0D;
            double totalSales = 0D;
            double totalDBC = 0D;
            string standardFormula = "";

            bool gotStandard = false;
            if ( G1.get_column_number(dt10, "standardFormula") >= 0 )
                gotStandard = true;

            double tBaseCommission = 0D;
            double sBaseCommission = 0D;
            for (int i = 0; i < dt10.Rows.Count; i++)
            {
                if (gotStandard)
                {
                    if (!String.IsNullOrWhiteSpace(dt10.Rows[i]["standardFormula"].ObjToString()))
                        standardFormula = dt10.Rows[i]["standardFormula"].ObjToString();
                }
                totalPayments += dt10.Rows[i]["totalPayments"].ObjToDouble();
                totalContracts += dt10.Rows[i]["totalContracts"].ObjToDouble();
                formulaSales += dt10.Rows[i]["Formula Sales"].ObjToDouble();
                totalReins += dt10.Rows[i]["reins"].ObjToDouble();
                totalRecap += dt10.Rows[i]["recap"].ObjToDouble();
                totalCommission += dt10.Rows[i]["totalCommission"].ObjToDouble();
                tBaseCommission = dt10.Rows[i]["commission"].ObjToDouble();
                sBaseCommission = dt10.Rows[i]["splitBaseCommission"].ObjToDouble();
                if (doSplitBase)
                {
                    if (sBaseCommission > 0D)
                    {
                        splitBaseCommission += sBaseCommission;
                        tBaseCommission = 0D;
                    }
                }
                if (doSplitGoal)
                    splitGoalCommission += dt10.Rows[i]["splitGoalCommission"].ObjToDouble();
                totalGoalCommission += dt10.Rows[i]["goalCommission"].ObjToDouble();
                totalPastFailures += dt10.Rows[i]["pastFailures"].ObjToDouble();
                totalBaseCommission += tBaseCommission;
                totalContractCommission += dt10.Rows[i]["contractCommission"].ObjToDouble();
                totalDBC += dt10.Rows[i]["dbrValue"].ObjToDouble();
                totalDBCMoney += dt10.Rows[i]["dbcMoney"].ObjToDouble();
                MC += dt10.Rows[i]["MC"].ObjToDouble();
                MR += dt10.Rows[i]["MR"].ObjToDouble();
            }

            //if ( doSplitBase )
            //{
            //    if ( splitBaseCommission > 0D )
            //    {
            //        totalBaseCommission = totalCommission - splitBaseCommission - totalContractCommission;
            //        //totalBaseCommission = G1.RoundValue(totalBaseCommission);
            //    }
            //}

            double goalSales = 0D;
            double goalReins = 0D;
            double goalLapses = 0D;

            if (totalGoalCommission > 0D)
            {
                string formula = "";
                for (int i = 0; i < dt10.Rows.Count; i++)
                {
                    formula = dt10.Rows[i]["formula"].ObjToString();
                    value = dt10.Rows[i]["location Sales"].ObjToDouble();
                    value = dt10.Rows[i]["goal"].ObjToDouble();
                    goalCommission = dt10.Rows[i]["goalCommission"].ObjToDouble();
                    value = dt10.Rows[i]["reins"].ObjToDouble();
                    value = dt10.Rows[i]["goalCommission"].ObjToDouble();
                    value = dt10.Rows[i]["commission"].ObjToDouble();
                    if (goalCommission > 0D)
                        goalSales += dt10.Rows[i]["location Sales"].ObjToDouble();
                    goalReins += dt10.Rows[i]["reins"].ObjToDouble();
                    goalLapses += dt10.Rows[i]["recap"].ObjToDouble();

                    //if (value > 0D)
                    //    AddRow(dgvDt, "Base Commission", value, "5% Commission on Payments");
                }
            }

            bool got5Percent = true;
            if (totalBaseCommission == 0D)
            {
                if (splitBaseCommission == 0D)
                    got5Percent = false;
            }
            bool got1Percent = true;
            double totalC = splitGoalCommission + totalGoalCommission - totalPastFailures + totalReins - totalRecap;
            if (totalC == 0D)
                got1Percent = false;

            if (workStatus != "ALL")
            {
                if (agentStatus.ToUpper() == "GONE" && workStatus.ToUpper() != "GONE" )
                    return null;

                if (comboWhat == "5%")
                {
                    if (workStatus.ToUpper() == "INACTIVE" && agentStatus.ToUpper() != "INACTIVE")
                        return null;

                    if (totalBaseCommission == 0D)
                    {
                        if (splitBaseCommission == 0D)
                            return null;
                    }
                }
                else if (comboWhat == "1%")
                {
                    if (workStatus.ToUpper() == "INACTIVE" && agentStatus.ToUpper() != "INACTIVE")
                        return null;

                    totalC = splitGoalCommission + totalGoalCommission - totalPastFailures + totalReins - totalRecap;
                    if (totalC == 0D)
                        return null;
                }
                else
                {
                    if (workStatus.ToUpper() == "INACTIVE" && agentStatus.ToUpper() != "INACTIVE")
                        return null;

                    totalC = splitGoalCommission + totalGoalCommission - totalPastFailures + totalReins - totalRecap;
                    if (totalBaseCommission == 0D && totalC == 0D)
                        return null;
                }
            }
            else // Showing All
            {
                if ( agentStatus.ToUpper() == "INACTIVE" )
                {
                }
                if (agentStatus.ToUpper() == "GONE")
                    return null;
                if (comboWhat == "5%")
                {
                    if ( agentStatus.ToUpper() == "INACTIVE" && !got5Percent )
                        return null;
                }
                else if (comboWhat == "1%")
                {
                    if (agentStatus.ToUpper() == "INACTIVE" && !got1Percent)
                        return null;
                }
                else
                {
                    if (agentStatus.ToUpper() == "INACTIVE" && !got1Percent && !got5Percent )
                        return null;
                }
            }

            DataTable dgvDt = new DataTable();
            dgvDt.Columns.Add("desc");
            dgvDt.Columns.Add("detail");
            dgvDt.Columns.Add("help");

            DateTime dates;

            AddRow(dgvDt, "Total Payments", totalPayments, standardFormula );
            AddRow(dgvDt, "", "");
            AddRow(dgvDt, "Total Contracts", formulaSales);
            if (goalSales > 0D)
            {
                AddRow(dgvDt, "   Goal Sales", goalSales, "Contract Sales that made Goal!");
                AddRow(dgvDt, "   Goal Lapses", goalLapses, "Contract Lapses to subtract from Commission!");
                AddRow(dgvDt, "   Goal Reinstates", goalReins, "Contract Reinstates to add back to Commission!");
            }
            if (chkIncludeDetails.Checked)
            {
                for (int i = 0; i < dtDBR.Rows.Count; i++)
                {
                    contractNumber = dtDBR.Rows[i]["contractNumber"].ObjToString();
                    AddRow(dgvDt, "   DBC Contract", contractNumber);
                    contractValue = dtDBR.Rows[i]["contractValue"].ObjToDouble();
                    AddRow(dgvDt, "   DBC Contract Value", contractValue);
                }
            }
            AddRow(dgvDt, "Total DBC's", totalDBC);
            AddRow(dgvDt, "", "================");
            value = formulaSales - totalDBC;
            if (formulaSales <= 0D)
            {
                //                value = goalSales - goalReins; // Remove 4/16/2019 15:29
                value = goalSales - totalDBC;
            }
            AddRow(dgvDt, "Total Sales", value);
            if (value > 0D && totalGoalCommission <= 0D)
                AddRow(dgvDt, "   Goal Commission", totalGoalCommission, "Agent didn't make Goal!");
            else
                AddRow(dgvDt, "   Goal Commission", totalGoalCommission);

            double agentLocationSales = 0D;
            double agentTotalSales = 0D;
            double agentTotalCommissions = 0D;
            string agentSplits = "";
            double agentSplitBaseCommission = 0D;
            double agentPercent = 0D;
            string agentName = "";
            if (totalGoalCommission > 0D)
            {
                AddRow(dgvDt, "", "");
                string formula = "";
                for (int i = 0; i < dt10.Rows.Count; i++)
                {
                    agentSplits = dt10.Rows[i]["splits"].ObjToString();
                    agentLocationSales = 0D;
                    agentTotalSales = 0D;
                    agentTotalCommissions = 0D;
                    agentSplitBaseCommission = 0D;
                    formula = dt10.Rows[i]["formula"].ObjToString();
                    if (String.IsNullOrWhiteSpace(formula))
                        formula = dt10.Rows[i]["agentCode"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(formula))
                        AddRow(dgvDt, "Goal Formula", formula);
                    if (comboWhat == "ALL" || comboWhat == "5%")
                    {
                        value = dt10.Rows[i]["location Sales"].ObjToDouble();
                        if (value == 0D && !String.IsNullOrWhiteSpace(agentSplits))
                        {
                            agentSplitBaseCommission = dt10.Rows[i]["splitBaseCommission"].ObjToDouble();
                            agentTotalSales = dt10.Rows[i]["Total Sales"].ObjToDouble();
                            agentTotalCommissions = dt10.Rows[i]["TCommission"].ObjToDouble();
                            if (agentTotalSales > 0D && agentTotalCommissions > 0D)
                            {
                                agentPercent = agentSplitBaseCommission / agentTotalCommissions;
                                agentPercent = G1.RoundValue(agentPercent);
                                value = agentPercent * agentTotalSales;
                            }
                        }
                        AddRow(dgvDt, "Location Sales", value);
                    }
                    else if (comboWhat == "ALL" || comboWhat == "1%")
                    {
                        value = dt10.Rows[i]["goal"].ObjToDouble();
                        AddRow(dgvDt, "Current Goal", value);
                        goalCommission = dt10.Rows[i]["goalCommission"].ObjToDouble();
                        AddRow(dgvDt, "Goal Commission", goalCommission, "Commission before Reductions");
                        value = dt10.Rows[i]["reins"].ObjToDouble();
                        AddRow(dgvDt, "Reinstates", value);
                        value = dt10.Rows[i]["recap"].ObjToDouble();
                        value = value * -1D;
                        AddRow(dgvDt, "Lapse Recaps", value);
                        value = dt10.Rows[i]["goalCommission"].ObjToDouble();
                        AddRow(dgvDt, "Goal Commission", value);
                    }
                    if (comboWhat == "ALL" || comboWhat == "5%")
                    {
                        value = dt10.Rows[i]["commission"].ObjToDouble();
                        if (value > 0D)
                            AddRow(dgvDt, "Base Commission", value, "5% Commission on Payments");
                        else
                        {
                            value = dt10.Rows[i]["splitBaseCommission"].ObjToDouble();
                            if (value > 0D)
                                AddRow(dgvDt, "Split Base Commission", value, "5% Commission on Payments");
                        }
                    }
                    AddRow(dgvDt, "", "");
                }
            }

            if (totalPastFailures > 0D && chkIncludeDetails.Checked)
            {
                AddRow(dgvDt, "", "");
                for (int i = 0; i < dt10.Rows.Count; i++)
                {
                    pastFailures = dt10.Rows[i]["pastFailures"].ObjToDouble();
                    if (pastFailures > 0D)
                    {
                        str = dt10.Rows[i]["agentCode"].ObjToString();
                        AddRow(dgvDt, "  (" + (i + 1).ToString() + ") Amount Due from Past Negatives (" + str + ")", pastFailures, "Failures before Reductions");
                    }
                }
            }
            double fbi = 0;
            for (int i = 0; i < dt10.Rows.Count; i++)
                fbi += dt10.Rows[i]["fbi$"].ObjToDouble();
            if (fbi > 0D)
            {
                if (chkIncludeDetails.Checked)
                {
                    AddRow(dgvDt, "", "");
                    for (int i = 0; i < dtFBI.Rows.Count; i++)
                    {
                        contractNumber = dtFBI.Rows[i]["contractNumber"].ObjToString();
                        AddRow(dgvDt, "  (" + (i + 1).ToString() + ") FBI CONTRACT", contractNumber, "Funded By Insurance");
                    }
                }
            }

            DataRow[] dRR = null;

            if (comboWhat == "ALL" || comboWhat == "1%")
            {
                if (bothDt.Rows.Count > 0)
                {
                    if (chkIncludeDetails.Checked)
                    {
                        double goal = 0D;
                        double pastContractValue = 0D;
                        double newContractResults = 0D;
                        double totalRecapHere = 0D;
                        double recaptureAmount = 0D;
                        double percent = 0D;
                        int rowCount = 0;
                        bool good = false;

                        string where = "";
                        lapseContracts.TrimEnd(',');
                        AddRow(dgvDt, "", "");
                        string lastYearMonth = "";
                        double lastcontractResult = 0D;
                        string[] Lines = lapseContracts.Split(',');
                        for (int i = 0; i < bothDt.Rows.Count; i++)
                        {
                            contractNumber = bothDt.Rows[i]["contractNumber"].ObjToString();
                            if (String.IsNullOrWhiteSpace(contractNumber))
                                continue;
                            if (contractNumber == "E17069LI")
                            {

                            }
                            where = bothDt.Rows[i]["where"].ObjToString();
                            DataRow[] dR = null;
                            if (where == "8")
                            {
                                dR = dt8.Select("contractNumber = '" + contractNumber + "'");
                                issueDate = dR[0]["issueDate8"].ObjToDateTime();
                                good = qualifyReinstate(issueDate);
                                if (!good)
                                    continue;
                            }
                            else
                            {
                                dR = dt9.Select("contractNumber = '" + contractNumber + "'");
                                issueDate = dR[0]["issueDate8"].ObjToDateTime();
                                good = qualifyReinstate(issueDate);
                                if (!good)
                                    continue;
                            }
                            string yearMonth = dR[0]["YearMonth"].ObjToString();
                            string lapseName = "";
                            string phone = "";
                            getContractDetails(contractNumber, ref lapseName, ref phone);
                            AddRow(dgvDt, "", "");
                            if (where == "8")
                                AddRow(dgvDt, "  (" + (i + 1).ToString() + ") LAPSE CONTRACT", contractNumber, lapseName + " Phone : " + phone);
                            else
                                AddRow(dgvDt, "  (" + (i + 1).ToString() + ") REINSTATE CONTRACT", contractNumber, lapseName + " Phone : " + phone);
                            rowCount++;
                            if (dR.Length > 0)
                            {
                                agentName = dR[0]["agentName"].ObjToString();
                                dRR = workDt.Select("contractNumber='" + contractNumber + "'");
                                if ( dRR.Length > 0 )
                                {
                                    if ( contractNumber == "L21021LI")
                                    {
                                    }
                                    agentName = dRR[0]["agentName"].ObjToString();
                                }
                                issueDate = dR[0]["issueDate8"].ObjToDateTime();
                                issueDateStr = issueDate.ToString("MM/dd/yyyy");
                                AddRow(dgvDt, "    Issue Date", issueDateStr, "Agent: " + agentName );
                                if (where == "8")
                                {
                                    dates = dR[0]["lapseDate8"].ObjToDateTime();
                                    str = dates.ToString("MM/dd/yyyy");
                                    AddRow(dgvDt, "    Lapse Date", str);
                                    cmd = "SELECT * from `contracts` where `contractNumber` = '" + contractNumber + "';";
                                    oldLapseDt = G1.get_db_data(cmd);
                                    if (oldLapseDt.Rows.Count > 0)
                                    {
                                        dates = oldLapseDt.Rows[0]["dueDate8"].ObjToDateTime();
                                        str = dates.ToString("MM/dd/yyyy");
                                        AddRow(dgvDt, "    Due Date", str);
                                    }
                                }
                                else
                                {
                                    dates = dR[0]["reinstateDate8"].ObjToDateTime();
                                    str = dates.ToString("MM/dd/yyyy");
                                    AddRow(dgvDt, "    Reinstate Date", str);
                                    cmd = "SELECT * from `contracts` where `contractNumber` = '" + contractNumber + "';";
                                    oldLapseDt = G1.get_db_data(cmd);
                                    if (oldLapseDt.Rows.Count > 0)
                                    {
                                        dates = oldLapseDt.Rows[0]["dueDate8"].ObjToDateTime();
                                        str = dates.ToString("MM/dd/yyyy");
                                        AddRow(dgvDt, "    Due Date", str);
                                    }
                                }
                                value = dR[0]["totalContracts"].ObjToDouble();
                                if (yearMonth == lastYearMonth)
                                {
                                    value = lastcontractResult;
                                    dR[0]["lapseRecaps"] = 0D;
                                    dR[0]["dbrSales"] = 0D;
                                }
                                else
                                    lastYearMonth = yearMonth;
                                double lapseTotalContracts = value;
                                AddRow(dgvDt, "     " + yearMonth + " Contracts", value, "Contracts Values for " + issueDateStr);
                                double lapseRecaps = dR[0]["lapseRecaps"].ObjToDouble();
                                AddRow(dgvDt, "     " + yearMonth + " Previous Lapses", lapseRecaps, "Previous Lapse Values for " + issueDateStr);
                                value = dR[0]["dbrSales"].ObjToDouble();
                                double lapseDBRs = value;
                                AddRow(dgvDt, "     " + yearMonth + " DBR's", value, "DBR's for " + issueDateStr);
                                AddRow(dgvDt, "", "================");
                                value = lapseTotalContracts - lapseDBRs - lapseRecaps;
                                pastContractValue = value;
                                AddRow(dgvDt, "     " + yearMonth + " Contract Results Now", value, "Results");
                                AddRow(dgvDt, "", "");


                                value = dR[0]["contractValue"].ObjToDouble();
                                double lapseContractValue = value;
                                AddRow(dgvDt, "     " + yearMonth + " Contract Value", value);
                                if (where == "8")
                                {
                                    newContractResults = pastContractValue - lapseContractValue;
                                    value = value - lapseContractValue;
                                    recaptureAmount = 0D - lapseContractValue;
                                }
                                else
                                {
                                    DataTable ddd = bothDt.Clone();
                                    G1.ConvertToTable(dR, ddd);
                                    newContractResults = pastContractValue + lapseContractValue;
                                    value = value + lapseContractValue;
                                    recaptureAmount = lapseContractValue;
                                }
                                AddRow(dgvDt, "     " + yearMonth + " New Contracts Results", newContractResults);
                                lastcontractResult = newContractResults;

                                goal = dR[0]["goal"].ObjToDouble();
                                AddRow(dgvDt, "     " + yearMonth + " Goal", goal, "Goal Objective");

                                percent = dR[0]["percent"].ObjToDouble();
                                if (where == "8") // Lapse Recap
                                {
                                    if (pastContractValue < goal)
                                        recaptureAmount = 0D;
                                    else if ((pastContractValue - lapseContractValue) < goal)
                                    { // Lapse Caused RePay of total bonus Back to Company
                                        recaptureAmount = pastContractValue * percent;
                                        //                                    recaptureAmount = recaptureAmount * 100D;
                                    }
                                    else if (pastContractValue > goal)
                                        recaptureAmount = lapseContractValue * percent;
                                    recaptureAmount = G1.RoundValue(recaptureAmount);
                                    recaptureAmount = 0D - recaptureAmount;
                                }
                                else // Reinstate
                                {
                                    if (pastContractValue > goal)
                                        recaptureAmount = lapseContractValue * percent;
                                    else if ((pastContractValue + lapseContractValue) >= goal)
                                    { // Reinstate Caused RePay of total bonus
                                        recaptureAmount = (pastContractValue + lapseContractValue) * percent;
                                        //                                    recaptureAmount = recaptureAmount * 100D;
                                    }
                                    else
                                        recaptureAmount = 0D;
                                    recaptureAmount = G1.RoundValue(recaptureAmount);
                                }
                                value = dR[0]["Recap"].ObjToDouble();
                                value = percent * recaptureAmount;
                                totalRecapHere += recaptureAmount;
                                if (where == "8")
                                    AddRow(dgvDt, "     " + yearMonth + " Lapse Recap", recaptureAmount, "Lapse Recapture Amount");
                                else
                                    AddRow(dgvDt, "     " + yearMonth + " Reinstate", recaptureAmount, "Reinstated Amount");
                            }
                        }
                        if (!chkChronological.Checked)
                            RebuildDataTable(dgvDt);
                        if (totalRecapHere > 0D)
                        {
                            AddRow(dgvDt, "", "");
                            AddRow(dgvDt, "     Recapure Amount", totalRecapHere, "TotalRecapture Amount");
                            AddRow(dgvDt, "", "");
                        }
                    }
                }
                if (chkIncludeDetails.Checked && meetingDt.Rows.Count > 0)
                {
                    bool firstMeeting = true;
                    DataRow [] dR = null;
                    int meetingCount = 0;
                    for (int i = 0; i < meetingDt.Rows.Count; i++)
                    {
                        contractNumber = meetingDt.Rows[i]["contractNumber"].ObjToString();
                        if (String.IsNullOrWhiteSpace(contractNumber))
                            continue;
                        string customerName = "";
                        string phone = "";
                        getContractDetails(contractNumber, ref customerName, ref phone);
                        dR = workDt.Select("contractNumber = '" + contractNumber + "'");
                        string meetingNumber = dR[0]["meetingNumber"].ObjToString();
                        if (dR[0]["firstName1"].ObjToString().ToUpper() != agentFirstName.ToUpper())
                            continue;
                        if (dR[0]["lastName1"].ObjToString().ToUpper() != agentLastName.ToUpper())
                            continue;

                        cmd = "Select * from `agent_meetings` WHERE `meetingNumber`='" + meetingNumber + "';";
                        mDt = G1.get_db_data(cmd);
                        if (mDt.Rows.Count <= 0)
                            continue;
                        if ( firstMeeting )
                        {
                            AddRow(dgvDt, "", "");
                            AddRow(dgvDt, "  *************************" + "*************", "************************", "***************************");
                            firstMeeting = false;
                        }

                        if ( meetingCount >= 1 )
                            AddRow(dgvDt, "", "");
                        meetingCount++;
                        AddRow(dgvDt, "  (" + meetingCount.ToString() + ") Meeting [#" + meetingNumber + "]" + " Contract", contractNumber, customerName );

                        issueDate = dR[0]["issueDate8"].ObjToDateTime();
                        issueDateStr = issueDate.ToString("MM/dd/yyyy");
                        AddRow(dgvDt, "    Issue Date", issueDateStr);

                        contractValue = dR[0]["contractValue"].ObjToDouble();
                        AddRow(dgvDt, "   Contract Value", contractValue);

                        if ( mDt.Rows.Count > 0 )
                        {
                            effectiveFromDate = mDt.Rows[0]["effectiveFromDate"].ObjToDateTime();
                            effectiveToDate = mDt.Rows[0]["effectiveToDate"].ObjToDateTime();
                            commissionPercent = mDt.Rows[0]["commissionPercent"].ObjToDouble();
                            splitCommissionPercent = mDt.Rows[0]["splitCommissionPercent"].ObjToDouble();
                            if (issueDate < effectiveFromDate || issueDate > effectiveToDate)
                                continue;
                            dValue = contractValue * commissionPercent / 100D;
                            if (splitCommissionPercent > 0D)
                                dValue = dValue * splitCommissionPercent;
                            dValue = G1.RoundValue(dValue);
                            AddRow(dgvDt, "   Meeting Commission", dValue);
                        }
                    }

                    if (!firstMeeting)
                    {
                        AddRow(dgvDt, "", "");
                        if ( MC > 0D )
                            AddRow(dgvDt, "   Total Meeting Commissions", MC);
                        if ( MR > 0D )
                            AddRow(dgvDt, "   Total Meeting MR Commissions", MR);
                    }
                }
            }

            if (totalReins > 0D && 1 != 1)
            {
                if (chkIncludeDetails.Checked)
                {
                    AddRow(dgvDt, "", "");
                    reinContracts.TrimEnd(',');
                    string[] Lines = reinContracts.Split(',');
                    for (int i = 0; i < Lines.Length; i++)
                    {
                        contractNumber = Lines[i].Trim();
                        if (!String.IsNullOrWhiteSpace(contractNumber))
                        {
                            AddRow(dgvDt, "", "");
                            AddRow(dgvDt, "  (" + (i + 1).ToString() + ") REINSTATE CONTRACT", contractNumber);
                        }
                        DataRow[] dR = dt9.Select("contractNumber = '" + contractNumber + "'");
                        if (dR.Length > 0)
                        {
                            string yearMonth = dR[0]["YearMonth"].ObjToString();
                            issueDate = dR[0]["issueDate8"].ObjToDateTime();
                            issueDateStr = issueDate.ToString("MM/dd/yyyy");
                            AddRow(dgvDt, "    Issue Date", issueDateStr);
                            dates = dR[0]["lapseDate8"].ObjToDateTime();
                            str = dates.ToString("MM/dd/yyyy");
                            AddRow(dgvDt, "    Lapse Date", str);
                            value = dR[0]["totalContracts"].ObjToDouble();
                            double lapseTotalContracts = value;
                            AddRow(dgvDt, "     " + yearMonth + " Contracts", value, "Contracts Values for " + issueDateStr);
                            value = dR[0]["dbrSales"].ObjToDouble();
                            double lapseDBRs = value;
                            AddRow(dgvDt, "     " + yearMonth + " DBR's", value, "DBR's for " + issueDateStr);
                            value = dR[0]["contractValue"].ObjToDouble();
                            double lapseContractValue = value;
                            AddRow(dgvDt, "     " + yearMonth + " Contract Value", value);
                            AddRow(dgvDt, "", "================");
                            value = lapseTotalContracts - lapseDBRs - lapseContractValue;
                            AddRow(dgvDt, "     " + yearMonth + " Contracts Results", value);
                            value = dR[0]["goal"].ObjToDouble();
                            AddRow(dgvDt, "     " + yearMonth + " Goal", value);
                            value = dR[0]["Reins"].ObjToDouble();
                            AddRow(dgvDt, "     " + yearMonth + " Reinstate", value, "Reinstate Amount");
                        }
                    }
                    AddRow(dgvDt, "", "");
                }
            }

            AddRow(dgvDt, "", "");
            double totalBase = 0D;
            if (comboWhat == "ALL" || comboWhat == "5%")
            {
                if (fbi > 0D)
                    AddRow(dgvDt, "Total FBI $", fbi, "Already Included in Base/Split Commission");
                totalBase = totalBaseCommission + splitBaseCommission;
                totalBase = totalBaseCommission;
                //                totalBase = totalBaseCommission + splitBaseCommission + fbi;
                double beforeDBC = totalBase + totalDBCMoney;
                if (beforeDBC > 0D)
                    AddRow(dgvDt, "Total Base Commission", beforeDBC, "Total 5% Commission based on Total Payments.");
                if (splitBaseCommission > 0D)
                    AddRow(dgvDt, "Total Split Base Commission", splitBaseCommission, "Total 5% Commission based on Split Base Commissions");
                totalBase += splitBaseCommission;
                AddRow(dgvDt, "Total DBC Money", totalDBCMoney * -1D);
                AddRow(dgvDt, "", "================");
                AddRow(dgvDt, "Total 5% Commission", totalBase, "Total 5% Commission.");
                AddRow(dgvDt, "", "");
            }

            //if (totalContractCommission > 0D)
            //    AddRow(dgvDt, "Total Contract Commission", totalContractCommission, "Total 1% Commission based on Total Contract Sales.");
            double totalContract = 0D;
            if (comboWhat == "ALL" || comboWhat == "1%")
            {
                if (splitGoalCommission > 0D)
                    AddRow(dgvDt, "Total Split Goal Commission", splitGoalCommission, "Total 1% Commission based on Split Sales Commission.");
                if (totalGoalCommission > 0D)
                    AddRow(dgvDt, "Total Goal Commission", totalGoalCommission, "Total 1% Commission based on Total Sales.");
                if (totalPastFailures > 0D)
                    AddRow(dgvDt, "Total Past Failures", totalPastFailures * -1D, "Total 1% Due to Company for Unpaid Past Lapses, etc.");
                if (totalReins > 0D)
                    AddRow(dgvDt, "Total Reinstates", totalReins, "Total 1% Commission Due to Reinstated Contracts.");
                if (totalRecap > 0D)
                    AddRow(dgvDt, "Total Lapse Recaps", totalRecap * -1D, "Total 1% Due to Company for Lapsed Recap Contracts.");
                totalContract = splitGoalCommission + totalGoalCommission - totalPastFailures + totalReins - totalRecap;
                AddRow(dgvDt, "", "================");
                AddRow(dgvDt, "Total Sales Commission", totalContract, "Total 1% Commission.");
            }

            AddRow(dgvDt, "", "");

            totalCommission = totalContract + totalBase;

            if ( comboWhat == "ALL")
                AddRow(dgvDt, "Grand Total Commission", totalCommission, "Total 5% Commission + Total 1% Commission.");

            dgv.DataSource = dgvDt;
            if ( workPrintDetail )
            {
                if ( !PrintPreview )
                    printToolStripMenuItem_Click(null, null);
            }
            loading = false;
            return dgvDt;
        }
        /****************************************************************************************/
        private bool qualifyReinstate ( DateTime issueDate )
        {
            bool good = false;
            DataRow [] dRows = searchAgents.Select("type='goal'");
            if (dRows.Length <= 0)
                return false;
            DataTable dt = dRows.CopyToDataTable();
            DateTime effectiveDate = DateTime.Now;
            double percent = 0D;
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                effectiveDate = dt.Rows[i]["effectiveDate"].ObjToDateTime();
                if ( effectiveDate <= issueDate)
                    percent = dt.Rows[i]["percent"].ObjToDouble();
            }
            if (percent > 0D)
                good = true;
            return good;
        }
        /****************************************************************************************/
        private void RebuildDataTable ( DataTable dgvDt )
        {
            DataTable lapseDt = dgvDt.Clone();
            DataTable reinsDt = dgvDt.Clone();
            int startRow = -1;
            int lapseStart = -1;
            int reinsStart = -1;
            string desc = "";
            string detail = "";
            int idx = 0;
            for ( int i=0; i<dgvDt.Rows.Count; i++)
            {
                desc = dgvDt.Rows[i]["desc"].ObjToString();
                if (checkForBreak(desc))
                {
                    if (desc.ToUpper().IndexOf("REINSTATE CONTRACT") > 0)
                    {
                        G1.copy_dt_row(dgvDt, i, reinsDt, reinsDt.Rows.Count);
                        for (int j = i+1; j < dgvDt.Rows.Count; j++)
                        {
                            desc = dgvDt.Rows[j]["desc"].ObjToString();
                            if (checkForBreak(desc))
                                break;
                            G1.copy_dt_row(dgvDt, j, reinsDt, reinsDt.Rows.Count);
                        }
                    }
                    else if (desc.ToUpper().IndexOf("LAPSE CONTRACT") > 0)
                    {
                        G1.copy_dt_row(dgvDt, i, lapseDt, lapseDt.Rows.Count);
                        for (int j = i + 1; j < dgvDt.Rows.Count; j++)
                        {
                            desc = dgvDt.Rows[j]["desc"].ObjToString();
                            if (checkForBreak(desc))
                                break;
                            G1.copy_dt_row(dgvDt, j, lapseDt, lapseDt.Rows.Count);
                        }
                    }
                }
            }
            for ( int i=0; i<dgvDt.Rows.Count; i++)
            {
                desc = dgvDt.Rows[i]["desc"].ObjToString();
                if (checkForBreak(desc))
                {
                    startRow = i;
                    for ( int j=0; j<lapseDt.Rows.Count; j++)
                    {
                        G1.copy_dt_row(lapseDt, j, dgvDt, startRow + j);
                    }
                    startRow = startRow + lapseDt.Rows.Count;
                    for (int j = 0; j < reinsDt.Rows.Count; j++)
                    {
                        G1.copy_dt_row(reinsDt, j, dgvDt, startRow + j);
                    }
                    break;
                }
            }
        }
        /****************************************************************************************/
        private bool checkForBreak ( string desc)
        {
            string detail = "";
            bool found = false;
            int idx = desc.IndexOf(")");
            if (idx > 0)
            {
                detail = desc.Substring(0, idx);
                detail = detail.Replace("(", "");
                detail = detail.Replace(")", "");
                detail = detail.Trim();
                if (G1.validate_numeric(detail))
                    found = true;
            }
            return found;
        }
        /****************************************************************************************/
        private void AddRow ( DataTable dt, string desc, double value, string help = "" )
        {
            string str = G1.ReformatMoney(value);
            AddRow ( dt, desc, str, help );
        }
        /****************************************************************************************/
        private void AddRow ( DataTable dt, string desc, string detail, string help = "" )
        {
            DataRow dRow = dt.NewRow();
            dRow["desc"] = desc;
            dRow["detail"] = detail;
            dRow["help"] = help;
            dt.Rows.Add(dRow);
        }
        /****************************************************************************************/
        private void gridMain_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
        {
            if (e.RowHandle < 0)
                return;
            DataTable dt = (DataTable)dgv.DataSource;
            int row = gridMain.GetDataSourceRowIndex(e.RowHandle);
            string desc = dt.Rows[row]["desc"].ObjToString();
            string help = dt.Rows[row]["help"].ObjToString();
            double value = dt.Rows[row]["detail"].ObjToDouble();
            if (e.Column.FieldName.ToUpper() == "HELP")
            {
                if (help == "Reinstated Amount" && value > 0D)
                {
                    e.Appearance.BackColor = Color.Green;
                    e.Appearance.ForeColor = Color.Yellow;
                    Font ff = new Font(e.Appearance.Font.FontFamily, 10, FontStyle.Bold);
                    e.Appearance.Font = ff;
                }
                else if (help == "Lapse Recapture Amount" && value < 0D)
                {
                    e.Appearance.BackColor = Color.Red;
                    e.Appearance.ForeColor = Color.Yellow;
                }
                if (help == "TotalRecapture Amount" )
                {
                    e.Appearance.BackColor = Color.Purple;
                    e.Appearance.ForeColor = Color.Yellow;
                }
            }
            else if (e.Column.FieldName.ToUpper() == "DESC")
            {
                if (desc == "Total 5% Commission" )
                {
                    e.Appearance.BackColor = Color.Green;
                    e.Appearance.ForeColor = Color.Yellow;
                }
                else if (desc == "Total Sales Commission")
                {
                    e.Appearance.BackColor = Color.Green;
                    e.Appearance.ForeColor = Color.Yellow;
                }
            }
            //if (e.Column.FieldName.ToUpper() == "DESC")
            //{
            //    if (e.RowHandle >= 0)
            //    {
            //        string description = e.DisplayText.Trim().ObjToString();
            //        if ( description.ToUpper().IndexOf ( ") LAPSE CONTRACT") > 0 )
            //        {
            //            e.Appearance.FillRectangle(e.Cache, e.Bounds);
            //            e.Appearance.DrawString(e.Cache, description, e.Bounds,
            //                  new Font(e.Appearance.Font.FontFamily, 10, FontStyle.Bold),
            //                  new StringFormat());
            //            e.Handled = true;
            //            //e.Appearance.BackColor = Color.Red;
            //            //e.Appearance.ForeColor = Color.Yellow;
            //        }
            //    }
            //}
        }
        /***********************************************************************************************/
        private string getPhoneNumber ( string contractNumber )
        {
            string phoneNumber = "";
            string cmd = "Select * from `customers` where `contractNumber` = '" + contractNumber + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
                phoneNumber = dt.Rows[0]["phoneNumber"].ObjToString();
            return phoneNumber;
        }
        /***********************************************************************************************/
        private bool getContractDetails(string contractNumber, ref string customerName, ref string phoneNumber)
        {
            bool found = false;
            customerName = "";
            phoneNumber = "";
            string cmd = "Select * from `customers` where `contractNumber` = '" + contractNumber + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                phoneNumber = dt.Rows[0]["phoneNumber"].ObjToString();
                customerName = dt.Rows[0]["lastName"].ObjToString() + ", " + dt.Rows[0]["firstName"].ObjToString();
            }
            return found;
        }
        /***********************************************************************************************/
        private int pageMarginLeft = 0;
        private int pageMarginRight = 0;
        private int pageMarginTop = 0;
        private int pageMarginBottom = 0;
        /****************************************************************************************/
        private void printContinuousPreviewTool_Click(object sender, EventArgs e)
        {
            if (this.components == null)
                this.components = new System.ComponentModel.Container();

            DevExpress.XtraPrinting.PrintingSystem printingSystem1 = new DevExpress.XtraPrinting.PrintingSystem(this.components);

            int lastRow = workDt10.Rows.Count;
            DataTable myDataTable = null;
            DataTable agentDataTable = null;
            string agentStatus = "";

            string workStatus = cmbStatusWhat.Text.Trim().ToUpper();

            for (int i = 0; i < lastRow; i++)
            {
                workAgent = workDt10.Rows[i]["customer"].ObjToString();

                agentDataTable = LoadData ( ref agentStatus );
                if ( agentDataTable == null )
                {
                    if (workStatus != "ALL")
                        continue;
                }

                if (myDataTable == null)
                    myDataTable = agentDataTable.Clone();
                DataRow dR = myDataTable.NewRow();
                myDataTable.Rows.Add(dR);
                dR = myDataTable.NewRow();
                dR["desc"] = workAgent;
                myDataTable.Rows.Add(dR);

                for (int k = 0; k < agentDataTable.Rows.Count; k++)
                    myDataTable.ImportRow(agentDataTable.Rows[k]);
            }

            dgv.DataSource = myDataTable;
            DevExpress.XtraPrinting.PrintableComponentLink printableComponentLink1 = new DevExpress.XtraPrinting.PrintableComponentLink(this.components);

            printingSystem1.Links.AddRange(new object[] {
            printableComponentLink1});

            printableComponentLink1.Component = dgv;

            printableComponentLink1.PrintingSystemBase = printingSystem1;
            printableComponentLink1.CreateDetailHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateDetailHeaderArea);
            printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);
            printableComponentLink1.BeforeCreateAreas += new System.EventHandler(this.printableComponentLink1_BeforeCreateAreas);
            printableComponentLink1.AfterCreateAreas += new System.EventHandler(this.printableComponentLink1_AfterCreateAreas);
            this.gridMain.BeforePrintRow += new DevExpress.XtraGrid.Views.Base.BeforePrintRowEventHandler(this.gridMain_BeforePrintRow);
            this.gridMain.AfterPrintRow += new DevExpress.XtraGrid.Views.Base.AfterPrintRowEventHandler(this.gridMain_AfterPrintRow);

            printableComponentLink1.Landscape = false;

            Printer.setupPrinterMargins(50, 100, 80, 50);

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

        /****************************************************************************************/
        private bool printPreviewFirst = true;
        private void printPreviewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            printPreviewFirst = true;
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
            this.gridMain.BeforePrintRow += new DevExpress.XtraGrid.Views.Base.BeforePrintRowEventHandler(this.gridMain_BeforePrintRow);
            this.gridMain.AfterPrintRow += new DevExpress.XtraGrid.Views.Base.AfterPrintRowEventHandler(this.gridMain_AfterPrintRow);

            printableComponentLink1.Landscape = true;
            if (workPrintDetail)
                printableComponentLink1.Landscape = false;

            Printer.setupPrinterMargins(50, 100, 80, 50);

            pageMarginLeft = Printer.pageMarginLeft;
            pageMarginRight = Printer.pageMarginRight;
            pageMarginTop = Printer.pageMarginTop;
            pageMarginBottom = Printer.pageMarginBottom;


            printableComponentLink1.Margins.Left = pageMarginLeft;
            printableComponentLink1.Margins.Right = pageMarginRight;
            printableComponentLink1.Margins.Top = pageMarginTop;
            printableComponentLink1.Margins.Bottom = pageMarginBottom;

            G1.AdjustColumnWidths(gridMain, 0.65D, true);

            printableComponentLink1.CreateDocument();
            printingSystem1.AddCommandHandler(new PrintDocumentCommandHandler());
            printingSystem1.AddCommandHandler(new ExportToImageCommandHandler());

            if (continuousPrint)
            {
                if (File.Exists(fullPath))
                    File.Delete(fullPath);
                if (fullPath.ToUpper().IndexOf(".PDF") > 0)
                    printableComponentLink1.ExportToPdf(fullPath);
                else
                    printableComponentLink1.ExportToCsv(fullPath);
            }
            else
                printableComponentLink1.ShowPreview();

            G1.AdjustColumnWidths(gridMain, 0.65D, false );
        }
        /****************************************************************************************/
        public class PrintDocumentCommandHandler : ICommandHandler
        {
            public virtual void HandleCommand(PrintingSystemCommand command, object[] args, IPrintControl printControl, ref bool handled)
            {
                if (!CanHandleCommand(command, printControl))
                    return;
                //_Commissions_Printed = "Printed";
                //if (MessageBox.Show("Contract Is Being Printed!!\nDo you want to save this as a permanent copy of this Contract in the customers file?", "Contract Printed Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly) == DialogResult.Yes)
                //{
                //    string contract = workContractNumber;
                //    DateTime today = DateTime.Now;
                //    string filename = @"c:\smfsdata\contract_" + contract + "_" + today.Year.ToString("D4") + today.Month.ToString("D2") + today.Day.ToString("D2") + ".pdf";
                //    if (File.Exists(filename))
                //        File.Delete(filename);
                //    printControl.PrintingSystem.ExportToPdf(filename);
                //    File.Delete(filename); // Now Remove the temp file
                //}

            }
            public virtual bool CanHandleCommand(PrintingSystemCommand command, IPrintControl printControl)
            {
                return command == PrintingSystemCommand.Print;
            }
        }
        /****************************************************************************************/
        public class ExportToImageCommandHandler : ICommandHandler
        {
            public virtual void HandleCommand(PrintingSystemCommand command,
            object[] args, IPrintControl printControl, ref bool handled)
            {
                if (!CanHandleCommand(command, printControl))
                    return;

                //_Commissions_Printed = "Printed";

                // Export the document to PNG.
                //printControl.PrintingSystem.ExportToImage("C:\\Report.png", System.Drawing.Imaging.ImageFormat.Png);

                // Prevent the default exporting procedure from being called.
                handled = false;
            }

            public virtual bool CanHandleCommand(PrintingSystemCommand command, IPrintControl printControl)
            {
                return command == PrintingSystemCommand.ExportPdf;
            }
        }
        /****************************************************************************************/
        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            printPreviewFirst = true;
            try
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
                this.gridMain.BeforePrintRow += new DevExpress.XtraGrid.Views.Base.BeforePrintRowEventHandler(this.gridMain_BeforePrintRow);
                this.gridMain.AfterPrintRow += new DevExpress.XtraGrid.Views.Base.AfterPrintRowEventHandler(this.gridMain_AfterPrintRow);

                printableComponentLink1.Landscape = true;
                if ( workPrintDetail )
                    printableComponentLink1.Landscape = false;

                Printer.setupPrinterMargins(50, 100, 80, 50);

                pageMarginLeft = Printer.pageMarginLeft;
                pageMarginRight = Printer.pageMarginRight;
                pageMarginTop = Printer.pageMarginTop;
                pageMarginBottom = Printer.pageMarginBottom;

                printableComponentLink1.Margins.Left = pageMarginLeft;
                printableComponentLink1.Margins.Right = pageMarginRight;
                printableComponentLink1.Margins.Top = pageMarginTop;
                printableComponentLink1.Margins.Bottom = pageMarginBottom;

                G1.AdjustColumnWidths(gridMain, 0.65D, true);

                printableComponentLink1.CreateDocument();
                if (LoginForm.doLapseReport)
                    printableComponentLink1.Print();
                else
                {
                    if (workPrintDetail)
                    {
                        if (firstPrint)
                        {
                            if (!String.IsNullOrWhiteSpace(workPrinterName))
                                printableComponentLink1.Print(workPrinterName);
                        }
                        else
                        {
                            if ( !String.IsNullOrWhiteSpace ( workPrinterName))
                                printableComponentLink1.Print( workPrinterName);
                        }
                        firstPrint = false;
                    }
                    else
                        printableComponentLink1.PrintDlg();
                }
            }
            catch ( Exception ex)
            {
            }
            G1.AdjustColumnWidths(gridMain, 0.65D, false );
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

            font = new Font("Ariel", 6);
            Printer.DrawGridDate(2, 3, 2, 3, Color.Black, BorderSide.None, font);
            Printer.DrawGridPage(11, 3, 2, 3, Color.Black, BorderSide.None, font);

            //            Printer.DrawQuad(1, 9, 2, 3, "User : " + LoginForm.username, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);

            font = new Font("Ariel", 10, FontStyle.Bold);

            if ( !workPrintDetail )
                Printer.DrawQuad(5, 8, 5, 4, "Commission Details for " + workAgent, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            else
                Printer.DrawQuad(5, 8, 5, 4, "Commission Details for All Agents", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);


            //            Printer.DrawQuadTicks();
            Printer.SetQuadSize(24, 12);
            font = new Font("Ariel", 7, FontStyle.Regular);
            //Printer.DrawQuad(16, 7, 5, 2, lblBalance.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Top);
            //Printer.DrawQuad(16, 10, 5, 2, lblPayment.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            font = new Font("Ariel", 8);
            if (!workPrintDetail)
            {
                string search = "Agent : " + workAgent;
                Printer.DrawQuad(1, 6, 6, 3, search, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Top);
            }
            Printer.DrawQuad(1, 9, 6, 3, "User : " + LoginForm.username, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);

            //DateTime date = this.dateTimePicker2.Value;
            //date = date.AddDays(1);
            //string workDate = date.ToString("MM/dd/yyyy");
            //Printer.SetQuadSize(24, 12);
            //font = new Font("Ariel", 9, FontStyle.Bold);
            //Printer.DrawQuad(20, 8, 5, 4, "Report Ending: " + workDate, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /****************************************************************************************/
        private int footerCount = 0;
        private void gridMain_BeforePrintRow(object sender, DevExpress.XtraGrid.Views.Printing.CancelPrintRowEventArgs e)
        {
            int row = e.RowHandle;
            if (row < 0)
                return;
            row = gridMain.GetDataSourceRowIndex(row);
            DataTable dt = (DataTable)dgv.DataSource;
            string finale = dt.Rows[row]["detail"].ObjToString();
            bool doit = false;
            if (finale.Trim().ToUpper() == "ACTIVE")
                doit = true;
            else if (finale.Trim().ToUpper() == "INACTIVE")
                doit = true;
            else if (finale.Trim().ToUpper() == "GONE")
                doit = true;
            if ( doit )
            {
                if (!printPreviewFirst)
                    e.PS.InsertPageBreak(e.Y);
                //printPreviewFirst = false;
            }
            finale = dt.Rows[row]["desc"].ObjToString();
            if (finale.Trim().ToUpper().IndexOf("TOTAL PAYMENTS") == 0)
                printPreviewFirst = false;

            if (e.HasFooter)
            {
                footerCount++;
            }
        }
        /****************************************************************************************/
        private void gridMain_AfterPrintRow(object sender, DevExpress.XtraGrid.Views.Printing.PrintRowEventArgs e)
        {
            if (e.HasFooter)
            {
                footerCount++;
                if (footerCount >= 1)
                {
                    footerCount = 0;
                }
            }
        }
        /****************************************************************************************/
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        /****************************************************************************************/
        private void chkIncludeDetails_CheckedChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            if (workPrintDetail)
            {
                if (chkIncludeDetails.Checked)
                    chkChronological.Show();
                else
                    chkChronological.Hide();
                LoadAllData();
            }
            else
            {
                string agentStatus = "";
                LoadData ( ref agentStatus );
            }

            //CommissionDetail_Load(null, null);
        }
        /****************************************************************************************/
        private void chkChronological_CheckedChanged(object sender, EventArgs e)
        {
            CommissionDetail_Load(null, null);
        }
        /****************************************************************************************/
        private DataTable determineFormulaRecapReins( DataTable dt8, DataTable dt9, DataTable dt10)
        {
            string formula = "";
            bool doLocation = false;

            DataTable allAgentsDt = G1.get_db_data("Select * from `agents`;");

            DataTable bothDt = dt8.Clone();
            bothDt.Columns.Add("Where");
            bothDt.Columns.Add("Done");

            for (int i = 0; i < dt10.Rows.Count; i++)
            {
                doLocation = false;
                formula = dt10.Rows[i]["formula"].ObjToString();
                if (String.IsNullOrWhiteSpace(formula))
                    continue;
                string[] Lines = formula.Split('+');
                formula = "(";
                for (int j = 0; j < Lines.Length; j++)
                {
                    if (!Trust85.isAgent(Lines[j].Trim(), allAgentsDt))
                        doLocation = true;
                    formula += "'" + Lines[j] + "',";
                }
                formula = formula.TrimEnd(',');
                formula += ")";
                if ( doLocation )
                {
                    DataRow[] dRows = dt8.Select("loc IN " + formula);
                    for (int j = 0; j < dRows.Length; j++)
                        bothDt.ImportRow(dRows[j]);
                }
            }

            for (int i = 0; i < bothDt.Rows.Count; i++)
                bothDt.Rows[i]["Where"] = "8";

            int count = bothDt.Rows.Count;

            for (int i = 0; i < dt10.Rows.Count; i++)
            {
                doLocation = false;
                formula = dt10.Rows[i]["formula"].ObjToString();
                string[] Lines = formula.Split('+');
                formula = "(";
                for (int j = 0; j < Lines.Length; j++)
                {
                    if (!Trust85.isAgent(Lines[j].Trim(), allAgentsDt))
                        doLocation = true;
                    formula += "'" + Lines[j] + "',";
                }
                formula = formula.TrimEnd(',');
                formula += ")";
                if (doLocation)
                {
                    DataRow[] dRows = dt9.Select("loc IN " + formula);
                    for (int j = 0; j < dRows.Length; j++)
                        bothDt.ImportRow(dRows[j]);
                }
            }

            string where = "";
            for (int i = 0; i < bothDt.Rows.Count; i++)
            {
                where = bothDt.Rows[i]["Where"].ObjToString();
                if ( String.IsNullOrWhiteSpace (where))
                    bothDt.Rows[i]["Where"] = "9";
            }

            DataView tempview = bothDt.DefaultView;
            tempview.Sort = "YearMonth asc,ContractNumber asc, Where asc";
            bothDt = tempview.ToTable();

            return bothDt;
        }
        /***********************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (dgv.Visible)
            {
                G1.SpyGlass(gridMain);
                //SetSpyGlass(gridMain);
            }
        }
        /***********************************************************************************************/
        private void SetSpyGlass(DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView grid)
        {
            if (grid.OptionsFind.AlwaysVisible == true)
                grid.OptionsFind.AlwaysVisible = false;
            else
                grid.OptionsFind.AlwaysVisible = true;
        }
        /****************************************************************************************/
        private void cmbShowWhat_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loading)
                return;

            CommissionDetail_Load(null, null);
        }
        /****************************************************************************************/
        private void cmbStatusWhat_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loading)
                return;

            CommissionDetail_Load(null, null);
        }
        /****************************************************************************************/
        private void massSaveReportsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable saveDt = (DataTable)dgv.DataSource;

            string savePercent = cmbShowWhat.Text;
            string saveWhat = cmbStatusWhat.Text;

            DateTime date = workStop;
            string yyyy = date.Year.ToString("D4");
            string month = G1.ToMonthName(date);

            DialogResult result = MessageBox.Show("Are you sure you want to RUN the Mass Commission Rports for " + date.ToString("MM/dd/yyyy") + "?", "Mass Commissions Report Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;
            string cmd = "Select * from `mass_reports` where `mainReport` = 'Commissions';";
            DataTable dx = G1.get_db_data(cmd);

            string mainReport = "";
            string report = "";
            string outputFilname = "";
            string outputDirectory = "";


            if (dx.Rows.Count > 0)
            {
                outputFilname = dx.Rows[0]["outputFilename"].ObjToString();
                outputDirectory = dx.Rows[0]["outputDirectory"].ObjToString();
            }
            else
            {
                outputFilname = "Commissions (5%) month yyyy.pdf";
                outputDirectory = "C:/SMFS Reports/Commissions/2021 Commissions";
            }

            outputDirectory = outputDirectory.Replace("2021", yyyy);
            outputFilname = outputFilname.Replace("yyyy", yyyy);
            outputFilname = outputFilname.Replace("month", month);

            fullPath = outputDirectory + "/" + outputFilname;

            G1.verify_path(outputDirectory);

            this.Text = mainReport + " " + report;

            loading = true;
            cmbShowWhat.Text = "5%";
            cmbStatusWhat.Text = "Active";
            chkShowAll.Checked = true;
            chkIncludeDetails.Checked = true;
            loading = false;

            CommissionDetail_Load(null, null);
            //LoadAllData();

            DataTable dt = (DataTable)dgv.DataSource;

            continuousPrint = true;
            fullPath = QualifyFilename(fullPath);
            printPreviewToolStripMenuItem_Click(null, null);

            fullPath = outputDirectory + "/" + outputFilname;
            fullPath = fullPath.Replace(".pdf", ".csv");
            fullPath = QualifyFilename(fullPath);
            printPreviewToolStripMenuItem_Click(null, null);
            continuousPrint = false;


            if (dx.Rows.Count > 0)
            {
                outputFilname = dx.Rows[0]["outputFilename"].ObjToString();
                outputDirectory = dx.Rows[0]["outputDirectory"].ObjToString();
            }
            else
            {
                outputFilname = "Commissions (1%) month yyyy.pdf";
                outputDirectory = "C:/SMFS Reports/Commissions/2021 Commissions";
            }

            outputDirectory = outputDirectory.Replace("2021", yyyy);
            outputFilname = outputFilname.Replace("yyyy", yyyy);
            outputFilname = outputFilname.Replace("month", month);

            fullPath = outputDirectory + "/" + outputFilname;

            G1.verify_path(outputDirectory);

            this.Text = mainReport + " " + report;

            loading = true;
            cmbShowWhat.Text = "1%";
            cmbStatusWhat.Text = "Active";
            chkShowAll.Checked = true;
            chkIncludeDetails.Checked = true;
            loading = false;

            CommissionDetail_Load(null, null);
            //LoadAllData();

            dt = (DataTable)dgv.DataSource;

            continuousPrint = true;
            fullPath = QualifyFilename ( fullPath );
            printPreviewToolStripMenuItem_Click(null, null);

            fullPath = outputDirectory + "/" + outputFilname;
            fullPath = fullPath.Replace(".pdf", ".csv");
            fullPath = QualifyFilename(fullPath);
            printPreviewToolStripMenuItem_Click(null, null);
            continuousPrint = false;

            fullPath = "";

            loading = true;
            cmbShowWhat.Text = savePercent;
            cmbStatusWhat.Text = saveWhat;
            chkShowAll.Checked = true;
            chkIncludeDetails.Checked = true;
            loading = false;

            if (Trust85.localTrust85 != null)
            {
                if (dx.Rows.Count > 0)
                {
                    outputFilname = dx.Rows[0]["outputFilename"].ObjToString();
                    outputDirectory = dx.Rows[0]["outputDirectory"].ObjToString();
                }
                else
                {
                    outputFilname = "Agent Commissions (5%) month yyyy.pdf";
                    outputDirectory = "C:/SMFS Reports/Commissions/2021 Commissions";
                }

                outputDirectory = outputDirectory.Replace("2021", yyyy);
                outputFilname = outputFilname.Replace("yyyy", yyyy);
                outputFilname = outputFilname.Replace("month", month);

                fullPath = outputDirectory + "/" + outputFilname;

                G1.verify_path(outputDirectory);

                fullPath = QualifyFilename(fullPath);
                Trust85.localTrust85.FireEventAgentTotals( fullPath );

                fullPath = outputDirectory + "/" + outputFilname;
                fullPath = fullPath.Replace(".pdf", ".csv");
                fullPath = QualifyFilename(fullPath);
                Trust85.localTrust85.FireEventAgentTotals(fullPath);
            }

            if (Trust85.localTrust85 != null)
            {
                if (dx.Rows.Count > 0)
                {
                    outputFilname = dx.Rows[0]["outputFilename"].ObjToString();
                    outputDirectory = dx.Rows[0]["outputDirectory"].ObjToString();
                }
                else
                {
                    outputFilname = "Agent Meetings month yyyy.pdf";
                    outputDirectory = "C:/SMFS Reports/Commissions/2021 Commissions";
                }

                outputDirectory = outputDirectory.Replace("2021", yyyy);
                outputFilname = outputFilname.Replace("yyyy", yyyy);
                outputFilname = outputFilname.Replace("month", month);

                fullPath = outputDirectory + "/" + outputFilname;

                G1.verify_path(outputDirectory);

                fullPath = QualifyFilename(fullPath);
                Trust85.localTrust85.FireEventAgentMeetings(fullPath);

                fullPath = outputDirectory + "/" + outputFilname;
                fullPath = fullPath.Replace(".pdf", ".csv");
                fullPath = QualifyFilename(fullPath);
                Trust85.localTrust85.FireEventAgentMeetings(fullPath);
            }
            dgv.DataSource = saveDt;
            dgv.Refresh();

            _Commissions_Printed = "Printed";
        }
        /****************************************************************************************/
        private string QualifyFilename ( string fullPath )
        {
            bool isPDF = false;
            bool isCSV = false;

            if ( fullPath.IndexOf ( ".pdf") > 0 )
            {
                isPDF = true;
                fullPath = fullPath.Replace(".pdf", "");
            }
            else if (fullPath.IndexOf(".csv") > 0)
            {
                isCSV = true;
                fullPath = fullPath.Replace(".csv", "");
            }

            string newPath = fullPath;

            int start = 1;
            for (; ;)
            {
                //newPath = fullPath;
                if ( isPDF )
                {
                    newPath = newPath + ".pdf";
                    if (!File.Exists(newPath))
                        break;
                }
                else if (isCSV)
                {
                    newPath = newPath + ".csv";
                    if (!File.Exists(newPath))
                        break;
                }
                newPath = fullPath + "_" + start.ToString();
                start++;
            }
            return newPath;
        }
        /****************************************************************************************/
        private void CommissionDetail_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (disallowSave)
                _Commissions_Printed = "";
        }
        /****************************************************************************************/
        private void workingDayGenerateAndSaveMassReportsAndDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable saveDt = (DataTable)dgv.DataSource;

            string savePercent = cmbShowWhat.Text;
            string saveWhat = cmbStatusWhat.Text;

            DateTime date = workStop;
            string yyyy = date.Year.ToString("D4");
            string month = G1.ToMonthName(date);

            DialogResult result = MessageBox.Show("Are you sure you want to RUN the 10-Working Day Mass Commission Rports for " + date.ToString("MM/dd/yyyy") + "?", "Mass Commissions Report Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;
            string cmd = "Select * from `mass_reports` where `mainReport` = 'Commissions';";
            DataTable dx = G1.get_db_data(cmd);

            string mainReport = "";
            string report = "";
            string outputFilname = "";
            string outputDirectory = "";


            if (dx.Rows.Count > 0)
            {
                outputFilname = dx.Rows[0]["outputFilename"].ObjToString();
                outputDirectory = dx.Rows[0]["outputDirectory"].ObjToString();
            }
            else
            {
                outputFilname = "Commissions (5%) month yyyy 10-Working Day.pdf";
                outputDirectory = "C:/SMFS Reports/Commissions/2021 Commissions";
            }

            outputDirectory = outputDirectory.Replace("2021", yyyy);
            outputFilname = outputFilname.Replace("yyyy", yyyy);
            outputFilname = outputFilname.Replace("month", month);

            fullPath = outputDirectory + "/" + outputFilname;

            G1.verify_path(outputDirectory);

            this.Text = mainReport + " " + report;

            loading = true;
            cmbShowWhat.Text = "5%";
            cmbStatusWhat.Text = "Active";
            chkShowAll.Checked = true;
            chkIncludeDetails.Checked = true;
            loading = false;

            CommissionDetail_Load(null, null);
            //LoadAllData();

            DataTable dt = (DataTable)dgv.DataSource;

            continuousPrint = true;
            fullPath = QualifyFilename(fullPath);
            printPreviewToolStripMenuItem_Click(null, null);

            fullPath = outputDirectory + "/" + outputFilname;
            fullPath = fullPath.Replace(".pdf", ".csv");
            fullPath = QualifyFilename(fullPath);
            printPreviewToolStripMenuItem_Click(null, null);
            continuousPrint = false;


            if (dx.Rows.Count > 0)
            {
                outputFilname = dx.Rows[0]["outputFilename"].ObjToString();
                outputDirectory = dx.Rows[0]["outputDirectory"].ObjToString();
            }
            else
            {
                outputFilname = "Commissions (1%) month yyyy 10-Working Day.pdf";
                outputDirectory = "C:/SMFS Reports/Commissions/2021 Commissions";
            }

            outputDirectory = outputDirectory.Replace("2021", yyyy);
            outputFilname = outputFilname.Replace("yyyy", yyyy);
            outputFilname = outputFilname.Replace("month", month);

            fullPath = outputDirectory + "/" + outputFilname;

            G1.verify_path(outputDirectory);

            this.Text = mainReport + " " + report;

            loading = true;
            cmbShowWhat.Text = "1%";
            cmbStatusWhat.Text = "Active";
            chkShowAll.Checked = true;
            chkIncludeDetails.Checked = true;
            loading = false;

            CommissionDetail_Load(null, null);
            //LoadAllData();

            dt = (DataTable)dgv.DataSource;

            continuousPrint = true;
            fullPath = QualifyFilename(fullPath);
            printPreviewToolStripMenuItem_Click(null, null);

            fullPath = outputDirectory + "/" + outputFilname;
            fullPath = fullPath.Replace(".pdf", ".csv");
            fullPath = QualifyFilename(fullPath);
            printPreviewToolStripMenuItem_Click(null, null);
            continuousPrint = false;

            fullPath = "";

            loading = true;
            cmbShowWhat.Text = savePercent;
            cmbStatusWhat.Text = saveWhat;
            chkShowAll.Checked = true;
            chkIncludeDetails.Checked = true;
            loading = false;

            if (Trust85.localTrust85 != null)
            {
                if (dx.Rows.Count > 0)
                {
                    outputFilname = dx.Rows[0]["outputFilename"].ObjToString();
                    outputDirectory = dx.Rows[0]["outputDirectory"].ObjToString();
                }
                else
                {
                    outputFilname = "Agent Commissions (5%) month yyyy 10-Working Day.pdf";
                    outputDirectory = "C:/SMFS Reports/Commissions/2021 Commissions";
                }

                outputDirectory = outputDirectory.Replace("2021", yyyy);
                outputFilname = outputFilname.Replace("yyyy", yyyy);
                outputFilname = outputFilname.Replace("month", month);

                fullPath = outputDirectory + "/" + outputFilname;

                G1.verify_path(outputDirectory);

                fullPath = QualifyFilename(fullPath);
                Trust85.localTrust85.FireEventAgentTotals(fullPath);

                fullPath = outputDirectory + "/" + outputFilname;
                fullPath = fullPath.Replace(".pdf", ".csv");
                fullPath = QualifyFilename(fullPath);
                Trust85.localTrust85.FireEventAgentTotals(fullPath);
            }

            if (Trust85.localTrust85 != null)
            {
                if (dx.Rows.Count > 0)
                {
                    outputFilname = dx.Rows[0]["outputFilename"].ObjToString();
                    outputDirectory = dx.Rows[0]["outputDirectory"].ObjToString();
                }
                else
                {
                    outputFilname = "Agent Meetings month yyyy 10-Working Day.pdf";
                    outputDirectory = "C:/SMFS Reports/Commissions/2021 Commissions";
                }

                outputDirectory = outputDirectory.Replace("2021", yyyy);
                outputFilname = outputFilname.Replace("yyyy", yyyy);
                outputFilname = outputFilname.Replace("month", month);

                fullPath = outputDirectory + "/" + outputFilname;

                G1.verify_path(outputDirectory);

                fullPath = QualifyFilename(fullPath);
                Trust85.localTrust85.FireEventAgentMeetings(fullPath);

                fullPath = outputDirectory + "/" + outputFilname;
                fullPath = fullPath.Replace(".pdf", ".csv");
                fullPath = QualifyFilename(fullPath);
                Trust85.localTrust85.FireEventAgentMeetings(fullPath);
            }
            dgv.DataSource = saveDt;
            dgv.Refresh();

            _Commissions_Printed = "Printed";
        }
        /****************************************************************************************/
    }
}