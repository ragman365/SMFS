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
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class PriceSingle : DevExpress.XtraEditors.XtraForm
    {
        private string _rtf1 = "";
        private string _rtf2 = "";
        public string rtf1 { get { return _rtf1; } }
        public string rtf2 { get { return _rtf2; } }
        /***********************************************************************************************/
        public PriceSingle( RichTextBox r1, RichTextBox r2 )
        {
            InitializeComponent();

            if (r1 != null)
            {
                if ( r1.Rtf != null)
                    this.rtb1.RichTextBox.Rtf = r1.Rtf;
            }
            if (r2 != null)
            {
                if (r2.Rtf != null)
                    this.rtb2.RichTextBox.Rtf = r2.Rtf;
            }
            this.rtb1.RichTextBox.Modified = false;
            this.rtb2.RichTextBox.Modified = false;
        }
        /***********************************************************************************************/
        private void PriceSingle_Load(object sender, EventArgs e)
        {
        }
        /***********************************************************************************************/
        private void PriceLists_FormClosing(object sender, FormClosingEventArgs e)
        {
            bool modified = false;
            if (rtb1.RichTextBox.Modified)
                modified = true;
            if (rtb2.RichTextBox.Modified)
                modified = true;
            if (!modified)
                return;
            DialogResult result = MessageBox.Show("***Question***\nText has been modified!\nWould you like to save your changes?", "Text Modified Dialog", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (result == DialogResult.Cancel)
            {
                e.Cancel = true;
                return;
            }
            modified = false;
            if (result == DialogResult.No)
                return;
            this.DialogResult = DialogResult.Yes;
            _rtf1 = this.rtb1.RichTextBox.Rtf;
            _rtf2 = this.rtb2.RichTextBox.Rtf;
        }
        /***********************************************************************************************/
    }
}