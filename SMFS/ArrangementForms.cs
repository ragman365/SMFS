using System;
using System.IO;
using System.Data;
using System.Drawing;
//using System.Drawing.Printing;
using GeneralLib;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using DevExpress.Utils;
using DevExpress.XtraPrinting;
using DevExpress.XtraRichEdit.API.Native;
using DevExpress.XtraRichEdit;
using DevExpress.Utils.Extensions;
using EMRControlLib;
using DevExpress.Office.Utils;
using DevExpress.XtraRichEdit;
using DevExpress.XtraRichEdit.API.Native;
using System.Diagnostics;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class ArrangementForms : DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        private bool workLetters = false;
        private string workRecord = "";
        private string workFormName = "";
        private string pdfRecord = "";
        private string workContractNumber = "";
        private bool workContract = false;
        private string workLocation = "";
        private string workFile = "";
        private bool createRecord = false;
        private bool modified = false;
        private byte[] filearray = null;
        private bool workSeparate = false;
        private bool removeEmpty = false;
        private bool workForceSave = false;
        private bool forceNoBorder = false;
        private bool first = true;
        private bool workingText = false;
        private string workingMemoryText = "";
        private bool workFromForm = false;
        private bool loading = true;
        /***********************************************************************************************/
        public ArrangementForms(string form, string location, string record, string contractNumber, byte[] array, bool allowSeperate = false, bool noBorder = false, string memoryTitle = "", bool fromForm = false )
        {
            InitializeComponent();
            workContractNumber = contractNumber;
            if (!String.IsNullOrWhiteSpace(contractNumber))
                workContract = true;
            btnSave.Hide();
            filearray = array;
            workRecord = record;
            workFormName = form;
            workLocation = location;
            workSeparate = allowSeperate;
            forceNoBorder = noBorder;
            workFromForm = fromForm;
            if (!String.IsNullOrWhiteSpace(memoryTitle))
            {
                workingText = true;
                workingMemoryText = memoryTitle;
            }
        }
        /***********************************************************************************************/
        public ArrangementForms(string record, string formName, string location, string contractNumber)
        {
            InitializeComponent();
            workRecord = record;
            workFormName = formName;
            workLocation = location;
            workContractNumber = contractNumber;
            if (!String.IsNullOrWhiteSpace(contractNumber))
                workContract = true;
            btnSave.Hide();
        }
        /***********************************************************************************************/
        public ArrangementForms(string filename, string location, string contractNumber, bool forceSave = false )
        {
            InitializeComponent();
            workFile = filename;
            workLocation = location;
            workContractNumber = contractNumber;
            if (!String.IsNullOrWhiteSpace(contractNumber))
                workContract = true;
            btnSave.Hide();
            workForceSave = forceSave;
        }
        /***********************************************************************************************/
        public ArrangementForms ( bool letters )
        {
            InitializeComponent();
            workLetters = letters;
            btnSave.Hide();
        }
        /***********************************************************************************************/
        //public ArrangementForms(string rtfText )
        //{
        //    InitializeComponent();
        //    workFile = "";
        //    workLocation = "";
        //    workContractNumber = "";
        //    workingText = true;
        //    workingRtfText = rtfText;
        //    btnSave.Hide();
        //}
        /***********************************************************************************************/
        private bool allowWord = false;
        private void LoadFileArray()
        {
            if (filearray == null)
                return;

            WaitCursor();

            btnOpenWord.Hide();

            string str = G1.ConvertToString(filearray);
            if (str.IndexOf("rtf1") > 0)
            {
                byte[] bytes = Encoding.ASCII.GetBytes(str);

                MemoryStream stream = new MemoryStream(bytes);

                rtb.Document.Delete(rtb.Document.Range);

                rtb.Document.LoadDocument(stream, DevExpress.XtraRichEdit.DocumentFormat.Rtf);
                if (workContract)
                    LoadAgreementData();
                //else
                //    MajorTest();

                this.BringToFront();
                allowWord = true;
                btnOpenWord.Show();
                btnOpenWord.Refresh();
                //if (G1.isAdmin())
                //{
                //    string filePath = @"c:\rag\rawRtfText.rtf";

                //    if (File.Exists(filePath))
                //    {
                //        File.SetAttributes(filePath, FileAttributes.Normal);
                //        File.Delete(filePath);
                //    }

                //    //write the raw RTF string to a text file.
                //    System.IO.StreamWriter rawTextFile = new System.IO.StreamWriter(filePath, false);
                //    str = rtb.RtfText.ObjToString();
                //    rawTextFile.Write(str);
                //    rawTextFile.Close();

                //    ShowExternalReference(filePath, true);

                //    //now open the RTF file using word.
                //    //Microsoft.Office.Interop.Word.Application msWord = new Microsoft.Office.Interop.Word.Application();
                //    //msWord.Visible = true;
                //    //Microsoft.Office.Interop.Word.Document wordDoc = msWord.Documents.Open(filePath);
                //}
            }

            DefaultCursor();
        }
        public bool ShowExternalReference(string externalRef, bool waitForCompletion)
        {
            bool gotError = false;
            if (externalRef.Length > 0)
            {
                Microsoft.Office.Interop.Word.Application myWordApp = null;
                var pInfo = new ProcessStartInfo { FileName = externalRef };
                bool isrunning = false;
                Process[] pList = Process.GetProcesses();
                foreach (Process x in pList)
                {
                    if (x.ProcessName.Contains("WINWORD"))
                    {
                        isrunning = true;
                        try
                        {
                            myWordApp = System.Runtime.InteropServices.Marshal.GetActiveObject("Word.Application") as Microsoft.Office.Interop.Word.Application;
                            if (myWordApp.ActiveDocument.FullName.Contains(externalRef))
                            {
                                // do something
                                MessageBox.Show("***ERROR***, Something went wrong!\nPerhaps Word is already Open!\nIf so, close it!", "Word Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);

                                //myWordApp.ActiveDocument.Content.Text = " already open";
                            }
                        }
                        catch ( Exception ex)
                        {
                            MessageBox.Show("***ERROR***, Something went wrong!\nPerhaps Word is already Open!\nIf so, close it!\n" + ex.Message, "Word Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                        }
                    }
                }
                if (!isrunning)
                {
                    // Start the process.
                    Process p = Process.Start(pInfo);

                    if (waitForCompletion)
                    {
                        // Wait for the window to finish loading.
                        p.WaitForInputIdle();

                        // Wait for the process to end.
                        p.WaitForExit();

                        //string str = myWordApp.ActiveDocument.Content.ObjToString();
                    }
                }
                else
                {
                    gotError = true;
                    MessageBox.Show("***ERROR***, Something went wrong!\nPerhaps Word is already Open!\nIf so, close it!", "Word Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                }
            }
            return gotError;
        }
        /***********************************************************************************************/
        private void MajorTest()
        {
            DevExpress.XtraRichEdit.RichEditControl rtb2 = new RichEditControl();
            //rtb2.Document.Delete(rtb2.Document.Range);
            //string filename = @"C:\Users\Robby\Documents\SMFS\WordStuff\Cliff\case_file_document.rtf";
            //rtb2.Document.LoadDocument(filename, DocumentFormat.Undefined);
            //rtb.Document.AppendRtfText(rtb2.Document.RtfText);

            rtb2.Document.Delete(rtb2.Document.Range);
            string filename = @"C:\Users\Robby\Documents\SMFS\WordStuff\Cliff\case_file_document2.rtf";
            rtb2.Document.LoadDocument(filename, DevExpress.XtraRichEdit.DocumentFormat.Undefined);
            rtb.Document.AppendRtfText(rtb2.Document.RtfText);
        }
        /***********************************************************************************************/
        private void ArrangementForms_Load(object sender, EventArgs e)
        {
            lblTitles.Hide();
            cmbTitles.Hide();
            chkAccept.Hide();
            btnSeparate.Show();
            btnSeparate.Hide();
            if ( !LoginForm.administrator )
            {
                btnEditDetails.Hide();
                btnRemoveEmpty.Hide();
            }
            if (String.IsNullOrWhiteSpace(workContractNumber))
                btnRemoveEmpty.Hide();
            else
            {
                //if (!LoginForm.administrator)
                //{
                //    btnRemoveEmpty.Text = "Show Empty Fields";
                //    removeEmpty = true;
                //    //LoadAgreementData();
                //}
            }
            if (!workSeparate)
                btnSeparate.Hide();
            //else
            //    this.ribbonControl1.Minimized = true;
            string tempLocation = workLocation;
            if (String.IsNullOrWhiteSpace(tempLocation))
                tempLocation = "Generic";
            this.Text = "Arrangement Form " + workFormName + " (" + tempLocation + ")";
            if (String.IsNullOrWhiteSpace(workFormName))
                this.Text = workFile;
            if ( !String.IsNullOrWhiteSpace ( workContractNumber))
            { // Working Contract
                tableToolsRibbonPageCategory1.Visible = false;
                tableLayoutRibbonPage1.Visible = false;
                rtb.Options.HorizontalRuler.Visibility = RichEditRulerVisibility.Hidden;
                rtb.Options.VerticalRuler.Visibility = RichEditRulerVisibility.Hidden;
                ribbonControl1.AllowMinimizeRibbon = true;
                ribbonControl1.Visible = false;
            }
            else
            {
                tableToolsRibbonPageCategory1.Visible = true;
                tableLayoutRibbonPage1.Visible = true;
                rtb.Options.HorizontalRuler.Visibility = RichEditRulerVisibility.Visible;
                rtb.Options.VerticalRuler.Visibility = RichEditRulerVisibility.Visible;
            }

            if (1 == 1)
            {
                WaitCursor();

                if (filearray != null)
                    LoadFileArray();
                else if (!String.IsNullOrWhiteSpace(workFile))
                {
                    rtb.Document.Delete(rtb.Document.Range);

                    rtb.Document.LoadDocument(workFile, DevExpress.XtraRichEdit.DocumentFormat.Undefined);
                    rtb.Show();
                    this.BringToFront();
                }
                if ( forceNoBorder )
                {
                    rtb.Document.Sections[0].Margins.Left = 0;
                    rtb.Document.Sections[0].Margins.Right = 0;
                    rtb.Document.Sections[0].Margins.Top = 0;
                    rtb.ActiveViewType = RichEditViewType.Simple;
                    //this.ribbonControl1.Minimized = false;
                    //ribbonControl1.AllowMinimizeRibbon = true;
                    //ribbonControl1.Visible = true;
                }
                if (workingText )
                {
                    string content = RTF_Stuff.LoadGeneralTitle(workingMemoryText );

                    content = content.Replace("\n", "\\line ");
                    string text = this.rtb.Document.RtfText;

                    text = RTF_Stuff.ReplaceField(text, "[*MEMTITLE*]", workingMemoryText);
                    text = RTF_Stuff.ReplaceField(text, "[*MEMCONTENT*]", content);

                    this.rtb.Document.RtfText = text;
                    this.WindowState = FormWindowState.Maximized;

                    cmbTitles.Hide();
                    lblTitles.Hide();
                    chkAccept.Show();

                }
                DefaultCursor();
                if ( first )
                    btnRemoveEmpty_Click(null, null);
                first = false;
                ribbonControl1.Visible = true;
                btnShowRibbon.Text = "Hide Ribbon Controls";
                //CheckPrintLayout();
                //this.menuStrip1.Hide();
                return;
            }
            WaitCursor();
            modified = false;
            if (!String.IsNullOrWhiteSpace(workFile))
            {
                rtb.Document.Delete(rtb.Document.Range);

                rtb.Document.LoadDocument(workFile, DevExpress.XtraRichEdit.DocumentFormat.Undefined);
                rtb.Show();
                DefaultCursor();
                this.BringToFront();
                return;
            }
            string form = workFormName;
            if (!String.IsNullOrWhiteSpace(workContractNumber))
                form = workContractNumber + " " + workFormName;

            string cmd = "Select * from `pdfimages` where `filename` = '" + form + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0 && workContract)
            {
                cmd = "Select * from `pdfimages` where `filename` = '" + workFormName + "';";
                dt = G1.get_db_data(cmd);
                if (workContract)
                    createRecord = true;
            }
            bool loaded = false;
            if (dt.Rows.Count > 0)
            {
                pdfRecord = dt.Rows[0]["record"].ObjToString();
                string str = G1.get_db_blob("pdfimages", pdfRecord, "image");
                if (str.IndexOf("rtf1") > 0)
                {
                    byte[] bytes = Encoding.ASCII.GetBytes(str);

                    MemoryStream stream = new MemoryStream(bytes);

                    rtb.Document.Delete(rtb.Document.Range);

                    rtb.Document.LoadDocument(stream, DevExpress.XtraRichEdit.DocumentFormat.Rtf);
                    loaded = true;
                }
            }
            if (workContract && loaded)
                LoadAgreementData();
            DefaultCursor();
            this.BringToFront();
            loading = false;
        }
        /***********************************************************************************************/
        public static string ReplaceField(string text, string field, string replace, Image image = null )
        {
            if ( field.ToUpper().IndexOf( "PRECEDED3") >= 0 )
            {

            }
            int originalIdx = -1;
            int idx = -1;
            string str = "";
            if (field.IndexOf("[*") >= 0)
            {
                idx = text.IndexOf(field);
                if (idx > 0)
                    originalIdx = idx;
                if (replace.ToUpper().IndexOf("MULTI-RTF") >= 0)
                {
                    string newData = "";
                    string[] Lines = replace.Split('\n');
                    for (int i = 1; i < Lines.Length; i++)
                    {
                        str = Lines[i].TrimEnd();
                        str = str.TrimEnd('-');
                        str = str.Replace("-", "      ");
                        newData += str.Trim() + "MULTI_RTF";
                    }
                    RichTextBox rtb = new RichTextBox();
                    rtb.AppendText("");
                    rtb.SelectAll();
                    rtb.SelectionAlignment = HorizontalAlignment.Center;
                    newData = newData.Replace("MULTI_RTF", rtb.Rtf);
                    text = text.Replace(field, newData);
                }
                else
                    text = text.Replace(field, replace);
                return text;
            }
            str = field.Substring(0, 1);
            if (str == "$")
            {
                idx = text.IndexOf(field);
                text = text.Replace(field, replace);
                return text;
            }
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
                        {
                            sb[i] = ' ';
                        }
                        count++;
                    }
                }
                text = sb.ToString();
                count = 0;
            }
            text = sb.ToString();
            if (image != null )
            {
                //Clipboard.SetImage(signature);
                //rtb.SelectionStart = 0;
                //rtb.Paste();
                PictureBox pic = new PictureBox();
                pic.Image = image;
                //pic.Image = ScaleImage(pic.Image, 0.30F, 0.30F);
                pic.SizeMode = PictureBoxSizeMode.AutoSize;
                //rtb.Controls.Add(pic);
                //                rtb.InsertImage(signature);
                //picPurchaser.Image = signature;
                //picCoPurchaser.Image = signature;
                //                pic.Image = signature;
                RichTextBoxEx rtb = new RichTextBoxEx();
                rtb.Rtf = text;
                rtb.SelectionStart = 0;
                //rtb.SelectionStart = originalIdx;
                rtb.InsertImage(image);
                //text = rtb.Rtf;
                rtb.SelectAll();
                rtb.SelectionAlignment = HorizontalAlignment.Center;
                rtb.Controls.Add(pic);
                //newData = newData.Replace("MULTI_RTF", rtb.Rtf);
                //text = text.Replace(field, newData);
                text = rtb.Rtf;
            }
            return text;
        }
        /***********************************************************************************************/
        public static string ReplaceField2(string text, string field, string replace, Image image = null, int position = -1 )
        {
            if (field.ToUpper().IndexOf("PRECEDED3") >= 0)
            {

            }
            int originalIdx = -1;
            int idx = -1;
            string str = "";
            RichTextBoxEx rtb3 = new RichTextBoxEx();
            rtb3.Rtf = text;

            if (field.IndexOf("[*") >= 0)
            {
                idx = text.IndexOf(field);
                if (replace.ToUpper().IndexOf("MULTI-RTF") >= 0)
                {
                    string newData = "";
                    string[] Lines = replace.Split('\n');
                    for (int i = 1; i < Lines.Length; i++)
                    {
                        str = Lines[i].TrimEnd();
                        str = str.TrimEnd('-');
                        str = str.Replace("-", "      ");
                        newData += str.Trim() + "MULTI_RTF";
                    }
                    RichTextBox rtb = new RichTextBox();
                    rtb.AppendText("");
                    rtb.SelectAll();
                    rtb.SelectionAlignment = HorizontalAlignment.Center;
                    newData = newData.Replace("MULTI_RTF", rtb.Rtf);
                    text = text.Replace(field, newData);
                }
                else
                    text = text.Replace(field, replace);
                return text;
            }
            str = field.Substring(0, 1);
            if (str == "$")
            {
                idx = text.IndexOf(field);
                text = text.Replace(field, replace);
                return text;
            }
            StringBuilder sb = new StringBuilder(text);
            StringBuilder xb = new StringBuilder(replace);
            originalIdx = text.IndexOf(field);
            int count = 0;
            for (; ; )
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
                        {
                            sb[i] = ' ';
                        }
                        count++;
                    }
                }
                text = sb.ToString();
                count = 0;
            }
            text = sb.ToString();
            if (image != null && originalIdx >= 0 )
            {
                PictureBox pic = new PictureBox();
                pic.Image = image;
                if (pic.Image != null && pic.Image.Width > 3 )
                {
                    pic.Image = Contract1.ScaleImage(pic.Image, 0.30F, 0.30F);
                    pic.SizeMode = PictureBoxSizeMode.Normal;
                    Clipboard.SetImage(pic.Image);
                }
                //rtb.SelectionStart = 0;
                //rtb.Paste();
                //rtb.Controls.Add(pic);
                //                rtb.InsertImage(signature);
                //picPurchaser.Image = signature;
                //picCoPurchaser.Image = signature;
                //                pic.Image = signature;
                RichTextBoxEx rtb = new RichTextBoxEx();
                rtb.SelectionStart = 0;
                rtb.Paste();

                //// Paste it into the rich tetx box.
                //rtb.Paste();
                //rtb.InsertImage(pic.Image);
                //rtb.AppendText("k ds dsk adsk dfs ");
                RichTextBoxEx rtb2 = new RichTextBoxEx();
                rtb2.Rtf = text;
                if (position >= 0)
                    originalIdx = position;
                else if (position < -1)
                    originalIdx += position;
                rtb2.SelectionStart = originalIdx;
                //rtb2.SelectionStart = 0;
                rtb2.SelectedRtf = rtb.Rtf;
                //text = rtb.Rtf;
                //rtb.SelectAll();
                //rtb.SelectionAlignment = HorizontalAlignment.Center;
                //rtb.Controls.Add(pic);
                //newData = newData.Replace("MULTI_RTF", rtb.Rtf);
                //text = text.Replace(field, newData);

                //rtb2.AppendRtf(rtb.Rtf);

                //ViewRTF vForm2 = new ViewRTF(rtb2.Rtf);
                //vForm2.ShowDialog();

                text = rtb2.Rtf;
                //text = rtb.Rtf;

                //rtb.SaveFile("c:/rag/testImage.rtf"); // This worked, so I have an image in the rtb
            }
            return text;
        }
        /***********************************************************************************************/
        private void btnSave_ClickOld(object sender, EventArgs e)
        {
            string record = pdfRecord;
            if (String.IsNullOrWhiteSpace(record))
                record = G1.create_record("pdfimages", "filename", "-1");
            if (String.IsNullOrWhiteSpace(record))
            {
                MessageBox.Show("***ERROR*** Creating Image Record!");
                return;
            }
            if (record == "0" || record == "-1")
            {
                MessageBox.Show("***ERROR*** Creating Image Record!");
                return;
            }

            string form = workFormName;
            if (workContract)
                form = workContract + " " + workFormName;

            G1.update_db_table("pdfimages", "record", record, new string[] { "filename", form });

            MemoryStream s = new MemoryStream();
            rtb.Document.SaveDocument(s, DevExpress.XtraRichEdit.DocumentFormat.Rtf);

            byte[] b = G1.GetBytesFromStream(s);
            G1.update_blob("pdfimages", "record", record, "image", b);
            s.Close();
            btnSave.Hide();
        }
        /***********************************************************************************************/
        private void rtb_KeyUp(object sender, KeyEventArgs e)
        {
            modified = true;
            if ( RtfModified != null)
                RtfModified.Invoke();

            //if (String.IsNullOrWhiteSpace(workContractNumber))
            btnSave.Show();
        }
        /***************************************************************************************/
        private void SetFont(DevExpress.XtraRichEdit.RichEditControl rtb, string fontname, float size)
        {
            //if (!String.IsNullOrWhiteSpace(fontname))
            //{
            //    Paragraph currentParagraph = rtb.Document.GetParagraph(rtb.Document.CaretPosition);

            //    ParagraphProperties ppp = rtb.Document.BeginUpdateParagraphs(currentParagraph.Range);
            //    ppp.Alignment = DevExpress.XtraRichEdit.API.Native.ParagraphAlignment.Left;
            //    //                ppp.BackColor = Color.LightGray;
            //    rtb.Document.EndUpdateParagraphs(ppp);
            //    CharacterProperties cp = rtb.Document.BeginUpdateCharacters(currentParagraph.Range);
            //    cp.Bold = false;
            //    cp.FontSize = size;
            //    cp.FontName = fontname;
            //    rtb.Document.EndUpdateCharacters(cp);
            //}
            //else
            //{
            //    Paragraph currentParagraph = rtb.Document.GetParagraph(rtb.Document.CaretPosition);

            //    ParagraphProperties ppp = rtb.Document.BeginUpdateParagraphs(currentParagraph.Range);
            //    ppp.Alignment = DevExpress.XtraRichEdit.API.Native.ParagraphAlignment.Left;
            //    //                ppp.BackColor = Color.LightGray;
            //    rtb.Document.EndUpdateParagraphs(ppp);
            //    CharacterProperties cp = rtb.Document.BeginUpdateCharacters(currentParagraph.Range);
            //    cp.Bold = false;
            //    cp.FontSize = size;
            //    cp.FontName = "Times New Roman";
            //    rtb.Document.EndUpdateCharacters(cp);
            //}
        }
        /***********************************************************************************************/
        private void AppendToTable(DataTable dt, string service, string data, string font, float size, string font2 = "", float size2 = 0f)
        {
            DataRow dR = dt.NewRow();
            dR["service"] = service;
            dR["data"] = data;
            dR["font"] = font;
            dR["size"] = size;
            dR["font2"] = font2;
            dR["size2"] = size2;
            dt.Rows.Add(dR);
        }
        /***********************************************************************************************/
        private double TotalUpTable(DataTable dt)
        {
            double dValue = 0D;
            double total = 0D;
            string data = "";
            bool found = false;
            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    data = dt.Rows[i]["data"].ObjToString();
                    data = data.Replace("$", "");
                    if (!G1.validate_numeric(data))
                        continue;
                    dValue = data.ObjToDouble();
                    total += dValue;
                    found = true;
                }
                data = G1.ReformatMoney(total);
                AppendToTable(dt, "       TOTAL :", data, "Arial", 7f, "Lucida Console", 7f);
            }
            catch (Exception ex)
            {
            }
            return total;
        }
        /***********************************************************************************************/
        private void LoadServicesTable(string searchText)
        {
            DateTime start = DateTime.Now;
            string text = rtb.Document.RtfText;
            if (text.IndexOf(searchText) < 0)
                return;

            DataTable[] allDt = new DataTable[8];
            for (int i = 0; i < 8; i++)
            {
                allDt[i] = new DataTable();
                allDt[i].Columns.Add("service");
                allDt[i].Columns.Add("data");
                allDt[i].Columns.Add("font");
                allDt[i].Columns.Add("size", Type.GetType("System.Double"));
                allDt[i].Columns.Add("font2");
                allDt[i].Columns.Add("size2", Type.GetType("System.Double"));
                if (i == 0)
                {
                    AppendToTable(allDt[i], "A. CHARGE FOR SERVICES:", "", "Arial Black", 8f);
                    AppendToTable(allDt[i], "Professional Services:", "", "Arial Black", 7f);
                }
                else if (i == 1)
                    AppendToTable(allDt[i], "Additional Services and Fees:", "", "Arial Black", 7f);
                else if (i == 2)
                    AppendToTable(allDt[i], "Automotive Equipment:", "", "Arial Black", 7f);
                else if (i == 3)
                    AppendToTable(allDt[i], "B. CHARGE FOR MERCHANDISE:", "", "Arial Black", 7f);
                else if (i == 4)
                    AppendToTable(allDt[i], "C. SPECIAL CHARGES:", "", "Arial Black", 7f);
                else if (i == 5)
                    AppendToTable(allDt[i], "D. CASH ADVANCE:", "", "Arial Black", 7f);
                else if (i == 6)
                    AppendToTable(allDt[i], "SUMMARY OF CHARGES:", "", "Arial Black", 8f);
                else if (i == 7)
                {
                    AppendToTable(allDt[i], "", "", "Arial Black", 8f);
                    AppendToTable(allDt[i], "PAYMENTS:", "", "Arial Black", 8f);
                }
            }

            string cmd = "Select * from `cust_services` where `contractNumber` = '" + workContractNumber + "';";
            DataTable dt = G1.get_db_data(cmd);

            int idx = text.IndexOf(searchText);
            int count = 0;
            string service = "";
            string data = "";
            double dValue = 0D;
            string localText = "";
            string type = "";

            DataTable funDt = null;
            double total = 0D;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                funDt = null;
                service = dt.Rows[i]["service"].ObjToString();
                if (String.IsNullOrWhiteSpace(service))
                    continue;
                cmd = "Select * from `services` where `service` = '" + service + "';";
                DataTable dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    type = dx.Rows[0]["type"].ObjToString();
                    if (type.ToUpper() == "SERVICE")
                        funDt = allDt[0];
                    else if (type.ToUpper() == "ADDITIONAL")
                        funDt = allDt[1];
                    else if (type.ToUpper() == "AUTOMOTIVE")
                        funDt = allDt[2];
                    else if (type.ToUpper() == "MERCHANDICE")
                        funDt = allDt[3];
                    else if (type.ToUpper() == "SPECIAL")
                        funDt = allDt[4];
                    else if (type.ToUpper() == "CASH ADVANCE")
                        funDt = allDt[5];
                    else if (type.ToUpper() == "DISCOUNT")
                        funDt = allDt[7];
                }
                if (funDt == null)
                    funDt = allDt[0];
                data = dt.Rows[i]["data"].ObjToString();
                if (!G1.validate_numeric(data))
                    continue;
                dValue = data.ObjToDouble();
                if (dValue == 0D)
                    continue;
                //                total += dValue;

                //                AppendToTable(funDt, "   " + service, data, "Arial", 7f);
                AppendToTable(funDt, "   " + service, data, "Arial", 7f, "Lucida Console", 7f);
            }

            double professionalServices = TotalUpTable(allDt[0]);
            double additionalServices = TotalUpTable(allDt[1]);
            double automotiveServices = TotalUpTable(allDt[2]);
            double merchandice = TotalUpTable(allDt[3]);
            double specialCharges = TotalUpTable(allDt[4]);
            double cashAdvance = TotalUpTable(allDt[5]);
            double totalServices = professionalServices + additionalServices + automotiveServices;
            double tax = 0D;

            double totalTotal = totalServices + merchandice + specialCharges + cashAdvance + tax;

            data = G1.ReformatMoney(totalServices);
            data = data.PadLeft(12);
            AppendToTable(allDt[6], "   A. CHARGES FOR SERVICES:", data, "Arial", 7f, "Lucida Console", 7f);
            data = G1.ReformatMoney(merchandice);
            data = data.PadLeft(12);
            AppendToTable(allDt[6], "   B. CHARGES FOR MERCHANDISE:", data, "Arial", 7f, "Lucida Console", 7f);
            data = G1.ReformatMoney(specialCharges);
            data = data.PadLeft(12);
            AppendToTable(allDt[6], "   C. SPECIAL CHARGES:", data, "Arial", 7f, "Lucida Console", 7f);
            data = G1.ReformatMoney(cashAdvance);
            data = data.PadLeft(12);
            AppendToTable(allDt[6], "   D. CASH ADVANCES:", data, "Arial", 7f, "Lucida Console", 7f);
            data = G1.ReformatMoney(tax);
            data = data.PadLeft(12);
            AppendToTable(allDt[6], "   E. SALES TAX, IF APPLICABLE :", data, "Arial", 7f, "Lucida Console", 7f);
            AppendToTable(allDt[6], "", "", "Arial", 7f, "Lucida Console", 7f);
            data = G1.ReformatMoney(totalTotal);
            data = data.PadLeft(12);
            AppendToTable(allDt[6], "      TOTAL FUNERAL HOME CHARGES", data, "Arial Black", 7f, "Lucida Console", 7f);

            double totalCredit = 0D;
            data = G1.ReformatMoney(totalCredit);
            data = data.PadLeft(12);
            AppendToTable(allDt[7], "               TOTAL CREDIT ", data, "Arial", 7f, "Lucida Console", 7f);

            double balanceDue = totalTotal - totalCredit;
            data = G1.ReformatMoney(balanceDue);
            data = data.PadLeft(12);
            AppendToTable(allDt[7], "        BALANCE DUE ", data, "Arial", 7f, "Lucida Console", 7f);
            AppendToTable(allDt[7], "", "", "Arial", 7f, "Lucida Console", 7f);

            DateTime afterStart = DateTime.Now;

            LoadRtfTable(allDt, searchText);

            DateTime afterLoad = DateTime.Now;

            TimeSpan tsAfterStart = afterStart - start;
            TimeSpan tsAfterLoad = afterLoad - afterStart;
        }
        /***********************************************************************************************/
        private void LoadDecPic1( string options = "" )
        {
            string searchText = "[*DECPIC*]";
            int idx = rtb.Document.RtfText.IndexOf(searchText);
            if (idx < 0)
                return;

            float myWidth = -1.0F;
            bool myBorder = false;
            if (FunForms.pictureBorder > 0)
                myBorder = true;
            string str = "";

            if ( !String.IsNullOrWhiteSpace ( options ))
            {
                string[] myOptions = options.Split('+');
                for ( int i=0; i<myOptions.Length; i++)
                {
                    if ( myOptions[i].ToUpper().IndexOf ( "W=") >= 0 )
                    {
                        str = myOptions[i].Replace("W=", "");
                        if (G1.validate_numeric(str))
                            myWidth = str.ObjToFloat();
                    }
                    else if ( myOptions[i].ToUpper().IndexOf ( "B=") >= 0 )
                    {
                        str = myOptions[i].Replace("B=", "");
                        if (str.ToUpper() == "Y")
                            myBorder = true;
                    }
                }
            }
            //string str = rtb.Document.Text.Trim();
            //int ddx = str.IndexOf("[*DECPIC");
            //if (ddx < 0)
            //    return;
            //string str2 = str.Substring(ddx+8);
            //ddx = str2.IndexOf("*]");
            //string str3 = str2.Substring(0, ddx);
            //str3 = str3.Replace("/", "");
            //float width = 3.5F;
            //if (G1.validate_numeric(str3))
            //    width = str3.ObjToFloat();
            //searchText += "/" + str3 + "*]";
            float width = 3.5F;
            if (myWidth > 0F)
                width = myWidth;

            string cmd = "Select * from `customers` where `contractNumber` = '" + workContractNumber + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
            {
                cmd = "Select * from `fcustomers` where `contractNumber` = '" + workContractNumber + "';";
                dt = G1.get_db_data(cmd);
                if ( dt.Rows.Count <= 0 )
                    return;
            }
            Byte[] bytes = dt.Rows[0]["picture"].ObjToBytes();
            Image myImage = new Bitmap(1, 1);
            if (bytes != null)
            {
                myImage = G1.byteArrayToImage(bytes);
                int iwidth = (int)(width * 100f);
                Image newImage = ResizeImage(myImage, new Size(iwidth, iwidth), true);
                myImage = newImage;
            }
            else
            {
                cmd = "Select * from `fcustomers` where `contractNumber` = '" + workContractNumber + "';";
                dt = G1.get_db_data(cmd);
                if (dt.Rows.Count <= 0)
                    return;
                bytes = dt.Rows[0]["picture"].ObjToBytes();
                myImage = new Bitmap(1, 1);
                width = 3.5f;
                if (myWidth > 0F)
                    width = myWidth;
                if (bytes != null)
                {
                    myImage = G1.byteArrayToImage(bytes);
                    int iwidth = (int)(width * 100f);
                    Image newImage = ResizeImage(myImage, new Size(iwidth, iwidth), true);
                    myImage = newImage;
                }
            }
            PictureBox pic = new PictureBox();
            pic.Image = myImage;
            pic.SizeMode = PictureBoxSizeMode.StretchImage;
            Image Img = pic.Image;
            byte[] inputImage = new byte[Img.Width * Img.Height];
            DocumentRange[] ranges = rtb.Document.FindAll(searchText, SearchOptions.None);
            DocumentPosition myStart = ranges[0].Start;

            //Shape myPicture = rtb.Document.Shapes.InsertPicture(rtb.Document.CaretPosition, DocumentImageSource.FromImage(Img));
            //Shape myPicture = rtb.Document.Shapes.InsertPicture(myStart, DocumentImageSource.FromImage(Img));
            //myPicture.RelativeHorizontalPosition = ShapeRelativeHorizontalPosition.LeftMargin;
            //myPicture.TextWrapping = TextWrappingType.Tight;

            //if (1 == 1)
            //{
            //    string t = rtb.Document.RtfText;
            //    if (bytes != null)
            //        rtb.Document.RtfText = t.Replace(searchText, "");
            //    return;
            //}

            //rtb.Document.InsertPicture(myStart, DocumentImageSource.FromImage(myImage));
            //Table table = rtb.Document.Tables.Create(myStart, 1, 2, AutoFitBehaviorType.AutoFitToContents);
            Table table = rtb.Document.Tables.Create(myStart, 1, 2, AutoFitBehaviorType.AutoFitToContents);

            table.Rows[0].FirstCell.PreferredWidthType = WidthType.Fixed;
            table.Rows[0].FirstCell.PreferredWidth = DevExpress.Office.Utils.Units.InchesToDocumentsF(0.6f);
            //table.Rows[0].FirstCell.PreferredWidth = DevExpress.Office.Utils.Units.InchesToDocumentsF(width);
            table.Rows[0].FirstCell.HeightType = HeightType.Exact;
            table[0, 0].HeightType = HeightType.Exact;
            table[0, 0].Height = DevExpress.Office.Utils.Units.InchesToDocumentsF(width);

            //Set the second column width and cell height
            table[0, 1].PreferredWidthType = WidthType.Fixed;
            //table[0, 1].PreferredWidthType = WidthType.None;
            table[0, 1].PreferredWidth = DevExpress.Office.Utils.Units.InchesToDocumentsF(width);
            table[0, 1].HeightType = HeightType.Auto;
            //table[0, 1].Height = DevExpress.Office.Utils.Units.InchesToDocumentsF(width - 0.5F);
            table[0, 1].Height = DevExpress.Office.Utils.Units.InchesToDocumentsF(width-0.5F);
            table[0, 1].VerticalAlignment = TableCellVerticalAlignment.Center;

            //rtb.Document.Images.Insert(table[0, 0].Range.Start, DocumentImageSource.FromImage(myImage));
            rtb.Document.Images.Insert(table[0, 1].Range.Start, DocumentImageSource.FromImage(myImage));
            //rtb.Document.InsertPicture(table[0, 1].Range.Start, DocumentImageSource.FromImage(myImage));
            table[0, 0].Borders.Bottom.LineStyle = TableBorderLineStyle.None;
            table[0, 0].Borders.Top.LineStyle = TableBorderLineStyle.None;
            table[0, 0].Borders.Left.LineStyle = TableBorderLineStyle.None;
            table[0, 0].Borders.Right.LineStyle = TableBorderLineStyle.None;
            table[0, 1].Borders.Bottom.LineStyle = TableBorderLineStyle.None;
            table[0, 1].Borders.Top.LineStyle = TableBorderLineStyle.None;
            table[0, 1].Borders.Left.LineStyle = TableBorderLineStyle.None;
            table[0, 1].Borders.Right.LineStyle = TableBorderLineStyle.None;

            if ( myBorder )
            {
                //table[0, 0].Borders.Bottom.LineStyle = TableBorderLineStyle.Single;
                //table[0, 0].Borders.Top.LineStyle = TableBorderLineStyle.Single;
                //table[0, 0].Borders.Left.LineStyle = TableBorderLineStyle.Single;
                //table[0, 0].Borders.Right.LineStyle = TableBorderLineStyle.Single;
                table[0, 1].Borders.Bottom.LineStyle = TableBorderLineStyle.Single;
                table[0, 1].Borders.Top.LineStyle = TableBorderLineStyle.Single;
                table[0, 1].Borders.Left.LineStyle = TableBorderLineStyle.Single;
                table[0, 1].Borders.Right.LineStyle = TableBorderLineStyle.Single;

                table[0, 1].Borders.Left.LineThickness = FunForms.pictureBorder;
                table[0, 1].Borders.Top.LineThickness = FunForms.pictureBorder;
                table[0, 1].Borders.Right.LineThickness = FunForms.pictureBorder;
                table[0, 1].Borders.Bottom.LineThickness = FunForms.pictureBorder;

                table[0, 1].LeftPadding = 5; // This was VERY Important!!!!
                table[0, 1].LeftPadding = 0; // This was VERY Important!!!!
            }
            //rtb.Document.Images.Insert(table[0, 1].Range.Start, DocumentImageSource.FromImage(myImage));
            //rtb.Document.InsertPicture(table[0, 0].Range.Start, DocumentImageSource.FromImage(myImage));

            //table[0, 0].Borders.Bottom.LineStyle = TableBorderLineStyle.ThickThinMediumGap;
            //table[0, 0].Borders.Top.LineStyle = TableBorderLineStyle.ThickThinMediumGap;
            //table[0, 0].Borders.Left.LineStyle = TableBorderLineStyle.ThickThinMediumGap;
            //table[0, 0].Borders.Right.LineStyle = TableBorderLineStyle.ThickThinMediumGap;
            //table[0, 1].Borders.Bottom.LineStyle = TableBorderLineStyle.Thick;
            //table[0, 1].Borders.Top.LineStyle = TableBorderLineStyle.Thick;
            //table[0, 1].Borders.Left.LineStyle = TableBorderLineStyle.Thick;
            //table[0, 1].Borders.Right.LineStyle = TableBorderLineStyle.Thick;

            //table[0, 1].Borders.Left.LineThickness = 5;

            string text = rtb.Document.RtfText;
            if (bytes != null)
                rtb.Document.RtfText = text.Replace(searchText, "");
        }
        /***********************************************************************************************/
        private bool GetPictureDetailsab(RichEditControl rtb, ref string searchText, ref float width, ref float leftPadding, ref string position)
        {
            width = 3.5F;
            leftPadding = 0F;
            bool result = false;
            searchText = "[*DECPIC";
            int idx = rtb.Document.RtfText.IndexOf(searchText);
            if (idx < 0)
                return result;

            string str = rtb.Document.RtfText;
            int ddx = str.IndexOf("[*DECPIC");
            if (ddx < 0)
                return result;
            try
            {
                string str2 = str.Substring(ddx + 8);
                ddx = str2.IndexOf("*]");
                string str3 = str2.Substring(0, ddx);
                searchText += str3 + "*]";
                str3 = str3.TrimStart('/');
                string[] Lines = str3.Split('/');
                if (Lines.Length > 0)
                {
                    if (Lines.Length > 1)
                    {
                        width = Lines[1].ObjToFloat();
                        if (Lines.Length > 2)
                        {
                            leftPadding = Lines[2].ObjToFloat();
                            if (Lines.Length > 3)
                                position = Lines[3];
                        }
                    }
                }
                result = true;
            }
            catch (Exception ex)
            {
            }
            return result;
        }
        /***********************************************************************************************/
        private bool GetPictureDetails ( RichEditControl rtb, ref string searchText, ref float width, ref float leftPadding, ref string position )
        {
            width = 3.5F;
            leftPadding = 0F;
            bool result = false;
            searchText = "[*DECPIC";
            int idx = rtb.Document.RtfText.IndexOf(searchText);
            if (idx < 0)
                return result;

            string str = rtb.Text.Trim();
            int ddx = str.IndexOf("[*DECPIC");
            if (ddx < 0)
                return result;
            try
            {
                string str2 = str.Substring(ddx + 8);
                ddx = str2.IndexOf("*]");
                string str3 = str2.Substring(0, ddx);
                searchText += str3 + "*]";
                str3 = str3.TrimStart('/');
                string[] Lines = str3.Split('/');
                if ( Lines.Length > 0 )
                {
                    if (Lines.Length > 1)
                    {
                        width = Lines[1].ObjToFloat();
                        if (Lines.Length > 2)
                        {
                            leftPadding = Lines[2].ObjToFloat();
                            if (Lines.Length > 3)
                                position = Lines[3];
                        }
                    }
                }
                result = true;
            }
            catch ( Exception ex)
            {
            }
            return result;
        }
        /***********************************************************************************************/
        private void LoadDecPicxxx()
        {
            string searchText = "[*DECPIC*]";
            int idx = rtb.Document.RtfText.IndexOf(searchText);
            if (idx >= 0)
            {
                LoadDecPic();
                return;
            }

            idx = 0;
            bool result = false;
            float width = 350F;
            float leftPadding = 0;
            string position = "";
            searchText = "";
            for (; ; )
            {
                result = GetPictureDetails(rtb, ref searchText, ref width, ref leftPadding, ref position );
                if (!result)
                    break;
                string cmd = "Select * from `customers` where `contractNumber` = '" + workContractNumber + "';";
                DataTable dt = G1.get_db_data(cmd);
                if (dt.Rows.Count <= 0)
                {
                    cmd = "Select * from `fcustomers` where `contractNumber` = '" + workContractNumber + "';";
                    dt = G1.get_db_data(cmd);
                    if (dt.Rows.Count <= 0)
                        return;
                }
                Byte[] bytes = dt.Rows[0]["picture"].ObjToBytes();
                Image myImage = new Bitmap(1, 1);
                if (bytes != null)
                {
                    myImage = G1.byteArrayToImage(bytes);
                    int iwidth = (int)(width * 100f);
                    Image newImage = ResizeImage(myImage, new Size(iwidth, iwidth), true);
                    myImage = newImage;
                }
                else
                {
                    cmd = "Select * from `fcustomers` where `contractNumber` = '" + workContractNumber + "';";
                    dt = G1.get_db_data(cmd);
                    if (dt.Rows.Count <= 0)
                        return;
                    bytes = dt.Rows[0]["picture"].ObjToBytes();
                    myImage = new Bitmap(1, 1);
                    //width = 3.5f;
                    if (bytes != null)
                    {
                        myImage = G1.byteArrayToImage(bytes);
                        int iwidth = (int)(width * 80f);
                        Image newImage = ResizeImage(myImage, new Size(iwidth, iwidth), true);
                        myImage = newImage;
                    }
                }
                PictureBox pic = new PictureBox();
                pic.Image = myImage;
                pic.SizeMode = PictureBoxSizeMode.StretchImage;
                Image Img = pic.Image;
                byte[] inputImage = new byte[Img.Width * Img.Height];
                DocumentRange[] ranges = rtb.Document.FindAll(searchText, SearchOptions.None);
                DocumentPosition myStart = ranges[0].Start;
                //rtb.Document.InsertPicture(myStart, DocumentImageSource.FromImage(myImage));
                Table table = rtb.Document.Tables.Create(myStart, 1, 1, AutoFitBehaviorType.AutoFitToContents);
                table.Rows[0].FirstCell.PreferredWidthType = WidthType.Fixed;
                table.Rows[0].FirstCell.PreferredWidth = DevExpress.Office.Utils.Units.InchesToDocumentsF(2.0f);
                table.Rows[0].FirstCell.HeightType = HeightType.Exact;
                table[0, 0].HeightType = HeightType.Exact;
                table[0, 0].Height = DevExpress.Office.Utils.Units.InchesToDocumentsF(width);

                rtb.Document.Images.Insert(table[0, 0].Range.Start, DocumentImageSource.FromImage(myImage));
                table[0, 0].Borders.Bottom.LineStyle = TableBorderLineStyle.None;
                table[0, 0].Borders.Top.LineStyle = TableBorderLineStyle.None;
                table[0, 0].Borders.Left.LineStyle = TableBorderLineStyle.None;
                table[0, 0].Borders.Right.LineStyle = TableBorderLineStyle.None;
                table[0, 0].LeftPadding = leftPadding;
                string text = rtb.Document.RtfText;
                if (bytes != null)
                    rtb.Document.RtfText = text.Replace(searchText, "");
            }
        }
        /***********************************************************************************************/
        private void LoadDecPic(string options = "" )
        {
            string searchText = "[*DECPIC*]";
            int idx = rtb.Document.RtfText.IndexOf(searchText);
            if (idx >= 0)
            {
                LoadDecPic1( options );
                return;
            }

            if (1 == 1)
            {
                LoadDecPicTest(options); // Use This for testing
                return;
            }

            idx = 0;
            bool result = false;
            float width = 350F;
            float leftPadding = 0;
            string position = "";
            searchText = "";
            for (; ; )
            {
                result = GetPictureDetails(rtb, ref searchText, ref width, ref leftPadding, ref position );
                if (!result)
                    break;
                string cmd = "Select * from `customers` where `contractNumber` = '" + workContractNumber + "';";
                DataTable dt = G1.get_db_data(cmd);
                if (dt.Rows.Count <= 0)
                {
                    cmd = "Select * from `fcustomers` where `contractNumber` = '" + workContractNumber + "';";
                    dt = G1.get_db_data(cmd);
                    if (dt.Rows.Count <= 0)
                        return;
                }
                Byte[] bytes = dt.Rows[0]["picture"].ObjToBytes();
                Image myImage = new Bitmap(1, 1);
                if (bytes != null)
                {
                    myImage = G1.byteArrayToImage(bytes);
                    int iwidth = (int)(width * 100f);
                    Image newImage = ResizeImage(myImage, new Size(iwidth, iwidth), true);
                    myImage = newImage;
                }
                else
                {
                    cmd = "Select * from `fcustomers` where `contractNumber` = '" + workContractNumber + "';";
                    dt = G1.get_db_data(cmd);
                    if (dt.Rows.Count <= 0)
                        return;
                    bytes = dt.Rows[0]["picture"].ObjToBytes();
                    myImage = new Bitmap(1, 1);
                    //width = 3.5f;
                    if (bytes != null)
                    {
                        myImage = G1.byteArrayToImage(bytes);
                        int iwidth = (int)(width * 80f);
                        Image newImage = ResizeImage(myImage, new Size(iwidth, iwidth), true);
                        myImage = newImage;
                    }
                }

                PictureBox pic = new PictureBox();
                pic.Image = myImage;
                pic.SizeMode = PictureBoxSizeMode.StretchImage;
                Image Img = pic.Image;
                byte[] inputImage = new byte[Img.Width * Img.Height];
                DocumentRange[] ranges = rtb.Document.FindAll(searchText, SearchOptions.None);
                DocumentPosition myStart = ranges[0].Start;

                //rtb.Document.InsertPicture(myStart, DocumentImageSource.FromImage(myImage));
                Table table = rtb.Document.Tables.Create(myStart, 1, 2, AutoFitBehaviorType.AutoFitToContents);
                table.Rows[0].FirstCell.PreferredWidthType = WidthType.Fixed;
                table.Rows[0].FirstCell.PreferredWidth = DevExpress.Office.Utils.Units.InchesToDocumentsF(1.0f);
                table.Rows[0].FirstCell.HeightType = HeightType.Exact;
                table[0, 0].PreferredWidthType = WidthType.Fixed;
                //table[0, 0].PreferredWidth = DevExpress.Office.Utils.Units.InchesToDocumentsF(0.3F);
                table[0, 0].PreferredWidth = DevExpress.Office.Utils.Units.InchesToDocumentsF(leftPadding);
                table[0, 0].HeightType = HeightType.Exact;
                table[0, 0].Height = DevExpress.Office.Utils.Units.InchesToDocumentsF(width);

                //Set the second column width and cell height
                table[0, 1].PreferredWidthType = WidthType.Fixed;
                table[0, 1].PreferredWidth = DevExpress.Office.Utils.Units.InchesToDocumentsF(width);
                table[0, 1].HeightType = HeightType.Auto;
                //table[0, 1].Height = DevExpress.Office.Utils.Units.InchesToDocumentsF(width - 0.5F);
                table[0, 1].Height = DevExpress.Office.Utils.Units.InchesToDocumentsF(width);
                table[0, 1].VerticalAlignment = TableCellVerticalAlignment.Center;

                rtb.Document.Images.Insert(table[0, 1].Range.Start, DocumentImageSource.FromImage(myImage));

                table[0, 0].Borders.Bottom.LineStyle = TableBorderLineStyle.None;
                table[0, 0].Borders.Top.LineStyle = TableBorderLineStyle.None;
                table[0, 0].Borders.Left.LineStyle = TableBorderLineStyle.None;
                table[0, 0].Borders.Right.LineStyle = TableBorderLineStyle.None;
                table[0, 1].Borders.Bottom.LineStyle = TableBorderLineStyle.None;
                table[0, 1].Borders.Top.LineStyle = TableBorderLineStyle.None;
                table[0, 1].Borders.Left.LineStyle = TableBorderLineStyle.None;
                table[0, 1].Borders.Right.LineStyle = TableBorderLineStyle.None;

                table[0, 1].Borders.Left.LineThickness = 3;
                table[0, 1].Borders.Top.LineThickness = 3;
                table[0, 1].Borders.Right.LineThickness = 3;
                table[0, 1].Borders.Bottom.LineThickness = 3;

                string text = rtb.Document.RtfText;
                //if (bytes != null)
                rtb.Document.RtfText = text.Replace(searchText, "");
            }
        }
        /***********************************************************************************************/
        private void LoadDecPic5(string options = "")
        {
            string searchText = "[*DECPIC*]";
            int idx = rtb.Document.RtfText.IndexOf(searchText);
            if (idx >= 0)
            {
                LoadDecPic1(options);
                return;
            }

            idx = 0;
            bool result = false;
            float width = 350F;
            float leftPadding = 0;
            string position = "";
            searchText = "";
            for (; ; )
            {
                result = GetPictureDetails(rtb, ref searchText, ref width, ref leftPadding, ref position);
                if (!result)
                    break;
                string cmd = "Select * from `customers` where `contractNumber` = '" + workContractNumber + "';";
                DataTable dt = G1.get_db_data(cmd);
                if (dt.Rows.Count <= 0)
                {
                    cmd = "Select * from `fcustomers` where `contractNumber` = '" + workContractNumber + "';";
                    dt = G1.get_db_data(cmd);
                    if (dt.Rows.Count <= 0)
                        return;
                }
                Byte[] bytes = dt.Rows[0]["picture"].ObjToBytes();
                Image myImage = new Bitmap(1, 1);
                if (bytes != null)
                {
                    myImage = G1.byteArrayToImage(bytes);
                    int iwidth = (int)(width * 100f);
                    Image newImage = ResizeImage(myImage, new Size(iwidth, iwidth), true);
                    myImage = newImage;
                }
                else
                {
                    cmd = "Select * from `fcustomers` where `contractNumber` = '" + workContractNumber + "';";
                    dt = G1.get_db_data(cmd);
                    if (dt.Rows.Count <= 0)
                        return;
                    bytes = dt.Rows[0]["picture"].ObjToBytes();
                    myImage = new Bitmap(1, 1);
                    //width = 3.5f;
                    if (bytes != null)
                    {
                        myImage = G1.byteArrayToImage(bytes);
                        int iwidth = (int)(width * 80f);
                        Image newImage = ResizeImage(myImage, new Size(iwidth, iwidth), true);
                        myImage = newImage;
                    }

                    int ddx = rtb.Document.RtfText.IndexOf(searchText);
                    ddx = rtb.Text.IndexOf(searchText);
                    if ( ddx >= 0 )
                    {
                        DocumentPosition pos = rtb.Document.CreatePosition(ddx);
                        rtb.Text = rtb.Text.Replace(searchText, "");
                        //rtb.Document.InsertImage(pos, myImage);
                    }
                    else
                        rtb.Document.RtfText = rtb.Document.RtfText.Replace(searchText, "");
                }

                //string xyz = rtb.Document.RtfText;
                //string abc = rtb.RtfText;

                //PictureBox pic = new PictureBox();
                //pic.Image = myImage;
                //pic.SizeMode = PictureBoxSizeMode.StretchImage;
                //Image Img = pic.Image;
                //byte[] inputImage = new byte[Img.Width * Img.Height];
                //DocumentRange[] ranges = rtb.Document.FindAll(searchText, SearchOptions.None);
                //DocumentPosition myStart = ranges[0].Start;

                ////rtb.Document.InsertPicture(myStart, DocumentImageSource.FromImage(myImage));
                //Table table = rtb.Document.Tables.Create(myStart, 1, 2, AutoFitBehaviorType.AutoFitToContents);
                //table.Rows[0].FirstCell.PreferredWidthType = WidthType.Fixed;
                //table.Rows[0].FirstCell.PreferredWidth = DevExpress.Office.Utils.Units.InchesToDocumentsF(1.0f);
                //table.Rows[0].FirstCell.HeightType = HeightType.Exact;
                //table[0, 0].PreferredWidthType = WidthType.Fixed;
                ////table[0, 0].PreferredWidth = DevExpress.Office.Utils.Units.InchesToDocumentsF(0.3F);
                //table[0, 0].PreferredWidth = DevExpress.Office.Utils.Units.InchesToDocumentsF(leftPadding);
                //table[0, 0].HeightType = HeightType.Exact;
                //table[0, 0].Height = DevExpress.Office.Utils.Units.InchesToDocumentsF(width);

                ////Set the second column width and cell height
                //table[0, 1].PreferredWidthType = WidthType.Fixed;
                //table[0, 1].PreferredWidth = DevExpress.Office.Utils.Units.InchesToDocumentsF(width);
                //table[0, 1].HeightType = HeightType.Auto;
                ////table[0, 1].Height = DevExpress.Office.Utils.Units.InchesToDocumentsF(width - 0.5F);
                //table[0, 1].Height = DevExpress.Office.Utils.Units.InchesToDocumentsF(width);
                //table[0, 1].VerticalAlignment = TableCellVerticalAlignment.Center;

                //rtb.Document.Images.Insert(table[0, 1].Range.Start, DocumentImageSource.FromImage(myImage));
                ////rtb.Document.InsertPicture(table[0, 0].Range.Start, DocumentImageSource.FromImage(myImage));
                //table[0, 0].Borders.Bottom.LineStyle = TableBorderLineStyle.None;
                //table[0, 0].Borders.Top.LineStyle = TableBorderLineStyle.None;
                //table[0, 0].Borders.Left.LineStyle = TableBorderLineStyle.None;
                //table[0, 0].Borders.Right.LineStyle = TableBorderLineStyle.None;
                //table[0, 1].Borders.Bottom.LineStyle = TableBorderLineStyle.None;
                //table[0, 1].Borders.Top.LineStyle = TableBorderLineStyle.None;
                //table[0, 1].Borders.Left.LineStyle = TableBorderLineStyle.None;
                //table[0, 1].Borders.Right.LineStyle = TableBorderLineStyle.None;
                //string text = rtb.Document.RtfText;
                ////if (bytes != null)
                //rtb.Document.RtfText = text.Replace(searchText, "");
            }
        }
        /***********************************************************************************************/
        private void LoadDecPicTest(string options = "")
        {
            string searchText = "[*DECPIC*]";
            int idx = rtb.Document.RtfText.IndexOf(searchText);
            if (idx >= 0)
            {
                LoadDecPic1(options);
                return;
            }

            idx = 0;
            bool result = false;
            float width = 350F;
            float leftPadding = 0;
            string position = "";
            searchText = "";
            string killText = "";
            for (; ; )
            {
                result = GetPictureDetails(rtb, ref searchText, ref width, ref leftPadding, ref position);
                if (!result)
                    break;
                killText += searchText + "~";

                //idx = rtb.Document.RtfText.IndexOf(searchText);
                idx = rtb.Document.Text.IndexOf(searchText);
                string cmd = "Select * from `customers` where `contractNumber` = '" + workContractNumber + "';";
                DataTable dt = G1.get_db_data(cmd);
                if (dt.Rows.Count <= 0)
                {
                    cmd = "Select * from `fcustomers` where `contractNumber` = '" + workContractNumber + "';";
                    dt = G1.get_db_data(cmd);
                    if (dt.Rows.Count <= 0)
                        return;
                }
                Byte[] bytes = dt.Rows[0]["picture"].ObjToBytes();
                Image myImage = new Bitmap(1, 1);
                if (bytes != null)
                {
                    myImage = G1.byteArrayToImage(bytes);
                    int iwidth = (int)(width * 100f);
                    Image newImage = ResizeImage(myImage, new Size(iwidth, iwidth), true);
                    myImage = newImage;
                }
                else
                {
                    cmd = "Select * from `fcustomers` where `contractNumber` = '" + workContractNumber + "';";
                    dt = G1.get_db_data(cmd);
                    if (dt.Rows.Count <= 0)
                        return;
                    bytes = dt.Rows[0]["picture"].ObjToBytes();
                    myImage = new Bitmap(1, 1);
                    //width = 3.5f;
                    if (bytes != null)
                    {
                        myImage = G1.byteArrayToImage(bytes);
                        int iwidth = (int)(width * 80f);
                        Image newImage = ResizeImage(myImage, new Size(iwidth, iwidth), true);
                        myImage = newImage;
                    }
                }

                PictureBox pic = new PictureBox();
                pic.Image = myImage;
                pic.SizeMode = PictureBoxSizeMode.StretchImage;
                Image Img = pic.Image;
                byte[] inputImage = new byte[Img.Width * Img.Height];

                searchText = "[*DECPIC";

                DocumentRange[] ranges = rtb.Document.FindAll(searchText, SearchOptions.None);

                for ( int j=0; j<ranges.Length; j++)
                {
                    DocumentPosition myStart = ranges[j].Start;
                    Shape myPicture = rtb.Document.Shapes.InsertPicture(myStart, DocumentImageSource.FromImage(Img));
                    //myPicture.RelativeHorizontalPosition = ShapeRelativeHorizontalPosition.LeftMargin;
                    //myPicture.RelativeVerticalPosition = ShapeRelativeVerticalPosition.Paragraph;
                    myPicture.RelativeHorizontalPosition = ShapeRelativeHorizontalPosition.Column;
                    myPicture.RelativeVerticalPosition = ShapeRelativeVerticalPosition.Paragraph;
                    myPicture.TextWrapping = TextWrappingType.Tight;

                }

                string str = rtb.Document.Text;

                //idx = rtb.Document.Text.IndexOf(searchText);
                DocumentPosition pos1 = rtb.Document.CreatePosition(28);

                string xyz = rtb.Document.RtfText;

                    //Shape myPicture = rtb.Document.Shapes.InsertPicture( myStart, DocumentImageSource.FromImage(Img));
                    //myPicture.RelativeHorizontalPosition = ShapeRelativeHorizontalPosition.LeftMargin;
                    //myPicture.RelativeVerticalPosition = ShapeRelativeVerticalPosition.Paragraph;
                    //myPicture.TextWrapping = TextWrappingType.Tight;

                string t = rtb.Document.RtfText;
                if (bytes != null)
                {
                    if (!String.IsNullOrWhiteSpace(killText))
                    {
                        killText = killText.TrimEnd('~');
                        //killText = killText.Replace(searchText, "");
                        rtb.Document.RtfText = t.Replace(killText, "");
                    }
                }
                if (1 == 1)
                    break;

                //Table table = rtb.Document.Tables.Create(myStart, 1, 2, AutoFitBehaviorType.AutoFitToContents);
                //table.Rows[0].FirstCell.PreferredWidthType = WidthType.Fixed;
                //table.Rows[0].FirstCell.PreferredWidth = DevExpress.Office.Utils.Units.InchesToDocumentsF(1.0f);
                //table.Rows[0].FirstCell.HeightType = HeightType.Exact;
                //table[0, 0].PreferredWidthType = WidthType.Fixed;
                ////table[0, 0].PreferredWidth = DevExpress.Office.Utils.Units.InchesToDocumentsF(0.3F);
                //table[0, 0].PreferredWidth = DevExpress.Office.Utils.Units.InchesToDocumentsF(leftPadding);
                //table[0, 0].HeightType = HeightType.Exact;
                //table[0, 0].Height = DevExpress.Office.Utils.Units.InchesToDocumentsF(width);

                ////Set the second column width and cell height
                //table[0, 1].PreferredWidthType = WidthType.Fixed;
                //table[0, 1].PreferredWidth = DevExpress.Office.Utils.Units.InchesToDocumentsF(width);
                //table[0, 1].HeightType = HeightType.Auto;
                ////table[0, 1].Height = DevExpress.Office.Utils.Units.InchesToDocumentsF(width - 0.5F);
                //table[0, 1].Height = DevExpress.Office.Utils.Units.InchesToDocumentsF(width);
                //table[0, 1].VerticalAlignment = TableCellVerticalAlignment.Center;

                //rtb.Document.Images.Insert(table[0, 1].Range.Start, DocumentImageSource.FromImage(myImage));
                ////rtb.Document.InsertPicture(table[0, 0].Range.Start, DocumentImageSource.FromImage(myImage));
                //table[0, 0].Borders.Bottom.LineStyle = TableBorderLineStyle.None;
                //table[0, 0].Borders.Top.LineStyle = TableBorderLineStyle.None;
                //table[0, 0].Borders.Left.LineStyle = TableBorderLineStyle.None;
                //table[0, 0].Borders.Right.LineStyle = TableBorderLineStyle.None;
                //table[0, 1].Borders.Bottom.LineStyle = TableBorderLineStyle.None;
                //table[0, 1].Borders.Top.LineStyle = TableBorderLineStyle.None;
                //table[0, 1].Borders.Left.LineStyle = TableBorderLineStyle.None;
                //table[0, 1].Borders.Right.LineStyle = TableBorderLineStyle.None;
                //string text = rtb.Document.RtfText;
                ////if (bytes != null)
                //rtb.Document.RtfText = text.Replace(searchText, "");
            }
        }
        /***********************************************************************************************/
        public static Image ResizeImage(Image image, Size size, bool preserveAspectRatio = true)
        {
            int newWidth;
            int newHeight;
            if (preserveAspectRatio)
            {
                int originalWidth = image.Width;
                int originalHeight = image.Height;
                float percentWidth = (float)size.Width / (float)originalWidth;
                float percentHeight = (float)size.Height / (float)originalHeight;
                float percent = percentHeight < percentWidth ? percentHeight : percentWidth;
                newWidth = (int)(originalWidth * percent);
                newHeight = (int)(originalHeight * percent);
            }
            else
            {
                newWidth = size.Width;
                newHeight = size.Height;
            }
            Image newImage = new Bitmap(newWidth, newHeight);
            using (Graphics graphicsHandle = Graphics.FromImage(newImage))
            {
                graphicsHandle.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphicsHandle.DrawImage(image, 0, 0, newWidth, newHeight);
            }
            return newImage;
        }
        /***********************************************************************************************/
        private void LoadRtfTable(DataTable[] dts, string searchText)
        {
            rtb.Document.BeginUpdate();

            int count = 0;
            for (int i = 0; i < 6; i++)
                count += dts[i].Rows.Count;

            DocumentRange[] ranges = rtb.Document.FindAll(searchText, SearchOptions.None);
            DocumentPosition myStart = ranges[0].Start;
            //Paragraph paragraph = rtb.Document.Paragraphs.Get(myStart);
            //paragraph.RightIndent = paragraph.LeftIndent;

            if (count < 45)
                count = 45;

            Table table = rtb.Document.Tables.Create(myStart, count, 4, AutoFitBehaviorType.AutoFitToContents);

            table.Rows[0].FirstCell.PreferredWidthType = WidthType.Fixed;
            table.Rows[0].FirstCell.PreferredWidth = DevExpress.Office.Utils.Units.InchesToDocumentsF(3.0f);
            table.Rows[0].FirstCell.HeightType = HeightType.Auto;
            table[0, 0].HeightType = HeightType.Auto;
            table[0, 0].Height = DevExpress.Office.Utils.Units.InchesToDocumentsF(0.5f);

            //Set the second column width and cell height
            table[0, 1].PreferredWidthType = WidthType.Fixed;
            table[0, 1].PreferredWidth = DevExpress.Office.Utils.Units.InchesToDocumentsF(0.8f);
            table[0, 1].HeightType = HeightType.AtLeast;
            table[0, 1].Height = DevExpress.Office.Utils.Units.InchesToDocumentsF(0.5f);

            table[0, 2].PreferredWidthType = WidthType.Fixed;
            table[0, 2].PreferredWidth = DevExpress.Office.Utils.Units.InchesToDocumentsF(2.5f);
            table[0, 2].HeightType = HeightType.Auto;
            table[0, 2].HeightType = HeightType.Auto;
            table[0, 2].Height = DevExpress.Office.Utils.Units.InchesToDocumentsF(0.5f);

            //Set the second column width and cell height
            table[0, 3].PreferredWidthType = WidthType.Fixed;
            table[0, 3].PreferredWidth = DevExpress.Office.Utils.Units.InchesToDocumentsF(0.8f);
            table[0, 3].HeightType = HeightType.AtLeast;
            table[0, 3].Height = DevExpress.Office.Utils.Units.InchesToDocumentsF(0.5f);

            float height = 0.5f;
            string service = "";
            string data = "";
            double dValue = 0D;
            int lastRow = 0;
            string font = "";
            float size = 0f;
            string font2 = "";
            float size2 = 0f;
            DataTable dt = null;
            int rows = 0;
            int row = 0;
            int col0 = 0;
            int col1 = 1;
            for (int j = 0; j < 8; j++)
            {
                dt = dts[j];
                if (j == 6)
                {
                    for (int k = rows; k < 45; k++)
                    {
                        table[k, 0].Borders.Bottom.LineStyle = TableBorderLineStyle.None;
                        table[k, 0].Borders.Top.LineStyle = TableBorderLineStyle.None;
                        table[k, 0].Borders.Left.LineStyle = TableBorderLineStyle.None;
                        table[k, 0].Borders.Right.LineStyle = TableBorderLineStyle.None;
                        table[k, 1].Borders.Bottom.LineStyle = TableBorderLineStyle.None;
                        table[k, 1].Borders.Top.LineStyle = TableBorderLineStyle.None;
                        table[k, 1].Borders.Left.LineStyle = TableBorderLineStyle.None;
                        table[k, 1].Borders.Right.LineStyle = TableBorderLineStyle.None;
                    }
                    col0 = 2;
                    col1 = 3;
                    rows = 0;
                }
                lastRow = dt.Rows.Count;
                for (int i = 0; i < lastRow; i++)
                {
                    font = dt.Rows[i]["font"].ObjToString();
                    size = dt.Rows[i]["size"].ObjToFloat();
                    font2 = dt.Rows[i]["font2"].ObjToString();
                    size2 = dt.Rows[i]["size2"].ObjToFloat();
                    service = dt.Rows[i]["service"].ObjToString();
                    data = dt.Rows[i]["data"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(data))
                    {
                        dValue = data.ObjToDouble();
                        data = "$" + G1.ReformatMoney(dValue);
                        if (j < 6)
                        {
                            if (service.ToUpper().IndexOf("TOTAL") < 0)
                            {
                                if (dValue == 0D)
                                    data = "";
                            }
                        }
                        data = data.PadLeft(12);
                    }
                    row = i + rows;
                    rtb.Document.InsertText(table[row, col0].Range.Start, service);
                    rtb.Document.InsertText(table[row, col1].Range.Start, data);
                    table.Rows[row].FirstCell.HeightType = HeightType.Auto;
                    ChangeCellFont(table[row, col0], font, size);
                    if (!String.IsNullOrWhiteSpace(font2) && size2 > 0f)
                        ChangeCellFont(table[row, col1], font2, size2);

                    table[row, col0].HeightType = HeightType.Auto;
                    table[row, col0].Height = DevExpress.Office.Utils.Units.InchesToDocumentsF(height);
                    table[row, col1].HeightType = HeightType.Auto;
                    table[row, col1].Height = DevExpress.Office.Utils.Units.InchesToDocumentsF(height);
                    table[row, col0].WordWrap = true;
                    table[row, col1].WordWrap = true;
                    //                table[i, 1].BackgroundColor = System.Drawing.Color.Red;
                    table[row, col0].HeightType = HeightType.Auto;
                    table[row, col1].HeightType = HeightType.Auto;
                    table[row, col0].Borders.Top.LineStyle = TableBorderLineStyle.None;
                    table[row, col0].Borders.Bottom.LineStyle = TableBorderLineStyle.None;
                    table[row, col0].Borders.Left.LineStyle = TableBorderLineStyle.None;
                    table[row, col0].Borders.Right.LineStyle = TableBorderLineStyle.None;
                    table[row, col1].Borders.Top.LineStyle = TableBorderLineStyle.None;
                    if (String.IsNullOrWhiteSpace(data))
                        table[row, col1].Borders.Bottom.LineStyle = TableBorderLineStyle.None;
                    table[row, col1].Borders.Left.LineStyle = TableBorderLineStyle.None;
                    table[row, col1].Borders.Right.LineStyle = TableBorderLineStyle.None;
                    table[row, col1].VerticalAlignment = TableCellVerticalAlignment.Bottom;
                }
                rows += lastRow;
            }
            ProduceDisclaimer(table, rows);
            string text = rtb.Document.RtfText;
            rtb.Document.RtfText = text.Replace(searchText, "");
            rtb.Document.EndUpdate();
        }
        /***********************************************************************************************/
        private void ProduceDisclaimer(Table table, int rows)
        {
            int lastRow = table.Rows.Count - 1;
            for (int i = rows; i <= lastRow; i++)
            {
                table[i, 2].Borders.Bottom.LineStyle = TableBorderLineStyle.None;
                table[i, 2].Borders.Top.LineStyle = TableBorderLineStyle.None;
                table[i, 2].Borders.Left.LineStyle = TableBorderLineStyle.None;
                table[i, 2].Borders.Right.LineStyle = TableBorderLineStyle.None;
                table[i, 3].Borders.Bottom.LineStyle = TableBorderLineStyle.None;
                table[i, 3].Borders.Top.LineStyle = TableBorderLineStyle.None;
                table[i, 3].Borders.Left.LineStyle = TableBorderLineStyle.None;
                table[i, 3].Borders.Right.LineStyle = TableBorderLineStyle.None;
            }
            table.MergeCells(table[rows, 2], table[lastRow, 3]);

            StringBuilder sb = new StringBuilder(" ");
            sb[0] = (char)127; // Unprintable Char, Causes Underline to be visible up to this char

            string underline = sb.ToString();

            RichTextBox rtf = new RichTextBox();

            G1.Toggle_Font(rtf, "Arial", 6f);
            rtf.AppendText("If any law, cemetery or crematory requirement have required the purchase of any items listed, the law or requirement is explained below.\n\n");

            G1.Toggle_Font(rtf, "Arial Black", 7f);
            rtf.AppendText("DISCLOSURES\n\n");

            G1.Toggle_Font(rtf, "Arial", 7f);
            rtf.AppendText("Reason for Embalming:");
            G1.Toggle_Bold(rtf, false, false, true);
            rtf.AppendText("[%RFE%]                                                  " + underline + "\n\n");

            G1.Toggle_Font(rtf, "Arial", 7f);
            rtf.AppendText("Cemetery Requirements:");
            G1.Toggle_Bold(rtf, false, false, true);
            rtf.AppendText("[%CER%]                                                " + underline + "\n\n");

            G1.Toggle_Font(rtf, "Arial", 7f);
            rtf.AppendText("Crematory Requirements:");
            G1.Toggle_Bold(rtf, false, false, true);
            rtf.AppendText("[%CR%]                                                 " + underline + "\n\n");

            G1.Toggle_Font(rtf, "Arial", 6f);
            rtf.AppendText("_________________________________________________________________________\n");



            G1.Toggle_Font(rtf, "Arial Black", 7f);
            rtf.AppendText("DISCLAIMER OF WARRANTIES\n");
            G1.Toggle_Font(rtf, "Arial", 6f);

            rtf.AppendText("THE ABOVE FUNERAL HOME MAKES NO WARRANTIES OR REPRESENTATIONS CONCERNING THE PRODUCTS SOLD HEREIN. THE ONLY WARRANTIES, EXPRESSED OR IMPLIED, GRANTED IN CONNECTION WITH THE PRODUCTS SOLD WITH THE FUNERAL SERVICE, ARE THE EXPRESSED WRITTEN WARRANTIES, IF ANY EXTENDED BY THE MANUFACTURERS THEREOF. THE ABOVE-NAMED FUNERAL HOME HEREBY, EXPRESSLY DISCLAIMS ALL WARRANTIES, EXPRESS OR IMPLIED, RELATING TO ALL SUCH PRODUCTS, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OR MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE.\n");
            G1.Toggle_Font(rtf, "Arial Black", 7f);
            rtf.AppendText("ACKNOWLEDGEMENT AND AGREEMENT\n");
            G1.Toggle_Font(rtf, "Arial", 6f);
            rtf.AppendText("I hereby acknowledge that I have the legal right to arrange the final services for the deceased, and I authorized this funeral establishment to perform services, furnish goods, and incur outside charges specified on this Statement.I acknowledge that I have received the General Price List and the Casket Price List.I agree to pay the balance due with acceptance of this statement and I have no right to defer payment of any amount due under this Agreement.In the event I default in payment to this funeral establishment, I agree to pay reasonable attorney’s fees, court costs and/or collection agency fees. In the event a balance remains unpaid after thirty(30) days, an interest charge of 1.5% per month will be added to the account.I understand and agree that I am assuming personal liability for the charges set forth in this Statement in addition to the liability imposed by law upon the estate of the deceased.By signing below, I hereby agree to all of the above and acknowledge receipt of a copy of this Statement.");

            rtb.Document.InsertRtfText(table[rows, 2].Range.Start, rtf.Rtf);
        }
        /***********************************************************************************************/
        private void ChangeCellFont(TableCell cell, string font, float size)
        {
            CharacterProperties cpCell = rtb.Document.BeginUpdateCharacters(cell.Range);
            //            cpCell.ForeColor = System.Drawing.Color.Green;
            //            cpCell.Bold = true;
            cpCell.FontName = font;
            cpCell.FontSize = size;
            rtb.Document.EndUpdateCharacters(cpCell);
        }
        /***********************************************************************************************/
        private void LoadAgreementData()
        {
            if (!workContract)
                return;

            DateTime start = DateTime.Now;

            //            DataTable dx = Structures.ExtractFields( rtb.Document.RtfText );
            DataTable dx = RTF_Stuff.ExtractFields(rtb.Document.RtfText);
            DateTime extractTime = DateTime.Now;

            if (dx.Rows.Count > 0)
            {
                DataRow[] dR = dx.Select("lookup='MEMCONTENT'");
                if (dR.Length > 0)
                {
                    lblTitles.Show();
                    cmbTitles.Show();
                    loadMemContent();
                }
            }
            DataTable dt = LoadFields(dx, rtb, workFormName, workLocation, removeEmpty);
            //DataTable dt = Structures.LoadFields(dx, workLocation, workFormName);
            //dt = LoadDbFields(dt);
            //PushFieldsToForm(dt, rtb);

            DateTime loadTime = DateTime.Now;

            LoadServicesTable("[%PS1%]");
            DateTime serviesTime = DateTime.Now;

            TimeSpan tsExtract = extractTime - start;
            TimeSpan tsLoad = loadTime - extractTime;
            TimeSpan tsService = serviesTime - loadTime;

            string moreOptions = "";

            DataRow[] dRows = dt.Select("field='DECPIC'");
            if (dRows.Length > 0)
            {
                moreOptions = dRows[0]["more_options"].ObjToString();
            }

            LoadDecPic( moreOptions );

            string text = rtb.RtfText;
            string f1 = "";
            string f2 = "";
            bool found = false;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                f2 = dt.Rows[i]["F2"].ObjToString();
                if (String.IsNullOrWhiteSpace(f2))
                {
                    f1 = dt.Rows[i]["F1"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(f1))
                    {
                        text = ReplaceField(text, f1, "");
                        found = true;
                    }
                }
            }
            if (found)
                rtb.RtfText = text;
        }
        /***********************************************************************************************/
        private void loadMemContent()
        {
            cmbTitles.Items.Clear();

            string title = "";
            string cmd = "Select * from `titlecontent`;";
            DataTable dt = G1.get_db_data(cmd);
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                title = dt.Rows[i]["title"].ObjToString();
                cmbTitles.Items.Add(title);
            }
            lblTitles.Show();
            cmbTitles.Show();
            lblTitles.Refresh();
            cmbTitles.Refresh();
        }
        /***********************************************************************************************/
        private int FindLastParm(string text, int idx)
        {
            int end = -1;
            char c = 'x';
            for (int i = idx; i < text.Length; i++)
            {
                c = (char)text[i];
                end = i;
                if (c == 'x')
                    break;
            }
            return end;
        }
        /***********************************************************************************************/
        private DataTable LoadFields(DataTable dx, RichEditControl rtbx = null, string formName = "", string location = "", bool removeEmpty = false )
        {
            if (String.IsNullOrWhiteSpace(formName))
                formName = workFormName;
            string cmd = "Select * from `structures` where `form` = '" + formName + "' ";
            cmd += " AND `location` = '" + location + "' ";
            cmd += "order by `order`;";
            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("num");
            dt.Columns.Add("mod");
            dt.Columns.Add("F1");
            dt.Columns.Add("F2");
            dt.Columns.Add("search");
            DataRow[] dRows = null;
            string field = "";
            string str = "";
            bool gotTable = false;
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                field = dx.Rows[i]["field"].ObjToString();
                str = field;
                str = str.Replace("[*", "");
                str = str.Replace("*]", "");
                str = str.Replace("[%", "");
                str = str.Replace("%]", "");
                str = Structures.cleanupField(str);
                if (str.ToUpper() == "DECPIC")
                {
                }
                if ( str.ToUpper() == "MEMTITLE" || str.ToUpper() == "MEMCONTENT" )
                {
                    continue;
                }
                if (str.ToUpper().IndexOf("BEGIN_TABLE") >= 0)
                {
                    if ( str.ToUpper().IndexOf( "END_TABLE") < 0 )
                        gotTable = true;
                }
                else if (str.ToUpper().IndexOf("END_TABLE") >= 0)
                    gotTable = false;
                dRows = dt.Select("field='" + str + "'");
                if (dRows.Length <= 0 || gotTable)
                {
                    DataRow dR = dt.NewRow();
                    dR["F1"] = field;
                    dt.Rows.Add(dR);
                }
                else
                {
                    dRows[0]["F1"] = field;
                }
            }
            //dt = LoadDbFields(dt);
            if ( !workFromForm )
                dt.TableName = workFormName;
            dt = RTF_Stuff.LoadDbFields(workContractNumber, "", dt);
            PushFieldsToForm(workContractNumber, dt, rtbx, removeEmpty );
            return dt;
        }
        /***********************************************************************************************/
        public static void PushFieldsToForm(string contractNumber, DataTable dt, RichEditControl rtbx=null, bool removeEmpty =false )
        {
            //if (rtbx == null)
            //    rtbx = rtb;
            string text = rtbx.Document.RtfText;
            string junk = rtbx.Document.Text;
            string field = "";
            string data = "";
            string qualify = "";
            bool pass = false;
            int i = 0;
            try
            {
                for (i = 0; i < dt.Rows.Count; i++)
                {
                    field = dt.Rows[i]["F1"].ObjToString();
                    if ( field.ToUpper().IndexOf( "SLOGAN") >= 0 )
                    {

                    }
                    //                field = dt.Rows[i]["field"].ObjToString();
                    if (field.ToUpper().IndexOf("BEGIN_TABLE") >= 0)
                    {
                        qualify = dt.Rows[i]["qualify"].ObjToString();
                        //pass = true;
                        continue;
                    }
                    else if (field.ToUpper().IndexOf("END_TABLE") >= 0)
                    {
                        pass = false;
                        continue;
                    }
                    if (pass)
                        continue;
                    data = dt.Rows[i]["F2"].ObjToString().Trim();
                    if (String.IsNullOrWhiteSpace(field))
                        continue;
                    if (String.IsNullOrWhiteSpace(data))
                    {
                        if (removeEmpty)
                            text = ReplaceField(text, field, data);
                        continue;
                    }
                    if (data.IndexOf("rtf1") >= 0)
                    {
                        text = ReplaceField(text, field, data);
                        //                    rtbx.Document.RtfText = text;
                    }
                    else
                        text = ReplaceField(text, field, data);
                }
            }
            catch ( Exception ex )
            {

            }
            rtbx.Document.RtfText = text;
            string result = RTF_Stuff.ProcessTables(contractNumber, dt, text );
            rtbx.Document.RtfText = result;
            junk = rtbx.Document.Text;
            rtbx.Document.RtfText = rtbx.Document.RtfText.Replace("the her", "her");
            rtbx.Document.RtfText = rtbx.Document.RtfText.Replace("the his", "his");
        }
        /***********************************************************************************************/
        public static string GetDecAge( string workContractNumber)
        {
            string rv = "";
            string cmd = "Select * from `customers` where `contractNumber` = '" + workContractNumber + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                DateTime dob = dt.Rows[0]["birthDate"].ObjToDateTime();
                DateTime dod = dt.Rows[0]["deceasedDate"].ObjToDateTime();
                if (dod.Year < 1875)
                    dod = DateTime.Now;
                int age = G1.GetAge(dob, dod);
                rv = age.ObjToString();
            }
            return rv;
        }
        /***********************************************************************************************/
        public static string GetSSN( string workContractNumber )
        {
            string rv = "";
            string cmd = "Select * from `customers` where `contractNumber` = '" + workContractNumber + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
                rv = dt.Rows[0]["ssn"].ObjToString();
            return rv;
        }
        /***********************************************************************************************/
        public static string GetDOD( string workContractNumber)
        {
            string rv = "";
            string cmd = "Select * from `customers` where `contractNumber` = '" + workContractNumber + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                DateTime dod = dt.Rows[0]["deceasedDate"].ObjToDateTime();
                if (dod.Year > 1800)
                    rv = dod.ToString("MM/dd/yyyy");
            }
            return rv;
        }
        /***********************************************************************************************/
        private int MatchCount = 0;
        private DataTable extractData( RichEditControl rtbx = null)
        {
            if (rtbx == null)
                rtbx = rtb;
            int idx = 0;
            string str = "";
            string part1 = "";
            string part2 = "";
            string part3 = "";
            int startPosition = -1;
            int stopPosition = -1;
            string whatTable = "";
            string match = "";
            MatchCount = 0;
            string text = rtbx.Document.RtfText;
            extractDt = Structures.ParseRTF(text, "[%");

            extractDt.Columns.Add("relationship");
            extractDt.Columns.Add("match");
            extractDt.Columns.Add("detail");
            int count = 1;
            bool workTable = false;
            for (int i = extractDt.Rows.Count - 1; i >= 0; i--)
            {
                workTable = false;
                str = extractDt.Rows[i]["field"].ObjToString();
                if (str.ToUpper().IndexOf("BEGIN_TABLE:~") >= 0)
                    workTable = true;
                if (str.ToUpper().IndexOf("BEGIN_TABLE:DECEASED~") >= 0)
                    workTable = true;
                if ( workTable )
                {
                    startPosition = extractDt.Rows[i]["position"].ObjToInt32();
                    match = "$match" + count.ToString() + "$";
                    extractDt.Rows[i]["match"] = match;
                    part1 = text.Substring(0, startPosition);
                    part1 += match;
                    part2 = text.Substring(stopPosition);
                    startPosition += str.Length;
                    //                        part2 = text.Substring(startPosition);

                    idx = text.IndexOf(str);
                    part3 = text.Substring(idx, stopPosition - idx);
//                    part3 = text.Substring(startPosition, stopPosition - startPosition);
                    extractDt.Rows[i]["detail"] = part3;
                    text = part1 + part2;
                    count++;
                }
                else if (str.ToUpper().IndexOf("BEGIN_TABLE") >= 0)
                {
                    whatTable = "";
                    startPosition = extractDt.Rows[i]["position"].ObjToInt32();
                    if ( str.ToUpper().IndexOf( "VISIT") >= 0 )
                    {
                        whatTable = "fcust_extended";
                        extractDt.Rows[i]["relationship"] = whatTable;
                        match = "$match" + count.ToString() + "$";
                        extractDt.Rows[i]["match"] = match;
                        part1 = text.Substring(0, startPosition);
                        part1 += match;
                        part2 = text.Substring(stopPosition);
                        startPosition += str.Length;
                        //                        part2 = text.Substring(startPosition);
                        part3 = text.Substring(startPosition, stopPosition - startPosition);
                        extractDt.Rows[i]["detail"] = part3;
                        text = part1 + part2;
                        whatTable = "";
                        count++;
                    }
                    else if (str.ToUpper().IndexOf("RELATIONSHIP") >= 0)
                    {
                        whatTable = parseRelationship(str);
                        extractDt.Rows[i]["relationship"] = whatTable;
                        match = "$match" + count.ToString() + "$";
                        extractDt.Rows[i]["match"] = match;
                        part1 = text.Substring(0, startPosition);
                        part1 += match;
                        part2 = text.Substring(stopPosition);
                        startPosition += str.Length;
                        //                        part2 = text.Substring(startPosition);
                        part3 = text.Substring(startPosition, stopPosition - startPosition);
                        extractDt.Rows[i]["detail"] = part3;
                        text = part1 + part2;
                        whatTable = "";
                        count++;
                    }
                }
                else if (str.ToUpper().IndexOf("END_TABLE") >= 0)
                {
                    stopPosition = extractDt.Rows[i]["position"].ObjToInt32();
                    stopPosition += str.Length;
                    startPosition = -1;
                }
            }
            rtbx.Document.RtfText = text;
            MatchCount = count;
            return extractDt;
        }
        /***********************************************************************************************/
        private void ProcessTables(DataTable ddx, RichEditControl rtbx = null )
        {
            string str = "";
            string part3 = "";
            string whatRelationship = "";
            string match = "";
            if (rtbx == null)
                rtbx = rtb;

            //if (1 == 1)
            //    return;

            extractDt = extractData(rtbx );

            string tt = rtbx.Document.Text;

            ddx.Columns.Add("detail");
            ddx.Columns.Add("match");
            ddx.Columns.Add("relationship");
            int i = 0;
            for (i = 0; i < extractDt.Rows.Count; i++)
            {
                str = extractDt.Rows[i]["field"].ObjToString();
                if (str.ToUpper().IndexOf("BEGIN_TABLE") >= 0)
                {
                    str = str.Replace("[%", "");
                    str = str.Replace("%]", "");
                    //str = str.Replace("[*", "");
                    //str = str.Replace("*]", "");
                    str = str.Replace("\'94", "");
                    str = str.Replace("\\u8221", "");
                    str = str.Replace("\\", "");
                    DataRow[] dRows = ddx.Select("field LIKE'%" + str + "%'");
                    if (dRows.Length > 0)
                    {
                        str = extractDt.Rows[i]["detail"].ObjToString();
                        dRows[0]["detail"] = extractDt.Rows[i]["detail"].ObjToString();
                        dRows[0]["match"] = extractDt.Rows[i]["match"].ObjToString();
                        dRows[0]["relationship"] = extractDt.Rows[i]["relationship"].ObjToString();
                    }
                }
            }
            string text = rtbx.Document.RtfText;
            tt = rtbx.Document.Text;
            part3 = "";
            string part4 = "";
            string allParts = "";
            string field = "";
            string dbField = "";
            string table = "";
            DataTable relationDt = null;
            string cmd = "";
            bool pass = false;
            string data = "";
            DateTime deceasedDate = DateTime.Now;
            bool workTable = false;

            G1.NumberDataTable(ddx);
            try
            {
                for (i = 0; i < ddx.Rows.Count; i++)
                {
                    if ( i == 19)
                    {

                    }
                    workTable = false;
                    field = ddx.Rows[i]["F1"].ObjToString();
                    if (field == "[*BIOGRAPHY*]")
                    {
                        ProcessBiography(rtbx);
                        continue;
                    }
                    table = ddx.Rows[i]["table"].ObjToString();
                    if (table.ToUpper() == "XXXX")
                        continue;
                    if (String.IsNullOrWhiteSpace(table))
                        continue;
                    field = ddx.Rows[i]["field"].ObjToString();
                    if (pass)
                    {
                        if (field.ToUpper().IndexOf("END_TABLE") >= 0)
                            continue;
                        if (field.ToUpper().IndexOf("BEGIN_TABLE") >= 0)
                            pass = false;
                        else
                            continue;
                    }
                    if (field.ToUpper().IndexOf("BEGIN_TABLE:~") >= 0)
                        workTable = true;
                    if (field.ToUpper().IndexOf("BEGIN_TABLE:DECEASED~") >= 0)
                        workTable = true;
                    if (workTable)
                    {
                        bool deceased = false;
                        bool addParens = false;
                        if (field.ToUpper().IndexOf("DECEASED") >= 0)
                            deceased = true;
                        part4 = ddx.Rows[i]["detail"].ObjToString();
                        match = ddx.Rows[i]["match"].ObjToString();
                        relationDt = ParseOutTableMembers(table, field);
                        allParts = "";
                        for (int k = 0; k < relationDt.Rows.Count; k++)
                        {
                            addParens = false;
                            part3 = part4;
                            field = ddx.Rows[i]["field"].ObjToString();
                            string[] Lines = field.Split('~');
                            deceasedDate = relationDt.Rows[k]["depDOD"].ObjToDateTime();
                            if (deceased)
                            {
                                if (deceasedDate.Year < 1500)
                                    continue;
                            }
                            else
                            {
                                if (deceasedDate.Year > 1500)
                                    continue;
                            }
                            str = Lines[2].Trim();
                            Lines = str.Split(' ');
                            for (int j = 0; j < Lines.Length; j++)
                            {
                                str = Lines[j].Trim();
                                if (str.ToUpper() == "FIRSTNAME")
                                    str = "depFirstName";
                                else if (str.ToUpper() == "LASTNAME")
                                    str = "depLastName";
                                else if (str.ToUpper() == "MI")
                                    str = "depMI";
                                else if (str.ToUpper() == "SUFFIX")
                                    str = "depSuffix";
                                else if (str.ToUpper() == "DOB")
                                    str = "depDOB";
                                else if (str.ToUpper() == "DOD")
                                    str = "depDOD";
                                else if (str.ToUpper() == "RELATIONSHIP")
                                    str = "depRelationShip";
                                else if (str.ToUpper() == "SPOUSE")
                                {
                                    str = "spouseFirstName";
                                    addParens = true;
                                }
                                dbField = str;
                                data = "";
                                try
                                {
                                    if (G1.get_column_number(relationDt, dbField.Trim()) >= 0)
                                    {
                                        data = relationDt.Rows[k][dbField.Trim()].ObjToString();
                                        if (!String.IsNullOrWhiteSpace(data) && addParens)
                                            data = "(" + data + ")";
                                        Lines[j] = data;
                                        addParens = false;
                                    }
                                }
                                catch
                                {
                                }
                            }
                            data = "";
                            for (int j = 0; j < Lines.Length; j++)
                            {
                                data += Lines[j].Trim() + " ";
                            }
                            data = data.TrimEnd(' ');
                            field = ddx.Rows[i]["field"].ObjToString();
                            field = field.Replace("~END_TABLE", "");
                            part3 = part3.Replace(field, data);

                            part3 = part3.Replace("[%", "");
                            part3 = part3.Replace("%]", "");
                            part3 = part3.Replace("[*", "");
                            part3 = part3.Replace("*]", "");
                            part3 = part3.Replace("END_TABLE", "");

                            allParts += part3;
                            part3 = part4;
                        }
                        text = rtbx.Document.RtfText;
                        if (!String.IsNullOrWhiteSpace(match))
                        {
                            text = ReplaceField(text, match, allParts);
                            rtbx.Document.RtfText = text;
                        }
                        allParts = "";
                        continue;
                    }
                    else if (field.ToUpper().IndexOf("BEGIN_TABLE") >= 0)
                    {
                        table = ddx.Rows[i]["table"].ObjToString();
                        if (table.ToUpper() == "XXXX")
                            continue;
                        part4 = ddx.Rows[i]["detail"].ObjToString();
                        match = ddx.Rows[i]["match"].ObjToString();
                        whatRelationship = ddx.Rows[i]["relationship"].ObjToString();
                        if (String.IsNullOrWhiteSpace(table) || String.IsNullOrWhiteSpace(whatRelationship))
                            continue;
                        //part4 = ddx.Rows[i]["search"].ObjToString();
                        allParts = "";
                        try
                        {
                            cmd = "Select * from `" + table + "` where `contractNumber` = '" + workContractNumber + "' ";
                            if (whatRelationship.ToUpper().IndexOf("ALL") < 0)
                                cmd += " and `depRelationship` IN " + whatRelationship + " ";
                            cmd += ";";
                            relationDt = G1.get_db_data(cmd);
                            for (int k = 0; k < relationDt.Rows.Count; k++)
                            {
                                part3 = part4;
                                field = ddx.Rows[i]["field"].ObjToString();
                                string[] Lines = field.Split('~');
                                bool deceased = false;
                                for (int j = 0; j < Lines.Length; j++)
                                {
                                    str = Lines[j].Trim();
                                    if (str.ToUpper().IndexOf("DECEASED=YES") >= 0)
                                        deceased = true;
                                }
                                deceasedDate = relationDt.Rows[k]["depDOD"].ObjToDateTime();
                                if (deceased)
                                {
                                    if (deceasedDate.Year < 1500)
                                        continue;
                                }
                                else
                                {
                                    if (deceasedDate.Year > 1500)
                                        continue;
                                }
                                for (int j = 0; j < Lines.Length; j++)
                                {
                                    str = Lines[j].Trim();
                                    if (str.ToUpper().IndexOf("BEGIN_TABLE") >= 0)
                                        continue;
                                    str = str.Replace("R:", "");
                                    if (str.ToUpper() == "FIRST_NAME")
                                        str = "depFirstName";
                                    else if (str.ToUpper() == "LAST_NAME")
                                        str = "depLastName";
                                    else if (str.ToUpper() == "MI")
                                        str = "depMI";
                                    else if (str.ToUpper() == "SUFFIX")
                                        str = "depSuffix";
                                    else if (str.ToUpper() == "DOB")
                                        str = "depDOB";
                                    else if (str.ToUpper() == "DOD")
                                        str = "depDOD";
                                    else if (str.ToUpper() == "RELATIONSHIP")
                                        str = "depRelationShip";
                                    else if (str.ToUpper() == "SPOUSE_FIRST_NAME")
                                        str = "spouseFirstName";
                                    dbField = str;
                                    data = "";
                                    try
                                    {
                                        data = relationDt.Rows[k][dbField].ObjToString();
                                        part3 = ReplaceField(part3, Lines[j], data);
                                    }
                                    catch
                                    {
                                    }
                                    str = Lines[j].Trim();
                                    part3 = part3.Replace(str, "");
                                }
                                part3 = part3.Replace("[%", "");
                                part3 = part3.Replace("%]", "");
                                part3 = part3.Replace("[*", "");
                                part3 = part3.Replace("*]", "");

                                allParts += part3;
                                part3 = part4;
                            }
                            text = rtbx.Document.RtfText;
                            text = ReplaceField(text, match, allParts);
                            rtbx.Document.RtfText = text;
                            allParts = "";
                        }
                        catch (Exception ex)
                        {
                        }
                        //pass = true;
                        part3 = "";
                        continue;
                    }
                    if (field.ToUpper().IndexOf("END_TABLE") >= 0)
                    {
                        //text = rtb.Document.RtfText;
                        //text = ReplaceField(text, match, part3);
                        //rtb.Document.RtfText = text;
                        part3 = "";
                        continue;
                    }
                }
            }
            catch ( Exception ex)
            {

            }
            allParts = "";
            text = rtbx.Document.RtfText;
            for (i = 1; i <= MatchCount; i++)
            {
                match = "$match" + i.ToString() + "$";
                text = ReplaceField(text, match, allParts);
            }
            rtbx.Document.RtfText = text;
            tt = rtbx.Document.Text;
        }
        //        /***********************************************************************************************/
        //        private void ProcessTablesx ( DataTable ddx )
        //        {
        //            string str = "";
        //            string part1 = "";
        //            string part2 = "";
        //            string part3 = "";
        //            int startPosition = -1;
        //            int stopPosition = -1;
        //            string whatTable = "";
        //            string match = "";
        //            MatchCount = 0;
        //            string text = rtb.Document.RtfText;
        //            extractDt = Structures.ParseRTF(text, "[%");

        //            extractDt.Columns.Add("relationship");
        //            extractDt.Columns.Add("match");
        //            extractDt.Columns.Add("detail");
        //            int count = 1;
        //            for ( int i=extractDt.Rows.Count-1; i>=0; i--)
        //            {
        //                str = extractDt.Rows[i]["field"].ObjToString();
        //                if ( str.ToUpper().IndexOf ( "BEGIN_TABLE") >= 0 )
        //                {
        //                    whatTable = "";
        //                    startPosition = extractDt.Rows[i]["position"].ObjToInt32();
        //                    if (str.ToUpper().IndexOf("DAUGHTER") >= 0)
        //                    {
        //                        whatTable = parseRelationship(str);
        //                        extractDt.Rows[i]["relationship"] = whatTable;
        //                        match = "$match" + count.ToString() + "$";
        //                        extractDt.Rows[i]["match"] = match;
        //                        part1 = text.Substring(0, startPosition);
        //                        part1 += match;
        //                        part2 = text.Substring(stopPosition);
        //                        startPosition += str.Length;
        ////                        part2 = text.Substring(startPosition);
        //                        part3 = text.Substring(startPosition, stopPosition - startPosition);
        //                        extractDt.Rows[i]["detail"] = part3;
        //                        text = part1 + part2;
        //                        whatTable = "";
        //                        count++;
        //                    }
        //                }
        //                else if (str.ToUpper().IndexOf("END_TABLE") >= 0)
        //                {
        //                    stopPosition = extractDt.Rows[i]["position"].ObjToInt32();
        //                    stopPosition += str.Length;
        //                    startPosition = -1;
        //                }
        //            }
        //            rtb.Document.RtfText = text;

        //            MatchCount = count;

        //            part3 = "";
        //            string field = "";
        //            string table = "";
        //            DataTable relationDt = null;
        //            string cmd = "";
        //            bool pass = false;
        //            string data = "";

        //            for ( int i=0; i<extractDt.Rows.Count; i++)
        //            {
        //                field = extractDt.Rows[i]["field"].ObjToString();
        //                if (pass)
        //                {
        //                    if (field.ToUpper().IndexOf("END_TABLE") >= 0)
        //                        pass = false;
        //                    continue;
        //                }
        //                if (field.ToUpper().IndexOf("BEGIN_TABLE") >= 0)
        //                {
        //                    part3 = extractDt.Rows[i]["detail"].ObjToString();
        //                    match = extractDt.Rows[i]["match"].ObjToString();
        //                    whatTable = extractDt.Rows[i]["relationship"].ObjToString();
        //                    try
        //                    {
        //                        for (int k = i + 1; k < extractDt.Rows.Count; k++)
        //                        {
        //                            field = extractDt.Rows[k]["field"].ObjToString();
        //                            field = field.Replace("[%", "");
        //                            field = field.Replace("%]", "");
        //                            DataRow[] dR = ddx.Select("field='" + field + "'");
        //                            if (dR.Length > 0)
        //                            {
        //                                cmd = "Select * from `" + table + "` where `contractNumber` = '" + workContractNumber + "' and `relationships` IN '" + whatTable + "';";
        //                                relationDt = G1.get_db_data(cmd);
        //                            }
        //                            if (field.ToUpper().IndexOf("END_TABLE") >= 0 || field.ToUpper().IndexOf("BEGIN_TABLE") >= 0)
        //                            {
        //                                text = rtb.Document.RtfText;
        //                                text = ReplaceField(text, match, part3);
        //                                rtb.Document.RtfText = text;
        //                            }
        //                            //else
        //                            //{
        //                            //    data = relationDt.Rows[j]["relationships"].ObjToString();
        //                            //    part3 = ReplaceField(part3, field, data);
        //                            //}
        //                        }
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                    }
        //                    pass = true;
        //                    part3 = "";
        //                    continue;
        //                }
        //                if (field.ToUpper().IndexOf("END_TABLE") >= 0)
        //                {
        //                    //text = rtb.Document.RtfText;
        //                    //text = ReplaceField(text, match, part3);
        //                    //rtb.Document.RtfText = text;
        //                    part3 = "";
        //                    continue;
        //                }
        //                //match = extractDt.Rows[i]["match"].ObjToString();
        //                //if (!String.IsNullOrWhiteSpace(match))
        //                //{
        //                //    whatTable = extractDt.Rows[i]["relationship"].ObjToString();
        //                //    text = rtb.Document.RtfText;
        //                //    //                    part3 = part3.Replace("D:FIRST_NAME", "X:FNAME");
        //                //    string extraPart = part3;
        //                //    part3 = part3 + extraPart;
        //                //    text = ReplaceField(text, match, part3);
        //                //    rtb.Document.RtfText = text;
        //                //    break;
        //                //}
        //            }
        //        }
        /***********************************************************************************************/
        private string parseRelationship(string text)
        {
            string originalText = text;
            string relationship = "";
            string[] relationships = LoadRelationships();
            int idx = text.ToUpper().IndexOf("RELATIONSHIPS=");
            if (idx < 0)
                return text;
            text = text.Substring(idx + 14);
            idx = text.IndexOf("%");
            if (idx < 0)
                return text;
            text = text.Substring(0, idx);

            string[] Lines = text.Split(',');
            text = "(";
            bool first = true;
            int count = 0;
            for (int i = 0; i < Lines.Length; i++)
            {
                if (!String.IsNullOrWhiteSpace(Lines[i]))
                {
                    relationship = Lines[i].Trim();
                    relationship = relationship.Replace("[%", "");
                    relationship = relationship.Replace("%]", "");
                    relationship = relationship.Replace("\'94", "");
                    relationship = relationship.Replace("\\u8221", "");
                    relationship = relationship.Replace("\\", "");
                    relationship = LocateRelationship(relationships, relationship);
                    if (!String.IsNullOrWhiteSpace(relationship))
                    {
                        if (!first)
                            text += ",";
                        text += "'";
                        text += relationship;
                        //                        text += Lines[i].Trim();
                        text += "'";
                        first = false;
                        count++;
                    }
                }
            }
            text += ")";
            if (count == 0)
                text = "";
            return text;
        }
        /***********************************************************************************************/
        private string LocateRelationship(string[] relationships, string relationship)
        {
            string actualRelationship = "";
            for (int i = (relationships.Length - 1); i >= 0; i--)
            {
                if (relationship.ToUpper().IndexOf(relationships[i].ToUpper()) >= 0)
                {
                    actualRelationship = relationships[i].Trim();
                    break;
                }
            }
            return actualRelationship;
        }
        /***********************************************************************************************/
        private string[] LoadRelationships()
        {
            string cmd = "Select * from `ref_relations`;";
            DataTable dt = G1.get_db_data(cmd);
            string relation = "";
            string relations = "";
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                relation = dt.Rows[i]["relationship"].ObjToString();
                relations += relation + ",";
            }
            relations += "all,";
            relations = relations.TrimEnd(',');
            string[] relationships = relations.Split(',');
            return relationships;
        }
        /***********************************************************************************************/
        private void ProcessBiography ( RichEditControl rtbx = null)
        {
            if (rtbx == null)
                rtbx = rtb;
            string cmd = "Select * from `agreements` where `formName` = 'Biography' AND `contractNumber` = '" + workContractNumber + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                string record = dt.Rows[0]["record"].ObjToString();
                string str = G1.get_db_blob("agreements", record, "image");
                if (str.IndexOf("rtf1") > 0)
                {
                    byte[] bytes = Encoding.ASCII.GetBytes(str);

                    MemoryStream stream = new MemoryStream(bytes);

                    DevExpress.XtraRichEdit.RichEditControl bioRTB = new DevExpress.XtraRichEdit.RichEditControl();
                    bioRTB.Document.LoadDocument(stream, DevExpress.XtraRichEdit.DocumentFormat.Rtf);
                    str = bioRTB.Document.Text;

                    string text = rtbx.Document.RtfText;
                    //int idx = text.IndexOf("[*BIOGRAPHY*]");
                    //if ( idx >= 0 )
                    //    rtb.Document.RtfText.Insert(idx, str);

                    text = ReplaceField(text, "[*BIOGRAPHY*]", str);
                    rtbx.Document.RtfText = text;
                }
            }
        }
        /***********************************************************************************************/
        /***********************************************************************************************/
        public static string GetDbField(string table, string field, string tableColumn, string qualifier, string contractNumber, int idx = 0, bool removeEmpty = false )
        {
            string data = "";
            string cmd = "";
            string str = "";
            try
            {
                cmd = "Select * from `" + table + "` where `contractNumber` = '" + contractNumber + "' ";
                if (!String.IsNullOrWhiteSpace(tableColumn) && !String.IsNullOrWhiteSpace(qualifier))
                    cmd += " and `" + tableColumn + "` = '" + qualifier + "' ";
                cmd += ";";
                DataTable dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    string strDelimitor = " +";
                    string[] Lines = field.Split(new[] { strDelimitor }, StringSplitOptions.None);
                    for (int i = 0; i < Lines.Length; i++)
                    {
                        field = Lines[i].Trim();
                        try
                        {
                            if (field.Trim() != "+")
                            {
                                if (idx > 0)
                                    str = dx.Rows[idx - 1][field].ObjToString().Trim();
                                else
                                    str = dx.Rows[0][field].ObjToString().Trim();
                                data = data.Trim();
                                data += " " + str;
                            }
                        }
                        catch
                        {
                            if (removeEmpty)
                                data = "";
                        }
                    }
                }
                else
                {
                    if (removeEmpty)
                        data = "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Looking up Table " + table + " Field " + field + " For ContractNumber " + contractNumber + "!!");
            }
            return data;
        }
        /***********************************************************************************************/
        public static string GetDbFieldAll(string table, string field, string tableColumn, string qualifier, string contractNumber, bool removeEmpty = false )
        {
            string data = "";
            string cmd = "";
            string str = "";
            string originalField = field;
            try
            {
                cmd = "Select * from `" + table + "` where `contractNumber` = '" + contractNumber + "' ";
                if (!String.IsNullOrWhiteSpace(tableColumn) && !String.IsNullOrWhiteSpace(qualifier))
                    cmd += " and `" + tableColumn + "` = '" + qualifier + "' ";
                cmd += ";";
                DataTable dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    for (int k = 0; k < dx.Rows.Count; k++)
                    {
                        string strDelimitor = " +";
                        string[] Lines = originalField.Split(new[] { strDelimitor }, StringSplitOptions.None);
                        for (int i = 0; i < Lines.Length; i++)
                        {
                            field = Lines[i].Trim();
                            try
                            {
                                if (field.Trim() != "+")
                                {
                                    str = dx.Rows[k][field].ObjToString().Trim();
                                    data = data.Trim();
                                    data += " " + str;
                                }
                            }
                            catch
                            {
                                if (removeEmpty)
                                    data = "";
                            }
                        }
                        if ( dx.Rows.Count > 1 )
                        {
                            if (dx.Rows.Count >= 2)
                                data += ", ";
                            if (k == (dx.Rows.Count - 2))
                                data += "and ";
                        }
                    }
                }
                else
                {
                    if (removeEmpty)
                        data = "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Looking up Table " + table + " Field " + field + " For ContractNumber " + contractNumber + "!!");
            }
            data = data.Trim();
            data = data.TrimEnd(',');
            return data;
        }
        /***********************************************************************************************/
        public static string GetDbField ( string table, string dbfield, string contractNumber )
        {
            string data = "";
            string cmd = "";
            string str = "";
            if ( table.ToUpper() == "RELATIVES")
            {

            }

            try
            {
                string myField = "";
                cmd = "Select * from `" + table + "` where `contractNumber` = '" + contractNumber + "';";
                DataTable dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    string strDelimitor = "+";
                    string[] Lines = dbfield.Split(new[] { strDelimitor }, StringSplitOptions.None);
                    for (int i = 0; i < Lines.Length; i++)
                    {
                        myField = Lines[i].Trim();
                        try
                        {
                            if (myField.Trim() != "+")
                            {
                                if (myField.IndexOf("\"") >= 0)
                                {
                                    string extraField = myField.Replace("\"", "");
                                    data += extraField;
                                }
                                else
                                {
                                    str = dx.Rows[0][myField].ObjToString().Trim();
                                    data = data.Trim();
                                    data += " " + str;
                                }
                            }
                        }
                        catch
                        {
                        }
                    }
                }
            }
            catch ( Exception ex )
            {
                MessageBox.Show("***ERROR*** Looking up Table " + table + " Field " + dbfield + " For ContractNumber " + contractNumber + "!!");
            }
            return data;
        }
        /***********************************************************************************************/
        private DataTable relativesDB = null;
        /***********************************************************************************************/
        private string BeginTable ( string field )
        {
            if ( relativesDB == null)
            {
                string cmd = "Select * from `relatives` where `contractNumber` = '" + workContractNumber + "';";
                relativesDB = G1.get_db_data(cmd);
            }
            string data = "";
            //BEGIN_TABLE:ACQUAINTANCES relationships=Daughter
            if ( field.ToUpper().IndexOf ( "DAUGHTER") >= 0 )
            {
                string relationship = "";
                string firstName = "";
                string lastName = "";
                for (int i = 0; i < relativesDB.Rows.Count; i++)
                {
                    relationship = relativesDB.Rows[i]["depRelationship"].ObjToString();
                    firstName = relativesDB.Rows[i]["depFirstName"].ObjToString();
                    lastName = relativesDB.Rows[i]["depLastName"].ObjToString();

                    if ( relationship.ToUpper() == "DAUGHTER")
                    {
                        if (!String.IsNullOrWhiteSpace(data))
                            data += "\r\n";
                        data += firstName + " " + lastName;
                    }
                }
            }
            return data;
        }
        /***********************************************************************************************/
        private DataTable LoadDbFields ( DataTable dt )
        {
            if (String.IsNullOrWhiteSpace(workContractNumber))
                return dt;
            string table = "";
            string field = "";
            string qualify = "";
            string dbfield = "";
            string tableColumn = "";
            string qualifier = "";
            int idx = 0;
            string data = "";
            string str = "";
            string cmd = "";
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                field = dt.Rows[i]["field"].ObjToString();
                if (field.ToUpper() == "AGE")
                    dt.Rows[i]["F2"] = GetDecAge(workContractNumber);
                else if (field.ToUpper() == "SSN")
                    dt.Rows[i]["F2"] = GetSSN(workContractNumber);
                else if (field.ToUpper() == "DOD")
                    dt.Rows[i]["F2"] = GetDOD(workContractNumber);
                else if (field.ToUpper() == "HESHECAP")
                    dt.Rows[i]["F2"] = GetDOD(workContractNumber);
                else if (field.ToUpper().IndexOf("OBIT-") >= 0)
                {
                    int idxx = field.ToUpper().IndexOf("OBIT-");
                    if (idxx >= 0)
                    {
                        cmd = field.Substring(idxx + 5);
                        str = LoadAndInstallForm(workContractNumber, cmd);
                        dt.Rows[i]["F2"] = str;
                    }
                    continue;
                }
                else if (field.ToUpper().IndexOf("MEMBERS,") >= 0)
                {
                    table = dt.Rows[i]["table"].ObjToString();
                    dt.Rows[i]["F2"] = "";
                    str = ParseOutMembers(table, field);
                    dt.Rows[i]["F2"] = str;
                }
                table = dt.Rows[i]["table"].ObjToString();
                dbfield = dt.Rows[i]["dbfield"].ObjToString();
                if (String.IsNullOrWhiteSpace(table))
                    continue;
                if (String.IsNullOrWhiteSpace(dbfield))
                    continue;
                qualify = dt.Rows[i]["qualify"].ObjToString();
                if (!String.IsNullOrWhiteSpace(qualify))
                {
                    if (!String.IsNullOrWhiteSpace(field))
                    {
                        string[] Lines = qualify.Split('=');
                        if (Lines.Length == 2)
                        {
                            tableColumn = Lines[0];
                            qualifier = Lines[1];
                            idx = G1.StripNumeric(field);
                            if ( field.ToUpper() == "PBX" || field.ToUpper() == "HPBX" || field.ToUpper() == "CLERGYX" || field.ToUpper() == "MUSICIANX")
                            {
                                data = GetDbFieldAll(table, dbfield, tableColumn, qualifier, workContractNumber);
                            }
                            else
                                data = GetDbField(table, dbfield, tableColumn, qualifier, workContractNumber, idx);

                        }
                    }
                    else
                        data = GetDbField(table, dbfield, workContractNumber);
                }
                else
                    data = GetDbField(table, dbfield, workContractNumber);
                if ( G1.validate_numeric ( data ) && data.IndexOf ( ".") >= 0 )
                {
                    double money = data.ObjToDouble();
                    data = G1.ReformatMoney(money);
                }
                dt.Rows[i]["F2"] = data;
            }
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                data = dt.Rows[i]["field"].ObjToString();
                if (data.ToUpper().IndexOf("MEMBER") >= 0)
                {
                    data = dt.Rows[i]["F2"].ObjToString();
                    string[] Lines = data.Split('\n');
                }
            }
            return dt;
        }
        /***********************************************************************************************/
        private DataTable ParseOutTableMembers(string table, string field )
        {
            bool all = false;
            string[] Lines = field.Split('~');
            string query = "";
            string[] LLines = Lines[1].Split(',');
            for (int kk = 0; kk < LLines.Length; kk++)
            {
                query += "'" + LLines[kk].Trim() + "',";
                if (LLines[kk].Trim().ToUpper().IndexOf("ALL") >= 0)
                    all = true;
            }
            query = query.TrimEnd(',');
            string cmd = "Select * from `" + table + "` ";
            if ( !all )
                cmd += " where `depRelationship` IN (" + query + ") ";
            cmd += ";";

            DataTable rDt = G1.get_db_data(cmd);
            return rDt;
        }
        /***********************************************************************************************/
        public static string ParseOutMembers ( string table, string field)
        {
            string data = "";
            string[] Lines = field.Split('~');
            string query = "";
            string[] LLines = Lines[1].Split(',');
            for (int kk = 0; kk < LLines.Length; kk++)
                query += "'" + LLines[kk].Trim() + "',";
            query = query.TrimEnd(',');
            string cmd = "Select * from `" + table + "` where `depRelationship` IN (" + query + ")";
            DataTable rDt = G1.get_db_data(cmd);
            for (int i = 0; i < rDt.Rows.Count; i++)
                data += "Member " + i.ToString() + "\n";
            data = data.TrimEnd('\n');
            return data;
        }
        /***********************************************************************************************/
        private string LoadAndInstallForm ( string workContractNumber, string formName )
        {
            string rv = "";
            string cmd = "Select * from `agreements` where `contractNumber` = '" + workContractNumber + "' and `formName` = '" + formName + "';";
            DataTable ddx = G1.get_db_data(cmd);
            if (ddx.Rows.Count <= 0)
                return rv;
            string record = ddx.Rows[0]["record"].ObjToString();
            string str = G1.get_db_blob("agreements", record, "image");
            if ( String.IsNullOrWhiteSpace ( str ))
            {
                cmd = "Select * from `arrangementForms` where `formName` = '" + formName + "' AND `location` = '' ;";
                ddx = G1.get_db_data(cmd);
                if (ddx.Rows.Count <= 0)
                    return rv;
                record = ddx.Rows[0]["record"].ObjToString();
                str = G1.get_db_blob("arrangementForms", record, "image");
                if (String.IsNullOrWhiteSpace(str))
                    return rv;
            }
            if (str.IndexOf("rtf1") > 0)
            {
                byte[] b = Encoding.UTF8.GetBytes(str);
                MemoryStream stream = new MemoryStream(b);
                DevExpress.XtraRichEdit.RichEditControl rtbx = new RichEditControl();
                rtbx.Document.Delete(rtbx.Document.Range);
                rtbx.Document.LoadDocument(stream, DevExpress.XtraRichEdit.DocumentFormat.Rtf);
                string tt = rtbx.Document.Text;
                DataTable dx = Structures.ExtractFields(rtbx.Document.RtfText);
                LoadFields(dx, rtbx, formName , "" );
                //RTF_Stuff.LoadFields(workContractNumber, dx, rtbx, "", formName);
                //DataTable dt = Structures.LoadFields(dx, "", formName);
                //dt = LoadDbFieldsx( workContractNumber, dt);
                //PushFieldsToForm(dt, rtbx);
                tt = rtbx.Document.Text;
                rv = rtbx.Document.RtfText;
            }
            return rv;
        }
        /***********************************************************************************************/
        private DataTable extractDt = null;
        /***********************************************************************************************/
        private DataTable ExtractFields ( RichEditControl rtb )
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("fields");
            dt.Columns.Add("mod");
            dt.Columns.Add("status");

            string text = rtb.Document.RtfText;

            string lines = Structures.ParseFields(text, "[*");
            string[] Lines = lines.Split('\n');
            string str = "";
            for (int i = 0; i < Lines.Length; i++)
            {
                DataRow dRow = dt.NewRow();
                str = Lines[i];

                dRow["fields"] = str;
                dt.Rows.Add(dRow);
            }
            lines = Structures.ParseFields(text, "[%");
            Lines = lines.Split('\n');
            str = "";
            for (int i = 0; i < Lines.Length; i++)
            {
                DataRow dRow = dt.NewRow();
                str = Lines[i];

                dRow["fields"] = str;
                dt.Rows.Add(dRow);
            }
            return dt;
        }
        /********************************************************************************************/
        private void AddDaughters ()
        {
            string text = rtb.Document.RtfText;
            int idx = text.IndexOf("[%R.D");
            if (idx < 0)
                return;
            string cmd = "Select * from `relatives` where `contractNumber` = '" + workContractNumber + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return;
            string daughters = "Name 1 \nName 2\nName 3";
            text = ReplaceField(text, "[%R.DAUGHTER%", daughters);
            string sons = "Son1\nSon2\nSon3";
            text = ReplaceField(text, "[%R.SON%", sons);

        }
        /***************************************************************************************/
        public delegate void d_void_eventdone_bool();
        public event d_void_eventdone_bool RtfModified;
        /***************************************************************************************/
        public delegate void d_void_eventdone_rtf(string filename, string record, string contractNumber, string rtfText, bool dontAsk, bool force );
        public event d_void_eventdone_rtf RtfFinished;
        /***************************************************************************************/
        public delegate void d_void_eventdone_string ( string accept );
        public event d_void_eventdone_string AcceptMemory;
        /***************************************************************************************/
        public delegate void d_void_eventdone_bytes(string filename, string contractNumber, string rtfText);
        public event d_void_eventdone_bytes RtfDone;
        protected void OnDone( bool dontAsk = false )
        {
            if (RtfDone != null)
            {
                string arg1 = workFile;
                if (!String.IsNullOrWhiteSpace(workContractNumber))
                    arg1 = workRecord;
                RtfDone.Invoke(arg1, workContractNumber, rtb.Document.RtfText);
            }
            else if (RtfFinished != null)
            {
                if (!String.IsNullOrWhiteSpace(workFile) && String.IsNullOrWhiteSpace(workFormName))
                    workFormName = workFile;
                if (!String.IsNullOrWhiteSpace(pdfRecord))
                {
                    if (!workContract)
                        workRecord = pdfRecord;
                    else if (createRecord)
                        workRecord = "";
                }
                if (rtb != null)
                {
                    if (rtb.Document != null)
                    {
                        if ( rtb.Document.RtfText != null )
                            RtfFinished.Invoke(workFormName, workRecord, workContractNumber, rtb.Document.RtfText, dontAsk, true );
                    }
                }
            }
        }
        /***************************************************************************************/
        public void FireEventSaveFunForms(bool save = false)
        {
            OnDone();
        }
        /***********************************************************************************************/
        private void ArrangementForms_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (RtfDone == null && RtfFinished == null )
                return;
            if (!String.IsNullOrWhiteSpace(workContractNumber))
            {
                if (rtb.Modified)
                {
                    modified = false;
                    MemoryStream ms = new MemoryStream();
                    try
                    {
                        OnDone();
                    }
                    catch (Exception ex)
                    {
                    }
                    return;
                }
            }
            DialogResult result = DialogResult.Yes;
            if (!workForceSave)
            {
                if (!modified)
                    return;
                result = MessageBox.Show("Do you want to save this file into the database?", "Save Data Dialog", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation);
                if (result == DialogResult.No)
                    return;
                if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
            }
            if (result == DialogResult.Yes)
            {
                modified = false;
                MemoryStream ms = new MemoryStream();
                try
                {
                    OnDone();
                }
                catch (Exception ex)
                {
                }
            }
        }
        /***********************************************************************************************/
        private void btnSeparate_Click(object sender, EventArgs e)
        {
            WaitCursor();

            string str = rtb.Document.RtfText;

            byte[] b = Encoding.UTF8.GetBytes(str);

            ArrangementForms editFunForm = new ArrangementForms(workFormName, workLocation, workRecord, workContractNumber, b );
            editFunForm.RtfFinished += EditFunForm_RtfFinished;
            editFunForm.Show();
            DefaultCursor();
        }
        /***********************************************************************************************/
        private void EditFunForm_RtfFinished(string filename, string record, string contractNumber, string rtfText, bool dontAsk, bool force )
        {
            rtb.Document.RtfText = rtfText;
            rtb.Refresh();
            if (RtfModified != null)
                RtfModified.Invoke();
        }
        /***********************************************************************************************/
        private void btnEditDetails_Click(object sender, EventArgs e)
        {
            WaitCursor();
            EditFormData editForm = new EditFormData(workContractNumber, workFormName, workLocation, workRecord);
            editForm.Show();
            DefaultCursor();
        }
        /***********************************************************************************************/
        private void fileOpenItem1_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
//            OpenFileDialog openFileDialog1 = new OpenFileDialog
//            {
//                InitialDirectory = @"C:\users\robby\documents",
//                Title = "Browse Files",

//                CheckFileExists = true,
//                CheckPathExists = true,

//                DefaultExt = "",
//                Filter = "all files (*.*)|*.*",

//                FilterIndex = 2,
//                RestoreDirectory = true,

//                ReadOnlyChecked = true,
//                ShowReadOnly = true
//            };

//            if (openFileDialog1.ShowDialog() != DialogResult.OK)
//            {
//                this.Cursor = Cursors.Default;
//                return;
//            }
//            this.Cursor = Cursors.WaitCursor;
//            string filename = openFileDialog1.FileName;

//            this.Cursor = Cursors.WaitCursor;
//            ArrangementForms aForm = new ArrangementForms(filename, workContractNumber);
////            aForm.RtfFinished += AForm_RtfDone;
//            aForm.Show();
        }
        /***********************************************************************************************/
        private void btnRemoveEmpty_Click(object sender, EventArgs e)
        {
            //if (1 == 1)
            //    return;
            if ( removeEmpty )
            {
                removeEmpty = false;
                btnRemoveEmpty.Text = "Remove Empty Fields";
                ArrangementForms_Load(null, null);
                //LoadAgreementData();
            }
            else
            {
                btnRemoveEmpty.Text = "Show Empty Fields";
                removeEmpty = true;
                //ArrangementForms_Load(null, null);
                //LoadAgreementData(); // Old Code
            }
        }
        /***********************************************************************************************/
        private void btnShowRibbon_Click(object sender, EventArgs e)
        {
            if ( ribbonControl1.Visible )
            {
                ribbonControl1.Visible = false;
                btnShowRibbon.Text = "Show Ribbon Controls";
                this.menuStrip1.Show();
            }
            else
            {
                ribbonControl1.Visible = true;
                btnShowRibbon.Text = "Hide Ribbon Controls";
                //this.menuStrip1.Hide();
            }
        }
        /***********************************************************************************************/
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        /***********************************************************************************************/
        private void CheckPrintLayout ()
        {
            System.Drawing.Printing.PaperKind kind = rtb.Document.Sections[0].Page.PaperKind;

            DevExpress.XtraRichEdit.API.Native.Implementation.NativeSectionPage page = (DevExpress.XtraRichEdit.API.Native.Implementation.NativeSectionPage)rtb.Document.Sections[0].Page;

            float txtHgt = page.Height;
            float textBoxHeight = Units.DocumentsToPixelsF(txtHgt, rtb.DpiY);

            txtHgt = page.Width;
            float textBoxWidth = Units.DocumentsToPixelsF(txtHgt, rtb.DpiY);

            if (textBoxHeight == 912F && textBoxWidth == 624F)
            {
                //Printer.setupPrinterMargins(300, 100, 100, 50);
                rtb.ActiveViewType = RichEditViewType.Simple;
                forceNoBorder = true;
            }
        }
        /***********************************************************************************************/
        private int pageMarginLeft = 0;
        private int pageMarginRight = 0;
        private int pageMarginTop = 0;
        private int pageMarginBottom = 0;
        /***********************************************************************************************/
        private void printPreviewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            printPreview();
        }
        /***********************************************************************************************/
        private void printPreview()
        {
            if (this.components == null)
                this.components = new System.ComponentModel.Container();

            DevExpress.XtraPrinting.PrintingSystem printingSystem1 = new DevExpress.XtraPrinting.PrintingSystem(this.components);
            DevExpress.XtraPrinting.PrintableComponentLink printableComponentLink1 = new DevExpress.XtraPrinting.PrintableComponentLink(this.components);

            printingSystem1.PageSettingsChanged += PrintingSystem1_PageSettingsChanged;

            printingSystem1.SetCommandVisibility(PrintingSystemCommand.DocumentMap, CommandVisibility.All);

            printingSystem1.Links.AddRange(new object[] {
            printableComponentLink1});


            printableComponentLink1.Component = rtb;
            printableComponentLink1.PrintingSystemBase = printingSystem1;

            printableComponentLink1.EnablePageDialog = true;

            printableComponentLink1.CreateDetailHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateDetailHeaderArea);
            printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);
            printableComponentLink1.BeforeCreateAreas += new System.EventHandler(this.printableComponentLink1_BeforeCreateAreas);
            printableComponentLink1.AfterCreateAreas += new System.EventHandler(this.printableComponentLink1_AfterCreateAreas);
            printableComponentLink1.Landscape = true;

            if ( forceNoBorder )
                Printer.setupPrinterMargins(0, 0, 0, 0);
            else
                Printer.setupPrinterMargins(50, 100, 100, 50);

            pageMarginLeft = Printer.pageMarginLeft;
            pageMarginRight = Printer.pageMarginRight;
            pageMarginTop = Printer.pageMarginTop;
            pageMarginBottom = Printer.pageMarginBottom;

            printableComponentLink1.Margins.Left = pageMarginLeft;
            printableComponentLink1.Margins.Right = pageMarginRight;
            printableComponentLink1.Margins.Top = pageMarginTop;
            printableComponentLink1.Margins.Bottom = pageMarginBottom;

            printableComponentLink1.CreateDocument();
            printableComponentLink1.ShowPreview();
        }
        /***********************************************************************************************/
        private void PrintingSystem1_PageSettingsChanged(object sender, EventArgs e)
        {
        }
        /***********************************************************************************************/
        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.components == null)
                this.components = new System.ComponentModel.Container();
            DevExpress.XtraPrinting.PrintingSystem printingSystem1 = new DevExpress.XtraPrinting.PrintingSystem(this.components);
            DevExpress.XtraPrinting.PrintableComponentLink printableComponentLink1 = new DevExpress.XtraPrinting.PrintableComponentLink(this.components);

            printingSystem1.Links.AddRange(new object[] {
            printableComponentLink1});

            printableComponentLink1.Component = rtb;
            printableComponentLink1.PrintingSystemBase = printingSystem1;
            printableComponentLink1.CreateDetailHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateDetailHeaderArea);
            printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);
            printableComponentLink1.BeforeCreateAreas += new System.EventHandler(this.printableComponentLink1_BeforeCreateAreas);
            printableComponentLink1.AfterCreateAreas += new System.EventHandler(this.printableComponentLink1_AfterCreateAreas);
            printableComponentLink1.Landscape = true;

            if (forceNoBorder)
                Printer.setupPrinterMargins(0, 0, 0, 0);
            else
                Printer.setupPrinterMargins(50, 100, 100, 50);

            pageMarginLeft = Printer.pageMarginLeft;
            pageMarginRight = Printer.pageMarginRight;
            pageMarginTop = Printer.pageMarginTop;
            pageMarginBottom = Printer.pageMarginBottom;

            printableComponentLink1.Margins.Left = pageMarginLeft;
            printableComponentLink1.Margins.Right = pageMarginRight;
            printableComponentLink1.Margins.Top = pageMarginTop;
            printableComponentLink1.Margins.Bottom = pageMarginBottom;

            printableComponentLink1.CreateDocument();
            printableComponentLink1.PrintDlg();
        }
        /***********************************************************************************************/
        private void printableComponentLink1_BeforeCreateAreas(object sender, EventArgs e)
        {
        }
        /***********************************************************************************************/
        private void printableComponentLink1_AfterCreateAreas(object sender, EventArgs e)
        {
        }
        /***********************************************************************************************/
        private void printableComponentLink1_CreateDetailHeaderArea(object sender, CreateAreaEventArgs e)
        {
        }
        /***********************************************************************************************/
        private void printableComponentLink1_CreateMarginalHeaderArea(object sender, CreateAreaEventArgs e)
        {
            //if (forceNoBorder)
            //return;
            if (!String.IsNullOrWhiteSpace(workContractNumber))
                return;
            Printer.setupPrinterQuads(e, 2, 3);
            Font font = new Font("Ariel", 10, FontStyle.Bold);
            string heading = "South Mississippi Funeral Services, LLC (" + workFormName + ")";
            Printer.DrawQuad(1, 1, Printer.xQuads, 2, heading, Color.Black, BorderSide.None, font, HorizontalAlignment.Left);

            //Printer.SetQuadSize(12, 12);
            //font = new Font("Ariel", 8);
            //Printer.DrawGridDate(2, 8, 2, 6, Color.Black, BorderSide.None, font);
//            Printer.DrawGridPage(9, 8, 3, 6, Color.Black, BorderSide.None, font);
            if (1 == 1)
                return;

            Printer.SetQuadSize(12, 12);

            font = new Font("Ariel", 8);
            Printer.DrawGridDate(2, 3, 2, 3, Color.Black, BorderSide.None, font);
            Printer.DrawGridPage(11, 3, 2, 3, Color.Black, BorderSide.None, font);

            Printer.DrawQuad(1, 9, 2, 3, "User : " + LoginForm.username, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);

            font = new Font("Ariel", 10, FontStyle.Regular);
            Printer.DrawQuad(4, 8, 7, 4, "Contract " + workContractNumber, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            //            Printer.DrawQuadTicks();
            DateTime date = DateTime.Now;
            string workDate = date.Month.ToString("D2") + "/" + date.Year.ToString("D4");
            Printer.SetQuadSize(24, 12);
            font = new Font("Ariel", 9, FontStyle.Bold);
            //            Printer.DrawQuad(20, 8, 5, 4, "Report Month:" + workDate, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(16, 8, 3, 4, lblPayment.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(19, 8, 3, 4, lblTrust85.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(22, 8, 3, 4, lblTrust100.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /***********************************************************************************************/
        public static string ProcessRTB ( string contractNumber, string formLocation, string formName, string rtf )
        {
//            DataTable dx = Structures.ExtractFields(rtf);
//            DataTable dt = Structures.LoadFields(dx, formLocation, formName);
//            dt = LoadDbFieldsx(contractNumber, dt);
//            PushFieldsToFormx(dt, rtf);
//            //rtbx.Document.RtfText = text;
////            ProcessTables(dt, rtbx);

            return rtf;
        }
        ///***********************************************************************************************/
        //public static DataTable LoadDbFieldsx( string workContractNumber, DataTable dt)
        //{
        //    if (String.IsNullOrWhiteSpace(workContractNumber))
        //        return dt;
        //    string table = "";
        //    string field = "";
        //    string qualify = "";
        //    string dbfield = "";
        //    string tableColumn = "";
        //    string qualifier = "";
        //    int idx = 0;
        //    string data = "";
        //    string str = "";
        //    string cmd = "";
        //    for (int i = 0; i < dt.Rows.Count; i++)
        //    {
        //        field = dt.Rows[i]["field"].ObjToString();
        //        if (field.ToUpper() == "AGE")
        //            dt.Rows[i]["F2"] = GetDecAge( workContractNumber);
        //        else if (field.ToUpper() == "SSN")
        //            dt.Rows[i]["F2"] = GetSSN( workContractNumber);
        //        else if (field.ToUpper() == "DOD")
        //            dt.Rows[i]["F2"] = GetDOD( workContractNumber);
        //        else if (field.ToUpper().IndexOf("OBIT-") >= 0)
        //        {
        //            int idxx = field.ToUpper().IndexOf("OBIT-");
        //            if (idxx >= 0)
        //            {
        //                cmd = field.Substring(idxx + 5);
        //                str = LoadAndInstallForm(workContractNumber, cmd);
        //                dt.Rows[i]["F2"] = str;
        //            }
        //            continue;
        //        }
        //        else if (field.ToUpper().IndexOf("MEMBERS,") >= 0)
        //        {
        //            table = dt.Rows[i]["table"].ObjToString();
        //            dt.Rows[i]["F2"] = "";
        //            str = ParseOutMembers(table, field);
        //            dt.Rows[i]["F2"] = str;
        //        }
        //        table = dt.Rows[i]["table"].ObjToString();
        //        dbfield = dt.Rows[i]["dbfield"].ObjToString();
        //        if (String.IsNullOrWhiteSpace(table))
        //            continue;
        //        if (String.IsNullOrWhiteSpace(dbfield))
        //            continue;
        //        qualify = dt.Rows[i]["qualify"].ObjToString();
        //        if (!String.IsNullOrWhiteSpace(qualify))
        //        {
        //            if (!String.IsNullOrWhiteSpace(field))
        //            {
        //                string[] Lines = qualify.Split('=');
        //                if (Lines.Length == 2)
        //                {
        //                    tableColumn = Lines[0];
        //                    qualifier = Lines[1];
        //                    idx = G1.StripNumeric(field);
        //                    if (field.ToUpper() == "PBX" || field.ToUpper() == "HPBX" || field.ToUpper() == "CLERGYX" || field.ToUpper() == "MUSICIANX")
        //                    {
        //                        data = GetDbFieldAll(table, dbfield, tableColumn, qualifier, workContractNumber);
        //                    }
        //                    else
        //                        data = GetDbField(table, dbfield, tableColumn, qualifier, workContractNumber, idx);

        //                }
        //            }
        //            else
        //                data = GetDbField(table, dbfield, workContractNumber);
        //        }
        //        else
        //            data = GetDbField(table, dbfield, workContractNumber);
        //        if (G1.validate_numeric(data) && data.IndexOf(".") >= 0)
        //        {
        //            double money = data.ObjToDouble();
        //            data = G1.ReformatMoney(money);
        //        }
        //        dt.Rows[i]["F2"] = data;
        //    }
        //    for (int i = 0; i < dt.Rows.Count; i++)
        //    {
        //        data = dt.Rows[i]["field"].ObjToString();
        //        if (data.ToUpper().IndexOf("MEMBER") >= 0)
        //        {
        //            data = dt.Rows[i]["F2"].ObjToString();
        //            string[] Lines = data.Split('\n');
        //        }
        //    }
        //    return dt;
        //}
        /***********************************************************************************************/
        public static string PushFieldsToFormx(DataTable dt, string rtf, bool removeEmpty = false )
        {
            string text = rtf;
            string field = "";
            string data = "";
            string qualify = "";
            bool pass = false;
            int i = 0;
            try
            {
                for (i = 0; i < dt.Rows.Count; i++)
                {
                    field = dt.Rows[i]["F1"].ObjToString();
                    //                field = dt.Rows[i]["field"].ObjToString();
                    if (field.ToUpper().IndexOf("BEGIN_TABLE") >= 0)
                    {
                        qualify = dt.Rows[i]["qualify"].ObjToString();
                        pass = true;
                        continue;
                    }
                    else if (field.ToUpper().IndexOf("END_TABLE") >= 0)
                    {
                        pass = false;
                        continue;
                    }
                    if (pass)
                        continue;
                    data = dt.Rows[i]["F2"].ObjToString();
                    if (String.IsNullOrWhiteSpace(field))
                        continue;
                    if (String.IsNullOrWhiteSpace(data))
                    {
                        if (removeEmpty)
                            text = ReplaceField(text, field, data);
                        continue;
                    }
                    if (data.IndexOf("rtf1") >= 0)
                    {
                        text = ReplaceField(text, field, data);
                        //                    rtbx.Document.RtfText = text;
                    }
                    else
                        text = ReplaceField(text, field, data);
                }
            }
            catch (Exception ex)
            {

            }
            //rtbx.Document.RtfText = text;
            //ProcessTables(dt, rtbx);
            return text;
        }
        /***********************************************************************************************/
        public static string GetBranch ()
        {
            string branch = "";
            string cmd = "Select * from `funeralhomes` where `keycode` = '" + LoginForm.activeFuneralHomeKeyCode + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return "";
            string activeFuneralHomeName = dt.Rows[0]["name"].ObjToString();
            if (!String.IsNullOrWhiteSpace(activeFuneralHomeName))
                branch = activeFuneralHomeName;
            return branch;
        }
        /***********************************************************************************************/
        public static string GetBranchCityState()
        {
            string branchcitystate = "";
            string cmd = "Select * from `funeralhomes` where `keycode` = '" + LoginForm.activeFuneralHomeKeyCode + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return "";
            string activeFuneralHomeName = dt.Rows[0]["name"].ObjToString();
            string city = dt.Rows[0]["city"].ObjToString();
            string state = dt.Rows[0]["state"].ObjToString();
            string zip = dt.Rows[0]["zip"].ObjToString();
            branchcitystate = city + ", " + state;
            return branchcitystate;
        }
        /***********************************************************************************************/
        public static string LoadFuneralHomeAddress(EMRControlLib.RichTextBoxEx rtb)
        {
            string cmd = "Select * from `funeralhomes` where `keycode` = '" + LoginForm.activeFuneralHomeKeyCode + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
            {
                MessageBox.Show("***ERROR*** Invalid Funeral Home!");
                //this.Close();
                return "";
            }
            string activeFuneralHomeName = dt.Rows[0]["name"].ObjToString();
            //if (String.IsNullOrWhiteSpace(activeFuneralHomeName))
            //    activeFuneralHomeName = dt.Rows[0]["name"].ObjToString();
            if (!String.IsNullOrWhiteSpace(activeFuneralHomeName))
            {
                rtb.Rtf = rtb.Rtf.Replace("*FUNERALHOME*", activeFuneralHomeName);
                rtb.Rtf = rtb.Rtf.Replace("[*BRANCH*]", activeFuneralHomeName);
                //                rtb.Rtf = ArrangementForms.ReplaceField(rtb.Rtf, "*FUNERALHOME*", activeFuneralHomeName);

            }

            string poBox = dt.Rows[0]["POBox"].ObjToString();
            string address = dt.Rows[0]["address"].ObjToString();
            if (!String.IsNullOrWhiteSpace(poBox))
                address += "     " + poBox;
            if (!String.IsNullOrWhiteSpace(address))
                rtb.Rtf = rtb.Rtf.Replace("%FUNERALADDRESS%", address);

            string city = dt.Rows[0]["city"].ObjToString();
            string state = dt.Rows[0]["state"].ObjToString();
            string zip = dt.Rows[0]["zip"].ObjToString();
            address = city + ", " + state + "  " + zip;
            if (!String.IsNullOrWhiteSpace(city) || !String.IsNullOrWhiteSpace(state))
            {
                rtb.Rtf = rtb.Rtf.Replace("%FUNERALLOCATION%", address);
                rtb.Rtf = rtb.Rtf.Replace("[*BRANCHCTYST*]", address);
            }

            string phone = dt.Rows[0]["phoneNumber"].ObjToString();
            if (!String.IsNullOrWhiteSpace(phone))
                rtb.Rtf = rtb.Rtf.Replace("%FUNERALPHONE%", phone);


            //% FUNERALHOME %
            //% FUNERALADDRESS %
            //% FUNERALLOCATION %
            //% FUNERALPHONE %
            return activeFuneralHomeName;
        }
        /***********************************************************************************************/
        private void btnSave_Click(object sender, EventArgs e)
        {
            OnDone( true );
            modified = false;
            btnSave.Hide();
        }
        /***********************************************************************************************/
        private void cmbTitles_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox combo = (ComboBox)sender;
            string title = combo.Text.Trim();
            if (String.IsNullOrWhiteSpace(title))
                return;
            //string content = RTF_Stuff.LoadGeneralTitle(title);

            //content = content.Replace("\n", "\\line ");
            //string text = this.rtb.Document.RtfText;

            //text = RTF_Stuff.ReplaceField(text, "[*MEMTITLE*]", title);
            //text = RTF_Stuff.ReplaceField(text, "[*MEMCONTENT*]", content);

            //RichTextBox rtb = new RichTextBox();
            //rtb.Rtf = text;

            WaitCursor();

            ArrangementForms newForm = new ArrangementForms(workFormName, workLocation, workRecord, workContractNumber, filearray, true, forceNoBorder, title );
            newForm.AcceptMemory += NewForm_AcceptMemory;
            newForm.Show();

            DefaultCursor();


            //this.rtb.Document.RtfText= text;

            //this.lblTitles.Hide();
            //this.cmbTitles.Hide();

            //modified = true;
            //btnSave.Show();
            //btnSave.Refresh();
        }
        /***********************************************************************************************/
        private void NewForm_AcceptMemory(string accept)
        {
            if (accept.ToUpper() != "YES")
                return;
            string title = cmbTitles.Text.Trim();
            if (String.IsNullOrWhiteSpace(title))
                return;
            string content = RTF_Stuff.LoadGeneralTitle(title);

            content = content.Replace("\n", "\\line ");
            string text = this.rtb.Document.RtfText;

            text = RTF_Stuff.ReplaceField(text, "[*MEMTITLE*]", title);
            text = RTF_Stuff.ReplaceField(text, "[*MEMCONTENT*]", content);
            this.rtb.Document.RtfText = text;
            modified = true;
            if (RtfModified != null)
                RtfModified.Invoke();
            btnSave.Show();
            btnSave.Refresh();
            this.Refresh();
        }
        /***********************************************************************************************/
        private void chkAccept_CheckedChanged(object sender, EventArgs e)
        {
            if (AcceptMemory != null && chkAccept.Checked)
            {
                AcceptMemory.Invoke("YES");
                modified = false;
                this.Close();
            }
        }
        /***************************************************************************************/
        private void WaitCursor()
        {
            this.Cursor = System.Windows.Forms.Cursors.WaitCursor;
        }
        private void DefaultCursor()
        {
            this.Cursor = System.Windows.Forms.Cursors.Default;
        }
        /***********************************************************************************************/
        private DateTime GetLastModifiedTime ( string filePath )
        {
            FileInfo file = new FileInfo(filePath);
            // file creation time
            DateTime dt = file.CreationTime;
            Console.WriteLine(dt);
            // last access time
            dt = file.LastAccessTime;
            Console.WriteLine(dt);
            // last write time
            dt = file.LastWriteTime;
            return dt;
        }
        /***********************************************************************************************/
        private void btnOpenWord_Click(object sender, EventArgs e)
        {
            btnOpenWord.Hide();
            btnOpenWord.Refresh();
            PleaseWait pleaseForm = G1.StartWait("Please Wait for Word!");

            string str = "";
            string user = LoginForm.username.Trim();
            string filePath = @"c:\SMFS_Other\" + user + "_rawRtfText.rtf";

            if (File.Exists(filePath))
            {
                try
                {
                    File.SetAttributes(filePath, FileAttributes.Normal);
                    File.Delete(filePath);
                }
                catch ( Exception ex)
                {
                    MessageBox.Show("*** ERROR *** Problem Removing File " + filePath + "!", "Word Error Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    btnOpenWord.Show();
                    btnOpenWord.Refresh();
                    return;
                }
            }

            //write the raw RTF string to a text file.
            System.IO.StreamWriter rawTextFile = new System.IO.StreamWriter(filePath, false);
            str = rtb.RtfText.ObjToString();
            rawTextFile.Write(str);
            rawTextFile.Close();

            DateTime writeTime = GetLastModifiedTime(filePath);

            bool gotError = ShowExternalReference(filePath, true);
            if (gotError)
            {
                G1.StopWait(ref pleaseForm);
                btnOpenWord.Show();
                btnOpenWord.Refresh();
                return;
            }

            DateTime lastWriteTime = GetLastModifiedTime(filePath);
            if (lastWriteTime != writeTime)
            {
                G1.StopWait(ref pleaseForm );

                DialogResult result = MessageBox.Show("Restore Word File Here?", "Word Restore Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                if (result == DialogResult.Yes)
                {
                    btnSave.Show();
                    btnSave.Refresh();

                    rtb.Document.Delete(rtb.Document.Range);

                    byte[] bte = File.ReadAllBytes(filePath); // Put the Reading file

                    str = G1.ConvertToString(bte);
                    byte[] bytes = Encoding.ASCII.GetBytes(str);

                    MemoryStream stream = new MemoryStream(bytes);

                    //rtb.Document.Delete(rtb.Document.Range);

                    rtb.Document.LoadDocument(stream, DevExpress.XtraRichEdit.DocumentFormat.Rtf);

                    //rtb.Document.LoadDocument(filePath, DevExpress.XtraRichEdit.DocumentFormat.Doc);
                    rtb.Show();
                    this.BringToFront();
                }
            }
            else
                G1.StopWait(ref pleaseForm);

            btnOpenWord.Show();
            btnOpenWord.Refresh();

            if ( File.Exists ( filePath))
            {
                try
                {
                    File.Delete(filePath);
                }
                catch ( Exception ex)
                {
                }
            }

            //now open the RTF file using word.
            //Microsoft.Office.Interop.Word.Application msWord = new Microsoft.Office.Interop.Word.Application();
            //msWord.Visible = true;
            //Microsoft.Office.Interop.Word.Document wordDoc = msWord.Documents.Open(filePath);
        }
        /***********************************************************************************************/
    }
}