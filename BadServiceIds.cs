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
using System.Linq;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class BadServiceIds : DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        private DataTable originalDt = null;
        private DataTable oDt = null;
        private bool loading = false;
        private bool insurance = false;
        private bool foundLocalPreference = false;
        private GridView originalGridView = null;
        /***********************************************************************************************/
        public BadServiceIds()
        {
            InitializeComponent();
        }
        /***********************************************************************************************/
        private void BadServiceIds_Load(object sender, EventArgs e)
        {
            loading = true;
            loading = false;
        }
        /****************************************************************************************/
        private void SetupTotalsSummary()
        {
            if (1 == 1)
                return;
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
            Import importForm = new Import();
            importForm.SelectDone += ImportForm_SelectDone1;
            importForm.Show();

        }
        /***********************************************************************************************/
        private void ImportForm_SelectDone1(DataTable dx)
        {
            if (dx.Rows.Count <= 0)
                return;
            DataTable dt = new DataTable();

            dt.Columns.Add("num");
            dt.Columns.Add("contractNumber");
            dt.Columns.Add("lastName");
            dt.Columns.Add("firstName");
            dt.Columns.Add("serviceId");
            dt.Columns.Add("deceasedDate");
            dt.Columns.Add("ssn");
            dt.Columns.Add("serviceId_1");
            dt.Columns.Add("deceasedDate_1");

            string contractNumber = "";
            DateTime date = DateTime.Now;
            string what = "";

            string cmd = "";
            DataTable ddt = null;

            DataRow dRow = null;

            int lastRow = dx.Rows.Count;
            //lastRow = 5;

            for (int i = 0; i < lastRow; i++)
            {
                contractNumber = dx.Rows[i]["contractNumber"].ObjToString();
                cmd = "Select * from `customers` c JOIN `contracts` j ON c.`contractNumber` = j.`contractNumber` where c.`contractNumber` = '" + contractNumber + "';";
                ddt = G1.get_db_data(cmd);
                if ( ddt.Rows.Count == 0 )
                {
                    dRow = dt.NewRow();
                    dRow["contractNumber"] = contractNumber;
                    dRow["serviceId"] = "BAD";
                    dt.Rows.Add(dRow);
                    continue;
                }
                dRow = dt.NewRow();
                try
                {
                    dRow["contractNumber"] = contractNumber;
                    dRow["lastName"] = ddt.Rows[0]["lastName"].ObjToString();
                    dRow["firstName"] = ddt.Rows[0]["firstName"].ObjToString();
                    dRow["serviceId"] = ddt.Rows[0]["serviceId"].ObjToString();
                    dRow["deceasedDate"] = ddt.Rows[0]["deceasedDate"].ObjToDateTime().ToString("yyyy-MM-dd");
                    dRow["ssn"] = ddt.Rows[0]["ssn"].ObjToString();

                    dRow["serviceId_1"] = ddt.Rows[0]["serviceId1"].ObjToString();
                    dRow["deceasedDate_1"] = ddt.Rows[0]["deceasedDate1"].ObjToDateTime().ToString("yyyy-MM-dd");
                    dt.Rows.Add(dRow);
                }
                catch ( Exception ex)
                {
                }
            }

            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void LoadExisting ()
        {
            string contractNumber = "";
            DateTime date = DateTime.Now;
            string what = "";

            DataTable ddt = null;

            DataRow dRow = null;

            string cmd = "Select * from `customers` c JOIN `contracts` x ON c.`contractNumber` = x.`contractNumber` WHERE x.`deceasedDate` >= '2020-11-01 00:00:00' AND x.`ServiceId` <> '' AND (`ssn`='0' OR `ssn`='');";
            DataTable dt = G1.get_db_data(cmd);

            int count = dt.Rows.Count;

            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            G1.ShowHideFindPanel(gridMain);

            //if (gridMain.OptionsFind.AlwaysVisible == true)
            //    gridMain.OptionsFind.AlwaysVisible = false;
            //else
            //    gridMain.OptionsFind.AlwaysVisible = true;
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
            if (chkExisting.Checked)
                LoadExisting();
            else
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
            string saveName = "FuneralsChanged " + name;
            G1.SaveLocalPreferences(this, gridMain, LoginForm.username, saveName);
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
        private void gridMain_CustomColumnDisplayText(object sender, CustomColumnDisplayTextEventArgs e)
        {
            ColumnView view = sender as ColumnView;
            if (e.Column.FieldName.ToUpper() == "DATECHANGED")
                return;
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
        private void gridMain_ColumnFilterChanged(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            int count = dt.Rows.Count;
            gridMain.SelectAll();
            int[] rows = gridMain.GetSelectedRows();
            int row = 0;
            for (int i = 0; i < rows.Length; i++)
            {
                row = rows[i];
                var dRow = gridMain.GetDataRow(row);
                if (dRow != null)
                    dRow["num"] = (i + 1).ToString();
            }
            gridMain.ClearSelection();
        }
        /***********************************************************************************************/
        private void gridMain_CustomDrawCell(object sender, RowCellCustomDrawEventArgs e)
        {
            if (e.Column.FieldName.ToUpper() == "NUM")
            {
                if (e.RowHandle >= 0)
                {
                    string num = (e.RowHandle + 1).ToString();
                    e.DisplayText = num;
                    int row = gridMain.GetDataSourceRowIndex(e.RowHandle);
                    DataTable dt = (DataTable)dgv.DataSource;
                    dt.Rows[row]["num"] = num;
                }
            }
        }
        /***********************************************************************************************/
        private void btnRead_Click(object sender, EventArgs e)
        {
            Import importForm = new Import();
            importForm.SelectDone += ImportForm_SelectDone;
            importForm.Show();
        }
        /***********************************************************************************************/
        private void ImportForm_SelectDone(DataTable dx)
        {
            this.Cursor = Cursors.WaitCursor;

            DataTable dt = new DataTable();

            dt.Columns.Add("num");
            dt.Columns.Add("contractNumber");
            dt.Columns.Add("lastName");
            dt.Columns.Add("firstName");
            dt.Columns.Add("ServiceId");
            dt.Columns.Add("deceasedDate");
            dt.Columns.Add("ssn");
            dt.Columns.Add("serviceId_1");
            dt.Columns.Add("deceasedDate_1");

            string contractNumber = "";
            DateTime date = DateTime.Now;
            DataRow dRow = null;

            for ( int i=0; i<dx.Rows.Count; i++)
            {
                contractNumber = dx.Rows[i]["contractNumber"].ObjToString();
                if (contractNumber == "B0785")
                    continue;
                if (contractNumber == "CA048")
                    continue;
                if (contractNumber == "R145")
                    continue;
                dRow = dt.NewRow();
                dRow["contractNumber"] = contractNumber;
                dRow["lastName"] = dx.Rows[i]["Last Name"].ObjToString();
                dRow["firstName"] = dx.Rows[i]["First Name"].ObjToString();
                dRow["ServiceId"] = dx.Rows[i]["Contract SeviceId"].ObjToString();
                date = dx.Rows[i]["Contract Deceased Date"].ObjToDateTime();
                dRow["deceasedDate"] = date.ToString("yyyy-MM-dd");
                dt.Rows.Add(dRow);
            }

            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void btnFix_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            DataTable dx = (DataTable)dgv.DataSource;
            string contractNumber = "";
            DateTime date = DateTime.Now;
            string serviceId = "";
            string cmd = "";
            string record = "";
            DataTable dt = null;
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                contractNumber = dx.Rows[i]["contractNumber"].ObjToString();
                if (contractNumber == "B0785")
                    continue;
                if (contractNumber == "CA048")
                    continue;
                if (contractNumber == "R145")
                    continue;
                serviceId = dx.Rows[i]["ServiceId"].ObjToString();
                date = dx.Rows[i]["deceasedDate"].ObjToDateTime();

                cmd = "Select * from `contracts` where `contractNumber` = '" + contractNumber + "';";
                dt = G1.get_db_data(cmd);
                if ( dt.Rows.Count > 0 )
                {
                    record = dt.Rows[0]["record"].ObjToString();
                    G1.update_db_table("contracts", "record", record, new string[] {"ServiceID", serviceId, "deceasedDate", date.ToString("yyyy-MM-dd") });
                }

                cmd = "Select * from `customers` where `contractNumber` = '" + contractNumber + "';";
                dt = G1.get_db_data(cmd);
                if (dt.Rows.Count > 0)
                {
                    record = dt.Rows[0]["record"].ObjToString();
                    G1.update_db_table("customers", "record", record, new string[] { "ServiceID", serviceId, "deceasedDate", date.ToString("yyyy-MM-dd") });
                }
            }
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
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
    }
}