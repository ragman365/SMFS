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
using DevExpress.XtraEditors;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class FirstCall : DevExpress.XtraEditors.XtraForm
    {
        /****************************************************************************************/
        private string workText = "";
        private bool workFamily = false;
        public string myTextAnswer { get { return workText; } }
        public bool myCheckFamily { get { return workFamily; } }
        /****************************************************************************************/
        public FirstCall( string text, bool familyPresent )
        {
            InitializeComponent();
            workText = text;
            workFamily = familyPresent;
        }
        /****************************************************************************************/
        private void FirstCall_Load(object sender, EventArgs e)
        {
            rtb.Text = workText;
            if (workFamily)
                chkFamily.Checked = true;
        }
        /****************************************************************************************/
        private void button1_Click(object sender, EventArgs e)
        {
            workText = rtb.Text;
            workFamily = chkFamily.Checked;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        /****************************************************************************************/
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
        /****************************************************************************************/
    }
}