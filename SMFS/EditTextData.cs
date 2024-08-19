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
    public partial class EditTextData : DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        private string workData = "";
        private string workTitle = "";
        private bool modified = false;
        private string _answer = "";
        public string Answer { get { return _answer; } }
        /***********************************************************************************************/
        public EditTextData( string title, string data )
        {
            InitializeComponent();
            workTitle = title;
            workData = data;
            _answer = "";
        }
        /***********************************************************************************************/
        private void EditTextData_Load(object sender, EventArgs e)
        {
            this.Text = "Edit Data for " + workTitle;
            rtb.Text = workData;
            modified = false;

            button2.Focus();
        }
        /***********************************************************************************************/
        private void btnCancel_Click(object sender, EventArgs e)
        {
            modified = false;
            _answer = "";
            this.Close();
            return;
        }
        /***********************************************************************************************/
        private void btnAccept_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            _answer = rtb.Text.Trim();
            modified = false;
            this.Close();
        }
        /***********************************************************************************************/
        private void FuneralDemo_FormClosed(object sender, FormClosedEventArgs e)
        {
            if ( modified )
            {
                DialogResult result = MessageBox.Show("***Question***\nData has been modified!\nWould you like to SAVE this data?", "Data Modified Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                if (result == DialogResult.Yes)
                {
                    this.DialogResult = DialogResult.OK;
                    _answer = rtb.Text.Trim();
                    modified = false;
                }
            }
        }
        /***********************************************************************************************/
        private void rtb_TextChanged(object sender, EventArgs e)
        {
            modified = true;
        }
        /***********************************************************************************************/
    }
}