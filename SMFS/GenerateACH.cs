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
using DevExpress.Office.Utils;
using DevExpress.XtraGrid;
//using java.awt;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class GenerateACH : DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        private string workWho = "";
        private bool workEdit = false;
        private bool workReport = false;
        private string lastFileCreated = "";
        /***********************************************************************************************/
        public GenerateACH(string who, bool edit = false, bool reporting = false)
        {
            InitializeComponent();
            workEdit = edit;
            workReport = reporting;
            workWho = who;
        }
        /***********************************************************************************************/
        private void GenerateACH_Load(object sender, EventArgs e)
        {
            btnSave.Hide();
            loadLocations();
            if (workEdit || workReport)
                tabControl1.TabPages.Remove(tabPage2);
            if (workEdit)
            {
                tabPage2.Hide();
                lblEffectiveDate.Hide();
                this.dateTimePicker1.Hide();
                btnRun.Hide();
                btnGenerateFile.Hide();
                chkProblems.Hide();
                btnImport.Hide();
                if (workReport)
                {
                    pictureDelete.Hide();
                    this.Text = "ACH Customers Report";
                    miscToolStripMenuItem.Dispose();
                }
                LoadData();
                if ( !LoginForm.administrator )
                    picAdd.Hide();
                G1.SetupToolTip(pictureDelete, "Delete Current ACH Permanently");
            }
            else
            {
                gridMain.Columns["dateBeginning"].Visible = false;
                this.Text = "Generate ACH File";
                btnGenerateFile.Hide();
                chkProblems.Hide();
                btnImport.Hide();
                G1.SetupToolTip(pictureDelete, "Delete Current ACH Payment");
                G1.SetupToolTip(picAdd, "Add Extra ACH Payment");

                gridMain.Columns["payment"].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Custom;
                gridMain.Columns["payment"].SummaryItem.DisplayFormat = "{0:C2}";

                gridMain2.Columns["payment"].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Custom;
                gridMain2.Columns["payment"].SummaryItem.DisplayFormat = "{0:C2}";
            }
            if (!String.IsNullOrWhiteSpace(workWho))
                this.Text = "Generate ACH Payments for " + workWho;
        }
        /***********************************************************************************************/
        private void loadLocations()
        {
            string cmd = "Select * from `funeralhomes` order by `keycode`;";
            DataTable locDt = G1.get_db_data(cmd);
            for (int i = 0; i < locDt.Rows.Count; i++)
            {
                this.repositoryItemComboBox1.Items.Add(locDt.Rows[i]["locationCode"].ObjToString());
                this.repositoryItemComboBox2.Items.Add(locDt.Rows[i]["locationCode"].ObjToString());
            }
        }
        /***********************************************************************************************/
        private void LoadData()
        {
            string cmd = "Select * from `ach` a LEFT JOIN `customers` c ON a.`contractNumber` = c.`contractNumber` LEFT JOIN `icustomers` b ON a.`contractNumber` = b.`contractNumber` ORDER by `dayOfMonth`;";
            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("name");

            string firstName = "";
            string lastName = "";

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                firstName = dt.Rows[i]["firstName"].ObjToString();
                lastName = dt.Rows[i]["lastName"].ObjToString();
                if (String.IsNullOrWhiteSpace(firstName))
                    dt.Rows[i]["firstName"] = dt.Rows[i]["firstName1"].ObjToString();
                if (String.IsNullOrWhiteSpace(lastName))
                    dt.Rows[i]["lastName"] = dt.Rows[i]["lastName1"].ObjToString();

                firstName = dt.Rows[i]["firstName"].ObjToString();
                lastName = dt.Rows[i]["lastName"].ObjToString();
                dt.Rows[i]["name"] = lastName + ", " + firstName;
            }
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            //gridMain.Columns["payment"].Visible = false;
            gridMain.Columns["effectiveDate"].Visible = false;
            gridMain.Columns["ID"].Visible = false;
            gridMain.Columns["DebitCredit"].Visible = false;
        }
        /***********************************************************************************************/
        private void btnRun_Click(object sender, EventArgs e)
        {
            if (workWho.ToUpper() == "BANK PLUS")
            {
                string achDirectory = AdminOptions.GetOptionAnswer("BANKPLUS Path");
                if (String.IsNullOrWhiteSpace(achDirectory))
                {
                    MessageBox.Show("***Problem*** You must first SETUP a BANK PLUS Directory using the MISC Menu Option!", "Setup BANK PLUS  Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Cursor = Cursors.Default;
                    return;
                }
            }
            this.Cursor = Cursors.WaitCursor;

            DateTime date = dateTimePicker1.Value;
            date = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);
            DateTime effectiveDate = dateTimePicker1.Value;
            DateTime date1 = this.dateTimePicker2.Value;
            DateTime date2 = this.dateTimePicker3.Value;

            string newDate = date1.ToString("yyyy-MM-dd 00:00:00");
            date1 = newDate.ObjToDateTime();

            newDate = date2.ToString("yyyy-MM-dd 23:59:59");
            date2 = newDate.ObjToDateTime();

            string dom1 = this.dateTimePicker2.Value.Day.ObjToString();
            string dom2 = this.dateTimePicker3.Value.Day.ObjToString();
            //if (dom2.ObjToInt32() < dom1.ObjToInt32())
            //{
            //    dom2 = DateTime.DaysInMonth(date1.Year, date1.Month).ObjToString();
            //    date2 = new DateTime(date1.Year, date1.Month, DateTime.DaysInMonth(date1.Year, date1.Month));
            //}
            string dom = "";
            int day = 0;

            //string dom = this.dateTimePicker1.Value.Day.ObjToString();
            //DateTime date = this.dateTimePicker1.Value;
            string cmd = "Select * from `ach` WHERE ";
            //cmd += " ((`dayOfMonth` >= '" + dom1 + "' AND `dayOfMonth` <= '" + dom2 + "' ) ";
            string tDom = "";
            date = this.dateTimePicker2.Value;
            date = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);
            int lastDOM = DateTime.DaysInMonth(this.dateTimePicker2.Value.Year, this.dateTimePicker2.Value.Month);
            for (; ;)
            {
                dom = date.Day.ObjToString();
                tDom += "'" + dom + "',";
                day = date.Day;
                if ( day == lastDOM )
                {
                    day++;
                    for (int i = day; i <= 32; i++)
                        tDom += "'" + i.ToString() + "',";
                }
                date = date.AddDays(1);
                if (date > this.dateTimePicker3.Value)
                {
                    date = this.dateTimePicker3.Value;
                    date = new DateTime(date.Year, date.Month, date.Day, 23, 59, 0);
                    break;
                }
            }
            tDom = tDom.TrimEnd(',');
            cmd += " ( `dayOfMonth` IN (" + tDom + ") ";
            cmd += " OR ( `dateBeginning` >= '" + date1.ToString("yyyy-MM-dd 00:00:00") + "' AND `dateBeginning` <= '" + date2.ToString("yyyy-MM-dd 23:59:59") + "' ) ) ";
            cmd += " AND ( `leftPayments` > '0' OR `numPayments` = '999' ) ";
            cmd += ";";

            //int days1 = DateTime.DaysInMonth(date1.Year, date1.Month);
            //int days2 = DateTime.DaysInMonth(date2.Year, date2.Month);
            DataTable dt = G1.get_db_data(cmd);

            dt.Columns.Add("effectiveDate");
            dt.Columns.Add("name");
            dt.Columns.Add("ID");
            dt.Columns.Add("DebitCredit");
            //dt.Columns.Add("status");
            dt.Columns.Add("backupName");
            dt.Columns.Add("location");

            DataTable sDt = dt.Clone();

            string contractNumber = "";
            string payer = "";
            double payment = 0D;
            double amtOfPayment = 0D;
            double balance = 0D;
            DataTable dx = null;
            string firstName = "";
            string lastName = "";
            int frequency = 0;
            DateTime dolp = DateTime.Now;
            int months = 0;
            DateTime dueDate8 = DateTime.Now;
            string id = "";
            DateTime dateBeginning = DateTime.Now;
            DateTime deceasedDate = DateTime.Now;
            DateTime lapseDate = DateTime.Now;
            DateTime reinstateDate = DateTime.Now;
            string lapsed = "";
            string status = "";
            int leftPayments = 0;
            int numPayments = 0;
            string legacy = "";
            bool good = true;
            string spayment = "";
            string trust = "";
            string loc = "";
            string contract = "";
            string location = "";
            string oldloc = "";
            string currentStatus = "";
            string str = "";
            double premium = 0D;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    status = "";
                    contractNumber = "";
                    payer = "";
                    currentStatus = dt.Rows[i]["status"].ObjToString();
                    legacy = dt.Rows[i]["legacy"].ObjToString();
                    if (legacy.ToUpper() == "Y")
                        legacy = "Legacy";
                    leftPayments = dt.Rows[i]["leftPayments"].ObjToInt32();
                    numPayments = dt.Rows[i]["numPayments"].ObjToInt32();

                    dt.Rows[i]["effectiveDate"] = effectiveDate.ToString("MM/dd/yyyy");
                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                    if ( contractNumber == "T17003UI")
                    {
                    
                    }
                    if (contractNumber == "ZZ0002137")
                    {

                    }
                    payer = dt.Rows[i]["payer"].ObjToString();
                    premium = Policies.CalcMonthlyPremium(payer, this.dateTimePicker1.Value );
                    if ( payer.ToUpper() == "CG-WL10656")
                    {
                    }
                    frequency = dt.Rows[i]["frequencyInMonths"].ObjToInt32();
                    payment = dt.Rows[i]["payment"].ObjToDouble();
                    dateBeginning = dt.Rows[i]["dateBeginning"].ObjToDateTime();
                    if (dateBeginning > date1 && dateBeginning > date2)
                        status = "Date Skipped";
                    if ( dateBeginning >= date1 && dateBeginning <= date2 )
                        status = "Date Match";
//                    if (dateBeginning.ToString("MM/dd/yyyy") == effectiveDate.ToString("MM/dd/yyyy"))
                    firstName = "";
                    lastName = "";
                    dolp = DateTime.Now;

                    if (!String.IsNullOrWhiteSpace(payer) || contractNumber.ToUpper().Contains("ZZ"))
                    {
                        if (premium <= 0D)
                            status = "Bad Premium";
                        cmd = "Select * from `icontracts` c JOIN `icustomers` b ON c.`contractNumber` = b.`contractNumber` WHERE c.`contractNumber` = '" + contractNumber + "';";
                        dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count > 0)
                        {
                            payer = dx.Rows[0]["payer"].ObjToString();
                            location = ImportDailyDeposits.FindLastPaymentLocation(payer, ref oldloc);

                            //if (!String.IsNullOrWhiteSpace(payer))
                            //    payment = Policies.CalcMonthlyPremium(payer, date);
                            lapsed = dx.Rows[0]["lapsed"].ObjToString();
                            lapseDate = dx.Rows[0]["lapseDate8"].ObjToDateTime();
                            deceasedDate = dx.Rows[0]["deceasedDate"].ObjToDateTime();
                            reinstateDate = dx.Rows[0]["reinstateDate8"].ObjToDateTime();
                            firstName = dx.Rows[0]["firstName"].ObjToString();
                            lastName = dx.Rows[0]["lastName"].ObjToString();
                            dolp = dx.Rows[0]["lastDatePaid8"].ObjToDateTime();
                            dueDate8 = dx.Rows[0]["dueDate8"].ObjToDateTime();
                            amtOfPayment = dx.Rows[0]["amtOfMonthlyPayt"].ObjToDouble();
                            if (dueDate8 == dolp)
                                dolp = DailyHistory.GetDOLPfromPayments(contractNumber);
                            if (deceasedDate.Year > 100)
                                status = "Deceased";
                            else
                            {
                                if (lapsed.ToUpper() == "Y")
                                    status = "Lapsed";
                                //else if (payment > balance)
                                //    status = "Balance";
                            }
                        }
                    }
                    else
                    {
                        cmd = "Select * from `contracts` c JOIN `customers` b ON c.`contractNumber` = b.`contractNumber` WHERE c.`contractNumber` = '" + contractNumber + "';";
                        dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count > 0)
                        {
                            contract = Trust85.decodeContractNumber(contractNumber, ref trust, ref location);
                            location = getLocationCode(location);
                            lapsed = dx.Rows[0]["lapsed"].ObjToString();
                            lapseDate = dx.Rows[0]["lapseDate8"].ObjToDateTime();
                            deceasedDate = dx.Rows[0]["deceasedDate"].ObjToDateTime();
                            reinstateDate = dx.Rows[0]["reinstateDate8"].ObjToDateTime();
                            firstName = dx.Rows[0]["firstName"].ObjToString();
                            lastName = dx.Rows[0]["lastName"].ObjToString();
                            dolp = dx.Rows[0]["lastDatePaid8"].ObjToDateTime();
                            dueDate8 = dx.Rows[0]["dueDate8"].ObjToDateTime();
                            amtOfPayment = dx.Rows[0]["amtOfMonthlyPayt"].ObjToDouble();
                            balance = dx.Rows[0]["balanceDue"].ObjToDouble();
                            if (deceasedDate.Year > 100)
                                status = "Deceased";
                            else
                            {
                                if (lapsed.ToUpper() == "Y")
                                    status = "Lapsed";
                                else if (payment > balance)
                                    status = "Balance";
                            }
                        }
                    }
                    dt.Rows[i]["name"] = lastName + ", " + firstName;
                    dt.Rows[i]["backupName"] = lastName + ", " + firstName;
                    spayment = G1.ReformatMoney(payment);
                    spayment = spayment.Replace(",", "");
                    dt.Rows[i]["payment"] = G1.ReformatMoney(payment);
                    dt.Rows[i]["DebitCredit"] = "Debit";
                    dt.Rows[i]["location"] = location;
                    dt.Rows[i]["ID"] = GenerateRandomId(10);
                    if ( currentStatus.ToUpper() == "PAUSE")
                    {
                        dt.Rows[i]["status"] = "Pause";
                        dt.Rows[i]["ID"] = "";
                        continue;
                    }

                    if (dueDate8.Year >= 2039)
                    {
                        dt.Rows[i]["name"] = "Due Date >= 2039";
                        dt.Rows[i]["status"] = legacy;
                        dt.Rows[i]["ID"] = "";
                        continue;
                    }
                    if (status.ToUpper() == "DECEASED")
                    {
                        dt.Rows[i]["ID"] = "";
                        dt.Rows[i]["status"] = status;
                        continue;
                    }
                    if ( status.ToUpper() == "DATE SKIPPED")
                    {
                        dt.Rows[i]["ID"] = "";
                        dt.Rows[i]["status"] = status;
                        continue;
                    }
                    if ( status.ToUpper() == "BAD PREMIUM")
                    {
                        dt.Rows[i]["ID"] = "";
                        dt.Rows[i]["status"] = status;
                        continue;
                    }
                    if (dolp.Year < 100)
                        dolp = DateTime.Now.AddMonths(-1);
                    if (dolp.Year > 100)
                    {
                        if (status.ToUpper() != "DATE MATCH" && status.ToUpper() != "BALANCE")
                        {
                            //months = G1.GetMonthsBetween(date, dolp);
                            months = G1.GetMonthsBetween(date1, date2, dolp);
                            //if ( dueDate8 > date2 )
                            //{
                            //    dt.Rows[i]["name"] = "Payment not due yet!";
                            //    dt.Rows[i]["status"] = legacy;
                            //    dt.Rows[i]["ID"] = "";
                            //    continue;
                            //}
                            if (months != frequency)
                            {
                                if (months == 0)
                                {
                                    dt.Rows[i]["name"] = "Payment already Exists in this Month";
                                    dt.Rows[i]["status"] = legacy;
                                    dt.Rows[i]["ID"] = "";
                                    continue;
                                }
                                if (months != frequency)
                                {
                                    dt.Rows[i]["name"] = "Frequency Months=" + months.ToString() + " Freq=" + frequency.ToString();
                                    dt.Rows[i]["status"] = legacy;
                                    dt.Rows[i]["ID"] = "";
                                    continue;
                                }
                            }
                        }
                    }
                    dt.Rows[i]["status"] = status;
                    if (DailyHistory.isInsurance(contractNumber))
                    {
                        if (payer == "BB-0392")
                        {
                        }
                        good = CheckForSecNat(payer, payment);
                        if (!good)
                        {
                            dt.Rows[i]["status"] = "SecNat?";
                        }
                        good = CheckFor3rdParty(payer, payment);
                        if (!good)
                        {
                            str = dt.Rows[i]["status"].ObjToString();
                            if (!String.IsNullOrWhiteSpace(str))
                                dt.Rows[i]["status"] += " " + "3rd Party?";
                            else
                                dt.Rows[i]["status"] = "3rd Party?";
                        }
                    }
                }
                catch (Exception ex)
                {
                    if ( !String.IsNullOrWhiteSpace ( payer ))
                        DevExpress.XtraEditors.XtraMessageBox.Show("***ERROR***\nFrequency Issue with Payer (" + payer + ")!", "Frequency Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    else
                        DevExpress.XtraEditors.XtraMessageBox.Show("***ERROR***\nFrequency Issue with Conract (" + contractNumber + ")!", "Frequency Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            int row = 0;
            for (int i = (dt.Rows.Count - 1); i >= 0; i--)
            {
                id = dt.Rows[i]["ID"].ObjToString();
                if (String.IsNullOrWhiteSpace(id))
                {
                    sDt.ImportRow(dt.Rows[i]);
                    row = sDt.Rows.Count - 1;
                    sDt.Rows[row]["ID"] = sDt.Rows[row]["name"].ObjToString();
                    sDt.Rows[row]["name"] = sDt.Rows[row]["backupName"].ObjToString();
                    dt.Rows.RemoveAt(i);
                }
            }

            DataView tempview = dt.DefaultView;
            tempview.Sort = "name asc";
            dt = tempview.ToTable();

            G1.NumberDataTable(dt);
            dgv.DataSource = dt;

            tempview = sDt.DefaultView;
            tempview.Sort = "name asc";
            sDt = tempview.ToTable();

            G1.NumberDataTable(sDt);
            dgv2.DataSource = sDt;
            btnGenerateFile.Show();
            chkProblems.Show();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private string getLocationCode(string loc)
        {
            string location = loc;
            if (funDt == null)
                funDt = G1.get_db_data("Select * from `funeralhomes`;");
            DataRow[] dR = funDt.Select("keycode='" + loc + "'");
            if (dR.Length > 0)
                location = dR[0]["SDICode"].ObjToString();
            if (String.IsNullOrWhiteSpace(location))
                location = loc;
            return location;
        }
        /***********************************************************************************************/
        private DataTable funDt = null;
        private string getLocationName ( string loc )
        {
            string location = loc;
            if (funDt == null)
                funDt = G1.get_db_data("Select * from `funeralhomes`;");
            DataRow[] dR = funDt.Select("keycode='" + loc + "'");
            if (dR.Length > 0)
                location = dR[0]["locationCode"].ObjToString();
            return location;
        }
        /***********************************************************************************************/
        public static string GenerateRandomId(int length)
        {
            char[] stringChars = new char[length];
            byte[] randomBytes = new byte[length];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }

            string chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[randomBytes[i] % chars.Length];
            }

            string rtnString = new string(stringChars);

            return rtnString;

        }
        /***********************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (dgv.Visible)
                G1.SpyGlass(gridMain);
            else if (dgv2.Visible)
                G1.SpyGlass(gridMain2);
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
        private void selectACHDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string directory = "";
            using (var fbd = new FolderBrowserDialog())
            {
                string achPath = AdminOptions.GetOptionAnswer("ACH Path");
                if (!String.IsNullOrWhiteSpace(achPath))
                {
                    string root = Directory.GetDirectoryRoot(achPath);
                    fbd.RootFolder = Environment.SpecialFolder.Desktop;
                    directory = achPath;
                    directory = directory.Replace("/", "\\");
                    fbd.SelectedPath = @directory;
                }
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    directory = fbd.SelectedPath;
                    directory = directory.Replace("\\", "/");
                    directory = directory + "/";
                    string record = AdminOptions.VerifyOption("ACH Path", directory);
                    if (String.IsNullOrWhiteSpace(record))
                        MessageBox.Show("***ERROR*** Creating New ACH Path Option in Options Table!");
                }
            }
        }
        /***********************************************************************************************/
        private void selectBankPlusACHDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string directory = "";
            using (var fbd = new FolderBrowserDialog())
            {
                string achPath = AdminOptions.GetOptionAnswer("BANKPLUS Path");
                if (!String.IsNullOrWhiteSpace(achPath))
                {
                    string root = Directory.GetDirectoryRoot(achPath);
                    fbd.RootFolder = Environment.SpecialFolder.Desktop;
                    directory = achPath;
                    directory = directory.Replace("/", "\\");
                    fbd.SelectedPath = @directory;
                }
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    directory = fbd.SelectedPath;
                    directory = directory.Replace("\\", "/");
                    directory = directory + "/";
                    string record = AdminOptions.VerifyOption("BANKPLUS Path", directory);
                    if (String.IsNullOrWhiteSpace(record))
                        MessageBox.Show("***ERROR*** Creating New BANKPLUS Path Option in Options Table!");
                }
            }
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
            else if (e.Column.FieldName.ToUpper().IndexOf("CODE") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                int row = e.ListSourceRowIndex;
                DataTable dt = (DataTable)dgv.DataSource;
                string payer = dt.Rows[row]["payer"].ObjToString();
                string contractNumber = dt.Rows[row]["contractNumber"].ObjToString();
                if (!String.IsNullOrWhiteSpace(payer))
                    e.DisplayText = "02";
                else if (contractNumber.ToUpper().Contains("ZZ"))
                    e.DisplayText = "02";
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
                using (CustomerDetails clientForm = new CustomerDetails(contract))
                {
                    clientForm.ShowDialog();
                }
                string cmd = "Select * from `ach` where `contractNumber` = '" + contract + "';";
                DataTable dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    string freq = dx.Rows[0]["frequencyInMonths"].ObjToString();
                    string payer = dx.Rows[0]["payer"].ObjToString();
                    string routingNumber = dx.Rows[0]["routingNumber"].ObjToString();
                    string accountNumber = dx.Rows[0]["accountNumber"].ObjToString();
                    string dayOfMonth = dx.Rows[0]["dayOfMonth"].ObjToString();
                    string code = dx.Rows[0]["code"].ObjToString();
                    string acctType = dx.Rows[0]["acctType"].ObjToString();
                    dr["payer"] = payer;
                    dr["frequencyInMonths"] = freq;
                    dr["code"] = code;
                    dr["routingNumber"] = routingNumber;
                    dr["accountNumber"] = accountNumber;
                    dr["acctType"] = acctType;
                    dr["dayOfMonth"] = dayOfMonth;
                    gridMain.RefreshData();
                }
                this.Cursor = Cursors.Default;
            }
        }
        /***********************************************************************************************/
        private void pictureDelete_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            string contractNumber = dr["contractNumber"].ObjToString();
            string payer = dr["payer"].ObjToString();
            string who = contractNumber;
            if (!String.IsNullOrWhiteSpace(payer))
                who = payer;
            DialogResult result = MessageBox.Show("***Question*** Are you sure you want to DELETE this Customers ACH (" + who + ") ?", "Delete ACH Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;

            DataTable dt = (DataTable)dgv.DataSource;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            dt.Rows.RemoveAt(row);
            gridMain.ClearSelection();
            if (workEdit)
                G1.delete_db_table("ach", "record", record);
            who = contractNumber;
            if (!String.IsNullOrWhiteSpace(payer))
                who += " / " + payer;
            if (workEdit)
                G1.AddToAudit(LoginForm.username, "Edit ACH", "ACH", "ACH Payment Removed for " + who, contractNumber);
            else
                G1.AddToAudit(LoginForm.username, "Generate ACH", "ACH", "ACH Payment Removed from List for " + who, contractNumber);
        }
        /***********************************************************************************************/
        //     xxx                     xxxx                   xxxx                                            xxxx                                            xxx                         xxx                  
        //Transaction Date            Status    Payment Type Name On Account Transaction Number  Ref.Number  Customer Number Operation   Location Name       Amount Disp.Acct Number Payment Origin
        //05/29/2020 05:02:41 PM CT   Processed Checking     MARTHA OQUIN	 :15:3101489	     W5N1ZLHLLF1 CT18014LI       Sale        Catchings Funeral   80.44	      7774	     Original Signature
        /***********************************************************************************************/
        private void btnGenerateFile_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            DataTable dt = (DataTable)dgv.DataSource;
            DataTable dx = dt.Copy();
            DataTable originalDt = null;
            string fullPath = "";

            try
            {
                string achDirectory = AdminOptions.GetOptionAnswer("ACH Path");
                if (String.IsNullOrWhiteSpace(achDirectory))
                {
                    MessageBox.Show("***Problem*** You must first SETUP an ACH Directory using the MISC Menu Option!", "Setup ACH Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Cursor = Cursors.Default;
                    return;
                }
                DateTime date = this.dateTimePicker1.Value;
                string filename = "SMFS.Draft " + date.Month.ToString("D2") + "." + date.Day.ToString("D2") + "." + date.Year.ToString("D4");
                fullPath = achDirectory + filename;
                int count = 0;
                for (; ; )
                {
                    if (!File.Exists(fullPath + ".csv"))
                        break;
                    count++;
                    filename = "SMFS.Draft " + date.Month.ToString("D2") + "." + date.Day.ToString("D2") + "." + date.Year.ToString("D4") + "_" + count.ToString();
                    fullPath = achDirectory + filename;
                }

                fullPath += ".csv";

                DataView tempview = dx.DefaultView;
                tempview.Sort = "name asc";
                dx = tempview.ToTable();
                string status = "";
                for (int i = (dx.Rows.Count - 1); i >= 0; i--)
                {
                    status = dx.Rows[i]["status"].ObjToString();
                    if (status.ToUpper() == "IGNORE!")
                        dx.Rows.RemoveAt(i);
                }

                originalDt = dx.Copy();

                dx.Columns.Remove("dayOfMonth");
                dx.Columns.Remove("frequencyInMonths");
                dx.Columns.Remove("tmstamp");
                dx.Columns.Remove("record");
                dx.Columns.Remove("num");
                dx.Columns.Remove("dateBeginning");
                dx.Columns.Remove("status");
                dx.Columns.Remove("numPayments");
                dx.Columns.Remove("leftPayments");
                dx.Columns.Remove("legacy");
                dx.Columns.Remove("contractNumber");
                dx.Columns.Remove("payer");
                dx.Columns.Remove("code");
                dx.Columns.Remove("backupName");
                dx.Columns.Remove("location");

                dx.Columns["name"].SetOrdinal(0);
                dx.Columns["ID"].SetOrdinal(1);
                dx.Columns["routingNumber"].SetOrdinal(2);
                dx.Columns["accountNumber"].SetOrdinal(3);
                dx.Columns["acctType"].SetOrdinal(4);
                dx.Columns["payment"].SetOrdinal(5);
                dx.Columns["DebitCredit"].SetOrdinal(6);
                dx.Columns["effectiveDate"].SetOrdinal(7);
                //dx.Columns["contractNumber"].SetOrdinal(8);
                //dx.Columns["payer"].SetOrdinal(9);
                //dx.Columns["code"].SetOrdinal(10);

                dx.Columns["name"].Caption = "Last Name, First";
                dx.Columns["routingNumber"].Caption = "Routing #";
                dx.Columns["accountNumber"].Caption = "Acct #";
                dx.Columns["AcctType"].Caption = "Acct Type";
                dx.Columns["payment"].Caption = "Amount";
                dx.Columns["DebitCredit"].Caption = "Debit/Credit";
                dx.Columns["effectiveDate"].Caption = "Effective Date";

                CreateCSVfile(dx, fullPath, true, ",", true);
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** on Building ACH CSV File " + ex.Message.ToString());
                btnGenerateFile.Hide();
                chkProblems.Hide();
                this.Cursor = Cursors.Default;
                return;
            }

            DialogResult result = MessageBox.Show("***QUESTION*** Do you want to update the ACH Table by reducing the payments left?", "Update ACH Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
                UpdateACH(dt);

            int records = dx.Rows.Count;
            MessageBox.Show("***INFO*** File " + fullPath + " Created with " + records.ToString() + " Customers.", "Create ACH Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information);

            lastFileCreated = fullPath;

            btnGenerateFile.Hide();
            chkProblems.Hide();
//            btnImport.Show();
            if (workWho.ToUpper() == "BANK PLUS")
                GenerateBankPlusACH(originalDt);
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private bool CheckForSecNat(string payer, double payment)
        {
            double monthlyPremium = 0D;
            double historicPremium = 0D;
            double monthlySecNat = 0D;
            double monthly3rdParty = 0D;
            CustomerDetails.CalcMonthlyPremium(payer, ref monthlyPremium, ref historicPremium, ref monthlySecNat, ref monthly3rdParty);
            double without = monthlyPremium - monthlySecNat;
            without = G1.RoundValue(without);
            monthlyPremium = G1.RoundValue(monthlyPremium);
            bool good = true;
            if (monthlySecNat > 0D)
            {
                if (payment != without)
                    good = false;
            }
            return good;
        }
        /***********************************************************************************************/
        private bool CheckFor3rdParty(string payer, double payment)
        {
            double monthlyPremium = 0D;
            double historicPremium = 0D;
            double monthlySecNat = 0D;
            double monthly3rdParty = 0D;
            CustomerDetails.CalcMonthlyPremium(payer, ref monthlyPremium, ref historicPremium, ref monthlySecNat, ref monthly3rdParty);
            double without = monthlyPremium - monthlySecNat;
            without = G1.RoundValue(without);
            monthlyPremium = G1.RoundValue(monthlyPremium);
            bool good = true;
            if (monthly3rdParty > 0D)
            {
                if (payment != without)
                    good = false;
            }
            return good;
        }
        /***********************************************************************************************/
        private void UpdateACH(DataTable dt)
        {
            int leftPayments = 0;
            int numPayments = 0;
            int frequency = 0;
            string status = "";
            string contractNumber = "";
            string code = "";
            string payer = "";
            string record = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    record = dt.Rows[i]["record"].ObjToString();
                    status = dt.Rows[i]["status"].ObjToString();
                    if (status.ToUpper() == "IGNORE!")
                        continue;

                    leftPayments = dt.Rows[i]["leftPayments"].ObjToInt32();
                    numPayments = dt.Rows[i]["numPayments"].ObjToInt32();
                    if (numPayments >= 999)
                        continue;

                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                    payer = dt.Rows[i]["payer"].ObjToString();
                    frequency = dt.Rows[i]["frequencyInMonths"].ObjToInt32();

                    leftPayments -= frequency;
                    G1.update_db_table("ach", "record", record, new string[] { "leftPayments", leftPayments.ToString() });
                }
                catch (Exception ex)
                {
                    MessageBox.Show("***ERROR*** Updating ACH File on Contract + " + contractNumber + " Payer " + payer + " " + ex.Message.ToString());
                }
            }
        }
        /***********************************************************************************************/
        private void btnImport_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            ImportDailyDeposits importForm = new ImportDailyDeposits(lastFileCreated);
            importForm.Show();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        public static void CreateCSVfile(DataTable dtable, string strFilePath, bool includeHeader = false, string delimiter = ",", bool useColumnNames = false)
        {
            StreamWriter sw = new StreamWriter(strFilePath, false);
            int icolcount = dtable.Columns.Count;
            string str = "";
            string colName = "";
            if (includeHeader)
            {
                for (int i = 0; i < icolcount; i++)
                {
                    colName = dtable.Columns[i].ColumnName;
                    if (useColumnNames)
                        colName = dtable.Columns[i].Caption.ObjToString();
                    if (colName.IndexOf(",") >= 0)
                        colName = "\"" + colName + "\"";
                    sw.Write(colName);
                    if (i < icolcount - 1)
                        sw.Write(delimiter);
                }
                sw.Write(sw.NewLine);
            }
            DateTime date = DateTime.Now;

            string data = "";
            foreach (DataRow drow in dtable.Rows)
            {
                for (int i = 0; i < icolcount; i++)
                {
                    if (!Convert.IsDBNull(drow[i]))
                    {
                        colName = dtable.Columns[i].ColumnName.ToUpper();
                        if (drow[i].ObjToString().IndexOf(",") >= 0 )
                        {
                            str = drow[i].ObjToString();
                            str = "\"" + str + "\"";
                            sw.Write(str);
                        }
                        //else if ( colName == "ROUTINGNUMBER" || colName == "ACCOUNTNUMBER")
                        //{
                        //    str = drow[i].ObjToString();
                        //    str = "\"" + str + "\"";
                        //    sw.Write(str);
                        //}
                        else
                        {
                            data = drow[i].ToString();
                            sw.Write(data);
                        }
                    }
                    if (i < icolcount - 1)
                    {
                        sw.Write(delimiter);
                    }
                }
                sw.Write(sw.NewLine);
            }
            sw.Close();
            sw.Dispose();
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
            string text = "ACH Payments for " + this.dateTimePicker1.Value.ToString("MM/dd/yyyy");
            if (dgv2.Visible)
                text = "ACH Payments Skipped for " + this.dateTimePicker1.Value.ToString("MM/dd/yyyy");
            Printer.DrawQuad(6, 7, 4, 4, text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);

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
        private void picAdd_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            DataTable dt = (DataTable)dgv.DataSource;
            if ( dt == null)
            {
                string cmd = "Select * from `ach` where `contractNumber` = 'ABCDXXX';";
                dt = G1.get_db_data(cmd);
                dt.Columns.Add("effectiveDate");
                dt.Columns.Add("name");
                dt.Columns.Add("ID");
                dt.Columns.Add("DebitCredit");
                dt.Columns.Add("status");
                dt.Columns.Add("backupName");
            }

            DateTime effectiveDate = this.dateTimePicker1.Value;
            using (ACHExtraPayment extraForm = new ACHExtraPayment(dt, effectiveDate))
            {
                DialogResult result = extraForm.ShowDialog();
                if (result != DialogResult.OK)
                    return;
                DataTable dx = (DataTable)extraForm.ACH_Answer;
                if (dx != null)
                {
                    int row = 0;
                    for (int i = 0; i < dx.Rows.Count; i++)
                    {
                        dt.ImportRow(dx.Rows[i]);
                        row = dt.Rows.Count - 1;
                        dt.Rows[row]["backupName"] = dt.Rows[row]["name"].ObjToString();
                    }
                    G1.NumberDataTable(dt);
                    dgv.DataSource = dt;
                    dgv.Refresh();
                }
            }
        }
        /***********************************************************************************************/
        private void GenerateBankPlusACH(DataTable dx)
        {
            if (dx == null)
                return;
            string fullPath = "";

            try
            {
                string achDirectory = AdminOptions.GetOptionAnswer("BANKPLUS Path");
                if (String.IsNullOrWhiteSpace(achDirectory))
                {
                    MessageBox.Show("***Problem*** You must first SETUP a BANK PLUS Directory using the MISC Menu Option!", "Setup BANK PLUS  Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Cursor = Cursors.Default;
                    return;
                }
                DateTime date = this.dateTimePicker1.Value;
                string filename = "SMFS.BP.Draft " + date.Month.ToString("D2") + "." + date.Day.ToString("D2") + "." + date.Year.ToString("D4");
                fullPath = achDirectory + filename;
                int count = 0;
                for (; ; )
                {
                    if (!File.Exists(fullPath + ".csv"))
                        break;
                    count++;
                    filename = "SMFS.BP.Draft " + date.Month.ToString("D2") + "." + date.Day.ToString("D2") + "." + date.Year.ToString("D4") + "_" + count.ToString();
                    fullPath = achDirectory + filename;
                }

                fullPath += ".csv";

                DataTable dt = new DataTable();
                dt.Columns.Add("Customer Number");
                dt.Columns.Add("Name");
                dt.Columns.Add("Amount");
                dt.Columns.Add("Type");
                dt.Columns.Add("Routing");
                dt.Columns.Add("Reference ID");
                dt.Columns.Add("Account Number");
                dt.Columns.Add("Blank");
                dt.Columns.Add("Fname");
                dt.Columns.Add("Lname");
                dt.Columns.Add("Location");


                //dx.Columns["name"].SetOrdinal(0);
                //dx.Columns["ID"].SetOrdinal(1);
                //dx.Columns["routingNumber"].SetOrdinal(2);
                //dx.Columns["accountNumber"].SetOrdinal(3);
                //dx.Columns["acctType"].SetOrdinal(4);
                //dx.Columns["payment"].SetOrdinal(5);
                //dx.Columns["DebitCredit"].SetOrdinal(6);
                //dx.Columns["effectiveDate"].SetOrdinal(7);
                //dx.Columns["contractNumber"].SetOrdinal(8);
                //dx.Columns["payer"].SetOrdinal(9);
                //dx.Columns["code"].SetOrdinal(10);

                //dx.Columns["name"].Caption = "Last Name, First";
                //dx.Columns["routingNumber"].Caption = "Routing #";
                //dx.Columns["accountNumber"].Caption = "Acct #";
                //dx.Columns["AcctType"].Caption = "Acct Type";
                //dx.Columns["payment"].Caption = "Amount";
                //dx.Columns["DebitCredit"].Caption = "Debit/Credit";
                //dx.Columns["effectiveDate"].Caption = "Effective Date";

                string cmd = "";
                string contractNumber = "";
                string payer = "";
                string fname = "";
                string lname = "";
                string customerFile = "";
                string payment = "";
                DataTable ddx = null;
                DataRow dRow = null;

                for (int i = 0; i < dx.Rows.Count; i++)
                {
                    payer = dx.Rows[i]["payer"].ObjToString();
                    customerFile = "customers";
                    contractNumber = dx.Rows[i]["contractNumber"].ObjToString();
                    if (DailyHistory.isInsurance(contractNumber))
                        customerFile = "icustomers";
                    cmd = "Select * from `" + customerFile + "` WHERE `contractNumber` = '" + contractNumber + "';";
                    ddx = G1.get_db_data(cmd);
                    if (ddx.Rows.Count <= 0)
                    {
                        MessageBox.Show("***ERROR*** Cannot Locate Customer with ContractNumber " + contractNumber + "'");
                        continue;
                    }
                    fname = ddx.Rows[0]["firstName"].ObjToString();
                    fname = fname.Replace(",", "");
                    lname = ddx.Rows[0]["lastName"].ObjToString();
                    lname = lname.Replace(",", "");

                    //Customer Number, Name, Amount,  Type, Routing, Reference ID,    Account Number,  Blank, Fname,   Lname,

                    dRow = dt.NewRow();
                    dRow["Customer Number"] = contractNumber;
                    if (!String.IsNullOrWhiteSpace(payer))
                        dRow["Customer Number"] = payer;
                    dRow["Name"] = fname + " " + lname;
                    payment = dx.Rows[i]["payment"].ObjToString();
                    payment = payment.Replace("$", "");
                    payment = payment.Replace(",", "");
                    dRow["Amount"] = payment;
                    dRow["type"] = dx.Rows[i]["AcctType"].ObjToString();
                    dRow["Routing"] = dx.Rows[i]["routingNumber"].ObjToString();
                    dRow["Reference ID"] = dx.Rows[i]["ID"].ObjToString();
                    dRow["Account Number"] = dx.Rows[i]["accountNumber"].ObjToString();
                    dRow["Blank"] = "";
                    dRow["Fname"] = fname;
                    dRow["Lname"] = lname;
                    dRow["Location"] = dx.Rows[i]["location"].ObjToString();

                    dt.Rows.Add(dRow);
                }

                CreateCSVfile(dt, fullPath, true, ",", true);
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** on Building Bank Plus CSV File " + ex.Message.ToString());
                btnGenerateFile.Hide();
                chkProblems.Hide();
                this.Cursor = Cursors.Default;
                return;
            }

            int records = dx.Rows.Count;
            MessageBox.Show("***INFO*** Bank Plus File " + fullPath + " Created with " + records.ToString() + " Customers.", "Create Bank Plus Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information);

            lastFileCreated = fullPath;

            GenerateNewACH(dx);

            btnGenerateFile.Hide();
            chkProblems.Hide();
            //btnImport.Show();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void GenerateNewACH(DataTable dx)
        {
            if (dx == null)
                return;
            string fullPath = "";

            try
            {
                string achDirectory = AdminOptions.GetOptionAnswer("BANKPLUS Path");
                if (String.IsNullOrWhiteSpace(achDirectory))
                {
                    MessageBox.Show("***Problem*** You must first SETUP a BANK PLUS Directory using the MISC Menu Option!", "Setup BANK PLUS  Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Cursor = Cursors.Default;
                    return;
                }
                DateTime date = this.dateTimePicker1.Value;
                string filename = "SMFS.NEW.Draft " + date.Month.ToString("D2") + "." + date.Day.ToString("D2") + "." + date.Year.ToString("D4");
                fullPath = achDirectory + filename;
                int count = 0;
                for (; ; )
                {
                    if (!File.Exists(fullPath + ".csv"))
                        break;
                    count++;
                    filename = "SMFS.NEW.Draft " + date.Month.ToString("D2") + "." + date.Day.ToString("D2") + "." + date.Year.ToString("D4") + "_" + count.ToString();
                    fullPath = achDirectory + filename;
                }

                fullPath += ".csv";

                DataTable dt = new DataTable();
                dt.Columns.Add("Customer Number");
                dt.Columns.Add("Reference ID");
                dt.Columns.Add("Name");
                dt.Columns.Add("Routing");
                dt.Columns.Add("Account Number");
                dt.Columns.Add("Type");
                dt.Columns.Add("Amount");
                dt.Columns.Add("DebitCredit");
                dt.Columns.Add("effectiveDate");
                dt.Columns.Add("Location");


                dx.Columns["name"].SetOrdinal(0);
                dx.Columns["ID"].SetOrdinal(1);
                dx.Columns["routingNumber"].SetOrdinal(2);
                dx.Columns["accountNumber"].SetOrdinal(3);
                dx.Columns["acctType"].SetOrdinal(4);
                dx.Columns["payment"].SetOrdinal(5);
                dx.Columns["DebitCredit"].SetOrdinal(6);
                dx.Columns["effectiveDate"].SetOrdinal(7);
                //dx.Columns["contractNumber"].SetOrdinal(8);
                //dx.Columns["payer"].SetOrdinal(9);
                //dx.Columns["code"].SetOrdinal(10);

                dx.Columns["name"].Caption = "Last Name, First";
                dx.Columns["routingNumber"].Caption = "Routing #";
                dx.Columns["accountNumber"].Caption = "Acct #";
                dx.Columns["AcctType"].Caption = "Acct Type";
                dx.Columns["payment"].Caption = "Amount";
                dx.Columns["DebitCredit"].Caption = "Debit/Credit";
                dx.Columns["effectiveDate"].Caption = "Effective Date";

                dt.Columns["name"].Caption = "Last Name, First";
                dt.Columns["routing"].Caption = "Routing #";
                dt.Columns["Account Number"].Caption = "Acct #";
                dt.Columns["type"].Caption = "Acct Type";
                dt.Columns["amount"].Caption = "Amount";
                dt.Columns["DebitCredit"].Caption = "Debit/Credit";
                dt.Columns["effectiveDate"].Caption = "Effective Date";

                string cmd = "";
                string contractNumber = "";
                string payer = "";
                string fname = "";
                string lname = "";
                string customerFile = "";
                string payment = "";

                DataTable ddx = null;
                DataRow dRow = null;

                for (int i = 0; i < dx.Rows.Count; i++)
                {
                    payer = dx.Rows[i]["payer"].ObjToString();
                    customerFile = "customers";
                    contractNumber = dx.Rows[i]["contractNumber"].ObjToString();
                    if (DailyHistory.isInsurance(contractNumber))
                        customerFile = "icustomers";
                    cmd = "Select * from `" + customerFile + "` WHERE `contractNumber` = '" + contractNumber + "';";
                    ddx = G1.get_db_data(cmd);
                    if (ddx.Rows.Count <= 0)
                    {
                        MessageBox.Show("***ERROR*** Cannot Locate Customer with ContractNumber " + contractNumber + "'");
                        continue;
                    }
                    fname = ddx.Rows[0]["firstName"].ObjToString();
                    fname = fname.Replace(",", "");
                    lname = ddx.Rows[0]["lastName"].ObjToString();
                    lname = lname.Replace(",", "");

                    //Customer Number, Name, Amount,  Type, Routing, Reference ID,    Account Number,  Blank, Fname,   Lname,

                    dRow = dt.NewRow();
                    dRow["Customer Number"] = contractNumber;
                    if (!String.IsNullOrWhiteSpace(payer))
                        dRow["Customer Number"] = payer;
                    dRow["Name"] = lname + ", " + fname;
                    payment = dx.Rows[i]["payment"].ObjToString();
                    payment = payment.Replace("$", "");
                    payment = payment.Replace(",", "");
                    dRow["Amount"] = payment;
                    dRow["type"] = dx.Rows[i]["AcctType"].ObjToString();
                    dRow["Routing"] = dx.Rows[i]["routingNumber"].ObjToString();
                    dRow["Reference ID"] = dx.Rows[i]["ID"].ObjToString();
                    dRow["Account Number"] = dx.Rows[i]["accountNumber"].ObjToString();
                    dRow["DebitCredit"] = dx.Rows[i]["DebitCredit"].ObjToString();
                    dRow["effectiveDate"] = dx.Rows[i]["effectiveDate"].ObjToString();
                    dRow["Location"] = dx.Rows[i]["location"].ObjToString();

                    dt.Rows.Add(dRow);
                }

                if ( 1 == 1)
                {

                }
                CreateCSVfile(dt, fullPath, true, ",", true);
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** on Building Bank Plus CSV File " + ex.Message.ToString());
                btnGenerateFile.Hide();
                chkProblems.Hide();
                this.Cursor = Cursors.Default;
                return;
            }

            int records = dx.Rows.Count;
            MessageBox.Show("***INFO*** Bank Plus File " + fullPath + " Created with " + records.ToString() + " Customers.", "Create Bank Plus Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information);

            lastFileCreated = fullPath;

            btnGenerateFile.Hide();
            chkProblems.Hide();
            //btnImport.Show();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
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
        /***********************************************************************************************/
        private void chkProblems_CheckedChanged(object sender, EventArgs e)
        {
            gridMain.RefreshData();
            dgv.Refresh();
        }
        /***********************************************************************************************/
        private void gridMain_CustomRowFilter(object sender, RowFilterEventArgs e)
        {
            if (!chkProblems.Checked)
                return;
            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv.DataSource;
            string status = dt.Rows[row]["status"].ObjToString().Trim().ToUpper();

            if (String.IsNullOrWhiteSpace(status))
            {
                e.Visible = false;
                e.Handled = true;
            }
        }
        /***********************************************************************************************/
        private void ignorePaymentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataTable dt2 = (DataTable)dgv2.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();
            dr["ID"] = "";
            try
            {
                dt2.ImportRow(dr);
                dt.Rows.Remove(dr);
            }
            catch ( Exception ex)
            {
            }

            DataView tempview = dt2.DefaultView;
            tempview.Sort = "name asc";
            dt2 = tempview.ToTable();

            G1.NumberDataTable(dt2);
            dgv2.DataSource = dt2;
            dgv2.Refresh();

            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            dgv.Refresh();
        }
        /***********************************************************************************************/
        private void acceptPaymentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataTable dt2 = (DataTable)dgv2.DataSource;
            DataRow dr = gridMain2.GetFocusedDataRow();
            string contractNumber = dr["contractNumber"].ObjToString();
            string cmd = "Select * from `customers` where `contractNumber` = '" + contractNumber + "';";
            if (DailyHistory.isInsurance(contractNumber))
                cmd = "Select * from `icustomers` where `contractNumber` = '" + contractNumber + "';";
            DataTable ddx = G1.get_db_data(cmd);
            if (ddx.Rows.Count <= 0)
                return;
            string fname = ddx.Rows[0]["firstName"].ObjToString();
            fname = fname.Replace(",", "");
            string lname = ddx.Rows[0]["lastName"].ObjToString();
            lname = lname.Replace(",", "");
            dr["name"] = lname + ", " + fname;
            dr["ID"] = GenerateRandomId(10);
            dr["status"] = "";

            dt.ImportRow(dr);
            dt2.Rows.Remove(dr);
            
            DataView tempview = dt.DefaultView;
            tempview.Sort = "name asc";
            dt = tempview.ToTable();

            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            dgv.Refresh();
        }
        /***********************************************************************************************/
        private double CalculateTotalPayments ( DataTable dt )
        {
            double totalPayments = 0D;
            string str = "";
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                str = dt.Rows[i]["payment"].ObjToString();
                str = str.Replace("$", "");
                str = str.Replace(",", "");
                totalPayments += str.ObjToDouble();
            }
            return totalPayments;
        }
        /***********************************************************************************************/
        private void gridMain_CustomSummaryCalculate(object sender, DevExpress.Data.CustomSummaryEventArgs e)
        {
            string field = (e.Item as GridSummaryItem).FieldName.ObjToString();

            if (field.ToUpper() == "PAYMENT")
            {
                DataTable dt = (DataTable)dgv.DataSource;
                e.TotalValueReady = true;
                e.TotalValue = CalculateTotalPayments(dt);
            }
        }
        /***********************************************************************************************/
        private void gridMain2_CustomSummaryCalculate(object sender, DevExpress.Data.CustomSummaryEventArgs e)
        {
            string field = (e.Item as GridSummaryItem).FieldName.ObjToString();

            if (field.ToUpper() == "PAYMENT")
            {
                DataTable dt = (DataTable)dgv2.DataSource;
                e.TotalValueReady = true;
                e.TotalValue = CalculateTotalPayments(dt);
            }
        }
        /***********************************************************************************************/
        private void gridMain_CustomDrawCell(object sender, RowCellCustomDrawEventArgs e)
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
        /***********************************************************************************************/
        private void btnSave_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            string status = "";
            string record = "";
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                status = dt.Rows[i]["status"].ObjToString();
                if ( status.ToUpper() == "NONE" || status.ToUpper() == "PAUSE")
                {
                    record = dt.Rows[i]["record"].ObjToString();
                    if (status.ToUpper() == "NONE")
                        status = "";
                    G1.update_db_table("ach", "record", record, new string[] { "status", status});
                }
            }
            btnSave.Hide();
        }
        /***********************************************************************************************/
        private void repositoryItemComboBox3_SelectedValueChanged(object sender, EventArgs e)
        {
            btnSave.Show();
            ComboBoxEdit combo = (ComboBoxEdit)sender;
            string what = combo.Text.Trim().ToUpper();
        }
        /***********************************************************************************************/
    }
}