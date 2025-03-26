using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GeneralLib;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class BankEditDebit : DevExpress.XtraEditors.XtraForm
    {
        private DataTable workDt = null;
        private bool workEdit = false;
        /***********************************************************************************************/
        public BankEditDebit(DataTable dt, bool allowEdit = false )
        {
            InitializeComponent();
            workDt = dt;
            workEdit = allowEdit;
        }
        /***********************************************************************************************/
        private void BankEditDebit_Load(object sender, EventArgs e)
        {
            //workDt.Columns.Add("AssignTo");
            //workDt.Columns.Add("depNum");
            dgv.DataSource = workDt;
            btnFinished.Hide();
        }
        /***************************************************************************************/
        public delegate void d_void_eventdone_datarow(DataTable dd );
        public event d_void_eventdone_datarow ManualDone;
        protected void OnManualDone(DataTable dd )
        {
            if (ManualDone != null)
                ManualDone.Invoke ( dd );
        }
        /***********************************************************************************************/
        private void btnFinished_Click(object sender, EventArgs e)
        {
            btnFinished.Visible = false;
            btnFinished.Refresh();

            this.Hide();

            DataTable dt = (DataTable)dgv.DataSource;
            OnManualDone(dt);
            this.Close();
        }
        /***********************************************************************************************/
        private void repositoryItemComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnFinished.Show();
            btnFinished.Refresh();
        }
        /***********************************************************************************************/
        private void BankEditDebit_FormClosing(object sender, FormClosingEventArgs e)
        {
            if ( btnFinished.Visible )
            {
                DialogResult result = MessageBox.Show("***Question***\nDebit has been modified!\nWould you like to SAVE your Changes?", "Debit Modified Dialog", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
                else if (result == DialogResult.No)
                {
                    btnFinished.Hide();
                    btnFinished.Refresh();
                    return;
                }
                btnFinished_Click(null, null);
            }
        }
        /***********************************************************************************************/
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (workEdit)
            {
                btnFinished.Show();
                btnFinished.Refresh();
                btnFinished.BackColor = Color.LightGreen;
            }
        }
        /***********************************************************************************************/
    }
}