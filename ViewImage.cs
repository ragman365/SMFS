using System;
using System.Data;
using System.Windows.Forms;
using GeneralLib;

using System.Drawing;
using DevExpress.XtraPrinting;
using DevExpress.Utils;
using DevExpress.Pdf;
using MySql.Data.MySqlClient;
using System.Text;
using System.IO;
using DevExpress.XtraRichEdit;
using System.Drawing.Printing;
using EMRControlLib;
using System.Collections.Generic;
using Microsoft.Ink;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class ViewImage : DevExpress.XtraEditors.XtraForm
    {
        private string rtfText = "";
        private string workRecord = "";
        private Image workImage = null;
        private byte[] workBytes = null;
        private bool loading = true;

        private Point _origin = Point.Empty;
        private Point _terminus = Point.Empty;
        private Boolean _draw = false;
        private List<Tuple<Point, Point>> _lines = new List<Tuple<Point, Point>>();

        /***********************************************************************************************/
        public ViewImage( Image image )
        {
            InitializeComponent();
            workImage = image;
        }
        public ViewImage( byte [] b)
        {
            InitializeComponent();
            workBytes = b;
        }
        /***********************************************************************************************/
        public ViewImage( string text )
        {
            InitializeComponent();
            rtfText = text;
        }
        /***********************************************************************************************/
        public ViewImage(string record, string text)
        {
            InitializeComponent();
            rtfText = text;
            workRecord = record;
        }
        /***********************************************************************************************/
        Microsoft.Ink.InkOverlay i_overlay;
        private void ViewImage_Load(object sender, EventArgs e)
        {
            //rtb.RichTextBox.AppendRtf(rtfText);
            //pictureBox1.Image = workImage;
            //pictureBox1.BringToFront();
            //rtb.RichTextBox.InsertImage(workImage);
            //rtb.RichTextBox.Modified = false;
            //rtb.RichTextBox.ScrollToCaret();
            //rtb.Hide();
            //rtb.Refresh();

            //pictureBox1.Show();
            //pictureBox1.Refresh();

            //TransparentRichTextBox tBox = new TransparentRichTextBox();
            //tBox.Dock = DockStyle.Fill;
            i_overlay = new Microsoft.Ink.InkOverlay(tBox);
            i_overlay.Enabled = true;
            i_overlay.AutoRedraw = true;
            i_overlay.MouseDown += I_overlay_MouseDown;
            i_overlay.MouseMove += I_overlay_MouseMove;
            i_overlay.MouseUp += I_overlay_MouseUp;

            if (workBytes.Length > 0)
            {
                rtb.Hide();
                pictureBox1.Hide();
                MemoryStream stream = new MemoryStream(workBytes);
                pdfViewer1.DetachStreamAfterLoadComplete = true;
                pdfViewer1.LoadDocument(stream);
                pdfViewer1.Dock = DockStyle.Fill;

                int top = panelAll.Top;
                int left = panelAll.Left;
                int width = pdfViewer1.Width + 300;
                int height = pdfViewer1.Height;
                height = this.Height;
                height = height * 2 + 500;
                tBox.SetBounds(left, top, width, height);
                //pdfViewer1.Controls.Add(tBox);

                pdfViewer1.Enabled = false;
                tBox.Enabled = true;
                panelAll.VerticalScroll.Enabled = true;
                this.Cursor = System.Windows.Forms.Cursors.Cross;

                panelAll.Scroll += PanelAll_Scroll;
                pdfViewer1.Scroll += PdfViewer1_Scroll;
                tBox.VScroll += TBox_VScroll;
                rtb.Scroll += Rtb_Scroll;

                panelAll.VerticalScroll.Value = 0; // Force to top of panel

                GoFullscreen();
            }

            tBox.Show();
            loading = false;

            DrawSomething();
        }

        private void Rtb_Scroll(object sender, ScrollEventArgs e)
        {
            if (loading)
                return;
            ReDrawOverlay();
        }

        private void TBox_VScroll(object sender, EventArgs e)
        {
            if (loading)
                return;
            ReDrawOverlay();
        }

        private void PdfViewer1_Scroll(object sender, ScrollEventArgs e)
        {
            if (loading)
                return;
            ReDrawOverlay();
        }

        private void PanelAll_Scroll(object sender, DevExpress.XtraEditors.XtraScrollEventArgs e)
        {
            if (loading)
                return;
            ReDrawOverlay();
        }
        /***********************************************************************************************/
        private void GoFullscreen()
        {
            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
        }
        /***********************************************************************************************/
        private void BuildPrintReport(bool clear = true)
        {
            if (clear)
                rtbPrint.RichTextBox.Clear();
            rtbPrint.RichTextBox.AppendRtf(rtb.RichTextBox.Rtf);
        }
        /***********************************************************************************************/
        private EMRRichTextBox rtbPrint = new EMRRichTextBox();
        private int checkPrint;
        private int pageNumber;
        /***********************************************************************************************/
        private void printPreviewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //BuildPrintReport();
            //printPreviewDialog1.BringToFront();
            //printPreviewDialog1.TopLevel = true;
            //printPreviewDialog1.SetBounds(0, 500, 500, 500);
            //printPreviewDialog1.ShowDialog();

            pdfViewer1.Print();
        }
        /***********************************************************************************************/
        private void printDocument1_BeginPrint(object sender, PrintEventArgs e)
        {
            pageNumber = 1;
            checkPrint = 0;
        }
        /***********************************************************************************************/
        private void printDocument1_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            e.PageSettings.Margins.Bottom = 10;
            checkPrint = rtbPrint.RichTextBox.Print(checkPrint, rtbPrint.RichTextBox.TextLength, e);

            string page = "Page - " + pageNumber.ToString();

            int x = (e.PageBounds.Width / 2) - 30;
            Point drawPoint = new Point(x, e.PageBounds.Height - 55);
            var topFont = new Font("Times New Roman", 12);
            e.Graphics.DrawString(page, topFont, Brushes.Black, drawPoint);

            if (checkPrint < rtbPrint.RichTextBox.TextLength)
                e.HasMorePages = true;
            else
                e.HasMorePages = false;
            pageNumber++;
        }
        /***********************************************************************************************/
        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BuildPrintReport();
            using (PrintDialog pd = new PrintDialog())
            {
                pd.Document = printDocument1;
                if (pd.ShowDialog() == DialogResult.OK)
                    printDocument1.Print();
            }
        }
        /***************************************************************************************/
        public delegate void d_void_eventdone_bytes(string record, string rtfText);
        public event d_void_eventdone_bytes RtfDone;
        protected void OnDone()
        {
            if (RtfDone != null)
            {
                RtfDone.Invoke(workRecord, this.rtb.RichTextBox.Rtf);
            }
        }
        /***********************************************************************************************/
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        /***********************************************************************************************/
        private void ViewRTF_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (rtb.RichTextBox.Modified)
                OnDone();
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

        }

        private void tBox_MouseDown(object sender, MouseEventArgs e)
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

        private void tBox_MouseMove(object sender, MouseEventArgs e)
        {
            base.OnMouseMove(e);
            //if (e.Button == MouseButtons.Left)
            _terminus = e.Location;
            //g.DrawLine(Pens.Blue, _origin, _terminus);
            //            _lines.Add(new Tuple<Point, Point>(_origin, _terminus));
            _origin = _terminus;
            Invalidate();
        }

        private void tBox_MouseUp(object sender, MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (_draw && !_origin.IsEmpty && !_terminus.IsEmpty)
                _lines.Add(new Tuple<Point, Point>(_origin, _terminus));
            _draw = false;
            _origin = Point.Empty;
            _terminus = Point.Empty;
            Invalidate();
        }

        private void I_overlay_MouseUp(object sender, Microsoft.Ink.CancelMouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (_draw && !_origin.IsEmpty && !_terminus.IsEmpty)
                _lines.Add(new Tuple<Point, Point>(_origin, _terminus));
            _draw = false;
            _origin = Point.Empty;
            _terminus = Point.Empty;
            Invalidate();
        }

        private void I_overlay_MouseMove(object sender, Microsoft.Ink.CancelMouseEventArgs e)
        {
            base.OnMouseMove(e);
            //if (e.Button == MouseButtons.Left)
            _terminus = e.Location;
            //g.DrawLine(Pens.Blue, _origin, _terminus);
            //            _lines.Add(new Tuple<Point, Point>(_origin, _terminus));
            _origin = _terminus;
            Invalidate();
        }

        private void I_overlay_MouseDown(object sender, Microsoft.Ink.CancelMouseEventArgs e)
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
        /****************************************************************************************/
        private void ReDrawOverlay()
        {
            try
            {
                if (i_overlay != null)
                {
                    foreach (Stroke stroke in i_overlay.Ink.Strokes)
                        stroke.DrawingAttributes.Width = 30;
                    i_overlay.DefaultDrawingAttributes.Width = 30;
                    Graphics g = tBox.CreateGraphics();
                    i_overlay.Renderer.Draw(g, i_overlay.Ink.Strokes);
                }
                tBox.BringToFront();
                tBox.Focus();
            }
            catch
            {
            }
        }
        /***********************************************************************************************/
        private void DrawSomething()
        {
            //using (PdfDocumentProcessor processor = new PdfDocumentProcessor())
            //{
            //    processor.CreateEmptyDocument();
            //    PdfPage page = processor.AddNewPage(PdfPaperSize.A4);
            //    using (PdfGraphics graphics = processor.CreateGraphics())
            //    {
            //        // Draw a line.
            //        using (var pen = new Pen(Color.Red, 5))
            //            graphics.DrawLine(pen, 100, 100, 500, 700);

            //        // Add graphics content to the document page.
            //        graphics.AddToPageForeground(page, 72, 72);
            //    }
            //}

            Graphics g = pdfViewer1.CreateGraphics();
            using (var pen = new Pen(Color.Red, 5))
                g.DrawLine(pen, 100, 100, 500, 700);
        }
        /***********************************************************************************************/
    }
}