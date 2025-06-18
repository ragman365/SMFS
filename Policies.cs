using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

using GeneralLib;
using MySql.Data.MySqlClient;
using MySql.Data.Types;

using DevExpress.XtraPrinting;
using DevExpress.Utils;
using DevExpress.XtraGrid.Views.BandedGrid;
using System.Drawing.Drawing2D;
using DevExpress.Utils.Drawing;
using DevExpress.XtraGrid.Views.Base;
using Excel = Microsoft.Office.Interop.Excel;
using DevExpress.XtraGrid;
using DevExpress.Data;
using DevExpress.XtraGrid.Views.Grid;
using System.Linq;
using DocumentFormat.OpenXml.Bibliography;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class Policies : DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        private string workContractNumber = "";
        private string workPayer = "";
        private bool selecting = false;
        private bool loading = true;
        private int saveRow = -1;
        private bool loadAll = false;
        private DataTable originalDt = null;
        private bool workJustViewing = false;
        private bool foundLocalPreference = false;
        public static DataTable PoliciesActiveDt = null;
        /***********************************************************************************************/
        public Policies()
        {
            loadAll = true;
            InitializeComponent();
        }
        /***********************************************************************************************/
        public Policies(bool select = false)
        {
            selecting = select;
            InitializeComponent();

            SetupTotalsSummary();
        }
        /***********************************************************************************************/
        public Policies( string contractNumber, bool justViewing = false)
        {
            workContractNumber = contractNumber;
            workJustViewing = justViewing;
            InitializeComponent();
        }
        /****************************************************************************************/
        private void SetupTotalsSummary()
        {
            AddSummaryColumn("liability", null);
            AddSummaryColumn("premium", null);

            this.gridMain.GroupSummary.AddRange(new DevExpress.XtraGrid.GridSummaryItem[]
            {
            new DevExpress.XtraGrid.GridGroupSummaryItem(DevExpress.Data.SummaryItemType.Sum, "liability38", this.bandedGridColumn6, "{0:0,0.00}"),
            new DevExpress.XtraGrid.GridGroupSummaryItem(DevExpress.Data.SummaryItemType.Sum, "premium33", this.bandedGridColumn7, "{0:0,0.00}")
            });

            gridMain.OptionsView.ShowFooter = true;
        }
        /***********************************************************************************************/
        private void LoadData()
        {
            loading = true;
            if (!loadAll)
            {
                cmbType.Hide();
                chkClassAreport.Hide();
                this.dateTimePicker1.Hide();
                this.dateTimePicker2.Hide();
                btnLeft.Hide();
                btnRight.Hide();
            }
            else
            {
                btnLapseAll.Hide();
                gridMain.OptionsView.ColumnAutoWidth = false;
            }

            G1.loadGroupCombo(cmbSelectColumns, "Policies", "Primary");
            if ( workJustViewing )
                cmbSelectColumns.Text = "Policy Summary";

            this.Cursor = Cursors.WaitCursor;

            string payerFname = "";
            string payerLname = "";
            string report = "";

            //gridMain.Columns["premium"].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Custom;
            //gridMain.Columns["premium"].SummaryItem.DisplayFormat = "{0:C2}";

            AddSummaryColumn("premium", gridMain);
            AddSummaryColumn("liability", gridMain);
            AddSummaryColumn("historicPremium", gridMain);
            AddSummaryColumn("persons", gridMain, "{0:0,0}");
            AddSummaryColumn("policies", gridMain, "{0:0,0}");
            //gridMain.Columns["liability"].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Custom;
            //gridMain.Columns["liability"].SummaryItem.DisplayFormat = "{0:C2}";

            //gridMain.Columns["historicPremium"].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Custom;
            //gridMain.Columns["historicPremium"].SummaryItem.DisplayFormat = "{0:C2}";

            string what = cmbType.Text.Trim().ToUpper();
            string payerCode = txtPayer.Text.Trim();

            DateTime date = this.dateTimePicker1.Value;
            string date1 = date.ToString("yyyy-MM-dd");

            date = this.dateTimePicker2.Value;
            string date2 = date.ToString("yyyy-MM-dd");
            DataRow[] dRows = null;
            DataRow[] dRows2 = null;
            DataTable dd = null;
            TimeSpan ts;
            DateTime lDate = DateTime.Now;

            string cmd = "Select * from `icustomers` p JOIN `icontracts` x ON p.`contractNumber` = x.`contractNumber` ";
            if (!String.IsNullOrWhiteSpace(workContractNumber))
                cmd += " WHERE p.`contractNumber` = '" + workContractNumber + "' ORDER BY p.`contractNumber` DESC ";
            else
            {
                cmd += " JOIN `policies` q ON p.`contractNumber` = q.`contractNumber` ";
                cmd += " LEFT JOIN `payers` c ON q.`payer` = c.`payer` ";
                if (loadAll)
                {
                    if (what == "DECEASED")
                    {
                        //cmd += " WHERE ( q.`deceasedDate` > '1800-01-01' OR c.`deceasedDate` > '1800-01-01' ) ";
                        cmd += " WHERE ( q.`deceasedDate` > '" + date1 + "' AND q.`deceasedDate` <= '" + date2 + "' ) ";
                        if (chkClassAreport.Checked)
                        {
                            cmd += " AND q.`report` = 'Not Third Party' ";
                            cmd += " AND ( q.`liability` >= '0.00' AND q.`liability` <= '450.00' ) ";
                        }
                    }
                    else if (what == "ACTIVE")
                    {
                        cmd += " WHERE q.`deceasedDate` <= '0001-01-01' AND q.`lapsed` <> 'Y' ";
                        if (chkClassAreport.Checked)
                        {
                            cmd = "Select * from `icustomers` p JOIN `icontracts` x ON p.`contractNumber` = x.`contractNumber` ";
                            cmd += " JOIN `policies` q ON p.`contractNumber` = q.`contractNumber` ";
                            cmd += " LEFT JOIN `payers` c ON q.`payer` = c.`payer` ";
                            cmd += " WHERE q.`deceasedDate` <= '0001-01-01' AND q.`lapsed` <> 'Y' ";
                            cmd += " AND q.`report` = 'Not Third Party' ";
                            cmd += " AND c.`dueDate8` >= '" + date1 + "' ";
                            cmd += " AND c.`lapsed` <> 'Y' ";
                            cmd += " AND q.`lapsedDate8` <= '0100-01-01' ";
                            cmd += " AND ( q.`liability` >= '0.00' AND q.`liability` <= '450.00' ) ";

                            //cmd += " AND q.`report` = 'Not Third Party' ";
                            //cmd += " AND c.`dueDate8` >= '" + date1 + "' ";
                            //cmd += " AND c.`lapsed` <> 'Y' ";
                            //cmd += " AND q.`lapsedDate8` <= '0100-01-01' ";
                            //cmd += " AND ( q.`liability` >= '0.00' AND q.`liability` <= '450.00' ) ";
                        }
                    }
                    else if (what == "LAPSED")
                    {
                        if (chkClassAreport.Checked)
                        {
                            cmd = "Select * from `icustomers` p JOIN `icontracts` x ON p.`contractNumber` = x.`contractNumber` ";
                            cmd += " JOIN `policies` q ON p.`contractNumber` = q.`contractNumber` ";
                            cmd += " LEFT JOIN `payers` c ON q.`payer` = c.`payer` ";
                            cmd += " WHERE q.`report` = 'Not Third Party' ";
                            cmd += " AND c.`dueDate8` >= '" + date1 + "' ";
                            cmd += " AND ( c.`deceasedDate` < '1000-01-01' AND p.`deceasedDate` < '1000-01-01' )";
                            cmd += " AND ( c.`lapsed` = 'Y' OR q.`lapsedDate8` >= '" + date1 + "' ) ";
                            cmd += " AND ( q.`liability` >= '0.00' AND q.`liability` <= '450.00' ) ";
                        }
                        else
                        {
                            cmd += " WHERE(  c.`lapsed` = 'Y' OR ( q.`lapsed` = 'Y' OR q.`lapsedDate8` > '1999-01-01' ) )";
                            cmd += " AND ( c.`deceasedDate` < '1000-01-01' AND q.`deceasedDate` < '1000-01-01' ) ";
                            cmd += " AND ( c.`lapsed` = 'Y' OR q.`lapsed` = 'Y' ) ";
                        }
                    }

                    if (!String.IsNullOrWhiteSpace(payerCode))
                    {
                        cmd = "Select * from `icustomers` p JOIN `icontracts` x ON p.`contractNumber` = x.`contractNumber` ";
                        cmd += " JOIN `policies` q ON p.`contractNumber` = q.`contractNumber` ";
                        cmd += " LEFT JOIN `payers` c ON q.`payer` = c.`payer` ";
                        cmd += " WHERE q.`payer` = '" + payerCode + "' ";
                    }
                    //cmd += " AND q.`payer` = '150041' ";
                }
                cmd += " ORDER by q.`payer`, c.`contractNumber` asc ";
            }
            cmd += ";";

            DataTable dt = G1.get_db_data(cmd);
            if ( dt.Rows.Count <= 0 )
            {
                CustomerDetails clientForm = new CustomerDetails(workContractNumber);
                clientForm.Show();
                return;
            }

            DataRow[] dddRow = dt.Select("payer='VI01318'");
            if (dddRow.Length > 0)
            {
                DataTable dddd = dddRow.CopyToDataTable();
            }

            //Trust85.FindContract(dt, "ZZ000");

            if ( dt.Rows.Count > 0 && !loadAll )
            {
                payerFname = dt.Rows[0]["firstName"].ObjToString();
                payerLname = dt.Rows[0]["lastName"].ObjToString();
                string payer = dt.Rows[0]["payer"].ObjToString();
                if (!String.IsNullOrWhiteSpace(payer))
                {
                    workPayer = payer;
                    if (!String.IsNullOrWhiteSpace(workContractNumber))
                    {
                        cmd = "Select * from `policies` p JOIN `icustomers` d ON p.`contractNumber` = d.`contractNumber` JOIN `icontracts` x ON p.`contractNumber` = x.`contractNumber` ";
                        if (!String.IsNullOrWhiteSpace(workContractNumber))
                        {
                            //cmd += " WHERE p.`payer` = '" + payer + "' ORDER BY p.`contractNumber` DESC ";
                            cmd += " WHERE p.`contractNumber` = '" + workContractNumber + "' ORDER by p.`contractNumber` DESC;";
                        }
                        dt = G1.get_db_data(cmd);

                        if ( chkHonor.Checked )
                        {
                            DataTable testDt = CustomerDetails.filterSecNat(chkSecNat.Checked, dt);
                            dt = testDt.Copy();
                            if (chk3rdParty.Checked)
                            {
                                for (int i = dt.Rows.Count - 1; i >= 0; i--)
                                {
                                    report = dt.Rows[i]["report"].ObjToString().ToUpper();
                                    if (report == "NOT THIRD PARTY" || String.IsNullOrWhiteSpace ( report))
                                        dt.Rows.RemoveAt(i);
                                }
                            }
                            else if ( !chkSecNat.Checked )
                            {
                                if (DateTime.Now > DailyHistory.kill3rdPartyDate)
                                {
                                    for (int i = dt.Rows.Count - 1; i >= 0; i--)
                                    {
                                        report = dt.Rows[i]["report"].ObjToString().ToUpper();
                                        if (report != "NOT THIRD PARTY")
                                            dt.Rows.RemoveAt(i);
                                    }
                                }
                            }
                        }

                        dt.Columns.Add("ddate");
                        dt.Columns.Add("duedate");
                        date = DateTime.Now;
                        for ( int i=0; i<dt.Rows.Count; i++)
                        {
                            dt.Rows[i]["firstName"] = payerFname;
                            dt.Rows[i]["lastName"] = payerLname;
                            date = dt.Rows[i]["deceasedDate"].ObjToDateTime();
                            dt.Rows[i]["ddate"] = date.ToString("MM/dd/yyyy");
                            date = dt.Rows[i]["dueDate8"].ObjToDateTime();
                            dt.Rows[i]["dueDate"] = date.ToString("MM/dd/yyyy");
                        }
                    }
                }
                else
                {
                    CustomerDetails clientForm = new CustomerDetails(workContractNumber);
                    clientForm.Show();
                    return;
                }
            }
            else
            {
                if (chkClassAreport.Checked)
                {
                    if (G1.get_column_number(dt, "ddate") < 0)
                        dt.Columns.Add("ddate");
                    if (G1.get_column_number(dt, "duedate") < 0)
                        dt.Columns.Add("duedate");

                    CustomerDetails.FixOrphanPolicies2(dt);
                    dt = FilterInactive(dt);
                }

                if ( G1.get_column_number ( dt, "ddate") < 0 )
                    dt.Columns.Add("ddate");
                if (G1.get_column_number(dt, "duedate") < 0)
                    dt.Columns.Add("duedate");

                dddRow = dt.Select("payer='100004'");
                if (dddRow.Length > 0)
                {
                    DataTable dddd = dddRow.CopyToDataTable();
                }

                date = DateTime.Now;
                DateTime payerDueDate = DateTime.Now;
                DateTime dolp = DateTime.Now;
                DateTime payerDolp = DateTime.Now;
                string payer = "";
                string orphanContract = "";
                string contractNumber = "";
                string lapsed3 = "";

                dddRow = dt.Select("payer='100004'");
                if ( dddRow.Length > 0 )
                {
                    DataTable dddd = dddRow.CopyToDataTable();
                }

                if (loadAll && what.ToUpper() == "ACTIVE")
                {
                    //CustomerDetails.FixOrphanPolicies2(dt);
                }
                else if (what.ToUpper() == "LAPSED")
                {
                    dt.Columns.Add("remove");
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                        if (contractNumber == "ZZ0000075")
                        {
                        }
                        payer = dt.Rows[i]["payer"].ObjToString();
                        if (payer == "BB-0392")
                        {
                        }
                        date = dt.Rows[i]["dueDate8"].ObjToDateTime();
                        payerDueDate = dt.Rows[i]["dueDate81"].ObjToDateTime();
                        if (payerDueDate >= date)
                        {
                            date = payerDueDate;
                            dt.Rows[i]["dueDate8"] = G1.DTtoMySQLDT(date.ToString("yyyy-MM-dd"));
                            dolp = dt.Rows[i]["lastDatePaid8"].ObjToDateTime();
                            payerDolp = dt.Rows[i]["lastDatePaid81"].ObjToDateTime();
                            if (payerDolp > dolp)
                                dt.Rows[i]["lastDatePaid8"] = G1.DTtoMySQLDT(payerDolp.ToString("yyyy-MM-dd"));
                        }
                        dt.Rows[i]["dueDate"] = date.ToString("yyyy-MM-dd");
                        date = dt.Rows[i]["deceasedDate2"].ObjToDateTime();
                        dt.Rows[i]["ddate"] = date.ToString("yyyy-MM-dd");


                        date = dt.Rows[i]["deceasedDate3"].ObjToDateTime();
                        if (date.Year > 1000)
                            dt.Rows[i]["remove"] = "1";
                        else
                        {
                            date = dt.Rows[i]["deceasedDate2"].ObjToDateTime();
                            if (date.Year > 1000)
                                dt.Rows[i]["remove"] = "1";
                            else
                                dt.Rows[i]["remove"] = "0";
                        }

                        lapsed3 = dt.Rows[i]["lapsed3"].ObjToString().ToUpper();
                        if (lapsed3 == "Y")
                        {
                            if (dt.Rows[i]["remove"].ObjToString() != "Y")
                            {
                                date = dt.Rows[i]["deceasedDate3"].ObjToDateTime();
                                if (date.Year < 1000)
                                    dt.Rows[i]["remove"] = "0";
                            }
                        }
                    }
                    DataRow[] xRows = dt.Select("remove = '0'");
                    if (xRows.Length > 0)
                        dt = xRows.CopyToDataTable();
                }
                else // Deceased Policies
                {
                    //                    1.Payer Death: Payer Death Date falls within 2022 / No remaining active policies on the payer
                    //                          If(lapedDate < 2022) ignore, if (lapsedDate >= 2022) keep
                    //                    2.Policy Death: Policy Death Date falls within 2022 / Other Active policies remain on the payer

                    dt.Columns.Add("remove");
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        payer = dt.Rows[i]["payer"].ObjToString();
                        date = dt.Rows[i]["deceasedDate3"].ObjToDateTime(); // Payer Deceased Date
                        //if (date >= this.dateTimePicker1.Value && date <= this.dateTimePicker2.Value) // Payer is Deceased
                        //{
                        //    if (dt.Rows[i]["deceasedDate1"].ObjToDateTime().Year < 1000)
                        //        dt.Rows[i]["deceasedDate1"] = G1.DTtoMySQLDT(date);

                        //    dRows = dt.Select("payer='" + payer + "'");
                        //    if (dRows.Length > 0)
                        //    {
                        //        dd = dRows.CopyToDataTable();
                        //        for (int j = 0; j < dRows.Length; j++)
                        //        {
                        //            if (dRows[j]["deceasedDate2"].ObjToDateTime().Year > 1000)
                        //                dRows[j]["ddate"] = dRows[j]["deceasedDate2"].ObjToDateTime().ToString("yyyy-MM-dd");

                        //            if (dRows[j]["deceasedDate2"].ObjToDateTime() >= this.dateTimePicker1.Value && dRows[j]["deceasedDate2"].ObjToDateTime() <= this.dateTimePicker2.Value)
                        //                dRows[j]["remove"] = "1";
                        //            else
                        //            {
                        //                lDate = dRows[j]["lapsedDate8"].ObjToDateTime();
                        //                if (dRows[j]["lapsedDate8"].ObjToDateTime() < this.dateTimePicker1.Value)
                        //                    dRows[j]["remove"] = 0;
                        //                else if (dRows[j]["lapsedDate8"].ObjToDateTime() >= this.dateTimePicker1.Value)
                        //                {
                        //                    try
                        //                    {
                        //                        ts = lDate - date;
                        //                        if (ts.TotalDays > 190)
                        //                            dRows[j]["remove"] = 0; // Policy is more than 190 days lapsed so consider it dead;
                        //                        else
                        //                            dRows[j]["remove"] = 1; // Policy can be reinstated so do not include as a deceased policy
                        //                    }
                        //                    catch (Exception ex)
                        //                    {
                        //                    }
                        //                }
                        //            }

                        //        }
                        //    }
                        //    else
                        //    {
                        //        dt.Rows[i]["remove"] = "1";
                        //        date = dt.Rows[i]["deceasedDate2"].ObjToDateTime();
                        //        if (date.Year > 1000)
                        //            dt.Rows[i]["ddate"] = date.ToString("yyyy-MM-dd");
                        //    }
                        //}
                        //else
                        //{
                            date = dt.Rows[i]["deceasedDate2"].ObjToDateTime(); // Policy is Deceased
                            if (date >= this.dateTimePicker1.Value && date <= this.dateTimePicker2.Value) // Payer is Deceased
                            {
                                dt.Rows[i]["remove"] = "1";
                                dt.Rows[i]["ddate"] = date.ToString("yyyy-MM-dd");
                            }
                            else
                                dt.Rows[i]["remove"] = "0";
                        //}
                    }

                    DataRow[] xRows = dt.Select("remove = '1'");
                    if (xRows.Length > 0)
                        dt = xRows.CopyToDataTable();
                }
                //dddRow = dt.Select("payer='BB-7967'");
                //if (dddRow.Length > 0)
                //{
                //    DataTable dddd = dddRow.CopyToDataTable();
                //}
            }
            if (dt.Rows.Count > 0 && !loadAll)
            {
                string name = dt.Rows[0]["firstName"].ObjToString() + " " + dt.Rows[0]["lastName"].ObjToString() + " (" + dt.Rows[0]["payer"].ObjToString() + ")";
                this.Text = name;
            }
            else if ( loadAll)
            {
                this.Text = "List of All Policies";
            }

            //            DataRow[] dRow = dt.Select("contractNumber='P16050UI'");
            //            int len = dRow.Length;
            DataRow [] dddRows = dt.Select("payer='BB-4302'");
            if (dddRows.Length > 0)
            {
                DataTable dddd = dddRows.CopyToDataTable();
            }

            dt.Columns.Add("num");
            dt.Columns.Add("bDate");
            dt.Columns.Add("ssno");
            dt.Columns.Add("select");
            dt.Columns.Add("fullname");
            dt.Columns.Add("policyfullname");
            if (G1.get_column_number(dt, "ddate") < 0)
                dt.Columns.Add("ddate");
            if (G1.get_column_number(dt, "dueDate") < 0)
                dt.Columns.Add("dueDate");

            if ( !loadAll )
                FastLookup.FilterPolicies(dt);

            G1.NumberDataTable(dt);
            //FixDates(dt, "birthDate", "bDate");
            //FormatSSN(dt, "ssn", "ssno");
            SetupFullNames(dt);
            //FixDeceasedDate(dt);
            if (selecting)
                gridMain.Columns["select"].Visible = true;

            //if (!String.IsNullOrWhiteSpace(workContractNumber))
            //    SetupPayerLine(dt);
            if (G1.get_column_number(dt, "OriginalRow") < 0)
                dt.Columns.Add("OriginalRow");
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["OriginalRow"] = dt.Rows[i]["num"].ObjToString();

            //dddRows = dt.Select("payer='CC-2816'");
            //if (dddRows.Length > 0)
            //{
            //    DataTable dddd = dddRows.CopyToDataTable();
            //}

            if ( loadAll )
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i]["issueDate8"] = G1.DTtoMySQLDT(dt.Rows[i]["issueDate81"].ObjToDateTime());
                    dt.Rows[i]["dueDate8"] = G1.DTtoMySQLDT(dt.Rows[i]["dueDate81"].ObjToDateTime());
                }

                if (what.ToUpper() == "LAPSED")
                {
                    DataView tempview = dt.DefaultView;
                    tempview.Sort = "contractNumber asc, payer asc";
                    dt = tempview.ToTable();
                }
            }

            dgv.DataSource = dt;
            originalDt = dt;

            PoliciesActiveDt = dt;

            dRows = dt.Select("payer='BB-4302'");
            if (dRows.Length > 0)
            {
                DataTable ddd = dRows.CopyToDataTable();
            }

            gridBand2.Fixed = DevExpress.XtraGrid.Columns.FixedStyle.Left;
            gridMain.Columns["num"].Fixed = DevExpress.XtraGrid.Columns.FixedStyle.Left;
            gridMain.Columns["contractNumber"].Fixed = DevExpress.XtraGrid.Columns.FixedStyle.Left;
            //AddSummaryColumn("premium", gridMain);
            //AddSummaryColumn("historicPremium", gridMain);
            this.Cursor = Cursors.Default;
            loading = false;
            if (saveRow == -2)
            {
                gridMain.FocusedRowHandle = dt.Rows.Count - 1;
                gridMain.SelectRow(dt.Rows.Count-1);
                gridMain.RefreshData();
                dgv.Refresh();
            }
            else if (saveRow >= 0)
            {
                gridMain.FocusedRowHandle = saveRow;
                gridMain.SelectRow(saveRow);
                gridMain.RefreshData();
                dgv.Refresh();
            }
            saveRow = -1;
            this.panelAll.Refresh();
            this.panelTop.Refresh();
            this.Refresh();
            pictureBox1.Refresh();
            btnSelectColumns.Show();
            pictureBox1.Show();

            if (chkShowUniquePayers.Checked)
                chkShowUniquePayers_CheckedChanged(null, null);
            if (chkShowUniquePayersAndCompany.Checked)
                chkShowUniquePayersAndCompany_CheckedChanged(null, null);

            if (workJustViewing)
            {
                cmbSelectColumns_SelectedIndexChanged(cmbSelectColumns, null);

                printPreviewToolStripMenuItem_Click(null, null);
                this.Close();
            }

            DataTable dt2 = (DataTable)dgv.DataSource;
            Trust85.FindContract(PoliciesActiveDt, "ZZ0014252");
        }
        /****************************************************************************************/
        private DataTable FilterInactive(DataTable dt)
        {
            DataRow[] dRows = dt.Select("contractNumber LIKE 'OO%' OR contractNumber LIKE 'MM%'");
            if (dRows.Length > 0)
            {
                for (int i = (dRows.Length - 1); i >= 0; i--)
                    dt.Rows.Remove(dRows[i]);
            }
            return dt;
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
        /****************************************************************************************/
        private void AddSummaryColumn(string columnName, DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gMain = null, string format = "")
        {
            if (gMain == null)
                gMain = gridMain;
            if (String.IsNullOrWhiteSpace(format))
                format = "${0:0,0.00}";
            gMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            //gMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Custom;
            gMain.Columns[columnName].SummaryItem.DisplayFormat = format;
        }
        /***********************************************************************************************/
        private void SetupPayerLine ( DataTable dt )
        {
            DataRow dRow = dt.NewRow();
            dRow["contractNumber"] = workContractNumber;
            dt.Rows.InsertAt(dRow, 0);
        }
        /***********************************************************************************************/
        private void FixDeceasedDate(DataTable dt)
        {
            string date1 = "";
            string date2 = "";
            if (G1.get_column_number(dt, "deceasedDate") < 0)
                return;
            //for (int i = 0; i < dt.Rows.Count; i++)
            //{
            //    date1 = dt.Rows[i]["deceasedDate"].ObjToString();
            //    if (date1.IndexOf("0000") >= 0)
            //    {
            //        date2 = dt.Rows[i]["deceasedDate1"].ObjToString();
            //        if (date2.IndexOf("0000") < 0)
            //            dt.Rows[i]["deceasedDate"] = dt.Rows[i]["deceasedDate1"];
            //    }
            //}
        }
        /***********************************************************************************************/
        private void SetupFullNames(DataTable dt)
        {
            string fullname = "";
            string fname = "";
            string lname = "";
            string fname1 = "";
            string lname1 = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                fname = dt.Rows[i]["firstName"].ObjToString();
                lname = dt.Rows[i]["lastName"].ObjToString();
                fname1 = dt.Rows[i]["firstName1"].ObjToString();
                lname1 = dt.Rows[i]["lastName1"].ObjToString();
                fullname = fname + " " + lname;
                dt.Rows[i]["fullname"] = fullname;

                if (fname.ToUpper() != fname1.ToUpper() || lname.ToUpper() != lname1.ToUpper())
                    dt.Rows[i]["fullname"] += " ***BAD***";

                fname = dt.Rows[i]["policyFirstName"].ObjToString();
                lname = dt.Rows[i]["policyLastName"].ObjToString();
                fullname = fname + " " + lname;
                dt.Rows[i]["policyfullname"] = fullname;
            }
        }
        /***********************************************************************************************/
        private void FormatSSN(DataTable dt, string columnName, string newColumn)
        {
            string ssn = "";
            string ssno = "";
            int row = 0;
            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    row = i;
                    ssn = dt.Rows[i][columnName].ObjToString().Trim();
                    ssn = ssn.Replace("-", "");
                    ssno = ssn;
                    if (ssn.Trim().Length >= 8)
                        try { ssno = "XXX-XX-" + ssn.Substring(5, 4); }
                        catch { }
                    dt.Rows[i][newColumn] = ssno;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Fixing Date Field " + columnName + " Row= " + row + "SSN= " + ssn + " " + ex.Message.ObjToString());
            }
        }
        /***********************************************************************************************/
        private void FixDates(DataTable dt, string columnName, string newColumn)
        {
            string date = "";
            long ldate = 0L;
            int row = 0;
            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    row = i;
                    date = dt.Rows[i][columnName].ObjToString();
                    if (String.IsNullOrWhiteSpace(date))
                        continue;
                    if (date == "0000-00-00")
                    {
                        date = "";
                        dt.Rows[i][columnName] = date;
                    }
                    else
                    {
                        ldate = G1.date_to_days(date);
                        date = G1.days_to_date(ldate);
                        dt.Rows[i][newColumn] = date;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Fixing Date Field " + columnName + " Row= " + row + "Date= " + date + " " + ex.Message.ObjToString());
            }
        }
        /***********************************************************************************************/
        private void Policies_Load(object sender, EventArgs e)
        {
            DateTime date = DateTime.Now;
            DateTime newDate = new DateTime(date.Year - 1, 1, 1);
            this.dateTimePicker1.Value = newDate;
            newDate = new DateTime(newDate.Year, 12, 31);
            this.dateTimePicker2.Value = newDate;

            chkSummarize.Hide();

            if (!loadAll)
            {
                btnExport.Hide();
                gridMain.Columns["firstName1"].Visible = false;
                gridMain.Columns["lastName1"].Visible = false;
                gridMain.Columns["policyFirstName"].Visible = false;
                gridMain.Columns["policyLastName"].Visible = false;
                gridMain.Columns["payer"].Visible = false;
                gridMain.Columns["fullname"].Visible = true;
                gridMain.Columns["policyfullname"].Visible = true;
                gridMain.Columns["dueDate8"].Visible = false;
                gridMain.Columns["duedate"].Visible = true;
                gridMain.Columns["beneficiary"].Visible = false;

                chkClassAreport.Hide();
                this.dateTimePicker1.Hide();
                this.dateTimePicker2.Hide();
                btnLeft.Hide();
                btnRight.Hide();

                LoadData();
            }
            else
            {
                btnLapseAll.Hide();
                gridMain.OptionsView.ShowFooter = true;
                gridMain.Columns["balanceDue"].Visible = false;
                gridMain.Columns["nowDue"].Visible = false;
                gridMain.Columns["firstName1"].Visible = true;
                gridMain.Columns["lastName1"].Visible = true;
                gridMain.Columns["policyFirstName"].Visible = true;
                gridMain.Columns["policyLastName"].Visible = true;
                gridMain.Columns["payer"].Visible = true;
                gridMain.Columns["fullname"].Visible = false;
                gridMain.Columns["policyfullname"].Visible = false;
                gridMain.Columns["dueDate8"].Visible = true;
                gridMain.Columns["duedate"].Visible = false;


                gridMain.Columns["dueDate8"].Visible = false;
                gridMain.Columns["duedate"].Visible = true;

                gridMain.Columns["ddate"].Visible = true;
                gridMain.Columns["deceasedDate1"].Visible = true;
                gridMain.Columns["beneficiary"].Visible = true;
                gridMain.Columns["contractValue"].Visible = false;
                gridMain.Columns["percentPaid"].Visible = false;
                gridMain.Columns["paid"].Visible = false;
                gridMain.Columns["purchase"].Visible = false;
            }

            if ( G1.isField())
            {
                btnAdd.Hide();
                btnEdit.Hide();
                btnDelete.Hide();
                btnLapseAll.Hide();

                btnCalcMonthly.Hide();
                btnExport.Hide();

                chk3rdParty.Hide();
                chkFilterDeceased.Hide();
                chkFilterLapsed.Hide();
                chkHonor.Hide();
                chkInactive.Hide();
                chkSecNat.Hide();
                chkShowUniquePayers.Hide();
                chkShowUniquePayersAndCompany.Hide();

                chkFilterDeceased.Checked = true;
                chkHonor.Checked = true;

                gridMain.OptionsView.ShowFooter = false;

                SetFieldUserFormat();
            }
        }
        /****************************************************************************************/
        private void SetFieldUserFormat()
        {
            try
            {
                gridMain.Columns["contractNumber"].Visible = false;
                gridMain.Columns["beneficiary"].Visible = false;
                gridMain.Columns["report"].Visible = false;
                gridMain.Columns["companyCode"].Visible = false;
                gridMain.Columns["groupNumber"].Visible = false;
                gridMain.Columns["ssn"].Visible = false;
                gridMain.Columns["ssno"].Visible = false;
                gridMain.Columns["bDate"].Visible = false;
            }
            catch ( Exception ex)
            {
            }
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
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            G1.SpyGlass(gridMain);
            //if (gridMain.OptionsFind.AlwaysVisible == true)
            //    gridMain.OptionsFind.AlwaysVisible = false;
            //else
            //    gridMain.OptionsFind.AlwaysVisible = true;
        }
        /***********************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string contract = dr["contractNumber"].ObjToString();
            if (String.IsNullOrWhiteSpace(contract))
                return;
            string cmd = "Select * from `icontracts` where `contractNumber` = '" + contract + "';";
            DataTable dx = G1.get_db_data(cmd);
            if ( dx.Rows.Count <= 0 )
            {
                MessageBox.Show("***ERROR*** Locating Contract " + contract + "!");
                return;
            }
            string contractRecord = dx.Rows[0]["record"].ObjToString();
            this.Cursor = Cursors.WaitCursor;
            DataTable dt = (DataTable)dgv.DataSource;
            G1.UpdatePreviousCustomer(contract, LoginForm.username);
            string policyNumber = dr["policyNumber"].ObjToString();
            string policyFirstName = dr["policyFirstName"].ObjToString();
            string policyLastName = dr["policyLastName"].ObjToString();
            string policyRecord = dr["record"].ObjToString();
//            CustomerDetails clientForm = new CustomerDetails(contract, policyRecord);
            CustomerDetails clientForm = new CustomerDetails(contract);
            clientForm.Show();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadData();
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
                    int row = gridMain.GetDataSourceRowIndex(e.RowHandle);
                    DataTable dt = (DataTable)dgv.DataSource;
                    dt.Rows[row]["num"] = num;
                }
            }
            else if (e.Column.FieldName.ToUpper().IndexOf ( "DATE") >= 0 )
            {
                if (e.RowHandle >= 0)
                {
                    if (!String.IsNullOrWhiteSpace(e.DisplayText))
                    {
                        DateTime date = e.DisplayText.ObjToDateTime();
                        if (date.Year < 1850)
                            e.DisplayText = "";
                        else
                        {
                            if ( !loadAll )
                                e.DisplayText = date.ToString("MM/dd/yyyy");
                            else
                                e.DisplayText = date.ToString("yyyy-MM-dd");
                        }
                    }
                }
            }
        }
        /***********************************************************************************************/
        private void gridMain_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt == null)
                return;
            if (!chkFilterDeceased.Checked)
            {
                if (!chkFilterLapsed.Checked)
                {
                    if ( !chkInactive.Checked )
                        return;
                }
            }
            ColumnView view = sender as ColumnView;
            if (chkFilterDeceased.Checked)
            {
                DateTime deceasedDate = dt.Rows[row]["deceasedDate"].ObjToDateTime();
                if (deceasedDate.Year > 100)
                {
                    e.Visible = false;
                    e.Handled = true;
                    return;
                }
            }
            if (chkFilterLapsed.Checked)
            {
                DateTime lapsedDate = dt.Rows[row]["lapsedDate8"].ObjToDateTime();
                if (lapsedDate.Year > 100)
                {
                    e.Visible = false;
                    e.Handled = true;
                    return;
                }
                string lapsed = dt.Rows[row]["lapsed"].ObjToString();
                if (lapsed == "Y")
                {
                    e.Visible = false;
                    e.Handled = true;
                    return;
                }
                lapsed = dt.Rows[row]["lapsed3"].ObjToString();
                if (lapsed == "Y")
                {
                    e.Visible = false;
                    e.Handled = true;
                    return;
                }
            }
            if ( chkInactive.Checked )
            {
                string contractNumber = dt.Rows[row]["contractNumber"].ObjToString();
                if ( contractNumber.IndexOf ( "OO") >= 0 || contractNumber.IndexOf ( "MM") >= 0)
                {
                    e.Visible = false;
                    e.Handled = true;
                    return;
                }
            }
        }
        /***********************************************************************************************/
        private int pageMarginLeft = 0;
        private int pageMarginRight = 0;
        private int pageMarginTop = 0;
        private int pageMarginBottom = 0;
        private int printCount = 0;
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
            printableComponentLink1.CreateDetailHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateDetailHeaderArea);
            printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);
            printableComponentLink1.BeforeCreateAreas += new System.EventHandler(this.printableComponentLink1_BeforeCreateAreas);
            printableComponentLink1.AfterCreateAreas += new System.EventHandler(this.printableComponentLink1_AfterCreateAreas);
            printableComponentLink1.Landscape = true;

            Printer.setupPrinterMargins(50, 100, 110, 50);

            pageMarginLeft = Printer.pageMarginLeft;
            pageMarginRight = Printer.pageMarginRight;
            pageMarginTop = Printer.pageMarginTop;
            pageMarginBottom = Printer.pageMarginBottom;


            printableComponentLink1.Margins.Left = pageMarginLeft;
            printableComponentLink1.Margins.Right = pageMarginRight;
            printableComponentLink1.Margins.Top = pageMarginTop;
            printableComponentLink1.Margins.Bottom = pageMarginBottom;

            printingSystem1.Document.AutoFitToPagesWidth = 1;

            G1.AdjustColumnWidths(gridMain, 0.65D, true);

            printableComponentLink1.CreateDocument();
            if (workJustViewing)
                printableComponentLink1.ShowPreviewDialog();
            else
                printableComponentLink1.ShowPreview();

            G1.AdjustColumnWidths(gridMain, 0.65D, false );
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

            Printer.setupPrinterMargins(50, 100, 110, 50);

            pageMarginLeft = Printer.pageMarginLeft;
            pageMarginRight = Printer.pageMarginRight;
            pageMarginTop = Printer.pageMarginTop;
            pageMarginBottom = Printer.pageMarginBottom;

            printableComponentLink1.Margins.Left = pageMarginLeft;
            printableComponentLink1.Margins.Right = pageMarginRight;
            printableComponentLink1.Margins.Top = pageMarginTop;
            printableComponentLink1.Margins.Bottom = pageMarginBottom;

            printCount = 0;

            printableComponentLink1.CreateDocument();
            if (LoginForm.doLapseReport)
                printableComponentLink1.Print();
            else
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

            font = new Font("Ariel", 6);
            Printer.DrawGridDate(2, 3, 2, 3, Color.Black, BorderSide.None, font);
            Printer.DrawGridPage(11, 3, 2, 3, Color.Black, BorderSide.None, font);

            //            Printer.DrawQuad(1, 9, 2, 3, "User : " + LoginForm.username, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);

            font = new Font("Ariel", 10, FontStyle.Regular);
            string title = "Insurance Policies Report for " + this.Text;
            int startX = 5;
            if (loadAll)
            {
                title = cmbType.Text.Trim() + " Insurance Policies Report";
                if ( chkSummarize.Checked )
                    title = cmbType.Text.Trim() + " Class \"A\" Policies Report";

                string report = gridMain.FilterPanelText.Trim();
                if ( !String.IsNullOrWhiteSpace ( report ))
                {
                    startX = 4;
                    title += " " + report;
                }
            }
            Printer.DrawQuad(startX, 8, 9, 4, title, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);


            //            Printer.DrawQuadTicks();
            Printer.SetQuadSize(24, 12);
            font = new Font("Ariel", 7, FontStyle.Regular);
            //Printer.DrawQuad(16, 7, 5, 2, lblBalance.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Top);
            //Printer.DrawQuad(16, 10, 5, 2, lblPayment.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            font = new Font("Ariel", 8);
//            Printer.DrawQuad(1, 6, 6, 3, search, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Top);
            Printer.DrawQuad(1, 9, 6, 3, "User : " + LoginForm.username, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);


            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /***********************************************************************************************/
        private void changeContractNumberToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if ( !LoginForm.administrator )
            {
                MessageBox.Show("***ERROR*** You do not have permission to do this.");
                return;
            }
            string goodContractNumber = "";

            DataRow dr = gridMain.GetFocusedDataRow();
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            DataTable dt = (DataTable)dgv.DataSource;

            string badContractNumber = dr["contractNumber"].ObjToString();

            using (Ask askForm = new Ask("Enter Good Contract #?"))
            {
                askForm.Text = "";
                askForm.ShowDialog();
                if (askForm.DialogResult != System.Windows.Forms.DialogResult.OK)
                    return;
                goodContractNumber = askForm.Answer;
                if (String.IsNullOrWhiteSpace(goodContractNumber))
                    return;
            }

            DialogResult result = MessageBox.Show("***Warning*** Are you SURE you want to change contract number " + badContractNumber + " to " + goodContractNumber + "?", "Change Contract # Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;
            this.Cursor = Cursors.WaitCursor;
            ChangeContractNumber("policies", badContractNumber, goodContractNumber);
            ChangeContractNumber("icontracts", badContractNumber, goodContractNumber);
            ChangeContractNumber("icustomers", badContractNumber, goodContractNumber);
            ChangeContractNumber("ipayments", badContractNumber, goodContractNumber);
//            ChangeContractNumber("cust_services", badContractNumber, goodContractNumber);
            dr["contractNumber"] = goodContractNumber;
            dt.Rows[row]["contractNumber"] = goodContractNumber;
            this.Cursor = Cursors.Default;
            MessageBox.Show("***Good News*** Contracts are changes!");
        }
        /***********************************************************************************************/
        private void ChangeContractNumber ( string table, string badContractNumber, string goodContractNumber )
        {
            string record = "";
            string cmd = "Select * from `" + table + "` where `contractNumber` = '" + goodContractNumber + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                MessageBox.Show("***WARNING*** Contract " + goodContractNumber + " Already Exists in " + table + " Table!");
            }
            else
            {
                cmd = "Select * from `" + table + "` where `contractNumber` = '" + badContractNumber + "';";
                dt = G1.get_db_data(cmd);
                if (dt.Rows.Count <= 0)
                {
                    MessageBox.Show("***WARNING*** Contract " + badContractNumber + " DOES NOT Exists in " + table + " Table!");
                }
                else
                {
                    for ( int i=0; i<dt.Rows.Count; i++)
                    {
                        record = dt.Rows[i]["record"].ObjToString();
                        G1.update_db_table(table, "record", record, new string[] { "contractNumber", goodContractNumber });
                    }
                }
            }
        }
        /***********************************************************************************************/
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (loading)
                return;
            DataRow dr = gridMain.GetFocusedDataRow();
            DataTable dt = (DataTable)dgv.DataSource;
            if (e.Column.FieldName.ToUpper() == "DUEDATE")
            {
                int rowhandle = gridMain.FocusedRowHandle;
                int row = gridMain.GetDataSourceRowIndex(rowhandle);
                string cnum = dt.Rows[row]["contractNumber"].ObjToString();
                if ( !String.IsNullOrWhiteSpace (cnum))
                {
                    string date = e.Value.ObjToString();
                    date = dr["dueDate"].ObjToString();
                    if (G1.validate_date(date))
                    {
                        MySqlDateTime myDate = (MySqlDateTime)G1.DTtoMySQLDT(date);
                        string dueDate = myDate.Month.ToString("D2") + "/" + myDate.Day.ToString("D2") + "/" + myDate.Year.ToString("D4");
                        string record = dt.Rows[row]["record1"].ObjToString();
                        G1.update_db_table("icontracts", "record", record, new string[] {"dueDate8", dueDate });
                        BandedGridView view = sender as BandedGridView;
                        loading = true;
                        view.SetRowCellValue(e.RowHandle, view.Columns["dueDate"], dueDate);
                        dr["dueDate"] = myDate;
                        loading = false;
                        gridMain.RefreshData();
                        this.Refresh();
                    }
                }
            }
            else if (e.Column.FieldName.ToUpper() == "DDATE")
            {
                int rowhandle = gridMain.FocusedRowHandle;
                int row = gridMain.GetDataSourceRowIndex(rowhandle);
                string cnum = dt.Rows[row]["contractNumber"].ObjToString();
                if (!String.IsNullOrWhiteSpace(cnum))
                {
                    string date = e.Value.ObjToString();
                    date = dr["ddate"].ObjToString();
                    if (G1.validate_date(date))
                    {
                        MySqlDateTime myDate = (MySqlDateTime)G1.DTtoMySQLDT(date);
                        string deceasedDate = myDate.Month.ToString("D2") + "/" + myDate.Day.ToString("D2") + "/" + myDate.Year.ToString("D4");
                        string record = dt.Rows[row]["record"].ObjToString();
                        G1.update_db_table("policies", "record", record, new string[] { "deceasedDate", deceasedDate });
                        BandedGridView view = sender as BandedGridView;
                        loading = true;
//                        view.SetRowCellValue(e.RowHandle, view.Columns["deceasedDate"], deceasedDate);
                        loading = false;
                    }
                }
            }
        }
        /***********************************************************************************************/
        private void SetupSelectedColumns()
        {
            string group = cmbSelectColumns.Text.Trim().ToUpper();
            if (group.Trim().Length == 0)
                return;
            string cmd = "Select * from procfiles where name = '" + group + "' and ProcType = 'Polocies' order by seq";
            DataTable dt = G1.get_db_data(cmd);
            for (int i = 0; i < gridMain.Columns.Count; i++)
                gridMain.Columns[i].Visible = false;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string name = dt.Rows[i]["Description"].ToString();
                int index = dt.Rows[i]["seq"].ObjToInt32();
                try
                {
                    gridMain.Columns[name].Visible = true;
                }
                catch
                {
                }
            }
        }
        /***********************************************************************************************/
        private void SetupSelectedColumns(string procType, string group, DevExpress.XtraGrid.GridControl dgv)
        {
            if (String.IsNullOrWhiteSpace(group))
                return;
            if (String.IsNullOrWhiteSpace(procType))
                procType = "Customers";

            DataTable ddx = (DataTable)dgv.DataSource;

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
                    if (G1.get_column_number(ddx, name) >= 0)
                    {
                        if (name.ToUpper() == "LAPSED")
                            name = "lapsed2";
                        gridMain.Columns[name].Visible = true;
                    }
                }
                catch ( Exception ex)
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
                SetupSelectedColumns("Policies", comboName, dgv);
                string name = "Policies " + comboName;
                foundLocalPreference = G1.RestoreGridLayout(this, this.dgv, gridMain, LoginForm.username, name, ref skinName);
                //SetupTotalsSummary();
                //gridMain.OptionsView.ShowFooter = showFooters;
            }
            else
            {
                SetupSelectedColumns("Policies", "Primary", dgv);
                string name = "Policies Primary";
                foundLocalPreference = G1.RestoreGridLayout(this, this.dgv, gridMain, LoginForm.username, name, ref skinName);
                //gridMain.OptionsView.ShowFooter = showFooters;
                //SetupTotalsSummary();
            }

            //CleanupFieldColumns();

        }
        /***********************************************************************************************/
        private void cmbSelectColumns_SelectedIndexChangedx(object sender, EventArgs e)
        {
            ComboBox combo = (ComboBox)sender;
            string comboName = combo.Text;
            if ( !String.IsNullOrWhiteSpace ( comboName))
                SetupSelectedColumns("Policies", comboName, dgv);
            else
                SetupSelectedColumns("Policies", "Primary", dgv);
        }
        /***********************************************************************************************/
        private void btnSelectColumns_Click(object sender, EventArgs e)
        {
            string actualName = cmbSelectColumns.Text;
            SelectColumns sform = new SelectColumns(dgv, "Policies", "Primary", actualName);
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
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        { // Set as Lapsed // Policies have lapsedDate8 and icontracts have lapseDate8
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            DataTable dt = (DataTable)dgv.DataSource;

            string contractNumber = dr["contractNumber"].ObjToString();
            string policyNumber = dr["policyNumber"].ObjToString();

            string contract = dr["contractNumber"].ObjToString();
            string payer = dr["payer"].ObjToString();
            string name = dr["policyfullname"].ObjToString();
            DialogResult result = MessageBox.Show("Are you sure you want to MARK Insurance Customer (" + name + ")  as Lapsed???", "Mark Lapse Accounts Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
            if (result == DialogResult.No)
                return;
            dt.Rows[row]["lapsed"] = "Y";
            string record = "";
            DateTime today = DateTime.Now;
            string lapseDate = today.ToString("yyyy-MM-dd");
            record = dr["record"].ObjToString();
            G1.update_db_table("policies", "record", record, new string[] { "lapsed", "Y", "lapsedDate8", lapseDate });
            G1.AddToAudit(LoginForm.username, "PastDue", "Lapse", "Set", name);

            bool allLapsed = true;
            string contracts = "";
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                if ( dt.Rows[i]["lapsed"].ObjToString().ToUpper() != "Y")
                    allLapsed = false;
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                if (!contracts.Contains(contractNumber))
                    contracts += contractNumber + ",";
            }

            SetAllLapsedOrNot(allLapsed, contracts);

            LoadData();
        }
        /***********************************************************************************************/
        private void SetAllLapsedOrNot ( bool allLapsed, string contracts)
        {
            string cmd = "";
            string record = "";
            string contractNumber = "";
            DataTable dt = null;
            DateTime today = DateTime.Now;
            string lapseDate = today.ToString("yyyy-MM-dd");

            contracts = contracts.TrimEnd(',');
            string[] Lines = contracts.Split(',');
            for (int i = 0; i < Lines.Length; i++)
            {
                contractNumber = Lines[i].Trim();
                cmd = "Select * from `icontracts` where `contractNumber` = '" + contractNumber + "';";
                dt = G1.get_db_data(cmd);
                if (dt.Rows.Count > 0)
                {
                    record = dt.Rows[0]["record"].ObjToString();
                    if ( allLapsed )
                        G1.update_db_table("icontracts", "record", record, new string[] { "lapsed", "Y", "lapseDate8", lapseDate });
                    else
                        G1.update_db_table("icontracts", "record", record, new string[] { "lapsed", "", "lapseDate8", "" });
                }
                cmd = "Select * from `icustomers` where `contractNumber` = '" + contractNumber + "';";
                dt = G1.get_db_data(cmd);
                if (dt.Rows.Count > 0)
                {
                    record = dt.Rows[0]["record"].ObjToString();
                    if ( allLapsed )
                        G1.update_db_table("icustomers", "record", record, new string[] { "lapsed", "Y" });
                    else
                        G1.update_db_table("icustomers", "record", record, new string[] { "lapsed", "" });
                }
            }
        }
        /***********************************************************************************************/
        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        { // Reinstate
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            string contract = dr["contractNumber"].ObjToString();
            string payer = dr["payer"].ObjToString();
            string name = dr["policyfullname"].ObjToString();
            DialogResult result = MessageBox.Show("Are you sure you want to Reinstate Insurance Policy (" + name + ")???", "Reinstate Accounts Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
            if (result == DialogResult.No)
                return;
            string record = "";
            DateTime today = DateTime.Now;

            string lapseDate = "0000-00-00";
            record = dr["record"].ObjToString();
            G1.update_db_table("policies", "record", record, new string[] { "lapsed", "", "lapsedDate8", lapseDate });
            G1.AddToAudit(LoginForm.username, "PastDue", "Reinstate", "ReSet", name);
            LoadData();
            this.Cursor = Cursors.WaitCursor;
            ReinstateReport reportForm = new ReinstateReport(contract);
            reportForm.Show();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        { // Clear Lapsed
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            string contract = dr["contractNumber"].ObjToString();
            string payer = dr["payer"].ObjToString();
            string name = dr["policyfullname"].ObjToString();
            DialogResult result = MessageBox.Show("Are you sure you want to CLEAR Insurance Customer (" + name + ")  Lapsed???", "Clear Lapse Accounts Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
            if (result == DialogResult.No)
                return;
            string record = "";
            DateTime today = DateTime.Now;
            string lapseDate = "0000-00-00";
            record = dr["record"].ObjToString();
            G1.update_db_table("policies", "record", record, new string[] { "lapsed", "", "lapsedDate8", lapseDate });
            G1.AddToAudit(LoginForm.username, "PastDue", "Clear Lapse", "ReSet", name);

            DataTable dt = (DataTable)dgv.DataSource;
            dt.Rows[row]["lapsed"] = "";
            bool allLapsed = true;
            string contracts = "";
            string contractNumber = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (dt.Rows[i]["lapsed"].ObjToString().ToUpper() != "Y")
                    allLapsed = false;
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                if (!contracts.Contains(contractNumber))
                    contracts += contractNumber + ",";
            }

            SetAllLapsedOrNot(allLapsed, contracts);

            LoadData();
            this.Cursor = Cursors.WaitCursor;
            ReinstateReport reportForm = new ReinstateReport(contract, true );
            reportForm.Show();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        { // Recalc Premium on Active Only
            DataRow dr = gridMain.GetFocusedDataRow();
            int row = gridMain.FocusedRowHandle;
            row = gridMain.GetDataSourceRowIndex(row);
            string contract = dr["contractNumber"].ObjToString();
            string payer = dr["payer"].ObjToString();
            string name = dr["policyfullname"].ObjToString();

            DialogResult result = MessageBox.Show("Are you sure you want to RECALCULATE the Total Premium for (" + name + ")???", "Recalculate Premium Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
            if (result == DialogResult.No)
                return;

            DataTable dt = (DataTable)dgv.DataSource;
            DateTime deceasedDate = DateTime.Now;
            DateTime lapseDate8 = DateTime.Now;
            double premium = 0D;
            double totalPremium = 0D;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                deceasedDate = dt.Rows[i]["deceasedDate"].ObjToDateTime();
                if (deceasedDate.Year > 1800)
                    continue;
                lapseDate8 = dt.Rows[i]["lapsedDate8"].ObjToDateTime();
                if (lapseDate8.Year > 1800)
                    continue;
                premium = dt.Rows[i]["premium"].ObjToDouble();
                totalPremium += premium;
            }
            UpdatePolicyPremium(totalPremium);
            G1.AddToAudit(LoginForm.username, "Policies", "Recalc Total Premium", "ReCalc", this.Text);
            string str = G1.ReformatMoney(totalPremium);
            result = MessageBox.Show("***Information*** Insurance Premium Set to " + str, "Reset Premium Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        /***********************************************************************************************/
        private bool UpdatePolicyPremium(double premium)
        {
            string contract = workContractNumber;
            string cmd = "Select * from `icontracts` where `contractNumber` = '" + contract + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return false;
            string record = dt.Rows[0]["record"].ObjToString();
            double balanceDue = premium;
            double nowDue = premium;
            G1.update_db_table("icontracts", "record", record, new string[] { "amtOfMonthlyPayt", premium.ToString(), "balanceDue", balanceDue.ToString(), "nowDue", nowDue.ToString() } );
            return true;
        }
        /***********************************************************************************************/
        public static double CalcMonthlyPremium ( string contractNumber, string payer, double amtOfMonthlyPayment )
        {
            if (!DailyHistory.isInsurance(contractNumber))
                return amtOfMonthlyPayment;
            if (amtOfMonthlyPayment < 500D)
                return amtOfMonthlyPayment;
            if (!String.IsNullOrWhiteSpace(payer))
                return CalcMonthlyPremium(payer, DateTime.Now );
            else if ( !String.IsNullOrWhiteSpace ( contractNumber))
            {
                string cmd = "Select * from `icustomers` where `contractNumber` = '" + contractNumber + "';";
                DataTable dt = G1.get_db_data(cmd);
                if (dt.Rows.Count <= 0)
                    return amtOfMonthlyPayment;
                payer = dt.Rows[0]["payer"].ObjToString();
                if (!String.IsNullOrWhiteSpace(payer))
                    amtOfMonthlyPayment = CalcMonthlyPremium(payer);
            }
            return amtOfMonthlyPayment;
        }
        /***********************************************************************************************/
        public static double CalcMonthlyPremium(string payer, DateTime date )
        {
            double monthlyPremium = 0D;
            double historicPremium = 0D;
            double monthlySecNat = 0D;
            double monthly3rdParty = 0D;
            if ( DailyHistory.isInsurance ( payer))
            {
                string cmd = "Select * from `icustomers` where `contractNumber` = '" + payer + "';";
                DataTable dx = G1.get_db_data(cmd);
                if ( dx.Rows.Count > 0 )
                    payer = dx.Rows[0]["payer"].ObjToString();
            }
            CustomerDetails.CalcMonthlyPremium(payer, ref monthlyPremium, ref historicPremium, ref monthlySecNat, ref monthly3rdParty );
            if (date <= DailyHistory.killSecNatDate )
                return monthlyPremium;
            monthlyPremium = monthlyPremium - monthlySecNat;
            monthlyPremium = G1.RoundValue(monthlyPremium);
            if (date <= DailyHistory.kill3rdPartyDate)
                return monthlyPremium;
            monthlyPremium = monthlyPremium - monthly3rdParty;
            monthlyPremium = G1.RoundValue(monthlyPremium);
            return monthlyPremium;
        }
        /***********************************************************************************************/
        public static double CalcMonthlyPremium(string payer )
        {
            double monthlyPremium = 0D;
            double historicPremium = 0D;
            double monthlySecNat = 0D;
            double monthly3rdParty = 0D;
            CustomerDetails.CalcMonthlyPremium(payer, ref monthlyPremium, ref historicPremium, ref monthlySecNat, ref monthly3rdParty );

            //string cmd = "Select * from `policies` where `payer` = '" + payer + "';";
            //DataTable dt = G1.get_db_data(cmd);
            //if (dt.Rows.Count <= 0)
            //    return monthlyPremium;

            //DateTime deceasedDate = DateTime.Now;
            //DateTime lapseDate8 = DateTime.Now;
            //double premium = 0D;
            //string lapsed = "";

            //bool gotPremium = false;
            //if (G1.get_column_number(dt, "myPremium") >= 0)
            //    gotPremium = true;
            //bool doit = true;

            //for (int i = 0; i < dt.Rows.Count; i++)
            //{
            //    doit = true;
            //    deceasedDate = dt.Rows[i]["deceasedDate"].ObjToDateTime();
            //    if (deceasedDate.Year > 1800)
            //        doit = false;
            //    lapseDate8 = dt.Rows[i]["lapsedDate8"].ObjToDateTime();
            //    if (lapseDate8.Year > 1800)
            //        doit = false;

            //    lapsed = dt.Rows[i]["lapsed"].ObjToString().ToUpper();
            //    if (lapsed == "Y")
            //        doit = false;

            //    if (gotPremium && !doit)
            //    {
            //        //dt.Rows[i]["myPremium"] = 0D;
            //        continue;
            //    }
            //    else if (!doit)
            //        continue;
            //    premium = dt.Rows[i]["premium"].ObjToDouble();
            //    monthlyPremium += premium;
            //}
            //monthlyPremium = G1.RoundDown(monthlyPremium);
            return monthlyPremium;
        }
        /***********************************************************************************************/
        public static double CalcAnnualPremium(string payer, DateTime payDate )
        {
            double annualPremium = 0D;
            string cmd = "Select * from `policies` where `payer` = '" + payer + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return annualPremium;

            DateTime deceasedDate = DateTime.Now;
            DateTime lapseDate8 = DateTime.Now;
            double premium = 0D;
            double totalPremium = 0D;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                deceasedDate = dt.Rows[i]["deceasedDate"].ObjToDateTime();
                if (deceasedDate.Year > 1800)
                {
                    if ( deceasedDate < payDate )
                        continue;
                }
                lapseDate8 = dt.Rows[i]["lapsedDate8"].ObjToDateTime();
                if (lapseDate8.Year > 1800)
                    continue;
                premium = dt.Rows[i]["premium"].ObjToDouble();
                premium = premium * 12D;
                premium = G1.RoundValue(premium);
                premium = premium * 0.95D;
                premium = G1.RoundDown(premium);
                totalPremium += premium;
            }
            annualPremium = totalPremium;
            //annualPremium = G1.RoundDown(annualPremium);
            annualPremium = G1.RoundValue(annualPremium);
            return annualPremium;
        }
        /***********************************************************************************************/
        public static double CalcAnnualPremium(string payer)
        {
            double annualPremium = 0D;
            string cmd = "Select * from `policies` where `payer` = '" + payer + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return annualPremium;

            DateTime deceasedDate = DateTime.Now;
            DateTime lapseDate8 = DateTime.Now;
            double premium = 0D;
            double totalPremium = 0D;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                deceasedDate = dt.Rows[i]["deceasedDate"].ObjToDateTime();
                if (deceasedDate.Year > 1800)
                    continue;
                lapseDate8 = dt.Rows[i]["lapsedDate8"].ObjToDateTime();
                if (lapseDate8.Year > 1800)
                    continue;
                premium = dt.Rows[i]["premium"].ObjToDouble();
                premium = premium * 12D;
                premium = G1.RoundValue(premium);
                premium = premium * 0.95D;
                premium = G1.RoundDown(premium);
                totalPremium += premium;
            }
            annualPremium = totalPremium;
            annualPremium = G1.RoundDown(annualPremium);
            return annualPremium;
        }
        /***********************************************************************************************/
        public static double CalcTotalLiability(string payer)
        {
            double totalLiability = 0D;
            string cmd = "Select * from `policies` where `payer` = '" + payer + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return totalLiability;

            DateTime deceasedDate = DateTime.Now;
            DateTime lapseDate8 = DateTime.Now;
            double liability = 0D;
            string lapsed = "";

            bool doit = true;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                doit = true;
                deceasedDate = dt.Rows[i]["deceasedDate"].ObjToDateTime();
                if (deceasedDate.Year > 1800)
                    doit = false;
                lapseDate8 = dt.Rows[i]["lapsedDate8"].ObjToDateTime();
                if (lapseDate8.Year > 1800)
                    doit = false;

                lapsed = dt.Rows[i]["lapsed"].ObjToString().ToUpper();
                if (lapsed == "Y")
                    doit = false;

                if (!doit)
                    continue;

                liability = dt.Rows[i]["liability"].ObjToDouble();
                totalLiability += liability;
            }
            totalLiability = G1.RoundDown(totalLiability);
            return totalLiability;
        }
        /***********************************************************************************************/
        private void runThirdPartyReportForThisPolicyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            int row = gridMain.FocusedRowHandle;
            row = gridMain.GetDataSourceRowIndex(row);
            string contract = dr["contractNumber"].ObjToString();
            InsuranceCollectionsReport insureForm = new InsuranceCollectionsReport( contract );
            insureForm.Show();
        }
        /***********************************************************************************************/
        private void gridMain_CustomDrawFooterCell(object sender, DevExpress.XtraGrid.Views.Grid.FooterCellCustomDrawEventArgs e)
        {
            //if (loadAll)
            //    return;
            if (e.Column.FieldName != "premium" && e.Column.FieldName != "historicPremium" )
                return;
            int dx = e.Bounds.Height;
            Brush brush = new System.Drawing.SolidBrush(this.gridMain.Appearance.BandPanelBackground.BackColor);
//            Brush brush = e.Cache.GetGradientBrush(e.Bounds, this.gridMain.Appearance.BandPanelBackground.BackColor, Color.FloralWhite, );
            Rectangle r = e.Bounds;
            //Draw a 3D border 
            BorderPainter painter = BorderHelper.GetPainter(DevExpress.XtraEditors.Controls.BorderStyles.Style3D);
            AppearanceObject borderAppearance = new AppearanceObject(e.Appearance);
            borderAppearance.BorderColor = Color.DarkGray;
            painter.DrawObject(new BorderObjectInfoArgs(e.Cache, borderAppearance, r));
            //Fill the inner region of the cell 
            r.Inflate(-1, -1);
            e.Cache.FillRectangle(brush, r);
            //Draw a summary value 
            r.Inflate(-2, 0);
            double total = calculateTotalPremiums();
            string text = G1.ReformatMoney(total);
            e.Appearance.DrawString(e.Cache, text, r);
            //Prevent default drawing of the cell 
            e.Handled = true;
        }
        /****************************************************************************************/
        private double calculateTotalPremiums()
        {
            DataTable dt = (DataTable)dgv.DataSource;
            double price = 0D;
            double total = 0D;
            string lapsed = "";
            DateTime date = DateTime.Now;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                date = dt.Rows[i]["deceasedDate"].ObjToDateTime();
                if (date.Year > 100)
                    continue;

                date = dt.Rows[i]["lapsedDate8"].ObjToDateTime();
                if (date.Year > 100)
                    continue;
                lapsed = dt.Rows[i]["lapsed"].ObjToString();
                if (lapsed.ToUpper() == "Y")
                    continue;

                price = dt.Rows[i]["premium"].ObjToDouble();
                total += price;
            }
            return total;
        }
        /***********************************************************************************************/
        private void btnLapseAll_Click(object sender, EventArgs e)
        { // Policies have lapsedDate8 and icontracts have lapseDate8
            DataTable dt = (DataTable)dgv.DataSource;
            DialogResult result = MessageBox.Show("Are you sure you want to MARK ALL Polices plus Payer as Lapsed???", "Mark ALL Policies Lapsed Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
            if (result == DialogResult.No)
                return;
            string contracts = "";
            string record = "";
            string name = "";
            string contractNumber = "";
            string mainContract = "";
            DateTime today = DateTime.Now;
            string lapseDate = today.ToString("yyyy-MM-dd");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                record = dt.Rows[i]["record"].ObjToString();
                name = dt.Rows[i]["policyfullname"].ObjToString();
                G1.update_db_table("policies", "record", record, new string[] { "lapsed", "Y", "lapsedDate8", lapseDate });
                G1.AddToAudit(LoginForm.username, "PastDue", "Lapse", "Set", name);
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                if (!contracts.Contains(contractNumber))
                {
                    contracts += contractNumber + ",";
                    if (String.IsNullOrWhiteSpace(mainContract))
                        mainContract = contractNumber;
                }
            }
            contracts = contracts.TrimEnd(',');

            SetAllLapsedOrNot(true, contracts);

            contracts = contracts.TrimEnd(',');
            string[] Lines = contracts.Split(',');
            string cmd = "";
            for (int i = 0; i < Lines.Length; i++)
            {
                contractNumber = Lines[i].Trim();
                cmd = "Select * from `icontracts` where `contractNumber` = '" + contractNumber + "';";
                dt = G1.get_db_data(cmd);
                if (dt.Rows.Count > 0)
                    DailyHistory.SetLapsed(contractNumber, lapseDate);
            }

            LoadData();

            this.Cursor = Cursors.WaitCursor;
            ReinstateReport report = new ReinstateReport(mainContract, true);
            report.Show();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void changePayerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string newPayer = "";
            using (Ask askForm = new Ask("Enter Payer # (Must Already Exist)?"))
            {
                askForm.Text = "";
                askForm.ShowDialog();
                if (askForm.DialogResult != System.Windows.Forms.DialogResult.OK)
                    return;
                newPayer = askForm.Answer;
                if (String.IsNullOrWhiteSpace(newPayer))
                    return;
            }
            if (String.IsNullOrWhiteSpace(newPayer))
                return;

            DataRow dr = gridMain.GetFocusedDataRow();
            string oldPolicyRecord = dr["record"].ObjToString();
            string cmd = "Select * from `icustomers` where `payer` = '" + newPayer + "';";
            DataTable dt = G1.get_db_data(cmd);
            if ( dt.Rows.Count <= 0 )
            {
                MessageBox.Show("***ERROR*** New Payer Does Not Exist yet! You must first create a new Insurance Payer!");
                return;
            }
            string contractNumber = dt.Rows[0]["contractNumber"].ObjToString();
            G1.update_db_table("policies", "record", oldPolicyRecord, new string[] { "contractNumber", contractNumber, "payer", newPayer});
            LoadData();
        }
        /***********************************************************************************************/
        private void btnAdd_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowhandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowhandle);
            string contractNumber = dr["contractNumber"].ObjToString();
            string payer = dr["payer"].ObjToString();
            string pcode = dr["pCode"].ObjToString();
            string ucode = dr["ucode"].ObjToString();
            string report = dr["report"].ObjToString();
            string address1 = dr["address1"].ObjToString();
            string address2 = dr["address2"].ObjToString();
            string city = dr["city"].ObjToString();
            string state = dr["state"].ObjToString();
            string zip1 = dr["zip1"].ObjToString();
            string zip2 = dr["zip2"].ObjToString();
            string agent = dr["agentCode"].ObjToString();
            string agent1 = dr["agentCode1"].ObjToString();
            string oldagent = dr["oldAgentInfo"].ObjToString();
            string coverageType = dr["coverageType"].ObjToString();

            DataTable cloneDt = dt.Clone();

            DataRow dRow = cloneDt.NewRow();
            dRow["contractNumber"] = contractNumber;
            dRow["payer"] = payer;
            dRow["pCode"] = pcode;
            dRow["ucode"] = ucode;
            dRow["report"] = report;
            dRow["address1"] = address1;
            dRow["address2"] = address2;
            dRow["city"] = city;
            dRow["state"] = state;
            dRow["zip1"] = zip1;
            dRow["zip2"] = zip2;
            dRow["agentCode"] = agent;
            dRow["agentCode1"] = agent1;
            dRow["oldAgentInfo"] = oldagent;
            dRow["coverageType"] = coverageType;
            cloneDt.Rows.Add(dRow);

            using (AddEditPolicy addEditForm = new AddEditPolicy(this.Text, cloneDt))
            {
                DialogResult result = addEditForm.ShowDialog();
                if (result == DialogResult.OK)
                {
                    saveRow = -2;
                    LoadData();
                    CheckPayerDead();
                }
            }
        }
        /***********************************************************************************************/
        private void btnEdit_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            string contractNumber = dr["contractNumber"].ObjToString();
            string payer = dr["payer"].ObjToString();

            DataTable dt = (DataTable)dgv.DataSource;
            DataTable cloneDt = dt.Clone();

            G1.copy_dt_row(dt, row, cloneDt, cloneDt.Rows.Count);

            using (AddEditPolicy addEditForm = new AddEditPolicy(this.Text, cloneDt))
            {
                DialogResult result = addEditForm.ShowDialog();
                if (result == DialogResult.OK)
                {
                    saveRow = row;
                    LoadData();
                    CheckPayerDead();
                }
            }
        }
        /***********************************************************************************************/
        private void CheckPayerDead()
        {
            FunPayments.DeterminePayerDead(workPayer);
        }
        /***********************************************************************************************/
        private void btnDelete_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            string record = dr["record"].ObjToString();
            string policy = dr["policyNumber"].ObjToString();
            DialogResult result = MessageBox.Show("***Warning*** Are you SURE you want to DELETE policy (" + policy + ")?", "Delete Policy Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;
            G1.delete_db_table("policies", "record", record);

            dt.Rows.RemoveAt(row);
            dt.AcceptChanges();
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            if (rowHandle > (dt.Rows.Count - 1))
            {
                gridMain.FocusedRowHandle = rowHandle - 1;
                gridMain.RefreshData();
                dgv.Refresh();
            }
        }
        /***********************************************************************************************/
        private void clearDeceasedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            string contract = dr["contractNumber"].ObjToString();
            string payer = dr["payer"].ObjToString();
            string name = dr["policyfullname"].ObjToString();
            DialogResult result = MessageBox.Show("Are you sure you want to CLEAR Insurance Customer (" + name + ")  Deceased???", "Clear Deceased Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
            if (result == DialogResult.No)
                return;
            string record = "";
            DateTime today = DateTime.Now;
            string lapseDate = "0000-00-00";
            record = dr["record"].ObjToString();
            G1.update_db_table("policies", "record", record, new string[] { "deceasedDate", lapseDate });
            G1.AddToAudit(LoginForm.username, "Policies", "Clear Deceased", "ReSet", name);

            LoadData();
        }
        /***********************************************************************************************/
        private void chkFilterDeceased_CheckedChanged(object sender, EventArgs e)
        {
            gridMain.RefreshData();
            dgv.Refresh();
        }
        /***********************************************************************************************/
        private void chkFIlterLapsed_CheckedChanged(object sender, EventArgs e)
        {
            gridMain.RefreshData();
            dgv.Refresh();
        }
        /***********************************************************************************************/
        private void gridMain_CustomColumnDisplayText(object sender, CustomColumnDisplayTextEventArgs e)
        {
            ColumnView view = sender as ColumnView;
            if ( e.Column.FieldName.ToUpper() == "DUEDATE" && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                DataTable dt = (DataTable)dgv.DataSource;
                int row = e.ListSourceRowIndex;
                string lapsed = dt.Rows[row]["lapsed"].ObjToString().ToUpper();
                DateTime lapsedDate = dt.Rows[row]["lapsedDate8"].ObjToDateTime();
                if ( lapsed == "Y" )
                {
                    if (lapsedDate.Year == 0 || lapsedDate.Year == 1)
                    {
                        e.DisplayText = "Lapsed";
                        return;
                    }
                }
            }
            if (e.Column.FieldName.ToUpper().IndexOf("DATE") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                if (e.DisplayText.IndexOf("0000") >= 0 || e.DisplayText.IndexOf("0001") >= 0)
                    e.DisplayText = "";
                else
                {
                    DateTime date = e.DisplayText.ObjToDateTime();
                    if (date.Year < 100)
                        e.DisplayText = "";
                    else
                    {
                        if (loadAll)
                            e.DisplayText = date.ToString("yyyy-MM-dd");
                        else
                            e.DisplayText = date.ToString("MM/dd/yyyy");
                    }
                }
            }
            else if (e.Column.FieldName.ToUpper() == "SSN" && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                string ssn = e.DisplayText.Trim();
                ssn = ssn.Replace("-", "");
                string ssno = ssn;
                if (ssn.Trim().Length >= 8)
                    try { ssno = "XXX-XX-" + ssn.Substring(5, 4); }
                    catch { }
                e.DisplayText = ssno;
            }
            //else if (e.Column.FieldName.ToUpper() == "FULLNAME" && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            //{
            //    DataTable dt = (DataTable)dgv.DataSource;
            //    int row = e.ListSourceRowIndex;
            //    string fname = dt.Rows[row]["firstName"].ObjToString();
            //    string lname = dt.Rows[row]["lastName"].ObjToString();
            //    e.DisplayText = fname + " " + lname;

            //    string fname1 = dt.Rows[row]["firstName1"].ObjToString();
            //    string lname1 = dt.Rows[row]["lastName1"].ObjToString();

            //    //if (fname1.ToUpper() != fname.ToUpper() || lname1.ToUpper() != lname.ToUpper())
            //    //{
            //    //    e.DisplayText = "BAD MATCH";
            //    //}
            //}
            else if (e.Column.FieldName.ToUpper() == "POLICYFULLNAME" && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                DataTable dt = (DataTable)dgv.DataSource;
                int row = e.ListSourceRowIndex;
                string fname = dt.Rows[row]["policyFirstName"].ObjToString();
                string lname = dt.Rows[row]["policyLastName"].ObjToString();
                e.DisplayText = fname + " " + lname;
            }
            else if (e.Column.FieldName.ToUpper() == "NUM" && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                //RowVisibleState visible = gridMain.IsRowVisible(e.ListSourceRowIndex);
                //if ( visible != RowVisibleState.Hidden )
                //    e.DisplayText = e.ListSourceRowIndex.ToString();
            }
        }
        /***********************************************************************************************/
        private object missing = Type.Missing;
        /***********************************************************************************************/
        private void ExportToExcel()
        {
            DialogResult result = MessageBox.Show("Do you REALLY want to SAVE this data to an Excel File?", "Save Excel Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;

            DateTime startTime = DateTime.Now;

            Excel.Application oXL = new Excel.Application();
            oXL.Visible = false;
            Excel.Workbook oWB = oXL.Workbooks.Add(missing);

            try
            {
                Excel.Worksheet oSheet = oWB.ActiveSheet as Excel.Worksheet;
                DataTable dt = (DataTable)dgv.DataSource;
                LoadUpExcelTab(dt, oSheet, "Policies", gridMain);

            }
            catch (Exception ex)
            {
            }

            try
            {
                using (SaveFileDialog ofdImage = new SaveFileDialog())
                {
                    ofdImage.Filter = "Excel files (*.xlsx)|*.xlsx";

                    if (ofdImage.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        string fileName = ofdImage.FileName;

                        if (!String.IsNullOrWhiteSpace(fileName))
                        {
                            oWB.SaveAs(fileName, Excel.XlFileFormat.xlOpenXMLWorkbook,
                                missing, missing, missing, missing,
                                Excel.XlSaveAsAccessMode.xlNoChange,
                                missing, missing, missing, missing, missing);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
            oWB.Close(missing, missing, missing);
            oXL.UserControl = true;
            oXL.Quit();

            DateTime stopTime = DateTime.Now;
            TimeSpan ts = stopTime - startTime;

            int hours = ts.Hours;
            int minutes = ts.Minutes;
            int seconds = ts.Seconds;

            MessageBox.Show("***INFO*** Total Processing Time = " + hours.ToString("D2") + ":" + minutes.ToString("D2") + ":" + seconds.ToString("D2") + "!!");
        }
        /***********************************************************************************************/
        private void LoadUpExcelTab(DataTable dt, Excel.Worksheet oSheet, string name, DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gridMain)
        {
            oSheet.Name = name;
            //txtSavingTab.Text = oSheet.Name;
            //txtSavingTab.Refresh();

            string caption = "";
            string data = "";
            int index = 0;

            DataTable sortDt = new DataTable();
            sortDt.Columns.Add("columns", Type.GetType("System.Int32"));
            sortDt.Columns.Add("col", Type.GetType("System.Int32"));
            for (int col = 0; col < gridMain.Columns.Count; col++)
            {
                if (!gridMain.Columns[col].Visible)
                    continue;
                index = gridMain.Columns[col].ColIndex.ObjToInt32();
                if (index < 0)
                    continue;
                DataRow dRow = sortDt.NewRow();
                dRow["columns"] = index;
                dRow["col"] = col;
                sortDt.Rows.Add(dRow);
            }
            DataView tempview = sortDt.DefaultView;
            tempview.Sort = "columns asc";
            sortDt = tempview.ToTable();

            int myCol = 0;

            for (int col = 0; col < sortDt.Rows.Count; col++)
            {
                try
                {
                    myCol = sortDt.Rows[col]["col"].ObjToInt32();
                    if (!gridMain.Columns[myCol].Visible)
                        continue;
                    caption = gridMain.Columns[myCol].Caption;
                    //txtSavingColumn.Text = caption;
                    //txtSavingColumn.Refresh();
                    name = gridMain.Columns[myCol].FieldName;
                    //                    oSheet.Cells[col + 1, 1] = caption;
                    oSheet.Cells[1, col + 1] = caption;
                    for (int j = 0; j < dt.Rows.Count; j++)
                    {
                        data = dt.Rows[j][name].ObjToString();
                        if (!String.IsNullOrWhiteSpace(data))
                            oSheet.Cells[col + 1][j + 2] = data;
                    }
                }
                catch (Exception ex)
                {
                }
            }
        }
        /***********************************************************************************************/
        private void btnExport_Click(object sender, EventArgs e)
        {
            //ExportToExcel();
            DataTable dt = (DataTable)dgv.DataSource;
            DataTable visibleDt = new DataTable();
            for ( int i=0; i<gridMain.Columns.Count; i++)
            {
                if ( gridMain.Columns[i].Visible )
                {
                    visibleDt.Columns.Add(gridMain.Columns[i].FieldName);
                }
            }
            using (SaveFileDialog ofdImage = new SaveFileDialog())
            {
                ofdImage.Filter = "CSV files (*.csv)|*.csv";

                if (ofdImage.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string fileName = ofdImage.FileName;

                    if (!String.IsNullOrWhiteSpace(fileName))
                    {
                        this.Cursor = Cursors.WaitCursor;
                        try
                        {
                            MySQL.CreateCSVfile(dt, fileName, visibleDt, true, ",");
                        }
                        catch ( Exception ex)
                        {
                        }
                        this.Cursor = Cursors.Default;
                        MessageBox.Show("***DONE***");
                    }
                }
            }
        }
        /***********************************************************************************************/
        private void chkHonor_CheckedChanged(object sender, EventArgs e)
        {
            LoadData();
        }
        /***********************************************************************************************/
        private void chkSecNat_CheckedChanged(object sender, EventArgs e)
        {
            LoadData();
        }
        /***********************************************************************************************/
        private void chk3rdParty_CheckedChanged(object sender, EventArgs e)
        {
            LoadData();
        }
        /***********************************************************************************************/
        private void gridMain_CustomSummaryCalculate(object sender, DevExpress.Data.CustomSummaryEventArgs e)
        {
            if (G1.isField())
                return;
            double value = e.TotalValue.ObjToDouble();
            string field = (e.Item as GridSummaryItem).FieldName.ObjToString();
            double monthlyPremium = 0D;
            double historicPremium = 0D;
            double monthlySecNat = 0D;
            double monthly3rdParty = 0D;
            double premium = 0D;
            CustomerDetails.CalcMonthlyPremium(workPayer, ref monthlyPremium, ref historicPremium, ref monthlySecNat, ref monthly3rdParty );

            if (field.ToUpper() == "PREMIUM")
            {
                e.TotalValueReady = true;
                premium = monthlyPremium;
                if (chkHonor.Checked)
                {
                    if (chkSecNat.Checked)
                        premium = monthlySecNat;
                    else if (chk3rdParty.Checked)
                        premium = monthly3rdParty;
                    else
                    {
                        if (DateTime.Now > DailyHistory.kill3rdPartyDate)
                            premium = premium - monthlySecNat - monthly3rdParty;
                        else
                            premium = premium - monthlySecNat;
                    }
                }
                e.TotalValue = premium;
            }
            if (field.ToUpper() == "HISTORICPREMIUM")
            {
                e.TotalValueReady = true;
                e.TotalValue = historicPremium;
            }
            else if (field.ToUpper() == "LIABILITY")
            {
                e.TotalValueReady = true;
                premium = Policies.CalcTotalLiability(workPayer);
                e.TotalValue = premium;
            }
        }
        /***********************************************************************************************/
        private void chkInactive_CheckedChanged(object sender, EventArgs e)
        {
            gridMain.RefreshData();
            dgv.Refresh();
        }
        /***********************************************************************************************/
        private void gridMain_BeforePrintRow(object sender, DevExpress.XtraGrid.Views.Printing.CancelPrintRowEventArgs e)
        {
        }
        /***********************************************************************************************/
        private void gridMain_ColumnFilterChanged(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt == null)
                return;
            int count = dt.Rows.Count;
            gridMain.SelectAll();
            int[] rows = gridMain.GetSelectedRows();
            int row = 0;
            for ( int i=0; i<rows.Length; i++)
            {
                row = rows[i];
                var dRow = gridMain.GetDataRow(row);
                if (dRow != null)
                    dRow["num"] = (i + 1).ToString();
            }
            gridMain.ClearSelection();
        }
        /***********************************************************************************************/
        private DataTable GetUniquePayer(DataTable dt )
        {
            if (dt.Rows.Count <= 0)
                return dt;
            if (G1.get_column_number(dt, "Int32_id") < 0)
                dt.Columns.Add("Int32_id", typeof(int), "num");

            DataTable groupDt = dt.AsEnumerable().GroupBy(r => new { Col1 = r["payer"] }).Select(g => g.OrderBy(r => r["Int32_id"]).First()).CopyToDataTable();
            groupDt.Columns.Remove("Int32_id");
            return groupDt;
        }
        /***********************************************************************************************/
        private void chkShowUniquePayers_CheckedChanged(object sender, EventArgs e)
        {
            if (originalDt == null)
                return;
            if (chkShowUniquePayers.Checked)
            {
                DataTable dt = GetUniquePayer(originalDt);
                dgv.DataSource = dt;
            }
            else
                dgv.DataSource = originalDt;
            dgv.Refresh();
        }
        /***********************************************************************************************/
        private DataTable GetUniquePayerCompany(DataTable dt)
        {
            if (dt.Rows.Count <= 0)
                return dt;

            DataTable groupDt = originalDt.Clone();

            try
            {
                DataView tempview = originalDt.DefaultView;
                tempview.Sort = "payer asc,companyCode asc";
                DataTable sortDt = tempview.ToTable();

                string lastPayer = "";
                string lastCompanyCode = "";

                string payer = "";
                string companyCode = "";

                for ( int i=0; i<sortDt.Rows.Count; i++)
                {
                    payer = sortDt.Rows[i]["payer"].ObjToString();
                    if ( payer != lastPayer)
                    {
                        lastPayer = payer;
                        lastCompanyCode = sortDt.Rows[i]["companyCode"].ObjToString();
                        groupDt.ImportRow(sortDt.Rows[i]);
                        continue;
                    }
                    companyCode = sortDt.Rows[i]["companyCode"].ObjToString();
                    if (companyCode == lastCompanyCode)
                        continue;
                    lastCompanyCode = companyCode;
                    groupDt.ImportRow(sortDt.Rows[i]);
                }
            }
            catch ( Exception ex)
            {
            }

            return groupDt;
        }
        /***********************************************************************************************/
        private void chkShowUniquePayersAndCompany_CheckedChanged(object sender, EventArgs e)
        {
            if (originalDt == null)
                return;
            if (chkShowUniquePayersAndCompany.Checked )
            {
                DataTable dt = GetUniquePayerCompany(originalDt);
                dgv.DataSource = dt;
            }
            else
                dgv.DataSource = originalDt;
            dgv.Refresh();
        }
        /***********************************************************************************************/
        private void gridMain_RowCellStyle(object sender, RowCellStyleEventArgs e)
        {
            GridView View = sender as GridView;
            if (e.Column.FieldName.ToUpper() == "PREMIUM")
            {
                bool doColor = false;
                DataTable dt = (DataTable)dgv.DataSource;
                DateTime date = View.GetRowCellValue(e.RowHandle, "deceasedDate").ObjToDateTime();
                if (date.Year > 100)
                    doColor = true;
                string str = View.GetRowCellValue(e.RowHandle, "lapsed2").ObjToString();
                if ( !String.IsNullOrWhiteSpace ( str))
                {
                    if (str.ToUpper() == "Y")
                        doColor = true;
                }
                if (doColor)
                {
                    e.Appearance.ForeColor = Color.Red;
                }
            }
        }
        /***********************************************************************************************/
        private void btnCalcMonthly_Click(object sender, EventArgs e)
        {
            double monthlyPremium = 0D;
            double historicPremium = 0D;
            double monthlySecNat = 0D;
            double monthly3rdParty = 0D;

            double monthlyLiability = 0D;
            double monthlySecNatLiability = 0D;
            double monthly3rdLiability = 0D;

            double premium = 0D;
            double liability = 0D;
            string payer = "";
            string companyCode = "";

            this.Cursor = Cursors.WaitCursor;

            for (int i = 0; i < gridMain.DataRowCount; i++)
            {
                payer = gridMain.GetRowCellValue(i, "payer").ObjToString();
                companyCode = gridMain.GetRowCellValue(i, "companyCode").ObjToString();
                CalcCompanyMonthlyPremium(payer, companyCode, ref monthlyPremium, ref historicPremium, ref monthlySecNat, ref monthly3rdParty, ref monthlyLiability, ref monthlySecNat, ref monthly3rdLiability );
                premium = monthlyPremium;
                premium = premium - monthlySecNat - monthly3rdParty;
                premium = G1.RoundValue(premium);
                gridMain.SetRowCellValue(i, gridMain.Columns["premium"], premium.ToString());

                liability = monthlyLiability;
                liability = liability - monthlySecNatLiability - monthly3rdLiability;
                liability = G1.RoundValue(liability);
                gridMain.SetRowCellValue(i, gridMain.Columns["liability"], liability.ToString());
            }
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void CalcCompanyMonthlyPremium(string payer, string companyCode, ref double monthlyPremium, ref double historicPremium, ref double monthlySecNat, ref double monthly3rdParty, ref double monthlyLiability, ref double monthlySecNatLiability, ref double monthly3rdLiability )
        {
            monthlyPremium = 0D;
            historicPremium = 0D;
            monthlySecNat = 0D;
            monthly3rdParty = 0D;

            monthlyLiability = 0D;
            monthlySecNatLiability = 0D;
            monthly3rdLiability = 0D;
            double liability = 0D;

            string cmd = "Select * from `policies` where `payer` = '" + payer + "' ";
            if ( !String.IsNullOrWhiteSpace ( companyCode ))
                cmd += " AND `companyCode` = '" + companyCode + "' ";
            cmd += ";";

            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return;

            DateTime deceasedDate = DateTime.Now;
            DateTime lapseDate8 = DateTime.Now;
            double premium = 0D;
            string lapsed = "";
            string report = "";

            bool gotPremium = false;
            if (G1.get_column_number(dt, "myPremium") >= 0)
                gotPremium = true;
            bool doit = true;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                premium = dt.Rows[i]["historicPremium"].ObjToDouble();
                historicPremium += premium;

                doit = true;
                deceasedDate = dt.Rows[i]["deceasedDate"].ObjToDateTime();
                if (deceasedDate.Year > 1800)
                    doit = false;
                lapseDate8 = dt.Rows[i]["lapsedDate8"].ObjToDateTime();
                if (lapseDate8.Year > 1800)
                    doit = false;

                lapsed = dt.Rows[i]["lapsed"].ObjToString().ToUpper();
                if (lapsed == "Y")
                    doit = false;

                if (gotPremium && !doit)
                {
                    //dt.Rows[i]["myPremium"] = 0D;
                    continue;
                }
                else if (!doit)
                    continue;
                premium = dt.Rows[i]["premium"].ObjToDouble();
                monthlyPremium += premium;
                report = dt.Rows[i]["report"].ObjToString();
                if (report.ToUpper() != "NOT THIRD PARTY")
                    monthly3rdParty += premium;
                liability = dt.Rows[i]["liability"].ObjToDouble();
                monthlyLiability += liability;
                if (report.ToUpper() != "NOT THIRD PARTY")
                    monthly3rdLiability += liability;
            }
            monthlyPremium = G1.RoundDown(monthlyPremium);
            historicPremium = G1.RoundDown(historicPremium);

            monthlyLiability = G1.RoundValue(monthlyLiability);

            DataTable testDt = CustomerDetails.filterSecNat(true, dt);
            dt = testDt.Copy();

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                doit = true;
                deceasedDate = dt.Rows[i]["deceasedDate"].ObjToDateTime();
                if (deceasedDate.Year > 1800)
                    doit = false;
                lapseDate8 = dt.Rows[i]["lapsedDate8"].ObjToDateTime();
                if (lapseDate8.Year > 1800)
                    doit = false;

                lapsed = dt.Rows[i]["lapsed"].ObjToString().ToUpper();
                if (lapsed == "Y")
                    doit = false;

                if (gotPremium && !doit)
                {
                    //dt.Rows[i]["myPremium"] = 0D;
                    continue;
                }
                else if (!doit)
                    continue;
                premium = dt.Rows[i]["premium"].ObjToDouble();
                monthlySecNat += premium;

                liability = dt.Rows[i]["liablity"].ObjToDouble();
                monthlySecNatLiability += liability;
            }
            monthlySecNat = G1.RoundDown(monthlySecNat);
            monthly3rdParty = monthly3rdParty - monthlySecNat;
            monthly3rdParty = G1.RoundDown(monthly3rdParty);

            monthly3rdLiability = monthly3rdLiability - monthlySecNatLiability;
            monthly3rdLiability = G1.RoundValue(monthly3rdLiability);
            return;
        }
        /***********************************************************************************************/
        private void saveFormatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string name = cmbSelectColumns.Text.Trim();
            if (String.IsNullOrWhiteSpace(name))
                name = "Primary";
            string saveName = "Policies " + name;
            G1.SaveLocalPreferences(this, gridMain, LoginForm.username, saveName);
        }
        /***********************************************************************************************/
        private void removeFormatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string comboName = cmbSelectColumns.Text;
            if (!String.IsNullOrWhiteSpace(comboName))
            {
                string name = "Policies " + comboName;
                G1.RemoveLocalPreferences(LoginForm.username, name);
                foundLocalPreference = false;
            }
        }
        /***********************************************************************************************/
        private void chkGroupByCompany_CheckedChanged(object sender, EventArgs e)
        {
            if (chkGroupByCompany.Checked)
            {
                this.Cursor = Cursors.WaitCursor;

                DataTable dt = (DataTable)dgv.DataSource;
                if (dt == null)
                    return;
                if (G1.get_column_number(dt, "persons") < 0)
                    dt.Columns.Add("persons", Type.GetType("System.Double"));
                if (G1.get_column_number(dt, "policies") < 0)
                    dt.Columns.Add("policies", Type.GetType("System.Double"));
                DataView tempview = dt.DefaultView;
                tempview.Sort = "companyCode asc, policyNumber asc";
                dt = tempview.ToTable();

                double persons = 0;
                double policies = 0;
                string oldPolicy = "";
                string policy = "";
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    policy = dt.Rows[i]["policyNumber"].ObjToString();
                    if (String.IsNullOrWhiteSpace(oldPolicy))
                        oldPolicy = policy;
                    dt.Rows[i]["persons"] = 1D;
                    dt.Rows[i]["policies"] = 0D;
                    if (oldPolicy == policy)
                        continue;
                    if (oldPolicy != policy)
                    {
                        if ( i >= 0 )
                            dt.Rows[i-1]["policies"] = 1D;
                        oldPolicy = policy;
                    }
                }
                int lastRow = dt.Rows.Count - 1;
                dt.Rows[lastRow]["policies"] = 1D;

                dgv.DataSource = dt;

                //gridBand2.Visible = false;

                gridMain.Columns["persons"].Visible = true;
                gridMain.Columns["policies"].Visible = true;
                gridMain.OptionsView.ShowFooter = true;
                gridMain.Columns["companyCode"].GroupIndex = 0;
                gridMain.ExpandAllGroups();
                chkSummarize.Show();
                chkSummarize.Refresh();

                this.Cursor = Cursors.Default;
            }
            else
            {
                if ( chkSummarize.Checked )
                    chkSummarize.Checked = false;
                gridBand2.Visible = true;
                gridMain.Columns["persons"].Visible = false;
                gridMain.Columns["policies"].Visible = false;
                gridMain.Columns["companyCode"].GroupIndex = -1;
                gridMain.CollapseAllGroups();
                chkSummarize.Hide();
                chkSummarize.Refresh();
            }
        }
        /***********************************************************************************************/
        private DataTable tempDt = null;
        private void chkSummarize_CheckedChanged(object sender, EventArgs e)
        {
            if ( !chkSummarize.Checked )
            {
                if (tempDt != null)
                    dgv.DataSource = tempDt;
                else
                    dgv.DataSource = originalDt;

                gridMain.Columns["companyCode"].GroupIndex = 0;
                gridMain.ExpandAllGroups();
                gridMain.RefreshData();
                gridMain.RefreshEditor(true);
                dgv.Refresh();
                return;
            }

            tempDt = (DataTable)dgv.DataSource;

            DataTable dt = originalDt;
            if (G1.get_column_number(dt, "persons") < 0)
                dt.Columns.Add("persons", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "policies") < 0)
                dt.Columns.Add("policies", Type.GetType("System.Double"));
            DataView tempview = dt.DefaultView;
            tempview.Sort = "companyCode asc, policyNumber asc";
            dt = tempview.ToTable();

            DataTable dx = dt.Clone();

            double premium = 0D;
            double totalPremium = 0D;
            double liability = 0D;
            double totalLiability = 0D;
            double historic = 0D;
            double totalHistoric = 0D;


            string company = "";
            string oldCompany = "";
            double persons = 0;
            double policies = 0;
            string oldPolicy = "";
            string policy = "";
            DataRow dRow = null;
            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    company = dt.Rows[i]["companyCode"].ObjToString();
                    if (String.IsNullOrWhiteSpace(company))
                        company = "XX";
                    if (String.IsNullOrWhiteSpace(oldCompany))
                        oldCompany = company;
                    if (company != oldCompany)
                    {
                        dRow = dx.NewRow();
                        dRow["companyCode"] = oldCompany;
                        dRow["persons"] = persons;
                        dRow["policies"] = policies;
                        dRow["premium"] = totalPremium;
                        dRow["liability"] = totalLiability;
                        dRow["historicPremium"] = totalHistoric;
                        dx.Rows.Add(dRow);
                        persons = 1;
                        policies = 1;
                        oldCompany = company;
                        oldPolicy = dt.Rows[i]["policyNumber"].ObjToString();

                        totalPremium = dt.Rows[i]["premium"].ObjToDouble();
                        totalLiability = dt.Rows[i]["liability"].ObjToDouble();
                        totalHistoric = dt.Rows[i]["historicPremium"].ObjToDouble();
                        continue;
                    }
                    policy = dt.Rows[i]["policyNumber"].ObjToString();
                    if (String.IsNullOrWhiteSpace(oldPolicy))
                        oldPolicy = policy;
                    persons = persons + 1;
                    if (oldPolicy != policy)
                    {
                        policies = policies + 1;
                        oldPolicy = policy;
                    }
                    premium = dt.Rows[i]["premium"].ObjToDouble();
                    totalPremium += premium;
                    liability = dt.Rows[i]["liability"].ObjToDouble();
                    totalLiability += liability;
                    historic = dt.Rows[i]["historicPremium"].ObjToDouble();
                    totalHistoric += historic;
                }
                dRow = dx.NewRow();
                dRow["companyCode"] = oldCompany;
                dRow["persons"] = persons;
                dRow["policies"] = policies;
                dRow["premium"] = totalPremium;
                dRow["liability"] = totalLiability;
                dRow["historicPremium"] = totalHistoric;
                dx.Rows.Add(dRow);

                dgv.DataSource = dx;
            }
            catch ( Exception ex )
            {
            }

            gridMain.Columns["companyCode"].GroupIndex = -1;
            gridMain.RefreshData();
            gridMain.RefreshEditor(true);
            dgv.Refresh();
        }
        /***********************************************************************************************/
        private void deceasedPoliciesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InsuranceCompanyReports insForm = new InsuranceCompanyReports("Deceased");
            insForm.Show();
        }
        /***********************************************************************************************/
        private void activePoliciesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InsuranceCompanyReports insForm = new InsuranceCompanyReports("Active");
            insForm.Show();
        }
        /***********************************************************************************************/
        private void lapsePoliciesReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Trust85.FindContract(Policies.PoliciesActiveDt, "ZZ0014252");
            InsuranceCompanyReports insForm = new InsuranceCompanyReports("Lapsed");
            insForm.Show();
        }
        /***********************************************************************************************/
    }
}
