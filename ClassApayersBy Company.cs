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
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class ClassApayersByCompany : DevExpress.XtraEditors.XtraForm
    {
        private bool modified = false;
        private bool loading = true;
        private string workReport = "";
        private string workDatabase = "";
        private DataTable companyDt = null;
        private DataTable companyDt1 = null;
        private DataTable uniqueDt = null;
        /****************************************************************************************/
        public ClassApayersByCompany( string report )
        {
            InitializeComponent();
            workReport = report;

            SetupTotalsSummary();

            loadComboCompanies();
        }
        /****************************************************************************************/
        private void SetupTotalsSummary()
        {
            AddSummaryColumn("persons", null, "{0:0}");
            AddSummaryColumn("policies", null, "{0:0}");
            AddSummaryColumn("liability", null);
            AddSummaryColumn("premium", null);

            this.gridMain.GroupSummary.AddRange(new DevExpress.XtraGrid.GridSummaryItem[] 
            {
            new DevExpress.XtraGrid.GridGroupSummaryItem(DevExpress.Data.SummaryItemType.Sum, "liability", this.bandedGridColumn6, "{0:0,0.00}"),
            new DevExpress.XtraGrid.GridGroupSummaryItem(DevExpress.Data.SummaryItemType.Sum, "persons", this.bandedGridColumn4, "{0:0}"),
            new DevExpress.XtraGrid.GridGroupSummaryItem(DevExpress.Data.SummaryItemType.Sum, "policies", this.bandedGridColumn5, "{0:0}"),
            new DevExpress.XtraGrid.GridGroupSummaryItem(DevExpress.Data.SummaryItemType.Sum, "premium", this.bandedGridColumn7, "{0:0,0.00}")
            });

            AddSummaryColumn("persons", gridMain2, "{0:0}");
            AddSummaryColumn("policies", gridMain2, "{0:0}");
            AddSummaryColumn("liability", gridMain2);
            AddSummaryColumn("premium", gridMain2);

            this.gridMain2.GroupSummary.AddRange(new DevExpress.XtraGrid.GridSummaryItem[]
{
            new DevExpress.XtraGrid.GridGroupSummaryItem(DevExpress.Data.SummaryItemType.Sum, "liability", this.bandedGridColumn14, "{0:0,0.00}"),
            new DevExpress.XtraGrid.GridGroupSummaryItem(DevExpress.Data.SummaryItemType.Sum, "persons", this.bandedGridColumn18, "{0:0}"),
            new DevExpress.XtraGrid.GridGroupSummaryItem(DevExpress.Data.SummaryItemType.Sum, "policies", this.bandedGridColumn19, "{0:0}"),
            new DevExpress.XtraGrid.GridGroupSummaryItem(DevExpress.Data.SummaryItemType.Sum, "premium", this.bandedGridColumn15, "{0:0,0.00}")
});

            gridMain.OptionsView.ShowFooter = true;
        }
        /****************************************************************************************/
        private void AddSummaryColumn(string columnName, DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gMain = null, string format = "")
        {
            if (gMain == null)
                gMain = gridMain;
            //if (String.IsNullOrWhiteSpace(format))
            //    format = "${0:0,0.00}";
            if (String.IsNullOrWhiteSpace(format))
                format = "{0:0,0.00}";
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
        private void SetReportColumns(DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gMain = null)
        {
            if (gMain == null)
                gMain = gridMain;

            int i = 1;
            if (!chkGroupBy.Checked)
            {
                G1.SetColumnPosition(gMain, "num", i++);
                G1.SetColumnPosition(gMain, "payer", i++);
                G1.SetColumnPosition(gMain, "payerName", i++);
                G1.SetColumnPosition(gMain, "amtOfMonthlyPayt", i++);
                G1.SetColumnPosition(gMain, "payerD_date", i++);
                G1.SetColumnPosition(gMain, "address1", i++);
                G1.SetColumnPosition(gMain, "address2", i++);
                G1.SetColumnPosition(gMain, "city", i++);
                G1.SetColumnPosition(gMain, "state", i++);
                G1.SetColumnPosition(gMain, "zip1", i++);
                G1.SetColumnPosition(gMain, "policyFirstName", i++);
                G1.SetColumnPosition(gMain, "policyLastName", i++);
                G1.SetColumnPosition(gMain, "policyNumber", i++);
                G1.SetColumnPosition(gMain, "issueDate81", i++);
                G1.SetColumnPosition(gMain, "premium", i++);
                G1.SetColumnPosition(gMain, "liability", i++);
                G1.SetColumnPosition(gMain, "policyD_date", i++);
            }
            else
            {
                G1.SetColumnPosition(gMain, "num", i++);
                G1.SetColumnPosition(gMain, "policyFirstName", i++);
                G1.SetColumnPosition(gMain, "policyLastName", i++);
                G1.SetColumnPosition(gMain, "policyNumber", i++);
                G1.SetColumnPosition(gMain, "issueDate81", i++);
                G1.SetColumnPosition(gMain, "premium", i++);
                G1.SetColumnPosition(gMain, "liability", i++);
                G1.SetColumnPosition(gMain, "policyD_date", i++);
                G1.SetColumnPosition(gMain, "payer", i++);
                G1.SetColumnPosition(gMain, "payerName", i++);
                G1.SetColumnPosition(gMain, "amtOfMonthlyPayt", i++);
                G1.SetColumnPosition(gMain, "payerD_date", i++);
                G1.SetColumnPosition(gMain, "address1", i++);
                G1.SetColumnPosition(gMain, "address2", i++);
                G1.SetColumnPosition(gMain, "city", i++);
                G1.SetColumnPosition(gMain, "state", i++);
                G1.SetColumnPosition(gMain, "zip1", i++);
            }
        }
        /****************************************************************************************/
        private void ClassApayersByCompany_Load(object sender, EventArgs e)
        {
            btnReport.Hide();

            chkGroupPayer.Hide();
            chkGroupCompany.Hide();
            chkCompanyDetail.Hide();
            chkGroupByCompany.Hide();

            string cmd = "Select * from `i_companies` ORDER BY `order`;";
            companyDt = G1.get_db_data(cmd);

            companyDt.Columns.Add("num");
            companyDt.Columns.Add("mod");
            G1.NumberDataTable(companyDt);

            DateTime date = DateTime.Now;
            DateTime newDate = new DateTime(date.Year - 1, 1, 1);
            //this.dateTimePicker1.Value = newDate;
            newDate = new DateTime(newDate.Year, 12, 31);
            this.dateTimePicker2.Value = newDate;

            dgv.Hide();
            dgv.Dock = DockStyle.Fill;

            dgv3.Hide();
            dgv3.Dock = DockStyle.Fill;

            dgv2.Show();
            dgv2.Dock = DockStyle.Fill;

            ClearAllPositions(gridMain2);
            SetReportColumns(gridMain2);

            loading = false;
        }
        /****************************************************************************************/
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (loading)
                return;
            modified = true;
            DataRow dr = gridMain.GetFocusedDataRow();
            dr["mod"] = "Y";
        }
        /****************************************************************************************/
        private void gridMain_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt == null)
                return;
            string delete = dt.Rows[row]["mod"].ObjToString();
            if (delete.ToUpper() == "D")
            {
                e.Visible = false;
                e.Handled = true;
            }
        }
        /****************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        { // Spy Glass
            if (dgv.Visible)
                G1.SpyGlass(gridMain);
            else if (dgv2.Visible)
                G1.SpyGlass(gridMain2);
            else if (dgv3.Visible)
                G1.SpyGlass(gridMain3);
        }
        /****************************************************************************************/
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
        }
        /****************************************************************************************/
        private void EditTable_FormClosing(object sender, FormClosingEventArgs e)
        {
            //if (!btnSaveAll.Visible)
            //    return;
            //DialogResult result = MessageBox.Show("***Question*** Data has been modified.\nDo you really want to exit WITHOUT saving your data?", "Data Modified Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            //if (result == DialogResult.Yes)
            //    return;
            //e.Cancel = true;
        }
        /****************************************************************************************/
        private void gridMain_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
        }
        /****************************************************************************************/
        private void btnRight_Click(object sender, EventArgs e)
        {
            DateTime date = this.dateTimePicker2.Value;
            date = new DateTime(date.Year + 1, 1, 1);
            //this.dateTimePicker1.Value = date;

            date = new DateTime(date.Year, 12, 31);
            this.dateTimePicker2.Value = date;
        }
        /****************************************************************************************/
        private void btnLeft_Click(object sender, EventArgs e)
        {
            DateTime date = this.dateTimePicker2.Value;
            date = new DateTime(date.Year - 1, 1, 1);
            //this.dateTimePicker1.Value = date;

            date = new DateTime(date.Year, 12, 31);
            this.dateTimePicker2.Value = date;
        }
        /***********************************************************************************************/
        private void loadComboCompanies ()
        {
            //string cmd = "Select *,LEFT(policyNumber, 2) AS first_two_digits from `policies` ";
            //cmd += " WHERE `deceasedDate` <= '0001-01-01' AND `lapsed` <> 'Y' ";
            //cmd += " AND `report` = 'Not Third Party' ";
            //cmd += " AND `lapsed` <> 'Y' ";
            //cmd += " AND `lapsedDate8` <= '0100-01-01' ";
            //cmd += " AND ( `liability` >= '0.00' AND `liability` <= '450.00' ) ";
            //cmd += " GROUP BY `first_two_digits` ";
            //cmd += " ORDER by `first_two_digits` ";
            //cmd += ";";

            string cmd = "Select * from `policies` ";
            cmd += " WHERE `deceasedDate` <= '0001-01-01' AND `lapsed` <> 'Y' ";
            cmd += " AND `report` = 'Not Third Party' ";
            cmd += " AND `lapsed` <> 'Y' ";
            cmd += " AND `lapsedDate8` <= '0100-01-01' ";
            cmd += " AND ( `liability` >= '0.00' AND `liability` <= '450.00' ) ";
            cmd += " GROUP BY `companyCode` ";
            cmd += " ORDER by `companyCode` ";
            cmd += ";";

            DataTable dt = G1.get_db_data(cmd);

            chkComboCompanies.Properties.DataSource = dt;

            string ccCodes = "CC|CS|CI|UC|UF|UI";
            chkComboCompanies.EditValue = ccCodes;
            chkComboCompanies.Text = ccCodes;
        }
        /*******************************************************************************************/
        private string getCompanyQuery()
        {
            string procLoc = "";
            string[] locIDs = this.chkComboCompanies.EditValue.ToString().Split('|');
            for (int i = 0; i < locIDs.Length; i++)
            {
                if (!String.IsNullOrWhiteSpace(locIDs[i]))
                {
                    if (procLoc.Trim().Length > 0)
                        procLoc += " OR ";
                    procLoc += "q.`companyCode` = '" + locIDs[i].Trim() + "'";
                }
            }
            return procLoc.Length > 0 ? " AND (" + procLoc + ") " : "";
        }
        /*******************************************************************************************/
        private string getCompanyQueryx()
        {
            string procLoc = "";
            string[] locIDs = this.chkComboCompanies.EditValue.ToString().Split('|');
            for (int i = 0; i < locIDs.Length; i++)
            {
                if (!String.IsNullOrWhiteSpace(locIDs[i]))
                {
                    if (procLoc.Trim().Length > 0)
                        procLoc += " OR ";
                    procLoc += "q.`policyNumber` LIKE " + "'" + locIDs[i].Trim() + "%'";
                }
            }
            return procLoc.Length > 0 ? " AND (" + procLoc + ") " : "";
        }
        /****************************************************************************************/
        private void btnRun_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            DateTime date = this.dateTimePicker2.Value;
            string date1 = date.AddMonths(-6).ToString("yyyy-MM-dd");

            string comboCompanies = chkComboCompanies.Text.Trim();

            this.Text = date.Year.ToString() + " " + comboCompanies + " Policies in Force";

            string companyCode = txtCompanyCode.Text.Trim();
            string payerCode = txtPayer.Text.Trim();

            string cmd = "Select * from `icustomers` p JOIN `icontracts` x ON p.`contractNumber` = x.`contractNumber` ";
            cmd += " JOIN `policies` q ON p.`contractNumber` = q.`contractNumber` ";
            cmd += " LEFT JOIN `payers` c ON q.`payer` = c.`payer` ";
            cmd += " WHERE q.`deceasedDate` <= '0001-01-01' AND q.`lapsed` <> 'Y' ";
            cmd += " AND q.`report` = 'Not Third Party' ";
            cmd += " AND c.`dueDate8` >= '" + date1 + "' ";
            cmd += " AND c.`lapsed` <> 'Y' ";
            cmd += " AND q.`lapsedDate8` <= '0100-01-01' ";
            cmd += " AND ( q.`liability` >= '0.00' AND q.`liability` <= '450.00' ) ";
            if (!String.IsNullOrWhiteSpace(companyCode) || !String.IsNullOrWhiteSpace(payerCode) )
            {
                if (!String.IsNullOrWhiteSpace(companyCode))
                    cmd += " AND q.`companyCode` = '" + companyCode + "' ";
                if (!String.IsNullOrWhiteSpace(payerCode))
                    cmd += " AND q.`payer` = '" + payerCode + "' ";
            }
            else
            {
                cmd += getCompanyQuery();
            }
            cmd += " ORDER by q.`payer` ";
            cmd += ";";
            DataTable dt2 = G1.get_db_data(cmd);

            DataRow[] dRows = dt2.Select("lapsed2='Y'");
            if ( dRows.Length > 0 )
            {
                DataTable dddx = dRows.CopyToDataTable();
            }

            DateTime payerDueDate = DateTime.Now;
            DateTime dolp = DateTime.Now;
            DateTime payerDolp = DateTime.Now;
            string orphanContract = "";
            string contractNumber = "";
            string payer = "";
            string payerFirstName = "";
            string payerLastName = "";
            string payerName = "";

            try
            {
                if (G1.get_column_number(dt2, "ddate") < 0)
                    dt2.Columns.Add("ddate");
                if (G1.get_column_number(dt2, "duedate") < 0)
                    dt2.Columns.Add("duedate");
                if (G1.get_column_number(dt2, "payerD_date") < 0)
                    dt2.Columns.Add("payerD_date");
                if (G1.get_column_number(dt2, "policyD_date") < 0)
                    dt2.Columns.Add("policyD_date");
                if (G1.get_column_number(dt2, "payerName") < 0)
                    dt2.Columns.Add("payerName");

                CustomerDetails.FixOrphanPolicies2(dt2);

                for ( int i=0; i<dt2.Rows.Count; i++)
                {
                    payerFirstName = dt2.Rows[i]["firstName"].ObjToString();
                    payerLastName = dt2.Rows[i]["lastName"].ObjToString();
                    payerName = payerLastName + ", " + payerFirstName;
                    dt2.Rows[i]["payerName"] = payerName;

                    date = dt2.Rows[i]["dueDate81"].ObjToDateTime();
                    dt2.Rows[i]["payerD_date"] = date.ToString("MM/dd/yyyy");
                    dt2.Rows[i]["policyD_date"] = date.ToString("MM/dd/yyyy");
                }

                //for (int i = 0; i < dt2.Rows.Count; i++)
                //{
                //    //dt.Rows[i]["firstName"] = payerFname;
                //    //dt.Rows[i]["lastName"] = payerLname;
                //    payer = dt2.Rows[i]["payer"].ObjToString();
                //    if ( payer == "100767")
                //    {
                //    }
                //    date = dt2.Rows[i]["deceasedDate2"].ObjToDateTime();
                //    dt2.Rows[i]["ddate"] = date.ToString("yyyy-MM-dd");
                //    date = dt2.Rows[i]["dueDate8"].ObjToDateTime();
                //    payerDueDate = dt2.Rows[i]["dueDate81"].ObjToDateTime();
                //    if (payerDueDate >= date)
                //    {
                //        orphanContract = dt2.Rows[i]["contractNumber"].ObjToString();
                //        if (orphanContract.IndexOf("OO") == 0 || orphanContract.IndexOf("MM") == 0)
                //        {
                //            contractNumber = dt2.Rows[i]["contractNumber3"].ObjToString();
                //            if (contractNumber.IndexOf("ZZ") == 0)
                //                dt2.Rows[i]["contractNumber"] = contractNumber;
                //        }
                //        date = payerDueDate;
                //        dt2.Rows[i]["dueDate8"] = G1.DTtoMySQLDT(date.ToString("yyyy-MM-dd"));
                //        dolp = dt2.Rows[i]["lastDatePaid8"].ObjToDateTime();
                //        payerDolp = dt2.Rows[i]["lastDatePaid81"].ObjToDateTime();
                //        if (payerDolp > dolp)
                //            dt2.Rows[i]["lastDatePaid8"] = G1.DTtoMySQLDT(payerDolp.ToString("yyyy-MM-dd"));
                //    }
                //    dt2.Rows[i]["dueDate"] = date.ToString("yyyy-MM-dd");
                //}
            }
            catch ( Exception ex )
            {
            }

            //dRows = dt2.Select("payer='100767'");
            //if (dRows.Length > 0)
            //{
            //    DataTable dddx = dRows.CopyToDataTable();
            //}
            dt2 = FilterInactive(dt2);

            //dRows = dt2.Select("payer='100004'");
            //if (dRows.Length > 0)
            //{
            //    DataTable dddx = dRows.CopyToDataTable();
            //}


            dt2.Columns.Add("persons", Type.GetType("System.Double"));
            dt2.Columns.Add("policies", Type.GetType("System.Double"));
            dt2.Columns.Add("num", Type.GetType("System.Double"));

            DataTable dt = dt2.Copy();

            cmd = "Select * from `i_companies` ORDER BY `order`;";
            companyDt = G1.get_db_data(cmd);

            companyDt.Columns.Add("persons", Type.GetType("System.Double"));
            companyDt.Columns.Add("policies", Type.GetType("System.Double"));
            companyDt.Columns.Add("liability", Type.GetType("System.Double"));
            companyDt.Columns.Add("premium", Type.GetType("System.Double"));
            companyDt.Columns.Add("num");
            companyDt.Columns.Add("mod");
            G1.NumberDataTable(companyDt);

            companyDt1 = companyDt.Copy();

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
            payer = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dt.Rows[i]["num"] = i.ObjToDouble();
                payer = dt.Rows[i]["payer"].ObjToString().Trim();
                if (payer == "VI01893")
                {
                }
                companyCode = dt.Rows[i]["companyCode"].ObjToString();
                if (companyCode == "VI")
                {
                }
                policy = dt.Rows[i]["policyNumber"].ObjToString().Trim();
                if (String.IsNullOrWhiteSpace(oldPolicy))
                    oldPolicy = policy;
                dt.Rows[i]["persons"] = 0D;
                dt.Rows[i]["policies"] = 0D;
                dt2.Rows[i]["policies"] = 0D;
                if (oldPolicy == policy)
                    continue;
                if (i >= 0)
                {
                    dt.Rows[i - 1]["policies"] = 1D;
                    dt2.Rows[i - 1]["policies"] = 1D;
                }
                oldPolicy = policy;
            }
            int lastRow = dt.Rows.Count - 1;
            if (lastRow >= 0)
            {
                dt.Rows[lastRow]["policies"] = 1D;
                dt2.Rows[lastRow]["policies"] = 1D;
            }

            this.Cursor = Cursors.WaitCursor;

            dt = LoadUniquePerPayer(dt, companyDt );

            this.Cursor = Cursors.Default;


            dgv.Hide();
            dgv.DataSource = companyDt;


            G1.NumberDataTable(dt);

            if (G1.get_column_number(dt, "OriginalRow") < 0)
                dt.Columns.Add("OriginalRow");
            if (G1.get_column_number(dt, "FullInsuredName") < 0)
                dt.Columns.Add("FullInsuredName");
            string firstName = "";
            string lastName = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dt.Rows[i]["OriginalRow"] = dt.Rows[i]["num"].ObjToString();
                lastName = dt.Rows[i]["policyLastName"].ObjToString();
                firstName = dt.Rows[i]["policyFirstName"].ObjToString();
                dt.Rows[i]["FullInsuredName"] = lastName + ", " + firstName;
            }

            dgv2.DataSource = dt;
            dgv2.Show();

            btnReport.Show();
            btnReport.Refresh();

            if ( chkGroupBy.Checked || chkAss.Checked )
            {
                gridMain2.ExpandAllGroups();
                gridMain2.RefreshEditor(true);
                gridMain2.RefreshData();
                gridMain2.OptionsCustomization.AllowColumnResizing = true;
                gridMain2.OptionsView.ColumnAutoWidth = false;
            }

            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        public static DataTable LoadUniquePerPayer(DataTable dt, DataTable companyDt )
        {
            DataView tempview = dt.DefaultView;
            tempview.Sort = "payer asc";
            dt = tempview.ToTable();

            string oldPayer = "";
            string payer = "";

            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["persons"] = 0D;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                payer = dt.Rows[i]["payer"].ObjToString().Trim();
                if (string.IsNullOrWhiteSpace(oldPayer))
                    oldPayer = payer;
                if (oldPayer == payer)
                    continue;
                if (i >= 0)
                    dt.Rows[i - 1]["persons"] = 1D;
                else
                    dt.Rows[i - 1]["persons"] = 0D;
                oldPayer = payer;
            }
            int lastRow = dt.Rows.Count - 1;
            dt.Rows[lastRow]["persons"] = 1D;

            DataRow[] dRows = null;
            string companyCode = "";
            double liability = 0D;
            double totalLiability = 0D;
            double premium = 0D;
            double totalPremium = 0D;
            DataTable ddd = null;
            payer = "";
            oldPayer = "";

            double persons = 0D;
            double policies = 0D;

            try

            {
                if (G1.get_column_number(dt, "found") < 0)
                    dt.Columns.Add("found", Type.GetType("System.Double"));
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i]["found"] = 0D;
                }

                for (int i = 0; i < companyDt.Rows.Count; i++)
                {
                    companyCode = companyDt.Rows[i]["company"].ObjToString();
                    dRows = dt.Select("companyCode='" + companyCode + "'");
                    if (dRows.Length <= 0)
                        continue;

                    liability = 0D;
                    totalLiability = 0D;
                    premium = 0D;
                    totalPremium = 0D;
                    persons = 0D;
                    policies = 0D;

                    for (int j = 0; j < dRows.Length; j++)
                    {
                        persons += dRows[j]["persons"].ObjToDouble();
                        policies += dRows[j]["policies"].ObjToDouble();
                        totalLiability += dRows[j]["liability"].ObjToDouble();
                        totalPremium += dRows[j]["premium"].ObjToDouble();
                        dRows[j]["found"] = 1D;
                        dRows[j]["policies"] = 1D;
                    }

                    ddd = dRows.CopyToDataTable();

                    tempview = ddd.DefaultView;
                    tempview.Sort = "payer asc";
                    ddd = tempview.ToTable();
                    persons = 0;
                    oldPayer = "";

                    for (int j = 0; j < ddd.Rows.Count; j++)
                        persons += ddd.Rows[j]["persons"].ObjToDouble();


                    // for (int j = 0; j < ddd.Rows.Count; j++)
                    //{
                    //    payer = ddd.Rows[j]["payer"].ObjToString();
                    //    if (String.IsNullOrWhiteSpace(oldPayer))
                    //        oldPayer = payer;
                    //    if (oldPayer == payer)
                    //        continue;
                    //    if (oldPayer != payer)
                    //    {
                    //        persons++;
                    //        oldPayer = payer;
                    //    }
                    //}

                    companyDt.Rows[i]["persons"] = persons;
                    //companyDt.Rows[i]["policies"] = policies;
                    companyDt.Rows[i]["policies"] = dRows.Length;
                    companyDt.Rows[i]["liability"] = totalLiability;
                    companyDt.Rows[i]["premium"] = totalPremium;
                }
            }
            catch (Exception ex)
            {
            }

            return dt;
        }
        /****************************************************************************************/
        public static DataTable LoadUniquePerCompany ( DataTable dt, DataTable companyDt )
        {
            DataView tempview = dt.DefaultView;
            tempview.Sort = "companyCode asc, payer asc";
            dt = tempview.ToTable();

            string oldPayer = "";
            string payer = "";
            string oldCompany = "";
            string company = "";
            int lastRow = dt.Rows.Count;

            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["persons"] = 0D;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                company = dt.Rows[i]["companyCode"].ObjToString();
                if (String.IsNullOrWhiteSpace(oldCompany))
                    oldCompany = company;
                if (company != oldCompany)
                {
                    oldCompany = company;
                    if (i > 0)
                        dt.Rows[i - 1]["persons"] = 1D;
                    oldPayer = "";
                }
                payer = dt.Rows[i]["payer"].ObjToString().Trim();
                if (string.IsNullOrWhiteSpace(oldPayer))
                    oldPayer = payer;
                if (oldPayer != payer)
                {
                    if (i > 0)
                        dt.Rows[i - 1]["persons"] = 1D;
                    oldPayer = payer;
                }
            }
            if (lastRow >= 0)
            {
                lastRow = dt.Rows.Count - 1;
                dt.Rows[lastRow]["persons"] = 1D;
            }
            DataRow[] dRows = null;
            string companyCode = "";
            double liability = 0D;
            double totalLiability = 0D;
            double premium = 0D;
            double totalPremium = 0D;
            DataTable ddd = null;
            payer = "";
            oldPayer = "";

            double persons = 0D;
            double policies = 0D;

            try

            {
                if (G1.get_column_number(dt, "found") < 0)
                    dt.Columns.Add("found", Type.GetType("System.Double"));
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i]["found"] = 0D;
                }

                for (int i = 0; i < companyDt.Rows.Count; i++)
                {
                    companyCode = companyDt.Rows[i]["company"].ObjToString();
                    dRows = dt.Select("companyCode='" + companyCode + "'");
                    if (dRows.Length <= 0)
                        continue;

                    liability = 0D;
                    totalLiability = 0D;
                    premium = 0D;
                    totalPremium = 0D;
                    persons = 0D;
                    policies = 0D;

                    for (int j = 0; j < dRows.Length; j++)
                    {
                        persons += dRows[j]["persons"].ObjToDouble();
                        policies += dRows[j]["policies"].ObjToDouble();
                        totalLiability += dRows[j]["liability"].ObjToDouble();
                        totalPremium += dRows[j]["premium"].ObjToDouble();
                        dRows[j]["found"] = 1D;
                        dRows[j]["policies"] = 1D;
                    }

                    ddd = dRows.CopyToDataTable();

                    tempview = ddd.DefaultView;
                    tempview.Sort = "payer asc";
                    ddd = tempview.ToTable();
                    persons = 0;
                    oldPayer = "";

                    for (int j = 0; j < ddd.Rows.Count; j++)
                        persons += ddd.Rows[j]["persons"].ObjToDouble();


                    // for (int j = 0; j < ddd.Rows.Count; j++)
                    //{
                    //    payer = ddd.Rows[j]["payer"].ObjToString();
                    //    if (String.IsNullOrWhiteSpace(oldPayer))
                    //        oldPayer = payer;
                    //    if (oldPayer == payer)
                    //        continue;
                    //    if (oldPayer != payer)
                    //    {
                    //        persons++;
                    //        oldPayer = payer;
                    //    }
                    //}

                    companyDt.Rows[i]["persons"] = persons;
                    //companyDt.Rows[i]["policies"] = policies;
                    companyDt.Rows[i]["policies"] = dRows.Length;
                    companyDt.Rows[i]["liability"] = totalLiability;
                    companyDt.Rows[i]["premium"] = totalPremium;
                }
            }
            catch (Exception ex)
            {
            }

            return dt;
        }
        /****************************************************************************************/
        private DataTable FilterInactive ( DataTable dt )
        {
            DataRow[] dRows = dt.Select("contractNumber LIKE 'OO%' OR contractNumber LIKE 'MM%'");
            if ( dRows.Length > 0 )
            {
                for (int i = (dRows.Length - 1); i >= 0; i--)
                    dt.Rows.Remove(dRows[i]);
            }
            return dt;
        }
        /****************************************************************************************/
        private void chkGroupByCompany_CheckedChanged(object sender, EventArgs e)
        {
            if (chkGroupByCompany.Checked)
            {
                gridMain.OptionsView.ShowFooter = true;
                gridMain.Columns["formalCompany"].GroupIndex = 0;
                gridMain.Columns["formalCompany"].Visible = false;
                gridMain.ExpandAllGroups();
            }
            else
            {
                gridMain.OptionsView.ShowFooter = true;
                gridMain.Columns["formalCompany"].GroupIndex = -1;
                gridMain.Columns["formalCompany"].Visible = false;
                gridMain.ExpandAllGroups();
            }
        }
        /****************************************************************************************/
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
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
            bool noHeader = false;
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
            {
                printableComponentLink1.Component = dgv3;
                noHeader = true;
            }

            printableComponentLink1.PrintingSystemBase = printingSystem1;
            printableComponentLink1.CreateDetailHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateDetailHeaderArea);

            if ( !noHeader )
                printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);
            printableComponentLink1.BeforeCreateAreas += new System.EventHandler(this.printableComponentLink1_BeforeCreateAreas);
            printableComponentLink1.AfterCreateAreas += new System.EventHandler(this.printableComponentLink1_AfterCreateAreas);
            printableComponentLink1.Landscape = true;

            //Printer.setupPrinterMargins(50, 100, 110, 50);

            if ( !noHeader )
                Printer.setupPrinterMargins(30, 30, 90, 10);
            else
                Printer.setupPrinterMargins(30, 30, 10, 10);


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
            printableComponentLink1.ShowPreview();

            G1.AdjustColumnWidths(gridMain, 0.65D, false);
        }
        /***********************************************************************************************/
        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool noHeader = false;
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
            printableComponentLink1.CreateDetailHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateDetailHeaderArea);

            if ( !noHeader )
                printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);
            printableComponentLink1.BeforeCreateAreas += new System.EventHandler(this.printableComponentLink1_BeforeCreateAreas);
            printableComponentLink1.AfterCreateAreas += new System.EventHandler(this.printableComponentLink1_AfterCreateAreas);
            printableComponentLink1.Landscape = true;

            //Printer.setupPrinterMargins(50, 100, 110, 50);

            if (!noHeader)
                Printer.setupPrinterMargins(30, 30, 90, 10);
            else
                Printer.setupPrinterMargins(30, 30, 10, 10);

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

            font = new Font("Ariel", 12, FontStyle.Regular);
            string title = this.Text;
            int startX = 6;
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
        /****************************************************************************************/
        private void gridMain2_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain2.GetFocusedDataRow();
            string contract = dr["contractNumber"].ObjToString();
            if (String.IsNullOrWhiteSpace(contract))
                return;
            string cmd = "Select * from `icontracts` where `contractNumber` = '" + contract + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
            {
                MessageBox.Show("***ERROR*** Locating Contract " + contract + "!");
                return;
            }
            string contractRecord = dx.Rows[0]["record"].ObjToString();
            this.Cursor = Cursors.WaitCursor;
            DataTable dt = (DataTable)dgv.DataSource;
            //G1.UpdatePreviousCustomer(contract, LoginForm.username);
            string policyNumber = dr["policyNumber"].ObjToString();
            string policyFirstName = dr["policyFirstName"].ObjToString();
            string policyLastName = dr["policyLastName"].ObjToString();
            string policyRecord = dr["record"].ObjToString();

            CustomerDetails clientForm = new CustomerDetails(contract);
            clientForm.Show();

            //Policies policyForm = new Policies(contract);
            //policyForm.Show();

            //CustomerDetails clientForm = new CustomerDetails(contract);
            //clientForm.Show();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void chkCompanyDetail_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox box = (CheckBox)sender;
            if (box.Checked)
            {
                if (!chkGroupCompany.Checked)
                {
                    this.Cursor = Cursors.WaitCursor;
                    DataTable dt = (DataTable)dgv2.DataSource;
                    dt = LoadUniquePerCompany(dt, companyDt);
                    this.Cursor = Cursors.Default;
                }

                dgv2.Hide();
                dgv.Show();
                dgv.Refresh();
            }
            else
            {
                if (!chkGroupCompany.Checked)
                {
                    this.Cursor = Cursors.WaitCursor;
                    DataTable dt = (DataTable)dgv2.DataSource;
                    dt = LoadUniquePerPayer(dt, companyDt);
                    this.Cursor = Cursors.Default;
                }
                dgv.Hide();
                dgv2.Show();
                dgv2.Refresh();
            }
        }
        /****************************************************************************************/
        private void chkGroupPayer_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox box = (CheckBox)sender;

            if ( box.Checked )
            {
                gridMain2.Columns["companyCode"].GroupIndex = -1;
                gridMain2.Columns["payer"].GroupIndex = 0;
                gridMain2.RefreshEditor(true);
                gridMain2.RefreshData();
                gridMain2.ExpandAllGroups();
            }
            else
            {
                gridMain2.Columns["payer"].GroupIndex = -1;
                gridMain2.RefreshEditor(true);
                gridMain2.RefreshData();
            }
        }
        /****************************************************************************************/
        private void chkGroupCompany_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox box = (CheckBox)sender;

            DataTable dt = (DataTable)dgv2.DataSource;
            this.Cursor = Cursors.WaitCursor;

            //companyDt = companyDt1.Copy();

            if ( box.Checked )
                dt = LoadUniquePerCompany ( dt, companyDt );
            else
                dt = LoadUniquePerPayer ( dt, companyDt );
            this.Cursor = Cursors.Default;

            DataView tempview = dt.DefaultView;
            tempview.Sort = "companyCode asc, payer asc, policyNumber asc";
            dt = tempview.ToTable();
            dgv2.DataSource = dt;

            if (box.Checked)
            {
                gridMain2.Columns["payer"].GroupIndex = -1;
                gridMain2.Columns["companyCode"].GroupIndex = 0;
                gridMain2.RefreshEditor(true);
                gridMain2.RefreshData();
                gridMain2.ExpandAllGroups();
            }
            else
            {
                gridMain2.Columns["companyCode"].GroupIndex = -1;
                gridMain2.RefreshEditor(true);
                gridMain2.RefreshData();
            }
        }
        /***************************************************************************************/
        private DataTable CompareDT(DataTable dt1, DataTable dt2, string columnName )
        {
            DataTable dt3 = dt1.Clone();
            try
            {
                dt3 = dt1.AsEnumerable().Where(ra => !dt2.AsEnumerable().Any(rb => rb.Field<string>(columnName) == ra.Field<string>(columnName))).CopyToDataTable();
            }
            catch (Exception ex)
            {
            }
            return dt3;
        }
        /****************************************************************************************/
        private void compareToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Policies.PoliciesActiveDt == null)
                return;
            DataRow [] dRows = Policies.PoliciesActiveDt.Select ( "report='Not Third Party'");
            if (dRows.Length <= 0)
                return;

            this.Cursor = Cursors.WaitCursor;

            DataRow dr = null;

            Trust85.FindContract(Policies.PoliciesActiveDt, "ZZ0005191");
            DataTable pDt = dRows.CopyToDataTable();


            dRows = pDt.Select("payer='BB-7967'");
            if ( dRows.Length > 0 )
            {
                DataTable ddd = dRows.CopyToDataTable();
            }

            DataTable dt = (DataTable)dgv2.DataSource;

            DataTable dt1 = new DataTable();
            dt1.Columns.Add("contractNumber");
            dt1.Columns.Add("payer");
            dt1.Columns.Add("policyNumber");
            dt1.Columns.Add("report");
            dt1.Columns.Add("companyCode");
            dt1.Columns.Add("ALL");

            string contractNumber = "";
            string lapsed = "";
            DateTime deceasedDate = DateTime.Now;
            DateTime lapsedDate = DateTime.Now;
            DateTime dueDate = DateTime.Now;
            double liability = 0D;

            string payer = "";
            string all = "";

            DateTime date = this.dateTimePicker2.Value;
            string date1 = date.ToString("yyyy-MM-dd");
            date = this.dateTimePicker2.Value;
            string date2 = date.ToString("yyyy-MM-dd");

            Trust85.FindContract(pDt, "ZZ0014252");
            Trust85.FindContract(pDt, "ZZ0034238");



            for ( int i=0; i<pDt.Rows.Count; i++)
            {
                payer = pDt.Rows[i]["payer"].ObjToString();
                if (payer == "100190")
                {
                }
                contractNumber = pDt.Rows[i]["contractNumber"].ObjToString().ToUpper();
                if (contractNumber.IndexOf("OO") == 0 || contractNumber.IndexOf("MM") == 0)
                    continue;
                if ( contractNumber == "ZZ0034238")
                {
                }
                if (workReport.ToUpper().IndexOf("DECEASED") >= 0)
                {
                    //deceasedDate = pDt.Rows[i]["deceasedDate2"].ObjToDateTime();

                    //if (deceasedDate < this.dateTimePicker1.Value || deceasedDate > this.dateTimePicker2.Value )
                    //    continue;
                    //liability = pDt.Rows[i]["liability"].ObjToDouble();
                    //if (liability < 0D || liability > 450D)
                    //    continue;
                }
                else if (workReport.ToUpper().IndexOf("LAPSED") >= 0)
                {
                    //deceasedDate = pDt.Rows[i]["deceasedDate"].ObjToDateTime();
                    //if (deceasedDate.Year > 100)
                    //    continue;

                    //liability = pDt.Rows[i]["liability"].ObjToDouble();
                    //if (liability < 0D || liability > 450D)
                    //    continue;

                    //lapsedDate = pDt.Rows[i]["lapsedDate8"].ObjToDateTime();
                    //lapsed = pDt.Rows[i]["lapsed3"].ObjToString();
                    //if (String.IsNullOrWhiteSpace(lapsed))
                    //{
                    //    if (lapsedDate.Year < 100)
                    //    {
                    //        lapsed = pDt.Rows[i]["lapsed2"].ObjToString();
                    //        if (lapsed != "Y")
                    //            continue;
                    //    }
                    //}
                }
                else // Active Policies
                {
                    //deceasedDate = pDt.Rows[i]["deceasedDate"].ObjToDateTime();
                    //if (deceasedDate.Year > 100)
                    //    continue;
                    //lapsedDate = pDt.Rows[i]["lapsedDate8"].ObjToDateTime();
                    //if (lapsedDate.Year > 100)
                    //    continue;
                    //lapsed = pDt.Rows[i]["lapsed"].ObjToString();
                    //if (lapsed == "Y")
                    //    continue;
                    //lapsed = pDt.Rows[i]["lapsed3"].ObjToString();
                    //if (lapsed == "Y")
                    //    continue;
                    //dueDate = pDt.Rows[i]["dueDate8"].ObjToDateTime();
                    //if (dueDate < dateTimePicker1.Value)
                    //    continue;
                    //liability = pDt.Rows[i]["liability"].ObjToDouble();
                    //if (liability < 0D || liability > 450D)
                    //    continue;
                }


                //all = pDt.Rows[i]["contractNumber"].ObjToString() + "~";
                all = pDt.Rows[i]["payer"].ObjToString() + "~";
                all += pDt.Rows[i]["policyNumber"].ObjToString() + "~";
                all += pDt.Rows[i]["policyLastName"].ObjToString() + "~";
                all += pDt.Rows[i]["policyFirstName"].ObjToString() + "~";

                dr = dt1.NewRow();
                dr["payer"] = pDt.Rows[i]["payer"].ObjToString();
                dr["contractNumber"] = pDt.Rows[i]["contractNumber"].ObjToString();
                dr["policyNumber"] = pDt.Rows[i]["policyNumber"].ObjToString();
                dr["report"] = pDt.Rows[i]["report"].ObjToString();
                dr["companyCode"] = pDt.Rows[i]["companyCode"].ObjToString();
                dr["ALL"] = all;

                dt1.Rows.Add(dr);
            }

            DataTable dt2 = new DataTable();
            dt2.Columns.Add("contractNumber");
            dt2.Columns.Add("payer");
            dt2.Columns.Add("policyNumber");
            dt2.Columns.Add("report");
            dt2.Columns.Add("companyCode");
            dt2.Columns.Add("ALL");

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                //all = dt.Rows[i]["contractNumber"].ObjToString() + "~";
                all = dt.Rows[i]["payer"].ObjToString() + "~";
                all += dt.Rows[i]["policyNumber"].ObjToString() + "~";
                all += dt.Rows[i]["policyLastName"].ObjToString() + "~";
                all += dt.Rows[i]["policyFirstName"].ObjToString() + "~";

                dr = dt2.NewRow();
                dr["payer"] = dt.Rows[i]["payer"].ObjToString();
                dr["contractNumber"] = dt.Rows[i]["contractNumber"].ObjToString();
                dr["policyNumber"] = dt.Rows[i]["policyNumber"].ObjToString();
                dr["report"] = dt.Rows[i]["report"].ObjToString();
                dr["companyCode"] = dt.Rows[i]["companyCode"].ObjToString();
                dr["ALL"] = all;

                dt2.Rows.Add(dr);
            }

            dRows = dt1.Select("payer='100190'");
            if (dRows.Length > 0)
            {
                DataTable ddd = dRows.CopyToDataTable();
            }

            dRows = dt2.Select("payer='100190'");
            if (dRows.Length > 0)
            {
                DataTable ddd = dRows.CopyToDataTable();
            }

            DataTable dt3 = CompareDT(dt1, dt2, "ALL");
            G1.NumberDataTable(dt3);

            DataTable dt4 = CompareDT(dt2, dt1, "ALL");
            G1.NumberDataTable(dt4);

            dRows = dt3.Select("payer='BB-7967'");
            if (dRows.Length > 0)
            {
                DataTable ddd = dRows.CopyToDataTable();
            }
            dRows = dt4.Select("payer='BB-7967'");
            if (dRows.Length > 0)
            {
                DataTable ddd = dRows.CopyToDataTable();
            }
            Trust85.FindContract(pDt, "ZZ0034238");
            Trust85.FindContract(dt, "ZZ0034238");

            Trust85.FindContract(dt1, "ZZ0034238");
            Trust85.FindContract(dt2, "ZZ0034238");

            DataTable bothDt = new DataTable();
            bothDt.Columns.Add("InGreen");
            bothDt.Columns.Add("InOrange");
            bothDt.Columns.Add("ALL");

            for ( int i=0; i<dt3.Rows.Count; i++)
            {
                payer = dt3.Rows[i]["payer"].ObjToString();
                all = dt3.Rows[i]["ALL"].ObjToString();
                dRows = bothDt.Select ( "InOrange='" + payer + "'");
                if (dRows.Length <= 0)
                {
                    dr = bothDt.NewRow();
                    dr["InOrange"] = payer;
                    dr["ALL"] = all;
                    bothDt.Rows.Add(dr);
                }
            }

            for (int i = 0; i < dt4.Rows.Count; i++)
            {
                payer = dt4.Rows[i]["payer"].ObjToString();
                all = dt4.Rows[i]["ALL"].ObjToString();
                dRows = bothDt.Select("InGreen='" + payer + "'");
                if (dRows.Length <= 0)
                {
                    dr = bothDt.NewRow();
                    dr["InGreen"] = payer;
                    dr["ALL"] = all;
                    bothDt.Rows.Add(dr);
                }
            }

            BadPolicies badForm = new BadPolicies(bothDt);
            badForm.Show();

            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void gridMain2_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
        {

            if (e.Column.FieldName.ToUpper() == "NUM")
            {
                if (e.RowHandle >= 0)
                {
                    string num = (e.RowHandle + 1).ToString();
                    e.DisplayText = num;
                    int row = gridMain2.GetDataSourceRowIndex(e.RowHandle);
                    DataTable dt = (DataTable)dgv2.DataSource;
                    dt.Rows[row]["num"] = num;
                }
            }
        }
        /****************************************************************************************/
        private void gridMain2_ColumnFilterChanged(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv2.DataSource;
            int count = dt.Rows.Count;
            gridMain2.SelectAll();
            int[] rows = gridMain.GetSelectedRows();
            int row = 0;
            for (int i = 0; i < rows.Length; i++)
            {
                row = rows[i];
                var dRow = gridMain2.GetDataRow(row);
                if (dRow != null)
                    dRow["num"] = (i + 1).ToString();
            }
            gridMain2.ClearSelection();
        }
        /****************************************************************************************/
        private void gridMain2_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv2.DataSource;
            double liability = dt.Rows[row]["liability"].ObjToDouble();
            if ( liability < 0D || liability > 450.00D )
            {
                e.Visible = false;
                e.Handled = true;
            }
        }
        /****************************************************************************************/
        private int footerCount = 0;
        private void gridMain2_BeforePrintRow(object sender, DevExpress.XtraGrid.Views.Printing.CancelPrintRowEventArgs e)
        {
            if (chkGroupCompany.Checked)
            {
                if (e.HasFooter)
                {
                    footerCount++;
                }
            }
        }
        /****************************************************************************************/

        private void gridMain2_AfterPrintRow(object sender, DevExpress.XtraGrid.Views.Printing.PrintRowEventArgs e)
        {
            if (e.HasFooter)
            {
                footerCount++;
                if (footerCount >= 1)
                {
                    footerCount = 0;
                    if (chkGroupCompany.Checked)
                        e.PS.InsertPageBreak(e.Y);
                }
            }
        }
        /****************************************************************************************/
        private Color backColor = Color.Red;
        private void btnReport_Click(object sender, EventArgs e)
        {
            if ( dgv3.Visible )
            {
                dgv3.Hide();
                dgv2.Show();
                dgv2.Refresh();

                btnReport.BackColor = backColor;
                btnReport.Text = "Generate Report";
                btnReport.Refresh();
                return;
            }

            backColor = btnReport.BackColor;

            DataTable dt = (DataTable)dgv2.DataSource;

            DataTable d3 = new DataTable();
            d3.Columns.Add("c1");
            d3.Columns.Add("c2");
            d3.Columns.Add("c3");
            d3.Columns.Add("c4");
            d3.Columns.Add("c5");
            d3.Columns.Add("c6");
            d3.Columns.Add("c7");
            d3.Columns.Add("c8");
            d3.Columns.Add("c9");

            string payerName = "";
            string oldPayerName = "";

            for ( int i=0; i<dt.Rows.Count; i++)
            {
                try
                {
                    payerName = dt.Rows[i]["payerName"].ObjToString();
                    if (payerName != oldPayerName)
                    {
                        d3 = AddMainHeader(d3, dt, i);
                        oldPayerName = payerName;
                    }
                    else
                    {
                        d3 = AddPolicyLine(d3, dt, i);
                    }
                }
                catch ( Exception ex)
                {
                }
            }

            dgv2.Hide();

            dgv3.DataSource = d3;
            dgv3.Refresh();

            dgv3.Show();
            dgv3.Refresh();

            btnReport.Text = "GO BACK";
            btnReport.BackColor = Color.Pink;
            btnReport.Refresh();
        }
        /****************************************************************************************/
        private DataTable AddMainHeader ( DataTable d3, DataTable dt, int i )
        {
            try
            {
                DataRow dRow = null;
                if ( d3.Rows.Count > 0 )
                {
                    dRow = d3.NewRow();
                    dRow["c1"] = "BREAK";
                    d3.Rows.Add(dRow);
                }

                dRow = d3.NewRow();
                dRow["c1"] = "Payer #";
                dRow["c2"] = "Payer Name";
                dRow["c3"] = "Payer Due Date";
                dRow["c4"] = "Payer Premium";
                dRow["c5"] = "Payer Address 1";
                dRow["c6"] = "Payer Address 2";
                dRow["c7"] = "Payer City";
                dRow["c8"] = "Payer State";
                dRow["c9"] = "Payer Zip";
                d3.Rows.Add(dRow);

                dRow = d3.NewRow();
                dRow["c1"] = dt.Rows[i]["payer"].ObjToString();
                dRow["c2"] = dt.Rows[i]["payerName"].ObjToString();
                dRow["c3"] = dt.Rows[i]["payerD_date"].ObjToString();
                dRow["c4"] = dt.Rows[i]["amtOfMonthlyPayt"].ObjToString();
                dRow["c5"] = dt.Rows[i]["address1"].ObjToString();
                dRow["c6"] = dt.Rows[i]["address2"].ObjToString();
                dRow["c7"] = dt.Rows[i]["city"].ObjToString();
                dRow["c8"] = dt.Rows[i]["state"].ObjToString();
                dRow["c9"] = dt.Rows[i]["zip1"].ObjToString();
                d3.Rows.Add(dRow);

                d3 = AddSubHeader(d3);

                d3 = AddPolicyLine(d3, dt, i);
            }
            catch ( Exception ex)
            {
            }

            return d3;
        }
        /****************************************************************************************/
        private DataTable AddSubHeader ( DataTable d3 )
        {
            DataRow dRow = d3.NewRow();
            d3.Rows.Add(dRow);

            dRow = d3.NewRow();
            dRow["c1"] = "";
            dRow["c2"] = "Insured Name";
            dRow["c3"] = "Policy #";
            dRow["c4"] = "Policy Issue Date";
            dRow["c5"] = "Policy Premium";
            dRow["c6"] = "Policy Liability";
            dRow["c7"] = "Policy Due Date";
            dRow["c8"] = "";
            dRow["c9"] = "";
            d3.Rows.Add(dRow);

            return d3;
        }
        /****************************************************************************************/
        private DataTable AddPolicyLine(DataTable d3, DataTable dt, int i)
        {
            string policyFirstName = dt.Rows[i]["policyFirstName"].ObjToString();
            string policyLastName = dt.Rows[i]["policyLastName"].ObjToString();

            DataRow dRow = d3.NewRow();
            dRow["c1"] = "";
            dRow["c2"] = policyFirstName + " " + policyLastName;
            dRow["c3"] = dt.Rows[i]["policyNumber"].ObjToString();
            dRow["c4"] = dt.Rows[i]["issueDate81"].ObjToString();
            dRow["c5"] = dt.Rows[i]["premium"].ObjToString();
            dRow["c6"] = dt.Rows[i]["liability"].ObjToString();
            dRow["c7"] = dt.Rows[i]["policyD_date"].ObjToString();
            dRow["c8"] = "";
            dRow["c9"] = "";
            d3.Rows.Add(dRow);

            return d3;
        }
        /****************************************************************************************/
        private bool pageBreak = false;
        private void gridMain3_BeforePrintRow(object sender, DevExpress.XtraGrid.Views.Printing.CancelPrintRowEventArgs e)
        {
            int rowHandle = e.RowHandle;
            if (gridMain3.IsDataRow(rowHandle))
            {
                try
                {
                    DataTable dt = (DataTable)dgv3.DataSource;
                    int row = gridMain3.GetDataSourceRowIndex(rowHandle);

                    string newPage = dt.Rows[row]["c1"].ObjToString();
                    if (newPage.ToUpper() == "BREAK")
                    {
                        pageBreak = true;
                        e.Cancel = true;
                    }
                }
                catch (Exception ex)
                {
                }
            }
        }
        /****************************************************************************************/
        private void gridMain3_AfterPrintRow(object sender, DevExpress.XtraGrid.Views.Printing.PrintRowEventArgs e)
        {
            if (pageBreak)
                e.PS.InsertPageBreak(e.Y);
            pageBreak = false;
        }
        /****************************************************************************************/
        private void chkGroupBy_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox check = (CheckBox)sender;
            DataTable dt = (DataTable)dgv2.DataSource;
            if (dt == null)
                return;

            if ( check.Checked )
            {
                ClearAllPositions(gridMain2);
                SetReportColumns(gridMain2);

                gridMain2.Columns["FullInsuredName"].GroupIndex = 0;
                gridMain2.ExpandAllGroups();
                gridMain2.OptionsCustomization.AllowColumnResizing = true;
                gridMain2.OptionsView.ColumnAutoWidth = false;
            }
            else
            {
                gridMain2.Columns["FullInsuredName"].GroupIndex = -1;

                ClearAllPositions(gridMain2);
                SetReportColumns(gridMain2);
            }

            gridMain2.RefreshEditor(true);
            gridMain2.RefreshData();
            gridMain2.OptionsCustomization.AllowColumnResizing = true;
            gridMain2.OptionsView.ColumnAutoWidth = false;
        }
        /****************************************************************************************/
        private void chkAss_CheckedChanged(object sender, EventArgs e)
        {
            if ( !chkAss.Checked )
            {
                gridMain2.Columns["payerName"].GroupIndex = -1;
                //chkAss.Checked = false;
                ClearAllPositions( gridMain2 );
                SetReportColumns( gridMain2 );

                gridMain2.Columns["FullInsuredName"].GroupIndex = -1;
                gridMain2.RefreshEditor(true);
                gridMain2.RefreshData();
                gridMain2.OptionsCustomization.AllowColumnResizing = true;
                gridMain2.OptionsView.ColumnAutoWidth = false;
                return;
            }
            if ( chkGroupBy.Checked )
                chkGroupBy.Checked = false;

            gridMain2.Columns["FullInsuredName"].GroupIndex = -1;
            gridMain2.Columns["payerName"].GroupIndex = 0;
            gridMain2.ExpandAllGroups();
            gridMain2.RefreshEditor(true);
            gridMain2.RefreshData();
            gridMain2.OptionsCustomization.AllowColumnResizing = true;
            gridMain2.OptionsView.ColumnAutoWidth = false;
        }
        /****************************************************************************************/
        private void chkComboCompanies_EditValueChanged(object sender, EventArgs e)
        {
            //chkComboCompanies.Text = chkComboCompanies.EditValue;
            chkComboCompanies.Refresh();
        }
        /****************************************************************************************/
    }
}