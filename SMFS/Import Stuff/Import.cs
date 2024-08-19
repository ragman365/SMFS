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
using System.Text.RegularExpressions;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class Import : Form
    {
        private string workWhat = "";
        private string actualFile = "";
        private bool doPayerDeceased = false;
        private bool workVerify = true;
        private bool recoveredFDLIC = false;
        public static bool ImportHonestyMode = false;
        private bool workByPass = false;
        /***********************************************************************************************/
        public Import(string what = "", bool verify = true, bool honestyMode = false, bool byPass = false )
        {
            InitializeComponent();
            workWhat = what;
            workVerify = verify;
            ImportHonestyMode = honestyMode;
            workByPass = byPass;
        }
        /***********************************************************************************************/
        private void Import_Load(object sender, EventArgs e)
        {
            btnFixAll.Hide();
            //if (LoginForm.username.ToUpper() == "ROBBY")
            //    btnFixAll.Show();
            if (workWhat.ToUpper() != "NEWCONTRACTS")
            {
                for (int i = menuStrip1.Items.Count - 1; i >= 0; i--)
                {
                    string text = menuStrip1.Items[i].Text;
                    if (menuStrip1.Items[i].Text == "Misc")
                        menuStrip1.Items.RemoveAt(i);
                }
            }

            txtStartRow.Hide();
            label1.Hide();
            tabControl1.TabPages.Remove(tabPage2);

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
            if (workVerify)
            {
                DialogResult result = MessageBox.Show("***Question*** Are you sure you want to Import this Data ?", "Import Data Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.No)
                    return;
            }
            DataTable dt = (DataTable)dgv.DataSource;
            if (SelectDone != null)
            {
                dt.TableName = actualFile;
                OnSelectDone(dt);
                this.Close();
                return;
            }
            if (workWhat.ToUpper() == "PAYMENTS")
            {
                ImportPayHistoryData(dt);
                return;
            }
            else if (workWhat.ToUpper() == "DECEASED")
            {
                ImportPreaccDead(dt, workWhat);
                //                ImportDeathData(dt, workWhat);
                return;
            }
            else if (workWhat.ToUpper() == "PREACC")
            {
                ImportContractData(dt, workWhat);
                return;
            }
            else if (workWhat.ToUpper() == "PREACCLAP")
            {
                //                ImportContractData(dt, workWhat);
                ImportPreaccLapse(dt, workWhat);
                return;
            }
            else if (workWhat.ToUpper() == "PREMST")
            {
                ImportCustomerData(dt, workWhat);
                return;
            }
            else if (workWhat.ToUpper() == "PREMSTLAP")
            {
                ImportCustomerData(dt, workWhat);
                return;
            }
            else if (workWhat.ToUpper() == "AGENTINCOMING")
            {
                ImportAgentIncoming(dt, workWhat);
                return;
            }
            else if (workWhat.ToUpper() == "NEWCONTRACTS")
            {
                if (!ValidateFDLIC(dt, true))
                    return;
                if (ValidBankAccounts(dt))
                    ImportNewContracts(dt, workWhat);
                return;
            }
            else if (workWhat.ToUpper() == "BATESVILLE")
            {
                ImportBatesville(dt, workWhat);
                return;
            }
            else if (workWhat.ToUpper() == "INSURANCE PAYER TEST")
            {
                ImportPayerTest(dt, workWhat, actualFile);
                return;
            }
            else if (workWhat.ToUpper() == "INSURANCE PAYER")
            {
                ImportPayerData(dt, workWhat);
                return;
            }
            else if (workWhat.ToUpper() == "INSURANCE PAYER FIX")
            {
                ImportPayerData(dt, workWhat);
                return;
            }
            else if (workWhat.ToUpper() == "INSURANCE PAYER FIX2")
            {
                ImportPayerData(dt, workWhat);
                return;
            }
            else if (workWhat.ToUpper() == "INSURANCE PAYER LAPSED DATA")
            {
                ImportPayerData(dt, workWhat);
                return;
            }
            else if (workWhat.ToUpper() == "INSURANCE PAYER DECEASED DATA")
            {
                ImportPayerData(dt, workWhat);
                return;
            }
            else if (workWhat.ToUpper() == "INSURANCE POLICIES")
            {
                ImportPolicyData(dt, workWhat);
                return;
            }
            else if (workWhat.ToUpper() == "INSURANCE POLICIES LAPSED")
            {
                ImportPolicyData(dt, workWhat);
                return;
            }
            else if (workWhat.ToUpper() == "INSURANCE POLICIES DECEASED")
            {
                //                ImportPolicyDeceased(dt, workWhat);
                ImportPolicyData(dt, workWhat);
                return;
            }
            else if (workWhat.ToUpper() == "INSURANCE PAYMENTS")
            {
                ImportPaymentData(dt, workWhat);
                return;
            }
            else if (workWhat.ToUpper() == "FDLIC PHONE NUMBERS")
            {
                ImportFDLIC_PhoneNumbers(dt, workWhat);
                return;
            }
        }
        /***********************************************************************************************/
        private void CheckSSNs ( DataTable dt )
        {
            picLoader.Show();
            picLoader.Refresh();

            string cmd = "";
            DataTable dx = null;
            string ssn = "";
            string contractNumber = "";
            string prefix = "";
            string suffix = "";
            string mi = "";
            string fName = "";
            string lName = "";
            string customer = "";
            string agentCode = "";
            string agentNumber = "";
            string trustNumber = "";
            string name = "";

            DataTable tempDt = dt.Clone();
            tempDt.Columns.Add("customer");
            tempDt.Columns.Add("oldCustomer");
            tempDt.Columns.Add("contractNumber");

            int row = 0;
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                try
                {
                    ssn = dt.Rows[i]["INSURED_SSN"].ObjToString();
                    if (String.IsNullOrWhiteSpace(ssn))
                        continue;
                    trustNumber = dt.Rows[i]["TRUST_NUMBER"].ObjToString();
                    agentNumber = dt.Rows[i]["AGENT_NUMBER"].ObjToString();

                    cmd = "Select * from `customers` WHERE `SSN` = '" + ssn + "';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                    {
                        row = tempDt.Rows.Count;
                        tempDt.ImportRow(dt.Rows[i]);

                        tempDt.Rows[row]["contractNumber"] = dx.Rows[0]["contractNumber"].ObjToString();
                        prefix = dx.Rows[0]["prefix"].ObjToString();
                        suffix = dx.Rows[0]["suffix"].ObjToString();
                        fName = dx.Rows[0]["firstName"].ObjToString();
                        lName = dx.Rows[0]["lastName"].ObjToString();
                        mi = dx.Rows[0]["middleName"].ObjToString();
                        customer = G1.BuildFullName(prefix, fName, mi, lName, suffix);
                        tempDt.Rows[row]["oldCustomer"] = customer;

                        agentCode = LookupAgentCode(agentNumber, trustNumber);
                        //dt.Rows[i]["agentCode"] = agentCode;
                        name = CustomerDetails.GetAgentName(agentCode);
                        if (!String.IsNullOrWhiteSpace(name))
                            tempDt.Rows[row]["AGENT_NUMBER"] = name;
                    }
                }
                catch ( Exception ex )
                {
                }
            }

            string preference = G1.getPreference(LoginForm.username, "Import FDLIC", "Force Duplicate Print");

            if ( tempDt.Rows.Count > 0 )
            {
                for ( int i=0; i<tempDt.Rows.Count; i++)
                {
                    try
                    {
                        prefix = tempDt.Rows[i]["INSURED_PREFIX"].ObjToString();
                        suffix = tempDt.Rows[i]["INSURED_SUFFIX"].ObjToString();
                        fName = tempDt.Rows[i]["INSURED_FIRST_NAME"].ObjToString();
                        lName = tempDt.Rows[i]["INSURED_LAST_NAME"].ObjToString();
                        mi = tempDt.Rows[i]["INSURED_MIDDLE_INITIAL"].ObjToString();
                        customer = G1.BuildFullName(prefix, fName, mi, lName, suffix);
                        tempDt.Rows[i]["customer"] = customer;
                    }
                    catch ( Exception ex )
                    {
                    }
                }
                if (G1.get_column_number(tempDt, "num") < 0)
                    tempDt.Columns.Add("num", typeof(string)).SetOrdinal(0);

                G1.NumberDataTable(tempDt);
                dgv2.DataSource = tempDt;
                tabControl1.TabPages.Add (tabPage2);
                tabControl1.SelectTab(tabPage2);
                tabPage2.Show();
                tabPage2.Refresh();

                btnImportFile.Hide();
                btnImportFile.Refresh();
            }
            else
            {
                DataRow dRow = tempDt.NewRow();
                dRow["customer"] = "No Duplicates";
                dRow["oldCustomer"] = "No Duplicates";
                tempDt.Rows.Add(dRow);

                if (G1.get_column_number(tempDt, "num") < 0)
                    tempDt.Columns.Add("num", typeof(string)).SetOrdinal(0);

                G1.NumberDataTable(tempDt);
                dgv2.DataSource = tempDt;
            }

            if (preference == "YES")
            {
                MessageBox.Show("*** INFO *** Forcing Duplicate Print!", "Force Duplicate Print Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);

                forcePrint = true;
                printToolStripMenuItem_Click(null, null);
                forcePrint = false;
            }

            picLoader.Hide();
            picLoader.Refresh();
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
                    DataTable dt = null;
                    try
                    {
                        dt = ImportCSVfile(file);
                    }
                    catch (Exception ex)
                    {
                    }
                    this.Cursor = Cursors.Default;
                    if ( workByPass )
                    {
                        dt.TableName = actualFile;
                        OnSelectDone(dt);
                        this.Close();
                        return;
                    }
                    recoveredFDLIC = false;
                    this.Cursor = Cursors.WaitCursor;
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        if (G1.get_column_number(dt, "BADDETAIL") > 0)
                            recoveredFDLIC = true;
                        else
                        {
                            dt.Columns.Add("NEW");
                            dt.Columns.Add("BAD");
                            dt.Columns.Add("BADDETAIL");
                        }
                        dt.AcceptChanges();

                        if (workWhat.ToUpper() == "NEWCONTRACTS")
                        {
                            if (!recoveredFDLIC)
                            {
                                bool good = AddAgentCode(dt);
                                if (!good)
                                    return;
                                dt.Columns.Add("deposit #");
                                dt.Columns.Add("bankAccount");
                                dt.Columns.Add("ccFee", Type.GetType("System.Double"));
                                dt.Columns.Add("trustLocation");
                                dt.Columns.Add("trustDpRecord");
                            }
                            else
                            {
                                CleanupBankAccounts(dt);
                            }
                        }
                        //if (workWhat.ToUpper() == "PAYMENTS")
                        //    CorrectPayDates(dt);
                        if (doPayerDeceased)
                            ProcessPayerDeceased(dt);
                        if (workWhat.ToUpper() == "ALL_LAPSES")
                            LookupAllLapses(dt);
                        if (workWhat.ToUpper() == "ALL_REINSTATES")
                            LookupAllReinstates(dt);
                        else if (workWhat == "FDLIC Phone Numbers")
                        {
                            btnFixAll.Show();
                            FixFDLIC_PhoneNumbers(dt);
                        }
                        if (workWhat.ToUpper() == "NEWCONTRACTS" && !recoveredFDLIC)
                            FixFDLIC_PhoneNumbers(dt);
                        G1.NumberDataTable(dt);
                        dgv.DataSource = dt;
                        if (workWhat.ToUpper() == "NEWCONTRACTS")
                        {
                            SetNewContractColumnWidths();
                            ValidateFDLIC(dt);
                            CheckSSNs(dt);
                        }
                        this.Cursor = Cursors.Default;
                        if (workWhat == "Insurance Payer")
                        {
                            //PreProcessPayerActive(dt);
                            //dgv.DataSource = dt;
                            //dgv.RefreshDataSource();
                            //dgv.Refresh();
                        }
                        else if (workWhat == "Insurance Payer Lapsed Data")
                        {
                            //PreProcessPayerActive(dt);
                            dgv.DataSource = dt;
                            dgv.RefreshDataSource();
                            dgv.Refresh();
                        }
                        else if (workWhat == "Insurance Payer Deceased Data")
                        {
                            //PreProcessPayerActive(dt);
                            dgv.DataSource = dt;
                            dgv.RefreshDataSource();
                            dgv.Refresh();
                        }
                        else if (workWhat.ToUpper() == "INSURANCE POLICIES")
                        {
                            //PreprocessPolicyData(dt);
                            dgv.DataSource = dt;
                            dgv.RefreshDataSource();
                            dgv.Refresh();
                        }
                        if ( !dgv2.Visible )
                            btnImportFile.Show();
                    }
                }
            }
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private string FixSpecial(string data)
        {
            string apostraphe = "ÃƒÆ’Ã";
            //if (1 == 1)
            //    return data;
            int idx = data.IndexOf(apostraphe);
            if (idx > 0)
                data = G1.Truncate(data, idx);

            data = G1.try_protect_data(data);
            data = G1.Truncate(data, 50);

            return data;
        }
        /***********************************************************************************************/
        private void CleanupBankAccounts(DataTable dt)
        {
            string bankAccount = "";
            string badApos = "� ";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                bankAccount = dt.Rows[i]["bankAccount"].ObjToString();
                if (bankAccount.IndexOf(badApos) >= 0)
                {
                    bankAccount = bankAccount.Replace(badApos, "'");
                    dt.Rows[i]["bankAccount"] = bankAccount;
                }
                //                if ( bankAccount.ToUpper().IndexOf ( "TRUSTMARK") >= 0 )
                //                {
                //                    char[] ch = bankAccount.ToCharArray();
                ////                    ch[3] = 'X'; // index starts at 0!
                //                    for ( int j=0; j<bankAccount.Length; j++)
                //                    {
                //                        char c = (char)bankAccount[j];
                //                        int x = (int)bankAccount[j];
                //                        if (x == 65533)
                //                            ch[j] = '\'';
                //                    }
                //                    bankAccount = new string(ch);
                //                    dt.Rows[i]["bankAccount"] = bankAccount;
                //                }
            }
        }
        /***********************************************************************************************/
        private void CorrectPayDates(DataTable dt)
        {
            string dateStr = "";
            string payDate8 = "";
            string paydt = "";
            string dueDate8 = "";
            int corrected = 0;
            string contract = "";
            DateTime date = DateTime.Now;
            DateTime startDate = new DateTime(2018, 7, 1);
            DateTime stopDate = new DateTime(2019, 7, 31);
            this.Cursor = Cursors.WaitCursor;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                contract = dt.Rows[i]["contract"].ObjToString();
                if (contract == "L18100LI")
                {
                }
                payDate8 = dt.Rows[i]["Pay Date 8"].ObjToString();
                paydt = dt.Rows[i]["PAYDTE"].ObjToString();
                dueDate8 = dt.Rows[i]["Due Date 8"].ObjToString();
                if (payDate8 == "0" && paydt == "0")
                {
                    //payDate8 = dueDate8;
                    corrected++;
                    dt.Rows[i]["FILL16"] = corrected.ToString("D2");
                }
                else if (payDate8 == "0")
                {
                    //payDate8 = paydt;
                    corrected++;
                    dt.Rows[i]["FILL16"] = corrected.ToString("D2");
                }
                date = payDate8.ObjToDateTime();
                if (date.Year > 100)
                {
                    if (date < startDate || date > stopDate)
                    {
                        dt.Rows[i]["Pay Date 8"] = "0";
                        dt.Rows[i]["PAYDTE"] = date.Year.ToString("D4") + date.Month.ToString("D2") + date.Day.ToString("D2");
                    }
                    else
                        dt.Rows[i]["Pay Date 8"] = date.Year.ToString("D4") + date.Month.ToString("D2") + date.Day.ToString("D2");
                }
                else
                {
                    corrected++;
                    dt.Rows[i]["FILL16"] = corrected.ToString("D2");
                }
            }
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void LookupAllLapses(DataTable dt)
        {
            string contractNumber = "";
            DateTime issueDate = DateTime.Now;
            DateTime lapseDate = DateTime.Now;
            DateTime myIssueDate = DateTime.Now;
            DateTime myLapseDate = DateTime.Now;
            int badIssueDates = 0;
            int badLapseDates = 0;
            int badLapses = 0;

            string lapsed = "";
            string cmd = "";
            dt.Columns.Add("MyIssueDate");
            dt.Columns.Add("MyLapseDate");
            dt.Columns.Add("Lapsed");
            DataTable dx = null;
            DateTime date = DateTime.Now;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                contractNumber = dt.Rows[i]["CONTRACT #"].ObjToString();
                cmd = "Select * from `contracts` where `contractNumber` = '" + contractNumber + "';";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    issueDate = dt.Rows[i]["Issue Date"].ObjToDateTime();
                    date = dx.Rows[0]["issueDate8"].ObjToDateTime();
                    if (issueDate != date)
                        badIssueDates++;
                    if (date.Year > 1300)
                        dt.Rows[i]["MyIssueDate"] = date.ToString("MM/dd/yyyy");

                    lapseDate = dt.Rows[i]["Lapse Date"].ObjToDateTime();
                    date = dx.Rows[0]["lapseDate8"].ObjToDateTime();
                    if (lapseDate != date)
                        badLapseDates++;
                    if (date.Year > 1300)
                        dt.Rows[i]["MyLapseDate"] = date.ToString("MM/dd/yyyy");

                    lapsed = dx.Rows[0]["lapsed"].ObjToString();
                    if (String.IsNullOrWhiteSpace(lapsed))
                        badLapses++;
                    dt.Rows[i]["Lapsed"] = lapsed;
                }
            }
        }
        /***********************************************************************************************/
        private void FixFDLIC_PhoneNumbers(DataTable dt)
        {
            string contractNumber = "";
            int badContracts = 0;
            string phoneNumber = "";
            string areaCode = "";
            dt.Columns.Add("MyPhoneNumber");
            dt.Columns.Add("MyAreaCode");
            dt.Columns.Add("Found");
            if (G1.get_column_number(dt, "INSURED_PHONE") < 0)
                return;
            if (G1.get_column_number(dt, "PAYOR_PHONE") < 0)
                return;
            string cmd = "";
            DataTable dx = null;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                contractNumber = dt.Rows[i]["Trust_Number"].ObjToString();
                cmd = "Select * from `contracts` where `contractNumber` = '" + contractNumber + "';";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count <= 0)
                {
                    dt.Rows[i]["Found"] = "NEW";
                    badContracts++;
                }
                phoneNumber = dt.Rows[i]["INSURED_PHONE"].ObjToString();
                if (String.IsNullOrWhiteSpace(phoneNumber))
                    phoneNumber = dt.Rows[i]["PAYOR_PHONE"].ObjToString();
                areaCode = "";
                if (phoneNumber.IndexOf("0000") >= 0)
                    continue;
                if (phoneNumber.Length == 10)
                {
                    try
                    {
                        areaCode = phoneNumber.Substring(0, 3);
                        phoneNumber = phoneNumber.Replace(areaCode, "");
                        phoneNumber = phoneNumber.Substring(0, 3) + "-" + phoneNumber.Substring(3);
                        phoneNumber = "(" + areaCode + ") " + phoneNumber;
                    }
                    catch (Exception ex)
                    {

                    }
                    dt.Rows[i]["MyPhoneNumber"] = phoneNumber;
                    dt.Rows[i]["MyAreaCode"] = areaCode;
                }
            }
        }
        /***********************************************************************************************/
        private void LookupAllReinstates(DataTable dt)
        {
            string contractNumber = "";
            DateTime issueDate = DateTime.Now;
            DateTime reinstateDate = DateTime.Now;
            DateTime myIssueDate = DateTime.Now;
            DateTime myReinstateDate = DateTime.Now;
            int badIssueDates = 0;
            int badReinstateDates = 0;
            int badLapses = 0;

            string lapsed = "";
            string cmd = "";
            dt.Columns.Add("MyIssueDate");
            dt.Columns.Add("MyReinstateDate");
            DataTable dx = null;
            DateTime date = DateTime.Now;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                contractNumber = dt.Rows[i]["CONTRACT #"].ObjToString();
                cmd = "Select * from `contracts` where `contractNumber` = '" + contractNumber + "';";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    issueDate = dt.Rows[i]["Issue Date"].ObjToDateTime();
                    date = dx.Rows[0]["issueDate8"].ObjToDateTime();
                    if (issueDate != date)
                        badIssueDates++;
                    if (date.Year > 1300)
                        dt.Rows[i]["MyIssueDate"] = date.ToString("MM/dd/yyyy");

                    reinstateDate = dt.Rows[i]["Reinstate Date"].ObjToDateTime();
                    date = dx.Rows[0]["reinstateDate8"].ObjToDateTime();
                    if (reinstateDate != date)
                        badReinstateDates++;
                    if (date.Year > 1300)
                        dt.Rows[i]["MyReinstateDate"] = date.ToString("MM/dd/yyyy");
                }
            }
        }
        /***********************************************************************************************/
        private void ProcessPayerDeceased(DataTable dt)
        {
            string str = "";
            dt.Columns.Add("deceasedDate");
            string address1 = "";
            string city = "";
            string month = "";
            string year = "";
            int imonth = 0;
            int iyear = 0;
            int iday = 0;
            string[] dates = null;
            DateTime deceasedDate = DateTime.Now;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                address1 = dt.Rows[i]["address 1"].ObjToString();
                city = dt.Rows[i]["city"].ObjToString().Trim();
                try
                {
                    if (address1.IndexOf("DEATH") >= 0)
                    {
                        address1 = address1.Replace("DEATH", "").Trim();
                        if (address1.Length == 4)
                        {
                            month = address1.Substring(0, 2);
                            year = address1.Substring(2, 2);
                            iday = 1;
                            iyear = year.ObjToInt32();
                            if (iyear >= 35 && iyear <= 99)
                                iyear += 1900;
                            else
                                iyear += 2000;
                            imonth = month.ObjToInt32();
                            if (imonth <= 0 || imonth > 12)
                                imonth = 1;
                            str = imonth.ToString("D2") + "/" + iday.ToString("D2") + "/" + iyear.ToString("D4");
                            dt.Rows[i]["deceasedDate"] = str;
                        }
                    }
                    else if (city.Length == 6)
                    {
                        if (G1.validate_numeric(city))
                        {
                            month = city.Substring(0, 2);
                            str = city.Substring(2, 2);
                            year = city.Substring(4);
                            iday = str.ObjToInt32();
                            iyear = year.ObjToInt32();
                            if (iyear >= 35 && iyear <= 99)
                                iyear += 1900;
                            else
                                iyear += 2000;
                            imonth = month.ObjToInt32();
                            if (imonth <= 0 || imonth > 12)
                                imonth = 1;
                            str = imonth.ToString("D2") + "/" + iday.ToString("D2") + "/" + iyear.ToString("D4");
                            dt.Rows[i]["deceasedDate"] = str;
                        }
                        else
                        {
                            year = city.Substring(0, 2);
                            if (G1.validate_numeric(year))
                            {
                                if (iyear >= 35 && iyear <= 99)
                                    iyear += 1900;
                                else
                                    iyear += 2000;
                                iyear = year.ObjToInt32();
                                iday = 1;
                                city = city.Substring(2).Trim().ToUpper();
                                imonth = 1;
                                if (city == "JAN")
                                    imonth = 1;
                                else if (city == "FEB")
                                    imonth = 2;
                                else if (city == "MAR")
                                    imonth = 3;
                                else if (city == "APR")
                                    imonth = 4;
                                else if (city == "MAY")
                                    imonth = 5;
                                else if (city == "JUN")
                                    imonth = 6;
                                else if (city == "JUL")
                                    imonth = 7;
                                else if (city == "AUG")
                                    imonth = 8;
                                else if (city == "SEP")
                                    imonth = 9;
                                else if (city == "OCT")
                                    imonth = 10;
                                else if (city == "NOV")
                                    imonth = 11;
                                else if (city == "DEC")
                                    imonth = 12;
                                str = imonth.ToString("D2") + "/" + iday.ToString("D2") + "/" + iyear.ToString("D4");
                                dt.Rows[i]["deceasedDate"] = str;
                            }
                        }
                        //110113
                    }
                    else if (city.Length == 8)
                    {
                        if (G1.validate_numeric(city))
                        {
                            month = city.Substring(0, 2);
                            imonth = month.ObjToInt32();
                            if (imonth <= 0 || imonth > 12)
                                imonth = 1;
                            city = city.Substring(2);
                            str = city.Substring(0, 2);
                            iday = str.ObjToInt32();
                            if (iday <= 0)
                                iday = 1;
                            else if (iday > 31)
                                iday = 28;
                            city = city.Substring(2);
                            iyear = city.ObjToInt32();
                            str = imonth.ToString("D2") + "/" + iday.ToString("D2") + "/" + iyear.ToString("D4");
                            dt.Rows[i]["deceasedDate"] = str;
                        }
                        else if (city.IndexOf("/") > 0)
                        {
                            deceasedDate = city.ObjToDateTime();
                            if (deceasedDate.Year <= 1850)
                            {
                                dates = city.Split('/');
                                if (dates.Length >= 3)
                                {
                                    iyear = dates[0].ObjToInt32();
                                    if (iyear > 21)
                                        iyear = 1900 + iyear;
                                    else
                                        iyear = 2000 + iyear;
                                    imonth = dates[1].ObjToInt32();
                                    iday = dates[2].ObjToInt32();
                                    city = imonth.ToString("D2") + "/" + iday.ToString("D2") + "/" + iyear.ToString("D4");
                                }
                            }
                            else
                                city = deceasedDate.ToString("MM/dd/yyyy");
                            dt.Rows[i]["deceasedDate"] = city;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("***ERROR*** " + ex.Message.ToString());
                }
                deceasedDate = dt.Rows[i]["deceasedDate"].ObjToDateTime();
                if (deceasedDate.Year < 1850)
                    dt.Rows[i]["deceasedDate"] = "12/31/1910";
            }
        }
        /***********************************************************************************************/
        private bool AddAgentCode(DataTable dt)
        {
            if (G1.get_column_number(dt, "OldAgent") < 0)
                dt.Columns.Add("OldAgent");
            if (G1.get_column_number(dt, "agentCode") < 0)
                dt.Columns.Add("agentCode");
            if (G1.get_column_number(dt, "agentName") < 0)
                dt.Columns.Add("agentName");
            if (G1.get_column_number(dt, "dateDPPaid") < 0)
                dt.Columns.Add("dateDPPaid");
            string agentNumber = "";
            string agentCode = "";
            string trustNumber = "";
            string name = "";
            string cmd = "";
            bool good = true;
            DataTable dx = new DataTable();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    agentNumber = dt.Rows[i]["AGENT_NUMBER"].ObjToString();
                    trustNumber = dt.Rows[i]["TRUST_NUMBER"].ObjToString();
                    agentCode = LookupAgentCode(agentNumber, trustNumber);
                    dt.Rows[i]["agentCode"] = agentCode;
                    name = CustomerDetails.GetAgentName(agentCode);
                    if (!String.IsNullOrWhiteSpace(name))
                        dt.Rows[i]["agentName"] = name;
                    cmd = "Select `agentCode` from `customers` where `contractNumber` = '" + trustNumber + "';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                    {
                        dt.Rows[i]["OldAgent"] = dx.Rows[0]["agentCode"].ObjToString();
                    }
                }
                catch ( Exception ex )
                {
                    MessageBox.Show("***ERROR*** " + ex.Message.ToString());
                    good = false;
                    break;
                }
            }
            return good;
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
        public static DataTable ImportCSVfile(string filename, int minColumns )
        {
            DataTable dt = ImportCSVfile(filename, null, false, ",", minColumns);
            return dt;
        }
        /***********************************************************************************************/
        public static DataTable ImportCSVfile(string filename, PictureBox picLoader = null, bool skipNum = false, string delimiter = ",", int minColumns = -1 )
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
            char cDelimiter = (char)delimiter[0];
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
                            if (line.IndexOf("~") >= 0)
                                delimiter = "~";
                            else if (line.IndexOf("\t") >= 0)
                                delimiter = "\t";
                            first = false;
                            dt = BuildImportDt(line, skipNum, delimiter, minColumns, ImportHonestyMode );
                            maxColumns = (dt.Columns.Count - 1);
                            continue;
                        }
                        string[] Lines = line.Split(cDelimiter);
                        G1.parse_answer_data(line, delimiter);
                        int count = G1.of_ans_count;
                        //if (G1.of_ans_count <= maxColumns)
                        //{
                        if (honorInsuranceDebug)
                        {
                            payer = Lines[0].ObjToString();
                            if (payer.Trim() != "CC-843")
                                continue;
                        }
                        DataRow dRow = dt.NewRow();
                        int inc = 1;
                        if (skipNum)
                            inc = 0;
                        for (int i = 0; i < G1.of_ans_count; i++)
                        {
                            str = G1.of_answer[i].ObjToString().Trim();
                            str = str.Replace("\\,", ",");
                            str = trim(str);
                            try
                            {
                                if (i + inc < dt.Columns.Count)
                                    dRow[i + inc] = str;
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                        dt.Rows.Add(dRow);
                    }
                    row++;
                    //                        picLoader.Refresh();
                    //}
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
        public static DataTable ImportCCfile(string filename, PictureBox picLoader = null)
        {
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
        public static DataTable BuildImportDt(string line, bool skipNum = false, string delimiter = ",", int minColumns = -1, bool honestyMode = false )
        {
            DataTable dt = new DataTable();
            if (!skipNum)
                dt.Columns.Add("Num");
            string name = "";
            G1.parse_answer_data(line, delimiter);
            for (int i = 0; i < G1.of_ans_count; i++)
            {
                name = G1.of_answer[i].ObjToString();
                name = trim(name);
                if (String.IsNullOrEmpty(name))
                    name = "COL " + i.ToString();
                name = name.Trim();
                int col = G1.get_column_number(dt, name);
                if (col < 0 )
                    dt.Columns.Add(name);
                else if ( honestyMode )
                {
                    name += i.ToString();
                    col = G1.get_column_number(dt, name);
                    if (col < 0)
                        dt.Columns.Add(name);
                }
            }
            if ( minColumns > 0 && dt.Columns.Count < minColumns )
            {
                for ( int i=dt.Columns.Count+1; i<minColumns; i++)
                {
                    name = "COL " + i.ToString();
                    int col = G1.get_column_number(dt, name);
                    if (col < 0)
                        dt.Columns.Add(name);
                }
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
        private void ImportContractData(DataTable dt, string workwhat)
        {
            picLoader.Show();
            bool doingDeath = false;
            if (G1.get_column_number(dt, "Death Date") >= 0)
                doingDeath = true;
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
            G1.CreateAudit("preacc");
            DateTime beginDate = new DateTime(1998, 1, 1);
            DateTime endDate = new DateTime(2018, 10, 1);
            beginDate = new DateTime(2016, 1, 1);
            endDate = new DateTime(2019, 11, 1);
            DateTime iDate = DateTime.Now;
            DateTime dDate = DateTime.Now;
            bool fixDueDate = false;
            bool fixBalances = false;
            bool fixLapses = false;
            bool force = false;
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
                    if (i % 1000 == 0)
                        GC.Collect();
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
                        if (fixDueDate)
                        {
                            FixDueDates(contract, dt, i);
                            continue;
                        }
                        else if (fixBalances)
                        {
                            FixBalances(contract, dt, i);
                            continue;
                        }
                        else if (fixLapses)
                        {
                            FixLapses(contract, dt, i);
                            continue;
                        }
                        //if (1 == 1)
                        //    continue;
                        cmd = "Select * from `contracts` where `contractNumber` = '" + contract + "';";
                        dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count > 0)
                        {
                            record = dx.Rows[0]["record"].ObjToString();

                            dueDate8 = GetSQLDate(dt, i, "ddue8");
                            apr = dt.Rows[i]["apr"].ObjToString();
                            ageAtIssue = dt.Rows[i]["ageiss"].ObjToString();
                            numberOfPayments = dt.Rows[i]["pay#"].ObjToString();
                            amtOfMonthlyPayt = dt.Rows[i]["pamt"].ObjToString();
                            balanceDue = dt.Rows[i]["baldue"].ObjToString();
                            nowDue = dt.Rows[i]["nowd"].ObjToString();
                            lastDatePaid = GetSQLDate(dt, i, "ldate8");
                            downPayment = dt.Rows[i]["dpay"].ObjToString();

                            issueDate8 = GetSQLDate(dt, i, "issdt8");
                            issueDate8 = dt.Rows[i]["issdt8"].ObjToString();
                            iDate = DecodePayDate(issueDate8, issueDate8, "");
                            dueDate8 = dueDate8.Replace("-", "");
                            dDate = DecodePayDate(dueDate8, dueDate8, "");
                            if (iDate.Year >= 2100 || iDate.Year < 100)
                            {
                                if (dDate.Year >= 2100 || dDate.Year < 100)
                                {
                                    G1.WriteAudit("Bad Issue/Due Date Row=" + i.ToString() + " Contract " + contract + " D8=" + issueDate8 + "Due8=" + dueDate8 + "!");
                                    continue;
                                }
                            }
                            dueDate8 = GetSQLDate(dDate.ToString("MM/dd/yyyy"));
                            issueDate8 = GetSQLDate(iDate.ToString("MM/dd/yyyy"));
                            if (G1.validate_date(issueDate8) && issueDate8.IndexOf("2100-") < 0)
                            {
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
                                if (dx.Rows.Count > 0)
                                {
                                    tempDate2 = dx.Rows[0]["payDate8"].ObjToDateTime();
                                    if (tempDate2 < tempDate)
                                    {
                                        record = dx.Rows[0]["record"].ObjToString();
                                        issueDate8 = tempDate.ToString("yyyy-MM-dd");
                                        G1.update_db_table("payments", "record", record, new string[] { "payDate8", issueDate8 });
                                    }
                                }
                            }
                            G1.update_db_table("contracts", "record", record, new string[] { "downPayment", downPayment, "ageAtIssue", ageAtIssue, "numberOfPayments", numberOfPayments, "amtOfMonthlyPayt", amtOfMonthlyPayt, "balanceDue", balanceDue, "nowDue", nowDue, "lastDatePaid8", lastDatePaid8, "dueDate8", dueDate8, "apr", apr });
                            //G1.WriteAudit("***INFO*** Updated Contract " + contract + " Row=" + i.ToString());
                            continue;
                        }
                        G1.WriteAudit("***ERROR*** Bad Contract " + contract + " Row=" + i.ToString());
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
                        G1.WriteAudit("***ERROR*** Row=" + i.ToString() + " " + ex.ToString());
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
            G1.WriteAudit("Finished Import");
        }
        /***********************************************************************************************/
        private void ImportPreaccLapse(DataTable dt, string workwhat)
        {
            picLoader.Show();
            DataTable dx = null;
            DateTime tempDate = DateTime.Now;
            DateTime tempDate2 = DateTime.Now;
            string cmd = "";
            string record = "";
            string contract = "";
            string lapseDate8 = "";
            string dueDate8 = "";
            barImport.Show();
            int lastrow = dt.Rows.Count;
            int tableRow = 0;
            int created = 0;
            G1.CreateAudit("preacclap");
            DateTime beginDate = new DateTime(1998, 1, 1);
            DateTime endDate = new DateTime(2019, 11, 1);
            DateTime iDate = DateTime.Now;
            DateTime dDate = DateTime.Now;
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
                    if (i % 5000 == 0)
                        GC.Collect();
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
                        cmd = "Select * from `contracts` where `contractNumber` = '" + contract + "';";
                        dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count > 0)
                        {
                            record = dx.Rows[0]["record"].ObjToString();
                            dueDate8 = GetSQLDate(dt, i, "ddue8");

                            lapseDate8 = GetSQLDate(dt, i, "lapdt8");
                            lapseDate8 = lapseDate8.Replace("-", "");
                            iDate = DecodePayDate(lapseDate8, lapseDate8, "");
                            if (iDate.Year >= 2100 || iDate.Year < 100)
                            {
                                //                                G1.WriteAudit("Bad Issue/Due Date Row=" + i.ToString() + " Contract " + contract + " L8=" + lapseDate8 + "!");
                                continue;
                            }
                            dDate = DecodePayDate(dueDate8, dueDate8, "");
                            dueDate8 = GetSQLDate(dDate.ToString("MM/dd/yyyy"));

                            lapseDate8 = GetSQLDate(iDate.ToString("MM/dd/yyyy"));
                            G1.update_db_table("contracts", "record", record, new string[] { "lapsed", "Y", "lapseDate8", lapseDate8, "dueDate8", dueDate8 });
                            continue;
                        }
                        G1.WriteAudit("***ERROR*** Bad Contract " + contract + " Row=" + i.ToString());
                        if (1 == 1)
                            continue;
                    }
                    catch (Exception ex)
                    {
                        G1.WriteAudit("***ERROR*** Row=" + i.ToString() + " " + ex.ToString());
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
            G1.WriteAudit("Finished Import");
        }
        /***********************************************************************************************/
        private void ImportPreaccDead(DataTable dt, string workwhat)
        {
            picLoader.Show();
            DataTable dx = null;
            DateTime tempDate = DateTime.Now;
            DateTime tempDate2 = DateTime.Now;
            string cmd = "";
            string record = "";
            string record2 = "";
            string contract = "";
            string lapseDate8 = "";
            barImport.Show();
            int lastrow = dt.Rows.Count;
            int tableRow = 0;
            int created = 0;
            G1.CreateAudit("preaccdead");
            DateTime beginDate = new DateTime(1998, 1, 1);
            DateTime endDate = new DateTime(2019, 11, 1);
            DateTime iDate = DateTime.Now;
            DateTime dDate = DateTime.Now;
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
                    if (i % 5000 == 0)
                        GC.Collect();
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
                        cmd = "Select * from `contracts` where `contractNumber` = '" + contract + "';";
                        dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count > 0)
                        {
                            record = dx.Rows[0]["record"].ObjToString();

                            lapseDate8 = GetSQLDate(dt, i, "lapdt8"); // This is the deceased Date in the deceased file
                            lapseDate8 = lapseDate8.Replace("-", "");
                            iDate = DecodePayDate(lapseDate8, lapseDate8, "");
                            if (iDate.Year >= 2100 || iDate.Year < 100)
                            {
                                //                                G1.WriteAudit("Bad Issue/Due Date Row=" + i.ToString() + " Contract " + contract + " L8=" + lapseDate8 + "!");
                                continue;
                            }
                            lapseDate8 = GetSQLDate(iDate.ToString("MM/dd/yyyy"));
                            G1.update_db_table("contracts", "record", record, new string[] { "deceasedDate", lapseDate8 });

                            cmd = "Select * from `customers` where `contractNumber` = '" + contract + "';";
                            dx = G1.get_db_data(cmd);
                            if (dx.Rows.Count > 0)
                            {
                                record = dx.Rows[0]["record"].ObjToString();
                                G1.update_db_table("customers", "record", record, new string[] { "deceasedDate", lapseDate8 });
                            }
                            continue;
                        }
                        G1.WriteAudit("***ERROR*** Bad Contract " + contract + " Row=" + i.ToString());
                        if (1 == 1)
                            continue;
                    }
                    catch (Exception ex)
                    {
                        G1.WriteAudit("***ERROR*** Row=" + i.ToString() + " " + ex.ToString());
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
            G1.WriteAudit("Finished Import");
        }
        /***********************************************************************************************/
        private void FixBalances(string contract, DataTable dt, int i)
        {
            if (String.IsNullOrWhiteSpace(contract))
                return;
            DataTable dx = G1.get_db_data("Select * from `contracts` where `contractNumber` = '" + contract + "';");
            if (dx.Rows.Count <= 0)
            {
                MessageBox.Show("Missing Contract : " + contract);
                return;
            }

            string record = dx.Rows[0]["record"].ObjToString();
            string balanceDue = dt.Rows[i]["baldue"].ObjToString();
            string dueDate8 = GetSQLDate(dt, i, "ddue8");
            string lastDatePaid8 = GetSQLDate(dt, i, "ldate8");
            string nowDue = dt.Rows[i]["nowd"].ObjToString();
            G1.update_db_table("contracts", "record", record, new string[] { "balanceDue", balanceDue, "nowDue", nowDue, "dueDate8", dueDate8, "lastDatePaid8", lastDatePaid8 });
        }
        /***********************************************************************************************/
        private void FixLapses(string contract, DataTable dt, int i)
        {
            if (String.IsNullOrWhiteSpace(contract))
                return;
            DataTable dx = G1.get_db_data("Select * from `contracts` where `contractNumber` = '" + contract + "';");
            if (dx.Rows.Count <= 0)
            {
                MessageBox.Show("Missing Contract : " + contract);
                return;
            }

            string record = dx.Rows[0]["record"].ObjToString();
            string balanceDue = dt.Rows[i]["baldue"].ObjToString();
            string dueDate8 = GetSQLDate(dt, i, "ddue8");
            string lastDatePaid8 = GetSQLDate(dt, i, "ldate8");
            string nowDue = dt.Rows[i]["nowd"].ObjToString();
            G1.update_db_table("contracts", "record", record, new string[] { "balanceDue", balanceDue, "nowDue", nowDue, "dueDate8", dueDate8, "lastDatePaid8", lastDatePaid8 });
        }
        /***********************************************************************************************/
        private void FixDueDates(string contract, DataTable dt, int i)
        {
            string record = "";
            string downPayment = "";
            string issueDate8 = "";
            DateTime tempDate = DateTime.Now;
            DateTime tempDate2 = DateTime.Now;
            DateTime beginDate = new DateTime(2016, 1, 1);
            DateTime endDate = new DateTime(2019, 11, 1);
            int yy = 0;
            int mm = 0;
            int dd = 0;
            string cmd = "Select * from `contracts` where `contractNumber` = '" + contract + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
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
                        return;
                    tempDate2 = dx.Rows[0]["issueDate8"].ObjToDateTime();
                    if (tempDate != tempDate2)
                    {
                        issueDate8 = tempDate.ToString("yyyy-MM-dd");
                        G1.update_db_table("contracts", "record", record, new string[] { "issueDate8", issueDate8 });
                    }
                    cmd = "Select * from `payments` where `contractNumber` = '" + contract + "' and `downPayment` = '" + downPayment.ToString() + "';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                    {
                        tempDate2 = dx.Rows[0]["payDate8"].ObjToDateTime();
                        if (tempDate2 < tempDate)
                        {
                            record = dx.Rows[0]["record"].ObjToString();
                            issueDate8 = tempDate.ToString("yyyy-MM-dd");
                            G1.update_db_table("payments", "record", record, new string[] { "payDate8", issueDate8 });
                        }
                    }
                }
            }
        }
        /***********************************************************************************************/
        private void ImportDeathData(DataTable dt, string workwhat)
        {
            picLoader.Show();
            bool doingDeath = false;
            if (G1.get_column_number(dt, "Death Date") >= 0)
                doingDeath = true;
            if (!doingDeath)
            {
                MessageBox.Show("***ERROR*** Cannot Locate Death Date!");
                return;
            }
            DataTable dx = null;
            DateTime tempDate = DateTime.Now;
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
            barImport.Show();
            int lastrow = dt.Rows.Count;
            int tableRow = 0;
            int created = 0;
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
                        contract = dt.Rows[i]["contract"].ObjToString();
                        //if (contract != "B18013LI")
                        //    continue;
                        //if (contract.ToUpper() == "B18013LI")
                        //{

                        //}
                        cmd = "Select * from `contracts` where `contractNumber` = '" + contract + "';";
                        dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count > 0)
                            record = dx.Rows[0]["record"].ObjToString();
                        else
                        {
                            record = G1.create_record("contracts", "contractNumber", "-1");
                            if (G1.BadRecord("contracts", record))
                                continue;
                            created++;
                        }
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

                        deleteFlag = dt.Rows[i]["Delete"].ObjToString();
                        serviceTotal = dt.Rows[i]["Service Total"].ObjToString();
                        merchandiseTotal = dt.Rows[i]["Merchandise Total"].ObjToString();
                        allowMerchandise = dt.Rows[i]["Merch Allowance"].ObjToString();
                        allowInsurance = dt.Rows[i]["Insurance Allowance"].ObjToString();
                        downPayment = dt.Rows[i]["Down Pay"].ObjToString();
                        ageAtIssue = dt.Rows[i]["Issue Age"].ObjToString();
                        numberOfPayments = dt.Rows[i]["Number of Payments"].ObjToString();
                        amtOfMonthlyPayt = dt.Rows[i]["Monthly Payment Amount"].ObjToString();
                        balanceDue = dt.Rows[i]["Balance Due"].ObjToString();
                        nowDue = dt.Rows[i]["Now Due"].ObjToString();
                        pullCode = dt.Rows[i]["Pull"].ObjToString();

                        dueDate8 = GetSQLDate(dt, i, "Due Date");
                        issueDate8 = GetSQLDate(dt, i, "Issue Date");
                        reinstateDate8 = GetSQLDate(dt, i, "Reinstate Date");
                        apr = dt.Rows[i]["APR"].ObjToString();
                        totalInterest = dt.Rows[i]["Total Interest"].ObjToString();
                        interestPaid = dt.Rows[i]["Interest Paid"].ObjToString();

                        G1.update_db_table("contracts", "record", record, new string[] { "deleteFlag", deleteFlag, "serviceTotal", serviceTotal, "merchandiseTotal", merchandiseTotal });

                        G1.update_db_table("contracts", "record", record, new string[] { "allowMerchandise", allowMerchandise, "allowInsurance", allowInsurance, "downPayment", downPayment });
                        G1.update_db_table("contracts", "record", record, new string[] { "ageAtIssue", ageAtIssue, "numberOfPayments", numberOfPayments, "amtOfMonthlyPayt", amtOfMonthlyPayt });
                        G1.update_db_table("contracts", "record", record, new string[] { "nowDue", nowDue, "pullCode", pullCode, "balanceDue", balanceDue });
                        G1.update_db_table("contracts", "record", record, new string[] { "dueDate8", dueDate8, "issueDate8", issueDate8 });
                        G1.update_db_table("contracts", "record", record, new string[] { "reinstateDate8", reinstateDate8, "apr", apr, "totalInterest", totalInterest, "interestPaid", interestPaid });
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
        private void ImportAgentIncoming(DataTable dt, string workwhat)
        {
            picLoader.Show();
            labelMaximum.Show();
            labelMaximum.Text = "0";
            barImport.Show();

            int lastrow = dt.Rows.Count;
            string agentFDLIC = "";
            string agentCode = "";
            string agentFirstName = "";
            string agentLastName = "";
            string record = "";
            string cmd = "";

            int tableRow = 0;
            //            lastrow = 1;
            try
            {
                barImport.Minimum = 0;
                barImport.Maximum = lastrow;
                lblTotal.Text = "of " + lastrow.ToString();
                lblTotal.Refresh();
                int created = 0;
                picLoader.Show();
                DataTable dx = null;

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
                        agentFDLIC = dt.Rows[i]["FAGTCODE"].ObjToString();
                        if (String.IsNullOrWhiteSpace(agentFDLIC))
                            continue;
                        if (agentFDLIC == "013399")
                        {

                        }
                        agentCode = dt.Rows[i]["AGT"].ObjToString();
                        agentLastName = dt.Rows[i]["FAGTLNAME"].ObjToString();
                        agentFirstName = dt.Rows[i]["FAGTFNAME"].ObjToString();
                        cmd = "Select * from `agents` where `agentCode` = '" + agentCode + "';";
                        dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count <= 0)
                        {
                            record = G1.create_record("agents", "agentCode", "-1");
                            created++;
                        }
                        else
                            record = dx.Rows[0]["record"].ObjToString();
                        if (G1.BadRecord("agents", record))
                            continue;
                        G1.update_db_table("agents", "record", record, new string[] { "agentIncoming", agentFDLIC, "agentCode", agentCode, "firstName", agentFirstName, "lastName", agentLastName });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("***ERROR*** " + ex.Message.ToString());
                    }
                }
                picLoader.Hide();
                barImport.Value = lastrow;
                MessageBox.Show("Agent Data Import of " + lastrow + " Rows Complete - Created " + created.ToString() + " . . .");
            }
            catch (Exception ex)
            {
                MessageBox.Show("***MAJOR ERROR*** " + ex.Message.ToString());

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
                if (G1.validate_numeric(startText))
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
                        if (fixMerchandise)
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
            string sql_date = "0000-00-00";
            string date = "";
            try
            {
                date = dt.Rows[row][columnName].ObjToString();
                sql_date = G1.date_to_sql(date).Trim();
                if (sql_date == "0001-01-01")
                    sql_date = "0000-00-00";
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** SQL Date Error (" + date + ") " + ex.Message.ToString());
            }
            return sql_date;
        }
        /***********************************************************************************************/
        public static string GetSQLDate(string date)
        {
            string sql_date = "0000-00-00";
            try
            {
                sql_date = G1.date_to_sql(date).Trim();
                if (sql_date == "0001-01-01")
                    sql_date = "0000-00-00";
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** SQL Date Error (" + date + ") " + ex.Message.ToString());
            }
            return sql_date;
        }
        /***********************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgv.Visible)
                    G1.ShowHideFindPanel(mainGrid);
                else if (dgv2.Visible)
                    G1.SpyGlass(gridMain2);
                //if (this.mainGrid.OptionsFind.AlwaysVisible == true)
                //    mainGrid.OptionsFind.AlwaysVisible = false;
                //else
                //    mainGrid.OptionsFind.AlwaysVisible = true;
            }
            catch (Exception ex)
            {

            }
        }
        /***********************************************************************************************/
        private void ImportNewCustomerFile()
        {
            DataTable dt = (DataTable)dgv.DataSource;
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
            string areaCode = "";
            string phoneNumber = "";
            string extraItemAmtMR1 = "";
            string extraItemAmtMR2 = "";
            string directorSaleCode = "";
            string birthDate = "";
            string firstPayDate = "";
            string contractDate = "";
            string str = "";
            labelMaximum.Show();
            labelMaximum.Text = "0";
            barImport.Show();

            int lastrow = dt.Rows.Count;

            int tableRow = 0;
            //            lastrow = 1;
            try
            {
                barImport.Minimum = 0;
                barImport.Maximum = lastrow;
                lblTotal.Text = "of " + lastrow.ToString();
                picLoader.Show();

                for (int i = 1; i < lastrow; i++)
                {
                    //                    Application.DoEvents();
                    barImport.Value = i;
                    barImport.Refresh();
                    labelMaximum.Text = i.ToString();
                    labelMaximum.Refresh();

                    tableRow = i;
                    record = "";
                    try
                    {
                        contract = dt.Rows[i]["contract"].ObjToString();
                        if (String.IsNullOrWhiteSpace(contract))
                            continue;
                        if (contract == "/")
                            contract = "X" + i.ToString();
                        else if (contract == "\\")
                            contract = "X" + i.ToString();
                        str = dt.Rows[i]["pay date 8"].ObjToString();

                        cmd = "Select * from `customers` where `contractNumber` = '" + contract + "';";
                        dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count <= 0)
                        {
                            MessageBox.Show("***ERROR*** Bad Customer Contract " + contract + "!");
                            continue;
                        }
                        cmd = "Select * from `payments` where `contractNumber` = '" + contract + "';";
                        dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count <= 0)
                        {
                            MessageBox.Show("***ERROR*** Bad Contract Contract " + contract + "!");
                            continue;
                        }
                        if (1 == 1)
                            continue;
                        //if (dx.Rows.Count > 0)
                        //    record = dx.Rows[0]["record"].ObjToString();
                        //else
                        //    record = G1.create_record("customers", "contractNumber", "-1");
                        //if (string.IsNullOrWhiteSpace(record))
                        //{
                        //    MessageBox.Show("***ERROR*** Creating Customer Record! " + contract + " Stopping!");
                        //    break;
                        //}
                        //else if (record == "-1")
                        //{
                        //    MessageBox.Show("***ERROR*** Creating Customer Record! " + contract + " Stopping!");
                        //    break;
                        //}

                        //G1.update_db_table("customers", "record", record, new string[] { "contractNumber", contract });

                        //firstName = dt.Rows[i]["fname"].ObjToString();
                        //lastName = dt.Rows[i]["lname"].ObjToString();

                        //address1 = dt.Rows[i]["add1"].ObjToString();
                        //address2 = dt.Rows[i]["add2"].ObjToString();
                        //city = dt.Rows[i]["city"].ObjToString();
                        //state = dt.Rows[i]["state"].ObjToString();
                        //zip1 = dt.Rows[i]["zip1"].ObjToString();
                        //zip2 = dt.Rows[i]["zip2"].ObjToString();

                        //sex = dt.Rows[i]["sex"].ObjToString();
                        //ssn = dt.Rows[i]["ssno"].ObjToString();
                        //agentCode = dt.Rows[i]["anum"].ObjToString();
                        //coverageType = dt.Rows[i]["instr"].ObjToString();
                        //deleteFlag = dt.Rows[i]["del"].ObjToString();
                        //areaCode = dt.Rows[i]["area"].ObjToString();
                        //phoneNumber = dt.Rows[i]["phne"].ObjToString();
                        //extraItemAmtMR1 = dt.Rows[i]["mr1"].ObjToString();
                        //extraItemAmtMR2 = dt.Rows[i]["mr2"].ObjToString();
                        //directorSaleCode = dt.Rows[i]["dnum"].ObjToString();
                        //birthDate = GetSQLDate(dt, i, "bdate8");
                        //firstPayDate = GetSQLDate(dt, i, "fpay8");
                        //contractDate = GetSQLDate(dt, i, "cdte8");

                        //G1.update_db_table("customers", "record", record, new string[] { "firstName", firstName, "lastName", lastName, "address1", address1 });

                        //G1.update_db_table("customers", "record", record, new string[] { "address2", address2, "city", city, "state", state, "zip1", zip1, "zip2", zip2 });
                        //G1.update_db_table("customers", "record", record, new string[] { "sex", sex, "ssn", ssn, "agentCode", agentCode, "coverageType", coverageType, "deleteFlag", deleteFlag });
                        //G1.update_db_table("customers", "record", record, new string[] { "areaCode", areaCode, "phoneNumber", phoneNumber, "directorSaleCode", directorSaleCode, "birthDate", birthDate, "firstPayDate", firstPayDate, "contractDate", contractDate });
                        //G1.update_db_table("customers", "record", record, new string[] { "pulled", "2" });
                        //G1.update_db_table("customers", "record", record, new string[] { "lapsed", "Y" });
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
        private void ImportPayHistoryData(DataTable dt)
        {

            //DateTime ddd = DecodePayDate("19910618", "61891", "61891");
            //ddd = DecodePayDate("0", "61891", "61891");
            //ddd = DecodePayDate("0", "0", "61891");

            //string pDate8 = DeterminePayDate("19910618", "61891", "61891");

            picLoader.Show();

            DataTable dx = null;
            string cmd = "";
            string record = "";
            string contract = "";
            string lastName = "";
            string firstName = "";
            string paidDate = "";
            string downPayment = "";
            string checkNumber = "";
            string agentNumber = "";
            string paymentAmount = "";
            string numMonthPaid = "";
            string paymentDate = "";
            string debitAdjustment = "";
            string creditAdjustment = "";
            string debitReason = "";
            string creditReason = "";
            string location = "";
            string userId = "";
            string depositNumber = "";
            string interestPaid = "";
            string trust85P = "";
            string trust100P = "";
            string dueDate8 = "";
            string payDate8 = "";
            string payDate6 = "";
            string sysDate6 = "";
            DateTime actualPayDate8 = DateTime.Now;

            int bad = 0;

            labelMaximum.Show();
            labelMaximum.Text = "0";
            barImport.Show();
            int lastrow = dt.Rows.Count;
            int tableRow = 0;
            //lastrow = 85;
            int notFound = 0;
            string found = "Y";

            // CNUM	    LNAME	    FNAME	    DPAY	    ANUM	PAYAMT	    DEBIT	    CREDIT	LOC	USERID	DEP#	INTPD	TRUST85	TRUST100	DATE8	PAYDT8	FILL16
            // Contract	Last Name	First Name	Down Pay	Agent	Pay Amount	Debit Adj	Credit Adj	Interest Paid	Trust 85	Trust 100	Pay Date 8	User	Deposit #

            //Contract Last Name First Name Down Pay Agent   Pay Amount  PAYDTE Debit Adj Credit Adj Location    User Deposit #	Interest Paid	Trust 85	Trust 100	Due Date 8	Pay Date 8	FILL16


            // Contract Last Name,First Name,System Date when keyed,Down Pay,Agent,Pay Amount,PAYDTE,Debit Adj,Credit Adj,debit reason,credit reason,Location,User,Deposit #,Interest Paid,Trust 85,Trust 100,Due Date 8,Pay Date 8,FILL16


            //cmd = "Delete from `payments` where `paydate8` >= '20180701' AND `payDate8` <= '20190731';";
            //cmd = "Delete from `payments` where `paydate8` >= '20180501';";
            //            G1.get_db_data(cmd);

            G1.CreateAudit("Import Pay History");

            try
            {
                string str = "";
                lblTotal.Show();

                lblTotal.Text = "of " + lastrow.ToString();
                lblTotal.Refresh();

                barImport.Minimum = 0;
                barImport.Maximum = lastrow;
                int start = 0;
                //start = 37461;
                for (int i = start; i < lastrow; i++)
                {
                    if (i % 1000 == 0)
                        GC.Collect();
                    //                    Application.DoEvents();
                    picLoader.Refresh();
                    barImport.Value = i;
                    barImport.Refresh();
                    labelMaximum.Text = i.ToString();
                    labelMaximum.Refresh();
                    tableRow = i;
                    record = "";
                    contract = dt.Rows[i]["contract"].ObjToString();
                    if (String.IsNullOrWhiteSpace(contract))
                        continue;
                    //str = dt.Rows[i]["Pay Date 8"].ObjToString();
                    //if (str == "0")
                    //    continue;
                    //payDate8 = GetSQLDate(dt, i, "Pay Date 8");
                    //payDate6 = GetSQLDate(dt, i, "PAYDTE");
                    //sysDate6 = GetSQLDate(dt, i, "System Date when keyed");

                    payDate8 = dt.Rows[i]["Pay Date 8"].ObjToString();
                    payDate6 = dt.Rows[i]["PAYDTE"].ObjToString();
                    //sysDate6 = dt.Rows[i]["System Date when keyed"].ObjToString();
                    sysDate6 = dt.Rows[i]["SYSDTE"].ObjToString();

                    try
                    {
                        actualPayDate8 = DecodePayDate(payDate8, payDate6, sysDate6);
                        if (actualPayDate8.Year >= 2100 || actualPayDate8.Year < 100)
                        {
                            G1.WriteAudit("Bad Date Row=" + i.ToString() + " Contract " + contract + " D8=" + payDate8 + "D6=" + payDate6 + "S6=" + sysDate6 + "!");
                            continue;
                        }
                        payDate8 = GetSQLDate(actualPayDate8.ToString("MM/dd/yyyy"));
                    }
                    catch (Exception ex)
                    {
                        G1.WriteAudit("Bad Date Row=" + i.ToString() + " Contract " + contract + " D8=" + payDate8 + "D6=" + payDate6 + "S6=" + sysDate6 + "!");
                        continue;
                    }


                    try
                    {
                        dueDate8 = GetSQLDate(dt, i, "Due Date 8");

                        record = G1.create_record("payments", "contractNumber", "-1");
                        if (G1.BadRecord("payments", record))
                        {
                            bad++;
                            G1.WriteAudit("Bad Create Record Row=" + i.ToString() + " Contract " + contract + " D8=" + payDate8 + "D6=" + payDate6 + "S6=" + sysDate6 + "!");
                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        G1.WriteAudit("Bad Create Record Row=" + i.ToString() + " Contract " + contract + " D8=" + payDate8 + "D6=" + payDate6 + "S6=" + sysDate6 + "!");
                        continue;
                    }

                    try
                    {
                        lastName = dt.Rows[i]["Last Name"].ObjToString();
                        firstName = dt.Rows[i]["First Name"].ObjToString();

                        //                    paidDate = dt.Rows[i]["date"].ObjToString();
                        downPayment = dt.Rows[i]["Down Pay"].ObjToString();
                        //                    checkNumber = dt.Rows[i]["chk1"].ObjToString();
                        agentNumber = dt.Rows[i]["Agent"].ObjToString();

                        paymentAmount = dt.Rows[i]["Pay Amount"].ObjToString();
                        //                    numMonthPaid = dt.Rows[i]["tmon"].ObjToString();
                        //                    paymentDate = dt.Rows[i]["paydte"].ObjToString();
                        debitAdjustment = dt.Rows[i]["Debit Adj"].ObjToString();

                        creditAdjustment = dt.Rows[i]["Credit Adj"].ObjToString();

                        debitReason = dt.Rows[i]["debit reason"].ObjToString();
                        creditReason = dt.Rows[i]["credit reason"].ObjToString();


                        location = dt.Rows[i]["Location"].ObjToString();
                        userId = dt.Rows[i]["User"].ObjToString();
                        depositNumber = dt.Rows[i]["Deposit #"].ObjToString();

                        interestPaid = dt.Rows[i]["Interest Paid"].ObjToString();
                        trust85P = dt.Rows[i]["Trust 85"].ObjToString();
                        trust100P = dt.Rows[i]["Trust 100"].ObjToString();

                        //                    dueDate8 = dt.Rows[i]["date8"].ObjToString();
                        G1.update_db_table("payments", "record", record, new string[] { "contractNumber", contract, "payDate8", payDate8, "dueDate8", dueDate8, "lastName", lastName, "firstName", firstName, "downPayment", downPayment, "agentNumber", agentNumber, "paymentAmount", paymentAmount, "debitAdjustment", debitAdjustment, "creditAdjustment", creditAdjustment, "interestPaid", interestPaid, "trust85P", trust85P, "trust100P", trust100P, "location", location, "userID", userId, "depositNumber", depositNumber, "creditReason", creditReason, "debitReason", debitReason });
                    }
                    catch (Exception ex)
                    {
                        G1.WriteAudit("Bad Update Record Row=" + i.ToString() + " Contract " + contract + " D8=" + payDate8 + "D6=" + payDate6 + "S6=" + sysDate6 + "!");
                        continue;

                    }
                    //                    G1.update_db_table("payments", "record", record, new string[] { "contractNumber", contract, "payDate8", payDate8, "lastName", lastName, "firstName", firstName, "downPayment", downPayment, "agentNumber", agentNumber, "paymentAmount", paymentAmount, "debitAdjustment", debitAdjustment, "creditAdjustment", creditAdjustment, "interestPaid", interestPaid, "trust85P", trust85P, "trust100P", trust100P, "dueDate8", dueDate8 });
                    //G1.update_db_table("payments", "record", record, new string[] { "downPayment", downPayment, "agentNumber", agentNumber });
                    //G1.update_db_table("payments", "record", record, new string[] { "paymentAmount", paymentAmount, "debitAdjustment", debitAdjustment });
                    //G1.update_db_table("payments", "record", record, new string[] { "creditAdjustment", creditAdjustment });
                    //G1.update_db_table("payments", "record", record, new string[] { "interestPaid", interestPaid, "trust85P", trust85P });
                    //G1.update_db_table("payments", "record", record, new string[] { "trust100P", trust100P, "dueDate8", dueDate8 });
                }
                picLoader.Hide();
                //                barImport.Value = lastrow;
                MessageBox.Show("Payment History Data Import of " + lastrow + " Rows Complete . . . Bad=" + bad.ToString());
            }
            catch (Exception ex)
            {
                picLoader.Hide();
                MessageBox.Show("***ERROR*** Creating Payment History Record/Row! " + contract + "/" + tableRow.ToString() + " Stopping! " + ex.Message.ObjToString());
            }
            G1.WriteAudit("Stop Audit!");
        }
        /***********************************************************************************************/
        private string DeterminePayDate(string payDate8, string payDate6, string sysDate6)
        {
            string pDate8 = "2100-01-01";
            try
            {
                DateTime ddd = DecodePayDate(payDate8, payDate6, sysDate6);
                if (ddd.Year > 100)
                    pDate8 = GetSQLDate(ddd.ToString("MM/dd/yyyy"));
            }
            catch (Exception ex)
            {

            }
            return pDate8;
        }
        /***********************************************************************************************/
        private DateTime DecodePayDate(string payDate8, string payDate6, string sysDate6)
        {
            DateTime actualPayDate = new DateTime(2100, 1, 1);
            try
            {
                payDate8 = GetSQLDate(payDate8);

                DateTime testDate = payDate8.ObjToDateTime();
                if (testDate.Year < 100)
                {
                    if (payDate6.Length >= 5)
                    {
                        if (payDate6.Length == 5)
                            payDate6 = "0" + payDate6;
                        int mm = payDate6.Substring(0, 2).ObjToInt32();
                        int dd = payDate6.Substring(2, 2).ObjToInt32();
                        int yy = payDate6.Substring(4, 2).ObjToInt32();

                        if (yy < 40)
                            yy = yy + 2000;
                        else
                            yy = yy + 1900;
                        payDate6 = mm.ToString("D2") + "/" + dd.ToString("D2") + "/" + yy.ToString("D4");
                        testDate = payDate6.ObjToDateTime();
                        if (testDate.Year < 100)
                        {
                            if (sysDate6.Length >= 5)
                            {
                                if (sysDate6.Length == 5)
                                    sysDate6 = "0" + payDate6;
                                mm = sysDate6.Substring(0, 2).ObjToInt32();
                                dd = sysDate6.Substring(2, 2).ObjToInt32();
                                yy = sysDate6.Substring(4, 2).ObjToInt32();

                                if (yy < 40)
                                    yy = yy + 2000;
                                else
                                    yy = yy + 1900;
                                sysDate6 = mm.ToString("D2") + "/" + dd.ToString("D2") + "/" + yy.ToString("D4");
                                testDate = sysDate6.ObjToDateTime();
                                if (testDate.Year > 100)
                                    actualPayDate = testDate;
                            }
                        }
                        else
                            actualPayDate = testDate;
                    }
                    else
                    {
                        if (sysDate6.Length >= 5)
                        {
                            if (sysDate6.Length == 5)
                                sysDate6 = "0" + sysDate6;
                            int mm = sysDate6.Substring(0, 2).ObjToInt32();
                            int dd = sysDate6.Substring(2, 2).ObjToInt32();
                            int yy = sysDate6.Substring(4, 2).ObjToInt32();

                            if (yy < 40)
                                yy = yy + 2000;
                            else
                                yy = yy + 1900;
                            sysDate6 = mm.ToString("D2") + "/" + dd.ToString("D2") + "/" + yy.ToString("D4");
                            testDate = sysDate6.ObjToDateTime();
                            if (testDate.Year > 100)
                                actualPayDate = testDate;
                        }
                    }
                }
                else
                    actualPayDate = testDate;
            }
            catch (Exception ex)
            {

            }
            return actualPayDate;
        }
        /***********************************************************************************************/
        private void ImportDeceasedData(DataTable dt)
        {
            picLoader.Show();

            DataTable dx = null;
            string cmd = "";
            string record = "";
            string contract = "";
            string lastName = "";
            string firstName = "";
            string paidDate = "";
            string downPayment = "";
            string checkNumber = "";
            string agentNumber = "";
            string paymentAmount = "";
            string numMonthPaid = "";
            string paymentDate = "";
            string debitAdjustment = "";
            string creditAdjustment = "";
            string debitReason = "";
            string creditReason = "";
            string location = "";
            string userId = "";
            string depositNumber = "";
            string interestPaid = "";
            string trust85P = "";
            string trust100P = "";
            string dueDate8 = "";
            string payDate8 = "";
            string deathDate8 = "";

            labelMaximum.Show();
            labelMaximum.Text = "0";
            barImport.Show();
            int lastrow = dt.Rows.Count;
            int tableRow = 0;
            //lastrow = 85;
            int notFound = 0;
            string found = "Y";

            try
            {
                barImport.Minimum = 0;
                barImport.Maximum = lastrow;
                for (int i = 0; i < lastrow; i++)
                {
                    //                    Application.DoEvents();
                    picLoader.Refresh();
                    barImport.Value = i;
                    barImport.Refresh();
                    labelMaximum.Text = i.ToString();
                    labelMaximum.Refresh();
                    tableRow = i;
                    record = "";
                    contract = dt.Rows[i]["contract"].ObjToString();
                    //payDate8 = GetSQLDate(dt, i, "Pay Date 8");
                    //dueDate8 = GetSQLDate(dt, i, "Due Date 8");
                    deathDate8 = GetSQLDate(dt, i, "Death Date");
                    cmd = "Select * from `contracts` where `contractNumber` = '" + contract + "';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                    {
                        record = dx.Rows[0]["record"].ObjToString();
                        found = "";
                    }
                    else
                    {
                        //                        record = G1.create_record("payments", "contractNumber", "-1");
                        notFound++;
                        continue;
                    }
                    if (G1.BadRecord("contracts", record))
                        continue;

                    G1.update_db_table("contracts", "record", record, new string[] { "contractNumber", contract, "deceasedDate", deathDate8 });

                }
                picLoader.Hide();
                //                barImport.Value = lastrow;
                MessageBox.Show("Contract Death Data Import of " + lastrow + " Rows Complete . . . Not Found = " + notFound.ToString() + "!");
            }
            catch (Exception ex)
            {
                picLoader.Hide();
                MessageBox.Show("***ERROR*** Creating Payment History Record/Row! " + contract + "/" + tableRow.ToString() + " Stopping! " + ex.Message.ObjToString());
            }
        }
        /***********************************************************************************************/
        private string LookupAgentCode(string agentFDLIC, string contract)
        {
            string agentCode = "";
            string trust = "";
            string loc = "";
            string locCode = "";
            string miniContract = Trust85.decodeContractNumber(contract, ref trust, ref loc);
            for (; ; )
            {
                if (agentFDLIC.Length >= 6)
                    break;
                agentFDLIC = "0" + agentFDLIC;
            }

            string cmd = "Select * from `agents` where `agentIncoming` = '" + agentFDLIC + "';";
            //            DataTable dx = G1.get_db_data(cmd);
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
                agentCode = dx.Rows[0]["agentCode"].ObjToString();
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                locCode = dx.Rows[i]["locCode"].ObjToString();
                string[] Lines = locCode.Split(',');
                for (int j = 0; j < Lines.Length; j++)
                {
                    if (Lines[j].ToUpper() == loc.ToUpper())
                    {
                        agentCode = dx.Rows[i]["agentCode"].ObjToString();
                        break;
                    }
                }
            }
            return agentCode;
        }
        /***********************************************************************************************/
        private void ImportFDLIC_PhoneNumbers(DataTable dt, string workwhat)
        {
            picLoader.Show();
            labelMaximum.Show();
            labelMaximum.Text = "0";
            barImport.Show();

            int lastrow = dt.Rows.Count;

            string phoneNumber = "";
            string areaCode = "";

            double serviceTotal = 0D;
            double merchandiseTotal = 0D;
            double cashAdvance = 0D;
            double aptot = 0D;
            double downPayment = 0D;
            double trust85P = 0D;
            double trust100P = 0D;
            double balanceDue = 0D;
            double paymentAmount = 0D;
            double numberPayments = 0D;
            double contractAmount = 0D;

            string record = "";
            string contractRecord = "";
            string customerRecord = "";
            string cmd = "";
            string trustNumber = "";
            string CONTRACT_AMOUNT = "";
            string agentNumber = "";
            string apr = "";
            string dueDate = "";
            string issueDate = "";
            string agentCode = "";
            string dob = "";
            DateTime dtDOB;
            DateTime dtIssue;
            int ageAtIssue = 0;
            string instr = "";
            double totalInt = 0D;

            DateTime dateDPPaid = DateTime.Now;
            bool addDateDPPaid = false;
            string str = "";

            string DEPOSIT_NUMBER = "DIGI";
            string USER_ID = "DWNPA";

            string FUNDED_AMOUNT = "";
            string TOTAL_TO_PAY = "";
            string DOWN_PAYMENT = "";
            string PREMIUM = "";


            string SERVICES_BASIC_SERVICES = ""; // (1) BASIC SERVICES OF FUNERAL DIRECTOR AND STAFF
            string SERVICES_DIRECT_CREMATION = ""; // (36) DIRECT CREMATION WITH CONTAINER PROVIDED BY PURCHASER
            string SERVICES_VISITATION = ""; // (4)(5)(17)(18)
            string SERVICES_EMBALMING = ""; // (16) EMBALMING, AUTOPSY AND DONOR
            string SERVICES_TRANSFER_REMAINS = ""; // (9) TRANSFER OF REMAINS TO THE FUNERAL HOME
            string SERVICES_RECEIVING_REMAINS = ""; // (29) RECEIVING REMAINS FROM ANOTHER FUNERAL HOME
            string SERVICES_COMMITTAL_EQUIPMENT = "";
            string SERVICES_AUTOMOTIVE_EQUIPMENT = ""; // (11)(12)
            string SERVICES_TRANSPORTATION = ""; // (10) HEARSE
            string SERVICES_IMMEDIATE_BURIAL = "";
            string SERVICES_GRAVESIDE_SERVICE = ""; // (8) STAFF AND EQUIPMENT FOR GRAVESIDE SERVICE
            string SERVICES_FORWARDING_REMAINS = ""; // (28) FORWARDING REMAINS TO ANOTHER FUNERAL HOME
            string SERVICES_BODY_PREP = ""; // (3) OTHER PREPARATION OF THE BODY
            string SERVICES_FACILITIES = ""; // (6)(7)
            string SERVICES_DISCOUNTS = "";
            string SERVICES_GUARANTEED = "";

            string MERCH_CASKET_NAME = "";
            string MERCH_CASKET_PRICE = "";
            string MERCH_CASKET_DESCRIPTION = "";
            string MERCH_URN_NAME = "";
            string MERCH_URN_PRICE = "";
            string MERCH_URN_DESCRIPTION = "";
            string MERCH_OUTER_CONTAINER_NAME = "";
            string MERCH_OUTER_CONTAINER_PRICE = "";
            string MERCH_OUTER_CONTAINER_DESCRIPTION = "";
            string MERCH_ALT_CONTAINER_NAME = "";
            string MERCH_ALT_CONTAINER_PRICE = "";
            string MERCH_ALT_CONTAINER_DESCRIPTION = "";
            string MERCH_REGISTER_BOOK = ""; // (15) REGISTER BOOK AND POUCH
            string MERCH_GRAVE_MARKER = ""; // (14) TEMPORARY GRAVE MARKER
            string MERCH_ACKNOWLEDGEMENT_CARDS = ""; // (13) ACKNOLEDGEMENT CARDS
            string MERCH_OTHER = "";
            string MERCH_DISCOUNTS = "";
            string MERCH_GUARANTEED = "";

            string INSURED_PREFIX = "";
            string INSURED_FIRST_NAME = "";
            string INSURED_MIDDLE_INITIAL = "";
            string INSURED_LAST_NAME = "";
            string INSURED_SUFFIX = "";
            string INSURED_GENDER = "";
            string INSURED_ADDRESS1 = "";
            string INSURED_ADDRESS2 = "";
            string INSURED_CITY = "";
            string INSURED_STATE = "";
            string INSURED_ZIP = "";
            string INSURED_DOB = "";
            string INSURED_SSN = "";

            string CASH_ADVANCE_DEATH_CERTIFICATES = "";
            string CASH_ADVANCE_BEAUTICIAN = "";
            string CASH_ADVANCE_MUSIC = "";
            string CASH_ADVANCE_MSC = "";
            string CASH_ADVANCE_DISCOUNTS = "";

            int tableRow = 0;
            try
            {
                barImport.Minimum = 0;
                barImport.Maximum = lastrow;
                lblTotal.Text = "of " + lastrow.ToString();
                lblTotal.Show();
                lblTotal.Refresh();
                lblTotal.Show();
                int created = 0;
                picLoader.Show();
                DataTable dx = null;

                //                lastrow = 1;

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
                        trustNumber = dt.Rows[i]["TRUST_NUMBER"].ObjToString();
                        if (String.IsNullOrWhiteSpace(trustNumber))
                            continue;
                        cmd = "Select * from `contracts` where `contractNumber` = '" + trustNumber + "';";
                        DataTable contractDt = G1.get_db_data(cmd);
                        if (contractDt.Rows.Count > 0)
                            record = contractDt.Rows[0]["record"].ObjToString();
                        else
                        {
                            //                            record = G1.create_record("contracts", "contractNumber", "-1");
                            created++;
                            continue;
                        }
                        if (G1.BadRecord("contracts", record))
                            continue;
                        picLoader.Refresh();
                        contractRecord = record;
                        phoneNumber = dt.Rows[i]["MyPhoneNumber"].ObjToString();
                        areaCode = dt.Rows[i]["MyAreaCode"].ObjToString();
                        //if ( phoneNumber.Length == 10 )
                        //{
                        //    areaCode = phoneNumber.Substring(0, 3);
                        //    phoneNumber = phoneNumber.Replace(areaCode, "");
                        //    phoneNumber = phoneNumber.Substring(0, 3) + "-" + phoneNumber.Substring(3);
                        //    phoneNumber = "(" + areaCode + ") " + phoneNumber;
                        //}
                        //if (!String.IsNullOrWhiteSpace(phoneNumber))
                        //    G1.update_db_table("customers", "record", contractRecord, new string[] { "phoneNumber", phoneNumber, "areaCode", areaCode, "PhoneNumber1", phoneNumber });
                    }
                    catch (Exception ex)
                    {

                    }
                }
                picLoader.Hide();
                barImport.Value = lastrow;
                printPreviewToolStripMenuItem_Click(null, null);
                MessageBox.Show("Agent Data Import of " + lastrow + " Rows Complete - Created " + created.ToString() + " . . .");
            }
            catch (Exception ex)
            {

            }
        }
        /***********************************************************************************************/
        private bool ValidateFDLIC(DataTable dt, bool checkDateDpPaid = false)
        {
            bool rtn = true;
            if (recoveredFDLIC)
                return true;
            string dueDate = "";
            string issueDate = "";
            string dateDPPaid = "";
            string TOTAL_TO_PAY = "";
            string PREMIUM = "";
            double numberPayments = 0D;
            double contractValue = 0D;
            double apr = 0D;
            string contractNumber = "";
            bool showError = false;
            bool dueDateBad = false;
            bool dateDpPaidBad = false;
            bool signedDateBad = false;
            bool trustSeqDateBad = false;
            bool contractValueBad = false;
            bool contractAPRbad = false;
            bool numPaymentsBad = false;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    contractNumber = dt.Rows[i]["TRUST_NUMBER"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(contractNumber))
                    {
                        contractValue = dt.Rows[i]["CONTRACT_AMOUNT"].ObjToDouble();
                        apr = dt.Rows[i]["APR"].ObjToDouble();
                        showError = false;
                        if (apr > 5D)
                        {
                            TOTAL_TO_PAY = dt.Rows[i]["TOTAL_TO_PAY"].ObjToString();
                            PREMIUM = dt.Rows[i]["PREMIUM"].ObjToString();
                            numberPayments = TOTAL_TO_PAY.ObjToDouble() / PREMIUM.ObjToDouble();
                            if (numberPayments < 59)
                            {
                                showError = true;
                                numPaymentsBad = true;
                            }
                            if (contractValue > 1000D)
                            {
                                showError = true;
                                contractValueBad = true;
                            }
                        }
                        else if (apr <= 5D && contractValue < 2000D)
                        {
                            showError = true;
                            contractAPRbad = true;
                        }
                        if (showError)
                            MessageBox.Show("***WARNING*** APR (" + apr.ToString() + "%) may be incorrect for this contract (" + contractNumber + ")!\nPlease check it out!");
                    }
                }
                catch (Exception ex)
                {
                }

                try
                {
                    dueDate = dt.Rows[i]["DUE_DATE"].ObjToString();
                    if (dueDate.Length >= 8)
                        dueDate = dueDate.Substring(0, 8);
                    if (!G1.validate_date(dueDate))
                    {
                        dt.Rows[i]["DUE_DATE"] = "**" + dueDate + "**";
                        rtn = false;
                        dueDateBad = true;
                    }

                    if (checkDateDpPaid)
                    {
                        dateDPPaid = dt.Rows[i]["dateDPPaid"].ObjToString();
                        if (String.IsNullOrWhiteSpace(dateDPPaid))
                        {
                            rtn = false;
                            dateDpPaidBad = true;
                        }
                        else
                        {
                            if (!G1.validate_date(dateDPPaid))
                            {
                                dt.Rows[i]["dateDPPaid"] = "**" + dateDPPaid + "**";
                                rtn = false;
                                dateDpPaidBad = true;
                            }
                        }
                    }

                    issueDate = dt.Rows[i]["TRUST_SEQ_DATE"].ObjToString();
                    if (issueDate.Length >= 8)
                        issueDate = issueDate.Substring(0, 8);

                    if (!G1.validate_date(issueDate))
                    {
                        issueDate = dt.Rows[i]["SIGNED_DATE"].ObjToString();
                        if (issueDate.Length >= 8)
                            issueDate = issueDate.Substring(0, 8);
                        if (!G1.validate_date(issueDate))
                        {
                            dt.Rows[i]["SIGNED_DATE"] = "**" + issueDate + "**";
                            rtn = false;
                            signedDateBad = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                }
            }
            if (!rtn)
            {
                string message = "***ERROR*** \n";
                if (dateDpPaidBad)
                    message += "Date DP Paid is bad somewhere!\n";
                if (contractValueBad)
                    message += "Contract Value is bad somewhere!\n";
                if (contractAPRbad)
                    message += "Contract APR is bad somewhere!\n";
                if (dueDateBad)
                    message += "Due Date is bad somewhere!\n";
                if (trustSeqDateBad)
                    message += "Trust Seq Date is bad somewhere!\n";
                if (signedDateBad)
                    message += "Signed Date is bad somewhere!\n";
                if (numPaymentsBad)
                    message += "Number of Payments is bad somewhere!\n";
                MessageBox.Show(message, "FDLIC Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                //if ( checkDateDpPaid )
                //    MessageBox.Show("***ERROR*** There are some invalid DueDates, Trust_Seq_Dates or Date DP Paid in this File! You will not be able to import without fixing!");
                //else
                //    MessageBox.Show("***ERROR*** There are some invalid DueDates, or Trust_Seq_Dates in this File! You will not be able to import without fixing!");
            }
            return rtn;
        }
        /***********************************************************************************************/
        private void ImportNewContracts(DataTable dt, string workwhat)
        {
            bool rtn = ValidateFDLIC(dt);
            if (!rtn)
                return;
            picLoader.Show();
            labelMaximum.Show();
            labelMaximum.Text = "0";
            barImport.Show();

            int lastrow = dt.Rows.Count;
            DateTime date = DateTime.Now;

            double serviceTotal = 0D;
            double merchandiseTotal = 0D;
            double cashAdvance = 0D;
            double aptot = 0D;
            double downPayment = 0D;
            double trust85P = 0D;
            double trust100P = 0D;
            double balanceDue = 0D;
            double paymentAmount = 0D;
            double numberPayments = 0D;
            double contractAmount = 0D;

            string record = "";
            string contractRecord = "";
            string customerRecord = "";
            string cmd = "";
            string trustNumber = "";
            string CONTRACT_AMOUNT = "";
            string agentNumber = "";
            string apr = "";
            string dueDate = "";
            string issueDate = "";
            string agentCode = "";
            string dob = "";
            string bankAccount = "";
            double ccFee = 0D;
            string trustLocation = "";
            string trustDpRecord = "";
            DataTable downDt = null;
            DateTime dtDOB;
            DateTime dtIssue;
            int ageAtIssue = 0;
            string instr = "";
            double totalInt = 0D;

            DateTime dateDPPaid = DateTime.Now;
            bool addDateDPPaid = false;
            string str = "";

            string DEPOSIT_NUMBER = "DIGI";
            string USER_ID = "DWNPA";
            string depositNumber = "";

            string FUNDED_AMOUNT = "";
            string TOTAL_TO_PAY = "";
            string DOWN_PAYMENT = "";
            string PREMIUM = "";
            double fullDownPayment = 0D;
            double oldDownPayment = 0D;


            string SERVICES_BASIC_SERVICES = ""; // (1) BASIC SERVICES OF FUNERAL DIRECTOR AND STAFF
            string SERVICES_DIRECT_CREMATION = ""; // (36) DIRECT CREMATION WITH CONTAINER PROVIDED BY PURCHASER
            string SERVICES_VISITATION = ""; // (4)(5)(17)(18)
            string SERVICES_EMBALMING = ""; // (16) EMBALMING, AUTOPSY AND DONOR
            string SERVICES_TRANSFER_REMAINS = ""; // (9) TRANSFER OF REMAINS TO THE FUNERAL HOME
            string SERVICES_RECEIVING_REMAINS = ""; // (29) RECEIVING REMAINS FROM ANOTHER FUNERAL HOME
            string SERVICES_COMMITTAL_EQUIPMENT = "";
            string SERVICES_AUTOMOTIVE_EQUIPMENT = ""; // (11)(12)
            string SERVICES_TRANSPORTATION = ""; // (10) HEARSE
            string SERVICES_IMMEDIATE_BURIAL = "";
            string SERVICES_GRAVESIDE_SERVICE = ""; // (8) STAFF AND EQUIPMENT FOR GRAVESIDE SERVICE
            string SERVICES_FORWARDING_REMAINS = ""; // (28) FORWARDING REMAINS TO ANOTHER FUNERAL HOME
            string SERVICES_BODY_PREP = ""; // (3) OTHER PREPARATION OF THE BODY
            string SERVICES_FACILITIES = ""; // (6)(7)
            string SERVICES_DISCOUNTS = "";
            string SERVICES_GUARANTEED = "";

            string MERCH_CASKET_NAME = "";
            string MERCH_CASKET_PRICE = "";
            string MERCH_CASKET_DESCRIPTION = "";
            string MERCH_URN_NAME = "";
            string MERCH_URN_PRICE = "";
            string MERCH_URN_DESCRIPTION = "";
            string MERCH_OUTER_CONTAINER_NAME = "";
            string MERCH_OUTER_CONTAINER_PRICE = "";
            string MERCH_OUTER_CONTAINER_DESCRIPTION = "";
            string MERCH_ALT_CONTAINER_NAME = "";
            string MERCH_ALT_CONTAINER_PRICE = "";
            string MERCH_ALT_CONTAINER_DESCRIPTION = "";
            string MERCH_REGISTER_BOOK = ""; // (15) REGISTER BOOK AND POUCH
            string MERCH_GRAVE_MARKER = ""; // (14) TEMPORARY GRAVE MARKER
            string MERCH_ACKNOWLEDGEMENT_CARDS = ""; // (13) ACKNOLEDGEMENT CARDS
            string MERCH_OTHER = "";
            string MERCH_DISCOUNTS = "";
            string MERCH_GUARANTEED = "";

            string INSURED_PREFIX = "";
            string INSURED_FIRST_NAME = "";
            string INSURED_MIDDLE_INITIAL = "";
            string INSURED_LAST_NAME = "";
            string INSURED_SUFFIX = "";
            string INSURED_GENDER = "";
            string INSURED_ADDRESS1 = "";
            string INSURED_ADDRESS2 = "";
            string INSURED_CITY = "";
            string INSURED_STATE = "";
            string INSURED_ZIP = "";
            string INSURED_DOB = "";
            string INSURED_SSN = "";

            string CASH_ADVANCE_DEATH_CERTIFICATES = "";
            string CASH_ADVANCE_BEAUTICIAN = "";
            string CASH_ADVANCE_MUSIC = "";
            string CASH_ADVANCE_MSC = "";
            string CASH_ADVANCE_DISCOUNTS = "";
            string AREA_CODE = "";
            string PHONE_NUMBER = "";

            int tableRow = 0;
            bool showError = false;
            bool gotFuneral = false;
            string funContract = "";
            string serviceId = "";
            DateTime deceasedDate = DateTime.Now;
            DateTime serviceDate = DateTime.Now;
            DateTime arrangementDate = DateTime.Now;

            bool duplicate = false;

            try
            {
                barImport.Minimum = 0;
                barImport.Maximum = lastrow;
                lblTotal.Text = "of " + lastrow.ToString();
                lblTotal.Show();
                lblTotal.Refresh();
                lblTotal.Show();
                int created = 0;
                picLoader.Show();
                DataTable dx = null;

                //                lastrow = 1;

                double contractValue = 0D;
                double rate = 0D;

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
                        duplicate = false;
                        trustNumber = dt.Rows[i]["TRUST_NUMBER"].ObjToString();
                        trustNumber = trustNumber.Trim();
                        if (String.IsNullOrWhiteSpace(trustNumber))
                            continue;

                        addDateDPPaid = false;
                        str = dt.Rows[i]["dateDPPaid"].ObjToString();
                        if (!String.IsNullOrWhiteSpace(str))
                        {
                            dateDPPaid = str.ObjToDateTime();
                            if (dateDPPaid.Year > 1850)
                            {
                                addDateDPPaid = true;
                                str = G1.date_to_sql(dateDPPaid.ToString("MM/dd/yyyy"));
                                str = str.Replace("-", "");
                            }
                        }
                        ccFee = dt.Rows[i]["ccFee"].ObjToDouble();
                        ccFee = G1.RoundValue(ccFee);

                        bankAccount = dt.Rows[i]["bankAccount"].ObjToString();
                        depositNumber = dt.Rows[i]["deposit #"].ObjToString();
                        trustLocation = dt.Rows[i]["trustLocation"].ObjToString();
                        trustDpRecord = dt.Rows[i]["trustDpRecord"].ObjToString();

                        CONTRACT_AMOUNT = dt.Rows[i]["CONTRACT_AMOUNT"].ObjToString();
                        DOWN_PAYMENT = dt.Rows[i]["DOWN_PAYMENT"].ObjToString();
                        agentNumber = dt.Rows[i]["AGENT_NUMBER"].ObjToString();
                        apr = dt.Rows[i]["APR"].ObjToString();
                        PREMIUM = dt.Rows[i]["PREMIUM"].ObjToString();
                        dueDate = dt.Rows[i]["DUE_DATE"].ObjToString();
                        if (dueDate.Length >= 8)
                        {
                            if (dueDate.IndexOf("/") >= 0)
                            {
                                date = dueDate.ObjToDateTime();
                                dueDate = date.Year.ToString("D4") + date.Month.ToString("D2") + date.Day.ToString("D2");
                            }
                            else
                                dueDate = dueDate.Substring(0, 8);
                        }
                        if (!G1.validate_date(dueDate))
                        {

                        }


                        issueDate = dt.Rows[i]["TRUST_SEQ_DATE"].ObjToString();
                        if (issueDate.Length >= 8)
                        {
                            if (issueDate.IndexOf("/") >= 0)
                            {
                                date = issueDate.ObjToDateTime();
                                issueDate = date.Year.ToString("D4") + date.Month.ToString("D2") + date.Day.ToString("D2");
                            }
                            else
                                issueDate = issueDate.Substring(0, 8);
                        }

                        if (!G1.validate_date(issueDate))
                        {
                            issueDate = dt.Rows[i]["SIGNED_DATE"].ObjToString();
                            if (issueDate.Length >= 8)
                            {
                                if (issueDate.IndexOf("/") >= 0)
                                {
                                    date = issueDate.ObjToDateTime();
                                    issueDate = date.Year.ToString("D4") + date.Month.ToString("D2") + date.Day.ToString("D2");
                                }
                                else
                                    issueDate = issueDate.Substring(0, 8);
                            }
                        }

                        FUNDED_AMOUNT = dt.Rows[i]["FUNDED_AMOUNT"].ObjToString();
                        TOTAL_TO_PAY = dt.Rows[i]["TOTAL_TO_PAY"].ObjToString();

                        SERVICES_BASIC_SERVICES = dt.Rows[i]["SERVICES_BASIC_SERVICES"].ObjToString();
                        SERVICES_DIRECT_CREMATION = dt.Rows[i]["SERVICES_DIRECT_CREMATION"].ObjToString();
                        SERVICES_VISITATION = dt.Rows[i]["SERVICES_VISITATION"].ObjToString();
                        SERVICES_EMBALMING = dt.Rows[i]["SERVICES_EMBALMING"].ObjToString();
                        SERVICES_TRANSFER_REMAINS = dt.Rows[i]["SERVICES_TRANSFER_REMAINS"].ObjToString();
                        SERVICES_RECEIVING_REMAINS = dt.Rows[i]["SERVICES_RECEIVING_REMAINS"].ObjToString();
                        SERVICES_COMMITTAL_EQUIPMENT = dt.Rows[i]["SERVICES_COMMITTAL_EQUIPMENT"].ObjToString();
                        SERVICES_AUTOMOTIVE_EQUIPMENT = dt.Rows[i]["SERVICES_AUTOMOTIVE_EQUIPMENT"].ObjToString();
                        SERVICES_TRANSPORTATION = dt.Rows[i]["SERVICES_TRANSPORTATION"].ObjToString();
                        SERVICES_IMMEDIATE_BURIAL = dt.Rows[i]["SERVICES_IMMEDIATE_BURIAL"].ObjToString();
                        SERVICES_GRAVESIDE_SERVICE = dt.Rows[i]["SERVICES_GRAVESIDE_SERVICE"].ObjToString();
                        SERVICES_FORWARDING_REMAINS = dt.Rows[i]["SERVICES_FORWARDING_REMAINS"].ObjToString();
                        SERVICES_BODY_PREP = dt.Rows[i]["SERVICES_BODY_PREP"].ObjToString();
                        SERVICES_FACILITIES = dt.Rows[i]["SERVICES_FACILITIES"].ObjToString();
                        SERVICES_DISCOUNTS = dt.Rows[i]["SERVICES_DISCOUNTS"].ObjToString();
                        SERVICES_GUARANTEED = dt.Rows[i]["SERVICES_GUARANTEED"].ObjToString();

                        serviceTotal = SERVICES_BASIC_SERVICES.ObjToDouble() + SERVICES_DIRECT_CREMATION.ObjToDouble();
                        serviceTotal += SERVICES_VISITATION.ObjToDouble() + SERVICES_EMBALMING.ObjToDouble();
                        serviceTotal += SERVICES_TRANSFER_REMAINS.ObjToDouble() + SERVICES_RECEIVING_REMAINS.ObjToDouble();
                        serviceTotal += SERVICES_COMMITTAL_EQUIPMENT.ObjToDouble() + SERVICES_AUTOMOTIVE_EQUIPMENT.ObjToDouble();
                        serviceTotal += SERVICES_TRANSPORTATION.ObjToDouble() + SERVICES_IMMEDIATE_BURIAL.ObjToDouble();
                        serviceTotal += SERVICES_GRAVESIDE_SERVICE.ObjToDouble() + SERVICES_FORWARDING_REMAINS.ObjToDouble();
                        serviceTotal += SERVICES_BODY_PREP.ObjToDouble() + SERVICES_FACILITIES.ObjToDouble();
                        serviceTotal = G1.RoundValue(serviceTotal);

                        MERCH_CASKET_NAME = dt.Rows[i]["MERCH_CASKET_NAME"].ObjToString();
                        MERCH_CASKET_PRICE = dt.Rows[i]["MERCH_CASKET_PRICE"].ObjToString();
                        MERCH_CASKET_DESCRIPTION = dt.Rows[i]["MERCH_CASKET_DESCRIPTION"].ObjToString();
                        MERCH_URN_NAME = dt.Rows[i]["MERCH_URN_NAME"].ObjToString();
                        MERCH_URN_PRICE = dt.Rows[i]["MERCH_URN_PRICE"].ObjToString();
                        MERCH_URN_DESCRIPTION = dt.Rows[i]["MERCH_URN_DESCRIPTION"].ObjToString();
                        MERCH_OUTER_CONTAINER_NAME = dt.Rows[i]["MERCH_OUTER_CONTAINER_NAME"].ObjToString();
                        MERCH_OUTER_CONTAINER_PRICE = dt.Rows[i]["MERCH_OUTER_CONTAINER_PRICE"].ObjToString();
                        MERCH_OUTER_CONTAINER_DESCRIPTION = dt.Rows[i]["MERCH_OUTER_CONTAINER_DESCRIPTION"].ObjToString();
                        MERCH_ALT_CONTAINER_NAME = dt.Rows[i]["MERCH_ALT_CONTAINER_NAME"].ObjToString();
                        MERCH_ALT_CONTAINER_PRICE = dt.Rows[i]["MERCH_ALT_CONTAINER_PRICE"].ObjToString();
                        MERCH_ALT_CONTAINER_DESCRIPTION = dt.Rows[i]["MERCH_ALT_CONTAINER_DESCRIPTION"].ObjToString();
                        MERCH_REGISTER_BOOK = dt.Rows[i]["MERCH_REGISTER_BOOK"].ObjToString();
                        MERCH_GRAVE_MARKER = dt.Rows[i]["MERCH_GRAVE_MARKER"].ObjToString();
                        MERCH_ACKNOWLEDGEMENT_CARDS = dt.Rows[i]["MERCH_ACKNOWLEDGEMENT_CARDS"].ObjToString();
                        MERCH_OTHER = dt.Rows[i]["MERCH_OTHER"].ObjToString();
                        MERCH_DISCOUNTS = dt.Rows[i]["MERCH_DISCOUNTS"].ObjToString();
                        MERCH_GUARANTEED = dt.Rows[i]["MERCH_GUARANTEED"].ObjToString();

                        merchandiseTotal = MERCH_CASKET_PRICE.ObjToDouble() + MERCH_URN_PRICE.ObjToDouble();
                        merchandiseTotal += MERCH_OUTER_CONTAINER_PRICE.ObjToDouble() + MERCH_ALT_CONTAINER_PRICE.ObjToDouble();
                        merchandiseTotal += MERCH_REGISTER_BOOK.ObjToDouble() + MERCH_GRAVE_MARKER.ObjToDouble();
                        merchandiseTotal += MERCH_ACKNOWLEDGEMENT_CARDS.ObjToDouble() + MERCH_OTHER.ObjToDouble();
                        merchandiseTotal = G1.RoundValue(merchandiseTotal);

                        INSURED_PREFIX = dt.Rows[i]["INSURED_PREFIX"].ObjToString();
                        INSURED_FIRST_NAME = dt.Rows[i]["INSURED_FIRST_NAME"].ObjToString();
                        INSURED_MIDDLE_INITIAL = dt.Rows[i]["INSURED_MIDDLE_INITIAL"].ObjToString();
                        INSURED_LAST_NAME = dt.Rows[i]["INSURED_LAST_NAME"].ObjToString();
                        INSURED_SUFFIX = dt.Rows[i]["INSURED_SUFFIX"].ObjToString();
                        INSURED_GENDER = dt.Rows[i]["INSURED_GENDER"].ObjToString();
                        INSURED_ADDRESS1 = dt.Rows[i]["INSURED_ADDRESS1"].ObjToString();
                        INSURED_ADDRESS2 = dt.Rows[i]["INSURED_ADDRESS2"].ObjToString();
                        INSURED_CITY = dt.Rows[i]["INSURED_CITY"].ObjToString();
                        INSURED_STATE = dt.Rows[i]["INSURED_STATE"].ObjToString();
                        INSURED_ZIP = dt.Rows[i]["INSURED_ZIP"].ObjToString();
                        INSURED_DOB = dt.Rows[i]["INSURED_DOB"].ObjToString();
                        INSURED_SSN = dt.Rows[i]["INSURED_SSN"].ObjToString();

                        AREA_CODE = dt.Rows[i]["myAreaCode"].ObjToString();
                        PHONE_NUMBER = dt.Rows[i]["myPhoneNumber"].ObjToString();

                        CASH_ADVANCE_DEATH_CERTIFICATES = dt.Rows[i]["CASH_ADVANCE_DEATH_CERTIFICATES"].ObjToString();
                        CASH_ADVANCE_BEAUTICIAN = dt.Rows[i]["CASH_ADVANCE_BEAUTICIAN"].ObjToString();
                        CASH_ADVANCE_MUSIC = dt.Rows[i]["CASH_ADVANCE_MUSIC"].ObjToString();
                        CASH_ADVANCE_MSC = dt.Rows[i]["CASH_ADVANCE_MSC"].ObjToString();
                        CASH_ADVANCE_DISCOUNTS = dt.Rows[i]["CASH_ADVANCE_DISCOUNTS"].ObjToString();

                        cashAdvance = CASH_ADVANCE_DEATH_CERTIFICATES.ObjToDouble() + CASH_ADVANCE_BEAUTICIAN.ObjToDouble();
                        cashAdvance += CASH_ADVANCE_MUSIC.ObjToDouble() + CASH_ADVANCE_MSC.ObjToDouble();

                        dtDOB = INSURED_DOB.ObjToDateTime();
                        dob = dtDOB.Year.ToString("D4") + "-" + dtDOB.Month.ToString("D2") + "-" + dtDOB.Day.ToString("D2");
                        dtIssue = issueDate.ObjToDateTime();
                        ageAtIssue = G1.GetAge(dtDOB, dtIssue);

                        contractAmount = CONTRACT_AMOUNT.ObjToDouble();
                        contractAmount = G1.RoundValue(contractAmount);

                        aptot = Math.Abs(((FUNDED_AMOUNT.ObjToDouble()) / 0.85D) - contractAmount);
                        if (aptot < 0.01D)
                            aptot = 0D;
                        aptot = G1.RoundValue(aptot);

                        downPayment = DOWN_PAYMENT.ObjToDouble();
                        downPayment = G1.RoundValue(downPayment);

                        if (contractAmount == downPayment)
                            downPayment = downPayment - aptot;


                        //                        balanceDue = ((FUNDED_AMOUNT.ObjToDouble()) / 0.85D) - downPayment + cashAdvance; // This may not be correct
                        balanceDue = ((FUNDED_AMOUNT.ObjToDouble()) / 0.85D) - downPayment;
                        balanceDue = G1.RoundValue(balanceDue);
                        trust100P = downPayment;
                        trust85P = downPayment * 0.85D;

                        fullDownPayment = downPayment;

                        paymentAmount = PREMIUM.ObjToDouble();
                        if (balanceDue <= 0D)
                        {
                            paymentAmount = 0D;
                            numberPayments = 0D;
                            dueDate = "20391231";
                            totalInt = 0D;
                        }
                        else
                        {
                            numberPayments = TOTAL_TO_PAY.ObjToDouble() / PREMIUM.ObjToDouble();
                            totalInt = TOTAL_TO_PAY.ObjToDouble() - balanceDue;
                        }

                        if (FUNDED_AMOUNT.ObjToDouble() <= 0D)
                            instr = "I";
                        else
                            instr = "T";

                        agentCode = dt.Rows[i]["agentCode"].ObjToString();
                        if (String.IsNullOrWhiteSpace(agentCode))
                            agentCode = LookupAgentCode(agentNumber, trustNumber);
                        if (String.IsNullOrWhiteSpace(agentCode))
                        {
                            MessageBox.Show("***ERROR*** Missing Agent " + agentNumber + " Contract " + trustNumber);
                            continue;
                        }
                        //if (1 == 1)
                        //    continue;

                        gotFuneral = checkForFuneral(trustNumber, INSURED_SSN, ref funContract, ref deceasedDate, ref serviceId, ref serviceDate, ref arrangementDate);


                        cmd = "Select * from `contracts` where `contractNumber` = '" + trustNumber + "';";
                        DataTable contractDt = G1.get_db_data(cmd);
                        if (contractDt.Rows.Count > 0)
                        {
                            record = contractDt.Rows[0]["record"].ObjToString();
                            if ( G1.get_column_number ( dt, "duplicate") >= 0 )
                            {
                                if (dt.Rows[i]["duplicate"].ObjToString().Trim().ToUpper() == "Y")
                                {
                                    oldDownPayment = contractDt.Rows[0]["downPayment"].ObjToDouble();
                                    fullDownPayment += oldDownPayment;
                                    duplicate = true;
                                }
                            }
                        }
                        else
                        {
                            record = G1.create_record("contracts", "contractNumber", "-1");
                            created++;
                        }
                        if (G1.BadRecord("contracts", record))
                            continue;
                        picLoader.Refresh();
                        contractRecord = record;
                        G1.update_db_table("contracts", "record", record, new string[] { "contractNumber", trustNumber });

                        if (gotFuneral)
                            G1.update_db_table("contracts", "record", contractRecord, new string[] { "deceasedDate", deceasedDate.ToString("yyyy-MM-dd"), "serviceId", serviceId });

                        cmd = "Select * from `customers` where `contractNumber` = '" + trustNumber + "';";
                        DataTable customerDt = G1.get_db_data(cmd);
                        if (customerDt.Rows.Count > 0)
                            record = customerDt.Rows[0]["record"].ObjToString();
                        else
                            record = G1.create_record("customers", "contractNumber", "-1");
                        if (G1.BadRecord("customers", record))
                            continue;
                        picLoader.Refresh();
                        customerRecord = record;
                        if (gotFuneral)
                            G1.update_db_table("customers", "record", customerRecord, new string[] { "deceasedDate", deceasedDate.ToString("yyyy-MM-dd"), "serviceId", serviceId });

                        G1.update_db_table("contracts", "record", contractRecord, new string[] { "serviceTotal", serviceTotal.ToString(), "merchandiseTotal", merchandiseTotal.ToString(), "downPayment", fullDownPayment.ToString(), "allowInsurance", aptot.ToString(), "balanceDue", balanceDue.ToString(), "cashAdvance", cashAdvance.ToString(), "apr", apr.ToString() });
                        G1.update_db_table("contracts", "record", contractRecord, new string[] { "numberOfPayments", numberPayments.ToString(), "amtOfMonthlyPayt", paymentAmount.ToString(), "ageAtIssue", ageAtIssue.ToString(), "totalInterest", totalInt.ToString(), "issueDate8", issueDate, "dueDate8", dueDate, "lastDatePaid8", issueDate });
                        if (addDateDPPaid)
                        {
                            str = G1.date_to_sql(dateDPPaid.ToString("MM/dd/yyyy"));
                            str = str.Replace("-", "");
                            G1.update_db_table("contracts", "record", contractRecord, new string[] { "dateDPPaid", str });
                        }

                        G1.update_db_table("customers", "record", record, new string[] { "contractNumber", trustNumber, "firstName", INSURED_FIRST_NAME, "lastName", INSURED_LAST_NAME, "middleName", INSURED_MIDDLE_INITIAL, "suffix", INSURED_SUFFIX, "prefix", INSURED_PREFIX, "areaCode", AREA_CODE, "phoneNumber", PHONE_NUMBER });
                        G1.update_db_table("customers", "record", record, new string[] { "birthDate", dob, "sex", INSURED_GENDER, "address1", INSURED_ADDRESS1, "address2", INSURED_ADDRESS2, "city", INSURED_CITY, "state", INSURED_STATE, "zip1", INSURED_ZIP, "agentCode", agentCode, "coverageType", instr, "ssn", INSURED_SSN });

                        picLoader.Refresh();
                        if (downPayment > 0D)
                        {
                            cmd = "Select * from `payments` where `contractNumber` = '" + trustNumber + "' and `downPayment` > '0.00';";
                            DataTable paymentDt = G1.get_db_data(cmd);
                            string paymentRecord = "";
                            if (paymentDt.Rows.Count > 0 && !duplicate )
                                paymentRecord = paymentDt.Rows[0]["record"].ObjToString();
                            else
                                paymentRecord = G1.create_record("payments", "lastName", "-1");
                            if (!G1.BadRecord("payments", paymentRecord))
                            {
                                if (!G1.validate_date(dateDPPaid))
                                    dateDPPaid = issueDate.ObjToDateTime();
                                G1.update_db_table("payments", "record", paymentRecord, new string[] { "contractNumber", trustNumber, "firstName", INSURED_FIRST_NAME, "lastName", INSURED_LAST_NAME });
                                G1.update_db_table("payments", "record", paymentRecord, new string[] { "downPayment", downPayment.ToString(), "trust85P", trust85P.ToString(), "trust100P", trust100P.ToString(), "agentNumber", agentCode, "payDate8", dateDPPaid.ToString("MM/dd/yyyy"), "dueDate8", dueDate, "bank_account", bankAccount, "depositNumber", depositNumber, "ccFee", ccFee.ToString() });
                                if ( ccFee > 0D )
                                    G1.AddToAudit(LoginForm.username, "FDLIC Import", "CCFee", "CCfee = " + ccFee.ToString(), trustNumber);

                                //G1.update_db_table("payments", "record", paymentRecord, new string[] { "downPayment", downPayment.ToString(), "trust85P", trust85P.ToString(), "trust100P", trust100P.ToString(), "agentNumber", agentCode, "payDate8", issueDate, "dueDate8", dueDate, "bank_account", bankAccount, "depositNumber", depositNumber, "ccFee", ccFee.ToString() });
                            }
                        }

                        picLoader.Refresh();

                        AddServices(trustNumber, "BASIC SERVICES OF FUNERAL DIRECTOR AND STAFF", SERVICES_BASIC_SERVICES, "Service");
                        AddServices(trustNumber, "DIRECT CREMATION WITH CONTAINER PROVIDED BY PURCHASER", SERVICES_DIRECT_CREMATION, "Service");
                        AddServices(trustNumber, "FACILITY, STAFF AND EQUIPMENT FOR VISITATION EVENING BEFORE FUNERAL SERVICE", SERVICES_VISITATION, "Service");
                        //                        AddServices(trustNumber, "EMBALMING, AUTOPSY AND DONOR", SERVICES_EMBALMING, "Service");
                        AddServices(trustNumber, "EMBALMING", SERVICES_EMBALMING, "Service");
                        AddServices(trustNumber, "TRANSFER OF REMAINS TO THE FUNERAL HOME", SERVICES_TRANSFER_REMAINS, "Service");
                        AddServices(trustNumber, "RECEIVING REMAINS FROM ANOTHER FUNERAL HOME", SERVICES_RECEIVING_REMAINS, "Service");
                        AddServices(trustNumber, "Committal Equipment", SERVICES_COMMITTAL_EQUIPMENT, "Service");
                        AddServices(trustNumber, "LEAD/SAFETY CAR", SERVICES_AUTOMOTIVE_EQUIPMENT, "Service");
                        AddServices(trustNumber, "HEARSE", SERVICES_TRANSPORTATION, "Service");
                        AddServices(trustNumber, "Immediate Burial", SERVICES_IMMEDIATE_BURIAL, "Service");
                        AddServices(trustNumber, "STAFF AND EQUIPMENT FOR GRAVESIDE SERVICE", SERVICES_GRAVESIDE_SERVICE, "Service");
                        AddServices(trustNumber, "FORWARDING REMAINS TO ANOTHER FUNERAL HOME", SERVICES_FORWARDING_REMAINS, "Service");
                        AddServices(trustNumber, "OTHER PREPARATION OF THE BODY", SERVICES_BODY_PREP, "Service");
                        AddServices(trustNumber, "FACILITY, STAFF AND EQUIPMENT FOR FUNERAL SERVICE", SERVICES_FACILITIES, "Service");
                        AddServices(trustNumber, "Services Discounts", SERVICES_DISCOUNTS, "Service");
                        AddServices(trustNumber, "Services Guaranteed", SERVICES_GUARANTEED, "Service");
                        picLoader.Refresh();

                        AddServices(trustNumber, "Casket Name", MERCH_CASKET_NAME, "Merchandise");
                        AddServices(trustNumber, "Casket Price", MERCH_CASKET_PRICE, "Merchandise");
                        AddServices(trustNumber, "Casket Description", MERCH_CASKET_DESCRIPTION, "Merchandise");
                        AddServices(trustNumber, "URN Name", MERCH_URN_NAME, "Merchandise");
                        AddServices(trustNumber, "URN Price", MERCH_URN_PRICE, "Merchandise");
                        AddServices(trustNumber, "URN Description", MERCH_URN_DESCRIPTION, "Merchandise");
                        AddServices(trustNumber, "Outer Container Name", MERCH_OUTER_CONTAINER_NAME, "Merchandise");
                        AddServices(trustNumber, "Outer Container Price", MERCH_OUTER_CONTAINER_PRICE, "Merchandise");
                        AddServices(trustNumber, "Outer Container Description", MERCH_OUTER_CONTAINER_DESCRIPTION, "Merchandise");
                        AddServices(trustNumber, "ALT Container Name", MERCH_ALT_CONTAINER_NAME, "Merchandise");
                        AddServices(trustNumber, "ALT Container Price", MERCH_ALT_CONTAINER_PRICE, "Merchandise");
                        AddServices(trustNumber, "ALT Container Description", MERCH_ALT_CONTAINER_DESCRIPTION, "Merchandise");
                        AddServices(trustNumber, "REGISTER BOOK AND POUCH", MERCH_REGISTER_BOOK, "Merchandise");
                        AddServices(trustNumber, "TEMPORARY GRAVE MARKER", MERCH_GRAVE_MARKER, "Merchandise");
                        AddServices(trustNumber, "ACKNOWLEDGEMENT CARDS", MERCH_ACKNOWLEDGEMENT_CARDS, "Merchandise");
                        AddServices(trustNumber, "Merchandise Other", MERCH_OTHER, "Merchandise");
                        AddServices(trustNumber, "Merchandise Discounts", MERCH_DISCOUNTS, "Merchandise");
                        AddServices(trustNumber, "Merchandise Guaranteed", MERCH_GUARANTEED, "Merchandise");
                        picLoader.Refresh();


                        AddServices(trustNumber, "Cash Death Certificates", CASH_ADVANCE_DEATH_CERTIFICATES, "Cash Advance");
                        AddServices(trustNumber, "Cash Beautician", CASH_ADVANCE_BEAUTICIAN, "Cash Advance");
                        AddServices(trustNumber, "Cash Music", CASH_ADVANCE_MUSIC, "Cash Advance");
                        AddServices(trustNumber, "Cash Msc", CASH_ADVANCE_MSC, "Cash Advance");
                        AddServices(trustNumber, "Cash Discounts", CASH_ADVANCE_DISCOUNTS, "Cash Advance");
                        if (!String.IsNullOrWhiteSpace(trustDpRecord) && !String.IsNullOrWhiteSpace(trustLocation))
                        {
                            string locationDetail = GetLocationDetail(trustLocation);
                            G1.update_db_table("downpayments", "record", trustDpRecord, new string[] { "location", locationDetail });
                        }

                        if (gotFuneral)
                        {
                            dt.Rows[i]["TRUST_NUMBER"] = trustNumber + "/" + funContract;
                            G1.AddToAudit(LoginForm.username, "Customers", "Convert Funeral " + funContract + " to " + trustNumber, "Convert Funeral", trustNumber);
                        }
                        picLoader.Refresh();

                        //                        string CASH_ADVANCE_DEATH_CERTIFICATES = "";
                        //                        string CASH_ADVANCE_BEAUTICIAN = "";
                        //string CASH_ADVANCE_MUSIC = "";
                        //string CASH_ADVANCE_MSC = "";
                        //string CASH_ADVANCE_DISCOUNTS = "";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("***ERROR*** " + ex.Message.ToString());
                    }
                }
                picLoader.Hide();
                barImport.Value = lastrow;
                printPreviewToolStripMenuItem_Click(null, null);
                MessageBox.Show("Agent Data Import of " + lastrow + " Rows Complete - Created " + created.ToString() + " . . .");
            }
            catch (Exception ex)
            {
                MessageBox.Show("*** Add New Contracts ERROR *** " + ex.Message.ToString());

            }
        }
        /***********************************************************************************************/
        private string GetLocationDetail ( string keyCode )
        {
            string location = keyCode;
            if (String.IsNullOrWhiteSpace(keyCode))
                return location;
            string cmd = "Select * from `funeralhomes` where `keycode` = '" + keyCode + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
                location = dx.Rows[0]["LocationCode"].ObjToString();
            return location;
        }
        /***********************************************************************************************/
        private bool checkForFuneral(string contractNumber, string ssn, ref string funContract, ref DateTime deceasedDate, ref string serviceId, ref DateTime serviceDate, ref DateTime arrangementDate)
        {
            if (String.IsNullOrWhiteSpace(ssn))
                return false;
            if (ssn == "0")
                return false;
            if (ssn == "1")
                return false;

            funContract = "";
            string cmd = "Select * from `fcustomers` where `ssn` = '" + ssn + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return false;

            funContract = dt.Rows[0]["contractNumber"].ObjToString();
            DialogResult result = DevExpress.XtraEditors.XtraMessageBox.Show("***Question***\nFuneral already exists for SSN " + ssn + "\nUnder Contract (" + funContract + ")!\n" + "Would you like to convert that Funeral to this contract (" + contractNumber + ")?", "Funeral Exists Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
            if (result == DialogResult.No)
            {
                funContract = "";
                return false;
            }

            serviceId = dt.Rows[0]["serviceId"].ObjToString();
            deceasedDate = dt.Rows[0]["deceasedDate"].ObjToDateTime();

            string record = dt.Rows[0]["record"].ObjToString();
            G1.update_db_table("fcustomers", "record", record, new string[] { "contractNumber", contractNumber });

            cmd = "Select * from `fcontracts` where `contractNumber` = '" + funContract + "';";
            dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                record = dt.Rows[0]["record"].ObjToString();
                G1.update_db_table("fcontracts", "record", record, new string[] { "contractNumber", contractNumber });
            }

            cmd = "Select * from `fcust_extended` where `contractNumber` = '" + funContract + "';";
            dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                serviceDate = dt.Rows[0]["serviceDate"].ObjToDateTime();
                arrangementDate = dt.Rows[0]["arrangementDate"].ObjToDateTime();
                record = dt.Rows[0]["record"].ObjToString();
                G1.update_db_table("fcust_extended", "record", record, new string[] { "contractNumber", contractNumber });

                cmd = "Select * from `cust_extended` where `contractNumber` = '" + funContract + "';";
                dt = G1.get_db_data(cmd);
                if (dt.Rows.Count > 0)
                {
                    record = dt.Rows[0]["record"].ObjToString();
                    G1.delete_db_table("cust_extended", "record", record);
                }
                CustomerDetails.CopyFromTableToTable("fcust_extended", "cust_extended", contractNumber);
            }

            cmd = "Select * from `fcust_services` where `contractNumber` = '" + funContract + "';";
            dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                for (int j = 0; j < dt.Rows.Count; j++)
                {
                    record = dt.Rows[j]["record"].ObjToString();
                    G1.update_db_table("fcust_services", "record", record, new string[] { "contractNumber", contractNumber });
                }
            }

            cmd = "Select * from `cust_payments` where `contractNumber` = '" + funContract + "';";
            dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                for (int j = 0; j < dt.Rows.Count; j++)
                {
                    record = dt.Rows[j]["record"].ObjToString();
                    G1.update_db_table("cust_payments", "record", record, new string[] { "contractNumber", contractNumber });
                }
            }

            cmd = "Select * from `cust_payment_details` where `contractNumber` = '" + funContract + "';";
            dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                for (int j = 0; j < dt.Rows.Count; j++)
                {
                    record = dt.Rows[j]["record"].ObjToString();
                    G1.update_db_table("cust_payment_details", "record", record, new string[] { "contractNumber", contractNumber });
                }
            }

            cmd = "Select * from `cust_payment_ins_checklist` where `contractNumber` = '" + funContract + "';";
            dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                for (int j = 0; j < dt.Rows.Count; j++)
                {
                    record = dt.Rows[j]["record"].ObjToString();
                    G1.update_db_table("cust_payment_ins_checklist", "record", record, new string[] { "contractNumber", contractNumber });
                }
            }

            cmd = "Select * from `relatives` where `contractNumber` = '" + funContract + "';";
            dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                for (int j = 0; j < dt.Rows.Count; j++)
                {
                    record = dt.Rows[j]["record"].ObjToString();
                    G1.update_db_table("relatives", "record", record, new string[] { "contractNumber", contractNumber });
                }
            }

            cmd = "Select * from `contracts` where `contractNumber` = '" + contractNumber + "';";
            dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                record = dt.Rows[0]["record"].ObjToString();
                G1.update_db_table("contracts", "record", record, new string[] { "deceasedDate", deceasedDate.ToString("yyyy-MM-dd") });
            }

            cmd = "Select * from `customers` where `contractNumber` = '" + contractNumber + "';";
            dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                record = dt.Rows[0]["record"].ObjToString();
                G1.update_db_table("customers", "record", record, new string[] { "ServiceId", serviceId, "deceasedDate", deceasedDate.ToString("yyyy-MM-dd") });
            }

            return true;
        }
        /***********************************************************************************************/
        private void SetNewContractColumnWidths()
        {
            this.mainGrid.OptionsPrint.AutoWidth = false;
            GridView gridView = mainGrid as GridView;
            mainGrid.Columns["Num"].Fixed = DevExpress.XtraGrid.Columns.FixedStyle.Left;
            mainGrid.Columns["TRUST_NUMBER"].Fixed = DevExpress.XtraGrid.Columns.FixedStyle.Left;
            this.gridBand2.Columns.Add(mainGrid.Columns["Num"]);
            this.gridBand2.Columns.Add(mainGrid.Columns["TRUST_NUMBER"]);
            gridBand2.Fixed = DevExpress.XtraGrid.Columns.FixedStyle.Left;
            for (int i = 2; i < mainGrid.Columns.Count; i++)
            {
                this.gridBand1.Columns.Add(mainGrid.Columns[i]);
            }
            mainGrid.OptionsPrint.AutoWidth = false;

            DataTable dt = (DataTable)dgv.DataSource;
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                mainGrid.Columns[i].Width = 75;
            }
            mainGrid.Columns["DUE_DATE"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            mainGrid.Columns["DUE_DATE"].DisplayFormat.FormatString = "mm/dd/yyyy";
            ProcessDate(dt, "DUE_DATE");
            ProcessDate(dt, "SIGNED_DATE");
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
            if ( dgv2.Visible )
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
        }
        /***********************************************************************************************/
        private void PrintingSystem1_PageSettingsChanged(object sender, EventArgs e)
        {
        }
        /***********************************************************************************************/
        private bool forcePrint = false;
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
            if (forcePrint)
                printableComponentLink1.Print();
            else
                printableComponentLink1.PrintDlg();
            forcePrint = false;
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
            if ( dgv2.Visible )
                Printer.DrawQuad(4, 8, 7, 4, "Duplicate Data for (" + workWhat + ") File: " + actualFile, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            else
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
        private void ImportBatesville(DataTable dt, string workwhat)
        {
            picLoader.Show();
            labelMaximum.Show();
            labelMaximum.Text = "0";
            barImport.Show();

            int lastrow = dt.Rows.Count;
            string qty = "";
            string code = "";
            string itemnumber = "";
            string description = "";
            string record = "";
            string cmd = "";

            int found = 0;
            int doublecount = 0;
            int tableRow = 0;
            //            lastrow = 1;
            try
            {
                barImport.Minimum = 0;
                barImport.Maximum = lastrow;
                lblTotal.Text = "of " + lastrow.ToString();
                lblTotal.Refresh();
                int created = 0;
                picLoader.Show();
                DataTable dx = null;

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
                        qty = dt.Rows[i][1].ObjToString();
                        code = dt.Rows[i][2].ObjToString();
                        itemnumber = dt.Rows[i][3].ObjToString();
                        description = dt.Rows[i][4].ObjToString();
                        if (!G1.validate_numeric(qty))
                        {
                            dt.Rows[i]["num"] = "ERROR";
                            continue;
                        }
                        cmd = "SELECT * FROM `inventorylist` WHERE `casketcode` LIKE '%" + code + "%' AND `casketdesc` LIKE '%" + description + "%';";
                        dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count > 0)
                        {
                            if (dx.Rows.Count > 1)
                                doublecount++;
                            found++;
                            record = dx.Rows[0]["record"].ObjToString();
                            G1.update_db_table("inventorylist", "record", record, new string[] { "itemnumber", itemnumber });
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("***ERROR*** " + ex.Message.ToString());
                    }
                }
                picLoader.Hide();
                barImport.Value = lastrow;
                MessageBox.Show("Batesville Data Import of " + lastrow.ToString() + " Rows Complete - Found " + found.ToString() + " Doubles = " + doublecount.ToString() + " . . .");
            }
            catch (Exception ex)
            {
                MessageBox.Show("***MAJOR ERROR*** " + ex.Message.ToString());

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
                        if (debug)
                            e.DisplayText = num;
                    }
                }
            }
            else if (e.Column.FieldName.ToUpper() == "DATEDPPAID")
            {
                if (e.RowHandle >= 0 && !recoveredFDLIC)
                {
                    string date = e.DisplayText;
                    if (!String.IsNullOrWhiteSpace(date))
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
                    if (myMonth != Month || myYear != Year)
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
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                record = dx.Rows[i]["record"].ObjToString();
                G1.update_db_table("payments", "record", record, new string[] { "agentNumber", newAgent });
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
                    if (dx.Rows.Count > 0)
                    {
                        record = dx.Rows[0]["record"].ObjToString();
                        G1.update_db_table("contracts", "record", record, new string[] { "issueDate8", issueDate });
                    }
                }
            }
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void btnFixAll_Clickx(object sender, EventArgs e)
        {
            string cmd = "";
            DataTable dt = (DataTable)dgv.DataSource;
            string contract = "";
            string ssn = "";
            string oldAgent = "";
            string newAgent = "";
            this.Cursor = Cursors.WaitCursor;
            for (int j = 0; j < dt.Rows.Count; j++)
            {
                contract = dt.Rows[j]["TRUST_NUMBER"].ObjToString();
                ssn = dt.Rows[j]["INSURED_SSN"].ObjToString();
                oldAgent = dt.Rows[j]["OldAgent"].ObjToString();
                newAgent = dt.Rows[j]["agentCode"].ObjToString();
                DataTable dx = new DataTable();
                if (!String.IsNullOrWhiteSpace(contract))
                {
                    cmd = "Select * from `customers` where `contractNumber` = '" + contract + "';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count <= 0)
                    {
                        MessageBox.Show("***ERROR*** Contract Number " + contract + " Does Not Exist");
                        continue;
                    }
                }
                string name = CustomerDetails.GetAgentName(newAgent);
                dt.Rows[j]["agentName"] = name;
                dt.Rows[j]["OldAgent"] = newAgent;
                string record = dx.Rows[0]["record"].ObjToString();
                G1.update_db_table("customers", "record", record, new string[] { "agentCode", newAgent, "ssn", ssn });
                cmd = "Select * from `payments` where `contractNumber` = '" + contract + "';";
                dx = G1.get_db_data(cmd);
                for (int i = 0; i < dx.Rows.Count; i++)
                {
                    record = dx.Rows[i]["record"].ObjToString();
                    G1.update_db_table("payments", "record", record, new string[] { "agentNumber", newAgent });
                }
            }
            dgv.DataSource = dt;
            dgv.Refresh();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void PreProcessPayerActive(DataTable dt)
        {
            int lastrow = dt.Rows.Count;
            int tableRow = 0;
            string record = "";
            string contractRecord = "";
            string contract = "";
            string payer = "";
            string firstName = "";
            string lastName = "";
            string cmd = "";
            string dueNow = "";
            double amtOfMonthlyPayt = 0D;
            double annualPremium = 0D;
            string str = "";
            string detail = "";
            DataTable dx = null;

            DateTime dueDate8 = DateTime.Now;
            DateTime lastDatePaid8 = DateTime.Now;

            bool doDeceased = false;
            if (G1.get_column_number(dt, "deceasedDate") >= 0)
                doDeceased = true;

            int plnameCol = G1.get_column_number(dt, "PAYER LAST NAME");
            if (plnameCol < 0)
                plnameCol = G1.get_column_number(dt, "PLNAME");

            int pfnameCol = G1.get_column_number(dt, "PAYER FIRST NAME");
            if (pfnameCol < 0)
                pfnameCol = G1.get_column_number(dt, "PFNAME");


            try
            {
                lblTotal.Show();
                barImport.Minimum = 0;
                barImport.Maximum = lastrow;
                lblTotal.Text = "of " + lastrow.ToString();
                lblTotal.Refresh();
                int created = 0;
                int errors = 0;
                picLoader.Show();
                labelMaximum.Show();
                labelMaximum.Text = "0";
                int start = 0;
                //                lastrow = 1;
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
                    contractRecord = "";
                    try
                    {
                        payer = dt.Rows[i]["PAYER#"].ObjToString();
                        if (String.IsNullOrWhiteSpace(payer))
                            continue;
                        payer = payer.TrimStart('0');
                        firstName = dt.Rows[i][pfnameCol].ObjToString();
                        lastName = dt.Rows[i][plnameCol].ObjToString();
                        firstName = G1.protect_data(firstName);
                        lastName = G1.protect_data(lastName);
                        detail = "";

                        cmd = "Select * from `icustomers` where `payer` = '" + payer + "' AND `lastName` = '" + lastName + "' and `firstName` = '" + firstName + "';";
                        dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count > 0)
                        {
                            record = dx.Rows[0]["record"].ObjToString();
                            contract = dx.Rows[0]["contractNumber"].ObjToString();
                        }
                        else
                            dt.Rows[i]["NEW"] = "Y";

                        amtOfMonthlyPayt = dt.Rows[i]["due each month"].ObjToDouble();
                        if (amtOfMonthlyPayt == 40404.00D)
                        {
                            dt.Rows[i]["BAD"] = "Y";
                            detail = "DUE EACH MONTH,";
                        }
                        if (amtOfMonthlyPayt > 200D)
                        {
                            dt.Rows[i]["BAD"] = "Y";
                            detail = "BAD > DUE,";
                        }

                        dueNow = dt.Rows[i]["total due now"].ObjToString();

                        str = dt.Rows[i]["due date"].ObjToString();
                        if (String.IsNullOrWhiteSpace(str))
                        {
                            dt.Rows[i]["BAD"] = "Y";
                            detail += "DUE DATE,";
                        }

                        dueDate8 = str.ObjToDateTime();
                        if (dueDate8.Year < 100)
                        {
                            dt.Rows[i]["BAD"] = "Y";
                            detail += "DUE YEAR,";
                        }

                        str = dt.Rows[i]["last paid date"].ObjToString();
                        if (String.IsNullOrWhiteSpace(str))
                        {
                            dt.Rows[i]["BAD"] = "Y";
                            detail += "LPD,";
                        }

                        lastDatePaid8 = str.ObjToDateTime();
                        if (lastDatePaid8.Year < 100)
                        {
                            dt.Rows[i]["BAD"] = "Y";
                            detail += "LPD YEAR,";
                        }

                        annualPremium = dt.Rows[i]["annual amount due"].ObjToDouble();
                        if (annualPremium == 40404.00D)
                        {
                            dt.Rows[i]["BAD"] = "Y";
                            detail += "ANNUAL,";
                        }
                        dt.Rows[i]["BADDETAIL"] = detail;
                    }
                    catch (Exception ex)
                    {
                        dt.Rows[i]["num"] = "*ERROR*";
                        errors++;
                    }
                }
            }
            catch (Exception ex)
            {
            }
            picLoader.Hide();
        }
        /***********************************************************************************************/
        private void ImportPayerData(DataTable dt, string workwhat)
        {
            picLoader.Show();
            DataTable dx = null;
            string cmd = "";
            string record = "";
            string contractRecord = "";
            string payer = "";
            string contract = "";
            string firstName = "";
            string lastName = "";
            string address1 = "";
            string address2 = "";
            string address3 = "";
            string city = "";
            string state = "";
            string zip1 = "";

            string ssn = "";
            string agentCode = "";

            string deleteFlag = "";
            string areaCode = "";
            string phoneNumber = "";
            string lapsed = "";
            string lapseDate8 = "";
            string dueDate8 = "";
            string lastDatePaid8 = "";
            string amtOfMonthlyPayt = "";
            double badAmount = 0D;
            double annualPremium = 0D;
            string dueNow = "";
            string deceasedDate = "";
            DateTime date = DateTime.Now;
            double creditBalance = 0D;
            string oldloc = "";

            G1.CreateAudit("Payer Import");
            G1.WriteAudit(workwhat);



            bool doFix = false;
            bool doFix2 = false;
            bool doLapsed = false;
            bool doDeceased = false;
            if (workwhat.ToUpper() == "INSURANCE PAYER LAPSED DATA")
                doLapsed = true;
            else if (workwhat.ToUpper() == "INSURANCE PAYER DECEASED DATA")
                doDeceased = true;

            //if (workWhat.ToUpper().IndexOf("FIX") >= 0)
            //    doFix = true;
            //if (workWhat.ToUpper().IndexOf("FIX2") >= 0)
            //    doFix2 = true;
            //doFix2 = true;
            bool fixDueDate = false;

            DateTime date1 = DateTime.Now;
            DateTime date2 = DateTime.Now;
            DateTime nullDeceasedDate = new DateTime(1, 1, 1);


            labelMaximum.Show();
            labelMaximum.Text = "0";
            barImport.Show();

            int plnameCol = G1.get_column_number(dt, "PAYER LAST NAME");
            if (plnameCol < 0)
                plnameCol = G1.get_column_number(dt, "PLNAME");

            int pfnameCol = G1.get_column_number(dt, "PAYER FIRST NAME");
            if (pfnameCol < 0)
                pfnameCol = G1.get_column_number(dt, "PFNAME");

            cmd = "Select COUNT(*) from `icustomers`;";
            dx = G1.get_db_data(cmd);
            int totalCustomers = dx.Rows[0][0].ObjToInt32();

            int lastrow = dt.Rows.Count;

            int tableRow = 0;
            //            lastrow = 10; // Just for Testing
            try
            {
                lblTotal.Show();
                barImport.Minimum = 0;
                barImport.Maximum = lastrow;
                lblTotal.Text = "of " + lastrow.ToString();
                lblTotal.Refresh();
                int created = 0;
                int possible = 0;
                int errors = 0;
                int deceasedCount = 0;
                bool justCreated = false;
                picLoader.Show();
                int start = 0;
                //                lastrow = 1;
                //                start = 9000;

                for (int i = start; i < lastrow; i++)
                {
                    Application.DoEvents();

                    picLoader.Refresh();
                    barImport.Value = i;
                    barImport.Refresh();
                    labelMaximum.Text = i.ToString();
                    labelMaximum.Refresh();

                    tableRow = i;
                    record = "";
                    contractRecord = "";
                    try
                    {
                        justCreated = false;
                        deceasedDate = "";
                        if (doDeceased)
                            deceasedDate = GetSQLDate(dt, i, "deceasedDate");

                        payer = dt.Rows[i]["PAYER#"].ObjToString();
                        if (String.IsNullOrWhiteSpace(payer))
                            continue;
                        payer = payer.TrimStart('0');
                        firstName = dt.Rows[i][pfnameCol].ObjToString();
                        lastName = dt.Rows[i][plnameCol].ObjToString();
                        firstName = G1.protect_data(firstName);
                        lastName = G1.protect_data(lastName);
                        oldloc = dt.Rows[i]["OLDLOC"].ObjToString();

                        if (payer == "790140")
                        {
                        }

                        cmd = "Select * from `icustomers` where `payer` = '" + payer + "' AND `lastName` = '" + lastName + "' and `firstName` = '" + firstName + "' ORDER BY `contractNumber` DESC;";
                        dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count > 0)
                        {
                            record = dx.Rows[0]["record"].ObjToString();
                            contract = dx.Rows[0]["contractNumber"].ObjToString();
                        }
                        else
                        {
                            if (fixDueDate)
                            {
                                possible++;
                                G1.WriteAudit("Create Payer Row " + i.ToString() + " payer " + payer + " " + lastName + " " + firstName + "!");
                                //continue;
                            }
                            //if (doFix || doFix2 )
                            //    continue;
                            totalCustomers++;
                            contract = "ZZ" + totalCustomers.ToString("D7");
                            record = G1.create_record("icustomers", "contractNumber", "-1");
                            if (G1.BadRecord("icustomers", record))
                                continue;
                            justCreated = true;
                            created++;
                            G1.WriteAudit("Create Payer Row " + i.ToString() + " payer " + payer + " " + lastName + " " + firstName + "!");
                        }

                        //if ( !doFix && !doFix2 && !fixDueDate)
                        if (justCreated)
                            G1.update_db_table("icustomers", "record", record, new string[] { "contractNumber", contract, "payer", payer, "firstName", firstName, "lastName", lastName });

                        cmd = "Select * from `icontracts` where `contractNumber` = '" + contract + "';";
                        dx = G1.get_db_data(cmd);
                        //if ( fixDueDate)
                        //{
                        //    if (dx.Rows.Count <= 0)
                        //        continue;
                        //    contractRecord = dx.Rows[0]["record"].ObjToString();
                        //    //date = dx.Rows[0]["dueDate8"].ObjToDateTime();
                        //    dueDate8 = GetSQLDate(dt, i, "due date");
                        //    date = dueDate8.ObjToDateTime();
                        //    annualPremium = dt.Rows[i]["annual amount due"].ObjToDouble();
                        //    if (annualPremium > 1000D)
                        //        annualPremium = 0D;
                        //    amtOfMonthlyPayt = dt.Rows[i]["due each month"].ObjToString();
                        //    badAmount = amtOfMonthlyPayt.ObjToDouble();
                        //    if (badAmount >= 500D)
                        //        amtOfMonthlyPayt = "0.00";
                        //    creditBalance = dt.Rows[i]["over"].ObjToDouble();
                        //    if (date.Year > 1900)
                        //        G1.update_db_table("icontracts", "record", contractRecord, new string[] { "dueDate8", dueDate8 });
                        //    G1.update_db_table("icontracts", "record", contractRecord, new string[] { "annualPremium", annualPremium.ToString(), "deceasedDate", nullDeceasedDate.ToString("MM/dd/yyyy"), "lapsed", "", "creditBalance", creditBalance.ToString(), "amtOfMonthlyPayt", amtOfMonthlyPayt.ToString()});
                        //    G1.update_db_table("icustomers", "record", record, new string[] { "lapsed","", "deceasedDate", nullDeceasedDate.ToString("MM/dd/yyyy") });

                        //    if (doLapsed)
                        //    {
                        //        G1.update_db_table("icontracts", "record", contractRecord, new string[] { "lapsed", "Y" });
                        //        G1.update_db_table("icustomers", "record", record, new string[] { "lapsed", "Y" });
                        //    }
                        //    if ( payer == "UC-2186")
                        //    {
                        //    }
                        //    continue;
                        //}
                        //if (1 == 1)
                        //    continue;

                        //if ( doFix2 && dx.Rows.Count > 0 )
                        //{
                        //    contractRecord = dx.Rows[0]["record"].ObjToString();
                        //    date = dx.Rows[0]["dueDate8"].ObjToDateTime();
                        //        dueDate8 = GetSQLDate(dt, i, "DDUE8");
                        //        date = dueDate8.ObjToDateTime();
                        //        if (date.Year > 1950)
                        //            G1.update_db_table("icontracts", "record", contractRecord, new string[] { "dueDate8", dueDate8 });

                        //    //amtOfMonthlyPayt = dt.Rows[i]["DMON"].ObjToString();
                        //    //badAmount = dx.Rows[0]["amtOfMonthlyPayt"].ObjToDouble();
                        //    //if (badAmount == 40404.00D )
                        //    //    G1.update_db_table("icontracts", "record", contractRecord, new string[] { "amtOfMonthlyPayt", amtOfMonthlyPayt });
                        //    //else if (badAmount == 2084.00D)
                        //    //    G1.update_db_table("icontracts", "record", contractRecord, new string[] { "amtOfMonthlyPayt", amtOfMonthlyPayt });
                        //    //else if (badAmount >= 500.00D)
                        //    //    G1.update_db_table("icontracts", "record", contractRecord, new string[] { "amtOfMonthlyPayt", amtOfMonthlyPayt });

                        //    //date = dx.Rows[0]["lastPaidDate8"].ObjToDateTime();
                        //    //if (date.Year < 1900)
                        //    //{
                        //    //    dueDate8 = GetSQLDate(dt, i, "LPAID8");
                        //    //    date = dueDate8.ObjToDateTime();
                        //    //    if (date.Year > 1900)
                        //    //        G1.update_db_table("icontracts", "record", contractRecord, new string[] { "lastPaidDate8", dueDate8 });
                        //    //}
                        //    continue;
                        //}
                        //if (1 == 1)
                        //    continue;
                        //if ( doFix2 )
                        //{
                        //    if (dx.Rows.Count <= 0)
                        //        continue;
                        //    contractRecord = dx.Rows[0]["record"].ObjToString();
                        //    contract = dx.Rows[0]["contractNumber"].ObjToString();
                        //    date = dx.Rows[0]["deceasedDate"].ObjToDateTime();
                        //    amtOfMonthlyPayt = dt.Rows[i]["due each month"].ObjToString();
                        //    badAmount = dx.Rows[0]["amtOfMonthlyPayt"].ObjToDouble();
                        //    if (badAmount == 40404.00D)
                        //        G1.update_db_table("icontracts", "record", contractRecord, new string[] { "amtOfMonthlyPayt", amtOfMonthlyPayt });
                        //    if (date.Year < 20)
                        //        continue;
                        //    if ( date.Year == 1910)
                        //        G1.update_db_table("icontracts", "record", contractRecord, new string[] { "deceasedDate", "0000-00-00" });
                        //}
                        //if (doFix)
                        //{
                        //    if (dx.Rows.Count <= 0)
                        //        continue;
                        //    contractRecord = dx.Rows[0]["record"].ObjToString();
                        //    contract = dx.Rows[0]["contractNumber"].ObjToString();
                        //    amtOfMonthlyPayt = dt.Rows[i]["due each month"].ObjToString();
                        //    dueNow = dt.Rows[i]["total due now"].ObjToString();
                        //    G1.update_db_table("icontracts", "record", contractRecord, new string[] { "amtOfMonthlyPayt", amtOfMonthlyPayt, "balanceDue", dueNow, "nowDue", dueNow });
                        //    continue;
                        //}
                        if (dx.Rows.Count > 0)
                        {
                            contractRecord = dx.Rows[0]["record"].ObjToString();
                            if (doDeceased)
                            {
                                date1 = dx.Rows[0]["deceasedDate"].ObjToDateTime();
                                date2 = deceasedDate.ObjToDateTime();
                                if (date1.Year > 1911 && date2.Year <= 1911)
                                {
                                    if (date1 > date2)
                                        deceasedDate = date1.ToString("yyyy-MM-dd");
                                    //deceasedDate = "";
                                }
                            }
                        }
                        else
                        {
                            contractRecord = G1.create_record("icontracts", "contractNumber", "-1");
                            if (G1.BadRecord("icontracts", contractRecord))
                                continue;
                            G1.update_db_table("icontracts", "record", contractRecord, new string[] { "contractNumber", contract });
                            G1.WriteAudit("Create Contract Row " + i.ToString() + " Contract " + contract + " " + lastName + " " + firstName + "!");
                        }

                        address1 = dt.Rows[i]["address 1"].ObjToString();
                        address2 = dt.Rows[i]["address 2"].ObjToString();
                        address3 = dt.Rows[i]["address 3"].ObjToString();
                        if (!String.IsNullOrWhiteSpace(address2))
                            address2 += " ";
                        address2 += address3;

                        city = dt.Rows[i]["city"].ObjToString();
                        state = dt.Rows[i]["state"].ObjToString();
                        zip1 = dt.Rows[i]["zip1"].ObjToString();

                        ssn = dt.Rows[i]["soc sec no."].ObjToString();

                        agentCode = dt.Rows[i]["AGENT"].ObjToString();
                        deleteFlag = dt.Rows[i]["delete indicator"].ObjToString();
                        lapsed = dt.Rows[i]["lapsed indicator"].ObjToString();

                        areaCode = dt.Rows[i]["area code"].ObjToString();
                        phoneNumber = dt.Rows[i]["phone first 3"].ObjToString() + dt.Rows[i]["phone last 4"].ObjToString();
                        if (!String.IsNullOrWhiteSpace(areaCode))
                            phoneNumber = "(" + areaCode + ") " + phoneNumber;

                        dueDate8 = GetSQLDate(dt, i, "due date");
                        date = dueDate8.ObjToDateTime();

                        lastDatePaid8 = GetSQLDate(dt, i, "last paid date");
                        lastDatePaid8 = lastDatePaid8.Replace("-", "");

                        dueNow = dt.Rows[i]["total due now"].ObjToString();
                        badAmount = dueNow.ObjToDouble();
                        if (badAmount >= 500D)
                            dueNow = "0.00";

                        amtOfMonthlyPayt = dt.Rows[i]["due each month"].ObjToString();
                        badAmount = amtOfMonthlyPayt.ObjToDouble();
                        if (badAmount >= 500D)
                            amtOfMonthlyPayt = dueNow;

                        annualPremium = dt.Rows[i]["annual amount due"].ObjToDouble();
                        if (annualPremium > 1000D)
                            annualPremium = 0D;

                        creditBalance = dt.Rows[i]["over"].ObjToDouble();

                        lapsed = "";
                        lapseDate8 = nullDeceasedDate.ToString("MM/dd/yyyy");
                        if (doLapsed)
                        {
                            lapsed = "Y";
                            lapseDate8 = dueDate8;
                        }

                        if (!doDeceased)
                        {
                            G1.update_db_table("icustomers", "record", record, new string[] { "address1", address1, "address2", address2, "city", city, "state", state,
                        "zip1", zip1, "ssn", ssn, "agentCode", agentCode, "deleteFlag", deleteFlag, "coverageType", "ZZ",
                        "areaCode", areaCode, "phoneNumber", phoneNumber, "phoneNumber1", phoneNumber, "lapsed", lapsed, "deceasedDate", nullDeceasedDate.ToString("MM/dd/yyyy"), "oldloc", oldloc });
                        }

                        if (doDeceased && !String.IsNullOrWhiteSpace(deceasedDate))
                            G1.update_db_table("icustomers", "record", record, new string[] { "deceasedDate", deceasedDate });

                        if (!doDeceased)
                        {
                            G1.update_db_table("icontracts", "record", contractRecord, new string[] { "amtOfMonthlyPayt", amtOfMonthlyPayt, "balanceDue", dueNow, "nowDue", dueNow, "creditBalance", creditBalance.ToString() });
                            G1.update_db_table("icontracts", "record", contractRecord, new string[] { "lastDatePaid8", dueDate8, "dueDate8", dueDate8, "issueDate8", lastDatePaid8, "lapsed", lapsed, "lapseDate8", lapseDate8, "annualPremium", annualPremium.ToString(), "deceasedDate", nullDeceasedDate.ToString("MM/dd/yyyy") });
                        }
                        if (doLapsed)
                            G1.update_db_table("icontracts", "record", contractRecord, new string[] { "lapsed", "Y", "lapseDate8", dueDate8 });

                        if (doDeceased && !String.IsNullOrWhiteSpace(deceasedDate))
                        {
                            deceasedCount++;
                            G1.update_db_table("icontracts", "record", contractRecord, new string[] { "deceasedDate", deceasedDate });
                        }
                        if ((i % 500) == 0)
                            GC.Collect();
                    }
                    catch (Exception ex)
                    {
                        dt.Rows[i]["num"] = "*ERROR*";
                        errors++;
                    }
                    //                    picLoader.Refresh();
                }
                picLoader.Hide();
                barImport.Value = lastrow;
                MessageBox.Show("Payer Data Import of " + lastrow + " Rows Complete Created = " + created.ToString() + " Errors = " + errors.ToString() + " Possible = " + possible.ToString() + " . . .");
            }
            catch (Exception ex)
            {
                picLoader.Hide();
                MessageBox.Show("***ERROR*** Creating Payer Record/Row! " + contract + "/" + tableRow.ToString() + " Stopping! " + ex.Message.ObjToString());
            }
        }
        /***********************************************************************************************/
        private void PreprocessPolicyData(DataTable dt)
        {
            picLoader.Show();
            DataTable dx = null;
            string cmd = "";
            string record = "";
            string payer = "";
            string contract = "";
            string firstName = "";
            string lastName = "";
            string ssn = "";
            string agentCode = "";
            DateTime date = DateTime.Now;

            string lapsed = "";
            bool doDeceased = false;
            string deceasedDate = "";

            string deleteFlag = "";
            string pCode = "";

            string birthDate = "";
            string issueDate8 = "";
            string premium = "";

            string beneficiary = "";
            string liability = "";
            string companyCode = "";
            string issueAge = "";
            string type = "";
            string oldAgentInfo = "";
            string groupNumber = "";
            string policyFirstName = "";
            string policyLastName = "";
            string policyNumber = "";
            string detail = "";
            bool orphan = false;
            int irecord = 0;
            bool fix = false;
            //            fix = true;

            labelMaximum.Show();
            labelMaximum.Text = "0";
            barImport.Show();

            int lastrow = dt.Rows.Count;
            //            lastrow = 1000;

            int tableRow = 0;
            //            lastrow = 1; // Just for Testing
            int wrongCount = 0;
            int intCount = 0;
            try
            {
                lblTotal.Show();
                barImport.Minimum = 0;
                barImport.Maximum = lastrow;
                lblTotal.Text = "of " + lastrow.ToString();
                lblTotal.Refresh();
                int created = 0;
                int errors = 0;
                picLoader.Show();
                int start = 0;
                //                start = 22314;

                for (int i = start; i < lastrow; i++)
                {
                    picLoader.Refresh();
                    barImport.Value = i;
                    barImport.Refresh();
                    labelMaximum.Text = i.ToString();
                    labelMaximum.Refresh();

                    tableRow = i;
                    record = "";
                    string newPolicy = "";
                    try
                    {
                        policyNumber = dt.Rows[i]["policy#"].ObjToString();
                        policyNumber = policyNumber.TrimStart('0');
                        if (String.IsNullOrWhiteSpace(policyNumber))
                            continue;
                        if (policyNumber == "11,627.845")
                        {

                        }
                        //if (fix)
                        //{
                        //    newPolicy = policyNumber.TrimStart('0');
                        //    if (newPolicy != policyNumber && !String.IsNullOrWhiteSpace(newPolicy))
                        //    {
                        //        cmd = "Select * from `policies` where `policyNumber` = '" + newPolicy + "';";
                        //        dx = G1.get_db_data(cmd);
                        //        if (dx.Rows.Count > 0)
                        //        {
                        //            record = dx.Rows[0]["record"].ObjToString();
                        //            cmd = "policyNumber," + policyNumber;
                        //            G1.update_db_table("policies", "record", record, cmd);
                        //            intCount++;
                        //            continue;
                        //        }
                        //        else
                        //        {
                        //            policyFirstName = dt.Rows[i]["policy first name"].ObjToString();
                        //            policyFirstName = G1.protect_data(policyFirstName);
                        //            policyLastName = dt.Rows[i]["policy last name"].ObjToString();
                        //            policyLastName = G1.protect_data(policyLastName);
                        //            cmd = "Select * from `policies` where `policyNumber` = '" + policyNumber + "' AND `policyLastName` = '" + policyLastName + "' and `policyFirstName` = '" + policyFirstName + "';";
                        //            dx = G1.get_db_data(cmd);
                        //            if (dx.Rows.Count > 0)
                        //                continue;
                        //            wrongCount++;
                        //        }
                        //    }
                        //    else
                        //    {
                        //        policyFirstName = dt.Rows[i]["policy first name"].ObjToString();
                        //        policyFirstName = G1.protect_data(policyFirstName);
                        //        policyLastName = dt.Rows[i]["policy last name"].ObjToString();
                        //        policyLastName = G1.protect_data(policyLastName);
                        //        cmd = "Select * from `policies` where `policyNumber` = '" + policyNumber + "' AND `policyLastName` = '" + policyLastName + "' and `policyFirstName` = '" + policyFirstName + "';";
                        //        dx = G1.get_db_data(cmd);
                        //        if (dx.Rows.Count <= 0)
                        //            wrongCount++;
                        //        else
                        //            continue; // Don't bother with those that are already in the Database
                        //    }
                        //    //continue;
                        //}

                        payer = dt.Rows[i]["PAYER#"].ObjToString();
                        payer = payer.TrimStart('0');
                        if (String.IsNullOrWhiteSpace(payer))
                        {
                            dt.Rows[i]["BAD"] = "Y";
                            dt.Rows[i]["BADDETAIL"] = "EMPTY PAYER";
                            continue;
                        }
                        //if (payer.Trim() != "CC-843")
                        //    continue;

                        firstName = dt.Rows[i]["PAYER FIRST NAME"].ObjToString();
                        lastName = dt.Rows[i]["PAYER LAST NAME"].ObjToString();
                        firstName = G1.protect_data(firstName);
                        lastName = G1.protect_data(lastName);

                        orphan = false;

                        cmd = "Select * from `icustomers` where `payer` = '" + payer + "' AND `lastName` = '" + lastName + "' and `firstName` = '" + firstName + "';";
                        dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count > 0)
                        {
                            record = dx.Rows[0]["record"].ObjToString();
                            contract = dx.Rows[0]["contractNumber"].ObjToString();
                        }
                        else
                        {
                            //if (fix)
                            //    continue;
                            //G1.WriteAudit("Orphan ERROR Row " + i.ToString() + " Cannot find payer " + payer + " " + lastName + " " + firstName + "!");

                            orphan = true;
                            dt.Rows[i]["num"] = "*ERROR*";
                            errors++;
                        }

                        // policyNumber = dt.Rows[i]["policy#"].ObjToString();
                        policyFirstName = dt.Rows[i]["policy first name"].ObjToString();
                        policyFirstName = G1.protect_data(policyFirstName);
                        policyLastName = dt.Rows[i]["policy last name"].ObjToString();
                        policyLastName = G1.protect_data(policyLastName);
                        cmd = "Select * from `policies` where `payer` = '" + payer + "' AND `policyNumber` = '" + policyNumber + "' AND `policyLastName` = '" + policyLastName + "' and `policyFirstName` = '" + policyFirstName + "';";
                        dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count > 0)
                        {
                            record = dx.Rows[0]["record"].ObjToString();
                            //if (fix)
                            //    continue;
                            //if (fix)
                            //{
                            //    date = dx.Rows[0]["deceasedDate"].ObjToDateTime();
                            //    if (date.Year < 20)
                            //        continue;
                            //    G1.update_db_table("policies", "record", record, new string[] { "deceasedDate", "0000-00-00" });
                            //    continue;
                            //}
                        }
                        else
                        {
                            dt.Rows[i]["NEW"] = "Y";
                            //if (fix)
                            //    continue;
                            //record = G1.create_record("policies", "contractNumber", "-1");
                            //if (G1.BadRecord("policies", record))
                            //    continue;
                            //created++;
                            //if (orphan)
                            //{
                            //    irecord = record.ObjToInt32();
                            //    contract = "OO" + irecord.ToString("D6");
                            //}
                        }

                        detail = "";
                        if (orphan)
                            detail = "O,";

                        //G1.update_db_table("policies", "record", record, new string[] { "contractNumber", contract, "payer", payer, "firstName", firstName, "lastName", lastName, "policyNumber", policyNumber, "policyFirstName", policyFirstName, "policyLastName", policyLastName });

                        ssn = dt.Rows[i]["policy soc sec no"].ObjToString();

                        agentCode = dt.Rows[i]["AGENT"].ObjToString();

                        deleteFlag = dt.Rows[i]["delete"].ObjToString();
                        pCode = dt.Rows[i]["pcode"].ObjToString();

                        birthDate = GetSQLDate(dt, i, "birth date");
                        issueDate8 = GetSQLDate(dt, i, "issue date");
                        premium = dt.Rows[i]["premium amount"].ObjToString();
                        if (premium.ObjToDouble() > 1000D)
                        {
                            dt.Rows[i]["BAD"] = "Y";
                            detail += "Premium,";
                        }

                        beneficiary = dt.Rows[i]["beneficiary"].ObjToString();
                        liability = dt.Rows[i]["liability"].ObjToString();
                        companyCode = dt.Rows[i]["company code"].ObjToString();
                        issueAge = dt.Rows[i]["issue age"].ObjToString();
                        type = dt.Rows[i]["type"].ObjToString();
                        oldAgentInfo = dt.Rows[i]["old agent info"].ObjToString();
                        groupNumber = dt.Rows[i]["group#"].ObjToString();
                        dt.Rows[i]["BADDETAIL"] = detail;

                        //G1.update_db_table("policies", "record", record, new string[] { "ssn", ssn, "deleteFlag", deleteFlag, "pCode", pCode, "birthDate", birthDate,
                        //"issueDate8", issueDate8, "premium", premium, "beneficiary", beneficiary, "liability", liability, "companyCode", companyCode, "issueAge", issueAge,
                        //"type", type, "oldAgentInfo", oldAgentInfo, "groupNumber", groupNumber, "lapsed", lapsed });
                        //if (doDeceased)
                        //    G1.update_db_table("policies", "record", record, new string[] { "deceasedDate", deceasedDate });
                    }
                    catch (Exception ex)
                    {
                        G1.WriteAudit("*ERROR* Row " + i.ToString() + " " + ex.Message.ToString());

                        dt.Rows[i]["num"] = "*ERROR*";
                    }
                    //                    picLoader.Refresh();
                }
                picLoader.Hide();
                barImport.Value = lastrow;
                MessageBox.Show("Policy Data Import of " + lastrow + " Rows Complete Created = " + created.ToString() + " Errors = " + errors.ToString() + " . . .");
            }
            catch (Exception ex)
            {
                G1.WriteAudit("***ERROR*** Row " + tableRow.ToString() + " " + ex.Message.ToString());
                picLoader.Hide();
                MessageBox.Show("***ERROR*** Creating Policy Record/Row! " + contract + "/" + tableRow.ToString() + " Stopping! " + ex.Message.ObjToString());
            }
        }
        /***********************************************************************************************/
        private void ImportPolicyData(DataTable dt, string workwhat)
        {
            picLoader.Show();
            DataTable dx = null;
            string cmd = "";
            string record = "";
            string payer = "";
            string contract = "";
            string firstName = "";
            string lastName = "";
            string ssn = "";
            string agentCode = "";
            DateTime date = DateTime.Now;

            string lapsed = "";
            bool doDeceased = false;
            string deceasedDate = "";
            if (workwhat.ToUpper().IndexOf("LAPSED") >= 0)
                lapsed = "Y";
            else if (workwhat.ToUpper().IndexOf("DECEASED") >= 0)
            {
                doDeceased = true;
                deceasedDate = "12/31/1910";
            }


            string deleteFlag = "";
            string pCode = "";

            string birthDate = "";
            string issueDate8 = "";
            string premium = "";

            string beneficiary = "";
            string liability = "";
            string companyCode = "";
            string issueAge = "";
            string type = "";
            string oldAgentInfo = "";
            string groupNumber = "";
            string policyFirstName = "";
            string policyLastName = "";
            string policyNumber = "";
            bool orphan = false;
            int irecord = 0;
            string record2 = "";
            bool fix = false;
            //fix = true;

            DateTime date1 = DateTime.Now;
            DateTime date2 = DateTime.Now;

            if (lapsed == "Y")
                G1.CreateAudit("PolicyLapsed");
            else if (doDeceased)
                G1.CreateAudit("PolicyDeceased");
            else
                G1.CreateAudit("PolicyActive");
            G1.WriteAudit(workwhat);


            labelMaximum.Show();
            labelMaximum.Text = "0";
            barImport.Show();

            DateTime nullDate = new DateTime(1, 1, 1);

            int lastrow = dt.Rows.Count;
            //            lastrow = 1000;

            int tableRow = 0;
            //            lastrow = 1; // Just for Testing
            int wrongCount = 0;
            int intCount = 0;
            bool found = false;
            DataTable ddx = null;
            try
            {
                lblTotal.Show();
                barImport.Minimum = 0;
                barImport.Maximum = lastrow;
                lblTotal.Text = "of " + lastrow.ToString();
                lblTotal.Refresh();
                int created = 0;
                int errors = 0;
                picLoader.Show();
                int start = 0;
                //                start = 22314;

                for (int i = start; i < lastrow; i++)
                {
                    Application.DoEvents();

                    picLoader.Refresh();
                    barImport.Value = i;
                    barImport.Refresh();
                    labelMaximum.Text = i.ToString();
                    labelMaximum.Refresh();

                    tableRow = i;
                    record = "";
                    string newPolicy = "";
                    try
                    {
                        policyNumber = dt.Rows[i]["policy#"].ObjToString();
                        policyNumber = policyNumber.TrimStart('0');
                        if (String.IsNullOrWhiteSpace(policyNumber))
                            continue;
                        if (policyNumber == "11,627.845")
                        {

                        }
                        //if (fix)
                        //{
                        //    newPolicy = policyNumber.TrimStart('0');
                        //    if (newPolicy != policyNumber && !String.IsNullOrWhiteSpace ( newPolicy))
                        //    {
                        //        cmd = "Select * from `policies` where `policyNumber` = '" + newPolicy + "';";
                        //        dx = G1.get_db_data(cmd);
                        //        if (dx.Rows.Count > 0)
                        //        {
                        //            record = dx.Rows[0]["record"].ObjToString();
                        //            cmd = "policyNumber," + policyNumber;
                        //            G1.update_db_table("policies", "record", record, cmd);
                        //            intCount++;
                        //            continue;
                        //        }
                        //        else
                        //        {
                        //            policyFirstName = dt.Rows[i]["policy first name"].ObjToString();
                        //            policyFirstName = G1.protect_data(policyFirstName);
                        //            policyLastName = dt.Rows[i]["policy last name"].ObjToString();
                        //            policyLastName = G1.protect_data(policyLastName);
                        //            cmd = "Select * from `policies` where `policyNumber` = '" + policyNumber + "' AND `policyLastName` = '" + policyLastName + "' and `policyFirstName` = '" + policyFirstName + "';";
                        //            dx = G1.get_db_data(cmd);
                        //            if (dx.Rows.Count > 0)
                        //                continue;
                        //            wrongCount++;
                        //        }
                        //    }
                        //    else
                        //    {
                        //        policyFirstName = dt.Rows[i]["policy first name"].ObjToString();
                        //        policyFirstName = G1.protect_data(policyFirstName);
                        //        policyLastName = dt.Rows[i]["policy last name"].ObjToString();
                        //        policyLastName = G1.protect_data(policyLastName);
                        //        cmd = "Select * from `policies` where `policyNumber` = '" + policyNumber + "' AND `policyLastName` = '" + policyLastName + "' and `policyFirstName` = '" + policyFirstName + "';";
                        //        dx = G1.get_db_data(cmd);
                        //        if (dx.Rows.Count <= 0)
                        //            wrongCount++;
                        //        else
                        //            continue; // Don't bother with those that are already in the Database
                        //    }
                        //    //continue;
                        //}

                        payer = dt.Rows[i]["PAYER#"].ObjToString();
                        payer = payer.TrimStart('0');
                        //if (payer.Trim() != "CC-843")
                        //    continue;

                        firstName = dt.Rows[i]["PAYER FIRST NAME"].ObjToString();
                        lastName = dt.Rows[i]["PAYER LAST NAME"].ObjToString();
                        firstName = G1.protect_data(firstName);
                        lastName = G1.protect_data(lastName);
                        if (String.IsNullOrWhiteSpace(payer))
                        {
                            G1.WriteAudit("Empty Payer ERROR Row " + i.ToString() + " Empty find payer " + payer + " " + lastName + " " + firstName + "!");
                            continue;
                        }

                        orphan = false;
                        found = false;

                        cmd = "Select * from `icustomers` where `payer` = '" + payer + "' AND `lastName` = '" + lastName + "' and `firstName` = '" + firstName + "';";
                        dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count > 0)
                        {
                            record = dx.Rows[0]["record"].ObjToString();
                            contract = dx.Rows[0]["contractNumber"].ObjToString();
                        }
                        else
                        {
                            G1.WriteAudit("Orphan ERROR Row " + i.ToString() + " Cannot find payer " + payer + " " + lastName + " " + firstName + "!");

                            orphan = true;
                            dt.Rows[i]["num"] = "*ERROR*";
                            errors++;
                        }

                        //policyNumber = dt.Rows[i]["policy#"].ObjToString();
                        policyFirstName = dt.Rows[i]["policy first name"].ObjToString();
                        policyFirstName = G1.protect_data(policyFirstName);
                        policyLastName = dt.Rows[i]["policy last name"].ObjToString();
                        policyLastName = G1.protect_data(policyLastName);
                        cmd = "Select * from `policies` where `payer` = '" + payer + "' AND `policyNumber` = '" + policyNumber + "' AND `policyLastName` = '" + policyLastName + "' and `policyFirstName` = '" + policyFirstName + "';";
                        dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count > 0)
                        {
                            record = dx.Rows[0]["record"].ObjToString();
                            found = true;
                            if (orphan)
                            {
                            }
                        }
                        else
                        {
                            record = G1.create_record("policies", "contractNumber", "-1");
                            if (G1.BadRecord("policies", record))
                                continue;
                            created++;
                            if (orphan)
                            {
                                irecord = record.ObjToInt32();
                                contract = "OO" + irecord.ToString("D6");
                            }
                        }

                        if (!doDeceased)
                            G1.update_db_table("policies", "record", record, new string[] { "contractNumber", contract, "payer", payer, "firstName", firstName, "lastName", lastName, "policyNumber", policyNumber, "policyFirstName", policyFirstName, "policyLastName", policyLastName });
                        else if (!found)
                            G1.update_db_table("policies", "record", record, new string[] { "contractNumber", contract, "payer", payer, "firstName", firstName, "lastName", lastName, "policyNumber", policyNumber, "policyFirstName", policyFirstName, "policyLastName", policyLastName });

                        ssn = dt.Rows[i]["policy soc sec no"].ObjToString();

                        agentCode = dt.Rows[i]["agent"].ObjToString();

                        deleteFlag = dt.Rows[i]["delete"].ObjToString();
                        pCode = dt.Rows[i]["pcode"].ObjToString();

                        birthDate = GetSQLDate(dt, i, "birth date");
                        issueDate8 = GetSQLDate(dt, i, "issue date");
                        premium = dt.Rows[i]["premium amount"].ObjToString();

                        beneficiary = dt.Rows[i]["beneficiary"].ObjToString();
                        liability = dt.Rows[i]["liability"].ObjToString();
                        companyCode = dt.Rows[i]["company code"].ObjToString();
                        issueAge = dt.Rows[i]["issue age"].ObjToString();
                        type = dt.Rows[i]["type"].ObjToString();
                        oldAgentInfo = dt.Rows[i]["old agent info"].ObjToString();
                        groupNumber = dt.Rows[i]["group#"].ObjToString();

                        if (!doDeceased)
                        {
                            if (String.IsNullOrWhiteSpace(lapsed))
                            {
                                G1.update_db_table("policies", "record", record, new string[] { "ssn", ssn, "deleteFlag", deleteFlag, "pCode", pCode, "birthDate", birthDate,
                        "issueDate8", issueDate8, "premium", premium, "beneficiary", beneficiary, "liability", liability, "companyCode", companyCode, "issueAge", issueAge,
                        "type", type, "oldAgentInfo", oldAgentInfo, "groupNumber", groupNumber, "lapsed", lapsed, "lapsedDate8", nullDate.ToString("yyyy-MM-dd"), "historicPremium", premium, "agentCode", agentCode });
                            }
                            else
                            {
                                G1.update_db_table("policies", "record", record, new string[] { "ssn", ssn, "deleteFlag", deleteFlag, "pCode", pCode, "birthDate", birthDate,
                        "issueDate8", issueDate8, "premium", premium, "beneficiary", beneficiary, "liability", liability, "companyCode", companyCode, "issueAge", issueAge,
                        "type", type, "oldAgentInfo", oldAgentInfo, "groupNumber", groupNumber, "lapsed", lapsed, "historicPremium", premium, "agentCode", agentCode });
                            }
                        }
                        else if (!found)
                        {
                            G1.update_db_table("policies", "record", record, new string[] { "ssn", ssn, "deleteFlag", deleteFlag, "pCode", pCode, "birthDate", birthDate,
                        "issueDate8", issueDate8, "premium", premium, "beneficiary", beneficiary, "liability", liability, "companyCode", companyCode, "issueAge", issueAge,
                        "type", type, "oldAgentInfo", oldAgentInfo, "groupNumber", groupNumber, "lapsed", lapsed, "historicPremium", premium, "agentCode", agentCode });
                        }
                        if (doDeceased)
                            G1.update_db_table("policies", "record", record, new string[] { "deceasedDate", deceasedDate });
                        if ((i % 500) == 0)
                            GC.Collect();
                    }
                    catch (Exception ex)
                    {
                        G1.WriteAudit("*ERROR* Row " + i.ToString() + " " + ex.Message.ToString());

                        dt.Rows[i]["num"] = "*ERROR*";
                    }
                    //                    picLoader.Refresh();
                }
                picLoader.Hide();
                barImport.Value = lastrow;
                MessageBox.Show("Policy Data Import of " + lastrow + " Rows Complete Created = " + created.ToString() + " Errors = " + errors.ToString() + " . . .");
            }
            catch (Exception ex)
            {
                G1.WriteAudit("***ERROR*** Row " + tableRow.ToString() + " " + ex.Message.ToString());
                picLoader.Hide();
                MessageBox.Show("***ERROR*** Creating Policy Record/Row! " + contract + "/" + tableRow.ToString() + " Stopping! " + ex.Message.ObjToString());
            }
        }
        /***********************************************************************************************/
        private void ImportPolicyDeceased(DataTable dt, string workwhat)
        { // BAD?
            picLoader.Show();
            DataTable dx = null;
            string cmd = "";
            string record = "";
            string payer = "";
            string contract = "";
            string firstName = "";
            string lastName = "";
            string ssn = "";
            string agentCode = "";

            string deleteFlag = "";
            string pCode = "";

            string birthDate = "";
            string issueDate8 = "";
            string premium = "";

            string beneficiary = "";
            string liability = "";
            string companyCode = "";
            string issueAge = "";
            string type = "";
            string oldAgentInfo = "";
            string groupNumber = "";
            string policyFirstName = "";
            string policyLastName = "";
            string policyNumber = "";

            labelMaximum.Show();
            labelMaximum.Text = "0";
            barImport.Show();

            int lastrow = dt.Rows.Count;

            int tableRow = 0;
            //            lastrow = 2; // Just for Testing
            try
            {
                lblTotal.Show();
                barImport.Minimum = 0;
                barImport.Maximum = lastrow;
                lblTotal.Text = "of " + lastrow.ToString();
                lblTotal.Refresh();
                int created = 0;
                int errors = 0;
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
                        payer = dt.Rows[i]["PAYER#"].ObjToString();
                        //if (payer.Trim() != "CC-843")
                        //    continue;

                        firstName = dt.Rows[i]["PFNAME"].ObjToString();
                        lastName = dt.Rows[i]["PLNAME"].ObjToString();
                        firstName = G1.protect_data(firstName);
                        lastName = G1.protect_data(lastName);

                        cmd = "Select * from `icustomers` where `payer` = '" + payer + "' AND `lastName` = '" + lastName + "' and `firstName` = '" + firstName + "';";
                        dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count > 0)
                        {
                            record = dx.Rows[0]["record"].ObjToString();
                            contract = dx.Rows[0]["contractNumber"].ObjToString();
                            continue;
                        }
                        else
                        {
                            dt.Rows[i]["num"] = "*ERROR*";
                            errors++;
                            continue;
                        }

                        policyNumber = dt.Rows[i]["POLICY#"].ObjToString();
                        policyFirstName = dt.Rows[i]["policy first name"].ObjToString();
                        policyLastName = dt.Rows[i]["policy last name"].ObjToString();
                        cmd = "Select * from `policies` where `payer` = '" + payer + "' AND `policyNumber` = '" + policyNumber + "' AND `policyLastName` = '" + policyLastName + "' and `policyFirstName` = '" + policyFirstName + "';";
                        dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count > 0)
                            record = dx.Rows[0]["record"].ObjToString();
                        else
                        {
                            record = G1.create_record("policies", "contractNumber", "-1");
                            if (G1.BadRecord("policies", record))
                                continue;
                            created++;
                        }

                        G1.update_db_table("policies", "record", record, new string[] { "contractNumber", contract, "payer", payer, "firstName", firstName, "lastName", lastName, "policyNumber", policyNumber, "policyFirstName", policyFirstName, "policyLastName", policyLastName });

                        ssn = dt.Rows[i]["policy soc sec no"].ObjToString();

                        agentCode = dt.Rows[i]["AGENT"].ObjToString();

                        deleteFlag = dt.Rows[i]["delete"].ObjToString();
                        pCode = dt.Rows[i]["pcode"].ObjToString();

                        birthDate = GetSQLDate(dt, i, "birth date");
                        issueDate8 = GetSQLDate(dt, i, "issue date");
                        premium = dt.Rows[i]["premium amount"].ObjToString();

                        beneficiary = dt.Rows[i]["beneficiary"].ObjToString();
                        liability = dt.Rows[i]["liability"].ObjToString();
                        companyCode = dt.Rows[i]["company code"].ObjToString();
                        issueAge = dt.Rows[i]["issue age"].ObjToString();
                        type = dt.Rows[i]["type"].ObjToString();
                        oldAgentInfo = dt.Rows[i]["old agent info"].ObjToString();
                        groupNumber = dt.Rows[i]["group#"].ObjToString();

                        G1.update_db_table("policies", "record", record, new string[] { "ssn", ssn, "deleteFlag", deleteFlag, "pCode", pCode, "birthDate", birthDate,
                        "issueDate8", issueDate8, "premium", premium, "beneficiary", beneficiary, "liability", liability, "companyCode", companyCode, "issueAge", issueAge,
                        "type", type, "oldAgentInfo", oldAgentInfo, "groupNumber", groupNumber });
                    }
                    catch (Exception ex)
                    {
                        dt.Rows[i]["num"] = "*ERROR*";
                    }
                    //                    picLoader.Refresh();
                }
                picLoader.Hide();
                barImport.Value = lastrow;
                MessageBox.Show("Policy Data Import of " + lastrow + " Rows Complete Created = " + created.ToString() + " Errors = " + errors.ToString() + " . . .");
            }
            catch (Exception ex)
            {
                picLoader.Hide();
                MessageBox.Show("***ERROR*** Creating Policy Record/Row! " + contract + "/" + tableRow.ToString() + " Stopping! " + ex.Message.ObjToString());
            }
        }
        /***********************************************************************************************/
        private void ImportPaymentData(DataTable dt, string workwhat)
        {
            picLoader.Show();
            DataTable dx = null;
            //string file = actualFile.Trim();
            //int idx = actualFile.IndexOf("_file");
            //if (idx < 0)
            //    return;
            //file = actualFile.Substring(idx + 5).Trim();
            //idx = file.IndexOf(" ");
            //if (idx < 0)
            //    return;
            //file = file.Substring(0, idx).Trim();

            MessageBox.Show("***ERROR*** This is old version!");
            if (1 == 1)
                return;

            G1.CreateAudit("Import Insurance Payments");
            G1.WriteAudit(workwhat);

            string cmd = "";
            string record = "";
            string payer = "";
            string oldpayer = "";
            string contract = "";
            string firstName = "";
            string lastName = "";

            string policyFirstName = "";
            string policyLastName = "";
            string policyNumber = "";

            string amountKeyed = "";
            string payment = "";
            string debit = "";
            string credit = "";
            string totalMonths = "";
            string transactionCode = "";
            string batchNumber = "";
            string agent = "";
            string depositNumber = "";
            string user = "";
            string dateKeyed = "";

            DataView tempview = dt.DefaultView;
            tempview.Sort = "PAYER# asc";
            dt = tempview.ToTable();


            labelMaximum.Show();
            labelMaximum.Text = "0";
            barImport.Show();

            int lastrow = dt.Rows.Count;

            int tableRow = 0;
            //            lastrow = 1; // Just for Testing
            try
            {
                lblTotal.Show();
                barImport.Minimum = 0;
                barImport.Maximum = lastrow;
                lblTotal.Text = "of " + lastrow.ToString();
                lblTotal.Refresh();
                int created = 0;
                int errors = 0;
                int majorError = 0;
                picLoader.Show();
                int start = 0;
                //                start = 200771; // Don't forget to reset this

                for (int i = start; i < lastrow; i++)
                {
                    Application.DoEvents();

                    picLoader.Refresh();
                    barImport.Value = i;
                    barImport.Refresh();
                    labelMaximum.Text = i.ToString();
                    labelMaximum.Refresh();

                    tableRow = i;
                    record = "";
                    try
                    {
                        payer = dt.Rows[i]["PAYER#"].ObjToString();
                        payer = payer.TrimStart('0');
                        if (String.IsNullOrWhiteSpace(payer))
                            continue;
                        //if (payer.Trim() != "CC-843")
                        //    continue;
                        firstName = dt.Rows[i]["PAYER FIRST NAME"].ObjToString();
                        lastName = dt.Rows[i]["PAYER LAST NAME"].ObjToString();
                        firstName = G1.protect_data(firstName);
                        lastName = G1.protect_data(lastName);

                        if (payer != oldpayer)
                        {
                            cmd = "Select * from `icustomers` where `payer` = '" + payer + "' AND `lastName` = '" + lastName + "' and `firstName` = '" + firstName + "';";
                            dx = G1.get_db_data(cmd);
                            if (dx.Rows.Count > 0)
                            {
                                record = dx.Rows[0]["record"].ObjToString();
                                contract = dx.Rows[0]["contractNumber"].ObjToString();
                            }
                            else
                            {
                                dt.Rows[i]["num"] = "*ERROR*";
                                errors++;
                                contract = CreateNewContract(payer, lastName, firstName, i);
                                if (String.IsNullOrWhiteSpace(contract))
                                {
                                    majorError++;
                                    continue;
                                }
                            }
                            oldpayer = payer;
                        }
                        if (String.IsNullOrWhiteSpace(contract))
                        {
                            continue;
                        }

                        batchNumber = dt.Rows[i]["batch number"].ObjToString();
                        transactionCode = dt.Rows[i]["transaction code"].ObjToString().Trim();
                        amountKeyed = dt.Rows[i]["amount keyed"].ObjToString();
                        agent = dt.Rows[i]["agent at time keyed"].ObjToString();
                        totalMonths = dt.Rows[i]["total months"].ObjToString();
                        depositNumber = dt.Rows[i]["deposit number"].ObjToString();
                        user = dt.Rows[i]["user"].ObjToString();
                        dateKeyed = dt.Rows[i]["date keyed"].ObjToString();
                        if (!G1.validate_numeric(dateKeyed))
                            dateKeyed = "2019-11-29";

                        //cmd = "Select * from `ipayments` where `contractNumber` = '" + contract + "';";
                        //dx = G1.get_db_data(cmd);

                        //DataRow [] dR = dx.Select("s50='" + dateKeyed + "'");
                        //if (dR.Length > 0)
                        //    record = dR[0]["record"].ObjToString();
                        //else
                        //{
                        record = G1.create_record("ipayments", "contractNumber", "-1");
                        if (G1.BadRecord("ipayments", record))
                            continue;
                        created++;
                        //                        }
                        payment = "";
                        debit = "";
                        credit = "";
                        if (transactionCode == "01")
                            payment = amountKeyed;
                        else if (transactionCode == "1")
                            payment = amountKeyed;
                        else if (transactionCode == "98")
                            debit = amountKeyed;
                        else if (transactionCode == "99")
                            credit = amountKeyed;

                        G1.update_db_table("ipayments", "record", record, new string[] { "contractNumber", contract, "payDate8", dateKeyed, "agentNumber", agent, "paymentAmount", payment, "debitAdjustment", debit, "creditAdjustment", credit, "userId", user, "depositNumber", depositNumber, "new", batchNumber, "firstName", firstName, "lastName", lastName });
                        if ((i % 500) == 0)
                            GC.Collect();
                    }
                    catch (Exception ex)
                    {
                        dt.Rows[i]["num"] = "*ERROR*";
                    }
                }
                picLoader.Hide();
                barImport.Value = lastrow;
                MessageBox.Show("Payment Data Import of " + lastrow + " Rows Complete Created = " + created.ToString() + " Errors = " + errors.ToString() + " . . .");
            }
            catch (Exception ex)
            {
                picLoader.Hide();
                MessageBox.Show("***ERROR*** Creating Payment Record/Row! " + contract + "/" + tableRow.ToString() + " Stopping! " + ex.Message.ObjToString());
            }
        }
        /***********************************************************************************************/
        public static string CreateNewContract(string payer, string lastName, string firstName, int row)
        {
            string contractNumber = "";
            string cmd = "Select * from `payers` where `payer` = '" + payer + "';";
            DataTable ddx = G1.get_db_data(cmd); // Try existing payers file first
            if (ddx.Rows.Count > 0)
            {
                contractNumber = ddx.Rows[0]["contractNumber"].ObjToString();
                return contractNumber;
            }
            //string cmd = "SELECT * from `icustomers` where `payer` = '" + payer + "' and `lastName` = '" + lastName + "';";
            //DataTable dx = G1.get_db_data(cmd);
            //if ( dx.Rows.Count > 0 )
            //{
            //    contractNumber = dx.Rows[0]["contractNumber"].ObjToString();
            //    return contractNumber;
            //}
            string record = G1.create_record("icustomers", "contractNumber", "-1");
            if (G1.BadRecord("icustomers", record))
                return "";
            string customerRecord = record;
            int irecord = record.ObjToInt32();
            contractNumber = "MM" + irecord.ToString("D6");

            int count = 0;
            for (; ; )
            {
                cmd = "select * from `icustomers` where `contractNumber` = '" + contractNumber + "';";
                ddx = G1.get_db_data(cmd);
                if (ddx.Rows.Count <= 0)
                    break;
                contractNumber += "x";
                count++;
            }
            G1.update_db_table("icustomers", "record", customerRecord, new string[] { "payer", payer, "firstName", firstName, "lastName", lastName, "contractNumber", contractNumber });

            string contractRecord = G1.create_record("icontracts", "contractNumber", "-1");
            if (!G1.BadRecord("icontracts", contractRecord))
                G1.update_db_table("icontracts", "record", contractRecord, new string[] { "contractNumber", contractNumber });

            G1.WriteAudit("Create Contract Row " + row.ToString() + " Contract " + contractNumber + " " + lastName + " " + firstName + "!");
            return contractNumber;
        }
        /***************************************************************************************/
        public delegate void d_void_eventdone_dt(DataTable dt);
        public event d_void_eventdone_dt SelectDone;
        protected void OnSelectDone(DataTable dt)
        {
            SelectDone?.Invoke(dt);
        }
        /***********************************************************************************************/
        private void generateCouponBookFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            TrustCoupons couponForm = new TrustCoupons(dt);
            couponForm.Show();
        }
        /***********************************************************************************************/
        private void ImportPayerTest(DataTable dt, string workwhat, string actualFile)
        {
            picLoader.Show();
            DataTable dx = null;
            string cmd = "";
            string record = "";
            string contractRecord = "";
            string payer = "";
            string contract = "";
            string firstName = "";
            string lastName = "";
            string address1 = "";
            string address2 = "";
            string address3 = "";
            string city = "";
            string state = "";
            string zip1 = "";

            string ssn = "";
            string agentCode = "";

            string deleteFlag = "";
            string areaCode = "";
            string phoneNumber = "";
            string lapsed = "";
            string dueDate8 = "";
            string lastDatePaid8 = "";
            string amtOfMonthlyPayt = "";
            double badAmount = 0D;
            double annualPremium = 0D;
            string dueNow = "";
            string deceasedDate = "";
            DateTime date = DateTime.Now;
            double creditBalance = 0D;

            G1.CreateAudit("Payer Import");
            G1.WriteAudit(workwhat);



            bool doFix = false;
            bool doFix2 = false;
            bool doLapsed = false;
            bool doDeceased = false;
            if (workwhat.ToUpper() == "INSURANCE PAYER LAPSED DATA")
                doLapsed = true;
            else if (workwhat.ToUpper() == "INSURANCE PAYER DECEASED DATA")
                doDeceased = true;

            if (actualFile.ToUpper().IndexOf("PAYER LAPSED") >= 0)
                doLapsed = true;
            if (actualFile.ToUpper().IndexOf("PAYER DECEASED") >= 0)
                doDeceased = true;

            string updateType = "A";
            if (doLapsed)
                updateType = "L";
            if (doDeceased)
                updateType = "D";

            //if (workWhat.ToUpper().IndexOf("FIX") >= 0)
            //    doFix = true;
            //if (workWhat.ToUpper().IndexOf("FIX2") >= 0)
            //    doFix2 = true;
            //doFix2 = true;
            bool fixDueDate = true;

            DateTime date1 = DateTime.Now;
            DateTime date2 = DateTime.Now;
            DateTime nullDeceasedDate = new DateTime(1, 1, 1);


            labelMaximum.Show();
            labelMaximum.Text = "0";
            barImport.Show();

            int plnameCol = G1.get_column_number(dt, "PAYER LAST NAME");
            if (plnameCol < 0)
                plnameCol = G1.get_column_number(dt, "PLNAME");

            int pfnameCol = G1.get_column_number(dt, "PAYER FIRST NAME");
            if (pfnameCol < 0)
                pfnameCol = G1.get_column_number(dt, "PFNAME");

            //cmd = "Select COUNT(*) from `icustomers`;";
            //dx = G1.get_db_data(cmd);
            //int totalCustomers = dx.Rows[0][0].ObjToInt32();

            int lastrow = dt.Rows.Count;

            int tableRow = 0;
            //            lastrow = 10; // Just for Testing
            try
            {
                lblTotal.Show();
                barImport.Minimum = 0;
                barImport.Maximum = lastrow;
                lblTotal.Text = "of " + lastrow.ToString();
                lblTotal.Refresh();
                int created = 0;
                int possible = 0;
                int errors = 0;
                bool justCreated = false;
                picLoader.Show();
                int start = 0;
                string contractNumber = "";
                string data = "";
                //                lastrow = 1;
                //                start = 9000;

                for (int i = start; i < lastrow; i++)
                {
                    Application.DoEvents();
                    picLoader.Refresh();
                    barImport.Value = i;
                    barImport.Refresh();
                    labelMaximum.Text = i.ToString();
                    labelMaximum.Refresh();

                    tableRow = i;
                    record = "";
                    contractRecord = "";
                    try
                    {
                        justCreated = false;

                        payer = dt.Rows[i]["PAYER#"].ObjToString();
                        if (String.IsNullOrWhiteSpace(payer))
                            continue;
                        payer = payer.TrimStart('0');
                        firstName = dt.Rows[i][pfnameCol].ObjToString();
                        lastName = dt.Rows[i][plnameCol].ObjToString();
                        firstName = G1.protect_data(firstName);
                        lastName = G1.protect_data(lastName);

                        contractNumber = FindPayerContract(payer, firstName, lastName, ref record);
                        if (String.IsNullOrWhiteSpace(contractNumber))
                        {
                            G1.WriteAudit("Cannot Find Payer Row " + i.ToString() + " payer " + payer + " " + lastName + " " + firstName + "!");
                            continue;
                        }

                        data = "touched," + updateType;
                        G1.update_db_table("icustomers", "record", record, data);

                        if ((i % 500) == 0)
                            GC.Collect();
                    }
                    catch (Exception ex)
                    {
                        dt.Rows[i]["num"] = "*ERROR*";
                        errors++;
                    }
                }
                picLoader.Hide();
                barImport.Value = lastrow;
                MessageBox.Show("Payer Data Import of " + lastrow + " Rows Complete Created = " + created.ToString() + " Errors = " + errors.ToString() + " Possible = " + possible.ToString() + " . . .");
            }
            catch (Exception ex)
            {
                picLoader.Hide();
                MessageBox.Show("***ERROR*** Creating Payer Record/Row! " + contract + "/" + tableRow.ToString() + " Stopping! " + ex.Message.ObjToString());
            }
        }
        /***********************************************************************************************/
        public static string FindPayerContract(string payer, string firstName, string lastName, ref string record)
        {
            string contractNumber = "";
            record = "";
            string cmd = "";
            payer = payer.TrimStart('0');

            payer = payer.ToUpper().Replace("NEW", "");
            payer = payer.ToUpper().Replace("INSURANCE", "");
            payer = payer.Replace(" ", "");
            payer = payer.Trim();


            //            cmd = "Select * from `icustomers` where `payer` = '" + payer + "' AND `lastName` = '" + lastName + "' and `firstName` = '" + firstName + "' ORDER BY `contractNumber` DESC;";
            cmd = "Select * from `icustomers` where `payer` = '" + payer + "' ORDER BY `contractNumber` DESC;";
            DataTable ddx = G1.get_db_data(cmd);
            if (ddx.Rows.Count <= 0)
                return "";

            string list = "";
            for (int i = 0; i < ddx.Rows.Count; i++)
            {
                string contract = ddx.Rows[i]["contractNumber"].ObjToString();
                list += "'" + contract + "',";
            }
            list = list.TrimEnd(',');
            list = "(" + list + ")";

            cmd = "Select * from `ipayments` where `contractNumber` IN " + list + " order by `payDate8` DESC, `tmstamp` DESC;";
            ddx = G1.get_db_data(cmd);
            if (ddx.Rows.Count <= 0)
                return "";

            contractNumber = ddx.Rows[0]["contractNumber"].ObjToString();

            cmd = "Select * from `icustomers` where `contractNumber` = '" + contractNumber + "';";
            ddx = G1.get_db_data(cmd);
            if (ddx.Rows.Count <= 0)
                return "";
            string customerRecord = ddx.Rows[0]["record"].ObjToString();

            cmd = "Select * from `icontracts` where `contractNumber` = '" + contractNumber + "';";
            ddx = G1.get_db_data(cmd);
            if (ddx.Rows.Count <= 0)
                return "";
            record = customerRecord;
            return contractNumber;
        }
        /***********************************************************************************************/
        private void mainGrid_CellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            if (e == null)
                return;
            if (workWhat.ToUpper() != "NEWCONTRACTS")
                return;

            //DataTable dt = (DataTable)dgv.DataSource;
            //if (G1.get_column_number(dt, "ccFee") < 0)
            //    dt.Columns.Add("ccFee", Type.GetType("System.Double"));

            int rowHandle = mainGrid.FocusedRowHandle;
            DataRow dr = mainGrid.GetFocusedDataRow();
            if (dr == null)
                return;

            if (e.Column.FieldName.ToUpper() == "DEPOSIT #") // Ramma Zamma
            {
                string depositNumber = dr["deposit #"].ObjToString();
                if (String.IsNullOrWhiteSpace(depositNumber))
                    return;
                string trust = "";
                string loc = "";
                string trustNumber = dr["TRUST_NUMBER"].ObjToString();
                string miniContract = Trust85.decodeContractNumber(trustNumber, ref trust, ref loc);
                dr["trustLocation"] = loc;

                DateTime dateDpPaid = dr["dateDpPaid"].ObjToDateTime();
                string cmd = "Select * from `downpayments` WHERE `depositNumber` = '" + depositNumber + "' ";
                if (dateDpPaid.Year > 1000)
                    cmd += " AND `date` = '" + dateDpPaid.ToString("yyyy-MM-dd") + "' ";
                cmd += ";";
                DataTable dx = G1.get_db_data(cmd);
                if (dx.Rows.Count <= 0)
                {
                    string str = "***QUESTION*** Deposit Number " + depositNumber + " DOES NOT Exist\nin the Down Payments Table!\n";
                    str += "Do you want to keep it anyway?";
                    DialogResult result = MessageBox.Show(str, "Deposit Number Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (result == DialogResult.No)
                    {
                        dr["deposit #"] = "";
                        dr["bankAccount"] = "";
                        dr["ccFee"] = 0D;
                    }
                    return;
                }
                string downPayments = "";
                bool found = false;
                string bankAccount = "";
                string multiDepositNumbers = "";
                string multiDPs = "";
                decimal originalDownPayment = 0;
                decimal totalDeposit = 0;
                decimal downPayment = dr["DOWN_PAYMENT"].ObjToDecimal();
                decimal lr = 0;
                decimal cf = 0;
                for (int i = 0; i < dx.Rows.Count; i++)
                {
                    originalDownPayment = dx.Rows[i]["downPayment"].ObjToDecimal();
                    totalDeposit = dx.Rows[i]["totalDeposit"].ObjToDecimal();
                    lr = dx.Rows[i]["lossRecoveryFee"].ObjToDecimal();
                    cf = dx.Rows[i]["ccFee"].ObjToDecimal();
                    if (downPayment == (totalDeposit - lr - cf))
                    {
                        found = true;
                        dr["trustDpRecord"] = dx.Rows[i]["record"].ObjToString();
                        bankAccount = dx.Rows[i]["bankAccount"].ObjToString();
                        dr["bankAccount"] = bankAccount;
                        double ccFee = dx.Rows[i]["ccFee"].ObjToDouble();
                        dr["ccFee"] = ccFee;
                        DateTime depositDate = dx.Rows[i]["date"].ObjToDateTime();
                        dr["dateDpPaid"] = G1.DTtoMySQLDT(depositDate);
                        break;
                    }
                    else
                    {
                        downPayments += "Manual Down Payment = " + originalDownPayment.ToString() + "\n";
                    }
                }
                if (!found)
                {
                    double ccFee = 0D;
                    string trustDpRecord = "";
                    bool fountIt = CheckForMultipleDPs(dx, ref originalDownPayment, ref bankAccount, ref multiDepositNumbers, ref multiDPs, ref ccFee, ref trustDpRecord);
                    if (downPayment == originalDownPayment)
                    {
                        found = true;
                        dr["bankAccount"] = bankAccount;
                        dr["ccFee"] = ccFee;
                        dr["trustDpRecord"] = trustDpRecord;
                        found = true;
                    }
                }
                if (!found)
                {
                    DialogResult result = MessageBox.Show("***WARNING*** " + downPayments + "Does Not Match this FDLIC down payment of " + downPayment.ToString() + "!", "Down Payment Dialog", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    dr["deposit #"] = "";
                    dr["bankAccount"] = "";
                    dr["ccFee"] = 0D;
                    return;
                }
            }

            mainGrid.RefreshData();
        }
        /***********************************************************************************************/
        private void GetDPDetail(int row)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = dt.Rows[row];

            string depositNumber = dr["deposit #"].ObjToString();
            if (String.IsNullOrWhiteSpace(depositNumber))
                return;
            string trust = "";
            string loc = "";
            string trustNumber = dr["TRUST_NUMBER"].ObjToString();
            string dp = dr["DOWN_PAYMENT"].ObjToString();
            string miniContract = Trust85.decodeContractNumber(trustNumber, ref trust, ref loc);
            dr["trustLocation"] = loc;

            DateTime dateDpPaid = dr["dateDpPaid"].ObjToDateTime();
            string cmd = "Select * from `downpayments` WHERE `depositNumber` = '" + depositNumber + "' AND `downPayment` = '" + dp + "' ";
            if (dateDpPaid.Year > 1000)
                cmd += " AND `date` = '" + dateDpPaid.ToString("yyyy-MM-dd") + "' ";
            cmd += ";";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
            {
                string str = "***QUESTION*** Deposit Number " + depositNumber + " DOES NOT Exist\nin the Down Payments Table!\n";
                str += "Do you want to keep it anyway?";
                DialogResult result = MessageBox.Show(str, "Deposit Number Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.No)
                {
                    dr["deposit #"] = "";
                    dr["bankAccount"] = "";
                    dr["ccFee"] = 0D;
                }
                return;
            }
            string downPayments = "";
            bool found = false;
            string bankAccount = "";
            string multiDepositNumbers = "";
            string multiDPs = "";
            decimal originalDownPayment = 0;
            decimal totalDeposit = 0;
            decimal downPayment = dr["DOWN_PAYMENT"].ObjToDecimal();
            decimal lr = 0;
            decimal cf = 0;
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                originalDownPayment = dx.Rows[i]["downPayment"].ObjToDecimal();
                totalDeposit = dx.Rows[i]["totalDeposit"].ObjToDecimal();
                lr = dx.Rows[i]["lossRecoveryFee"].ObjToDecimal();
                cf = dx.Rows[i]["ccFee"].ObjToDecimal();
                if (downPayment == (totalDeposit - lr - cf))
                {
                    found = true;
                    dr["trustDpRecord"] = dx.Rows[i]["record"].ObjToString();
                    bankAccount = dx.Rows[i]["bankAccount"].ObjToString();
                    dr["bankAccount"] = bankAccount;
                    double ccFee = dx.Rows[i]["ccFee"].ObjToDouble();
                    dr["ccFee"] = ccFee;
                    break;
                }
                else
                {
                    downPayments += "Manual Down Payment = " + originalDownPayment.ToString() + "\n";
                }
            }
            if ( found )
            {
            }
            else if (!found)
            {
                double ccFee = 0D;
                string trustDpRecord = "";
                bool fountIt = CheckForMultipleDPs(dx, ref originalDownPayment, ref bankAccount, ref multiDepositNumbers, ref multiDPs, ref ccFee, ref trustDpRecord);
                if (downPayment == originalDownPayment)
                {
                    found = true;
                    dr["bankAccount"] = bankAccount;
                    dr["ccFee"] = ccFee;
                    dr["trustDpRecord"] = trustDpRecord;
                    found = true;
                }
            }
            if (!found)
            {
                DialogResult result = MessageBox.Show("***WARNING*** " + downPayments + "Does Not Match this FDLIC down payment of " + downPayment.ToString() + "!", "Down Payment Dialog", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                dr["deposit #"] = "";
                dr["bankAccount"] = "";
                dr["ccFee"] = 0D;
                return;
            }
            mainGrid.RefreshData();
        }
    /***********************************************************************************************/
    private bool CheckForMultipleDPs  (DataTable dx, ref decimal downPayment, ref string bankAccount, ref string multiDepositNumbers, ref string multiDPs, ref double ccFee, ref string trustDpRecord )
        {
            downPayment = 0;
            if (dx == null)
                return false;
            if (dx.Rows.Count <= 0)
                return false;
            bool found = false;

            multiDepositNumbers = "";
            multiDPs = "";
            ccFee = 0D;
            trustDpRecord = "";

            string depositNumber = "";

            string firstName = dx.Rows[0]["firstName"].ObjToString();
            string lastName = dx.Rows[0]["lastName"].ObjToString();
            DateTime dateDpPaid = dx.Rows[0]["date"].ObjToDateTime();
            DateTime date1 = dateDpPaid.AddDays(-10);
            DateTime date2 = dateDpPaid.AddDays(7);
            string cmd = "Select * from `downpayments` WHERE ";
            //cmd += " `date` = '" + dateDpPaid.ToString("yyyy-MM-dd") + "' ";
            cmd += " `date` >= '" + date1.ToString("yyyy-MM-dd") + "' AND `date` <= '" + date2.ToString("yyyy-MM-dd") + "' ";
            cmd += " AND `firstName` = '" + firstName + "' AND `lastName` = '" + lastName + "' ";
            cmd += ";";
            DataTable dd = G1.get_db_data(cmd);
            decimal dP = 0;
            for ( int i=0; i<dd.Rows.Count; i++)
            {
                found = true;
                dP = dd.Rows[i]["downPayment"].ObjToDecimal();
                downPayment += dP;
                bankAccount = dd.Rows[i]["bankAccount"].ObjToString();

                depositNumber = dd.Rows[i]["depositNumber"].ObjToString();
                multiDepositNumbers += depositNumber + "~";
                multiDPs += dP.ToString() + "~";

                ccFee += dd.Rows[i]["ccFee"].ObjToDouble();
                trustDpRecord = dd.Rows[i]["record"].ObjToString();

            }
            if (!String.IsNullOrWhiteSpace(multiDepositNumbers))
                multiDepositNumbers = multiDepositNumbers.TrimEnd('~');
            if (!String.IsNullOrWhiteSpace(multiDPs))
                multiDPs = multiDPs.TrimEnd('~');
            return found;
        }
        /***********************************************************************************************/
        private bool ValidBankAccounts ( DataTable dt)
        {
            string bankAccount = "";
            int count = 0;
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                bankAccount = dt.Rows[i]["bankAccount"].ObjToString();
                if (String.IsNullOrWhiteSpace(bankAccount))
                    count++;
            }
            bool valid = true;
            if ( count > 0 )
            {
                DialogResult result = MessageBox.Show("***WARNING*** Some Bank Accounts have been left UNASSIGNED!\nDo you want to continue ANYWAY?", "Bank Accounts Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.No)
                    valid = false;
            }
            return valid;
        }
        /***********************************************************************************************/
        private void deleteRowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = mainGrid.GetFocusedDataRow();

            dt.Rows.Remove(dr);
            dgv.RefreshDataSource();
            dgv.Refresh();
        }
        /***********************************************************************************************/
        private void locateDownpaymentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (workWhat.ToUpper() != "NEWCONTRACTS")
                return;

            DataTable dt = (DataTable)dgv.DataSource;
            int rowHandle = mainGrid.FocusedRowHandle;
            viewRow = mainGrid.GetDataSourceRowIndex(rowHandle);
            DataRow dr = mainGrid.GetFocusedDataRow();
            if (dr == null)
                return;

            string firstName = dr["INSURED_FIRST_NAME"].ObjToString();
            string middleName = dr["INSURED_MIDDLE_INITIAL"].ObjToString();
            string lastName = dr["INSURED_LAST_NAME"].ObjToString();
            string suffix = dr["INSURED_SUFFIX"].ObjToString();
            double downPayment = dr["DOWN_PAYMENT"].ObjToDouble();

            string fullName = firstName + " " + middleName + " " + lastName;
            if (!String.IsNullOrWhiteSpace(suffix))
                fullName += ", " + suffix;

            string sPayment = G1.ReformatMoney(downPayment);
            sPayment = sPayment.Replace(",", "");

            try
            {
                DateTime dateDpPaid = dr["trust_seq_date"].ObjToDateTime();
                if ( dateDpPaid.Year < 100 )
                {
                    dateDpPaid = dr["SIGNED_DATE"].ObjToDateTime();
                    if ( dateDpPaid.Year <= 100 )
                    {
                        MessageBox.Show("*** ERROR *** Cannot determine the Down Payment Date!", "Down Payment Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                        return;
                    }
                }
                DateTime date1 = dateDpPaid.AddDays(-14);
                DateTime date2 = dateDpPaid.AddDays(14);
                string cmd = "Select * from `downpayments` WHERE ";
                cmd += " `date` >= '" + date1.ToString("yyyy-MM-dd") + "' AND `date` <= '" + date2.ToString("yyyy-MM-dd") + "' ";
                cmd += " AND `firstName` = '" + firstName + "' AND `lastName` = '" + lastName + "' AND `downPayment` = '" + sPayment + "' ";
                cmd += ";";
                DataTable dd = G1.get_db_data(cmd);
                if (dd.Rows.Count <= 0)
                {
                    cmd = "Select * from `downpayments` WHERE ";
                    cmd += " `date` >= '" + date1.ToString("yyyy-MM-dd") + "' AND `date` <= '" + date2.ToString("yyyy-MM-dd") + "' ";
                    cmd += " AND `lastName` = '" + lastName + "' AND `downPayment` = '" + sPayment + "' ";
                    cmd += ";";

                    dd = G1.get_db_data(cmd);
                }
                else if ( dd.Rows.Count > 0 )
                {
                    cmd = "Select * from `downpayments` WHERE ";
                    cmd += " `date` >= '" + date1.ToString("yyyy-MM-dd") + "' AND `date` <= '" + date2.ToString("yyyy-MM-dd") + "' ";
                    cmd += " AND `lastName` = '" + lastName + "' ";
                    cmd += ";";

                    DataTable ddd = G1.get_db_data(cmd);
                    if (ddd.Rows.Count > 0)
                        dd = ddd.Copy();
                }
                if (dd.Rows.Count > 0)
                {
                    sPayment = G1.ReformatMoney(downPayment);

                    viewForm = new ViewDataTable(dd, true, "date,downPayment,ccfee,firstName,lastName,depositNumber,location,(record),(bankAccount)");
                    viewForm.Text = "Looking for downPayments between " + date1 + " to " + date2 + " for " + fullName + " for $" + sPayment;
                    viewForm.TopMost = true;
                    viewForm.ManualDone += ViewForm_ManualDone;
                    viewForm.Show();
                }
            }
            catch ( Exception ex)
            {
            }
        }
        /***********************************************************************************************/
        ViewDataTable viewForm = null;
        private int viewRow = -1;
        private void ViewForm_ManualDone( DataTable dd, DataRow dx)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (dx == null)
            {
                string depositNumber = "";
                DateTime date = DateTime.Now;
                double totalDP = 0D;
                double totalccFee = 0D;
                double downPayment = 0D;
                double dp = 0D;
                double ccFee = 0D;
                string record = "";
                string bankAccount = "";
                string cmd = "";
                string select = "";
                DataTable ddx = null;
                if (G1.get_column_number(dd, "SelectedRow") < 0)
                    return;
                try
                {
                    if (viewForm != null)
                    {
                        viewForm.Hide();
                        viewForm.Close();
                        viewForm = null;
                    }
                    downPayment = dt.Rows[viewRow]["down_Payment"].ObjToDouble();
                    date = dt.Rows[viewRow].ObjToDateTime();
                    for (int i = 0; i < dd.Rows.Count; i++)
                    {
                        select = dd.Rows[i]["SelectedRow"].ObjToString();
                        if (select != "Y")
                            continue;
                        record = dd.Rows[i]["record"].ObjToString();
                        //downPayment = dd.Rows[i]["downPayment"].ObjToDouble();
                        cmd = "Select * from `downPayments` WHERE `record` = '" + record + "';";
                        ddx = G1.get_db_data(cmd);
                        if (ddx.Rows.Count <= 0)
                            continue;
                        dp = ddx.Rows[0]["downPayment"].ObjToDouble();
                        depositNumber = ddx.Rows[0]["depositNumber"].ObjToString();
                        if (dp != downPayment)
                        {
                            G1.copy_dt_row(dt, viewRow, dt, dt.Rows.Count);
                            if (G1.get_column_number(dt, "duplicate") < 0)
                                dt.Columns.Add("duplicate");
                            int row = dt.Rows.Count - 1;
                            dt.Rows[row]["DOWN_PAYMENT"] = dp.ToString();
                            dt.Rows[row]["ccFee"] = ddx.Rows[0]["ccFee"].ObjToString();
                            dt.Rows[row]["bankAccount"] = ddx.Rows[0]["bankAccount"].ObjToString();
                            dt.Rows[row]["deposit #"] = depositNumber;
                            dt.Rows[row]["dateDPPaid"] = ddx.Rows[0]["date"].ObjToDateTime().ToString("MM/dd/yyyy");
                            dt.Rows[row]["duplicate"] = "Y";
                            dt.Rows[row]["trustlocation"] = ddx.Rows[0]["location"].ObjToString();
                            dgv.DataSource = dt;
                            dgv.RefreshDataSource();
                            dgv.Refresh();
                            GetDPDetail(row);
                        }
                        else
                        {
                            dt.Rows[viewRow]["dateDPPaid"] = ddx.Rows[0]["date"].ObjToDateTime().ToString("MM/dd/yyyy");
                            dt.Rows[viewRow]["deposit #"] = depositNumber;
                            GetDPDetail(viewRow);
                        }
                    }
                }
                catch (Exception ex)
                {
                }

                //if (viewForm != null)
                //{
                //    try
                //    {
                //        viewForm.Hide();

                //        dt.Rows[viewRow]["dateDPPaid"] = date.ToString("MM/dd/yyyy");
                //        dt.Rows[viewRow]["deposit #"] = depositNumber;

                //        viewForm.Close();
                //        viewForm = null;

                //        if (viewRow >= 0)
                //            GetDPDetail(viewRow);
                //        viewRow = -1;
                //    }
                //    catch (Exception ex)
                //    {
                //    }
                //}
            }
            else
            {
                DateTime date = dx["date"].ObjToDateTime();
                string depositNumber = dx["depositNumber"].ObjToString();
                if (viewForm != null)
                {
                    try
                    {
                        viewForm.Hide();
                        dt = (DataTable)dgv.DataSource;
                        dt.Rows[viewRow]["dateDPPaid"] = date.ToString("MM/dd/yyyy");
                        dt.Rows[viewRow]["deposit #"] = depositNumber;
                        viewForm.Close();
                        viewForm = null;

                        if (viewRow >= 0)
                            GetDPDetail(viewRow);
                        viewRow = -1;
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
        }
        /***********************************************************************************************/
        private void gridMain2_DoubleClick(object sender, EventArgs e)
        {
            if (workWhat.ToUpper() != "NEWCONTRACTS")
                return;
            DataRow dr = gridMain2.GetFocusedDataRow();
            string contract = dr["contractNumber"].ObjToString();
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
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            TabControl tPage = (TabControl)sender;

            if ( tPage.SelectedIndex == 0 )
            {
                btnImportFile.Show();
                btnImportFile.Refresh();
            }
            else if ( tPage.SelectedIndex == 1 )
            {
                btnImportFile.Hide();
            }
        }
        /***********************************************************************************************/
        private void gridMain2_CustomDrawCell(object sender, RowCellCustomDrawEventArgs e)
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
        private void gridMain2_CustomColumnDisplayText(object sender, CustomColumnDisplayTextEventArgs e)
        {
            if (e.ListSourceRowIndex == DevExpress.XtraGrid.GridControl.InvalidRowHandle)
                return;
            string name = e.Column.FieldName;
            if (name.ToUpper().IndexOf("INSURED_SSN") >= 0)
            {
                DataTable dt = (DataTable)dgv2.DataSource;
                if (e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
                {
                    int row = e.ListSourceRowIndex;
                    string ssn = dt.Rows[row]["INSURED_SSN"].ObjToString();
                    if (ssn.Trim().Length >= 9)
                    {
                        try
                        {
                            ssn = "XXX-XX-" + ssn.Substring(5, 4);
                            e.DisplayText = ssn;
                        }
                        catch ( Exception ex)
                        {
                        }
                    }
                }
            }
        }
        /***********************************************************************************************/
    }
}
