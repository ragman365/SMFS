using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraBars;
using System.Linq;
using System.Diagnostics;
using System.IO;
using DevExpress.XtraGrid;
using DevExpress.XtraPrinting;
using System.Data.OleDb;
//using EMRControlLib;
using GeneralLib;
//using Microsoft.Office.Core;
using Excel = Microsoft.Office.Interop.Excel;

using System.Xml;
/***********************************************************************************************/
namespace GeneralLib
{
/***********************************************************************************************/
    public class myExcel
    {
        public static int TotalsOffset = 5;
        public static int RowOffset = 7;
        public static int ssnCol = 1;
        public static int lastnameCol = 2;
        public static int firstnameCol = 3;
        public static int miCol = 4;
        public static int divisionCol = 5;
        public static int grossCol = 6;
        public static int k401Col = 7;
        public static int rothCol = 8;
        public static int loanCol = 9;
        public static int matchCol = 10;
        public static int psCol = 11;
        public static int safeCol = 13;
        public static int hoursCol = 17;
        public static int add1Col = 18;
        public static int add2Col = 19;
        public static int cityCol = 20;
        public static int stateCol = 21;
        public static int zipCol = 22;
        public static int bdateCol = 23;
        public static int hdateCol = 24;
        public static int elidateCol = 25;
        public static int termdateCol = 26;
        public static int priorhireCol = 27;
        public static int priortermCol = 28;
        public static int annualCol = 29;
        public static int statusCol = 30;
        public static int hceCol = 31;
        public static int keyCol = 32;
        public static int hoursserviceCol = 33;
        public static int unionCol = 34;
/***********************************************************************************************/
//        public static void WriteExcelData( DateTime date, DataTable dt, bool yearend = false )
//        {
//            string filename = "c:/rag/p" + date.Day.ToString("D2") + date.Month.ToString("D2") + date.Year.ToString("D4") + ".xls";
//            if (yearend)
//            {
//                filename = "c:/rag/PS3112" + date.Year.ToString("D4") + ".xls";
//            }
//            if (File.Exists(filename))
//                File.Delete(filename);
//            string templateFile = "c:/rag/Blank TR Excel.xls";
//            File.Copy(templateFile, filename);
//            //string tempfile = "c:/rag/TempBlank.xls";
//            //if (File.Exists(tempfile))
//            //    File.Delete(tempfile);
//            //File.Copy(templateFile, tempfile);

//            object misValue = System.Reflection.Missing.Value;
//            var excelApp = new Excel.Application();
//            excelApp.Workbooks.Open(filename);
//            Excel.Worksheet myExcelWorkSheet = (Excel.Worksheet)excelApp.ActiveSheet;
//            excelApp.Visible = true;

//            if (yearend)
//                LoadYearendTable(date, dt, excelApp);
//            else
//                LoadDataTable(date, dt, excelApp); // This is the guy that does the work
            
////            excelApp.SaveWorkspace(filename);
//            excelApp.Workbooks.Close();
//            MessageBox.Show("Press 'Return' to continue . . .");
//        }
/***********************************************************************************************/
//        public static void WriteCensusData( DateTime date, DataTable dt )
//        {
//            string filename = "c:/rag/PS3112" + date.Year.ToString("D4") + ".xls";
//            if (File.Exists(filename))
//                File.Delete(filename);
//            string templateFile = "c:/rag/Blank TR Excel.xls";
//            File.Copy(templateFile, filename);
//            //string tempfile = "c:/rag/TempBlank.xls";
//            //if (File.Exists(tempfile))
//            //    File.Delete(tempfile);
//            //File.Copy(templateFile, tempfile);

//            object misValue = System.Reflection.Missing.Value;
//            var excelApp = new Excel.Application();
//            excelApp.Workbooks.Open(filename);
//            Excel.Worksheet myExcelWorkSheet = (Excel.Worksheet)excelApp.ActiveSheet;
//            excelApp.Visible = true;

//            LoadDataTable(date, dt, excelApp); // This is the guy that does the work
            
////            excelApp.SaveWorkspace(filename);
//            excelApp.Workbooks.Close();
//            MessageBox.Show("Press 'Return' to continue . . .");
//        }
/***********************************************************************************************/
//        public static void JunkExcel( string templateFile, string filename )
//        {
//            //            var fileName = @"C:\calc\dev\jmapro\2012 Contribution File.xls";
//            Excel.Application myExcelApp;Excel.Workbooks myExcelWorkbooks;
//            Excel.Workbook myExcelWorkBook;
//            object misValue = System.Reflection.Missing.Value;
//            myExcelApp = new Excel.Application();
//            myExcelApp.Visible = true;
//            myExcelWorkbooks = myExcelApp.Workbooks;
//            myExcelWorkBook = myExcelWorkbooks.Open(templateFile, misValue, false, misValue, misValue, false, true, misValue, misValue, true, false, misValue, misValue, misValue, misValue);
//            Excel.Worksheet myExcelWorkSheet = (Excel.Worksheet)myExcelWorkBook.ActiveSheet;

//            myExcelWorkSheet.get_Range("C22", misValue).Formula = "New Value";
//            myExcelWorkSheet.Cells[1, 1] = "Blah";
//            myExcelWorkSheet.SaveAs(filename, misValue, misValue, false, false, false, false, false, false, false);
//            myExcelWorkBook.Close();




//            DataTable dt = new DataTable();
//            var connectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + filename + ";Extended Properties=\"Excel 12.0;IMEX=1;HDR=NO;TypeGuessRows=0;ImportMixedTypes=Text\""; ;
//            using (var conn = new OleDbConnection(connectionString))
//            {
//                conn.Open();

//                var sheets = conn.GetOleDbSchemaTable(System.Data.OleDb.OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });
//                using (var cmd = conn.CreateCommand())
//                {
//                    cmd.CommandText = "SELECT * FROM [" + sheets.Rows[0]["TABLE_NAME"].ToString() + "] ";

//                    var adapter = new OleDbDataAdapter(cmd);
//                    var ds = new DataSet();
//                    adapter.Fill(ds);
//                    dt = ds.Tables[0].Copy();
//                    dt.Rows[0][0] = "This is my stuff here";
//                    using (var conn2 = new OleDbConnection(connectionString))
//                    {
//                        OleDbCommand updateCmd = new OleDbCommand("UPDATE [" + sheets.Rows[0]["TABLE_NAME"].ToString() + "] ");
//                        updateCmd.Connection = conn2;
//                        adapter.UpdateCommand = updateCmd;
//                        conn2.Open();
//                        adapter.Update(dt);
//                        conn2.Close();
//                    }
//                }
//                conn.Close();
//            }
//            return;
//        }
///***********************************************************************************************/
//        public static void LoadDataTable(DateTime date, DataTable dt, Excel.Application excelApp )
//        {
//            excelApp.Cells[2, 2] = "SMFS Colonial Chapel";
//            excelApp.Cells[3, 2] = "218871";
//            excelApp.Cells[4, 2] = date.Month.ToString("D2") + "/" + date.Day.ToString("D2") + "/" + date.Year.ToString("D4");
//            int row = RowOffset;
//            double k401 = 0D;
//            double safe = 0D;
//            double loan = 0D;
//            for (int i = 0; i < dt.Rows.Count; i++)
//            {
//                string num = dt.Rows[i]["num"].ToString();
//                if (num.Trim().Length == 0)
//                    continue;
//                string ssn = dt.Rows[i]["ssn"].ToString();
//                if (ssn.Trim().Length == 0 && i == (dt.Rows.Count - 1))
//                    continue;
//                string lastname = dt.Rows[i]["lastname"].ToString();
//                string firstname = dt.Rows[i]["firstname"].ToString();
//                string mi = dt.Rows[i]["mi"].ToString();
//                string gross = dt.Rows[i]["gross"].ToString();
//                string safepaid = dt.Rows[i]["safepaid"].ToString();
//                string reductionpaid = dt.Rows[i]["reductionpaid"].ToString();
//                string loanpaid = dt.Rows[i]["loanpaid"].ToString();
//                string birthdate = fixDate(dt.Rows[i]["bdate"].ToString());
//                string hiredate = fixDate(dt.Rows[i]["hdate"].ToString());
//                string termdate = fixDate(dt.Rows[i]["tdate"].ToString());
//                string eligibledate = fixDate(dt.Rows[i]["eligible"].ToString());
//                string hours = dt.Rows[i]["hours"].ToString();
//                string address1 = dt.Rows[i]["address1"].ToString();
//                string address2 = dt.Rows[i]["address2"].ToString();
//                string city     = dt.Rows[i]["city"].ToString();
//                string state    = dt.Rows[i]["state"].ToString();
//                string zip      = dt.Rows[i]["zip"].ToString();
//                excelApp.Cells[row,ssnCol] = ssn;
//                excelApp.Cells[row,lastnameCol] = lastname;
//                excelApp.Cells[row,firstnameCol] = firstname;
//                excelApp.Cells[row,miCol] = mi;
//                excelApp.Cells[row,grossCol] = gross;
//                excelApp.Cells[row,safeCol] = safepaid;
//                excelApp.Cells[row,k401Col] = reductionpaid;
//                excelApp.Cells[row,loanCol] = loanpaid;
//                excelApp.Cells[row, bdateCol] = birthdate;
//                excelApp.Cells[row, hdateCol] = hiredate;
//                excelApp.Cells[row, termdateCol] = termdate;
//                excelApp.Cells[row, elidateCol] = eligibledate;
//                excelApp.Cells[row, hoursCol] = hours;
//                excelApp.Cells[row,add1Col] = address1;
//                excelApp.Cells[row,add2Col] = address2;
//                excelApp.Cells[row,cityCol] = city;
//                excelApp.Cells[row,stateCol] = state;
//                excelApp.Cells[row,zipCol] = zip;
//                if (G1.validate_numeric(safepaid))
//                    safe += Convert.ToDouble(safepaid);
//                if (G1.validate_numeric(reductionpaid))
//                    k401 += Convert.ToDouble(reductionpaid);
//                if (G1.validate_numeric(loanpaid))
//                    loan += Convert.ToDouble(loanpaid);
//                row++;
//            }
//            double total = safe + loan + k401;
//            excelApp.Cells[2, 10] = total.ToString();
//        }
/***********************************************************************************************/
        public static string fixDate(string date)
        {
            if (!G1.validate_date(date))
                return date;
            long days = G1.date_to_days(date);
//            DateTime dd = G1.days_to_datetime(days);
            DateTime dd = date.ObjToDateTime();
            date = dd.Month.ToString("D2") + "/" + dd.Day.ToString("D2") + "/" + dd.Year.ToString("D4");
            return date;
        }
///***********************************************************************************************/
//        public static void LoadYearendTable(DateTime date, DataTable dt, Excel.Application excelApp )
//        {
//            excelApp.Cells[2, 2] = "SMFS Colonial Chapel";
//            excelApp.Cells[3, 2] = "218871";
//            excelApp.Cells[4, 2] = date.Month.ToString("D2") + "/" + date.Day.ToString("D2") + "/" + date.Year.ToString("D4");
//            int row = RowOffset;
//            double k401 = 0D;
//            double safe = 0D;
//            double loan = 0D;
//            double gateway = 0D;
//            double psyear = 0D;
//            string lastname = "";
//            string firstname = "";
//            string mi = "";
//            string gross = ""; ;
//            string safepaid = ""; ;
//            string reductionpaid = "";
//            string ps = "";
//            string gw = "";
//            string loanpaid = "";
//            string birthdate = "";
//            string hiredate = "";
//            string termdate = "";
//            string eligibledate = "";
//            string hours = "";
//            string address1 = "";
//            string address2 = "";
//            string city     = "";
//            string state    = "";
//            string zip      = "";
//            double dvalue = 0D;
//            for (int i = 0; i < dt.Rows.Count; i++)
//            {
//                string num = dt.Rows[i]["num"].ToString();
//                if (num.Trim().Length == 0)
//                    continue;
//                string ssn = dt.Rows[i]["ssn"].ToString();
//                if (ssn.Trim().Length == 0 && i == (dt.Rows.Count - 1))
//                    continue;
//                lastname = dt.Rows[i]["lastname"].ToString();
//                firstname = dt.Rows[i]["firstname"].ToString();
//                mi = dt.Rows[i]["mi"].ToString();
//                gross = dt.Rows[i]["gross"].ToString();
////                safepaid = dt.Rows[i]["safepaid"].ToString();
//                safepaid = dt.Rows[i]["safediff"].ToString();
////                reductionpaid = dt.Rows[i]["reductionpaid"].ToString();
//                ps = dt.Rows[i]["profitsharing"].ToString();
//                if (string.IsNullOrEmpty(ps))
//                    ps = "0.00";
//                gw = dt.Rows[i]["gateway"].ToString();
//                if (string.IsNullOrEmpty(gw))
//                    gw = "0.00";
//                dvalue = Convert.ToDouble(ps) + Convert.ToDouble(gw);

////                loanpaid = dt.Rows[i]["loanpaid"].ToString();
//                birthdate = fixDate(dt.Rows[i]["bdate"].ToString());
//                hiredate = fixDate(dt.Rows[i]["hdate"].ToString());
//                termdate = fixDate(dt.Rows[i]["tdate"].ToString());
//                eligibledate = fixDate(dt.Rows[i]["eligible"].ToString());
//                //hours = dt.Rows[i]["hours"].ToString();
//                address1 = dt.Rows[i]["address1"].ToString();
//                address2 = dt.Rows[i]["address2"].ToString();
//                city     = dt.Rows[i]["city"].ToString();
//                state    = dt.Rows[i]["state"].ToString();
//                zip      = dt.Rows[i]["zip"].ToString();
//                excelApp.Cells[row,ssnCol] = ssn;
//                excelApp.Cells[row,lastnameCol] = lastname;
//                excelApp.Cells[row,firstnameCol] = firstname;
//                excelApp.Cells[row,miCol] = mi;
//                excelApp.Cells[row,grossCol] = gross;
//                excelApp.Cells[row,safeCol] = safepaid;
//                excelApp.Cells[row,k401Col] = reductionpaid;
//                excelApp.Cells[row, psCol] = dvalue.ToString();
//                excelApp.Cells[row,loanCol] = loanpaid;
//                excelApp.Cells[row, bdateCol] = birthdate;
//                excelApp.Cells[row, hdateCol] = hiredate;
//                excelApp.Cells[row, termdateCol] = termdate;
//                excelApp.Cells[row, elidateCol] = eligibledate;
//                excelApp.Cells[row, hoursCol] = hours;
//                excelApp.Cells[row,add1Col] = address1;
//                excelApp.Cells[row,add2Col] = address2;
//                excelApp.Cells[row,cityCol] = city;
//                excelApp.Cells[row,stateCol] = state;
//                excelApp.Cells[row,zipCol] = zip;
//                if (G1.validate_numeric(safepaid))
//                    safe += Convert.ToDouble(safepaid);
//                if (G1.validate_numeric(gw))
//                    gateway += Convert.ToDouble(gw);
//                if (G1.validate_numeric(reductionpaid))
//                    k401 += Convert.ToDouble(reductionpaid);
//                if (G1.validate_numeric(loanpaid))
//                    loan += Convert.ToDouble(loanpaid);
//                if (G1.validate_numeric(ps))
//                    psyear += Convert.ToDouble(ps);
//                row++;
//            }
//            double total = safe + psyear + gateway;
//            excelApp.Cells[2, 10] = total.ToString();
//        }
///***********************************************************************************************/
//        public static DataTable ReadExcelFile(string dir, string filename)
//        {
//            DataTable dt = new DataTable();
//            //Directory.SetCurrentDirectory(dir);
//            object misValue = System.Reflection.Missing.Value;
//            var excelApp = new Excel.Application();
//            //excelApp.Workbooks.Open(dir + "/" + filename);
//            excelApp.Workbooks.Open(filename);
//            Excel.Worksheet myExcelWorkSheet = (Excel.Worksheet)excelApp.ActiveSheet;
//            excelApp.Visible = false;
//            string data = "";
//            string cname = "";
//            string[] List = new string [] { "C 1", "C 3", "C 45", "C 46", "C 47", "C 48", "C 49", "C 98", "C 146", "C 147", "C 148", "C 149", "C 150", "C 162" };
//            try
//            {
//                for (int col = 0; col < List.Length; col++)
//                {
//                    string name = List[col].ObjToString();
//                    dt.Columns.Add(name);
//                }
//                //for (int col = 1; col <= myExcelWorkSheet.Columns.Count; col++)
//                //    dt.Columns.Add("C " + col.ToString());
//                for (int row = 1; row <= myExcelWorkSheet.Rows.Count; row++)
//                {
//                    DataRow dr = dt.NewRow();
//                    for (int col = 0; col < List.Length; col++)
//                    {
//                        string name = List[col].ObjToString();
//                        int actualcol = name.Replace ( "C ", "" ).ObjToInt32();

//                        data = GetCell(myExcelWorkSheet, row, actualcol);
//                        if (!String.IsNullOrWhiteSpace ( data ) )
//                            dr[name] = data;
//                    }
//                    //for (int col = 1; col <= myExcelWorkSheet.Columns.Count; col++)
//                    //{
//                    //    data = GetCell(myExcelWorkSheet, row, col);
//                    //    if (!String.IsNullOrWhiteSpace ( data ) )
//                    //    {
//                    //        cname = "C " + col.ToString();
//                    //        dr[cname] = data;
//                    //    }
//                    //}
//                    dt.Rows.Add(dr);
//                    int maxrow = dt.Rows.Count-1;
//                    string total = dt.Rows[maxrow]["C 1"].ObjToString().Trim().ToUpper();
//                    if (total == "TOTALS")
//                        break;
//                }
//            }
//            catch
//            {
//            }
//            return dt;
//        }
///****************************************************************************************/
//        public static string GetCell(Excel.Worksheet myExcelWorkSheet, int row, int col)
//        {
//            string str = "";
//            try
//            {
//                object var = (object)(myExcelWorkSheet.Cells[row, col] as Excel.Range).Value;
//                str = var.ObjToString();
//            }
//            catch
//            {
//            }
//            return str;
//        }
/***********************************************************************************************/
    }
}
namespace ExcelExample
{
    class CreateExcelDoc
    {
        private Excel.Application app = null;
        private Excel.Workbook workbook = null;
        private Excel.Worksheet worksheet = null;
        private Excel.Range workSheet_range = null;
        public CreateExcelDoc()
        {
            createDoc();
        }
        public void writeDoc(string filename)
        {
            try
            {
                if (File.Exists(filename))
                    File.Delete(filename);

                object misValue = System.Reflection.Missing.Value;
                Excel.Workbook workbook = app.Workbooks.Add(misValue);
//                workbook.SaveAs(filename);
                workbook.SaveAs(filename, misValue, misValue, misValue,
                misValue, misValue, Excel.XlSaveAsAccessMode.xlNoChange, misValue, misValue, misValue, misValue, misValue);

                //                app.SaveWorkspace(filename);
            }
            catch ( Exception ex)
            {
                MessageBox.Show("***ERROR*** " + ex.Message.ToString());
            }
        }
        public void close()
        {
            object misValue = System.Reflection.Missing.Value;
            app.Workbooks.Close();
            app.Quit();
        }
        public void createDoc()
        {
            try
            {
                app = new Excel.Application();
                app.Visible = true;
                workbook = app.Workbooks.Add(1);
                worksheet = (Excel.Worksheet)workbook.Sheets[1];
            }
            catch (Exception e)
            {
                Console.Write("Error");
            }
            finally
            {
            }
        }

        public void createHeaders(int row, int col, string htext, string cell1,
        string cell2, int mergeColumns, string b, bool font, int size, string
        fcolor)
        {
            worksheet.Cells[row, col] = htext;
            workSheet_range = worksheet.get_Range(cell1, cell2);
            workSheet_range.Merge(mergeColumns);
            switch (b)
            {
                case "YELLOW":
                    workSheet_range.Interior.Color = System.Drawing.Color.Yellow.ToArgb();
                    break;
                case "GRAY":
                    workSheet_range.Interior.Color = System.Drawing.Color.Gray.ToArgb();
                    break;
                case "GAINSBORO":
                    workSheet_range.Interior.Color =
            System.Drawing.Color.Gainsboro.ToArgb();
                    break;
                case "Turquoise":
                    workSheet_range.Interior.Color =
            System.Drawing.Color.Turquoise.ToArgb();
                    break;
                case "PeachPuff":
                    workSheet_range.Interior.Color =
            System.Drawing.Color.PeachPuff.ToArgb();
                    break;
                default:
                    //  workSheet_range.Interior.Color = System.Drawing.Color..ToArgb();
                    break;
            }

            workSheet_range.Borders.Color = System.Drawing.Color.Black.ToArgb();
            workSheet_range.Font.Bold = font;
            workSheet_range.ColumnWidth = size;
            if (fcolor.Equals(""))
            {
                workSheet_range.Font.Color = System.Drawing.Color.White.ToArgb();
            }
            else
            {
                workSheet_range.Font.Color = System.Drawing.Color.Black.ToArgb();
            }
        }

        public void addData(int row, int col, string data,
            string cell1, string cell2, string format)
        {
            worksheet.Cells[row, col] = data;
            workSheet_range = worksheet.get_Range(cell1, cell2);
            workSheet_range.Borders.Color = System.Drawing.Color.Black.ToArgb();
            workSheet_range.NumberFormat = format;
        }
    }
}