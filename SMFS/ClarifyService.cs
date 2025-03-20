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
    public partial class ClarifyService : DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        private string workAnswer = "";
        private string workField = "";
        /***********************************************************************************************/
        private string _CasketCode = "";
        private string _CasketDesc = "";
        private string _CasketCost = "";
        private string _Type = "";
        private string _CasketType = "";
        private string _CasketGauge = "";

        private string workService = "";
        /***********************************************************************************************/
        public ClarifyService( string service )
        {
            InitializeComponent();

            workService = service;
        }
        /***********************************************************************************************/
        private void ClarifyService_Load(object sender, EventArgs e)
        {
            txtCasketDesc.Text = workService;
            this.Text = "Clarrify Details for " + workService;
            string str = "";

            string cmd = "Select * from `secondary_inventory` WHERE `casketDesc` = '" + workService + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0 )
            {
                str = dt.Rows[0]["casketCode"].ObjToString();
                txtCasketCode.Text = str;
                str = dt.Rows[0]["casketDesc"].ObjToString();
                txtCasketDesc.Text = str;
                str = dt.Rows[0]["cost"].ObjToString();
                if ( G1.validate_numeric ( str ))
                {
                    double cost = str.ObjToDouble();
                    str = cost.ToString();
                }
                txtCasketCost.Text = str;

                str = dt.Rows[0]["type"].ObjToString();
                cmbType.Text = str;

                str = dt.Rows[0]["caskettype"].ObjToString();
                cmbCasketType.Text = str;

                str = dt.Rows[0]["casketgauge"].ObjToString();
                txtCasketGauge.Text = str;
            }

            cmbCasketType.Items.Clear();

            cmd = "Select * from `inventorylist` GROUP BY `caskettype`;";
            dt = G1.get_db_data(cmd);

            for ( int i=0; i<dt.Rows.Count; i++)
            {
                str = dt.Rows[i]["caskettype"].ObjToString();
                if ( !String.IsNullOrWhiteSpace ( str ))
                    cmbCasketType.Items.Add(dt.Rows[i]["caskettype"].ObjToString());
            }
        }
        /***********************************************************************************************/
        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (ClarifyDone == null)
            {
                this.Close();
                return;
            }
            ClarifyDone.Invoke("Cancel", "", "", "", "", "", "" );
            this.Hide();
            //this.Close();
            return;
        }
        /***********************************************************************************************/
        private void btnAccept_Click(object sender, EventArgs e)
        {
            _CasketCode = txtCasketCode.Text;
            _CasketDesc = txtCasketDesc.Text;
            _CasketCost = txtCasketCost.Text;
            _Type       = cmbType.Text;
            _CasketType = cmbCasketType.Text;
            _CasketGauge = txtCasketGauge.Text;
            this.DialogResult = DialogResult.OK;
            OnDone();
            return;
        }
        /***********************************************************************************************/
        private void txtCasketCode_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                txtCasketDesc.Focus();
        }
        /***********************************************************************************************/
        private void txtCasketDesc_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                txtCasketCost.Focus();
        }
        /***********************************************************************************************/
        private void txtCasketCost_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                cmbCasketType.Focus();
        }
        /***************************************************************************************/
        public void fireDemoDone ()
        {
            OnDone();
        }
        /***************************************************************************************/
        public delegate void d_void_eventdone_string(string workService, string casketCode, string casketDesc, string casketCost, string Type, string casketType, string casketGauge );
        public event d_void_eventdone_string ClarifyDone;
        protected void OnDone()
        {
            if (ClarifyDone != null)
            {
                ClarifyDone.Invoke( workService, _CasketCode, _CasketDesc, _CasketCost, _Type, _CasketType, _CasketGauge );
                this.Hide();
                //this.Close();
            }
        }
        /***********************************************************************************************/
        private void Clarify_FormClosed(object sender, FormClosedEventArgs e)
        {
            OnDone();
        }
        /***********************************************************************************************/
    }
}