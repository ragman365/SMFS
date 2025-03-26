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
using DevExpress.XtraRichEdit.API.Native;
using DevExpress.XtraGrid.Views.Grid;
using MySql.Data.Types;
using System.Drawing.Drawing2D;
using DevExpress.Utils.Drawing;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class InsuranceCollectionsReport : DevExpress.XtraEditors.XtraForm
    {
        private bool loading = true;
        private DataTable logicDt = null;
        private string workContract = "";
        /****************************************************************************************/
        public InsuranceCollectionsReport( string contract = "")
        {
            InitializeComponent();
            workContract = contract;
        }
        /****************************************************************************************/
        private void InsuranceCollectionsReport_Load(object sender, EventArgs e)
        {
            chkSecondMethod.Hide();

            DateTime now = DateTime.Now;
            for (;;)
            {
                if (now.DayOfWeek == DayOfWeek.Friday)
                {
                    this.dateTimePicker2.Value = now;
                    this.dateTimePicker1.Value = now.AddDays(-4);
                    this.dateTimePicker3.Value = this.dateTimePicker1.Value;
                    this.dateTimePicker4.Value = this.dateTimePicker2.Value;
                    break;
                }
                now = now.AddDays(-1);
            }
//            SetupLogicCombo();
            SetupReportCombo();
            loading = false;
            gridMain.Appearance.GroupFooter.Changed += GroupFooter_Changed;
            gridMain.Appearance.GroupRow.Changed += GroupRow_Changed;
            SetupTotalsSummary();
        }
        /****************************************************************************************/
        private void SetupTotalsSummary()
        {
            AddSummaryColumn("paymentAmount", null);
            AddSummaryColumn("Paid", null);
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
        /****************************************************************************************/
        private void AddSummaryColumn(string columnName, DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gMain = null)
        {
            if (gMain == null)
                gMain = gridMain;
            //            gMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            gMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            //            gMain.Columns[columnName].SummaryItem.DisplayFormat = "${0:0,0.00}";
            gMain.Columns[columnName].SummaryItem.DisplayFormat = "{0:N2}";
        }
        /****************************************************************************************/
        private void SetupReportCombo()
        {
            cmbReport.Items.Add("ALL");
            string cmd = "Select `report` from `policies` where `report` <> '' GROUP by `report` ORDER by `report`;";
            DataTable dx = G1.get_db_data(cmd);
            dx.Columns.Add("order", Type.GetType("System.Int32"));
            string report = "";
            int order = 999;
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                order = 999;
                report = dx.Rows[i]["report"].ObjToString();
                if (report == "CLI")
                    order = 0;
                else if (report == "Selected Funeral Insurance - Magee")
                    order = 1;
                else if (report == "Gulf National / SFIC-Forest")
                    order = 2;
                else if (report == "Magnolia Guaranty")
                    order = 4;
                else if (report == "Colonial Guaranty")
                    order = 5;
                else if (report == "Colonial Ordinary")
                    order = 6;
                dx.Rows[i]["order"] = order;
            }
            DataView tempview = dx.DefaultView;
            tempview.Sort = "order";
            dx = tempview.ToTable();

            report = "";
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                report = dx.Rows[i]["report"].ObjToString();
                cmbReport.Items.Add(report);
            }
            cmbReport.Text = "ALL";
        }
        /****************************************************************************************/
        private void SetupLogicCombo ()
        {
            string location = "";
            string firstReport = "";
            PullLogicFile();
            cmbReport.Items.Add("ALL");
            firstReport = "ALL";
            for ( int i=0; i<logicDt.Rows.Count; i++)
            {
                logicDt.Rows[i]["num"] = i.ToString();
                location = logicDt.Rows[i]["location"].ObjToString();
                if (String.IsNullOrWhiteSpace(location))
                    continue;
                if (String.IsNullOrWhiteSpace(firstReport))
                    firstReport = location;
                cmbReport.Items.Add(location);
            }
            cmbReport.Text = firstReport;
        }
        /****************************************************************************************/
        private void PullLogicFile ()
        {
            logicDt = G1.get_db_data("Select * from `logic`;");
            logicDt.Columns.Add("num");
            logicDt.Columns.Add("company");
            logicDt.Columns.Add("oldAgent");
            logicDt.Columns.Add("agent");
            logicDt.Columns.Add("ucode");
        }
        /****************************************************************************************/
        private void btnRun_Click(object sender, EventArgs e)
        {
            LoadData();
        }
        /****************************************************************************************/
        private void LoadData()
        {
            this.Cursor = Cursors.WaitCursor;

            PullLogicFile();

            string report = cmbReport.Text;

            DateTime date = dateTimePicker1.Value;
            string date1 = G1.DateTimeToSQLDateTime(date);
            date = dateTimePicker2.Value;
            string date2 = G1.DateTimeToSQLDateTime(date);

            date = dateTimePicker3.Value;
            string date3 = G1.DateTimeToSQLDateTime(date);
            date = dateTimePicker4.Value;
            string date4 = G1.DateTimeToSQLDateTime(date);

            DateTime now = DateTime.Now;

            DateTime paidout = new DateTime(2039, 12, 31);

            string cmd = "Select *, CONCAT(p.`lastName`, ', ', p.`firstName` ) payerName, CONCAT(j.`policyLastName`,', ', j.`policyFirstName`) policyName ";
            cmd += " FROM `ipayments` p JOIN `icontracts` c ON p.`contractNumber` = c.`contractNumber` ";
            cmd += " JOIN `icustomers` u ON p.`contractNumber` = u.`contractNumber` ";
            cmd += " JOIN `policies` j ON j.`payer` = u.`payer` ";
            cmd += " WHERE `payDate8` >= 'XYZZY1' and `payDate8` <= 'XYZZY2' ";
            cmd += " AND ( p.`paymentAmount` > 0.00 OR p.`debitAdjustment` > 0.00 OR p.`creditAdjustment` > 0.00 )";
            //            cmd += " AND j.`groupNumber` <> '' ";
            cmd += " AND j.`deceasedDate` < '0002-12-31' ";
            if (!chkIncludeLapses.Checked)
                cmd += " and j.`lapsed` <> 'Y' ";
            if (!String.IsNullOrWhiteSpace(report))
            {
                if (report == "ALL")
                {
                    cmd += " AND `report` <> 'Not Third Party' ";
                }
                else
                    cmd += " AND `report` = '" + report + "' ";
            }
            //else
            //    cmd += " AND `report` <> 'Not Third Party' ";

            if (!String.IsNullOrWhiteSpace(workContract))
                cmd += " AND u.`contractNumber` = '" + workContract + "' ";

            string saveDate = cmd;

            if (!chkACH.Checked)
                cmd += " AND (`depositNumber` LIKE 'T%' OR `depositNumber` LIKE 'A%') ";
            else
                cmd += " AND (`depositNumber` LIKE 'T%') ";

            //            cmd += " ORDER BY `report`, `payerName`, `policyName` ";
            cmd += ";";

            string saveCmd = cmd;
            cmd = cmd.Replace("XYZZY1", date1);
            cmd = cmd.Replace("XYZZY2", date2);

            DataTable dt = G1.get_db_data(cmd);

            ////int count = dt.Rows.Count;
            ////if (chkHonor.Checked)
            ////{
            ////    dt = CustomerDetails.filterSecNat(chkSecNat.Checked, dt);
            ////    int newCount = dt.Rows.Count;
            ////}

            int i = 0;
            int j = 0;
            int idx = 0;

            cmd = cmd.Replace("XYZZY1", date3);
            cmd = cmd.Replace("XYZZY2", date4);
            if (!chkACH.Checked)
                cmd = cmd.Replace("(`depositNumber` LIKE 'T%' OR `depositNumber` LIKE 'A%')", "(`depositNumber` NOT LIKE 'T%' AND `depositNumber` NOT LIKE 'A%')");
            else
                cmd = cmd.Replace("(`depositNumber` LIKE 'T%')", "(`depositNumber` LIKE 'A%') ");

            DataTable ddt = G1.get_db_data(cmd);

            //if (chkHonor.Checked)
            //    ddt = CustomerDetails.filterSecNat(chkSecNat.Checked, ddt);

            for (i = 0; i < ddt.Rows.Count; i++)
            {
                dt.ImportRow(ddt.Rows[i]);
            }
            cmd = saveDate;
            cmd = cmd.Replace("XYZZY1", date3);
            cmd = cmd.Replace("XYZZY2", date4);
            cmd += " AND (`depositNumber` NOT LIKE 'T%' AND `depositNumber` NOT LIKE 'A%')";
            cmd += " AND `edited` = 'Manual'";
            ddt = G1.get_db_data(cmd);
            for (i = 0; i < ddt.Rows.Count; i++)
            {
                dt.ImportRow(ddt.Rows[i]);
            }

            double payment = 0D;
            double debit = 0D;
            double credit = 0D;
            string contractNumber = "";
            string oldContractNumber = "";
            string lapsed2 = "";
            string payer = "";
            double amtOfMonthlyPayt = 0D;
            DateTime payDate8 = DateTime.Now;
            DateTime mainDate8 = DateTime.Now;

            //string testPayer = txtPayer.Text.Trim();
            //if ( !String.IsNullOrWhiteSpace ( testPayer))
            //{
            //    DataRow[] ddR = dt.Select("payer='" + testPayer + "'");
            //    if (ddR.Length > 0)
            //        dt = ddR.CopyToDataTable();
            //}

            dt.Columns.Add("policyLapsed");
            Trust85.FindContract(dt, "ZZ0004510");
            for (i = 0; i < dt.Rows.Count; i++)
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                payer = dt.Rows[i]["payer"].ObjToString();
                if (String.IsNullOrWhiteSpace(oldContractNumber))
                {
                    oldContractNumber = contractNumber;
                    mainDate8 = DateTime.Now;
                    mainDate8 = payDate8;
                    amtOfMonthlyPayt = Policies.CalcMonthlyPremium(payer, mainDate8);
                    dt.Rows[i]["amtOfMonthlyPayt"] = amtOfMonthlyPayt;
                }
                if (contractNumber == "ZZ0004510")
                {
                }
                if (payer == "EV-090548")
                {
                }
                if (payer == "UC-1521")
                {
                }
                payDate8 = dt.Rows[i]["payDate8"].ObjToDateTime();
                payment = dt.Rows[i]["paymentAmount"].ObjToDouble();
                debit = dt.Rows[i]["debitAdjustment"].ObjToDouble();
                credit = dt.Rows[i]["creditAdjustment"].ObjToDouble();
                payment = payment - debit + credit;
                dt.Rows[i]["paymentAmount"] = payment;
                lapsed2 = dt.Rows[i]["lapsed2"].ObjToString();
                dt.Rows[i]["policyLapsed"] = lapsed2;
                payer = dt.Rows[i]["payer"].ObjToString();
                if (String.IsNullOrWhiteSpace(payer))
                {
                    payer = dt.Rows[i]["payer2"].ObjToString();
                    dt.Rows[i]["payer"] = payer;
                }
                if (oldContractNumber != contractNumber)
                {
                    oldContractNumber = contractNumber;
                    mainDate8 = DateTime.Now;
                    mainDate8 = payDate8;
                    amtOfMonthlyPayt = Policies.CalcMonthlyPremium(payer, mainDate8);
                    dt.Rows[i]["amtOfMonthlyPayt"] = amtOfMonthlyPayt;
                }
                else
                    dt.Rows[i]["amtOfMonthlyPayt"] = amtOfMonthlyPayt;
            }
            Trust85.FindContract(dt, "ZZ0001560");

            //            cmd += " ORDER BY `report`, `payerName`, `policyName` ";
            DataView tempview = dt.DefaultView;
            tempview.Sort = "report, payerName, tmstamp, policyName";
            dt = tempview.ToTable();

            dt.Columns.Add("num");
            dt.Columns.Add("customer");
            dt.Columns.Add("majorBreak");
            dt.Columns.Add("printDueDate");
            dt.Columns.Add("Paid", Type.GetType("System.Double"));
            dt.Columns.Add("months", Type.GetType("System.Double"));
            dt.Columns.Add("reject");
            //            dt.Columns.Add("Report");
            dt.Columns.Add("finale");
            dt.Columns.Add("premiumNow", Type.GetType("System.Double"));

            int addCount = 0;

            string companyCode = "";
            string status = "";

            DataTable backupDt = dt.Copy();
            DataTable finalDt = dt.Clone();
            int backupRows = dt.Rows.Count;

            //if (chkHonorReport.Checked)
            //{
            //    report = cmbReport.Text.Trim();
            //    if (report == "ALL")
            //    {
            //        lblTime.Text = "0:00";
            //        lblTime.Show();

            //        lblTotal.Show();
            //        lblTotal.Text = "of " + cmbReport.Items.Count.ToString();
            //        lblTotal.Refresh();

            //        labelMaximum.Show();
            //        lblTotal.Show();
            //        barImport.Show();

            //        barImport.Minimum = 0;
            //        barImport.Maximum = cmbReport.Items.Count;
            //        labelMaximum.Show();

            //        DateTime start = DateTime.Now;
            //        DateTime stop = DateTime.Now;
            //        TimeSpan ts = stop - start;
            //        int minutes = 0;
            //        int hours = 0;

            //        for (i = 0; i < cmbReport.Items.Count; i++)
            //        {
            //            barImport.Value = i;
            //            barImport.Refresh();
            //            labelMaximum.Text = i.ToString();
            //            labelMaximum.Refresh();
            //            //if (i > 4)
            //            //    break;

            //            report = cmbReport.Items[i].ObjToString().Trim();
            //            if (report == "ALL")
            //                continue;
            //            if (report.ToUpper() == "ALL THIRD PARTY")
            //                continue;
            //            if (report.ToUpper() == "RAG")
            //                continue;
            //            //                        dt = backupDt.Copy();
            //            ProcessLogicData(dt, report);
            //            for (j = 0; j < dt.Rows.Count; j++)
            //            {
            //                status = dt.Rows[j]["reject"].ObjToString().Trim().ToUpper();
            //                if (String.IsNullOrWhiteSpace(status))
            //                {
            //                    companyCode = dt.Rows[j]["companyCode"].ObjToString();
            //                    dt.Rows[j]["Report"] = companyCode + " " + report;
            //                    finalDt.ImportRow(dt.Rows[j]);
            //                    addCount++;
            //                }
            //            }
            //            idx = finalDt.Rows.Count - 1;
            //            if (idx >= 0)
            //                finalDt.Rows[idx]["finale"] = "YES";

            //            for (j = dt.Rows.Count - 1; j >= 0; j--)
            //            {
            //                status = dt.Rows[j]["reject"].ObjToString().Trim().ToUpper();
            //                if (String.IsNullOrWhiteSpace(status))
            //                    dt.Rows.RemoveAt(j);
            //            }

            //            stop = DateTime.Now;
            //            ts = stop - start;
            //            minutes = ts.TotalMinutes.ObjToInt32();
            //            hours = 0;
            //            if (minutes >= 60)
            //            {
            //                hours = minutes / 60;
            //                minutes = minutes % 60;
            //            }
            //            lblTime.Text = ts.Hours.ToString() + ":" + ts.Minutes.ToString() + ":" + ts.Seconds.ToString();
            //            lblTime.Refresh();
            //        }
            //        barImport.Value = cmbReport.Items.Count;
            //        barImport.Refresh();
            //        labelMaximum.Text = cmbReport.Items.Count.ToString();
            //        labelMaximum.Refresh();
            //        dt = finalDt;
            //    }
            //    else
            //    {
            //        ProcessLogicData(dt);
            //        for (j = 0; j < dt.Rows.Count; j++)
            //        {
            //            status = dt.Rows[j]["reject"].ObjToString().Trim().ToUpper();
            //            if (String.IsNullOrWhiteSpace(status))
            //            {
            //                companyCode = dt.Rows[j]["companyCode"].ObjToString();
            //                dt.Rows[j]["Report"] = report;
            //                finalDt.ImportRow(dt.Rows[j]);
            //                addCount++;
            //            }
            //        }
            //        idx = finalDt.Rows.Count - 1;
            //        if (idx >= 0)
            //            finalDt.Rows[idx]["finale"] = "YES";
            //        dt = finalDt;
            //    }
            //}

            int newRows = dt.Rows.Count;


            string fname = "";
            string lname = "";
            string name = "";
            string oldPayer = "";
            //string payer = "";
            string oldPayerName = "";
            string payerName = "";
            string policyName = "";
            string groupNumber = "";
            string policyNumber = "";
            double paymentAmount = 0D;
            double premium = 0D;
            amtOfMonthlyPayt = 0D;
            double totalPremiums = 0D;
            double newTotalPremiums = 0D;
            double newPayment = 0D;
            double balanceDue = 0D;
            double nowDue = 0D;
            double months = 0D;
            DateTime deceasedDate = DateTime.Now;
            string oldReport = "";
            int lastRow = -1;
            idx = 0;
            bool first = true;

            Trust85.FindContract(dt, "ZZ0001560");
            double monthlyPremium = 0D;
            double historicPremium = 0D;
            double monthlySecNat = 0D;
            double monthly3rdParty = 0D;
            double annual = 0D;
            double nonSecNatPremium = 0D;
            bool gotSecNat = false;
            bool got3rdParty = false;

            for (i = 0; i < dt.Rows.Count; i++)
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                if (contractNumber == "ZZ0004510")
                {
                }
                groupNumber = dt.Rows[i]["groupNumber"].ObjToString();
                idx = groupNumber.IndexOf('-');
                if (idx > 0)
                {
                    groupNumber = groupNumber.Substring(0, idx);
                    groupNumber = groupNumber.Replace("-", "");
                    dt.Rows[i]["groupNumber"] = groupNumber;
                }
                report = dt.Rows[i]["report"].ObjToString();
                if (String.IsNullOrWhiteSpace(report))
                    report = "No Report";
                if (String.IsNullOrWhiteSpace(oldReport))
                    oldReport = report;
                if (oldReport != report)
                {
                    if (i > 0)
                        dt.Rows[i - 1]["finale"] = "YES";
                }
                oldReport = report;
                payerName = dt.Rows[i]["payerName"].ObjToString();
                payer = dt.Rows[i]["payer"].ObjToString();
                if ( payer == "UC-1521")
                {
                }
                //                name = report + ": " + payerName + " (" + payer + ") " + groupNumber;
                name = report + ": " + payerName + " (" + payer + ")";
                dt.Rows[i]["majorBreak"] = name;
                balanceDue = dt.Rows[i]["balanceDue"].ObjToDouble();
                nowDue = dt.Rows[i]["nowDue"].ObjToDouble();
                paymentAmount = dt.Rows[i]["paymentAmount"].ObjToDouble();
                amtOfMonthlyPayt = dt.Rows[i]["amtOfMonthlyPayt"].ObjToDouble();
                payDate8 = dt.Rows[i]["payDate8"].ObjToDateTime();
                //amtOfMonthlyPayt = Policies.CalcMonthlyPremium(payer, DateTime.Now);
                companyCode = dt.Rows[i]["companyCode"].ObjToString();
                gotSecNat = CustomerDetails.isSecNat(companyCode);
                got3rdParty = false;
                if ( !gotSecNat )
                {
                    if (report.ToUpper() != "NOT THIRD PARTY")
                        got3rdParty = true;
                }
                //if (gotSecNat)
                //    amtOfMonthlyPayt = Policies.CalcMonthlyPremium(payer, DateTime.Now);
                if (balanceDue <= 0D && nowDue >= 0D)
                    dt.Rows[i]["balanceDue"] = nowDue;
                if (amtOfMonthlyPayt > 1000D)
                    dt.Rows[i]["amtOfMonthlyPayt"] = dt.Rows[i]["balanceDue"].ObjToDouble();
                if (payer == "EV-090548")
                {
                }
                if (payer == "180680")
                {
                }
                if (!chkSecNat.Checked && !chk3rdParty.Checked )
                {
                    if (gotSecNat && got3rdParty )
                    {
                        CustomerDetails.CalcMonthlyPremium(payer, ref monthlyPremium, ref historicPremium, ref monthlySecNat, ref monthly3rdParty );
                        nonSecNatPremium = monthlyPremium - monthlySecNat;
                        if (payDate8 > DailyHistory.kill3rdPartyDate)
                            nonSecNatPremium = monthlyPremium - monthlySecNat - monthly3rdParty;
                        nonSecNatPremium = G1.RoundValue(nonSecNatPremium);
                        dt.Rows[i]["premiumNow"] = nonSecNatPremium;

                        double remainder = paymentAmount % nonSecNatPremium;
                        remainder = G1.RoundValue(remainder);
                        if (remainder == 0D)
                            dt.Rows[i]["paymentAmount"] = 0D;

                        remainder = paymentAmount % amtOfMonthlyPayt;
                        remainder = G1.RoundValue(remainder);
                        if (remainder == 0D)
                            dt.Rows[i]["paymentAmount"] = 0D;

                        if (amtOfMonthlyPayt == 0D)
                            dt.Rows[i]["amtOfMonthlyPayt"] = monthlySecNat;
                        else if (G1.WithInPenny(paymentAmount, amtOfMonthlyPayt))
                            dt.Rows[i]["paymentAmount"] = 0D;
                        else if (G1.WithInPenny(paymentAmount, monthlyPremium))
                            dt.Rows[i]["amtOfMonthlyPayt"] = monthlyPremium;
                        else
                        {
                            annual = amtOfMonthlyPayt * 12D * 0.95D;
                            annual = G1.RoundValue(annual);
                            if (paymentAmount == annual)
                                dt.Rows[i]["paymentAmount"] = 0D;
                            else
                            {
                                annual = amtOfMonthlyPayt * 12D;
                                annual = G1.RoundValue(annual);
                                if (paymentAmount == annual)
                                    dt.Rows[i]["paymentAmount"] = 0D;
                            }
                        }
                    }
                    else
                    {
                        CustomerDetails.CalcMonthlyPremium(payer, ref monthlyPremium, ref historicPremium, ref monthlySecNat, ref monthly3rdParty );
                        nonSecNatPremium = monthlyPremium - monthlySecNat;
                        if (payDate8 > DailyHistory.kill3rdPartyDate)
                            nonSecNatPremium = monthlyPremium - monthlySecNat - monthly3rdParty;
                        nonSecNatPremium = G1.RoundValue(nonSecNatPremium);
                        dt.Rows[i]["premiumNow"] = nonSecNatPremium;
                        if (gotSecNat)
                        {
                            double remainder = paymentAmount % nonSecNatPremium;
                            remainder = G1.RoundValue(remainder);
                            if (remainder == 0D)
                                dt.Rows[i]["paymentAmount"] = 0D;

                            remainder = paymentAmount % amtOfMonthlyPayt;
                            remainder = G1.RoundValue(remainder);
                            if (remainder == 0D)
                                dt.Rows[i]["paymentAmount"] = 0D;

                            if (amtOfMonthlyPayt == 0D)
                                dt.Rows[i]["amtOfMonthlyPayt"] = monthlySecNat;
                            else if (G1.WithInPenny(paymentAmount, amtOfMonthlyPayt))
                                dt.Rows[i]["paymentAmount"] = amtOfMonthlyPayt;
                            else if (G1.WithInPenny(paymentAmount, monthlyPremium))
                                dt.Rows[i]["amtOfMonthlyPayt"] = monthlyPremium;
                            else
                            {
                                annual = amtOfMonthlyPayt * 12D * 0.95D;
                                annual = G1.RoundValue(annual);
                                if (paymentAmount == annual)
                                    dt.Rows[i]["paymentAmount"] = 0D;
                                else
                                {
                                    annual = amtOfMonthlyPayt * 12D;
                                    annual = G1.RoundValue(annual);
                                    if (paymentAmount == annual)
                                        dt.Rows[i]["paymentAmount"] = 0D;
                                }
                            }
                        }
                        else if ( got3rdParty && payDate8 > DailyHistory.kill3rdPartyDate )
                        {
                            double remainder = paymentAmount % nonSecNatPremium;
                            remainder = G1.RoundValue(remainder);
                            if (remainder == 0D)
                                dt.Rows[i]["paymentAmount"] = 0D;

                            remainder = paymentAmount % amtOfMonthlyPayt;
                            remainder = G1.RoundValue(remainder);
                            if (remainder == 0D)
                                dt.Rows[i]["paymentAmount"] = 0D;

                            if (amtOfMonthlyPayt == 0D)
                                dt.Rows[i]["amtOfMonthlyPayt"] = monthly3rdParty;
                            else if (G1.WithInPenny(paymentAmount, amtOfMonthlyPayt))
                                dt.Rows[i]["paymentAmount"] = amtOfMonthlyPayt;
                            else if (G1.WithInPenny(paymentAmount, monthlyPremium))
                                dt.Rows[i]["amtOfMonthlyPayt"] = monthlyPremium;
                            else
                            {
                                annual = amtOfMonthlyPayt * 12D * 0.95D;
                                annual = G1.RoundValue(annual);
                                if (paymentAmount == annual)
                                    dt.Rows[i]["paymentAmount"] = 0D;
                                else
                                {
                                    annual = amtOfMonthlyPayt * 12D;
                                    annual = G1.RoundValue(annual);
                                    if (paymentAmount == annual)
                                        dt.Rows[i]["paymentAmount"] = 0D;
                                }
                            }
                        }
                        else
                        {
                            if (amtOfMonthlyPayt == 0D)
                            {
                                dt.Rows[i]["amtOfMonthlyPayt"] = monthlySecNat;
                                if (monthlySecNat == 0D)
                                    dt.Rows[i]["amtOfMonthlyPayt"] = monthly3rdParty;
                                if ( payDate8 > DailyHistory.kill3rdPartyDate )
                                    dt.Rows[i]["amtOfMonthlyPayt"] = monthlySecNat + monthly3rdParty;
                            }
                            else if (G1.WithInPenny(paymentAmount, monthlyPremium))
                                dt.Rows[i]["amtOfMonthlyPayt"] = monthlyPremium;
                            else if (G1.WithInPenny(paymentAmount, amtOfMonthlyPayt))
                                dt.Rows[i]["paymentAmount"] = amtOfMonthlyPayt;
                            else
                            {
                                annual = amtOfMonthlyPayt * 12D * 0.95D;
                                annual = G1.RoundValue(annual);
                                if (paymentAmount == annual)
                                    dt.Rows[i]["paymentAmount"] = 0D;
                                else
                                {
                                    annual = amtOfMonthlyPayt * 12D;
                                    annual = G1.RoundValue(annual);
                                    if (paymentAmount == annual)
                                        dt.Rows[i]["paymentAmount"] = 0D;
                                }
                            }
                        }
                    }
                }
                if (chkSecNat.Checked && chkHonor.Checked)
                {
                    CustomerDetails.CalcMonthlyPremium(payer, ref monthlyPremium, ref historicPremium, ref monthlySecNat, ref monthly3rdParty );
                    nonSecNatPremium = monthlyPremium - monthlySecNat;
                    nonSecNatPremium = G1.RoundValue(nonSecNatPremium);
                    double remainder = paymentAmount % nonSecNatPremium;
                    remainder = G1.RoundValue(remainder);
                    if (remainder == 0D)
                        dt.Rows[i]["paymentAmount"] = 0D;

                    remainder = paymentAmount % amtOfMonthlyPayt;
                    remainder = G1.RoundValue(remainder);
                    if (remainder == 0D)

                        dt.Rows[i]["paymentAmount"] = 0D;
                    if (amtOfMonthlyPayt == 0D)
                        dt.Rows[i]["amtOfMonthlyPayt"] = monthlySecNat;
                    else if (paymentAmount == nonSecNatPremium)
                        dt.Rows[i]["paymentAmount"] = 0D;
                    else if (G1.WithInPenny(paymentAmount, amtOfMonthlyPayt))
                        dt.Rows[i]["paymentAmount"] = 0D;
                    else if (G1.WithInPenny(paymentAmount, monthlyPremium))
                        dt.Rows[i]["amtOfMonthlyPayt"] = monthlyPremium;
                    else
                    {
                        annual = amtOfMonthlyPayt * 12D * 0.95D;
                        annual = G1.RoundValue(annual);
                        if (paymentAmount == annual)
                            dt.Rows[i]["paymentAmount"] = 0D;
                        else
                        {
                            annual = amtOfMonthlyPayt * 12D;
                            annual = G1.RoundValue(annual);
                            if (paymentAmount == annual)
                                dt.Rows[i]["paymentAmount"] = 0D;
                        }

                    }
                }
                if (chk3rdParty.Checked && chkHonor.Checked)
                {
                    CustomerDetails.CalcMonthlyPremium(payer, ref monthlyPremium, ref historicPremium, ref monthlySecNat, ref monthly3rdParty);
                    nonSecNatPremium = monthlyPremium;
                    if ( payDate8 > DailyHistory.kill3rdPartyDate)
                        nonSecNatPremium = monthlyPremium - monthly3rdParty;
                    nonSecNatPremium = G1.RoundValue(nonSecNatPremium);
                    double remainder = paymentAmount % nonSecNatPremium;
                    remainder = G1.RoundValue(remainder);
                    if (remainder == 0D)
                        dt.Rows[i]["paymentAmount"] = 0D;

                    remainder = paymentAmount % amtOfMonthlyPayt;
                    remainder = G1.RoundValue(remainder);
                    if (remainder == 0D)

                        dt.Rows[i]["paymentAmount"] = 0D;
                    if (amtOfMonthlyPayt == 0D)
                        dt.Rows[i]["amtOfMonthlyPayt"] = monthly3rdParty;
                    else if (paymentAmount == nonSecNatPremium)
                        dt.Rows[i]["paymentAmount"] = 0D;
                    else if (G1.WithInPenny(paymentAmount, amtOfMonthlyPayt))
                        dt.Rows[i]["paymentAmount"] = 0D;
                    else if (G1.WithInPenny(paymentAmount, monthlyPremium))
                        dt.Rows[i]["amtOfMonthlyPayt"] = monthlyPremium;
                    else
                    {
                        annual = amtOfMonthlyPayt * 12D * 0.95D;
                        annual = G1.RoundValue(annual);
                        if (paymentAmount == annual)
                            dt.Rows[i]["paymentAmount"] = 0D;
                        else
                        {
                            annual = amtOfMonthlyPayt * 12D;
                            annual = G1.RoundValue(annual);
                            if (paymentAmount == annual)
                                dt.Rows[i]["paymentAmount"] = 0D;
                        }

                    }
                }
            }

            //            string contractNumber = "";

            DateTime pDate = DateTime.Now;
            DateTime dDate = DateTime.Now;
            double savePaymentAmount = 0D;
            for (i = 0; i < dt.Rows.Count; i++)
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                if (contractNumber == "ZZ0004510")
                {
                }
                policyNumber = dt.Rows[i]["policyNumber"].ObjToString();
                if (policyNumber == "CG-WL9285")
                {

                }
                policyName = dt.Rows[i]["policyName"].ObjToString();
                dt.Rows[i]["customer"] = policyName;

                groupNumber = dt.Rows[i]["groupNumber"].ObjToString();
                policyNumber = dt.Rows[i]["policyNumber"].ObjToString();

                payer = dt.Rows[i]["payer"].ObjToString();
                payerName = dt.Rows[i]["payerName"].ObjToString();
                payment = dt.Rows[i]["paymentAmount"].ObjToDouble();

                if (payer != oldPayer || payerName != oldPayerName || payment > 0D)
                {
                    if (payer == "EV-090548")
                    {
                    }
                    companyCode = dt.Rows[i]["companyCode"].ObjToString();
                    gotSecNat = CustomerDetails.isSecNat(companyCode);
                    oldPayer = payer;
                    oldPayerName = payerName;
                    savePaymentAmount = paymentAmount;
                    paymentAmount = dt.Rows[i]["paymentAmount"].ObjToDouble();
                    amtOfMonthlyPayt = dt.Rows[i]["amtOfMonthlyPayt"].ObjToDouble();
                    //amtOfMonthlyPayt = Policies.CalcMonthlyPremium(payer, DateTime.Now);
                    if (amtOfMonthlyPayt >= 40404D || amtOfMonthlyPayt == 0D)
                    {
                        amtOfMonthlyPayt = GetProperPaymentAmount(oldPayer, policyNumber, contractNumber);
                    }
                    months = 0D;
                    if (paymentAmount > 0D && amtOfMonthlyPayt > 0D)
                    {
                        pDate = dt.Rows[i]["payDate8"].ObjToDateTime();
                        dDate = dt.Rows[i]["dueDate8"].ObjToDateTime();
                        months = paymentAmount / amtOfMonthlyPayt;
                        months = DailyHistory.CheckMonthsForInsurance(contractNumber, payer, amtOfMonthlyPayt, paymentAmount, pDate, dDate);
                    }

                    totalPremiums = 0D;
                    lastRow = i;
                    first = true;
                    DateTime timeStamp = DateTime.Now;
                    DateTime oldStamp = DateTime.Now;
                    for (j = i; j < dt.Rows.Count; j++)
                    {
                        try
                        {
                            payer = dt.Rows[j]["payer"].ObjToString();

                            //                            payerName = dt.Rows[j]["lastName"].ObjToString() + ", " + dt.Rows[j]["firstName"].ObjToString();
                            payerName = dt.Rows[j]["payerName"].ObjToString();
                            if (payer != oldPayer || payerName != oldPayerName)
                                break;
                            //payment = dt.Rows[j]["paymentAmount"].ObjToDouble();
                            //if (payment > 0D && j > i )
                            //    break;
                            deceasedDate = dt.Rows[j]["deceasedDate2"].ObjToDateTime();
                            if (deceasedDate.Year > 1001)
                                continue;
                            premium = dt.Rows[j]["premium"].ObjToDouble();
                            totalPremiums += premium;
                            timeStamp = dt.Rows[j]["tmstamp"].ObjToDateTime();
                            //if (first)
                            //    oldStamp = timeStamp;
                            lastRow = j;
                            //if (!first)
                            if (oldStamp == timeStamp)
                                dt.Rows[j]["paymentAmount"] = 0D;
                            dt.Rows[j]["months"] = months;
                            first = false;
                            oldStamp = timeStamp;
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                    newTotalPremiums = 0D;
                    totalPremiums = G1.RoundValue(totalPremiums);
                    if (paymentAmount == 18.18D)
                    {

                    }
                    int imonths = 0;
                    for (j = i; j <= lastRow; j++)
                    {
                        try
                        {
                            deceasedDate = dt.Rows[j]["deceasedDate2"].ObjToDateTime();
                            if (deceasedDate.Year > 1001)
                                continue;
                            premium = dt.Rows[j]["premium"].ObjToDouble();
                            if (premium <= 0D)
                                continue;
                            if (totalPremiums <= 0D)
                                continue;
                            if (paymentAmount <= 0D)
                                continue;
                            newPayment = premium / totalPremiums * paymentAmount;
                            months = dt.Rows[j]["months"].ObjToDouble();
                            imonths = Convert.ToInt32(months);
                            months = (double)imonths;
                            dt.Rows[j]["months"] = months;
                            newPayment = premium * months;
                            newTotalPremiums += newPayment;
                            dt.Rows[j]["Paid"] = newPayment;
                            dt.Rows[j]["customer"] = dt.Rows[j]["policyName"].ObjToString();
                            dt.Rows[j]["balanceDue"] = newPayment;
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                    //                    break;
                }
            }

            if (chkHonor.Checked)
            {
                dt = CustomerDetails.filterSecNat(chkSecNat.Checked, dt);
                //DataRow[] dR = dt.Select("report='NOT THIRD PARTY'");
                //if (dR.Length > 0)
                //    dt = dR.CopyToDataTable();
            }

            if (chkExcludeZeros.Checked)
            {
                oldReport = "";
                report = "";
                bool found = false;
                for (i = (dt.Rows.Count - 1); i >= 0; i--)
                {
                    payer = dt.Rows[i]["payer"].ObjToString();
                    if (payer == "EV-090548")
                    {
                    }
                    report = dt.Rows[i]["report"].ObjToString();
                    if (String.IsNullOrWhiteSpace(oldReport))
                        oldReport = report;
                    if (oldReport != report)
                    {
                        found = false;
                        oldReport = report;
                    }
                    name = dt.Rows[i]["finale"].ObjToString();
                    if (name.ToUpper() == "YES")
                        dt.Rows[i]["policyLapsed"] = "XXX";
                    if (name.ToUpper() == "YES" && !found )
                        continue;
                    paymentAmount = dt.Rows[i]["Paid"].ObjToDouble();
                    if (paymentAmount <= 0D)
                        dt.Rows.RemoveAt(i);
                    else
                        found = true;
                }
            }
            else
            {
                for (i = (dt.Rows.Count - 1); i >= 0; i--)
                {
                    name = dt.Rows[i]["finale"].ObjToString();
                    if (name.ToUpper() == "YES")
                        dt.Rows[i]["policyLapsed"] = "XXX";
                }
            }

            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private double GetProperPaymentAmount( string payer, string policyNumber, string contractNumber)
        {
            double payment = 0D;
            double amtOfMonthlyPayt = 0D;
            string cmd = "SELECT * from `icustomers` where `payer`= '" + payer + "';";
            DataTable ddx = G1.get_db_data(cmd);
            if (ddx.Rows.Count > 0)
            {
                string list = "";
                for (int i = 0; i < ddx.Rows.Count; i++)
                {
                    string contract = ddx.Rows[i]["contractNumber"].ObjToString();
                    list += "'" + contract + "',";
                }
                list = list.TrimEnd(',');
                list = "(" + list + ")";
                cmd = "Select * from `icontracts` where `contractNumber` IN " + list + ";";
                ddx = G1.get_db_data(cmd);
                for ( int i=0; i<ddx.Rows.Count; i++)
                {
                    amtOfMonthlyPayt = ddx.Rows[i]["amtOfMonthlyPayt"].ObjToDouble();
                    if ( amtOfMonthlyPayt != 40404D )
                    {
                        payment = amtOfMonthlyPayt;
                        break;
                    }
                }
            }
            if ( payment <= 0D || payment >= 40404D)
            {
                cmd = "Select * from `policies` where `contractNumber` = '" + contractNumber + "' and `payer` = '" + payer + "' and `policyNumber` = '" + policyNumber + "';";
                ddx = G1.get_db_data(cmd);
                if (ddx.Rows.Count > 0)
                    payment = ddx.Rows[0]["premium"].ObjToDouble();
            }
            payment = Policies.CalcMonthlyPremium(payer, DateTime.Now);

            return payment;
        }
        /****************************************************************************************/
        private void chkCompany_CheckedChanged(object sender, EventArgs e)
        {
            gridMain.Columns["majorBreak"].GroupIndex = -1;
            gridMain.Columns["payerName"].GroupIndex = -1;
            gridMain.Columns["payer"].GroupIndex = -1;
            gridMain.Columns["groupNumber"].GroupIndex = -1;
            gridMain.Columns["customer"].GroupIndex = -1;
            gridMain.OptionsBehavior.AutoExpandAllGroups = false;
            gridMain.CollapseAllGroups();
            gridMain.OptionsPrint.ExpandAllGroups = false;
            gridMain.OptionsPrint.PrintGroupFooter = true;
            gridMain.Columns["report"].Visible = true;
            gridMain.Columns["majorBreak"].Visible = true;
            gridMain.Columns["payer"].Visible = true;
            gridMain.Columns["payerName"].Visible = true;
            gridMain.Columns["groupNumber"].Visible = true;
            if ( chkCompany.Checked )
            {
                gridMain.Columns["report"].GroupIndex = 0;
//                gridMain.Columns["companyCode"].GroupIndex = 1;
//                gridMain.Columns["payerName"].GroupIndex = 1;
                gridMain.Columns["majorBreak"].GroupIndex = 1;
                gridMain.OptionsBehavior.AutoExpandAllGroups = true;
                gridMain.ExpandAllGroups();
                gridMain.OptionsPrint.ExpandAllGroups = true;
                gridMain.OptionsPrint.PrintGroupFooter = true;
                gridMain.Columns["companyCode"].Visible = false;
                gridMain.Columns["payer"].Visible = false;
                gridMain.Columns["payerName"].Visible = false;
                gridMain.Columns["groupNumber"].Visible = false;
                gridMain.Columns["majorBreak"].Visible = false;
                gridMain.Columns["report"].Visible = false;
            }
            else
            {
                gridMain.Columns["report"].GroupIndex = -1;
                gridMain.Columns["companyCode"].GroupIndex = -1;
                gridMain.Columns["payerName"].GroupIndex = -1;
                gridMain.OptionsBehavior.AutoExpandAllGroups = false;
                gridMain.CollapseAllGroups();
                gridMain.OptionsPrint.ExpandAllGroups = false;
                gridMain.OptionsPrint.PrintGroupFooter = true;
                gridMain.Columns["companyCode"].Visible = true;
            }
        }
        /****************************************************************************************/
        private void chkGroupData_CheckedChanged(object sender, EventArgs e)
        {
            gridMain.Columns["companyCode"].GroupIndex = -1;
            gridMain.OptionsBehavior.AutoExpandAllGroups = false;
            gridMain.CollapseAllGroups();
            gridMain.OptionsPrint.ExpandAllGroups = false;
            gridMain.OptionsPrint.PrintGroupFooter = true;
            gridMain.Columns["companyCode"].Visible = true;

            if (chkGroupData.Checked)
            {
                gridMain.Columns["majorBreak"].GroupIndex = 0;
                //gridMain.Columns["payerName"].GroupIndex = 0;
                //gridMain.Columns["payer"].GroupIndex = 1;
                //gridMain.Columns["groupNumber"].GroupIndex = 2;
                gridMain.OptionsBehavior.AutoExpandAllGroups = true;
                gridMain.ExpandAllGroups();
                gridMain.OptionsPrint.ExpandAllGroups = true;
                gridMain.OptionsPrint.PrintGroupFooter = true;
                gridMain.Columns["majorBreak"].Visible = false;
                gridMain.Columns["payer"].Visible = false;
                gridMain.Columns["payerName"].Visible = false;
                gridMain.Columns["groupNumber"].Visible = false;
            }
            else
            {
                gridMain.Columns["majorBreak"].GroupIndex = -1;
                gridMain.Columns["payerName"].GroupIndex = -1;
                gridMain.Columns["payer"].GroupIndex = -1;
                gridMain.Columns["groupNumber"].GroupIndex = -1;
                gridMain.OptionsBehavior.AutoExpandAllGroups = false;
                gridMain.CollapseAllGroups();
                gridMain.OptionsPrint.ExpandAllGroups = false;
                gridMain.OptionsPrint.PrintGroupFooter = true;
                gridMain.Columns["majorBreak"].Visible = true;
                gridMain.Columns["payer"].Visible = true;
                gridMain.Columns["payerName"].Visible = true;
                gridMain.Columns["groupNumber"].Visible = true;
            }
            gridMain.RefreshData();
            dgv.Refresh();
        }
        /****************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (dgv.Visible)
                SetSpyGlass(gridMain);
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
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string contract = dr["contractNumber"].ObjToString();
            if (!String.IsNullOrWhiteSpace(contract))
            {
                this.Cursor = Cursors.WaitCursor;
                DataTable dt = (DataTable)dgv.DataSource;
                G1.UpdatePreviousCustomer(contract, LoginForm.username);
                bool insurance = false;
                if (contract.ToUpper().IndexOf("ZZ") == 0)
                    insurance = true;
                if (contract.ToUpper().IndexOf("MM") == 0)
                    insurance = true;
                if (contract.ToUpper().IndexOf("OO") == 0)
                    insurance = true;
                if (insurance)
                {
                    string cmd = "Select * from `policies` p JOIN `icustomers` d ON p.`contractNumber` = d.`contractNumber` JOIN `icontracts` x ON p.`contractNumber` = x.`contractNumber` ";
                    cmd += " WHERE p.`contractNumber` = '" + contract + "' ";

                    DataTable dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                    {
                        Policies policyForm = new Policies(contract);
                        policyForm.Show();
                    }
                    else
                    {
                        CustomerDetails clientForm = new CustomerDetails(contract);
                        clientForm.Show();
                    }
                }
                else
                {
                    CustomerDetails clientForm = new CustomerDetails(contract);
                    clientForm.Show();
                }
                this.Cursor = Cursors.Default;
            }
        }
        /***********************************************************************************************/
        private void GetWeeklyDate(DateTime date, string direction)
        {
            loading = true;
            DateTime idate = date;
            if (direction == "BACK")
            {
                date = date.AddDays(-7);
                this.dateTimePicker2.Value = date;
                date = date.AddDays(-4);
                this.dateTimePicker1.Value = date;
            }
            else
            {
                date = date.AddDays(7);
                this.dateTimePicker2.Value = date;
                date = date.AddDays(-4);
                this.dateTimePicker1.Value = date;

            }
        }
        /****************************************************************************************/
        private void btnRight_Click(object sender, EventArgs e)
        {
            GetWeeklyDate(this.dateTimePicker2.Value, "FORWARD");
            this.dateTimePicker3.Value = this.dateTimePicker1.Value;
            this.dateTimePicker4.Value = this.dateTimePicker2.Value;
        }
        /****************************************************************************************/
        private void btnLeft_Click(object sender, EventArgs e)
        {
            GetWeeklyDate(this.dateTimePicker2.Value, "BACK");
            this.dateTimePicker3.Value = this.dateTimePicker1.Value;
            this.dateTimePicker4.Value = this.dateTimePicker2.Value;
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
        /****************************************************************************************/
        private void printPreviewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            footerCount = 0;
            lastReport = "";
            printLines = 0;
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

            printingSystem1.Document.AutoFitToPagesWidth = 1;

            G1.PrintPreview(printableComponentLink1, gridMain);

            //printableComponentLink1.CreateDocument();
            //printableComponentLink1.ShowPreview();
        }
        /****************************************************************************************/
        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            footerCount = 0;
            lastReport = "";
            printLines = 0;
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

            printingSystem1.Document.AutoFitToPagesWidth = 1;

            printableComponentLink1.CreateDocument();
            printableComponentLink1.PrintDlg();
        }
        /***********************************************************************************************/
        private void printableComponentLink1_BeforeCreateAreas(object sender, EventArgs e)
        {
//            footerCount = 0;
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
            string report = "Collection Report for ";
            DateTime date = this.dateTimePicker1.Value;
            report += date.ToString("MM/dd/yyyy") + " - ";
            date = this.dateTimePicker2.Value;
            report += date.ToString("MM/dd/yyyy");
            Printer.DrawQuad(5, 8, 8, 4, report, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            if ( !chkHonor.Checked )
                Printer.DrawQuad(6, 5, 4, 4, "Full Report", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            else if ( chkSecNat.Checked && chkHonor.Checked )
                Printer.DrawQuad(6, 5, 4, 4, "SecNat Report", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            else if (chkHonor.Checked)
                Printer.DrawQuad(6, 5, 4, 4, "Exclude SecNat Report", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            //font = new Font("Ariel", 8, FontStyle.Regular);
            //report = cmbWhat.Text + " / " + cmbWho.Text + " / " + cmbDeposits.Text;
            //Printer.DrawQuad(10, 8, 2, 4, report, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            //            Printer.DrawQuadTicks();
        }
        /****************************************************************************************/
        private void cmbReport_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
        }
        /****************************************************************************************/
        private void ProcessLogicData ( DataTable dt, string location = "")
        {
            if ( String.IsNullOrWhiteSpace ( location ))
                location = cmbReport.Text;
            if (string.IsNullOrWhiteSpace(location))
            {
                MessageBox.Show("***ERROR*** You must select a location!");
                return;
            }

            if (G1.get_column_number(logicDt, "fail") < 0)
                logicDt.Columns.Add("fail");
            if (G1.get_column_number(logicDt, "major") < 0)
                logicDt.Columns.Add("major");
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["reject"] = "";
            string payer = "";
            string company = "";
            string oldAgent = "";
            string agent = "";
            string uCode = "";
            bool reject = false;
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                try
                {
                    agent = dt.Rows[i]["agentCode"].ObjToString();
                    payer = dt.Rows[i]["payer"].ObjToString();
                    if ( payer == "BB-1061")
                    {

                    }
                    company = dt.Rows[i]["companyCode"].ObjToString();
                    uCode = dt.Rows[i]["ucode"].ObjToString();
                    oldAgent = dt.Rows[i]["oldAgentInfo"].ObjToString();
                    reject = ProcessLogic(location, company, oldAgent, agent, uCode);
                    if (reject)
                        dt.Rows[i]["reject"] = "Y";
                    else
                        dt.Rows[i]["reject"] = "";
                }
                catch ( Exception ex)
                {

                }
            }
            //string status = "";
            //for ( int i=dt.Rows.Count-1; i>=0; i--)
            //{
            //    status = dt.Rows[i]["reject"].ObjToString();
            //    if (status == "Y")
            //        dt.Rows.RemoveAt(i);
            //}
        }
        /****************************************************************************************/
        private void ClearAllFail ()
        {
            if (G1.get_column_number(logicDt, "fail") < 0)
                logicDt.Columns.Add("fail");
            for (int i = 0; i < logicDt.Rows.Count; i++)
            {
                logicDt.Rows[i]["fail"] = "";
                logicDt.Rows[i]["company"] = "";
                logicDt.Rows[i]["oldAgent"] = "";
                logicDt.Rows[i]["agent"] = "";
                logicDt.Rows[i]["ucode"] = "";
            }
        }
        /****************************************************************************************/
        private void GetLogic ( DataTable dt, int i, ref string location, ref string and_or, ref string Operator, ref string C1, ref string C2, ref string C3, ref string C4, ref string O1, ref string O2, ref string O3, ref string A1, ref string A2, ref string A3, ref string U1)
        {
            location = dt.Rows[i]["location"].ObjToString();
            and_or = dt.Rows[i]["and_or"].ObjToString();
            Operator = dt.Rows[i]["operator"].ObjToString();
            C1 = dt.Rows[i]["C1"].ObjToString();
            C2 = dt.Rows[i]["C2"].ObjToString();
            C3 = dt.Rows[i]["C3"].ObjToString();
            C4 = dt.Rows[i]["C4"].ObjToString();
            O1 = dt.Rows[i]["O1"].ObjToString();
            O2 = dt.Rows[i]["O2"].ObjToString();
            O3 = dt.Rows[i]["O3"].ObjToString();
            A1 = dt.Rows[i]["A1"].ObjToString();
            A2 = dt.Rows[i]["A2"].ObjToString();
            A3 = dt.Rows[i]["A3"].ObjToString();
            U1 = dt.Rows[i]["U1"].ObjToString();
        }
        /****************************************************************************************/
        private bool ProcessLogic ( string location, string company, string oldAgent, string agent, string ucode)
        {
            bool reject = false;

            string loc = "";
            string and_or = "";
            string Operator = "";
            string C1 = "";
            string C2 = "";
            string C3 = "";
            string C4 = "";
            string O1 = "";
            string O2 = "";
            string O3 = "";
            string A1 = "";
            string A2 = "";
            string A3 = "";
            string U1 = "";

            string saveLoc = "";

            bool R_C1 = false;
            bool R_C2 = false;
            bool R_C3 = false;
            bool R_C4 = false;
            bool R_O1 = false;
            bool R_O2 = false;
            bool R_O3 = false;
            bool R_A1 = false;
            bool R_A2 = false;
            bool R_A3 = false;
            bool R_U1 = false;
            ClearAllFail();

            int firstIndex = 0;
            bool[] passFail = new bool[100];
            int passFailCount = 0;
            bool open = false;

            int i = 0;

            for (i = 0; i < logicDt.Rows.Count; i++)
            {
                try
                {
                    R_C1 = false;
                    R_C2 = false;
                    R_C3 = false;
                    R_C4 = false;
                    R_O1 = false;
                    R_O2 = false;
                    R_O3 = false;
                    R_A1 = false;
                    R_A2 = false;
                    R_A3 = false;
                    R_U1 = false;

                    logicDt.Rows[i]["company"] = company;
                    logicDt.Rows[i]["oldAgent"] = oldAgent;
                    logicDt.Rows[i]["agent"] = agent;
                    logicDt.Rows[i]["ucode"] = ucode;

                    GetLogic(logicDt, i, ref loc, ref and_or, ref Operator, ref C1, ref C2, ref C3, ref C4, ref O1, ref O2, ref O3, ref A1, ref A2, ref A3, ref U1);
                    if (String.IsNullOrWhiteSpace(saveLoc) && !String.IsNullOrWhiteSpace(loc))
                        saveLoc = loc;
                    if (!String.IsNullOrWhiteSpace(saveLoc) && String.IsNullOrWhiteSpace(loc))
                        loc = saveLoc;
                    if (loc == "All Third Party")
                        continue;
                    if (loc != "All Third Party")
                    {
                        if (loc != location)
                        {
                            if ((firstIndex + 1) < i)
                            {
                                passFail[passFailCount] = CheckPassFail(firstIndex, i);
                                passFailCount++;
                                open = false;
                                //company = "XX";
                                //agent = "X";
                            }
                            saveLoc = loc;
                            firstIndex = i;
                            continue;
                        }
                        saveLoc = loc;
                    }
                    if (String.IsNullOrWhiteSpace(loc))
                        break;
                    open = true;
                    R_C1 = CheckLogic(company, 1, Operator, C1);
                    R_C2 = CheckLogic(company, 2, Operator, C2);
                    R_C3 = CheckLogic(company, 3, Operator, C3);
                    R_C4 = CheckLogic(company, 4, Operator, C4);

                    R_O1 = CheckLogic(oldAgent, 1, Operator, O1);
                    R_O2 = CheckLogic(oldAgent, 2, Operator, O2);
                    R_O3 = CheckLogic(oldAgent, 3, Operator, O3);

                    R_A1 = CheckLogic(agent, 1, Operator, A1);
                    R_A2 = CheckLogic(agent, 2, Operator, A2);
                    R_A3 = CheckLogic(agent, 3, Operator, A3);

                    R_U1 = CheckLogic(ucode, 1, Operator, U1);

                    if (R_C1 || R_C2 || R_C3 || R_C3 || R_O1 || R_O2 || R_O3 || R_A1 || R_A2 || R_A3 || R_U1)
                        logicDt.Rows[i]["fail"] = "fail";
                    else
                        logicDt.Rows[i]["fail"] = "accept";
                }
                catch (Exception ex)
                {

                }
            }
            if (open)
            {
                passFail[passFailCount] = CheckPassFail(firstIndex, logicDt.Rows.Count);
                passFailCount++;
            }
            bool secondMethod = false;
            if (chkSecondMethod.Checked)
                secondMethod = true;
            if (!secondMethod)
            {
                reject = false;
                for (i = 0; i < passFailCount; i++)
                {
                    if (!passFail[i])
                    {
                        reject = true;
                        break;
                    }
                }
            }
            else
            {
                reject = true;
                for (i = 0; i < passFailCount; i++)
                {
                    if (passFail[i])
                    {
                        reject = false;
                        break;
                    }
                }
            }
            return reject;
        }
        /****************************************************************************************/
        private bool CheckPassFail ( int start, int stop )
        {
            bool pass = true;
            string status = "";
            string and_or = "";
            string last = "AND";
            string save = "";
            string Operator = "";

            string[,] list = new string[100,2];
            for (int i = start; i < stop; i++)
            {
                Operator = logicDt.Rows[i]["operator"].ObjToString();
                and_or = logicDt.Rows[i]["and_or"].ObjToString();
                if (String.IsNullOrWhiteSpace(and_or))
                    logicDt.Rows[i]["and_or"] = last;
                else
                    last = and_or;
                logicDt.Rows[i]["major"] = and_or;
                logicDt.Rows[i]["result"] = logicDt.Rows[i]["fail"].ObjToString();
            }

            int listCount = 0;
            for (int i = start; i < stop; i++)
            {
                Operator = logicDt.Rows[i]["operator"].ObjToString();
                if (String.IsNullOrWhiteSpace(Operator))
                    continue;
                status = logicDt.Rows[i]["fail"].ObjToString();
                if (String.IsNullOrWhiteSpace(status))
                    continue;
                and_or = logicDt.Rows[i]["and_or"].ObjToString();
                if (String.IsNullOrWhiteSpace(and_or))
                    and_or = "=";
                if (and_or == "AND")
                {
                    list[listCount, 0] = and_or;
                    list[listCount, 1] = status;
                    listCount++;
                }
                else
                {
                    if (listCount > 0)
                    {
                        if (list[listCount-1, 1] == "fail")
                            list[listCount-1, 1] = status;
                    }
                }
            }
            pass = true;
            for ( int i=0; i<listCount; i++)
            {
                status = list[i, 1];
                if ( status == "fail")
                {
                    pass = false;
                    break;
                }
            }
            return pass;
        }
        /****************************************************************************************/
        private bool CheckLogic ( string data, int index, string Operator, string what )
        {
            bool reject = false;
            if (String.IsNullOrWhiteSpace(data))
                return reject;
            if ( Operator == "<>")
            {
                if (data.Length < index)
                    return reject;
                string chr = data.Substring(index - 1, 1);
                if (chr == what)
                    return true;
            }
            else if ( Operator == "=")
            {
                if (String.IsNullOrWhiteSpace(what))
                    return false;
                if (data.Length < index)
                    return true;
                string chr = data.Substring(index - 1, 1);
                if (chr != what)
                    return true;
            }
            return reject;
        }
        /****************************************************************************************/
        private void GroupRow_Changed(object sender, EventArgs e)
        {
        }
        /****************************************************************************************/
        private void GroupFooter_Changed(object sender, EventArgs e)
        {
        }
        /****************************************************************************************/
        private int footerCount = 0;
        private int printLines = 0;
        private bool lastFooter = false;
        private void gridMain_BeforePrintRow(object sender, DevExpress.XtraGrid.Views.Printing.CancelPrintRowEventArgs e)
        {
            //printLines++;
            if ( e.RowHandle >= 0 )
            {
                int rowHandle = e.RowHandle;
                DataTable dt = (DataTable)dgv.DataSource;
                int row = gridMain.GetDataSourceRowIndex(rowHandle);
                string report = dt.Rows[row]["Report"].ObjToString();
                if (String.IsNullOrWhiteSpace(lastReport))
                    lastReport = report;
                if ( report != lastReport )
                {
//                    e.PS.InsertPageBreak(e.Y);

                    lastFooter = true;
                    lastReport = report;
                }
                footerCount = 5;
            }
//            if (!e.HasFooter)
//            {
//                lastFooter = false;
//                printLines++;
////                footerCount = 0;
//                return;
//            }
////            footerCount++;
        }
        /****************************************************************************************/
        private string lastReport = "";
        private string lastMajor = "";
        private void gridMain_AfterPrintRow(object sender, DevExpress.XtraGrid.Views.Printing.PrintRowEventArgs e)
        {
            if (!e.HasFooter)
                return;
            int row = e.RowHandle;
            if (row < 0)
                return;
            row = gridMain.GetDataSourceRowIndex(row);
            DataTable dt = (DataTable)dgv.DataSource;
            string finale = dt.Rows[row]["finale"].ObjToString();
            if (finale.Trim().ToUpper() == "YES")
            {
                e.PS.InsertPageBreak(e.Y);
            }
            if ( lastFooter )
            {
//                e.PS.InsertPageBreak(e.Y);
                lastFooter = false;
                return;
            }
        }
        /****************************************************************************************/
        private void showDetailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (G1.get_column_number(logicDt, "fail") < 0)
                logicDt.Columns.Add("fail");
            if (G1.get_column_number(logicDt, "major") < 0)
                logicDt.Columns.Add("major");

            DataRow dr = gridMain.GetFocusedDataRow();

            string payer = dr["payer"].ObjToString();
            string company = dr["companyCode"].ObjToString();
            string oldAgent = dr["oldAgentInfo"].ObjToString();
            string agent = dr["agentCode"].ObjToString();
            string uCode = dr["ucode"].ObjToString();
            string location = cmbReport.Text.Trim();
            string firstName = dr["firstName"].ObjToString();
            string lastName = dr["lastName"].ObjToString();
            string contract = dr["contractNumber"].ObjToString();

            if ( location.ToUpper() == "ALL")
            {
                MessageBox.Show("***ERROR*** Report Cannot be 'ALL'");
                return;
            }
            bool reject = ProcessLogic(location, company, oldAgent, agent, uCode);
            Logic logicForm = new Logic( contract, lastName, firstName, logicDt, location, company, oldAgent, agent, uCode);
            logicForm.Show();
        }
        /****************************************************************************************/
        private void gridMain_CustomDrawFooterCell(object sender, FooterCellCustomDrawEventArgs e)
        {

        }

        private void gridMain_CustomDrawBandHeader(object sender, DevExpress.XtraGrid.Views.BandedGrid.BandHeaderCustomDrawEventArgs e)
        {
            this.gridBand7.Caption = lastMajor;
        }

        private void gridMain_CustomDrawGroupPanel(object sender, DevExpress.XtraGrid.Views.Base.CustomDrawEventArgs e)
        {

        }
        /****************************************************************************************/
        private void compareResultsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            CompareResults compareForm = new CompareResults(dt);
            compareForm.Show();
        }
        /****************************************************************************************/
    }
}