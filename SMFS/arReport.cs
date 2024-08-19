using System;
using System.Data;
using System.Windows.Forms;
using GeneralLib;

using System.Drawing;
using DevExpress.XtraPrinting;
using DevExpress.Utils;
using DevExpress.Pdf;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.BandedGrid;
using DevExpress.XtraGrid.Controls;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class arReport : DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        private DataTable originalDt = null;
        private DataTable oDt = null;
        private bool loading = false;
        private bool insurance = false;
        private bool foundLocalPreference = false;
        private GridView originalGridView = null;
        /***********************************************************************************************/
        public arReport()
        {
            InitializeComponent();
        }
        /***********************************************************************************************/
        private void arReport_Load(object sender, EventArgs e)
        {
            loading = true;
            //SetupVisibleColumns();

            loadLocatons();
            SetupTotalsSummary();
            G1.loadGroupCombo(cmbSelectColumns, "Funerals", "Primary", true);
            cmbSelectColumns.Text = "Original";
            loading = false;
            toolStripMenuItem1_Click(null, null);

            this.dateTimePicker1.Value = new DateTime(DateTime.Now.Year, 1, 1);
        }
        /***********************************************************************************************/
        private void loadLocatons()
        {
            string cmd = "Select * from `funeralhomes` order by `keycode`;";
            DataTable locDt = G1.get_db_data(cmd);
            chkComboLocation.Properties.DataSource = locDt;
        }
        /****************************************************************************************/
        private void SetupTotalsSummary()
        {
            //AddSummaryColumn("payment", null);
            AddSummaryColumn("amountReceived", null);
            AddSummaryColumn("amountFiled", null);
            AddSummaryColumn("custPrice", null);
            AddSummaryColumn("custMerchandise", null);
            AddSummaryColumn("custServices", null);
            AddSummaryColumn("totalDiscount", null);
            //AddSummaryColumn("merchandiseDiscount", null);
            //AddSummaryColumn("servicesDiscount", null);
            AddSummaryColumn("currentPrice", null);
            AddSummaryColumn("currentMerchandise", null);
            AddSummaryColumn("currentServices", null);
            AddSummaryColumn("balanceDue", null);
            AddSummaryColumn("additionalDiscount", null);
            AddSummaryColumn("classa", null);
            AddSummaryColumn("grossAmountReceived", null);
            AddSummaryColumn("cashAdvance", null);

            gridMain.Columns["cash"].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Custom;
            gridMain.Columns["cash"].SummaryItem.DisplayFormat = "{0:N2}";
            gridMain.Columns["creditCard"].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Custom;
            gridMain.Columns["creditCard"].SummaryItem.DisplayFormat = "{0:N2}";


            //AddSummaryColumn("currentprice", null);
            //AddSummaryColumn("difference", null);
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
                    procLoc += "'" + locIDs[i].Trim() + "'";
                }
            }
            return procLoc.Length > 0 ? " loc IN (" + procLoc + ") " : "";
        }
        /****************************************************************************************/
        private void AddSummaryColumn(string columnName, DevExpress.XtraGrid.Views.Grid.GridView gMain = null)
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

            string contractsFile = "fcontracts";
            string customersFile = "fcustomers";
            insurance = false;

            string cmd = "SELECT *, (SELECT `pdfimages`.`record` FROM `pdfimages` WHERE `pdfimages`.`contractNumber` = e.`contractNumber` ) AS `picRecord` FROM `fcust_extended` e LEFT JOIN `fcontracts` p ON p.`contractNumber` = e.`contractNumber` left join `fcustomers` d ON e.`contractNumber` = d.`contractNumber` LEFT JOIN `icontracts` i ON i.`contractNumber` = e.`contractNumber` WHERE e.`ServiceID` <> '' ";
            if ( chkUseDates.Checked )
            {
                string date1 = this.dateTimePicker1.Value.ToString("yyyy-MM-dd");
                string date2 = this.dateTimePicker2.Value.ToString("yyyy-MM-dd");
                if ( chkDeceasedDate.Checked )
                    cmd += " AND p.`deceasedDate` >= '" + date1 + "' AND p.`deceasedDate` <= '" + date2 + "' ";
                else
                    cmd += " AND e.`serviceDate` >= '" + date1 + "' AND e.`serviceDate` <= '" + date2 + "' ";
            }

            //cmd += " AND e.`serviceDate` >= '2015-01-01' ";
            //string locations = getLocationQuery();
            //if (!String.IsNullOrWhiteSpace(locations))
            //    cmd += " AND " + locations;
            cmd += " ORDER BY e.`serviceDate` DESC ";
            cmd += ";";

            dt = G1.get_db_data(cmd);

            PreProcessData(dt);

            LoadFuneralLocations(dt);

            dt.Columns.Add("num");
            dt.Columns.Add("agreement");
            dt.Columns.Add("paid", Type.GetType("System.Double"));
            dt.Columns.Add("purchase", Type.GetType("System.Double"));
            dt.Columns.Add("contractValue", Type.GetType("System.Double"));

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
        private void PreProcessData ( DataTable dt )
        {
            string contractNumber = "";
            string lastName = "";
            string firstName = "";
            if (G1.get_column_number(dt, "firstName1") < 0)
                return;
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                if ( DailyHistory.isInsurance ( contractNumber))
                {
                    firstName = dt.Rows[i]["firstName1"].ObjToString();
                    lastName = dt.Rows[i]["lastName1"].ObjToString();
                    dt.Rows[i]["firstName"] = firstName;
                    dt.Rows[i]["lastName"] = lastName;
                }
            }
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
            double contractValue = 0D;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                serviceTotal = dt.Rows[i]["serviceTotal"].ObjToDouble();
                merchandiseTotal = dt.Rows[i]["merchandiseTotal"].ObjToDouble();
                balanceDue = dt.Rows[i]["balanceDue"].ObjToDouble();
                totalPurchase = serviceTotal + merchandiseTotal;
                paid = totalPurchase - balanceDue;
                dt.Rows[i]["paid"] = paid;
                dt.Rows[i]["purchase"] = totalPurchase;
                dt.Rows[i]["contractValue"] = DailyHistory.GetContractValue(dt.Rows[i]);
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
            G1.ShowHideFindPanel(gridMain);
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
                filename = dt.Rows[i]["picRecord"].ObjToString();
                if (!String.IsNullOrWhiteSpace(filename))
                    dt.Rows[i]["agreement"] = "1";
                else
                    dt.Rows[i]["agreement"] = "0";
            }
        }
        /***********************************************************************************************/
        private void repCheckEdit1_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            DataTable dt = (DataTable)(dgv.DataSource);
            DataRow dr = gridMain.GetFocusedDataRow();
            string contract = dr["contractNumber"].ObjToString();
            string value = dr["agreement"].ObjToString();
            string record = dr["picRecord"].ObjToString();
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
                    if (!String.IsNullOrWhiteSpace(record))
                    {
                        if (!String.IsNullOrWhiteSpace ( record))
                            Customers.ShowPDfImage(record, title, filename);
                    }
                }
            }
            this.Cursor = Cursors.Default;
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

            gridMain.Columns["agreement"].Visible = false;
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
            gridMain.Columns["agreement"].Visible = true;
        }
        /***********************************************************************************************/
        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.components == null)
                this.components = new System.ComponentModel.Container();

            gridMain.Columns["agreement"].Visible = false;
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
            gridMain.Columns["agreement"].Visible = true;
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
            Printer.DrawQuad(6, 8, 4, 4, "Funeral Services Report", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            //            Printer.DrawQuadTicks();
            DateTime date = DateTime.Now;
            string workDate = date.Month.ToString("D2") + "/" + date.Year.ToString("D4");
            Printer.SetQuadSize(24, 12);
            font = new Font("Ariel", 9, FontStyle.Regular);
            //Printer.DrawQuad(20, 8, 5, 4, "Report Month:" + workDate, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(16, 8, 3, 4, lblPayment.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(19, 8, 3, 4, lblTrust85.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(22, 8, 3, 4, lblTrust100.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /***********************************************************************************************/
        private void pictureAdd_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            using (NewContract contractForm = new NewContract("Funeral"))
            {
                contractForm.SelectDone += ContractForm_SelectDone;
                contractForm.ShowDialog();
            }
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void ContractForm_SelectDone(string contract)
        {
            if (String.IsNullOrWhiteSpace(contract))
                return;
            this.Cursor = Cursors.WaitCursor;

            string cmd = "SELECT *, (SELECT `pdfimages`.`record` FROM `pdfimages` WHERE `pdfimages`.`contractNumber` = e.`contractNumber` ) AS `picRecord` FROM `cust_extended` e LEFT JOIN `contracts` p ON p.`contractNumber` = e.`contractNumber` left join `customers` d ON e.`contractNumber` = d.`contractNumber` LEFT JOIN `icontracts` i ON i.`contractNumber` = e.`contractNumber` LEFT JOIN `icustomers` a ON a.`contractNumber` = e.`contractNumber` WHERE e.`ServiceID` <> '' ";
            cmd += " AND p.`contractNumber` = '" + contract + "' ";
            cmd += " ORDER BY e.`serviceDate` DESC ";
            cmd += ";";

            DataTable dt = G1.get_db_data(cmd);

            PreProcessData(dt);

            LoadFuneralLocations(dt);

            dt.Columns.Add("num");
            dt.Columns.Add("agreement");
            dt.Columns.Add("paid", Type.GetType("System.Double"));
            dt.Columns.Add("purchase", Type.GetType("System.Double"));
            dt.Columns.Add("contractValue", Type.GetType("System.Double"));

            SetupAgreementIcon(dt);
            CalcPaid(dt);
            DetermineLapsed(dt);

            DataTable dx = originalDt;
            if ( dx == null )
                dx = (DataTable)dgv.DataSource;
            if (dx == null)
                return;
            dx.ImportRow(dt.Rows[0]);
            G1.NumberDataTable(dx);
            if (originalDt != null)
                originalDt = dx;
            dgv.DataSource = dx;

            if (!String.IsNullOrWhiteSpace(chkComboLocation.Text))
                chkComboLocation_EditValueChanged(null, null);

            dx = (DataTable)dgv.DataSource;
            int row = dx.Rows.Count - 1;
            gridMain.FocusedRowHandle = row;
            gridMain.SelectRow(row);
            gridMain.RefreshEditor(true);
            dgv.RefreshDataSource();
            gridMain.RefreshData();
            dgv.Refresh();


            //EditCust custForm = new EditCust(contract);
            //custForm.Show();
            //this.Cursor = Cursors.Default;
            //this.TopMost = false;
            //custForm.TopMost = true;
            //custForm.BringToFront();

            //CustomerDetails custForm = new CustomerDetails(contract);
            //custForm.Show();
        }
        /***********************************************************************************************/
        private void gridMain_DoubleClick_2(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string contract = dr["contractNumber"].ObjToString();
            if (!String.IsNullOrWhiteSpace(contract))
            {
                this.Cursor = Cursors.WaitCursor;
                EditCust custForm = new EditCust(contract);
                custForm.custClosing += CustForm_custClosing;
                custForm.Show();
                this.Cursor = Cursors.Default;
            }
        }
        /***********************************************************************************************/
        private void CustForm_custClosing(string contractNumber, double amountFiled, double amountReceived)
        {
            string extendedRecord = "";
            string record = "";
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt == null)
                return;
            string cmd = "SELECT *, (SELECT `pdfimages`.`record` FROM `pdfimages` WHERE `pdfimages`.`contractNumber` = e.`contractNumber` ) AS `picRecord` FROM `fcust_extended` e LEFT JOIN `fcontracts` p ON p.`contractNumber` = e.`contractNumber` left join `fcustomers` d ON e.`contractNumber` = d.`contractNumber` WHERE e.`contractNumber` = '" + contractNumber + "';";
            //cmd += " AND e.`record` = '" + record + "';";

            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                extendedRecord = dx.Rows[0]["record"].ObjToString();
                record = extendedRecord;
                //G1.NumberDataTable(dt);
                //SetupAgreementIcon(dt);
                //CalcPaid(dt);
                //DetermineLapsed(dt);
                //if (oDt == null)
                //    oDt = dt.Copy();
                //originalDt = dt.Copy();
                //dgv.DataSource = dt;


                string mRecord = "";
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    mRecord = dt.Rows[i]["record"].ObjToString();
                    if (mRecord == record)
                    {
                        G1.HardCopyDtRow(dx, 0, dt, i);
                        break;
                    }
                }
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

            if (DailyHistory.isInsurance(contract))
            {
                contractsFile = "icontracts";
                customersFile = "icustomers";
                paymentsFile = "ipayments";
                insurance = true;
            }

            DialogResult result = MessageBox.Show("***Question*** Are you sure you want to DELETE this Funeral Contract (" + contract + ") ?", "Delete Funeral Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
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
                G1.get_db_data("Delete from `cust_extended` where `contractNumber` = '" + contract + "';");
            }

            dt = (DataTable)dgv.DataSource;
            int row = gridMain.GetDataSourceRowIndex(gridMain.FocusedRowHandle);
            gridMain.DeleteRow(gridMain.FocusedRowHandle);
            dt.Rows.RemoveAt(row);
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;

            //if ( insurance )
            //    G1.AddToAudit(LoginForm.username, "Customers", "Delete Insurance Customer", "Deleted", contract);
            //else
            //    G1.AddToAudit(LoginForm.username, "Customers", "Delete Trust Customer", "Deleted", contract);
        }
        /***********************************************************************************************/
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
                        e.DisplayText = date.ToString("MM/dd/yyyy");
                    else
                        e.DisplayText = "";
                }
            }
        }
        /***********************************************************************************************/
        private void SetupSelectedColumns(string procType, string group, DevExpress.XtraGrid.GridControl dgv)
        {
            if (String.IsNullOrWhiteSpace(group))
                return;
            if (String.IsNullOrWhiteSpace(procType))
                procType = "Funerals";
            string cmd = "Select * from procfiles where name = '" + group + "' and ProcType = '" + procType + "' order by seq";
            DataTable dt = G1.get_db_data(cmd);
            DevExpress.XtraGrid.Views.Grid.GridView gridMain = (DevExpress.XtraGrid.Views.Grid.GridView) dgv.MainView;
            for (int i = 0; i < gridMain.Columns.Count; i++)
                gridMain.Columns[i].Visible = false;
            DataTable ddx = (DataTable)dgv.DataSource;
            int idx = 0;
            string name = "";
            int index = 0;
            for (int i = (dt.Rows.Count-1); i>=0; i--)
            {
                name = dt.Rows[i]["Description"].ToString();
                index = dt.Rows[i]["seq"].ObjToInt32();
                try
                {
                    idx = G1.get_column_number(gridMain, name);
                    if (idx >= 0)
                        gridMain.Columns[name].Visible = true;
                }
                catch
                {
                }
            }
        }
        /***********************************************************************************************/
        private void cmbSelectColumns_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            ComboBox combo = (ComboBox)sender;
            string comboName = combo.Text;
            string skinName = "";
            if (!String.IsNullOrWhiteSpace(comboName))
            {
                SetupSelectedColumns("Funerals", comboName, dgv);
                string name = "Funerals " + comboName;
                foundLocalPreference = G1.RestoreGridLayout(this, this.dgv, gridMain, LoginForm.username, name, ref skinName);
                gridMain.OptionsView.ShowFooter = true;
                SetupTotalsSummary();
            }
            else
            {
                SetupSelectedColumns("Funerals", "Primary", dgv);
                string name = "Funerals Primary";
                foundLocalPreference = G1.RestoreGridLayout(this, this.dgv, gridMain, LoginForm.username, name, ref skinName);
                gridMain.OptionsView.ShowFooter = true;
                SetupTotalsSummary();
            }
        }
        /***********************************************************************************************/
        void sform_Done( DataTable dt )
        {
            string name = cmbSelectColumns.Text.Trim();
            if (String.IsNullOrWhiteSpace(name))
                name = "Primary";
            string saveName = "Funerals " + name;
            string skinName = "";
            SetupSelectedColumns("Funerals", name, dgv);
            foundLocalPreference = G1.RestoreGridLayout(this, this.dgv, gridMain, LoginForm.username, saveName, ref skinName);
            gridMain.OptionsView.ShowFooter = true;
            SetupTotalsSummary();
            string field = "";
            string select = "";
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                field = dt.Rows[i]["field"].ObjToString();
                select = dt.Rows[i]["select"].ObjToString();
                if ( G1.get_column_number ( gridMain, field) >= 0 )
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
        private void btnSelectColumns_Click_1(object sender, EventArgs e)
        {
            string actualName = cmbSelectColumns.Text;
//            SelectColumns sform = new SelectColumns(dgv, "Funerals", "Primary", actualName);
            SelectDisplayColumns sform = new SelectDisplayColumns(dgv, "Funerals", "Primary", actualName);
            sform.Done += new SelectDisplayColumns.d_void_selectionDone(sform_Done);
            sform.Show();
        }
        /***********************************************************************************************/
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            string name = cmbSelectColumns.Text.Trim();
            if (String.IsNullOrWhiteSpace(name))
                name = "Primary";
            string saveName = "Funerals " + name;
            G1.SaveLocalPreferences(this, gridMain, LoginForm.username, saveName);
        }
        /***********************************************************************************************/
        private void gridMain_CustomSummaryCalculate(object sender, DevExpress.Data.CustomSummaryEventArgs e)
        {
            string field = (e.Item as GridSummaryItem).FieldName.ObjToString();
            if ( field.ToUpper() == "CASH")
            {
                DataTable dt = (DataTable)dgv.DataSource;
                string str = "";
                double cash = 0D;
                for ( int i=0; i<dt.Rows.Count; i++)
                {
                    str = dt.Rows[i]["cash"].ObjToString();
                    str = str.Replace("CA - ", "");
                    if ( !String.IsNullOrWhiteSpace ( str))
                    {
                        string[] Lines = str.Split(' ');
                        for ( int j=0; j<Lines.Length; j++)
                        {
                            str = Lines[j].Trim();
                            if (G1.validate_numeric(str))
                                cash += str.ObjToDouble();
                        }
                    }
                }
                e.TotalValue = cash;
            }
            else if (field.ToUpper() == "CREDITCARD")
            {
                DataTable dt = (DataTable)dgv.DataSource;
                string str = "";
                double cash = 0D;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    str = dt.Rows[i]["creditCard"].ObjToString();
                    str = str.Replace("CC - ", "");
                    if (!String.IsNullOrWhiteSpace(str))
                    {
                        string[] Lines = str.Split(' ');
                        for (int j = 0; j < Lines.Length; j++)
                        {
                            str = Lines[j].Trim();
                            if (G1.validate_numeric(str))
                                cash += str.ObjToDouble();
                        }
                    }
                }
                e.TotalValue = cash;
            }
        }
        /*******************************************************************************************/
        private DataTable funeralsDt = null;
        private void LoadFuneralLocations(DataTable dt)
        {
            if (funeralsDt == null)
                funeralsDt = G1.get_db_data("Select * from `funeralhomes`;");
            string contract = "";
            string trust = "";
            string loc = "";
            DataRow[] dR = null;
            if (G1.get_column_number(dt, "loc") < 0)
                dt.Columns.Add("loc");
            if (G1.get_column_number(dt, "location") < 0)
                dt.Columns.Add("location");

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    contract = dt.Rows[i]["serviceId"].ObjToString();
                    contract = Trust85.decodeContractNumber(contract, ref trust, ref loc);

                    if (String.IsNullOrWhiteSpace(loc))
                        continue;
                    dR = funeralsDt.Select("keycode='" + loc + "'");
                    if ( dR.Length > 0 )
                    {
                        dt.Rows[i]["loc"] = loc;
                        dt.Rows[i]["location"] = dR[0]["LocationCode"].ObjToString();
                    }
                    else
                    {
                        dt.Rows[i]["loc"] = loc;
                        dt.Rows[i]["location"] = loc;
                    }
                }
                catch (Exception ex)
                {

                }
            }
        }
        /***********************************************************************************************/
        private void chkSort_CheckedChanged(object sender, EventArgs e)
        {
            if (dgv.DataSource == null)
                return;
            DataTable dt = (DataTable)dgv.DataSource;
            if (chkSort.Checked)
            {
                DataView tempview = dt.DefaultView;
                tempview.Sort = "location, lastName, firstName";
                dt = tempview.ToTable();
                G1.NumberDataTable(dt);
                dgv.DataSource = dt;

                gridMain.Columns["location"].GroupIndex = 0;
                this.gridMain.ExpandAllGroups();
            }
            else
            {
                gridMain.Columns["location"].GroupIndex = -1;
                gridMain.CollapseAllGroups();
            }
            gridMain.RefreshData();
            dgv.Refresh();
        }
        /***********************************************************************************************/
        private void chkComboLocation_EditValueChanged(object sender, EventArgs e)
        {
            if (originalDt == null)
                return;
            string names = getLocationQuery();
            DataRow[] dRows = originalDt.Select(names);
            DataTable dt = originalDt.Clone();
            for (int i = 0; i < dRows.Length; i++)
                dt.ImportRow(dRows[i]);
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
        }
        /***********************************************************************************************/
        private void latestChangesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            FuneralsChanges fForm = new FuneralsChanges();
            fForm.Show();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
    }
}