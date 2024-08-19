using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using DevExpress.XtraPrinting;
using DevExpress.Utils;
using GeneralLib;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using MySql.Data.MySqlClient;
using DevExpress.XtraGrid;
using DevExpress.Utils.Drawing;
using OfficeOpenXml;
using Excel = Microsoft.Office.Interop.Excel;
using System.Reflection;
using DevExpress.XtraGrid.Views.Base;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class OutIn : DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        public OutIn()
        {
            InitializeComponent();
        }
        /***********************************************************************************************/
        private void btnRun_Click(object sender, EventArgs e)
        {
            string fromTable = txtFromTable.Text;
            fromTable = "payments";
            string toFile = txtToFile.Text;
            //If file does not exist then create it and right data into it..
            string filename = "C:/rag/myTable.csv";
            if (!File.Exists(filename))
            {
                FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write);
                fs.Close();
                fs.Dispose();
            }

            //Generate csv file from where data read

            DataTable dt = G1.get_db_data("Select * from `contracts`;");
            MySQL.CreateCSVfile(dt, filename);
        }
        /***********************************************************************************************/
    }
}