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
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class TrustDataDeceased : DevExpress.XtraEditors.XtraForm
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
        private CheckedComboBoxEdit workCompanies = null;
        /****************************************************************************************/
        public TrustDataDeceased(CheckedComboBoxEdit companies, string report, string month, string year, string preOrPost, string oldStuff, DataTable dt, string nextDays )
        {
            InitializeComponent();

            workReport = report;
            workCompanies = companies;
            workMonth = month;
            workYear = year;
            workPreOrPost = preOrPost;
            workOldStuff = oldStuff;
            workDt = dt;

            if ( !String.IsNullOrWhiteSpace ( nextDays ))
            {
                if (G1.validate_numeric(nextDays))
                    workNextDays = nextDays.ObjToInt32();
            }

            SetupTotalsSummary();

            DoSetup();

            btnAdd.Hide();
            btnDelete.Hide();
            btnSave.Hide();
        }
        /****************************************************************************************/
        private void SetupTotalsSummary()
        {
            //AddSummaryColumn("beginningPaymentBalance", null);
            //AddSummaryColumn("beginningDeathBenefit", null);
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
        private void SetupDetailColumns ()
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

            if ( workReport == "Post 2002 Report - FDLIC")
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
        private void AddNewColumn ( string fieldName, string caption, int width )
        {
            if (G1.get_column_number(gridMain, fieldName) < 0)
                G1.AddNewColumn(gridMain, fieldName, caption, "", FormatType.None, width, true);
            else
                gridMain.Columns[fieldName].Visible = true;
            G1.SetColumnWidth(gridMain, fieldName, width);
            gridMain.Columns[fieldName].OptionsColumn.FixedWidth = true;
            gridMain.Columns[fieldName].AppearanceHeader.ForeColor = Color.Black;
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
        /****************************************************************************************/
        private void TrustDataDeceased_Load(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            string title = "Edit Trust Deceased for " + workMonth;
            if (workIndependent)
                title = "Edit Trust Deceased";
            this.Text = title;

            if (workIndependent)
                return;

            string month = "";
            string year = "";

            int iMonth = G1.ConvertMonthToIndex(workMonth);
            int iYear = workYear.ObjToInt32();

            int days = DateTime.DaysInMonth(iYear, iMonth);

            DateTime date1 = new DateTime(iYear, iMonth, 1);
            DateTime date2 = new DateTime(iYear, iMonth, days);

            DataTable newDt = workDt.Clone();
            int row = -1;
            for ( int i=0; i<workDt.Rows.Count; i++)
            {
                month = workDt.Rows[i]["month"].ObjToString();
                year = workDt.Rows[i]["year"].ObjToString();

                if ( month == workMonth && year == workYear )
                {
                    row = i;
                    if ( i > 0 )
                        G1.copy_dt_row(workDt, i-1, newDt, 0);
                    break;
                }
            }

            newDt = LoadDeceased(newDt, workDt, row );

            if ( row >= 0 )
                G1.copy_dt_row(workDt, row, newDt, newDt.Rows.Count );

            dgv.DataSource = newDt;
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private DataTable LoadDeceased(DataTable dx, DataTable workDt, int nextRow)
        {
            int iMonth = G1.ConvertMonthToIndex(workMonth);
            int iYear = workYear.ObjToInt32();

            int days = DateTime.DaysInMonth(iYear, iMonth);

            DateTime date1 = new DateTime(iYear, iMonth, 1);
            DateTime date2 = new DateTime(iYear, iMonth, days);

            string sDate1 = date1.ToString("yyyy-MM-dd");
            string sDate2 = date2.ToString("yyyy-MM-dd");

            string contractNumber = "";
            DataRow[] dRows = null;
            string paidFrom = "";
            string company = "";

            string companies = getCompanyQuery(workCompanies);

            string[] locIDs = workCompanies.EditValue.ToString().Split('|');
            if (locIDs.Length > 0)
                this.Text = locIDs[0].Trim() + " Trust Deceased for " + workMonth + ", " + workYear;

            DateTime dateReceived = DateTime.Now;
            DateTime dateFiled = DateTime.Now;
            DateTime reportDate = DateTime.Now;
            DateTime date = DateTime.Now;

            DateTime maxDate = date2.AddDays(1);
            DateTime minDate = date1.AddDays(-1);
            int previousMonth = minDate.Month;

            string cmd = "Select * from `trust_data` WHERE `deathPaidDate` >= '" + date1.ToString("yyyy-MM-dd") + "' AND `deathPaidDate` <= '" + date2.ToString("yyyy-MM-dd") + " 23:59:59' ";
            if (companies.Contains("Unity"))
            {
                cmd = "Select * from `trust_data` WHERE `date` >= '" + date1.ToString("yyyy-MM-dd") + "' AND `date` <= '" + date2.ToString("yyyy-MM-dd") + " 23:59:59' AND `statusReason` = 'DC' ";
                cmd = "SELECT * FROM `trust_data` t JOIN cust_payment_details c ON t.`contractNumber` = c.`contractNumber` WHERE `date` >= '" + sDate1 + "' AND `date` <= '" + sDate2 + " 23:59:59' AND `statusReason` = 'DC' AND c.`status` = 'Deposited' AND `type` = 'Trust' AND `paidFrom` = 'Unity' AND c.`dateReceived` >= '" + sDate1 + "' AND c.`dateReceived` <= '" + sDate2 + "' ";
                //cmd = "SELECT * FROM `trust_data` t JOIN cust_payment_details c ON t.`contractNumber` = c.`contractNumber` WHERE `date` >= '" + sDate1 + "' AND `date` <= '" + sDate2 + " 23:59:59' AND `statusReason` = 'DC' AND c.`status` = 'Deposited' AND `type` = 'Trust' AND `paidFrom` = 'Unity' AND c.`dateFiled` >= '" + sDate1 + "' AND c.`dateFiled` <= '" + sDate2 + "' ";
            }
            else if (companies.Contains("FDLIC"))
            {
                cmd = "SELECT * FROM `trust_data` t WHERE `deathPaidDate` >= '" + sDate1 + "' AND `deathPaidDate` <= '" + sDate2 + " 23:59:59' ";
                cmd = "SELECT * FROM `trust_data` t JOIN cust_payment_details c ON t.`contractNumber` = c.`contractNumber` WHERE `deathPaidDate` >= '" + sDate1 + "' AND `deathPaidDate` <= '" + sDate2 + " 23:59:59' AND c.`status` = 'Deposited' AND `type` = 'Trust' AND `paidFrom` = 'FDLIC' AND c.`dateReceived` >= '" + sDate1 + "' AND c.`dateReceived` <= '" + sDate2 + "' ";
            }
            if (!String.IsNullOrWhiteSpace(companies))
                cmd += " AND " + companies + " ";
            cmd += " ORDER by `deathPaidDate` ";
            cmd += ";";

            DataTable dt = G1.get_db_data(cmd);

            Trust85.FindContract(dt, "WM13030U");

            //cmd = "SELECT * FROM `trust_data` t JOIN cust_payment_details c ON t.`contractNumber` = c.`contractNumber` WHERE `date` >= '" + sDate1 + "' AND `date` <= '" + sDate2 + " 23:59:59' AND `statusReason` = 'DC' AND c.`status` = 'Deposited' AND `type` = 'Trust' AND `paidFrom` = 'Unity' AND c.`dateReceived` >= '" + sDate1 + "' AND c.`dateReceived` <= '" + sDate2 + "' ";
            cmd = "Select * from `trust_data` t JOIN cust_payment_details c ON t.`contractNumber` = c.`contractNumber` WHERE `reportDate` >= '" + sDate1 + "' AND `reportDate` <= '" + sDate2 + " 23:59:59' ";
            if (!String.IsNullOrWhiteSpace(companies))
            {
                string newCompany = companies;
                if (newCompany.IndexOf("FDLIC PB") >= 0)
                    newCompany = newCompany.Replace("'FDLIC PB'", "'FDLIC PB','FDLIC CCI'");
                cmd += " AND " + newCompany + " ";
            }
            cmd += " ORDER by `date`, `trustCompany`;  ";

            DataTable ddx = G1.get_db_data(cmd);

            dRows = null;
            for (int i = 0; i < ddx.Rows.Count; i++)
            {
                contractNumber = ddx.Rows[i]["contractNumber"].ObjToString();
                reportDate = ddx.Rows[i]["reportDate"].ObjToDateTime();
                dRows = dt.Select("contractNumber='" + contractNumber + "'");
                if (dRows.Length > 0)
                {
                    for (int j = 0; j < dRows.Length; j++)
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
                    ddx.Rows[i]["date"] = G1.DTtoMySQLDT(reportDate.ToString("yyyy-MM-dd"));
                    dt.ImportRow(ddx.Rows[i]);
                }
            }

            Trust85.FindContract(dt, "WM13030U");

            //cmd = "SELECT * FROM cust_payment_details c JOIN cust_payments b ON c.`paymentRecord` = b.`record` WHERE c.`dateReceived` >= '2022-12-01' AND c.`dateReceived` <= '2022-12-31 23:59:59' AND c.`status` = 'Deposited' AND c.`type` = 'Trust' AND c.`paidFrom` IN('Unity', 'Unity PB')  ORDER BY `dateReceived`;


            //SELECT* FROM cust_payment_details c JOIN cust_payments b ON c.`contractNumber` = b.`contractNumber` WHERE c.`dateReceived` >= '2022-12-01' AND c.`dateReceived` <= '2022-12-31 23:59:59' AND c.`status` = 'Deposited' AND b.`type` = 'Trust' AND c.`paidFrom` IN('Unity', 'Unity PB')  ORDER BY `dateReceived`;

            DateTime date3 = date2;
            date3 = date3.AddDays(workNextDays);
            string sDate3 = date3.ToString("yyyy-MM-dd");

            date3 = date1;
            date3 = date3.AddMonths(1);
            string sDate4 = date3.ToString("yyyy-MM-dd");

            cmd = "Select * FROM cust_payment_details c JOIN cust_payments b ON c.`contractNumber` = b.`contractNumber` WHERE c.`dateReceived` >= '" + sDate1 + "' AND c.`dateReceived` <= '" + sDate2 + " 23:59:59' AND c.`status` = 'Deposited' AND c.`type` = 'Trust' AND b.`type` = 'Trust' ";
            if (!String.IsNullOrWhiteSpace(companies))
            {
                string newCompany = companies;
                if (newCompany.IndexOf("FDLIC PB") >= 0)
                    newCompany = newCompany.Replace("'FDLIC PB'", "'FDLIC PB','FDLIC CCI'");
                cmd += " AND " + newCompany + " ";
            }
            cmd += " ORDER by `dateReceived`;  ";
            cmd = cmd.Replace("`trustCompany`", "`paidFrom`");

            ddx = G1.get_db_data(cmd);

            Trust85.FindContract(ddx, "WM13030U");

            DataTable dddd = null;

            for ( int i=0; i<ddx.Rows.Count; i++)
            {
                contractNumber = ddx.Rows[i]["trust_policy"].ObjToString();
                //company = ddx.Rows[i]["trustCompany"].ObjToString();
                //paidFrom = ddx.Rows[i]["paidFrom"].ObjToString().Trim();
                //if (company.ToUpper() != paidFrom.ToUpper())
                //    continue;
                dRows = dt.Select("contractNumber='" + contractNumber + "'");
                if ( dRows.Length > 0 )
                {
                }
                else
                {
                    cmd = "SELECT * FROM `trust_data` t JOIN `cust_payments` b ON t.`contractNumber` = b.`trust_policy` JOIN `cust_payment_details` c ON c.`paymentRecord` = b.`record` WHERE t.`contractNumber` = '" + contractNumber + "' AND `date` >= '" + sDate1 + "' AND `date` <= '" + sDate2 + " 23:59:59' ";

                    //cmd = "Select * from `trust_data` t JOIN cust_payment_details c ON t.`contractNumber` = c.`contractNumber` WHERE t.`contractNumber` = '" + contractNumber + "' AND `date` >= '" + sDate1 + "' AND `date` <= '" + sDate2 + " 23:59:59' ";
                    if (!String.IsNullOrWhiteSpace(companies))
                    {
                        string newCompany = companies;
                        if (newCompany.IndexOf("FDLIC PB") >= 0)
                            newCompany = newCompany.Replace("'FDLIC PB'", "'FDLIC PB','FDLIC CCI'");
                        cmd += " AND " + newCompany + " ";
                    }
                    cmd += " ORDER by `date`, `trustCompany`;  ";

                    dddd = G1.get_db_data(cmd);
                    if (dddd.Rows.Count > 0)
                    {
                        for ( int j=0; j<dddd.Rows.Count; j++)
                            dt.ImportRow(dddd.Rows[j]);
                    }
                }
            }

            cmd = "Select * FROM cust_payment_details c JOIN cust_payments b ON c.`contractNumber` = b.`contractNumber` WHERE c.`dateReceived` >= '" + sDate4 + "' AND c.`dateReceived` <= '" + sDate3 + " 23:59:59' AND c.`status` = 'Deposited' AND c.`type` = 'Trust' AND b.`type` = 'Trust' ";
            if (!String.IsNullOrWhiteSpace(companies))
            {
                string newCompany = companies;
                if (newCompany.IndexOf("FDLIC PB") >= 0)
                    newCompany = newCompany.Replace("'FDLIC PB'", "'FDLIC PB','FDLIC CCI'");
                cmd += " AND " + newCompany + " ";
            }
            cmd += " ORDER by `dateReceived`;  ";
            cmd = cmd.Replace("`trustCompany`", "`paidFrom`");

            ddx = G1.get_db_data(cmd);

            Trust85.FindContract(ddx, "WM13030U");

            dddd = null;

            DateTime monthend = sDate4.ObjToDateTime();
            days = DateTime.DaysInMonth(monthend.Year, monthend.Month);
            monthend = new DateTime(monthend.Year, monthend.Month, days);
            bool found = false;
            for (int i = 0; i < ddx.Rows.Count; i++)
            {
                contractNumber = ddx.Rows[i]["trust_policy"].ObjToString();
                //contractNumber = ddx.Rows[i]["contractNumber1"].ObjToString();
                //company = ddx.Rows[i]["trustCompany"].ObjToString();
                //paidFrom = ddx.Rows[i]["paidFrom"].ObjToString().Trim();
                //if (company.ToUpper() != paidFrom.ToUpper())
                //    continue;
                dRows = dt.Select("contractNumber='" + contractNumber + "'");
                if (dRows.Length > 0)
                {
                }
                else
                {
                    found = GetTrustReceived ( contractNumber, sDate4, monthend, companies, ref dt);
                    if ( !found )
                    {
                        contractNumber = ddx.Rows[i]["contractNumber"].ObjToString();
                        dRows = dt.Select("contractNumber='" + contractNumber + "'");
                        if ( dRows.Length <= 0 )
                            found = GetTrustReceived ( contractNumber, sDate4, monthend, companies, ref dt, true );
                    }
                    //cmd = "SELECT * FROM `trust_data` t JOIN `cust_payments` b ON t.`contractNumber` = b.`trust_policy` JOIN `cust_payment_details` c ON c.`paymentRecord` = b.`record` WHERE t.`contractNumber` = '" + contractNumber + "' AND `date` >= '" + sDate4 + "' AND `date` <= '" + monthend.ToString("yyyy-MM-dd") + " 23:59:59' ";
                    //if (!String.IsNullOrWhiteSpace(companies))
                    //{
                    //    string newCompany = companies;
                    //    if (newCompany.IndexOf("FDLIC PB") >= 0)
                    //        newCompany = newCompany.Replace("'FDLIC PB'", "'FDLIC PB','FDLIC CCI'");
                    //    cmd += " AND " + newCompany + " ";
                    //}
                    //cmd += " ORDER by `date`, `trustCompany`;  ";

                    //dddd = G1.get_db_data(cmd);
                    //if (dddd.Rows.Count > 0)
                    //{
                    //    for (int j = 0; j < dddd.Rows.Count; j++)
                    //        dt.ImportRow(dddd.Rows[j]);
                    //}
                }
            }

            Trust85.FindContract(dt, "P16075UI");

            if (companies.Contains("Unity"))
            {
                cmd = "Select * from `trust_data_edits` WHERE `date` >= '" + sDate1 + "' AND `date` <= '" + sDate2 + "' ";
                cmd += " AND `trustCompany` LIKE '" + workCompany + "%' ";
                cmd += " ORDER by `date`, `trustCompany`;  ";

                ddx = G1.get_db_data(cmd);
                ddx.Columns.Add("manual");
                for (int i = 0; i < ddx.Rows.Count; i++)
                    ddx.Rows[i]["manual"] = "Y";

                dt.Merge(ddx);

                //for ( int i=dt.Rows.Count-1; i>=0;  i-- )
                //{
                //    reportDate = dt.Rows[i]["reportDate"].ObjToDateTime();
                //    if (reportDate.Year > 1000)
                //    {
                //        if (reportDate < date1 || reportDate > date2)
                //        {
                //            dt.Rows.RemoveAt(i);
                //            continue;
                //        }
                //    }
                //    dateFiled = dt.Rows[i]["dateFiled"].ObjToDateTime();
                //    //if (dateFiled.Year <= 2000)
                //    //    dateFiled = dt.Rows[i]["tmstamp"].ObjToDateTime(); // This could cause a record to be missed
                //    dateReceived = dt.Rows[i]["dateReceived"].ObjToDateTime();
                //    if ( dateFiled.Month == previousMonth )
                //    {
                //        if (dateReceived.Day == 1)
                //            dt.Rows.RemoveAt(i);
                //    }
                //}
                //cmd = "SELECT * FROM `trust_data` t JOIN cust_payment_details c ON t.`contractNumber` = c.`contractNumber` WHERE `date` >= '" + sDate1 + "' AND `date` <= '" + sDate2 + " 23:59:59' AND `statusReason` = 'DC' AND c.`status` = 'Deposited' AND `type` = 'Trust' AND `paidFrom` = 'Unity' AND c.`dateFiled` >= '" + sDate1 + "' AND c.`dateFiled` <= '" + sDate2 + "' ";
                //if (!String.IsNullOrWhiteSpace(companies))
                //    cmd += " AND " + companies + " ";
                //cmd += " ORDER by `deathPaidDate` ";
                //cmd += ";";

                //DataTable dd = G1.get_db_data(cmd);
                //for ( int i=0; i<dd.Rows.Count; i++)
                //{
                //    contractNumber = dd.Rows[i]["contractNumber"].ObjToString();
                //    reportDate = dd.Rows[i]["reportDate"].ObjToDateTime();
                //    if (reportDate.Year > 1000)
                //    {
                //        if (reportDate < date1 || reportDate > date2)
                //        {
                //            dd.Rows.RemoveAt(i);
                //            continue;
                //        }
                //    }
                //    dRows = dt.Select("contractNumber='" + contractNumber + "'");
                //    if (dRows.Length <= 0)
                //    {
                //        dateReceived = dd.Rows[i]["dateReceived"].ObjToDateTime();
                //        if ( dateReceived == maxDate )
                //            dt.ImportRow(dd.Rows[i]);
                //    }
                //}
            }

            dt = verifyContracts(dt);

            Trust85.FindContract(dt, "WF15009UI");

            company = "";
            string preOrPost = "";
            date = DateTime.Now;
            double endingDeathBenefit = 0D;
            double snTotalDeceased = 0D;
            double foreTotalDeceased = 0D;
            string lastName = "";
            string firstName = "";
            string insuredName = "";
            contractNumber = "";
            int firstRow = 1;
            int secondRow = 1;
            DataRow dRow = null;
            dRows = null;

            DataTable[] mainDts = new DataTable[7];

            int dtCount = 0;
            DataTable tempDt = null;

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

            dRows = newTable.Select("trustCompany='Security National'");
            if (dRows.Length > 0)
                gotSN = LoadTrustCompany(dt, "Security National", "S/N", workDt, nextRow, ref mainDts, ref dtCount);

            dRows = newTable.Select("trustCompany='FORETHOUGHT'");
            if (dRows.Length > 0)
                gotFT = LoadTrustCompany(dt, "Forethought", "F/T", workDt, nextRow, ref mainDts, ref dtCount);

            dRows = newTable.Select("trustCompany='CD'");
            if (dRows.Length > 0)
                gotCD = LoadTrustCompany(dt, "CD", "CD", workDt, nextRow, ref mainDts, ref dtCount);

            dRows = newTable.Select("trustCompany='FDLIC'");
            if (dRows.Length > 0)
            {
                gotFDLIC = LoadTrustCompany(dt, "fdlic", "FDLIC", workDt, nextRow, ref mainDts, ref dtCount);
                gotFDOldWebb = LoadTrustCompany(dt, "fdlicOldWebb", "FDLIC", workDt, nextRow, ref mainDts, ref dtCount);
                gotFDOldCCI = LoadTrustCompany(dt, "fdlicOldCCI", "FDLIC", workDt, nextRow, ref mainDts, ref dtCount);
            }

            dRows = newTable.Select("trustCompany='Unity'");
            if (dRows.Length > 0)
            {
                gotUnity = LoadTrustCompany(dt, "unity", "Unity", workDt, nextRow, ref mainDts, ref dtCount);
                gotUnOldWebb = LoadTrustCompany(dt, "unityOldWebb", "Unity", workDt, nextRow, ref mainDts, ref dtCount);
                gotUnOldBar = LoadTrustCompany(dt, "unityOldBarham", "Unity", workDt, nextRow, ref mainDts, ref dtCount);
            }
            dRows = newTable.Select("trustCompany='Unity PB'");
            if ( dRows.Length > 0 )
                gotUnityPB = LoadTrustCompany(dt, "unity pb", "Unity PB", workDt, nextRow, ref mainDts, ref dtCount);

            dRows = newTable.Select("trustCompany='FDLIC PB'");
            if (dRows.Length > 0)
                gotFDLICPB = LoadTrustCompany(dt, "fdlic pb", "FDLIC PB", workDt, nextRow, ref mainDts, ref dtCount);

            dx = setTrustColumns(gotSN, dx, "sn");
            dx = setTrustColumns(gotFT, dx, "fore");
            if ( gotCD )
                dx = setTrustColumns(gotCD, dx, "cd");
            if ( gotFDOldWebb || gotFDOldCCI )
                dx = setTrustColumns(true , dx, "fdlic");
            else
                dx = setTrustColumns(false, dx, "fdlic");
            if (workReport == "Post 2002 Report - Unity")
            {
                if (gotUnity)
                {
                    dx = setTrustColumns(true, dx, "unity");
                    dx = setTrustColumns(true, dx, "unity trust");

                    dx.Columns.Add("unity trust");
                    //gridMain.Columns["unity trust"].Visible = true;
                }
                else
                    dx = setTrustColumns(false, dx, "unity");
                if (gotUnityPB)
                    dx = setTrustColumns(true, dx, "unity pb");
                else
                    dx = setTrustColumns(false, dx, "unity pb");
            }
            else if (workReport == "Post 2002 Report - FDLIC")
            {
                if (gotFDLIC)
                {
                    dx = setTrustColumns(true, dx, "fdlic");
                    dx = setTrustColumns(true, dx, "fdlic trust");

                    dx.Columns.Add("fdlic trust");

                    gotFDLICPB = false;
                    gridMain.Columns["FDLIC PB"].Visible = false;
                }
                else
                {
                    dx = setTrustColumns(false, dx, "fdlic");
                    dx = setTrustColumns(false, dx, "fdlic trust");
                }
                if (gotFDLICPB)
                    dx = setTrustColumns(true, dx, "fdlic pb");
                else
                    dx = setTrustColumns(false, dx, "fdlic pb");
            }
            else
            {
                if (gotUnOldWebb || gotUnOldBar)
                    dx = setTrustColumns(true, dx, "unity");
                else
                    dx = setTrustColumns(false, dx, "unity");
            }

            string trust = "";
            string trustCompany = "";
            double dValue = 0D;
            double value = 0D;
            string sDate = "";

            int maxRow = 0;
            for (int j = 0; j < dtCount; j++)
            {
                tempDt = mainDts[j];
                if (tempDt == null)
                    continue;
                maxRow += tempDt.Rows.Count;
                if (j < dtCount - 1)
                    maxRow = maxRow - 3;
            }

            for (int i = 0; i < maxRow; i++)
            {
                dRow = dx.NewRow();
                dx.Rows.Add(dRow);
                if (firstRow < 0)
                    firstRow = dx.Rows.Count - 1;
            }

            firstRow = 1;
            int fdlicRow = 1;
            int unityRow = 1;

            for ( int j=0; j<dtCount; j++)
            {
                tempDt = mainDts[j];
                if (tempDt == null)
                    continue;
                trustCompany = tempDt.TableName.ObjToString();
                dValue = tempDt.DisplayExpression.ObjToDouble();

                trust = "";
                if (trustCompany == "Security National")
                    trust = "sn";
                else if (trustCompany == "Forethought")
                    trust = "Fore";
                else if (trustCompany == "fdlicOldCCI")
                    trust = "fdlic";
                else if (trustCompany == "fdlicOldWebb")
                    trust = "fdlic";
                else if (trustCompany == "unityOldBarham")
                    trust = "unity";
                else if (trustCompany == "unityOldWebb")
                    trust = "unity";
                else if (trustCompany == "unity")
                    trust = "unity";
                else if (trustCompany == "unity pb")
                    trust = "unity pb";
                else if (trustCompany == "fdlic")
                    trust = "fdlic";
                else if (trustCompany == "fdlic pb")
                    trust = "fdlic pb";

                for ( int i=0; i<tempDt.Rows.Count; i++)
                {
                    insuredName = tempDt.Rows[i]["desc"].ObjToString();
                    sDate = tempDt.Rows[i]["date"].ObjToString();
                    contractNumber = tempDt.Rows[i]["contract"].ObjToString();
                    value = tempDt.Rows[i]["value"].ObjToDouble();

                    if (trust == "fdlic")
                    {
                        if (insuredName != "FDLIC Adjustment")
                        {
                            dx.Rows[fdlicRow + i][trust + " desc"] = insuredName;
                            dx.Rows[fdlicRow + i][trust + " date"] = sDate;
                            dx.Rows[fdlicRow + i][trust + " contract"] = contractNumber;
                            dx.Rows[fdlicRow + i][trust + " funeral"] = getFuneralService(contractNumber);
                            dx.Rows[fdlicRow + i][trustCompany] = value;


                            dx.Rows[fdlicRow + i][trust + " trust desc"] = insuredName;
                            dx.Rows[fdlicRow + i][trust + " trust date"] = sDate;
                            dx.Rows[fdlicRow + i][trust + " trust contract"] = contractNumber;
                            dx.Rows[fdlicRow + i][trust + " trust funeral"] = getFuneralService(contractNumber);
                            dx.Rows[fdlicRow + i][trustCompany + " trust"] = value;
                        }
                    }
                    else if (trust == "unity")
                    {
                        //if (insuredName != "Unity Adjustment")
                        //{
                            dx.Rows[unityRow + i][trust + " desc"] = insuredName;
                            dx.Rows[unityRow + i][trust + " date"] = sDate;
                            dx.Rows[unityRow + i][trust + " contract"] = contractNumber;
                            dx.Rows[unityRow + i][trust + " funeral"] = getFuneralService(contractNumber);
                            dx.Rows[unityRow + i][trustCompany] = value;

                            dx.Rows[unityRow + i][trust + " trust desc"] = insuredName;
                            dx.Rows[unityRow + i][trust + " trust date"] = sDate;
                            dx.Rows[unityRow + i][trust + " trust contract"] = contractNumber;
                            dx.Rows[unityRow + i][trust + " trust funeral"] = getFuneralService(contractNumber);
                            dx.Rows[unityRow + i][trustCompany + " trust"] = value;
                        //}
                    }
                    else
                    {
                        if (!String.IsNullOrWhiteSpace(trust))
                        {
                            dx.Rows[firstRow + i][trust + " desc"] = insuredName;
                            dx.Rows[firstRow + i][trust + " date"] = sDate;
                            dx.Rows[firstRow + i][trust + " contract"] = contractNumber;
                            dx.Rows[firstRow + i][trust + " funeral"] = getFuneralService(contractNumber);
                            dx.Rows[firstRow + i][trustCompany] = value;
                        }
                    }
                }
                if (trust == "fdlic")
                    fdlicRow++;
                else if (trust == "unity")
                    unityRow++;
            }
            if (workReport == "Post 2002 Report - FDLIC")
            {
                if (companies.Contains("FDLIC"))
                    dx = GrabFuneralInfo(dx, "FDLIC", mainDts, dtCount);
            }
            if (workReport == "Post 2002 Report - Unity")
            {
                if (companies.Contains("Unity"))
                {
                    dx = GrabFuneralInfo(dx, "Unity", mainDts, dtCount );
                    gridMain.Columns["unity trust"].Visible = true;
                }
            }

            return dx;
        }
        /***********************************************************************************************/
        private bool GetTrustReceived ( string contractNumber, string sDate4, DateTime monthend, string companies, ref DataTable dt, bool secondary = false )
        {
            bool found = false;
            string cmd = "SELECT * FROM `trust_data` t JOIN `cust_payments` b ON t.`contractNumber` = b.`trust_policy` JOIN `cust_payment_details` c ON c.`paymentRecord` = b.`record` WHERE t.`contractNumber` = '" + contractNumber + "' AND `date` >= '" + sDate4 + "' AND `date` <= '" + monthend.ToString("yyyy-MM-dd") + " 23:59:59' ";
            if ( secondary )
                cmd = "SELECT * FROM `trust_data` t JOIN `cust_payments` b ON t.`contractNumber` = b.`contractNumber` JOIN `cust_payment_details` c ON c.`paymentRecord` = b.`record` WHERE t.`contractNumber` = '" + contractNumber + "' AND `date` >= '" + sDate4 + "' AND `date` <= '" + monthend.ToString("yyyy-MM-dd") + " 23:59:59' ";
            if (!String.IsNullOrWhiteSpace(companies))
            {
                string newCompany = companies;
                if (newCompany.IndexOf("FDLIC PB") >= 0)
                    newCompany = newCompany.Replace("'FDLIC PB'", "'FDLIC PB','FDLIC CCI'");
                cmd += " AND " + newCompany + " ";
            }
            cmd += " ORDER by `date`, `trustCompany`;  ";

            DataTable dddd = G1.get_db_data(cmd);
            if (dddd.Rows.Count > 0)
            {
                for (int j = 0; j < dddd.Rows.Count; j++)
                {
                    dt.ImportRow(dddd.Rows[j]);
                    found = true;
                }
            }
            return found;
        }
        /***********************************************************************************************/
        private DataTable GrabFuneralInfo(DataTable dt, string company, DataTable[] mainDts, int dtCount )
        {
            string contractNumber = "";
            DataTable dx = null;
            string cmd = "";
            if (G1.get_column_number(dt, "dateReceived") < 0)
                dt.Columns.Add("dateReceived");
            if (G1.get_column_number(dt, "amtActuallyReceived") < 0)
                dt.Columns.Add("amtActuallyReceived", Type.GetType("System.Double"));

            double filedAmount = 0D;
            double receivedAmount = 0D;
            double growth = 0D;
            DataTable tempDt = null;
            int mainRow = 0;

            for (int k = 0; k < dtCount; k++)
            {
                tempDt = mainDts[k];
                if (G1.get_column_number(tempDt, "dateReceived") < 0)
                    tempDt.Columns.Add("dateReceived");
                if (G1.get_column_number(tempDt, "amtActuallyReceived") < 0)
                    tempDt.Columns.Add("amtActuallyReceived", Type.GetType("System.Double"));
                for (int i = 0; i < tempDt.Rows.Count; i++)
                {
                    contractNumber = tempDt.Rows[i]["contract"].ObjToString();
                    if (String.IsNullOrWhiteSpace(contractNumber))
                        continue;
                    if (contractNumber == "T05003")
                    {
                    }
                    cmd = "Select * from `cust_payment_details` WHERE `contractNumber` = '" + contractNumber + "' AND `status` = 'Deposited' AND `type` = 'Trust' AND `paidFrom` = '" + company + "';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                    {
                        filedAmount = dx.Rows[0]["trustAmtFiled"].ObjToDouble();
                        receivedAmount = dx.Rows[0]["amtActuallyReceived"].ObjToDouble();
                        growth = receivedAmount - filedAmount;
                        if (filedAmount > 0D)
                            receivedAmount = receivedAmount - growth;
                        tempDt.Rows[i]["amtActuallyReceived"] = receivedAmount;
                        tempDt.Rows[i]["dateReceived"] = dx.Rows[0]["dateReceived"].ObjToDateTime().ToString("MM/dd/yyyy");
                    }
                    else
                    {
                        cmd = "Select * from `cust_payments` WHERE `trust_policy` = '" + contractNumber + "';";
                        dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count > 0)
                        {
                            contractNumber = dx.Rows[0]["record"].ObjToString();
                            cmd = "Select * from `cust_payment_details` WHERE `paymentRecord` = '" + contractNumber + "' AND `status` = 'Deposited' AND `type` = 'Trust' AND `paidFrom` = '" + company + "';";
                            dx = G1.get_db_data(cmd);
                            if (dx.Rows.Count > 0)
                            {
                                filedAmount = dx.Rows[0]["trustAmtFiled"].ObjToDouble();
                                receivedAmount = dx.Rows[0]["amtActuallyReceived"].ObjToDouble();
                                growth = receivedAmount - filedAmount;
                                if (filedAmount > 0D)
                                    receivedAmount = receivedAmount - growth;
                                tempDt.Rows[i]["amtActuallyReceived"] = receivedAmount;
                                tempDt.Rows[i]["dateReceived"] = dx.Rows[0]["dateReceived"].ObjToDateTime().ToString("MM/dd/yyyy");
                            }
                        }
                    }
                }
            }

            DateTime date = DateTime.Now;
            double total = 0D;
            int row = 1;
            DataRow[] dRows = null;
            string funeral = "";
            string tableName = "";
            for (int k = 0; k < dtCount; k++)
            {
                tempDt = mainDts[k];
                tableName = tempDt.TableName.Trim();
                for ( int i=0; i<tempDt.Rows.Count; i++)
                {
                    funeral = "";
                    contractNumber = tempDt.Rows[i]["contract"].ObjToString();
                    if ( String.IsNullOrWhiteSpace ( contractNumber))
                    {
                        if (k < mainDts.Length )
                            continue;
                    }
                    if ( contractNumber == "P13028UI")
                    {
                    }
                    dRows = dt.Select("`" + tableName + " contract` = '" + contractNumber + "'") ;
                    if (dRows.Length > 0)
                        funeral = dRows[0][tableName + " funeral"].ObjToString();
                    date = tempDt.Rows[i]["dateReceived"].ObjToDateTime();
                    receivedAmount = tempDt.Rows[i]["amtActuallyReceived"].ObjToDouble();
                    if (date.Year < 1000)
                    {
                        date = tempDt.Rows[i]["date"].ObjToDateTime();
                        receivedAmount = tempDt.Rows[i]["value"].ObjToDouble();
                    }
                    dt.Rows[row][company + " trust contract"] = tempDt.Rows[i]["contract"].ObjToString();
                    dt.Rows[row][company + " trust desc"] = tempDt.Rows[i]["desc"].ObjToString();
                    dt.Rows[row][company + " trust date"] = G1.DTtoMySQLDT(date);
                    dt.Rows[row][company + " trust"] = receivedAmount;
                    dt.Rows[row][company + " trust funeral"] = funeral;
                    total += receivedAmount;
                    row++;

                }
            }
            //for ( int i=0; i<dt.Rows.Count; i++)
            //{
            //    date = dt.Rows[i]["dateReceived"].ObjToDateTime();
            //    receivedAmount = dt.Rows[i]["amtActuallyReceived"].ObjToDouble();
            //    dt.Rows[i][company + " trust date"] = G1.DTtoMySQLDT(date);
            //    dt.Rows[i][company + " trust"] = receivedAmount;
            //    total += receivedAmount;
            //}

            row = dt.Rows.Count - 2;
            dt.Rows[row][company + " trust"] = total;
            dt.Rows[row][company + " trust desc"] = company + " SMFS Deceased Total";

            return dt;
        }
        /***********************************************************************************************/
        private DataTable policyTrustsDt = null;
        /***********************************************************************************************/
        private DataTable verifyContracts ( DataTable dt)
        {
            string policyNumber = "";
            string contractNumber = "";
            string company = "";
            string type = "";
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                if ( String.IsNullOrWhiteSpace ( contractNumber) || contractNumber == "NULL" )
                {
                    policyNumber = dt.Rows[i]["policyNumber"].ObjToString();
                    company = dt.Rows[i]["trustCompany"].ObjToString();
                    contractNumber = FindContractNumber(policyNumber, company, ref type);
                    if (!String.IsNullOrWhiteSpace(contractNumber))
                        dt.Rows[i]["contractNumber"] = contractNumber;
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
                    cmd = "policyNumber='" + policyNumber + "' AND `Company` = '" + Company + "'";
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
        private DataTable setTrustColumns ( bool got, DataTable dx, string trust )
        {
            if (got)
            {
                dx.Columns.Add(trust+" desc");
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

            double beginningPaymentBalance = 0D;
            double endingPaymentBalance = 0D;
            double beginningDeathBenefit = 0D;
            double endingDeathBenefit = 0D;
            double money = 0D;


            string paymentRecord = "";
            string oldPaymentRecord = "";
            for (int i = (ddd.Rows.Count - 1); i >= 0; i--)
            {
                paymentRecord = ddd.Rows[i]["paymentRecord"].ObjToString();
                if (String.IsNullOrWhiteSpace(paymentRecord))
                    continue;
                if (paymentRecord == oldPaymentRecord)
                {
                    beginningPaymentBalance = ddd.Rows[i]["beginningPaymentBalance"].ObjToDouble();
                    endingPaymentBalance = ddd.Rows[i]["endingPaymentBalance"].ObjToDouble();
                    beginningDeathBenefit = ddd.Rows[i]["beginningDeathBenefit"].ObjToDouble();
                    endingDeathBenefit = ddd.Rows[i]["endingDeathBenefit"].ObjToDouble();

                    money = ddd.Rows[i+1]["beginningDeathBenefit"].ObjToDouble();
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
                oldPaymentRecord = paymentRecord;
            }

            DataRow [] mRows = ddd.Select("position='Top'");
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
            string contractNumber = "";

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
            string policyNumber = "";
            string oldPolicyNumber = "";
            int lastRow = -1;

            double total = 0D;
            double other = 0D;
            string position = "";
            string manual = "";
            double dValue = 0D;
            bool gotManual = false;
            if (G1.get_column_number(ddd, "manual") >= 0)
                gotManual = true;
            for (int i = 0; i < dRows.Length; i++)
            {
                position = dRows[i]["position"].ObjToString().ToUpper();
                if (gotManual)
                {
                    manual = dRows[i]["manual"].ObjToString();
                    if (manual == "Y")
                        dRows[i]["dateReceived"] = G1.DTtoMySQLDT(dRows[i]["deathPaidDate"].ObjToDateTime().ToString("yyyy-MM-dd"));
                }
                preOrPost = dRows[i]["preOrPost"].ObjToString();
                date = dRows[i]["deathPaidDate"].ObjToDateTime();
                firstName = dRows[i]["firstName"].ObjToString();
                lastName = dRows[i]["lastName"].ObjToString();
                if (!String.IsNullOrWhiteSpace(lastName))
                    insuredName = firstName + " " + lastName;
                else
                    insuredName = dRows[i]["insuredName"].ObjToString();
                endingDeathBenefit = dRows[i]["endingDeathBenefit"].ObjToDouble();
                if (workOldStuff != "YES")
                {
                    if (trustCompany.ToUpper() == "FDLIC" || trustCompany.ToUpper() == "FDLIC PB")
                        endingDeathBenefit = dRows[i]["beginningPaymentBalance"].ObjToDouble();
                }
                contractNumber = dRows[i]["contractNumber"].ObjToString().ToUpper();
                if ( contractNumber == "T05003")
                {

                }

                if (trustCompany == "Security National" || trustCompany == "Forethought")
                {
                    if (preOrPost.ToUpper() != "POST")
                        continue;
                }
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
                    if ( !String.IsNullOrWhiteSpace ( contractNumber))
                    {
                        mRows = tempDt.Select("contract='" + contractNumber + "'");
                        if ( mRows.Length > 0 )
                        {

                            dValue = mRows[0]["value"].ObjToDouble();
                            dValue += endingDeathBenefit;
                            mRows[0]["value"] = dValue;

                            if (!String.IsNullOrWhiteSpace(contractNumber))
                                total += endingDeathBenefit;
                            else
                                other += endingDeathBenefit;
                            continue;
                        }
                    }
                }

                if (workOldStuff == "YES")
                {
                    if (trustCompany == "Security National" || trustCompany == "Forethought")
                    {
                        if (preOrPost.ToUpper() != "PRE")
                            continue;
                    }
                    dRow = tempDt.NewRow();
                    dRow["desc"] = insuredName;
                    dRow["date"] = date.ToString("yyyy-MM-dd");
                    dRow["contract"] = contractNumber;
                    dRow["value"] = endingDeathBenefit;
                    tempDt.Rows.Add(dRow);

                    if ( !String.IsNullOrWhiteSpace ( contractNumber ))
                        total += endingDeathBenefit;
                    else
                        other += endingDeathBenefit;
                }
                else
                {
                    dRow = tempDt.NewRow();
                    dRow["desc"] = insuredName;
                    dRow["date"] = date.ToString("yyyy-MM-dd");
                    dRow["contract"] = contractNumber;
                    dRow["value"] = endingDeathBenefit;

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

            dRow = tempDt.NewRow();
            tempDt.Rows.Add(dRow);

            dRow = tempDt.NewRow();
            dRow["desc"] = trust + " Deceased Total";
            dRow["date"] = "";
            dRow["contract"] = "";
            dRow["value"] = total;
            tempDt.Rows.Add(dRow);

            dRow = tempDt.NewRow();
            dRow["desc"] = trust + " Other Total";
            dRow["date"] = "";
            dRow["contract"] = "";
            dRow["value"] = other;
            tempDt.Rows.Add(dRow);

            double diff = firstValue - total - other;

            double newDiff = nextValue - diff;
            newDiff = G1.RoundValue(newDiff);

            dRow = tempDt.NewRow();
            dRow["desc"] = trust + " Adjustment";
            dRow["date"] = "";
            dRow["contract"] = "";
            dRow["value"] = newDiff;
            tempDt.Rows.Add(dRow);


            mainDts[dtCount] = tempDt;
            dtCount++;
            return true;
        }
        /****************************************************************************************/
        private string getFuneralService ( string contractNumber )
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
                    serviceId = dx.Rows[0]["serviceId"].ObjToString();
            }
            return serviceId;
        }
        /****************************************************************************************/
        private DataTable CreateTempDt ()
        {
            DataTable tempDt = new DataTable();
            tempDt.Columns.Add("desc");
            tempDt.Columns.Add("date");
            tempDt.Columns.Add("contract");
            tempDt.Columns.Add("funeral");
            tempDt.Columns.Add("value", Type.GetType("System.Double"));
            tempDt.Columns.Add("deathClaimAmount", Type.GetType("System.Double"));
            return tempDt;
        }
        /****************************************************************************************/
        private void TrustDataDeceased_Loadx(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            string title = "Edit Trust Deceased for " + workMonth;
            this.Text = title;

            string[] Lines = workMonth.Split(' ');
            if (Lines.Length < 2)
                return;

            string str = Lines[0].Trim();
            int iMonth = G1.ConvertMonthToIndex(str);
            string year = Lines[1].Trim();
            int iYear = year.ObjToInt32();

            int days = DateTime.DaysInMonth(iYear, iMonth);

            DateTime date1 = new DateTime(iYear, iMonth, 1);
            DateTime date2 = new DateTime(iYear, iMonth, days);

            workDate = date2;

            string startDate = date1.ToString("yyyy-MM-dd");
            string stopDate = date2.ToString("yyyy-MM-dd");

            this.Cursor = Cursors.WaitCursor;

            string newCompany = workCompany;
            if (workCompany == "Unity Old Barham")
                newCompany = "Unity";

            string cmd = "Select * from `trust_data` WHERE `date` >= '" + startDate + "' AND `date` <= '" + stopDate + "' ";
            cmd += " AND `trustCompany` LIKE '" + newCompany + "%' ";
            cmd += " ORDER by `date`, `trustCompany`;  ";

            DataTable dt = G1.get_db_data(cmd);

            cmd = "Select * from `trust_data_edits` WHERE `date` >= '" + startDate + "' AND `date` <= '" + stopDate + "' ";
            cmd += " AND `trustCompany` LIKE '" + workCompany + "%' ";
            cmd += " ORDER by `date`, `trustCompany`;  ";

            DataTable dx = G1.get_db_data(cmd);
            dx.Columns.Add("manual");
            for (int i = 0; i < dx.Rows.Count; i++)
                dx.Rows[i]["manual"] = "Y";

            dt.Merge(dx);

            string saveName = "TrustDataEdit Primary";
            string skinName = "";
            if (!String.IsNullOrWhiteSpace(workCompany))
                saveName = "TrustDataEdit " + workCompany;

            foundLocalPreference = G1.RestoreGridLayoutExact(this, this.dgv, gridMain, LoginForm.username, saveName, ref skinName);
            if ( !String.IsNullOrWhiteSpace ( workCompany ))
                loadGroupCombo(cmbSelectColumns, "TrustDataEdit", workCompany);
            else
                loadGroupCombo(cmbSelectColumns, "TrustDataEdit", "Primary");

            if ( !String.IsNullOrWhiteSpace ( workCompany ))
                cmbSelectColumns.Text = workCompany;
            else
                cmbSelectColumns.Text = "Primary";

            ScaleCells();

            G1.NumberDataTable(dt);

            dgv.DataSource = dt;

            btnSave.Hide();
            btnSave.Refresh();

            loading = false;

            this.Cursor = Cursors.Default;
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

            DataTable ddd = (DataTable)dgv.DataSource;

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
            Printer.setupPrinterQuads(e, 2, 3);
            Font font = new Font("Ariel", 16);
            Printer.DrawQuad(1, 1, Printer.xQuads, 2, "South Mississippi Funeral Services, LLC", Color.Black, BorderSide.Top, font, HorizontalAlignment.Center);

            Printer.SetQuadSize(12, 12);

            font = new Font("Ariel", 8);
            Printer.DrawGridDate(2, 3, 2, 3, Color.Black, BorderSide.None, font);
            Printer.DrawGridPage(11, 3, 2, 3, Color.Black, BorderSide.None, font);

            //            Printer.DrawQuad(1, 9, 2, 3, "User : " + LoginForm.username, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);

            font = new Font("Ariel", 10, FontStyle.Regular);
            string title = this.Text;
            string location = "";
            string trusts = "";

            if (!String.IsNullOrWhiteSpace(location))
                title += " " + location;
            if (!String.IsNullOrWhiteSpace(trusts))
                title += " (" + trusts + ")";

            string user = LoginForm.username;
            string format = cmbSelectColumns.Text.Trim();
            //if (!String.IsNullOrWhiteSpace(format))
            //    user += " Format " + format;
            Printer.DrawQuad(6, 7, 4, 3, title, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            Printer.DrawQuad(1, 9, 4, 3, "User : " + user, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            if ( !string.IsNullOrWhiteSpace ( format))
                Printer.DrawQuad(3, 9, 4, 3, "Format : " + format, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);


            string workDate = workMonth;
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
            int row = e.ListSourceRow;
            if (row >= 0)
            {
                DataTable dt = (DataTable)dgv.DataSource;
                if (dt == null)
                    return;
                string str = dt.Rows[row]["status"].ObjToString().ToUpper();
                if ( str == "DELETE")
                {
                    e.Visible = false;
                    e.Handled = true;
                    return;
                }
            }
        }
        /****************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        { // Spy Glass
            G1.SpyGlass(gridMain);
        }
        /****************************************************************************************/
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (e == null)
                return;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            DataRow dr = gridMain.GetFocusedDataRow();
            if (dr == null)
                return;

            DataTable dt = (DataTable)dgv.DataSource;
        }
        /****************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataTable ddd = (DataTable)dgv.DataSource;

            DataRow dr = gridMain.GetFocusedDataRow();
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);

            string contract = dr["contractNumber"].ObjToString();
            if (!String.IsNullOrWhiteSpace(contract))
            {
                this.Cursor = Cursors.WaitCursor;
                CustomerDetails clientForm = new CustomerDetails(contract);
                clientForm.ShowDialog();
                this.Cursor = Cursors.Default;
            }
        }
        /****************************************************************************************/
        private void gridMain_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
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
            if (originalSize == 0D)
            {
                //                originalSize = gridMain.Columns["address1"].AppearanceCell.FontSizeDelta.ObjToDouble();
                originalSize = gridMain.Columns["Security National"].AppearanceCell.Font.Size;
                mainFont = gridMain.Columns["Security National"].AppearanceCell.Font;
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
            dgv.Refresh();
            this.Refresh();
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
        private void btnAdd_Click(object sender, EventArgs e)
        {
            //DataTable dt = (DataTable)dgv.DataSource;

            //DataRow dRow = dt.NewRow();
            //dRow["date"] = G1.DTtoMySQLDT(workDate);
            //dRow["trustCompany"] = workCompany;
            //dRow["manual"] = "Y";
            //dt.Rows.Add(dRow);

            //G1.NumberDataTable(dt);
            //dt.AcceptChanges();
            //dgv.DataSource = dt;

            //G1.GoToLastRow(gridMain);

            //btnSave.Show();
            //btnSave.Refresh();

            ////dgv.Refresh();
        }
        /****************************************************************************************/
        private void gridMain_ValidatingEditor(object sender, DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventArgs e)
        {
            GridView view = sender as GridView;
            DataRow dr = gridMain.GetFocusedDataRow();
            string manual = dr["manual"].ObjToString().ToUpper();
            if (manual != "Y")
            {
                e.Valid = false;
                return;
            }
        }
        /****************************************************************************************/
        private void btnSave_Click(object sender, EventArgs e)
        {
            //DataTable dt = (DataTable)dgv.DataSource;
            //if (G1.get_column_number(dt, "manual") < 0)
            //    return;

            //string data = "";
            //string type = "";
            //string field = "";
            //string record = "";
            //string modList = "";

            //DataTable dx = null;
            //DataRow[] dRows = dt.Select("status='DELETE' ");
            //if (dRows.Length <= 0)
            //    dx = dt.Clone();
            //else
            //    dx = dRows.CopyToDataTable();
            //for ( int i=0; i<dRows.Length; i++)
            //{
            //    record = dRows[i]["record"].ObjToString();
            //    if (record == "-1")
            //        continue;
            //    if ( !String.IsNullOrWhiteSpace ( record ))
            //    {
            //        G1.delete_db_table("trust_data_edits", "record", record);
            //        dRows[i]["record"] = -1;
            //    }
            //}

            //dRows = dt.Select("manual='Y' ");
            //if (dRows.Length <= 0)
            //    return;
            //dx = dRows.CopyToDataTable();

            //for (int i = 0; i < dRows.Length; i++)
            //{
            //    record = dRows[i]["record"].ObjToString();
            //    if (record == "-1")
            //        continue;
            //    if (String.IsNullOrWhiteSpace(record))
            //    {
            //        record = G1.create_record("trust_data_edits", "status", "-1");
            //        dRows[i]["record"] = record.ObjToInt32();
            //        G1.update_db_table("trust_data_edits", "record", record, new string[] {"status", "" });
            //    }
            //    if (G1.BadRecord("trust_data_edits", record))
            //        return;

            //    modList = "";

            //    for (int j = 0; j < dx.Columns.Count; j++)
            //    {
            //        field = dx.Columns[j].ColumnName;
            //        if (field.ToUpper() == "NUM")
            //            continue;
            //        if (field.ToUpper() == "RECORD")
            //            continue;
            //        if (field.ToUpper() == "MANUAL")
            //            continue;

            //        data = dRows[i][j].ObjToString();
            //        if (G1.get_column_number(dt, field) >= 0)
            //        {
            //            try
            //            {
            //                type = dt.Columns[field].DataType.ToString().ToUpper();
            //                if (data.IndexOf(",") >= 0)
            //                {
            //                    G1.update_db_table("trust_data_edits", "record", record, new string[] { field, data });
            //                    continue;
            //                }
            //                if (String.IsNullOrWhiteSpace(data))
            //                    data = "NODATA";
            //                modList += field + "," + data + ",";
            //            }
            //            catch (Exception ex)
            //            {
            //            }
            //        }
            //    }

            //    modList = modList.TrimEnd(',');
            //    G1.update_db_table("trust_data_edits", "record", record, modList);
            //}

            //btnSave.Hide();
            //btnSave.Refresh();
        }
        /****************************************************************************************/
        private void btnDelete_Click(object sender, EventArgs e)
        {
            //DataTable dt = (DataTable)dgv.DataSource;
            //if (dt.Rows.Count <= 0)
            //    return;

            //if (G1.get_column_number(dt, "manual") < 0)
            //    return;

            //DataRow dr = gridMain.GetFocusedDataRow();
            //string manual = dr["manual"].ObjToString();
            //if (String.IsNullOrWhiteSpace(manual))
            //    return;
            //dr["status"] = "DELETE";

            //G1.NumberDataTable(dt);
            //dt.AcceptChanges();
            //dgv.DataSource = dt;
            //dgv.Refresh();

            //btnSave.Show();
            //btnSave.Refresh();
        }
        /****************************************************************************************/
        private void gridMain_RowCellStyle(object sender, RowCellStyleEventArgs e)
        {
            GridView View = sender as GridView;
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
            }
        }
        /****************************************************************************************/
        private void gridMain_CellValueChanged_1(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            btnSave.Show();
            btnSave.Refresh();
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

            int iMonth = G1.ConvertMonthToIndex(workMonth);
            int iYear = workYear.ObjToInt32();

            int days = DateTime.DaysInMonth(iYear, iMonth);

            DateTime date1 = new DateTime(iYear, iMonth, 1);
            DateTime date2 = new DateTime(iYear, iMonth, days);

            string trust = "";
            bool first = true;
            bool found = false;
            string cmd = "Select * from `trust2013r` WHERE `payDate8` = '" + date2.ToString("yyyy-MM-dd") + "' and `currentRemovals` > '0' ";
            string[] locIDs = null;
            locIDs = workCompanies.EditValue.ToString().Split('|');
            if ( locIDs.Length > 0 )
            {
                cmd += " AND (";
                for ( int i=0; i<locIDs.Length; i++)
                {
                    trust = locIDs[i].Trim().ToUpper();
                    if ( !String.IsNullOrWhiteSpace ( trust) && found && first )
                    {
                        first = false;
                        cmd += " OR ";
                    }
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

            DataTable ddx = null;
            DataRow[] dRows = null;
            int lastRow = 0;
            double surrender = 0D;
            double overShort = 0D;
            int oldRow = -1;
            string lastName = "";

            dgv.DataSource = dt;
            gridMain.ExpandAllGroups();

            gridMain.RefreshData();
            gridMain.RefreshEditor(true);
            dgv.Refresh();

            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void gridMain_CustomColumnDisplayText_1(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
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
            else if ( e.DisplayText == "0.00" || e.DisplayText == "0")
                e.DisplayText = "";
            else
            {
                if ( e.Column.DisplayFormat.FormatType == FormatType.Numeric )
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
        }
        /****************************************************************************************/
        private void goToPaymentsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt == null)
                return;
            if (dt.Rows.Count <= 0)
                return;

            string[] Lines = workCompanies.EditValue.ToString().Split('|');

            string company = Lines[0].Trim();

            DataRow dr = gridMain.GetFocusedDataRow();

            string contractNumber = dr[company + " contract"].ObjToString();
            string serviceId = dr[company + " funeral"].ObjToString();

            if (!String.IsNullOrWhiteSpace(serviceId))
            {
                string cmd = "select * from fcust_extended where `serviceId` = '" + serviceId + "';";
                DataTable ddx = G1.get_db_data(cmd);
                if (ddx.Rows.Count > 0)
                    contractNumber = ddx.Rows[0]["contractNumber"].ObjToString();
            }

            this.Cursor = Cursors.WaitCursor;
            using (FunPayments editFunPayments = new FunPayments(null, contractNumber, "", false, true))
            {
                editFunPayments.TopMost = true;
                editFunPayments.ShowDialog();
            }
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void goToDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt == null)
                return;
            if (dt.Rows.Count <= 0)
                return;

            string[] Lines = workCompanies.EditValue.ToString().Split('|');

            string company = Lines[0].Trim();

            DataRow dr = gridMain.GetFocusedDataRow();

            string contractNumber = dr[company + " contract"].ObjToString();

            int iMonth = G1.ConvertMonthToIndex(workMonth);
            int iYear = workYear.ObjToInt32();

            int days = DateTime.DaysInMonth(iYear, iMonth);

            DateTime date1 = new DateTime(iYear, iMonth, 1);
            DateTime date2 = new DateTime(iYear, iMonth, days);

            using (ImportTrustFile editImport = new ImportTrustFile(company, company, contractNumber, date2 ))
            {
                editImport.TopMost = true;
                editImport.ShowDialog();
            }
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void btnVerifyTrusts_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            string filename = "C:/Sandra Files/ROBBY COPY OF 2023 SPREADSHEETS/2023 POST 2002 COMBINED - Copy";
            string sheetName = "UN JAN - AUG";
            DataTable workDt = ExcelWriter.ReadFile2(filename, 0, sheetName );

            int workCount = 0;
            DataTable tempDt = workDt.Clone();
            string month = "";
            int iMonth = G1.ConvertMonthToIndex(workMonth);
            DateTime date = new DateTime(workYear.ObjToInt32(), iMonth, 1);
            DateTime nextDate = date.AddMonths(1);
            string nextMonth = nextDate.ToString("MMMMMMMMMM");

            for ( int i=0; i<workDt.Rows.Count; i++)
            {
                month = workDt.Rows[i][0].ObjToString();
                if (month.ToUpper() == nextMonth.ToUpper())
                    break;
                if ( month.ToUpper() == workMonth.ToUpper())
                {
                    workCount++;
                    if (workCount >= 2)
                        break;
                    if ( workCount >= 1 )
                    {
                        tempDt.ImportRow(workDt.Rows[i]);
                    }
                }
                else if ( workCount >= 1 )
                    tempDt.ImportRow(workDt.Rows[i]);
            }

            DataTable trustDt = new DataTable();
            trustDt.Columns.Add("Trust");
            trustDt.Columns.Add("Date Received");
            trustDt.Columns.Add("Funeral Number");
            trustDt.Columns.Add("Name");

            trustDt.Columns.Add("Sandra Money", Type.GetType("System.Double"));
            trustDt.Columns.Add("Trust Money", Type.GetType("System.Double"));
            trustDt.Columns.Add("Difference", Type.GetType("System.Double"));

            DataRow dRow = null;
            DataRow[] dRows = null;

            string contractNumber = "";
            string name  = "";
            double dValue = 0D;
            double sandraTotal = 0D;
            double trustTotal = 0D;
            double difference = 0D;
            double sandraMoney = 0D;

            for ( int i=0; i<tempDt.Rows.Count; i++)
            {
                try
                {
                    contractNumber = tempDt.Rows[i]["Column19"].ObjToString();
                    if (String.IsNullOrWhiteSpace(contractNumber))
                        continue;
                    contractNumber = contractNumber.Replace(".", "");
                    contractNumber = contractNumber.Replace(",", "");
                    if (String.IsNullOrWhiteSpace(contractNumber))
                        continue;

                    name = tempDt.Rows[i]["Column17"].ObjToString();
                    if (String.IsNullOrWhiteSpace(name))
                        continue;
                    sandraMoney = tempDt.Rows[i]["Column18"].ObjToDouble();
                    sandraMoney = Math.Abs(sandraMoney);
                    dRow = trustDt.NewRow();
                    dRow["Trust"] = contractNumber;
                    dRow["Name"] = name;
                    dRow["Sandra Money"] = sandraMoney;

                    sandraTotal += sandraMoney;

                    dValue = 0D;

                    dRows = dt.Select("`Unity Trust Funeral`='" + contractNumber + "'");
                    if (dRows != null)
                    {
                        if (dRows.Length > 0)
                        {
                            dValue = dRows[0]["unity trust"].ObjToDouble();
                            dValue = Math.Abs(dValue);
                            dRow["Trust Money"] = dValue;
                            dRow["Funeral Number"] = contractNumber;
                            dRow["Date Received"] = dRows[0]["Unity Trust Date"].ObjToDateTime().ToString("MM/dd/yyyy");
                            trustTotal += dValue;
                        }
                        else
                        {
                            dRow["Trust Money"] = sandraMoney;
                            dValue = sandraMoney;
                        }
                    }
                    difference = sandraMoney - dValue;
                    difference = G1.RoundValue(difference);
                    dRow["Difference"] = difference;

                    trustDt.Rows.Add(dRow);
                }
                catch ( Exception ex)
                {
                }
            }

            string funeral = "";
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                try
                {
                    funeral = dt.Rows[i]["unity funeral"].ObjToString();
                    if (String.IsNullOrWhiteSpace(funeral))
                        continue;
                    dRows = trustDt.Select("`Funeral Number`='" + funeral + "'");
                    if (dRows.Length <= 0)
                    {
                        dRow = trustDt.NewRow();
                        dRow["Trust"] = dt.Rows[i]["unity trust contract"].ObjToString();
                        dValue = dt.Rows[i]["unity trust"].ObjToDouble();
                        dRow["Trust Money"] = Math.Abs(dValue);
                        dRow["Date Received"] = dt.Rows[i]["Unity Trust Date"].ObjToDateTime().ToString("MM/dd/yyyy");
                        dRow["Funeral Number"] = funeral;
                        dRow["Name"] = dt.Rows[i]["unity desc"].ObjToString();
                        dRow["Difference"] = Math.Abs(dValue);
                        trustDt.Rows.Add(dRow);

                        trustTotal += dValue;
                    }
                }
                catch ( Exception ex)
                {
                }
            }

            //dRow = trustDt.NewRow();
            //trustDt.Rows.Add(dRow);

            //dRow = trustDt.NewRow();
            //dRow["Trust"] = "Totals";
            //dRow["Name"] = "Totals";
            //dRow["Sandra Money"] = sandraTotal;
            //dRow["Trust Money"] = trustTotal;
            //dRow["Difference"] = sandraTotal - trustTotal;
            //trustDt.Rows.Add(dRow);

            ViewDataTable viewForm = new ViewDataTable(trustDt, "Trust,Date Received,Funeral Number, Name, Sandra Money, Trust Money, Difference", "Sandra Money, Trust Money, Difference");
            {
                viewForm.Text = this.Text;
                viewForm.ManualDone += ViewForm_ManualDone;
                //viewForm.TopMost = true;
                viewForm.Show();
            }

        }
        /****************************************************************************************/
        private void ViewForm_ManualDone(DataTable dd, DataRow dx)
        {
            string serviceId = dx["Funeral Number"].ObjToString();
            if (String.IsNullOrWhiteSpace(serviceId))
                return;
            string cmd = "select * from fcust_extended where `serviceId` = '" + serviceId + "';";
            DataTable ddx = G1.get_db_data(cmd);
            if (ddx.Rows.Count <= 0)
                return;
            string contractNumber = ddx.Rows[0]["contractNumber"].ObjToString();

            this.Cursor = Cursors.WaitCursor;
            using (FunPayments editFunPayments = new FunPayments(null, contractNumber, "", false, true))
            {
                editFunPayments.TopMost = true;
                editFunPayments.ShowDialog();
            }
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
    }
}