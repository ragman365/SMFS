using System;
using System.Data;
using System.Windows.Forms;
using GeneralLib;

using System.Drawing;
using DevExpress.XtraPrinting;
using DevExpress.Utils;
using DevExpress.Pdf;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class Contracts : DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        private DataTable originalDt = null;
        private DataTable oDt = null;
        private bool loading = false;
        private bool insurance = false;
        /***********************************************************************************************/
        public Contracts()
        {
            InitializeComponent();
        }
        /***********************************************************************************************/
        private void XtraForm1_Load(object sender, EventArgs e)
        {
            SetupTotalsSummary();
            SetupVisibleColumns();
            LoadData();
        }
        /****************************************************************************************/
        private void SetupTotalsSummary()
        {
            AddSummaryColumn("serviceTotal");
            AddSummaryColumn("merchandiseTotal");
            AddSummaryColumn("downPayment");
            AddSummaryColumn("purchase");
            AddSummaryColumn("paid");
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
        private void LoadData()
        {
            this.Cursor = Cursors.WaitCursor;
            DataTable dt = null;

            DateTime date = dateTimePicker1.Value;
            string date1 = G1.DateTimeToSQLDateTime(date);
            date = dateTimePicker2.Value;
            string date2 = G1.DateTimeToSQLDateTime(date);
            string issueDate = "`issueDate8` >= '" + date1 + "' and `issueDate8` <= '" + date2 + "' ";

            string contractsFile = "contracts";
            string customersFile = "customers";
            string paymentsFile = "payments";
            insurance = false;

            if ( cmbType.Text.ToUpper() == "INSURANCE")
            {
                contractsFile = "icontracts";
                customersFile = "icustomers";
                paymentsFile = "ipayments";
                insurance = true;
            }

            string cmd = "Select * from `" + contractsFile + "` p JOIN `" + customersFile + "` d ON p.`contractNumber` = d.`contractNumber` ";
            if (chkNewContracts.Checked)
                cmd += " WHERE + " + issueDate;

            string what = cmbType.Text.Trim().ToUpper();

            if (cmd.ToUpper().IndexOf("WHERE") > 0)
            {
                if (what == "TRUSTS")
                    cmd += " AND d.`coverageType` <> 'ZZ' "; // T
                else if (what == "SINGLE PREMIUM")
                    cmd += " AND `downpayment` >= (`serviceTotal`+`merchandiseTotal`-`allowInsurance` - `allowMerchandise`) AND `serviceTotal` > '0' AND d.`coverageType` <> 'ZZ' ";
                else if (what == "INSURANCE")
                    cmd += " AND d.`coverageType` = 'ZZ' ";
            }
            else
            {
                if (what == "TRUSTS")
                    cmd += " WHERE d.`coverageType` <> 'ZZ' "; // T
                else if (what == "SINGLE PREMIUM")
                    cmd += " WHERE `downpayment` >= (`serviceTotal`+`merchandiseTotal`-`allowInsurance` - `allowMerchandise`) AND `serviceTotal` > '0' AND d.`coverageType` <> 'ZZ' ";
                else if (what == "INSURANCE")
                    cmd += " WHERE d.`coverageType` = 'ZZ' ";
            }

            cmd += ";";
            dt = G1.get_db_data(cmd);

            dt.Columns.Add("num");
            dt.Columns.Add("agreement");
            dt.Columns.Add("paid", Type.GetType("System.Double"));
            dt.Columns.Add("purchase", Type.GetType("System.Double"));

            G1.NumberDataTable(dt);
            SetupAgreementIcon(dt);
            CalcPaid(dt);
            DetermineLapsed(dt);
            if (oDt == null)
                oDt = dt.Copy();
            originalDt = dt.Copy();
            dgv.DataSource = dt;
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private DataTable CheckNewContracts ()
        {
            DateTime date = this.dateTimePicker1.Value;
            string date1 = G1.DateTimeToSQLDateTime(date);
            date = this.dateTimePicker2.Value;
            string date2 = G1.DateTimeToSQLDateTime(date);

            long startDays = G1.date_to_days(date1);
            long stopDays = G1.date_to_days(date2);
            long date3 = 0L;

            DataTable dt = originalDt.Clone();
            for ( int i=0; i<originalDt.Rows.Count; i++)
            {
                date1 = originalDt.Rows[i]["issueDate8"].ObjToDateTime().ToString("MM/dd/yyyy");
                date3 = G1.date_to_days(date1);
                if (date3 >= startDays && date3 <= stopDays)
                    dt.ImportRow(originalDt.Rows[i]);
            }
            return dt;
        }
        /***********************************************************************************************/
        private void DetermineLapsed(DataTable dt)
        {
            if (G1.get_column_number(dt, "lapsed1") < 0)
                return;
            string lapse = "";
            string lapse1 = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                lapse = dt.Rows[i]["lapsed"].ObjToString();
                lapse1 = dt.Rows[i]["lapsed1"].ObjToString();
                if (String.IsNullOrWhiteSpace(lapse))
                    lapse = " ";
                lapse += lapse1;
                dt.Rows[i]["lapsed"] = lapse;
            }
        }
        /***********************************************************************************************/
        private void CalcPaid(DataTable dt)
        {
            double serviceTotal = 0D;
            double merchandiseTotal = 0D;
            double totalPurchase = 0D;
            double balanceDue = 0D;
            double paid = 0D;
            double totalPaid = 0D;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                serviceTotal = dt.Rows[i]["serviceTotal"].ObjToDouble();
                merchandiseTotal = dt.Rows[i]["merchandiseTotal"].ObjToDouble();
                balanceDue = dt.Rows[i]["balanceDue"].ObjToDouble();
                totalPurchase = serviceTotal + merchandiseTotal;
                paid = totalPurchase - balanceDue;
                dt.Rows[i]["paid"] = paid;
                dt.Rows[i]["purchase"] = totalPurchase;
                totalPaid += paid;
            }
        }
        /***********************************************************************************************/
        private void SetupVisibleColumns()
        {
            ToolStripMenuItem menu = this.columnsToolStripMenuItem;
            for (int i = 0; i < gridMain.Columns.Count; i++)
            {
                string name = gridMain.Columns[i].Name;
                string caption = gridMain.Columns[i].Caption;
                ToolStripMenuItem nmenu = new ToolStripMenuItem();
                nmenu.Name = name;
                nmenu.Text = caption;
                nmenu.Checked = true;
                nmenu.Click += new EventHandler(nmenu_Click);
                menu.DropDownItems.Add(nmenu);
            }
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
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (gridMain.OptionsFind.AlwaysVisible == true)
                gridMain.OptionsFind.AlwaysVisible = false;
            else
                gridMain.OptionsFind.AlwaysVisible = true;
        }
        /***********************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            //if (!G1.checkUserPreference(LoginForm.username, "DailyHistory", "Change"))
            //    return;
            DataRow dr = gridMain.GetFocusedDataRow();
            string contract = dr["contractNumber"].ObjToString();
            if (!String.IsNullOrWhiteSpace(contract))
            {
                DailyHistory dailyForm = new DailyHistory(contract, null, null);
                dailyForm.Show();
            }
        }
        /***********************************************************************************************/
        private void SetupAgreementIcon(DataTable dt)
        {
            DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew = this.repCheckEdit1;
            selectnew.NullText = "";
            selectnew.ValueChecked = "1";
            selectnew.ValueUnchecked = "0";
            selectnew.ValueGrayed = "";
            string filename = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                filename = dt.Rows[i]["agreementFile"].ObjToString();
                if (!String.IsNullOrWhiteSpace(filename))
                    dt.Rows[i]["agreement"] = "1";
            }
        }
        /***********************************************************************************************/
        private void repCheckEdit1_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)(dgv.DataSource);
            DataRow dr = gridMain.GetFocusedDataRow();
            string contract = dr["contractNumber"].ObjToString();
            string value = dr["agreement"].ObjToString();
            if (value == "1")
            {
                string filename = "";
                string title = "Agreement for (" + contract + ") ";
                string cmd = "Select * from `customers` where `contractNumber` = '" + contract + "';";
                DataTable dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    filename = dx.Rows[0]["agreementFile"].ObjToString();
                    string firstName = dx.Rows[0]["firstName"].ObjToString();
                    string lastName = dx.Rows[0]["lastName"].ObjToString();
                    title = "Agreement for (" + contract + ") " + firstName + " " + lastName;
                    if (!String.IsNullOrWhiteSpace(filename))
                    {
                        string record = dr["!imagesRecord"].ObjToString();
                        if (record != "-1")
                            Customers.ShowPDfImage(record, title, filename);
                    }
                }
            }
        }
        /***********************************************************************************************/
        private void gridMain_DoubleClick_1(object sender, EventArgs e)
        {
            //if (!G1.checkUserPreference(LoginForm.username, "DailyHistory", "Change"))
            //    return;
            DataRow dr = gridMain.GetFocusedDataRow();
            string contract = dr["contractNumber"].ObjToString();
            if (!String.IsNullOrWhiteSpace(contract))
            {
                DailyHistory dailyForm = new DailyHistory(contract, null, null);
                dailyForm.Show();
            }
        }
        /***********************************************************************************************/
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadData();
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
            if (chkShowAll.Checked)
                Printer.DrawQuad(6, 8, 4, 4, "(ALL) Contracts Report", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            else if (chkLapse.Checked)
                Printer.DrawQuad(6, 8, 4, 4, "(Lapsed) Contracts Report", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            else if (chkActiveOnly.Checked)
                Printer.DrawQuad(6, 8, 4, 4, "(Active) Contracts Report", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            else
                Printer.DrawQuad(6, 8, 4, 4, "Contracts Report", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);


            //            Printer.DrawQuadTicks();
            DateTime date = DateTime.Now;
            string workDate = date.Month.ToString("D2") + "/" + date.Year.ToString("D4");
            Printer.SetQuadSize(24, 12);
            font = new Font("Ariel", 9, FontStyle.Bold);
            Printer.DrawQuad(20, 8, 5, 4, "Report Month:" + workDate, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(16, 8, 3, 4, lblPayment.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(19, 8, 3, 4, lblTrust85.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(22, 8, 3, 4, lblTrust100.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /***********************************************************************************************/
        private void chkIncludePaid_CheckedChanged(object sender, EventArgs e)
        {
            if (chkIncludePaid.Checked)
            {
                DataTable dt = (DataTable)dgv.DataSource;
                gridMain.Columns["purchase"].Visible = true;
                gridMain.Columns["paid"].Visible = true;
            }
            else
            {
                gridMain.Columns["purchase"].Visible = false;
                gridMain.Columns["paid"].Visible = false;
            }
        }
        /***********************************************************************************************/
        private void chkLapse_CheckedChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            if (chkLapse.Checked)
            {
                loading = true;
                chkShowAll.Checked = false;
                chkActiveOnly.Checked = false;
                loading = false;
                DataRow[] dRow = originalDt.Select("lapsed<>''");
                DataTable dt = originalDt.Clone();
                for (int i = 0; i < dRow.Length; i++)
                    dt.ImportRow(dRow[i]);
                G1.NumberDataTable(dt);
                dgv.DataSource = dt;
                dgv.RefreshDataSource();
                dgv.Refresh();
            }
            else
            {
                loading = true;
                chkShowAll.Checked = false;
                loading = false;
                G1.NumberDataTable(originalDt);
                dgv.DataSource = originalDt;
                dgv.RefreshDataSource();
                dgv.Refresh();
            }
        }
        /***********************************************************************************************/
        private void chkShowAll_CheckedChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            if (!chkLapse.Checked && !chkActiveOnly.Checked)
            {
                loading = true;
                chkShowAll.Checked = true;
                loading = false;
                return;
            }
            loading = true;
            chkActiveOnly.Checked = false;
            chkLapse.Checked = false;
            loading = false;
            originalDt = oDt;
            dgv.DataSource = originalDt;
            dgv.RefreshDataSource();
            dgv.Refresh();
        }
        /***********************************************************************************************/
        private void chkActiveOnly_CheckedChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            if (chkActiveOnly.Checked)
            {
                loading = true;
                chkShowAll.Checked = false;
                chkLapse.Checked = false;
                loading = false;
                this.Cursor = Cursors.WaitCursor;
                DataRow[] dRow = originalDt.Select("lapsed=''");
                DataTable dt = originalDt.Clone();
                for (int i = 0; i < dRow.Length; i++)
                    dt.ImportRow(dRow[i]);
                G1.NumberDataTable(dt);
                dgv.DataSource = dt;
                dgv.RefreshDataSource();
                dgv.Refresh();
                this.Cursor = Cursors.Default;
            }
            else
            {
                loading = true;
                chkShowAll.Checked = true;
                loading = false;
                G1.NumberDataTable(originalDt);
                dgv.DataSource = originalDt;
                dgv.RefreshDataSource();
                dgv.Refresh();
                this.Cursor = Cursors.Default;
            }
        }
        /***********************************************************************************************/
        private void pictureAdd_Click(object sender, EventArgs e)
        {
            if (!insurance)
            {
                using (NewContract contractForm = new NewContract())
                {
                    contractForm.SelectDone += ContractForm_SelectDone;
                    contractForm.ShowDialog();
                }
            }
            else
            {
                using (NewInsurance contractForm = new NewInsurance())
                {
                    contractForm.SelectDone += ContractForm_SelectDone;
                    contractForm.ShowDialog();
                }
            }
        }
        /***********************************************************************************************/
        private void ContractForm_SelectDone(string contract)
        {
            if (String.IsNullOrWhiteSpace(contract))
                return;
            this.Cursor = Cursors.WaitCursor;
            CustomerDetails custForm = new CustomerDetails(contract);
            custForm.Show();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void gridMain_DoubleClick_2(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string contract = dr["contractNumber"].ObjToString();
            if (!String.IsNullOrWhiteSpace(contract))
            {
                this.Cursor = Cursors.WaitCursor;
                CustomerDetails custForm = new CustomerDetails(contract);
                custForm.Show();
                this.Cursor = Cursors.Default;
            }
        }
        /***********************************************************************************************/
        private void pictureDelete_Click(object sender, EventArgs e)
        {
            if (!LoginForm.administrator)
            {
                MessageBox.Show("***Warning*** You do not have permission to delete a contract!");
                return;
            }

            string customerRecord = "";
            string contractRecord = "";

            DataRow dr = gridMain.GetFocusedDataRow();
            string contract = dr["contractNumber"].ObjToString();
            if (String.IsNullOrWhiteSpace(contract))
            {
                MessageBox.Show("***Warning*** This contract number is empty!!");
                return;
            }

            string contractsFile = "contracts";
            string customersFile = "customers";
            string paymentsFile = "payments";
            bool insurance = false;

            if (DailyHistory.isInsurance(contract) || cmbType.Text.ToUpper() == "INSURANCE")
            {
                contractsFile = "icontracts";
                customersFile = "icustomers";
                paymentsFile = "ipayments";
                insurance = true;
            }

            DialogResult result = MessageBox.Show("***Question*** Are you sure you want to DELETE this Contract (" + contract + ") ?", "Delete Contract Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;

            string cmd = "Select * from `" + customersFile + "` where `contractNumber` = '" + contract + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                MessageBox.Show("***ERROR*** No Customers exist for Contract " + contract + "!");
            else
                customerRecord = dt.Rows[0]["record"].ObjToString();

            cmd = "Select * from `" + contractsFile + "` where `contractNumber` = '" + contract + "';";
            dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                MessageBox.Show("***ERROR*** No Contracts exist for Contract " + contract + "!");
            else
                contractRecord = dt.Rows[0]["record"].ObjToString();

            if (!String.IsNullOrWhiteSpace(contractRecord))
            {
                G1.get_db_data("Delete from `" + contractsFile + "` where `record` = '" + contractRecord + "';");
                G1.get_db_data("Delete from `" + paymentsFile + "` where `contractNumber` = '" + contract + "';");
            }

            if (!String.IsNullOrWhiteSpace(customerRecord))
            {
                G1.get_db_data("Delete from `" + customersFile + "` where `record` = '" + customerRecord + "';");
                G1.get_db_data("Delete from `cust_services` where `contractNumber` = '" + contract + "';");
            }
            if ( insurance )
                G1.AddToAudit(LoginForm.username, "Customers", "Delete Insurance Customer", "Deleted", contract);
            else
                G1.AddToAudit(LoginForm.username, "Customers", "Delete Trust Customer", "Deleted", contract);
        }
        /***********************************************************************************************/
        private void addNewContractToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //string filename = "C:/Users/Robby/Documents/SMFS/Untitled 1.pdf";
            //ViewPDF pdfForm1 = new ViewPDF("This is a Test", filename);
            //pdfForm1.Show();

            NewContract newForm = new NewContract();
            newForm.Show();

        }
        /***********************************************************************************************/
        private void chkNewContracts_CheckedChanged(object sender, EventArgs e)
        {
            int extra = 30;
            if (chkNewContracts.Checked)
                extra = 30;
            else
            {
                extra = -30;
                originalDt = oDt;
                dgv.DataSource = originalDt;
            }
            int top = this.panelTop.Top;
            int left = this.panelTop.Left;
            int width = this.panelTop.Width;
            int height = this.panelTop.Height + extra;

            this.panelTop.SetBounds(left, top, width, height);
            this.panelTop.Refresh();
            this.panelBottom.Refresh();
            this.panelAll.Refresh();
            this.Refresh();
        }
        /***********************************************************************************************/
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
        /***********************************************************************************************/
        private void btnRight_Click(object sender, EventArgs e)
        {
            DateTime now = this.dateTimePicker1.Value;
            DateTime date = new DateTime(now.Year, now.Month, 1);
            date = date.AddMonths(1);
            this.dateTimePicker1.Value = date;
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            date = new DateTime(date.Year, date.Month, days);
            this.dateTimePicker2.Value = date;
        }
        /***********************************************************************************************/
    }
}