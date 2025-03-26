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
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class ReImportRiles : DevExpress.XtraEditors.XtraForm
    {
        private bool modified = false;
        private bool loading = true;
        private DataTable rilesDt = null;
        /****************************************************************************************/
        public ReImportRiles(DataTable dt)
        {
            InitializeComponent();

            rilesDt = dt;

            SetupTotalsSummary();
        }
        /****************************************************************************************/
        private void ReImportRiles_Load(object sender, EventArgs e)
        {
            btnSaveAll.Hide();
        }
        /****************************************************************************************/
        private void SetupTotalsSummary()
        {
            AddSummaryColumn("beginningBalance", null);
            AddSummaryColumn("newTBB", null);
            AddSummaryColumn("paymentCurrMonth", null);
            AddSummaryColumn("newPPIT", null);

            AddSummaryColumn("diffTBB", null);
            AddSummaryColumn("diffPPIT", null);

            AddSummaryColumn("newRefunds", null);
            AddSummaryColumn("newRemovals", null);
            AddSummaryColumn("refundRemCurrMonth", null);
            AddSummaryColumn("deathRemCurrMonth", null);

            AddSummaryColumn("endingBalance", null);
            AddSummaryColumn("newEB", null);
        }
        /****************************************************************************************/
        private void AddSummaryColumn(string columnName, DevExpress.XtraGrid.Views.Grid.GridView gMain = null)
        {
            if (gMain == null)
                gMain = gridMain;
            //            gMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            gMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            //            gMain.Columns[columnName].SummaryItem.DisplayFormat = "${0:0,0.00}";
            gMain.Columns[columnName].SummaryItem.DisplayFormat = "{0:N2}";
        }
        /****************************************************************************************/
        private void pictureBox12_Click(object sender, EventArgs e)
        { // Add New Row
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dRow = dt.NewRow();
            dt.Rows.Add(dRow);
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            gridMain.RefreshData();
            gridMain_CellValueChanged(null, null);
        }
        /****************************************************************************************/
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (loading)
                return;
            //modified = true;
            //btnSaveAll.Show();
            //btnSaveAll.Refresh();
            //DataRow dr = gridMain.GetFocusedDataRow();
            //dr["mod"] = "Y";
        }
        /****************************************************************************************/
        private void gridMain_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            if (!chkShowDiff.Checked)
                return;
            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt == null)
                return;
            string delete = dt.Rows[row]["mod"].ObjToString();
            if (delete.ToUpper() != "S")
            {
                e.Visible = false;
                e.Handled = true;
            }
        }
        /****************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        { // Spy Glass
            G1.SpyGlass(gridMain);
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
            if (!btnSaveAll.Visible)
                return;
            DialogResult result = MessageBox.Show("***Question*** Data has been modified.\nDo you really want to exit WITHOUT saving your data?", "Data Modified Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
                return;
            e.Cancel = true;
        }
        /****************************************************************************************/
        private void gridMain_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column.FieldName.ToUpper().IndexOf("DATE") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                DateTime date = e.DisplayText.ObjToDateTime();
                if (date.Year < 1000)
                    e.DisplayText = "";
                else
                    e.DisplayText = date.ToString("MM/dd/yyyy");
            }
        }
        /****************************************************************************************/
        private void gridMain6_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column.FieldName.ToUpper().IndexOf("DATE") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                DateTime date = e.DisplayText.ObjToDateTime();
                if (date.Year < 1000)
                    e.DisplayText = "";
                else
                    e.DisplayText = date.ToString("MM/dd/yyyy");
            }
        }
        /****************************************************************************************/
        private void btnRun_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            btnSaveAll.Hide();
            btnSaveAll.Refresh();

            string cmd = "Select * from `trust2013r` a JOIN `customers` c ON a.`contractNumber` = c.`contractNumber` ";
            cmd += " JOIN `contracts` x ON a.`contractNumber` = x.`contractNumber` ";
            //if (!String.IsNullOrWhiteSpace(contract))
            //    cmd += " AND a.`contractNumber` = '" + contract + "' ";

            DateTime date = this.dateTimePicker2.Value;

            string date1 = date.Year.ToString("D4") + "-" + date.Month.ToString("D2") + "-01 00:00:00";
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            string date2 = date.Year.ToString("D4") + "-" + date.Month.ToString("D2") + "-" + days.ToString("D2") + " 23:59:59";

            cmd += " AND `payDate8` >= '" + date1 + "' AND `payDate8` <= '" + date2 + "' ";

            cmd += " AND a.`riles` = 'Y' ";
            cmd += ";";

            DataTable dt = G1.get_db_data(cmd);

            dt.Columns.Add("newContract");
            dt.Columns.Add("newTBB", Type.GetType("System.Double"));
            dt.Columns.Add("diffTBB", Type.GetType("System.Double"));
            dt.Columns.Add("payDate");
            dt.Columns.Add("fix");
            dt.Columns.Add("newPPIT", Type.GetType("System.Double"));
            dt.Columns.Add("diffPPIT", Type.GetType("System.Double"));
            dt.Columns.Add("newRefunds", Type.GetType("System.Double"));
            dt.Columns.Add("newRemovals", Type.GetType("System.Double"));
            dt.Columns.Add("newEB", Type.GetType("System.Double"));
            dt.Columns.Add("mod");




            string tbbDate = date.ToString("MM/01/yyyy") + " TBB";

            DataRow[] tbbRows = null;
            string contractNumber = "";
            string columnNameDate = "";
            string columnNameAmount = "";
            string columnNameOutside = "";
            string columnNameRemoval = "";
            double tbb = 0D;
            double diff = 0D;
            DataRow dRow = null;

            DataTable paymentDt = new DataTable();
            paymentDt.Columns.Add("Num");
            paymentDt.Columns.Add("contractNumber");

            DataTable dateDt = new DataTable();
            dateDt.Columns.Add("Num");
            dateDt.Columns.Add("contractNumber");

            DataTable outsideDt = new DataTable();
            outsideDt.Columns.Add("Num");
            outsideDt.Columns.Add("contractNumber");

            DataTable removalDt = new DataTable();
            removalDt.Columns.Add("Num");
            removalDt.Columns.Add("contractNumber");

            for (int i = 0; i < rilesDt.Rows.Count; i++)
            {
                contractNumber = "RF" + rilesDt.Rows[i]["contract"].ObjToString();
                if (contractNumber == "RF")
                    continue;
                dRow = paymentDt.NewRow();
                dRow["contractNumber"] = contractNumber;
                paymentDt.Rows.Add(dRow);

                dRow = dateDt.NewRow();
                dRow["contractNumber"] = contractNumber;
                dateDt.Rows.Add(dRow);

                dRow = outsideDt.NewRow();
                dRow["contractNumber"] = contractNumber;
                outsideDt.Rows.Add(dRow);

                dRow = removalDt.NewRow();
                dRow["contractNumber"] = contractNumber;
                removalDt.Rows.Add(dRow);
            }

            G1.NumberDataTable(removalDt);
            G1.NumberDataTable(outsideDt);
            G1.NumberDataTable(paymentDt);
            G1.NumberDataTable(dateDt);

            int count = 57;
            bool gotit = false;
            DateTime iDate = DateTime.Now;
            double iPayment = 0D;
            string monthYear = "";
            for (; ; )
            {
                gotit = false;
                count++;
                columnNameDate = "Installment Payment " + count.ToString() + " Date";
                columnNameAmount = "Installment Payment " + count.ToString() + " Amount";
                try
                {
                    for (int j = 0; j < rilesDt.Rows.Count; j++)
                    {
                        iDate = rilesDt.Rows[j][columnNameDate].ObjToDateTime();
                        if (iDate.Year < 1500)
                            continue;
                        if (!gotit)
                        {
                            monthYear = iDate.Month.ToString("D2") + "/" + iDate.Year.ToString("D4");
                            paymentDt.Columns.Add(monthYear, Type.GetType("System.Double"));
                            dateDt.Columns.Add(monthYear);
                            gotit = true;
                        }
                        iPayment = rilesDt.Rows[j][columnNameAmount].ObjToDouble();
                        paymentDt.Rows[j][monthYear] = iPayment;
                        dateDt.Rows[j][monthYear] = iDate.ToString("MM/dd/yyyy");
                    }
                }
                catch (Exception ex)
                {
                    break;
                }
            }

            string columnName = "";
            for (int i = 0; i < rilesDt.Columns.Count; i++)
            {
                columnNameOutside = rilesDt.Columns[i].ColumnName.Trim();
                if (columnNameOutside.IndexOf("Outside Claims") > 0)
                {
                    try
                    {
                        columnNameOutside = columnNameOutside.Replace("Outside Claims", "").Trim();
                        columnName = columnNameOutside.ObjToDateTime().ToString("MM/yyyy");
                        outsideDt.Columns.Add(columnName, Type.GetType("System.Double"));
                        for (int j = 0; j < dateDt.Rows.Count; j++)
                        {
                            try
                            {
                                iPayment = rilesDt.Rows[j][i].ObjToDouble();
                                if (iPayment != 0D)
                                    outsideDt.Rows[j][columnName] = iPayment;
                            }
                            catch (Exception ex)
                            {
                                break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        break;
                    }
                }

            }

            columnName = "";
            for (int i = 0; i < rilesDt.Columns.Count; i++)
            {
                columnNameRemoval = rilesDt.Columns[i].ColumnName.Trim();
                if (columnNameRemoval.IndexOf("Removals") > 0)
                {
                    try
                    {
                        columnNameRemoval = columnNameRemoval.Replace("Removals", "").Trim();
                        columnName = columnNameRemoval.ObjToDateTime().ToString("MM/yyyy");
                        removalDt.Columns.Add(columnName, Type.GetType("System.Double"));
                        for (int j = 0; j < dateDt.Rows.Count; j++)
                        {
                            try
                            {
                                iPayment = rilesDt.Rows[j][i].ObjToDouble();
                                if (iPayment != 0D)
                                    removalDt.Rows[j][columnName] = iPayment;
                            }
                            catch (Exception ex)
                            {
                                break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        break;
                    }
                }

            }

            dgv2.DataSource = paymentDt;
            dgv3.DataSource = dateDt;
            dgv4.DataSource = outsideDt;
            dgv5.DataSource = removalDt;

            double refund = 0D;
            double removal = 0D;
            double newEB = 0D;

            DateTime beginningDate = new DateTime(2020, 1, 1);
            int months = G1.GetMonthsBetween(beginningDate, date);
            months += 58;
            string tbbPayments = "Installment Payment " + months.ToString() + " Amount";
            double payments = 0D;
            string payDate = "";
            monthYear = date.Month.ToString("D2") + "/" + date.Year.ToString("D4");

            try
            {
                for (int i = 0; i < rilesDt.Rows.Count; i++)
                {
                    try
                    {
                        contractNumber = rilesDt.Rows[i]["contract"].ObjToString();
                        if (String.IsNullOrWhiteSpace(contractNumber))
                            continue;
                        tbb = rilesDt.Rows[i][tbbDate].ObjToDouble();
                        payments = 0D;
                        refund = 0D;
                        removal = 0D;
                        payDate = "";
                        if (G1.get_column_number(paymentDt, monthYear) >= 0)
                            payments = paymentDt.Rows[i][monthYear].ObjToDouble();
                        if (G1.get_column_number(dateDt, monthYear) >= 0)
                            payDate = dateDt.Rows[i][monthYear].ObjToString();
                        if (G1.get_column_number(outsideDt, monthYear) >= 0)
                            refund = outsideDt.Rows[i][monthYear].ObjToDouble();
                        if (G1.get_column_number(removalDt, monthYear) >= 0)
                            removal = removalDt.Rows[i][monthYear].ObjToDouble();


                        try
                        {
                            tbbRows = dt.Select("contractNumber='RF" + contractNumber + "'");
                            if (tbbRows.Length > 0)
                            {
                                tbbRows[0]["newTBB"] = tbb;
                                diff = tbbRows[0]["beginningBalance"].ObjToDouble() - tbb;
                                G1.RoundValue(diff);
                                tbbRows[0]["diffTBB"] = diff;
                                if (diff != 0D)
                                    tbbRows[0]["mod"] = "S";

                                tbbRows[0]["payDate"] = payDate;
                                tbbRows[0]["newPPIT"] = payments;
                                diff = tbbRows[0]["paymentCurrMonth"].ObjToDouble() - payments;
                                G1.RoundValue(diff);
                                tbbRows[0]["diffPPIT"] = diff;
                                if (diff != 0D)
                                    tbbRows[0]["mod"] = "S";

                                tbbRows[0]["newRefunds"] = refund;
                                diff = tbbRows[0]["refundRemCurrMonth"].ObjToDouble() - refund;
                                if (diff != 0D)
                                    tbbRows[0]["mod"] = "S";

                                tbbRows[0]["newRemovals"] = removal;
                                diff = tbbRows[0]["deathRemCurrMonth"].ObjToDouble() - removal;
                                if (diff != 0D)
                                    tbbRows[0]["mod"] = "S";

                                newEB = tbb + payments - refund - removal;
                                tbbRows[0]["newEB"] = newEB;
                                diff = tbbRows[0]["endingBalance"].ObjToDouble() - newEB;
                                if (diff != 0D)
                                    tbbRows[0]["mod"] = "S";
                                if (contractNumber == "901535")
                                    tbbRows[0]["mod"] = "";
                            }
                            else
                            {
                                dRow = dt.NewRow();
                                dRow["payDate8"] = G1.DTtoMySQLDT(date);
                                dRow["newContract"] = contractNumber;
                                dRow["newTBB"] = tbb;
                                dRow["newPPIT"] = payments;
                                dRow["mod"] = "S";
                                dt.Rows.Add(dRow);
                            }
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
            catch (Exception ex)
            {
            }
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;

            gridMain2.RefreshEditor(true);
            string str = "";
            for (int i = 2; i < gridMain2.Columns.Count; i++)
            {
                str = gridMain2.Columns[i].Name.Trim();
                str = str.Replace("col", "");
                AddSummaryColumn(str, gridMain2);

            }
            gridMain4.RefreshEditor(true);
            for (int i = 2; i < gridMain4.Columns.Count; i++)
            {
                str = gridMain4.Columns[i].Name.Trim();
                str = str.Replace("col", "");
                AddSummaryColumn(str, gridMain4);

            }
            gridMain5.RefreshEditor(true);
            for (int i = 2; i < gridMain5.Columns.Count; i++)
            {
                str = gridMain5.Columns[i].Name.Trim();
                str = str.Replace("col", "");
                AddSummaryColumn(str, gridMain5);

            }
            showNeedsFixing(dt);

            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void showNeedsFixing ( DataTable dt )
        {
            string cmd = "";
            string contractNumber = "";
            string payDate = "";
            double payment = 0D;
            double paymentAmount = 0D;
            DateTime date = DateTime.Now;
            DataTable dx = null;
            DataRow[] dRows = dt.Select("newPPIT > '0'");
            for ( int i=0; i<dRows.Length; i++)
            {
                payment = dRows[i]["newPPIT"].ObjToDouble();
                date = dRows[i]["payDate"].ObjToDateTime();
                payDate = date.ToString("yyyy-MM-dd");
                contractNumber = dRows[i]["contractNumber"].ObjToString();
                cmd = "Select * from `payments` WHERE `contractNumber` = '" + contractNumber + "' AND `payDate8` = '" + payDate + "';";
                dx = G1.get_db_data(cmd);
                if ( dx.Rows.Count <= 0 )
                {
                    dRows[i]["fix"] = "*";
                    continue;
                }
                paymentAmount = dx.Rows[0]["paymentAmount"].ObjToDouble();
                if ( paymentAmount != payment )
                    dRows[i]["fix"] = "*";
            }
        }
        /****************************************************************************************/
        private void btnRight_Click(object sender, EventArgs e)
        {
            DateTime date = this.dateTimePicker2.Value;
            date = date.AddMonths(1);
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            date = new DateTime(date.Year, date.Month, days);
            this.dateTimePicker2.Value = date;
        }
        /****************************************************************************************/
        private void btnLeft_Click(object sender, EventArgs e)
        {
            DateTime date = this.dateTimePicker2.Value;
            date = date.AddMonths(-1);
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            date = new DateTime(date.Year, date.Month, days);
            this.dateTimePicker2.Value = date;
        }
        /****************************************************************************************/
        private void gridMain3_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain3.GetFocusedDataRow();
            string contract = dr["contractNumber"].ObjToString();
            if (!String.IsNullOrWhiteSpace(contract))
            {
                this.Cursor = Cursors.WaitCursor;
                CustomerDetails clientForm = new CustomerDetails(contract);
                clientForm.Show();
                this.Cursor = Cursors.Default;
            }
        }
        /****************************************************************************************/
        private void gridMain2_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain2.GetFocusedDataRow();
            string contract = dr["contractNumber"].ObjToString();
            if (!String.IsNullOrWhiteSpace(contract))
            {
                this.Cursor = Cursors.WaitCursor;
                CustomerDetails clientForm = new CustomerDetails(contract);
                clientForm.Show();
                this.Cursor = Cursors.Default;
            }
        }
        /****************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
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
        /****************************************************************************************/
        private void chkShowDiff_CheckedChanged(object sender, EventArgs e)
        {
            gridMain.RefreshData();
            gridMain.RefreshEditor(true);
            dgv.Refresh();

            if (chkShowDiff.Checked)
                btnSaveAll.Show();
            else
                btnSaveAll.Hide();
            btnSaveAll.Refresh();
        }
        /****************************************************************************************/
        private void btnSaveAll_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();

            string contractNumber = "";
            DateTime payDate8 = DateTime.Now;
            string startDate = "";
            string cmd = "";
            DataTable dx = null;

            try
            {
                contractNumber = dr["contractNumber"].ObjToString();
                if (String.IsNullOrWhiteSpace(contractNumber))
                    return;
                payDate8 = dr["payDate8"].ObjToDateTime();
                startDate = payDate8.ToString("yyyy-MM-dd 00:00:00");

                cmd = "Select * from `trust2013r` where `contractNumber` = '" + contractNumber + "' and `payDate8` >= '" + startDate + "' ORDER by `payDate8`;";
                dx = G1.get_db_data(cmd);
                dgv6.DataSource = dx;
                dgv6.Refresh();

                tabControl1.SelectedTab = tabPage6;
            }
            catch (Exception ex)
            {
            }
        }
        /****************************************************************************************/
        private void btnSaveAll_Clickx(object sender, EventArgs e) // Fix Marked Contracts
        {
            DataTable dt = (DataTable)dgv.DataSource;
            int[] rows = gridMain.GetSelectedRows();
            int lastRow = dt.Rows.Count;
            if (rows.Length > 0)
                lastRow = rows.Length;

            string contractNumber = "";
            DateTime payDate8 = DateTime.Now;
            string startDate = "";
            string cmd = "";
            DataTable dx = null;

            double beginningBalance = 0D;
            double endingBalance = 0D;
            double previousPayments = 0D;
            double currentPayments = 0D;

            int row = 0;
            DataRow dr = null;
            try
            {
                for (int i = 0; i < lastRow; i++)
                {
                    Application.DoEvents();

                    row = rows[i];
                    row = gridMain.GetDataSourceRowIndex(row);

                    dr = dt.Rows[row];

                    contractNumber = dr["contractNumber"].ObjToString();
                    //if (String.IsNullOrWhiteSpace(contractNumber))
                    //{
                    //    contractNumber = dr["newContract"].ObjToString();
                    //    AddNewContract(contractNumber);
                    //}
                    //else
                    //{
                    payDate8 = dr["payDate8"].ObjToDateTime();
                    startDate = payDate8.ToString("yyyy-MM-dd 00:00:00");

                    cmd = "Select * from `trust2013r` where `contractNumber` = '" + contractNumber + "' and `payDate8` >= '" + startDate + "' ORDER by `payDate8`;";
                    dx = G1.get_db_data(cmd);
                    dgv6.DataSource = dx;
                    dgv6.Refresh();

                    tabControl1.SelectedTab = tabPage6;
                    //}
                }
            }
            catch ( Exception ex)
            {
            }
        }
        /****************************************************************************************/
        private void AddNewContract ( string contractNumber)
        {
            string cmd = "Select * from `trust2013r` where `contractNumber` = 'RF" + contractNumber + "' ORDER by `payDate8`;";
            DataTable dx = G1.get_db_data(cmd); // First Verify Does Not Exist
            if (dx.Rows.Count > 0)
                return;

            string firstName = "";
            string lastName = "";
            string record = "";
            string customerRecord = "";
            string contractRecord = "";

            string merchandise = "";
            string cashAdvances = "";
            string services = "";

            string str = "";

            string balanceDue = "";
            string downPayment = "";
            DateTime dateOfDownPayment = DateTime.Now;

            string address = "";
            string city = "";
            string state = "";
            string zip = "";
            string sex = "";
            string ssn = "";

            DateTime dob = DateTime.Now;
            DateTime dod = DateTime.Now;
            DateTime issueDate8 = DateTime.Now;
            DateTime dolp = DateTime.Now;
            DateTime nullDate = new DateTime(1, 1, 1);

            DataRow[] dRows = rilesDt.Select("contract='" + contractNumber + "'");
            if (dRows.Length <= 0)
                return;

            contractNumber = "RF" + contractNumber;

            DataRow dr = dRows[0];

            firstName = dr["First"].ObjToString();
            lastName = dr["Last"].ObjToString();

            firstName = G1.protect_data(firstName);
            lastName = G1.protect_data(lastName);

            sex = dr["sex"].ObjToString().ToUpper();
            if (sex == "M")
                sex = "male";
            else if (sex == "F")
                sex = "female";

            ssn = dr["SOCSEC"].ObjToString();
            ssn = ssn.Replace("-", "");

            address = dr["address"].ObjToString();
            address = G1.protect_data(address);
            city = dr["city"].ObjToString();
            state = dr["state"].ObjToString();
            zip = dr["zip"].ObjToString();

            str = dr["Date Of Birth"].ObjToString();
            str = str.Replace("**", "01");
            dob = str.ObjToDateTime();
            if (dob.Year < 100)
                dob = nullDate;

            dod = dr["Date Of Death"].ObjToDateTime();
            if (dod.Year < 100)
                dod = nullDate;

            issueDate8 = dr["Contract Date"].ObjToDateTime();
            if (issueDate8.Year < 100)
                issueDate8 = nullDate;

            dolp = dr["Date of Last Payment"].ObjToDateTime();
            if (dolp.Year < 100)
                dolp = nullDate;

            merchandise = dr["Merchandise"].ObjToString();
            services = dr["Services"].ObjToString();
            cashAdvances = dr["cash advances"].ObjToString();
            balanceDue = dr["amount due"].ObjToString();
            downPayment = dr["down payment"].ObjToString();

            string fundingPCT = dr["FUNDING PCT"].ObjToString();

            dateOfDownPayment = dr["Down Payment Date"].ObjToDateTime();
            if (dateOfDownPayment.Year < 100)
                dateOfDownPayment = nullDate;

            DataTable paymentDt = new DataTable();
            paymentDt.Columns.Add("payDate8");
            paymentDt.Columns.Add("payment");
            paymentDt.Columns.Add("downPayment");
            paymentDt.Rows.Clear();

            string amtOfMonthlyPayment = "";
            DataRow dR = null;
            string paymentDateCol = "";
            string paymentAmountCol = "";
            string datePaid8 = "";
            string paymentAmount = "";

            DateTime date = this.dateTimePicker2.Value;

            try
            {
                //dR = paymentDt.NewRow();
                //dR["payDate8"] = this.dateTimePicker2.Value.ToString("MM/dd/yyyy");
                //dR["downPayment"] = downPayment;
                //paymentDt.Rows.Add(dR);

                //for (int j = 1; j <= 57; j++)
                //{
                //    paymentDateCol = "Installment Payment " + j.ToString() + " Date";
                //    paymentAmountCol = "Installment Payment " + j.ToString() + " Amount";

                //    datePaid8 = dr[paymentDateCol].ObjToString();
                //    paymentAmount = dr[paymentAmountCol].ObjToString();
                //    if (String.IsNullOrWhiteSpace(amtOfMonthlyPayment))
                //        amtOfMonthlyPayment = paymentAmount;

                //    dR = paymentDt.NewRow();
                //    dR["payDate8"] = datePaid8;
                //    dR["payment"] = paymentAmount;
                //    paymentDt.Rows.Add(dR);
                //}
                paymentAmount = "8257.00";
                int days = 0;
                for (int i = 0; i < 12; i++)
                {
                    datePaid8 = date.ToString("MM/dd/yyyy");
                    dR = paymentDt.NewRow();
                    dR["payDate8"] = datePaid8;
                    dR["payment"] = paymentAmount;
                    paymentDt.Rows.Add(dR);
                    date = date.AddMonths(1);
                    days = DateTime.DaysInMonth(date.Year, date.Month);
                    date = new DateTime(date.Year, date.Month, days);
                }

                string beginningBalance = paymentAmount;
                string endingBalance = paymentAmount;
                bool gotRefund = false;

                for ( int i=0; i<paymentDt.Rows.Count; i++)
                {
                    date = paymentDt.Rows[i]["payDate8"].ObjToDateTime();
                    record = G1.create_record("trust2013r", "address2013", "-1");
                    if (G1.BadRecord("trust2013r", record))
                        break;
                    date = paymentDt.Rows[i]["payDate8"].ObjToDateTime();
                    G1.update_db_table("trust2013r", "record", record, new string[] { "contractNumber", contractNumber, "address2013", address, "firstName", firstName, "lastName", lastName, "city2013", city, "state2013", state, "zip2013", zip, "ssn2013", ssn, "Is2002", "2002", "location", "RF", "riles", "Y", "locind", "RF" });
                    str = date.ToString("MM/dd/yyyy");
                    if ( str == "02/29/2020" )
                    {
                        endingBalance = "0.00";
                        G1.update_db_table("trust2013r", "record", record, new string[] { "refundRemCurrMonth", beginningBalance });
                        gotRefund = true;
                    }
                    G1.update_db_table("trust2013r", "record", record, new string[] { "payDate8", date.ToString("yyyy-MM-dd"), "beginningBalance", beginningBalance, "endingBalance", endingBalance});
                    if ( gotRefund && str != "02/29/2020" )
                        G1.update_db_table("trust2013r", "record", record, new string[] { "refundRemYTDprevious", beginningBalance });
                }
            }
            catch (Exception ex)
            {
            }
        }
        /****************************************************************************************/
        private void btnSaveSingle_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv6.DataSource;
            if (dt.Rows.Count <= 0)
                return;
            string contractNumber = dt.Rows[0]["contractNumber"].ObjToString();
            string record = "";
            double beginningBalance = 0D;
            double ytdPrevious = 0D;
            double paymentCurrMonth = 0D;
            double currentPayments = 0D;
            double deathRemYTDprevious = 0D;
            double deathRemCurrMonth = 0D;
            double refundRemYTDprevious = 0D;
            double refundRemCurrMonth = 0D;
            double currentRemovals = 0D;
            double endingBalance = 0D;
            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    try
                    {
                        record = dt.Rows[i]["record"].ObjToString();
                        beginningBalance = dt.Rows[i]["beginningBalance"].ObjToDouble();
                        paymentCurrMonth = dt.Rows[i]["paymentCurrMonth"].ObjToDouble();
                        currentPayments = dt.Rows[i]["currentPayments"].ObjToDouble();
                        deathRemYTDprevious = dt.Rows[i]["deathRemYTDprevious"].ObjToDouble();
                        deathRemCurrMonth = dt.Rows[i]["deathRemCurrMonth"].ObjToDouble();
                        refundRemYTDprevious = dt.Rows[i]["refundRemYTDprevious"].ObjToDouble();
                        refundRemCurrMonth = dt.Rows[i]["refundRemCurrMonth"].ObjToDouble();
                        currentRemovals = dt.Rows[i]["currentRemovals"].ObjToDouble();
                        endingBalance = dt.Rows[i]["endingBalance"].ObjToDouble();

                        try
                        {
                            G1.update_db_table("trust2013r", "record", record, new string[] { "beginningBalance", beginningBalance.ToString(), "deathRemYTDprevious", deathRemYTDprevious.ToString(), "deathRemCurrMonth", deathRemCurrMonth.ToString(), "currentRemovals", currentRemovals.ToString() });
                            G1.update_db_table("trust2013r", "record", record, new string[] { "refundRemYTDprevious", refundRemYTDprevious.ToString(), "refundRemCurrMonth", refundRemCurrMonth.ToString(), "paymentCurrMonth", paymentCurrMonth.ToString(), "currentPayments", currentPayments.ToString(), "endingBalance", endingBalance.ToString() });
                        }
                        catch ( Exception ex )
                        {
                        }
                    }
                    catch ( Exception ex )
                    {
                    }
                }
            }
            catch ( Exception ex )
            {
            }
        }
        /****************************************************************************************/
        private void SaveSingleEmployee ( DataTable dt )
        {
        }
        /****************************************************************************************/
        private void btnFixSingle_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataTable dx = (DataTable)dgv6.DataSource;

            string contractNumber = "";
            if (dx.Rows.Count <= 0)
                return;

            try
            {
                contractNumber = dx.Rows[0]["contractNumber"].ObjToString();
                DataRow[] dRows = dt.Select("contractNumber='" + contractNumber + "'");
                if (dRows.Length <= 0)
                    return;
                dt = dRows.CopyToDataTable();
            }
            catch ( Exception ex)
            {
            }

            try
            {
                if (contractNumber == "RF901535")
                    return;
                int year = dt.Rows[0]["payDate8"].ObjToDateTime().Year;
                int newYear = year;
                double beginningBalance = dt.Rows[0]["newTBB"].ObjToDouble();
                double diffTBB = dt.Rows[0]["diffTBB"].ObjToDouble();
                double diffPPIT = dt.Rows[0]["diffPPIT"].ObjToDouble();
                double newPPIT = dt.Rows[0]["newPPIT"].ObjToDouble();
                double newRefunds = dt.Rows[0]["newRefunds"].ObjToDouble();
                double newRemovals = dt.Rows[0]["newRemovals"].ObjToDouble();
                double newEB = dt.Rows[0]["newEB"].ObjToDouble();

                double ytdPrevious = 0D;

                dx.Rows[0]["beginningBalance"] = beginningBalance;
                if (newPPIT > 0D)
                {
                    dx.Rows[0]["paymentCurrMonth"] = newPPIT;
                    dx.Rows[0]["currentPayments"] = newPPIT;
                }
                dx.Rows[0]["deathRemCurrMonth"] = newRemovals;
                dx.Rows[0]["refundRemCurrMonth"] = newRefunds;
                dx.Rows[0]["currentRemovals"] = newRemovals + newRefunds;
                dx.Rows[0]["endingBalance"] = newEB;

                if (newPPIT > 0D)
                    ytdPrevious = newPPIT;

                double ytdPayments = 0D;
                double currentRemovals = 0D;
                double deathRemCurrMonth = 0D;
                double deathRemYTDprevious = 0D;
                double refundRemCurrMonth = 0D;
                double refundRemYTDprevious = 0D;
                double beginning = beginningBalance;
                bool gotRemoval = false;
                if (newRefunds > 0D || newRemovals > 0D)
                {
                    gotRemoval = true;
                    if ( newRemovals > 0D )
                    {
                        deathRemCurrMonth = newRemovals;
                        deathRemYTDprevious = newRemovals;
                        currentRemovals = newRemovals;
                    }
                    if ( newRefunds > 0D)
                    {
                        refundRemCurrMonth = newRefunds;
                        refundRemYTDprevious = newRefunds;
                        currentRemovals = newRefunds;
                    }
                }
                newPPIT = dx.Rows[0]["paymentCurrMonth"].ObjToDouble();
                if ( newPPIT > 0D)
                {
                    dx.Rows[0]["currentPayments"] = newPPIT;
                    ytdPrevious = newPPIT;
                    newEB = beginningBalance + newPPIT - deathRemCurrMonth - refundRemCurrMonth;
                }
                try
                {
                    for (int i = 1; i < dx.Rows.Count; i++)
                    {
                        beginningBalance = newEB;
                        newYear = dx.Rows[i]["payDate8"].ObjToDateTime().Year;
                        if (newYear > year)
                        {
                            if (gotRemoval && newEB == 0D)
                            {
                                beginningBalance = 0D;
                                beginning = 0D;
                            }
                            dx.Rows[i]["beginningBalance"] = beginningBalance;
                            gotRemoval = false;
                            newRefunds = 0D;
                            newRemovals = 0D;
                            deathRemYTDprevious = 0D;
                            refundRemYTDprevious = 0D;
                            currentRemovals = 0D;
                            ytdPayments = 0D;
                            ytdPrevious = 0D;

                            year = newYear;
                        }
                        dx.Rows[i]["beginningBalance"] = beginning;
                        newPPIT = dx.Rows[i]["paymentCurrMonth"].ObjToDouble();
                        deathRemCurrMonth = dx.Rows[i]["deathRemCurrMonth"].ObjToDouble();
                        if (deathRemCurrMonth == 0D && gotRemoval && newRemovals > 0D)
                            deathRemCurrMonth = newRemovals;
                        if ( deathRemCurrMonth > 0D )
                        {
                            if (deathRemCurrMonth != beginningBalance)
                            {
                                deathRemCurrMonth = beginningBalance;
                                dx.Rows[i]["deathRemCurrMonth"] = beginningBalance;
                                dx.Rows[i]["currentRemovals"] = beginningBalance;
                                newRemovals = deathRemCurrMonth;
                                //currentRemovals = beginningBalance;
                                gotRemoval = true;
                            }
                        }
                        refundRemCurrMonth = dx.Rows[i]["refundRemCurrMonth"].ObjToDouble();
                        if (refundRemCurrMonth == 0D && gotRemoval && newRefunds > 0D)
                            refundRemCurrMonth = newRefunds;
                        if (refundRemCurrMonth > 0D)
                        {
                            if (refundRemCurrMonth != beginningBalance)
                            {
                                refundRemCurrMonth = beginningBalance;
                                dx.Rows[i]["refundRemCurrMonth"] = beginningBalance;
                                dx.Rows[i]["currentRemovals"] = beginningBalance;
                                newRefunds = refundRemCurrMonth;
                                //currentRemovals = beginningBalance;
                                gotRemoval = true;
                            }
                        }
                        currentRemovals += deathRemCurrMonth + refundRemCurrMonth;
                        dx.Rows[i]["currentRemovals"] = currentRemovals;
                        dx.Rows[i]["deathRemYTDprevious"] = deathRemYTDprevious;
                        dx.Rows[i]["refundRemYTDprevious"] = refundRemYTDprevious;
                        dx.Rows[i]["ytdPrevious"] = ytdPrevious;

                        newEB = beginningBalance + newPPIT - deathRemCurrMonth - refundRemCurrMonth;
                        dx.Rows[i]["endingBalance"] = newEB;
                        if ( newRemovals > 0D )
                        {
                            deathRemYTDprevious = newRemovals;
                            newRemovals = 0D;
                        }
                        if ( newRefunds > 0D)
                        {
                            refundRemYTDprevious = newRefunds;
                            newRefunds = 0D;
                        }
                    }
                }
                catch ( Exception ex)
                {
                }
            }
            catch ( Exception ex)
            {
            }

            G1.NumberDataTable(dx);
            dgv6.DataSource = dx;
            dgv6.Refresh();
        }
        /****************************************************************************************/
    }
}