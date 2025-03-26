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
    public partial class InsuranceCompanyReports : DevExpress.XtraEditors.XtraForm
    {
        private bool modified = false;
        private bool loading = true;
        private string workReport = "";
        private string workDatabase = "";
        private DataTable companyDt = null;
        private DataTable companyDt1 = null;
        private DataTable uniqueDt = null;
        /****************************************************************************************/
        public InsuranceCompanyReports( string report )
        {
            InitializeComponent();
            workReport = report;

            SetupTotalsSummary();
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
        private void InsuranceCompanyReports_Load(object sender, EventArgs e)
        {
            string cmd = "Select * from `i_companies` ORDER BY `order`;";
            companyDt = G1.get_db_data(cmd);

            companyDt.Columns.Add("num");
            companyDt.Columns.Add("mod");
            G1.NumberDataTable(companyDt);

            DateTime date = DateTime.Now;
            DateTime newDate = new DateTime(date.Year - 1, 1, 1);
            this.dateTimePicker1.Value = newDate;
            newDate = new DateTime(newDate.Year, 12, 31);
            this.dateTimePicker2.Value = newDate;

            dgv.Hide();
            dgv.Dock = DockStyle.Fill;
            dgv2.Show();
            dgv2.Dock = DockStyle.Fill;

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
            DateTime date = this.dateTimePicker1.Value;
            date = new DateTime(date.Year + 1, 1, 1);
            this.dateTimePicker1.Value = date;

            date = new DateTime(date.Year, 12, 31);
            this.dateTimePicker2.Value = date;
        }
        /****************************************************************************************/
        private void btnLeft_Click(object sender, EventArgs e)
        {
            DateTime date = this.dateTimePicker1.Value;
            date = new DateTime(date.Year - 1, 1, 1);
            this.dateTimePicker1.Value = date;

            date = new DateTime(date.Year, 12, 31);
            this.dateTimePicker2.Value = date;
        }
        /****************************************************************************************/
        private void btnRun_Click(object sender, EventArgs e)
        {
            if (workReport.ToUpper().IndexOf("DECEASED") >= 0)
            {
                LoadDeceased();
                return;
            }
            else if (workReport.ToUpper().IndexOf("LAPSED") >= 0)
            {
                LoadLapsed();
                return;
            }

            this.Cursor = Cursors.WaitCursor;

            DateTime date = this.dateTimePicker1.Value;
            string date1 = date.ToString("yyyy-MM-dd");

            this.Text = date.Year.ToString() + " Class \"A\" Policies in Force";

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
            //cmd += " AND c.`lapseDate8` < c.`reinstateDate8` ";
            if (!String.IsNullOrWhiteSpace(companyCode))
                cmd += " AND q.`companyCode` = '" + companyCode + "' ";
            if (!String.IsNullOrWhiteSpace(payerCode))
                cmd += " AND q.`payer` = '" + payerCode + "' ";
            cmd += " ORDER by q.`payer` ";
            cmd += ";";
            DataTable dt2 = G1.get_db_data(cmd);

            DataRow[] dRows = dt2.Select("lapsed2='Y'");
            if ( dRows.Length > 0 )
            {
                DataTable dddx = dRows.CopyToDataTable();
            }
            dRows = dt2.Select("payer='100004'");
            if (dRows.Length > 0)
            {
                DataTable dddx = dRows.CopyToDataTable();
            }

            Trust85.FindContract(dt2, "ZZ0002677");

            DateTime payerDueDate = DateTime.Now;
            DateTime dolp = DateTime.Now;
            DateTime payerDolp = DateTime.Now;
            string orphanContract = "";
            string contractNumber = "";
            string payer = "";

            try
            {
                if (G1.get_column_number(dt2, "ddate") < 0)
                    dt2.Columns.Add("ddate");
                if (G1.get_column_number(dt2, "duedate") < 0)
                    dt2.Columns.Add("duedate");

                CustomerDetails.FixOrphanPolicies2(dt2);

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

            dRows = dt2.Select("payer='100767'");
            if (dRows.Length > 0)
            {
                DataTable dddx = dRows.CopyToDataTable();
            }
            dt2 = FilterInactive(dt2);

            dRows = dt2.Select("payer='100004'");
            if (dRows.Length > 0)
            {
                DataTable dddx = dRows.CopyToDataTable();
            }


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

            //tempview = dt.DefaultView;
            //tempview.Sort = "companyCode asc, payer asc";
            //dt = tempview.ToTable();

            //string oldPayer = "";
            //string oldCompany = "";
            //string company = "";

            //for (int i = 0; i < dt.Rows.Count; i++)
            //{
            //    company = dt.Rows[i]["companyCode"].ObjToString();
            //    if (String.IsNullOrWhiteSpace(oldCompany) )
            //        oldCompany = company;
            //    if (company != oldCompany)
            //    {
            //        oldCompany = company;
            //        if (i > 0)
            //            dt.Rows[i - 1]["persons"] = 1D;
            //        oldPayer = "";
            //    }
            //    payer = dt.Rows[i]["payer"].ObjToString().Trim();
            //    if (string.IsNullOrWhiteSpace(oldPayer))
            //        oldPayer = payer;
            //    if (oldPayer != payer)
            //    {
            //        if (i > 0)
            //            dt.Rows[i - 1]["persons"] = 1D;
            //        oldPayer = payer;
            //    }
            //}
            //if (lastRow >= 0)
            //{
            //    lastRow = dt.Rows.Count - 1;
            //    dt.Rows[lastRow]["persons"] = 1D;
            //}


            //dRows = null;
            //companyCode = "";
            //double liability = 0D;
            //double totalLiability = 0D;
            //double premium = 0D;
            //double totalPremium = 0D;
            //DataTable ddd = null;
            //payer = "";
            //string oldPayer = "";

            //try
            //{
            //    if (G1.get_column_number(dt, "found") < 0)
            //        dt.Columns.Add("found", Type.GetType("System.Double"));
            //    for ( int i=0; i<dt.Rows.Count; i++)
            //    {
            //        dt.Rows[i]["found"] = 0D;
            //    }

            //    for (int i = 0; i < companyDt.Rows.Count; i++)
            //    {
            //        companyCode = companyDt.Rows[i]["company"].ObjToString();
            //        dRows = dt.Select("companyCode='" + companyCode + "'");
            //        if (dRows.Length <= 0)
            //            continue;

            //        liability = 0D;
            //        totalLiability = 0D;
            //        premium = 0D;
            //        totalPremium = 0D;
            //        persons = 0D;
            //        policies = 0D;

            //        for (int j = 0; j < dRows.Length; j++)
            //        {
            //            persons += dRows[j]["persons"].ObjToDouble();
            //            policies += dRows[j]["policies"].ObjToDouble();
            //            totalLiability += dRows[j]["liability"].ObjToDouble();
            //            totalPremium += dRows[j]["premium"].ObjToDouble();
            //            dRows[j]["found"] = 1D;
            //            dRows[j]["policies"] = 1D;
            //        }

            //        ddd = dRows.CopyToDataTable();

            //        tempview = ddd.DefaultView;
            //        tempview.Sort = "payer asc";
            //        ddd = tempview.ToTable();
            //        persons = 0;
            //        oldPayer = "";

            //        for (int j = 0; j < ddd.Rows.Count; j++)
            //            persons += ddd.Rows[j]["persons"].ObjToDouble();


            //        // for (int j = 0; j < ddd.Rows.Count; j++)
            //        //{
            //        //    payer = ddd.Rows[j]["payer"].ObjToString();
            //        //    if (String.IsNullOrWhiteSpace(oldPayer))
            //        //        oldPayer = payer;
            //        //    if (oldPayer == payer)
            //        //        continue;
            //        //    if (oldPayer != payer)
            //        //    {
            //        //        persons++;
            //        //        oldPayer = payer;
            //        //    }
            //        //}

            //        companyDt.Rows[i]["persons"] = persons;
            //        //companyDt.Rows[i]["policies"] = policies;
            //        companyDt.Rows[i]["policies"] = dRows.Length;
            //        companyDt.Rows[i]["liability"] = totalLiability;
            //        companyDt.Rows[i]["premium"] = totalPremium;
            //    }
            //}
            //catch ( Exception ex )
            //{
            //}

            //dRows = dt.Select("found = '0'");
            //if (dRows.Length > 0)
            //{
            //    ddd = dRows.CopyToDataTable();
            //    for (int i = 0; i < dRows.Length; i++)
            //        dRows[i]["companyCode"] = "Bad";
            //}

            dgv.Hide();
            dgv.DataSource = companyDt;


            G1.NumberDataTable(dt);

            if (G1.get_column_number(dt, "OriginalRow") < 0)
                dt.Columns.Add("OriginalRow");
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["OriginalRow"] = dt.Rows[i]["num"].ObjToString();

            dgv2.DataSource = dt;
            dgv2.Show();

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
        private void LoadDeceased()
        {
            this.Cursor = Cursors.WaitCursor;

            DateTime date = this.dateTimePicker1.Value;
            string date1 = date.ToString("yyyy-MM-dd");

            date = this.dateTimePicker2.Value;
            string date2 = date.ToString("yyyy-MM-dd");

            this.Text = date.Year.ToString() + " Class \"A\" Death Claims";

            string companyCode = txtCompanyCode.Text.Trim();

            string cmd = "Select * from `icustomers` p JOIN `icontracts` x ON p.`contractNumber` = x.`contractNumber` ";
            cmd += " JOIN `policies` q ON p.`contractNumber` = q.`contractNumber` ";
            cmd += " LEFT JOIN `payers` c ON q.`payer` = c.`payer` ";
            //cmd += " WHERE q.`deceasedDate` > '1900-01-01' ";
            //cmd += " WHERE ( q.`deceasedDate` > '1800-01-01' OR c.`deceasedDate` > '1800-01-01' ) ";
            cmd += " WHERE ( q.`deceasedDate` >= '" + date1 + "' AND q.`deceasedDate` <= '" + date2 + "' ) ";
            cmd += " AND ( q.`liability` >= '0.00' AND q.`liability` <= '450.00' ) ";
            cmd += " AND q.`report` = 'Not Third Party' ";
            //cmd += " AND x.`dueDate8` >= '" + date1 + "' ";
            if (!String.IsNullOrWhiteSpace(companyCode))
                cmd += " AND q.`companyCode` = '" + companyCode + "' ";
            cmd += " ORDER by q.`payer`, c.`contractNumber` asc";
            cmd += ";";

            //cmd = "SELECT * FROM payers c JOIN policies q ON c.`contractNumber` = q.`contractNumber` WHERE c.`lapseDate8` >= '" + date1 + "' AND c.`lapseDate8` <= '" + date2 + "' AND c.`reinstateDate8` <= c.`lapseDate8` ";
            //cmd += " AND q.deceasedDate > '1000-01-01' ";
            //cmd += " AND q.`report` = 'Not Third Party' ";

            DataTable dt2 = G1.get_db_data(cmd);

            if (G1.get_column_number(dt2, "ddate") < 0)
                dt2.Columns.Add("ddate");
            if (G1.get_column_number(dt2, "duedate") < 0)
                dt2.Columns.Add("duedate");

            CustomerDetails.FixOrphanPolicies2(dt2);
            dt2 = FilterInactive(dt2);

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

            string payer = "";
            DataRow[] dRows = null;
            DataTable dd = null;
            DateTime lDate = DateTime.Now;
            TimeSpan ts;

            DataRow[] dRows2 = dt2.Select("payer='200138'");
            if (dRows2.Length > 0)
            {
                DataTable ddx = dRows2.CopyToDataTable();
            }


            try
            {
                dt.Columns.Add("remove");
                //dt.Columns.Add("ddate");
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    payer = dt.Rows[i]["payer"].ObjToString();
                    if ( payer == "100190")
                    {
                    }
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
                    //                            dRows[j]["remove"] = 1; // Policy is more than 190 days lapsed so consider it dead;
                    //                        else
                    //                            dRows[j]["remove"] = 0; // Policy can be reinstated so do not include as a deceased policy
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
                        if (date >= this.dateTimePicker1.Value && date <= this.dateTimePicker2.Value) // Policy is Deceased
                        {
                            dt.Rows[i]["remove"] = "1";
                            dt.Rows[i]["ddate"] = date.ToString("yyyy-MM-dd");
                        }
                        else
                            dt.Rows[i]["remove"] = "0";
                    //}
                }
            }
            catch ( Exception ex)
            {
            }

            DataRow[] xRows = dt.Select("remove = '1'");
            if (xRows.Length > 0)
                dt = xRows.CopyToDataTable();


            if (G1.get_column_number(dt, "persons") < 0)
                dt.Columns.Add("persons", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "policies") < 0)
                dt.Columns.Add("policies", Type.GetType("System.Double"));

            dt = LoadUniquePerPayer(dt, companyDt);


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
                payer = dt.Rows[i]["payer"].ObjToString();
                if ( payer == "100004")
                {
                }
                policy = dt.Rows[i]["policyNumber"].ObjToString();
                if (String.IsNullOrWhiteSpace(oldPolicy))
                    oldPolicy = policy;
                dt.Rows[i]["persons"] = 0D;
                dt.Rows[i]["policies"] = 0D;
                dt2.Rows[i]["policies"] = 1D;
                if (oldPolicy == policy)
                    continue;
                if (oldPolicy != policy)
                {
                    if (i >= 0)
                        dt.Rows[i - 1]["policies"] = 1D;
                    oldPolicy = policy;
                }
            }
            int lastRow = dt.Rows.Count - 1;
            dt.Rows[lastRow]["policies"] = 1D;
            dt2.Rows[lastRow]["policies"] = 1D;

            tempview = dt.DefaultView;
            tempview.Sort = "payer asc";
            dt = tempview.ToTable();

            string oldPayer = "";
            payer = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                payer = dt.Rows[i]["payer"].ObjToString().Trim();
                if (string.IsNullOrWhiteSpace(oldPayer))
                    oldPayer = payer;
                if (oldPayer == payer)
                    continue;
                if (i >= 0)
                {
                    dt.Rows[i - 1]["persons"] = 1D;
                }
                oldPayer = payer;
            }
            lastRow = dt.Rows.Count - 1;
            dt.Rows[lastRow]["persons"] = 1D;

            //tempview = dt.DefaultView;
            //tempview.Sort = "companyCode asc, payer asc";
            //dt = tempview.ToTable();

            //oldPayer = "";
            //string oldCompany = "";
            //string company = "";

            //for (int i = 0; i < dt.Rows.Count; i++)
            //{
            //    company = dt.Rows[i]["companyCode"].ObjToString();
            //    if (String.IsNullOrWhiteSpace(oldCompany))
            //        oldCompany = company;
            //    if ( company == "BFIC")
            //    {
            //    }
            //    if (company != oldCompany)
            //    {
            //        oldCompany = company;
            //        if (i > 0)
            //            dt.Rows[i - 1]["persons"] = 1D;
            //        oldPayer = "";
            //    }
            //    payer = dt.Rows[i]["payer"].ObjToString().Trim();
            //    if ( payer == "200189")
            //    {
            //    }
            //    if (string.IsNullOrWhiteSpace(oldPayer))
            //        oldPayer = payer;
            //    if (oldPayer != payer)
            //    {
            //        if (i > 0)
            //            dt.Rows[i - 1]["persons"] = 1D;
            //        oldPayer = payer;
            //    }
            //}
            //if (lastRow >= 0)
            //{
            //    lastRow = dt.Rows.Count - 1;
            //    dt.Rows[lastRow]["persons"] = 1D;
            //}



            dRows = null;
            companyCode = "";
            double liability = 0D;
            double totalLiability = 0D;
            double premium = 0D;
            double totalPremium = 0D;
            DateTime deceasedDate = DateTime.Now;
            DataTable ddd = null;
            payer = "";
            oldPayer = "";

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
                        deceasedDate = dRows[j]["deceasedDate3"].ObjToDateTime();
                        if ( deceasedDate.Year > 1000 )
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

                    //for (int j = 0; j < ddd.Rows.Count; j++)
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

            dRows = dt.Select("found = '0'");
            if (dRows.Length > 0)
            {
                ddd = dRows.CopyToDataTable();
                for (int i = 0; i < dRows.Length; i++)
                    dRows[i]["companyCode"] = "Bad";
            }

            dgv.Hide();
            dgv.DataSource = companyDt;

            G1.NumberDataTable(dt);
            dgv2.DataSource = dt;
            dgv2.Show();

            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void LoadLapsed()
        {
            this.Cursor = Cursors.WaitCursor;

            DateTime date = this.dateTimePicker1.Value;
            string date1 = date.ToString("yyyy-MM-dd");

            date = this.dateTimePicker2.Value;
            string date2 = date.ToString("yyyy-MM-dd");

            this.Text = date.Year.ToString() + " Class \"A\" Lapsed Policies";

            string cmd = "Select * from `icustomers` p JOIN `icontracts` x ON p.`contractNumber` = x.`contractNumber` ";
            cmd += " JOIN `policies` q ON p.`contractNumber` = q.`contractNumber` ";
            cmd += " LEFT JOIN `payers` c ON q.`payer` = c.`payer` ";
            cmd += " WHERE q.`report` = 'Not Third Party' ";
            cmd += " AND c.`dueDate8` >= '" + date1 + "' ";
            //cmd += " AND ( c.`deceasedDate` < '1000-01-01' OR q.`deceasedDate` < '1000-01-01' )";
            cmd += " AND ( c.`deceasedDate` < '1000-01-01' AND p.`deceasedDate` < '1000-01-01' )";
            cmd += " AND ( c.`lapsed` = 'Y' OR q.`lapsedDate8` >= '" + date1 + "' ) ";
            //cmd += " AND ( c.`lapsed` = 'Y' OR q.`lapsed` = 'Y' ) ";
            cmd += " AND ( q.`liability` >= '0.00' AND q.`liability` <= '450.00' ) ";
            //cmd += " WHERE q.`deceasedDate` > '1999-01-01' ";
            //cmd += " WHERE ( c.`reinstateDate8` < q.`lapsedDate8` AND ( q.`lapsedDate8` >= '" + date1 + "' AND q.`lapsedDate8` <= '" + date2 + "' ) ) ";
            //cmd += " OR (c.`reinstateDate8` < q.`lapsedDate8` AND(c.`lapseDate8` >= '" + date1 + "' AND c.`lapseDate8` <= '" + date2 + "') ) ";

            //cmd = "SELECT * FROM payers c JOIN policies q ON c.`contractNumber` = q.`contractNumber` WHERE c.`lapseDate8` >= '" + date1 + "' AND c.`lapseDate8` <= '" + date2 + "' AND c.`reinstateDate8` <= c.`lapseDate8` ";
            //cmd += " AND q.deceasedDate < '1000-01-01' ";
            //cmd += " AND q.`report` = 'Not Third Party' ";

            string companyCode = txtCompanyCode.Text.Trim();
            if ( !String.IsNullOrWhiteSpace ( companyCode ))
            {
                cmd = "SELECT * FROM payers c JOIN policies q ON c.`contractNumber` = q.`contractNumber` WHERE q.companyCode = '" + companyCode + "' AND c.`lapseDate8` >= '" + date1 + "' AND c.`lapseDate8` <= '" + date2 + "' AND c.`reinstateDate8` <= c.`lapseDate8` ";
                cmd += " AND q.deceasedDate < '1000-01-01' ";
                cmd += " AND q.`report` = 'Not Third Party' ";

                //cmd = "Select * from `icustomers` p JOIN `icontracts` x ON p.`contractNumber` = x.`contractNumber` ";
                //cmd += " JOIN `policies` q ON p.`contractNumber` = q.`contractNumber` ";
                //cmd += " LEFT JOIN `payers` c ON q.`payer` = c.`payer` ";

                //cmd += " WHERE ( c.`reinstateDate8` < q.`lapsedDate8` AND ( q.`lapsedDate8` >= '" + date1 + "' AND q.`lapsedDate8` <= '" + date2 + "' ) AND q.`companyCode` = '" + companyCode + "' ) ";
                //cmd += " OR (c.`reinstateDate8` < q.`lapsedDate8` AND (c.`lapseDate8` >= '" + date1 + "' AND c.`lapseDate8` <= '" + date2 + "') AND q.`companyCode` = '" + companyCode + "' ) ";


                //cmd += " AND ( q.`companyCode` = '" + companyCode + "' ) ";
            }
            string payerCode = txtPayer.Text.Trim();
            if ( !String.IsNullOrWhiteSpace ( payerCode ))
            {
                cmd += " AND q.`payer` = '" + payerCode + "' ";
            }
            cmd += " ORDER by q.`payer` ";
            cmd += ";";
            DataTable dt2 = null;
            try
            {
                dt2 = G1.get_db_data(cmd);
            }
            catch ( Exception ex)
            {
            }

            //DataRow[] dRows2 = dt2.Select("payer='100040'");
            //if (dRows2.Length > 0)
            //{
            //    DataTable ddx = dRows2.CopyToDataTable();
            //}

            dt2.Columns.Add("remove");

            //Trust85.FindContract(dt2, "ZZ0034238"); // Lapsed

            DataRow[] dddRow = dt2.Select("payer='BB-0081T'");
            if (dddRow.Length > 0)
            {
                DataTable dddd = dddRow.CopyToDataTable();
            }


            string lapsed3 = "";
            string payer = "";

            for ( int i=0; i<dt2.Rows.Count; i++)
            {
                payer = dt2.Rows[i]["payer"].ObjToString();
                if ( payer == "BB-0081T")
                {
                }
                date = dt2.Rows[i]["deceasedDate3"].ObjToDateTime();
                if (date.Year > 1000)
                    dt2.Rows[i]["remove"] = "1";
                else
                {
                    date = dt2.Rows[i]["deceasedDate2"].ObjToDateTime();
                    if (date.Year > 1000)
                        dt2.Rows[i]["remove"] = "1";
                    else
                        dt2.Rows[i]["remove"] = "0";
                }
                lapsed3 = dt2.Rows[i]["lapsed3"].ObjToString().ToUpper();
                if ( lapsed3 == "Y" )
                {
                    if (dt2.Rows[i]["remove"].ObjToString() != "Y")
                    {
                        date = dt2.Rows[i]["deceasedDate3"].ObjToDateTime();
                        if (date.Year < 1000)
                            dt2.Rows[i]["remove"] = "0";
                    }
                }
            }

            DataRow[] xRows = dt2.Select("remove = '0'");
            if (xRows.Length > 0)
                dt2 = xRows.CopyToDataTable();

            Trust85.FindContract(dt2, "ZZ0014252");


            if (G1.get_column_number(dt2, "ddate") < 0)
                dt2.Columns.Add("ddate");
            if (G1.get_column_number(dt2, "duedate") < 0)
                dt2.Columns.Add("duedate");

            CustomerDetails.FixOrphanPolicies2(dt2);
            dt2 = FilterInactive(dt2);

            //dt2 = LoadLapsedPayers ( dt2, companyCode );

            dt2.Columns.Add("persons", Type.GetType("System.Double"));
            dt2.Columns.Add("policies", Type.GetType("System.Double"));

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
                dt.Rows[i]["persons"] = 0D;
                dt2.Rows[i]["policies"] = 1D;
                dt.Rows[i]["policies"] = 0D;
                if (oldPolicy == policy)
                    continue;
                if (oldPolicy != policy)
                {
                    if (i >= 0)
                        dt.Rows[i - 1]["policies"] = 1D;
                    oldPolicy = policy;
                }
            }
            int lastRow = dt.Rows.Count - 1;
            if (lastRow >= 0)
            {
                dt.Rows[lastRow]["policies"] = 1D;
                dt2.Rows[lastRow]["policies"] = 1D;
            }

            dt = LoadUniquePerPayer(dt, companyDt);


            //tempview = dt.DefaultView;
            //tempview.Sort = "companyCode asc, payer asc";
            //dt = tempview.ToTable();

            //string oldPayer = "";
            //string oldCompany = "";
            //string company = "";

            //for (int i = 0; i < dt.Rows.Count; i++)
            //{
            //    company = dt.Rows[i]["companyCode"].ObjToString();
            //    if (String.IsNullOrWhiteSpace(oldCompany))
            //        oldCompany = company;
            //    if (company != oldCompany)
            //    {
            //        oldCompany = company;
            //        if (i > 0)
            //            dt.Rows[i - 1]["persons"] = 1D;
            //        oldPayer = "";
            //    }
            //    payer = dt.Rows[i]["payer"].ObjToString().Trim();
            //    if (string.IsNullOrWhiteSpace(oldPayer))
            //        oldPayer = payer;
            //    if (oldPayer != payer)
            //    {
            //        if (i > 0)
            //            dt.Rows[i - 1]["persons"] = 1D;
            //        oldPayer = payer;
            //    }
            //}
            //if (lastRow >= 0)
            //{
            //    lastRow = dt.Rows.Count - 1;
            //    dt.Rows[lastRow]["persons"] = 1D;
            //}


            //tempview = dt.DefaultView;
            //tempview.Sort = "payer asc";
            //dt = tempview.ToTable();

            //string oldPayer = "";
            //payer = "";
            //for (int i = 0; i < dt.Rows.Count; i++)
            //{
            //    payer = dt.Rows[i]["payer"].ObjToString().Trim();
            //    if (string.IsNullOrWhiteSpace(oldPayer))
            //        oldPayer = payer;
            //    if (oldPayer == payer)
            //        continue;
            //    if (i >= 0)
            //    {
            //        dt.Rows[i - 1]["persons"] = 1D;
            //    }
            //    oldPayer = payer;
            //}
            //lastRow = dt.Rows.Count - 1;
            //dt.Rows[lastRow]["persons"] = 1D;


            //DataRow[] dRows = null;
            //companyCode = "";
            //double liability = 0D;
            //double totalLiability = 0D;
            //double premium = 0D;
            //double totalPremium = 0D;
            //DataTable ddd = null;
            //payer = "";
            //oldPayer = "";

            //try
            //{
            //    if (G1.get_column_number(dt, "found") < 0)
            //        dt.Columns.Add("found", Type.GetType("System.Double"));
            //    for (int i = 0; i < dt.Rows.Count; i++)
            //    {
            //        dt.Rows[i]["found"] = 0D;
            //    }


            //    for (int i = 0; i < companyDt.Rows.Count; i++)
            //    {
            //        companyCode = companyDt.Rows[i]["company"].ObjToString();
            //        dRows = dt.Select("companyCode='" + companyCode + "'");
            //        if (dRows.Length <= 0)
            //            continue;

            //        if ( companyCode == "CC" )
            //        {
            //        }

            //        liability = 0D;
            //        totalLiability = 0D;
            //        premium = 0D;
            //        totalPremium = 0D;
            //        persons = 0D;
            //        policies = 0D;

            //        for (int j = 0; j < dRows.Length; j++)
            //        {
            //            persons += dRows[j]["persons"].ObjToDouble();
            //            policies += dRows[j]["policies"].ObjToDouble();
            //            totalLiability += dRows[j]["liability"].ObjToDouble();
            //            totalPremium += dRows[j]["premium"].ObjToDouble();
            //            dRows[j]["found"] = 1D;
            //            dRows[j]["policies"] = 1D;
            //        }

            //        ddd = dRows.CopyToDataTable();

            //        tempview = ddd.DefaultView;
            //        tempview.Sort = "payer asc";
            //        ddd = tempview.ToTable();
            //        persons = 0;
            //        oldPayer = "";

            //        for (int j = 0; j < ddd.Rows.Count; j++)
            //            persons += ddd.Rows[j]["persons"].ObjToDouble();

            //        //for (int j = 0; j < ddd.Rows.Count; j++)
            //        //{
            //        //    payer = ddd.Rows[j]["payer"].ObjToString();
            //        //    if (String.IsNullOrWhiteSpace(oldPayer))
            //        //        oldPayer = payer;
            //        //    if (oldPayer == payer)
            //        //        continue;
            //        //    if (oldPayer != payer)
            //        //    {
            //        //        persons++;
            //        //        oldPayer = payer;
            //        //    }
            //        //}



            //        companyDt.Rows[i]["persons"] = persons;
            //        //companyDt.Rows[i]["policies"] = policies;
            //        companyDt.Rows[i]["policies"] = dRows.Length;
            //        companyDt.Rows[i]["liability"] = totalLiability;
            //        companyDt.Rows[i]["premium"] = totalPremium;
            //    }
            //}
            //catch (Exception ex)
            //{
            //}

            DataRow [] dRows = dt.Select("found = '0'");
            if (dRows.Length > 0)
            {
                //ddd = dRows.CopyToDataTable();
                for (int i = 0; i < dRows.Length; i++)
                    dRows[i]["companyCode"] = "Bad";
            }

            dgv.Hide();
            dgv.DataSource = companyDt;

            tempview = dt.DefaultView;
            tempview.Sort = "contractNumber asc, payer asc";
            dt = tempview.ToTable();


            G1.NumberDataTable(dt);
            dgv2.DataSource = dt;
            dgv2.Show();

            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private DataTable LoadLapsedPayers ( DataTable dt, string companyCode )
        {
            string cmd = "";
            DateTime lapseDate = DateTime.Now;
            string payer = "";
            string oldPayer = "";
            string lastPayer = "";
            DataTable backupDt = dt.Copy();
            DataTable dx = null;
            DataRow dRow = null;
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                payer = dt.Rows[i]["payer"].ObjToString();
                if (payer == oldPayer)
                    continue;
                lapseDate = dt.Rows[i]["lapseDate81"].ObjToDateTime();
                if ( lapseDate.Year > 100 )
                {
                    cmd = "Select * from `policies` where `payer` = '" + payer + "' ";
                    if (!String.IsNullOrWhiteSpace(companyCode))
                        cmd += " AND `companyCode` = '" + companyCode + "' ";
                    cmd += ";";
                    dx = G1.get_db_data(cmd);
                    for ( int j=0; j<dx.Rows.Count; j++)
                    {
                        dRow = backupDt.NewRow();
                        dRow["payer"] = payer;
                        dRow["companyCode"] = dx.Rows[j]["companyCode"].ObjToString();
                        dRow["premium"] = dx.Rows[j]["premium"].ObjToDouble();
                        dRow["liability"] = dx.Rows[j]["liability"].ObjToDouble();
                        dRow["lapsedDate8"] = G1.DTtoMySQLDT(lapseDate);
                        dRow["contractNumber"] = dx.Rows[j]["contractNumber"].ObjToString();
                        dRow["policyNumber"] = dx.Rows[j]["policyNumber"].ObjToString();
                        backupDt.Rows.Add(dRow);
                        oldPayer = payer;
                    }
                }
            }
            return backupDt;
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
            if (this.components == null)
                this.components = new System.ComponentModel.Container();
            DevExpress.XtraPrinting.PrintingSystem printingSystem1 = new DevExpress.XtraPrinting.PrintingSystem(this.components);
            DevExpress.XtraPrinting.PrintableComponentLink printableComponentLink1 = new DevExpress.XtraPrinting.PrintableComponentLink(this.components);

            printingSystem1.Links.AddRange(new object[] {
            printableComponentLink1});

            printableComponentLink1.Component = dgv;
            if ( dgv2.Visible )
                printableComponentLink1.Component = dgv2;
            printableComponentLink1.PrintingSystemBase = printingSystem1;
            printableComponentLink1.CreateDetailHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateDetailHeaderArea);
            printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);
            printableComponentLink1.BeforeCreateAreas += new System.EventHandler(this.printableComponentLink1_BeforeCreateAreas);
            printableComponentLink1.AfterCreateAreas += new System.EventHandler(this.printableComponentLink1_AfterCreateAreas);
            printableComponentLink1.Landscape = true;

            //Printer.setupPrinterMargins(50, 100, 110, 50);

            Printer.setupPrinterMargins(30, 30, 90, 50);


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
            if (this.components == null)
                this.components = new System.ComponentModel.Container();
            DevExpress.XtraPrinting.PrintingSystem printingSystem1 = new DevExpress.XtraPrinting.PrintingSystem(this.components);
            DevExpress.XtraPrinting.PrintableComponentLink printableComponentLink1 = new DevExpress.XtraPrinting.PrintableComponentLink(this.components);

            printingSystem1.Links.AddRange(new object[] {
            printableComponentLink1});

            printableComponentLink1.Component = dgv;
            if (dgv2.Visible)
                printableComponentLink1.Component = dgv2;
            printableComponentLink1.PrintingSystemBase = printingSystem1;
            printableComponentLink1.CreateDetailHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateDetailHeaderArea);
            printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);
            printableComponentLink1.BeforeCreateAreas += new System.EventHandler(this.printableComponentLink1_BeforeCreateAreas);
            printableComponentLink1.AfterCreateAreas += new System.EventHandler(this.printableComponentLink1_AfterCreateAreas);
            printableComponentLink1.Landscape = true;

            //Printer.setupPrinterMargins(50, 100, 110, 50);
            Printer.setupPrinterMargins(30, 30, 90, 50);

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

            DateTime date = this.dateTimePicker1.Value;
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
    }
}