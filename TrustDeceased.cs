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
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraPrinting;
using DevExpress.Utils;
using MyXtraGrid;
using DevExpress.XtraCharts;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.BandedGrid;
using DevExpress.XtraGrid;
using DevExpress.Export.Xl;
using System.IO;
using DevExpress.Printing.ExportHelpers;
using DevExpress.Export;
//using System.Windows.Input;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class TrustDeceased : DevExpress.XtraEditors.XtraForm
    {
        private bool foundLocalPreference = false;
        private DataTable originalDt = null;
        private bool loading = true;
        private string workCompany = "";
        private string workMonth = "";
        private string workYear = "";
        private DataTable workDt = null;
        private DateTime workDate = DateTime.Now;
        private string workPreOrPost = "";
        private string workOldStuff = "";
        private bool workIndependent = false;
        private string workReport = "";
        private int workNextDays = 0;
        private int workNextRow = 0;
        private string workContract = "";
        private CheckedComboBoxEdit workCompanies = null;
        private bool useCalculatedBeginningBalance = false;
        private bool useCalculatedEndingBalance = false;
        /****************************************************************************************/
        public TrustDeceased(CheckedComboBoxEdit companies, string report, string month, string year, string preOrPost, string oldStuff, DataTable dt, string nextDays)
        {
            InitializeComponent();

            workReport = report;
            workCompanies = companies;
            workMonth = month;
            workYear = year;
            workPreOrPost = preOrPost;
            workOldStuff = oldStuff;
            workDt = dt;

            if (!String.IsNullOrWhiteSpace(nextDays))
            {
                if (G1.validate_numeric(nextDays))
                    workNextDays = nextDays.ObjToInt32();
            }

            SetupTotalsSummary();

            DoSetup();

            btnSave.Hide();
        }
        /****************************************************************************************/
        public TrustDeceased()
        {
            InitializeComponent();
            SetupTotalsSummary();
        }
        /****************************************************************************************/
        public TrustDeceased( string contractNumnber )
        {
            workContract = contractNumnber;
            InitializeComponent();
            SetupTotalsSummary();
        }
        /****************************************************************************************/
        private void SetupTotalsSummary()
        {
            gridMain.OptionsView.ShowFooter = true;
            //AddSummaryColumn("value", null);
            gridMain.Columns["value"].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Custom;
            AddSummaryColumn("received", null);
            AddSummaryColumn("refunds", null);
            //AddSummaryColumn("principal", null);
            gridMain.Columns["principal"].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Custom;

            AddSummaryColumn("ourFiledAmount", null);
            AddSummaryColumn("overunder", null);
            AddSummaryColumn("dbr", null);
            AddSummaryColumn("trustOverUnder", null);
            AddSummaryColumn("reducedPaidUpAmount", null);
            AddSummaryColumn("sandraMoney");
            AddSummaryColumn("sandraPrincipal");
            AddSummaryColumn("difference");
            AddSummaryColumn("pdiff");


            AddSummaryColumn("beginningPaymentBalance", gridMain4);
            AddSummaryColumn("beginningDeathBenefit", gridMain4);
            AddSummaryColumn("endingPaymentBalance", gridMain4);
            AddSummaryColumn("endingDeathBenefit", gridMain4);
            AddSummaryColumn("downPayments", gridMain4);
            AddSummaryColumn("payments", gridMain4);
            AddSummaryColumn("growth", gridMain4);
            AddSummaryColumn("priorUnappliedCash", gridMain4);
            AddSummaryColumn("currentUnappliedCash", gridMain4);
            AddSummaryColumn("deathClaimAmount", gridMain4);
            AddSummaryColumn("endingBalance", gridMain4);

            //AddSummaryColumn("value", gridMain2);
            //AddSummaryColumn("received", gridMain2);

            gridMain2.Columns["value"].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Custom;
            gridMain2.Columns["value"].SummaryItem.DisplayFormat = "{0:N2}";

            gridMain2.Columns["received"].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Custom;
            gridMain2.Columns["received"].SummaryItem.DisplayFormat = "{0:N2}";

            //dRow["Forethought"] = G1.ReformatMoney(foreThought);            // beginningPaymentBalance
            //dRow["Security National"] = G1.ReformatMoney(securityNational); // beginningDeathBenefit
            //dRow["FDLIC Old Webb"] = G1.ReformatMoney(fdlicOldWebb);        // endingPaymentBalance
            //dRow["FDLIC Old CCI"] = G1.ReformatMoney(fdlicOldCCI);          // endingDeathBenefit
            //dRow["Unity Old Barham"] = G1.ReformatMoney(unityOldBarham);    // priorUnappliedCash
            //dRow["Unity Old Webb"] = G1.ReformatMoney(unityOldWebb);        // currentUnappliedCash

            AddSummaryColumn("Forethought", gridMain6);
            gridMain6.Columns["Forethought"].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Custom;
            gridMain6.Columns["Forethought"].SummaryItem.DisplayFormat = "{0:N2}";

            AddSummaryColumn("Security National", gridMain6);
            gridMain6.Columns["Security National"].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Custom;
            gridMain6.Columns["Security National"].SummaryItem.DisplayFormat = "{0:N2}";

            AddSummaryColumn("fdlicOldWebb", gridMain6);
            gridMain6.Columns["fdlicOldWebb"].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Custom;
            gridMain6.Columns["fdlicOldWebb"].SummaryItem.DisplayFormat = "{0:N2}";

            AddSummaryColumn("fdlicOldCCI", gridMain6);
            gridMain6.Columns["fdlicOldCCI"].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Custom;
            gridMain6.Columns["fdlicOldCCI"].SummaryItem.DisplayFormat = "{0:N2}";

            AddSummaryColumn("unityOldBarham", gridMain6);
            gridMain6.Columns["unityOldBarham"].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Custom;
            gridMain6.Columns["unityOldBarham"].SummaryItem.DisplayFormat = "{0:N2}";

            AddSummaryColumn("unityOldWebb", gridMain6);
            gridMain6.Columns["unityOldWebb"].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Custom;
            gridMain6.Columns["unityOldWebb"].SummaryItem.DisplayFormat = "{0:N2}";

            AddSummaryColumn("Security National", gridMain8);
            AddSummaryColumn("Forethought", gridMain8);
            AddSummaryColumn("Unity", gridMain8);
            AddSummaryColumn("FDLIC", gridMain8);
            AddSummaryColumn("total", gridMain8);
            AddSummaryColumn("endingBalance", gridMain8);

            AddSummaryColumn("amtActuallyReceived", gridMain9);


            //AddSummaryColumn("endingPaymentBalance", null);
            //AddSummaryColumn("endingDeathBenefit", null);
            //AddSummaryColumn("downPayments", null);
            //AddSummaryColumn("payments", null);
            //AddSummaryColumn("growth", null);
            //AddSummaryColumn("priorUnappliedCash", null);
            //AddSummaryColumn("currentUnappliedCash", null);
            //AddSummaryColumn("deathClaimAmount", null);


            //AddSummaryColumn("endingBalance", null);
            //AddSummaryColumn("overshort", null);
            //AddSummaryColumn("tSurrender", null);
            //AddSummaryColumn("refund", null);
            //AddSummaryColumn("surfacdiff", null);
        }
        /****************************************************************************************/
        private void AddSummaryColumn(string columnName)
        {
            gridMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            //gridMain.Columns[columnName].SummaryItem.DisplayFormat = "{0:0,0.00}";
            gridMain.Columns[columnName].SummaryItem.DisplayFormat = "{0:N2}";
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
        /*******************************************************************************************/
        private string getCompanyQuery(CheckedComboBoxEdit chkCompany = null)
        {
            string procLoc = "";
            string company = "";
            //string[] locIDs = chkCmbCompany.EditValue.ToString().Split('|');
            string[] locIDs = null;
            if (chkCompany != null)
                locIDs = chkCompany.EditValue.ToString().Split('|');
            for (int i = 0; i < locIDs.Length; i++)
            {
                if (!String.IsNullOrWhiteSpace(locIDs[i]))
                {
                    company = locIDs[i].Trim();
                    if (company == "Unity Barham" || company == "Unity Webb")
                        continue;
                    if (procLoc.Trim().Length > 0)
                        procLoc += ",";
                    procLoc += "'" + company + "'";
                }
            }
            return procLoc.Length > 0 ? " `trustCompany` IN (" + procLoc + ") " : "";
        }
        /****************************************************************************************/
        private void DoSetup()
        {
            if (workOldStuff.ToUpper() == "YES")
            {
                SetupDetailColumns();
                ClearAllPositions(gridMain);
                SetPrePositions();
            }
            else
            {
                SetupDetailColumns();
                ClearAllPositions(gridMain);
                SetPostPositions();
            }
        }
        /****************************************************************************************/
        private void SetPostPositions()
        {
            int i = 1;
            G1.SetColumnPosition(gridMain, "num", i++);
            G1.SetColumnPosition(gridMain, "month", i++);

            if (workReport == "Post 2002 Report - SN & FT")
            {
                G1.SetColumnPosition(gridMain, "Security National", i++);
                G1.SetColumnPosition(gridMain, "sn desc", i++);
                G1.SetColumnPosition(gridMain, "sn date", i++);
                G1.SetColumnPosition(gridMain, "sn contract", i++);
                G1.SetColumnPosition(gridMain, "sn funeral", i++);

                G1.SetColumnPosition(gridMain, "Forethought", i++);
                G1.SetColumnPosition(gridMain, "fore desc", i++);
                G1.SetColumnPosition(gridMain, "fore date", i++);
                G1.SetColumnPosition(gridMain, "fore contract", i++);
                G1.SetColumnPosition(gridMain, "fore funeral", i++);
            }
            else if (workReport == "Post 2002 Report - Unity")
            {
                G1.SetColumnPosition(gridMain, "Unity", i++);
                G1.SetColumnPosition(gridMain, "unity desc", i++);
                G1.SetColumnPosition(gridMain, "unity date", i++);
                G1.SetColumnPosition(gridMain, "unity contract", i++);
                G1.SetColumnPosition(gridMain, "unity funeral", i++);

                G1.SetColumnPosition(gridMain, "Unity PB", i++);
                G1.SetColumnPosition(gridMain, "unity pb desc", i++);
                G1.SetColumnPosition(gridMain, "unity pb date", i++);
                G1.SetColumnPosition(gridMain, "unity pb contract", i++);
                G1.SetColumnPosition(gridMain, "unity pb funeral", i++);

                G1.SetColumnPosition(gridMain, "unity trust", i++);
                G1.SetColumnPosition(gridMain, "unity trust desc", i++);
                G1.SetColumnPosition(gridMain, "unity trust date", i++);
                G1.SetColumnPosition(gridMain, "unity trust contract", i++);
                G1.SetColumnPosition(gridMain, "unity trust funeral", i++);
            }
            else if (workReport == "Post 2002 Report - FDLIC")
            {
                G1.SetColumnPosition(gridMain, "fdlic trust", i++);
                G1.SetColumnPosition(gridMain, "fdlic trust desc", i++);
                G1.SetColumnPosition(gridMain, "fdlic trust date", i++);
                G1.SetColumnPosition(gridMain, "fdlic trust contract", i++);
                G1.SetColumnPosition(gridMain, "fdlic trust funeral", i++);

                G1.SetColumnPosition(gridMain, "FDLIC", i++);
                G1.SetColumnPosition(gridMain, "fdlic desc", i++);
                G1.SetColumnPosition(gridMain, "fdlic date", i++);
                G1.SetColumnPosition(gridMain, "fdlic contract", i++);
                G1.SetColumnPosition(gridMain, "fdlic funeral", i++);

                G1.SetColumnPosition(gridMain, "FDLIC PB", i++);
                G1.SetColumnPosition(gridMain, "fdlic pb desc", i++);
                G1.SetColumnPosition(gridMain, "fdlic pb date", i++);
                G1.SetColumnPosition(gridMain, "fdlic pb contract", i++);
                G1.SetColumnPosition(gridMain, "fdlic pb funeral", i++);
            }
            else
            {
                G1.SetColumnPosition(gridMain, "CD", i++);
                G1.SetColumnPosition(gridMain, "CD desc", i++);
                G1.SetColumnPosition(gridMain, "CD date", i++);
                G1.SetColumnPosition(gridMain, "CD contract", i++);
                G1.SetColumnPosition(gridMain, "CD funeral", i++);

                G1.SetColumnPosition(gridMain, "fdlicOldWebb", i++);
                G1.SetColumnPosition(gridMain, "fdlicOldCCI", i++);
                G1.SetColumnPosition(gridMain, "fdlic desc", i++);
                G1.SetColumnPosition(gridMain, "fdlic date", i++);
                G1.SetColumnPosition(gridMain, "fdlic contract", i++);
                G1.SetColumnPosition(gridMain, "fdlic funeral", i++);

                G1.SetColumnPosition(gridMain, "unityOldBarham", i++);
                G1.SetColumnPosition(gridMain, "unityOldWebb", i++);
                G1.SetColumnPosition(gridMain, "unity desc", i++);
                G1.SetColumnPosition(gridMain, "unity date", i++);
                G1.SetColumnPosition(gridMain, "unity contract", i++);
                G1.SetColumnPosition(gridMain, "unity funeral", i++);
            }

            G1.SetColumnPosition(gridMain, "total", i++);
        }
        /****************************************************************************************/
        private void SetPrePositions()
        {
            int i = 1;
            G1.SetColumnPosition(gridMain, "num", i++);
            G1.SetColumnPosition(gridMain, "month", i++);

            G1.SetColumnPosition(gridMain, "CD", i++);
            G1.SetColumnPosition(gridMain, "CD desc", i++);
            G1.SetColumnPosition(gridMain, "CD date", i++);
            G1.SetColumnPosition(gridMain, "CD contract", i++);
            G1.SetColumnPosition(gridMain, "CD funeral", i++);

            G1.SetColumnPosition(gridMain, "fdlicOldWebb", i++);
            G1.SetColumnPosition(gridMain, "fdlicOldCCI", i++);
            G1.SetColumnPosition(gridMain, "fdlic desc", i++);
            G1.SetColumnPosition(gridMain, "fdlic date", i++);
            G1.SetColumnPosition(gridMain, "fdlic contract", i++);
            G1.SetColumnPosition(gridMain, "fdlic funeral", i++);

            G1.SetColumnPosition(gridMain, "Forethought", i++);
            G1.SetColumnPosition(gridMain, "fore desc", i++);
            G1.SetColumnPosition(gridMain, "fore date", i++);
            G1.SetColumnPosition(gridMain, "fore contract", i++);
            G1.SetColumnPosition(gridMain, "fore funeral", i++);


            if (workReport != "Post 2002 Report - Unity")
            {
                G1.SetColumnPosition(gridMain, "unityOldBarham", i++);
                G1.SetColumnPosition(gridMain, "unityOldWebb", i++);
                G1.SetColumnPosition(gridMain, "unity desc", i++);
                G1.SetColumnPosition(gridMain, "unity date", i++);
                G1.SetColumnPosition(gridMain, "unity contract", i++);
                G1.SetColumnPosition(gridMain, "unity funeral", i++);
            }

            if (workReport == "Post 2002 Report - Unity")
            {
                G1.SetColumnPosition(gridMain, "unity", i++);
                G1.SetColumnPosition(gridMain, "unity desc", i++);
                G1.SetColumnPosition(gridMain, "unity date", i++);
                G1.SetColumnPosition(gridMain, "unity contract", i++);
                G1.SetColumnPosition(gridMain, "unity funeral", i++);

                G1.SetColumnPosition(gridMain, "unity pb", i++);
                G1.SetColumnPosition(gridMain, "unity pb desc", i++);
                G1.SetColumnPosition(gridMain, "unity pb date", i++);
                G1.SetColumnPosition(gridMain, "unity pb contract", i++);
                G1.SetColumnPosition(gridMain, "unity pb funeral", i++);
            }
            else if (workReport == "Post 2002 Report - FDLIC")
            {
                G1.SetColumnPosition(gridMain, "fdlic", i++);
                G1.SetColumnPosition(gridMain, "fdlic desc", i++);
                G1.SetColumnPosition(gridMain, "fdlic date", i++);
                G1.SetColumnPosition(gridMain, "fdlic contract", i++);
                G1.SetColumnPosition(gridMain, "fdlic funeral", i++);

                G1.SetColumnPosition(gridMain, "fdlic pb", i++);
                G1.SetColumnPosition(gridMain, "fdlic pb desc", i++);
                G1.SetColumnPosition(gridMain, "fdlic pb date", i++);
                G1.SetColumnPosition(gridMain, "fdlic pb contract", i++);
                G1.SetColumnPosition(gridMain, "fdlic pb funeral", i++);
            }

            G1.SetColumnPosition(gridMain, "Security National", i++);
            G1.SetColumnPosition(gridMain, "sn desc", i++);
            G1.SetColumnPosition(gridMain, "sn date", i++);
            G1.SetColumnPosition(gridMain, "sn contract", i++);
            G1.SetColumnPosition(gridMain, "sn funeral", i++);


            G1.SetColumnPosition(gridMain, "total", i++);
        }
        /****************************************************************************************/
        private void SetupDetailColumns()
        {
            AddNewColumn("sn desc", "Description", 150);
            AddNewColumn("sn date", "Date", 100);
            AddNewColumn("sn contract", "Contract #", 100);
            AddNewColumn("sn funeral", "Funeral #", 100);

            AddNewColumn("fore desc", "Description", 150);
            AddNewColumn("fore date", "Date", 100);
            AddNewColumn("fore contract", "Contract #", 100);
            AddNewColumn("fore funeral", "Funeral #", 100);

            AddNewColumn("cd desc", "Description", 150);
            AddNewColumn("cd date", "Date", 100);
            AddNewColumn("cd contract", "Contract #", 100);
            AddNewColumn("cd funeral", "Funeral #", 100);

            AddNewColumn("unity desc", "Description", 150);
            AddNewColumn("unity date", "Date", 100);
            AddNewColumn("unity contract", "Contract #", 100);
            AddNewColumn("unity funeral", "Funeral #", 100);

            if (workReport == "Post 2002 Report - Unity")
            {
                AddNewColumn("unity trust", "Unity SMFS", 100);
                AddNewColumn("unity trust desc", "Description SMFS", 150);
                AddNewColumn("unity trust date", "Date SMFS", 100);
                AddNewColumn("unity trust contract", "Contract # SMFS", 100);
                AddNewColumn("unity trust funeral", "Funeral # SMFS", 100);
            }

            AddNewColumn("unity pb desc", "Description", 150);
            AddNewColumn("unity pb date", "Date", 100);
            AddNewColumn("unity pb contract", "Contract #", 100);
            AddNewColumn("unity pb funeral", "Funeral #", 100);

            if (workReport == "Post 2002 Report - FDLIC")
            {
                AddNewColumn("fdlic trust desc", "Description SMFS", 150);
                AddNewColumn("fdlic trust date", "Date SMFS", 100);
                AddNewColumn("fdlic trust contract", "Contract # SMFS", 100);
                AddNewColumn("fdlic trust funeral", "Funeral # SMFS", 100);
            }

            AddNewColumn("fdlic desc", "Description", 150);
            AddNewColumn("fdlic date", "Date", 100);
            AddNewColumn("fdlic contract", "Contract #", 100);
            AddNewColumn("fdlic funeral", "Funeral #", 100);

            AddNewColumn("fdlic pb desc", "Description", 150);
            AddNewColumn("fdlic pb date", "Date", 100);
            AddNewColumn("fdlic pb contract", "Contract #", 100);
            AddNewColumn("fdlic pb funeral", "Funeral #", 100);
        }
        /****************************************************************************************/
        private void AddNewColumn(string fieldName, string caption, int width, FormatType type)
        {
            if (G1.get_column_number(gridMain, fieldName) < 0)
                G1.AddNewColumn(gridMain, fieldName, caption, "", type, width, true);
            else
                gridMain.Columns[fieldName].Visible = true;
            G1.SetColumnWidth(gridMain, fieldName, width);
            gridMain.Columns[fieldName].OptionsColumn.FixedWidth = true;
            gridMain.Columns[fieldName].AppearanceHeader.ForeColor = Color.Black;
        }
        /****************************************************************************************/
        private void AddNewColumn(string fieldName, string caption, int width, DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView myGrid = null)
        {
            DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView grid = gridMain;
            if (myGrid != null)
                grid = myGrid;

            if (G1.get_column_number(grid, fieldName) < 0)
                G1.AddNewColumn(grid, fieldName, caption, "", FormatType.None, width, true);
            else
                grid.Columns[fieldName].Visible = true;
            G1.SetColumnWidth(grid, fieldName, width);
            grid.Columns[fieldName].OptionsColumn.FixedWidth = true;
            grid.Columns[fieldName].AppearanceHeader.ForeColor = Color.Black;
        }
        /****************************************************************************************/
        private void ClearAllPositions(DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gMain = null)
        {
            if (gMain == null)
                gMain = gridMain;
            for (int i = 0; i < gMain.Columns.Count; i++)
            {
                gMain.Columns[i].Visible = false;
                //gMain.Columns[i].VisibleIndex = 0;
                //gMain.Columns[i].AbsoluteIndex = 0;
            }
        }
        /****************************************************************************************/
        private void TrustDeceased_Load(object sender, EventArgs e)
        {
            try
            {
                btnAccept.Hide();

                G1.SetupToolTip(picMainDelete, "Delete Split");
                G1.SetupToolTip(btnMainInsert, "Split Row");

                dgv2.Dock = DockStyle.Fill;
                dgv6.Hide();
                dgv6.Dock = DockStyle.Fill;

                dgv10.Hide();
                dgv10.Dock = DockStyle.Fill;

                LoadStartDates();

                loadTrustCompanies();

                //this.Cursor = Cursors.WaitCursor;

                //string title = "Edit Trust Deceased for " + workMonth;
                //if (workIndependent)
                //    title = "Edit Trust Deceased";
                //this.Text = title;

                //if (workIndependent)
                //    return;

                //string month = "";
                //string year = "";

                //int iMonth = G1.ConvertMonthToIndex(workMonth);
                //int iYear = workYear.ObjToInt32();

                //int days = DateTime.DaysInMonth(iYear, iMonth);

                //DateTime date1 = new DateTime(iYear, iMonth, 1);
                //DateTime date2 = new DateTime(iYear, iMonth, days);

                //DataTable newDt = workDt.Clone();
                //int row = -1;
                //for ( int i=0; i<workDt.Rows.Count; i++)
                //{
                //    month = workDt.Rows[i]["month"].ObjToString();
                //    year = workDt.Rows[i]["year"].ObjToString();

                //    if ( month == workMonth && year == workYear )
                //    {
                //        row = i;
                //        if ( i > 0 )
                //            G1.copy_dt_row(workDt, i-1, newDt, 0);
                //        break;
                //    }
                //}

                //newDt = LoadDeceased(newDt, workDt, row );

                //if ( row >= 0 )
                //    G1.copy_dt_row(workDt, row, newDt, newDt.Rows.Count );

                //dgv.DataSource = newDt;
                //this.Cursor = Cursors.Default;
            }
            catch (Exception ex)
            {
            }

            if ( !String.IsNullOrWhiteSpace ( workContract ))
            {
                txtContract.Text = workContract;
                tabControl1.SelectedTab = tabPage7;
                button6_Click( null, null );
                dgv8.Refresh();
            }
        }
        /***********************************************************************************************/
        private void LoadStartDates ()
        {
            DateTime now = DateTime.Now;
            DateTime startDate = new DateTime(now.Year, now.Month, 1);

            int days = DateTime.DaysInMonth(now.Year, now.Month);

            DateTime stopDate = new DateTime(now.Year, now.Month, days);

            dateTimePicker5.Value = stopDate;

            DateTime dateLastMonthBOM = startDate.AddMonths(-1);
            days = DateTime.DaysInMonth(dateLastMonthBOM.Year, dateLastMonthBOM.Month);
            DateTime dateLastMonthEOM = new DateTime(dateLastMonthBOM.Year, dateLastMonthBOM.Month, days);

            dateTimePicker3.Value = dateLastMonthBOM;
            dateTimePicker4.Value = dateLastMonthEOM;
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

            //chkDiffCompanies.Properties.DataSource = dt;
            //chkDeathCompanies.Properties.DataSource = dt;
        }
        /****************************************************************************************/
        private void AddUnityColumns()
        {
            if (!chkIncludeDetails.Checked)
            {
                if (G1.get_column_number(gridMain, "fun_AmtFiled") >= 0)
                {
                    GridColumn col = gridMain.Columns["fun_AmtFiled"];
                    gridMain.Columns.Remove(col);
                    col = gridMain.Columns["fun_DateFiled"];
                    gridMain.Columns.Remove(col);
                    col = gridMain.Columns["fun_AmtReceived"];
                    gridMain.Columns.Remove(col);
                    col = gridMain.Columns["u_faceAmount"];
                    gridMain.Columns.Remove(col);
                    col = gridMain.Columns["u_endingDeathBenefit"];
                    gridMain.Columns.Remove(col);
                    col = gridMain.Columns["u_beginningDeathBenefit"];
                    gridMain.Columns.Remove(col);
                    col = gridMain.Columns["u_deathClaimAmount"];
                    gridMain.Columns.Remove(col);
                }
                return;
            }

            AddNewColumn("fun_AmtFiled", "fun AmtFiled", 100, FormatType.Numeric);
            AddNewColumn("fun_DateFiled", "fun DateFiled", 100, FormatType.DateTime);
            AddNewColumn("fun_AmtReceived", "fun AmtReceived", 100, FormatType.Numeric);
            AddNewColumn("u_faceAmount", "Face Amount", 100, FormatType.Numeric);
            AddNewColumn("u_endingDeathBenefit", "Death Benefit", 100, FormatType.Numeric);
            AddNewColumn("u_beginningDeathBenefit", "Current Cash Received", 100, FormatType.Numeric);
            AddNewColumn("u_deathClaimAmount", "Death Claim Amount", 100, FormatType.Numeric);

            AddSummaryColumn("fun_AmtFiled", gridMain);
            AddSummaryColumn("fun_AmtReceived", gridMain);
            AddSummaryColumn("u_faceAmount", gridMain);
            AddSummaryColumn("u_endingDeathBenefit", gridMain);
            AddSummaryColumn("u_beginningDeathBenefit", gridMain);
            AddSummaryColumn("u_deathClaimAmount", gridMain);
        }
        /****************************************************************************************/
        private DataTable LoadDeceased(DateTime startDate, DateTime stopDate, string report, ref DataTable newDt)
        {
            workReport = report;
            workDate = stopDate;

            DataTable dx = null;
            DataTable ddx = null;
            DataTable dddd = null;
            DataRow[] dRows = null;
            DataRow dRow = null;

            DateTime date1 = startDate;
            DateTime date2 = stopDate;

            string sDate1 = date1.ToString("yyyy-MM-dd");
            string sDate2 = date2.ToString("yyyy-MM-dd");

            string contractNumber = "";
            string paidFrom = "";
            string company = "";
            string newCompany = "";
            string contract = "";
            string preOrPost = cmbPreOrPost.Text.Trim();
            string masterPreOrPost = preOrPost;

            string companies = getCompanyQuery(workCompanies);

            string[] locIDs = workCompanies.EditValue.ToString().Split('|');
            if (locIDs.Length > 0)
                this.Text = workReport + " Trust Deceased for " + date2.ToString("MMMMMM") + ", " + date2.Year.ToString();

            DateTime dateReceived = DateTime.Now;
            DateTime dateFiled = DateTime.Now;
            DateTime reportDate = DateTime.Now;
            DateTime date = DateTime.Now;

            DateTime maxDate = date2.AddDays(1);
            DateTime minDate = date1.AddDays(-1);
            int previousMonth = minDate.Month;

            int days = DateTime.DaysInMonth(maxDate.Year, maxDate.Month);
            DateTime nextMonth = new DateTime(maxDate.Year, maxDate.Month, days);

            string cmd = "Select * from `trust_data_overruns` t WHERE `date` >= '" + maxDate.ToString("yyyy-MM-dd") + "' AND `date` <= '" + nextMonth.ToString("yyyy-MM-dd") + " 23:59:59' ";
            if (!String.IsNullOrWhiteSpace(companies))
            {
                newCompany = companies;
                cmd += " AND " + newCompany + " ";
            }
            cmd += " ORDER BY `date` DESC ;";
            DataTable nextDt = G1.get_db_data(cmd); // This allows for reruns so you don't have to remember what to set Next Month Days to Include
            if (nextDt.Rows.Count > 0)
            {
                nextMonth = nextDt.Rows[0]["date"].ObjToDateTime();
                days = nextMonth.Day;
                if (days > workNextDays)
                {
                    if ( !chkIgnoreOverruns.Checked )
                        workNextDays = days;
                }
            }

            DateTime date3 = date2;
            date3 = date3.AddDays(workNextDays);
            string sDate3 = date3.ToString("yyyy-MM-dd");
            DataTable dt = null;

            cmd = "Select * from `trust_data` WHERE `deathPaidDate` >= '" + date1.ToString("yyyy-MM-dd") + "' AND `deathPaidDate` <= '" + date2.ToString("yyyy-MM-dd") + " 23:59:59' ";
            if (companies.Contains("Unity"))
            {
                cmd = "Select * from `trust_data` WHERE `date` >= '" + date1.ToString("yyyy-MM-dd") + "' AND `date` <= '" + date2.ToString("yyyy-MM-dd") + " 23:59:59' AND `statusReason` = 'DC' ";

                cmd = "SELECT * FROM `trust_data` WHERE `deathPaidDate` >= '" + sDate1 + "' AND `deathPaidDate` <= '" + sDate2 + " 23:59:59' AND `statusReason` = 'DC' AND `preOrPost` = '" + preOrPost + "' ";
                //cmd = "SELECT * FROM `trust_data` t JOIN cust_payment_details c ON t.`contractNumber` = c.`contractNumber` WHERE `date` >= '" + sDate1 + "' AND `date` <= '" + sDate2 + " 23:59:59' AND `statusReason` = 'DC' AND c.`status` = 'Deposited' AND `type` = 'Trust' AND `paidFrom` = 'Unity' AND c.`dateFiled` >= '" + sDate1 + "' AND c.`dateFiled` <= '" + sDate2 + "' ";
            }
            else if (companies.Contains("FDLIC"))
            {
                cmd = "SELECT * FROM `trust_data` WHERE `deathPaidDate` >= '" + sDate1 + "' AND `deathPaidDate` <= '" + sDate2 + " 23:59:59' AND `preOrPost` = '" + preOrPost + "' ";
                //cmd = "SELECT * FROM `trust_data` t LEFT JOIN cust_payment_details c ON t.`contractNumber` = c.`contractNumber` WHERE `date` >= '" + sDate1 + "' AND `date` <= '" + sDate2 + " 23:59:59' AND c.`status` = 'Deposited' AND `type` = 'Trust' AND `paidFrom` = 'FDLIC' AND c.`dateReceived` >= '" + sDate1 + "' AND c.`dateReceived` <= '" + sDate3 + "' ";
            }
            else if (workReport.IndexOf("2002 Report - SN & FT") > 0)
            {
                cmd = "Select * FROM `trust_data` t WHERE t.`deathPaidDate` >= '" + sDate1 + "' AND t.`deathPaidDate` <= '" + sDate3 + " 23:59:59' AND t.`preOrPost` = '" + preOrPost + "' ";
                if (!String.IsNullOrWhiteSpace(companies))
                {
                    newCompany = companies;
                    cmd += " AND " + newCompany + " ";
                }
                cmd += " ORDER by `date` desc;  ";
                //cmd = cmd.Replace("`trustCompany`", "`paidFrom`");

                dt = G1.get_db_data(cmd);
            }
            if (workReport == "Pre 2002 Report")
            {
                cmd = "Select * FROM `trust_data` t WHERE t.`deathPaidDate` >= '" + sDate1 + "' AND t.`deathPaidDate` <= '" + sDate3 + " 23:59:59' AND t.`preOrPost` = 'Pre' ";
                //cmd = "SELECT * FROM `trust_data` t JOIN cust_payment_details c ON t.`contractNumber` = c.`contractNumber` WHERE `deathPaidDate` >= '" + sDate1 + "' AND t.`preOrPost` = 'Pre' AND `deathPaidDate` <= '" + sDate3 + " 23:59:59' AND c.`status` = 'Deposited' AND `type` = 'Trust' AND c.`dateReceived` >= '" + sDate1 + "' AND c.`dateReceived` <= '" + sDate3 + "' ";
                if (!String.IsNullOrWhiteSpace(companies))
                {
                    newCompany = companies;
                    cmd += " AND " + newCompany + " ";
                }
                cmd += " ORDER by `date` desc;  ";
                //cmd = cmd.Replace("`trustCompany`", "`paidFrom`");

                dt = G1.get_db_data(cmd);
            }
            if (workReport.IndexOf("2002 Report - SN & FT") < 0 && workReport != "Pre 2002 Report")
            {
                if (!String.IsNullOrWhiteSpace(companies))
                    cmd += " AND " + companies + " ";
                cmd += " ORDER by `deathPaidDate` ";
                cmd += ";";

                dt = G1.get_db_data(cmd);
            }

            for ( int i=dt.Rows.Count-1; i>=0; i--)
            {
                date = dt.Rows[i]["reportDate"].ObjToDateTime();
                if ( date.Year > 1000 )
                {
                    if (date < date1 || date > date2)
                        dt.Rows.RemoveAt(i);
                }
            }

            cmd = "Select * FROM `trust_data_edits` t WHERE t.`date` = '" + sDate2 + "' AND t.`preOrPost` = '" + preOrPost + "' AND `policyStatus` = 'SPLIT' ";
            if (!String.IsNullOrWhiteSpace(companies))
            {
                newCompany = companies;
                cmd += " AND " + newCompany + " ";
            }
            cmd += " ORDER by `date` desc;  ";
            DataTable ddt = G1.get_db_data(cmd);

            ddt.Columns.Add("manual");

            DateTime sDate = DateTime.Now;
            string str = "";
            string lastName = "";
            for (int i = 0; i < ddt.Rows.Count; i++)
            {
                lastName = ddt.Rows[i]["lastName"].ObjToString().Trim();
                str = ddt.Rows[i]["insuredName"].ObjToString().Trim();
                if (str.IndexOf("PD -") == 0 && !String.IsNullOrWhiteSpace ( lastName ) )
                    str = lastName;
                ddt.Rows[i]["lastName"] = str;
                ddt.Rows[i]["manual"] = "Y";
            }

            if (ddt.Rows.Count > 0)
            {
                dt.Merge(ddt);
                DataView tempview2 = dt.DefaultView;
                tempview2.Sort = "date desc";
                dt = tempview2.ToTable();
            }


            dt = TrustData.LookupTrusts(dt);

            DataTable majorDt = dt.Copy();

            dt.Columns.Add("fun_AmtFiled", Type.GetType("System.Double"));
            dt.Columns.Add("fun_DateFiled", Type.GetType("System.String"));
            dt.Columns.Add("fun_AmtReceived", Type.GetType("System.Double"));
            dt.Columns.Add("u_faceAmount", Type.GetType("System.Double"));
            dt.Columns.Add("u_endingDeathBenefit", Type.GetType("System.Double"));
            dt.Columns.Add("u_beginningDeathBenefit", Type.GetType("System.Double"));
            dt.Columns.Add("u_deathClaimAmount", Type.GetType("System.Double"));

            dt.Columns.Add("sandraMoney", Type.GetType("System.Double"));
            dt.Columns.Add("sandraPrincipal", Type.GetType("System.Double"));
            dt.Columns.Add("difference", Type.GetType("System.Double"));
            dt.Columns.Add("pdiff", Type.GetType("System.Double"));

            //gridMain.Columns["sandraMoney"].Visible = true;
            //gridMain.Columns["sandraPrincipal"].Visible = true;

            dt.Columns.Add("overunder", Type.GetType("System.Double"));
            dt.Columns.Add("dbr", Type.GetType("System.Double"));
            dt.Columns.Add("tbb", Type.GetType("System.Double"));
            dt.Columns.Add("trustOverUnder", Type.GetType("System.Double"));

            cmd = "Select * from `trust_data` t WHERE `reportDate` >= '" + sDate1 + "' AND `reportDate` <= '" + sDate2 + " 23:59:59' ";

            if (workReport == "Pre 2002 Report")
                cmd += " AND t.`preOrPost` = 'Pre' ";
            else
                cmd += " AND t.`preOrPost` <> 'Pre' ";

            if (!String.IsNullOrWhiteSpace(companies))
            {
                newCompany = companies;
                if (newCompany.IndexOf("FDLIC PB") >= 0)
                    newCompany = newCompany.Replace("'FDLIC PB'", "'FDLIC PB','FDLIC CCI'");
                cmd += " AND " + newCompany + " ";
            }
            cmd += " ORDER by `date`, `trustCompany`;  ";

            ddx = G1.get_db_data(cmd);

            dRows = null;
            //for (int i = 0; i < ddx.Rows.Count; i++)
            //{
            //    contractNumber = ddx.Rows[i]["contractNumber"].ObjToString();
            //    reportDate = ddx.Rows[i]["reportDate"].ObjToDateTime();
            //    dRows = dt.Select("contractNumber='" + contractNumber + "'");
            //    if (dRows.Length > 0)
            //    {
            //        for (int j = 0; j < dRows.Length; j++)
            //        {
            //            date = dRows[j]["date"].ObjToDateTime();
            //            if (date == reportDate)
            //            {
            //                dRows[j]["date"] = G1.DTtoMySQLDT(reportDate);
            //            }
            //        }
            //    }
            //    else
            //    {
            //        ddx.Rows[i]["date"] = G1.DTtoMySQLDT(reportDate.ToString("yyyy-MM-dd"));
            //        dt.ImportRow(ddx.Rows[i]);
            //    }
            //}


            //cmd = "Select * from `trust_data_edits` WHERE `date` >= '" + sDate1 + "' AND `date` <= '" + sDate2 + "' ";
            //cmd += " AND `trustCompany` LIKE 'FDLIC%' ";
            //cmd += " ORDER by `date`, `trustCompany`;  ";

            //ddx = G1.get_db_data(cmd);
            //ddx.Columns.Add("manual");
            //for (int i = 0; i < ddx.Rows.Count; i++)
            //    ddx.Rows[i]["manual"] = "Y";
            //if (ddx.Rows.Count > 0)
            //    dt.Merge(ddx);


            double dValue = 0D;
            try
            {
                //DataTable tempDt = CreateTempDt();
                //tempDt.Columns.Add("record");
                dt.Columns.Add("month");
                dt.Columns.Add("desc");
                //tempDt.Columns.Add("date");
                dt.Columns.Add("contract");
                dt.Columns.Add("funeral");
                dt.Columns.Add("value", Type.GetType("System.Double"));
                dt.Columns.Add("refunds", Type.GetType("System.Double"));
                dt.Columns.Add("principal", Type.GetType("System.Double"));
                dt.Columns.Add("ourFiledAmount", Type.GetType("System.Double"));
                dt.Columns.Add("balance", Type.GetType("System.Double"));
                dt.Columns.Add("first", Type.GetType("System.Double"));
                //tempDt.Columns.Add("policyNumber");
                dt.Columns.Add("trust");
                if (G1.get_column_number(dt, "dateReceived") < 0)
                    dt.Columns.Add("dateReceived");
                dt.Columns.Add("received", Type.GetType("System.Double"));
                //tempDt.Columns.Add("deathClaimAmount", Type.GetType("System.Double"));
                //dt.Columns.Add("reportDate");
                dt.Columns.Add("smfsBalance", Type.GetType("System.Double"));
                dt.Columns.Add("ftBalance", Type.GetType("System.Double"));
                if (G1.get_column_number(dt, "manual") < 0)
                    dt.Columns.Add("manual");

                contractNumber = "";
                double endingDeathBenefit = 0D;
                double beginningDeathBenefit = 0D;
                double beginningPaymentBalance = 0D;
                double faceAmount = 0D;
                double deathClaimAmount = 0D;

                double totalPaidInAtClaim = 0D;
                double payments = 0D;
                double value = 0D;
                string firstName = "";
                lastName = "";
                string insuredName = "";
                double trustAmtFiled = 0D;
                double amtActuallyReceived = 0D;
                double received = 0D;
                double growth = 0D;
                double rpu = 0D;
                string policyNumber = "";
                string trustCompany = "";
                string type = "";
                string funeral = "";
                string paymentRecord = "";
                double principal = 0D;
                string record = "";
                double ourFiledAmount = 0D;
                double refund = 0D;
                double overUnder = 0D;
                double dbr = 0D;
                double them = 0D;
                double us = 0D;
                double trustOverUnder = 0D;
                double tbb = 0D;
                string prePostWhat = "";
                bool gotFuneral = false;
                string policyStatus = "";

                dt = verifyContracts(dt);

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    try
                    {
                        dbr = 0D;
                        trustAmtFiled = 0D;
                        contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                        if (contractNumber == "C24042LI")
                        {
                        }
                        if (contractNumber == "P15099UI")
                        {
                        }

                        insuredName = dt.Rows[i]["insuredName"].ObjToString().Trim();
                        dt.Rows[i]["insuredName"] = insuredName;
                        policyStatus = dt.Rows[i]["policyStatus"].ObjToString().ToUpper();
                        policyNumber = dt.Rows[i]["policyNumber"].ObjToString();
                        trustCompany = dt.Rows[i]["trustCompany"].ObjToString();
                        if (policyStatus != "SPLIT" )
                        {
                            contract = FindContractNumber(policyNumber, trustCompany, ref type);
                            if (!String.IsNullOrWhiteSpace(contract))
                            {
                                if (contract != contractNumber)
                                    contractNumber = contract;
                            }
                        }
                        else
                        {
                            dValue = dt.Rows[i]["currentUnappliedCash"].ObjToDouble();
                            if (dValue != 0D)
                                dt.Rows[i]["received"] = dValue;
                        }
                        dt.Rows[i]["contract"] = contractNumber;
                        funeral = getFuneralService(contractNumber);
                        dt.Rows[i]["funeral"] = funeral;


                        firstName = dt.Rows[i]["firstName"].ObjToString();
                        lastName = dt.Rows[i]["lastName"].ObjToString();
                        if (!String.IsNullOrWhiteSpace(lastName))
                            insuredName = firstName + " " + lastName;
                        else
                            insuredName = dt.Rows[i]["insuredName"].ObjToString();
                        dt.Rows[i]["insuredName"] = insuredName;

                        faceAmount = dt.Rows[i]["endingPaymentBalance"].ObjToDouble();
                        deathClaimAmount = dt.Rows[i]["deathClaimAmount"].ObjToDouble();


                        endingDeathBenefit = 0D;
                        if (workReport == "Post 2002 Report - Unity")
                            endingDeathBenefit = dt.Rows[i]["beginningDeathBenefit"].ObjToDouble();
                        if (workReport == "Pre 2002 Report")
                            endingDeathBenefit = dt.Rows[i]["endingDeathBenefit"].ObjToDouble();
                        if (endingDeathBenefit <= 0D)
                            endingDeathBenefit = dt.Rows[i]["beginningPaymentBalance"].ObjToDouble();
                        if (endingDeathBenefit <= 0D)
                            endingDeathBenefit = dt.Rows[i]["downPayments"].ObjToDouble();
                        if (endingDeathBenefit <= 0D)
                            endingDeathBenefit = dt.Rows[i]["endingPaymentBalance"].ObjToDouble();
                        if (workReport == "Post 2002 Report - SN & FT" && endingDeathBenefit <= 0D)
                        {
                            endingDeathBenefit = dt.Rows[i]["endingDeathBenefit"].ObjToDouble();
                        }
                        value = endingDeathBenefit;
                        totalPaidInAtClaim = dt.Rows[i]["totalPaidInAtClaim"].ObjToDouble();

                        //if (dt.Rows[i]["reducedPaidUpAmount"].ObjToDouble() > 0D)
                        //    value = dt.Rows[i]["reducedPaidUpAmount"].ObjToDouble();
                        payments = dt.Rows[i]["Payments"].ObjToDouble();
                        dt.Rows[i]["value"] = value + payments;
                        if (dt.Rows[i]["reducedPaidUpAmount"].ObjToDouble() > 0D)
                            dt.Rows[i]["value"] = dt.Rows[i]["reducedPaidUpAmount"].ObjToDouble();
                        if (workReport == "Post 2002 Report - FDLIC")
                        {
                            dbr = getDBR(contractNumber);
                        }

                        dt.Rows[i]["dbr"] = dbr;
                        dt.Rows[i]["desc"] = insuredName;
                        dt.Rows[i]["lastName"] = lastName;
                        dt.Rows[i]["firstName"] = firstName;
                        dt.Rows[i]["trust"] = dt.Rows[i]["trustCompany"].ObjToString();
                        if (workReport == "Post 2002 Report - FDLIC")
                            dt.Rows[i]["principal"] = totalPaidInAtClaim;

                        //tbb = getTBB(date2, contractNumber);
                        //dt.Rows[i]["ourFiledAmount"] = tbb;

                        funeral = dt.Rows[i]["funeral"].ObjToString();

                        dt.Rows[i]["u_faceAmount"] = faceAmount;
                        dt.Rows[i]["u_endingDeathBenefit"] = endingDeathBenefit;
                        dt.Rows[i]["u_beginningDeathBenefit"] = beginningDeathBenefit;
                        dt.Rows[i]["u_deathClaimAmount"] = deathClaimAmount;
                        //dt.Rows[i]["fun_AmtFiled"] = dt.Rows[i]["value"].ObjToDouble();

                        if (workReport == "Post 2002 Report - SN & FT" )
                            dt.Rows[i]["value"] = deathClaimAmount;

                        if ( policyStatus == "SPLIT")
                        {
                            principal = dt.Rows[i]["beginningDeathBenefit"].ObjToDouble();
                            dt.Rows[i]["principal"] = principal;
                            dt.Rows[i]["received"] = principal;
                            //if (workReport == "Post 2002 Report - SN & FT")
                            //    dt.Rows[i]["value"] = dt.Rows[i]["beginningPaymentBalance"].ObjToDouble();
                            value = dt.Rows[i]["beginningPaymentBalance"].ObjToDouble();
                            dt.Rows[i]["value"] = value;

                            dValue = dt.Rows[i]["currentUnappliedCash"].ObjToDouble();
                            if (dValue != 0D)
                                dt.Rows[i]["received"] = dValue;
                        }
                        if (funeral.ToUpper().IndexOf("OS") >= 0 || funeral.ToUpper().IndexOf("O/S") >= 0)
                        {
                            principal = dt.Rows[i]["beginningDeathBenefit"].ObjToDouble();
                            if (principal <= 0D)
                                principal = dt.Rows[i]["endingPaymentBalance"].ObjToDouble();
                            if (principal > 0D)
                                trustAmtFiled = principal;
                            if (dt.Rows[i]["reducedPaidUpAmount"].ObjToDouble() > 0D)
                                trustAmtFiled = dt.Rows[i]["reducedPaidUpAmount"].ObjToDouble();
                            dt.Rows[i]["principal"] = trustAmtFiled;
                            if (workReport == "Post 2002 Report - FDLIC")
                                dt.Rows[i]["principal"] = totalPaidInAtClaim;
                            dt.Rows[i]["received"] = trustAmtFiled;
                            if (dt.Rows[i]["received"].ObjToDouble() == 0D)
                                dt.Rows[i]["received"] = dt.Rows[i]["value"].ObjToDouble();
                            if (dt.Rows[i]["principal"].ObjToDouble() == 0D)
                                dt.Rows[i]["principal"] = dt.Rows[i]["value"].ObjToDouble();
                            if (policyStatus == "SPLIT")
                            {
                                dValue = dt.Rows[i]["currentUnappliedCash"].ObjToDouble();
                                if (dValue != 0D)
                                    dt.Rows[i]["received"] = dValue;
                            }
                            cmd = "Select * from `cust_extended` where `contractNumber` = '" + contractNumber + "';";
                            ddx = G1.get_db_data(cmd);
                            if (ddx.Rows.Count > 0)
                                dt.Rows[i]["dateReceived"] = G1.DTtoMySQLDT(ddx.Rows[0]["serviceDate"].ObjToDateTime().ToString("yyyy-MM-dd"));
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }

                //dt = AddMissing(dt, majorDt);

                string oldContractNumber = "";
                string oldPolicyNumber = "";
                string middleName = "";
                rpu = 0D;
                amtActuallyReceived = 0D;
                faceAmount = 0D;
                endingDeathBenefit = 0D;
                beginningDeathBenefit = 0D;
                deathClaimAmount = 0D;
                dValue = 0D;
                value = 0D;
                trustAmtFiled = 0D;
                principal = 0D;
                Trust85.FindContract(dt, "WM22025LI");
                for (int i = dt.Rows.Count - 1; i >= 0; i--)
                {
                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                    if (contractNumber == "P15099UI")
                    {
                    }

                    policyStatus = dt.Rows[i]["policyStatus"].ObjToString().ToUpper();
                    policyNumber = dt.Rows[i]["policyNumber"].ObjToString();
                    if (policyStatus == "SPLIT" && String.IsNullOrWhiteSpace(policyNumber))
                        continue;

                    if (String.IsNullOrWhiteSpace(oldContractNumber))
                        oldContractNumber = contractNumber;
                    if (String.IsNullOrWhiteSpace(oldPolicyNumber))
                        oldPolicyNumber = policyNumber;
                    if (contractNumber == oldContractNumber && policyNumber != oldPolicyNumber)
                    {
                        if (dt.Rows[i]["reducedPaidUpAmount"].ObjToDouble() > 0D)
                        {
                            if ( rpu > 0D )
                                rpu += dt.Rows[i]["reducedPaidUpAmount"].ObjToDouble();
                        }
                        else
                            rpu = 0D;
                        dt.Rows[i]["reducedPaidUpAmount"] = rpu;

                        amtActuallyReceived += dt.Rows[i]["beginningPaymentBalance"].ObjToDouble();
                        dt.Rows[i]["beginningPaymentBalance"] = amtActuallyReceived;

                        faceAmount += dt.Rows[i]["endingPaymentBalance"].ObjToDouble();
                        dt.Rows[i]["endingPaymentBalance"] = faceAmount;

                        endingDeathBenefit += dt.Rows[i]["endingDeathBenefit"].ObjToDouble();
                        dt.Rows[i]["endingDeathBenefit"] = endingDeathBenefit;

                        beginningDeathBenefit += dt.Rows[i]["beginningDeathBenefit"].ObjToDouble();
                        dt.Rows[i]["beginningDeathBenefit"] = beginningDeathBenefit;

                        deathClaimAmount += dt.Rows[i]["deathClaimAmount"].ObjToDouble();
                        dt.Rows[i]["deathClaimAmount"] = deathClaimAmount;

                        value += dt.Rows[i]["value"].ObjToDouble();
                        dt.Rows[i]["value"] = value;

                        principal += dt.Rows[i]["principal"].ObjToDouble();
                        dt.Rows[i]["principal"] = principal;

                        dt.Rows.RemoveAt(i + 1);
                        oldPolicyNumber = policyNumber;
                        continue;
                    }
                    rpu = dt.Rows[i]["reducedPaidUpAmount"].ObjToDouble();
                    amtActuallyReceived = dt.Rows[i]["beginningPaymentBalance"].ObjToDouble();
                    faceAmount = dt.Rows[i]["endingPaymentBalance"].ObjToDouble();
                    endingDeathBenefit = dt.Rows[i]["endingDeathBenefit"].ObjToDouble();
                    beginningDeathBenefit = dt.Rows[i]["beginningDeathBenefit"].ObjToDouble();
                    deathClaimAmount = dt.Rows[i]["deathClaimAmount"].ObjToDouble();
                    value = dt.Rows[i]["value"].ObjToDouble();
                    principal = dt.Rows[i]["principal"].ObjToDouble();

                    oldContractNumber = contractNumber;
                    oldPolicyNumber = policyNumber;
                }
                int nextDays = txtNextDays.Text.ObjToString().Trim().ObjToInt32();
                nextDays = workNextDays;
                date3 = date1;
                date3 = date3.AddMonths(1);
                if (nextDays > 0)
                    date3 = date3.AddDays(nextDays - 1);
                string sDate4 = date3.ToString("yyyy-MM-dd");

                cmd = "Select * FROM cust_payment_details c JOIN cust_payments b ON c.`contractNumber` = b.`contractNumber` WHERE c.`dateReceived` >= '" + sDate1 + "' AND c.`dateReceived` <= '" + sDate4 + " 23:59:59' AND c.`status` = 'Deposited' AND c.`type` = 'Trust' AND b.`type` = 'Trust' AND c.`paymentRecord` = b.`record` ";
                if (!String.IsNullOrWhiteSpace(companies))
                {
                    newCompany = companies;
                    if (newCompany.IndexOf("FDLIC PB") >= 0)
                        newCompany = newCompany.Replace("'FDLIC PB'", "'FDLIC PB','FDLIC CCI'");
                    cmd += " AND " + newCompany + " ";
                }
                cmd += " ORDER by `dateReceived`;  ";
                cmd = cmd.Replace("`trustCompany`", "`paidFrom`");

                ddx = G1.get_db_data(cmd);
                if (ddx.Rows.Count > 0)
                {
                    if (workReport == "Post 2002 Report - SN & FT")
                    {
                        for (int j = 0; j < ddx.Rows.Count; j++)
                        {
                            paidFrom = ddx.Rows[j]["trust_policy"].ObjToString();
                            if (String.IsNullOrWhiteSpace(paidFrom))
                                continue;
                            cmd = "SELECT * FROM `trust_data` WHERE `contractNumber` = '" + contract + "' order by `date` desc";
                        }
                    }
                    for (int j = 0; j < ddx.Rows.Count; j++)
                    {
                        firstName = "";
                        lastName = "";
                        dbr = 0D;
                        prePostWhat = "";
                        contract = ddx.Rows[j]["contractNumber"].ObjToString().ToUpper();
                        paidFrom = ddx.Rows[j]["trust_policy"].ObjToString().ToUpper();
                        if (!String.IsNullOrWhiteSpace(paidFrom))
                            contract = paidFrom;
                        if (contract == "M1591")
                        {
                        }
                        if (contract == "P15099UI")
                        {
                        }
                        if (contract == "B24006L")
                        {
                        }
                        if (contract == "FF21050LI")
                        {
                        }
                        if (workReport == "Post 2002 Report - FDLIC")
                        {
                            dbr = getDBR(contract);
                        }

                        if (workReport == "FDLIC")
                        {
                            //double beginningBalance = 0D;
                            //double endingBalance = 0D;
                            //double trust85Pending = 0D;
                            //string locind = "";
                            //FunPaymentDetails.CalcTrust2013(contract, ref endingBalance, ref trust85Pending, ref beginningBalance, ref locind);

                            //dbr = getDBR(contract);
                        }

                        dateFiled = ddx.Rows[j]["dateFiled"].ObjToDateTime();
                        dateReceived = ddx.Rows[j]["dateReceived"].ObjToDateTime();
                        amtActuallyReceived = ddx.Rows[j]["amtActuallyReceived"].ObjToDouble();
                        received = amtActuallyReceived;
                        trustAmtFiled = ddx.Rows[j]["trustAmtFiled"].ObjToDouble();
                        //if (received > 0D && trustAmtFiled == 0D)
                        //    trustAmtFiled = received;
                        if (trustAmtFiled < amtActuallyReceived && received > 0D)
                            amtActuallyReceived = trustAmtFiled;
                        growth = amtActuallyReceived - trustAmtFiled;
                        //if (trustAmtFiled > 0D && amtActuallyReceived > trustAmtFiled)
                        //    amtActuallyReceived = amtActuallyReceived - growth;
                        refund = FunPaymentDetails.getPossibleRefund(contract);

                        if (workReport.IndexOf("2002 Report - SN & FT") > 0)
                        {
                            dRows = dt.Select("contractNumber='" + contract + "' AND policyStatus <> 'SPLIT'");
                        }
                        else
                        {
                            dRows = dt.Select("contract='" + contract + "' AND beginningPaymentBalance = '" + amtActuallyReceived.ToString() + "' AND policyStatus <> 'SPLIT'");
                            if (dRows.Length <= 0)
                                dRows = dt.Select("contract='" + contract + "' AND value = '" + received.ToString() + "' AND policyStatus <> 'SPLIT'");
                            if (dRows.Length <= 0)
                                dRows = dt.Select("contract='" + contract + "' AND deathClaimAmount = '" + received.ToString() + "' AND policyStatus <> 'SPLIT'");
                            if (dRows.Length <= 0)
                                dRows = dt.Select("contract='" + contract + "' AND endingPaymentBalance = '" + amtActuallyReceived.ToString() + "' AND policyStatus <> 'SPLIT'");
                            if (dRows.Length <= 0)
                                dRows = dt.Select("contract='" + contract + "' AND policyStatus <> 'SPLIT'");
                        }
                        if (dRows.Length > 0)
                        {
                            DataTable ddddd = dRows.CopyToDataTable();
                            if (trustAmtFiled > 0D)
                            {
                                dRows[0]["dbr"] = dbr;
                                dValue = dRows[0]["fun_AmtFiled"].ObjToDouble();
                                dValue += trustAmtFiled;
                                dRows[0]["fun_AmtFiled"] = dValue;

                                dValue = dRows[0]["ourFiledAmount"].ObjToDouble();
                                dValue += trustAmtFiled;
                                dRows[0]["ourFiledAmount"] = dValue;

                                dValue = dRows[0]["value"].ObjToDouble(); // Just Fixed THis!
                                if (dRows[0]["first"].ObjToDouble() == 0D)
                                {
                                    dRows[0]["value"] = trustAmtFiled;
                                    dRows[0]["first"] = 1D;
                                }
                                else
                                {
                                    dValue += trustAmtFiled;
                                    dRows[0]["value"] = dValue;
                                }

                                funeral = dRows[0]["funeral"].ObjToString();
                                if (String.IsNullOrWhiteSpace(funeral))
                                {
                                    paymentRecord = ddx.Rows[j]["paymentRecord"].ObjToString();
                                    funeral = getFuneralFromPaymentRecord(contract, paymentRecord);
                                    dRows[0]["funeral"] = funeral;
                                }
                            }

                            if (dRows[0]["reducedPaidUpAmount"].ObjToDouble() > 0D)
                                dRows[0]["value"] = dRows[0]["reducedPaidUpAmount"].ObjToDouble();


                            if (received > 0D)
                            {
                                dValue = dRows[0]["fun_AmtReceived"].ObjToDouble();
                                dValue += received;
                                dRows[0]["fun_AmtReceived"] = dValue;
                            }

                            faceAmount = dRows[0]["endingPaymentBalance"].ObjToDouble();
                            endingDeathBenefit = dRows[0]["endingDeathBenefit"].ObjToDouble();
                            beginningDeathBenefit = dRows[0]["beginningDeathBenefit"].ObjToDouble();
                            beginningPaymentBalance = dRows[0]["beginningPaymentBalance"].ObjToDouble();
                            //if (beginningPaymentBalance > beginningDeathBenefit)
                            //    beginningDeathBenefit = beginningPaymentBalance;
                            deathClaimAmount = dRows[0]["deathClaimAmount"].ObjToDouble();

                            dRows[0]["u_faceAmount"] = faceAmount;
                            dRows[0]["u_endingDeathBenefit"] = endingDeathBenefit;
                            dRows[0]["u_beginningDeathBenefit"] = beginningDeathBenefit;
                            dRows[0]["u_deathClaimAmount"] = deathClaimAmount;
                            dRows[0]["fun_DateFiled"] = dateFiled;


                            value = dRows[0]["value"].ObjToDouble();
                            if (deathClaimAmount > amtActuallyReceived)
                                amtActuallyReceived = deathClaimAmount;

                            dddd = dRows.CopyToDataTable();
                            if (dRows.Length > 1)
                            {
                            }
                            funeral = dRows[0]["funeral"].ObjToString();
                            if (String.IsNullOrWhiteSpace(funeral))
                                dRows[0]["funeral"] = getFuneralService(contract);
                            dRows[0]["dateReceived"] = G1.DTtoMySQLDT(dateReceived.ToString("yyyy-MM-dd"));
                            dRows[0]["principal"] = amtActuallyReceived;

                            if (workReport == "Post 2002 Report - FDLIC")
                                beginningDeathBenefit = dRows[0]["totalPaidInAtClaim"].ObjToDouble();

                            dRows[0]["received"] = received;
                            //principal = dRows[0]["beginningDeathBenefit"].ObjToDouble();
                            //if (principal <= 0D)
                            //    principal = dRows[0]["endingPaymentBalance"].ObjToDouble();
                            ////if (principal > 0D)
                            ////    trustAmtFiled = principal;
                            //if (dRows[0]["reducedPaidUpAmount"].ObjToDouble() > 0D)
                            //    trustAmtFiled = dRows[0]["reducedPaidUpAmount"].ObjToDouble();
                            //dRows[0]["principal"] = trustAmtFiled;

                            if (contract == "N20002L")
                            {
                                principal = dRows[0]["totalPaidInAtClaim"].ObjToDouble();
                            }
                            // if (beginningDeathBenefit > 0D)
                            dRows[0]["principal"] = beginningDeathBenefit;
                            //if (trustAmtFiled > beginningDeathBenefit && beginningDeathBenefit > 0D)
                            //    dRows[0]["principal"] = trustAmtFiled;
                            //if (refund != 0D)
                            //{
                            //    dRows[0]["principal"] = trustAmtFiled;
                            //    if (trustAmtFiled < beginningDeathBenefit && beginningDeathBenefit > 0D)
                            //        dRows[0]["principal"] = beginningDeathBenefit;
                            //    dRows[0]["principal"] = beginningDeathBenefit;
                            //}
                            //dRows[0]["principal"] = trustAmtFiled;
                            if (beginningDeathBenefit == 0D)
                                dRows[0]["principal"] = deathClaimAmount;
                            //if (dRows[0]["reducedPaidUpAmount"].ObjToDouble() > 0D)
                            //    dRows[0]["principal"] = dRows[0]["reducedPaidUpAmount"].ObjToDouble();
                            else
                            {
                                overUnder = (received + refund) - trustAmtFiled;
                                dRows[0]["overunder"] = overUnder;
                            }
                            dRows[0]["refunds"] = refund;

                            if (refund > 0D)
                            {
                                //dRows[0]["principal"] = trustAmtFiled - refund;
                                dRows[0]["principal"] = beginningDeathBenefit;
                            }
                            rpu = dRows[0]["reducedPaidUpAmount"].ObjToDouble();
                            them = dRows[0]["value"].ObjToDouble();
                            us = dRows[0]["principal"].ObjToDouble();
                            trustOverUnder = received - us;
                            dRows[0]["trustOverUnder"] = trustOverUnder;
                            overUnder = them - us;
                            if (rpu > 0D)
                                overUnder = rpu - them;
                            dRows[0]["overunder"] = overUnder;
                            //dRows[0]["desc"] = insuredName;
                            insuredName = ddx.Rows[j]["names"].ObjToString();
                            if (String.IsNullOrWhiteSpace(insuredName))
                                insuredName = dRows[0]["firstName"].ObjToString() + " " + dRows[0]["lastName"].ObjToString();
                            dRows[0]["desc"] = insuredName;
                            double removal = getTBB(this.dateTimePicker2.Value, contract);
                            if (received < removal)
                            {
                                dRows[0]["desc"] = "* " + insuredName;
                            }
                            if (workReport == "Post 2002 Report - SN & FT")
                                dRows[0]["value"] = deathClaimAmount;

                        }
                        else // Got Funeral but nothing in Claims Paid
                        {
                            gotFuneral = false;
                            record = "";
                            cmd = "Select * from `trust_data` WHERE `contractNumber` = '" + contract + "';";
                            dddd = G1.get_db_data(cmd);
                            if (dddd.Rows.Count > 0)
                            {
                                if (workReport == "Pre 2002 Report")
                                {
                                    prePostWhat = dddd.Rows[0]["preOrPost"].ObjToString();
                                    if (prePostWhat.ToUpper() != "PRE" )
                                        continue;
                                }
                                int lastRow = dddd.Rows.Count - 1;
                                trustCompany = dddd.Rows[lastRow]["preOrPost"].ObjToString();
                                if (trustCompany.ToUpper() != preOrPost.ToUpper())
                                    continue;
                                middleName = dddd.Rows[lastRow]["middleName"].ObjToString().ToUpper();
                                firstName = dddd.Rows[lastRow]["firstName"].ObjToString().ToUpper();
                                lastName = dddd.Rows[lastRow]["lastName"].ObjToString().ToUpper();
                                policyNumber = dddd.Rows[lastRow]["policyNumber"].ObjToString();
                                trustCompany = dddd.Rows[lastRow]["trustCompany"].ObjToString().ToUpper();
                                record = dddd.Rows[lastRow]["record"].ObjToString();
                                date = dddd.Rows[lastRow]["reportDate"].ObjToDateTime();
                                if (date.Year > 1000 && date != workDate)
                                    continue;
                                date = dddd.Rows[lastRow]["date"].ObjToDateTime();
                                if (date.Year > 1000 && date != workDate)
                                    continue;
                                if (trustCompany == "UNITY DI")
                                    continue;
                                if (trustCompany != "UNITY" && trustCompany != "UNITY PB")
                                {
                                    if (trustCompany != "FDLIC" && trustCompany != "FDLIC PB")
                                    {
                                        if (trustCompany != "SECURITY NATIONAL" )
                                            continue;
                                    }
                                }
                                if ( String.IsNullOrWhiteSpace ( prePostWhat ))
                                {
                                    prePostWhat = ImportTrustFile.determinePrePostByYear(contract);
                                    if (workReport == "Pre 2002 Report" && preOrPost.ToUpper() != "PRE")
                                        continue;
                                }
                                if (trustCompany == "SECURITY NATIONAL")
                                {
                                }
                                else if (trustCompany == "FORETHOUGHT")
                                {
                                }
                                cmd = "Select * from `trust_data` WHERE `contractNumber` = '" + contract + "' AND `reportDate` > '1001-01-01';";
                                dddd = G1.get_db_data(cmd);
                                if (dddd.Rows.Count > 0)
                                {
                                    date = dddd.Rows[0]["reportDate"].ObjToDateTime();
                                    if (date != workDate)
                                        continue;
                                }
                                gotFuneral = true;

                                gotFuneral = false; // Added for some reason but is a problem here

                                //faceAmount = dRows[0]["endingPaymentBalance"].ObjToDouble();
                                //endingDeathBenefit = dRows[0]["endingDeathBenefit"].ObjToDouble();
                                //beginningDeathBenefit = dRows[0]["beginningDeathBenefit"].ObjToDouble();
                                //beginningPaymentBalance = dRows[0]["beginningPaymentBalance"].ObjToDouble();
                                //deathClaimAmount = dRows[0]["deathClaimAmount"].ObjToDouble();

                                //dRows[0]["u_faceAmount"] = faceAmount;
                                //dRows[0]["u_endingDeathBenefit"] = endingDeathBenefit;
                                //dRows[0]["u_beginningDeathBenefit"] = beginningDeathBenefit;
                                //dRows[0]["u_deathClaimAmount"] = deathClaimAmount;
                            }
                            else
                            {
                                prePostWhat = ImportTrustFile.determinePrePostByYear(contract);
                                if (workReport == "Pre 2002 Report" && prePostWhat.ToUpper() != "PRE")
                                    continue;
                            }
                            if ( CheckPreviouslyReported ( trustCompany, contract, received ) )
                            {
                                continue;
                            }
                            if ( gotFuneral )
                                continue;
                            dRow = dt.NewRow();
                            dRow["date"] = G1.DTtoMySQLDT(this.dateTimePicker2.Value.ObjToDateTime().ToString("MM/dd/yyyy"));
                            dRow["contractNumber"] = contract;
                            dRow["contract"] = contract;
                            dRow["middleName"] = middleName;
                            dRow["firstName"] = firstName;
                            dRow["lastName"] = lastName;
                            dRow["policyNumber"] = policyNumber;
                            dRow["fun_DateFiled"] = dateFiled;
                            dRow["fun_AmtFiled"] = trustAmtFiled;
                            dRow["fun_AmtReceived"] = received;
                            dRow["value"] = trustAmtFiled;
                            //dRow["received"] = received;
                            //dRow["principal"] = received;
                            dRow["dbr"] = dbr;
                            if (!String.IsNullOrWhiteSpace(record))
                                dRow["record"] = record.ObjToInt32();
                            funeral = getFuneralService(contract);
                            paidFrom = ddx.Rows[j]["paidFrom"].ObjToString();
                            if (String.IsNullOrWhiteSpace(funeral))
                            {
                                paymentRecord = ddx.Rows[j]["paymentRecord"].ObjToString();

                                funeral = getFuneralFromPaymentRecord(contract, paymentRecord);
                            }
                            dRow["funeral"] = funeral;
                            dRow["refunds"] = refund;
                            dRow["dateReceived"] = G1.DTtoMySQLDT(dateReceived.ObjToDateTime().ToString("yyyy-MM-dd"));
                            dRow["trustCompany"] = ddx.Rows[j]["trust_policy"].ObjToString();
                            dRow["trust"] = ddx.Rows[j]["trust_policy"].ObjToString();
                            dRow["trust"] = paidFrom;

                            insuredName = ddx.Rows[j]["names"].ObjToString();
                            if (String.IsNullOrWhiteSpace(insuredName))
                            {
                                cmd = "Select * from `fcustomers` WHERE `contractNumber` = '" + contract + "';";
                                dddd = G1.get_db_data(cmd);
                                if (dddd.Rows.Count > 0)
                                {
                                    dRow["desc"] = dddd.Rows[0]["firstName"].ObjToString() + " " + dddd.Rows[0]["lastName"].ObjToString();
                                }
                                else
                                {
                                    cmd = "Select * from `customers` WHERE `contractNumber` = '" + contract + "';";
                                    dddd = G1.get_db_data(cmd);
                                    if (dddd.Rows.Count > 0)
                                    {
                                        dRow["desc"] = dddd.Rows[0]["firstName"].ObjToString() + " " + dddd.Rows[0]["lastName"].ObjToString();
                                    }
                                }
                            }
                            else
                                dRow["desc"] = insuredName;
                            double removal = getTBB(this.dateTimePicker2.Value, contract);
                            if (received < removal)
                            {
                                insuredName = dRow["desc"].ObjToString();
                                dRow["desc"] = "* " + insuredName;
                            }
                            dt.Rows.Add(dRow);
                        }
                    }
                    dt = CheckForTransitions(dt, ddx);
                }

                //if (workReport == "Post 2002 Report - FDLIC")
                //{
                //    for (int i = 0; i < dt.Rows.Count; i++)
                //    {
                //        trustAmtFiled = dt.Rows[i]["fun_AmtFiled"].ObjToDouble();
                //        if (trustAmtFiled > 0)
                //            dt.Rows[i]["value"] = trustAmtFiled;
                //        if (dt.Rows[i]["reducedPaidUpAmount"].ObjToDouble() > 0D)
                //            dt.Rows[i]["value"] = dt.Rows[i]["reducedPaidUpAmount"].ObjToDouble();
                //    }
                //}
            }
            catch (Exception ex)
            {
            }

            //cmd = "Select * FROM `trust_data_edits` t WHERE t.`date` = '" + sDate2 + "' AND t.`preOrPost` = '" + preOrPost + "' AND `policyStatus` = 'SPLIT' ";
            //if (!String.IsNullOrWhiteSpace(companies))
            //{
            //    newCompany = companies;
            //    cmd += " AND " + newCompany + " ";
            //}
            //cmd += " ORDER by `date` desc;  ";
            //DataTable ddt = G1.get_db_data(cmd);

            //ddt.Columns.Add("manual");

            //DateTime sDate = DateTime.Now;
            //string str = "";
            //for (int i = 0; i < ddt.Rows.Count; i++)
            //    ddt.Rows[i]["manual"] = "Y";

            //if (ddt.Rows.Count > 0)
            //{
            //    dt.Merge(ddt);
            //    DataView tempview2 = dt.DefaultView;
            //    tempview2.Sort = "date desc";
            //    dt = tempview2.ToTable();
            //}

            //dt = TrustData.LookupTrusts(dt);

            if (workReport == "Post 2002 Report - SN & FT")
            {
                dRows = dt.Select("funeral<>''");
                {
                    //if (dRows.Length > 0)
                    //    dt = dRows.CopyToDataTable();
                }
            }

            dValue = 0D;
            dRows = dt.Select("policyStatus='SPLIT'");
            for ( int i=0; i<dRows.Length; i++)
            {
                str = dRows[i]["middleName"].ObjToString();
                if (G1.validate_date(str))
                    dRows[i]["dateReceived"] = G1.DTtoMySQLDT(str);
            }

            dt = AddMissing(dt, majorDt);

            if (workReport == "Post 2002 Report - FDLIC")
                ProcessDBRs(dt);

            if (workReport == "Pre 2002 Report")
            {
                //dt = PreBreakApart(dt);
                setUnityWebb(dt);
                setUnityBarham(dt);
                //setOldWebb(dt);
                //setOldCCI(dt);
            }

            dt = LoadMainLineEdits(dt);

            DataView tempview1 = dt.DefaultView;
            tempview1.Sort = "dateReceived asc";
            dt = tempview1.ToTable();

            //dt = LoadMainLineEdits(dt);

            newDt = dt.Copy();

            return dt;
        }
        /****************************************************************************************/
        private DataTable CheckForTransitions(DataTable dt, DataTable ddx)
        {
            string contract = "";
            string newContract = "";
            string paidFrom = "";
            string cmd = "";
            string paymentRecord = "";
            DataTable dd = null;
            DataRow[] dRows = null;
            DataTable ddd = null;
            int lastRow = 0;
            double amtActuallyReceived = 0D;
            double trustAmtFiled = 0D;
            double value = 0D;
            double principal = 0D;
            DateTime date = DateTime.Now;
            for (int j = 0; j < ddx.Rows.Count; j++)
            {
                try
                {
                    contract = ddx.Rows[j]["contractNumber"].ObjToString().ToUpper();
                    paidFrom = ddx.Rows[j]["trust_policy"].ObjToString().ToUpper();
                    if (!String.IsNullOrWhiteSpace(paidFrom))
                        contract = paidFrom;
                    //if (contract != "B23020LI")
                    //{
                    //    continue;
                    //}

                    paymentRecord = ddx.Rows[j]["paymentRecord"].ObjToString();

                    cmd = "Select * from `cust_payment_details` WHERE `paymentRecord` = '" + paymentRecord + "' AND `pmtInTransition` = '1';";
                    dd = G1.get_db_data(cmd);
                    if (dd.Rows.Count <= 0)
                        continue;
                    amtActuallyReceived = dd.Rows[0]["amtActuallyReceived"].ObjToDouble();
                    trustAmtFiled = dd.Rows[0]["trustAmtFiled"].ObjToDouble();
                    date = dd.Rows[0]["dateReceived"].ObjToDateTime();
                    dRows = dt.Select("contractNumber='" + contract + "'");
                    if (dRows.Length > 0)
                    {
                        ddd = dRows.CopyToDataTable();
                        dt.ImportRow(dRows[0]);
                        lastRow = dt.Rows.Count - 1;
                        dt.Rows[lastRow]["manual"] = "Y";
                        dt.Rows[lastRow]["status"] = "Main Line Edit";
                        dt.Rows[lastRow]["value"] = trustAmtFiled;
                        dt.Rows[lastRow]["trustOverUnder"] = 0D;

                        //dt.Rows[lastRow]["principal"] = trustAmtFiled;
                        dt.Rows[lastRow]["principal"] = 0D;
                        dt.Rows[lastRow]["received"] = amtActuallyReceived;
                        //dt.Rows[lastRow]["ourFiledAmount"] = trustAmtFiled;
                        dt.Rows[lastRow]["ourFiledAmount"] = 0D;
                        dt.Rows[lastRow]["fun_AmtReceived"] = amtActuallyReceived;
                        //dt.Rows[lastRow]["fun_AmtFiled"] = trustAmtFiled;
                        dt.Rows[lastRow]["fun_AmtFiled"] = 0D;
                        dt.Rows[lastRow]["dateReceived"] = G1.DTtoMySQLDT(date.ToString("yyyyMMdd"));
                        
                    }
                }
                catch ( Exception ex)
                {
                }
            }
            return dt;
        }
        /****************************************************************************************/
        private void ProcessDBRs ( DataTable dt )
        {
            string contractNumber = "";
            double value = 0D;
            double filedAmt = 0D;
            double dbr = 0D;
            double tbb = 0D;
            double filedAmount = 0D;
            string policyStatus = "";

            for ( int i=0; i<dt.Rows.Count; i++)
            {
                dbr = dt.Rows[i]["dbr"].ObjToDouble();
                if ( dbr > 0D )
                {
                    policyStatus = dt.Rows[i]["policyStatus"].ObjToString().ToUpper();
                    if (policyStatus == "SPLIT")
                        continue;
                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                    if ( contractNumber == "HU23043LI")
                    {
                    }
                    filedAmt = dt.Rows[i]["fun_AmtFiled"].ObjToDouble();
                    value = dt.Rows[i]["value"].ObjToDouble();
                    tbb = GetCummulativeTBB(contractNumber);
                    tbb = G1.RoundValue(tbb);
                    dt.Rows[i]["tbb"] = tbb;
                    if ( tbb > 0D )
                    {
                        //if (filedAmt >= tbb)
                        //{
                            //filedAmt = filedAmt - dbr;
                        filedAmt = tbb - dbr;
                        dt.Rows[i]["value"] = filedAmt;
                            dt.Rows[i]["tbb"] = tbb;
                        //}
                    }
                }
            }
        }
        /****************************************************************************************/
        public static double GetCummulativeTBB(string contractNumber)
        {
            double tbb = 0D;

            string cmd = "Select * from `trust2013r` WHERE `contractNumber` = '" + contractNumber + "';";
            DataTable dx = G1.get_db_data(cmd);

            for (int i = 0; i < dx.Rows.Count; i++)
            {
                tbb += dx.Rows[i]["paymentCurrMonth"].ObjToDouble();
            }
            return tbb;
        }
        /****************************************************************************************/
        private double getDBR(string contract)
        {
            double dbr = 0D;
            if ( contract == "M24027LI")
            {
            }
            string cmd = "SELECT * FROM `dbrs` WHERE `contractNumber` = '" + contract + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                for ( int i=0; i<dx.Rows.Count; i++)
                    dbr += dx.Rows[i]["dbr"].ObjToDouble();
            }
            //else
            //    dbr = PaymentsReport.isDBR(contract, this.dateTimePicker2.Value);
            return dbr;
        }
        /****************************************************************************************/
        private bool CheckPreviouslyReported ( string trustCompany, string contract, double payment )
        {
            bool rtn = false;
            string money = payment.ToString();
            string cmd = "Select * from `trust_data_edits` WHERE `trustName` = '" + trustCompany + "' AND `date` < '" + this.dateTimePicker1.Value.ToString("yyyy-MM-dd") + "' AND `contractNumber` = '" + contract + "' AND `payments` = '" + money + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
                rtn = true;
            return rtn;
        }
        /****************************************************************************************/
        private DataTable LoadMainLineEdits(DataTable dx)
        {
            DataTable dt = null;
            string cmd = "";
            string trust = "";

            DateTime date = this.dateTimePicker2.Value;
            string preOrPost = cmbPreOrPost.Text;

            string[] locIDs = workCompanies.EditValue.ToString().Split('|');
            for (int i = 0; i < locIDs.Length; i++)
            {
                trust = locIDs[i].Trim();
                cmd = "Select * from `trust_data_edits` WHERE `trustName` = '" + trust + "' AND `status` = 'Main Line Edit' AND `date` = '" + date.ToString("yyyy-MM-dd") + "' AND `preOrPost` = '" + preOrPost + "';";
                dt = G1.get_db_data(cmd);
                if (dt.Rows.Count > 0)
                    dx = LoadMainLineEdits(dx, dt);
            }
            return dx;
        }
        /****************************************************************************************/
        private string getFuneralFromPaymentRecord(string contract, string paymentRecord)
        {
            string funeral = "";
            if (!String.IsNullOrWhiteSpace(paymentRecord))
            {
                string cmd = "Select * from `cust_payments` c JOIN `fcust_extended` r ON c.`contractNumber` = r.`contractNumber` WHERE c.`record` = '" + paymentRecord + "';";
                DataTable ofun = G1.get_db_data(cmd);
                if (ofun.Rows.Count > 0)
                    funeral = ofun.Rows[0]["ServiceId"].ObjToString();
            }
            return funeral;
        }
        /****************************************************************************************/
        private DataTable PreBreakApart(DataTable dt)
        {
            DataTable table = workCompanies.Properties.DataSource as DataTable;
            DataTable newTable = table.Clone();
            int count = workCompanies.Properties.Items.Count;
            for (int i = 0; i < count; i++)
            {
                if (workCompanies.Properties.Items[i].CheckState == CheckState.Checked)
                {
                    newTable.Rows.Add(table.Rows[i].ItemArray);
                }
            }

            DataTable[] mainDts = new DataTable[7];

            int dtCount = 0;
            DataTable tempDt = null;

            bool gotSN = false;
            bool gotFT = false;
            bool gotCD = false;
            bool gotUnity = false;
            bool gotFDLIC = false;
            bool gotFDOldWebb = false;
            bool gotFDOldCCI = false;
            bool gotUnOldWebb = false;
            bool gotUnOldBar = false;
            bool gotUnityPB = false;
            bool gotFDLICPB = false;

            DataRow[] dRows = null;

            try
            {
                dRows = newTable.Select("trustCompany='Unity'");
                if (dRows.Length > 0)
                {
                    //gotUnity = LoadTrustCompany(dt, "unity", "Unity", workDt, workNextRow, ref mainDts, ref dtCount);
                    gotUnOldWebb = LoadTrustCompany(dt, "unityOldWebb", "Unity", workDt, workNextRow, ref mainDts, ref dtCount);
                    //gotUnOldBar = LoadTrustCompany(dt, "unityOldBarham", "Unity", workDt, workNextRow, ref mainDts, ref dtCount);
                }



                dRows = newTable.Select("trustCompany='Security National'");
                if (dRows.Length > 0)
                    gotSN = LoadTrustCompany(dt, "Security National", "S/N", workDt, workNextRow, ref mainDts, ref dtCount);

                dRows = newTable.Select("trustCompany='FORETHOUGHT'");
                if (dRows.Length > 0)
                    gotFT = LoadTrustCompany(dt, "Forethought", "F/T", workDt, workNextRow, ref mainDts, ref dtCount);

                dRows = newTable.Select("trustCompany='CD'");
                if (dRows.Length > 0)
                    gotCD = LoadTrustCompany(dt, "CD", "CD", workDt, workNextRow, ref mainDts, ref dtCount);

                dRows = newTable.Select("trustCompany='FDLIC'");
                if (dRows.Length > 0)
                {
                    gotFDLIC = LoadTrustCompany(dt, "fdlic", "FDLIC", workDt, workNextRow, ref mainDts, ref dtCount);
                    gotFDOldWebb = LoadTrustCompany(dt, "fdlicOldWebb", "FDLIC", workDt, workNextRow, ref mainDts, ref dtCount);
                    gotFDOldCCI = LoadTrustCompany(dt, "fdlicOldCCI", "FDLIC", workDt, workNextRow, ref mainDts, ref dtCount);
                }

                dRows = newTable.Select("trustCompany='Unity'");
                if (dRows.Length > 0)
                {
                    gotUnity = LoadTrustCompany(dt, "unity", "Unity", workDt, workNextRow, ref mainDts, ref dtCount);
                    gotUnOldWebb = LoadTrustCompany(dt, "unityOldWebb", "Unity", workDt, workNextRow, ref mainDts, ref dtCount);
                    gotUnOldBar = LoadTrustCompany(dt, "unityOldBarham", "Unity", workDt, workNextRow, ref mainDts, ref dtCount);
                }
                dRows = newTable.Select("trustCompany='Unity PB'");
                if (dRows.Length > 0)
                    gotUnityPB = LoadTrustCompany(dt, "unity pb", "Unity PB", workDt, workNextRow, ref mainDts, ref dtCount);

                dRows = newTable.Select("trustCompany='FDLIC PB'");
                if (dRows.Length > 0)
                    gotFDLICPB = LoadTrustCompany(dt, "fdlic pb", "FDLIC PB", workDt, workNextRow, ref mainDts, ref dtCount);
            }
            catch (Exception ex)
            {
            }

            string trust = "";
            string trustCompany = "";
            double dValue = 0D;
            double value = 0D;
            string sDate = "";

            DataTable newDt = null;

            int maxRow = 0;
            for (int j = 0; j < dtCount; j++)
            {
                tempDt = mainDts[j];
                if (tempDt == null)
                    continue;
                if (newDt == null)
                    newDt = tempDt.Clone();
                newDt.Merge(tempDt);
                maxRow += tempDt.Rows.Count;
                if (j < dtCount - 1)
                    maxRow = maxRow - 3;
            }

            return newDt;
        }
        /***********************************************************************************************/
        private DataTable policyTrustsDt = null;
        /***********************************************************************************************/
        private DataTable verifyContracts(DataTable dt, bool full = false )
        {
            if (dt == null)
                return dt;
            if (dt.Rows.Count <= 0)
                return dt;
            string policyNumber = "";
            string contractNumber = "";
            string company = "";
            string type = "";
            string record = "";

            policyTrustsDt = G1.get_db_data("Select * from `policyTrusts`");

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                    if (String.IsNullOrWhiteSpace(contractNumber) || contractNumber == "NULL")
                    {
                        policyNumber = dt.Rows[i]["policyNumber"].ObjToString();
                        if (policyNumber == "SM1533865")
                        {
                        }
                        company = dt.Rows[i]["trustCompany"].ObjToString();
                        contractNumber = FindContractNumber(policyNumber, company, ref type);
                        if (!String.IsNullOrWhiteSpace(contractNumber))
                        {
                            dt.Rows[i]["contractNumber"] = contractNumber;
                            record = dt.Rows[i]["record"].ObjToString();
                            if (!String.IsNullOrWhiteSpace(record))
                                G1.update_db_table("trust_data", "record", record, new string[] { "contractNumber", contractNumber });
                        }
                    }
                    else if ( full )
                    {
                        policyNumber = dt.Rows[i]["policyNumber"].ObjToString();
                        company = dt.Rows[i]["trustCompany"].ObjToString();
                        contractNumber = FindContractNumber(policyNumber, company, ref type);
                        if (!String.IsNullOrWhiteSpace(contractNumber))
                        {
                            dt.Rows[i]["contractNumber"] = contractNumber;
                            record = dt.Rows[i]["record"].ObjToString();
                            //if (!String.IsNullOrWhiteSpace(record))
                            //    G1.update_db_table("trust_data", "record", record, new string[] { "contractNumber", contractNumber });
                        }
                    }
                }
                catch (Exception ex)
                {
                }
            }
            return dt;
        }
        /***********************************************************************************************/
        private string FindContractNumber(string policyNumber, string Company, ref string type)
        {
            string contractNumber = "";
            if (policyNumber.ToUpper() == "SM1533862")
            {
            }
            DataTable dx = null;
            string cmd = "";
            if (policyTrustsDt == null)
                policyTrustsDt = G1.get_db_data("Select * from `policyTrusts`");
            if (policyTrustsDt.Rows.Count > 0)
            {
                cmd = "policyNumber='" + policyNumber + "'";
                if (!String.IsNullOrWhiteSpace(Company))
                    cmd = "policyNumber='" + policyNumber + "' AND Company = '" + Company + "'";
                DataRow[] dRows = policyTrustsDt.Select(cmd);
                if (dRows.Length <= 0 && Company == "FDLIC")
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
        /****************************************************************************************/
        private DataTable setTrustColumns(bool got, DataTable dx, string trust)
        {
            if (got)
            {
                dx.Columns.Add(trust + " desc");
                dx.Columns.Add(trust + " date");
                dx.Columns.Add(trust + " contract");
                dx.Columns.Add(trust + " funeral");

                gridMain.Columns[trust + " desc"].Visible = true;
                gridMain.Columns[trust + " date"].Visible = true;
                gridMain.Columns[trust + " contract"].Visible = true;
                gridMain.Columns[trust + " funeral"].Visible = true;
            }
            else
            {
                gridMain.Columns[trust + " desc"].Visible = false;
                gridMain.Columns[trust + " date"].Visible = false;
                gridMain.Columns[trust + " contract"].Visible = false;
                gridMain.Columns[trust + " funeral"].Visible = false;
            }
            return dx;
        }
        /****************************************************************************************/
        private bool LoadTrustCompany(DataTable dt, string trustCompany, string trust, DataTable workDt, int nextRow, ref DataTable[] mainDts, ref int dtCount)
        {
            DataTable tempDt = null;
            DataRow[] dRows = dt.Select("trustCompany='" + trustCompany + "'");
            if (dRows.Length <= 0)
                return false;
            DataTable ddd = dRows.CopyToDataTable();
            if (trustCompany.ToUpper() == "FDLIC")
                dRows = dt.Select("paidFrom='" + trustCompany + "'");

            ddd = dRows.CopyToDataTable();

            DataView tempview1 = ddd.DefaultView;
            tempview1.Sort = "paymentRecord asc, policyNumber asc";
            ddd = tempview1.ToTable();

            bool gotRecord1 = false;
            if (G1.get_column_number(ddd, "record1") >= 0)
            {
                gotRecord1 = true;

                //DataView tempview = ddd.DefaultView;
                //tempview.Sort = "record1 asc";
                //ddd = tempview.ToTable();
            }



            double beginningPaymentBalance = 0D;
            double endingPaymentBalance = 0D;
            double beginningDeathBenefit = 0D;
            double endingDeathBenefit = 0D;
            double money = 0D;


            string paymentRecord = "";
            string oldPaymentRecord = "";
            string policyNumber = "";
            string oldPolicyNumber = "";
            string contractNumber = "";
            string oldContractNumber = "";
            for (int i = (ddd.Rows.Count - 1); i >= 0; i--)
            {
                contractNumber = ddd.Rows[i]["contractNumber1"].ObjToString();
                if (contractNumber == "HU21001LI")
                {
                }
                paymentRecord = ddd.Rows[i]["paymentRecord"].ObjToString();
                policyNumber = ddd.Rows[i]["policyNumber"].ObjToString();
                //policyNumber = contractNumber;
                if (String.IsNullOrWhiteSpace(paymentRecord))
                    continue;
                if (paymentRecord == oldPaymentRecord)
                {
                    if (oldContractNumber == contractNumber)
                    {
                        if (oldPolicyNumber == policyNumber)
                        {
                            ddd.Rows.RemoveAt(i);
                        }
                    }
                }
                oldPaymentRecord = paymentRecord;
                oldPolicyNumber = policyNumber;
                oldContractNumber = contractNumber;
            }

            paymentRecord = "";
            oldPaymentRecord = "";
            policyNumber = "";
            oldPolicyNumber = "";

            for (int i = (ddd.Rows.Count - 1); i >= 0; i--)
            {
                if (gotRecord1)
                    paymentRecord = ddd.Rows[i]["record1"].ObjToString();
                else
                    paymentRecord = "1";
                policyNumber = ddd.Rows[i]["policyNumber"].ObjToString();
                if (String.IsNullOrWhiteSpace(paymentRecord))
                    continue;
                if (paymentRecord == oldPaymentRecord)
                {
                    if (oldPolicyNumber == policyNumber)
                    {
                        //beginningPaymentBalance = ddd.Rows[i]["beginningPaymentBalance"].ObjToDouble();
                        //endingPaymentBalance = ddd.Rows[i]["endingPaymentBalance"].ObjToDouble();
                        //beginningDeathBenefit = ddd.Rows[i]["beginningDeathBenefit"].ObjToDouble();
                        //endingDeathBenefit = ddd.Rows[i]["endingDeathBenefit"].ObjToDouble();

                        //money = ddd.Rows[i + 1]["beginningDeathBenefit"].ObjToDouble();
                        //money += beginningDeathBenefit;
                        //ddd.Rows[i + 1]["beginningDeathBenefit"] = money;

                        //money = ddd.Rows[i + 1]["endingDeathBenefit"].ObjToDouble();
                        //money += endingDeathBenefit;
                        //ddd.Rows[i + 1]["endingDeathBenefit"] = money;

                        //money = ddd.Rows[i + 1]["beginningPaymentBalance"].ObjToDouble();
                        //money += beginningPaymentBalance;
                        //ddd.Rows[i + 1]["beginningPaymentBalance"] = money;

                        //money = ddd.Rows[i + 1]["endingPaymentBalance"].ObjToDouble();
                        //money += endingPaymentBalance;
                        //ddd.Rows[i + 1]["endingPaymentBalance"] = money;

                        ddd.Rows.RemoveAt(i);
                    }
                    else
                    {
                        beginningPaymentBalance = ddd.Rows[i]["beginningPaymentBalance"].ObjToDouble();
                        endingPaymentBalance = ddd.Rows[i]["endingPaymentBalance"].ObjToDouble();
                        beginningDeathBenefit = ddd.Rows[i]["beginningDeathBenefit"].ObjToDouble();
                        endingDeathBenefit = ddd.Rows[i]["endingDeathBenefit"].ObjToDouble();

                        money = ddd.Rows[i + 1]["beginningDeathBenefit"].ObjToDouble();
                        money += beginningDeathBenefit;
                        ddd.Rows[i + 1]["beginningDeathBenefit"] = money;

                        money = ddd.Rows[i + 1]["endingDeathBenefit"].ObjToDouble();
                        money += endingDeathBenefit;
                        ddd.Rows[i + 1]["endingDeathBenefit"] = money;

                        money = ddd.Rows[i + 1]["beginningPaymentBalance"].ObjToDouble();
                        money += beginningPaymentBalance;
                        ddd.Rows[i + 1]["beginningPaymentBalance"] = money;

                        money = ddd.Rows[i + 1]["endingPaymentBalance"].ObjToDouble();
                        money += endingPaymentBalance;
                        ddd.Rows[i + 1]["endingPaymentBalance"] = money;
                        ddd.Rows.RemoveAt(i);
                    }
                }
                oldPaymentRecord = paymentRecord;
                oldPolicyNumber = policyNumber;
            }

            DataRow[] mRows = ddd.Select("position='Top'");
            DataTable dddd = ddd.Clone();
            if (mRows.Length > 0)
                dddd.Merge(mRows.CopyToDataTable());
            mRows = ddd.Select("position=''");
            if (mRows.Length > 0)
                dddd.Merge(mRows.CopyToDataTable());
            mRows = ddd.Select("position='In-Line'");
            if (mRows.Length > 0)
                dddd.Merge(mRows.CopyToDataTable());
            mRows = ddd.Select("position='Bottom'");
            if (mRows.Length > 0)
                dddd.Merge(mRows.CopyToDataTable());

            dRows = dddd.Select();
            ddd = dRows.CopyToDataTable();

            DataRow dRow = null;
            string preOrPost = "";
            DateTime date = DateTime.Now;
            endingDeathBenefit = 0D;
            endingPaymentBalance = 0D;
            double snTotalDeceased = 0D;
            double foreTotalDeceased = 0D;
            string lastName = "";
            string firstName = "";
            string insuredName = "";

            tempDt = CreateTempDt();
            tempDt.TableName = trustCompany;

            int firstRow = nextRow - 1;
            if (firstRow < 0)
                firstRow = 0;

            double firstValue = 0D;
            double nextValue = 0D;
            if (workDt != null)
            {
                if (workDt.Rows.Count > 0)
                {
                    firstValue = workDt.Rows[firstRow][trustCompany].ObjToDouble();
                    nextValue = workDt.Rows[nextRow][trustCompany].ObjToDouble();
                }
            }

            paymentRecord = "";
            oldPaymentRecord = "";
            int gotPaymentRecord = G1.get_column_number(dt, "paymentRecord");
            policyNumber = "";
            oldPolicyNumber = "";
            int lastRow = -1;

            double total = 0D;
            double other = 0D;
            string position = "";
            string manual = "";
            double dValue = 0D;
            double filedAmount = 0D;
            double receivedAmount = 0D;
            double amtActuallyReceived = 0D;
            double growth = 0D;
            string dateReceived = "";
            string record = "";
            for (int i = 0; i < dRows.Length; i++)
            {
                contractNumber = dRows[i]["contractNumber"].ObjToString();
                if (contractNumber == "HU21001LI")
                {
                }
                record = dRows[i]["record"].ObjToString();
                policyNumber = dRows[i]["policyNumber"].ObjToString();
                position = dRows[i]["position"].ObjToString().ToUpper();
                manual = dRows[i]["manual"].ObjToString();
                if (manual == "Y")
                    dRows[i]["dateReceived"] = G1.DTtoMySQLDT(dRows[i]["deathPaidDate"].ObjToDateTime().ToString("yyyy-MM-dd"));
                preOrPost = dRows[i]["preOrPost"].ObjToString();
                date = dRows[i]["deathPaidDate"].ObjToDateTime();
                firstName = dRows[i]["firstName"].ObjToString();
                lastName = dRows[i]["lastName"].ObjToString();
                if (!String.IsNullOrWhiteSpace(lastName))
                    insuredName = firstName + " " + lastName;
                else
                    insuredName = dRows[i]["insuredName"].ObjToString();

                if (manual == "Y")
                {
                    dateReceived = date.ToString("MM/dd/yyyy");
                }
                else
                {
                    filedAmount = dRows[i]["trustAmtFiled"].ObjToDouble();
                    amtActuallyReceived = dRows[i]["amtActuallyReceived"].ObjToDouble();
                    receivedAmount = amtActuallyReceived;
                    growth = dRows[i]["growth"].ObjToDouble();
                    growth = receivedAmount - filedAmount;
                    if (filedAmount > 0D && receivedAmount > filedAmount)
                        receivedAmount = receivedAmount - growth;
                    dateReceived = dRows[i]["dateReceived"].ObjToDateTime().ToString("MM/dd/yyyy");
                }
                //tempDt.Rows[i]["received"] = receivedAmount;
                //tempDt.Rows[i]["dateReceived"] = dRows[i]["dateReceived"].ObjToDateTime().ToString("MM/dd/yyyy");

                endingDeathBenefit = dRows[i]["endingDeathBenefit"].ObjToDouble();
                if (workOldStuff != "YES")
                {
                    if (trustCompany.ToUpper() == "FDLIC" || trustCompany.ToUpper() == "FDLIC PB")
                        endingDeathBenefit = dRows[i]["beginningPaymentBalance"].ObjToDouble();
                }
                contractNumber = dRows[i]["contractNumber"].ObjToString().ToUpper();
                if (contractNumber == "T05003")
                {

                }

                //if (trustCompany == "Security National" || trustCompany == "Forethought")
                //{
                //    if (preOrPost.ToUpper() != "POST")
                //        continue;
                //}
                if (workReport == "Post 2002 Report - Unity")
                {
                    date = dRows[i]["dateReceived"].ObjToDateTime();
                    endingDeathBenefit = dRows[i]["amtActuallyReceived"].ObjToDouble();
                    if (trustCompany == "unity")
                        endingDeathBenefit = dRows[i]["beginningDeathBenefit"].ObjToDouble();
                }
                if (workReport == "Post 2002 Report - FDLIC")
                {
                    endingDeathBenefit = dRows[i]["deathClaimAmount"].ObjToDouble();
                    endingDeathBenefit = dRows[i]["beginningPaymentBalance"].ObjToDouble();
                    if (endingDeathBenefit <= 0D)
                        endingDeathBenefit = dRows[i]["deathClaimAmount"].ObjToDouble();
                }

                if (position != "TOP" && position != "BOTTOM" && position != "IN-LINE")
                {
                    //if ( !String.IsNullOrWhiteSpace ( contractNumber))
                    //{
                    //    mRows = tempDt.Select("contract='" + contractNumber + "'");
                    //    if ( mRows.Length > 0 )
                    //    {

                    //        dValue = mRows[0]["value"].ObjToDouble();
                    //        dValue += endingDeathBenefit;
                    //        mRows[0]["value"] = dValue;

                    //        if (!String.IsNullOrWhiteSpace(contractNumber))
                    //            total += endingDeathBenefit;
                    //        else
                    //            other += endingDeathBenefit;
                    //        continue;
                    //    }
                    //}
                }

                if (workOldStuff == "YES")
                {
                    //if (trustCompany == "Security National" || trustCompany == "Forethought")
                    //{
                    //    if (preOrPost.ToUpper() != "PRE")
                    //        continue;
                    //}
                    dRow = tempDt.NewRow();
                    dRow["record"] = record;
                    dRow["trust"] = trust;
                    dRow["desc"] = insuredName;
                    dRow["lastName"] = lastName;
                    dRow["firstName"] = firstName;
                    dRow["date"] = date.ToString("yyyy-MM-dd");
                    dRow["contract"] = contractNumber;
                    dRow["value"] = endingDeathBenefit;
                    if (manual == "Y")
                        receivedAmount = endingDeathBenefit;
                    dRow["received"] = receivedAmount;
                    dRow["dateReceived"] = dateReceived;
                    dRow["policyNumber"] = policyNumber;
                    dRow["reportDate"] = workDate.ToString("yyyy-MM-dd");
                    dRow["manual"] = manual;
                    tempDt.Rows.Add(dRow);

                    if (!String.IsNullOrWhiteSpace(contractNumber))
                        total += endingDeathBenefit;
                    else
                        other += endingDeathBenefit;
                }
                else
                {
                    dRow = tempDt.NewRow();
                    dRow["record"] = record;
                    dRow["trust"] = trust;
                    dRow["desc"] = insuredName;
                    dRow["lastName"] = lastName;
                    dRow["firstName"] = firstName;

                    dRow["date"] = date.ToString("yyyy-MM-dd");
                    dRow["contract"] = contractNumber;
                    dRow["value"] = endingDeathBenefit;
                    if (manual == "Y")
                        dRow["received"] = endingDeathBenefit;
                    else
                    {
                        dRow["principal"] = receivedAmount;
                        dRow["received"] = amtActuallyReceived;
                        dRow["refunds"] = FunPaymentDetails.getPossibleRefund(contractNumber);
                    }
                    dRow["dateReceived"] = dateReceived;
                    dRow["policyNumber"] = policyNumber;
                    dRow["reportDate"] = workDate.ToString("yyyy-MM-dd");
                    dRow["manual"] = manual;

                    tempDt.Rows.Add(dRow);

                    if (!String.IsNullOrWhiteSpace(contractNumber))
                        total += endingDeathBenefit;
                    else
                        other += endingDeathBenefit;
                }
            }

            total = G1.RoundValue(total);
            other = G1.RoundValue(other);
            dValue = total + other;
            tempDt.DisplayExpression = dValue.ObjToString();

            //dRow = tempDt.NewRow();
            //tempDt.Rows.Add(dRow);

            //dRow = tempDt.NewRow();
            //dRow["desc"] = trust + " Deceased Total";
            //dRow["date"] = "";
            //dRow["contract"] = "";
            //dRow["value"] = total;
            //tempDt.Rows.Add(dRow);

            //dRow = tempDt.NewRow();
            //dRow["desc"] = trust + " Other Total";
            //dRow["date"] = "";
            //dRow["contract"] = "";
            //dRow["value"] = other;
            //tempDt.Rows.Add(dRow);

            //double diff = firstValue - total - other;

            //double newDiff = nextValue - diff;
            //newDiff = G1.RoundValue(newDiff);

            //dRow = tempDt.NewRow();
            //dRow["desc"] = trust + " Adjustment";
            //dRow["date"] = "";
            //dRow["contract"] = "";
            //dRow["value"] = newDiff;
            //tempDt.Rows.Add(dRow);


            mainDts[dtCount] = tempDt;
            dtCount++;
            return true;
        }
        /****************************************************************************************/
        private string getFuneralService(string contractNumber)
        {
            string serviceId = "";
            if (String.IsNullOrWhiteSpace(contractNumber))
                return serviceId;
            string cmd = "Select * from `fcust_extended` WHERE `contractNumber` = '" + contractNumber + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
                serviceId = dx.Rows[0]["serviceId"].ObjToString();
            else
            {
                cmd = "Select * from `customers` where `contractNumber` = '" + contractNumber + "';";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    serviceId = dx.Rows[0]["serviceId"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(serviceId))
                    {
                        cmd = "Select * from `fcust_extended` WHERE `serviceId` = '" + serviceId + "';";
                        dx = G1.get_db_data(cmd);
                        if ( dx.Rows.Count <= 0 )
                        {
                            if (serviceId.IndexOf("OS") < 0 && serviceId.IndexOf("O/S") < 0)
                                serviceId = "O/S " + serviceId;
                        }
                    }
                }
            }
            return serviceId;
        }
        /***********************************************************************************************/
        private void loadGroupCombo(System.Windows.Forms.ComboBox cmb, string key, string module)
        {
            string primaryName = "";
            cmb.Items.Clear();
            string cmd = "Select * from procfiles where ProcType = '" + key + "' AND `module` = '" + module + "' group by name;";
            cmd = "Select * from procfiles where ProcType = '" + key + "' group by name;";
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
        /***********************************************************************************************/
        void skinForm_SkinSelected(string s)
        {
            if (s.ToUpper().IndexOf("SKIN : ") >= 0)
            {
                string skin = s.Replace("Skin : ", "");
                if (skin.Trim().Length == 0)
                    skin = "Windows Default";
                if (skin == "Windows Default")
                {
                    this.LookAndFeel.SetSkinStyle(skin);
                    this.gridMain.Appearance.EvenRow.BackColor = System.Drawing.Color.LightGreen;
                    this.gridMain.Appearance.EvenRow.BackColor2 = System.Drawing.Color.LightGreen;
                    this.gridMain.Appearance.SelectedRow.BackColor = System.Drawing.Color.Yellow;
                    this.gridMain.Appearance.SelectedRow.ForeColor = System.Drawing.Color.Black;
                }
                else
                {
                    this.panelTop.BackColor = Color.Transparent;
                    this.menuStrip1.BackColor = Color.Transparent;
                    this.gridMain.PaintStyleName = "Skin";
                    DevExpress.Skins.SkinManager.EnableFormSkins();
                    this.LookAndFeel.UseDefaultLookAndFeel = true;
                    DevExpress.LookAndFeel.UserLookAndFeel.Default.SetSkinStyle(skin);
                    this.LookAndFeel.SetSkinStyle(skin);
                    this.dgv.LookAndFeel.SetSkinStyle(skin);
                    this.dgv.LookAndFeel.SkinName = skin;
                    gridMain.Appearance.EvenRow.Options.UseBackColor = false;
                    gridMain.Appearance.OddRow.Options.UseBackColor = false;
                    this.panelTop.Refresh();
                    OnSkinChange(skin);

                    //DevExpress.LookAndFeel.UserLookAndFeel.Default.SkinName = skin;
                    //this.LookAndFeel.SetSkinStyle(skin);
                    //this.dgv.LookAndFeel.SetSkinStyle(skin);
                }
            }
            else if (s.ToUpper().IndexOf("COLOR : ") >= 0)
            {
                string color = s.Replace("Color : ", "");
                this.gridMain.Appearance.EvenRow.BackColor = Color.FromName(color);
                this.gridMain.Appearance.EvenRow.BackColor2 = Color.FromName(color);
                this.gridMain.Appearance.SelectedRow.BackColor = System.Drawing.Color.Yellow;
                this.gridMain.Appearance.SelectedRow.ForeColor = System.Drawing.Color.Black;
                this.gridMain.Appearance.EvenRow.Options.UseBackColor = true;
                this.gridMain.Appearance.OddRow.Options.UseBackColor = true;
            }
            else if (s.ToUpper().IndexOf("NO COLOR ON") >= 0)
            {
                this.gridMain.Appearance.EvenRow.Options.UseBackColor = false;
                this.gridMain.Appearance.OddRow.Options.UseBackColor = false;
            }
            else if (s.ToUpper().IndexOf("NO COLOR OFF") >= 0)
            {
                this.gridMain.Appearance.EvenRow.Options.UseBackColor = true;
                this.gridMain.Appearance.OddRow.Options.UseBackColor = true;
            }
        }
        /***********************************************************************************************/
        private int pageMarginLeft = 0;
        private int pageMarginRight = 0;
        private int pageMarginTop = 0;
        private int pageMarginBottom = 0;
        /***********************************************************************************************/
        private string fullPath = "";
        private string format = "";
        private bool continuousPrint = false;
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

            if (dgv3.Visible)
                printableComponentLink1.Component = dgv3;
            else if (dgv4.Visible)
                printableComponentLink1.Component = dgv4;
            else if (dgv5.Visible)
                printableComponentLink1.Component = dgv5;
            else if (dgv7.Visible)
                printableComponentLink1.Component = dgv7;
            else if (dgv8.Visible)
                printableComponentLink1.Component = dgv8;
            else if (dgv9.Visible)
                printableComponentLink1.Component = dgv9;

            if (workReport != "Pre 2002 Report")
            {
                if (dgv2.Visible)
                    printableComponentLink1.Component = dgv2;
            }

            if (workReport == "Pre 2002 Report")
            {
                if (dgv6.Visible)
                    printableComponentLink1.Component = dgv6;
                else if (dgv10.Visible)
                    printableComponentLink1.Component = dgv10;
                else if (dgv11.Visible)
                    printableComponentLink1.Component = dgv11;
                else if (dgv12.Visible)
                    printableComponentLink1.Component = dgv12;
                else if (dgv13.Visible)
                    printableComponentLink1.Component = dgv13;
                else if (dgv14.Visible)
                    printableComponentLink1.Component = dgv14;
            }

            printableComponentLink1.PrintingSystemBase = printingSystem1;

            printableComponentLink1.EnablePageDialog = true;

            printableComponentLink1.CreateDetailHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateDetailHeaderArea);
            printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);
            printableComponentLink1.BeforeCreateAreas += new System.EventHandler(this.printableComponentLink1_BeforeCreateAreas);
            printableComponentLink1.AfterCreateAreas += new System.EventHandler(this.printableComponentLink1_AfterCreateAreas);
            printableComponentLink1.Landscape = true;

            Printer.setupPrinterMargins(10, 10, 80, 50);

            pageMarginLeft = Printer.pageMarginLeft;
            pageMarginRight = Printer.pageMarginRight;
            pageMarginTop = Printer.pageMarginTop;
            pageMarginBottom = Printer.pageMarginBottom;

            printableComponentLink1.Margins.Left = pageMarginLeft;
            printableComponentLink1.Margins.Right = pageMarginRight;
            printableComponentLink1.Margins.Top = pageMarginTop;
            printableComponentLink1.Margins.Bottom = pageMarginBottom;

            printingSystem1.Document.AutoFitToPagesWidth = 1;
            if ( continuousPrint && fullPath.ToUpper().IndexOf(".XLSX") > 0 )
            {
                printableComponentLink1.Landscape = true;
                printableComponentLink1.Margins.Bottom = 0;
                //printableComponentLink1.Margins.Top = 0;
                printableComponentLink1.Margins.Left = 0;
                printableComponentLink1.Margins.Right = 0;

                printingSystem1.Document.AutoFitToPagesWidth = 1; //Does not work
            }

            string leftColumn = "Pages: [Page # of Pages #]";
            string middleColumn = "User: [User Name]";
            string rightColumn = "Date: [Date Printed]";

            // Create a PageHeaderFooter object and initializing it with  
            // the link's PageHeaderFooter.  
            //PageHeaderFooter phf = printableComponentLink1.PageHeaderFooter as PageHeaderFooter;
            printableComponentLink1.PaperKind = System.Drawing.Printing.PaperKind.Legal;
            //printableComponentLink1.CreateReportHeaderArea += PrintableComponentLink1_CreateReportHeaderArea;
            //PageHeader phx = printableComponentLink1.PageHeaderFooter as PageHeaderFeder;

            // Clear the PageHeaderFooter's contents.  
            //phf.Header.Content.Clear();

            // Add custom information to the link's header.  
            //phf.Header.Content.AddRange(new string[] { leftColumn, middleColumn, rightColumn });
            //phf.Header.LineAlignment = BrickAlignment.Far;


            printableComponentLink1.CreateDocument();

            if (continuousPrint)
            {
                if (File.Exists(fullPath))
                    File.Delete(fullPath);
                if (fullPath.ToUpper().IndexOf(".PDF") > 0)
                    printableComponentLink1.ExportToPdf(fullPath);
                else if (fullPath.ToUpper().IndexOf(".XLSX") > 0)
                {
                    XlsxExportOptionsEx options = new XlsxExportOptionsEx();
                    options.ExportType = DevExpress.Export.ExportType.DataAware;
                    //options.ExportType = DevExpress.Export.ExportType.WYSIWYG;
                    options.CustomizeCell += opt_CustomizeCell;
                    options.FitToPrintedPageWidth = true;
                    options.ShowColumnHeaders = DefaultBoolean.True;
                    options.AllowBandHeaderCellMerge = DefaultBoolean.True;
                    options.ShowPageTitle = DefaultBoolean.True;
                    //options.CustomizeSheetHeader += Options_CustomizeSheetHeader;
                    //options.DocumentOptions.Title = "$A$1:$T$10";
                    options.RawDataMode = false;
                    options.TextExportMode = TextExportMode.Value;
                    options.ShowGridLines = true;
                    options.ShowBandHeaders = DefaultBoolean.True;

                    try
                    {
                        printableComponentLink1.ExportToXlsx(fullPath, options);
                    }
                    catch ( Exception ex)
                    {
                    }
                }
                else
                    printableComponentLink1.ExportToCsv(fullPath);
            }
            else
                printableComponentLink1.ShowPreview();


            //DataTable ddd = (DataTable)dgv.DataSource;

            //printableComponentLink1.ShowPreview();
        }
        void opt_CustomizeCell(DevExpress.Export.CustomizeCellEventArgs e)
        {
            ColorizeCell(e.Formatting);
            e.Handled = true;
        }

        private void PrintableComponentLink1_CreateReportHeaderArea(object sender, CreateAreaEventArgs e)
        {
            if (fullPath.ToUpper().IndexOf(".XLSX") > 0)
            {
                string text = "Some header text";
                float brickWidth = e.Graph.MeasureString(text).Width;

                RectangleF r = new RectangleF(e.Graph.ClientPageSize.Width / 2 - brickWidth, 0, brickWidth, e.Graph.Font.Height);

                TextBrick brick = new TextBrick() { TextValue = text, Text = text, Rect = r, ForeColor = Color.Black, HorzAlignment = DevExpress.Utils.HorzAlignment.Center, Sides = BorderSide.None };
                LineBrick br = new LineBrick() { Rect = new RectangleF(0, brick.Size.Height, e.Graph.ClientPageSize.Width, 2) };

                e.Graph.DrawBrick(brick);
                e.Graph.DrawBrick(br);
            }
        }
        /***********************************************************************************************/
        private void Options_CustomizeSheetHeader(DevExpress.Export.ContextEventArgs e)
        {
            // Create a new row.
            CellObject row = new CellObject();
            // Specify row values.
            row.Value = "The document is exported from the IssueList database.";
            // Specify row formatting.
            XlFormattingObject rowFormatting = new XlFormattingObject();
            rowFormatting.Font = new XlCellFont { Bold = true, Size = 14 };
            rowFormatting.Alignment = new DevExpress.Export.Xl.XlCellAlignment { HorizontalAlignment = DevExpress.Export.Xl.XlHorizontalAlignment.Center, VerticalAlignment = DevExpress.Export.Xl.XlVerticalAlignment.Top };
            row.Formatting = rowFormatting;
            // Add the created row to the output document.
            e.ExportContext.AddRow(new[] { row });
            // Add an empty row to the output document.
            e.ExportContext.AddRow();
            // Merge cells of two new rows. 
            e.ExportContext.MergeCells(new DevExpress.Export.Xl.XlCellRange(new DevExpress.Export.Xl.XlCellPosition(0, 0), new DevExpress.Export.Xl.XlCellPosition(5, 1)));
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
            if (dgv3.Visible)
                printableComponentLink1.Component = dgv3;
            else if (dgv4.Visible)
                printableComponentLink1.Component = dgv4;
            else if (dgv5.Visible)
                printableComponentLink1.Component = dgv5;
            else if (dgv7.Visible)
                printableComponentLink1.Component = dgv7;
            else if (dgv8.Visible)
                printableComponentLink1.Component = dgv8;
            else if (dgv9.Visible)
                printableComponentLink1.Component = dgv9;

            if (workReport != "Pre 2002 Report")
            {
                if (dgv2.Visible)
                    printableComponentLink1.Component = dgv2;
            }
            if (workReport == "Pre 2002 Report")
            {
                if (dgv6.Visible)
                    printableComponentLink1.Component = dgv6;
                else if (dgv10.Visible)
                    printableComponentLink1.Component = dgv10;
                else if (dgv11.Visible)
                    printableComponentLink1.Component = dgv11;
                else if (dgv12.Visible)
                    printableComponentLink1.Component = dgv12;
                else if (dgv13.Visible)
                    printableComponentLink1.Component = dgv13;
                else if (dgv14.Visible)
                    printableComponentLink1.Component = dgv14;
            }

            printableComponentLink1.PrintingSystemBase = printingSystem1;
            printableComponentLink1.CreateDetailHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateDetailHeaderArea);
            printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);
            printableComponentLink1.BeforeCreateAreas += new System.EventHandler(this.printableComponentLink1_BeforeCreateAreas);
            printableComponentLink1.AfterCreateAreas += new System.EventHandler(this.printableComponentLink1_AfterCreateAreas);
            printableComponentLink1.Landscape = true;

            Printer.setupPrinterMargins(10, 10, 80, 50);

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
            string title = "";
            Printer.setupPrinterQuads(e, 2, 3);
            Font font = new Font("Ariel", 16);
            if (!continuousPrint)
                Printer.DrawQuad(1, 1, Printer.xQuads, 2, "South Mississippi Funeral Services, LLC", Color.Black, BorderSide.Top, font, HorizontalAlignment.Center);
            else
            {
                Printer.SetQuadSize(12, 12);
                font = new Font("Ariel", 9);
                Printer.DrawQuad(5, 1, 4, 3, "South Mississippi Funeral Services, LLC", Color.Black, BorderSide.None, font, HorizontalAlignment.Center);

                font = new Font("Ariel", 8);
                Printer.DrawGridDate(2, 3, 2, 3, Color.Black, BorderSide.None, font);
                Printer.DrawGridPage(11, 3, 2, 3, Color.Black, BorderSide.None, font);

                title = this.Text;

                if (dgv9.Visible)
                    title = "Daily Trust Funeral Deposits";
                if (workReport == "Pre 2002 Report")
                {
                    if (dgv6.Visible)
                        title = "Pre-2002 SN/FT";
                    else if (dgv10.Visible)
                        title = "Pre-2002 CD";
                    else if (dgv11.Visible)
                        title = "Pre-2002 FDLIC Old Webb";
                    else if (dgv12.Visible)
                        title = "Pre-2002 FDLIC Old CCI";
                    else if (dgv13.Visible)
                        title = "Pre-2002 Unity Old Barham";
                    else if (dgv14.Visible)
                        title = "Pre-2002 Unity Old Webb";
                }
                Printer.DrawQuad(5, 6, 4, 3, title, Color.Black, BorderSide.None, font, HorizontalAlignment.Center);
                return;
            }

            Printer.SetQuadSize(12, 12);

            font = new Font("Ariel", 8);
            Printer.DrawGridDate(2, 3, 2, 3, Color.Black, BorderSide.None, font);
            Printer.DrawGridPage(11, 3, 2, 3, Color.Black, BorderSide.None, font);

            //            Printer.DrawQuad(1, 9, 2, 3, "User : " + LoginForm.username, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);

            font = new Font("Ariel", 10, FontStyle.Regular);
            title = this.Text;
            string location = "";
            string trusts = "";

            if (!String.IsNullOrWhiteSpace(location))
                title += " " + location;
            if (!String.IsNullOrWhiteSpace(trusts))
                title += " (" + trusts + ")";

            if (dgv9.Visible)
                title = "Daily Trust Funeral Deposits";
            if (workReport == "Pre 2002 Report")
            {
                if (dgv6.Visible)
                    title = "Pre-2002 SN/FT";
                else if (dgv10.Visible)
                    title = "Pre-2002 CD";
                else if (dgv11.Visible)
                    title = "Pre-2002 FDLIC Old Webb";
                else if (dgv12.Visible)
                    title = "Pre-2002 FDLIC Old CCI";
                else if (dgv13.Visible)
                    title = "Pre-2002 Unity Old Barham";
                else if (dgv14.Visible)
                    title = "Pre-2002 Unity Old Webb";
            }

            string user = LoginForm.username;
            string format = cmbSelectColumns.Text.Trim();
            //if (!String.IsNullOrWhiteSpace(format))
            //    user += " Format " + format;
            Printer.DrawQuad(5, 6, 5, 3, title, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            Printer.DrawQuad(1, 9, 4, 3, "User : " + user, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            if (!string.IsNullOrWhiteSpace(format))
                Printer.DrawQuad(3, 9, 4, 3, "Format : " + format, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);


            string workDate = workMonth;
            workDate = this.dateTimePicker2.Value.ToString("MMMMMMMM");
            workDate += ", " + this.dateTimePicker2.Value.Year.ToString();
            if ( dgv7.Visible )
            {
                workDate = this.dateTimePicker4.Value.ToString("MMMMMMMM");
                workDate += ", " + this.dateTimePicker4.Value.Year.ToString();
            }
            else if (dgv8.Visible)
            {
                workDate = this.dateTimePicker5.Value.ToString("MMMMMMMM");
                workDate += ", " + this.dateTimePicker5.Value.Year.ToString();
            }
            else if (dgv9.Visible)
            {
                workDate = this.dateTimePicker9.Value.ToString("MMMMMMMM");
                workDate += ", " + this.dateTimePicker9.Value.Year.ToString();
            }
            Printer.SetQuadSize(24, 12);
            font = new Font("Ariel", 9, FontStyle.Regular);
            Printer.DrawQuad(20, 7, 5, 5, "Month Closing - " + workDate, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            //Printer.DrawQuad(16, 8, 3, 4, lblPayment.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(19, 8, 3, 4, lblTrust85.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(22, 8, 3, 4, lblTrust100.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /****************************************************************************************/
        private int footerCount = 0;
        private void gridMain_BeforePrintRow(object sender, DevExpress.XtraGrid.Views.Printing.CancelPrintRowEventArgs e)
        {
            if (e.HasFooter)
            {
                footerCount++;
            }
        }
        /****************************************************************************************/
        private void gridMain_AfterPrintRow(object sender, DevExpress.XtraGrid.Views.Printing.PrintRowEventArgs e)
        {
            if (e.HasFooter)
            {
                footerCount++;
                if (footerCount >= 1)
                {
                    footerCount = 0;
                    e.PS.InsertPageBreak(e.Y);
                }
            }
        }
        /****************************************************************************************/
        private void gridMain_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {

        }
        /****************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        { // Spy Glass
            if (dgv.Visible)
                G1.SpyGlass(gridMain);
            else if (dgv2.Visible)
                G1.SpyGlass(gridMain2);
            else if (dgv4.Visible)
                G1.SpyGlass(gridMain4);
        }
        /****************************************************************************************/
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            //if (1 == 1)
            //    return;

            if (e == null)
                return;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            DataRow dr = gridMain.GetFocusedDataRow();
            if (dr == null)
                return;

            DataTable dt = (DataTable)dgv.DataSource;
            string record = dr["record"].ObjToString();
            if (record == "0")
                record = "";

            string policyStatus = dr["policyStatus"].ObjToString().ToUpper();
            if (policyStatus != "SPLIT")
                return;

            string column = gridMain.FocusedColumn.FieldName;

            DateTime date = this.dateTimePicker2.Value;

            string trustCompany = dr["trust"].ObjToString();
            double smfsBalance = dr["smfsBalance"].ObjToDouble();
            double balance = dr["value"].ObjToDouble();
            double received = dr["received"].ObjToDouble();
            double ftBalance = dr["principal"].ObjToDouble();

            trustCompany = "XYZZY";
            if (workReport == "Post 2002 Report - SN & FT")
                trustCompany = "SNFT";
            else if (workReport == "Post 2002 Report - Unity")
                trustCompany = "Unity";
            else if (workReport == "Post 2002 Report - FDLIC")
                trustCompany = "FDLIC";
            else if (workReport == "Post 2002 Report - CD")
                trustCompany = "CD";

            string preOrPost = "Post";
            preOrPost = cmbPreOrPost.Text.Trim();

            string month = dr["month"].ObjToString().ToUpper();
            if (String.IsNullOrWhiteSpace(record))
            {
                record = G1.create_record("trust_data_edits", "status", "-1");
                dr["record"] = record.ObjToInt32();
                G1.update_db_table("trust_data_edits", "record", record, new string[] { "status", "Line Edit", "preOrPost", preOrPost });
            }
            if (G1.BadRecord("trust_data_edits", record))
                return;
            string lastName = dr["desc"].ObjToString();
            string firstName = "";
            if (G1.get_column_number(dt, "otherdesc") > 0)
            {
                firstName = dr["otherdesc"].ObjToString();
            }
            //G1.update_db_table("trust_data_edits", "record", record, new string[] { "beginningPaymentBalance", balance.ToString(), "beginningDeathBenefit", smfsBalance.ToString(), "endingPaymentBalance", ftBalance.ToString(), "date", date.ToString("yyyy-MM-dd"), "trustName", trustCompany, "lastName", lastName, "firstName", firstName, "position", row.ToString() });
            G1.update_db_table("trust_data_edits", "record", record, new string[] { "beginningPaymentBalance", balance.ToString(), "endingPaymentBalance", smfsBalance.ToString(), "beginningDeathBenefit", ftBalance.ToString(), "currentUnappliedCash", received.ToString(), "date", date.ToString("yyyy-MM-dd"), "lastName", lastName, "firstName", firstName, "preOrPost", preOrPost, "position", row.ToString() });
            if (column.ToUpper() == "DATE")
            {
                string data = dr[column].ObjToDateTime().ToString("yyyy-MM-dd");
                G1.update_db_table("trust_data_edits", "record", record, new string[] { "deathPaidDate", data });
            }
            else if (column.ToUpper() == "CONTRACT")
            {
                string data = dr[column].ObjToString();
                G1.update_db_table("trust_data_edits", "record", record, new string[] { "contractNumber", data });
            }
            else if (column.ToUpper() == "FUNERAL")
            {
                string data = dr[column].ObjToString();
                G1.update_db_table("trust_data_edits", "record", record, new string[] { "statusReason", data });
            }
            else if (column.ToUpper() == "DATERECEIVED")
            {
                string data = dr[column].ObjToDateTime().ToString("yyyy-MM-dd");
                G1.update_db_table("trust_data_edits", "record", record, new string[] { "middleName", data });
            }
            else if (column.ToUpper() == "OTHERCONTRACT")
            {
                string data = dr[column].ObjToString();
                G1.update_db_table("trust_data_edits", "record", record, new string[] { "billingReason", data });
            }
            else if (column.ToUpper() == "OTHERFUNERAL")
            {
                string data = dr[column].ObjToString();
                G1.update_db_table("trust_data_edits", "record", record, new string[] { "policyStatus", data });
            }

            gridMain.RefreshEditor(true);
            gridMain.RefreshData();
            dgv.Refresh();
            gridMain.PostEditor();
        }
        /****************************************************************************************/
        private void endingDataChanged()
        {
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            DataRow dr = gridMain.GetFocusedDataRow();
            if (dr == null)
                return;

            string month = dr["month"].ObjToString().ToUpper();

            DataTable dt = (DataTable)dgv.DataSource;
            if (dgv2.Visible)
            {
                rowHandle = gridMain2.FocusedRowHandle;
                row = gridMain2.GetDataSourceRowIndex(rowHandle);
                dr = gridMain2.GetFocusedDataRow();
                if (dr == null)
                    return;

                month = dr["month"].ObjToString().ToUpper();

                dt = (DataTable)dgv2.DataSource;
            }

            string record = dr["record"].ObjToString();
            if (record == "0")
                record = "";
            //double balance = dr["balance"].ObjToDouble();
            double balance = dr["value"].ObjToDouble();
            double smfsBalance = dr["smfsBalance"].ObjToDouble();
            double ftBalance = dr["ftBalance"].ObjToDouble();
            string trustCompany = dr["trust"].ObjToString();
            if (workReport == "Post 2002 Report - SN & FT")
                trustCompany = "SNFT";
            else if (workReport == "Post 2002 Report - Unity")
                trustCompany = "Unity";
            else if (workReport == "Post 2002 Report - FDLIC")
                trustCompany = "FDLIC";
            else if (workReport == "Post 2002 Report - CD")
                trustCompany = "CD";

            DateTime date = this.dateTimePicker2.Value;

            if (month == "ENDING BALANCE")
            {
                //if (String.IsNullOrWhiteSpace(record))
                //{
                //    record = G1.create_record("trust_data_edits", "status", "-1");
                //    dr["record"] = record.ObjToInt32();
                //    G1.update_db_table("trust_data_edits", "record", record, new string[] { "status", "EndingBalance" });
                //}
                //if (G1.BadRecord("trust_data_edits", record))
                //    return;
                //G1.update_db_table("trust_data_edits", "record", record, new string[] { "status", "EndingBalance", "beginningPaymentBalance", balance.ToString(), "beginningDeathBenefit", smfsBalance.ToString(), "endingPaymentBalance", ftBalance.ToString(), "date", date.ToString("yyyy-MM-dd"), "trustName", trustCompany });
            }
            else
            {
                if (String.IsNullOrWhiteSpace(record))
                {
                    record = G1.create_record("trust_data_edits", "status", "-1");
                    dr["record"] = record.ObjToInt32();
                    G1.update_db_table("trust_data_edits", "record", record, new string[] {"status", "EndingManualAdjustment", "preOrPost", "Post" });
                }
                if (G1.BadRecord("trust_data_edits", record))
                    return;
                G1.update_db_table("trust_data_edits", "record", record, new string[] { "status", "EndingManualAdjustment", "beginningPaymentBalance", balance.ToString(), "beginningDeathBenefit", smfsBalance.ToString(), "endingPaymentBalance", ftBalance.ToString(), "date", date.ToString("yyyy-MM-dd"), "trustName", trustCompany });
            }

            //LoadEndingBalances(dt);
        }
        /****************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt == null)
                return;
            if (dt.Rows.Count <= 0)
                return;

            DataRow dr = gridMain.GetFocusedDataRow();

            string contractNumber = dr["contract"].ObjToString();
            string serviceId = dr["funeral"].ObjToString();
            if (dgv5.Visible)
            {
                dr = gridMain5.GetFocusedDataRow();
                contractNumber = dr["Trust"].ObjToString();
                serviceId = dr["Funeral Number"].ObjToString();
            }

            this.Cursor = Cursors.WaitCursor;

            SelectPayments dailyPayments = new SelectPayments(contractNumber, dr);
            dailyPayments.ModuleDone += DailyPayments_ModuleDone;
            dailyPayments.Show();

            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void DailyPayments_ModuleDone(DataTable modDt, DataRow dRow)
        {
            if (modDt.Rows.Count <= 0)
                return;

            DataTable dt = (DataTable)dgv.DataSource;
            DataTable dx = dt.Clone();
            dx.ImportRow(dRow);
            DataRow ddRow = null;
            DataRow[] dRows = null;
            string funeral = "";
            for (int i = 0; i < modDt.Rows.Count; i++)
            {
                ddRow = dt.NewRow();
                ddRow["desc"] = dRow["desc"].ObjToString();
                ddRow["trust"] = dRow["trust"].ObjToString();
                //ddRow["trustCompany"] = dRow["trustCompany"].ObjToString();
                ddRow["trustName"] = dRow["trust"].ObjToString();
                ddRow["value"] = modDt.Rows[i]["trust85P"].ObjToDouble();
                ddRow["date"] = G1.DTtoMySQLDT(dRow["date"].ObjToDateTime().ToString("MM/dd/yyyy"));
                ddRow["manual"] = "Y";
                ddRow["contract"] = dRow["contract"].ObjToString();
                ddRow["contractNumber"] = dRow["contract"].ObjToString();
                ddRow["firstName"] = dRow["firstName"].ObjToString();
                ddRow["lastName"] = dRow["lastName"].ObjToString();
                ddRow["middleName"] = dRow["middleName"].ObjToString();
                ddRow["insuredName"] = dRow["desc"].ObjToString();
                funeral = dRow["funeral"].ObjToString();
                ddRow["funeral"] = funeral;
                if ( !String.IsNullOrWhiteSpace ( funeral ))
                {
                    dRows = dx.Select("funeral='" + funeral + "'");
                    if ( dRows.Length > 0 )
                        for ( int j=0; j<dRows.Length; j++)
                        {
                            if ( dRows[j]["dateReceived"].ObjToDateTime().Year > 100 )
                            {
                                ddRow["dateReceived"] = G1.DTtoMySQLDT(dRows[j]["dateReceived"].ObjToDateTime().ToString("yyyy-MM-dd"));
                                break;
                            }
                        }
                }
                ddRow["preOrPost"] = cmbPreOrPost.Text;
                ddRow["status"] = "Main Line Edit";

                dt.Rows.Add(ddRow);
            }

            btnSaveMain();

            //btnSave.Show();
            //btnSave.Refresh();

            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            dgv.Refresh();
        }
        /****************************************************************************************/
        private void gridMain_CustomSummaryCalculate(object sender, DevExpress.Data.CustomSummaryEventArgs e)
        {
            //double balance = 0D;
            //DataTable dt = (DataTable)dgv.DataSource;
            //if (dt.Rows.Count > 0)
            //{
            //    int lastRow = dt.Rows.Count - 1;
            //    balance = dt.Rows[lastRow]["balance"].ObjToDouble();
            //}
            //string str = G1.ReformatMoney(balance);
            //str = str.Replace("$", "");
            //e.TotalValue = str;
        }
        /****************************************************************************************/
        private double originalSize = 0D;
        private Font mainFont = null;
        private Font newFont = null;
        private Font HeaderFont = null;
        /****************************************************************************************/
        private void ScaleCells()
        {
            //if (originalSize == 0D)
            //{
            //    //                originalSize = gridMain.Columns["address1"].AppearanceCell.FontSizeDelta.ObjToDouble();
            //    originalSize = gridMain.Columns["desc"].AppearanceCell.Font.Size;
            //    mainFont = gridMain.Columns["desc"].AppearanceCell.Font;
            //    HeaderFont = gridMain.Appearance.HeaderPanel.Font;
            //}
            //double scale = txtScale.Text.ObjToDouble();
            //double size = scale / 100D * originalSize;
            //Font font = new Font(mainFont.Name, (float)size);
            //gridMain.Appearance.GroupFooter.Font = font;
            //gridMain.AppearancePrint.FooterPanel.Font = font;
            //gridMain.AppearancePrint.HeaderPanel.Font = font;


            //gridMain.Appearance.GroupFooter.Font = font;
            //gridMain.Appearance.FooterPanel.Font = font;
            //gridMain.AppearancePrint.FooterPanel.Font = font;
            //gridMain.AppearancePrint.GroupFooter.Font = font;

            //font = new Font(HeaderFont.Name, (float)size, FontStyle.Regular);
            //for (int i = 0; i < gridMain.Columns.Count; i++)
            //{
            //    gridMain.Columns[i].AppearanceCell.Font = font;
            //    gridMain.Columns[i].AppearanceHeader.Font = font;
            //}

            //newFont = font;
            //gridMain.RefreshData();
            //gridMain.RefreshEditor(true);

            if ( dgv.Visible )
            {
                ResizeGrid(gridMain);
                dgv.Refresh();
            }
            else if (dgv2.Visible)
            {
                ResizeGrid(gridMain2);
                dgv2.Refresh();
            }
            this.Refresh();
        }
        /****************************************************************************************/
        private void ResizeGrid (BandedGridView gridMain )
        {
            if (originalSize == 0D)
            {
                //                originalSize = gridMain.Columns["address1"].AppearanceCell.FontSizeDelta.ObjToDouble();
                originalSize = gridMain.Columns["desc"].AppearanceCell.Font.Size;
                mainFont = gridMain.Columns["desc"].AppearanceCell.Font;
                HeaderFont = gridMain.Appearance.HeaderPanel.Font;
            }
            double scale = txtScale.Text.ObjToDouble();
            double size = scale / 100D * originalSize;
            Font font = new Font(mainFont.Name, (float)size);
            gridMain.Appearance.GroupFooter.Font = font;
            gridMain.AppearancePrint.FooterPanel.Font = font;
            gridMain.AppearancePrint.HeaderPanel.Font = font;


            gridMain.Appearance.GroupFooter.Font = font;
            gridMain.Appearance.FooterPanel.Font = font;
            gridMain.AppearancePrint.FooterPanel.Font = font;
            gridMain.AppearancePrint.GroupFooter.Font = font;

            font = new Font(HeaderFont.Name, (float)size, FontStyle.Regular);
            for (int i = 0; i < gridMain.Columns.Count; i++)
            {
                gridMain.Columns[i].AppearanceCell.Font = font;
                gridMain.Columns[i].AppearanceHeader.Font = font;
            }

            newFont = font;
            gridMain.RefreshData();
            gridMain.RefreshEditor(true);
        }
        /****************************************************************************************/
        private void txtScale_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string balance = txtScale.Text.Trim();
                if (!G1.validate_numeric(balance))
                {
                    MessageBox.Show("***ERROR*** Scale must be numeric!", "Scale Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
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
                MessageBox.Show("***ERROR*** Key entered must be a number!", "Data Entry Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                e.Handled = true;
            }
        }
        /****************************************************************************************/
        private void btnSelectColumns_Click(object sender, EventArgs e)
        {
            string actualName = cmbSelectColumns.Text;

            SelectColumns sform = new SelectColumns(dgv, "TrustDataEdit", "Primary", actualName);
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
        private void SetupSelectedColumns(string procType, string group, DevExpress.XtraGrid.GridControl dgv)
        {
            if (String.IsNullOrWhiteSpace(group))
                return;
            if (String.IsNullOrWhiteSpace(procType))
                procType = "TrustDataEdit";
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
        /***************************************************************************************/
        public delegate void d_void_eventdone_string(string s);
        public event d_void_eventdone_string SkinChange;
        protected void OnSkinChange(string done)
        {
            if (SkinChange != null)
                SkinChange.Invoke(done);
        }
        /****************************************************************************************/
        private void cmbSelectColumns_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            System.Windows.Forms.ComboBox combo = (System.Windows.Forms.ComboBox)sender;
            string comboName = combo.Text;
            string skinName = "";
            if (!String.IsNullOrWhiteSpace(comboName))
            {
                SetupSelectedColumns("TrustDataEdit", comboName, dgv);
                string name = "TrustDataEdit " + comboName;
                foundLocalPreference = G1.RestoreGridLayout(this, this.dgv, gridMain, LoginForm.username, name, ref skinName);
                SetupTotalsSummary();
                gridMain.OptionsView.ShowFooter = true;
            }
            else
            {
                SetupSelectedColumns("TrustDataEdit", "Primary", dgv);
                string name = "TrustDataEdit Primary";
                foundLocalPreference = G1.RestoreGridLayout(this, this.dgv, gridMain, LoginForm.username, name, ref skinName);
                SetupTotalsSummary();
                gridMain.OptionsView.ShowFooter = true;
            }
        }
        /****************************************************************************************/
        private void gridMain_ValidatingEditor(object sender, DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventArgs e)
        {
            GridView view = sender as GridView;
            DataRow dr = gridMain.GetFocusedDataRow();
            string month = dr["month"].ObjToString();
            if (month.ToUpper() != "BEGINNING BALANCE")
            {
                e.Valid = false;
                return;
            }
        }
        /****************************************************************************************/
        private void btnSave_Click(object sender, EventArgs e)
        {
            if (workReport == "Pre 2002 Report")
            {
                Save_Pre2002();
                SaveOverRuns();
                btnSave.Hide();
                btnSave.Refresh();
                return;
            }

            if (dgv.Visible)
            {
                btnSaveMain();
                return;
            }

            SaveOverRuns();

            DataTable dt = (DataTable)dgv2.DataSource;

            string data = "";
            string type = "";
            string field = "";
            string record = "";
            string modList = "";
            string month = "";
            string trustCompany = dt.TableName.Trim();
            if (workReport == "Post 2002 Report - SN & FT")
                trustCompany = "SNFT";
            else if (workReport == "Post 2002 Report - Unity")
                trustCompany = "Unity";
            else if (workReport == "Post 2002 Report - FDLIC")
                trustCompany = "FDLIC";
            else if (workReport == "Post 2002 Report - CD")
                trustCompany = "CD";

            double balance = 0D;
            double received = 0D;
            double smfsBalance = 0D;
            string manual = "";

            DataTable dx = null;
            DataRow[] dRows = null;
            DateTime date = this.dateTimePicker2.Value;
            string cmd = "";

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                manual = dt.Rows[i]["manual"].ObjToString();
                month = dt.Rows[i]["month"].ObjToString();
                if (trustCompany == "CD")
                {
                    if (i == 0)
                        month = "BEGINNING BALANCE";
                    else if (month.ToUpper() == "BALANCE")
                        month = "ENDING BALANCE";
                }
                if (month.ToUpper() == "BEGINNING BALANCE")
                {
                    record = dt.Rows[i]["record"].ObjToString();
                    balance = dt.Rows[i]["value"].ObjToDouble();
                    received = dt.Rows[i]["received"].ObjToDouble();
                    smfsBalance = dt.Rows[i]["smfsBalance"].ObjToDouble();
                    cmd = "Select * from `trust_data_edits` where `status` = 'BeginningBalance' AND `trustName` = '" + trustCompany + "' AND `date` = '" + date.ToString("yyyy-MM-dd") + "' AND `preOrPost` = 'Post' ;";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                        record = dx.Rows[0]["record"].ObjToString();
                    if (String.IsNullOrWhiteSpace(record))
                    {
                        record = G1.create_record("trust_data_edits", "status", "-1");
                        dt.Rows[i]["record"] = record.ObjToInt32();
                        G1.update_db_table("trust_data_edits", "record", record, new string[] {"status", "BeginningBalance", "preOrPost", "Post" });
                    }
                    if (G1.BadRecord("trust_data_edits", record))
                        return;
                    G1.update_db_table("trust_data_edits", "record", record, new string[] { "status", "BeginningBalance", "beginningPaymentBalance", balance.ToString(), "beginningDeathBenefit", received.ToString(), "endingPaymentBalance", smfsBalance.ToString(), "date", date.ToString("yyyy-MM-dd"), "trustName", trustCompany });

                    continue;
                }
                else if (month.ToUpper() == "ENDING BALANCE")
                {
                    record = dt.Rows[i]["record"].ObjToString();
                    balance = dt.Rows[i]["value"].ObjToDouble();
                    received = dt.Rows[i]["received"].ObjToDouble();
                    smfsBalance = dt.Rows[i]["smfsBalance"].ObjToDouble();
                    cmd = "Select * from `trust_data_edits` where `status` = 'EndingBalance' AND `trustName` = '" + trustCompany + "' AND `date` = '" + date.ToString("yyyy-MM-dd") + "' AND `preOrPost` = 'Post';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                        record = dx.Rows[0]["record"].ObjToString();
                    if (String.IsNullOrWhiteSpace(record))
                    {
                        record = G1.create_record("trust_data_edits", "status", "-1");
                        dt.Rows[i]["record"] = record.ObjToInt32();
                        G1.update_db_table("trust_data_edits", "record", record, new string[] { "status", "EndingBalance", "preOrPost", "Post" });
                    }
                    if (G1.BadRecord("trust_data_edits", record))
                        return;
                    G1.update_db_table("trust_data_edits", "record", record, new string[] { "status", "endingBalance", "beginningPaymentBalance", balance.ToString(), "beginningDeathBenefit", received.ToString(), "endingPaymentBalance", smfsBalance.ToString(), "date", date.ToString("yyyy-MM-dd"), "trustName", trustCompany });

                    continue;
                }
            }

            //btnSaveMain();

            btnSave.Hide();
            btnSave.Refresh();
        }
        /****************************************************************************************/
        private void SaveOverRuns ()
        {
            DataTable dt = (DataTable)dgv.DataSource;

            DateTime lastDate = this.dateTimePicker2.Value;

            DateTime startDate = this.dateTimePicker1.Value;
            DateTime stopDate = this.dateTimePicker2.Value;
            DateTime newStopDate = stopDate.AddDays(workNextDays);


            DateTime date = DateTime.Now;
            DateTime reportDate = DateTime.Now;
            string desc = "";
            string contract = "";
            string funeral = "";
            double value = 0D;
            double principal = 0D;
            double fun_amtReceived = 0D;
            string trustCompany = "";
            string cmd = "";
            DataTable dx = null;
            string record = "";
            string mainRecord = "";
            string preOrPost = cmbPreOrPost.Text;
            if (String.IsNullOrWhiteSpace(preOrPost))
                preOrPost = "Post";

            cmd = "Delete from `trust_data_overruns` where `desc` = '-1';";
            G1.get_db_data(cmd);

            cmd = "Select * from `trust_data_overruns` WHERE `date` >= '" + startDate.ToString("yyyy-MM-dd") + "' and `date` <= '" + newStopDate.ToString("yyyy-MM-dd") + "' AND `preOrPost` = '" + preOrPost + "'; ";
            DataTable overDt = G1.get_db_data(cmd);
            for ( int i=0; i<overDt.Rows.Count; i++)
            {
                record = overDt.Rows[i]["record"].ObjToString();
                date = overDt.Rows[i]["reportDate"].ObjToDateTime();
                if (date.Year > 1000)
                {
                    if (date >= startDate && date <= newStopDate)
                    {
                        G1.delete_db_table("trust_data_overruns", "record", record);
                    }
                }
                else
                {
                    date = overDt.Rows[i]["date"].ObjToDateTime();
                    if (date > lastDate)
                        G1.delete_db_table("trust_data_overruns", "record", record);
                }
            }

            for ( int i=0; i<dt.Rows.Count; i++)
            {
                date = dt.Rows[i]["dateReceived"].ObjToDateTime();
                if ( date > lastDate )
                {
                    desc = dt.Rows[i]["desc"].ObjToString();
                    desc = G1.try_protect_data(desc);
                    contract = dt.Rows[i]["contract"].ObjToString();
                    funeral = dt.Rows[i]["funeral"].ObjToString();
                    value = dt.Rows[i]["value"].ObjToDouble();
                    principal = dt.Rows[i]["principal"].ObjToDouble();
                    fun_amtReceived = dt.Rows[i]["fun_AmtReceived"].ObjToDouble();
                    trustCompany = dt.Rows[i]["trust"].ObjToString();
                    mainRecord = dt.Rows[i]["record"].ObjToString();
                    reportDate = dt.Rows[i]["reportDate"].ObjToDateTime();

                    cmd = "Select * from `trust_data_overruns` WHERE `date` = '" + date.ToString("yyyy-MM-dd") + "' and `desc` = '" + desc + "' AND `contract` = '" + contract + "' AND `preOrPost` = '" + preOrPost + "';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count <= 0)
                    {
                        record = G1.create_record("trust_data_overruns", "desc", "-1");
                        if (G1.BadRecord("trust_data_overruns", record))
                            return;
                    }
                    else
                        record = dx.Rows[0]["record"].ObjToString();
                    G1.update_db_table("trust_data_overruns", "record", record, new string[] { "desc", desc, "date", date.ToString("yyyy-MM-dd"), "contract", contract, "funeral", funeral, "trustCompany", trustCompany, "value", value.ToString(), "principal", principal.ToString(), "preOrPost", preOrPost, "fun_amtReceived", fun_amtReceived.ToString(), "reportDate", reportDate.ToString("yyyy-MM-dd"), "mainRecord", mainRecord});
                }
            }
        }
        /****************************************************************************************/
        private void btnSaveMain()
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (G1.get_column_number(dt, "manual") < 0)
                return;

            string data = "";
            string type = "";
            string field = "";
            string record = "";
            string modList = "";

            DataTable dx = null;
            DataRow[] dRows = dt.Select("status='DELETE' ");
            if (dRows.Length <= 0)
                dx = dt.Clone();
            else
                dx = dRows.CopyToDataTable();
            for (int i = 0; i < dRows.Length; i++)
            {
                record = dRows[i]["record"].ObjToString();
                if (record == "-1")
                    continue;
                if (!String.IsNullOrWhiteSpace(record))
                {
                    G1.delete_db_table("trust_data_edits", "record", record);
                    dRows[i]["record"] = -1;
                }
            }

            dRows = dt.Select("manual='Y' ");
            if (dRows.Length <= 0)
                return;
            dx = dRows.CopyToDataTable();

            string trustCompany = dt.TableName.Trim();
            if (workReport == "Post 2002 Report - SN & FT")
                trustCompany = "SNFT";
            else if (workReport == "Post 2002 Report - Unity")
                trustCompany = "Unity";
            else if (workReport == "Post 2002 Report - FDLIC")
                trustCompany = "FDLIC";
            else if (workReport == "Post 2002 Report - CD")
                trustCompany = "CD";

            DataTable testDt = G1.get_db_data("Select * from `trust_data_edits` WHERE `trustName` = 'XYZZYABC'");


            for (int i = 0; i < dRows.Length; i++)
            {
                record = dRows[i]["record"].ObjToString();
                if (record == "-1")
                    continue;
                if (String.IsNullOrWhiteSpace(record))
                {
                    record = G1.create_record("trust_data_edits", "status", "Main Line Edit");
                    dRows[i]["record"] = record.ObjToInt32();
                    G1.update_db_table("trust_data_edits", "record", record, new string[] { "status", "Main Line Edit", "preOrPost", "Post" });
                }
                if (G1.BadRecord("trust_data_edits", record))
                    return;

                modList = "";

                for (int j = 0; j < dx.Columns.Count; j++)
                {
                    field = dx.Columns[j].ColumnName;
                    if (field.ToUpper() == "NUM")
                        continue;
                    if (field.ToUpper() == "RECORD")
                        continue;
                    if (field.ToUpper() == "MANUAL")
                        continue;

                    data = dRows[i][j].ObjToString();
                    //if (field.ToUpper() == "TRUSTCOMPANY")
                    //{
                    //    dRows[i]["trustName"] = data;
                    //    continue;
                    //}
                    if (field.ToUpper() == "CONTRACTNUMBER")
                        data = dRows[i]["contract"].ObjToString();
                    else if (field.ToUpper() == "STATUS")
                        data = "Main Line Edit";
                    else if (field.ToUpper() == "PAYMENTS")
                        data = dRows[i]["value"].ToString();

                    if (G1.get_column_number(testDt, field) < 0)
                        continue;
                    if (G1.get_column_number(dt, field) >= 0)
                    {
                        try
                        {
                            type = dt.Columns[field].DataType.ToString().ToUpper();
                            if (data.IndexOf(",") >= 0)
                            {
                                G1.update_db_table("trust_data_edits", "record", record, new string[] { field, data });
                                continue;
                            }
                            if (String.IsNullOrWhiteSpace(data))
                                data = "NODATA";
                            modList += field + "," + data + ",";
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }

                modList = modList.TrimEnd(',');
                G1.update_db_table("trust_data_edits", "record", record, modList);
            }

            btnSave.Hide();
            btnSave.Refresh();
        }
        /****************************************************************************************/
        private void Save_Pre2002()
        {
            if ( dgv10.Visible )
            {
                Save_Pre2002CD();
                return;
            }
            DataTable dt = (DataTable)dgv6.DataSource;

            string data = "";
            string type = "";
            string field = "";
            string record = "";
            string modList = "";
            string month = "";
            string trustCompany = "Pre2002";

            double CD = 0D;
            double Forethought = 0D;
            double SecurityNational = 0D;
            double FdlicOldWebb = 0D;
            double FdlicOldCCI = 0D;
            double UnityOldBarham = 0D;
            double UnityOldWebb = 0D;


            DataTable dx = null;
            DataRow[] dRows = null;
            DateTime date = this.dateTimePicker2.Value;
            string cmd = "";
            string status = "";

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                month = dt.Rows[i]["month"].ObjToString();
                month = dt.Rows[i]["status"].ObjToString();

                if (month.ToUpper() == "BEGINNING BALANCE")
                {
                    record = dt.Rows[i]["record"].ObjToString();
                    CD = dt.Rows[i]["CD"].ObjToDouble();
                    Forethought = dt.Rows[i]["Forethought"].ObjToDouble();
                    SecurityNational = dt.Rows[i]["Security National"].ObjToDouble();
                    FdlicOldWebb = dt.Rows[i]["FDLIC Old Webb"].ObjToDouble();
                    FdlicOldCCI = dt.Rows[i]["FDLIC Old CCI"].ObjToDouble();
                    UnityOldBarham = dt.Rows[i]["Unity Old Barham"].ObjToDouble();
                    UnityOldWebb = dt.Rows[i]["Unity Old Webb"].ObjToDouble();
                    //cmd = "Select * from `trust_data_edits` where `status` = 'BeginningBalance' AND `trustName` = '" + trustCompany + "' AND `date` = '" + date.ToString("yyyy-MM-dd") + "' AND `preOrPost` = 'Pre';";
                    //dx = G1.get_db_data(cmd);
                    //if (dx.Rows.Count > 0)
                    //    record = dx.Rows[0]["record"].ObjToString();
                    //if (String.IsNullOrWhiteSpace(record))
                    //{
                    //    record = G1.create_record("trust_data_edits", "status", "-1");
                    //    dt.Rows[i]["record"] = record.ObjToInt32();
                    //    G1.update_db_table("trust_data_edits", "record", record, new string[] { "status", "BeginningBalance", "preOrPost", "Pre" });
                    //}
                    //if (G1.BadRecord("trust_data_edits", record))
                    //    return;
                    ////G1.update_db_table("trust_data_edits", "record", record, new string[] { "status", "BeginningBalance", "beginningPaymentBalance", Forethought.ToString(), "beginningDeathBenefit", SecurityNational.ToString(), "endingPaymentBalance", FdlicOldWebb.ToString(), "endingDeathBenefit", FdlicOldCCI.ToString(), "priorUnappliedCash", UnityOldBarham.ToString(), "currentUnappliedCash", UnityOldWebb.ToString(), "downPayments", CD.ToString(), "date", date.ToString("yyyy-MM-dd"), "trustName", trustCompany });
                    //G1.update_db_table("trust_data_edits", "record", record, new string[] { "status", "BeginningBalance", "beginningPaymentBalance", Forethought.ToString(), "beginningDeathBenefit", SecurityNational.ToString(), "endingPaymentBalance", FdlicOldWebb.ToString(), "endingDeathBenefit", FdlicOldCCI.ToString(), "priorUnappliedCash", UnityOldBarham.ToString(), "currentUnappliedCash", UnityOldWebb.ToString(), "date", date.ToString("yyyy-MM-dd"), "trustName", trustCompany });
                    //                    G1.update_db_table("trust_data_edits", "record", record, new string[] { "status", "BeginningBalance", "beginningPaymentBalance", balance.ToString(), "beginningDeathBenefit", received.ToString(), "endingPaymentBalance", smfsBalance.ToString(), "date", date.ToString("yyyy-MM-dd"), "trustName", trustCompany });

                    continue;
                }
                else if (month.ToUpper() == "ENDING BALANCE")
                {
                    record = dt.Rows[i]["record"].ObjToString();
                    Forethought = dt.Rows[i]["Forethought"].ObjToDouble();
                    SecurityNational = dt.Rows[i]["Security National"].ObjToDouble();
                    FdlicOldWebb = dt.Rows[i]["FDLIC Old Webb"].ObjToDouble();
                    FdlicOldCCI = dt.Rows[i]["FDLIC Old CCI"].ObjToDouble();
                    UnityOldBarham = dt.Rows[i]["Unity Old Barham"].ObjToDouble();
                    UnityOldWebb = dt.Rows[i]["Unity Old Webb"].ObjToDouble();
                    cmd = "Select * from `trust_data_edits` where `status` = 'EndingBalance' AND `trustName` = '" + trustCompany + "' AND `date` = '" + date.ToString("yyyy-MM-dd") + "' AND `preOrPost` = 'Pre';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                        record = dx.Rows[0]["record"].ObjToString();
                    if (String.IsNullOrWhiteSpace(record))
                    {
                        record = G1.create_record("trust_data_edits", "status", "-1");
                        dt.Rows[i]["record"] = record.ObjToInt32();
                        G1.update_db_table("trust_data_edits", "record", record, new string[] { "status", "EndingBalance", "preOrPost", "Pre" });
                    }
                    if (G1.BadRecord("trust_data_edits", record))
                        return;
                    //G1.update_db_table("trust_data_edits", "record", record, new string[] { "status", "EndingBalance", "beginningPaymentBalance", Forethought.ToString(), "beginningDeathBenefit", SecurityNational.ToString(), "endingPaymentBalance", FdlicOldWebb.ToString(), "endingDeathBenefit", FdlicOldCCI.ToString(), "priorUnappliedCash", UnityOldBarham.ToString(), "currentUnappliedCash", UnityOldWebb.ToString(), "downPayments", CD.ToString(), "date", date.ToString("yyyy-MM-dd"), "trustName", trustCompany });
                    G1.update_db_table("trust_data_edits", "record", record, new string[] { "status", "EndingBalance", "beginningPaymentBalance", Forethought.ToString(), "beginningDeathBenefit", SecurityNational.ToString(), "endingPaymentBalance", FdlicOldWebb.ToString(), "endingDeathBenefit", FdlicOldCCI.ToString(), "priorUnappliedCash", UnityOldBarham.ToString(), "currentUnappliedCash", UnityOldWebb.ToString(), "date", date.ToString("yyyy-MM-dd"), "trustName", trustCompany });

                    continue;
                }
            }

            btnSave.Hide();
            btnSave.Refresh();
        }
        /****************************************************************************************/
        private void Save_Pre2002CD()
        {
            DataTable dt = (DataTable)dgv10.DataSource;

            string data = "";
            string type = "";
            string field = "";
            string record = "";
            string modList = "";
            string month = "";
            string trustCompany = "Pre2002";

            double CD = 0D;
            double bankCD = 0D;

            DataTable dx = null;
            DataRow[] dRows = null;
            DateTime date = this.dateTimePicker2.Value;
            string cmd = "";
            string status = "";

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                month = dt.Rows[i]["month"].ObjToString();
                //month = dt.Rows[i]["status"].ObjToString();

                if (month.ToUpper() == "BEGINNING BALANCE")
                {
                    record = dt.Rows[i]["record"].ObjToString();
                    CD = dt.Rows[i]["CD"].ObjToDouble();
                    bankCD = dt.Rows[i]["bankCD"].ObjToDouble();

                    cmd = "Select * from `trust_data_edits` where `status` = 'BeginningBalance' AND `trustName` = '" + trustCompany + "' AND `date` = '" + date.ToString("yyyy-MM-dd") + "' AND `preOrPost` = 'Pre';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                        record = dx.Rows[0]["record"].ObjToString();
                    if (String.IsNullOrWhiteSpace(record))
                    {
                        record = G1.create_record("trust_data_edits", "status", "-1");
                        dt.Rows[i]["record"] = record.ObjToInt32();
                        G1.update_db_table("trust_data_edits", "record", record, new string[] { "status", "BeginningBalance", "preOrPost", "Pre" });
                    }
                    if (G1.BadRecord("trust_data_edits", record))
                        return;
                    G1.update_db_table("trust_data_edits", "record", record, new string[] { "status", "BeginningBalance", "downPayments", CD.ToString(), "growth", bankCD.ToString(), "date", date.ToString("yyyy-MM-dd"), "trustName", trustCompany });
                    continue;
                }
                else if (month.ToUpper() == "ENDING BALANCE")
                {
                    record = dt.Rows[i]["record"].ObjToString();
                    CD = dt.Rows[i]["CD"].ObjToDouble();
                    bankCD = dt.Rows[i]["bankCD"].ObjToDouble();

                    cmd = "Select * from `trust_data_edits` where `status` = 'EndingBalance' AND `trustName` = '" + trustCompany + "' AND `date` = '" + date.ToString("yyyy-MM-dd") + "' AND `preOrPost` = 'Pre';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                        record = dx.Rows[0]["record"].ObjToString();
                    if (String.IsNullOrWhiteSpace(record))
                    {
                        record = G1.create_record("trust_data_edits", "status", "-1");
                        dt.Rows[i]["record"] = record.ObjToInt32();
                        G1.update_db_table("trust_data_edits", "record", record, new string[] { "status", "EndingBalance", "preOrPost", "Pre" });
                    }
                    if (G1.BadRecord("trust_data_edits", record))
                        return;
                    G1.update_db_table("trust_data_edits", "record", record, new string[] { "status", "EndingBalance", "downPayments", CD.ToString(), "growth", bankCD.ToString(), "date", date.ToString("yyyy-MM-dd"), "trustName", trustCompany });
                    continue;
                }
            }

            btnSave.Hide();
            btnSave.Refresh();
        }
        /****************************************************************************************/
        private void gridMain_RowCellStyle(object sender, RowCellStyleEventArgs e)
        {
            GridView View = sender as GridView;
            try
            {
                if (e.RowHandle >= 0)
                {
                    string column = e.Column.FieldName.ToUpper();
                    if (column == "NUM")
                    {
                        DataTable dt = (DataTable)dgv.DataSource;
                        int row = gridMain.GetDataSourceRowIndex(e.RowHandle);

                        string manual = dt.Rows[row]["manual"].ObjToString();
                        if (manual.Trim().ToUpper() == "Y")
                            e.Appearance.BackColor = Color.Red;
                        else
                            e.Appearance.BackColor = Color.Transparent;
                    }
                    else if (column == "SANDRAMONEY")
                    {
                        DataTable dt = (DataTable)dgv.DataSource;
                        int row = gridMain.GetDataSourceRowIndex(e.RowHandle);
                        double sandraMoney = dt.Rows[row]["sandraMoney"].ObjToDouble();
                        sandraMoney = G1.RoundValue(sandraMoney);
                        double trustMoney = dt.Rows[row]["value"].ObjToDouble();
                        trustMoney = G1.RoundValue(trustMoney);
                        if (sandraMoney != trustMoney)
                            e.Appearance.BackColor = Color.Pink;
                        else
                            e.Appearance.BackColor = Color.Transparent;
                    }
                    else if (column == "SANDRAPRINCIPAL")
                    {
                        DataTable dt = (DataTable)dgv.DataSource;
                        int row = gridMain.GetDataSourceRowIndex(e.RowHandle);
                        string funeral = dt.Rows[row]["funeral"].ObjToString();
                        if (funeral.ToUpper().IndexOf("OS") >= 0 || funeral.ToUpper().IndexOf("O/S") >= 0)
                            return;

                        double sandraPrincipal = dt.Rows[row]["sandraPrincipal"].ObjToDouble();
                        sandraPrincipal = G1.RoundValue(sandraPrincipal);
                        double trustMoney = dt.Rows[row]["principal"].ObjToDouble();
                        trustMoney = G1.RoundValue(trustMoney);
                        if (trustMoney != sandraPrincipal)
                            e.Appearance.BackColor = Color.Pink;
                        else
                            e.Appearance.BackColor = Color.Transparent;
                    }
                    else if (column == "VALUE")
                    {
                        DataTable dt = (DataTable)dgv.DataSource;
                        int row = gridMain.GetDataSourceRowIndex(e.RowHandle);
                        double dbr = dt.Rows[row]["dbr"].ObjToDouble();
                        double tbb = dt.Rows[row]["tbb"].ObjToDouble();
                        double filedAmount = dt.Rows[row]["fun_AmtFiled"].ObjToDouble();
                        double value = dt.Rows[row]["value"].ObjToDouble();
                        if (dbr > 0D)
                        {
                            if (value > 0D && filedAmount > 0D)
                            {
                                //if (tbb >= filedAmount)
                                //{
                                e.Appearance.BackColor = Color.Yellow;
                                e.Appearance.ForeColor = Color.Black;
                                //}
                            }
                        }
                    }
                    else
                    {
                        DataTable dt = (DataTable)dgv.DataSource;

                        int row = gridMain.GetDataSourceRowIndex(e.RowHandle);
                        DateTime date = dt.Rows[row]["dateReceived"].ObjToDateTime();
                        if (date > this.dateTimePicker2.Value)
                            e.Appearance.ForeColor = Color.Red;
                    }
                }
            }
            catch ( Exception ex)
            {
            }
        }
        /****************************************************************************************/
        private void lockSceenFormatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string name = cmbSelectColumns.Text.Trim();
            if (String.IsNullOrWhiteSpace(name))
                name = "Primary";
            string saveName = "TrustDataEdit " + name;
            G1.SaveLocalPreferences(this, gridMain, LoginForm.username, saveName);
        }
        /****************************************************************************************/
        private void unlockScreenFormatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string comboName = cmbSelectColumns.Text;
            if (!String.IsNullOrWhiteSpace(comboName))
            {
                string name = "TrustDataEdit " + comboName;
                G1.RemoveLocalPreferences(LoginForm.username, name);
                //foundLocalPreference = false;
            }
        }
        /****************************************************************************************/
        private void btnShowSMFS_Click(object sender, EventArgs e)
        {
            LoadTBB(dateTimePicker2.Value);
        }
        /***********************************************************************************************/
        private double LoadTBB(DateTime date)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt == null)
                return 0D;
            if (dt.Rows.Count <= 0)
                return 0D;

            int days = DateTime.DaysInMonth(date.Year, date.Month);

            DateTime date1 = new DateTime(date.Year, date.Month, 1);
            DateTime date2 = new DateTime(date.Year, date.Month, days);

            string trust = "";
            bool first = true;
            bool found = false;
            string cmd = "Select * from `trust2013r` WHERE `payDate8` = '" + date2.ToString("yyyy-MM-dd") + "' and `currentRemovals` = '0' ";
            string[] locIDs = null;
            locIDs = workCompanies.EditValue.ToString().Split('|');
            if (locIDs.Length > 0)
            {
                cmd += " AND (";
                for (int i = 0; i < locIDs.Length; i++)
                {
                    trust = locIDs[i].Trim().ToUpper();
                    //if ( !String.IsNullOrWhiteSpace ( trust) && found && first )
                    //{
                    //    first = false;
                    //    cmd += " OR ";
                    //}
                    if (trust == "FDLIC")
                        cmd += " `contractNumber` LIKE '%L' OR `contractNumber` LIKE '%LI' ";
                    else if (trust == "FDLIC PB")
                        cmd += " `contractNumber` LIKE '%L' OR `contractNumber` LIKE '%LI' ";
                    else if (trust == "UNITY")
                        cmd += " `contractNumber` LIKE '%U' OR `contractNumber` LIKE '%UI' ";
                    found = true;
                }

                cmd += " ) ";
            }

            cmd += ";";
            DataTable dx = G1.get_db_data(cmd);

            double balance = 0D;
            double dValue = 0D;

            for (int i = 0; i < dx.Rows.Count; i++)
            {
                dValue = dx.Rows[i]["endingBalance"].ObjToDouble();
                balance += dValue;
            }

            this.Cursor = Cursors.Default;

            return balance;
        }
        /***********************************************************************************************/
        private double getTBB(DateTime date, string contractNumber)
        {
            string cmd = "Select * from `trust2013r` WHERE `payDate8` = '" + date.ToString("yyyy-MM-dd") + "' and `contractNumber` = '" + contractNumber + "' ";
            cmd += ";";

            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
                return 0D;

            double dValue = dx.Rows[0]["endingBalance"].ObjToDouble();
            if (dValue == 0D)
                dValue = dx.Rows[0]["currentRemovals"].ObjToDouble();

            //this.Cursor = Cursors.Default;

            return dValue;
        }
        /****************************************************************************************/
        private void gridMain_CustomColumnDisplayText_1(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            //int row = e.ListSourceRowIndex;
            if (e == null)
                return;
            if (e.ListSourceRowIndex == DevExpress.XtraGrid.GridControl.InvalidRowHandle)
                return;
            try
            {
                int row = e.ListSourceRowIndex;
                string month = "";
                if (String.IsNullOrWhiteSpace(e.DisplayText))
                    return;
                int rowHandle = gridMain.FocusedRowHandle;
                DataRow dR = gridMain.GetDataRow(e.ListSourceRowIndex);
                if (dR == null)
                    return;

                DataTable dt = (DataTable)dgv.DataSource;
                //if (e.Column.FieldName.ToUpper() == "SANDRAMONEY")
                //{
                //    if (G1.get_column_number(dt, "sandraMoney") >= 0)
                //    {
                //        double sandraMoney = dR["sandraMoney"].ObjToDouble();
                //        sandraMoney = G1.RoundValue(sandraMoney);
                //        double trustMoney = dR["value"].ObjToDouble();
                //        trustMoney = G1.RoundValue(trustMoney);
                //        //if (sandraMoney == 0D && trustMoney > 0D)
                //        //    e.Column.AppearanceCell.BackColor = Color.Pink;
                //    }
                //}
                //else if (e.Column.FieldName.ToUpper() == "SANDRAPRINCIPAL")
                //{
                //    if (G1.get_column_number(dt, "sandraPrincipal") >= 0)
                //    {
                //        //string contract = dR["contractNumber"].ObjToString();
                //        double sandraMoney = dR["sandraPrincipal"].ObjToDouble();
                //        sandraMoney = G1.RoundValue(sandraMoney);
                //        double trustMoney = dR["principal"].ObjToDouble();
                //        trustMoney = G1.RoundValue(trustMoney);
                //        //if (sandraMoney == 0D && trustMoney > 0D)
                //        //    e.Column.AppearanceCell.BackColor = Color.Pink;
                //    }
                //}

                //row = gridMain.GetDataSourceRowIndex(rowHandle);
                ColumnView view = sender as ColumnView;
                if (e.Column.DisplayFormat.FormatType == FormatType.Numeric)
                {
                    double value = e.DisplayText.ObjToDouble();
                    if (value > 0D)
                    {
                        e.DisplayText = G1.ReformatMoney(value);
                    }
                    else if (value < 0D)
                    {
                        value = Math.Abs(value);
                        e.DisplayText = "(" + G1.ReformatMoney(value) + ")";
                    }
                }

                if (e.Column.FieldName.ToUpper().IndexOf("DATE") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
                {
                    if (e.DisplayText.IndexOf("0000") >= 0 || e.DisplayText.IndexOf("0001") >= 0)
                        e.DisplayText = "";
                    else
                    {
                        DateTime date = e.DisplayText.ObjToString().ObjToDateTime();
                        e.DisplayText = date.ToString("MM/dd/yyyy");
                        if (date.Year < 1500)
                            e.DisplayText = "";
                    }
                }
                else if (e.DisplayText == "0.00" || e.DisplayText == "0")
                {
                    //DataRow dR = gridMain.GetDataRow(e.ListSourceRowIndex);
                    if (dR != null)
                    {
                        if (G1.get_column_number(gridMain, "month") >= 0)
                        {
                            string field = dR["month"].ObjToString().Trim().ToUpper();
                            if (field.IndexOf("ADJUST") < 0 && field.IndexOf("BALANCE") < 0)
                            {
                                if (e.DisplayText == "0.00" || e.DisplayText == "0")
                                    e.DisplayText = "";
                            }
                        }
                    }
                }
                else if (e.Column.FieldName.ToUpper() == "PRINCIPAL")
                {
                    e.Column.AppearanceCell.BackColor = Color.Yellow;
                }
                else if (e.Column.FieldName.ToUpper() == "VALUE")
                {
                    e.Column.AppearanceCell.BackColor = Color.Pink;
                }
                else if (e.Column.FieldName.ToUpper() == "OURFILEDAMOUNT")
                {
                    e.Column.AppearanceCell.BackColor = Color.LightGreen;
                }
                //else if (e.Column.FieldName.ToUpper() == "DESC")
                //{
                //    string middleName = dR["middleName"].ObjToString().Trim().ToUpper();
                //    if (middleName == "REPLACE")
                //    {
                //        Font font = e.Column.AppearanceCell.Font;
                //        float Size = e.Column.AppearanceCell.Font.Size;
                //        e.Column.AppearanceCell.Font = new Font(font.Name, Size, FontStyle.Italic);
                //    }
                //    else
                //    {
                //        Font font = e.Column.AppearanceCell.Font;
                //        float Size = e.Column.AppearanceCell.Font.Size;
                //        e.Column.AppearanceCell.Font = new Font(font.Name, Size, FontStyle.Regular);
                //    }
                //}
                //else if (e.Column.FieldName.ToUpper() == "NUM")
                //{
                //    string status = dR["status"].ObjToString().ToUpper();
                //    if (status == "LINE EDIT")
                //    {
                //    }
                //}
            }
            catch (Exception ex)
            {
            }
        }
        /****************************************************************************************/
        private void goToPaymentsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;

            int selectedIndex = tabControl1.SelectedIndex;
            string pageName = tabControl1.TabPages[selectedIndex].Name.Trim();

            if ( pageName.ToUpper() == "TABPAGE1")
                dt = (DataTable)dgv.DataSource;
           else if (pageName.ToUpper() == "TABPAGE2")
                dt = (DataTable)dgv2.DataSource;
            else if (pageName.ToUpper() == "TABPAGE5")
                dt = (DataTable)dgv5.DataSource;
            if (dt == null)
                return;
            if (dt.Rows.Count <= 0)
                return;

            DataRow dr = gridMain.GetFocusedDataRow();
            if (pageName.ToUpper() == "TABPAGE2")
                dr = gridMain2.GetFocusedDataRow();
            else if (pageName.ToUpper() == "TABPAGE5")
                dr = gridMain5.GetFocusedDataRow();

            string contractNumber = dr["contract"].ObjToString();
            string serviceId = dr["funeral"].ObjToString();
            if (pageName.ToUpper() == "TABPAGE5")
            {
                dr = gridMain5.GetFocusedDataRow();
                contractNumber = dr["Trust"].ObjToString();
                serviceId = dr["Funeral Number"].ObjToString();
            }

            if (!String.IsNullOrWhiteSpace(serviceId))
            {
                string cmd = "select * from fcust_extended where `serviceId` = '" + serviceId + "';";
                DataTable ddx = G1.get_db_data(cmd);
                if (ddx.Rows.Count > 0)
                    contractNumber = ddx.Rows[0]["contractNumber"].ObjToString();
            }

            this.Cursor = Cursors.WaitCursor;
            using (FunPayments editFunPayments = new FunPayments(null, contractNumber, "", false, false))
            {
                //editFunPayments.TopMost = true;
                editFunPayments.ShowDialog();
            }
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void goToDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (dgv2.Visible)
                dt = (DataTable)dgv2.DataSource;
            if (dt == null)
                return;
            if (dt.Rows.Count <= 0)
                return;

            DataRow dr = gridMain.GetFocusedDataRow();
            if (dgv2.Visible)
                dr = gridMain2.GetFocusedDataRow();

            string contractNumber = dr["contract"].ObjToString();
            string company = dr["trust"].ObjToString();

            DateTime date1 = this.dateTimePicker1.Value;
            DateTime date2 = this.dateTimePicker2.Value;

            DateTime date = dr["date"].ObjToDateTime();
            date2 = date;

            if (dgv5.Visible)
            {
                dr = gridMain5.GetFocusedDataRow();
                contractNumber = dr["Trust"].ObjToString();
                if (String.IsNullOrWhiteSpace(contractNumber))
                    return;
                DataRow[] dRows = dt.Select("contractNumber='" + contractNumber + "'");
                if (dRows.Length > 0)
                    company = dRows[0]["trustCompany"].ObjToString();
                else
                    company = "";
                date2 = this.dateTimePicker2.Value;
            }

            using (ImportTrustFile editImport = new ImportTrustFile(company, company, contractNumber, date2))
            {
                editImport.TopMost = true;
                editImport.ShowDialog();
            }
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void ZeroOutColumn(DataTable dt, string column)
        {
            if (G1.get_column_number(dt, column) < 0)
                return;
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i][column] = 0D;
        }
        /****************************************************************************************/
        private string importedFile = "";
        private string actualFile = "";
        private void btnVerifyTrusts_Click(object sender, EventArgs e)
        {
            if (workReport == "Pre 2002 Report")
            {
                verifyTrustsPre2002();
                return;
            }
            DataTable dt = (DataTable)dgv.DataSource;
            if (G1.get_column_number(dt, "sandraMoney") < 0)
                dt.Columns.Add("sandraMoney", Type.GetType("System.Double"));
            else
                ZeroOutColumn(dt, "sandraMoney");
            if (G1.get_column_number(dt, "sandraPrincipal") < 0)
                dt.Columns.Add("sandraPrincipal", Type.GetType("System.Double"));
            else
                ZeroOutColumn(dt, "sandraPrincipal");
            if (G1.get_column_number(dt, "difference") < 0)
                dt.Columns.Add("difference", Type.GetType("System.Double"));
            else
                ZeroOutColumn(dt, "difference");

            if (G1.get_column_number(dt, "pdiff") < 0)
                dt.Columns.Add("pdiff", Type.GetType("System.Double"));
            else
                ZeroOutColumn(dt, "pdiff");

            if (G1.get_column_number(dt, "done") < 0)
                dt.Columns.Add("done");

            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["done"] = "N";

            DataTable dddd = null;

            if (workReport.IndexOf("Post 2002 Report - SN") == 0)
            {
                gridMain.Columns["sandraMoney"].Caption = "Sandra SN Trust Money";
                gridMain.Columns["sandraPrincipal"].Caption = "Sandra FT Trust Money";
            }

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

                    this.Cursor = Cursors.WaitCursor;
                    DataTable workDt = null;
                    try
                    {
                        if (workReport == "Post 2002 Report - FDLIC")
                            workDt = ExcelWriter.ReadFile2(file, 0, "FDLIC");
                        else if (workReport == "Post 2002 Report - Unity")
                            workDt = ExcelWriter.ReadFile2(file, 0, "Unity");
                        else if (workReport.IndexOf("Post 2002 Report - SN") == 0)
                            workDt = ExcelWriter.ReadFile2(file, 2);
                        int workCount = 0;
                        DataTable tempDt = workDt.Clone();
                        DataTable tempDt2 = workDt.Clone();
                        string month = "";
                        //int iMonth = G1.ConvertMonthToIndex(workMonth);
                        //DateTime date = new DateTime(workYear.ObjToInt32(), iMonth, 1);
                        DateTime date = this.dateTimePicker1.Value;
                        workMonth = date.ToString("MMMMMMMMMM").ToUpper();
                        DateTime nextDate = date.AddMonths(1);
                        string nextMonth = nextDate.ToString("MMMMMMMMMM").ToUpper();
                        string c2 = "";
                        string c3 = "";
                        string c4 = "";
                        string c5 = "";
                        string c6 = "";
                        string c8 = "";
                        string c9 = "";
                        string c10 = "";
                        string c11 = "";
                        string c12 = "";
                        string c13 = "";

                        int firstRow = -1;
                        int lastRow = -1;

                        for (int i = 0; i < workDt.Rows.Count; i++)
                        {
                            month = workDt.Rows[i][0].ObjToString().Trim();
                            if (month.IndexOf(workMonth) == 0)
                            {
                                if (firstRow >= 0)
                                    continue;
                                firstRow = i;
                                continue;
                            }
                            else if (firstRow >= 0)
                            {
                                if (month.IndexOf(nextMonth) == 0)
                                {
                                    lastRow = i - 1;
                                    break;
                                }
                            }
                        }

                        if (lastRow < 0)
                            lastRow = workDt.Rows.Count;
                        for (int i = firstRow; i < lastRow; i++)
                        {
                            c2 = workDt.Rows[i]["Column3"].ObjToString();
                            c3 = workDt.Rows[i]["Column3"].ObjToString();
                            c4 = workDt.Rows[i]["Column4"].ObjToString();
                            c5 = workDt.Rows[i]["Column5"].ObjToString();
                            c6 = workDt.Rows[i]["Column6"].ObjToString();
                            c8 = workDt.Rows[i]["Column8"].ObjToString();
                            c9 = workDt.Rows[i]["Column9"].ObjToString();
                            c10 = workDt.Rows[i]["Column10"].ObjToString();
                            c11 = workDt.Rows[i]["Column11"].ObjToString();
                            c12 = workDt.Rows[i]["Column12"].ObjToString();
                            if (workReport.IndexOf("Post 2002 Report - SN") == 0)
                            {
                                c13 = workDt.Rows[i]["Column13"].ObjToString();
                                if (!String.IsNullOrWhiteSpace(c4) && String.IsNullOrWhiteSpace(c5))
                                {
                                    tempDt.ImportRow(workDt.Rows[i]);
                                }
                                if (!String.IsNullOrWhiteSpace(c11) && String.IsNullOrWhiteSpace(c12) && !String.IsNullOrWhiteSpace(c13))
                                {
                                    tempDt2.ImportRow(workDt.Rows[i]);
                                }
                            }
                            else if (!String.IsNullOrWhiteSpace(c4))
                            {
                                if (String.IsNullOrWhiteSpace(c2))
                                    tempDt.ImportRow(workDt.Rows[i]);
                                else if (!String.IsNullOrWhiteSpace(c3))
                                {
                                    if (c3.ToUpper().IndexOf("PAID") < 0 && c3.ToUpper().IndexOf("CASH") < 0 && c3.ToUpper().IndexOf ( "PB DC") < 0 )
                                        tempDt.ImportRow(workDt.Rows[i]);
                                }
                            }
                            if (workReport == "Post 2002 Report - Unity")
                            {
                                if (!String.IsNullOrWhiteSpace(c9) && !String.IsNullOrWhiteSpace(c10))
                                {
                                    tempDt2.ImportRow(workDt.Rows[i]);
                                }
                            }
                            else if (workReport == "Post 2002 Report - FDLIC")
                            {
                                if (!String.IsNullOrWhiteSpace(c11) && !String.IsNullOrWhiteSpace(c12))
                                {
                                    tempDt2.ImportRow(workDt.Rows[i]);
                                }
                            }
                        }

                        DataTable trustDt = new DataTable();
                        trustDt.Columns.Add("Trust");
                        trustDt.Columns.Add("Date Received");
                        trustDt.Columns.Add("Funeral Number");
                        trustDt.Columns.Add("Name");

                        trustDt.Columns.Add("Sandra Money", Type.GetType("System.Double"));
                        trustDt.Columns.Add("Trust Money", Type.GetType("System.Double"));
                        trustDt.Columns.Add("Difference", Type.GetType("System.Double"));

                        trustDt.Columns.Add("Sandra Principal", Type.GetType("System.Double"));
                        trustDt.Columns.Add("Trust Principal", Type.GetType("System.Double"));
                        trustDt.Columns.Add("Principal Diff", Type.GetType("System.Double"));

                        DataTable diffDt = trustDt.Clone();

                        DataRow dRow = null;
                        DataRow[] dRows = null;
                        DataRow[] dRows2 = null;

                        string contractNumber = "";
                        string funeral = "";
                        string name = "";
                        double dValue = 0D;
                        double sandraTotal = 0D;
                        double trustTotal = 0D;
                        double difference = 0D;
                        double pdiff = 0D;
                        double sandraMoney = 0D;
                        double sandraPrincipal = 0D;
                        double principal = 0D;
                        bool got15 = false;
                        if (G1.get_column_number(workDt, "column15") > 0)
                            got15 = true;

                        int startRow = 1;
                        startRow = 0;

                        for (int i = startRow; i < tempDt.Rows.Count; i++)
                        {
                            try
                            {
                                contractNumber = tempDt.Rows[i]["Column6"].ObjToString();
                                if (workReport.IndexOf("Post 2002 Report - SN") == 0)
                                    contractNumber = tempDt.Rows[i]["Column8"].ObjToString();
                                if (String.IsNullOrWhiteSpace(contractNumber))
                                    continue;
                                contractNumber = contractNumber.Replace(".", "");
                                contractNumber = contractNumber.Replace(",", "");
                                if (String.IsNullOrWhiteSpace(contractNumber))
                                    continue;

                                if (contractNumber == "B18015L")
                                {
                                }

                                name = tempDt.Rows[i]["Column4"].ObjToString();
                                if (workReport.IndexOf("Post 2002 Report - SN") == 0)
                                    name = tempDt.Rows[i]["Column6"].ObjToString();
                                if (String.IsNullOrWhiteSpace(name))
                                    continue;
                                sandraMoney = tempDt.Rows[i]["Column2"].ObjToDouble();
                                if (workReport.IndexOf("Post 2002 Report - SN") == 0)
                                    sandraMoney = tempDt.Rows[i]["Column4"].ObjToDouble();
                                sandraMoney = Math.Abs(sandraMoney);

                                funeral = tempDt.Rows[i]["Column7"].ObjToString();

                                dRow = trustDt.NewRow();
                                dRow["Trust"] = contractNumber;
                                dRow["Name"] = name;
                                dRow["Sandra Money"] = sandraMoney;

                                sandraTotal += sandraMoney;

                                dValue = 0D;

                                dRows = dt.Select("`contractNumber`='" + contractNumber + "' AND `done` = 'N' ");
                                if (dRows != null)
                                {
                                    if (dRows.Length > 0)
                                    {
                                        dddd = dRows.CopyToDataTable();
                                        for ( int k=0; k<dRows.Length; k++)
                                        {
                                            dValue = dRows[k]["value"].ObjToDouble();
                                            dValue = Math.Abs(dValue);
                                            if (dValue == 0D)
                                                continue;
                                            dRow["Trust Money"] = dValue;
                                            dRow["Funeral Number"] = dRows[0]["funeral"].ObjToString();
                                            dRow["Trust Principal"] = dRows[0]["principal"].ObjToDouble();
                                            //dRow["Date Received"] = dRows[0]["dateReceived"].ObjToDateTime().ToString("MM/dd/yyyy");
                                            trustTotal += dValue;
                                            //dRows[k]["Sandra Money"] = sandraMoney;
                                            dRows[k]["done"] = "Y";
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        dRow["Funeral Number"] = funeral;
                                        dRow["Trust Money"] = sandraMoney;
                                        dValue = sandraMoney;
                                    }
                                }
                                difference = sandraMoney - dValue;
                                difference = G1.RoundValue(difference);
                                dRow["Difference"] = difference;

                                trustDt.Rows.Add(dRow);
                            }
                            catch (Exception ex)
                            {
                            }
                        }

                        for (int i = 0; i < tempDt2.Rows.Count; i++)
                        {
                            try
                            {
                                funeral = "";
                                contractNumber = "";
                                if (workReport == "Post 2002 Report - Unity")
                                {
                                    if ( got15 )
                                        contractNumber = tempDt2.Rows[i]["Column15"].ObjToString();
                                    funeral = tempDt2.Rows[i]["Column11"].ObjToString();
                                    name = tempDt2.Rows[i]["Column9"].ObjToString().Trim();
                                    if (String.IsNullOrWhiteSpace(name))
                                        continue;
                                    sandraMoney = tempDt2.Rows[i]["Column10"].ObjToDouble();
                                    sandraMoney = Math.Abs(sandraMoney);
                                }
                                else if (workReport == "Post 2002 Report - FDLIC")
                                {
                                    funeral = tempDt2.Rows[i]["Column14"].ObjToString();
                                    name = tempDt2.Rows[i]["Column11"].ObjToString().Trim();
                                    if (String.IsNullOrWhiteSpace(name))
                                        continue;
                                    if (name == "SIDNEY LEAVITT JR ")
                                    {
                                    }
                                    sandraMoney = tempDt2.Rows[i]["Column12"].ObjToDouble();
                                    sandraMoney = Math.Abs(sandraMoney);
                                }
                                else if (workReport.IndexOf("Post 2002 Report - SN") == 0)
                                {
                                    if (got15)
                                        contractNumber = tempDt2.Rows[i]["Column15"].ObjToString().Trim();
                                    funeral = tempDt2.Rows[i]["Column16"].ObjToString();
                                    name = tempDt2.Rows[i]["Column13"].ObjToString();
                                    if (String.IsNullOrWhiteSpace(name))
                                        continue;
                                    sandraMoney = tempDt2.Rows[i]["Column11"].ObjToDouble();
                                    sandraMoney = Math.Abs(sandraMoney);
                                }
                                if (String.IsNullOrWhiteSpace(funeral) || funeral.ToUpper().IndexOf ( "O/S") == 0 || funeral.ToUpper().IndexOf ("OS") == 0 )
                                {
                                    if (String.IsNullOrWhiteSpace(name))
                                        continue;
                                    dRows = trustDt.Select("`Name`='" + name + "'");
                                }
                                else
                                {
                                    dRows = trustDt.Select("`Funeral Number`='" + funeral + "'");
                                    if (dRows.Length <= 0)
                                        dRows = trustDt.Select("`Name`='" + name + "'");
                                }
                                if (workReport.IndexOf("Post 2002 Report - SN") == 0)
                                {
                                    if ( dRows.Length == 0 )
                                    {
                                        dRow = trustDt.NewRow();
                                        dRow["Trust"] = contractNumber;
                                        dRow["Name"] = name;
                                        dRow["Sandra Principal"] = sandraMoney;
                                        dRow["Funeral Number"] = funeral;
                                        dRow["Trust Money"] = sandraMoney;
                                        trustDt.Rows.Add(dRow);
                                        dRows = trustDt.Select("`Name`='" + name + "'");
                                    }
                                }
                                
                                if (funeral == "MA24001")
                                {
                                }
                                if (dRows.Length > 0)
                                {
                                    dddd = dRows.CopyToDataTable();
                                    string name2 = "";
                                    for (int j = 0; j < dRows.Length; j++)
                                    {
                                        dValue = dRows[j]["Sandra Principal"].ObjToDouble();
                                        if (dValue != 0D)
                                            continue;
                                        if (dRows.Length > 1)
                                        {
                                            name2 = dRows[j]["Name"].ObjToString();
                                            if (name != name2)
                                                continue;
                                            if ( loadByValue ( dRows, sandraMoney, funeral ))
                                            {
                                                sandraTotal += sandraMoney;
                                                break;
                                            }
                                        }
                                        dRows[j]["Sandra Principal"] = sandraMoney;
                                        if (String.IsNullOrWhiteSpace(dRows[j]["Funeral Number"].ObjToString()))
                                            dRows[j]["Funeral Number"] = funeral;
                                        sandraTotal += sandraMoney;
                                        break;
                                    }
                                }
                                else
                                {
                                    if (name.ToUpper() == "OVERAGE")
                                        continue;
                                    if (name.ToUpper() == "SHORTAGE")
                                        continue;
                                    if (name.ToUpper().IndexOf("PMTS") > 0)
                                        continue;
                                    if (name.ToUpper().IndexOf("ADJ") > 0)
                                        continue;
                                    name += " (Missing) ";
                                    dRow = diffDt.NewRow();
                                    if (workReport.IndexOf("Post 2002 Report - SN") == 0)
                                    {
                                        name += tempDt2.Rows[i]["Column15"].ObjToString();
                                    }
                                    dRow["Name"] = name;
                                    dRow["Funeral Number"] = funeral;

                                    diffDt.Rows.Add(dRow);
                                    continue;
                                }

                                dValue = 0D;

                                if (funeral.ToUpper().IndexOf("OS") < 0 && funeral.ToUpper().IndexOf("O/S") < 0)
                                {
                                    dRows2 = dt.Select("funeral='" + funeral + "'");
                                    if (dRows2 != null)
                                    {
                                        if (dRows2.Length > 0)
                                        {
                                            dddd = dRows.CopyToDataTable();
                                            dValue = dRows2[0]["principal"].ObjToDouble();
                                            dValue = Math.Abs(dValue);
                                            //dRows[0]["Trust Principal"] = dValue;
                                            trustTotal += dValue;
                                            difference = dValue - sandraMoney;
                                            difference = G1.RoundValue(difference);
                                            dRows[0]["Principal Diff"] = difference;
                                        }
                                        else
                                        {
                                            //dRow["Trust Principal"] = 0D;
                                            //dValue = sandraMoney;
                                        }
                                    }
                                }
                                //difference = sandraMoney - dValue;
                                //difference = G1.RoundValue(difference);
                                //dRow["PrincipalDifference"] = difference;

                                //trustDt.Rows.Add(dRow);
                            }
                            catch (Exception ex)
                            {
                            }
                        }

                        for (int i = 0; i < dt.Rows.Count; i++)
                            dt.Rows[i]["done"] = "N";

                        dddd = null;

                        for (int i = 0; i < trustDt.Rows.Count; i++)
                        {
                            contractNumber = trustDt.Rows[i]["trust"].ObjToString();
                            if (String.IsNullOrWhiteSpace(contractNumber))
                                continue;
                            if (contractNumber == "F09005")
                            {
                            }
                            if (contractNumber == "B23020LI")
                            {
                            }
                            funeral = trustDt.Rows[i]["Funeral Number"].ObjToString();
                            sandraMoney = trustDt.Rows[i]["Sandra Money"].ObjToDouble();
                            sandraPrincipal = trustDt.Rows[i]["Sandra Principal"].ObjToDouble();
                            dRows = dt.Select("`contract`='" + contractNumber + "' AND `done` = 'N' ");
                            //dRows = dt.Select("contract='" + contractNumber + "'");
                            if ( dRows.Length <= 0 && !String.IsNullOrWhiteSpace ( funeral ))
                            {
                                if (funeral.ToUpper().IndexOf("OS") < 0 && funeral.ToUpper().IndexOf("O/S") < 0)
                                {
                                    dRows = dt.Select("`funeral`='" + funeral + "' AND `done` = 'N' ");
                                }
                            }
                            if (dRows.Length > 0)
                            {
                                dddd = dRows.CopyToDataTable();
                                if (String.IsNullOrWhiteSpace(funeral) || sandraMoney == 0D || sandraPrincipal == 0D)
                                    diffDt.ImportRow(trustDt.Rows[i]);

                                if (dRows.Length > 1)
                                {
                                    for (int j = 0; j < dRows.Length; j++)
                                    {
                                        dValue = dRows[j]["value"].ObjToDouble();
                                        if (dValue == 0D)
                                            continue;
                                        if (dRows[j]["sandraMoney"].ObjToDouble() == 0D)
                                        {
                                            dRows[j]["sandraMoney"] = sandraMoney;
                                            dRows[j]["sandraPrincipal"] = sandraPrincipal;
                                            dRows[j]["done"] = "Y";
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    dValue = dRows[0]["sandraMoney"].ObjToDouble();
                                    dValue += sandraMoney;
                                    dRows[0]["sandraMoney"] = dValue;

                                    dValue = dRows[0]["sandraPrincipal"].ObjToDouble();
                                    dValue += sandraPrincipal;
                                    dRows[0]["sandraPrincipal"] = dValue;
                                }
                            }
                            else
                            {
                                name = trustDt.Rows[i]["name"].ObjToString();
                                if (name.ToUpper() == "DESCRIPTION")
                                    continue;
                                diffDt.ImportRow(trustDt.Rows[i]);
                            }
                        }

                        for ( int i=0; i<dt.Rows.Count; i++)
                        {
                            dValue = dt.Rows[i]["value"].ObjToDouble();
                            sandraMoney = dt.Rows[i]["sandraMoney"].ObjToDouble();
                            dValue = dValue - sandraMoney;
                            if (dValue != 0D)
                                dt.Rows[i]["difference"] = dValue;
                            principal = dt.Rows[i]["principal"].ObjToDouble();
                            sandraPrincipal = dt.Rows[i]["sandraPrincipal"].ObjToDouble();
                            dValue = principal - sandraPrincipal;
                            if (dValue != 0D)
                                dt.Rows[i]["pdiff"] = dValue;
                        }

                        dgv.DataSource = dt;
                        dgv.Refresh();

                        dgv5.DataSource = diffDt;
                        dgv5.Refresh();

                        gridMain5.Columns["Sandra Money"].Visible = true;
                        gridMain5.Columns["Trust Money"].Visible = true;
                        gridMain5.Columns["Difference"].Visible = true;

                        gridMain.Columns["sandraMoney"].Visible = true;
                        gridMain.Columns["sandraPrincipal"].Visible = true;
                        gridMain.Columns["difference"].Visible = true;
                        gridMain.Columns["pdiff"].Visible = true;

                        gridMain.RefreshData();
                        gridMain.RefreshEditor(true);

                        this.Refresh();

                        this.Cursor = Cursors.Default;
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
        }
        /****************************************************************************************/
        private bool loadByValue ( DataRow [] dRows, double sandraMoney, string funeral )
        {
            bool rtn = false;
            //if (1 == 1)
            //    return rtn;

            DataTable dddd = dRows.CopyToDataTable();

            double dValue = 0D;
            for ( int i=0; i<dRows.Length; i++)
            {
                dValue = dRows[i]["Sandra Money"].ObjToDouble();
                dValue = dRows[i]["Trust Principal"].ObjToDouble();
                if (dValue == sandraMoney)
                {
                    dRows[i]["Sandra Principal"] = sandraMoney;
                    if (String.IsNullOrWhiteSpace(dRows[i]["Funeral Number"].ObjToString()))
                        dRows[i]["Funeral Number"] = funeral;
                    rtn = true;
                    break;
                }
            }
            return rtn;
        }
        /****************************************************************************************/
        private void verifyTrustsPre2002()
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (G1.get_column_number(dt, "sandraMoney") < 0)
                dt.Columns.Add("sandraMoney", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "sandraPrincipal") < 0)
                dt.Columns.Add("sandraPrincipal", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "difference") < 0)
                dt.Columns.Add("sandraPrincipal", Type.GetType("System.Double"));

            ZeroOutColumn(dt, "sandraMoney");
            ZeroOutColumn(dt, "sandraPrincipal");
            ZeroOutColumn(dt, "difference");


            if (workReport.IndexOf("Post 2002 Report - SN") == 0)
            {
                gridMain.Columns["sandraMoney"].Caption = "Sandra SN Trust Money";
                gridMain.Columns["sandraPrincipal"].Caption = "Sandra FT Trust Money";
            }

            DataTable foreDt = new DataTable();
            foreDt.Columns.Add("money", Type.GetType("System.Double"));
            foreDt.Columns.Add("name");
            foreDt.Columns.Add("contract");
            foreDt.Columns.Add("funeral");
            foreDt.Columns.Add("date");

            DataTable secDt = new DataTable();
            secDt.Columns.Add("money", Type.GetType("System.Double"));
            secDt.Columns.Add("name");
            secDt.Columns.Add("contract");
            secDt.Columns.Add("funeral");
            secDt.Columns.Add("date");

            DataTable unityWebbDt = new DataTable();
            unityWebbDt.Columns.Add("money", Type.GetType("System.Double"));
            unityWebbDt.Columns.Add("name");
            unityWebbDt.Columns.Add("contract");
            unityWebbDt.Columns.Add("funeral");
            unityWebbDt.Columns.Add("date");

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

                    this.Cursor = Cursors.WaitCursor;
                    DataTable workDt = null;
                    try
                    {
                        workDt = ExcelWriter.ReadFile2(file, 0, "FORETHOUGHT AND SECURITY NATION");
                        int workCount = 0;
                        DataTable tempDt = workDt.Clone();
                        DataTable tempDt2 = workDt.Clone();
                        string month = "";
                        //int iMonth = G1.ConvertMonthToIndex(workMonth);
                        //DateTime date = new DateTime(workYear.ObjToInt32(), iMonth, 1);
                        DateTime date = this.dateTimePicker1.Value;
                        workMonth = date.ToString("MMMMMMMMMM").ToUpper();
                        DateTime nextDate = date.AddMonths(1);
                        string nextMonth = nextDate.ToString("MMMMMMMMMM").ToUpper();
                        string cx = "";
                        string c1 = "";
                        string c2 = "";
                        string c3 = "";
                        string c4 = "";
                        string c5 = "";
                        string c6 = "";
                        string c8 = "";
                        string c9 = "";
                        string c10 = "";
                        string c11 = "";
                        string c12 = "";
                        string c13 = "";

                        double d = 0D;
                        DateTime conv = DateTime.Now;

                        int firstRow = -1;
                        int lastRow = -1;

                        bool gotHeader = false;

                        for (int i = 0; i < workDt.Rows.Count; i++)
                        {
                            month = workDt.Rows[i][0].ObjToString().Trim();
                            if (!gotHeader && month == "MONTH")
                            {
                                tempDt.ImportRow(workDt.Rows[i]);
                                tempDt2.ImportRow(workDt.Rows[i]);
                            }
                            if (!gotHeader && month == "(NOT REMITTED)")
                            {
                                gotHeader = true;
                                tempDt.ImportRow(workDt.Rows[i]);
                                tempDt2.ImportRow(workDt.Rows[i]);
                            }
                            if (month.IndexOf(workMonth) == 0)
                            {
                                if (firstRow >= 0)
                                    continue;
                                firstRow = i;
                                continue;
                            }
                            else if (firstRow >= 0)
                            {
                                if (month.IndexOf(nextMonth) == 0)
                                {
                                    lastRow = i - 1;
                                    break;
                                }
                            }
                        }

                        if (lastRow < 0)
                            lastRow = workDt.Rows.Count;
                        for (int i = firstRow; i < lastRow; i++)
                        {
                            c3 = workDt.Rows[i]["Column3"].ObjToString();
                            c4 = workDt.Rows[i]["Column4"].ObjToString();
                            c5 = workDt.Rows[i]["Column5"].ObjToString();
                            c6 = workDt.Rows[i]["Column6"].ObjToString();
                            c8 = workDt.Rows[i]["Column8"].ObjToString();
                            c9 = workDt.Rows[i]["Column9"].ObjToString();
                            c10 = workDt.Rows[i]["Column10"].ObjToString();
                            c11 = workDt.Rows[i]["Column11"].ObjToString();
                            c12 = workDt.Rows[i]["Column12"].ObjToString();
                            tempDt.ImportRow(workDt.Rows[i]);
                            tempDt2.ImportRow(workDt.Rows[i]);
                        }

                        for (int col = 0; col < tempDt.Columns.Count; col++)
                        {
                            if (tempDt.Rows[0][col].ObjToString().Trim().ToUpper() == "FORETHOUGHT")
                            {
                                tempDt.Columns[col].ColumnName = "Forethought";
                                continue;
                            }
                            if (tempDt.Rows[0][col].ObjToString().Trim().ToUpper() == "SECURITY NATIONAL")
                            {
                                tempDt.Columns[col].ColumnName = "Security National";
                                continue;
                            }
                            if (tempDt.Rows[0][col].ObjToString().Trim().ToUpper() == "FDLIC")
                            {
                                tempDt.Columns[col].ColumnName = "FDLIC " + tempDt.Rows[1][col].ObjToString().Trim();
                                continue;
                            }
                            if (tempDt.Rows[0][col].ObjToString().Trim().ToUpper() == "UNITY")
                            {
                                tempDt.Columns[col].ColumnName = "Unity " + tempDt.Rows[1][col].ObjToString().Trim();
                                continue;
                            }
                        }

                        DataTable trustDt = new DataTable();
                        trustDt.Columns.Add("Trust");
                        trustDt.Columns.Add("Date Received");
                        trustDt.Columns.Add("Funeral Number");
                        trustDt.Columns.Add("Name");

                        trustDt.Columns.Add("Sandra Money", Type.GetType("System.Double"));
                        trustDt.Columns.Add("Trust Money", Type.GetType("System.Double"));
                        trustDt.Columns.Add("Difference", Type.GetType("System.Double"));

                        trustDt.Columns.Add("Sandra Principal", Type.GetType("System.Double"));
                        trustDt.Columns.Add("Trust Principal", Type.GetType("System.Double"));
                        trustDt.Columns.Add("Principal Diff", Type.GetType("System.Double"));

                        DataTable diffDt = trustDt.Clone();

                        DataRow dRow = null;
                        DataRow[] dRows = null;
                        DataRow[] dRows2 = null;

                        string contractNumber = "";
                        string funeral = "";
                        string name = "";
                        double dValue = 0D;
                        double sandraTotal = 0D;
                        double trustTotal = 0D;
                        double difference = 0D;
                        double sandraMoney = 0D;
                        double sandraPrincipal = 0D;

                        int forethought_Col = G1.get_column_number(tempDt, "Forethought");
                        int security_Col = G1.get_column_number(tempDt, "Security National");
                        int unityWebb_Col = G1.get_column_number(tempDt, "Unity Old Webb");

                        for (int i = 3; i < tempDt.Rows.Count; i++)
                        {
                            try
                            {
                                if (forethought_Col > 0 && tempDt.Rows[0][forethought_Col + 1].ObjToString().Trim().ToUpper() == "DESCRIPTION")
                                {
                                    c1 = tempDt.Rows[i][forethought_Col].ObjToString().Trim();
                                    c2 = tempDt.Rows[i][forethought_Col + 1].ObjToString().Trim();
                                    if (!String.IsNullOrWhiteSpace(c2))
                                    {
                                        if (c2.ToUpper().IndexOf("DC CASH") >= 0)
                                            continue;
                                        if (c2.ToUpper().IndexOf("DC PAID") >= 0)
                                            continue;
                                        if (c2.ToUpper().IndexOf(" ADJ") >= 0)
                                            continue;
                                        c3 = tempDt.Rows[i][forethought_Col + 2].ObjToString().Trim();
                                        c4 = tempDt.Rows[i][forethought_Col + 3].ObjToString().Trim();
                                        c5 = tempDt.Rows[i][forethought_Col + 4].ObjToString().Trim();
                                        dRow = foreDt.NewRow();
                                        dRow["money"] = Convert.ToDouble(c1);
                                        dRow["name"] = c2;
                                        dRow["contract"] = c4;
                                        dRow["funeral"] = c5;
                                        if (!String.IsNullOrWhiteSpace(c3))
                                        {
                                            if (G1.validate_numeric(c3))
                                            {
                                                d = double.Parse(c3);
                                                conv = DateTime.FromOADate(d);
                                                dRow["date"] = conv.ToString("MM/dd/yyyy");
                                            }
                                            else
                                                dRow["date"] = c3.ObjToDateTime().ToString("MM/dd/yyyy");
                                        }

                                        foreDt.Rows.Add(dRow);
                                    }
                                }
                                if (security_Col > 0 && tempDt.Rows[0][security_Col + 2].ObjToString().Trim().ToUpper() == "DESCRIPTION")
                                {
                                    c1 = tempDt.Rows[i][security_Col].ObjToString().Trim();
                                    cx = tempDt.Rows[i][security_Col + 1].ObjToString().Trim();
                                    c2 = tempDt.Rows[i][security_Col + 2].ObjToString().Trim();
                                    if (!String.IsNullOrWhiteSpace(c2))
                                    {
                                        if (cx.ToUpper().IndexOf("DC CASH") >= 0)
                                            continue;
                                        if (cx.ToUpper().IndexOf("DC PAID") >= 0)
                                            continue;
                                        if (cx.ToUpper().IndexOf(" ADJ") >= 0)
                                            continue;
                                        c3 = tempDt.Rows[i][security_Col + 3].ObjToString().Trim();
                                        c4 = tempDt.Rows[i][security_Col + 4].ObjToString().Trim();
                                        c5 = tempDt.Rows[i][security_Col + 5].ObjToString().Trim();
                                        dRow = secDt.NewRow();
                                        dRow["money"] = Convert.ToDouble(c1);
                                        dRow["name"] = c2;
                                        dRow["contract"] = c4;
                                        dRow["funeral"] = c5;
                                        if (!String.IsNullOrWhiteSpace(c3))
                                        {
                                            if (G1.validate_numeric(c3))
                                            {
                                                d = double.Parse(c3);
                                                conv = DateTime.FromOADate(d);
                                                dRow["date"] = conv.ToString("MM/dd/yyyy");
                                            }
                                            else
                                                dRow["date"] = c3.ObjToDateTime().ToString("MM/dd/yyyy");
                                        }

                                        secDt.Rows.Add(dRow);
                                    }
                                }
                                if (unityWebb_Col > 0 && tempDt.Rows[0][security_Col + 1].ObjToString().Trim().ToUpper() == "DESCRIPTION")
                                {
                                    c1 = tempDt.Rows[i][unityWebb_Col].ObjToString().Trim();
                                    c2 = tempDt.Rows[i][unityWebb_Col + 1].ObjToString().Trim();
                                    if (!String.IsNullOrWhiteSpace(c2))
                                    {
                                        if (c2.ToUpper().IndexOf("DC CASH") >= 0)
                                            continue;
                                        if (c2.ToUpper().IndexOf("DC PAID") >= 0)
                                            continue;
                                        if (c2.ToUpper().IndexOf(" ADJ") >= 0)
                                            continue;
                                        c3 = tempDt.Rows[i][unityWebb_Col + 2].ObjToString().Trim();
                                        c4 = tempDt.Rows[i][unityWebb_Col + 3].ObjToString().Trim();
                                        c5 = tempDt.Rows[i][unityWebb_Col + 4].ObjToString().Trim();
                                        dRow = unityWebbDt.NewRow();
                                        dRow["money"] = Convert.ToDouble(c1);
                                        dRow["name"] = c2;
                                        dRow["contract"] = c4;
                                        dRow["funeral"] = c5;
                                        if (!String.IsNullOrWhiteSpace(c3))
                                        {
                                            if (G1.validate_numeric(c3))
                                            {
                                                d = double.Parse(c3);
                                                conv = DateTime.FromOADate(d);
                                                dRow["date"] = conv.ToString("MM/dd/yyyy");
                                            }
                                            else
                                                dRow["date"] = c3.ObjToDateTime().ToString("MM/dd/yyyy");
                                        }

                                        unityWebbDt.Rows.Add(dRow);
                                    }
                                }
                                //contractNumber = tempDt.Rows[i]["Column6"].ObjToString();
                                //if (workReport.IndexOf("Post 2002 Report - SN") == 0)
                                //    contractNumber = tempDt.Rows[i]["Column8"].ObjToString();
                                //if (String.IsNullOrWhiteSpace(contractNumber))
                                //    continue;
                                //contractNumber = contractNumber.Replace(".", "");
                                //contractNumber = contractNumber.Replace(",", "");
                                //if (String.IsNullOrWhiteSpace(contractNumber))
                                //    continue;

                                //if (contractNumber == "WC22002L")
                                //{
                                //}

                                //name = tempDt.Rows[i]["Column4"].ObjToString();
                                //if (workReport.IndexOf("Post 2002 Report - SN") == 0)
                                //    name = tempDt.Rows[i]["Column6"].ObjToString();
                                //if (String.IsNullOrWhiteSpace(name))
                                //    continue;
                                //sandraMoney = tempDt.Rows[i]["Column2"].ObjToDouble();
                                //if (workReport.IndexOf("Post 2002 Report - SN") == 0)
                                //    sandraMoney = tempDt.Rows[i]["Column4"].ObjToDouble();
                                //sandraMoney = Math.Abs(sandraMoney);

                                //funeral = tempDt.Rows[i]["Column7"].ObjToString();

                                //dRow = trustDt.NewRow();
                                //dRow["Trust"] = contractNumber;
                                //dRow["Name"] = name;
                                //dRow["Sandra Money"] = sandraMoney;

                                //sandraTotal += sandraMoney;

                                //dValue = 0D;

                                //dRows = dt.Select("`contractNumber`='" + contractNumber + "'");
                                //if (dRows != null)
                                //{
                                //    if (dRows.Length > 0)
                                //    {
                                //        dValue = dRows[0]["value"].ObjToDouble();
                                //        dValue = Math.Abs(dValue);
                                //        dRow["Trust Money"] = dValue;
                                //        dRow["Funeral Number"] = dRows[0]["funeral"].ObjToString();
                                //        //dRow["Date Received"] = dRows[0]["dateReceived"].ObjToDateTime().ToString("MM/dd/yyyy");
                                //        trustTotal += dValue;
                                //    }
                                //    else
                                //    {
                                //        dRow["Funeral Number"] = funeral;
                                //        dRow["Trust Money"] = sandraMoney;
                                //        dValue = sandraMoney;
                                //    }
                                //}
                                //difference = sandraMoney - dValue;
                                //difference = G1.RoundValue(difference);
                                //dRow["Difference"] = difference;

                                //trustDt.Rows.Add(dRow);
                            }
                            catch (Exception ex)
                            {
                            }
                        }

                        int count = unityWebbDt.Rows.Count;
                        count = secDt.Rows.Count;
                        count = foreDt.Rows.Count;

                        trustDt = LoadTrustDt(trustDt, "Security National", secDt);
                        trustDt = LoadTrustDt(trustDt, "Forethought", foreDt);
                        trustDt = LoadTrustDt(trustDt, "Unity Old Webb", unityWebbDt);



                        //for (int i = 0; i < tempDt2.Rows.Count; i++)
                        //{
                        //    try
                        //    {
                        //        funeral = "";
                        //        if (workReport == "Post 2002 Report - Unity")
                        //        {
                        //            funeral = tempDt2.Rows[i]["Column11"].ObjToString();
                        //            name = tempDt2.Rows[i]["Column9"].ObjToString();
                        //            if (String.IsNullOrWhiteSpace(name))
                        //                continue;
                        //            sandraMoney = tempDt2.Rows[i]["Column10"].ObjToDouble();
                        //            sandraMoney = Math.Abs(sandraMoney);
                        //        }
                        //        else if (workReport == "Post 2002 Report - FDLIC")
                        //        {
                        //            funeral = tempDt2.Rows[i]["Column14"].ObjToString();
                        //            name = tempDt2.Rows[i]["Column11"].ObjToString();
                        //            if (String.IsNullOrWhiteSpace(name))
                        //                continue;
                        //            sandraMoney = tempDt2.Rows[i]["Column12"].ObjToDouble();
                        //            sandraMoney = Math.Abs(sandraMoney);
                        //        }
                        //        else if (workReport.IndexOf("Post 2002 Report - SN") == 0)
                        //        {
                        //            funeral = tempDt2.Rows[i]["Column16"].ObjToString();
                        //            name = tempDt2.Rows[i]["Column13"].ObjToString();
                        //            if (String.IsNullOrWhiteSpace(name))
                        //                continue;
                        //            sandraMoney = tempDt2.Rows[i]["Column12"].ObjToDouble();
                        //            sandraMoney = Math.Abs(sandraMoney);
                        //        }
                        //        if (String.IsNullOrWhiteSpace(funeral))
                        //        {
                        //            if (String.IsNullOrWhiteSpace(name))
                        //                continue;
                        //            dRows = trustDt.Select("`Name`='" + name + "'");
                        //        }
                        //        else
                        //        {
                        //            dRows = trustDt.Select("`Funeral Number`='" + funeral + "'");
                        //            if (dRows.Length <= 0)
                        //                dRows = trustDt.Select("`Name`='" + name + "'");
                        //        }
                        //        if (funeral == "HH24001")
                        //        {
                        //        }
                        //        if (dRows.Length > 0)
                        //        {
                        //            DataTable dddd = dRows.CopyToDataTable();
                        //            string name2 = "";
                        //            for (int j = 0; j < dRows.Length; j++)
                        //            {
                        //                dValue = dRows[j]["Sandra Principal"].ObjToDouble();
                        //                if (dValue != 0D)
                        //                    continue;
                        //                if (dRows.Length > 1)
                        //                {
                        //                    name2 = dRows[j]["Name"].ObjToString();
                        //                    if (name != name2)
                        //                        continue;
                        //                }
                        //                dRows[j]["Sandra Principal"] = sandraMoney;
                        //                if (String.IsNullOrWhiteSpace(dRows[j]["Funeral Number"].ObjToString()))
                        //                    dRows[j]["Funeral Number"] = funeral;
                        //                sandraTotal += sandraMoney;
                        //                break;
                        //            }
                        //        }
                        //        else
                        //        {
                        //            if (name.ToUpper() == "OVERAGE")
                        //                continue;
                        //            if (name.ToUpper() == "SHORTAGE")
                        //                continue;
                        //            if (name.ToUpper().IndexOf("PMTS") > 0)
                        //                continue;
                        //            if (name.ToUpper().IndexOf("ADJ") > 0)
                        //                continue;
                        //            name += " (Missing) ";
                        //            dRow = diffDt.NewRow();
                        //            if (workReport.IndexOf("Post 2002 Report - SN") == 0)
                        //            {
                        //                name += tempDt2.Rows[i]["Column15"].ObjToString();
                        //            }
                        //            dRow["Name"] = name;
                        //            dRow["Funeral Number"] = funeral;

                        //            diffDt.Rows.Add(dRow);
                        //            continue;
                        //        }

                        //        dValue = 0D;

                        //        if (funeral.ToUpper().IndexOf("OS") < 0 && funeral.ToUpper().IndexOf("O/S") < 0)
                        //        {
                        //            dRows2 = dt.Select("funeral='" + funeral + "'");
                        //            if (dRows2 != null)
                        //            {
                        //                if (dRows2.Length > 0)
                        //                {
                        //                    dValue = dRows2[0]["principal"].ObjToDouble();
                        //                    dValue = Math.Abs(dValue);
                        //                    dRows[0]["Trust Principal"] = dValue;
                        //                    trustTotal += dValue;
                        //                    difference = dValue - sandraMoney;
                        //                    difference = G1.RoundValue(difference);
                        //                    dRows[0]["Principal Diff"] = difference;
                        //                }
                        //                else
                        //                {
                        //                    //dRow["Trust Principal"] = 0D;
                        //                    //dValue = sandraMoney;
                        //                }
                        //            }
                        //        }
                        //        //difference = sandraMoney - dValue;
                        //        //difference = G1.RoundValue(difference);
                        //        //dRow["PrincipalDifference"] = difference;

                        //        //trustDt.Rows.Add(dRow);
                        //    }
                        //    catch (Exception ex)
                        //    {
                        //    }
                        //}


                        for (int i = 0; i < trustDt.Rows.Count; i++)
                        {
                            contractNumber = trustDt.Rows[i]["trust"].ObjToString();
                            if (String.IsNullOrWhiteSpace(contractNumber))
                                continue;
                            funeral = trustDt.Rows[i]["Funeral Number"].ObjToString();
                            sandraMoney = trustDt.Rows[i]["Sandra Money"].ObjToDouble();
                            sandraPrincipal = trustDt.Rows[i]["Sandra Principal"].ObjToDouble();
                            dRows = dt.Select("contract='" + contractNumber + "'");
                            if (dRows.Length > 0)
                            {
                                if (String.IsNullOrWhiteSpace(funeral) || sandraPrincipal == 0D)
                                    diffDt.ImportRow(trustDt.Rows[i]);

                                if (dRows.Length > 1)
                                {
                                    for (int j = 0; j < dRows.Length; j++)
                                    {
                                        if (dRows[j]["sandraPrincipal"].ObjToDouble() == 0D)
                                        {
                                            dRows[j]["sandraMoney"] = sandraMoney;
                                            dRows[j]["sandraPrincipal"] = sandraPrincipal;
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    dValue = dRows[0]["sandraMoney"].ObjToDouble();
                                    dValue += sandraMoney;
                                    dRows[0]["sandraMoney"] = dValue;

                                    dValue = dRows[0]["sandraPrincipal"].ObjToDouble();
                                    dValue += sandraPrincipal;
                                    dRows[0]["sandraPrincipal"] = dValue;
                                }
                            }
                            else
                            {
                                name = trustDt.Rows[i]["name"].ObjToString();
                                if (name.ToUpper() == "DESCRIPTION")
                                    continue;
                                diffDt.ImportRow(trustDt.Rows[i]);
                            }
                        }

                        dgv.DataSource = dt;
                        dgv.Refresh();

                        dgv5.DataSource = diffDt;
                        dgv5.Refresh();

                        gridMain5.Columns["Sandra Money"].Visible = false;
                        gridMain5.Columns["Trust Money"].Visible = false;
                        gridMain5.Columns["Difference"].Visible = false;

                        gridMain.Columns["sandraMoney"].Visible = false;
                        gridMain.Columns["sandraPrincipal"].Visible = true;

                        gridMain.RefreshData();
                        gridMain.RefreshEditor(true);

                        gridMain.PostEditor();


                        this.Refresh();

                        this.Cursor = Cursors.Default;
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
        }
        /****************************************************************************************/
        private DataTable LoadTrustDt(DataTable trustDt, string trust, DataTable dt)
        {
            DataTable tDt = trustDt.Copy();
            string funeral = "";
            string contract = "";
            string name = "";
            double money = 0D;
            DateTime date = DateTime.Now;
            DataRow dRow = null;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                name = dt.Rows[i]["name"].ObjToString();
                money = dt.Rows[i]["money"].ObjToDouble();
                funeral = dt.Rows[i]["funeral"].ObjToString();
                contract = dt.Rows[i]["contract"].ObjToString();
                date = dt.Rows[i]["date"].ObjToDateTime();

                dRow = tDt.NewRow();
                dRow["Trust"] = contract;
                dRow["Funeral Number"] = funeral;
                dRow["Name"] = name;
                dRow["Sandra Principal"] = Math.Abs(money);
                dRow["Date Received"] = date.ToString("MM/dd/yyyy");

                tDt.Rows.Add(dRow);
            }
            return tDt;
        }
        /****************************************************************************************/
        private void post2002ReportUnityToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menu = (ToolStripMenuItem)sender;

            workReport = menu.Text;

            chkOldStuff.Checked = false;
            cmbPreOrPost.Text = "Post";
            chkJustTrustsSelected.Checked = true;

            chkCmbCompany.DeselectAll();

            chkCmbCompany.SetEditValue("Unity|Unity PB");

            chkCmbCompany.Refresh();

            cmbSelectColumns.Text = "Unity Post Totals";
            cmbSelectColumns.SelectedItem = "Unity Post Totals";
        }
        /****************************************************************************************/
        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            DateTime date = this.dateTimePicker1.Value;
            date = new DateTime(date.Year, date.Month, 1);
            this.dateTimePicker1.Value = date;

            int days = DateTime.DaysInMonth(date.Year, date.Month);
            DateTime stopDate = new DateTime(date.Year, date.Month, days);
            this.dateTimePicker2.Value = stopDate;
            this.dateTimePicker2.Refresh();
            this.dateTimePicker1.Refresh();
        }
        /****************************************************************************************/
        private void dateTimePicker2_ValueChanged(object sender, EventArgs e)
        {
            DateTime date = this.dateTimePicker2.Value;
            date = new DateTime(date.Year, date.Month, 1);
            this.dateTimePicker1.Value = date;

            int days = DateTime.DaysInMonth(date.Year, date.Month);
            DateTime stopDate = new DateTime(date.Year, date.Month, days);
            this.dateTimePicker2.Value = stopDate;
            this.dateTimePicker2.Refresh();
            this.dateTimePicker1.Refresh();
        }
        /****************************************************************************************/
        private void btnLeft_Click(object sender, EventArgs e)
        {
            DateTime now = this.dateTimePicker1.Value;
            now = now.AddMonths(-1);
            this.dateTimePicker1.Value = now;

            //if (gridMain.Columns["sandraMoney"].Visible)
            //    gridMain.Columns["sandraMoney"].Visible = false;
            //if (gridMain.Columns["sandraPrincipal"].Visible)
            //    gridMain.Columns["sandraPrincipal"].Visible = false;

            int days = DateTime.DaysInMonth(now.Year, now.Month);
            this.dateTimePicker2.Value = new DateTime(now.Year, now.Month, days);
            this.dateTimePicker2.Refresh();
            this.dateTimePicker1.Refresh();
        }
        /****************************************************************************************/
        private void btnRight_Click(object sender, EventArgs e)
        {
            DateTime now = this.dateTimePicker1.Value;
            now = now.AddMonths(1);
            this.dateTimePicker1.Value = now;

            //if (gridMain.Columns["sandraMoney"].Visible)
            //    gridMain.Columns["sandraMoney"].Visible = false;
            //if (gridMain.Columns["sandraPrincipal"].Visible)
            //    gridMain.Columns["sandraPrincipal"].Visible = false;

            int days = DateTime.DaysInMonth(now.Year, now.Month);
            this.dateTimePicker2.Value = new DateTime(now.Year, now.Month, days);
            this.dateTimePicker2.Refresh();
            this.dateTimePicker1.Refresh();
        }
        /****************************************************************************************/
        private DataTable CreateTempDt()
        {
            DataTable tempDt = new DataTable();
            tempDt.Columns.Add("record");
            tempDt.Columns.Add("month");
            tempDt.Columns.Add("desc");
            tempDt.Columns.Add("firstName");
            tempDt.Columns.Add("lastName");
            tempDt.Columns.Add("middleName");
            tempDt.Columns.Add("date");
            tempDt.Columns.Add("contract");
            tempDt.Columns.Add("funeral");
            tempDt.Columns.Add("value", Type.GetType("System.Double"));
            tempDt.Columns.Add("refunds", Type.GetType("System.Double"));
            tempDt.Columns.Add("principal", Type.GetType("System.Double"));
            tempDt.Columns.Add("ourFiledAmount", Type.GetType("System.Double"));
            tempDt.Columns.Add("balance", Type.GetType("System.Double"));
            tempDt.Columns.Add("policyNumber");
            tempDt.Columns.Add("trust");
            tempDt.Columns.Add("dateReceived");
            tempDt.Columns.Add("received", Type.GetType("System.Double"));
            tempDt.Columns.Add("deathClaimAmount", Type.GetType("System.Double"));
            tempDt.Columns.Add("reportDate");
            tempDt.Columns.Add("smfsBalance", Type.GetType("System.Double"));
            tempDt.Columns.Add("ftBalance", Type.GetType("System.Double"));
            tempDt.Columns.Add("manual");
            tempDt.Columns.Add("fun_amtReceived", Type.GetType("System.Double"));
            return tempDt;
        }
        /****************************************************************************************/
        private void btnRunTotals_Click(object sender, EventArgs e)
        {
            dgv2.Dock = DockStyle.Fill;
            dgv6.Dock = DockStyle.Fill;
            dgv10.Dock = DockStyle.Fill;

            tabControl1.SelectedTab = tabPage1;

            //if (dgv2.Visible)
            //{
            //    //LoadSplit();
            //    dgv2.Show();
            //    dgv2.Refresh();
            //    return;
            //}
            //if (dgv6.Visible)
            //{
            //    //LoadSplit();
            //    dgv6.Show();
            //    dgv6.Refresh();
            //    return;
            //}
            workDt = null;
            workNextDays = txtNextDays.Text.ObjToInt32();
            workCompanies = chkCmbCompany;
            if (String.IsNullOrWhiteSpace(workReport))
            {
                MessageBox.Show("*** ERROR *** You must select a report from the menu!", "No Report Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }

            PleaseWait pleaseForm = G1.StartWait("Please Wait for " + workReport + "!");

            try
            {
                this.Cursor = Cursors.WaitCursor;
                DataTable newDt = null;
                string companies = getCompanyQuery(workCompanies);
                DataTable dx = null;

                AddUnityColumns();

                if (workReport == "Pre 2002 Report")
                {
                    gridMain.Columns["value"].Caption = "Pre 2002 Trust Money (Them)";
                    gridMain.Columns["refunds"].Visible = false;
                    dx = LoadDeceased(this.dateTimePicker1.Value, this.dateTimePicker2.Value, workReport, ref newDt);
                }
                else if (workReport == "Post 2002 Report - SN & FT")
                {
                    gridMain.Columns["value"].Caption = "SN/FT Trust Money";
                    gridMain.Columns["refunds"].Visible = false;
                    //dx = LoadDeceasedSNFT(this.dateTimePicker1.Value, this.dateTimePicker2.Value, workReport, ref newDt);
                    dx = LoadDeceased(this.dateTimePicker1.Value, this.dateTimePicker2.Value, workReport, ref newDt);
                }
                else if (workReport == "Post 2002 Report - Forethought")
                {
                    gridMain.Columns["value"].Caption = "Forethought Trust Money";
                    gridMain.Columns["refunds"].Visible = false;
                    //dx = LoadDeceasedSNFT(this.dateTimePicker1.Value, this.dateTimePicker2.Value, workReport, ref newDt);
                    dx = LoadDeceased(this.dateTimePicker1.Value, this.dateTimePicker2.Value, workReport, ref newDt);
                }
                else if (companies.Contains("FDLIC"))
                {
                    gridMain.Columns["value"].Caption = "FDLIC Trust Money";
                    gridMain.Columns["refunds"].Visible = false;
                    gridMain.Columns["balance"].Visible = false;
                    gridMain.Columns["smfsBalance"].Visible = false;
                    gridMain.Columns["ftBalance"].Visible = false;
                    dx = LoadDeceased(this.dateTimePicker1.Value, this.dateTimePicker2.Value, workReport, ref newDt);
                }
                else
                {
                    if (companies.Contains("Unity"))
                    {
                        gridMain.Columns["value"].Caption = "Unity Trust Money (Them)";
                        gridMain.Columns["refunds"].Visible = true;
                    }
                    //dx = LoadDeceasedOther(this.dateTimePicker1.Value, this.dateTimePicker2.Value, workReport, ref newDt);
                    dx = LoadDeceased(this.dateTimePicker1.Value, this.dateTimePicker2.Value, workReport, ref newDt);
                }


                DataView tempview = newDt.DefaultView;
                tempview.Sort = "date asc";
                newDt = tempview.ToTable();

                //try
                //{
                //    LoadBeginningBalances(newDt);
                //}
                //catch (Exception ex)
                //{
                //}
                //try
                //{
                //    LoadEndingBalances(newDt);
                //}
                //catch (Exception ex)
                //{
                //}

                if (chkMissing.Checked)
                    newDt = ShowMissing(newDt);

                newDt = setupGrouping(newDt);

                G1.NumberDataTable(newDt);
                dgv.DataSource = newDt;
                this.Cursor = Cursors.Default;

                if (chkGroup.Checked)
                    gridMain.ExpandAllGroups();

                gridMain.Columns["sandraMoney"].Visible = false;
                gridMain.Columns["sandraPrincipal"].Visible = false;
                gridMain.Columns["difference"].Visible = false;
                gridMain.Columns["pdiff"].Visible = false;

                //btnSave.Show();
                //btnSave.Refresh();

                originalDt = newDt;
            }
            catch (Exception ex)
            {
            }

            btnSave.Hide();
            btnSave.Refresh();

            G1.StopWait(ref pleaseForm);
            pleaseForm = null;
        }
        /****************************************************************************************/
        private DataTable setupGrouping(DataTable dt)
        {
            if (G1.get_column_number(dt, "iMonth") < 0)
                dt.Columns.Add("iMonth");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (dt.Rows[i]["dateReceived"].ObjToDateTime() > this.dateTimePicker2.Value)
                    dt.Rows[i]["iMonth"] = "2";
                else
                    dt.Rows[i]["iMonth"] = "1";
            }
            return dt;
        }
        /****************************************************************************************/
        private void LoadBeginningBalances(DataTable dt)
        {
            if (workReport == "Pre 2002 Report")
                return;

            //LoadBeginningAdjustment(dt);

            double balance = 0D;
            double smfsBalance = 0D;
            double ftBalance = 0D;
            double dValue = 0D;
            string record = "";
            DataTable activeDt = null;

            string trustCompany = dt.TableName.Trim();
            if (workReport == "Post 2002 Report - SN & FT")
                trustCompany = "SNFT";
            else if (workReport == "Post 2002 Report - Unity")
                trustCompany = "Unity";
            else if (workReport == "Post 2002 Report - FDLIC")
                trustCompany = "FDLIC";
            else if (workReport == "Post 2002 Report - CD")
                trustCompany = "CD";

            if (trustCompany != "SNFT")
            {
                gridMain.Columns["ftBalance"].Visible = false;
                gridMain2.Columns["ftBalance"].Visible = false;

                //gridMain.Columns["smfsBalance"].Caption = "SMFS Balance";
                //gridMain2.Columns["smfsBalance"].Caption = "SMFS Total Balance";
                gridMain.Columns["smfsBalance"].Caption = "Trust Principal Balance (US)";
                gridMain2.Columns["smfsBalance"].Caption = "Trust Principal Balance (US)";
            }
            else
            {
                gridMain.Columns["ftBalance"].Visible = true;
                gridMain2.Columns["ftBalance"].Visible = true;

                gridMain.Columns["smfsBalance"].Caption = "SN/FT Total Balance";
                gridMain2.Columns["smfsBalance"].Caption = "SN/FT Total Balance";
            }

            string str = "";
            bool useTrustCalculatedBeginningBalance = true;
            bool useTrustCalculatedEndingBalance = true;
            bool useSMFSCalculatedBeginningBalance = true;
            bool useSMFSCalculatedEndingBalance = true;
            string[] balances = this.chkBalances.EditValue.ToString().Split('|');
            for (int i = 0; i < chkBalances.Properties.Items.Count; i++)
            {
                str = chkBalances.Properties.Items[i].Description.ObjToString();
                if (chkBalances.Properties.Items[i].CheckState == CheckState.Checked)
                {
                    if (str == "Use SMFS Manually Entered Beginning Balance")
                        useSMFSCalculatedBeginningBalance = false;
                    else if (str == "Use SMFS Manually Entered Ending Balance")
                        useSMFSCalculatedEndingBalance = false;
                    if (str == "Use Trust Manually Entered Beginning Balance")
                        useTrustCalculatedBeginningBalance = false;
                    else if (str == "Use Trust Manually Entered Ending Balance")
                        useTrustCalculatedEndingBalance = false;
                }
            }

            DateTime date = this.dateTimePicker2.Value;
            DateTime lastMonth = date.AddMonths(-1);
            int days = DateTime.DaysInMonth(lastMonth.Year, lastMonth.Month);
            lastMonth = new DateTime(lastMonth.Year, lastMonth.Month, days);

            string cmd = "Select * from `trust_data_edits` where `date` = '" + date.ToString("yyyy-MM-dd") + "' AND `status` = 'BeginningBalance' AND `trustName` = '" + trustCompany + "' AND `preOrPOst` = 'Post';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                if (!useTrustCalculatedBeginningBalance)
                    balance = dx.Rows[0]["beginningPaymentBalance"].ObjToDouble();
                if (!useSMFSCalculatedBeginningBalance)
                    ftBalance = dx.Rows[0]["beginningDeathBenefit"].ObjToDouble();
                smfsBalance = dx.Rows[0]["endingPaymentBalance"].ObjToDouble();
                record = dx.Rows[0]["record"].ObjToString();

                if (workReport == "Post 2002 Report - Unity")
                {
                    if (useTrustCalculatedBeginningBalance)
                        balance = loadUnityBalance(date.AddMonths(-1), cmbPreOrPost.Text.Trim(), ref activeDt);
                    //if (useSMFSCalculatedBeginningBalance)
                    //    smfsBalance = LoadTBB(date.AddMonths(-1));
                }
                else if (workReport == "Post 2002 Report - SN & FT")
                {
                    //dValue = ftBalance;
                    //ftBalance = smfsBalance;
                    //smfsBalance = dValue;
                }
                else if (workReport == "Post 2002 Report - CD")
                {
                    //dValue = ftBalance;
                    //ftBalance = smfsBalance;
                    //smfsBalance = dValue;
                }
            }
            else
            {
                if (workReport == "Post 2002 Report - Unity")
                {
                    cmd = "Select * from `trust_data_edits` where `date` = '" + lastMonth.ToString("yyyy-MM-dd") + "' AND `status` = 'EndingBalance' AND `trustName` = '" + trustCompany + "' AND `preOrPost` = 'Post';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                    {
                        if (!useTrustCalculatedBeginningBalance)
                            balance = dx.Rows[0]["beginningPaymentBalance"].ObjToDouble();
                        if (!useSMFSCalculatedBeginningBalance)
                            smfsBalance = dx.Rows[0]["beginningDeathBenefit"].ObjToDouble();
                        ftBalance = dx.Rows[0]["endingPaymentBalance"].ObjToDouble();
                        //dValue = ftBalance;
                        //ftBalance = smfsBalance;
                        //smfsBalance = dValue;
                    }
                    if (balance == 0D)
                        balance = loadUnityBalance(date.AddMonths(-1), cmbPreOrPost.Text.Trim(), ref activeDt);
                    //smfsBalance = LoadTBB(date.AddMonths(-1));
                }
                else if (workReport == "Post 2002 Report - SN & FT")
                {
                    cmd = "Select * from `trust_data_edits` where `date` = '" + lastMonth.ToString("yyyy-MM-dd") + "' AND `status` = 'EndingBalance' AND `trustName` = '" + trustCompany + "' AND `preOrPost` = 'Post';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                    {
                        if (!useTrustCalculatedBeginningBalance)
                            balance = dx.Rows[0]["beginningPaymentBalance"].ObjToDouble();
                        if (!useSMFSCalculatedBeginningBalance)
                            ftBalance = dx.Rows[0]["beginningDeathBenefit"].ObjToDouble();
                        smfsBalance = dx.Rows[0]["endingPaymentBalance"].ObjToDouble();
                    }
                    if (balance == 0D)
                    {
                        balance = loadSecurityNationalBalance(lastMonth, cmbPreOrPost.Text.Trim(), ref activeDt);
                        ftBalance = loadForethoughtBalance(lastMonth, cmbPreOrPost.Text.Trim(), ref activeDt);
                    }
                }
                else if (workReport == "Post 2002 Report - FDLIC")
                {
                    cmd = "Select * from `trust_data_edits` where `date` = '" + lastMonth.ToString("yyyy-MM-dd") + "' AND `status` = 'EndingBalance' AND `trustName` = '" + trustCompany + "' AND `preOrPost` = 'Post';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                    {
                        if (!useTrustCalculatedBeginningBalance)
                            balance = dx.Rows[0]["beginningPaymentBalance"].ObjToDouble();
                        if (!useSMFSCalculatedBeginningBalance)
                            ftBalance = dx.Rows[0]["beginningDeathBenefit"].ObjToDouble();
                        smfsBalance = dx.Rows[0]["endingPaymentBalance"].ObjToDouble();
                    }
                }
                else if (workReport == "Post 2002 Report - CD")
                {
                    cmd = "Select * from `trust_data_edits` where `date` = '" + lastMonth.ToString("yyyy-MM-dd") + "' AND `status` = 'EndingBalance' AND `trustName` = '" + trustCompany + "' AND `preOrPost` = 'Post';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                    {
                        if (!useTrustCalculatedBeginningBalance)
                            balance = dx.Rows[0]["beginningPaymentBalance"].ObjToDouble();
                        if (!useSMFSCalculatedBeginningBalance)
                            ftBalance = dx.Rows[0]["beginningDeathBenefit"].ObjToDouble();
                        smfsBalance = dx.Rows[0]["endingPaymentBalance"].ObjToDouble();
                    }
                }
            }
            DataRow dRow = dt.NewRow();
            dRow["record"] = record.ObjToInt32();
            dRow["month"] = "Beginning Balance";
            dRow["value"] = balance;
            dRow["smfsBalance"] = smfsBalance;
            dRow["received"] = ftBalance;
            if (trustCompany == "SNFT")
            {
                dRow["smfsBalance"] = balance + ftBalance;
                //dRow["received"] = balance;
                //dRow["value"] = ftBalance;
            }
            else if (workReport == "Post 2002 Report - Unity")
                dRow["received"] = smfsBalance;
            else if (workReport == "Post 2002 Report - FDLIC")
                dRow["received"] = ftBalance;
            else if (workReport == "Post 2002 Report - CD")
            {
                dRow["received"] = ftBalance;
                dRow["smfsBalance"] = balance + ftBalance;
            }
            dt.Rows.InsertAt(dRow, 0);

            if (trustCompany == "SNFT")
            {
                gridMain2.Columns["balance"].Caption = "Security National Trust Balance";
                gridMain2.Columns["value"].Caption = "Security National Trust Balance";
                gridMain2.Columns["ftBalance"].Caption = "Forethought Trust Balance";
                gridMain2.Columns["ftBalance"].Visible = false;
            }
            else if (workReport == "Post 2002 Report - Unity" || workReport == "Post 2002 Report - FDLIC")
            {
                gridMain2.Columns["smfsBalance"].Visible = false;
                gridMain2.Columns["received"].Caption = trustCompany + " Trust Principal";
            }

            dt = loadBalanceDifferences(dt, "BEGINNING");
            dt = loadCadenceDeathBenefits(dt, "BEGINNING");
        }
        /****************************************************************************************/
        private DataTable loadBalanceDifferences ( DataTable dt, string which )
        {
            if (G1.get_column_number(dt, "diff") < 0)
                dt.Columns.Add("diff", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "cadenceDeathBenefits") < 0)
                dt.Columns.Add("cadenceDeathBenefits", Type.GetType("System.Double"));

            DataRow[] dRows = null;
            double received = 0D;
            double balance = 0D;
            double diff = 0D;

            if ( which.ToUpper() == "BEGINNING")
            {
                dRows = dt.Select("month='Beginning Balance'");
                if ( dRows.Length > 0 )
                {
                    received = dRows[0]["value"].ObjToDouble();
                    balance = dRows[0]["received"].ObjToDouble();
                    diff = balance - received;
                    dRows[0]["diff"] = diff;
                }
            }
            else
            {
                dRows = dt.Select("month='Ending Balance'");
                if (dRows.Length > 0)
                {
                    received = dRows[0]["value"].ObjToDouble();
                    balance = dRows[0]["received"].ObjToDouble();
                    diff = balance - received;
                    dRows[0]["diff"] = diff;
                }
            }
            return dt;
        }
        /****************************************************************************************/
        private DataTable loadCadenceDeathBenefits(DataTable dt, string which)
        {
            if (G1.get_column_number(dt, "diff") < 0)
                dt.Columns.Add("diff", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "cadenceDeathBenefits") < 0)
                dt.Columns.Add("cadenceDeathBenefits", Type.GetType("System.Double"));

            DataRow[] dRows = null;
            double received = 0D;
            double balance = 0D;
            double diff = 0D;
            DataTable activeDt = null;

            string preOrPost = "Post";
            DateTime date = this.dateTimePicker2.Value;

            if (which.ToUpper() == "BEGINNING")
            {
                dRows = dt.Select("month='Beginning Balance'");
                if (dRows.Length > 0)
                {
                    if (workReport == "Post 2002 Report - Unity")
                    {
                        date = this.dateTimePicker1.Value;
                        date = date.AddDays(-1);
                        double deathBenefit = loadUnityDeathBenefit(date, preOrPost, ref activeDt);
                        dRows[0]["cadenceDeathBenefits"] = deathBenefit;
                    }
                    else if (workReport == "Post 2002 Report - FDLIC")
                    {
                        date = this.dateTimePicker1.Value;
                        date = date.AddDays(-1);
                        double deathBenefit = loadFDLICDeathBenefit(date, preOrPost, ref activeDt);
                        dRows[0]["cadenceDeathBenefits"] = deathBenefit;
                    }
                }
            }
            else
            {
                dRows = dt.Select("month='Ending Balance'");
                if (dRows.Length > 0)
                {
                    if (workReport == "Post 2002 Report - Unity")
                    {
                        double deathBenefit = loadUnityDeathBenefit(date, preOrPost, ref activeDt);
                        dRows[0]["cadenceDeathBenefits"] = deathBenefit;
                    }
                    else if (workReport == "Post 2002 Report - FDLIC")
                    {
                        double deathBenefit = loadFDLICDeathBenefit(date, preOrPost, ref activeDt);
                        dRows[0]["cadenceDeathBenefits"] = deathBenefit;
                    }
                }
            }
            return dt;
        }
        /****************************************************************************************/
        //private void LoadBeginningAdjustment(DataTable dt)
        //{
        //    double balance = 0D;
        //    double smfsBalance = 0D;
        //    double ftBalance = 0D;
        //    string record = "";

        //    string trustCompany = dt.TableName.Trim();
        //    if (workReport == "Post 2002 Report - SN & FT")
        //        trustCompany = "SNFT";
        //    else if (workReport == "Post 2002 Report - Unity")
        //        trustCompany = "Unity";
        //    else if (workReport == "Post 2002 Report - FDLIC")
        //        trustCompany = "FDLIC";

        //    if (trustCompany != "SNFT")
        //    {
        //        gridMain.Columns["ftBalance"].Visible = false;
        //        gridMain2.Columns["ftBalance"].Visible = false;

        //        gridMain.Columns["smfsBalance"].Caption = "SMFS Balance";
        //        gridMain2.Columns["smfsBalance"].Caption = "SMFS Total Balance";
        //    }
        //    else
        //    {
        //        gridMain.Columns["ftBalance"].Visible = true;
        //        gridMain2.Columns["ftBalance"].Visible = true;

        //        gridMain.Columns["smfsBalance"].Caption = "SN/FT Total Balance";
        //        gridMain2.Columns["smfsBalance"].Caption = "SN/FT Total Balance";
        //    }

        //    DateTime date = this.dateTimePicker2.Value;
        //    string cmd = "Select * from `trust_data_edits` where `date` = '" + date.ToString("yyyy-MM-dd") + "' AND `status` = 'BeginningAdjustment' AND `trustName` = '" + trustCompany + "';";
        //    DataTable dx = G1.get_db_data(cmd);
        //    if (dx.Rows.Count > 0)
        //    {
        //        balance = dx.Rows[0]["beginningPaymentBalance"].ObjToDouble();
        //        smfsBalance = dx.Rows[0]["beginningDeathBenefit"].ObjToDouble();
        //        ftBalance = dx.Rows[0]["endingPaymentBalance"].ObjToDouble();
        //        record = dx.Rows[0]["record"].ObjToString();
        //    }

        //    DataRow dRow = dt.NewRow();
        //    dRow["record"] = record.ObjToInt32();
        //    dRow["month"] = "Beginning Adjustment";
        //    dRow["balance"] = balance;
        //    dRow["smfsBalance"] = smfsBalance;
        //    dRow["ftBalance"] = ftBalance;
        //    if (trustCompany == "SNFT")
        //        dRow["smfsBalance"] = balance + ftBalance;
        //    dRow["reportDate"] = G1.DTtoMySQLDT(workDate.ToString("MM/dd/yyyy"));
        //    dt.Rows.InsertAt(dRow, 0);
        //}
        /****************************************************************************************/
        private double loadFDLICBalance(DateTime date, string preOrPost, ref DataTable activeDt)
        {
            DateTime newDate = new DateTime(date.Year, date.Month, 1);
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            DateTime newDate2 = new DateTime(date.Year, date.Month, days);

            string startDate = newDate.ToString("yyyy-MM-dd");
            string stopDate = newDate2.ToString("yyyy-MM-dd") + " 23:59:59";


            string cmd = "Select * from `trust_data` WHERE `date` >= '" + startDate + "' AND `date` <= '" + stopDate + "' AND `preOrPost` = '" + preOrPost + "' ";
            cmd += " AND `trustCompany` IN ('FDLIC','FDLIC PB') ";
            cmd += " ORDER by `date`, `trustCompany`;  ";

            DataTable dt = G1.get_db_data(cmd);

            //DataRow[] dRows = dt.Select("(`trustCompany` = 'FDLIC' OR `trustCompany` = 'FDLIC PB' ) AND `endingDeathBenefit` > '0.00' ");
            DataRow[] dRows = dt.Select("(`trustCompany` = 'FDLIC' OR `trustCompany` = 'FDLIC PB' ) AND `endingPaymentBalance` > '0.00' ");
            if (dRows.Length <= 0)
                return 0D;

            dt = dRows.CopyToDataTable();

            DataView tempview = dt.DefaultView;
            tempview.Sort = "date ASC";
            dt = tempview.ToTable();

            if (activeDt == null)
                activeDt = new DataTable();

            activeDt = dt.Clone();

            double balance = 0D;
            //balance = sumUpColumn(dt, "beginningDeathBenefit");
            balance = sumUpColumn(dt, "endingPaymentBalance");

            //DataTable lapsedDt = dt.Clone();
            //DataTable questionedDt = dt.Clone();
            //DataTable pbDt = dt.Clone();

            //activeDt = pullUnityActive(dt);
            //balance = sumUpColumn(activeDt, "beginningDeathBenefit");
            //Trust85.FindContract(activeDt, "L14140UI");

            //lapsedDt = pullUnityLapsed(dt);
            //balance += sumUpColumn(lapsedDt, "beginningDeathBenefit");
            //activeDt.Merge(lapsedDt);

            //questionedDt = pullUnityLapsedQuestioned(dt);
            //balance += sumUpColumn(questionedDt, "beginningDeathBenefit");
            //activeDt.Merge(questionedDt);

            //pbDt = pullUnityPB(dt);
            //balance += sumUpColumn(pbDt, "beginningDeathBenefit");
            //activeDt.Merge(pbDt);
            //Trust85.FindContract(activeDt, "L14140UI");

            return balance;
        }
        /****************************************************************************************/
        private double loadFDLICDeathBenefit (DateTime date, string preOrPost, ref DataTable activeDt)
        {
            DateTime newDate = new DateTime(date.Year, date.Month, 1);
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            DateTime newDate2 = new DateTime(date.Year, date.Month, days);

            string startDate = newDate.ToString("yyyy-MM-dd");
            string stopDate = newDate2.ToString("yyyy-MM-dd") + " 23:59:59";


            string cmd = "Select * from `trust_data` WHERE `date` >= '" + startDate + "' AND `date` <= '" + stopDate + "' AND `preOrPost` = '" + preOrPost + "' ";
            cmd += " AND `trustCompany` IN ('FDLIC','FDLIC PB') ";
            cmd += " ORDER by `date`, `trustCompany`;  ";

            DataTable dt = G1.get_db_data(cmd);

            DataRow[] dRows = dt.Select("(`trustCompany` = 'FDLIC' OR `trustCompany` = 'FDLIC PB' ) AND `endingDeathBenefit` <> '0.00' ");
            if (dRows.Length <= 0)
                return 0D;

            dt = dRows.CopyToDataTable();


            DataView tempview = dt.DefaultView;
            tempview.Sort = "date ASC";
            dt = tempview.ToTable();

            if (activeDt == null)
                activeDt = new DataTable();

            activeDt = dt.Clone();

            double balance = 0D;
            balance = sumUpColumn(dt, "endingDeathBenefit");

            return balance;
        }
        /****************************************************************************************/
        private double loadUnityBalance(DateTime date, string preOrPost, ref DataTable activeDt)
        {
            DateTime newDate = new DateTime(date.Year, date.Month, 1);
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            DateTime newDate2 = new DateTime(date.Year, date.Month, days);

            string startDate = newDate.ToString("yyyy-MM-dd");
            string stopDate = newDate2.ToString("yyyy-MM-dd") + " 23:59:59";

            string cmd = "Select * from `trust_data` WHERE `date` >= '" + startDate + "' AND `date` <= '" + stopDate + "' AND `preOrPost` = '" + preOrPost + "' ";
            cmd += " AND `trustCompany` IN ('Unity','Unity PB') ";
            cmd += " ORDER by `date`, `trustCompany`;  ";

            DataTable dt = G1.get_db_data(cmd);

            DataView tempview = dt.DefaultView;
            tempview.Sort = "date ASC";
            dt = tempview.ToTable();

            if (activeDt == null)
                activeDt = new DataTable();

            activeDt = dt.Clone();
            DataTable lapsedDt = dt.Clone();
            DataTable questionedDt = dt.Clone();
            DataTable pbDt = dt.Clone();

            activeDt = pullUnityActive(dt);
            double balance = sumUpColumn(activeDt, "beginningDeathBenefit");

            lapsedDt = pullUnityLapsed(dt);
            balance += sumUpColumn(lapsedDt, "beginningDeathBenefit");
            activeDt.Merge(lapsedDt);

            questionedDt = pullUnityLapsedQuestioned(dt);
            balance += sumUpColumn(questionedDt, "beginningDeathBenefit");
            activeDt.Merge(questionedDt);

            pbDt = pullUnityPB(dt);
            balance += sumUpColumn(pbDt, "beginningDeathBenefit");
            activeDt.Merge(pbDt);

            return balance;
        }
        /****************************************************************************************/
        private double loadUnityDeathBenefit(DateTime date, string preOrPost, ref DataTable activeDt)
        {
            DateTime newDate = new DateTime(date.Year, date.Month, 1);
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            DateTime newDate2 = new DateTime(date.Year, date.Month, days);

            string startDate = newDate.ToString("yyyy-MM-dd");
            string stopDate = newDate2.ToString("yyyy-MM-dd") + " 23:59:59";

            string cmd = "Select * from `trust_data` WHERE `date` >= '" + startDate + "' AND `date` <= '" + stopDate + "' AND `preOrPost` = '" + preOrPost + "' ";
            cmd += " AND `trustCompany` IN ('Unity','Unity PB') ";
            cmd += " ORDER by `date`, `trustCompany`;  ";

            DataTable dt = G1.get_db_data(cmd);

            DataView tempview = dt.DefaultView;
            tempview.Sort = "date ASC";
            dt = tempview.ToTable();

            if (activeDt == null)
                activeDt = new DataTable();

            activeDt = dt.Clone();
            DataTable lapsedDt = dt.Clone();
            DataTable questionedDt = dt.Clone();
            DataTable pbDt = dt.Clone();

            activeDt = pullUnityActive(dt);
            double balance = sumUpColumn(activeDt, "endingDeathBenefit");
            double active = balance;
            //double balance = sumUpColumn(activeDt, "beginningDeathBenefit");

            lapsedDt = pullUnityLapsed(dt);
            //balance += sumUpColumn(lapsedDt, "endingDeathBenefit");
            double lapsed = sumUpColumn(lapsedDt, "beginningDeathBenefit");
            balance += lapsed;

            activeDt.Merge(lapsedDt);

            questionedDt = pullUnityLapsedQuestioned(dt);
            double questioned = sumUpColumn(questionedDt, "endingDeathBenefit");
            //double questioned = sumUpColumn(questionedDt, "beginningDeathBenefit");
            balance += questioned;
            activeDt.Merge(questionedDt);

            pbDt = pullUnityPB(dt);
            double pb = sumUpColumn(pbDt, "endingDeathBenefit");
            balance += pb;
            //balance += sumUpColumn(pbDt, "beginningDeathBenefit");
            activeDt.Merge(pbDt);

            //balance = sumUpColumn(dt, "endingDeathBenefit");

            return balance;
        }
        /****************************************************************************************/
        public static double loadSecurityNationalBalance(DateTime date, string preOrPost, ref DataTable activeDt)
        {
            DateTime newDate = new DateTime(date.Year, date.Month, 1);
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            DateTime newDate2 = new DateTime(date.Year, date.Month, days);

            string startDate = newDate.ToString("yyyy-MM-dd");
            string stopDate = newDate2.ToString("yyyy-MM-dd") + " 23:59:59";

            string cmd = "Select * from `trust_data` WHERE `date` >= '" + startDate + "' AND `date` <= '" + stopDate + "' AND `preOrPost` = '" + preOrPost + "' ";
            cmd += " AND `trustCompany` IN ('Security National') ";
            cmd += " ORDER by `date`, `trustCompany`;  ";

            DataTable dt = G1.get_db_data(cmd);


            //dt = fixTheData(dt, true);

            DataView tempview = dt.DefaultView;
            tempview.Sort = "date ASC";
            dt = tempview.ToTable();

            activeDt = dt.Clone();
            activeDt = pullActiveSecurityNational(dt, preOrPost);

            DataRow[] dRows = activeDt.Select("policyNumber='F131747A'");
            if (dRows.Length > 0)
            {
                DataTable dx = dRows.CopyToDataTable();
            }
            double balance = sumUpColumn(activeDt, "endingDeathBenefit");

            return balance;
        }
        /****************************************************************************************/
        public static double loadForethoughtBalance(DateTime date, string preOrPost, ref DataTable activeDt)
        {
            DateTime newDate = new DateTime(date.Year, date.Month, 1);
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            DateTime newDate2 = new DateTime(date.Year, date.Month, days);

            string startDate = newDate.ToString("yyyy-MM-dd");
            string stopDate = newDate2.ToString("yyyy-MM-dd") + " 23:59:59";

            string cmd = "Select * from `trust_data` WHERE `date` >= '" + startDate + "' AND `date` <= '" + stopDate + "' AND `preOrPost` = '" + preOrPost + "' ";
            cmd += " AND `trustCompany` IN ('Forethought') ";
            cmd += " ORDER by `date`, `trustCompany`;  ";

            DataTable dt = G1.get_db_data(cmd);


            //dt = fixTheData(dt, true);

            DataView tempview = dt.DefaultView;
            tempview.Sort = "date ASC";
            dt = tempview.ToTable();

            activeDt = dt.Clone();
            activeDt = pullActiveForethought(dt, preOrPost);

            //DataRow[] dRows = activeDt.Select("policyNumber='F131747A'");
            //if (dRows.Length > 0)
            //{
            //    DataTable dx = dRows.CopyToDataTable();
            //}
            double balance = sumUpColumn(activeDt, "endingDeathBenefit");

            return balance;
        }
        /***********************************************************************************************/
        public static DataTable pullActiveForethought(DataTable dt, string preOrPost)
        {
            DataTable dx = dt.Clone();
            //dRows = dt.Select("`Death Benefit` <> '0.00' AND `Policy Extract_Policy Status` = 'A' AND `Policy Number` LIKE '77%'");

            //if (preOrPost == "Both" && chkOldStuff.Checked)
            //    preOrPost = "Pre";
            if (preOrPost == "Both")
                preOrPost = "Pre";

            DataRow[] dRows = null;
            if (!String.IsNullOrWhiteSpace(preOrPost))
                dRows = dt.Select("`trustCompany` = 'FORETHOUGHT' AND `endingDeathBenefit` <> '0.00' AND `preOrPost` = '" + preOrPost + "'");
            else
                dRows = dt.Select("`trustCompany` = 'FORETHOUGHT' AND `endingDeathBenefit` <> '0.00'");

            if (dRows.Length > 0)
                dx = dRows.CopyToDataTable();
            return dx;
        }
        /***********************************************************************************************/
        public static double pullActiveForethoughtValue(DataTable dt, string preOrPost)
        {
            DataTable dx = dt.Clone();
            //dRows = dt.Select("`Death Benefit` <> '0.00' AND `Policy Extract_Policy Status` = 'A' AND `Policy Number` LIKE '77%'");

            //if (preOrPost == "Both" && chkOldStuff.Checked)
            //    preOrPost = "Pre";
            if (preOrPost == "Both")
                preOrPost = "Pre";

            DataRow[] dRows = null;
            if (!String.IsNullOrWhiteSpace(preOrPost))
                dRows = dt.Select("`trustCompany` = 'FORETHOUGHT' AND `endingDeathBenefit` <> '0.00' AND `deathClaimAmount` = '0.00' AND `preOrPost` = '" + preOrPost + "'");
            else
                dRows = dt.Select("`trustCompany` = 'FORETHOUGHT' AND `endingDeathBenefit` <> '0.00' AND `deathClaimAmount` = '0.00' ");

            if (dRows.Length > 0)
                dx = dRows.CopyToDataTable();

            double total = 0D;
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                total += dx.Rows[i]["endingDeathBenefit"].ObjToDouble();
            }
            return total;
        }
        /***********************************************************************************************/
        public static double sumUpColumn(DataTable dt, string column)
        {
            if (dt == null)
                return 0D;
            double dValue = 0D;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dValue += dt.Rows[i][column].ObjToDouble();
            }
            return dValue;
        }
        /***********************************************************************************************/
        private DataTable pullUnityActive(DataTable dt)
        {
            Trust85.FindContract(dt, "L14140UI");
            DataTable dx = dt.Clone();
            DataRow[] dRows = dt.Select("`trustCompany` = 'Unity' AND `beginningDeathBenefit` <> '0.00' AND `policyStatus` = 'A'  AND `policyNumber` LIKE '77%' ");
            if (dRows.Length > 0)
                dx = dRows.CopyToDataTable();
            Trust85.FindContract(dx, "L14140UI");
            return dx;
        }
        /***********************************************************************************************/
        private DataTable pullUnityLapsed(DataTable dt)
        {
            Trust85.FindContract(dt, "L14140UI");
            DataTable dx = dt.Clone();
            DataRow[] dRows = dt.Select("`trustCompany` = 'Unity' AND `endingDeathBenefit` = '0.00' AND `policyStatus` = 'T'  AND `policyNumber` LIKE '77%' AND `statusReason` IN ('LP','NI','NN','NT','SR')");
            if (dRows.Length > 0)
                dx = dRows.CopyToDataTable();
            Trust85.FindContract(dx, "L14140UI");
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
        private DataTable pullUnityPB(DataTable dt)
        {
            DataTable dx = dt.Clone();
            DataRow[] dRows = dt.Select("`trustCompany` = 'Unity PB' AND `endingDeathBenefit` <> '0.00' AND `policyStatus` = 'A' AND `policyNumber` LIKE 'P%' AND `policyNumber` NOT LIKE 'PB%' AND `policyNumber` NOT LIKE 'PSPNB%' AND `policyNumber` NOT LIKE 'PSPWT%'");
            if (dRows.Length > 0)
            {
                dx = dRows.CopyToDataTable();
                double dValue = 0D;
                for (int i = 0; i < dx.Rows.Count; i++)
                {
                    dValue = dx.Rows[i]["endingDeathBenefit"].ObjToDouble();
                    dx.Rows[i]["beginningDeathBenefit"] = dValue;
                }
            }
            return dx;
        }
        /***********************************************************************************************/
        public static DataTable pullActiveSecurityNational(DataTable dt, string preOrPost)
        {
            DataTable dx = dt.Clone();

            //string preOrPost = cmbPreOrPost.Text;
            //if (preOrPost == "Both" && chkOldStuff.Checked)
            //    preOrPost = "Pre";
            if (preOrPost == "Both")
                preOrPost = "Pre";

            DataRow[] dRows = dt.Select("`trustCompany` = 'Security National' AND `endingDeathBenefit` <> '0.00'  AND `preOrPost` = '" + preOrPost + "'");
            if (dRows.Length > 0)
                dx = dRows.CopyToDataTable();
            return dx;
        }
        /****************************************************************************************/
        private void LoadEndingBalances(DataTable dt, bool editing = false )
        {
            double balance = 0D;
            double smfsBalance = 0D;
            double ftBalance = 0D;
            double trustTotal = 0D;
            double smfsTotal = 0D;
            double principalTotal = 0D;
            double ftTotal = 0D;
            double dValue = 0D;

            double endingTrustBalance = 0D;
            double endingSmfsBalance = 0D;
            double endingFtBalance = 0D;

            double beginningTrustBalance = 0D;
            double beginningSmfsBalance = 0D;
            double beginningFtBalance = 0D;

            double endingForethoughtBalance = 0D;

            double lastTrustBalance = 0D;
            double lastSmfsBalance = 0D;
            double lastFtBalance = 0D;

            double beginningTrustAdjustment = 0D;
            double beginningSmfsAdjustment = 0D;
            double beginningFtAdjustment = 0D;

            double endingTrustAdjustment = 0D;
            double endingSmfsAdjustment = 0D;
            double endingFtAdjustment = 0D;

            string str = "";
            DataTable activeDt = null;

            string month = "";
            int endingRow = -1;
            int adjustmentManualRow = -1;
            string adjustmentRecord = "";
            string endingBalanceRecord = "";
            int adjustmentRow = -1;

            DateTime date = this.dateTimePicker2.Value;
            DateTime currentEOM = date;
            DateTime currentBOM = date;
            date = date.AddMonths(1);
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            date = new DateTime(date.Year, date.Month, days);

            DateTime lastMonth = currentEOM.AddMonths(-1);
            days = DateTime.DaysInMonth(lastMonth.Year, lastMonth.Month);
            lastMonth = new DateTime(lastMonth.Year, lastMonth.Month, days);

            string trustCompany = "";
            if (workReport == "Post 2002 Report - SN & FT")
                trustCompany = "SNFT";
            else if (workReport == "Post 2002 Report - Unity")
                trustCompany = "Unity";
            else if (workReport == "Post 2002 Report - FDLIC")
                trustCompany = "FDLIC";
            else if (workReport == "Post 2002 Report - CD")
                trustCompany = "CD";
            else if (workReport == "Pre 2002 Report")
                trustCompany = "Pre2002";

            bool useTrustCalculatedBeginningBalance = true;
            bool useTrustCalculatedEndingBalance = true;
            bool useSMFSCalculatedBeginningBalance = true;
            bool useSMFSCalculatedEndingBalance = true;
            string[] balances = this.chkBalances.EditValue.ToString().Split('|');
            for (int i = 0; i < chkBalances.Properties.Items.Count; i++)
            {
                str = chkBalances.Properties.Items[i].Description.ObjToString();
                if (chkBalances.Properties.Items[i].CheckState == CheckState.Checked)
                {
                    if (str == "Use SMFS Manually Entered Beginning Balance")
                        useSMFSCalculatedBeginningBalance = false;
                    else if (str == "Use SMFS Manually Entered Ending Balance")
                        useSMFSCalculatedEndingBalance = false;
                    if (str == "Use Trust Manually Entered Beginning Balance")
                        useTrustCalculatedBeginningBalance = false;
                    else if (str == "Use Trust Manually Entered Ending Balance")
                        useTrustCalculatedEndingBalance = false;
                }
            }

            string cmd = "Select * from `trust_data_edits` where `date` = '" + currentBOM.ToString("yyyy-MM-dd") + "' AND `status` = 'BeginningBalance' AND `trustName` = '" + trustCompany + "' AND `preOrPost` = 'Post' AND `policyStatus` <> 'SPLIT';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                if (!useTrustCalculatedBeginningBalance)
                    beginningTrustBalance = dx.Rows[0]["beginningPaymentBalance"].ObjToDouble();
                if (!useSMFSCalculatedBeginningBalance)
                    beginningSmfsBalance = dx.Rows[0]["beginningDeathBenefit"].ObjToDouble();
                beginningFtBalance = dx.Rows[0]["endingPaymentBalance"].ObjToDouble();
                if (workReport == "Post 2002 Report - FDLIC")
                    beginningSmfsBalance = beginningFtBalance;
                else if (workReport == "Post 2002 Report - SN & FT")
                {
                    activeDt = null;
                    DataTable activeDt2 = null;

                    dValue = beginningSmfsBalance;
                    beginningSmfsBalance = beginningTrustBalance;
                    beginningFtBalance = dValue;
                    if (beginningSmfsBalance == 0D)
                        beginningSmfsBalance = loadSecurityNationalBalance(date, cmbPreOrPost.Text.Trim(), ref activeDt);
                    if (beginningFtBalance == 0D)
                        beginningFtBalance = loadForethoughtBalance(date, cmbPreOrPost.Text.Trim(), ref activeDt2);
                }
                else if (workReport == "Post 2002 Report - Unity")
                {
                }
                else if (workReport == "Post 2002 Report - CD")
                {
                    dValue = beginningSmfsBalance;
                    beginningSmfsBalance = beginningTrustBalance;
                    beginningFtBalance = dValue;
                }
            }
            else
            {
                if (!editing)
                {
                    cmd = "Select * from `trust_data_edits` where `date` = '" + lastMonth.ToString("yyyy-MM-dd") + "' AND `status` = 'EndingBalance' AND `trustName` = '" + trustCompany + "' AND `preOrPost` = 'Post' AND `policyStatus` <> 'SPLIT';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                    {
                        if (!useTrustCalculatedBeginningBalance)
                            beginningTrustBalance = dx.Rows[0]["beginningPaymentBalance"].ObjToDouble();
                        if (!useSMFSCalculatedBeginningBalance)
                            beginningSmfsBalance = dx.Rows[0]["beginningDeathBenefit"].ObjToDouble();
                        beginningFtBalance = dx.Rows[0]["endingPaymentBalance"].ObjToDouble();
                        if (workReport == "Post 2002 Report - SN & FT")
                        {
                            dValue = beginningSmfsBalance;
                            beginningSmfsBalance = beginningFtBalance;
                            beginningFtBalance = dValue;
                        }
                        else if (workReport == "Post 2002 Report - Unity")
                        {
                            dValue = beginningSmfsBalance;
                            beginningSmfsBalance = beginningFtBalance;
                            beginningFtBalance = dValue;
                        }
                        else if (workReport == "Post 2002 Report - CD")
                        {
                            dValue = beginningSmfsBalance;
                            beginningSmfsBalance = beginningFtBalance;
                            beginningFtBalance = dValue;
                        }
                    }
                }
            }
            cmd = "Select * from `trust_data_edits` where `date` = '" + date.ToString("yyyy-MM-dd") + "' AND `status` = 'BeginningBalance' AND `trustName` = '" + trustCompany + "' AND `preOrPost` = 'Post' AND `policyStatus` <> 'SPLIT';";
            dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                if (!useTrustCalculatedBeginningBalance)
                    endingTrustBalance = dx.Rows[0]["beginningPaymentBalance"].ObjToDouble();
                if (!useSMFSCalculatedBeginningBalance)
                    endingSmfsBalance = dx.Rows[0]["beginningDeathBenefit"].ObjToDouble();
                endingFtBalance = dx.Rows[0]["endingPaymentBalance"].ObjToDouble();

                if (workReport == "Post 2002 Report - Unity")
                {
                    date = this.dateTimePicker2.Value;
                    if (useTrustCalculatedEndingBalance)
                        endingTrustBalance = loadUnityBalance(date, cmbPreOrPost.Text.Trim(), ref activeDt);
                    dgv4.DataSource = activeDt;
                    //if (useSMFSCalculatedEndingBalance)
                    //    endingSmfsBalance = LoadTBB(date);
                }
                //if (workReport == "Post 2002 Report - FDLIC")
                //{
                //    date = this.dateTimePicker2.Value;
                //    if (useSMFSCalculatedEndingBalance)
                //        endingTrustBalance = loadFDLICBalance(date, cmbPreOrPost.Text.Trim(), ref activeDt);
                //}
            }
            else
            {
                if (workReport == "Post 2002 Report - Unity")
                {
                    date = this.dateTimePicker2.Value;
                    endingTrustBalance = loadUnityBalance(date, cmbPreOrPost.Text.Trim(), ref activeDt);
                    dgv4.DataSource = activeDt;
                    //endingSmfsBalance = LoadTBB(date);
                }
                else if (workReport == "Post 2002 Report - FDLIC")
                {
                    //date = this.dateTimePicker2.Value;
                    //if (useSMFSCalculatedEndingBalance)
                    //    endingTrustBalance = loadFDLICBalance(date, cmbPreOrPost.Text.Trim(), ref activeDt);
                }
                else if (workReport == "Post 2002 Report - SN & FT")
                {
                    activeDt = null;
                    DataTable activeDt2 = null;
                    date = this.dateTimePicker2.Value;
                    if (beginningSmfsBalance == 0D)
                        beginningSmfsBalance = loadSecurityNationalBalance(lastMonth, cmbPreOrPost.Text.Trim(), ref activeDt);
                    if (beginningFtBalance == 0D)
                        beginningFtBalance = loadForethoughtBalance(lastMonth, cmbPreOrPost.Text.Trim(), ref activeDt2);

                    endingTrustBalance = loadSecurityNationalBalance(date, cmbPreOrPost.Text.Trim(), ref activeDt);
                    endingForethoughtBalance = loadForethoughtBalance(date, cmbPreOrPost.Text.Trim(), ref activeDt2);
                    activeDt.Merge(activeDt2);
                    dgv4.DataSource = activeDt;
                }
            }

            cmd = "Select * from `trust_data_edits` where `date` = '" + date.ToString("yyyy-MM-dd") + "' AND `status` = 'BeginningAdjustment' AND `trustName` = '" + trustCompany + "' AND `preOrPost` = 'Post' AND `policyStatus` <> 'SPLIT';";
            dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                beginningTrustAdjustment = dx.Rows[0]["beginningPaymentBalance"].ObjToDouble();
                beginningSmfsAdjustment = dx.Rows[0]["beginningDeathBenefit"].ObjToDouble();
                beginningFtAdjustment = dx.Rows[0]["endingPaymentBalance"].ObjToDouble();
            }

            cmd = "Select * from `trust_data_edits` where `date` = '" + this.dateTimePicker2.Value.ToString("yyyy-MM-dd") + "' AND `status` = 'EndingManualAdjustment' AND `trustName` = '" + trustCompany + "' AND `preOrPost` = 'Post' AND `policyStatus` <> 'SPLIT';";
            dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                endingTrustAdjustment = dx.Rows[0]["beginningPaymentBalance"].ObjToDouble();
                endingSmfsAdjustment = dx.Rows[0]["beginningDeathBenefit"].ObjToDouble();
                endingFtAdjustment = dx.Rows[0]["endingPaymentBalance"].ObjToDouble();
                adjustmentRecord = dx.Rows[0]["record"].ObjToString();
            }

            if (!useSMFSCalculatedEndingBalance)
            {
                date = this.dateTimePicker2.Value;
                cmd = "Select * from `trust_data_edits` where `date` = '" + date.ToString("yyyy-MM-dd") + "' AND `status` = 'EndingBalance' AND `trustName` = '" + trustCompany + "' AND `preOrPost` = 'Post' AND `policyStatus` <> 'SPLIT';";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count <= 0)
                {
                    cmd = "Select * from `trust_data_edits` where `date` = '" + date.ToString("yyyy-MM-dd") + "' AND `status` = 'EndingBalance' AND `trustName` = '" + trustCompany + "' AND `preOrPost` = 'Post' AND `policyStatus` <> 'SPLIT';";
                    dx = G1.get_db_data(cmd);
                }
            }
            else
            {
                cmd = "Select * from `trust_data_edits` where `date` = '" + date.ToString("yyyy-MM-dd") + "' AND `status` = 'EndingBalance' AND `trustName` = '" + trustCompany + "' AND `preOrPost` = 'Post' AND `policyStatus` <> 'SPLIT';";
                dx = G1.get_db_data(cmd);
            }
            if (dx.Rows.Count > 0)
            {
                if (!useTrustCalculatedEndingBalance)
                    lastTrustBalance = dx.Rows[0]["beginningPaymentBalance"].ObjToDouble();
                if (!useSMFSCalculatedEndingBalance)
                    lastSmfsBalance = dx.Rows[0]["beginningDeathBenefit"].ObjToDouble();
                lastFtBalance = dx.Rows[0]["endingPaymentBalance"].ObjToDouble();
                endingBalanceRecord = dx.Rows[0]["record"].ObjToString();
            }

            bool found = false;
            string desc = "";
            string otherdesc = "";
            string status = "";
            bool avoidDesc = false;
            bool avoidOtherDesc = false;
            string cashPaid1 = "";
            string cashPaid2 = "";
            bool gotCashPaid2 = false;
            if (G1.get_column_number(dt, "cashPaid2") >= 0)
                gotCashPaid2 = true;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    avoidDesc = false;
                    avoidOtherDesc = false;
                    cashPaid1 = dt.Rows[i]["cashPaid1"].ObjToString();
                    cashPaid2 = "";
                    if (gotCashPaid2)
                        cashPaid2 = dt.Rows[i]["cashPaid2"].ObjToString();
                    if (cashPaid1.ToUpper() == "DC CASH" || cashPaid1.ToUpper() == "DC PAID")
                    {
                        avoidDesc = true;
                        //continue;
                    }

                    if (cashPaid1.ToUpper() == "BLACK TOTAL" || cashPaid1.ToUpper() == "RED TOTAL")
                        avoidDesc = true;

                    if (cashPaid1.ToUpper() == "PAID CURRENT MONTH" || cashPaid1.ToUpper() == "PAID NEXT MONTH")
                        avoidDesc = true;

                    if (cashPaid2.ToUpper() == "DC CASH" || cashPaid2.ToUpper() == "DC PAID")
                    {
                        avoidOtherDesc = true;
                        //continue;
                    }
                    status = dt.Rows[i]["status"].ObjToString().ToUpper();
                    desc = dt.Rows[i]["desc"].ObjToString();
                    if (desc.ToUpper().IndexOf("PD") == 0 && status != "INCLUDE" )
                        avoidDesc = true;
                    if (desc.ToUpper().IndexOf("TOTAL") == 0)
                        continue;
                    if (G1.get_column_number(dt, "otherdesc") > 0)
                    {
                        otherdesc = dt.Rows[i]["otherdesc"].ObjToString();
                        if (otherdesc.ToUpper().IndexOf("PD") == 0 && status != "INCLUDE" )
                            avoidOtherDesc = true;
                    }
                    month = dt.Rows[i]["month"].ObjToString();
                    if (month.ToUpper() == "BEGINNING BALANCE")
                    {
                        balance = dt.Rows[i]["balance"].ObjToDouble();
                        smfsBalance = dt.Rows[i]["smfsBalance"].ObjToDouble();
                        ftBalance = dt.Rows[i]["ftBalance"].ObjToDouble();
                        continue;
                    }
                    if (month.ToUpper() == "BEGINNING ADJUSTMENT")
                    {
                        //balance += dt.Rows[i]["balance"].ObjToDouble();
                        //smfsBalance += dt.Rows[i]["smfsBalance"].ObjToDouble();
                        //ftBalance += dt.Rows[i]["ftBalance"].ObjToDouble();
                        continue;
                    }
                    else if (month.ToUpper() == "ENDING MANUAL ADJUSTMENT")
                    {
                        adjustmentManualRow = i;
                        continue;
                    }
                    else if (month.ToUpper() == "ENDING CALCULATED ADJUSTMENT")
                    {
                        adjustmentRow = i;
                        continue;
                    }
                    else if (month.ToUpper() == "ENDING BALANCE")
                    {
                        endingRow = i;

                        if (lastTrustBalance != 0D)
                            endingTrustBalance = lastTrustBalance;

                        double workBalance = balance - beginningTrustAdjustment - endingTrustBalance - trustTotal - endingTrustAdjustment;
                        workBalance = G1.RoundValue(workBalance);
                        dt.Rows[i]["balance"] = endingTrustBalance;
                        if (lastTrustBalance != 0D)
                            dt.Rows[i]["balance"] = lastTrustBalance;

                        if (G1.get_column_number(dt, "ftBalance") >= 0)
                            dt.Rows[i]["ftBalance"] = endingForethoughtBalance;

                        workBalance = smfsBalance - smfsTotal;
                        workBalance = G1.RoundValue(workBalance);
                        dt.Rows[i]["smfsBalance"] = endingSmfsBalance - beginningSmfsAdjustment - smfsTotal - endingSmfsAdjustment;
                        if (lastSmfsBalance != 0D)
                        {
                            dt.Rows[i]["smfsBalance"] = lastSmfsBalance;
                            endingSmfsBalance = lastSmfsBalance;
                        }

                        if (adjustmentRow > 0)
                        {
                            workBalance = endingTrustBalance - balance - beginningTrustAdjustment + endingTrustAdjustment + trustTotal;
                            workBalance = G1.RoundValue(workBalance);
                            dt.Rows[adjustmentRow]["balance"] = workBalance;

                            workBalance = endingSmfsBalance - smfsTotal - beginningSmfsAdjustment - smfsBalance - endingSmfsAdjustment;
                            workBalance = endingTrustBalance - endingSmfsBalance;
                            workBalance = G1.RoundValue(workBalance);
                            dt.Rows[adjustmentRow]["smfsBalance"] = workBalance;
                        }

                        gridMain2.RefreshData();
                        gridMain2.RefreshEditor(true);
                        dgv.Refresh();
                        break;
                    }
                    if (trustCompany == "SNFT")
                    {
                        if (!avoidDesc)
                        {
                            str = dt.Rows[i]["trust"].ObjToString();
                            if (str.Trim().ToUpper() == "FORETHOUGHT")
                                ftTotal += dt.Rows[i]["value"].ObjToDouble();
                            else if (str.Trim().ToUpper() == "SECURITY NATIONAL")
                                trustTotal += dt.Rows[i]["value"].ObjToDouble();
                            else if (str.Trim().ToUpper() == "SNFT")
                                trustTotal += dt.Rows[i]["value"].ObjToDouble();
                        }
                        if (G1.get_column_number(dt, "otherTrust") >= 0)
                        {
                            if (!avoidOtherDesc)
                            {
                                str = dt.Rows[i]["otherTrust"].ObjToString();
                                if (str.Trim().ToUpper() == "FORETHOUGHT")
                                    ftTotal += dt.Rows[i]["received"].ObjToDouble();
                                else if (str.Trim().ToUpper() == "SECURITY NATIONAL")
                                    trustTotal += dt.Rows[i]["received"].ObjToDouble();
                                else if (str.Trim().ToUpper() == "SNFT")
                                    ftTotal += dt.Rows[i]["received"].ObjToDouble();
                            }
                        }
                    }
                    else
                    {
                        if (!avoidDesc)
                            trustTotal += dt.Rows[i]["value"].ObjToDouble();
                    }
                    if (workReport == "Post 2002 Report - SN & FT")
                    {
                        if (!avoidOtherDesc)
                            smfsTotal += dt.Rows[i]["received"].ObjToDouble();
                    }
                    else
                        smfsTotal += dt.Rows[i]["received"].ObjToDouble();
                    if (!avoidOtherDesc)
                        principalTotal += dt.Rows[i]["principal"].ObjToDouble();
                }
                catch (Exception ex)
                {
                }
            }
            //if (endingRow < 0)
            if ( 1 == 1)
            {
                try
                {
                    DataRow dRow = null;
                    //DataRow dRow = dt.NewRow();
                    //dRow["month"] = "Ending Manual Adjustment";
                    //dRow["reportDate"] = G1.DTtoMySQLDT(workDate.ToString("MM/dd/yyyy"));
                    //dRow["balance"] = endingTrustAdjustment;
                    //dRow["value"] = endingTrustAdjustment;
                    //dRow["smfsBalance"] = endingSmfsAdjustment;
                    //dRow["ftBalance"] = endingFtAdjustment;
                    //dRow["received"] = endingFtAdjustment;
                    //if (!String.IsNullOrWhiteSpace(adjustmentRecord))
                    //    dRow["record"] = adjustmentRecord;
                    //dt.Rows.Add(dRow);

                    //dRow = dt.NewRow();
                    //dRow["month"] = "Ending Calculated Adjustment";
                    //dRow["reportDate"] = G1.DTtoMySQLDT(workDate.ToString("MM/dd/yyyy"));

                    double newEndingBalance = beginningTrustBalance + trustTotal - beginningTrustAdjustment - endingTrustAdjustment;
                    double newSMFSEndingBalance = beginningSmfsBalance + smfsTotal - beginningSmfsAdjustment - endingSmfsAdjustment;
                    double newFtBalance = beginningFtBalance - smfsTotal - beginningFtAdjustment - endingFtAdjustment;

                    if (lastTrustBalance != 0D)
                        endingTrustBalance = lastTrustBalance;

                    double endingBalance = balance - trustTotal;
                    endingTrustBalance = endingBalance;

                    double workBalance = endingBalance - balance - beginningTrustAdjustment + endingTrustAdjustment + trustTotal;
                    workBalance = G1.RoundValue(workBalance);
                    //dRow["balance"] = workBalance;

                    endingSmfsBalance = smfsBalance - smfsTotal;

                    if (lastSmfsBalance > 0D)
                        endingSmfsBalance = lastSmfsBalance;

                    workBalance = smfsBalance - beginningSmfsAdjustment - endingSmfsBalance - smfsTotal - endingSmfsAdjustment;
                    workBalance = endingSmfsBalance - smfsTotal - beginningSmfsAdjustment - smfsBalance - endingSmfsAdjustment;
                    workBalance = endingTrustBalance - endingSmfsBalance;
                    workBalance = endingSmfsBalance - beginningSmfsBalance - beginningTrustAdjustment + endingTrustAdjustment + smfsTotal;
                    workBalance = G1.RoundValue(workBalance);
                    //dRow["smfsBalance"] = workBalance;


                    if (trustCompany == "SNFT")
                    {
                        workBalance = ftBalance - endingForethoughtBalance - endingFtAdjustment;
                        workBalance = beginningFtBalance;
                        workBalance = workBalance + ftTotal;
                        workBalance = G1.RoundValue(workBalance);
                        //dRow["ftBalance"] = workBalance;
                        //dRow["received"] = workBalance;

                        //dRow["smfsbalance"] = workBalance + dRow["balance"].ObjToDouble();
                    }
                    //else
                    //    dRow["smfsBalance"] = smfsBalance - endingTrustBalance - endingForethoughtBalance;
                    //dt.Rows.Add(dRow);

                    if ( endingRow < 0 )
                        dRow = dt.NewRow();
                    else
                    {
                        int jj = dt.Rows.Count - 1;
                        dRow = dt.Rows[jj];
                    }
                    dRow["month"] = "Ending Balance";
                    dRow["reportDate"] = G1.DTtoMySQLDT(workDate.ToString("MM/dd/yyyy"));
                    if (trustCompany == "SNFT")
                    {
                        endingTrustBalance = balance - trustTotal;
                        dRow["balance"] = balance - trustTotal;
                        if (endingTrustBalance > 0D)
                            dRow["balance"] = endingTrustBalance;
                        if (lastTrustBalance != 0D)
                            dRow["balance"] = lastTrustBalance;
                        dRow["value"] = dRow["balance"].ObjToDouble();

                        //double endingPaymentBalance = endingForethoughtBalance - ftTotal;
                        endingForethoughtBalance = beginningFtBalance + ftTotal;
                        dRow["ftBalance"] = endingForethoughtBalance;
                        dRow["received"] = endingForethoughtBalance;
                        //if (endingPaymentBalance > 0D)
                        //    dRow["ftBalance"] = endingPaymentBalance;

                        if (endingSmfsBalance == 0D)
                            endingSmfsBalance = smfsBalance - smfsTotal - endingSmfsAdjustment;
                        endingSmfsBalance = endingForethoughtBalance + endingTrustBalance - endingSmfsAdjustment;
                        dRow["smfsBalance"] = smfsBalance - smfsTotal - endingSmfsAdjustment;
                        //dRow["smfsBalance"] = smfsBalance - endingTrustBalance - endingForethoughtBalance;
                        if (endingSmfsBalance > 0D)
                            dRow["smfsBalance"] = endingSmfsBalance;

                        if (!String.IsNullOrWhiteSpace(endingBalanceRecord))
                            dRow["record"] = endingBalanceRecord;
                    }
                    else
                    {
                        endingBalance = balance - trustTotal;
                        dRow["balance"] = balance - trustTotal;
                        if (endingTrustBalance > 0D)
                            dRow["balance"] = endingTrustBalance;
                        if (lastTrustBalance != 0D)
                            dRow["balance"] = lastTrustBalance;
                        dRow["value"] = dRow["balance"].ObjToDouble();

                        //double endingPaymentBalance = endingForethoughtBalance - ftTotal;
                        dRow["ftBalance"] = endingForethoughtBalance;
                        dRow["received"] = dRow["ftBalance"].ObjToDouble();
                        //if (endingPaymentBalance > 0D)
                        //    dRow["ftBalance"] = endingPaymentBalance;

                        if (endingSmfsBalance == 0D)
                            endingSmfsBalance = smfsBalance - smfsTotal - endingSmfsAdjustment;
                        dRow["smfsBalance"] = smfsBalance - smfsTotal - endingSmfsAdjustment;
                        //dRow["smfsBalance"] = smfsBalance - endingTrustBalance - endingForethoughtBalance;
                        if (endingSmfsBalance > 0D)
                            dRow["smfsBalance"] = endingSmfsBalance;

                        if (!String.IsNullOrWhiteSpace(endingBalanceRecord))
                            dRow["record"] = endingBalanceRecord;
                    }

                    if (useTrustCalculatedEndingBalance)
                        dRow["balance"] = newEndingBalance;
                    if (useSMFSCalculatedEndingBalance)
                        dRow["smfsBalance"] = newSMFSEndingBalance;
                    if (useSMFSCalculatedEndingBalance)
                        dRow["ftBalance"] = newFtBalance;

                    balance = dRow["balance"].ObjToDouble();
                    endingFtBalance = dRow["ftBalance"].ObjToDouble();
                    endingSmfsBalance = dRow["smfsBalance"].ObjToDouble();

                    dRow["value"] = balance;
                    dRow["received"] = endingSmfsBalance;
                    if (workReport == "Post 2002 Report - SN & FT")
                    {
                        endingFtBalance = beginningFtBalance + smfsTotal;
                        //endingFtBalance = beginningFtBalance + ftTotal;
                        dRow["received"] = endingFtBalance;
                        endingSmfsBalance = beginningTrustBalance + trustTotal;
                        dRow["value"] = endingSmfsBalance;
                    }
                    else if (workReport == "Post 2002 Report - Unity")
                    {
                        endingFtBalance = beginningFtBalance + smfsTotal;
                        dRow["received"] = endingFtBalance;
                    }
                    else if (workReport == "Post 2002 Report - CD")
                    {
                        endingFtBalance = beginningFtBalance + smfsTotal;
                        dRow["received"] = endingFtBalance;
                    }

                    newSMFSEndingBalance = dRow["value"].ObjToDouble() + dRow["received"].ObjToDouble();
                    dRow["smfsBalance"] = newSMFSEndingBalance;

                    if (endingRow < 0)
                        dt.Rows.Add(dRow);
                }
                catch (Exception ex)
                {
                }
            }
            dt = loadBalanceDifferences(dt, "ENDING");
            dt = loadCadenceDeathBenefits(dt, "ENDING");
        }
        /****************************************************************************************/
        private void gridMain_MouseDown(object sender, MouseEventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource; // Leave as a GetDate example

            try
            {
                var hitInfo = gridMain.CalcHitInfo(e.Location);
                if (hitInfo.InRowCell)
                {
                    int rowHandle = hitInfo.RowHandle;
                    gridMain.FocusedRowHandle = rowHandle;
                    gridMain.SelectRow(rowHandle);
                    gridMain.RefreshEditor(true);
                    int row = gridMain.GetDataSourceRowIndex(rowHandle);
                    GridColumn column = hitInfo.Column;
                    gridMain.FocusedColumn = column;
                    string currentColumn = column.FieldName.Trim();
                    if (currentColumn.ToUpper() == "REPORTDATE")
                    {
                        DataRow dr = gridMain.GetFocusedDataRow();
                        DateTime date = dr["reportDate"].ObjToDateTime();
                        if (date.Year <= 1000)
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
                                {
                                    string status = dr["status"].ObjToString();
                                    if ( status.Trim() == "Line Edit")
                                        G1.update_db_table ( "trust_data_edits", "record", record, new string[] { "reportDate", date.ToString("yyyy-MM-dd") });
                                    else
                                        G1.update_db_table("trust_data", "record", record, new string[] { "reportDate", date.ToString("yyyy-MM-dd") });
                                }
                                //DataChanged();
                                gridMain.ClearSelection();
                                gridMain.FocusedRowHandle = rowHandle;

                                gridMain.RefreshData();
                                gridMain.RefreshEditor(true);
                                gridMain.SelectRow(rowHandle);

                                if (date != workDate)
                                {
                                    dt.Rows.Remove(dr);
                                    gridMain.RefreshData();
                                    gridMain.RefreshEditor(true);
                                    //LoadEndingBalances(dt);
                                }
                            }
                            else if (dateForm.DialogResult == System.Windows.Forms.DialogResult.Cancel )
                            {
                                date = dr["reportDate"].ObjToDateTime(); // xyzzy
                                dr["reportDate"] = G1.DTtoMySQLDT (DateTime.MinValue.ToString("MM/dd/yyyy"));
                                if (!String.IsNullOrWhiteSpace(record))
                                {
                                    G1.update_db_table("trust_data", "record", record, new string[] { "reportDate", DateTime.MinValue.ToString("yyyy-MM-dd") });
                                    string mainRecord = dr["record"].ObjToString();
                                    string cmd = "Select * from `trust_data_overruns` WHERE `mainRecord` = '" + mainRecord + "';";
                                    DataTable ddx = G1.get_db_data(cmd);
                                    if ( ddx.Rows.Count > 0 )
                                    {
                                        mainRecord = ddx.Rows[0]["record"].ObjToString();
                                        G1.delete_db_table("trust_data_overruns", "record", mainRecord);
                                    }
                                }
                                ////DataChanged();
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
            catch (Exception ex)
            {
            }
        }
        /****************************************************************************************/
        private void chkGroup_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox check = (CheckBox)sender;
            string groupBy = cmbGroupBy.Text.Trim().ToUpper();
            if (check.Checked)
            {
                if (groupBy == "TRUST")
                    gridMain.Columns["trust"].GroupIndex = 0;
                else
                    gridMain.Columns["iMonth"].GroupIndex = 0;
                gridMain.ExpandAllGroups();
            }
            else
            {
                gridMain.Columns["trust"].GroupIndex = -1;
                gridMain.Columns["iMonth"].GroupIndex = -1;
            }
            gridMain.RefreshData();
            gridMain.RefreshEditor(true);
            dgv.Refresh();
        }
        /****************************************************************************************/
        private void btnEditCustom_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            if (dr == null)
                return;

            DataTable dt = (DataTable)dgv.DataSource;

            string month = "";
            string year = "";
            string monthYear = "";

            DateTime date = this.dateTimePicker2.Value;
            month = date.ToString("MMMM");
            monthYear = month + " " + date.Year.ToString("D4");
            string trustCompany = dr["trust"].ObjToString();

            if (workReport == "Post 2002 Report - SN & FT")
                trustCompany = "SNFT";
            else if (workReport == "Post 2002 Report - Unity")
                trustCompany = "Unity";
            else if (workReport == "Post 2002 Report - FDLIC")
                trustCompany = "FDLIC";

            if (!String.IsNullOrWhiteSpace(trustCompany))
            {
                TrustDataEdit tForm = new TrustDataEdit(trustCompany, monthYear);
                tForm.Show();
            }
        }
        /****************************************************************************************/
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

            cmbSelectColumns.Text = "FDLIC Post Totals";
            cmbSelectColumns.SelectedItem = "FDLIC Post Totals";
        }
        /****************************************************************************************/
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            chkGroup_CheckedChanged(chkGroup, null);
        }
        /****************************************************************************************/
        private void editPolicyToTrustToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            EditPolicyTrusts policyForm = new EditPolicyTrusts();
            policyForm.Show();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private int mainRowIndex = -1;
        private void lookupPreNeedCustomerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable dt = null;
            DataRow dr = null;
            string contractNumber = "";

            if (dgv.Visible)
            {
                dt = (DataTable)dgv.DataSource;

                dr = gridMain.GetFocusedDataRow();
                if (dr == null)
                    return;
                contractNumber = dr["contractNumber"].ObjToString();
                mainRowIndex = gridMain.GetFocusedDataSourceRowIndex();
            }
            else if (dgv2.Visible)
            {
                dt = (DataTable)dgv2.DataSource;

                dr = gridMain2.GetFocusedDataRow();
                if (dr == null)
                    return;
                contractNumber = dr["contract"].ObjToString();
                if (String.IsNullOrWhiteSpace(contractNumber))
                    return;
                mainRowIndex = gridMain2.GetFocusedDataSourceRowIndex();
                mainRowIndex = -1; // Do not allow this for Mismatches
            }
            else if (dgv5.Visible)
            {
                dt = (DataTable)dgv5.DataSource;

                dr = gridMain5.GetFocusedDataRow();
                if (dr == null)
                    return;
                contractNumber = dr["trust"].ObjToString();
                mainRowIndex = gridMain5.GetFocusedDataSourceRowIndex();
                mainRowIndex = -1; // Do not allow this for Mismatches
            }
            else
                return;

            FunLookup fastForm = new FunLookup(contractNumber);
            fastForm.SelectDone += FastForm_SelectDone;
            fastForm.ShowDialog();
        }
        /****************************************************************************************/
        private void FastForm_SelectDone(DataTable s)
        {
            if (s.Rows.Count <= 0)
                return;

            DataRow[] dRows = s.Select("select='1'");
            if (dRows.Length <= 0)
                return;
            s = dRows.CopyToDataTable();

            string contractNumber = s.Rows[0]["contractNumber"].ObjToString();
            string firstname = s.Rows[0]["firstName"].ObjToString();
            string lastName = s.Rows[0]["lastName"].ObjToString();
            if (mainRowIndex >= 0)
            {
                int row = gridMain.GetDataSourceRowIndex(mainRowIndex);
                DataTable dt = (DataTable)dgv.DataSource;
                dt.Rows[row]["contractNumber"] = contractNumber;
                dt.Rows[row]["contract"] = contractNumber;
                dt.Rows[row]["firstName"] = firstname;
                dt.Rows[row]["lastName"] = lastName;
                dt.Rows[row]["desc"] = firstname + " " + lastName;
                DataRow dr = dt.Rows[row];
                dr["contractNumber"] = contractNumber;
                dr["contract"] = contractNumber;
                dr["firstName"] = firstname;
                dr["lastName"] = lastName;
                dr["desc"] = firstname + " " + lastName;
                gridMain.RefreshData();
                gridMain.RefreshEditor(true);
                dgv.Refresh();
            }

        }
        /****************************************************************************************/
        private void monthlyTrustDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DateTime date = this.dateTimePicker2.Value;
            TrustData trustForm = new TrustData(date, chkCmbCompany);
            trustForm.Show();
        }
        /****************************************************************************************/
        private DataTable AddMissing(DataTable dt, DataTable majorDt)
        {
            bool found = false;
            string firstName = "";
            string lastName = "";
            string contractNumber = "";
            string policyNumber = "";
            string trust = "";
            DataRow dRow = null;
            DataRow[] dRows = null;

            majorDt = verifyContracts(majorDt);

            bool gotContract = false;
            if (G1.get_column_number(dt, "contract") >= 0)
                gotContract = true;

            DateTime date = DateTime.Now;
            double deathClaimAmount = 0D;
            double endingDeathBenefit = 0D;

            for (int i = 0; i < majorDt.Rows.Count; i++)
            {
                found = false;
                contractNumber = majorDt.Rows[i]["contractNumber"].ObjToString();
                lastName = majorDt.Rows[i]["lastName"].ObjToString();
                firstName = majorDt.Rows[i]["firstName"].ObjToString();
                policyNumber = majorDt.Rows[i]["policyNumber"].ObjToString();
                trust = majorDt.Rows[i]["trustCompany"].ObjToString();
                date = majorDt.Rows[i]["date"].ObjToDateTime();
                deathClaimAmount = majorDt.Rows[i]["deathClaimAmount"].ObjToDouble();
                endingDeathBenefit = majorDt.Rows[i]["endingDeathBenefit"].ObjToDouble();
                if (String.IsNullOrWhiteSpace(firstName) && String.IsNullOrWhiteSpace(lastName))
                    continue;
                dRows = dt.Select("lastName='" + lastName + "' AND firstName='" + firstName + "'");
                if (dRows.Length > 0)
                    found = true;
                if (!found)
                {
                    dRow = dt.NewRow();
                    dRow["firstName"] = firstName;
                    dRow["lastName"] = lastName;
                    dRow["contractNumber"] = contractNumber;
                    dRow["policyNumber"] = policyNumber;
                    dRow["trustCompany"] = trust;
                    dRow["deathClaimAmount"] = deathClaimAmount;
                    dRow["endingDeathBenefit"] = endingDeathBenefit;
                    dRow["date"] = G1.DTtoMySQLDT(date.ToString("yyyy-MM-dd"));
                    if (gotContract)
                    {
                        dRow["trust"] = trust;
                        dRow["contract"] = contractNumber;
                        dRow["desc"] = firstName + " " + lastName;
                    }
                    dt.Rows.Add(dRow);
                }
            }
            return dt;
        }
        /****************************************************************************************/
        private DataTable ShowMissing(DataTable dx)
        {
            DateTime date1 = this.dateTimePicker1.Value;
            DateTime date2 = this.dateTimePicker2.Value;

            string sDate1 = date1.ToString("yyyy-MM-dd");
            string sDate2 = date2.ToString("yyyy-MM-dd");

            string contractNumber = "";
            string policyNumber = "";
            DataRow[] dRows = null;
            DataRow dRow = null;
            string paidFrom = "";
            string company = "";
            string newCompany = "";

            string companies = getCompanyQuery(workCompanies);

            DateTime dateReceived = DateTime.Now;
            DateTime dateFiled = DateTime.Now;
            DateTime reportDate = DateTime.Now;
            DateTime date = DateTime.Now;

            DateTime maxDate = date2.AddDays(1);
            DateTime minDate = date1.AddDays(-1);
            int previousMonth = minDate.Month;
            date = date2.AddDays(workNextDays);


            string cmd = "Select * from `trust_data` WHERE `deathPaidDate` >= '" + date1.ToString("yyyy-MM-dd") + "' AND `deathPaidDate` <= '" + date2.ToString("yyyy-MM-dd") + " 23:59:59' ";
            if (companies.Contains("Unity"))
            {
                cmd = "SELECT * FROM `contracts` c JOIN `customers` t ON c.`contractNumber` = t.`contractNumber` WHERE (c.`contractNumber` LIKE '%U' OR c.`contractNumber` liKE '%UI') AND t.`deceaseddate` >= '" + sDate1 + "' AND t.`deceaseddate` <= '" + sDate2 + " 23:59:59' ";
                cmd += " ORDER by t.`deceasedDate` ";
                cmd += ";";
            }
            else
            {
                if (!String.IsNullOrWhiteSpace(companies))
                    cmd += " AND " + companies + " ";
                cmd += " ORDER by `deathPaidDate` ";
                cmd += ";";
            }

            DataTable dt = G1.get_db_data(cmd);

            string lastName = "";
            string firstName = "";
            string name = "";
            string funeral = "";
            double amtActuallyReceived = 0D;
            DataTable dd = null;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                    lastName = dt.Rows[i]["lastName"].ObjToString();
                    firstName = dt.Rows[i]["firstName"].ObjToString();
                    name = firstName + " " + lastName;

                    dRows = dx.Select("desc='" + name + "'");
                    if (dRows.Length > 0)
                        continue;

                    cmd = "Select * from `cust_payments` c JOIN `cust_payment_details` t ON t.`paymentRecord` = c.`record` WHERE c.`contractNumber` = '" + contractNumber + "' AND t.`status` = 'DEPOSITED' AND t.`type` = 'TRUST' ;";
                    dd = G1.get_db_data(cmd);
                    if (dd.Rows.Count <= 0)
                        continue;

                    for (int j = 0; j < dd.Rows.Count; j++)
                    {
                        paidFrom = dd.Rows[j]["paidFrom"].ObjToString();
                        amtActuallyReceived = dd.Rows[j]["amtActuallyReceived"].ObjToDouble();
                        dateReceived = dd.Rows[j]["dateReceived"].ObjToDateTime();
                        funeral = getFuneralService(contractNumber);
                        if (!String.IsNullOrWhiteSpace(funeral))
                        {
                            dRows = dx.Select("funeral='" + funeral + "'");
                            if (dRows.Length <= 0)
                            {
                                dRow = dx.NewRow();
                                dRow["contract"] = contractNumber;
                                dRow["desc"] = name + " / " + paidFrom;
                                dRow["date"] = G1.DTtoMySQLDT(date2);
                                dRow["funeral"] = funeral;
                                dRow["received"] = amtActuallyReceived;
                                dRow["dateReceived"] = G1.DTtoMySQLDT(dateReceived.ToString("yyyy-MM-dd"));
                                dx.Rows.Add(dRow);
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                }
            }

            gridMain.Columns["policyNumber"].Visible = true;

            return dx;
        }
        /****************************************************************************************/
        private void post2002ReportSNFTToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menu = (ToolStripMenuItem)sender;

            workReport = menu.Text;

            chkOldStuff.Checked = false;
            cmbPreOrPost.Text = "Post";
            chkJustTrustsSelected.Checked = true;

            chkCmbCompany.DeselectAll();

            chkCmbCompany.SetEditValue("Security National|FORETHOUGHT");
            //workReport = "Post 2002 Report - Forethought";
            //chkCmbCompany.SetEditValue("FORETHOUGHT");

            chkCmbCompany.Refresh();

            cmbSelectColumns.Text = "SN/FT Post Totals";
            cmbSelectColumns.SelectedItem = "SN/FT Post Totals";

            //cmbSelectColumns.Text = "Forethough Post Totals";
            //cmbSelectColumns.SelectedItem = "Forethought Post Totals";
        }
        /****************************************************************************************/
        private void post2002ReportCadenceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menu = (ToolStripMenuItem)sender;

            workReport = menu.Text;

            chkOldStuff.Checked = false;
            cmbPreOrPost.Text = "Post";
            chkJustTrustsSelected.Checked = true;

            chkCmbCompany.DeselectAll();

            chkCmbCompany.SetEditValue("CD");

            chkCmbCompany.Refresh();

            cmbSelectColumns.Text = "CD Post Totals";
            cmbSelectColumns.SelectedItem = "CD Post Totals";
        }
        /****************************************************************************************/
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string oldWorkReport = workReport;
            TabControl tabControl = (TabControl)sender;
            int selectedIndex = tabControl.SelectedIndex;
            string pageName = tabControl.TabPages[selectedIndex].Name.Trim();
            if (pageName.ToUpper() == "TABPAGE1")
            {
                if (originalDt != null)
                {
                    DataTable ddd = (DataTable)dgv.DataSource;
                    gridMain.RefreshEditor(true);
                    gridMain.RefreshData();

                    //ddd = originalDt.Copy();
                    if ( G1.get_column_number ( ddd, "mine") < 0 )
                        ddd.Columns.Add("mine", Type.GetType("System.Double"));
                    for (int i = 0; i < ddd.Rows.Count; i++)
                        ddd.Rows[i]["mine"] = ddd.Rows[i]["num"].ObjToDouble();

                    DataView tempview = ddd.DefaultView;
                    tempview.Sort = "mine";
                    ddd = tempview.ToTable();
                    ddd.Columns.Remove("mine");
                    dgv.DataSource = ddd;


                    dgv.RefreshDataSource();
                    dgv.Refresh();

                    btnSave.Hide();
                    btnSave.Refresh();

                }
                return;
            }
            if (pageName.ToUpper() == "TABPAGE4")
                return;
            if (pageName.ToUpper() == "TABPAGE6")
                return;
            if (pageName.ToUpper() == "TABPAGE7")
                return;
            if (pageName.ToUpper() == "TABPAGE5")
            {
                originalDt = (DataTable)dgv.DataSource;
                DataTable ddd = (DataTable)dgv5.DataSource;
                gridMain5.Columns["Sandra Money"].Caption = "Sandra Money";
                gridMain5.Columns["Sandra Principal"].Caption = "Sandra Principal";
                if (workReport.IndexOf("Post 2002 Report - SN") == 0)
                {
                    gridMain5.Columns["Sandra Money"].Caption = "Sandra SN Trust Money";
                    gridMain5.Columns["Sandra Principal"].Caption = "Sandra FT Trust Money";
                }
                btnSave.Hide();
                btnSave.Refresh();
                return;
            }
            if (pageName.ToUpper() == "TABPAGE3")
            {
                DetermineDifference();
                return;
            }
            if (pageName.ToUpper() == "TABPAGE9")
            {
                originalDt = (DataTable)dgv.DataSource;
                return;
            }

            if (workReport == "Pre 2002 Report")
            {
                dgv6.Show();
                LoadSplitPre();
                dgv6.Visible = true;
                dgv6.Refresh();
            }
            else
            {
                originalDt = (DataTable)dgv.DataSource;

                SetupTab(tabPage9, true);
                SetupTab(tabPage11, false);
                SetupTab(tabPage12, false);
                SetupTab(tabPage13, false);
                SetupTab(tabPage14, false);
                SetupTab(tabPage15, false);
                SetupTab(tabPage16, false);

                if (workReport == "Post 2002 Report - SN & FT")
                {
                    //workReport = "Post 2002 Report - Forethought";
                    //DataRow[] dRows = originalDt.Select("trust='Forethought'");
                    //if (dRows.Length > 0)
                    //{
                    //    DataTable tempDt = dRows.CopyToDataTable();
                    //    LoadSplit(tempDt);

                    //    DataTable dx = (DataTable)dgv2.DataSource;

                    //    workReport = oldWorkReport;

                    //    LoadSplit();

                    //    DataTable dd = (DataTable)dgv2.DataSource;
                    //}
                    LoadSplit();
                }
                else
                    LoadSplit();
                dgv6.Visible = false;
                dgv2.Visible = true;
                dgv2.Refresh();

                workReport = oldWorkReport;
            }

            //DataTable dt = (DataTable)dgv.DataSource;

            //string trust = "trust";
            //if (G1.get_column_number(dt, "trust") < 0)
            //    trust = "trust";

            //DataView tempview = dt.DefaultView;
            //tempview.Sort = trust + " desc";
            //DataTable dd = tempview.ToTable();

            //DataTable dt1 = dt.DefaultView.ToTable(true, trust );

            //string trustCompany = "";
            //DataRow[] dRows = null;
            //DataTable[] mainDts = new DataTable[2];
            //int dtCount = 0;
            //for ( int i=0; i<dt1.Rows.Count; i++)
            //{
            //    trustCompany = dt1.Rows[i][trust].ObjToString();
            //    dRows = dt.Select("trust='" + trustCompany + "'");
            //    if ( dRows.Length > 0 )
            //    {
            //        mainDts[dtCount] = dRows.CopyToDataTable();
            //        dtCount++;
            //    }
            //}

            //DataTable tempDt = null;
            //int maxRow = 0;
            //for (int j = 0; j < dtCount; j++)
            //{
            //    tempDt = mainDts[j];
            //    if (tempDt == null)
            //        continue;
            //    if ( tempDt.Rows.Count > maxRow )
            //        maxRow = tempDt.Rows.Count;
            //}

            //DataRow dRow = null;
            //DataTable dx = CreateTempDt();

            //dx.Columns.Add("otherdesc");
            //dx.Columns.Add("otherContract");
            //dx.Columns.Add("otherFuneral");
            //dx.Columns.Add("othertrust");

            //int firstRow = 1;
            //for (int i = 0; i < maxRow; i++)
            //{
            //    dRow = dx.NewRow();
            //    dx.Rows.Add(dRow);
            //    if (firstRow < 0)
            //        firstRow = dx.Rows.Count - 1;
            //}

            //dd = null;
            //for (int i = 0; i < dtCount; i++)
            //{
            //    trustCompany = dt1.Rows[i][trust].ObjToString();
            //    tempDt = CreateTempDt();
            //    tempDt.TableName = trustCompany;
            //    dd = mainDts[i].Copy();
            //    for ( int j=0; j<dd.Rows.Count; j++)
            //    {
            //        if (i == 0)
            //        {
            //            dx.Rows[j]["date"] = G1.DTtoMySQLDT (dd.Rows[j]["date"].ObjToDateTime().ToString("yyyy-MM-dd"));
            //            dx.Rows[j]["trust"] = trustCompany;
            //            gridMain2.Columns["value"].Caption = trustCompany + " Trust Money";
            //            dx.Rows[j]["value"] = dd.Rows[j]["value"].ObjToDouble();
            //            dx.Rows[j]["funeral"] = dd.Rows[j]["funeral"].ObjToString();
            //            dx.Rows[j]["contract"] = dd.Rows[j]["contract"].ObjToString();
            //            dx.Rows[j]["desc"] = dd.Rows[j]["desc"].ObjToString();
            //        }
            //        else
            //        {
            //            dx.Rows[j]["dateReceived"] = G1.DTtoMySQLDT(dd.Rows[j]["date"].ObjToDateTime().ToString("yyyy-MM-dd"));
            //            dx.Rows[j]["othertrust"] = trustCompany;
            //            gridMain2.Columns["received"].Caption = trustCompany + " Trust Money";
            //            dx.Rows[j]["received"] = dd.Rows[j]["value"].ObjToDouble();
            //            dx.Rows[j]["otherfuneral"] = dd.Rows[j]["funeral"].ObjToString();
            //            dx.Rows[j]["othercontract"] = dd.Rows[j]["contract"].ObjToString();
            //            dx.Rows[j]["otherdesc"] = dd.Rows[j]["desc"].ObjToString();
            //        }
            //    }
            //}

            //LoadBeginningBalances(dx);
            //LoadEndingBalances(dx);

            //G1.NumberDataTable(dx);
            //dgv2.DataSource = dx;
            //dgv2.Refresh();
        }
        /****************************************************************************************/
        private void LoadSplit(DataTable xDt = null )
        {
            this.Cursor = Cursors.WaitCursor;
            G1.SetupToolTip(btnDelete, "Delete Row");
            G1.SetupToolTip(btnInsert, "Insert Row");

            DataTable dt = (DataTable)dgv.DataSource;
            if (dt == null)
                return;

            if (xDt != null)
                dt = xDt;

            string trust = "trust";
            if (G1.get_column_number(dt, "trust") < 0)
                trust = "trust";

            DateTime dateReceived = DateTime.Now;
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                dateReceived = dt.Rows[i]["dateReceived"].ObjToDateTime();
                if (dateReceived > this.dateTimePicker2.Value)
                    dt.Rows[i]["billingReason"] = "XX";
            }

            DataView tempview = dt.DefaultView;
            tempview.Sort = trust + " desc";
            DataTable dd = tempview.ToTable();

            //DataTable dt1 = dt.DefaultView.ToTable(true, trust);

            DataTable dt1 = BuildTrustList(dd);

            string status = "";

            DataTable saveDt1 = dt1.Copy();

            string trustCompany = "";
            if (workReport == "Post 2002 Report - Unity")
                trustCompany = "Unity";
            else if (workReport == "Post 2002 Report - FDLIC")
                trustCompany = "FDLIC";
            else if (workReport == "Post 2002 Report - Forethought")
                trustCompany = "Forethought";
            else if (workReport == "Post 2002 Report - CD")
            {
                trustCompany = "CD";
                if ( dt1.Rows.Count <= 0 )
                {
                    DataRow ddRow = dt1.NewRow();
                    ddRow["trust"] = "CD";
                    dt1.Rows.Add(ddRow);
                    saveDt1 = dt1.Copy();
                }
            }

            int dtCount = 0;
            DataRow[] dRows = null;
            DataTable[] mainDts = new DataTable[7];
            if (workReport != "Post 2002 Report - SN & FT")
            {
                dt1 = dt.Copy();
                mainDts[0] = dt1.Copy();
                dtCount = 1;
            }
            else
            {
                for (int i = 0; i < dt1.Rows.Count; i++)
                {
                    trustCompany = dt1.Rows[i][trust].ObjToString();
                    dt.CaseSensitive = false;
                    dRows = dt.Select("trust = '" + trustCompany + "'");
                    if (dRows.Length > 0)
                    {
                        mainDts[dtCount] = dRows.CopyToDataTable();
                        dtCount++;
                    }
                }
            }

            int replaceRows = 0;
            dRows = dt.Select("middleName='REPLACE'");
            if (dRows.Length > 0)
                replaceRows = dRows.Length;

            DataTable tempDt = null;
            int maxRow = 0;
            for (int j = 0; j < dtCount; j++)
            {
                tempDt = mainDts[j];
                if (tempDt == null)
                    continue;
                if (tempDt.Rows.Count > maxRow)
                    maxRow = tempDt.Rows.Count;
            }

            maxRow = maxRow - replaceRows;

            DataRow dRow = null;
            DataTable dx = CreateTempDt();
            if (G1.get_column_number(dx, "status") < 0)
                dx.Columns.Add("status");

            if (workReport == "Post 2002 Report - SN & FT")
            {
                dx.Columns.Add("otherdesc");
                dx.Columns.Add("otherContract");
                dx.Columns.Add("otherFuneral");
                dx.Columns.Add("othertrust");

                gridMain2.Columns["otherdesc"].Visible = true;
                gridMain2.Columns["otherContract"].Visible = true;
                gridMain2.Columns["otherFuneral"].Visible = true;
                gridMain2.Columns["dateReceived"].Visible = true;
            }
            else if (workReport == "Post 2002 Report - FDLIC")
            {
                dx.Columns.Add("otherdesc");
                dx.Columns.Add("otherContract");
                dx.Columns.Add("otherFuneral");
                dx.Columns.Add("othertrust");

                gridMain2.Columns["otherdesc"].Visible = true;
                gridMain2.Columns["otherContract"].Visible = true;
                gridMain2.Columns["otherFuneral"].Visible = true;
                gridMain2.Columns["dateReceived"].Visible = true;

                gridMain2.Columns["smfsBalance"].Visible = false;

                gridMain2.Columns["diff"].Visible = true;
                gridMain2.Columns["cadenceDeathBenefits"].Visible = true;
            }
            else if (workReport == "Post 2002 Report - Unity")
            {
                dx.Columns.Add("otherdesc");
                dx.Columns.Add("otherContract");
                dx.Columns.Add("otherFuneral");
                dx.Columns.Add("othertrust");

                gridMain2.Columns["otherdesc"].Visible = true;
                gridMain2.Columns["otherContract"].Visible = true;
                gridMain2.Columns["otherFuneral"].Visible = true;
                gridMain2.Columns["dateReceived"].Visible = true;

                gridMain2.Columns["smfsBalance"].Visible = false;

                gridMain2.Columns["diff"].Visible = true;
                gridMain2.Columns["cadenceDeathBenefits"].Visible = true;
            }
            else if (workReport == "Post 2002 Report - CD")
            {
                dx.Columns.Add("otherdesc");
                dx.Columns.Add("otherContract");
                dx.Columns.Add("otherFuneral");
                dx.Columns.Add("othertrust");

                gridMain2.Columns["otherdesc"].Visible = true;
                gridMain2.Columns["otherContract"].Visible = true;
                gridMain2.Columns["otherFuneral"].Visible = true;
                gridMain2.Columns["dateReceived"].Visible = true;

                gridMain2.Columns["smfsBalance"].Visible = false;
            }
            else
            {
                gridMain2.Columns["otherdesc"].Visible = false;
                gridMain2.Columns["otherContract"].Visible = false;
                gridMain2.Columns["otherFuneral"].Visible = false;
                gridMain2.Columns["dateReceived"].Visible = false;
            }

            if (G1.get_column_number(dx, "rp") < 0)
                dx.Columns.Add("rp");

            int firstRow = 1;
            for (int i = 0; i < maxRow; i++)
            {
                dRow = dx.NewRow();
                dx.Rows.Add(dRow);
                if (firstRow < 0)
                    firstRow = dx.Rows.Count - 1;
            }

            dd = null;
            string fName = "";
            string mName = "";
            string lName = "";
            string name = "";
            string contract = "";
            string manual = "";
            double principal = 0D;
            double received = 0D;
            string policyStatus = "";
            string rp = "";
            string[] Lines = null;
            DateTime receivedDate = DateTime.Now;
            for (int i = 0; i < dtCount; i++)
            {
                if (i >= dt1.Rows.Count)
                    break;
                trustCompany = dt1.Rows[i][trust].ObjToString();
                tempDt = CreateTempDt();
                tempDt.TableName = trustCompany;
                dd = mainDts[i].Copy();

                dRows = dd.Select("middleName='REPLACE'");
                if (dRows.Length > 0)
                {
                    for (int kk = 0; kk < dRows.Length; kk++)
                        dd.Rows.Remove(dRows[kk]);
                }

                for (int j = 0; j < dd.Rows.Count; j++)
                {
                    contract = dd.Rows[j]["contract"].ObjToString();
                    if ( contract == "P15099UI")
                    {
                    }
                    rp = dd.Rows[j]["billingReason"].ObjToString().ToUpper();
                    if (rp == "RP")
                        dd.Rows[j]["imonth"] = 2;
                    status = dd.Rows[j]["status"].ObjToString();
                    if (status == "Line Edit")
                        dd.Rows[j]["status"] = "Include";
                    fName = dd.Rows[j]["firstName"].ObjToString();
                    mName = dd.Rows[j]["middleName"].ObjToString();
                    lName = dd.Rows[j]["lastName"].ObjToString();
                    if (mName.Length > 0)
                        mName = mName.Substring(0, 1);
                    name = fName + " " + mName + " " + lName;
                    if (status == "Line Edit")
                        name = dd.Rows[j]["desc"].ObjToString();
                    if ( String.IsNullOrWhiteSpace ( name ))
                    {
                        name = dd.Rows[j]["insuredName"].ObjToString();
                        name = name.Replace("*", "");
                        if ( name.IndexOf ( ",") > 0 )
                        {
                            Lines = name.Split(',');
                            if ( Lines.Length > 1 )
                                name = Lines[1] + " " + Lines[0];
                        }
                    }
                    if (i == 0 && trustCompany != "Forethought" )
                    {
                        dx.Rows[j]["date"] = G1.DTtoMySQLDT(dd.Rows[j]["date"].ObjToDateTime().ToString("yyyy-MM-dd"));
                        receivedDate = dd.Rows[j]["dateReceived"].ObjToDateTime();
                        if ( receivedDate.Year > 1000 )
                            dx.Rows[j]["date"] = G1.DTtoMySQLDT(dd.Rows[j]["dateReceived"].ObjToDateTime().ToString("yyyy-MM-dd"));
                        dx.Rows[j]["trust"] = trustCompany;
                        gridMain2.Columns["value"].Caption = trustCompany + " Trust Money";
                        gridMain2.Columns["balance"].Caption = trustCompany + " Trust Balance";
                        dx.Rows[j]["value"] = dd.Rows[j]["value"].ObjToDouble();
                        dx.Rows[j]["fun_amtReceived"] = dd.Rows[j]["fun_AmtReceived"].ObjToDouble();
                        dx.Rows[j]["principal"] = dd.Rows[j]["principal"].ObjToDouble();

                        dx.Rows[j]["rp"] = rp;

                        if (workReport == "Post 2002 Report - Unity")
                            dx.Rows[j]["received"] = dd.Rows[j]["principal"].ObjToDouble();
                        else if (workReport == "Post 2002 Report - FDLIC")
                        {
                            policyStatus = dd.Rows[j]["policyStatus"].ObjToString().ToUpper();
                            principal = dd.Rows[j]["principal"].ObjToDouble();
                            received = dd.Rows[j]["received"].ObjToDouble();
                            if (policyStatus == "SPLIT" && received != 0D)
                                principal = received;
                            dx.Rows[j]["received"] = principal;
                        }

                        dx.Rows[j]["funeral"] = dd.Rows[j]["funeral"].ObjToString();
                        dx.Rows[j]["contract"] = dd.Rows[j]["contract"].ObjToString();
                        dx.Rows[j]["desc"] = dd.Rows[j]["desc"].ObjToString();
                        if ( !String.IsNullOrWhiteSpace ( name ))
                            dx.Rows[j]["desc"] = name;
                        dx.Rows[j]["status"] = dd.Rows[j]["status"].ObjToString();
                        dx.Rows[j]["funeral"] = dd.Rows[j]["funeral"].ObjToString();
                        dx.Rows[j]["record"] = dd.Rows[j]["record"].ObjToInt32();
                        //dx.Rows[j]["manual"] = dd.Rows[j]["manual"].ObjToString();
                    }
                    else
                    {
                        dx.Rows[j]["dateReceived"] = G1.DTtoMySQLDT(dd.Rows[j]["dateReceived"].ObjToDateTime().ToString("yyyy-MM-dd"));
                        dx.Rows[j]["othertrust"] = trustCompany;
                        gridMain2.Columns["received"].Caption = trustCompany + " Trust Money";
                        gridMain2.Columns["ftBalance"].Caption = trustCompany + " Trust Balance";
                        dx.Rows[j]["received"] = dd.Rows[j]["value"].ObjToDouble();
                        dx.Rows[j]["otherfuneral"] = dd.Rows[j]["funeral"].ObjToString();
                        dx.Rows[j]["othercontract"] = dd.Rows[j]["contract"].ObjToString();
                        dx.Rows[j]["otherdesc"] = dd.Rows[j]["desc"].ObjToString();
                        dx.Rows[j]["status"] = dd.Rows[j]["status"].ObjToString();
                        //dx.Rows[j]["funeral"] = dd.Rows[j]["funeral"].ObjToString();
                        dx.Rows[j]["record"] = dd.Rows[j]["record"].ObjToInt32();
                        //dx.Rows[j]["manual"] = dd.Rows[j]["manual"].ObjToString();
                    }
                }
            }

            dRows = dx.Select("status='Main Line Edit'");
            if (dRows.Length > 0)
            {
                tempDt = dx.Clone();
                for ( int i=(dx.Rows.Count-1); i>=0; i-- )
                {
                    status = dx.Rows[i]["status"].ObjToString();
                    if (status == "Main Line Edit")
                    {
                        tempDt.ImportRow(dx.Rows[i]);
                        dx.Rows.RemoveAt(i);
                    }
                }

                tempview = tempDt.DefaultView;
                tempview.Sort = "dateReceived";
                tempDt = tempview.ToTable();

                for (int j = (tempDt.Rows.Count-1); j >= 0; j--)
                    dx.ImportRow(tempDt.Rows[j]);
            }


            for ( int i=0; i<dx.Rows.Count; i++)
            {
                dx.Rows[i]["value"] = dx.Rows[i]["value"].ObjToDouble() * -1D;
                dx.Rows[i]["received"] = dx.Rows[i]["received"].ObjToDouble() * -1D;
            }

            if (workReport == "Post 2002 Report - FDLIC")
            {
            }


            gridMain2.Columns["balance"].Visible = false;
            gridMain2.Columns["ftBalance"].Visible = false;

            tempview = dx.DefaultView;
            tempview.Sort = "rp, date";
            dx = tempview.ToTable();

            if (workReport == "Post 2002 Report - SN & FT")
            {
                DataTable ddx = dx.Copy();

                tempview = ddx.DefaultView;
                tempview.Sort = "dateReceived";
                ddx = tempview.ToTable();

                for ( int i=0; i<ddx.Rows.Count; i++)
                {
                    dx.Rows[i]["otherContract"] = ddx.Rows[i]["otherContract"].ObjToString();
                    dx.Rows[i]["otherdesc"] = ddx.Rows[i]["otherdesc"].ObjToString();
                    dx.Rows[i]["otherFuneral"] = ddx.Rows[i]["otherFuneral"].ObjToString();
                    dx.Rows[i]["dateReceived"] = G1.DTtoMySQLDT(ddx.Rows[i]["dateReceived"].ObjToDateTime().ToString("yyyy-MM-dd"));
                    dx.Rows[i]["received"] = ddx.Rows[i]["received"].ObjToDouble();
                    dx.Rows[i]["othertrust"] = ddx.Rows[i]["othertrust"].ObjToString();
                }
            }


            LoadBeginningBalances(dx);

            string cmd = "";
            DateTime date = this.dateTimePicker2.Value;

            //for (int j = 0; j < saveDt1.Rows.Count; j++)
            //{
            //    trust = saveDt1.Rows[j]["trust"].ObjToString();
            //    if (trust == "Security National")
            //        trust = "SNFT";
            //    cmd = "Select * from `trust_data_edits` WHERE `trustName` = '" + trust + "' AND `status` = 'Line Edit' AND `date` = '" + date.ToString("yyyy-MM-dd") + "' AND `preOrPost` = 'Post';";
            //    dd = G1.get_db_data(cmd);
            //    if (dd.Rows.Count > 0)
            //        dx = LoadLineEdits(dx, dd);
            //}

            if (workReport == "Post 2002 Report - SN & FT")
            {
                loadCashPaidSNFT(dx);
            }
            else if (workReport == "Post 2002 Report - CD")
            {
                loadCashPaidCD(dx);
            }
            else
                loadCashPaid(dx);

            for ( int i=0; i<dx.Rows.Count; i++)
            {
                rp = dx.Rows[i]["rp"].ObjToString().ToUpper();
                if (rp == "RP")
                    dx.Rows[i]["red1"] = "Y";
            }

            bool gotSNFT = false;

            for (int j = 0; j < saveDt1.Rows.Count; j++)
            {
                trust = saveDt1.Rows[j]["trust"].ObjToString();
                if (trust == "Security National")
                    trust = "SNFT";
                if (workReport == "Post 2002 Report - SN & FT")
                    trust = "SNFT";
                if (!gotSNFT)
                {
                    cmd = "Select * from `trust_data_edits` WHERE `trustName` = '" + trust + "' AND `status` = 'Line Edit' AND `date` = '" + date.ToString("yyyy-MM-dd") + "' AND `preOrPost` = 'Post' AND `policyStatus` <> 'SPLIT';";
                    dd = G1.get_db_data(cmd);
                    if (dd.Rows.Count > 0)
                        dx = LoadLineEdits(dx, dd);
                }
                if (trust == "SNFT")
                    gotSNFT = true;
            }

            LoadEndingBalances(dx);

            string month = this.dateTimePicker2.Value.ToString("MMMM");
            if (dx.Rows.Count >= 1) // xyzzy
            {
                dx.Rows[0]["month"] = month + " " + this.dateTimePicker2.Value.Year.ToString("D4") + " Beg Bal";
                //if (dx.Rows.Count >= 2)
                //    dx.Rows[1]["month"] = "Beginning";
                //if ( dx.Rows.Count >= 3 )
                //    dx.Rows[2]["month"] = "Balance";
            }

            SetupPostPositions();

            btnSave.Show();
            btnSave.Refresh();

            gridMain2.OptionsClipboard.AllowCopy = DevExpress.Utils.DefaultBoolean.True;
            gridMain2.OptionsClipboard.ClipboardMode = DevExpress.Export.ClipboardMode.Formatted;

            G1.NumberDataTable(dx);
            dgv2.DataSource = dx;
            dgv2.Refresh();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void SetupPostPositions ()
        {

            int i = 1;
            if (workReport == "Post 2002 Report - SN & FT")
            {
            }
            else if (workReport == "Post 2002 Report - CD")
            {
            }
            else if (workReport == "Post 2002 Report - Unity")
            {
                ClearAllPositions(gridMain2);
                
                G1.SetColumnPosition(gridMain2, "num", i++);
                G1.SetColumnPosition(gridMain2, "month", i++);
                //G1.SetColumnPosition(gridMain2, "sn1", i++);
                //G1.SetColumnPosition(gridMain2, "junk1", i++);
                G1.SetColumnPosition(gridMain2, "value", i++);
                G1.SetColumnPosition(gridMain2, "cashPaid1", i++);
                G1.SetColumnPosition(gridMain2, "desc", i++);
                G1.SetColumnPosition(gridMain2, "date", i++);
                G1.SetColumnPosition(gridMain2, "contract", i++);
                G1.SetColumnPosition(gridMain2, "funeral", i++);

                G1.SetColumnPosition(gridMain2, "sn2", i++);
                G1.SetColumnPosition(gridMain2, "otherdesc", i++);
                G1.SetColumnPosition(gridMain2, "received", i++);
                G1.SetColumnPosition(gridMain2, "otherFuneral", i++);
                G1.SetColumnPosition(gridMain2, "diff", i++);
                G1.SetColumnPosition(gridMain2, "cadenceDeathBenefits", i++);
                //G1.SetColumnPosition(gridMain2, "cashPaid2", i++);
                //G1.SetColumnPosition(gridMain2, "dateReceived", i++);
                //G1.SetColumnPosition(gridMain2, "otherContract", i++);
            }
            else if (workReport == "Post 2002 Report - FDLIC")
            {
                ClearAllPositions(gridMain2);

                G1.SetColumnPosition(gridMain2, "num", i++);
                G1.SetColumnPosition(gridMain2, "month", i++);
                G1.SetColumnPosition(gridMain2, "value", i++);
                G1.SetColumnPosition(gridMain2, "cashPaid1", i++);
                G1.SetColumnPosition(gridMain2, "desc", i++);
                G1.SetColumnPosition(gridMain2, "date", i++);
                G1.SetColumnPosition(gridMain2, "contract", i++);
                G1.SetColumnPosition(gridMain2, "funeral", i++);

                G1.SetColumnPosition(gridMain2, "sn1", i++);
                G1.SetColumnPosition(gridMain2, "sn2", i++);
                G1.SetColumnPosition(gridMain2, "junk1", i++);
                G1.SetColumnPosition(gridMain2, "otherdesc", i++);
                G1.SetColumnPosition(gridMain2, "received", i++);
                G1.SetColumnPosition(gridMain2, "junk2", i++);
                G1.SetColumnPosition(gridMain2, "otherFuneral", i++);
                G1.SetColumnPosition(gridMain2, "diff", i++);
                G1.SetColumnPosition(gridMain2, "cadenceDeathBenefits", i++);
                //G1.SetColumnPosition(gridMain2, "cashPaid2", i++);
                //G1.SetColumnPosition(gridMain2, "dateReceived", i++);
                //G1.SetColumnPosition(gridMain2, "otherContract", i++);
            }
        }
        /****************************************************************************************/
        private DataTable loadCashPaidSNFT(DataTable dt)
        {
            int firstRow = -1;
            int lastRow = -1;
            string contractNumber = "";
            string contractNumber2 = "";
            DateTime date = DateTime.Now;
            DataRow dRow = null;
            double dValue = 0D;
            int i = 0;
            string month = "";

            dt.Columns.Add("cashPaid1");
            dt.Columns.Add("red1");
            dt.Columns.Add("cashPaid2");
            dt.Columns.Add("red2");

            string otherTrust = "";
            string trustCompany = "";

            for ( i = 0; i < dt.Rows.Count; i++)
            {
                month = dt.Rows[i]["month"].ObjToString();
                if (month.ToUpper().IndexOf("BALANCE") > 0)
                    continue;
                contractNumber = dt.Rows[i]["contract"].ObjToString();
                if (!String.IsNullOrWhiteSpace(contractNumber) && firstRow < 0)
                    firstRow = i;
                if (!String.IsNullOrWhiteSpace(contractNumber))
                    lastRow = i;
                if (firstRow < 0)
                {
                    contractNumber = dt.Rows[i]["otherContract"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(contractNumber) && firstRow < 0)
                        firstRow = i;
                }
                otherTrust = dt.Rows[i]["otherTrust"].ObjToString().ToUpper();
                if ( otherTrust == "FORETHOUGHT")
                {
                    date = dt.Rows[i]["dateReceived"].ObjToDateTime();
                    if (date.Year < 1000)
                    {
                        otherTrust = dt.Rows[i]["otherFuneral"].ObjToString();
                        if (otherTrust.ToUpper().IndexOf("O/S") < 0 && otherTrust.ToUpper().IndexOf("OS") < 0)
                        {
                            dt.Rows[i]["received"] = DBNull.Value;
                            dt.Rows[i]["otherdesc"] = "";
                            dt.Rows[i]["otherContract"] = "";
                            dt.Rows[i]["otherFuneral"] = "";
                        }
                    }
                }
                else
                {
                    date = dt.Rows[i]["date"].ObjToDateTime();
                    if (date.Year < 1000)
                    {
                        dt.Rows[i]["received"] = DBNull.Value;
                    }
                }
            }

            for ( i=0; i<dt.Rows.Count; i++)
            {
                dValue = dt.Rows[i]["received"].ObjToDouble();
                if (dValue == 0D)
                    dt.Rows[i]["received"] = DBNull.Value;
            }

            DateTime startDate = this.dateTimePicker1.Value;
            DateTime stopDate = this.dateTimePicker2.Value;
            DateTime newStopDate = stopDate.AddDays(workNextDays);
            string sDate1 = startDate.ToString("yyyy-MM-dd");
            string sDate2 = stopDate.ToString("yyyy-MM-dd");

            string companies = getCompanyQuery(workCompanies);

            string cmd = "Select * from `trust_data_overruns` t WHERE `date` >= '" + sDate1 + "' AND `date` <= '" + sDate2 + " 23:59:59' ";

            if (workReport == "Pre 2002 Report")
                cmd += " AND t.`preOrPost` = 'Pre' ";
            else
                cmd += " AND t.`preOrPost` <> 'Pre' ";

            if (!String.IsNullOrWhiteSpace(companies))
            {
                string newCompany = companies;
                if (newCompany.IndexOf("FDLIC PB") >= 0)
                    newCompany = newCompany.Replace("'FDLIC PB'", "'FDLIC PB','FDLIC CCI'");
                cmd += " AND " + newCompany + " ";
            }
            cmd += " ORDER by `date`, `trustCompany`;  ";

            DataTable ddx = G1.get_db_data(cmd);

            DateTime reportDate = DateTime.Now;
            //for ( i=(ddx.Rows.Count-1); i>=0; i--)
            //{
            //    reportDate = ddx.Rows[i]["reportDate"].ObjToDateTime();
            //    if (reportDate < startDate || reportDate > stopDate)
            //        ddx.Rows.RemoveAt(i);
            //}


            DataTable dd = dt.Clone();
            string desc = "";
            double totalValue = 0D;
            double totalReceived = 0D;
            bool didIt = false;

            if (lastRow < 0)
                lastRow = firstRow;

            for ( i = firstRow; i <= lastRow; i++)
            {
                didIt = false;
                try
                {
                    date = dt.Rows[i]["date"].ObjToDateTime();
                    if (date.Year > 100)
                    {
                        if (date < startDate || date > stopDate)
                        {
                            //dt.Rows[i]["received"] = 0D;
                            //continue;
                        }
                    }

                    contractNumber = dt.Rows[i]["contract"].ObjToString();
                    dRow = dd.NewRow();
                    if (!String.IsNullOrWhiteSpace(contractNumber))
                    {
                        if (date.Year > 1000 && (date >= startDate && date <= stopDate))
                        {
                            didIt = true;
                            dRow["contract"] = contractNumber;
                            dRow["cashPaid1"] = "DC Cash";
                            desc = dt.Rows[i]["desc"].ObjToString();
                            dRow["desc"] = desc;
                            dRow["funeral"] = dt.Rows[i]["funeral"].ObjToString();
                            date = dt.Rows[i]["date"].ObjToDateTime();
                            dRow["date"] = G1.DTtoMySQLDT(date.ObjToDateTime().ToString("yyyy-MM-dd"));
                            dRow["value"] = dt.Rows[i]["value"].ObjToDouble();
                            if (date > this.dateTimePicker2.Value)
                                dRow["red1"] = "Y";
                            dRow["trust"] = dt.Rows[i]["trust"].ObjToString();

                            if (desc.ToUpper().IndexOf("PD") < 0)
                                totalValue += dt.Rows[i]["value"].ObjToDouble();
                        }
                    }

                    contractNumber2 = dt.Rows[i]["otherContract"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(contractNumber2))
                    {
                        date = dt.Rows[i]["dateReceived"].ObjToDateTime();
                        if (date.Year > 1000 && (date >= startDate && date <= stopDate))
                        {
                            didIt = true;
                            dRow["otherContract"] = contractNumber2;
                            dRow["cashPaid2"] = "DC Cash";
                            desc = dt.Rows[i]["otherdesc"].ObjToString();
                            dRow["otherdesc"] = desc;
                            dRow["otherFuneral"] = dt.Rows[i]["otherFuneral"].ObjToString();
                            date = dt.Rows[i]["dateReceived"].ObjToDateTime();
                            dRow["dateReceived"] = G1.DTtoMySQLDT(date.ObjToDateTime().ToString("yyyy-MM-dd"));
                            dRow["received"] = dt.Rows[i]["received"].ObjToDouble();
                            if (date > this.dateTimePicker2.Value)
                                dRow["red2"] = "Y";

                            dRow["othertrust"] = dt.Rows[i]["othertrust"].ObjToString();
                            if (desc.ToUpper().IndexOf("PD") < 0)
                                totalReceived += dt.Rows[i]["received"].ObjToDouble();
                        }
                    }
                    if ( didIt )
                        dd.Rows.Add(dRow);

                    if ( (!String.IsNullOrWhiteSpace ( contractNumber ) || !String.IsNullOrWhiteSpace ( contractNumber2 )) && didIt )
                    {
                        dRow = dd.NewRow();
                        int rowNum = dd.Rows.Count - 1;
                        var sourceRow = dd.Rows[rowNum];
                        dRow.ItemArray = sourceRow.ItemArray.Clone() as object[];
                        if ( !String.IsNullOrWhiteSpace ( contractNumber))
                            dRow["cashPaid1"] = "DC Paid";
                        if (!String.IsNullOrWhiteSpace(contractNumber2))
                            dRow["cashPaid2"] = "DC Paid";
                        dd.Rows.Add(dRow);
                    }
                }
                catch (Exception ex)
                {
                }
            }

            DataRow dr = null;
            dr = dt.NewRow();
            dt.Rows.InsertAt(dr, firstRow);

            //dr = dt.NewRow();
            //dr["value"] = totalValue;
            //dr["desc"] = "Total DC Paid";
            //dt.Rows.InsertAt(dr, firstRow);

            dr = dt.NewRow();
            dt.Rows.InsertAt(dr, firstRow);

            DataTable cashDt = dd.Clone();
            DataTable paidDt = dd.Clone();

            DataRow[] dRows = dd.Select("cashPaid1='DC Cash' OR cashPaid2='DC Cash'");
            if (dRows.Length > 0)
                cashDt = dRows.CopyToDataTable();

            dRows = dd.Select("cashPaid1='DC Paid' OR cashPaid2='DC Paid'");
            if (dRows.Length > 0)
                paidDt = dRows.CopyToDataTable();

            double cashReceived = 0D;
            int receivedCount = 0;
            double paidTotal = 0D;
            int totalCount = 0;
            double totalPaid = 0D;

            for (i = paidDt.Rows.Count - 1; i >= 0; i--)
            {
                trustCompany = paidDt.Rows[i]["othertrust"].ObjToString();
                if (trustCompany.ToUpper() == "FORETHOUGHT")
                {
                    dValue = paidDt.Rows[i]["received"].ObjToDouble();
                    cashReceived += dValue;
                    receivedCount++;
                }
                trustCompany = paidDt.Rows[i]["trust"].ObjToString();
                if (trustCompany.ToUpper() == "SECURITY NATIONAL")
                {
                    dValue = paidDt.Rows[i]["value"].ObjToDouble();
                    paidTotal += dValue;
                    totalCount++;
                }

                dValue = paidDt.Rows[i]["value"].ObjToDouble();
                totalPaid += dValue;
            }

            if (totalCount > 1 || receivedCount > 1)
            {
                dRow = dt.NewRow();
                if (paidTotal != 0D)
                {
                    if (paidTotal > 0D)
                        paidTotal = paidTotal * -1D;
                    dRow["value"] = paidTotal;
                    dRow["red1"] = "Y";
                    dRow["desc"] = "Total DC Paid";
                }
                if (cashReceived != 0D)
                {
                    if (cashReceived > 0D)
                        cashReceived = cashReceived * -1D;
                    dRow["received"] = cashReceived;
                    dRow["red2"] = "Y";
                    dRow["otherdesc"] = "Total DC Paid";
                }
                dt.Rows.InsertAt(dRow, firstRow);

                dRow = dt.NewRow();
                dt.Rows.InsertAt(dRow, firstRow);
            }


            for ( i=paidDt.Rows.Count - 1; i>=0; i--)
            {
                dr = paidDt.Rows[i];
                dValue = dr["value"].ObjToDouble();
                if (dValue != 0D)
                    dr["value"] = dValue;

                dValue = dr["received"].ObjToDouble();
                if (dValue != 0D)
                    dr["received"] = dValue;
                else
                    dr["cashPaid2"] = "";
                dRow = dt.NewRow();
                dRow.ItemArray = dr.ItemArray;
                dt.Rows.InsertAt(dRow, firstRow);
            }

            cashReceived = 0D;
            receivedCount = 0;
            paidTotal = 0D;
            totalCount = 0;
            totalPaid = 0D;

            for (i = cashDt.Rows.Count - 1; i >= 0; i--)
            {
                trustCompany = cashDt.Rows[i]["othertrust"].ObjToString();
                if (trustCompany.ToUpper() == "FORETHOUGHT")
                {
                    dValue = cashDt.Rows[i]["received"].ObjToDouble();
                    cashReceived += dValue;
                    receivedCount++;
                }
                trustCompany = cashDt.Rows[i]["trust"].ObjToString();
                if (trustCompany.ToUpper() == "SECURITY NATIONAL")
                {
                    dValue = cashDt.Rows[i]["value"].ObjToDouble();
                    paidTotal += dValue;
                    totalCount++;
                }

                dValue = cashDt.Rows[i]["value"].ObjToDouble();
                totalPaid += dValue;
            }

            if (totalCount > 1 || receivedCount > 1)
            {
                dRow = dt.NewRow();
                dt.Rows.InsertAt(dRow, firstRow);

                dRow = dt.NewRow();
                if (paidTotal != 0D)
                {
                    if (paidTotal < 0D)
                        paidTotal = paidTotal * -1D;
                    dRow["value"] = paidTotal;
                    dRow["red1"] = "Y";
                    dRow["desc"] = "Total DC Cash";
                }
                if (cashReceived != 0D)
                {
                    if (cashReceived < 0D)
                        cashReceived = cashReceived * -1D;
                    dRow["received"] = cashReceived;
                    dRow["red2"] = "Y";
                    dRow["otherdesc"] = "Total DC Cash";
                }
                dt.Rows.InsertAt(dRow, firstRow);

                dRow = dt.NewRow();
                dt.Rows.InsertAt(dRow, firstRow);
            }


            for (i = cashDt.Rows.Count - 1; i >= 0; i--)
            {
                dr = cashDt.Rows[i];
                dValue = dr["value"].ObjToDouble();
                if (dValue < 0D)
                    dr["value"] = dValue * -1D;
                dValue = dr["received"].ObjToDouble();
                if (dValue < 0D)
                    dr["received"] = dValue * -1D;
                dRow = dt.NewRow();
                dRow.ItemArray = dr.ItemArray;
                dt.Rows.InsertAt(dRow, firstRow);
            }

            //for ( i = dd.Rows.Count - 1; i >= 0; i--)
            //{
            //    dr = dd.Rows[i];
            //    dRow = dt.NewRow();
            //    dRow.ItemArray = dr.ItemArray;
            //    dt.Rows.InsertAt(dRow, firstRow);
            //}

            dRow = dt.NewRow();
            dt.Rows.InsertAt(dRow, firstRow);

            DataView tempview = ddx.DefaultView;
            tempview.Sort = "date DESC";
            ddx = tempview.ToTable();

            paidTotal = 0D;
            dRow = dt.NewRow();
            dt.Rows.InsertAt(dRow, firstRow);

            double cashTotal = 0D;
            cashReceived = 0D;
            totalCount = 0;
            receivedCount = 0;

            for (i = 0; i < ddx.Rows.Count; i++)
            {
                trustCompany = ddx.Rows[i]["trustCompany"].ObjToString();
                if (trustCompany.ToUpper() == "FORETHOUGHT")
                {
                    dValue = ddx.Rows[i]["fun_amtReceived"].ObjToDouble();
                    cashReceived += dValue * -1D;
                    receivedCount++;
                }
                else
                {
                    dValue = ddx.Rows[i]["value"].ObjToDouble();
                    paidTotal += dValue * -1D;
                    totalCount++;
                }
            }

            if (totalCount > 1 || receivedCount > 1)
            {
                dRow = dt.NewRow();
                if (paidTotal != 0D)
                {
                    dRow["value"] = paidTotal;
                    dRow["red1"] = "Y";
                    dRow["desc"] = "Total DC Paid";
                }
                if (cashReceived != 0D)
                {
                    dRow["received"] = cashReceived;
                    dRow["red2"] = "Y";
                    dRow["otherdesc"] = "Total DC Paid";
                }
                dt.Rows.InsertAt(dRow, firstRow);

                dRow = dt.NewRow();
                dt.Rows.InsertAt(dRow, firstRow);
            }

            dRow = dt.NewRow();
            dt.Rows.InsertAt(dRow, firstRow);

            for (i = 0; i < ddx.Rows.Count; i++)
            {
                trustCompany = ddx.Rows[i]["trustCompany"].ObjToString();
                dRow = dt.NewRow();
                dRow["contract"] = ddx.Rows[i]["contract"].ObjToString();
                if (trustCompany.ToUpper() == "FORETHOUGHT")
                {
                    dRow["otherContract"] = ddx.Rows[i]["contract"].ObjToString();
                    dRow["contract"] = "";
                    dRow["cashPaid2"] = "DC Paid";
                    desc = ddx.Rows[i]["desc"].ObjToString();
                    dRow["otherdesc"] = desc;
                    dRow["otherFuneral"] = ddx.Rows[i]["funeral"].ObjToString();
                    date = ddx.Rows[i]["date"].ObjToDateTime();
                    //dRow["date"] = G1.DTtoMySQLDT(date.ObjToDateTime().ToString("yyyy-MM-dd"));
                    dRow["dateReceived"] = G1.DTtoMySQLDT(date.ObjToDateTime().ToString("yyyy-MM-dd"));
                    dValue = ddx.Rows[i]["value"].ObjToDouble();
                    dValue = ddx.Rows[i]["fun_amtReceived"].ObjToDouble();
                    dRow["received"] = dValue * -1D;
                    paidTotal = dValue * -1D;
                    dRow["red2"] = "Y";
                }
                else
                {
                    dRow["cashPaid1"] = "DC Paid";
                    desc = ddx.Rows[i]["desc"].ObjToString();
                    dRow["desc"] = desc;
                    dRow["funeral"] = ddx.Rows[i]["funeral"].ObjToString();
                    date = ddx.Rows[i]["date"].ObjToDateTime();
                    dRow["date"] = G1.DTtoMySQLDT(date.ObjToDateTime().ToString("yyyy-MM-dd"));
                    dValue = ddx.Rows[i]["value"].ObjToDouble();
                    dValue = ddx.Rows[i]["fun_amtReceived"].ObjToDouble();
                    dRow["value"] = dValue * -1D;
                    paidTotal = dValue * -1D;
                    dRow["red1"] = "Y";
                }
                dt.Rows.InsertAt(dRow, firstRow);
            }

            dRow = dt.NewRow();
            dt.Rows.InsertAt(dRow, firstRow);

            cashTotal = 0D;
            cashReceived = 0D;
            totalCount = 0;
            receivedCount = 0;
            for (i = 0; i < ddx.Rows.Count; i++)
            {
                trustCompany = ddx.Rows[i]["trustCompany"].ObjToString();
                if (trustCompany.ToUpper() == "FORETHOUGHT")
                {
                    cashReceived += ddx.Rows[i]["fun_amtReceived"].ObjToDouble();
                    receivedCount++;
                }
                else
                {
                    cashTotal += ddx.Rows[i]["fun_amtReceived"].ObjToDouble();
                    totalCount++;
                }
            }

            if (totalCount > 1 || receivedCount > 1)
            {
                dRow = dt.NewRow();
                if (cashTotal != 0D)
                {
                    dRow["value"] = cashTotal;
                    dRow["red1"] = "Y";
                    dRow["desc"] = "Total DC Cash";
                }
                else if (cashReceived != 0D)
                {
                    dRow["received"] = cashReceived;
                    dRow["red2"] = "Y";
                    dRow["otherdesc"] = "Total DC Cash";
                }
                dt.Rows.InsertAt(dRow, firstRow);
            }

            dRow = dt.NewRow();
            dt.Rows.InsertAt(dRow, firstRow);

            for (i = 0; i < ddx.Rows.Count; i++)
            {
                trustCompany = ddx.Rows[i]["trustCompany"].ObjToString();
                dRow = dt.NewRow();
                if (trustCompany.ToUpper() == "FORETHOUGHT")
                {
                    dRow["otherContract"] = ddx.Rows[i]["contract"].ObjToString();
                    dRow["cashPaid2"] = "DC Cash";
                    desc = ddx.Rows[i]["desc"].ObjToString();
                    dRow["otherdesc"] = desc;
                    dRow["otherFuneral"] = ddx.Rows[i]["funeral"].ObjToString();
                    date = ddx.Rows[i]["date"].ObjToDateTime();
                    //dRow["date"] = G1.DTtoMySQLDT(date.ObjToDateTime().ToString("yyyy-MM-dd"));
                    dRow["dateReceived"] = G1.DTtoMySQLDT(date.ObjToDateTime().ToString("yyyy-MM-dd"));
                    //dRow["value"] = ddx.Rows[i]["value"].ObjToDouble();
                    dValue = ddx.Rows[i]["fun_amtReceived"].ObjToDouble();
                    dRow["received"] = dValue;
                    cashTotal += dRow["value"].ObjToDouble();
                    dRow["red2"] = "Y";
                }
                else
                {
                    dRow["contract"] = ddx.Rows[i]["contract"].ObjToString();
                    dRow["cashPaid1"] = "DC Cash";
                    desc = ddx.Rows[i]["desc"].ObjToString();
                    dRow["desc"] = desc;
                    dRow["funeral"] = ddx.Rows[i]["funeral"].ObjToString();
                    date = ddx.Rows[i]["date"].ObjToDateTime();
                    dRow["date"] = G1.DTtoMySQLDT(date.ObjToDateTime().ToString("yyyy-MM-dd"));
                    //dRow["value"] = ddx.Rows[i]["value"].ObjToDouble();
                    dValue = ddx.Rows[i]["fun_amtReceived"].ObjToDouble();
                    dRow["value"] = dValue;
                    cashTotal += dRow["value"].ObjToDouble();
                    dRow["red1"] = "Y";
                }
                dt.Rows.InsertAt(dRow, firstRow);
            }


            gridMain2.Columns["cashPaid1"].Caption = " ";
            gridMain2.Columns["cashPaid2"].Caption = " ";
            gridMain2.Columns["junk1"].Caption = " ";
            gridMain2.Columns["junk2"].Caption = " ";
            gridMain2.Columns["junk3"].Caption = " ";
            gridMain2.Columns["sn1"].Caption = " ";
            gridMain2.Columns["sn2"].Caption = " ";

            gridMain2.Columns["otherContract"].Visible = true;
            gridMain2.Columns["dateReceived"].Visible = true;
            gridMain2.Columns["reportDate"].Visible = false;

            ClearAllPositions(gridMain2);

            i = 1;
            G1.SetColumnPosition(gridMain2, "num", i++);
            G1.SetColumnPosition(gridMain2, "month", i++);
            G1.SetColumnPosition(gridMain2, "sn1", i++);
            G1.SetColumnPosition(gridMain2, "junk1", i++);
            G1.SetColumnPosition(gridMain2, "value", i++);
            G1.SetColumnPosition(gridMain2, "cashPaid1", i++);
            G1.SetColumnPosition(gridMain2, "desc", i++);
            G1.SetColumnPosition(gridMain2, "date", i++);
            G1.SetColumnPosition(gridMain2, "contract", i++);
            G1.SetColumnPosition(gridMain2, "funeral", i++);

            G1.SetColumnPosition(gridMain2, "sn2", i++);
            G1.SetColumnPosition(gridMain2, "received", i++);
            G1.SetColumnPosition(gridMain2, "cashPaid2", i++);
            G1.SetColumnPosition(gridMain2, "otherdesc", i++);
            G1.SetColumnPosition(gridMain2, "dateReceived", i++);
            G1.SetColumnPosition(gridMain2, "otherContract", i++);
            G1.SetColumnPosition(gridMain2, "otherFuneral", i++);

            return dt;
        }
        /****************************************************************************************/
        private DataTable loadCashPaidSNFTx(DataTable dt)
        {
            int firstRow = -1;
            int lastRow = -1;
            string contractNumber = "";
            string contractNumber2 = "";
            DateTime date = DateTime.Now;
            DataRow dRow = null;
            double dValue = 0D;
            int i = 0;
            string month = "";

            dt.Columns.Add("cashPaid1");
            dt.Columns.Add("red1");
            dt.Columns.Add("cashPaid2");
            dt.Columns.Add("red2");

            string otherTrust = "";
            string trustCompany = "";

            for (i = 0; i < dt.Rows.Count; i++)
            {
                month = dt.Rows[i]["month"].ObjToString();
                if (month.ToUpper().IndexOf("BALANCE") > 0)
                    continue;
                contractNumber = dt.Rows[i]["contract"].ObjToString();
                if (!String.IsNullOrWhiteSpace(contractNumber) && firstRow < 0)
                    firstRow = i;
                if (!String.IsNullOrWhiteSpace(contractNumber))
                    lastRow = i;
                otherTrust = dt.Rows[i]["otherTrust"].ObjToString().ToUpper();
                if (otherTrust == "FORETHOUGHT")
                {
                    date = dt.Rows[i]["dateReceived"].ObjToDateTime();
                    if (date.Year < 1000)
                    {
                        otherTrust = dt.Rows[i]["otherFuneral"].ObjToString();
                        if (otherTrust.ToUpper().IndexOf("O/S") < 0 && otherTrust.ToUpper().IndexOf("OS") < 0)
                        {
                            dt.Rows[i]["received"] = 0D;
                            dt.Rows[i]["otherdesc"] = "";
                            dt.Rows[i]["otherContract"] = "";
                            dt.Rows[i]["otherFuneral"] = "";
                        }
                    }
                }
                else
                {
                    date = dt.Rows[i]["date"].ObjToDateTime();
                    if (date.Year < 1000)
                    {
                        dt.Rows[i]["received"] = 0D;
                    }
                }
            }

            DateTime startDate = this.dateTimePicker1.Value;
            DateTime stopDate = this.dateTimePicker2.Value;
            string sDate1 = startDate.ToString("yyyy-MM-dd");
            string sDate2 = stopDate.ToString("yyyy-MM-dd");

            string companies = getCompanyQuery(workCompanies);

            string cmd = "Select * from `trust_data_overruns` t WHERE `date` >= '" + sDate1 + "' AND `date` <= '" + sDate2 + " 23:59:59' ";

            if (workReport == "Pre 2002 Report")
                cmd += " AND t.`preOrPost` = 'Pre' ";
            else
                cmd += " AND t.`preOrPost` <> 'Pre' ";

            if (!String.IsNullOrWhiteSpace(companies))
            {
                string newCompany = companies;
                if (newCompany.IndexOf("FDLIC PB") >= 0)
                    newCompany = newCompany.Replace("'FDLIC PB'", "'FDLIC PB','FDLIC CCI'");
                cmd += " AND " + newCompany + " ";
            }
            cmd += " ORDER by `date`, `trustCompany`;  ";

            DataTable ddx = G1.get_db_data(cmd);


            DataTable dd = dt.Clone();
            string desc = "";
            double totalValue = 0D;
            double totalReceived = 0D;

            for (i = firstRow; i <= lastRow; i++)
            {
                try
                {
                    date = dt.Rows[i]["date"].ObjToDateTime();
                    if (date.Year > 100)
                    {
                        if (date < startDate || date > stopDate)
                        {
                            dt.Rows[i]["received"] = 0D;
                            continue;
                        }
                    }

                    contractNumber = dt.Rows[i]["contract"].ObjToString();
                    dRow = dd.NewRow();
                    if (!String.IsNullOrWhiteSpace(contractNumber))
                    {
                        dRow["contract"] = contractNumber;
                        dRow["cashPaid1"] = "DC Cash";
                        desc = dt.Rows[i]["desc"].ObjToString();
                        dRow["desc"] = desc;
                        dRow["funeral"] = dt.Rows[i]["funeral"].ObjToString();
                        date = dt.Rows[i]["date"].ObjToDateTime();
                        dRow["date"] = G1.DTtoMySQLDT(date.ObjToDateTime().ToString("yyyy-MM-dd"));
                        dRow["value"] = dt.Rows[i]["value"].ObjToDouble();
                        if (date > this.dateTimePicker2.Value)
                            dRow["red1"] = "Y";

                        if (desc.ToUpper().IndexOf("PD") < 0)
                            totalValue += dt.Rows[i]["value"].ObjToDouble();
                    }

                    contractNumber2 = dt.Rows[i]["otherContract"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(contractNumber2))
                    {
                        dRow["otherContract"] = contractNumber2;
                        dRow["cashPaid2"] = "DC Cash";
                        desc = dt.Rows[i]["otherdesc"].ObjToString();
                        dRow["otherdesc"] = desc;
                        dRow["otherFuneral"] = dt.Rows[i]["otherFuneral"].ObjToString();
                        date = dt.Rows[i]["dateReceived"].ObjToDateTime();
                        dRow["dateReceived"] = G1.DTtoMySQLDT(date.ObjToDateTime().ToString("yyyy-MM-dd"));
                        dRow["received"] = dt.Rows[i]["received"].ObjToDouble();
                        if (date > this.dateTimePicker2.Value)
                            dRow["red2"] = "Y";

                        if (desc.ToUpper().IndexOf("PD") < 0)
                            totalReceived += dt.Rows[i]["received"].ObjToDouble();
                    }
                    dd.Rows.Add(dRow);

                    if (!String.IsNullOrWhiteSpace(contractNumber) || !String.IsNullOrWhiteSpace(contractNumber2))
                    {
                        dRow = dd.NewRow();
                        int rowNum = dd.Rows.Count - 1;
                        var sourceRow = dd.Rows[rowNum];
                        dRow.ItemArray = sourceRow.ItemArray.Clone() as object[];
                        if (!String.IsNullOrWhiteSpace(contractNumber))
                            dRow["cashPaid1"] = "DC Paid";
                        if (!String.IsNullOrWhiteSpace(contractNumber2))
                            dRow["cashPaid2"] = "DC Paid";
                        dd.Rows.Add(dRow);
                    }
                }
                catch (Exception ex)
                {
                }
            }

            DataRow dr = null;
            dr = dt.NewRow();
            dt.Rows.InsertAt(dr, firstRow);

            //dr = dt.NewRow();
            //dr["value"] = totalValue;
            //dr["desc"] = "Total DC Paid";
            //dt.Rows.InsertAt(dr, firstRow);

            dr = dt.NewRow();
            dt.Rows.InsertAt(dr, firstRow);

            for (i = dd.Rows.Count - 1; i >= 0; i--)
            {
                dr = dd.Rows[i];
                dRow = dt.NewRow();
                dRow.ItemArray = dr.ItemArray;
                dt.Rows.InsertAt(dRow, firstRow);
            }

            dRow = dt.NewRow();
            dt.Rows.InsertAt(dRow, firstRow);

            DataView tempview = ddx.DefaultView;
            tempview.Sort = "date DESC";
            ddx = tempview.ToTable();

            double paidTotal = 0D;
            dRow = dt.NewRow();
            dt.Rows.InsertAt(dRow, firstRow);

            double cashTotal = 0D;
            double cashReceived = 0D;
            int totalCount = 0;
            int receivedCount = 0;

            for (i = 0; i < ddx.Rows.Count; i++)
            {
                trustCompany = ddx.Rows[i]["trustCompany"].ObjToString();
                if (trustCompany.ToUpper() == "FORETHOUGHT")
                {
                    dValue = ddx.Rows[i]["fun_amtReceived"].ObjToDouble();
                    cashReceived += dValue * -1D;
                    receivedCount++;
                }
                else
                {
                    dValue = ddx.Rows[i]["value"].ObjToDouble();
                    paidTotal += dValue * -1D;
                    totalCount++;
                }
            }

            if (totalCount > 1 || receivedCount > 1)
            {
                dRow = dt.NewRow();
                if (paidTotal != 0D)
                {
                    dRow["value"] = paidTotal;
                    dRow["red1"] = "Y";
                    dRow["desc"] = "Total DC Paid";
                }
                else if (cashReceived != 0D)
                {
                    dRow["received"] = cashReceived;
                    dRow["red2"] = "Y";
                    dRow["otherdesc"] = "Total DC Paid";
                }
                dt.Rows.InsertAt(dRow, firstRow);
            }

            dRow = dt.NewRow();
            dt.Rows.InsertAt(dRow, firstRow);

            for (i = 0; i < ddx.Rows.Count; i++)
            {
                trustCompany = ddx.Rows[i]["trustCompany"].ObjToString();
                dRow = dt.NewRow();
                dRow["contract"] = ddx.Rows[i]["contract"].ObjToString();
                if (trustCompany.ToUpper() == "FORETHOUGHT")
                {
                    dRow["cashPaid2"] = "DC Paid";
                    desc = ddx.Rows[i]["desc"].ObjToString();
                    dRow["otherdesc"] = desc;
                    dRow["otherFuneral"] = ddx.Rows[i]["funeral"].ObjToString();
                    date = ddx.Rows[i]["date"].ObjToDateTime();
                    dRow["date"] = G1.DTtoMySQLDT(date.ObjToDateTime().ToString("yyyy-MM-dd"));
                    dValue = ddx.Rows[i]["value"].ObjToDouble();
                    dValue = ddx.Rows[i]["fun_amtReceived"].ObjToDouble();
                    dRow["received"] = dValue * -1D;
                    paidTotal = dValue * -1D;
                    dRow["red2"] = "Y";
                }
                else
                {
                    dRow["cashPaid1"] = "DC Paid";
                    desc = ddx.Rows[i]["desc"].ObjToString();
                    dRow["desc"] = desc;
                    dRow["funeral"] = ddx.Rows[i]["funeral"].ObjToString();
                    date = ddx.Rows[i]["date"].ObjToDateTime();
                    dRow["date"] = G1.DTtoMySQLDT(date.ObjToDateTime().ToString("yyyy-MM-dd"));
                    dValue = ddx.Rows[i]["value"].ObjToDouble();
                    dValue = ddx.Rows[i]["fun_amtReceived"].ObjToDouble();
                    dRow["value"] = dValue * -1D;
                    paidTotal = dValue * -1D;
                    dRow["red1"] = "Y";
                }
                dt.Rows.InsertAt(dRow, firstRow);
            }

            dRow = dt.NewRow();
            dt.Rows.InsertAt(dRow, firstRow);

            cashTotal = 0D;
            cashReceived = 0D;
            totalCount = 0;
            receivedCount = 0;
            for (i = 0; i < ddx.Rows.Count; i++)
            {
                trustCompany = ddx.Rows[i]["trustCompany"].ObjToString();
                if (trustCompany.ToUpper() == "FORETHOUGHT")
                {
                    cashReceived += ddx.Rows[i]["fun_amtReceived"].ObjToDouble();
                    receivedCount++;
                }
                else
                {
                    cashTotal += ddx.Rows[i]["fun_amtReceived"].ObjToDouble();
                    totalCount++;
                }
            }

            if (totalCount > 1 || receivedCount > 1)
            {
                dRow = dt.NewRow();
                if (cashTotal != 0D)
                {
                    dRow["value"] = cashTotal;
                    dRow["red1"] = "Y";
                    dRow["desc"] = "Total DC Cash";
                }
                else if (cashReceived != 0D)
                {
                    dRow["received"] = cashReceived;
                    dRow["red2"] = "Y";
                    dRow["otherdesc"] = "Total DC Cash";
                }
                dt.Rows.InsertAt(dRow, firstRow);
            }

            dRow = dt.NewRow();
            dt.Rows.InsertAt(dRow, firstRow);

            for (i = 0; i < ddx.Rows.Count; i++)
            {
                trustCompany = ddx.Rows[i]["trustCompany"].ObjToString();
                dRow = dt.NewRow();
                if (trustCompany.ToUpper() == "FORETHOUGHT")
                {
                    dRow["otherContract"] = ddx.Rows[i]["contract"].ObjToString();
                    dRow["cashPaid2"] = "DC Cash";
                    desc = ddx.Rows[i]["desc"].ObjToString();
                    dRow["otherdesc"] = desc;
                    dRow["otherFuneral"] = ddx.Rows[i]["funeral"].ObjToString();
                    date = ddx.Rows[i]["date"].ObjToDateTime();
                    dRow["date"] = G1.DTtoMySQLDT(date.ObjToDateTime().ToString("yyyy-MM-dd"));
                    //dRow["value"] = ddx.Rows[i]["value"].ObjToDouble();
                    dValue = ddx.Rows[i]["fun_amtReceived"].ObjToDouble();
                    dRow["received"] = dValue;
                    cashTotal += dRow["value"].ObjToDouble();
                    dRow["red2"] = "Y";
                }
                else
                {
                    dRow["contract"] = ddx.Rows[i]["contract"].ObjToString();
                    dRow["cashPaid1"] = "DC Cash";
                    desc = ddx.Rows[i]["desc"].ObjToString();
                    dRow["desc"] = desc;
                    dRow["funeral"] = ddx.Rows[i]["funeral"].ObjToString();
                    date = ddx.Rows[i]["date"].ObjToDateTime();
                    dRow["date"] = G1.DTtoMySQLDT(date.ObjToDateTime().ToString("yyyy-MM-dd"));
                    //dRow["value"] = ddx.Rows[i]["value"].ObjToDouble();
                    dValue = ddx.Rows[i]["fun_amtReceived"].ObjToDouble();
                    dRow["value"] = dValue;
                    cashTotal += dRow["value"].ObjToDouble();
                    dRow["red1"] = "Y";
                }
                dt.Rows.InsertAt(dRow, firstRow);
            }


            gridMain2.Columns["cashPaid1"].Caption = " ";
            gridMain2.Columns["cashPaid2"].Caption = " ";
            gridMain2.Columns["junk1"].Caption = " ";
            gridMain2.Columns["junk2"].Caption = " ";
            gridMain2.Columns["junk3"].Caption = " ";
            gridMain2.Columns["sn1"].Caption = " ";
            gridMain2.Columns["sn2"].Caption = " ";

            gridMain2.Columns["otherContract"].Visible = true;
            gridMain2.Columns["dateReceived"].Visible = true;
            gridMain2.Columns["reportDate"].Visible = false;

            ClearAllPositions(gridMain2);

            i = 1;
            G1.SetColumnPosition(gridMain2, "num", i++);
            G1.SetColumnPosition(gridMain2, "month", i++);
            G1.SetColumnPosition(gridMain2, "sn1", i++);
            G1.SetColumnPosition(gridMain2, "junk1", i++);
            G1.SetColumnPosition(gridMain2, "value", i++);
            G1.SetColumnPosition(gridMain2, "cashPaid1", i++);
            G1.SetColumnPosition(gridMain2, "desc", i++);
            G1.SetColumnPosition(gridMain2, "date", i++);
            G1.SetColumnPosition(gridMain2, "contract", i++);
            G1.SetColumnPosition(gridMain2, "funeral", i++);

            G1.SetColumnPosition(gridMain2, "sn2", i++);
            G1.SetColumnPosition(gridMain2, "received", i++);
            G1.SetColumnPosition(gridMain2, "cashPaid2", i++);
            G1.SetColumnPosition(gridMain2, "otherdesc", i++);
            G1.SetColumnPosition(gridMain2, "dateReceived", i++);
            G1.SetColumnPosition(gridMain2, "otherContract", i++);
            G1.SetColumnPosition(gridMain2, "otherFuneral", i++);

            return dt;
        }
        /****************************************************************************************/
        private DataTable loadCashPaidCD(DataTable dt)
        {
            int firstRow = -1;
            int lastRow = -1;
            string contractNumber = "";
            DateTime date = DateTime.Now;
            DataRow dRow = null;
            double dValue = 0D;

            dt.Columns.Add("cashPaid1");
            dt.Columns.Add("red1");
            dt.Columns.Add("cashPaid2");
            dt.Columns.Add("red2");

            int i = 0;

            for (i = 0; i < dt.Rows.Count; i++)
            {
                contractNumber = dt.Rows[i]["contract"].ObjToString();
                if (!String.IsNullOrWhiteSpace(contractNumber) && firstRow < 0)
                    firstRow = i;
                if (!String.IsNullOrWhiteSpace(contractNumber))
                    lastRow = i;
            }

            DateTime startDate = this.dateTimePicker1.Value;
            DateTime stopDate = this.dateTimePicker2.Value;
            string sDate1 = startDate.ToString("yyyy-MM-dd");
            string sDate2 = stopDate.ToString("yyyy-MM-dd");

            string companies = getCompanyQuery(workCompanies);

            string cmd = "Select * from `trust_data_overruns` t WHERE `date` >= '" + sDate1 + "' AND `date` <= '" + sDate2 + " 23:59:59' ";

            //if (workReport == "Pre 2002 Report")
            //    cmd += " AND t.`preOrPost` = 'Pre' ";
            //else
            //    cmd += " AND t.`preOrPost` <> 'Pre' ";

            if (!String.IsNullOrWhiteSpace(companies))
            {
                string newCompany = companies;
                if (newCompany.IndexOf("FDLIC PB") >= 0)
                    newCompany = newCompany.Replace("'FDLIC PB'", "'FDLIC PB','FDLIC CCI'");
                cmd += " AND " + newCompany + " ";
            }
            cmd += " ORDER by `date`, `trustCompany`;  ";

            DataTable ddx = G1.get_db_data(cmd);


            DataTable dd = dt.Clone();
            string desc = "";
            double totalValue = 0D;

            //for (i = firstRow; i <= lastRow; i++)
            //{
            //    try
            //    {
            //        date = dt.Rows[i]["date"].ObjToDateTime();
            //        if (date.Year > 100)
            //        {
            //            if (date < startDate || date > stopDate)
            //            {
            //                dt.Rows[i]["received"] = DBNull.Value;
            //                continue;
            //            }
            //        }

            //        contractNumber = dt.Rows[i]["contract"].ObjToString();
            //        dRow = dd.NewRow();
            //        dRow["contract"] = contractNumber;
            //        dRow["cashPaid1"] = "DC Paid";
            //        desc = dt.Rows[i]["desc"].ObjToString();
            //        dRow["desc"] = desc;
            //        dRow["funeral"] = dt.Rows[i]["funeral"].ObjToString();
            //        date = dt.Rows[i]["date"].ObjToDateTime();
            //        dRow["date"] = G1.DTtoMySQLDT(date.ObjToDateTime().ToString("yyyy-MM-dd"));
            //        dRow["value"] = dt.Rows[i]["value"].ObjToDouble();

            //        if (desc.ToUpper().IndexOf("PD") < 0)
            //            totalValue += dt.Rows[i]["value"].ObjToDouble();

            //        //dRow["otherFuneral"] = dt.Rows[i]["funeral"].ObjToString();
            //        //dRow["otherdesc"] = desc;
            //        //dRow["received"] = dt.Rows[i]["received"].ObjToDouble();
            //        //dRow["otherContract"] = contractNumber;

            //        //dt.Rows[i]["received"] = 0D;
            //        dd.Rows.Add(dRow);
            //    }
            //    catch (Exception ex)
            //    {
            //    }
            //}
            //DataRow dr = null;
            //dr = dt.NewRow();
            //dt.Rows.InsertAt(dr, firstRow);

            //dr = dt.NewRow();
            //dr["value"] = totalValue;
            //dr["desc"] = "Total DC Paid";
            //dt.Rows.InsertAt(dr, firstRow);

            //dr = dt.NewRow();
            //dt.Rows.InsertAt(dr, firstRow);

            //for (i = dd.Rows.Count - 1; i >= 0; i--)
            //{
            //    dr = dd.Rows[i];
            //    dRow = dt.NewRow();
            //    dRow.ItemArray = dr.ItemArray;
            //    dt.Rows.InsertAt(dRow, firstRow);
            //}

            //dRow = dt.NewRow();
            //dt.Rows.InsertAt(dRow, firstRow);

            ////firstRow += 4 + dd.Rows.Count;
            ////lastRow += 4 + dd.Rows.Count;

            //int xRow = dd.Rows.Count + 4;

            //dd.Rows.Clear();
            //totalValue = 0D;

            //for (i = firstRow + xRow; i <= lastRow + xRow; i++)
            //{
            //    try
            //    {
            //        date = dt.Rows[i]["date"].ObjToDateTime();
            //        if (date.Year > 100)
            //        {
            //            if (date < startDate || date > stopDate)
            //            {
            //                dt.Rows[i]["received"] = DBNull.Value;
            //                continue;
            //            }
            //        }

            //        contractNumber = dt.Rows[i]["contract"].ObjToString();
            //        dRow = dd.NewRow();
            //        dRow["contract"] = contractNumber;
            //        dRow["cashPaid1"] = "DC Cash";
            //        desc = dt.Rows[i]["desc"].ObjToString();
            //        dRow["desc"] = desc;
            //        dRow["funeral"] = dt.Rows[i]["funeral"].ObjToString();
            //        date = dt.Rows[i]["date"].ObjToDateTime();
            //        dRow["date"] = G1.DTtoMySQLDT(date.ObjToDateTime().ToString("yyyy-MM-dd"));
            //        dRow["value"] = dt.Rows[i]["value"].ObjToDouble();

            //        if (desc.ToUpper().IndexOf("PD") < 0)
            //            totalValue += dt.Rows[i]["value"].ObjToDouble();

            //        dRow["otherFuneral"] = dt.Rows[i]["funeral"].ObjToString();
            //        dRow["otherdesc"] = desc;
            //        dValue = dt.Rows[i]["received"].ObjToDouble();
            //        dRow["received"] = dValue;
            //        dRow["otherContract"] = contractNumber;

            //        dt.Rows[i]["received"] = DBNull.Value;
            //        dd.Rows.Add(dRow);
            //    }
            //    catch (Exception ex)
            //    {
            //    }
            //}
            //dr = dt.NewRow();
            //dt.Rows.InsertAt(dr, firstRow);

            //dr = dt.NewRow();
            //dr["value"] = totalValue;
            //dr["desc"] = "Total DC Cash";
            //dt.Rows.InsertAt(dr, firstRow);

            //dr = dt.NewRow();
            //dt.Rows.InsertAt(dr, firstRow);

            //for (i = dd.Rows.Count - 1; i >= 0; i--)
            //{
            //    dr = dd.Rows[i];
            //    dRow = dt.NewRow();
            //    dRow.ItemArray = dr.ItemArray;
            //    dt.Rows.InsertAt(dRow, firstRow);
            //}

            //totalValue = 0D;
            //dd.Rows.Clear();

            //dValue = 0D;

            //for (i = 0; i < ddx.Rows.Count; i++)
            //{
            //    dRow = dt.NewRow();
            //    dt.Rows.InsertAt(dRow, firstRow);

            //    dRow = dt.NewRow();
            //    dRow["contract"] = ddx.Rows[i]["contract"].ObjToString();
            //    dRow["cashPaid1"] = "DC Paid";
            //    desc = ddx.Rows[i]["desc"].ObjToString();
            //    dRow["desc"] = desc;
            //    dRow["funeral"] = ddx.Rows[i]["funeral"].ObjToString();
            //    date = ddx.Rows[i]["date"].ObjToDateTime();
            //    dRow["date"] = G1.DTtoMySQLDT(date.ObjToDateTime().ToString("yyyy-MM-dd"));
            //    dValue = ddx.Rows[i]["value"].ObjToDouble();
            //    dRow["value"] = dValue * -1D;
            //    dRow["red1"] = "Y";
            //    dt.Rows.InsertAt(dRow, firstRow);

            //    dRow = dt.NewRow();
            //    dRow["contract"] = ddx.Rows[i]["contract"].ObjToString();
            //    dRow["cashPaid1"] = "DC Cash";
            //    desc = ddx.Rows[i]["desc"].ObjToString();
            //    dRow["desc"] = desc;
            //    dRow["funeral"] = ddx.Rows[i]["funeral"].ObjToString();
            //    date = ddx.Rows[i]["date"].ObjToDateTime();
            //    dRow["date"] = G1.DTtoMySQLDT(date.ObjToDateTime().ToString("yyyy-MM-dd"));
            //    dRow["value"] = ddx.Rows[i]["value"].ObjToDouble();
            //    dRow["red1"] = "Y";
            //    dt.Rows.InsertAt(dRow, firstRow);
            //}

            gridMain2.Columns["cashPaid1"].Caption = " ";
            gridMain2.Columns["cashPaid2"].Caption = " ";
            gridMain2.Columns["junk1"].Caption = " ";
            gridMain2.Columns["junk2"].Caption = " ";
            gridMain2.Columns["junk3"].Caption = " ";
            gridMain2.Columns["sn1"].Caption = " ";
            gridMain2.Columns["sn2"].Caption = " ";

            gridMain2.Columns["otherContract"].Visible = false;
            gridMain2.Columns["dateReceived"].Visible = false;
            gridMain2.Columns["reportDate"].Visible = false;

            gridMain2.Columns["sn1"].Visible = true;
            gridMain2.Columns["sn2"].Visible = true;
            gridMain2.Columns["junk1"].Visible = true;
            gridMain2.Columns["junk2"].Visible = true;

            gridMain2.Columns["value"].Caption = "CD BALANCE";
            gridMain2.Columns["received"].Caption = "CADENCE BALANCE";

            ClearAllPositions(gridMain2);

            i = 1;

            G1.SetColumnPosition(gridMain2, "num", i++);
            G1.SetColumnPosition(gridMain2, "month", i++);
            G1.SetColumnPosition(gridMain2, "sn1", i++);
            //G1.SetColumnPosition(gridMain2, "junk1", i++);
            G1.SetColumnPosition(gridMain2, "value", i++);
            G1.SetColumnPosition(gridMain2, "desc", i++);
            G1.SetColumnPosition(gridMain2, "date", i++);
            G1.SetColumnPosition(gridMain2, "contract", i++);
            G1.SetColumnPosition(gridMain2, "funeral", i++);

            G1.SetColumnPosition(gridMain2, "junk1", i++);
            G1.SetColumnPosition(gridMain2, "junk2", i++);
            G1.SetColumnPosition(gridMain2, "junk3", i++);
            G1.SetColumnPosition(gridMain2, "sn2", i++);

            //G1.SetColumnPosition(gridMain2, "sn1", i++);
            G1.SetColumnPosition(gridMain2, "received", i++);
            G1.SetColumnPosition(gridMain2, "otherdesc", i++);
            //G1.SetColumnPosition(gridMain2, "cashPaid2", i++);
            G1.SetColumnPosition(gridMain2, "dateReceived", i++);
            G1.SetColumnPosition(gridMain2, "otherContract", i++);
            G1.SetColumnPosition(gridMain2, "otherFuneral", i++);

            return dt;
        }
        /****************************************************************************************/
        private DataTable loadCashPaid ( DataTable dt )
        {
            int firstRow = -1;
            int lastRow = -1;
            string contractNumber = "";
            DateTime date = DateTime.Now;
            DataRow dRow = null;
            double dValue = 0D;
            double value = 0D;
            string status = "";
            bool doit = false;
            string rp = "";

            dt.Columns.Add("cashPaid1");
            dt.Columns.Add("red1");
            dt.Columns.Add("cashPaid2");
            dt.Columns.Add("red2");

            //if (1 == 1)
            //    return dt;

            int i = 0;

            double redTotal = 0D;
            double totalTotal = 0D;

            for (i = 1; i < dt.Rows.Count; i++)
            {
                status = dt.Rows[i]["status"].ObjToString().ToUpper();
                value = dt.Rows[i]["value"].ObjToDouble();
                //if (status == "INCLUDE")
                //{
                //    if (dt.Rows[i]["received"].ObjToDouble() != 0D)
                //        value = dt.Rows[i]["received"].ObjToDouble();
                //}
                status = dt.Rows[i]["rp"].ObjToString();
                if (status == "XX")
                    redTotal += value;
                else
                    totalTotal += value;
            }

            for ( i=0; i<dt.Rows.Count; i++)
            {
                contractNumber = dt.Rows[i]["contract"].ObjToString();
                if ( !String.IsNullOrWhiteSpace ( contractNumber ) && firstRow < 0 )
                    firstRow = i;
                if (!String.IsNullOrWhiteSpace(contractNumber))
                    lastRow = i;
            }

            DateTime startDate = this.dateTimePicker1.Value;
            DateTime stopDate = this.dateTimePicker2.Value;
            DateTime newStopDate = stopDate.AddDays(workNextDays);
            string sDate1 = startDate.ToString("yyyy-MM-dd");
            string sDate2 = stopDate.ToString("yyyy-MM-dd");

            string companies = getCompanyQuery(workCompanies);

            string cmd = "Select * from `trust_data_overruns` t WHERE `date` >= '" + sDate1 + "' AND `date` <= '" + sDate2 + " 23:59:59' ";

            if (workReport == "Pre 2002 Report")
                cmd += " AND t.`preOrPost` = 'Pre' ";
            else
                cmd += " AND t.`preOrPost` = 'Post' ";

            if (!String.IsNullOrWhiteSpace(companies))
            {
                if (workReport == "Post 2002 Report - Forethought")
                    companies = "`trustCompany` IN ('FORETHOUGHT')";
                string newCompany = companies;
                if (newCompany.IndexOf("FDLIC PB") >= 0)
                    newCompany = newCompany.Replace("'FDLIC PB'", "'FDLIC PB','FDLIC CCI'");
                cmd += " AND " + newCompany + " ";
            }
            cmd += " ORDER by `date`, `trustCompany`;  ";

            DataTable ddx = G1.get_db_data(cmd);

            DateTime reportDate = DateTime.Now;
            for ( i = (ddx.Rows.Count - 1 ); i>=0; i--)
            {
                reportDate = ddx.Rows[i]["reportDate"].ObjToDateTime();
                if (reportDate.Year > 1000)
                {
                    if (reportDate < startDate || reportDate > stopDate)
                        ddx.Rows.RemoveAt(i);
                }

            }

            DataTable dd = dt.Clone();
            string desc = "";
            double totalValue = 0D;
            string funeral = "";
            bool avoid = false;

            if ( firstRow < 0 || lastRow < 0 )
            {
                return dt;
            }

            DataRow dr = null;

            for (i = firstRow; i <= lastRow; i++)
            {
                try
                {
                    avoid = false;
                    date = dt.Rows[i]["date"].ObjToDateTime();
                    if (date.Year > 100)
                    {
                        if (date < startDate || date > stopDate)
                        {
                            dt.Rows[i]["received"] = DBNull.Value;
                            continue;
                        }
                    }

                    //reportDate = dt.Rows[i]["reportDate"].ObjToDateTime();
                    //if ( reportDate.Year > 1000 )
                    //{
                    //    if (reportDate < startDate || reportDate > stopDate)
                    //        continue;
                    //}
                    contractNumber = dt.Rows[i]["contract"].ObjToString();
                    if (contractNumber == "P21045LI")
                    {
                    }
                    status = dt.Rows[i]["status"].ObjToString().ToUpper();
                    if (status == "INCLUDE")
                    {
                    }
                    rp = dt.Rows[i]["rp"].ObjToString().ToUpper();

                    dRow = dd.NewRow();
                    dRow["contract"] = contractNumber;
                    dRow["cashPaid1"] = "DC Paid";
                    desc = dt.Rows[i]["desc"].ObjToString();
                    dRow["desc"] = desc;
                    funeral = dt.Rows[i]["funeral"].ObjToString().ToUpper();
                    dRow["funeral"] = dt.Rows[i]["funeral"].ObjToString();
                    date = dt.Rows[i]["date"].ObjToDateTime();
                    dRow["date"] = G1.DTtoMySQLDT(date.ObjToDateTime().ToString("yyyy-MM-dd"));
                    value = dt.Rows[i]["value"].ObjToDouble();
                    if ( status == "INCLUDE")
                    {
                        if (dt.Rows[i]["received"].ObjToDouble() != 0D)
                            value = dt.Rows[i]["received"].ObjToDouble();
                    }
                    dRow["value"] = value;
                    dValue = dt.Rows[i]["fun_amtReceived"].ObjToDouble() * -1D;
                    if (dValue == 0D )
                    {
                        if (rp != "RP")
                        {
                            if (funeral.IndexOf("OS") == 0 || funeral.IndexOf("O/S") == 0)
                                dValue = dt.Rows[i]["value"].ObjToDouble();
                            if (dValue == 0D && value != 0D)
                                dValue = value;
                        }
                        else
                            avoid = true;
                    }
                    if (avoid)
                        continue;
                    dRow["value"] = dValue;

                    if (desc.ToUpper().IndexOf("PD") < 0 || status == "INCLUDE" )
                        totalValue += dRow["value"].ObjToDouble();

                    //dRow["otherFuneral"] = dt.Rows[i]["funeral"].ObjToString();
                    //dRow["otherdesc"] = desc;
                    //dRow["received"] = dt.Rows[i]["received"].ObjToDouble();
                    //dRow["otherContract"] = contractNumber;

                    //dt.Rows[i]["received"] = 0D;
                    dd.Rows.Add(dRow);
                }
                catch (Exception ex)
                {
                }
            }
            dr = null;
            dr = dt.NewRow();
            dt.Rows.InsertAt(dr, firstRow);

            dr = dt.NewRow();
            dr["value"] = totalValue;
            dr["desc"] = "Total DC Paid";
            dt.Rows.InsertAt(dr, firstRow);

            dr = dt.NewRow();
            dt.Rows.InsertAt(dr, firstRow);

            for ( i = dd.Rows.Count - 1; i >= 0; i--)
            {
                dr = dd.Rows[i];
                dRow = dt.NewRow();
                dRow.ItemArray = dr.ItemArray;
                dt.Rows.InsertAt(dRow, firstRow);
            }

            dRow = dt.NewRow();
            dt.Rows.InsertAt(dRow, firstRow);

            //firstRow += 4 + dd.Rows.Count;
            //lastRow += 4 + dd.Rows.Count;

            int xRow = dd.Rows.Count + 4;

            dd.Rows.Clear();
            totalValue = 0D;

            for ( i=firstRow+xRow; i<=lastRow+xRow; i++)
            {
                try
                {
                    avoid = false;
                    date = dt.Rows[i]["date"].ObjToDateTime();
                    //if (date.Year > 100)
                    //{
                    //    if (date < startDate || date > stopDate)
                    //    {
                    //        dt.Rows[i]["received"] = DBNull.Value;
                    //        continue;
                    //    }
                    //}

                    contractNumber = dt.Rows[i]["contract"].ObjToString();
                    if ( contractNumber == "P20029L")
                    {
                    }
                    desc = dt.Rows[i]["desc"].ObjToString();
                    status = dt.Rows[i]["status"].ObjToString().ToUpper();
                    if (status == "INCLUDE")
                    {
                    }
                    if (contractNumber == "FF23022LI")
                    {
                    }

                    rp = dt.Rows[i]["rp"].ObjToString().ToUpper();

                    dRow = dd.NewRow();
                    doit = false;
                    if (date.Year > 100 && date >= startDate && date <= stopDate)
                        doit = true;
                    //if (status == "INCLUDE")
                    //    doit = true;

                    if ( doit )
                    {
                        dRow["contract"] = contractNumber;
                        dRow["cashPaid1"] = "DC Cash";
                        desc = dt.Rows[i]["desc"].ObjToString();
                        dRow["desc"] = desc;
                        funeral = dt.Rows[i]["funeral"].ObjToString().ToUpper();
                        dRow["funeral"] = dt.Rows[i]["funeral"].ObjToString().ToUpper();
                        date = dt.Rows[i]["date"].ObjToDateTime();
                        dRow["date"] = G1.DTtoMySQLDT(date.ObjToDateTime().ToString("yyyy-MM-dd"));
                        value = Math.Abs(dt.Rows[i]["value"].ObjToDouble());
                        if (status == "INCLUDE")
                        {
                            if (dt.Rows[i]["received"].ObjToDouble() != 0D)
                                value = Math.Abs(dt.Rows[i]["received"].ObjToDouble());
                        }
                        dRow["value"] = value;
                        dValue = Math.Abs(dt.Rows[i]["fun_amtReceived"].ObjToDouble());
                        if (dValue == 0D)
                        {
                            rp = dt.Rows[i]["rp"].ObjToString().ToUpper();
                            if (rp != "RP")
                            {
                                if (funeral.IndexOf("OS") == 0 || funeral.IndexOf("O/S") == 0)
                                    dValue = Math.Abs(dt.Rows[i]["value"].ObjToDouble());
                                if (dValue == 0D && value != 0D)
                                    dValue = value;
                            }
                            else
                                avoid = true;
                        }

                        if (avoid)
                            continue;

                        dRow["value"] = dValue;

                        if (desc.ToUpper().IndexOf("PD") < 0 || status == "INCLUDE")
                        {
                            //totalValue += Math.Abs(dt.Rows[i]["value"].ObjToDouble());

                            dValue = Math.Abs(dt.Rows[i]["fun_amtReceived"].ObjToDouble());
                            if (dValue == 0D)
                            {
                                if (funeral.IndexOf("OS") == 0 || funeral.IndexOf("O/S") == 0)
                                    dValue = Math.Abs(dt.Rows[i]["value"].ObjToDouble());
                                if (dValue == 0D && value != 0D)
                                    dValue = value;
                            }

                            totalValue += dValue;
                        }
                    }

                    if (date.Year > 100)
                    {
                        doit = false;
                        if ( (date >= startDate && date <= newStopDate) )
                            doit = true;
                        dValue = dt.Rows[i]["principal"].ObjToDouble();
                        if (dValue == 0D)
                            doit = false;
                        if ( doit )
                        {
                            desc = dt.Rows[i]["desc"].ObjToString();
                            try
                            {
                                dRow["otherFuneral"] = dt.Rows[i]["funeral"].ObjToString();
                                dRow["otherdesc"] = desc;
                                dValue = dt.Rows[i]["received"].ObjToDouble();
                                dValue = dt.Rows[i]["principal"].ObjToDouble() * -1D;
                                if (dValue == 0D && status == "INCLUDE")
                                {
                                    //dValue = Math.Abs(dt.Rows[i]["fun_amtReceived"].ObjToDouble()) * -1D;
                                    //dValue = Math.Abs(dt.Rows[i]["value"].ObjToDouble()) * -1D;
                                    //dValue = value;
                                }
                                dRow["received"] = dValue;
                                dRow["otherContract"] = contractNumber;
                                if (date > stopDate)
                                    dRow["red2"] = "Y";
                            }
                            catch ( Exception ex)
                            {
                            }
                        }
                    }
                    //dRow["otherFuneral"] = dt.Rows[i]["funeral"].ObjToString();
                    //dRow["otherdesc"] = desc;
                    //dValue = dt.Rows[i]["received"].ObjToDouble();
                    //if (dValue == 0D)
                    //{
                    //    //dValue = Math.Abs(dt.Rows[i]["fun_amtReceived"].ObjToDouble()) * -1D;
                    //    dValue = Math.Abs(dt.Rows[i]["value"].ObjToDouble()) * -1D;
                    //}
                    //dRow["received"] = dValue;
                    //dRow["otherContract"] = contractNumber;

                    dt.Rows[i]["received"] = DBNull.Value;
                    dd.Rows.Add(dRow);
                }
                catch ( Exception ex)
                {
                }
            }
            dr = dt.NewRow();
            dt.Rows.InsertAt(dr, firstRow);

            dr = dt.NewRow();
            dr["value"] = totalValue;
            dr["desc"] = "Total DC Cash";
            dt.Rows.InsertAt(dr, firstRow);

            dr = dt.NewRow();
            dt.Rows.InsertAt(dr, firstRow);

            for ( i = dd.Rows.Count - 1; i >= 0; i--)
            {
                dr = dd.Rows[i];
                dRow = dt.NewRow();
                dRow.ItemArray = dr.ItemArray;
                dt.Rows.InsertAt(dRow, firstRow);
            }

            totalValue = 0D;
            dd.Rows.Clear();

            dValue = 0D;

            DataView tempview = ddx.DefaultView;
            tempview.Sort = "date DESC";
            ddx = tempview.ToTable();

            double paidTotal = 0D;
            dRow = dt.NewRow();
            dt.Rows.InsertAt(dRow, firstRow);

            for (i = 0; i < ddx.Rows.Count; i++)
            {
                dValue = ddx.Rows[i]["value"].ObjToDouble();
                dValue = ddx.Rows[i]["fun_amtReceived"].ObjToDouble();
                paidTotal += dValue * -1D;
            }

            //dRow = dt.NewRow();
            //dRow["value"] = paidTotal;
            //dRow["red1"] = "Y";
            //dRow["desc"] = "Total DC Paid";
            //dt.Rows.InsertAt(dRow, firstRow);

            dRow = dt.NewRow();
            dt.Rows.InsertAt(dRow, firstRow);

            for ( i = 0; i < ddx.Rows.Count; i++)
            {
                dRow = dt.NewRow();
                dRow["contract"] = ddx.Rows[i]["contract"].ObjToString();
                dRow["cashPaid1"] = "DC Paid";
                desc = ddx.Rows[i]["desc"].ObjToString();
                dRow["desc"] = desc;
                funeral = ddx.Rows[i]["funeral"].ObjToString().ToUpper();
                dRow["funeral"] = ddx.Rows[i]["funeral"].ObjToString();
                date = ddx.Rows[i]["date"].ObjToDateTime();
                dRow["date"] = G1.DTtoMySQLDT(date.ObjToDateTime().ToString("yyyy-MM-dd"));
                dValue = ddx.Rows[i]["value"].ObjToDouble();
                dValue = ddx.Rows[i]["fun_amtReceived"].ObjToDouble();
                if (dValue == 0D)
                {
                    if (funeral.IndexOf("OS") == 0 || funeral.IndexOf("O/S") == 0)
                        dValue = Math.Abs(dt.Rows[i]["value"].ObjToDouble());
                }
                dRow["value"] = dValue * -1D;
                paidTotal = dValue * -1D;
                dRow["red1"] = "Y";
                dt.Rows.InsertAt(dRow, firstRow);
            }

            dRow = dt.NewRow();
            dt.Rows.InsertAt(dRow, firstRow);

            double cashTotal = 0D;
            for (i = 0; i < ddx.Rows.Count; i++)
            {
                //cashTotal += ddx.Rows[i]["value"].ObjToDouble();
                funeral = ddx.Rows[i]["funeral"].ObjToString().ToUpper();
                dValue = ddx.Rows[i]["fun_amtReceived"].ObjToDouble();
                if (dValue == 0D)
                {
                    if (funeral.IndexOf("OS") == 0 || funeral.IndexOf("O/S") == 0)
                        dValue = Math.Abs(dt.Rows[i]["value"].ObjToDouble());
                }
                cashTotal += dValue;
            }

            //dRow = dt.NewRow();
            //dRow["value"] = cashTotal;
            //dRow["red1"] = "Y";
            //dRow["desc"] = "Total DC Cash";
            //dt.Rows.InsertAt(dRow, firstRow);

            dRow = dt.NewRow();
            dt.Rows.InsertAt(dRow, firstRow);

            for (i = 0; i < ddx.Rows.Count; i++)
            {
                dRow = dt.NewRow();
                dRow["contract"] = ddx.Rows[i]["contract"].ObjToString();
                dRow["cashPaid1"] = "DC Cash";
                desc = ddx.Rows[i]["desc"].ObjToString();
                dRow["desc"] = desc;
                funeral = ddx.Rows[i]["funeral"].ObjToString().ToUpper();
                dRow["funeral"] = ddx.Rows[i]["funeral"].ObjToString();
                date = ddx.Rows[i]["date"].ObjToDateTime();
                dRow["date"] = G1.DTtoMySQLDT(date.ObjToDateTime().ToString("yyyy-MM-dd"));
                //dRow["value"] = ddx.Rows[i]["value"].ObjToDouble();
                dValue = ddx.Rows[i]["fun_amtReceived"].ObjToDouble();
                if (dValue == 0D)
                {
                    if (funeral.IndexOf("OS") == 0 || funeral.IndexOf("O/S") == 0)
                        dValue = Math.Abs(dt.Rows[i]["value"].ObjToDouble());
                }
                dRow["value"] = dValue;
                cashTotal += dRow["value"].ObjToDouble();
                dRow["red1"] = "Y";
                dt.Rows.InsertAt(dRow, firstRow);
            }

            dRow = dt.NewRow();
            dt.Rows.InsertAt(dRow, firstRow);

            if (redTotal != 0D)
            {
                int maxRow = dt.Rows.Count;

                dr = dt.NewRow();
                dt.Rows.Add(dr);

                dr = dt.NewRow();
                dr["value"] = redTotal;
                dr["cashPaid1"] = "Red Total";
                dr["cashPaid1"] = "Paid Next Month";
                dr["red1"] = "Y";
                dt.Rows.Add(dr);

                dr = dt.NewRow();
                dt.Rows.Add(dr);

                for (i = maxRow - 1; i >= 0; i--)
                {
                    try
                    {
                        status = dt.Rows[i]["rp"].ObjToString();
                        if (status != "XX")
                        {
                            dr = dt.NewRow();
                            dt.Rows.InsertAt(dr, i + 1);

                            dr = dt.NewRow();
                            dr["value"] = totalTotal;
                            dr["cashPaid1"] = "Black Total";
                            dr["cashPaid1"] = "Paid Current Month";
                            dt.Rows.InsertAt(dr, i + 1);

                            dr = dt.NewRow();
                            dt.Rows.InsertAt(dr, i + 1);
                            break;
                        }
                    }
                    catch ( Exception ex)
                    {
                    }
                }
            }
            else
            {
                dr = dt.NewRow();
                dt.Rows.Add(dr);

                dr = dt.NewRow();
                dr["value"] = totalTotal;
                dr["cashPaid1"] = "Black Total";
                dr["cashPaid1"] = "Paid Current Month";
                dt.Rows.Add(dr);

                dr = dt.NewRow();
                dt.Rows.Add(dr);
            }

            gridMain2.Columns["cashPaid1"].Caption = " ";
            gridMain2.Columns["cashPaid2"].Caption = " ";
            gridMain2.Columns["junk1"].Caption = " ";
            gridMain2.Columns["junk2"].Caption = " ";
            gridMain2.Columns["junk3"].Caption = " ";
            gridMain2.Columns["sn1"].Caption = " ";
            gridMain2.Columns["sn2"].Caption = " ";

            gridMain2.Columns["otherContract"].Visible = false;
            gridMain2.Columns["dateReceived"].Visible = false;
            gridMain2.Columns["reportDate"].Visible = false;

            ClearAllPositions(gridMain2);

            i = 1;

            G1.SetColumnPosition(gridMain2, "num", i++);
            G1.SetColumnPosition(gridMain2, "month", i++);
            //G1.SetColumnPosition(gridMain2, "junk1", i++);
            G1.SetColumnPosition(gridMain2, "value", i++);
            G1.SetColumnPosition(gridMain2, "cashPaid1", i++);
            G1.SetColumnPosition(gridMain2, "desc", i++);
            G1.SetColumnPosition(gridMain2, "date", i++);
            G1.SetColumnPosition(gridMain2, "contract", i++);
            G1.SetColumnPosition(gridMain2, "funeral", i++);

            G1.SetColumnPosition(gridMain2, "junk1", i++);
            //G1.SetColumnPosition(gridMain2, "junk2", i++);
            //G1.SetColumnPosition(gridMain2, "sn1", i++);
            G1.SetColumnPosition(gridMain2, "otherdesc", i++);
            G1.SetColumnPosition(gridMain2, "received", i++);
            G1.SetColumnPosition(gridMain2, "sn2", i++);
            //G1.SetColumnPosition(gridMain2, "cashPaid2", i++);
            //G1.SetColumnPosition(gridMain2, "dateReceived", i++);
            //G1.SetColumnPosition(gridMain2, "otherContract", i++);
            G1.SetColumnPosition(gridMain2, "otherFuneral", i++);

            return dt;
        }
        /****************************************************************************************/
        private DataTable LoadMainLineEdits(DataTable dx, DataTable dd)
        {
            string trustCompany = "";
            double smfsBalance = 0D;
            double balance = 0D;
            double ftBalance = 0D;
            string desc = "";
            string otherdesc = "";
            string insuredName = "";
            string contractNumber = "";
            string funeral = "";
            string preOrPost = "";
            string record = "";
            DateTime date = DateTime.Now;
            DateTime dateReceived = DateTime.Now;
            int row = 0;

            DataRow dRow = null;
            DataRow[] dRows = null;

            for (int i = 0; i < dd.Rows.Count; i++)
            {
                record = dd.Rows[i]["record"].ObjToString();
                trustCompany = dd.Rows[i]["trustName"].ObjToString();
                desc = dd.Rows[i]["lastName"].ObjToString();
                otherdesc = dd.Rows[i]["firstName"].ObjToString();
                insuredName = dd.Rows[i]["insuredName"].ObjToString();
                if (!String.IsNullOrWhiteSpace(insuredName))
                    desc = insuredName;
                smfsBalance = dd.Rows[i]["beginningDeathBenefit"].ObjToDouble();
                balance = dd.Rows[i]["beginningPaymentBalance"].ObjToDouble();
                balance = dd.Rows[i]["payments"].ObjToDouble();
                ftBalance = dd.Rows[i]["endingPaymentBalance"].ObjToDouble();
                row = dd.Rows[i]["position"].ObjToInt32();
                contractNumber = dd.Rows[i]["contractNumber"].ObjToString();

                preOrPost = dd.Rows[i]["preOrPost"].ObjToString();

                date = dd.Rows[i]["date"].ObjToDateTime();
                dateReceived = date;
                funeral = "";
                dRows = dx.Select("contract='" + contractNumber + "' AND funeral<> ''");
                if (dRows.Length > 0)
                {
                    funeral = dRows[0]["funeral"].ObjToString();
                    dateReceived = dRows[0]["dateReceived"].ObjToDateTime();
                }

                dRow = dx.NewRow();
                dRow["record"] = record.ObjToInt32();
                dRow["trust"] = trustCompany;
                dRow["trustName"] = trustCompany;
                dRow["preOrPost"] = preOrPost;
                dRow["contractNumber"] = contractNumber;
                dRow["contract"] = contractNumber;
                dRow["funeral"] = funeral;
                dRow["date"] = G1.DTtoMySQLDT(date.ToString("yyyy-MM-dd"));
                dRow["dateReceived"] = G1.DTtoMySQLDT(dateReceived.ToString("yyyy-MM-dd"));
                dRow["desc"] = desc;
                dRow["insuredName"] = desc;
                dRow["smfsBalance"] = smfsBalance;
                dRow["value"] = balance;
                dRow["received"] = ftBalance;
                dRow["manual"] = "Y";
                dRow["status"] = "Main Line Edit";

                if (G1.get_column_number(dx, "otherdesc") > 0)
                    dRow["otherdesc"] = otherdesc;

                dx.Rows.Add(dRow);
            }
            return dx;
        }
        /****************************************************************************************/
        private DataTable LoadLineEdits(DataTable dx, DataTable dd)
        {
            string trustCompany = "";
            double smfsBalance = 0D;
            double balance = 0D;
            double ftBalance = 0D;
            double dValue = 0D;
            string desc = "";
            string otherdesc = "";
            string record = "";
            DateTime date = DateTime.Now;
            int row = 0;

            DateTime extraDate = DateTime.Now;
            DateTime otherDate = DateTime.Now;
            string contractNumber = "";
            string otherContract = "";
            string funeral = "";
            string otherFuneral = "";

            DataRow dRow = null;

            for (int i = 0; i < dd.Rows.Count; i++)
            {
                record = dd.Rows[i]["record"].ObjToString();
                trustCompany = dd.Rows[i]["trustName"].ObjToString();
                desc = dd.Rows[i]["lastName"].ObjToString();
                otherdesc = dd.Rows[i]["firstName"].ObjToString();
                smfsBalance = dd.Rows[i]["beginningDeathBenefit"].ObjToDouble();
                balance = dd.Rows[i]["beginningPaymentBalance"].ObjToDouble();
                ftBalance = dd.Rows[i]["endingPaymentBalance"].ObjToDouble();

                if (workReport == "Post 2002 Report - CD")
                {
                    dValue = smfsBalance;
                    ftBalance = smfsBalance;
                    smfsBalance = dValue;
                }
                else if (workReport == "Post 2002 Report - Unity")
                {
                    dValue = smfsBalance;
                    ftBalance = smfsBalance;
                    smfsBalance = dValue;
                }
                else if (workReport == "Post 2002 Report - FDLIC")
                {
                    dValue = smfsBalance;
                    ftBalance = smfsBalance;
                    smfsBalance = dValue;
                }
                else if (workReport == "Post 2002 Report - SN & FT")
                {
                    dValue = smfsBalance;
                    ftBalance = smfsBalance;
                    smfsBalance = dValue;
                    smfsBalance = 0D;
                }


                extraDate = dd.Rows[i]["deathPaidDate"].ObjToDateTime();
                contractNumber = dd.Rows[i]["contractNumber"].ObjToString();
                funeral = dd.Rows[i]["statusReason"].ObjToString();

                otherDate = dd.Rows[i]["middleName"].ObjToDateTime();
                otherContract = dd.Rows[i]["billingReason"].ObjToString();
                otherFuneral = dd.Rows[i]["policyStatus"].ObjToString();

                row = dd.Rows[i]["position"].ObjToInt32();


                date = dd.Rows[i]["date"].ObjToDateTime();

                dRow = dx.NewRow();
                dRow["record"] = record.ObjToInt32();
                dRow["trust"] = trustCompany;
                dRow["date"] = G1.DTtoMySQLDT(date.ToString("yyyy-MM-dd"));
                if ( extraDate.Year > 100 )
                    dRow["date"] = G1.DTtoMySQLDT(extraDate.ToString("yyyy-MM-dd"));
                if ( !String.IsNullOrWhiteSpace ( contractNumber ))
                    dRow["contract"] = contractNumber;
                if (!String.IsNullOrWhiteSpace(funeral))
                    dRow["funeral"] = funeral;
                dRow["desc"] = desc;
                //dRow["smfsBalance"] = smfsBalance;
                dRow["value"] = balance;
                dRow["received"] = ftBalance;
                dRow["manual"] = "Y";
                dRow["status"] = "Line Edit";

                if (G1.get_column_number(dx, "otherdesc") > 0)
                    dRow["otherdesc"] = otherdesc;
                if (G1.get_column_number(dx, "otherContract") > 0)
                    dRow["otherContract"] = otherContract;
                if (G1.get_column_number(dx, "otherFuneral") > 0)
                    dRow["otherFuneral"] = otherFuneral;
                if (G1.get_column_number(dx, "dateReceived") > 0 && otherDate.Year > 100 )
                    dRow["dateReceived"] = G1.DTtoMySQLDT ( otherDate.ToString("yyyy-MM-dd"));

                dx.Rows.InsertAt(dRow, row);
            }
            return dx;
        }
        /****************************************************************************************/
        private DataTable LoadLineEditsPre(DataTable dx, DataTable dd)
        {
            string trustCompany = "";

            //double Forethought = dr["Forethought"].ObjToDouble();
            //double SecurityNational = dr["Security National"].ObjToDouble();
            //double FdlicOldWebb = dr["FDLIC Old Webb"].ObjToDouble();
            //double FdlicOldCCI = dr["FDLIC Old CCI"].ObjToDouble();
            //double UnityOldBarham = dr["Unity Old Barham"].ObjToDouble();
            //double UnityOldWebb = dr["Unity Old Webb"].ObjToDouble();

            double Forethought = 0D;
            double SecurityNational = 0D;
            double FdlicOldWebb = 0D;
            double FdlicOldCCI = 0D;
            double UnityOldBarham = 0D;
            double UnityOldWebb = 0D;

            string snDesc = "";
            string ftDesc = "";

            double smfsBalance = 0D;
            double balance = 0D;
            double ftBalance = 0D;
            double dValue = 0D;
            string desc = "";
            string otherdesc = "";
            string record = "";
            DateTime date = DateTime.Now;
            int row = 0;

            DateTime extraDate = DateTime.Now;
            DateTime otherDate = DateTime.Now;
            string contractNumber = "";
            string otherContract = "";
            string funeral = "";
            string otherFuneral = "";

            DataRow dRow = null;

            for (int i = 0; i < dd.Rows.Count; i++)
            {
                record = dd.Rows[i]["record"].ObjToString();
                trustCompany = dd.Rows[i]["trustName"].ObjToString();
                desc = dd.Rows[i]["lastName"].ObjToString();
                otherdesc = dd.Rows[i]["firstName"].ObjToString();

                //G1.update_db_table("trust_data_edits", "record", record, new string[] { "beginningPaymentBalance", Forethought.ToString(), "beginningDeathBenefit", SecurityNational.ToString(), "endingPaymentBalance", FdlicOldWebb.ToString(), "endingDeathBenefit", FdlicOldCCI.ToString(), "priorUnappliedCash", UnityOldBarham.ToString(), "currentUnappliedCash", UnityOldWebb.ToString(), "date", date.ToString("yyyy-MM-dd"), "trustName", trustCompany }); ;

                Forethought = dd.Rows[i]["beginningPaymentBalance"].ObjToDouble();
                SecurityNational = dd.Rows[i]["beginningDeathBenefit"].ObjToDouble();
                FdlicOldWebb = dd.Rows[i]["endingPaymentBalance"].ObjToDouble();
                FdlicOldCCI = dd.Rows[i]["endingDeathBenefit"].ObjToDouble();
                UnityOldBarham = dd.Rows[i]["priorUnappliedCash"].ObjToDouble();
                UnityOldWebb = dd.Rows[i]["currentUnappliedCash"].ObjToDouble();

                snDesc = dd.Rows[i]["lastName"].ObjToString();
                ftDesc = dd.Rows[i]["firstName"].ObjToString();

                //if (snDesc.ToUpper().IndexOf("PD") == 0)
                //    continue;
                //if (ftDesc.ToUpper().IndexOf("PD") == 0)
                //    continue;

                row = dd.Rows[i]["position"].ObjToInt32();

                date = dd.Rows[i]["date"].ObjToDateTime();

                dRow = dx.NewRow();
                dRow["record"] = record.ObjToInt32();
                dRow["trust"] = trustCompany;
                dRow["date"] = G1.DTtoMySQLDT(date.ToString("yyyy-MM-dd"));
                if (extraDate.Year > 100)
                    dRow["date"] = G1.DTtoMySQLDT(extraDate.ToString("yyyy-MM-dd"));
                if (!String.IsNullOrWhiteSpace(contractNumber))
                    dRow["contract"] = contractNumber;
                if (!String.IsNullOrWhiteSpace(funeral))
                    dRow["funeral"] = funeral;
                dRow["desc"] = desc;

                dRow["Forethought"] = Forethought;
                if (Forethought == 0D)
                    dRow["Forethought"] = DBNull.Value;

                dRow["Security National"] = SecurityNational;
                if ( SecurityNational == 0D )
                    dRow["Security National"] = DBNull.Value;

                dRow["FDLIC Old Webb"] = FdlicOldWebb;
                if ( FdlicOldWebb == 0D )
                    dRow["FDLIC Old Webb"] = DBNull.Value;

                dRow["FDLIC Old CCI"] = FdlicOldCCI;
                if (FdlicOldCCI == 0D)
                    dRow["FDLIC Old CCI"] = DBNull.Value;

                dRow["Unity Old Barham"] = UnityOldBarham;
                if ( UnityOldBarham == 0D )
                    dRow["Unity Old Barham"] = DBNull.Value;

                dRow["Unity Old Webb"] = UnityOldWebb;
                if ( UnityOldWebb == 0D )
                    dRow["Unity Old Webb"] = DBNull.Value;

                if (G1.get_column_number(dx, "Security National Desc") > 0)
                    dRow["Security National Desc"] = snDesc;
                if (G1.get_column_number(dx, "Forethought Desc") > 0)
                    dRow["Forethought Desc"] = ftDesc;

                dRow["manual"] = "Y";
                dRow["status"] = "Line Edit";

                if (G1.get_column_number(dx, "otherdesc") > 0)
                    dRow["otherdesc"] = otherdesc;
                if (G1.get_column_number(dx, "otherContract") > 0)
                    dRow["otherContract"] = otherContract;
                if (G1.get_column_number(dx, "otherFuneral") > 0)
                    dRow["otherFuneral"] = otherFuneral;
                if (G1.get_column_number(dx, "dateReceived") > 0 && otherDate.Year > 100)
                    dRow["dateReceived"] = G1.DTtoMySQLDT(otherDate.ToString("yyyy-MM-dd"));

                dx.Rows.InsertAt(dRow, row);
            }
            return dx;
        }
        /****************************************************************************************/
        private DataRow FindRecord ( DataTable dx, string record )
        {
            DataRow dRow = null;
            DataRow[] dRows = dx.Select("Convert([record],System.String) = '" + record + "'");
            if (dRows.Length > 0)
                dRow = dRows[0];
            else
            {
                string rec = "";
                for ( int i=0; i<dx.Rows.Count; i++)
                {
                    rec = dx.Rows[i]["record"].ObjToString();
                    if ( rec == record )
                    {
                        dRow = dx.Rows[i];
                        break;
                    }
                }
            }
            return dRow;
        }
        /****************************************************************************************/
        private DataTable LoadLineEditsPreCD(DataTable dx, DateTime date )
        {
            string who = "CD";
            string startDate = this.dateTimePicker1.Value.ToString("yyyy-MM-dd");
            string stopDate = this.dateTimePicker2.Value.ToString("yyyy-MM-dd");
            string cmd = "Select * from `trust_data_edits` WHERE `trustName` = 'Pre2002' AND `status` = 'Line Edit' AND `date` >= '" + startDate + "' AND `date` <= '" + stopDate + "' AND `preOrPost` = 'Pre' ";
            cmd += " AND `billingReason` = '" + who + "' ;";
            DataTable dd = G1.get_db_data(cmd);

            string trustCompany = "";

            double CD = 0D;
            double bankCD = 0D;

            string desc = "";
            string otherDesc = "";
            string record = "";
            int row = 0;

            DateTime extraDate = DateTime.Now;
            DateTime otherDate = DateTime.Now;

            string contractNumber = "";
            string otherContract = "";
            string funeral = "";
            string otherFuneral = "";

            DataRow dRow = null;
            DataRow[] dRows = null;
            bool foundRecord = false;

            for (int i = 0; i < dd.Rows.Count; i++)
            {
                foundRecord = false;
                record = dd.Rows[i]["record"].ObjToString();
                trustCompany = dd.Rows[i]["trustName"].ObjToString();
                desc = dd.Rows[i]["insuredName"].ObjToString();
                otherDesc = dd.Rows[i]["middleName"].ObjToString();
                CD = dd.Rows[i]["downPayments"].ObjToDouble();
                bankCD = dd.Rows[i]["growth"].ObjToDouble();
                date = dd.Rows[i]["date"].ObjToDateTime();

                row = dd.Rows[i]["position"].ObjToInt32();

                date = dd.Rows[i]["date"].ObjToDateTime();

                dRow = FindRecord(dx, record);

                if (dRow == null)
                    dRow = dx.NewRow();
                else
                    foundRecord = true;

                dRow["record"] = record.ObjToInt32();
                dRow["trust"] = trustCompany;
                dRow["date"] = G1.DTtoMySQLDT(date.ToString("yyyy-MM-dd"));
                //if (extraDate.Year > 100)
                //    dRow["date"] = G1.DTtoMySQLDT(extraDate.ToString("yyyy-MM-dd"));
                if (!String.IsNullOrWhiteSpace(contractNumber))
                    dRow["contract"] = contractNumber;
                if (!String.IsNullOrWhiteSpace(funeral))
                    dRow["funeral"] = funeral;
                dRow["desc"] = desc;
                dRow["otherDesc"] = otherDesc;

                dRow["CD"] = CD;
                if (CD == 0D)
                    dRow["CD"] = DBNull.Value;

                dRow["bankCD"] = bankCD;
                if ( bankCD == 0D)
                    dRow["bankCD"] = DBNull.Value;

                dRow["manual"] = "Y";
                dRow["status"] = "Line Edit";

                //if (G1.get_column_number(dx, "otherdesc") > 0)
                //    dRow["otherdesc"] = otherdesc;
                //if (G1.get_column_number(dx, "otherContract") > 0)
                //    dRow["otherContract"] = otherContract;
                //if (G1.get_column_number(dx, "otherFuneral") > 0)
                //    dRow["otherFuneral"] = otherFuneral;
                //if (G1.get_column_number(dx, "dateReceived") > 0 && otherDate.Year > 100)
                //    dRow["dateReceived"] = G1.DTtoMySQLDT(otherDate.ToString("yyyy-MM-dd"));

                if ( !foundRecord )
                    dx.Rows.InsertAt(dRow, row);
            }
            return dx;
        }
        /****************************************************************************************/
        //DataTable BuildTrustList(DataTable dt)
        //{
        //    DataTable dt1 = new DataTable();
        //    dt1.Columns.Add("trust");

        //    DataRow[] dRows = null;
        //    DataRow dR = null;
        //    string trust = "";
        //    for (int i = 0; i < dt.Rows.Count; i++)
        //    {
        //        trust = dt.Rows[i]["trust"].ObjToString().Trim();
        //        if (String.IsNullOrWhiteSpace(trust))
        //            continue;
        //        trust = G1.force_lower_line(trust);
        //        dRows = dt1.Select("trust='" + trust + "'");
        //        if (dRows.Length == 0)
        //        {
        //            dR = dt1.NewRow();
        //            dR["trust"] = trust;
        //            dt1.Rows.Add(dR);
        //        }
        //    }
        //    return dt1;
        //}
        /****************************************************************************************/
        private DataTable LoadLineEditsPre2002(DataTable dx, DateTime date, string who)
        {

            string startDate = this.dateTimePicker1.Value.ToString("yyyy-MM-dd");
            string stopDate = this.dateTimePicker2.Value.ToString("yyyy-MM-dd");
            string cmd = "Select * from `trust_data_edits` WHERE `trustName` = 'Pre2002' AND `status` = 'Line Edit' AND `date` >= '" + startDate + "' AND `date` <= '" + stopDate + "' AND `preOrPost` = 'Pre' ";
            cmd += " AND `billingReason` = '" + who + "' ;";
            DataTable dd = G1.get_db_data(cmd);
            if (dd.Rows.Count <= 0)
                return dx;

            string trustCompany = "";

            double CD = 0D;
            double bankCD = 0D;

            string desc = "";
            string otherDesc = "";
            string record = "";
            int row = 0;

            DateTime extraDate = DateTime.Now;
            DateTime otherDate = DateTime.Now;

            string contractNumber = "";
            string otherContract = "";
            string funeral = "";
            string otherFuneral = "";

            DataRow dRow = null;
            DataRow[] dRows = null;
            bool foundRecord = false;

            for (int i = 0; i < dd.Rows.Count; i++)
            {
                foundRecord = false;
                record = dd.Rows[i]["record"].ObjToString();
                trustCompany = dd.Rows[i]["trustName"].ObjToString();
                desc = dd.Rows[i]["insuredName"].ObjToString();
                otherDesc = dd.Rows[i]["middleName"].ObjToString();
                CD = dd.Rows[i]["downPayments"].ObjToDouble();
                bankCD = dd.Rows[i]["growth"].ObjToDouble();
                date = dd.Rows[i]["date"].ObjToDateTime();

                row = dd.Rows[i]["position"].ObjToInt32();

                date = dd.Rows[i]["date"].ObjToDateTime();

                dRow = FindRecord(dx, record);

                if (dRow == null)
                    dRow = dx.NewRow();
                else
                    foundRecord = true;

                dRow["record"] = record.ObjToInt32();
                dRow["trust"] = trustCompany;
                dRow["date"] = G1.DTtoMySQLDT(date.ToString("yyyy-MM-dd"));
                //if (extraDate.Year > 100)
                //    dRow["date"] = G1.DTtoMySQLDT(extraDate.ToString("yyyy-MM-dd"));
                if (!String.IsNullOrWhiteSpace(contractNumber))
                    dRow["contract"] = contractNumber;
                if (!String.IsNullOrWhiteSpace(funeral))
                    dRow["funeral"] = funeral;
                dRow["desc"] = desc;
                dRow["otherDesc"] = otherDesc;

                dRow[who] = CD;
                if (CD == 0D)
                    dRow[who] = DBNull.Value;

                dRow["bankCD"] = bankCD;
                if (bankCD == 0D)
                    dRow["bankCD"] = DBNull.Value;

                dRow["manual"] = "Y";
                dRow["status"] = "Line Edit";

                //if (G1.get_column_number(dx, "otherdesc") > 0)
                //    dRow["otherdesc"] = otherdesc;
                //if (G1.get_column_number(dx, "otherContract") > 0)
                //    dRow["otherContract"] = otherContract;
                //if (G1.get_column_number(dx, "otherFuneral") > 0)
                //    dRow["otherFuneral"] = otherFuneral;
                //if (G1.get_column_number(dx, "dateReceived") > 0 && otherDate.Year > 100)
                //    dRow["dateReceived"] = G1.DTtoMySQLDT(otherDate.ToString("yyyy-MM-dd"));

                if (!foundRecord)
                    dx.Rows.InsertAt(dRow, row);
            }
            return dx;
        }
        /****************************************************************************************/
        DataTable BuildTrustList(DataTable dt)
        {
            DataTable dt1 = new DataTable();
            dt1.Columns.Add("trust");

            DataRow[] dRows = null;
            DataRow dR = null;
            string trust = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                trust = dt.Rows[i]["trust"].ObjToString().Trim();
                if (String.IsNullOrWhiteSpace(trust))
                    continue;
                trust = G1.force_lower_line(trust);
                dRows = dt1.Select("trust='" + trust + "'");
                if (dRows.Length == 0)
                {
                    dR = dt1.NewRow();
                    dR["trust"] = trust;
                    dt1.Rows.Add(dR);
                }
            }
            return dt1;
        }
        /****************************************************************************************/
        private DataTable getOverrunsPre ()
        {
            DateTime startDate = this.dateTimePicker1.Value;
            DateTime stopDate = this.dateTimePicker2.Value;
            DateTime newStopDate = stopDate.AddDays(workNextDays);
            string sDate1 = startDate.ToString("yyyy-MM-dd");
            string sDate2 = stopDate.ToString("yyyy-MM-dd");

            string companies = getCompanyQuery(workCompanies);

            string cmd = "Select * from `trust_data_overruns` t WHERE `date` >= '" + sDate1 + "' AND `date` <= '" + sDate2 + " 23:59:59' ";

            if (workReport == "Pre 2002 Report")
                cmd += " AND t.`preOrPost` = 'Pre' ";
            else
                cmd += " AND t.`preOrPost` <> 'Pre' ";

            if (!String.IsNullOrWhiteSpace(companies))
            {
                string newCompany = companies;
                if (newCompany.IndexOf("FDLIC PB") >= 0)
                    newCompany = newCompany.Replace("'FDLIC PB'", "'FDLIC PB','FDLIC CCI'");
                cmd += " AND " + newCompany + " ";
            }
            cmd += " ORDER by `date`, `trustCompany`;  ";

            DataTable ddx = G1.get_db_data(cmd);
            return ddx;
        }
        /****************************************************************************************/
        private void LoadSplitPre()
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt == null)
                return;

            this.Cursor = Cursors.WaitCursor;

            string trust = "trust";
            if (G1.get_column_number(dt, "trust") < 0)
                trust = "trust";

            int i = 0;

            DataView tempview = dt.DefaultView;
            tempview.Sort = trust;
            DataTable dd = tempview.ToTable();

            DataRow[] dRows = dd.Select("middleName<>'REPLACE'");
            if (dRows.Length > 0)
                dd = dRows.CopyToDataTable();

            //DataTable dt1 = dt.DefaultView.ToTable(true, trust);


            DataTable dt1 = BuildTrustList(dt);


            string trustCompany = "";
            dRows = null;
            DataRow dRow = null;

            DataTable overDt = getOverrunsPre();
            if (overDt.Rows.Count > 0)
            {
                for (i = 0; i < overDt.Rows.Count; i++)
                {
                    trustCompany = overDt.Rows[i]["trustCompany"].ObjToString();
                    if ( !String.IsNullOrWhiteSpace ( trustCompany ))
                    {
                        dRows = dt1.Select("trust='" + trustCompany + "'");
                        if ( dRows.Length <= 0 )
                        {
                            dRow = dt1.NewRow();
                            dRow["trust"] = trustCompany;
                            dt1.Rows.Add(dRow);
                        }
                    }
                }
            }
            DataTable[] mainDts = new DataTable[7];
            int dtCount = 0;
            for (i = 0; i < dt1.Rows.Count; i++)
            {
                trustCompany = dt1.Rows[i][trust].ObjToString();
                if (String.IsNullOrWhiteSpace(trustCompany))
                    continue;
                dRows = dt.Select("trust='" + trustCompany + "'");
                if (dRows.Length > 0)
                {
                    mainDts[dtCount] = dRows.CopyToDataTable();
                    dtCount++;
                }
            }

            DataTable tempDt = null;
            int maxRow = 0;
            int replaceRows = 0;
            dRows = dt.Select("middleName='REPLACE'");
            if (dRows.Length > 0)
                replaceRows = dRows.Length;

            for (int j = 0; j < dtCount; j++)
            {
                tempDt = mainDts[j];
                if (tempDt == null)
                    continue;
                if ((tempDt.Rows.Count*3+5) > maxRow)
                    maxRow = (tempDt.Rows.Count*3)+5;
            }

            if ( maxRow-replaceRows > 0 )
                maxRow = maxRow - replaceRows;

            dRow = null;
            DataTable dx = CreateTempDt();
            dx.Columns.Add("status");

            //dx.Columns.Add("otherdesc");
            //dx.Columns.Add("otherContract");
            //dx.Columns.Add("otherFuneral");
            //dx.Columns.Add("othertrust");

            int firstRow = 1;
            for (i = 0; i < maxRow; i++)
            {
                dRow = dx.NewRow();
                dx.Rows.Add(dRow);
                if (firstRow < 0)
                    firstRow = dx.Rows.Count - 1;
            }

            dd = null;
            string cmd = "";
            for (i = 0; i < mainDts.Length; i++)
            {
                if (mainDts[i] == null)
                    continue;
                if (mainDts[i].Rows.Count <= 0)
                    continue;
                trustCompany = mainDts[i].Rows[0]["trust"].ObjToString();
                if (trustCompany.ToUpper() == "FORETHOUGHT")
                    trustCompany = G1.force_lower_line(trustCompany);
                else if (trustCompany.ToUpper() == "SECURITY NATIONAL")
                    trustCompany = G1.force_lower_line(trustCompany);
                if (trustCompany.ToUpper() != "FDLIC OLD WEBB" && trustCompany.ToUpper() != "FDLIC OLD CCI" &&
                     trustCompany.ToUpper() != "UNITY OLD BARHAM" && trustCompany.ToUpper() != "UNITY OLD WEBB" &&
                     trustCompany.ToUpper() != "FORETHOUGHT" && trustCompany.ToUpper() != "SECURITY NATIONAL" &&
                     trustCompany.ToUpper() != "CD" )
                    continue;
                if (G1.get_column_number(dx, trustCompany + "Desc") < 0)
                {
                    AddNewColumn(trustCompany, trustCompany, 100, gridMain6);
                    gridMain6.Columns[trustCompany].AppearanceCell.TextOptions.HAlignment = HorzAlignment.Far;
                    AddNewColumn(trustCompany + " desc", "Description", 150, gridMain6);
                    AddNewColumn(trustCompany + " date", "Date", 100, gridMain6);
                    AddNewColumn(trustCompany + " contract", "Contract #", 100, gridMain6);
                    AddNewColumn(trustCompany + " funeral", "Funeral #", 100, gridMain6);

                    dx.Columns.Add(trustCompany);
                    dx.Columns.Add(trustCompany + " desc");
                    dx.Columns.Add(trustCompany + " date");
                    dx.Columns.Add(trustCompany + " contract");
                    dx.Columns.Add(trustCompany + " funeral");
                    dx.Columns.Add(trustCompany + " redx");
                }
                dd = mainDts[i].Copy();

                dRows = dd.Select("middleName='REPLACE'");
                if (dRows.Length > 0)
                {
                    for (int kk = 0; kk < dRows.Length; kk++)
                        dd.Rows.Remove(dRows[kk]);
                }

                //dx = LoadOverruns(dx, overDt, firstRow );

                double totalCash = 0D;
                double totalPaid = 0D;
                double principal = 0D;
                string middleName = "";
                string name = "";
                DateTime dateReceived = DateTime.Now;
                int rows = 0;
                int nRows = 0;
                for (int kk = 0; kk < 3; kk++)
                {
                    nRows = 0;
                    for (int j = 0; j < dd.Rows.Count; j++)
                    {
                        middleName = dd.Rows[j]["middleName"].ObjToString().ToUpper();
                        name = dd.Rows[j]["insuredName"].ObjToString();
                        //if (middleName == "REPLACE")
                        //{
                        //    rows--;
                        //    continue;
                        //}

                        principal = dd.Rows[j]["principal"].ObjToDouble();
                        if (principal == 0D)
                            principal = dd.Rows[j]["value"].ObjToDouble();
                        dateReceived = dd.Rows[j]["dateReceived"].ObjToDateTime();
                        if (dateReceived > this.dateTimePicker2.Value)
                        {
                            if (kk != 2)
                            {
                                nRows++;
                                continue;
                            }
                            dx.Rows[j + rows][trustCompany + " redx"] = "Y";
                        }

                        dx.Rows[j+rows][trustCompany] = G1.ReformatMoney(principal);

                        dx.Rows[j + rows][trustCompany + " date"] = G1.DTtoMySQLDT(dd.Rows[j]["date"].ObjToDateTime().ToString("yyyy-MM-dd"));
                        //dx.Rows[j]["received"] = dd.Rows[j]["value"].ObjToDouble();
                        dx.Rows[j + rows][trustCompany + " funeral"] = dd.Rows[j]["funeral"].ObjToString();
                        dx.Rows[j + rows][trustCompany + " contract"] = dd.Rows[j]["contract"].ObjToString();
                        dx.Rows[j + rows][trustCompany + " desc"] = dd.Rows[j]["desc"].ObjToString();
                        //dx.Rows[j + rows]["status"] = dd.Rows[j]["status"].ObjToString();
                        if (i == 0)
                            dx.Rows[j + rows]["firstName"] = dd.Rows[j]["status"].ObjToString();
                        else
                            dx.Rows[j + rows]["lastName"] = dd.Rows[j]["status"].ObjToString();


                        if (kk == 0)
                        {
                            dx.Rows[j + rows][trustCompany + " desc"] = "DC CASH-" + dd.Rows[j]["desc"].ObjToString();
                            totalCash += principal;
                        }
                        else if (kk == 1)
                        {
                            dx.Rows[j + rows][trustCompany + " desc"] = "DC PAID-" + dd.Rows[j]["desc"].ObjToString();
                            totalPaid += principal;
                        }
                    }
                    rows = rows + dd.Rows.Count - nRows;
                    if (dd.Rows.Count > 1)
                    {
                        if ((dd.Rows.Count - nRows) > 1)
                        {
                            if (kk == 0)
                            {
                                dx.Rows[rows][trustCompany] = totalCash;
                                dx.Rows[rows][trustCompany + " desc"] = "Total DC Cash";
                            }
                            else if (kk == 1)
                            {
                                dx.Rows[rows][trustCompany] = totalPaid;
                                dx.Rows[rows][trustCompany + " desc"] = "Total DC Paid";
                            }
                            rows = rows + 1;
                        }
                    }
                    rows = rows + 1;
                }
            }

            ClearAllPositions(gridMain6);

            i = 1;
            G1.SetColumnPosition(gridMain6, "num", i++);
            G1.SetColumnPosition(gridMain6, "month", i++);
            G1.SetColumnPosition(gridMain6, "junk1", i++);
            dx.Columns.Add("junk1");
            dx.Columns.Add("junk2");
            dx.Columns.Add("junk3");
            dx.Columns.Add("junk4");
            dx.Columns.Add("junk5");
            dx.Columns.Add("junk6");

            int mainWidth = 85;


            trustCompany = "CD";
            if (G1.get_column_number(dx, trustCompany) >= 0)
            {
                G1.SetColumnPosition(gridMain6, trustCompany, i++);
                G1.SetColumnPosition(gridMain6, trustCompany + " desc", i++);
                G1.SetColumnPosition(gridMain6, trustCompany + " date", i++);
                G1.SetColumnPosition(gridMain6, trustCompany + " funeral", i++);
                G1.SetColumnPosition(gridMain6, trustCompany + " contract", i++);
            }
            else
            {
                AddNewColumn(trustCompany, trustCompany, mainWidth, gridMain6);
                gridMain6.Columns[trustCompany].AppearanceCell.TextOptions.HAlignment = HorzAlignment.Far;
                dx.Columns.Add(trustCompany);
                G1.SetColumnPosition(gridMain6, trustCompany, i++);
                gridMain6.Columns[trustCompany].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Custom;
                gridMain6.Columns[trustCompany].SummaryItem.DisplayFormat = "{0:N2}";

                dx.Columns.Add("bankCD");

            }

            trustCompany = "FDLIC Old Webb";
            if (G1.get_column_number(dx, trustCompany) >= 0)
            {
                G1.SetColumnPosition(gridMain6, trustCompany, i++);
                G1.SetColumnPosition(gridMain6, trustCompany + " desc", i++);
                G1.SetColumnPosition(gridMain6, trustCompany + " date", i++);
                G1.SetColumnPosition(gridMain6, trustCompany + " funeral", i++);
                G1.SetColumnPosition(gridMain6, trustCompany + " contract", i++);
            }
            else
            {
                AddNewColumn(trustCompany, trustCompany, mainWidth, gridMain6);
                gridMain6.Columns[trustCompany].AppearanceCell.TextOptions.HAlignment = HorzAlignment.Far;
                dx.Columns.Add(trustCompany);
                G1.SetColumnPosition(gridMain6, trustCompany, i++);
                gridMain6.Columns[trustCompany].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Custom;
                gridMain6.Columns[trustCompany].SummaryItem.DisplayFormat = "{0:N2}";
            }

            trustCompany = "FDLIC Old CCI";
            if (G1.get_column_number(dx, trustCompany) >= 0)
            {
                G1.SetColumnPosition(gridMain6, trustCompany, i++);
                G1.SetColumnPosition(gridMain6, trustCompany + " desc", i++);
                G1.SetColumnPosition(gridMain6, trustCompany + " date", i++);
                G1.SetColumnPosition(gridMain6, trustCompany + " funeral", i++);
                G1.SetColumnPosition(gridMain6, trustCompany + " contract", i++);
            }
            else
            {
                AddNewColumn(trustCompany, trustCompany, mainWidth, gridMain6);
                gridMain6.Columns[trustCompany].AppearanceCell.TextOptions.HAlignment = HorzAlignment.Far;
                dx.Columns.Add(trustCompany);
                G1.SetColumnPosition(gridMain6, trustCompany, i++);
                gridMain6.Columns[trustCompany].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Custom;
                gridMain6.Columns[trustCompany].SummaryItem.DisplayFormat = "{0:N2}";
            }

            trustCompany = "Unity Old Barham";
            if (G1.get_column_number(dx, trustCompany) >= 0)
            {
                G1.SetColumnPosition(gridMain6, trustCompany, i++);
                G1.SetColumnPosition(gridMain6, trustCompany + " desc", i++);
                G1.SetColumnPosition(gridMain6, trustCompany + " date", i++);
                G1.SetColumnPosition(gridMain6, trustCompany + " funeral", i++);
                G1.SetColumnPosition(gridMain6, trustCompany + " contract", i++);
            }
            else
            {
                AddNewColumn(trustCompany, trustCompany, mainWidth, gridMain6);
                gridMain6.Columns[trustCompany].AppearanceCell.TextOptions.HAlignment = HorzAlignment.Far;
                dx.Columns.Add(trustCompany);
                G1.SetColumnPosition(gridMain6, trustCompany, i++);
                gridMain6.Columns[trustCompany].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Custom;
                gridMain6.Columns[trustCompany].SummaryItem.DisplayFormat = "{0:N2}";
            }

            trustCompany = "Unity Old Webb";
            if (G1.get_column_number(dx, trustCompany) >= 0)
            {
                G1.SetColumnPosition(gridMain6, trustCompany, i++);
                gridMain6.Columns[trustCompany].Caption = trustCompany;
                G1.SetColumnPosition(gridMain6, trustCompany + " desc", i++);
                G1.SetColumnPosition(gridMain6, trustCompany + " date", i++);
                G1.SetColumnPosition(gridMain6, trustCompany + " funeral", i++);
                G1.SetColumnPosition(gridMain6, trustCompany + " contract", i++);
            }
            else
            {
                AddNewColumn(trustCompany, trustCompany, mainWidth, gridMain6);
                gridMain6.Columns[trustCompany].AppearanceCell.TextOptions.HAlignment = HorzAlignment.Far;
                dx.Columns.Add(trustCompany);
                G1.SetColumnPosition(gridMain6, trustCompany, i++);
                gridMain6.Columns[trustCompany].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Custom;
                gridMain6.Columns[trustCompany].SummaryItem.DisplayFormat = "{0:N2}";
            }

            G1.SetColumnPosition(gridMain6, "junk2", i++);

            AddNewColumn("junk7", " ", 15, gridMain6);
            G1.SetColumnPosition(gridMain6, "junk7", i++);

            AddNewColumn("junk8", " ", 15, gridMain6);
            G1.SetColumnPosition(gridMain6, "junk8", i++);

            AddNewColumn("junk9", " ", 15, gridMain6);
            G1.SetColumnPosition(gridMain6, "junk9", i++);

            G1.SetColumnPosition(gridMain6, "junk7", i++);
            G1.SetColumnPosition(gridMain6, "junk8", i++);
            G1.SetColumnPosition(gridMain6, "junk9", i++);

            trustCompany = "Forethought";
            if (G1.get_column_number(dx, trustCompany) >= 0)
            {
                G1.SetColumnPosition(gridMain6, trustCompany, i++);
                G1.SetColumnPosition(gridMain6, trustCompany + " desc", i++);
                G1.SetColumnPosition(gridMain6, trustCompany + " date", i++);
                G1.SetColumnPosition(gridMain6, trustCompany + " funeral", i++);
                G1.SetColumnPosition(gridMain6, trustCompany + " contract", i++);
            }
            else
            {
                AddNewColumn(trustCompany, trustCompany, 100, gridMain6);
                gridMain6.Columns[trustCompany].AppearanceCell.TextOptions.HAlignment = HorzAlignment.Far;
                dx.Columns.Add(trustCompany);
                G1.SetColumnPosition(gridMain6, trustCompany, i++);
                //dx.Rows[0]["Forethought"] = " ";
                gridMain6.Columns[trustCompany].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Custom;
                gridMain6.Columns[trustCompany].SummaryItem.DisplayFormat = "{0:N2}";

                AddNewColumn(trustCompany + " desc", "Description", 150, gridMain6);
                gridMain6.Columns[trustCompany + " desc"].AppearanceCell.TextOptions.HAlignment = HorzAlignment.Far;
                dx.Columns.Add(trustCompany + " desc" );
                G1.SetColumnPosition(gridMain6, trustCompany + " desc", i++);

                AddNewColumn(trustCompany + " date", "Date", 100, gridMain6);
                gridMain6.Columns[trustCompany + " date"].AppearanceCell.TextOptions.HAlignment = HorzAlignment.Far;
                dx.Columns.Add(trustCompany + " date");
                G1.SetColumnPosition(gridMain6, trustCompany + " date", i++);

                AddNewColumn(trustCompany + " contract", "Contract #", 100, gridMain6);
                gridMain6.Columns[trustCompany + " contract"].AppearanceCell.TextOptions.HAlignment = HorzAlignment.Far;
                dx.Columns.Add(trustCompany + " contract");
                G1.SetColumnPosition(gridMain6, trustCompany + " contract", i++);

                AddNewColumn(trustCompany + " funeral", "Funeral #", 100, gridMain6);
                gridMain6.Columns[trustCompany + " funeral"].AppearanceCell.TextOptions.HAlignment = HorzAlignment.Far;
                dx.Columns.Add(trustCompany + " funeral");
                G1.SetColumnPosition(gridMain6, trustCompany + " funeral", i++);

                dx.Columns.Add(trustCompany + " redx");
            }

            G1.SetColumnPosition(gridMain6, "junk3", i++);
            G1.SetColumnPosition(gridMain6, "junk4", i++);
            G1.SetColumnPosition(gridMain6, "junk5", i++);
            G1.SetColumnPosition(gridMain6, "junk6", i++);

            trustCompany = "Security National";
            if (G1.get_column_number(dx, trustCompany) >= 0)
            {
                G1.SetColumnPosition(gridMain6, trustCompany, i++);
                G1.SetColumnPosition(gridMain6, trustCompany + " desc", i++);
                G1.SetColumnPosition(gridMain6, trustCompany + " date", i++);
                G1.SetColumnPosition(gridMain6, trustCompany + " funeral", i++);
                G1.SetColumnPosition(gridMain6, trustCompany + " contract", i++);
            }
            else
            {
                AddNewColumn(trustCompany, trustCompany, mainWidth, gridMain6);
                gridMain6.Columns[trustCompany].AppearanceCell.TextOptions.HAlignment = HorzAlignment.Far;
                dx.Columns.Add(trustCompany);
                G1.SetColumnPosition(gridMain6, trustCompany, i++);
                gridMain6.Columns[trustCompany].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Custom;
                gridMain6.Columns[trustCompany].SummaryItem.DisplayFormat = "{0:N2}";
            }

            DateTime date = this.dateTimePicker2.Value;
            DateTime lastDate = date.AddMonths(-1);
            int days = DateTime.DaysInMonth(lastDate.Year, lastDate.Month);
            lastDate = new DateTime(lastDate.Year, lastDate.Month, days);

            for ( i = 0; i < dx.Rows.Count; i++)
            {
                configureCell ( dx, i, "Forethought" );
                configureCell ( dx, i, "Security National" );
                configureCell(dx, i, "FDLIC Old Webb");
                configureCell(dx, i, "FDLIC Old CCI");
                configureCell(dx, i, "Unity Old Barham");
                configureCell(dx, i, "Unity Old Webb");
                //dx.Rows[i]["Forethought"] = dx.Rows[i]["Forethought"].ObjToDouble() * -1D;
                //dx.Rows[i]["Security National"] = dx.Rows[i]["Security National"].ObjToDouble() * -1D;
                //dx.Rows[i]["FDLIC Old Webb"] = dx.Rows[i]["FDLIC Old Webb"].ObjToDouble() * -1D;
                //dx.Rows[i]["FDLIC Old CCI"] = dx.Rows[i]["FDLIC Old CCI"].ObjToDouble() * -1D;
                //dx.Rows[i]["Unity Old Barham"] = dx.Rows[i]["Unity Old Barham"].ObjToDouble() * -1D;
                //dx.Rows[i]["Unity Old Webb"] = dx.Rows[i]["Unity Old Webb"].ObjToDouble() * -1D;
            }


            dRow = dx.NewRow();
            dRow["month"] = date.ToString("MMMM");

            tempDt = null;
            cmd = "Select * from `trust_data` WHERE `date` = '" + lastDate.ToString("yyyy-MM-dd") + "';";
            DataTable trustDt = G1.get_db_data(cmd);

            double CD = 0D;
            double bankCD = 0D;
            double foreThought = pullActiveForethoughtValue(trustDt, "Pre");
            double securityNational = loadSecurityNationalBalance(lastDate, "Pre", ref tempDt);
            double fdlicOldWebb = pullOldWebb(trustDt);
            double fdlicOldCCI = pullOldCCI(trustDt);
            double unityOldBarham = pullUnityBarhamBalance(trustDt);
            double unityOldWebb = pullUnityWebbBalance(trustDt);
            string record = "";

            dx = LoadOverruns(dx, overDt);

            GetBeginningBalancesPre2002(dx, ref CD, ref bankCD, ref foreThought, ref securityNational, ref fdlicOldWebb, ref fdlicOldCCI, ref unityOldBarham, ref unityOldWebb, ref record);

            dRow["CD"] = G1.ReformatMoney(CD);
            dRow["bankCD"] = G1.ReformatMoney(bankCD );
            dRow["Forethought"] = G1.ReformatMoney(foreThought);
            dRow["Security National"] = G1.ReformatMoney(securityNational);
            dRow["FDLIC Old Webb"] = G1.ReformatMoney(fdlicOldWebb);
            dRow["FDLIC Old CCI"] = G1.ReformatMoney(fdlicOldCCI);
            dRow["Unity Old Barham"] = G1.ReformatMoney(unityOldBarham);
            dRow["Unity Old Webb"] = G1.ReformatMoney(unityOldWebb);
            dRow["status"] = "Beginning Balance";
            dRow["record"] = record;

            dx.Rows.InsertAt(dRow, 0);

            dRow = dx.NewRow();
            dRow["month"] = date.ToString("MMMM");

            tempDt = null;
            //cmd = "Select * from `trust_data` WHERE `date` = '" + date.ToString("yyyy-MM-dd") + "';";
            //trustDt = G1.get_db_data(cmd);

            //foreThought = pullActiveForethoughtValue(trustDt, "Pre");
            //securityNational = loadSecurityNationalBalance(lastDate, "Pre", ref tempDt);
            //fdlicOldWebb = pullOldWebb(trustDt);
            //fdlicOldCCI = pullOldCCI(trustDt);
            //unityOldBarham = pullUnityBarhamBalance(trustDt);
            //unityOldWebb = pullUnityWebbBalance(trustDt);

            CalcEndingBalancesPre2002(dx, ref CD, ref bankCD, ref foreThought, ref securityNational, ref fdlicOldWebb, ref fdlicOldCCI, ref unityOldBarham, ref unityOldWebb, ref record);

            dRow["CD"] = G1.ReformatMoney(CD);
            dRow["bankCD"] = G1.ReformatMoney(bankCD);
            dRow["Forethought"] = G1.ReformatMoney(foreThought);
            dRow["Security National"] = G1.ReformatMoney(securityNational);
            dRow["FDLIC Old Webb"] = G1.ReformatMoney(fdlicOldWebb);
            dRow["FDLIC Old CCI"] = G1.ReformatMoney(fdlicOldCCI);
            dRow["Unity Old Barham"] = G1.ReformatMoney(unityOldBarham);
            dRow["Unity Old Webb"] = G1.ReformatMoney(unityOldWebb);
            dRow["status"] = "Ending Balance";

            dx.Rows.Add(dRow);

            //LoadBeginningBalances(dx);
            //LoadEndingBalances(dx);

            string month = this.dateTimePicker2.Value.ToString("MMMM");
            if (dx.Rows.Count >= 3)
            {
                dx.Rows[0]["month"] = month + " " + this.dateTimePicker2.Value.Year.ToString("D4");
                dx.Rows[1]["month"] = "Beginning";
                dx.Rows[2]["month"] = "Balance";
            }


            //gridMain6.Columns["CD"].Visible = false;

            gridMain6.OptionsClipboard.AllowCopy = DevExpress.Utils.DefaultBoolean.True;
            gridMain6.OptionsClipboard.ClipboardMode = DevExpress.Export.ClipboardMode.Formatted;

            BreakUpPre2002(dt, dx, mainDts, dtCount);

            ReconfigurePre2002(dx);

            btnSave.Show();
            btnSave.Refresh();

            G1.NumberDataTable(dx);
            dgv6.DataSource = dx;
            dgv6.Refresh();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        DataTable LoadOverruns ( DataTable dt, DataTable overDt )
        {
            DateTime date = DateTime.Now;
            DateTime startDate = this.dateTimePicker1.Value;
            DateTime stopDate = this.dateTimePicker2.Value;
            DateTime newStopDate = stopDate.AddDays(workNextDays);
            string sDate1 = startDate.ToString("yyyy-MM-dd");
            string sDate2 = stopDate.ToString("yyyy-MM-dd");
            int i = 0;

            string companies = getCompanyQuery(workCompanies);

            DataView tempview = overDt.DefaultView;
            tempview.Sort = "date DESC";
            overDt= tempview.ToTable();

            double paidTotal = 0D;
            double cashTotal = 0D;
            double cashReceived = 0D;
            double dValue = 0D;
            int totalCount = 0;
            int receivedCount = 0;

            double foreOver = 0D;
            double SecOver = 0D;

            DataTable[] dts = new DataTable[7];
            dts[0] = dt.Clone();
            dts[1] = dt.Clone();

            //dRow["Forethought"] = G1.ReformatMoney(foreThought);
            //dRow["Security National"] = G1.ReformatMoney(securityNational);
            //dRow["FDLIC Old Webb"] = G1.ReformatMoney(fdlicOldWebb);
            //dRow["FDLIC Old CCI"] = G1.ReformatMoney(fdlicOldCCI);
            //dRow["Unity Old Barham"] = G1.ReformatMoney(unityOldBarham);
            //dRow["Unity Old Webb"] = G1.ReformatMoney(unityOldWebb);


            string trustCompany = "";
            string contract = "";
            string desc = "";
            string funeral = "";

            int row = 0;
            DataTable dx = dt.Clone();
            DataRow dRow = null;

            for (i = 0; i < overDt.Rows.Count; i++)
            {
                trustCompany = overDt.Rows[i]["trustCompany"].ObjToString();
                dValue = overDt.Rows[i]["fun_amtReceived"].ObjToDouble();
                if ( dValue == 0D )
                    dValue = overDt.Rows[i]["value"].ObjToDouble();
                if (trustCompany == "Forethought")
                {
                    foreOver += dValue;
                    dx = dts[0];
                }
                else if (trustCompany == "Security National")
                {
                    SecOver += dValue;
                    dx = dts[1];
                }

                date = overDt.Rows[i]["date"].ObjToDateTime();
                contract = overDt.Rows[i]["contract"].ObjToString();
                desc = overDt.Rows[i]["desc"].ObjToString();
                funeral = overDt.Rows[i]["funeral"].ObjToString();

                dRow = dx.NewRow();
                dx.Rows.Add(dRow);
                row = dx.Rows.Count - 1;

                dx.Rows[row][trustCompany + " contract"] = contract;
                dx.Rows[row][trustCompany + " desc"] = desc;
                dx.Rows[row][trustCompany + " funeral"] = funeral;
                dx.Rows[row][trustCompany + " date"] = G1.DTtoMySQLDT(date.ObjToDateTime().ToString("yyyy-MM-dd"));
                dx.Rows[row][trustCompany] = dValue;
                dx.Rows[row][trustCompany + " redx"] = "Y";

                row++;
            }

            int maxRow = 0;
            int count = 0;
            for ( i=0; i<7; i++)
            {
                if (dts[i] == null)
                    continue;
                count = dts[i].Rows.Count;
                if (count > 0)
                {
                    if (count > 1)
                        count = count + 2;
                    count = count + 1;
                    count = count * 2;
                    if (count > maxRow)
                        maxRow = count;
                }
            }

            if (maxRow <= 0)
                return dt; // Nothing to do

            for (i = 0; i < maxRow; i++)
            {
                dRow = dt.NewRow();
                dt.Rows.InsertAt(dRow, 0);
            }

            dt = LoadTrustCompany(dt, dts[0], "Forethought", "DC CASH");
            dt = LoadTrustCompany(dt, dts[0], "Forethought", "DC PAID");

            dt = LoadTrustCompany(dt, dts[1], "Security National", "DC CASH" );
            dt = LoadTrustCompany(dt, dts[1], "Security National", "DC PAID");

            string junk = "";

            return dt;
        }
        /***********************************************************************************************/
        DataTable LoadTrustCompany ( DataTable dt, DataTable dx, string trustCompany, string what )
        {
            DateTime date = DateTime.Now;
            double dValue = 0D;
            double total = 0D;

            int firstRow = 0;
            if (what == "DC PAID")
            {
                if (dx.Rows.Count == 1)
                    firstRow = 2;
                else
                    firstRow = dx.Rows.Count + 3;
            }

            for ( int i=0; i<dx.Rows.Count; i++ )
            {
                dt.Rows[firstRow + i][trustCompany + " contract"] = dx.Rows[i][trustCompany + " contract"].ObjToString();
                dt.Rows[firstRow + i][trustCompany + " desc"] = what + "- " + dx.Rows[i][trustCompany + " desc"].ObjToString();
                dt.Rows[firstRow + i][trustCompany + " funeral"] = dx.Rows[i][trustCompany + " funeral"].ObjToString();

                date = dx.Rows[i][trustCompany + " date"].ObjToDateTime();
                dt.Rows[firstRow + i][trustCompany + " date"] = G1.DTtoMySQLDT(date.ObjToDateTime().ToString("yyyy-MM-dd"));

                dValue = dx.Rows[i][trustCompany].ObjToDouble();
                if (what == "DC PAID")
                    dValue = dValue * -1D;
                dt.Rows[firstRow + i][trustCompany] = dValue;
                total += Math.Abs (dValue);

                dt.Rows[firstRow + i][trustCompany + " redx"] = "Y";
            }

            if (dx.Rows.Count > 1)
            {
                int row = dx.Rows.Count + 1;
                dt.Rows[firstRow + row][trustCompany + " desc"] = what + " Total";
                dt.Rows[firstRow + row][trustCompany + " redx"] = "Y";
                if (what == "DC PAID")
                    total = total * -1D;
                dt.Rows[firstRow + row][trustCompany] = total;
            }

            return dt;
        }
        /***********************************************************************************************/
        private void SetupTab ( TabPage tp, bool include )
        {
            if ( include )
            {
                if (tabControl2.Contains(tp))
                    return;
                tabControl2.TabPages.Add(tp);
            }
            else
            {
                if (tabControl2.Contains(tp))
                    tabControl2.TabPages.Remove(tp);
            }
        }
        /***********************************************************************************************/
        private void BreakUpPre2002 ( DataTable dt, DataTable dx , DataTable [] mainDts, int dtCount )
        {
            SetupTab(tabPage9, false);
            SetupTab(tabPage11, true);
            SetupTab(tabPage12, true);
            SetupTab(tabPage13, true);
            SetupTab(tabPage14, true);
            SetupTab(tabPage15, true);
            SetupTab(tabPage16, true);

            dgv10.Show();
            dgv10.Refresh();
            dgv11.Show();
            dgv11.Refresh();
            dgv12.Show();
            dgv12.Refresh();
            dgv13.Show();
            dgv13.Refresh();
            dgv14.Show();
            dgv14.Refresh();


            LoadPre2002_CD( dx );

            LoadPre2002_OldData(dgv11, gridMain11, dx, "FDLIC Old Webb", "fdlicOldWebb");
            LoadPre2002_OldData(dgv12, gridMain12, dx, "FDLIC Old CCI", "fdlicOldCCI");
            LoadPre2002_OldData(dgv13, gridMain13, dx, "Unity Old Barham", "unityOldBarham");
            LoadPre2002_OldData(dgv14, gridMain14, dx, "Unity Old Webb", "unityOldWebb");
        }
        /***********************************************************************************************/
        private void ReconfigurePre2002 ( DataTable dx )
        {
            ClearAllPositions(gridMain6);

            int i = 1;
            G1.SetColumnPosition(gridMain6, "num", i++);
            G1.SetColumnPosition(gridMain6, "month", i++);
            G1.SetColumnPosition(gridMain6, "junk1", i++);
            G1.SetColumnPosition(gridMain6, "junk2", i++);
            G1.SetColumnPosition(gridMain6, "junk3", i++);
            G1.SetColumnPosition(gridMain6, "junk4", i++);

            //G1.SetColumnPosition(gridMain6, "FDLIC Old Webb", i++);
            //ReconfigureColumnPre2002(dx, "FDLIC Old Webb", false, ref i);

            //G1.SetColumnPosition(gridMain6, "FDLIC Old CCI", i++);
            //ReconfigureColumnPre2002(dx, "FDLIC Old CCI", false, ref i);

            //G1.SetColumnPosition(gridMain6, "Unity Old Barham", i++);
            //ReconfigureColumnPre2002(dx, "Unity Old Barham", false, ref i);

            //G1.SetColumnPosition(gridMain6, "Unity Old Webb", i++);
            //ReconfigureColumnPre2002(dx, "Unity Old Webb", false, ref i);

            //G1.SetColumnPosition(gridMain6, "junk4", i++);
            //G1.SetColumnPosition(gridMain6, "junk5", i++);

            G1.SetColumnPosition(gridMain6, "Forethought", i++);
            ReconfigureColumnPre2002(dx, "Forethought", true, ref i);

            G1.SetColumnPosition(gridMain6, "junk6", i++);
            G1.SetColumnPosition(gridMain6, "junk7", i++);
            G1.SetColumnPosition(gridMain6, "junk8", i++);
            G1.SetColumnPosition(gridMain6, "junk9", i++);

            G1.SetColumnPosition(gridMain6, "Security National", i++);
            ReconfigureColumnPre2002(dx, "Security National", true, ref i);
        }
        /***********************************************************************************************/
        private void ReconfigureColumnPre2002 ( DataTable dx, string name, bool force, ref int i )
        {
            if (G1.get_column_number(dx, name + " desc") > 0)
            {
                G1.SetColumnPosition(gridMain6, name + " desc", i++);
                G1.SetColumnPosition(gridMain6, name + " date", i++);
                G1.SetColumnPosition(gridMain6, name + " contract", i++);
                G1.SetColumnPosition(gridMain6, name + " funeral", i++);
            }
            else if ( force )
            {
                G1.SetColumnPosition(gridMain6, name, i++);
                G1.SetColumnPosition(gridMain6, name + " desc", i++);
                G1.SetColumnPosition(gridMain6, name + " date", i++);
                G1.SetColumnPosition(gridMain6, name + " contract", i++);
                G1.SetColumnPosition(gridMain6, name + " funeral", i++);
            }
        }
        /***********************************************************************************************/
        private void configureCell ( DataTable dx, int row, string column )
        {
            if (G1.get_column_number(dx, column) < 0)
                return;
            if (G1.get_column_number(dx, column + " desc" ) < 0)
                return;
            double dValue = dx.Rows[row][column].ObjToDouble();
            if (dValue == 0D)
                dx.Rows[row][column] = DBNull.Value;
            else
            {
                string desc = dx.Rows[row][column + " desc"].ObjToString();
                if (desc.ToUpper().IndexOf("DC CASH") >= 0)
                    return;
                //if (desc.ToUpper().IndexOf("DC PAID") >= 0)
                dx.Rows[row][column] = dValue * -1D;
            }
        }
        /***********************************************************************************************/
        private double pullUnityWebbBalance(DataTable dt)
        {
            DataTable dx = dt.Clone();
            double total = 0D;
            try
            {
                string preOrPost = "Pre";
                DataRow[] dRows = dt.Select("`endingDeathBenefit` <> '0.00' AND `policyStatus` = 'A'  AND `policyNumber` LIKE 'PSPWT%' AND `preOrPost` = '" + preOrPost + "'");
                if (dRows.Length > 0)
                {
                    dx = dRows.CopyToDataTable();
                    dx = SortBy(dx);

                    for (int i = 0; i < dx.Rows.Count; i++)
                    {
                        total += dx.Rows[i]["endingPaymentBalance"].ObjToDouble();
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return total;
        }
        /***********************************************************************************************/
        private double pullUnityBarhamBalance(DataTable dt)
        {
            DataTable dx = dt.Clone();
            double total = 0D;
            try
            {
                string preOrPost = "Pre";

                DataRow[] dRows = dt.Select("(`endingDeathBenefit` <> '0.00' AND `policyStatus` = 'A'  AND `policyNumber` LIKE 'PSPNB%') OR `policyNumber` = 'PSPNB08002' AND `preOrPost` = '" + preOrPost + "'");
                //dRows = dt.Select("( `Death Benefit` <> '0.00' AND `Policy Extract_Policy Status` = 'A' AND `Policy Number` LIKE 'PSPNB%' ) OR `Policy Number` = 'PSPNB08002'");
                if (dRows.Length > 0)
                {
                    dx = dRows.CopyToDataTable();
                    dx = SortBy(dx);
                    dx.Columns.Add("unityOldBarham", Type.GetType("System.Double"));
                    for (int i = 0; i < dx.Rows.Count; i++)
                    {
                        total += dx.Rows[i]["endingPaymentBalance"].ObjToDouble();
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return total;
        }
        /***********************************************************************************************/
        private double pullOldWebb(DataTable dt)
        {
            DataTable dx = dt.Clone();
            double total = 0D;
            try
            {
                string preOrPost = "Pre";
                DataRow[] dRows = dt.Select("`trustCompany` = 'FDLIC PB' AND `preOrPost` = 'Pre' ");
                if (dRows.Length > 0)
                {
                    dx = dRows.CopyToDataTable();

                    for (int i = 0; i < dx.Rows.Count; i++)
                    {
                        total += dx.Rows[i]["endingDeathBenefit"].ObjToDouble();
                    }
                    G1.NumberDataTable(dx);
                    //unityWebbDt = dx.Copy();
                }
            }
            catch (Exception ex)
            {
            }
            return total;
        }
        /***********************************************************************************************/
        private double pullOldCCI(DataTable dt)
        {
            DataTable dx = dt.Clone();
            double total = 0D;
            try
            {
                string preOrPost = "Pre";
                DataRow[] dRows = dt.Select("`trustCompany` = 'FDLIC CCI' AND `preOrPost` = 'Pre' ");
                if (dRows.Length > 0)
                {
                    dx = dRows.CopyToDataTable();

                    for (int i = 0; i < dx.Rows.Count; i++)
                    {
                        total += dx.Rows[i]["endingDeathBenefit"].ObjToDouble();
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return total;
        }
        /****************************************************************************************/
        private void SetupDetailColumnsPre()
        {
            AddNewColumn("sn desc", "Description", 150);
            AddNewColumn("sn date", "Date", 100);
            AddNewColumn("sn contract", "Contract #", 100);
            AddNewColumn("sn funeral", "Funeral #", 100);

            AddNewColumn("fore desc", "Description", 150);
            AddNewColumn("fore date", "Date", 100);
            AddNewColumn("fore contract", "Contract #", 100);
            AddNewColumn("fore funeral", "Funeral #", 100);

            AddNewColumn("cd desc", "Description", 150);
            AddNewColumn("cd date", "Date", 100);
            AddNewColumn("cd contract", "Contract #", 100);
            AddNewColumn("cd funeral", "Funeral #", 100);

            AddNewColumn("unity desc", "Description", 150);
            AddNewColumn("unity date", "Date", 100);
            AddNewColumn("unity contract", "Contract #", 100);
            AddNewColumn("unity funeral", "Funeral #", 100);

            AddNewColumn("unity pb desc", "Description", 150);
            AddNewColumn("unity pb date", "Date", 100);
            AddNewColumn("unity pb contract", "Contract #", 100);
            AddNewColumn("unity pb funeral", "Funeral #", 100);

            AddNewColumn("fdlic desc", "Description", 150);
            AddNewColumn("fdlic date", "Date", 100);
            AddNewColumn("fdlic contract", "Contract #", 100);
            AddNewColumn("fdlic funeral", "Funeral #", 100);

            AddNewColumn("fdlic pb desc", "Description", 150);
            AddNewColumn("fdlic pb date", "Date", 100);
            AddNewColumn("fdlic pb contract", "Contract #", 100);
            AddNewColumn("fdlic pb funeral", "Funeral #", 100);
        }
        /****************************************************************************************/
        private void useCalculatedBeginningBalanceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //if (useCalculatedBeginningBalanceToolStripMenuItem.Checked)
            //{
            //    useCalculatedBeginningBalanceToolStripMenuItem.Checked = false;
            //    useCalculatedBeginningBalance = false;
            //}
            //else
            //{
            //    useCalculatedBeginningBalanceToolStripMenuItem.Checked = true;
            //    useCalculatedBeginningBalance = true;
            //}

            if (dgv.DataSource == null)
                return;

            DataTable dt = (DataTable)dgv.DataSource;
            if (dt.Rows.Count <= 0)
                return;

            double balance = 0D;
            double smfsBalance = 0D;
            double ftBalance = 0D;
            string month = "";
            DataTable activeDt = null;

            string trustCompany = dt.TableName;
            if (workReport == "Post 2002 Report - SN & FT")
                trustCompany = "SNFT";
            else if (workReport == "Post 2002 Report - Unity")
                trustCompany = "Unity";
            else if (workReport == "Post 2002 Report - FDLIC")
                trustCompany = "FDLIC";

            DateTime date = this.dateTimePicker2.Value;
            //date = date.AddMonths(1);
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            date = new DateTime(date.Year, date.Month, days);
            string cmd = "Select * from `trust_data_edits` where `date` = '" + date.ToString("yyyy-MM-dd") + "' AND `status` = 'BeginningBalance' AND `trustName` = '" + trustCompany + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                string record = dx.Rows[0]["record"].ObjToString();
                G1.delete_db_table("trust_data_edits", "record", record);
            }
            if (workReport == "Post 2002 Report - Unity")
            {
                this.Cursor = Cursors.WaitCursor;
                balance = loadUnityBalance(date.AddMonths(-1), cmbPreOrPost.Text.Trim(), ref activeDt);
                smfsBalance = LoadTBB(date.AddMonths(-1));
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    month = dt.Rows[i]["month"].ObjToString();
                    if (month.ToUpper() == "BEGINNING BALANCE")
                    {
                        dt.Rows[i]["balance"] = G1.RoundValue(balance);
                        dt.Rows[i]["smfsBalance"] = smfsBalance;
                        gridMain.RefreshEditor(true);
                        gridMain.RefreshData();
                        dgv.RefreshDataSource();

                        LoadEndingBalances(dt);
                        break;
                    }
                }
                this.Cursor = Cursors.Default;
            }
        }
        /****************************************************************************************/
        private void useCalculatedEndingBalanceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //if (useCalculatedEndingBalanceToolStripMenuItem.Checked)
            //{
            //    useCalculatedEndingBalanceToolStripMenuItem.Checked = false;
            //    useCalculatedEndingBalance = false;
            //}
            //else
            //{
            //    useCalculatedEndingBalanceToolStripMenuItem.Checked = true;
            //    useCalculatedEndingBalance = true;
            //}
            if (dgv.DataSource == null)
                return;

            DataTable dt = (DataTable)dgv.DataSource;
            if (dt.Rows.Count <= 0)
                return;

            string trustCompany = dt.TableName;
            if (workReport == "Post 2002 Report - SN & FT")
                trustCompany = "SNFT";
            else if (workReport == "Post 2002 Report - Unity")
                trustCompany = "Unity";
            else if (workReport == "Post 2002 Report - FDLIC")
                trustCompany = "FDLIC";

            DateTime date = this.dateTimePicker2.Value;
            //date = date.AddMonths(1);
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            date = new DateTime(date.Year, date.Month, days);
            string cmd = "Select * from `trust_data_edits` where `date` = '" + date.ToString("yyyy-MM-dd") + "' AND `status` = 'EndingBalance' AND `trustName` = '" + trustCompany + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                string record = dx.Rows[0]["record"].ObjToString();
                G1.delete_db_table("trust_data_edits", "record", record);
            }

            LoadEndingBalances(dt);
        }
        /****************************************************************************************/
        private void DetermineDifference()
        {
            if (btnHold.BackColor == Color.Green)
                return;

            this.Cursor = Cursors.WaitCursor;

            DateTime date = this.dateTimePicker2.Value;
            string preOrPost = this.cmbPreOrPost.Text.Trim();

            DataTable dtNow = getAllActive(date, preOrPost);

            DataRow[] dRows = dtNow.Select("policyNumber='770073115'");
            int dtCount = dRows.Length;

            date = date.AddMonths(-1);
            DataTable dtThen = getAllActive(date, preOrPost);
            dRows = dtThen.Select("policyNumber='770073115'");
            dtCount = dRows.Length;

            DataTable dtNew = CompareRows(dtNow, dtThen);
            DataTable dtNext = CompareRows(dtThen, dtNow);

            //DataTable dtNew = GetTableDiff(dtNow, dtThen, "policyNumber");
            int count = dtNew.Rows.Count;

            DataTable dt = (DataTable)dgv.DataSource;
            DataTable ddd = CompareRows(dtNew, dt);

            string fName = "";
            string lName = "";
            string mName = "";
            string insuredName = "";
            for (int i = 0; i < ddd.Rows.Count; i++)
            {
                fName = ddd.Rows[i]["firstName"].ObjToString();
                lName = ddd.Rows[i]["lastName"].ObjToString();
                mName = ddd.Rows[i]["middleName"].ObjToString();
                insuredName = G1.BuildFullName("", fName, mName, lName, "");
                ddd.Rows[i]["insuredName"] = insuredName;
            }

            G1.NumberDataTable(ddd);

            dgv3.DataSource = ddd;

            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private DataTable getAllActive(DateTime date, string preOrPost)
        {
            DateTime newDate = new DateTime(date.Year, date.Month, 1);
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            DateTime newDate2 = new DateTime(date.Year, date.Month, days);

            string startDate = newDate.ToString("yyyy-MM-dd");
            string stopDate = newDate2.ToString("yyyy-MM-dd") + " 23:59:59";

            string cmd = "Select * from `trust_data` WHERE `date` >= '" + startDate + "' AND `date` <= '" + stopDate + "' AND `preOrPost` = '" + preOrPost + "' ";
            cmd += " AND `trustCompany` IN ('Unity','Unity PB') ";
            cmd += " ORDER by `date`, `trustCompany`;  ";

            DataTable dt = G1.get_db_data(cmd);


            //dt = fixTheData(dt, true);

            DataView tempview = dt.DefaultView;
            tempview.Sort = "date ASC";
            dt = tempview.ToTable();

            DataTable activeDt = dt.Clone();
            DataTable lapsedDt = dt.Clone();
            DataTable questionedDt = dt.Clone();
            DataTable pbDt = dt.Clone();

            activeDt = pullUnityActive(dt);

            DataRow[] dRows = activeDt.Select("policyNumber='770073115'");
            int dtCount = dRows.Length;


            lapsedDt = pullUnityLapsed(dt);
            dRows = lapsedDt.Select("policyNumber='770073115'");
            dtCount = dRows.Length;
            activeDt.Merge(lapsedDt);

            questionedDt = pullUnityLapsedQuestioned(dt);
            dRows = questionedDt.Select("policyNumber='770073115'");
            dtCount = dRows.Length;
            activeDt.Merge(questionedDt);

            pbDt = pullUnityPB(dt);
            dRows = pbDt.Select("policyNumber='770073115'");
            dtCount = dRows.Length;
            activeDt.Merge(pbDt);

            return activeDt;
        }
        /***********************************************************************************************/
        private DataTable CompareRows(DataTable dx, DataTable dx2)
        {
            DataTable dx3 = dx.Clone();
            string policyNumber = "";
            DataRow[] dRows = null;
            int firstCol = G1.get_column_number(dx, "policyNumber");
            string data1 = "";
            string data2 = "";
            string colName = "";
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                policyNumber = dx.Rows[i]["policyNumber"].ObjToString();
                if (String.IsNullOrWhiteSpace(policyNumber))
                    continue;
                if (policyNumber == "770073117")
                {
                }
                dRows = dx2.Select("policyNumber='" + policyNumber + "'");
                if (dRows.Length <= 0)
                    dx3.ImportRow(dx.Rows[i]);
            }
            for (int i = 0; i < dx2.Rows.Count; i++)
            {
                policyNumber = dx2.Rows[i]["policyNumber"].ObjToString();
                if (String.IsNullOrWhiteSpace(policyNumber))
                    continue;
                dRows = dx.Select("policyNumber='" + policyNumber + "'");
                if (dRows.Length <= 0)
                {
                    dRows = dx3.Select("policyNumber='" + policyNumber + "'");
                    if (dRows.Length <= 0)
                        dx3.ImportRow(dx2.Rows[i]);
                }
            }
            return dx3;
        }
        /****************************************************************************************/
        public DataTable GetTableDiff(DataTable dt1, DataTable dt2, string columnName)
        {
            dt1.PrimaryKey = new DataColumn[] { dt1.Columns["record"] };
            dt2.PrimaryKey = new DataColumn[] { dt2.Columns["record"] };

            IEqualityComparer<string> comparer = StringComparer.OrdinalIgnoreCase;

            var lookup = dt2.AsEnumerable().ToLookup(row => row["record"].ToString().Trim(),
                comparer);

            var diffList = dt1.AsEnumerable()
                .Where(r1 => !lookup[r1["record"].ToString().Trim()].Any(r2 => comparer.Equals(
                    r1[columnName].ToString().Trim(), r2[columnName].ToString().Trim())))
                .ToList();

            DataTable dtResult = dt1.Clone();
            if (diffList.Count == 0) return dtResult;
            return diffList.CopyToDataTable();
        }
        /****************************************************************************************/
        private void btnHold_Click(object sender, EventArgs e)
        {
            if (btnHold.BackColor == Color.Green)
            {
                btnHold.BackColor = Color.SandyBrown;
                btnHold.ForeColor = Color.Black;
                btnHold.Text = "Hold";
                btnHold.Refresh();
                //DetermineDifference();
            }
            else
            {
                btnHold.BackColor = Color.Green;
                btnHold.ForeColor = Color.White;
                btnHold.Text = "Release";
                btnHold.Refresh();
            }
        }
        /****************************************************************************************/
        private void moveToDeceasedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dgv5.Visible)
                return;
            if (dgv2.Visible)
                return;
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt == null)
                return;
            if (dt.Rows.Count <= 0)
                return;

            DataRow dr = gridMain.GetFocusedDataRow();

            string contract = dr["contractNumber"].ObjToString();
            string funeral = dr["funeral"].ObjToString();
            string cmd = "Select * from `trust_data` WHERE `contractNumber` = '" + contract + "' AND `date` = '" + workDate.ToString("yyyy-MM-dd") + "' AND `deathPaidDate` < '1001-01-01';";

            DataTable dddd = G1.get_db_data(cmd);
            if (dddd.Rows.Count > 0)
            {
                string record = dddd.Rows[0]["record"].ObjToString();
                DateTime dateReceived = dr["dateReceived"].ObjToDateTime();
                G1.update_db_table("trust_data", "record", record, new string[] { "reportDate", workDate.ToString("yyyy-MM-dd"), "deathPaidDate", dateReceived.ToString("yyyy-MM-dd") });
            }
        }
        /****************************************************************************************/
        private void tBBToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (dgv2.Visible)
                dt = (DataTable)dgv2.DataSource;
            if (dt == null)
                return;
            if (dt.Rows.Count <= 0)
                return;

            DataRow dr = gridMain.GetFocusedDataRow();
            string contract = dr["contractNumber"].ObjToString();
            if (dgv2.Visible)
            {
                dr = gridMain2.GetFocusedDataRow();
                contract = dr["contract"].ObjToString();
            }
            if (String.IsNullOrWhiteSpace(contract))
                return;

            if (dgv5.Visible)
            {
                dt = (DataTable)dgv5.DataSource;
                if (dt == null)
                    return;
                if (dt.Rows.Count <= 0)
                    return;

                dr = gridMain5.GetFocusedDataRow();
                contract = dr["Trust"].ObjToString();
                if (String.IsNullOrWhiteSpace(contract))
                    return;
            }

            this.Cursor = Cursors.WaitCursor;
            PayOffDetail detail = new PayOffDetail(contract);
            detail.Show();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void gridMain_CustomDrawCell(object sender, RowCellCustomDrawEventArgs e)
        {
            if (e == null)
                return;
            DataTable dt = (DataTable)dgv.DataSource;
            string contractNumber = "";
            //if (1 == 1)
            //    return;
            DataRow dR = gridMain.GetDataRow(e.RowHandle);
            int row = gridMain.GetDataSourceRowIndex(e.RowHandle);
            if (e.Column.FieldName.ToUpper() == "SANDRAMONEY")
            {
                if (dR != null)
                {
                    dR["month"] = "";
                    //int row = gridMain.GetDataSourceRowIndex(e.RowHandle);
                    contractNumber = dt.Rows[row]["contractNumber"].ObjToString();
                    if (contractNumber == "C13046U")
                    {
                    }
                    if (G1.get_column_number(dt, "sandraMoney") >= 0)
                    {
                        //row = e.RowHandle;
                        //if (row > 0)
                        {
                            double sandraMoney = dt.Rows[row]["sandraMoney"].ObjToDouble();
                            //sandraMoney = dR["sandraMoney"].ObjToDouble();
                            sandraMoney = G1.RoundValue(sandraMoney);
                            double trustMoney = dt.Rows[row]["value"].ObjToDouble();
                            //trustMoney = dR["value"].ObjToDouble();
                            trustMoney = G1.RoundValue(trustMoney);
                            if (trustMoney != sandraMoney)
                            {
                                dR["month"] = "*";
                                //dR["SandraMoney"] = 15D;
                                //if ( trustMoney> 0D && sandraMoney == 0D )
                                //    e.Column.AppearanceCell.BackColor = Color.Pink;
                                //e.Handled = true;
                            }
                            //else
                            //    e.Column.AppearanceCell.BackColor = Color.Transparent;
                        }
                    }
                }
            }
            if (e.Column.FieldName.ToUpper() == "SANDRAPRINCIPAL")
            {
                if (dR != null)
                {
                    if (G1.get_column_number(dt, "sandraPrincipal") >= 0)
                    {
                        double sandraPrincipal = dR["sandraPrincipal"].ObjToDouble();
                        sandraPrincipal = dR["sandraPrincipal"].ObjToDouble();
                        sandraPrincipal = G1.RoundValue(sandraPrincipal);
                        double trustMoney = dR["principal"].ObjToDouble();
                        trustMoney = dR["principal"].ObjToDouble();
                        trustMoney = G1.RoundValue(trustMoney);
                        if (trustMoney != sandraPrincipal)
                        {
                            string month = dR["month"].ObjToString();
                            if (month.IndexOf("P") < 0)
                            {
                                month += "P";
                                dR["month"] = month;
                            }
                            //e.Column.AppearanceCell.BackColor = Color.Pink;
                            //e.Handled = true;
                        }
                        //else
                        //    e.Column.AppearanceCell.BackColor = Color.Transparent;
                    }
                }
            }
            if (e.Column.FieldName.ToUpper() == "NUM" || e.Column.FieldName.ToUpper() == "DESC" )
            {
                string status = dR["status"].ObjToString().ToUpper();
                if ( status == "LINE EDIT")
                    e.Appearance.BackColor = Color.LightGreen;
                else
                {
                    Font font = e.Appearance.Font;
                    float Size = font.Size;
                    string middleName = dR["middleName"].ObjToString().Trim().ToUpper();
                    if (middleName == "REPLACE")
                    {
                        e.Appearance.BackColor = Color.Pink;
                        if ( e.Column.FieldName.ToUpper() == "DESC")
                            e.Appearance.Font = new Font(font.Name, Size, FontStyle.Italic);
                    }
                    else
                    {
                        e.Appearance.BackColor = Color.Transparent;
                        //e.Column.AppearanceCell.Font = new Font(font.Name, Size, FontStyle.Regular);
                    }
                }
                if (e.Column.FieldName.ToUpper() == "NUM")
                {
                    string billingReason = dR["billingReason"].ObjToString().ToUpper();
                    if (billingReason == "RP")
                    {
                        status = dR["num"].ObjToString();
                        if ( status.IndexOf ( "RP") < 0 )
                            status += " RP";
                        dR["num"] = status;
                    }
                }
            }
            //else if (e.Column.FieldName.ToUpper() == "DESC")
            //{
            //    dR = gridMain.GetDataRow(e.RowHandle);
            //    Font font = e.Column.AppearanceCell.Font;
            //    float Size = font.Size;

            //    string middleName = dR["middleName"].ObjToString().Trim().ToUpper();
            //    if (middleName == "REPLACE")
            //        e.Column.AppearanceCell.Font = new Font(font.Name, Size, FontStyle.Italic);
            //    else
            //        e.Column.AppearanceCell.Font = new Font(font.Name, Size, FontStyle.Regular);
            //}
        }
        /****************************************************************************************/
        private void gridMain2_CustomColumnDisplayText(object sender, CustomColumnDisplayTextEventArgs e)
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
                    if (date.Year < 1500)
                        e.DisplayText = "";
                }
            }
            if (e.Column.DisplayFormat.FormatType == FormatType.Numeric)
            {
                double value = e.DisplayText.ObjToDouble();
                //string str = e.DisplayText.ObjToString();
                //bool negative = false;
                //if (value < 0D)
                //    negative = true;
                //else if (str.IndexOf("-") >= 0)
                //    negative = true;
                //else if (str.IndexOf("(") >= 0)
                //    negative = true;
                //else if (str.IndexOf(")") >= 0)
                //    negative = true;
                //if (String.IsNullOrWhiteSpace(str))
                //    return;
                //str = str.Replace(",", "");
                //str = str.Replace("(", "");
                //str = str.Replace(")", "");
                //value = Convert.ToDouble(str);
                //if (negative)
                //    value = value * -1D;
                //if (value > 0D && !negative )
                //{
                //    e.DisplayText = G1.ReformatMoney(value);
                //}
                //else if (value < 0D || negative )
                //{
                //    value = Math.Abs(value);
                //    e.DisplayText = "(" + G1.ReformatMoney(value) + ")";
                //}
                //else if (value == 0D)
                //{
                //    if (e.Column.FieldName.ToUpper() == "RECEIVED")
                //        e.DisplayText = "";
                //}
            }
        }
        /***********************************************************************************************/
        private void setUnityWebb(DataTable dt)
        {
            DataTable dx = dt.Clone();
            try
            {
                string preOrPost = cmbPreOrPost.Text;
                if (preOrPost == "Both")
                    preOrPost = "Pre";
                DataRow[] dRows = dt.Select("`trustCompany` LIKE '%Unity%' AND `endingDeathBenefit` <> '0.00' AND `policyStatus` = 'T'  AND `policyNumber` LIKE 'PSPWT%' AND `preOrPost` = '" + preOrPost + "'");
                if (dRows.Length > 0)
                {
                    for (int i = 0; i < dRows.Length; i++)
                    {
                        dRows[i]["trustCompany"] = "Unity Old Webb";
                        dRows[i]["trust"] = "Unity Old Webb";
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
        /***********************************************************************************************/
        private void setUnityBarham(DataTable dt)
        {
            DataTable dx = dt.Clone();

            try
            {
                string preOrPost = cmbPreOrPost.Text;
                if (preOrPost == "Both")
                    preOrPost = "Pre";

                DataRow[] dRows = dt.Select("(`trustCompany` LIKE '%Unity%' AND `endingDeathBenefit` <> '0.00' AND `policyStatus` = 'T'  AND `policyNumber` LIKE 'PSPNB%') OR `policyNumber` = 'PSPNB08002' AND `preOrPost` = '" + preOrPost + "'");
                //dRows = dt.Select("( `Death Benefit` <> '0.00' AND `Policy Extract_Policy Status` = 'A' AND `Policy Number` LIKE 'PSPNB%' ) OR `Policy Number` = 'PSPNB08002'");
                if (dRows.Length > 0)
                {
                    for (int i = 0; i < dRows.Length; i++)
                    {
                        dRows[i]["trustCompany"] = "Unity Old Barham";
                        dRows[i]["trust"] = "Unity Old Barham";
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
        /***********************************************************************************************/
        private void setOldWebb(DataTable dt)
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
                    for (int i = 0; i < dRows.Length; i++)
                    {
                        dRows[i]["trustCompany"] = "OldOld Webb";
                        dRows[i]["trust"] = "OldOld Webb";
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
        /***********************************************************************************************/
        private void setOldCCI(DataTable dt)
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
                    for (int i = 0; i < dRows.Length; i++)
                    {
                        dRows[i]["trustCompany"] = "Old CCI";
                        dRows[i]["trust"] = "Old CCI";
                    }
                }
            }
            catch (Exception ex)
            {
            }
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
        /****************************************************************************************/
        private void pre2002ReportSNFTToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menu = (ToolStripMenuItem)sender;

            workReport = menu.Text;

            chkOldStuff.Checked = true;
            cmbPreOrPost.Text = "Pre";
            chkJustTrustsSelected.Checked = true;

            chkCmbCompany.CheckAll();
            chkCmbCompany.Refresh();

            cmbSelectColumns.Text = "Pre 2002 Totals";
            cmbSelectColumns.SelectedItem = "Pre 2002 Totals";
        }
        /****************************************************************************************/
        private void gridMain6_CustomColumnDisplayText(object sender, CustomColumnDisplayTextEventArgs e)
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
                    if (date.Year < 1500)
                        e.DisplayText = "";
                }
            }
            if (e.Column.DisplayFormat.FormatType == FormatType.Numeric)
            {
                double value = e.DisplayText.ObjToDouble();
                if (value > 0D)
                {
                    e.DisplayText = G1.ReformatMoney(value);
                }
                else if (value < 0D)
                {
                    value = Math.Abs(value);
                    e.DisplayText = "(" + G1.ReformatMoney(value) + ")";
                }
            }
        }
        /****************************************************************************************/
        private void gridMain_DoubleClick_1(object sender, EventArgs e)
        {

        }
        /****************************************************************************************/
        private void gridMain2_CellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            if (e == null)
                return;
            int rowHandle = gridMain2.FocusedRowHandle;
            int row = gridMain2.GetDataSourceRowIndex(rowHandle);
            DataRow dr = gridMain2.GetFocusedDataRow();
            if (dr == null)
                return;

            DataTable dt = (DataTable)dgv2.DataSource;
            string record = dr["record"].ObjToString();
            if (record == "0")
                record = "";

            string column = gridMain2.FocusedColumn.FieldName;

            DateTime date = this.dateTimePicker2.Value;

            string trustCompany = dr["trust"].ObjToString();
            double smfsBalance = dr["smfsBalance"].ObjToDouble();
            double balance = dr["value"].ObjToDouble();
            double ftBalance = dr["received"].ObjToDouble();

            trustCompany = "XYZZY";
            if (workReport == "Post 2002 Report - SN & FT")
                trustCompany = "SNFT";
            else if (workReport == "Post 2002 Report - Unity")
                trustCompany = "Unity";
            else if (workReport == "Post 2002 Report - FDLIC")
                trustCompany = "FDLIC";
            else if (workReport == "Post 2002 Report - CD")
                trustCompany = "CD";

            string preOrPost = "Post";

            string month = dr["month"].ObjToString().ToUpper();
            if (month != "BEGINNING BALANCE" && month != "BEGINNING ADJUSTMENT")
            {
                if (month == "ENDING BALANCE" || month == "ENDING MANUAL ADJUSTMENT")
                    endingDataChanged();
                else
                {
                    if (String.IsNullOrWhiteSpace(record))
                    {
                        record = G1.create_record("trust_data_edits", "status", "-1");
                        dr["record"] = record.ObjToInt32();
                        G1.update_db_table("trust_data_edits", "record", record, new string[] { "status", "Line Edit", "preOrPost", "Post" });
                    }
                    if (G1.BadRecord("trust_data_edits", record))
                        return;
                    string lastName = dr["desc"].ObjToString();
                    string firstName = "";
                    if (G1.get_column_number(dt, "otherdesc") > 0)
                    {
                        firstName = dr["otherdesc"].ObjToString();
                    }
                    //G1.update_db_table("trust_data_edits", "record", record, new string[] { "beginningPaymentBalance", balance.ToString(), "beginningDeathBenefit", smfsBalance.ToString(), "endingPaymentBalance", ftBalance.ToString(), "date", date.ToString("yyyy-MM-dd"), "trustName", trustCompany, "lastName", lastName, "firstName", firstName, "position", row.ToString() });
                    G1.update_db_table("trust_data_edits", "record", record, new string[] { "beginningPaymentBalance", balance.ToString(), "endingPaymentBalance", smfsBalance.ToString(), "beginningDeathBenefit", ftBalance.ToString(), "date", date.ToString("yyyy-MM-dd"), "trustName", trustCompany, "lastName", lastName, "firstName", firstName, "preOrPost", preOrPost, "position", row.ToString() });
                    if ( column.ToUpper() == "DATE")
                    {
                        string data = dr[column].ObjToDateTime().ToString("yyyy-MM-dd");
                        G1.update_db_table("trust_data_edits", "record", record, new string[] { "deathPaidDate", data });
                    }
                    else if (column.ToUpper() == "CONTRACT")
                    {
                        string data = dr[column].ObjToString();
                        G1.update_db_table("trust_data_edits", "record", record, new string[] { "contractNumber", data });
                    }
                    else if (column.ToUpper() == "FUNERAL")
                    {
                        string data = dr[column].ObjToString();
                        G1.update_db_table("trust_data_edits", "record", record, new string[] { "statusReason", data });
                    }
                    else if (column.ToUpper() == "DATERECEIVED")
                    {
                        string data = dr[column].ObjToDateTime().ToString("yyyy-MM-dd");
                        G1.update_db_table("trust_data_edits", "record", record, new string[] { "middleName", data });
                    }
                    else if (column.ToUpper() == "OTHERCONTRACT")
                    {
                        string data = dr[column].ObjToString();
                        G1.update_db_table("trust_data_edits", "record", record, new string[] { "billingReason", data });
                    }
                    else if (column.ToUpper() == "OTHERFUNERAL")
                    {
                        string data = dr[column].ObjToString();
                        G1.update_db_table("trust_data_edits", "record", record, new string[] { "policyStatus", data });
                    }

                    LoadEndingBalances(dt, true );

                    gridMain2.RefreshEditor(true);
                    gridMain2.RefreshData();
                    dgv2.Refresh();
                    gridMain2.PostEditor();
                }
                //else
                //    LoadEndingBalances(dt); // Raw Data Must Have Changed
                return;
            }


            string colName = gridMain2.FocusedColumn.FieldName;
            if (colName.ToUpper() != "VALUE" && colName.ToUpper() != "RECEIVED" && colName.ToUpper() != "SMFSBALANCE")
                return;

            //double balance = dr[colName].ObjToDouble();
            //double balance = dr["balance"].ObjToDouble();

            //double smfsBalance = dr["smfsBalance"].ObjToDouble();
            //double balance = dr["value"].ObjToDouble();
            //double ftBalance = dr["received"].ObjToDouble();

            trustCompany = "XYZZY";
            if (workReport == "Post 2002 Report - SN & FT")
                trustCompany = "SNFT";
            else if (workReport == "Post 2002 Report - Unity")
                trustCompany = "Unity";
            else if (workReport == "Post 2002 Report - FDLIC")
                trustCompany = "FDLIC";
            else if (workReport == "Post 2002 Report - CD")
                trustCompany = "CD";

            if (trustCompany == "XYZZY")
                return;

            string mainUpdate = "BeginningBalance";

            if (month == "BEGINNING BALANCE")
            {
                mainUpdate = "BeginningBalance";
                if (String.IsNullOrWhiteSpace(record))
                {
                    record = G1.create_record("trust_data_edits", "status", "-1");
                    dr["record"] = record.ObjToInt32();
                    G1.update_db_table("trust_data_edits", "record", record, new string[] {"status", mainUpdate, "preOrPost", "Post" });
                }
                if (G1.BadRecord("trust_data_edits", record))
                    return;
                G1.update_db_table("trust_data_edits", "record", record, new string[] { "status", mainUpdate, "beginningPaymentBalance", balance.ToString(), "endingPaymentBalance", smfsBalance.ToString(), "beginningDeathBenefit", ftBalance.ToString(), "date", date.ToString("yyyy-MM-dd"), "trustName", trustCompany, "preOrPost", preOrPost });
                //G1.update_db_table("trust_data_edits", "record", record, new string[] { "status", mainUpdate, "beginningPaymentBalance", balance.ToString(), "beginningDeathBenefit", smfsBalance.ToString(), "endingPaymentBalance", ftBalance.ToString(), "date", date.ToString("yyyy-MM-dd"), "trustName", trustCompany });
            }

            //LoadEndingBalances(dt);
        }
        /****************************************************************************************/
        private void gridMain2_CustomSummaryCalculate(object sender, DevExpress.Data.CustomSummaryEventArgs e)
        {
            try
            {
                string what = "";
                double value = e.TotalValue.ObjToDouble();
                string field = (e.Item as GridSummaryItem).FieldName.ObjToString();
                DataTable dt = (DataTable)dgv2.DataSource;

                double totalValue = 0D;
                double totalReceived = 0D;
                double dValue = 0D;
                string manual = "";
                string desc = "";
                bool doit = false;
                bool gotOther = true;
                string cashPaid = "";
                if (G1.get_column_number(dt, "otherdesc") < 0)
                    gotOther = false;
                bool gotCashPaid1 = true;
                if (G1.get_column_number(dt, "cashPaid1") < 0)
                    gotCashPaid1 = false;

                for (int i = 1; i < dt.Rows.Count; i++)
                {
                    doit = true;
                    what = dt.Rows[i]["month"].ObjToString().Trim();
                    if (what.ToUpper() == "BEGINNING")
                        what = "";
                    else if (what.ToUpper() == "BALANCE")
                        what = "";
                    if (!String.IsNullOrWhiteSpace(what))
                    {
                        manual = dt.Rows[i]["manual"].ObjToString().ToUpper();
                        if (manual != "Y")
                            continue;
                    }

                    desc = dt.Rows[i]["desc"].ObjToString();
                    cashPaid = "";
                    if (gotCashPaid1)
                        cashPaid = dt.Rows[i]["cashPaid1"].ObjToString();
                    if (desc.ToUpper().IndexOf("PD") == 0)
                        doit = false;
                    if (desc.ToUpper().IndexOf("TOTAL") == 0)
                        doit = false;
                    if (cashPaid.ToUpper() == "DC PAID")
                        doit = false;
                    if (cashPaid.ToUpper() == "DC CASH")
                        doit = false;

                    if ( doit )
                    {
                        dValue = dt.Rows[i]["value"].ObjToDouble();
                        totalValue += dValue;
                    }

                    if (gotOther)
                    {
                        desc = dt.Rows[i]["otherdesc"].ObjToString();
                        if (desc.ToUpper().IndexOf("PD") < 0)
                        {
                            dValue = dt.Rows[i]["received"].ObjToDouble();
                            totalReceived += dValue;
                        }
                    }
                    else
                    {
                        dValue = dt.Rows[i]["received"].ObjToDouble();
                        totalReceived += dValue;
                    }
                }

                if (field.ToUpper() == "VALUE")
                {
                    e.TotalValueReady = true;
                    e.TotalValue = totalValue;
                    gridMain2.PostEditor();
                    gridMain2.RefreshEditor(true);
                }
                else if (field.ToUpper() == "RECEIVED")
                {
                    e.TotalValueReady = true;
                    e.TotalValue = totalReceived;
                    gridMain2.PostEditor();
                    gridMain2.RefreshEditor(true);
                }
            }
            catch (Exception ex)
            {
            }
        }
        /****************************************************************************************/
        private void btnInsert_Click(object sender, EventArgs e)
        {
            if (workReport == "Pre 2002 Report")
            {
                if (dgv6.Visible)
                {
                    Insert_Row(dgv6, gridMain6);
                    gridMain6_DataValueChanged(true);
                }
                else if (dgv10.Visible)
                    Insert_Row(dgv10, gridMain10);
                else if (dgv11.Visible)
                    Insert_Row(dgv11, gridMain11);
                else if (dgv12.Visible)
                    Insert_Row(dgv12, gridMain12);
                else if (dgv13.Visible)
                    Insert_Row(dgv13, gridMain13);
                else if (dgv14.Visible)
                    Insert_Row(dgv14, gridMain14);
                else
                    return;
            }
            else
            {
                if (dgv2.Visible)
                    Insert_Row(dgv2, gridMain2);
            }
        }
        /****************************************************************************************/
        private void Insert_Row(DevExpress.XtraGrid.GridControl dgv, DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gridMain, bool zeroOkay = false )
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt.Rows.Count <= 0)
                return;
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            if (row == 0 && !zeroOkay )
            {
                MessageBox.Show("*** ERROR *** You can not insert a new row before the first row!", "Bad Insert Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }
            if (row < 0 || row > (dt.Rows.Count - 1))
                return;
            if (rowHandle > (dt.Rows.Count - 1))
                return; // Already at the last row

            string trustCompany = dt.TableName.Trim();
            trustCompany = dr["trust"].ObjToString();
            if (String.IsNullOrWhiteSpace(trustCompany))
            {
                if (workReport == "Post 2002 Report - SN & FT")
                    trustCompany = "SNFT";
                else if (workReport == "Post 2002 Report - Unity")
                    trustCompany = "Unity";
                else if (workReport == "Post 2002 Report - FDLIC")
                    trustCompany = "FDLIC";
                else if (workReport == "Post 2002 Report - CD")
                    trustCompany = "CD";
                else if (workReport == "Pre 2002 Report")
                    trustCompany = "Pre2002";
                else
                    return;
            }


            if (G1.get_column_number(dt, "mod") < 0)
                dt.Columns.Add("mod");

            DataRow dRow = dt.NewRow();

            dRow["trust"] = trustCompany;
            //dRow["trustCompany"] = trustCompany;
            //dRow["trustName"] = dr["trustName"].ObjToString();
            //dRow["contractNumber"] = dr["contractNumber"].ObjToString();
            //dRow["contract"] = dr["contractNumber"].ObjToString();
            //dRow["insuredName"] = "PD - " + dr["insuredName"].ObjToString();
            //dRow["desc"] = "PD - " + dr["insuredName"].ObjToString();
            //dRow["funeral"] = dr["funeral"].ObjToString();
            //DateTime date = dr["date"].ObjToDateTime();
            //dRow["date"] = G1.DTtoMySQLDT(date);
            //date = dr["dateReceived"].ObjToDateTime();
            //dRow["dateReceived"] = G1.DTtoMySQLDT(date);
            //dRow["value"] = dr["value"].ObjToDouble();
            //dRow["principal"] = dr["principal"].ObjToDouble();
            //dRow["status"] = "Line Edit";
            //dRow["firstName"] = "SPLIT";
            dRow["mod"] = "Y";
            dt.Rows.InsertAt(dRow, row);

            //dr["firstName"] = "REPLACE";

            G1.NumberDataTable(dt);
            dt.AcceptChanges();
            dgv.DataSource = dt;
            gridMain.ClearSelection();
            gridMain.RefreshData();
            gridMain.FocusedRowHandle = rowHandle + 1;
            gridMain.SelectRow(rowHandle + 1);
            dgv.Refresh();
        }
        /****************************************************************************************/
        private void Insert_Split_Row(DevExpress.XtraGrid.GridControl dgv, DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gridMain, bool zeroOkay = false)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt.Rows.Count <= 0)
                return;
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            if (row == 0 && !zeroOkay)
            {
                MessageBox.Show("*** ERROR *** You can not insert a new row before the first row!", "Bad Insert Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }
            if (row < 0 || row > (dt.Rows.Count - 1))
                return;
            if (rowHandle > (dt.Rows.Count - 1))
                return; // Already at the last row

            string trustCompany = dt.TableName.Trim();
            trustCompany = dr["trust"].ObjToString();
            if (String.IsNullOrWhiteSpace(trustCompany))
            {
                if (workReport == "Post 2002 Report - SN & FT")
                    trustCompany = "SNFT";
                else if (workReport == "Post 2002 Report - Unity")
                    trustCompany = "Unity";
                else if (workReport == "Post 2002 Report - FDLIC")
                    trustCompany = "FDLIC";
                else if (workReport == "Post 2002 Report - CD")
                    trustCompany = "CD";
                else if (workReport == "Pre 2002 Report")
                    trustCompany = "Pre2002";
                else
                    return;
            }

            string policyStatus = dr["policyStatus"].ObjToString().ToUpper();
            if ( policyStatus == "SPLIT")
            {
                MessageBox.Show("*** ERROR *** You can not SPLIT a SPLIT row!", "Bad SPLIT Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }

            string preOrPost = cmbPreOrPost.Text.Trim();
            string contractNumber = dr["contractNumber"].ObjToString();
            string trustName = dr["trustName"].ObjToString();
            string policyNumber = dr["policyNumber"].ObjToString();
            string insuredName = dr["insuredName"].ObjToString();
            if (String.IsNullOrWhiteSpace(insuredName))
                dr["insuredName"] = dr["desc"].ObjToString();
            insuredName = "PD - " + dr["insuredName"].ObjToString();
            string funeral = dr["funeral"].ObjToString();
            DateTime date = dr["date"].ObjToDateTime();
            DateTime dateReceived = dr["dateReceived"].ObjToDateTime();
            double value = dr["value"].ObjToDouble();
            double principal = dr["principal"].ObjToDouble();
            string firstName = "";
            string lastName = "";
            if (insuredName == "PD - ")
                insuredName += dr["desc"].ObjToString();

            if (workReport == "Post 2002 Report - SN & FT")
                trustName = "SNFT";
            else if (workReport == "Post 2002 Report - Unity")
                trustName = "Unity";
            else if (workReport == "Post 2002 Report - FDLIC")
                trustName = "FDLIC";
            else if (workReport == "Post 2002 Report - CD")
                trustName = "CD";
            else if (workReport == "Pre 2002 Report")
                trustName = "Pre2002";

            if (G1.get_column_number(dt, "mod") < 0)
                dt.Columns.Add("mod");

            DataRow dRow = dt.NewRow();

            dRow["trust"] = trustCompany;
            dRow["trustCompany"] = trustCompany;
            dRow["trustName"] = trustName;
            dRow["contractNumber"] = contractNumber;
            dRow["contract"] = contractNumber;
            dRow["insuredName"] = insuredName;
            dRow["desc"] = insuredName;
            dRow["policyNumber"] = policyNumber;
            dRow["funeral"] = funeral;
            dRow["date"] = G1.DTtoMySQLDT(date);
            dRow["dateReceived"] = G1.DTtoMySQLDT(dateReceived);
            dRow["value"] = value;
            dRow["principal"] = principal;
            dRow["status"] = "Line Edit";
            dRow["firstName"] = "SPLIT";
            dRow["policyStatus"] = "SPLIT";
            dRow["mod"] = "Y";
            dt.Rows.InsertAt(dRow, row);

            dr["middleName"] = "REPLACE";

            string record = G1.create_record("trust_data_edits", "status", "-1");
            if (G1.BadRecord("trust_data_edits", record))
                return;

            dRow["record"] = record.ObjToInt32();
            G1.update_db_table("trust_data_edits", "record", record, new string[] { "status", "Line Edit", "preOrPost", preOrPost, "trustCompany", trustCompany, "trustName", trustName, "contractNumber", contractNumber, "policyNumber", policyNumber, "insuredName", insuredName, "date", date.ToString("yyyyMMdd"), "policyStatus", "SPLIT" });


            double smfsBalance = dr["smfsBalance"].ObjToDouble();
            double balance = dr["value"].ObjToDouble();
            double ftBalance = dr["received"].ObjToDouble();
            ftBalance = dr["principal"].ObjToDouble();

            G1.update_db_table("trust_data_edits", "record", record, new string[] { "beginningPaymentBalance", balance.ToString(), "endingPaymentBalance", smfsBalance.ToString(), "beginningDeathBenefit", ftBalance.ToString() });

            record = dr["record"].ObjToString();
            G1.update_db_table("trust_data", "record", record, new string[] { "middleName", "REPLACE" });

            G1.NumberDataTable(dt);
            dt.AcceptChanges();
            dgv.DataSource = dt;
            gridMain.ClearSelection();
            gridMain.RefreshData();
            gridMain.FocusedRowHandle = rowHandle + 1;
            gridMain.SelectRow(rowHandle + 1);
            dgv.Refresh();
        }
        /****************************************************************************************/
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (workReport == "Pre 2002 Report")
            {
                if (dgv6.Visible)
                    Delete_Row(dgv6, gridMain6);
                else if (dgv10.Visible)
                    Delete_Row(dgv10, gridMain10);
                else if (dgv11.Visible)
                    Delete_Row(dgv11, gridMain11);
                else if (dgv12.Visible)
                    Delete_Row(dgv12, gridMain12);
                else if (dgv13.Visible)
                    Delete_Row(dgv13, gridMain13);
                else if (dgv14.Visible)
                    Delete_Row(dgv14, gridMain14);
            }
            else
            {
                if (dgv2.Visible)
                    Delete_Row(dgv2, gridMain2);
            }
        }
        /****************************************************************************************/
        private void Delete_Row(DevExpress.XtraGrid.GridControl dgvS, DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gridMainS)
        {
            DataRow dr = gridMainS.GetFocusedDataRow();
            DialogResult result = MessageBox.Show("***Question*** Are you sure you want to delete this row?", "Delete Row Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            if (result == DialogResult.No)
                return;
            int rowHandle = gridMainS.FocusedRowHandle;
            DataTable dt = (DataTable)dgvS.DataSource;

            if (G1.get_column_number(dt, "mod") < 0)
                dt.Columns.Add("mod");

            int[] rows = gridMainS.GetSelectedRows();
            int dtRow = 0;
            int firstRow = 0;
            if (rows.Length > 0)
                firstRow = rows[0];
            int row = 0;
            string status = "";
            try
            {
                loading = true;
                for (int i = 0; i < rows.Length; i++)
                {
                    row = rows[i];
                    dtRow = gridMainS.GetDataSourceRowIndex(row);
                    if (dtRow < 0 || dtRow > (dt.Rows.Count - 1))
                    {
                        continue;
                    }
                    var dRow = gridMainS.GetDataRow(row);
                    if (dRow != null)
                    {
                        status = dRow["status"].ObjToString();
                        if (status.IndexOf("Line Edit") < 0)
                            continue;
                        dRow["mod"] = "D";
                    }
                    dt.Rows[dtRow]["mod"] = "D";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("*ERROR*** " + ex.Message.ToString());
            }

            string mod = "";
            string record = "";
            DataTable mainDt = null;
            DataRow[] dRows = null;
            for (int i = dt.Rows.Count - 1; i >= 0; i--)
            {
                mod = dt.Rows[i]["mod"].ObjToString();
                if (mod == "D")
                {
                    status = dt.Rows[i]["status"].ObjToString();
                    record = dt.Rows[i]["record"].ObjToString();
                    if (record == "0")
                        continue;
                    if (!String.IsNullOrWhiteSpace(record))
                        G1.delete_db_table("trust_data_edits", "record", record);
                    dt.Rows.RemoveAt(i);

                    if (status == "Main Line Edit" && dgv2.Visible)
                    {
                        mainDt = (DataTable)dgv.DataSource;
                        dRows = mainDt.Select("record='" + record + "' AND status='Main Line Edit'");
                        if (dRows.Length > 0)
                        {
                            mainDt.Rows.Remove(dRows[0]);
                            originalDt = mainDt;
                        }
                    }
                    //else if (status == "Main Line Edit" && dgv10.Visible)
                    //{
                    //    mainDt = (DataTable)dgv.DataSource;
                    //    dRows = mainDt.Select("record='" + record + "' AND status='Main Line Edit'");
                    //    if (dRows.Length > 0)
                    //    {
                    //        mainDt.Rows.Remove(dRows[0]);
                    //        originalDt = mainDt;
                    //    }
                    //}
                }
            }

            loading = false;
            if (firstRow > (dt.Rows.Count - 1))
                firstRow = (dt.Rows.Count - 1);
            dgvS.DataSource = dt;
            gridMainS.RefreshEditor(true);
            gridMainS.RefreshData();
            dgvS.Refresh();

            gridMainS.FocusedRowHandle = firstRow;
            gridMainS.SelectRow(firstRow);
        }
        /****************************************************************************************/
        //dRow["Forethought"] = G1.ReformatMoney(foreThought);            // beginningPaymentBalance
        //dRow["Security National"] = G1.ReformatMoney(securityNational); // beginningDeathBenefit
        //dRow["FDLIC Old Webb"] = G1.ReformatMoney(fdlicOldWebb);        // endingPaymentBalance
        //dRow["FDLIC Old CCI"] = G1.ReformatMoney(fdlicOldCCI);          // endingDeathBenefit
        //dRow["Unity Old Barham"] = G1.ReformatMoney(unityOldBarham);    // priorUnappliedCash
        //dRow["Unity Old Webb"] = G1.ReformatMoney(unityOldWebb);        // currentUnappliedCash

        /****************************************************************************************/
        private void gridMain6_CellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            if (e == null)
                return;
            gridMain6_DataValueChanged();
        }
        /****************************************************************************************/
        private void gridMain6_DataValueChanged( bool empty = false )
        {
            try
            {
                int rowHandle = gridMain6.FocusedRowHandle;
                int row = gridMain6.GetDataSourceRowIndex(rowHandle);
                DataRow dr = gridMain6.GetFocusedDataRow();
                if (dr == null)
                    return;

                DataTable dt = (DataTable)dgv6.DataSource;
                string record = dr["record"].ObjToString();
                if (record == "0")
                    record = "";

                DateTime date = this.dateTimePicker2.Value;

                //dRow["Forethought"] = G1.ReformatMoney(foreThought);            // beginningPaymentBalance
                //dRow["Security National"] = G1.ReformatMoney(securityNational); // beginningDeathBenefit
                //dRow["FDLIC Old Webb"] = G1.ReformatMoney(fdlicOldWebb);        // endingPaymentBalance
                //dRow["FDLIC Old CCI"] = G1.ReformatMoney(fdlicOldCCI);          // endingDeathBenefit
                //dRow["Unity Old Barham"] = G1.ReformatMoney(unityOldBarham);    // priorUnappliedCash
                //dRow["Unity Old Webb"] = G1.ReformatMoney(unityOldWebb);        // currentUnappliedCash

                string trustCompany = "Pre2002";
                double CD = dr["CD"].ObjToDouble();
                double Forethought = dr["Forethought"].ObjToDouble();
                double SecurityNational = dr["Security National"].ObjToDouble();
                double FdlicOldWebb = dr["FDLIC Old Webb"].ObjToDouble();
                double FdlicOldCCI = dr["FDLIC Old CCI"].ObjToDouble();
                double UnityOldBarham = dr["Unity Old Barham"].ObjToDouble();
                double UnityOldWebb = dr["Unity Old Webb"].ObjToDouble();

                string snDesc = "";
                if (G1.get_column_number(dt, "Security National Desc") > 0)
                    snDesc = dr["Security National Desc"].ObjToString();
                string ftDesc = "";
                if (G1.get_column_number(dt, "Forethought Desc") > 0)
                    ftDesc = dr["Forethought Desc"].ObjToString();

                string month = dr["month"].ObjToString().ToUpper();
                month = dr["status"].ObjToString().ToUpper();

                if (month != "BEGINNING BALANCE" && month != "BEGINNING ADJUSTMENT")
                {
                    if (month == "ENDING BALANCE" || month == "ENDING MANUAL ADJUSTMENT")
                        endingDataChanged();
                    else
                    {
                        if (String.IsNullOrWhiteSpace(record))
                        {
                            record = G1.create_record("trust_data_edits", "status", "-1");
                            dr["record"] = record.ObjToInt32();
                            G1.update_db_table("trust_data_edits", "record", record, new string[] { "status", "Line Edit", "preOrPost", "Pre" });
                        }
                        if (G1.BadRecord("trust_data_edits", record))
                            return;

                        if ( !empty )
                            G1.update_db_table("trust_data_edits", "record", record, new string[] { "beginningPaymentBalance", Forethought.ToString(), "beginningDeathBenefit", SecurityNational.ToString(), "endingPaymentBalance", FdlicOldWebb.ToString(), "endingDeathBenefit", FdlicOldCCI.ToString(), "priorUnappliedCash", UnityOldBarham.ToString(), "currentUnappliedCash", UnityOldWebb.ToString(), "downPayments", CD.ToString(), "date", date.ToString("yyyy-MM-dd"), "trustName", trustCompany, "position", row.ToString(), "lastName", snDesc, "firstName", ftDesc });
                        else
                            G1.update_db_table("trust_data_edits", "record", record, new string[] { "date", date.ToString("yyyy-MM-dd"), "trustName", trustCompany, "position", row.ToString() });

                        gridMain6.RefreshEditor(true);
                        gridMain6.RefreshData();
                        dgv6.Refresh();
                        gridMain6.PostEditor();
                    }
                    //else
                    //    LoadEndingBalances(dt); // Raw Data Must Have Changed
                    return;
                }


                string colName = gridMain6.FocusedColumn.FieldName;
                //dRow["Forethought"] = G1.ReformatMoney(foreThought);            // beginningPaymentBalance
                //dRow["Security National"] = G1.ReformatMoney(securityNational); // beginningDeathBenefit
                //dRow["FDLIC Old Webb"] = G1.ReformatMoney(fdlicOldWebb);        // endingPaymentBalance
                //dRow["FDLIC Old CCI"] = G1.ReformatMoney(fdlicOldCCI);          // endingDeathBenefit
                //dRow["Unity Old Barham"] = G1.ReformatMoney(unityOldBarham);    // priorUnappliedCash
                //dRow["Unity Old Webb"] = G1.ReformatMoney(unityOldWebb);        // currentUnappliedCash

                if (colName != "Forethought" && colName != "Security National" && colName != "FDLIC Old Webb" &&
                    colName != "FDLIC Old CCI" && colName != "Unity Old Barham" && colName != "Unity Old Webb" &&
                    colName != "CD")
                    return;

                double dValue = dr[colName].ObjToDouble();
                dr[colName] = G1.ReformatMoney(dValue);

                string mainUpdate = "BeginningBalance";
                if (month == "BEGINNING BALANCE")
                {
                    mainUpdate = "BeginningBalance";
                    if (String.IsNullOrWhiteSpace(record))
                    {
                        record = G1.create_record("trust_data_edits", "status", "-1");
                        dr["record"] = record.ObjToInt32();
                        G1.update_db_table("trust_data_edits", "record", record, new string[] { "status", mainUpdate, "preOrPost", "Pre" });
                    }
                    if (G1.BadRecord("trust_data_edits", record))
                        return;
                    G1.update_db_table("trust_data_edits", "record", record, new string[] { "status", mainUpdate, "beginningPaymentBalance", Forethought.ToString(), "beginningDeathBenefit", SecurityNational.ToString(), "endingPaymentBalance", FdlicOldWebb.ToString(), "endingDeathBenefit", FdlicOldCCI.ToString(), "priorUnappliedCash", UnityOldBarham.ToString(), "currentUnappliedCash", UnityOldWebb.ToString(), "downPayments", CD.ToString(), "date", date.ToString("yyyy-MM-dd"), "trustName", trustCompany });
                }

                //LoadEndingBalances(dt);
            }
            catch ( Exception ex)
            {
            }
        }
        /****************************************************************************************/
        private void GetBeginningBalancesPre2002(DataTable dt, ref double CD, ref double bankCD, ref double Forethought, ref double SecurityNational, ref double FdlicOldWebb, ref double FdlicOldCCI, ref double UnityOldBarham, ref double UnityOldWebb, ref string record)
        {
            //if (1 == 1)
            //    return;
            //LoadBeginningAdjustment(dt);

            //dRow["Forethought"] = G1.ReformatMoney(foreThought);            // beginningPaymentBalance
            //dRow["Security National"] = G1.ReformatMoney(securityNational); // beginningDeathBenefit
            //dRow["FDLIC Old Webb"] = G1.ReformatMoney(fdlicOldWebb);        // endingPaymentBalance
            //dRow["FDLIC Old CCI"] = G1.ReformatMoney(fdlicOldCCI);          // endingDeathBenefit
            //dRow["Unity Old Barham"] = G1.ReformatMoney(unityOldBarham);    // priorUnappliedCash
            //dRow["Unity Old Webb"] = G1.ReformatMoney(unityOldWebb);        // currentUnappliedCash

            Forethought = 0D;
            SecurityNational = 0D;
            FdlicOldWebb = 0D;
            FdlicOldCCI = 0D;
            UnityOldBarham = 0D;
            UnityOldWebb = 0D;
            bankCD = 0D;

            record = "";
            DataTable activeDt = null;

            string trustCompany = dt.TableName.Trim();
            trustCompany = "Pre2002";

            string str = "";

            bool useTrustCalculatedBeginningBalance = true;
            bool useTrustCalculatedEndingBalance = true;
            bool useSMFSCalculatedBeginningBalance = true;
            bool useSMFSCalculatedEndingBalance = true;

            string[] balances = this.chkBalances.EditValue.ToString().Split('|');
            for (int i = 0; i < chkBalances.Properties.Items.Count; i++)
            {
                str = chkBalances.Properties.Items[i].Description.ObjToString();
                if (chkBalances.Properties.Items[i].CheckState == CheckState.Checked)
                {
                    if (str == "Use SMFS Manually Entered Beginning Balance")
                        useSMFSCalculatedBeginningBalance = false;
                    else if (str == "Use SMFS Manually Entered Ending Balance")
                        useSMFSCalculatedEndingBalance = false;
                    if (str == "Use Trust Manually Entered Beginning Balance")
                        useTrustCalculatedBeginningBalance = false;
                    else if (str == "Use Trust Manually Entered Ending Balance")
                        useTrustCalculatedEndingBalance = false;
                }
            }

            DateTime date = this.dateTimePicker2.Value;
            DateTime lastMonth = date.AddMonths(-1);
            int days = DateTime.DaysInMonth(lastMonth.Year, lastMonth.Month);
            lastMonth = new DateTime(lastMonth.Year, lastMonth.Month, days);

            string cmd = "Select * from `trust_data_edits` where `date` = '" + date.ToString("yyyy-MM-dd") + "' AND `status` = 'BeginningBalance' AND `trustName` = '" + trustCompany + "' AND `preOrPost` = 'Pre';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                CD = dx.Rows[0]["downPayments"].ObjToDouble();
                bankCD = dx.Rows[0]["growth"].ObjToDouble();
                Forethought = dx.Rows[0]["beginningPaymentBalance"].ObjToDouble();
                SecurityNational = dx.Rows[0]["beginningDeathBenefit"].ObjToDouble();
                FdlicOldWebb = dx.Rows[0]["endingPaymentBalance"].ObjToDouble();
                FdlicOldCCI = dx.Rows[0]["endingDeathBenefit"].ObjToDouble();
                UnityOldBarham = dx.Rows[0]["priorUnappliedCash"].ObjToDouble();
                UnityOldWebb = dx.Rows[0]["currentUnappliedCash"].ObjToDouble();
                record = dx.Rows[0]["record"].ObjToString();
            }
            else
            {
                cmd = "Select * from `trust_data_edits` where `date` = '" + lastMonth.ToString("yyyy-MM-dd") + "' AND `status` = 'EndingBalance' AND `trustName` = '" + trustCompany + "' AND `preOrPost` = 'Pre';";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    CD = dx.Rows[0]["downPayments"].ObjToDouble();
                    bankCD = dx.Rows[0]["growth"].ObjToDouble();
                    Forethought = dx.Rows[0]["beginningPaymentBalance"].ObjToDouble();
                    SecurityNational = dx.Rows[0]["beginningDeathBenefit"].ObjToDouble();
                    FdlicOldWebb = dx.Rows[0]["endingPaymentBalance"].ObjToDouble();
                    FdlicOldCCI = dx.Rows[0]["endingDeathBenefit"].ObjToDouble();
                    UnityOldBarham = dx.Rows[0]["priorUnappliedCash"].ObjToDouble();
                    UnityOldWebb = dx.Rows[0]["currentUnappliedCash"].ObjToDouble();
                    record = dx.Rows[0]["record"].ObjToString();
                    record = "";
                }
            }

            if ( bankCD == 0D)
            {
                cmd = "Select * from `trust_data_edits` where `date` = '" + lastMonth.ToString("yyyy-MM-dd") + "' AND `status` = 'EndingBalance' AND `trustName` = '" + trustCompany + "' AND `preOrPost` = 'Pre';";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    bankCD = dx.Rows[0]["growth"].ObjToDouble();
                }
            }

            //dRow["Forethought"] = G1.ReformatMoney(foreThought);            // beginningPaymentBalance
            //dRow["Security National"] = G1.ReformatMoney(securityNational); // beginningDeathBenefit
            //dRow["FDLIC Old Webb"] = G1.ReformatMoney(fdlicOldWebb);        // endingPaymentBalance
            //dRow["FDLIC Old CCI"] = G1.ReformatMoney(fdlicOldCCI);          // endingDeathBenefit
            //dRow["Unity Old Barham"] = G1.ReformatMoney(unityOldBarham);    // priorUnappliedCash
            //dRow["Unity Old Webb"] = G1.ReformatMoney(unityOldWebb);        // currentUnappliedCash

            //DataRow dRow = dt.NewRow();
            //dRow["record"] = record.ObjToInt32();
            //dRow["month"] = "Beginning Balance";
            //dRow["Forethought"] = Forethought;
            //dRow["Security National"] = SecurityNational;
            //dRow["FDLIC Old Webb"] = FdlicOldWebb;
            //dRow["FDLIC Old CCI"] = FdlicOldCCI;
            //dRow["Unity Old Barham"] = UnityOldBarham;
            //dRow["Unity Old Webb"] = UnityOldWebb;
            //dt.Rows.InsertAt(dRow, 0);
        }
        /****************************************************************************************/
        private void CalcEndingBalancesPre2002(DataTable dt, ref double CD, ref double bankCD, ref double Forethought, ref double SecurityNational, ref double FdlicOldWebb, ref double FdlicOldCCI, ref double UnityOldBarham, ref double UnityOldWebb, ref string record)
        {
            //dRow["Forethought"] = G1.ReformatMoney(foreThought);            // beginningPaymentBalance
            //dRow["Security National"] = G1.ReformatMoney(securityNational); // beginningDeathBenefit
            //dRow["FDLIC Old Webb"] = G1.ReformatMoney(fdlicOldWebb);        // endingPaymentBalance
            //dRow["FDLIC Old CCI"] = G1.ReformatMoney(fdlicOldCCI);          // endingDeathBenefit
            //dRow["Unity Old Barham"] = G1.ReformatMoney(unityOldBarham);    // priorUnappliedCash
            //dRow["Unity Old Webb"] = G1.ReformatMoney(unityOldWebb);        // currentUnappliedCash

            CD = 0D;
            bankCD = 0D;
            Forethought = 0D;
            SecurityNational = 0D;
            FdlicOldWebb = 0D;
            FdlicOldCCI = 0D;
            UnityOldBarham = 0D;
            UnityOldWebb = 0D;

            record = "";
            string status = "";

            double dValue = 0D;
            double ft = 0D;
            double secnat = 0D;
            double fdlicOW = 0D;
            double fdlicCCI = 0D;
            double unityOB = 0D;
            double unityOW = 0D;
            string desc = "";

            GetBeginningBalancesPre2002(dt, ref CD, ref bankCD, ref Forethought, ref SecurityNational, ref FdlicOldWebb, ref FdlicOldCCI, ref UnityOldBarham, ref UnityOldWebb, ref record);

            string cmd = "";
            DateTime date = this.dateTimePicker2.Value;

            string startDate = this.dateTimePicker1.Value.ToString("yyyy-MM-dd");
            string stopDate = this.dateTimePicker2.Value.ToString("yyyy-MM-dd");

            cmd = "Select * from `trust_data_edits` WHERE `trustName` = 'Pre2002' AND `status` = 'Line Edit' AND `date` >= '" + startDate + "' AND `date` <= '" + stopDate + "' AND `preOrPost` = 'Pre' AND `policyStatus` <> 'SPLIT';";
            DataTable dd = G1.get_db_data(cmd);
            if (dd.Rows.Count > 0)
                dt = LoadLineEditsPre(dt, dd);

            int snCol = G1.get_column_number(dt, "Security National Desc");
            int ftCol = G1.get_column_number(dt, "Forethought Desc");
            int fdlicOWCol = G1.get_column_number(dt, "FDLIC Old Webb Desc");
            int fdlicCCICol = G1.get_column_number(dt, "FDLIC Old CCI Desc");
            int unityOBCol = G1.get_column_number(dt, "Unity Old Barham Desc");
            int unityOWCol = G1.get_column_number(dt, "Unity Old Webb Desc");

            string firstName = "";
            string lastName = "";
            string middleName = "";
            double security = 0D;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                status = dt.Rows[i]["status"].ObjToString().ToUpper();
                if (status.ToUpper() == "BEGINNING BALANCE")
                    continue;
                firstName = dt.Rows[i]["firstName"].ObjToString().ToUpper();
                lastName = dt.Rows[i]["lastName"].ObjToString().ToUpper();

                middleName = dt.Rows[i]["middleName"].ObjToString().ToUpper();
                if (middleName == "REPLACE")
                    continue;

                if (String.IsNullOrWhiteSpace(status) && firstName == "LINE EDIT")
                    status = firstName;
                else if (String.IsNullOrWhiteSpace(status) && lastName == "LINE EDIT")
                    status = lastName;
                //if ( status == "LINE EDIT")
                //{
                //if ( snCol >= 0 )
                //{
                //    desc = dt.Rows[i][snCol].ObjToString();
                //    if (desc.ToUpper().IndexOf("PD") == 0)
                //        continue;

                //}
                if (CheckPreIncludeEdits(dt, i, snCol, firstName, lastName))
                {
                    secnat += dt.Rows[i]["Security National"].ObjToDouble();
                    //continue;
                }
                if (CheckPreIncludeEdits(dt, i, ftCol, firstName, lastName))
                {
                    ft += dt.Rows[i]["Forethought"].ObjToDouble();
                    //continue;
                }
                if (!CheckPreIncludeEdits(dt, i, fdlicOWCol, firstName, lastName))
                {
                    fdlicOW += dt.Rows[i]["FDLIC Old Webb"].ObjToDouble();
                    //continue;
                }
                if (CheckPreIncludeEdits(dt, i, fdlicCCICol, firstName, lastName))
                {
                    fdlicCCI += dt.Rows[i]["FDLIC Old CCI"].ObjToDouble();
                    //continue;
                }
                if (CheckPreIncludeEdits(dt, i, unityOBCol, firstName, lastName))
                {
                    unityOB += dt.Rows[i]["Unity Old Barham"].ObjToDouble();
                    //continue;
                }
                if (CheckPreIncludeEdits(dt, i, unityOWCol, firstName, lastName))
                {
                    unityOW += dt.Rows[i]["Unity Old Webb"].ObjToDouble();
                    //continue;
                }
                //}

                //security = dt.Rows[i]["Security National"].ObjToDouble();
                //ft += dt.Rows[i]["Forethought"].ObjToDouble();
                //secnat += dt.Rows[i]["Security National"].ObjToDouble();
                //fdlicOW += dt.Rows[i]["FDLIC Old Webb"].ObjToDouble();
                //fdlicCCI += dt.Rows[i]["FDLIC Old CCI"].ObjToDouble();
                //unityOB += dt.Rows[i]["Unity Old Barham"].ObjToDouble();
                //unityOW += dt.Rows[i]["Unity Old Webb"].ObjToDouble();
            }

            Forethought += ft;
            SecurityNational += secnat;
            FdlicOldWebb += fdlicOW;
            FdlicOldCCI += fdlicCCI;
            UnityOldBarham += unityOB;
            UnityOldWebb += unityOW;
        }
        /****************************************************************************************/
        private bool CheckPreIncludeEdits ( DataTable dt, int row, int col, string firstName, string lastName )
        {
            bool include = true;
            if (col < 0)
                return include;

            string desc = dt.Rows[row][col].ObjToString();
            if (String.IsNullOrWhiteSpace(desc))
                return include;

            if (desc.ToUpper().IndexOf("PD") == 0)
            {
                include = false;
                if (firstName.Trim().ToUpper() == "LINE EDIT")
                    include = true;
                else if (lastName.Trim().ToUpper() == "LINE EDIT")
                    include = true;
            }

            else if (desc.ToUpper().IndexOf("DC PAID") >= 0)
                include = false;
            else if (desc.ToUpper().IndexOf("DC CASH") >= 0)
                include = false;
            return include;
        }
        /****************************************************************************************/
        private void gridMain6_CustomSummaryCalculate(object sender, DevExpress.Data.CustomSummaryEventArgs e)
        {
            try
            {
                string what = "";
                double value = e.TotalValue.ObjToDouble();
                string field = (e.Item as GridSummaryItem).FieldName.ObjToString();
                DataTable dt = (DataTable)dgv6.DataSource;

                double totalValue = 0D;
                double totalReceived = 0D;
                double dValue = 0D;
                string manual = "";

                string status = "";

                double ft = 0D;
                double secnat = 0D;
                double fdlicOW = 0D;
                double fdlicCCI = 0D;
                double unityOB = 0D;
                double unityOW = 0D;

                for (int i = 1; i < dt.Rows.Count; i++)
                {
                    what = dt.Rows[i]["month"].ObjToString().Trim();
                    if (what.ToUpper() == "BEGINNING")
                        what = "";
                    else if (what.ToUpper() == "BALANCE")
                        what = "";
                    if (!String.IsNullOrWhiteSpace(what))
                    {
                        manual = dt.Rows[i]["manual"].ObjToString().ToUpper();
                        if (manual != "Y")
                            continue;
                    }

                    ft += addPreTotal(dt, i, "Forethought");
                    secnat += addPreTotal(dt, i, "Security National");
                    fdlicOW += addPreTotal(dt, i, "FDLIC Old Webb");
                    fdlicCCI += addPreTotal(dt, i, "FDLIC Old CCI");
                    unityOB += addPreTotal(dt, i, "Unity Old Barham");
                    unityOW += addPreTotal(dt, i, "Unity Old Webb");
                }


                if (field.ToUpper() == "FORETHOUGHT")
                {
                    e.TotalValueReady = true;
                    e.TotalValue = ft;
                    gridMain6.PostEditor();
                    gridMain6.RefreshEditor(true);
                }
                else if (field.ToUpper() == "SECURITY NATIONAL")
                {
                    e.TotalValueReady = true;
                    e.TotalValue = secnat;
                    gridMain6.PostEditor();
                    gridMain6.RefreshEditor(true);
                }
                else if (field.ToUpper() == "FDLIC OLD WEBB")
                {
                    e.TotalValueReady = true;
                    e.TotalValue = fdlicOW;
                    gridMain6.PostEditor();
                    gridMain6.RefreshEditor(true);
                }
                else if (field.ToUpper() == "FDLIC OLD CCI")
                {
                    e.TotalValueReady = true;
                    e.TotalValue = fdlicCCI;
                    gridMain6.PostEditor();
                    gridMain6.RefreshEditor(true);
                }
                else if (field.ToUpper() == "UNITY OLD BARHAM")
                {
                    e.TotalValueReady = true;
                    e.TotalValue = unityOB;
                    gridMain6.PostEditor();
                    gridMain6.RefreshEditor(true);
                }
                else if (field.ToUpper() == "UNITY OLD WEBB")
                {
                    e.TotalValueReady = true;
                    e.TotalValue = unityOW;
                    gridMain6.PostEditor();
                    gridMain6.RefreshEditor(true);
                }
            }
            catch (Exception ex)
            {
            }
        }
        /****************************************************************************************/
        private double addPreTotal ( DataTable dt, int row, string column )
        {
            double dValue = 0D;
            if (G1.get_column_number(dt, column) < 0)
                return dValue;
            if (G1.get_column_number(dt, column + " desc") < 0)
                return dValue;

            bool doit = true;
            string desc = dt.Rows[row][column + " desc"].ObjToString();
            if (desc.ToUpper().IndexOf("PD") == 0)
                doit = false;
            if (desc.ToUpper().IndexOf("TOTAL") == 0)
                doit = false;
            if (desc.ToUpper().IndexOf ("DC PAID") == 0 )
                doit = false;
            if (desc.ToUpper().IndexOf("DC CASH") == 0)
                doit = false;

            if (doit)
                dValue = dt.Rows[row][column].ObjToDouble();
            return dValue;
        }
        /****************************************************************************************/
        private void gridMain2_RowCellStyle(object sender, RowCellStyleEventArgs e)
        {
            GridView View = sender as GridView;
            if (e.RowHandle >= 0)
            {
                string column = e.Column.FieldName.ToUpper();
                if (column == "NUM")
                {
                    bool done = false;
                    DataTable dt = (DataTable)dgv2.DataSource;
                    int row = gridMain2.GetDataSourceRowIndex(e.RowHandle);

                    string manual = dt.Rows[row]["manual"].ObjToString();
                    if (manual.Trim().ToUpper() == "Y")
                    {
                        e.Appearance.BackColor = Color.Red;
                        done = true;
                    }
                    else
                        e.Appearance.BackColor = Color.Transparent;
                    if ( !done )
                    {
                        string status = dt.Rows[row]["status"].ObjToString();
                        if (status.IndexOf("Main Line Edit") >= 0)
                            e.Appearance.BackColor = Color.Pink;
                        else
                            e.Appearance.BackColor = Color.Transparent;
                    }
                }
                else if (column == "DESC" || column == "VALUE" || column == "CONTRACT" || column == "FUNERAL" || column == "DATE" || column == "CASHPAID1" )
                {
                    DataTable dt = (DataTable)dgv2.DataSource;
                    int row = gridMain2.GetDataSourceRowIndex(e.RowHandle);

                    string manual = "";
                    if ( G1.get_column_number (dt, "red1") >= 0 )
                        manual = dt.Rows[row]["red1"].ObjToString();
                    if (manual.Trim().ToUpper() == "Y")
                    {
                        e.Appearance.ForeColor = Color.Red;
                        ColorizeCell(e.Appearance);
                    }
                    else
                    {
                        DateTime date = dt.Rows[row]["date"].ObjToDateTime();
                        if (date > this.dateTimePicker2.Value)
                        {
                            e.Appearance.ForeColor = Color.Red;
                            ColorizeCell(e.Appearance);
                        }
                        else
                        {
                            string status = dt.Rows[row]["status"].ObjToString();
                            if (status == "Main Line Edit")
                            {
                                e.Appearance.ForeColor = Color.Red;
                                ColorizeCell(e.Appearance);
                            }
                        }
                    }
                }
                else if (column == "OTHERDESC" || column == "RECEIVED" || column == "OTHERCONTRACT" || column == "OTHERFUNERAL" || column == "DATERECEIVED" || column == "CASHPAID2")
                {
                    DataTable dt = (DataTable)dgv2.DataSource;
                    int row = gridMain2.GetDataSourceRowIndex(e.RowHandle);

                    string manual = "";
                    if (G1.get_column_number(dt, "red2") >= 0)
                        manual = dt.Rows[row]["red2"].ObjToString();
                    if (manual.Trim().ToUpper() == "Y")
                    {
                        e.Appearance.ForeColor = Color.Red;
                        ColorizeCell(e.Appearance);
                    }
                    else
                    {
                        DateTime date = dt.Rows[row]["dateReceived"].ObjToDateTime();
                        if (date > this.dateTimePicker2.Value)
                        {
                            e.Appearance.ForeColor = Color.Red;
                            ColorizeCell(e.Appearance);
                        }
                        else
                        {
                            string status = dt.Rows[row]["status"].ObjToString();
                            if (status == "Main Line Edit")
                            {
                                ColorizeCell(e.Appearance);
                                e.Appearance.ForeColor = Color.Red;
                            }
                        }
                    }
                }
                else
                {
                    DataTable dt = (DataTable)dgv2.DataSource;

                    try
                    {
                        int row = gridMain2.GetDataSourceRowIndex(e.RowHandle);
                        DateTime date = dt.Rows[row]["date"].ObjToDateTime();
                        if (date > this.dateTimePicker2.Value)
                        {
                            e.Appearance.ForeColor = Color.Red;
                            ColorizeCell(e.Appearance);
                        }
                        else
                        {
                            string status = dt.Rows[row]["status"].ObjToString();
                            if (status == "Main Line Edit")
                            {
                                e.Appearance.ForeColor = Color.Red;
                                ColorizeCell(e.Appearance);
                            }
                        }
                    }
                    catch ( Exception ex)
                    {
                    }
                }
            }
        }
        /****************************************************************************************/
        private void ColorizeCell(object appearanceObject)
        {
            //if (fieldName == "Text" && rowHandle >= 0 && rowHandle % 2 != 0)
            //{
                AppearanceObject app = appearanceObject as AppearanceObject;
                if (app != null)
                    app.ForeColor = Color.Red;
                else
                {
                    XlFormattingObject obj = appearanceObject as XlFormattingObject;
                    if (obj != null)
                        obj.BackColor = Color.Red;
                }
            //}
        }  
        /****************************************************************************************/
        private void chkRemoveEmpty_CheckedChanged(object sender, EventArgs e)
        {
            if ( dgv2.Visible )
            {
                gridMain2.RefreshEditor(true);
                gridMain2.RefreshData();
                dgv2.Refresh();
            }
            else  if (dgv6.Visible)
            {
                gridMain6.RefreshEditor(true);
                gridMain6.RefreshData();
                dgv6.Refresh();
            }
        }
        /****************************************************************************************/
        private void gridMain2_CustomRowFilter(object sender, RowFilterEventArgs e)
        {
            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv2.DataSource;
            if (chkRemoveEmpty.Checked )
            {
                string month = dt.Rows[row]["month"].ObjToString();
                if (month.ToUpper().IndexOf("BALANCE") > 0)
                    return;
                if (month.ToUpper().IndexOf("ADJUSTMENT") > 0)
                    return;
                double value = dt.Rows[row]["value"].ObjToDouble();
                double received = dt.Rows[row]["received"].ObjToDouble();
                if ( value == 0D && received == 0D)
                {
                    e.Visible = false;
                    e.Handled = true;
                    return;
                }
            }
        }
        /****************************************************************************************/
        private void pictureBox2_Click(object sender, EventArgs e)
        {
            G1.SpyGlass(gridMain7);
        }
        /****************************************************************************************/
        private void button3_Click(object sender, EventArgs e)
        {
            DateTime now = this.dateTimePicker3.Value;
            now = now.AddMonths(-1);
            now = new DateTime(now.Year, now.Month, 1);
            this.dateTimePicker3.Value = now;

            int days = DateTime.DaysInMonth(now.Year, now.Month);
            this.dateTimePicker4.Value = new DateTime(now.Year, now.Month, days);
            this.dateTimePicker4.Refresh();
            this.dateTimePicker3.Refresh();
        }
        /****************************************************************************************/
        private void button2_Click(object sender, EventArgs e)
        {
            DateTime now = this.dateTimePicker3.Value;
            now = now.AddMonths(1);
            now = new DateTime(now.Year, now.Month, 1);
            this.dateTimePicker3.Value = now;

            int days = DateTime.DaysInMonth(now.Year, now.Month);
            this.dateTimePicker4.Value = new DateTime(now.Year, now.Month, days);
            this.dateTimePicker4.Refresh();
            this.dateTimePicker3.Refresh();
        }
        /****************************************************************************************/
        private void button1_Click(object sender, EventArgs e)
        {
            string preOrPost = cmbPreOrPost2.Text;
            DateTime startDate = this.dateTimePicker3.Value;
            DateTime stopDate = this.dateTimePicker4.Value;

            startDate = new DateTime(startDate.Year, startDate.Month, 1);
            int days = DateTime.DaysInMonth(stopDate.Year, stopDate.Month);
            stopDate = new DateTime(stopDate.Year, stopDate.Month, days);

            string sDate1 = startDate.ToString("yyyy-MM-dd");
            string sDate2 = stopDate.ToString("yyyy-MM-dd");

            string cmd = "Select * from `trust_data_edits` WHERE `date` >= '" + sDate1 + "' AND `date` <= '" + sDate2 + "' AND `status` = 'EndingBalance' AND `preOrPost` = '" + preOrPost + "' ORDER BY `date`;";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return;

            DataTable dx = new DataTable();
            dx.Columns.Add("month");
            dx.Columns.Add("record");

            dx.Columns.Add("Security National", Type.GetType("System.Double"));
            dx.Columns.Add("Forethought", Type.GetType("System.Double"));
            dx.Columns.Add("CD", Type.GetType("System.Double"));
            dx.Columns.Add("Unity", Type.GetType("System.Double"));
            dx.Columns.Add("FDLIC", Type.GetType("System.Double"));
            dx.Columns.Add("total", Type.GetType("System.Double"));

            dx.Columns.Add("Unity PB", Type.GetType("System.Double"));
            dx.Columns.Add("Unity DC", Type.GetType("System.Double"));
            dx.Columns.Add("Unity Total", Type.GetType("System.Double"));
            dx.Columns.Add("Unity Over/Under", Type.GetType("System.Double"));
            dx.Columns.Add("unityOldBarham", Type.GetType("System.Double"));
            dx.Columns.Add("unityOldWebb", Type.GetType("System.Double"));
            dx.Columns.Add("fdlicOldWebb", Type.GetType("System.Double"));
            dx.Columns.Add("fdlicOldCCI", Type.GetType("System.Double"));
            dx.Columns.Add("FDLIC PB", Type.GetType("System.Double"));

            dx.Columns.Add("unityCash", Type.GetType("System.Double"));
            dx.Columns.Add("unityOverUnder", Type.GetType("System.Double"));
            dx.Columns.Add("unityDeathBenefit", Type.GetType("System.Double"));

            dx.Columns.Add("fdlicCash", Type.GetType("System.Double"));
            dx.Columns.Add("fdlicOverUnder", Type.GetType("System.Double"));
            dx.Columns.Add("fdlicDeathBenefit", Type.GetType("System.Double"));

            dx.Columns.Add("bankCD", Type.GetType("System.Double"));

            dx.Columns.Add("year");
            dx.Columns.Add("date");

            DataRow dRow = null;
            DateTime date = startDate;
            double unityCash = 0D;
            double unityDeathBenefit = 0D;

            double fdlicCash = 0D;
            double fdlicDeathBenefit = 0D;

            DataTable activeDt = null;

            this.Cursor = Cursors.WaitCursor;

            int row = 0;

            for(; ;)
            {
                days = DateTime.DaysInMonth(date.Year, date.Month);
                date = new DateTime(date.Year, date.Month, days );

                dRow = dx.NewRow();
                dRow["year"] = date.Year.ToString();
                dRow["month"] = date.ToString("MMMM").ToUpper();
                dRow["date"] = date.ToString("yyyyMMdd");

                if (preOrPost.ToUpper() == "POST")
                {
                    unityCash = loadUnityBalance(date, preOrPost, ref activeDt);
                    dRow["unityCash"] = unityCash;

                    unityDeathBenefit = loadUnityDeathBenefit(date, preOrPost, ref activeDt);
                    dRow["unityDeathBenefit"] = unityDeathBenefit;

                    fdlicCash = loadFDLICBalance(date, preOrPost, ref activeDt);
                    dRow["fdlicCash"] = fdlicCash;

                    fdlicDeathBenefit = loadFDLICDeathBenefit(date, preOrPost, ref activeDt);
                    dRow["fdlicDeathBenefit"] = fdlicDeathBenefit;
                }
                else
                {
                    if (row >= dt.Rows.Count)
                        break;
                    dRow["record"] = dt.Rows[row]["record"].ObjToString();
                    dRow["CD"] = dt.Rows[row]["downPayments"].ObjToDouble();
                    dRow["Forethought"] = dt.Rows[row]["beginningPaymentBalance"].ObjToDouble();
                    dRow["Security National"] = dt.Rows[row]["beginningDeathBenefit"].ObjToDouble();

                    dRow["fdlicOldWebb"] = dt.Rows[row]["endingPaymentBalance"].ObjToDouble();
                    dRow["fdlicOldCCI"] = dt.Rows[row]["endingDeathBenefit"].ObjToDouble();
                    dRow["unityOldBarham"] = dt.Rows[row]["priorUnappliedCash"].ObjToDouble();
                    dRow["unityOldWebb"] = dt.Rows[row]["currentUnappliedCash"].ObjToDouble();

                    dRow["bankCD"] = dt.Rows[row]["payments"].ObjToDouble();
                    dRow["bankCD"] = dt.Rows[row]["growth"].ObjToDouble();

                    row++;
                }

                dx.Rows.Add(dRow);

                date = date.AddMonths(1);
                if (date > stopDate )
                    break;
            }

            double unity = 0D;
            double overUnder = 0D;
            double fdlic = 0D;

            if (preOrPost.ToUpper() == "POST")
            {
                dx = LoadHistory(startDate, stopDate, dt, dx, "SNFT");
                dx = LoadHistory(startDate, stopDate, dt, dx, "Unity");
                dx = LoadHistory(startDate, stopDate, dt, dx, "FDLIC");
                dx = LoadHistory(startDate, stopDate, dt, dx, "CD");

                dx = CalculateTotal(dx, "Security National", "FDLIC", "total");

                for (int i = 0; i < dx.Rows.Count; i++)
                {
                    unity = dx.Rows[i]["unity"].ObjToDouble();
                    unityCash = dx.Rows[i]["unityCash"].ObjToDouble();
                    overUnder = unityCash - unity;
                    dx.Rows[i]["unityOverUnder"] = overUnder;

                    fdlic = dx.Rows[i]["fdlic"].ObjToDouble();
                    fdlicCash = dx.Rows[i]["fdlicCash"].ObjToDouble();
                    overUnder = fdlicCash - fdlic;
                    dx.Rows[i]["fdlicOverUnder"] = overUnder;

                }

                SetPostReport();
            }
            else // Pre2002
            {
                //dx = LoadHistory(startDate, stopDate, dt, dx, "SNFT");
            }

            CleanupHistory( preOrPost );

            if (preOrPost.ToUpper() == "PRE")
            {
                gridMain7.Columns["CD"].Visible = true;
                gridMain7.Columns["Unity PB"].Visible = false;
                gridMain7.Columns["Unity DC"].Visible = false;
                gridMain7.Columns["Unity Total"].Visible = false;
                gridMain7.Columns["Unity Over/Under"].Visible = false;
                gridMain7.Columns["unityOldBarham"].Visible = true;
                gridMain7.Columns["unityOldWebb"].Visible = true;
                gridMain7.Columns["fdlicOldWebb"].Visible = true;
                gridMain7.Columns["fdlicOldCCI"].Visible = true;
                gridMain7.Columns["bankCD"].Visible = true;
                gridMain7.Columns["FDLIC PB"].Visible = false;

                gridMain7.Columns["unityCash"].Visible = false;
                gridMain7.Columns["unityOverUnder"].Visible = false;
                gridMain7.Columns["unityDeathBenefit"].Visible = false;

                gridMain7.Columns["fdlicCash"].Visible = false;
                gridMain7.Columns["fdlicOverUnder"].Visible = false;
                gridMain7.Columns["fdlicDeathBenefit"].Visible = false;
                gridMain7.Columns["month"].Visible = true;
                gridMain7.Columns["spacer"].Visible = false;

                SetPreReport();

                dx = CalculateTotal(dx, "Security National", "fdlicOldCCI", "total");
            }

            G1.NumberDataTable(dx);
            dgv7.DataSource = dx;
            dgv7.Refresh();

            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void SetPostReport()
        {
            gridMain7.Columns["Security National"].Visible = true;
            gridMain7.Columns["Forethought"].Visible = true;
            gridMain7.Columns["CD"].Visible = true;
            gridMain7.Columns["Unity"].Visible = true;
            gridMain7.Columns["FDLIC"].Visible = true;
            gridMain7.Columns["total"].Visible = true;
            gridMain7.Columns["spacer"].Visible = true;
            gridMain7.Columns["unityCash"].Visible = true;
            gridMain7.Columns["unityOverUnder"].Visible = true;
            gridMain7.Columns["unityDeathBenefit"].Visible = true;
            gridMain7.Columns["spacer2"].Visible = true;
            gridMain7.Columns["fdlicCash"].Visible = true;
            gridMain7.Columns["fdlicOverUnder"].Visible = true;
            gridMain7.Columns["fdlicDeathBenefit"].Visible = true;
            gridMain7.Columns["bankCD"].Visible = true;

            G1.ClearAllPositions(gridMain7);
            G1.SetColumnPosition(gridMain7, "num", 1);
            G1.SetColumnPosition(gridMain7, "month", 2);
            G1.SetColumnPosition(gridMain7, "Security National", 3);
            G1.SetColumnPosition(gridMain7, "Forethought", 4);
            G1.SetColumnPosition(gridMain7, "CD", 5);
            G1.SetColumnPosition(gridMain7, "Unity", 6);
            G1.SetColumnPosition(gridMain7, "FDLIC", 7);
            G1.SetColumnPosition(gridMain7, "total", 8);
            G1.SetColumnPosition(gridMain7, "spacer", 9);
            G1.SetColumnPosition(gridMain7, "unityCash", 10);
            G1.SetColumnPosition(gridMain7, "unityOverUnder", 11);
            G1.SetColumnPosition(gridMain7, "unityDeathBenefit", 12);
            G1.SetColumnPosition(gridMain7, "spacer2", 13);
            G1.SetColumnPosition(gridMain7, "fdlicCash", 14);
            G1.SetColumnPosition(gridMain7, "fdlicOverUnder", 15);
            G1.SetColumnPosition(gridMain7, "fdlicDeathBenefit", 16);
            G1.SetColumnPosition(gridMain7, "bankCD", 17);
        }
        /****************************************************************************************/
        private void SetPreReport()
        {
            G1.ClearAllPositions(gridMain7);
            G1.SetColumnPosition(gridMain7, "num", 1);
            G1.SetColumnPosition(gridMain7, "month", 2);
            G1.SetColumnPosition(gridMain7, "CD", 3);
            G1.SetColumnPosition(gridMain7, "fdlicOldWebb", 4);
            G1.SetColumnPosition(gridMain7, "fdlicOldCCI", 5);
            G1.SetColumnPosition(gridMain7, "Forethought", 6);
            G1.SetColumnPosition(gridMain7, "unityOldBarham", 7);
            G1.SetColumnPosition(gridMain7, "unityOldWebb", 8);
            G1.SetColumnPosition(gridMain7, "Security National", 9);
            G1.SetColumnPosition(gridMain7, "spacer", 10);
            G1.SetColumnPosition(gridMain7, "total", 11);
            G1.SetColumnPosition(gridMain7, "spacer2", 12);
            G1.SetColumnPosition(gridMain7, "bankCD", 13);
        }
        /****************************************************************************************/
        private DataTable CalculateTotal ( DataTable dx, string fromCol, string toCol, string resultCol )
        {
            int fCol = G1.get_column_number(dx, fromCol);
            int tCol = G1.get_column_number(dx, toCol);
            int sCol = G1.get_column_number(dx, resultCol);
            if (fCol < 0 || tCol < 0 || sCol < 0)
                return dx;

            double dValue = 0D;
            double result = 0D;
            try
            {
                for (int i = 0; i < dx.Rows.Count; i++)
                {
                    result = 0D;
                    for (int col = fCol; col <= tCol; col++)
                    {
                        dValue = dx.Rows[i][col].ObjToDouble();
                        result += dValue;
                    }
                    dx.Rows[i][resultCol] = result;
                }
            }
            catch ( Exception )
            {
            }
            return dx;
        }
        /****************************************************************************************/
        private void CleanupHistory ( string preOrPost, BandedGridView gridMain = null )
        {
            if ( preOrPost.ToUpper() == "POST")
            {
                if (gridMain == null)
                {
                    gridMain7.Columns["Security National"].Visible = true;
                    gridMain7.Columns["Forethought"].Visible = true;
                    gridMain7.Columns["Unity"].Visible = true;
                    gridMain7.Columns["FDLIC"].Visible = true;
                    gridMain7.Columns["CD"].Visible = true;
                    gridMain7.Columns["total"].Visible = true;

                    gridMain7.Columns["fdlicOldWebb"].Visible = false;
                    gridMain7.Columns["fdlicOldCCI"].Visible = false;
                    gridMain7.Columns["unityOldBarham"].Visible = false;
                    gridMain7.Columns["unityOldWebb"].Visible = false;
                    gridMain7.Columns["Unity Total"].Visible = false;
                    gridMain7.Columns["Unity PB"].Visible = false;
                    gridMain7.Columns["Unity DC"].Visible = false;
                    gridMain7.Columns["FDLIC PB"].Visible = false;
                    gridMain7.Columns["Unity Over/Under"].Visible = false;
                }
                else 
                {
                    gridMain.Columns["Security National"].Visible = true;
                    gridMain.Columns["Forethought"].Visible = true;
                    gridMain.Columns["Unity"].Visible = true;
                    gridMain.Columns["FDLIC"].Visible = true;
                    gridMain.Columns["CD"].Visible = true;
                    gridMain.Columns["total"].Visible = true;

                    gridMain.Columns["fdlicOldWebb"].Visible = false;
                    gridMain.Columns["fdlicOldCCI"].Visible = false;
                    gridMain.Columns["unityOldBarham"].Visible = false;
                    gridMain.Columns["unityOldWebb"].Visible = false;
                    gridMain.Columns["Unity Total"].Visible = false;
                    gridMain.Columns["Unity PB"].Visible = false;
                    gridMain.Columns["Unity DC"].Visible = false;
                    gridMain.Columns["FDLIC PB"].Visible = false;
                    gridMain.Columns["Unity Over/Under"].Visible = false;
                }
            }
            else
            {
                if (gridMain == null)
                {
                    gridMain7.Columns["Security National"].Visible = true;
                    gridMain7.Columns["Forethought"].Visible = true;
                    gridMain7.Columns["Unity"].Visible = false;
                    gridMain7.Columns["FDLIC"].Visible = true;
                    gridMain7.Columns["CD"].Visible = false;
                    gridMain7.Columns["total"].Visible = true;

                    gridMain7.Columns["fdlicOldWebb"].Visible = true;
                    gridMain7.Columns["fdlicOldCCI"].Visible = true;
                    gridMain7.Columns["unityOldBarham"].Visible = true;
                    gridMain7.Columns["unityOldWebb"].Visible = true;
                    gridMain7.Columns["Unity Total"].Visible = false;
                    gridMain7.Columns["Unity PB"].Visible = false;
                    gridMain7.Columns["Unity DC"].Visible = false;
                    gridMain7.Columns["FDLIC PB"].Visible = false;
                    gridMain7.Columns["Unity Over/Under"].Visible = false;
                }
                else
                {
                    gridMain.Columns["Security National"].Visible = true;
                    gridMain.Columns["Forethought"].Visible = true;
                    gridMain.Columns["Unity"].Visible = false;
                    gridMain.Columns["FDLIC"].Visible = true;
                    gridMain.Columns["CD"].Visible = false;
                    gridMain.Columns["total"].Visible = true;

                    gridMain.Columns["fdlicOldWebb"].Visible = true;
                    gridMain.Columns["fdlicOldCCI"].Visible = true;
                    gridMain.Columns["unityOldBarham"].Visible = true;
                    gridMain.Columns["unityOldWebb"].Visible = true;
                    gridMain.Columns["Unity Total"].Visible = false;
                    gridMain.Columns["Unity PB"].Visible = false;
                    gridMain.Columns["Unity DC"].Visible = false;
                    gridMain.Columns["FDLIC PB"].Visible = false;
                    gridMain.Columns["Unity Over/Under"].Visible = false;
                }
            }
        }
        /****************************************************************************************/
        private DataTable LoadHistory ( DateTime startDate, DateTime stopDate, DataTable dt, DataTable dx, string company )
        {
            DataRow[] dRows = dt.Select("trustName='" + company + "'");
            if (dRows.Length <= 0)
                return dx;
            DataTable dd = dRows.CopyToDataTable();

            DateTime date = DateTime.Now;

            double money = 0D;
            TimeSpan ts;
            int row = 0;
            double beginningPaymentBalance = 0D;
            double beginningDeathBenefit = 0D;
            double endingPaymentBalance = 0D;
            for ( int i=0; i<dd.Rows.Count; i++)
            {
                date = dd.Rows[i]["date"].ObjToDateTime();
                beginningPaymentBalance = dd.Rows[i]["beginningPaymentBalance"].ObjToDouble();
                beginningDeathBenefit = dd.Rows[i]["beginningDeathBenefit"].ObjToDouble();
                endingPaymentBalance = dd.Rows[i]["endingPaymentBalance"].ObjToDouble();

                ts = date - startDate;
                row = G1.GetMonthsBetween(date, startDate );

                if ( company == "SNFT")
                {
                    dx.Rows[row]["Security National"] = beginningPaymentBalance;
                    dx.Rows[row]["Forethought"] = beginningDeathBenefit;
                }
                else
                {
                    dx.Rows[row][company] = beginningPaymentBalance;
                    if (company == "CD")
                    {
                        //dx.Rows[row]["bankCD"] = dd.Rows[i]["payments"].ObjToDouble();
                        dx.Rows[row]["bankCD"] = dd.Rows[i]["beginningDeathBenefit"].ObjToDouble();
                    }
                }

            }
            return dx;
        }
        /****************************************************************************************/
        private void button4_Click(object sender, EventArgs e)
        {
            DateTime now = this.dateTimePicker5.Value;
            now = now.AddMonths(-1);
            now = new DateTime(now.Year, now.Month, 1);
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            this.dateTimePicker5.Value = new DateTime(now.Year, now.Month, days);
            this.dateTimePicker5.Refresh();
        }
        /****************************************************************************************/
        private void button5_Click(object sender, EventArgs e)
        {
            DateTime now = this.dateTimePicker5.Value;
            now = now.AddMonths(1);
            now = new DateTime(now.Year, now.Month, 1);
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            this.dateTimePicker5.Value = new DateTime(now.Year, now.Month, days);
            this.dateTimePicker5.Refresh();
        }
        /****************************************************************************************/
        private void button6_Click(object sender, EventArgs e)
        {
            string preOrPost = cmbPreOrPost8.Text;
            DateTime stopDate = this.dateTimePicker5.Value;

            DateTime startDate = new DateTime(stopDate.Year, stopDate.Month, 1);

            string sDate1 = startDate.ToString("yyyy-MM-dd");
            string sDate2 = stopDate.ToString("yyyy-MM-dd");

            string cmd = "Select * from `trust_data` WHERE `date` >= '" + sDate1 + "' AND `date` <= '" + sDate2 + "' AND `preOrPost` = '" + preOrPost + "' ";
            cmd += " ORDER BY `contractNumber`;";
            string contractNumber = txtContract.Text.Trim();
            if (!String.IsNullOrWhiteSpace(contractNumber))
            {
                cmd = "Select * from `trust_data` WHERE `contractNumber` = '" + contractNumber + "' ORDER BY `date` desc;";
            }
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
            {
                this.Close();
                return;
            }
            if (!String.IsNullOrWhiteSpace(contractNumber))
            {
                startDate = dt.Rows[0]["date"].ObjToDateTime();
                sDate1 = startDate.ToString("yyyy-MM-01");
                int dayss = DateTime.DaysInMonth(startDate.Year, startDate.Month);
                stopDate = new DateTime(startDate.Year, startDate.Month, dayss);
                sDate2 = stopDate.ToString("yyyy-MM-dd");
                cmd = "Select * from `trust_data` WHERE `date` >= '" + sDate1 + "' AND `date` <= '" + sDate2 + "' AND `contractNumber` = '" + contractNumber + "';";
                dt = G1.get_db_data(cmd);
                if (dt.Rows.Count <= 0)
                {
                    this.Close();
                    return;
                }
                btnAccept.Show();
                btnAccept.Refresh();
            }

            this.Cursor = Cursors.WaitCursor;

            dt = verifyContracts(dt, true );

            DateTime lastMonth = stopDate.AddMonths(-1);
            int days = DateTime.DaysInMonth(lastMonth.Year, lastMonth.Month);
            lastMonth = new DateTime(lastMonth.Year, lastMonth.Month, days);
            cmd = "Select * from `trust2013r` r JOIN `customers` c ON r.`contractNumber` = c.`contractNumber` WHERE `payDate8` = '" + lastMonth.ToString("yyyyMMdd") + "' ORDER BY r.`contractNumber`;";
            //cmd = "Select * from `trust2013r` WHERE `payDate8` = '" + sDate2 + "' ORDER BY `contractNumber`;";
            DataTable rDt = G1.get_db_data(cmd);

            DataRow[] dRows = null;

            DataTable dx = new DataTable();
            dx.Columns.Add("contractNumber");
            dx.Columns.Add("month");
            dx.Columns.Add("deceasedDate");
            dx.Columns.Add("serviceId");
            dx.Columns.Add("Security National", Type.GetType("System.Double"));
            dx.Columns.Add("Forethought", Type.GetType("System.Double"));
            dx.Columns.Add("CD", Type.GetType("System.Double"));
            dx.Columns.Add("Unity", Type.GetType("System.Double"));
            dx.Columns.Add("FDLIC", Type.GetType("System.Double"));
            dx.Columns.Add("total", Type.GetType("System.Double"));
            dx.Columns.Add("endingBalance", Type.GetType("System.Double"));

            dx.Columns.Add("Unity PB", Type.GetType("System.Double"));
            dx.Columns.Add("Unity DC", Type.GetType("System.Double"));
            dx.Columns.Add("Unity Total", Type.GetType("System.Double"));
            dx.Columns.Add("Unity Over/Under", Type.GetType("System.Double"));
            dx.Columns.Add("unityOldBarham", Type.GetType("System.Double"));
            dx.Columns.Add("unityOldWebb", Type.GetType("System.Double"));
            dx.Columns.Add("fdlicOldWebb", Type.GetType("System.Double"));
            dx.Columns.Add("fdlicOldCCI", Type.GetType("System.Double"));
            dx.Columns.Add("FDLIC PB", Type.GetType("System.Double"));

            dx.Columns.Add("unityCash", Type.GetType("System.Double"));
            dx.Columns.Add("unityOverUnder", Type.GetType("System.Double"));
            dx.Columns.Add("unityDeathBenefit", Type.GetType("System.Double"));

            dx.Columns.Add("fdlicCash", Type.GetType("System.Double"));
            dx.Columns.Add("fdlicOverUnder", Type.GetType("System.Double"));
            dx.Columns.Add("fdlicDeathBenefit", Type.GetType("System.Double"));

            dx.Columns.Add("year");
            dx.Columns.Add("date");

            contractNumber = "";
            string oldContract = "";
            string trustCompany = "";
            double beginningPaymentBalance = 0D;
            double endingPaymentBalance = 0D;
            double beginningDeathBenefit = 0D;
            double endingDeathBenefit = 0D;

            double total_beginningPaymentBalance = 0D;
            double total_endingPaymentBalance = 0D;
            double total_beginningDeathBenefit = 0D;
            double total_endingDeathBenefit = 0D;

            double dValue = 0D;

            DataRow dRow = null;

            DateTime deceasedDate = DateTime.Now;
            DateTime oldDeceasedDate = DateTime.Now;
            string serviceId = "";
            string oldServiceId = "";

            for ( int i=0; i<dt.Rows.Count; i++)
            {
                try
                {
                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                    if (String.IsNullOrWhiteSpace(contractNumber))
                        continue;
                    if (contractNumber == "B04002")
                    {
                    }
                    if (String.IsNullOrWhiteSpace(oldContract))
                    {
                        oldContract = contractNumber;
                    }
                    if (dRow == null)
                        dRow = dx.NewRow();
                    if (oldContract != contractNumber)
                    {
                        dRow["contractNumber"] = oldContract;
                        dx.Rows.Add(dRow);
                        dRow = dx.NewRow();

                        total_beginningPaymentBalance = 0D;
                        total_endingPaymentBalance = 0D;
                        total_beginningDeathBenefit = 0D;
                        total_endingDeathBenefit = 0D;

                        oldContract = contractNumber;
                        oldDeceasedDate = deceasedDate;
                        oldServiceId = serviceId;
                    }
                    trustCompany = dt.Rows[i]["trustCompany"].ObjToString();

                    beginningPaymentBalance = dt.Rows[i]["beginningPaymentBalance"].ObjToDouble();
                    endingPaymentBalance = dt.Rows[i]["endingPaymentBalance"].ObjToDouble();
                    beginningDeathBenefit = dt.Rows[i]["beginningDeathBenefit"].ObjToDouble();
                    endingDeathBenefit = dt.Rows[i]["endingDeathBenefit"].ObjToDouble();
                    if (endingDeathBenefit == 0D)
                        continue;
                    if (endingPaymentBalance == 0D)
                        continue;

                    if (trustCompany == "Security National")
                    {
                        dValue = dRow["Security National"].ObjToDouble();
                        dValue += beginningPaymentBalance;
                        dRow["Security National"] = dValue;
                    }
                    else if (trustCompany == "Forethought")
                    {
                        dValue = dRow["Forethought"].ObjToDouble();
                        dValue += endingPaymentBalance;
                        dRow["Forethought"] = dValue;
                    }
                    else if (trustCompany == "Unity" || trustCompany == "Unity PB")
                    {
                        dValue = dRow["Unity"].ObjToDouble();
                        dValue += endingPaymentBalance;
                        dRow["Unity"] = dValue;
                    }
                    else if (trustCompany == "FDLIC" || trustCompany == "FDLIC PB")
                    {
                        dValue = dRow["FDLIC"].ObjToDouble();
                        dValue += endingPaymentBalance;
                        dRow["FDLIC"] = dValue;
                    }
                    if (i == dt.Rows.Count - 1)
                    {
                        dRow["contractNumber"] = oldContract;
                        dx.Rows.Add(dRow);
                    }
                }
                catch ( Exception ex)
                {
                }
            }

            for ( int i=0; i<dx.Rows.Count; i++)
            {
                contractNumber = dx.Rows[i]["contractNumber"].ObjToString();
                if (String.IsNullOrWhiteSpace(contractNumber))
                    continue;
                //if (contractNumber == "?")
                //    continue;
                dRows = rDt.Select("contractNumber='" + contractNumber + "'");
                if ( dRows.Length > 0 )
                {
                    dValue = dRows[0]["endingBalance"].ObjToDouble();
                    //if (dValue == 0D)
                    //    dValue = dRows[0]["currentRemovals"].ObjToDouble();
                    //if (dValue == 0D)
                    //    dValue = dRows[0]["beginningBalance"].ObjToDouble();
                    dx.Rows[i]["endingBalance"] = dValue;

                    dx.Rows[i]["deceasedDate"] = dRows[0]["deceasedDate"].ObjToDateTime().ToString("MM/dd/yyyy");
                    dx.Rows[i]["serviceId"] = dRows[0]["serviceId1"].ObjToString();
                }
            }

            dx = CalculateTotal(dx, "Security National", "FDLIC", "total");

            CleanupHistory(preOrPost, gridMain8 );

            if (preOrPost.ToUpper() == "POST")
            {
                gridMain8.Columns["CD"].Visible = false;
                gridMain8.Columns["Unity PB"].Visible = false;
                gridMain8.Columns["Unity DC"].Visible = false;
                gridMain8.Columns["Unity Total"].Visible = false;
                gridMain8.Columns["Unity Over/Under"].Visible = false;
                gridMain8.Columns["unityOldBarham"].Visible = false;
                gridMain8.Columns["unityOldWebb"].Visible = false;
                gridMain8.Columns["fdlicOldWebb"].Visible = false;
                gridMain8.Columns["fdlicOldCCI"].Visible = false;
                gridMain8.Columns["FDLIC PB"].Visible = false;

                gridMain8.Columns["unityCash"].Visible = false;
                gridMain8.Columns["unityOverUnder"].Visible = false;
                gridMain8.Columns["unityDeathBenefit"].Visible = false;

                gridMain8.Columns["fdlicCash"].Visible = false;
                gridMain8.Columns["fdlicOverUnder"].Visible = false;
                gridMain8.Columns["fdlicDeathBenefit"].Visible = false;
                gridMain8.Columns["month"].Visible = false;
                gridMain8.Columns["spacer"].Visible = false;
                //gridMain8.Columns["spacer2"].Visible = false;
            }

            G1.NumberDataTable(dx);

            dgv8.DataSource = dx;
            dgv8.Refresh();

            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void pictureBox3_Click(object sender, EventArgs e)
        {
            G1.SpyGlass(gridMain8);
        }
        /****************************************************************************************/
        private void gridMain8_DoubleClick(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv8.DataSource;
            if (dt == null)
                return;
            if (dt.Rows.Count <= 0)
                return;

            DataRow dr = gridMain8.GetFocusedDataRow();

            string contractNumber = dr["contractNumber"].ObjToString();

            this.Cursor = Cursors.WaitCursor;

            CustomerDetails clientForm = new CustomerDetails(contractNumber);
            clientForm.Show();

            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void chkFilterEmpty_CheckedChanged(object sender, EventArgs e)
        {
            gridMain8.RefreshEditor(true);
            gridMain8.RefreshData();
            //dgv8.Refresh();
        }
        /****************************************************************************************/
        private void gridMain8_CustomRowFilter(object sender, RowFilterEventArgs e)
        {
            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv8.DataSource;
            if (chkFilterEmpty.Checked)
            {
                double value = dt.Rows[row]["total"].ObjToDouble();
                double received = dt.Rows[row]["endingBalance"].ObjToDouble();
                if (value == 0D && received == 0D)
                {
                    e.Visible = false;
                    e.Handled = true;
                    return;
                }
            }
        }
        /****************************************************************************************/
        private void goToTrustDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv8.DataSource;
            if (dt == null)
                return;
            if (dt.Rows.Count <= 0)
                return;

            DataRow dr = gridMain8.GetFocusedDataRow();

            string contractNumber = dr["contractNumber"].ObjToString();

            string cmd = "Select * from `trust_data` WHERE `contractNumber` = '" + contractNumber + "';";
            DataTable dx = G1.get_db_data(cmd);

            this.Cursor = Cursors.WaitCursor;
            TrustData detail = new TrustData ( dx );
            detail.Show();

            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void showTBBToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv8.DataSource;
            if (dt == null)
                return;
            if (dt.Rows.Count <= 0)
                return;

            DataRow dr = gridMain8.GetFocusedDataRow();
            string contract = dr["contractNumber"].ObjToString();
            if (String.IsNullOrWhiteSpace(contract))
                return;

            this.Cursor = Cursors.WaitCursor;
            PayOffDetail detail = new PayOffDetail(contract);
            detail.Show();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void gridMain7_CellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            if (e == null)
                return;
            int rowHandle = gridMain7.FocusedRowHandle;
            int row = gridMain7.GetDataSourceRowIndex(rowHandle);
            DataRow dr = gridMain7.GetFocusedDataRow();
            if (dr == null)
                return;

            DataTable dt = (DataTable)dgv7.DataSource;
            string record = dr["record"].ObjToString();
            if (record == "0")
                record = "";

            string column = gridMain7.FocusedColumn.FieldName;
            if (column.ToUpper() != "BANKCD")
                return;

            string preOrPost = cmbPreOrPost2.Text.ObjToString();

            if (preOrPost.ToUpper() == "PRE")
            {
                if (String.IsNullOrWhiteSpace(record))
                    return;
                double bankCD = dr["bankCD"].ObjToDouble();
                G1.update_db_table("trust_data_edits", "record", record, new string[] { "payments", bankCD.ToString() });
            }
            else
            {
                double bankCD = dr["bankCD"].ObjToDouble();
                DateTime date = dr["date"].ObjToDateTime();
                string cmd = "Select * from `trust_data_edits` where `date` = '" + date.ToString("yyyy-MM-dd") + "' AND `status` = 'EndingBalance' AND `trustName` = 'CD' AND `preOrPost` = 'Post';";
                DataTable dx = G1.get_db_data(cmd);
                if ( dx.Rows.Count > 0 )
                {
                    record = dx.Rows[0]["record"].ObjToString();
                    G1.update_db_table("trust_data_edits", "record", record, new string[] { "payments", bankCD.ToString() });
                }
            }
        }
        /****************************************************************************************/
        private void pictureBox4_Click(object sender, EventArgs e)
        {
            G1.SpyGlass(gridMain9);
        }
        /****************************************************************************************/
        private void button9_Click(object sender, EventArgs e)
        {
            DateTime date = this.dateTimePicker9.Value;
            date = date.AddDays(-1);
            this.dateTimePicker9.Value = date;
            this.dateTimePicker9.Refresh();
        }
        /****************************************************************************************/
        private void button8_Click(object sender, EventArgs e)
        {
            DateTime date = this.dateTimePicker9.Value;
            date = date.AddDays(1);
            this.dateTimePicker9.Value = date;
            this.dateTimePicker9.Refresh();
        }
        /****************************************************************************************/
        private string saveTitle = "";
        private void button7_Click(object sender, EventArgs e)
        {
            DateTime date = this.dateTimePicker9.Value;
            string paymentRecord = "";
            string fName = "";
            string mName = "";
            string lName = "";
            string prefix = "";
            string suffix = "";
            string name = "";

            string cmd = "SELECT * FROM `cust_payment_details` p JOIN `cust_payments` c ON p.`paymentRecord` = c.`record` LEFT JOIN `fcust_extended` f ON p.`contractNumber` = f.`contractNumber` WHERE p.`dateReceived` = '" + date.ToString("yyyy-MM-dd") + "' AND p.`type` = 'TRUST';";

            cmd = "SELECT * FROM `cust_payment_details` p JOIN `cust_payments` c ON p.`paymentRecord` = c.`record` LEFT JOIN `fcust_extended` f ON p.`contractNumber` = f.`contractNumber` LEFT JOIN `fcustomers` x ON p.`contractNumber` = x.`contractNumber` WHERE p.`dateReceived` = '" + date.ToString("yyyy-MM-dd") + "' AND p.`type` = 'TRUST';";


            DataTable dt = G1.get_db_data(cmd);

            dt.Columns.Add("name");

            for ( int i=0; i<dt.Rows.Count; i++)
            {
                fName = dt.Rows[i]["firstName"].ObjToString();
                mName = dt.Rows[i]["middleName"].ObjToString();
                lName = dt.Rows[i]["lastName"].ObjToString();
                prefix = dt.Rows[i]["prefix"].ObjToString();
                suffix = dt.Rows[i]["suffix"].ObjToString();

                name = G1.BuildFullName(prefix, fName, mName, lName, suffix);
                dt.Rows[i]["name"] = name;
            }

            G1.NumberDataTable(dt);
            dgv9.DataSource = dt;
        }
        /****************************************************************************************/
        private void chkGroupPaidFrom_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox box = (CheckBox)sender;

            if ( box.Checked )
            {
                gridMain9.Columns["paidFrom"].GroupIndex = 0;
                gridMain9.ExpandAllGroups();
            }
            else
            {
                gridMain9.Columns["paidFrom"].GroupIndex = -1;
            }

            gridMain9.RefreshEditor(true);
            dgv9.Refresh();
        }
        /****************************************************************************************/
        private void dgv2_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            //if (e.KeyCode == Keys.A && System.Windows.Input.Keyboard.Modifiers.HasFlag ( System.Windows.Input.ModifierKeys.Control ))
            //{
            //    //e.Handled = true;
            //    gridMain2.Columns["num"].Visible = false;
            //    gridMain2.SelectAll();
            //    gridMain2.CopyToClipboard();
            //    gridMain2.Columns["num"].Visible = true;
            //}
        }
        /****************************************************************************************/
        private void SnapShot ( DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gridMain, string font, float size )
        {
            gridMain.Columns["num"].Visible = false;

            gridMain.Appearance.Row.Font = new Font(font, size, FontStyle.Bold);
            gridMain.Appearance.HeaderPanel.Font = new Font(font, size, FontStyle.Bold);

            gridMain.SelectAll();
            gridMain.CopyToClipboard();

            gridMain.ClearSelection();
            gridMain.Columns["num"].Visible = true;

            gridMain.Appearance.Row.Font = new Font("Tahoma", 7.8F, FontStyle.Regular);
            gridMain.Appearance.HeaderPanel.Font = new Font("Tahoma", 7.8F, FontStyle.Regular);

            MessageBox.Show("*** INFO *** All Data Copied to Clipboard!", "Copy All Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);

        }
        /****************************************************************************************/
        private void btnCopy_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv2.DataSource;
            if (workReport == "Pre 2002 Report")
            {
                if (dgv6.Visible)
                    SnapShot(gridMain6, "Calibri", 13.8F); // FT - SN
                else if (dgv10.Visible)
                    SnapShot(gridMain10, "Calibri", 13.8F); // CD
                else if (dgv11.Visible)
                    SnapShot(gridMain11, "Calibri", 13.8F); // FDLIC Old Webb
                else if (dgv12.Visible)
                    SnapShot(gridMain12, "Calibri", 13.8F); // FDLIC Old CCI
                else if (dgv13.Visible)
                    SnapShot(gridMain13, "Calibri", 13.8F); // Unity Old Barham
                else if (dgv14.Visible)
                    SnapShot(gridMain14, "Calibri", 13.8F); // Unity Old Webb
                return;
            }

            gridMain2.Columns["num"].Visible = false;
            if (workReport == "Post 2002 Report - CD")
            {
                gridMain2.Appearance.Row.Font = new Font("Calibri", 12.0F, FontStyle.Bold);
                gridMain2.Appearance.HeaderPanel.Font = new Font("Calibri", 12.0F, FontStyle.Bold);
            }
            else
            {
                gridMain2.Appearance.Row.Font = new Font("Calibri", 13.8F, FontStyle.Bold);
                gridMain2.Appearance.HeaderPanel.Font = new Font("Calibri", 13.8F, FontStyle.Bold);
            }

            if (workReport == "Post 2002 Report - FDLIC")
            {
                gridMain2.Columns["junk1"].Visible = true;
                gridMain2.Columns["junk2"].Visible = true;
                gridMain2.Columns["junk3"].Visible = true;
            }
            else if (workReport == "Post 2002 Report - SN & FT" )
            {
                gridMain2.Columns["sn1"].Visible = true;
                gridMain2.Columns["sn2"].Visible = true;
            }
            gridMain2.SelectAll();
            gridMain2.CopyToClipboard();

            if (workReport == "Post 2002 Report - FDLIC")
            {
                gridMain2.Columns["junk1"].Visible = false;
                gridMain2.Columns["junk2"].Visible = false;
                gridMain2.Columns["junk3"].Visible = false;
                gridMain2.RefreshData();
                gridMain2.RefreshEditor(true);
                dgv2.Refresh();
            }
            else if (workReport == "Post 2002 Report - SN & FT")
            {
                gridMain2.Columns["sn1"].Visible = false;
                gridMain2.Columns["sn2"].Visible = false;
            }

            gridMain2.ClearSelection();
            gridMain2.Columns["num"].Visible = true;

            gridMain2.Appearance.Row.Font = new Font("Tahoma", 7.8F, FontStyle.Regular);
            gridMain2.Appearance.HeaderPanel.Font = new Font("Tahoma", 7.8F, FontStyle.Regular );

            MessageBox.Show("*** INFO *** All Data Copied to Clipboard!", "Copy All Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
        }
        /****************************************************************************************/
        private void button10_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv7.DataSource;

            gridMain7.Columns["num"].Visible = false;
            gridMain7.Appearance.Row.Font = new Font("Calibri", 13.8F, FontStyle.Bold);
            gridMain7.Appearance.HeaderPanel.Font = new Font("Calibri", 13.8F, FontStyle.Bold);

            gridMain7.SelectAll();
            gridMain7.CopyToClipboard();

            gridMain7.ClearSelection();
            gridMain7.Columns["num"].Visible = true;

            gridMain7.Appearance.Row.Font = new Font("Tahoma", 7.8F, FontStyle.Regular);
            gridMain7.Appearance.HeaderPanel.Font = new Font("Tahoma", 7.8F, FontStyle.Regular);

            MessageBox.Show("*** INFO *** All Data Copied to Clipboard!", "Copy All Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
        }
        /****************************************************************************************/
        private void LoadPre2002_CD ( DataTable dx )
        {
            int mainWidth = 85;

            //DataTable dx = (DataTable)dgv6.DataSource;
            string trustCompany = "CD";
            string newCol = "";

            int i = 1;

            ClearAllPositions(gridMain10);

            G1.SetColumnPosition(gridMain10, "num", i++);
            G1.SetColumnPosition(gridMain10, "month", i++);
            G1.SetColumnPosition(gridMain10, "junk1", i++);


            if (G1.get_column_number(dx, trustCompany) >= 0)
            {
                G1.SetColumnPosition(gridMain10, trustCompany, i++);

                G1.SetColumnPosition(gridMain10, "desc", i++);
                G1.SetColumnPosition(gridMain10, "date", i++);
                G1.SetColumnPosition(gridMain10, "contract", i++);
                G1.SetColumnPosition(gridMain10, "funeral", i++);
                G1.SetColumnPosition(gridMain10, "junk2", i++);
                G1.SetColumnPosition(gridMain10, "junk3", i++);
                G1.SetColumnPosition(gridMain10, "otherDesc", i++);
                G1.SetColumnPosition(gridMain10, "bankCD", i++);
            }

            DataTable dt = new DataTable();
            dt.Columns.Add("record");
            dt.Columns.Add("month");
            dt.Columns.Add("CD", Type.GetType("System.Double"));
            dt.Columns.Add("desc");
            dt.Columns.Add("date");
            dt.Columns.Add("contract");
            dt.Columns.Add("funeral");
            dt.Columns.Add("bankCD", Type.GetType("System.Double"));
            dt.Columns.Add("trust");
            dt.Columns.Add("otherDesc");
            dt.Columns.Add("status");
            dt.Columns.Add("manual");

            double dValue = 0D;
            string desc = "";
            double bankCD = 0D;
            string otherDesc = "";
            string month = "";
            string status = "";
            string record = "";

            DataRow dRow = null;
            int year = this.dateTimePicker2.Value.Year;

            for (i = 0; i < dx.Rows.Count; i++)
            {
                month = dx.Rows[i]["month"].ObjToString();
                if (String.IsNullOrWhiteSpace(month))
                    continue;

                record = dx.Rows[i]["record"].ObjToString();
                status = dx.Rows[i]["status"].ObjToString();
                dValue = dx.Rows[i]["CD"].ObjToDouble();
                bankCD = dx.Rows[i]["bankCd"].ObjToDouble();
                otherDesc = dx.Rows[i]["middleName"].ObjToString();

                if (dValue > 0D)
                {
                    if (dt.Rows.Count <= 0)
                    {
                        dRow = dt.NewRow();
                        dRow["month"] = month + " " + year.ToString("D4");
                        dt.Rows.Add(dRow);
                    }

                    dRow = dt.NewRow();
                    if (status.ToUpper().IndexOf("BEGINNING") >= 0)
                        dRow["month"] = status;
                    else if (status.ToUpper().IndexOf("ENDING") >= 0)
                        dRow["month"] = status;
                    else
                        dRow["month"] = "";
                    dRow["record"] = record;

                    dRow["CD"] = dValue;
                    dRow["bankCD"] = bankCD;
                    dRow["otherDesc"] = otherDesc;
                    dt.Rows.Add(dRow);
                    if (status.ToUpper().IndexOf("BEGINNING") >= 0)
                    {
                        dRow = dt.NewRow();
                        dt.Rows.Add(dRow);
                    }
                }
            }

            LoadLineEditsPreCD(dt, this.dateTimePicker2.Value);

            double totalDValue = 0D;
            double totalbankCD = 0D;
            for (i = 0; i < dt.Rows.Count; i++)
            {
                month = dt.Rows[i]["month"].ObjToString();
                if (month.ToUpper().IndexOf("BEGINNING") >= 0)
                {
                    totalDValue = dt.Rows[i]["CD"].ObjToDouble();
                    totalbankCD = dt.Rows[i]["bankCD"].ObjToDouble();
                    continue;
                }
                if (month.ToUpper().IndexOf("ENDING") >= 0)
                {
                    dt.Rows[i]["CD"] = totalDValue;
                    dt.Rows[i]["bankCD"] = totalbankCD;
                    break;
                }
                dValue = dt.Rows[i]["CD"].ObjToDouble();
                bankCD = dt.Rows[i]["bankCD"].ObjToDouble();

                totalDValue += dValue;
                totalbankCD += bankCD;
            }

            G1.NumberDataTable(dt);

            month = this.dateTimePicker2.Value.ToString("MMMM");
            if (dt.Rows.Count >= 2)
            {
                dt.Rows[0]["month"] = month + " " + this.dateTimePicker2.Value.Year.ToString("D4");
                dt.Rows[1]["month"] = "Beginning";
            }

            gridMain10.OptionsClipboard.AllowCopy = DevExpress.Utils.DefaultBoolean.True;
            gridMain10.OptionsClipboard.ClipboardMode = DevExpress.Export.ClipboardMode.Formatted;

            dt.TableName = "Post 2002 Report - CD";
            dgv10.DataSource = dt;

            dgv6.Hide();

            dgv10.Show();
            dgv10.Refresh();
        }
        /****************************************************************************************/
        private void LoadPre2002_OldData (DevExpress.XtraGrid.GridControl dgv, DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gridMain, DataTable dx, string trustCompany, string columnName )
        {
            int mainWidth = 85; // ramma zamma

            string newCol = "";

            int i = 1;

            ClearAllPositions(gridMain);

            G1.SetColumnPosition(gridMain, "num", i++);
            G1.SetColumnPosition(gridMain, "month", i++);
            G1.SetColumnPosition(gridMain, "junk1", i++);


            if (G1.get_column_number(dx, trustCompany) >= 0)
            {
                gridMain.Columns[columnName].Visible = true;
                G1.SetColumnPosition(gridMain, columnName, i++);

                G1.SetColumnPosition(gridMain, "desc", i++);
                G1.SetColumnPosition(gridMain, "date", i++);
                G1.SetColumnPosition(gridMain, "contract", i++);
                G1.SetColumnPosition(gridMain, "funeral", i++);
                //G1.SetColumnPosition(gridMain, "junk2", i++);
                //G1.SetColumnPosition(gridMain, "junk3", i++);
                //G1.SetColumnPosition(gridMain, "otherDesc", i++);
                G1.SetColumnPosition(gridMain, "bankCD", i++);
            }

            DataTable dt = new DataTable();
            dt.Columns.Add("record");
            dt.Columns.Add("month");
            dt.Columns.Add(columnName, Type.GetType("System.Double"));
            dt.Columns.Add("desc");
            dt.Columns.Add("date");
            dt.Columns.Add("contract");
            dt.Columns.Add("funeral");
            dt.Columns.Add("bankCD", Type.GetType("System.Double"));
            dt.Columns.Add("trust");
            dt.Columns.Add("otherDesc");
            dt.Columns.Add("status");
            dt.Columns.Add("manual");

            double dValue = 0D;
            string desc = "";
            double bankCD = 0D;
            string otherDesc = "";
            string date = "";
            string contract = "";
            string funeral = "";
            string month = "";
            string status = "";
            string record = "";
            bool firstDCPaid = true;
            bool firstData = true;

            DataRow dRow = null;
            int year = this.dateTimePicker2.Value.Year;

            for (i = 0; i < dx.Rows.Count; i++)
            {
                try
                {
                    month = dx.Rows[i]["month"].ObjToString();
                    //if (String.IsNullOrWhiteSpace(month))
                    //    continue;

                    record = dx.Rows[i]["record"].ObjToString();
                    status = dx.Rows[i]["status"].ObjToString();
                    dValue = dx.Rows[i][trustCompany].ObjToDouble();
                    bankCD = dx.Rows[i]["bankCd"].ObjToDouble();
                    bankCD = dValue;

                    desc = "";
                    date = "";
                    contract = "";
                    funeral = "";

                    if ( G1.get_column_number ( dx, trustCompany + " desc" ) >= 0 )
                        desc = dx.Rows[i][trustCompany + " desc"].ObjToString();
                    if (G1.get_column_number(dx, trustCompany + " date") >= 0 )
                        date = dx.Rows[i][trustCompany + " date"].ObjToString();
                    if (G1.get_column_number(dx, trustCompany + " contract") >= 0 )
                        contract = dx.Rows[i][trustCompany + " contract"].ObjToString();
                    if (G1.get_column_number(dx, trustCompany + " funeral") >= 0 )
                        funeral = dx.Rows[i][trustCompany + " funeral"].ObjToString();

                    if (desc.ToUpper().IndexOf("DC PAID") >= 0)
                    {
                        if (firstDCPaid)
                        {
                            firstDCPaid = false;
                            dRow = dt.NewRow();
                            dt.Rows.Add(dRow);
                        }
                    }

                    if (dValue > 0D || !String.IsNullOrWhiteSpace(desc))
                    {
                        if (dt.Rows.Count <= 0)
                        {
                            dRow = dt.NewRow();
                            dRow["month"] = month + " " + year.ToString("D4");
                            dt.Rows.Add(dRow);
                        }

                        if (status.ToUpper().IndexOf("ENDING") >= 0)
                        {
                            dRow = dt.NewRow();
                            dt.Rows.Add(dRow);
                        }
                        dRow = dt.NewRow();
                        if (status.ToUpper().IndexOf("BEGINNING") >= 0)
                            dRow["month"] = status;
                        else if (status.ToUpper().IndexOf("ENDING") >= 0)
                            dRow["month"] = status;
                        else
                            dRow["month"] = "";
                        dRow["record"] = record;

                        dRow[columnName] = dValue;
                        dRow["bankCD"] = bankCD;
                        dRow["desc"] = desc;
                        dRow["date"] = date;
                        dRow["contract"] = contract;
                        dRow["funeral"] = funeral;
                        dt.Rows.Add(dRow);
                        if (status.ToUpper().IndexOf("BEGINNING") >= 0)
                        {
                            dRow = dt.NewRow();
                            dt.Rows.Add(dRow);
                        }

                        if (!firstDCPaid && firstData)
                        {
                            firstData = false;
                            dRow = dt.NewRow();
                            dt.Rows.Add(dRow);
                        }

                    }
                }
                catch ( Exception ex )
                {
                }
            }

            LoadLineEditsPre2002(dt, this.dateTimePicker2.Value, columnName );

            double totalDValue = 0D;
            double totalbankCD = 0D;
            for (i = 0; i < dt.Rows.Count; i++)
            {
                month = dt.Rows[i]["month"].ObjToString();
                if (month.ToUpper().IndexOf("BEGINNING") >= 0)
                {
                    totalDValue = dt.Rows[i][columnName].ObjToDouble();
                    totalbankCD = dt.Rows[i]["bankCD"].ObjToDouble();
                    totalbankCD = totalDValue;
                    continue;
                }
                if (month.ToUpper().IndexOf("ENDING") >= 0)
                {
                    dt.Rows[i][columnName] = totalDValue;
                    dt.Rows[i]["bankCD"] = totalbankCD;
                    break;
                }
                dValue = dt.Rows[i][columnName].ObjToDouble();
                bankCD = dt.Rows[i]["bankCD"].ObjToDouble();
                bankCD = dValue;

                totalDValue += dValue;
                totalbankCD += bankCD;
            }

            gridMain.Columns["bankCD"].Caption = "Totals";
            G1.NumberDataTable(dt);

            month = this.dateTimePicker2.Value.ToString("MMMM");
            if (dt.Rows.Count >= 2)
            {
                dt.Rows[0]["month"] = month + " " + this.dateTimePicker2.Value.Year.ToString("D4");
                dt.Rows[1]["month"] = "Beginning";
            }

            gridMain.OptionsClipboard.AllowCopy = DevExpress.Utils.DefaultBoolean.True;
            gridMain.OptionsClipboard.ClipboardMode = DevExpress.Export.ClipboardMode.Formatted;

            dt.TableName = "Pre 2002 Report - " + trustCompany;
            dgv.DataSource = dt;

            //dgv6.Hide();

            dgv.Show();
            dgv.Refresh();
        }
        /****************************************************************************************/
        private void Pre2002_CellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            if (e == null)
                return;

            DevExpress.XtraGrid.GridControl dgv = null;
            string column = "";

            if (dgv11.Visible)
            {
                dgv = dgv11;
                column = "fdlicOldWebb";
            }
            else if (dgv12.Visible)
            {
                dgv = dgv12;
                column = "fdlicOldCCI";
            }
            else if (dgv13.Visible)
            {
                dgv = dgv13;
                column = "unityOldBarham";
            }
            else if (dgv14.Visible)
            {
                dgv = dgv14;
                column = "unityOldWebb";
            }
            if (dgv == null)
                return;

            DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gridMain = (AdvBandedGridView)dgv.MainView;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            DataRow dr = gridMain.GetFocusedDataRow();
            if (dr == null)
                return;

            DataTable dt = (DataTable)dgv.DataSource;
            string record = dr["record"].ObjToString();
            if (record == "0")
                record = "";

            DateTime date = this.dateTimePicker2.Value;
            string detail = dt.TableName;
            detail = detail.Replace("Pre 2002 Report - ", "").Trim();

            string trustCompany = "Pre2002";
            //double CD = dr["CD"].ObjToDouble();
            double dValue = dr[column].ObjToDouble();
            double bankCD = dr["bankCD"].ObjToDouble();

            string month = dr["month"].ObjToString().ToUpper();
            //month = dr["status"].ObjToString().ToUpper();
            string desc = dr["desc"].ObjToString();
            string otherDesc = dr["otherDesc"].ObjToString();

            if (month != "BEGINNING BALANCE")
            {
                if (String.IsNullOrWhiteSpace(record))
                {
                    record = G1.create_record("trust_data_edits", "status", "-1");
                    dr["record"] = record.ObjToInt32();
                    G1.update_db_table("trust_data_edits", "record", record, new string[] { "status", "Line Edit", "preOrPost", "Pre" });
                    dr["record"] = record.ObjToInt32();
                }
                if (G1.BadRecord("trust_data_edits", record))
                    return;

                G1.update_db_table("trust_data_edits", "record", record, new string[] { "downPayments", dValue.ToString(), "growth", bankCD.ToString(), "date", date.ToString("yyyy-MM-dd"), "trustName", trustCompany, "position", row.ToString(), "insuredName", desc, "middleName", otherDesc, "billingReason", column });

                dt = LoadLineEditsPre2002(dt, this.dateTimePicker2.Value, detail );

                double totalDValue = 0D;
                double totalbankCD = 0D;
                dValue = 0D;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    month = dt.Rows[i]["month"].ObjToString();
                    if (month.ToUpper().IndexOf("BEGINNING") >= 0)
                    {
                        totalDValue = dt.Rows[i][column].ObjToDouble();
                        //totalbankCD = dt.Rows[i]["bankCD"].ObjToDouble();
                        continue;
                    }
                    if (month.ToUpper().IndexOf("ENDING") >= 0)
                    {
                        dt.Rows[i][column] = totalDValue;
                        dt.Rows[i]["bankCD"] = totalbankCD;
                        dt.Rows[i]["bankCD"] = totalDValue;
                        break;
                    }
                    dValue = dt.Rows[i][column].ObjToDouble();
                    bankCD = dt.Rows[i]["bankCD"].ObjToDouble();

                    totalDValue += dValue;
                    totalbankCD += bankCD;
                }


                gridMain.Columns["bankCD"].Caption = "Totals";
                gridMain.RefreshEditor(true);
                gridMain.RefreshData();
                dgv.Refresh();
                gridMain.PostEditor();
                return;
            }
            else
            {
                string mainUpdate = "BeginningBalance";
                if (month == "BEGINNING BALANCE")
                {
                    mainUpdate = "BeginningBalance";
                    if (String.IsNullOrWhiteSpace(record))
                    {
                        record = G1.create_record("trust_data_edits", "status", "-1");
                        dr["record"] = record.ObjToInt32();
                        G1.update_db_table("trust_data_edits", "record", record, new string[] { "status", mainUpdate, "preOrPost", "Pre" });
                    }
                    if (G1.BadRecord("trust_data_edits", record))
                        return;

                    //G1.update_db_table("trust_data_edits", "record", record, new string[] { "status", mainUpdate, "beginningPaymentBalance", Forethought.ToString(), "beginningDeathBenefit", SecurityNational.ToString(), "endingPaymentBalance", FdlicOldWebb.ToString(), "endingDeathBenefit", FdlicOldCCI.ToString(), "priorUnappliedCash", UnityOldBarham.ToString(), "currentUnappliedCash", UnityOldWebb.ToString(), "downPayments", CD.ToString(), "date", date.ToString("yyyy-MM-dd"), "trustName", trustCompany });

                    if ( column == "unityOldWebb")
                        G1.update_db_table("trust_data_edits", "record", record, new string[] { "status", mainUpdate, "currentUnappliedCash", dValue.ToString(), "date", date.ToString("yyyy-MM-dd"), "trustName", trustCompany });
                    else if (column == "unityOldBarham")
                        G1.update_db_table("trust_data_edits", "record", record, new string[] { "status", mainUpdate, "priorUnappliedCash", dValue.ToString(), "date", date.ToString("yyyy-MM-dd"), "trustName", trustCompany });
                    else if (column == "fdlicOldWebb")
                        G1.update_db_table("trust_data_edits", "record", record, new string[] { "status", mainUpdate, "endingPaymentBalance", dValue.ToString(), "date", date.ToString("yyyy-MM-dd"), "trustName", trustCompany });
                    else if (column == "fdlicOldCCI")
                        G1.update_db_table("trust_data_edits", "record", record, new string[] { "status", mainUpdate, "endingDeathBenefit", dValue.ToString(), "date", date.ToString("yyyy-MM-dd"), "trustName", trustCompany });
                }
            }
        }
        /****************************************************************************************/
        private void gridMain10_CellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            if (e == null)
                return;
            int rowHandle = gridMain10.FocusedRowHandle;
            int row = gridMain10.GetDataSourceRowIndex(rowHandle);
            DataRow dr = gridMain10.GetFocusedDataRow();
            if (dr == null)
                return;

            DataTable dt = (DataTable)dgv10.DataSource;
            string record = dr["record"].ObjToString();
            if (record == "0")
                record = "";

            DateTime date = this.dateTimePicker2.Value;

            string trustCompany = "Pre2002";
            double CD = dr["CD"].ObjToDouble();
            double bankCD = dr["bankCD"].ObjToDouble();

            string month = dr["month"].ObjToString().ToUpper();
            //month = dr["status"].ObjToString().ToUpper();
            string desc = dr["desc"].ObjToString();
            string otherDesc = dr["otherDesc"].ObjToString();

            if (month != "BEGINNING BALANCE" )
            {
                if (String.IsNullOrWhiteSpace(record))
                {
                    record = G1.create_record("trust_data_edits", "status", "-1");
                    dr["record"] = record.ObjToInt32();
                    G1.update_db_table("trust_data_edits", "record", record, new string[] { "status", "Line Edit", "preOrPost", "Pre" });
                    dr["record"] = record.ObjToInt32();
                }
                if (G1.BadRecord("trust_data_edits", record))
                    return;

                G1.update_db_table("trust_data_edits", "record", record, new string[] {  "downPayments", CD.ToString(), "growth", bankCD.ToString(), "date", date.ToString("yyyy-MM-dd"), "trustName", trustCompany, "position", row.ToString(), "insuredName", desc, "middleName", otherDesc, "billingReason", "CD" });

                dt = LoadLineEditsPreCD(dt, this.dateTimePicker2.Value);

                double totalDValue = 0D;
                double totalbankCD = 0D;
                double dValue = 0D;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    month = dt.Rows[i]["month"].ObjToString();
                    if (month.ToUpper().IndexOf("BEGINNING") >= 0)
                    {
                        totalDValue = dt.Rows[i]["CD"].ObjToDouble();
                        totalbankCD = dt.Rows[i]["bankCD"].ObjToDouble();
                        continue;
                    }
                    if (month.ToUpper().IndexOf("ENDING") >= 0)
                    {
                        dt.Rows[i]["CD"] = totalDValue;
                        dt.Rows[i]["bankCD"] = totalbankCD;
                        break;
                    }
                    dValue = dt.Rows[i]["CD"].ObjToDouble();
                    bankCD = dt.Rows[i]["bankCD"].ObjToDouble();

                    totalDValue += dValue;
                    totalbankCD += bankCD;
                }


                gridMain10.RefreshEditor(true);
                gridMain10.RefreshData();
                dgv10.Refresh();
                gridMain10.PostEditor();
                return;
            }
            else
            {
                string mainUpdate = "BeginningBalance";
                if (month == "BEGINNING BALANCE")
                {
                    mainUpdate = "BeginningBalance";
                    if (String.IsNullOrWhiteSpace(record))
                    {
                        record = G1.create_record("trust_data_edits", "status", "-1");
                        dr["record"] = record.ObjToInt32();
                        G1.update_db_table("trust_data_edits", "record", record, new string[] { "status", mainUpdate, "preOrPost", "Pre" });
                    }
                    if (G1.BadRecord("trust_data_edits", record))
                        return;
                    G1.update_db_table("trust_data_edits", "record", record, new string[] { "status", mainUpdate, "downPayments", CD.ToString(), "Growth", bankCD.ToString(), "date", date.ToString("yyyy-MM-dd"), "trustName", trustCompany });
                }
            }
        }
        /****************************************************************************************/
        private void gridMain10_MouseDown(object sender, MouseEventArgs e)
        {
            DataTable dt = (DataTable)dgv10.DataSource; // Leave as a GetDate example

            try
            {
                var hitInfo = gridMain10.CalcHitInfo(e.Location);
                if (hitInfo.InRowCell)
                {
                    int rowHandle = hitInfo.RowHandle;
                    gridMain10.FocusedRowHandle = rowHandle;
                    gridMain10.SelectRow(rowHandle);
                    gridMain10.RefreshEditor(true);
                    int row = gridMain10.GetDataSourceRowIndex(rowHandle);
                    GridColumn column = hitInfo.Column;
                    gridMain10.FocusedColumn = column;
                    string currentColumn = column.FieldName.Trim();
                    if (currentColumn.ToUpper() == "DATE")
                    {
                        DataRow dr = gridMain10.GetFocusedDataRow();
                        DateTime date = dr["date"].ObjToDateTime();
                        if (date.Year <= 1000)
                        {
                            date = dr["date"].ObjToDateTime();
                            if (date.Year < 1000)
                                date = DateTime.Now;
                        }
                        string record = dr["record"].ObjToString();
                        using (GetDate dateForm = new GetDate(date, "Enter Date"))
                        {
                            dateForm.TopMost = true;
                            dateForm.ShowDialog();
                            if (dateForm.DialogResult == System.Windows.Forms.DialogResult.OK)
                            {
                                date = dateForm.myDateAnswer;
                                dr["date"] = G1.DTtoMySQLDT(date);
                                if (!String.IsNullOrWhiteSpace(record))
                                    G1.update_db_table("trust_data_edits", "record", record, new string[] { "date", date.ToString("yyyy-MM-dd") });
                                //DataChanged();
                                gridMain10.ClearSelection();
                                gridMain10.FocusedRowHandle = rowHandle;

                                gridMain10.RefreshData();
                                gridMain10.RefreshEditor(true);
                                gridMain10.SelectRow(rowHandle);

                                if (date != workDate)
                                {
                                    dt.Rows.Remove(dr);
                                    gridMain10.RefreshData();
                                    gridMain10.RefreshEditor(true);
                                    //LoadEndingBalances(dt);
                                }
                            }
                            else if (dateForm.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                            {
                                date = dr["date"].ObjToDateTime(); // xyzzy
                                dr["date"] = G1.DTtoMySQLDT(DateTime.MinValue.ToString("MM/dd/yyyy"));
                                if (!String.IsNullOrWhiteSpace(record))
                                {
                                    G1.update_db_table("trust_data_edits", "record", record, new string[] { "date", DateTime.MinValue.ToString("yyyy-MM-dd") });
                                    string mainRecord = dr["record"].ObjToString();
                                }
                                ////DataChanged();
                                gridMain10.ClearSelection();
                                gridMain10.FocusedRowHandle = rowHandle;

                                gridMain10.RefreshData();
                                gridMain10.RefreshEditor(true);
                                gridMain10.SelectRow(rowHandle);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
        /****************************************************************************************/
        private void gridMain10_CustomColumnDisplayText(object sender, CustomColumnDisplayTextEventArgs e)
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
                    if (date.Year < 1500)
                        e.DisplayText = "";
                }
            }
        }
        /***************************************************************************************/
        public delegate void d_void_acceptTrustMoney ( DataTable dt );
        public event d_void_acceptTrustMoney acceptTrustMoney;
        protected void OnSaveSelected()
        {
            DataTable dt = (DataTable)dgv8.DataSource;
            acceptTrustMoney?.Invoke( dt );

            this.Close();
        }
        /****************************************************************************************/
        private void btnAccept_Click(object sender, EventArgs e)
        {
            OnSaveSelected();
        }
        /****************************************************************************************/
        private void gridMain8_CustomColumnDisplayText(object sender, CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column.FieldName.ToUpper().IndexOf("DATE") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                if (e.DisplayText.IndexOf("0000") >= 0 || e.DisplayText.IndexOf("0001") >= 0)
                    e.DisplayText = "";
                else
                {
                    DateTime date = e.DisplayText.ObjToString().ObjToDateTime();
                    e.DisplayText = date.ToString("MM/dd/yyyy");
                    if (date.Year < 1500)
                        e.DisplayText = "";
                }
            }
        }
        /****************************************************************************************/
        private void massPrintPost2002ReportsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("***Question*** Print Mass POST 2002 Reports?", "Post 2002 Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            if (result == DialogResult.No)
                return;

            PleaseWait pleaseForm = new PleaseWait("Please Wait!\nExporting Post Trust Deceased Data to Excel!");
            pleaseForm.TopMost = true;
            pleaseForm.Show();
            pleaseForm.Refresh();

            continuousPrint = true;

            DateTime stopDate = this.dateTimePicker2.Value;
            string year = stopDate.Year.ToString("D4");
            string month = stopDate.ToString("MMMM");

            DateTime startDate = new DateTime(stopDate.Year, 1, 1);

            string outputDirectory = "C:/SMFS Reports/Trust Deceased/" + year + " " + month + "/";
            G1.verify_path(outputDirectory);

            gridMain2.OptionsView.ShowFooter = false;
            gridMain6.OptionsView.ShowFooter = false;
            gridMain10.OptionsView.ShowFooter = false;
            gridMain11.OptionsView.ShowFooter = false;
            gridMain12.OptionsView.ShowFooter = false;
            gridMain13.OptionsView.ShowFooter = false;
            gridMain14.OptionsView.ShowFooter = false;

            gridMain2.OptionsPrint.PrintFooter = false;
            gridMain6.OptionsPrint.PrintFooter = false;
            gridMain10.OptionsPrint.PrintFooter = false;
            gridMain11.OptionsPrint.PrintFooter = false;
            gridMain12.OptionsPrint.PrintFooter = false;
            gridMain13.OptionsPrint.PrintFooter = false;
            gridMain14.OptionsPrint.PrintFooter = false;

            post2002ReportUnityToolStripMenuItem.PerformClick();
            btnRunTotals_Click(null, null);
            tabControl1.SelectedTab = tabPage2;

            gridMain2.OptionsPrint.PrintDetails = true;
            gridMain2.OptionsPrint.ExpandAllDetails = true;

            ExportPostReport(gridMain2, "Post Unity", outputDirectory );
            //if (1 == 1)
            //{
            //    continuousPrint = false;

            //    tabControl1.SelectedTab = tabPage1; // Go Back to Main Tab

            //    pleaseForm.FireEvent1();
            //    pleaseForm.Dispose();
            //    pleaseForm = null;
            //    return;
            //}

            post2002ReportFDLICToolStripMenuItem.PerformClick();
            gridMain2.OptionsView.ShowFooter = false;
            btnRunTotals_Click(null, null);
            tabControl1.SelectedTab = tabPage2;

            ExportPostReport(gridMain2, "Post FDLIC", outputDirectory);

            post2002ReportSNFTToolStripMenuItem.PerformClick();
            btnRunTotals_Click(null, null);
            tabControl1.SelectedTab = tabPage2;

            ExportPostReport(gridMain2, "Post SNFT", outputDirectory);

            post2002ReportCadenceToolStripMenuItem.PerformClick();
            btnRunTotals_Click(null, null);
            tabControl1.SelectedTab = tabPage2;

            ExportPostReport(gridMain2, "Post CD", outputDirectory);

            pleaseForm.FireEvent1();
            pleaseForm.Dispose();
            pleaseForm = null;

            pleaseForm = new PleaseWait("Please Wait!\nExporting Post Trust Deceased Counts to Excel!");
            pleaseForm.TopMost = true;
            pleaseForm.Show();
            pleaseForm.Refresh();

            tabControl1.SelectedTab = tabPage6; // History
            this.dateTimePicker3.Value = startDate;
            //this.dateTimePicker3.Value = new DateTime(2024, 8, 1); // For Testing
            this.dateTimePicker4.Value = stopDate;
            cmbPreOrPost2.Text = "Post";
            button1_Click(null, null);

            GridView gridView = (GridView)gridMain7;
            if (gridView != null)
            {
                gridView.OptionsPrint.AutoWidth = false;
                gridView.OptionsView.ColumnAutoWidth = false;
                gridView.BestFitColumns();
            }

            //fullPath = outputDirectory + "Post Totals.pdf";
            //printPreviewToolStripMenuItem_Click(null, null);
            //fullPath = outputDirectory + "Post Totals.xlsx";
            //printPreviewToolStripMenuItem_Click(null, null);
            //gridMain7.Columns["num"].Visible = true;

            ExportPostReport(gridMain7, "Post Totals", outputDirectory);

            continuousPrint = false;

            gridMain2.OptionsView.ShowFooter = true;
            gridMain6.OptionsView.ShowFooter = true;
            gridMain10.OptionsView.ShowFooter = true;
            gridMain11.OptionsView.ShowFooter = true;
            gridMain12.OptionsView.ShowFooter = true;
            gridMain13.OptionsView.ShowFooter = true;
            gridMain14.OptionsView.ShowFooter = true;

            gridMain2.OptionsPrint.PrintFooter = true;
            gridMain6.OptionsPrint.PrintFooter = true;
            gridMain10.OptionsPrint.PrintFooter = true;
            gridMain11.OptionsPrint.PrintFooter = true;
            gridMain12.OptionsPrint.PrintFooter = true;
            gridMain13.OptionsPrint.PrintFooter = true;
            gridMain14.OptionsPrint.PrintFooter = true;

            tabControl1.SelectedTab = tabPage1; // Go Back to Main Tab

            pleaseForm.FireEvent1();
            pleaseForm.Dispose();
            pleaseForm = null;
        }
        /****************************************************************************************/
        private void ExportPostReport (DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gMain, string mainFileName, string outputDirectory )
        {
            gMain.OptionsPrint.UsePrintStyles = true;
            gMain.OptionsPrint.PrintPreview = true;
            SetupGridWidths(gMain);
            if (G1.get_column_number(gMain, "num") >= 0)
                gMain.Columns["num"].Visible = false;

            fullPath = outputDirectory + mainFileName + ".pdf";
            printPreviewToolStripMenuItem_Click(null, null);

            fullPath = outputDirectory + mainFileName + ".xlsx";
            printPreviewToolStripMenuItem_Click(null, null);
            if (G1.get_column_number(gMain, "num") >= 0)
                gMain.Columns["num"].Visible = true;
        }
        /****************************************************************************************/
        private void massPrintPre2002ReportsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("***Question*** Print Mass Pre 2002 Reports?", "Pre 2002 Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            if (result == DialogResult.No)
                return;

            PleaseWait pleaseForm = new PleaseWait("Please Wait!\nExporting Pre Trust Deceased Data to Excel!");
            pleaseForm.TopMost = true;
            pleaseForm.Show();
            pleaseForm.Refresh();

            continuousPrint = true;

            DateTime stopDate = this.dateTimePicker2.Value;
            string year = stopDate.Year.ToString("D4");
            string month = stopDate.ToString("MMMM");

            DateTime startDate = new DateTime(stopDate.Year, 1, 1);

            string outputDirectory = "C:/SMFS Reports/Trust Deceased/" + year + " " + month + "/";
            G1.verify_path(outputDirectory);

            gridMain2.OptionsView.ShowFooter = false;
            gridMain6.OptionsView.ShowFooter = false;
            gridMain10.OptionsView.ShowFooter = false;
            gridMain11.OptionsView.ShowFooter = false;
            gridMain12.OptionsView.ShowFooter = false;
            gridMain13.OptionsView.ShowFooter = false;
            gridMain14.OptionsView.ShowFooter = false;


            gridMain2.OptionsPrint.PrintFooter = false;
            gridMain6.OptionsPrint.PrintFooter = false;
            gridMain10.OptionsPrint.PrintFooter = false;
            gridMain11.OptionsPrint.PrintFooter = false;
            gridMain12.OptionsPrint.PrintFooter = false;
            gridMain13.OptionsPrint.PrintFooter = false;
            gridMain14.OptionsPrint.PrintFooter = false;

            pre2002ReportSNFTToolStripMenuItem.PerformClick();
            btnRunTotals_Click(null, null);
            tabControl1.SelectedTab = tabPage2;

            tabControl2.SelectedTab = tabPage11;
            fullPath = outputDirectory + "Pre CD.pdf";
            printPreviewToolStripMenuItem_Click(null, null);
            fullPath = outputDirectory + "Pre CD.xlsx";
            printPreviewToolStripMenuItem_Click(null, null);

            tabControl2.SelectedTab = tabPage12;
            fullPath = outputDirectory + "Pre FDLIC Old Webb.pdf";
            printPreviewToolStripMenuItem_Click(null, null);
            fullPath = outputDirectory + "Pre FDLIC Old Webb.xlsx";
            printPreviewToolStripMenuItem_Click(null, null);

            tabControl2.SelectedTab = tabPage13;
            fullPath = outputDirectory + "Pre FDLIC Old CCI.pdf";
            printPreviewToolStripMenuItem_Click(null, null);
            fullPath = outputDirectory + "Pre FDLIC Old CCI.xlsx";
            printPreviewToolStripMenuItem_Click(null, null);

            tabControl2.SelectedTab = tabPage14;
            fullPath = outputDirectory + "Pre Unity Old Barham.pdf";
            printPreviewToolStripMenuItem_Click(null, null);
            fullPath = outputDirectory + "Pre Unity Old Barham.xlsx";
            printPreviewToolStripMenuItem_Click(null, null);

            tabControl2.SelectedTab = tabPage15;
            fullPath = outputDirectory + "Pre Unity Old Webb.pdf";
            printPreviewToolStripMenuItem_Click(null, null);
            fullPath = outputDirectory + "Pre Unity Old Webb.xlsx";
            printPreviewToolStripMenuItem_Click(null, null);

            tabControl2.SelectedTab = tabPage16;
            fullPath = outputDirectory + "Pre FT_SN.pdf";
            printPreviewToolStripMenuItem_Click(null, null);
            fullPath = outputDirectory + "Pre FT_SN.xlsx";
            printPreviewToolStripMenuItem_Click(null, null);

            continuousPrint = false;

            gridMain2.OptionsView.ShowFooter = true;
            gridMain6.OptionsView.ShowFooter = true;
            gridMain10.OptionsView.ShowFooter = true;
            gridMain11.OptionsView.ShowFooter = true;
            gridMain12.OptionsView.ShowFooter = true;
            gridMain13.OptionsView.ShowFooter = true;
            gridMain14.OptionsView.ShowFooter = true;


            gridMain2.OptionsPrint.PrintFooter = true;
            gridMain6.OptionsPrint.PrintFooter = true;
            gridMain10.OptionsPrint.PrintFooter = true;
            gridMain11.OptionsPrint.PrintFooter = true;
            gridMain12.OptionsPrint.PrintFooter = true;
            gridMain13.OptionsPrint.PrintFooter = true;
            gridMain14.OptionsPrint.PrintFooter = true;

            tabControl1.SelectedTab = tabPage1; // Go Back to Main Tab

            pleaseForm.FireEvent1();
            pleaseForm.Dispose();
            pleaseForm = null;
        }
        /***********************************************************************************************/
        private void SetupGridWidths(DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gMain = null)
        {
            GridView gridView = (GridView)gMain;
            DevExpress.XtraGrid.GridControl dgv = gMain.GridControl;
            DataTable dt = (DataTable)dgv.DataSource;

            bool autoWidth = false, columnAutoWidth = false;
            Dictionary<GridColumn, int> widthByColumn = null;
            if (gridView != null)
            {
                autoWidth = gridView.OptionsPrint.AutoWidth;
                columnAutoWidth = gridView.OptionsView.ColumnAutoWidth;
                widthByColumn = gridView.Columns.ToDictionary(x => x, x => x.Width );

                gridView.OptionsPrint.AutoWidth = false;
                gridView.OptionsView.ColumnAutoWidth = false;
                string str = "";
                int width = 0;

                foreach (var item in widthByColumn)
                {
                    str = item.Key.FieldName.ObjToString();
                    //str = (item.Key).ObjToString();
                    width = (item.Value).ObjToInt32();

                    gMain.Columns[str].Width = width;
                    //G1.SetColumnWidth(gridView, str, width);
                }

                //gridView.OptionsPrint.AutoWidth = false;
                //gridView.OptionsView.ColumnAutoWidth = false;
                //gridView.BestFitColumns();
            }
        }
        /****************************************************************************************/
        //private void splitRowToolStripMenuItem_Clickx(object sender, EventArgs e)
        //{
        //    DataTable dt = null;
        //    DataRow dr = null;
        //    string contractNumber = "";
        //    workPreOrPost = cmbPreOrPost.Text;

        //    if (dgv.Visible)
        //    {
        //        dt = (DataTable)dgv.DataSource;

        //        dr = gridMain.GetFocusedDataRow();
        //        if (dr == null)
        //            return;
        //        contractNumber = dr["trust"].ObjToString();
        //        mainRowIndex = gridMain10.GetFocusedDataSourceRowIndex();
        //        TrustSplit splitForm = new TrustSplit(this.dateTimePicker2.Value, workReport, contractNumber, dt, dr, workPreOrPost);
        //        DialogResult results = splitForm.ShowDialog();
        //        if (results == DialogResult.OK)
        //        {
        //        }
        //    }
        //    else
        //        return;
        //}
        /****************************************************************************************/
        private void btnMainInsert_Click(object sender, EventArgs e)
        {
            DataTable dt = null;
            DataRow dr = null;
            string contractNumber = "";
            workPreOrPost = cmbPreOrPost.Text;

            if (dgv.Visible)
            {
                dt = (DataTable)dgv.DataSource;

                dr = gridMain.GetFocusedDataRow();
                if (dr == null)
                    return;

                contractNumber = dr["trust"].ObjToString();
                //mainRowIndex = gridMain10.GetFocusedDataSourceRowIndex();
                //mainRowIndex = -1; // Do not allow this for Mismatches

                Insert_Split_Row(dgv, gridMain, true);
            }
            else
                return;
        }
        /****************************************************************************************/
        private void picMainDelete_Click(object sender, EventArgs e)
        {
            if (dgv.Visible)
            {
                DataTable dt = (DataTable)dgv.DataSource;

                DataRow dr = gridMain.GetFocusedDataRow();
                if (dr == null)

                    return;

                string status = dr["status"].ObjToString();
                if (status != "Line Edit")
                {
                    MessageBox.Show("***ERROR*** You can only delete user\ninserted rows!", "Delete Row Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    return;
                }

                DialogResult result = MessageBox.Show("***Question*** Are you sure you want to delete this split row?", "Delete Split Row Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                if (result == DialogResult.No)
                    return;

                string contractNumber = dr["contractNumber"].ObjToString();
                string policyNumber = dr["policyNumber"].ObjToString();
                string record = dr["record"].ObjToString();
                if (String.IsNullOrWhiteSpace(record))
                    return;

                int rowHandle = gridMain.FocusedRowHandle;
                int row = gridMain.GetDataSourceRowIndex(rowHandle);

                G1.delete_db_table("trust_data_edits", "record", record);

                DataRow[] dRows = dt.Select("policyNumber='" + policyNumber + "' AND middleName = 'REPLACE'");
                if ( dRows.Length > 0 )
                {
                    record = dRows[0]["record"].ObjToString();
                    if ( !String.IsNullOrWhiteSpace ( record ))
                    {
                        G1.update_db_table("trust_data", "record", record, new string[] { "middleName", "" });
                        dRows = dt.Select("record='" + record + "'");
                        if (dRows.Length > 0)
                        {
                            dRows[0]["middleName"] = "";
                        }
                    }
                }

                dt.Rows.RemoveAt(row);
                G1.NumberDataTable(dt);
                dgv.DataSource = dt;
                //gridMain.DeleteRow(rowHandle);
                //gridMain.RefreshEditor(true);
                //gridMain.RefreshData();
                dgv.Refresh();
            }
        }
        /****************************************************************************************/
        private void gridMain_CustomSummaryCalculate_1(object sender, DevExpress.Data.CustomSummaryEventArgs e)
        {
            string field = (e.Item as GridSummaryItem).FieldName.ObjToString().ToUpper();
            if (field != "VALUE" && field != "PRINCIPAL")
                return;
            DataTable dt = (DataTable)dgv.DataSource;
            double totalValue = 0D;
            double totalPrincipal = 0D;
            string middleName = "";
            double value = 0D;
            double princpal = 0D;
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                middleName = dt.Rows[i]["middleName"].ObjToString().ToUpper();
                if (middleName == "REPLACE")
                    continue;
                totalValue += dt.Rows[i]["value"].ObjToDouble();
                totalPrincipal += dt.Rows[i]["principal"].ObjToDouble();
            }

            if (field.ToUpper() == "VALUE")
                e.TotalValue = totalValue;
            else if (field.ToUpper() == "PRINCIPAL")
                e.TotalValue = totalPrincipal;
        }
        /****************************************************************************************/
        private void gridMain_ValidatingEditor_1(object sender, DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventArgs e)
        {
            //GridView view = sender as GridView;
            //DataRow dr = gridMain.GetFocusedDataRow();
            //string month = dr["month"].ObjToString();
            //string policyStatus = dr["policyStatus"].ObjToString().ToUpper();
            //if (policyStatus != "SPLIT")
            //    return;
        }
        /****************************************************************************************/
        private void gridMain_ShowingEditor(object sender, CancelEventArgs e)
        {
            GridView view = sender as GridView;
            DataRow dr = gridMain.GetFocusedDataRow();
            try
            {
                string policyStatus = dr["policyStatus"].ObjToString().ToUpper();
                if (policyStatus != "SPLIT")
                    e.Cancel = true;
            }
            catch ( Exception ex)
            {
            }
        }
        /****************************************************************************************/
        private void btnRP_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            DataTable dt = (DataTable)dgv.DataSource;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);

            string record = dr["record"].ObjToString();
            string status = dr["status"].ObjToString();
            string rp = dr["billingReason"].ObjToString();
            if (rp == "RP")
            {
                G1.update_db_table("trust_data_edits", "record", record, new string[] { "billingReason", "" });
                dr["billingReason"] = "";
                dt.Rows[row]["billingReason"] = "";
                status = dr["num"].ObjToString();
                status = status.Replace("RP", "").Trim();
                dr["num"] = status;
                dt.Rows[row]["num"] = status;
                gridMain.RefreshRow(rowHandle);
                gridMain.RefreshEditor(true);
                dgv.RefreshDataSource();
                dgv.Refresh();
                gridMain.PostEditor();
            }
            else
            {
                G1.update_db_table("trust_data_edits", "record", record, new string[] { "billingReason", "RP" });
                dr["billingReason"] = "RP";
                dt.Rows[row]["billingReason"] = "RP";
                status = dr["num"].ObjToString();
                status = status.Replace("RP", "").Trim();
                status = status + " RP";
                dr["num"] = status;
                dt.Rows[row]["num"] = status;
                gridMain.RefreshRow(rowHandle);
                gridMain.RefreshEditor(true);
                dgv.RefreshDataSource();
                dgv.Refresh();
                gridMain.PostEditor();
            }
        }
        /****************************************************************************************/
        private void gridMain6_RowCellStyle(object sender, RowCellStyleEventArgs e)
        {
            GridView View = sender as GridView;
            if (e.RowHandle >= 0)
            {
                string column = e.Column.FieldName.ToUpper();
                DataTable dt = (DataTable)dgv6.DataSource;
                int row = gridMain6.GetDataSourceRowIndex(e.RowHandle);

                string red1 = "";
                string red2 = "";

                int col = G1.get_column_number(dt, "Security National redx");
                if ( col >= 0 )
                    red1 = dt.Rows[row]["Security National redx"].ObjToString();

                col = G1.get_column_number(dt, "Forethought redx");
                if (col >= 0)
                    red2 = dt.Rows[row]["Forethought redx"].ObjToString();

                if ( red1 == "Y")
                {
                    if (column.IndexOf("SECURITY NATIONAL") >= 0)
                    {
                        e.Appearance.ForeColor = Color.Red;
                        ColorizeCell(e.Appearance);
                    }
                }
                if ( red2 == "Y" )
                {
                    if (column.IndexOf("FORETHOUGHT") >= 0)
                    {
                        e.Appearance.ForeColor = Color.Red;
                        ColorizeCell(e.Appearance);
                    }
                }
            }
        }
        /****************************************************************************************/
        private void trustMoneyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            totalMarked(dgv2, gridMain2, "value");
        }
        /****************************************************************************************/
        private void trustPrincipalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            totalMarked(dgv2, gridMain2, "received");
        }
        /****************************************************************************************/
        private void totalMarked ( GridControl dgv, AdvBandedGridView gridMain, string column )
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt == null)
            {
                MessageBox.Show("***INFO*** There are no rows of data!!", "Sum Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }
            if (dt.Rows.Count <= 0)
            {
                MessageBox.Show("***INFO*** There are no rows of data!!", "Sum Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }
            DataRow dr = gridMain.GetFocusedDataRow();

            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);


            int[] rows = gridMain.GetSelectedRows();
            int lastRow = dt.Rows.Count;
            lastRow = rows.Length;

            int count = 0;

            double total = 0D;
            double value = 0D;

            for (int i = 0; i < lastRow; i++)
            {
                row = rows[i];
                row = gridMain.GetDataSourceRowIndex(row);

                dr = dt.Rows[row];

                value = dr[column].ObjToDouble();
                total += value;
            }

            string str = G1.ReformatMoney(total);
            MessageBox.Show("***INFO*** Total of the data is " + str + "!!", "Sum Total Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
        }
        /****************************************************************************************/
    }
}
