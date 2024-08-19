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
    public partial class VerifyPayer : DevExpress.XtraEditors.XtraForm
    {
        private DataTable workDt = null;
        private string workContract = "";
        private string workPayer = "";
        private string workFname = "";
        private string workLname = "";
        public VerifyPayer( DataTable dx, string contract, string payer, string fname, string lname )
        {
            InitializeComponent();
            workDt = dx;
            workContract = contract;
            workPayer = payer;
            workFname = fname;
            workLname = lname;
        }
        /***********************************************************************************************/
        private void VerifyPayer_Load(object sender, EventArgs e)
        {
            try
            {
                DataRow[] dRow = workDt.Select("`payer last name`='" + workLname + "' AND `payer first name`='" + workFname + "'");

                DataTable dx = workDt.Clone();
                G1.ConvertToTable(dRow, dx);

                dgv.DataSource = dx;
            }
            catch ( Exception ex)
            {
            }
            return;
        }
        /***********************************************************************************************/
    }
}