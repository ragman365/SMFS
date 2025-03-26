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
using GeneralLib;
using DevExpress.Pdf;
using DevExpress.XtraBars.Ribbon;
using Microsoft.Ink;
using DevExpress.Pdf;
using DevExpress.Pdf.Interop;
using System.Drawing;
using iTextSharp.text.pdf;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class ViewPDF : DevExpress.XtraEditors.XtraForm
    {
        private string workTitle = "";
        private string workFile = "";
        private byte[] filearray = null;
        private byte[] workOriginal = null;
        private bool modified = false;
        private string workContract = "";
        private string workRecord = "";
        private bool workFullScreen = false;
        private bool showRibbon = false;
        private bool workLapses = false;
        private bool acceptLapses = false;
        private bool workGSContract = false;
        private bool gotSignatures = false;
        private bool fileGotPrinted = false;

        private Point _origin = Point.Empty;
        private Point _terminus = Point.Empty;
        private Boolean _draw = false;
        private List<Tuple<Point, Point>> _lines = new List<Tuple<Point, Point>>();

        private Graphics g = null;

        private DataTable fieldDt = null;
        public static DataTable signatureDt = null;
        /***********************************************************************************************/
        public ViewPDF(string title, string contractNumber, string filename, bool fullScreen = false, bool allowLapseButton = false )
        {
            InitializeComponent();
            workTitle = title;
            workFile = filename;
            workContract = contractNumber;
            workFullScreen = fullScreen;
            workLapses = allowLapseButton;
        }
        /***********************************************************************************************/
        public ViewPDF(string title, string filename, bool fullScreen = false)
        {
            InitializeComponent();
            workTitle = title;
            workFile = filename;
            workContract = "";
            workFullScreen = fullScreen;
            workLapses = false;
            showRibbon = true;
        }
        /***********************************************************************************************/
        public ViewPDF(string title, string filename, bool GSContract, string contract, bool doingSignatures = false, bool fullScreen = false )
        {
            InitializeComponent();
            workTitle = title;
            workFile = filename;
            workContract = contract;
            workFullScreen = fullScreen;
            workLapses = false;
            showRibbon = true;
            workGSContract = GSContract;
            gotSignatures = doingSignatures;
        }
        /***********************************************************************************************/
        public ViewPDF(string title, string record, string contractNumber, byte[] array, byte [] original = null )
        {
            InitializeComponent();
            workTitle = title;
            filearray = array;
            workContract = contractNumber;
            workFile = title;
            workRecord = record;
            workOriginal = original;
        }
        /***********************************************************************************************/
        private void ViewPDF_Loadx(object sender, EventArgs e)
        {
            //if (!G1.RobbyServer)
            //    button1.Hide();

            //i_overlay = new Microsoft.Ink.InkOverlay(tBox);
            //i_overlay.Enabled = true;
            //i_overlay.AutoRedraw = true;
            //tBox.Hide();

            this.AutoSize = false;
            ribbonControl1.Visible = false;
            if (showRibbon)
                ribbonControl1.Visible = true;
            ribbonControl1.Visible = true;
            //if (workFullScreen)
                GoFullscreen(true);
            modified = false;
            acceptLapses = false;
            //if (!workLapses)
            //{
            //    barButtonItem1.Dispose();
            //}
            this.Text = workTitle;
            try
            {
                if (filearray != null)
                {
                    MemoryStream stream = new MemoryStream(filearray);
                    pdfViewer1.DetachStreamAfterLoadComplete = true;
                    pdfViewer1.LoadDocument(stream);
                    pdfViewer1.FormFieldValueChanged += PdfViewer1_FormFieldValueChanged;
                    //using (PdfDocumentProcessor documentProcessor = new PdfDocumentProcessor())
                    //{
                    //    documentProcessor.LoadDocument(filePath + fileName + ".pdf");
                    //}

                    pdfFileSaveAsBarItem1.Enabled = false;
                    pdfFileOpenBarItem1.Enabled = false;
                }
                else
                {
                    pdfViewer1.LoadDocument(workFile);
                    pdfViewer1.FormFieldValueChanged += PdfViewer1_FormFieldValueChanged;
                    pdfFileSaveAsBarItem1.Enabled = false;
                    pdfFileOpenBarItem1.Enabled = false;
                    pdfViewer1.CloseDocument();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** File not found! " + ex.Message.ToString());
            }
            this.BringToFront();
            this.TopMost = true;
        }
        /***********************************************************************************************/
        private void ViewPDF_Load(object sender, EventArgs e)
        {
            //if (!G1.RobbyServer)
            //    button1.Hide();

            if (workGSContract)
                btnSign.Hide();
            else
                btnSign.Show();

            if ( signatureDt != null )
            {
                signatureDt.Rows.Clear();
                signatureDt.Dispose();
            }
            signatureDt = null;

            this.AutoSize = false;
            ribbonControl1.Visible = false;
            if (showRibbon)
                ribbonControl1.Visible = true;
            ribbonControl1.Visible = true;
            if (workFullScreen)
                GoFullscreen(true);
            modified = false;

            if ( workGSContract )
            {
                pdfViewer1.PrintPage += PdfViewer1_PrintPage;
                //printingSystem1.AddCommandHandler(new PrintDocumentCommandHandler());
                ////printingSystem1.AddCommandHandler(new ExportToImageCommandHandler());
            }

            this.Text = workTitle;
            try
            {
                if (filearray != null)
                {
                    MemoryStream stream = new MemoryStream(filearray);
                    pdfViewer1.DetachStreamAfterLoadComplete = true;
                    pdfViewer1.LoadDocument(stream);
                    pdfViewer1.FormFieldValueChanged += PdfViewer1_FormFieldValueChanged;
                    //using (PdfDocumentProcessor documentProcessor = new PdfDocumentProcessor())
                    //{
                    //    documentProcessor.LoadDocument(filePath + fileName + ".pdf");
                    //}

                    pdfFileSaveAsBarItem1.Enabled = false;
                    pdfFileOpenBarItem1.Enabled = false;
                }
                else
                {
                    pdfViewer1.LoadDocument(workFile);
                    pdfViewer1.FormFieldValueChanged += PdfViewer1_FormFieldValueChanged;
                    pdfFileSaveAsBarItem1.Enabled = false;
                    pdfFileOpenBarItem1.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** File not found! " + ex.Message.ToString(), "PDF Open Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            }
            this.BringToFront();

            string cmd = "Select * from `structures` where `form` = '" + workFile + "' order by `order`;";
            fieldDt = G1.get_db_data(cmd);
            if (fieldDt.Rows.Count <= 0)
                btnSign.Hide();
            else
            {
                if (!CheckForSignatures2())
                    btnSign.Hide();
            }
        }
        /***************************************************************************************/
        private void PdfViewer1_PrintPage(object sender, PdfPrintPageEventArgs e)
        {
            fileGotPrinted = true;
        }
        /***************************************************************************************/
        public bool FireEventModified()
        {
            if (modified)
                return true;
            return false;
        }
        /***************************************************************************************/
        private void PdfViewer1_FormFieldValueChanged(object sender, DevExpress.XtraPdfViewer.PdfFormFieldValueChangedEventArgs args)
        {
            try
            {
                modified = true;
            }
            catch ( Exception ex )
            {
            }
        }
        /***************************************************************************************/
        public delegate void d_void_eventdone_gsContract( bool printed );
        public event d_void_eventdone_gsContract GSDone;
        protected void OnGSDone()
        {
            if (GSDone != null)
                GSDone.Invoke(fileGotPrinted);
        }
        /***************************************************************************************/
        public delegate void d_void_eventdone_bytes( string filename, string record, string contractNumber, byte [] b );
        public event d_void_eventdone_bytes PdfDone;
        protected void OnDone( byte [] b)
        {
            if (PdfDone != null)
                PdfDone.Invoke(workFile, workRecord, workContract, b);
        }
        /***************************************************************************************/
        public delegate void d_void_eventlapses( string str );
        public event d_void_eventlapses PdfLapses;
        protected void OnLapsed( string str )
        {
            if (PdfLapses != null)
                PdfLapses.Invoke( str );
        }
        /***********************************************************************************************/
        private void ViewPDF_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (workGSContract)
            {
                if ( gotSignatures )
                {
                    if (G1.RobbyServer)
                    {
                        DialogResult results = MessageBox.Show("(RAG) This is a signed G&S Contract!\nSo, a copy will be saved!", "Signed Contract Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                        if ( results == DialogResult.Yes )
                            SaveToDatabase();
                    }
                    else
                    {
                        MessageBox.Show("This is a signed G&S Contract!\nSo, a copy will be saved!", "Signed Contract Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                        SaveToDatabase();
                    }

                    //string funeralDirectory = G1.GetFuneralFileDirectory ( workContract );
                    fileGotPrinted = true;
                }
                OnGSDone();
                return;
            }

            if (PdfDone == null)
            {
                pdfViewer1.CloseDocument();
                return;
            }

            if (!modified)
            {
                if ( !String.IsNullOrWhiteSpace ( workContract))
                    return;
            }

            DialogResult result = MessageBox.Show("Do you want to save this file into the database?", "Save Data Dialog", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            if (result == DialogResult.No)
                return;
            if ( result == DialogResult.Cancel )
            {
                e.Cancel = true;
                return;
            }
            if (result == DialogResult.Yes)
            {
                MemoryStream ms = new MemoryStream();
                try
                {
                    pdfViewer1.SaveDocument(ms);
                    byte[] b = G1.GetBytesFromStream(ms);
                    OnDone(b);
                }
                catch (Exception ex)
                {
                }
            }
            modified = false;
        }
        /***********************************************************************************************/
        private void pdfViewer1_KeyUp(object sender, KeyEventArgs e)
        {
            modified = true;
        }
        /***********************************************************************************************/
        private void GoFullscreen(bool fullscreen)
        {
            //if (fullscreen)
            //{
            //    this.WindowState = FormWindowState.Normal;
            //    this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            //    this.Bounds = Screen.PrimaryScreen.Bounds;
            //}
            //else
            //{
                this.WindowState = FormWindowState.Maximized;
                this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            //            }
        }

        private void pdfViewer1_FormFieldValueChanged_1(object sender, DevExpress.XtraPdfViewer.PdfFormFieldValueChangedEventArgs args)
        {
            try
            {
                string field = args.FieldName.ObjToString();
                if (field.ToUpper() != "ADD_QTY")
                    return;

                string cmd = "Select * from `options`;";
                DataTable dt = G1.get_db_data(cmd);
                if (dt.Rows.Count <= 0)
                    return;

                DataRow[] dRows = dt.Select("option='Price of First Death Certificate'");
                if (dRows.Length <= 0)
                    return;
                string str = dRows[0]["answer"].ObjToString().ToUpper();
                if (String.IsNullOrWhiteSpace(str))
                    return;
                double firstDC = str.ObjToDouble();

                dRows = dt.Select("option='Price of Additional Death Certificates'");
                if (dRows.Length <= 0)
                    return;
                str = dRows[0]["answer"].ObjToString().ToUpper();
                if (String.IsNullOrWhiteSpace(str))
                    return;
                double addPrice = str.ObjToDouble();

                MemoryStream ms = new MemoryStream();
                pdfViewer1.SaveDocument(ms);
                byte[] b = G1.GetBytesFromStream(ms);

                System.IO.MemoryStream mo = new System.IO.MemoryStream();
                iTextSharp.text.pdf.PdfStamper pdfStamper = null;
                iTextSharp.text.pdf.PdfReader reader = null;

                if ( workOriginal == null )
                    reader = new iTextSharp.text.pdf.PdfReader(b);
                else
                    reader = new iTextSharp.text.pdf.PdfReader(b);
                try
                {
                    pdfStamper = new iTextSharp.text.pdf.PdfStamper(reader, mo);
                }
                catch (Exception ex)
                {
                }

                iTextSharp.text.pdf.AcroFields fields = pdfStamper.AcroFields;

                pdfViewer1.ReadOnly = false;

                    string newValue = args.NewValue.ObjToString();
                    double total = newValue.ObjToDouble() * addPrice;
                    str = G1.ReformatMoney(total);
                    fields.SetField("add_amt", "$" + str );
                    total += firstDC;
                    str = G1.ReformatMoney(total);
                    fields.SetField("tot_amt", "$" + str);
                    total = 1.0D + newValue.ObjToDouble();
                    str = G1.ReformatMoney(total);
                    str = str.Replace(".00", "");
                    fields.SetField("tot_qty", str);

                    pdfStamper.Close();
                    reader.Close();
                    byte [] result = mo.ToArray();

                    MemoryStream stream = new MemoryStream(result);
                    pdfViewer1.DetachStreamAfterLoadComplete = true;
                    pdfViewer1.LoadDocument(stream);
                    //pdfViewer1.Refresh();
            }
            catch ( Exception ex)
            {
            }
        }
        /***********************************************************************************************/
        private void barButtonItem1_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            OnLapsed("YES");
        }
        /***********************************************************************************************/
        //private InkOverlay i_overlay = null;
        private void button1_Click(object sender, EventArgs e)
        {
            //if (tBox.Visible )
            //{
            //    tBox.Hide();
            //    tBox.Refresh();
            //    ReDrawOverlay();
            //}
            //else
            //{
            //    tBox.Show();
            //    tBox.Refresh();
            //}

            try
            {
                MemoryStream ms = new System.IO.MemoryStream();
                pdfViewer1.SaveDocument(ms);
                //Image img = (Image) System.Drawing.Image.FromStream(ms);
                byte[] b = G1.GetBytesFromStream(ms);

                Image img = G1.byteArrayToImage(b);

                if (b != null && b.Length != 0 && img != null )
                {
                    using (ViewImage viewForm = new ViewImage(img))
                    {
                        viewForm.ShowDialog();
                    }
                }
            }
            catch ( Exception ex )
            {
            }
        }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                _draw = true;
                _origin = e.Location;
            }
            else
            {
                _draw = false;
                _origin = Point.Empty;
            }

            _terminus = Point.Empty;
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (_draw && !_origin.IsEmpty && !_terminus.IsEmpty)
                _lines.Add(new Tuple<Point, Point>(_origin, _terminus));
            _draw = false;
            _origin = Point.Empty;
            _terminus = Point.Empty;
            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (e.Button == MouseButtons.Left)
                _terminus = e.Location;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            foreach (var line in _lines)
                e.Graphics.DrawLine(Pens.Blue, line.Item1, line.Item2);
            if (!_origin.IsEmpty && !_terminus.IsEmpty)
                e.Graphics.DrawLine(Pens.Red, _origin, _terminus);
        }

        private void pdfViewer1_MouseDown(object sender, MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                _draw = true;
                _origin = e.Location;
            }
            else
            {
                _draw = false;
                _origin = Point.Empty;
            }

            _terminus = Point.Empty;
            Invalidate();
        }

        private void pdfViewer1_MouseMove(object sender, MouseEventArgs e)
        {
            base.OnMouseMove(e);
            //if (e.Button == MouseButtons.Left)
                _terminus = e.Location;
            //g.DrawLine(Pens.Blue, _origin, _terminus);
//            _lines.Add(new Tuple<Point, Point>(_origin, _terminus));
            _origin = _terminus;
            Invalidate();
        }

        private void pdfViewer1_MouseUp(object sender, MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (_draw && !_origin.IsEmpty && !_terminus.IsEmpty)
                _lines.Add(new Tuple<Point, Point>(_origin, _terminus));
            _draw = false;
            _origin = Point.Empty;
            _terminus = Point.Empty;
            Invalidate();

        }
        /****************************************************************************************/
        private void ReDrawOverlay()
        {
            try
            {
                //if (i_overlay != null)
                //{
                //    foreach (Stroke stroke in i_overlay.Ink.Strokes)
                //        stroke.DrawingAttributes.Width = 30;
                //    i_overlay.DefaultDrawingAttributes.Width = 30;
                //    Graphics g = scribblePanel.CreateGraphics();
                //    i_overlay.Renderer.Draw(g, i_overlay.Ink.Strokes);
                //}
                //tBox.Show();
                //tBox.BringToFront();
                //tBox.Focus();
                //G1.sleep(2000);
                //tBox.Hide();
                //tBox.Refresh();
            }
            catch
            {
            }
        }
        /***********************************************************************************************/
        private void tBox_VScroll(object sender, EventArgs e)
        {
            ReDrawOverlay();
        }
        /***********************************************************************************************/
        private void btnSign_Clickx(object sender, EventArgs e)
        {
            try
            {
                MemoryStream ms = new System.IO.MemoryStream();
                pdfViewer1.SaveDocument(ms);
                //Image img = (Image) System.Drawing.Image.FromStream(ms);
                byte[] b = G1.GetBytesFromStream(ms);

                Image img = G1.byteArrayToImage(b);

                if (b != null && b.Length != 0 && img != null)
                {
                    using (ViewImage viewForm = new ViewImage(b))
                    {
                        viewForm.ShowDialog();
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
        /***********************************************************************************************/
        private void btnSign_Click(object sender, EventArgs e)
        {

            string description = "";
            string signature = WhatSignatures ( ref description );
            //if ( String.IsNullOrWhiteSpace ( signature ))
            //{
            //    MessageBox.Show("*** Sorry*** There are no Signature Fields in this PDF!", "No PDF Signature Fields Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            //    return;
            //}

            if ( signatureDt.Rows.Count <= 0 )
            {
                MessageBox.Show("*** Sorry*** There are no Signature Fields in this PDF!", "No PDF Signature Fields Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }

            if (signature == "QUIT")
                return;

            string field = "";
            for ( int i=0; i<signatureDt.Rows.Count; i++)
            {
                Image sig = new Bitmap(1, 1);
                field = signatureDt.Rows[i]["field"].ObjToString();
                byte[] bytes = signatureDt.Rows[i]["signature"].ObjToBytes();
                if (bytes != null)
                {
                    sig = G1.byteArrayToImage(bytes);
                    AddSignature(field, sig);
                    modified = true;
                }
            }

            //string title = "Enter Signature";
            //if (!String.IsNullOrWhiteSpace(description))
            //    title = description;

            //Image sig = new Bitmap(1, 1);
            //using (SignatureForm signatureForm = new SignatureForm(title, sig))
            //{
            //    if (signatureForm.ShowDialog() == DialogResult.OK)
            //    {
            //        sig = signatureForm.SignatureResult;
                    //AddSignature(signature, sig);
            //        modified = true;
            //    }
            //}
        }
        /***********************************************************************************************/
        private bool CheckForSignatures ()
        {
            iTextSharp.text.pdf.PdfStamper stamper = null;
            iTextSharp.text.pdf.PdfReader reader = null;
            try
            {
                MemoryStream ms = new MemoryStream();
                pdfViewer1.SaveDocument(ms);
                byte[] b = G1.GetBytesFromStream(ms);

                System.IO.MemoryStream mo = new System.IO.MemoryStream();

                reader = new iTextSharp.text.pdf.PdfReader(b);
                stamper = new iTextSharp.text.pdf.PdfStamper(reader, mo);
            }
            catch (Exception ex)
            {
            }

            DataRow[] dR = null;

            string field = "";
            string str = "";
            string desc = "";
            string options = "";
            string[] Lines = null;
            iTextSharp.text.pdf.AcroFields fields = stamper.AcroFields;
            List<KeyValuePair<string, AcroFields.Item>> allFields = fields.Fields.ToList();
            for (int i = 0; i < allFields.Count; i++)
            {
                field = allFields[i].ObjToString();

                Lines = field.Split(',');
                str = Lines[0].Replace("[", "").Trim();
                dR = fieldDt.Select("field='" + str + "'");
                if (dR.Length <= 0)
                    continue;
                options = dR[0]["more_options"].ObjToString();
                if (options.ToUpper().IndexOf("SIGNATURE") >= 0)
                    return true;
            }
            return false;
        }
        /***********************************************************************************************/
        private bool CheckForSignatures2()
        {

            DataRow[] dR = null;
            if (fieldDt == null)
                return false;
            if (fieldDt.Rows.Count <= 0)
                return false;

            dR = fieldDt.Select("more_options='SIGNATURE'");
            if (dR.Length > 0)
                return true;
            return false;
        }
        /***********************************************************************************************/
        private string WhatSignatures ( ref string description )
        {
            string signature = "";
            description = "";
            MemoryStream ms = new MemoryStream();
            pdfViewer1.SaveDocument(ms);
            byte[] b = G1.GetBytesFromStream(ms);

            System.IO.MemoryStream mo = new System.IO.MemoryStream();
            iTextSharp.text.pdf.PdfStamper stamper = null;
            iTextSharp.text.pdf.PdfReader reader = null;

            if (workOriginal == null)
                reader = new iTextSharp.text.pdf.PdfReader(b);
            else
                reader = new iTextSharp.text.pdf.PdfReader(b);
            try
            {
                stamper = new iTextSharp.text.pdf.PdfStamper(reader, mo);
            }
            catch (Exception ex)
            {
            }

            string cmd = "Select * from `relatives` WHERE `contractNumber` = '" + workContract + "' ";
            cmd += " AND `depRelationship` <> 'DISCLOSURES' ";
            cmd += " AND `depRelationship` <> 'CLERGY' ";
            cmd += " AND `depRelationship` <> 'PB' ";
            cmd += " AND `depRelationship` <> 'HPB' ";
            cmd += " AND `depRelationship` <> 'MUSICIAN' ";
            cmd += " AND `depRelationship` <> 'FUNERAL DIRECTOR' ";
            cmd += " AND `depRelationship` <> 'PALLBEARER' ";
            cmd += " ORDER by `order`, `depLastName`,`depFirstName`,`depMI` ";

            DataTable dx = G1.get_db_data(cmd);

            DataTable dt = new DataTable();
            dt.Columns.Add("field");
            dt.Columns.Add("desc");
            dt.Columns.Add("signature", dx.Columns["signature"].DataType);


            DataRow dRow = null;
            DataRow[] dR = null;

            string field = "";
            string str = "";
            string desc = "";
            string options = "";
            string[] Lines = null;
            iTextSharp.text.pdf.AcroFields fields = stamper.AcroFields;
            List <KeyValuePair<string,AcroFields.Item>> allFields = fields.Fields.ToList();
            for ( int i=0; i< allFields.Count; i++)
            {
                field = allFields[i].ObjToString();

                Lines = field.Split(',');
                str = Lines[0].Replace("[", "").Trim();
                dR = fieldDt.Select("field='" + str + "'");
                if (dR.Length <= 0)
                    continue;
                options = dR[0]["more_options"].ObjToString();
                if ( options.ToUpper().IndexOf ( "SIGNATURE") >= 0 )
                {
                    dRow = dt.NewRow();
                    dRow["field"] = str;

                    dR = fieldDt.Select("field='" + str + "'");
                    if (dR.Length > 0)
                    {
                        desc = dR[0]["help"].ObjToString();
                        dRow["desc"] = desc;
                    }

                    dt.Rows.Add(dRow);
                }
            }
            if ( dt.Rows.Count <= 0 )
                return "";

            if (signatureDt == null)
                signatureDt = dt.Copy();

            using (PdfSignatures sigForm = new PdfSignatures(workContract, signatureDt))
            {
                if (sigForm.ShowDialog() == DialogResult.OK)
                {
                    bool QuitAll = sigForm.ExitAll;
                    if (QuitAll)
                    {
                        return "QUIT";
                    }
                    signatureDt = sigForm.SignatureResults;
                }
                else
                    return "QUIT";
            }
            return "";
        }
        /***********************************************************************************************/
        private string WhichSignaturex(ref string description)
        {
            string signature = "";
            description = "";
            MemoryStream ms = new MemoryStream();
            pdfViewer1.SaveDocument(ms);
            byte[] b = G1.GetBytesFromStream(ms);

            System.IO.MemoryStream mo = new System.IO.MemoryStream();
            iTextSharp.text.pdf.PdfStamper stamper = null;
            iTextSharp.text.pdf.PdfReader reader = null;

            if (workOriginal == null)
                reader = new iTextSharp.text.pdf.PdfReader(b);
            else
                reader = new iTextSharp.text.pdf.PdfReader(b);
            try
            {
                stamper = new iTextSharp.text.pdf.PdfStamper(reader, mo);
            }
            catch (Exception ex)
            {
            }

            string cmd = "Select * from `relatives` WHERE `contractNumber` = '" + workContract + "' ";
            cmd += " AND `depRelationship` <> 'DISCLOSURES' ";
            cmd += " AND `depRelationship` <> 'CLERGY' ";
            cmd += " AND `depRelationship` <> 'PB' ";
            cmd += " AND `depRelationship` <> 'HPB' ";
            cmd += " AND `depRelationship` <> 'MUSICIAN' ";
            cmd += " AND `depRelationship` <> 'FUNERAL DIRECTOR' ";
            cmd += " AND `depRelationship` <> 'PALLBEARER' ";
            cmd += " ORDER by `order`, `depLastName`,`depFirstName`,`depMI` ";

            DataTable dx = G1.get_db_data(cmd);

            DataTable dt = new DataTable();
            dt.Columns.Add("field");
            dt.Columns.Add("desc");
            dt.Columns.Add("signature", dx.Columns["signature"].DataType);


            DataRow dRow = null;
            DataRow[] dR = null;

            string field = "";
            string str = "";
            string desc = "";
            string options = "";
            string[] Lines = null;
            iTextSharp.text.pdf.AcroFields fields = stamper.AcroFields;
            List<KeyValuePair<string, AcroFields.Item>> allFields = fields.Fields.ToList();
            for (int i = 0; i < allFields.Count; i++)
            {
                field = allFields[i].ObjToString();

                Lines = field.Split(',');
                str = Lines[0].Replace("[", "").Trim();
                dR = fieldDt.Select("field='" + str + "'");
                if (dR.Length <= 0)
                    continue;
                options = dR[0]["more_options"].ObjToString();
                if (options.ToUpper().IndexOf("SIGNATURE") >= 0)
                {
                    dRow = dt.NewRow();
                    dRow["field"] = str;

                    dR = fieldDt.Select("field='" + str + "'");
                    if (dR.Length > 0)
                    {
                        desc = dR[0]["help"].ObjToString();
                        dRow["desc"] = desc;
                    }

                    dt.Rows.Add(dRow);
                }
            }
            if (dt.Rows.Count <= 0)
                return "";

            if (dt.Rows.Count == 1)
            {
                signature = dt.Rows[0]["field"].ObjToString();
                description = dt.Rows[0]["desc"].ObjToString();
                return signature;
            }

            string lines = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                desc = dt.Rows[i]["desc"].ObjToString();
                if (!String.IsNullOrWhiteSpace(desc))
                    lines += desc;
                else
                    lines += dt.Rows[i]["field"].ObjToString();
                lines += "\n";
            }

            using (SelectFromList listForm = new SelectFromList(lines, false))
            {
                listForm.Text = "Choose only one of these options!";
                listForm.ShowDialog();
                signature = SelectFromList.theseSelections;
                if (String.IsNullOrWhiteSpace(signature))
                    return "";
                dR = dt.Select("desc='" + signature + "'");
                if (dR.Length > 0)
                {
                    description = signature;
                    signature = dR[0]["field"].ObjToString();
                }
            }

            return signature;
        }
        /***********************************************************************************************/
        private void AddSignature( string fieldName, Image image )
        {
            MemoryStream ms = new MemoryStream();
            pdfViewer1.SaveDocument(ms);
            byte[] b = G1.GetBytesFromStream(ms);

            System.IO.MemoryStream mo = new System.IO.MemoryStream();
            iTextSharp.text.pdf.PdfStamper stamper = null;
            iTextSharp.text.pdf.PdfReader reader = null;

            if (workOriginal == null)
                reader = new iTextSharp.text.pdf.PdfReader(b);
            else
                reader = new iTextSharp.text.pdf.PdfReader(b);
            try
            {
                stamper = new iTextSharp.text.pdf.PdfStamper(reader, mo);
            }
            catch (Exception ex)
            {
            }

            iTextSharp.text.pdf.AcroFields fields = stamper.AcroFields;

            pdfViewer1.ReadOnly = false;

            AcroFields.FieldPosition fieldPosition = stamper.AcroFields.GetFieldPositions(fieldName)[0];

            PushbuttonField imageField = new PushbuttonField(stamper.Writer, fieldPosition.position, fieldName);
            imageField.Layout = PushbuttonField.LAYOUT_ICON_ONLY;
            imageField.Image = iTextSharp.text.Image.GetInstance ( (System.Drawing.Image) image, iTextSharp.text.BaseColor.WHITE );
            imageField.ScaleIcon = PushbuttonField.SCALE_ICON_ALWAYS;
            imageField.ProportionalIcon = false;
            imageField.Options = BaseField.READ_ONLY;

            stamper.AcroFields.RemoveField(fieldName);
            stamper.AddAnnotation(imageField.Field, fieldPosition.page);

            stamper.Close();

            stamper.Close();
            reader.Close();
            byte[] result = mo.ToArray();

            MemoryStream stream = new MemoryStream(result);
            pdfViewer1.DetachStreamAfterLoadComplete = true;
            pdfViewer1.LoadDocument(stream);
        }
        /***********************************************************************************************/
        void ConvertTextFieldToImage(string inputFile, string fieldName, string imageFile, string outputFile)
        {
            using (PdfStamper stamper = new PdfStamper(new PdfReader(inputFile), File.Create(outputFile)))
            {
                AcroFields.FieldPosition fieldPosition = stamper.AcroFields.GetFieldPositions(fieldName)[0];

                PushbuttonField imageField = new PushbuttonField(stamper.Writer, fieldPosition.position, fieldName);
                imageField.Layout = PushbuttonField.LAYOUT_ICON_ONLY;
                imageField.Image = iTextSharp.text.Image.GetInstance(imageFile);
                imageField.ScaleIcon = PushbuttonField.SCALE_ICON_ALWAYS;
                imageField.ProportionalIcon = false;
                imageField.Options = BaseField.READ_ONLY;

                stamper.AcroFields.RemoveField(fieldName);
                stamper.AddAnnotation(imageField.Field, fieldPosition.page);

                stamper.Close();
            }
        }
        /***********************************************************************************************/
        private void pdfFilePrintBarItem1_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (!workGSContract)
                return;
            if (MessageBox.Show("Contract Is Being Printed!!\nDo you want to save this as a permanent copy of this Contract in the customers file?", "Contract Printed Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly) == DialogResult.Yes)
            {
                SaveToDatabase();
            }
        }
        /***********************************************************************************************/
        private void pdfFileSaveAsBarItem1_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (!workGSContract)
                return;
            if (MessageBox.Show("Contract Is Being Saved!!\nDo you want to save this as a permanent copy of this Contract in the customers file?", "Contract Saved Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly) == DialogResult.Yes)
            {
                SaveToDatabase();
            }
        }
        /***********************************************************************************************/
        private void SaveToDatabase ()
        {
            string contract = workContract;
            DateTime today = DateTime.Now;
            using (StreamReader sr = new StreamReader(workFile))
            {
                byte[] result;
                using (MemoryStream ms = new MemoryStream())
                {
                    sr.BaseStream.CopyTo(ms);
                    result = ms.ToArray();
                }
                string sDate = today.ToString("MM/dd/yyyy");
                string record = "";
                string noticeRecord = Contract1.SavePdfToDatabase(result, "trust");
                if (!String.IsNullOrWhiteSpace(noticeRecord))
                {
                    if (!String.IsNullOrWhiteSpace(contract))
                    {
                        record = G1.create_record("lapse_list", "type", "-1");
                        if (!G1.BadRecord("lapse_list", record))
                        {
                            fileGotPrinted = true;
                            G1.update_db_table("lapse_list", "record", record, new string[] { "contractNumber", contract, "noticeDate", sDate, "type", "trust", "noticeRecord", noticeRecord, "detail", "Goods and Services" });
                            string preference = G1.getPreference(LoginForm.username, "Backup GS Contracts", "Allow Access", false);
                            if (preference.ToUpper() == "YES")
                            {
                                string funeralDirectory = G1.GetFuneralFileDirectory(workContract);
                                if (!String.IsNullOrWhiteSpace(funeralDirectory))
                                {
                                    string path = G1.DecodeFilename(workFile, true);
                                    int idx = 0;
                                    string filename = "";
                                    string fullFilename = "";
                                    for (; ; )
                                    {
                                        if (idx >= 20)
                                            break;
                                        filename = "GS_" + idx.ToString() + ".pdf";
                                        fullFilename = funeralDirectory + "/" + filename;
                                        if (!File.Exists(fullFilename))
                                        {
                                            File.Copy(workFile, fullFilename);
                                            break;
                                        }
                                        idx++;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        /***********************************************************************************************/
    }
}