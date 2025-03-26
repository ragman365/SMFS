using System;
using System.Data;
using System.Windows.Forms;
using System.IO;
using DevExpress.Utils;

using GeneralLib;

using System.Drawing;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraPrinting;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class ImportCasketCosts : Form
    {
        private string importType = "";
        private bool gotTim = false;
        /***********************************************************************************************/
        public ImportCasketCosts( string type = "" )
        {
            importType = type;
            InitializeComponent();
        }
        /***********************************************************************************************/
        private void ImportCasketCosts_Load(object sender, EventArgs e)
        {
            picLoader.Hide();
            labelMaximum.Hide();
            lblTotal.Hide();
            barImport.Hide();

            this.btnImportFile.Hide();
            btnImportBatesville.Hide();
            if (importType.ToUpper() != "DELIMITED")
                btnTie.Hide();
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
            double cost = 0D;
            string str = "";
            DataTable dt = null;
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string file = ofd.FileName;

                    this.Cursor = Cursors.WaitCursor;
                    dt = ExcelWriter.ReadFile2(file, 0, "Sheet1");
                    if ( dt.Rows.Count > 0 )
                    {
                        //dt.Columns["Column26"].ColumnName = "casketCode";
                        //dt.Columns["Column27"].ColumnName = "casketDescription";
                        dt.Columns["Column2"].ColumnName = "itemNumber";
                        dt.Columns["Column3"].ColumnName = "casketDescription";
                        dt.Columns.Add("casketCost", Type.GetType("System.Double"));
                        dt.Columns.Add("oldCasketCost", Type.GetType("System.Double"));
                        dt.Columns.Add("Status");
                        dt.Columns.Add("casketCode");

                        dt.Rows.RemoveAt(0);

                        for ( int i=0; i<dt.Rows.Count; i++)
                        {
                            //str = dt.Rows[i]["Column28"].ObjToString();
                            str = dt.Rows[i]["Column12"].ObjToString();
                            cost = str.ObjToDouble();
                            dt.Rows[i]["casketCost"] = cost;
                        }

                        //for ( int i=(dt.Rows.Count-1); i>=0; i-- )
                        //{
                        //    str = dt.Rows[i]["casketCode"].ObjToString();
                        //    if ( String.IsNullOrWhiteSpace ( str ))
                        //    {
                        //        str = dt.Rows[i]["casketDescription"].ObjToString();
                        //        if (String.IsNullOrWhiteSpace(str))
                        //            dt.Rows.RemoveAt(i);
                        //    }
                        //}

                        G1.NumberDataTable(dt);
                        dgv.DataSource = dt;

                        btnImportFile.Show();
                        btnImportFile.Refresh();

                        btnImportBatesville.Show();
                        btnImportBatesville.Refresh();
                    }
                    this.Cursor = Cursors.Default;
                }
            }

            DataTable inventoryDt = G1.get_db_data("Select * from `inventorylist`;");
            if (inventoryDt.Rows.Count <= 0)
                return;
            DataTable casketDt = G1.get_db_data("Select * from `casket_master`;");
            if (casketDt.Rows.Count <= 0)
                return;

            string itemNumber = "";
            DataRow[] dRows = null;
            DataRow[] qRows = null;
            string casketCode = "";
            for ( int i=0; i<inventoryDt.Rows.Count; i++)
            {
                itemNumber = inventoryDt.Rows[i]["itemnumber"].ObjToString();
                if ( !String.IsNullOrWhiteSpace ( itemNumber ))
                {
                    dRows = dt.Select("itemNumber='" + itemNumber + "'");
                    if ( dRows.Length > 0 )
                    {
                        casketCode = inventoryDt.Rows[i]["casketcode"].ObjToString();
                        dRows[0]["casketcode"] = casketCode;
                        if (!String.IsNullOrWhiteSpace(casketCode))
                        {
                            qRows = casketDt.Select("casketcode='" + casketCode + "'");
                            if ( qRows.Length > 0 )
                            {
                                qRows[0]["itemnumber"] = itemNumber;
                                cost = qRows[0]["casketcost"].ObjToDouble();
                                dRows[0]["oldCasketCost"] = cost;
                            }
                        }

                    }
                }
            }
        }
        /***********************************************************************************************/
        private bool qualifyLineImport(DataTable dt, string line)
        {
            if (line.IndexOf("serial number:") > 0)
                return false;
            if (line.ToUpper().IndexOf("CASE IDENTIFIER") > 0)
                return false;
            if (line.ToUpper().IndexOf("DECEDENT FULL NAME") > 0)
                return false;
            string delimiter = ",";
            if (line.IndexOf("\t") > 0)
                delimiter = "\t";
            G1.parse_answer_data(line, delimiter);
            int count = G1.of_ans_count;
            if (count < 7)
                return false;
            bool valid = false;
            try
            {
                string serviceId = G1.of_answer[0].ObjToString().Trim();
                string serviceDate = G1.of_answer[2].ObjToString().Trim();
                serviceDate = serviceDate.Replace("am", "");
                serviceDate = serviceDate.Replace("pm", "");
                DateTime date = DateTime.Now;
                if (!G1.validate_date(serviceDate))
                    serviceDate = "BAD DATA";
                else
                {
                    date = serviceDate.ObjToDateTime();
                    serviceDate = date.Year.ToString("D4") + "-" + date.Month.ToString("D2") + "-" + date.Day.ToString("D2");
                }
                string serialNumber = G1.of_answer[4].ObjToString().Trim();
                if (!G1.validate_numeric(serialNumber))
                    serialNumber = "BAD DATA";
                string deceasedDate = G1.of_answer[6].ObjToString().Trim();
                deceasedDate = deceasedDate.Replace("am", "");
                deceasedDate = deceasedDate.Replace("pm", "");
                //if (!G1.validate_date(deceasedDate))
                //    return false;
                date = deceasedDate.ObjToDateTime();
                deceasedDate = date.Year.ToString("D4") + "-" + date.Month.ToString("D2") + "-" + date.Day.ToString("D2");
                DataRow dRow = dt.NewRow();
                string cmd = "Select * from `inventory` where `SerialNumber` = '" + serialNumber + "';";
                DataTable dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    dRow["casketRecord"] = dx.Rows[0]["record"].ObjToString();
                    if (checkUsage(dx, 0))
                        dRow["status"] = "DUPLICATE";
                }
                else
                    dRow["status"] = "NEW";
                dRow["serviceID"] = serviceId;
                dRow["Container 1 Serial Number"] = serialNumber;
                dRow["Service Date - First"] = serviceDate;
                dRow["Decedent Date of Death"] = deceasedDate;
                dRow["Decedent Full Name"] = G1.of_answer[1].ObjToString().Trim();
                dRow["Item Name"] = G1.of_answer[3].ObjToString().Trim();
                dRow["Secondary Arranger"] = G1.of_answer[5].ObjToString().Trim();
                dt.Rows.Add(dRow);
            }
            catch ( Exception ex)
            {
                return false;
            }
            return valid;
        }
        /***********************************************************************************************/
        private DataTable ImportFlatFile ( string filename )
        {
            //Case Identifier Decedent Full Name Service Date - First    Item Name   Container 1 Serial Number   Secondary Arranger
            string delimiter = ",";
            DataTable dt = new DataTable();
            dt.Columns.Add("Num");
            dt.Columns.Add("serviceID");
            dt.Columns.Add("Decedent Full Name");
            dt.Columns.Add("Service Date - First");
            dt.Columns.Add("Item Name");
            dt.Columns.Add("Container 1 Serial Number");
            dt.Columns.Add("Secondary Arranger");
            dt.Columns.Add("Decedent Date of Death");
            dt.Columns.Add("casketRecord");
            dt.Columns.Add("Status");
            string str = "";
            string msg = "";
            string cmd = "";
            picLoader.Show();
            btnTie.Hide();
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

                FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using (StreamReader sr = new StreamReader(fs))
                {
                    while ((line = sr.ReadLine()) != null)
                    {
                        Application.DoEvents();
                        if (line.IndexOf("\t") > 0)
                            delimiter = "\t";
                        try
                        {
                            msg = "1";
                            if (line.IndexOf("Basic Filter Criteria:") >= 0)
                                break;
                            if (qualifyLineImport(dt, line))
                                continue;
                            G1.parse_answer_data(line, delimiter);
                            if (line.IndexOf("serial number:") > 0)
                            {
                                msg = "2";
                                int count = G1.of_ans_count;
                                if (count < 5)
                                    continue;
                                DataRow dRow = dt.NewRow();
                                for (int i = 0; i < 5; i++)
                                {
                                    msg = "3 " + i.ToString();
                                    str = G1.of_answer[i].ObjToString().Trim();
                                    if (str.IndexOf("serial number:") > 0)
                                    {
                                        int index = str.IndexOf("serial number:");
                                        msg = "4 Index=" + index.ToString();
                                        str = str.Substring(index + 14).Trim();
                                        cmd = "Select * from `inventory` where `SerialNumber` = '" + str + "';";
                                        DataTable dx = G1.get_db_data(cmd);
                                        if (dx.Rows.Count > 0)
                                            dRow["casketRecord"] = dx.Rows[0]["record"].ObjToString();
                                        else
                                            dRow["casketRecord"] = "NEW";
                                        if ( i+2 <= G1.of_ans_count)
                                        {
                                            str = G1.of_answer[i + 2].ObjToString();
                                            dRow["Decedent Date of Death"] = str;
                                        }
                                    }
                                    else if ( G1.validate_date ( str))
                                    {
                                        str = str.Replace("am", "");
                                        str = str.Replace("pm", "");
                                        DateTime date = str.ObjToDateTime();
                                        str = date.Year.ToString("D4") + "-" + date.Month.ToString("D2") + "-" + date.Day.ToString("D2");
                                    }
                                    dRow[i+1] = str;
                                }
                                msg = "5";
                                dt.Rows.Add(dRow);
                                row++;
                            }
                        }
                        catch ( Exception ex)
                        {
                            MessageBox.Show("***ERROR*** MSG=" + msg + " " + ex.Message.ToString());
                        }
                    }
                    sr.Close();
                    btnImportFile.Show();
                }
            }
            catch (Exception ex)
            {
                DevExpress.XtraEditors.XtraMessageBox.Show(ex.Message, "Error Occurred");
                btnImportFile.Hide();
            }
            G1.NumberDataTable(dt);
            picLoader.Hide();
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
                            continue;
                        }
                        string[] Lines = line.Split(',');
                        G1.parse_answer_data(line, ",");
                        int count = G1.of_ans_count;
                        DataRow dRow = dt.NewRow();
                        for (int i = 0; i < G1.of_ans_count; i++)
                            dRow[i + 1] = G1.of_answer[i].ObjToString().Trim();
                        dt.Rows.Add(dRow);
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
            picLoader.Hide();
            return dt;
        }
        /***********************************************************************************************/
        private DataTable BuildImportDt ( string line )
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Num");
            G1.parse_answer_data(line, ",");
            for ( int i=0; i<G1.of_ans_count; i++)
            {
                string name = G1.of_answer[i].ObjToString();
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
        private void ImportUsageData(DataTable dt)
        {
            if (!ImportInventoryList.VerifyImportColumns(dt, new string[] { "casketRecord", "serviceId", "Decedent Date of Death"}))
                return;

            picLoader.Show();
            DataTable dx = null;
            bool error = false;
            string cmd = "";
            string record = "";
            string contract = "";
            string firstName = "";
            string lastName = "";
            string deceasedDate = "";
            string serviceDate = "";
            string serviceID = "";
            string old_deceasedDate = "";
            string old_serviceDate = "";
            string old_serviceID = "";
            string inventoryRecord = "";
            string caseId = "";
            string status = "";
            labelMaximum.Show();
            labelMaximum.Text = "0";
            barImport.Show();
            if ( G1.get_column_number ( dt, "serviceId") < 0 )
            {
                MessageBox.Show("***ERROR*** Locating Column 'serviceId' !!!");
                return;
            }
            if (G1.get_column_number(dt, "Decedent Date of Death") < 0)
            {
                MessageBox.Show("***ERROR*** Locating Column 'Decedent Date of Death' !!!");
                return;
            }
            if (G1.get_column_number(dt, "Service Date - First") < 0)
            {
                MessageBox.Show("***ERROR*** Locating Column 'Service Date - Field' !!!");
                return;
            }

            int lastrow = dt.Rows.Count;

            int tableRow = 0;
            int rowCount = 0;
//            lastrow = 1;
            try
            {
                barImport.Minimum = 0;
                barImport.Maximum = lastrow;
                lblTotal.Text = "of " + lastrow.ToString();
                picLoader.Show();

                for (int i = 0; i < lastrow; i++)
                {
                    Application.DoEvents();
                    barImport.Value = i;
                    barImport.Refresh();
                    labelMaximum.Text = i.ToString();
                    labelMaximum.Refresh();

                    tableRow = i;
                    record = "";
                    try
                    {
                        inventoryRecord = dt.Rows[i]["casketRecord"].ObjToString();
                        status = dt.Rows[i]["status"].ObjToString();
                        if (status.Trim().ToUpper() == "NEW")
                            continue;
                        if (status.Trim().ToUpper() == "ERROR")
                            continue;
                        if (status.Trim().ToUpper() == "DUPLICATE")
                        {
                            if ( !chkAllowDuplicates.Checked )
                                continue;
                        }

                        if (String.IsNullOrWhiteSpace(inventoryRecord))
                            continue;

                        //serviceID = dt.Rows[i]["serviceId"].ObjToString();
                        //deceasedDate = dt.Rows[i]["Decedent Date of Death"].ObjToString();
                        //serviceDate = dt.Rows[i]["Service Date - First"].ObjToString();

                        serviceID = GetData(dt, i, "serviceId");
                        deceasedDate = GetData(dt, i, "Decedent Date of Death");
                        serviceDate = GetData(dt, i, "Service Date - First");

                        cmd = "Select * from `inventory` where `record` = '" + inventoryRecord + "';";
                        dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count <= 0)
                            continue;

                        record = dx.Rows[0]["record"].ObjToString();

                        G1.update_db_table("inventory", "record", record, new string[] { "deceasedDate", deceasedDate });
                        if ( !String.IsNullOrWhiteSpace ( serviceID ))
                            G1.update_db_table("inventory", "record", record, new string[] { "serviceID", serviceID });
                        G1.update_db_table("inventory", "record", record, new string[] { "DateUsed", serviceDate });
                        rowCount++;
                    }
                    catch ( Exception ex)
                    {
                        dt.Rows[i]["num"] = "*ERROR*";
                    }
//                    picLoader.Refresh();
                }
                picLoader.Hide();
                barImport.Value = lastrow;
                MessageBox.Show("CasketUsage Data Import of " + rowCount + " Rows Complete . . .");
            }
            catch (Exception ex)
            {
                picLoader.Hide();
                MessageBox.Show("***ERROR*** CasketUsage Record/Row! " + inventoryRecord + "/" + tableRow.ToString() + " Stopping! " + ex.Message.ObjToString());
            }
        }
        /***********************************************************************************************/
        private string GetData ( DataTable dt, int i, string field )
        {
            string data = "";
            try
            {
                data = dt.Rows[i][field].ObjToString();
            }
            catch ( Exception ex )
            {
                MessageBox.Show("***ERROR*** with data located in row " + i.ToString() + " Field Name " + field + "!!!");
            }
            return data;
        }
        /***********************************************************************************************/
        private void btnImportFile_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            //ImportUsageData(dt);

            string casketCode = "";
            string casketDesc = "";
            string price = "";
            string record = "";

            string cmd = "";
            DataTable dx = null;

            for ( int i=0; i<dt.Rows.Count; i++)
            {
                try
                {
                    casketCode = dt.Rows[i]["casketCode"].ObjToString();
                    price = dt.Rows[i]["casketCost"].ObjToString();

                    if (String.IsNullOrWhiteSpace(casketCode))
                        continue;

                    cmd = "Select * from `casket_master` WHERE `casketcode` = '" + casketCode + "';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                    {
                        record = dx.Rows[0]["record"].ObjToString();
                        G1.update_db_table("casket_master", "record", record, new string[] { "casketcost", price });
                    }
                    else
                    {
                        dt.Rows[i]["status"] = "Not Found";
                    }
                }
                catch ( Exception ex)
                {
                }
            }

            mainGrid.RefreshEditor(true);
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
        private void btnTie_Click(object sender, EventArgs e)
        {
            btnImportFile.Show();
            DataTable dt = (DataTable)dgv.DataSource;

            picLoader.Show();

//            AddNewColumn(dt, "Num", "System.String", 20);
            AddNewColumn(dt, "contractNumber", "System.String", 20);
            AddNewColumn(dt, "lastName", "System.String", 20);
            AddNewColumn(dt, "firstName", "System.String", 20);
            AddNewColumn(dt, "CustHits", "System.Double", 20);
            AddNewColumn(dt, "customerRecord", "System.String", 20);
            AddNewColumn(dt, "Caskethits", "System.Double", 20);
            AddNewColumn(dt, "casketRecord", "System.String", 20);
            AddNewColumn(dt, "ServiceID", "System.String", 20);

            string fullname = "";
            string fname = "";
            string lname = "";
            string cmd = "";
            int hits = 0;
            string caseId = "";
            string deceaseDate = "";
            string contract = "";
            string record = "";

            labelMaximum.Show();
            labelMaximum.Text = "0";
            barImport.Show();

            int tableRow = 0;

            int lastrow = dt.Rows.Count;
            //            lastrow = 5;
            try
            {
                barImport.Minimum = 0;
                barImport.Maximum = lastrow;
                lblTotal.Text = "of " + lastrow.ToString();
                picLoader.Show();
                DataTable dx = null;

                for (int i = 0; i < lastrow; i++)
                {
                    Application.DoEvents();
                    barImport.Value = i;
                    barImport.Refresh();
                    labelMaximum.Text = i.ToString();
                    labelMaximum.Refresh();

                    tableRow = i;
                    try
                    {
                        fullname = dt.Rows[i]["Decedent Full Name"].ObjToString();
                        //fullname = "Mrs. Dona Lucille Bennett";
                        //fullname = "Robert Gerald Smith";
                        fullname = G1.try_protect_data(fullname);
                        if (!String.IsNullOrWhiteSpace(fullname))
                        {
                            hits = fix_fname(fullname, ref fname, ref lname, ref record, ref contract);
                            dt.Rows[i]["firstName"] = fname;
                            dt.Rows[i]["lastName"] = lname;
                            dt.Rows[i]["CustHits"] = hits.ObjToDouble();
                            dt.Rows[i]["customerRecord"] = record;
                            dt.Rows[i]["contractNumber"] = contract;
                        }

                        caseId = dt.Rows[i]["Case Identifier"].ObjToString();
                        deceaseDate = dt.Rows[i]["Decedent Date of Death"].ObjToString();
                        string serialNumber = dt.Rows[i]["Container 1 Serial Number"].ObjToString();
                        hits = 0;
                        if (!String.IsNullOrWhiteSpace(serialNumber))
                        {
                            cmd = "Select * from `inventory` where `SerialNumber` = '" + serialNumber + "';";
                            dx = G1.get_db_data(cmd);
                            hits = dx.Rows.Count;
                        }
                        if (hits > 1)
                            dt.Rows[i]["num"] = "*ERROR* Dup SN";
                        else if ( hits == 0 )
                        {
                            if (!String.IsNullOrWhiteSpace(caseId))
                            {
                                cmd = "Select * from `inventory` where `ServiceID` = '" + caseId + "';";
                                dx = G1.get_db_data(cmd);
                                hits = dx.Rows.Count;
                            }
                            if ( hits > 1 )
                                dt.Rows[i]["num"] = "*ERROR* Dup SID";
                            else if ( hits == 1)
                            {
                                record = dx.Rows[0]["record"].ObjToString();
                                if ( checkUsage ( dx, 0 ))
                                {
                                    dt.Rows[i]["casketRecord"] = "ERROR";
                                    continue;
                                }

                                dt.Rows[i]["casketRecord"] = record;
                                dt.Rows[i]["ServiceID"] = caseId;
                            }
                        }
                        else if (hits == 1)
                        {
                            record = dx.Rows[0]["record"].ObjToString();
                            if (checkUsage(dx, 0))
                            {
                                dt.Rows[i]["casketRecord"] = "ERROR";
                                continue;
                            }

                            dt.Rows[i]["casketRecord"] = record;
                            dt.Rows[i]["ServiceID"] = caseId;
                        }
                        dt.Rows[i]["CasketHits"] = hits.ObjToDouble();
                    }
                    catch ( Exception ex )
                    {
                        dt.Rows[i]["num"] = "*ERROR*";
                    }
                }
            }
            catch (Exception ex)
            {
                picLoader.Hide();
                MessageBox.Show("***ERROR*** Creating Customer Record/Row! " + contract + "/" + tableRow.ToString() + " Stopping! " + ex.Message.ObjToString());
            }
            picLoader.Hide();
            barImport.Value = lastrow;

//            G1.SetColumnPosition(newDt, mainGrid);

            mainGrid.BestFitColumns(false);
            mainGrid.OptionsView.ColumnAutoWidth = false;

            mainGrid.Columns["Num"].Fixed = DevExpress.XtraGrid.Columns.FixedStyle.Left;
            mainGrid.Columns["contractNumber"].Fixed = DevExpress.XtraGrid.Columns.FixedStyle.Left;
//            this.Text = "Import Customer Data";
            dgv.DataSource = dt;
        }
        /***************************************************************************************/
        private bool checkUsage( DataTable dx, int row )
        {
            bool rv = false;
            string str = dx.Rows[row]["serviceId"].ObjToString();
            if (!string.IsNullOrWhiteSpace(str))
                return true;
            str = dx.Rows[row]["deceasedDate"].ObjToString();
            if (!string.IsNullOrWhiteSpace(str))
            {
                DateTime date = str.ObjToDateTime();
                if ( date.Year > 1950 )
                    return true;
            }
            str = dx.Rows[row]["DateUsed"].ObjToString();
            if (!string.IsNullOrWhiteSpace(str))
            {
                DateTime date = str.ObjToDateTime();
                if (date.Year > 1950)
                    return true;
            }
            return rv;
        }
        /***************************************************************************************/
        private int fix_fname(string str, ref string fname, ref string lname, ref string record, ref string contract)
        {
            record = "";
            contract = "";
            str = str.Replace(".", "");
            str = str.Replace(",", "");
            str = str.Replace("(", "");
            str = str.Replace(")", "");
            str = str.Replace("`", "");
            str = str.Replace("'", "");
            str = str.Replace("\"", "");

            string[] Lines = str.Trim().ToUpper().Split(' ');
            fname = "";
            lname = "";
            string word = "";
            string fn = "";
            string ln = "";
            string word2 = "";
            int hits = 0;
            for ( int i=0; i<Lines.Length; i++)
            {
                word = Lines[i].Trim();
                if ( !checkSpecialWords ( word ))
                {
                    if (String.IsNullOrWhiteSpace(fname))
                        fname = word;
                    lname = word;
                }
            }
            for (int i = 0; i < Lines.Length; i++)
            {
                word = Lines[i].Trim();
                if (word == lname)
                    break;
                if (word.Length == 1)
                    continue;
                if (!checkSpecialWords(word))
                {
                    string cmd = "Select * from `customers` where `firstName` LIKE '%" + word + "%' AND `lastName` LIKE '%" + lname + "%';";
                    DataTable dx = G1.get_db_data(cmd);
                    hits = dx.Rows.Count;
                    if (hits == 1)
                    {
                        hits = 0;
                        ln = dx.Rows[0]["lastName"].ObjToString().ToUpper();
                        fn = dx.Rows[0]["firstName"].ObjToString().ToUpper();
                        if (checkAllWords(word, fn, ref word2))
                        {
                            fn = word2;
                            if (checkAllWords(lname, ln, ref word2))
                            {
                                hits = 1;
                                record = dx.Rows[0]["record"].ObjToString();
                                contract = dx.Rows[0]["contractNumber"].ObjToString();
                                break;
                            }
                        }
                    }
                }
            }
            return hits;
        }
        /***************************************************************************************/
        private bool checkAllWords ( string str, string str2, ref string word )
        {
            bool found = false;
            word = "";
            str = str.ToUpper();
            str2 = str2.ToUpper();
            str = str.Replace(".", "");
            str = str.Replace(",", "");
            str = str.Replace("(", "");
            str = str.Replace(")", "");
            str = str.Replace("`", "");
            str = str.Replace("'", "");
            str = str.Replace("\"", "");
            string[] Lines = str.Split(' ');

            str2 = str2.Replace(".", "");
            str2 = str2.Replace(",", "");
            str2 = str2.Replace("(", "");
            str2 = str2.Replace(")", "");
            str2 = str2.Replace("`", "");
            str2 = str2.Replace("'", "");
            str2 = str2.Replace("\"", "");
            string[] Lines2 = str2.Split(' ');

            for ( int i=0; i<Lines.Length; i++)
            {
                if (Lines[i].Trim().Length == 1)
                    continue;
                if (checkSpecialWords(Lines[i]))
                    continue;
                for ( int j=0; j<Lines2.Length; j++)
                {
                    if (checkSpecialWords(Lines2[j]))
                        continue;
                    if ( Lines[i] == Lines2[j])
                    {
                        found = true;
                        word = Lines[i];
                        break;
                    }
                }
                if (found)
                    break;
            }
            return found;
        }
        /***************************************************************************************/
        private bool checkSpecialWords ( string word )
        {
            string[] cases = new string[] { "MD", "DR", "SR", "JR", "II", "III", "DDS", "MR", "MRS", "MS" };
            bool found = false;
            for ( int i=0; i<cases.Length; i++)
            {
                if ( word == cases[i])
                {
                    found = true;
                    break;
                }
            }
            return found;
        }
        /***************************************************************************************/
        public static string xfix_fname(string str, ref string fname, ref string lname )
        {
            bool md, dr, sr, jr, iii, ii, dds;
            bool comma = false;
            int i, j, k, nealint;
            string[] words = new string[100];
            str = str.Trim();
            string oldstr = str;
            string newstr = "";
            str = str.Replace("(", "");
            str = str.Replace(")", "");
            str += " ~";
            j = str.IndexOf(",");
            int last_count = 0;

            if (j >= 0)
            {
                comma = true;
                last_count = count_words(str);
                str = str.Replace(",", "");
            }
            md = false;
            dr = false;
            sr = false;
            jr = false;
            ii = false;
            iii = false;
            dds = false;
            i = 0;
            nealint = j;
            for (;;)
            {
                j = str.IndexOf(" ");
                if ((str.IndexOf(" ", 0, str.Length) < nealint) && (i == 0))
                    j = nealint;
                if (j < 0)
                    break;
                string text = str.Substring(0, j);
                if (text == "~")
                    break;
                if (text.ToUpper() == "MD")
                    md = true;
                else if (text.ToUpper() == "DR")
                    dr = true;
                else if (text.ToUpper() == "SR")
                    sr = true;
                else if (text.ToUpper() == "JR")
                    jr = true;
                else if (text.ToUpper() == "II")
                    ii = true;
                else if (text.ToUpper() == "III")
                    iii = true;
                else if (text.ToUpper() == "DDS")
                    dds = true;
                else if (text.ToUpper() == "MR.")
                    dds = true;
                else if (text.ToUpper() == "MRS.")
                    dds = true;
                else if (text.ToUpper() == "MS.")
                    dds = true;
                else
                {
                    words[i] = text;
                    i++;
                }
                str = str.Substring(j + 1, str.Length - j - 1);
            }
            int start = 0;
            if (comma == true)
                start = last_count;
            newstr = "";
            for (k = start; k < i; k++)
            {
                if (newstr.Length > 0)
                    newstr += " ";
                newstr += words[k];
            }
            if (comma == true)
            {
                for (k = 0; k < last_count; k++)
                    newstr += " " + words[k];
            }

            //if (md == true)
            //    newstr += ", MD";
            //else if (dr == true)
            //    newstr = "Dr. " + newstr;
            //else if (sr == true)
            //    newstr += ", Sr.";
            //else if (jr == true)
            //    newstr += ", Jr.";
            //else if (ii == true)
            //    newstr += ", II";
            //else if (iii == true)
            //    newstr += ", III";
            //else if (dds == true)
            //    newstr += ", DDS";

            fname = "";
            lname = "";
            string[] Lines = newstr.Split(' ');
            if ( Lines.Length > 0 )
            {
                fname = Lines[0].Trim();
                if ( Lines.Length > 1 )
                {
                    int lastIndex = Lines.Length - 1;
                    lname = Lines[lastIndex];
                }
            }

            return newstr.Trim();
        }
        /***************************************************************************************/
        public static int count_words(string str)
        {
            int count = 0;
            int idx = str.IndexOf(",");
            if (idx >= 0)
                str = str.Substring(0, idx);
            idx = str.IndexOf(" ");
            if (idx < 0)
                return 1;
            str = str.Trim() + " ";
            for (;;)
            {
                if (idx <= 0)
                    break;
                string word = str.Substring(0, idx);
                count++;
                if ((idx + 1) >= str.Length)
                    break;
                str = str.Substring((idx + 1));
                idx = str.IndexOf(" ");
            }
            return count;
        }
        /***********************************************************************************************/
        private void mainGrid_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
        {
            if (e.Column.FieldName.ToUpper() == "NUM")
            {
                if (e.RowHandle >= 0)
                {
                    string num = (e.RowHandle + 1).ToString();
                    e.DisplayText = num;
                }
            }
            else if ( e.DisplayText.Trim().ToUpper() == "BAD DATA")
            {
                e.Appearance.BackColor = System.Drawing.Color.Red;
                e.Appearance.ForeColor = System.Drawing.Color.Yellow;
            }
            else if (e.DisplayText.Trim().ToUpper() == "ERROR")
            {
                e.Appearance.BackColor = System.Drawing.Color.Red;
                e.Appearance.ForeColor = System.Drawing.Color.Yellow;
            }
            else if (e.DisplayText.Trim().ToUpper() == "DUPLICATE")
            {
                e.Appearance.BackColor = System.Drawing.Color.Green;
                e.Appearance.ForeColor = System.Drawing.Color.Yellow;
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

            font = new Font("Ariel", 6);
            Printer.DrawGridDate(2, 3, 2, 3, Color.Black, BorderSide.None, font);
            Printer.DrawGridPage(11, 3, 2, 3, Color.Black, BorderSide.None, font);

            //            Printer.DrawQuad(1, 9, 2, 3, "User : " + LoginForm.username, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);

            font = new Font("Ariel", 10, FontStyle.Bold);

            Printer.DrawQuad(5, 8, 5, 4, "Casket Cost Import", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            //            Printer.DrawQuadTicks();
            Printer.SetQuadSize(24, 12);
            font = new Font("Ariel", 7, FontStyle.Regular);
            //Printer.DrawQuad(16, 7, 5, 2, lblBalance.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Top);
            //Printer.DrawQuad(16, 10, 5, 2, lblPayment.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            font = new Font("Ariel", 8);
            Printer.DrawQuad(1, 9, 6, 3, "User : " + LoginForm.username, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);


            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /***********************************************************************************************/
        private void pictureBox4_Click(object sender, EventArgs e)
        {
            G1.SpyGlass(mainGrid);
            //if (mainGrid.OptionsFind.AlwaysVisible == true)
            //    mainGrid.OptionsFind.AlwaysVisible = false;
            //else
            //    mainGrid.OptionsFind.AlwaysVisible = true;
        }
        /***********************************************************************************************/
        private void btnImportBatesville_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;

            string casketCode = "";
            string casketDesc = "";
            string cost = "";
            string itemnumber = "";
            string record = "";

            DataTable dx = null;
            DataTable bateDt = null;
            string[] Lines = null;

            this.Cursor = Cursors.WaitCursor;

            string cmd = "";
            if (!gotTim)
            {
                cmd = "Delete from `batesville_inventory` where `casketDescription` <> '';";
                G1.get_db_data(cmd);
            }
            else
            {
                bateDt = G1.get_db_data("Select * from `batesville_inventory`;");
            }


            DataRow[] dRows = null;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    casketCode = dt.Rows[i]["casketCode"].ObjToString();
                    cost = dt.Rows[i]["casketCost"].ObjToString();
                    cost = cost.Replace("$", "");
                    cost = cost.Replace(",", "");
                    casketDesc = dt.Rows[i]["casketDescription"].ObjToString();

                    if (casketDesc.Length > 99)
                        casketDesc = casketDesc.Substring(0, 99);

                    itemnumber = dt.Rows[i]["itemNumber"].ObjToString();
                    if (itemnumber == "Col 1")
                        continue;

                    if (String.IsNullOrWhiteSpace(casketCode))
                    {
                        if (String.IsNullOrWhiteSpace(casketDesc))
                            continue;
                        Lines = casketDesc.Split(' ');
                        if (Lines.Length <= 1)
                            continue;
                        casketCode = Lines[0].Trim();
                        if (casketCode.Length > 5)
                            casketCode = "";
                    }

                    if ( gotTim )
                    {
                        if (String.IsNullOrWhiteSpace(casketDesc))
                            continue;
                        dRows = null;
                        if (!String.IsNullOrWhiteSpace(casketDesc))
                            dRows = bateDt.Select("casketDescription='" + casketDesc + "'");
                        else if (!String.IsNullOrWhiteSpace(casketCode))
                            dRows = bateDt.Select("casketCode='" + casketCode + "'");
                        if (dRows.Length > 0)
                            continue;
                    }
                    record = G1.create_record("batesville_inventory", "casketDescription", casketDesc);
                    if (G1.BadRecord("batesville_inventory", record))
                        continue;

                    G1.update_db_table("batesville_inventory", "record", record, new string[] { "casketCode", casketCode, "casketDescription", casketDesc, "itemnumber", itemnumber, "cost", cost });
                }
                catch (Exception ex)
                {
                }
            }

            mainGrid.RefreshEditor(true);

            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void timCostsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            double cost = 0D;
            string str = "";
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string file = ofd.FileName;
                    DataTable dt = null;

                    this.Cursor = Cursors.WaitCursor;
                    dt = ExcelWriter.ReadFile2(file, 0, "Sheet1");
                    if (dt.Rows.Count > 0)
                    {
                        try
                        {
                            dt.Columns["Column26"].ColumnName = "casketCode";
                            dt.Columns["Column27"].ColumnName = "casketDescription";
                            dt.Columns.Add("casketCost", Type.GetType("System.Double"));
                            dt.Columns.Add("Status");
                            dt.Columns.Add("itemNumber");

                            dt.Rows.RemoveAt(0);

                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                str = dt.Rows[i]["Column28"].ObjToString();
                                cost = str.ObjToDouble();
                                dt.Rows[i]["casketCost"] = cost;

                                str = dt.Rows[i]["casketDescription"].ObjToString();
                                if (str.IndexOf("\'6\"") > 0)
                                {
                                    str = str.Replace("\'6\"", ".5F");
                                    dt.Rows[i]["casketDescription"] = str;
                                }
                                if (str.IndexOf("'") > 0)
                                {
                                    str = str.Replace("'", " ");
                                    dt.Rows[i]["casketDescription"] = str;
                                }
                                if (str.IndexOf("\"") > 0)
                                {
                                    str = str.Replace("\"", "I");
                                    dt.Rows[i]["casketDescription"] = str;
                                }
                                if (str.IndexOf("\\") > 0)
                                {
                                    str = str.Replace("\\", "");
                                    dt.Rows[i]["casketDescription"] = str;
                                }
                            }

                            for (int i = (dt.Rows.Count - 1); i >= 0; i--)
                            {
                                str = dt.Rows[i]["casketCode"].ObjToString();
                                if (String.IsNullOrWhiteSpace(str))
                                {
                                    str = dt.Rows[i]["casketDescription"].ObjToString();
                                    if (String.IsNullOrWhiteSpace(str))
                                        dt.Rows.RemoveAt(i);
                                }
                            }

                            dgv.DataSource = dt;
                        }
                        catch (Exception ex)
                        {
                        }
                    }

                    btnImportBatesville.Show();
                    btnImportBatesville.Refresh();
                    this.Cursor = Cursors.Default;

                    gotTim = true;
                }
            }
        }
        /***********************************************************************************************/
    }
}
