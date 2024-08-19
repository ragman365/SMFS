using DevExpress.CodeParser;
using DevExpress.Pdf;
using DevExpress.Utils;
using DevExpress.Utils.Drawing;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraEditors.ViewInfo;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Controls;
using DevExpress.XtraGrid.Views.BandedGrid;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports.UI;
using GeneralLib;
using System;
using System.Data;
using System.Drawing;
using System.Web.UI.WebControls;
using System.Windows.Forms;
using GridView = DevExpress.XtraGrid.Views.Grid.GridView;
using iTextSharp.text.pdf;
using System.IO;
using System.Text.RegularExpressions;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class FunManager : DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        private DataTable workDt = null;
        private DataTable originalDt = null;
        private string workReport = "";
        private string workManager = "";
        private string workArranger = "";
        private string workWho = "";
        private string workArrangerFirstName = "";
        private string workArrangerLastName = "";
        private DataTable oDt = null;
        private bool loading = false;
        private bool insurance = false;
        private bool foundLocalPreference = false;
        private GridView originalGridView = null;
        private bool showFooters = true;
        private string serviceLocs = "";
        private bool workPDF = false;
        /***********************************************************************************************/
        public FunManager( DataTable dt, string manager, string who )
        {
            InitializeComponent();
            //workReport = report;
            workDt = dt;
            workManager = manager;
            workWho = who;
        }
        /***********************************************************************************************/
        RepositoryItemDateEdit ri = null;
        private void FunManager_Load(object sender, EventArgs e)
        {
            loading = true;

            barImport.Hide();

            btnRunCommission.Hide();

            ri = new RepositoryItemDateEdit();
            ri.VistaEditTime = DevExpress.Utils.DefaultBoolean.True;
            ri.Mask.MaskType = DevExpress.XtraEditors.Mask.MaskType.DateTime;
            ri.Mask.UseMaskAsDisplayFormat = true;
            ri.Mask.EditMask = @"yyyy-MM-dd hh-mm";

            string name = G1.GetUserFullName();

            string title = "Funeral Commission for ";
            if (workDt == null)
            {
                if (workWho.ToUpper() == "M")
                    title += " " + workManager + " as Manager";
                else if (workWho.ToUpper() == "A")
                {
                    if ( !String.IsNullOrWhiteSpace ( workArranger ))
                        title += " " + workArranger + " as Arranger";
                    else
                        title += " " + workManager + " as Arranger";
                }
            }
            else
            {
                btnRefresh.Hide();
                btnRunCommission.Show();
                btnRunCommission.Text = "Run Commissions";
                string ma = workDt.Rows[0]["ma"].ObjToString();
                if (ma.ToUpper() == "M")
                    title += "All Managers";
                else if (ma.ToUpper() == "A")
                    title += "All Arrangers";
            }

            this.Text = title;

            showFooters = true;
            string preference = G1.getPreference(LoginForm.username, "Funerals CB Chooser", "Allow Access");
            if (preference != "YES")
                showFooters = false;

            string prefix = "";
            string suffix = "";
            string firstName = "";
            string lastName = "";
            string mi = "";

            G1.ParseOutName(workManager, ref prefix, ref firstName, ref lastName, ref mi, ref suffix);

            if (!String.IsNullOrWhiteSpace(firstName))
                workArrangerFirstName = firstName;
            if (!String.IsNullOrWhiteSpace(lastName))
                workArrangerLastName = lastName;

            workArranger = firstName + " " + lastName;

            loadLocatons();

            SetupTotalsSummary();

            cmbReportType.Text = workReport;
            loading = false;

            DateTime today = DateTime.Now;
            int days = DateTime.DaysInMonth(today.Year, today.Month);
            this.dateTimePicker2.Value = new DateTime(today.Year, today.Month, days);
            this.dateTimePicker1.Value = new DateTime(today.Year, today.Month, 1);

            this.Refresh();
            gridMain.RefreshEditor(true);

            //cmbSelectColumns_SelectedIndexChanged(cmbSelectColumns, null);

            gridMain.ShowCustomizationForm += GridMain_ShowCustomizationForm;


            if (showFooters)
            {
                //this.gridMain.GroupSummary.AddRange(new DevExpress.XtraGrid.GridSummaryItem[]
                //{
                //new DevExpress.XtraGrid.GridGroupSummaryItem(DevExpress.Data.SummaryItemType.Sum, "balanceDue", this.balanceDue6, "${0:0,0.00}"),
                //new DevExpress.XtraGrid.GridGroupSummaryItem(DevExpress.Data.SummaryItemType.Sum, "grossAmountReceived", this.grossAmountReceived12, "${0:0,0.00}"),
                //new DevExpress.XtraGrid.GridGroupSummaryItem(DevExpress.Data.SummaryItemType.Sum, "amountDiscount", this.amountDiscount6, "${0:0,0.00}"),
                //new DevExpress.XtraGrid.GridGroupSummaryItem(DevExpress.Data.SummaryItemType.Sum, "trustAmountFiled", this.trustAmountFiled2, "${0:0,0.00}"),
                //new DevExpress.XtraGrid.GridGroupSummaryItem(DevExpress.Data.SummaryItemType.Sum, "trustAmountReceived", this.trustAmountReceived12, "${0:0,0.00}"),
                //new DevExpress.XtraGrid.GridGroupSummaryItem(DevExpress.Data.SummaryItemType.Sum, "insAmountFiled", this.insAmountFiled6, "${0:0,0.00}"),
                //new DevExpress.XtraGrid.GridGroupSummaryItem(DevExpress.Data.SummaryItemType.Sum, "insAmountReceived", this.insAmountReceived18, "${0:0,0.00}"),
                //new DevExpress.XtraGrid.GridGroupSummaryItem(DevExpress.Data.SummaryItemType.Sum, "amountGrowth1", this.amountGrowth12, "${0:0,0.00}")});
            }

            SetAllClaims();

            SetupServiceLocs();
        }
        /****************************************************************************************/
        private void SetupServiceLocs ()
        {
            serviceLocs = "";
            DataTable funDt = G1.get_db_data("Select * from `funeralhomes` where `manager` = '" + workManager + "';");
            if (funDt.Rows.Count <= 0)
                return;

            string atNeedCode = "";
            for ( int i=0; i<funDt.Rows.Count; i++)
            {
                atNeedCode = funDt.Rows[i]["atneedcode"].ObjToString();
                if ( !String.IsNullOrWhiteSpace ( atNeedCode ))
                {
                    if (!String.IsNullOrWhiteSpace(serviceLocs))
                        serviceLocs += ",";
                    serviceLocs += "'" + atNeedCode + "'";
                }
            }
        }
        /****************************************************************************************/
        private void SetAllClaims()
        {
            if (!chkSingle.Checked)
                return;
            //if (1 == 1)
            //    return;
            //ClearAllPositions(gridMain);
            int i = 0;
            G1.SetColumnPosition(gridMain, "num", ++i, 30 );
            G1.SetColumnPosition(gridMain, "serviceId", ++i, 40 );
            G1.SetColumnPosition(gridMain, "name", ++i, 100 );
            G1.SetColumnPosition(gridMain, "deceasedDate", ++i, 55);
            G1.SetColumnPosition(gridMain, "funeralType", ++i, 40);
            G1.SetColumnPosition(gridMain, "netFuneral", ++i, 55);
            G1.SetColumnPosition(gridMain, "gotPackage", ++i, 40);
            G1.SetColumnPosition(gridMain, "casket", ++i, 40);
            G1.SetColumnPosition(gridMain, "vault", ++i, 30);
            G1.SetColumnPosition(gridMain, "currentServices", ++i, 60);
            G1.SetColumnPosition(gridMain, "casketAmount", ++i, 60);
            G1.SetColumnPosition(gridMain, "urn", ++i, 40);
            G1.SetColumnPosition(gridMain, "vaultAmount", ++i, 60);
            G1.SetColumnPosition(gridMain, "cashAdvance", ++i, 60);
            G1.SetColumnPosition(gridMain, "currentPrice", ++i, 70);
            G1.SetColumnPosition(gridMain, "classa", ++i, 60);
            G1.SetColumnPosition(gridMain, "totalDiscount", ++i, 70);
            G1.SetColumnPosition(gridMain, "endingBalance", ++i, 70);
            G1.SetColumnPosition(gridMain, "balanceDue", ++i, 60);
            G1.SetColumnPosition(gridMain, "casketgauge", ++i, 50);
            G1.SetColumnPosition(gridMain, "caskettype", ++i, 50);
            G1.SetColumnPosition(gridMain, "SRVLOC", ++i, 25);
            G1.SetColumnPosition(gridMain, "casketCost", ++i, 60);
            G1.SetColumnPosition(gridMain, "vaultCost", ++i, 60);
            G1.SetColumnPosition(gridMain, "upgrade", ++i, 50);
            G1.SetColumnPosition(gridMain, "premiumType", ++i, 40);
            G1.SetColumnPosition(gridMain, "amountReceived", ++i, 50);
            //G1.SetColumnPosition(gridMain, "contractNumber", ++i, 60);

            gridMain.Columns["empty"].Visible = false;

            gridMain.RefreshEditor(true);
        }
        /****************************************************************************************/
        private void ClearAllPositions(DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gMain = null)
        {
            if (gMain == null)
                gMain = gridMain;
            string name = "";
            for (int i = 0; i < gMain.Columns.Count; i++)
            {
                name = gMain.Columns[i].Name.ToUpper();
                if ( name != "NUM" )
                    gMain.Columns[i].Visible = false;
                else
                    gMain.Columns[i].Visible = true;
            }
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
            AddSummaryColumn("amountReceived", null);
            AddSummaryColumn("amountFiled", null);
            AddSummaryColumn("custPrice", null);
            AddSummaryColumn("custMerchandise", null);
            AddSummaryColumn("custServices", null);
            AddSummaryColumn("totalDiscount", null);
            AddSummaryColumn("currentPrice", null);
            AddSummaryColumn("currentMerchandise", null);
            //AddSummaryColumn("currentServices", null);
            AddSummaryColumn("balanceDue", null);
            AddSummaryColumn("additionalDiscount", null);
            AddSummaryColumn("classa", null);
            AddSummaryColumn("grossAmountReceived", null);
            AddSummaryColumn("amountDiscount", null);
            AddSummaryColumn("amountGrowth1", null);
            AddSummaryColumn("cashAdvance", null);
            AddSummaryColumn("trustAmountFiled", null);
            AddSummaryColumn("trustAmountReceived", null);
            AddSummaryColumn("insAmountFiled", null);
            AddSummaryColumn("insAmountReceived", null);
            AddSummaryColumn("trustPayment", null);
            AddSummaryColumn("netFuneral", null);
            AddSummaryColumn("cashCheck", null);
            AddSummaryColumn("cc", null);
            AddSummaryColumn("casketAmount", null);
            AddSummaryColumn("vaultAmount", null);
            AddSummaryColumn("casketCost", null);
            AddSummaryColumn("vaultCost", null);
            AddSummaryColumn("endingBalance", null);
            AddSummaryColumn("upgrade", null);

            gridMain.Columns["cash"].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Custom;
            gridMain.Columns["cash"].SummaryItem.DisplayFormat = "{0:N2}";
            gridMain.Columns["creditCard"].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Custom;
            gridMain.Columns["creditCard"].SummaryItem.DisplayFormat = "{0:N2}";


            //AddSummaryColumn("currentprice", null);
            //AddSummaryColumn("difference", null);
        }
        /****************************************************************************************/
        private void AddSummaryColumn(string columnName, DevExpress.XtraGrid.Views.Grid.GridView gMain = null)
        {
            if (gMain == null)
                gMain = gridMain;
            gMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            gMain.Columns[columnName].SummaryItem.DisplayFormat = "{0:N2}";
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
            return procLoc.Length > 0 ? " serviceLoc IN (" + procLoc + ") " : "";
        }
        /***********************************************************************************************/
        private DataTable LoadData() // Ramma Zamma
        {
            this.Cursor = Cursors.WaitCursor;

            DataTable dt = null;

            string contractsFile = "fcontracts";
            string customersFile = "fcustomers";
            insurance = false;

            workReport = cmbReportType.Text.Trim().ToUpper();

            try
            {
                string cmd = "";
                //            string cmd = "SELECT * FROM `fcust_extended` e LEFT JOIN `fcontracts` p ON p.`contractNumber` = e.`contractNumber` left join `fcustomers` d ON e.`contractNumber` = d.`contractNumber` LEFT JOIN `icontracts` i ON i.`contractNumber` = e.`contractNumber` LEFT JOIN `icustomers` j ON j.`contractNumber` = e.`contractNumber` WHERE e.`ServiceID` <> '' ";
                cmd = "SELECT * FROM `fcust_extended` e LEFT JOIN `fcontracts` p ON p.`contractNumber` = e.`contractNumber` left join `fcustomers` d ON d.`contractNumber` = e.`contractNumber` LEFT JOIN `icontracts` i ON i.`contractNumber` = e.`contractNumber` LEFT JOIN `icustomers` j ON j.`contractNumber` = e.`contractNumber` ";
                //cmd += " LEFT JOIN `cust_payments` c ON c.`contractNumber` = e.`contractNumber` LEFT JOIN `cust_payment_details` x ON c.`record` = x.`paymentRecord` ";
                //cmd += " WHERE e.`ServiceID` <> '' AND ( e.`serviceLoc` IN (" + serviceLocs + ") OR e.`Funeral Arranger` LIKE '" + workArrangerFirstName + "%' AND e.`Funeral Arranger` LIKE  '%" + workArrangerLastName + "%' )";
                cmd += " WHERE e.`ServiceID` <> '' ";

                if ( !String.IsNullOrWhiteSpace ( serviceLocs ))
                    cmd += " AND ( e.`serviceLoc` IN (" + serviceLocs + ") OR e.`Funeral Arranger` LIKE '" + workArrangerFirstName + "%' AND e.`Funeral Arranger` LIKE  '%" + workArrangerLastName + "%' )";
                else
                    cmd += " AND e.`Funeral Arranger` LIKE '" + workArrangerFirstName + "%' AND e.`Funeral Arranger` LIKE  '%" + workArrangerLastName + "%' ";

                //OR e.`Funeral Arranger` LIKE 'Arthur%' AND e.`Funeral Arranger` LIKE  '%Newman%' )
                string paidInFull = cmbPaidInFull.Text;
                if (!String.IsNullOrWhiteSpace(paidInFull))
                {
                    //if (workReport == "CASH BALANCE REPORT")
                    //{
                    //    if (paidInFull.Trim().ToUpper() == "EXCLUDE PAID IN FULL")
                    //    {
                    //        cmd += " AND c.`paidInFull` <> '1' AND (c.`type`='Cash' OR c.`type`='Check' OR c.`type`='Credit Card' OR c.`type`='Class A')";
                    //    }
                    //    else if (paidInFull.Trim().ToUpper() == "PAID IN FULL ONLY")
                    //    {
                    //        cmd += " AND c.`paidInFull` = '1' AND (c.`type`='Cash' OR c.`type`='Check' OR c.`type`='Credit Card' OR c.`type`='Class A')";
                    //    }
                    //}
                    //else
                    //{
                    //    if (paidInFull.Trim().ToUpper() == "EXCLUDE PAID IN FULL")
                    //        cmd += " AND c.`paidInFull` <> '1' ";
                    //    else if (paidInFull.Trim().ToUpper() == "PAID IN FULL ONLY")
                    //        cmd += " AND c.`paidInFull` = '1' ";
                    //}
                }
                if (chkUseDates.Checked || chkDeceasedDate.Checked)
                {
                    string date1 = this.dateTimePicker1.Value.ToString("yyyy-MM-dd");
                    string date2 = this.dateTimePicker2.Value.ToString("yyyy-MM-dd");
                    if (chkDeceasedDate.Checked)
                        cmd += " AND p.`deceasedDate` >= '" + date1 + "' AND p.`deceasedDate` <= '" + date2 + "' ";
                    else
                        cmd += " AND e.`serviceDate` >= '" + date1 + "' AND e.`serviceDate` <= '" + date2 + "' ";
                }

                if (chkBalanceDue.Checked)
                    cmd += " AND e.`balanceDue` <> '0.00' ";

                //cmd += " AND e.`serviceDate` >= '2015-01-01' ";
                //string locations = getLocationQuery();
                //if (!String.IsNullOrWhiteSpace(locations))
                //    cmd += " AND " + locations;
                cmd += " ORDER BY e.`serviceDate` DESC ";
                cmd += ";";

                dt = G1.get_db_data(cmd);

                //dt.Columns.Add("casket");
                //dt.Columns.Add("vault");
                //dt.Columns.Add("serviceAmount", Type.GetType("System.Double"));
                //dt.Columns.Add("casketAmount", Type.GetType("System.Double"));
                //dt.Columns.Add("vaultAmount", Type.GetType("System.Double"));
                //dt.Columns.Add("cashAdvance", Type.GetType("System.Double"));
                dt.Columns.Add("netFuneral", Type.GetType("System.Double"));
                dt.Columns.Add("funeralType");


                workReport = cmbReportType.Text.Trim().ToUpper();
                this.Text = "Funerals for Manager " + workManager;

                string title = "Funeral Commission for ";
                if (workDt == null)
                {
                    if (workWho.ToUpper() == "M")
                        title += " " + workManager + " as Manager";
                    else if (workWho.ToUpper() == "A")
                    {
                        if (!String.IsNullOrWhiteSpace(workArranger))
                            title += " " + workArranger + " as Arranger";
                        else
                            title += " " + workManager + " as Arranger";
                    }
                    this.Text = title;
                }


                SetAllClaims();

                //Trust85.FindContract(dt, "F0372");


                PreProcessData(dt);

                //SetupPaidUpCheck(dt);

                //DetermineLapsed(dt);

                //if (workReport == "CASH BALANCE REPORT")
                //    dt = ProcessCashBalance(dt);

                G1.NumberDataTable(dt);
                originalDt = dt.Copy();
                if (workDt == null)
                {
                    dgv.DataSource = dt;
                    ScaleCells();
                }
            }
            catch ( Exception ex)
            {
            }

            this.Cursor = Cursors.Default;

            return dt;
        }
        /***********************************************************************************************/
        private DataTable trust2013Dt = null;
        private void PullTheData ()
        {
            trust2013Dt = null;
            DateTime date = this.dateTimePicker2.Value;
            string myDate = date.ToString("yyyy-MM-dd");
            string cmd = "Select * from `trust2013r` where `payDate8` <= '" + myDate + "' ORDER BY `payDate8` DESC LIMIT 1;";
            DataTable dx = G1.get_db_data(cmd);
            if ( dx.Rows.Count > 0 )
            {
                myDate = dx.Rows[0]["payDate8"].ObjToDateTime().ToString("yyyy-MM-dd");
                cmd = "Select * from `trust2013r` where `payDate8` = '" + myDate + "' ORDER BY `payDate8`;";
                trust2013Dt = G1.get_db_data(cmd);
            }
        }
        /***********************************************************************************************/
        private string GetFuneralHome ( string serviceId )
        {
            string funeralHome = "";
            if (String.IsNullOrWhiteSpace(serviceId))
                return funeralHome;

            string trust = "";
            string loc = "";
            Trust85.decodeContractNumber(serviceId, ref trust, ref loc);

            DataTable funDt = (DataTable)chkComboLocation.Properties.DataSource;
            DataRow[] dRows = funDt.Select("atneedcode='" + loc + "'");
            if (dRows.Length > 0)
                funeralHome = dRows[0]["LocationCode"].ObjToString();
            return funeralHome;
        }
        /***********************************************************************************************/
        private void PreProcessData(DataTable dt)
        {

            //PullTheData();

            string contractNumber = "";
            string lastName = "";
            string firstName = "";
            string payer = "";
            string serviceId = "";
            bool noFirstName1 = false;
            DateTime dueDate = DateTime.Now;
            string cmd = "";
            DataTable dx = null;
            double endingBalance = 0D;
            double amountDBR = 0D;
            double trustAmountFiled = 0D;
            double amountReceived = 0D;
            string type = "";
            string tbbLoc = "";
            string is2000 = "";
            string locind = "";
            string cnum = "";
            DataRow[] dRows = null;
            double balanceDue = 0D;
            double contractValue = 0D;
            double growth = 0D;
            double paid = 0D;
            string paymentRecord = "";
            string trust_policy = "";

            double casketCost = 0D;
            double vaultCost = 0D;
            double cashAdvance = 0D;
            double totalFuneral = 0D;
            double totalDiscount = 0D;
            double netFuneral = 0D;
            string funeralType = "";
            string SRVLOC = "";
            string arranger = "";
            bool isManager = false;
            bool isArranger = false;
            try
            {
                if (G1.get_column_number(dt, "name") < 0)
                    dt.Columns.Add("name");
                if (G1.get_column_number(dt, "premiumType") < 0)
                    dt.Columns.Add("premiumType");
                if (G1.get_column_number(dt, "paidUp") < 0)
                    dt.Columns.Add("paidUp");
                if (G1.get_column_number(dt, "trustee") < 0)
                    dt.Columns.Add("trustee");
                if (G1.get_column_number(dt, "endingBalance") < 0)
                    dt.Columns.Add("endingBalance", Type.GetType("System.Double"));
                if (G1.get_column_number(dt, "dbr") < 0)
                    dt.Columns.Add("dbr", Type.GetType("System.Double"));
                if (G1.get_column_number(dt, "tbbLoc") < 0)
                    dt.Columns.Add("tbbLoc");
                if (G1.get_column_number(dt, "principleReceived") < 0)
                    dt.Columns.Add("principleReceived", Type.GetType("System.Double"));
                if (G1.get_column_number(dt, "amountGrowth1") < 0)
                    dt.Columns.Add("amountGrowth1", Type.GetType("System.Double"));
                if (G1.get_column_number(dt, "notes") < 0)
                    dt.Columns.Add("notes");
                if (G1.get_column_number(dt, "names") < 0)
                    dt.Columns.Add("names");
                if (G1.get_column_number(dt, "class_a") < 0)
                    dt.Columns.Add("class_a");
                if (G1.get_column_number(dt, "amountFiled") < 0)
                    dt.Columns.Add("amountFiled", Type.GetType("System.Double"));
                if (G1.get_column_number(dt, "dateInsFiled") < 0)
                    dt.Columns.Add("dateInsFiled");
                if (G1.get_column_number(dt, "trustPayment") < 0)
                    dt.Columns.Add("trustPayment", Type.GetType("System.Double"));
                if (G1.get_column_number(dt, "netFuneral") < 0)
                    dt.Columns.Add("netFuneral", Type.GetType("System.Double"));
                if (G1.get_column_number(dt, "cashCheck") < 0)
                    dt.Columns.Add("cashCheck", Type.GetType("System.Double"));
                if (G1.get_column_number(dt, "depositNumber") < 0)
                    dt.Columns.Add("depositNumber" );
                if (G1.get_column_number(dt, "depositDate") < 0)
                    dt.Columns.Add("depositDate");
                if (G1.get_column_number(dt, "cc") < 0)
                    dt.Columns.Add("cc", Type.GetType("System.Double"));
                if (G1.get_column_number(dt, "ccDepositNumber") < 0)
                    dt.Columns.Add("ccDepositNumber");
                if (G1.get_column_number(dt, "ccDepositDate") < 0)
                    dt.Columns.Add("ccDepositDate");
                if (G1.get_column_number(dt, "remainingBalance") < 0)
                    dt.Columns.Add("remainingBalance", Type.GetType("System.Double"));
                if (G1.get_column_number(dt, "notes") < 0)
                    dt.Columns.Add("notes");

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    serviceId = dt.Rows[i]["serviceId"].ObjToString();
                    isArranger = false;
                    isManager = false;
                    SRVLOC = dt.Rows[i]["serviceLoc"].ObjToString().ToUpper();
                    dt.Rows[i]["SRVLOC"] = dt.Rows[i]["serviceLoc"].ObjToString();
                    arranger = dt.Rows[i]["Funeral Arranger"].ObjToString().ToUpper();
                    if (arranger.IndexOf(workArrangerFirstName.ToUpper()) >= 0 && arranger.IndexOf(workArrangerLastName.ToUpper()) > 0)
                        isArranger = true;
                    if (serviceLocs.ToUpper().IndexOf ( SRVLOC ) >= 0 )
                        isManager = true;
                    if (isManager && isArranger)
                        dt.Rows[i]["funeralType"] = "MA";
                    else if (isManager)
                        dt.Rows[i]["funeralType"] = "M";
                    else
                        dt.Rows[i]["funeralType"] = "A";

                    firstName = dt.Rows[i]["firstName"].ObjToString();
                    lastName = dt.Rows[i]["lastName"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(firstName) && !String.IsNullOrWhiteSpace(lastName))
                        dt.Rows[i]["name"] = lastName + ", " + firstName;
                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                    if (contractNumber.ToUpper().IndexOf("UI") > 0 || contractNumber.ToUpper().IndexOf("LI") > 0)
                        dt.Rows[i]["premiumType"] = "M";
                    else
                        dt.Rows[i]["premiumType"] = "SP";
                    dueDate = dt.Rows[i]["dueDate8"].ObjToDateTime();
                    if (dueDate.ToString("MM/dd/yyyy") == "12/31/2039")
                        dt.Rows[i]["paidUp"] = "YES";
                    else
                        dt.Rows[i]["paidUp"] = "NO";

                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString();

                    //LoadServiceDetails(dt, i);

                    //FunServices serviceForm = new FunServices(contractNumber);
                    //DataTable serviceDt = serviceForm.funServicesDT;


                    //dt.Rows[i]["trustee"] = dt.Rows[i]["paidFrom"].ObjToString();
                    //dt.Rows[i]["trustAmountFiled"] = dt.Rows[i]["trustAmtFiled"].ObjToDouble();


                    //type = dt.Rows[i]["type1"].ObjToString();
                    //if (type.ToUpper() == "GROWTH" || type == "DISCOUNT")
                    //    continue;
                    //type = dt.Rows[i]["type"].ObjToString();

                    //if (workReport == "NON-UNITY CLAIMS")
                    //{
                    //    dt.Rows[i]["localDescription"] = dt.Rows[i]["localDescription1"].ObjToString();
                    //    dt.Rows[i]["depositNumber"] = dt.Rows[i]["depositNumber2"].ObjToString();
                    //    dt.Rows[i]["dateFiled"] = G1.DTtoMySQLDT(dt.Rows[i]["dateFiled1"].ObjToDateTime());
                    //    dt.Rows[i]["trustAmountReceived"] = dt.Rows[i]["amountReceived1"].ObjToDouble();
                    //}
                    //else if (workReport == "UNITY CLAIMS")
                    //{
                    //    dt.Rows[i]["localDescription"] = dt.Rows[i]["localDescription1"].ObjToString();
                    //    dt.Rows[i]["depositNumber"] = dt.Rows[i]["depositNumber2"].ObjToString();
                    //    dt.Rows[i]["dateFiled"] = G1.DTtoMySQLDT(dt.Rows[i]["dateFiled1"].ObjToDateTime());
                    //    dt.Rows[i]["trustAmountReceived"] = dt.Rows[i]["amountReceived1"].ObjToDouble();
                    //}
                    //else if (workReport == "CASH BALANCE REPORT")
                    //{
                    //    dt.Rows[i]["localDescription"] = dt.Rows[i]["localDescription1"].ObjToString();
                    //    dt.Rows[i]["depositNumber"] = dt.Rows[i]["depositNumber2"].ObjToString();
                    //    dt.Rows[i]["dateFiled"] = G1.DTtoMySQLDT(dt.Rows[i]["dateFiled1"].ObjToDateTime());
                    //    dt.Rows[i]["trustAmountReceived"] = dt.Rows[i]["amountReceived1"].ObjToDouble();
                    //    dt.Rows[i]["totalDiscount"] = dt.Rows[i]["totalDiscount"].ObjToDouble() + dt.Rows[i]["additionalDiscount"].ObjToDouble();
                    //}

                    //if (trust2013Dt != null )
                    //{
                    //    cnum = dt.Rows[i]["contractNumber"].ObjToString();
                    //    if (String.IsNullOrWhiteSpace(cnum))
                    //        cnum = contractNumber;
                    //    //else
                    //    //    dt.Rows[i]["contractNumber"] = cnum;
                    //    dRows = trust2013Dt.Select("contractNumber='" + cnum + "'");
                    //    if (dRows.Length > 0)
                    //    {
                    //        locind = dRows[0]["locind"].ObjToString();
                    //        dt.Rows[i]["tbbLoc"] = locind;
                    //        endingBalance = dRows[0]["endingBalance"].ObjToDouble();
                    //        if (endingBalance == 0D)
                    //            endingBalance = dRows[0]["beginningBalance"].ObjToDouble();
                    //        dt.Rows[i]["endingBalance"] = endingBalance;
                    //    }
                    //    else
                    //    {
                    //        cmd = "SELECT * FROM `trust2013r` WHERE contractNumber = '" + cnum + "' AND `endingBalance` > '0.00' ORDER BY payDate8 DESC LIMIT 1;";
                    //        dx = G1.get_db_data(cmd);
                    //        if (dx.Rows.Count > 0)
                    //        {
                    //            locind = dx.Rows[0]["locind"].ObjToString();
                    //            dt.Rows[i]["tbbLoc"] = locind;
                    //            endingBalance = dx.Rows[0]["endingBalance"].ObjToDouble();
                    //            if (endingBalance == 0D)
                    //                endingBalance = dx.Rows[0]["beginningBalance"].ObjToDouble();
                    //            dt.Rows[i]["endingBalance"] = endingBalance;
                    //        }
                    //    }
                    //}
                    trustAmountFiled = dt.Rows[i]["trustAmountFiled"].ObjToDouble();
                    amountReceived = dt.Rows[i]["amountReceived"].ObjToDouble();
                    balanceDue = dt.Rows[i]["balanceDue"].ObjToDouble();
                    if ( balanceDue > 0D)
                    {
                        balanceDue = balanceDue - trustAmountFiled;
                        if (balanceDue < 0D)
                            balanceDue = 0D;
                        if ( balanceDue == 0D )
                        {
                            amountReceived += trustAmountFiled;
                            dt.Rows[i]["amountReceived"] = amountReceived;
                        }
                    }
                    dt.Rows[i]["balanceDue"] = balanceDue;
                    dt.Rows[i]["endingBalance"] = trustAmountFiled;
                    contractValue = DailyHistory.GetContractValuePlus(dt.Rows[i]);
                    dt.Rows[i]["principleReceived"] = contractValue - balanceDue;

                    casketCost = dt.Rows[i]["casketCost"].ObjToDouble();
                    vaultCost = dt.Rows[i]["vaultCost"].ObjToDouble();
                    cashAdvance = dt.Rows[i]["cashAdvance"].ObjToDouble();
                    totalFuneral = dt.Rows[i]["currentPrice"].ObjToDouble();
                    totalDiscount = dt.Rows[i]["totalDiscount"].ObjToDouble();
                    netFuneral = totalFuneral - totalDiscount - casketCost - vaultCost - cashAdvance;
                    if (netFuneral <= 0D)
                        netFuneral = 0D;
                    dt.Rows[i]["netFuneral"] = netFuneral;



                    //dt.Rows[i]["notes"] = "kads sadkads adsksad dsakdsa adskads adskads adskads adskads adskads adskads adskads adskads adskads adsk adsadsk";

                }
                //DataRow[] adjustRows = dt.Select("type1='discount' OR type1='growth'");
                //if (adjustRows.Length > 0)
                //{
                //    growth = 0D; 
                //    dx = adjustRows.CopyToDataTable();
                //    for (int i = 0; i < adjustRows.Length; i++)
                //    {
                //        type = adjustRows[i]["type"].ObjToString();
                //        paid = adjustRows[i]["paid"].ObjToDouble();
                //        paymentRecord = adjustRows[i]["paymentRecord"].ObjToString();
                //        if (!String.IsNullOrWhiteSpace(paymentRecord))
                //        {
                //            dRows = dt.Select("paymentRecord='" + paymentRecord + "' and type = 'TRUST' AND (type1 <> 'GROWTH' AND type1 <> 'DISCOUNT')");
                //            if (dRows.Length > 0)
                //            {
                //                type = adjustRows[i]["type1"].ObjToString();
                //                growth = dRows[0]["growthReceived"].ObjToDouble();
                //                if (type.ToUpper() == "GROWTH")
                //                    growth += paid;
                //                else if (type.ToUpper() == "DISCOUNT")
                //                    growth -= paid;
                //                dRows[0]["growthReceived"] = growth;
                //                adjustRows[i]["paymentRecord"] = -1;
                //            }
                //        }
                //    }
                //    for ( int i=dt.Rows.Count-1; i>=0; i--)
                //    {
                //        paymentRecord = dt.Rows[i]["paymentRecord"].ObjToString();
                //        if (paymentRecord == "-1" )
                //            dt.Rows.RemoveAt(i);
                //    }
                //}
            }
            catch ( Exception ex)
            {
            }
        }
        /***********************************************************************************************/
        private void LoadServiceDetails ( DataTable dt, int i )
        {
            try
            {
                string contractNumber = dt.Rows[i]["contractNumber"].ObjToString();

                if (String.IsNullOrWhiteSpace(contractNumber))
                    return;

                FunServices serviceForm = new FunServices(contractNumber);
                DataTable serviceDt = serviceForm.funServicesDT;
                if (serviceDt.Rows.Count <= 0)
                    return;

                string merchandise = "";
                string cmd = "";
                string type = "";
                DataTable mDt = null;
                string casketCode = "";
                string ignore = "";
                string[] Lines = null;

                double totalServices = 0D;
                double totalMerchandise = 0D;
                double cashAdvance = 0D;
                double upgrade = 0D;
                double price = 0D;

                for (int j = 0; j < serviceDt.Rows.Count; j++)
                {
                    ignore = serviceDt.Rows[j]["ignore"].ObjToString().ToUpper();
                    if (ignore == "Y")
                        continue;
                    type = serviceDt.Rows[j]["type"].ObjToString().ToUpper();
                    upgrade = serviceDt.Rows[j]["upgrade"].ObjToDouble();
                    price = serviceDt.Rows[j]["price"].ObjToDouble();
                    price = Math.Abs(price);

                    if (type == "CASH ADVANCE")
                        cashAdvance += price;
                    else if (type == "SERVICE")
                        totalServices += price;
                    else if (type == "MERCHANDISE")
                        totalMerchandise += price;
                }

                dt.Rows[i]["serviceAmount"] = totalServices;
                dt.Rows[i]["cashAdvance"] = cashAdvance;
                dt.Rows[i]["totalFuneral"] = totalServices + totalMerchandise + cashAdvance;

                DataRow[] dRows = serviceDt.Select("type='Merchandise'");
                if (dRows.Length <= 0)
                    return;
                try
                {
                    DataTable sDt = dRows.CopyToDataTable();
                    for (int j = 0; j < sDt.Rows.Count; j++)
                    {
                        merchandise = sDt.Rows[j]["service"].ObjToString();
                        if (merchandise.ToUpper().IndexOf("ACKNOW") >= 0)
                            continue;
                        if (merchandise.ToUpper().IndexOf("GRAVE MARKER") >= 0)
                            continue;
                        if (merchandise.ToUpper().IndexOf("REGISTER BOOK") >= 0)
                            continue;
                        if (merchandise.ToUpper().IndexOf("V") == 0)
                        {
                            Lines = merchandise.Split(' ');
                            merchandise = merchandise.Replace(Lines[0].Trim(), "").Trim();
                        }
                        cmd = "SELECT * FROM `casket_master` WHERE `casketDesc` = '" + merchandise + "';";
                        mDt = G1.get_db_data(cmd);
                        if (mDt.Rows.Count <= 0)
                        {
                            Lines = merchandise.Split(' ');
                            merchandise = merchandise.Replace(Lines[0].Trim(), "").Trim();
                            cmd = "SELECT * FROM `casket_master` WHERE `casketDesc` = '" + merchandise + "';";
                            mDt = G1.get_db_data(cmd);
                            if ( mDt.Rows.Count <= 0 )
                                continue;
                        }
                        casketCode = mDt.Rows[0]["casketcode"].ObjToString();
                        if (casketCode.ToUpper().IndexOf("V") == 0)
                        {
                            dt.Rows[i]["vault"] = casketCode;
                            dt.Rows[i]["vaultAmount"] = sDt.Rows[j]["price"].ObjToDouble();
                        }
                        else
                        {
                            dt.Rows[i]["casket"] = casketCode;
                            dt.Rows[i]["casketAmount"] = sDt.Rows[j]["price"].ObjToDouble();
                        }
                    }
                }
                catch (Exception ex)
                {
                }
            }
            catch ( Exception ex)
            {
            }
        }
        /***********************************************************************************************/
        private DataTable ProcessCashBalance ( DataTable dt )
        {
            DataView tempview = dt.DefaultView;
            tempview.Sort = "contractNumber";
            dt = tempview.ToTable();

            string lastContract = "";
            string contractNumber = "";
            string type = "";
            string name = "";
            string record = "";
            string records = "";
            string trust_policy = "";
            string classa = "";
            double classaTotal = 0D;
            double dValue = 0D;
            int lastRow = -1;
            double preneedDiscount = 0D;
            double packageDiscount = 0D;
            string discountType = "";
            string reason = "";
            string status = "";
            string dateInsFiled = "";
            double amountFiled = 0D;
            double trustPayment = 0D;
            double netFuneral = 0D;
            double cashCheck = 0D;
            double creditCard = 0D;
            double remainingBalance = 0D;
            string str = "";
            string depositNumber = "";
            string depositDate = "";
            string ccDepositNumber = "";
            string ccDepositDate = "";
            string notes = "";
            DateTime date = DateTime.Now;

            if (G1.get_column_number(dt, "discountType") < 0)
                dt.Columns.Add("discountType");

            if (G1.get_column_number(dt, "records") < 0)
                dt.Columns.Add("records");

            Trust85.FindContract(dt, "F0372");

            for ( int i=0; i<dt.Rows.Count; i++)
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                type = dt.Rows[i]["type"].ObjToString().ToUpper();
                status = dt.Rows[i]["status"].ObjToString().ToUpper();
                if (contractNumber == "L0343" && type == "TRUST" )
                {
                }
                name = dt.Rows[i]["names"].ObjToString();
                record = dt.Rows[i]["record5"].ObjToString();
                if ( contractNumber != lastContract )
                {
                    if (lastRow >= 0)
                    {
                        records = records.TrimEnd(',');
                        dt.Rows[lastRow]["records"] = records;
                        netFuneral = dt.Rows[lastRow]["currentPrice"].ObjToDouble() - dt.Rows[lastRow]["classa"].ObjToDouble() - dt.Rows[lastRow]["totalDiscount"].ObjToDouble();
                        dt.Rows[lastRow]["netFuneral"] = netFuneral;
                    }
                    lastRow = i;
                    lastContract = contractNumber;
                    records = "";
                    if (type == "CASH" || type == "CHECK" || type == "CREDIT CARD")
                        records = record + ",";
                    if (type.ToUpper().IndexOf("INSURANCE") >= 0 || type.ToUpper() == "CLASS A")
                        dt.Rows[i]["names"] = name;
                    else
                        dt.Rows[i]["names"] = "";
                    if (type.ToUpper() == "CLASS A")
                    {
                        trust_policy = dt.Rows[i]["trust_policy"].ObjToString();
                        if (!String.IsNullOrWhiteSpace(trust_policy))
                            dt.Rows[i]["class_a"] = trust_policy;
                    }
                    discountType = "";
                    preneedDiscount = dt.Rows[i]["preneedDiscount"].ObjToDouble();
                    packageDiscount = dt.Rows[i]["packageDiscount"].ObjToDouble();
                    if (preneedDiscount > 0D)
                        discountType = "PN";
                    if (packageDiscount > 0D)
                    {
                        if (!String.IsNullOrWhiteSpace(discountType))
                            discountType += ", ";
                        discountType += "PK";
                    }
                    if ( type.ToUpper() == "DISCOUNT")
                    {
                        reason = dt.Rows[i]["description"].ObjToString();
                        if (!String.IsNullOrWhiteSpace(reason))
                        {
                            if (!String.IsNullOrWhiteSpace(discountType))
                                discountType += ", ";
                            discountType += reason;
                        }
                    }
                    dt.Rows[i]["discountType"] = discountType;
                    if ( status == "FILED")
                    {
                        dt.Rows[i]["dateInsFiled"] = dt.Rows[i]["dateEntered"].ObjToDateTime().ToString("MM/dd/yyyy");
                        dt.Rows[i]["amountFiled"] = dt.Rows[i]["payment"].ObjToDouble();
                    }
                    if (type == "TRUST")
                        dt.Rows[i]["trustPayment"] = dt.Rows[i]["payment"].ObjToDouble();
                    if (type == "CASH" || type == "CHECK")
                    {
                        dt.Rows[i]["cashCheck"] = dt.Rows[i]["payment"].ObjToDouble();
                        depositNumber = dt.Rows[i]["depositNumber2"].ObjToString();
                        if (!String.IsNullOrWhiteSpace(depositNumber))
                            dt.Rows[i]["depositNumber"] = depositNumber;
                        else
                            dt.Rows[i]["depositNumber"] = "";
                        date = dt.Rows[i]["dateReceived"].ObjToDateTime();
                        if (date.Year > 100)
                            dt.Rows[i]["depositDate"] = date.ToString("MM/dd/yyyy");
                        else
                            dt.Rows[i]["depositDate"] = "";
                    }
                    if (type == "CREDIT CARD")
                    {
                        dt.Rows[i]["cc"] = dt.Rows[i]["payment"].ObjToDouble();
                        depositNumber = dt.Rows[i]["depositNumber2"].ObjToString();
                        if (!String.IsNullOrWhiteSpace(depositNumber))
                            dt.Rows[i]["ccDepositNumber"] = depositNumber;
                        else
                            dt.Rows[i]["ccDepositNumber"] = "";
                        date = dt.Rows[i]["dateReceived"].ObjToDateTime();
                        if (date.Year > 100)
                            dt.Rows[i]["ccDepositDate"] = date.ToString("MM/dd/yyyy");
                        else
                            dt.Rows[i]["ccDepositDate"] = "";
                    }

                    notes = dt.Rows[i]["notes2"].ObjToString();
                    if (notes == "-1")
                        notes = "";
                    dt.Rows[i]["notes"] = notes;

                    continue;
                }

                if (type == "CASH" || type == "CHECK" || type == "CREDIT CARD")
                    records += record + ",";
                if (type.ToUpper().IndexOf("INSURANCE") >= 0 || type.ToUpper() == "CLASS A")
                    dt.Rows[lastRow]["names"] += " " + name;
                if ( type.ToUpper() == "CLASS A")
                {
                    trust_policy = dt.Rows[i]["trust_policy"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(trust_policy))
                    {
                        classa = dt.Rows[lastRow]["class_a"].ObjToString();
                        if (!String.IsNullOrWhiteSpace(classa))
                            classa += ", ";
                        classa += trust_policy;
                        dt.Rows[lastRow]["class_a"] = classa;
                        classaTotal = dt.Rows[lastRow]["classa"].ObjToDouble();
                        dValue = dt.Rows[i]["payment"].ObjToDouble();
                        classaTotal += dValue;
                        dt.Rows[lastRow]["classa"] = classaTotal;
                    }
                }
                if ( type == "DISCOUNT")
                {
                    if (type.ToUpper() == "DISCOUNT")
                    {
                        reason = dt.Rows[i]["description"].ObjToString();
                        if (!String.IsNullOrWhiteSpace(reason))
                        {
                            discountType = dt.Rows[lastRow]["discountType"].ObjToString();
                            if (!String.IsNullOrWhiteSpace(discountType))
                                discountType += ", ";
                            discountType += reason;
                        }
                    }
                    dt.Rows[lastRow]["discountType"] = discountType;
                }
                if (status == "FILED")
                {
                    dateInsFiled = dt.Rows[lastRow]["dateInsFiled"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(dateInsFiled))
                        dateInsFiled += ", ";
                    dateInsFiled += dt.Rows[i]["dateEntered"].ObjToDateTime().ToString("MM/dd/yyyy");
                    dt.Rows[lastRow]["dateInsFiled"] = dateInsFiled;
                    amountFiled = dt.Rows[lastRow]["amountFiled"].ObjToDouble();
                    amountFiled += dt.Rows[i]["payment"].ObjToDouble();
                    dt.Rows[lastRow]["amountFiled"] = amountFiled;
                }
                if (type == "TRUST")
                {
                    trustPayment = dt.Rows[lastRow]["trustPayment"].ObjToDouble();
                    trustPayment += dt.Rows[i]["payment"].ObjToDouble();
                    dt.Rows[lastRow]["trustPayment"] = trustPayment;
                }
                if (type == "CASH" || type == "CHECK")
                {
                    cashCheck = dt.Rows[lastRow]["cashCheck"].ObjToDouble() + dt.Rows[i]["payment"].ObjToDouble();
                    dt.Rows[lastRow]["cashCheck"] = cashCheck;

                    str = dt.Rows[i]["depositNumber2"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(str))
                    {
                        depositNumber = dt.Rows[lastRow]["depositNumber"].ObjToString();
                        if (!String.IsNullOrWhiteSpace(depositNumber))
                            depositNumber += ", ";
                        depositNumber += str;
                        dt.Rows[lastRow]["depositNumber"] = depositNumber;

                        date = dt.Rows[i]["dateReceived"].ObjToDateTime();
                        if (date.Year > 100)
                        {
                            depositDate = dt.Rows[lastRow]["depositDate"].ObjToString();
                            if (!String.IsNullOrWhiteSpace(depositDate))
                                depositDate += " ";
                            depositDate += date.ToString("MM/dd/yyyy");
                            dt.Rows[lastRow]["depositDate"] = depositDate;
                        }
                    }
                }

                if (type == "CREDIT CARD" )
                {
                    if (contractNumber == "L0343")
                    {
                    }

                    creditCard = dt.Rows[lastRow]["cc"].ObjToDouble() + dt.Rows[i]["payment"].ObjToDouble();
                    dt.Rows[lastRow]["cc"] = creditCard;

                    depositNumber = dt.Rows[i]["depositNumber2"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(depositNumber))
                    {
                        ccDepositNumber = dt.Rows[lastRow]["ccDepositNumber"].ObjToString();
                        if (!String.IsNullOrWhiteSpace(ccDepositNumber))
                            ccDepositNumber += ", ";
                        ccDepositNumber += depositNumber;
                        dt.Rows[lastRow]["ccDepositNumber"] = ccDepositNumber;
                    }
                    date = dt.Rows[i]["dateReceived"].ObjToDateTime();
                    if (date.Year > 100)
                    {
                        ccDepositDate = dt.Rows[lastRow]["ccDepositDate"].ObjToString();
                        if (!String.IsNullOrWhiteSpace(ccDepositDate))
                            ccDepositDate += ", ";
                        ccDepositDate += date.ToString("MM/dd/yyyy");
                        dt.Rows[lastRow]["ccDepositDate"] = ccDepositDate;
                    }
                }

                notes = dt.Rows[i]["notes2"].ObjToString();
                if (notes == "-1")
                    notes = "";
                if (!String.IsNullOrWhiteSpace(notes))
                {
                    str = dt.Rows[lastRow]["notes"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(str))
                        str += ", ";
                    str += notes;
                    dt.Rows[lastRow]["notes"] = str;
                }

                dt.Rows[i]["contractNumber"] = "";
            }
            for ( int i=(dt.Rows.Count-1); i>=0; i--)
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                if (String.IsNullOrWhiteSpace(contractNumber))
                    dt.Rows.RemoveAt(i);
            }
            return dt;
        }
        /***********************************************************************************************/
        private DataTable FilterTrustClaims ( DataTable dt)
        {
            string contractNumber = "";
            string serviceId = "";
            string serviceId1 = "";
            string type = "";
            for ( int i=dt.Rows.Count-1; i>=0; i--)
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                if (contractNumber.ToUpper().IndexOf("SX") == 0)
                    dt.Rows.RemoveAt(i);
                else if (contractNumber.ToUpper().IndexOf("ZZ") == 0)
                    dt.Rows.RemoveAt(i);
                else
                {
                    type = dt.Rows[i]["type"].ObjToString().ToUpper();
                    if (type != "TRUST")
                        dt.Rows.RemoveAt(i);
                    else
                    {
                        serviceId = dt.Rows[i]["serviceId"].ObjToString();
                        serviceId1 = dt.Rows[i]["serviceId1"].ObjToString();
                        if (String.IsNullOrWhiteSpace(serviceId))
                            dt.Rows.RemoveAt(i);
                        else if (serviceId != serviceId1)
                            dt.Rows.RemoveAt(i);
                    }
                }
            }
            return dt;
        }
        /***********************************************************************************************/
        private DataTable FilterUnityClaims(DataTable dt)
        {
            string contractNumber = "";
            string serviceId = "";
            string serviceId1 = "";
            string paidFrom = "";
            Trust85.FindContract(dt, "SX21096");
            for (int i = dt.Rows.Count - 1; i >= 0; i--)
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                //if (contractNumber.ToUpper().IndexOf("SX") == 0)
                //    dt.Rows.RemoveAt(i);
                if (contractNumber.ToUpper().IndexOf("ZZ") == 0)
                    dt.Rows.RemoveAt(i);
                else
                {
                    paidFrom = dt.Rows[i]["paidFrom"].ObjToString();
                    if ( paidFrom.ToUpper() != "UNITY")
                        dt.Rows.RemoveAt(i);
                }
            }
            Trust85.FindContract(dt, "SX21096");
            return dt;
        }
        /***********************************************************************************************/
        private DataTable FilterNonUnityClaims(DataTable dt)
        {
            string contractNumber = "";
            string serviceId = "";
            string serviceId1 = "";
            Trust85.FindContract(dt, "L0343");
            string type = "";
            string paidFrom = "";
            for (int i = dt.Rows.Count - 1; i >= 0; i--)
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                if (contractNumber.ToUpper().IndexOf("SX") == 0)
                    dt.Rows.RemoveAt(i);
                else if (contractNumber.ToUpper().IndexOf("ZZ") == 0)
                    dt.Rows.RemoveAt(i);
                else
                {
                    type = dt.Rows[i]["type1"].ObjToString().ToUpper();
                    paidFrom = dt.Rows[i]["paidFrom"].ObjToString().ToUpper();
                    if (paidFrom.ToUpper() == "FILED DIRECT")
                        continue;
                    if (type == "3RD PARTY")
                        continue;
                    dt.Rows.RemoveAt(i);
                }
            }
            return dt;
        }
        /***********************************************************************************************/
        private DataTable FilterCashBalance(DataTable dt)
        {
            string contractNumber = "";
            string serviceId = "";
            string serviceId1 = "";
            string type = "";
            for (int i = dt.Rows.Count - 1; i >= 0; i--)
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                if (contractNumber.ToUpper().IndexOf("SX") == 0)
                    dt.Rows.RemoveAt(i);
                else if (contractNumber.ToUpper().IndexOf("ZZ") == 0)
                    dt.Rows.RemoveAt(i);
                else
                {
                    type = dt.Rows[i]["type"].ObjToString().ToUpper();
                    if (type != "CASH" && type != "CHECK" && type != "CREDIT CARD")
                        dt.Rows.RemoveAt(i);
                }
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
            double contractValue = 0D;
            try
            {
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
            catch (Exception ex)
            {
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
                paid = dt.Rows[i]["paidInFull1"].ObjToString();
                if ( paid == "1" )
                    dt.Rows[i]["paidInFull1"] = "1";
                else
                    dt.Rows[i]["paidInFull1"] = "0";
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
            SetAllClaims();
            btnRunCommission.Show();
            btnRunCommission.Refresh();
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

//            Printer.setupPrinterMargins(50, 50, 80, 50);
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

            if (workPDF)
            {
                string filename = "";
                //string filename = path + @"\" + workReport + "_" + today.Year.ToString("D4") + today.Month.ToString("D2") + today.Day.ToString("D2") + ".pdf";

                filename = @"C:/rag/pdfDaily.pdf";
                //filename = workPDFfile;
                if (File.Exists(filename))
                {
                    File.SetAttributes(filename, FileAttributes.Normal);
                    File.Delete(filename);
                }
                printableComponentLink1.ExportToPdf(filename);
            }
            else
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

            printingSystem1.Document.AutoFitToPagesWidth = 1;

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
            string title = this.Text;
            Printer.DrawQuad(5, 8, 5, 4, title, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

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
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();
            string contract = dr["contractNumber"].ObjToString();
            if (!String.IsNullOrWhiteSpace(contract))
            {
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
                    //custForm.custClosing += CustForm_custClosing;
                    custForm.Show();
                }
                //FunPayments editFunPayments = new FunPayments(null, contract, "", false, true );
                //editFunPayments.Show();

                //CustomerDetails clientForm = new CustomerDetails(contract);
                //clientForm.Show();
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
        private void SetupSelectedColumns(string procType, string group, DevExpress.XtraGrid.GridControl dgv)
        {
            if (String.IsNullOrWhiteSpace(group))
                return;
            if (String.IsNullOrWhiteSpace(procType))
                procType = "AR " + workReport;
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
                SetupSelectedColumns("AR", comboName, dgv);
                string name = "AR " + comboName;
                foundLocalPreference = G1.RestoreGridLayout(this, this.dgv, gridMain, LoginForm.username, name, ref skinName);
                SetupTotalsSummary();
                gridMain.OptionsView.ShowFooter = showFooters;
            }
            else
            {
                SetupSelectedColumns("AR", "Primary", dgv);
                string name = "AR Primary";
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
        }
        /***********************************************************************************************/
        private void CleanupFieldColumns()
        {
            if (LoginForm.classification.ToUpper() != "FIELD")
                return;
            gridMain.Columns["amountGrowth1"].Visible = false;
            gridMain.Columns["amountDiscount"].Visible = false;
            gridMain.Columns["contractNumber"].Visible = false;
        }
        /***********************************************************************************************/
        void sform_Done(DataTable dt)
        {
            string name = cmbSelectColumns.Text.Trim();
            if (String.IsNullOrWhiteSpace(name))
                name = "Primary";
            string saveName = "AR " + name;
            string skinName = "";
            SetupSelectedColumns("AR", name, dgv);
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
            SelectDisplayColumns sform = new SelectDisplayColumns(dgv, "AR " + workReport , "Primary", actualName, LoginForm.username);
            sform.Done += new SelectDisplayColumns.d_void_selectionDone(sform_Done);
            sform.Show();
        }
        /***********************************************************************************************/
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            string name = cmbSelectColumns.Text.Trim();
            if (String.IsNullOrWhiteSpace(name))
                name = "Primary";
            string saveName = "AR " + workReport + " " + name;
            G1.SaveLocalPreferences(this, gridMain, LoginForm.username, saveName);
        }
        /***********************************************************************************************/
        private void gridMain_CustomSummaryCalculate(object sender, DevExpress.Data.CustomSummaryEventArgs e)
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
        }
        /*******************************************************************************************/
        private DataTable funeralsDt = null;
        private void LoadFuneralLocations(DataTable dt)
        {
            if (funeralsDt == null)
                funeralsDt = G1.get_db_data("Select * from `funeralhomes`;");
            string contract = "";
            string contractNumber = "";
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
                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                    if (contractNumber == "L17035UI")
                    {
                    }
                    contract = dt.Rows[i]["serviceId"].ObjToString();
                    contract = Trust85.decodeContractNumber(contract, ref trust, ref loc);

                    if (String.IsNullOrWhiteSpace(loc))
                        continue;
                    //dR = funeralsDt.Select("keycode='" + loc + "'");
                    dR = funeralsDt.Select("atneedcode='" + loc + "'");
                    if (dR.Length > 0)
                    {
                        dt.Rows[i]["loc"] = loc;
                        dt.Rows[i]["location"] = dR[0]["LocationCode"].ObjToString();
                    }
                    else
                    {
                        dR = funeralsDt.Select("keycode='" + loc + "'");
                        if (dR.Length > 0)
                        {
                            dt.Rows[i]["loc"] = dR[0]["atneedcode"].ObjToString();
                            dt.Rows[i]["location"] = dR[0]["LocationCode"].ObjToString();
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
            if (dt.Rows.Count <= 0)
                return;
            try
            {
                if (chkSort.Checked)
                {
                    DataView tempview = dt.DefaultView;
                    tempview.Sort = "serviceLoc, lastName, firstName";
                    dt = tempview.ToTable();
                    G1.NumberDataTable(dt);
                    dgv.DataSource = dt;

                    gridMain.Columns["serviceLoc"].GroupIndex = 0;
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
            }
            catch ( Exception ex)
            {
            }
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
        private void recalculateBalanceToolStripMenuItem_Click(object sender, EventArgs e)
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
            CalculateCustomerDetails(contractNumber, record, dr);
        }
        /***********************************************************************************************/
        private void CalculateCustomerDetails(string contractNumber, string custExtendedRecord, DataRow dR)
        {
            if (1 == 1)
                return;
            //this.Cursor = Cursors.WaitCursor;

            //string cmd = "Select * from `cust_payments` where `contractNumber` = '" + contractNumber + "';";
            //DataTable dt = G1.get_db_data(cmd);

            //double oldDiscount = dR["totalDiscount"].ObjToDouble();

            //string amountFiled = "";
            //string amountReceived = "";
            //string amountDiscount = "";
            //string amountGrowth = "";
            //string grossAmountReceived = "";
            //double totalFiled = 0D;
            //double totalReceived = 0D;
            //double totalDiscount = 0D;
            //double totalAmountGrowth = 0D;
            //double totalAmountDiscount = 0D;
            //double totalGross = 0D;
            //double payment = 0D;
            //double totalPayments = 0D;

            //double trustAmountFiled = 0D;
            //double insAmountFiled = 0D;
            //double trustAmountReceived = 0D;
            //double insAmountReceived = 0D;

            //string str = "";
            //string type = "";
            //string cash = "";
            //string status = "";
            //string deposit = "";
            //string creditCard = "";
            //string ccDepNumber = "";
            //double dValue = 0D;
            //double balanceDue = 0D;
            //double discount = 0D;
            //double classa = 0D;
            //string approvedBy = "";
            //DateTime dateEntered = DateTime.Now;
            //DateTime dateModified = DateTime.Now;
            //for (int i = 0; i < dt.Rows.Count; i++)
            //{
            //    try
            //    {
            //        status = dt.Rows[i]["status"].ObjToString().ToUpper();
            //        if (status.ToUpper() == "CANCELLED")
            //            continue;
            //        amountFiled = dt.Rows[i]["amountFiled"].ObjToString();
            //        amountReceived = dt.Rows[i]["amountReceived"].ObjToString();
            //        amountDiscount = dt.Rows[i]["amountDiscount"].ObjToString();
            //        amountGrowth = dt.Rows[i]["amountGrowth"].ObjToString();
            //        grossAmountReceived = dt.Rows[i]["grossAmountReceived"].ObjToString();
            //        payment = dt.Rows[i]["payment"].ObjToDouble();
            //        totalFiled += amountFiled.ObjToDouble();
            //        totalReceived += amountReceived.ObjToDouble();
            //        totalAmountDiscount += amountDiscount.ObjToDouble();
            //        totalAmountGrowth += amountGrowth.ObjToDouble();
            //        totalGross += grossAmountReceived.ObjToDouble();

            //        type = dt.Rows[i]["type"].ObjToString().ToUpper();
            //        if (type.ToUpper() == "DISCOUNT" || status.ToUpper() == "DEPOSITED")
            //            totalPayments += payment;
            //        if (type == "CASH")
            //        {
            //            dValue = dt.Rows[i]["payment"].ObjToDouble();
            //            str = G1.ReformatMoney(dValue);
            //            cash += "CA - " + str + " ";
            //        }
            //        else if (type == "CREDIT CARD")
            //        {
            //            dValue = dt.Rows[i]["payment"].ObjToDouble();
            //            str = G1.ReformatMoney(dValue);
            //            creditCard += "CC - " + str + " ";
            //        }
            //        else if (type == "CLASSA")
            //        {
            //            classa += dt.Rows[i]["payment"].ObjToDouble();
            //        }
            //        str = dt.Rows[i]["depositNumber"].ObjToString();
            //        if (!String.IsNullOrWhiteSpace(str))
            //        {
            //            if (type == "CASH")
            //                deposit += str + " ";
            //            else if (type == "CREDIT CARD")
            //                ccDepNumber += str + " ";
            //        }
            //        if (type == "DISCOUNT")
            //        {
            //            discount += dt.Rows[i]["payment"].ObjToDouble();
            //            str = dt.Rows[i]["approvedBy"].ObjToString();
            //            if (!String.IsNullOrWhiteSpace(str))
            //                approvedBy += str + " ";
            //        }
            //        if (type == "TRUST")
            //        {
            //            if (status == "PENDING")
            //                trustAmountFiled += amountFiled.ObjToDouble();
            //            else if (status == "DEPOSITED")
            //                trustAmountReceived += amountReceived.ObjToDouble();
            //        }
            //        else if (type == "INSURANCE")
            //        {
            //            if (status == "PENDING")
            //                insAmountFiled += amountFiled.ObjToDouble();
            //            else if (status == "DEPOSITED")
            //                insAmountReceived += amountReceived.ObjToDouble();
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //    }
            //}

            //string serviceId = dR["ServiceId"].ObjToString();
            //EditCust.DetermineActiveGroups(contractNumber, serviceId);

            //string myActiveFuneralHomeGroup = EditCust.activeFuneralHomeGroup;
            //string myActiveFuneralHomeCasketGroup = EditCust.activeFuneralHomeCasketGroup;

            //EditCustomer.activeFuneralHomeGroup = myActiveFuneralHomeGroup;
            //EditCustomer.activeFuneralHomeCasketGroup = myActiveFuneralHomeCasketGroup;


            //FunServices funForm = new FunServices(contractNumber);
            //DataTable funDt = funForm.funServicesDT;
            //double price = 0D;
            //double totalServices = 0D;
            //double totalMerchandise = 0D;
            //double totalCashAdvance = 0D;
            //double totalCost = 0D;
            //double difference = 0D;
            //double totalDifference = 0D;
            //double currentPrice = 0D;
            //double totalCurrentPrice = 0D;

            //string service = "";

            //double currentServices = 0D;
            //double currentMerchandise = 0D;

            //double merchandiseDiscount = 0D;
            //double servicesDiscount = 0D;

            //double totalPackagePrice = 0D;
            //double packagePrice = 0D;
            //double packageDiscount = 0D;
            //bool gotPackage = false;

            //if (funDt != null)
            //{
            //    for (int i = 0; i < funDt.Rows.Count; i++)
            //    {
            //        price = funDt.Rows[i]["price"].ObjToDouble();
            //        currentPrice = funDt.Rows[i]["currentPrice"].ObjToDouble();
            //        difference = currentPrice - price;

            //        service = funDt.Rows[i]["service"].ObjToString().ToUpper();
            //        if (service == "TOTAL LISTED PRICE")
            //        {
            //            totalPackagePrice = funDt.Rows[i]["price"].ObjToDouble();
            //            continue;
            //        }
            //        else if (service == "PACKAGE PRICE")
            //        {
            //            gotPackage = true;
            //            packagePrice = funDt.Rows[i]["price"].ObjToDouble();
            //            continue;
            //        }
            //        else if (service == "PACKAGE DISCOUNT")
            //        {
            //            packageDiscount = funDt.Rows[i]["price"].ObjToDouble();
            //            packageDiscount = Math.Abs(packageDiscount);
            //            continue;
            //        }

            //        type = funDt.Rows[i]["type"].ObjToString().ToUpper();
            //        if (type == "SERVICE")
            //        {
            //            totalServices += price;
            //            servicesDiscount += difference;
            //            currentServices += currentPrice;
            //        }
            //        else if (type == "MERCHANDISE")
            //        {
            //            totalMerchandise += price;
            //            merchandiseDiscount += difference;
            //            currentMerchandise += currentPrice;
            //        }
            //        else if (type == "CASH ADVANCE")
            //            totalCashAdvance += price;
            //        totalCurrentPrice += currentPrice;
            //        totalDifference += (currentPrice - price);
            //    }
            //}

            //totalCost = totalCurrentPrice - totalDifference;
            //if (gotPackage)
            //    totalCost = packagePrice;
            //balanceDue = totalCost - totalPayments;
            //totalDiscount = servicesDiscount + merchandiseDiscount;
            //if (gotPackage)
            //    totalDiscount = packageDiscount;
            //if (oldDiscount != totalDiscount)
            //{
            //}

            //if (!String.IsNullOrWhiteSpace(custExtendedRecord))
            //{
            //    G1.update_db_table("fcust_extended", "record", custExtendedRecord, new string[] { "amountFiled", totalFiled.ToString(), "amountReceived", totalReceived.ToString(), "cash", cash, "depositNumber", deposit, "balanceDue", balanceDue.ToString(), "additionalDiscount", discount.ToString(), "approvedBy", approvedBy, "creditCard", creditCard, "ccDepNumber", ccDepNumber, "grossAmountReceived", totalGross.ObjToString(), "classa", classa.ToString(), "amountDiscount", totalAmountDiscount.ObjToString(), "amountGrowth", totalAmountGrowth.ObjToString() });
            //    G1.update_db_table("fcust_extended", "record", custExtendedRecord, new string[] { "custPrice", totalCost.ToString(), "custMerchandise", totalMerchandise.ToString(), "custServices", totalServices.ToString(), "merchandiseDiscount", merchandiseDiscount.ToString(), "servicesDiscount", servicesDiscount.ToString(), "totalDiscount", totalDiscount.ToString(), "currentPrice", totalCurrentPrice.ToString(), "currentMerchandise", currentMerchandise.ToString(), "currentServices", currentServices.ToString() });
            //    G1.update_db_table("fcust_extended", "record", custExtendedRecord, new string[] { "trustAmountFiled", trustAmountFiled.ObjToString(), "trustAmountReceived", trustAmountReceived.ObjToString(), "insAmountFiled", insAmountFiled.ObjToString(), "insAmountReceived", insAmountReceived.ObjToString() });
            //}
            //try
            //{
            //    dR["balanceDue"] = balanceDue;
            //    dR["custPrice"] = totalCost;
            //    dR["custMerchandise"] = totalMerchandise;
            //    dR["custServices"] = totalServices;
            //    dR["merchandiseDiscount"] = merchandiseDiscount;
            //    dR["servicesDiscount"] = servicesDiscount;
            //    dR["additionalDiscount"] = discount;
            //    dR["totalDiscount"] = totalDiscount;

            //    dR["amountFiled"] = totalFiled;
            //    dR["amountReceived"] = totalReceived;

            //    dR["currentPrice"] = totalCurrentPrice;
            //    dR["currentMerchandise"] = currentMerchandise;
            //    dR["currentServices"] = currentServices;

            //    dR["grossAmountReceived"] = totalGross;

            //    dR["amountDiscount"] = totalAmountDiscount;
            //    dR["amountGrowth"] = totalAmountGrowth;
            //}
            //catch (Exception ex)
            //{
            //}

            //this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void toolStripRemoveFormat_Click(object sender, EventArgs e)
        {
            string comboName = cmbSelectColumns.Text;
            if (!String.IsNullOrWhiteSpace(comboName))
            {
                string name = "AR " + workReport + " " + comboName;
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

            DataTable dt = (DataTable)(dgv.DataSource);
            DataRow dr = gridMain.GetFocusedDataRow();
            string contract = dr["contractNumber"].ObjToString();
            string value = dr["paidInFull1"].ObjToString();
            string record = dr["record5"].ObjToString();
            int row = gridMain.GetDataSourceRowIndex(gridMain.FocusedRowHandle);
            if (value == "0")
                value = "1";
            else
                value = "0";
            dr["paidInFull1"] = value;
            dt.Rows[row]["paidInFull1"] = value;
            dgv.DataSource = dt;
            if (workReport.ToUpper() == "CASH BALANCE REPORT")
            {
                string records = dr["records"].ObjToString();
                if (!String.IsNullOrWhiteSpace(records))
                {
                    string[] Lines = records.Split(',');
                    for (int i = 0; i < Lines.Length; i++)
                    {
                        record = Lines[i].Trim();
                        if (String.IsNullOrWhiteSpace(record))
                            continue;
                        G1.update_db_table("cust_payments", "record", record, new string[] { "paidInFull", value });
                    }
                }
                //return;
            }
            else
                G1.update_db_table("cust_payments", "record", record, new string[] { "paidInFull", value });
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

            DataTable dt = (DataTable)(dgv.DataSource);
            DataRow dr = gridMain.GetFocusedDataRow();
            string contract = dr["contractNumber"].ObjToString();
            string value = dr["paidInFull1"].ObjToString();
            string record = dr["record5"].ObjToString();
            int row = gridMain.GetDataSourceRowIndex(gridMain.FocusedRowHandle);
            if (value == "0")
                value = "1";
            else
                value = "0";
            dr["paidInFull1"] = value;
            dt.Rows[row]["paidInFull1"] = value;
            dgv.DataSource = dt;
            //G1.update_db_table("cust_payments", "record", record, new string[] { "paidInFull", value });
            gridMain.PostEditor();
        }
        /***********************************************************************************************/
        private void trustsClaimsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
        }
        /***********************************************************************************************/
        private void gridMain_CalcRowHeightx(object sender, RowHeightEventArgs e)
        {
            GridView View = sender as GridView;
            if (e.RowHandle >= 0)
            {
                string cancelled = View.GetRowCellDisplayText(e.RowHandle, View.Columns["notes"]);
                if (!String.IsNullOrWhiteSpace(cancelled))
                {
                   // DataGridViewCell cell = View[e.RowHandle, "notes"];

                    string text = cancelled;
                    Size textSize = TextRenderer.MeasureText(text, View.Columns["notes"].AppearanceCell.Font);
                    int charWidth = textSize.Width / text.Length;
                    int charCount = View.Columns["notes"].Width / charWidth;

                    int originalRowHeight = e.RowHeight;
                    cancelled = cancelled.TrimEnd('\n');
                    string[] Lines = cancelled.Split('\n');
                    int count = Lines.Length;
                    count = charWidth;
                    if (count > 1)
                        e.RowHeight = originalRowHeight * count;
                }
            }
        }
        /***********************************************************************************************/
        private void gridMain_CalcRowHeight(object sender, RowHeightEventArgs e)
        {
            if (1 == 1)
                return;
            GridView View = sender as GridView;
            if (e.RowHandle >= 0)
            {
                int maxHeight = 0;
                int newHeight = 0;
                string name = "";
                bool doit = false;
                foreach (GridColumn column in gridMain.Columns)
                {
                    doit = true;
                    if (workReport.ToUpper() == "CASH BALANCE REPORT")
                    {
                        doit = false;
                        name = column.FieldName.ToUpper();
                        if (name == "NAMES" || name == "CLASS_A" || name == "DISCOUNTTYPE" || name == "DATEINSFILED" || name == "DEPOSITNUMBER" || name == "DEPOSITDATE" || name == "CCDEPOSITNUMBER" || name == "CCDEPOSITDATE" || name == "NOTES" )
                            doit = true;
                        if (name == "DEPOSITDATE")
                        {
                        }
                    }
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

                if ( maxHeight > 0 )
                    e.RowHeight = maxHeight;
            }
        }
        /***********************************************************************************************/
        private void gridMain_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            ColumnView view = sender as ColumnView;

            string columnName = e.Column.FieldName.ToUpper();
            if (columnName == "DATEINSFILED")
                return;
            if (columnName == "DEPOSITDATE")
                return;
            if (columnName == "CCDEPOSITDATE")
                return;

            if (e.Column.FieldName.ToUpper().IndexOf("CASECREATEDDATE") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                string str = e.DisplayText.Trim();
                if (String.IsNullOrWhiteSpace(str))
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
                if (Lines.Length > 1)
                {
                    data = Lines[1];
                    data = data.Replace("(", "");
                    data = data.Replace(")", "");
                    e.DisplayText = data;
                }
            }
            else if (e.Column.FieldName.ToUpper() == "FUNERAL ARRANGER" && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                string str = e.DisplayText.Trim();
                if (!String.IsNullOrWhiteSpace(str))
                {
                    string[] Lines = str.Split('[');
                    if (Lines.Length > 0)
                    {
                        str = Lines[0].Trim();
                        e.DisplayText = str;
                    }
                }
            }
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
        private double originalSize = 0D;
        private Font mainFont = null;
        private void ScaleCells()
        {
            if (originalSize == 0D)
            {
                //                originalSize = gridMain.Columns["address1"].AppearanceCell.FontSizeDelta.ObjToDouble();
                originalSize = gridMain.Columns["name"].AppearanceCell.Font.Size;
                mainFont = gridMain.Columns["name"].AppearanceCell.Font;
            }
            double scale = txtScale.Text.ObjToDouble();
            double size = scale / 100D * originalSize;
            Font font = new Font(mainFont.Name, (float)size);
            for (int i = 0; i < gridMain.Columns.Count; i++)
            {
                gridMain.Columns[i].AppearanceCell.Font = font;
            }
            gridMain.RefreshData();
            dgv.Refresh();
            this.Refresh();
        }
        /***********************************************************************************************/
        private void btnLeft_Click(object sender, EventArgs e)
        {
            DateTime now = this.dateTimePicker2.Value;
            DateTime date = new DateTime(now.Year, now.Month, 1);
            date = date.AddMonths(-1);
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            date = new DateTime(date.Year, date.Month, days);
            this.dateTimePicker2.Value = date;
            this.dateTimePicker1.Value = new DateTime(date.Year, date.Month, 1 );
        }
        /***********************************************************************************************/
        private void btnRight_Click(object sender, EventArgs e)
        {
            DateTime now = this.dateTimePicker2.Value;
            DateTime date = new DateTime(now.Year, now.Month, 1);
            date = date.AddMonths(1);
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            date = new DateTime(date.Year, date.Month, days);
            this.dateTimePicker2.Value = date;
            this.dateTimePicker1.Value = new DateTime(date.Year, date.Month, 1);
        }
        /***********************************************************************************************/
        private void btnRunCommission_Click(object sender, EventArgs e)
        {
            if ( workDt != null )
            {
                runBatch();
                return;
            }
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();

            string who = dr["funeralType"].ObjToString();

            //string cmd = "Select * from `funcommissiondata` where `name` = '" + workManager + "' OR `name` = '" + workArranger + "';";
            //DataTable dx = G1.get_db_data(cmd);

            this.Cursor = Cursors.WaitCursor;
            FunCommissions funForm = null;
            if (String.IsNullOrWhiteSpace(workManager) && String.IsNullOrWhiteSpace(workArranger))
            {
                funForm = new FunCommissions(dt, workManager, workArranger, workWho, false);
                funForm.Show();
            }
            else
            {
                funForm = new FunCommissions(dt, workManager, workArranger, workWho, false);
                funForm.funCommissionClosing += FunForm_funCommissionClosing;
                funForm.ShowDialog();
            }
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void runBatch ()
        {
            workPDF = true;

            string prefix = "";
            string suffix = "";
            string firstName = "";
            string lastName = "";
            string mi = "";

            DataTable dt = null;
            myDataTable = null;
            workCommission = 0D;
            workFirst = true;
            int lastRow = workDt.Rows.Count;

            barImport.Show();
            barImport.Minimum = 0;
            barImport.Maximum = lastRow;
            barImport.Refresh();

            iTextSharp.text.Document sourceDocument = null;
            PdfCopy pdfCopyProvider = null;
            PdfImportedPage importedPage;
            string outputPdfPath = @"C:/rag/pdfX.pdf";

            if (File.Exists(outputPdfPath))
            {
                File.SetAttributes(outputPdfPath, FileAttributes.Normal);
                File.Delete(outputPdfPath);
            }

            sourceDocument = new iTextSharp.text.Document();
            pdfCopyProvider = new PdfCopy(sourceDocument, new System.IO.FileStream(outputPdfPath, System.IO.FileMode.Create));

            //output file Open  
            sourceDocument.Open();

            string historyFile = @"C:/rag/pdfDaily.pdf";
            string manualFile = @"c:/rag/Manual.pdf";
            FunCommissions funForm = null;

            //lastRow = 1;
            for (int i = 0; i < lastRow; i++)
            {
                barImport.Value = i + 1;
                barImport.Refresh();

                workManager = workDt.Rows[i]["name"].ObjToString();
                if (String.IsNullOrWhiteSpace(workManager))
                    continue;
                workWho = workDt.Rows[i]["ma"].ObjToString();

                workArrangerLastName = "";
                workArrangerFirstName = "";

                G1.ParseOutName(workManager, ref prefix, ref firstName, ref lastName, ref mi, ref suffix);

                if (!String.IsNullOrWhiteSpace(firstName))
                    workArrangerFirstName = firstName;
                if (!String.IsNullOrWhiteSpace(lastName))
                    workArrangerLastName = lastName;

                workArranger = firstName + " " + lastName;

                SetupServiceLocs();

                historyFile = @"C:/rag/pdfDaily.pdf";

                dt = LoadData();

                dgv.DataSource = dt;
                gridMain.RefreshEditor(true);

                printPreviewToolStripMenuItem_Click(null, null);

                this.Cursor = Cursors.WaitCursor;

                //dt = (DataTable)dgv.DataSource;

                manualFile = @"c:/rag/Manual.pdf";
                //string payOffFile = @"c:/rag/ForcePayoff.pdf";

                funForm = new FunCommissions(dt, workManager, workArranger, workWho, true);
                funForm.funCommissionClosing += FunForm_funCommissionClosing;
                funForm.Show();

                this.Cursor = Cursors.Default;
                //DailyHistory histForm = new DailyHistory(contract, historyFile, true);

                //string manualFile = @"c:/rag/Manual.pdf";
                //string payOffFile = @"c:/rag/ForcePayoff.pdf";


                //ForcedPayoffs forceForm = new ForcedPayoffs(true, record, this.dateTimePicker1.Value, this.dateTimePicker2.Value);

                MergeAllPDF(pdfCopyProvider, historyFile, manualFile );

                //if (File.Exists(payOffFile))
                //{
                //    File.SetAttributes(payOffFile, FileAttributes.Normal);
                //    File.Delete(payOffFile);
                //}

                if (File.Exists(historyFile))
                {
                    File.SetAttributes(historyFile, FileAttributes.Normal);
                    File.Delete(historyFile);
                }

                if (File.Exists(manualFile))
                {
                    File.SetAttributes(manualFile, FileAttributes.Normal);
                    File.Delete(manualFile);
                }
            }

            funForm = new FunCommissions(myDataTable, workWho, workCommission );
            funForm.Show();

            MergeAllPDF(pdfCopyProvider, null, manualFile);

            barImport.Value = lastRow;
            barImport.Refresh();

            //save the output file  
            sourceDocument.Close();

            ViewPDF myView = new ViewPDF("TEST", outputPdfPath);
            myView.ShowDialog();

            if (File.Exists(outputPdfPath))
            {
                File.SetAttributes(outputPdfPath, FileAttributes.Normal);
                File.Delete(outputPdfPath);
            }

            barImport.Hide();

            //if (myDataTable != null)
            //{
            //    dt = (DataTable)dgv.DataSource;
            //    if (dt != null)
            //    {
            //        dt.Rows.Clear();
            //        dgv.DataSource = dt;
            //    }

            //    FunCommissions funForm = new FunCommissions(myDataTable, workWho);
            //    funForm.Show();
            //}

            workManager = "";
            workArranger = "";
            myDataTable = null;
            workFirst = true;
        }
        /***********************************************************************************************/
        private static void MergeAllPDF(PdfCopy pdfCopyProvider, string File2, string File3)
        {
            string[] fileArray = new string[3];
            fileArray[0] = File2;
            fileArray[1] = File3;

            PdfReader reader = null;
            PdfImportedPage importedPage;


            //files list wise Loop  
            for (int f = 0; f < fileArray.Length - 1; f++)
            {
                if (fileArray[f] == null)
                    continue;

                int pages = TotalPageCount(fileArray[f]);

                reader = new PdfReader(fileArray[f]);
                //Add pages in new file  
                for (int i = 1; i <= pages; i++)
                {
                    importedPage = pdfCopyProvider.GetImportedPage(reader, i);
                    pdfCopyProvider.AddPage(importedPage);
                }

                reader.Close();
            }
        }
        /***********************************************************************************************/
        private static int TotalPageCount(string file)
        {
            using (StreamReader sr = new StreamReader(System.IO.File.OpenRead(file)))
            {
                Regex regex = new Regex(@"/Type\s*/Page[^s]");
                MatchCollection matches = regex.Matches(sr.ReadToEnd());

                return matches.Count;
            }
        }
        /***********************************************************************************************/
        private DataTable myDataTable = null;
        private bool workFirst = true;
        private double workCommission = 0D;
        private void FunForm_funCommissionClosing(DataTable dt, string manager, string arranger, string who, double commission )
        {
            if (myDataTable == null)
                myDataTable = dt.Clone();

            DataRow dR = null;
            dR = myDataTable.NewRow();
            if (who.ToUpper() == "M")
                dR["name"] = manager;
            else
                dR["name"] = arranger;
            dR["commission"] = commission;

            myDataTable.Rows.Add(dR);

            workCommission += commission;
        }
        /***********************************************************************************************/
        private void chkSingle_CheckedChanged(object sender, EventArgs e)
        {
            SetAllClaims();
        }
        /***********************************************************************************************/
    }
}
