using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;

using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Base.ViewInfo;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraGrid.Views.BandedGrid;
using DevExpress.XtraGrid.Views.BandedGrid.ViewInfo;

using System.Drawing.Printing;
using System.Collections;
using System.CodeDom.Compiler;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using DevExpress.XtraPrinting;
using DevExpress.Utils;

using GeneralLib;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class ImportPaymentHistory : Form
    {
        /***********************************************************************************************/
        public ImportPaymentHistory()
        {
            InitializeComponent();
        }
        /***********************************************************************************************/
        private void Import_Load(object sender, EventArgs e)
        {
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
                    DataTable dt = ImportCSVfile(file);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        dgv.DataSource = dt;
                        btnImportFile.Show();
                    }
                }
            }
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
                using (StreamReader sr = new StreamReader(filename))
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
                        if (G1.of_ans_count >= 12)
                        {
                            DataRow dRow = dt.NewRow();
                            for (int i = 0; i < G1.of_ans_count; i++)
                                dRow[i+1] = G1.of_answer[i];
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
        private void ImportPayHistoryData ( DataTable dt )
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

            labelMaximum.Show();
            labelMaximum.Text = "0";
            barImport.Show();
            int lastrow = dt.Rows.Count;
            int tableRow = 0;
                        //lastrow = 85;

            try
            {
                cmd = "LOAD DATA LOCAL INFILE 'C:/Users/Robby/Documents/My Data Sources/test.csv' IGNORE INTO TABLE smfs.payments CHARACTER SET utf8 FIELDS TERMINATED BY ',' OPTIONALLY ENCLOSED BY '\"' IGNORE 1 LINES;";
                DataTable dd = G1.get_db_data(cmd);

                //barImport.Minimum = 0;
                //barImport.Maximum = lastrow;
                //for (int i = 0; i < lastrow; i++)
                //{
                //    Application.DoEvents();
                //    barImport.Value = i;
                //    barImport.Refresh();
                //    labelMaximum.Text = i.ToString();
                //    labelMaximum.Refresh();
                //    tableRow = i;
                //    record = "";
                //    contract = dt.Rows[i]["cnum"].ObjToString();
                //    payDate8 = GetSQLDate(dt, i, "paydt8");
                //    cmd = "Select * from `payments` where `contractNumber` = '" + contract + "' and `payDate8` = '" + payDate8 + "';";
                //    dx = G1.get_db_data(cmd);
                //    if (dx.Rows.Count > 0)
                //        record = dx.Rows[0]["record"].ObjToString();
                //    else
                //        record = G1.create_record("payments", "contractNumber", "-1");
                //    if (string.IsNullOrWhiteSpace(record))
                //    {
                //        MessageBox.Show("***ERROR*** Creating Payment Record! " + contract + " Stopping!");
                //        break;
                //    }
                //    else if (record == "-1")
                //    {
                //        MessageBox.Show("***ERROR*** Creating Payment Record! " + contract + " Stopping!");
                //        break;
                //    }
                //    lastName = dt.Rows[i]["lname"].ObjToString();
                //    firstName = dt.Rows[i]["fName"].ObjToString();

                //    paidDate = dt.Rows[i]["date"].ObjToString();
                //    downPayment = dt.Rows[i]["dpay"].ObjToString();
                //    checkNumber = dt.Rows[i]["chk1"].ObjToString();
                //    agentNumber = dt.Rows[i]["anum"].ObjToString();

                //    paymentAmount = dt.Rows[i]["payamt"].ObjToString();
                //    numMonthPaid = dt.Rows[i]["tmon"].ObjToString();
                //    paymentDate = dt.Rows[i]["paydte"].ObjToString();
                //    debitAdjustment = dt.Rows[i]["debit"].ObjToString();

                //    creditAdjustment = dt.Rows[i]["credit"].ObjToString();
                //    debitReason = dt.Rows[i]["drrea"].ObjToString();
                //    creditReason = dt.Rows[i]["crrea"].ObjToString();
                //    location = dt.Rows[i]["loc"].ObjToString();

                //    userId = dt.Rows[i]["userid"].ObjToString();
                //    depositNumber = dt.Rows[i]["dep#"].ObjToString();
                //    interestPaid = dt.Rows[i]["intpd"].ObjToString();
                //    trust85P = dt.Rows[i]["trust85"].ObjToString();
                //    trust100P = dt.Rows[i]["trust100"].ObjToString();
                //    dueDate8 = dt.Rows[i]["date8"].ObjToString();

                //    G1.update_db_table("payments", "record", record, new string[] { "contractNumber", contract, "payDate8", payDate8, "lastName", lastName, "firstName", firstName });
                //    G1.update_db_table("payments", "record", record, new string[] { "paidDate", paidDate, "downPayment", downPayment, "checkNumber", checkNumber, "agentNumber", agentNumber });
                //    G1.update_db_table("payments", "record", record, new string[] { "paymentAmount", paymentAmount, "numMonthPaid", numMonthPaid, "paymentDate", paymentDate, "debitAdjustment", debitAdjustment });
                //    G1.update_db_table("payments", "record", record, new string[] { "creditAdjustment", creditAdjustment, "debitReason", debitReason, "creditReason", creditReason, "location", location });
                //    G1.update_db_table("payments", "record", record, new string[] { "userId", userId, "depositNumber", depositNumber, "interestPaid", interestPaid, "trust85P", trust85P });
                //    G1.update_db_table("payments", "record", record, new string[] { "trust100P", trust100P, "dueDate8", dueDate8 });
                //}
                picLoader.Hide();
//                barImport.Value = lastrow;
                MessageBox.Show("Payment History Data Import of " + lastrow + " Rows Complete . . .");
            }
            catch (Exception ex)
            {
                picLoader.Hide();
                MessageBox.Show("***ERROR*** Creating Payment History Record/Row! " + contract + "/" + tableRow.ToString() + " Stopping! " + ex.Message.ObjToString());
            }
        }
        /***********************************************************************************************/
        private void btnImportFile_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            int col = G1.get_column_number(dt, "chk1");
            if (col > 0)
                ImportPayHistoryData(dt);
            else
            {
                MessageBox.Show("***ERROR*** Imported Data does not appear to be Pay History File!");
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
    }
}
