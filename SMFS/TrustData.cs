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
using DevExpress.XtraGrid.Views.Base;
using DevExpress.Xpo.Helpers;
using System.IO;
using ExcelLibrary.BinaryFileFormat;
using System.Security.Cryptography;
using DevExpress.XtraPrinting;
using DevExpress.Utils;
using MySql.Data.MySqlClient;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Columns;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class TrustData : DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        private DataTable workDt = null;
        private string workWhat = "";
        private string title = "";
        private string workContract = "";
        private string workPolicy = "";
        private bool loading = true;
        private bool foundLocalPreference = false;
        private DataTable originalDt = null;
        private string workReport = "";
        private bool workAll = false;
        /***********************************************************************************************/
        public TrustData(string contractNumber = "", string policyNumber = "")
        {
            InitializeComponent();
            workContract = contractNumber;
            workPolicy = policyNumber;
            //workDt = dt;
            //workWhat = what;

            SetupTotalsSummary();

            barImport.Hide();

            if (!LoginForm.isReallyRobby)
                txtContract.Hide();

            //LoadMonths();
            //LoadYears();
        }
        /***********************************************************************************************/
        public TrustData( DataTable dt )
        {
            InitializeComponent();
            workContract = "";
            workPolicy = "";
            workDt = dt;
            //workWhat = what;

            SetupTotalsSummary();

            barImport.Hide();

            if (!LoginForm.isReallyRobby)
                txtContract.Hide();

            //LoadMonths();
            //LoadYears();
        }
        /***********************************************************************************************/
        private bool GoAheadAndRun = false;
        private string trustCompanysText = "";
        private DateTime TrustRunDate = DateTime.Now;
        public TrustData(DateTime date, CheckedComboBoxEdit companies )
        {
            InitializeComponent();
            workContract = "";
            workPolicy = "";
            this.dateTimePicker1.Value = date;
            TrustRunDate = date;
            chkCmbCompany = companies;
            trustCompanysText = companies.Text.Trim();
            GoAheadAndRun = true;
            //workDt = dt;
            //workWhat = what;

            SetupTotalsSummary();

            barImport.Hide();

            if (!LoginForm.isReallyRobby)
                txtContract.Hide();

            //LoadMonths();
            //LoadYears();
        }
        /****************************************************************************************/
        private void SetupTotalsSummary()
        {
            AddSummaryColumn("beginningPaymentBalance", null);
            AddSummaryColumn("beginningDeathBenefit", null);
            AddSummaryColumn("endingPaymentBalance", null);
            AddSummaryColumn("endingDeathBenefit", null);
            AddSummaryColumn("downPayments", null);
            AddSummaryColumn("payments", null);
            AddSummaryColumn("growth", null);
            AddSummaryColumn("priorUnappliedCash", null);
            AddSummaryColumn("currentUnappliedCash", null);
            AddSummaryColumn("deathClaimAmount", null);
            AddSummaryColumn("endingBalance", null);
            AddSummaryColumn("overshort", null);
            AddSummaryColumn("tSurrender", null);
            AddSummaryColumn("refund", null);
            AddSummaryColumn("surfacdiff", null);
        }
        /****************************************************************************************/
        private void AddSummaryColumn(string columnName, string format = "")
        {
            if (String.IsNullOrWhiteSpace(format))
                format = "{0:0,0.00}";
            gridMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            gridMain.Columns[columnName].SummaryItem.DisplayFormat = format;
        }
        /***********************************************************************************************/
        private void LoadAvoid ()
        {
            string name = "";
            DataTable dt = new DataTable();
            dt.Columns.Add("avoid");
            DataRow dRow = null;
            bool found = false;
            for ( int i=0; i<gridMain5.Columns.Count; i++)
            {
                if ( !found )
                {
                    if (gridMain5.Columns[i].FieldName != "policyNumber")
                        continue;
                    found = true;
                }
                name = gridMain5.Columns[i].Caption;
                dRow = dt.NewRow();
                dRow["avoid"] = name;
                dt.Rows.Add(dRow);

            }
            cmbAvoid.Properties.DataSource = dt;
        }
        /***********************************************************************************************/
        private void TrustData_Load(object sender, EventArgs e)
        {
            btnSave.Hide();
            lblMonth.Hide();
            lblYear.Hide();
            cmbMonth.Hide();
            cmbYear.Hide();
            chkFilterMismatches.Hide();

            LoadAvoid();

            loadTrustCompanies();

            DateTime now = DateTime.Now;

            int days = DateTime.DaysInMonth(now.Year, now.Month);

            DateTime stop1 = new DateTime(now.Year, now.Month, days - 1);
            this.dateTimePicker1.Value = stop1;

            if (!String.IsNullOrWhiteSpace(workContract))
                LoadContract();
            if (!String.IsNullOrWhiteSpace(workPolicy))
                LoadPolicy();

            string saveName = "TrustData Primary";
            string skinName = "";

            foundLocalPreference = G1.RestoreGridLayout(this, this.dgv, gridMain, LoginForm.username, saveName, ref skinName);
            loadGroupCombo(cmbSelectColumns, "TrustData", "Primary");
            cmbSelectColumns.Text = "Primary";

            foundLocalPreference = G1.RestoreGridLayout(this, this.dgv5, gridMain5, LoginForm.username, saveName, ref skinName);
            loadGroupCombo(comboBox3, "TrustDataDiff", "Primary");
            comboBox3.Text = "Primary";

            saveName = "TrustDataTotals Primary";
            foundLocalPreference = G1.RestoreGridLayout(this, this.dgv2, gridMain2, LoginForm.username, saveName, ref skinName);
            loadGroupCombo(cmbSelectColumns2, "TrustDataTotals", "Primary");
            cmbSelectColumns2.Text = "Primary";

            loading = false;

            if (GoAheadAndRun)
            {
                this.dateTimePicker1.Value = TrustRunDate;
                chkShowAll.Checked = true;
                chkCmbCompany.Text = trustCompanysText;
                btnRun_Click(null, null);
            }
            else if ( workDt != null )
            {
                G1.NumberDataTable(workDt);
                dgv.DataSource = workDt;
            }
        }
        /***********************************************************************************************/
        private void LoadContract()
        {
            string cmd = "Select * from `trust_data` WHERE `contractNumber` = '" + workContract + "';";
            DataTable dx = G1.get_db_data(cmd);
            G1.NumberDataTable(dx);
            //gridMain.Columns["date"].Visible = true;
            dgv.DataSource = dx;
            //gridMain.RefreshData();
            //gridMain.RefreshEditor(true);
            //dgv.Refresh();
        }
        /***********************************************************************************************/
        private void LoadPolicy()
        {
            string cmd = "Select * from `trust_data` WHERE `policyNumber` = '" + workPolicy + "';";
            DataTable dx = G1.get_db_data(cmd);
            G1.NumberDataTable(dx);
            //gridMain.Columns["date"].Visible = true;
            dgv.DataSource = dx;
            //gridMain.RefreshData();
            //gridMain.RefreshEditor(true);
            //dgv.Refresh();
        }
        /***********************************************************************************************/
        private void loadTrustCompanies()
        {
            string cmd = "Select DISTINCT `trustCompany` from `trust_data`;";
            DataTable dt = G1.get_db_data(cmd);

            DataView tempview = dt.DefaultView;
            tempview.Sort = "trustCompany asc";
            dt = tempview.ToTable();

            dt.Rows.Clear();
            DataRow dRow = dt.NewRow();
            dRow["trustCompany"] = "Unity";
            dt.Rows.Add(dRow);

            dRow = dt.NewRow();
            dRow["trustCompany"] = "Unity PB";
            dt.Rows.Add(dRow);

            dRow = dt.NewRow();
            dRow["trustCompany"] = "Security National";
            dt.Rows.Add(dRow);

            dRow = dt.NewRow();
            dRow["trustCompany"] = "FDLIC";
            dt.Rows.Add(dRow);

            dRow = dt.NewRow();
            dRow["trustCompany"] = "FDLIC PB";
            dt.Rows.Add(dRow);

            dRow = dt.NewRow();
            dRow["trustCompany"] = "FORETHOUGHT";
            dt.Rows.Add(dRow);

            dRow = dt.NewRow();
            dRow["trustCompany"] = "CD";
            dt.Rows.Add(dRow);

            tempview = dt.DefaultView;
            tempview.Sort = "trustCompany asc";
            dt = tempview.ToTable();

            chkCmbCompany.Properties.DataSource = dt;

            chkDiffCompanies.Properties.DataSource = dt;
            chkDeathCompanies.Properties.DataSource = dt;
        }
        /***********************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (dgv.Visible)
                G1.SpyGlass(gridMain);
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
                    e.DisplayText = date.ToString("MM/dd/yyyy");
                    if (date.Year < 30)
                        e.DisplayText = "";
                }
            }
        }
        /***********************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            if (dr == null)
                return;
            string contract = dr["contractNumber"].ObjToString();
            if (!String.IsNullOrWhiteSpace(contract))
            {
                this.Cursor = Cursors.WaitCursor;
                string cmd = "Select * from `contracts` where `contractNumber` = '" + contract + "';";
                DataTable dx = G1.get_db_data(cmd);
                if (dx.Rows.Count <= 0)
                {
                    string cnum = contract.TrimStart('0');
                    cnum = cnum.Replace(" ", "");

                    cmd = "Select * from `icustomers` where `payer` = '" + cnum + "';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                        contract = dx.Rows[0]["contractNumber"].ObjToString();
                }
                CustomerDetails clientForm = new CustomerDetails(contract);
                clientForm.Show();
                this.Cursor = Cursors.Default;
            }
        }
        /***********************************************************************************************/
        private int pageMarginLeft = 0;
        private int pageMarginRight = 0;
        private int pageMarginTop = 0;
        private int pageMarginBottom = 0;
        private bool isPrinting = false;
        /***********************************************************************************************/
        private void printPreviewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            isPrinting = true;
            if (this.components == null)
                this.components = new System.ComponentModel.Container();

            DevExpress.XtraPrinting.PrintingSystem printingSystem1 = new DevExpress.XtraPrinting.PrintingSystem(this.components);
            DevExpress.XtraPrinting.PrintableComponentLink printableComponentLink1 = new DevExpress.XtraPrinting.PrintableComponentLink(this.components);

            printingSystem1.Links.AddRange(new object[] {
            printableComponentLink1});


            printableComponentLink1.Component = dgv;
            if (dgv2.Visible)
                printableComponentLink1.Component = dgv2;
            else if (dgv3.Visible)
                printableComponentLink1.Component = dgv3;
            else if (dgv4.Visible)
                printableComponentLink1.Component = dgv4;
            else if (dgv5.Visible)
                printableComponentLink1.Component = dgv5;
            else if (dgv6.Visible)
                printableComponentLink1.Component = dgv6;


            printableComponentLink1.PrintingSystemBase = printingSystem1;

            printableComponentLink1.EnablePageDialog = true;

            printableComponentLink1.CreateDetailHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateDetailHeaderArea);
            printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);
            printableComponentLink1.BeforeCreateAreas += new System.EventHandler(this.printableComponentLink1_BeforeCreateAreas);
            printableComponentLink1.AfterCreateAreas += new System.EventHandler(this.printableComponentLink1_AfterCreateAreas);
            printableComponentLink1.Landscape = true;

            Printer.setupPrinterMargins(50, 100, 80, 50);

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
            isPrinting = false;
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

            font = new Font("Ariel", 12);
            string text = this.Text;
            if (dgv2.Visible)
            {
                text = "Trust Data Combined (";
                text += cmbPreOrPost.Text.Trim() + ")";
            }
            else if (dgv3.Visible)
            {
                text = "Trust Data Combined (";
                text += cmbPreOrPost.Text.Trim() + ")";
            }
            else if (dgv4.Visible)
            {
                text = "Unity Details (";
                text += cmbPreOrPost.Text.Trim() + ")";
            }
            else if (dgv5.Visible)
            {
                text = "Trust Dfferences (";
                text += cmbPreOrPost.Text.Trim() + ")";
            }
            else if (dgv6.Visible)
            {
                text = "Death Benefit Diff (";
                text += cmbPreOrPost.Text.Trim() + ")";
            }
            Printer.DrawQuad(5, 7, 5, 4, text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);

            font = new Font("Ariel", 10, FontStyle.Bold);

            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /****************************************************************************************/
        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            isPrinting = true;
            if (this.components == null)
                this.components = new System.ComponentModel.Container();

            DevExpress.XtraPrinting.PrintingSystem printingSystem1 = new DevExpress.XtraPrinting.PrintingSystem(this.components);
            DevExpress.XtraPrinting.PrintableComponentLink printableComponentLink1 = new DevExpress.XtraPrinting.PrintableComponentLink(this.components);

            printingSystem1.Links.AddRange(new object[] {
            printableComponentLink1});


            printableComponentLink1.Component = dgv;
            if (dgv2.Visible)
                printableComponentLink1.Component = dgv2;
            else if (dgv3.Visible)
                printableComponentLink1.Component = dgv3;

            printableComponentLink1.PrintingSystemBase = printingSystem1;

            printableComponentLink1.EnablePageDialog = true;

            printableComponentLink1.CreateDetailHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateDetailHeaderArea);
            printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);
            printableComponentLink1.BeforeCreateAreas += new System.EventHandler(this.printableComponentLink1_BeforeCreateAreas);
            printableComponentLink1.AfterCreateAreas += new System.EventHandler(this.printableComponentLink1_AfterCreateAreas);
            printableComponentLink1.Landscape = true;

            Printer.setupPrinterMargins(50, 100, 80, 50);

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
            isPrinting = false;
        }
        /***********************************************************************************************/
        private void gridMain_CellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            DataTable dt = (DataTable)dgv.DataSource;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            if (G1.get_column_number(dt, "mod") < 0)
                dt.Columns.Add("mod");
            dt.Rows[row]["mod"] = "Y";
            btnSave.Show();
            btnSave.Refresh();
        }
        /***********************************************************************************************/
        private void gridMain_CustomDrawCell(object sender, RowCellCustomDrawEventArgs e)
        {
            if (e.RowHandle < 0)
                return;
            if (e.Column.FieldName.Trim().ToUpper() == "ENDINGBALANCE")
            {
                int row = gridMain.GetDataSourceRowIndex(e.RowHandle);
                DataTable dt = (DataTable)dgv.DataSource;
                if (G1.get_column_number(dt, "endingBalance") >= 0)
                {
                    double endingBalance = dt.Rows[row]["endingBalance"].ObjToDouble();
                    double surrender = dt.Rows[row]["beginningDeathBenefit"].ObjToDouble();
                    surrender = dt.Rows[row]["tSurrender"].ObjToDouble();
                    if (surrender != endingBalance)
                    {
                        e.Appearance.BackColor = System.Drawing.Color.Red;
                        e.Appearance.ForeColor = System.Drawing.Color.Yellow;
                    }
                }
            }
            else if (e.Column.FieldName.ToUpper() == "NUM")
            {
                if (e.RowHandle >= 0)
                {
                    string num = (e.RowHandle + 1).ToString();
                    e.DisplayText = num;
                }
            }
        }
        /***********************************************************************************************/
        private void btnSave_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataTable dx = null;
            string record = "";
            string cmd = "";

            string company = workWhat;
            string trustName = "";
            string policyNumber = "";

            string contractNumber = "";
            double premium = 0D;
            double surrender = 0D;
            double faceAmount = 0D;
            double deathBenefit = 0D;
            double downPayments = 0D;
            double payments = 0D;
            double growth = 0D;
            string preOrPost = "";

            string insuredName = "";
            string lastName = "";
            string firstName = "";
            string middleName = "";
            string trustCompany = "";

            DataTable myDt = new DataTable();
            string cName = "";
            string type = "";
            string mod = "";
            DataRow dRow = null;
            DateTime date = DateTime.Now;

            if (G1.get_column_number(dt, "mod") < 0)
                dt.Columns.Add("mod");

            try
            {

                this.Cursor = Cursors.WaitCursor;

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    mod = dt.Rows[i]["mod"].ObjToString();
                    if (mod.ToUpper() != "Y")
                        continue;
                    date = dt.Rows[i]["date"].ObjToDateTime();

                    trustName = dt.Rows[i]["trustName"].ObjToString();
                    trustCompany = dt.Rows[i]["trustCompany"].ObjToString();
                    policyNumber = dt.Rows[i]["policyNumber"].ObjToString();
                    preOrPost = dt.Rows[i]["preOrPost"].ObjToString();

                    record = dt.Rows[i]["record"].ObjToString();
                    if (String.IsNullOrWhiteSpace(record))
                        record = G1.create_record("trust_data", "status", "-1");
                    if (G1.BadRecord("trust_data", record))
                        break;

                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                    premium = dt.Rows[i]["beginningPaymentBalance"].ObjToDouble();
                    surrender = dt.Rows[i]["beginningDeathBenefit"].ObjToDouble();
                    faceAmount = dt.Rows[i]["endingPaymentBalance"].ObjToDouble();
                    deathBenefit = dt.Rows[i]["endingDeathBenefit"].ObjToDouble();

                    downPayments = dt.Rows[i]["downPayments"].ObjToDouble();
                    payments = dt.Rows[i]["payments"].ObjToDouble();

                    //growth = deathBenefit - surrender - payments;

                    growth = dt.Rows[i]["growth"].ObjToDouble();

                    insuredName = dt.Rows[i]["insuredName"].ObjToString();
                    firstName = dt.Rows[i]["firstName"].ObjToString();
                    lastName = dt.Rows[i]["lastName"].ObjToString();
                    middleName = dt.Rows[i]["middleName"].ObjToString();

                    G1.update_db_table("trust_data", "record", record, new string[] { "status", "", "trustCompany", trustCompany, "preOrPost", preOrPost, "date", date.ToString("yyyyMMdd"), "trustName", trustName, "contractNumber", contractNumber, "policyNumber", policyNumber, "beginningPaymentBalance", premium.ToString(), "beginningDeathBenefit", surrender.ToString(), "endingPaymentBalance", faceAmount.ToString(), "endingDeathBenefit", deathBenefit.ToString(), "insuredName", insuredName, "lastName", lastName, "firstName", firstName, "middleName", middleName, "downPayments", downPayments.ToString(), "payments", payments.ToString(), "growth", growth.ToString() });

                }
                this.Cursor = Cursors.Default;
            }
            catch (Exception ex)
            {
            }
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void CleanupCommas(DataTable dt, string column)
        {
            string str = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                str = dt.Rows[i][column].ObjToString();
                if (str.IndexOf("$") >= 0)
                {
                    str = str.Replace("$", "");
                    dt.Rows[i][column] = str;
                }
                if (String.IsNullOrWhiteSpace(str))
                    dt.Rows[i][column] = "0";
                else if (str.IndexOf(",") > 0)
                {
                    str = str.Replace(",", "");
                    dt.Rows[i][column] = str;
                }
            }
        }
        /***********************************************************************************************/
        private bool SaveData(DataTable dt, DateTime saveDate)
        {
            this.Cursor = Cursors.WaitCursor;

            DataTable saveDt = dt.Copy();

            if (G1.get_column_number(saveDt, "date") < 0)
                saveDt.Columns.Add("date");
            if (G1.get_column_number(saveDt, "trustCompany") < 0)
                saveDt.Columns.Add("trustCompany");

            if (G1.get_column_number(saveDt, "tmstamp") >= 0)
                saveDt.Columns.Remove("tmstamp");
            if (G1.get_column_number(saveDt, "record") >= 0)
                saveDt.Columns.Remove("record");
            if (G1.get_column_number(saveDt, "num") >= 0)
                saveDt.Columns.Remove("num");

            DataColumn Col = saveDt.Columns.Add("record", System.Type.GetType("System.String"));
            Col.SetOrdinal(0);// to put the column in position 0;
            DataColumn Col1 = saveDt.Columns.Add("tmstamp", System.Type.GetType("System.String"));
            Col1.SetOrdinal(0);// to put the column in position 0;

            double dValue = 0D;
            string str = "";

            for (int i = 0; i < saveDt.Rows.Count; i++)
            {
                //saveDt.Rows[i]["tmstamp"] = "0000-00-00";
                saveDt.Rows[i]["tmstamp"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                saveDt.Rows[i]["record"] = "0";
                saveDt.Rows[i]["insuredName"] = G1.try_protect_data(saveDt.Rows[i]["insuredName"].ObjToString());
                saveDt.Rows[i]["firstName"] = G1.try_protect_data(saveDt.Rows[i]["firstName"].ObjToString());
                saveDt.Rows[i]["middleName"] = G1.try_protect_data(saveDt.Rows[i]["middleName"].ObjToString());
                saveDt.Rows[i]["lastName"] = G1.try_protect_data(saveDt.Rows[i]["lastName"].ObjToString());
                saveDt.Rows[i]["firstName"] = G1.Truncate(saveDt.Rows[i]["firstName"].ObjToString(), 80);
                saveDt.Rows[i]["middleName"] = G1.Truncate(saveDt.Rows[i]["middleName"].ObjToString(), 80);
                saveDt.Rows[i]["lastName"] = G1.Truncate(saveDt.Rows[i]["lastName"].ObjToString(), 80);

                saveDt.Rows[i]["trustCompany"] = workWhat;
                saveDt.Rows[i]["date"] = saveDate.ToString("yyyyMMdd");
            }

            DateTime date = saveDate;

            DeletePreviousData(saveDate, workWhat);

            string strFile = "/TrustData/TrustData_P_" + date.ToString("yyyyMMdd") + ".csv";
            string Server = "C:/rag";
            //Create directory if not exist... Make sure directory has required rights..
            if (!Directory.Exists(Server + "/TrustData/"))
                Directory.CreateDirectory(Server + "/TrustData/");

            //If file does not exist then create it and right data into it..
            if (!File.Exists(Server + strFile))
            {
                FileStream fs = new FileStream(Server + strFile, FileMode.Create, FileAccess.Write);
                fs.Close();
                fs.Dispose();
            }

            //Generate csv file from where data read

            try
            {
                //DateTime saveDate = this.dateTimePicker2.Value;
                int days = DateTime.DaysInMonth(saveDate.Year, saveDate.Month);
                //                string mySaveDate = saveDate.Year.ToString("D4") + "-" + saveDate.Month.ToString("D2") + "-" + days.ToString("D2") + " 00:00:00";

                var mySaveDate = G1.DTtoMySQLDT(saveDate);

                //for ( int i=0; i<saveDt.Rows.Count; i++)
                //    saveDt.Rows[i]["payDate8"] = mySaveDate;

                MySQL.CreateCSVfile(saveDt, Server + strFile, false, "~");
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Creating CSV File to load into Database " + ex.Message.ToString());
                this.Cursor = Cursors.Default;
                return false;
            }
            try
            {
                Structures.TieDbTable("trust_data", saveDt);
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Tieing trust_data to DataTable " + ex.Message.ToString());
                this.Cursor = Cursors.Default;
                return false;
            }
            try
            {
                G1.conn1.Open();
                MySqlBulkLoader bcp1 = new MySqlBulkLoader(G1.conn1);
                bcp1.TableName = "trust_data"; //Create table into MYSQL database...
                bcp1.FieldTerminator = ",";

                bcp1.LineTerminator = "\r\n";
                bcp1.FileName = Server + strFile;
                bcp1.NumberOfLinesToSkip = 0;
                bcp1.FieldTerminator = "~";
                bcp1.Load();
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Bulk Loading trust_data to DataTable " + ex.Message.ToString());
            }

            saveDt.Dispose();
            saveDt = null;

            //            File.Delete(Server + strFile);

            this.Cursor = Cursors.Default;
            return true;
        }
        /***********************************************************************************************/
        private void DeletePreviousData(DateTime saveDate, string trustCompany)
        {
            string date1 = saveDate.ToString("yyyy-MM-dd");

            string cmd = "DELETE from `trust_data` where `date` = '" + date1 + "' AND `trustCompany` = '" + trustCompany + "' ";
            cmd += ";";
            try
            {
                G1.get_db_data(cmd);
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Delete Previous Data " + ex.Message.ToString());
            }
        }
        /***********************************************************************************************/
        private void btnLeft_Click(object sender, EventArgs e)
        {
            DateTime now = this.dateTimePicker1.Value;
            now = now.AddMonths(-1);
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            this.dateTimePicker1.Value = new DateTime(now.Year, now.Month, days);
        }
        /***********************************************************************************************/
        private void btnRight_Click(object sender, EventArgs e)
        {
            DateTime now = this.dateTimePicker1.Value;
            now = now.AddMonths(1);
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            this.dateTimePicker1.Value = new DateTime(now.Year, now.Month, days);
        }
        /*******************************************************************************************/
        private string getCompanyQuery( CheckedComboBoxEdit chkCompany = null )
        {
            string procLoc = "";
            string company = "";
            string[] locIDs = chkCmbCompany.EditValue.ToString().Split('|');
            if ( chkCompany != null )
                locIDs = chkCompany.EditValue.ToString().Split('|');
            for (int i = 0; i < locIDs.Length; i++)
            {
                if (!String.IsNullOrWhiteSpace(locIDs[i]))
                {
                    company = locIDs[i].Trim();
                    if (company == "Unity Barham" || company == "Unity Webb" )
                        continue;
                    if (procLoc.Trim().Length > 0)
                        procLoc += ",";
                    procLoc += "'" + company + "'";
                }
            }
            return procLoc.Length > 0 ? " `trustCompany` IN (" + procLoc + ") " : "";
        }
        /***********************************************************************************************/
        private DataTable unityActive = null;
        private DataTable unityActivePB = null;
        private void btnRun_Click(object sender, EventArgs e)
        {
            DateTime date = this.dateTimePicker1.Value;
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            DateTime newDate = new DateTime(date.Year, date.Month, days);
            string date1 = newDate.ToString("yyyy-MM-dd");

            barImport.Hide();

            string companies = getCompanyQuery();

            string cmd = "Select * from `trust_data` t LEFT JOIN fcust_extended f ON t.`contractNumber` = f.`contractNumber` LEFT JOIN customers x on t.`contractNumber` = x.`contractNumber` LEFT JOIN contracts c ON t.`contractNumber` = c.`contractNumber` WHERE `date` = '" + date1 + "' AND f.`Funeral Director` <> '' ";
            cmd = "Select * from `trust_data` t LEFT JOIN customers x on t.`contractNumber` = x.`contractNumber` LEFT JOIN contracts c ON t.`contractNumber` = c.`contractNumber` WHERE `date` = '" + date1 + "' ";
            cmd = "Select * from `trust_data` t WHERE `date` = '" + date1 + "' ";
            //string cmd = "Select * from `trust_data` WHERE `date` = '" + date1 + "' ";
            if (!String.IsNullOrWhiteSpace(companies))
                cmd += " AND " + companies + " ";

            string contract = txtContract1.Text.Trim();
            if (!String.IsNullOrWhiteSpace(contract))
            {
                if ( chkBoxShowAllContractRecord.Checked )
                    cmd = "Select * from `trust_data` t WHERE `contractNumber` = '" + contract + "' ORDER by 'date' ";
                else
                    cmd += " AND `contractNumber` = '" + contract + "' ";
            }


            cmd += ";";

            if (String.IsNullOrWhiteSpace(companies))
                tabPage1.Text = "All Companies";
            else
                tabPage1.Text = chkCmbCompany.Text.Trim();

            DataTable dt = G1.get_db_data(cmd);

            Trust85.FindContract(dt, "WM05008");

            dt.Columns.Add("mark");
            SetupSelection(dt, this.repositoryItemCheckEdit1, "mark");
            dt.Columns.Add("surfacdiff", Type.GetType("System.Double"));

            string company = "";
            string policy = "";
            string contractNumber = "";
            DataTable newDt = dt.Clone();
            DataRow[] dRows = null;
            DataTable dx = null;

            dRows = dt.Select("contractNumber=''");
            if (dRows.Length > 0)
            {
                for (int i = 0; i < dRows.Length; i++)
                {
                    policy = dRows[i]["policyNumber"].ObjToString().Trim();
                    if ( policy == "SM1533901")
                    {
                    }
                    if (!String.IsNullOrWhiteSpace(policy))
                    {
                        contractNumber = findContractNumber(policy);
                        if (String.IsNullOrWhiteSpace(contractNumber))
                            dRows[i]["contractNumber"] = contractNumber;
                    }
                }
            }

            dt = LookupTrusts(dt);

            dt = LookupRefunds(dt);

            //dt = pullUnityActive(dt);


            Trust85.FindContract(dt, "M04095");
            Trust85.FindContract(dt, "L14140UI");


            if (!chkShowAll.Checked)
            {

                string[] locIDs = chkCmbCompany.EditValue.ToString().Split('|');
                try
                {
                    for (int i = 0; i < locIDs.Length; i++)
                    {
                        if (String.IsNullOrWhiteSpace(locIDs[i]))
                            continue;
                        company = locIDs[i].Trim();
                        if (company.ToUpper() == "UNITY")
                        {
                            Trust85.FindContract(dt, "L14140UI");
                            dx = pullUnityActive(dt);
                            Trust85.FindContract(dx, "L14140UI");
                            newDt.Merge(dx);

                            if (!chkShowAll.Checked)
                            {
                                dx = pullUnityLapsed(dt);
                                Trust85.FindContract(dx, "L14140UI");
                                newDt.Merge(dx);

                                dx = pullUnityLapsedQuestioned(dt);
                                Trust85.FindContract(dx, "L14140UI");
                                newDt.Merge(dx);

                                unityActive = newDt.Copy();

                                dx = pullUnityPB(dt);
                                Trust85.FindContract(dx, "L14140UI");
                                unityActivePB = dx.Copy();
                                newDt.Merge(dx);
                            }
                        }
                        else if (company.ToUpper() == "SECURITY NATIONAL")
                        {
                            dx = pullSecurityNationalActive(dt);
                            newDt.Merge(dx);
                        }
                        else if (company.ToUpper() == "FDLIC")
                        {
                            //dx = pullFDLICActive(dt);
                            dx = pullFDLICAll(dt);
                            newDt.Merge(dx);
                        }
                        else if (company.ToUpper() == "FDLIC PB")
                        {
                            dx = pullFDLIC_PBActive(dt);
                            newDt.Merge(dx);
                        }
                        else if (company.ToUpper() == "FORETHOUGHT")
                        {
                            dx = pullFORETHOUGHT(dt);
                            newDt.Merge(dx);
                        }
                        else if (company.ToUpper() == "CD")
                        {
                            dx = pullCD(dt);
                            newDt.Merge(dx);
                        }
                    }
                }
                catch (Exception ex)
                {
                }
            }
            else
                newDt = dt.Copy();

            string trustCompany = "";
            double downPayment = 0D;
            double beginningPaymentBalance = 0D;
            double beginningDeathBenefit = 0D;
            double endingDeathBenefit = 0D;
            double endingPaymentBalance = 0D;
            double deathClaim = 0D;
            double premium = 0D;
            double diff = 0D;
            string prePost = "";

            for (int i = 0; i < newDt.Rows.Count; i++)
            {
                try
                {
                    contractNumber = newDt.Rows[i]["contractNumber"].ObjToString();
                    if (contractNumber.ToUpper() == "L14140UI")
                    {
                    }
                    prePost = newDt.Rows[i]["preOrPost"].ObjToString();
                    downPayment = newDt.Rows[i]["downPayments"].ObjToDouble();
                    trustCompany = newDt.Rows[i]["trustCompany"].ObjToString().ToUpper();
                    beginningDeathBenefit = newDt.Rows[i]["beginningDeathBenefit"].ObjToDouble();
                    endingDeathBenefit = newDt.Rows[i]["endingDeathBenefit"].ObjToDouble();
                    deathClaim = newDt.Rows[i]["beginningPaymentBalance"].ObjToDouble();
                    beginningPaymentBalance = deathClaim;
                    endingPaymentBalance = newDt.Rows[i]["endingPaymentBalance"].ObjToDouble();
                    premium = deathClaim;
                    deathClaim = newDt.Rows[i]["deathClaimAmount"].ObjToDouble();
                    if (trustCompany.ToUpper().IndexOf("FDLIC") >= 0) // xyzzy FDLIC
                    {
                        if (beginningDeathBenefit <= 0D && downPayment > 0D && endingDeathBenefit > 0D)
                        {
                            deathClaim = endingDeathBenefit;
                            //newDt.Rows[i]["beginningDeathBenefit"] = endingDeathBenefit;
                        }
                        else
                        {
                            //newDt.Rows[i]["beginningDeathBenefit"] = endingPaymentBalance;
                        }
                    }
                    else if (trustCompany.ToUpper().IndexOf("SECURITY NAT") >= 0)
                    {
                        newDt.Rows[i]["beginningDeathBenefit"] = beginningPaymentBalance;
                    }
                    diff = beginningPaymentBalance - endingPaymentBalance;
                    beginningDeathBenefit = newDt.Rows[i]["beginningDeathBenefit"].ObjToDouble();
                    diff = beginningDeathBenefit - endingPaymentBalance;
                    newDt.Rows[i]["surfacdiff"] = diff;
                }
                catch (Exception ex)
                {
                }
            }

            //DataTable unityDt = processUnityData(dt);

            //string contract = txtContract.Text.Trim();
            //if (!String.IsNullOrWhiteSpace(contract))
            //{
            //    dRows = newDt.Select("contractNumber='" + contract + "'");
            //    if (dRows.Length == 0)
            //        newDt.Rows.Clear();
            //    else
            //        newDt = dRows.CopyToDataTable();
            //}


            if (!chkBoxShowAllContractRecord.Checked)
            {
                DataView tempview = newDt.DefaultView;
                tempview.Sort = "policyNumber ASC";
                newDt = tempview.ToTable();
            }


            Trust85.FindContract(newDt, "L14140UI");

            G1.NumberDataTable(newDt);
            dgv.DataSource = newDt;

            if (chkTBB.Checked)
                LoadTBB();

            DataTable ddx = (DataTable)dgv.DataSource;
            Trust85.FindContract(ddx, "L14140UI");
        }
        /***********************************************************************************************/
        private DataTable fixTheData(DataTable dt, bool outEarly = false )
        {
            string company = "";
            string policy = "";
            string contractNumber = "";
            DataTable newDt = dt.Clone();
            DataRow[] dRows = null;
            DataTable dx = null;

            try
            {
                dRows = dt.Select("contractNumber=''");
                if (dRows.Length > 0)
                {
                    for (int i = 0; i < dRows.Length; i++)
                    {
                        policy = dRows[i]["policyNumber"].ObjToString().Trim();
                        if (!String.IsNullOrWhiteSpace(policy))
                        {
                            contractNumber = findContractNumber(policy);
                            if (String.IsNullOrWhiteSpace(contractNumber))
                                dRows[i]["contractNumber"] = contractNumber;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
            }

            try
            {
                dt = LookupTrusts(dt);

                dt = LookupRefunds(dt);
            }
            catch (Exception ex)
            {
            }

            if (outEarly)
                return dt;
            try
            {
                if (chkOldStuff.Checked)
                {
                    dx = pullUnityWebb(dt);
                    newDt.Merge(dx);

                    dx = pullUnityBarham(dt);
                    newDt.Merge(dx);

                    dx = pullOldWebb(dt);
                    newDt.Merge(dx);

                    dx = pullOldCCI(dt);
                    newDt.Merge(dx);
                }
                else
                {
                    dx = pullUnityActive(dt);
                    unityActive = dx.Copy();
                    newDt.Merge(dx);


                    dx = pullUnityLapsed(dt);
                    newDt.Merge(dx);

                    dx = pullUnityLapsedQuestioned(dt);
                    newDt.Merge(dx);

                    //unityActive = newDt.Copy();

                    dx = pullUnityPB(dt);
                    unityActivePB = dx.Copy();
                    newDt.Merge(dx);


                    dx = pullUnityDeceased(dt);
                    newDt.Merge(dx);
                }

                dx = pullSecurityNationalActive(dt);
                newDt.Merge(dx);

                dx = pullFDLICActive(dt);
                newDt.Merge(dx);
                dx = pullFDLIC_PBActive(dt);
                newDt.Merge(dx);
                dx = pullFORETHOUGHT(dt);
                newDt.Merge(dx);
                dx = pullCD(dt);
                newDt.Merge(dx);
            }
            catch (Exception ex)
            {
            }

            string trustCompany = "";
            double downPayment = 0D;
            double beginningPaymentBalance = 0D;
            double beginningDeathBenefit = 0D;
            double endingDeathBenefit = 0D;
            double endingPaymentBalance = 0D;
            double deathClaim = 0D;
            double premium = 0D;
            string prePost = "";
            string statusReason = "";

            for (int i = 0; i < newDt.Rows.Count; i++)
            {
                try
                {
                    contractNumber = newDt.Rows[i]["contractNumber"].ObjToString();
                    if (contractNumber.ToUpper() == "WM13005U")
                    {
                    }
                    statusReason = newDt.Rows[i]["statusReason"].ObjToString();
                    prePost = newDt.Rows[i]["preOrPost"].ObjToString();
                    downPayment = newDt.Rows[i]["downPayments"].ObjToDouble();
                    trustCompany = newDt.Rows[i]["trustCompany"].ObjToString().ToUpper();
                    beginningDeathBenefit = newDt.Rows[i]["beginningDeathBenefit"].ObjToDouble();
                    endingDeathBenefit = newDt.Rows[i]["endingDeathBenefit"].ObjToDouble();
                    deathClaim = newDt.Rows[i]["beginningPaymentBalance"].ObjToDouble();
                    beginningPaymentBalance = deathClaim;
                    endingPaymentBalance = newDt.Rows[i]["endingPaymentBalance"].ObjToDouble();
                    premium = deathClaim;
                    deathClaim = newDt.Rows[i]["deathClaimAmount"].ObjToDouble();
                    if (trustCompany.ToUpper().IndexOf("UNITY") >= 0)
                    {
                        if (statusReason.ToUpper() == "DC")
                        {
                            newDt.Rows[i]["deathClaimAmount"] = endingPaymentBalance;
                            newDt.Rows[i]["endingPaymentBalance"] = 0D;
                            //newDt.Rows[i]["beginningPaymentBalance"] = 0D;
                        }
                    }
                    if (trustCompany.ToUpper().IndexOf("FDLIC") >= 0)
                    {
                        if (beginningDeathBenefit <= 0D && downPayment > 0D && endingDeathBenefit > 0D)
                        {
                            deathClaim = endingDeathBenefit;
                            newDt.Rows[i]["beginningDeathBenefit"] = endingDeathBenefit;
                        }
                        else
                        {
                            newDt.Rows[i]["beginningDeathBenefit"] = endingPaymentBalance;
                        }
                    }
                    else if (trustCompany.ToUpper().IndexOf("SECURITY NAT") >= 0)
                    {
                        newDt.Rows[i]["beginningDeathBenefit"] = beginningPaymentBalance;
                    }
                }
                catch (Exception ex)
                {
                }
            }

            ////DataTable unityDt = processUnityData(dt);

            string contract = txtContract.Text.Trim();
            if (!String.IsNullOrWhiteSpace(contract))
            {
                dRows = newDt.Select("contractNumber='" + contract + "'");
                if (dRows.Length == 0)
                    newDt.Rows.Clear();
                else
                    newDt = dRows.CopyToDataTable();
            }

            DataView tempview = newDt.DefaultView;
            tempview.Sort = "policyNumber ASC";
            newDt = tempview.ToTable();

            return newDt;
        }
        /***********************************************************************************************/
        private void SetupSelection(DataTable dt, DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew = null, string columnName = "")
        {
            if (selectnew == null)
                selectnew = this.repositoryItemCheckEdit1;
            selectnew.NullText = null;
            selectnew.ValueChecked = "Y";
            selectnew.ValueUnchecked = "N";
            selectnew.ValueGrayed = null;
            if (G1.get_column_number(dt, columnName) < 0)
                dt.Columns.Add(columnName);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (dt.Rows[i][columnName].ObjToString().ToUpper() != "Y")
                    dt.Rows[i][columnName] = "N";
            }
        }
        /***********************************************************************************************/
        private void CheckFile(DataTable dt)
        {
            DataRow[] dRows = dt.Select("contractNumber='C13121UI'");
            if (dRows.Length > 0)
            {
                DataTable ddd = dRows.CopyToDataTable();
            }
        }
        /***********************************************************************************************/
        private DataTable pullUnityActive(DataTable dt)
        {
            Trust85.FindContract(dt, "L14140UI");
            DataTable dx = dt.Clone();
            //dRows = dt.Select("`Death Benefit` <> '0.00' AND `Policy Extract_Policy Status` = 'A' AND `Policy Number` LIKE '77%'");
            //DataRow[] dRows = dt.Select("`trustCompany` = 'Unity' AND `endingDeathBenefit` <> '0.00' AND `policyStatus` = 'A'  AND `policyNumber` LIKE '77%' ");
            DataRow[] dRows = dt.Select("`trustCompany` = 'Unity' AND `beginningDeathBenefit` <> '0.00' AND `policyStatus` = 'A'  AND `policyNumber` LIKE '77%' ");
            if (chkShowAll.Checked)
            {
                dRows = dt.Select("`trustCompany` = 'Unity' AND `policyNumber` LIKE '77%' ");
            }
            if (dRows.Length > 0)
            {
                dx = dRows.CopyToDataTable();
                Trust85.FindContract(dx, "L14140UI");
                dx = SortBy(dx);

                dRows = dt.Select("`tab` = 'Unity Active'");
                if (dRows.Length > 0)
                {
                    DataTable dd = dt.Clone();
                    dd = dRows.CopyToDataTable();
                    dx.Merge(dd);
                    dx = SortBy(dx);
                }
                dRows = dx.Select("contractNumber='C13121UI'");
                if (dRows.Length > 0)
                {
                    DataTable ddd = dRows.CopyToDataTable();
                }
            }
            return dx;
        }
        /***********************************************************************************************/
        private DataTable pullUnityWebb(DataTable dt)
        {
            DataTable dx = dt.Clone();
            try
            {
                string preOrPost = cmbPreOrPost.Text;
                if (preOrPost == "Both" && chkOldStuff.Checked)
                    preOrPost = "Post";
                else if (preOrPost == "Both")
                    preOrPost = "Pre";
                DataRow [] dRows = dt.Select("`endingDeathBenefit` <> '0.00' AND `policyStatus` = 'A'  AND `policyNumber` LIKE 'PSPWT%' AND `preOrPost` = '" + preOrPost + "'");
                if (dRows.Length > 0)
                {
                    dx = dRows.CopyToDataTable();
                    dx = SortBy(dx);
                    dx.Columns.Add("unityOldWebb", Type.GetType("System.Double"));

                    for ( int i=0; i<dx.Rows.Count; i++)
                    {
                        dx.Rows[i]["unityOldWebb"] = dx.Rows[i]["endingPaymentBalance"].ObjToDouble();
                    }
                    G1.NumberDataTable(dx);
                    //unityWebbDt = dx.Copy();
                }
            }
            catch (Exception ex)
            {
            }
            return dx;
        }
        /***********************************************************************************************/
        private DataTable pullUnityBarham(DataTable dt)
        {
            DataTable dx = dt.Clone();

            try
            {
                string preOrPost = cmbPreOrPost.Text;
                if (preOrPost == "Both" && chkOldStuff.Checked)
                    preOrPost = "Post";
                else if (preOrPost == "Both")
                    preOrPost = "Pre";

                DataRow[] dRows = dt.Select("(`endingDeathBenefit` <> '0.00' AND `policyStatus` = 'A'  AND `policyNumber` LIKE 'PSPNB%') OR `policyNumber` = 'PSPNB08002' AND `preOrPost` = '" + preOrPost + "'");
                //dRows = dt.Select("( `Death Benefit` <> '0.00' AND `Policy Extract_Policy Status` = 'A' AND `Policy Number` LIKE 'PSPNB%' ) OR `Policy Number` = 'PSPNB08002'");
                if (dRows.Length > 0)
                {
                    dx = dRows.CopyToDataTable();
                    dx = SortBy(dx);
                    dx.Columns.Add("unityOldBarham", Type.GetType("System.Double"));
                    for (int i = 0; i < dx.Rows.Count; i++)
                    {
                        dx.Rows[i]["unityOldBarham"] = dx.Rows[i]["endingPaymentBalance"].ObjToDouble();
                    }
                    G1.NumberDataTable(dx);
                    //unityBarhamDt = dx.Copy();
                }
            }
            catch (Exception ex)
            {
            }
            return dx;
        }
        /***********************************************************************************************/
        private DataTable pullOldWebb(DataTable dt)
        {
            DataTable dx = dt.Clone();
            try
            {
                string preOrPost = cmbPreOrPost.Text;
                if (preOrPost == "Both" && chkOldStuff.Checked)
                    preOrPost = "Post";
                else if (preOrPost == "Both")
                    preOrPost = "Pre";
                DataRow[] dRows = dt.Select("`trustCompany` = 'FDLIC PB' AND `preOrPost` = 'Pre' ");
                if (dRows.Length > 0)
                {
                    dx = dRows.CopyToDataTable();
                    dx = SortBy(dx);
                    dx.Columns.Add("fdlicOldWebb", Type.GetType("System.Double"));

                    for (int i = 0; i < dx.Rows.Count; i++)
                    {
                        dx.Rows[i]["fdlicOldWebb"] = dx.Rows[i]["endingDeathBenefit"].ObjToDouble();
                    }
                    G1.NumberDataTable(dx);
                    //unityWebbDt = dx.Copy();
                }
            }
            catch (Exception ex)
            {
            }
            return dx;
        }
        /***********************************************************************************************/
        private DataTable pullOldCCI(DataTable dt)
        {
            DataTable dx = dt.Clone();
            try
            {
                string preOrPost = cmbPreOrPost.Text;
                if (preOrPost == "Both" && chkOldStuff.Checked)
                    preOrPost = "Post";
                else if (preOrPost == "Both")
                    preOrPost = "Pre";
                DataRow[] dRows = dt.Select("`trustCompany` = 'FDLIC CCI' AND `preOrPost` = 'Pre' ");
                if (dRows.Length > 0)
                {
                    dx = dRows.CopyToDataTable();
                    dx = SortBy(dx);
                    dx.Columns.Add("fdlicOldCCI", Type.GetType("System.Double"));

                    for (int i = 0; i < dx.Rows.Count; i++)
                    {
                        dx.Rows[i]["fdlicOldCCI"] = dx.Rows[i]["endingDeathBenefit"].ObjToDouble();
                    }
                    G1.NumberDataTable(dx);
                    //unityWebbDt = dx.Copy();
                }
            }
            catch (Exception ex)
            {
            }
            return dx;
        }
        /***********************************************************************************************/
        private DataTable pullUnityDeceased(DataTable dt)
        {
            DataTable dx = dt.Clone();
            //dRows = dt.Select("`Death Benefit` <> '0.00' AND `Policy Extract_Policy Status` = 'A' AND `Policy Number` LIKE '77%'");
            //DataRow[] dRows = dt.Select("`trustCompany` = 'Unity' AND `endingPaymentBalance` = '0.00' AND `statusReason` = 'DC'  AND `policyNumber` LIKE '77%' ");
            DataRow[] dRows = dt.Select("`trustCompany` = 'Unity' AND `statusReason` = 'DC'  AND `policyNumber` LIKE '77%' ");
            if (dRows.Length > 0)
            {
                dx = dRows.CopyToDataTable();
                dx = SortBy(dx);

                dRows = dt.Select("`tab` = 'Unity Active'");
                if (dRows.Length > 0)
                {
                    DataTable dd = dt.Clone();
                    dd = dRows.CopyToDataTable();
                    dx.Merge(dd);
                    dx = SortBy(dx);
                }
                dRows = dx.Select("contractNumber='HT15021U'");
                if (dRows.Length > 0)
                {
                    DataTable ddd = dRows.CopyToDataTable();
                }
            }
            return dx;
        }
        /***********************************************************************************************/
        private DataTable pullUnityLapsed(DataTable dt)
        {
            DataTable dx = dt.Clone();
            //dRows = dt.Select("`Death Benefit` = '0.00' AND `Policy Extract_Policy Status` = 'T' AND `Policy Number` LIKE '77%' AND `Policy Extract_Status Reason` IN ('LP','NI','NN','NT','SR')");

            DataRow[] dRows = dt.Select("`trustCompany` = 'Unity' AND `endingDeathBenefit` = '0.00' AND `policyStatus` = 'T'  AND `policyNumber` LIKE '77%' AND `statusReason` IN ('LP','NI','NN','NT','SR')");
            //DataRow[] dRows = dt.Select("`trustCompany` = 'Unity' AND `endingDeathBenefit` = '0.00' AND `policyStatus` = 'T'  AND `policyNumber` LIKE '77%' ");
            if (dRows.Length > 0)
            {
                dx = dRows.CopyToDataTable();
                dx = SortBy(dx);

                DataView tempview = dx.DefaultView;
                tempview.Sort = "lastName, firstName";
                DataTable tempdx = tempview.ToTable();


                CheckFile(dx);


            }
            dRows = dt.Select("`tab` = 'Unity Lapsed'");
            if (dRows.Length > 0)
            {
                DataTable dd = dt.Clone();
                dd = dRows.CopyToDataTable();
                dx.Merge(dd);
                dx = SortBy(dx);
            }
            return dx;
        }
        /***********************************************************************************************/
        private DataTable pullUnityLapsedQuestioned(DataTable dt)
        {
            DataTable dx = dt.Clone();
            DataRow[] dRows = dt.Select("`trustCompany` = 'Unity' AND `endingDeathBenefit` = '0.00' AND `policyStatus` = 'S'  AND `policyNumber` LIKE '77%' AND `statusReason` ='AN'");
            if (dRows.Length > 0)
                dx = dRows.CopyToDataTable();
            dRows = dt.Select("`trustCompany` = 'Unity' AND `endingDeathBenefit` > '0.00' AND `policyStatus` = 'S'  AND `policyNumber` LIKE '77%' AND `statusReason` ='DP'");
            if (dRows.Length > 0)
                dx.Merge(dRows.CopyToDataTable());
            Trust85.FindContract(dx, "L14140UI");
            return dx;
        }
        /***********************************************************************************************/
        //private DataTable pullUnityLapsedQuestioned(DataTable dt)
        //{
        //    DataTable dx = dt.Clone();

        //    //dRows = dt.Select("`Death Benefit` = '0.00' AND `Policy Extract_Policy Status` = 'S' AND `Policy Number` LIKE '77%' AND `Policy Extract_Status Reason` = 'AN'");


        //    DataRow[] dRows = dt.Select("`trustCompany` = 'Unity' AND `endingDeathBenefit` = '0.00' AND `policyStatus` = 'S'  AND `policyNumber` LIKE '77%' AND `statusReason` ='AN'");
        //    if (dRows.Length > 0)
        //    {
        //        dx = dRows.CopyToDataTable();
        //        dx = SortBy(dx);
        //    }
        //    return dx;
        //}
        /***********************************************************************************************/
        private DataTable pullUnityDI(DataTable dt)
        {
            DataTable dx = dt.Clone();
            DataRow[] dRows = dt.Select("`policyNumber` LIKE 'PB%'");
            if (dRows.Length > 0)
            {
                dx = dRows.CopyToDataTable();
                dx = SortBy(dx);
            }
            return dx;
        }
        /***********************************************************************************************/
        private DataTable pullUnityPB(DataTable dt)
        {
            DataTable dx = dt.Clone();
            //dRows = dt.Select("`Death Benefit` <> '0.00' AND `policyStatus` = 'A' AND `PolicyNumber` LIKE 'P%' AND `Policy Number` NOT LIKE 'PB%' AND `Policy Number` NOT LIKE 'PSPNB%' AND `Policy Number` NOT LIKE 'PSPWT%'");

            //DataRow[] dRows = dt.Select("`endingDeathBenefit` <> '0.00' AND `policyStatus` = 'A'  AND `policyNumber` LIKE 'P%' AND `policyNumber` NOT LIKE 'PB%' AND `policyNumber` NOT LIKE 'PSPNB%' AND `policyNumber` NOT LIKE 'PSPWT%' ");
            DataRow[] dRows = dt.Select("`trustCompany` = 'Unity PB' AND `endingDeathBenefit` <> '0.00' AND `policyStatus` = 'A' AND `policyNumber` LIKE 'P%' AND `policyNumber` NOT LIKE 'PB%' AND `policyNumber` NOT LIKE 'PSPNB%' AND `policyNumber` NOT LIKE 'PSPWT%'");

            //DataRow[] dRows = dt.Select("`endingDeathBenefit` <> '0.00' AND `policyNumber` LIKE 'P%' ");
            if (dRows.Length > 0)
            {
                dx = dRows.CopyToDataTable();
                double dValue = 0D;
                for (int i = 0; i < dx.Rows.Count; i++)
                {
                    dValue = dx.Rows[i]["endingDeathBenefit"].ObjToDouble();
                    dx.Rows[i]["beginningDeathBenefit"] = dValue;
                }
                dx = SortBy(dx);
            }
            return dx;
        }
        /***********************************************************************************************/
        private DataTable pullUnityPB_Deceased(DataTable dt)
        {
            DataTable dx = dt.Clone();
            //dRows = dt.Select("`Death Benefit` <> '0.00' AND `policyStatus` = 'A' AND `PolicyNumber` LIKE 'P%' AND `Policy Number` NOT LIKE 'PB%' AND `Policy Number` NOT LIKE 'PSPNB%' AND `Policy Number` NOT LIKE 'PSPWT%'");

            //DataRow[] dRows = dt.Select("`endingDeathBenefit` <> '0.00' AND `policyStatus` = 'A'  AND `policyNumber` LIKE 'P%' AND `policyNumber` NOT LIKE 'PB%' AND `policyNumber` NOT LIKE 'PSPNB%' AND `policyNumber` NOT LIKE 'PSPWT%' ");
            DataRow[] dRows = dt.Select("`trustCompany` = 'Unity PB' AND `statusReason` = 'DC' AND `policyNumber` LIKE 'P%' AND `policyNumber` NOT LIKE 'PB%' AND `policyNumber` NOT LIKE 'PSPNB%' AND `policyNumber` NOT LIKE 'PSPWT%'");

            //DataRow[] dRows = dt.Select("`trustCompany` = 'Unity PB'");
            if (dRows.Length > 0)
            {
                dx = dRows.CopyToDataTable();
                double dValue = 0D;
                for (int i = 0; i < dx.Rows.Count; i++)
                {
                    dValue = dx.Rows[i]["endingPaymentBalance"].ObjToDouble();
                    dx.Rows[i]["beginningDeathBenefit"] = dValue;
                }
                dx = SortBy(dx);
            }
            return dx;
        }
        /***********************************************************************************************/
        private DataTable pullUnityDeathBenefit(DataTable dt)
        {
            DataTable dx = dt.Clone();
            //dRows = dt.Select("`Death Benefit` <> '0.00' AND `Policy Extract_Policy Status` = 'A' AND `Policy Number` LIKE '77%'");
            DataRow[] dRows = dt.Select("`endingDeathBenefit` <> '0.00' ");
            if (dRows.Length > 0)
            {
                dx = dRows.CopyToDataTable();
                dx = SortBy(dx);
                for ( int i=0; i<dx.Rows.Count; i++)
                {
                    dx.Rows[i]["beginningDeathBenefit"] = dx.Rows[i]["endingDeathBenefit"].ObjToDouble();
                }
            }
            return dx;
        }
        /***********************************************************************************************/
        private DataTable pullSecurityNationalActive(DataTable dt)
        {
            DataTable dx = dt.Clone();
            //dRows = dt.Select("`Death Benefit` <> '0.00' AND `Policy Extract_Policy Status` = 'A' AND `Policy Number` LIKE '77%'");

            string preOrPost = cmbPreOrPost.Text;
            if (preOrPost == "Both" && chkOldStuff.Checked)
                preOrPost = "Pre";
            else if (preOrPost == "Both")
                preOrPost = "Pre";

            DataRow[] dRows = dt.Select("`trustCompany` = 'Security National' AND `endingDeathBenefit` <> '0.00'  AND `preOrPost` = '" + preOrPost + "'");
            if ( chkShowAll.Checked )
                dRows = dt.Select("`trustCompany` = 'Security National'");
            if (dRows.Length > 0)
            {
                dx = dRows.CopyToDataTable();
                dx = SortBy(dx);
            }

            Trust85.FindContract(dx, "WM05008");
            if ( chkShowPaidOut.Checked )
            {
                DateTime date1 = this.dateTimePicker1.Value;
                string companies = getCompanyQuery();

                string cmd = "Select * from `trust_data` t LEFT JOIN fcust_extended f ON t.`contractNumber` = f.`contractNumber` LEFT JOIN customers x on t.`contractNumber` = x.`contractNumber` LEFT JOIN contracts c ON t.`contractNumber` = c.`contractNumber` WHERE ";
                if (!String.IsNullOrWhiteSpace(companies))
                    cmd += companies + " ";
                DateTime startDate = new DateTime(date1.Year, date1.Month, 1);
                int days = DateTime.DaysInMonth(startDate.Year, startDate.Month);
                DateTime stopDate = new DateTime(date1.Year, date1.Month, days);

                cmd += " AND (`deathPaidDate` >= '" + startDate.ToString("yyyy-MM-dd") + "' AND `deathPaidDate` <= '" + stopDate.ToString("yyyy-MM-dd") + "' )";

                cmd += ";";

                DataTable dd = G1.get_db_data(cmd);
                if ( dd.Rows.Count > 0 )
                {
                    dx.Merge(dd);
                }

            }
            return dx;
        }
        /***********************************************************************************************/
        private DataTable pullFDLICAll(DataTable dt)
        {
            DataTable dx = dt.Clone();
            //dRows = dt.Select("`Death Benefit` <> '0.00' AND `Policy Extract_Policy Status` = 'A' AND `Policy Number` LIKE '77%'");
            DataRow[] dRows = dt.Select("`trustCompany` = 'FDLIC'" );
            if (dRows.Length > 0)
            {
                dx = dRows.CopyToDataTable();
                dx = SortBy(dx);
            }
            return dx;
        }
        /***********************************************************************************************/
        private DataTable pullFDLICActive(DataTable dt)
        {
            DataTable dx = dt.Clone();
            //dRows = dt.Select("`Death Benefit` <> '0.00' AND `Policy Extract_Policy Status` = 'A' AND `Policy Number` LIKE '77%'");
            DataRow[] dRows = dt.Select("`trustCompany` = 'FDLIC' AND `endingDeathBenefit` <> '0.00' ");
            if (dRows.Length > 0)
            {
                dx = dRows.CopyToDataTable();
                dx = SortBy(dx);
            }
            return dx;
        }
        /***********************************************************************************************/
        private DataTable pullFDLIC_PBActive(DataTable dt)
        {
            DataTable dx = dt.Clone();
            //dRows = dt.Select("`Death Benefit` <> '0.00' AND `Policy Extract_Policy Status` = 'A' AND `Policy Number` LIKE '77%'");
            DataRow[] dRows = dt.Select("`trustCompany` = 'FDLIC PB' AND `endingDeathBenefit` <> '0.00' ");
            if (dRows.Length > 0)
            {
                dx = dRows.CopyToDataTable();
                dx = SortBy(dx);
            }
            return dx;
        }
        /***********************************************************************************************/
        private DataTable pullFORETHOUGHT(DataTable dt)
        {
            DataTable dx = dt.Clone();
            //dRows = dt.Select("`Death Benefit` <> '0.00' AND `Policy Extract_Policy Status` = 'A' AND `Policy Number` LIKE '77%'");

            string preOrPost = cmbPreOrPost.Text;
            if (preOrPost == "Both" && chkOldStuff.Checked)
                preOrPost = "Pre";
            else if (preOrPost == "Both")
                preOrPost = "Pre";

            if ( dgv.Visible )
            {
                preOrPost = cmbMainPrePost.Text;
                if (preOrPost == "Both")
                    preOrPost = "";
            }

            DataRow[] dRows = null;
            if ( !String.IsNullOrWhiteSpace ( preOrPost ))
                dRows = dt.Select("`trustCompany` = 'FORETHOUGHT' AND `endingDeathBenefit` <> '0.00' AND `preOrPost` = '" + preOrPost + "'");
            else
                dRows = dt.Select("`trustCompany` = 'FORETHOUGHT' AND `endingDeathBenefit` <> '0.00'");

            if (dRows.Length > 0)
            {
                dx = dRows.CopyToDataTable();
                dx = SortBy(dx);
            }
            return dx;
        }
        /***********************************************************************************************/
        private DataTable pullCD(DataTable dt)
        {
            DataTable dx = dt.Clone();
            //dRows = dt.Select("`Death Benefit` <> '0.00' AND `Policy Extract_Policy Status` = 'A' AND `Policy Number` LIKE '77%'");
            DataRow[] dRows = dt.Select("`trustCompany` = 'CD' AND `endingDeathBenefit` <> '0.00' ");
            if (dRows.Length > 0)
            {
                dx = dRows.CopyToDataTable();
                dx = SortBy(dx);
            }
            return dx;
        }
        /***********************************************************************************************/
        private void btnRunTotals_Click(object sender, EventArgs e)
        {
            workAll = false;
            if (workReport == "Post 2002 Report - All")
                workAll = true;
            DateTime date1 = this.dateTimePicker2.Value;
            DateTime date2 = this.dateTimePicker3.Value;

            string startDate = date1.ToString("yyyy-MM-dd");
            string stopDate = date2.ToString("yyyy-MM-dd");

            if ( chkQuick.Checked )
            {
                RunQuickDeceased ();
                return;
            }

            string preOrPost = cmbPreOrPost.Text.Trim();
            //if (preOrPost != "Pre" && preOrPost != "Post")
            //    preOrPost = "Post";

            PleaseWait pleaseForm = G1.StartWait("Please Wait.\nProcessing the data.");

            this.Cursor = Cursors.WaitCursor;

            string companies = "";
            DateTime date = DateTime.Now;
            DateTime reportDate = DateTime.Now;

            if (chkJustTrustsSelected.Checked)
                companies = getCompanyQuery();

            string cmd = "Select * from `trust_data` t LEFT JOIN fcust_extended f ON t.`contractNumber` = f.`contractNumber` LEFT JOIN customers x on t.`contractNumber` = x.`contractNumber` LEFT JOIN contracts c ON t.`contractNumber` = c.`contractNumber` WHERE `date` >= '" + startDate + "' AND `date` <= '" + stopDate + "' ";
            cmd = "Select * from `trust_data` t LEFT JOIN customers x on t.`contractNumber` = x.`contractNumber` LEFT JOIN contracts c ON t.`contractNumber` = c.`contractNumber` WHERE `date` >= '" + startDate + "' AND `date` <= '" + stopDate + "' ";
            cmd = "Select * from `trust_data` t WHERE `date` >= '" + startDate + "' AND `date` <= '" + stopDate + "' ";
            if (!String.IsNullOrWhiteSpace(preOrPost))
            {
                if ( preOrPost != "Both")
                    cmd += " AND `preOrPost` = '" + preOrPost + "' ";
            }
            if (!String.IsNullOrWhiteSpace(companies))
                cmd += " AND " + companies + " ";
            cmd += " ORDER by `date`, `trustCompany`;  ";

            DataTable dt = G1.get_db_data(cmd);

            string contractNumber = "";

            cmd = "Select * from `trust_data` WHERE `reportDate` >= '" + startDate + "' AND `reportDate` <= '" + stopDate + "' ";
            if (!String.IsNullOrWhiteSpace(companies))
            {
                string newCompany = companies;
                if (newCompany.IndexOf("FDLIC PB") >= 0)
                    newCompany = newCompany.Replace("'FDLIC PB'", "'FDLIC PB','FDLIC CCI'");
                cmd += " AND " + newCompany + " ";
            }
            cmd += " ORDER by `date`, `trustCompany`;  ";

            DataTable dx = G1.get_db_data(cmd);

            DataRow[] dRows = null;
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                contractNumber = dx.Rows[i]["contractNumber"].ObjToString();
                reportDate = dx.Rows[i]["reportDate"].ObjToDateTime();
                dRows = dt.Select("contractNumber='" + contractNumber + "'");
                if ( dRows.Length > 0 )
                {
                    for ( int j=0; j<dRows.Length; j++)
                    {
                        date = dRows[j]["date"].ObjToDateTime();
                        if ( date == reportDate )
                        {
                            dRows[j]["date"] = G1.DTtoMySQLDT(reportDate);
                        }
                    }
                }
                else
                {
                    dx.Rows[i]["date"] = G1.DTtoMySQLDT(reportDate.ToString("yyyy-MM-dd"));
                    dt.ImportRow(dx.Rows[i]);
                }
            }

            cmd = "Select * from `trust_data_edits` WHERE `date` >= '" + startDate + "' AND `date` <= '" + stopDate + "' ";
            if (!String.IsNullOrWhiteSpace(companies))
            {
                string newCompany = companies;
                if (newCompany.IndexOf("FDLIC PB") >= 0)
                    newCompany = newCompany.Replace("'FDLIC PB'", "'FDLIC PB','FDLIC CCI'");
                cmd += " AND " + newCompany + " ";
            }
            cmd += " ORDER by `date`, `trustCompany`;  ";

            dx = G1.get_db_data(cmd);
            dx.Columns.Add("manual");
            for (int i = 0; i < dx.Rows.Count; i++)
                dx.Rows[i]["manual"] = "Y";

            dt.Merge(dx);

            dx = new DataTable();
            dx.Columns.Add("month");
            dx.Columns.Add("Security National", Type.GetType("System.Double"));
            dx.Columns.Add("Forethought", Type.GetType("System.Double"));
            dx.Columns.Add("CD", Type.GetType("System.Double"));
            dx.Columns.Add("Unity", Type.GetType("System.Double"));
            dx.Columns.Add("Unity PB", Type.GetType("System.Double"));
            dx.Columns.Add("Unity DC", Type.GetType("System.Double"));
            dx.Columns.Add("Unity Total", Type.GetType("System.Double"));
            dx.Columns.Add("Unity Over/Under", Type.GetType("System.Double"));
            dx.Columns.Add("unityOldBarham", Type.GetType("System.Double"));
            dx.Columns.Add("unityOldWebb", Type.GetType("System.Double"));
            dx.Columns.Add("fdlicOldWebb", Type.GetType("System.Double"));
            dx.Columns.Add("fdlicOldCCI", Type.GetType("System.Double"));
            dx.Columns.Add("FDLIC", Type.GetType("System.Double"));
            dx.Columns.Add("FDLIC PB", Type.GetType("System.Double"));
            dx.Columns.Add("total", Type.GetType("System.Double"));

            dx.Columns.Add("unityCash", Type.GetType("System.Double"));
            dx.Columns.Add("unityOverUnder", Type.GetType("System.Double"));
            dx.Columns.Add("unityDeathBenefit", Type.GetType("System.Double"));

            dx.Columns.Add("fdlicCash", Type.GetType("System.Double"));
            dx.Columns.Add("fdlicOverUnder", Type.GetType("System.Double"));
            dx.Columns.Add("fdlicDeathBenefit", Type.GetType("System.Double"));
            dx.Columns.Add("year");

            dt = fixTheData(dt);

            DataView tempview = dt.DefaultView;
            tempview.Sort = "date ASC";
            dt = tempview.ToTable();


            date = DateTime.Now;
            string month = "";
            string lastMonth = "";
            string trustCompany = "";
            string type = "";
            double money = 0D;
            double dValue = 0D;

            double total = 0D;

            DataRow dRow = null;
            int col = 0;
            string statusReason = "";
            string policyStatus = "";
            bool gotUnityOldWebb = false;
            bool gotUnityOldBarham = false;
            bool gotFDLICOldWebb = false;
            bool gotFDLICOldCCI = false;
            if (G1.get_column_number(dt, "unityOldWebb") >= 0)
                gotUnityOldWebb = true;
            if (G1.get_column_number(dt, "unityOldBarham") >= 0)
                gotUnityOldBarham = true;
            if (G1.get_column_number(dt, "fdlicOldWebb") >= 0)
                gotFDLICOldWebb = true;
            if (G1.get_column_number(dt, "fdlicOldCCI") >= 0)
                gotFDLICOldCCI = true;

            if (workReport == "Post 2002 Report - Unity")
            {
                //dt = unityActive.Copy();
                //dt.Merge(unityActivePB);

                //tempview = dt.DefaultView;
                //tempview.Sort = "date ASC";
                //dt = tempview.ToTable();


            }

            double endingDeathBenefit = 0D;
            double endingPaymentBalance = 0D;
            double deathClaimAmount = 0D;

            double totalEDB = 0D;
            double totalEPB = 0D;
            double totalDCA = 0D;

            double fdlicTotalEDB = 0D;
            double fdlicTotalEPB = 0D;
            double fdlicTotalDCA = 0D;

            string policyNumber = "";
            reportDate = DateTime.Now;

            dt.Columns.Add("newReportDate");

            bool gotReportDate = false;
            try
            {
                for ( int i=0; i<dt.Rows.Count; i++)
                {
                    date = dt.Rows[i]["date"].ObjToDateTime();
                    reportDate = dt.Rows[i]["reportDate"].ObjToDateTime();
                    if ( reportDate.Year > 1000 )
                    {
                        if (reportDate != date)
                        {
                            dt.Rows[i]["newReportDate"] = reportDate.ToString("yyyyMMdd");
                            gotReportDate = true;
                        }
                        else
                            dt.Rows[i]["newReportDate"] = date.ToString("yyyyMMdd");
                    }
                    else
                        dt.Rows[i]["newReportDate"] = date.ToString("yyyyMMdd");
                }
            }
            catch ( Exception ex)
            {
            }

            if ( gotReportDate )
            {
                tempview = dt.DefaultView;
                tempview.Sort = "newReportDate ASC";
                dt = tempview.ToTable();

                for ( int i=0; i<dt.Rows.Count; i++)
                {
                    reportDate = dt.Rows[i]["newReportDate"].ObjToDateTime();
                    dt.Rows[i]["date"] = G1.DTtoMySQLDT(reportDate.ToString("yyyy-MM-dd"));
                }
            }

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    date = dt.Rows[i]["date"].ObjToDateTime();
                    reportDate = dt.Rows[i]["reportDate"].ObjToDateTime();
                    if ( reportDate.Year > 1000 )
                    {
                    }
                    month = G1.ToMonthName(date);
                    if (month != lastMonth)
                    {
                        dRow = dx.NewRow();
                        dRow["month"] = month;
                        dRow["year"] = date.Year.ToString();
                        dx.Rows.Add(dRow);
                        lastMonth = month;

                        totalEDB = 0D;
                        totalEPB = 0D;
                        totalDCA = 0D;

                        fdlicTotalEDB = 0D;
                        fdlicTotalEPB = 0D;
                        fdlicTotalDCA = 0D;
                    }
                    policyNumber = dt.Rows[i]["policyNumber"].ObjToString();
                    if ( policyNumber == "770115438")
                    {
                    }
                    statusReason = dt.Rows[i]["statusReason"].ObjToString();
                    policyStatus = dt.Rows[i]["policyStatus"].ObjToString();
                    trustCompany = dt.Rows[i]["trustCompany"].ObjToString();
                    if (trustCompany.ToUpper().IndexOf("UNITY") >= 0)
                    {
                        if (statusReason == "DC")
                        {
                            dValue = dt.Rows[i]["endingPaymentBalance"].ObjToDouble();
                            //dValue = dt.Rows[i]["deathClaimAmount"].ObjToDouble(); // xyzzy
                            dt.Rows[i]["beginningDeathBenefit"] = 0D;
                            money = dRow["Unity DC"].ObjToDouble();
                            money += dValue;
                            dRow["Unity DC"] = money;
                            col = G1.get_column_number(dx, "unityOldBarham");
                            if (col > 0)
                            {
                                //dValue = dt.Rows[i]["unityOldBarham"].ObjToDouble();
                                dValue = dt.Rows[i]["endingDeathBenefit"].ObjToDouble();
                                money = dRow[col].ObjToDouble();
                                money += dValue;
                                dRow[col] = money;
                            }
                            continue;
                        }
                        if (workReport == "Post 2002 Report - Unity" || workAll )
                        {
                            if (policyStatus.ToUpper() != "A")
                            {
                            //    if (policyStatus == "T")
                            //    {
                            //        if (statusReason != "LP" && statusReason != "NI" && statusReason != "NN" && statusReason != "NT" && statusReason != "SR")
                            //            continue;
                            //    }
                            //    else
                            //        continue;
                            //}
                            //else
                            //{
                            //    if ( !String.IsNullOrWhiteSpace ( statusReason) )
                            //    {
                            //        if (statusReason != "RI")
                            //            continue;
                            //    }
                            }
                        }
                    }
                    col = G1.get_column_number(dx, trustCompany);
                    if (col > 0)
                    {
                        dValue = dt.Rows[i]["beginningDeathBenefit"].ObjToDouble();
                        dValue = dt.Rows[i]["endingDeathBenefit"].ObjToDouble();
                        if (workReport == "Post 2002 Report - Unity" || workAll )
                        {
                            if (trustCompany == "Unity" || trustCompany == "Unity PB" )
                            {
                                endingPaymentBalance = dt.Rows[i]["endingPaymentBalance"].ObjToDouble();
                                endingDeathBenefit = dt.Rows[i]["endingDeathBenefit"].ObjToDouble();
                                deathClaimAmount = dt.Rows[i]["deathClaimAmount"].ObjToDouble();

                                totalEPB += endingPaymentBalance;
                                totalEDB += endingDeathBenefit;
                                totalDCA += deathClaimAmount;

                                dValue = dt.Rows[i]["beginningPaymentBalance"].ObjToDouble();
                                dValue = dt.Rows[i]["beginningDeathBenefit"].ObjToDouble();
                            }
                            else
                                dValue = dt.Rows[i]["endingPaymentBalance"].ObjToDouble();
                        }
                        money = dRow[col].ObjToDouble();
                        money += dValue;
                        dRow[col] = money;
                        if (trustCompany == "Unity" || trustCompany == "Unity PB" )
                        {
                            col = G1.get_column_number(dx, "unityDeathBenefit");
                            if ( col >= 0 )
                            {
                                money = dRow[col].ObjToDouble();
                                money += dValue;
                                dRow[col] = totalEDB;
                            }
                        }
                        else if (trustCompany == "FDLIC" || trustCompany == "FDLIC PB")
                        {
                            endingDeathBenefit = dt.Rows[i]["endingDeathBenefit"].ObjToDouble();
                            col = G1.get_column_number(dx, "fdlicDeathBenefit");
                            if (col >= 0)
                            {
                                money = dRow[col].ObjToDouble();
                                money += endingDeathBenefit;
                                dRow[col] = money;
                            }
                        }
                    }
                    else
                    {
                    }
                    col = G1.get_column_number(dx, "unityOldWebb");
                    if (col > 0 && gotUnityOldWebb )
                    {
                        dValue = dt.Rows[i]["unityOldWebb"].ObjToDouble();
                        money = dRow[col].ObjToDouble();
                        money += dValue;
                        dRow[col] = money;
                    }
                    col = G1.get_column_number(dx, "unityOldBarham");
                    if (col > 0 && gotUnityOldBarham )
                    {
                        dValue = dt.Rows[i]["unityOldBarham"].ObjToDouble();
                        money = dRow[col].ObjToDouble();
                        money += dValue;
                        dRow[col] = money;
                    }
                    col = G1.get_column_number(dx, "fdlicOldWebb");
                    if (col > 0 && gotFDLICOldWebb )
                    {
                        dValue = dt.Rows[i]["fdlicOldWebb"].ObjToDouble();
                        money = dRow[col].ObjToDouble();
                        money += dValue;
                        dRow[col] = money;
                    }
                    col = G1.get_column_number(dx, "fdlicOldCCI");
                    if (col > 0 && gotFDLICOldCCI )
                    {
                        dValue = dt.Rows[i]["fdlicOldCCI"].ObjToDouble();
                        money = dRow[col].ObjToDouble();
                        money += dValue;
                        dRow[col] = money;
                    }
                }
                catch (Exception ex)
                {
                }
            }


            int col1 = G1.get_column_number(dx, "Security National");
            int col2 = G1.get_column_number(dx, "FDLIC");

            double unity = 0D;
            double unityPB = 0D;
            double unityCash = 0D;
            double unityDC = 0D;
            double unityTotal = 0D;

            double fdlic = 0D;
            double fdlicPB = 0D;
            double fdlicCash = 0D;

            endingPaymentBalance = 0D;

            statusReason = "";

            for (int i = 0; i < dx.Rows.Count; i++)
            {
                unity = dx.Rows[i]["UNITY"].ObjToDouble();
                unityPB = dx.Rows[i]["UNITY PB"].ObjToDouble();
                unityCash = unity + unityPB;
                dx.Rows[i]["unityCash"] = unityCash;

                fdlic = dx.Rows[i]["FDLIC"].ObjToDouble();
                fdlicPB = dx.Rows[i]["FDLIC PB"].ObjToDouble();
                fdlicCash = fdlic + fdlicPB;
                dx.Rows[i]["fdlicCash"] = fdlicCash;
            }

            LoadUS(dx, startDate, stopDate );

            string fieldName = "";
            money = 0D;
            double unityDeathBenefit = 0D;
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                try
                {
                    total = 0D;
                    for (int j = col1; j <= col2; j++)
                    {
                        fieldName = dx.Columns[j].ColumnName.ObjToString();
                        if (gridMain2.Columns[fieldName].Visible)
                        {
                            money = dx.Rows[i][j].ObjToDouble();
                            total += money;
                        }
                    }
                    dx.Rows[i]["total"] = total;

                    //statusReason = dx.Rows[i]["statusReason"].ObjToString();
                    //trustCompany = dx.Rows[i]["trustCompany"].ObjToString();
                    //unity = dx.Rows[i]["UNITY"].ObjToDouble();
                    //unityPB = dx.Rows[i]["UNITY PB"].ObjToDouble();
                    //unityCash = unity + unityPB;
                    //dx.Rows[i]["unityCash"] = unityCash;
                    //unityDC = dx.Rows[i]["Unity DC"].ObjToDouble();
                    //dx.Rows[i]["Unity Total"] = unity + unityPB - unityDC;

                    //fdlic = dx.Rows[i]["FDLIC"].ObjToDouble();
                    //fdlicPB = dx.Rows[i]["FDLIC PB"].ObjToDouble();
                    //fdlicCash = fdlic + fdlicPB;
                    //dx.Rows[i]["fdlicCash"] = fdlicCash;

                    unityDeathBenefit = dx.Rows[i]["unityDeathBenefit"].ObjToDouble();
                    money = unityCash - unity;
                    //dx.Rows[i]["unityOverUnder"] = money;
                }
                catch (Exception ex)
                {
                }
            }

            double oldUnityCash = 0D;
            double oldUnityDC = 0D;
            if (dx.Rows.Count > 1)
            {
                oldUnityCash = dx.Rows[0]["unityCash"].ObjToDouble();
                oldUnityDC = dx.Rows[0]["Unity DC"].ObjToDouble();
            }
            for (int i = 1; i < dx.Rows.Count; i++)
            {
                unityCash = dx.Rows[i]["unityCash"].ObjToDouble();
                unityDC = dx.Rows[i]["Unity DC"].ObjToDouble();
                dx.Rows[i]["Unity Over/Under"] = (unityCash - oldUnityCash) - (unityDC - oldUnityDC);
            }

            date = date1;
            int days = 0;


            //for (int i = 0; i < dx.Rows.Count; i++)
            //{
            //    days = DateTime.DaysInMonth(date.Year, date.Month);
            //    date1 = new DateTime(date.Year, date.Month, 1);
            //    startDate = date1.ToString("yyyy-MM-dd");

            //    date2 = new DateTime(date.Year, date.Month, days);
            //    stopDate = date2.ToString("yyyy-MM-dd");

            //    cmd = "SELECT * FROM trust_data WHERE((deathPaidDate >= '" + startDate + "' AND deathPaidDate <= '" + stopDate + "') OR(`date` = '" + stopDate + "' AND deathClaimAmount > '0')) AND `preOrPost` = '" + preOrPost + "' AND `trustCompany` LIKE 'UNITY%';";
            //    dt = G1.get_db_data(cmd);

            //    total = 0D;
            //    for (int j = 0; j < dt.Rows.Count; j++)
            //        total += dt.Rows[j]["deathClaimAmount"].ObjToDouble();
            //    //dx.Rows[i]["unityDeathBenefit"] = total;

            //    cmd = "SELECT * FROM trust_data WHERE((deathPaidDate >= '" + startDate + "' AND deathPaidDate <= '" + stopDate + "') OR(`date` = '" + stopDate + "' AND deathClaimAmount > '0')) AND `preOrPost` = '" + preOrPost + "' AND `trustCompany` LIKE 'FDLIC%';";
            //    dt = G1.get_db_data(cmd);

            //    total = 0D;
            //    for (int j = 0; j < dt.Rows.Count; j++)
            //        total += dt.Rows[j]["deathClaimAmount"].ObjToDouble();
            //    //dx.Rows[i]["fdlicDeathBenefit"] = total;
            //}

            G1.NumberDataTable(dx);
            dgv2.DataSource = dx;

            G1.StopWait(ref pleaseForm);
            pleaseForm = null;
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private DataTable trustDts = null;
        private void LoadUS ( DataTable dx, string startDate, string stopDate )
        {
            string cmd = "";
            DataTable dt = null;
            DateTime date = DateTime.Now;
            string month = "";
            string str = "";
            int mm = 0;
            int days = 0;
            double money = 0D;
            DataRow[] dRows = null;
            DataTable activeDt = null;
            cmd = "Select * from `trust_data_edits` WHERE `status` = 'EndingBalance' AND `date` >= '" + startDate + "' AND `date` <= '" + stopDate + "';";
            trustDts = G1.get_db_data(cmd);
            trustDts.Columns.Add("sDate");
            for ( int i=0; i<trustDts.Rows.Count; i++)
            {
                date = trustDts.Rows[i]["date"].ObjToDateTime();
                trustDts.Rows[i]["sDate"] = date.ToString("yyyyMMdd");
            }

            string column = "";
            for ( int i=0; i<dx.Rows.Count; i++)
            {
                month = dx.Rows[i]["month"].ObjToString();
                mm = G1.ConvertMonthToIndex(month);
                str = dx.Rows[i]["year"].ObjToString();
                days = DateTime.DaysInMonth(str.ObjToInt32(), mm);
                date = new DateTime(str.ObjToInt32(), mm, days);

                for ( int j=0; j<dx.Columns.Count-1; j++)
                {
                    column = dx.Columns[j].ColumnName.ObjToString().Trim();
                    dRows = trustDts.Select("sDate='" + date.ToString("yyyyMMdd") + "' AND trustName = '" + column + "'");
                    if ( dRows.Length > 0 )
                    {
                        money = dRows[0]["beginningDeathBenefit"].ObjToDouble();
                        dx.Rows[i][column] = money;
                    }
                }
                //cmd = "Select * from `trust_data_edits` WHERE `trustName` = 'Unity' and `status` = 'EndingBalance' AND `date` = '" + date.ToString("yyyy-MM-dd") + "';" ;
                //dt = G1.get_db_data(cmd);
                //if ( dt.Rows.Count > 0 )
                //{
                //    money = dt.Rows[0]["beginningDeathBenefit"].ObjToDouble();
                //    dx.Rows[i]["Unity"] = money;
                //}

                //cmd = "Select * from `trust_data_edits` WHERE `trustName` = 'FDLIC' and `status` = 'EndingBalance' AND `date` = '" + date.ToString("yyyy-MM-dd") + "';";
                //dt = G1.get_db_data(cmd);
                //if (dt.Rows.Count > 0)
                //{
                //    money = dt.Rows[0]["beginningDeathBenefit"].ObjToDouble();
                //    dx.Rows[i]["FDLIC"] = money;
                //}

                //cmd = "Select * from `trust_data_edits` WHERE `trustName` = 'FORETHOUGHT' and `status` = 'EndingBalance' AND `date` = '" + date.ToString("yyyy-MM-dd") + "';";
                //dt = G1.get_db_data(cmd);
                //if (dt.Rows.Count > 0)
                //{
                //    money = dt.Rows[0]["beginningDeathBenefit"].ObjToDouble();
                //    dx.Rows[i]["Forethought"] = money;
                //}

                dRows = trustDts.Select("sDate='" + date.ToString("yyyyMMdd") + "' AND trustName = 'Forethought'");
                if ( dRows.Length == 0 )
                {
                    money = TrustDeceased.loadForethoughtBalance(date, cmbPreOrPost.Text.Trim(), ref activeDt);
                    dx.Rows[i]["Forethought"] = money;
                }
                dRows = trustDts.Select("sDate='" + date.ToString("yyyyMMdd") + "' AND trustName = 'Security National'");
                if (dRows.Length == 0)
                {
                    money = TrustDeceased.loadSecurityNationalBalance(date, cmbPreOrPost.Text.Trim(), ref activeDt);
                    dx.Rows[i]["Security National"] = money;
                }
            }
        }
        /***********************************************************************************************/
        private void button2_Click(object sender, EventArgs e)
        {
            DateTime now = this.dateTimePicker2.Value;
            now = now.AddMonths(-1);
            this.dateTimePicker2.Value = new DateTime(now.Year, now.Month, 1);

            int days = DateTime.DaysInMonth(now.Year, now.Month);
            this.dateTimePicker3.Value = new DateTime(now.Year, now.Month, days);
            this.dateTimePicker3.Refresh();
            this.dateTimePicker2.Refresh();
        }
        /***********************************************************************************************/
        private void button1_Click(object sender, EventArgs e)
        {
            DateTime now = this.dateTimePicker2.Value;
            now = now.AddMonths(1);
            this.dateTimePicker2.Value = new DateTime(now.Year, now.Month, 1);

            int days = DateTime.DaysInMonth(now.Year, now.Month);
            this.dateTimePicker3.Value = new DateTime(now.Year, now.Month, days);
            this.dateTimePicker3.Refresh();
            this.dateTimePicker2.Refresh();
        }
        /***********************************************************************************************/
        private void btnGenerate_Click(object sender, EventArgs e)
        {
            if (dgv == null)
                return;
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt == null)
                return;
            DataRow[] dRows = dt.Select("trustCompany='FDLIC'");
            if (dRows.Length <= 0)
                return;

            DateTime lastPaidDate = DateTime.Now;
            double bBalance = 0D;
            double endingBalance = 0D;

            this.Cursor = Cursors.WaitCursor;

            dt = dRows.CopyToDataTable();

            DataTable dx = new DataTable();
            dx.Columns.Add("month");
            dx.Columns.Add("FDLICTRUST", Type.GetType("System.Double"));
            dx.Columns.Add("DC");
            dx.Columns.Add("description");
            dx.Columns.Add("cadenceDate");
            dx.Columns.Add("contractNumber");
            dx.Columns.Add("funeralNumber");

            dx.Columns.Add("FDLICTRUST2", Type.GetType("System.Double"));
            //            dx.Columns.Add("space");
            dx.Columns.Add("DC2");
            dx.Columns.Add("description2");
            dx.Columns.Add("cadenceDate2");
            dx.Columns.Add("contractNumber2");
            dx.Columns.Add("funeralNumber2");

            DateTime date = this.dateTimePicker1.Value;
            DateTime newDate = date.AddMonths(-1);

            double beginningBalance = 14092320.65D;
            double previousDP = 0D;
            double previousPayments = 0D;

            string date1 = newDate.ToString("yyyy-MM-dd");
            string cmd = "Select * from `cashremit_coversheet` where `date` = '" + date1 + "';";
            DataTable ddd = G1.get_db_data(cmd);
            if (ddd.Rows.Count > 0)
            {
                beginningBalance = ddd.Rows[0]["beginningBalance"].ObjToDouble();
                previousDP = ddd.Rows[0]["fdlicDownPayments"].ObjToDouble();
                previousPayments = ddd.Rows[0]["fdlicMonthlyPayments"].ObjToDouble();
            }

            double payments = 0D;
            double downPayments = 0D;

            DataRow dR = null;
            string month = G1.ToMonthName(newDate);
            dR = dx.NewRow();
            dR["month"] = month;
            dx.Rows.Add(dR);
            dR = dx.NewRow();
            dR["month"] = "Beginning";
            dx.Rows.Add(dR);
            dR = dx.NewRow();
            dR["month"] = "Balance";
            dR["FDLICTRUST"] = beginningBalance;
            dx.Rows.Add(dR);

            dx = addEmptyRow(dx);
            dx = addEmptyRow(dx);

            string monthName = G1.ToMonthName(newDate);

            dx = addNewRow(dx, previousDP, monthName + " Down Payments");
            dx = addNewRow(dx, previousPayments, monthName + " Monthly Payments");

            dx = addEmptyRow(dx);

            double dTotal = previousPayments + previousDP;
            dx = addNewRow(dx, dTotal, "");

            dx = addEmptyRow(dx);
            dx = addEmptyRow(dx);

            dx = addNewRow(dx, -previousDP, "Sent to FDLIC");
            dx = addNewRow(dx, -previousPayments, "Sent to FDLIC");

            dx = addEmptyRow(dx);

            double fdlicTotal = 0 - previousPayments - previousDP;
            dx = addNewRow(dx, fdlicTotal, "");

            dx = addEmptyRow(dx);

            int pass = 1;

            payments = 0D;
            downPayments = 0D;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                payments += dt.Rows[i]["payments"].ObjToDouble();
                downPayments += dt.Rows[i]["downPayments"].ObjToDouble();
            }

            string extra = "";
            double diff = downPayments - previousDP;
            if (diff != 0D)
            {
                if (diff > 0D)
                    extra = "Over " + G1.ReformatMoney(diff);
                else if (diff < 0D)
                    extra = "Under " + G1.ReformatMoney(diff);
            }
            dx = addNewRow(dx, downPayments, "FDLIC DECEMBER DOWN PAYMENTS", extra);

            extra = "";
            diff = payments - previousPayments;
            if (diff != 0D)
            {
                if (diff > 0D)
                    extra = "Over " + G1.ReformatMoney(diff);
                else if (diff < 0)
                    extra = "Under " + G1.ReformatMoney(diff);
            }
            dx = addNewRow(dx, payments, "FDLIC DECEMBER MONTHLY PAYMENTS", extra);

            dx = addEmptyRow(dx);

            dTotal = payments + downPayments;
            dx = addNewRow(dx, dTotal, "");

            dx = addEmptyRow(dx);
            dx = addEmptyRow(dx);
            dx = addEmptyRow(dx);


            DateTime dDate = DateTime.Now;
            DataTable ddx = dt.Clone();
            dRows = dt.Select("deathClaimAmount <> '0.00' AND trustCompany='FDLIC'");
            if (dRows.Length > 0)
            {
                ddx = dRows.CopyToDataTable();

                ddx.Columns.Add("tempDate");
                for (int i = 0; i < ddx.Rows.Count; i++)
                {
                    dDate = ddx.Rows[i]["deathPaidDate"].ObjToDateTime();
                    if (dDate.Year > 1900)
                        ddx.Rows[i]["tempDate"] = dDate.ToString("yyyyMMdd");
                }
                DataView tempview = ddx.DefaultView;
                tempview.Sort = "tempDate asc";
                ddx = tempview.ToTable();
            }

            dTotal = 0D;
            double deathClaim = 0D;
            string contractNumber = "";
            string funeralNumber = "";
            string deathDate = "";
            string firstName = "";
            string lastName = "";
            string middleName = "";
            string policyNumber = "";
            bool nextMonth = false;

            int firstDcCashRow = -1;
            int firstDcPaidRow = -1;
            int firstXXXRow = -1;

            for (int i = 0; i < ddx.Rows.Count; i++)
            {
                try
                {
                    deathClaim = ddx.Rows[i]["deathClaimAmount"].ObjToDouble();
                    if (deathClaim == 0D)
                        continue;
                    dDate = ddx.Rows[i]["deathPaidDate"].ObjToDateTime();
                    deathDate = dDate.ToString("MM/dd/yyyy");
                    funeralNumber = ddx.Rows[i]["serviceId"].ObjToString();
                    firstName = ddx.Rows[i]["firstName"].ObjToString();
                    middleName = ddx.Rows[i]["middleName"].ObjToString();
                    lastName = ddx.Rows[i]["lastName"].ObjToString();
                    policyNumber = ddx.Rows[i]["policyNumber"].ObjToString();
                    contractNumber = ddx.Rows[i]["contractNumber"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(middleName))
                        firstName += " " + middleName;
                    if (!String.IsNullOrWhiteSpace(lastName))
                        firstName += " " + lastName;

                    if (checkPB(contractNumber))
                        continue;

                    if (!VerifyDate(date, contractNumber, funeralNumber, ref nextMonth))
                        continue;

                    dx = addNewRow(dx, deathClaim, firstName, deathDate, " DC CASH", contractNumber, funeralNumber);
                    if (firstDcCashRow < 0)
                        firstDcCashRow = dx.Rows.Count - 1;

                    dTotal += deathClaim;
                }
                catch (Exception ex)
                {
                }
            }

            dx = addEmptyRow(dx);
            dx = addNewRow(dx, dTotal, "");

            dx = addEmptyRow(dx);

            dx = VerifyFromFunerals(dx, date, pass, ref firstDcPaidRow);

            dx = addEmptyRow(dx);
            dTotal = 0D;

            bool isRemoved = false;
            int row = 0;

            date = this.dateTimePicker1.Value;
            date1 = date.ToString("yyyy-MM-dd");

            double newDeathClaim = 0D;
            double premium = 0D;

            string trustCompany = "";
            double downPayment = 0D;
            double beginningPaymentBalance = 0D;
            double beginningDeathBenefit = 0D;
            double endingDeathBenefit = 0D;
            double endingPaymentBalance = 0D;



            cmd = "Select * from `trust2013r` where `payDate8` = '" + date1 + "' AND ( `deathRemCurrMonth` > '0.00' || `refundRemCurrMonth` > '0.00' ) ;";
            DataTable trustDt = G1.get_db_data(cmd);

            for (int i = 0; i < ddx.Rows.Count; i++)
            {
                try
                {
                    contractNumber = ddx.Rows[i]["contractNumber"].ObjToString();
                    if (contractNumber.ToUpper() == "P22071L")
                    {
                    }
                    downPayment = ddx.Rows[i]["downPayment"].ObjToDouble();
                    trustCompany = ddx.Rows[i]["trustCompany"].ObjToString().ToUpper();
                    beginningDeathBenefit = ddx.Rows[i]["beginningDeathBenefit"].ObjToDouble();
                    endingDeathBenefit = ddx.Rows[i]["endingDeathBenefit"].ObjToDouble();
                    deathClaim = ddx.Rows[i]["beginningPaymentBalance"].ObjToDouble();
                    premium = deathClaim;
                    deathClaim = ddx.Rows[i]["deathClaimAmount"].ObjToDouble();
                    //if ( trustCompany.ToUpper().IndexOf ( "FDLIC") >= 0 )
                    //{
                    //    if( beginningDeathBenefit <= 0D && downPayment > 0D && endingDeathBenefit > 0D )
                    //    {
                    //        deathClaim = endingDeathBenefit;
                    //        ddx.Rows[i]["beginningPaymentBalance"] = endingDeathBenefit;
                    //    }
                    //}
                    if (deathClaim == 0D)
                        continue;

                    dDate = ddx.Rows[i]["deathPaidDate"].ObjToDateTime();
                    deathDate = dDate.ToString("MM/dd/yyyy");
                    funeralNumber = ddx.Rows[i]["serviceId"].ObjToString();
                    firstName = ddx.Rows[i]["firstName"].ObjToString();
                    middleName = ddx.Rows[i]["middleName"].ObjToString();
                    lastName = ddx.Rows[i]["lastName"].ObjToString();
                    policyNumber = ddx.Rows[i]["policyNumber"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(middleName))
                        firstName += " " + middleName;
                    if (!String.IsNullOrWhiteSpace(lastName))
                        firstName += " " + lastName;

                    if (checkPB(contractNumber))
                        continue;

                    if (contractNumber == "P19003L")
                    {
                    }

                    isRemoved = true;
                    if (!VerifyDate(date, contractNumber, funeralNumber, ref nextMonth))
                    {
                        if (!nextMonth)
                            continue;
                        isRemoved = false;
                        if (CheckRemoved(contractNumber, date))
                            isRemoved = true;
                    }
                    if (!String.IsNullOrWhiteSpace(contractNumber))
                    {
                        if (contractNumber.ToUpper() == "L21146LI")
                        {
                        }
                        deathClaim = 0D;
                        dRows = trustDt.Select("contractNumber='" + contractNumber + "'");
                        if (dRows.Length > 0)
                        {
                            deathClaim = dRows[0]["deathRemCurrMonth"].ObjToDouble();
                            if (deathClaim == 0D)
                                deathClaim = dRows[0]["refundRemCurrMonth"].ObjToDouble();
                        }
                        newDeathClaim = 0D;
                        lastPaidDate = DailyHistory.GetTrustLastPaid(contractNumber, ref bBalance, ref endingBalance);
                        if (endingBalance != 0D)
                            newDeathClaim = endingBalance;
                        else if (bBalance != 0D)
                            newDeathClaim = bBalance;
                        if (newDeathClaim != deathClaim)
                        {
                            deathClaim = newDeathClaim;
                        }
                        if (deathClaim == 0D)
                            deathClaim = premium;
                    }

                    dx = addNewRow(dx, deathClaim, firstName, deathDate, " DC XXXX", contractNumber, funeralNumber);
                    row = dx.Rows.Count - 1;

                    if (firstXXXRow < 0)
                        firstXXXRow = dx.Rows.Count - 1;

                    if (nextMonth && isRemoved)
                        dx.Rows[row][0] = "BC Paid Next / Is Removed";
                    else if (nextMonth && !isRemoved)
                        dx.Rows[row][0] = "BC Paid Next / Not Removed";
                    dTotal += deathClaim;
                }
                catch (Exception ex)
                {
                }
            }

            dx = addEmptyRow(dx);
            dx = addNewRow(dx, dTotal, "");

            /***********************************************************************************************/

            pass = 2;
            dTotal = 0D;

            for (int i = 0; i < ddx.Rows.Count; i++)
            {
                try
                {
                    deathClaim = ddx.Rows[i]["deathClaimAmount"].ObjToDouble();
                    if (deathClaim == 0D)
                        continue;
                    dDate = ddx.Rows[i]["deathPaidDate"].ObjToDateTime();
                    deathDate = dDate.ToString("MM/dd/yyyy");
                    funeralNumber = ddx.Rows[i]["serviceId"].ObjToString();
                    firstName = ddx.Rows[i]["firstName"].ObjToString();
                    middleName = ddx.Rows[i]["middleName"].ObjToString();
                    lastName = ddx.Rows[i]["lastName"].ObjToString();
                    policyNumber = ddx.Rows[i]["policyNumber"].ObjToString();
                    contractNumber = ddx.Rows[i]["contractNumber"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(middleName))
                        firstName += " " + middleName;
                    if (!String.IsNullOrWhiteSpace(lastName))
                        firstName += " " + lastName;

                    if (!checkPB(contractNumber))
                    {
                        continue;
                    }

                    if (!VerifyDate(date, contractNumber, funeralNumber, ref nextMonth))
                        continue;

                    //dx = addNewRow(dx, deathClaim, firstName, deathDate, " DC CASH", contractNumber, funeralNumber);

                    dR = dx.Rows[firstDcCashRow];
                    dR["FDLICTRUST2"] = deathClaim;
                    dR["description2"] = firstName;
                    if (!String.IsNullOrWhiteSpace(deathDate))
                        dR["cadenceDate2"] = deathDate;
                    dR["DC2"] = "PB DC CASH";
                    if (!String.IsNullOrWhiteSpace(contractNumber))
                        dR["contractNumber2"] = contractNumber;
                    if (!String.IsNullOrWhiteSpace(funeralNumber))
                        dR["funeralNumber2"] = funeralNumber;
                    firstDcCashRow++;

                    dTotal += deathClaim;
                }
                catch (Exception ex)
                {
                }
            }

            firstDcCashRow++;
            dR = dx.Rows[firstDcCashRow];
            dR["FDLICTRUST2"] = dTotal;

            dx = VerifyFromFunerals(dx, date, pass, ref firstDcPaidRow);

            dTotal = 0D;

            isRemoved = false;
            row = 0;

            date = this.dateTimePicker1.Value;
            date1 = date.ToString("yyyy-MM-dd");

            newDeathClaim = 0D;
            premium = 0D;

            cmd = "Select * from `trust2013r` where `payDate8` = '" + date1 + "' AND ( `deathRemCurrMonth` > '0.00' || `refundRemCurrMonth` > '0.00' ) ;";
            trustDt = G1.get_db_data(cmd);

            for (int i = 0; i < ddx.Rows.Count; i++)
            {
                try
                {
                    contractNumber = ddx.Rows[i]["contractNumber"].ObjToString();
                    if (contractNumber.ToUpper() == "P22071L")
                    {
                    }
                    deathClaim = ddx.Rows[i]["beginningPaymentBalance"].ObjToDouble();
                    premium = deathClaim;
                    deathClaim = ddx.Rows[i]["deathClaimAmount"].ObjToDouble();
                    if (deathClaim == 0D)
                        continue;

                    dDate = ddx.Rows[i]["deathPaidDate"].ObjToDateTime();
                    deathDate = dDate.ToString("MM/dd/yyyy");
                    funeralNumber = ddx.Rows[i]["serviceId"].ObjToString();
                    firstName = ddx.Rows[i]["firstName"].ObjToString();
                    middleName = ddx.Rows[i]["middleName"].ObjToString();
                    lastName = ddx.Rows[i]["lastName"].ObjToString();
                    policyNumber = ddx.Rows[i]["policyNumber"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(middleName))
                        firstName += " " + middleName;
                    if (!String.IsNullOrWhiteSpace(lastName))
                        firstName += " " + lastName;

                    if (!checkPB(contractNumber))
                        continue;

                    isRemoved = true;
                    if (!VerifyDate(date, contractNumber, funeralNumber, ref nextMonth))
                    {
                        if (!nextMonth)
                            continue;
                        isRemoved = false;
                        if (CheckRemoved(contractNumber, date))
                            isRemoved = true;
                    }
                    if (!String.IsNullOrWhiteSpace(contractNumber))
                    {
                        deathClaim = 0D;
                        dRows = trustDt.Select("contractNumber='" + contractNumber + "'");
                        if (dRows.Length > 0)
                        {
                            deathClaim = dRows[0]["deathRemCurrMonth"].ObjToDouble();
                            if (deathClaim == 0D)
                                deathClaim = dRows[0]["refundRemCurrMonth"].ObjToDouble();
                        }
                        newDeathClaim = 0D;
                        lastPaidDate = DailyHistory.GetTrustLastPaid(contractNumber, ref bBalance, ref endingBalance);
                        if (endingBalance != 0D)
                            newDeathClaim = endingBalance;
                        else if (bBalance != 0D)
                            newDeathClaim = bBalance;
                        if (newDeathClaim != deathClaim)
                        {
                            deathClaim = newDeathClaim;
                        }
                        if (deathClaim == 0D)
                            deathClaim = premium;
                    }

                    //dx = addNewRow(dx, deathClaim, firstName, deathDate, " DC XXXX", contractNumber, funeralNumber);
                    //row = dx.Rows.Count - 1;

                    dR = dx.Rows[firstXXXRow];
                    dR["FDLICTRUST2"] = deathClaim;
                    dR["description2"] = firstName;
                    if (!String.IsNullOrWhiteSpace(deathDate))
                        dR["cadenceDate2"] = deathDate;
                    dR["DC2"] = "PB DC XXX";
                    if (!String.IsNullOrWhiteSpace(contractNumber))
                        dR["contractNumber2"] = contractNumber;
                    if (!String.IsNullOrWhiteSpace(funeralNumber))
                        dR["funeralNumber2"] = funeralNumber;
                    firstXXXRow++;

                    //if (nextMonth && isRemoved)
                    //    dx.Rows[row][0] = "BC Paid Next / Is Removed";
                    //else if (nextMonth && !isRemoved)
                    //    dx.Rows[row][0] = "BC Paid Next / Not Removed";
                    dTotal += deathClaim;
                }
                catch (Exception ex)
                {
                }
            }

            firstXXXRow++;
            dR = dx.Rows[firstXXXRow];
            dR["FDLICTRUST2"] = dTotal;

            dgv3.DataSource = dx;

            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private bool CheckRemoved(string contractNumber, DateTime date)
        {
            bool rv = false;
            date = date.AddMonths(1);
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            DateTime newDate = new DateTime(date.Year, date.Month, days);
            string cmd = "Select * from `contracts` WHERE `contractNumber` = '" + contractNumber + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                DateTime dateRemoved = dx.Rows[0]["dateRemoved"].ObjToDateTime();
                if (dateRemoved == newDate)
                    rv = true;
            }
            return rv;
        }
        /***********************************************************************************************/
        private string findContractNumber(string policy)
        {
            string contractNumber = "";
            string cmd = "Select * from `policytrusts` WHERE `policyNumber` = '" + policy + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
                contractNumber = dx.Rows[0]["contractNumber"].ObjToString();
            return contractNumber;
        }
        /***********************************************************************************************/
        private bool checkPB(string contractNumber)
        {
            bool rv = false;
            string cmd = "Select * from `policytrusts` WHERE `contractNumber` = '" + contractNumber + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                if (dx.Rows[0]["type"].ObjToString().ToUpper() == "PB")
                    rv = true;
            }
            return rv;
        }
        /***********************************************************************************************/
        private bool VerifyDate(DateTime date, string contractNumber, string funeralNumber, ref bool nextMonth)
        {
            bool rv = true;
            if (contractNumber.ToUpper() == "NULL")
                return rv;
            //if (String.IsNullOrWhiteSpace(funeralNumber))
            //    return rv;

            DateTime dateStart = new DateTime(date.Year, date.Month, 1);
            string date1 = date.ToString("yyyy-MM-01");
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            DateTime dateStop = new DateTime(date.Year, date.Month, days);
            string date2 = dateStop.ToString("yyyy-MM-dd");

            DateTime nextStart = dateStart.AddMonths(1);
            days = DateTime.DaysInMonth(nextStart.Year, nextStart.Month);
            DateTime nextStop = new DateTime(nextStart.Year, nextStart.Month, days);

            string cmd = "";
            DataTable dx = null;
            DataTable dt = null;
            bool itsOk = false;
            nextMonth = false;
            if (!String.IsNullOrWhiteSpace(contractNumber))
            {
                cmd = "SELECT * FROM `cust_payments` WHERE `type` = 'TRUST' AND `status` = 'Deposited' ";
                cmd += " AND `trust_policy` LIKE '%" + contractNumber + "%' ";
                cmd += " ORDER BY `dateModified` ";
                cmd += ";";

                dt = G1.get_db_data(cmd);
                if (dt.Rows.Count <= 0)
                {
                    cmd = "SELECT * FROM `cust_payments` WHERE `type` = 'TRUST' AND `status` = 'Deposited' ";
                    cmd += " AND `referenceNumber` LIKE '%" + contractNumber + "%' ";
                    cmd += " ORDER BY `dateModified` ";
                    cmd += ";";

                    dt = G1.get_db_data(cmd);
                }
                if (dt.Rows.Count > 0)
                {
                    for (int j = 0; j < dt.Rows.Count; j++)
                    {
                        string paymentRecord = dt.Rows[j]["record"].ObjToString();
                        cmd = "SELECT * FROM `cust_payment_details` WHERE `type` = 'TRUST' AND `status` = 'Deposited' ";
                        cmd += " AND `paymentRecord` = '" + paymentRecord + "' ";
                        cmd += " ORDER BY `dateReceived` ";
                        cmd += ";";

                        dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count > 0)
                        {
                            for (int i = 0; i < dx.Rows.Count; i++)
                            {
                                DateTime dateReceived = dx.Rows[i]["dateReceived"].ObjToDateTime();
                                if (dateReceived >= dateStart && dateReceived <= dateStop)
                                {
                                    itsOk = true;
                                }
                                else if (dateReceived >= nextStart && dateReceived <= nextStop)
                                    nextMonth = true;
                            }
                        }
                    }
                }
            }
            if (!itsOk)
                rv = false;

            return rv;
        }
        /***********************************************************************************************/
        private DataTable VerifyFromFunerals(DataTable ddx, DateTime date, int pass, ref int firstDcPaidRow)
        {
            string date1 = date.ToString("yyyy-MM-01");
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            date = new DateTime(date.Year, date.Month, days);
            string date2 = date.ToString("yyyy-MM-dd");

            double dTotal = 0D;

            string cmd = "SELECT * FROM `cust_payment_details` WHERE `type` = 'TRUST' AND `status` = 'Deposited' AND `dateReceived` >= '" + date1 + "' AND `dateReceived` <= '" + date2 + "' ";
            cmd += " AND ( `contractNumber` LIKE '%L' OR `contractNumber` LIKE '%LI' ) ";
            cmd += " ORDER BY `dateReceived` ";
            cmd += ";";

            cmd = "SELECT * FROM `cust_payments` a JOIN `cust_payment_details` p ON a.`record` = p.`paymentRecord` WHERE a.`type` = 'TRUST' AND a.`status` = 'Deposited' AND p.`dateReceived` >= '" + date1 + "' AND p.`dateReceived` <= '" + date2 + "' ";
            cmd += " AND ( ( `trust_policy` LIKE '%L' OR `trust_policy` LIKE '%LI' )  OR ( `referenceNumber` LIKE '%L' OR `referenceNumber` LIKE '%LI' ) )";
            cmd += " ORDER BY p.`dateReceived` ";
            cmd += ";";

            if (pass == 2)
            {
                cmd = "SELECT * FROM `cust_payments` a JOIN `cust_payment_details` p ON a.`record` = p.`paymentRecord` WHERE a.`type` = 'TRUST' AND a.`status` = 'Deposited' AND p.`dateReceived` >= '" + date1 + "' AND p.`dateReceived` <= '" + date2 + "' ";
                //cmd += " AND ( ( `trust_policy` LIKE '%L' OR `trust_policy` LIKE '%LI' )  OR ( `referenceNumber` LIKE '%L' OR `referenceNumber` LIKE '%LI' ) )";
                cmd += " ORDER BY p.`dateReceived` ";
                cmd += ";";
            }

            DataTable dt = G1.get_db_data(cmd);

            if (dt.Rows.Count > 0)
            {
                string contractNumber = "";
                string serviceId = "";
                DataRow[] dRows = null;
                DataRow dR = null;
                DataTable dx = null;
                double deathClaim = 0D;
                string deathDate = "";
                string funeralNumber = "";
                string firstName = "";
                string middleName = "";
                string lastName = "";
                string paidFrom = "";
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    try
                    {
                        contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                        if (pass == 2)
                        {
                            if (!checkPB(contractNumber))
                                continue;
                        }
                        paidFrom = dt.Rows[i]["paidFrom"].ObjToString();
                        if (paidFrom.ToUpper() != "FDLIC")
                            continue;
                        deathDate = dt.Rows[i]["dateReceived"].ObjToDateTime().ToString("yyyy-MM-dd");
                        deathClaim = dt.Rows[i]["amtActuallyReceived"].ObjToDouble();
                        firstName = "";
                        middleName = "";
                        lastName = "";
                        cmd = "Select * from `customers` where `contractNumber` = '" + contractNumber + "';";
                        dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count > 0)
                        {
                            firstName = dx.Rows[0]["firstName"].ObjToString();
                            middleName = dx.Rows[0]["middleName"].ObjToString();
                            lastName = dx.Rows[0]["lastName"].ObjToString();
                            if (!String.IsNullOrWhiteSpace(middleName))
                                firstName += " " + middleName;
                            if (!String.IsNullOrWhiteSpace(lastName))
                                firstName += " " + lastName;
                        }

                        funeralNumber = "";
                        cmd = "Select * from `fcust_extended` where `contractNumber` = '" + contractNumber + "';";
                        dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count > 0)
                            funeralNumber = dx.Rows[0]["serviceId"].ObjToString();
                        if (pass == 1)
                        {
                            ddx = addNewRow(ddx, deathClaim, firstName, deathDate, " DC PAID", contractNumber, funeralNumber);

                            if (firstDcPaidRow < 0)
                                firstDcPaidRow = ddx.Rows.Count - 1;
                        }
                        else
                        {
                            dR = ddx.Rows[firstDcPaidRow];
                            dR["FDLICTRUST2"] = deathClaim;
                            dR["description2"] = firstName;
                            if (!String.IsNullOrWhiteSpace(deathDate))
                                dR["cadenceDate2"] = deathDate;
                            dR["DC2"] = "PB DC PAID";
                            if (!String.IsNullOrWhiteSpace(contractNumber))
                                dR["contractNumber2"] = contractNumber;
                            if (!String.IsNullOrWhiteSpace(funeralNumber))
                                dR["funeralNumber2"] = funeralNumber;
                            firstDcPaidRow++;
                        }

                        dTotal += deathClaim;
                    }
                    catch (Exception ex)
                    {
                    }
                }
                if (pass == 1)
                {
                    dRows = ddx.Select("contractNumber='NULL'");
                    if (dRows.Length > 0)
                    {
                        dx = dRows.CopyToDataTable();
                        for (int j = 0; j < dx.Rows.Count; j++)
                        {
                            deathClaim = dx.Rows[j]["FDLICTRUST"].ObjToDouble();
                            firstName = dx.Rows[j]["description"].ObjToString();
                            ddx = addNewRow(ddx, deathClaim, firstName, deathDate, " DC PAID", "NULL", "O/S");

                            if (firstDcPaidRow < 0)
                                firstDcPaidRow = ddx.Rows.Count - 1;

                            dTotal += deathClaim;
                        }
                    }
                }

                if (pass == 1)
                {
                    ddx = addEmptyRow(ddx);
                    ddx = addNewRow(ddx, dTotal, "");
                }
                else if (pass == 2)
                {
                    firstDcPaidRow++;
                    dR = ddx.Rows[firstDcPaidRow];
                    dR["FDLICTRUST2"] = dTotal;
                }
            }

            return ddx;
        }
        /***********************************************************************************************/
        private DataTable addEmptyRow(DataTable dx)
        {
            DataRow dR = dx.NewRow();
            dx.Rows.Add(dR);
            return dx;
        }
        /***********************************************************************************************/
        private DataTable addNewRow(DataTable dx, double value, string desc, string extra = "", string dc = "", string contractNumber = "", string funeralNumber = "")
        {
            DataRow dR = dx.NewRow();
            dR["FDLICTRUST"] = value;
            dR["description"] = desc;
            if (!String.IsNullOrWhiteSpace(extra))
                dR["cadenceDate"] = extra;
            if (!String.IsNullOrWhiteSpace(dc))
                dR["DC"] = dc;
            if (!String.IsNullOrWhiteSpace(contractNumber))
                dR["contractNumber"] = contractNumber;
            if (!String.IsNullOrWhiteSpace(funeralNumber))
                dR["funeralNumber"] = funeralNumber;
            dx.Rows.Add(dR);
            return dx;
        }
        /***********************************************************************************************/
        private void pictureBox3_Click(object sender, EventArgs e)
        {
            if (dgv3.Visible)
                G1.SpyGlass(gridMain3);
        }
        /***********************************************************************************************/
        private void gridMain3_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain3.GetFocusedDataRow();
            string contract = dr["contractNumber"].ObjToString();
            string funeralNumber = dr["funeralNumber"].ObjToString();
            string dc = dr["DC"].ObjToString();
            if (dc.ToUpper().IndexOf("XXX") > 0)
            {
                this.Cursor = Cursors.WaitCursor;
                CustomerDetails clientForm = new CustomerDetails(contract);
                clientForm.Show();
                this.Cursor = Cursors.Default;
                return;
            }
            if (!String.IsNullOrWhiteSpace(funeralNumber) && !String.IsNullOrWhiteSpace(contract) && contract != "NULL")
            {
                this.Cursor = Cursors.WaitCursor;
                FunPayments editFunPayments = new FunPayments(this, contract, "", false, false);
                editFunPayments.TopMost = true;
                editFunPayments.Show();
                this.Cursor = Cursors.Default;
                return;
            }
            if (!String.IsNullOrWhiteSpace(contract) && contract != "NULL")
            {
                this.Cursor = Cursors.WaitCursor;
                string cmd = "Select * from `contracts` where `contractNumber` = '" + contract + "';";
                DataTable dx = G1.get_db_data(cmd);
                if (dx.Rows.Count <= 0)
                {
                    string cnum = contract.TrimStart('0');
                    cnum = cnum.Replace(" ", "");

                    cmd = "Select * from `icustomers` where `payer` = '" + cnum + "';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                        contract = dx.Rows[0]["contractNumber"].ObjToString();
                }
                CustomerDetails clientForm = new CustomerDetails(contract);
                clientForm.Show();
                this.Cursor = Cursors.Default;
            }
        }
        /***********************************************************************************************/
        private void chkGroupContract_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            if (checkBox.Checked)
            {
                gridMain.Columns["contractNumber"].GroupIndex = 0;
                gridMain.ExpandAllGroups();
            }
            else
            {
                gridMain.Columns["contractNumber"].GroupIndex = -1;
                gridMain.CollapseAllDetails();
            }
            gridMain.RefreshData();
            gridMain.RefreshEditor(true);
            dgv.Refresh();
        }
        /***********************************************************************************************/
        private void chkTBB_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox cBox = (CheckBox)sender;
            if (!cBox.Checked)
            {
                //gridMain.Columns["endingBalance"].Visible = false;
                //gridMain.Columns["overshort"].Visible = false;
                gridMain.RefreshData();
                gridMain.RefreshEditor(true);
                dgv.Refresh();
                barImport.Hide();
                chkFilterMismatches.Hide();
                chkFilterMismatches.Refresh();
                return;
            }
            else
                LoadTBB();
        }
        /***********************************************************************************************/
        private void LoadTBB()
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt == null)
                return;
            if (dt.Rows.Count <= 0)
                return;

            DataView tempview = dt.DefaultView;
            tempview.Sort = "contractNumber";
            dt = tempview.ToTable();

            if (G1.get_column_number(dt, "endingBalance") < 0)
                dt.Columns.Add("endingBalance", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "overshort") < 0)
                dt.Columns.Add("overshort", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "tSurrender") < 0)
                dt.Columns.Add("tSurrender", Type.GetType("System.Double"));

            gridMain.Columns["endingBalance"].Visible = true;
            gridMain.Columns["overshort"].Visible = true;
            gridMain.Columns["tSurrender"].Visible = true;

            string contractNumber = "";
            string oldContract = "";
            string contractNumber3 = "";

            double dValue = 0D;
            double refund = 0D;

            this.Cursor = Cursors.WaitCursor;

            barImport.Show();
            barImport.Minimum = 0;
            barImport.Maximum = dt.Rows.Count;
            barImport.Value = 0;

            DateTime date = this.dateTimePicker1.Value;
            date = date.AddMonths(-1);
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            DateTime newDate = new DateTime(date.Year, date.Month, days);
            string date1 = newDate.ToString("yyyy-MM-dd");


            string cmd = "Select * from `trust2013r` WHERE `payDate8` = '" + date1 + "';";
            DataTable dx = G1.get_db_data(cmd);

            DataTable ddx = null;
            DataRow[] dRows = null;
            int lastRow = 0;
            double surrender = 0D;
            double overShort = 0D;
            int oldRow = -1;
            string lastName = "";

            string txtContract = this.txtContract.Text.Trim();
            if (!String.IsNullOrWhiteSpace(txtContract))
            {
                dRows = dt.Select("contractNumber='" + txtContract + "'");
                if (dRows.Length > 0)
                    dt = dRows.CopyToDataTable();
            }

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                Application.DoEvents();

                barImport.Value = i;
                barImport.Refresh();

                contractNumber3 = dt.Rows[i]["contractNumber3"].ObjToString();
                if (String.IsNullOrWhiteSpace(contractNumber3))
                {
                    lastName = dt.Rows[i]["lastName"].ObjToString();
                    lastName += " ( NCR )";
                    dt.Rows[i]["lastName"] = lastName;

                }

                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                if (string.IsNullOrWhiteSpace(contractNumber))
                    continue;
                if (contractNumber == "FF17017UI")
                {
                }
                surrender = dt.Rows[i]["beginningDeathBenefit"].ObjToDouble();
                dt.Rows[i]["tSurrender"] = surrender;
                //if (contractNumber == oldContract)
                //{
                //    dValue = 0D;
                //    dt.Rows[i]["endingBalance"] = dValue;
                //    surrender = dt.Rows[i]["beginningDeathBenefit"].ObjToDouble();
                //    overShort = surrender - dValue;
                //    dt.Rows[i]["overshort"] = overShort;
                //    continue;
                //}
                if (contractNumber == oldContract)
                {
                    dValue = 0D;
                    dt.Rows[i]["endingBalance"] = dValue;
                    surrender = dt.Rows[i]["beginningDeathBenefit"].ObjToDouble();
                    if (oldRow >= 0)
                    {
                        dValue = dt.Rows[oldRow]["tSurrender"].ObjToDouble();
                        //dt.Rows[oldRow]["beginningDeathBenefit"] = dValue + surrender;
                        //dt.Rows[i]["beginningDeathBenefit"] = 0D;
                        dt.Rows[oldRow]["tSurrender"] = dValue + surrender;
                        dt.Rows[i]["tSurrender"] = 0D;
                    }
                    //overShort = surrender - dValue;
                    //dt.Rows[i]["overshort"] = 0D;
                    continue;
                }
                oldContract = contractNumber;
                oldRow = i;
                dRows = dx.Select("contractNumber='" + contractNumber + "'");
                if (dRows.Length > 0)
                {
                    //ddx = dRows.CopyToDataTable();
                    dValue = dRows[0]["endingBalance"].ObjToDouble();
                    refund = dt.Rows[i]["refund"].ObjToDouble();
                    if (refund > 0D)
                        dValue -= refund;
                    dt.Rows[i]["endingBalance"] = dValue;
                    surrender = dt.Rows[i]["beginningDeathBenefit"].ObjToDouble();
                    dt.Rows[i]["tSurrender"] = surrender;

                    overShort = surrender - dValue;
                    dt.Rows[i]["overshort"] = overShort;
                }
                else
                {
                    DateTime removePaid = dt.Rows[i]["dateRemoved"].ObjToDateTime();
                    if (removePaid.Year < 1000)
                    {
                        contractNumber3 = dt.Rows[i]["contractNumber3"].ObjToString();
                        string trustRemoved = dt.Rows[i]["trustRemoved"].ObjToString();
                        string trustRefunded = dt.Rows[i]["trustRefunded"].ObjToString();
                        cmd = "Select * from `trust2013r` WHERE `contractNumber` = '" + contractNumber + "' ORDER BY `payDate8` DESC limit 1;";
                        ddx = G1.get_db_data(cmd);
                        if (ddx.Rows.Count > 0)
                        {
                            lastName = dt.Rows[i]["lastName"].ObjToString();
                            lastName = lastName + (" *");
                            dt.Rows[i]["lastName"] = lastName;
                        }
                    }
                    else
                    {
                        lastName = dt.Rows[i]["lastName"].ObjToString();
                        dt.Rows[i]["lastName"] = lastName + " (" + removePaid.ToString("yyyy-MM-dd") + " )";
                    }
                }
            }

            dt.Columns.Add("MySort");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                if (contractNumber == "L03010")
                {
                }
                if (contractNumber.ToUpper().IndexOf("U") > 0)
                    dt.Rows[i]["MySort"] = "1";
                else
                    dt.Rows[i]["MySort"] = "2";
                dValue = dt.Rows[i]["endingBalance"].ObjToDouble();
                surrender = dt.Rows[i]["tSurrender"].ObjToDouble(); // This was Current Cash Received
                //surrender = dt.Rows[i]["beginningDeathBenefit"].ObjToDouble(); // This was Current Cash Received
                //surrender = dt.Rows[i]["endingDeathBenefit"].ObjToDouble();
                overShort = surrender - dValue;
                dt.Rows[i]["overshort"] = overShort;
            }

            tempview = dt.DefaultView;
            tempview.Sort = "contractNumber asc";
            dt = tempview.ToTable();

            dgv.DataSource = dt;
            gridMain.ExpandAllGroups();

            gridMain.RefreshData();
            gridMain.RefreshEditor(true);
            dgv.Refresh();

            chkFilterMismatches.Show();
            chkFilterMismatches.Refresh();

            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void addToPolicyContractXReferenceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string contractNumber = "";
            string policyNumber = "";
            string trustCompany = "";

            DataRow dr = gridMain.GetFocusedDataRow();
            policyNumber = dr["policyNumber"].ObjToString();
            if (String.IsNullOrWhiteSpace(policyNumber))
            {
                MessageBox.Show("***ERROR*** There is no Policy Number here!!!", "Policy Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }

            trustCompany = dr["trustCompany"].ObjToString();
            contractNumber = dr["contractNumber"].ObjToString();
            if (!String.IsNullOrWhiteSpace(contractNumber))
            {
                DialogResult result = MessageBox.Show("***Warning*** Contract Number (" + contractNumber + ")\n is already assigned here!\nDo you still want to assign a different one?", "Assign Contract Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                if (result == DialogResult.No)
                    return;
            }

            FunLookup fastForm = new FunLookup("", "");
            fastForm.SelectDone += FastForm_SelectDone;
            fastForm.Show();


            //using (Ask askForm = new Ask("Enter Contract # to associate with Policy (" + policyNumber + ") ! "))
            //{
            //    askForm.Text = "";
            //    askForm.ShowDialog();
            //    if (askForm.DialogResult != System.Windows.Forms.DialogResult.OK)
            //        return;
            //    contractNumber = askForm.Answer;
            //    if (String.IsNullOrWhiteSpace(contractNumber))
            //        return;
            //}

            //string record = "";
            //string cmd = "Select * from `policyTrusts` WHERE `policyNumber` = '" + policyNumber + "';";
            //DataTable dx = G1.get_db_data(cmd);
            //if (dx.Rows.Count > 0)
            //    record = dx.Rows[0]["record"].ObjToString();
            //else
            //    record = G1.create_record("policyTrusts", "Company", trustCompany);
            //if (G1.BadRecord("policyTrusts", record))
            //    return;
            //G1.update_db_table("policyTrusts", "record", record, new string[] { "policyNumber", policyNumber, "contractNumber", contractNumber, "type", "", "trustCompany", trustCompany });

            //dr["contractNumber"] = contractNumber;

            //gridMain.RefreshData();
            //gridMain.RefreshEditor(true);
            //dgv.Refresh();
        }
        /***********************************************************************************************/
        private void FastForm_SelectDone(DataTable s)
        {
            if (s == null)
                return;
            if (s.Rows.Count <= 0)
                return;
            try
            {
                DataRow[] dRows = s.Select("select='1'");
                if (dRows.Length <= 0)
                    return;

                string contractNumber = dRows[0]["contractNumber"].ObjToString();
                if (string.IsNullOrWhiteSpace(contractNumber))
                    return;
                DataRow dr = gridMain.GetFocusedDataRow();
                string policyNumber = dr["policyNumber"].ObjToString();
                string trustCompany = dr["trustCompany"].ObjToString();
                string type = "";
                if (trustCompany.IndexOf("FDLIC") >= 0)
                {
                    if (trustCompany.IndexOf("PB") > 0)
                    {
                        trustCompany = "FDLIC";
                        type = "PB";
                    }
                }

                string record = "";
                string cmd = "Select * from `policyTrusts` WHERE `policyNumber` = '" + policyNumber + "';";
                DataTable dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                    record = dx.Rows[0]["record"].ObjToString();
                else
                    record = G1.create_record("policyTrusts", "Company", trustCompany);
                if (G1.BadRecord("policyTrusts", record))
                    return;

                G1.update_db_table("policyTrusts", "record", record, new string[] { "policyNumber", policyNumber, "contractNumber", contractNumber, "type", type, "Company", trustCompany });

                dr["contractNumber"] = contractNumber;

                record = dr["record"].ObjToString();
                G1.update_db_table("trust_data", "record", record, new string[] { "contractNumber", contractNumber });

                gridMain.RefreshData();
                gridMain.RefreshEditor(true);
                dgv.Refresh();
            }
            catch (Exception ex)
            {
            }
        }
        /***********************************************************************************************/
        private void cmbSelectColumns_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            System.Windows.Forms.ComboBox combo = (System.Windows.Forms.ComboBox)sender;
            string comboName = combo.Text;
            string skinName = "";
            if (!String.IsNullOrWhiteSpace(comboName))
            {
                SetupSelectedColumns("TrustData", comboName, dgv);
                string name = "TrustData " + comboName;
                foundLocalPreference = G1.RestoreGridLayout(this, this.dgv, gridMain, LoginForm.username, name, ref skinName);
            }
            else
            {
                SetupSelectedColumns("TrustData", "Primary", dgv);
                string name = "TrustData Primary";
                foundLocalPreference = G1.RestoreGridLayout(this, this.dgv, gridMain, LoginForm.username, name, ref skinName);
            }
        }
        /***********************************************************************************************/
        private void SetupSelectedColumns(string procType, string group, DevExpress.XtraGrid.GridControl dgv)
        {
            if (String.IsNullOrWhiteSpace(group))
                return;
            if (String.IsNullOrWhiteSpace(procType))
                procType = "TrustData";
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
        private void lockScreenFormatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dgv.Visible)
            {
                string name = cmbSelectColumns.Text.Trim();
                if (String.IsNullOrWhiteSpace(name))
                    name = "Primary";
                string saveName = "TrustData " + name;
                G1.SaveLocalPreferences(this, gridMain, LoginForm.username, saveName);
            }
            else if ( dgv2.Visible )
            {
                string name = cmbSelectColumns2.Text.Trim();
                if (String.IsNullOrWhiteSpace(name))
                    name = "Primary";
                string saveName = "TrustDataTotals " + name;
                G1.SaveLocalPreferences(this, gridMain2, LoginForm.username, saveName);
            }
            else if (dgv5.Visible)
            {
                string name = comboBox3.Text.Trim();
                if (String.IsNullOrWhiteSpace(name))
                    name = "Primary";
                string saveName = "TrustDataDiff " + name;
                G1.SaveLocalPreferences(this, gridMain5, LoginForm.username, saveName);
            }
        }
        /****************************************************************************************/
        private void unlockScreenFormatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dgv.Visible)
            {
                string comboName = cmbSelectColumns.Text;
                if (!String.IsNullOrWhiteSpace(comboName))
                {
                    string name = "TrustData " + comboName;
                    G1.RemoveLocalPreferences(LoginForm.username, name);
                    //foundLocalPreference = false;
                }
            }
            else if (dgv2.Visible)
            {
                string comboName = cmbSelectColumns2.Text;
                if (!String.IsNullOrWhiteSpace(comboName))
                {
                    string name = "TrustDataTotals " + comboName;
                    G1.RemoveLocalPreferences(LoginForm.username, name);
                    //foundLocalPreference = false;
                }
            }
            else if ( dgv5.Visible )
            {
                string comboName = comboBox3.Text;
                if (!String.IsNullOrWhiteSpace(comboName))
                {
                    string name = "TrustDataDiff " + comboName;
                    G1.RemoveLocalPreferences(LoginForm.username, name);
                    //foundLocalPreference = false;
                }
            }
        }
        /****************************************************************************************/
        private void btnSelectColumns_Click(object sender, EventArgs e)
        {
            string actualName = cmbSelectColumns.Text;
            SelectColumns sform = new SelectColumns(dgv, "TrustData", "Primary", actualName);
            sform.Done += new SelectColumns.d_void_eventdone(sform_Done);
            sform.Show();
        }
        /***********************************************************************************************/
        void sform_Done()
        {
            dgv.Refresh();
            this.Refresh();
        }
        /***********************************************************************************************/
        private DataTable RemoveFromMain(DataTable bDt, DataRow[] rDt)
        {
            int i = 0;
            try
            {
                for (i = (rDt.Length - 1); i >= 0; i--)
                    bDt.Rows.Remove(rDt[i]);
            }
            catch (Exception ex)
            {
            }
            return bDt;
        }
        /***********************************************************************************************/
        private double GetFaceAmount(DataTable dt)
        {
            double faceAmount = 0D;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                faceAmount += dt.Rows[i]["endingPaymentBalance"].ObjToDouble();
                //faceAmount += dt.Rows[i]["Face Amount"].ObjToDouble();
            }
            return faceAmount;
        }
        /***********************************************************************************************/
        private DataTable reportDt = null;
        private void AddReportCount(string title, int count, double faceAmount)
        {
            DataRow dr = reportDt.NewRow();
            dr["Tab"] = title;
            dr["Count"] = count;
            dr["faceAmount"] = faceAmount;
            reportDt.Rows.Add(dr);
        }
        /***********************************************************************************************/
        private DataTable SortBy(DataTable dx)
        {
            try
            {
                DataView tempview = dx.DefaultView;
                tempview.Sort = "Insured Last Name, Insured First Name";
                dx = tempview.ToTable();
            }
            catch (Exception ex)
            {
            }
            return dx;
        }
        /***********************************************************************************************/
        DataTable unityActiveDt = null;
        DataTable unityLapsedDt = null;
        DataTable unityLapsedQuestionedDt = null;
        DataTable unityDeceasedDt = null;
        DataTable unityCancelledDt = null;
        DataTable unityPbActiveDt = null;
        DataTable unityPbDeceasedDt = null;
        DataTable unityBarhamDt = null;
        DataTable unityWebbDt = null;
        DataTable unityBarhamWebbDeceasedDt = null;
        DataTable unityPbDirectDt = null;
        DataTable unityNotFoundDt = null;
        /***********************************************************************************************/
        private DataTable processUnityData(DataTable bDt)
        {
            //this.Cursor = Cursors.WaitCursor;

            //DataTable bDt = (DataTable)dgv.DataSource;

            DataTable dt = bDt.Copy();

            DataTable dx = dt.Clone();
            DataTable backupDt = dt.Clone();

            DataRow[] dRows = null;

            reportDt = new DataTable();
            reportDt.Columns.Add("Tab");
            reportDt.Columns.Add("totalRows", Type.GetType("System.Int32"));
            reportDt.Columns.Add("Count", Type.GetType("System.Int32"));

            reportDt.Columns.Add("totalFaceAmount", Type.GetType("System.Double"));
            reportDt.Columns.Add("faceAmount", Type.GetType("System.Double"));

            DataRow dr = reportDt.NewRow();
            dr["Tab"] = "All Policies";
            dr["totalRows"] = dt.Rows.Count;
            dr["totalFaceAmount"] = GetFaceAmount(dt);
            dr["faceAmount"] = 0D;
            reportDt.Rows.Add(dr);

            double faceAmount = 0D;

            try
            {
                dRows = dt.Select("`endingDeathBenefit` <> '0.00' AND `policyStatus` = 'A'  AND `policyNumber` LIKE '77%' ");
                //dRows = dt.Select("`Death Benefit` <> '0.00' AND `Policy Extract_Policy Status` = 'A' AND `Policy Number` LIKE '77%'");

                if (dRows.Length > 0)
                {
                    dx = dRows.CopyToDataTable();
                    dx = SortBy(dx);
                    G1.NumberDataTable(dx);
                    unityActiveDt = dx.Copy();
                    //dgv2.DataSource = dx;

                    faceAmount = GetFaceAmount(dx);

                    AddReportCount("Unity Active", dx.Rows.Count, faceAmount);

                    dt = RemoveFromMain(dt, dRows);
                }
                else
                {
                    unityActiveDt = backupDt.Copy();
                    //dgv2.DataSource = backupDt;
                }
            }
            catch (Exception ex)
            {
            }

            try
            {
                dRows = dt.Select("`endingDeathBenefit` = '0.00' AND `policyStatus` = 'T'  AND `PolicyNumber` LIKE '77%' AND `statusReason` IN ('LP','NI','NN','NT','SR') ");
                //dRows = dt.Select("`Death Benefit` = '0.00' AND `Policy Extract_Policy Status` = 'T' AND `Policy Number` LIKE '77%' AND `Policy Extract_Status Reason` IN ('LP','NI','NN','NT','SR')");
                if (dRows.Length > 0)
                {
                    dx = dRows.CopyToDataTable();
                    dx = SortBy(dx);
                    G1.NumberDataTable(dx);
                    unityLapsedDt = dx.Copy();
                    //dgv3.DataSource = dx;

                    faceAmount = GetFaceAmount(dx);

                    AddReportCount("Unity Lapsed", dx.Rows.Count, faceAmount);

                    dt = RemoveFromMain(dt, dRows);
                }
                else
                {
                    unityLapsedDt = backupDt.Copy();
                    //dgv3.DataSource = backupDt;
                }
            }
            catch (Exception ex)
            {
            }

            try
            {
                dRows = dt.Select("`endingDeathBenefit` = '0.00' AND `policyStatus` = 'S'  AND `PolicyNumber` LIKE '77%' AND `statusReason` = 'AN' ");
                //dRows = dt.Select("`Death Benefit` = '0.00' AND `Policy Extract_Policy Status` = 'S' AND `Policy Number` LIKE '77%' AND `Policy Extract_Status Reason` = 'AN'");
                if (dRows.Length > 0)
                {
                    dx = dRows.CopyToDataTable();
                    dx = SortBy(dx);
                    G1.NumberDataTable(dx);
                    unityLapsedQuestionedDt = dx.Copy();
                    //dgv4.DataSource = dx;

                    faceAmount = GetFaceAmount(dx);

                    AddReportCount("Unity Lapsed Questioned", dx.Rows.Count, faceAmount);

                    dt = RemoveFromMain(dt, dRows);
                }
                else
                {
                    unityLapsedQuestionedDt = backupDt.Copy();
                    //dgv4.DataSource = backupDt;
                }
            }
            catch (Exception ex)
            {
            }

            try
            {
                dRows = dt.Select("`endingDeathBenefit` = '0.00' AND `policyStatus` = 'T'  AND `PolicyNumber` LIKE '77%' AND `statusReason` = 'DC' ");
                //dRows = dt.Select("`Death Benefit` = '0.00' AND `Policy Extract_Policy Status` = 'T' AND `Policy Number` LIKE '77%' AND `Policy Extract_Status Reason` = 'DC'");
                if (dRows.Length > 0)
                {
                    dx = dRows.CopyToDataTable();
                    dx = SortBy(dx);
                    G1.NumberDataTable(dx);
                    unityDeceasedDt = dx.Copy();
                    //dgv5.DataSource = dx;

                    faceAmount = GetFaceAmount(dx);

                    AddReportCount("Unity Deceased", dx.Rows.Count, faceAmount);

                    dt = RemoveFromMain(dt, dRows);
                }
                else
                {
                    unityDeceasedDt = backupDt.Copy();
                    //dgv5.DataSource = backupDt;
                }
            }
            catch (Exception ex)
            {
            }

            try
            {
                dRows = dt.Select("`endingDeathBenefit` = '0.00' AND `policyStatus` = 'T'  AND `PolicyNumber` LIKE '77%' AND `statusReason` = 'CA' ");
                //dRows = dt.Select("`Death Benefit` = '0.00' AND `Policy Extract_Policy Status` = 'T' AND `Policy Number` LIKE '77%' AND `Policy Extract_Status Reason` = 'CA'");
                if (dRows.Length > 0)
                {
                    dx = dRows.CopyToDataTable();
                    dx = SortBy(dx);
                    G1.NumberDataTable(dx);
                    unityCancelledDt = dx.Copy();
                    //dgv6.DataSource = dx;

                    faceAmount = GetFaceAmount(dx);

                    AddReportCount("Unity Cancelled", dx.Rows.Count, faceAmount);

                    dt = RemoveFromMain(dt, dRows);
                }
                else
                {
                    unityCancelledDt = backupDt.Copy();
                    //dgv6.DataSource = backupDt;
                }
            }
            catch (Exception ex)
            {
            }

            try
            {
                dRows = dt.Select("`endingDeathBenefit` <> '0.00' AND `policyStatus` = 'A'  AND `policyNumber` LIKE 'P%' AND `policyNumber` NOT LIKE 'PB%' AND `policyNumber` NOT LIKE 'PSPNB%' AND `policyNumber` NOT LIKE 'PSPWT%' ");
                //dRows = dt.Select("`Death Benefit` <> '0.00' AND `Policy Extract_Policy Status` = 'A' AND `Policy Number` LIKE 'P%' AND `Policy Number` NOT LIKE 'PB%' AND `Policy Number` NOT LIKE 'PSPNB%' AND `Policy Number` NOT LIKE 'PSPWT%'");
                if (dRows.Length > 0)
                {
                    dx = dRows.CopyToDataTable();
                    dx = SortBy(dx);
                    G1.NumberDataTable(dx);
                    unityPbActiveDt = dx.Copy();
                    //dgv7.DataSource = dx;

                    faceAmount = GetFaceAmount(dx);

                    AddReportCount("Unity PB Active", dx.Rows.Count, faceAmount);

                    dt = RemoveFromMain(dt, dRows);
                }
                else
                {
                    unityPbActiveDt = backupDt.Copy();
                    //dgv7.DataSource = backupDt;
                }
            }
            catch (Exception ex)
            {
            }

            try
            {
                dRows = dt.Select("`endingDeathBenefit` = '0.00' AND `policyStatus` = 'T'  AND `policyNumber` LIKE 'P%' AND `policyNumber` NOT LIKE 'PB%' AND `policyNumber` NOT LIKE 'PSPNB%' AND `policyNumber` NOT LIKE 'PSPWT%' AND `statusReason` = 'DC' ");
                //                dRows = dt.Select("`Death Benefit` = '0.00' AND `Policy Extract_Policy Status` = 'T' AND `Policy Number` LIKE 'P%' AND `Policy Number` NOT LIKE 'PB%' AND `Policy Number` NOT LIKE 'PSPNB%' AND `Policy Number` NOT LIKE 'PSPWT%' AND `Policy Extract_Status Reason` = 'DC'");
                if (dRows.Length > 0)
                {
                    dx = dRows.CopyToDataTable();
                    dx = SortBy(dx);
                    G1.NumberDataTable(dx);
                    unityPbDeceasedDt = dx.Copy();
                    //dgv8.DataSource = dx;

                    faceAmount = GetFaceAmount(dx);

                    AddReportCount("Unity PB Deceased", dx.Rows.Count, faceAmount);

                    dt = RemoveFromMain(dt, dRows);
                }
                else
                {
                    unityPbDeceasedDt = backupDt.Copy();
                    //dgv8.DataSource = backupDt;
                }
            }
            catch (Exception ex)
            {
            }

            try
            {
                dRows = dt.Select("(`endingDeathBenefit` <> '0.00' AND `policyStatus` = 'A'  AND `policyNumber` LIKE 'PSPNB%') OR `policyNumber` = 'PSPNB08002'");
                //dRows = dt.Select("( `Death Benefit` <> '0.00' AND `Policy Extract_Policy Status` = 'A' AND `Policy Number` LIKE 'PSPNB%' ) OR `Policy Number` = 'PSPNB08002'");
                if (dRows.Length > 0)
                {
                    dx = dRows.CopyToDataTable();
                    dx = SortBy(dx);
                    G1.NumberDataTable(dx);
                    unityBarhamDt = dx.Copy();
                    //dgv9.DataSource = dx;

                    faceAmount = GetFaceAmount(dx);

                    AddReportCount("Unity Barham", dx.Rows.Count, faceAmount);

                    dt = RemoveFromMain(dt, dRows);
                }
                else
                {
                    unityBarhamDt = backupDt.Copy();
                    //dgv9.DataSource = backupDt;
                }
            }
            catch (Exception ex)
            {
            }

            try
            {
                dRows = dt.Select("`endingDeathBenefit` <> '0.00' AND `policyStatus` = 'A'  AND `policyNumber` LIKE 'PSPWT%'");
                //dRows = dt.Select("`Death Benefit` <> '0.00' AND `Policy Extract_Policy Status` = 'A' AND `Policy Number` LIKE 'PSPWT%'");
                if (dRows.Length > 0)
                {
                    dx = dRows.CopyToDataTable();
                    dx = SortBy(dx);
                    G1.NumberDataTable(dx);
                    unityWebbDt = dx.Copy();
                    //dgv10.DataSource = dx;

                    faceAmount = GetFaceAmount(dx);

                    AddReportCount("Unity Webb", dx.Rows.Count, faceAmount);

                    dt = RemoveFromMain(dt, dRows);
                }
                else
                {
                    unityWebbDt = backupDt.Copy();
                    //dgv10.DataSource = backupDt;
                }
            }
            catch (Exception ex)
            {
            }

            try
            {
                dRows = dt.Select("`endingDeathBenefit` = '0.00' AND `policyStatus` = 'T'  AND (`policyNumber` LIKE 'PSPNB%' OR `policyNumber` LIKE 'PSPWT' ) AND `statusReason` = 'DC' ");
                //dRows = dt.Select("`Death Benefit` = '0.00' AND `Policy Extract_Policy Status` = 'T' AND ( `Policy Number` LIKE 'PSPNB%' OR `Policy Number` LIKE 'PSPWT%' ) AND `Policy Extract_Status Reason` = 'DC'");
                if (dRows.Length > 0)
                {
                    dx = dRows.CopyToDataTable();
                    dx = SortBy(dx);
                    G1.NumberDataTable(dx);
                    unityBarhamWebbDeceasedDt = dx.Copy();
                    //dgv11.DataSource = dx;

                    faceAmount = GetFaceAmount(dx);

                    AddReportCount("Unity Barham Webb Deceased", dx.Rows.Count, faceAmount);

                    dt = RemoveFromMain(dt, dRows);
                }
                else
                {
                    unityBarhamWebbDeceasedDt = backupDt.Copy();
                    //dgv11.DataSource = backupDt;
                }
            }
            catch (Exception ex)
            {
            }

            try
            {
                dRows = dt.Select("`policyNumber` LIKE 'PB%'");
                if (dRows.Length > 0)
                {
                    dx = dRows.CopyToDataTable();
                    dx = SortBy(dx);
                    G1.NumberDataTable(dx);
                    unityPbDirectDt = dx.Copy();
                    //dgv12.DataSource = dx;

                    faceAmount = GetFaceAmount(dx);

                    AddReportCount("Unity PB Direct", dx.Rows.Count, faceAmount);

                    dt = RemoveFromMain(dt, dRows);
                }
                else
                {
                    unityPbDirectDt = backupDt.Copy();
                    //dgv12.DataSource = backupDt;
                }
            }
            catch (Exception ex)
            {
            }

            if (dt.Rows.Count > 0)
            {
                dt = SortBy(dt);
                G1.NumberDataTable(dt);
            }
            unityNotFoundDt = dt.Copy();
            //dgv13.DataSource = dt;

            faceAmount = GetFaceAmount(dt);

            AddReportCount("Unity Not Found", dt.Rows.Count, faceAmount);

            G1.NumberDataTable(reportDt);
            //dgv14.DataSource = reportDt; // Unity Summary Count

            //SetupTotalsSummary();

            //BuildContextMenu();

            //btnExportToExcel.Show();
            //btnExportToExcel.Refresh();

            //if ( chkHonorPrevious.Checked )
            //    HonorPreviousMoves();

            //this.Cursor = Cursors.Default;
            return unityActiveDt;
        }
        /***********************************************************************************************/
        private void chkFilterMismatches_CheckedChanged(object sender, EventArgs e)
        {
            gridMain.RefreshData();
            gridMain.RefreshEditor(true);
            dgv.Refresh();
        }
        /***********************************************************************************************/
        private void gridMain_CustomRowFilter(object sender, RowFilterEventArgs e)
        {
            int row = e.ListSourceRow;
            if (dgv == null)
                return;
            if (dgv.DataSource == null)
                return;
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt.Rows.Count <= 0)
                return;

            Trust85.FindContract(dt, "M04095");

            if (chkFilterMismatches.Checked)
            {
                double endingBalance = dt.Rows[row]["endingBalance"].ObjToDouble();
                double surrender = dt.Rows[row]["beginningDeathBenefit"].ObjToDouble();
                surrender = dt.Rows[row]["tSurrender"].ObjToDouble();
                double overShort = dt.Rows[row]["overshort"].ObjToDouble();
                if (overShort == 0D)
                {
                    e.Visible = false;
                    e.Handled = true;
                }
            }
            if (chkMarked.Checked)
            {
                string mark = dt.Rows[row]["mark"].ObjToString();
                if (mark == "Y")
                {
                    //e.Visible = false;
                    //e.Handled = true;
                }
                else
                {
                    e.Visible = false;
                    e.Handled = true;
                }
            }
            if (chkRemoved.Checked)
            {
                if (G1.get_column_number(dt, "endingBalance") >= 0)
                {
                    double endingBalance = dt.Rows[row]["endingBalance"].ObjToDouble();
                    if (endingBalance == 0D)
                    {
                        DateTime deceasedDate = dt.Rows[row]["deceasedDate"].ObjToDateTime();
                        if (deceasedDate.Year >= 1800)
                        {
                            e.Visible = false;
                            e.Handled = true;
                        }
                    }
                }
            }
        }
        /***********************************************************************************************/
        public static DataTable LookupTrusts(DataTable dt)
        {
            string policy = "";
            string contractNumber = "";
            DataRow[] dRows = null;

            //dt.Columns.Add("contractNumber").SetOrdinal(0);

            string cmd = "Select * from `policytrusts`;";
            DataTable dx = G1.get_db_data(cmd);

            string oldContract = "";

            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    policy = dt.Rows[i]["policyNumber"].ObjToString();
                    if (String.IsNullOrWhiteSpace(policy))
                        continue;
                    if (policy == "SM1533865")
                    {
                        //dRows = dx.Select("policyNumber='" + policy + "'");
                        //if (dRows.Length > 0)
                        //{
                        //    DataTable ddd = dRows.CopyToDataTable();
                        //}
                    }
                    oldContract = dt.Rows[i]["contractNumber"].ObjToString();
                    dRows = dx.Select("policyNumber='" + policy + "'");
                    if (dRows.Length > 0)
                    {
                        DataTable ddd = dRows.CopyToDataTable();
                        for (int j = 0; j < dRows.Length; j++)
                        {
                            contractNumber = dRows[j]["contractNumber"].ObjToString();
                            if (contractNumber == oldContract)
                            {
                                dt.Rows[i]["contractNumber"] = contractNumber;
                                //dt.Rows[i]["contractNumber2"] = contractNumber;
                                break;
                            }
                            else if (contractNumber != "?")
                            {
                                dt.Rows[i]["contractNumber"] = contractNumber;
                                //dt.Rows[i]["contractNumber2"] = contractNumber;
                            }
                        }
                    }
                }
            }
            catch ( Exception ex)
            {
            }
            return dt;
        }
        /***********************************************************************************************/
        private DataTable LookupRefunds(DataTable dt)
        {
            string contractNumber = "";
            DataRow[] dRows = null;

            //dt.Columns.Add("contractNumber").SetOrdinal(0);

            if (G1.get_column_number(dt, "refund") < 0)
                dt.Columns.Add("refund", Type.GetType("System.Double"));

            string cmd = "Select * from `unityrefunds`;";
            DataTable dx = G1.get_db_data(cmd);
            double refund = 0D;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                if (String.IsNullOrWhiteSpace(contractNumber))
                    continue;
                dRows = dx.Select("contractNumber='" + contractNumber + "'");
                if (dRows.Length > 0)
                {
                    refund = dRows[0]["unityRefund"].ObjToDouble();
                    dt.Rows[i]["refund"] = refund;
                }
            }
            return dt;
        }
        /***********************************************************************************************/
        private void editPolicyToTrustToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            EditPolicyTrusts policyForm = new EditPolicyTrusts();
            policyForm.Show();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void editUnityRefundsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            EditUnityRefunds unityForm = new EditUnityRefunds();
            unityForm.Show();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void lookupCharlotteDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string contractNumber = dr["contractNumber"].ObjToString();
            if (String.IsNullOrWhiteSpace(contractNumber))
                return;

            PayOffDetail payoffForm = new PayOffDetail(contractNumber);
            payoffForm.Show();
        }
        /***********************************************************************************************/
        private void chkMarked_CheckedChanged(object sender, EventArgs e)
        {
            gridMain.RefreshEditor(true);
            dgv.RefreshDataSource();
            dgv.Refresh();
        }
        /***********************************************************************************************/
        private void repositoryItemCheckEdit1_CheckedChanged(object sender, EventArgs e)
        {
            int rowHandle = gridMain.FocusedRowHandle;
            string column = gridMain.FocusedColumn.FieldName.Trim();
            DataRow dr = gridMain.GetFocusedDataRow();
            if (dr == null)
                return;
            string isOn = dr[column].ObjToString();
            if (isOn.ToUpper() == "Y")
                dr[column] = "N";
            else
                dr[column] = "Y";
            btnSave.Hide();
            btnSave.Refresh();
        }
        /***********************************************************************************************/
        private void chkRemoved_CheckedChanged(object sender, EventArgs e)
        {
            gridMain.RefreshEditor(true);
            dgv.RefreshDataSource();
            dgv.Refresh();
        }
        /***********************************************************************************************/
        private void button4_Click(object sender, EventArgs e)
        {
            DateTime now = this.dateTimePicker4.Value;
            now = now.AddMonths(-1);
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            this.dateTimePicker4.Value = new DateTime(now.Year, now.Month, 1 );
            this.dateTimePicker5.Value = new DateTime(now.Year, now.Month, days);
        }
        /***********************************************************************************************/
        private void button5_Click(object sender, EventArgs e)
        {
            DateTime now = this.dateTimePicker4.Value;
            now = now.AddMonths(1);
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            this.dateTimePicker4.Value = new DateTime(now.Year, now.Month, 1);
            this.dateTimePicker5.Value = new DateTime(now.Year, now.Month, days);
        }
        /***********************************************************************************************/
        private void button3_Click(object sender, EventArgs e)
        { // Run Button for Unity
            DateTime date1 = this.dateTimePicker4.Value;
            DateTime date2 = this.dateTimePicker5.Value;

            string startDate = date1.ToString("yyyy-MM-dd");
            string stopDate = date2.ToString("yyyy-MM-dd");

            string preOrPost = comboBox1.Text.Trim();
            if (preOrPost != "Pre" && preOrPost != "Post")
                preOrPost = "Post";

            this.Cursor = Cursors.WaitCursor;

            string cmd = "Select * from `trust_data` t LEFT JOIN fcust_extended f ON t.`contractNumber` = f.`contractNumber` LEFT JOIN customers x on t.`contractNumber` = x.`contractNumber` LEFT JOIN contracts c ON t.`contractNumber` = c.`contractNumber` WHERE `date` >= '" + startDate + "' AND `date` <= '" + stopDate + "' AND `preOrPost` = '" + preOrPost + "' ";
            cmd += " AND `trustCompany` IN ('Unity','Unity PB') ";
            cmd += " ORDER by `date`, `trustCompany`;  ";

            DataTable dt = G1.get_db_data(cmd);

            DataTable dx = new DataTable();
            dx.Columns.Add("month");
            dx.Columns.Add("CD", Type.GetType("System.Double"));
            dx.Columns.Add("Unity", Type.GetType("System.Double"));
            dx.Columns.Add("Unity PB", Type.GetType("System.Double"));
            dx.Columns.Add("Unity Lapsed", Type.GetType("System.Double"));
            dx.Columns.Add("Unity Questioned Lapsed", Type.GetType("System.Double"));
            dx.Columns.Add("Unity Total", Type.GetType("System.Double"));
            dx.Columns.Add("Unity Active Difference", Type.GetType("System.Double"));
            dx.Columns.Add("Unity DC", Type.GetType("System.Double"));
            dx.Columns.Add("Unity PB DC", Type.GetType("System.Double"));
            dx.Columns.Add("Unity DC Total", Type.GetType("System.Double"));
            dx.Columns.Add("Unity DC Difference", Type.GetType("System.Double"));
            dx.Columns.Add("Unity Over/Under", Type.GetType("System.Double"));

            dx.Columns.Add("unityCash", Type.GetType("System.Double"));
            dx.Columns.Add("unityOverUnder", Type.GetType("System.Double"));
            dx.Columns.Add("unityDeathBenefit", Type.GetType("System.Double"));
            dx.Columns.Add("deathDiff", Type.GetType("System.Double"));

            dx.Columns.Add("fdlicCash", Type.GetType("System.Double"));
            dx.Columns.Add("fdlicOverUnder", Type.GetType("System.Double"));
            dx.Columns.Add("fdlicDeathBenefit", Type.GetType("System.Double"));

            dt = fixTheData(dt, true );

            DataView tempview = dt.DefaultView;
            tempview.Sort = "date ASC";
            dt = tempview.ToTable();

            DataTable activeDt = dt.Clone();
            DataTable lapsedDt = dt.Clone();
            DataTable questionedDt = dt.Clone();
            DataTable pbDt = dt.Clone();
            DataTable dcDt = dt.Clone();
            DataTable pbDtD = dt.Clone();
            DataTable deathDt = dt.Clone();

            activeDt = pullUnityActive(dt);

            lapsedDt = pullUnityLapsed(dt);

            questionedDt = pullUnityLapsedQuestioned(dt);

            pbDt = pullUnityPB(dt);

            dcDt = pullUnityDeceased(dt);
            pbDtD = pullUnityPB_Deceased(dt);

            deathDt = pullUnityDeathBenefit(dt);
            deathDt = activeDt.Copy();
            deathDt.Merge(lapsedDt);
            deathDt.Merge(questionedDt);
            deathDt.Merge(pbDt);

            DateTime date = DateTime.Now;
            string month = "";
            string lastMonth = "";
            string trustCompany = "";
            string type = "";
            double money = 0D;
            double dValue = 0D;

            double total = 0D;
            double difference = 0D;
            double difference1 = 0D;
            double difference2 = 0D;

            DataRow dRow = null;
            int col = 0;
            string statusReason = "";

            dx.Rows.Clear();

            dx = LoadColumn(dx, activeDt, "Unity");
            dx = LoadColumn(dx, lapsedDt, "Unity Lapsed");
            dx = LoadColumn(dx, questionedDt, "Unity Questioned Lapsed");
            dx = LoadColumn(dx, pbDt, "Unity PB");
            dx = LoadColumn(dx, dcDt, "Unity DC");
            dx = LoadColumn(dx, pbDtD, "Unity PB DC");
            dx = LoadColumn(dx, deathDt, "unityDeathBenefit", "endingDeathBenefit" );

            total = 0D;

            for ( int i=0; i<dx.Rows.Count; i++)
            {
                try
                {
                    dValue = dx.Rows[i]["Unity"].ObjToDouble() + dx.Rows[i]["Unity Lapsed"].ObjToDouble() + dx.Rows[i]["Unity Questioned Lapsed"].ObjToDouble() + dx.Rows[i]["Unity PB"].ObjToDouble();
                    dx.Rows[i]["Unity Total"] = dValue;
                    total = dx.Rows[i]["Unity DC"].ObjToDouble() + dx.Rows[i]["Unity PB DC"].ObjToDouble();
                    dx.Rows[i]["Unity DC Total"] = total;
                    if (i > 0)
                    {
                        money = dx.Rows[i - 1]["Unity Total"].ObjToDouble();
                        difference = money - dValue;
                        dx.Rows[i]["Unity Active Difference"] = difference;

                        dValue = dx.Rows[i]["Unity DC Total"].ObjToDouble();
                        money = dx.Rows[i - 1]["Unity DC Total"].ObjToDouble();
                        difference = money - dValue;
                        dx.Rows[i]["Unity DC Difference"] = difference;

                        dValue = dx.Rows[i]["unityDeathBenefit"].ObjToDouble();
                        money = dx.Rows[i - 1]["unityDeathBenefit"].ObjToDouble();
                        difference = money - dValue;
                        dx.Rows[i]["deathDiff"] = difference;

                        difference1 = dx.Rows[i]["Unity Active Difference"].ObjToDouble();
                        difference2 = dx.Rows[i]["Unity DC Difference"].ObjToDouble();
                        difference = difference1 + difference2;
                        dx.Rows[i]["Unity Over/Under"] = difference;
                    }
                }
                catch ( Exception ex)
                {
                }
            }

            DataRow[] dRows = null;
            string cDate = "";

            for ( int i=0; i<dx.Rows.Count; i++)
            {
            }

            G1.NumberDataTable(dx);
            dgv4.DataSource = dx;

            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private DataTable LoadColumn(DataTable dx, DataTable dt, string column, string columnIn = "" )
        {
            DateTime date = DateTime.Now;
            string month = "";
            string lastMonth = "";
            DataRow dRow = null;
            string statusReason = "";
            string trustCompany = "";
            double dValue = 0D;
            double money = 0D;
            int year = 0;
            int col = G1.get_column_number(dx, column);
            if ( col < 0 )
            {
                return dx;
            }
            if (String.IsNullOrWhiteSpace(columnIn))
                columnIn = "beginningDeathBenefit";
            DataRow [] dRows = null;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    date = dt.Rows[i]["date"].ObjToDateTime();
                    year = date.Year;
                    month = G1.ToMonthName(date);
                    month = month + " " + year;
                    dRows = dx.Select("month='" + month + "'");
                    if (dRows.Length <= 0 )
                    {
                        dRow = dx.NewRow();
                        dRow["month"] = month;
                        dx.Rows.Add(dRow);
                        dRows = dx.Select("month='" + month + "'");
                        if ( dRows.Length <= 0 )
                        {
                        }
                    }
                    dValue = dt.Rows[i][columnIn].ObjToDouble();
                    money = dRows[0][col].ObjToDouble();
                    money += dValue;
                    dRows[0][col] = money;
                }
                catch (Exception ex)
                {
                }
            }
            return dx;
        }
        /***********************************************************************************************/
        private void button8_Click(object sender, EventArgs e)
        {
            DateTime now = this.dateTimePicker7.Value;
            now = now.AddMonths(-1);
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            this.dateTimePicker7.Value = new DateTime(now.Year, now.Month, 1);
        }
        /***********************************************************************************************/
        private void button7_Click(object sender, EventArgs e)
        {
            DateTime now = this.dateTimePicker7.Value;
            now = now.AddMonths(1);
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            this.dateTimePicker7.Value = new DateTime(now.Year, now.Month, 1);
        }
        /***********************************************************************************************/
        private void button6_Click(object sender, EventArgs e)
        {
            string avoid = cmbAvoid.Text.Trim();
            string name = "";
            string avoidColumns = "";
            string[] Lines = avoid.Split('|');
            for (int i = 0; i < gridMain5.Columns.Count; i++)
            {
                name = gridMain5.Columns[i].Caption.Trim();
                if (String.IsNullOrWhiteSpace(name))
                    continue;
                if (avoid.Contains(name))
                    avoidColumns += gridMain5.Columns[i].FieldName.Trim() + ",";
            }

            avoidColumns.TrimEnd(',');

            DataTable ddd = (DataTable)dgv5.DataSource;
            if (ddd != null)
            {
                ddd.Rows.Clear();
                dgv5.DataSource = ddd;
                dgv5.Refresh();
            }

            string preOrPost = comboBox2.Text.Trim();
            if (preOrPost != "Pre" && preOrPost != "Post")
                preOrPost = "Post";

            this.Cursor = Cursors.WaitCursor;

            string companies = getCompanyQuery(chkDiffCompanies);

            DataTable dt = null;

            DateTime date = DateTime.Now;
            string start1 = "";
            string start2 = "";

            DataTable dx = null;
            DataTable dx2 = null;
            DataTable dx3 = null;

            string cmd = "";

            date = this.dateTimePicker7.Value;
            date = date.AddMonths(-1);
            start1 = new DateTime(date.Year, date.Month, 1).ToString("yyyy-MM-dd");
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            start2 = new DateTime(date.Year, date.Month, days).ToString("yyyy-MM-dd");

            cmd = "Select * from `trust_data` t LEFT JOIN fcust_extended f ON t.`contractNumber` = f.`contractNumber` LEFT JOIN customers x on t.`contractNumber` = x.`contractNumber` LEFT JOIN contracts c ON t.`contractNumber` = c.`contractNumber` WHERE `date` >= '" + start1 + "' AND `date` <= '" + start2 + "' AND `preOrPost` = '" + preOrPost + "' ";
            if (!String.IsNullOrWhiteSpace(companies))
                cmd += " AND " + companies + " ";
            cmd += " ORDER BY `date`; ";

            dx = G1.get_db_data(cmd);
            dx.Columns.Add("What");

            date = this.dateTimePicker7.Value;
            start1 = new DateTime(date.Year, date.Month, 1).ToString("yyyy-MM-dd");
            days = DateTime.DaysInMonth(date.Year, date.Month);
            start2 = new DateTime(date.Year, date.Month, days).ToString("yyyy-MM-dd");

            cmd = "Select * from `trust_data` t LEFT JOIN fcust_extended f ON t.`contractNumber` = f.`contractNumber` LEFT JOIN customers x on t.`contractNumber` = x.`contractNumber` LEFT JOIN contracts c ON t.`contractNumber` = c.`contractNumber` WHERE `date` >= '" + start1 + "' AND `date` <= '" + start2 + "' AND `preOrPost` = '" + preOrPost + "' ";
            if (!String.IsNullOrWhiteSpace(companies))
                cmd += " AND " + companies + " ";
            cmd += " ORDER BY `date`; ";

            dx2 = G1.get_db_data(cmd);
            dx2.Columns.Add("What");

            dx3 = CompareRows(dx, dx2, avoidColumns);

            string what = "";
            int col = 0;
            for (int j = (dx3.Rows.Count - 1); j >= 0; j--)
            {
                what = dx3.Rows[j]["What"].ObjToString();
                col = G1.get_column_number(gridMain5, what);
                if (col >= 0)
                {
                    dx3.Rows[j]["What"] = gridMain5.Columns[col].Caption;
                }
                else
                    dx3.Rows.RemoveAt(j);
            }

            G1.NumberDataTable(dx3);
            dgv5.DataSource = dx3;
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private DataTable CompareRows ( DataTable dx, DataTable dx2, string exceptColumns )
        {
            DataTable dx3 = dx.Clone();
            string policyNumber = "";
            DataRow[] dRows = null;
            int firstCol = G1.get_column_number(dx, "policyNumber");
            string data1 = "";
            string data2 = "";
            string colName = "";
            for ( int i=0; i<dx.Rows.Count; i++)
            {
                policyNumber = dx.Rows[i]["policyNumber"].ObjToString();
                dRows = dx2.Select("policyNumber='" + policyNumber + "'");
                if (dRows.Length <= 0)
                    dx3.ImportRow(dx.Rows[i]);
                else
                {
                    for ( int j=firstCol; j<dx.Columns.Count; j++)
                    {
                        colName = dx.Columns[j].Caption.Trim();
                        if (exceptColumns.Contains(colName))
                            continue;
                        //if (colName == "GROWTH")
                        //    continue;
                        data1 = dx.Rows[i][j].ObjToString();
                        data2 = dRows[0][j].ObjToString();
                        if ( data1 != data2 )
                        {
                            dx.Rows[i]["What"] = colName;
                            dRows[0]["What"] = colName;
                            dx3.ImportRow(dx.Rows[i]);
                            dx3.ImportRow(dRows[0]);
                            break;
                        }
                    }
                }
            }
            return dx3;
        }
        /***********************************************************************************************/
        private void button9_Click(object sender, EventArgs e)
        { // Select Columns
            string actualName = cmbSelectColumns.Text;
            SelectColumns sform = new SelectColumns(dgv5, "TrustDataDiff", "Primary", actualName);
            sform.Done += new SelectColumns.d_void_eventdone(sform_DoneDiff);
            sform.Show();
        }
        /***********************************************************************************************/
        void sform_DoneDiff()
        {
            dgv5.Refresh();
            this.Refresh();
        }
        /***********************************************************************************************/
        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            System.Windows.Forms.ComboBox combo = (System.Windows.Forms.ComboBox)sender;
            string comboName = combo.Text;
            string skinName = "";
            if (!String.IsNullOrWhiteSpace(comboName))
            {
                SetupSelectedColumns("TrustDataDiff", comboName, dgv5);
                string name = "TrustDataDiff " + comboName;
                //foundLocalPreference = G1.RestoreGridLayout(this, this.dgv, gridMain, LoginForm.username, name, ref skinName);
                G1.RestoreGridLayout(this, this.dgv5, gridMain5, LoginForm.username, name, ref skinName);
            }
            else
            {
                SetupSelectedColumns("TrustDataDiff", "Primary", dgv);
                string name = "TrustDataDiff Primary";
                //foundLocalPreference = G1.RestoreGridLayout(this, this.dgv, gridMain, LoginForm.username, name, ref skinName);
                G1.RestoreGridLayout(this, this.dgv5, gridMain5, LoginForm.username, name, ref skinName);
            }
        }
        /***********************************************************************************************/
        private void button12_Click(object sender, EventArgs e)
        { // Left
            DateTime now = this.dateTimePicker8.Value;
            now = now.AddMonths(-1);
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            this.dateTimePicker8.Value = new DateTime(now.Year, now.Month, 1);
            this.dateTimePicker9.Value = new DateTime(now.Year, now.Month, days);
        }
        /***********************************************************************************************/
        private void button11_Click(object sender, EventArgs e)
        { // Right
            DateTime now = this.dateTimePicker8.Value;
            now = now.AddMonths(1);
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            this.dateTimePicker8.Value = new DateTime(now.Year, now.Month, 1);
            this.dateTimePicker9.Value = new DateTime(now.Year, now.Month, days);
        }
        /***********************************************************************************************/
        private void button10_Click(object sender, EventArgs e)
        { // Run Death Balance
            DateTime date1 = this.dateTimePicker8.Value;
            DateTime date2 = this.dateTimePicker9.Value;

            string startDate = date1.ToString("yyyy-MM-dd");
            string stopDate = date2.ToString("yyyy-MM-dd");

            string preOrPost = comboBox1.Text.Trim();
            if (preOrPost != "Pre" && preOrPost != "Post")
                preOrPost = "Post";

            this.Cursor = Cursors.WaitCursor;

            string companies = getCompanyQuery(chkDeathCompanies);

            string cmd = "Select * from `trust_data` t LEFT JOIN fcust_extended f ON t.`contractNumber` = f.`contractNumber` LEFT JOIN customers x on t.`contractNumber` = x.`contractNumber` LEFT JOIN contracts c ON t.`contractNumber` = c.`contractNumber` WHERE `date` >= '" + startDate + "' AND `date` <= '" + stopDate + "' AND `preOrPost` = '" + preOrPost + "' ";
            if (!String.IsNullOrWhiteSpace(companies))
                cmd += " AND " + companies + " ";
            cmd += " ORDER by `date`, `trustCompany`;  ";

            DataTable dt = G1.get_db_data(cmd);

            dt.Columns.Add("month");

            DataTable dx = dt.Clone();

            LoadColumn(dx, dt, "beginningDeathBenefit", "beginningDeathBenefit");
            LoadColumn(dx, dt, "endingDeathBenefit", "endingDeathBenefit");
            LoadColumn(dx, dt, "beginningPaymentBalance", "beginningPaymentBalance");
            LoadColumn(dx, dt, "downPayments", "downPayments");
            LoadColumn(dx, dt, "payments", "payments");
            LoadColumn(dx, dt, "endingPaymentBalance", "endingPaymentBalance");

            companies = companies.Replace("`trustCompany` IN", "");
            companies = companies.Replace("'", "");
            companies = companies.Replace("(", "");
            companies = companies.Replace(")", "");
            companies = companies.Trim();
            //companies = companies.Replace(",", "");

            for (int i=0; i<dx.Rows.Count; i++)
            {
                dx.Rows[i]["trustCompany"] = companies;
            }

            if (chkLoadSMFS.Checked)
                dx = LoadSMFS(dx);

            G1.NumberDataTable(dx);
            dgv6.DataSource = dx;

            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private DataTable LoadSMFS( DataTable dt )
        {
            DateTime date1 = this.dateTimePicker8.Value;
            DateTime date2 = this.dateTimePicker9.Value;

            string startDate = date1.ToString("yyyy-MM-dd");
            string stopDate = date2.ToString("yyyy-MM-dd");

            string preOrPost = comboBox1.Text.Trim();
            if (preOrPost != "Pre" && preOrPost != "Post")
                preOrPost = "Post";

            this.Cursor = Cursors.WaitCursor;

            string companies = getCompanyQuery(chkDeathCompanies);

            //DataTable dt = (DataTable)dgv6.DataSource;

            if (G1.get_column_number(dt, "smfs") < 0)
                dt.Columns.Add("smfs", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "smfsDiff") < 0)
                dt.Columns.Add("smfsDiff", Type.GetType("System.Double"));

            //if (G1.get_column_number(dt, "overshort") < 0)
            //    dt.Columns.Add("overshort", Type.GetType("System.Double"));
            //if (G1.get_column_number(dt, "tSurrender") < 0)
            //    dt.Columns.Add("tSurrender", Type.GetType("System.Double"));

            //gridMain.Columns["endingBalance"].Visible = true;
            //gridMain.Columns["overshort"].Visible = true;
            //gridMain.Columns["tSurrender"].Visible = true;

            string contractNumber = "";
            string oldContract = "";
            string contractNumber3 = "";

            double dValue = 0D;
            double refund = 0D;

            this.Cursor = Cursors.WaitCursor;

            barImport.Show();
            barImport.Minimum = 0;
            barImport.Maximum = dt.Rows.Count;
            barImport.Value = 0;

            //string cmd = "Select * from `trust2013r` WHERE `payDate8` >= '" + startDate + "' and `payDate8` <= `" + stopDate + "' ";
            //cmd += " AND (`contractNumber` LIKE '%L' OR `contractNumber` LIKE '%LI' ";
            //cmd += ";";
            //DataTable dx = G1.get_db_data(cmd);

            DataTable dx = null;
            string cmd = "";

            DataTable ddx = null;
            DataRow[] dRows = null;
            int lastRow = 0;
            double surrender = 0D;
            double overShort = 0D;
            int oldRow = -1;
            string lastName = "";

            string txtContract = this.txtContract.Text.Trim();
            if (!String.IsNullOrWhiteSpace(txtContract))
            {
                dRows = dt.Select("contractNumber='" + txtContract + "'");
                if (dRows.Length > 0)
                    dt = dRows.CopyToDataTable();
            }

            string month = "";
            string year = "";
            string str = "";
            string[] Lines = null;
            int iMonth = 0;
            int days = 0;
            DateTime date = DateTime.Now;

            double balance = 0D;
            double endingBalance = 0D;
            double oldEndingBalance = 0D;
            double diff = 0D;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                Application.DoEvents();

                barImport.Value = i;
                barImport.Refresh();

                str = dt.Rows[i]["month"].ObjToString();
                if (String.IsNullOrWhiteSpace(str))
                    continue;
                Lines = str.Split(' ');
                if (Lines.Length < 2)
                    continue;
                month = Lines[0].Trim();
                year = Lines[1].Trim();
                iMonth = G1.ConvertMonthToIndex(month);
                if (iMonth < 1 || iMonth > 12)
                    continue;
                days = DateTime.DaysInMonth(year.ObjToInt32(), iMonth);
                date = new DateTime(year.ObjToInt32(), iMonth, days);

                startDate = date.ToString("yyyy-MM-dd");


                cmd = "Select * from `trust2013r` WHERE `payDate8` = '" + startDate + "' ";
                cmd += " AND (`contractNumber` LIKE '%L' OR `contractNumber` LIKE '%LI' ) ";
                cmd += ";";
                dx = G1.get_db_data(cmd);

                endingBalance = 0D;
                for ( int j=0; j<dx.Rows.Count; j++)
                {
                    balance = dx.Rows[j]["endingBalance"].ObjToDouble();
                    endingBalance += balance;
                }
                endingBalance = G1.RoundValue(endingBalance);
                dt.Rows[i]["smfs"] = endingBalance;
            }

            try
            {
                double smfs = 0D;
                double endingPaymentBalance = 0D;
                lastRow = dt.Rows.Count - 1;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    smfs = dt.Rows[i]["smfs"].ObjToDouble();
                    if (i >= lastRow)
                        break;
                    endingPaymentBalance = dt.Rows[i + 1]["endingPaymentBalance"].ObjToDouble();
                    diff = smfs - endingPaymentBalance;
                    dt.Rows[i]["smfsDiff"] = diff;
                }
            }
            catch ( Exception ex)
            {
            }

            barImport.Value = dt.Rows.Count;

            this.Cursor = Cursors.Default;

            return (dt);
        }
        /***********************************************************************************************/
        private void fDLICToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if ( dgv.Visible )
                ShowMapTable("FDLIC", this.gridMain );
            else if ( dgv6.Visible )
                ShowMapTable("FDLIC", this.gridMain6 );
        }
        /***********************************************************************************************/
        private void ShowMapTable ( string who, DevExpress.XtraGrid.Views.Grid.GridView dg )
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Excel Col");
            dt.Columns.Add("MySQL Col");
            dt.Columns.Add("Caption");

            if (who.ToUpper() == "FDLIC")
            {
                dt = AddMapRow(dt, dg, "Policy No.", "policyNumber");
                dt = AddMapRow(dt, dg, "SMFS Policy No.", "contractNumber");
                dt = AddMapRow(dt, dg, "Funeral Home", "trustName");
                dt = AddMapRow(dt, dg, "Beginning Payment Balance", "beginningPaymentBalance");
                dt = AddMapRow(dt, dg, "Beginning DB", "beginningDeathBenefit");
                dt = AddMapRow(dt, dg, "Ending Death Benefit", "endingDeathBenefit");
                dt = AddMapRow(dt, dg, "Ending Payment Balance", "endingPaymentBalance");
                dt = AddMapRow(dt, dg, "Down Payments", "downPayments");
                dt = AddMapRow(dt, dg, "Payments", "payments");
                dt = AddMapRow(dt, dg, "Death Claim Amount", "deathClaimAmount");
                dt = AddMapRow(dt, dg, "Paid Date", "deathPaidDate");
                dt = AddMapRow(dt, dg, "Insured Name", "insuredName");
            }
            else if ( who.ToUpper() == "UNITY")
            {
                dt = AddMapRow(dt, dg, "Policy Number", "policyNumber");
                dt = AddMapRow(dt, dg, "FH Name", "trustName");
                dt = AddMapRow(dt, dg, "Prior Cash Received", "beginningPaymentBalance");
                dt = AddMapRow(dt, dg, "Current Cash Received", "beginningDeathBenefit");
                dt = AddMapRow(dt, dg, "Prior Unapplied Cash", "priorUnappliedCash");
                dt = AddMapRow(dt, dg, "Current Unapplied Cash", "currentUnappliedCash");
                dt = AddMapRow(dt, dg, "Death Benefit", "endingDeathBenefit");
                dt = AddMapRow(dt, dg, "Face Amount", "endingPaymentBalance");
                dt = AddMapRow(dt, dg, "Down Payments", "downPayments");
                dt = AddMapRow(dt, dg, "Payments", "payments");
                dt = AddMapRow(dt, dg, "Insured Name", "insuredName");
                dt = AddMapRow(dt, dg, "Policy Extract_Polilcy Status", "policyStatus");
                dt = AddMapRow(dt, dg, "Policy Extract_Polilcy Reason", "statusReason");
                dt = AddMapRow(dt, dg, "Policy Extract_Billing Reason", "billingReason");
                dt = AddMapRow(dt, dg, "Date Claim Processed", "deathPaidDate");
            }

            if ( dt.Rows.Count <= 0 )
            {
                return;
            }
            using (ViewDataTable viewForm = new ViewDataTable(dt, "Excel Col, MySQL Col, Caption"))
            {
                viewForm.Text = who + " Column Mapping";
                viewForm.ShowDialog();
            }
        }
        /***********************************************************************************************/
        private DataTable AddMapRow(DataTable dt, DevExpress.XtraGrid.Views.Grid.GridView dg, string from, string to )
        {
            int col = G1.get_column_number(dg, to);
            if (col < 0)
                return dt;

            string caption = dg.Columns[col].Caption.ObjToString();

            DataRow dRow = dt.NewRow();
            dRow["Excel Col"] = from;
            dRow["MySQL Col"] = to;
            dRow["Caption"] = caption;
            dt.Rows.Add(dRow);

            return dt;
        }
        /***********************************************************************************************/
        private void unityToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dgv.Visible)
                ShowMapTable("Unity", this.gridMain);
            else if (dgv4.Visible)
                ShowMapTable("Unity", this.gridMain4);
            else if (dgv6.Visible)
                ShowMapTable("Unity", this.gridMain6);
        }
        /***********************************************************************************************/
        private void chkLoadSMFS_CheckedChanged(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv6.DataSource;
            if (dt == null)
                return;
            if (dt.Rows.Count <= 0)
                return;

            dt = LoadSMFS(dt);

            dgv6.DataSource = dt;
            gridMain6.RefreshData();
            gridMain6.RefreshEditor(true);
            dgv6.Refresh();
        }
        /***********************************************************************************************/
        private void EditTrustData ( string trustCompany )
        {
            DataRow dr = gridMain2.GetFocusedDataRow();
            if (dr == null)
                return;

            string month = dr["month"].ObjToString();
            string year = dr["year"].ObjToString();
            string monthYear = month + " " + year;
            //using (TrustDataEdit tForm = new TrustDataEdit(trustCompany, monthYear))
            //{
            //    tForm.ShowDialog();
            //}
            TrustDataEdit tForm = new TrustDataEdit(trustCompany, monthYear);
            tForm.Show();
        }
        /***********************************************************************************************/
        private void editFDLICToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditTrustData("FDLIC");
        }
        /***********************************************************************************************/
        private void editSecurityNationalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditTrustData("Security National");
        }
        /***********************************************************************************************/
        private void editForethoughtToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditTrustData("Forethought");
        }
        /***********************************************************************************************/
        private void editCadenceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditTrustData("CD");
        }
        /***********************************************************************************************/
        private void editUnityToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditTrustData("Unity");
        }
        /***********************************************************************************************/
        private void cmbSelectColumns2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            System.Windows.Forms.ComboBox combo = (System.Windows.Forms.ComboBox)sender;
            string comboName = combo.Text;
            string skinName = "";
            if (!String.IsNullOrWhiteSpace(comboName))
            {
                SetupSelectedColumns("TrustDataTotals", comboName, dgv2);
                string name = "TrustDataTotals " + comboName;
                foundLocalPreference = G1.RestoreGridLayout(this, this.dgv2, gridMain2, LoginForm.username, name, ref skinName);
            }
            else
            {
                SetupSelectedColumns("TrustDataTotals", "Primary", dgv2);
                string name = "TrustDataTotals Primary";
                foundLocalPreference = G1.RestoreGridLayout(this, this.dgv2, gridMain2, LoginForm.username, name, ref skinName);
            }
        }
        /***********************************************************************************************/
        private void btnSelectColumn2_Click(object sender, EventArgs e)
        { // Select Columns
            string actualName = cmbSelectColumns2.Text;
            SelectColumns sform = new SelectColumns(dgv2, "TrustDataTotals", "Primary", actualName);
            sform.Done += new SelectColumns.d_void_eventdone(sform_DoneTotals);
            sform.Show();
        }
        /***********************************************************************************************/
        void sform_DoneTotals()
        {
            dgv2.Refresh();
            this.Refresh();
        }
        /***********************************************************************************************/
        private void showDeceasedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //DataRow dr = gridMain2.GetFocusedDataRow();
            //DataTable dt = (DataTable)dgv2.DataSource;

            //string month = dr["month"].ObjToString();
            //string year = dr["year"].ObjToString();
            //string preOrPost = cmbPreOrPost.Text.Trim();
            //string oldStuff = "NO";
            //if (chkOldStuff.Checked)
            //    oldStuff = "YES";

            //string nextDays = txtNextDays.Text;

            //TrustDataDeceased deceasedForm = new TrustDataDeceased( chkCmbCompany, workReport, month, year, preOrPost, oldStuff, dt, nextDays );
            //deceasedForm.Show();
        }
        /***********************************************************************************************/
        private void chkOldStuff_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox check = (CheckBox)sender;
            if (!check.Checked)
                return;

            cmbPreOrPost.Text = "Both";
            chkJustTrustsSelected.Checked = true;

            chkCmbCompany.CheckAll();
            chkCmbCompany.Refresh();

            cmbSelectColumns2.Text = "Trust Data Pre Totals";
            cmbSelectColumns2.SelectedItem = "Trust Data Pre Totals";
        }
        /***********************************************************************************************/
        private void pre2002ReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menu = (ToolStripMenuItem)sender;

            workReport = menu.Text;

            chkOldStuff.Checked = true;
            cmbPreOrPost.Text = "Both";
            chkJustTrustsSelected.Checked = true;

            chkCmbCompany.CheckAll();
            chkCmbCompany.Refresh();

            cmbSelectColumns2.Text = "Trust Data Pre Totals";
            cmbSelectColumns2.SelectedItem = "Trust Data Pre Totals";

            tabControl1.SelectTab("tabPage2");

            btnRunTotals_Click(null, null);
        }
        /***********************************************************************************************/
        private void post2002ReportSNFTToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menu = (ToolStripMenuItem)sender;

            workReport = menu.Text;

            chkOldStuff.Checked = false;
            cmbPreOrPost.Text = "Post";
            chkJustTrustsSelected.Checked = true;

            chkCmbCompany.DeselectAll();

            chkCmbCompany.SetEditValue("Security National|FORETHOUGHT");
 
            chkCmbCompany.Refresh();

            cmbSelectColumns2.Text = "SN/FT Post Totals";
            cmbSelectColumns2.SelectedItem = "SN/FT Post Totals";

            tabControl1.SelectTab("tabPage2");

            btnRunTotals_Click(null, null);
        }
        /***********************************************************************************************/
        private void post2002UnityReportToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            ToolStripMenuItem menu = (ToolStripMenuItem)sender;

            workReport = menu.Text;

            chkOldStuff.Checked = false;
            cmbPreOrPost.Text = "Post";
            chkJustTrustsSelected.Checked = true;

            chkCmbCompany.DeselectAll();

            chkCmbCompany.SetEditValue("Unity|Unity PB");

            chkCmbCompany.Refresh();

            cmbSelectColumns2.Text = "Unity Post Totals";
            cmbSelectColumns2.SelectedItem = "Unity Post Totals";

            tabControl1.SelectTab("tabPage2");

            btnRunTotals_Click(null, null);
        }
        /***********************************************************************************************/
        private void post2002ReportFDLICToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menu = (ToolStripMenuItem)sender;

            workReport = menu.Text;

            chkOldStuff.Checked = false;
            cmbPreOrPost.Text = "Post";
            chkJustTrustsSelected.Checked = true;

            chkCmbCompany.DeselectAll();

            chkCmbCompany.SetEditValue("FDLIC|FDLIC PB");

            chkCmbCompany.Refresh();

            cmbSelectColumns2.Text = "FDLIC Post Totals";
            cmbSelectColumns2.SelectedItem = "FDLIC Post Totals";

            tabControl1.SelectTab("tabPage2");

            btnRunTotals_Click(null, null);
        }
        /***********************************************************************************************/
        private void dateTimePicker2_ValueChanged(object sender, EventArgs e)
        {
            DateTime date = this.dateTimePicker2.Value;
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            DateTime stopDate = new DateTime(date.Year, date.Month, days);
            this.dateTimePicker3.Value = stopDate;
            this.dateTimePicker3.Refresh();
        }
        /***********************************************************************************************/
        private void gridMain_MouseDown(object sender, MouseEventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource; // Leave as a GetDate example

            var hitInfo = gridMain.CalcHitInfo(e.Location);
            if (hitInfo.InRowCell)
            {
                int rowHandle = hitInfo.RowHandle;
                gridMain.FocusedRowHandle = rowHandle;
                gridMain.SelectRow(rowHandle);
                gridMain.RefreshEditor(true);
                GridColumn column = hitInfo.Column;
                gridMain.FocusedColumn = column;
                string currentColumn = column.FieldName.Trim();
                if (currentColumn.ToUpper() == "REPORTDATE")
                {
                    DataRow dr = gridMain.GetFocusedDataRow();
                    DateTime date = dr["reportDate"].ObjToDateTime();
                    if (date.Year < 1000)
                    {
                        date = dr["date"].ObjToDateTime();
                        if (date.Year < 1000)
                            date = DateTime.Now;
                    }
                    string record = dr["record"].ObjToString();
                    using (GetDate dateForm = new GetDate(date, "Enter Report Date"))
                    {
                        dateForm.TopMost = true;
                        dateForm.ShowDialog();
                        if (dateForm.DialogResult == System.Windows.Forms.DialogResult.OK)
                        {
                            date = dateForm.myDateAnswer;
                            dr["reportDate"] = G1.DTtoMySQLDT(date);
                            if (!String.IsNullOrWhiteSpace(record))
                                G1.update_db_table("trust_data", "record", record, new string[] { "reportDate", date.ToString("yyyy-MM-dd")});
                            //DataChanged();
                            gridMain.ClearSelection();
                            gridMain.FocusedRowHandle = rowHandle;

                            gridMain.RefreshData();
                            gridMain.RefreshEditor(true);
                            gridMain.SelectRow(rowHandle);
                        }
                    }
                }
            }
        }
        /***********************************************************************************************/
        private void RunQuickDeceased()
        {
            DataRow dr = gridMain2.GetFocusedDataRow();
            DataTable dt = (DataTable)dgv2.DataSource;

            //string month = dr["month"].ObjToString();
            //string year = dr["year"].ObjToString();
            DateTime date = this.dateTimePicker2.Value;
            string month = date.ToString("MMMMMMMMMMMMM");
            string year = date.Year.ToString();

            string preOrPost = cmbPreOrPost.Text.Trim();
            string oldStuff = "NO";
            if (chkOldStuff.Checked)
                oldStuff = "YES";

            DataTable dx = new DataTable();
            dx.Columns.Add("month");
            dx.Columns.Add("Security National", Type.GetType("System.Double"));
            dx.Columns.Add("Forethought", Type.GetType("System.Double"));
            dx.Columns.Add("CD", Type.GetType("System.Double"));
            dx.Columns.Add("Unity", Type.GetType("System.Double"));
            dx.Columns.Add("Unity PB", Type.GetType("System.Double"));
            dx.Columns.Add("Unity DC", Type.GetType("System.Double"));
            dx.Columns.Add("Unity Total", Type.GetType("System.Double"));
            dx.Columns.Add("Unity Over/Under", Type.GetType("System.Double"));
            dx.Columns.Add("unityOldBarham", Type.GetType("System.Double"));
            dx.Columns.Add("unityOldWebb", Type.GetType("System.Double"));
            dx.Columns.Add("fdlicOldWebb", Type.GetType("System.Double"));
            dx.Columns.Add("fdlicOldCCI", Type.GetType("System.Double"));
            dx.Columns.Add("FDLIC", Type.GetType("System.Double"));
            dx.Columns.Add("FDLIC PB", Type.GetType("System.Double"));
            dx.Columns.Add("total", Type.GetType("System.Double"));

            dx.Columns.Add("unityCash", Type.GetType("System.Double"));
            dx.Columns.Add("unityOverUnder", Type.GetType("System.Double"));
            dx.Columns.Add("unityDeathBenefit", Type.GetType("System.Double"));

            dx.Columns.Add("fdlicCash", Type.GetType("System.Double"));
            dx.Columns.Add("fdlicOverUnder", Type.GetType("System.Double"));
            dx.Columns.Add("fdlicDeathBenefit", Type.GetType("System.Double"));
            dx.Columns.Add("year");

            string nextDays = txtNextDays.Text;

            TrustDataDeceased deceasedForm = new TrustDataDeceased(chkCmbCompany, workReport, month, year, preOrPost, oldStuff, dx, nextDays );
            deceasedForm.Show();
        }
        /***********************************************************************************************/
        private void post2002ReportAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menu = (ToolStripMenuItem)sender;

            workReport = menu.Text;

            chkOldStuff.Checked = false;
            cmbPreOrPost.Text = "Post";
            chkJustTrustsSelected.Checked = true;

            chkCmbCompany.DeselectAll();

            chkCmbCompany.SetEditValue("Unity|Unity PB|Security National|FORETHOUGHT|FDLIC|FDLIC PB");

            chkCmbCompany.Refresh();

            cmbSelectColumns2.Text = "Post 2002 All Totals";
            cmbSelectColumns2.SelectedItem = "Post 2002 All Totals";

            tabControl1.SelectTab("tabPage2");

            gridMain2.Columns["fdlicOldWebb"].Visible = false;
            gridMain2.Columns["fdlicOldCCI"].Visible = false;
            gridMain2.Columns["unityOldWebb"].Visible = false;
            gridMain2.Columns["unityOldBarham"].Visible = false;
            gridMain2.Columns["Unity DC"].Visible = false;
            gridMain2.Columns["Unity Over/Under"].Visible = false;
            gridMain2.Columns["Unity PB"].Visible = false;
            gridMain2.Columns["FDLIC PB"].Visible = false;
            gridMain2.Columns["Unity Total"].Visible = false;

            btnRunTotals_Click(null, null);
        }
        /***********************************************************************************************/
        private void gridMain2_CellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            if (e == null)
                return;
            int rowHandle = gridMain2.FocusedRowHandle;
            int row = gridMain2.GetDataSourceRowIndex(rowHandle);
            DataRow dr = gridMain2.GetFocusedDataRow();
            if (dr == null)
                return;

            string column = e.Column.FieldName.Trim();
            if (column.ToUpper() == "MONTH")
                return;

            DataTable dt = (DataTable)dgv2.DataSource;

            string month = dr["month"].ObjToString().ToUpper();
            if (String.IsNullOrWhiteSpace(month))
                return;
            string sYear = dr["year"].ObjToString();
            int year = sYear.ObjToInt32();
            int mm = G1.ConvertMonthToIndex(month);
            int days = DateTime.DaysInMonth(year, mm);
            DateTime date = new DateTime(year, mm, days);

            double balance = dr[column].ObjToDouble();
            string record = "";

            string cmd = "Select * from `trust_data_edits` where `trustName` = '" + column + "' and `status` = 'EndingBalance' AND `date` = '" + date.ToString("yyyy-MM-dd") + "';";
            DataTable dx = G1.get_db_data(cmd);
            if ( dx.Rows.Count <= 0 )
            {
                if (balance != -1D)
                {
                    record = G1.create_record("trust_data_edits", "status", "-1");
                    if (G1.BadRecord("trust_data_edits", record))
                        return;
                    G1.update_db_table("trust_data_edits", "record", record, new string[] { "status", "EndingBalance", "trustName", column, "beginningDeathBenefit", balance.ToString(), "date", date.ToString("yyyy-MM-dd") });
                }

            }
            else
            {
                record = dx.Rows[0]["record"].ObjToString();
                if (G1.BadRecord("trust_data_edits", record))
                    return;
                if (balance == -1D)
                    G1.delete_db_table("trust_data_edits", "record", record);
                else
                    G1.update_db_table("trust_data_edits", "record", record, new string[] { "status", "EndingBalance", "trustName", column, "beginningDeathBenefit", balance.ToString(), "date", date.ToString("yyyy-MM-dd") });
            }
        }
        /***********************************************************************************************/
        private void saveDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (1 == 1)
                return;
            DataTable dt = (DataTable)dgv2.DataSource;
            int row = 0;
            int[] rows = gridMain2.GetSelectedRows();
            int firstRow = 0;
            if (rows.Length > 0)
                firstRow = rows[0];
            try
            {
                string column = "";
                double balance = 0D;
                DateTime date = DateTime.Now;
                string cmd = "";
                DataTable dx = null;
                DataRow[] dRows = null;
                string record = "";
                string month = "";
                int mm = 0;
                string str = "";
                int days = 0;
                for (int i = 0; i < rows.Length; i++)
                {
                    row = rows[i];
                    month = dt.Rows[row]["month"].ObjToString();
                    mm = G1.ConvertMonthToIndex(month);
                    str = dt.Rows[row]["year"].ObjToString();
                    days = DateTime.DaysInMonth(str.ObjToInt32(), mm);
                    date = new DateTime(str.ObjToInt32(), mm, days);

                    cmd = "Select * from `trust_data_edits` WHERE `date` = '" + date.ToString("yyyy-MM-dd") + "' AND `status` = 'EndingBalance';";
                    dx = G1.get_db_data(cmd);
                    for ( int j=1; j<dt.Columns.Count; j++)
                    {
                        column = dt.Columns[j].ColumnName.Trim();
                        if (column.ToUpper() == "MONTH")
                            continue;
                        balance = dt.Rows[row][column].ObjToDouble();
                        dRows = dx.Select("trustName='" + column + "'");
                        if ( dRows.Length > 0 )
                        {
                            record = dRows[0]["record"].ObjToString();
                            if (G1.BadRecord("trust_data_edits", record))
                                return;
                            if (balance == -1D)
                                G1.delete_db_table("trust_data_edits", "record", record);
                            else
                                G1.update_db_table("trust_data_edits", "record", record, new string[] { "status", "EndingBalance", "trustName", column, "beginningDeathBenefit", balance.ToString(), "date", date.ToString("yyyy-MM-dd") });
                        }
                        else
                        {
                            if (balance != -1D)
                            {
                                record = G1.create_record("trust_data_edits", "status", "-1");
                                if (G1.BadRecord("trust_data_edits", record))
                                    return;
                                G1.update_db_table("trust_data_edits", "record", record, new string[] { "status", "EndingBalance", "trustName", column, "beginningDeathBenefit", balance.ToString(), "date", date.ToString("yyyy-MM-dd") });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("*ERROR*** " + ex.Message.ToString());
            }
        }
        /***********************************************************************************************/
    }
}