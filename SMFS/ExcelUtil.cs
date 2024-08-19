using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using DevExpress.XtraEditors;
using GeneralLib;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using System.Data.OleDb;
using GeneralLib;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace SMFS
{
    /// <summary>
    /// Produces Excel file without using Excel
    /// </summary>
    public class ExcelWriter
    {
        private Stream stream;
        private BinaryWriter writer;
        private BinaryReader reader;

        private ushort[] clBegin = { 0x0809, 8, 0, 0x10, 0, 0 };
        private ushort[] clEnd = { 0x0A, 00 };

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowThreadProcessId(IntPtr hwnd, ref int lpdwProcessId);


        private void WriteUshortArray(ushort[] value)
        {
            for (int i = 0; i < value.Length; i++)
                writer.Write(value[i]);
        }

        //private void ReadUshortArray(ushort[] value)
        //{
        //    for (int i = 0; i < value.Length; i++)
        //        reader.ReadByte(value[i]);
        //}

        /// <summary>
        /// Initializes a new instance of the <see cref="ExcelWriter"/> class.
        /// </summary>
        /// <param name="stream">The stream.</param>
        public ExcelWriter(Stream stream)
        {
            this.stream = stream;
            writer = new BinaryWriter(stream);
        }

        /// <summary>
        /// Writes the text cell value.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="col">The col.</param>
        /// <param name="value">The string value.</param>
        public void WriteCell(int row, int col, string value)
        {
            ushort[] clData = { 0x0204, 0, 0, 0, 0, 0 };
            int iLen = value.Length;
            byte[] plainText = Encoding.ASCII.GetBytes(value);
            clData[1] = (ushort)(8 + iLen);
            clData[2] = (ushort)row;
            clData[3] = (ushort)col;
            clData[5] = (ushort)iLen;
            WriteUshortArray(clData);
            writer.Write(plainText);
        }

        /// <summary>
        /// Writes the integer cell value.
        /// </summary>
        /// <param name="row">The row number.</param>
        /// <param name="col">The column number.</param>
        /// <param name="value">The value.</param>
        public void WriteCell(int row, int col, int value)
        {
            ushort[] clData = { 0x027E, 10, 0, 0, 0 };
            clData[2] = (ushort)row;
            clData[3] = (ushort)col;
            WriteUshortArray(clData);
            int iValue = (value << 2) | 2;
            writer.Write(iValue);
        }

        /// <summary>
        /// Writes the double cell value.
        /// </summary>
        /// <param name="row">The row number.</param>
        /// <param name="col">The column number.</param>
        /// <param name="value">The value.</param>
        public void WriteCell(int row, int col, double value)
        {
            ushort[] clData = { 0x0203, 14, 0, 0, 0 };
            clData[2] = (ushort)row;
            clData[3] = (ushort)col;
            WriteUshortArray(clData);
            writer.Write(value);
        }

        /// <summary>
        /// Writes the empty cell.
        /// </summary>
        /// <param name="row">The row number.</param>
        /// <param name="col">The column number.</param>
        public void WriteCell(int row, int col)
        {
            ushort[] clData = { 0x0201, 6, 0, 0, 0x17 };
            clData[2] = (ushort)row;
            clData[3] = (ushort)col;
            WriteUshortArray(clData);
        }

        /// <summary>
        /// Must be called once for creating XLS file header
        /// </summary>
        public void BeginWrite()
        {
            WriteUshortArray(clBegin);
        }

        /// <summary>
        /// Ends the writing operation, but do not close the stream
        /// </summary>
        public void EndWrite()
        {
            WriteUshortArray(clEnd);
            writer.Flush();
        }
        public DataTable ExcelToDataTable(string pathName, string sheetName)
        {
            DataTable tbContainer = new DataTable();
            //string strConn = string.Empty;
            //if (string.IsNullOrEmpty(sheetName)) { sheetName = "Sheet1"; }
            //FileInfo file = new FileInfo(pathName);
            //if (!file.Exists) { throw new Exception("Error, file doesn't exists!"); }
            //string extension = file.Extension;
            //switch (extension)
            //{
            //    case ".xls":
            //        strConn = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + pathName + ";Extended Properties='Excel 8.0;HDR=Yes;IMEX=1;'";
            //        break;
            //    case ".xlsx":
            //        strConn = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + pathName + ";Extended Properties='Excel 12.0;HDR=Yes;IMEX=1;'";
            //        break;
            //    default:
            //        strConn = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + pathName + ";Extended Properties='Excel 8.0;HDR=Yes;IMEX=1;'";
            //        break;
            //}
            //OleDbConnection cnnxls = new OleDbConnection(strConn);
            //string name = sheets.Rows[0]["TABLE_NAME"].ToString();
            //cmd.CommandText = "SELECT * FROM [" + name + "] ";


            //OleDbDataAdapter oda = new OleDbDataAdapter(string.Format("select * from [{0}$]", sheetName), cnnxls);
            //DataSet ds = new DataSet();
            //oda.Fill(tbContainer);
            return tbContainer;
        }
        /***********************************************************************************************/
        public static DataTable ReadExcelFile(string filename)
        {
            //            var fileName = @"C:\calc\dev\jmapro\2012 Contribution File.xls";
            DataTable dt = new DataTable();
            string strConn = string.Empty;
            FileInfo file = new FileInfo(filename);
            if (!file.Exists) { throw new Exception("Error, file doesn't exists!"); }
            string extension = file.Extension;
            try
            {
                switch (extension)
                {
                    case ".xls":
                        strConn = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + filename + ";Extended Properties='Excel 8.0;HDR=Yes;IMEX=1;'";
                        break;
                    case ".xlsx":
                        strConn = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + filename + ";Extended Properties='Excel 12.0;HDR=Yes;IMEX=1;'";
                        break;
                    default:
                        strConn = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + filename + ";Extended Properties='Excel 8.0;HDR=Yes;IMEX=1;'";
                        break;
                }
                //            var connectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + filename + ";Extended Properties=\"Excel 12.0;IMEX=1;HDR=NO;TypeGuessRows=0;ImportMixedTypes=Text\""; ;
                using (var conn = new OleDbConnection(strConn))
                {
                    try
                    {
                        conn.Open();
                    }
                    catch (Exception ex)
                    {

                    }

                    DataTable sheets = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "Table" });


                    //                var sheets = conn.GetOleDbSchemaTable(System.Data.OleDb.OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });
                    using (var cmd = conn.CreateCommand())
                    {
                        //                    string name = sheets.Rows[8]["TABLE_NAME"].ToString();

                        if (sheets.Rows.Count > 0)
                        {
                            string name = sheets.Rows[0]["TABLE_NAME"].ToString();
                            cmd.CommandText = "SELECT * FROM [" + name + "] ";
                        }

                        OleDbDataAdapter oda = new OleDbDataAdapter(string.Format("select * from [demo"), conn);
                        //                    var adapter = new OleDbDataAdapter(string.Format("select * from [{0}$]"), "Sheet1", conn);
                        var ds = new DataSet();
                        oda.Fill(ds);
                        dt = ds.Tables[0].Copy();
                        for (int i = 0; i < dt.Columns.Count; i++)
                        {
                            for (int j = 0; j < dt.Rows.Count; j++)
                            {
                                string str = dt.Rows[j][i].ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return dt;
        }
        /***********************************************************************************************/
        public static DataTable ReadFile2(string filePath, sbyte sheet_index = 0, string sheet_name = "" )
        {
            // Create new instance of Excel Application
            var excelApp = new Microsoft.Office.Interop.Excel.Application();
            excelApp.Visible = false;

            // Open workook at specified filepath
            Microsoft.Office.Interop.Excel.Workbook wb = excelApp.Workbooks.Open(filePath);

            // Determine sheet based on optional parameters
            // If no sheet index or sheet name is passed get sheet 1
            Microsoft.Office.Interop.Excel.Worksheet ws = null;
            DataTable _myDataTable = null;

            bool isGood = true;

            if (!String.IsNullOrEmpty(sheet_name))
            {
                try
                {
                    ws = (Microsoft.Office.Interop.Excel.Worksheet)wb.Worksheets[sheet_name];
                }
                catch ( Exception ex)
                {
                    if (1 == 1)
                        isGood = false;
                    else
                    {
                        sheet_index = 1;
                        ws = (Microsoft.Office.Interop.Excel.Worksheet)wb.Worksheets.get_Item(sheet_index);
                    }
                }
            }
            else if (sheet_index > 0)
            {
                //ws = (Microsoft.Office.Interop.Excel.Worksheet)wb.Worksheets[sheet_index];
                ws = (Microsoft.Office.Interop.Excel.Worksheet)wb.Worksheets.get_Item(sheet_index);
            }
            else
            {
                ws = (Microsoft.Office.Interop.Excel.Worksheet)wb.Worksheets[1];
            }

            //xlApp = new Excel.Application();
            //xlWorkBook = xlApp.Workbooks.Open(@"d:\csharp-Excel.xls", 0, true, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);
            //xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);

            // Determine used range and worksheet name
            if (isGood)
            {
                Microsoft.Office.Interop.Excel.Range range = ws.UsedRange;
                string sheetName = ws.Name;



                object[,] cellValues = (object[,])range.Value2;

                _myDataTable = null;

                if (range.Columns.Count > 200)
                {
                    object[,] cValues = new object[range.Rows.Count, 200];
                    for (int i = 0; i < range.Rows.Count; i++)
                    {
                        for (int j = 0; j < 200; j++)
                        {
                            try
                            {
                                cValues[i, j] = cellValues[i, j];
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                    }
                    _myDataTable = ArraytoDatatable(cValues);
                }
                else
                    _myDataTable = ArraytoDatatable(cellValues);
            }

            //            range.Clear();

            if (excelApp != null)
            {
                int excelProcessId = 0; // simple declare, zero is merely a place holder

                GetWindowThreadProcessId(new IntPtr(excelApp.Hwnd), ref excelProcessId);

                Process ExcelProc = Process.GetProcessById(excelProcessId);

                if (ExcelProc != null)
                {
                    ExcelProc.Kill();
                }
            }

            //wb.Close(false, Type.Missing, Type.Missing);
            //excelApp.Quit();
            //GC.Collect();
            //Marshal.FinalReleaseComObject(wb);
            //Marshal.FinalReleaseComObject(excelApp);

            //System.Runtime.InteropServices.Marshal.ReleaseComObject(excelApp);
            //System.Runtime.InteropServices.Marshal.ReleaseComObject(wb);
            excelApp = null;
            return _myDataTable;
        }
        public static DataTable ArraytoDatatable(Object[,] numbers)
        {
            DataTable dt = new DataTable();
            try
            {
                for (int i = 0; i < numbers.GetLength(1); i++)
                {
                    try
                    {
                        dt.Columns.Add("Column" + (i + 1));
                    }
                    catch ( Exception ex)
                    {
                    }
                }

                for (var i = 1; i <= numbers.GetLength(0); i++)
                {
                    DataRow row = dt.NewRow();
                    try
                    {
                        for (var j = 1; j <= numbers.GetLength(1); j++)
                        {
                            if (numbers[i, j] != null)
                                row[j - 1] = numbers[i, j];
                        }
                    }
                    catch ( Exception ex)
                    {
                    }
                    dt.Rows.Add(row);
                }
            }
            catch ( Exception ex )
            {
            }
            return dt;
        }
        /***********************************************************************************************/
    }
}

