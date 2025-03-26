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
using System.IO;
using System.Drawing.Printing;
using GeneralLib;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class textEdit : DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        private bool modified = false;
        private DataTable fields = null;
        private FieldDisplayMode curDisplayMode = FieldDisplayMode.ShowFieldText;
        private string sLoadedFile;
//        private TXTextControl.StreamType stLoadedStreamType;
        private bool bDirtyFlag = false;

        private enum FieldDisplayMode
        {
            ShowFieldCodes,
            ShowFieldText,
            PreviewField
        }
        /***********************************************************************************************/
        private int dpiX = 15;

        // unicode strings for the checkbox characters
        private string UNCHECKED = UnicodeHexToString("\\u2610");
        private string CHECKED = UnicodeHexToString("\\u2612");

        private static string UnicodeHexToString(string text)
        {
            // returns the string representation
            return System.Text.Encoding.Unicode.GetString(BitConverter.GetBytes(short.Parse(text.Substring(2), System.Globalization.NumberStyles.HexNumber)));
        }
        /***********************************************************************************************/
        private string workContract = "";
        private string workField = "";
        private string workText = "";
        /***********************************************************************************************/
        public textEdit()
        {
            InitializeComponent();
        }
        /***********************************************************************************************/
        public textEdit( string contract, string field, string text )
        {
            workContract = contract;
            workField = field;
            workText = text;
            InitializeComponent();
        }
        /***********************************************************************************************/
        private void textEdit_Load(object sender, EventArgs e)
        {
            if (workText.ToUpper().IndexOf("RTF") >= 0)
                rtb1.RichTextBox.Rtf = workText;
            else
                rtb1.RichTextBox.Text = workText;

            rtb1.RichTextBox.KeyPress += RichTextBox_KeyPress;
        }
        ///***********************************************************************************************/
        private void RichTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            modified = true;
        }
        ///***********************************************************************************************/
        //private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    using (OpenFileDialog ofdImage = new OpenFileDialog())
        //    {
        //        ofdImage.Multiselect = false;

        //        if (ofdImage.ShowDialog() == DialogResult.OK)
        //        {
        //            string filename = ofdImage.FileName;
        //            filename = filename.Replace('\\', '/');
        //            if (!String.IsNullOrWhiteSpace(filename))
        //            {
        //                MemoryStream ms = new MemoryStream();
        //                FileStream fs = new FileStream(filename, FileMode.Open);
        //                fs.CopyTo(ms);

        //                textControl1.Load(MemoryStream);
        //            }
        //        }
        //        this.Refresh();
        //    }
        //}
        /***********************************************************************************************/
        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //fields = new DataTable();
            //fields.Columns.Add("field");
            //fields.Columns.Add("data");
            //try
            //{
            //    TXTextControl.LoadSettings ls = new TXTextControl.LoadSettings();
            //    ls.ApplicationFieldFormat = TXTextControl.ApplicationFieldFormat.MSWord;
            //    using (OpenFileDialog ofdImage = new OpenFileDialog())
            //    {
            //        ofdImage.Filter = "All files (*.*)|*.*";
            //        ofdImage.Multiselect = false;

            //        if (ofdImage.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            //        {
            //            string filename = ofdImage.FileName;
            //            filename = filename.Replace('\\', '/');
            //            if (!String.IsNullOrWhiteSpace(filename))
            //            {
            //                this.rtb1.RichTextBox.LoadFile(filename, RichTextBoxStreamType.RichText);
            //            }
            //        }
            //    }
            //    sLoadedFile = ls.LoadedFile;
            //    stLoadedStreamType = ls.LoadedStreamType;

            //    this.Text = "TX Text Control Mail Merge Designer: " + sLoadedFile;
            //    bDirtyFlag = false;
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show("***ERROR*** " + ex.Message.ToString());
            //}
            //string data = "";
            //string name = "";
            //string fieldName = "";
            ////foreach (TXTextControl.ApplicationField appField in textControl1.ApplicationFields)
            ////{
            ////    name = appField.Name.ObjToString();
            ////    if ( !String.IsNullOrWhiteSpace ( name))
            ////    {

            ////    }
            ////    data = appField.Text.ObjToString();
            ////    fieldName = data;
            ////    if (data.IndexOf("[%DECNAME%]") >= 0)
            ////        appField.Text = "Robby Graham";
            ////    DataRow dRow = fields.NewRow();
            ////    dRow["field"] = fieldName;
            ////    fields.Rows.Add(dRow);
            ////}


            ////TextFieldCollection tfc = textControl1.TextFields;
            ////for (int i = 0; i < tfc.Count; i++)
            ////{
            ////    TextField textField = tfc.GetItem(i);
            ////    if (textField != null)
            ////    {
            ////        data = textField.Text.ObjToString();
            ////        if (data.IndexOf("[%DECNAME%]") >= 0)
            ////            data.Replace("[%DECNAME%]", "Robby Graham");
            ////        DataRow dRow = fields.NewRow();
            ////        dRow["field"] = data;
            ////        fields.Rows.Add(dRow);
            ////    }
            ////}
            ////data = "";
            ////for (int i = 0; i < textControl1.Lines.Count; i++)
            ////{
            ////    TXTextControl.Line line = textControl1.Lines.GetItem(0);
            ////    data = line.Text.ObjToString();
            ////    if (data.IndexOf("[%DECNAME%]") >= 0)
            ////        data.Replace("[%DECNAME%]", "Robby Graham");
            ////    DataRow dRow = fields.NewRow();
            ////    dRow["field"] = data;
            ////    fields.Rows.Add(dRow);
            ////}

        }
        /***************************************************************************************/
        public delegate void d_void_eventdone_bytes(string contractNumber, string parameter, string rtfText);
        public event d_void_eventdone_bytes RtfDone;
        protected void OnDone()
        {
            if (RtfDone != null)
            {
//                RtfDone.Invoke(workContract, workField, rtb1.RichTextBox.Rtf);
                RtfDone.Invoke(workContract, workField, rtb1.RichTextBox.Text);
            }
        }
        /***********************************************************************************************/
        private void textEdit_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (RtfDone == null )
                return;
            if (!modified)
                return;
            DialogResult result = MessageBox.Show("Data Modified?\nDo you want to save?", "Modified Data Dialog", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation);
            if (result == DialogResult.No)
                return;
            if (result == DialogResult.Cancel)
            {
                e.Cancel = true;
                return;
            }
            OnDone();
        }
        /***********************************************************************************************/
        private void printPreviewToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }
        /***********************************************************************************************/
        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }
        /***********************************************************************************************/
        private void rtb1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)22)
            {

            }

                modified = true;
        }

        private void rtb1_KeyDown(object sender, KeyEventArgs e)
        {
            modified = true;

        }

        private void rtb1_Validating(object sender, CancelEventArgs e)
        {
            modified = true;

        }
        /***********************************************************************************************/
    }
}