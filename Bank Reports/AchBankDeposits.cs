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
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraPrinting;
using DevExpress.Utils;
using MyXtraGrid;
using DevExpress.XtraCharts;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraEditors.ViewInfo;
using DevExpress.Utils.Drawing;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class AchBankDeposits : DevExpress.XtraEditors.XtraForm
    {
        private DataTable groupContracts = null;
        private DataTable agentsDt = null;
        private bool runAgents = false;
        private DataTable paymentDetail = null;
        private string bankDetails = "";
        private double beginningBalance = 0D;
        private string workReport = "";
        private string majorBankAccount = "";
        private bool foundLocalPreference = false;
        private bool loading = true;
        /****************************************************************************************/
        public AchBankDeposits( string report )
        {
            InitializeComponent();
            SetupTotalsSummary();
            workReport = report;

            btnDraftReport.Hide();
            btnDraftReport.Refresh();

            btnEditCombos.Hide();
            btnEditCombos.Refresh();

            chkUseCombos.Hide();
            chkUseCombos.Refresh();

            this.Text = "ACH Deposit Report ( " + workReport + " )";

            if (workReport == "Funeral Detail Report")
                SetDetailDeposits();
            else if (workReport == "ACH Detail Report")
                SetMainDeposits();
            else if (workReport == "Cover Report")
            {
                btnDraftReport.Text = "Run Cover Report";
                SetMainDeposits();
            }
        }
        /****************************************************************************************/
        private void SetupTotalsSummary()
        {
            AddSummaryColumn("DDA", null);
            AddSummaryColumn("TDA", null);
            AddSummaryColumn("IDA", null);
            AddSummaryColumn("NDA", null);
            AddSummaryColumn("misc", null);
            AddSummaryColumn("returns", null);
            AddSummaryColumn("transfers", null);
            gridMain.Columns["balance"].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Custom;
        }
        /****************************************************************************************/
        private void AddSummaryColumn(string columnName)
        {
            gridMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            //gridMain.Columns[columnName].SummaryItem.DisplayFormat = "{0:0,0.00}";
            gridMain.Columns[columnName].SummaryItem.DisplayFormat = "{0:N2}";
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
        private void SetMainDeposits()
        {
            int i = 1;
            G1.SetColumnPosition(gridMain, "num", i++);
            G1.SetColumnPosition(gridMain, "date", i++);
            G1.SetColumnPosition(gridMain, "dow", i++);
            //G1.SetColumnPosition(gridMain, "depositNumber", i++);
            //G1.SetColumnPosition(gridMain, "serviceId", i++);
            G1.SetColumnPosition(gridMain, "DDA", i++);
            G1.SetColumnPosition(gridMain, "TDA", i++);
            G1.SetColumnPosition(gridMain, "IDA", i++);
            G1.SetColumnPosition(gridMain, "NDA", i++);
            G1.SetColumnPosition(gridMain, "dailyTotals", i++);
            G1.SetColumnPosition(gridMain, "misc", i++);
            G1.SetColumnPosition(gridMain, "returns", i++);
            G1.SetColumnPosition(gridMain, "transfers", i++);
            if ( workReport == "Cover Report")
                G1.SetColumnPosition(gridMain, "balance", i++);
            G1.SetColumnPosition(gridMain, "comment", i++);

            gridMain.Columns["balance"].Visible = false;
            if (workReport == "Cover Report")
                gridMain.Columns["balance"].Visible = true;
            gridMain.Columns["depositNumber"].Visible = false;
            gridMain.Columns["serviceId"].Visible = false;
            gridMain.Columns["trusts"].Visible = false;
            gridMain.Columns["insurance"].Visible = false;

            //gridMain.Columns["transfers"].Visible = false;
            gridMain.Columns["NDA"].Visible = false;
            if ( workReport.ToUpper() == "COVER REPORT")
                gridMain.Columns["NDA"].Visible = true;
        }
        /****************************************************************************************/
        private void SetDetailDeposits()
        {
            int i = 1;
            G1.SetColumnPosition(gridMain, "num", i++);
            G1.SetColumnPosition(gridMain, "date", i++);
            G1.SetColumnPosition(gridMain, "dow", i++);
            G1.SetColumnPosition(gridMain, "depositNumber", i++);
            G1.SetColumnPosition(gridMain, "serviceId", i++);
            G1.SetColumnPosition(gridMain, "trusts", i++);
            G1.SetColumnPosition(gridMain, "insurance", i++);
            G1.SetColumnPosition(gridMain, "NDA", i++);
            G1.SetColumnPosition(gridMain, "misc", i++);
            G1.SetColumnPosition(gridMain, "returns", i++);
            G1.SetColumnPosition(gridMain, "transfers", i++);
            G1.SetColumnPosition(gridMain, "dailyTotals", i++);
            G1.SetColumnPosition(gridMain, "comment", i++);

            gridMain.Columns["balance"].Visible = false;
            gridMain.Columns["DDA"].Visible = false;
            gridMain.Columns["TDA"].Visible = false;
            gridMain.Columns["IDA"].Visible = false;

            gridMain.Columns["depositNumber"].OptionsColumn.AllowEdit = true;
            gridMain.Columns["serviceId"].OptionsColumn.AllowEdit = true;
            gridMain.Columns["trusts"].OptionsColumn.AllowEdit = true;
            gridMain.Columns["insurance"].OptionsColumn.AllowEdit = true;
            gridMain.Columns["NDA"].OptionsColumn.AllowEdit = true;
            gridMain.Columns["misc"].OptionsColumn.AllowEdit = true;
            gridMain.Columns["returns"].OptionsColumn.AllowEdit = true;
            gridMain.Columns["transfers"].OptionsColumn.AllowEdit = true;

            AddSummaryColumn("misc", null);
            AddSummaryColumn("returns", null);
            AddSummaryColumn("transfers", null);

            gridMain.OptionsView.ShowBands = false;
            gridMain.OptionsPrint.PrintBandHeader = false;
        }
        /****************************************************************************************/
        private void AchBankDeposits_Load(object sender, EventArgs e)
        {
            PleaseWait pleaseForm = null;
            pleaseForm = new PleaseWait("Please Wait!\nLoading Information");
            pleaseForm.Show();
            pleaseForm.Refresh();

            if (workReport == "Cover Report")
            {
                pictureAdd.Hide();
                pictureDelete.Hide();
            }    

            btnSave.Hide();
            txtBeginningBalance.Hide();
            label1.Hide();

            string saveName = "ACH Bank Deposits " + workReport + " Primary";
            string skinName = "";

            foundLocalPreference = G1.RestoreGridLayout(this, this.dgv, gridMain, LoginForm.username, saveName, ref skinName);
            if (!String.IsNullOrWhiteSpace(skinName))
            {
                if (skinName != "DevExpress Style")
                    skinForm_SkinSelected("Skin : " + skinName);
            }

            loadGroupCombo(cmbSelectColumns, "ACH Bank Deposits " + workReport, "Primary");



            DateTime now = DateTime.Now;
            now = now.AddMonths(-1);
            now = new DateTime(now.Year, now.Month, 1);

            int days = DateTime.DaysInMonth(now.Year, now.Month);
            DateTime stop = new DateTime(now.Year, now.Month, days);
            this.dateTimePicker1.Value = stop;

            this.Cursor = Cursors.WaitCursor;

            if (workReport == "Funeral Detail Report")
            {
                txtBeginningBalance.Hide();
                label1.Hide();
                loadLocatons();
            }
            else
            {
                chkComboLocation.Hide();
                label2.Hide();
                //btnGetDeposits_Click(null, null);
            }

            ScaleCells();

            this.Cursor = Cursors.Default;

            loading = false;

            pleaseForm.FireEvent1();
            pleaseForm.Dispose();
            pleaseForm = null;
        }
        /***********************************************************************************************/
        private void loadLocatons()
        {
            string cmd = "Select * from `funeralReports` ORDER by `order`;";
            DataTable locDt = G1.get_db_data(cmd);

            DataTable newLocDt = locDt.Clone();

            for (int i = 0; i < locDt.Rows.Count; i++)
            {
                G1.copy_dt_row(locDt, i, newLocDt, newLocDt.Rows.Count);
            }
            chkComboLocation.Properties.DataSource = newLocDt;
        }
        /***********************************************************************************************/
        private void loadLocatonsx()
        {
            string cmd = "Select * from `bank_accounts` order by `record`;";
            DataTable locDt = G1.get_db_data(cmd);

            DataTable newLocDt = locDt.Clone();

            string account_title = "";

            for (int i = 0; i < locDt.Rows.Count; i++)
            {
                if (locDt.Rows[i]["show_dropdown"].ObjToString() == "1")
                    G1.copy_dt_row(locDt, i, newLocDt, newLocDt.Rows.Count);
                else
                {
                    account_title = locDt.Rows[i]["account_title"].ObjToString();
                    if ( account_title == "Cash - Trustmark Fisher Insurance")
                        G1.copy_dt_row(locDt, i, newLocDt, newLocDt.Rows.Count);
                }
            }
            chkComboLocation.Properties.DataSource = newLocDt;

            //locations = locations.TrimEnd('|');
            //chkComboLocation.EditValue = locations;
            //chkComboLocation.Text = locations;
        }
        /****************************************************************************************/
        private void AddToLocationCombo(DataTable locationDt, string text)
        {
            DataRow ddrx = locationDt.NewRow();
            ddrx["options"] = text;
            locationDt.Rows.Add(ddrx);
        }
        /****************************************************************************************/
        private void checkedComboBoxEdit1_Properties_EditValueChanged(object sender, EventArgs e)
        {
        }
        /****************************************************************************************/
        private void CleanupScreen()
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt != null)
            {
                dt.Rows.Clear();
                dgv.DataSource = dt;
            }
            btnSave.Hide();
            btnSave.Refresh();

            btnDraftReport.Hide();
            btnDraftReport.Refresh();
            btnEditCombos.Hide();
            btnEditCombos.Refresh();
            chkUseCombos.Hide();
            chkUseCombos.Refresh();

        }
        /****************************************************************************************/
        private void btnRight_Click(object sender, EventArgs e)
        {
            if (!CheckForSave())
                return;
            this.Cursor = Cursors.WaitCursor;
            DateTime now = this.dateTimePicker1.Value;
            now = now.AddMonths(1);
            now = new DateTime(now.Year, now.Month, 1);
            //this.dateTimePicker1.Value = now;
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            this.dateTimePicker1.Value = new DateTime(now.Year, now.Month, days);

            DateTime startDate = now;
            DateTime stopDate = this.dateTimePicker1.Value;

            CleanupScreen();

            //btnGetDeposits_Click(null, null);

            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void btnLeft_Click(object sender, EventArgs e)
        {
            if (!CheckForSave())
                return;
            this.Cursor = Cursors.WaitCursor;
            DateTime now = this.dateTimePicker1.Value;
            now = now.AddMonths(-1);
            now = new DateTime(now.Year, now.Month, 1);
            //this.dateTimePicker1.Value = now;
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            this.dateTimePicker1.Value = new DateTime(now.Year, now.Month, days);

            DateTime startDate = now;
            DateTime stopDate = this.dateTimePicker1.Value;

            CleanupScreen();

            //btnGetDeposits_Click(null, null);

            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private DataTable LoadMain(DateTime startDate, DateTime stopDate)
        {
            DateTime start = startDate;
            string date1 = G1.DateTimeToSQLDateTime(start);
            DateTime stop = stopDate;
            string date2 = G1.DateTimeToSQLDateTime(stop);
            DateTime aDate = DateTime.Now;

            DataTable dx = new DataTable();
            dx.Columns.Add("num");
            dx.Columns.Add("record");
            dx.Columns.Add("date");
            dx.Columns.Add("dow");
            dx.Columns.Add("adate");
            dx.Columns.Add("DDA", Type.GetType("System.Double"));
            dx.Columns.Add("TDA", Type.GetType("System.Double"));
            dx.Columns.Add("IDA", Type.GetType("System.Double"));
            dx.Columns.Add("NDA", Type.GetType("System.Double"));
            dx.Columns.Add("misc", Type.GetType("System.Double"));
            dx.Columns.Add("returns", Type.GetType("System.Double"));
            dx.Columns.Add("transfers", Type.GetType("System.Double"));
            dx.Columns.Add("dailyTotals", Type.GetType("System.Double"));
            dx.Columns.Add("balance", Type.GetType("System.Double"));
            dx.Columns.Add("comment");
            dx.Columns.Add("bank_account");
            dx.Columns.Add("manual");
            dx.Columns.Add("depositNumber");
            dx.Columns.Add("serviceId");
            dx.Columns.Add("location");

            TimeSpan ts = stop - start;

            for (int i = 0; i <= ts.Days; i++)
            {
                if (start.AddDays(i) > stop)
                    break;
                DataRow dR = dx.NewRow();
                aDate = start.AddDays(i);
                dR["date"] = aDate.ToString("MM/dd/yyyy");
                dR["adate"] = aDate.ToString("yyyyMMdd");
                dR["dow"] = G1.DayOfWeekText(aDate);
                dx.Rows.Add(dR);
            }

            return dx;
        }
        /****************************************************************************************/
        private DataTable LoadData( DateTime startDate, DateTime stopDate, string bankDetails, string specialSearch = "" )
        {
            DateTime start = startDate;
            string date1 = G1.DateTimeToSQLDateTime(start);
            DateTime stop = stopDate;
            string date2 = G1.DateTimeToSQLDateTime(stop);
            DateTime aDate = DateTime.Now;

            string cmd = "Select * from `lockboxdeposits` where `location` = 'XYZZYZZZ';";
            DataTable dt = G1.get_db_data (cmd);

            cmd = "Select * from `lockboxdeposits` where `date` >= '" + date1 + "' and `date` <= '" + date2 + "' AND `bank_account` = '" + bankDetails + "' ";
            if (!String.IsNullOrWhiteSpace(specialSearch))
            {
                if (specialSearch != "ALL")
                {
                    string[] Lines = specialSearch.Split(',');
                    if (Lines.Length > 0)
                    {
                        cmd += " AND `location` IN (";
                        for (int i = 0; i < Lines.Length; i++)
                            cmd += "'" + Lines[i].Trim() + "',";
                        cmd = cmd.TrimEnd(',');
                        cmd += ") ";
                    }
                }
            }
            else
                cmd += " AND `location` = '' ";
            cmd += " ORDER BY `date` asc;";

            dt = G1.get_db_data(cmd);
            if (G1.get_column_number(dt, "num") < 0)
                dt.Columns.Add("num");
            if (G1.get_column_number(dt, "dow") < 0)
                dt.Columns.Add("dow");
            if ( G1.get_column_number ( dt, "dailyTotals") < 0 )
                dt.Columns.Add("dailyTotals", Type.GetType("System.Double"));
            if (dt.Rows.Count <= 0)
                return dt;
            return dt;
        }
        /****************************************************************************************/
        private void LoadUpGroupRows(DataTable dt)
        {
            string location = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                location = dt.Rows[i]["Location Name"].ObjToString();
                DataRow[] dRows = groupContracts.Select("loc='" + location.Trim() + "'");
                if (dRows.Length > 0)
                    dt.Rows[i]["Location Name"] = location + " [ Contracts: " + dRows[0]["contracts"].ObjToString() + " ]";
            }
        }
        /****************************************************************************************/
        private void gridMain_CustomDrawGroupRow(object sender, DevExpress.XtraGrid.Views.Base.RowObjectCustomDrawEventArgs e)
        {
            //GridGroupRowInfo info = e.Info as GridGroupRowInfo;
            //string location = info.GroupText;
            //int idx = location.LastIndexOf(']');
            //if (idx > 0)
            //{
            //    location = location.Substring(idx+1);
            //    DataRow[] dRows = groupContracts.Select("loc='" + location.Trim() + "'");
            //    if (dRows.Length > 0)
            //        info.GroupText += " " + dRows[0]["contracts"].ObjToString();
            //}
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

            printableComponentLink1.EnablePageDialog = true;

            printableComponentLink1.CreateDetailHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateDetailHeaderArea);
            printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);
            printableComponentLink1.BeforeCreateAreas += new System.EventHandler(this.printableComponentLink1_BeforeCreateAreas);
            printableComponentLink1.AfterCreateAreas += new System.EventHandler(this.printableComponentLink1_AfterCreateAreas);
            printableComponentLink1.Landscape = false;

            Printer.setupPrinterMargins(10, 5, 80, 50);

            pageMarginLeft = Printer.pageMarginLeft;
            pageMarginRight = Printer.pageMarginRight;
            pageMarginTop = Printer.pageMarginTop;
            pageMarginBottom = Printer.pageMarginBottom;

            printableComponentLink1.Margins.Left = pageMarginLeft;
            printableComponentLink1.Margins.Right = pageMarginRight;
            printableComponentLink1.Margins.Top = pageMarginTop;
            printableComponentLink1.Margins.Bottom = pageMarginBottom;

            gridMain.Columns["dow"].Visible = false;
            gridMain.Columns["num"].Visible = false;
            int dateWidth = gridMain.Columns["date"].Width;
            gridMain.Columns["date"].Width = 80;

            printingSystem1.Document.AutoFitToPagesWidth = 1;

            printableComponentLink1.CreateDocument();
            printableComponentLink1.ShowPreview();

            gridMain.Columns["dow"].Visible = true;
            gridMain.Columns["num"].Visible = true;
            gridMain.Columns["date"].Width = dateWidth;
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
            printableComponentLink1.Landscape = false;

            Printer.setupPrinterMargins(10, 5, 80, 50);

            pageMarginLeft = Printer.pageMarginLeft;
            pageMarginRight = Printer.pageMarginRight;
            pageMarginTop = Printer.pageMarginTop;
            pageMarginBottom = Printer.pageMarginBottom;

            printableComponentLink1.Margins.Left = pageMarginLeft;
            printableComponentLink1.Margins.Right = pageMarginRight;
            printableComponentLink1.Margins.Top = pageMarginTop;
            printableComponentLink1.Margins.Bottom = pageMarginBottom;

            gridMain.Columns["dow"].Visible = false;
            gridMain.Columns["num"].Visible = false;
            int dateWidth = gridMain.Columns["date"].Width;
            gridMain.Columns["date"].Width = 80;

            printingSystem1.Document.AutoFitToPagesWidth = 1;

            printableComponentLink1.CreateDocument();
            printableComponentLink1.PrintDlg();

            gridMain.Columns["dow"].Visible = true;
            gridMain.Columns["num"].Visible = true;
            gridMain.Columns["date"].Width = dateWidth;
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

//            Printer.DrawQuad(1, 9, 2, 3, "User : " + LoginForm.username, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);

            font = new Font("Ariel", 10, FontStyle.Regular);
            string title = this.Text;
            //if (workReport == "ACH Detail Report")
            //    title = "The First Drafts";
            //else
            //    title = "The First (All)";
            Printer.DrawQuad(6, 7, 4, 3, title, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            Printer.DrawQuad(1, 9, 2, 3, "User : " + LoginForm.username, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);


            DateTime date = this.dateTimePicker1.Value;
            string workDate = date.Month.ToString("D2") + "/" + date.Year.ToString("D4");
            Printer.SetQuadSize(24, 12);
            font = new Font("Ariel", 9, FontStyle.Regular);
            Printer.DrawQuad(20, 8, 5, 4, "Month Closing - " + workDate, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            //Printer.DrawQuad(16, 8, 3, 4, lblPayment.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(19, 8, 3, 4, lblTrust85.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(22, 8, 3, 4, lblTrust100.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /****************************************************************************************/
        private int footerCount = 0;
        private void gridMain_BeforePrintRow(object sender, DevExpress.XtraGrid.Views.Printing.CancelPrintRowEventArgs e)
        {
            if (workReport != "Funeral Detail Report")
                return;
            if (e.HasFooter)
            {
                footerCount++;
            }
        }
        /****************************************************************************************/
        private void gridMain_AfterPrintRow(object sender, DevExpress.XtraGrid.Views.Printing.PrintRowEventArgs e)
        {
            if (workReport != "Funeral Detail Report")
                return;
            if (e.HasFooter)
            {
                footerCount++;
                if (footerCount >= 1)
                {
                    footerCount = 0;
                    e.PS.InsertPageBreak(e.Y);
                }
            }
        }
        /****************************************************************************************/
        private void gridMain_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            if (runAgents)
            {
                int row = e.ListSourceRow;
                if (row >= 0)
                {
                    //if (gridMain.IsDataRow(row))
                    //{
                    //    e.Visible = false;
                    //    e.Handled = true;
                    //    return;
                    //}
                }
            }
        }
        /****************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        { // Spy Glass
            G1.SpyGlass(gridMain);
            //if (this.gridMain.OptionsFind.AlwaysVisible == true)
            //    gridMain.OptionsFind.AlwaysVisible = false;
            //else
            //    gridMain.OptionsFind.AlwaysVisible = true;
        }
        /****************************************************************************************/
        private DataTable oldDt = null;
        private void btnGetDeposits_Click(object sender, EventArgs e)
        {
            if (workReport == "Funeral Detail Report")
            {
                GetLocationDeposits();
                return;
            }

            btnDraftReport.Show();
            btnDraftReport.Refresh();
            if (workReport == "ACH Detail Report" || workReport == "Cover Report" )
            {
                btnEditCombos.Show();
                btnEditCombos.Refresh();
                chkUseCombos.Show();
                chkUseCombos.Refresh();
            }


            this.Cursor = Cursors.WaitCursor;
            DateTime stopDate = this.dateTimePicker1.Value;
            DateTime startDate = new DateTime(stopDate.Year, stopDate.Month, 1);

            btnSave.Hide();

            //startDate = new DateTime();2022, 10, 3
            //stopDate = startDate;

            PleaseWait pleaseForm = null;
            pleaseForm = new PleaseWait("Please Wait!\nLoading Information");
            pleaseForm.Show();
            pleaseForm.Refresh();

            string whatBox = workReport;

            bankDetails = GetBankAccount(whatBox);

            DataRow[] dRows = null;
            DateTime date = DateTime.Now;

            DataTable dt = LoadMain(startDate, stopDate);

            string searchLocation = "";
            if (workReport == "Cover Report")
                searchLocation = "ALL";

            oldDt = LoadData(startDate, stopDate, bankDetails, searchLocation );
            if ( oldDt.Rows.Count > 0 )
            {
                for ( int i=0; i<oldDt.Rows.Count; i++)
                {
                    date = oldDt.Rows[i]["date"].ObjToDateTime();
                    dRows = dt.Select("aDate='" + date.ToString("yyyyMMdd") + "'");
                    if (dRows.Length > 0)
                    {
                        CombineData(dRows[0], oldDt.Rows[i]);
                        //CombineData(oldDt.Rows[i], dRows[0]);
                    }
                }
            }

            G1.NumberDataTable(dt);
            dgv.DataSource = dt;


            string date1 = G1.DateTimeToSQLDateTime(startDate);
            string date2 = G1.DateTimeToSQLDateTime(stopDate);
            DateTime aDate = DateTime.Now;

            DataTable dx = (DataTable)dgv.DataSource;

            if (paymentDetail != null)
            {
                paymentDetail.Rows.Clear();
                paymentDetail.Dispose();
            }

            paymentDetail = null;

            double endingBalance = 0D;
            string accountTitle = "";
            beginningBalance = getBeginningBalance(bankDetails, startDate, ref endingBalance, ref accountTitle );
            string str = G1.ReformatMoney(beginningBalance);
            txtBeginningBalance.Text = str;

            //zeroOutColumn(dx, "DDA");
            //zeroOutColumn(dx, "TDA");
            //zeroOutColumn(dx, "IDA");
            //zeroOutColumn(dx, "NDA");
            //zeroOutColumn(dx, "returns");


            DataRow dR = dx.NewRow();
            dR["date"] = G1.DTtoMySQLDT(startDate);
            dR["comment"] = "Balance Forward";
            dR["balance"] = beginningBalance;
            //dR["bank_account"] = bankDetails;
            //dR["accountTitle"] = whatBox;
            dx.Rows.InsertAt(dR, 0);

            dx = getTrustPayments(dx, startDate, stopDate );
            dx = getInsurancePayments(dx, startDate, stopDate );
            dx = getFuneralPayments(dx, startDate, stopDate);

            dx = loadBankDebits(dx, startDate, stopDate, "" );

            DataView tempview = dx.DefaultView;
            tempview.Sort = "adate";
            dx = tempview.ToTable();


            LoadDOW(dx);

            gridMain.Columns["depositNumber"].Visible = false;
            gridMain.Columns["serviceId"].Visible = false;

            double balance = RecalcTotals(dx);
            balance = G1.RoundValue(balance);

            if (workReport == "Cover Report")
            {
                if (endingBalance != balance)
                    btnSave.Show();
                else
                    btnSave.Hide();
                btnSave.Refresh();
            }

            dx = SortDownTable(dx);

            G1.NumberDataTable(dx);
            dgv.DataSource = dx;

            str = bankDetails.Replace("~", " / ");

            this.Text = "ACH Deposit Report (" + whatBox + ")";
            gridBand5.Caption = "ACH Deposits for " + str + " " + accountTitle;
            majorBankAccount = str;
            majorBankAccount = majorBankAccount.Replace(" / ", "~");
            this.Cursor = Cursors.Default;

            pleaseForm.FireEvent1();
            pleaseForm.Dispose();
            pleaseForm = null;
        }
        /****************************************************************************************/
        private DataTable SortDownTable(DataTable dt)
        {
            string depositNumber = "";
            DateTime date = DateTime.Now;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                depositNumber = dt.Rows[i]["depositNumber"].ObjToString();
                if (String.IsNullOrWhiteSpace(depositNumber))
                {
                    date = dt.Rows[i]["date"].ObjToDateTime();
                    depositNumber = "Z" + date.ToString("yyyyMMdd");
                    dt.Rows[i]["depositNumber"] = depositNumber;
                }
            }

            DataView tempview = dt.DefaultView;
            tempview.Sort = "date asc, depositNumber asc";
            dt = tempview.ToTable();
            return dt;
        }
        /****************************************************************************************/
        private void GetLocationDeposits ()
        {
            DateTime stopDate = this.dateTimePicker1.Value;
            DateTime startDate = new DateTime(stopDate.Year, stopDate.Month, 1);

            string locationList = chkComboLocation.Text.Trim();
            if (String.IsNullOrWhiteSpace(locationList))
            {
                MessageBox.Show("*** ERROR ***\nYou must select some locations before getting deposits", "Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            PleaseWait pleaseForm = null;
            pleaseForm = new PleaseWait("Please Wait!\nLoading Information");
            pleaseForm.Show();
            pleaseForm.Refresh();

            string[] Lines = locationList.Split('|');

            string accountTitle = "";
            string accountNumber = "";
            string localDescription = "";
            string generalLedger = "";
            string bank_account = "";
            string cmd = "";
            DataTable payDt = null;
            string str = "";
            int row = 0;

            DataTable firstDx = null;

            this.Cursor = Cursors.WaitCursor;

            DataTable mainDt = null;
            DataTable ddd = null;

            string bankDetails = "";

            DataTable bankDt = G1.get_db_data("Select * from `bank_accounts` WHERE `account_title` = 'Cash - The First - Remote';");
            if ( bankDt.Rows.Count > 0 )
            {
                accountNumber = bankDt.Rows[0]["account_no"].ObjToString();
                accountTitle = bankDt.Rows[0]["account_title"].ObjToString();
                localDescription = bankDt.Rows[0]["localDescription"].ObjToString();
                generalLedger = bankDt.Rows[0]["general_ledger_no"].ObjToString();
                bank_account = localDescription + "~" + generalLedger + "~" + accountNumber;
            }


            funDt = G1.get_db_data("Select * from `funeralHomes`;");
            DataTable reportDt = (DataTable) chkComboLocation.Properties.DataSource;
            DataRow[] dRows = null;

            string atNeedCode = "";
            string mercCode = "";
            string[] subLines = null;
            string specialSearch = "";

            //startDate = new DateTime(2022, 10, 3);
            //stopDate = startDate;

            string date1 = G1.DateTimeToSQLDateTime(startDate);
            string date2 = G1.DateTimeToSQLDateTime(stopDate);

            for (int i = 0; i < Lines.Length; i++)
            {
                accountTitle = Lines[i].Trim();
                if (String.IsNullOrWhiteSpace(accountTitle))
                    continue;
                dRows = reportDt.Select("reportName='" + accountTitle + "'");
                if (dRows.Length <= 0)
                    continue;
                ddd = dRows.CopyToDataTable();

                cmd = "Select * from `cust_payment_details` c JOIN `fcust_extended` f ON c.`contractNumber` = f.`contractNumber` WHERE c.`dateReceived` >= '" + date1 + "' and c.`dateReceived` <= '" + date2 + "' ";
                cmd += " AND `bankAccount` = '" + accountNumber + "' ";
                str = dRows[0]["funeralLocations"].ObjToString();
                atNeedCode = "";

                subLines = str.Split(',');
                for ( int j=0; j<subLines.Length; j++)
                {
                    if (String.IsNullOrWhiteSpace(subLines[j]))
                        continue;
                    if (!String.IsNullOrWhiteSpace(atNeedCode))
                        atNeedCode += ",";
                    atNeedCode += subLines[j].Trim();
                }
                str = dRows[0]["merchandiseLocations"].ObjToString();

                subLines = str.Split(',');
                for (int j = 0; j < subLines.Length; j++)
                {
                    if (String.IsNullOrWhiteSpace(subLines[j]))
                        continue;
                    if (!String.IsNullOrWhiteSpace(atNeedCode))
                        atNeedCode += ",";
                    atNeedCode += subLines[j].Trim();
                }
                //if (!String.IsNullOrWhiteSpace(atNeedCode))
                //    cmd += " AND `serviceLoc` IN (" + atNeedCode + ")";

                if (!String.IsNullOrWhiteSpace(atNeedCode))
                {
                    subLines = atNeedCode.Split(',');
                    cmd += " AND ( ";
                    for (int j = 0; j < subLines.Length; j++)
                    {
                        if (j > 0)
                            cmd += " OR ";
                        cmd += " c.`depositNumber` LIKE '" + subLines[j].Trim() + "%' ";
                    }
                    cmd += " ) ";

                    specialSearch = atNeedCode;

                    //cmd += " AND c.`depositNumber` IN (" + atNeedCode + ")";
                }

                if (String.IsNullOrWhiteSpace(atNeedCode) )
                    continue;
                cmd += ";";

                payDt = G1.get_db_data(cmd);
                if (payDt.Rows.Count <= 0)
                {
                    this.Cursor = Cursors.Default;
                    continue;
                }
                string depositNu = payDt.Rows[0]["depositNumber"].ObjToString();

                string whatBox = accountTitle;

                bankDetails = whatBox;

                DataTable dt = LoadData(startDate, stopDate, bank_account, specialSearch );

                //if (dt.Rows.Count <= 0)
                //    dt = LoadMain(startDate, stopDate);

                //G1.NumberDataTable(dt);
                //dgv.DataSource = dt;

                //DataTable dx = (DataTable)dgv.DataSource;

                DataTable dx = dt.Copy();

                DateTime aDate = DateTime.Now;

                if (paymentDetail != null)
                {
                    paymentDetail.Rows.Clear();
                    paymentDetail.Dispose();
                }

                paymentDetail = null;

                double endingBalance = 0D;
                //accountTitle = "";
                //beginningBalance = getBeginningBalance(bankDetails, startDate, ref endingBalance, ref accountTitle);
                //str = G1.ReformatMoney(beginningBalance);
                //txtBeginningBalance.Text = str;

                //zeroOutColumn(dx, "DDA");
                //zeroOutColumn(dx, "TDA");
                //zeroOutColumn(dx, "IDA");
                //zeroOutColumn(dx, "NDA");
                //zeroOutColumn(dx, "returns");
                //zeroOutColumn(dx, "transfers");
                //zeroOutColumn(dx, "misc");

                if (G1.get_column_number(dx, "accountTitle") < 0)
                    dx.Columns.Add("accountTitle");

                getFuneralDetailPayments(dx, startDate, stopDate, payDt );

                dx = loadBankDebits(dx, startDate, stopDate, accountNumber );

                if (mainDt == null)
                    mainDt = dx.Clone();

                firstDx = dx.Clone();

                if (G1.get_column_number(mainDt, "accountTitle") < 0)
                    mainDt.Columns.Add("accountTitle");

                for ( int j=0; j<dx.Rows.Count; j++)
                    G1.copy_dt_row(dx, j, mainDt, mainDt.Rows.Count);
            }

            if (mainDt == null)
            {
                ddd = (DataTable)dgv.DataSource;
                if (ddd == null)
                    return;
                if (ddd.Rows.Count > 0)
                {
                    ddd.Rows.Clear();
                    dgv.DataSource = ddd;
                }
                return;
            }

            if (mainDt.Rows.Count <= 0)
            {
                ddd = (DataTable)dgv.DataSource;
                if (ddd.Rows.Count > 0)
                {
                    ddd.Rows.Clear();
                    dgv.DataSource = ddd;
                }
                return;
            }

            dRows = mainDt.Select("manual='Y'");
            if ( dRows.Length > 0 )
            {
                for (int i = 0; i < dRows.Length; i++)
                    dRows[i]["accountTitle"] = dRows[i]["location"].ObjToString();
            }

            DataView tempview = mainDt.DefaultView;
            tempview.Sort = "accountTitle,date";
            mainDt = tempview.ToTable();

            LoadDOW(mainDt);

            mainDt = SortDownTable(mainDt);

            RecalcDailyTotals(mainDt);

            gridMain.Columns["accountTitle"].GroupIndex = 0;

            //if (endingBalance != balance)
            //    btnSave.Show();
            //else
            //    btnSave.Hide();
            btnSave.Refresh();

            //mainDt = SortDownTable(mainDt);

            G1.NumberDataTable(mainDt);
            dgv.DataSource = mainDt;

            gridMain.OptionsView.ShowFooter = true;
            this.gridMain.ExpandAllGroups();
            gridMain.RefreshEditor(true);

            str = bankDetails.Replace("~", " / ");

            this.Text = "Funeral Detail Deposit Report";
            gridBand5.Caption = "Funeral Detail Deposits for " + str + " " + accountTitle;
            this.Cursor = Cursors.Default;

            pleaseForm.FireEvent1();
            pleaseForm.Dispose();
            pleaseForm = null;
        }
        /***************************************************************************************/
        private double RecalcTotals ( DataTable dt )
        {
            double originalEndingBalance = 0D;
            double dda = 0D;
            double tda = 0D;
            double ida = 0D;
            double nda = 0D;
            double returns = 0D;
            double transfers = 0D;
            double dailyTotals = 0D;
            double balance = beginningBalance;

            if ( G1.get_column_number ( dt, "dailyTotals") < 0 )
                dt.Columns.Add("dailyTotals", Type.GetType("System.Double"));

            string box = workReport.ToUpper();
            bool gotNox = false;

            DateTime lastDate = DateTime.MinValue;
            DateTime date = DateTime.Now;

            for ( int i=0; i<dt.Rows.Count; i++)
            {
                date = dt.Rows[i]["date"].ObjToDateTime();
                if (lastDate == DateTime.MinValue)
                    lastDate = date;
                if (date != lastDate)
                {
                    dt.Rows[i-1]["dailyTotals"] = dailyTotals;
                    dailyTotals = 0D;
                    lastDate = date;
                }

                dt.Rows[i]["dailyTotals"] = 0D;

                dda = dt.Rows[i]["DDA"].ObjToDouble();
                tda = dt.Rows[i]["TDA"].ObjToDouble();
                ida = dt.Rows[i]["ida"].ObjToDouble();
                nda = dt.Rows[i]["nda"].ObjToDouble();
                if (nda != 0D)
                    gotNox = true;
                if (workReport == "ACH Detail Report")
                {
                    nda = 0D;
                    transfers = 0D;
                }
                returns = dt.Rows[i]["returns"].ObjToDouble();
                transfers = dt.Rows[i]["transfers"].ObjToDouble();

                balance = balance + dda + tda + ida + nda - returns - transfers;
                balance = G1.RoundValue(balance);
                dt.Rows[i]["balance"] = balance;

                dailyTotals += dda + tda + ida + nda;
                dailyTotals = G1.RoundValue(dailyTotals);

                if ( i == (dt.Rows.Count - 1))
                {
                    dt.Rows[i]["dailyTotals"] = dailyTotals;
                    dailyTotals = 0D;
                }
            }

            return balance;
        }
        /***************************************************************************************/
        private double RecalcDailyTotals(DataTable dt)
        {
            double originalEndingBalance = 0D;
            double dda = 0D;
            double tda = 0D;
            double ida = 0D;
            double nda = 0D;
            double returns = 0D;
            double transfers = 0D;
            double dailyTotals = 0D;
            double balance = beginningBalance;

            string accountTitle = "";
            string lastTitle = "";

            if ( G1.get_column_number ( dt, "dailyTotals") < 0 )
                dt.Columns.Add("dailyTotals", Type.GetType("System.Double"));

            string box = workReport.ToUpper();
            bool gotNox = false;

            DateTime lastDate = DateTime.MinValue;
            DateTime date = DateTime.Now;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                accountTitle = dt.Rows[i]["accountTitle"].ObjToString();
                if (String.IsNullOrWhiteSpace(lastTitle))
                    lastTitle = accountTitle;
                date = dt.Rows[i]["date"].ObjToDateTime();
                if (lastDate == DateTime.MinValue)
                    lastDate = date;
                if (date != lastDate || lastTitle != accountTitle )
                {
                    dt.Rows[i - 1]["dailyTotals"] = dailyTotals;
                    dailyTotals = 0D;
                    lastDate = date;

                    lastTitle = accountTitle;
                }

                dt.Rows[i]["dailyTotals"] = 0D;

                dda = dt.Rows[i]["DDA"].ObjToDouble();
                tda = dt.Rows[i]["TDA"].ObjToDouble();
                ida = dt.Rows[i]["ida"].ObjToDouble();
                nda = dt.Rows[i]["nda"].ObjToDouble();
                if (nda != 0D)
                    gotNox = true;
                returns = dt.Rows[i]["returns"].ObjToDouble();
                transfers = dt.Rows[i]["transfers"].ObjToDouble();

                balance = balance + dda + tda + ida + nda - returns - transfers;
                balance = G1.RoundValue(balance);
                dt.Rows[i]["balance"] = balance;

                dailyTotals += dda + tda + ida + nda - returns;
                dailyTotals = G1.RoundValue(dailyTotals);

                if (i == (dt.Rows.Count - 1))
                {
                    dt.Rows[i]["dailyTotals"] = dailyTotals;
                    dailyTotals = 0D;
                }
            }

            return balance;
        }
        /***************************************************************************************/
        private void LoadDOW ( DataTable dt )
        {
            if ( G1.get_column_number ( dt, "dow") < 0 )
                dt.Columns.Add("dow");
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                dt.Rows[i]["dow"] = G1.DayOfWeekText(dt.Rows[i]["date"].ObjToDateTime());
            }
        }
        /***************************************************************************************/
        private DataTable bankDt = null;
        private string GetBankAccount( string what, string where = "" )
        {
            string location = "";
            string localDescription = "";
            string bank_gl = "";
            string bankAccount = "";
            if (String.IsNullOrWhiteSpace(where))
                where = "Funeral";

            string bankDetails = "";
            string cmd = "";
            if (what == "LKBX")
                cmd = "Select * from `bank_accounts` where `lkbx_ach` = '1';";
            else if (what == "TFBX")
                cmd = "Select * from `bank_accounts` where `tfbx` = '1';";
            else if (what.ToUpper() == "COVER REPORT")
                cmd = "Select * from `bank_accounts` where `ach` = '1';";
            else if (what.ToUpper() == "ACH DETAIL REPORT")
                cmd = "Select * from `bank_accounts` where `ach` = '1';";
            else if (what.ToUpper() == "FUNERAL DETAIL REPORT")
                cmd = "Select * from `bank_accounts` where `ach` = '1';";
            else if (what.ToUpper() == "CC TRUST AND INSURANCE")
                cmd = "Select * from `bank_accounts` where `ccInsTrusts` = '1';";
            else if (what.ToUpper() == "CC FUNERALS")
                cmd = "Select * from `bank_accounts` where `funeral` = '1';";

            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                location = dx.Rows[0]["location"].ObjToString();
                localDescription = dx.Rows[0]["localDescription"].ObjToString();

                if (what.ToUpper() == "COVER REPORT")
                {
                    if (where.ToUpper() == "FUNERAL")
                        location = localDescription;
                }
                else if (what.ToUpper() == "CC TRUST AND INSURANCE")
                {
                    if (where.ToUpper() == "FUNERAL")
                        location = localDescription;
                }
                else if (what.ToUpper() == "CC FUNERALS")
                {
                    if (where.ToUpper() == "FUNERAL")
                        location = localDescription;
                }
                else if (what.ToUpper() == "ACH DETAIL REPORT")
                {
                    //location = localDescription;
                    if (where.ToUpper() == "FUNERAL")
                        location = localDescription;
                }
                else if (what.ToUpper() == "FUNERAL DETAIL REPORT")
                    location = localDescription;
                bank_gl = dx.Rows[0]["general_ledger_no"].ObjToString();
                bankAccount = dx.Rows[0]["account_no"].ObjToString();
                bankDetails = location + "~" + bank_gl + "~" + bankAccount;

                bankDt = dx.Copy();
            }
            return bankDetails;
        }
        /****************************************************************************************/
        DataTable getTrustPayments ( DataTable dx, DateTime startDate, DateTime stopDate )
        {
            string date1 = G1.DateTimeToSQLDateTime(startDate);
            string date2 = G1.DateTimeToSQLDateTime(stopDate);

            try
            {
                string box = workReport.ToUpper();

                string bankDetails = GetBankAccount(box, "Trust");
                if (String.IsNullOrWhiteSpace(bankDetails))
                    return dx;

                string cmd = "Select * from `payments` p LEFT JOIN `customers` f ON p.`contractNumber` = f.`contractNumber` WHERE `payDate8` >= '" + date1 + "' and `payDate8` <= '" + date2 + "' AND `bank_account` = '" + bankDetails + "' ORDER BY `payDate8` asc;";
                DataTable dt = G1.get_db_data(cmd);
                if (dt.Rows.Count <= 0)
                    return dx;

                if (paymentDetail == null)
                    paymentDetail = dt.Clone();

                DailyHistory.RemoveDeletedPayments(dt);

                DateTime oldDate = DateTime.Now;
                DateTime date = DateTime.Now;
                double dailyAmount = 0D;
                double paymentAmount = 0D;
                double credit = 0D;
                double debit = 0D;
                double downPayment = 0D;
                double payment = 0D;
                double totals = 0D;
                bool first = true;
                string depositNumber = "";
                string location = "";
                string edited = "";
                int day = 0;
                DateTime testDate = new DateTime(2019, 7, 31);
                string c = "";
                string fill1 = "";
                string firstChar = "";
                int numChars = 4;
                string fName = "";
                string lName = "";
                string comment = "";
                string comment1 = "";

                string what = workReport;

                DataRow[] dRows = null;

                string contractNumber = "";

                TimeSpan ts;

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                    date = dt.Rows[i]["payDate8"].ObjToDateTime();
                    paymentAmount = dt.Rows[i]["paymentAmount"].ObjToDouble();
                    credit = dt.Rows[i]["creditAdjustment"].ObjToDouble();
                    debit = dt.Rows[i]["debitAdjustment"].ObjToDouble();
                    downPayment = dt.Rows[i]["downPayment"].ObjToDouble();
                    payment = paymentAmount;
                    depositNumber = dt.Rows[i]["depositNumber"].ObjToString();
                    location = dt.Rows[i]["location"].ObjToString();
                    edited = dt.Rows[i]["edited"].ObjToString().ToUpper();
                    if (edited == "TRUSTADJ" || edited == "CEMETERY")
                        continue;
                    if (edited == "MANUAL")
                    {
                        if (workReport.ToUpper() != "CC TRUST AND INSURANCE" && workReport != "CC FUNERALS")
                        {
                            if (debit <= 0D)
                                continue;
                        }
                    }

                    if (String.IsNullOrWhiteSpace(depositNumber))
                        continue;

                    firstChar = depositNumber.ToUpper().Substring(0, 1);
                    if (box == "ACH")
                    {
                        if (debit > 0D)
                        {
                            if (contractNumber.ToUpper().StartsWith("ZZ"))
                                continue;
                        }
                        else
                        {
                            if (firstChar != "A")
                                continue;
                        }
                    }
                    else
                    {
                        if (firstChar == "T")
                        {
                            if (box != "TFBX" && fill1.ToUpper() == "TFBX")
                                continue;
                        }
                        else
                        {
                            if (debit > 0D)
                            {
                                if (contractNumber.ToUpper().StartsWith("ZZ"))
                                    continue;
                            }
                        }
                    }

                    G1.copy_dt_row(dt, i, paymentDetail, paymentDetail.Rows.Count);

                    totals += payment;

                    dRows = dx.Select("aDate='" + date.ToString("yyyyMMdd") + "'");
                    if (dRows.Length > 0)
                    {
                        dailyAmount = dRows[0]["TDA"].ObjToDouble();
                        dailyAmount += payment;
                        dRows[0]["TDA"] = dailyAmount;

                        if (debit > 0D)
                        {
                            dailyAmount = dRows[0]["returns"].ObjToDouble();
                            dailyAmount += debit;
                            dRows[0]["returns"] = dailyAmount;
                            comment = dRows[0]["comment"].ObjToString();
                            if (!String.IsNullOrWhiteSpace(comment))
                                comment += "\n";
                            comment += dt.Rows[i]["contractNumber"].ObjToString() + " " + G1.ReformatMoney(debit);
                            dRows[0]["comment"] = comment;
                        }
                    }
                    else
                    {
                    }
                }
                dx.AcceptChanges();
            }
            catch (Exception ex)
            {
            }
            return dx;
        }
        /****************************************************************************************/
        DataTable getFuneralPayments(DataTable dx, DateTime startDate, DateTime stopDate)
        {
            string date1 = G1.DateTimeToSQLDateTime(startDate);
            string date2 = G1.DateTimeToSQLDateTime(stopDate);

            string cmd = "Select * from `funeralReports` ORDER by `order`;";
            DataTable locDt = G1.get_db_data(cmd);

            try
            {
                string box = workReport.ToUpper();

                string bankDetails = GetBankAccount(box, "Funeral" );
                if (String.IsNullOrWhiteSpace(bankDetails))
                    return dx;

                string[] Lines = bankDetails.Split('~');
                if (Lines.Length < 3)
                    return dx;

                string description = Lines[0];
                string bankAccount = Lines[2];

                cmd = "Select * from `cust_payment_details` where `dateReceived` >= '" + date1 + "' and `dateReceived` <= '" + date2 + "' AND `bankAccount` = '" + bankAccount + "' AND `localDescription` = '" + description + "' ORDER BY `dateReceived` asc;";
                DataTable dt = G1.get_db_data(cmd);
                if (dt.Rows.Count <= 0)
                    return dx;

                if (paymentDetail == null)
                    paymentDetail = dt.Clone();

                //DailyHistory.RemoveDeletedPayments(dt);

                DateTime oldDate = DateTime.Now;
                DateTime date = DateTime.Now;
                double dailyAmount = 0D;
                double paymentAmount = 0D;
                double credit = 0D;
                double debit = 0D;
                double downPayment = 0D;
                double payment = 0D;
                double totals = 0D;
                bool first = true;
                string depositNumber = "";
                string location = "";
                string edited = "";
                int day = 0;
                DateTime testDate = new DateTime(2019, 7, 31);
                string c = "";
                string fill1 = "";
                string firstChar = "";
                int numChars = 4;

                string what = workReport.ToUpper();

                DataRow[] dRows = null;
                DataRow dRow = null;

                string contractNumber = "";
                string status = "";

                TimeSpan ts;

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    status = dt.Rows[i]["status"].ObjToString().ToUpper();
                    if (status != "RECEIVED" && status != "DEPOSITED"&& status != "DEBIT" )
                        continue;
                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                    date = dt.Rows[i]["dateReceived"].ObjToDateTime();
                    paymentAmount = dt.Rows[i]["amtActuallyReceived"].ObjToDouble();
                    if ( paymentAmount <= 0D)
                        paymentAmount = dt.Rows[i]["paid"].ObjToDouble();
                    payment = paymentAmount;
                    depositNumber = dt.Rows[i]["depositNumber"].ObjToString();

                    if (String.IsNullOrWhiteSpace(depositNumber))
                        continue;

                    firstChar = depositNumber.ToUpper().Substring(0, 1);

                    dRow = paymentDetail.NewRow();
                    dRow["contractNumber"] = dt.Rows[i]["contractNumber"].ObjToString();
                    dRow["payDate8"] = G1.DTtoMySQLDT(date);
                    dRow["paymentAmount"] = paymentAmount;
                    dRow["depositNumber"] = depositNumber;
                    dRow["location"] = depositNumber;
                    if ( depositNumber.Length >= 2 )
                    {
                        location = depositNumber.Substring(0, 2);
                        dRow["location"] = location;
                    }
                    paymentDetail.Rows.Add(dRow);

                    //G1.copy_dt_row(dt, i, paymentDetail, paymentDetail.Rows.Count);

                    totals += payment;

                    dRows = dx.Select("aDate='" + date.ToString("yyyyMMdd") + "'");
                    if (dRows.Length > 0)
                    {
                        if (status == "DEBIT")
                        {
                            dailyAmount = dRows[0]["Returns"].ObjToDouble();
                            dailyAmount += payment;
                            dRows[0]["Returns"] = dailyAmount;
                        }
                        else
                        {
                            dailyAmount = dRows[0]["NDA"].ObjToDouble();
                            dailyAmount += payment;
                            dRows[0]["NDA"] = dailyAmount;
                        }
                    }
                    else
                    {
                    }
                }
                dx.AcceptChanges();
            }
            catch (Exception ex)
            {
            }
            return dx;
        }
        /****************************************************************************************/
        DataTable getFuneralDetailPayments(DataTable dx, DateTime startDate, DateTime stopDate, DataTable dt )
        {
            string date1 = G1.DateTimeToSQLDateTime(startDate);
            string date2 = G1.DateTimeToSQLDateTime(stopDate);

            if (G1.get_column_number(dx, "trusts") < 0)
                dx.Columns.Add("trusts");
            if (G1.get_column_number(dx, "insurance") < 0)
                dx.Columns.Add("insurance");

            try
            {
                if (paymentDetail == null)
                    paymentDetail = dt.Clone();

                DateTime oldDate = DateTime.Now;
                DateTime date = DateTime.Now;
                double dailyAmount = 0D;
                double paymentAmount = 0D;
                double credit = 0D;
                double debit = 0D;
                double downPayment = 0D;
                double payment = 0D;
                double totals = 0D;
                bool first = true;
                string depositNumber = "";
                string location = "";
                string edited = "";
                int day = 0;
                DateTime testDate = new DateTime(2019, 7, 31);
                string c = "";
                string fill1 = "";
                string firstChar = "";
                string serviceId = "";
                int numChars = 4;
                string type = "";
                string paymentRecord = "";
                string dor = "";

                string what = workReport.ToUpper();

                DataRow[] dRows = null;
                DataRow dR = null;

                string contractNumber = "";
                string status = "";

                TimeSpan ts;

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dor = "";
                    status = dt.Rows[i]["status"].ObjToString().ToUpper();
                    if (status != "RECEIVED" && status != "DEPOSITED" && status != "DEBIT" && status != "CANCELLED" )
                        continue;
                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                    date = dt.Rows[i]["dateReceived"].ObjToDateTime();
                    paymentAmount = dt.Rows[i]["amtActuallyReceived"].ObjToDouble();
                    if (paymentAmount <= 0D)
                        paymentAmount = dt.Rows[i]["paid"].ObjToDouble();
                    payment = paymentAmount;
                    depositNumber = dt.Rows[i]["depositNumber"].ObjToString();
                    type = dt.Rows[i]["type"].ObjToString();
                    serviceId = dt.Rows[i]["serviceId"].ObjToString();
                    paymentRecord = dt.Rows[i]["paymentRecord"].ObjToString();

                    //if (String.IsNullOrWhiteSpace(depositNumber))
                    //    continue;

                    //firstChar = depositNumber.ToUpper().Substring(0, 1);

                    //G1.copy_dt_row(dt, i, paymentDetail, paymentDetail.Rows.Count);

                    totals += payment;

                    dR = dx.NewRow();

                    location = dt.Rows[i]["serviceLoc"].ObjToString();
                    dR["accountTitle"] = location;
                    dR["depositNumber"] = depositNumber;

                    if ( depositNumber.Length >= 2)
                        dR["accountTitle"] = depositNumber.Substring(0, 2);
                    dR["serviceId"] = serviceId;

                    dR["date"] = G1.DTtoMySQLDT(dt.Rows[i]["dateReceived"].ObjToDateTime());
                    date = dt.Rows[i]["dateReceived"].ObjToDateTime();
                    dR["aDate"] = date.ToString("yyyyMMdd");

                    //dR["comment"] = contractNumber + " " + depositNumber;

                    if (status == "DEBIT" || status == "CANCELLED")
                    {
                        //dR["returns"] = payment;
                        if (status == "CANCELLED")
                        {
                            //if (dt.Rows[i]["notes"].ObjToString().ToUpper().IndexOf("DOR=") == 0)
                            //    dor = dt.Rows[i]["notes"].ObjToString();
                            dR["NDA"] = payment;
                            //dR["comment"] = dt.Rows[i]["notes"].ObjToString();
                        }
                    }
                    else
                        dR["NDA"] = payment;

                    if (type.ToUpper() == "TRUST")
                        dR["trusts"] = contractNumber;
                    else if (type.ToUpper().IndexOf("INSURANCE") >= 0)
                        dR["insurance"] = getPayer(paymentRecord);
                    else if (type.ToUpper() == "CLASS A")
                        dR["insurance"] = getPayer(paymentRecord);
                    dx.Rows.Add(dR);
                    if (!String.IsNullOrWhiteSpace(dor))
                    {
                        dor = dor.ToUpper().Replace("DOR=", "");
                        DateTime dorDate = dor.ObjToDateTime();
                        if (G1.validate_date(dor))
                        {
                            dR = dx.NewRow();
                            var sourceRow = dx.Rows[dx.Rows.Count - 1];
                            dR.ItemArray = sourceRow.ItemArray.Clone() as object[];
                            dR["date"] = G1.DTtoMySQLDT(dor);
                            dR["aDate"] = dor.ObjToDateTime().ToString("yyyyMMdd");
                            dR["returns"] = payment;
                            dR["NDA"] = 0D;
                            dR["comment"] = dt.Rows[i]["notes"].ObjToString();
                            dx.Rows.Add(dR);
                        }
                    }
                }
                dx.AcceptChanges();
            }
            catch (Exception ex)
            {
            }
            return dx;
        }
        /****************************************************************************************/
        private DataTable loadBankDebits(DataTable dx, DateTime startDate, DateTime stopDate, string accountNumber )
        {
            string date1 = G1.DateTimeToSQLDateTime(startDate);
            string date2 = G1.DateTimeToSQLDateTime(stopDate);

            if (G1.get_column_number(dx, "bankDebit") < 0)
                dx.Columns.Add("bankDebit");
            if (G1.get_column_number(dx, "misc") < 0)
                dx.Columns.Add("misc", Type.GetType("System.Double"));

            try
            {
                string box = workReport.ToUpper();

                string bankDetails = GetBankAccount(box, "Trust");
                if (String.IsNullOrWhiteSpace(bankDetails))
                    return dx;

                string[] Lines = bankDetails.Split('~');
                if (Lines.Length < 3)
                    return dx;

                string description = Lines[0];
                string bankAccount = Lines[2];
                //if (String.IsNullOrWhiteSpace(accountNumber))
                //    accountNumber = bankAccount;

                string cmd = "Select * from `bank_details` p WHERE `date` >= '" + date1 + "' and `date` <= '" + date2 + "' AND `bankAccount` = '" + bankAccount + "' AND `debit` > '0.00' ORDER BY `date` asc;";
                DataTable dt = G1.get_db_data(cmd);
                if (dt.Rows.Count <= 0)
                    return dx;

                string assignTo = "";
                double debit = 0D;
                description = "";
                string debitDepNum = "";
                double dValue = 0D;

                DataRow dRow = null;
                DataRow[] dRows = null;
                DateTime date = DateTime.Now;
                string aDate = "";

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    date = dt.Rows[i]["date"].ObjToDateTime();
                    debit = dt.Rows[i]["debit"].ObjToDouble();
                    assignTo = dt.Rows[i]["assignTo"].ObjToString().ToUpper();
                    description = dt.Rows[i]["description"].ObjToString();
                    debitDepNum = dt.Rows[i]["debitDepNum"].ObjToString();

                    debit = debit * -1D;

                    if ( workReport == "Cover Report" && !String.IsNullOrWhiteSpace(assignTo) )
                    {
                        if (assignTo != "RETURN")
                            description = "";
                        aDate = date.ToString("yyyyMMdd");
                        dRows = dx.Select("aDate='" + aDate + "'" );
                        if ( dRows.Length > 0 )
                        {
                            CombineDetail(dRows[0], debit, assignTo, description );
                            continue;
                        }
                    }
                    if (workReport == "Funeral Detail Report" )
                    {
                        if (String.IsNullOrWhiteSpace(debitDepNum))
                            continue;
                        if (assignTo == "TRANSFER")
                            continue;
                        aDate = date.ToString("yyyyMMdd");
                        dRows = dx.Select("aDate='" + aDate + "'");
                        if (dRows.Length > 0)
                        {
                            CombineDetail(dRows[0], debit, assignTo, description);
                            continue;
                        }
                        continue;
                    }

                    if (!String.IsNullOrWhiteSpace(assignTo))
                    {
                        if (assignTo != "RETURN")
                            continue;
                        aDate = date.ToString("yyyyMMdd");
                        dRows = dx.Select("aDate='" + aDate + "'");
                        if (dRows.Length > 0)
                        {
                            CombineDetail(dRows[0], debit, assignTo, description );
                            continue;
                        }
                    }


                    dRow = dx.NewRow();
                    dRow["date"] = G1.DTtoMySQLDT(date);
                    dRow["adate"] = date.ToString("yyyyMMdd");
                    dRow["bankDebit"] = dt.Rows[i]["record"].ObjToString();
                    dValue = Math.Abs(debit);
                    dRow["comment"] = description + "~" + debitDepNum + " " + G1.ReformatMoney(dValue);
                    if (!String.IsNullOrWhiteSpace(accountNumber))
                    {
                        if (debitDepNum.Length >= 2)
                            dRow["accountTitle"] = debitDepNum.Substring(0, 2);
                    }
                    //dRow["depositNumber"] = debitDepNum;
                    if (assignTo == "TRUST DOWN PAYMENT")
                        dRow["DDA"] = debit;
                    else if (assignTo == "TRUST DEPOSIT")
                        dRow["TDA"] = debit;
                    else if (assignTo == "INSURANCE DEPOSIT")
                        dRow["IDA"] = debit;
                    else if (assignTo == "FUNERAL DEPOSIT")
                        dRow["NDA"] = debit;
                    else if (assignTo == "TRANSFER")
                        dRow["transfers"] = Math.Abs(debit);
                    else if (assignTo == "MISCELLANEOUS")
                        dRow["returns"] = debit;
                    dx.Rows.Add(dRow);
                }
            }
            catch (Exception ex)
            {
            }
            return dx;
        }
        /****************************************************************************************/
        private string getPayer ( string paymentRecord )
        {
            string payer = "";
            string cmd = "Select * from `cust_payments` where `record` = '" + paymentRecord + "';";
            DataTable dt = G1.get_db_data(cmd);
            if ( dt.Rows.Count > 0 )
            {
                payer = dt.Rows[0]["trust_policy"].ObjToString();
            }
            return payer;
        }
        /****************************************************************************************/
        private void zeroOutColumn(DataTable dx, string column)
        {
            if (G1.get_column_number(dx, column) < 0)
                return;
            for (int i = 0; i < dx.Rows.Count; i++)
                dx.Rows[i][column] = 0D;
        }
        /****************************************************************************************/
        DataTable getInsurancePayments(DataTable dx, DateTime startDate, DateTime stopDate )
        {
            string date1 = G1.DateTimeToSQLDateTime(startDate);
            string date2 = G1.DateTimeToSQLDateTime(stopDate);

            try
            {
                string box = workReport.ToUpper();
                string bankDetails = GetBankAccount(box, "Insurance" );
                if (String.IsNullOrWhiteSpace(bankDetails))
                    return dx;

                string cmd = "Select * from `ipayments` where `payDate8` >= '" + date1 + "' and `payDate8` <= '" + date2 + "' AND `bank_account` = '" + bankDetails + "' ORDER BY `payDate8` asc;";
                DataTable dt = G1.get_db_data(cmd);
                if (dt.Rows.Count <= 0)
                    return dx;

                if (paymentDetail == null)
                    paymentDetail = dt.Clone();

                DailyHistory.RemoveDeletedPayments(dt);

                DateTime oldDate = DateTime.Now;
                DateTime date = DateTime.Now;
                double dailyAmount = 0D;
                double paymentAmount = 0D;
                double credit = 0D;
                double debit = 0D;
                double downPayment = 0D;
                double payment = 0D;
                double totals = 0D;
                bool first = true;
                int day = 0;
                string depositNumber = "";
                string edited = "";
                string c = "";
                DateTime testDate = new DateTime(2019, 7, 31);

                DataRow[] dRows = null;

                string fill1 = "";
                string firstChar = "";
                int numChars = 4;

                string what = workReport;
                string contractNumber = "";

                TimeSpan ts;

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                    date = dt.Rows[i]["payDate8"].ObjToDateTime();
                    paymentAmount = dt.Rows[i]["paymentAmount"].ObjToDouble();
                    credit = dt.Rows[i]["creditAdjustment"].ObjToDouble();
                    debit = dt.Rows[i]["debitAdjustment"].ObjToDouble();
                    downPayment = dt.Rows[i]["downPayment"].ObjToDouble();
                    edited = dt.Rows[i]["edited"].ObjToString().ToUpper();

                    if ( edited == "TRUSTADJ" || edited == "CEMETERY")
                        continue;

                    if (edited == "MANUAL")
                    {
                        if (workReport.ToUpper() != "CC TRUST AND INSURANCE" && workReport != "CC FUNERALS")
                        {
                            if (debit <= 0D)
                                continue;
                        }
                    }
                    depositNumber = dt.Rows[i]["depositNumber"].ObjToString();
                    if (String.IsNullOrWhiteSpace(depositNumber))
                        continue;

                    firstChar = depositNumber.ToUpper().Substring(0, 1);
                    if (box == "ACH")
                    {
                        if (debit > 0D)
                        {
                            if (contractNumber.ToUpper().StartsWith("ZZ"))
                                continue;
                        }
                        else
                        {
                            if (firstChar != "A")
                                continue;
                        }
                    }
                    else
                    {
                        if (firstChar == "T")
                        {
                            if (box != "TFBX" && fill1.ToUpper() == "TFBX")
                                continue;
                        }
                        else
                        {
                            if (debit > 0D)
                            {
                                if (contractNumber.ToUpper().StartsWith("ZZ"))
                                    continue;
                            }
                        }
                    }

                    G1.copy_dt_row(dt, i, paymentDetail, paymentDetail.Rows.Count);

                    payment = paymentAmount;
                    totals += payment;

                    dRows = dx.Select("aDate='" + date.ToString("yyyyMMdd") + "'");
                    if (dRows.Length > 0)
                    {
                        dailyAmount = dRows[0]["IDA"].ObjToDouble();
                        dailyAmount += payment;
                        dRows[0]["IDA"] = dailyAmount;

                        if (debit > 0D)
                        {
                            dailyAmount = dRows[0]["returns"].ObjToDouble();
                            dailyAmount += debit;
                            dRows[0]["returns"] = dailyAmount;
                        }
                    }
                }
                dx.AcceptChanges();
            }
            catch ( Exception ex)
            {
            }
            return dx;
        }
        /****************************************************************************************/
        private bool CheckForSave ()
        {
            if (!btnSave.Visible)
                return true;
            DialogResult result = MessageBox.Show("***Question***\nInformation has been modified!\nWould you like to save your changes?", "Modified Data Dialog", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (result == DialogResult.Cancel)
                return false;
            if (result == DialogResult.No)
                return true;
            SaveData();
            return true;
        }
        /****************************************************************************************/
        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveData();
        }
        /****************************************************************************************/
        private void SaveFuneralData()
        {
            this.Cursor = Cursors.WaitCursor;
            DateTime stopDate = this.dateTimePicker1.Value;
            DateTime startDate = new DateTime(stopDate.Year, stopDate.Month, 1);
            string date1 = G1.DateTimeToSQLDateTime(startDate);
            string date2 = G1.DateTimeToSQLDateTime(stopDate);

            DateTime saveDate = DateTime.Now;
            string date = "";

            DataTable dt = (DataTable)dgv.DataSource;

            string accountNumber = "";
            string accountTitle = "";
            string localDescription = "";
            string generalLedger = "";
            string bankAccount = "";
            string bank_account = "";

            DataTable bankDt = G1.get_db_data("Select * from `bank_accounts` WHERE `account_title` = 'Cash - The First - Remote';");
            if (bankDt.Rows.Count > 0)
            {
                accountNumber = bankDt.Rows[0]["account_no"].ObjToString();
                accountTitle = bankDt.Rows[0]["account_title"].ObjToString();
                localDescription = bankDt.Rows[0]["localDescription"].ObjToString();
                generalLedger = bankDt.Rows[0]["general_ledger_no"].ObjToString();
                bankAccount = localDescription + "~" + generalLedger + "~" + accountNumber;
                bank_account = bankAccount;
            }
            if (String.IsNullOrWhiteSpace(accountNumber))
            {
                MessageBox.Show("*** ERROR ***\nThere are no Bank Account Assigned Here!", "Save Data Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }


            double tda = 0D;
            double ida = 0D;
            double nda = 0D;
            double dda = 0D;
            double misc = 0D;
            double returns = 0D;
            double transfer = 0D;
            double balance = 0D;
            string comment = "";
            string aDate = "";
            string record = "";
            string manual = "";
            string oldBankAccount = "";
            double endingBalance = 0D;

            string depositNumber = "";
            string serviceId = "";

            double credits = 0D;
            double debits = 0D;
            accountTitle = "";

            DataRow[] dRows = dt.Select("manual='Y'");
            if (dRows.Length <= 0)
            {
                this.Cursor = Cursors.Default;
                return;
            }
            dt = dRows.CopyToDataTable();

            DataView tempview = dt.DefaultView;
            tempview.Sort = "accountTitle";
            dt = tempview.ToTable();

            string location = "";

            DataTable groupDt = dt.AsEnumerable().GroupBy(r => new { Col1 = r["accountTitle"] }).Select(g => g.OrderBy(r => r["accountTitle"]).First()).CopyToDataTable();
            for ( int i=0; i<groupDt.Rows.Count; i++)
            {
                location = groupDt.Rows[i]["accountTitle"].ObjToString();
                string cmd = "DELETE FROM `lockboxdeposits` where `date` >= '" + date1 + "' AND `date` <= '" + date2 + "' AND `bank_account` = '" + bankAccount + "' ";
                cmd += " AND `location` = '" + location + "';";
                G1.get_db_data(cmd);
            }


            for (int i = 0; i < dt.Rows.Count; i++)
            {
                bankAccount = dt.Rows[i]["bank_account"].ObjToString();
                location = dt.Rows[i]["accountTitle"].ObjToString();
                saveDate = dt.Rows[i]["date"].ObjToDateTime();
                date = saveDate.ToString("MM/dd/yyyy");
                tda = dt.Rows[i]["TDA"].ObjToDouble();
                ida = dt.Rows[i]["IDA"].ObjToDouble();
                nda = dt.Rows[i]["NDA"].ObjToDouble();
                dda = dt.Rows[i]["DDA"].ObjToDouble();
                misc = dt.Rows[i]["misc"].ObjToDouble();
                returns = dt.Rows[i]["returns"].ObjToDouble();
                transfer = dt.Rows[i]["transfers"].ObjToDouble();
                balance = dt.Rows[i]["balance"].ObjToDouble();
                comment = dt.Rows[i]["comment"].ObjToString();
                if (comment.Trim().ToUpper() == "BALANCE FORWARD")
                    continue;
                aDate = dt.Rows[i]["adate"].ObjToString();
                accountTitle = dt.Rows[i]["accountTitle"].ObjToString();
                manual = dt.Rows[i]["manual"].ObjToString();

                tda = G1.RoundValue(tda);
                ida = G1.RoundValue(ida);
                nda = G1.RoundValue(nda);
                dda = G1.RoundValue(dda);
                misc = G1.RoundValue(misc);
                returns = G1.RoundValue(returns);
                transfer = G1.RoundValue(transfer);
                balance = G1.RoundValue(balance);

                credits += tda + ida + nda;
                debits += Math.Abs (misc ) + returns + transfer;

                if (manual.ToUpper() == "Y")
                {
                    record = G1.create_record("lockboxdeposits", "comment", "-1");
                    if (G1.BadRecord("lockboxdeposits", record))
                        break;
                    G1.update_db_table("lockboxdeposits", "record", record, new string[] { "date", date, "adate", aDate, "comment", comment, "TDA", tda.ToString(), "IDA", ida.ToString(), "NDA", nda.ToString(), "dda", dda.ToString(), "misc", misc.ToString(), "returns", returns.ToString(), "transfers", transfer.ToString(), "balance", balance.ToString(), "manual", manual, "bank_account", bank_account });
                    if (manual.ToUpper() == "Y")
                    {
                        depositNumber = dt.Rows[i]["depositNumber"].ObjToString();
                        serviceId = dt.Rows[i]["serviceId"].ObjToString();
                        G1.update_db_table("lockboxdeposits", "record", record, new string[] { "depositNumber", depositNumber, "serviceId", serviceId, "location", accountTitle });
                    }
                }
            }

            this.Cursor = Cursors.Default;
            btnSave.Hide();
        }
        /****************************************************************************************/
        private void SaveData()
        {
            if (workReport == "Funeral Detail Report")
            {
                SaveFuneralData();
                return;
            }

            string accountNumber = "";
            string accountTitle = "";
            string localDescription = "";
            string generalLedger = "";
            string bankAccount = "";
            string bank_account = "";

            DataTable bankDt = G1.get_db_data("Select * from `bank_accounts` WHERE `account_title` = 'Cash - The First - Remote';");
            if (bankDt.Rows.Count > 0)
            {
                accountNumber = bankDt.Rows[0]["account_no"].ObjToString();
                accountTitle = bankDt.Rows[0]["account_title"].ObjToString();
                localDescription = bankDt.Rows[0]["localDescription"].ObjToString();
                generalLedger = bankDt.Rows[0]["general_ledger_no"].ObjToString();
                bankAccount = localDescription + "~" + generalLedger + "~" + accountNumber;
                bank_account = bankAccount;
            }
            if (String.IsNullOrWhiteSpace(accountNumber))
            {
                MessageBox.Show("*** ERROR ***\nThere are no Bank Account Assigned Here!", "Save Data Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }


            this.Cursor = Cursors.WaitCursor;
            DateTime stopDate = this.dateTimePicker1.Value;
            DateTime startDate = new DateTime(stopDate.Year, stopDate.Month, 1);
            string date1 = G1.DateTimeToSQLDateTime(startDate);
            string date2 = G1.DateTimeToSQLDateTime(stopDate);

            DateTime saveDate = DateTime.Now;
            string date = "";

            //string cmd = "DELETE FROM `lockboxdeposits` where `date` >= '" + date1 + "' AND `date` <= '" + date2 + "' AND `bank_account` = '" + bank_account + "' ";
            //cmd += " AND `location` = '';";
            //if (workReport == "ACH Detail Report")
            //    G1.get_db_data(cmd);

            DataTable dt = (DataTable)dgv.DataSource;

            double dda = 0D;
            double tda = 0D;
            double ida = 0D;
            double nda = 0D;
            double returns = 0D;
            double transfer = 0D;
            double misc = 0D;
            double balance = 0D;
            string comment = "";
            string aDate = "";
            string record = "";
            string manual = "";

            double credits = 0D;
            double debits = 0D;

            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    saveDate = dt.Rows[i]["date"].ObjToDateTime();
                    date = saveDate.ToString("MM/dd/yyyy");
                    dda = dt.Rows[i]["DDA"].ObjToDouble();
                    tda = dt.Rows[i]["TDA"].ObjToDouble();
                    ida = dt.Rows[i]["IDA"].ObjToDouble();
                    nda = dt.Rows[i]["NDA"].ObjToDouble();
                    returns = dt.Rows[i]["returns"].ObjToDouble();
                    transfer = dt.Rows[i]["transfers"].ObjToDouble();
                    misc = dt.Rows[i]["misc"].ObjToDouble();
                    balance = dt.Rows[i]["balance"].ObjToDouble();
                    comment = dt.Rows[i]["comment"].ObjToString();
                    if (comment.Trim().ToUpper() == "BALANCE FORWARD")
                        continue;
                    aDate = dt.Rows[i]["adate"].ObjToString();

                    dda = G1.RoundValue(dda);
                    tda = G1.RoundValue(tda);
                    ida = G1.RoundValue(ida);
                    nda = G1.RoundValue(nda);
                    returns = G1.RoundValue(returns);
                    transfer = G1.RoundValue(transfer);
                    misc = G1.RoundValue(misc);
                    balance = G1.RoundValue(balance);
                    manual = dt.Rows[i]["manual"].ObjToString();

                    credits += dda + tda + ida + nda;
                    debits += Math.Abs (misc) + returns + transfer;

                    if (workReport == "ACH Detail Report")
                    {
                        if (manual.ToUpper() == "Y")
                        {
                            record = G1.create_record("lockboxdeposits", "comment", "-1");
                            if (G1.BadRecord("lockboxdeposits", record))
                                break;
                            G1.update_db_table("lockboxdeposits", "record", record, new string[] { "date", date, "adate", aDate, "comment", comment, "DDA", dda.ToString(), "TDA", tda.ToString(), "IDA", ida.ToString(), "NDA", nda.ToString(), "returns", returns.ToString(), "transfers", transfer.ToString(), "misc", misc.ToString(), "balance", balance.ToString(), "bank_account", bankDetails, "manual", manual });
                            dt.Rows[i]["record"] = record.ObjToInt64();
                        }
                    }
                }

                if (workReport == "Cover Report")
                    UpdateBankTotals(bankDetails, this.dateTimePicker1.Value, credits, debits, beginningBalance, balance);

                if (workReport == "ACH Detail Report")
                {
                    CleanupAddedRows(dt, bank_account );
                }
            }
            catch ( Exception ex )
            {
            }
            this.Cursor = Cursors.Default;
            btnSave.Hide();
        }
        /****************************************************************************************/
        private void CleanupAddedRows(DataTable dt, string bank_account )
        {
            DataRow [] dRows = dt.Select("manual='Y'");
            if (dRows.Length <= 0)
                return;
            DataTable eDt = dRows.CopyToDataTable();
            eDt.Columns.Add("mod");

            DataTable xDt = eDt.Copy();

            DataRow dRow = null;
            string record = "";
            DateTime date = DateTime.Now;

            try
            {
                for (int i = 0; i < eDt.Rows.Count; i++)
                {
                    record = eDt.Rows[i]["record"].ObjToString();
                    date = eDt.Rows[i]["date"].ObjToDateTime();
                    dRow = oldDt.NewRow();
                    dRow["record"] = record;
                    dRow["date"] = G1.DTtoMySQLDT(date);
                    dRow["adate"] = date.ToString("yyyyMMdd");
                    dRow["comment"] = eDt.Rows[i]["comment"].ObjToString();
                    dRow["returns"] = eDt.Rows[i]["returns"].ObjToDouble();
                    dRow["TDA"] = eDt.Rows[i]["TDA"].ObjToDouble();
                    dRow["IDA"] = eDt.Rows[i]["IDA"].ObjToDouble();
                    dRow["NDA"] = eDt.Rows[i]["NDA"].ObjToDouble();
                    dRow["DDA"] = eDt.Rows[i]["DDA"].ObjToDouble();
                    dRow["transfers"] = eDt.Rows[i]["transfers"].ObjToDouble();
                    dRow["misc"] = eDt.Rows[i]["misc"].ObjToDouble();
                    dRow["bank_account"] = bank_account;
                    dRow["manual"] = "Y";
                    oldDt.Rows.Add(dRow);
                }

                DateTime workDate = DateTime.Now;
                for (int i = 0; i < eDt.Rows.Count; i++)
                {
                    workDate = eDt.Rows[i]["date"].ObjToDateTime();
                    ResetEditRow(eDt, workDate, null, "comment");

                    ResetEditRow(eDt, workDate, null, "returns");
                    ResetEditRow(eDt, workDate, null, "TDA");
                    ResetEditRow(eDt, workDate, null, "IDA");
                    //            ResetEditRow(dd, workDate, originalDt, "PDA");
                    ResetEditRow(eDt, workDate, null, "NDA");
                    ResetEditRow(eDt, workDate, null, "DDA");
                    ResetEditRow(eDt, workDate, null, "misc");
                    ResetEditRow(eDt, workDate, null, "transfers");
                }
                for (int i = (dt.Rows.Count - 1); i >= 0; i--)
                {
                    if (dt.Rows[i]["manual"].ObjToString() == "Y")
                        dt.Rows.RemoveAt(i);
                }

                string oldRecord = "";
                dRows = oldDt.Select("manual='Y'");
                for (int i = 0; i < xDt.Rows.Count; i++)
                {
                    record = xDt.Rows[i]["record"].ObjToString();
                    for (int j = 0; j < dRows.Length; j++)
                    {
                        oldRecord = dRows[j]["record"].ObjToString();
                        if (record == oldRecord)
                        {
                            dRows[j]["comment"] = xDt.Rows[i]["comment"].ObjToString();
                            dRows[j]["returns"] = xDt.Rows[i]["returns"].ObjToDouble();
                            dRows[j]["TDA"]     = xDt.Rows[i]["TDA"].ObjToDouble();
                            dRows[j]["IDA"]     = xDt.Rows[i]["IDA"].ObjToDouble();
                            dRows[j]["NDA"]     = xDt.Rows[i]["NDA"].ObjToDouble();
                            dRows[j]["DDA"]     = xDt.Rows[i]["DDA"].ObjToDouble();
                            dRows[j]["transfers"] = xDt.Rows[i]["transfers"].ObjToDouble();
                            dRows[j]["misc"] = xDt.Rows[i]["misc"].ObjToDouble();
                            dRows[j]["manual"] = "Y";
                            break;
                        }
                    }
                }
            }
            catch ( Exception ex)
            {
            }
        }
        /****************************************************************************************/
        private void UpdateBankTotals ( string bankAccount, DateTime monthDate, double credits, double debits, double beginningBalance, double endingbalance )
        {
            int days = DateTime.DaysInMonth(monthDate.Year, monthDate.Month);

            DateTime searchDate = new DateTime(monthDate.Year, monthDate.Month, days);

            string cmd = "Select * from `bank_totals` where `bank_account` = '" + bankAccount + "' AND `date` = '" + searchDate.ToString("yyyy-MM-dd") + "';";
            DataTable dt = G1.get_db_data(cmd);
            string record = "";
            if (dt.Rows.Count <= 0)
                record = G1.create_record("bank_totals", "adate", "-1");
            else
                record = dt.Rows[0]["record"].ObjToString();

            G1.update_db_table("bank_totals", "record", record, new string[] { "bank_account", bankAccount, "date", searchDate.ToString("yyyy-MM-dd"), "adate", "", "credits", credits.ToString(), "debits", debits.ToString(), "beginningBalance", beginningBalance.ToString(), "endingBalance", endingbalance.ToString() });

            cmd = "Select * from `bank_totals` where `bank_account` = '" + bankAccount + "' AND `date` > '" + searchDate.ToString("yyyy-MM-dd") + "';";
            dt = G1.get_db_data(cmd);
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                record = dt.Rows[i]["record"].ObjToString();
                debits = dt.Rows[i]["debits"].ObjToDouble();
                credits = dt.Rows[i]["credits"].ObjToDouble();
                beginningBalance = endingbalance;
                beginningBalance = G1.RoundValue(beginningBalance);
                endingbalance = beginningBalance + credits - debits;
                endingbalance = G1.RoundValue(endingbalance);

                G1.update_db_table("bank_totals", "record", record, new string[] { "beginningBalance", beginningBalance.ToString(), "endingBalance", endingbalance.ToString() });
            }
        }
        /****************************************************************************************/
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            btnSave.Show();

            DataTable dt = (DataTable)dgv.DataSource;
            RecalcTotals(dt);

            dgv.DataSource = dt;
            dgv.Refresh();
        }
        /****************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);

            DateTime date = dr["date"].ObjToDateTime();
            string bankDebit = dr["bankDebit"].ObjToString();
            if ( !String.IsNullOrWhiteSpace ( bankDebit) )
            {
                string cmd = "Select * from `bank_details` WHERE `record` = '" + bankDebit + "';";
                DataTable bankDt = G1.get_db_data(cmd);
                if (bankDt.Rows.Count > 0)
                {
                    bankDt.Columns.Add("depositNumber");
                    bankDt.Rows[0]["depositNumber"] = bankDt.Rows[0]["description"].ObjToString();
                    BankEditDebit debitForm = new BankEditDebit(bankDt);
                    debitForm.Text = "Document Debit for " + date.ToString("MM/dd/yyyy");
                    debitForm.TopMost = true;
                    debitForm.ManualDone += DebitForm_ManualDone;
                    debitForm.ShowDialog();
                    return;
                }
            }

            string what = workReport;

            if ( paymentDetail != null )
            {
                try
                {
                    if (G1.get_column_number(paymentDetail, "search") >= 0)
                        paymentDetail.Columns.Remove("search");

                    paymentDetail.Columns.Add("search");

                    for (int i = 0; i < paymentDetail.Rows.Count; i++)
                        paymentDetail.Rows[i]["search"] = paymentDetail.Rows[i]["payDate8"].ObjToDateTime().ToString("yyyyMMdd");

                    string str = date.ToString("yyyyMMdd");

                    string cmd = "search='" + str + "'";

                    DataRow[] dRows = paymentDetail.Select( cmd );

                    if (oldDt != null)
                    {
                        if (oldDt.Rows.Count > 0)
                        {
                            DateTime oldDate = DateTime.Now;
                            for (int j = 0; j < oldDt.Rows.Count; j++)
                            {
                                oldDate = oldDt.Rows[j]["date"].ObjToDateTime();
                                oldDt.Rows[j]["aDate"] = oldDate.ToString("yyyyMMdd");
                            }
                        }
                    }

                    bool doManual = false;
                    str = date.ToString("yyyyMMdd");
                    cmd = "adate='" + str + "'";

                    DataRow[] tRows = null;
                    if (oldDt != null)
                    {
                        tRows = oldDt.Select(cmd);
                        if (tRows.Length > 0)
                            doManual = true;
                    }

                    string selection = "";
                    bool doDeposits = false;
                    if (dRows.Length > 0)
                        doDeposits = true;
                    if ( doManual && dRows.Length > 0 )
                    {
                        doManual = false;
                        doDeposits = false;
                        string lines = "Manual Edits\nDeposit Details";
                        using (SelectFromList listForm = new SelectFromList(lines, false))
                        {
                            listForm.Text = "Choose only one of these options!";
                            listForm.ShowDialog();
                            selection = SelectFromList.theseSelections;
                            if (String.IsNullOrWhiteSpace(selection))
                                return;
                            if (selection == "Manual Edits")
                                doManual = true;
                            else
                                doDeposits = true;
                        }
                    }
                    if ( doManual )
                    {
                        DataTable dt = tRows.CopyToDataTable();
                        //ViewDataTable viewForm = new ViewDataTable(dt, "adate,misc, returns,transfers,comment");
                        //viewForm.Text = this.Text + " for Manual Edits for " + date.ToString("MM/dd/yyyy");
                        //viewForm.ShowDialog();
                        EditManualBank viewForm = new EditManualBank(dt);
                        viewForm.ManualDone += ViewForm_ManualDone;
                        viewForm.Text = this.Text + " for Manual Edits for " + date.ToString("MM/dd/yyyy");
                        viewForm.ShowDialog();
                        return;
                    }

                    if ( doDeposits )
                    {
                        DataTable dt = dRows.CopyToDataTable();
                        ViewDataTable viewForm = new ViewDataTable(dt, "contractNumber,firstName, lastName,paymentAmount,debitAdjustment,depositNumber,fill1");
                        viewForm.Text = this.Text + " for " + date.ToString("MM/dd/yyyy");
                        viewForm.ShowDialog();
                        return;
                    }
                }
                catch ( Exception ex)
                {
                }
            }

            //WeeklyClose weekForm = new WeeklyClose(date, date, what, "Lock Box Deposits");
            //weekForm.Show();
        }
        /****************************************************************************************/
        private void ViewForm_ManualDone(DataTable dd, DateTime workDate, DataTable originalDt )
        {
            DataTable dt = (DataTable)dgv.DataSource;

            ResetEditRow(dd, workDate, originalDt, "comment");

            ResetEditRow(dd, workDate, originalDt, "returns");
            ResetEditRow(dd, workDate, originalDt, "TDA");
            ResetEditRow(dd, workDate, originalDt, "IDA");
//            ResetEditRow(dd, workDate, originalDt, "PDA");
            ResetEditRow(dd, workDate, originalDt, "NDA");
            ResetEditRow(dd, workDate, originalDt, "DDA");
            ResetEditRow(dd, workDate, originalDt, "misc");
            ResetEditRow(dd, workDate, originalDt, "transfers");

            DataRow[] ddRows = dd.Select("mod='D'");
            if (ddRows.Length <= 0)
                return;

            string record = "";
            string oldRecord = "";

            for (int i = 0; i < ddRows.Length; i++)
            {
                record = ddRows[i]["record"].ObjToString();
                if (!String.IsNullOrWhiteSpace(record))
                {
                    for (int j = 0; j < oldDt.Rows.Count; j++)
                    {
                        oldRecord = oldDt.Rows[j]["record"].ObjToString();
                        if (record == oldRecord)
                        {
                            oldDt.Rows.RemoveAt(j);
                            break;
                        }
                    }
                }
            }
        }
        /****************************************************************************************/
        private void ResetEditRow (DataTable dd, DateTime workDate, DataTable originalDt, string fieldName )
        {
            DataTable dt = (DataTable)dgv.DataSource;

            string record = "";
            string oldRecord = "";

            try
            {
                bool gotDelete = false;
                DataRow[] ddRows = dd.Select("mod='D'");
                if (ddRows.Length > 0)
                    gotDelete = true;

                DataRow [] dRows = dt.Select("adate=" + workDate.ToString("yyyyMMdd"));
                string dataType = dd.Columns[fieldName].DataType.ObjToString();
                if (dRows.Length > 0)
                {
                    if (dataType.ToUpper() == "SYSTEM.STRING")
                    {
                        ResetEditCommentRow(dd, workDate, originalDt, fieldName);
                        gridMain.RefreshEditor(true);
                        return;
                    }
                    DataTable editDt = dRows.CopyToDataTable();
                    double oldReturn = 0D;
                    double newReturn = 0D;
                    double returns = 0D;
                    if (originalDt != null)
                    {
                        for (int i = 0; i < originalDt.Rows.Count; i++)
                            oldReturn += originalDt.Rows[i][fieldName].ObjToDouble();
                    }

                    for (int i = 0; i < dd.Rows.Count; i++)
                    {
                        if (dd.Rows[i]["mod"].ObjToString() != "D")
                        {
                            record = dd.Rows[i]["record"].ObjToString();
                            newReturn += dd.Rows[i][fieldName].ObjToDouble();
                        }
                    }

                    returns = editDt.Rows[0][fieldName].ObjToDouble();
                    returns = returns - oldReturn + newReturn;
                    dRows[0][fieldName] = returns;

                    string str = workDate.ToString("yyyyMMdd");
                    string cmd = "adate='" + str + "'";

                    DataRow[] tRows = oldDt.Select(cmd);
                    if (tRows.Length > 0)
                    {
                        for (int i = 0; i < tRows.Length; i++)
                        {
                            record = tRows[i]["record"].ObjToString();
                            for (int j = 0; j < dd.Rows.Count; j++)
                            {
                                oldRecord = dd.Rows[j]["record"].ObjToString();
                                if (record == oldRecord)
                                {
                                    tRows[i][fieldName] = dd.Rows[j][fieldName].ObjToDouble();
                                    dd.Rows[j][fieldName] = 0D;
                                    break;
                                }
                            }
                        }
                        //if (gotDelete)
                        //{
                        //    for ( int i=0; i<ddRows.Length; i++ )
                        //    {
                        //        record = ddRows[i]["record"].ObjToString();
                        //        if ( !String.IsNullOrWhiteSpace ( record ))
                        //        {
                        //            for ( int j=0; j<oldDt.Rows.Count; j++)
                        //            {
                        //                oldRecord = oldDt.Rows[j]["record"].ObjToString();
                        //                if ( record == oldRecord )
                        //                {
                        //                    oldDt.Rows.RemoveAt(j);
                        //                    break;
                        //                }
                        //            }
                        //        }
                        //    }
                        //}
                    }
                    gridMain.RefreshEditor(true);
                }
            }
            catch ( Exception ex )
            {
            }
        }
        /****************************************************************************************/
        private void ResetEditCommentRow(DataTable dd, DateTime workDate, DataTable originalDt, string fieldName)
        {
            DataTable dt = (DataTable)dgv.DataSource;

            DataRow[] dRows = dt.Select("adate=" + workDate.ToString("yyyyMMdd"));
            string dataType = dd.Columns[fieldName].DataType.ObjToString();
            if (dRows.Length > 0)
            {
                DataTable editDt = dRows.CopyToDataTable();

                string comment = "";
                string newComment = "";
                if (originalDt == null)
                {
                    newComment = dRows[0][fieldName].ObjToString();
                    newComment = newComment.TrimEnd('\n');
                    newComment = newComment.TrimEnd('\r');
                    //if (newComment.IndexOf('\n') >= 0)
                    //    newComment = newComment.Replace("\n", "");
                    //if (newComment.IndexOf('\r') >= 0)
                    //    newComment = newComment.Replace("\r", "");
                }

                for (int i = 0; i < dd.Rows.Count; i++)
                {
                    if (dd.Rows[i]["mod"].ObjToString() == "D")
                        continue;
                    comment = dd.Rows[i][fieldName].ObjToString();
                    comment = comment.TrimEnd('\n');
                    comment = comment.TrimEnd('\r');
                    //if (comment.IndexOf('\n') >= 0)
                    //    comment = comment.Replace("\n", "");
                    //if (comment.IndexOf('\r') >= 0)
                    //    comment = comment.Replace("\r", "");
                    if (!String.IsNullOrWhiteSpace(newComment))
                       newComment += "\n";
                    newComment += comment;
                }
                dRows[0][fieldName] = newComment;

                string str = workDate.ToString("yyyyMMdd");
                string cmd = "adate='" + str + "'";

                string record = "";
                string oldRecord = "";

                DataRow[] tRows = oldDt.Select(cmd);
                if (tRows.Length > 0)
                {
                    for (int i = 0; i < tRows.Length; i++)
                    {
                        record = tRows[i]["record"].ObjToString();
                        for (int j = 0; j < dd.Rows.Count; j++)
                        {
                            oldRecord = dd.Rows[j]["record"].ObjToString();
                            if (record == oldRecord)
                            {
                                if (dd.Rows[j]["mod"].ObjToString() == "D")
                                    tRows[i][fieldName] = "";
                                else
                                {
                                    comment = dd.Rows[j][fieldName].ObjToString();
                                    tRows[i][fieldName] = comment;
                                    dd.Rows[j][fieldName] = "";
                                }
                                break;
                            }
                        }
                    }
                }
                gridMain.RefreshEditor(true);
            }
        }
        /****************************************************************************************/
        private void DebitForm_ManualDone(DataTable dd)
        {
            if (dd == null)
                return;
            if (dd.Rows.Count <= 0)
                return;
            string record = dd.Rows[0]["record"].ObjToString();
            double debit = dd.Rows[0]["debit"].ObjToDouble();
            DateTime date = dd.Rows[0]["date"].ObjToDateTime();
            string bank = dd.Rows[0]["bankAccount"].ObjToString();
            string debitDepNum = dd.Rows[0]["debitDepNum"].ObjToString();
            string depositNumber = dd.Rows[0]["depositNumber"].ObjToString();
            string assignTo = dd.Rows[0]["assignTo"].ObjToString().ToUpper();

            string found = "";
            string bankRecord = "";

            DataTable dt = (DataTable)dgv.DataSource;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                bankRecord = dt.Rows[i]["bankDebit"].ObjToString();
                if (bankRecord == record)
                {
                    G1.update_db_table("bank_details", "record", record, new string[] { "assignTo", assignTo, "description", depositNumber, "debitDepNum", debitDepNum });
                    //dt.Rows[i]["depositNumber"] = depositNumber;
                    //dt.Rows[i]["assignTo"] = assignTo;
                    //dt.Rows[i]["debitDepNum"] = debitDepNum;
                    if (assignTo == "TRUST DOWN PAYMENT")
                        dt.Rows[i]["DDA"] = debit;
                    else if (assignTo == "TRUST DEPOSIT")
                        dt.Rows[i]["TDA"] = debit;
                    else if (assignTo == "INSURANCE DEPOSIT")
                        dt.Rows[i]["IDA"] = debit;
                    else if (assignTo == "FUNERAL DEPOSIT")
                        dt.Rows[i]["NDA"] = debit;
                    else if (assignTo == "TRANSFER")
                        dt.Rows[i]["transfers"] = Math.Abs(debit);
                    else if (assignTo == "RETURN")
                        dt.Rows[i]["returns"] = debit;
                    else if (assignTo == "MISCELLANEOUS")
                        dt.Rows[i]["misc"] = debit;
                    string aDate = date.ToString("yyyyMMdd");
                    DataRow[] dRows = dt.Select("aDate='" + aDate + "'");
                    if ( dRows.Length > 0 )
                    {
                        double dda = 0D;
                        double tda = 0D;
                        double ida = 0D;
                        double nda = 0D;
                        double returns = 0D;
                        double transfers = 0D;
                        double misc = 0D;

                        DataTable bankDt = dRows.CopyToDataTable();
                        for ( int j=0; j<bankDt.Rows.Count; j++)
                        {
                            dda += bankDt.Rows[j]["DDA"].ObjToDouble();
                            tda += bankDt.Rows[j]["TDA"].ObjToDouble();
                            ida += bankDt.Rows[j]["IDA"].ObjToDouble();
                            nda += bankDt.Rows[j]["NDA"].ObjToDouble();
                            returns += bankDt.Rows[j]["returns"].ObjToDouble();
                            transfers += bankDt.Rows[j]["transfers"].ObjToDouble();
                        }
                        dRows = dt.Select("bankDebit='" + bankRecord + "'");
                        if (dRows.Length > 0)
                            dt.Rows.Remove(dRows[0]);
                        dRows = dt.Select("aDate='" + aDate + "'");
                        if ( dRows.Length > 0 )
                        {
                            dRows[0]["DDA"] = dda;
                            dRows[0]["TDA"] = tda;
                            dRows[0]["IDA"] = ida;
                            dRows[0]["NDA"] = nda;
                            dRows[0]["returns"] = returns;
                            dRows[0]["transfers"] = transfers;
                        }
                    }
                    double balance = RecalcTotals(dt);
                    dgv.DataSource = dt;
                    dgv.RefreshDataSource();
                    gridMain.RefreshEditor(true);
                    break;
                }
            }
        }
        /****************************************************************************************/
        private void CombineDetail(DataRow fRow, double debit, string assignTo, string comment = "" )
        {
            double dda = 0D;
            double tda = 0D;
            double ida = 0D;
            double nda = 0D;
            double returns = 0D;
            double transfers = 0D;
            string detail = "";
            try
            {
                dda += fRow["DDA"].ObjToDouble();
                tda += fRow["TDA"].ObjToDouble();
                ida += fRow["IDA"].ObjToDouble();
                nda += fRow["NDA"].ObjToDouble();
                returns += fRow["returns"].ObjToDouble();
                transfers += fRow["transfers"].ObjToDouble();
                detail = fRow["comment"].ObjToString();

                if (assignTo == "TRUST DOWN PAYMENT")
                    dda += debit;
                else if (assignTo == "TRUST DEPOSIT")
                    tda += debit;
                else if (assignTo == "INSURANCE DEPOSIT")
                    ida += debit;
                else if (assignTo == "FUNERAL DEPOSIT")
                    nda += debit;
                else if (assignTo == "TRANSFER")
                    transfers += Math.Abs(debit);
                else if (assignTo == "RETURN")
                    returns += Math.Abs(debit);

                fRow["DDA"] = dda;
                fRow["TDA"] = tda;
                fRow["IDA"] = ida;
                fRow["NDA"] = nda;
                fRow["returns"] = returns;
                fRow["transfers"] = transfers;
                if (!String.IsNullOrWhiteSpace(comment))
                {
                    if (!String.IsNullOrWhiteSpace(detail))
                        detail += "\n" + comment;
                    else
                        detail = comment;
                    fRow["comment"] = detail;
                }
            }
            catch (Exception ex)
            {
            }
        }
        /****************************************************************************************/
        private void CombineData(DataRow fRow, DataRow lRow )
        {
            double dda = 0D;
            double tda = 0D;
            double ida = 0D;
            double nda = 0D;
            double misc = 0D;
            double returns = 0D;
            double transfers = 0D;
            string comment = "";
            string extra = "";
            try
            {
                dda += fRow["DDA"].ObjToDouble();
                tda += fRow["TDA"].ObjToDouble();
                ida += fRow["IDA"].ObjToDouble();
                nda += fRow["NDA"].ObjToDouble();
                misc += fRow["misc"].ObjToDouble();
                returns += fRow["returns"].ObjToDouble();
                transfers += fRow["transfers"].ObjToDouble();
                comment = fRow["comment"].ObjToString();

                dda += lRow["DDA"].ObjToDouble();
                tda += lRow["TDA"].ObjToDouble();
                ida += lRow["IDA"].ObjToDouble();
                nda += lRow["NDA"].ObjToDouble();
                misc += lRow["misc"].ObjToDouble();
                returns += lRow["returns"].ObjToDouble();
                transfers += lRow["transfers"].ObjToDouble();
                extra = lRow["comment"].ObjToString();
                if (!String.IsNullOrWhiteSpace(comment))
                    comment += "\n";
                comment += extra;

                fRow["DDA"] = dda;
                fRow["TDA"] = tda;
                fRow["IDA"] = ida;
                fRow["NDA"] = nda;
                fRow["misc"] = misc;
                fRow["returns"] = returns;
                fRow["transfers"] = transfers;
                fRow["comment"] = comment;
            }
            catch ( Exception ex)
            {
            }
        }
        /****************************************************************************************/
        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            DateTime date = this.dateTimePicker1.Value;
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            date = new DateTime(date.Year, date.Month, days);
            this.dateTimePicker1.Value = date;
        }
        /****************************************************************************************/
        private void pictureAdd_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt.Rows.Count <= 0)
                return;
            DataRow dr = gridMain.GetFocusedDataRow();
            DateTime date = dr["date"].ObjToDateTime();
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            if (row < 0 || row > (dt.Rows.Count - 1))
                return;
            if (rowHandle == (dt.Rows.Count - 1))
                return; // Already at the last row
            try
            {
                if (gridMain.PostEditor())
                    gridMain.UpdateCurrentRow();
                gridMain.RefreshEditor(true);
                gridMain.RefreshData();
                string accountTitle = "";
                if (G1.get_column_number(dt, "accountTitle") >= 0)
                    accountTitle = dr["accountTitle"].ObjToString();
                DataRow dRow = dt.NewRow();
                if (G1.get_column_number(dt, "accountTitle") >= 0)
                    dRow["accountTitle"] = accountTitle;
                dRow["date"] = G1.DTtoMySQLDT(date);
                dRow["DDA"] = 0.0D;
                dRow["TDA"] = 0.0D;
                dRow["IDA"] = 0.0D;
                dRow["NDA"] = 0.0D;
                dRow["returns"] = 0.0D;
                dRow["transfers"] = 0.0D;
                dRow["dailyTotals"] = 0.00D;
                dRow["manual"] = "Y";
                dRow["comment"] = "Enter Comment Here";
                dRow["dow"] = G1.DayOfWeekText(date);
                dRow["bank_account"] = majorBankAccount;
                dt.Rows.InsertAt(dRow, row);
            }
            catch ( Exception ex )
            {
            }
            G1.NumberDataTable(dt);
            dt.AcceptChanges();
            dgv.DataSource = dt;
            gridMain.ClearSelection();
            gridMain.RefreshData();
            gridMain.FocusedRowHandle = rowHandle + 1;
            gridMain.SelectRow(rowHandle + 1);

            btnSave.Show();
            btnSave.Refresh();

            dgv.Refresh();
        }
        /****************************************************************************************/
        private void gridMain_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
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
            else if (e.Column.FieldName.ToUpper().IndexOf("DEPOSITNUMBER") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                string str = e.DisplayText.Trim().ToUpper();
                if (str.IndexOf("Z") == 0)
                    e.DisplayText = "";
            }
            else if (e.Column.FieldName.ToUpper().IndexOf("COMMENT") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                //string str = e.DisplayText;
                //if ( str.IndexOf ( "\n") >= 0 )
                //{
                //    e.DisplayText = str.Replace("\n", "\r\n");
                //}
            }
            else
            {
                bool doit = false;
                if (e.Column.FieldName.ToUpper().IndexOf("TDA") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
                    doit = true;
                else if (e.Column.FieldName.ToUpper().IndexOf("DDA") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
                    doit = true;
                else if (e.Column.FieldName.ToUpper().IndexOf("IDA") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
                    doit = true;
                else if (e.Column.FieldName.ToUpper().IndexOf("NDA") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
                    doit = true;
                else if (e.Column.FieldName.ToUpper().IndexOf("TRANSFERS") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
                    doit = true;
                else if (e.Column.FieldName.ToUpper().IndexOf("RETURNS") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
                    doit = true;
                else if (e.Column.FieldName.ToUpper().IndexOf("MISC") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
                    doit = true;
                else if (e.Column.FieldName.ToUpper().IndexOf("DAILYTOTALS") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
                    doit = true;
                if (doit)
                {
                    string str = e.DisplayText;
                    str = str.Replace(",", "");
                    double dValue = str.ObjToDouble();
                    if (dValue == 0D)
                        e.DisplayText = "";
                }
            }
        }
        /****************************************************************************************/
        private double getBeginningBalance(string bankAccount, DateTime monthDate, ref double endingBalance, ref string bankDescription )
        {
            double beginningBalance = 0D;
            endingBalance = 0D;
            bankDescription = "";

            int days = DateTime.DaysInMonth(monthDate.Year, monthDate.Month);

            DateTime searchDate = new DateTime(monthDate.Year, monthDate.Month, days);

            string[] Lines = bankAccount.Split('~');

            if (Lines.Length < 3)
                return beginningBalance;

            string location = Lines[0];
            string generalLedger = Lines[1];
            string account = Lines[2];

            string cmd = "";

            if ( workReport != "Funeral Detail Report" )
            {
                cmd = "Select * from `bank_accounts` where `localDescription` = '" + location + "' AND `general_ledger_no` = '" + generalLedger + "' AND `account_no` = '" + account + "';";
            }
            else
            {
                cmd = "Select * from `bank_accounts` where `location` = '" + location + "' AND `general_ledger_no` = '" + generalLedger + "' AND `account_no` = '" + account + "';";
            }

            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return beginningBalance;

            beginningBalance = dt.Rows[0]["beginningBalance"].ObjToDouble();
            DateTime asOf = dt.Rows[0]["asOfDate"].ObjToDateTime();
            bankDescription = dt.Rows[0]["account_title"].ObjToString();

            if (asOf.Year < 10)
                asOf = monthDate.AddMonths ( -1 );

            cmd = "Select * from `bank_totals` where `bank_account` = '" + bankAccount + "' AND `date` = '" + searchDate.ToString("yyyy-MM-dd") + "';";
            dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
                endingBalance = dt.Rows[0]["endingBalance"].ObjToDouble();

            for (; ; )
            {
                searchDate = searchDate.AddMonths(-1);
                days = DateTime.DaysInMonth(searchDate.Year, searchDate.Month);

                searchDate = new DateTime(searchDate.Year, searchDate.Month, days);
                if (searchDate < asOf)
                    break;
                cmd = "Select * from `bank_totals` where `bank_account` = '" + bankAccount + "' AND `date` = '" + searchDate.ToString("yyyy-MM-dd") + "';";
                dt = G1.get_db_data(cmd);
                if (dt.Rows.Count > 0)
                {
                    beginningBalance = dt.Rows[0]["endingBalance"].ObjToDouble();
                    break;
                }
            }
            return beginningBalance;
        }
        /****************************************************************************************/
        private void gridMain_CustomSummaryCalculate(object sender, DevExpress.Data.CustomSummaryEventArgs e)
        {
            double balance = 0D;
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt.Rows.Count > 0)
            {
                int lastRow = dt.Rows.Count - 1;
                balance = dt.Rows[lastRow]["balance"].ObjToDouble();
            }
            string str = G1.ReformatMoney(balance);
            e.TotalValue = str;
        }
        /****************************************************************************************/
        private void dateTimePicker1_CloseUp(object sender, EventArgs e)
        {
            DateTime date = this.dateTimePicker1.Value;
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            date = new DateTime(date.Year, date.Month, days);
            this.dateTimePicker1.Value = date;

            if (!CheckForSave())
                return;

            btnGetDeposits_Click(null, null);
        }
        /****************************************************************************************/
        private void gridMain_RowCellStyle(object sender, RowCellStyleEventArgs e)
        {

            GridView View = sender as GridView;
            if (e.RowHandle >= 0)
            {
                string column = e.Column.FieldName.ToUpper();
                if (column == "NUM")
                {
                    DataTable dt = (DataTable)dgv.DataSource;
                    int row = gridMain.GetDataSourceRowIndex(e.RowHandle);

                    string comment = dt.Rows[row]["comment"].ObjToString();
                    if (comment.Trim().ToUpper() != "BALANCE FORWARD")
                    {
                        string adate = dt.Rows[row]["adate"].ObjToString();

                        //if (String.IsNullOrWhiteSpace(adate) && workReport != "Funeral Detail Report")
                        //{
                        //    e.Appearance.BackColor = Color.Red;
                        //}
                        if (String.IsNullOrWhiteSpace(adate))
                        {
                            e.Appearance.BackColor = Color.Red;
                        }
                    }
                }
            }
        }
        /****************************************************************************************/
        private void pictureDelete_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt.Rows.Count <= 0)
                return;
            DataRow dr = gridMain.GetFocusedDataRow();
            DateTime date = dr["date"].ObjToDateTime();
            string aDate = dr["aDate"].ObjToString();
            if (!String.IsNullOrWhiteSpace(aDate))
                return;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            if (row < 0 || row > (dt.Rows.Count - 1))
                return;
            dt.Rows.RemoveAt(row);

            double balance = RecalcTotals(dt);

            G1.NumberDataTable(dt);
            dt.AcceptChanges();
            dgv.DataSource = dt;
            dgv.Refresh();

            btnSave.Show();
            btnSave.Refresh();
        }
        /****************************************************************************************/
        private double originalSize = 0D;
        private Font mainFont = null;
        private Font newFont = null;
        private void ScaleCells()
        {
            if (originalSize == 0D)
            {
                //                originalSize = gridMain.Columns["address1"].AppearanceCell.FontSizeDelta.ObjToDouble();
                originalSize = gridMain.Columns["comment"].AppearanceCell.Font.Size;
                mainFont = gridMain.Columns["comment"].AppearanceCell.Font;
            }
            double scale = txtScale.Text.ObjToDouble();
            double size = scale / 100D * originalSize;
            Font font = new Font(mainFont.Name, (float)size);
            for (int i = 0; i < gridMain.Columns.Count; i++)
            {
                gridMain.Columns[i].AppearanceCell.Font = font;
                gridMain.Columns[i].AppearanceHeader.Font = font;
            }
            gridMain.Appearance.GroupFooter.Font = font;
            gridMain.AppearancePrint.FooterPanel.Font = font;
            newFont = font;
            gridMain.RefreshData();
            gridMain.RefreshEditor(true);
            dgv.Refresh();
            this.Refresh();
        }
        /****************************************************************************************/
        private void txtScale_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string balance = txtScale.Text.Trim();
                if (!G1.validate_numeric(balance))
                {
                    MessageBox.Show("***ERROR*** Scale must be numeric!");
                    return;
                }
                double money = balance.ObjToDouble();
                balance = G1.ReformatMoney(money);
                txtScale.Text = balance;
                ScaleCells();
                return;
            }
            // Initialize the flag to false.
            bool nonNumberEntered = false;

            // Determine whether the keystroke is a number from the top of the keyboard.
            if (e.KeyCode < Keys.D0 || e.KeyCode > Keys.D9)
            {
                // Determine whether the keystroke is a number from the keypad.
                if (e.KeyCode < Keys.NumPad0 || e.KeyCode > Keys.NumPad9)
                {
                    // Determine whether the keystroke is a backspace.
                    if (e.KeyCode != Keys.Back)
                    {
                        // A non-numerical keystroke was pressed.
                        // Set the flag to true and evaluate in KeyPress event.
                        if (e.KeyCode != Keys.OemPeriod)
                            nonNumberEntered = true;
                    }
                }
            }
            //If shift key was pressed, it's not a number.
            if (Control.ModifierKeys == Keys.Shift)
            {
                nonNumberEntered = true;
            }
            if (nonNumberEntered)
            {
                MessageBox.Show("***ERROR*** Key entered must be a number!");
                e.Handled = true;
            }
        }
        /****************************************************************************************/
        private void gridMain_CalcRowHeightZ(object sender, RowHeightEventArgs e)
        {
            GridView View = sender as GridView;
            if (e.RowHandle >= 0)
            {
                int maxHeight = 0;
                int newHeight = 0;
                bool doit = false;
                string name = "";
                foreach (GridColumn column in gridMain.Columns)
                {
                    name = column.FieldName.ToUpper();
                    if (name == "COMMENT" )
                        doit = true;
                    if (doit)
                    {
                        using (RepositoryItemMemoEdit edit = new RepositoryItemMemoEdit())
                        {
                            using (MemoEditViewInfo viewInfo = edit.CreateViewInfo() as MemoEditViewInfo)
                            {
                                viewInfo.EditValue = gridMain.GetRowCellValue(e.RowHandle, column.FieldName);
                                viewInfo.Bounds = new Rectangle(0, 0, column.VisibleWidth, dgv.Height);
                                using (Graphics graphics = dgv.CreateGraphics())
                                using (GraphicsCache cache = new GraphicsCache(graphics))
                                {
                                    viewInfo.CalcViewInfo(graphics);
                                    var height = ((IHeightAdaptable)viewInfo).CalcHeight(cache, column.VisibleWidth);
                                    newHeight = Math.Max(height, maxHeight);
                                    if (newHeight > maxHeight)
                                        maxHeight = newHeight;
                                }
                            }
                        }
                    }
                }

                if (maxHeight > 0)
                    e.RowHeight = maxHeight;
            }
        }
        /***********************************************************************************************/
        private void gridMain_CalcRowHeight(object sender, RowHeightEventArgs e)
        {
            GridView View = sender as GridView;
            if (e.RowHandle >= 0)
            {
                string cancelled = View.GetRowCellDisplayText(e.RowHandle, View.Columns["comment"]);
                if (!String.IsNullOrWhiteSpace(cancelled))
                {
                    int originalRowHeight = e.RowHeight;
                    cancelled = cancelled.TrimEnd('\n');
                    string[] Lines = cancelled.Split('\n');
                    int count = Lines.Length;
                    if (count > 1)
                        e.RowHeight = originalRowHeight * count;
                }
            }
        }
        /****************************************************************************************/
        private string oldWhat = "";
        /****************************************************************************************/
        private void gridMain_ValidatingEditor(object sender, DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventArgs e)
        {
            GridView view = sender as GridView;
            DataRow dr = gridMain.GetFocusedDataRow();
            string manual = dr["manual"].ObjToString().ToUpper();
            if (manual != "Y")
            {
                e.Valid = false;
                return;
            }
            if (view.FocusedColumn.FieldName.ToUpper() == "DATE")
            {
                DataTable dt = (DataTable)dgv.DataSource;
                int rowhandle = gridMain.FocusedRowHandle;
                int row = gridMain.GetDataSourceRowIndex(rowhandle);
                //string manual = dr["manual"].ObjToString().ToUpper();
                //if (manual != "Y")
                //{
                //    e.Valid = false;
                //    return;
                //}
                oldWhat = e.Value.ObjToString();
                DateTime date = oldWhat.ObjToDateTime();
                dt.Rows[row]["date"] = G1.DTtoMySQLDT(date);
                e.Value = G1.DTtoMySQLDT(date);
            }
            else if (view.FocusedColumn.FieldName.ToUpper() == "TRANSFERS")
            {
                DataTable dt = (DataTable)dgv.DataSource;
                //DataRow dr = gridMain.GetFocusedDataRow();
                int rowhandle = gridMain.FocusedRowHandle;
                int row = gridMain.GetDataSourceRowIndex(rowhandle);
                string bankAccount = dr["bank_account"].ObjToString().ToUpper();
            }

            string ba = dr["bank_account"].ObjToString();
            if (!String.IsNullOrWhiteSpace(ba))
            {
                DataTable dx = (DataTable)dgv.DataSource;
                //RecalcBankAccount(dx, ba );
                //RecalcDailyTotals(dx, beginningBalance);
                //dgv.DataSource = dx;
                //dgv.Refresh();
            }
        }
        /****************************************************************************************/
        private void btnDraftReport_Click(object sender, EventArgs e)
        {
            if (paymentDetail == null)
                return;
            if (paymentDetail.Rows.Count <= 0)
                return;

            if (workReport == "Cover Report")
            {
                this.Cursor = Cursors.WaitCursor;
                doCoverTotals();
                this.Cursor = Cursors.Default;
                return;
            }

            DataTable backupDt = paymentDetail.Copy();

            this.Cursor = Cursors.WaitCursor;

            DataTable dt = (DataTable)dgv.DataSource;
            DataTable reportDt = (DataTable)chkComboLocation.Properties.DataSource;
            string location = "";
            string oldloc = "";
            string contractNumber = "";
            string payer = "";
            string cmd = "";
            string aDate = "";
            DateTime date = DateTime.Now;
            DataTable dx = null;
            DataTable rDt = new DataTable();
            rDt.Columns.Add("Num");
            rDt.Columns.Add("Date");
            rDt.Columns.Add("aDate");
            DateTime startDate = this.dateTimePicker1.Value;
            startDate = new DateTime(startDate.Year, startDate.Month, 1);
            int days = DateTime.DaysInMonth(startDate.Year, startDate.Month);
            DataRow dRow = null;
            DataRow[] dRows = null;
            for ( int i=0; i<days; i++)
            {
                dRow = rDt.NewRow();
                dRow["aDate"] = startDate.AddDays(i).ToString("yyyyMMdd");
                dRow["Date"] = startDate.AddDays(i).ToString("MM/dd/yy");
                rDt.Rows.Add(dRow);
            }

            int col = 0;
            int row = 0;
            double payment = 0D;
            double debit = 0D;
            double downPayment = 0D;
            double dValue = 0D;

            bool gotInsurance = false;

            for (int i = 0; i < paymentDetail.Rows.Count; i++)
            {
                try
                {
                    date = paymentDetail.Rows[i]["payDate8"].ObjToDateTime();
                    aDate = date.ToString("yyyyMMdd");
                    dRows = rDt.Select("aDate='" + aDate + "'");
                    if (dRows.Length <= 0)
                        continue;

                    contractNumber = paymentDetail.Rows[i]["contractNumber"].ObjToString();
                    if ( contractNumber == "M19098LI")
                    {
                    }
                    if (contractNumber.IndexOf("ZZ") == 0)
                    {
                        location = paymentDetail.Rows[i]["location"].ObjToString();
                        //if (G1.validate_numeric(location))
                        //{
                        //    cmd = "Select * from `icontracts` c JOIN `icustomers` b ON c.`contractNumber` = b.`contractNumber` WHERE c.`contractNumber` = '" + contractNumber + "';";
                        //    dx = G1.get_db_data(cmd);
                        //    if (dx.Rows.Count > 0)
                        //    {
                        //        payer = dx.Rows[0]["payer"].ObjToString();
                        //        location = ImportDailyDeposits.FindLastPaymentLocation(payer, ref oldloc);
                        //    }
                        //}
                        location = DetermineLocation(contractNumber, location, "Ins");
                        col = CheckForColumn(rDt, location, "Ins");
                        if (col <= 0)
                            continue;
                        dValue = dRows[0][col].ObjToDouble();
                        payment = paymentDetail.Rows[i]["paymentAmount"].ObjToDouble();
                        debit = paymentDetail.Rows[i]["debitAdjustment"].ObjToDouble();
                        //if (payment == 0D && debit != 0D)
                        //    payment = Math.Abs(debit) * -1D;
                        dValue += payment;
                        dRows[0][col] = dValue;
                        gotInsurance = true;
                    }
                    else
                    {
                        if (gotInsurance)
                            break;
                        location = paymentDetail.Rows[i]["location"].ObjToString();
                        location = DetermineLocation(contractNumber, location, "Trust");
                        col = CheckForColumn(rDt, location, "Trust");
                        if (col <= 0)
                            continue;
                        dValue = dRows[0][col].ObjToDouble();
                        payment = paymentDetail.Rows[i]["paymentAmount"].ObjToDouble();
                        debit = paymentDetail.Rows[i]["debitAdjustment"].ObjToDouble();
                        downPayment = paymentDetail.Rows[i]["downPayment"].ObjToDouble();
                        //if (payment == 0D && debit != 0D)
                        //    payment = Math.Abs(debit) * -1D;
                        //else if (payment == 0D && downPayment != 0D)
                        //    payment = downPayment;
                        dValue += payment;
                        dRows[0][col] = dValue;
                    }
                }
                catch ( Exception ex )
                {
                }
            }

            int lastRow = rDt.Rows.Count;

            dRow = rDt.NewRow();
            rDt.Rows.Add(dRow);

            double total = 0D;
            double columnTotals = 0D;
            try
            {
                dRow = rDt.NewRow();
                for (int i = 3; i < rDt.Columns.Count; i++)
                {
                    total = 0D;
                    for (int j = 0; j < lastRow; j++)
                    {
                        payment = rDt.Rows[j][i].ObjToDouble();
                        total += payment;
                    }
                    dRow[i] = total;
                    columnTotals += total;
                }
                rDt.Rows.Add(dRow);
            }
            catch (Exception ex)
            {
            }

            int lastCol = rDt.Columns.Count;
            double rowTotals = 0D;
            double transfers = 0D;
            double dailyTotal = 0D;

            try
            {
                rDt.Columns.Add("Total", Type.GetType("System.Double"));
                rDt.Columns.Add("Transfers", Type.GetType("System.Double"));
                rDt.Columns.Add("Daily Total", Type.GetType("System.Double"));
                col = G1.get_column_number(rDt, "Total");
                for (int i = 0; i < lastRow; i++)
                {
                    total = 0D;
                    for (int j = 3; j < lastCol; j++)
                    {
                        payment = rDt.Rows[i][j].ObjToDouble();
                        total += payment;
                    }
                    rDt.Rows[i][col] = total;
                    rowTotals += total;

                    rDt.Rows[i]["Num"] = (i + 1).ToString();
                    transfers = dt.Rows[i + 1]["transfers"].ObjToDouble();
                    rDt.Rows[i]["Transfers"] = transfers;

                    dailyTotal = total - transfers;
                    dailyTotal = G1.RoundValue(dailyTotal);
                    rDt.Rows[i]["Daily Total"] = dailyTotal;
                }
            }
            catch (Exception ex)
            {
            }

            try
            {
                columnTotals = G1.RoundValue(columnTotals);
                rowTotals = G1.RoundValue(rowTotals);
                if (columnTotals != rowTotals)
                {
                }

                lastRow = rDt.Rows.Count - 1;
                rDt.Rows[lastRow]["Total"] = rowTotals;
                rDt.Rows[lastRow]["Daily Total"] = columnTotals;
            }
            catch (Exception ex)
            {
            }

            col = G1.get_column_number(rDt, "aDate");
            rDt.Columns.RemoveAt(col);

            if (chkUseCombos.Checked)
            {
                DataTable mDt = CombineLocations(rDt, workReport);

                BankDetailsByLocation bankForm = new BankDetailsByLocation(mDt, this.Text, paymentDetail, oldDt, this.dateTimePicker1.Value, workReport, chkUseCombos.Checked );
                bankForm.Show();
            }
            else
            {
                BankDetailsByLocation bankForm = new BankDetailsByLocation(rDt, this.Text, paymentDetail, oldDt, this.dateTimePicker1.Value, workReport, chkUseCombos.Checked );
                bankForm.Show();
            }

            paymentDetail = backupDt.Copy();

            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private DataTable CombineLocations ( DataTable xDt, string workReport )
        {
            if (workReport != "ACH Detail Report" && workReport != "Cover Report" )
                return xDt;

            string cmd = "Select * from `ach_combinations` ORDER by `order`;";
            if ( workReport == "Cover Report")
                cmd = "Select * from `ach_draft_combinations` ORDER by `order`;";

            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return xDt;


            DataTable rDt = xDt.Copy();

            DataTable dx = new DataTable();
            DataRow dRow = null;

            dx.Columns.Add("Num");
            dx.Columns.Add("Date");

            DateTime date = DateTime.Now;

            bool gotColumn = false;
            string heading = "";
            string location = "";

            double value1 = 0D;
            double value2 = 0D;
            int count = 0;
            int col = 0;

            try
            {
                for (int i = 0; i < rDt.Rows.Count; i++)
                {
                    dRow = dx.NewRow();
                    dx.Rows.Add();
                }

                //G1.copy_dt_column(rDt, "num", dx, "num");
                //G1.copy_dt_column(rDt, "record", dx, "record");
                G1.copy_dt_column(rDt, "date", dx, "Date");
                //G1.copy_dt_column(rDt, "dow", dx, "dow");

                for ( int i=0; i<dt.Rows.Count; i++)
                {
                    heading = dt.Rows[i]["heading"].ObjToString().Trim();
                    location = dt.Rows[i]["location"].ObjToString().Trim();
                    if (String.IsNullOrWhiteSpace(heading))
                        heading = location;

                    col = G1.get_column_number(rDt, location);
                    if ( col < 0 )
                    {
                        dx.Columns.Add(heading, Type.GetType("System.Double"));
                        continue;
                    }

                    if (G1.get_column_number(dx, heading) < 0)
                    {
                        dx.Columns.Add(heading, Type.GetType("System.Double"));
                        G1.copy_dt_column(rDt, location, dx, heading);
                    }
                    else
                    {
                        for ( int j=0; j<rDt.Rows.Count; j++)
                        {
                            value1 = dx.Rows[j][heading].ObjToDouble();
                            value2 = rDt.Rows[j][location].ObjToDouble();
                            dx.Rows[j][heading] = value1 + value2;
                        }
                    }
                }

                int lastRow = dx.Rows.Count - 1;
                dx.Rows.RemoveAt(lastRow);
                lastRow = dx.Rows.Count - 1;
                dx.Rows.RemoveAt(lastRow);

                if ( G1.get_column_number ( dx, "DRAFTS") > 0 )
                {
                    dx.Columns.Add("aDate");
                    for (int j = 0; j < dx.Rows.Count; j++)
                        dx.Rows[j]["aDate"] = dx.Rows[j]["date"].ObjToDateTime().ToString("yyyyMMdd");
                    DateTime now = this.dateTimePicker1.Value;
                    now = new DateTime(now.Year, now.Month, 1);
                    //this.dateTimePicker1.Value = now;
                    int days = DateTime.DaysInMonth(now.Year, now.Month);
                    //this.dateTimePicker1.Value = new DateTime(now.Year, now.Month, days);

                    DateTime startDate = now;
                    DateTime stopDate = this.dateTimePicker1.Value;
                    DataTable ddd = LoadMain(startDate, stopDate);
                    ddd = getTrustPayments(ddd, startDate, stopDate);
                    ddd = getInsurancePayments(ddd, startDate, stopDate);
                    double dValue = 0D;
                    string aDate = "";
                    DataRow[] dRows = null;
                    for (int j = 0; j < ddd.Rows.Count; j++)
                    {
                        try
                        {
                            dValue = ddd.Rows[j]["TDA"].ObjToDouble() + ddd.Rows[j]["IDA"].ObjToDouble();
                            date = ddd.Rows[j]["date"].ObjToDateTime();
                            aDate = date.ToString("yyyyMMdd");
                            dRows = dx.Select("aDate='" + aDate + "'");
                            if (dRows.Length > 0)
                                dRows[0]["DRAFTS"] = dValue;
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                    col = G1.get_column_number(dx, "aDate");
                    dx.Columns.RemoveAt(col);
                }


                dx = TotalUpTable(dx, rDt);

                G1.NumberDataTable(dx);
            }
            catch ( Exception ex )
            {
            }

            return dx;
        }
        /****************************************************************************************/
        private DataTable TotalUpTable ( DataTable dx, DataTable rDt )
        {
            int lastRow = dx.Rows.Count;
            DateTime date = DateTime.Now;

            for ( int i=0; i<dx.Rows.Count; i++ )
            {
                date = dx.Rows[i]["date"].ObjToDateTime();
                if ( date.Year < 100 )
                {
                    lastRow = i;
                    break;
                }
            }

            DataRow dRow = dx.NewRow();
            dx.Rows.Add(dRow);

            int firstCol = G1.get_column_number(dx, "Date") + 1;

            double total = 0D;
            double columnTotals = 0D;
            double payment = 0D;
            try
            {
                dRow = dx.NewRow();
                for (int i = firstCol; i < dx.Columns.Count; i++)
                {
                    total = 0D;
                    for (int j = 0; j < lastRow; j++)
                    {
                        payment = dx.Rows[j][i].ObjToDouble();
                        total += payment;
                    }
                    dRow[i] = total;
                    columnTotals += total;
                }
                dx.Rows.Add(dRow);
            }
            catch (Exception ex)
            {
            }

            int lastCol = dx.Columns.Count;
            double rowTotals = 0D;
            double transfers = 0D;
            double dailyTotal = 0D;

            try
            {
                dx.Columns.Add("Total", Type.GetType("System.Double"));
                dx.Columns.Add("Transfers", Type.GetType("System.Double"));
                dx.Columns.Add("Daily Total", Type.GetType("System.Double"));
                int col = G1.get_column_number(dx, "Total");
                for (int i = 0; i < lastRow; i++)
                {
                    total = 0D;
                    for (int j = firstCol; j < lastCol; j++)
                    {
                        payment = dx.Rows[i][j].ObjToDouble();
                        total += payment;
                    }
                    dx.Rows[i][col] = total;
                    rowTotals += total;

                    dx.Rows[i]["Num"] = (i + 1).ToString();
                    transfers = rDt.Rows[i + 1]["transfers"].ObjToDouble();
                    dx.Rows[i]["Transfers"] = transfers;

                    dailyTotal = total - transfers;
                    dailyTotal = G1.RoundValue(dailyTotal);
                    dx.Rows[i]["Daily Total"] = dailyTotal;
                }
            }
            catch (Exception ex)
            {
            }

            try
            {
                columnTotals = G1.RoundValue(columnTotals);
                rowTotals = G1.RoundValue(rowTotals);
                if (columnTotals != rowTotals)
                {
                }

                lastRow = dx.Rows.Count - 1;
                dx.Rows[lastRow]["Total"] = rowTotals;
                dx.Rows[lastRow]["Daily Total"] = columnTotals;
            }
            catch (Exception ex)
            {
            }
            return dx;
        }
        /****************************************************************************************/
        private DataTable funDt = null;
        private int CheckForColumn ( DataTable rDt, string location, string type = "" )
        {
            int col = -1;
            DataRow[] dRows = null;
            try
            {
                location = CleanupLocation(location);
                if (String.IsNullOrWhiteSpace(location))
                    location = "BLANK";
                if ( !String.IsNullOrWhiteSpace ( type ))
                    location += " " + type;

                col = G1.get_column_number(rDt, location);
                if (col > 0)
                    return col;
                rDt.Columns.Add(location, Type.GetType("System.Double"));
                col = G1.get_column_number(rDt, location);
            }
            catch ( Exception ex )
            {
            }
            return col;
        }
        /****************************************************************************************/
        public static string CleanupLocation ( string location )
        {
            location = location.Replace("Colonial-", "");
            location = location.Replace("Colonial", "");
            location = location.Replace("Chapel", "");
            location = location.Replace("Funeral", "");
            location = location.Replace("Home", "");
            location = location.Trim();
            return location;
        }
        /****************************************************************************************/
        private void gridMain_MouseDown(object sender, MouseEventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource; // Leave as a GetDate example

            var hitInfo = gridMain.CalcHitInfo(e.Location);
            if (hitInfo.InRowCell)
            {
            }
            else
            {
            }
        }
        /****************************************************************************************/
        private void doCoverTotals ()
        {
            if (paymentDetail == null)
                return;
            if (paymentDetail.Rows.Count <= 0)
                return;

            DataTable backupDt = paymentDetail.Copy();

            DataTable dt = (DataTable)dgv.DataSource;
            DataTable reportDt = (DataTable)chkComboLocation.Properties.DataSource;
            string location = "";
            string oldloc = "";
            string contractNumber = "";
            string payer = "";
            string cmd = "";
            string aDate = "";
            DateTime date = DateTime.Now;

            DataTable dx = null;
            DataTable rDt = new DataTable();
            rDt.Columns.Add("Num");
            rDt.Columns.Add("Date");
            rDt.Columns.Add("aDate");
            rDt.Columns.Add("What");
            DateTime startDate = this.dateTimePicker1.Value;
            int days = DateTime.DaysInMonth(startDate.Year, startDate.Month);
            startDate = new DateTime(startDate.Year, startDate.Month, 1);
            DataRow dRow = null;
            DataRow[] dRows = null;
            for (int i = 0; i < days; i++)
            {
                dRow = rDt.NewRow();
                dRow["aDate"] = startDate.AddDays(i).ToString("yyyyMMdd");
                dRow["Date"] = startDate.AddDays(i).ToString("MM/dd/yyyy");
                rDt.Rows.Add(dRow);
            }

            int col = 0;
            int row = 0;
            double payment = 0D;
            double debit = 0D;
            double downPayment = 0D;
            double dValue = 0D;
            string what = "";

            bool gotInsurance = false;

            for (int i = 0; i < paymentDetail.Rows.Count; i++)
            {
                try
                {
                    date = paymentDetail.Rows[i]["payDate8"].ObjToDateTime();
                    aDate = date.ToString("yyyyMMdd");
                    dRows = rDt.Select("aDate='" + aDate + "'");
                    if (dRows.Length <= 0)
                        continue;

                    contractNumber = paymentDetail.Rows[i]["contractNumber"].ObjToString();
                    if (contractNumber.IndexOf("ZZ") == 0)
                    {
                        if ( 1 == 1 )
                        {
                            gotInsurance = true;
                            continue;
                        }
                        location = paymentDetail.Rows[i]["location"].ObjToString();
                        location = DetermineLocation(contractNumber, location, "Ins");
                        col = CheckForColumn(rDt, location );
                        if (col <= 0)
                            continue;
                        dValue = dRows[0][col].ObjToDouble();
                        payment = paymentDetail.Rows[i]["paymentAmount"].ObjToDouble();
                        debit = paymentDetail.Rows[i]["debitAdjustment"].ObjToDouble();
                        if (payment == 0D && debit != 0D)
                            payment = Math.Abs(debit) * -1D;
                        dValue += payment;
                        dRows[0][col] = dValue;
                        dRows[0]["what"] = "Ins";
                        gotInsurance = true;
                    }
                    else
                    {
                        location = paymentDetail.Rows[i]["location"].ObjToString();
                        if (gotInsurance)
                        {
                            location = DetermineLocation(contractNumber, location, "Funeral");
                            dRows[0]["what"] = "Funeral";
                        }
                        else
                        {
                            if (1 == 1)
                                continue;
                            location = DetermineLocation(contractNumber, location, "Trust");
                            dRows[0]["what"] = "Trust";
                        }
                        col = CheckForColumn(rDt, location );
                        if (col <= 0)
                            continue;
                        dValue = dRows[0][col].ObjToDouble();
                        payment = paymentDetail.Rows[i]["paymentAmount"].ObjToDouble();
                        debit = paymentDetail.Rows[i]["debitAdjustment"].ObjToDouble();
                        downPayment = paymentDetail.Rows[i]["downPayment"].ObjToDouble();
                        if (payment == 0D && debit != 0D)
                            payment = Math.Abs(debit) * -1D;
                        //else if (payment == 0D && downPayment != 0D)
                        //    payment = downPayment;
                        dValue += payment;
                        dRows[0][col] = dValue;
                    }
                }
                catch (Exception ex)
                {
                }
            }

            int lastRow = rDt.Rows.Count;

            dRow = rDt.NewRow();
            rDt.Rows.Add(dRow);

            double total = 0D;
            double columnTotals = 0D;
            try
            {
                dRow = rDt.NewRow();
                for (int i = 3; i < rDt.Columns.Count; i++)
                {
                    total = 0D;
                    for (int j = 0; j < lastRow; j++)
                    {
                        payment = rDt.Rows[j][i].ObjToDouble();
                        total += payment;
                    }
                    dRow[i] = total;
                    columnTotals += total;
                }
                rDt.Rows.Add(dRow);
            }
            catch (Exception ex)
            {
            }

            int lastCol = rDt.Columns.Count;
            double rowTotals = 0D;
            double transfers = 0D;
            double dailyTotal = 0D;

            try
            {
                rDt.Columns.Add("Total", Type.GetType("System.Double"));
                rDt.Columns.Add("Transfers", Type.GetType("System.Double"));
                rDt.Columns.Add("Daily Total", Type.GetType("System.Double"));
                col = G1.get_column_number(rDt, "Total");
                for (int i = 0; i < lastRow; i++)
                {
                    total = 0D;
                    for (int j = 3; j < lastCol; j++)
                    {
                        payment = rDt.Rows[i][j].ObjToDouble();
                        total += payment;
                    }
                    rDt.Rows[i][col] = total;
                    rowTotals += total;

                    transfers = dt.Rows[i+1]["transfers"].ObjToDouble();
                    rDt.Rows[i]["Transfers"] = transfers;

                    dailyTotal = total - transfers;
                    dailyTotal = G1.RoundValue(dailyTotal);
                    rDt.Rows[i]["Daily Total"] = dailyTotal;

                    rDt.Rows[i]["Num"] = (i + 1).ToString();
                }
            }
            catch (Exception ex)
            {
            }

            try
            {
                columnTotals = G1.RoundValue(columnTotals);
                rowTotals = G1.RoundValue(rowTotals);
                if (columnTotals != rowTotals)
                {
                }

                lastRow = rDt.Rows.Count - 1;
                rDt.Rows[lastRow]["Total"] = rowTotals;
                rDt.Rows[lastRow]["Daily Total"] = columnTotals;
            }
            catch (Exception ex)
            {
            }

            col = G1.get_column_number(rDt, "aDate");
            rDt.Columns.RemoveAt(col);
            col = G1.get_column_number(rDt, "What");
            rDt.Columns.RemoveAt(col);

            if (chkUseCombos.Checked)
            {
                DataTable mDt = CombineLocations(rDt, workReport);

                BankDetailsByLocation bankForm = new BankDetailsByLocation(mDt, this.Text, paymentDetail, oldDt, this.dateTimePicker1.Value, workReport, chkUseCombos.Checked);
                bankForm.Show();
            }
            else
            {
                BankDetailsByLocation bankForm = new BankDetailsByLocation(rDt, this.Text, paymentDetail, oldDt, this.dateTimePicker1.Value, workReport, chkUseCombos.Checked);
                bankForm.Show();
            }


            //BankDetailsByLocation bankForm = new BankDetailsByLocation(rDt, this.Text, paymentDetail, oldDt, this.dateTimePicker1.Value, workReport, false );
            //bankForm.Show();

            paymentDetail = backupDt.Copy();
        }
        /****************************************************************************************/
        private string DetermineLocation(string contractNumber, string location, string type )
        {
            string cmd = "";
            string payer = "";
            DataTable dx = null;
            string oldloc = "";
            string wordLocation = "";

            string[] Lines = null;

            location = CleanupLocation(location);

            DataRow[] dRows = null;

            if (funDt == null)
                funDt = G1.get_db_data("Select * from `funeralHomes`;");

            try
            {
                if (contractNumber.IndexOf("ZZ") == 0)
                {
                    if (G1.validate_numeric(location))
                    {
                        cmd = "Select * from `icontracts` c JOIN `icustomers` b ON c.`contractNumber` = b.`contractNumber` WHERE c.`contractNumber` = '" + contractNumber + "';";
                        dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count > 0)
                        {
                            payer = dx.Rows[0]["payer"].ObjToString();
                            location = ImportDailyDeposits.FindLastPaymentLocation(payer, ref oldloc);
                            location = CleanupLocation(location);
                        }
                    }
                }
                else if (type == "Trust")
                {
                    if (G1.validate_numeric(location))
                    {
                        dRows = funDt.Select("SDICode='" + location + "'");
                        if (dRows.Length > 0)
                            location = dRows[0]["LocationCode"].ObjToString();
                    }
                    else
                    {
                        Lines = location.Split(' ');
                        for (int i = 0; i < Lines.Length; i++)
                        {
                            wordLocation = Lines[i].Trim();
                            if (wordLocation == "Colonial")
                                continue;
                            if (wordLocation == "Colonial-")
                                continue;
                            if (wordLocation == "Funeral")
                                continue;
                            dRows = funDt.Select("LocationCode LIKE '%" + wordLocation + "'");
                            if (dRows.Length > 0)
                            {
                                location = dRows[0]["LocationCode"].ObjToString();
                                break;
                            }
                        }
                    }
                }
                else
                {
                    if (G1.validate_numeric(location))
                    {
                        dRows = funDt.Select("SDICode='" + location + "'");
                        if (dRows.Length > 0)
                            location = dRows[0]["LocationCode"].ObjToString();
                    }
                    else
                    {
                        Lines = location.Split(' ');
                        for (int i = 0; i < Lines.Length; i++)
                        {
                            wordLocation = Lines[i].Trim();
                            if (wordLocation == "Colonial")
                                continue;
                            if (wordLocation == "Colonial-")
                                continue;
                            if (wordLocation == "Funeral")
                                continue;
                            dRows = funDt.Select("LocationCode LIKE '%" + wordLocation + "'");
                            if (dRows.Length > 0)
                            {
                                location = dRows[0]["LocationCode"].ObjToString();
                                break;
                            }
                            else
                            {
                                dRows = funDt.Select("atneedcode = '" + wordLocation + "'");
                                if (dRows.Length > 0)
                                {
                                    location = dRows[0]["LocationCode"].ObjToString();
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch ( Exception ex )
            {
            }
            return location;
        }
        /****************************************************************************************/
        private void btnEditCombos_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            EditLocationCombos comboForm = new EditLocationCombos( workReport );
            comboForm.Show();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void chkUseCombos_CheckedChanged(object sender, EventArgs e)
        {
        }
        /****************************************************************************************/
        private void btnSelectColumns_Click(object sender, EventArgs e)
        {
            string actualName = cmbSelectColumns.Text;
            SelectColumns sform = new SelectColumns(dgv, "ACH Bank Deposits " + workReport, "Primary", actualName);
            sform.Done += new SelectColumns.d_void_eventdone(sform_Done);
            sform.Show();
        }
        /***********************************************************************************************/
        void sform_Done(DataTable dt)
        {
            string name = cmbSelectColumns.Text.Trim();
            if (String.IsNullOrWhiteSpace(name))
                name = workReport + " Primary";
            string saveName = "ACH Bank Deposits " + workReport + " " + name;
            string skinName = "";
            SetupSelectedColumns("ACH Bank Deposits " + workReport, name, dgv);
            foundLocalPreference = G1.RestoreGridLayout(this, this.dgv, gridMain, LoginForm.username, saveName, ref skinName);
            gridMain.OptionsView.ShowFooter = true;
            SetupTotalsSummary();
            string field = "";
            string select = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                field = dt.Rows[i]["field"].ObjToString();
                select = dt.Rows[i]["select"].ObjToString();
                if (G1.get_column_number(gridMain, field) >= 0)
                {
                    if (select == "0")
                        gridMain.Columns[field].Visible = false;
                    else
                        gridMain.Columns[field].Visible = true;
                }
            }
            dgv.Refresh();
            this.Refresh();
        }
        /***********************************************************************************************/
        void sform_Done()
        {
            dgv.Refresh();
            this.Refresh();
        }
        /***********************************************************************************************/
        private void SetupSelectedColumns(string procType, string group, DevExpress.XtraGrid.GridControl dgv)
        {
            if (String.IsNullOrWhiteSpace(group))
                return;
            if (String.IsNullOrWhiteSpace(procType))
                procType = "ACH Bank Deposits " + workReport;
            string cmd = "Select * from procfiles where name = '" + group + "' and ProcType = '" + procType + "' order by seq";
            DataTable dt = G1.get_db_data(cmd);
            DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gridMain = (DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView)dgv.MainView;
            for (int i = 0; i < gridMain.Columns.Count; i++)
                gridMain.Columns[i].Visible = false;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string name = dt.Rows[i]["Description"].ToString();
                int index = dt.Rows[i]["seq"].ObjToInt32();
                try
                {
                    ((GridView)dgv.MainView).Columns[name].Visible = true;
                }
                catch
                {
                }
            }
        }
        /****************************************************************************************/
        private void lockScreenDetailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string name = cmbSelectColumns.Text.Trim();
            if (String.IsNullOrWhiteSpace(name))
                name = "Primary";
            string saveName = "ACH Bank Deposits " + workReport + " " + name;
            G1.SaveLocalPreferences(this, gridMain, LoginForm.username, saveName);

            //G1.SaveLocalPreferences(this, gridMain, LoginForm.username, "DailyHistoryLayout" );
            foundLocalPreference = true;
        }
        /****************************************************************************************/
        private void unLockScreenDetailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string comboName = cmbSelectColumns.Text;
            if (!String.IsNullOrWhiteSpace(comboName))
            {
                string name = comboName;
                if (String.IsNullOrWhiteSpace(name))
                    name = "Primary";
                string saveName = "ACH Bank Deposits " + workReport + " " + name;
                G1.RemoveLocalPreferences(LoginForm.username, saveName);
                foundLocalPreference = false;
            }

            //G1.RemoveLocalPreferences(LoginForm.username, "DailyHistoryLayout");
            foundLocalPreference = false;
        }
        /***********************************************************************************************/
        void skinForm_SkinSelected(string s)
        {
            if (s.ToUpper().IndexOf("SKIN : ") >= 0)
            {
                string skin = s.Replace("Skin : ", "");
                if (skin.Trim().Length == 0)
                    skin = "Windows Default";
                if (skin == "Windows Default")
                {
                    this.LookAndFeel.SetSkinStyle(skin);
                    this.gridMain.Appearance.EvenRow.BackColor = System.Drawing.Color.LightGreen;
                    this.gridMain.Appearance.EvenRow.BackColor2 = System.Drawing.Color.LightGreen;
                    this.gridMain.Appearance.SelectedRow.BackColor = System.Drawing.Color.Yellow;
                    this.gridMain.Appearance.SelectedRow.ForeColor = System.Drawing.Color.Black;
                }
                else
                {
                    this.panelTop.BackColor = Color.Transparent;
                    this.menuStrip1.BackColor = Color.Transparent;
                    this.gridMain.PaintStyleName = "Skin";
                    DevExpress.Skins.SkinManager.EnableFormSkins();
                    this.LookAndFeel.UseDefaultLookAndFeel = true;
                    DevExpress.LookAndFeel.UserLookAndFeel.Default.SetSkinStyle(skin);
                    this.LookAndFeel.SetSkinStyle(skin);
                    this.dgv.LookAndFeel.SetSkinStyle(skin);
                    this.dgv.LookAndFeel.SkinName = skin;
                    gridMain.Appearance.EvenRow.Options.UseBackColor = false;
                    gridMain.Appearance.OddRow.Options.UseBackColor = false;
                    this.panelTop.Refresh();
                    OnSkinChange(skin);

                    //DevExpress.LookAndFeel.UserLookAndFeel.Default.SkinName = skin;
                    //this.LookAndFeel.SetSkinStyle(skin);
                    //this.dgv.LookAndFeel.SetSkinStyle(skin);
                }
            }
            else if (s.ToUpper().IndexOf("COLOR : ") >= 0)
            {
                string color = s.Replace("Color : ", "");
                this.gridMain.Appearance.EvenRow.BackColor = Color.FromName(color);
                this.gridMain.Appearance.EvenRow.BackColor2 = Color.FromName(color);
                this.gridMain.Appearance.SelectedRow.BackColor = System.Drawing.Color.Yellow;
                this.gridMain.Appearance.SelectedRow.ForeColor = System.Drawing.Color.Black;
                this.gridMain.Appearance.EvenRow.Options.UseBackColor = true;
                this.gridMain.Appearance.OddRow.Options.UseBackColor = true;
            }
            else if (s.ToUpper().IndexOf("NO COLOR ON") >= 0)
            {
                this.gridMain.Appearance.EvenRow.Options.UseBackColor = false;
                this.gridMain.Appearance.OddRow.Options.UseBackColor = false;
            }
            else if (s.ToUpper().IndexOf("NO COLOR OFF") >= 0)
            {
                this.gridMain.Appearance.EvenRow.Options.UseBackColor = true;
                this.gridMain.Appearance.OddRow.Options.UseBackColor = true;
            }
        }
        /***************************************************************************************/
        public delegate void d_void_eventdone_string(string s);
        public event d_void_eventdone_string SkinChange;
        protected void OnSkinChange(string done)
        {
            if (SkinChange != null)
                SkinChange.Invoke(done);
        }
        /***********************************************************************************************/
        private void SetupSelectedColumns()
        {
            string group = cmbSelectColumns.Text.Trim().ToUpper();
            if (group.Trim().Length == 0)
                return;
            string procName = "ACH Bank Deposits " + workReport;
            string cmd = "Select * from procfiles where name = '" + group + "' and ProcType = '" + procName + "' order by seq";
            DataTable dt = G1.get_db_data(cmd);
            for (int i = 0; i < gridMain.Columns.Count; i++)
                gridMain.Columns[i].Visible = false;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string name = dt.Rows[i]["Description"].ToString();
                int index = dt.Rows[i]["seq"].ObjToInt32();
                try
                {
                    ((GridView)dgv.MainView).Columns[name].Visible = true;
                }
                catch
                {
                }
            }
        }
        /***********************************************************************************************/
        private void loadGroupCombo(System.Windows.Forms.ComboBox cmb, string key, string module)
        {
            string primaryName = "";
            cmb.Items.Clear();
            string cmd = "Select * from procfiles where ProcType = '" + key + "' AND `module` = '" + module + "' group by name;";
            DataTable dt = G1.get_db_data(cmd);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string name = dt.Rows[i]["Name"].ToString();
                if (name.Trim().ToUpper() == "PRIMARY")
                    primaryName = name;
                cmb.Items.Add(name);
            }
            if (!String.IsNullOrWhiteSpace(primaryName))
                cmb.Text = primaryName;
        }
        /****************************************************************************************/
        private void cmbSelectColumns_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            string comboName = cmbSelectColumns.Text;
            string skinName = "";
            if (!String.IsNullOrWhiteSpace(comboName))
            {
                SetupSelectedColumns("ACH Bank Deposits " + workReport, comboName, dgv);
                string name = "ACH Bank Deposits " + workReport + " " + comboName;
                foundLocalPreference = G1.RestoreGridLayout(this, this.dgv, gridMain, LoginForm.username, name, ref skinName);
                SetupTotalsSummary();
                gridMain.OptionsView.ShowFooter = true;
            }
            else
            {
                SetupSelectedColumns("ACH Bank Deposits " + workReport, "Primary", dgv);
                string name = "ACH Bank Deposits" + workReport + " Primary";
                foundLocalPreference = G1.RestoreGridLayout(this, this.dgv, gridMain, LoginForm.username, name, ref skinName);
                gridMain.OptionsView.ShowFooter = true;
                SetupTotalsSummary();
            }
        }
        /****************************************************************************************/
        private void gridMain_CustomDrawFooterCell(object sender, FooterCellCustomDrawEventArgs e)
        {
            if (newFont != null)
                e.Appearance.Font = newFont;
        }
        /****************************************************************************************/
    }
}