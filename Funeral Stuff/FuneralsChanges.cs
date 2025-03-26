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
using DevExpress.Utils.Serializing;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class FuneralsChanges : DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        private DataTable originalDt = null;
        private DataTable oDt = null;
        private bool loading = false;
        private bool insurance = false;
        private bool foundLocalPreference = false;
        private GridView originalGridView = null;
        private bool RunSinceLast = false;
        private string workContract = "";
        /***********************************************************************************************/
        public FuneralsChanges( string contract = "" )
        {
            InitializeComponent();
            workContract = contract;
        }
        /***********************************************************************************************/
        private void FuneralsChanges_Load(object sender, EventArgs e)
        {
            loading = true;

            lblTotal.Hide();
            barImport.Hide();

            RunSinceLast = false;

            loadLastRun();

            loadLocatons();
            G1.loadGroupCombo(cmbSelectColumns, "FuneralsChanges", "Primary", true);
            cmbSelectColumns.Text = "Primary";

            loading = false;

            if ( !String.IsNullOrWhiteSpace ( workContract ))
                LoadData();
        }
        /***********************************************************************************************/
        private void loadLastRun()
        {
            cmbLastRunList.Items.Clear();
            //if (1 == 1)
            //    return;
            DateTime date = DateTime.Now;
            string cmd = "Select * from `last_runs` where `user` = '" + LoginForm.username + "' ORDER BY `lastRun` DESC;";
            DataTable dt = G1.get_db_data(cmd);
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                if (i > 30)
                    break;
                date = dt.Rows[i]["lastRun"].ObjToDateTime();
                cmbLastRunList.Items.Add(date.ToString("yyyy-MM-dd HH:mm:ss"));
                if (i == 0)
                    cmbLastRunList.Text = date.ToString("yyyy-MM-dd HH:mm:ss");
            }
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
            this.Cursor = Cursors.WaitCursor;

            string cmd = "";
            DataTable dt = null;
            DataTable dx = null;
            DataRow dRow = null;

            DateTime date = DateTime.Now;
            string what = "";
            string type = "";
            string loc = "";
            string trust = "";
            string serviceId = "";

            if (!String.IsNullOrWhiteSpace(workContract))
            {
                dt = new DataTable();

                dt.Columns.Add("num");
                dt.Columns.Add("contractNumber");
                dt.Columns.Add("caseCreatedDate");
                dt.Columns.Add("dateChanged");
                dt.Columns.Add("whatChanged");
                dt.Columns.Add("whoChanged");
                dt.Columns.Add("lastName");
                dt.Columns.Add("firstName");
                dt.Columns.Add("Funeral Arranger");
                dt.Columns.Add("Funeral Director");
                dt.Columns.Add("serviceId");
                dt.Columns.Add("serviceDate");
                dt.Columns.Add("deceasedDate");
                dt.Columns.Add("loc");
                dt.Columns.Add("action");
                dt.Columns.Add("what");
                dt.Columns.Add("type");


                cmd = "Select * from `fcust_changes` y JOIN `fcustomers` f ON y.`contractNumber` = f.`contractNumber` JOIN `fcontracts` c ON y.`contractNumber` = c.`contractNumber` JOIN `fcust_extended` x ON y.`contractNumber` = x.`contractNumber` WHERE y.`contractNumber` = '" + workContract + "';";


                //cmd = "Select * from `fcust_changes` where `contractNumber` = '" + workContract + "' ORDER by `date` DESC;";
                dx = G1.get_db_data(cmd);
                for ( int i=0; i<dx.Rows.Count; i++)
                {
                    date = dx.Rows[i]["date"].ObjToDateTime();
                    type = dx.Rows[i]["type"].ObjToString();
                    dRow = dt.NewRow();
                    dRow["contractNumber"] = workContract;
                    //dRow["whatChanged"] = type + " " + dx.Rows[i]["what"].ObjToString();
                    dRow["dateChanged"] = date.ToString("yyyy-MM-dd HH:mm:ss");
                    dRow["whoChanged"] = dx.Rows[i]["user"].ObjToString();
                    dRow["action"] = dx.Rows[i]["action"].ObjToString();
                    dRow["what"] = dx.Rows[i]["what"].ObjToString();
                    dRow["type"] = dx.Rows[i]["type"].ObjToString();

                    dRow["firstName"] = dx.Rows[i]["firstName"].ObjToString();
                    dRow["lastName"] = dx.Rows[i]["lastName"].ObjToString();
                    dRow["serviceId"] = dx.Rows[i]["serviceId"].ObjToString();
                    dRow["serviceDate"] = dx.Rows[i]["serviceDate"].ObjToDateTime().ToString("MM/dd/yyyy");
                    dRow["deceasedDate"] = dx.Rows[i]["deceasedDate"].ObjToDateTime().ToString("MM/dd/yyyy");

                    dRow["caseCreatedDate"] = dx.Rows[i]["caseCreatedDate"].ObjToString();

                    serviceId = dx.Rows[0]["serviceId"].ObjToString();


                    Trust85.decodeContractNumber(serviceId, ref trust, ref loc);
                    dRow["loc"] = loc;

                    dt.Rows.Add(dRow);

                }

                gridMain.Columns["Funeral Director"].Visible = false;
                gridMain.Columns["Funeral Arranger"].Visible = false;
                gridMain.Columns["whatChanged"].Visible = false;
                gridMain.Columns["select"].Visible = false;
                gridMain.Columns["what"].Visible = true;
                gridMain.Columns["type"].Visible = true;
                gridMain.Columns["action"].Visible = true;

                SetupSelection(dt);

                G1.NumberDataTable(dt);

                originalDt = dt.Copy();
                dgv.DataSource = dt;
                RunSinceLast = false;
                this.Cursor = Cursors.Default;
                return;
            }


            string date1 = this.dateTimePicker1.Value.ToString("yyyy-MM-dd") + " 00:00:00";
            string date2 = this.dateTimePicker2.Value.ToString("yyyy-MM-dd") + " 23:59:59";

            string date8 = this.dateTimePicker1.Value.ToString("yyyy-MM-dd");
            string date9 = this.dateTimePicker2.Value.ToString("yyyy-MM-dd");

            if ( RunSinceLast )
            {
                date1 = cmbLastRunList.Text;
                if ( String.IsNullOrWhiteSpace ( date1 ))
                    date1 = this.dateTimePicker1.Value.ToString("yyyy-MM-dd") + " 00:00:00";
                date2 = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                DateTime dateXXX = date1.ObjToDateTime();
                date8 = dateXXX.ToString("yyyy-MM-dd");
                date9 = DateTime.Now.ToString("yyyy-MM-dd");
            }

            cmd = "Select * from `fcust_services` ";
            if (!String.IsNullOrWhiteSpace(workContract))
                cmd += " WHERE `contractNumber` = '" + workContract + "' ";
            else
                cmd += " WHERE (`tmstamp` >= '" + date1 + "' AND `tmstamp` <= '" + date2 + "' ) ";
            cmd += ";";

            DataTable dt1 = G1.get_db_data(cmd);

            cmd = "Select * from `cust_payments` ";
            if (!String.IsNullOrWhiteSpace(workContract))
                cmd += " WHERE `contractNumber` = '" + workContract + "' ";
            else
                cmd += " WHERE (`tmstamp` >= '" + date1 + "' AND `tmstamp` <= '" + date2 + "' ) ";
            cmd += ";";

            DataTable dt2 = G1.get_db_data(cmd);


            cmd = "Select * from `fcust_extended` ";
            if (!String.IsNullOrWhiteSpace(workContract))
                cmd += " WHERE `contractNumber` = '" + workContract + "' ";
            else
                cmd += " WHERE (`caseCreatedDate` >= '" + date8 + "' AND `caseCreatedDate` <= '" + date9 + "' ) ";
            cmd += ";";

            DataTable dt3 = G1.get_db_data(cmd);
            //PreProcessData(dt);

            //LoadFuneralLocations(dt);

            dt = new DataTable();

            dt.Columns.Add("num");
            dt.Columns.Add("contractNumber");
            dt.Columns.Add("caseCreatedDate");
            dt.Columns.Add("dateChanged");
            dt.Columns.Add("whatChanged");
            dt.Columns.Add("whoChanged");
            dt.Columns.Add("lastName");
            dt.Columns.Add("firstName");
            dt.Columns.Add("Funeral Arranger");
            dt.Columns.Add("Funeral Director");
            dt.Columns.Add("serviceId");
            dt.Columns.Add("serviceDate");
            dt.Columns.Add("deceasedDate");
            dt.Columns.Add("loc");

            string contractNumber = "";

            dRow = null;

            DataTable ddx = null;

            DateTime date3 = DateTime.Now;

            for ( int i=0; i<dt1.Rows.Count; i++)
            {
                contractNumber = dt1.Rows[i]["contractNumber"].ObjToString();
                date = dt1.Rows[i]["tmstamp"].ObjToDateTime();
                type = dt1.Rows[i]["type"].ObjToString();
                dRow = dt.NewRow();
                dRow["contractNumber"] = contractNumber;
                dRow["whatChanged"] = type;
                dRow["dateChanged"] = date.ToString("yyyy-MM-dd HH:mm:ss");
                dRow["whoChanged"] = dt1.Rows[i]["who"].ObjToString();

                cmd = "Select * from `fcust_extended` where `contractNumber` = '" + contractNumber + "';";
                ddx = G1.get_db_data(cmd);
                if ( ddx.Rows.Count > 0 )
                    dRow["caseCreatedDate"] = ddx.Rows[0]["caseCreatedDate"].ObjToString();

                date3 = date.AddMinutes(1);


                cmd = "Select * from `fcust_changes` where `contractNumber` = '" + contractNumber + "' AND `type` = '" + type + "' AND `date` >= '" + date.ToString("yyyy-MM-dd HH:mm") + "' AND `date` <= '" + date3.ToString("yyyy-MM-dd HH:mm") + "';";
                ddx = G1.get_db_data(cmd);
                if (ddx.Rows.Count > 0)
                    dRow["whoChanged"] = ddx.Rows[0]["user"].ObjToString();
                dt.Rows.Add(dRow);
            }
            for (int i = 0; i < dt2.Rows.Count; i++)
            {
                contractNumber = dt2.Rows[i]["contractNumber"].ObjToString();
                date = dt2.Rows[i]["tmstamp"].ObjToDateTime();
                dRow = dt.NewRow();
                dRow["contractNumber"] = contractNumber;
                dRow["whatChanged"] = "Payment";
                dRow["dateChanged"] = date.ToString("yyyy-MM-dd HH:mm:ss");

                cmd = "Select * from `fcust_extended` where `contractNumber` = '" + contractNumber + "';";
                ddx = G1.get_db_data(cmd);
                if (ddx.Rows.Count > 0)
                    dRow["caseCreatedDate"] = ddx.Rows[0]["caseCreatedDate"].ObjToString();

                date3 = date.AddMinutes(1);

                cmd = "Select * from `fcust_changes` where `contractNumber` = '" + contractNumber + "' AND `type` = 'Payment' AND `date` >= '" + date.ToString("yyyy-MM-dd HH:mm") + "' AND `date` <= '" + date3.ToString("yyyy-MM-dd HH:mm") + "';";
                ddx = G1.get_db_data(cmd);
                if (ddx.Rows.Count > 0)
                    dRow["whoChanged"] = ddx.Rows[0]["user"].ObjToString();
                dt.Rows.Add(dRow);
            }

            DataRow[] dRows = null;

            for (int i = 0; i < dt3.Rows.Count; i++)
            {
                contractNumber = dt3.Rows[i]["contractNumber"].ObjToString();

                dRows = dt1.Select("contractNumber='" + contractNumber + "'");
                if ( dRows.Length <= 0 )
                {
                    date = dt3.Rows[i]["caseCreatedDate"].ObjToDateTime();
                    dRow = dt.NewRow();
                    dRow["contractNumber"] = contractNumber;
                    dRow["whatChanged"] = "New Funeral";
                    dRow["dateChanged"] = date.ToString("yyyy-MM-dd HH:mm:ss");
                    dRow["caseCreatedDate"] = date;
                    dt.Rows.Add(dRow);
                }
            }
            DataView tempview = dt.DefaultView;
            tempview.Sort = "contractNumber, dateChanged";
            dt = tempview.ToTable();

            if (dt.Rows.Count > 0 && !chkDetail.Checked )
            {
                DataTable groupDt = dt.AsEnumerable().GroupBy(r => new { Col1 = r["contractNumber"] }).Select(g => g.OrderBy(r => r["dateChanged"]).Last()).CopyToDataTable();

                dt = groupDt.Copy();
            }

            dx = null;
            ddx = null;

            loc = "";
            trust = "";
            serviceId = "";
            string payer = "";
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                try
                {
                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                    cmd = "Select * from `fcustomers` f JOIN `fcontracts` c ON f.`contractNumber` = c.`contractNumber` JOIN `fcust_extended` x ON f.`contractNumber` = x.`contractNumber` WHERE f.`contractNumber` = '" + contractNumber + "';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count <= 0)
                        continue;
                    dt.Rows[i]["firstName"] = dx.Rows[0]["firstName"].ObjToString();
                    dt.Rows[i]["lastName"] = dx.Rows[0]["lastName"].ObjToString();
                    dt.Rows[i]["serviceId"] = dx.Rows[0]["serviceId"].ObjToString();
                    dt.Rows[i]["serviceDate"] = dx.Rows[0]["serviceDate"].ObjToDateTime().ToString("MM/dd/yyyy");
                    dt.Rows[i]["deceasedDate"] = dx.Rows[0]["deceasedDate"].ObjToDateTime().ToString("MM/dd/yyyy");

                    dt.Rows[i]["Funeral Arranger"] = dx.Rows[0]["Funeral Arranger"].ObjToString();
                    dt.Rows[i]["Funeral Director"] = dx.Rows[0]["Funeral Director"].ObjToString();

                    serviceId = dx.Rows[0]["serviceId"].ObjToString();

                    Trust85.decodeContractNumber(serviceId, ref trust, ref loc);
                    dt.Rows[i]["loc"] = loc;
                    if ( DailyHistory.isInsurance ( contractNumber))
                    {
                        cmd = "Select * from `icustomers` where `contractNumber` = '" + contractNumber + "';";
                        ddx = G1.get_db_data(cmd);
                        if ( ddx.Rows.Count > 0 )
                        {
                            payer = ddx.Rows[0]["payer"].ObjToString();
                            if (!String.IsNullOrWhiteSpace(payer))
                                dt.Rows[i]["contractNumber"] = payer;
                        }
                    }
                }
                catch ( Exception ex)
                {
                }
            }

            SetupSelection(dt);

            G1.NumberDataTable(dt);

            originalDt = dt.Copy();
            dgv.DataSource = dt;
            RunSinceLast = false;
            this.Cursor = Cursors.Default;
            if ( dt.Rows.Count <= 0 )
            {
                MessageBox.Show("***INFORMATION*** Nothing NEW to Report!", "Report Changes Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            }
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
                tempview.Sort = "loc, lastName, firstName";
                dt = tempview.ToTable();
                G1.NumberDataTable(dt);
                dgv.DataSource = dt;

                gridMain.Columns["loc"].GroupIndex = 0;
                this.gridMain.ExpandAllGroups();
            }
            else
            {
                gridMain.Columns["loc"].GroupIndex = -1;
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
        private void SetupSelection(DataTable dt)
        {
            if (G1.get_column_number(dt, "select") < 0)
                dt.Columns.Add("select");
            DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew = this.repositoryItemCheckEdit1;
            selectnew.NullText = "";
            selectnew.ValueChecked = "1";
            selectnew.ValueUnchecked = "0";
            selectnew.ValueGrayed = "";
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["select"] = "0";
        }
        /***********************************************************************************************/
        private void repositoryItemCheckEdit1_Click(object sender, EventArgs e)
        {
            if (loading)
                return;
            loading = true;
            DataRow dr = gridMain.GetFocusedDataRow();
            string x = dr["select"].ObjToString();
            if (x == "1")
                x = "0";
            else
                x = "1";
            dr["select"] = x;
            DataTable dt = (DataTable)dgv.DataSource;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            dt.Rows[row]["select"] = x;
            dt.AcceptChanges();
            loading = false;
            gridMain.RefreshEditor(true);
        }
        /***********************************************************************************************/
        private void btnAllOn_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            for ( int i=0; i<dt.Rows.Count; i++)
                dt.Rows[i]["select"] = "1";
            dgv.DataSource = dt;
            gridMain.RefreshEditor(true);
        }
        /***********************************************************************************************/
        private void btnAllOff_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["select"] = "0";
            dgv.DataSource = dt;
            gridMain.RefreshEditor(true);
        }
        /***********************************************************************************************/
        private void btnPrintSelected_Click(object sender, EventArgs e)
        {
            PrintSelected(false);
        }
        /***********************************************************************************************/
        private void btnPrintAll_Click(object sender, EventArgs e)
        {
            PrintSelected(true);
        }
        /***********************************************************************************************/
        private void PrintSelected ( bool actuallyPrint = false )
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt == null)
                return;

            DataTable dx = dt.Clone();
            DataRow[] dRows = dt.Select("select='1'");

            if (dRows.Length <= 0)
                return;

            dx = dRows.CopyToDataTable();

            DataTable groupDt = dx.AsEnumerable().GroupBy(r => new { Col1 = r["contractNumber"] }).Select(g => g.OrderBy(r => r["dateChanged"]).Last()).CopyToDataTable();
            dx = groupDt.Copy();

            int lastRow = dx.Rows.Count;

            barImport.Minimum = 0;
            barImport.Maximum = lastRow;
            barImport.Value = 0;

            lblTotal.Text = "0 of " + lastRow.ToString();
            lblTotal.Show();
            lblTotal.Refresh();

            barImport.Show();
            barImport.Refresh();

            DataTable ddx = null;
            string contractNumber = "";
            string payer = "";
            string select = "";
            string cmd = "";
            string serviceId = "";
            DataTable workPaymentsDt = null;
            DataTable workServicesDt = null;
            DataTable extendedDt = null;
            string serviceLoc = "";
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                this.Cursor = Cursors.WaitCursor;

                lblTotal.Text = (i + 1).ToString() + " of " + lastRow.ToString();
                lblTotal.Refresh();

                barImport.Value = i + 1;
                barImport.Refresh();

                if (workPaymentsDt != null)
                {
                    workPaymentsDt.Dispose();
                    workPaymentsDt = null;
                }
                if (workServicesDt != null)
                {
                    workServicesDt.Dispose();
                    workServicesDt = null;
                }
                select = dx.Rows[i]["select"].ObjToString();
                if (select == "1")
                {
                    contractNumber = dx.Rows[i]["contractNumber"].ObjToString();


                    cmd = "Select * from `fcust_extended` WHERE `contractNumber` = '" + contractNumber + "';";
                    extendedDt = G1.get_db_data(cmd);
                    if ( extendedDt.Rows.Count <= 0 )
                    {
                        cmd = "Select * from `icustomers` WHERE `payer` = '" + contractNumber + "';";
                        ddx = G1.get_db_data(cmd);
                        if (ddx.Rows.Count <= 0)
                            continue;
                        contractNumber = ddx.Rows[0]["contractNumber"].ObjToString();
                        cmd = "Select * from `fcust_extended` WHERE `contractNumber` = '" + contractNumber + "';";
                        extendedDt = G1.get_db_data(cmd);
                    }
                    serviceLoc = "";
                    if (extendedDt.Rows.Count > 0)
                    {
                        serviceLoc = extendedDt.Rows[0]["serviceLoc"].ObjToString();
                        if (String.IsNullOrWhiteSpace(serviceLoc))
                        {
                            MessageBox.Show("***ERROR*** Invalid Funeral Home Location for Contract " + contractNumber, "Service Location Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                            continue;
                        }
                        LoginForm.activeFuneralHomeKeyCode = serviceLoc;
                    }
                    extendedDt.Dispose();
                    extendedDt = null;

                    serviceId = EditCust.ConfirmServiceId(contractNumber);

                    EditCust.DetermineActiveGroups(contractNumber, serviceId );

                    string group = EditCust.activeFuneralHomeGroup;
                    string casketGroup = EditCust.activeFuneralHomeCasketGroup;
                    EditCustomer.activeFuneralHomeGroup = group;
                    EditCustomer.activeFuneralHomeCasketGroup = casketGroup;


                    this.Cursor = Cursors.WaitCursor;

                    cmd = "Select * from `cust_payments` where `contractNumber` = '" + contractNumber + "';";
                    workPaymentsDt = G1.get_db_data(cmd);
                    workPaymentsDt.Columns.Add("contractValue", Type.GetType("System.Double"));

                    this.Cursor = Cursors.WaitCursor;

                    FunServices editFunServices = new FunServices(contractNumber, true);
                    workServicesDt = editFunServices.FireEventFunServicesReturn();
                    editFunServices.Close();

                    Contract1 conActive = new Contract1(contractNumber, workServicesDt, workPaymentsDt, false, actuallyPrint, true );
                    conActive.Show();

                    this.Cursor = Cursors.WaitCursor;

                    if ( DailyHistory.isInsurance ( contractNumber))
                    {
                        Policies policyForm = new Policies(contractNumber, true );
                        policyForm.Show();
                    }

                    this.Cursor = Cursors.WaitCursor;
                }
            }
            if (workPaymentsDt != null)
            {
                workPaymentsDt.Dispose();
                workPaymentsDt = null;
            }
            if (workServicesDt != null)
            {
                workServicesDt.Dispose();
                workServicesDt = null;
            }

            lblTotal.Hide();
            barImport.Hide();

            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void btnLeft_Click(object sender, EventArgs e)
        {
            DateTime now = this.dateTimePicker1.Value;
            now = now.AddDays(-1);
            this.dateTimePicker1.Value = now;

            now = this.dateTimePicker2.Value;
            now = now.AddDays(-1);
            this.dateTimePicker2.Value = now;
        }
        /***********************************************************************************************/
        private void btnRight_Click(object sender, EventArgs e)
        {
            DateTime now = this.dateTimePicker1.Value;
            now = now.AddDays(1);
            this.dateTimePicker1.Value = now;

            now = this.dateTimePicker2.Value;
            now = now.AddDays(1);
            this.dateTimePicker2.Value = now;
        }
        /***********************************************************************************************/
        private void btnLastRun_Click(object sender, EventArgs e)
        {
            RunSinceLast = true;
            LoadData();
        }
        /***********************************************************************************************/
        private void FuneralsChanges_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(workContract))
                return;
            DialogResult result = MessageBox.Show("Would you like to save the time now as your last run time?", "Last Run Time Save Dialog", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            if (result == DialogResult.Cancel)
            {
                e.Cancel = true;
                return;
            }
            else if (result == DialogResult.No)
                return;
            else
            {
                DateTime now = DateTime.Now;
                string str = now.ToString("yyyy-MM-dd HH:mm:ss");
                string record = G1.create_record("last_runs", "user", "-1");
                if (G1.BadRecord("last_runs", record))
                    return;
                G1.update_db_table("last_runs", "record", record, new string[] { "user", LoginForm.username, "lastRun", str });
            }
        }
        /***********************************************************************************************/
        private void whatsChangedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowhandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowhandle);
            string contractNumber = dr["contractNumber"].ObjToString();

            ChangedContracts changedForm = new ChangedContracts(contractNumber);
            changedForm.Show();
        }
        /***********************************************************************************************/
    }
}