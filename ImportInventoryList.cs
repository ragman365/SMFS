using System;
using System.Data;
using System.Windows.Forms;
using System.IO;
using DevExpress.Utils;
using org.apache.pdfbox.pdmodel;
using org.apache.pdfbox.util;
using GeneralLib;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class ImportInventoryList : Form
    {
        private string workTitle = "";
        private string delimiter = ",";
        /***********************************************************************************************/
        public ImportInventoryList( string title )
        {
            workTitle = title;
            InitializeComponent();
        }
        /***********************************************************************************************/
        private void Import_Load(object sender, EventArgs e)
        {
            this.Text = workTitle;
            picLoader.Hide();
            labelMaximum.Hide();
            barImport.Hide();
            this.btnImportFile.Hide();
        }
        /***********************************************************************************************/
        private void AddNewColumn(DataTable dt, string name, string format, int width )
        {
            if (string.IsNullOrEmpty(format))
            {
                int col = G1.get_column_number(dt, name);
                if ( col < 0 )
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
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string file = ofd.FileName;
                    if (file.ToUpper().IndexOf(".PDF") > 0)
                    {
                        DataTable dt = PullPDF(file);
                        if (dt != null && dt.Rows.Count > 0)
                        {
                            dgv.DataSource = dt;
                            btnImportFile.Show();
                        }
                    }
                    else
                    {
                        DataTable dt = ImportCSVfile(file);
                        if (dt != null && dt.Rows.Count > 0)
                        {
                            if (workTitle.Trim().ToUpper() == "IMPORT SALES REPORT")
                                MatchOnHand(dt);

                            dgv.DataSource = dt;
                            btnImportFile.Show();
                        }
                    }
                }
            }
        }
        /***********************************************************************************************/
        private void MatchOnHand ( DataTable dt )
        {
            this.Cursor = Cursors.WaitCursor;
            string cmd = "";
            string accountcode = "";
            string casketdesc = "";
            DataTable dx = null;
            string homeRecord = "";
            string funeralHomeName = "";
            string casketRecord = "";
            int minimum = 0;
            int actual = 0;
            int needed = 0;

            if (G1.get_column_number(dt, "minimumOnHand") <= 0)
                dt.Columns.Add("minimumOnHand", Type.GetType("System.Int32"));
            if (G1.get_column_number(dt, "actualOnHand") <= 0)
                dt.Columns.Add("actualOnHand", Type.GetType("System.Int32"));
            if (G1.get_column_number(dt, "Needed") <= 0)
                dt.Columns.Add("Needed", Type.GetType("System.Int32"));


            for ( int i=0; i<dt.Rows.Count; i++)
            {
                accountcode = dt.Rows[i]["Ship to Address Book Number"].ObjToString();
                if (String.IsNullOrWhiteSpace(accountcode))
                    continue;
                casketdesc = dt.Rows[i]["Desc Ln 1"].ObjToString();
                if (String.IsNullOrWhiteSpace(casketdesc))
                    continue;
                casketdesc = G1.protect_data(casketdesc);
                cmd = "Select * from `funeralhomes` where `accountcode` = '" + accountcode + "';";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count <= 0)
                    continue;
                homeRecord = dx.Rows[0]["record"].ObjToString();
                funeralHomeName = dx.Rows[0]["LocationCode"].ObjToString();

                cmd = "Select * from `inventorylist` where `casketdesc` = '" + casketdesc + "';";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count <= 0)
                    continue;
                casketRecord = dx.Rows[0]["record"].ObjToString();

                cmd = "Select * from `inventory_on_hand` where `!casketRecord` = '" + casketRecord + "' and `!homeRecord` = '" + homeRecord + "';";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count <= 0)
                    continue;
                minimum = dx.Rows[0]["minimumOnHand"].ObjToInt32();
                dt.Rows[i]["minimumOnHand"] = minimum;
                actual = GetActualOnHand(funeralHomeName, casketdesc);
                dt.Rows[i]["actualOnHand"] = actual;
                needed = minimum - actual;
                dt.Rows[i]["Needed"] = needed;
            }
            this.Cursor = this.DefaultCursor;
        }
        /***********************************************************************************************/
        public static int GetActualOnHand ( string funeralHomeName, string casketDescription, string ownerShip = "")
        {
            int actualOnHand = 0;
            bool notEqual = false;
            if ( ownerShip.IndexOf ( "!") == 0 )
            {
                ownerShip = ownerShip.Replace("!", "");
                notEqual = true;
            }
            string cmd = "Select Count(*) from `inventory` where `LocationCode` = '" + funeralHomeName + "' ";
            cmd += " and `CasketDescription` = '" + casketDescription + "' ";
            if (!String.IsNullOrWhiteSpace(ownerShip))
            {
                if ( notEqual )
                    cmd += " and `Ownership` <> '" + ownerShip + "' ";
                else
                    cmd += " and `Ownership` = '" + ownerShip + "' ";
            }
            cmd += " and `ServiceID` = '' and `del` <> '1' ";
            cmd += ";";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                actualOnHand = dx.Rows[0][0].ObjToInt32();
            }
            return actualOnHand;
        }
        /***********************************************************************************************/
        private DataTable PullPDF ( string filename )
        {
            if (!File.Exists(filename))
                return null;
            DataTable dt = new DataTable();
            dt.Columns.Add("num");
            dt.Columns.Add("Case Identifier");
            dt.Columns.Add("Decedent Full Name");
            dt.Columns.Add("Service Date - First");
            dt.Columns.Add("Item Name");
            dt.Columns.Add("Container 1 Serial Number");
            dt.Columns.Add("Secondary Arranger");
            dt.Columns.Add("Decedent Date of Death");
            PDDocument doc = null;
            try
            {
                string str = "";
                string field = "";
                string date1 = "";
                string date2 = "";
                string serialNumber = "";
                string serviceId = "";
                bool gotNumber = false;
                doc = PDDocument.load(filename);
                PDFTextStripper stripper = new PDFTextStripper();
                string text = stripper.getText(doc);
                string[] Lines = text.Split('\n');
                for (int i = 0; i < Lines.Length; i++)
                {
                    if (Lines[i].ToUpper().IndexOf("CASE IDENTIFIER") >= 0)
                    {
                        gotNumber = false;
                        continue;
                    }
                    str = Lines[i].Trim();
                    string[] lines = Lines[i].Split(' ');
                    if (gotNumber)
                    {
                        serialNumber = lines[0].Trim();
                        if (!G1.validate_numeric(serialNumber))
                            serialNumber = "";
                        gotNumber = false;
                    }
                    for (int j = 0; j < lines.Length; j++)
                    {
                        field = lines[j].Trim();
                        if (G1.validate_date(field))
                        {
                            if (String.IsNullOrWhiteSpace(date1))
                            {
                                serviceId = lines[0].Trim();
                                date1 = field;
                                j++; // Move Past the Time
                                continue;
                            }
                            else
                            {
                                if ( String.IsNullOrWhiteSpace ( serialNumber))
                                {
                                    date1 = "";
                                    serviceId = "";
                                }
                                else
                                    date2 = field;
                                break;
                            }
                        }
                        if (lines[j].ToUpper().Trim() == "SERIALNUMBER:")
                        {
                            if (j < (lines.Length - 1))
                                serialNumber = lines[j + 1].Trim();
                            else
                                gotNumber = true;
                        }
                        else if (lines[j].ToUpper().Trim() == "NUMBER:")
                        {
                            if (j < (lines.Length - 1))
                                serialNumber = lines[j + 1].Trim();
                            else
                            {
                                gotNumber = true;
                                if ( String.IsNullOrWhiteSpace ( serviceId))
                                {
                                    string[] l = Lines[i - 1].Split(' ');
                                    serviceId = l[0];
                                    if (String.IsNullOrWhiteSpace(date1))
                                        date1 = "01/01/1999";
                                }
                            }
                        }
                    }
                    if (gotNumber)
                        continue;
                    bool avoid1 = false;
                    bool avoid2 = false;
                    bool avoid3 = false;
                    bool avoid4 = false;
                    if (String.IsNullOrWhiteSpace(serviceId))
                        avoid1 = true;
                    if (String.IsNullOrWhiteSpace(date1))
                        avoid2 = true;
                    if (String.IsNullOrWhiteSpace(date2))
                        avoid3 = true;
                    if (String.IsNullOrWhiteSpace(serialNumber))
                        avoid4 = true;
                    if (avoid1 && avoid2 && avoid3 && avoid4 )
                    {
                        serviceId = "";
                        date1 = "";
                        date2 = "";
                        serialNumber = "";
                        continue;
                    }
                    if ( !avoid1 && !avoid2 )
                    {
                        if (avoid3)
                            continue;
                    }
                    if ( !avoid1 && !avoid2 && !avoid3 )
                    {
                        if (avoid4)
                            continue;
                    }
                    DataRow dRow = dt.NewRow();
                    dRow["Case Identifier"] = serviceId;
                    dRow["Service Date - First"] = date1;
                    dRow["Container 1 Serial Number"] = serialNumber;
                    dRow["Decedent Date of Death"] = date2;
                    dt.Rows.Add(dRow);
                    serviceId = "";
                    date1 = "";
                    date2 = "";
                    serialNumber = "";
                    gotNumber = false;
                }
            }
            finally
            {
                if (doc != null)
                {
                    doc.close();
                }
            }
            return dt;
        }
        /***********************************************************************************************/
        private void ImportCustomerFile ( DataTable dt )
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
        private string ParseOutNewDate(string date)
        {
            if (date == "0")
                return "00/00/0000";
            if ( date.Trim().Length < 8)
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
        private string ParseOutOldDate ( string date )
        {
            if (date == "0")
                return "00/00/0000";
            if (date.Trim().Length < 6)
                date = "0" + date;
            if ( date.Trim().Length < 6 )
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
        private void AddToTable ( DataTable dt, int row, DataRow dr, string dtName, string gridName, string dateType = "" )
        {
            string str = dt.Rows[row][dtName].ObjToString();
            if (dateType == "1" )
                str = ParseOutOldDate(str);
            else if (dateType == "2")
                str = ParseOutNewDate(str);
            dr[gridName] = str;
        }
        /***********************************************************************************************/
        private DataTable ImportCSVfile(string filename)
        {
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
                int columnCount = 0;
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
                            columnCount = dt.Columns.Count;
                            continue;
                        }
//                        string[] Lines = line.Split(',');
                        G1.parse_answer_data(line, delimiter);
                        int count = G1.of_ans_count;
                        if (G1.of_ans_count <= columnCount)
                        {
                            DataRow dRow = dt.NewRow();
                            for (int i = 0; i < G1.of_ans_count; i++)
                            {
                                str = G1.of_answer[i].ObjToString().Trim();
                                str = Import.trim(str);
                                dRow[i + 1] = str;
                            }
                            dt.Rows.Add(dRow);
                        }
                        row++;
                    }
                    sr.Close();
                }
            }
            catch (Exception ex)
            {
                DevExpress.XtraEditors.XtraMessageBox.Show(ex.Message, "Error Occurred");
            }
            NumberDataTable(dt);
            picLoader.Hide();
            return dt;
        }
        /***********************************************************************************************/
        private void NumberDataTable(DataTable dt)
        {
            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                    dt.Rows[i]["num"] = (i + 1).ToString();
            }
            catch
            {
            }
        }
        /***********************************************************************************************/
        private DataTable BuildImportDt ( string line )
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Num");
            delimiter = ",";
            if ( line.IndexOf ( "\t") > 0 )
                delimiter = "\t";
            G1.parse_answer_data(line, delimiter );
            for ( int i=0; i<G1.of_ans_count; i++)
            {
                string name = G1.of_answer[i].ObjToString();
                name = Import.trim(name);
                if (String.IsNullOrEmpty(name))
                    name = "COL " + i.ToString();
                int col = G1.get_column_number(dt, name);
                if ( col < 0 )
                    dt.Columns.Add(name);
            }
            return dt;
        }
        /***********************************************************************************************/
        private bool checkDuplicateContract( string contract )
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
        public static bool VerifyImportColumns ( DataTable dt, string [] columns)
        {
            string column = "";
            string message = "";
            for ( int i=0; i<columns.Length; i++)
            {
                column = columns[i].Trim().ToUpper();
                if (G1.get_column_number(dt, column) < 0)
                    message += column + "\n";
            }
            if (String.IsNullOrWhiteSpace(message))
                return true;
            else
            {
                MessageBox.Show("***ERROR*** Expected Import Column Names:\n" + message + "Make certain these column names are in the import file!");
                return false;
            }
        }
        /***********************************************************************************************/
        private void ImportInventory( DataTable dt )
        {
            if ( workTitle.Trim().ToUpper() == "IMPORT SALES REPORT")
            {
                ImportSalesReport(dt);
                return; 
            }

            if (!VerifyImportColumns(dt, new string[] { "serialnumber", "casketdescription", "datereceived", "locationcode", "ownership", "serviceid", "dateused" }))
                return;

            picLoader.Show();

            DataTable dx = null;
            string cmd = "";
            string record = "";
            string datereceived = "";
            string locationcode = "";
            string serialnumber = "";
            string casketdesc = "";
            string ownership = "";
            string serviceid = "";
            string dateused = "";
            bool created = false;
            string added = "1";
            bool found = false;

            labelMaximum.Show();
            labelMaximum.Text = "0";
            barImport.Show();
            int lastrow = dt.Rows.Count;
            int tableRow = 0;
                        //lastrow = 85;

            try
            {
                //                cmd = "LOAD DATA LOCAL INFILE 'C:/Users/Robby/Documents/SMFS/Inventory/InventoryList.csv' IGNORE INTO TABLE smfs.inventorylist CHARACTER SET utf8 FIELDS TERMINATED BY ',' OPTIONALLY ENCLOSED BY '\"' IGNORE 1 LINES;";
                //                DataTable dd = G1.get_db_data(cmd);

                barImport.Minimum = 0;
                barImport.Maximum = lastrow;
                for (int i = 0; i < lastrow; i++)
                {
//                    Application.DoEvents();
                    barImport.Value = i;
                    barImport.Refresh();
                    labelMaximum.Text = i.ToString();
                    labelMaximum.Refresh();
                    tableRow = i;
                    record = "";
                    created = false;
                    added = "1";
                    found = false;
                    serialnumber = dt.Rows[i]["serialnumber"].ObjToString();
                    cmd = "Select * from `inventory` where `serialnumber` = '" + serialnumber + "' ;";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                    {
                        record = dx.Rows[0]["record"].ObjToString();
                        found = true;
                    }
                    else
                    {
                        record = G1.create_record("inventory", "serviceid", "-1");
                        created = true;
                    }
                    if (string.IsNullOrWhiteSpace(record))
                    {
                        MessageBox.Show("***ERROR*** Creating Inventory Record! " + serialnumber + " Stopping!");
                        break;
                    }
                    else if (record == "-1")
                    {
                        MessageBox.Show("***ERROR*** Creating Inventory Record! " + serialnumber + " Stopping!");
                        break;
                    }

                    casketdesc = dt.Rows[i]["casketdescription"].ObjToString();
                    datereceived = dt.Rows[i]["datereceived"].ObjToString();
                    locationcode = dt.Rows[i]["locationcode"].ObjToString();
                    ownership = dt.Rows[i]["ownership"].ObjToString();
                    serviceid = dt.Rows[i]["serviceid"].ObjToString();
                    dateused = dt.Rows[i]["dateused"].ObjToString();
                    if (created)
                        added = "2";
                    if ( found )
                        G1.update_db_table("inventory", "record", record, new string[] { "Ownership", ownership, "ServiceID", serviceid, "DateUsed", dateused, "added", added, "LocationCode", locationcode });
                    else
                        G1.update_db_table("inventory", "record", record, new string[] { "CasketDescription", casketdesc, "DateReceived", datereceived, "LocationCode", locationcode, "SerialNumber", serialnumber, "Ownership", ownership, "ServiceID", serviceid, "DateUsed", dateused, "added", added });
                    picLoader.Refresh();
                }
                picLoader.Hide();
                barImport.Value = lastrow;
                MessageBox.Show("Inventory List Data Import of " + lastrow + " Rows Complete . . .");
            }
            catch (Exception ex)
            {
                picLoader.Hide();
                MessageBox.Show("***ERROR*** Creating Inventory List Record/Row! " + datereceived + "/" + tableRow.ToString() + " Stopping! " + ex.Message.ObjToString());
            }
        }
        /***********************************************************************************************/
        private void btnImportFile_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (workTitle.Trim().ToUpper() == "IMPORT SALES REPORT")
            {
                if ( G1.get_column_number(dt, "Ship to Address Book Number") <= 0 )
                {
                    MessageBox.Show("***ERROR*** Imported Data does not appear to be Sale Report File! Missing 'Ship to Address Book Number'!");
                    return;
                }
                ImportSalesReport(dt);
                return;
            }
            int col = G1.get_column_number(dt, "serviceid");
            if (col > 0)
                ImportInventory(dt);
            else
            {
                MessageBox.Show("***ERROR*** Imported Data does not appear to be Inventory File!");
            }
        }
        /***********************************************************************************************/
        private string GetSQLDate ( DataTable dt, int row, string columnName )
        {
            string date = dt.Rows[row][columnName].ObjToString();
            string sql_date = G1.date_to_sql(date).Trim();
            if (sql_date == "0001-01-01")
                sql_date = "0000-00-00";
            return sql_date;
        }
        /***********************************************************************************************/
        private void ImportSalesReport(DataTable dt)
        {
            if (!VerifyImportColumns(dt, new string[] { "serial #", "Ship to Address Book Number", "Desc Ln 1", "Dt Invc" }))
                return;

            picLoader.Show();

            DataTable dx = null;
            string cmd = "";
            string record = "";
            string contractNumber = "";
            string dateReceived = "";
            string locationcode = "";
            string accountcode = "";
            string serialnumber = "";
            string loadedSerialNumber = "";
            string casketdesc = "";
            string ownership = "";
            string serviceid = "";
            string dateused = "";
            double grossAmt = 0D;
            double discount = 0D;
            double netAmount = 0D;
            double surcharge = 0D;
            bool created = false;
            string added = "1";
            DateTime serviceDate = DateTime.MinValue;
            DateTime deceasedDate = DateTime.MinValue;

            DataTable funDt = null;
            DataTable exDt = null;
            DataTable custDt = null;

            labelMaximum.Show();
            labelMaximum.Text = "0";
            barImport.Show();
            int lastrow = dt.Rows.Count;
            int tableRow = 0;
            int newrecords = 0;
            int oldrecords = 0;

            DataTable surDt = LoadSurchargeTable();

            //lastrow = 85;

            try
            {
                //                cmd = "LOAD DATA LOCAL INFILE 'C:/Users/Robby/Documents/SMFS/Inventory/InventoryList.csv' IGNORE INTO TABLE smfs.inventorylist CHARACTER SET utf8 FIELDS TERMINATED BY ',' OPTIONALLY ENCLOSED BY '\"' IGNORE 1 LINES;";
                //                DataTable dd = G1.get_db_data(cmd);

                barImport.Minimum = 0;
                barImport.Maximum = lastrow;
                int importCount = 0;
                for (int i = 0; i < lastrow; i++)
                {
                    //                    Application.DoEvents();
                    barImport.Value = i;
                    barImport.Refresh();
                    labelMaximum.Text = i.ToString();
                    labelMaximum.Refresh();
                    tableRow = i;
                    record = "";
                    created = false;
                    added = "1";
                    serialnumber = dt.Rows[i]["Serial #"].ObjToString();
                    if (String.IsNullOrWhiteSpace(serialnumber))
                        continue;
                    accountcode = dt.Rows[i]["Ship to Address Book Number"].ObjToString();
                    if (String.IsNullOrWhiteSpace(accountcode))
                        continue;
                    cmd = "Select * from `funeralhomes` where `accountcode` = '" + accountcode + "';";
                    dx = G1.get_db_data(cmd);
                    if ( dx.Rows.Count <= 0 )
                    {
                        MessageBox.Show("***ERROR*** Cannot Locate Funeral Home with AccountCode " + accountcode + "!\nFix Data Somewhere and rerun!");
                        continue;
                    }
                    locationcode = dx.Rows[0]["LocationCode"].ObjToString();
                    casketdesc = dt.Rows[i]["Desc Ln 1"].ObjToString();
                    dateReceived = dt.Rows[i]["Dt Invc"].ObjToString();

                    surcharge = GetSurcharge(surDt, dateReceived.ObjToDateTime());

                    grossAmt = dt.Rows[i]["Amt Grs"].ObjToDouble();
                    discount = dt.Rows[i]["Amt Disc Avl"].ObjToDouble();
                    netAmount = grossAmt - discount - surcharge;
                    if (netAmount < 0D)
                        netAmount = 0D;

                    string poNumber = dt.Rows[i]["Customer PO"].ObjToString();


                    UpdatePendingOrders(locationcode, casketdesc, serialnumber, poNumber );
                    //if (1 == 1)
                    //    continue;

                    cmd = "DELETE from `inventory` where `LocationCode` = '-1';"; // Tnis is to correct any past failures
                    G1.get_db_data(cmd);

                    cmd = "DELETE from `inventory` where `serialnumber` = '-1';";
                    G1.get_db_data(cmd);

                    cmd = "DELETE from `inventory` where `serviceid` = '-1';";
                    G1.get_db_data(cmd);

                    cmd = "Select * from `inventory` where `serialnumber` = '" + serialnumber + "' ;";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                    {
                        record = dx.Rows[0]["record"].ObjToString();
                        oldrecords++;
                    }
                    else
                    {
                        record = G1.create_record("inventory", "serviceid", "-1");
                        created = true;
                        newrecords++;
                    }
                    if (string.IsNullOrWhiteSpace(record))
                    {
                        MessageBox.Show("***ERROR*** Creating Inventory Record! " + serialnumber + " Stopping!");
                        break;
                    }
                    else if (record == "-1")
                    {
                        MessageBox.Show("***ERROR*** Creating Inventory Record! " + serialnumber + " Stopping!");
                        break;
                    }

                    serviceid = "";
                    dateused = "";

                    //serviceid = dt.Rows[i]["Customer PO"].ObjToString();
                    //if ( !String.IsNullOrWhiteSpace ( serviceid ))
                    //{
                    //    cmd = "Select * from `fcust_extended` where `serviceId` = '" + serviceid + "';";
                    //    exDt = G1.get_db_data(cmd);
                    //    if (exDt.Rows.Count > 0)
                    //    {
                    //        contractNumber = exDt.Rows[0]["contractNumber"].ObjToString();
                    //        cmd = "Select * from `fcustomers` where `contractNumber` = '" + contractNumber + "';";
                    //        custDt = G1.get_db_data(cmd);
                    //        if (custDt.Rows.Count > 0)
                    //            deceasedDate = custDt.Rows[0]["deceasedDate"].ObjToDateTime();

                    //        cmd = "Select * from `fcust_services` where `contractNumber` = '" + contractNumber + "' AND `serialNumber` <> '';";
                    //        funDt = G1.get_db_data(cmd);
                    //        if (funDt.Rows.Count > 0)
                    //        {
                    //            loadedSerialNumber = funDt.Rows[0]["serialNumber"].ObjToString();
                    //            if (loadedSerialNumber != serialnumber)
                    //            {
                    //                // DialogResult result = MessageBox.Show("***ERROR*** Serial Number " + serialnumber + "\nmismatch " + loadedSerialNumber + " ServiceId " + serviceid + "!\nMake a Note!", "Serial Number Mismatch", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    //                serviceid = "";
                    //            }
                    //            else
                    //            {
                    //                serviceDate = exDt.Rows[0]["serviceDate"].ObjToDateTime();
                    //                if ( serviceDate.Year > 100 )
                    //                    dateused = serviceDate.ToString("yyyy-MM-dd");
                    //            }
                    //        }
                    //        else
                    //            serviceid = "";
                    //    }
                    //    else
                    //        serviceid = "";
                    //}
                    if ( String.IsNullOrWhiteSpace ( serviceid ))
                    {
                        cmd = "Select * from `fcust_services` where `serialNumber` = '" + serialnumber + "';";
                        funDt = G1.get_db_data(cmd);
                        if ( funDt.Rows.Count > 0 )
                        {
                            contractNumber = funDt.Rows[0]["contractNumber"].ObjToString();
                            cmd = "Select * from `fcust_extended` where `contractNumber` = '" + contractNumber + "';";
                            exDt = G1.get_db_data(cmd);
                            if ( exDt.Rows.Count > 0 )
                            {
                                serviceid = exDt.Rows[0]["serviceId"].ObjToString();
                                serviceDate = exDt.Rows[0]["serviceDate"].ObjToDateTime();
                                if (serviceDate.Year > 100)
                                    dateused = serviceDate.ToString("yyyy-MM-dd");

                                cmd = "Select * from `fcustomers` where `contractNumber` = '" + contractNumber + "';";
                                custDt = G1.get_db_data(cmd);
                                if (custDt.Rows.Count > 0)
                                    deceasedDate = custDt.Rows[0]["deceasedDate"].ObjToDateTime();
                            }
                        }
                    }
                    casketdesc = dt.Rows[i]["Desc Ln 1"].ObjToString();
                    dateReceived = dt.Rows[i]["Dt Invc"].ObjToString();
                    DateTime date = dateReceived.ObjToDateTime();
                    dateReceived = G1.DTtoSQLString(date);

                    ownership = "";
                    if (created)
                        added = "2";

                    if (created)
                    {
                        if (!String.IsNullOrWhiteSpace(dateused) && G1.validate_date(dateused))
                            G1.update_db_table("inventory", "record", record, new string[] { "CasketDescription", casketdesc, "DateReceived", dateReceived, "DateUsed", dateused, "deceasedDate", deceasedDate.ToString("yyyy-MM-dd"), "LocationCode", locationcode, "SerialNumber", serialnumber, "ServiceID", serviceid, "added", added, "del", "", "gross", grossAmt.ToString(), "discount", discount.ToString(), "net", netAmount.ToString() });
                        else
                            G1.update_db_table("inventory", "record", record, new string[] { "CasketDescription", casketdesc, "DateReceived", dateReceived, "LocationCode", locationcode, "SerialNumber", serialnumber, "ServiceID", serviceid, "added", added, "del", "", "gross", grossAmt.ToString(), "discount", discount.ToString(), "net", netAmount.ToString() });
                    }
                    else
                    {
                        if (!String.IsNullOrWhiteSpace(dateused) && G1.validate_date(dateused))
                            G1.update_db_table("inventory", "record", record, new string[] { "CasketDescription", casketdesc, "DateReceived", dateReceived, "DateUsed", dateused, "deceasedDate", deceasedDate.ToString("yyyy-MM-dd"), "LocationCode", locationcode, "SerialNumber", serialnumber, "ServiceID", serviceid, "added", added, "gross", grossAmt.ToString(), "discount", discount.ToString(), "net", netAmount.ToString() });
                        else
                            G1.update_db_table("inventory", "record", record, new string[] { "CasketDescription", casketdesc, "DateReceived", dateReceived, "LocationCode", locationcode, "SerialNumber", serialnumber, "ServiceID", serviceid, "added", added, "gross", grossAmt.ToString(), "discount", discount.ToString(), "net", netAmount.ToString() });
                    }
                    importCount++;
                    picLoader.Refresh();
                }
                picLoader.Hide();
                barImport.Value = lastrow;
                MessageBox.Show("Inventory List Data Import of " + importCount + " Records, " + newrecords.ToString() + " New and " + oldrecords + " Old . . .");
                if ( AddHomeNew.addHomeFormNew != null)
                {
                    AddHomeNew.addHomeFormNew.FireEventInventoryImported();
                }
            }
            catch (Exception ex)
            {
                picLoader.Hide();
                MessageBox.Show("***ERROR*** Creating Inventory List Record/Row! " + dateReceived + "/" + tableRow.ToString() + " Stopping! " + ex.Message.ObjToString());
            }
        }
        /***********************************************************************************************/
        private void ImportSalesReportx(DataTable dt)
        {
            if (!VerifyImportColumns(dt, new string[] { "serial #", "Ship to Address Book Number", "Desc Ln 1", "Dt Invc" }))
                return;

            picLoader.Show();

            DataTable dx = null;
            string cmd = "";
            string record = "";
            string contractNumber = "";
            string dateReceived = "";
            string locationcode = "";
            string accountcode = "";
            string serialnumber = "";
            string loadedSerialNumber = "";
            string casketdesc = "";
            string ownership = "";
            string serviceid = "";
            string dateused = "";
            double grossAmt = 0D;
            double discount = 0D;
            double netAmount = 0D;
            bool created = false;
            string added = "1";
            DateTime serviceDate = DateTime.MinValue;
            DateTime deceasedDate = DateTime.MinValue;

            DataTable funDt = null;
            DataTable exDt = null;
            DataTable custDt = null;

            labelMaximum.Show();
            labelMaximum.Text = "0";
            barImport.Show();
            int lastrow = dt.Rows.Count;
            int tableRow = 0;
            int newrecords = 0;
            int oldrecords = 0;
            //lastrow = 85;

            try
            {
                //                cmd = "LOAD DATA LOCAL INFILE 'C:/Users/Robby/Documents/SMFS/Inventory/InventoryList.csv' IGNORE INTO TABLE smfs.inventorylist CHARACTER SET utf8 FIELDS TERMINATED BY ',' OPTIONALLY ENCLOSED BY '\"' IGNORE 1 LINES;";
                //                DataTable dd = G1.get_db_data(cmd);

                barImport.Minimum = 0;
                barImport.Maximum = lastrow;
                int importCount = 0;
                for (int i = 0; i < lastrow; i++)
                {
                    //                    Application.DoEvents();
                    barImport.Value = i;
                    barImport.Refresh();
                    labelMaximum.Text = i.ToString();
                    labelMaximum.Refresh();
                    tableRow = i;
                    record = "";
                    created = false;
                    added = "1";
                    serialnumber = dt.Rows[i]["Serial #"].ObjToString();
                    if (String.IsNullOrWhiteSpace(serialnumber))
                        continue;
                    accountcode = dt.Rows[i]["Ship to Address Book Number"].ObjToString();
                    if (String.IsNullOrWhiteSpace(accountcode))
                        continue;
                    cmd = "Select * from `funeralhomes` where `accountcode` = '" + accountcode + "';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count <= 0)
                    {
                        MessageBox.Show("***ERROR*** Cannot Locate Funeral Home with AccountCode " + accountcode + "!\nFix Data Somewhere and rerun!");
                        continue;
                    }
                    locationcode = dx.Rows[0]["LocationCode"].ObjToString();
                    casketdesc = dt.Rows[i]["Desc Ln 1"].ObjToString();

                    grossAmt = dt.Rows[i]["Amt Grs"].ObjToDouble();
                    discount = dt.Rows[i]["Amt Disc Avl"].ObjToDouble();
                    netAmount = grossAmt - discount;
                    if (netAmount < 0D)
                        netAmount = 0D;


                    UpdatePendingOrders(locationcode, casketdesc, serialnumber);
                    //if (1 == 1)
                    //    continue;

                    cmd = "DELETE from `inventory` where `LocationCode` = '-1';"; // Tnis is to correct any past failures
                    G1.get_db_data(cmd);

                    cmd = "DELETE from `inventory` where `serialnumber` = '-1';";
                    G1.get_db_data(cmd);

                    cmd = "DELETE from `inventory` where `serviceid` = '-1';";
                    G1.get_db_data(cmd);

                    cmd = "Select * from `inventory` where `serialnumber` = '" + serialnumber + "' ;";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                    {
                        record = dx.Rows[0]["record"].ObjToString();
                        oldrecords++;
                    }
                    else
                    {
                        record = G1.create_record("inventory", "serviceid", "-1");
                        created = true;
                        newrecords++;
                    }
                    if (string.IsNullOrWhiteSpace(record))
                    {
                        MessageBox.Show("***ERROR*** Creating Inventory Record! " + serialnumber + " Stopping!");
                        break;
                    }
                    else if (record == "-1")
                    {
                        MessageBox.Show("***ERROR*** Creating Inventory Record! " + serialnumber + " Stopping!");
                        break;
                    }

                    serviceid = "";
                    dateused = "";

                    serviceid = dt.Rows[i]["Customer PO"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(serviceid))
                    {
                        cmd = "Select * from `fcust_extended` where `serviceId` = '" + serviceid + "';";
                        exDt = G1.get_db_data(cmd);
                        if (exDt.Rows.Count > 0)
                        {
                            contractNumber = exDt.Rows[0]["contractNumber"].ObjToString();
                            cmd = "Select * from `fcustomers` where `contractNumber` = '" + contractNumber + "';";
                            custDt = G1.get_db_data(cmd);
                            if (custDt.Rows.Count > 0)
                                deceasedDate = custDt.Rows[0]["deceasedDate"].ObjToDateTime();

                            cmd = "Select * from `fcust_services` where `contractNumber` = '" + contractNumber + "' AND `serialNumber` <> '';";
                            funDt = G1.get_db_data(cmd);
                            if (funDt.Rows.Count > 0)
                            {
                                loadedSerialNumber = funDt.Rows[0]["serialNumber"].ObjToString();
                                if (loadedSerialNumber != serialnumber)
                                {
                                    // DialogResult result = MessageBox.Show("***ERROR*** Serial Number " + serialnumber + "\nmismatch " + loadedSerialNumber + " ServiceId " + serviceid + "!\nMake a Note!", "Serial Number Mismatch", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                                    serviceid = "";
                                }
                                else
                                {
                                    serviceDate = exDt.Rows[0]["serviceDate"].ObjToDateTime();
                                    if (serviceDate.Year > 100)
                                        dateused = serviceDate.ToString("yyyy-MM-dd");
                                }
                            }
                            else
                                serviceid = "";
                        }
                        else
                            serviceid = "";
                    }
                    if (String.IsNullOrWhiteSpace(serviceid))
                    {
                        cmd = "Select * from `fcust_services` where `serialNumber` = '" + serialnumber + "';";
                        funDt = G1.get_db_data(cmd);
                        if (funDt.Rows.Count > 0)
                        {
                            contractNumber = funDt.Rows[0]["contractNumber"].ObjToString();
                            cmd = "Select * from `fcust_extended` where `contractNumber` = '" + contractNumber + "';";
                            exDt = G1.get_db_data(cmd);
                            if (exDt.Rows.Count > 0)
                            {
                                serviceid = exDt.Rows[0]["serviceId"].ObjToString();
                                serviceDate = exDt.Rows[0]["serviceDate"].ObjToDateTime();
                                if (serviceDate.Year > 100)
                                    dateused = serviceDate.ToString("yyyy-MM-dd");

                                cmd = "Select * from `fcustomers` where `contractNumber` = '" + contractNumber + "';";
                                custDt = G1.get_db_data(cmd);
                                if (custDt.Rows.Count > 0)
                                    deceasedDate = custDt.Rows[0]["deceasedDate"].ObjToDateTime();
                            }
                        }
                    }
                    casketdesc = dt.Rows[i]["Desc Ln 1"].ObjToString();
                    dateReceived = dt.Rows[i]["Dt Invc"].ObjToString();
                    DateTime date = dateReceived.ObjToDateTime();
                    dateReceived = G1.DTtoSQLString(date);

                    ownership = "";
                    if (created)
                        added = "2";

                    if (created)
                    {
                        if (!String.IsNullOrWhiteSpace(dateused) && G1.validate_date(dateused))
                            G1.update_db_table("inventory", "record", record, new string[] { "CasketDescription", casketdesc, "DateReceived", dateReceived, "DateUsed", dateused, "deceasedDate", deceasedDate.ToString("yyyy-MM-dd"), "LocationCode", locationcode, "SerialNumber", serialnumber, "ServiceID", serviceid, "added", added, "del", "", "gross", grossAmt.ToString(), "discount", discount.ToString(), "net", netAmount.ToString() });
                        else
                            G1.update_db_table("inventory", "record", record, new string[] { "CasketDescription", casketdesc, "DateReceived", dateReceived, "LocationCode", locationcode, "SerialNumber", serialnumber, "ServiceID", serviceid, "added", added, "del", "", "gross", grossAmt.ToString(), "discount", discount.ToString(), "net", netAmount.ToString() });
                    }
                    else
                    {
                        if (!String.IsNullOrWhiteSpace(dateused) && G1.validate_date(dateused))
                            G1.update_db_table("inventory", "record", record, new string[] { "CasketDescription", casketdesc, "DateReceived", dateReceived, "DateUsed", dateused, "deceasedDate", deceasedDate.ToString("yyyy-MM-dd"), "LocationCode", locationcode, "SerialNumber", serialnumber, "ServiceID", serviceid, "added", added, "gross", grossAmt.ToString(), "discount", discount.ToString(), "net", netAmount.ToString() });
                        else
                            G1.update_db_table("inventory", "record", record, new string[] { "CasketDescription", casketdesc, "DateReceived", dateReceived, "LocationCode", locationcode, "SerialNumber", serialnumber, "ServiceID", serviceid, "added", added, "gross", grossAmt.ToString(), "discount", discount.ToString(), "net", netAmount.ToString() });
                    }
                    importCount++;
                    picLoader.Refresh();
                }
                picLoader.Hide();
                barImport.Value = lastrow;
                MessageBox.Show("Inventory List Data Import of " + importCount + " Records, " + newrecords.ToString() + " New and " + oldrecords + " Old . . .");
                if (AddHomeNew.addHomeFormNew != null)
                {
                    AddHomeNew.addHomeFormNew.FireEventInventoryImported();
                }
            }
            catch (Exception ex)
            {
                picLoader.Hide();
                MessageBox.Show("***ERROR*** Creating Inventory List Record/Row! " + dateReceived + "/" + tableRow.ToString() + " Stopping! " + ex.Message.ObjToString());
            }
        }
        /***********************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (mainGrid.OptionsFind.AlwaysVisible == true)
                mainGrid.OptionsFind.AlwaysVisible = false;
            else
                mainGrid.OptionsFind.AlwaysVisible = true;
        }
        /***********************************************************************************************/
        private void UpdatePendingOrders ( string LocationCode, string CasketDescription, string serialNumber, string poNumber = "", string ownerShip = "" )
        {
            string casketCode = "";
            string[] Lines = CasketDescription.Split(' ');
            if (Lines.Length > 0)
                casketCode = Lines[0].Trim();

            string where = " where ";

            string record = "";
            string cmd = "Select * from `inventory_orders` where `LocationCode` = '" + LocationCode + "' ";
            cmd += " and `CasketDescription` = '" + CasketDescription + "' ";
            if (!String.IsNullOrWhiteSpace(ownerShip))
                cmd += " and `Ownership` = '" + ownerShip + "' ";
            cmd += " and `del` <> '1' ";
            if (!String.IsNullOrWhiteSpace(poNumber))
                cmd += " AND `replacement` = '" + poNumber + "' ";
            cmd += " ORDER BY `DateOrdered`, `tmstamp` ";
            cmd += ";";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0) // Order Found for Location
            {
                record = dx.Rows[0]["record"].ObjToString();
                G1.update_db_table("inventory_orders", "record", record, new string[] { "CasketDescription", CasketDescription, "serialNumber", serialNumber, "matched", "", "CasketCode", casketCode, "DateImported", DateTime.Now.ToString("MM/dd/yyyy") });
                // Now look for Field User Data Entered

                //cmd = "Select * from `inventory_orders` where `LocationCode` = '" + LocationCode + "' "; // Now look for serial number under delivered
                cmd = "Select * from `inventory_orders` "; // Now look for serial number under delivered but for any location
                if (!String.IsNullOrWhiteSpace(ownerShip))
                {
                    cmd += " " + where + " `Ownership` = '" + ownerShip + "' ";
                    where = " and ";
                }
                cmd += " " + where + " `del` <> '1' ";
                cmd += " AND `deliveredSerialNumber` = '" + serialNumber + "' ";
                cmd += " ORDER BY `DateOrdered`, `tmstamp` ";
                cmd += ";";
                dx = G1.get_db_data(cmd);
                if ( dx.Rows.Count > 0) // Found One! Now Match it up to original order and delete the delivered one
                {
                    DateTime delivereddate = dx.Rows[0]["DateDelivered"].ObjToDateTime();
                    string deliveredLocation = dx.Rows[0]["LocationCode"].ObjToString();
                    G1.update_db_table("inventory_orders", "record", record, new string[] { "DateDelivered", delivereddate.ToString("MM/dd/yyyy"), "matched", "MATCHED", "deliveredserialNumber", serialNumber, "qty", "1", "deliveredLoc", deliveredLocation });

                    string fieldRecord = dx.Rows[0]["record"].ObjToString();
                    G1.delete_db_table("inventory_orders", "record", fieldRecord); //  Must remove Field entered delivered order because it merged with original order
                }
                //else
                //{
                //    for (int i = 0; i < dx.Rows.Count; i++)
                //    {
                //        int pending = dx.Rows[i]["qtyPending"].ObjToInt32();
                //        if (pending > 0)
                //        {
                //            record = dx.Rows[i]["record"].ObjToString();
                //            pending = pending - 1;
                //            G1.update_db_table("inventory_orders", "record", record, new string[] { "qtyPending", pending.ToString(), "serialNumber", serialNumber, "DateImported", DateTime.Now.ToString("MM/dd/yyyy") });
                //            break;
                //        }
                //    }
                //}
            }
            else
            {
                //cmd = "Select * from `inventory_orders` where `LocationCode` = '" + LocationCode + "' ";
                where = " where ";
                cmd = "Select * from `inventory_orders` "; // Didn't find oreder for location, so now look for delivered order. Location does not matter here.
                if (!String.IsNullOrWhiteSpace(ownerShip))
                {
                    cmd += " " + where + " `Ownership` = '" + ownerShip + "' ";
                    where = " and ";
                }
                cmd += " " + where + " `del` <> '1' ";
                if (!String.IsNullOrWhiteSpace(serialNumber))
                    cmd += " AND `deliveredSerialNumber` = '" + serialNumber + "' ";
                cmd += " ORDER BY `DateOrdered`, `tmstamp` ";
                cmd += ";";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    record = dx.Rows[0]["record"].ObjToString();
                    string deliveredLocation = dx.Rows[0]["LocationCode"].ObjToString();
                    G1.update_db_table("inventory_orders", "record", record, new string[] { "CasketDescription", CasketDescription, "serialNumber", serialNumber, "matched", "MATCHED", "CasketCode", casketCode, "DateImported", DateTime.Now.ToString("MM/dd/yyyy"), "replacement", poNumber, "deliveredLoc", deliveredLocation });
                }
                else
                {
                    cmd = "Select * from `fcust_services` where `serialNumber` = '" + serialNumber + "';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                    {
                        record = G1.create_record("inventory_orders", "LocationCode", "-1");
                        if (G1.BadRecord("inventory_orders", record))
                            return;
                        DateTime d = DateTime.Now;
                        string orderdate = d.ToString("yyyy-MM-dd");
                        string user = LoginForm.username.Trim();

                        string accountcode = "";
                        string itemnumber = "";

                        G1.update_db_table("inventory_orders", "record", record, new string[] { "serialNumber", serialNumber, "qty", "1", "qtyPending", "1", "LocationCode", LocationCode, "CasketDescription", CasketDescription, "CasketCode", "" });
                        G1.update_db_table("inventory_orders", "record", record, new string[] { "Ownership", "Consigned", "orderedby", user, "DateOrdered", orderdate, "replacement", poNumber, "matched", "MISMAtCHED", "DateImported", DateTime.Now.ToString("MM/dd/yyyy") });
                    }
                    else
                    {
                        record = G1.create_record("inventory_orders", "LocationCode", "-1");
                        if (G1.BadRecord("inventory_orders", record))
                            return;
                        DateTime d = DateTime.Now;
                        string orderdate = d.ToString("yyyy-MM-dd");
                        string user = LoginForm.username.Trim();

                        string accountcode = "";
                        string itemnumber = "";

                        G1.update_db_table("inventory_orders", "record", record, new string[] { "serialNumber", serialNumber, "qty", "1", "qtyPending", "1", "LocationCode", LocationCode, "CasketDescription", CasketDescription, "CasketCode", "" });
                        G1.update_db_table("inventory_orders", "record", record, new string[] { "Ownership", "Consigned", "orderedby", user, "DateOrdered", orderdate, "replacement", poNumber, "matched", "MISMATCHED", "DateImported", DateTime.Now.ToString("MM/dd/yyyy") });
                    }
                }
            }
        }
        /***********************************************************************************************/
        private void FindAcctAndItem ( string locationCode, string casketDesc, ref string accountcode, ref string itemNumber )
        {
            accountcode = "";
            itemNumber = "";

            string cmd = "";
            DataTable dt = null;

            if (!String.IsNullOrWhiteSpace(locationCode))
            {
                cmd = "Select * from `funeralhomes` WHERE `LocationCode` = '" + locationCode + "';";
                dt = G1.get_db_data(cmd);
                if (dt.Rows.Count > 0)
                    accountcode = dt.Rows[0]["accountcode"].ObjToString();
            }

            if (!String.IsNullOrWhiteSpace(casketDesc))
            {
                cmd = "Select * from `inventorylist` where `caskeDesc` = '" + casketDesc + "';";
                dt = G1.get_db_data(cmd);
                if (dt.Rows.Count > 0)
                    itemNumber = dt.Rows[0]["itemnumber"].ObjToString();
            }
        }
        /***********************************************************************************************/
        public static DataTable LoadSurchargeTable()
        {
            DateTime date = DateTime.Now;
            DataTable dt = G1.get_db_data("Select * from `batesville_surcharges` ORDER BY `beginDate` DESC;");
            dt.Columns.Add("bDate");
            dt.Columns.Add("eDate");

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                date = dt.Rows[i]["endDate"].ObjToDateTime();
                if (date.Year <= 1000)
                    dt.Rows[i]["endDate"] = G1.DTtoMySQLDT(DateTime.Now);
            }

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                date = dt.Rows[i]["beginDate"].ObjToDateTime();
                dt.Rows[i]["bDate"] = date.ToString("yyyyMMdd");

                date = dt.Rows[i]["endDate"].ObjToDateTime();
                dt.Rows[i]["eDate"] = date.ToString("yyyyMMdd");
            }

            DataView tempview = dt.DefaultView;
            tempview.Sort = "endDate DESC";
            dt = tempview.ToTable();
            return dt;
        }
        /***********************************************************************************************/
        public static double GetSurcharge(DataTable surDt, DateTime dateReceived)
        {
            if (surDt == null)
                return 0D;
            if (surDt.Rows.Count <= 0)
                return 0D;

            double surcharge = 0D;
            try
            {
                DataRow[] dRows = surDt.Select("eDate>='" + dateReceived.ToString("yyyyMMdd") + "'");
                if (dRows.Length > 0)
                {
                    //DataTable dd = dRows.CopyToDataTable();
                    surcharge = dRows[0]["surcharge"].ObjToDouble();
                }
            }
            catch (Exception ex)
            {
            }
            return surcharge;
        }
        /***********************************************************************************************/
    }
}
