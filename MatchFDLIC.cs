using System;
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
    public partial class MatchFDLIC : DevExpress.XtraEditors.XtraForm
    {
        public MatchFDLIC()
        {
            InitializeComponent();
        }
        /***********************************************************************************************/
        private void MatchFDLIC_Load(object sender, EventArgs e)
        {
            LoadServices();
        }
        /***********************************************************************************************/
        private void LoadServices ()
        {
            DataTable dx = new DataTable();
            dx.Columns.Add("services");
            dx.Columns.Add("fdlic");
            dx.Columns.Add("excel");
            string service = "";
            DataRow dR = null;
            string cmd = "Select * from `services`;";
            DataTable dt = G1.get_db_data(cmd);
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                service = dt.Rows[i]["service"].ObjToString();
                dR = dx.NewRow();
                dR["services"] = service;
                dx.Rows.Add(dR);
            }
            LoadFDLIC(dx);
            LoadExcel(dx);
            G1.NumberDataTable(dx);
            dgv.DataSource = dx;
        }
        /***********************************************************************************************/
        private void LoadFDLIC ( DataTable dx )
        {
            string filename = "C:/Users/Robby/Downloads/FDLIC_20180919_12-59-10.csv";
            DataTable dt = Import.ImportCSVfile(filename);
            int col = G1.get_column_number(dt, "SERVICES_BASIC_SERVICES");
            if (col < 0)
                return;
            string service = "";
            int row = 0;
            DataRow dR = null;
            for ( int i=col; i<dt.Columns.Count; i++)
            {
                service = dt.Columns[i].ColumnName.ObjToString();
                if (row >= dx.Rows.Count )
                {
                    dR = dx.NewRow();
                    dx.Rows.Add(dR);
                }
                dx.Rows[row]["fdlic"] = service;
                row++;
            }
        }
        /***********************************************************************************************/
        private void LoadExcel(DataTable dx)
        {
            string filename = "C:/Users/Robby/Documents/SMFS/FDLIC/New FDLIC Import Data/Monicello Allocation of service Charges.csv";
            DataTable dt = Import.ImportCSVfile(filename);
            int col = G1.get_column_number(dt, "SERTOT");
            if (col < 0)
                return;
            string service = "";
            int row = 0;
            DataRow dR = null;
            for (int i = col; i < dt.Columns.Count; i++)
            {
                service = dt.Columns[i].ColumnName.ObjToString();
                if (row >= dx.Rows.Count)
                {
                    dR = dx.NewRow();
                    dx.Rows.Add(dR);
                }
                dx.Rows[row]["excel"] = service;
                row++;
            }
        }
        /***********************************************************************************************/
    }
}