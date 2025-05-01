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
using DevExpress.XtraPrinting;
using DevExpress.Utils;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.Utils.Extensions;
using DevExpress.XtraGrid.Views.BandedGrid;
using DevExpress.XtraGrid;
using sun.misc;
using DevExpress.XtraRichEdit.Commands.Internal;
using DevExpress.Utils.DPI;

/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class Services : DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        private PleaseWait pleaseForm = null;

        private bool alreadyLoaded = false;
        private string savePackage = "";
        private bool modified = false;
        private bool Selecting = false;
        private DataTable workDt = null;
        private string workContract = "";
        private bool loading = false;
        private bool workingLocation = false;
        private string loadedPackage = "";
        private string loadededLocation = "";
        private string workGPL = "";
        private string workFrom = "";

        private string workGroup = "";
        private string workPackage = "";
        private string casketPackage = "";
        private string selectWhat = "Service";
        private string selectSubWhat = "";
        private string workServiceId = "";
        private bool gotPackage = false;
        /***********************************************************************************************/
        public Services(bool selecting, DataTable dt = null, string contract = "")
        {
            InitializeComponent();
            Selecting = selecting;
            workDt = dt;
            workContract = contract;
            SetupTotalsSummary();
            if (!selecting && dt == null && String.IsNullOrWhiteSpace(contract))
            {
            }
            else
                contextMenuStrip1.Dispose();
        }
        /***********************************************************************************************/
        public Services(bool selecting, bool locations, string gplGroup)
        {
            InitializeComponent();
            Selecting = selecting;
            workGPL = gplGroup;
            workDt = null;
            workContract = "";
            workingLocation = locations;
            contextMenuStrip1.Dispose();
        }
        /***********************************************************************************************/
        public Services(string from, bool selecting, DataTable dt = null, string contract = "")
        {
            InitializeComponent();
            Selecting = selecting;
            workDt = dt;
            workContract = contract;
            workFrom = from;
            SetupTotalsSummary();
        }
        /***********************************************************************************************/
        public Services(string fromGroup, string fromPackage, bool selecting, DataTable dt = null, string contract = "", string what = "", string subWhat = "", string serviceId = "" )
        {
            InitializeComponent();
            Selecting = selecting;
            btnAllOff.Show();
            btnAllOn.Hide();
            workDt = dt;
            workContract = contract;
            workServiceId = serviceId;
            workGroup = fromGroup;
            workPackage = fromPackage;
            if (workPackage.ToUpper().IndexOf("CASKET") >= 0)
            {
                casketPackage = fromPackage;
                workPackage = "Master";
            }
            selectWhat = what;
            if (String.IsNullOrWhiteSpace(selectWhat))
                selectWhat = "Service";
            selectSubWhat = subWhat;
            SetupTotalsSummary();
        }
        /***********************************************************************************************/
        private void SetupTotalsSummary()
        {
            AddSummaryColumn("price", null);
            AddSummaryColumn("total", null);
            AddSummaryColumn("data", null);
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
        private void Services_Load(object sender, EventArgs e)
        {
            gotPackage = false;

            labBalDue.Text = "Total Charges :";

            G1.SetupToolTip ( picMoveFuture, "Move Future to Current");
            G1.SetupToolTip ( picMovePast, "Move Past to Current");

            btnSave.Hide();
            btnAllOff.Show();
            btnAllOn.Hide();
            btnEdit.Hide();
            btnDeleteLocation.Hide();
            loading = true;
            gridMain.Columns["basicService"].Visible = true;
            gridMain.Columns["plonly"].Visible = true;
            gridMain.Columns["noSelect"].Visible = true;

            if (!G1.isAdmin())
                chkPackage.Hide();

            int additional = gridMain.Columns["basicService"].Width;
            int top = picMoveFuture.Top;
            int left = picMoveFuture.Left + additional;
            int height = picMoveFuture.Height;
            int width = picMoveFuture.Width;
            picMoveFuture.SetBounds(left, top, width, height);

            top = picMovePast.Top;
            left = picMovePast.Left + additional;
            height = picMovePast.Height;
            width = picMovePast.Width;
            picMovePast.SetBounds(left, top, width, height);

            if (!Selecting)
            {
                LoadGroupCombo();
                LoadPackagesCombo();
                labBalanceDue.Hide();
                labBalDue.Hide();
                gridMain.OptionsBehavior.ReadOnly = false;
                gridMain.Columns["total"].Visible = false;
                tabControl1.TabPages.Remove(tabCaskets);
                tabControl1.TabPages.Remove(tabVaults);
                tabControl1.TabPages.Remove(tabUrns);
                tabControl1.TabPages.Remove(tabMisc);
            }
            else
            {
                LoadGroupCombo();
                LoadPackagesCombo();
                //cmbGroups.Hide();
                //cmbPackage.Hide();
                lblLocation.Hide();
                //lblPackage.Hide();
                btnAddPackage.Hide();
                btnDeletePackage.Hide();
                if (selectWhat.ToUpper() == "MERCHANDISE")
                {
                    cmbGroups.Visible = false;
                    cmbPackage.Visible = false;
                    cmbLocations.Visible = false;
                    picMoveFuture.Hide();
                    picMovePast.Hide();
                    btnAllOff.Hide();
                    btnRunMarkup.Hide();
                    txtMarkup.Hide();
                    label1.Hide();
                    btnAddLocation.Hide();
                    lblLocations.Hide();
                    lblPackage.Hide();
                }
                else if ( selectWhat.ToUpper() == "SERVICE")
                {
                    cmbGroups.Visible = false;
                    lblLocations.Hide();
                    cmbLocations.Visible = false;
                    btnAddLocation.Hide();
                }
                gridMain.OptionsView.ShowFooter = true;
                gridMain.Columns["total"].Visible = false;
                gridMain.Columns["plonly"].Visible = false;
                gridMain.Columns["noSelect"].Visible = false;
                btnSave.Text = "Copy to Customer";
                top = btnSave.Top;
                width = btnSave.Width;
                left = btnSave.Left;
                width = width * 3;
                height = btnSave.Height;
                top = top + height;
               // btnSave.SetBounds(left, top, width, height);
                if (selectWhat.ToUpper() == "SERVICE")
                {
                    tabControl1.TabPages.Remove(tabCaskets);
                    tabControl1.TabPages.Remove(tabVaults);
                    tabControl1.TabPages.Remove(tabUrns);
                    tabControl1.TabPages.Remove(tabMisc);
                }
                else
                {
                    tabControl1.TabPages.Remove(tabServices);
                    dgv.Visible = false;
                }
            }
            LoadData();
            DataTable dddt = (DataTable)dgv.DataSource;
            if (Selecting)
            {
                btnRunMarkup.Hide();
                txtMarkup.Hide();
                label1.Hide();

                pictureAdd.Hide();
                pictureDelete.Hide();
                btnInsert.Hide();
                picRowDown.Hide();
                picRowUp.Hide();
                picMoveFuture.Hide();
                picMovePast.Hide();
                gridMain.Columns["select"].Visible = true;
                gridMain.Columns["SameAsMaster"].Visible = true;
                gridMain.Columns["basicService"].Visible = false;
                gridMain.Columns["plonly"].Visible = false;
                gridMain.Columns["noSelect"].Visible = false;
                if (workingLocation)
                {
                    LoadGPL();
                    DataTable dt = (DataTable)dgv.DataSource;
                    SetupSelection(dt);
                    SetupTax(dt);
                    SetupForLocations();
                }
                else
                    ReSelectServices();
                this.Text = "Services for Contract (" + workContract + ")";
            }
            else
            {
                DataTable dt = (DataTable)dgv.DataSource;
                SetupBasicServices(dt);
                SetupPLOnly(dt);
                SetupNoSelect(dt);
            }
            if (selectWhat.ToUpper() == "MERCHANDISE")
            {
                DataTable dt = (DataTable)dgv.DataSource;
                //ProcessMerchansice(dt);
                AddMerchandise();
            }
            if (Selecting)
            {
                btnAllOn.Hide();
                if (selectWhat.ToUpper() == "MERCHANDISE")
                    btnAllOff.Hide();

                gridMain.Columns["pastPrice"].Visible = false;
                gridMain.Columns["futurePrice"].Visible = false;
                gridMain.Columns["data"].Visible = false;
                gridMain.Columns["total"].Visible = false;
                gridMain.Columns["tax"].Visible = false;
                gridMain.Columns["location"].Visible = false;
                gridMain.Columns["locRecord"].Visible = false;

                gridMain2.Columns["pastPrice"].Visible = false;
                gridMain2.Columns["futurePrice"].Visible = false;
                gridMain2.Columns["data"].Visible = false;
                gridMain2.Columns["total"].Visible = false;
                gridMain2.Columns["tax"].Visible = false;
                gridMain2.Columns["location"].Visible = false;
                gridMain2.Columns["locRecord"].Visible = false;

                gridMain3.Columns["pastPrice"].Visible = false;
                gridMain3.Columns["futurePrice"].Visible = false;
                gridMain3.Columns["data"].Visible = false;
                gridMain3.Columns["total"].Visible = false;
                gridMain3.Columns["tax"].Visible = false;
                gridMain3.Columns["location"].Visible = false;
                gridMain3.Columns["locRecord"].Visible = false;

                gridMain4.Columns["pastPrice"].Visible = false;
                gridMain4.Columns["futurePrice"].Visible = false;
                gridMain4.Columns["data"].Visible = false;
                gridMain4.Columns["total"].Visible = false;
                gridMain4.Columns["tax"].Visible = false;
                gridMain4.Columns["location"].Visible = false;
                gridMain4.Columns["locRecord"].Visible = false;

                gridMain5.Columns["pastPrice"].Visible = false;
                gridMain5.Columns["futurePrice"].Visible = false;
                gridMain5.Columns["data"].Visible = false;
                gridMain5.Columns["total"].Visible = false;
                gridMain5.Columns["tax"].Visible = false;
                gridMain5.Columns["location"].Visible = false;
                gridMain5.Columns["locRecord"].Visible = false;

                if ( selectWhat.ToUpper() == "MERCHANDISE")
                    this.Text = "Select Merchandise from (" + casketPackage + ")";
                else if ( selectWhat.ToUpper() == "SERVICE")
                    this.Text = "Select Services from (" + workGroup + ")";

                DataTable dt = (DataTable)dgv.DataSource;
                dt.Columns.Add("ModMod");

                dt = (DataTable)dgv2.DataSource;
                if ( dt != null )
                    dt.Columns.Add("ModMod");

                dt = (DataTable)dgv3.DataSource;
                if (dt != null)
                    dt.Columns.Add("ModMod");

                dt = (DataTable)dgv4.DataSource;
                if (dt != null)
                    dt.Columns.Add("ModMod");

                dt = (DataTable)dgv5.DataSource;
                if (dt != null)
                    dt.Columns.Add("ModMod");
            }

            if (Selecting)
            {
                gridMain.OptionsView.ShowFooter = true;
                gridMain2.OptionsView.ShowFooter = true;
                gridMain3.OptionsView.ShowFooter = true;
                gridMain4.OptionsView.ShowFooter = true;
                gridMain5.OptionsView.ShowFooter = true;
            }

            gridMain.Columns["price"].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Custom;
            gridMain.Columns["price"].SummaryItem.DisplayFormat = "{0:C2}";

            if ( Selecting )
            {
                gridMain2.CustomSummaryCalculate += new DevExpress.Data.CustomSummaryEventHandler(this.gridMain_CustomSummaryCalculate);
                gridMain3.CustomSummaryCalculate += new DevExpress.Data.CustomSummaryEventHandler(this.gridMain_CustomSummaryCalculate);
                gridMain4.CustomSummaryCalculate += new DevExpress.Data.CustomSummaryEventHandler(this.gridMain_CustomSummaryCalculate);
                gridMain5.CustomSummaryCalculate += new DevExpress.Data.CustomSummaryEventHandler(this.gridMain_CustomSummaryCalculate);

                gridMain2.Columns["price"].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Custom;
                gridMain2.Columns["price"].SummaryItem.DisplayFormat = "{0:C2}";
                gridMain3.Columns["price"].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Custom;
                gridMain3.Columns["price"].SummaryItem.DisplayFormat = "{0:C2}";
                gridMain4.Columns["price"].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Custom;
                gridMain4.Columns["price"].SummaryItem.DisplayFormat = "{0:C2}";
                gridMain5.Columns["price"].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Custom;
                gridMain5.Columns["price"].SummaryItem.DisplayFormat = "{0:C2}";

            }

            if (selectSubWhat.ToUpper() == "VAULT")
                tabControl1.SelectTab("tabVaults");
            else if (selectSubWhat.ToUpper() == "URN")
                tabControl1.SelectTab("tabUrns");
            else if (selectSubWhat.ToUpper() == "MISCELLANEOUS")
                tabControl1.SelectTab("tabMisc");

            FilterLocations();

            loading = false;
        }
        /***********************************************************************************************/
        private void FilterLocations ()
        {
            if (String.IsNullOrWhiteSpace(workServiceId))
                return;
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow[] dRows = dt.Select("location<>''");
            if (dRows.Length <= 0)
                return;
            DataTable dx = dRows.CopyToDataTable();
        }
        /***********************************************************************************************/
        private void ProcessMerchansice(DataTable dt)
        {
            string cmd = "Select * from `casket_packages` p RIGHT JOIN `casket_master` s ON p.`!masterRecord` = s.`record` where ";
            cmd += " `groupname` = '" + casketPackage + "' ";
            cmd += ";";

            double markup = 0D;
            double cost = 0D;
            double price = 0D;
            double packageCost = 0D;
            double rounding = 0D;
            string service = "";
            string type = "";
            DataRow[] dRows = null;

            DataTable gDt = G1.get_db_data(cmd);
            //PullLocationCaskets(gDt);

            cmd = "Select * from `casket_master`;";
            DataTable cDt = G1.get_db_data(cmd);

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    type = dt.Rows[i]["type"].ObjToString();
                    if (type.ToUpper() != "MERCHANDISE")
                        continue;
                    service = dt.Rows[i]["service"].ObjToString();
                    if (service.Trim() == "O39 Aegean Bronze - Champagne Velvet")
                    {
                    }
                    dRows = gDt.Select("casketdesc='" + service + "'");
                    if (dRows.Length <= 0)
                    {
                        dRows = cDt.Select("casketdesc='" + service + "'");
                        if (dRows.Length > 0)
                        {
                            rounding = dRows[0]["round"].ObjToDouble();
                            //markup = dRows[0]["markup"].ObjToDouble();
                            markup = 1D;
                            price = dRows[0]["casketcost"].ObjToDouble();
                            if (rounding > 0D)
                                price = Caskets.RoundTo(price, rounding);
                            price = price * markup;
                            dt.Rows[i]["price"] = price;
                            dt.Rows[i]["found"] = "Y";
                        }
                    }
                    else
                    {
                        rounding = dRows[0]["round"].ObjToDouble();
                        markup = dRows[0]["markup"].ObjToDouble();
                        price = dRows[0]["casketCost"].ObjToDouble();
                        cost = dRows[0]["casketcost"].ObjToDouble();
                        price = cost;
                        price = price * markup;
                        if (rounding > 0D)
                            price = Caskets.RoundTo(price, rounding);
                        dt.Rows[i]["price"] = price;
                        dt.Rows[i]["found"] = "Y";
                    }
                }
                catch (Exception ex)
                {
                }
            }
        }
        /***********************************************************************************************/
        private void AddMerchandise()
        {
            string casketCode = "";
            string str = "";
            string service = "";
            double cost = 0D;
            string tax = "";
            double taxAmount = 0D;
            string location = "";
            double futureCost = 0D;
            double pastCost = 0D;
            double rounding = 0D;
            double markup = 0D;
            double price = 0D;
            string status = "";
            DataRow dRow = null;
            DataRow[] dRows = null;
            DataTable dx = (DataTable)dgv.DataSource;
            string cmd = "Select * from `casket_master` order by `order`;";
            DataTable dt = G1.get_db_data(cmd);

            cmd = "Select * from `casket_packages` p RIGHT JOIN `casket_master` s ON p.`!masterRecord` = s.`record` where ";
            cmd += " `groupname` = '" + casketPackage + "' ";
            cmd += ";";
            DataTable gDt = G1.get_db_data(cmd);


            DataTable dt2 = dx.Clone(); // Caskets
            DataTable dt3 = dx.Clone(); // Vaults
            DataTable dt4 = dx.Clone(); // Urns
            DataTable dt5 = dx.Clone(); // Misc
            string locations = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                locations = dt.Rows[i]["locations"].ObjToString();
                if ( !String.IsNullOrWhiteSpace ( locations ))
                {
                    if (!VerifyLocation(locations))
                        continue;
                }
                casketCode = dt.Rows[i]["casketcode"].ObjToString();
                if (String.IsNullOrWhiteSpace(casketCode))
                    continue;
                if (casketCode.ToUpper() == "MISC")
                {
                    service = dt.Rows[i]["casketdesc"].ObjToString();
                    cost = dt.Rows[i]["casketcost"].ObjToDouble();
                    futureCost = dt.Rows[i]["futureCasketCost"].ObjToDouble();
                    pastCost = dt.Rows[i]["pastCasketCost"].ObjToDouble();
                    tax = dt.Rows[i]["tax"].ObjToString();
                    taxAmount = dt.Rows[i]["taxAmount"].ObjToDouble();
                    dRows = gDt.Select("casketdesc='" + service + "'");
                    if (dRows.Length > 0)
                    {
                        rounding = dRows[0]["round"].ObjToDouble();
                        markup = dRows[0]["markup"].ObjToDouble();
                        price = dRows[0]["casketCost"].ObjToDouble();
                        cost = dRows[0]["price"].ObjToDouble();
                        price = cost;
                        price = price * markup;
                        if (rounding > 0D)
                            price = Caskets.RoundTo(price, rounding);
                        cost = price;
                    }
                    AddMerchandiseRow(dt5, service, cost, futureCost, pastCost, tax, taxAmount);
                    continue;
                }
                str = casketCode.Substring(0, 1).ToUpper();
                if (str == "V")
                {
                    str = casketCode.Substring(1);
                    if (G1.validate_numeric(str))
                    {
                        cost = price;
                        service = dt.Rows[i]["casketdesc"].ObjToString();
                        cost = dt.Rows[i]["casketcost"].ObjToDouble();
                        futureCost = dt.Rows[i]["futureCasketCost"].ObjToDouble();
                        pastCost = dt.Rows[i]["pastCasketCost"].ObjToDouble();
                        tax = dt.Rows[i]["tax"].ObjToString();
                        taxAmount = dt.Rows[i]["taxAmount"].ObjToDouble();
                        dRows = gDt.Select("casketdesc='" + service + "'");
                        if (dRows.Length > 0)
                        {
                            rounding = dRows[0]["round"].ObjToDouble();
                            markup = dRows[0]["markup"].ObjToDouble();
                            price = dRows[0]["price"].ObjToDouble();
                            cost = dRows[0]["casketcost"].ObjToDouble();
                            price = cost;
                            price = price * markup;
                            if (rounding > 0D)
                                price = Caskets.RoundTo(price, rounding);
                            cost = price;
                        }
                        AddMerchandiseRow(dt3, service, cost, futureCost, pastCost, tax, taxAmount);
                        continue;
                    }
                }

                //if (str == "V" && casketCode.Length == 3)
                //{
                //    service = dt.Rows[i]["casketdesc"].ObjToString();
                //    cost = dt.Rows[i]["casketcost"].ObjToDouble();
                //    futureCost = dt.Rows[i]["futureCasketCost"].ObjToDouble();
                //    pastCost = dt.Rows[i]["pastCasketCost"].ObjToDouble();
                //    AddMerchandiseRow(dt3, service, cost, futureCost, pastCost);
                //    continue;
                //}
                if (casketCode.Contains("URN"))
                {
                    service = dt.Rows[i]["casketdesc"].ObjToString();
                    cost = dt.Rows[i]["casketcost"].ObjToDouble();
                    futureCost = dt.Rows[i]["futureCasketCost"].ObjToDouble();
                    pastCost = dt.Rows[i]["pastCasketCost"].ObjToDouble();
                    tax = dt.Rows[i]["tax"].ObjToString();
                    taxAmount = dt.Rows[i]["taxAmount"].ObjToDouble();
                    dRows = gDt.Select("casketdesc='" + service + "'");
                    if (dRows.Length > 0)
                    {
                        rounding = dRows[0]["round"].ObjToDouble();
                        markup = dRows[0]["markup"].ObjToDouble();
                        price = dRows[0]["price"].ObjToDouble();
                        cost = dRows[0]["price"].ObjToDouble();
                        price = cost;
                        price = price * markup;
                        if (rounding > 0D)
                            price = Caskets.RoundTo(price, rounding);
                        cost = price;
                    }
                    AddMerchandiseRow(dt4, service, cost, futureCost, pastCost, tax, taxAmount );
                    continue;
                }
                if (casketCode.Length >= 2)
                {
                    str = casketCode.Substring(0, 2).ToUpper();
                    if (str == "UV")
                    {
                        service = dt.Rows[i]["casketdesc"].ObjToString();
                        cost = dt.Rows[i]["casketcost"].ObjToDouble();
                        futureCost = dt.Rows[i]["futureCasketCost"].ObjToDouble();
                        pastCost = dt.Rows[i]["pastCasketCost"].ObjToDouble();
                        tax = dt.Rows[i]["tax"].ObjToString();
                        taxAmount = dt.Rows[i]["taxAmount"].ObjToDouble();
                        tax = dt.Rows[i]["tax"].ObjToString();
                        dRows = gDt.Select("casketdesc='" + service + "'");
                        if (dRows.Length > 0)
                        {
                            rounding = dRows[0]["round"].ObjToDouble();
                            markup = dRows[0]["markup"].ObjToDouble();
                            price = dRows[0]["price"].ObjToDouble();
                            cost = dRows[0]["price"].ObjToDouble();
                            price = cost;
                            price = price * markup;
                            if (rounding > 0D)
                                price = Caskets.RoundTo(price, rounding);
                            cost = price;
                        }
                        AddMerchandiseRow(dt4, service, cost, futureCost, pastCost, tax, taxAmount);
                        continue;
                    }
                }

                service = dt.Rows[i]["casketdesc"].ObjToString();
                cost = dt.Rows[i]["casketcost"].ObjToDouble();
                futureCost = dt.Rows[i]["futureCasketCost"].ObjToDouble();
                pastCost = dt.Rows[i]["pastCasketCost"].ObjToDouble();
                tax = dt.Rows[i]["tax"].ObjToString();
                taxAmount = dt.Rows[i]["taxAmount"].ObjToDouble();
//                location = dt.Rows[i]["location"].ObjToString();
                dRows = gDt.Select("casketdesc='" + service + "'");
                if ( dRows.Length > 0 )
                {
                    rounding = dRows[0]["round"].ObjToDouble();
                    markup = dRows[0]["markup"].ObjToDouble();
                    price = dRows[0]["casketCost"].ObjToDouble();
                    cost = dRows[0]["casketcost"].ObjToDouble();
                    price = cost;
                    price = price * markup;
                    if (rounding > 0D)
                        price = Caskets.RoundTo(price, rounding);
                    cost = price;
                }
                AddMerchandiseRow(dt2, service, cost, futureCost, pastCost, tax, taxAmount );
            }

            string select = "";
            bool found = false;
            price = 0D;
            double pastPrice = 0D;
            DataRow[] dR = dx.Select("type='merchandise'");
            if (dR.Length > 0)
            {
                for (int i = (dR.Length - 1); i >= 0; i--)
                {
                    service = dR[i]["service"].ObjToString();
                    select = dR[i]["select"].ObjToString();
                    found = false;
                    if (dt2.Select("service='" + service + "'").Length > 0)
                        found = true;
                    else if (dt3.Select("service='" + service + "'").Length > 0)
                        found = true;
                    else if (dt4.Select("service='" + service + "'").Length > 0)
                        found = true;
                    if (!found)
                    {
                        price = dR[i]["price"].ObjToDouble();
                        pastPrice = dR[i]["pastPrice"].ObjToDouble();
                        if ( price != 0D || pastPrice != 0D ) // Rmma Zamma last Changed 5/16/2023
                            dt5.ImportRow(dR[i]);
                    }
                }
                //dt5 = dR.CopyToDataTable();
            }
            else
            {
                cmd = "Select * from `services` where `type` = 'merchandise';";
                DataTable ddd = G1.get_db_data(cmd);
                for (int i = (ddd.Rows.Count - 1); i >= 0; i--)
                {
                    service = ddd.Rows[i]["service"].ObjToString();
                    found = false;
                    if (dt2.Select("service='" + service + "'").Length > 0)
                        found = true;
                    else if (dt3.Select("service='" + service + "'").Length > 0)
                        found = true;
                    else if (dt4.Select("service='" + service + "'").Length > 0)
                        found = true;
                    //if (!found)
                    //    dt5.ImportRow(ddd.Rows[i]);
                }
            }
            G1.NumberDataTable(dx);
            dgv.DataSource = dx;

            SetupSelection(dt2, this.repositoryItemCheckEdit5);
            SetupTax(dt2, this.repositoryItemCheckEdit18);
            gridMain2.Columns["select"].Visible = true;
            if (Selecting)
                MatchTable(dx, dt2);
            G1.NumberDataTable(dt2);
            dgv2.DataSource = dt2;

            SetupSelection(dt3, this.repositoryItemCheckEdit8);
            SetupTax(dt3, this.repositoryItemCheckEdit19);
            gridMain3.Columns["select"].Visible = true;
            if (Selecting)
                MatchTable(dx, dt3);
            G1.NumberDataTable(dt3);
            dgv3.DataSource = dt3;

            SetupSelection(dt4, this.repositoryItemCheckEdit11);
            SetupTax(dt4, this.repositoryItemCheckEdit20);
            gridMain4.Columns["select"].Visible = true;
            if (Selecting)
                MatchTable(dx, dt4);
            G1.NumberDataTable(dt4);
            dgv4.DataSource = dt4;

            SetupSelection(dt5, this.repositoryItemCheckEdit14);
            SetupTax(dt5, this.repositoryItemCheckEdit21);
            gridMain5.Columns["select"].Visible = true;
            if (Selecting)
                MatchTable(dx, dt5);
            G1.NumberDataTable(dt5);
            dgv5.DataSource = dt5;

            //ReSelectServices();
        }
        /***********************************************************************************************/
        DataTable funDt = null;
        private bool VerifyLocation ( string locations )
        {
            if (String.IsNullOrWhiteSpace(workServiceId))
                return true;

            if (funDt == null)
                funDt = G1.get_db_data("Select * from `funeralhomes`;");

            string trust = "";
            string loc = "";
            string miniContract;

            miniContract = Trust85.decodeContractNumber(workServiceId, ref trust, ref loc);
            if (String.IsNullOrWhiteSpace(loc))
                return true;

            DataRow[] dRows = funDt.Select("atneedcode='" + loc + "'");
            if (dRows.Length <= 0)
                return true;
            string funeralHome = dRows[0]["LocationCode"].ObjToString();

            string[] Lines = locations.Split(',');
            bool valid = false;

            string location = "";
            for ( int i=0; i<Lines.Length; i++)
            {
                if (Lines[i].Trim() == funeralHome.Trim())
                {
                    valid = true;
                    break;
                }
            }
            return valid;
        }
        /***********************************************************************************************/
        public static string ClassifyCode ( string casketCode )
        {
            string classCode = "";
            if (casketCode.ToUpper() == "MISC")
                classCode = "MISC";
            string str = casketCode.Substring(0, 1).ToUpper();
            if (str == "V" && casketCode.Length == 3)
                classCode = "Vault";
            if (casketCode.Contains("URN"))
                classCode = "URN";
            if (casketCode.Length >= 2)
            {
                str = casketCode.Substring(0, 2).ToUpper();
                if (str == "UV")
                    classCode = "URN";
            }
            return classCode;
        }
        /***********************************************************************************************/
        private void MatchTable(DataTable dt, DataTable dt2)
        {
            string service = "";
            string service2 = "";
            string select = "";
            double price = 0D;
            if (G1.get_column_number(dt, "select") < 0)
                return;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                service = dt.Rows[i]["service"].ObjToString().ToUpper().Trim();
                if ( service.ToUpper().Trim() == "GUARDIAN")
                {
                }
                select = dt.Rows[i]["select"].ObjToString();
                price = dt.Rows[i]["price"].ObjToDouble();
                if (price == 0D)
                    continue;
                if (select == "1")
                {
                    for (int j = 0; j < dt2.Rows.Count; j++)
                    {
                        service2 = dt2.Rows[j]["service"].ObjToString().ToUpper().Trim();
                        if (service == service2)
                        {
                            dt2.Rows[j]["select"] = "1";
                            break;
                        }
                    }
                }
            }
        }
        /***********************************************************************************************/
        private void AddMerchandiseRow(DataTable dx, string service, double cost, double futureCost, double pastCost, string tax, double taxAmount )
        {
            DataRow[] dR = dx.Select("service='" + service + "'");
            if (dR.Length > 0)
                return;
            bool gotPrice1 = false;
            if (G1.get_column_number(dx, "price1") >= 0)
                gotPrice1 = true;
            if (G1.get_column_number(dx, "select") < 0)
                dx.Columns.Add("select");
            DataRow dRow = dx.NewRow();
            dRow["service"] = service;
            dRow["type"] = selectWhat;
            dRow["select"] = "0";
            dRow["price"] = cost;
            if (gotPrice1)
                dRow["price1"] = cost;
            dRow["futurePrice"] = futureCost;
            dRow["pastPrice"] = pastCost;
            dRow["tax"] = tax;
            dRow["taxAmount"] = taxAmount;
            dx.Rows.Add(dRow);
        }
        /***********************************************************************************************/
        private void LoadGPL()
        {
            if (String.IsNullOrWhiteSpace(workGPL))
                return;
            LoadSelection();
        }
        /***********************************************************************************************/
        private void SetupForLocations()
        {
            labBalanceDue.Hide();
            labBalDue.Hide();
            label1.Hide();
            txtMarkup.Hide();
            btnRunMarkup.Hide();
            picMoveFuture.Hide();
            picMovePast.Hide();
            lblLocations.Hide();
            btnAddLocation.Hide();
            btnDeleteLocation.Hide();
            cmbLocations.Hide();
            int left = panelTop.Left;
            int top = panelTop.Top;
            int height = panelTop.Height;
            int width = panelTop.Width;
            height = height / 2;
            panelTop.SetBounds(left, top, width, height);
            gridMain.Columns["SameAsMaster"].Visible = false;
        }
        /***********************************************************************************************/
        private DataTable ReSelectServices(DataTable ddx = null)
        {
            if (workDt == null)
                return null;
            DataTable dt = (DataTable)dgv.DataSource;

            if (G1.get_column_number(dt, "select") < 0)
                dt.Columns.Add("select");

            if (G1.get_column_number(dt, "isPackage") < 0)
                dt.Columns.Add("isPackage");

            if (G1.get_column_number(dt, "pSelect") < 0)
                dt.Columns.Add("pSelect");

            if (G1.get_column_number(dt, "upgrade") < 0)
                dt.Columns.Add("upgrade");

            if (G1.get_column_number(dt, "ignore") < 0)
                dt.Columns.Add("ignore");

            if (G1.get_column_number(dt, "pastPrice") < 0)
                dt.Columns.Add ( "pastPrice", Type.GetType("System.Double") );

            if (G1.get_column_number(dt, "who") < 0)
                dt.Columns.Add("who");

            if (G1.get_column_number(dt, "who") < 0)
                dt.Columns.Add("who");

            if (G1.get_column_number(dt, "DELETED") < 0)
                dt.Columns.Add("DELETED");

            if (G1.get_column_number(dt, "tax") < 0)
                dt.Columns.Add("tax");

            if (G1.get_column_number(dt, "taxAmount") < 0)
                dt.Columns.Add("taxAmount", Type.GetType("System.Double"));

            if (G1.get_column_number(dt, "location") < 0)
                dt.Columns.Add("location");

            if (G1.get_column_number(dt, "locRecord") < 0)
                dt.Columns.Add("locRecord");

            if (G1.get_column_number(workDt, "select") < 0)
                workDt.Columns.Add("select");

            if (G1.get_column_number(workDt, "isPackage") < 0)
                workDt.Columns.Add("isPackage");

            if (G1.get_column_number(workDt, "pSelect") < 0)
                workDt.Columns.Add("pSelect");

            if (G1.get_column_number(workDt, "upgrade") < 0)
                workDt.Columns.Add("upgrade");

            if (G1.get_column_number(workDt, "ignore") < 0)
                workDt.Columns.Add("ignore");

            if (G1.get_column_number(workDt, "pastPrice") < 0)
                workDt.Columns.Add("pastPrice", Type.GetType("System.Double"));

            if (G1.get_column_number(workDt, "who") < 0)
                workDt.Columns.Add("who");

            if (G1.get_column_number(workDt, "who") < 0)
                workDt.Columns.Add("who");

            if (G1.get_column_number(workDt, "DELETED") < 0)
                workDt.Columns.Add("DELETED");

            if (G1.get_column_number(workDt, "tax") < 0)
                workDt.Columns.Add("tax");

            if (G1.get_column_number(workDt, "taxAmount") < 0)
                workDt.Columns.Add("taxAmount", Type.GetType("System.Double"));

            if (G1.get_column_number(workDt, "location") < 0)
                workDt.Columns.Add("location");

            if (G1.get_column_number(workDt, "locRecord") < 0)
                workDt.Columns.Add("locRecord");

            gridMain.OptionsBehavior.ReadOnly = false;

            //DataTable ddx = null;

            if (ddx == null)
            {
                if (workGroup.ToUpper() != "MASTER")
                {
                    string cmd = "Select * from `funeral_gplgroups` where `groupname` = '" + workGroup + "';";
                    ddx = G1.get_db_data(cmd);
                }
            }
            string availableService = "";
            string service = "";
            string isPackage = "";
            string pSelect = "";
            string upgrade = "";
            string ignore = "";
            string tax = "";
            double taxAmount = 0D;
            string location = "";
            string locRecord = "";
            string who = "";
            string deleted = "";
            string select = "";
            double price = 0D;
            double price1 = 0D;
            double pastPrice = 0D;
            double samePrice = 0D;
            double cost = 0D;
            string data = "";
            string type = "";
            string same = "";
            bool found = false;
            bool added = false;
            bool serviceRecord = false;
            string record = "";
            DataRow[] dR = null;
            if (G1.get_column_number(workDt, "!serviceRecord") < 0)
            {
                serviceRecord = true;
            }
            if (G1.get_column_number(workDt, "cost") < 0)
                workDt.Columns.Add("cost", Type.GetType("System.Double"));
            if (G1.get_column_number(workDt, "SameAsMaster") < 0)
                workDt.Columns.Add("SameAsMaster");
            if (G1.get_column_number(workDt, "data") < 0)
                workDt.Columns.Add("data", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "data") < 0)
                dt.Columns.Add("data", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "type") < 0)
                dt.Columns.Add("type");
            if (G1.get_column_number(dt, "status") < 0)
                dt.Columns.Add("status");

            if (G1.get_column_number(workDt, "data") < 0)
                workDt.Columns.Add("data", Type.GetType("System.Double"));
            if (G1.get_column_number(workDt, "type") < 0)
                workDt.Columns.Add("type");
            if (G1.get_column_number(workDt, "status") < 0)
                workDt.Columns.Add("status");

            if (G1.get_column_number(workDt, "select") < 0)
            {
                workDt.Columns.Add("select");
                added = true;
            }
            if (G1.get_column_number(workDt, "mod") < 0)
            {
                workDt.Columns.Add("mod");
                added = true;
            }
            if (G1.get_column_number(workDt, "price1") < 0)
                workDt.Columns.Add("price1");

            string status = "";
            string mod = "";

            for (int i = 0; i < workDt.Rows.Count; i++)
            {
                service = workDt.Rows[i]["service"].ObjToString();
                if (service.ToUpper() == "TRANSFER OF REMAINS TO THE FUNERAL HOME")
                {

                }
                isPackage = workDt.Rows[i]["isPackage"].ObjToString();
                pSelect = workDt.Rows[i]["pSelect"].ObjToString();
                upgrade = workDt.Rows[i]["upgrade"].ObjToString();
                status = workDt.Rows[i]["status"].ObjToString();
                price = workDt.Rows[i]["price"].ObjToDouble();
                data = workDt.Rows[i]["data"].ObjToString();
                price1 = workDt.Rows[i]["price1"].ObjToDouble();
                cost = workDt.Rows[i]["cost"].ObjToDouble();
                type = workDt.Rows[i]["type"].ObjToString();
                same = workDt.Rows[i]["SameAsMaster"].ObjToString();
                ignore = workDt.Rows[i]["ignore"].ObjToString();
                tax = workDt.Rows[i]["tax"].ObjToString();
                taxAmount = workDt.Rows[i]["taxAmount"].ObjToDouble();
                location = workDt.Rows[i]["location"].ObjToString();
                locRecord = workDt.Rows[i]["locRecord"].ObjToString();
                pastPrice = workDt.Rows[i]["pastPrice"].ObjToDouble();
                who = workDt.Rows[i]["who"].ObjToString();
                deleted = workDt.Rows[i]["DELETED"].ObjToString();
                //if (deleted.ToUpper() == "D")
                //    continue;
                select = workDt.Rows[i]["select"].ObjToString();
                pSelect = select;
                mod = workDt.Rows[i]["mod"].ObjToString();
                if (added)
                    select = "1";
                if (same == "1" && ddx != null)
                {
                    dR = ddx.Select("service='" + service + "'");
                    if (dR.Length > 0)
                    {
                        samePrice = dR[0]["price"].ObjToDouble();
                        if (samePrice != 0D)
                        {
                            if (price == 0D)
                                price = samePrice;
                            if (price1 == 0D)
                                price1 = samePrice;
                        }
                    }
                }
                found = false;
                for (int j = 0; j < dt.Rows.Count; j++)
                {
                    availableService = dt.Rows[j]["service"].ObjToString();
                    if (availableService == service)
                    {
                        dt.Rows[j]["select"] = select;
                        dt.Rows[j]["price"] = price;
                        dt.Rows[j]["data"] = cost;
                        dt.Rows[j]["mod"] = mod;
                        dt.Rows[j]["isPackage"] = isPackage;
                        dt.Rows[j]["pSelect"] = pSelect;
                        dt.Rows[j]["ignore"] = ignore;
                        dt.Rows[j]["tax"] = tax;
                        dt.Rows[i]["taxAmount"] = taxAmount;
                        dt.Rows[j]["location"] = location;
                        dt.Rows[j]["locRecord"] = locRecord;
                        dt.Rows[j]["pastPrice"] = pastPrice;
                        dt.Rows[j]["who"] = who;
                        dt.Rows[j]["DELETED"] = deleted;
                        dt.Rows[j]["upgrade"] = upgrade;
                        if (same == "1")
                        {
                            dt.Rows[j]["price"] = price1;
                            if (!String.IsNullOrWhiteSpace(data))
                            {
                                if (G1.validate_numeric(data))
                                    dt.Rows[j]["data"] = data;
                            }
                            //                            dt.Rows[j]["data"] = data;
                            dt.Rows[j]["SameAsMaster"] = "1";
                        }
                        if (!String.IsNullOrWhiteSpace(type))
                            dt.Rows[j]["type"] = type;
                        found = true;
                        break;
                    }
                    else
                    {
                        //dt.Rows[j]["select"] = "0";
                    }
                }
                if (!found)
                {
                    DataRow dRow = dt.NewRow();
                    dRow["service"] = service;
                    if (!String.IsNullOrWhiteSpace(data))
                    {
                        if (G1.validate_numeric(data))
                            dRow["data"] = data;
                    }
                    //                    dRow["data"] = data;
                    dRow["type"] = type;
                    dRow["price"] = price;
                    dRow["select"] = select;
                    dRow["mod"] = mod;
                    dRow["isPackage"] = isPackage;
                    dRow["pSelect"] = pSelect;
                    dRow["upgrade"] = upgrade;
                    dRow["ignore"] = ignore;
                    dRow["tax"] = tax;
                    dRow["taxAmount"] = taxAmount;
                    dRow["location"] = location;
                    dRow["locRecord"] = locRecord;
                    dRow["pastPrice"] = pastPrice;
                    dRow["who"] = who;
                    dRow["DELETED"] = deleted;
                    if ( status.ToUpper() == "IMPORTED")
                        dRow["status"] = status;
                    dt.Rows.Add(dRow);
                }
            }
            G1.NumberDataTable(dt);
            CleanupSelections(dt);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                select = dt.Rows[i]["select"].ObjToString();
                if (select != "1")
                    dt.Rows[i]["select"] = "0";
            }
            //if (selectWhat.ToUpper() == "SERVICE")
            //{
            //    DataRow[] dRows = dt.Select("type='Service'");
            //    if (dRows.Length > 0)
            //        dt = dRows.CopyToDataTable();
            //}
            dgv.DataSource = dt;
            
            ReCalcTotal();
            //dt = (DataTable)dgv.DataSource;
            return dt;
        }
        /***********************************************************************************************/
        private void CleanupSelections(DataTable dt)
        {
            if (G1.get_column_number(dt, "SameAsMaster") < 0)
                return;
            string SameAsMaster = "";
            string pSelect = "";
            string service = "";
            string isPackage = "";

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (SameAsMaster != "0" && SameAsMaster != "1")
                    dt.Rows[i]["sameAsMaster"] = "0";
            }
        }
        /***********************************************************************************************/
        private double mainPackageDiscount = 0D;
        private void LoadPackage()
        {
            DataTable protectDt = (DataTable)dgv.DataSource;

            string serviceRecord = "";
            string group = GetGroup();
            string package = cmbPackage.Text;
            if (String.IsNullOrWhiteSpace(package))
                package = "Master";
            if (String.IsNullOrWhiteSpace(group) || String.IsNullOrWhiteSpace(package))
            {
                MessageBox.Show("***ERROR*** Empty group or package!");
                return;
            }
            gridMain.OptionsView.ShowFooter = true;
            if (group.Trim().ToUpper() == "MASTER")
                gridMain.OptionsView.ShowFooter = false;
            if (package.Trim().ToUpper() == "MASTER")
                gridMain.OptionsView.ShowFooter = false;

            if (group.ToUpper() != "MASTER" && package.ToUpper() == "MASTER")
            {
                cmbLocation_SelectedIndexChanged(null, null);
                this.Cursor = Cursors.Default;
                return;
            }

            loadededLocation = group;
            loadedPackage = package;
            string list = "";
            string cmd = "Select * from `packages` where `groupname` = 'master' and `PackageName` = '" + package + "';";
            DataTable dx = G1.get_db_data(cmd);
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                serviceRecord = dx.Rows[i]["!serviceRecord"].ObjToString();
                if (String.IsNullOrWhiteSpace(serviceRecord))
                    continue;
                list += "'" + serviceRecord + "',";
            }
            list = list.TrimEnd(',');
            if (!String.IsNullOrWhiteSpace(list))
            {
                cmd = "Select * from `packages` p LEFT JOIN `funeral_master` s ON p.`!serviceRecord` = s.`record` where s.`record` IN (" + list + ") ";
                //                cmd = "Select * from `packages` p RIGHT JOIN `services` s ON p.`!serviceRecord` = s.`record` where s.`record` IN (" + list + ") ";
                cmd += " and `groupname` = 'master' and `PackageName` = '" + package + "' ";
                cmd += ";";
                //                cmd = "Select * from `services` where `record` IN (" + list + ");";
                //if (!String.IsNullOrWhiteSpace(group) && !String.IsNullOrWhiteSpace(package))
                //{
                //    cmd = "Select * from `packages` p LEFT JOIN `funeral_gplgroups` s ON p.`!serviceRecord` = s.`record` where s.`record` IN (" + list + ") ";
                //    cmd += " and s.`groupname` = '" + group + "'  and `PackageName` = '" + package + "' ";
                //    cmd += ";";
                //}
            }
            else
            {
                cmd = "Select * from `packages` p JOIN `funeral_master` s ON p.`!serviceRecord` = s.`record` where `service` = 'xyzzyxxxx';";
            }
            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("num");
            dt.Columns.Add("mod");
            dt.Columns.Add("select");
            //            dt.Columns.Add("SameAsMaster");
            dt.Columns.Add("total", Type.GetType("System.Double"));
            SetupSelection(dt);
            SetupTax(dt);
            string record = "";
            string service = "";
            DataRow[] dR = null;
            DataTable ddx = null;
            bool changed = false;
            if (!String.IsNullOrWhiteSpace(group) && !String.IsNullOrWhiteSpace(package))
            {
                cmd = "Select * from `funeral_gplgroups` where `groupname` = '" + group + "';";
                ddx = G1.get_db_data(cmd);
                if (package.ToUpper() != "MASTER")
                {
                    if ( group.Trim().ToUpper() != "MASTER")
                    {
                        cmd = "Select * from `funeral_master` order by `order`, `record`;";
                        DataTable ddxx = G1.get_db_data(cmd);
                        FixAllData(ddxx);
                        SetupSameAsMaster(ddx, ddxx);
                    }
                    cmd = "Select * from `packages` where `groupname` = '" + group + "' and `PackageName` = '" + package + "';";
                    DataTable newddx = G1.get_db_data(cmd);
                    for ( int i=0; i<newddx.Rows.Count; i++)
                    {
                        record = newddx.Rows[i]["!serviceRecord"].ObjToString();
                        dR = ddx.Select("!masterRecord='" + record + "'");
                        if ( dR.Length > 0 )
                        {
                            if (dR[0]["price"].ObjToDouble() <= 0D)
                                dR[0]["price"] = newddx.Rows[i]["price"].ObjToString();
                            if (dR[0]["futurePrice"].ObjToDouble() <= 0D)
                                dR[0]["futurePrice"] = newddx.Rows[i]["futurePrice"].ObjToString();
                            if (dR[0]["pastPrice"].ObjToDouble() <= 0D)
                                dR[0]["pastPrice"] = newddx.Rows[i]["pastPrice"].ObjToString();
                        }
                    }
                }
                if (ddx.Rows.Count > 0 && !Selecting)
                {
                    changed = SetupSameAsMaster(dt, ddx);
                    if ( changed )
                    {
                        modified = true;
                        btnSave.Show();
                    }
                }
                else
                    SetupSameAsMaster(dt, ddx);
                if (Selecting && package.ToUpper() == "MASTER")
                    dt = ddx.Copy();
            }
            else
                SetupSameAsMaster(dt);
            if ( Selecting && package.ToUpper() != "MASTER")
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                    dt.Rows[i]["price1"] = dt.Rows[i]["price"].ObjToDouble();
            }
            CleanupSelections(dt);
            G1.NumberDataTable(dt);

            protectDt = (DataTable)dgv.DataSource;

            dgv.DataSource = dt;
            if (package.ToUpper() != "MASTER")
            {
                gridMain.Columns["basicService"].Visible = false;
                gridMain.Columns["plonly"].Visible = false;
                dR = dt.Select("service='Package Price'");
                if (dR.Length > 0)
                {
                    double packagePrice = dR[0]["price"].ObjToDouble();
                    double packageFuture = dR[0]["futurePrice"].ObjToDouble();
                    double totalPrice = 0D;
                    double totalFuture = 0D;
                    service = "";
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        service = dt.Rows[i]["service"].ObjToString().ToUpper();
                        if (service == "TOTAL LISTED PRICE")
                            continue;
                        if (service == "PACKAGE DISCOUNT")
                        {
                            //mainPackageDiscount = dt.Rows[i]["price"].ObjToDouble();
                            continue;
                        }
                        if (service == "PACKAGE PRICE")
                            continue;
                        totalPrice += dt.Rows[i]["price"].ObjToDouble();
                        totalFuture += dt.Rows[i]["futurePrice"].ObjToDouble();
                    }
                    double totalListedPrice = totalPrice;
                    double packageDiscount = totalPrice - packagePrice;
                    if (packageDiscount > 0D)
                        packageDiscount = packageDiscount * (-1D);

                    double totalListedFuture = totalFuture;
                    double packageFutureDiscount = totalFuture - packageFuture;
                    if (packageFutureDiscount > 0D)
                        packageFutureDiscount = packageFutureDiscount * (-1D);

                    dR = dt.Select("service='Total Listed Price'");
                    if (dR.Length > 0)
                    {
                        dR[0]["price"] = totalListedPrice.ToString();
                        dR[0]["futurePrice"] = totalListedFuture;
                    }

                    dR = dt.Select("service='Package Discount'");
                    if (dR.Length > 0)
                    {
                        dR[0]["price"] = packageDiscount.ToString();
                        dR[0]["futurePrice"] = packageFutureDiscount;
                        if ( mainPackageDiscount == 0D)
                            mainPackageDiscount = packageDiscount;
                    }
                }
            }
            else
            {
                gridMain.Columns["basicService"].Visible = true;
                gridMain.Columns["plonly"].Visible = true;
            }

            if (Selecting && package.ToUpper() != "MASTER")
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                    dt.Rows[i]["price1"] = dt.Rows[i]["price"].ObjToDouble();
            }
            if (package.ToUpper() != "MASTER")
            {
                if ( selectWhat.ToUpper() == "SERVICE")
                {
                    ProtectMerchandise(dt);
                }
                //AddMerchandise();
                //tabControl1.TabPages.Add(tabCaskets);
                //tabControl1.TabPages.Add(tabVaults);
                //tabControl1.TabPages.Add(tabUrns);
                //tabControl1.TabPages.Add(tabMisc);
            }
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void LoadData()
        {
            this.Cursor = Cursors.WaitCursor;
            btnSave.Hide();
            if (workingLocation)
            {
                LoadGPL();
                this.Cursor = Cursors.Default;
                return;
            }
            string location = GetGroup();
            string package = cmbPackage.Text;
            if (!Selecting)
            {
                if (String.IsNullOrWhiteSpace(package))
                    package = "Master";
                if (location != "Master")
                {
                    LoadPackage();
                    this.Cursor = Cursors.Default;
                    return;
                }
                if (package != "Master")
                {
                    pictureAdd.Show();
                    pictureDelete.Show();
                    btnInsert.Hide();
                    picRowDown.Show();
                    picRowUp.Show();
                    picAddMerch.Hide();
                    btnInsertMerch.Hide();
                    LoadPackage();
                    this.Cursor = Cursors.Default;
                    return;
                }
            }
            if (package == "Master")
            {
                if (location.Trim().ToUpper() != "MASTER")
                {
                    cmbLocation_SelectedIndexChanged(null, null);
                    this.Cursor = Cursors.Default;
                    return;
                }
            }
            if (package.ToUpper() != "MASTER")
            {
                gridMain.Columns["basicService"].Visible = false;
                gridMain.Columns["plonly"].Visible = false;
            }
            else
            {
                pictureAdd.Show();
                pictureDelete.Show();
                btnInsert.Show();
                picRowDown.Show();
                picRowUp.Show();
                picAddMerch.Show();
                btnInsertMerch.Show();
                gridMain.Columns["basicService"].Visible = true;
                gridMain.Columns["plonly"].Visible = true;
            }

            gridMain.OptionsView.ShowFooter = false;
            if (!String.IsNullOrWhiteSpace(workGroup) && !String.IsNullOrWhiteSpace(workPackage))
            {
                loading = true;
                cmbGroups.Text = workGroup;
                cmbPackage.Text = workPackage;
                btnAllOn.Hide();
                btnAllOff.Show();
                if (workPackage.ToUpper() != "MASTER")
                {
                    btnAllOn.Show();
                    btnAllOff.Show();
                }
                loading = false;
                LoadPackage();
                gridMain.Columns["data"].Visible = false;
                gridMain.Columns["SameAsMaster"].Visible = false;
                DataTable dt = (DataTable)dgv.DataSource;
                SetupBasicServices(dt);
                SetupPLOnly(dt);
                SetupNoSelect(dt);
            }
            else
            {
                string cmd = "Select * from `funeral_master` order by `order`;";
                //if (!String.IsNullOrWhiteSpace(workGroup))
                //    cmd = "Select * from `funeral_gplgroups` where `groupname` = '" + workGroup + "';";
                //                string cmd = "Select * from `services` order by `order`, `record`;";
                DataTable dt = G1.get_db_data(cmd);
                FixAllData(dt);
                dt.Columns.Add("num");
                dt.Columns.Add("mod");
                //            dt.Columns.Add("agreement");
                dt.Columns.Add("select");
                if (G1.get_column_number(dt, "SameAsMaster") < 0)
                    dt.Columns.Add("SameAsMaster");
                dt.Columns.Add("total", Type.GetType("System.Double"));
                SetupSelection(dt);
                SetupTax(dt);
                if (workGroup.ToUpper() != "MASTER")
                {
                    cmd = "Select * from `funeral_gplgroups` where `groupname` = '" + workGroup + "';";
                    DataTable ddx = G1.get_db_data(cmd);
                    if (ddx.Rows.Count > 0)
                        SetupSameAsMaster(dt, ddx);
                    else
                        SetupSameAsMaster(dt);
                }
                else
                    SetupSameAsMaster(dt);
                CleanupSelections(dt);
                SetupBasicServices(dt);
                SetupPLOnly(dt);
                G1.NumberDataTable(dt);

                ProtectMerchandise(dt);

                dgv.DataSource = dt;
            }

            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void ProtectMerchandise ( DataTable dt )
        {
            if (selectWhat.ToUpper() != "SERVICE")
                return;
            if (workDt == null)
                return;
            DataRow[] dRows = workDt.Select("type='Merchandise' OR type='Cash Advance'");
            if (dRows.Length <= 0)
                return;
            DataTable workMerchandiseDt = dRows.CopyToDataTable();
            DataTable backupDt = workMerchandiseDt.Clone();

            DataRow[] ddRows = null;
            string service = "";
            for (int i = 0; i < workMerchandiseDt.Rows.Count; i++)
            {
                service = workMerchandiseDt.Rows[i]["service"].ObjToString();
                ddRows = dt.Select("service='" + service + "'");
                if (ddRows.Length <= 0)
                {
                    G1.copy_dt_row(workMerchandiseDt, i, backupDt, backupDt.Rows.Count);
                }
            }
            if (backupDt.Rows.Count > 0)
                G1.HardCopyDataTable(backupDt, dt);
            dgv.DataSource = dt;
        }
        /***********************************************************************************************/
        public static void FixAllData ( DataTable dt)
        {
            string service = "";
            string newService = "";
            string str = "";
            string type = "";
            bool gotit = false;
            string[] Lines = null;
            bool gotType = false;
            if (G1.get_column_number(dt, "type") >= 0)
                gotType = true;
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                service = dt.Rows[i]["service"].ObjToString();
                if ( !String.IsNullOrWhiteSpace ( service))
                    service = G1.force_lower_line(service);
                if (service.IndexOf("Usps") >= 0)
                    service = service.Replace("Usps", "USPS");
                if (!gotType)
                    continue;
                type = dt.Rows[i]["type"].ObjToString().ToUpper();
                if (type == "MERCHANDISE")
                {
                    Lines = service.Split(' ');
                    if ( Lines.Length > 0 )
                    {
                        newService = "";
                        gotit = false;
                        string firstWord = "";
                        for ( int j=0; j<Lines.Length; j++)
                        {
                            str = Lines[j].Trim();
                            if (String.IsNullOrWhiteSpace(str))
                                continue;
                            if (String.IsNullOrWhiteSpace(firstWord))
                            {
                                firstWord = str;
                                continue;
                            }
                            if (str.IndexOf(firstWord) >= 0)
                                gotit = true;
                            newService += str + " ";
                        }
                        if (!gotit)
                            service = firstWord + " " + newService.Trim();
                        else
                            service = newService.Trim();
                    }
                }
                dt.Rows[i]["service"] = service;
            }
        }
        /***********************************************************************************************/
        private void SetupSelection(DataTable dt, DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew = null)
        {
            if (selectnew == null)
                selectnew = this.repositoryItemCheckEdit3;
            selectnew.NullText = "";
            selectnew.ValueChecked = "1";
            selectnew.ValueUnchecked = "0";
            selectnew.ValueGrayed = "";
            if (G1.get_column_number(dt, "select") < 0)
                dt.Columns.Add("select");
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["select"] = "0";
        }
        /***********************************************************************************************/
        private void SetupTax(DataTable dt, DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew = null)
        {
            bool hideColumn = false;
            if (selectnew == null)
            {
                selectnew = this.repositoryItemCheckEdit17;
                hideColumn = true;
            }
            selectnew.NullText = "";
            selectnew.ValueChecked = "Y";
            selectnew.ValueUnchecked = "";
            selectnew.ValueGrayed = "";
            if (G1.get_column_number(dt, "tax") < 0)
                dt.Columns.Add("tax");
            string tax = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                tax = dt.Rows[i]["tax"].ObjToString().ToUpper();
                if ( tax == "Y")
                    dt.Rows[i]["tax"] = "Y";
                else
                    dt.Rows[i]["tax"] = "";
            }
            if (hideColumn)
                gridMain.Columns["tax"].Visible = false;
        }
        /***********************************************************************************************/
        private void SetupSameAsMasterXXXXX(DataTable dt, DataTable ddx = null)
        {
            DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew = this.repositoryItemCheckEdit1;
            selectnew.NullText = "";
            selectnew.ValueChecked = "1";
            selectnew.ValueUnchecked = "0";
            selectnew.ValueGrayed = "";
            if (dt != null)
            {
                if (ddx == null)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                        dt.Rows[i]["SameAsMaster"] = "0";
                }
                else
                {
                    DataRow[] dR = null;
                    string select = "";
                    string service = "";
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        select = dt.Rows[i]["SameAsMaster"].ObjToString();
                        if (select == "1")
                        {
                            dt.Rows[i]["SameAsMaster"] = "1";
                            service = dt.Rows[i]["service"].ObjToString();
                            dR = ddx.Select("service='" + service + "'");
                            if (dR.Length > 0)
                            {
                                dt.Rows[i]["price"] = dR[0]["price"];
                            }
                        }
                        else
                        {
                            dt.Rows[i]["SameAsMaster"] = "0";
                        }
                    }
                }
            }
        }
        /***********************************************************************************************/
        private bool SetupSameAsMaster(DataTable dt, DataTable ddx = null)
        {
            DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew = this.repositoryItemCheckEdit1;
            selectnew.NullText = "";
            selectnew.ValueChecked = "1";
            selectnew.ValueUnchecked = "0";
            selectnew.ValueGrayed = "";
            double price1 = 0D;
            double price2 = 0D;
            bool changed = false;
            bool GPLddx = false;
            if (dt != null)
            {
                if (ddx == null)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                        dt.Rows[i]["SameAsMaster"] = "0";
                }
                else
                {
                    if (G1.get_column_number(ddx, "groupname") >= 0)
                        GPLddx = true;
                    DataRow[] dR = null;
                    string select = "";
                    string service = "";
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (!GPLddx)
                        {
                            select = dt.Rows[i]["SameAsMaster"].ObjToString();
                            if (select == "1")
                            {
                                dt.Rows[i]["SameAsMaster"] = "1";
                                service = dt.Rows[i]["service"].ObjToString();
                                dR = ddx.Select("service='" + service + "'");
                                if (dR.Length > 0)
                                {
                                    dt.Rows[i]["price"] = dR[0]["price"];
                                }
                            }
                            else
                                dt.Rows[i]["SameAsMaster"] = "0";
                        }
                        else
                        {
                            service = dt.Rows[i]["service"].ObjToString();
                            dR = ddx.Select("service='" + service + "'");
                            if (dR.Length > 0)
                            {
                                select = dR[0]["SameAsMaster"].ObjToString();
                                if (select == "1")
                                {
                                    price1 = dR[0]["price"].ObjToDouble();
                                    price2 = dt.Rows[i]["price1"].ObjToDouble();
                                    if (price1 != price2)
                                    {
                                        changed = true;
                                        dR[0]["price"] = price2;
                                    }
                                    dt.Rows[i]["SameAsMaster"] = "1";
                                    dt.Rows[i]["price"] = dR[0]["price"];
                                    dt.Rows[i]["futurePrice"] = dR[0]["futurePrice"];
                                    dt.Rows[i]["pastPrice"] = dR[0]["pastPrice"];
                                }
                                else
                                {
                                    dt.Rows[i]["SameAsMaster"] = "0";
                                    dt.Rows[i]["price"] = dR[0]["price"];
                                    dt.Rows[i]["futurePrice"] = dR[0]["futurePrice"];
                                    dt.Rows[i]["pastPrice"] = dR[0]["pastPrice"];
                                }
                            }
                            else
                                dt.Rows[i]["SameAsMaster"] = "0";
                        }
                    }
                }
            }
            return changed;
        }
        /***********************************************************************************************/
        private void SetupBasicServices(DataTable dt)
        {
            DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew = this.repositoryItemCheckEdit2;
            selectnew.NullText = "";
            selectnew.ValueChecked = "1";
            selectnew.ValueUnchecked = "0";
            selectnew.ValueGrayed = "";
            if (dt != null)
            {
                if (G1.get_column_number(dt, "basicService") < 0)
                    return;
                string basic = "";
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    basic = dt.Rows[i]["basicService"].ObjToString();
                    if (basic == "1")
                        dt.Rows[i]["basicService"] = "1";
                    else
                        dt.Rows[i]["basicService"] = "0";
                }
            }
        }
        /***********************************************************************************************/
        private void SetupPLOnly(DataTable dt)
        {
            DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew = this.repositoryItemCheckEdit16;
            selectnew.NullText = "";
            selectnew.ValueChecked = "1";
            selectnew.ValueUnchecked = "0";
            selectnew.ValueGrayed = "";
            if (dt != null)
            {
                if (G1.get_column_number(dt, "plonly") < 0)
                    return;
                string basic = "";
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    basic = dt.Rows[i]["plonly"].ObjToString();
                    if (basic == "1")
                        dt.Rows[i]["plonly"] = "1";
                    else
                        dt.Rows[i]["plonly"] = "0";
                }
            }
        }
        /***********************************************************************************************/
        private void SetupNoSelect(DataTable dt)
        {
            DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew = this.repositoryItemCheckEdit22;
            selectnew.NullText = "";
            selectnew.ValueChecked = "1";
            selectnew.ValueUnchecked = "0";
            selectnew.ValueGrayed = "";
            if (dt != null)
            {
                if (G1.get_column_number(dt, "noSelect") < 0)
                    return;
                string basic = "";
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    basic = dt.Rows[i]["noSelect"].ObjToString();
                    if (basic == "1")
                        dt.Rows[i]["noSelect"] = "1";
                    else
                        dt.Rows[i]["noSelect"] = "0";
                }
            }
        }
        /***********************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (dgv.Visible)
                SetSpyGlass(gridMain);
            else if (dgv2.Visible)
                SetSpyGlass(gridMain2);
            else if (dgv3.Visible)
                SetSpyGlass(gridMain3);
            else if (dgv4.Visible)
                SetSpyGlass(gridMain4);
            else if (dgv5.Visible)
                SetSpyGlass(gridMain5);
        }
        /***********************************************************************************************/
        private void SetSpyGlass(DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView grid)
        {
            G1.ShowHideFindPanel(grid);

            //if (grid.OptionsFind.AlwaysVisible == true)
            //    grid.OptionsFind.AlwaysVisible = false;
            //else
            //    grid.OptionsFind.AlwaysVisible = true;
        }
        /***********************************************************************************************/
        private void repositoryItemCheckEdit3_Click(object sender, EventArgs e)
        {
            if (loading)
                return;
            DevExpress.XtraEditors.CheckEdit check = (DevExpress.XtraEditors.CheckEdit)sender;
            bool isChecked = true;
            if (!check.Checked)
                isChecked = false;
            //DataRow dr = gridMain.GetFocusedDataRow();
            DataRow dr = GetCurrentDataRow();

            string service = dr["service"].ObjToString();
            string record = dr["record"].ObjToString();
            int rowHandle = GetCurrentGridView().FocusedRowHandle;
            string select = dr["select"].ObjToString();
            double price = dr["price"].ObjToDouble();
            int row = GetCurrentGridView().GetDataSourceRowIndex(rowHandle);
            DataTable myDt = GetCurrentDataTable();
            if (isChecked)
                myDt.Rows[row]["select"] = "1";
            else
                myDt.Rows[row]["select"] = "0";
            if (!isChecked)
                myDt.Rows[row]["select"] = "0";
            else
                myDt.Rows[row]["select"] = "1";

            //if (isChecked)
            //    myDt.Rows[row]["select"] = "1";
            //else
            //    myDt.Rows[row]["select"] = "0";

            myDt.Rows[row]["mod"] = "1";

            if (G1.get_column_number(myDt, "ModMod") > 0)
                myDt.Rows[row]["ModMod"] = "Y";

            GetCurrentDataGrid().DataSource = myDt;
            //labBalanceDue.Text = "$" + G1.ReformatMoney(0D);
            ReCalcTotal((DataTable)dgv.DataSource); // ramma
            ReCalcTotal((DataTable)dgv2.DataSource);
            ReCalcTotal((DataTable)dgv3.DataSource);
            ReCalcTotal((DataTable)dgv4.DataSource);
            ReCalcTotal((DataTable)dgv5.DataSource);
            modified = true;
            btnSave.Show();
            //if (1 == 1)
            //    return;
            //string doit = "0";
            //if (workingLocation)
            //{
            //    doit = select;
            //    if (doit == "0")
            //        doit = "1";
            //    else
            //        doit = "0";
            //}
            //else
            //{
            //    doit = select;
            //    if (doit == "0")
            //        doit = "1";
            //    else
            //        doit = "0";
            //}
            //loading = true;
            ////dr["select"] = doit;
            ////int row = GetCurrentGridView().GetDataSourceRowIndex(rowHandle);
            ////DataTable myDt = GetCurrentDataTable();
            ////myDt.Rows[row]["select"] = doit;
            ////GetCurrentDataGrid().DataSource = myDt;
            ////GetCurrentGridView().RefreshData();
            ////GetCurrentGridView().RefreshRow(rowHandle);
            ////GetCurrentDataGrid().Refresh();
            ////GetCurrentGridView().RefreshEditor(true);
            //loading = false;
            //modified = true;
            //labBalanceDue.Text = "$" + G1.ReformatMoney(0D);
            //ReCalcTotal((DataTable)dgv.DataSource);
            //ReCalcTotal((DataTable)dgv2.DataSource);
            //ReCalcTotal((DataTable)dgv3.DataSource);
            //ReCalcTotal((DataTable)dgv4.DataSource);
            //ReCalcTotal((DataTable)dgv5.DataSource);
            //GetCurrentGridView().RefreshData();
            ////gridMain.RefreshData();
            //GetCurrentGridView().RefreshRow(rowHandle);
            ////gridMain.RefreshRow(rowHandle);
            ////dgv.Refresh();
            //GetCurrentDataGrid().Refresh();
            //btnSave.Show();
        }
        /****************************************************************************************/
        private DevExpress.XtraGrid.GridControl GetCurrentDataGrid()
        {
            DevExpress.XtraGrid.GridControl currentDGV = null;
            if (dgv.Visible)
                currentDGV = dgv;
            else if (dgv2.Visible)
                currentDGV = dgv2;
            else if (dgv3.Visible)
                currentDGV = dgv3;
            else if (dgv4.Visible)
                currentDGV = dgv4;
            else if (dgv5.Visible)
                currentDGV = dgv5;
            return currentDGV;
        }
        /****************************************************************************************/
        private DataTable GetCurrentDataTable()
        {
            DataTable dt = null;
            if (dgv.Visible)
                dt = (DataTable)dgv.DataSource;
            else if (dgv2.Visible)
                dt = (DataTable)dgv2.DataSource;
            else if (dgv3.Visible)
                dt = (DataTable)dgv3.DataSource;
            else if (dgv4.Visible)
                dt = (DataTable)dgv4.DataSource;
            else if (dgv5.Visible)
                dt = (DataTable)dgv5.DataSource;
            return dt;
        }
        /****************************************************************************************/
        private DataRow GetCurrentDataRow()
        {
            DataRow dr = null;
            if (dgv.Visible)
                dr = gridMain.GetFocusedDataRow();
            else if (dgv2.Visible)
                dr = gridMain2.GetFocusedDataRow();
            else if (dgv3.Visible)
                dr = gridMain3.GetFocusedDataRow();
            else if (dgv4.Visible)
                dr = gridMain4.GetFocusedDataRow();
            else if (dgv5.Visible)
                dr = gridMain5.GetFocusedDataRow();
            return dr;
        }
        /****************************************************************************************/
        private DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView GetCurrentGridView()
        {
            DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gv = null;
            if (dgv.Visible)
                gv = gridMain;
            else if (dgv2.Visible)
                gv = gridMain2;
            else if (dgv3.Visible)
                gv = gridMain3;
            else if (dgv4.Visible)
                gv = gridMain4;
            else if (dgv5.Visible)
                gv = gridMain5;
            return gv;
        }
        /***************************************************************************************/
        public delegate void d_void_eventdone_dt(DataTable dt, string what );
        public event d_void_eventdone_dt SelectDone;
        protected void OnSelectDone(DataTable dt)
        {
            string what = cmbPackage.Text.Trim();
            if (allOff)
                what = "DELETE ALL";
            SelectDone?.Invoke(dt, what );
        }
        /***********************************************************************************************/
        private void ReCalcTotal(DataTable dt = null)
        {
            string select = "";
            string type = "";
            double price = 0D;
            double total = 0D;
            string data = "";
            if (dt == null)
            {
                if (dgv.DataSource == null)
                    return;
                dt = (DataTable)dgv.DataSource;
                if (dt == null)
                    return;
                return;
            }
            else
            {
                string str = labBalanceDue.Text.Trim();
                str = str.Replace("$", "");
                str = str.Replace(",", "");
                if (!String.IsNullOrWhiteSpace(str))
                {
                    if ( G1.validate_numeric ( str ))
                        total = str.ObjToDouble();
                }
            }

            if (dt == null)
                return;
            if (G1.get_column_number(dt, "total") < 0)
                dt.Columns.Add("total", Type.GetType("System.Double"));

            string service = "";
            double totalPrice = 0D;
            double totalUnselected = 0D;

            double packagePrice = 0D;
            double packageDiscount = 0D;
            double newTotal = 0D;
            //gotPackage = false;

            int packageDiscountRow = -1;


            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    if (G1.get_column_number(dt, "select") >= 0)
                        select = dt.Rows[i]["select"].ObjToString();
                    else if (G1.get_column_number(dt, "SameAsMaster") >= 0)
                        select = dt.Rows[i]["SameAsMaster"].ObjToString();
                    else
                        continue;
                    type = dt.Rows[i]["type"].ObjToString();
                    if (type.ToUpper() != selectWhat.ToUpper())
                        continue;

                    service = dt.Rows[i]["service"].ObjToString().ToUpper();
                    if (service == "TOTAL LISTED PRICE")
                    {
                        //gotPackage = true;
                        continue;
                    }
                    if (service == "PACKAGE DISCOUNT")
                    {
                        //gotPackage = true;
                        packageDiscount = dt.Rows[i]["price"].ObjToDouble();
                        packageDiscountRow = i;
                        continue;
                    }
                    if (service == "PACKAGE PRICE")
                    {
                        //gotPackage = true;
                        packagePrice = dt.Rows[i]["price"].ObjToDouble();
                        continue;
                    }

                    totalPrice += dt.Rows[i]["price"].ObjToDouble();
                    if (select == "1")
                    {
                        price = dt.Rows[i]["price"].ObjToDouble();
                        data = "";
                        if (G1.get_column_number(dt, "data") >= 0)
                            data = dt.Rows[i]["data"].ObjToString();
                        if (G1.validate_numeric(data))
                        {
                            if (!String.IsNullOrWhiteSpace(data))
                            {
                                if (data.ObjToDouble() > 0)
                                    price = data.ObjToDouble();
                            }
                            total = total + price;
                            dt.Rows[i]["total"] = total;
                            newTotal += price;
                        }
                        else
                        {
                            total += price;
                            newTotal += price;
                        }
                    }
                    else
                    {
                        dt.Rows[i]["total"] = 0D;
                        totalUnselected += dt.Rows[i]["price"].ObjToDouble();
                    }
                }
                catch (Exception ex)
                {
                }
            }
            if ( gotPackage )
            {
                if (mainPackageDiscount == 0D && packageDiscount != 0D && packageDiscountRow > 0)
                    dt.Rows[packageDiscountRow]["price"] = packageDiscount;
                else
                {
                    packageDiscount = mainPackageDiscount + totalUnselected;
                    if (packageDiscountRow >= 0)
                        dt.Rows[packageDiscountRow]["price"] = packageDiscount;
                }
                total = packagePrice;
            }
            //labBalanceDue.Text = "$" + G1.ReformatMoney(newTotal);
            //labBalanceDue.Refresh();
        }
        /***********************************************************************************************/
        private bool CheckForSaving()
        {
            if (!modified)
                return true;
            DialogResult result = MessageBox.Show("***Question***\nMerchandise has been modified!\nWould you like to save your changes?", "Select Merchandise Dialog", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (result == DialogResult.Cancel)
            {
                savePackage = "";
                return false;
            }
            modified = false;
            if (result == DialogResult.No)
                return true;
            DataTable dt = (DataTable)dgv.DataSource;
            if (String.IsNullOrWhiteSpace(loadededLocation) || String.IsNullOrWhiteSpace(loadedPackage))
                return true;
            if (loadededLocation == "MASTER" && loadedPackage == "MASTER")
                SaveServices();
            else
                SaveServices(dt);
            return true;
        }
        /***********************************************************************************************/
        private void Services_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!modified)
                return;
            DialogResult result = MessageBox.Show("***Question***\nServices have been modified!\nWould you like to save your changes?", "Select Services Dialog", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            if (result == DialogResult.Cancel)
            {
                e.Cancel = true;
                return;
            }
            modified = false;
            if (result == DialogResult.No)
                return;
            DataTable dt = (DataTable)dgv.DataSource;
            if (!Selecting && !workingLocation)
            {
                loadededLocation = cmbGroups.Text.ToUpper();
                loadedPackage = cmbPackage.Text.ToUpper();
                if (loadededLocation == "MASTER" && loadedPackage == "MASTER")
                    SaveServices();
                else
                    SaveServices(dt);
                return;
            }
            DataTable dx = dt.Clone();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (dt.Rows[i]["select"].ObjToString() == "1")
                    G1.copy_dt_row(dt, i, dx, dx.Rows.Count);
            }
            OnSelectDone(dx);
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
            printableComponentLink1.Landscape = false;

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
            if (Selecting)
                Printer.DrawQuad(6, 8, 4, 4, this.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            else
                Printer.DrawQuad(6, 8, 4, 4, "Services Report", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);


            //            Printer.DrawQuadTicks();
            DateTime date = DateTime.Now;
            string workDate = date.Month.ToString("D2") + "/" + date.Year.ToString("D4");
            Printer.SetQuadSize(24, 12);
            font = new Font("Ariel", 9, FontStyle.Bold);
            //            Printer.DrawQuad(20, 8, 5, 4, "Report Month:" + workDate, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
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
            savePackage = cmbPackage.Text.Trim();
            if (cmbPackage.Text.Trim().ToUpper() != "MASTER")
            {
                LoadPackage(cmbPackage.Text.Trim());
                return;
            }
            DataTable dt = (DataTable)dgv.DataSource;
            string group = GetGroup();
            if (group.ToUpper() != "MASTER")
            {
                LoadPackage(cmbPackage.Text.Trim());
                return;
            }
            int lines = 1;
            for (int i = 0; i < lines; i++)
            {
                DataRow dRow = dt.NewRow();
                dRow["num"] = dt.Rows.Count.ObjToInt32();
                dRow["type"] = "service";
                dRow["service"] = "New Service";
                dRow["basicService"] = "0";
                dt.Rows.Add(dRow);
            }
            CleanupSelections(dt);
            dgv.DataSource = dt;

            int row = dt.Rows.Count - 1;
            gridMain.SelectRow(row);
            gridMain.FocusedRowHandle = row;
            gridMain.RefreshData();
            dgv.RefreshDataSource();
            dgv.Refresh();
            ColumnView columnview = (ColumnView)dgv.FocusedView;
            columnview.MoveLast();
        }
        /***********************************************************************************************/
        private void pictureDelete_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            string service = dr["service"].ObjToString();
            DialogResult result = MessageBox.Show("***Question*** Are you sure you want to delete this service (" + service + ") ?", "Delete Service Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;
            //gridMain.DeleteRow(gridMain.FocusedRowHandle);
            DataTable dt = (DataTable)dgv.DataSource;
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
            //dgv.DataSource = dt;
            gridMain.RefreshEditor(true);
            gridMain.RefreshData();
            dgv.Refresh();

            gridMain.FocusedRowHandle = firstRow;
            gridMain.SelectRow(firstRow);
            btnSave.Show();
            modified = true;
        }
        /***********************************************************************************************/
        private void pictureDelete_ClickX(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            string service = dr["service"].ObjToString();
            DialogResult result = MessageBox.Show("***Question*** Are you sure you want to delete this service (" + service + ") ?", "Delete Service Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;

            DataTable dt = (DataTable)dgv.DataSource;
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

            //            gridMain.FocusedRowHandle = firstRow;
            loading = false;
            if (firstRow > (dt.Rows.Count - 1))
                firstRow = (dt.Rows.Count - 1);
            CleanupSelections(dt);
            dgv.DataSource = dt;
            gridMain.RefreshData();
            dgv.Refresh();

            gridMain.FocusedRowHandle = firstRow;
            gridMain.SelectRow(firstRow);
        }
        /***********************************************************************************************/
        private void picRowUp_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable dt = (DataTable)dgv.DataSource;
                if (dt.Rows.Count <= 0)
                    return;
                DataRow dr = gridMain.GetFocusedDataRow();
                int rowHandle = gridMain.FocusedRowHandle;
                int row = gridMain.GetDataSourceRowIndex(rowHandle);
                if (row == 0)
                    return; // Already at the first row
                //MoveRowUp(dt, rowHandle);
                massRowsUp(dt, row);
                dt.AcceptChanges();
                dgv.DataSource = dt;
                gridMain.ClearSelection();
                gridMain.SelectRow(rowHandle - 1);
                gridMain.FocusedRowHandle = rowHandle - 1;
                gridMain.RefreshData();
                dgv.Refresh();
                btnSave.Show();
                modified = true;
            }
            catch (Exception ex)
            {
            }
        }
        /***************************************************************************************/
        private void massRowsUp(DataTable dt, int row)
        {
            string type = "";
            if (G1.get_column_number(dt, "count") < 0)
                dt.Columns.Add("Count", Type.GetType("System.Int32"));
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["Count"] = i.ToString();
            type = dt.Rows[row]["type"].ObjToString();
            int mainRow = row;
            for (; ; )
            {
                dt.Rows[mainRow]["Count"] = (row - 1).ToString();
                dt.Rows[row - 1]["Count"] = row.ToString();
                type = dt.Rows[row - 1]["type"].ObjToString();
                if (type == "service")
                    break;
                //if (String.IsNullOrWhiteSpace(type))
                //    break;
                row--;
            }

            G1.sortTable(dt, "Count", "asc");
            dt.Columns.Remove("Count");
            G1.NumberDataTable(dt);
        }
        /***************************************************************************************/
        private void MoveRowDown(DataTable dt, int row)
        {
            if (G1.get_column_number(dt, "count") < 0)
                dt.Columns.Add("Count", Type.GetType("System.Int32"));
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["Count"] = i.ToString();
            string type = "";
            int mainRow = row;
            for (; ; )
            {
                dt.Rows[mainRow]["Count"] = (row + 1).ToString();
                dt.Rows[row + 1]["Count"] = row.ToString();
                type = dt.Rows[row + 1]["type"].ObjToString();
                if (type == "service")
                    break;
                //if (String.IsNullOrWhiteSpace(type))
                //    break;
                row++;
            }
            G1.sortTable(dt, "Count", "asc");
            dt.Columns.Remove("Count");
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["Num"] = (i + 1).ToString();
        }
        /***********************************************************************************************/
        private void picRowDown_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable dt = (DataTable)dgv.DataSource;
                if (dt.Rows.Count <= 0)
                    return;
                DataRow dr = gridMain.GetFocusedDataRow();
                int rowHandle = gridMain.FocusedRowHandle;
                int row = gridMain.GetDataSourceRowIndex(rowHandle);
                if (row == (dt.Rows.Count - 1))
                    return; // Already at the last row
                MoveRowDown(dt, row);
                dt.AcceptChanges();
                dgv.DataSource = dt;
                gridMain.ClearSelection();
                gridMain.SelectRow(rowHandle + 1);
                gridMain.FocusedRowHandle = rowHandle + 1;
                gridMain.RefreshData();
                dgv.Refresh();
                btnSave.Show();
                modified = true;
            }
            catch (Exception ex)
            {
            }
        }
        /***********************************************************************************************/
        private void btnInsert_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt.Rows.Count <= 0)
                return;
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            if (row < 0 || row > (dt.Rows.Count - 1))
                return;
            if (rowHandle == (dt.Rows.Count - 1))
                return; // Already at the last row
            DataRow dRow = dt.NewRow();
            dRow["service"] = "New Service";
            dRow["type"] = "service";
            dRow["basicService"] = "0";
            dt.Rows.InsertAt(dRow, row);
            G1.NumberDataTable(dt);
            dt.AcceptChanges();
            dgv.DataSource = dt;
            gridMain.ClearSelection();
            gridMain.RefreshData();
            gridMain.FocusedRowHandle = rowHandle + 1;
            gridMain.SelectRow(rowHandle + 1);
            dgv.Refresh();
        }
        /***********************************************************************************************/
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        /***********************************************************************************************/
        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditHelp helpForm = new EditHelp("customers");
            helpForm.Show();
        }
        /***********************************************************************************************/
        private void SaveServices()
        {
            string record = "";
            string service = "";
            string data = "";
            string type = "";
            string price = "";
            string futurePrice = "";
            string pastPrice = "";
            string mod = "";
            string basicService = "";
            string plonly = "";
            string noSelect = "";

            this.Cursor = Cursors.WaitCursor;

            DataTable dt = (DataTable)dgv.DataSource;

            bool gotData = false;
            if (G1.get_column_number(dt, "data") >= 0)
                gotData = true;
            bool gotPlonly = false;
            if (G1.get_column_number(dt, "plonly") >= 0)
                gotPlonly = true;

            bool gotNoSelect = false;
            if (G1.get_column_number(dt, "noSelect") >= 0)
                gotNoSelect = true;

            this.Cursor = Cursors.WaitCursor;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (gotData)
                {
                    data = dt.Rows[i]["data"].ObjToString();
                    if (data.Trim() == "CUSTOM")
                        continue;
                }
                record = dt.Rows[i]["record"].ObjToString();
                mod = dt.Rows[i]["mod"].ObjToString();
                if (mod.ToUpper() == "D")
                {
                    if (!String.IsNullOrWhiteSpace(record))
                        G1.delete_db_table("funeral_master", "record", record);
                    continue;
                }
                if (String.IsNullOrWhiteSpace(record))
                    record = G1.create_record("funeral_master", "service", "-1");
                if (G1.BadRecord("funeral_master", record))
                    continue;
                service = dt.Rows[i]["service"].ObjToString();
                price = dt.Rows[i]["price"].ObjToString();
                futurePrice = dt.Rows[i]["futurePrice"].ObjToString();
                pastPrice = dt.Rows[i]["pastPrice"].ObjToString();
                basicService = dt.Rows[i]["basicService"].ObjToString();
                //                data = dt.Rows[i]["data"].ObjToString();
                type = dt.Rows[i]["type"].ObjToString();
                plonly = "0";
                noSelect = "0";
                if (gotPlonly)
                    plonly = dt.Rows[i]["plonly"].ObjToString();
                if (gotNoSelect)
                    noSelect = dt.Rows[i]["noSelect"].ObjToString();
                G1.update_db_table("funeral_master", "record", record, new string[] { "service", service, "price", price, "type", type, "futurePrice", futurePrice, "pastPrice", pastPrice, "basicService", basicService, "plonly", plonly, "noSelect", noSelect, "order", i.ToString() });
            }
            btnSave.Hide();
            modified = false;
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (e == null)
                return;
            modified = true;
            if (!Selecting)
                btnSave.Show();

            int rowHandle = gridMain.FocusedRowHandle;
            DataRow dr = gridMain.GetFocusedDataRow();
            if (dr == null)
                return;

            string columnName = gridMain.FocusedColumn.FieldName.Trim();

            dr["mod"] = "1";

            if (columnName.ToUpper() != "PRICE" && columnName.ToUpper() != "FUTUREPRICE" && columnName.ToUpper() != "PASTPRICE" )
                return;

            DataTable dt = (DataTable)dgv.DataSource;
            DataRow[] dRows = dt.Select("service='Package Discount'");
            if (dRows.Length <= 0)
                return;

            dRows = dt.Select("service='Total Listed Price'");
            if (dRows.Length <= 0)
                return;
            double totalListedPrice = dRows[0][columnName].ObjToDouble();

            dRows = dt.Select("service='Package Price'");
            if (dRows.Length <= 0)
                return;
            double packagePrice = dRows[0][columnName].ObjToDouble();

            double packageDiscount = packagePrice - totalListedPrice;

            dRows = dt.Select("service='Package Discount'");
            if (dRows.Length <= 0)
                return;

            dRows[0][columnName] = packageDiscount;
            dRows[0]["mod"] = "1";
            dgv.DataSource = dt;
            dgv.Refresh();
        }
        /***********************************************************************************************/
        private void gridMain_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv.DataSource;
            if (G1.get_column_number(dt, "mod") < 0)
                return;
            if (Selecting)
            {
                string noSelect = dt.Rows[row]["noSelect"].ObjToString();
                if (noSelect == "1")
                {
                    e.Visible = false;
                    e.Handled = true;
                    return;
                }
            }
            string service = dt.Rows[row]["service"].ObjToString();
            string delete = dt.Rows[row]["mod"].ObjToString();
            if (delete.ToUpper() == "D")
            {
                e.Visible = false;
                e.Handled = true;
                return;
            }
            if ( Selecting )
            {
                if ( G1.get_column_number ( dt, "plonly") >= 0 )
                {
                    string plonly = dt.Rows[row]["plonly"].ObjToString();
                    string select = dt.Rows[row]["select"].ObjToString();
                    if ( plonly == "1" && select == "0" )
                    {
                        if (!gotPackage)
                        {
                            if (service.ToUpper() == "PACKAGE DISCOUNT")
                            {
                            }
                            e.Visible = false;
                            e.Handled = true;
                        }
                    }
                }
            }
//            string service = dt.Rows[row]["service"].ObjToString();
            if ( String.IsNullOrWhiteSpace ( service))
            {
                e.Visible = false;
                e.Handled = true;
                return;
            }
            if (Selecting)
            {
                double price = dt.Rows[row]["price"].ObjToDouble();
                if (price <= 0)
                {
                    //e.Visible = false;
                    //e.Handled = true;
                }
            }
            //if (!String.IsNullOrWhiteSpace(selectWhat))
            //{
            //    string type = dt.Rows[row]["type"].ObjToString();
            //    if (type.ToUpper() != selectWhat.ToUpper())
            //    {
            //        e.Visible = false;
            //        e.Handled = true;
            //    }
            //}
        }
        /***********************************************************************************************/
        private void gridMain_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
        {
            if (e.Column.FieldName.ToUpper() == "NUM")
            {
                if (e.RowHandle >= 0)
                {
                    string num = (e.RowHandle + 1).ToString();
                    e.DisplayText = num;
                }
            }
            else if (e.Column.FieldName.ToUpper() == "DATA")
            {
                if (e.RowHandle >= 0)
                {
                    string data = e.DisplayText.ObjToString();
                    if (G1.validate_numeric(data))
                    {
                        double dvalue = data.ObjToDouble();
                        e.DisplayText = G1.ReformatMoney(dvalue);
                    }
                    else if (data.ToUpper() == "CUSTOM")
                    {
                        e.Appearance.BackColor = Color.Yellow;
                        e.Appearance.ForeColor = Color.Black;
                    }
                }
            }
        }
        /***********************************************************************************************/
        private void gridMain_RowCellStyle(object sender, DevExpress.XtraGrid.Views.Grid.RowCellStyleEventArgs e)
        {
            GridView View = sender as GridView;
            if (e.Column.FieldName.ToUpper() == "DATA")
            {
                string str = View.GetRowCellValue(e.RowHandle, "data").ObjToString();
                if (str != null)
                {
                    if (G1.validate_numeric(str))
                        e.Appearance.TextOptions.HAlignment = HorzAlignment.Far;
                }
            }
        }
        /***********************************************************************************************/
        private void LoadGroupCombo()
        {
            string cmd = "Select * from `funeral_groups` ORDER BY `order`,`record`;";
            DataTable dt = G1.get_db_data(cmd);
            string groupname = "";
            string name = "";
            string locationCode = "";
            string str = "";
            cmbGroups.Items.Clear();
            cmbGroups.Items.Add("Master");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                groupname = dt.Rows[i]["shortname"].ObjToString();
                cmbGroups.Items.Add(groupname);
            }
            cmbGroups.Text = "Master";
        }
        /***********************************************************************************************/
        private void LoadPackagesCombo()
        {
            gotPackage = false;
            string group = GetGroup();
            if (String.IsNullOrWhiteSpace(group))
                return;
            //string cmd = "Select * from `packages` where `groupname` = '" + group + "' GROUP BY `PackageName`;";
            string cmd = "Select * from `packages` where `groupname` = 'master' GROUP BY `PackageName`;";
            DataTable dt = G1.get_db_data(cmd);
            string firstPackage = "";
            string package = "";
            cmbPackage.Items.Clear();
            cmbPackage.Items.Add("Master");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                package = dt.Rows[i]["PackageName"].ObjToString();
                if (String.IsNullOrWhiteSpace(package))
                    continue;
                if (String.IsNullOrWhiteSpace(firstPackage))
                    firstPackage = package;
                cmbPackage.Items.Add(package);
            }
            cmbPackage.Text = "Master";
        }
        /***********************************************************************************************/
        private string GetGroup()
        {
            string location = cmbGroups.Text;
            if (location.ToUpper() == "MASTER")
                return location;
            return location;
        }
        /***********************************************************************************************/
        private void cmbLocation_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            if (!CheckForSaving())
                return;
            LoadSelection();
            picMoveFuture.Show();
        }
        /***********************************************************************************************/
        private void LoadSelection()
        {
            bool localModified = false;
            btnSave.Hide();
            string group = GetGroup();
            if (workingLocation && !String.IsNullOrWhiteSpace(workGPL))
                group = workGPL;
//            string cmd = "Select * from `packages` where `groupname` = '" + group + "' GROUP BY `PackageName`;";
            string cmd = "Select * from `packages` where `groupname` = 'master' GROUP BY `PackageName`;";
            DataTable dt = G1.get_db_data(cmd);
            cmbPackage.Items.Clear();
            string packageName = "";
            string firstPackage = "";
            gridMain.Columns["basicService"].Visible = false;
            gridMain.Columns["plonly"].Visible = false;
            if (group.ToUpper() == "MASTER")
            {
                gridMain.OptionsView.ShowFooter = false;
                cmbPackage.Items.Add("Master");
                firstPackage = "Master";
            }
            cmbPackage.Items.Add("Master");
            firstPackage = "Master";
            loading = true;
            cmbPackage.Text = "Master";
            loading = false;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                packageName = dt.Rows[i]["PackageName"].ObjToString();
                if (String.IsNullOrWhiteSpace(firstPackage))
                    firstPackage = packageName;
                cmbPackage.Items.Add(packageName);
            }
            if (!String.IsNullOrWhiteSpace(savePackage))
                cmbPackage.Text = savePackage;
            if (!String.IsNullOrWhiteSpace(firstPackage) && firstPackage != "Master")
            {
                cmbPackage.Text = firstPackage;
                LoadData();
                //                LoadPackage(cmbPackage.Text);
                //cmd = "Select * from `packages` where `PackageName` = '" + firstPackage + "';";
                //dt = G1.get_db_data(cmd);
                //dgv.DataSource = dt;
            }
            else
            {
                string package = cmbPackage.Text.Trim().ToUpper();
                if (package == "MASTER")
                {
                    if (group.ToUpper() == "MASTER")
                    {
                        cmd = "Select * from `funeral_master` order by `order`, `record`;";
                        dt = G1.get_db_data(cmd);
                        FixAllData(dt);
                        pictureAdd.Show();
                        pictureDelete.Show();
                        btnInsert.Show();
                        picRowDown.Show();
                        picRowUp.Show();
                        picAddMerch.Show();
                        btnInsertMerch.Show();
                    }
                    else
                    {
                        cmd = "Select * from `funeral_gplgroups` g LEFT JOIN `funeral_master` s on g.`service` = s.`service` where g.`groupname` = '" + group + "' order by g.`record`;";
                        dt = G1.get_db_data(cmd);

                        dt = MatchToMaster(dt);

                        pictureAdd.Hide();
                        pictureDelete.Hide();
                        btnInsert.Hide();
                        picRowDown.Hide();
                        picRowUp.Hide();
                        picAddMerch.Hide();
                        btnInsertMerch.Hide();
                    }
                }
                else
                {
                    cmd = "Select * from `packages` where `PackageName` = 'xyzzyxxx';";
                    dt = G1.get_db_data(cmd);
                }
                if (G1.get_column_number(dt, "futurePrice2") >= 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                        dt.Rows[i]["futurePrice"] = dt.Rows[i]["futurePrice2"].ObjToDouble();
                }
                if (G1.get_column_number(dt, "pastPrice2") >= 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                        dt.Rows[i]["pastPrice"] = dt.Rows[i]["pastPrice2"].ObjToDouble();
                }
                dt.Columns.Add("num");
                //dt.Columns.Add("mod");
                ////            dt.Columns.Add("agreement");
                //dt.Columns.Add("select");
                if (G1.get_column_number(dt, "SameAsMaster") < 0)
                    dt.Columns.Add("SameAsMaster");
                if (G1.get_column_number(dt, "mod") < 0)
                    dt.Columns.Add("mod");
                if (G1.get_column_number(dt, "data") < 0)
                    dt.Columns.Add("data");
                string type = "";
                string type1 = "";
                if (G1.get_column_number(dt, "type1") >= 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        type = dt.Rows[i]["type"].ObjToString();
                        type1 = dt.Rows[i]["type1"].ObjToString();
                        if (!String.IsNullOrWhiteSpace(type1))
                            dt.Rows[i]["type"] = type1;
                    }
                }


                //dt.Columns.Add("total", Type.GetType("System.Double"));
                if (Selecting)
                {
                    SetupSelection(dt);
                    SetupTax(dt);
                }
                if (group.Trim().ToUpper() != "MASTER")
                {
                    cmd = "Select * from `funeral_master` order by `order`, `record`;";
                    DataTable dx = G1.get_db_data(cmd);
                    FixAllData(dx);
                    SetupSameAsMaster(dt, dx);
                }
                else
                    SetupSameAsMaster(null);
                if (group.Trim().ToUpper() == "MASTER" && package.Trim().ToUpper() == "MASTER")
                {
                    gridMain.Columns["SameAsMaster"].Visible = false;
                    gridMain.Columns["data"].Visible = false;
                    gridMain.Columns["basicService"].Visible = true;
                    gridMain.Columns["plonly"].Visible = true;
                    SetupBasicServices(dt);
                    SetupPLOnly(dt);
                    SetupNoSelect(dt);
                }
                else
                {
                    gridMain.Columns["SameAsMaster"].Visible = true;
                    gridMain.Columns["basicService"].Visible = false;
                    gridMain.Columns["plonly"].Visible = false;
                    gridMain.Columns["noSelect"].Visible = false;
                }

                G1.NumberDataTable(dt);
                CleanupSelections(dt);
                dgv.DataSource = dt;
            }
            dt = (DataTable)dgv.DataSource;
            LoadGPLLocations();

            if (localModified)
            {
                btnSave.Show();
                modified = true;
            }
        }
        /***********************************************************************************************/
        private DataTable MatchToMaster ( DataTable dt )
        {
            string service = "";
            string type = "";
            DataRow[] dR = null;
            string cmd = "Select * from `funeral_master` order by `order`, `record`;";
            DataTable dx = G1.get_db_data(cmd);
            FixAllData(dx);
            FixAllData(dt);

            dt.Columns.Add("BAD");
            dx.Columns.Add("BAD");

            bool modified = false;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dt.Rows[i]["BAD"] = "BAD";
                type = dt.Rows[i]["type1"].ObjToString();
                //if (type.ToUpper() != "SERVICE")
                //    continue;
                service = dt.Rows[i]["service"].ObjToString();
                if ( !String.IsNullOrWhiteSpace ( service ))
                {
                    service = G1.force_lower_line(service);
                    dR = dx.Select("service='" + service + "'");
                    if (dR.Length > 0)
                    {
                        dt.Rows[i]["BAD"] = "";
                        dt.Rows[i]["order"] = dR[0]["order"].ObjToInt32();
                        dR[0]["BAD"] = "GOOD";
                    }
                }
            }
            for ( int i=dt.Rows.Count-1; i>=0; i--)
            {
                service = dt.Rows[i]["BAD"].ObjToString();
                if (service.ToUpper() == "BAD")
                {
                    dt.Rows.RemoveAt(i);
                    modified = true;
                }
            }
            for ( int i=0; i<dx.Rows.Count; i++)
            {
                type = dx.Rows[i]["type"].ObjToString();
                //if (type.ToUpper() != "SERVICE")
                //    continue;
                service = dx.Rows[i]["BAD"].ObjToString();
                if (service.ToUpper() == "GOOD")
                    continue;
                service = dx.Rows[i]["service"].ObjToString();
                if (!String.IsNullOrWhiteSpace(service))
                {
                    dt.ImportRow(dx.Rows[i]);
                    modified = true;
                }
            }

            DoTheMath(dt); // Add Services Together

            DataView tempview = dt.DefaultView;
            tempview.Sort = "order asc";
            dt = tempview.ToTable();

            if (modified)
            {
                btnSave.Show();
                modified = true;
            }

            return dt;
        }
        /***********************************************************************************************/
        public static void DoTheMath(DataTable dt)
        {
            AddServiceTogether(dt, "Immediate Burial With Minimum Casket", "Lowest Casket Price", "Immediate Burial With Container Provided By The Purchaser");
            AddServiceTogether(dt, "Highest Priced Immediate Burial", "Highest Casket Price", "Immediate Burial With Container Provided By The Purchaser");
            AddServiceTogether(dt, "Direct Cremation With Alternative Container", "Direct Cremation With Alt. Container Provided By The Purchaser", "Alternative Cremation Container (Made Of Heavy Cardboard)");
            AddServiceTogether(dt, "Highest Priced Direct Cremation", "Direct Cremation With Alt. Container Provided By The Purchaser", "Alternative Cremation Container (Made Of Heavy Cardboard)");
        }
        /***********************************************************************************************/
        public static void AddServiceTogether(DataTable dt, string mainService, string service1, string service2)
        {
            DataRow[] dR = dt.Select("service='" + mainService + "'");
            if (dR.Length > 0)
            {
                double price1 = 0D;
                double price2 = 0D;
                double price3 = 0D;
                DataRow[] dR1 = dt.Select("service='" + service1 + "'");
                DataRow[] dR2 = dt.Select("service='" + service2 + "'");
                if (dR1.Length > 0 && dR2.Length > 0)
                {
                    price2 = dR1[0]["price"].ObjToDouble();
                    price3 = dR2[0]["price"].ObjToDouble();
                    price1 = price2 + price3;
                    price1 = G1.RoundValue(price1);
                    dR[0]["price"] = price1;

                    price2 = dR1[0]["futureprice"].ObjToDouble();
                    price3 = dR2[0]["futureprice"].ObjToDouble();
                    price1 = price2 + price3;
                    price1 = G1.RoundValue(price1);
                    dR[0]["futureprice"] = price1;
                }
            }
        }
        /***********************************************************************************************/
        private void LoadGPLLocations()
        {
            string gplGroup = cmbGroups.Text.Trim();
            cmbLocations.Items.Clear();
            cmbLocations.Text = "";
            if (gplGroup.ToUpper() == "MASTER")
                return;

            lblLocations.Text = "Locations (0)";

            string cmd = "Select * from `gpl_locations` where `gpl` = '" + gplGroup + "' GROUP BY `location`;";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
            {
                btnEdit.Hide();
                btnDeleteLocation.Hide();
                return;
            }
            bool saveLoading = loading;
            loading = true;
            string location = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                location = dt.Rows[i]["location"].ObjToString();
                cmbLocations.Items.Add(location);
            }
            lblLocations.Text = "Locations (" + dt.Rows.Count.ToString() + ")";

            btnEdit.Hide();
            btnDeleteLocation.Hide();

            loading = saveLoading;
        }
        /***********************************************************************************************/
        private void PullLocationGPL(DataTable dx)
        {
            string gplGroup = cmbGroups.Text.Trim();
            if (String.IsNullOrWhiteSpace(gplGroup))
                return;
            if (gplGroup.Trim().ToUpper() == "MASTER")
                return;

            string location = cmbLocations.Text.Trim();
            if (String.IsNullOrWhiteSpace(location))
                return;

            string cmd = "Select * from `gpl_locations` where `gpl` = '" + gplGroup + "' and `location` = '" + location + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return;
            if (dx != null)
            {
                DataRow[] dRows = null;
                string service = "";
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    service = dt.Rows[i]["service"].ObjToString();
                    dRows = dx.Select("service='" + service + "'");
                    if (dRows.Length > 0)
                    {
                        dRows[0]["price"] = dt.Rows[i]["price"].ObjToString();
                        dRows[0]["futurePrice"] = dt.Rows[i]["futurePrice"].ObjToString();
                        dRows[0]["pastPrice"] = dt.Rows[i]["pastPrice"].ObjToString();
                        dRows[0]["data"] = "CUSTOM";
                    }
                }
            }
        }
        /***********************************************************************************************/
        private void btnAddPackage_Click(object sender, EventArgs e)
        {
            string packName = "";
            using (Ask askForm = new Ask("Enter New Package Name?"))
            {
                askForm.Text = "";
                askForm.ShowDialog();
                if (askForm.DialogResult != System.Windows.Forms.DialogResult.OK)
                    return;
                packName = askForm.Answer;
                if (String.IsNullOrWhiteSpace(packName))
                    return;
            }
            LoadPackage(packName);
        }
        /***********************************************************************************************/
        private void LoadPackage(string packName)
        {
            loadedPackage = packName;
            string group = GetGroup();
            loadededLocation = group;
            string cmd = "Select * from `packages` p RIGHT JOIN `services` s ON p.`!serviceRecord` = s.`record` where p.`groupname` = '" + group + "' and p.`PackageName` = '" + packName + "';";
            if (packName.Trim().ToUpper() == "MASTER")
                cmd = "Select * from `packages` p JOIN `funeral_gplgroups` s ON p.`!serviceRecord` = s.`record` where p.`groupname` = '" + group + "' and p.`PackageName` = '" + packName + "';";
            else
                cmd = "Select * from `packages` p JOIN `funeral_master` s ON p.`!serviceRecord` = s.`record` where p.`groupname` = '" + group + "' and p.`PackageName` = '" + packName + "';";
            //cmd = "Select * from `funeral_gplgroups` p LEFT JOIN `services` s ON p.`!masterRecord` = s.`record` where p.`groupname` = '" + group + "';";
            //cmd = "Select * from `funeral_gplgroups` where `groupname` = '" + group + "'";
            DataTable dt = G1.get_db_data(cmd);
            string record = "";
            if (dt.Rows.Count > 0)
                record = dt.Rows[0]["record"].ObjToString();
            //            Services serviceForm = new Services("Packages", true, dt, "");
            Services serviceForm = new Services(group, "", true, dt, "");
            serviceForm.SelectDone += ServiceForm_SelectDone;
            serviceForm.Show();
        }
        /***********************************************************************************************/
        private void ServiceForm_SelectDone(DataTable dt, string what )
        {
            SaveServices(dt);
            if (G1.get_column_number(dt, "num") < 0)
                dt.Columns.Add("num");
            G1.NumberDataTable(dt);
            if (!alreadyLoaded)
                dgv.DataSource = dt;
        }
        /***********************************************************************************************/
        private void SaveServices(DataTable dt)
        {
            string service = "";
            string serviceRecord = "";
            string SameAsMaster = "";
            string data = "";
            double price = 0D;
            double futurePrice = 0D;
            double pastPrice = 0D;
            double cost = 0D;
            string type = "";
            string record = "";
            int recordCol = G1.get_column_number(dt, "record");
            if (G1.get_column_number(dt, "!serviceRecord") >= 0)
                recordCol = G1.get_column_number(dt, "!serviceRecord");
            if (String.IsNullOrWhiteSpace(loadededLocation))
            {
                MessageBox.Show("***ERROR*** Empty Location");
                return;
            }
            if (String.IsNullOrWhiteSpace(loadedPackage))
            {
                MessageBox.Show("***ERROR*** Empty Package");
                return;
            }
            this.Cursor = Cursors.WaitCursor;
            string cmd = "Delete from `packages` where `groupname` = '" + loadededLocation + "' and `PackageName` = '" + loadedPackage + "';";
            G1.get_db_data(cmd);

            if ( loadededLocation.ToUpper() != "MASTER")
            {
                if (loadedPackage.ToUpper() == "MASTER")
                {
                    cmd = "Delete from `funeral_gplgroups` where `groupname` = '" + loadededLocation + "';";
                    G1.get_db_data(cmd);
                }
            }

            DataTable gDt = null;
            DataTable dx = null;
            string masterRecord = "";
            string mod = "";
            bool gotData = false;
            if (G1.get_column_number(dt, "data") >= 0)
                gotData = true;
            int modColumn = G1.get_column_number(dt, "mod");
            string sRecord = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    if (gotData)
                    {
                        data = dt.Rows[i]["data"].ObjToString();
                        if (data.Trim() == "CUSTOM")
                            continue;
                    }
                    serviceRecord = dt.Rows[i][recordCol].ObjToString();
                    if (modColumn >= 0)
                    {
                        mod = dt.Rows[i]["mod"].ObjToString();
                        if (mod == "D")
                            continue;
                    }
                    service = dt.Rows[i]["service"].ObjToString();
                    if ( service.ToUpper() == "TOTAL LISTED PRICE")
                    {
                    }
                    //data = dt.Rows[i]["data"].ObjToString();
                    type = dt.Rows[i]["type"].ObjToString();
                    if (type.Trim().ToUpper() == "MERCHANDISE")
                    {

                    }
                    SameAsMaster = dt.Rows[i]["SameAsMaster"].ObjToString();
                    price = dt.Rows[i]["price"].ObjToDouble();
                    futurePrice = dt.Rows[i]["futurePrice"].ObjToDouble();
                    pastPrice = dt.Rows[i]["pastPrice"].ObjToDouble();
                    cost = 0D;
                    if (G1.get_column_number(dt, "data") >= 0)
                        cost = dt.Rows[i]["data"].ObjToDouble();
                    record = G1.create_record("packages", "groupname", "-1");
                    if (G1.BadRecord("packages", record))
                        continue;
                    G1.update_db_table("packages", "record", record, new string[] { "groupname", loadededLocation, "PackageName", loadedPackage, "!serviceRecord", serviceRecord, "SameAsMaster", SameAsMaster, "price", price.ToString(), "cost", cost.ToString(), "futurePrice", futurePrice.ToString(), "pastPrice", pastPrice.ToString() });

                    if (loadedPackage.Trim().ToUpper() == "MASTER")
                    {
                        cmd = "Select * from `funeral_gplgroups` where `groupname` = '" + loadededLocation + "' and `record` = '" + serviceRecord + "';";
                        gDt = G1.get_db_data(cmd);
                        if (gDt.Rows.Count <= 0)
                        {
                            cmd = "Select * from `funeral_gplgroups` where `groupname` = '" + loadededLocation + "' and `service` = '" + service + "';";
                            gDt = G1.get_db_data(cmd);
                            if (gDt.Rows.Count > 0)
                                record = gDt.Rows[0]["record"].ObjToString();
                            else
                                record = G1.create_record("funeral_gplgroups", "type", "-1");
                            if (G1.BadRecord("funeral_gplgroups", record))
                                continue;
                        }
                        else
                            record = gDt.Rows[0]["record"].ObjToString();
                        masterRecord = "";
                        cmd = "Select * from `funeral_master` where `service` = '" + service + "';";
                        dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count > 0)
                            masterRecord = dx.Rows[0]["record"].ObjToString();

                        G1.update_db_table("funeral_gplgroups", "record", record, new string[] { "service", service, "price", price.ToString(), "groupname", loadededLocation, "!masterRecord", masterRecord, "SameAsMaster", SameAsMaster, "type", "service", "futurePrice", futurePrice.ToString(), "pastPrice", pastPrice.ToString() });
                    }
                }
                catch (Exception ex)
                {
                }

                //G1.update_db_table("funeral_gplgroups", "record", serviceRecord, new string[] {"futurePrice", futurePrice.ToString() });
            }
            if (G1.get_column_number(dt, "num") < 0)
                dt.Columns.Add("num");
            G1.NumberDataTable(dt);
            modified = false;
            if (loadedPackage.Trim().ToUpper() == "MASTER")
            {
                cmbLocation_SelectedIndexChanged(null, null);
                alreadyLoaded = true;
            }
            else
            {
                if (!String.IsNullOrWhiteSpace(savePackage))
                    cmbPackage.Text = savePackage;
            }
            savePackage = "";
            loading = true;
            cmbPackage.Text = loadedPackage;
            loading = false;
            modified = false;
            btnSave.Hide();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void cmbPackage_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            if (!CheckForSaving())
                return;
            if (Selecting)
            {
                workPackage = cmbPackage.Text;
                btnAllOff.Show();
                btnAllOn.Hide();
                if (workPackage.ToUpper() != "MASTER")
                {
                    btnAllOff.Show();
                    btnAllOn.Show();
                }
            }
            LoadData();
        }
        /***********************************************************************************************/
        private void repositoryItemCheckEdit1_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();
            string service = dr["service"].ObjToString();
            string record = dr["record"].ObjToString();
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            string select = dr["SameAsMaster"].ObjToString();
            select = dt.Rows[row]["SameAsMaster"].ObjToString();
            if (String.IsNullOrWhiteSpace(workGroup))
            {
                workGroup = cmbGroups.Text.Trim();
            }

            string doit = "0";
            if (select == "0")
                doit = "1";
            dr["SameAsMaster"] = doit;
            dt.Rows[row]["SameAsMaster"] = doit;
            dgv.DataSource = dt;
            dgv.RefreshDataSource();
            //if (1 == 1)
            //    return;

            if (doit == "1" && !String.IsNullOrWhiteSpace(workGroup))
            {
                try
                {
                    string cmd = "Select * from `services` where `record` = '" + record + "';";
                    if (!String.IsNullOrWhiteSpace(workGroup))
                    {
                        if (workGroup.ToUpper() != "MASTER")
                            cmd = "Select * from `funeral_master` WHERE `service` = '" + service + "';";
                        else
                            cmd = "Select * from `funeral_gplgroups` where `groupname` = '" + workGroup + "' and `service` = '" + service + "';";
                    }
                    DataTable dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                    {
                        dr["price"] = dx.Rows[0]["price"].ObjToDouble();
                        dr["futurePrice"] = dx.Rows[0]["futurePrice"].ObjToDouble();
                        dr["pastPrice"] = dx.Rows[0]["pastPrice"].ObjToDouble();
                        if (G1.get_column_number(dt, "data") >= 0)
                        {
                            if (G1.get_column_number(dt, "data") >= 0)
                            {
                                if (G1.get_column_number(dx, "data") >= 0)
                                    dr["data"] = dx.Rows[0]["data"].ObjToDouble();
                                else
                                    dr["data"] = dx.Rows[0]["price"].ObjToDouble();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                }
            }
            else
            {
                try
                {
                    if (workDt != null)
                    {
                        DataRow[] dRow = workDt.Select("service='" + service + "'");
                        if (dRow.Length > 0)
                        {
                            dr["price"] = dRow[0]["price"].ObjToDouble();
                            if (G1.get_column_number(dt, "data") >= 0)
                                dr["data"] = dRow[0]["cost"].ObjToDouble();
                        }
                        else
                        {
                            dr["price"] = 0D;
                            if (G1.get_column_number(dt, "data") >= 0)
                                dr["data"] = 0D;
                        }
                    }
                    else
                    {
                        //dr["price"] = 0D;
                        //if (G1.get_column_number(dt, "data") >= 0)
                        //    dr["data"] = 0D;
                    }
                }
                catch (Exception)
                {
                }
            }
            modified = true;
            ReCalcTotal();
            gridMain.RefreshData();
            //            gridMain.RefreshRow(rowHandle);
            dgv.Refresh();
            btnSave.Show();
            btnSave.Visible = true;
            btnSave.Refresh();
        }
        /***********************************************************************************************/
        private void importGroupInfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Import importForm = new Import();
            importForm.SelectDone += ImportForm_SelectDone;
            importForm.Show();
        }
        /***********************************************************************************************/
        private void ImportFuneralMaster(DataTable dt)
        {
            string service = "";
            string price = "";
            string record = "";
            string num = "";

            string cmd = "DELETE FROM `funeral_master` where `record` > '0';";
            G1.get_db_data(cmd);

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                num = dt.Rows[i][0].ObjToString();
                service = dt.Rows[i][1].ObjToString();
                if (String.IsNullOrWhiteSpace(service))
                    continue;
                price = dt.Rows[i][2].ObjToString();
                //if (String.IsNullOrWhiteSpace(price))
                //    continue;
                if (String.IsNullOrWhiteSpace(price))
                    price = "$0.00";
                price = price.Replace("$", "");
                price = price.Replace(",", "");
                if (!G1.validate_numeric(price))
                    continue;
                service = G1.protect_data(service);

                if (service.ToUpper().IndexOf("ITEMS THAT HAVE PRICES") >= 0)
                    continue;
                if (service.ToUpper().IndexOf("EFFECTIVE DATE") >= 0)
                    continue;
                if (service.ToUpper().IndexOf("THESE PRICES ARE") >= 0)
                    continue;

                record = G1.create_record("funeral_master", "service", "-1");
                if (G1.BadRecord("funeral_master", record))
                    continue;
                G1.update_db_table("funeral_master", "record", record, new string[] { "service", service, "price", price, "type", "service" });
            }
        }
        /***********************************************************************************************/
        private void ImportForm_SelectDone(DataTable dt)
        {
            if (dt == null)
                return;
            if (dt.Rows.Count <= 0)
                return;
            string group = GetGroup();
            if (group.ToUpper().Trim() == "MASTER")
            {
                ImportFuneralMaster(dt);
                return;
            }

            string cmd = "DELETE FROM `funeral_gplgroups` where `groupname` = '" + group + "';";
            G1.get_db_data(cmd);

            DataTable dx = null;
            string service = "";
            string price = "";
            string record = "";
            string autoUpdated = "";
            string SameAsMaster = "";
            string num = "";
            string masterRecord = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                num = dt.Rows[i][0].ObjToString();
                service = dt.Rows[i][1].ObjToString();
                if (String.IsNullOrWhiteSpace(service))
                    continue;
                if (service.Trim().ToUpper() == "TOTAL")
                    continue;
                price = dt.Rows[i][2].ObjToString();
                if (String.IsNullOrWhiteSpace(price))
                    continue;
                price = price.Replace("$", "");
                price = price.Replace(",", "");
                if (!G1.validate_numeric(price))
                    continue;
                service = G1.protect_data(service);

                autoUpdated = dt.Rows[i][3].ObjToString();
                SameAsMaster = "0";
                if (autoUpdated.Trim().ToUpper() == "AUTO UPDATED")
                    SameAsMaster = "1";

                record = G1.create_record("funeral_gplgroups", "service", "-1");
                if (G1.BadRecord("funeral_master", record))
                    continue;

                masterRecord = "";
                cmd = "Select * from `funeral_master` where `service` = '" + service + "';";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                    masterRecord = dx.Rows[0]["record"].ObjToString();

                G1.update_db_table("funeral_gplgroups", "record", record, new string[] { "service", service, "price", price, "groupname", group, "!masterRecord", masterRecord, "SameAsMaster", SameAsMaster, "type", "service" });
            }
        }
        /***********************************************************************************************/
        private void btnSave_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            loadededLocation = GetGroup();
            loadedPackage = cmbPackage.Text;

            DataTable dt = (DataTable)dgv.DataSource;
            if (!Selecting && !workingLocation)
            {
                loadededLocation = cmbGroups.Text.ToUpper();
                loadedPackage = cmbPackage.Text.ToUpper();
                if (loadededLocation == "MASTER" && loadedPackage == "MASTER")
                    SaveServices();
                else
                    SaveServices(dt);
                return;
            }

            DataTable mainDt = (DataTable)dgv.DataSource;

            DataTable dx = dt.Clone();
            string select = "";
            string pSelect = "";
            string upgrade = "";
            string isPackage = "";
            string service = "";
            string service1 = "";
            string ignore = "";
            string tax = "";
            double taxAmount = 0D;
            string location = "";
            string locRecord = "";
            double pastPrice = 0D;
            double price = 0D;
            string who = "";
            string deleted = "";
            string mod = "";
            if (G1.get_column_number(dt, "isPackage") < 0)
                dt.Columns.Add("isPackage");
            if (G1.get_column_number(dt, "pSelect") < 0)
                dt.Columns.Add("pSelect");
            if (G1.get_column_number(dt, "upgrade") < 0)
                dt.Columns.Add("upgrade");
            if (G1.get_column_number(dt, "ignore") < 0)
                dt.Columns.Add("ignore");
            if (G1.get_column_number(dt, "tax") < 0)
                dt.Columns.Add("tax");
            if (G1.get_column_number(dt, "taxAmount") < 0)
                dt.Columns.Add("taxAmount", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "location") < 0)
                dt.Columns.Add("location");
            if (G1.get_column_number(dt, "locRecord") < 0)
                dt.Columns.Add("locRecord");
            if (G1.get_column_number(dt, "pastPrice") < 0)
                dt.Columns.Add("pastPrice", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "who") < 0)
                dt.Columns.Add("who");
            if (G1.get_column_number(dt, "DELETED") < 0)
                dt.Columns.Add("DELETED");

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                service = dt.Rows[i]["service"].ObjToString();
                select = dt.Rows[i]["select"].ObjToString();
                ignore = dt.Rows[i]["ignore"].ObjToString();
                tax = dt.Rows[i]["tax"].ObjToString();
                taxAmount = dt.Rows[i]["taxAmount"].ObjToDouble();
                location = dt.Rows[i]["location"].ObjToString();
                locRecord = dt.Rows[i]["locRecord"].ObjToString();
                pastPrice = dt.Rows[i]["pastPrice"].ObjToDouble();
                price = dt.Rows[i]["price"].ObjToDouble();
                who = dt.Rows[i]["who"].ObjToString();
                deleted = dt.Rows[i]["DELETED"].ObjToString();
                pSelect = dt.Rows[i]["pSelect"].ObjToString();
                upgrade = dt.Rows[i]["upgrade"].ObjToString();
                isPackage = dt.Rows[i]["isPackage"].ObjToString();
                if (service.ToUpper() == "TRANSFER OF REMAINS TO THE FUNERAL HOME")
                {

                }
                if (select == "1")
                {
                    if (price == 0D && pastPrice > 0D)
                    {
                        //DataRow[] dRows = mainDt.Select("service='" + service + "'");
                        //if (dRows.Length > 0)
                        //{
                        //    service1 = dRows[0]["service1"].ObjToString();
                        //    if ( !String.IsNullOrWhiteSpace ( service1 ))
                        //        dRows[0]["price"] = pastPrice;
                        //}
                        //mod = dt.Rows[i]["mod"].ObjToString();
                        //if (G1.get_column_number(dt, "ModMod") < 0)
                        //    dt.Columns.Add("ModMod");
                        //if (mod == "1")
                        //    dt.Rows[i]["ModMod"] = "Y";
                    }
                    //dt.Rows[i]["mod"] = "1";
                    G1.copy_dt_row(dt, i, dx, dx.Rows.Count);
                }
                else
                {
                    if ( isPackage.ToUpper() == "P" )
                        G1.copy_dt_row(dt, i, dx, dx.Rows.Count);
                }
            }
            if (chkDiscretionary.Checked)
            {
                DataRow ddRow = dx.NewRow();
                ddRow["price"] = 1D;
                ddRow["mod"] = "1";
                ddRow["modmod"] = "Y";
                ddRow["select"] = "1";
                //if (dx.Rows.Count > 1)
                //{
                //    if (G1.get_column_number(dx, "PackageName") >= 0)
                //    {
                //        ddRow["PackageName"] = loadedPackage;
                //        ddRow["groupname"] = "MASTER";
                //        ddRow["price1"] = 1D;
                //    }
                //}
                if (selectWhat.ToUpper() == "SERVICE")
                {
                    ddRow["type"] = "Service";
                    ddRow["service"] = "D-";
                }
                else if (selectWhat.ToUpper() == "MERCHANDISE")
                {
                    ddRow["type"] = "Merchandise";
                    ddRow["service"] = "D-";
                }
                dx.Rows.Add(ddRow);
            }

            bool gotMerchandise = false;
            DataRow [] dR = dx.Select ( "type='Merchandise'");
            if (dR.Length > 0)
                gotMerchandise = true;
            dR = dx.Select("type='misc'");
            if (dR.Length > 0)
                gotMerchandise = true;
            if (selectWhat.ToUpper() == "MERCHANDISE")
            {
                CopyMerchandise((DataTable)dgv2.DataSource, dx);
                CopyMerchandise((DataTable)dgv3.DataSource, dx);
                CopyMerchandise((DataTable)dgv4.DataSource, dx);
                CopyMerchandise((DataTable)dgv5.DataSource, dx);
            }

            OnSelectDone(dx);

            btnSave.Hide();
            modified = false;

            if (btnSave.Text.ToUpper() == "COPY TO CUSTOMER")
                this.Close();


            //if (loadededLocation.Trim().ToUpper() == "MASTER" && loadedPackage.Trim().ToUpper() == "MASTER")
            //    SaveServices();
            //else
            //{
            //    DataTable dt = (DataTable)dgv.DataSource;
            //    SaveServices(dt);
            //}
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void CopyMerchandise(DataTable fromDt, DataTable toDt)
        {
            if (fromDt == null)
                return;
            string service = "";
            string select = "";
            DataRow[] dRows = null;
            string modmod = "";
            for (int i = 0; i < fromDt.Rows.Count; i++)
            {
                if (fromDt.Rows[i]["select"].ObjToString() == "1")
                {
                    service = fromDt.Rows[i]["service"].ObjToString();
                    dRows = toDt.Select("service='" + service + "'");
                    if (dRows.Length > 0)
                    {
                        select = dRows[0]["select"].ObjToString();
                        if (select != "1")
                            G1.copy_dt_row(fromDt, i, toDt, toDt.Rows.Count);
                        else
                        {
                            modmod = fromDt.Rows[i]["modmod"].ObjToString();
                            if (modmod.ToUpper() == "Y")
                            {
                                dRows[0]["modmod"] = "Y";
                                dRows[0]["price"] = fromDt.Rows[i]["price"].ObjToDouble();
                            }
                        }
                    }
                    else
                        G1.copy_dt_row(fromDt, i, toDt, toDt.Rows.Count);
                }
            }
        }
        /***********************************************************************************************/
        private void updateAllCustomersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string type = dr["type"].ObjToString();
            string service = dr["service"].ObjToString();
            string record = "";

            if (String.IsNullOrWhiteSpace(type) || String.IsNullOrWhiteSpace(service))
            {
                MessageBox.Show("***ERROR*** Type and Service must not be blank!");
                return;
            }
            string cmd = "Select * from `cust_services` where `service` = '" + service + "';";
            DataTable dt = G1.get_db_data(cmd);
            for (int i = 0; i < dt.Rows.Count; i++)
            {

            }
        }
        /***********************************************************************************************/
        private void btnRunMarkup_Click(object sender, EventArgs e)
        {
            string str = txtMarkup.Text;
            if (!G1.validate_numeric(str))
                return;
            double markup = str.ObjToDouble();
            double price = 0D;
            double futurePrice = 0D;
            double pastPrice = 0D; // Don't do anything with pastPrice
            DataTable dt = (DataTable)dgv.DataSource;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                price = dt.Rows[i]["price"].ObjToDouble();
                futurePrice = price * markup;
                dt.Rows[i]["futurePrice"] = futurePrice;
            }
            dgv.DataSource = dt;
            dgv.Refresh();
            modified = true;
            btnSave.Show();
        }
        /***********************************************************************************************/
        private void txtMarkup_TextChanged(object sender, EventArgs e)
        {
        }
        /***********************************************************************************************/
        private void txtMarkup_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
                return;
            string str = txtMarkup.Text.Trim();
            if (!G1.validate_numeric(str))
            {
                MessageBox.Show("***Warning*** Markup must me numeric!");
                txtMarkup.Text = "1.00";
                return;
            }
            double markup = txtMarkup.Text.ObjToDouble();
            str = G1.ReformatMoney(markup);
            txtMarkup.Text = str;
        }
        /***********************************************************************************************/
        private void txtMarkup_Leave(object sender, EventArgs e)
        {
            string str = txtMarkup.Text.Trim();
            if (!G1.validate_numeric(str))
            {
                MessageBox.Show("***Warning*** Markup must me numeric!");
                txtMarkup.Text = "1.00";
                return;
            }
            double markup = txtMarkup.Text.ObjToDouble();
            str = G1.ReformatMoney(markup);
            txtMarkup.Text = str;
        }
        /***********************************************************************************************/
        private bool byPassQuestion = false;
        private void picMoveFuture_Click(object sender, EventArgs e)
        {
            if (loading)
                return;
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt == null)
                return;

            double futurePrice = 0D;
            double currentPrice = 0D;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                currentPrice = dt.Rows[i]["price"].ObjToDouble();
                dt.Rows[i]["pastPrice"] = currentPrice;
                futurePrice = dt.Rows[i]["futurePrice"].ObjToDouble();
                //if (futurePrice <= 0D)
                //    futurePrice = currentPrice; // Made this change so Packages would work properly 5/16/2023
                dt.Rows[i]["price"] = futurePrice;
            }

            dgv.DataSource = dt;
            dgv.Refresh();
            modified = true;
            btnSave.Show();
            picMoveFuture.Hide();
            picMovePast.Show();

            if (!byPassQuestion)
            {

                DialogResult result = MessageBox.Show("***Question***\nMove GPL Cremation Packages too?", "Move GLP Cremation Package Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                if (result == DialogResult.Yes)
                {
                    result = MessageBox.Show("***Question***\nThis will cause the Master and Cremation Packages to be saved!\nOkay?", "Save All Data Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    if (result == DialogResult.Yes)
                        MoveEverything(dt, true );
                }
            }
        }
        /***********************************************************************************************/
        private void MoveEverything ( DataTable dt, bool future )
        {
            this.Cursor = Cursors.WaitCursor;

            pleaseForm = new PleaseWait("Please Wait for Mass Cremation Packages to be Saved!");
            pleaseForm.Show();
            pleaseForm.Refresh();

            btnSave_Click(null, null);

            //cmbPackage
            //SIMPLE DISPOSITION PACKAGE
            //MEMORIAL SERVICES WITH CREMATION PACKAGE
            //TRADITIONAL FUNERAL SERVICE WITH CREMATION

            byPassQuestion = true;

            modified = false;
            cmbPackage.Text = "SIMPLE DISPOSITION PACKAGE";
            workPackage = cmbPackage.Text;
            modified = false;
            LoadData();
            if ( future )
                picMoveFuture_Click(null, null);
            else
                picMovePast_Click(null, null);
            btnSave_Click(null, null);
            modified = false;

            cmbPackage.Text = "MEMORIAL SERVICES WITH CREMATION PACKAGE";
            workPackage = cmbPackage.Text;
            modified = false;
            LoadData();
            if (future)
                picMoveFuture_Click(null, null);
            else
                picMovePast_Click(null, null);
            btnSave_Click(null, null);
            modified = false;

            cmbPackage.Text = "TRADITIONAL FUNERAL SERVICE WITH CREMATION";
            workPackage = cmbPackage.Text;
            modified = false;
            LoadData();
            if (future)
                picMoveFuture_Click(null, null);
            else
                picMovePast_Click(null, null);
            btnSave_Click(null, null);
            modified = false;

            byPassQuestion = false;

            cmbPackage.Text = "MASTER";
            workPackage = cmbPackage.Text;
            modified = false;
            LoadData();
            modified = false;

            picMoveFuture.Show();
            picMoveFuture.Refresh();

            dgv.DataSource = dt;
            dgv.Refresh();

            this.Cursor = Cursors.Default;

            pleaseForm.FireEvent1();
            pleaseForm.Dispose();
            pleaseForm = null;
        }
        /***********************************************************************************************/
        private void picMovePast_Click(object sender, EventArgs e)
        {
            if (loading)
                return;
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt == null)
                return;

            double pastPrice = 0D;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                pastPrice = dt.Rows[i]["pastPrice"].ObjToDouble();
                dt.Rows[i]["price"] = pastPrice;
            }

            dgv.DataSource = dt;
            dgv.Refresh();

            modified = true;
            btnSave.Show();
            picMoveFuture.Show();
            picMovePast.Hide();

            if (!byPassQuestion)
            {

                DialogResult result = MessageBox.Show("***Question***\nRestore GPL Cremation Packages too?", "Rerstore GLP Cremation Package Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                if (result == DialogResult.Yes)
                {
                    result = MessageBox.Show("***Question***\nThis will cause the Master and Cremation Packages to be saved!\nOkay?", "Save All Data Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    if (result == DialogResult.Yes)
                        MoveEverything(dt, false );
                }
            }
        }
        /***********************************************************************************************/
        private void btnAddLocation_Click(object sender, EventArgs e)
        {
            string gplGroup = cmbGroups.Text.Trim();
            if (gplGroup.ToUpper() == "MASTER")
            {
                MessageBox.Show("***ERROR*** You cannot customize the Master GPL Group!\nChoose another GPL Group!");
                return;
            }
            string cmd = "Select * from `funeralhomes` ORDER BY `atneedcode`;";
            DataTable dt = G1.get_db_data(cmd);
            string lines = "";
            string atNeedCode = "";
            string location = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                atNeedCode = dt.Rows[i]["atneedcode"].ObjToString();
                location = dt.Rows[i]["LocationCode"].ObjToString();
                lines += "(" + atNeedCode + ")" + " " + location + "\n";
            }
            //using (ListSelect listForm = new ListSelect(lines, false))
            //{
            //    listForm.Text = "Select Location for GPL " + cmbGroups.Text;
            //    listForm.ListDone += ListForm_LocationDone;
            //    listForm.Show();
            //    listForm.BringToFront();
            //}
            ListSelect listForm = new ListSelect(lines, false);
            listForm.Text = "Select Location for GPL " + cmbGroups.Text;
            listForm.ListDone += ListForm_LocationDone;
            listForm.Show();
            listForm.BringToFront();
        }
        /***********************************************************************************************/
        private void ListForm_LocationDone(string s)
        {
            if (String.IsNullOrWhiteSpace(s))
                return;
            string gplGroup = cmbGroups.Text.Trim();
            if (gplGroup.ToUpper() == "MASTER")
                return;
            string location = s;
            string[] Lines = s.Split('\n');
            if (Lines.Length <= 0)
                return;
            location = Lines[0];

            ServicesLocations servicesLocationForm = new ServicesLocations(gplGroup, location, false, null);
            servicesLocationForm.SelectDone += ServicesLocationForm_SelectDone;
            servicesLocationForm.Show();
            //ServicesLocationForm_SelectDone( location );
        }
        /***********************************************************************************************/
        private void ServicesLocationForm_SelectDone(string location)
        {
            LoadSelection();
            cmbLocations.Text = location;
            //DataTable dx = (DataTable)dgv.DataSource;
            //PullLocationGPL(dx);
        }
        /***********************************************************************************************/
        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (loading)
                return;
            string gplGroup = cmbGroups.Text.Trim();
            if (String.IsNullOrWhiteSpace(gplGroup))
                return;
            string location = cmbLocations.Text.Trim();
            if (String.IsNullOrWhiteSpace(location))
                return;
            if (gplGroup.ToUpper() == "MASTER")
                return;
            ServicesLocations servicesForm = new ServicesLocations(gplGroup, location, false, null);
            servicesForm.ShowDialog();
            ServicesLocationForm_SelectDone(location);
        }
        /***********************************************************************************************/
        private void cmbLocations_SelectedIndexChanged(object sender, EventArgs e)
        {
            DataTable dx = (DataTable)dgv.DataSource;
            PullLocationGPL(dx);
            string location = cmbLocations.Text.Trim();
            btnEdit.Hide();
            btnDeleteLocation.Hide();
            if (!String.IsNullOrWhiteSpace(location))
            {
                btnEdit.Show();
                btnDeleteLocation.Show();
            }
        }
        /***********************************************************************************************/
        private void btnDeleteLocation_Click(object sender, EventArgs e)
        {
            if (loading)
                return;
            string gplGroup = cmbGroups.Text.Trim();
            if (String.IsNullOrWhiteSpace(gplGroup))
                return;
            string location = cmbLocations.Text.Trim();
            if (String.IsNullOrWhiteSpace(location))
                return;
            if (gplGroup.ToUpper() == "MASTER")
                return;
            string cmd = "Select * from `gpl_locations` where `gpl` = '" + gplGroup + "' AND `location` = '" + location + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                string record = "";
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    record = dt.Rows[i]["record"].ObjToString();
                    G1.delete_db_table("gpl_locations", "record", record);
                }
            }
            LoadGPLLocations();
            LoadData();
        }
        /***********************************************************************************************/
        private void repositoryItemCheckEdit5_CheckedChanged(object sender, EventArgs e)
        {
            repositoryItemCheckEdit3_Click(sender, e);
        }
        /***********************************************************************************************/
        private void repositoryItemCheckEdit8_CheckedChanged(object sender, EventArgs e)
        {
            repositoryItemCheckEdit3_Click(sender, e);
        }
        /***********************************************************************************************/
        private void repositoryItemCheckEdit11_CheckedChanged(object sender, EventArgs e)
        {
            repositoryItemCheckEdit3_Click(sender, e);
        }
        /***********************************************************************************************/
        private void repositoryItemCheckEdit14_CheckedChanged(object sender, EventArgs e)
        {
            repositoryItemCheckEdit3_Click(sender, e);
        }
        /***********************************************************************************************/
        private void showBatesvilleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://selections.batesville.com/#/burial-solutions/caskets?selectionRoomId=32611"); // Batesville Caskets
        }
        /***********************************************************************************************/
        private void picAddMerch_Click(object sender, EventArgs e)
        {
            Caskets casketForm = new Caskets(true);
            casketForm.SelectDone += CasketForm_SelectDone;
            casketForm.Show();
        }
        private void CasketForm_SelectDone(DataTable dt)
        {
            workDt = (DataTable)dgv.DataSource;

            string select = "";
            string service = "";
            string casketCode = "";

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                select = dt.Rows[i]["select"].ObjToString();
                if (select == "1")
                {
                    service = dt.Rows[i]["casketdesc"].ObjToString();
                    casketCode = dt.Rows[i]["casketCode"].ObjToString();
                    if (casketCode.ToUpper() != "MISC")
                        casketCode = "Merchandise";
                    DataRow newRow = workDt.NewRow();
                    if (G1.get_column_number(workDt, "select") >= 0)
                        newRow["Select"] = "1";
                    newRow["service"] = service;
                    newRow["type"] = casketCode;
                    if ( G1.get_column_number ( workDt, "basicService") >= 0 )
                        newRow["basicService"] = "0";
                    workDt.Rows.Add(newRow);
                }
            }
            dgv.DataSource = workDt;
            dgv.Refresh();
            modified = true;
            btnSave.Show();
        }
        /***********************************************************************************************/
        private void btnInsertMerch_Click(object sender, EventArgs e)
        {
            Caskets casketForm = new Caskets(true);
            casketForm.SelectDone += CasketForm_SelectDone1;
            casketForm.Show();
            //DataTable dt = (DataTable)dgv.DataSource;
            //if (dt.Rows.Count <= 0)
            //    return;
            //DataRow dr = gridMain.GetFocusedDataRow();
            //int rowHandle = gridMain.FocusedRowHandle;
            //int dtRow = gridMain.GetDataSourceRowIndex(rowHandle);
            //if (dtRow < 0 || dtRow > (dt.Rows.Count - 1))
            //    return;
            //if (rowHandle == (dt.Rows.Count - 1))
            //    return; // Already at the last row
            //DataRow dRow = dt.NewRow();
            //dt.Rows.InsertAt(dRow, dtRow);
            //G1.NumberDataTable(dt);
            //dt.AcceptChanges();
            //dgv.DataSource = dt;
            //gridMain.ClearSelection();
            //gridMain.RefreshData();
            //gridMain.FocusedRowHandle = rowHandle + 1;
            //gridMain.SelectRow(rowHandle + 1);
            //dgv.Refresh();
        }
        private void CasketForm_SelectDone1(DataTable dtt)
        {
            DataTable dt = (DataTable)dgv.DataSource;

            string select = "";
            string service = "";
            string casketCode = "";

            int rowHandle = gridMain.FocusedRowHandle;
            int dtRow = gridMain.GetDataSourceRowIndex(rowHandle);
            if (dtRow < 0 || dtRow > (dt.Rows.Count - 1))
                return;
            if (rowHandle == (dt.Rows.Count - 1))
                return; // Already at the last row

            for (int i = 0; i < dtt.Rows.Count; i++)
            {
                select = dtt.Rows[i]["select"].ObjToString();
                if (select == "1")
                {
                    service = dtt.Rows[i]["casketdesc"].ObjToString();
                    casketCode = dtt.Rows[i]["casketCode"].ObjToString();
                    if (casketCode.ToUpper() != "MISC")
                        casketCode = "Merchandise";
                    DataRow dRow = dt.NewRow();
                    dRow["service"] = service;
                    dRow["type"] = casketCode;
                    dRow["basicService"] = "0";
                    dt.Rows.InsertAt(dRow, dtRow);
                }
            }
            dgv.DataSource = dt;
            dgv.Refresh();
            modified = true;
            btnSave.Show();
        }
        /***********************************************************************************************/
        private void btnDeletePackage_Click(object sender, EventArgs e)
        {
            if (cmbPackage.Text.Trim().ToUpper() != "MASTER")
            {
                string record = "";
                string packName = cmbPackage.Text.Trim().ToUpper();

                if (MessageBox.Show("Do you really want to delete Package " + packName + "?", "Select Package Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2, MessageBoxOptions.DefaultDesktopOnly) == DialogResult.No)
                    return;

                string group = GetGroup();
                string cmd = "Select * from `packages` p RIGHT JOIN `services` s ON p.`!serviceRecord` = s.`record` where p.`groupname` = '" + group + "' and p.`PackageName` = '" + packName + "';";
                cmd = "Select * from `packages` p JOIN `funeral_master` s ON p.`!serviceRecord` = s.`record` where p.`groupname` = '" + group + "' and p.`PackageName` = '" + packName + "';";
                DataTable dt = G1.get_db_data(cmd);
                for ( int i=0; i<dt.Rows.Count; i++)
                {
                    record = dt.Rows[i]["record"].ObjToString();
                    G1.delete_db_table("packages", "record", record);
                }
                cmd = "Select * from `packages` where `groupname` = 'master' AND `PackageName` = '" + packName + "';";
                dt = G1.get_db_data(cmd);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    record = dt.Rows[i]["record"].ObjToString();
                    G1.delete_db_table("packages", "record", record);
                }

                LoadPackagesCombo();
                LoadData();
            }
        }
        /***********************************************************************************************/
        private void btnAllOn_Click(object sender, EventArgs e)
        {
            if (!Selecting)
                return;
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt == null)
                return;
            if (G1.get_column_number(dt, "select") < 0)
                return;
            loading = true;
            if (G1.get_column_number(dt, "ModMod") < 0)
                dt.Columns.Add("ModMod");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dt.Rows[i]["select"] = "1";
                dt.Rows[i]["mod"] = "1";
                dt.Rows[i]["ModMod"] = "Y";
            }
            dgv.DataSource = dt;
            modified = true;
            loading = false;
            btnSave.Show();
            DataRow[] dRows = dt.Select("service='Package Discount'");
            if ( dRows.Length <= 0 )
                ReCalcTotal(dt);
        }
        /***********************************************************************************************/
        private bool allOff = false;
        private void btnAllOff_Click(object sender, EventArgs e)
        {
            if (!Selecting)
                return;
            allOff = true;
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt == null)
                return;
            if (G1.get_column_number(dt, "select") < 0)
                return;
            double price = 0D;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dt.Rows[i]["select"] = "0";
                dt.Rows[i]["mod"] = "1";
                price = dt.Rows[i]["price"].ObjToDouble();
                if (price < 0D)
                    dt.Rows[i]["price"] = Math.Abs(price);
            }
            dgv.DataSource = dt;
            modified = true;
            btnSave.Show();

            labBalanceDue.Text = "$" + G1.ReformatMoney(0D);


            DataRow[] dRows = dt.Select("service='Package Discount'");
            if (dRows.Length > 0)
                dRows[0]["price"] = mainPackageDiscount;
            else
                ReCalcTotal( dt );
        }
        /***********************************************************************************************/
        private void changeToMerchandiseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            dr["type"] = "Merchandise";
            modified = true;
            btnSave.Show();
        }
        /***********************************************************************************************/
        private void changeToServiceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            dr["type"] = "Service";
            modified = true;
            btnSave.Show();
        }
        /***********************************************************************************************/
        private void LoadCasketPackage()
        {
        }
        /***********************************************************************************************/
        private void chkDiscretionary_CheckedChanged(object sender, EventArgs e)
        {
            modified = true;
            btnSave.Show();
        }
        /***********************************************************************************************/
        private void gridMain_CustomSummaryCalculate(object sender, DevExpress.Data.CustomSummaryEventArgs e)
        {
            double value = e.TotalValue.ObjToDouble();
            string field = (e.Item as GridSummaryItem).FieldName.ObjToString();
            double price = 0D;
            double totalPrice = 0D;
            double currentPrice = 0D;
            double totalCurrentPrice = 0D;
            double difference = 0D;
            double totalDifference = 0D;
            double totalPackagePrice = 0D;
            double packagePrice = 0D;
            double packageDiscount = 0D;
            double cashAdvance = 0D;
            gotPackage = false;
            string type = "";
            string select = "";
            string service = "";

            bool gotSome = false;

            DataTable dt = (DataTable)dgv.DataSource;
            if ( dgv2.Visible )
                dt = (DataTable)dgv2.DataSource;
            if (dgv3.Visible)
                dt = (DataTable)dgv3.DataSource;
            if (dgv4.Visible)
                dt = (DataTable)dgv4.DataSource;
            if (dgv5.Visible)
                dt = (DataTable)dgv5.DataSource;

            if (G1.get_column_number(dt, "select") < 0)
                dt.Columns.Add("select");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                service = dt.Rows[i]["service"].ObjToString().ToUpper();
                if ( service.ToUpper() == "CASH ADVANCE")
                {
                    cashAdvance += dt.Rows[i]["price"].ObjToDouble();
                    continue;
                }
                if (service == "TOTAL LISTED PRICE")
                {
                    totalPackagePrice = dt.Rows[i]["price"].ObjToDouble();
                    continue;
                }
                else if (service == "PACKAGE PRICE")
                {
                    packagePrice = dt.Rows[i]["price"].ObjToDouble();
                    if (packagePrice > 0)
                        gotPackage = true;
                    continue;
                }
                else if (service == "PACKAGE DISCOUNT")
                {
                    packageDiscount = dt.Rows[i]["price"].ObjToDouble();
                    packageDiscount = Math.Abs(packageDiscount);
                    continue;
                }

                type = dt.Rows[i]["type"].ObjToString().ToUpper();
                select = dt.Rows[i]["select"].ObjToString();
                if (select == "1")
                {
                    gotSome = true;
                    totalPrice += dt.Rows[i]["price"].ObjToDouble();
                }
            }

            if (field.ToUpper() == "PRICE")
            {
                e.TotalValueReady = true;
                e.TotalValue = totalPrice;
                string str = "$" + G1.ReformatMoney(totalPrice);
                labBalanceDue.Text = str;
                labBalanceDue.Refresh();
                if (gotPackage && gotSome)
                {
                    e.TotalValue = packagePrice + cashAdvance;
                    str = "$" + G1.ReformatMoney(packagePrice + cashAdvance);
                    labBalanceDue.Text = str;
                    labBalanceDue.Refresh();
                }
            }
        }
        /***********************************************************************************************/
        private void repositoryItemCheckEdit16_CheckedChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            DevExpress.XtraEditors.CheckEdit check = (DevExpress.XtraEditors.CheckEdit)sender;
            bool isChecked = true;
            if (!check.Checked)
                isChecked = false;
            //DataRow dr = gridMain.GetFocusedDataRow();
            DataRow dr = GetCurrentDataRow();
            string record = dr["record"].ObjToString();
            int rowHandle = GetCurrentGridView().FocusedRowHandle;
            string select = dr["select"].ObjToString();
            int row = GetCurrentGridView().GetDataSourceRowIndex(rowHandle);
            DataTable myDt = GetCurrentDataTable();
            if (isChecked)
                myDt.Rows[row]["plonly"] = "0";
            else
                myDt.Rows[row]["plonly"] = "1";
            myDt.Rows[row]["mod"] = "1";
            GetCurrentDataGrid().DataSource = myDt;
            labBalanceDue.Text = "$" + G1.ReformatMoney(0D);
            ReCalcTotal((DataTable)dgv.DataSource);
            ReCalcTotal((DataTable)dgv2.DataSource);
            ReCalcTotal((DataTable)dgv3.DataSource);
            ReCalcTotal((DataTable)dgv4.DataSource);
            ReCalcTotal((DataTable)dgv5.DataSource);
            modified = true;
            btnSave.Show();
        }
        /***********************************************************************************************/
        private void repositoryItemCheckEdit3_CheckStateChanged(object sender, EventArgs e)
        {
            DevExpress.XtraEditors.CheckEdit check = (DevExpress.XtraEditors.CheckEdit)sender;
            bool isChecked = true;
            if (!check.Checked)
                isChecked = false;

            DataRow dr = GetCurrentDataRow();
            string record = dr["record"].ObjToString();
            int rowHandle = GetCurrentGridView().FocusedRowHandle;
            string select = dr["select"].ObjToString();
            int row = GetCurrentGridView().GetDataSourceRowIndex(rowHandle);
            DataTable dt = GetCurrentDataTable();



            if (isChecked)
            {
                dr["select"] = "1";
                dr["mod"] = "1";
            }
            else
            {
                dr["select"] = "0";
                dr["mod"] = "1";
            }

            if (G1.get_column_number(dt, "ModMod") > 0)
                dt.Rows[row]["ModMod"] = "Y";

            dt.Rows[row]["select"] = dr["select"].ObjToString();
            dt.Rows[row]["mod"] = "1";
            dt.AcceptChanges();

            GetCurrentGridView().RefreshData();
            GetCurrentGridView().EndInit();

            DataTable dx = dt.Copy();
            ReCalcTotal(dx);
            modified = true;
            btnSave.Show();
            btnSave.Refresh();

            if (dgv.Visible)
                gridMain.RefreshEditor(true);
            else if (dgv2.Visible)
                gridMain2.RefreshEditor(true);
            else if (dgv3.Visible)
                gridMain3.RefreshEditor(true);
            else if (dgv4.Visible)
                gridMain4.RefreshEditor(true);
            else if (dgv5.Visible)
                gridMain5.RefreshEditor(true);
        }
        /***********************************************************************************************/
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if ( dgv.DataSource != null )
                ReCalcTotal((DataTable)dgv.DataSource);
            if (dgv2.DataSource != null)
                ReCalcTotal((DataTable)dgv2.DataSource);
            if (dgv3.DataSource != null)
                ReCalcTotal((DataTable)dgv3.DataSource);
            if (dgv4.DataSource != null)
                ReCalcTotal((DataTable)dgv4.DataSource);
            if (dgv5.DataSource != null)
                ReCalcTotal((DataTable)dgv5.DataSource);

            if (dgv.Visible)
                gridMain.RefreshEditor(true);
            else if (dgv2.Visible)
                gridMain2.RefreshEditor(true);
            else if (dgv3.Visible)
                gridMain3.RefreshEditor(true);
            else if (dgv4.Visible)
                gridMain4.RefreshEditor(true);
            else if (dgv5.Visible)
                gridMain5.RefreshEditor(true);
        }
        /***********************************************************************************************/
        private void repositoryItemCheckEdit17_CheckedChanged(object sender, EventArgs e)
        {
        }
        /***********************************************************************************************/
        private void repositoryItemCheckEdit17_CheckStateChanged(object sender, EventArgs e)
        {
        }
        /***********************************************************************************************/
        private void repositoryItemCheckEdit18_CheckedChanged(object sender, EventArgs e)
        {
            modified = true;
            btnSave.Visible = true;
            btnSave.Refresh();
        }
        /***********************************************************************************************/
        private void repositoryItemCheckEdit19_CheckedChanged(object sender, EventArgs e)
        {
            modified = true;
            btnSave.Visible = true;
            btnSave.Refresh();
        }
        /***********************************************************************************************/
        private void repositoryItemCheckEdit20_CheckedChanged(object sender, EventArgs e)
        {
            modified = true;
            btnSave.Visible = true;
            btnSave.Refresh();
        }
        /***********************************************************************************************/
        private void repositoryItemCheckEdit21_CheckedChanged(object sender, EventArgs e)
        {
            modified = true;
            btnSave.Visible = true;
            btnSave.Refresh();
        }

        private void repositoryItemCheckEdit3_CheckedChanged(object sender, EventArgs e)
        {

        }
        /***********************************************************************************************/
        private void repositoryItemCheckEdit22_CheckedChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            DevExpress.XtraEditors.CheckEdit check = (DevExpress.XtraEditors.CheckEdit)sender;
            bool isChecked = true;
            if (!check.Checked)
                isChecked = false;
            //DataRow dr = gridMain.GetFocusedDataRow();
            DataRow dr = GetCurrentDataRow();
            string record = dr["record"].ObjToString();
            int rowHandle = GetCurrentGridView().FocusedRowHandle;
            string select = dr["select"].ObjToString();
            int row = GetCurrentGridView().GetDataSourceRowIndex(rowHandle);
            DataTable myDt = GetCurrentDataTable();
            if (isChecked)
                myDt.Rows[row]["noSelect"] = "0";
            else
                myDt.Rows[row]["noSelect"] = "1";
            myDt.Rows[row]["mod"] = "1";
            GetCurrentDataGrid().DataSource = myDt;
            labBalanceDue.Text = "$" + G1.ReformatMoney(0D);
            ReCalcTotal((DataTable)dgv.DataSource);
            //ReCalcTotal((DataTable)dgv2.DataSource);
            //ReCalcTotal((DataTable)dgv3.DataSource);
            //ReCalcTotal((DataTable)dgv4.DataSource);
            //ReCalcTotal((DataTable)dgv5.DataSource);
            modified = true;
            btnSave.Show();
        }
        /***********************************************************************************************/
        private void chkPackage_CheckedChanged(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt.Rows.Count <= 0)
                return;

            DataRow dRow = null;
            DataRow[] dRows = null;

            int packageCount = 0;

            if (chkPackage.Checked)
            {
                dt = LoadPackageService(dt, "Total Listed Price");
                dt = LoadPackageService(dt, "Package Discount");
                dt = LoadPackageService(dt, "Package Price");
                btnSave.Show();
                btnSave.Refresh();
                modified = true;
                dgv.DataSource = dt;
                dgv.Refresh();
            }
        }
        /***********************************************************************************************/
        private DataTable LoadPackageService ( DataTable dt, string service )
        {
            DataRow[] dRows = dt.Select("service='" + service + "'");
            if (dRows.Length > 0)
                return dt;

            string packageName = dt.Rows[0]["PackageName"].ObjToString();

            string cmd  = "Select * from `funeral_master` WHERE `service` = '" + service + "';";
            DataTable dx = G1.get_db_data(cmd);
            if ( dx.Rows.Count > 0 )
            {
                DataRow dRow = dt.NewRow();
                dRow["groupname"] = "MASTER";
                dRow["PackageName"] = packageName;
                dRow["!serviceRecord"] = dx.Rows[0]["record"].ObjToString();
                dRow["type"] = "service";
                dRow["select"] = "0";
                dRow["noSelect"] = "0";
                dRow["service"] = service;
                dRow["price"] = dx.Rows[0]["price"].ObjToDouble();
                dRow["futurePrice"] = dx.Rows[0]["futurePrice"].ObjToDouble();
                dRow["pastPrice"] = dx.Rows[0]["pastPrice"].ObjToDouble();
                dRow["price1"] = dx.Rows[0]["price"].ObjToDouble();
                dRow["futurePrice1"] = dx.Rows[0]["futurePrice"].ObjToDouble();
                dRow["pastPrice1"] = dx.Rows[0]["pastPrice"].ObjToDouble();
                dRow["mod"] = "1";
                dt.Rows.Add(dRow);
            }
            return dt;
        }
        /***********************************************************************************************/
    }
}