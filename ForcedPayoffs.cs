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
using DevExpress.XtraPrintingLinks;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraEditors.ViewInfo;
using DevExpress.Utils.Drawing;
using DevExpress.XtraGrid.Columns;
using iTextSharp.text.pdf;
using System.IO;
using System.Text.RegularExpressions;
using System.Security.Principal;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class ForcedPayoffs : DevExpress.XtraEditors.XtraForm
    {
        DataTable workDt = null;
        DataTable originalDt = null;
        private string workRecord = "";
        private bool workPDF = false;
        private DateTime workStart = DateTime.Now;
        private DateTime workStop = DateTime.Now;
        /***********************************************************************************************/
        public ForcedPayoffs( DataTable dt )
        {
            InitializeComponent();
            workDt = dt;
        }
        /***********************************************************************************************/
        public ForcedPayoffs( bool pdf, string record, DateTime start, DateTime stop )
        {
            InitializeComponent();
            workPDF = pdf;
            workRecord = record;
            workStart = start;
            workStop = stop;
            ForcedPayoffs_Load(null, null);
        }
        /***********************************************************************************************/
        public ForcedPayoffs()
        {
            InitializeComponent();
        }
        /***********************************************************************************************/
        private void ForcedPayoffs_Load(object sender, EventArgs e)
        {
            dgv3.Hide();
            dgv2.Hide();
            dgv.Dock = DockStyle.Fill;
            btnGenerate.Hide();
            dgv.Dock = DockStyle.Fill;

            if (workDt != null)
            {
                G1.NumberDataTable(workDt);
                dgv.DataSource = workDt;
                panelTop.Hide();
            }
            else
            {
                DateTime now = DateTime.Now;
                DateTime start = new DateTime(now.Year, now.Month, 1);
                this.dateTimePicker1.Value = start;
                int days = DateTime.DaysInMonth(now.Year, now.Month);
                DateTime stop = new DateTime(now.Year, now.Month, days);
                this.dateTimePicker2.Value = stop;
            }

            if ( workPDF )
            {
                this.dateTimePicker1.Value = workStart;
                this.dateTimePicker2.Value = workStop;
            }
            SetupTotalsSummary();

            if ( workPDF )
            {
                btnRefresh_Click(null, null);
                this.Close();
            }
            barImport.Hide();
        }
        /****************************************************************************************/
        private void SetupTotalsSummary()
        {
            AddSummaryColumn("difference", null);
            AddSummaryColumn("difference2", null);
            AddSummaryColumn("interestPaid", null);
            AddSummaryColumn("retained", null);
            AddSummaryColumn("trust85P", null);
            AddSummaryColumn("trust100P", null);
        }
        /****************************************************************************************/
        private void AddSummaryColumn(string columnName, string format = "")
        {
            if (String.IsNullOrWhiteSpace(format))
                format = "${0:0,0.00}";
            gridMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            gridMain.Columns[columnName].SummaryItem.DisplayFormat = format;
        }
        /***********************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string contract = dr["contractNumber"].ObjToString();
            if (String.IsNullOrWhiteSpace(contract))
                return;
            this.Cursor = Cursors.WaitCursor;
            CustomerDetails clientForm = new CustomerDetails(contract);
            clientForm.Show();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void setDueDateTo12312039ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string contract = dr["contractNumber"].ObjToString();
            if (String.IsNullOrWhiteSpace(contract))
                return;
            string cmd = "Select * from `contracts` where `contractNumber` = '" + contract + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return;
            string record = dt.Rows[0]["record"].ObjToString();
            DateTime date = new DateTime(2039, 12, 31);
            G1.update_db_table("contracts", "record", record, new string[] {"dueDate8", date.ToString("yyyy-MM-dd") });
        }
        /***********************************************************************************************/
        private int pageMarginLeft = 0;
        private int pageMarginRight = 0;
        private int pageMarginTop = 0;
        private int pageMarginBottom = 0;
        private bool isPrinting = false;
        /***********************************************************************************************/
        private void printPreviewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            isPrinting = true;
            if (this.components == null)
                this.components = new System.ComponentModel.Container();

            DevExpress.XtraPrinting.PrintingSystem printingSystem1 = new DevExpress.XtraPrinting.PrintingSystem(this.components);
            DevExpress.XtraPrinting.PrintableComponentLink printableComponentLink1 = new DevExpress.XtraPrinting.PrintableComponentLink(this.components);

            printingSystem1.Links.AddRange(new object[] {
            printableComponentLink1});


            printableComponentLink1.Component = dgv;
            printableComponentLink1.PrintingSystemBase = printingSystem1;

            //printableComponentLink1.EnablePageDialog = true;

            printableComponentLink1.CreateDetailHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateDetailHeaderArea);
            printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);
            printableComponentLink1.BeforeCreateAreas += new System.EventHandler(this.printableComponentLink1_BeforeCreateAreas);
            printableComponentLink1.AfterCreateAreas += new System.EventHandler(this.printableComponentLink1_AfterCreateAreas);
            printableComponentLink1.Landscape = true;

            Printer.setupPrinterMargins(30, 30, 80, 50);

            pageMarginLeft = Printer.pageMarginLeft;
            pageMarginRight = Printer.pageMarginRight;
            pageMarginTop = Printer.pageMarginTop;
            pageMarginBottom = Printer.pageMarginBottom;

            printableComponentLink1.Margins.Left = pageMarginLeft;
            printableComponentLink1.Margins.Right = pageMarginRight;
            printableComponentLink1.Margins.Top = pageMarginTop;
            printableComponentLink1.Margins.Bottom = pageMarginBottom;

            printingSystem1.Document.AutoFitToPagesWidth = 1;

            G1.AdjustColumnWidths(gridMain, 0.65D, true);

            printableComponentLink1.CreateDocument();
            if ( workPDF )
            {
                string filename = @"c:/rag/ForcePayoff.pdf";
                GrantAccess ( @"c:/rag" );
                if (File.Exists(filename))
                {
                    GrantFileAccess(filename);
                    File.SetAttributes(filename, FileAttributes.Normal);
                    File.Delete(filename);
                }
                printableComponentLink1.ExportToPdf(filename);
            }
            else
                printableComponentLink1.ShowPreview();

            G1.AdjustColumnWidths(gridMain, 0.65D, false );

            isPrinting = false;
            gridMain.OptionsPrint.PrintHeader = true;
        }
        /***********************************************************************************************/
        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            isPrinting = true;
            if (this.components == null)
                this.components = new System.ComponentModel.Container();
            DevExpress.XtraPrinting.PrintingSystem printingSystem1 = new DevExpress.XtraPrinting.PrintingSystem(this.components);
            DevExpress.XtraPrinting.PrintableComponentLink printableComponentLink1 = new DevExpress.XtraPrinting.PrintableComponentLink(this.components);

            printingSystem1.Links.AddRange(new object[] {
            printableComponentLink1});

            printableComponentLink1.Component = dgv;
            printableComponentLink1.PrintingSystemBase = printingSystem1;

            printableComponentLink1.CreateDetailHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateDetailHeaderArea);
            printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);
            printableComponentLink1.BeforeCreateAreas += new System.EventHandler(this.printableComponentLink1_BeforeCreateAreas);
            printableComponentLink1.AfterCreateAreas += new System.EventHandler(this.printableComponentLink1_AfterCreateAreas);
            printableComponentLink1.Landscape = true;

            Printer.setupPrinterMargins(30, 30, 80, 50);

            pageMarginLeft = Printer.pageMarginLeft;
            pageMarginRight = Printer.pageMarginRight;
            pageMarginTop = Printer.pageMarginTop;
            pageMarginBottom = Printer.pageMarginBottom;

            printableComponentLink1.Margins.Left = pageMarginLeft;
            printableComponentLink1.Margins.Right = pageMarginRight;
            printableComponentLink1.Margins.Top = pageMarginTop;
            printableComponentLink1.Margins.Bottom = pageMarginBottom;

            printingSystem1.Document.AutoFitToPagesWidth = 1;

            printableComponentLink1.CreateDocument();
            printableComponentLink1.PrintDlg();
            isPrinting = false;
            gridMain.OptionsPrint.PrintHeader = true;
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
            Printer.setupPrinterQuads(e, 2, 3);
            Font font = new Font("Ariel", 16, FontStyle.Regular);
            Printer.DrawQuad(1, 1, Printer.xQuads, 2, "South Mississippi Funeral Services, LLC", Color.Black, BorderSide.Top, font, HorizontalAlignment.Center);

            Printer.SetQuadSize(12, 12);

            font = new Font("Ariel", 8, FontStyle.Regular);
            Printer.DrawGridDate(2, 3, 2, 3, Color.Black, BorderSide.None, font);
            Printer.DrawGridPage(11, 3, 2, 3, Color.Black, BorderSide.None, font);

            Printer.DrawQuad(1, 9, 2, 3, "User : " + LoginForm.username, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);


            font = new Font("Ariel", 10, FontStyle.Regular);
            Printer.DrawQuad(6, 8, 4, 4, "Trust Credit Adjustment Report", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            DateTime date = DateTime.Now;
            string workDate = date.Month.ToString("D2") + "/" + date.Day.ToString("D2") + "/" + (date.Year % 100).ToString("D2");

            string str = "Report : " + workDate;

            Printer.SetQuadSize(24, 12);
            font = new Font("Ariel", 9, FontStyle.Regular);
            //Printer.DrawQuad(19, 8, 5, 4, str, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            string lock1 = this.dateTimePicker1.Value.ToString("MM/dd/yyyy");
            string lock2 = this.dateTimePicker2.Value.ToString("MM/dd/yyyy");
            Printer.DrawQuad(22, 8, 5, 4, "Stop " + lock2, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            Printer.DrawQuad(22, 5, 5, 4, "Start " + lock1, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);


            //Printer.DrawQuad(16, 8, 3, 4, lblPayment.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(19, 8, 3, 4, lblTrust85.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(22, 8, 3, 4, lblTrust100.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /***********************************************************************************************/
        private void btnLeft_Click(object sender, EventArgs e)
        {
            DateTime start = this.dateTimePicker1.Value;
            DateTime now = start.AddMonths(-1);
            start = new DateTime(now.Year, now.Month, 1);
            this.dateTimePicker1.Value = start;
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            DateTime stop = new DateTime(now.Year, now.Month, days);
            this.dateTimePicker2.Value = stop;
        }
        /***********************************************************************************************/
        private void btnRight_Click(object sender, EventArgs e)
        {
            DateTime start = this.dateTimePicker1.Value;
            DateTime now = start.AddMonths(1);
            start = new DateTime(now.Year, now.Month, 1);
            this.dateTimePicker1.Value = start;
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            DateTime stop = new DateTime(now.Year, now.Month, days);
            this.dateTimePicker2.Value = stop;
        }
        /***********************************************************************************************/
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            DateTime start = this.dateTimePicker1.Value;
            DateTime stop = this.dateTimePicker2.Value;

            //string cmd = "Select * from `forced_paidoff` f JOIN `contracts` c ON f.`contractNumber` = c.`contractNumber` JOIN `customers` p ON f.`contractNumber` = p.`contractNumber` ";
            //cmd += "RIGHT JOIN `payments` x ON f.`contractNumber` = x.`contractNumber` ";
            //cmd += " WHERE `datePaid` >= '" + start.ToString("yyyy-MM-dd") + "' AND `datePaid` <= '" + stop.ToString("yyyy-MM-dd") + "' ";
            //cmd += " AND x.`edited` = 'TrustAdj'";
            //cmd += ";";

            string cmd = "Select * from `payments` f JOIN `contracts` c ON f.`contractNumber` = c.`contractNumber` JOIN `customers` p ON f.`contractNumber` = p.`contractNumber` ";
            //cmd += "JOIN `forced_paidoff` x ON f.`contractNumber` = x.`contractNumber` ";
            cmd += " WHERE `payDate8` >= '" + start.ToString("yyyy-MM-dd") + "' AND `payDate8` <= '" + stop.ToString("yyyy-MM-dd") + "' ";
            cmd += " AND `edited` = 'TrustAdj' AND `creditReason` = 'TCA' ";
            cmd += " ORDER BY f.`record` ";
            cmd += ";";

            if ( workPDF )
            {
                cmd = "Select * from `payments` f JOIN `contracts` c ON f.`contractNumber` = c.`contractNumber` JOIN `customers` p ON f.`contractNumber` = p.`contractNumber` ";
                cmd += "LEFT JOIN `forced_paidoff` x ON f.`contractNumber` = x.`contractNumber` ";
                cmd += " WHERE f.`record` = '" + workRecord + "' ";
                //cmd += " AND `edited` = 'TrustAdj' AND `creditReason` = 'TCA' ";
                cmd += " ORDER BY f.`record` ";
                cmd += ";";
            }



            DataTable dt = G1.get_db_data(cmd);

            FixRetained(dt);

            G1.NumberDataTable(dt);
            dgv.DataSource = dt;

            originalDt = dt;

            if ( workPDF )
            {
                printPreviewToolStripMenuItem_Click(null, null);
                return;
            }

            btnGenerate.Show();
        }
        /****************************************************************************************/
        private void SetupColumnView()
        {
            G1.SetColumnPosition(gridMain, "num", 1);
            G1.SetColumnPosition(gridMain, "contractNumber", 2);
            G1.SetColumnPosition(gridMain, "lastName", 3);
            G1.SetColumnPosition(gridMain, "firstName", 4);
            G1.SetColumnPosition(gridMain, "datePaid", 5);
            G1.SetColumnPosition(gridMain, "balanceDue", 6);
            G1.SetColumnPosition(gridMain, "contractValue", 7);
            G1.SetColumnPosition(gridMain, "maxTrust85", 8);
            G1.SetColumnPosition(gridMain, "totalTrust85", 9);
            G1.SetColumnPosition(gridMain, "difference", 10);
            G1.SetColumnPosition(gridMain, "difference2", 11);
        }
        /***********************************************************************************************/
        private void FixRetained ( DataTable dt)
        {
            if ( G1.get_column_number ( dt, "difference2") < 0 )
                dt.Columns.Add("difference2", Type.GetType("System.Double"));

            double difference = 0D;

            //for ( int i=0; i<dt.Rows.Count; i++)
            //{
            //    difference = dt.Rows[i]["difference"].ObjToDouble();
            //    if (difference < 0D)
            //    {
            //        dt.Rows[i]["difference"] = Math.Abs(difference);
            //        difference = difference / 0.85D;
            //        difference = G1.RoundValue(difference);
            //        dt.Rows[i]["difference"] = Math.Abs(difference);
            //        dt.Rows[i]["difference2"] = 0D;
            //    }
            //    else
            //    {
            //        difference = difference / 0.85D;
            //        difference = G1.RoundValue(difference);
            //        dt.Rows[i]["difference2"] = difference;
            //        dt.Rows[i]["difference"] = 0D;
            //    }
            //}
        }
        /***********************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            SetSpyGlass(gridMain);
        }
        /***********************************************************************************************/
        private void SetSpyGlass(DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView grid)
        {
            G1.ShowHideFindPanel(grid);
        }
        /***********************************************************************************************/
        private bool GeneratePages = false;
        private void btnGenerate_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt == null)
                return;
            if (dt.Rows.Count <= 0)
                return;

            int[] rows = gridMain.GetSelectedRows();

            int firstRow = 0;
            int lastRow = dt.Rows.Count;
            if (rows.Length <= 0)
                return;
            int row = 0;
            DataTable dx = dt.Clone();
            DataRow dRow = null;
            for (int i = 0; i < rows.Length; i++)
            {
                row = rows[i];
                if (row < 0)
                    continue;
                firstRow = gridMain.GetDataSourceRowIndex(row);
                dRow = dt.Rows[firstRow];
                dx.ImportRow(dRow);
            }

            this.Cursor = Cursors.WaitCursor;

            barImport.Show();
            barImport.Minimum = 0;
            barImport.Maximum = dx.Rows.Count;
            barImport.Refresh();

            iTextSharp.text.Document sourceDocument = null;
            PdfCopy pdfCopyProvider = null;
            PdfImportedPage importedPage;


            string outputPdfPath = @"C:/rag/pdfX.pdf";
            string username = LoginForm.username.Trim();
            if ( !String.IsNullOrWhiteSpace ( username))
                outputPdfPath = @"C:/rag/" + username + "_pdfX.pdf";

            GrantAccess(@"C:/rag" );
            GrantFileAccess(outputPdfPath);

            if (File.Exists(outputPdfPath))
            {
                File.SetAttributes(outputPdfPath, FileAttributes.Normal);
                File.Delete(outputPdfPath);
            }

            sourceDocument = new iTextSharp.text.Document();
            pdfCopyProvider = new PdfCopy(sourceDocument, new System.IO.FileStream(outputPdfPath, System.IO.FileMode.Create));

            //output file Open  
            sourceDocument.Open();

            string contract = "";
            string record = "";
            for ( int i=0; i<dx.Rows.Count; i++)
            {
                barImport.Value = i + 1;
                barImport.Refresh();
                contract = dx.Rows[i]["contractNumber"].ObjToString();
                if (String.IsNullOrWhiteSpace(contract))
                    return;

                record = dx.Rows[i]["record"].ObjToString();
                if (String.IsNullOrWhiteSpace(record))
                    return;


                string historyFile = @"C:/rag/pdfDaily.pdf";
                GrantFileAccess(historyFile);
                DailyHistory histForm = new DailyHistory(contract, historyFile, true);

                string manualFile = @"c:/rag/Manual.pdf";
                string payOffFile = @"c:/rag/ForcePayoff.pdf";

                GrantFileAccess(manualFile);
                GrantFileAccess(payOffFile);


                ForcedPayoffs forceForm = new ForcedPayoffs(true, record, this.dateTimePicker1.Value, this.dateTimePicker2.Value);

                MergeAllPDF(pdfCopyProvider, payOffFile, manualFile, historyFile );

                if (File.Exists(payOffFile))
                {
                    File.SetAttributes(payOffFile, FileAttributes.Normal);
                    File.Delete(payOffFile);
                }

                if (File.Exists(historyFile))
                {
                    File.SetAttributes(historyFile, FileAttributes.Normal);
                    File.Delete(historyFile);
                }

                if (File.Exists(manualFile))
                {
                    File.SetAttributes(manualFile, FileAttributes.Normal);
                    File.Delete(manualFile);
                }
            }

            //save the output file  
            sourceDocument.Close();

            barImport.Value = dx.Rows.Count;
            barImport.Refresh();

            ViewPDF myView = new ViewPDF("TEST", outputPdfPath);
            myView.ShowDialog();

            if (File.Exists(outputPdfPath))
            {
                File.SetAttributes(outputPdfPath, FileAttributes.Normal);
                File.Delete(outputPdfPath);
            }

            this.Cursor = Cursors.Default;
            dx.Dispose();

            barImport.Hide();
        }
        /***********************************************************************************************/
        private static void MergeAllPDF( PdfCopy pdfCopyProvider, string File1, string File2, string File3)
        {
            string[] fileArray = new string[4];
            fileArray[0] = File1;
            fileArray[1] = File2;
            fileArray[2] = File3;

            PdfReader reader = null;
            PdfImportedPage importedPage;


            //files list wise Loop  
            for (int f = 0; f < fileArray.Length - 1; f++)
            {
                int pages = TotalPageCount(fileArray[f]);

                reader = new PdfReader(fileArray[f]);
                //Add pages in new file  
                for (int i = 1; i <= pages; i++)
                {
                    importedPage = pdfCopyProvider.GetImportedPage(reader, i);
                    pdfCopyProvider.AddPage(importedPage);
                }

                reader.Close();
            }
        }
        /***********************************************************************************************/
        private void btnGenerate_Clickx(object sender, EventArgs e)
        {
            GeneratePages = true;
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt == null)
                return;
            if (dt.Rows.Count <= 0)
                return;
            int[] rows = gridMain.GetSelectedRows();
            int firstRow = 0;
            int lastRow = dt.Rows.Count;
            if (rows.Length <= 0)
            {
                GenerateNotices(dt, e);
                return;
            }
            int row = 0;
            DataTable dx = dt.Clone();
            DataRow dRow = null;
            for (int i = 0; i < rows.Length; i++)
            {
                row = rows[i];
                if (row < 0)
                    continue;
                firstRow = gridMain.GetDataSourceRowIndex(row);
                dRow = dt.Rows[firstRow];
                dx.ImportRow(dRow);
            }
            GenerateNotices(dx, e);
            dx.Dispose();
            GeneratePages = false;
        }
        /***********************************************************************************************/
        private string majorLastLocation = "";
        private string majorLastDetail = "";
        private string lastLocation = "";

        private bool printFirst = true;
        private int printRow = 0;
        /***********************************************************************************************/
        private DataTable FixForPrint ( DataTable dt, bool beforePrint )
        {
            if ( beforePrint)
            {
                if (G1.get_column_number(dt, "orderG") < 0)
                    dt.Columns.Add("orderG");
                if (G1.get_column_number(dt, "explaination") < 0)
                    dt.Columns.Add("explaination");
                if (G1.get_column_number(dt, "totalAdjustment") < 0)
                    dt.Columns.Add("totalAdjustment", Type.GetType("System.Double"));

                double totalAdjustment = 0D;

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i]["orderG"] = i.ToString("D4");
                    dt.Rows[i]["lastName"] += ", " + dt.Rows[i]["firstName"].ObjToString();
                    dt.Rows[i]["explaination"] = "Automated Trust Adjustment To Correct Trust Beginning Balance On Final Payment";
                    totalAdjustment = dt.Rows[i]["interestPaid"].ObjToDouble() + dt.Rows[i]["retained"].ObjToDouble();
                    totalAdjustment = G1.RoundValue(totalAdjustment);
                    dt.Rows[i]["totalAdjustment"] = totalAdjustment;
                }
                DataView tempview = dt.DefaultView;
                tempview.Sort = "orderG asc";
                dt = tempview.ToTable();

                gridMain.Columns["name"].Width = gridMain.Columns["lastName"].Width;
                gridMain.Columns["lastName"].Caption = "NAME";
                gridMain.Columns["firstName"].Visible = false;
                gridMain.Columns["depositNumber"].Visible = false;
                gridMain.Columns["num"].Visible = false;
                gridMain.Columns["totalAdjustment"].Visible = true;
                gridMain.Columns["explaination"].Visible = true;
                dgv.DataSource = dt;
            }
            else
            {
                gridMain.Columns["lastName"].Caption = "Last Name";
                gridMain.Columns["lastName"].Width = gridMain.Columns["name"].Width;
                gridMain.Columns["firstName"].Visible = false;
                gridMain.Columns["num"].Visible = true;
                gridMain.Columns["explaination"].Visible = false;
                gridMain.Columns["totalAdjustment"].Visible = false;
                gridMain.Columns["explaination"].Visible = false;
                dgv.DataSource = originalDt;
            }

            return dt;
        }
        /***********************************************************************************************/
        private void GenerateNotices( DataTable dt, EventArgs e)
        {
            if (dt == null)
                return;
            if (dt.Rows.Count <= 0)
                return;

            dt = FixForPrint(dt, true);

            //gridMain2.Columns["lastName"].Caption = "Name";
            ////gridMain2.Columns["lastName"].Width = gridMain.Columns["lastName"].Width + gridMain.Columns["firstName"].Width;
            //gridMain2.Columns["firstName"].Visible = false;
            //gridMain2.Columns["balanceDue"].Visible = false;
            //gridMain2.Columns["contractValue"].Visible = false;
            //gridMain2.Columns["totalTrust85"].Visible = false;

            Font font = new Font("Calibri", 11F, FontStyle.Regular );

            //gridMain2.ColumnPanelRowHeight = -1;
            //gridMain3.ColumnPanelRowHeight = -1;
            //gridMain3.OptionsPrint.AutoWidth = true;

            //gridMain2.Columns["lastName"].AppearanceHeader.Font = font;
            //gridMain2.Columns["datePaid"].AppearanceHeader.Font = font;
            //gridMain2.Columns["contractNumber"].AppearanceHeader.Font = font;

            //gridMain2.AppearancePrint.HeaderPanel.TextOptions.WordWrap = WordWrap.Wrap;
            gridMain.AppearancePrint.HeaderPanel.Font = font;

            printFirst = true;
            isPrinting = true;
            //footerCount = 0;
            printRow = 1;
            if (this.components == null)
                this.components = new System.ComponentModel.Container();

            gridMain.BeginUpdate();
            gridMain.ColumnPanelRowHeight = 75;
            //gridMain3.AppearancePrint.HeaderPanel.TextOptions.WordWrap = WordWrap.Wrap;

            DevExpress.XtraPrinting.PrintingSystem printingSystem1 = new DevExpress.XtraPrinting.PrintingSystem(this.components);
            DevExpress.XtraPrinting.PrintableComponentLink printableComponentLink1 = new DevExpress.XtraPrinting.PrintableComponentLink(this.components);

            printingSystem1.Links.AddRange(new object[] {
            printableComponentLink1});

            if (dt.Rows.Count > 0)
            {
                majorLastDetail = dt.Rows[0]["depositNumber"].ObjToString();
            }

            dgv.DataSource = dt;
            printableComponentLink1.Component = dgv;

            printableComponentLink1.PrintingSystemBase = printingSystem1;

            //printableComponentLink1.EnablePageDialog = true;

            printableComponentLink1.CreateDetailHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateDetailHeaderArea);
            printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);
            printableComponentLink1.BeforeCreateAreas += new System.EventHandler(this.printableComponentLink1_BeforeCreateAreas);
            printableComponentLink1.AfterCreateAreas += new System.EventHandler(this.printableComponentLink1_AfterCreateAreas);

            printableComponentLink1.Landscape = true;

            Printer.setupPrinterMargins(50, 100, 80, 50);
            //Printer.setupPrinterMargins(50, 50, 80, 50);

            pageMarginLeft = Printer.pageMarginLeft;
            pageMarginRight = Printer.pageMarginRight;
            pageMarginTop = Printer.pageMarginTop;
            pageMarginBottom = Printer.pageMarginBottom;

            printableComponentLink1.Margins.Left = pageMarginLeft;
            printableComponentLink1.Margins.Right = pageMarginRight;
            printableComponentLink1.Margins.Top = pageMarginTop;
            printableComponentLink1.Margins.Bottom = pageMarginBottom;

            //printingSystem1.Document.AutoFitToPagesWidth = 1;

            gridMain.EndUpdate();

            printableComponentLink1.CreateDocument();
            printableComponentLink1.ShowPreview();
            isPrinting = false;

            dt = FixForPrint(dt, false);

            gridMain.BeginUpdate();
            gridMain.ColumnPanelRowHeight = -1;
            gridMain.EndUpdate();
        }
        /***********************************************************************************************/
        private bool FindLastLocation(DevExpress.XtraGrid.Views.Printing.PrintRowEventArgs e)
        {
            if (e.RowHandle < 0)
                return false;
            majorLastLocation = majorLastDetail;
            lastLocation = "";

            try
            {
                DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gMain = gridMain;
                //DevExpress.XtraGrid.Views.BandedGrid.BandedGridView gMain = gridMain2;
                DataTable dt = (DataTable) dgv.DataSource;
                int rowHandle = e.RowHandle;
                int row = gMain.GetDataSourceRowIndex(rowHandle);
                majorLastDetail = dt.Rows[row]["depositNumber"].ObjToString();
            }
            catch ( Exception ex)
            {
            }
            return true;
        }
        /***********************************************************************************************/
        private void gridMain2_BeforePrintRow(object sender, DevExpress.XtraGrid.Views.Printing.CancelPrintRowEventArgs e)
        {
            if ( printFirst )
            {
                AddHeading(e);
                printFirst = false;
                return;
            }
            bool good = FindLastLocation(e);
            if (!good)
                return;
            //gridBand5.Caption = "DepositNumber" + majorLastDetail;
            //gridBand4.Caption = "DepositNumber" + majorLastDetail;
            e.PS.InsertPageBreak(e.Y);
            AddHeading(e);
        }
        /***********************************************************************************************/
        private void AddHeading(DevExpress.XtraGrid.Views.Printing.CancelPrintRowEventArgs e)
        {
            TextBrick tb = e.PS.CreateTextBrick() as TextBrick;
            string junk = "Automated Trust Adjustment To Correct Trust Beginning Balance On Final Payment";
            tb.Text = majorLastDetail + " " + junk;
            tb.Font = new Font(tb.Font, FontStyle.Regular);
            tb.HorzAlignment = DevExpress.Utils.HorzAlignment.Center;
            tb.Padding = new PaddingInfo(5, 0, 0, 0);
            tb.BackColor = Color.LightGray;
            tb.ForeColor = Color.Black;
            // Get the client page width. 
            SizeF clientPageSize = (e.BrickGraphics as BrickGraphics).ClientPageSize;
            float textBrickHeight = e.Graphics.MeasureString(tb.Text, tb.Font).Height + 4;
            // Calculate a rectangle for the brick and draw the brick. 
            RectangleF textBrickRect = new RectangleF(0, e.Y, (int)clientPageSize.Width, textBrickHeight);
            e.BrickGraphics.DrawBrick(tb, textBrickRect);
            // Adjust the current Y position to print the following row below the brick. 
            e.Y += (int)textBrickHeight;
        }
        /***********************************************************************************************/
        private void gridMain2_CalcRowHeight(object sender, DevExpress.XtraGrid.Views.Grid.RowHeightEventArgs e)
        {
            int maxHeight = 0;
            foreach (GridColumn column in gridMain2.Columns)
            {
                using (RepositoryItemMemoEdit edit = new RepositoryItemMemoEdit())
                {
                    using (MemoEditViewInfo viewInfo = edit.CreateViewInfo() as MemoEditViewInfo)
                    {
                        viewInfo.EditValue = gridMain2.GetRowCellValue(e.RowHandle, column.FieldName);
                        viewInfo.Bounds = new Rectangle(0, 0, column.VisibleWidth, dgv2.Height);
                        using (Graphics graphics = dgv2.CreateGraphics())
                        using (GraphicsCache cache = new GraphicsCache(graphics))
                        {
                            viewInfo.CalcViewInfo(graphics);
                            var height = ((IHeightAdaptable)viewInfo).CalcHeight(cache, column.VisibleWidth);
                            maxHeight = Math.Max(height, maxHeight);
                        }
                    }
                }
            }
            e.RowHeight = maxHeight;
        }
        /***********************************************************************************************/
        private void gridMain3_CalcRowHeight(object sender, DevExpress.XtraGrid.Views.Grid.RowHeightEventArgs e)
        {
            int maxHeight = 0;
            foreach (GridColumn column in gridMain3.Columns)
            {
                using (RepositoryItemMemoEdit edit = new RepositoryItemMemoEdit())
                {
                    using (MemoEditViewInfo viewInfo = edit.CreateViewInfo() as MemoEditViewInfo)
                    {
                        viewInfo.EditValue = gridMain3.GetRowCellValue(e.RowHandle, column.FieldName);
                        viewInfo.Bounds = new Rectangle(0, 0, column.VisibleWidth, dgv2.Height);
                        using (Graphics graphics = dgv3.CreateGraphics())
                        using (GraphicsCache cache = new GraphicsCache(graphics))
                        {
                            viewInfo.CalcViewInfo(graphics);
                            var height = ((IHeightAdaptable)viewInfo).CalcHeight(cache, column.VisibleWidth);
                            maxHeight = Math.Max(height, maxHeight);
                        }
                    }
                }
            }
            e.RowHeight = maxHeight;
        }
        /***********************************************************************************************/
        private void gridMain_CalcRowHeight(object sender, DevExpress.XtraGrid.Views.Grid.RowHeightEventArgs e)
        {
            //if (1 == 1)
            //    return;
            int maxHeight = 0;
            foreach (GridColumn column in gridMain.Columns)
            {
                using (RepositoryItemMemoEdit edit = new RepositoryItemMemoEdit())
                {
                    using (MemoEditViewInfo viewInfo = edit.CreateViewInfo() as MemoEditViewInfo)
                    {
                        viewInfo.EditValue = gridMain.GetRowCellValue(e.RowHandle, column.FieldName);
                        viewInfo.Bounds = new Rectangle(0, 0, column.VisibleWidth, dgv.Height);
                        using (Graphics graphics = dgv.CreateGraphics())
                        using (GraphicsCache cache = new GraphicsCache(graphics))
                        {
                            viewInfo.CalcViewInfo(graphics);
                            var height = ((IHeightAdaptable)viewInfo).CalcHeight(cache, column.VisibleWidth);
                            maxHeight = Math.Max(height, maxHeight);
                        }
                    }
                }
            }
            e.RowHeight = maxHeight;
        }
        /***********************************************************************************************/
        private void gridMain_BeforePrintRow(object sender, DevExpress.XtraGrid.Views.Printing.CancelPrintRowEventArgs e)
        {
            if (!GeneratePages)
                return;
            if (printFirst)
            {
                AddHeading(e);
                printFirst = false;
                return;
            }
            bool good = FindLastLocation(e);
            if (!good)
                return;
            e.PS.InsertPageBreak(e.Y);
            AddHeading(e);
        }
        /***********************************************************************************************/
        private static void MergePDF(string File1, string File2, string File3 )
        {
            string[] fileArray = new string[4];
            fileArray[0] = File1;
            fileArray[1] = File2;
            fileArray[2] = File3;

            PdfReader reader = null;
            iTextSharp.text.Document sourceDocument = null;
            PdfCopy pdfCopyProvider = null;
            PdfImportedPage importedPage;
            string inputPdfPath = File1;
            string outputPdfPath = @"C:/Users/robby/downloads/pdfX.pdf";
            GrantAccess(@"C:/Users/robby/downloads");

            if (File.Exists(outputPdfPath))
            {
                GrantAccess(outputPdfPath);
                File.Delete(outputPdfPath);
            }

            //try
            //{
            //    File.Copy(inputPdfPath, outputPdfPath);
            //}
            //catch ( Exception ex)
            //{
            //}

            sourceDocument = new iTextSharp.text.Document();
            pdfCopyProvider = new PdfCopy(sourceDocument, new System.IO.FileStream(outputPdfPath, System.IO.FileMode.Create));

            //output file Open  
            sourceDocument.Open();


            //files list wise Loop  
            for (int f = 0; f < fileArray.Length - 1; f++)
            {
                int pages = TotalPageCount(fileArray[f]);

                reader = new PdfReader(fileArray[f]);
                //Add pages in new file  
                for (int i = 1; i <= pages; i++)
                {
                    importedPage = pdfCopyProvider.GetImportedPage(reader, i);
                    pdfCopyProvider.AddPage(importedPage);
                }

                reader.Close();
            }
            //save the output file  
            sourceDocument.Close();

            ViewPDF myView = new ViewPDF("TEST", outputPdfPath);
            myView.ShowDialog();

            if (File.Exists(outputPdfPath))
                File.Delete(outputPdfPath);
        }
        /***********************************************************************************************/
        private static int TotalPageCount(string file)
        {
            if (File.Exists(file))
            {
                using (StreamReader sr = new StreamReader(System.IO.File.OpenRead(file)))
                {
                    Regex regex = new Regex(@"/Type\s*/Page[^s]");
                    MatchCollection matches = regex.Matches(sr.ReadToEnd());

                    return matches.Count;
                }
            }
            else
                return 0;
        }
        /***********************************************************************************************/
        private void printDetailToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();

            string contract = dr["contractNumber"].ObjToString();
            if (String.IsNullOrWhiteSpace(contract))
                return;

            string record = dr["record"].ObjToString();
            if (String.IsNullOrWhiteSpace(record))
                return;

            this.Cursor = Cursors.WaitCursor;

            string historyFile = @"C:/rag/pdfDaily.pdf";
            GrantFileAccess(historyFile);

            DailyHistory histForm = new DailyHistory(contract, historyFile, true);

            string manualFile = @"c:/rag/Manual.pdf";
            string payOffFile = @"c:/rag/ForcePayoff.pdf";

            GrantFileAccess(manualFile);
            GrantFileAccess(payOffFile);


            ForcedPayoffs forceForm = new ForcedPayoffs(true, record, this.dateTimePicker1.Value, this.dateTimePicker2.Value );

            this.Cursor = Cursors.Default;

            MergePDF(payOffFile, manualFile, historyFile );

            if (File.Exists(payOffFile))
                File.Delete(payOffFile);

            if (File.Exists(historyFile))
                File.Delete(historyFile);

            if (File.Exists(manualFile))
                File.Delete(manualFile);

        }
        /***********************************************************************************************/
        private void GrantFileAccess(string fullPath)
        {
            try
            {
                if (!File.Exists(fullPath))
                    return;
                DirectoryInfo dInfo = new DirectoryInfo(fullPath);
                DirectorySecurity dSecurity = null;
                try
                {
                    dSecurity = dInfo.GetAccessControl();
                }
                catch ( Exception ex)
                {
                }
                dSecurity.AddAccessRule(new FileSystemAccessRule(
                    new SecurityIdentifier(WellKnownSidType.WorldSid, null),
                    FileSystemRights.FullControl,
                    InheritanceFlags.ObjectInherit |
                       InheritanceFlags.ContainerInherit,
                    PropagationFlags.NoPropagateInherit,
                    AccessControlType.Allow));

                dInfo.SetAccessControl(dSecurity);
            }
            catch ( Exception ex)
            {
                MessageBox.Show("***ERROR*** " + ex.Message.ToString());
            }
        }        
        /***********************************************************************************************/
        private static void GrantAccess(string file)
        {
            bool exists = System.IO.Directory.Exists(file);
            if (!exists)
            {
                DirectoryInfo di = System.IO.Directory.CreateDirectory(file);
                //Console.WriteLine("The Folder is created Sucessfully");
            }
            else
            {
                //Console.WriteLine("The Folder already exists");
            }
            DirectoryInfo dInfo = new DirectoryInfo(file);
            DirectorySecurity dSecurity = dInfo.GetAccessControl();
            dSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
            dInfo.SetAccessControl(dSecurity);

        }
        /***********************************************************************************************/
        void SetOwner(FileInfo file)
        {
            try
            {
                var acl = file.GetAccessControl(System.Security.AccessControl.AccessControlSections.All);

                acl.SetOwner(CoreUtils.RunningAccount);
                acl.AddAccessRule(new System.Security.AccessControl.FileSystemAccessRule(
                    CoreUtils.RunningUser, System.Security.AccessControl.FileSystemRights.FullControl, System.Security.AccessControl.AccessControlType.Allow));

                file.SetAccessControl(acl);
            }
            catch ( Exception ex)
            {
            }
        }
        /***********************************************************************************************/
    }
    public static class CoreUtils
    {
        public static string RunningUser { get; } = $"{Environment.UserDomainName}\\{Environment.UserName}";
        public static NTAccount RunningAccount { get; } = new NTAccount(Environment.UserDomainName, Environment.UserName);
    }
}