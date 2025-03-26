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
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraEditors.Repository;
using System.Security.RightsManagement;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class DownPayments : DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        private bool loading = false;
        private string workTitle = "";
        private bool workVerify = false;
        private bool modified = false;
        private DataTable feeDt = null;
        private DataTable workDt = null;
        private int workLastRow = -1;
        private string workContractNumber = "";
        private string workLastName = "";
        private string workFirstName = "";
        private DateTime workDate = DateTime.Now;
        private bool matching = false;
        /***********************************************************************************************/
        public DownPayments( string title = "", bool verify = false )
        {
            InitializeComponent();
            workTitle = title;
            workVerify = verify;
            SetupTotalsSummary();
        }
        /***********************************************************************************************/
        public DownPayments( DataTable dx, string title = "" )
        {
            InitializeComponent();
            workDt = dx;
            workTitle = title;
            SetupTotalsSummary();
        }
        /***********************************************************************************************/
        public DownPayments(string contractNumber, string lastName, string firstName, DateTime depositDate, string title = "")
        {
            InitializeComponent();
            workContractNumber = contractNumber;
            workLastName = lastName;
            workFirstName = firstName;
            workDate = depositDate;
            workTitle = title;
            matching = true;
            SetupTotalsSummary();
        }
        /***********************************************************************************************/
        private void DownPayments_Load(object sender, EventArgs e)
        {
            G1.SetupVisibleColumns(gridMain, this.columnsToolStripMenuItem, nmenu_Click);

            gridMain.Columns["payment"].Visible = false;

            LoadData();

            if ( workDt != null )
            {
                workDt.Columns.Add("myDate");
                workDt.Columns.Add("mod");
                workDt.Columns.Add("status");
                workDt.Columns.Add("contractNumber");
                workDt.Columns.Add("payer");

                for (int i = 0; i < workDt.Rows.Count; i++)
                {
                    workDt.Rows[i]["myDate"] = workDt.Rows[i]["date"].ObjToDateTime().ToString("MM/dd/yyyy");
                }

                G1.NumberDataTable(workDt);
                dgv.DataSource = workDt;

                originalDt = workDt;

                btnRun.Hide();
                btnRun.Refresh();

                dgv.RefreshDataSource();
                gridMain.RefreshData();
                dgv.Refresh();
                this.Refresh();
            }
            this.Text = workTitle;

            feeDt = LoadFeeTable();
            //gridMain.Columns["totalDeposit"].Visible = false;
        }
        /****************************************************************************************/
        private void LoadData()
        {
            loadLocatons();

            string cmd = "Select * from `downpayments` where `record` = 'XYZZYAAA';";
            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("myDate");
            dt.Columns.Add("mod");
            dt.Columns.Add("status");
            dt.Columns.Add("contractNumber");
            dt.Columns.Add("payer");
            //dt.Columns.Add("localDescription");
            G1.NumberDataTable(dt);

            if (matching)
                dt = matchData();
            else
                dgv.ContextMenu = null;

            dgv.DataSource = dt;
            loading = false;
            btnSave.Hide();
            if (!workVerify)
                gridMain.Columns["status"].Visible = false;
            gridMain.Columns["contractNumber"].Visible = false;
            gridMain.Columns["payer"].Visible = false;
        }
        /****************************************************************************************/
        private DataTable matchData ()
        {
            bool byName = false;
            string cmd = "";
            if (!String.IsNullOrWhiteSpace(workContractNumber))
                cmd = "Select * from `downpayments` where `trustNumber` = '" + workContractNumber + "';";
            else
            {
                DateTime date = workDate.AddDays(-30);
                string firstDate = date.ToString("yyyyMMdd");
                date = workDate.AddDays(30);
                string lastDate = date.ToString("yyyyMMdd");

                this.dateTimePicker1.Value = firstDate.ObjToDateTime();
                this.dateTimePicker2.Value = lastDate.ObjToDateTime();

                cmd = "Select * from `downpayments` where `date` >= '" + firstDate + "' AND `date` <= '" + lastDate + " ";
                cmd += " AND `lastName` = '" + workLastName + "' ";
                cmd += ";";
                byName = true;
            }

            DataTable dt = G1.get_db_data(cmd);
            if ( dt.Rows.Count <= 0 && !byName )
            {
                DateTime date = workDate.AddDays(-30);
                string firstDate = date.ToString("yyyyMMdd");
                date = workDate.AddDays(30);
                string lastDate = date.ToString("yyyyMMdd");

                this.dateTimePicker1.Value = firstDate.ObjToDateTime();
                this.dateTimePicker2.Value = lastDate.ObjToDateTime();

                cmd = "Select * from `downpayments` where `date` >= '" + firstDate + "' AND `date` <= '" + lastDate + "' ";
                cmd += " AND `lastName` = '" + workLastName + "' ";
                cmd += ";";
                dt = G1.get_db_data(cmd);
                if ( dt.Rows.Count > 0 )
                {
                    DataRow[] dRows = dt.Select("firstName='" + workFirstName + "'");
                    if (dRows.Length > 0)
                        dt = dRows.CopyToDataTable();
                }
            }

            dt.Columns.Add("num");
            dt.Columns.Add("myDate");
            dt.Columns.Add("mod");
            dt.Columns.Add("status");
            dt.Columns.Add("contractNumber");
            dt.Columns.Add("payer");

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dt.Rows[i]["myDate"] = dt.Rows[i]["date"].ObjToDateTime().ToString("MM/dd/yyyy");
            }

            G1.NumberDataTable(dt);

            gridMain.Columns["trustNumber"].OptionsColumn.AllowEdit = true;

            return dt;
        }
        /****************************************************************************************/
        private void SetupTotalsSummary()
        {
            AddSummaryColumn("payment", null);
            AddSummaryColumn("downPayment", null);
            AddSummaryColumn("lossRecoveryFee", null);
            AddSummaryColumn("totalDeposit", null);
            AddSummaryColumn("ccFee", null);
        }
        /***********************************************************************************************/
        private void loadLocatons()
        {
            string cmd = "Select * from `funeralhomes` order by `keycode`;";
            DataTable locDt = G1.get_db_data(cmd);
            chkComboLocNames.Properties.DataSource = locDt;
            chkComboLocation.Properties.DataSource = locDt;

            for (int i = 0; i < locDt.Rows.Count; i++)
                this.repositoryItemComboBox2.Items.Add(locDt.Rows[i]["locationCode"].ObjToString());
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
            printableComponentLink1.Landscape = true;

            Printer.setupPrinterMargins(50, 100, 130, 50);

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
            //printableComponentLink1.ShowPreviewDialog();
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

            Printer.setupPrinterMargins(50, 100, 130, 50);

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
            string title = "Down Payments Report";
            Printer.DrawQuad(6, 7, 4, 4, title, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);


            //            Printer.DrawQuadTicks();
            DateTime date = this.dateTimePicker2.Value;
            string workDate = date.ToString("MM/dd/yyyy");
            Printer.SetQuadSize(24, 12);
            font = new Font("Ariel", 9, FontStyle.Regular);
            title = "Month Ending: ";
            string lock1 = this.dateTimePicker1.Value.ToString("MM/dd/yyyy");
            string lock2 = this.dateTimePicker2.Value.ToString("MM/dd/yyyy");
            Printer.DrawQuad(20, 3, 5, 4, "Stop " + lock2, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            Printer.DrawQuad(20, 1, 5, 4, "Start " + lock1, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /****************************************************************************************/
        private int footerCount = 0;
        private void gridMain_BeforePrintRow(object sender, DevExpress.XtraGrid.Views.Printing.CancelPrintRowEventArgs e)
        {
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
                if (footerCount >= 2)
                {
                    footerCount = 0;
//                    e.PS.InsertPageBreak(e.Y);
                }
            }
        }
        /****************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            G1.SpyGlass(gridMain);
            //if (dgv.Visible)
            //    SetSpyGlass(gridMain);
        }
        /***********************************************************************************************/
        private void SetSpyGlass(DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView grid)
        {
            if (grid.OptionsFind.AlwaysVisible == true)
                grid.OptionsFind.AlwaysVisible = false;
            else
                grid.OptionsFind.AlwaysVisible = true;
        }
        /***********************************************************************************************/
        private void btnRun_Click(object sender, EventArgs e)
        {
            //gridMain.Columns["totalDeposit"].Visible = false;

            this.Cursor = Cursors.WaitCursor;

            DateTime date = dateTimePicker1.Value;
            DateTime beginningDate = date;
            DateTime oldImportDate = DailyHistory.majorDate;

            string date1 = G1.DateTimeToSQLDateTime(date);
            date = dateTimePicker2.Value;
            string date2 = G1.DateTimeToSQLDateTime(date);

            DateTime now = DateTime.Now;

            DateTime paidout = new DateTime(2039, 12, 31);

            string cmd = "Select * from `downpayments` WHERE `date` >= '" + date1 + "' AND `date` <= '" + date2 + "';";

            DataTable dt = G1.get_db_data(cmd);

            dt.Columns.Add("num");
            dt.Columns.Add("myDate");
            dt.Columns.Add("mod");
            dt.Columns.Add("status");
            dt.Columns.Add("contractNumber");
            dt.Columns.Add("payer");
            //dt.Columns.Add("totalDeposit", Type.GetType("System.Double"));

            decimal downPayment = 0;
            decimal payment = 0;
            decimal actualPayment = 0;
            string deposit = "";
            string depositNumber = "";
            string status = "";
            DataTable dx = null;
            bool match = false;

            decimal lossRecovery = 0;
            decimal ccFee = 0;
            decimal totalDeposit = 0;
            decimal testDeposit = 0;

            if (!workVerify)
                gridMain.Columns["status"].Visible = false;
            gridMain.Columns["contractNumber"].Visible = false;
            gridMain.Columns["payer"].Visible = false;

            if ( G1.isAdminOrSuper() )
            {
                gridMain.Columns["trustNumber"].OptionsColumn.AllowEdit = true;
                gridMain.Columns["contractNumber"].OptionsColumn.AllowEdit = true;
            }


            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dt.Rows[i]["myDate"] = dt.Rows[i]["date"].ObjToDateTime().ToString("MM/dd/yyyy");
                if ( workVerify )
                {
                    downPayment = dt.Rows[i]["downPayment"].ObjToDecimal();
                    payment = dt.Rows[i]["payment"].ObjToDecimal();
                    if (downPayment > 0)
                    {
                        depositNumber = dt.Rows[i]["depositNumber"].ObjToString();
                        cmd = "Select * from `payments` where `depositNumber` = '" + depositNumber + "';";
                        dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count < 0)
                        {
                            cmd = "Select * from `ipayments` where `depositNumber` = '" + depositNumber + "';";
                            dx = G1.get_db_data(cmd);
                        }
                        if (dx.Rows.Count <= 0)
                            dt.Rows[i]["status"] = "NOT FOUND";
                        else
                        {
                            match = false;
                            for (int j = 0; j < dx.Rows.Count; j++)
                            {
                                actualPayment = dx.Rows[j]["downPayment"].ObjToDecimal();
                                if (downPayment != actualPayment)
                                    dt.Rows[i]["status"] = "Different";
                                else
                                {
                                    dt.Rows[i]["status"] = "Verified";
                                    dt.Rows[i]["contractNumber"] = dx.Rows[j]["contractNumber"].ObjToString();
                                    dt.Rows[i]["payer"] = dx.Rows[j]["payer"].ObjToString();
                                    break;
                                }
                            }
                        }
                    }
                    else if (payment > 0)
                    {
                        depositNumber = dt.Rows[i]["depositNumber"].ObjToString();
                        cmd = "Select * from `payments` where `depositNumber` = '" + depositNumber + "';";
                        dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count < 0)
                        {
                            cmd = "Select * from `ipayments` where `depositNumber` = '" + depositNumber + "';";
                            dx = G1.get_db_data(cmd);
                        }
                        if (dx.Rows.Count <= 0)
                            dt.Rows[i]["status"] = "NOT FOUND";
                        else
                        {
                            dt.Rows[i]["contractNumber"] = dx.Rows[i]["contractNumber"].ObjToString();
                            dt.Rows[i]["payer"] = dx.Rows[i]["payer"].ObjToString();
                            actualPayment = dx.Rows[0]["paymentAmount"].ObjToDecimal();
                            if (payment != actualPayment)
                                dt.Rows[i]["status"] = "Different";
                            else
                                dt.Rows[i]["status"] = "Verified";
                        }
                    }
                }
                else
                {
                    downPayment = dt.Rows[i]["downPayment"].ObjToDecimal();
                    lossRecovery = dt.Rows[i]["lossRecoveryFee"].ObjToDecimal();
                    ccFee = dt.Rows[i]["ccFee"].ObjToDecimal();

                    totalDeposit = downPayment + lossRecovery + ccFee;

                    testDeposit = dt.Rows[i]["totalDeposit"].ObjToDecimal();
                    if (testDeposit != totalDeposit)
                    {
                        btnSave.Show();
                        btnSave.Refresh();
                        dt.Rows[i]["mod"] = "Y";
                    }
                    dt.Rows[i]["totalDeposit"] = totalDeposit;
                }
            }

            //for ( int i=(dt.Rows.Count - 1); i>=0; i--)
            //{
            //    status = dt.Rows[i]["status"].ObjToString();
            //    if (status.ToUpper() == "DELETE")
            //        dt.Rows.RemoveAt(i);
            //}

            G1.NumberDataTable(dt);
            dgv.DataSource = dt;

            originalDt = dt;

            dgv.RefreshDataSource();
            gridMain.RefreshData();
            dgv.Refresh();
            this.Refresh();

            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void chkComboLocation_EditValueChanged(object sender, EventArgs e)
        {
            string names = getLocationQuery();
            DataRow[] dRows = originalDt.Select(names);
            DataTable dt = originalDt.Clone();
            for (int i = 0; i < dRows.Length; i++)
                dt.ImportRow(dRows[i]);
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
        }
        /***********************************************************************************************/
        private void chkComboLocNames_EditValueChanged(object sender, EventArgs e)
        {
            string names = getLocationNameQuery();
            DataRow[] dRows = originalDt.Select(names);
            DataTable dt = originalDt.Clone();
            for (int i = 0; i < dRows.Length; i++)
                dt.ImportRow(dRows[i]);
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
        }
        /*******************************************************************************************/
        private string getLocationQuery()
        {
            string procLoc = "";
            string[] locIDs = this.chkComboLocation.EditValue.ToString().Split('|');
            for (int i = 0; i < locIDs.Length; i++)
            {
                if (!String.IsNullOrWhiteSpace(locIDs[i]))
                {
                    if (procLoc.Trim().Length > 0)
                        procLoc += ",";
                    string cmd = "Select * from `funeralhomes` where `keycode` = '" + locIDs[i].Trim() + "';";
                    DataTable dt = G1.get_db_data(cmd);
                    if (dt.Rows.Count > 0)
                    {
                        string id = dt.Rows[0]["locationCode"].ObjToString();
                        procLoc += "'" + id.Trim() + "'";
                    }
                }
            }
            return procLoc.Length > 0 ? " `location` IN (" + procLoc + ") " : "";
        }
        /*******************************************************************************************/
        private string getLocationNameQueryx()
        {
            string procLoc = "";
            string[] locIDs = this.chkComboLocNames.EditValue.ToString().Split('|');
            for (int i = 0; i < locIDs.Length; i++)
            {
                if (!String.IsNullOrWhiteSpace(locIDs[i]))
                {
                    if (procLoc.Trim().Length > 0)
                        procLoc += ",";
                    string cmd = "Select * from `funeralhomes` where `name` = '" + locIDs[i].Trim() + "';";
                    DataTable dt = G1.get_db_data(cmd);
                    if (dt.Rows.Count > 0)
                    {
                        string id = dt.Rows[0]["keycode"].ObjToString();
                        procLoc += "'" + id.Trim() + "'";
                    }
                }
            }
            return procLoc.Length > 0 ? " `location` IN (" + procLoc + ") " : "";
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
                    procLoc += "'" + locIDs[i].Trim() + "'";
                }
            }
            return procLoc.Length > 0 ? " `location` IN (" + procLoc + ") " : "";
        }
        /***********************************************************************************************/
        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            DateTime date = this.dateTimePicker1.Value;
//            GetWeeklyDate(date);
        }
        /***********************************************************************************************/
        private void dateTimePicker2_ValueChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            DateTime date = this.dateTimePicker2.Value;
//            GetWeeklyDate(date);
        }
        /***********************************************************************************************/
        private void GetWeeklyDate(DateTime date, string direction )
        {
            loading = true;
            int idx = 1;
            if (direction.ToUpper() == "BACK")
                idx = -1;
            DateTime idate = date;
            if (idate.DayOfWeek == DayOfWeek.Sunday)
            {
                this.dateTimePicker1.Value = date;
                this.dateTimePicker2.Value = date.AddDays(6);
                return;
            }
            for (;;)
            {
                idate = idate.AddDays(idx);
                if (idate.DayOfWeek == DayOfWeek.Sunday)
                {
                    this.dateTimePicker1.Value = idate;
                    this.dateTimePicker2.Value = idate.AddDays(6);
                    break;
                }
            }
            loading = false;
        }
        /***********************************************************************************************/
        private void btnRight_Click(object sender, EventArgs e)
        {
            DateTime now = this.dateTimePicker2.Value;
            if (cmbDateType.Text.ToUpper() == "WEEKLY")
            {
                GetWeeklyDate(this.dateTimePicker2.Value, "FORWARD");
                return;
            }
            else if (cmbDateType.Text.ToUpper() == "DAILY")
            {
                now = now.AddDays(1);
                this.dateTimePicker1.Value = now;
                this.dateTimePicker2.Value = now;
                return;
            }
            now = now.AddMonths(1);
            now = new DateTime(now.Year, now.Month, 1);
            this.dateTimePicker1.Value = now;
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            this.dateTimePicker2.Value = new DateTime(now.Year, now.Month, days);
        }
        /***********************************************************************************************/
        private void btnLeft_Click(object sender, EventArgs e)
        {
            DateTime now = this.dateTimePicker2.Value;
            if (cmbDateType.Text.ToUpper() == "WEEKLY")
            {
                GetWeeklyDate(this.dateTimePicker2.Value, "BACK");
                return;
            }
            else if (cmbDateType.Text.ToUpper() == "DAILY")
            {
                now = now.AddDays(-1);
                this.dateTimePicker1.Value = now;
                this.dateTimePicker2.Value = now;
                return;
            }
            now = now.AddMonths(-1);
            now = new DateTime(now.Year, now.Month, 1);
            this.dateTimePicker1.Value = now;
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            this.dateTimePicker2.Value = new DateTime(now.Year, now.Month, days);
        }
        /***********************************************************************************************/
        private void chkSort_CheckedChanged(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (chkSort.Checked)
            {
                DataView tempview = dt.DefaultView;
                tempview.Sort = "location asc, date asc";
                dt = tempview.ToTable();
                G1.NumberDataTable(dt);
                dgv.DataSource = dt;

                gridMain.Columns["location"].GroupIndex = 0;
                gridMain.OptionsView.ShowFooter = true;
                this.gridMain.ExpandAllGroups();
            }
            else
            {
                DataView tempview = dt.DefaultView;
                tempview.Sort = "date asc";
                dt = tempview.ToTable();
                G1.NumberDataTable(dt);
                dgv.DataSource = dt;

                gridMain.Columns["location"].GroupIndex = -1;
                gridMain.OptionsView.ShowFooter = true;
                gridMain.CollapseAllGroups();
            }
            gridMain.RefreshData();
            dgv.Refresh();
        }
        /***********************************************************************************************/
        private void gridMain_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv.DataSource;
            string mod = dt.Rows[row]["mod"].ObjToString();
            if ( mod == "D" )
            {
                e.Visible = false;
                e.Handled = true;
                return;
            }
            if (!chkProblems.Checked)
                return;
            string status = dt.Rows[row]["status"].ObjToString();
            if ( status.ToUpper() == "VERIFIED")
            {
                e.Visible = false;
                e.Handled = true;
                return;
            }
        }
        /***********************************************************************************************/
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
        }
        /***********************************************************************************************/
        private void pictureAdd_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            int lines = 1;
            for (int i = 0; i < lines; i++)
            {
                DataRow dRow = dt.NewRow();
                dRow["num"] = dt.Rows.Count.ObjToInt32() + 1;
                dt.Rows.Add(dRow);
            }

            int row = dt.Rows.Count - 1;
            dt.Rows[row]["myDate"] = DateTime.Now.ToString("MM/dd/yyyy");
            dt.Rows[row]["user"] = LoginForm.username;
            dt.Rows[row]["mod"] = "Y";
            dgv.DataSource = dt;
            gridMain.SelectRow(row);
            gridMain.FocusedRowHandle = row;
            dgv.RefreshDataSource();
            dgv.Refresh();
            FixBank("");
        }
        /***********************************************************************************************/
        private void pictureDelete_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            DataTable dt = (DataTable)dgv.DataSource;
            string record = dr["record"].ObjToString();
            //string service = dr["report"].ObjToString();
            DialogResult result = MessageBox.Show("***Question*** Are you sure you want to delete this Down Payment ?", "Delete Down Payment Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;

            //DataTable dt = (DataTable)dgv.DataSource;
            int[] rows = gridMain.GetSelectedRows();
            int dtRow = 0;
            int firstRow = 0;
            if (rows.Length > 0)
                firstRow = rows[0];
            int row = 0;
            try
            {
                loading = true;
                for (int i = 0; i < rows.Length; i++)
                {
                    row = rows[i];
                    dtRow = gridMain.GetDataSourceRowIndex(row);
                    if (dtRow < 0 || dtRow > (dt.Rows.Count - 1))
                    {
                        continue;
                    }
                    var dRow = gridMain.GetDataRow(row);
                    if (dRow != null)
                        dRow["mod"] = "D";
                    dt.Rows[dtRow]["mod"] = "D";
                    modified = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("*ERROR*** " + ex.Message.ToString());
            }

            loading = false;
            if (firstRow > (dt.Rows.Count - 1))
                firstRow = (dt.Rows.Count - 1);
            dgv.DataSource = dt;
            gridMain.RefreshData();
            dgv.Refresh();

            gridMain.FocusedRowHandle = firstRow;
            gridMain.SelectRow(firstRow);
        }
        /***********************************************************************************************/
        private void btnSave_Click(object sender, EventArgs e)
        {
            if (workVerify)
                return;
            SaveAllData();
        }
        /***********************************************************************************************/
        private void SaveAllData()
        {
            DateTime date = DateTime.Now;
            string depositNumber = "";
            decimal downPayment = 0;
            decimal lossRecoveryFee = 0;
            decimal payment = 0;
            decimal totalDeposit = 0;
            decimal ccFee = 0;
            string trustNumber = "";
            string firstName = "";
            string lastName = "";
            string record = "";
            string location = "";
            string paymentType = "";
            string bankAccount = "";
            string localDescription = "";
            string mod = "";
            string user = LoginForm.username;

            bool adding = false;
            string what = "";
            string detail = "";

            this.Cursor = Cursors.WaitCursor;

            DataTable dt = (DataTable)dgv.DataSource;
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                record = dt.Rows[i]["record"].ObjToString();
                mod = dt.Rows[i]["mod"].ObjToString();
                if ( mod == "D")
                {
                    if (!String.IsNullOrWhiteSpace(record))
                        G1.delete_db_table("downpayments", "record", record);
                    dt.Rows[i]["mod"] = "";
                    dt.Rows[i]["record"] = -1;
                    continue;
                }
                if (mod != "Y")
                    continue;
                adding = false;
                if (String.IsNullOrWhiteSpace(record))
                {
                    record = G1.create_record("downpayments", "firstName", "-1");
                    adding = true;
                }
                if (G1.BadRecord("downpayments", record))
                    continue;
                dt.Rows[i]["record"] = record;
                trustNumber = dt.Rows[i]["trustNumber"].ObjToString();
                date = dt.Rows[i]["myDate"].ObjToDateTime();
                location = dt.Rows[i]["location"].ObjToString();
                downPayment = dt.Rows[i]["downPayment"].ObjToDecimal();
                lossRecoveryFee = dt.Rows[i]["lossRecoveryFee"].ObjToDecimal();
                payment = dt.Rows[i]["payment"].ObjToDecimal();
                depositNumber = dt.Rows[i]["depositNumber"].ObjToString();
                firstName = dt.Rows[i]["firstName"].ObjToString();
                lastName = dt.Rows[i]["lastName"].ObjToString();
                paymentType = dt.Rows[i]["paymentType"].ObjToString();
                bankAccount = dt.Rows[i]["bankAccount"].ObjToString();
                localDescription = dt.Rows[i]["localDescription"].ObjToString();
                ccFee = dt.Rows[i]["ccFee"].ObjToDecimal();
                totalDeposit = downPayment + lossRecoveryFee + payment + ccFee;


                G1.update_db_table("downpayments", "record", record, new string[] { "depositNumber", depositNumber, "date", date.ToString("yyyy-MM-dd"), "downPayment", downPayment.ToString(), "lossRecoveryFee", lossRecoveryFee.ToString(), "payment", payment.ToString(), "totalDeposit", totalDeposit.ToString(), "firstName", firstName, "lastName", lastName, "location", location, "paymentType", paymentType, "bankAccount", bankAccount, "localDescription", localDescription, "ccFee", ccFee.ToString(), "user", user } );
                if ( matching )
                    G1.update_db_table("downpayments", "record", record, new string[] { "trustNumber", trustNumber });
                else if ( G1.isAdminOrSuper () )
                    G1.update_db_table("downpayments", "record", record, new string[] { "trustNumber", trustNumber });

                dt.Rows[i]["mod"] = "";

                what = "Adding";
                if (!adding)
                    what = "Modifying";
                detail = date.ToString("yyyy-MM-dd") + "~" + paymentType + "~" + lastName + "~" + G1.ReformatMoney((double) totalDeposit);

                G1.AddToAudit(LoginForm.username, "DownPayments", what, detail, depositNumber);
            }
            btnSave.Hide();
            modified = false;
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void DownPayments_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (modified)
            {
                DialogResult result = MessageBox.Show("***Question***\nData has been modified!\nWould you like to save your changes?", "Data Modified Dialog", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
                if (result == DialogResult.Yes)
                    SaveAllData();
            }
        }
        /***********************************************************************************************/
        public static decimal GetLossRecoveryFee ()
        {
            decimal fee = 0;
            string cmd = "Select * from `options`;";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return fee;
            DataRow[] dRows = dt.Select("option='Loss Recovery Fee'");
            if (dRows.Length <= 0)
                return fee;
            string answer = dRows[0]["answer"].ObjToString().ToUpper();
            if (String.IsNullOrWhiteSpace(answer))
                return fee;
            if (G1.validate_numeric(answer))
                fee = answer.ObjToDecimal();
            return fee;
        }
        /***********************************************************************************************/
        public static DataTable LoadFeeTable()
        {
            DateTime date = DateTime.Now;
            DataTable dt = G1.get_db_data("Select * from `creditcard_fees` ORDER BY `beginDate` DESC;");
            dt.Columns.Add("bDate");
            dt.Columns.Add("eDate");

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                date = dt.Rows[i]["endDate"].ObjToDateTime();
                if (date.Year <= 1000)
                    dt.Rows[i]["endDate"] = G1.DTtoMySQLDT(DateTime.Now);
            }

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                date = dt.Rows[i]["beginDate"].ObjToDateTime();
                dt.Rows[i]["bDate"] = date.ToString("yyyyMMdd");

                date = dt.Rows[i]["endDate"].ObjToDateTime();
                dt.Rows[i]["eDate"] = date.ToString("yyyyMMdd");
            }

            DataView tempview = dt.DefaultView;
            tempview.Sort = "endDate DESC";
            dt = tempview.ToTable();
            return dt;
        }
        /***********************************************************************************************/
        public static double GetCreditCardFee(DataTable feeDt, DateTime dateReceived)
        {
            if (feeDt == null)
                return 0D;
            if (feeDt.Rows.Count <= 0)
                return 0D;

            double fee = 0D;
            try
            {
                DataRow[] dRows = feeDt.Select("eDate>='" + dateReceived.ToString("yyyyMMdd") + "'");
                if (dRows.Length > 0)
                {
                    //DataTable dd = dRows.CopyToDataTable();
                    fee = dRows[0]["fee"].ObjToDouble();
                    fee = fee / 100D;
                }
            }
            catch (Exception ex)
            {
            }
            return fee;
        }
        /***********************************************************************************************/
        private string currentColumn = "";
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (loading || workVerify )
                return;
            DataRow dr = gridMain.GetFocusedDataRow();
            DataTable dt = (DataTable)dgv.DataSource;
            int rowhandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowhandle);
            string user = dr["user"].ObjToString();

            double payment = 0D;
            double lossRecoveryFee = 0D;
            double fee = 0D;
            double ccFee = 0D;
            double totalDeposit = 0D;

            string paymentType = dr["paymentType"].ObjToString();

            fee = GetCreditCardFee(feeDt, DateTime.Now);

            GridColumn currCol = gridMain.FocusedColumn;
            currentColumn = currCol.FieldName;
            if (currentColumn.ToUpper() == "DOWNPAYMENT" )
            {
                dr["payment"] = 0;
                dr["lossRecoveryFee"] = GetLossRecoveryFee();
                payment = dr["downPayment"].ObjToDouble();
                if (paymentType.ToUpper() == "CREDIT CARD")
                {
                    ccFee = payment * fee;
                    ccFee = G1.RoundValue(ccFee);
                    dr["ccFee"] = (decimal)ccFee;
                }
                else
                    dr["ccFee"] = (decimal)0;
                dr["totalDeposit"] = dr["downPayment"].ObjToDecimal() + dr["lossRecoveryFee"].ObjToDecimal() + dr["ccfee"].ObjToDecimal();
                FixPaymentType();
            }
            else if ( currentColumn.ToUpper() == "LOSSRECOVERYFEE")
            {
                payment = dr["downPayment"].ObjToDouble();
                if (paymentType.ToUpper() == "CREDIT CARD")
                {
                    ccFee = payment * fee;
                    ccFee = G1.RoundValue(ccFee);
                    dr["ccFee"] = (decimal)ccFee;
                }
                else
                    dr["ccFee"] = (decimal)0;
                lossRecoveryFee = dr["lossRecoveryFee"].ObjToDouble();
                totalDeposit = payment + lossRecoveryFee + ccFee;
                dr["totalDeposit"] = totalDeposit;
                FixPaymentType();
            }
            else if (currentColumn.ToUpper() == "PAYMENT")
            {
                dr["downPayment"] = 0;
                dr["lossRecoveryFee"] = 0;
                payment = dr["payment"].ObjToDouble();
                if (paymentType.ToUpper() == "CREDIT CARD")
                {
                    ccFee = payment * fee;
                    ccFee = G1.RoundValue(ccFee);
                    dr["ccFee"] = (decimal)ccFee;
                }
                else
                    dr["ccFee"] = (decimal)0;
                dr["totalDeposit"] = dr["payment"].ObjToDecimal() + (decimal) ccFee;
                FixPaymentType();
            }
            else if (currentColumn.ToUpper() == "CCFEE")
            {
                payment = dr["downPayment"].ObjToDouble();
                dr["payment"] = 0;
                //dr["lossRecoveryFee"] = GetLossRecoveryFee();
                payment = dr["payment"].ObjToDouble();
                ccFee = dr["ccFee"].ObjToDouble();
                dr["totalDeposit"] = dr["downPayment"].ObjToDecimal() + dr["lossRecoveryFee"].ObjToDecimal() + dr["ccfee"].ObjToDecimal();
                FixPaymentType();
            }
            else if (currentColumn.ToUpper() == "PAYMENTTYPE")
            {
                payment = dr["downPayment"].ObjToDouble();
                if (payment > 0D)
                {
                    if (paymentType.ToUpper() == "CREDIT CARD")
                    {
                        ccFee = payment * fee;
                        ccFee = G1.RoundValue(ccFee);
                        dr["ccFee"] = (decimal)ccFee;
                    }
                    else
                        dr["ccFee"] = (decimal)0;
                    dr["payment"] = 0;
                    //dr["lossRecoveryFee"] = GetLossRecoveryFee();
                    dr["totalDeposit"] = dr["downPayment"].ObjToDecimal() + dr["lossRecoveryFee"].ObjToDecimal() + dr["ccfee"].ObjToDecimal();
                }
                FixPaymentType();
                tryBankSetup();
            }

            dr["mod"] = "Y";
            dt.Rows[row]["mod"] = "Y";
            modified = true;
            btnSave.Show();
        }
        /***********************************************************************************************/
        private bool validEdit ( string user )
        {
            if (workVerify)
                return true;
            if (user == LoginForm.username)
                return true;
            string answer = G1.getPreference(LoginForm.username, "DownPayments", "Allow Edit");
            if (answer.Trim().ToUpper() == "YES")
                return true;
            string str = "                      ***ERROR***\nYou are not the same user who last created or edited this down payment or payment!\n";
            str += "Or you do not have permission to edit this down payment!\n";
            str += "Sorry!";
            MessageBox.Show(str, "Down Payment/Payment Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
        /***********************************************************************************************/
        private string GetDepositBankAccount( string paymentType, string location )
        {
            string bankAccount = "";
            if (!String.IsNullOrWhiteSpace(location))
            {
                string cmd = "Select * from `funeralhomes` where `locationCode` = '" + location + "';";
                DataTable dt = G1.get_db_data(cmd);
                if (dt.Rows.Count > 0)
                {
                    if (String.IsNullOrWhiteSpace(paymentType))
                        paymentType = "CASH";
                    if (paymentType.ToUpper() == "CASH")
                        bankAccount = SetupBanks(dt, "cashLocal");
                    else if (paymentType.ToUpper() == "CHECK-LOCAL")
                        bankAccount = SetupBanks(dt, "checkLocal");
                    else if (paymentType.ToUpper() == "CHECK-REMOTE")
                        bankAccount = SetupMainBanks("checkRemote");
                    else if (paymentType.ToUpper() == "CREDIT CARD")
                        bankAccount = SetupMainBanks("ccInsTrusts");
                }
            }
            return bankAccount;
        }
        /***********************************************************************************************/
        private DataTable bankDt = null;
        private string SetupMainBanks(string what)
        {
            string bankAccount = "";
            string bankList = "";
            string generalLedgerNo = "";
            if (bankDt == null)
                bankDt = G1.get_db_data("Select * from `bank_accounts`;");
            if (bankDt.Rows.Count <= 0)
                return bankAccount;
            DataRow[] dRows = bankDt.Select(what + "='1'");
            for (int i = 0; i < dRows.Length; i++)
            {
                bankList += dRows[i]["localDescription"].ObjToString() + "~";
                if (String.IsNullOrWhiteSpace(bankAccount))
                {
                    generalLedgerNo = dRows[i]["general_ledger_no"].ObjToString();
                    bankAccount = dRows[i]["account_no"].ObjToString();
                    bankAccount = dRows[i]["localDescription"].ObjToString() + "~" + generalLedgerNo + "~" + bankAccount;
                }
            }

            if (!String.IsNullOrWhiteSpace(bankList))
                FixBank(bankList);

            return bankAccount;
        }
        /***********************************************************************************************/
        private string SetupBanksx ( DataTable dt, string what )
        {
            string bankAccount = "";
            string bankList = "";
            string cmd = "";
            string str = dt.Rows[0][what].ObjToString();
            str = str.TrimEnd('~');
            if (!String.IsNullOrWhiteSpace(str))
            {
                try
                {
                    DataTable dx = null;
                    string record = "";
                    string[] Lines = str.Split('~');
                    string[] account = null;
                    string general_ledger_no = "";
                    string account_no = "";
                    string localDescription = "";
                    string location = "";
                    for (int i = 0; i < Lines.Length; i++)
                    {

                        record = Lines[i].Trim();
                        account = record.Split('/');
                        if (account.Length < 2)
                            continue;
                        general_ledger_no = account[0].Trim();
                        account_no = account[1].Trim();
                        //cmd = "Select * from `bank_accounts` where `record` = '" + record + "';";
                        cmd = "Select * from `bank_accounts` where `general_ledger_no` = '" + general_ledger_no + "' and `account_no` = '" + account_no + "';";
                        dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count > 0)
                        {
                            localDescription = dx.Rows[0]["localDescription"].ObjToString();
                            location = dx.Rows[0]["locationCode"].ObjToString();
                            if (String.IsNullOrWhiteSpace(localDescription))
                                localDescription = location;
                            bankList += localDescription + "~";
                            if (String.IsNullOrWhiteSpace(bankAccount))
                                bankAccount = dx.Rows[0]["account_no"].ObjToString();
                        }
                    }
                    FixBank(bankList);
                }
                catch ( Exception ex)
                {
                }
            }
            return bankAccount;
        }
        /***********************************************************************************************/
        private string SetupBanks(DataTable dt, string what)
        {
            string bankAccount = "";
            string bankList = "";
            string cmd = "";
            string generalLedgerNo = "";
            string[] account = null;
            string str = dt.Rows[0][what].ObjToString();
            str = str.TrimEnd('~');
            if (!String.IsNullOrWhiteSpace(str))
            {
                DataTable dx = null;
                string record = "";
                string general_ledger_no = "";
                string account_no = "";
                string[] Lines = str.Split('~');
                for (int i = 0; i < Lines.Length; i++)
                {
                    record = Lines[i].Trim();
                    account = record.Split('/');
                    if (account.Length < 2)
                        continue;
                    general_ledger_no = account[0].Trim();
                    account_no = account[1].Trim();
                    //cmd = "Select * from `bank_accounts` where `record` = '" + record + "';";
                    cmd = "Select * from `bank_accounts` where `general_ledger_no` = '" + general_ledger_no + "' and `account_no` = '" + account_no + "';";
                    //cmd = "Select * from `bank_accounts` where `account_no` = '" + account_no + "';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                    {
                        bankList += dx.Rows[0]["localDescription"].ObjToString() + "~";
                        if (String.IsNullOrWhiteSpace(bankAccount))
                        {
                            generalLedgerNo = dx.Rows[0]["general_ledger_no"].ObjToString();
                            bankAccount = dx.Rows[0]["account_no"].ObjToString();
                            bankAccount = dx.Rows[0]["localDescription"].ObjToString() + "~" + generalLedgerNo + "~" + bankAccount;
                        }
                    }
                }
                FixBank(bankList);
            }
            return bankAccount;
        }
        /***********************************************************************************************/
        private void repositoryItemComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        { // Payment Type Changed
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowhandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowhandle);

            ComboBoxEdit combo = (ComboBoxEdit)sender;
            string paymentType = combo.Text.Trim().ToUpper();
            if ( paymentType.ToUpper() == "CREDIT CARD")
            {
            }
            string location = dr["location"].ObjToString();
            if (!String.IsNullOrWhiteSpace(location))
            {
                try
                {
                    string bankAccount = GetDepositBankAccount(paymentType, location);
                    if (!String.IsNullOrWhiteSpace(bankAccount))
                    {
                        dr["bankAccount"] = bankAccount;
                        dt.Rows[row]["bankAccount"] = bankAccount;
                        dgv.RefreshDataSource();
                        dgv.Refresh();
                    }
                }
                catch ( Exception ex)
                {
                }
            }
        }
        /***********************************************************************************************/
        private void repositoryItemComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        { // Location Changed
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowhandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowhandle);

            ComboBoxEdit combo = (ComboBoxEdit)sender;
            string location = combo.Text.Trim().ToUpper();
            string paymentType = dr["paymentType"].ObjToString();
            if (!String.IsNullOrWhiteSpace(location))
            {
                dr["paymentType"] = "Cash";
                dr["bankAccount"] = "";
                dr["localDescription"] = "";
                paymentType = "";
                string bankAccount = GetDepositBankAccount(paymentType, location);
                dr["bankAccount"] = bankAccount;
                dr["location"] = combo.Text.Trim();
                dt.Rows[row]["bankAccount"] = bankAccount;
                dt.Rows[row]["location"] = combo.Text.Trim();
                dgv.RefreshDataSource();
                dgv.Refresh();
            }
        }
        /***********************************************************************************************/
        private void cmbDateType_SelectedIndexChanged(object sender, EventArgs e)
        {
            string dateType = cmbDateType.Text;
            if (dateType.ToUpper() == "MONTHLY")
            {
                DateTime now = DateTime.Now;
                now = new DateTime(now.Year, now.Month, 1);
                this.dateTimePicker1.Value = now;
                int days = DateTime.DaysInMonth(now.Year, now.Month);
                this.dateTimePicker2.Value = new DateTime(now.Year, now.Month, days);
            }
            else if ( dateType.ToUpper() == "WEEKLY")
            {
                GetWeeklyDate(this.dateTimePicker2.Value, "BACK");
            }
            else
            {
                this.dateTimePicker2.Value = this.dateTimePicker1.Value;
            }
            this.Refresh();
        }
        /***********************************************************************************************/
        private void gridMain_ShowingEditor(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowhandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowhandle);
            string user = dr["user"].ObjToString();
            if (String.IsNullOrWhiteSpace(user))
                return;
            if (!validEdit(user))
                e.Cancel = true;
        }
        /***********************************************************************************************/
        RepositoryItemComboBox ciLookup = null;
        private void gridMain_ShownEditor(object sender, EventArgs e)
        {
            if (workVerify)
                return;
            if ( ciLookup == null)
            {
                ciLookup = new RepositoryItemComboBox();
                ciLookup.SelectedIndexChanged += repositoryItemComboBox1_SelectedIndexChanged;
            }
            GridColumn currCol = gridMain.FocusedColumn;
            currentColumn = currCol.FieldName;
            if (currentColumn.ToUpper() != "PAYMENTTYPE")
                return;
            DataTable dt = (DataTable)dgv.DataSource;
            int focusedRow = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(focusedRow);
            //gridMain.ClearSelection();
            //gridMain.SelectRow(focusedRow);
            //gridMain.FocusedRowHandle = row;
            //DataRow dr = gridMain.GetFocusedDataRow();
            decimal downPayment = dt.Rows[row]["downPayment"].ObjToDecimal();
            decimal payment = dt.Rows[row]["payment"].ObjToDecimal();

            string paymentType = dt.Rows[row]["paymentType"].ObjToString();

            bool gotDownPayment = true;
            if (payment > 0)
                gotDownPayment = false;
            ciLookup.Items.Clear();
            if ( gotDownPayment)
            {
                ciLookup.Items.Add("Cash");
                ciLookup.Items.Add("Check-Local");
                ciLookup.Items.Add("Check-Remote");
                ciLookup.Items.Add("Credit Card");
            }
            else
            {
                ciLookup.Items.Add("Cash");
                ciLookup.Items.Add("Check-Local");
            }
            gridMain.Columns["paymentType"].ColumnEdit = ciLookup;
            FixBank ( "", paymentType );
        }
        private void gridMain_ShownEditorxx(object sender, EventArgs e)
        {
            if (workVerify)
                return;
            if (ciLookup == null)
            {
                ciLookup = new RepositoryItemComboBox();
                ciLookup.SelectedIndexChanged += repositoryItemComboBox1_SelectedIndexChanged;
            }
            GridColumn currCol = gridMain.FocusedColumn;
            currentColumn = currCol.FieldName;
            if (currentColumn.ToUpper() != "PAYMENTTYPE")
                return;
            int focusedRow = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(focusedRow);
            gridMain.ClearSelection();
            gridMain.SelectRow(focusedRow);
            gridMain.FocusedRowHandle = row;
            DataRow dr = gridMain.GetFocusedDataRow();
            decimal downPayment = dr["downPayment"].ObjToDecimal();
            decimal payment = dr["payment"].ObjToDecimal();
            string paymentType = dr["paymentType"].ObjToString();

            bool gotDownPayment = true;
            if (payment > 0)
                gotDownPayment = false;
            ciLookup.Items.Clear();
            if (gotDownPayment)
            {
                ciLookup.Items.Add("Cash");
                ciLookup.Items.Add("Check-Local");
                ciLookup.Items.Add("Check-Remote");
                ciLookup.Items.Add("Credit Card");
            }
            else
            {
                ciLookup.Items.Add("Cash");
                ciLookup.Items.Add("Check-Local");
            }
            gridMain.Columns["paymentType"].ColumnEdit = ciLookup;
            FixBank("", paymentType);
        }
        /***********************************************************************************************/
        private void FixPaymentType()
        {
            if (workVerify)
                return;
            if (ciLookup == null)
            {
                ciLookup = new RepositoryItemComboBox();
                ciLookup.SelectedIndexChanged += repositoryItemComboBox1_SelectedIndexChanged;
            }

            GridColumn currCol = gridMain.FocusedColumn;
            currentColumn = currCol.FieldName;
            currentColumn = "paymentType";
            int focusedRow = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(focusedRow);
            gridMain.ClearSelection();
            gridMain.SelectRow(focusedRow);
            gridMain.FocusedRowHandle = row;
            DataRow dr = gridMain.GetFocusedDataRow();
            decimal downPayment = dr["downPayment"].ObjToDecimal();
            decimal payment = dr["payment"].ObjToDecimal();

            bool gotDownPayment = true;
            if (payment > 0)
                gotDownPayment = false;
            ciLookup.Items.Clear();
            if (gotDownPayment)
            {
                ciLookup.Items.Add("Cash");
                ciLookup.Items.Add("Check-Local");
                ciLookup.Items.Add("Check-Remote");
                ciLookup.Items.Add("Credit Card");
            }
            else
            {
                ciLookup.Items.Add("Cash");
                ciLookup.Items.Add("Check-Local");
            }
            gridMain.Columns["paymentType"].ColumnEdit = ciLookup;
            FixBank("");
        }
        /***********************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            if (!workVerify)
                return;
            DataRow dr = gridMain.GetFocusedDataRow();
            string contractNumber = dr["contractNumber"].ObjToString();
            if (String.IsNullOrWhiteSpace(contractNumber))
                return;
            string payer = dr["payer"].ObjToString();
            if ( DailyHistory.isInsurance ( contractNumber))
            {
                this.Cursor = Cursors.WaitCursor;
                DataTable dt = (DataTable)dgv.DataSource;
                CustomerDetails clientForm = new CustomerDetails(contractNumber);
                clientForm.Show();
                this.Cursor = Cursors.Default;
            }
            else
            {
                this.Cursor = Cursors.WaitCursor;
                DataTable dt = (DataTable)dgv.DataSource;
                CustomerDetails clientForm = new CustomerDetails(contractNumber);
                clientForm.Show();
                this.Cursor = Cursors.Default;
            }
        }
        /***********************************************************************************************/
        private void chkProblems_CheckedChanged(object sender, EventArgs e)
        {
            gridMain.RefreshData();
            dgv.Refresh();
        }
        /***********************************************************************************************/
        private DataTable originalDt = null;
        private void chkSortDeposit_CheckedChanged(object sender, EventArgs e)
        {
            if ( chkSortDeposit.Checked )
            {
                originalDt = (DataTable)dgv.DataSource;
                DataTable dt = (DataTable)dgv.DataSource;
                DataView tempview = dt.DefaultView;
                tempview.Sort = "depositNumber asc";
                dt = tempview.ToTable();
                DataTable dx = dt.Clone();
                int row = 0;
                DataRow dRow = null;
                string depositNumber = "";
                string depositStr = "";
                double totalDeposit = 0D;
                double downPayment = 0D;
                int count = 0;
                for ( int i=0; i<dt.Rows.Count; i++)
                {
                    try
                    {
                        depositNumber = dt.Rows[i]["depositNumber"].ObjToString();
                        if (String.IsNullOrEmpty(depositNumber))
                        {
                            dx.ImportRow(dt.Rows[i]);
                            continue;
                        }
                        if (String.IsNullOrWhiteSpace(depositStr))
                            depositStr = depositNumber;
                        if (depositStr != depositNumber)
                        {
                            if (count > 0)
                            {
                                row = dx.Rows.Count - 1;
                                dx.ImportRow(dx.Rows[row]);
                                row = dx.Rows.Count - 1;
                                dx.Rows[row]["depositNumber"] = depositStr;
                                dx.Rows[row]["totalDeposit"] = totalDeposit;
                                dx.Rows[row]["downPayment"] = 0D;
                                dx.Rows[row]["lastName"] = "";
                                dx.Rows[row]["firstName"] = "";
                                count = 0;
                                totalDeposit = 0D;
                            }
                        }
                        count++;
                        depositStr = depositNumber;
                        downPayment = dt.Rows[i]["downPayment"].ObjToDouble();
                        totalDeposit += downPayment;
                        dx.ImportRow(dt.Rows[i]);
                        row = dx.Rows.Count - 1;
                        dx.Rows[row]["totalDeposit"] = downPayment;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("***ERROR*** " + ex.Message.ToString());
                    }
                }
                if ( totalDeposit > 0D && count > 1 )
                {
                    try
                    {
                        row = dx.Rows.Count - 1;
                        dx.ImportRow(dx.Rows[row]);
                        row = dx.Rows.Count - 1;
                        dx.Rows[row]["depositNumber"] = depositStr;
                        dx.Rows[row]["totalDeposit"] = totalDeposit;
                        dx.Rows[row]["downPayment"] = 0D;
                        dx.Rows[row]["lastName"] = "Combo";
                        dx.Rows[row]["firstName"] = "Combo";
                    }
                    catch ( Exception ex)
                    {
                        MessageBox.Show("***ERROR2*** " + ex.Message.ToString());
                    }
                }
                string name = "";
                depositStr = "";
                totalDeposit = 0D;
                for (int i = 0; i < dx.Rows.Count; i++)
                {
                    try
                    {
                        totalDeposit = dx.Rows[i]["totalDeposit"].ObjToDouble();
                        downPayment = dx.Rows[i]["downPayment"].ObjToDouble();
                        depositStr = dx.Rows[i]["depositNumber"].ObjToString();
                        if (downPayment != totalDeposit)
                        {
                            for ( int j=(i-1); j>=0; j--)
                            {
                                depositNumber = dx.Rows[j]["depositNumber"].ObjToString();
                                if (depositNumber != depositStr)
                                    break;
                                dx.Rows[j]["totalDeposit"] = 0D;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
                gridMain.Columns["totalDeposit"].Visible = true;
                dgv.DataSource = dx;
                dgv.Refresh();
            }
            else
            {
                gridMain.Columns["totalDeposit"].Visible = false;
                dgv.DataSource = originalDt;
                gridMain.RefreshData();
                dgv.RefreshDataSource();
                dgv.Refresh();
            }
        }
        /***********************************************************************************************/
        RepositoryItemComboBox ciLookup2 = null;
        private void FixBank(string bankList, string paymentType = "")
        {
            if (workVerify)
                return;
            bankList = bankList.TrimEnd('~');
            string[] Lines = bankList.Split('~');

            if (ciLookup2 == null)
            {
                ciLookup2 = new RepositoryItemComboBox();
                ciLookup2.SelectedIndexChanged += repositoryItemComboBox3_SelectedIndexChanged;
            }

            GridColumn currCol = gridMain.FocusedColumn;
            currentColumn = currCol.FieldName;
            currentColumn = "paymentType";
            int focusedRow = gridMain.FocusedRowHandle;

            int row = gridMain.GetDataSourceRowIndex(focusedRow);
            //gridMain.ClearSelection();
            //gridMain.SelectRow(focusedRow);
            //gridMain.FocusedRowHandle = row;

            DataTable dt = (DataTable)dgv.DataSource;
            //if ( paymentType.ToUpper() != "CREDIT CARD" )
            //    dt.Rows[row]["localDescription"] = "";
            string saveLocation = dt.Rows[row]["localDescription"].ObjToString();

            ciLookup2.Items.Clear();
            string localDescription = "";
            string saveDescription = "";
            bool first = true;
            for (int i = 0; i < Lines.Length; i++)
            {
                localDescription = Lines[i].Trim();
                if (!String.IsNullOrWhiteSpace(localDescription))
                {
                    ciLookup2.Items.Add(localDescription);
                    if (first)
                    {
                        dt.Rows[row]["localDescription"] = localDescription;
                        saveDescription = localDescription;
                    }
                    first = false;
                }
            }
            gridMain.Columns["localDescription"].ColumnEdit = ciLookup2;
            gridMain.RefreshData();

            if (!String.IsNullOrWhiteSpace(paymentType))
            {
                string location = dt.Rows[row]["location"].ObjToString();
                if (!String.IsNullOrWhiteSpace(location))
                {
                    string bankAccount = GetDepositBankAccount(paymentType, location);
                    dt.Rows[row]["localDescription"] = saveLocation;
                    if (!String.IsNullOrWhiteSpace(saveDescription))
                    {
                        string cmd = "Select * from `bank_accounts` where `localDescription` = '" + saveDescription + "';";
                        DataTable dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count <= 0)
                        {
                            cmd = "Select * from `bank_accounts` where `account_title` = '" + saveDescription + "';";
                            dx = G1.get_db_data(cmd);
                        }
                        if (dx.Rows.Count > 0)
                        {
                            bankAccount = dx.Rows[0]["account_no"].ObjToString();
                            dt.Rows[row]["bankAccount"] = bankAccount;
                            gridMain.RefreshData();
                        }
                        else
                        {
                        }
                    }
                }
            }
        }
        /***********************************************************************************************/
        private void repositoryItemComboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBoxEdit edit = (ComboBoxEdit)sender;
            string str = edit.Text;
            string cmd = "Select * from `bank_accounts` where `localDescription` = '" + str + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
            {
                cmd = "Select * from `bank_accounts` where `account_title` = '" + str + "';";
                dt = G1.get_db_data(cmd);
            }
            if ( dt.Rows.Count > 0 )
            {
                string bankAccount = dt.Rows[0]["account_no"].ObjToString();
                DataTable dx = (DataTable)dgv.DataSource;
                int focusedRow = gridMain.FocusedRowHandle;
                int row = gridMain.GetDataSourceRowIndex(focusedRow);
                dx.Rows[row]["bankAccount"] = bankAccount;
                gridMain.RefreshData();
            }
        }
        /***********************************************************************************************/
        private void repositoryItemComboBox1_Enter(object sender, EventArgs e)
        {
            //DataTable dx = (DataTable)dgv.DataSource;
            //int focusedRow = gridMain.FocusedRowHandle;
            //if (focusedRow < 0)
            //    return;
            //int row = gridMain.GetDataSourceRowIndex(focusedRow);
            //if (row < 0)
            //    return;
            //string paymentType = dx.Rows[row]["paymentType"].ObjToString();
            //string localDescription = dx.Rows[row]["localDescription"].ObjToString();

            //string cmd = "Select * from `bank_accounts` where `localDescription` = '" + localDescription + "';";
            //DataTable dt = G1.get_db_data(cmd);

            //if (String.IsNullOrWhiteSpace(paymentType))
            //    paymentType = "CASH";
            //if (paymentType.ToUpper() == "CASH")
            //    SetupBanks(dt, "cashLocal");
            //else if (paymentType.ToUpper() == "CHECK-LOCAL")
            //    SetupBanks(dt, "checkLocal");
            //else if (paymentType.ToUpper() == "CHECK-REMOTE")
            //    SetupBanks(dt, "checkRemote");
            //else if (paymentType.ToUpper() == "CREDIT CARD")
            //    SetupBanks(dt, "ccInsTrusts");
        }
        /***********************************************************************************************/
        private void tryBankSetup ( int mainRowHandle = -1 )
        { // Location Changed
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowhandle = gridMain.FocusedRowHandle;
            if (mainRowHandle >= 0)
                rowhandle = mainRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowhandle);
            dr = dt.Rows[row];

            string location = dt.Rows[row]["location"].ObjToString();
            string paymentType = dt.Rows[row]["paymentType"].ObjToString();
            if (!String.IsNullOrWhiteSpace(location))
            {
                //dr["paymentType"] = "Cash";
                //dr["bankAccount"] = "";
                //dr["localDescription"] = "";
                //paymentType = "";

                //string bankAccount = GetDepositBankAccount(paymentType, location);

                string bankAccount = GetDepositBankAccount(paymentType, location);
                dr["bankAccount"] = bankAccount;
                dr["location"] = location;
                dt.Rows[row]["bankAccount"] = bankAccount;
                dt.Rows[row]["location"] = location;
                dgv.RefreshDataSource();
                dgv.Refresh();

                workLastRow = rowhandle;


                //dr["bankAccount"] = bankAccount;
                //dt.Rows[row]["bankAccount"] = bankAccount;
                //dgv.RefreshDataSource();
                //dgv.Refresh();
            }
        }
        /***********************************************************************************************/
        private void gridMain_MouseDown(object sender, MouseEventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource; // Leave as a GetDate example

            var hitInfo = gridMain.CalcHitInfo(e.Location);
            if (hitInfo.InRowCell)
            {
                int rowHandle = hitInfo.RowHandle;
                //if (workLastRow == rowHandle)
                //    return;
                GridColumn column = hitInfo.Column;
                string currentColumn = column.FieldName.Trim();
                DataRow dr = gridMain.GetFocusedDataRow();
                if (currentColumn.ToUpper() == "LOCALDESCRIPTION")
                    tryBankSetup( rowHandle );
            }
        }
        /***********************************************************************************************/
        private void updateTrustNumberToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();

            dr["trustNumber"] = workContractNumber;
            dr["mod"] = "Y";
            gridMain.RefreshEditor( true );

            btnSave.Show();
            btnSave.Refresh();
        }
        /***********************************************************************************************/
    }
}