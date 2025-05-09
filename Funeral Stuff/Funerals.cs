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
//using Org.BouncyCastle.Bcpg.OpenPgp;
using DevExpress.XtraReports.UI;
using DevExpress.XtraEditors.Repository;
using System.Web.UI.WebControls;
using GridView = DevExpress.XtraGrid.Views.Grid.GridView;
using DevExpress.CodeParser;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraEditors.ViewInfo;
using DevExpress.Utils.Drawing;
using MySql.Data.MySqlClient;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class Funerals : DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        private PleaseWait pleaseForm = null;

        private DataTable originalDt = null;
        private DataTable oDt = null;
        private bool loading = false;
        private bool insurance = false;
        private bool foundLocalPreference = false;
        private GridView originalGridView = null;
        private bool showFooters = true;
        /***********************************************************************************************/
        public Funerals()
        {
            InitializeComponent();
        }
        /***********************************************************************************************/
        RepositoryItemDateEdit ri = null;
        private void Funerals_Load(object sender, EventArgs e)
        {
            if (LoginForm.username.ToUpper() != "ROBBY")
                this.recalcServicesToolStripMenuItem.Enabled = false;
            loading = true;
            if ( G1.oldCopy )
                menuStrip1.BackColor = Color.LightBlue;

            MatchTables();

            ri = new RepositoryItemDateEdit();
            ri.VistaEditTime = DevExpress.Utils.DefaultBoolean.True;
            ri.Mask.MaskType = DevExpress.XtraEditors.Mask.MaskType.DateTime;
            ri.Mask.UseMaskAsDisplayFormat = true;
            ri.Mask.EditMask = @"yyyy-MM-dd hh-mm";

            string name = G1.GetUserFullName();

            this.Text = "Funerals for User : " + name;

            showFooters = true;
            string preference = G1.getPreference(LoginForm.username, "Funerals CB Chooser", "Allow Access");
            if (preference != "YES")
                showFooters = false;

            loadLocatons();

            SetupTotalsSummary();

            G1.loadGroupCombo(cmbSelectColumns, "Funerals", "Primary", true, LoginForm.username);
            cmbSelectColumns.Text = "Original";
            //cmbSelectColumns.Text = "Primary";

            loading = false;
            //toolStripMenuItem1_Click(null, null);

            DateTime nowDate = DateTime.Now;
            this.dateTimePicker2.Value = nowDate;

            nowDate = nowDate.AddMonths(-3);
            this.dateTimePicker1.Value = nowDate;

            this.Refresh();
            gridMain.RefreshEditor(true);

            cmbSelectColumns_SelectedIndexChanged(cmbSelectColumns, null);

            gridMain.ShowCustomizationForm += GridMain_ShowCustomizationForm;


            if (showFooters)
            {
                this.gridMain.GroupSummary.AddRange(new DevExpress.XtraGrid.GridSummaryItem[]
                {
                new DevExpress.XtraGrid.GridGroupSummaryItem(DevExpress.Data.SummaryItemType.Sum, "balanceDue", this.balanceDue6, "${0:0,0.00}"),
                new DevExpress.XtraGrid.GridGroupSummaryItem(DevExpress.Data.SummaryItemType.Sum, "grossAmountReceived", this.grossAmountReceived12, "${0:0,0.00}"),
                new DevExpress.XtraGrid.GridGroupSummaryItem(DevExpress.Data.SummaryItemType.Sum, "amountDiscount", this.amountDiscount6, "${0:0,0.00}"),
                new DevExpress.XtraGrid.GridGroupSummaryItem(DevExpress.Data.SummaryItemType.Sum, "trustAmountFiled", this.trustAmountFiled2, "${0:0,0.00}"),
                new DevExpress.XtraGrid.GridGroupSummaryItem(DevExpress.Data.SummaryItemType.Sum, "trustAmountReceived", this.trustAmountReceived12, "${0:0,0.00}"),
                new DevExpress.XtraGrid.GridGroupSummaryItem(DevExpress.Data.SummaryItemType.Sum, "insAmountFiled", this.insAmountFiled6, "${0:0,0.00}"),
                new DevExpress.XtraGrid.GridGroupSummaryItem(DevExpress.Data.SummaryItemType.Sum, "insAmountReceived", this.insAmountReceived18, "${0:0,0.00}"),
                new DevExpress.XtraGrid.GridGroupSummaryItem(DevExpress.Data.SummaryItemType.Sum, "amountGrowth", this.amountGrowth12, "${0:0,0.00}")});
            }
            gridMain.Columns["paidInFull"].Visible = false;

            if (G1.isField())
            {
                pictureDelete.Hide();
                recalculateBalanceToolStripMenuItem.Enabled = false;
                //burialCremationSummaryReportToolStripMenuItem.Enabled = false;
                burialCremationSummaryReportToolStripMenuItem.Visible = false;
                commissionsMenu.Visible = false;
            }

            bandedGridColumn29.Visible = false; // Extra Deceased Date Column
            bandedGridColumn30.Visible = false; // Extra Arranger Column

            //goodsAndServicesContractToolStripMenuItem.Enabled = false;
        }
        /***********************************************************************************************/
        private void GridMain_ShowCustomizationForm(object sender, EventArgs e)
        {
            if (!showFooters)
            {
                gridMain.DestroyCustomization();
            }
        }
        /***********************************************************************************************/
        private void loadLocatons()
        {
            string cmd = "Select * from `funeralhomes` order by `keycode`;";
            DataTable locDt = G1.get_db_data(cmd);

            DataTable newLocDt = locDt.Clone();

            string assignedLocations = "";

            cmd = "Select * from `users` where `username` = '" + LoginForm.username + "';";
            DataTable userDt = G1.get_db_data(cmd);
            if ( userDt.Rows.Count > 0 )
                assignedLocations = userDt.Rows[0]["assignedLocations"].ObjToString();

            string locationCode = "";
            string keyCode = "";
            string[] Lines = null;
            string locations = "";
            string location = "";

            for ( int i=locDt.Rows.Count-1; i>=0; i--)
            {
                keyCode = locDt.Rows[i]["keycode"].ObjToString();
                if (keyCode.IndexOf("-") > 0)
                    locDt.Rows.RemoveAt(i);
            }
            for (int i = 0; i < locDt.Rows.Count; i++)
            {
                locationCode = locDt.Rows[i]["locationCode"].ObjToString();
                if (String.IsNullOrWhiteSpace(locationCode))
                    continue;
                Lines = assignedLocations.Split('~');
                for (int j = 0; j < Lines.Length; j++)
                {
                    location = Lines[j].Trim();
                    if (String.IsNullOrWhiteSpace(location))
                        continue;
                    if (location.ToUpper() == locationCode.ToUpper())
                    {
                        location = locDt.Rows[i]["atNeedCode"].ObjToString();
                        locations += location + "|";
                        newLocDt.ImportRow(locDt.Rows[i]);
                    }
                }
            }
            if (!LoginForm.administrator)
                locDt = newLocDt;

            DataView tempview = locDt.DefaultView;
            tempview.Sort = "atneedcode";
            locDt = tempview.ToTable();


            chkComboLocation.Properties.DataSource = locDt;

            locations = locations.TrimEnd('|');
            chkComboLocation.EditValue = locations;
            chkComboLocation.Text = locations;
        }
        /****************************************************************************************/
        private void SetupTotalsSummary()
        {
            if (!showFooters)
            {
                gridMain.GroupSummary.Clear();
                return;
            }
            //AddSummaryColumn("payment", null);
            AddSummaryColumn("insurancePaymentsReceived", null);
            AddSummaryColumn("amountFiled", null);
            AddSummaryColumn("custPrice", null);
            AddSummaryColumn("custMerchandise", null);
            AddSummaryColumn("custServices", null);
            AddSummaryColumn("totalDiscount", null);
            AddSummaryColumn("currentPrice", null);
            AddSummaryColumn("currentMerchandise", null);
            AddSummaryColumn("currentServices", null);
            AddSummaryColumn("balanceDue", null);
            AddSummaryColumn("additionalDiscount", null);
            AddSummaryColumn("classa", null);
            AddSummaryColumn("grossAmountReceived", null);
            AddSummaryColumn("amountDiscount", null);
            AddSummaryColumn("amountGrowth", null);
            AddSummaryColumn("cashAdvance", null);
            AddSummaryColumn("trustAmountFiled", null);
            AddSummaryColumn("trustAmountReceived", null);
            AddSummaryColumn("insAmountFiled", null);
            AddSummaryColumn("insAmountReceived", null);

            gridMain.Columns["cash"].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Custom;
            gridMain.Columns["cash"].SummaryItem.DisplayFormat = "{0:N2}";
            gridMain.Columns["creditCard"].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Custom;
            gridMain.Columns["creditCard"].SummaryItem.DisplayFormat = "{0:N2}";
            gridMain.Columns["check"].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Custom;
            gridMain.Columns["check"].SummaryItem.DisplayFormat = "{0:N2}";

            gridMain.Columns["tmstamp"].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.None;
            gridMain.Columns["contractType"].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.None;
            gridMain.Columns["caseCreatedDate"].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.None;

            //AddSummaryColumn("currentprice", null);
            //AddSummaryColumn("difference", null);
        }
        /*******************************************************************************************/
        private string getLocationQuery()
        {
            DataRow[] dRows = null;
            DataTable locDt = (DataTable) this.chkComboLocation.Properties.DataSource;
            string procLoc = "";
            string jewelLoc = "";
            string[] locIDs = this.chkComboLocation.EditValue.ToString().Split('|');
            for (int i = 0; i < locIDs.Length; i++)
            {
                if (!String.IsNullOrWhiteSpace(locIDs[i]))
                {
                    if (procLoc.Trim().Length > 0)
                        procLoc += ",";
                    procLoc += "'" + locIDs[i].Trim() + "'";
                    dRows = locDt.Select("atneedcode='" + locIDs[i].Trim() + "'");
                    if ( dRows.Length > 0 )
                    {
                        jewelLoc = dRows[0]["merchandiseCode"].ObjToString();
                        if ( !String.IsNullOrWhiteSpace ( jewelLoc ))
                            procLoc += ",'" + jewelLoc.Trim() + "'";
                    }
                }
            }
            return procLoc.Length > 0 ? " loc IN (" + procLoc + ") " : "";
        }
        /****************************************************************************************/
        private void AddSummaryColumn(string columnName, DevExpress.XtraGrid.Views.Grid.GridView gMain = null)
        {
            if (gMain == null)
                gMain = gridMain;
            if (!showFooters)
            {
                gMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.None;
                gMain.Columns[columnName].SummaryItem.DisplayFormat = "";
                return;
            }
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

            string cmd = "SELECT *, (SELECT `pdfimages`.`record` FROM `pdfimages` WHERE `pdfimages`.`contractNumber` = e.`contractNumber` ) AS `picRecord` FROM `fcust_extended` e LEFT JOIN `fcontracts` p ON p.`contractNumber` = e.`contractNumber` left join `fcustomers` d ON e.`contractNumber` = d.`contractNumber` LEFT JOIN `icontracts` i ON i.`contractNumber` = e.`contractNumber` LEFT JOIN `icustomers` j ON j.`contractNumber` = e.`contractNumber` WHERE e.`ServiceID` <> '' ";
            //cmd = "SELECT * FROM `fcust_extended` e LEFT JOIN `fcontracts` p ON p.`contractNumber` = e.`contractNumber` left join `fcustomers` d ON e.`contractNumber` = d.`contractNumber` LEFT JOIN `icontracts` i ON i.`contractNumber` = e.`contractNumber` LEFT JOIN `icustomers` j ON j.`contractNumber` = e.`contractNumber` WHERE e.`ServiceID` <> '' ";
            string paidInFull = cmbPaidInFull.Text;
            if ( !String.IsNullOrWhiteSpace ( paidInFull ))
            {
                if (paidInFull.Trim().ToUpper() == "EXCLUDE PAID IN FULL")
                    cmd += " AND `paidInFull` <> '1' ";
                else if (paidInFull.Trim().ToUpper() == "PAID IN FULL ONLY")
                    cmd += " AND `paidInFull` = '1' ";
            }

            string date1 = this.dateTimePicker1.Value.ToString("yyyy-MM-dd");
            string date2 = this.dateTimePicker2.Value.ToString("yyyy-MM-dd");
            string serviceId = this.txtServiceId.Text.Trim();
            if (!String.IsNullOrWhiteSpace(serviceId))
            {
                cmd = "SELECT *, (SELECT `pdfimages`.`record` FROM `pdfimages` WHERE `pdfimages`.`contractNumber` = e.`contractNumber` ) AS `picRecord` FROM `fcust_extended` e LEFT JOIN `fcontracts` p ON p.`contractNumber` = e.`contractNumber` left join `fcustomers` d ON e.`contractNumber` = d.`contractNumber` LEFT JOIN `icontracts` i ON i.`contractNumber` = e.`contractNumber` LEFT JOIN `icustomers` j ON j.`contractNumber` = e.`contractNumber` WHERE ";
                //cmd += " e.`serviceId` >= '" + serviceId + "' AND p.`serviceId` = '" + serviceId + "' AND d.`serviceId` = '" + serviceId + "' ";
                cmd += " e.`serviceId` = '" + serviceId + "' ";
            }
            else
            {
                if (chkUseDates.Checked)
                    cmd += " AND e.`serviceDate` >= '" + date1 + "' AND e.`serviceDate` <= '" + date2 + "' ";

                if (chkDeceasedDate.Checked)
                    cmd += " AND p.`deceasedDate` >= '" + date1 + "' AND p.`deceasedDate` <= '" + date2 + "' ";

                if (chkCaseCreateDate.Checked)
                    cmd += " AND e.`caseCreatedDate` >= '" + date1 + "' AND e.`caseCreatedDate` <= '" + date2 + "' ";
            }

            if (chkBalanceDue.Checked)
                cmd += " AND e.`balanceDue` <> '0.00' ";

            //cmd += " AND e.`serviceDate` >= '2015-01-01' ";
            //string locations = getLocationQuery();
            //if (!String.IsNullOrWhiteSpace(locations))
            //    cmd += " AND " + locations;
            if ( chkUseDates.Checked )
                cmd += " ORDER BY e.`serviceDate` DESC ";
            else if ( chkDeceasedDate.Checked )
                cmd += " ORDER BY p.`deceasedDate` DESC ";
            else if ( chkCaseCreateDate.Checked )
                cmd += " ORDER BY e.`caseCreatedDate` DESC ";
            cmd += ";";

            dt = G1.get_db_data(cmd);

            dt = FilterTrustClaims(dt);

            PreProcessData(dt);

            LoadFuneralLocations(dt);

            dt.Columns.Add("num");
            dt.Columns.Add("agreement");
            dt.Columns.Add("paid", Type.GetType("System.Double"));
            dt.Columns.Add("purchase", Type.GetType("System.Double"));
            dt.Columns.Add("contractValue", Type.GetType("System.Double"));

            DataView tempview = dt.DefaultView;
            tempview.Sort = "location, lastName, firstName";
            dt = tempview.ToTable();

            G1.NumberDataTable(dt);
            SetupAgreementIcon(dt);
            SetupPaidUpCheck(dt);
            CalcPaid(dt);
            DetermineLapsed(dt);
            if (oDt == null)
                oDt = dt.Copy();
            originalDt = dt.Copy();

            DataTable manDt = LoadManagers(dt);
            chkManagers.Properties.DataSource = manDt;
            
            DataTable chkDt = LoadArrangers(dt);
            chkArrangers.Properties.DataSource = chkDt;

            dt = FilterTrustClaims(dt);

            dgv.DataSource = dt;

            serviceId = this.txtServiceId.Text.Trim();
            if (String.IsNullOrWhiteSpace(serviceId))
                chkComboLocation_EditValueChanged(null, null);

            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        public static DataTable LoadManagers ( DataTable dt  )
        {
            DataTable manDt = new DataTable();
            manDt.Columns.Add("manager");
            string manager = "";

            DataRow[] dRows = null;
            DataRow dR = null;
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                manager = dt.Rows[i]["manager"].ObjToString();
                if (String.IsNullOrWhiteSpace(manager))
                    continue;
                dRows = manDt.Select("manager='" + manager + "'");
                if ( dRows.Length <= 0 )
                {
                    dR = manDt.NewRow();
                    dR["manager"] = manager;
                    manDt.Rows.Add(dR);
                }
            }

            DataView tempview = manDt.DefaultView;
            tempview.Sort = "manager";
            manDt = tempview.ToTable();
            return manDt;
        }
        /***********************************************************************************************/
        public static DataTable LoadArrangers(DataTable dt)
        {
            DataTable manDt = new DataTable();
            manDt.Columns.Add("arranger");
            string manager = "";

            DataRow[] dRows = null;
            DataRow dR = null;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                manager = dt.Rows[i]["Funeral Arranger"].ObjToString();
                if (String.IsNullOrWhiteSpace(manager))
                    continue;
                dRows = manDt.Select("arranger='" + manager + "'");
                if (dRows.Length <= 0)
                {
                    dR = manDt.NewRow();
                    dR["arranger"] = manager;
                    manDt.Rows.Add(dR);
                }
            }
            DataView tempview = manDt.DefaultView;
            tempview.Sort = "arranger";
            manDt = tempview.ToTable();
            return manDt;
        }
        /***********************************************************************************************/
        public static void PreProcessData(DataTable dt)
        {
            string contractNumber = "";
            string lastName = "";
            string firstName = "";
            string payer = "";
            bool noFirstName1 = false;
            if (G1.get_column_number(dt, "firstName1") < 0)
                noFirstName1 = true;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                if (contractNumber == "SX221474")
                {
                }
                if (DailyHistory.isInsurance(contractNumber))
                {
                    if (!noFirstName1)
                    {
                        firstName = dt.Rows[i]["firstName1"].ObjToString();
                        lastName = dt.Rows[i]["lastName1"].ObjToString();
                        dt.Rows[i]["firstName"] = firstName;
                        dt.Rows[i]["lastName"] = lastName;
                    }
                    payer = dt.Rows[i]["payer"].ObjToString();
                    if (String.IsNullOrWhiteSpace(payer))
                    {
                        payer = dt.Rows[i]["payer1"].ObjToString();
                    }
                    dt.Rows[i]["contractNumber"] = payer;
                }
            }
        }
        /***********************************************************************************************/
        public static void DetermineLapsed(DataTable dt)
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
        public static void CalcPaid(DataTable dt)
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
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            G1.ShowHideFindPanel(gridMain);
        }
        /***********************************************************************************************/
        private void SetupPaidUpCheck ( DataTable dt)
        {
            DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew = this.repPaidCheckEdit;
            selectnew.NullText = "";
            selectnew.ValueChecked = "1";
            selectnew.ValueUnchecked = "0";
            selectnew.ValueGrayed = "";
            string paid = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                paid = dt.Rows[i]["paidInFull"].ObjToString();
                if ( paid == "1" )
                    dt.Rows[i]["paidInFull"] = "1";
                else
                    dt.Rows[i]["paidInFull"] = "0";
            }
        }
        /***********************************************************************************************/
        private void SetupAgreementIcon(DataTable dt)
        {
            if (G1.get_column_number(dt, "picRecord") < 0)
                return;
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
                        if (!String.IsNullOrWhiteSpace(record))
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

            printingSystem1.Document.AutoFitToPagesWidth = 1;

            G1.PrintPreview(printableComponentLink1, gridMain);

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

            printingSystem1.Document.AutoFitToPagesWidth = 1;

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

            string cmd = "SELECT *, (SELECT `pdfimages`.`record` FROM `pdfimages` WHERE `pdfimages`.`contractNumber` = e.`contractNumber` ) AS `picRecord` FROM `fcust_extended` e LEFT JOIN `fcontracts` p ON p.`contractNumber` = e.`contractNumber` left join `fcustomers` d ON e.`contractNumber` = d.`contractNumber` LEFT JOIN `icontracts` i ON i.`contractNumber` = e.`contractNumber` LEFT JOIN `icustomers` a ON a.`contractNumber` = e.`contractNumber` WHERE e.`ServiceID` <> '' ";
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
            if (dx == null)
                dx = (DataTable)dgv.DataSource;
            if (dx == null)
            {
                if ( dt.Rows.Count <= 0 )
                    return;
                dx = dt.Copy();
            }
            else
            {
                if (dt.Rows.Count > 0)
                    dx.ImportRow(dt.Rows[0]);
            }
            G1.NumberDataTable(dx);
            if (originalDt != null)
                originalDt = dx;
            dgv.DataSource = dx;

            dx = (DataTable)dgv.DataSource;
            int row = dx.Rows.Count - 1;
            gridMain.FocusedRowHandle = row;
            gridMain.SelectRow(row);

            DataRow dr = dx.Rows[row];
            recalculateBalance(contract, dr);

            //gridMain.RefreshEditor(true);
            //dgv.RefreshDataSource();
            //gridMain.RefreshData();
            dgv.Refresh();

            //recalculateBalanceToolStripMenuItem_Click(null, null);

            string format = chkComboLocation.Text;
            if (!String.IsNullOrWhiteSpace(format))
                chkComboLocation_EditValueChanged(null, null);
            if (chkSort.Checked)
                ForceGroups();

            string cnum = "";
            dt = (DataTable)dgv.DataSource;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                cnum = dt.Rows[i]["contractNumber"].ObjToString();
                if (cnum == contract)
                {
                    gridMain.FocusedRowHandle = i;
                    gridMain.SelectRow(i);
                    gridMain.RefreshEditor(true);
                    dgv.RefreshDataSource();
                    gridMain.RefreshData();
                    dgv.Refresh();
                    break;
                }
            }

            this.Cursor = Cursors.WaitCursor;

            CheckForDuplicateSSNs(contract);

            EditCust custForm = new EditCust(contract); // RAMMA ZAMMA
            custForm.Tag = contract;
            custForm.custClosing += CustForm_custClosing;
            custForm.custRename += CustForm_custRename;
            //custForm.TopMost = true;
            custForm.BringToFront();
            custForm.Show();
            //custForm.TopMost = true;
            custForm.WindowState = FormWindowState.Maximized;
            custForm.BringToFront();
            this.Cursor = Cursors.Default;

            this.WindowState = FormWindowState.Minimized;



            //EditCust custForm = new EditCust(contract);
            //custForm.Show();
            //this.Cursor = Cursors.Default;
            //this.TopMost = false;

            //CustomerDetails custForm = new CustomerDetails(contract);
            //custForm.Show();
        }
        /****************************************************************************************/
        private void CustForm_custRename(string contractNumber)
        {
            btnRefresh_Click(null, null);
        }
        /****************************************************************************************/
        private void CheckForDuplicateSSNs( string contract )
        {
            string contractNumber = "";
            DataRow[] dRows = null;

            string workSSN = "";
            string cmd = "Select * from `fcustomers` where `contractNumber` = '" + contract + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
                return;
            workSSN = dx.Rows[0]["ssn"].ObjToString();
            if (workSSN == "0")
                return;
            if (String.IsNullOrWhiteSpace(workSSN))
                return;
            workSSN = workSSN.Replace("-", "");

            cmd = "Select * from `fcustomers` where `ssn` = '" + workSSN + "';";
            dx = G1.get_db_data(cmd);

            cmd = "Select * from `customers` where `ssn` = '" + workSSN + "';";
            DataTable ddx = G1.get_db_data(cmd);
            for (int i = 0; i < ddx.Rows.Count; i++)
            {
                contractNumber = ddx.Rows[i]["contractNumber"].ObjToString();
                if (String.IsNullOrWhiteSpace(contractNumber))
                    continue;
                dRows = dx.Select("contractNumber='" + contractNumber + "'");
                if (dRows.Length <= 0)
                    G1.copy_dt_row(ddx, i, dx, dx.Rows.Count);
            }
            if (dx.Rows.Count <= 1)
                return;

            MessageBox.Show("***INFO*** Contract has duplicate SSN's!\nPlease take time to verify services\nbefore doing any other editing!", "Duplicate SSN's Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
        }
        /***********************************************************************************************/
        private DateTime editCustDate = DateTime.MinValue;
        private void gridMain_DoubleClick_2(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt == null)
                return;
            DataRow dr = gridMain.GetFocusedDataRow();
            string contract = dr["contractNumber"].ObjToString();
            if (!String.IsNullOrWhiteSpace(contract))
            {
                string cmd = "SELECT * FROM `fcust_extended` e LEFT JOIN `fcontracts` p ON p.`contractNumber` = e.`contractNumber` left join `fcustomers` d ON e.`contractNumber` = d.`contractNumber` WHERE e.`contractNumber` = '" + contract + "';";
                DataTable dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                    editCustDate = dx.Rows[0]["tmstamp"].ObjToDateTime();
                else
                    editCustDate = DateTime.MinValue;

                this.Cursor = Cursors.WaitCursor;

                Form form = G1.IsFormOpen("EditCust", contract);
                if (form != null)
                {
                    form.Show();
                    form.WindowState = FormWindowState.Maximized;
                    form.Visible = true;
                    form.BringToFront();
                }
                else
                {
                    EditCust custForm = new EditCust(contract);
                    custForm.Tag = contract;
                    custForm.custClosing += CustForm_custClosing;
                    custForm.custRename += CustForm_custRename1;
                    custForm.Show();
                }
                this.Cursor = Cursors.Default;
            }
        }
        /***********************************************************************************************/
        private void CustForm_custRename1(string contractNumber)
        {
            btnRefresh_Click(null, null);
            CustForm_custClosing(contractNumber, 0D, 0D);
            G1.FindGridViewRow(dgv, gridMain, "contractNumber", contractNumber);
        }
        /***********************************************************************************************/
        private Form IsFormOpen( string name, string contract )
        {
            Form myForm = null;
            bool retval = false;
            string myContract = "";
            FormCollection fc = Application.OpenForms;
            foreach (Form Appforms in fc)
            {
                myContract = Appforms.Tag.ObjToString();
                if ( myContract == contract )
                {
                    if ( Appforms.Name.ToUpper() == name.ToUpper() )
                    {
                        myForm = Appforms;
                        break;
                    }    
                }
                //if (Appforms == frm)
                //{
                //    retval = true;
                //}
            }
            return (myForm);
        }
        /***********************************************************************************************/
        private bool checkForChanges ( string contractNumber, DateTime lastChange, string custExtendedRecord )
        {
            string lastDate = lastChange.ToString("yyyy-MM-dd HH:mm:ss");

            string cmd = "SELECT * FROM `fcust_extended` WHERE `contractNumber` = '" + contractNumber + "' AND `tmstamp` > '" + lastDate + "'ORDER BY `tmstamp` DESC LIMIT 1;";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
                return true;
            return false;
        }
        /***********************************************************************************************/
        private void CustForm_custClosing(string contractNumber, double amountFiled, double amountReceived)
        {
            string extendedRecord = "";
            string record = "";
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt == null)
                return;

            string cmd = "SELECT * FROM `fcust_extended` e LEFT JOIN `fcontracts` p ON p.`contractNumber` = e.`contractNumber` left join `fcustomers` d ON e.`contractNumber` = d.`contractNumber` WHERE e.`contractNumber` = '" + contractNumber + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                extendedRecord = dx.Rows[0]["record"].ObjToString();
                record = extendedRecord;

                bool changed = checkForChanges(contractNumber, editCustDate, extendedRecord );
                if (!changed)
                    return;

                //if (amountFiled != -1D && amountReceived != -1D)
                //{
                try
                {
                    CalculateCustomerDetails(contractNumber, record, dx.Rows[0]);
                    string mRecord = "";
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        mRecord = dt.Rows[i]["record"].ObjToString();
                        if (mRecord == record)
                        {
                            G1.HardCopyDtRow(dx, 0, dt, i);
                            break;
                        }
                        //}
                    }
                }
                catch ( Exception ex)
                {
                }
            }
            try
            {
                this.Show();
                this.Refresh();
                this.WindowState = FormWindowState.Normal;
                this.BringToFront();
            }
            catch ( Exception ex)
            {
            }
        }
        /***********************************************************************************************/
        private void pictureDelete_Click(object sender, EventArgs e)
        {
            if (!LoginForm.administrator)
            {
                MessageBox.Show("***Warning*** You do not have permission to delete a contract!", "Delete Funeral Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }

            string customerRecord = "";
            string contractRecord = "";

            DataRow dr = gridMain.GetFocusedDataRow();
            if (dr == null)
                return;
            string serviceId = dr["serviceId"].ObjToString();
            string contract = dr["contractNumber"].ObjToString();
            if (String.IsNullOrWhiteSpace(contract))
            {
                MessageBox.Show("***Warning*** This Contract Number is empty!!!", "Empty Contract Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }

            string contractsFile = "fcontracts";
            string customersFile = "fcustomers";

            DialogResult result = MessageBox.Show("***Question*** Are you sure you want to DELETE this Funeral Contract (" + contract + ") ?", "Delete Funeral Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            if (result == DialogResult.No)
                return;

            string cmd = "Select * from `" + customersFile + "` where `contractNumber` = '" + contract + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
            {
                MessageBox.Show("***ERROR*** No Customers exist for Contract " + contract + "!", "No Customer Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }
            else
                customerRecord = dt.Rows[0]["record"].ObjToString();

            PleaseWait pleaseForm = G1.StartWait ("Please Wait!\nRemoving All Funeral Information for " + contract + "!");

            string workSSN = dt.Rows[0]["ssn"].ObjToString();

            cmd = "Select * from `" + contractsFile + "` where `contractNumber` = '" + contract + "';";
            dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                MessageBox.Show("***ERROR*** No Contracts exist for Contract " + contract + "!", "No Contract Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            else
                contractRecord = dt.Rows[0]["record"].ObjToString();


            if (!String.IsNullOrWhiteSpace(contractRecord))
                G1.get_db_data("Delete from `" + contractsFile + "` where `record` = '" + contractRecord + "';");

            if (!String.IsNullOrWhiteSpace(customerRecord))
                G1.get_db_data("Delete from `" + customersFile + "` where `record` = '" + customerRecord + "';");

            CleanoutPolicyPayments(contract);

            G1.get_db_data("Delete from `fcust_services` where `contractNumber` = '" + contract + "';");
            G1.get_db_data("Delete from `fcust_extended` where `contractNumber` = '" + contract + "';");
            G1.get_db_data("Delete from `cust_payments` where `contractNumber` = '" + contract + "';");
            G1.get_db_data("Delete from `cust_payment_details` where `contractNumber` = '" + contract + "';");

            if ( contract.ToUpper().IndexOf ( "SX" ) == 0 )
                G1.get_db_data("Delete from `relatives` where `contractNumber` = '" + contract + "';");


            CleanOutServiceId(contract);

            dt = (DataTable)dgv.DataSource;
            int row = gridMain.GetDataSourceRowIndex(gridMain.FocusedRowHandle);
            try
            {
                dt.Rows.RemoveAt(row);
                dt.AcceptChanges();
                gridMain.DeleteRow(gridMain.FocusedRowHandle);
            }
            catch ( Exception ex)
            {
            }
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            dgv.RefreshDataSource();
            dgv.Refresh();

            originalDt = dt;

            G1.StopWait(ref pleaseForm);

            DataTable multiSsnDt = GetMultipleSSN(workSSN);
            if ( multiSsnDt.Rows.Count > 0 )
            {
                result = MessageBox.Show("***Question*** Do you want to DELETE DUPLICATE SSN's (" + workSSN + ")\nFunerals and Pre-Need Service Id's also?", "Delete Duplicate SSN's Info Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                if (result == DialogResult.No)
                    return;
            }

            string otherContract = "";
            string where = "";

            for ( int i=0; i<multiSsnDt.Rows.Count; i++)
            {
                otherContract = multiSsnDt.Rows[i]["contractNumber"].ObjToString();
                where = multiSsnDt.Rows[i]["where"].ObjToString();
                if (where.ToUpper() == "CONTRACTS")
                    CleanOutServiceId(otherContract);
                else
                    DeleteFuneral(otherContract);
            }

            G1.AddToAudit(LoginForm.username, "Funerals", "Delete Main Funeral", "Deleted", contract);

        }
        /***********************************************************************************************/
        private void CleanoutPolicyPayments(string contractNumber )
        {
            if (String.IsNullOrWhiteSpace(contractNumber))
                return;
            string record = "";
            string cmd = "Select * from `cust_payments` where `contractNumber` = '" + contractNumber + "';";
            DataTable dt = G1.get_db_data(cmd);
            for ( int i=0; i<dt.Rows.Count; i++)
                CleanupPossibleInsurance( dt.Rows[i] );
        }
        /****************************************************************************************/
        private void CleanupPossibleInsurance(DataRow dr)
        {
            try
            {
                string type = dr["type"].ObjToString().ToUpper();
                if (type.IndexOf("INSURANCE") >= 0 || type == "3RD PARTY" || type == "CLASS A")
                {
                    string names = dr["names"].ObjToString();
                    string trustOrPolicy = dr["trust_policy"].ObjToString();
                    string[] Lines = trustOrPolicy.Split('/');
                    if (Lines.Length >= 2)
                    {
                        string payer = Lines[0];
                        string policyNumber = Lines[1];
                        Lines = names.Split(',');
                        if (Lines.Length >= 2)
                        {
                            string lName = Lines[0].Trim();
                            string fName = Lines[1].Trim();
                            if (!String.IsNullOrWhiteSpace(policyNumber) && !String.IsNullOrWhiteSpace(payer) && !String.IsNullOrWhiteSpace(lName) && !String.IsNullOrWhiteSpace(fName))
                                FunPayments.UpdatePayerPolicies(payer, policyNumber, fName, lName, "0000-00-00", "", true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
        /***********************************************************************************************/
        public static void DeleteFuneral ( string contract )
        {
            if (String.IsNullOrWhiteSpace(contract))
                return;
            if (contract == "0")
                return;
            if (contract == "1")
                return;

            string contractsFile = "fcontracts";
            string customersFile = "fcustomers";
            string contractRecord = "";
            string customerRecord = "";

            string cmd = "Select * from `" + customersFile + "` where `contractNumber` = '" + contract + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
            {
                customerRecord = "";
                //MessageBox.Show("***ERROR*** No Customers exist for Contract " + contract + "!", "No Customer Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                //return;
            }
            else
                customerRecord = dt.Rows[0]["record"].ObjToString();

            string workSSN = dt.Rows[0]["ssn"].ObjToString();

            cmd = "Select * from `" + contractsFile + "` where `contractNumber` = '" + contract + "';";
            dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
            {
                contractRecord = "";
                //MessageBox.Show("***ERROR*** No Contracts exist for Contract " + contract + "!", "No Contract Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            }
            else
                contractRecord = dt.Rows[0]["record"].ObjToString();

            if (!String.IsNullOrWhiteSpace(contractRecord))
                G1.get_db_data("Delete from `" + contractsFile + "` where `record` = '" + contractRecord + "';");

            if (!String.IsNullOrWhiteSpace(customerRecord))
                G1.get_db_data("Delete from `" + customersFile + "` where `record` = '" + customerRecord + "';");

            G1.get_db_data("Delete from `fcust_services` where `contractNumber` = '" + contract + "';");
            G1.get_db_data("Delete from `fcust_extended` where `contractNumber` = '" + contract + "';");
            G1.get_db_data("Delete from `cust_payments` where `contractNumber` = '" + contract + "';");
            G1.get_db_data("Delete from `cust_payment_details` where `contractNumber` = '" + contract + "';");

            if (contract.ToUpper().IndexOf("SX") == 0)
                G1.get_db_data("Delete from `relatives` where `contractNumber` = '" + contract + "';");


            CleanOutServiceId(contract);

            G1.AddToAudit(LoginForm.username, "Funerals", "Delete Other Funeral", "Deleted", contract);
        }
        /***********************************************************************************************/
        public static DataTable GetMultipleSSN ( string workSSN )
        {
            if (String.IsNullOrWhiteSpace(workSSN))
                workSSN = "000-00-0000";
            else if (workSSN == "0")
                workSSN = "000-00-0000";
            else if (workSSN == "1")
                workSSN = "000-00-0000";

            string contractNumber = "";
            DataRow[] dRows = null;
            string cmd = "Select * from `fcustomers` c JOIN `fcontracts` x ON c.`contractNumber` = x.`contractNumber` where `ssn` = '" + workSSN + "';";
            DataTable dx = G1.get_db_data(cmd);

            dx.Columns.Add("where");
            int row = 0;

            cmd = "Select * from `customers` c JOIN `contracts` x ON c.`contractNumber` = x.`contractNumber` where `ssn` = '" + workSSN + "';";
            DataTable ddx = G1.get_db_data(cmd);
            for (int i = 0; i < ddx.Rows.Count; i++)
            {
                contractNumber = ddx.Rows[i]["contractNumber"].ObjToString();
                if (String.IsNullOrWhiteSpace(contractNumber))
                    continue;
                dRows = dx.Select("contractNumber='" + contractNumber + "'");
                if (dRows.Length <= 0)
                {
                    G1.copy_dt_row(ddx, i, dx, dx.Rows.Count);
                    row = dx.Rows.Count - 1;
                    dx.Rows[row]["where"] = "contracts";
                }
            }
            return dx;
        }
        /***********************************************************************************************/
        public static void DeleteFuneralContract(string contract)
        {
            string customerRecord = "";
            string contractRecord = "";

            string contractsFile = "fcontracts";
            string customersFile = "fcustomers";

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
                G1.get_db_data("Delete from `" + contractsFile + "` where `record` = '" + contractRecord + "';");

            if (!String.IsNullOrWhiteSpace(customerRecord))
                G1.get_db_data("Delete from `" + customersFile + "` where `record` = '" + customerRecord + "';");

            G1.get_db_data("Delete from `fcust_services` where `contractNumber` = '" + contract + "';");
            G1.get_db_data("Delete from `fcust_extended` where `contractNumber` = '" + contract + "';");
            G1.get_db_data("Delete from `cust_payments` where `contractNumber` = '" + contract + "';");
            G1.get_db_data("Delete from `cust_payment_details` where `contractNumber` = '" + contract + "';");

            if (contract.ToUpper().IndexOf("SX") == 0)
                G1.get_db_data("Delete from `relatives` where `contractNumber` = '" + contract + "';");

            CleanOutServiceId(contract);
        }
        /***********************************************************************************************/
        public static void CleanOutServiceId ( string contractNumber)
        {
            string record = "";
            string cmd = "Select * from `cust_extended` where `contractNumber` = '" + contractNumber + "';";
            DataTable dt = G1.get_db_data(cmd);
            if ( dt.Rows.Count > 0 )
            {
                record = dt.Rows[0]["record"].ObjToString();
                G1.update_db_table("cust_extended", "record", record, new string[] {"serviceId", "", "arrangementDate", "0000-00-00", "serviceDate", "0000-00-00", "SRVDATE", "" });
            }

            cmd = "Select * from `customers` where `contractNumber` = '" + contractNumber + "';";
            dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                record = dt.Rows[0]["record"].ObjToString();
                G1.update_db_table("customers", "record", record, new string[] { "ServiceId", "", "deceasedDate", "0000-00-00" });
            }

            cmd = "Select * from `contracts` where `contractNumber` = '" + contractNumber + "';";
            dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                record = dt.Rows[0]["record"].ObjToString();
                G1.update_db_table("contracts", "record", record, new string[] { "ServiceId", "", "deceasedDate", "0000-00-00" });
            }
        }
        /***********************************************************************************************/
        private void gridMain_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            ColumnView view = sender as ColumnView;
            if (e.Column.FieldName.ToUpper().IndexOf("CASECREATEDDATE") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                string str = e.DisplayText.Trim();
                if ( String.IsNullOrWhiteSpace ( str))
                {
                    DataTable dt = (DataTable)dgv.DataSource;
                    int row = e.ListSourceRowIndex;
                    DateTime date = dt.Rows[row]["tmstamp"].ObjToDateTime();
                    e.DisplayText = date.ToString("yyyy-MM-dd");
                }
                else
                {
                    DateTime date = e.DisplayText.ObjToDateTime();
                    if (date.Year < 100)
                        e.DisplayText = "";
                    else
                        e.DisplayText = date.ToString("yyyy-MM-dd");
                }
            }
            else if (e.Column.FieldName.ToUpper().IndexOf("DATE") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
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
            else if (e.Column.FieldName.ToUpper().IndexOf("TMSTAMP") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                DateTime date = e.DisplayText.ObjToString().ObjToDateTime();
                e.DisplayText = date.ToString("yyyy-MM-dd HH:mm:ss");
            }
            else if (e.Column.FieldName.ToUpper().IndexOf("CONTRACTTYPE") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                string data = e.DisplayText;
                string[] Lines = data.Split('(');
                if ( Lines.Length > 1 )
                {
                    data = Lines[1];
                    data = data.Replace("(", "");
                    data = data.Replace(")", "");
                    e.DisplayText = data;
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
            string user = LoginForm.username;
            if (group.ToUpper().IndexOf("(C)") >= 0)
            {
                user = "Common";
                group = group.Replace("(C) ", "");
            }
            string cmd = "Select * from procfiles where name = '" + group + "' and ProcType = '" + procType + "' AND `user` = '" + user + "' order by seq";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
            {
                cmd = "Select * from procfiles where name = '" + group + "' and ProcType = '" + procType + "' AND ( `user` = 'Common' OR `user` = '' ) order by seq";
                dt = G1.get_db_data(cmd);
            }
            if (dt.Rows.Count <= 0)
                return;
            DevExpress.XtraGrid.Views.Grid.GridView gridMain = (DevExpress.XtraGrid.Views.Grid.GridView)dgv.MainView;
            for (int i = 0; i < gridMain.Columns.Count; i++)
                gridMain.Columns[i].Visible = false;
            DataTable ddx = (DataTable)dgv.DataSource;
            int idx = 0;
            string name = "";
            int index = 0;
            for (int i = (dt.Rows.Count - 1); i >= 0; i--)
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
                SetupTotalsSummary();
                gridMain.OptionsView.ShowFooter = showFooters;
            }
            else
            {
                SetupSelectedColumns("Funerals", "Primary", dgv);
                string name = "Funerals Primary";
                foundLocalPreference = G1.RestoreGridLayout(this, this.dgv, gridMain, LoginForm.username, name, ref skinName);
                gridMain.OptionsView.ShowFooter = showFooters;
                SetupTotalsSummary();
            }

            CleanupFieldColumns();

            string format = chkComboLocation.Text;
            if (!String.IsNullOrWhiteSpace(format))
                chkComboLocation_EditValueChanged(null, null);
            if (chkSort.Checked)
                ForceGroups();
            gridMain.OptionsView.ShowBands = true;
            gridMain.Columns["paidInFull"].Visible = false;
        }
        /***********************************************************************************************/
        private void CleanupFieldColumns()
        {
            if (LoginForm.classification.ToUpper() != "FIELD")
                return;
            gridMain.Columns["amountGrowth"].Visible = false;
            gridMain.Columns["amountDiscount"].Visible = false;
            gridMain.Columns["contractNumber"].Visible = false;
        }
        /***********************************************************************************************/
        void sform_Done(DataTable dt)
        {
            string name = cmbSelectColumns.Text.Trim();
            if (String.IsNullOrWhiteSpace(name))
                name = "Primary";
            string saveName = "Funerals " + name;
            string skinName = "";
            SetupSelectedColumns("Funerals", name, dgv);
            foundLocalPreference = G1.RestoreGridLayout(this, this.dgv, gridMain, LoginForm.username, saveName, ref skinName);
            gridMain.OptionsView.ShowFooter = showFooters;
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
        private void btnSelectColumns_Click_1(object sender, EventArgs e)
        {
            string actualName = cmbSelectColumns.Text;
            if (actualName.ToUpper().IndexOf("(C)") >= 0 && !LoginForm.administrator)
            {
                MessageBox.Show("***Warning*** You do not have permission to modify a Common Display Format!", "Display Format Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            //            SelectColumns sform = new SelectColumns(dgv, "Funerals", "Primary", actualName);
            string user = LoginForm.username;
            SelectDisplayColumns sform = new SelectDisplayColumns(dgv, "Funerals", "Primary", actualName, LoginForm.username);
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
            try
            {
                string field = (e.Item as GridSummaryItem).FieldName.ObjToString();
                if (field.ToUpper() == "CASH")
                {
                    DataTable dt = (DataTable)dgv.DataSource;
                    string str = "";
                    double cash = 0D;
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        str = dt.Rows[i]["cash"].ObjToString();
                        str = str.Replace("CA - ", "");
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
                else if (field.ToUpper() == "CHECK")
                {
                    DataTable dt = (DataTable)dgv.DataSource;
                    string str = "";
                    double cash = 0D;
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        str = dt.Rows[i]["check"].ObjToString();
                        str = str.Replace("CK - ", "");
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
            catch ( Exception ex)
            {
            }
        }
        /*******************************************************************************************/
        private DataTable funeralsDt = null;
        public static void LoadFuneralLocations(DataTable dt)
        {
            DataTable funeralsDt = null;
            if (funeralsDt == null)
                funeralsDt = G1.get_db_data("Select * from `funeralhomes`;");
            string contract = "";
            string contractNumber = "";
            string trust = "";
            string loc = "";
            DateTime date = DateTime.Now;
            DataRow[] dR = null;
            if (G1.get_column_number(dt, "loc") < 0)
                dt.Columns.Add("loc");
            if (G1.get_column_number(dt, "location") < 0)
                dt.Columns.Add("location");
            if (G1.get_column_number(dt, "manager") < 0)
                dt.Columns.Add("manager");

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                    if (contractNumber == "SX221474")
                    {
                    }
                    if (contractNumber == "L17035UI")
                    {
                    }
                    contract = dt.Rows[i]["serviceId"].ObjToString();
                    contract = Trust85.decodeContractNumber(contract, true, ref trust, ref loc);

                    if (String.IsNullOrWhiteSpace(loc))
                        continue;
                    //dR = funeralsDt.Select("keycode='" + loc + "'");
                    dR = funeralsDt.Select("atneedcode='" + loc + "'");
                    if (dR.Length > 0)
                    {
                        dt.Rows[i]["loc"] = loc;
                        dt.Rows[i]["location"] = dR[0]["LocationCode"].ObjToString();
                        dt.Rows[i]["manager"] = dR[0]["manager"].ObjToString();
                    }
                    else
                    {
                        dR = funeralsDt.Select("keycode='" + loc + "'");
                        if (dR.Length > 0)
                        {
                            dt.Rows[i]["loc"] = dR[0]["atneedcode"].ObjToString();
                            dt.Rows[i]["location"] = dR[0]["LocationCode"].ObjToString();
                            dt.Rows[i]["manager"] = dR[0]["manager"].ObjToString();
                        }
                        else
                        {
                            dt.Rows[i]["loc"] = loc;
                            dt.Rows[i]["location"] = loc;
                        }
                    }
                }
                catch (Exception ex)
                {

                }
            }
        }
        /***********************************************************************************************/
        private void ForceGroups()
        {
            if (dgv.DataSource == null)
                return;
            DataTable dt = (DataTable)dgv.DataSource;
            DataView tempview = dt.DefaultView;
            tempview.Sort = "location, lastName, firstName";
            dt = tempview.ToTable();
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;

            gridMain.Columns["location"].GroupIndex = 0;
            this.gridMain.ExpandAllGroups();
            SetupTotalsSummary();
            gridMain.RefreshData();
            dgv.Refresh();
            gridMain.OptionsView.ShowFooter = showFooters;
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
                //if (!showFooters)
                //{
                //    gridMain.OptionsView.ShowFooter = false;
                //    gridMain.Appearance.FooterPanel.Dispose();
                //    gridMain.Appearance.GroupFooter.Dispose();
                //    gridMain.Columns["balanceDue"].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.None;
                //    gridMain.Columns["balanceDue"].SummaryItem.DisplayFormat = "";
                //    gridMain.Columns["balanceDue"].Summary.Clear();

                //}
                this.gridMain.ExpandAllGroups();
                SetupTotalsSummary();
            }
            else
            {
                gridMain.Columns["location"].GroupIndex = -1;
                gridMain.CollapseAllGroups();
            }
            gridMain.RefreshData();
            gridMain.RefreshEditor(true);
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
            this.gridMain.ExpandAllGroups();
            gridMain.OptionsView.ShowFooter = showFooters;
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
        private void recalculateBalanceToolStripMenuItem_Clickx(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            string contractNumber = dr["contractNumber"].ObjToString();

            string cmd = "Select * from `fcust_extended` where `contractNumber` = '" + contractNumber + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return;
            string record = dt.Rows[0]["record"].ObjToString();
            CalculateCustomerDetails(contractNumber, record, dr);
        }
        /***********************************************************************************************/
        private void recalculateBalanceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if ( dt == null )
            {
                MessageBox.Show("***INFO*** There are no rows of data to Re-Balance!!", "Re-BalanceDialog", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }
            if (dt.Rows.Count <= 0)
            {
                MessageBox.Show("***INFO*** There are no rows of data to Re-Balance!!", "Re-BalanceDialog", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }
            DataTable dx = null;
            string record = "";
            string contractNumber = "";

            DataRow dr = gridMain.GetFocusedDataRow();

            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);


            int[] rows = gridMain.GetSelectedRows();
            int lastRow = dt.Rows.Count;
            lastRow = rows.Length;

            barImport.Maximum = lastRow;
            barImport.Minimum = 0;
            barImport.Value = 0;
            barImport.Refresh();

            int originalHeight = this.panelTop.Height;
            int newHeight = originalHeight + 26;
            int left = this.panelTop.Left;
            int top = this.panelTop.Top;
            int width = this.panelTop.Width;

            this.panelTop.SetBounds(left, top, width, newHeight);
            this.panelTop.Refresh();

            int count = 0;

            for ( int i=0; i<lastRow; i++)
            {
                Application.DoEvents();
                try
                {
                    barImport.Value = i+1;
                    barImport.Refresh();

                    row = rows[i];
                    row = gridMain.GetDataSourceRowIndex(row);

                    dr = dt.Rows[row];
                    contractNumber = dr["contractNumber"].ObjToString();

                    string cmd = "Select * from `fcust_extended` where `contractNumber` = '" + contractNumber + "';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count <= 0)
                        return;

                    record = dx.Rows[0]["record"].ObjToString();
                    CalculateCustomerDetails(contractNumber, record, dr, true );

                    gridMain.RefreshData();
                    gridMain.RefreshEditor(true);
                    dgv.Refresh();

                    count++;
                }
                catch ( Exception ex )
                {
                    MessageBox.Show("***ERROR*** " + ex.Message.ToString(), "Rebalance Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                }
            }

            string funerals = "Funeral";
            if (count > 1)
                funerals = "Funerals";

            barImport.Value = barImport.Maximum;
            barImport.Refresh();

            MessageBox.Show("***INFO*** (" + count.ToString() + ") " + funerals + " Re-Balanced!", "Re-BalanceDialog", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);

            this.panelTop.SetBounds(left, top, width, originalHeight );
            this.panelTop.Refresh();
        }
        /***********************************************************************************************/
        private void recalculateBalance(string contractNumber, DataRow dr)
        {
            //DataRow dr = gridMain.GetFocusedDataRow();
            //int rowHandle = gridMain.FocusedRowHandle;
            //int row = gridMain.GetDataSourceRowIndex(rowHandle);
            //string contractNumber = dr["contractNumber"].ObjToString();

            string cmd = "Select * from `fcust_extended` where `contractNumber` = '" + contractNumber + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return;
            string record = dt.Rows[0]["record"].ObjToString();
            CalculateCustomerDetails(contractNumber, record, dr, true );
        }
        /***********************************************************************************************/
        public static void CalculateCustomerDetails(string contractNumber, string custExtendedRecord, DataRow dR, bool Rebalance = false )
        {
            DateTime startTime = DateTime.Now; // ZAMMA

            PleaseWait pleaseForm = null;
            pleaseForm = new PleaseWait("Please Wait!\nUpdating Funeral Informtion");
            pleaseForm.Show();
            pleaseForm.Refresh();

            //this.Cursor = Cursors.WaitCursor;

            string cmd = "Select * from `cust_payments` c LEFT JOIN `cust_payment_details` x ON c.`record` = x.`paymentRecord` where c.`contractNumber` = '" + contractNumber + "' order by c.`record` ;";
            //cmd = "Select * from `cust_payments` c where c.`contractNumber` = '" + contractNumber + "';";
            DataTable dt = G1.get_db_data(cmd);

            double oldDiscount = dR["totalDiscount"].ObjToDouble();

            string amountFiled = "";
            string amountReceived = "";
            string amountDiscount = "";
            string amountGrowth = "";
            string grossAmountReceived = "";
            double totalFiled = 0D;
            double totalReceived = 0D;
            double totalDiscount = 0D;
            double totalAmountGrowth = 0D;
            double totalAmountDiscount = 0D;
            double totalGross = 0D;
            double payment = 0D;
            double totalPayments = 0D;
            double totalDBR = 0D;
            double dbr = 0D;
            double totalRefund = 0D;
            double refund = 0D;
            double money = 0D;
            double totalMoney = 0D;

            double trustAmountFiled = 0D;
            double trustFiledRemaining = 0D;
            double tempReceived = 0D;
            double insAmountFiled = 0D;
            double trustAmountReceived = 0D;
            double trustPaid = 0D;
            double trustDiscount = 0D;
            double insAmountReceived = 0D;
            double amtActuallyReceived = 0D;
            double insuraceDirectGrowth = 0D;

            DataTable exceptionDt = G1.get_db_data("Select * from `funeral_master`;");

            DateTime date = DateTime.Now;

            string str = "";
            string type = "";
            string cash = "";
            string status = "";
            string deposit = "";
            string creditCard = "";
            string check = "";
            string ccDepNumber = "";
            string chkDepNumber = "";
            string trustDepNumber = "";
            string insDepNumber = "";
            double dValue = 0D;
            double balanceDue = 0D;
            double discount = 0D;
            double classa = 0D;
            double insuranceDirectGrowth = 0D;
            string approvedBy = "";
            DateTime dateEntered = DateTime.Now;
            DateTime dateModified = DateTime.Now;
            string lastRecord = "";
            string record = "";
            string trustNumber = "";
            DataRow[] dRows = null;
            string pRecord = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    record = dt.Rows[i]["record"].ObjToString();
                    if (lastRecord != record)
                    {
                        status = dt.Rows[i]["status"].ObjToString().ToUpper();
                        if (status.ToUpper() == "CANCELLED" || status.ToUpper() == "REJECTED")
                            continue;
                        amountFiled = dt.Rows[i]["amountFiled"].ObjToString();
                        amountReceived = dt.Rows[i]["amountReceived"].ObjToString();
                        amountDiscount = dt.Rows[i]["amountDiscount"].ObjToString();
                        amountGrowth = dt.Rows[i]["amountGrowth"].ObjToString();
                        grossAmountReceived = dt.Rows[i]["grossAmountReceived"].ObjToString();
                        payment = dt.Rows[i]["payment"].ObjToDouble();
                        totalFiled += amountFiled.ObjToDouble();
                        //totalReceived += amountReceived.ObjToDouble();
                        totalAmountDiscount += amountDiscount.ObjToDouble();
                        type = dt.Rows[i]["type"].ObjToString().ToUpper();
                        if ( type != "TRUST")
                            totalAmountGrowth += amountGrowth.ObjToDouble();
                        totalGross += grossAmountReceived.ObjToDouble();

                        amtActuallyReceived += dt.Rows[i]["amtActuallyReceived"].ObjToDouble();

                        type = dt.Rows[i]["type"].ObjToString().ToUpper();
                        if ( type.ToUpper() == "CHECK" || type.ToUpper() == "CASH" || type.ToUpper() == "CREDIT CARD" || type.ToUpper().IndexOf ( "ACH" ) > 0 )
                        {
                            money = dt.Rows[i]["payment"].ObjToDouble();
                            totalMoney += money;
                        }
                        if (type.ToUpper() == "REFUND")
                        {
                            refund = dt.Rows[i]["payment"].ObjToDouble();
                            totalRefund += refund;
                            continue;
                        }
                        if (type.ToUpper() == "DISCOUNT" || status.ToUpper() == "DEPOSITED")
                        {
                            totalPayments += payment;
                            if (status.ToUpper() == "DEPOSITED")
                                totalReceived += payment;
                        }
                        if (type == "INSURANCE DIRECT" && status == "DEPOSITED")
                            insuranceDirectGrowth += amountGrowth.ObjToDouble();

                        if (type.ToUpper() == "CHECK" && (status.ToUpper() == "ACCEPT" || status.ToUpper() == "DEPOSITED" ) )
                        {
                            totalPayments += payment;
                            totalReceived += payment;

                            str = G1.ReformatMoney(payment);
                            check += "CK - " + str + " ";
                        }
                        if (type == "CASH")
                        {
                            totalPayments += payment;
                            totalReceived += payment;

                            dValue = dt.Rows[i]["payment"].ObjToDouble();
                            str = G1.ReformatMoney(dValue);
                            cash += "CA - " + str + " ";
                        }
                        else if (type == "OTHER")
                        {
                            totalPayments += payment;
                            totalReceived += payment;

                            dValue = dt.Rows[i]["payment"].ObjToDouble();
                            str = G1.ReformatMoney(dValue);
                            cash += "CA - " + str + " ";
                        }
                        else if (type == "CREDIT CARD")
                        {
                            totalPayments += payment;
                            totalReceived += payment;

                            dValue = dt.Rows[i]["payment"].ObjToDouble();
                            str = G1.ReformatMoney(dValue);
                            creditCard += "CC - " + str + " ";
                        }
                        else if (type == "CLASS A")
                        {
                            classa += dt.Rows[i]["payment"].ObjToDouble();
                        }
                        str = dt.Rows[i]["depositNumber"].ObjToString();
                        if (!String.IsNullOrWhiteSpace(str))
                        {
                            if (type == "CASH")
                                deposit += str + " ";
                            else if (type == "CREDIT CARD")
                                ccDepNumber += str + " ";
                            else if (type == "CHECK")
                                chkDepNumber += str + " ";
                        }
                        if (type == "DISCOUNT")
                        {
                            discount += dt.Rows[i]["payment"].ObjToDouble();
                            str = dt.Rows[i]["approvedBy"].ObjToString();
                            if (!String.IsNullOrWhiteSpace(str))
                                approvedBy += str + " ";
                        }
                        if (type == "TRUST")
                        {
                            trustPaid += dt.Rows[i]["paid"].ObjToDouble();
                            trustDepNumber += str + " ";
                            amountFiled = dt.Rows[i]["trustAmtFiled"].ObjToString();
                            tempReceived = dt.Rows[i]["amtActuallyReceived"].ObjToDouble();
                            if (tempReceived == 0D)
                                trustFiledRemaining += amountFiled.ObjToDouble();
                            if (status == "PENDING")
                            {
                                if (amountFiled.ObjToDouble() > 0D)
                                    trustAmountFiled += amountFiled.ObjToDouble();
                                //else
                                //    trustAmountFiled += payment;
                            }
                            else if (status == "DEPOSITED")
                            {
                                if (amountFiled.ObjToDouble() > 0D)
                                    trustAmountFiled += amountFiled.ObjToDouble();
                                trustAmountReceived += amountReceived.ObjToDouble();
                            }
                            dbr = dt.Rows[i]["dbr"].ObjToDouble();
                            if (dbr == 0D || 1 == 1 )
                            {
                                trustNumber = dt.Rows[i]["trust_policy"].ObjToString();
                                if (!String.IsNullOrWhiteSpace(trustNumber))
                                {
                                    dbr = DailyHistory.GetPossibleDBR(trustNumber);
                                    dbr = G1.RoundValue(dbr);
                                    if ( dbr > 0D )
                                    {
                                        pRecord = dt.Rows[i]["paymentRecord"].ObjToString();
                                        if (!String.IsNullOrWhiteSpace(pRecord))
                                        {
                                            dRows = dt.Select("paymentRecord='" + pRecord + "' AND status1='CANCELLED'");
                                            if (dRows.Length > 0)
                                                dbr = 0D;
                                        }
                                    }
                                    totalDBR += dbr;
                                }
                            }
                            else
                                totalDBR += dbr;
                        }
                        else if (type.ToUpper().IndexOf ( "INSURANCE") >= 0 )
                        {
                            insDepNumber += str + " ";
                            if (status == "PENDING")
                            {
                                if (amountFiled.ObjToDouble() > 0D)
                                    insAmountFiled += amountFiled.ObjToDouble();
                                else
                                    insAmountFiled += payment;
                                //else
                                //    insAmountFiled += payment;
                            }
                            else if (status == "DEPOSITED")
                            {
                                if (amountFiled.ObjToDouble() > 0D)
                                    insAmountFiled += amountFiled.ObjToDouble();
                                else
                                    insAmountFiled += payment;
                                insAmountReceived += amountReceived.ObjToDouble();
                            }
                        }
                    }
                    else
                    {
                        status = dt.Rows[i]["status"].ObjToString().ToUpper();
                        if (status.ToUpper() == "CANCELLED")
                            continue;
                        amountFiled = dt.Rows[i]["amountFiled"].ObjToString();
                        amountReceived = dt.Rows[i]["amountReceived"].ObjToString();
                        amountDiscount = dt.Rows[i]["amountDiscount"].ObjToString();
                        amountGrowth = dt.Rows[i]["amountGrowth"].ObjToString();
                        grossAmountReceived = dt.Rows[i]["grossAmountReceived"].ObjToString();
                        payment = dt.Rows[i]["payment"].ObjToDouble();
                        type = dt.Rows[i]["type"].ObjToString().ToUpper();
                        if ( type == "TRUST")
                        {
                            amountFiled = dt.Rows[i]["TrustAmtFiled"].ObjToString();
                            trustAmountFiled += amountFiled.ObjToDouble();
                            if (amountReceived.ObjToDouble() == 0D)
                                trustDiscount += amountFiled.ObjToDouble();
                            tempReceived = dt.Rows[i]["amtActuallyReceived"].ObjToDouble();
                            if (tempReceived == 0D)
                                trustFiledRemaining += amountFiled.ObjToDouble();
                        }

                    }
                    lastRecord = record;
                    //date = dt.Rows[i]["dateFiled"].ObjToDateTime();
                    //if (date.Year > 100)
                    //    dateFiled += date.ToString("MM/dd/yyyy") + " ";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("***ERROR*** 1 " + ex.Message.ToString(), "Rebalance Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                }
            }

            amtActuallyReceived = G1.RoundValue(amtActuallyReceived);
            trustDiscount = G1.RoundValue(trustDiscount);
            trustDiscount = trustAmountFiled - trustAmountReceived;
            trustDiscount = G1.RoundValue(trustDiscount);

            trustDiscount = trustPaid - trustAmountFiled;
            trustDiscount = G1.RoundValue(trustDiscount);

            string serviceId = dR["ServiceId"].ObjToString();
            EditCust.DetermineActiveGroups(contractNumber, serviceId);

            string myActiveFuneralHomeGroup = EditCust.activeFuneralHomeGroup;
            string myActiveFuneralHomeCasketGroup = EditCust.activeFuneralHomeCasketGroup;

            EditCustomer.activeFuneralHomeGroup = myActiveFuneralHomeGroup;
            EditCustomer.activeFuneralHomeCasketGroup = myActiveFuneralHomeCasketGroup;


            double newContractTotal = 0D;
            double newTotalCost = 0D;
            double newPreDiscount = 0D;
            calculateTotalServices(contractNumber, ref newContractTotal, ref newTotalCost, ref newPreDiscount);


            FunServices funForm = new FunServices(contractNumber);
            DataTable funDt = funForm.funServicesDT;

            double price = 0D;
            double totalServices = 0D;
            double totalMerchandise = 0D;
            double totalCashAdvance = 0D;
            double totalCost = 0D;
            double difference = 0D;
            double totalDifference = 0D;
            double currentPrice = 0D;
            double totalCurrentPrice = 0D;
            double contractTotal = 0D;
            double preDiscount = 0D;
            double thirdDiscount = 0D;

            FunServices.CalcTotalServices(funDt, ref contractTotal, ref totalCost, ref preDiscount );

            if (SMFS.activeSystem.ToUpper() == "OTHER")
            {
                preDiscount = Math.Abs(preDiscount);
                preDiscount = 0D;
            }

            double trustPayments = 0D;
            double trustPaymentsReceived = 0D;
            double trustGrowth = 0D;
            double insurancePayments = 0D;
            double insurancePaymentsReceived = 0D;
            double insuranceGrowth = 0D;
            double cashReceived = 0D;
            double compDiscounts = 0D;
            double classA = 0D;
            double otherPreDiscount = 0D;
            trustFiledRemaining = 0D;

            double payments = calculateTotalPayments(contractNumber, ref trustPayments, ref trustPaymentsReceived, ref insurancePayments, ref insurancePaymentsReceived, ref cashReceived, ref compDiscounts, ref classA , ref trustFiledRemaining, ref thirdDiscount, ref trustGrowth, ref insuranceGrowth, ref otherPreDiscount );
            if (SMFS.activeSystem.ToUpper() == "OTHER")
            {
                trustAmountReceived = trustPaymentsReceived;
                if (trustPayments > 0D)
                    trustPayments += otherPreDiscount;
                else
                    compDiscounts += otherPreDiscount;
                trustDiscount = otherPreDiscount;
                //compDiscounts = otherPreDiscount;
            }

            string service = "";
            string oldService = "";

            double currentServices = 0D;
            double currentMerchandise = 0D;

            double merchandiseDiscount = 0D;
            double servicesDiscount = 0D;

            double totalPackagePrice = 0D;
            double packagePrice = 0D;
            double packageDiscount = 0D;
            bool gotPackage = false;

            bool gotDesc = false;

            string[] Lines = null;
            DataTable mDt = null;
            string casket = "";
            string vault = "";
            string casketCode = "";
            string serialNumber = "";
            string casketDesc = "";
            double casketAmount = 0D;
            double casketCost = 0D;
            double vaultAmount = 0D;
            double vaultCost = 0D;
            double urnCost = 0D;
            string urnDesc = "";
            string casketType = "";
            string casketGauge = "";
            double urn = 0D;
            double headCapPanel = 0D;
            double misc = 0D;
            double upgrade = 0D;
            double salesTax = 0D;
            double taxMerchandise = 0D;
            double taxAmount = 0D;
            double asService = 0D;
            double asCash = 0D;
            double asNothing = 0D;
            double asMerc = 0D;
            double fromService = 0D;
            double fromMerc = 0D;
            string zeroData = "";

            string fpc = "";
            DataTable bateDt = null;


            if (funDt != null)
            {
                dRows = funDt.Select("serialNumber<>''");
                if ( dRows.Length == 0 )
                {
                    dRows = funDt.Select("service LIKE '%Family Provided Casket%'");
                    if ( dRows.Length > 0 )
                        fpc = "Y";
                }
                for (int i = 0; i < funDt.Rows.Count; i++)
                {
                    if (funDt.Rows[i]["ignore"].ObjToString().ToUpper() == "Y")
                        continue;
                    zeroData = funDt.Rows[i]["data"].ObjToString();
                    upgrade = funDt.Rows[i]["upgrade"].ObjToDouble();
                    price = funDt.Rows[i]["price"].ObjToDouble();
                    if (price <= 0D && upgrade <= 0D)
                    {
                        service = funDt.Rows[i]["service"].ObjToString().ToUpper();
                        if (service != "PACKAGE DISCOUNT")
                        {
                            if (zeroData.ToUpper() != "ZERO")
                                continue;
                        }
                    }

                    //if (price == 0D) // Changed 3/13/2025 because of Funeral Bonus // 3/14/2025 Changed Back Because of BN25016
                    //    continue;
                    currentPrice = funDt.Rows[i]["currentPrice"].ObjToDouble();
                    difference = currentPrice - price;

                    type = funDt.Rows[i]["type"].ObjToString().ToUpper();
                    service = funDt.Rows[i]["service"].ObjToString().ToUpper();
                    if (service.IndexOf("D- ") == 0)
                        service = service.Replace ("D- ", "");
                    else if ( service.IndexOf ( "D-") == 0 )
                        service = service.Replace("D-", "");

                    dRows = exceptionDt.Select("service='" + service + "'");
                    if ( dRows.Length > 0 && type.ToUpper() != "CASH ADVANCE" )
                    {
                        if ( dRows[0]["asService"].ObjToString() == "1" )
                            asService += funDt.Rows[i]["currentprice"].ObjToDouble();
                        if ( dRows[0]["fromService"].ObjToString() == "1")
                        {
                            if (type == "SERVICE")
                                fromService += funDt.Rows[i]["currentprice"].ObjToDouble();
                        }
                        if (dRows[0]["fromMerc"].ObjToString() == "1")
                        {                            
                            if (type == "MERCHANDISE")
                                fromMerc += funDt.Rows[i]["currentprice"].ObjToDouble();
                        }
                        if (dRows[0]["asCash"].ObjToString() == "1")
                        {
                            if (type.ToUpper() == "MERCHANDISE")
                            {
                                asCash += funDt.Rows[i]["currentprice"].ObjToDouble();
                                asService += funDt.Rows[i]["currentprice"].ObjToDouble();
                            }
                            else
                                asCash += funDt.Rows[i]["currentprice"].ObjToDouble();
                        }
                        if (dRows[0]["asNothing"].ObjToString() == "1")
                            asNothing += funDt.Rows[i]["currentprice"].ObjToDouble();
                        if (dRows[0]["asMerc"].ObjToString() == "1")
                        {
                            if (type.ToUpper() != "MERCHANDISE")
                                asMerc += funDt.Rows[i]["price"].ObjToDouble();
                        }
                    }
                    else
                    {
                        if (service.ToUpper().IndexOf("MILEAGE") >= 0)
                        {
                            if (type.ToUpper() != "CASH ADVANCE")
                            {
                                if (type.ToUpper() == "MERCHANDISE")
                                    fromMerc += funDt.Rows[i]["currentprice"].ObjToDouble();
                                else
                                    asCash += funDt.Rows[i]["currentprice"].ObjToDouble();
                            }
                        }
                        else if (service.ToUpper().IndexOf("ENGRAV") >= 0)
                        {
                            if (type.ToUpper() != "CASH ADVANCE")
                            {
                                if (type.ToUpper() == "MERCHANDISE")
                                {
                                    asCash += funDt.Rows[i]["currentprice"].ObjToDouble();
                                    asService += funDt.Rows[i]["currentprice"].ObjToDouble();
                                }
                                else
                                    asCash += funDt.Rows[i]["currentprice"].ObjToDouble();
                            }
                        }
                        else if (service.ToUpper().IndexOf("MEDALLION") >= 0)
                        {
                            if (type.ToUpper() != "CASH ADVANCE")
                            {
                                if (type.ToUpper() == "MERCHANDISE")
                                {
                                    asCash += funDt.Rows[i]["currentprice"].ObjToDouble();
                                    asService += funDt.Rows[i]["currentprice"].ObjToDouble();
                                }
                                else
                                    asCash += funDt.Rows[i]["currentprice"].ObjToDouble();
                            }
                        }
                        else if (service.ToUpper().IndexOf("KEEPSAKE") >= 0)
                        {
                            if (type.ToUpper() != "CASH ADVANCE")
                            {
                                if (type.ToUpper() == "MERCHANDISE")
                                {
                                    asCash += funDt.Rows[i]["currentprice"].ObjToDouble();
                                    asService += funDt.Rows[i]["currentprice"].ObjToDouble();
                                }
                                else
                                    asCash += funDt.Rows[i]["currentprice"].ObjToDouble();
                            }
                        }
                        else if (service.ToUpper().IndexOf("LIFE PRINT") >= 0)
                        {
                            if (type.ToUpper() != "CASH ADVANCE")
                            {
                                if (type.ToUpper() == "MERCHANDISE")
                                {
                                    asCash += funDt.Rows[i]["currentprice"].ObjToDouble();
                                    asService += funDt.Rows[i]["currentprice"].ObjToDouble();
                                }
                                else
                                    asCash += funDt.Rows[i]["currentprice"].ObjToDouble();
                            }
                        }
                        else if (service.ToUpper().IndexOf("LIFE STOR") >= 0)
                        {
                            if (type.ToUpper() != "CASH ADVANCE")
                            {
                                if (type.ToUpper() == "MERCHANDISE")
                                {
                                    asCash += funDt.Rows[i]["currentprice"].ObjToDouble();
                                    asService += funDt.Rows[i]["currentprice"].ObjToDouble();
                                }
                                else
                                    asCash += funDt.Rows[i]["currentprice"].ObjToDouble();
                            }
                        }
                        else if (service.ToUpper().IndexOf("BOOKMARK") >= 0)
                        {
                            if (type.ToUpper() != "CASH ADVANCE")
                            {
                                if (type.ToUpper() == "MERCHANDISE")
                                {
                                    asCash += funDt.Rows[i]["currentprice"].ObjToDouble();
                                    asService += funDt.Rows[i]["currentprice"].ObjToDouble();
                                }
                                else
                                    asCash += funDt.Rows[i]["currentprice"].ObjToDouble();
                            }
                        }
                        else if (service.ToUpper().IndexOf("SHIPPING") >= 0)
                        {
                            if (type.ToUpper() != "CASH ADVANCE")
                            {
                                if (type.ToUpper() == "MERCHANDISE")
                                    fromMerc += funDt.Rows[i]["currentprice"].ObjToDouble();
                                else
                                    asCash += funDt.Rows[i]["currentprice"].ObjToDouble();
                            }
                        }
                        else if (type.ToUpper() == "SERVICE")
                        {
                            if (service.ToUpper().IndexOf("D-") == 0)
                            {
                                if (service.ToUpper().IndexOf("INFANT") < 0)
                                    asCash += funDt.Rows[i]["currentprice"].ObjToDouble();
                            }
                        }
                        else
                        {
                            //if (dRows[0]["asService"].ObjToString() == "1")
                            //    asService += funDt.Rows[i]["currentprice"].ObjToDouble();
                            //if (dRows[0]["fromService"].ObjToString() == "1")
                            //{
                            //    if (type == "SERVICE")
                            //        fromService += funDt.Rows[i]["currentprice"].ObjToDouble();
                            //}
                            //if (dRows[0]["fromMerc"].ObjToString() == "1")
                            //{
                            //    if (type == "MERCHANDISE")
                            //        fromMerc += funDt.Rows[i]["currentprice"].ObjToDouble();
                            //}
                            //if (dRows[0]["asCash"].ObjToString() == "1")
                            //{
                            //    asCash += funDt.Rows[i]["currentprice"].ObjToDouble();
                            //}
                            //if (dRows[0]["asNothing"].ObjToString() == "1")
                            //    asNothing += funDt.Rows[i]["currentprice"].ObjToDouble();
                            //if (dRows[0]["asMerc"].ObjToString() == "1")
                            //    asMerc += funDt.Rows[i]["price"].ObjToDouble();
                            //if (service.ToUpper().IndexOf("D-") == 0)
                            //{
                            //    if (service.ToUpper().IndexOf("INFANT CASKET") < 0)
                            //        asCash += funDt.Rows[i]["currentprice"].ObjToDouble();
                            //}
                        }
                    }
                    if (service == "TOTAL LISTED PRICE")
                    {
                        totalPackagePrice = funDt.Rows[i]["price"].ObjToDouble();
                        continue;
                    }
                    else if (service == "PACKAGE PRICE")
                    {
                        gotPackage = true;
                        packagePrice = funDt.Rows[i]["price"].ObjToDouble();
                        continue;
                    }
                    else if (service == "PACKAGE DISCOUNT")
                    {
                        packageDiscount = funDt.Rows[i]["price"].ObjToDouble();
                        packageDiscount = Math.Abs(packageDiscount);
                        continue;
                    }

                    type = funDt.Rows[i]["type"].ObjToString().ToUpper();
                    if (price < 0D)
                    {
                        price = Math.Abs(price);
                        if (type.ToUpper() == "SERVICE")
                            servicesDiscount += funDt.Rows[i]["difference"].ObjToDouble();
                        else if (type.ToUpper() == "MERCHANDISE")
                            merchandiseDiscount += funDt.Rows[i]["difference"].ObjToDouble();
                        continue;
                    }

                    taxAmount = funDt.Rows[i]["taxAmount"].ObjToDouble();
                    if ( taxAmount > 0D )
                    {
                        salesTax += taxAmount;
                        taxMerchandise += price;
                    }

                    if (type == "SERVICE")
                    {
                        totalServices += price;
                        servicesDiscount += difference;
                        currentServices += currentPrice;
                        if (service.IndexOf("RENTAL CASKET") >= 0)
                        {
                            casketDesc = service;
                            casketCode = "Rental";
                        }
                        else if (service.ToUpper().IndexOf("INFANT") >= 0)
                        {
                            casketDesc = service;
                            casketCost = price;
                            casketCode = "Infant";
                        }
                    }
                    else if (type == "MERCHANDISE")
                    {
                        if (service.ToUpper().IndexOf("INFANT") >= 0)
                        {
                            casketDesc = service;
                            casketCost = price;
                            casketCode = "Infant";
                        }
                        totalMerchandise += price;
                        merchandiseDiscount += difference;
                        currentMerchandise += currentPrice;
                        if (service.IndexOf("D-") == 0)
                            service = service.Substring(2).Trim();
                        oldService = service;
                        if (service.IndexOf("ACKNOW") < 0 && service.IndexOf("GRAVE MARKER") < 0 && service.ToUpper().IndexOf("REGISTER BOOK") < 0)
                        {
                            if (service.IndexOf("V") == 0)
                            {
                                Lines = service.Split(' ');
                                service = service.Replace(Lines[0].Trim(), "").Trim();
                                if (String.IsNullOrWhiteSpace(service))
                                    service = oldService;
                            }

                            cmd = "SELECT * FROM `casket_master` WHERE `casketDesc` = '" + service + "';";
                            mDt = G1.get_db_data(cmd);
                            if (mDt.Rows.Count <= 0)
                            {
                                Lines = service.Split(' ');
                                str = Lines[0].Trim();
                                cmd = "SELECT * FROM `casket_master` WHERE `casketcode` = '" + str + "';";
                                mDt = G1.get_db_data(cmd);
                                if ( mDt.Rows.Count <= 0 )
                                {
                                    service = service.Replace(Lines[0].Trim(), "").Trim();
                                    cmd = "SELECT * FROM `casket_master` WHERE `casketDesc` = '" + service + "';";
                                    mDt = G1.get_db_data(cmd);
                                    if (mDt.Rows.Count <= 0 )
                                    {
                                        cmd = "SELECT * FROM `casket_master` WHERE `casketDesc` LIKE '" + str + "%';";
                                        mDt = G1.get_db_data(cmd);
                                    }
                                }
                            }
                            if (mDt.Rows.Count > 0)
                            {
                                casketCode = mDt.Rows[0]["casketcode"].ObjToString();
                                if (casketCode.ToUpper().IndexOf("V") == 0)
                                {
                                    vault = casketCode;
                                    vaultAmount = funDt.Rows[i]["currentprice"].ObjToDouble();
                                    if (vaultAmount <= 0D)
                                        vaultAmount = funDt.Rows[i]["price"].ObjToDouble();
                                    vaultCost = mDt.Rows[0]["casketcost"].ObjToDouble();
                                }
                                else if (casketCode.ToUpper().IndexOf("URN") == 0)
                                {
                                    dValue = funDt.Rows[i]["currentprice"].ObjToDouble();
                                    if (dValue <= 0D)
                                        dValue = funDt.Rows[i]["price"].ObjToDouble();
                                    urn += dValue;
                                    if ( mDt.Rows.Count > 0 )
                                    {
                                        urnCost = mDt.Rows[0]["casketcost"].ObjToDouble();
                                        urnDesc = mDt.Rows[0]["casketdesc"].ObjToString();
                                    }
                                }
                                else if (casketCode.ToUpper().IndexOf("UV") == 0)
                                {
                                    dValue = funDt.Rows[i]["price"].ObjToDouble();
                                    urn += dValue;
                                }
                                else if ( casketCode.ToUpper() != "MISC" )
                                {
                                    casket = casketCode;
                                    casketAmount = funDt.Rows[i]["currentprice"].ObjToDouble();
                                    if ( casketAmount <= 0D )
                                        casketAmount = funDt.Rows[i]["price"].ObjToDouble();
                                    serialNumber = funDt.Rows[i]["SerialNumber"].ObjToString();
                                    //if (!String.IsNullOrWhiteSpace(serialNumber))
                                    //{
                                        casketCost = mDt.Rows[0]["casketcost"].ObjToDouble();
                                        casketDesc = mDt.Rows[0]["casketdesc"].ObjToString();
                                        casketGauge = getCasketGauge(serialNumber, casketCode, casketDesc, ref casketType);
                                    if (casketCode.ToUpper() == "INFANT" && vaultCost > 0D)
                                        vaultCost = 0D;

                                    //}
                                }
                            }
                            else
                            {
                                str = funDt.Rows[i]["serialNumber"].ObjToString();
                                if (!String.IsNullOrWhiteSpace(str))
                                {
                                    serialNumber = str;
                                    casketAmount = funDt.Rows[i]["currentprice"].ObjToDouble();
                                    if (casketAmount <= 0D)
                                        casketAmount = funDt.Rows[i]["price"].ObjToDouble();
                                    casketDesc = funDt.Rows[i]["service"].ObjToString();

                                    service = funDt.Rows[i]["service"].ObjToString().ToUpper();
                                    if (service.IndexOf("D-") == 0)
                                        service = service.Replace("D-", "").Trim();
                                    bateDt = G1.get_db_data("Select * from `batesville_inventory` where `casketDescription` = '" + service + "';");
                                    if (bateDt.Rows.Count > 0)
                                    {
                                        casketCode = bateDt.Rows[0]["casketCode"].ObjToString().ToUpper();
                                        if (casketCode.IndexOf("V") == 0)
                                            vaultCost = bateDt.Rows[0]["cost"].ObjToDouble();
                                        else
                                            casketCost = bateDt.Rows[0]["cost"].ObjToDouble();
                                    }
                                    else
                                    {
                                        bateDt = G1.get_db_data("Select * from `secondary_inventory` where `casketDesc` = '" + service + "';");
                                        if (bateDt.Rows.Count > 0)
                                        {
                                            str = bateDt.Rows[0]["type"].ObjToString().ToUpper();
                                            if (str == "CASKET")
                                            {
                                                casketCode = bateDt.Rows[0]["casketCode"].ObjToString();
                                                casketDesc = service;
                                                casketCost = bateDt.Rows[0]["cost"].ObjToDouble();
                                                casketGauge = bateDt.Rows[0]["casketgauge"].ObjToString();
                                            }
                                            else if (str == "VAULT")
                                            {
                                                casketCode = bateDt.Rows[0]["casketCode"].ObjToString();
                                                casketDesc = service;
                                                vaultCost = bateDt.Rows[0]["cost"].ObjToDouble();
                                                casketGauge = bateDt.Rows[0]["casketgauge"].ObjToString();
                                                vaultAmount = currentPrice;
                                            }
                                            else if (str == "URN")
                                            {
                                                casketCode = bateDt.Rows[0]["casketCode"].ObjToString();
                                                casketDesc = service;
                                                urnCost = bateDt.Rows[0]["cost"].ObjToDouble();
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    service = funDt.Rows[i]["service"].ObjToString().ToUpper();
                                    if (service.IndexOf("D-") == 0)
                                        service = service.Replace("D-", "").Trim();
                                    bateDt = G1.get_db_data("Select * from `batesville_inventory` where `casketDescription` = '" + service + "';");
                                    if ( bateDt.Rows.Count > 0 )
                                    {
                                        casketCode = bateDt.Rows[0]["casketCode"].ObjToString().ToUpper();
                                        if (casketCode.IndexOf ( "V") == 0 )
                                            vaultCost = bateDt.Rows[0]["cost"].ObjToDouble();
                                        else
                                            casketCost = bateDt.Rows[0]["cost"].ObjToDouble();
                                    }
                                    else
                                    {
                                        bateDt = G1.get_db_data("Select * from `secondary_inventory` where `casketDesc` = '" + service + "';");
                                        if (bateDt.Rows.Count > 0)
                                        {
                                            str = bateDt.Rows[0]["type"].ObjToString().ToUpper();
                                            if ( str == "CASKET")
                                            {
                                                casketCode = bateDt.Rows[0]["casketCode"].ObjToString();
                                                casketDesc = service;
                                                casket = service;
                                                casketCost = bateDt.Rows[0]["cost"].ObjToDouble();
                                                casketGauge = bateDt.Rows[0]["casketgauge"].ObjToString();
                                            }
                                            else if ( str == "VAULT")
                                            {
                                                vault = service;
                                                vaultCost = bateDt.Rows[0]["cost"].ObjToDouble();
                                                vaultAmount = currentPrice;
                                            }
                                            else if (str == "URN")
                                            {
                                                urnDesc = service;
                                                urnCost = bateDt.Rows[0]["cost"].ObjToDouble();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else if (type == "CASH ADVANCE")
                        totalCashAdvance += currentPrice; // Changed 3/13/2025 for Funeral Bonus
                    totalCurrentPrice += currentPrice;
                    totalDifference += (currentPrice - price);
                }
            }

            if ( String.IsNullOrWhiteSpace ( casketDesc ) && funDt.Rows.Count > 0 )
            {
                DataView tempview = funDt.DefaultView; // Check for Discrestionary
                tempview.Sort = "price desc";
                funDt = tempview.ToTable();

                for ( int i=0; i<funDt.Rows.Count; i++)
                {
                    type = funDt.Rows[i]["type"].ObjToString().ToUpper();
                    if ( type == "MERCHANDISE")
                    {
                        service = funDt.Rows[i]["service"].ObjToString().ToUpper();
                        if ( service.IndexOf ( "D-") == 0 )
                        {
                            casketAmount = funDt.Rows[i]["currentprice"].ObjToDouble();
                            if (casketAmount <= 0D)
                                casketAmount = funDt.Rows[i]["price"].ObjToDouble();
                            serialNumber = funDt.Rows[i]["SerialNumber"].ObjToString();
                            casketDesc = service;
                            break;
                        }
                    }
                }
            }

            totalCost = totalCurrentPrice - totalDifference;
            if (gotPackage)
                totalCost = packagePrice;
            totalCurrentPrice = newContractTotal;
            totalCost = newTotalCost;

            if (trustPaymentsReceived > trustPayments)
            {
                //compD = trustPayments - trustPaymentsReceived;
                //compD = G1.RoundValue(compD);

                //dt.Rows[i]["classa"] = dt.Rows[i]["classa"].ObjToDouble() + compD;
            }
            else
            {
                trustDiscount = trustPayments - trustPaymentsReceived - dbr; // Not used
                trustDiscount = trustAmountFiled - trustPaymentsReceived - dbr; // Not used
                trustDiscount = trustPayments - trustAmountFiled; // Not used, had to add trustFiledRemaining to get proper trust discount
                trustDiscount = trustPayments - trustPaymentsReceived - trustFiledRemaining - dbr;
                trustDiscount = trustPayments - trustPaymentsReceived - dbr;
                dValue = trustDiscount;
                dValue = Math.Abs(dValue);
                //dt.Rows[i]["trustDiscount"] = dValue;
                trustDiscount = trustPayments - trustPaymentsReceived - dbr;
            }
            //trustDiscount = trustPayments - trustPaymentsReceived - dbr;

            double insuranceDiscount = insurancePayments - insurancePaymentsReceived;
            insuranceDiscount += insuranceGrowth;

            //balanceDue = custPrice - classa - trustPaymentsReceived - cashReceived - insurancePaymentsReceived - trustDiscount - insuranceDiscount;

            classa = classA;
            balanceDue = newTotalCost - classa - trustPaymentsReceived - cashReceived - insurancePaymentsReceived - trustDiscount - insuranceDiscount - compDiscounts - totalDBR - thirdDiscount + insuranceGrowth;

            balanceDue = newTotalCost - compDiscounts - classA - trustDiscount + trustGrowth - trustPaymentsReceived - dbr - insuranceDiscount + insuranceGrowth - insurancePaymentsReceived - cashReceived;

            double TotalsbalanceDue = newTotalCost - compDiscounts - classA + trustGrowth - trustPaymentsReceived - dbr - insuranceDiscount + insuranceGrowth - insurancePaymentsReceived - cashReceived;
            if (SMFS.activeSystem.ToUpper() == "OTHER")
                TotalsbalanceDue -= trustDiscount;
            balanceDue = TotalsbalanceDue;

            totalRefund = Math.Abs(totalRefund);
            balanceDue += totalRefund;
            balanceDue = G1.RoundValue(balanceDue);

            if (balanceDue + totalAmountGrowth == 0D)
            {
                cmd = "Select * from `cust_payments` WHERE `contractNumber` = '" + contractNumber + "' AND `type` = 'Insurance Direct';";
                DataTable payDt = G1.get_db_data(cmd);
                if (payDt.Rows.Count > 0)
                    balanceDue = 0D;
            }


            //balanceDue = totalCost - totalPayments;
            totalDiscount = servicesDiscount + merchandiseDiscount;
            if (gotPackage)
            {
                if (preDiscount < packageDiscount)
                    packageDiscount = preDiscount;
                totalDiscount = packageDiscount;
                preDiscount = 0D;
            }
            if (oldDiscount != totalDiscount)
            {
            }

            string isPackage = "";
            if (gotPackage)
                isPackage = "Y";

            if (!String.IsNullOrWhiteSpace(custExtendedRecord))
            {
                //if ( !Rebalance )
                    G1.update_db_table("fcust_extended", "record", custExtendedRecord, new string[] { "casketCost", casketCost.ToString(), "vaultCost", vaultCost.ToString() });
                G1.update_db_table("fcust_extended", "record", custExtendedRecord, new string[] { "amountFiled", totalFiled.ToString(), "amountReceived", totalReceived.ToString(), "cash", cash, "check", check, "depositNumber", deposit, "balanceDue", balanceDue.ToString(), "additionalDiscount", discount.ToString(), "approvedBy", approvedBy, "creditCard", creditCard, "ccDepNumber", ccDepNumber, "checkDepNumber", chkDepNumber, "grossAmountReceived", totalGross.ObjToString(), "classa", classa.ToString(), "amountDiscount", totalAmountDiscount.ObjToString(), "amountGrowth", totalAmountGrowth.ObjToString(), "gotPackage", isPackage, "casket", casket, "vault", vault, "casketAmount", casketAmount.ToString(), "vaultAmount", vaultAmount.ToString(), "urnDesc", urnDesc, "urnCost", urnCost.ToString() });
                G1.update_db_table("fcust_extended", "record", custExtendedRecord, new string[] { "custPrice", totalCost.ToString(), "custMerchandise", totalMerchandise.ToString(), "custServices", totalServices.ToString(), "merchandiseDiscount", merchandiseDiscount.ToString(), "servicesDiscount", servicesDiscount.ToString(), "totalDiscount", totalDiscount.ToString(), "currentPrice", totalCurrentPrice.ToString(), "currentMerchandise", currentMerchandise.ToString(), "currentServices", currentServices.ToString(), "serialNumber", serialNumber, "casketdesc", casketDesc, "preneedDiscount", preDiscount.ToString(), "packageDiscount", packageDiscount.ToString(), "cashAdvance", totalCashAdvance.ToString() });
                G1.update_db_table("fcust_extended", "record", custExtendedRecord, new string[] { "trustAmountFiled", trustAmountFiled.ObjToString(), "trustAmountReceived", trustAmountReceived.ObjToString(), "insAmountFiled", insAmountFiled.ObjToString(), "insAmountReceived", insAmountReceived.ObjToString(), "casketgauge", casketGauge, "caskettype", casketType, "urn", urn.ToString(), "trustDepNumber", trustDepNumber, "insDepNumber", insDepNumber, "refund", totalRefund.ToString(), "FPC", fpc, "thirdDiscount", thirdDiscount.ToString(), "trustGrowth", trustGrowth.ToString(), "insuranceGrowth", insuranceGrowth.ToString(), "money", totalMoney.ToString() });
                G1.update_db_table("fcust_extended", "record", custExtendedRecord, new string[] { "compDiscount", compDiscounts.ToString(), "cashReceived", cashReceived.ToString(), "trustPayments", trustPayments.ToString(), "trustPaymentsReceived", trustPaymentsReceived.ToString(), "insurancePayments", insurancePayments.ToString(), "insurancePaymentsReceived", insurancePaymentsReceived.ToString(), "taxAmount", salesTax.ToString(), "taxMerchandise", taxMerchandise.ToString(), "dbr", totalDBR.ToString(), "trustFiledRemaining", trustFiledRemaining.ToString(), "asService", asService.ToString(), "asCash", asCash.ToString(), "asNothing", asNothing.ToString(), "asMerc", asMerc.ToString(), "fromService", fromService.ToString(), "fromMerc", fromMerc.ToString() });

                //cmd = "UPDATE `fcust_extended` SET `tmstamp` = CURRENT_TIMESTAMP() WHERE `record` = '" + custExtendedRecord + "'; ";
                //G1.get_db_data(cmd);

                //UpdateTimeStamp("fcust_extended", "tmstamp", custExtendedRecord);
            }
            try // Ramma Zamma
            {
                //dR["balanceDue"] = balanceDue;
                //dR["custPrice"] = totalCost;
                //dR["custMerchandise"] = totalMerchandise;
                //dR["custServices"] = totalServices;
                //dR["merchandiseDiscount"] = merchandiseDiscount;
                //dR["servicesDiscount"] = servicesDiscount;
                //dR["additionalDiscount"] = discount;
                //dR["totalDiscount"] = totalDiscount;

                //dR["amountFiled"] = totalFiled;
                //dR["amountReceived"] = totalReceived;

                //dR["currentPrice"] = totalCurrentPrice;
                //dR["currentMerchandise"] = currentMerchandise;
                //dR["currentServices"] = currentServices;

                //dR["grossAmountReceived"] = totalGross;

                //dR["amountDiscount"] = totalAmountDiscount;
                //dR["amountGrowth"] = totalAmountGrowth;
                //dR["serialNumber"] = serialNumber;
                //dR["casketdesc"] = casketDesc;

                //dR["check"] = check;
                //dR["checkDepNumber"] = chkDepNumber;


                DataTable newDt = ReplaceData (serviceId );

                if ( newDt != null )
                {
                    string field = "";
                    for ( int i=0; i<newDt.Columns.Count; i++)
                    {
                        try
                        {
                            field = newDt.Columns[i].ColumnName.Trim();
                            dR[field] = newDt.Rows[0][i];
                        }
                        catch ( Exception ex)
                        {
                        }
                    }
                    dValue = insurancePaymentsReceived;
                    dValue = Math.Abs(dValue);
                    if ( G1.is_valid_column ( dR, "insuranceAmountReceived"))
                        dR["insuranceAmountReceived"] = dValue;

                    dValue = insurancePaymentsReceived;
                    dValue = Math.Abs(dValue);
                    if (G1.is_valid_column(dR, "insurancePayments"))
                        dR["insurancePayments"] = dValue;
                    if ( SMFS.activeSystem.ToUpper() == "OTHER")
                    {
                        if ( preDiscount == 0D && packageDiscount > 0D )
                            dR["preneedDiscount"] = packageDiscount;
                    }
                    else
                    {
                        if ( preDiscount > 0D )
                        {
                        }
                        if (preDiscount == 0D && packageDiscount > 0D)
                            dR["preneedDiscount"] = packageDiscount;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** 2 " + ex.Message.ToString(), "Rebalance Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            }

            //this.Cursor = Cursors.Default;
            pleaseForm.FireEvent1();
            pleaseForm.Dispose();
            pleaseForm = null;

            DateTime stopTime = DateTime.Now;
            TimeSpan ts = stopTime - startTime;
        }
        /***********************************************************************************************/
        public static void UpdateTimeStamp ( string table, string tmstamp, string record )
        {
            string cmd = "UPDATE `" + table + "` SET `" + tmstamp + "` = CURRENT_TIMESTAMP() WHERE `record` = '" + record + "'; ";
            G1.get_db_data(cmd);
        }
        /***********************************************************************************************/
        public static DataTable ReplaceData( string serviceId )
        {
            if (String.IsNullOrWhiteSpace(serviceId))
                return null;
            DataTable dt = null;

            string contractsFile = "fcontracts";
            string customersFile = "fcustomers";
            //insurance = false;

            string cmd = "SELECT *, (SELECT `pdfimages`.`record` FROM `pdfimages` WHERE `pdfimages`.`contractNumber` = e.`contractNumber` ) AS `picRecord` FROM `fcust_extended` e LEFT JOIN `fcontracts` p ON p.`contractNumber` = e.`contractNumber` left join `fcustomers` d ON e.`contractNumber` = d.`contractNumber` LEFT JOIN `icontracts` i ON i.`contractNumber` = e.`contractNumber` LEFT JOIN `icustomers` j ON j.`contractNumber` = e.`contractNumber` WHERE ";
            cmd += " e.`serviceId` >= '" + serviceId + "' AND p.`serviceId` = '" + serviceId + "' AND d.`serviceId` = '" + serviceId + "' ";
            cmd += ";";

            dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return null;

            PreProcessData(dt);

            LoadFuneralLocations(dt);

            dt.Columns.Add("num");
            dt.Columns.Add("agreement");
            dt.Columns.Add("paid", Type.GetType("System.Double"));
            dt.Columns.Add("purchase", Type.GetType("System.Double"));
            dt.Columns.Add("contractValue", Type.GetType("System.Double"));

            //SetupAgreementIcon(dt);
            //SetupPaidUpCheck(dt);
            CalcPaid(dt);
            DetermineLapsed(dt);

            LoadManagers(dt);
            LoadArrangers(dt);

            return dt;
        }
        /****************************************************************************************/
        public static double calculateTotalPayments(string contractNumber, ref double trustPayments, ref double trustPaymentsReceived, ref double insurancePayments, ref double insurancePaymentsReceived, ref double cashReceived, ref double compDiscounts, ref double classA, ref double trustFiledRemaining, ref double thirdDiscount, ref double trustGrowth, ref double insuranceGrowth, ref double preDiscount )
        {
            trustPayments = 0D;
            trustPaymentsReceived = 0D;
            trustGrowth = 0D;
            insurancePayments = 0D;
            insurancePaymentsReceived = 0D;
            insuranceGrowth = 0D;
            cashReceived = 0D;
            compDiscounts = 0D;
            classA = 0D;
            trustFiledRemaining = 0D;
            thirdDiscount = 0D;

            string type = "";
            double price = 0D;
            double total = 0D;
            string status = "";
            double paid = 0D;
            double received = 0D;

            string record = "";
            string paymentRecord = "";
            string paymentType = "";
            DataRow[] dRows = null;

            string cmd = "Select * from `cust_payments` where `contractNumber` = '" + contractNumber + "';";
            DataTable dx = G1.get_db_data(cmd);
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                status = dx.Rows[i]["status"].ObjToString().Trim().ToUpper();
                if (status == "CANCELLED" || status == "REJECTED" || status == "PENDING" )
                    continue;
                type = dx.Rows[i]["type"].ObjToString().Trim().ToUpper();
                if (type == "DISCOUNT")
                {
                    if (status == "ACCEPT" || status == "DEPOSITED")
                    {
                        if (SMFS.activeSystem.ToUpper() == "OTHER")
                            preDiscount += dx.Rows[i]["payment"].ObjToDouble();
                        else
                            compDiscounts += dx.Rows[i]["payment"].ObjToDouble();
                    }
                }
                else if (type == "CLASS A")
                {
                    if (status == "ACCEPT" || status == "DEPOSITED" || status == "PENDING" )
                        classA += dx.Rows[i]["payment"].ObjToDouble();
                }
                else if (type == "OTHER")
                {
                    if (status == "ACCEPT" || status == "DEPOSITED")
                    {
                        cashReceived += dx.Rows[i]["payment"].ObjToDouble();
                    }
                }
                if ( SMFS.activeSystem.ToUpper() == "OTHER")
                {
                    if (status == "DEPOSITED")
                    {
                        if (type == "PAYMENTS")
                        {
                            cashReceived += dx.Rows[i]["payment"].ObjToDouble();
                        }
                        else if (type == "ADJUSTMENT")
                        {
                            compDiscounts += dx.Rows[i]["payment"].ObjToDouble();
                        }
                        else if (type == "TRUST")
                        {
                            trustPayments += dx.Rows[i]["payment"].ObjToDouble();
                            trustPaymentsReceived += dx.Rows[i]["payment"].ObjToDouble();
                        }
                        else if (type == "INSURANCE")
                        {
                            insurancePayments += dx.Rows[i]["payment"].ObjToDouble();
                            insurancePaymentsReceived += dx.Rows[i]["payment"].ObjToDouble();
                        }
                    }
                }

                //else if (type == "CREDIT CARD")
                //{
                //    if (status == "ACCEPT" || status == "DEPOSITED")
                //        cashReceived += dx.Rows[i]["payment"].ObjToDouble();
                //}
                //else if (type == "CHECK")
                //{
                //    if (status == "ACCEPT" || status == "DEPOSITED")
                //        cashReceived += dx.Rows[i]["payment"].ObjToDouble();
                //}
                //else if (type == "CASH")
                //{
                //    if (status == "ACCEPT" || status == "DEPOSITED")
                //        cashReceived += dx.Rows[i]["payment"].ObjToDouble();
                //}
            }

            cmd = "Select * from `cust_payment_details` where `contractNumber` = '" + contractNumber + "' ;";
            DataTable dt = G1.get_db_data(cmd);

            dRows = dt.Select("type='trust' AND amtActuallyReceived = '0' AND trustAmtFiled > '0'");
            if ( dRows.Length > 0 )
            {
                for ( int i=0; i<dRows.Length; i++)
                    trustFiledRemaining += dRows[i]["trustAmtFiled"].ObjToDouble();
            }

            string paidFrom = "";
            string fatherType = "";

            double trustPaid = 0D;
            double trustReceived = 0D;

            dRows = dt.Select("status='DEPOSITED' AND type='Trust'");
            if (dRows.Length > 1)
            {
                trustPaid = dRows[0]["paid"].ObjToDouble();
                trustReceived = dRows[0]["amtActuallyReceived"].ObjToDouble();
                for (int i = 1; i < dRows.Length; i++)
                {
                    paid = dRows[i]["paid"].ObjToDouble();
                    received = dRows[i]["amtActuallyReceived"].ObjToDouble();
                    trustPaid += paid;
                    trustReceived += received;
                    dRows[i]["paid"] = 0D;
                    dRows[i]["amtActuallyReceived"] = 0D;
                }
                dRows[0]["paid"] = trustPaid;
                dRows[0]["amtActuallyReceived"] = trustReceived;
            }

            bool trustIsPaid = false;
            bool insuranceIsPaid = false;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    trustIsPaid = false;
                    insuranceIsPaid = false;

                    status = dt.Rows[i]["status"].ObjToString().Trim().ToUpper();
                    if (status == "CANCELLED" || status == "REJECTED" )
                        continue;
                    type = dt.Rows[i]["type"].ObjToString().Trim().ToUpper();
                    fatherType = type;

                    if (status.ToUpper() != "DEPOSITED")
                        continue;

                    paidFrom = dt.Rows[i]["paidFrom"].ObjToString();
                    paid = dt.Rows[i]["paid"].ObjToDouble();
                    received = dt.Rows[i]["amtActuallyReceived"].ObjToDouble();

                    record = dt.Rows[i]["paymentRecord"].ObjToString();
                    dRows = dx.Select("record='" + record + "'");
                    if ( dRows.Length > 0 )
                    {
                        status = dRows[0]["status"].ObjToString().ToUpper();
                        if (status == "CANCELLED" || status == "REJECTED")
                            continue;
                        fatherType = dRows[0]["type"].ObjToString().ToUpper();
                        if ( fatherType.ToUpper() == "TRUST")
                        {
                             if (received > paid && paid == 0D)
                            {
                            }
                            if (received > paid && paid != 0D)
                            {
                                trustGrowth += received - paid;
                                trustPaymentsReceived += received;
                                trustIsPaid = true;
                            }
                        }
                        else if ( fatherType.ToUpper().IndexOf( "INSURANCE") == 0 )
                        {
                            //if (fatherType.ToUpper().IndexOf("DIRECT") > 0)
                            //{
                                if (received > paid)
                                    insuranceGrowth += received - paid;
                                //else
                                //    insuranceGrowth += received - paid;
                            //}
                        }
                        else if (fatherType.ToUpper().IndexOf("3RD PARTY") == 0)
                        {
                            //if (fatherType.ToUpper().IndexOf("DIRECT") > 0)
                            //{
                            if (received > paid)
                                insuranceGrowth += received - paid;
                            //else
                            //    insuranceGrowth += received - paid;
                            //}
                        }

                        //    if ( dRows[0]["status"].ObjToString().ToUpper() == "ACCEPT" || dRows[0]["status"].ObjToString().ToUpper() == "DEPOSITED")
                        //    {
                        //        if (dRows[0]["type"].ObjToString().ToUpper() == "CHECK" || dRows[0]["type"].ObjToString().ToUpper() == "CREDIT CARD" || dRows[0]["type"].ObjToString().ToUpper() == "CASH" )
                        //            continue;
                        //    }
                    }


                    paidFrom = dt.Rows[i]["paidFrom"].ObjToString();
                    paid = dt.Rows[i]["paid"].ObjToDouble();
                    received = dt.Rows[i]["amtActuallyReceived"].ObjToDouble();
                    if (received > 0D && paid != received)
                    {
                        paymentRecord = dt.Rows[i]["paymentRecord"].ObjToString();
                        if (!String.IsNullOrWhiteSpace(paymentRecord))
                        {
                            dRows = dx.Select("record='" + paymentRecord + "'");
                            if (dRows.Length > 0)
                            {
                                paymentType = dRows[0]["type"].ObjToString().ToUpper();
                                if (paymentType == "3RD PARTY")
                                {
                                    //thirdDiscount += paid - received;
                                    //paid = received;
                                    insurancePayments += paid;
                                    insurancePaymentsReceived += received;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (fatherType == "3RD PARTY")
                        {
                            //thirdDiscount += paid - received;
                            //paid = received;
                            insurancePayments += paid;
                            insurancePaymentsReceived += received;
                        }
                        else if ( fatherType == "TRUST")
                        {

                        }
                    }

                    if (fatherType.ToUpper() == "3RD PARTY")
                        continue;

                    if (type == "TRUST")
                    {
                        trustPayments += paid;
                        if (received == 0D)
                            received = paid;
                        if ( !trustIsPaid )
                            trustPaymentsReceived += received;
                    }
                    else if (type == "CHECK-REMOTE")
                    {
                        if (fatherType.IndexOf("INSURANCE") == 0)
                        {
                            insurancePayments += paid;
                            insurancePaymentsReceived += received;
                        }
                        else
                        {
                            if (fatherType != "TRUST")
                            {
                                if (String.IsNullOrWhiteSpace(paidFrom))
                                    cashReceived += paid;
                                else
                                    cashReceived += received;
                            }
                            else
                            {
                                trustPaymentsReceived += received;
                            }
                        }
                    }
                    else if (type == "CHECK-LOCAL")
                    {
                        if (fatherType.IndexOf("INSURANCE") == 0)
                        {
                            insurancePayments += paid;
                            insurancePaymentsReceived += received;
                        }
                        else
                        {
                            if (String.IsNullOrWhiteSpace(paidFrom))
                                cashReceived += paid;
                            else
                            {
                                if ( paidFrom.ToUpper() != "MFDA" )
                                    cashReceived += received;
                            }
                        }
                    }
                    else if (type == "CASH")
                        cashReceived += paid;
                    else if (type == "LOCKBOX DBR")
                        cashReceived += paid;
                    else if (type == "CREDIT CARD")
                        cashReceived += paid;
                    else if (type.IndexOf("INSURANCE") == 0)
                    {
                        insurancePayments += paid;
                        insurancePaymentsReceived += received;
                    }
                }
                catch (Exception ex)
                {
                }
            }
            return total;
        }
        /****************************************************************************************/
        public static void calculateTotalServices(string contractNumber, ref double contractTotal, ref double totalCost, ref double preDiscount)
        {
            //string cmd = "Select * from `fcust_services` where `contractNumber` = '" + contractNumber + "';";
            //DataTable dt = G1.get_db_data(cmd);

            //if (G1.get_column_number(dt, "select") < 0)
            //    dt.Columns.Add("select");
            //for (int i = 0; i < dt.Rows.Count; i++)
            //    dt.Rows[i]["select"] = dt.Rows[i]["pSelect"].ObjToString();

            //dt.Columns["pastPrice"].ColumnName = "currentprice";

            FunServices serviceForm = new FunServices(contractNumber);
            DataTable dt = serviceForm.funServicesDT.Copy();

            contractTotal = 0D;
            totalCost = 0D;
            preDiscount = 0D;

            FunServices.CalcTotalServices(dt, ref contractTotal, ref totalCost, ref preDiscount, true);
        }
        /***********************************************************************************************/
        public static string getCasketGauge ( string serialNumber, string casketCode, string casketDesc, ref string caskettype )
        {
            bool gotit = false;
            string casketGauge = "";
            caskettype = "";
            string cmd = "SELECT* FROM inventorylist WHERE casketdesc = '" + casketDesc + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                casketGauge = dt.Rows[0]["casketguage"].ObjToString();
                caskettype = dt.Rows[0]["caskettype"].ObjToString();
            }
            else
            {
                if ( !string.IsNullOrWhiteSpace ( casketCode ))
                {
                    cmd = "SELECT* FROM inventorylist WHERE casketcode = '" + casketCode + "';";
                    dt = G1.get_db_data(cmd);
                    if (dt.Rows.Count > 0)
                    {
                        casketGauge = dt.Rows[0]["casketguage"].ObjToString();
                        caskettype = dt.Rows[0]["caskettype"].ObjToString();
                        gotit = true;
                    }
                }
                if (!String.IsNullOrWhiteSpace(serialNumber) && !gotit )
                {
                    cmd = "SELECT* FROM inventory WHERE `SerialNumber` = '" + serialNumber + "';";
                    dt = G1.get_db_data(cmd);
                    if (dt.Rows.Count > 0)
                    {
                        casketDesc = dt.Rows[0]["CasketDescription"].ObjToString();
                        cmd = "SELECT* FROM inventorylist WHERE casketdesc = '" + casketDesc + "';";
                        dt = G1.get_db_data(cmd);
                        if (dt.Rows.Count > 0)
                        {
                            casketGauge = dt.Rows[0]["casketguage"].ObjToString();
                            casketGauge = casketGauge.Replace("\r", "");
                            caskettype = dt.Rows[0]["caskettype"].ObjToString();
                        }
                    }
                }
            }
            return casketGauge;
        }
        /***********************************************************************************************/
        private void toolStripRemoveFormat_Click(object sender, EventArgs e)
        {
            string comboName = cmbSelectColumns.Text;
            if (!String.IsNullOrWhiteSpace(comboName))
            {
                string name = "Funerals " + comboName;
                G1.RemoveLocalPreferences(LoginForm.username, name);
                foundLocalPreference = false;
            }
        }
        /***********************************************************************************************/
        private void btnSelectPosition_Click(object sender, EventArgs e)
        {
        }
        /***********************************************************************************************/
        private void gridMain_FilterEditorCreated(object sender, FilterControlEventArgs e)
        {
            e.FilterControl.BeforeShowValueEditor += new DevExpress.XtraEditors.Filtering.ShowValueEditorEventHandler(FilterControl_BeforeShowValueEditor);
        }
        /***********************************************************************************************/
        void FilterControl_BeforeShowValueEditor(object sender, DevExpress.XtraEditors.Filtering.ShowValueEditorEventArgs e)
        {
            if ( e.PropertyName.ToUpper() == "TMSTAMP")
                e.CustomRepositoryItem = ri;
        }
        /***********************************************************************************************/
        private void casketUsageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CasketUsageReport usageForm = new CasketUsageReport();
            usageForm.Show();
        }
        /***********************************************************************************************/
        private void repPaidCheckEdit_Click(object sender, EventArgs e)
        {
            if (loading)
                return;
            gridMain.PostEditor();

            DataTable dt = (DataTable)(dgv.DataSource);
            DataRow dr = gridMain.GetFocusedDataRow();
            string contract = dr["contractNumber"].ObjToString();
            string value = dr["paidInFull"].ObjToString();
            string record = dr["record"].ObjToString();
            int row = gridMain.GetDataSourceRowIndex(gridMain.FocusedRowHandle);
            if (value == "0")
                value = "1";
            else
                value = "0";
            dr["paidInFull"] = value;
            dt.Rows[row]["paidInFull"] = value;
            dgv.DataSource = dt;
            G1.update_db_table("fcust_extended", "record", record, new string[] { "paidInFull", value });
            gridMain.PostEditor();
            gridMain.RefreshData();
            gridMain.RefreshEditor(true);
            dgv.Refresh();
        }
        /***********************************************************************************************/
        private void repPaidCheckEdit_CheckStateChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            gridMain.PostEditor();
        }
        /***********************************************************************************************/
        private DataTable FilterTrustClaims(DataTable dt)
        {
            if (dt == null)
                return dt;
            string contractNumber = "";
            string serviceId = "";
            string serviceId1 = "";
            for (int i = dt.Rows.Count - 1; i >= 0; i--)
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                if ( contractNumber == "SX221474")
                {
                }
                serviceId = dt.Rows[i]["serviceId"].ObjToString();
                serviceId1 = dt.Rows[i]["serviceId1"].ObjToString();
                if (String.IsNullOrWhiteSpace(serviceId))
                    dt.Rows.RemoveAt(i);
                else if (serviceId != serviceId1)
                    dt.Rows.RemoveAt(i);
            }
            return dt;
        }
        /***********************************************************************************************/
        private void trustsClaimsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AR arForm = new AR( "All");
            arForm.Show();
        }
        /***********************************************************************************************/
        private void outsideTrustClaimsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AROutsideTrusts arForm = new AROutsideTrusts("Outside Trust Claims");
            arForm.Show();
        }
        /***********************************************************************************************/
        private void gridMain_CalcRowHeight(object sender, RowHeightEventArgs e)
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
                    if (name == "CASH" || name == "DEPOSITNUMBER" || name == "CREDIT CARD" || name == "CCDEPOSITNUMBER")
                        doit = true;
                    if ( doit )
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
        private void chkLockPanel_CheckedChanged(object sender, EventArgs e)
        {
            if (chkLockPanel.Checked)
                gridBand1.Fixed = DevExpress.XtraGrid.Columns.FixedStyle.Left;
            else
                gridBand1.Fixed = DevExpress.XtraGrid.Columns.FixedStyle.None;

        }
        /***********************************************************************************************/
        private void chkUseDates_CheckedChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            loading = true;
            chkUseDates.Checked = true;
            chkDeceasedDate.Checked = false;
            chkCaseCreateDate.Checked = false;
            chkBalanceDue.Checked = false;
            loading = false;
        }
        /***********************************************************************************************/
        private void chkDeceasedDate_CheckedChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            loading = true;
            chkUseDates.Checked = false;
            chkDeceasedDate.Checked = true;
            chkCaseCreateDate.Checked = false;
            chkBalanceDue.Checked = false;
            loading = false;
        }
        /***********************************************************************************************/
        private void chkCaseCreateDate_CheckedChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            loading = true;
            chkUseDates.Checked = false;
            chkDeceasedDate.Checked = false;
            chkCaseCreateDate.Checked = true;
            chkBalanceDue.Checked = false;
            loading = false;
        }
        /***********************************************************************************************/
        private void chkBalanceDue_CheckedChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            loading = true;
            chkUseDates.Checked = false;
            chkDeceasedDate.Checked = false;
            chkCaseCreateDate.Checked = false;
            chkBalanceDue.Checked = true;
            loading = false;
        }
        /***********************************************************************************************/
        private void goodsAndServicesContractToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            if (dr == null)
                return;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            string workContract = dr["contractNumber"].ObjToString();
            string serviceId = dr["serviceId"].ObjToString();

            this.Cursor = Cursors.WaitCursor;

            string cmd = "Select * from `cust_payments` where `contractNumber` = '" + workContract + "';";
            DataTable workPaymentsDt = G1.get_db_data(cmd);
            workPaymentsDt.Columns.Add("contractValue", Type.GetType("System.Double"));

            FunServices serviceForm = new FunServices(workContract);
            DataTable workServicesDt = serviceForm.funServicesDT;

            string trust = "";
            string loc = "";
            Trust85.decodeContractNumber(serviceId, true, ref trust, ref loc);
            LoginForm.activeFuneralHomeKeyCode = loc;

            cmd = "Select * from `funeralhomes` where `atneedcode` = '" + LoginForm.activeFuneralHomeKeyCode + "';";
            DataTable dt = G1.get_db_data(cmd);
            if ( dt.Rows.Count <= 0 )
            {
                if (!String.IsNullOrWhiteSpace(LoginForm.activeFuneralHomeKeyCode))
                {
                    DataTable funDt = G1.get_db_data("Select * from `funeralHomes` WHERE `keycode` = '" + LoginForm.activeFuneralHomeKeyCode + "';");
                    if (funDt.Rows.Count <= 0)
                    {
                        funDt = G1.get_db_data("Select * from `funeralHomes` WHERE `merchandisecode` = '" + LoginForm.activeFuneralHomeKeyCode + "';");
                        if (funDt.Rows.Count <= 0)
                        {
                            bool doit = true;
                            cmd = "Select * from `fcust_extended` WHERE `serviceId` = '" + serviceId + "';";
                            funDt = G1.get_db_data(cmd);
                            if (funDt.Rows.Count > 0)
                            {
                                if (funDt.Rows[0]["OpenCloseFuneral"].ObjToString().ToUpper() == "Y")
                                {
                                    doit = false;
                                    Trust85.decodeContractNumber(serviceId, true, ref trust, ref loc);
                                    LoginForm.activeFuneralHomeKeyCode = loc;
                                    //OpenCloseFuneral = true;
                                }
                            }
                            if (doit)
                            {
                                using (FuneralHomeSelect funSelect = new FuneralHomeSelect())
                                {
                                    funSelect.ShowDialog();
                                }
                            }
                        }
                        else
                        {
                            LoginForm.activeFuneralHomeKeyCode = funDt.Rows[0]["keycode"].ObjToString();
                        }
                    }
                }
            }



            PleaseWait pleaseForm = G1.StartWait("Please Wait!\nGenerating G&&S Contract!!!");

            this.Hide();
            Contract1 conActive = new Contract1(workContract, workServicesDt, workPaymentsDt, false, false, true );
            conActive.ShowDialog();
            this.Show();
            G1.StopWait(ref pleaseForm);

            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void goodsAndServicesContractToolStripMenuItem_Clickx(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            if (dr == null)
                return;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            string workContract = dr["contractNumber"].ObjToString();

            EditCust editForm = new EditCust(true, workContract);
            if (editForm != null)
                editForm.Dispose();
            editForm = null;
        }
        /***********************************************************************************************/
        private void commissionOptionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FunCommOptions funForm = new FunCommOptions();
            funForm.Show();
        }
        /***********************************************************************************************/
        private void funeralsAvailableForCommissionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            EditManagerCommissions managerForm = new EditManagerCommissions( "M" );
            managerForm.Show();
            this.Cursor = Cursors.Default;
        }
        /*******************************************************************************************/
        private string getManagerQuery()
        {
            DataRow[] dRows = null;
            //DataTable locDt = (DataTable)chkManagers.Properties.DataSource;
            string procLoc = "";
            string[] locIDs = this.chkManagers.EditValue.ToString().Split('|');
            for (int i = 0; i < locIDs.Length; i++)
            {
                if (!String.IsNullOrWhiteSpace(locIDs[i]))
                {
                    if (procLoc.Trim().Length > 0)
                        procLoc += ",";
                    procLoc += "'" + locIDs[i].Trim() + "'";
                }
            }
            return procLoc.Length > 0 ? " manager IN (" + procLoc + ") " : "";
        }
        /***********************************************************************************************/
        private void chkManagers_EditValueChanged(object sender, EventArgs e)
        {
            if (originalDt == null)
                return;
            string names = getManagerQuery();
            DataRow[] dRows = originalDt.Select(names);
            DataTable dt = originalDt.Clone();
            for (int i = 0; i < dRows.Length; i++)
                dt.ImportRow(dRows[i]);
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            this.gridMain.ExpandAllGroups();
            gridMain.OptionsView.ShowFooter = showFooters;
        }
        /*******************************************************************************************/
        private string getArrangerQuery()
        {
            DataRow[] dRows = null;
            //DataTable locDt = (DataTable)chkArrangers.Properties.DataSource;
            string procLoc = "";
            string[] locIDs = this.chkArrangers.EditValue.ToString().Split('|');
            for (int i = 0; i < locIDs.Length; i++)
            {
                if (!String.IsNullOrWhiteSpace(locIDs[i]))
                {
                    if (procLoc.Trim().Length > 0)
                        procLoc += ",";
                    procLoc += "'" + locIDs[i].Trim() + "'";
                }
            }
            return procLoc.Length > 0 ? " `Funeral Arranger` IN (" + procLoc + ") " : "";
        }
        /***********************************************************************************************/
        private void chkArrangers_EditValueChanged(object sender, EventArgs e)
        {
            if (originalDt == null)
                return;
            string names = getArrangerQuery();
            DataRow[] dRows = originalDt.Select(names);
            DataTable dt = originalDt.Clone();
            for (int i = 0; i < dRows.Length; i++)
                dt.ImportRow(dRows[i]);
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            this.gridMain.ExpandAllGroups();
            gridMain.OptionsView.ShowFooter = showFooters;
        }
        /***********************************************************************************************/
        private void commissionDirectorArrangerSetupsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            EditFunOptions funForm = new EditFunOptions();
            funForm.Show();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void burialCremationSummaryReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            BurialSummary burialForm = new BurialSummary();
            burialForm.Show();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void funeralCommissionsForArrangersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            EditManagers managerForm = new EditManagers( "A" );
            //managerForm.Text = "Edit Arrangers";
            managerForm.Show();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void contractActivityReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            ContractActivity contractForm = new ContractActivity();
            contractForm.Show();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void recalcServicesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;

            string contractNumber = "";
            DataTable funDt = null;
            FunServices funForm = null;
            string record = "";
            DateTime timeStamp = DateTime.Now;
            DateTime compareDate = new DateTime(2021, 12, 22);

            double currentPrice = 0D;
            double pastPrice = 0D;

            this.Cursor = Cursors.WaitCursor;

            for ( int i=0; i<dt.Rows.Count; i++)
            {
                Application.DoEvents();

                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();

                contractNumber = "B11316";
                try
                {
                    funForm = new FunServices(contractNumber);
                    funDt = funForm.funServicesDT;
                    for (int j = 0; j < funDt.Rows.Count; j++)
                    {
                        pastPrice = funDt.Rows[j]["pastPrice"].ObjToDouble();
                        if (pastPrice > 0D)
                            continue;
                        timeStamp = funDt.Rows[j]["tmstamp"].ObjToDateTime();
                        if (timeStamp <= compareDate)
                        {
                            record = funDt.Rows[j]["record"].ObjToString();
                            currentPrice = funDt.Rows[j]["currentPrice"].ObjToDouble();
                            if ( currentPrice > 0D)
                                G1.update_db_table("fcust_services", "record", record, new string[] { "pastPrice", currentPrice.ToString() });
                        }
                    }
                    funDt.Dispose();
                    funDt = null;

                    funForm.Dispose();
                    funForm = null;
                }
                catch ( Exception ex )
                {
                }
                if (1 == 1)
                    break;
            }
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void classAInsuranceReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            ClassAInsuranceReport reportForm = new ClassAInsuranceReport();
            reportForm.Show();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void MatchTables ()
        {
            if (!G1.oldCopy)
                return;
            MatchTable("fcust_services");
            MatchTable("fcust_extended");
            MatchTable("cust_payments");
            MatchTable("cust_payment_details");
            MatchTable("cust_payment_outside");
            MatchTable("cust_payment_ins_checklist");
        }
        /***********************************************************************************************/
        private void MatchTable ( string table )
        {
            G1.oldCopy = false;
            string cmd = "SHOW COLUMNS FROM `" + table + "`;";
            DataTable dt = G1.get_db_data(cmd);
            if (dt == null || dt.Rows == null || dt.Rows.Count == 0)
                return; // Somehow the table does not exist

            G1.oldCopy = true;
            cmd = "SHOW COLUMNS FROM `" + table + "`;";
            DataTable rx = G1.get_db_data(cmd);
            if (rx == null || rx.Rows == null || rx.Rows.Count == 0)
                return; // Somehow the table does not exist

            string field = "";
            string type = "";
            string isNull = "";
            string fDefault = "";
            string newType = "";
            string length = "";
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                field = dt.Rows[i]["Field"].ObjToString();
                type = dt.Rows[i]["Type"].ObjToString();
                isNull = dt.Rows[i]["Null"].ObjToString();
                fDefault = dt.Rows[i]["Default"].ObjToString();
                DataRow[] dRows = rx.Select("Field='" + field + "'");
                if ( dRows.Length <= 0 )
                {
                    DecodeType(type, ref newType, ref length);
                    string newstr = "";
                    if ( String.IsNullOrWhiteSpace ( length))
                        newstr = "alter table `" + table + "` add `" + field + "` " + newType + " NOT NULL DEFAULT '" + fDefault + "';";
                    else
                        newstr = "alter table `" + table + "` add `" + field + "` " + newType + " (" + length + ") NOT NULL DEFAULT '" + fDefault + "';";
                    try
                    {
                        DataTable ddx = G1.get_db_data(newstr);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("***ERROR 1*** Adding New Field " + field + " for Table " + table + " " + ex.Message.ToString());
                    }
                }
            }
        }
        /***********************************************************************************************/
        private void DecodeType ( string type, ref string newType, ref string length )
        {
            length = "";
            newType = "";
            if (type.IndexOf("(") > 0)
            {
                int idx = type.IndexOf("(");
                newType = type.Substring(0, idx);
                type = type.Substring(idx);
                type = type.Replace("(", "");
                type = type.Replace(")", "");
                type = type.Trim();
                length = type.Trim();
            }
            else
                newType = type;
        }
        /***********************************************************************************************/
        private void txtServiceId_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string serviceId = txtServiceId.Text.Trim();
                if (!String.IsNullOrWhiteSpace(serviceId))
                    btnRefresh_Click(null, null);
            }
        }
        /***********************************************************************************************/
        private void funeralActivityReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FuneralActivityReport funeralForm = new FuneralActivityReport();
            funeralForm.Show();
        }
        /***********************************************************************************************/
        private void salesTaxReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SalesTaxReport salesForm = new SalesTaxReport();
            salesForm.Show();
        }
        /***********************************************************************************************/
        private void agentFamilyReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                AgentProspectReport agentForm = new AgentProspectReport();
                agentForm.Show();
            }
            catch ( Exception ex )
            {
            }
        }
        /***********************************************************************************************/
        private void menuVaultUsage_Click(object sender, EventArgs e)
        {
            bool vaultUsage = true;
            CasketUsageReport usageForm = new CasketUsageReport( vaultUsage );
            usageForm.Show();
        }
        /***********************************************************************************************/
        private void funeralDelaysToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            FuneralDelays funForm = new FuneralDelays();
            funForm.Show();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void menuUrnUsage_Click(object sender, EventArgs e)
        {
            bool vaultUsage = false;
            bool urnUsage = true;
            CasketUsageReport usageForm = new CasketUsageReport ( vaultUsage, urnUsage );
            usageForm.Show();
        }
        /***********************************************************************************************/
        private void fullRelativesReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            FullRelativesReport fullForm = new FullRelativesReport();
            fullForm.Show();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void burialDetailReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            BurialDetailReport fullForm = new BurialDetailReport();
            fullForm.Show();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void funeralArrangerTotalsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            FunArrangerTotals funForm = new FunArrangerTotals();
            funForm.Show();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void testToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FunPayments.GeneratePayerDeathLapse("CC-5414");
        }
        /***********************************************************************************************/
        private void burialCremationSummaryReportTestToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            BurialSummaryTest burialForm = new BurialSummaryTest();
            burialForm.Show();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
    }
}
