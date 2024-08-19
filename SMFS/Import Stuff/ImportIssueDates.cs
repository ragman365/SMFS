using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Base;
using System.Globalization;
using System.IO;
using DevExpress.XtraPrinting;
using DevExpress.Utils;

using GeneralLib;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class ImportIssueDates : Form
    {
        private string workWhat = "";
        private string actualFile = "";
        private bool doPayerDeceased = false;
        /***********************************************************************************************/
        public ImportIssueDates(string what = "")
        {
            InitializeComponent();
            workWhat = what;
        }
        /***********************************************************************************************/
        private void Import_Load(object sender, EventArgs e)
        {
            btnFixAll.Hide();
            if (LoginForm.username.ToUpper() == "ROBBY")
                btnFixAll.Show();
            picLoader.Hide();
            labelMaximum.Hide();
            lblTotal.Hide();
            barImport.Hide();
            this.btnImportFile.Hide();
            if (workWhat.ToUpper() == "BATESVILLE")
                mainGrid.OptionsView.ShowBands = false;
            if (!String.IsNullOrWhiteSpace(workWhat))
                this.Text = "Import " + workWhat;
            doPayerDeceased = false;
            if (workWhat.ToUpper() == "INSURANCE PAYER DECEASED DATA")
                doPayerDeceased = true;
        }
        /***********************************************************************************************/
        private void btnImportFile_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if ( SelectDone != null)
            {
                OnSelectDone(dt);
                this.Close();
                return;
            }
            if (workWhat.ToUpper() == "PREACC")
            {
                ImportPreaccData(dt, workWhat);
                return;
            }
            else if (workWhat.ToUpper() == "PREMST")
            {
                ImportCustomerData(dt, workWhat);
                return;
            }
        }
        /***********************************************************************************************/
        private void AddNewColumn(DataTable dt, string name, string format, int width)
        {
            if (string.IsNullOrEmpty(format))
            {
                int col = G1.get_column_number(dt, name);
                if (col < 0)
                    dt.Columns.Add(name, Type.GetType("System.String"));
                string caption = name;
                G1.AddNewColumn(mainGrid, name, caption, "", FormatType.None, width, true);
            }
            else
            {
                int col = G1.get_column_number(dt, name);
                if (col < 0)
                {
                    if (format.ToUpper() == "SYSTEM.STRING")
                        dt.Columns.Add(name, Type.GetType("System.String"));
                    else if (format.ToUpper() == "SYSTEM.DATE")
                        dt.Columns.Add(name, Type.GetType("System.String"));
                    else
                        dt.Columns.Add(name, Type.GetType("System.Double"));
                }
                string caption = name;
                if (format.ToUpper() == "SYSTEM.STRING")
                    G1.AddNewColumn(mainGrid, name, caption, "", FormatType.None, width, true);
                else if (format.ToUpper() == "SYSTEM.DATE")
                    G1.AddNewColumn(mainGrid, name, caption, "", FormatType.DateTime, width, true);
                else
                    G1.AddNewColumn(mainGrid, name, caption, "N2", FormatType.Numeric, width, true);
            }
        }
        /***********************************************************************************************/
        private void AddNewColumn(string name, string format, string type = "System.Double")
        {
            DataTable dt = (DataTable)(dgv.DataSource);
            int col = G1.get_column_number(dt, name);
            if (col < 0)
                dt.Columns.Add(name, Type.GetType(type));
            string caption = name;
            G1.AddNewColumn(mainGrid, name, caption, format, FormatType.Numeric, 75, true);
        }
        /***********************************************************************************************/
        private void btnSelectFile_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string file = ofd.FileName;
                    int idx = file.LastIndexOf("\\");
                    if (idx > 0)
                    {
                        actualFile = file.Substring(idx);
                        actualFile = actualFile.Replace("\\", "");
                    }
                    dgv.DataSource = null;
                    this.Cursor = Cursors.WaitCursor;
                    DataTable dt = ImportCSVfile(file);
                    this.Cursor = Cursors.Default;
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        if (workWhat.ToUpper() == "PREACC")
                            dt = PreProcessPreacc(dt);
                        G1.NumberDataTable(dt);
                        dgv.DataSource = dt;
                        btnImportFile.Show();
                    }
                }
            }
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private DataTable PreProcessPreacc ( DataTable dt)
        {
            DataTable newDt = dt.Clone();
            DateTime date = DateTime.Now;
            string dateStr = "";
            DateTime oldDate = DateTime.Now;
            string oldDateStr = "";
            string cmd = "";
            string contractNumber = "";
            newDt.Columns.Add("OldIssueDate");
            newDt.Columns.Add("Diff");
            DataTable dx = null;
            int row = 0;
            this.Cursor = Cursors.WaitCursor;
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                date = dt.Rows[i]["ISSDT8"].ObjToDateTime();
                if (date.Year >= 2015)
                {
                    newDt.ImportRow(dt.Rows[i]);
                    try
                    {
                        contractNumber = dt.Rows[i]["CNUM"].ObjToString();
                        if (!String.IsNullOrWhiteSpace(contractNumber))
                        {
                            cmd = "Select * from `contracts` where `contractNumber` = '" + contractNumber + "';";
                            dx = G1.get_db_data(cmd);
                            if (dx.Rows.Count > 0)
                            {
                                row = newDt.Rows.Count - 1;
                                dateStr = date.ToString("yyyyMMdd");
                                oldDate = dx.Rows[0]["issueDate8"].ObjToDateTime();
                                oldDateStr = oldDate.ToString("yyyyMMdd");
                                newDt.Rows[row]["OldIssueDate"] = oldDateStr;
                                if (oldDateStr != dateStr)
                                    newDt.Rows[row]["Diff"] = "DIFF";
                            }
                        }
                    }
                    catch ( Exception ex )
                    {
                    }
                }
            }
            this.Cursor = Cursors.Default;
            return newDt;
        }
        /***********************************************************************************************/
        private void ImportCustomerFile(DataTable dt)
        {
            picLoader.Show();
            DataTable newDt = new DataTable();
            AddNewColumn(newDt, "Num", "System.String", 5);
            AddNewColumn(newDt, "contractNumber", "System.String", 20);
            AddNewColumn(newDt, "lastName", "System.String", 20);
            AddNewColumn(newDt, "firstName", "System.String", 20);

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                Application.DoEvents();
                DataRow dRow = newDt.NewRow();
                dRow["Num"] = (i + 1).ToString();
                AddToTable(dt, i, dRow, "cnum", "contractNumber");
                AddToTable(dt, i, dRow, "lname", "lastName");
                AddToTable(dt, i, dRow, "fname", "firstName");
                newDt.Rows.Add(dRow);
            }
            picLoader.Hide();

            G1.SetColumnPosition(newDt, mainGrid);

            mainGrid.BestFitColumns(false);
            mainGrid.OptionsView.ColumnAutoWidth = false;

            mainGrid.Columns["Num"].Fixed = DevExpress.XtraGrid.Columns.FixedStyle.Left;
            mainGrid.Columns["contractNumber"].Fixed = DevExpress.XtraGrid.Columns.FixedStyle.Left;
            this.Text = "Import Customer Data";
            dgv.DataSource = newDt;
        }
        /***********************************************************************************************/
        private void ImportContractFile(DataTable dt)
        {
            picLoader.Show();
            DataTable newDt = new DataTable();
            AddNewColumn(newDt, "Num", "System.String", 5);
            AddNewColumn(newDt, "contractNumber", "System.String", 20);
            AddNewColumn(newDt, "deleteFlag", "System.String", 10);
            AddNewColumn(newDt, "serviceTotal", "System.Double", 25);
            AddNewColumn(newDt, "merchandiseTotal", "System.Double", 25);
            AddNewColumn(newDt, "allowMerchandise", "System.Double", 25);
            AddNewColumn(newDt, "allowInsurance", "System.Double", 25);
            AddNewColumn(newDt, "downPayment", "System.Double", 25);
            AddNewColumn(newDt, "ageAtIssue", "System.Double", 25);
            AddNewColumn(newDt, "numberOfPayments", "System.Double", 25);
            AddNewColumn(newDt, "amtOfMonthlyPayt", "System.Double", 25);
            AddNewColumn(newDt, "lastDatePaid", "System.String", 25);
            AddNewColumn(newDt, "decliningNumPaymts", "System.Double", 25);
            AddNewColumn(newDt, "balanceDue", "System.Double", 25);
            AddNewColumn(newDt, "nowDue", "System.Double", 25);
            AddNewColumn(newDt, "pullCode", "System.String", 25);
            AddNewColumn(newDt, "pullReason", "System.String", 25);
            AddNewColumn(newDt, "bank", "System.String", 10);
            AddNewColumn(newDt, "notes", "System.String", 50);
            AddNewColumn(newDt, "amountPaid", "System.Double", 25);
            AddNewColumn(newDt, "lastDatePaid8", "System.String", 25);
            AddNewColumn(newDt, "dueDate8", "System.String", 25);
            AddNewColumn(newDt, "issueDate8", "System.String", 25);
            AddNewColumn(newDt, "lapseDate8", "System.String", 25);
            AddNewColumn(newDt, "reinstateDate8", "System.String", 25);
            AddNewColumn(newDt, "apr", "System.Double", 25);
            AddNewColumn(newDt, "totalInterest", "System.Double", 25);
            AddNewColumn(newDt, "interestPaid", "System.Double", 25);

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                Application.DoEvents();
                DataRow dRow = newDt.NewRow();
                dRow["Num"] = (i + 1).ToString();
                AddToTable(dt, i, dRow, "cnum", "contractNumber");
                AddToTable(dt, i, dRow, "del", "deleteFlag");
                AddToTable(dt, i, dRow, "sertot", "serviceTotal");
                AddToTable(dt, i, dRow, "mertot", "merchandiseTotal");
                AddToTable(dt, i, dRow, "amtot", "allowMerchandise");
                AddToTable(dt, i, dRow, "aptot", "allowInsurance");
                AddToTable(dt, i, dRow, "dpay", "downPayment");
                AddToTable(dt, i, dRow, "ageiss", "ageAtIssue");
                AddToTable(dt, i, dRow, "pay#", "numberOfPayments");
                AddToTable(dt, i, dRow, "pamt", "amtOfMonthlyPayt");
                AddToTable(dt, i, dRow, "ldate", "lastDatePaid", "1");
                AddToTable(dt, i, dRow, "dpay#", "decliningNumPaymts");
                AddToTable(dt, i, dRow, "baldue", "balanceDue");
                AddToTable(dt, i, dRow, "nowd", "nowDue");
                AddToTable(dt, i, dRow, "pull", "pullCode");
                AddToTable(dt, i, dRow, "prea", "pullReason");
                AddToTable(dt, i, dRow, "bnk", "bank");
                AddToTable(dt, i, dRow, "notes", "notes");
                AddToTable(dt, i, dRow, "pdamt", "amountPaid");
                AddToTable(dt, i, dRow, "ldate8", "lastDatePaid8", "2");
                AddToTable(dt, i, dRow, "ddue8", "dueDate8", "2");
                AddToTable(dt, i, dRow, "issdt8", "issueDate8", "2");
                AddToTable(dt, i, dRow, "lapdt8", "lapseDate8", "2");
                AddToTable(dt, i, dRow, "rendt8", "reinstateDate8", "2");
                AddToTable(dt, i, dRow, "apr", "apr");
                AddToTable(dt, i, dRow, "totint", "totalInterest");
                AddToTable(dt, i, dRow, "intpd", "interestPaid");
                newDt.Rows.Add(dRow);
            }
            picLoader.Hide();

            G1.SetColumnPosition(newDt, mainGrid);

            mainGrid.BestFitColumns(false);
            mainGrid.OptionsView.ColumnAutoWidth = false;

            mainGrid.Columns["Num"].Fixed = DevExpress.XtraGrid.Columns.FixedStyle.Left;
            mainGrid.Columns["contractNumber"].Fixed = DevExpress.XtraGrid.Columns.FixedStyle.Left;
            this.Text = "Import Contract Data";
            dgv.DataSource = newDt;
        }
        /***********************************************************************************************/
        private string ParseOutNewDate(string date)
        {
            if (date == "0")
                return "00/00/0000";
            if (date.Trim().Length < 8)
            {
                MessageBox.Show("***ERROR*** Date < 8 characters! " + date);
                return date;
            }
            string year = date.Substring(0, 4);
            string month = date.Substring(4, 2);
            string day = date.Substring(6, 2);
            string newdate = month + "/" + day + "/" + year;
            long ldate = G1.date_to_days(newdate);
            newdate = G1.days_to_date(ldate);
            return newdate;
        }
        /***********************************************************************************************/
        private string ParseOutOldDate(string date)
        {
            if (date == "0")
                return "00/00/0000";
            if (date.Trim().Length < 6)
                date = "0" + date;
            if (date.Trim().Length < 6)
            {
                MessageBox.Show("***ERROR*** Date < 6 characters! " + date);
                return date;
            }
            string year = date.Substring(4, 2);
            string day = date.Substring(2, 2);
            string month = date.Substring(0, 2);
            string newdate = month + "/" + day + "/" + year;
            long ldate = G1.date_to_days(newdate);
            newdate = G1.days_to_date(ldate);
            return newdate;
        }
        /***********************************************************************************************/
        private void AddToTable(DataTable dt, int row, DataRow dr, string dtName, string gridName, string dateType = "")
        {
            string str = dt.Rows[row][dtName].ObjToString();
            if (dateType == "1")
                str = ParseOutOldDate(str);
            else if (dateType == "2")
                str = ParseOutNewDate(str);
            dr[gridName] = str;
        }
        /***********************************************************************************************/
        public static DataTable ImportCSVfile(string filename, PictureBox picLoader = null)
        {
            bool honorInsuranceDebug = false;
            //if (filename.ToUpper().IndexOf("INSURANCE") >= 0)
            //{
            //    if ( LoginForm.username.ToUpper() == "ROBBY")
            //    {
            //        DialogResult result = MessageBox.Show("Robby, Debug Insurance Payments?", "Debug Insurance Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
            //        if (result == DialogResult.Yes)
            //            honorInsuranceDebug = true;
            //    }
            //}
                int maxColumns = 0;
            if (picLoader != null)
                picLoader.Show();
            DataTable dt = new DataTable();
            if (!File.Exists(filename))
            {
                DevExpress.XtraEditors.XtraMessageBox.Show("***ERROR*** File does not exist!");
                return null;
            }
            try
            {
                bool first = true;
                string payer = "";
                string line = "";
                int row = 0;
                string str = "";
                FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using (StreamReader sr = new StreamReader(fs))

                {
                    while ((line = sr.ReadLine()) != null)
                    {
                        Application.DoEvents();
                        if (first)
                        {
                            first = false;
                            dt = BuildImportDt(line);
                            maxColumns = (dt.Columns.Count - 1);
                            continue;
                        }
                        string[] Lines = line.Split(',');
                        G1.parse_answer_data(line, ",");
                        int count = G1.of_ans_count;
                        if (G1.of_ans_count >= maxColumns)
                        {
                            if (honorInsuranceDebug)
                            {
                                payer = Lines[0].ObjToString();
                                if (payer.Trim() != "CC-843")
                                    continue;
                            }
                            DataRow dRow = dt.NewRow();
                            for (int i = 0; i < G1.of_ans_count; i++)
                            {
                                str = G1.of_answer[i].ObjToString().Trim();
                                str = trim(str);
                                dRow[i + 1] = str;
                            }
                            dt.Rows.Add(dRow);
                        }
                        row++;
                        //                        picLoader.Refresh();
                    }
                    sr.Close();
                }
            }
            catch (Exception ex)
            {
                DevExpress.XtraEditors.XtraMessageBox.Show(ex.Message, "Error Occurred");
            }
            G1.NumberDataTable(dt);
            if (picLoader != null)
                picLoader.Hide();
            return dt;
        }
        /***********************************************************************************************/
        public static string trim(string str)
        {
            string text = "";
            int j = 0;
            for (int i = 0; i < str.Length; i++)
            {
                j = (int)(str[i]);
                if (j <= 0)
                    break;
                text += str.Substring(i, 1);
            }
            return text;
        }
        /***********************************************************************************************/
        public static DataTable BuildImportDt(string line)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Num");
            G1.parse_answer_data(line, ",");
            for (int i = 0; i < G1.of_ans_count; i++)
            {
                string name = G1.of_answer[i].ObjToString();
                name = trim(name);
                if (String.IsNullOrEmpty(name))
                    name = "COL " + i.ToString();
                name = name.Trim();
                int col = G1.get_column_number(dt, name);
                if (col < 0)
                    dt.Columns.Add(name);
            }
            return dt;
        }
        /***********************************************************************************************/
        private bool checkDuplicateContract(string contract)
        {
            if (String.IsNullOrWhiteSpace(contract))
            {
                MessageBox.Show("***ERROR*** Invalid Key!\nContract must be unique and not blank!", "Import Contract Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return true;
            }
            string cmd = "Select * from `contracts` where `contractNumber` = '" + contract + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                MessageBox.Show("***ERROR*** Duplicate Key!\nYou must enter a unique Contract!", "Import Contract Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return true;
            }
            return false;
        }
        /***********************************************************************************************/
        private void ImportPreaccData(DataTable dt, string workwhat)
        {
            picLoader.Show();
            bool doingDeath = false;
            DataTable dx = null;
            DateTime tempDate = DateTime.Now;
            DateTime tempDate2 = DateTime.Now;
            string cmd = "";
            string record = "";
            string contract = "";
            string deleteFlag = "";
            string serviceTotal = "";
            string merchandiseTotal = "";
            string allowMerchandise = "";
            string allowInsurance = "";
            string downPayment = "";
            string ageAtIssue = "";
            string numberOfPayments = "";
            string amtOfMonthlyPayt = "";
            string lastDatePaid = "";
            string decliningNumPaymts = "";
            string balanceDue = "";
            string nowDue = "";
            string pullCode = "";
            string pullReason = "";
            string bank = "";
            string notes = "";
            string amountPaid = "";
            string lastDatePaid8 = "";
            string dueDate8 = "";
            string issueDate8 = "";
            string lapseDate8 = "";
            string reinstateDate8 = "";
            string apr = "";
            string totalInterest = "";
            string interestPaid = "";
            string deathDate8 = "";
            int mm = 0;
            int dd = 0;
            int yy = 0;
            barImport.Show();
            int lastrow = dt.Rows.Count;
            int tableRow = 0;
            int created = 0;
            DateTime beginDate = new DateTime(2018, 7, 1);
            DateTime endDate = new DateTime(2018, 9, 1);
            //            lastrow = 1;
            try
            {
                lblTotal.Show();

                lblTotal.Text = "of " + lastrow.ToString();
                lblTotal.Refresh();

                barImport.Minimum = 0;
                barImport.Maximum = lastrow;
                labelMaximum.Show();
                for (int i = 0; i < lastrow; i++)
                {
                    picLoader.Refresh();
                    barImport.Value = i;
                    barImport.Refresh();
                    labelMaximum.Text = i.ToString();
                    labelMaximum.Refresh();
                    tableRow = i;
                    record = "";
                    try
                    {
                        contract = dt.Rows[i]["cnum"].ObjToString();
                        //if (contract != "B18013LI")
                        //    continue;
                        //if (contract.ToUpper() != "B18030LI")
                        //{
                        //    continue;
                        //}
                        cmd = "Select * from `contracts` where `contractNumber` = '" + contract + "';";
                        dx = G1.get_db_data(cmd);
                        if ( dx.Rows.Count > 0 )
                        {
                            record = dx.Rows[0]["record"].ObjToString();
                            downPayment = dt.Rows[i]["dpay"].ObjToString();
                            issueDate8 = GetSQLDate(dt, i, "issdt8");
                            issueDate8 = dt.Rows[i]["issdt8"].ObjToString();
                            if (issueDate8.Length >= 8)
                            {
                                yy = issueDate8.Substring(0, 4).ObjToInt32();
                                mm = issueDate8.Substring(4, 2).ObjToInt32();
                                dd = issueDate8.Substring(6, 2).ObjToInt32();
                                if (yy == 0)
                                    yy = 1990;
                                if (mm == 0)
                                    mm = 1;
                                if (dd == 0)
                                    dd = 1;
                                issueDate8 = yy.ToString("D4") + "-" + mm.ToString("D2") + "-" + dd.ToString("D2");
                                tempDate = issueDate8.ObjToDateTime();
                                if (tempDate < beginDate || tempDate > endDate)
                                    continue;
                                tempDate2 = dx.Rows[0]["issueDate8"].ObjToDateTime();
                                if (tempDate != tempDate2)
                                {
                                    issueDate8 = tempDate.ToString("yyyy-MM-dd");
                                    G1.update_db_table("contracts", "record", record, new string[] { "issueDate8", issueDate8 });
                                }
                                cmd = "Select * from `payments` where `contractNumber` = '" + contract + "' and `downPayment` = '" + downPayment.ToString() + "';";
                                dx = G1.get_db_data(cmd);
                                if ( dx.Rows.Count > 0 )
                                {
                                    tempDate2 = dx.Rows[0]["payDate8"].ObjToDateTime();
                                    if ( tempDate2 < tempDate )
                                    {
                                        record = dx.Rows[0]["record"].ObjToString();
                                        issueDate8 = tempDate.ToString("yyyy-MM-dd");
                                        G1.update_db_table("payments", "record", record, new string[] { "payDate8", issueDate8 });
                                    }
                                }
                            }
                            continue;
                        }
                        if (1 == 1)
                            continue;
                        if (dx.Rows.Count > 0)
                        {
                            record = dx.Rows[0]["record"].ObjToString();
                            balanceDue = dt.Rows[i]["baldue"].ObjToString();
                            nowDue = dt.Rows[i]["nowd"].ObjToString();
                            dueDate8 = GetSQLDate(dt, i, "ddue8");
                            G1.update_db_table("contracts", "record", record, new string[] { "balanceDue", balanceDue, "nowDue", nowDue, "dueDate8", dueDate8 });
                        }
                        //else
                        //{
                        //    record = G1.create_record("contracts", "contractNumber", "-1");
                        //    if (G1.BadRecord("contracts", record))
                        //        continue;
                        //    created++;
                        //}
                        if (1 == 1)
                            continue;
                        if (string.IsNullOrWhiteSpace(record))
                        {
                            MessageBox.Show("***ERROR*** Creating Contract Record! " + contract + " Stopping!");
                            break;
                        }
                        else if (record == "-1")
                        {
                            MessageBox.Show("***ERROR*** Creating Contract Record! " + contract + " Stopping!");
                            break;
                        }
                        G1.update_db_table("contracts", "record", record, new string[] { "contractNumber", contract });

                        deleteFlag = dt.Rows[i]["del"].ObjToString();
                        serviceTotal = dt.Rows[i]["sertot"].ObjToString();
                        merchandiseTotal = dt.Rows[i]["mertot"].ObjToString();
                        allowMerchandise = dt.Rows[i]["amtot"].ObjToString();
                        allowInsurance = dt.Rows[i]["aptot"].ObjToString();
                        downPayment = dt.Rows[i]["dpay"].ObjToString();
                        ageAtIssue = dt.Rows[i]["ageiss"].ObjToString();
                        numberOfPayments = dt.Rows[i]["pay#"].ObjToString();
                        amtOfMonthlyPayt = dt.Rows[i]["pamt"].ObjToString();
                        balanceDue = dt.Rows[i]["baldue"].ObjToString();
                        nowDue = dt.Rows[i]["nowd"].ObjToString();
                        pullCode = dt.Rows[i]["pull"].ObjToString();
                        if (!doingDeath)
                        {
                            lastDatePaid = GetSQLDate(dt, i, "ldate");
                            pullReason = dt.Rows[i]["prea"].ObjToString();
                            bank = dt.Rows[i]["bnk"].ObjToString();
                            notes = dt.Rows[i]["notes"].ObjToString();
                            amountPaid = dt.Rows[i]["pdamt"].ObjToString();
                            lastDatePaid8 = GetSQLDate(dt, i, "ldate8");
                            lapseDate8 = GetSQLDate(dt, i, "lapdt8");
                            decliningNumPaymts = dt.Rows[i]["dpay#"].ObjToString();
                        }
                        dueDate8 = GetSQLDate(dt, i, "ddue8");
                        issueDate8 = GetSQLDate(dt, i, "issdt8");
                        reinstateDate8 = GetSQLDate(dt, i, "rendt8");
                        apr = dt.Rows[i]["apr"].ObjToString();
                        totalInterest = dt.Rows[i]["totint"].ObjToString();
                        interestPaid = dt.Rows[i]["intpd"].ObjToString();

                        if (workwhat.ToUpper() == "PREACCLAP")
                        {
                            if (lapseDate8.IndexOf("0000") >= 0)
                            {
                                tempDate = dueDate8.ObjToDateTime();
                                tempDate = tempDate.AddDays(60);
                                lapseDate8 = tempDate.ToString("yyyy-MM-dd");
                            }
                        }
                        G1.update_db_table("contracts", "record", record, new string[] { "deleteFlag", deleteFlag, "serviceTotal", serviceTotal, "merchandiseTotal", merchandiseTotal });

                        G1.update_db_table("contracts", "record", record, new string[] { "allowMerchandise", allowMerchandise, "allowInsurance", allowInsurance, "downPayment", downPayment });
                        G1.update_db_table("contracts", "record", record, new string[] { "ageAtIssue", ageAtIssue, "numberOfPayments", numberOfPayments, "amtOfMonthlyPayt", amtOfMonthlyPayt });
                        G1.update_db_table("contracts", "record", record, new string[] { "decliningNumPaymts", decliningNumPaymts, "balanceDue", balanceDue });
                        G1.update_db_table("contracts", "record", record, new string[] { "nowDue", nowDue, "pullCode", pullCode, "pullReason", pullReason });
                        G1.update_db_table("contracts", "record", record, new string[] { "bank", bank, "notes", notes, "amountPaid", amountPaid });
                        G1.update_db_table("contracts", "record", record, new string[] { "lastDatePaid8", lastDatePaid8, "dueDate8", dueDate8, "issueDate8", issueDate8, "lapseDate8", lapseDate8 });
                        G1.update_db_table("contracts", "record", record, new string[] { "reinstateDate8", reinstateDate8, "apr", apr, "totalInterest", totalInterest, "interestPaid", interestPaid });
                        if (workwhat.ToUpper() == "PREACCLAP")
                        {
                            if (deleteFlag.ToUpper() == "B")
                                G1.update_db_table("contracts", "record", record, new string[] { "deceasedDate", dueDate8 });
                            else if (lapseDate8.IndexOf("0000") < 0 || deleteFlag.ToUpper() == "L" || deleteFlag.ToUpper() == "X")
                                G1.update_db_table("contracts", "record", record, new string[] { "lapsed", "Y", "lapseDate8", dueDate8 });
                            else if (deleteFlag.ToUpper() == "R")
                            {
                                if (reinstateDate8.IndexOf("0000") < 0)
                                    G1.update_db_table("contracts", "record", record, new string[] { "reinstateDate8", dueDate8 });
                            }
                        }
                        else if (workwhat.ToUpper() == "PREACC")
                        {
                            G1.update_db_table("contracts", "record", record, new string[] { "lastDatePaid", lastDatePaid });
                            if (deleteFlag.ToUpper() == "B")
                                G1.update_db_table("contracts", "record", record, new string[] { "deceasedDate", dueDate8 });
                            else if (deleteFlag.ToUpper() == "L" || deleteFlag.ToUpper() == "X")
                            {
                                if (lapseDate8.IndexOf("0000") < 0)
                                    G1.update_db_table("contracts", "record", record, new string[] { "lapsed", "Y", "lapseDate8", dueDate8 });
                            }
                            else if (deleteFlag.ToUpper() == "R")
                            {
                                if (reinstateDate8.IndexOf("0000") < 0)
                                    G1.update_db_table("contracts", "record", record, new string[] { "reinstateDate8", dueDate8 });
                            }
                        }
                        if (doingDeath)
                        {
                            deathDate8 = GetSQLDate(dt, i, "Death Date");
                            G1.update_db_table("contracts", "record", record, new string[] { "deceasedDate", deathDate8 });
                        }
                    }
                    catch (Exception ex)
                    {
                        dt.Rows[i]["num"] = "*ERROR*";
                    }
                }
                picLoader.Hide();
                barImport.Value = lastrow;
                MessageBox.Show("Contract Data Import of " + lastrow + " Rows Complete - Created " + created.ToString() + " . . .");
            }
            catch (Exception ex)
            {
                picLoader.Hide();
                MessageBox.Show("***ERROR*** Creating Contract Record/Row! " + contract + "/" + tableRow.ToString() + " Stopping! " + ex.Message.ObjToString());
            }
        }
        /***********************************************************************************************/
        private void ImportCustomerData(DataTable dt, string workwhat)
        {
            picLoader.Show();
            DataTable dx = null;
            string cmd = "";
            string record = "";
            string contract = "";
            string firstName = "";
            string lastName = "";
            string address1 = "";
            string address2 = "";
            string city = "";
            string state = "";
            string zip1 = "";
            string zip2 = "";
            string sex = "";
            string ssn = "";
            string agentCode = "";
            string coverageType = "";
            string deleteFlag = "";
            string extraItemAmtMI1 = "";
            string extraItemAmtMI2 = "";
            string extraItemAmtMI3 = "";
            string extraItemAmtMI4 = "";
            string extraItemAmtMI5 = "";
            string extraItemAmtMI6 = "";
            string extraItemAmtMI7 = "";
            string extraItemAmtMI8 = "";
            string allowMerDesc1 = "";
            string allowMerDesc2 = "";
            string allowMerDesc3 = "";
            string allowMerDesc4 = "";
            string allowMerAmt1 = "";
            string allowMerAmt2 = "";
            string allowMerAmt3 = "";
            string allowMerAmt4 = "";
            string allowPolicyS1 = "";
            string allowPolicyS2 = "";
            string allowPolicyS3 = "";
            string allowPolicyS4 = "";
            string allowPolicyS5 = "";
            string allowPolicyS6 = "";
            string allowPolicyS7 = "";
            string allowPolicyS8 = "";
            string allowPolicyAmt1 = "";
            string allowPolicyAmt2 = "";
            string allowPolicyAmt3 = "";
            string allowPolicyAmt4 = "";
            string allowPolicyAmt5 = "";
            string allowPolicyAmt6 = "";
            string allowPolicyAmt7 = "";
            string allowPolicyAmt8 = "";
            string areaCode = "";
            string phoneNumber = "";
            string extraItemAmtMR1 = "";
            string extraItemAmtMR2 = "";
            string extraItemAmtMR3 = "";
            string extraItemAmtMR4 = "";
            string extraItemAmtMR5 = "";
            string extraItemAmtMR6 = "";
            string extraItemAmtMR7 = "";
            string extraItemAmtMR8 = "";
            string directorSaleCode = "";
            string birthDate = "";
            string firstPayDate = "";
            string contractDate = "";
            string lapsed = "";
            string casketCode = "";
            string vaultCode = "";
            double casketPrice = 0D;
            double vaultPrice = 0D;
            labelMaximum.Show();
            labelMaximum.Text = "0";
            barImport.Show();

            int lastrow = dt.Rows.Count;
            bool fixMerchandise = false;
            fixMerchandise = true;

            int tableRow = 0;
            //            lastrow = 1;
            try
            {
                lblTotal.Show();
                barImport.Minimum = 0;
                barImport.Maximum = lastrow;
                lblTotal.Text = "of " + lastrow.ToString();
                lblTotal.Refresh();
                int created = 0;
                picLoader.Show();
                int start = 0;
                string startText = this.txtStartRow.Text;
                if ( G1.validate_numeric ( startText ))
                {
                    start = startText.ObjToInt32();
                    if (start <= 0)
                        start = 1;
                    start = start - 1;
                }
                //                start = 9000;

                for (int i = start; i < lastrow; i++)
                {
                    picLoader.Refresh();
                    barImport.Value = i;
                    barImport.Refresh();
                    labelMaximum.Text = i.ToString();
                    labelMaximum.Refresh();

                    tableRow = i;
                    record = "";
                    try
                    {
                        contract = dt.Rows[i]["cnum"].ObjToString();
                        if (String.IsNullOrWhiteSpace(contract))
                            continue;
                        if (contract == "/")
                            contract = "X" + i.ToString();
                        else if (contract == "\\")
                            contract = "X" + i.ToString();

                        cmd = "Select * from `customers` where `contractNumber` = '" + contract + "';";
                        dx = G1.get_db_data(cmd);
                        if (fixMerchandise )
                        { // Update Casket Name, Vault, and Price
                            if (dx.Rows.Count <= 0)
                                continue;
                            record = dx.Rows[0]["record"].ObjToString();
                            extraItemAmtMI1 = dt.Rows[i]["mi1"].ObjToString(); // Casket Code
                            extraItemAmtMI1 = extraItemAmtMI1.Replace("-", "");
                            extraItemAmtMI2 = dt.Rows[i]["mi2"].ObjToString(); // Vault Code
                            extraItemAmtMI3 = dt.Rows[i]["mi3"].ObjToString();
                            extraItemAmtMI4 = dt.Rows[i]["mi4"].ObjToString();
                            extraItemAmtMI5 = dt.Rows[i]["mi5"].ObjToString();
                            extraItemAmtMI6 = dt.Rows[i]["mi6"].ObjToString();
                            extraItemAmtMI7 = dt.Rows[i]["mi7"].ObjToString();
                            extraItemAmtMI8 = dt.Rows[i]["mi8"].ObjToString();

                            extraItemAmtMR1 = dt.Rows[i]["mr1"].ObjToString(); // Casket Price1
                            extraItemAmtMR2 = dt.Rows[i]["mr2"].ObjToString(); // Vault Price1
                            extraItemAmtMR3 = dt.Rows[i]["mr3"].ObjToString(); // Casket Price2, add to Casket Price1 for Total Price
                            extraItemAmtMR4 = dt.Rows[i]["mr4"].ObjToString(); // Vault Price2, add to Vault Price1 for Total Price
                            extraItemAmtMR5 = dt.Rows[i]["mr5"].ObjToString();
                            extraItemAmtMR6 = dt.Rows[i]["mr6"].ObjToString();
                            extraItemAmtMR7 = dt.Rows[i]["mr7"].ObjToString();
                            extraItemAmtMR8 = dt.Rows[i]["mr8"].ObjToString();

                            casketPrice = extraItemAmtMR1.ObjToDouble() + extraItemAmtMR3.ObjToDouble();
                            vaultPrice = extraItemAmtMR2.ObjToDouble() + extraItemAmtMR4.ObjToDouble();

                            G1.update_db_table("customers", "record", record, new string[] { "extraItemAmtMI1", extraItemAmtMI1, "extraItemAmtMI2", extraItemAmtMI2, "extraItemAmtMI3", extraItemAmtMI3, "extraItemAmtMI4", extraItemAmtMI4, "extraItemAmtMI5", extraItemAmtMI5, "extraItemAmtMI6", extraItemAmtMI6, "extraItemAmtMI7", extraItemAmtMI7, "extraItemAmtMI8", extraItemAmtMI8 });
                            G1.update_db_table("customers", "record", record, new string[] { "extraItemAmtMR1", extraItemAmtMR1, "extraItemAmtMR2", extraItemAmtMR2, "extraItemAmtMR3", extraItemAmtMR3, "extraItemAmtMR4", extraItemAmtMR4, "extraItemAmtMR5", extraItemAmtMR5, "extraItemAmtMR6", extraItemAmtMR6, "extraItemAmtMR7", extraItemAmtMR7, "extraItemAmtMR8", extraItemAmtMR8 });

                            AddServices(contract, "Casket Name", extraItemAmtMI1, "Merchandise", false);
                            AddServices(contract, "Casket Price", casketPrice.ToString(), "Merchandise", false);
                            AddServices(contract, "Outer Container Name", extraItemAmtMI2, "Merchandise", false);
                            AddServices(contract, "Outer Container Price", vaultPrice.ToString(), "Merchandise", false);

                            G1.sleep(100);
                            GC.Collect();
                            continue;
                        }
                        if (1 == 1)
                            continue;
                        if (dx.Rows.Count > 0)
                            record = dx.Rows[0]["record"].ObjToString();
                        else
                        {
                            record = G1.create_record("customers", "contractNumber", "-1");
                            if (G1.BadRecord("customers", record))
                                continue;
                            created++;
                        }
                        if (string.IsNullOrWhiteSpace(record))
                        {
                            MessageBox.Show("***ERROR*** Creating Customer Record! " + contract + " Stopping!");
                            break;
                        }
                        else if (record == "-1")
                        {
                            MessageBox.Show("***ERROR*** Creating Customer Record! " + contract + " Stopping!");
                            break;
                        }

                        G1.update_db_table("customers", "record", record, new string[] { "contractNumber", contract });

                        firstName = dt.Rows[i]["fname"].ObjToString();
                        lastName = dt.Rows[i]["lname"].ObjToString();

                        address1 = dt.Rows[i]["add1"].ObjToString();
                        address2 = dt.Rows[i]["add2"].ObjToString();
                        city = dt.Rows[i]["city"].ObjToString();
                        state = dt.Rows[i]["state"].ObjToString();
                        zip1 = dt.Rows[i]["zip1"].ObjToString();
                        zip2 = dt.Rows[i]["zip2"].ObjToString();

                        sex = dt.Rows[i]["sex"].ObjToString();
                        ssn = dt.Rows[i]["ssno"].ObjToString();
                        agentCode = dt.Rows[i]["anum"].ObjToString();
                        coverageType = dt.Rows[i]["instr"].ObjToString();
                        deleteFlag = dt.Rows[i]["del"].ObjToString();
                        extraItemAmtMI1 = dt.Rows[i]["mi1"].ObjToString();
                        extraItemAmtMI2 = dt.Rows[i]["mi2"].ObjToString();
                        //extraItemAmtMI3 = dt.Rows[i]["mi3"].ObjToString();
                        //extraItemAmtMI4 = dt.Rows[i]["mi4"].ObjToString();
                        //extraItemAmtMI5 = dt.Rows[i]["mi5"].ObjToString();
                        //extraItemAmtMI6 = dt.Rows[i]["mi6"].ObjToString();
                        //extraItemAmtMI7 = dt.Rows[i]["mi7"].ObjToString();
                        //extraItemAmtMI8 = dt.Rows[i]["mi8"].ObjToString();
                        //allowMerDesc1 = dt.Rows[i]["amd1"].ObjToString();
                        //allowMerDesc2 = dt.Rows[i]["amd2"].ObjToString();
                        //allowMerDesc3 = dt.Rows[i]["amd3"].ObjToString();
                        //allowMerDesc4 = dt.Rows[i]["amd4"].ObjToString();
                        //allowMerAmt1 = dt.Rows[i]["ama1"].ObjToString();
                        //allowMerAmt2 = dt.Rows[i]["ama2"].ObjToString();
                        //allowMerAmt3 = dt.Rows[i]["ama3"].ObjToString();
                        //allowMerAmt4 = dt.Rows[i]["ama4"].ObjToString();
                        //allowPolicyS1 = dt.Rows[i]["pp1"].ObjToString();
                        //allowPolicyS2 = dt.Rows[i]["pp2"].ObjToString();
                        //allowPolicyS3 = dt.Rows[i]["pp3"].ObjToString();
                        //allowPolicyS4 = dt.Rows[i]["pp4"].ObjToString();
                        //allowPolicyS5 = dt.Rows[i]["pp5"].ObjToString();
                        //allowPolicyS6 = dt.Rows[i]["pp6"].ObjToString();
                        //allowPolicyS7 = dt.Rows[i]["pp7"].ObjToString();
                        //allowPolicyS8 = dt.Rows[i]["pp8"].ObjToString();
                        //allowPolicyAmt1 = dt.Rows[i]["ppa1"].ObjToString();
                        //allowPolicyAmt2 = dt.Rows[i]["ppa2"].ObjToString();
                        //allowPolicyAmt3 = dt.Rows[i]["ppa3"].ObjToString();
                        //allowPolicyAmt4 = dt.Rows[i]["ppa4"].ObjToString();
                        //allowPolicyAmt5 = dt.Rows[i]["ppa5"].ObjToString();
                        //allowPolicyAmt6 = dt.Rows[i]["ppa6"].ObjToString();
                        //allowPolicyAmt7 = dt.Rows[i]["ppa7"].ObjToString();
                        //allowPolicyAmt8 = dt.Rows[i]["ppa8"].ObjToString();
                        areaCode = dt.Rows[i]["area"].ObjToString();
                        phoneNumber = dt.Rows[i]["phne"].ObjToString();
                        extraItemAmtMR1 = dt.Rows[i]["mr1"].ObjToString();
                        extraItemAmtMR2 = dt.Rows[i]["mr2"].ObjToString();
                        //extraItemAmtMR3 = dt.Rows[i]["mr3"].ObjToString();
                        //extraItemAmtMR4 = dt.Rows[i]["mr4"].ObjToString();
                        //extraItemAmtMR5 = dt.Rows[i]["mr5"].ObjToString();
                        //extraItemAmtMR6 = dt.Rows[i]["mr6"].ObjToString();
                        //extraItemAmtMR7 = dt.Rows[i]["mr7"].ObjToString();
                        //extraItemAmtMR8 = dt.Rows[i]["mr8"].ObjToString();
                        directorSaleCode = dt.Rows[i]["dnum"].ObjToString();
                        birthDate = GetSQLDate(dt, i, "bdate8");
                        firstPayDate = GetSQLDate(dt, i, "fpay8");
                        contractDate = GetSQLDate(dt, i, "cdte8");
                        if (!String.IsNullOrWhiteSpace(areaCode))
                            phoneNumber = "(" + areaCode + ") " + phoneNumber;
                        lapsed = "";
                        if (workwhat.ToUpper() == "PREMSTLAP")
                            lapsed = "Y";

                        G1.update_db_table("customers", "record", record, new string[] { "firstName", firstName, "lastName", lastName, "address1", address1,
                        "address2", address2, "city", city, "state", state, "zip1", zip1, "zip2", zip2, "sex", sex, "ssn", ssn, "agentCode", agentCode, "coverageType", coverageType, "deleteFlag", deleteFlag,
                        "areaCode", areaCode, "phoneNumber", phoneNumber, "directorSaleCode", directorSaleCode, "birthDate", birthDate, "firstPayDate", firstPayDate, "contractDate", contractDate,
                         "pulled", "2", "lapsed", "", "phoneNumber1", phoneNumber, "lapsed", lapsed });

                        //*                        G1.update_db_table("customers", "record", record, new string[] { "address2", address2, "city", city, "state", state, "zip1", zip1, "zip2", zip2 });
                        //*                        G1.update_db_table("customers", "record", record, new string[] { "sex", sex, "ssn", ssn, "agentCode", agentCode, "coverageType", coverageType, "deleteFlag", deleteFlag });
                        //G1.update_db_table("customers", "record", record, new string[] { "extraItemAmtMI1", extraItemAmtMI1, "extraItemAmtMI2", extraItemAmtMI2, "extraItemAmtMI3", extraItemAmtMI3, "extraItemAmtMI4", extraItemAmtMI4, "extraItemAmtMI5", extraItemAmtMI5, "extraItemAmtMI6", extraItemAmtMI6, "extraItemAmtMI7", extraItemAmtMI7, "extraItemAmtMI8", extraItemAmtMI8 });
                        //G1.update_db_table("customers", "record", record, new string[] { "allowMerDesc1", allowMerDesc1, "allowMerDesc2", allowMerDesc2, "allowMerDesc3", allowMerDesc3, "allowMerDesc4", allowMerDesc4, "allowMerAmt1", allowMerAmt1, "allowMerAmt2", allowMerAmt2, "allowMerAmt3", allowMerAmt3, "allowMerAmt4", allowMerAmt4 });
                        //G1.update_db_table("customers", "record", record, new string[] { "allowPolicyS1", allowPolicyS1, "allowPolicyS2", allowPolicyS2, "allowPolicyS3", allowPolicyS3, "allowPolicyS4", allowPolicyS4, "allowPolicyS5", allowPolicyS5, "allowPolicyS6", allowPolicyS6, "allowPolicyS7", allowPolicyS7, "allowPolicyS8", allowPolicyS8 });
                        //G1.update_db_table("customers", "record", record, new string[] { "allowPolicyAmt1", allowPolicyAmt1, "allowPolicyAmt2", allowPolicyAmt2, "allowPolicyAmt3", allowPolicyAmt3, "allowPolicyAmt4", allowPolicyAmt4, "allowPolicyAmt5", allowPolicyAmt5, "allowPolicyAmt6", allowPolicyAmt6, "allowPolicyAmt7", allowPolicyAmt7, "allowPolicyAmt8", allowPolicyAmt8 });
                        //G1.update_db_table("customers", "record", record, new string[] { "extraItemAmtMR1", extraItemAmtMR1, "extraItemAmtMR2", extraItemAmtMR2, "extraItemAmtMR3", extraItemAmtMR3, "extraItemAmtMR4", extraItemAmtMR4, "extraItemAmtMR5", extraItemAmtMR5, "extraItemAmtMR6", extraItemAmtMR6, "extraItemAmtMR7", extraItemAmtMR7, "extraItemAmtMR8", extraItemAmtMR8 });
                        //*                        G1.update_db_table("customers", "record", record, new string[] { "areaCode", areaCode, "phoneNumber", phoneNumber, "directorSaleCode", directorSaleCode, "birthDate", birthDate, "firstPayDate", firstPayDate, "contractDate", contractDate });


                        //*                        G1.update_db_table("customers", "record", record, new string[] { "pulled", "2", "lapsed", "", "phoneNumber1", phoneNumber });
                        //if (workwhat.ToUpper() == "PREMSTLAP")
                        //{
                        //    G1.update_db_table("customers", "record", record, new string[] { "lapsed", "Y" });
                        //}
                    }
                    catch (Exception ex)
                    {
                        dt.Rows[i]["num"] = "*ERROR*";
                    }
                    //                    picLoader.Refresh();
                }
                picLoader.Hide();
                barImport.Value = lastrow;
                MessageBox.Show("Customer Data Import of " + lastrow + " Rows Complete . . .");
            }
            catch (Exception ex)
            {
                picLoader.Hide();
                MessageBox.Show("***ERROR*** Creating Customer Record/Row! " + contract + "/" + tableRow.ToString() + " Stopping! " + ex.Message.ObjToString());
            }
        }
        /***********************************************************************************************/
        private void AddServices(string contractNumber, string service, string data, string type, bool replace = true)
        {
            string record = "";
            string cmd = "Select * from `cust_services` where `contractNumber` = '" + contractNumber + "' ";
            cmd += " and `service` = '" + service + "';";
            DataTable customerDt = G1.get_db_data(cmd);
            if (customerDt.Rows.Count > 0)
            {
                if (!replace)
                    return;
                record = customerDt.Rows[0]["record"].ObjToString();
            }
            else
                record = G1.create_record("cust_services", "data", "-1");
            if (G1.BadRecord("cust_services", record))
                return;
            G1.update_db_table("cust_services", "record", record, new string[] { "service", service, "data", data, "type", type, "contractNumber", contractNumber });
            customerDt.Dispose();
            customerDt = null;
        }
        /***********************************************************************************************/
        private void ImportCustomerMerchandise(DataTable dt, string workwhat)
        {
            picLoader.Show();
            DataTable dx = null;
            string cmd = "";
            string record = "";
            string contract = "";
            string extraItemAmtMI1 = "";
            string extraItemAmtMI2 = "";
            string extraItemAmtMI3 = "";
            string extraItemAmtMI4 = "";
            string extraItemAmtMI5 = "";
            string extraItemAmtMI6 = "";
            string extraItemAmtMI7 = "";
            string extraItemAmtMI8 = "";
            string extraItemAmtMR1 = "";
            string extraItemAmtMR2 = "";
            string extraItemAmtMR3 = "";
            string extraItemAmtMR4 = "";
            string extraItemAmtMR5 = "";
            string extraItemAmtMR6 = "";
            string extraItemAmtMR7 = "";
            string extraItemAmtMR8 = "";
            double casketPrice = 0D;
            double vaultPrice = 0D;
            labelMaximum.Show();
            labelMaximum.Text = "0";
            barImport.Show();

            int lastrow = dt.Rows.Count;
            int tableRow = 0;
            //            lastrow = 1;
            try
            {
                lblTotal.Show();
                barImport.Minimum = 0;
                barImport.Maximum = lastrow;
                lblTotal.Text = "of " + lastrow.ToString();
                lblTotal.Refresh();
                picLoader.Show();
                int start = 0;
                //                start = 9000;

                for (int i = start; i < lastrow; i++)
                {
                    picLoader.Refresh();
                    barImport.Value = i;
                    barImport.Refresh();
                    labelMaximum.Text = i.ToString();
                    labelMaximum.Refresh();

                    tableRow = i;
                    record = "";
                    try
                    {
                        contract = dt.Rows[i]["cnum"].ObjToString();
                        if (String.IsNullOrWhiteSpace(contract))
                            continue;
                        if (contract == "/")
                            contract = "X" + i.ToString();
                        else if (contract == "\\")
                            contract = "X" + i.ToString();

                        cmd = "Select * from `customers` where `contractNumber` = '" + contract + "';";
                        dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count <= 0)
                            continue;
                        record = dx.Rows[0]["record"].ObjToString();
                        extraItemAmtMI1 = dt.Rows[i]["mi1"].ObjToString(); // Casket Code
                        extraItemAmtMI1 = extraItemAmtMI1.Replace("-", "");
                        extraItemAmtMI2 = dt.Rows[i]["mi2"].ObjToString(); // Vault Code
                        extraItemAmtMI3 = dt.Rows[i]["mi3"].ObjToString();
                        extraItemAmtMI4 = dt.Rows[i]["mi4"].ObjToString();
                        extraItemAmtMI5 = dt.Rows[i]["mi5"].ObjToString();
                        extraItemAmtMI6 = dt.Rows[i]["mi6"].ObjToString();
                        extraItemAmtMI7 = dt.Rows[i]["mi7"].ObjToString();
                        extraItemAmtMI8 = dt.Rows[i]["mi8"].ObjToString();

                        extraItemAmtMR1 = dt.Rows[i]["mr1"].ObjToString(); // Casket Price1
                        extraItemAmtMR2 = dt.Rows[i]["mr2"].ObjToString(); // Vault Price1
                        extraItemAmtMR3 = dt.Rows[i]["mr3"].ObjToString(); // Casket Price2, add to Casket Price1 for Total Price
                        extraItemAmtMR4 = dt.Rows[i]["mr4"].ObjToString(); // Vault Price2, add to Vault Price1 for Total Price
                        extraItemAmtMR5 = dt.Rows[i]["mr5"].ObjToString();
                        extraItemAmtMR6 = dt.Rows[i]["mr6"].ObjToString();
                        extraItemAmtMR7 = dt.Rows[i]["mr7"].ObjToString();
                        extraItemAmtMR8 = dt.Rows[i]["mr8"].ObjToString();

                        casketPrice = extraItemAmtMR1.ObjToDouble() + extraItemAmtMR3.ObjToDouble();
                        vaultPrice = extraItemAmtMR2.ObjToDouble() + extraItemAmtMR4.ObjToDouble();

                        G1.update_db_table("customers", "record", record, new string[] { "extraItemAmtMI1", extraItemAmtMI1, "extraItemAmtMI2", extraItemAmtMI2, "extraItemAmtMI3", extraItemAmtMI3, "extraItemAmtMI4", extraItemAmtMI4, "extraItemAmtMI5", extraItemAmtMI5, "extraItemAmtMI6", extraItemAmtMI6, "extraItemAmtMI7", extraItemAmtMI7, "extraItemAmtMI8", extraItemAmtMI8 });
                        G1.update_db_table("customers", "record", record, new string[] { "extraItemAmtMR1", extraItemAmtMR1, "extraItemAmtMR2", extraItemAmtMR2, "extraItemAmtMR3", extraItemAmtMR3, "extraItemAmtMR4", extraItemAmtMR4, "extraItemAmtMR5", extraItemAmtMR5, "extraItemAmtMR6", extraItemAmtMR6, "extraItemAmtMR7", extraItemAmtMR7, "extraItemAmtMR8", extraItemAmtMR8 });

                        AddServices(contract, "Casket Name", extraItemAmtMI1, "Merchandise", false);
                        AddServices(contract, "Casket Price", casketPrice.ToString(), "Merchandise", false);
                        AddServices(contract, "Outer Container Name", extraItemAmtMI2, "Merchandise", false);
                        AddServices(contract, "Outer Container Price", vaultPrice.ToString(), "Merchandise", false);
                    }
                    catch (Exception ex)
                    {
                        dt.Rows[i]["num"] = "*ERROR*";
                    }
                    //                    picLoader.Refresh();
                }
                picLoader.Hide();
                barImport.Value = lastrow;
                MessageBox.Show("Customer Data Import of " + lastrow + " Rows Complete . . .");
            }
            catch (Exception ex)
            {
                picLoader.Hide();
                MessageBox.Show("***ERROR*** Creating Customer Record/Row! " + contract + "/" + tableRow.ToString() + " Stopping! " + ex.Message.ObjToString());
            }
        }
        /***********************************************************************************************/
        public static string GetSQLDate(DataTable dt, int row, string columnName)
        {
            string date = dt.Rows[row][columnName].ObjToString();
            string sql_date = G1.date_to_sql(date).Trim();
            if (sql_date == "0001-01-01")
                sql_date = "0000-00-00";
            return sql_date;
        }
        /***********************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.mainGrid.OptionsFind.AlwaysVisible == true)
                    mainGrid.OptionsFind.AlwaysVisible = false;
                else
                    mainGrid.OptionsFind.AlwaysVisible = true;
            }
            catch ( Exception ex )
            {

            }
        }
        /***********************************************************************************************/
        private void ProcessDate(DataTable dt, string column)
        {
            string date = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                date = dt.Rows[i][column].ObjToString();
                if (date.IndexOf("E+") > 0)
                {
                    date = ConvertScientificDate(date);
                    dt.Rows[i][column] = date;
                }
            }
        }
        /***********************************************************************************************/
        public static string ConvertScientificDate(string date)
        {
            decimal h2 = Decimal.Parse(date, NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint);
            date = h2.ToString();
            if (date.Length >= 8)
                date = date.Substring(0, 8);
            string year = date.Substring(0, 4);
            string month = date.Substring(4, 2);
            string day = date.Substring(6, 2);
            if (day == "00")
                day = "01";
            date = year + month + day;
            return date;
        }
        /***********************************************************************************************/
        private int pageMarginLeft = 0;
        private int pageMarginRight = 0;
        private int pageMarginTop = 0;
        private int pageMarginBottom = 0;
        /***********************************************************************************************/
        private void printPreviewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (workWhat.ToUpper() != "NEWCONTRACTS")
            {
                printPreview();
                return;
            }
            printPreview();
        }
        /***********************************************************************************************/
        private void printPreview()
        {
            if (this.components == null)
                this.components = new System.ComponentModel.Container();

            DevExpress.XtraPrinting.PrintingSystem printingSystem1 = new DevExpress.XtraPrinting.PrintingSystem(this.components);
            DevExpress.XtraPrinting.PrintableComponentLink printableComponentLink1 = new DevExpress.XtraPrinting.PrintableComponentLink(this.components);

            printingSystem1.PageSettingsChanged += PrintingSystem1_PageSettingsChanged;

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
        }
        /***********************************************************************************************/
        private void PrintingSystem1_PageSettingsChanged(object sender, EventArgs e)
        {
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
            Printer.DrawQuad(4, 8, 7, 4, "Import Data for (" + workWhat + ") File: " + actualFile, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

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
        private void mainGrid_DoubleClick(object sender, EventArgs e)
        {
            if (workWhat.ToUpper() != "NEWCONTRACTS")
                return;
            DataRow dr = mainGrid.GetFocusedDataRow();
            string contract = dr["TRUST_NUMBER"].ObjToString();
            if (!String.IsNullOrWhiteSpace(contract))
            {
                string cmd = "Select * from `customers` where `contractNumber` = '" + contract + "';";
                DataTable dx = G1.get_db_data(cmd);
                if (dx.Rows.Count <= 0)
                {
                    MessageBox.Show("***ERROR*** Contract Number " + contract + " Does Not Exist");
                    return;
                }
                this.Cursor = Cursors.WaitCursor;
                DataTable dt = (DataTable)dgv.DataSource;
                CustomerDetails clientForm = new CustomerDetails(contract);
                clientForm.Show();
                this.Cursor = Cursors.Default;
            }
        }
        /***********************************************************************************************/
        private void mainGrid_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
                return;
            if (workWhat.ToUpper() == "NEWCONTRACTS")
            {
                DevExpress.XtraGrid.Columns.GridColumn column = mainGrid.FocusedColumn;
                if (column.FieldName.ToUpper() == "AGENTCODE")
                {
                    try
                    {
                        DataRow dr = mainGrid.GetFocusedDataRow();
                        int rowHandle = mainGrid.FocusedRowHandle;
                        int row = mainGrid.GetDataSourceRowIndex(rowHandle);
                        DataTable dt = (DataTable)dgv.DataSource;
                        string agentCode = dr["agentCode"].ObjToString();
                        dt.Rows[row]["agentCode"] = agentCode;
                        if (!String.IsNullOrWhiteSpace(agentCode))
                        {
                            string name = CustomerDetails.GetAgentName(agentCode);
                            dr["agentName"] = name;
                            mainGrid.RefreshData();
                            dgv.Refresh();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("***ERROR*** Problem changing Agent Code!\nCall Administrator!");
                    }
                }
            }
        }
        /***********************************************************************************************/
        private void mainGrid_CustomDrawCell(object sender, RowCellCustomDrawEventArgs e)
        {
            bool debug = true;
            if (e.Column.FieldName.ToUpper() == "NUM")
            {
                if (e.RowHandle >= 0)
                {
                    if (e.DisplayText.ToUpper() == "ERROR")
                    {
                        e.Appearance.ForeColor = Color.Red;
                    }
                    else
                    {
                        string num = (e.RowHandle + 1).ToString();
                        if ( debug )
                            e.DisplayText = num;
                    }
                }
            }
            else if (e.Column.FieldName.ToUpper() == "DATEDPPAID")
            {
                if (e.RowHandle >= 0)
                {
                    string date = e.DisplayText;
                    if ( !String.IsNullOrWhiteSpace ( date))
                    {
                        DateTime date1 = date.ObjToDateTime();
                        if (date1.Year > 1850)
                            e.DisplayText = date1.ToString("MM/dd/yyyy");
                        else
                        {
                            e.DisplayText = "";
                            DataTable dt = (DataTable)dgv.DataSource;
                            dt.Rows[e.RowHandle]["DATEDPPAID"] = "";
                            dgv.DataSource = dt;
                        }
                    }
                }
            }
            if (workWhat.ToUpper() == "ALL_LAPSES")
            {
                if (e.RowHandle < 0)
                    return;
                DataTable dt = (DataTable)dgv.DataSource;
                int row = e.RowHandle;
                if (e.Column.FieldName.ToUpper() == "MYISSUEDATE")
                {
                    DateTime myIssueDate = dt.Rows[row]["MyIssueDate"].ObjToDateTime();
                    DateTime issueDate = dt.Rows[row]["Issue Date"].ObjToDateTime();
                    int myMonth = myIssueDate.Month;
                    int myYear = myIssueDate.Year;
                    int Month = issueDate.Month;
                    int Year = issueDate.Year;
                    if ( myMonth != Month || myYear != Year )
                    {
                        e.Appearance.BackColor = Color.Red;
                    }
                }
                if (e.Column.FieldName.ToUpper() == "MYLAPSEDATE")
                {
                    DateTime myLapseDate = dt.Rows[row]["MyLapseDate"].ObjToDateTime();
                    DateTime lapseDate = dt.Rows[row]["Lapse Date"].ObjToDateTime();
                    int myMonth = myLapseDate.Month;
                    int myYear = myLapseDate.Year;
                    int Month = lapseDate.Month;
                    int Year = lapseDate.Year;
                    if (myMonth != Month || myYear != Year)
                    {
                        e.Appearance.BackColor = Color.Red;
                    }
                }
            }
            if (workWhat.ToUpper() == "ALL_REINSTATES")
            {
                if (e.RowHandle < 0)
                    return;
                DataTable dt = (DataTable)dgv.DataSource;
                int row = e.RowHandle;
                if (e.Column.FieldName.ToUpper() == "MYISSUEDATE")
                {
                    DateTime myIssueDate = dt.Rows[row]["MyIssueDate"].ObjToDateTime();
                    DateTime issueDate = dt.Rows[row]["Issue Date"].ObjToDateTime();
                    int myMonth = myIssueDate.Month;
                    int myYear = myIssueDate.Year;
                    int Month = issueDate.Month;
                    int Year = issueDate.Year;
                    if (myMonth != Month || myYear != Year)
                    {
                        e.Appearance.BackColor = Color.Red;
                    }
                }
                if (e.Column.FieldName.ToUpper() == "MYREINSTATEDATE")
                {
                    DateTime myReinstateDate = dt.Rows[row]["MyReinstateDate"].ObjToDateTime();
                    DateTime reinstateDate = dt.Rows[row]["Reinstate Date"].ObjToDateTime();
                    int myMonth = myReinstateDate.Month;
                    int myYear = myReinstateDate.Year;
                    int Month = reinstateDate.Month;
                    int Year = reinstateDate.Year;
                    if (myMonth != Month || myYear != Year)
                    {
                        e.Appearance.BackColor = Color.Red;
                    }
                }
            }
        }
        /***********************************************************************************************/
        private void fixAgentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string cmd = "";
            DataRow dr = mainGrid.GetFocusedDataRow();
            string contract = dr["TRUST_NUMBER"].ObjToString();
            DataTable dx = new DataTable();
            if (!String.IsNullOrWhiteSpace(contract))
            {
                cmd = "Select * from `customers` where `contractNumber` = '" + contract + "';";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count <= 0)
                {
                    MessageBox.Show("***ERROR*** Contract Number " + contract + " Does Not Exist");
                    return;
                }
            }
            string ssn = dr["INSURED_SSN"].ObjToString();
            string oldAgent = dr["OldAgent"].ObjToString();
            string newAgent = dr["agentCode"].ObjToString();
            string name = CustomerDetails.GetAgentName(newAgent);
            dr["agentName"] = name;
            dr["OldAgent"] = newAgent;
            string record = dx.Rows[0]["record"].ObjToString();
            G1.update_db_table("customers", "record", record, new string[] { "agentCode", newAgent, "ssn", ssn });
            cmd = "Select * from `payments` where `contractNumber` = '" + contract + "';";
            dx = G1.get_db_data(cmd);
            for ( int i=0; i<dx.Rows.Count; i++)
            {
                record = dx.Rows[i]["record"].ObjToString();
                G1.update_db_table("payments", "record", record, new string[] { "agentNumber", newAgent});
            }
            mainGrid.RefreshData();
            dgv.Refresh();
        }
        /***********************************************************************************************/
        private void btnFixAll_Click(object sender, EventArgs e)
        {
            string cmd = "";
            DataTable dt = (DataTable)dgv.DataSource;
            string contract = "";
            string issueDate = "";
            string record = "";
            DataTable dx = null;
            string diff = "";
            DateTime date = DateTime.Now;
            this.Cursor = Cursors.WaitCursor;
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                diff = dt.Rows[i]["diff"].ObjToString();
                if ( diff.ToUpper() == "DIFF")
                {
                    contract = dt.Rows[i]["CNUM"].ObjToString();
                    cmd = "Select * from `contracts` where `contractNumber` = '" + contract + "';";
                    dx = G1.get_db_data(cmd);
                    if ( dx.Rows.Count > 0 )
                    {
                        record = dx.Rows[0]["record"].ObjToString();
                        date = dt.Rows[i]["ISSDT8"].ObjToDateTime();
                        issueDate = date.ToString("MM/dd/yyyy");
                        G1.update_db_table("contracts", "record", record, new string[] {"issueDate8", issueDate });
                    }
                }
            }

        }
        /***********************************************************************************************/
        private void btnFixAll_Clickx(object sender, EventArgs e)
        {
            string cmd = "";
            DataTable dt = (DataTable)dgv.DataSource;
            string contract = "";
            string issueDate = "";
            string record = "";
            DataTable dx = null;
            this.Cursor = Cursors.WaitCursor;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                contract = dt.Rows[i]["TRUST_NUMBER"].ObjToString();
                issueDate = dt.Rows[i]["TRUST_SEQ_DATE"].ObjToString();
                if (issueDate.Length >= 8)
                    issueDate = issueDate.Substring(0, 8);
                if (G1.validate_date(issueDate))
                {
                    cmd = "Select * from `contracts` where `contractNumber` = '" + contract + "';";
                    dx = G1.get_db_data(cmd);
                    if ( dx.Rows.Count > 0 )
                    {
                        record = dx.Rows[0]["record"].ObjToString();
                        G1.update_db_table("contracts", "record", record, new string[] { "issueDate8", issueDate});
                    }
                }
            }
            this.Cursor = Cursors.Default;
        }
        /***************************************************************************************/
        public delegate void d_void_eventdone_dt(DataTable dt);
        public event d_void_eventdone_dt SelectDone;
        protected void OnSelectDone(DataTable dt)
        {
            SelectDone?.Invoke(dt);
        }
        /***********************************************************************************************/
    }
}
