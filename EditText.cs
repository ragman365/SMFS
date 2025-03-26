using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraRichEdit.API.Native;

using System.Threading.Tasks;
using DevExpress.XtraEditors;
using System.IO;
using System.Drawing.Printing;
using GeneralLib;
using DevExpress.XtraRichEdit;
/***********************************************************************************************/

namespace SMFS
{
    /***********************************************************************************************/
    public partial class EditText : Form
    {
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
        private string DocumentFileName = "";
        //private TXTextControl.StreamType DocumentStreamType = 0;
        /***********************************************************************************************/

        private DataTable fields = null;
        private FieldDisplayMode curDisplayMode = FieldDisplayMode.ShowFieldText;
        private string sLoadedFile;
        //private TXTextControl.StreamType stLoadedStreamType;
        private bool bDirtyFlag = false;
        private string workFormName = "";
        private bool workIsText = false;

        private enum FieldDisplayMode
        {
            ShowFieldCodes,
            ShowFieldText,
            PreviewField
        }
        /***********************************************************************************************/
        public EditText(string formName)
        {
            InitializeComponent();
            workFormName = formName;
        }
        /***********************************************************************************************/
        public EditText(bool isText, string formName)
        {
            InitializeComponent();
            workFormName = formName;
            workIsText = true;
        }
        /***********************************************************************************************/
        private void EditText_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!workIsText)
            {
                for (int i = 0; i < rtb.Document.Fields.Count; i++)
                {
                    string fieldCode = rtb.Document.GetText(rtb.Document.Fields[i].CodeRange);
                }
            }
        }
        /***********************************************************************************************/
        private void EditText_Load(object sender, EventArgs e)
        {
            string pdfRecord = "";
            if (!workIsText)
            {
                string cmd = "Select * from `pdfimages` where `filename` = '" + workFormName + "';";
                DataTable dt = G1.get_db_data(cmd);
                if (dt.Rows.Count > 0)
                {
                    //pdfRecord = dt.Rows[0]["record"].ObjToString();
                    //string str = G1.get_db_blob("pdfimages", pdfRecord, "image");
                    //byte[] bytes = Encoding.ASCII.GetBytes(str);

                    //MemoryStream stream = new MemoryStream(bytes);

                    //rtb.Document.Delete(rtb.Document.Range);

                    //rtb.Document.LoadDocument(stream, DocumentFormat.Rtf);
                }
            }
            else
            {
                rtb.Document.Text = workFormName;
            }
            //            LineNumbering(rtb.Document);
            //CreateColumns(rtb.Document);
            //PrintLayout(rtb.Document);
            //TabStops(rtb.Document);
            //            doDateTime(rtb.Document);
        }
        /***********************************************************************************************/
        private void doDateTime(Document document)
        {
            document.BeginUpdate();

            //Create a DATE field at the caret position
            document.Fields.Create(document.CaretPosition, "DATE");
            document.EndUpdate();
            for (int i = 0; i < document.Fields.Count; i++)
            {
                string fieldCode = document.GetText(document.Fields[i].CodeRange);
                if (fieldCode == "DATE")
                {
                    //Retrieve the range obtained by the field code
                    DocumentPosition position = document.Fields[i].CodeRange.End;

                    //Insert the format switch to the end of the field code range
                    document.InsertText(position, @"\@ ""M/d/yyyy h:mm am/pm""");
                }
            }

            //Update all document fields
            document.Fields.Update();
        }
        /***********************************************************************************************/
        static void LineNumbering(Document document)
        {
            #region #LineNumbering
            //            document.LoadDocument("Grimm.docx", DevExpress.XtraRichEdit.DocumentFormat.OpenXml);
            document.Unit = DevExpress.Office.DocumentUnit.Inch;
            Section sec = document.Sections[0];
            sec.LineNumbering.CountBy = 2;
            sec.LineNumbering.Start = 1;
            sec.LineNumbering.Distance = 0.25f;
            sec.LineNumbering.RestartType = LineNumberingRestart.NewSection;
            #endregion #LineNumbering
        }

        static void CreateColumns(Document document)
        {
            #region #CreateColumns
            //            document.LoadDocument("Grimm.docx", DevExpress.XtraRichEdit.DocumentFormat.OpenXml);
            document.Unit = DevExpress.Office.DocumentUnit.Inch;
            // Get the first section in a document.
            Section firstSection = document.Sections[0];
            // Create equal width column layout.
            SectionColumnCollection sectionColumnsLayout =
                firstSection.Columns.CreateUniformColumns(firstSection.Page, 0.2f, 3);
            // Set different column width.
            sectionColumnsLayout[0].Width = 3f;
            sectionColumnsLayout[1].Width = 2f;
            sectionColumnsLayout[2].Width = 1f;
            // Apply layout to the document.
            firstSection.Columns.SetColumns(sectionColumnsLayout);
            #endregion #CreateColumns
        }

        static void PrintLayout(Document document)
        {
            #region #PrintLayout
            document.Unit = DevExpress.Office.DocumentUnit.Inch;
            document.Sections[0].Page.PaperKind = System.Drawing.Printing.PaperKind.A6;
            document.Sections[0].Page.Landscape = true;
            document.Sections[0].Margins.Left = 2.0f;
            #endregion #PrintLayout
        }

        static void TabStops(Document document)
        {
            #region #TabStops
            document.Unit = DevExpress.Office.DocumentUnit.Inch;
            TabInfoCollection tabs = document.Paragraphs[0].BeginUpdateTabs(true);
            DevExpress.XtraRichEdit.API.Native.TabInfo tab1 = new DevExpress.XtraRichEdit.API.Native.TabInfo();
            // Sets tab stop at 2.5 inch.
            tab1.Position = 2.5f;
            tab1.Alignment = TabAlignmentType.Left;
            tab1.Leader = TabLeaderType.MiddleDots;
            tabs.Add(tab1);
            DevExpress.XtraRichEdit.API.Native.TabInfo tab2 = new DevExpress.XtraRichEdit.API.Native.TabInfo();
            tab2.Position = 5.5f;
            tab2.Alignment = TabAlignmentType.Decimal;
            tab2.Leader = TabLeaderType.EqualSign;
            tabs.Add(tab2);
            document.Paragraphs[0].EndUpdateTabs(tabs);
            #endregion #TabStops
        }

        private void newFieldToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string fieldName = "";
            using (Ask askForm = new Ask("Enter New Field Name?"))
            {
                askForm.Text = "";
                askForm.ShowDialog();
                if (askForm.DialogResult != System.Windows.Forms.DialogResult.OK)
                    return;
                fieldName = askForm.Answer;
                if (String.IsNullOrWhiteSpace(fieldName))
                    return;
            }
            rtb.Document.BeginUpdate();

            //            fieldName = "MERGEFIELD FirstName \f" " }{ MERGEFIELD MiddleName \f" " }{ MERGEFIELD LastName";

            //Create a DATE field at the caret position

            //string commentAuthor = "Johnson Alphonso D";
            //DocumentRange docRange = rtb.Document.Paragraphs[0].Range;
            //rtb.Document.Comments.Create(docRange, commentAuthor, DateTime.Now);
            //DevExpress.XtraRichEdit.API.Native.Comment comment = rtb.Document.Comments[rtb.Document.Comments.Count - 1];

            //            rtb.Document.Fields.Create(rtb.Document.CaretPosition, fieldName);
            string field = "DOCPROPERTY ";
            field += @"";
            field += fieldName;
            field += @"""";
            rtb.Document.Fields.Create(rtb.Document.CaretPosition, field);
            rtb.Document.EndUpdate();
            //for (int i = 0; i < rtb.Document.Fields.Count; i++)
            //{
            //    string fieldCode = rtb.Document.GetText(rtb.Document.Fields[i].CodeRange);
            //    if (fieldCode == "DATE")
            //    {
            //        //Retrieve the range obtained by the field code
            //        DocumentPosition position = document.Fields[i].CodeRange.End;

            //        //Insert the format switch to the end of the field code range
            //        document.InsertText(position, @"\@ ""M/d/yyyy h:mm am/pm""");
            //    }
            //}

            //Update all document fields
            rtb.Document.Fields.Update();

//            CustomDocumentProperties(rtb.Document);
        }


        static void CustomDocumentProperties(Document document)
        {
            #region #CustomDocumentProperties
            document.BeginUpdate();
            document.AppendText("\nA new value of MyBookmarkProperty is obtained from here: NEWVALUE!\n");
            document.Bookmarks.Create(document.FindAll("NEWVALUE!", SearchOptions.CaseSensitive)[0], "bmOne");
            document.AppendText("\nMyNumericProperty: ");
            document.Fields.Create(document.Range.End, @"DOCPROPERTY ""MyNumericProperty""");
            document.AppendText("\nMyStringProperty: ");
            document.Fields.Create(document.Range.End, @"DOCPROPERTY ""MyStringProperty""");
            document.AppendText("\nMyBooleanProperty: ");
            document.Fields.Create(document.Range.End, @"DOCPROPERTY ""MyBooleanProperty""");
            document.AppendText("\nMyBookmarkProperty: ");
            document.Fields.Create(document.Range.End, @"DOCPROPERTY ""MyBookmarkProperty""");
            document.EndUpdate();

            //document.CustomProperties["MyNumericProperty"] = 123.45;
            //document.CustomProperties["MyStringProperty"] = "The Final Answer";
            //document.CustomProperties["MyBookmarkProperty"] = document.Bookmarks[0];
            //document.CustomProperties["MyBooleanProperty"] = true;

            document.Fields.Update();
            #endregion #CustomDocumentProperties
        }
        /***********************************************************************************************/
        private void processFieldsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            rtb.Document.BeginUpdate();
            rtb.Document.CustomProperties["[%DECNAME%"] = "The Final Answer";
            rtb.Document.Fields.Update();
            string text = rtb.Document.RtfText;
            text = ReplaceField(text, "[%BRANCHNAME%", "Colonial Chapel -Bay Springs, Raleigh, and Forest");
            text = ReplaceField(text, "[%DECNAME%", "Robby Graham");
            text = ReplaceField(text, "[%DECA%", "65");
            text = ReplaceField(text, "[%DECMARSTAT%", "M");
            text = ReplaceField(text, "[%SEX%", "Male");
            text = ReplaceField(text, "[%DRACE%", "White");
            text = ReplaceField(text, "[%DECADDRESS%", "351 Service Road");
            text = ReplaceField(text, "[%TMDETH%", "10:20 PM");
            text = ReplaceField(text, "[%SPOUSENAME%", "Mildred Lee Graham");
            text = ReplaceField(text, "[%CSEOFDETH%", "Natrual Causes");
            text = ReplaceField(text, "[%LI%", "0");

            rtb.Document.RtfText = text;
        }
        /***********************************************************************************************/
        private string ReplaceField(string text, string field, string replace)
        {
            int idx = -1;
            string str = "";
            StringBuilder sb = new StringBuilder(text);
            StringBuilder xb = new StringBuilder(replace);
            int count = 0;
            for (;;)
            {
                idx = text.IndexOf(field);
                if (idx < 0)
                    break;
                for (int i = idx; i < text.Length; i++)
                {
                    str = text.Substring(i, 1);
                    if (str == "]")
                    {
                        sb[i] = (char)127; // Unprintable Char, Causes Underline to be visible up to this char
                        break;
                    }
                    else
                    {
                        if (count < xb.Length)
                            sb[i] = xb[count];
                        else
                            sb[i] = ' ';
                        count++;
                    }
                }
                text = sb.ToString();
                count = 0;
            }
            text = sb.ToString();
            return text;
        }

        static void CreateFieldFromRange(Document document)
        {
            #region #CreateFieldFromRange
            document.BeginUpdate();
            //Insert the text to the document end
            document.AppendText("SYMBOL 0x54 \\f Wingdings \\s 24");
            document.EndUpdate();

            //Convert the inserted text to the field 
            document.Fields.Create(document.Paragraphs[0].Range);
            document.Fields.Update();
            #endregion #CreateFieldFromRange
        }

        private void processToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if ( 1 == 1)
            {
                processFieldsToolStripMenuItem_Click(null, null);
                return;
            }
            rtb.Document.BeginUpdate();
            for (int i = 0; i < rtb.Document.Fields.Count; i++)
            {
                string fieldCode = rtb.Document.GetText(rtb.Document.Fields[i].CodeRange);
                string resultCode = rtb.Document.GetText(rtb.Document.Fields[i].ResultRange);
                fieldCode = fieldCode.ToUpper();
                if (fieldCode == "DATE")
                {
                    //Retrieve the range obtained by the field code
                    DocumentPosition position = rtb.Document.Fields[i].CodeRange.End;

                    //Insert the format switch to the end of the field code range
                    rtb.Document.InsertText(position, @"\@ ""M/d/yyyy h:mm am/pm""");
                }
                else if (fieldCode.ToUpper() == "NAME")
                {
                    DocumentPosition position = rtb.Document.Fields[i].CodeRange.End;

                    //Insert the format switch to the end of the field code range
                    rtb.Document.InsertText(position, @"\@ 222""Robby Graham""xyzzy");
                }
                else if (fieldCode.ToUpper() == "ADDRESS")
                {
                    DocumentPosition position = rtb.Document.Fields[i].CodeRange.End;

                    //Insert the format switch to the end of the field code range
                    rtb.Document.InsertText(position, @"\@ ""351 Service Road""abc123");
                }
            }
            rtb.Document.CustomProperties["MyNumericProperty"] = 123.45;
            rtb.Document.CustomProperties["MyStringProperty"] = "The Final Answer";
            rtb.Document.CustomProperties["MyBookmarkProperty"] = rtb.Document.Bookmarks[0];
            rtb.Document.CustomProperties["MyBooleanProperty"] = true;
            rtb.Document.EndUpdate();
            rtb.Document.Fields.Update();

        }

        private void lookToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < rtb.Document.Fields.Count; i++)
            {
                string fieldCode = rtb.Document.GetText(rtb.Document.Fields[i].CodeRange);
                string resultCode = rtb.Document.GetText(rtb.Document.Fields[i].ResultRange);
                fieldCode = fieldCode.ToUpper();

            }
        }
    }
}

