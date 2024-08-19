using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeneralLib;

using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Xml;
using System.Threading;
using MySql.Data;
using System.Text.RegularExpressions;

/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    class Passare
    {
        /***********************************************************************************************/
        public static void SetupPassare()
        {
            //SMFS.mainSystemToolStripMenuItem.Checked = false;
            //SMFS.menuStrip1.BackColor = Color.LightBlue;

            G1.conn1.Close();
            //G1.CloseConnection();
            G1.oldCopy = true;
            G1.conn1.Open();

            string database = G1.conn1.Database.ObjToString();

            G1.oldCopy = true;
            G1.OpenConnection(G1.conn1);
            database = G1.conn1.Database.ObjToString();
        }
        /***********************************************************************************************/
        public static void SetupMain()
        {
            //SMFS.mainSystemToolStripMenuItem.Checked = false;
            //SMFS.menuStrip1.BackColor = Color.LightBlue;

            G1.conn1.Close();
            //G1.CloseConnection();
            G1.oldCopy = false;
            G1.conn1.Open();

            string database = G1.conn1.Database.ObjToString();

            G1.oldCopy = false;
            G1.OpenConnection(G1.conn1);
            database = G1.conn1.Database.ObjToString();
        }
        /***********************************************************************************************/
        public static string GetNextAtNeedContract(string sYear, string loc = "" )
        {
            if (String.IsNullOrWhiteSpace(loc))
                loc = "SX";

            string cmd = "Select * from `fcontracts` where `contractNumber` LIKE '%" + loc + sYear + "%' ORDER by `contractNumber` DESC LIMIT 10;";
            //cmd = "SELECT MAX(`record`) from `fcontracts`;";
            DataTable dt = G1.get_db_data(cmd);

            int maxContract = 1;
            if (dt.Rows.Count > 0)
            {
                string contract = dt.Rows[0]["contractNumber"].ObjToString();
                contract = contract.Replace(loc + sYear, "");
                if (G1.validate_numeric(contract))
                    maxContract = contract.ObjToInt32() + 1;
            }
            string newContract = loc + sYear + maxContract.ToString("D03");
            return newContract;
        }
        /***********************************************************************************************/
        static string workWhat = "";
        public static void Import_Cases( string what = "" )
        {
            workWhat = what.ToUpper();
            string title = "SMFS";
            if (what == "SMART")
                title = "Smart Director";
            Import importCasesForm = new Import(" " + title + " Cases");
            importCasesForm.SelectDone += ImportCasesForm_SelectDone;
            importCasesForm.Show();
        }
        /***********************************************************************************************/
        static string GetWhat ( DataTable dt, int i, string what )
        {
            string data = "";
            try
            {
                if (workWhat == "SMART")
                    what = what.Replace(" ", "");
                data = dt.Rows[i][what].ObjToString();
            }
            catch ( Exception ex )
            {
                MessageBox.Show("***ERROR*** Row " + i.ToString() + " Getting Data for Field " + what + "!", "Bad Field Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            }
            return data;
        }
        /***********************************************************************************************/
        private static void ImportCasesForm_SelectDone(System.Data.DataTable dt)
        {
            string cmd = "Select * from `funeralhomes`;";
            DataTable funDt = G1.get_db_data(cmd);

            string keyCode = "";
            string atNeedCode = "";
            string serviceId = "";
            string caseType = "";
            string serviceType = "";
            string contractNumber = "";
            string contract = "";
            string trust = "";
            string loc = "";
            string firstName = "";
            string prefix = "";
            string middleName = "";
            string lastName = "";
            string suffix = "";
            string preferredName = "";
            string ssn = "";
            string address1 = "";
            string address2 = "";
            string city = "";
            string state = "";
            string county = "";
            string zip = "";

            string educationLevel = "";
            string ethnicity = "";
            string maritalStatus = "";
            string language = "";
            string race = "";
            string occupation = "";
            string disposition = "";
            string funeralType = "";
            string locationOfDeath = "";
            string locationOfDeathName = "";

            string cName = "";


            string gender = "";
            DateTime deceasedDate = DateTime.Now;
            DateTime birthDate = DateTime.Now;
            DateTime dispositionDate = DateTime.Now;

            int year = 0;

            string str = "";
            string record = "";

            DataRow[] dRows = null;
            DataTable dd = null;

            int importCount = 0;

            SetupPassare();

            cmd = "Delete from `fcustomers` where `ServiceId` = '-1';";
            G1.get_db_data(cmd);

            cmd = "Delete from `fcontracts` where `ServiceId` = '-1';";
            G1.get_db_data(cmd);

            cmd = "Delete from `fcust_extended` where `ServiceId` = '-1';";
            G1.get_db_data(cmd);

            int lastRow = dt.Rows.Count;
            //lastRow = 1;

            ProgressBar pBar = new ProgressBar();
            pBar.Location = new System.Drawing.Point(0, 0);
            pBar.Name = "progressBar1";
            pBar.Width = 200;
            pBar.Height = 30;
            //pBar.Dock = DockStyle.Fill;

            Form f = new Form();
            f.StartPosition = FormStartPosition.CenterScreen;
            f.Bounds = new Rectangle(0, 0, 200, 100);
            f.Controls.Add(pBar);
            f.Show();
            f.Refresh();
            pBar.Visible = true;
            pBar.Refresh();

            pBar.Minimum = 0;
            pBar.Maximum = lastRow;
            pBar.Value = 0;
            pBar.Show();
            pBar.Refresh();

            for (int i = 0; i < lastRow; i++)
            {
                Application.DoEvents();
                try
                {
                    pBar.Value = i + 1;
                    pBar.Refresh();

                    if (workWhat == "SMART")
                        caseType = GetWhat(dt, i, "Case Type");
                    else
                        caseType = GetWhat(dt, i, "Case Types");
                    if (caseType.ToUpper() == "PRE_NEED")
                        continue;

                    serviceId = GetWhat(dt, i, "Case Number");
                    //serviceId = dt.Rows[i][cName].ObjToString();
                    if (String.IsNullOrWhiteSpace(serviceId))
                        continue;
                    if (serviceId.ToUpper() == "N/A")
                        continue;
                    if (serviceId.ToUpper() == "TEST CASE")
                        continue;

                    contract = Trust85.decodeContractNumber(serviceId, ref trust, ref loc);
                    year = 0;
                    if (contract.Length > 2)
                    {
                        str = contract.Substring(0, 2);
                        year = str.ObjToInt32();
                    }
                    if (year <= 0 || year >= 23)
                    {
                        continue;
                    }

                    dRows = funDt.Select("atneedcode='" + loc + "'");
                    if (dRows.Length <= 0)
                    {
                        //dRows = funDt.Select("merchandiseCode='" + loc + "'");
                        //if ( dRows.Length > 0 )
                        //    keyCode = dRows[0]["keycode"].ObjToString();
                        //else
                        keyCode = loc;
                    }
                    else
                        keyCode = dRows[0]["keycode"].ObjToString();

                    if (loc.ToUpper() == "RF")
                        keyCode = "RF";

                    if ( workWhat == "SMART")
                        serviceType = GetWhat(dt, i, "Case Type");
                    else
                        serviceType = GetWhat(dt, i, "Case Types");
                    //serviceType = dt.Rows[i][cName].ObjToString();

                    if (serviceType.ToUpper() == "AT_NEED")
                    {
                        //contractNumber = GetNextAtNeedContract(year.ToString(), keyCode);
                        contractNumber = keyCode + contract;
                    }
                    else
                        contractNumber = keyCode + contract;

                    prefix = GetWhat(dt, i, "Decedent Title");
                    //prefix = dt.Rows[i]["Decedent Title"].ObjToString();
                    firstName = GetWhat(dt, i, "Decedent First Name");
                    //firstName = dt.Rows[i]["Decedent First Name"].ObjToString();
                    middleName = GetWhat(dt, i, "Decedent Middle Name");
                    //middleName = dt.Rows[i]["Decedent Middle Name"].ObjToString();
                    lastName = GetWhat(dt, i, "Decedent Last Name");
                    //lastName = dt.Rows[i]["Decedent Last Name"].ObjToString();
                    suffix = GetWhat(dt, i, "Decedent Suffix");
                    //suffix = dt.Rows[i]["Decedent Suffix"].ObjToString();

                    preferredName = GetWhat(dt, i, "Decedent Alternate Name");
                    //preferredName = dt.Rows[i]["Decedent Alternate Name"].ObjToString();

                    ssn = GetWhat(dt, i, "Decedent SSN");
                    //ssn = dt.Rows[i]["Decedent SSN"].ObjToString();

                    deceasedDate = GetWhat(dt, i, "Decedent DOD").ObjToDateTime();
                    //deceasedDate = dt.Rows[i]["Decedent DOD"].ObjToDateTime();
                    if (deceasedDate.Year < 1000 )
                    {
                        //MessageBox.Show("***ERROR*** Row " + i.ToString() + " ServiceId=" + serviceId + " DeceasedDate=" + deceasedDate.ToString("MM/dd/yyyy"), "Bad Deceased Date Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                        //continue;
                    }

                    birthDate = GetWhat(dt, i, "Decedent DOB").ObjToDateTime();
                    //birthDate = dt.Rows[i]["Decedent DOB"].ObjToDateTime();

                    dispositionDate = GetWhat(dt, i, "Disposition Date").ObjToDateTime();
                    //dispositionDate = dt.Rows[i]["Disposition Date"].ObjToDateTime();

                    address1 = GetWhat(dt, i, "Decedent Address 1");
                    //address1 = dt.Rows[i]["Decedent Address 1"].ObjToString();

                    address2 = GetWhat(dt, i, "Decedent Address 2");
                    //address2 = dt.Rows[i]["Decedent Address 2"].ObjToString();

                    city = GetWhat(dt, i, "Decedent City");
                    //city = dt.Rows[i]["Decedent City"].ObjToString();

                    state = GetWhat(dt, i, "Decedent State");
                    //state = dt.Rows[i]["Decedent State"].ObjToString();

                    zip = GetWhat(dt, i, "Decedent Zip Code");
                    //zip = dt.Rows[i]["Decedent Zip Code"].ObjToString();

                    county = GetWhat(dt, i, "Decedent County");
                    //county = dt.Rows[i]["Decedent County"].ObjToString();

                    gender = GetWhat(dt, i, "Decedent Gender");
                    //gender = dt.Rows[i]["Decedent Gender"].ObjToString();

                    if (gender.ToUpper() == "MALE")
                        gender = "Male";
                    else if (gender.ToUpper() == "FEMALE")
                        gender = "Female";
                    else
                        gender = "Male";

                    educationLevel = GetWhat(dt, i, "Decedent Level of Education");
                    //educationLevel = dt.Rows[i]["Decedent Level of Education"].ObjToString();


                    maritalStatus = GetWhat(dt, i, "Decedent Marital Status");
                    //maritalStatus = dt.Rows[i]["Decedent Marital Status"].ObjToString();

                    occupation = GetWhat(dt, i, "Decedent Occupation");
                    //occupation = dt.Rows[i]["Decedent Occupation"].ObjToString();

                    disposition = GetWhat(dt, i, "Disposition Type");
                    //disposition = dt.Rows[i]["Disposition Type"].ObjToString();

                    funeralType = GetWhat(dt, i, "Client Service Type");
                    //funeralType = dt.Rows[i]["Client Service Type"].ObjToString();

                    locationOfDeath = GetWhat(dt, i, "Decedent Location of Death");
                    //locationOfDeath = dt.Rows[i]["Decedent Location of Death"].ObjToString();

                    locationOfDeathName = GetWhat(dt, i, "Decedent Location of Death Name");
                    //locationOfDeathName = dt.Rows[i]["Decedent Location of Death Name"].ObjToString();

                    cmd = "Select * from `fcontracts` where `serviceId` = '" + serviceId + "';";
                    dd = G1.get_db_data(cmd);
                    if (dd.Rows.Count > 0)
                        record = dd.Rows[0]["record"].ObjToString();
                    else
                        record = G1.create_record("fcontracts", "ServiceId", "-1");
                    if (G1.BadRecord("fcontracts", record))
                        continue;

                    G1.update_db_table("fcontracts", "record", record, new string[] { "contractNumber", contractNumber, "ServiceId", serviceId, "deceasedDate", deceasedDate.ToString("yyyy-MM-dd") });


                    cmd = "Select * from `fcustomers` where `serviceId` = '" + serviceId + "';";
                    dd = G1.get_db_data(cmd);
                    if (dd.Rows.Count > 0)
                    {
                        record = dd.Rows[0]["record"].ObjToString();
                        contractNumber = dd.Rows[0]["contractNumber"].ObjToString();
                    }
                    else
                        record = G1.create_record("fcustomers", "ServiceId", "-1");
                    if (G1.BadRecord("fcustomers", record))
                        continue;

                    G1.update_db_table("fcustomers", "record", record, new string[] { "contractNumber", contractNumber, "ServiceId", serviceId, "deceasedDate", deceasedDate.ToString("yyyy-MM-dd"), "ssn", ssn, "sex", gender });
                    G1.update_db_table("fcustomers", "record", record, new string[] { "birthDate", birthDate.ToString("yyyy-MM-dd"), "firstName", firstName, "middleName", middleName, "lastName", lastName, "prefix", prefix, "suffix", suffix, "address1", address1, "address2", address2, "city", city, "State", state, "zip1", zip });

                    G1.update_db_table("fcustomers", "record", record, new string[] { "ethnicity", ethnicity, "maritalstatus", maritalStatus, "race", race, "language", language, "preferredName", preferredName });



                    cmd = "Select * from `fcust_extended` where `serviceId` = '" + serviceId + "';";
                    dd = G1.get_db_data(cmd);
                    if (dd.Rows.Count > 0)
                        record = dd.Rows[0]["record"].ObjToString();
                    else
                        record = G1.create_record("fcust_extended", "ServiceId", "-1");
                    if (G1.BadRecord("fcust_extended", record))
                        continue;

                    G1.update_db_table("fcust_extended", "record", record, new string[] { "contractNumber", contractNumber, "ServiceId", serviceId, "deccounty", county, "serviceDate", dispositionDate.ToString("yyyy-MM-dd"), "EducationLevel", educationLevel, "Occ", occupation, "Disposition", disposition, "funeral_classification", disposition + " - " + funeralType });
                    G1.update_db_table("fcust_extended", "record", record, new string[] { "Place of Death", locationOfDeath, "POD", locationOfDeathName });

                    importCount++;

                    //if (1 == 1)
                    //    break;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("***ERROR*** Row " + i.ToString() + "\n" + ex.Message.ToUpper(), "Case Import Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                }
            }

            pBar.Dispose();
            pBar = null;

            f.Close();
            f.Dispose();
            f = null;

            SetupMain();

            MessageBox.Show("***INFO*** " + importCount.ToString() + " Record Imported!", "Imported Record Count Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
        }
        /***********************************************************************************************/
        public static void Import_Services(string what = "" )
        {
            workWhat = what.ToUpper();
            string title = "SMFS";
            if (what == "SMART")
                title = "Smart Director";
            Import importServicesForm = new Import(" " + title + " Goods and Services");
            importServicesForm.SelectDone += ImportServicesForm_SelectDone;
            importServicesForm.Show();
        }
        /***********************************************************************************************/
        private static void ImportServicesForm_SelectDone(System.Data.DataTable dt)
        {
            string cmd = "Select * from `casket_master`;";
            DataTable casketDt = G1.get_db_data(cmd);

            SetupPassare();

            string oldServiceId = "";
            string serviceId = "";
            string contractNumber = "";
            string record = "";
            string service = "";
            string type = "";
            string quantity = "";
            string unitPrice = "";
            string price = "";

            double dValue = 0D;

            int importCount = 0;

            bool first = true;

            DataTable dd = null;
            DataTable dx = null;
            DataRow[] dRows = null;
            string[] Lines = null;

            int lastRow = dt.Rows.Count;
            //lastRow = 1;

            cmd = "Delete from `fcust_services` where `service` = '-1';";
            G1.get_db_data(cmd);

            ProgressBar pBar = new ProgressBar();
            pBar.Location = new System.Drawing.Point(0, 0);
            pBar.Name = "progressBar1";
            pBar.Width = 200;
            pBar.Height = 30;
            //pBar.Dock = DockStyle.Fill;

            Form f = new Form();
            f.StartPosition = FormStartPosition.CenterScreen;
            f.Bounds = new Rectangle(0, 0, 200, 100);
            f.Controls.Add(pBar);
            f.Show();
            f.Refresh();
            pBar.Visible = true;
            pBar.Refresh();

            pBar.Minimum = 0;
            pBar.Maximum = lastRow;
            pBar.Value = 0;
            pBar.Show();
            pBar.Refresh();

            for (int i = 0; i < lastRow; i++)
            {
                Application.DoEvents();
                try
                {
                    pBar.Value = i + 1;
                    pBar.Refresh();

                    serviceId = GetWhat(dt, i, "Case Number");
                    //serviceId = dt.Rows[i]["Case Number"].ObjToString();
                    if (String.IsNullOrWhiteSpace(serviceId))
                        continue;
                    if (serviceId.ToUpper() == "N/A")
                        continue;
                    if (serviceId.ToUpper() == "TEST CASE")
                        continue;

                    if (String.IsNullOrWhiteSpace(oldServiceId))
                        oldServiceId = serviceId;

                    if (oldServiceId != serviceId)
                    {
                        contractNumber = "";
                        //break;
                    }

                    if (String.IsNullOrWhiteSpace(contractNumber))
                    {
                        cmd = "Select * from `fcustomers` where `serviceId` = '" + serviceId + "';";
                        dd = G1.get_db_data(cmd);
                        if (dd.Rows.Count <= 0)
                        {
                            MessageBox.Show("***ERROR*** Row " + i.ToString() + " ServiceId=" + serviceId + " Does Not Exist!!", "Bad Service ID Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                            break;
                        }
                        contractNumber = dd.Rows[0]["contractNumber"].ObjToString();
                    }

                    service = GetWhat(dt, i, "Item Title");
                    //service = dt.Rows[i]["Item Title"].ObjToString();
                    if (String.IsNullOrWhiteSpace(service))
                        continue;

                    quantity = GetWhat(dt, i, "Quantity");
                    //quantity = dt.Rows[i]["Quantity"].ObjToString();

                    unitPrice = GetWhat(dt, i, "Unit Price");
                    //unitPrice = dt.Rows[i]["Unit Price"].ObjToString();
                    //if (unitPrice.IndexOf("-") >= 0)
                    //    unitPrice = "0.00";

                    price = GetWhat(dt, i, "Total Price");
                    //price = dt.Rows[i]["Total Price"].ObjToString();
                    //price = price.Replace("-", "");

                    type = "Service";
                    if (service.ToUpper() == "ACKNOWLEDGEMENT CARDS")
                        type = "Merchandise";
                    if (service.ToUpper() == "REGISTER BOOK AND POUCH")
                        type = "Merchandise";
                    if (service.ToUpper() == "TEMPORARY GRAVE MARKER")
                        type = "Merchandise";

                    service = G1.try_protect_data(service);
                    if ( type.ToUpper() == "SERVICE")
                    {
                        dRows = casketDt.Select("casketdesc='" + service + "'");
                        if (dRows.Length > 0)
                            type = "Merchandise";
                        else
                        {
                            Lines = service.Split(' ');
                            if ( Lines.Length > 1 )
                            {
                                dRows = casketDt.Select("casketcode='" + Lines[0].Trim() + "'");
                                if (dRows.Length > 0)
                                    type = "Merchandise";
                                else
                                {
                                    dRows = casketDt.Select("casketcode LIKE '" + Lines[0].Trim() + "%'");
                                    if (dRows.Length > 0)
                                        type = "Merchandise";
                                }
                            }
                        }
                    }


                    cmd = "Select * from `fcust_services` WHERE `contractNumber` = '" + contractNumber + "' AND `service` = '" + service + "';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                        record = dx.Rows[0]["record"].ObjToString();
                    else
                        record = G1.create_record("fcust_services", "service", "-1");
                    if (G1.BadRecord("fcust_services", record))
                        continue;


                    G1.update_db_table("fcust_services", "record", record, new string[] { "contractNumber", contractNumber, "service", service, "type", type, "data", unitPrice, "price", price });

                    importCount++;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("***ERROR*** Row " + i.ToString() + "\n" + ex.Message.ToUpper(), "Services Import Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                }
            }

            pBar.Dispose();
            pBar = null;

            f.Close();
            f.Dispose();
            f = null;

            SetupMain();
            MessageBox.Show("***INFO*** " + importCount.ToString() + " Record Imported!", "Imported Record Count Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
        }
        /***********************************************************************************************/
        public static  DataTable GetPaymentData(StreamReader sr)
        {
            //Regex regex = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
            Regex regex = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");

            DataTable dt = new DataTable();
            bool first = true;
            string str = "";
            int i = 0;
            try
            {
                string master = sr.ReadToEnd();

                dt = ParseOutPayments(master, "\n");
            }
            catch (Exception ex)
            {
            }
            return dt;
        }
        /********************************************************************************************/
        public static DataTable ParseOutPayments(string str, string delimiter )
        {
            DataTable dt = new DataTable();
            DataRow dR = null;

            str += delimiter;
            int length = str.Length;
            bool quote = false;
            int delim = (int)delimiter[0];
            string data = "";
            byte[] pstr = new byte[500];
            int count = 0;
            bool first = true;
            int totalColumns = 0;
            int columnCount = 0;
            for (int i = 0; i < length; i++)
            {
                int ch = (int)(str[i]);
                if (ch == '"')
                {
                    if (!quote)
                    {
                        quote = true;
                        continue;
                    }
                    quote = false;
                    continue;
                }
                if (ch == ',')
                {
                    if (quote)
                    {
                        pstr[count] = (byte)ch;
                        count++;
                        continue;
                    }
                    pstr[count] = (byte)'\0';
                    count = 0;
                    data = G1.ConvertToString(pstr);
                    data = G1.trim(data);
                    if (first)
                    {
                        dt.Columns.Add(data);
                        totalColumns++;
                        continue;
                    }
                    if ( columnCount == 0 )
                        dR = dt.NewRow();
                    if ( columnCount < totalColumns )
                        dR[columnCount] = data;
                    columnCount++;
                    continue;
                }
                if (ch == delim)
                {
                    if (quote)
                    {
                        pstr[count] = (byte)ch;
                        count++;
                        continue;
                    }
                    else
                    {
                        pstr[count] = (byte)'\0';
                        count = 0;
                        data = G1.ConvertToString(pstr);
                        data = G1.trim(data);
                        if ( first )
                        {
                            dt.Columns.Add(data);
                            totalColumns++;
                            first = false;
                            continue;
                        }
                        if (columnCount < totalColumns && dR != null )
                            dR[columnCount] = data;
                        if ( dR != null )
                            dt.Rows.Add(dR);
                        columnCount = 0;
                        dR = null;
                    }
                    continue;
                }
                pstr[count] = (byte)ch;
                count++;
            }
            return dt;
        }
        /***********************************************************************************************/
        public static void Import_Payments( string what = "" )
        {
            workWhat = what.ToUpper();
            string title = "SMFS";
            if (what == "SMART")
                title = "Smart Director";

            if ( what == "SMART")
            {
                Import importPaymentsForm = new Import(" " + title + " Payments");
                importPaymentsForm.SelectDone += ImportPaymentDetails;
                importPaymentsForm.Show();
                return;
            }

            string actualFile = "";
            string file = "";
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                if (ofd.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    return;
                file = ofd.FileName;
                int idx = file.LastIndexOf("\\");
                if (idx > 0)
                {
                    actualFile = file.Substring(idx);
                    actualFile = actualFile.Replace("\\", "");
                }
                ofd.Dispose();
            }


            DataTable dt = null;
            try
            {
                string delimiter = ",";
                char cDelimiter = (char)delimiter[0];
                dt = new DataTable();
                if (!File.Exists(file))
                {
                    DevExpress.XtraEditors.XtraMessageBox.Show("***ERROR*** File does not exist!");
                    return;
                }
                try
                {
                    FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        dt = GetPaymentData(sr);
                        sr.Close();
                        ImportPaymentDetails(dt);
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
        /***********************************************************************************************/
        private static void ImportPaymentDetails ( DataTable dt )
        {
            if (dt == null)
                return;
            if (dt.Rows.Count <= 0)
                return;

            SetupPassare();

            string oldServiceId = "";
            string serviceId = "";
            string contractNumber = "";
            string cmd = "";
            string record = "";
            string type = "";
            string payment = "";
            string paymentDetails = "";
            string status = "";
            DateTime paymentDate = DateTime.Now;
            DataTable dd = null;
            DataTable dx = null;

            int lastRow = dt.Rows.Count;
            //lastRow = 1;

            cmd = "Delete from `cust_payments` where `status` = '-1';";
            G1.get_db_data(cmd);

            ProgressBar pBar = new ProgressBar();
            pBar.Location = new System.Drawing.Point(0, 0);
            pBar.Name = "progressBar1";
            pBar.Width = 200;
            pBar.Height = 30;
            //pBar.Dock = DockStyle.Fill;

            Form f = new Form();
            f.StartPosition = FormStartPosition.CenterScreen;
            f.Bounds = new Rectangle(0, 0, 200, 100);
            f.Controls.Add(pBar);
            f.Show();
            f.Refresh();
            pBar.Visible = true;
            pBar.Refresh();

            pBar.Minimum = 0;
            pBar.Maximum = lastRow;
            pBar.Value = 0;
            pBar.Show();
            pBar.Refresh();

            int importCount = 0;
            int badCount = 0;
            int created = 0;

            for (int i = 0; i < lastRow; i++)
            {
                Application.DoEvents();
                try
                {
                    pBar.Value = i + 1;
                    pBar.Refresh();

                    serviceId = GetWhat(dt, i, "Case Number");
                    //serviceId = dt.Rows[i]["Case Number"].ObjToString();
                    if (String.IsNullOrWhiteSpace(serviceId))
                        continue;
                    if (serviceId.ToUpper() == "N/A")
                        continue;
                    if (serviceId.ToUpper() == "TEST CASE")
                        continue;

                    if (String.IsNullOrWhiteSpace(oldServiceId))
                        oldServiceId = serviceId;

                    if (oldServiceId != serviceId)
                    {
                        contractNumber = "";
                        //break;
                    }
                    if (serviceId.ToUpper() == "EV20022")
                    {
                    }

                    if (String.IsNullOrWhiteSpace(contractNumber))
                    {
                        cmd = "Select * from `fcustomers` where `serviceId` = '" + serviceId + "';";
                        dd = G1.get_db_data(cmd);
                        if (dd.Rows.Count <= 0)
                        {
                            MessageBox.Show("***ERROR*** Row " + i.ToString() + " ServiceId=" + serviceId + " Does Not Exist!!", "Bad Service ID Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                            break;
                        }
                        contractNumber = dd.Rows[0]["contractNumber"].ObjToString();
                    }

                    type = GetWhat(dt, i, "Payment Type");
                    //type = dt.Rows[i]["Payment Type"].ObjToString();

                    payment = GetWhat(dt, i, "Payment Amount");
                    //payment = dt.Rows[i]["Payment Amount"].ObjToString();

                    paymentDetails = GetWhat(dt, i, "Payment Details");
                    //paymentDetails = dt.Rows[i]["Payment Details"].ObjToString();

                    paymentDetails = paymentDetails.Replace("\n", "~");

                    paymentDate = GetWhat(dt, i, "Payment Date").ObjToDateTime();
                    //paymentDate = dt.Rows[i]["Payment Date"].ObjToDateTime();
                    status = "Deposited";

                    if (paymentDetails.IndexOf("Trusts (Insurance Assignment)") >= 0)
                    {
                        paymentDetails = paymentDetails.Replace("Trusts (Insurance Assignment)", "");
                        type = "Trust";
                    }
                    else if (paymentDetails.IndexOf("Insurance Assignment") >= 0)
                    {
                        paymentDetails = paymentDetails.Replace("Insurance Assignment", "");
                        type = "Insurance";
                    }
                    else if (paymentDetails.IndexOf("Adjustment") >= 0)
                    {
                        paymentDetails = paymentDetails.Replace("Adjustment", "");
                        type = "Adjustment";
                    }
                    else if (paymentDetails.IndexOf("Payments") >= 0)
                    {
                        paymentDetails = paymentDetails.Replace("Payments", "");
                        type = "Payments";
                    }

                    if (paymentDetails.IndexOf("Preneed Discount") >= 0)
                        type = "Discount";

                    paymentDetails = G1.try_protect_data(paymentDetails);
                    if (paymentDetails.Length > 100)
                        paymentDetails = paymentDetails.Substring(0, 100);
                    if ( String.IsNullOrWhiteSpace ( paymentDetails ))
                        cmd = "Select * from `cust_payments` WHERE `contractNumber` = '" + contractNumber + "' AND `payment` = '" + payment.ToString() + "';";
                    else
                        cmd = "Select * from `cust_payments` WHERE `contractNumber` = '" + contractNumber + "' AND `description` = '" + paymentDetails + "' AND `payment` = '" + payment.ToString() + "';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                    {
                        record = dx.Rows[0]["record"].ObjToString();
                        if (G1.BadRecord("cust_payments", record))
                            continue;
                        //G1.update_db_table("cust_payments", "record", record, new string[] { "contractNumber", contractNumber, "description", paymentDetails, "status", status, "type", type, "payment", payment, "dateEntered", paymentDate.ToString("yyyy-MM-dd"), "dateModified", paymentDate.ToString("yyyy-MM-dd") });
                    }
                    else
                    {
                        badCount++;
                        record = G1.create_record("cust_payments", "status", "-1");
                        G1.update_db_table("cust_payments", "record", record, new string[] { "contractNumber", contractNumber, "description", paymentDetails, "status", status, "type", type, "payment", payment, "dateEntered", paymentDate.ToString("yyyy-MM-dd"), "dateModified", paymentDate.ToString("yyyy-MM-dd") });
                        created++;
                    }
                    importCount++;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("***ERROR*** Row " + i.ToString() + "\n" + ex.Message.ToUpper(), "Services Import Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                }
            }

            pBar.Dispose();
            pBar = null;

            f.Close();
            f.Dispose();
            f = null;

            SetupMain();

            MessageBox.Show("***INFO*** " + importCount.ToString() + " Record Processed!\nCreated " + created.ToString() + "!", "Imported Record Count Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
        }
        /***********************************************************************************************/
        public static void Import_Acquaintances( string what = "" )
        {
            workWhat = what.ToUpper();
            string title = "SMFS";
            if (what == "SMART")
                title = "Smart Director";

            Import importAcquaintancesForm = new Import(" " + title + " Acquaintances");
            importAcquaintancesForm.SelectDone += ImportAcquaintancesForm_SelectDone;
            importAcquaintancesForm.Show();
        }
        /***********************************************************************************************/
        private static void ImportAcquaintancesForm_SelectDone(System.Data.DataTable dt)
        {
            SetupPassare();

            string oldServiceId = "";
            string serviceId = "";
            string contractNumber = "";
            string cmd = "";
            string record = "";

            string firstName = "";
            string prefix = "";
            string middleName = "";
            string lastName = "";
            string suffix = "";
            string fullName = "";
            string preferredName = "";
            string address1 = "";
            string address2 = "";
            string address = "";
            string city = "";
            string state = "";
            string county = "";
            string zip = "";

            string str = "";

            string email = "";
            string phone = "";
            string phoneType = "";

            string homePhone = "";
            string workPhone = "";
            string cellPhone = "";

            string spouseFirstName = "";
            string spouseLastName = "";

            string nok = "";
            string pp = "";
            string informant = "";
            string embalming = "";
            string deceased = "";
            DateTime deceasedDate = DateTime.Now;
            string deceasedStr = "";

            string roles = "";
            string relationship = "";

            string contract = "";
            string trust = "";
            string loc = "";

            int year = 0;

            DataTable dd = null;
            DataTable dx = null;
            string[] Lines = null;

            int lastRow = dt.Rows.Count;

            int importCount = 0;
            //lastRow = 1;

            cmd = "Delete from `relatives` where `depSuffix` = '-1';";
            G1.get_db_data(cmd);

            ProgressBar pBar = new ProgressBar();
            pBar.Location = new System.Drawing.Point(0, 0);
            pBar.Name = "progressBar1";
            pBar.Width = 200;
            pBar.Height = 30;
            //pBar.Dock = DockStyle.Fill;

            Form f = new Form();
            f.StartPosition = FormStartPosition.CenterScreen;
            f.Bounds = new Rectangle(0, 0, 200, 100);
            f.Controls.Add(pBar);
            f.Show();
            f.Refresh();
            pBar.Visible = true;
            pBar.Refresh();

            pBar.Minimum = 0;
            pBar.Maximum = lastRow;
            pBar.Value = 0;
            pBar.Show();
            pBar.Refresh();

            for (int i = 0; i < lastRow; i++)
            {
                Application.DoEvents();
                try
                {
                    pBar.Value = i+1;
                    pBar.Refresh();

                    serviceId = GetWhat(dt, i, "Case Number");
                    //serviceId = dt.Rows[i]["Case Number"].ObjToString();
                    if (String.IsNullOrWhiteSpace(serviceId))
                        continue;
                    if (serviceId.ToUpper() == "N/A")
                        continue;
                    if (serviceId.ToUpper() == "TEST CASE")
                        continue;
                    if (serviceId.ToUpper() == "TEST")
                        continue;
                    if (serviceId.ToUpper().IndexOf("TEST") == 0)
                        continue;

                    contract = Trust85.decodeContractNumber(serviceId, ref trust, ref loc);
                    year = 0;
                    if (contract.Length > 2)
                    {
                        str = contract.Substring(0, 2);
                        year = str.ObjToInt32();
                    }
                    if (year <= 0 || year >= 23)
                    {
                        continue;
                    }

                    if (String.IsNullOrWhiteSpace(oldServiceId))
                        oldServiceId = serviceId;

                    if (oldServiceId != serviceId)
                    {
                        contractNumber = "";
                        //break;
                    }

                    if (String.IsNullOrWhiteSpace(contractNumber))
                    {
                        cmd = "Select * from `fcustomers` where `serviceId` = '" + serviceId + "';";
                        dd = G1.get_db_data(cmd);
                        if (dd.Rows.Count <= 0)
                        {
                            //.Show("***ERROR*** Row " + i.ToString() + " ServiceId=" + serviceId + " Does Not Exist!!", "Bad Service ID Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                            continue;
                        }
                        contractNumber = dd.Rows[0]["contractNumber"].ObjToString();
                        if (String.IsNullOrWhiteSpace(contractNumber))
                            continue;

                        cmd = "Delete from `relatives` where `contractNumber` = '" + contractNumber + "';";
                        G1.get_db_data(cmd);
                    }

                    prefix = GetWhat(dt, i, "Title");
                    //prefix = dt.Rows[i]["Title"].ObjToString();

                    firstName = GetWhat(dt, i, "First Name");
                    //firstName = dt.Rows[i]["First Name"].ObjToString();

                    middleName = GetWhat(dt, i, "Middle Name");
                    //middleName = dt.Rows[i]["Middle Name"].ObjToString();

                    lastName = GetWhat(dt, i, "Last Name");
                    //lastName = dt.Rows[i]["Last Name"].ObjToString();

                    suffix = GetWhat(dt, i, "Suffix");
                    //suffix = dt.Rows[i]["Suffix"].ObjToString();

                    if ( workWhat == "SMART")
                        preferredName = GetWhat(dt, i, "PreferredName");
                    else
                        preferredName = GetWhat(dt, i, "Alternate Name");
                    //preferredName = dt.Rows[i]["Alternate Name"].ObjToString();

                    fullName = FunFamilyNew.BuildFullName(prefix, firstName, middleName, lastName, suffix);

                    nok = "";
                    pp = "";
                    informant = "";
                    embalming = "0";

                    roles = GetWhat(dt, i, "Roles");
                    //roles = dt.Rows[i]["Roles"].ObjToString();
                    if ( !String.IsNullOrWhiteSpace ( roles ))
                    {
                        Lines = roles.Split('|');
                        for ( int j=0; j<Lines.Length; j++)
                        {
                            if (Lines[j].ToUpper() == "NEXT_OF_KIN")
                                nok = "1";
                            else if (Lines[j].ToUpper() == "INFORMANT")
                                informant = "1";
                            else if (Lines[j].ToUpper() == "PRIMARY_PURCHASER")
                                pp = "1";
                            else if (Lines[j].ToUpper() == "EMBALMING_AUTHORIZER")
                                embalming = "1";
                        }
                    }
                    relationship = GetWhat(dt, i, "Relationship");
                    //relationship = dt.Rows[i]["Relationship"].ObjToString();

                    deceased = "0";
                    str = GetWhat(dt, i, "Deceased");
                    //str = dt.Rows[i]["Deceased"].ObjToString();
                    if (str.ToUpper() == "TRUE")
                        deceased = "1";

                    deceasedDate = GetWhat ( dt, i, "Date of Death").ObjToDateTime();
                    //deceasedDate = dt.Rows[i]["Date of Death"].ObjToDateTime();
                    if (deceasedDate.Year > 1000)
                        deceased = "1";

                    deceasedStr = "";
                    if (deceased == "1" && deceasedDate.Year > 1000)
                        deceasedStr = deceasedDate.ToString("yyyy-MM-dd");

                    email = GetWhat(dt, i, "Email Address");
                    //email = dt.Rows[i]["Email Address"].ObjToString();

                    homePhone = GetWhat(dt, i, "Home Phone");
                    //homePhone = dt.Rows[i]["Home Phone"].ObjToString();

                    workPhone = GetWhat(dt, i, "Work Phone");
                    //workPhone = dt.Rows[i]["Work Phone"].ObjToString();

                    cellPhone = GetWhat(dt, i, "Mobile Phone");
                    //cellPhone = dt.Rows[i]["Mobile Phone"].ObjToString();
                    if ( !String.IsNullOrWhiteSpace ( cellPhone ))
                    {
                        phone = cellPhone;
                        phoneType = "Cell";
                    }
                    else if (!String.IsNullOrWhiteSpace(homePhone))
                    {
                        phone = homePhone;
                        phoneType = "Home";
                    }
                    else if (!String.IsNullOrWhiteSpace(workPhone))
                    {
                        phone = workPhone;
                        phoneType = "Work";
                    }

                    address1 = GetWhat(dt, i, "Address 1");
                    //address1 = dt.Rows[i]["Address 1"].ObjToString();

                    address2 = GetWhat(dt, i, "Address 2");
                    //address2 = dt.Rows[i]["Address 2"].ObjToString();
                    address = address1.Trim() + " " + address2;
                    address = address.Trim();

                    city = GetWhat(dt, i, "City");
                    //city = dt.Rows[i]["City"].ObjToString();

                    state = GetWhat(dt, i, "State");
                    //state = dt.Rows[i]["State"].ObjToString();

                    zip = GetWhat(dt, i, "Zip");
                    //zip = dt.Rows[i]["Zip"].ObjToString();

                    spouseFirstName = GetWhat(dt, i, "Spouse First Name");
                    //spouseFirstName = dt.Rows[i]["Spouse First Name"].ObjToString();

                    spouseLastName = GetWhat(dt, i, "Spouse Last Name");
                    //spouseLastName = dt.Rows[i]["Spouse Last Name"].ObjToString();
                    spouseFirstName += " " + spouseLastName;
                    spouseFirstName = spouseFirstName.Trim();

                    record = G1.create_record("relatives", "depSuffix", "-1");
                    if (G1.BadRecord("relatives", record))
                        continue;


                    G1.update_db_table("relatives", "record", record, new string[] { "contractNumber", contractNumber, "depFirstName", firstName, "depLastName", lastName, "depMI", middleName, "depPrefix", prefix, "depSuffix", suffix, "depRelationship", relationship, "fullName", fullName });
                    G1.update_db_table("relatives", "record", record, new string[] { "depDOD", deceasedStr, "deceased", deceased, "address", address, "city", city, "state", state, "zip", zip, "email", email, "phone", phone, "phoneType", phoneType, "nextOfKin", nok, "informant", informant, "purchaser", pp, "spouseFirstName", spouseFirstName, "authEmbalming", embalming });

                    importCount++;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("***ERROR*** Row " + i.ToString() + "\n" + ex.Message.ToUpper(), "Relatives Import Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                }
            }

            pBar.Dispose();
            pBar = null;

            f.Close();
            f.Dispose();
            f = null;

            SetupMain();

            MessageBox.Show("***INFO*** " + importCount.ToString() + " Record Imported!", "Imported Record Count Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
        }
        /***********************************************************************************************/
        public static void Import_Events( string what = "" )
        {
            workWhat = what.ToUpper();
            string title = "SMFS";
            if (what == "SMART")
                title = "Smart Director";

            Import importEventsForm = new Import(" " + title + " Events");
            importEventsForm.SelectDone += ImportEventsForm_SelectDone;
            importEventsForm.Show();
        }
        /***********************************************************************************************/
        private static void ImportEventsForm_SelectDone(System.Data.DataTable dt)
        {
            SetupPassare();

            string oldServiceId = "";
            string serviceId = "";
            string contractNumber = "";
            string cmd = "";
            string record = "";


            DataTable dd = null;
            DataTable dx = null;
            string[] Lines = null;

            int lastRow = dt.Rows.Count;
            //lastRow = 1;

            ProgressBar pBar = new ProgressBar();
            pBar.Location = new System.Drawing.Point(0, 0);
            pBar.Name = "progressBar1";
            pBar.Width = 200;
            pBar.Height = 30;
            //pBar.Dock = DockStyle.Fill;

            Form f = new Form();
            f.StartPosition = FormStartPosition.CenterScreen;
            f.Bounds = new Rectangle(0, 0, 200, 100);
            f.Controls.Add(pBar);
            f.Show();
            f.Refresh();
            pBar.Visible = true;
            pBar.Refresh();

            pBar.Minimum = 0;
            pBar.Maximum = lastRow;
            pBar.Value = 0;
            pBar.Show();
            pBar.Refresh();

            string eventType = "";
            string eventDate = "";
            string startTime = "";
            string endTime = "";
            string venueName = "";
            string venueType = "";
            string address1 = "";
            string address2 = "";
            string address = "";
            string city = "";
            string state = "";
            string zip = "";
            string attendance = "";

            DateTime myDate = DateTime.Now;
            string dow = "";

            int importCount = 0;

            string service1 = "";
            string service2 = "";
            string visitation1 = "";
            string visitation2 = "";

            for (int i = 0; i < lastRow; i++)
            {
                Application.DoEvents();
                try
                {
                    pBar.Value = i + 1;
                    pBar.Refresh();

                    serviceId = GetWhat(dt, i, "Case Number");
                    //serviceId = dt.Rows[i]["Case Number"].ObjToString();
                    if (String.IsNullOrWhiteSpace(serviceId))
                        continue;
                    if (serviceId.ToUpper() == "N/A")
                        continue;
                    if (serviceId.ToUpper() == "TEST CASE")
                        continue;

                    if (String.IsNullOrWhiteSpace(oldServiceId))
                        oldServiceId = serviceId;

                    if (oldServiceId != serviceId)
                    {
                        contractNumber = "";
                        //break;
                    }

                    if (String.IsNullOrWhiteSpace(contractNumber))
                    {
                        cmd = "Select * from `fcustomers` where `serviceId` = '" + serviceId + "';";
                        dd = G1.get_db_data(cmd);
                        if (dd.Rows.Count <= 0)
                        {
                            //MessageBox.Show("***ERROR*** Row " + i.ToString() + " ServiceId=" + serviceId + " Does Not Exist!!", "Bad Service ID Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                            continue;
                        }
                        contractNumber = dd.Rows[0]["contractNumber"].ObjToString();
                        if (String.IsNullOrWhiteSpace(contractNumber))
                            continue;

                        cmd = "Select * from `fcust_extended` where `contractNumber` = '" + contractNumber + "';";
                        dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count <= 0)
                            continue;
                        record = dx.Rows[0]["record"].ObjToString();
                        G1.update_db_table("fcust_extended", "record", record, new string[] { "SRVType", "", "SRVLoc", "", "SRVDate", "", "SRVTime", "", "SRVCity", "", "SRVState", "", "SRVDAYDATE", "" });
                        G1.update_db_table("fcust_extended", "record", record, new string[] { "SRV2Type", "", "SRV2Loc", "", "SRV2Date", "", "SRV2Time", "", "SRV2City", "" + " " + "", "RV2DAYDATE", "" });
                        G1.update_db_table("fcust_extended", "record", record, new string[] { "VIS1Type", "", "VIS1Loc", "", "VSTDATE", "", "VSTSTART", "", "VST1Add", "", "VISDAYDATE", "" });
                        G1.update_db_table("fcust_extended", "record", record, new string[] { "VIS2Type", "", "VIS2Loc", "", "Vis2Date", "", "Vis2TimeStart", "", "VIS2Add", "", "Vis2DayDate", "" });
                    }

                    cmd = "Select * from `fcust_extended` where `contractNumber` = '" + contractNumber + "';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count <= 0)
                        continue;
                    record = dx.Rows[0]["record"].ObjToString();

                    eventType = GetWhat(dt, i, "Event Type");
                    //eventType = dt.Rows[i]["Event Type"].ObjToString();

                    eventDate = GetWhat(dt, i, "Event Date");
                    //eventDate = dt.Rows[i]["Event Date"].ObjToString();

                    startTime = GetWhat(dt, i, "Start Time");
                    //startTime = dt.Rows[i]["Start Time"].ObjToString();

                    endTime = GetWhat(dt, i, "End Time");
                    //endTime = dt.Rows[i]["End Time"].ObjToString();

                    venueName = GetWhat(dt, i, "Venue Name");
                    //venueName = dt.Rows[i]["Venue Name"].ObjToString();

                    venueType = GetWhat(dt, i, "Venue Type");
                    //venueType = dt.Rows[i]["Venue Type"].ObjToString();

                    address1 = GetWhat(dt, i, "Venue Address Line 1");
                    //address1 = dt.Rows[i]["Venue Address Line 1"].ObjToSting();

                    address2 = GetWhat(dt, i, "Venue Address Line 2");
                    //address2 = dt.Rows[i]["Venue Address Line 2"].ObjToString();

                    city = GetWhat(dt, i, "Venue City");
                    //city = dt.Rows[i]["Venue City"].ObjToString();

                    state = GetWhat(dt, i, "Venue State");
                    //state = dt.Rows[i]["Venue State"].ObjToString();

                    zip = GetWhat(dt, i, "Venue Postal Code");
                    //zip = dt.Rows[i]["Venue Postal Code"].ObjToString();

                    attendance = GetWhat(dt, i, "Attendance");
                    //attendance = dt.Rows[i]["Attendance"].ObjToString();

                    address = address1 + " " + address2;
                    address = address.Trim();

                    visitation1 = dx.Rows[0]["VIS1LOC"].ObjToString();
                    visitation2 = dx.Rows[0]["VIS2LOC"].ObjToString();

                    service1 = dx.Rows[0]["SRVTYPE"].ObjToString();
                    service2 = dx.Rows[0]["SRV2TYPE"].ObjToString();

                    myDate = eventDate.ObjToDateTime();
                    dow = "";
                    if ( myDate.Year > 1000)
                        dow = G1.DayOfWeekText(myDate);


                    if ( eventType.ToUpper().IndexOf ( "SERVICE") >= 0 )
                    {
                        if ( String.IsNullOrWhiteSpace ( service1 ))
                        {
                            G1.update_db_table("fcust_extended", "record", record, new string[] { "SRVType", eventType, "SRVLoc", venueName, "SRVDate", eventDate, "SRVTime", startTime, "SRVCity", city, "SRVState", state, "SRVDAYDATE", dow });
                        }
                        else
                        {
                            G1.update_db_table("fcust_extended", "record", record, new string[] { "SRV2Type", eventType, "SRV2Loc", venueName, "SRV2Date", eventDate, "SRV2Time", startTime, "SRV2City", city + " " + state, "RV2DAYDATE", dow });
                        }
                    }
                    else
                    { // Must be a Viewing
                        if (String.IsNullOrWhiteSpace(visitation1))
                        {
                            G1.update_db_table("fcust_extended", "record", record, new string[] { "VIS1Type", eventType, "VIS1Loc", venueName, "VSTDATE", eventDate, "VSTSTART", startTime, "VST1Add", address, "VISDAYDATE", dow });
                        }
                        else
                        {
                            G1.update_db_table("fcust_extended", "record", record, new string[] { "VIS2Type", eventType, "VIS2Loc", venueName, "Vis2Date", eventDate, "Vis2TimeStart", startTime, "VIS2Add", address, "Vis2DayDate", dow });
                        }
                    }
                    importCount++;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("***ERROR*** Row " + i.ToString() + "\n" + ex.Message.ToUpper(), "Events Import Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                }
            }

            pBar.Dispose();
            pBar = null;

            f.Close();
            f.Dispose();
            f = null;

            SetupMain();

            MessageBox.Show("***INFO*** " + importCount.ToString() + " Record Imported!", "Imported Record Count Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
        }
        /***********************************************************************************************/
        public static void Import_Veteran( string what = "" )
        {
            workWhat = what.ToUpper();
            string title = "SMFS";
            if (what == "SMART")
                title = "Smart Director";

            Import importVeteranForm = new Import(" " + title + " Veteran Information");
            importVeteranForm.SelectDone += ImportVeteranForm_SelectDone;
            importVeteranForm.Show();
        }
        /***********************************************************************************************/
        private static void ImportVeteranForm_SelectDone(System.Data.DataTable dt)
        {
            SetupPassare();

            string oldServiceId = "";
            string serviceId = "";
            string contractNumber = "";
            string cmd = "";
            string record = "";

            string contract = "";
            string trust = "";
            string loc = "";
            string str = "";

            int year = 0;


            DataTable dd = null;
            DataTable dx = null;
            string[] Lines = null;

            int lastRow = dt.Rows.Count;
            //lastRow = 1;

            ProgressBar pBar = new ProgressBar();
            pBar.Location = new System.Drawing.Point(0, 0);
            pBar.Name = "progressBar1";
            pBar.Width = 200;
            pBar.Height = 30;
            //pBar.Dock = DockStyle.Fill;

            Form f = new Form();
            f.StartPosition = FormStartPosition.CenterScreen;
            f.Bounds = new Rectangle(0, 0, 200, 100);
            f.Controls.Add(pBar);
            f.Show();
            f.Refresh();
            pBar.Visible = true;
            pBar.Refresh();

            pBar.Minimum = 0;
            pBar.Maximum = lastRow;
            pBar.Value = 0;
            pBar.Show();
            pBar.Refresh();

            int importCount = 0;

            string militaryService = "";
            string militaryBranch = "";
            string dateOfEnlistment = "";
            string dateOfDischarge = "";

            string isVet = "";
            string military = "";

            DateTime myDate = DateTime.Now;

            for (int i = 0; i < lastRow; i++)
            {
                Application.DoEvents();
                try
                {
                    pBar.Value = i + 1;
                    pBar.Refresh();

                    serviceId = GetWhat(dt, i, "Case Number");
                    //serviceId = dt.Rows[i]["Case Number"].ObjToString();
                    if (String.IsNullOrWhiteSpace(serviceId))
                        continue;
                    if (serviceId.ToUpper() == "N/A")
                        continue;
                    if (serviceId.ToUpper() == "TEST CASE")
                        continue;

                    contract = Trust85.decodeContractNumber(serviceId, ref trust, ref loc);
                    year = 0;
                    if (contract.Length > 2)
                    {
                        str = contract.Substring(0, 2);
                        year = str.ObjToInt32();
                    }
                    if (year <= 0 || year >= 23)
                    {
                        continue;
                    }

                    if (String.IsNullOrWhiteSpace(oldServiceId))
                        oldServiceId = serviceId;

                    if (oldServiceId != serviceId)
                    {
                        contractNumber = "";
                        //break;
                    }

                    if (String.IsNullOrWhiteSpace(contractNumber))
                    {
                        cmd = "Select * from `fcustomers` where `serviceId` = '" + serviceId + "';";
                        dd = G1.get_db_data(cmd);
                        if (dd.Rows.Count <= 0)
                        {
                            MessageBox.Show("***ERROR*** Row " + i.ToString() + " ServiceId=" + serviceId + " Does Not Exist!!", "Bad Service ID Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                            break;
                        }
                        contractNumber = dd.Rows[0]["contractNumber"].ObjToString();
                        if (String.IsNullOrWhiteSpace(contractNumber))
                            continue;
                    }

                    cmd = "Select * from `fcust_extended` where `contractNumber` = '" + contractNumber + "';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count <= 0)
                        continue;
                    record = dx.Rows[0]["record"].ObjToString();

                    isVet = "No";
                    military = "";
                    dateOfEnlistment = "";
                    dateOfDischarge = "";

                    militaryService = GetWhat(dt, i, "Military Service");
                    //militaryService = dt.Rows[i]["Military Service"].ObjToString();
                    if (militaryService.ToUpper() == "TRUE")
                    {
                        militaryBranch = GetWhat(dt, i, "Branch of Service");
                        //militaryBranch = dt.Rows[i]["Branch of Service"].ObjToString();
                        if ( !String.IsNullOrWhiteSpace ( militaryBranch ))
                        {
                            military = militaryBranch;
                            isVet = "Yes";
                            dateOfEnlistment = GetWhat(dt, i, "Date of Enlistment");
                            //dateOfEnlistment = dt.Rows[i]["Date of Enlistment"].ObjToString();

                            dateOfDischarge = GetWhat(dt, i, "Date of Discharge");
                            //dateOfDischarge = dt.Rows[i]["Date of Discharge"].ObjToString();
                        }
                    }
                    G1.update_db_table("fcust_extended", "record", record, new string[] { "Branch of Military Service", military, "Is Veteran", isVet });

                    importCount++;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("***ERROR*** Row " + i.ToString() + "\n" + ex.Message.ToUpper(), "Military Import Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                }
            }

            pBar.Dispose();
            pBar = null;

            f.Close();
            f.Dispose();
            f = null;

            SetupMain();

            MessageBox.Show("***INFO*** " + importCount.ToString() + " Record Imported!", "Imported Record Count Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
        }
        /***********************************************************************************************/
        public static void FixBadContractNumbers( string table, bool checkDuplicates = true )
        {
            if (String.IsNullOrWhiteSpace(table))
                return;

            SetupPassare();

            string contractNumber = "";
            string newContract = "";
            string cmd = "";
            string record = "";

            string contract = "";
            string trust = "";
            string loc = "";
            string str = "";

            int year = 0;

            cmd = "Select * from `" + table + "`;";
            DataTable dt = G1.get_db_data(cmd);
            if ( dt.Rows.Count <= 0 )
            {
                return;
            }

            int lastRow = dt.Rows.Count;
            //lastRow = 1;

            ProgressBar pBar = new ProgressBar();
            pBar.Location = new System.Drawing.Point(0, 0);
            pBar.Name = "progressBar1";
            pBar.Width = 200;
            pBar.Height = 30;
            //pBar.Dock = DockStyle.Fill;

            Form f = new Form();
            f.StartPosition = FormStartPosition.CenterScreen;
            f.Bounds = new Rectangle(0, 0, 200, 100);
            f.Controls.Add(pBar);
            f.Show();
            f.Refresh();
            pBar.Visible = true;
            pBar.Refresh();

            pBar.Minimum = 0;
            pBar.Maximum = lastRow;
            pBar.Value = 0;
            pBar.Show();
            pBar.Refresh();

            DataTable dx = null;

            int importCount = 0;

            for (int i = 0; i < lastRow; i++)
            {
                Application.DoEvents();
                try
                {
                    pBar.Value = i + 1;
                    pBar.Refresh();

                    record = dt.Rows[i]["record"].ObjToString();
                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString();

                    newContract = decodeBadContractNumber(contractNumber);

                    if (newContract == contractNumber)
                        continue;
                    if (checkDuplicates)
                    {
                        cmd = "Select * from `" + table + "` WHERE `contractNumber` = '" + newContract + "';";
                        dx = G1.get_db_data(cmd);

                        if (dx.Rows.Count <= 0)
                            G1.update_db_table(table, "record", record, new string[] { "contractNumber", newContract });
                    }
                    else
                        G1.update_db_table(table, "record", record, new string[] { "contractNumber", newContract });

                    importCount++;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("***ERROR*** Row " + i.ToString() + "\n" + ex.Message.ToUpper(), "Fix Bad Contract Number Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                }
            }

            pBar.Dispose();
            pBar = null;

            f.Close();
            f.Dispose();
            f = null;

            SetupMain();

            MessageBox.Show("***INFO*** " + importCount.ToString() + " Contracts Corrected!", "Contracts Corrected Count Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
        }
        /*******************************************************************************************/
        public static string decodeBadContractNumber(string contract)
        {
            string loc = "";
            string year = "";
            string c = "";
            if (contract.Length < 5)
            {
                MessageBox.Show("***ERROR*** " + contract + " BAD CONTRACT LENGTH!", "Contracts Corrected Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return contract;
            }
            year = contract.Substring(0, 2);
            if (!G1.validate_numeric(year))
            {
                //MessageBox.Show("***ERROR*** " + contract + " BAD CONTRACT YEAR!", "Contracts Corrected Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return contract;
            }
            contract = contract.Substring(2);

            for (int j = 0; j < contract.Length; j++)
            {
                c = contract.Substring(j, 1);
                if (G1.validate_numeric(c))
                    break;
                loc += c;
            }
            contract = contract.Replace(loc, "");
            string newContract = loc + year + contract;
            return newContract;
        }
        /*******************************************************************************************/
        public static string decodeServiceId(string serviceId)
        {
            string loc = "";
            string year = "";
            string c = "";

            for (int j = 0; j < serviceId.Length; j++)
            {
                c = serviceId.Substring(j, 1);
                if (G1.validate_numeric(c))
                    break;
                loc += c;
            }
            serviceId = serviceId.Replace(loc, "").Trim();
            string newContract = loc + serviceId;
            return newContract;
        }
        /***********************************************************************************************/
    }
}
