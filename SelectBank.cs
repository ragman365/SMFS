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
using DevExpress.XtraPrinting;
using DevExpress.Utils;
using DevExpress.XtraGrid.Views.Grid;
using iTextSharp.text.pdf;
using System.IO;
//using iTextSharp.text;

/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class SelectBank : DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        private string _answer = "";
        public string Answer { get { return _answer; } }
        /***********************************************************************************************/
        public SelectBank()
        {
            InitializeComponent();
        }
        /***********************************************************************************************/
        private void SelectBank_Load(object sender, EventArgs e)
        {
            _answer = "";
            LoadData();
        }
        /***********************************************************************************************/
        private void LoadData()
        {
            this.Cursor = Cursors.WaitCursor;
            string cmd = "Select * from `bank_accounts` where `show_dropdown` = '1';";
            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("num");
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (gridMain.OptionsFind.AlwaysVisible == true)
                gridMain.OptionsFind.AlwaysVisible = false;
            else
                gridMain.OptionsFind.AlwaysVisible = true;
        }
        /***********************************************************************************************/
        private void EditBankAccounts_FormClosing(object sender, FormClosingEventArgs e)
        {
        }
        /***********************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            _answer = dr["record"].ObjToString();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        /***********************************************************************************************/
    }
}