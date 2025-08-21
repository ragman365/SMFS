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
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class ImportTrustFile : DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        /* Forethough - Import All Active Data. This code determines Pre or Post */
        /* FDLIC - Import All - FDLIC PB is included all together
        /***********************************************************************************************/
        private DataTable workDt = null;
        private DataTable workDt2 = null;
        private string workWhat = "";
        private bool workDC = false;
        private string workContract = "";
        private string workCompany = "";
        private DateTime workDate = DateTime.Now;
        private string title = "";

        private DataTable problemDt = null;
        /***********************************************************************************************/
        public ImportTrustFile(string what)
        {
            InitializeComponent();
            //workDt = dt;
            workWhat = what;

            cmbTrust.Text = what;

            SetupTotalsSummary();

            chkShowAll.Hide();

            //LoadMonths();
            //LoadYears();
        }
        /***********************************************************************************************/
        public ImportTrustFile(string what, string company, string contractNumber, DateTime date )
        {
            InitializeComponent();
            workWhat = what;
            workCompany = company;
            workContract = contractNumber;
            workDate = date;

            cmbTrust.Hide();
            lblTrust.Hide();

            SetupTotalsSummary();
        }
        /****************************************************************************************/
        private void SetupTotalsSummary()
        {
            AddSummaryColumn("beginningPaymentBalance", null);
            AddSummaryColumn("beginningDeathBenefit", null);
            AddSummaryColumn("endingPaymentBalance", null);
            AddSummaryColumn("endingDeathBenefit", null);
            AddSummaryColumn("downPayments", null);
            AddSummaryColumn("Payments", null);
            AddSummaryColumn("growth", null);
            AddSummaryColumn("priorUnappliedCash", null);
            AddSummaryColumn("currentUnappliedCash", null);
            AddSummaryColumn("deathClaimAmount", null);
            AddSummaryColumn("reducedPaidUpAmount", null);

            AddSummaryColumn("beginningPaymentBalance", gridMain2 );
            AddSummaryColumn("beginningDeathBenefit", gridMain2 );
            AddSummaryColumn("endingPaymentBalance", gridMain2 );
            AddSummaryColumn("endingDeathBenefit", gridMain2 );
            AddSummaryColumn("downPayments", gridMain2);
            AddSummaryColumn("Payments", gridMain2);
            AddSummaryColumn("growth", gridMain2 );
            AddSummaryColumn("priorUnappliedCash", gridMain2 );
            AddSummaryColumn("currentUnappliedCash", gridMain2 );
            AddSummaryColumn("deathClaimAmount", gridMain2 );
            AddSummaryColumn("reducedPaidUpAmount", gridMain2);

            AddSummaryColumn("beginningPaymentBalance", gridMain3);
            AddSummaryColumn("beginningDeathBenefit", gridMain3);
            AddSummaryColumn("endingPaymentBalance", gridMain3);
            AddSummaryColumn("endingDeathBenefit", gridMain3);
            AddSummaryColumn("downPayments", gridMain3);
            AddSummaryColumn("Payments", gridMain3);
            AddSummaryColumn("growth", gridMain3);
            AddSummaryColumn("priorUnappliedCash", gridMain3);
            AddSummaryColumn("currentUnappliedCash", gridMain3);
            AddSummaryColumn("deathClaimAmount", gridMain3);
            AddSummaryColumn("reducedPaidUpAmount", gridMain3);

            AddSummaryColumn("beginningPaymentBalance", gridMain4);
            AddSummaryColumn("beginningDeathBenefit", gridMain4);
            AddSummaryColumn("endingPaymentBalance", gridMain4);
            AddSummaryColumn("endingDeathBenefit", gridMain4);
            AddSummaryColumn("downPayments", gridMain4);
            AddSummaryColumn("Payments", gridMain4);
            AddSummaryColumn("growth", gridMain4);
            AddSummaryColumn("priorUnappliedCash", gridMain4);
            AddSummaryColumn("currentUnappliedCash", gridMain4);
            AddSummaryColumn("deathClaimAmount", gridMain4);
            AddSummaryColumn("reducedPaidUpAmount", gridMain4);
        }
        /****************************************************************************************/
        private void AddSummaryColumn(string columnName, DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gMain = null, string format = "")
        {
            if (gMain == null)
                gMain = gridMain;

            if (String.IsNullOrWhiteSpace(format))
                format = "{0:N2}";
            gMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            gMain.Columns[columnName].SummaryItem.DisplayFormat = format;
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
        private string trustFileName = "";
        private void LoadMonths()
        {
            cmbMonth.Items.Clear();

            cmbMonth.Items.Add("January");
            cmbMonth.Items.Add("February");
            cmbMonth.Items.Add("March");
            cmbMonth.Items.Add("April");
            cmbMonth.Items.Add("May");
            cmbMonth.Items.Add("June");
            cmbMonth.Items.Add("July");
            cmbMonth.Items.Add("August");
            cmbMonth.Items.Add("September");
            cmbMonth.Items.Add("October");
            cmbMonth.Items.Add("November");
            cmbMonth.Items.Add("December");

            try
            {
                string name = workDt.TableName.Trim().ToUpper();
                name = name.Replace(workWhat.ToUpper(), "");
                name = name.Replace(".xlsx", "");
                name = name.Replace(".xls", "");
                name = name.Replace(".XLSX", "");
                name = name.Replace(".XLS", "");
                name = name.Trim();

                trustFileName = name;

                string str = "";
                bool gotit = false;
                string year = "";
                string month = "";
                string[] Lines = name.Split(' ');
                for ( int i=0; i<Lines.Length; i++)
                {
                    month = Lines[i].Trim();
                    for (int j = 0; j < cmbMonth.Items.Count; j++)
                    {
                        if ( month.ToUpper() == cmbMonth.Items[j].ObjToString().ToUpper().Trim() )
                        {
                            month = cmbMonth.Items[j].ObjToString().Trim();
                            gotit = true;
                            break;
                        }
                    }
                    if (gotit)
                    {
                        if (i < Lines.Length)
                            year = Lines[i + 1].ObjToString().Trim();
                        break;
                    }
                }
                month = G1.force_lower_line(month);
                if (String.IsNullOrWhiteSpace(year))
                {
                    for (int i = 0; i < cmbMonth.Items.Count; i++)
                    {
                        str = cmbMonth.Items[i].ObjToString().Trim();
                        if (month.IndexOf(str) == 0)
                        {
                            month = month.Replace(str, "");
                            if (!String.IsNullOrWhiteSpace(month))
                            {
                                year = month.Trim();
                                month = str;
                                break;
                            }
                            else
                            {
                                month = str;
                                break;
                            }
                        }
                    }
                }
                cmbMonth.Text = month;
                if (!String.IsNullOrWhiteSpace(year))
                {
                    int iyear = year.ObjToInt32();
                    if (iyear < 100)
                        iyear += 2000;
                    cmbYear.Text = iyear.ToString();
                }
            }
            catch (Exception ex)
            {
            }
        }
        /***********************************************************************************************/
        private void LoadYears()
        {
            try
            {
                cmbYear.Items.Clear();



                string name = workDt.TableName.Trim().ToUpper();
                name = name.Replace(workWhat.ToUpper(), "");
                name = name.Trim();

                name = name.Replace(".xlsx", "");
                name = name.Replace(".xls", "");
                name = name.Replace(".XLSX", "");
                name = name.Replace(".XLS", "");
                string year = cmbYear.Text.Trim();
                string str = "";
                string[] Lines = name.Split(' ');
                for (int i = 0; i < Lines.Length; i++)
                {
                    str = Lines[i].Trim();
                    if (G1.validate_numeric(str))
                    {
                        int iyear = str.ObjToInt32();
                        if (iyear < 100)
                            iyear += 2000;
                        cmbYear.Text = iyear.ToString();
                    }
                }

                str = year;

                int startYear = DateTime.Now.Year - 2;

                if (G1.validate_numeric(str))
                    startYear = str.ObjToInt32() - 2;

                for (int i = startYear; i <= startYear + 10; i++)
                {
                    cmbYear.Items.Add(i.ToString());
                }
            }
            catch (Exception ex)
            {
            }
        }
        /***********************************************************************************************/
        private void removeExtraTabs ()
        {
            tabControl1.TabPages.Remove(tabPage2);
            tabControl1.TabPages.Remove(tabPage3);
        }
        /***********************************************************************************************/
        private void ImportTrustFile_Load(object sender, EventArgs e)
        {
            if (workWhat == "CD")
                removeExtraTabs();

            gridMain.Columns["date"].Visible = false;

            btnSave.Hide();
            lblMonth.Hide();
            lblYear.Hide();
            cmbMonth.Hide();
            cmbYear.Hide();

            gridMain.Columns["found"].Visible = false;

            this.Text = "Import Trustee Data for " + workWhat;

            if (!String.IsNullOrWhiteSpace(workContract))
            {
                LoadContractData();
                chkShowAll.Show();
                chkShowAll.Refresh();
                gridMain.Columns["date"].Visible = true;
            }
        }
        /***********************************************************************************************/
        private void LoadContractData ()
        {
            int days = DateTime.DaysInMonth(workDate.Year, workDate.Month);
            DateTime date1 = new DateTime(workDate.Year, workDate.Month, 1);
            DateTime date2 = new DateTime(workDate.Year, workDate.Month, days);
            string cmd = "select * from `trust_data` where `trustCompany` LIKE '" + workCompany + "%' AND `contractNumber` = '" + workContract + "' AND `date` >= '" + date1.ToString("yyyy-MM-dd") + "' AND `date` <= '" + date2.ToString("yyyy-MM-dd") + "';";
            if ( chkShowAll.Checked )
                cmd = "select * from `trust_data` where `trustCompany` LIKE '" + workCompany + "%' AND `contractNumber` = '" + workContract + "' ORDER BY `date`;";
            DataTable dt = G1.get_db_data(cmd);
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;

            this.tabControl1.TabPages.Remove(tabPage2);
            this.tabControl1.TabPages.Remove(tabPage3);
            this.tabControl1.TabPages.Remove(tabPage4);

            btnImport.Hide();
            tabPage2.Hide();
            tabPage3.Hide();
            tabPage4.Hide();
        }
        /***********************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (dgv.Visible)
                SetSpyGlass(gridMain);
            else if ( dgv2.Visible )
                SetSpyGlass(gridMain2);
            else if (dgv3.Visible)
                SetSpyGlass(gridMain3);
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
                cmd = "Select * from `customers` where `contractNumber` = '" + contract + "';";
                dx = G1.get_db_data(cmd);
                if ( dx.Rows.Count <= 0 )
                {
                    MessageBox.Show("***ERROR*** Contract " + contract + "\nDoes Not Have a Customer File!\nBe sure to edit all Demographics", "Customer File Record Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
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
            if (!String.IsNullOrWhiteSpace(workContract))
            {
                btnSave.Show();
                btnSave.Refresh();
            }
        }
        /***********************************************************************************************/
        private void gridMain_CustomDrawCell(object sender, RowCellCustomDrawEventArgs e)
        {
            if (e.RowHandle < 0)
                return;
            if (e.Column.FieldName.ToUpper() == "NUM")
            {
                if (e.RowHandle >= 0)
                {
                    string num = (e.RowHandle + 1).ToString();
                    e.DisplayText = num;
                }
            }
        }
        /***********************************************************************************************/
        private DateTime DetermineDate ()
        {
            DateTime date = DateTime.Now;
            string date1 = "";
            string month = cmbMonth.Text;
            int year = cmbYear.Text.ObjToInt32();
            if (month.ToUpper() == "JANUARY")
                date = new DateTime(year, 1, 31);
            else if (month.ToUpper() == "FEBRUARY")
                date = new DateTime(year, 2, 28);
            else if (month.ToUpper() == "MARCH")
                date = new DateTime(year, 3, 31);
            else if (month.ToUpper() == "APRIL")
                date = new DateTime(year, 4, 30);
            else if (month.ToUpper() == "MAY")
                date = new DateTime(year, 5, 31);
            else if (month.ToUpper() == "JUNE")
                date = new DateTime(year, 6, 30);
            else if (month.ToUpper() == "JULY")
                date = new DateTime(year, 7, 31);
            else if (month.ToUpper() == "AUGUST")
                date = new DateTime(year, 8, 31);
            else if (month.ToUpper() == "SEPTEMBER")
                date = new DateTime(year, 9, 30);
            else if (month.ToUpper() == "OCTOBER")
                date = new DateTime(year, 10, 31);
            else if (month.ToUpper() == "NOVEMBER")
                date = new DateTime(year, 11, 30);
            else if (month.ToUpper() == "DECEMBER")
                date = new DateTime(year, 12, 31);
            return date;
        }
        /***********************************************************************************************/
        private void OldSaveDC ()
        {
            string contractNumber = "";
            string record = "";
            double deathClaimAmount = 0D;
            DataTable dt = (DataTable)dgv.DataSource;

            DateTime date = DetermineDate();
            DateTime deceasedDate = date;

            date = date.AddMonths(-1);
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            date = new DateTime(date.Year, date.Month, days);
            string date1 = date.ToString("yyyy-MM-dd");

            DataRow[] dRows = null;

            string cmd = "Select * from `trust_data` WHERE `date` = '" + date1 + "' AND `trustCompany` = '" + workWhat.Trim() + "';";
            DataTable dx = G1.get_db_data(cmd);

            for ( int i=0; i<dt.Rows.Count; i++)
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                dRows = dx.Select("contractNumber='" + contractNumber + "'");
                if (dRows.Length > 0)
                {
                    record = dRows[0]["record"].ObjToString();
                    deathClaimAmount = dt.Rows[i]["deathClaimAmount"].ObjToDouble();
                    date = dt.Rows[i]["deathPaidDate"].ObjToDateTime();
                    if (date.Year < 1000)
                        date = deceasedDate;
                    //G1.update_db_table("trust_data", "record", record, new string[] {"deathClaimAmount", deathClaimAmount.ToString(), "deathPaidDate", date.ToString("yyyy-MM-dd") });
                }
                else
                {
                    MessageBox.Show("***ERROR*** Deceased Contract Number (" + contractNumber + ")\ncannot be located in previous months data!", "Deceased Contract Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                }
            }
        }
        /***********************************************************************************************/
        private void SaveDC()
        {
            DateTime date = DetermineDate();
            DateTime deceasedDate = date;

            //if ( workWhat == "Security National")
            //    date = date.AddMonths(-1);
            DateTime firstDate = new DateTime(date.Year, date.Month, 1);

            int days = DateTime.DaysInMonth(date.Year, date.Month);
            date = new DateTime(date.Year, date.Month, days);
            DateTime lastDate = date;
            string date1 = date.ToString("yyyy-MM-dd");

            string contractNumber = "";
            string record = "";
            double deathClaimAmount = 0D;
            double reducedPaidUpAmount = 0D;
            double endingDeathBenefit = 0D;
            DateTime deathPaidDate = DateTime.Now;
            DataTable dt = (DataTable)dgv2.DataSource;
            if (dt == null)
            {
                if (workDC)
                    dt = (DataTable)dgv.DataSource;
                if (dt == null)
                    return;
            }
            else
            {
                if (workDC && dt.Rows.Count <= 0)
                    dt = (DataTable)dgv.DataSource;
            }

            string trustName = "";
            string policyNumber = "";
            string preOrPost = "";
            string cmd = "";
            DataTable dx = null;
            string trustCompany = "";
            bool gotReduced = false;
            bool gotInsuredName = false;
            string firstName = "";
            string lastName = "";
            string insuredName = "";
            if ( G1.get_column_number ( dt, "reducedPaidUpAmount") >= 0 )
                gotReduced = true;
            if (G1.get_column_number(dt, "insuredName") >= 0)
                gotInsuredName = true;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                    //if (String.IsNullOrWhiteSpace(contractNumber))
                    //    continue;
                    trustName = dt.Rows[i]["trustName"].ObjToString();
                    policyNumber = dt.Rows[i]["policyNumber"].ObjToString();
                    preOrPost = dt.Rows[i]["preOrPost"].ObjToString();
                    trustCompany = workWhat;
                    firstName = dt.Rows[i]["firstName"].ObjToString();
                    lastName = dt.Rows[i]["lastName"].ObjToString();
                    if ( gotInsuredName )
                        insuredName = dt.Rows[i]["insuredName"].ObjToString();
                    else
                    {
                        insuredName = firstName;
                        if (!String.IsNullOrWhiteSpace(firstName))
                            insuredName += " ";
                        insuredName += lastName;
                    }

                    if (policyNumber.ToUpper().IndexOf("PB") == 0)
                        trustCompany = "Unity DI";
                    else if (policyNumber.ToUpper().IndexOf("PIB") == 0)
                        trustCompany = "Unity PB";
                    else if (policyNumber.ToUpper().IndexOf("PSP") == 0)
                        trustCompany = "Unity PB";

                    if (G1.get_column_number(dt, "record") >= 0)
                        record = dt.Rows[i]["record"].ObjToString();
                    else
                    {
                        cmd = "Select * from `trust_data` WHERE `trustCompany` = '" + trustCompany + "' AND `trustName` = '" + trustName + "' AND `policyNumber` = '" + policyNumber + "' AND `date` = '" + date1 + "';";
                        dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count <= 0)
                        {
                            record = G1.create_record("trust_data", "status", "-1");
                            if (G1.BadRecord("trust_data", record))
                                continue;
                            G1.update_db_table("trust_data", "record", record, new string[] { "date", date1, "status", "", "contractNumber", contractNumber, "policyNumber", policyNumber, "preOrPost", preOrPost, "trustCompany", trustCompany, "trustName", trustName, "firstName", firstName, "lastName", lastName, "insuredName", insuredName });
                            //continue;
                        }
                        else
                            record = dx.Rows[0]["record"].ObjToString();
                    }


                    deathClaimAmount = dt.Rows[i]["deathClaimAmount"].ObjToDouble();
                    endingDeathBenefit = dt.Rows[i]["endingDeathBenefit"].ObjToDouble();
                    if ( gotReduced )
                        reducedPaidUpAmount = dt.Rows[i]["reducedPaidUpAmount"].ObjToDouble();
                    deathPaidDate = dt.Rows[i]["deathPaidDate"].ObjToDateTime();
                    if ((deathPaidDate >= firstDate && deathPaidDate <= lastDate) || preOrPost.ToUpper() == "PRE" )
                    {
                        if (!String.IsNullOrWhiteSpace(record))
                            G1.update_db_table("trust_data", "record", record, new string[] { "deathClaimAmount", deathClaimAmount.ToString(), "deathPaidDate", deathPaidDate.ToString("yyyy-MM-dd"), "reducedPaidUpAmount", reducedPaidUpAmount.ToString(), "endingDeathBenefit", deathClaimAmount.ToString(), "firstName", firstName, "lastName", lastName  });
                        else
                            MessageBox.Show("***ERROR*** Deceased Contract Number (" + contractNumber + ")\ncannot be located in previous months data!", "Deceased Contract Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    }
                }
                catch ( Exception ex )
                {
                }
            }
        }
        /***********************************************************************************************/
        private void btnSave_Click(object sender, EventArgs e)
        {
            if ( workDC )
            {
                SaveDC();
                return;
            }
            DataTable dt = (DataTable)dgv.DataSource;

            if (G1.get_column_number(dt, "totalPaidInAtClaim") < 0)
                dt.Columns.Add("totalPaidInAtClaim", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "reducedPaidUpAmount") < 0)
                dt.Columns.Add("reducedPaidUpAmount", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "billingReason") < 0)
                dt.Columns.Add("billingReason");
            if (G1.get_column_number(dt, "statusReason") < 0)
                dt.Columns.Add("statusReason");
            if (G1.get_column_number(dt, "policyStatus") < 0)
                dt.Columns.Add("policyStatus");
            if (G1.get_column_number(dt, "tab") < 0)
                dt.Columns.Add("tab");


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
            double deathClaimAmount = 0D;
            double reducedPaidUp = 0D;
            string deathPaidDate = "";

            string insuredName = "";
            string lastName = "";
            string firstName = "";
            string middleName = "";

            DataTable myDt = new DataTable();
            string cName = "";
            string type = "";
            string str = "";
            double dValue = 0D;
            DataRow dRow = null;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dRow = myDt.NewRow();
                myDt.Rows.Add(dRow);
            }

            for ( int i=0; i<gridMain.Columns.Count; i++)
            {
                cName = gridMain.Columns[i].FieldName.Trim();
                if (G1.get_column_number(dt, cName) < 0)
                    dt.Columns.Add(cName);

                type = dt.Columns[cName].DataType.ObjToString();
                if (type == "System.String")
                    myDt.Columns.Add(cName);
                else if (type == "System.Double")
                    myDt.Columns.Add(cName, Type.GetType("System.Double"));
                else
                {
                }

                G1.copy_dt_column(dt, cName, myDt, cName);
            }

            if (G1.get_column_number(myDt, "tab") < 0)
                myDt.Columns.Add("tab");

            CleanupCommas(myDt, "beginningPaymentBalance");
            CleanupCommas(myDt, "beginningDeathBenefit");
            CleanupCommas(myDt, "endingPaymentBalance");
            CleanupCommas(myDt, "endingDeathBenefit");
            CleanupCommas(myDt, "downPayments");
            CleanupCommas(myDt, "Payments");
            CleanupCommas(myDt, "growth");
            CleanupCommas(myDt, "priorUnappliedCash");
            CleanupCommas(myDt, "currentUnappliedCash");
            CleanupCommas(myDt, "deathClaimAmount");
            CleanupCommas(myDt, "reducedPaidUpAmount");
            CleanupCommas(myDt, "totalPaidInAtClaim");

            try
            {
                DateTime date = DateTime.Now;
                string date1 = "";
                string month = cmbMonth.Text;
                int year = cmbYear.Text.ObjToInt32();
                if (month.ToUpper() == "JANUARY")
                    date = new DateTime(year, 1, 31);
                else if (month.ToUpper() == "FEBRUARY")
                {
                    int days = DateTime.DaysInMonth(year, 2);
                    date = new DateTime(year, 2, days);
                }
                else if (month.ToUpper() == "MARCH")
                    date = new DateTime(year, 3, 31);
                else if (month.ToUpper() == "APRIL")
                    date = new DateTime(year, 4, 30);
                else if (month.ToUpper() == "MAY")
                    date = new DateTime(year, 5, 31);
                else if (month.ToUpper() == "JUNE")
                    date = new DateTime(year, 6, 30);
                else if (month.ToUpper() == "JULY")
                    date = new DateTime(year, 7, 31);
                else if (month.ToUpper() == "AUGUST")
                    date = new DateTime(year, 8, 31);
                else if (month.ToUpper() == "SEPTEMBER")
                    date = new DateTime(year, 9, 30);
                else if (month.ToUpper() == "OCTOBER")
                    date = new DateTime(year, 10, 31);
                else if (month.ToUpper() == "NOVEMBER")
                    date = new DateTime(year, 11, 30);
                else if (month.ToUpper() == "DECEMBER")
                    date = new DateTime(year, 12, 31);

                if (1 == 1 && String.IsNullOrWhiteSpace(workContract ) )
                {
                    this.Cursor = Cursors.WaitCursor;
                    SaveData( myDt, date );
                    //if ( workWhat.ToUpper() != "UNITY")
                        SaveDC();
                    btnSave.Hide();
                    btnSave.Refresh();
                    this.Cursor = Cursors.Default;

                    MessageBox.Show("***INFO*** Saving Imported Data Complete!", "Save Data Finsher Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);

                    return;
                }

                this.Cursor = Cursors.WaitCursor;

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    trustName = dt.Rows[i]["trustName"].ObjToString();
                    policyNumber = dt.Rows[i]["policyNumber"].ObjToString();
                    preOrPost = dt.Rows[i]["preOrPost"].ObjToString();

                    record = dt.Rows[i]["record"].ObjToString();

                    date = dt.Rows[i]["date"].ObjToDateTime();

                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                    premium = dt.Rows[i]["beginningPaymentBalance"].ObjToDouble();
                    surrender = dt.Rows[i]["beginningDeathBenefit"].ObjToDouble();
                    faceAmount = dt.Rows[i]["endingPaymentBalance"].ObjToDouble();
                    deathBenefit = dt.Rows[i]["endingDeathBenefit"].ObjToDouble();
                    deathClaimAmount = dt.Rows[i]["deathClaimAmount"].ObjToDouble();
                    reducedPaidUp = dt.Rows[i]["reducedPaidUpAmount"].ObjToDouble();
                    deathPaidDate = dt.Rows[i]["deathPaidDate"].ObjToString();

                    downPayments = dt.Rows[i]["downPayments"].ObjToDouble();
                    payments = dt.Rows[i]["Payments"].ObjToDouble();
                    if ( payments > 0D )
                    {
                    }

                    //growth = deathBenefit - surrender - payments;

                    growth = dt.Rows[i]["growth"].ObjToDouble();

                    insuredName = dt.Rows[i]["insuredName"].ObjToString();
                    firstName = dt.Rows[i]["firstName"].ObjToString();
                    lastName = dt.Rows[i]["lastName"].ObjToString();
                    middleName = dt.Rows[i]["middleName"].ObjToString();

                    G1.update_db_table("trust_data", "record", record, new string[] { "status", "", "preOrPost", preOrPost, "date", date.ToString("yyyyMMdd"), "trustCompany", company, "trustName", trustName, "contractNumber", contractNumber, "policyNumber", policyNumber, "beginningPaymentBalance", premium.ToString(), "beginningDeathBenefit", surrender.ToString(), "endingPaymentBalance", faceAmount.ToString(), "endingDeathBenefit", deathBenefit.ToString(), "insuredName", insuredName, "lastName", lastName, "firstName", firstName, "middleName", middleName, "downPayments", downPayments.ToString(), "Payments", payments.ToString(), "growth", growth.ToString(), "deathClaimAmount", deathClaimAmount.ToString(), "deathPaidDate", deathPaidDate, "reducedPaidUpAmount", reducedPaidUp.ToString() });
                }
                this.Cursor = Cursors.Default;
                btnSave.Hide();
            }
            catch (Exception ex)
            {
            }
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void CleanupCommas ( DataTable dt, string column )
        {
            string str = "";
            if (G1.get_column_number(dt, column) < 0)
                return;

            for ( int i=0; i<dt.Rows.Count; i++)
            {
                str = dt.Rows[i][column].ObjToString();
                if ( str.IndexOf ( "$") >= 0 )
                {
                    str = str.Replace("$", "");
                    dt.Rows[i][column] = str;
                }
                if (String.IsNullOrWhiteSpace(str))
                    dt.Rows[i][column] = "0";
                else if ( str.IndexOf ( ",") > 0 )
                {
                    str = str.Replace(",", "");
                    dt.Rows[i][column] = str;
                }
            }
        }
        /***********************************************************************************************/
        private string determineWorkWhat ( string filename, ref string sheetName )
        {
            string search = "";
            sheetName = "";
            filename = filename.ToUpper();
            if (filename.IndexOf(workWhat.ToUpper()) < 0)
                filename = workWhat.ToUpper() + " " + filename;
            if ( filename.IndexOf ( "UNITY") >= 0 )
            {
                search = "FH Name";
                workWhat = "Unity";
                sheetName = "List of all policies";
            }
            else if (filename.IndexOf("FORETHOUGHT") >= 0)
            {
                search = "Insured Last Name";
                workWhat = "Forethought";
            }
            else if (filename.IndexOf("FDLIC") >= 0)
            {
                search = "FH No.";
                search = "Funeral Home";
                workWhat = "FDLIC";
            }
            else if (filename.IndexOf("SECURITY NATIONAL") >= 0)
            {
                search = "TRUST#";
                workWhat = "Security National";
            }
            else if (filename.IndexOf(" CD") >= 0)
            {
                search = "FIRST NAME";
                workWhat = "CD";
            }
            return search;
        }
        /***********************************************************************************************/
        private string actualFile = "";
        private string importedFile = "";
        private void btnImport_Click(object sender, EventArgs e)
        {
            string search = "TRUST#";
            //if (workWhat == "FDLIC")
            //    search = "FH No.";
            if (workWhat == "FDLIC")
                search = "Funeral Home";
            else if (workWhat == "Unity")
                search = "FH Name";
            else if (workWhat == "Forethought")
                search = "Insured Last Name";
            else if (workWhat == "CD")
                search = "contractNumber";

            this.Cursor = Cursors.WaitCursor;
            string sheetName = "";
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string file = ofd.FileName;
                    importedFile = file;
                    int idx = file.LastIndexOf("\\");
                    if (idx > 0)
                    {
                        actualFile = file.Substring(idx);
                        actualFile = actualFile.Replace("\\", "");
                    }

                    workDC = false;
                    search = determineWorkWhat(actualFile, ref sheetName );
                    if (actualFile.ToUpper().IndexOf(" DC") > 0)
                        workDC = true;
                    else if (actualFile.ToUpper().IndexOf(" DECEASED") > 0)
                        workDC = true;

                    dgv.DataSource = null;
                    this.Cursor = Cursors.WaitCursor;
                    workDt = null;
                    workDt2 = null;
                    try
                    {
                        workDt = ExcelWriter.ReadFile2(file, 0, sheetName );
                        if (workDt == null )
                        {
                            MessageBox.Show("***ERROR*** Cannot Locate " + sheetName + " in Excel File!!", "Excel Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                            this.Cursor = Cursors.Default;
                            return;
                        }

                        workDt.TableName = actualFile;
                        LoadMonths();
                        LoadYears();

                        if ( workWhat == "FDLIC")
                        {
                            workDt2 = ExcelWriter.ReadFile2(file, 0, "Newton County South");
                            if (workDt2 == null)
                            {
                                workDt2 = ExcelWriter.ReadFile2(file, 0, "Pre 2002");
                                if ( workDt2 == null )
                                    workDt2 = ExcelWriter.ReadFile2(file, 0, "Newton County (South)");
                            }
                            if ( workDt2 == null)
                            {
                                MessageBox.Show("***ERROR*** Cannot Locate FDLIC Newton County South!", "FDLIC Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                            }
                        }

                        workDt = ProcessTheData(workDt, workDt2 );
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("***ERROR*** Cannot Locate " + sheetName + " in Excel File!!", "Excel Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                        this.Cursor = Cursors.Default;
                        return;
                    }
                    workDt.TableName = actualFile;

                    string trustNumber = "";

                    string title = workDt.TableName.Trim();
                    title = title.Replace(".csv", "");

                    tabPage1.Text = title;
                    this.Text = title;
                    string downPayment = "";

                    LoadMonths();
                    LoadYears();

                    if (workDC)
                        workDt = FindLastActive(workDt, gridMain );

                    this.Text = workWhat + " for " + cmbMonth.Text + ", " + cmbYear.Text;

                    if (G1.get_column_number(workDt, "num") < 0)
                        workDt.Columns.Add("num");
                    G1.NumberDataTable(workDt);
                    dgv.DataSource = workDt;

                    //try
                    //{
                    //    DataTable ttDt = (DataTable)dgv.DataSource;

                    //    DataRow[] dRows = ttDt.Select("Payments<>''");
                    //    if (dRows.Length > 0)
                    //    {
                    //        try
                    //        {
                    //            DataTable dddd = dRows.CopyToDataTable();
                    //        }
                    //        catch (Exception ex)
                    //        {
                    //        }
                    //    }
                    //}
                    //catch ( Exception ex )
                    //{
                    //}

                    btnSave.Show();
                    btnSave.Refresh();
                    lblMonth.Show();
                    lblYear.Show();
                    cmbMonth.Show();
                    cmbYear.Show();
                }
            }
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private DataTable ProcessTheData ( DataTable dt, DataTable dt2 )
        {
            if (workWhat.Trim() == "Security National")
                dt = PreProcessSecurityNational(dt);
            else if (workWhat.Trim() == "FDLIC")
            {
                dt = PreProcessFDLIC(dt);
                if (dt2 != null)
                {
                    dt2 = PreProcessFDLIC(dt2);
                    for (int i = 0; i < dt2.Rows.Count; i++)
                        dt2.Rows[i]["preOrPost"] = "Pre";
                    dt.Merge(dt2);
                }
            }
            else if (workWhat.Trim() == "Forethought")
                dt = PreProcessForethought(dt);
            else if (workWhat.Trim() == "CD")
                dt = PreProcessCD(dt);
            else if (workWhat.Trim() == "Unity")
                dt = PreProcessUnity(dt);

            if (G1.get_column_number(dt, "num") < 0)
                dt.Columns.Add("num");

            G1.NumberDataTable(dt);
            return dt;
        }
        /***********************************************************************************************/
        private DataTable FindLastActive ( DataTable dt, DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gridMain  )
        {
            DateTime date = DetermineDate();
            date = date.AddMonths(-1);
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            date = new DateTime(date.Year, date.Month, days);
            string date1 = date.ToString("yyyy-MM-dd");

            if (G1.get_column_number(dt, "contractNumber") < 0)
                dt.Columns.Add("contractNumber");
            if (G1.get_column_number(dt, "trustName") < 0)
                dt.Columns.Add("trustName");

            gridMain.Columns["found"].Visible = true;
            if (G1.get_column_number(dt, "found") < 0)
                dt.Columns.Add("found");

            problemDt = dt.Clone();

            string cmd = "Select * from `trust_data` WHERE `date` = '" + date1 + "' AND `trustCompany` = '" + workWhat.Trim() + "';";
            DataTable dx = G1.get_db_data(cmd);

            string policyNumber = "";
            string trustName = "";
            string contractNumber = "";
            DataRow[] dRows = null;

            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    policyNumber = dt.Rows[i]["policyNumber"].ObjToString();
                    dRows = dx.Select("policyNumber='" + policyNumber + "'");
                    if (dRows.Length > 0)
                    {
                        dt.Rows[i]["contractNumber"] = dRows[0]["contractNumber"].ObjToString();
                        dt.Rows[i]["trustName"] = dRows[0]["trustName"].ObjToString();
                    }
                    else
                    {
                        contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                        if (!String.IsNullOrWhiteSpace(contractNumber))
                        {
                            dRows = dx.Select("contractNumber='" + contractNumber + "'");
                            if (dRows.Length > 0)
                            {
                                dt.Rows[i]["contractNumber"] = dRows[0]["contractNumber"].ObjToString();
                                dt.Rows[i]["trustName"] = dRows[0]["trustName"].ObjToString();
                            }
                            else
                            {
                                problemDt.ImportRow(dt.Rows[i]);
                                dt.Rows[i]["found"] = "ERROR";
                            }
                        }
                        else
                        {
                            problemDt.ImportRow(dt.Rows[i]);
                            dt.Rows[i]["found"] = "ERROR";
                        }
                    }
                }

                gridMain.Columns["contractNumber"].Caption = "Trust Number Found";
                gridMain.Columns["preOrPost"].Visible = false;
                gridMain.Columns["downPayments"].Visible = false;
                gridMain.Columns["Payments"].Visible = false;
                gridMain.Columns["priorUnappliedCash"].Visible = false;
                gridMain.Columns["currentUnappliedCash"].Visible = false;
            }
            catch ( Exception ex)
            {
            }

            return dt;
        }
        /***********************************************************************************************/
        private DataTable PreProcessSecurityNational ( DataTable dt )
        {
            int firstRow = -1;
            bool newFormat = false;
            string search = "INSURED NAME";
            string str = "";
            DataTable newDt = dt.Clone();

            for ( int i=0; i<dt.Rows.Count; i++)
            {
                for ( int j=0; j<dt.Columns.Count; j++)
                {
                    str = dt.Rows[i][j].ObjToString().ToUpper();
                    if ( str == search )
                    {
                        firstRow = i;
                        search = "DEBIT";
                        break;
                    }
                }
                if (firstRow >= 0)
                    break;
            }
            if ( firstRow < 0 )
            {
                search = "INSUREDFIRSTNAME";
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        str = dt.Rows[i][j].ObjToString().ToUpper();
                        if (str == search)
                        {
                            newFormat = true;
                            firstRow = i;
                            search = "POLICYNUMBER";
                            break;
                        }
                    }
                    if (firstRow >= 0)
                        break;
                }

            }
            if (firstRow < 0)
                return newDt;
            if (!newFormat)
            {
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    str = dt.Rows[firstRow][i].ObjToString();
                    if (String.IsNullOrWhiteSpace(str))
                        continue;
                    if (G1.get_column_number(dt, str) >= 0)
                        str = str + "2";
                    newDt.Columns[i].ColumnName = str;
                    newDt.Columns[i].Caption = str;

                    dt.Columns[i].ColumnName = str;
                    dt.Columns[i].Caption = str;
                }
                for (int i = (firstRow + 1); i < dt.Rows.Count; i++)
                {
                    str = dt.Rows[i][search].ObjToString();
                    if (String.IsNullOrWhiteSpace(str))
                        continue;
                    if (!G1.validate_numeric(str))
                        continue;
                    newDt.ImportRow(dt.Rows[i]);
                }
            }
            else
            {
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    str = dt.Rows[firstRow][i].ObjToString();
                    if (String.IsNullOrWhiteSpace(str))
                        continue;
                    if (G1.get_column_number(dt, str) >= 0)
                        str = str + "2";
                    newDt.Columns[i].ColumnName = str;
                    newDt.Columns[i].Caption = str;

                    dt.Columns[i].ColumnName = str;
                    dt.Columns[i].Caption = str;
                }
                for (int i = (firstRow + 1); i < dt.Rows.Count; i++)
                {
                    str = dt.Rows[i]["PolicyNumber"].ObjToString();
                    if (String.IsNullOrWhiteSpace(str))
                        continue;
                    //if (!G1.validate_numeric(str))
                    //    continue;
                    newDt.ImportRow(dt.Rows[i]);
                }
            }

            if (workDC)
            {
                if (newFormat)
                    newDt = mapSecurityNationalNewDC(newDt);
                else
                    newDt = mapSecurityNationalDC(newDt);
            }
            else
            {
                if ( newFormat )
                    newDt = mapSecurityNationalNew(newDt);
                else
                    newDt = mapSecurityNational(newDt);
            }

            LoadSecurityNationalDC();

            return newDt;
        }
        /***********************************************************************************************/
        private void LoadSecurityNationalDC ()
        {
            bool newFormat = false;
            string filename = importedFile.Trim();
            if (filename.ToUpper().IndexOf("ACTIVE") < 0)
                return;
            filename = filename.ToUpper().Replace("ACTIVE", "DC");

            string originalDCfile = filename;
            if (!File.Exists(filename))
            {
                if (filename.ToUpper().IndexOf(".XLSX") > 0)
                {
                    filename = filename.ToUpper().Replace(".XLSX", ".XLS");
                    if (!File.Exists(filename))
                    {
                        MessageBox.Show("***ERROR*** Cannot locate Deceased file " + originalDCfile + "!", "Import Sec Nat Deceased Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                        return;
                    }
                }
                else if (filename.ToUpper().IndexOf(".XLS") > 0)
                {
                    filename = filename.ToUpper().Replace(".XLS", ".XLSX");
                    if (!File.Exists(filename))
                    {
                        MessageBox.Show("***ERROR*** Cannot locate Deceased file " + originalDCfile + "!", "Import Sec Nat Deceased Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("***ERROR*** Cannot locate Deceased file " + filename + "!", "Import Sec Nat Deceased Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    return;
                }
            }

            DataTable dt = ExcelWriter.ReadFile2(filename);
            if (dt.Rows.Count <= 0)
                return;

            int firstRow = -1;
            string search = "INSURED NAME";
            string str = "";
            DataTable newDt = dt.Clone();

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    str = dt.Rows[i][j].ObjToString().ToUpper();
                    if (str == search)
                    {
                        firstRow = i;
                        search = "DEBIT";
                        break;
                    }
                }
                if (firstRow >= 0)
                    break;
            }
            if (firstRow < 0)
            {
                search = "INSUREDFIRSTNAME";
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        str = dt.Rows[i][j].ObjToString().ToUpper();
                        if (str == search)
                        {
                            newFormat = true;
                            firstRow = i;
                            search = "POLICYNUMBER";
                            break;
                        }
                    }
                    if (firstRow >= 0)
                        break;
                }

            }
            if (firstRow < 0)
                return;
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                str = dt.Rows[firstRow][i].ObjToString();
                if (String.IsNullOrWhiteSpace(str))
                    continue;
                if (G1.get_column_number(dt, str) >= 0)
                    str = str + "2";
                newDt.Columns[i].ColumnName = str;
                newDt.Columns[i].Caption = str;

                dt.Columns[i].ColumnName = str;
                dt.Columns[i].Caption = str;
            }
            for (int i = (firstRow + 1); i < dt.Rows.Count; i++)
            {
                str = dt.Rows[i][search].ObjToString();
                if (String.IsNullOrWhiteSpace(str))
                    continue;
                //if (!G1.validate_numeric(str))
                //    continue;
                newDt.ImportRow(dt.Rows[i]);
            }

            workDC = true;


            if ( newFormat )
                newDt = mapSecurityNationalNewDC(newDt);
            else
                newDt = mapSecurityNationalDC(newDt);

            newDt = FindLastActive(newDt, gridMain2);

            str = actualFile.Trim();
            str = str.Replace("ACTIVE", "DC");
            tabPage2.Text = str;

            newDt = AddNumColumn(newDt);

            dgv2.DataSource = newDt;

            DateTime date = DetermineDate();
            date = date.AddMonths(-1);
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            date = new DateTime(date.Year, date.Month, days);
            string date1 = date.ToString("yyyy-MM-dd");

            string cmd = "Select * from `trust_data` WHERE `date` = '" + date1 + "' AND `trustCompany` = '" + workWhat.Trim() + "';";
            DataTable dx = G1.get_db_data(cmd);
            //if (dx.Rows.Count > 0)
            //    dgv3.DataSource = dx;

            string policyNumber = "";
            DataRow[] dRows = null;
            DateTime deathPaidDate = DateTime.Now;
            double deathClaimAmount = 0D;

            for ( int i = 0; i<newDt.Rows.Count; i++)
            {
                policyNumber = newDt.Rows[i]["policyNumber"].ObjToString();
                deathPaidDate = newDt.Rows[i]["deathPaidDate"].ObjToDateTime();
                deathClaimAmount = newDt.Rows[i]["deathClaimAmount"].ObjToDouble();
                if ( !String.IsNullOrWhiteSpace ( policyNumber ))
                {
                    dRows = dx.Select("policyNumber='" + policyNumber + "'");
                    if (dRows.Length > 0)
                    {
                        dRows[0]["deathPaidDate"] = deathPaidDate.ToString("yyyy-MM-dd");
                        dRows[0]["deathClaimAmount"] = deathClaimAmount;
                    }
                    else
                        problemDt.ImportRow(newDt.Rows[i]);
                }
                else
                    problemDt.ImportRow(newDt.Rows[i]);
            }
            if (dx.Rows.Count > 0)
            {
                dx = AddNumColumn(dx);
                dgv3.DataSource = dx;
            }

            tabPage3.Text = date1 + " Last Month";

            tabControl1.TabPages.Remove(tabPage4);
            if ( problemDt.Rows.Count > 0 )
            {
                problemDt = AddNumColumn(problemDt);
                dgv4.DataSource = problemDt;
                gridMain4.RefreshData();
                gridMain4.RefreshEditor(true);
                dgv4.Refresh();
                tabControl1.TabPages.Add(tabPage4);
            }

            workDC = false;
        }
        /***********************************************************************************************/
        private DataTable LoadLastMonth ( DataTable newDt, DataTable currentDt = null )
        {
            DateTime date = DetermineDate();
            DateTime importDate = date;
            date = date.AddMonths(-1);
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            date = new DateTime(date.Year, date.Month, days);
            string date1 = date.ToString("yyyy-MM-dd");

            string cmd = "Select * from `trust_data` WHERE `date` = '" + date1 + "' AND `trustCompany` = '" + workWhat.Trim() + "';";
            if ( workWhat.Trim().ToUpper() == "FDLIC")
                cmd = "Select * from `trust_data` WHERE `date` = '" + date1 + "' AND ( `trustCompany` = '" + workWhat.Trim() + "' OR `trustCompany` = 'FDLIC PB' );";
            else if (workWhat.Trim().ToUpper() == "UNITY")
                cmd = "Select * from `trust_data` WHERE `date` = '" + date1 + "' AND `trustCompany` LIKE '" + workWhat.Trim() + "%';";
            DataTable dx = G1.get_db_data(cmd);

            string policyNumber = "";
            DataRow[] dRows = null;
            DataRow[] cRows = null;
            DateTime deathPaidDate = DateTime.Now;
            double deathClaimAmount = 0D;
            double reducedPaidUpAmount = 0D;
            string contractNumber = "";
            string statusReason = "";
            string billingReason = "";

            if (G1.get_column_number(newDt, "reducedPaidUpAmount") < 0)
                newDt.Columns.Add("reducedPaidUpAmount", Type.GetType("System.Double"));

            problemDt = newDt.Clone();

            for (int i = 0; i < newDt.Rows.Count; i++)
            {
                try
                {
                    policyNumber = newDt.Rows[i]["policyNumber"].ObjToString();
                    deathPaidDate = newDt.Rows[i]["deathPaidDate"].ObjToDateTime();
                    deathClaimAmount = newDt.Rows[i]["deathClaimAmount"].ObjToDouble();
                    reducedPaidUpAmount = newDt.Rows[i]["reducedPaidUpAmount"].ObjToDouble();
                    contractNumber = newDt.Rows[i]["contractNumber"].ObjToString();
                    if ( workWhat.Trim().ToUpper() == "UNITY")
                    {
                        if ( contractNumber == "HT16021UI")
                        {
                        }
                        deathPaidDate = importDate;
                        statusReason = newDt.Rows[i]["statusReason"].ObjToString().ToUpper();
                        billingReason = newDt.Rows[i]["billingReason"].ObjToString().ToUpper();
                        if (statusReason.Trim() == "DC")
                            deathClaimAmount = newDt.Rows[i]["endingDeathBenefit"].ObjToDouble();
                        if (reducedPaidUpAmount > 0D)
                            deathClaimAmount = reducedPaidUpAmount;
                    }

                    if (!String.IsNullOrWhiteSpace(policyNumber))
                    {
                        dRows = dx.Select("policyNumber='" + policyNumber + "'");
                        if (dRows.Length > 0)
                        {
                            if (workWhat.Trim().ToUpper() == "UNITY")
                            {
                                reducedPaidUpAmount = 0D;
                                deathClaimAmount = 0D;
                                if (billingReason.Trim() == "RU")
                                {
                                    reducedPaidUpAmount = dRows[0]["endingDeathBenefit"].ObjToDouble();
                                    if (currentDt != null)
                                    {
                                        cRows = currentDt.Select("policyNumber='" + policyNumber + "'");
                                        if (cRows.Length > 0)
                                            cRows[0]["endingPaymentBalance"] = reducedPaidUpAmount;
                                    }
                                }
                                else if (statusReason.Trim().ToUpper() == "DC")
                                    deathClaimAmount = dRows[0]["endingDeathBenefit"].ObjToDouble();
                                newDt.Rows[i]["deathPaidDate"] = deathPaidDate.ToString("yyyy-MM-dd");
                                newDt.Rows[i]["deathClaimAmount"] = deathClaimAmount;
                                newDt.Rows[i]["reducedPaidUpAmount"] = reducedPaidUpAmount;
                            }
                            dRows[0]["deathPaidDate"] = deathPaidDate.ToString("yyyy-MM-dd");
                            dRows[0]["deathClaimAmount"] = deathClaimAmount;
                            dRows[0]["reducedPaidUpAmount"] = reducedPaidUpAmount;
                            if ( contractNumber.Trim().ToUpper() == "NULL")
                                problemDt.ImportRow(newDt.Rows[i]);
                        }
                        else
                        {
                            problemDt.ImportRow(newDt.Rows[i]);
                            //MessageBox.Show("***ERROR*** Deceased Policy Number (" + policyNumber + ")\ncannot be located in previous months data!", "Deceased Policy Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                        }
                    }
                }
                catch ( Exception ex)
                {
                }
            }

            FixDeathPaidDate(dx);

            dx = AddNumColumn(dx);

            if (dx.Rows.Count > 0)
                dgv3.DataSource = dx;

            if (currentDt != null)
            {
                if (currentDt.Rows.Count > 0)
                {
                    dgv.DataSource = currentDt;
                    gridMain.RefreshData();
                    gridMain.RefreshEditor(true);
                    dgv.Refresh();
                }
            }

            tabControl1.TabPages.Remove(tabPage4);

            if (problemDt.Rows.Count > 0)
            {
                tabControl1.TabPages.Add(tabPage4);

                problemDt = AddNumColumn(problemDt);

                dgv4.DataSource = problemDt;
                gridMain4.RefreshData();
                gridMain4.RefreshEditor(true);
                dgv4.Refresh();
            }

            tabPage3.Text = date1 + " Last Month";
            return dx;
        }
        /***********************************************************************************************/
        private DataTable PreProcessFDLIC(DataTable dt )
        {
            int firstRow = -1;
            string search = "INSURED NAME";
            string str = "";
            DataTable newDt = dt.Clone();

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    str = dt.Rows[i][j].ObjToString().ToUpper();
                    if (str == search)
                    {
                        firstRow = i;
                        search = "FH No.";
                        break;
                    }
                }
                if (firstRow >= 0)
                    break;
            }
            if (firstRow < 0)
                return newDt;
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                str = dt.Rows[firstRow][i].ObjToString();
                if (String.IsNullOrWhiteSpace(str))
                    continue;
                if (G1.get_column_number(dt, str) >= 0)
                    str = str + "2";
                newDt.Columns[i].ColumnName = str;
                newDt.Columns[i].Caption = str;

                dt.Columns[i].ColumnName = str;
                dt.Columns[i].Caption = str;
            }
            for (int i = (firstRow + 1); i < dt.Rows.Count; i++)
            {
                str = dt.Rows[i]["FH No."].ObjToString();
                //if (String.IsNullOrWhiteSpace(str))
                //    continue;
                newDt.ImportRow(dt.Rows[i]);
            }
            newDt = mapFDLIC(newDt);

            return newDt;
        }
        /***********************************************************************************************/
        private DataTable PreProcessForethought(DataTable dt)
        {
            int firstRow = -1;
            string search = "INSURED LAST NAME";
            string str = "";
            DataTable newDt = dt.Clone();

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    str = dt.Rows[i][j].ObjToString().ToUpper();
                    if (str == search)
                    {
                        firstRow = i;
                        search = "Policy ID";
                        break;
                    }
                }
                if (firstRow >= 0)
                    break;
            }
            if (firstRow < 0)
                return newDt;
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                str = dt.Rows[firstRow][i].ObjToString();
                if (String.IsNullOrWhiteSpace(str))
                    continue;
                if (G1.get_column_number(dt, str) >= 0)
                    str = str + "2";
                newDt.Columns[i].ColumnName = str;
                newDt.Columns[i].Caption = str;

                dt.Columns[i].ColumnName = str;
                dt.Columns[i].Caption = str;
            }
            try
            {
                for (int i = (firstRow + 1); i < dt.Rows.Count; i++)
                {
                    if (workDC)
                        str = dt.Rows[i]["Pol Id"].ObjToString();
                    else
                        str = dt.Rows[i]["Policy ID"].ObjToString();
                    if (String.IsNullOrWhiteSpace(str))
                        continue;
                    if (!G1.validate_numeric(str))
                        continue;
                    if ( workDC )
                    {
                        str = dt.Rows[i]["Firm Name"].ObjToString();
                        if (String.IsNullOrWhiteSpace(str))
                            continue;
                    }
                    newDt.ImportRow(dt.Rows[i]);
                }
                if (workDC)
                {
                    newDt = mapForethoughtDC(newDt);
                }
                else
                    newDt = mapForethought(newDt);

                DataTable testDt = newDt.Clone();
                dgv2.DataSource = testDt;
                dgv3.DataSource = testDt;
                dgv4.DataSource = testDt;

                LoadForethoughtDC();
            }
            catch ( Exception ex)
            {
            }

            return newDt;
        }
        /***********************************************************************************************/
        private void LoadForethoughtDC()
        {
            try
            {
                string filename = importedFile.Trim();
                if (filename.ToUpper().IndexOf("ACTIVE") < 0)
                    return;
                string[] Lines = filename.Split(' ');
                filename = "";
                for (int i = 0; i < Lines.Length; i++)
                {
                    if (String.IsNullOrWhiteSpace(Lines[i]))
                        continue;
                    filename += Lines[i].Trim() + " ";
                }

                filename = filename.Trim();
                filename = filename.ToUpper().Replace("ACTIVE", "DC");

                DataTable dt = ExcelWriter.ReadFile2(filename);
                if (dt.Rows.Count <= 0)
                    return;

                int firstRow = -1;
                string search = "INSURED LAST NAME";
                string str = "";
                DataTable newDt = dt.Clone();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        str = dt.Rows[i][j].ObjToString().ToUpper();
                        if (str == search)
                        {
                            firstRow = i;
                            search = "Pol ID";
                            break;
                        }
                    }
                    if (firstRow >= 0)
                        break;
                }
                if (firstRow < 0)
                    return;
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    str = dt.Rows[firstRow][i].ObjToString().Trim();
                    if (String.IsNullOrWhiteSpace(str))
                        continue;
                    if (G1.get_column_number(dt, str) >= 0)
                        str = str + "2";
                    newDt.Columns[i].ColumnName = str;
                    newDt.Columns[i].Caption = str;

                    dt.Columns[i].ColumnName = str;
                    dt.Columns[i].Caption = str;
                }
                for (int i = (firstRow + 1); i < dt.Rows.Count; i++)
                {
                    try
                    {
                        str = dt.Rows[i]["Pol Id"].ObjToString().Trim();
                        if (String.IsNullOrWhiteSpace(str))
                            continue;
                        if (!G1.validate_numeric(str))
                            continue;
                        str = dt.Rows[i]["Firm Name"].ObjToString();
                        if (String.IsNullOrWhiteSpace(str))
                            continue;
                        newDt.ImportRow(dt.Rows[i]);
                    }
                    catch (Exception ex)
                    {
                    }
                }

                workDC = true;

                newDt = mapForethoughtDC(newDt);

                newDt = FindLastActive(newDt, gridMain2);

                str = actualFile.Trim();
                str = str.Replace("ACTIVE", "DC");
                tabPage2.Text = str;

                newDt = AddNumColumn(newDt);
                dgv2.DataSource = newDt;

                DateTime date = DetermineDate();
                date = date.AddMonths(-1);
                int days = DateTime.DaysInMonth(date.Year, date.Month);
                date = new DateTime(date.Year, date.Month, days);
                string date1 = date.ToString("yyyy-MM-dd");

                string cmd = "Select * from `trust_data` WHERE `date` = '" + date1 + "' AND `trustCompany` = '" + workWhat.Trim() + "';";
                DataTable dx = G1.get_db_data(cmd);
                //if (dx.Rows.Count > 0)
                //    dgv3.DataSource = dx;

                string policyNumber = "";
                string contractNumber = "";
                DataRow[] dRows = null;
                DateTime deathPaidDate = DateTime.Now;
                double deathClaimAmount = 0D;

                problemDt = newDt.Clone();

                for (int i = 0; i < newDt.Rows.Count; i++)
                {
                    policyNumber = newDt.Rows[i]["policyNumber"].ObjToString();
                    deathPaidDate = newDt.Rows[i]["deathPaidDate"].ObjToDateTime();
                    deathClaimAmount = newDt.Rows[i]["deathClaimAmount"].ObjToDouble();
                    if (!String.IsNullOrWhiteSpace(policyNumber))
                    {
                        dRows = dx.Select("policyNumber='" + policyNumber + "'");
                        if (dRows.Length > 0)
                        {
                            dRows[0]["deathPaidDate"] = deathPaidDate.ToString("yyyy-MM-dd");
                            dRows[0]["deathClaimAmount"] = deathClaimAmount;
                            contractNumber = dRows[0]["contractNumber"].ObjToString();
                            newDt.Rows[i]["contractNumber"] = contractNumber;
                            newDt.Rows[i]["found"] = contractNumber;
                            if (String.IsNullOrWhiteSpace(contractNumber))
                                problemDt.ImportRow(newDt.Rows[i]);
                        }
                        else
                            problemDt.ImportRow(newDt.Rows[i]);
                    }
                    else
                        problemDt.ImportRow(newDt.Rows[i]);
                }
                if (dx.Rows.Count > 0)
                {
                    dx = AddNumColumn(dx);
                    dgv3.DataSource = dx;
                    dgv2.DataSource = newDt;
                }

                tabPage3.Text = date1 + " Last Month";

                tabControl1.TabPages.Remove(tabPage4);
                if (problemDt.Rows.Count > 0)
                {
                    problemDt = AddNumColumn(problemDt);
                    tabControl1.TabPages.Add(tabPage4);
                    dgv4.DataSource = problemDt;
                    gridMain4.RefreshData();
                    gridMain4.RefreshEditor(true);
                    dgv4.Refresh();
                }
            }
            catch ( Exception ex )
            {
            }

            workDC = false;
        }
        /***********************************************************************************************/
        private DataTable PreProcessCD(DataTable dt)
        {
            int firstRow = -1;
            string search = "LAST NAME";
            string str = "";
            DataTable newDt = dt.Clone();

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    str = dt.Rows[i][j].ObjToString().ToUpper();
                    if (str == search)
                    {
                        firstRow = i;
                        break;
                    }
                }
                if (firstRow >= 0)
                    break;
            }
            if (firstRow < 0)
                return newDt;
            int balColumn = -1;
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                str = dt.Rows[firstRow][i].ObjToString();
                if (String.IsNullOrWhiteSpace(str))
                    continue;
                if (G1.get_column_number(dt, str) >= 0)
                    str = str + "2";
                if (str.ToUpper().IndexOf("BAL") == 0 && balColumn < 0)
                    balColumn = i;
                newDt.Columns[i].ColumnName = str;
                newDt.Columns[i].Caption = str;

                dt.Columns[i].ColumnName = str;
                dt.Columns[i].Caption = str;
            }
            if (balColumn < 0)
                return newDt;
            try
            {
                for (int i = (firstRow + 1); i < dt.Rows.Count; i++)
                {
                    str = dt.Rows[i][balColumn].ObjToString();
                    if (String.IsNullOrWhiteSpace(str))
                        continue;
                    str = str.Replace(",", "");
                    str = str.Replace("$", "");
                    if (!G1.validate_numeric(str))
                        continue;
                    str = dt.Rows[i]["FIRST NAME"].ObjToString();
                    if ( String.IsNullOrWhiteSpace ( str ))
                    {
                        str = dt.Rows[i]["LAST NAME"].ObjToString();
                        if (String.IsNullOrWhiteSpace(str))
                            continue;
                    }
                    newDt.ImportRow(dt.Rows[i]);
                }
                newDt = mapCD(newDt);
            }
            catch (Exception ex)
            {
            }

            return newDt;
        }
        /***********************************************************************************************/
        private DataTable PreProcessUnity(DataTable dt, bool second = false )
        {
            int firstRow = -1;
            string search = "POLICY NUMBER";
            search = "FH NO.";
            search = "FH NAME";
            string str = "";
            DataTable newDt = dt.Clone();

            int trys = 0;

            for (; ; )
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        str = dt.Rows[i][j].ObjToString().ToUpper();
                        if (str == search)
                        {
                            firstRow = i;
                            break;
                        }
                    }
                    if (firstRow >= 0)
                        break;
                }
                if (firstRow >= 0)
                    break;
                if (trys > 0)
                    break;
                trys++;
                search = "FUNERAL HOME";
            }
            if (firstRow < 0)
                return newDt;
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                str = dt.Rows[firstRow][i].ObjToString();
                if (String.IsNullOrWhiteSpace(str))
                    continue;
                if (G1.get_column_number(dt, str) >= 0)
                {
                    for (; ; )
                    {
                        str = str + "2";
                        if (G1.get_column_number(dt, str) < 0)
                            break;
                    }
                }
                newDt.Columns[i].ColumnName = str;
                newDt.Columns[i].Caption = str;

                dt.Columns[i].ColumnName = str;
                dt.Columns[i].Caption = str;
            }
            string policyNumber = "";
            string polSearch= "POLICY NUMBER";
            if ( G1.get_column_number ( dt, polSearch) < 0 )
            {
                polSearch = "Policy No.";
                if (G1.get_column_number(dt, polSearch) < 0)
                {
                    return newDt;
                }
            }
            for (int i = (firstRow + 1); i < dt.Rows.Count; i++)
            {
                str = dt.Rows[i][polSearch].ObjToString().ToUpper();
                if (String.IsNullOrWhiteSpace(str))
                    continue;
                policyNumber = str;
                str = dt.Rows[i][search].ObjToString();
                if (String.IsNullOrWhiteSpace(str))
                    continue;
                if (str.ToUpper() == search)
                {
                    if ( !G1.validate_numeric ( policyNumber))
                        continue;
                }
                //if (!G1.validate_numeric(str))
                //    continue;
                newDt.ImportRow(dt.Rows[i]);
            }

            if (workDC)
                newDt = mapUnityDC(newDt);
            else
            {
                newDt = mapUnity(newDt);

                if (!second)
                {
                    DataTable ddt = ExcelWriter.ReadFile2(importedFile, 0, "Claims Paid");
                    //DataTable ddt = ExcelWriter.ReadFile2(importedFile, 5);
                    if (ddt.Rows.Count <= 0)
                        return newDt;

                    ddt = PreProcessUnity(ddt, true);
                    ddt = AddNumColumn(ddt);
                    dgv2.DataSource = ddt;
                    gridMain2.RefreshData();
                    gridMain2.RefreshEditor(true);
                    dgv2.Refresh();

                    DateTime date = DetermineDate();

                    string date1 = date.ToString("yyyy-MM-dd");
                    tabPage2.Text = date1 + " DC";

                    date = date.AddMonths(-1);
                    int days = DateTime.DaysInMonth(date.Year, date.Month);
                    date = new DateTime(date.Year, date.Month, days);
                    date1 = date.ToString("yyyy-MM-dd");

                    DataTable dx = LoadLastMonth(ddt, newDt );
                }
            }

            return newDt;
        }
        /***********************************************************************************************/
        private void CleanupData ( DataTable dt )
        {
            string option = "";
            string columnName = "";
            string value = "";
            try
            {
                for (int i = 0; i < gridMain.Columns.Count; i++)
                {
                    option = gridMain.Columns[i].DisplayFormat.FormatString.Trim();
                    if (option.ToUpper() == "N2")
                    {
                        columnName = gridMain.Columns[i].FieldName.Trim();
                        for (int j = 0; j < dt.Rows.Count; j++)
                        {
                            try
                            {
                                if (G1.get_column_number(dt, columnName) >= 0)
                                {
                                    value = dt.Rows[j][columnName].ObjToString().Trim();
                                    value = value.Replace(",", "");
                                    if (value == "-")
                                        dt.Rows[j][columnName] = "0.00";
                                    else if (!G1.validate_numeric(value))
                                        dt.Rows[j][columnName] = "0.00";
                                }
                            }
                            catch ( Exception ex )
                            {
                            }
                        }
                    }
                }
            }
            catch ( Exception ex)
            {
            }
        }
        /***********************************************************************************************/
        private DataTable PreprocessTable ( DataTable dt)
        {
            string str = "";
            int goodRow = -1;

            string search = "INSURED NAME";
            if (workWhat == "FDLIC")
                search = "FH NO.";
            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    str = dt.Rows[i][1].ObjToString().Trim();
                    if (str.ToUpper() == search)
                    {
                        goodRow = i;
                        break;
                    }
                }
                if (goodRow < 0)
                    return dt;
                for (int i = (dt.Rows.Count - 1); i >= 0; i--)
                {
                    if (i < goodRow)
                        dt.Rows.RemoveAt(i);
                }
                for (int i = 1; i < dt.Columns.Count; i++)
                {
                    str = dt.Rows[0][i].ObjToString();
                    if (!String.IsNullOrWhiteSpace(str))
                    {
                        dt.Columns[i].ColumnName = str;
                        dt.Columns[i].Caption = str;
                    }
                }
                dt.Rows.RemoveAt(0); // Remove Column Names in first Row
            }
            catch ( Exception ex)
            {
            }
            return dt;
        }
        /***********************************************************************************************/
        private DataTable mapAllColumns ( DataTable dt )
        {
            if (workWhat.Trim() == "Security National")
                dt = mapSecurityNational(dt);
            else if (workWhat.Trim() == "FDLIC")
                dt = mapFDLIC(dt);
            else if (workWhat.Trim() == "Unity")
                dt = mapUnity(dt);
            else if (workWhat.Trim() == "Forethought")
                dt = mapForethought(dt);
            else if (workWhat.Trim() == "CD")
            {
                dt = mapCD(dt);
                return dt;
            }

            try
            {
                string str = "";
                for (int i = dt.Rows.Count - 1; i >= 0; i--)
                {
                    if (workWhat.Trim() == "FDLIC")
                    {
                        str = dt.Rows[i]["trustName"].ObjToString();
                        if (String.IsNullOrWhiteSpace(str))
                            dt.Rows.RemoveAt(i);
                    }
                    else
                    {
                        str = dt.Rows[i]["endingPaymentBalance"].ObjToString();
                        str = str.Replace(",", "");
                        str = str.Replace("$", "");
                        if (!G1.validate_numeric(str))
                            dt.Rows.RemoveAt(i);
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return dt;
        }
        /***********************************************************************************************/
        private DataTable mapSecurityNational ( DataTable dt )
        {
            string str = "";
            if (G1.get_column_number(dt, "lastName") < 0)
            {
                try
                {
                    dt.Columns.Add("lastName");
                    dt.Columns.Add("firstName");
                    dt.Columns.Add("middleName");
                }
                catch (Exception ex)
                {
                }
            }

            if (G1.get_column_number(dt, "preOrPost") < 0)
                dt.Columns.Add("preOrPost");
            if (G1.get_column_number(dt, "growth") < 0)
                dt.Columns.Add("growth", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "currentUnappliedCash") < 0)
                dt.Columns.Add("currentUnappliedCash", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "priorUnappliedCash") < 0)
                dt.Columns.Add("priorUnappliedCash", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "trustCompany") < 0)
                dt.Columns.Add("trustCompany");

            if (G1.get_column_number(dt, "deathClaimAmount") < 0)
                dt.Columns.Add("deathClaimAmount", Type.GetType("System.Double"));
            //if (G1.get_column_number(dt, "deathPaidDate") < 0)
            //    dt.Columns.Add("deathPaidDate");

            string[] Lines = null;

            try
            {
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    str = dt.Columns[i].ColumnName.ObjToString().Trim().ToUpper();
                    if (str == "POLICY#")
                    {
                        dt.Columns[i].ColumnName = "policyNumber";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "TRUST#")
                    {
                        dt.Columns[i].ColumnName = "contractNumber";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "TRUST NAME")
                    {
                        dt.Columns[i].ColumnName = "trustName";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "PREMIUM")
                    {
                        dt.Columns[i].ColumnName = "beginningPaymentBalance";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "SURRENDER")
                    {
                        dt.Columns[i].ColumnName = "beginningDeathBenefit";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "DEATH BENEFIT")
                    {
                        dt.Columns[i].ColumnName = "endingDeathBenefit";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "FACE AMOUNT")
                    {
                        dt.Columns[i].ColumnName = "endingPaymentBalance";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "INSURED NAME")
                    {
                        dt.Columns[i].ColumnName = "insuredName";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "PAID DATE")
                    {
                        dt.Columns[i].ColumnName = "deathPaidDate";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                }
            }
            catch ( Exception ex)
            {
            }
            string name = "";
            string firstName = "";
            string middleName = "";
            string lastName = "";
            string trustName = "";
            string preOrPost = "";
            string contractNumber = "";

            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i]["trustCompany"] = workWhat;
                    name = dt.Rows[i]["insuredName"].ObjToString();
                    Lines = name.Split('*');
                    lastName = "";
                    firstName = "";
                    middleName = "";
                    if (Lines.Length > 0)
                        lastName = Lines[0].Trim();
                    if (Lines.Length > 1)
                        firstName = Lines[1].Trim();
                    if (Lines.Length > 2)
                        middleName = Lines[2].Trim();
                    dt.Rows[i]["lastName"] = lastName;
                    Lines = firstName.Split(' ');
                    if (Lines.Length > 0)
                        firstName = Lines[0].Trim();
                    if (Lines.Length > 1)
                        middleName = Lines[1].Trim();

                    dt.Rows[i]["firstName"] = firstName;
                    dt.Rows[i]["middleName"] = middleName;
                    //if (!workDC)
                    //{
                    //    trustName = dt.Rows[i]["trustName"].ObjToString();

                    //    preOrPost = "";
                    //    if (trustFileName.IndexOf("POST") > 0)
                    //        preOrPost = "post";
                    //    else if (trustFileName.IndexOf("PRE") > 0)
                    //        preOrPost = "pre";
                    //    else
                    //    {
                    //        if (trustName.IndexOf("2002") > 0)
                    //            preOrPost = "post";
                    //        else
                    //            preOrPost = "pre";
                    //    }
                    //}
                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                    preOrPost = determinePrePostSecurityNational(contractNumber);
                    dt.Rows[i]["preOrPost"] = preOrPost;
                }
            }
            catch (Exception ex)
            {
            }

            return dt;
        }
        /***********************************************************************************************/
        private DataTable mapSecurityNationalNew(DataTable dt)
        {
            string str = "";

            DataTable dddd = null;

            DataRow[] dRows = dt.Select("trustnumber='P04076'");
            if ( dRows.Length > 0 )
            {
                dddd = dRows.CopyToDataTable();
            }
            if (G1.get_column_number(dt, "lastName") < 0)
            {
                try
                {
                    dt.Columns.Add("lastName");
                    dt.Columns.Add("firstName");
                    dt.Columns.Add("middleName");
                }
                catch (Exception ex)
                {
                }
            }

            if (G1.get_column_number(dt, "preOrPost") < 0)
                dt.Columns.Add("preOrPost");
            if (G1.get_column_number(dt, "insuredName") < 0)
                dt.Columns.Add("insuredName");
            if (G1.get_column_number(dt, "growth") < 0)
                dt.Columns.Add("growth", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "currentUnappliedCash") < 0)
                dt.Columns.Add("currentUnappliedCash", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "priorUnappliedCash") < 0)
                dt.Columns.Add("priorUnappliedCash", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "trustCompany") < 0)
                dt.Columns.Add("trustCompany");

            if (G1.get_column_number(dt, "deathClaimAmount") < 0)
                dt.Columns.Add("deathClaimAmount", Type.GetType("System.Double"));
            //if (G1.get_column_number(dt, "deathPaidDate") < 0)
            //    dt.Columns.Add("deathPaidDate");

            string[] Lines = null;

            try
            {
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    str = dt.Columns[i].ColumnName.ObjToString().Trim().ToUpper();
                    if (str == "POLICYNUMBER")
                    {
                        dt.Columns[i].ColumnName = "policyNumber";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "TRUSTNUMBER")
                    {
                        dt.Columns[i].ColumnName = "contractNumber";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "BILLINGSTATUS")
                    {
                        dt.Columns[i].ColumnName = "billingReason";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "POLICYSTATUS")
                    {
                        dt.Columns[i].ColumnName = "statusReason";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "TRUSTNAME")
                    {
                        dt.Columns[i].ColumnName = "trustName";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "PREMIUMPAID")
                    {
                        dt.Columns[i].ColumnName = "beginningPaymentBalance";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "SURRENDER")
                    {
                        dt.Columns[i].ColumnName = "beginningDeathBenefit";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "DEATHBENEFIT")
                    {
                        dt.Columns[i].ColumnName = "endingDeathBenefit";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "FACEAMOUNT")
                    {
                        dt.Columns[i].ColumnName = "endingPaymentBalance";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "INSURED NAME")
                    {
                        dt.Columns[i].ColumnName = "insuredName";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "PAIDTODATE")
                    {
                        dt.Columns[i].ColumnName = "deathPaidDate";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                }
            }
            catch (Exception ex)
            {
            }
            string name = "";
            string firstName = "";
            string middleName = "";
            string lastName = "";
            string trustName = "";
            string preOrPost = "";
            bool reverseName = false;
            string contractNumber = "";

            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i]["trustCompany"] = workWhat;
                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                    if (contractNumber == "P02036")
                    {
                    }

                    name = dt.Rows[i]["InsuredFirstName"].ObjToString() + " " + dt.Rows[i]["InsuredLastName"].ObjToString();
                    if (name.IndexOf("*") > 0)
                    {
                        reverseName = true;
                        name = name.Replace("*", " ");
                    }
                    name = name.Trim();
                    dt.Rows[i]["insuredName"] = name;
                    G1.ParseName(name, ref firstName, ref middleName, ref lastName, true );

                    if (reverseName)
                    {
                        dt.Rows[i]["lastName"] = firstName;
                        if ( String.IsNullOrWhiteSpace ( middleName) && !String.IsNullOrWhiteSpace ( lastName))
                        {
                            middleName = lastName;
                            lastName = "";
                        }
                        dt.Rows[i]["firstName"] = middleName;
                        dt.Rows[i]["middleName"] = lastName;

                        firstName = dt.Rows[i]["firstName"].ObjToString();
                        middleName = dt.Rows[i]["middleName"].ObjToString();
                        lastName = dt.Rows[i]["lastName"].ObjToString();
                        if (!String.IsNullOrWhiteSpace(firstName))
                            name = firstName;
                        if ( !String.IsNullOrWhiteSpace ( middleName ))
                        {
                            if (!String.IsNullOrWhiteSpace(name))
                                name += " ";
                            name += middleName.Trim();
                        }
                        if (!String.IsNullOrWhiteSpace(name))
                            name += " ";
                        name += lastName;
                        dt.Rows[i]["insuredName"] = name;
                    }
                    else
                    {
                        dt.Rows[i]["lastName"] = lastName;
                        dt.Rows[i]["firstName"] = firstName;
                        dt.Rows[i]["middleName"] = middleName;
                    }
                }

                string policyNumber = "";
                string b = "";
                double d = 0D;
                DateTime conv = DateTime.Now;

                for ( int i=dt.Rows.Count-1; i>=0; i-- )
                {
                    policyNumber = dt.Rows[i]["policyNumber"].ObjToString();
                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                    if ( contractNumber == "P04076")
                    {
                    }
                    if ( policyNumber.ToUpper() == "POLICYNUMBER")
                    {
                        dt.Rows.RemoveAt(i);
                        continue;
                    }
                    b = dt.Rows[i]["deathPaidDate"].ObjToString();
                    if (String.IsNullOrWhiteSpace(b))
                    {
                        //dt.Rows.RemoveAt(i);
                        //continue;
                    }
                    if ( b.Trim().ToUpper() == "INSUREDFIRSTNAME")
                    {
                        dt.Rows.RemoveAt(i);
                        continue;
                    }
                }
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    try
                    {
                        contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                        preOrPost = determinePrePostSecurityNational(contractNumber);
                        dt.Rows[i]["preOrPost"] = preOrPost;

                        b = dt.Rows[i]["deathPaidDate"].ObjToString();
                        if (String.IsNullOrWhiteSpace(b))
                            continue;
                        d = double.Parse(b);
                        conv = DateTime.FromOADate(d);
                        dt.Rows[i]["deathPaidDate"] = conv.ToString("MM/dd/yyyy");
                        dt.Rows[i]["deathClaimAmount"] = dt.Rows[i]["endingDeathBenefit"].ObjToDouble();
                    }
                    catch ( Exception ex)
                    {
                        MessageBox.Show("***ERROR*** Row (" + i.ToString() + ") / " + b + "\n", "Trying to Converting Excel Date!", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    }
                }
            }
            catch (Exception ex)
            {
            }

            return dt;
        }
        /***********************************************************************************************/
        private DataTable mapSecurityNationalDC(DataTable dt)
        {
            string str = "";
            if (G1.get_column_number(dt, "lastName") < 0)
            {
                try
                {
                    dt.Columns.Add("lastName");
                    dt.Columns.Add("firstName");
                    dt.Columns.Add("middleName");
                }
                catch (Exception ex)
                {
                }
            }

            if (G1.get_column_number(dt, "preOrPost") < 0)
                dt.Columns.Add("preOrPost");
            if (G1.get_column_number(dt, "growth") < 0)
                dt.Columns.Add("growth", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "currentUnappliedCash") < 0)
                dt.Columns.Add("currentUnappliedCash", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "priorUnappliedCash") < 0)
                dt.Columns.Add("priorUnappliedCash", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "trustCompany") < 0)
                dt.Columns.Add("trustCompany");

            if (G1.get_column_number(dt, "deathClaimAmount") < 0)
                dt.Columns.Add("deathClaimAmount", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "deathPaidDate") < 0)
                dt.Columns.Add("deathPaidDate");

            string[] Lines = null;

            try
            {
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    str = dt.Columns[i].ColumnName.ObjToString().Trim().ToUpper();
                    if (str == "POLICY")
                    {
                        dt.Columns[i].ColumnName = "policyNumber";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "TRUST#")
                    {
                        dt.Columns[i].ColumnName = "contractNumber";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "TRUST NAME")
                    {
                        dt.Columns[i].ColumnName = "trustName";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "PREMIUM")
                    {
                        dt.Columns[i].ColumnName = "beginningPaymentBalance";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "SURRENDER")
                    {
                        dt.Columns[i].ColumnName = "beginningDeathBenefit";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "DEATH")
                    {
                        dt.Columns[i].ColumnName = "endingDeathBenefit";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "CHECK2")
                    {
                        dt.Columns[i].ColumnName = "endingPaymentBalance";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "INSURED NAME")
                    {
                        dt.Columns[i].ColumnName = "insuredName";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "PAID DATE")
                    {
                        dt.Columns[i].ColumnName = "deathPaidDate";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                }
            }
            catch (Exception ex)
            {
            }
            string name = "";
            string firstName = "";
            string middleName = "";
            string lastName = "";
            string trustName = "";
            string preOrPost = "";
            string contractNumber = "";

            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i]["trustCompany"] = workWhat;
                    name = dt.Rows[i]["insuredName"].ObjToString();
                    Lines = name.Split('*');
                    lastName = "";
                    firstName = "";
                    middleName = "";
                    if (Lines.Length > 0)
                        lastName = Lines[0].Trim();
                    if (Lines.Length > 1)
                        firstName = Lines[1].Trim();
                    if (Lines.Length > 2)
                        middleName = Lines[2].Trim();
                    dt.Rows[i]["lastName"] = lastName;
                    Lines = firstName.Split(' ');
                    if (Lines.Length > 0)
                        firstName = Lines[0].Trim();
                    if (Lines.Length > 1)
                        middleName = Lines[1].Trim();

                    dt.Rows[i]["firstName"] = firstName;
                    dt.Rows[i]["middleName"] = middleName;
                    if (!workDC)
                    {
                        trustName = dt.Rows[i]["trustName"].ObjToString();

                        preOrPost = "";
                        if (trustFileName.IndexOf("POST") > 0)
                            preOrPost = "post";
                        else if (trustFileName.IndexOf("PRE") > 0)
                            preOrPost = "pre";
                        else
                        {
                            if (trustName.IndexOf("2002") > 0)
                                preOrPost = "post";
                            else
                                preOrPost = "pre";
                        }
                    }
                    dt.Rows[i]["preOrPost"] = preOrPost;
                }

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                    preOrPost = determinePrePostSecurityNational(contractNumber);
                    dt.Rows[i]["preOrPost"] = preOrPost;

                    string b = dt.Rows[i]["PAID"].ObjToString();
                    if (String.IsNullOrWhiteSpace(b))
                        continue;
                    double d = double.Parse(b);
                    DateTime conv = DateTime.FromOADate(d);
                    dt.Rows[i]["deathPaidDate"] = conv.ToString("MM/dd/yyyy");
                    dt.Rows[i]["deathClaimAmount"] = dt.Rows[i]["endingDeathBenefit"].ObjToDouble();
                }

                gridMain.Columns["beginningDeathBenefit"].Visible = false; // Surrender
                gridMain.Columns["beginningPaymentBalance"].Visible = false; // Premium
                gridMain.Columns["growth"].Visible = false;
            }
            catch (Exception ex)
            {
            }

            return dt;
        }
        /***********************************************************************************************/
        private DataTable mapSecurityNationalNewDC(DataTable dt)
        {
            string str = "";
            if (G1.get_column_number(dt, "lastName") < 0)
            {
                try
                {
                    dt.Columns.Add("lastName");
                    dt.Columns.Add("firstName");
                    dt.Columns.Add("middleName");
                }
                catch (Exception ex)
                {
                }
            }

            if (G1.get_column_number(dt, "preOrPost") < 0)
                dt.Columns.Add("preOrPost");
            if (G1.get_column_number(dt, "insuredName") < 0)
                dt.Columns.Add("insuredName");
            if (G1.get_column_number(dt, "growth") < 0)
                dt.Columns.Add("growth", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "currentUnappliedCash") < 0)
                dt.Columns.Add("currentUnappliedCash", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "priorUnappliedCash") < 0)
                dt.Columns.Add("priorUnappliedCash", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "trustCompany") < 0)
                dt.Columns.Add("trustCompany");

            if (G1.get_column_number(dt, "deathClaimAmount") < 0)
                dt.Columns.Add("deathClaimAmount", Type.GetType("System.Double"));
            //if (G1.get_column_number(dt, "deathPaidDate") < 0)
            //    dt.Columns.Add("deathPaidDate");

            string[] Lines = null;

            try
            {
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    str = dt.Columns[i].ColumnName.ObjToString().Trim().ToUpper();
                    if (str == "POLICYNUMBER")
                    {
                        dt.Columns[i].ColumnName = "policyNumber";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "TRUSTNUMBER")
                    {
                        dt.Columns[i].ColumnName = "contractNumber";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "BILLINGSTATUS")
                    {
                        dt.Columns[i].ColumnName = "billingReason";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "POLICYSTATUS")
                    {
                        dt.Columns[i].ColumnName = "statusReason";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "TRUSTNAME")
                    {
                        dt.Columns[i].ColumnName = "trustName";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "PREMIUM")
                    {
                        dt.Columns[i].ColumnName = "beginningPaymentBalance";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "SURRENDER")
                    {
                        dt.Columns[i].ColumnName = "beginningDeathBenefit";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "DEATHBENEFIT")
                    {
                        dt.Columns[i].ColumnName = "endingDeathBenefit";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "CLAIMPAIDAMOUNT")
                    {
                        dt.Columns[i].ColumnName = "endingDeathBenefit";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "FACEAMOUNT")
                    {
                        dt.Columns[i].ColumnName = "endingPaymentBalance";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "INSURED NAME")
                    {
                        dt.Columns[i].ColumnName = "insuredName";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "STATUSDATE")
                    {
                        dt.Columns[i].ColumnName = "deathPaidDate";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                }
            }
            catch (Exception ex)
            {
            }
            string name = "";
            string firstName = "";
            string middleName = "";
            string lastName = "";
            string trustName = "";
            string preOrPost = "";
            string contractNumber = "";
            bool reverseName = false;

            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i]["trustCompany"] = workWhat;
                    name = dt.Rows[i]["InsuredFirstName"].ObjToString() + " " + dt.Rows[i]["InsuredLastName"].ObjToString();
                    if (name.IndexOf("*") > 0)
                    {
                        reverseName = true;
                        name = name.Replace("*", " ");
                    }
                    name = name.Trim();
                    dt.Rows[i]["insuredName"] = name;
                    G1.ParseName(name, ref firstName, ref middleName, ref lastName, true);

                    if (reverseName)
                    {
                        dt.Rows[i]["lastName"] = firstName;
                        if (String.IsNullOrWhiteSpace(middleName) && !String.IsNullOrWhiteSpace(lastName))
                        {
                            middleName = lastName;
                            lastName = "";
                        }
                        dt.Rows[i]["firstName"] = middleName;
                        dt.Rows[i]["middleName"] = lastName;

                        firstName = dt.Rows[i]["firstName"].ObjToString();
                        middleName = dt.Rows[i]["middleName"].ObjToString();
                        lastName = dt.Rows[i]["lastName"].ObjToString();
                        if (!String.IsNullOrWhiteSpace(firstName))
                            name = firstName;
                        if (!String.IsNullOrWhiteSpace(middleName))
                        {
                            if (!String.IsNullOrWhiteSpace(name))
                                name += " ";
                            name += middleName.Trim();
                        }
                        if (!String.IsNullOrWhiteSpace(name))
                            name += " ";
                        name += lastName;
                        dt.Rows[i]["insuredName"] = name;
                    }
                    else
                    {
                        dt.Rows[i]["lastName"] = lastName;
                        dt.Rows[i]["firstName"] = firstName;
                        dt.Rows[i]["middleName"] = middleName;
                    }
                }

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                    preOrPost = determinePrePostSecurityNational(contractNumber);
                    dt.Rows[i]["preOrPost"] = preOrPost;
                    string b = dt.Rows[i]["deathPaidDate"].ObjToString();
                    if (String.IsNullOrWhiteSpace(b))
                    {
                        b = dt.Rows[i]["statusDate"].ObjToString();
                        if ( String.IsNullOrWhiteSpace ( b ))
                            continue;
                    }
                    double d = double.Parse(b);
                    DateTime conv = DateTime.FromOADate(d);
                    dt.Rows[i]["deathPaidDate"] = conv.ToString("MM/dd/yyyy");
                    dt.Rows[i]["deathClaimAmount"] = dt.Rows[i]["endingDeathBenefit"].ObjToDouble();
                }

                gridMain.Columns["beginningDeathBenefit"].Visible = false; // Surrender
                gridMain.Columns["beginningPaymentBalance"].Visible = false; // Premium
                gridMain.Columns["growth"].Visible = false;
            }
            catch (Exception ex)
            {
            }

            return dt;
        }
        /***********************************************************************************************/
        private DataTable mapFDLIC(DataTable dt)
        {
            string str = "";
            if ( G1.get_column_number ( dt, "lastName") < 0 )
            {
                try
                {
                    dt.Columns.Add("lastName");
                    dt.Columns.Add("firstName");
                    dt.Columns.Add("middleName");
                }
                catch ( Exception ex)
                {
                }
            }
            if (G1.get_column_number(dt, "preOrPost") < 0)
                dt.Columns.Add("preOrPost");
            if (G1.get_column_number(dt, "growth") < 0)
                dt.Columns.Add("growth", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "currentUnappliedCash") < 0)
                dt.Columns.Add("currentUnappliedCash", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "priorUnappliedCash") < 0)
                dt.Columns.Add("priorUnappliedCash", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "trustCompany") < 0)
                dt.Columns.Add("trustCompany");

            string[] Lines = null;
            int foundColumns = 0;
            int expecteColumns = 13;

            for (int i = 0; i < dt.Columns.Count; i++)
            {
                try
                {
                    str = dt.Columns[i].ColumnName.ObjToString().Trim();
                    if (str == "Policy No.")
                    {
                        dt.Columns[i].ColumnName = "policyNumber";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                        foundColumns++;
                    }
                    else if (str == "SMFS Policy No.")
                    {
                        dt.Columns[i].ColumnName = "contractNumber";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                        foundColumns++;
                    }
                    else if (str == "Funeral Home")
                    {
                        dt.Columns[i].ColumnName = "trustName";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                        foundColumns++;
                    }
                    else if (str == "Beginning Payment Balance")
                    {
                        dt.Columns[i].ColumnName = "beginningPaymentBalance";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                        foundColumns++;
                    }
                    else if (str == "Beginning DB")
                    {
                        dt.Columns[i].ColumnName = "beginningDeathBenefit";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                        foundColumns++;
                    }
                    else if (str == "Ending Death Benefit")
                    {
                        dt.Columns[i].ColumnName = "endingDeathBenefit";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                        foundColumns++;
                    }
                    else if (str == "Ending Payment Balance")
                    {
                        dt.Columns[i].ColumnName = "endingPaymentBalance";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                        foundColumns++;
                    }
                    else if (str == "Down Payments")
                    {
                        dt.Columns[i].ColumnName = "downPayments";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                        foundColumns++;
                    }
                    else if (str == "Payments")
                    {
                        dt.Columns[i].ColumnName = "Payments";
                        //dt.Columns[i].ColumnName = "abcde";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                        foundColumns++;
                    }
                    else if (str == "Death Claim Amount")
                    {
                        dt.Columns[i].ColumnName = "deathClaimAmount";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                        foundColumns++;
                    }
                    else if (str == "Paid Date" || str == "Claim Date" )
                    {
                        dt.Columns[i].ColumnName = "deathPaidDate";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                        foundColumns++;
                    }
                    else if (str == "Insured Name")
                    {
                        dt.Columns[i].ColumnName = "insuredName";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                        foundColumns++;
                    }
                    else if (str == "Total Paid-in at Claim" || str == "Total Paid in at Claim" )
                    {
                        dt.Columns[i].ColumnName = "totalPaidInAtClaim";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                        foundColumns++;
                    }
                }
                catch ( Exception ex)
                {
                }
            }

            if ( foundColumns != expecteColumns )
            {
                MessageBox.Show("***Warning***\nExpected " + expecteColumns.ToString() + " Columns - Got " + foundColumns.ToString() + " Columns!\nSomething may be missing or\nColumn Names may have changed!", "FDLIC Import Warning Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);

            }
            string name = "";
            string firstName = "";
            string middleName = "";
            string lastName = "";
            string preOrPost = "";
            string trustName = "";
            string contractNumber = "";
            string policyNumber = "";

            double payments = 0D;
            double downPayments = 0D;
            double beginningDeathBenefit = 0D;
            double endingDeathBenefit = 0D;
            double growth = 0D;

            DateTime paidDate = DateTime.Now;
            DateTime date = DetermineDate();
            DateTime firstDate = new DateTime(date.Year, date.Month, 1, 0, 0, 0);
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            DateTime lastDate = new DateTime(date.Year, date.Month, days);

            FixDeathPaidDate(dt);

            DataTable dDt = dt.Clone();

            string type = "";

            if (policyTrustsDt == null)
                policyTrustsDt = G1.get_db_data("Select * from `policyTrusts`");

            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                    if ( contractNumber == "P04039")
                    {

                    }
                    if (String.IsNullOrWhiteSpace(contractNumber))
                    {
                        policyNumber = dt.Rows[i]["policyNumber"].ObjToString();
                        if ( policyNumber == "SM1614321")
                        {
                        }
                        if (String.IsNullOrWhiteSpace(policyNumber))
                            continue;
                        contractNumber = FindContractNumber(policyNumber, "FDLIC", ref type );
                        if (contractNumber == "WT021")
                        {
                        }
                        dt.Rows[i]["contractNumber"] = contractNumber;
                        if (type == "PB")
                        {
                            if ( !contractNumber.EndsWith ( "L") && !contractNumber.EndsWith ( "LI" ) )
                                dt.Rows[i]["trustCompany"] = workWhat + " PB";
                            else
                                dt.Rows[i]["trustCompany"] = workWhat;
                        }
                        else
                            dt.Rows[i]["trustCompany"] = workWhat;

                        contractNumber = FindBadContractNumber(contractNumber, "FDLIC");
                        if ( !String.IsNullOrWhiteSpace ( contractNumber ))
                            dt.Rows[i]["contractNumber"] = contractNumber;
                    }
                    else
                    {
                        dt.Rows[i]["trustCompany"] = workWhat;
                        contractNumber = FindBadContractNumber(contractNumber, "FDLIC");
                        if ( !String.IsNullOrWhiteSpace ( contractNumber ))
                            dt.Rows[i]["contractNumber"] = contractNumber;
                        //else
                        //{
                        //    contractNumber = FindContractNumber(policyNumber, "FDLIC");
                        //    if ( !String.IsNullOrWhiteSpace ( contractNumber))
                        //    {
                        //        dt.Rows[i]["contractNumber"] = contractNumber;
                        //        dt.Rows[i]["trustCompany"] = workWhat + " PB";
                        //    }
                        //}
                    }

                    name = dt.Rows[i]["insuredName"].ObjToString();
                    Lines = name.Split('/');
                    lastName = "";
                    firstName = "";
                    middleName = "";
                    if (Lines.Length > 0)
                        lastName = Lines[0].Trim();
                    if (Lines.Length > 1)
                        firstName = Lines[1].Trim();
                    if (Lines.Length > 2)
                        middleName = Lines[2].Trim();
                    dt.Rows[i]["lastName"] = lastName;
                    dt.Rows[i]["firstName"] = firstName;
                    dt.Rows[i]["middleName"] = middleName;

                    trustName = dt.Rows[i]["trustName"].ObjToString();

                    preOrPost = "post";
                    if (trustFileName.IndexOf("POST") > 0)
                        preOrPost = "post";
                    else if (trustFileName.IndexOf("PRE") > 0)
                        preOrPost = "pre";
                    //else
                    //{
                    //    if (trustName.IndexOf("2002") > 0)
                    //        preOrPost = "post";
                    //    else
                    //        preOrPost = "pre";
                    //}
                    dt.Rows[i]["preOrPost"] = preOrPost;

                    beginningDeathBenefit = dt.Rows[i]["beginningDeathBenefit"].ObjToDouble();
                    endingDeathBenefit = dt.Rows[i]["endingDeathBenefit"].ObjToDouble();
                    payments = dt.Rows[i]["Payments"].ObjToDouble();
                    downPayments = dt.Rows[i]["downPayments"].ObjToDouble();
                    if (endingDeathBenefit > 0D)
                    {
                        growth = endingDeathBenefit - beginningDeathBenefit - payments - downPayments;
                        growth = G1.RoundValue(growth);
                    }
                    else
                        growth = 0D;
                    dt.Rows[i]["growth"] = growth;

                    paidDate = dt.Rows[i]["deathPaidDate"].ObjToDateTime();

                    if (paidDate >= firstDate && paidDate <= lastDate)
                        dDt.ImportRow(dt.Rows[i]);
                }

                dDt = AddNumColumn(dDt);

                dgv2.DataSource = dDt;

                date = DetermineDate();
                DateTime importedDate = date;
                date = date.AddMonths(-1);
                days = DateTime.DaysInMonth(date.Year, date.Month);
                date = new DateTime(date.Year, date.Month, days);
                string date1 = importedDate.ToString("yyyy-MM-dd");

                tabPage2.Text = date1 + " DC";

                DataTable dx = LoadLastMonth( dDt );
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** " + ex.Message.ToString(), "FDLIC Import Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            }
            return dt;
        }
        /***********************************************************************************************/
        private DataTable AddNumColumn ( DataTable dt )
        {
            if (G1.get_column_number(dt, "num") >= 0)
                dt.Columns.Remove("num");
            else if (G1.get_column_number(dt, "Num") >= 0)
                dt.Columns.Remove("num");
            dt.Columns.Add("num");
            G1.NumberDataTable(dt);
            return dt;
        }
        /***********************************************************************************************/
        private DataTable policyTrustsDt = null;
        /***********************************************************************************************/
        private string FindContractNumber ( string policyNumber, string Company, ref string type )
        {
            string contractNumber = "";
            if ( policyNumber.ToUpper() == "SM1533862")
            {
            }
            DataTable dx = null;
            string cmd = "";
            if ( policyTrustsDt == null )
                policyTrustsDt = G1.get_db_data("Select * from `policyTrusts`");
            if (policyTrustsDt.Rows.Count > 0)
            {
                cmd = "policyNumber='" + policyNumber + "'";
                if (!String.IsNullOrWhiteSpace(Company))
                    cmd = "policyNumber='" + policyNumber + "' AND `Company` = '" + Company + "'";
                DataRow[] dRows = policyTrustsDt.Select(cmd);
                if ( dRows.Length <= 0 && Company == "FDLIC")
                {
                    cmd = "policyNumber='" + policyNumber + "' AND `Company` = '" + Company + " PB'";
                    dRows = policyTrustsDt.Select(cmd);
                }
                if (dRows.Length > 0)
                    dx = dRows.CopyToDataTable();
                else
                    dx = policyTrustsDt.Clone();
            }
            else
            {
                cmd = "Select * from `policytrusts` where `policyNumber` = '" + policyNumber + "';";
                if (!String.IsNullOrWhiteSpace(Company))
                    cmd = "Select * from `policytrusts` where `policyNumber` = '" + policyNumber + "' AND `Company` = '" + Company + "';";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count <= 0 && !String.IsNullOrWhiteSpace(Company) && Company == "FDLIC")
                {
                    cmd = "Select * from `policytrusts` where `policyNumber` = '" + policyNumber + "' AND `Company` = '" + Company + " PB';";
                    dx = G1.get_db_data(cmd);
                }
            }

            if (dx.Rows.Count > 0)
            {
                contractNumber = dx.Rows[0]["contractNumber"].ObjToString();
                type = dx.Rows[0]["type"].ObjToString();
                if (type.ToUpper().IndexOf("PB") >= 0)
                    type = "PB";
            }
            else
            {
                if (policyNumber.ToUpper().IndexOf("PBI") == 0)
                {
                    policyNumber = policyNumber.ToUpper().Replace("PBI", "");
                    cmd = "Select * from `contracts` where `contractNumber` = '" + policyNumber + "';";
                    if (!String.IsNullOrWhiteSpace(Company))
                        cmd = "Select * from `policytrusts` where `policyNumber` = '" + policyNumber + "' AND `Company` = '" + Company + "';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                    {
                        contractNumber = dx.Rows[0]["contractNumber"].ObjToString();
                        type = dx.Rows[0]["type"].ObjToString();
                        if (type.ToUpper().IndexOf("PB") >= 0)
                            type = "PB";
                    }
                }
            }
            return contractNumber;
        }
        /***********************************************************************************************/
        private string FindBadContractNumber(string badContractNumber, string Company)
        {
            if (String.IsNullOrWhiteSpace(badContractNumber))
                return badContractNumber;

            string contractNumber = "";
            string cmd = "";
            DataTable dx = null;
            if (policyTrustsDt.Rows.Count > 0)
            {
                cmd = "badTrustNumber = '" + badContractNumber + "'";
                if (!String.IsNullOrWhiteSpace(Company))
                    cmd = "badTrustNumber = '" + badContractNumber + "' AND Company = '" + Company + "'";
                DataRow[] dRows = policyTrustsDt.Select(cmd);
                if (dRows.Length <= 0 && Company == "FDLIC")
                {
                    cmd = "badTrustNumber = '" + badContractNumber + "' AND Company = '" + Company + " PB'";
                    dRows = policyTrustsDt.Select(cmd);
                }
                if (dRows.Length > 0)
                    dx = dRows.CopyToDataTable();
                else
                    dx = policyTrustsDt.Clone();
            }
            else
            {
                cmd = "Select * from `policytrusts` where `badTrustNumber` = '" + badContractNumber + "';";
                if (!String.IsNullOrWhiteSpace(Company))
                    cmd = "Select * from `policytrusts` where `badTrustNumber` = '" + badContractNumber + "' AND `Company` = '" + Company + "';";
                dx = G1.get_db_data(cmd);
            }
            if (dx.Rows.Count > 0)
            {
                contractNumber = dx.Rows[0]["contractNumber"].ObjToString();
            }
            return contractNumber;
        }
        /***********************************************************************************************/
        private DataTable mapUnity(DataTable dt)
        {
            string str = "";
            if (G1.get_column_number(dt, "lastName") < 0)
            {
                try
                {
                    dt.Columns.Add("middleName");
                }
                catch (Exception ex)
                {
                }
            }
            if (G1.get_column_number(dt, "preOrPost") < 0)
                dt.Columns.Add("preOrPost");
            if (G1.get_column_number(dt, "trustCompany") < 0)
                dt.Columns.Add("trustCompany");
            if (G1.get_column_number(dt, "growth") < 0)
                dt.Columns.Add("growth", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "Payments") < 0)
                dt.Columns.Add("Payments", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "downPayments") < 0)
                dt.Columns.Add("downPayments", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "contractNumber") < 0)
                dt.Columns.Add("contractNumber");

            if (G1.get_column_number(dt, "deathClaimAmount") < 0)
                dt.Columns.Add("deathClaimAmount", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "reducedPaidUpAmount") < 0)
                dt.Columns.Add("reducedPaidUpAmount", Type.GetType("System.Double"));
            //if (G1.get_column_number(dt, "deathPaidDate") < 0)
            //    dt.Columns.Add("deathPaidDate");

            string[] Lines = null;

            string cName = "";
            bool found = false;

            for (int i = 0; i < dt.Columns.Count; i++)
            {
                try
                {
                    str = dt.Columns[i].ColumnName.ObjToString().Trim();
                    if (str == "Policy Number")
                    {
                        dt.Columns[i].ColumnName = "policyNumber";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "Policy No.")
                    {
                        dt.Columns[i].ColumnName = "policyNumber";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "FH Name")
                    {
                        dt.Columns[i].ColumnName = "trustName";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "Funeral Home")
                    {
                        dt.Columns[i].ColumnName = "trustName";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "Num")
                    {
                        dt.Columns[i].ColumnName = "num";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "Prior Cash Received")
                    {
                        dt.Columns[i].ColumnName = "beginningPaymentBalance";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "Current Cash Received")
                    {
                        dt.Columns[i].ColumnName = "beginningDeathBenefit";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "Beginning DB")
                    {
                        dt.Columns[i].ColumnName = "beginningDeathBenefit";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "Prior Unapplied Cash")
                    {
                        dt.Columns[i].ColumnName = "priorUnappliedCash";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "Current Unapplied Cash")
                    {
                        dt.Columns[i].ColumnName = "currentUnappliedCash";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "Death Benefit")
                    {
                        dt.Columns[i].ColumnName = "endingDeathBenefit";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "Face Amount")
                    {
                        dt.Columns[i].ColumnName = "endingPaymentBalance";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "Down Payments")
                    {
                        dt.Columns[i].ColumnName = "downPayments";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "Payments")
                    {
                        dt.Columns[i].ColumnName = "Payments";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "Insured Name")
                    {
                        dt.Columns[i].ColumnName = "insuredName";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "Insured First Name")
                    {
                        dt.Columns[i].ColumnName = "firstName";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "Insured Last Name")
                    {
                        dt.Columns[i].ColumnName = "lastName";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if ( str == "Policy Extract_Policy Status")
                    {
                        dt.Columns[i].ColumnName = "policyStatus";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "Policy Extract_Status Reason")
                    {
                        dt.Columns[i].ColumnName = "statusReason";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "Policy Extract_Billing Reason")
                    {
                        dt.Columns[i].ColumnName = "billingReason";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    //else if (str == "Paid-to-Date")
                    //{
                    //    dt.Columns[i].ColumnName = "deathPaidDate";
                    //    dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    //}
                    else if (str == "Date Claim Processed")
                    {
                        dt.Columns[i].ColumnName = "deathPaidDate";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                }
                catch (Exception ex)
                {
                }
            }

            if (G1.get_column_number(dt, "trustName") < 0)
                dt.Columns.Add("trustName");
            if (G1.get_column_number(dt, "statusReason") < 0)
                dt.Columns.Add("statusReason");
            if (G1.get_column_number(dt, "billingReason") < 0)
                dt.Columns.Add("billingReason");
            if (G1.get_column_number(dt, "policyStatus") < 0)
                dt.Columns.Add("policyStatus");
            string name = "";
            string firstName = "";
            string middleName = "";
            string lastName = "";
            string preOrPost = "";
            string trustName = "";

            double payments = 0D;
            double downPayments = 0D;
            double beginningDeathBenefit = 0D;
            double endingDeathBenefit = 0D;
            double reducedPaidUpAmount = 0D;
            double growth = 0D;

            try
            {
                string cmd = "";
                DataTable dx = null;
                string contractNumber = "";
                string policyNumber = "";
                string trustCompany = "";
                string billingReason = "";

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    trustCompany = workWhat;
                    policyNumber = dt.Rows[i]["policyNumber"].ObjToString();
                    if (policyNumber == "770080466")
                    {
                    }
                    if (policyNumber.ToUpper().IndexOf("PB") == 0)
                        trustCompany = "Unity DI";
                    else if (policyNumber.ToUpper().IndexOf("PIB") == 0)
                        trustCompany = "Unity PB";
                    else if (policyNumber.ToUpper().IndexOf("PSP") == 0)
                        trustCompany = "Unity PB";

                    dt.Rows[i]["trustCompany"] = trustCompany;

                    reducedPaidUpAmount = 0D;

                    billingReason = dt.Rows[i]["billingReason"].ObjToString();

                    trustName = dt.Rows[i]["trustName"].ObjToString();

                    preOrPost = determinePrePostUnity(policyNumber);
                    dt.Rows[i]["preOrPost"] = preOrPost;

                    if ( policyNumber == "770080466")
                    {
                    }

                    beginningDeathBenefit = dt.Rows[i]["beginningDeathBenefit"].ObjToDouble();
                    endingDeathBenefit = dt.Rows[i]["endingDeathBenefit"].ObjToDouble();
                    //if ( billingReason.ToUpper() == "RU")
                    //{
                    //    reducedPaidUpAmount = endingDeathBenefit;
                    //    endingDeathBenefit = reducedPaidUpAmount;
                    //    dt.Rows[i]["endingDeathBenefit"] = 0D;
                    //    //dt.Rows[i]["endingPaymentBalance"] = reducedPaidUpAmount;
                    //}
                    payments = dt.Rows[i]["Payments"].ObjToDouble();
                    downPayments = dt.Rows[i]["downPayments"].ObjToDouble();
                    if (endingDeathBenefit > 0D)
                    {
                        growth = endingDeathBenefit - beginningDeathBenefit - payments - downPayments;
                        growth = G1.RoundValue(growth);
                    }
                    else
                        growth = 0D;
                    dt.Rows[i]["growth"] = growth;

                    policyNumber = dt.Rows[i]["policyNumber"].ObjToString().Trim();
                    cmd = "Select * from `policytrusts` where `policyNumber` = '" + policyNumber + "';";
                    dx = G1.get_db_data(cmd);
                    if ( dx.Rows.Count > 0 )
                    {
                        contractNumber = dx.Rows[0]["contractNumber"].ObjToString();
                        dt.Rows[i]["contractNumber"] = contractNumber;
                    }
                    else
                    {
                        if ( policyNumber.ToUpper().IndexOf ( "PBI") == 0 )
                        {
                            policyNumber = policyNumber.ToUpper().Replace("PBI", "");
                            cmd = "Select * from `contracts` where `contractNumber` = '" + policyNumber + "';";
                            dx = G1.get_db_data(cmd);
                            if (dx.Rows.Count > 0)
                            {
                                contractNumber = dx.Rows[0]["contractNumber"].ObjToString();
                                dt.Rows[i]["contractNumber"] = contractNumber;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return dt;
        }
        /***********************************************************************************************/
        private DataTable mapUnityDC(DataTable dt)
        {
            string str = "";
            if (G1.get_column_number(dt, "lastName") < 0)
            {
                try
                {
                    dt.Columns.Add("middleName");
                }
                catch (Exception ex)
                {
                }
            }
            if (G1.get_column_number(dt, "preOrPost") < 0)
                dt.Columns.Add("preOrPost");
            if (G1.get_column_number(dt, "trustCompany") < 0)
                dt.Columns.Add("trustCompany");
            if (G1.get_column_number(dt, "growth") < 0)
                dt.Columns.Add("growth", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "Payments") < 0)
                dt.Columns.Add("Payments", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "downPayments") < 0)
                dt.Columns.Add("downPayments", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "contractNumber") < 0)
                dt.Columns.Add("contractNumber");

            if (G1.get_column_number(dt, "deathClaimAmount") < 0)
                dt.Columns.Add("deathClaimAmount", Type.GetType("System.Double"));
            //if (G1.get_column_number(dt, "deathPaidDate") < 0)
            //    dt.Columns.Add("deathPaidDate");

            string[] Lines = null;

            string cName = "";
            bool found = false;

            for (int i = 0; i < dt.Columns.Count; i++)
            {
                try
                {
                    str = dt.Columns[i].ColumnName.ObjToString().Trim();
                    if (str == "Policy Number")
                    {
                        dt.Columns[i].ColumnName = "policyNumber";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "Num")
                    {
                        dt.Columns[i].ColumnName = "num";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "FH Name")
                    {
                        dt.Columns[i].ColumnName = "trustName";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "Prior Cash Received")
                    {
                        dt.Columns[i].ColumnName = "beginningPaymentBalance";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "Current Cash Received")
                    {
                        dt.Columns[i].ColumnName = "beginningDeathBenefit";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "Prior Unapplied Cash")
                    {
                        dt.Columns[i].ColumnName = "priorUnappliedCash";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "Current Unapplied Cash")
                    {
                        dt.Columns[i].ColumnName = "currentUnappliedCash";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "Death Benefit")
                    {
                        dt.Columns[i].ColumnName = "endingDeathBenefit";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "Face Amount")
                    {
                        dt.Columns[i].ColumnName = "endingPaymentBalance";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "Down Payments")
                    {
                        dt.Columns[i].ColumnName = "downPayments";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "Payments")
                    {
                        dt.Columns[i].ColumnName = "Payments";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "Insured Name")
                    {
                        dt.Columns[i].ColumnName = "insuredName";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "Insured First Name")
                    {
                        dt.Columns[i].ColumnName = "firstName";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "Insured Last Name")
                    {
                        dt.Columns[i].ColumnName = "lastName";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "Paid-to-Date")
                    {
                        dt.Columns[i].ColumnName = "deathPaidDate";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                }
                catch (Exception ex)
                {
                }
            }
            string name = "";
            string firstName = "";
            string middleName = "";
            string lastName = "";
            string preOrPost = "";
            string trustName = "";

            double payments = 0D;
            double downPayments = 0D;
            double beginningDeathBenefit = 0D;
            double endingDeathBenefit = 0D;
            double growth = 0D;

            try
            {
                string cmd = "";
                DataTable dx = null;
                string contractNumber = "";
                string policyNumber = "";
                string trustCompany = "";

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    trustCompany = workWhat;
                    policyNumber = dt.Rows[i]["policyNumber"].ObjToString();
                    if (policyNumber.ToUpper().IndexOf("PB") == 0)
                        trustCompany = "Unity DI";
                    else if (policyNumber.ToUpper().IndexOf("PI") == 0)
                        trustCompany = "Unity PB";
                    else if (policyNumber.ToUpper().IndexOf("PS") == 0)
                        trustCompany = "Unity PB";

                    dt.Rows[i]["trustCompany"] = trustCompany;

                    trustName = dt.Rows[i]["trustName"].ObjToString();

                    preOrPost = determinePrePostUnity(policyNumber);

                    dt.Rows[i]["preOrPost"] = preOrPost;

                    beginningDeathBenefit = dt.Rows[i]["beginningDeathBenefit"].ObjToDouble();
                    endingDeathBenefit = dt.Rows[i]["endingDeathBenefit"].ObjToDouble();
                    payments = dt.Rows[i]["Payments"].ObjToDouble();
                    downPayments = dt.Rows[i]["downPayments"].ObjToDouble();
                    if (endingDeathBenefit > 0D)
                    {
                        growth = endingDeathBenefit - beginningDeathBenefit - payments - downPayments;
                        growth = G1.RoundValue(growth);
                    }
                    else
                        growth = 0D;
                    dt.Rows[i]["growth"] = growth;

                    policyNumber = dt.Rows[i]["policyNumber"].ObjToString().Trim();
                    cmd = "Select * from `policytrusts` where `policyNumber` = '" + policyNumber + "';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                    {
                        contractNumber = dx.Rows[0]["contractNumber"].ObjToString();
                        dt.Rows[i]["contractNumber"] = contractNumber;
                    }
                    else
                    {
                        if (policyNumber.ToUpper().IndexOf("PBI") == 0)
                        {
                            policyNumber = policyNumber.ToUpper().Replace("PBI", "");
                            cmd = "Select * from `contracts` where `contractNumber` = '" + policyNumber + "';";
                            dx = G1.get_db_data(cmd);
                            if (dx.Rows.Count > 0)
                            {
                                contractNumber = dx.Rows[0]["contractNumber"].ObjToString();
                                dt.Rows[i]["contractNumber"] = contractNumber;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return dt;
        }
        /***********************************************************************************************/
        private DataTable mapForethought(DataTable dt)
        {
            string str = "";
            if (G1.get_column_number(dt, "lastName") < 0)
            {
                try
                {
                    dt.Columns.Add("middleName");
                }
                catch (Exception ex)
                {
                }
            }
            if (G1.get_column_number(dt, "preOrPost") < 0)
                dt.Columns.Add("preOrPost");
            if (G1.get_column_number(dt, "growth") < 0)
                dt.Columns.Add("growth", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "Payments") < 0)
                dt.Columns.Add("Payments", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "downPayments") < 0)
                dt.Columns.Add("downPayments", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "contractNumber") < 0)
                dt.Columns.Add("contractNumber");

            if (G1.get_column_number(dt, "insuredName") < 0)
                dt.Columns.Add("insuredName");
            if (G1.get_column_number(dt, "beginningDeathBenefit") < 0)
                dt.Columns.Add("beginningDeathBenefit", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "endingDeathBenefit") < 0)
                dt.Columns.Add("endingDeathBenefit", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "priorUnappliedCash") < 0)
                dt.Columns.Add("priorUnappliedCash", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "currentUnappliedCash") < 0)
                dt.Columns.Add("currentUnappliedCash", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "trustCompany") < 0)
                dt.Columns.Add("trustCompany");

            if (G1.get_column_number(dt, "deathClaimAmount") < 0)
                dt.Columns.Add("deathClaimAmount", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "deathPaidDate") < 0)
                dt.Columns.Add("deathPaidDate");

            string[] Lines = null;

            string cName = "";
            bool found = false;

            for (int i = 0; i < dt.Columns.Count; i++)
            {
                try
                {
                    str = dt.Columns[i].ColumnName.ObjToString().Trim();
                    if (str == "Policy ID")
                    {
                        dt.Columns[i].ColumnName = "policyNumber";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "Firm Name")
                    {
                        dt.Columns[i].ColumnName = "trustName";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "Original Face")
                    {
                        dt.Columns[i].ColumnName = "beginningPaymentBalance";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "Estimated Current Face*")
                    {
                        dt.Columns[i].ColumnName = "endingPaymentBalance";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "Insured First Name")
                    {
                        dt.Columns[i].ColumnName = "firstName";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "Insured Last Name")
                    {
                        dt.Columns[i].ColumnName = "lastName";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                }
                catch (Exception ex)
                {
                }
            }
            string name = "";
            string firstName = "";
            string middleName = "";
            string lastName = "";
            string preOrPost = "";
            string newPreOrPost = "";
            string trustName = "";

            double payments = 0D;
            double downPayments = 0D;
            double beginningDeathBenefit = 0D;
            double endingDeathBenefit = 0D;
            double growth = 0D;

            try
            {
                string cmd = "";
                DataTable dx = null;
                string contractNumber = "";
                string policyNumber = "";
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i]["trustCompany"] = workWhat;
                    trustName = dt.Rows[i]["trustName"].ObjToString();
                    firstName = dt.Rows[i]["firstName"].ObjToString();
                    lastName = dt.Rows[i]["lastName"].ObjToString();

                    preOrPost = determinePrePostForethought(trustName, firstName, lastName);

                    //preOrPost = "pre";
                    dt.Rows[i]["preOrPost"] = preOrPost;

                    str = dt.Rows[i]["beginningPaymentBalance"].ObjToString();
                    str = str.Replace(",", "");
                    str = str.Replace("$", "");
                    dt.Rows[i]["beginningPaymentBalance"] = str;

                    str = dt.Rows[i]["endingPaymentBalance"].ObjToString();
                    str = str.Replace(",", "");
                    str = str.Replace("$", "");
                    dt.Rows[i]["endingPaymentBalance"] = str;

                    beginningDeathBenefit = dt.Rows[i]["beginningPaymentBalance"].ObjToDouble();
                    dt.Rows[i]["beginningDeathBenefit"] = beginningDeathBenefit;

                    endingDeathBenefit = dt.Rows[i]["endingPaymentBalance"].ObjToDouble();
                    dt.Rows[i]["endingDeathBenefit"] = endingDeathBenefit;

                    payments = dt.Rows[i]["Payments"].ObjToDouble();
                    downPayments = dt.Rows[i]["downPayments"].ObjToDouble();
                    if (endingDeathBenefit > 0D)
                    {
                        growth = endingDeathBenefit - beginningDeathBenefit - payments - downPayments;
                        growth = G1.RoundValue(growth);
                    }
                    else
                        growth = 0D;
                    dt.Rows[i]["growth"] = growth;

                    policyNumber = dt.Rows[i]["policyNumber"].ObjToString().Trim();
                    cmd = "Select * from `policytrusts` where `policyNumber` = '" + policyNumber + "' AND `Company` = 'FORETHOUGHT';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                    {
                        contractNumber = dx.Rows[0]["contractNumber"].ObjToString();
                        dt.Rows[i]["contractNumber"] = contractNumber;
                    }
                    else
                    {
                        if (policyNumber.ToUpper().IndexOf("PBI") == 0)
                        {
                            policyNumber = policyNumber.ToUpper().Replace("PBI", "");
                            cmd = "Select * from `contracts` where `contractNumber` = '" + policyNumber + "';";
                            dx = G1.get_db_data(cmd);
                            if (dx.Rows.Count > 0)
                            {
                                contractNumber = dx.Rows[0]["contractNumber"].ObjToString();
                                dt.Rows[i]["contractNumber"] = contractNumber;
                            }
                        }
                    }
                    newPreOrPost = determinePrePostByYear(contractNumber);
                    if (newPreOrPost == "Pre" && preOrPost != newPreOrPost)
                        dt.Rows[i]["preOrPost"] = newPreOrPost;
                }
            }
            catch (Exception ex)
            {
            }
            return dt;
        }
        /***********************************************************************************************/
        private DataTable mapForethoughtDC(DataTable dt)
        {
            string str = "";
            if (G1.get_column_number(dt, "lastName") < 0)
            {
                try
                {
                    dt.Columns.Add("middleName");
                }
                catch (Exception ex)
                {
                }
            }
            if (G1.get_column_number(dt, "preOrPost") < 0)
                dt.Columns.Add("preOrPost");
            if (G1.get_column_number(dt, "growth") < 0)
                dt.Columns.Add("growth", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "Payments") < 0)
                dt.Columns.Add("Payments", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "downPayments") < 0)
                dt.Columns.Add("downPayments", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "contractNumber") < 0)
                dt.Columns.Add("contractNumber");

            if (G1.get_column_number(dt, "insuredName") < 0)
                dt.Columns.Add("insuredName");
            if (G1.get_column_number(dt, "beginningDeathBenefit") < 0)
                dt.Columns.Add("beginningDeathBenefit", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "endingDeathBenefit") < 0)
                dt.Columns.Add("endingDeathBenefit", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "priorUnappliedCash") < 0)
                dt.Columns.Add("priorUnappliedCash", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "currentUnappliedCash") < 0)
                dt.Columns.Add("currentUnappliedCash", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "trustCompany") < 0)
                dt.Columns.Add("trustCompany");

            if (G1.get_column_number(dt, "deathClaimAmount") < 0)
                dt.Columns.Add("deathClaimAmount", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "deathPaidDate") < 0)
                dt.Columns.Add("deathPaidDate");

            string[] Lines = null;

            string cName = "";
            bool found = false;

            for (int i = 0; i < dt.Columns.Count; i++)
            {
                try
                {
                    str = dt.Columns[i].ColumnName.ObjToString().Trim();
                    if (str == "Pol Id")
                    {
                        dt.Columns[i].ColumnName = "policyNumber";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "Firm Name")
                    {
                        dt.Columns[i].ColumnName = "trustName";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "Trustee Death Benefit")
                    {
                        dt.Columns[i].ColumnName = "beginningPaymentBalance";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                        gridMain.Columns["beginningPaymentBalance"].Caption = "Trustee Death Benefit";
                    }
                    else if (str == "Total Trustee Paid")
                    {
                        dt.Columns[i].ColumnName = "endingPaymentBalance";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "Claim Paid Dt")
                    {
                        dt.Columns[i].ColumnName = "deathPaidDate";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "Insured First Name")
                    {
                        dt.Columns[i].ColumnName = "firstName";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "Insured Last Name")
                    {
                        dt.Columns[i].ColumnName = "lastName";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                }
                catch (Exception ex)
                {
                }
            }
            string name = "";
            string firstName = "";
            string middleName = "";
            string lastName = "";
            string preOrPost = "";
            string newPreOrPost = "";
            string trustName = "";

            double payments = 0D;
            double downPayments = 0D;
            double beginningDeathBenefit = 0D;
            double endingDeathBenefit = 0D;
            double growth = 0D;

            try
            {
                string cmd = "";
                DataTable dx = null;
                string contractNumber = "";
                string policyNumber = "";
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i]["trustCompany"] = workWhat;
                    trustName = dt.Rows[i]["trustName"].ObjToString();
                    firstName = dt.Rows[i]["firstName"].ObjToString();
                    lastName = dt.Rows[i]["lastName"].ObjToString();

                    preOrPost = determinePrePostForethought(trustName, firstName, lastName);

                    //preOrPost = "pre";
                    dt.Rows[i]["preOrPost"] = preOrPost;

                    str = dt.Rows[i]["beginningPaymentBalance"].ObjToString();
                    str = str.Replace(",", "");
                    str = str.Replace("$", "");
                    dt.Rows[i]["beginningPaymentBalance"] = str;

                    str = dt.Rows[i]["endingPaymentBalance"].ObjToString();
                    str = str.Replace(",", "");
                    str = str.Replace("$", "");
                    dt.Rows[i]["endingPaymentBalance"] = str;

                    beginningDeathBenefit = dt.Rows[i]["beginningPaymentBalance"].ObjToDouble();
                    dt.Rows[i]["beginningDeathBenefit"] = beginningDeathBenefit;

                    endingDeathBenefit = dt.Rows[i]["endingPaymentBalance"].ObjToDouble();
                    dt.Rows[i]["endingDeathBenefit"] = endingDeathBenefit;

                    payments = dt.Rows[i]["Payments"].ObjToDouble();
                    downPayments = dt.Rows[i]["downPayments"].ObjToDouble();
                    if (endingDeathBenefit > 0D)
                    {
                        growth = endingDeathBenefit - beginningDeathBenefit - payments - downPayments;
                        growth = G1.RoundValue(growth);
                    }
                    else
                        growth = 0D;
                    dt.Rows[i]["growth"] = growth;

                    policyNumber = dt.Rows[i]["policyNumber"].ObjToString().Trim();
                    cmd = "Select * from `policytrusts` where `policyNumber` = '" + policyNumber + "' AND `Company` = 'FORETHOUGHT';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                    {
                        contractNumber = dx.Rows[0]["contractNumber"].ObjToString();
                        dt.Rows[i]["contractNumber"] = contractNumber;
                    }
                    else
                    {
                        if (policyNumber.ToUpper().IndexOf("PBI") == 0)
                        {
                            policyNumber = policyNumber.ToUpper().Replace("PBI", "");
                            cmd = "Select * from `contracts` where `contractNumber` = '" + policyNumber + "';";
                            dx = G1.get_db_data(cmd);
                            if (dx.Rows.Count > 0)
                            {
                                contractNumber = dx.Rows[0]["contractNumber"].ObjToString();
                                dt.Rows[i]["contractNumber"] = contractNumber;
                            }
                        }
                    }
                    newPreOrPost = determinePrePostByYear(contractNumber);
                    if ( newPreOrPost == "Pre" && preOrPost != newPreOrPost )
                        dt.Rows[i]["preOrPost"] = newPreOrPost;
                }
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string b = dt.Rows[i]["Claim Paid Dt"].ObjToString();
                    if (String.IsNullOrWhiteSpace(b))
                        continue;
                    double d = double.Parse(b);
                    DateTime conv = DateTime.FromOADate(d);
                    dt.Rows[i]["deathPaidDate"] = conv.ToString("MM/dd/yyyy");
                    dt.Rows[i]["deathClaimAmount"] = dt.Rows[i]["endingDeathBenefit"].ObjToDouble();
                }
            }
            catch (Exception ex)
            {
            }
            return dt;
        }
        /***********************************************************************************************/
        private void FixDeathPaidDate ( DataTable dt )
        {
            DateTime date = DateTime.Now;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    string b = dt.Rows[i]["deathPaidDate"].ObjToString();
                    if (String.IsNullOrWhiteSpace(b))
                        continue;
                    date = b.ObjToDateTime();
                    if (date.Year > 1000)
                        continue;
                    double d = double.Parse(b);
                    DateTime conv = DateTime.FromOADate(d);
                    dt.Rows[i]["deathPaidDate"] = conv.ToString("MM/dd/yyyy");
                    //                dt.Rows[i]["deathClaimAmount"] = dt.Rows[i]["endingDeathBenefit"].ObjToDouble();
                }
                catch ( Exception ex)
                {
                }
            }
        }
        /***********************************************************************************************/
        private DataTable mapCD(DataTable dt)
        {
            string str = "";
            if (G1.get_column_number(dt, "lastName") < 0)
            {
                try
                {
                    dt.Columns.Add("middleName");
                }
                catch (Exception ex)
                {
                }
            }
            if (G1.get_column_number(dt, "preOrPost") < 0)
                dt.Columns.Add("preOrPost");
            if (G1.get_column_number(dt, "growth") < 0)
                dt.Columns.Add("growth", Type.GetType("System.Double"));
            //if (G1.get_column_number(dt, "payments") < 0)
            //    dt.Columns.Add("payments", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "downPayments") < 0)
                dt.Columns.Add("downPayments", Type.GetType("System.Double"));

            if (G1.get_column_number(dt, "insuredName") < 0)
                dt.Columns.Add("insuredName");
            if (G1.get_column_number(dt, "beginningDeathBenefit") < 0)
                dt.Columns.Add("beginningDeathBenefit", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "endingDeathBenefit") < 0)
                dt.Columns.Add("endingDeathBenefit", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "priorUnappliedCash") < 0)
                dt.Columns.Add("priorUnappliedCash", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "currentUnappliedCash") < 0)
                dt.Columns.Add("currentUnappliedCash", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "trustCompany") < 0)
                dt.Columns.Add("trustCompany");

            //if (G1.get_column_number(dt, "deathClaimAmount") < 0)
            //    dt.Columns.Add("deathClaimAmount", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "deathPaidDate") < 0)
                dt.Columns.Add("deathPaidDate");

            //if (G1.get_column_number(dt, "trustCompany") < 0)
            //    dt.Columns.Add("trustCompany");
            if (G1.get_column_number(dt, "trustName") < 0)
                dt.Columns.Add("trustName");
            if (G1.get_column_number(dt, "insuredName") < 0)
                dt.Columns.Add("insuredName");

            string[] Lines = null;

            string cName = "";
            bool found = false;
            int balCount = 0;

            for (int i = 0; i < dt.Columns.Count; i++)
            {
                try
                {
                    str = dt.Columns[i].ColumnName.ObjToString().Trim().ToUpper();

                    if (str == "COLUMN1")
                    {
                        dt.Columns[i].Caption = "contractNumber";
                        dt.Columns[i].ColumnName = "contractNumber";
                    }
                    if (str == "FIRST NAME")
                    {
                        dt.Columns[i].ColumnName = "firstName";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "LAST NAME")
                    {
                        dt.Columns[i].ColumnName = "lastName";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if ( str.IndexOf ( "BAL") >= 0 )
                    {
                        if ( balCount == 0 )
                        {
                            dt.Columns[i].ColumnName = "beginningPaymentBalance";
                            dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                            balCount++;
                        }
                        else
                        {
                            dt.Columns[i].ColumnName = "endingPaymentBalance";
                            dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                            balCount++;
                        }
                    }
                    else if (str.IndexOf("REMIT") >= 0)
                    {
                        dt.Columns[i].ColumnName = "Payments";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str.IndexOf("DC") >= 0)
                    {
                        if (G1.get_column_number(dt, "deathClaimAmount") < 0)
                        {
                            dt.Columns[i].ColumnName = "deathClaimAmount";
                            dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                        }
                    }
                }
                catch (Exception ex)
                {
                }
            }
            string name = "";
            string firstName = "";
            string middleName = "";
            string lastName = "";
            string preOrPost = "";
            string trustName = "";

            double payments = 0D;
            double downPayments = 0D;
            double beginningDeathBenefit = 0D;
            double endingDeathBenefit = 0D;
            double growth = 0D;
            string cmd = "";
            DataTable dx = null;
            string contractNumber = "";
            string newPreOrPost = "";

            try
            {
                cmd = "";
                string policyNumber = "";
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i]["trustCompany"] = workWhat;
                    dt.Rows[i]["trustName"] = "Cadence";
                    trustName = dt.Rows[i]["trustName"].ObjToString();
                    firstName = dt.Rows[i]["firstName"].ObjToString();
                    lastName = dt.Rows[i]["lastName"].ObjToString();
                    dt.Rows[i]["insuredName"] = lastName + ", " + firstName;

                    preOrPost = determinePrePostForethought(trustName, firstName, lastName);

                    //preOrPost = "pre";
                    dt.Rows[i]["preOrPost"] = preOrPost;

                    str = dt.Rows[i]["beginningPaymentBalance"].ObjToString();
                    str = str.Replace(",", "");
                    str = str.Replace("$", "");
                    dt.Rows[i]["beginningPaymentBalance"] = str;

                    str = dt.Rows[i]["endingPaymentBalance"].ObjToString();
                    str = str.Replace(",", "");
                    str = str.Replace("$", "");
                    dt.Rows[i]["endingPaymentBalance"] = str;

                    beginningDeathBenefit = dt.Rows[i]["beginningPaymentBalance"].ObjToDouble();
                    dt.Rows[i]["beginningDeathBenefit"] = beginningDeathBenefit;

                    endingDeathBenefit = dt.Rows[i]["endingPaymentBalance"].ObjToDouble();
                    dt.Rows[i]["endingDeathBenefit"] = endingDeathBenefit;

                    payments = dt.Rows[i]["Payments"].ObjToDouble();
                    downPayments = dt.Rows[i]["downPayments"].ObjToDouble();
                    if (endingDeathBenefit > 0D)
                    {
                        growth = endingDeathBenefit - beginningDeathBenefit - payments - downPayments;
                        growth = G1.RoundValue(growth);
                    }
                }
            }
            catch (Exception ex)
            {
            }

            for (int i = dt.Rows.Count - 1; i >= 0; i--)
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                if (String.IsNullOrWhiteSpace(contractNumber))
                    continue;
                if (contractNumber.IndexOf("-") >= 0)
                {
                    dt.Rows[i]["contractNumber"] = "";
                    continue;
                }
                cmd = "Select * from `contracts` where `contractNumber` = '" + contractNumber + "';";
                dx = G1.get_db_data(cmd);
                if ( dx.Rows.Count <= 0 )
                {
                    cmd = "Select * from `customers` where `contractNumber` = '" + contractNumber + "';";
                    dx = G1.get_db_data(cmd);
                }
                if (dx.Rows.Count <= 0)
                    dt.Rows[i]["contractNumber"] = contractNumber + " NF";

                preOrPost = dt.Rows[i]["preOrPost"].ObjToString();
                newPreOrPost = determinePrePostByYear(contractNumber);
                if (newPreOrPost == "Pre" && preOrPost != newPreOrPost)
                    dt.Rows[i]["preOrPost"] = newPreOrPost;

            }
            return dt;
        }
        /***********************************************************************************************/
        public static string determinePrePostByYear(string contractNumber)
        {
            string preOrPost = "Pre";
            int currentYear = DateTime.Now.Year % 100;
            if ( contractNumber == "UM187A")
            {
            }
            if (contractNumber.Length >= 6)
            {
                string trust = "";
                string loc = "";
                try
                {
                    trust = Trust85.decodeContractNumber(contractNumber, ref trust, ref loc);
                    trust = contractNumber.Substring(loc.Length);
                    if (trust.Length >= 5)
                    {
                        string what = trust.Substring(0, 2);
                        int year = what.ObjToInt32();
                        if (year <= currentYear)
                        {
                            if (year >= 2)
                                preOrPost = "Post";
                        }
                    }
                }
                catch (Exception ex)
                {
                }
            }
            return preOrPost;
        }
        /***********************************************************************************************/
        public static string determinePrePostSecurityNational(string contractNumber)
        {
            string preOrPost = "Pre";
            int currentYear = DateTime.Now.Year % 100;
            if (contractNumber == "UM187A")
            {
            }
            if (contractNumber.Length >= 6)
            {
                string trust = "";
                string loc = "";
                try
                {
                    trust = Trust85.decodeContractNumber(contractNumber, ref trust, ref loc);
                    trust = contractNumber.Substring(loc.Length);
                    if (trust.Length >= 5)
                    {
                        string what = trust.Substring(0, 2);
                        int year = what.ObjToInt32();
                        if (year <= currentYear)
                        {
                            if (year >= 2)
                                preOrPost = "Post";
                        }
                    }
                }
                catch (Exception ex)
                {
                }
            }
            return preOrPost;
        }
        /***********************************************************************************************/
        private string determinePrePostUnity ( string policyNumber )
        {
            string prePost = "Post";
            if (policyNumber == "PSPNB08002")
                prePost = "Pre";
            else if (policyNumber.ToUpper().IndexOf("PSPNB") == 0)
                prePost = "Pre";
            else if (policyNumber.ToUpper().IndexOf("PSPWT") == 0)
                prePost = "Pre";
            return prePost;
        }
        /***********************************************************************************************/
        public static string determinePrePostForethought ( string trustName, string firstName, string lastName )
        {
            string prePost = "Post";
            if (trustName.ToUpper().IndexOf("FISHER") >= 0)
            {
                prePost = "Pre";
                firstName = firstName.ToUpper();
                lastName = lastName.ToUpper();
                if (lastName == "BUTLER" && firstName == "JOANN M")
                    prePost = "Post";
                else if (lastName == "BUTLER" && firstName == "JOHN WILSON")
                    prePost = "Post";
                else if (lastName == "HERRINGTON" && firstName == "MILDRED M")
                    prePost = "Post";
                else if (lastName == "HOLLINGSWORTH" && firstName == "JOHN DILLON")
                    prePost = "Post";
                else if (lastName == "JAMES" && firstName == "DIANNE ALETRIS")
                    prePost = "Post";
                else if (lastName == "JAMES" && firstName == "GEORGE WILLIAM")
                    prePost = "Post";
                else if (lastName == "JAMES" && firstName == "JACK W")
                    prePost = "Post";
                else if (lastName == "KING" && firstName == "JULIA W")
                    prePost = "Post";
                else if (lastName == "KING" && firstName == "THELMA ANN")
                    prePost = "Post";
                else if (lastName == "MUIRHEAD" && firstName == "STELLA I")
                    prePost = "Post";
                else if (lastName == "TOMPKINS" && firstName == "ILSE R")
                    prePost = "Post";
            }
            else if (trustName.ToUpper().IndexOf("CATCHINGS") >= 0)
            {
                prePost = "Pre";
                firstName = firstName.ToUpper();
                lastName = lastName.ToUpper();
                if (lastName == "DILLARD" && firstName == "BARRY")
                    prePost = "Post";
                else if (lastName == "FOSTER" && firstName == "DORTHEA FAYE")
                    prePost = "Post";
                else if (lastName == "MIXON" && firstName == "WILLIAM A")
                    prePost = "Post";
                else if (lastName == "PEZANT" && firstName == "HIDEKO")
                    prePost = "Post";
                else if (lastName == "PEZANT" && firstName == "HORACE H")
                    prePost = "Post";
                else if (lastName == "SIMS" && firstName == "DOROTHY ANN")
                    prePost = "Post";
                else if (lastName == "SIMS" && firstName == "EVERETT EARL")
                    prePost = "Post";
                else if (lastName == "TRUDEAU CT12006F" && firstName == "MARY E")
                    prePost = "Post";
            }

            return prePost;
        }
        /***********************************************************************************************/
        private bool SaveData(DataTable dt, DateTime saveDate )
        {
            this.Cursor = Cursors.WaitCursor;

            DataTable saveDt = dt.Copy();

            if (G1.get_column_number(saveDt, "date") < 0)
                saveDt.Columns.Add("date");
            if (G1.get_column_number(saveDt, "trustCompany") < 0)
                saveDt.Columns.Add("trustCompany");

            if (G1.get_column_number(saveDt, "reportDate") < 0)
                saveDt.Columns.Add("reportDate" );
            if (G1.get_column_number(saveDt, "position") < 0)
                saveDt.Columns.Add("position");

            if (G1.get_column_number(saveDt, "tmstamp") >= 0)
                saveDt.Columns.Remove("tmstamp");
            if (G1.get_column_number(saveDt, "record") >= 0)
                saveDt.Columns.Remove("record");
            if (G1.get_column_number(saveDt, "num") >= 0)
                saveDt.Columns.Remove("num");
            if (G1.get_column_number(saveDt, "found") >= 0)
                saveDt.Columns.Remove("found");

            DataColumn Col = saveDt.Columns.Add("record", System.Type.GetType("System.String"));
            Col.SetOrdinal(0);// to put the column in position 0;
            DataColumn Col1 = saveDt.Columns.Add("tmstamp", System.Type.GetType("System.String"));
            Col1.SetOrdinal(0);// to put the column in position 0;

            double dValue = 0D;
            string str = "";
            string contractNumber = "";

            for (int i = 0; i < saveDt.Rows.Count; i++)
            {
                contractNumber = saveDt.Rows[i]["contractNumber"].ObjToString();
                if ( contractNumber == "WM05008")
                {
                }
                //saveDt.Rows[i]["tmstamp"] = "0000-00-00";
                saveDt.Rows[i]["tmstamp"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                saveDt.Rows[i]["record"] = "0";
                saveDt.Rows[i]["trustName"] = G1.try_protect_data(saveDt.Rows[i]["trustName"].ObjToString());
                saveDt.Rows[i]["insuredName"] = G1.try_protect_data(saveDt.Rows[i]["insuredName"].ObjToString());
                saveDt.Rows[i]["firstName"] = G1.try_protect_data(saveDt.Rows[i]["firstName"].ObjToString());
                saveDt.Rows[i]["middleName"] = G1.try_protect_data(saveDt.Rows[i]["middleName"].ObjToString());
                saveDt.Rows[i]["lastName"] = G1.try_protect_data(saveDt.Rows[i]["lastName"].ObjToString());
                saveDt.Rows[i]["firstName"] = G1.Truncate(saveDt.Rows[i]["firstName"].ObjToString(), 80);
                saveDt.Rows[i]["middleName"] = G1.Truncate(saveDt.Rows[i]["middleName"].ObjToString(), 80);
                saveDt.Rows[i]["lastName"] = G1.Truncate(saveDt.Rows[i]["lastName"].ObjToString(), 80);

                //saveDt.Rows[i]["trustCompany"] = workWhat;
                saveDt.Rows[i]["date"] = saveDate.ToString("yyyyMMdd");
                saveDt.Rows[i]["reportDate"] = "10000101";
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
                saveDt = ArrangeTable("trust_data", saveDt);
                MySQL.CreateCSVfile(saveDt, Server + strFile, false, "~");
            }
            catch ( Exception ex)
            {
                MessageBox.Show("***ERROR*** Arranging trust_data to DataTable " + ex.Message.ToString());
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
        private DataTable ArrangeTable(string db, DataTable dt)
        {
            DataTable dd = dt.Copy();

            string command = "SELECT* FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '" + db + "' ORDER BY ordinal_position;";

            DataTable dx = G1.get_db_data(command);
            int col = 0;
            int oldCol = 0;
            string cName = "";
            DataColumn Col1 = null;
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                cName = dx.Rows[i]["column_name"].ObjToString();
                oldCol = G1.get_column_number(dd, cName);
                if (oldCol >= 0)
                {
                    Col1 = dd.Columns[oldCol];
                    Col1.SetOrdinal(col);// to put the column in position 0;
                    col++;
                }
            }
            return dd;
        }
        /***********************************************************************************************/
        private void DeletePreviousData ( DateTime saveDate, string trustCompany )
        {
            string date1 = saveDate.ToString("yyyy-MM-dd");

            string cmd = "DELETE from `trust_data` where `date` = '" + date1 + "' AND `trustCompany` = '" + trustCompany + "' ";
            if ( trustCompany.ToUpper() == "UNITY")
            {
                cmd = "DELETE from `trust_data` where `date` = '" + date1 + "' AND `trustCompany` LIKE '" + trustCompany + "%' ";
            }
            else if ( trustCompany.ToUpper() == "FDLIC" )
            {
                cmd = "DELETE from `trust_data` where `date` = '" + date1 + "' AND `trustCompany` LIKE '" + trustCompany + "%' ";
            }
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
        private void locateContractNumberToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string contractNumber = "";
            string policyNumber = "";
            string trustCompany = "";
            DataRow dr = null;
            if ( dgv.Visible )
                dr = gridMain.GetFocusedDataRow();
            else if ( dgv2.Visible )
                dr = gridMain2.GetFocusedDataRow();
            else if (dgv3.Visible)
                dr = gridMain2.GetFocusedDataRow();
            else if (dgv4.Visible)
                dr = gridMain2.GetFocusedDataRow();
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
                if( s.Rows.Count > 0 )
                {
                    DataRow[] dRows = s.Select("select='1'");
                    if (dRows.Length <= 0)
                        return;
                    s = dRows.CopyToDataTable();
                }
                string contractNumber = s.Rows[0]["contractNumber"].ObjToString();
                if (string.IsNullOrWhiteSpace(contractNumber))
                    return;

                if (dgv.Visible)
                {
                    DataRow dr = gridMain.GetFocusedDataRow();
                    dr["contractNumber"] = contractNumber;

                    gridMain.RefreshData();
                    gridMain.RefreshEditor(true);
                    dgv.Refresh();

                    string policyNumber = dr["policyNumber"].ObjToString();
                    FixLastMonth(policyNumber, contractNumber);
                }
                else if (dgv2.Visible)
                {
                    DataRow dr = gridMain2.GetFocusedDataRow();
                    dr["contractNumber"] = contractNumber;

                    gridMain2.RefreshData();
                    gridMain2.RefreshEditor(true);
                    dgv2.Refresh();

                    string policyNumber = dr["policyNumber"].ObjToString();
                    FixLastMonth(policyNumber, contractNumber);
                }
                else if (dgv3.Visible)
                {
                    DataRow dr = gridMain3.GetFocusedDataRow();
                    dr["contractNumber"] = contractNumber;

                    gridMain3.RefreshData();
                    gridMain3.RefreshEditor(true);
                    dgv3.Refresh();

                    //string policyNumber = dr["policyNumber"].ObjToString();
                    //FixLastMonth(policyNumber, contractNumber);
                }
                else if (dgv4.Visible)
                {
                    DataRow dr = gridMain4.GetFocusedDataRow();
                    dr["contractNumber"] = contractNumber;

                    gridMain4.RefreshData();
                    gridMain4.RefreshEditor(true);
                    dgv4.Refresh();

                    string policyNumber = dr["policyNumber"].ObjToString();
                    FixLastMonth(policyNumber, contractNumber);
                }
            }
            catch (Exception ex)
            {
            }
        }
        /***********************************************************************************************/
        private void FixLastMonth ( string policyNumber, string contractNumber )
        {
            if (String.IsNullOrWhiteSpace(policyNumber))
                return;
            if (String.IsNullOrWhiteSpace(contractNumber))
                return;
            DataTable dx = (DataTable)dgv3.DataSource;
            if (dx == null)
                return;
            if (dx.Rows.Count <= 0)
                return;
            DataRow[] dRows = dx.Select("policyNumber='" + policyNumber + "'");
            if (dRows.Length <= 0)
            {
                MessageBox.Show("***ERROR*** Deceased Policy Number (" + policyNumber + ")\ncannot be located in previous months data!", "Deceased Policy Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }
            dRows[0]["contractNumber"] = contractNumber;

            dgv3.DataSource = dx;
            gridMain3.RefreshData();
            gridMain3.RefreshEditor(true);
            dgv3.Refresh();
        }
        /***********************************************************************************************/
        private void gridMain2_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain2.GetFocusedDataRow();
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
        private void gridMain3_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain3.GetFocusedDataRow();
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
        private void gridMain4_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain4.GetFocusedDataRow();
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
        private void chkShowAll_CheckedChanged(object sender, EventArgs e)
        {
            LoadContractData();
            gridMain.Columns["date"].Visible = true;
        }
        /***********************************************************************************************/
        private void cmbTrust_SelectedIndexChanged(object sender, EventArgs e)
        {
            workWhat = cmbTrust.Text;
        }
        /***********************************************************************************************/
    }
}