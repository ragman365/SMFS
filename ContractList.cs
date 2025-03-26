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
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class ContractList : DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        private DataTable workDt = null;
        public ContractList( DataTable dt )
        {
            InitializeComponent();
            workDt = dt;
        }
        /***********************************************************************************************/
        private void ContractList_Load(object sender, EventArgs e)
        {
            G1.NumberDataTable(workDt);
            dgv.DataSource = workDt;
        }
        /***********************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string contract = dr["contractNumber"].ObjToString();
            string record = dr["record"].ObjToString();
            if (!String.IsNullOrWhiteSpace(contract))
            {
                string cmd = "Select * from `customers` where `contractNumber` = '" + contract + "';";
                DataTable dt = G1.get_db_data(cmd);
                if (dt.Rows.Count <= 0)
                    return;

                this.Cursor = Cursors.WaitCursor;
                string filename = dt.Rows[0]["agreementFile"].ObjToString();
                string firstName = dt.Rows[0]["firstName"].ObjToString();
                string lastName = dt.Rows[0]["lastName"].ObjToString();
                string title = "Agreement for (" + contract + ") " + firstName + " " + lastName;
                if (record != "-1")
                    Customers.ShowPDfImage(record, title, filename);
                this.Cursor = Cursors.Default;
            }
        }
        /***********************************************************************************************/
    }
}