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
using DevExpress.XtraGrid.Views.Base;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class InsuranceThirdPartyDumpReport : DevExpress.XtraEditors.XtraForm
    {
        private bool loading = true;
        private DataTable logicDt = null;
        private string workContract = "";
        /****************************************************************************************/
        public InsuranceThirdPartyDumpReport( string contract = "")
        {
            InitializeComponent();
            workContract = contract;
        }
        /****************************************************************************************/
        private void InsuranceThirdPartyDumpReport_Load(object sender, EventArgs e)
        {
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
            chkComboLocNames.Properties.DataSource = dx;
            chkComboLocNames.Text = "All";
            cmbReport.Hide();
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
        /*******************************************************************************************/
        private string getLocationNameQuery()
        {
            string procLoc = "";
            string[] locIDs = this.chkComboLocNames.EditValue.ToString().Split('|');
            for (int i = 0; i < locIDs.Length; i++)
            {
                if (!String.IsNullOrWhiteSpace(locIDs[i]))
                {
                    if (procLoc.Trim().Length > 0)
                        procLoc += ",";
                    string id = locIDs[i];
                    procLoc += "'" + id.Trim() + "'";
                }
            }
            return procLoc.Length > 0 ? " `report` IN (" + procLoc + ") " : "";
        }
        /****************************************************************************************/
        private void LoadData()
        {
            this.Cursor = Cursors.WaitCursor;

            //PullLogicFile();

            string report = cmbReport.Text;

            report = chkComboLocNames.Text;

            string cmd = "Select *, CONCAT(u.`lastName`, ', ', u.`firstName` ) payerName, CONCAT(j.`policyLastName`,', ', j.`policyFirstName`) policyName, ";
//            cmd += " j.`birthdate` AS policyBirthDate, ";
//            cmd += " j.`issueDate8` AS policyIssueDate8, ";
//            cmd += " j.`deceasedDate` AS policyDeceasedDate, ";
            cmd += " j.`lapsed` AS policyLapsed ";
            cmd += " FROM `policies` j ";
            cmd += " JOIN `icustomers` u ON j.`payer` = u.`payer` ";
            cmd += " JOIN `icontracts` x ON u.`contractNumber` = x.`contractNumber` ";
            if (!String.IsNullOrWhiteSpace(report))
            {
                if (report.ToUpper().Trim() == "ALL")
                    cmd += " WHERE `report` <> 'Not Third Party' ";
                else
                {
                    report = getLocationNameQuery();
                    cmd += " WHERE " + report;
                    //cmd += " WHERE `report` = '" + report + "' ";
                }
            }
            else
                cmd += " AND `report` <> 'Not Third Party' ";

            if ( !String.IsNullOrWhiteSpace ( workContract ))
                cmd += " AND u.`contractNumber` = '" + workContract + "' ";

            if ( chkPayerOnly.Checked )
                cmd += "GROUP BY j.`payer` ";
            else
                cmd += " ORDER BY `report`, j.`payer`, `policyNumber` ";
            cmd += ";";

            DataTable dt = G1.get_db_data(cmd);

            if ( chkPayerOnly.Checked )
            {
                DataView tempview = dt.DefaultView;
                tempview.Sort = "payer asc";
                dt = tempview.ToTable();
            }

            int lastRow = dt.Rows.Count;

            barImport.Minimum = 0;
            barImport.Maximum = lastRow;
            this.labelMaximum.Text = "0 of " + lastRow.ToString();
            this.labelMaximum.Show();
            this.labelMaximum.Refresh();


            dt.Columns.Add("num");
            dt.Columns.Add("customer");
            dt.Columns.Add("majorBreak");
            dt.Columns.Add("reject");
            dt.Columns.Add("finale");


            string contractNumber = "";
            string groupNumber = "";
            int idx = 0;
            string name = "";
            string oldReport = "";
            string payerName = "";
            string payer = "";
            double expected = 0D;
            DateTime paymentDate = DateTime.Now;
            if (chkHonorAsOfDate.Checked)
                paymentDate = this.dateTimePicker1.Value;
            DataTable dx = null;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                Application.DoEvents();

                this.labelMaximum.Text = (i + 1).ToString() + " of " + lastRow.ToString();
                this.labelMaximum.Refresh();
                barImport.Value = i + 1;

                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
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
                name = report + ": " + payerName + " (" + payer + ")";
                dt.Rows[i]["majorBreak"] = name;

                if (chkPayerOnly.Checked)
                {

                    expected = Policies.CalcMonthlyPremium(payer, paymentDate);
                    dt.Rows[i]["premium"] = expected;
                }

                if (chkHonorAsOfDate.Checked)
                {
                    string searchDate = this.dateTimePicker1.Value.ToString("yyyy-MM-dd");
                    cmd = "Select * from `icustomers` where `payer` = '" + payer + "';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                    {
                        string list = "";
                        for (int j = 0; j < dx.Rows.Count; j++)
                        {
                            string contract = dx.Rows[j]["contractNumber"].ObjToString();
                            list += "'" + contract + "',";
                        }
                        list = list.TrimEnd(',');
                        list = "(" + list + ")";
                        cmd = "Select * from `ipayments` where `contractNumber` IN " + list + " AND `payDate8` > '" + searchDate + "' order by `payDate8` ASC LIMIT 2;";
                        dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count > 0)
                        {
                            dt.Rows[i]["dueDate8"] = dx.Rows[0]["dueDate8"];
                        }
                    }
                }
            }

            this.labelMaximum.Text = lastRow.ToString() + " of " + lastRow.ToString();
            this.labelMaximum.Refresh();
            barImport.Value = lastRow;


            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void chkCompany_CheckedChanged(object sender, EventArgs e)
        {
            gridMain.Columns["majorBreak"].GroupIndex = -1;
            gridMain.Columns["payerName"].GroupIndex = -1;
            gridMain.Columns["payer"].GroupIndex = -1;
            gridMain.Columns["groupNumber"].GroupIndex = -1;
            //gridMain.Columns["customer"].GroupIndex = -1;
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
            if (!chkIncludeHeader.Checked)
                return;
            Printer.setupPrinterQuads(e, 2, 3);
            Font font = new Font("Ariel", 16);
            Printer.DrawQuad(1, 1, Printer.xQuads, 2, "South Mississippi Funeral Services, LLC", Color.Black, BorderSide.Top, font, HorizontalAlignment.Center);

            Printer.SetQuadSize(12, 12);

            font = new Font("Ariel", 8);
            Printer.DrawGridDate(2, 3, 2, 3, Color.Black, BorderSide.None, font);
            Printer.DrawGridPage(11, 3, 2, 3, Color.Black, BorderSide.None, font);

            Printer.DrawQuad(1, 9, 2, 3, "User : " + LoginForm.username, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);

            font = new Font("Ariel", 10, FontStyle.Bold);

            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
        }
        /****************************************************************************************/
        private void cmbReport_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
        }
        ///****************************************************************************************/
        //private void ProcessLogicData ( DataTable dt, string location = "")
        //{
        //    if ( String.IsNullOrWhiteSpace ( location ))
        //        location = cmbReport.Text;
        //    if (string.IsNullOrWhiteSpace(location))
        //    {
        //        MessageBox.Show("***ERROR*** You must select a location!");
        //        return;
        //    }

        //    if (G1.get_column_number(logicDt, "fail") < 0)
        //        logicDt.Columns.Add("fail");
        //    if (G1.get_column_number(logicDt, "major") < 0)
        //        logicDt.Columns.Add("major");
        //    for (int i = 0; i < dt.Rows.Count; i++)
        //        dt.Rows[i]["reject"] = "";
        //    string payer = "";
        //    string company = "";
        //    string oldAgent = "";
        //    string agent = "";
        //    string uCode = "";
        //    bool reject = false;
        //    for ( int i=0; i<dt.Rows.Count; i++)
        //    {
        //        try
        //        {
        //            agent = dt.Rows[i]["agentCode"].ObjToString();
        //            payer = dt.Rows[i]["payer"].ObjToString();
        //            if ( payer == "BB-1061")
        //            {

        //            }
        //            company = dt.Rows[i]["companyCode"].ObjToString();
        //            uCode = dt.Rows[i]["ucode"].ObjToString();
        //            oldAgent = dt.Rows[i]["oldAgentInfo"].ObjToString();
        //            reject = ProcessLogic(location, company, oldAgent, agent, uCode);
        //            if (reject)
        //                dt.Rows[i]["reject"] = "Y";
        //            else
        //                dt.Rows[i]["reject"] = "";
        //        }
        //        catch ( Exception ex)
        //        {

        //        }
        //    }
        //    //string status = "";
        //    //for ( int i=dt.Rows.Count-1; i>=0; i--)
        //    //{
        //    //    status = dt.Rows[i]["reject"].ObjToString();
        //    //    if (status == "Y")
        //    //        dt.Rows.RemoveAt(i);
        //    //}
        //}
        ///****************************************************************************************/
        //private void ClearAllFail ()
        //{
        //    if (G1.get_column_number(logicDt, "fail") < 0)
        //        logicDt.Columns.Add("fail");
        //    for (int i = 0; i < logicDt.Rows.Count; i++)
        //    {
        //        logicDt.Rows[i]["fail"] = "";
        //        logicDt.Rows[i]["company"] = "";
        //        logicDt.Rows[i]["oldAgent"] = "";
        //        logicDt.Rows[i]["agent"] = "";
        //        logicDt.Rows[i]["ucode"] = "";
        //    }
        //}
        ///****************************************************************************************/
        //private void GetLogic ( DataTable dt, int i, ref string location, ref string and_or, ref string Operator, ref string C1, ref string C2, ref string C3, ref string C4, ref string O1, ref string O2, ref string O3, ref string A1, ref string A2, ref string A3, ref string U1)
        //{
        //    location = dt.Rows[i]["location"].ObjToString();
        //    and_or = dt.Rows[i]["and_or"].ObjToString();
        //    Operator = dt.Rows[i]["operator"].ObjToString();
        //    C1 = dt.Rows[i]["C1"].ObjToString();
        //    C2 = dt.Rows[i]["C2"].ObjToString();
        //    C3 = dt.Rows[i]["C3"].ObjToString();
        //    C4 = dt.Rows[i]["C4"].ObjToString();
        //    O1 = dt.Rows[i]["O1"].ObjToString();
        //    O2 = dt.Rows[i]["O2"].ObjToString();
        //    O3 = dt.Rows[i]["O3"].ObjToString();
        //    A1 = dt.Rows[i]["A1"].ObjToString();
        //    A2 = dt.Rows[i]["A2"].ObjToString();
        //    A3 = dt.Rows[i]["A3"].ObjToString();
        //    U1 = dt.Rows[i]["U1"].ObjToString();
        //}
        ///****************************************************************************************/
        //private bool ProcessLogic ( string location, string company, string oldAgent, string agent, string ucode)
        //{
        //    bool reject = false;

        //    string loc = "";
        //    string and_or = "";
        //    string Operator = "";
        //    string C1 = "";
        //    string C2 = "";
        //    string C3 = "";
        //    string C4 = "";
        //    string O1 = "";
        //    string O2 = "";
        //    string O3 = "";
        //    string A1 = "";
        //    string A2 = "";
        //    string A3 = "";
        //    string U1 = "";

        //    string saveLoc = "";

        //    bool R_C1 = false;
        //    bool R_C2 = false;
        //    bool R_C3 = false;
        //    bool R_C4 = false;
        //    bool R_O1 = false;
        //    bool R_O2 = false;
        //    bool R_O3 = false;
        //    bool R_A1 = false;
        //    bool R_A2 = false;
        //    bool R_A3 = false;
        //    bool R_U1 = false;
        //    ClearAllFail();

        //    int firstIndex = 0;
        //    bool[] passFail = new bool[100];
        //    int passFailCount = 0;
        //    bool open = false;

        //    int i = 0;

        //    for (i = 0; i < logicDt.Rows.Count; i++)
        //    {
        //        try
        //        {
        //            R_C1 = false;
        //            R_C2 = false;
        //            R_C3 = false;
        //            R_C4 = false;
        //            R_O1 = false;
        //            R_O2 = false;
        //            R_O3 = false;
        //            R_A1 = false;
        //            R_A2 = false;
        //            R_A3 = false;
        //            R_U1 = false;

        //            logicDt.Rows[i]["company"] = company;
        //            logicDt.Rows[i]["oldAgent"] = oldAgent;
        //            logicDt.Rows[i]["agent"] = agent;
        //            logicDt.Rows[i]["ucode"] = ucode;

        //            GetLogic(logicDt, i, ref loc, ref and_or, ref Operator, ref C1, ref C2, ref C3, ref C4, ref O1, ref O2, ref O3, ref A1, ref A2, ref A3, ref U1);
        //            if (String.IsNullOrWhiteSpace(saveLoc) && !String.IsNullOrWhiteSpace(loc))
        //                saveLoc = loc;
        //            if (!String.IsNullOrWhiteSpace(saveLoc) && String.IsNullOrWhiteSpace(loc))
        //                loc = saveLoc;
        //            if (loc == "All Third Party")
        //                continue;
        //            if (loc != "All Third Party")
        //            {
        //                if (loc != location)
        //                {
        //                    if ((firstIndex + 1) < i)
        //                    {
        //                        passFail[passFailCount] = CheckPassFail(firstIndex, i);
        //                        passFailCount++;
        //                        open = false;
        //                        //company = "XX";
        //                        //agent = "X";
        //                    }
        //                    saveLoc = loc;
        //                    firstIndex = i;
        //                    continue;
        //                }
        //                saveLoc = loc;
        //            }
        //            if (String.IsNullOrWhiteSpace(loc))
        //                break;
        //            open = true;
        //            R_C1 = CheckLogic(company, 1, Operator, C1);
        //            R_C2 = CheckLogic(company, 2, Operator, C2);
        //            R_C3 = CheckLogic(company, 3, Operator, C3);
        //            R_C4 = CheckLogic(company, 4, Operator, C4);

        //            R_O1 = CheckLogic(oldAgent, 1, Operator, O1);
        //            R_O2 = CheckLogic(oldAgent, 2, Operator, O2);
        //            R_O3 = CheckLogic(oldAgent, 3, Operator, O3);

        //            R_A1 = CheckLogic(agent, 1, Operator, A1);
        //            R_A2 = CheckLogic(agent, 2, Operator, A2);
        //            R_A3 = CheckLogic(agent, 3, Operator, A3);

        //            R_U1 = CheckLogic(ucode, 1, Operator, U1);

        //            if (R_C1 || R_C2 || R_C3 || R_C3 || R_O1 || R_O2 || R_O3 || R_A1 || R_A2 || R_A3 || R_U1)
        //                logicDt.Rows[i]["fail"] = "fail";
        //            else
        //                logicDt.Rows[i]["fail"] = "accept";
        //        }
        //        catch (Exception ex)
        //        {

        //        }
        //    }
        //    if (open)
        //    {
        //        passFail[passFailCount] = CheckPassFail(firstIndex, logicDt.Rows.Count);
        //        passFailCount++;
        //    }
        //    bool secondMethod = false;
        //    if (chkSecondMethod.Checked)
        //        secondMethod = true;
        //    if (!secondMethod)
        //    {
        //        reject = false;
        //        for (i = 0; i < passFailCount; i++)
        //        {
        //            if (!passFail[i])
        //            {
        //                reject = true;
        //                break;
        //            }
        //        }
        //    }
        //    else
        //    {
        //        reject = true;
        //        for (i = 0; i < passFailCount; i++)
        //        {
        //            if (passFail[i])
        //            {
        //                reject = false;
        //                break;
        //            }
        //        }
        //    }
        //    return reject;
        //}
        ///****************************************************************************************/
        //private bool CheckPassFail ( int start, int stop )
        //{
        //    bool pass = true;
        //    string status = "";
        //    string and_or = "";
        //    string last = "AND";
        //    string save = "";
        //    string Operator = "";

        //    string[,] list = new string[100,2];
        //    for (int i = start; i < stop; i++)
        //    {
        //        Operator = logicDt.Rows[i]["operator"].ObjToString();
        //        and_or = logicDt.Rows[i]["and_or"].ObjToString();
        //        if (String.IsNullOrWhiteSpace(and_or))
        //            logicDt.Rows[i]["and_or"] = last;
        //        else
        //            last = and_or;
        //        logicDt.Rows[i]["major"] = and_or;
        //        logicDt.Rows[i]["result"] = logicDt.Rows[i]["fail"].ObjToString();
        //    }

        //    int listCount = 0;
        //    for (int i = start; i < stop; i++)
        //    {
        //        Operator = logicDt.Rows[i]["operator"].ObjToString();
        //        if (String.IsNullOrWhiteSpace(Operator))
        //            continue;
        //        status = logicDt.Rows[i]["fail"].ObjToString();
        //        if (String.IsNullOrWhiteSpace(status))
        //            continue;
        //        and_or = logicDt.Rows[i]["and_or"].ObjToString();
        //        if (String.IsNullOrWhiteSpace(and_or))
        //            and_or = "=";
        //        if (and_or == "AND")
        //        {
        //            list[listCount, 0] = and_or;
        //            list[listCount, 1] = status;
        //            listCount++;
        //        }
        //        else
        //        {
        //            if (listCount > 0)
        //            {
        //                if (list[listCount-1, 1] == "fail")
        //                    list[listCount-1, 1] = status;
        //            }
        //        }
        //    }
        //    pass = true;
        //    for ( int i=0; i<listCount; i++)
        //    {
        //        status = list[i, 1];
        //        if ( status == "fail")
        //        {
        //            pass = false;
        //            break;
        //        }
        //    }
        //    return pass;
        //}
        ///****************************************************************************************/
        //private bool CheckLogic ( string data, int index, string Operator, string what )
        //{
        //    bool reject = false;
        //    if (String.IsNullOrWhiteSpace(data))
        //        return reject;
        //    if ( Operator == "<>")
        //    {
        //        if (data.Length < index)
        //            return reject;
        //        string chr = data.Substring(index - 1, 1);
        //        if (chr == what)
        //            return true;
        //    }
        //    else if ( Operator == "=")
        //    {
        //        if (String.IsNullOrWhiteSpace(what))
        //            return false;
        //        if (data.Length < index)
        //            return true;
        //        string chr = data.Substring(index - 1, 1);
        //        if (chr != what)
        //            return true;
        //    }
        //    return reject;
        //}
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
        private void gridMain_CustomDrawBandHeader(object sender, DevExpress.XtraGrid.Views.BandedGrid.BandHeaderCustomDrawEventArgs e)
        {
            this.gridBand7.Caption = lastMajor;
        }
        /****************************************************************************************/
        private void gridMain_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            ColumnView view = sender as ColumnView;
            if (e.Column.FieldName.ToUpper().IndexOf("DATE") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                if (e.DisplayText.IndexOf("0000") >= 0 || e.DisplayText.IndexOf("0001") >= 0)
                    e.DisplayText = "";
                else
                {
                    DateTime date = e.DisplayText.ObjToString().ObjToDateTime();
                    if (date.Year > 100)
                    {
                        if (date.Year < 1932)
                            e.DisplayText = "Y";
                        else
                            e.DisplayText = date.ToString("MM/dd/yyyy");
                    }
                    else
                        e.DisplayText = "";
                }
            }
            else if (e.Column.FieldName.ToUpper().IndexOf("SSN") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                string ssn = e.DisplayText;
                if (ssn.Trim().Length >= 9)
                    e.DisplayText = "XXX-XX-" + ssn.Substring(5, 4);
                else
                    e.DisplayText = "";
            }
            else if (e.Column.FieldName.ToUpper().IndexOf("ZIP1") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                if (e.DisplayText == "0")
                    e.DisplayText = "";
            }
        }
        /****************************************************************************************/
    }
}