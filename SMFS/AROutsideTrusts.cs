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

using DevExpress.Utils.Drawing;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraEditors.ViewInfo;
using DevExpress.XtraEditors.Repository;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class AROutsideTrusts : DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        private DataTable workDt = null;
        private DataTable originalDt = null;
        private string workReport = "";
        private DataTable oDt = null;
        private bool loading = false;
        private bool insurance = false;
        private bool foundLocalPreference = false;
        private GridView originalGridView = null;
        private bool showFooters = true;
        private bool modified = false;
        /***********************************************************************************************/
        public AROutsideTrusts(string report)
        {
            InitializeComponent();
            workReport = report;
        }
        /***********************************************************************************************/
        RepositoryItemDateEdit ri = null;
        private void AROutsideTrusts_Load(object sender, EventArgs e)
        {
            loading = true;

            btnSavePayments.Hide();
            modified = false;

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

            G1.loadGroupCombo(cmbSelectColumns, "AR " + workReport, "Primary", true, LoginForm.username);
            cmbSelectColumns.Text = "Original";

            loading = false;

            this.dateTimePicker1.Value = new DateTime(DateTime.Now.Year, 1, 1);

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

            SetOutsideTrustClaims();
        }
        /****************************************************************************************/
        private void SetOutsideTrustClaims()
        {
            int i = 1;
            ClearAllPositions(gridMain);
            G1.SetColumnPosition(gridMain, "num", i);
            G1.SetColumnPosition(gridMain, "paidInFull", ++i);
            G1.SetColumnPosition(gridMain, "dateFiled", ++i);
            G1.SetColumnPosition(gridMain, "name", ++i);
            G1.SetColumnPosition(gridMain, "endingBalance", ++i);
            G1.SetColumnPosition(gridMain, "dbr", ++i);
            G1.SetColumnPosition(gridMain, "locind", ++i);
            G1.SetColumnPosition(gridMain, "issueDateAfter", ++i);
            G1.SetColumnPosition(gridMain, "contractNumber", ++i);
            G1.SetColumnPosition(gridMain, "trustee", ++i);
            G1.SetColumnPosition(gridMain, "paidOut", ++i);
            G1.SetColumnPosition(gridMain, "paidUs", ++i);
            G1.SetColumnPosition(gridMain, "datePaid", ++i);
            G1.SetColumnPosition(gridMain, "agent", ++i);
            G1.SetColumnPosition(gridMain, "datePaid", ++i);
            G1.SetColumnPosition(gridMain, "funeralHome", ++i);
            G1.SetColumnPosition(gridMain, "deceasedDate", ++i);
            G1.SetColumnPosition(gridMain, "amountReceived", ++i);

            gridMain.Columns["amountReceived"].Caption = "TBB Report";

            //G1.SetColumnPosition(gridMain, "insCompany", ++i);
            //G1.SetColumnPosition(gridMain, "trustAmountFiled", ++i);
            //G1.SetColumnPosition(gridMain, "certDCRequired", ++i);
            //G1.SetColumnPosition(gridMain, "dateCertDCFiled", ++i);
            //G1.SetColumnPosition(gridMain, "trustAmountReceived", ++i);
            //G1.SetColumnPosition(gridMain, "dateReceived", ++i);


            //G1.SetColumnPosition(gridMain, "serviceId", ++i);
            //G1.SetColumnPosition(gridMain, "premiumType", ++i);
            //G1.SetColumnPosition(gridMain, "issueDate8", ++i);
            //G1.SetColumnPosition(gridMain, "paidUp", ++i);
            //G1.SetColumnPosition(gridMain, "trustee", ++i);
            //G1.SetColumnPosition(gridMain, "endingBalance", ++i);
            //G1.SetColumnPosition(gridMain, "amountDBR", ++i);
            //G1.SetColumnPosition(gridMain, "contractNumber", ++i);
            //G1.SetColumnPosition(gridMain, "principleReceived", ++i);
            //G1.SetColumnPosition(gridMain, "growthReceived", ++i);

            //G1.SetColumnPosition(gridMain, "localDescription", ++i);
            //G1.SetColumnPosition(gridMain, "depositNumber", ++i);
            //G1.SetColumnPosition(gridMain, "notes", ++i);



            //G1.SetColumnPosition(gridMain, "num", 1);
            //G1.SetColumnPosition(gridMain, "paidInFull", 2);
            //G1.SetColumnPosition(gridMain, "deceasedDate", 3);
            //G1.SetColumnPosition(gridMain, "serviceId", 4);
            //G1.SetColumnPosition(gridMain, "name", 5);
            //G1.SetColumnPosition(gridMain, "premiumType", 6);
            //G1.SetColumnPosition(gridMain, "issueDate8", 7);
            //G1.SetColumnPosition(gridMain, "paidUp", 8);
            //G1.SetColumnPosition(gridMain, "trustee", 9);
            //G1.SetColumnPosition(gridMain, "endingBalance", 10);
            //G1.SetColumnPosition(gridMain, "amountDBR", 11);
            //G1.SetColumnPosition(gridMain, "tbbLoc", 12);
            //G1.SetColumnPosition(gridMain, "principleReceived", 14);
            //G1.SetColumnPosition(gridMain, "growthReceived", 15);
            //G1.SetColumnPosition(gridMain, "notes", 16);
        }
        /****************************************************************************************/
        private void SetUnityClaims()
        {
            int i = 1;
            ClearAllPositions(gridMain);
            G1.SetColumnPosition(gridMain, "num", ++i);
            G1.SetColumnPosition(gridMain, "serviceId", ++i);
            G1.SetColumnPosition(gridMain, "SRVLOC", ++i);
            G1.SetColumnPosition(gridMain, "deceasedDate", ++i);
            G1.SetColumnPosition(gridMain, "name", ++i);



            //G1.SetColumnPosition(gridMain, "premiumType", 6);
            //G1.SetColumnPosition(gridMain, "issueDate8", 7);
            //G1.SetColumnPosition(gridMain, "paidUp", 8);
            //G1.SetColumnPosition(gridMain, "trustee", 9);
            //G1.SetColumnPosition(gridMain, "endingBalance", 10);
            //G1.SetColumnPosition(gridMain, "amountDBR", 11);
            //G1.SetColumnPosition(gridMain, "tbbLoc", 12);
            //G1.SetColumnPosition(gridMain, "contractNumber", 13);
            //G1.SetColumnPosition(gridMain, "principleReceived", 14);
            //G1.SetColumnPosition(gridMain, "growthReceived", 15);
            //G1.SetColumnPosition(gridMain, "notes", 16);

            //G1.SetColumnPosition(gridMain, "paidInFull", 2);
        }
        /****************************************************************************************/
        private void SetNonUnityClaims()
        {
            int i = 1;
            ClearAllPositions(gridMain);
            G1.SetColumnPosition(gridMain, "num", i);
            G1.SetColumnPosition(gridMain, "paidInFull", ++i);
            G1.SetColumnPosition(gridMain, "serviceId", ++i);
            G1.SetColumnPosition(gridMain, "name", ++i);
            G1.SetColumnPosition(gridMain, "Funeral Arranger", ++i);
            G1.SetColumnPosition(gridMain, "deceasedDate", ++i);
            G1.SetColumnPosition(gridMain, "insCompany", ++i);
            G1.SetColumnPosition(gridMain, "trustAmountFiled", ++i);
            G1.SetColumnPosition(gridMain, "dateFiled", ++i);
            G1.SetColumnPosition(gridMain, "certDCRequired", ++i);
            G1.SetColumnPosition(gridMain, "dateCertDCFiled", ++i);
            G1.SetColumnPosition(gridMain, "trustAmountReceived", ++i);
            G1.SetColumnPosition(gridMain, "dateReceived", ++i);


            //G1.SetColumnPosition(gridMain, "premiumType", ++i);
            //G1.SetColumnPosition(gridMain, "issueDate8", ++i);
            //G1.SetColumnPosition(gridMain, "paidUp", ++i);
            //G1.SetColumnPosition(gridMain, "trustee", ++i);
            //G1.SetColumnPosition(gridMain, "endingBalance", ++i);
            //G1.SetColumnPosition(gridMain, "amountDBR", ++i);
            //G1.SetColumnPosition(gridMain, "tbbLoc", ++i);
            //G1.SetColumnPosition(gridMain, "contractNumber", ++i);
            //G1.SetColumnPosition(gridMain, "principleReceived", ++i);
            //G1.SetColumnPosition(gridMain, "growthReceived", ++i);

            G1.SetColumnPosition(gridMain, "localDescription", ++i);
            G1.SetColumnPosition(gridMain, "depositNumber", ++i);
            G1.SetColumnPosition(gridMain, "notes2", ++i);
        }
        /****************************************************************************************/
        private void ClearAllPositions(DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gMain = null)
        {
            if (gMain == null)
                gMain = gridMain;
            for (int i = 0; i < gMain.Columns.Count; i++)
            {
                gMain.Columns[i].Visible = false;
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
            if (userDt.Rows.Count > 0)
                assignedLocations = userDt.Rows[0]["assignedLocations"].ObjToString();

            string locationCode = "";
            string keyCode = "";
            string[] Lines = null;
            string locations = "";
            string location = "";

            for (int i = locDt.Rows.Count - 1; i >= 0; i--)
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


            //AddSummaryColumn("currentprice", null);
            //AddSummaryColumn("difference", null);
        }
        /*******************************************************************************************/
        private string getLocationQuery()
        {
            DataRow[] dRows = null;
            DataTable locDt = (DataTable)this.chkComboLocation.Properties.DataSource;
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
                    if (dRows.Length > 0)
                    {
                        jewelLoc = dRows[0]["merchandiseCode"].ObjToString();
                        if (!String.IsNullOrWhiteSpace(jewelLoc))
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
            modified = false;
            btnSavePayments.Hide();
            modified = false;
            this.Cursor = Cursors.WaitCursor;

            DataTable dt = null;

            //            string cmd = "SELECT * FROM `fcust_extended` e LEFT JOIN `fcontracts` p ON p.`contractNumber` = e.`contractNumber` left join `fcustomers` d ON e.`contractNumber` = d.`contractNumber` LEFT JOIN `icontracts` i ON i.`contractNumber` = e.`contractNumber` LEFT JOIN `icustomers` j ON j.`contractNumber` = e.`contractNumber` WHERE e.`ServiceID` <> '' ";
            //string cmd = "SELECT * FROM `fcust_extended` e LEFT JOIN `fcontracts` p ON p.`contractNumber` = e.`contractNumber` left join `fcustomers` d ON e.`contractNumber` = d.`contractNumber` LEFT JOIN `icontracts` i ON i.`contractNumber` = e.`contractNumber` LEFT JOIN `icustomers` j ON j.`contractNumber` = e.`contractNumber` ";
            string cmd = "Select * from `cust_payment_outside` c LEFT JOIN `customers` x ON c.`contractNumber` = x.`contractNumber` LEFT JOIN `contracts` q ON c.`contractNumber` = q.`contractNumber`";

            string paidInFull = cmbPaidInFull.Text;
            if (!String.IsNullOrWhiteSpace(paidInFull))
            {
                if (paidInFull.Trim().ToUpper() == "EXCLUDE PAID IN FULL")
                    cmd += " AND `paidInFull` <> '1' ";
                else if (paidInFull.Trim().ToUpper() == "PAID IN FULL ONLY")
                    cmd += " AND `paidInFull` = '1' ";
            }
            string what = "";
            if (chkUseDates.Checked)
            {
                string date1 = this.dateTimePicker1.Value.ToString("yyyy-MM-dd");
                string date2 = this.dateTimePicker2.Value.ToString("yyyy-MM-dd");
                what = comboBox1.Text.Trim().ToUpper();
                if (what == "DECEASED")
                    cmd += " AND c.`deceasedDate` >= '" + date1 + "' AND c.`deceasedDate` <= '" + date2 + "' ";
                else if (what == "PAID")
                    cmd += " AND c.`datePaid` >= '" + date1 + "' AND c.`datePaid` <= '" + date2 + "' ";
                else
                    cmd += " AND c.`dateFiled` >= '" + date1 + "' AND c.`dateFiled` <= '" + date2 + "' ";
            }

            if (what == "DECEASED")
                cmd += " ORDER BY c.`deceasedDate`, c.`contractNumber`, c.`secondary` ";
            else if (what == "PAID")
                cmd += " ORDER BY c.`datePaid`, c.`contractNumber`, c.`secondary` ";
            else if (what == "FILED")
                cmd += " ORDER BY c.`dateFiled`, c.`contractNumber`, c.`secondary` ";
            else
                cmd += " ORDER BY c.`dateFiled`, c.`contractNumber`, c.`secondary` ";

            cmd += ";";

            dt = G1.get_db_data(cmd);

            dt.Columns.Add("mod");

            PreProcessData(dt);

            SetupPaidUpCheck(dt);

            DataView tempview = dt.DefaultView;
            what = comboBox1.Text.Trim().ToUpper();
            if (what == "DECEASED")
                tempview.Sort = "contractNumber, secondary";
            else if (what == "PAID")
                tempview.Sort = "contractNumber, secondary";
            else if (what == "FILED")
                tempview.Sort = "contractNumber, secondary";
            else
                tempview.Sort = "contractNumber, secondary";

            dt = tempview.ToTable();


            G1.NumberDataTable(dt);
            originalDt = dt.Copy();
            dgv.DataSource = dt;
            ScaleCells();

            //chkComboLocation_EditValueChanged(null, null);

            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private DataTable trust2013Dt = null;
        private void PullTheData()
        {
            trust2013Dt = null;
            DateTime date = this.dateTimePicker2.Value;
            string myDate = date.ToString("yyyy-MM-dd");
            string cmd = "Select * from `trust2013r` where `payDate8` <= '" + myDate + "' ORDER BY `payDate8` DESC LIMIT 1;";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                myDate = dx.Rows[0]["payDate8"].ObjToDateTime().ToString("yyyy-MM-dd");
                cmd = "Select * from `trust2013r` where `payDate8` = '" + myDate + "' ORDER BY `payDate8`;";
                trust2013Dt = G1.get_db_data(cmd);
            }
        }
        /***********************************************************************************************/
        private void PreProcessData(DataTable dt)
        {

            PullTheData();

            string contractNumber = "";
            string lastName = "";
            string firstName = "";
            string payer = "";
            bool noFirstName1 = false;
            DateTime dueDate = DateTime.Now;
            string cmd = "";
            DataTable dx = null;
            double endingBalance = 0D;
            double amountDBR = 0D;
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
                if (G1.get_column_number(dt, "amountDBR") < 0)
                    dt.Columns.Add("amountDBR", Type.GetType("System.Double"));
                if (G1.get_column_number(dt, "principleReceived") < 0)
                    dt.Columns.Add("principleReceived", Type.GetType("System.Double"));
                if (G1.get_column_number(dt, "growthReceived") < 0)
                    dt.Columns.Add("growthReceived", Type.GetType("System.Double"));
                if (G1.get_column_number(dt, "notes") < 0)
                    dt.Columns.Add("notes");
                if (G1.get_column_number(dt, "insCompany") < 0)
                    dt.Columns.Add("insCompany");

                for (int i = 0; i < dt.Rows.Count; i++)
                {
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

                    dt.Rows[i]["trustee"] = dt.Rows[i]["trustee"].ObjToString();
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

                    //if (trust2013Dt != null && type.ToUpper() == "TRUST")
                    //{
                    //    cnum = dt.Rows[i]["trust_policy"].ObjToString();
                    //    if (String.IsNullOrWhiteSpace(cnum))
                    //        cnum = contractNumber;
                    //    else
                    //        dt.Rows[i]["contractNumber"] = cnum;
                    //    dRows = trust2013Dt.Select("contractNumber='" + cnum + "'");
                    //    if (dRows.Length > 0)
                    //    {
                    //        locind = dRows[0]["locind"].ObjToString();
                    //        dt.Rows[i]["locind"] = locind;
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
                    //            dt.Rows[i]["locind"] = locind;
                    //            endingBalance = dx.Rows[0]["endingBalance"].ObjToDouble();
                    //            if (endingBalance == 0D)
                    //                endingBalance = dx.Rows[0]["beginningBalance"].ObjToDouble();
                    //            dt.Rows[i]["endingBalance"] = endingBalance;
                    //        }
                    //    }
                    //}
                    //balanceDue = dt.Rows[i]["balanceDue"].ObjToDouble();
                    //contractValue = DailyHistory.GetContractValuePlus(dt.Rows[i]);
                    //dt.Rows[i]["principleReceived"] = contractValue - balanceDue;

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
                //    for (int i = dt.Rows.Count - 1; i >= 0; i--)
                //    {
                //        paymentRecord = dt.Rows[i]["paymentRecord"].ObjToString();
                //        if (paymentRecord == "-1")
                //            dt.Rows.RemoveAt(i);
                //    }
                //}
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
        private void SetupPaidUpCheck(DataTable dt)
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
                if (paid == "1")
                    dt.Rows[i]["paidInFull"] = "1";
                else
                    dt.Rows[i]["paidInFull"] = "0";
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
            string title = workReport + " Report";
            Printer.DrawQuad(6, 8, 4, 4, title, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

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
                CustomerDetails clientForm = new CustomerDetails(contract);
                clientForm.Show();
                this.Cursor = Cursors.Default;
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
            SelectDisplayColumns sform = new SelectDisplayColumns(dgv, "AR " + workReport, "Primary", actualName, LoginForm.username);
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
        void FilterControl_BeforeShowValueEditor(object sender, DevExpress.XtraEditors.Filtering.ShowValueEditorEventArgs e)
        {
            if (e.PropertyName.ToUpper() == "TMSTAMP")
                e.CustomRepositoryItem = ri;
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
            if (!String.IsNullOrWhiteSpace(record))
                G1.update_db_table("cust_payment_outside", "record", record, new string[] { "paidInFull", value });
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

            //DataTable dt = (DataTable)(dgv.DataSource);
            //DataRow dr = gridMain.GetFocusedDataRow();
            //string contract = dr["contractNumber"].ObjToString();
            //string value = dr["paidInFull"].ObjToString();
            //string record = dr["record"].ObjToString();
            //int row = gridMain.GetDataSourceRowIndex(gridMain.FocusedRowHandle);
            //if (value == "0")
            //    value = "1";
            //else
            //    value = "0";
            //dr["paidInFull"] = value;
            //dt.Rows[row]["paidInFull"] = value;
            //dgv.DataSource = dt;
            //G1.update_db_table("fcust_extended", "record", record, new string[] { "paidInFull", value });
            //gridMain.PostEditor();

        }
        /***********************************************************************************************/
        private void gridMain_CalcRowHeight(object sender, RowHeightEventArgs e)
        {
            GridView View = sender as GridView;
            if (e.RowHandle >= 0)
            {
                int maxHeight = 0;
                int newHeight = 0;
                foreach (GridColumn column in gridMain.Columns)
                {
                    if (column.Visible)
                    {
                        if (column.FieldName.ToUpper() != "FUNERALHOME")
                            continue;
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
        private void gridMain_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            ColumnView view = sender as ColumnView;
            string fieldName = e.Column.FieldName.ToUpper();

            if ( fieldName == "DECEASEDDATE" || fieldName == "FUNERALHOME" || fieldName == "AMOUNTRECEIVED" || fieldName == "AGENT" || fieldName == "LOCIND" || fieldName == "NAME")
            {
                DataTable dt = (DataTable)dgv.DataSource;
                int row = e.ListSourceRowIndex;
                string secondary = dt.Rows[row]["secondary"].ObjToString().ToUpper();
                if (secondary == "Y")
                {
                    e.DisplayText = "";
                    return;
                }
            }

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
        private Font newFont = null;
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
            gridMain.Appearance.GroupFooter.Font = font;
            gridMain.AppearancePrint.FooterPanel.Font = font;
            newFont = font;
            gridMain.RefreshData();
            gridMain.RefreshEditor(true);
            dgv.Refresh();
            this.Refresh();
        }
        /***********************************************************************************************/
        private void pictureDelete_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt == null)
                return;
            if (dt.Rows.Count <= 0)
                return;
            DataRow dr = gridMain.GetFocusedDataRow();
            if (dr == null)
                return;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            string contractNumber = dr["contractNumber"].ObjToString();
            string record = dr["record"].ObjToString();

            DialogResult result = DevExpress.XtraEditors.XtraMessageBox.Show("***Question***\nDo you really want to REMOVE Contract " + contractNumber + "?", "Remove Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;

            if (!String.IsNullOrWhiteSpace(record))
            {
                if (record != "0")
                    G1.delete_db_table("cust_payment_outside", "record", record);
            }
            dt.Rows.Remove(dr);
        }
        /***********************************************************************************************/
        private void pictureAdd_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            FunLookup fastForm = new FunLookup("", "");
            fastForm.SelectDone += FastForm_SelectDone;
            //fastForm.ListDone += FastForm_ListDone;
            fastForm.Show();
        }
        /****************************************************************************************/
        private DataTable SetupFirst ()
        {
            string cmd = "Select * from `cust_payment_outside` where `contractNumber` = 'Xyzzy223344';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
                dt.Clear();

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
            if (G1.get_column_number(dt, "amountDBR") < 0)
                dt.Columns.Add("amountDBR", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "locind") < 0)
                dt.Columns.Add("locind");
            if (G1.get_column_number(dt, "principleReceived") < 0)
                dt.Columns.Add("principleReceived", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "growthReceived") < 0)
                dt.Columns.Add("growthReceived", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "notes") < 0)
                dt.Columns.Add("notes");
            if (G1.get_column_number(dt, "insCompany") < 0)
                dt.Columns.Add("insCompany");
            if (G1.get_column_number(dt, "amountReceived") < 0)
                dt.Columns.Add("amountReceived", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "trustee") < 0)
                dt.Columns.Add("trustee");
            if (G1.get_column_number(dt, "agent") < 0)
                dt.Columns.Add("agent");
            if (G1.get_column_number(dt, "funeralHome") < 0)
                dt.Columns.Add("funeralHome");
            if (G1.get_column_number(dt, "secondary") < 0)
                dt.Columns.Add("secondary");

            SetupPaidUpCheck(dt);

            return dt;
        }
        /****************************************************************************************/
        private void FastForm_SelectDone(DataTable s)
        {
            if (s == null)
                return;
            DataRow[] dRows = s.Select("select='1'");
            if (dRows.Length <= 0)
                return;

            DataTable dt = (DataTable)dgv.DataSource;
            if (dt == null)
                dt = SetupFirst();

            AddMod(dt, gridMain);
            string contractNumber = "";
            string record = "";
            string agentCode = "";
            string agent = "";
            DateTime date = DateTime.Now;
            DateTime deceasedDate = DateTime.Now;
            string cmd = "";
            DataTable dx = null;
            string secondary = "";
            string name = "";
            for (int i = 0; i < dRows.Length; i++)
            {
                contractNumber = dRows[i]["contractNumber"].ObjToString();
                if (String.IsNullOrWhiteSpace(contractNumber))
                    continue;
                if (DailyHistory.isInsurance(contractNumber))
                {
                    continue;
                }
                else
                {
                    try
                    {
                        secondary = "";
                        cmd = "Select * from `cust_payment_outside` where `contractNumber` = '" + contractNumber + "';";
                        dx = G1.get_db_data(cmd);
                        if ( dx.Rows.Count > 0 )
                        {
                            if (MessageBox.Show("Contract Number (" + contractNumber + ") is already listed as an Outside Claim!\nDo you want to add a Secondary Claim for this contract?", "Duplicate Contract Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly) == DialogResult.No)
                                continue;
                            secondary = "Y";
                        }
                        record = G1.create_record("cust_payment_outside", "agent", "-1");
                        if (G1.BadRecord("cust_payment_outside", record))
                            continue;

                        G1.update_db_table("cust_payment_outside", "record", record, new string[] { "contractNumber", contractNumber, "secondary", secondary});

                        DataRow dRow = dt.NewRow();
                        dRow["record"] = record;

                        dRow["contractNumber"] = contractNumber;
                        name = dRows[i]["lastName"].ObjToString() + ", " + dRows[i]["firstName"].ObjToString() + " " + dRows[i]["middleName"].ObjToString();
                        dRow["name"] = name;

                        double tbb = 0D;
                        double teb = 0D;
                        double dValue = 0D;
                        string locind = "";

                        cmd = "Select * from `trust2013r` where `contractNumber` = '" + contractNumber + "' ORDER BY `payDate8` DESC;";
                        dx = G1.get_db_data(cmd);
                        for (int j = 0; j < dx.Rows.Count; j++)
                        {
                            if (String.IsNullOrWhiteSpace(locind))
                                locind = dx.Rows[j]["locind"].ObjToString();

                            if (teb == 0D)
                                teb = dx.Rows[j]["endingBalance"].ObjToDouble();
                            if (tbb == 0D)
                                tbb = dx.Rows[j]["beginningBalance"].ObjToDouble();
                            if (teb != 0D && tbb != 0D)
                                break;
                        }

                        dRow["amountReceived"] = teb;
                        if (teb == 0D)
                            dRow["amountReceived"] = tbb;
                        dRow["locind"] = locind;
                        dRow["paidInFull"] = "0";

                        agent = "";
                        agentCode = dRows[i]["agentCode"].ObjToString();
                        if (!String.IsNullOrWhiteSpace(agentCode))
                        {
                            cmd = "Select * from `agents` where `agentCode` = '" + agentCode + "';";
                            dx = G1.get_db_data(cmd);
                            if (dx.Rows.Count > 0)
                            {
                                agent = dx.Rows[0]["firstName"] + " " + dx.Rows[0]["lastName"].ObjToString();
                                dRow["agent"] = agent;
                            }
                        }

                        deceasedDate = dRows[i]["deceasedDate"].ObjToDateTime();
                        dRow["deceasedDate"] = G1.DTtoMySQLDT(deceasedDate);

                        dRow["secondary"] = secondary;

                        dt.Rows.Add(dRow);

                        dValue = dRow["amountReceived"].ObjToDouble();
                        G1.update_db_table("cust_payment_outside", "record", record, new string[] {"name", name, "deceasedDate", deceasedDate.ToString("yyyy-MM-dd"), "locind", locind, "paidInFull", "0", "amountReceived", dValue.ToString(), "agent", agent });
                    }
                    catch ( Exception ex)
                    {
                    }
                }
            }


            DataView tempview = dt.DefaultView;
            string what = comboBox1.Text.Trim().ToUpper();
            if (what == "DECEASED")
                tempview.Sort = "contractNumber, secondary";
            else if (what == "PAID")
                tempview.Sort = "contractNumber, secondary";
            else if (what == "FILED")
                tempview.Sort = "contractNumber, secondary";
            else
                tempview.Sort = "contractNumber, secondary";

            dt = tempview.ToTable();

            G1.NumberDataTable(dt);

            DataRow [] ddR = dt.Select("record='" + record + "'");

            int row = -1;
            if (!String.IsNullOrWhiteSpace(record))
            {
                if ( ddR.Length > 0 )
                    row = ddR[0]["num"].ObjToInt32() - 1;
            }


            dgv.DataSource = dt;
            dgv.Refresh();

            //int row = dt.Rows.Count - 1;
            if ( row >= 0 )
                gridMain.FocusedRowHandle = row;
            this.Cursor = Cursors.Arrow;
        }
        /***********************************************************************************************/
        private void AddMod(DataTable dt, DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView grid)
        {
            if (G1.get_column_number(dt, "mod") < 0)
                dt.Columns.Add("mod");
        }
        /***********************************************************************************************/
        private void gridMain_CellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            if (e == null)
                return;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            DataRow dr = gridMain.GetFocusedDataRow();
            if (dr == null)
                return;
            //dr["mod"] = "Y";
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt == null)
                return;
            string column = e.Column.FieldName.ToUpper();
            string record = dr["record"].ObjToString();
            if (String.IsNullOrWhiteSpace(record))
                return;
            if ( column == "DATEFILED")
            {
                string date = dr["datefiled"].ObjToDateTime().ToString("yyyy-MM-dd");
                G1.update_db_table("cust_payment_outside", "record", record, new string[] {"datefiled", date });
            }

            //dt.Rows[row]["mod"] = "Y";
            //SetupSave();
        }
        /****************************************************************************************/
        private void SetupSave()
        {
            modified = true;
            btnSavePayments.Show();
            btnSavePayments.Refresh();
        }
        /****************************************************************************************/
        private string oldWhat = "";
        /***********************************************************************************************/
        private void gridMain_ValidatingEditor(object sender, DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventArgs e)
        {
            GridView view = sender as GridView;
            if (view.FocusedColumn.FieldName.ToUpper() == "DATEFILED")
            {
                DataTable dt = (DataTable)dgv.DataSource;
                DataRow dr = gridMain.GetFocusedDataRow();
                int rowhandle = gridMain.FocusedRowHandle;
                int row = gridMain.GetDataSourceRowIndex(rowhandle);
                oldWhat = e.Value.ObjToString();
                DateTime date = oldWhat.ObjToDateTime();
                dt.Rows[row]["dateFiled"] = G1.DTtoMySQLDT(date);
                e.Value = G1.DTtoMySQLDT(date);
            }
            else if (view.FocusedColumn.FieldName.ToUpper() == "DATEPAID")
            {
                DataTable dt = (DataTable)dgv.DataSource;
                DataRow dr = gridMain.GetFocusedDataRow();
                int rowhandle = gridMain.FocusedRowHandle;
                int row = gridMain.GetDataSourceRowIndex(rowhandle);
                oldWhat = e.Value.ObjToString();
                DateTime date = oldWhat.ObjToDateTime();
                dt.Rows[row]["datePaid"] = G1.DTtoMySQLDT(date);
                e.Value = G1.DTtoMySQLDT(date);
            }
            else if (view.FocusedColumn.FieldName.ToUpper() == "ISSUEDATEAFTER")
            {
                DataTable dt = (DataTable)dgv.DataSource;
                DataRow dr = gridMain.GetFocusedDataRow();
                int rowhandle = gridMain.FocusedRowHandle;
                int row = gridMain.GetDataSourceRowIndex(rowhandle);
                oldWhat = e.Value.ObjToString();
                DateTime date = oldWhat.ObjToDateTime();
                dt.Rows[row]["issueDateAfter"] = G1.DTtoMySQLDT(date);
                e.Value = G1.DTtoMySQLDT(date);
            }
            else if (view.FocusedColumn.FieldName.ToUpper() == "DECEASEDDATE")
            {
                DataTable dt = (DataTable)dgv.DataSource;
                DataRow dr = gridMain.GetFocusedDataRow();
                int rowhandle = gridMain.FocusedRowHandle;
                int row = gridMain.GetDataSourceRowIndex(rowhandle);
                oldWhat = e.Value.ObjToString();
                DateTime date = oldWhat.ObjToDateTime();
                dt.Rows[row]["deceasedDate"] = G1.DTtoMySQLDT(date);
                e.Value = G1.DTtoMySQLDT(date);
            }
        }
        /***********************************************************************************************/
        private void gridMain_ShowingEditor(object sender, System.ComponentModel.CancelEventArgs e)
        {
            GridView view = sender as GridView;
            DataRow dr = gridMain.GetFocusedDataRow();
            string column = gridMain.FocusedColumn.FieldName.ToUpper();

            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            if (column == "DATEFILED")
            {
                DateTime date = dr["dateFiled"].ObjToDateTime();
                dr["dateFiled"] = G1.DTtoMySQLDT(date);
            }
            else if (column == "ISSUEDATEAFTER")
            {
                DateTime date = dr["issueDateAfter"].ObjToDateTime();
                dr["issueDateAfter"] = G1.DTtoMySQLDT(date);
            }
        }
        /***********************************************************************************************/
        private void btnSavePayments_Click(object sender, EventArgs e)
        {
        }
        /***********************************************************************************************/
        private void gridMain_CustomDrawFooterCell(object sender, FooterCellCustomDrawEventArgs e)
        {
            if (newFont != null)
                e.Appearance.Font = newFont;
        }
        /***********************************************************************************************/
    }
}
