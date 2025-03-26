using System;
using System.Windows.Forms;
using DevExpress.XtraBars;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Base.ViewInfo;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
/********************************************************************************************/
namespace SMFS
{
/********************************************************************************************/
    public partial class ButtonEdit : DevExpress.XtraEditors.XtraForm
    {
        public static bool be_pressed = false;
        public static string be_type  = "";
        public static string be_name  = "";
        public static string be_width = "";
        public static string be_answer = "";
        private bool be_delete = false;
/********************************************************************************************/
        public ButtonEdit()
        {
            be_name    = "";
            be_width   = "";
            be_type    = "";
            be_answer  = "";
            be_pressed = false;
            InitializeComponent();
        }
/********************************************************************************************/
        public ButtonEdit( string name, string width, string type, string answer )
        {
            be_name    = name;
            be_width   = width;
            be_type    = type;
            be_answer  = answer;
            be_pressed = false;
            InitializeComponent();
        }
/********************************************************************************************/
        public ButtonEdit(string name, string width, string type, string answer, bool delete )
        {
            be_name    = name;
            be_width   = width;
            be_type    = type;
            be_answer  = answer;
            be_pressed = false;
            be_delete  = delete;
            InitializeComponent();
        }
/********************************************************************************************/
        private void ButtonEdit_Load(object sender, EventArgs e)
        {
            this.textBox1.Text = be_name;
            this.textBox2.Text = be_width;
            this.textBox3.Text = be_answer;
            if (be_type == "TBOX")
                this.radioButton2.Checked = true;
            else if (be_type == "CBOX")
                this.radioButton3.Checked = true;
            else if ( be_type == "RADIO" )
                this.radioButton4.Checked = true;
            else if ( be_type == "CEDIT" )
                this.radioButton5.Checked = true;
            else if (be_type == "LABEL")
                this.radioLabel.Checked = true;
            else if (be_type == "FREE")
                this.radioFree.Checked = true;
            else
                this.radioButton1.Checked = true;
            if (be_name.Trim().Length > 0)
            { // Editing! Don't allow this to be changed
                this.radioButton1.Enabled = false;
                this.radioButton2.Enabled = false;
                this.radioButton3.Enabled = false;
                this.radioButton4.Enabled = false;
                this.radioButton5.Enabled = false;
                this.radioLabel.Enabled = false;
                this.radioFree.Enabled = false;
            }
            if (be_delete)
                this.button1.Text = "Delete";
        }
/********************************************************************************************/
        private void button1_Click(object sender, EventArgs e)
        { // Accept Data
            if (this.radioButton1.Checked)
                be_type = "CHECKBOX";
            else if (this.radioButton2.Checked)
                be_type = "TBOX";
            else if (this.radioButton3.Checked)
                be_type = "CBOX";
            else if (this.radioButton4.Checked)
                be_type = "RADIO";
            else if (this.radioButton5.Checked)
                be_type = "CEDIT";
            else if (this.radioLabel.Checked)
                be_type = "LABEL";
            else if (this.radioFree.Checked)
                be_type = "FREE";
            be_name = this.textBox1.Text;
            be_width    = this.textBox2.Text;
            be_answer   = this.textBox3.Text;
            be_pressed  = true;
            this.Close();
        }
/********************************************************************************************/
        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        { // CheckBox
            this.label2.Enabled   = true;
            this.textBox2.Enabled = true;
            this.lblAnswer.Enabled   = true;
            this.textBox3.Enabled = true;
        }
/********************************************************************************************/
        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        { // TextBox
            this.label2.Enabled   = true;
            this.textBox2.Enabled = true;
            this.lblAnswer.Enabled   = true;
            this.textBox3.Enabled = true;
        }
/********************************************************************************************/
        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        { // ComboBox
            this.label2.Enabled   = true;
            this.textBox2.Enabled = true;
            this.lblAnswer.Enabled   = true;
            this.textBox3.Enabled = true;
        }
/********************************************************************************************/
        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        { // Radio Button
            RadioButton radio = (RadioButton)(sender);
            if (radio.Checked)
            {
                this.label2.Enabled   = false;
                this.textBox2.Enabled = false;
                this.lblAnswer.Enabled   = true;
                this.textBox3.Enabled = true;
                this.lblAnswer.Text = "Group  :";
            }
            else
                this.lblAnswer.Text = "Answer :";
        }
/********************************************************************************************/
        private void radioButton5_CheckedChanged(object sender, EventArgs e)
        { // ComboEdit Button
            this.label2.Enabled   = true;
            this.textBox2.Enabled = true;
            this.lblAnswer.Enabled   = true;
            this.textBox3.Enabled = true;
        }
        /********************************************************************************************/
        private void radioLabel_CheckedChanged(object sender, EventArgs e)
        {
            this.label2.Enabled = true;
            this.textBox2.Enabled = true;
            this.lblAnswer.Enabled = true;
            this.textBox3.Enabled = true;
        }
        /********************************************************************************************/
        private void radioFree_CheckedChanged(object sender, EventArgs e)
        {
            this.label2.Enabled = true;
            this.textBox2.Enabled = true;
            this.lblAnswer.Enabled = true;
            this.textBox3.Enabled = true;
        }
        /********************************************************************************************/
    }
}
