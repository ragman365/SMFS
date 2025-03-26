using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using DevExpress.XtraPrinting;
using DevExpress.Utils;
using GeneralLib;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using MySql.Data.MySqlClient;
using DevExpress.XtraGrid;
using DevExpress.Utils.Drawing;
using OfficeOpenXml;
using Excel = Microsoft.Office.Interop.Excel;
using System.Reflection;
using DevExpress.XtraGrid.Views.Base;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class TrustNonPaidOff : DevExpress.XtraEditors.XtraForm
    {
        private bool autoRun = false;
        private bool autoForce = false;
        private string sendTo = "";
        private string sendWhere = "";
        private string emailLocations = "";
        /****************************************************************************************/
        DataTable originalDt = null;
        /***********************************************************************************************/
        public TrustNonPaidOff()
        {
            InitializeComponent();
        }
        /****************************************************************************************/
        public TrustNonPaidOff(bool auto, bool force, string title = "")
        {
            autoRun = auto;
            autoForce = force;
            InitializeComponent();
            RunAutoReports();
        }
        /****************************************************************************************/
        private void RunAutoReports()
        {
            string cmd = "Select * from `remote_processing`;";
            DataTable dt = G1.get_db_data(cmd);
            string report = "";
            DateTime date = DateTime.Now;
            long currentDay = G1.date_to_days(date.ToString("MM/dd/yyyy"));
            int presentDay = date.Day;
            int dayToRun = 0;
            string status = "";
            string frequency = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                report = dt.Rows[i]["report"].ObjToString();
                if (report.ToUpper() != "TRUST NOT PAID OFF")
                    continue;
                status = dt.Rows[i]["status"].ObjToString();
                if (status.ToUpper() == "INACTIVE")
                    continue;
                if (!autoForce)
                {
                    dayToRun = dt.Rows[i]["day_to_run"].ObjToInt32();
                    frequency = dt.Rows[i]["dateIncrement"].ObjToString();
                    if (!AutoRunSetup.CheckOkToRun(dayToRun, frequency))
                        return;
                }
                report = dt.Rows[i]["report"].ObjToString();
                sendTo = dt.Rows[i]["sendTo"].ObjToString();
                sendWhere = dt.Rows[i]["sendWhere"].ObjToString();
                TrustNonPaidOff_Load(null, null);
            }
        }
        /***********************************************************************************************/
        private void TrustNonPaidOff_Load(object sender, EventArgs e)
        {
            if (autoRun)
            {
                btnRun_Click(null, null);
                DataTable dt = (DataTable)dgv.DataSource;
                emailLocations = DailyHistory.ParseOutLocations(dt);

                printPreviewToolStripMenuItem_Click(null, null);
                this.Close();
            }
            DateTime now = DateTime.Now;
            int year = now.Year - 6;
            txtYear.Text = year.ToString("D4");
        }
        /***********************************************************************************************/
        private void btnRun_Click(object sender, EventArgs e)
        {
            int year = txtYear.Text.ObjToInt32();
            year = year % 100;
            string lookup = "_" + year.ToString("D2") + "%";
            string lookup2 = "__" + year.ToString("D2") + "%";
            this.Cursor = Cursors.WaitCursor;
            string cmd = "Select * from `contracts` x JOIN `customers` c ON x.`contractNumber` = c.`contractNumber` WHERE x.`deceasedDate` < '1805-01-01' ";
            cmd += " AND ( x.`contractNumber` LIKE '" + lookup + "' OR x.`contractNumber` LIKE '" + lookup2 + "' ) ";
            cmd += " AND x.`lastDatePaid8` <> '0000-00-00' ";
            cmd += " AND x.`dueDate8` < '2039-12-31' ";
            if (!chkIncludeLapsed.Checked)
                cmd += " AND x.`lapsed` <> 'Y' ";
            cmd += " ORDER BY c.`lastName`,c.`firstName`;";
            DataTable dt = G1.get_db_data(cmd);

            dt.Columns.Add("num");
            dt.Columns.Add("issueDate");

            DateTime date = DateTime.Now;
            DateTime deceasedDate = DateTime.Now;
            string contractNumber = "";

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                //deceasedDate = dt.Rows[i]["deceasedDate"].ObjToDateTime();
                //if ( deceasedDate.Year > 100)
                //{
                //    dt.Rows[i]["contractNumber"] = "";
                //    continue;
                //}
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                date = dt.Rows[i]["issueDate8"].ObjToDateTime();
                date = DailyHistory.GetIssueDate(date, contractNumber, dt);
                dt.Rows[i]["issueDate"] = date.ToString("MM/dd/yyyy");
            }

            //for (int i = (dt.Rows.Count - 1); i >= 0; i--)
            //{
            //    contractNumber = dt.Rows[i]["contractNumnber"].ObjToString();
            //    if ( String.IsNullOrWhiteSpace ( contractNumber))
            //        dt.Rows.RemoveAt(i);
            //}

            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            originalDt = dt;
            this.Cursor = Cursors.Default;
        }

        /***********************************************************************************************/
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        /***********************************************************************************************/
        private void btnLeft_Click(object sender, EventArgs e)
        {
        }
        /***********************************************************************************************/
        private void btnRight_Click(object sender, EventArgs e)
        {
        }
        /***********************************************************************************************/
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
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

            printableComponentLink1.EnablePageDialog = true;

            printableComponentLink1.CreateDetailHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateDetailHeaderArea);
            printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);
            printableComponentLink1.BeforeCreateAreas += new System.EventHandler(this.printableComponentLink1_BeforeCreateAreas);
            printableComponentLink1.AfterCreateAreas += new System.EventHandler(this.printableComponentLink1_AfterCreateAreas);
            printableComponentLink1.Landscape = true;

            Printer.setupPrinterMargins(50, 100, 80, 50);

            pageMarginLeft = Printer.pageMarginLeft;
            pageMarginRight = Printer.pageMarginRight;
            pageMarginTop = Printer.pageMarginTop;
            pageMarginBottom = Printer.pageMarginBottom;

            printableComponentLink1.Margins.Left = pageMarginLeft;
            printableComponentLink1.Margins.Right = pageMarginRight;
            printableComponentLink1.Margins.Top = pageMarginTop;
            printableComponentLink1.Margins.Bottom = pageMarginBottom;

            G1.AdjustColumnWidths(gridMain, 0.65D, true);

            printableComponentLink1.CreateDocument();
            if (autoRun)
            {
                string path = G1.GetReportPath();
                DateTime today = DateTime.Now;

                string filename = path + @"\Trust_Summary_" + today.Year.ToString("D4") + today.Month.ToString("D2") + today.Day.ToString("D2") + ".pdf";
                if (File.Exists(filename))
                    File.Delete(filename);
                printableComponentLink1.ExportToPdf(filename);
                RemoteProcessing.AutoRunSendTo("Trust Summary Report", filename, sendTo, sendWhere, emailLocations);
            }
            else
                printableComponentLink1.ShowPreview();

            G1.AdjustColumnWidths(gridMain, 0.65D, false );

            isPrinting = false;
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

            Printer.setupPrinterMargins(50, 100, 80, 50);

            pageMarginLeft = Printer.pageMarginLeft;
            pageMarginRight = Printer.pageMarginRight;
            pageMarginTop = Printer.pageMarginTop;
            pageMarginBottom = Printer.pageMarginBottom;

            printableComponentLink1.Margins.Left = pageMarginLeft;
            printableComponentLink1.Margins.Right = pageMarginRight;
            printableComponentLink1.Margins.Top = pageMarginTop;
            printableComponentLink1.Margins.Bottom = pageMarginBottom;

            printableComponentLink1.CreateDocument();
            if (LoginForm.doLapseReport)
                printableComponentLink1.Print();
            else
                printableComponentLink1.PrintDlg();
            isPrinting = false;
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
            Font font = new Font("Ariel", 16);
            Printer.DrawQuad(1, 1, Printer.xQuads, 2, "South Mississippi Funeral Services, LLC", Color.Black, BorderSide.Top, font, HorizontalAlignment.Center);

            Printer.SetQuadSize(12, 12);

            font = new Font("Ariel", 8);
            Printer.DrawGridDate(2, 3, 2, 3, Color.Black, BorderSide.None, font);
            Printer.DrawGridPage(11, 3, 2, 3, Color.Black, BorderSide.None, font);

            Printer.DrawQuad(1, 9, 2, 3, "User : " + LoginForm.username, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);


            font = new Font("Ariel", 10, FontStyle.Regular);
            Printer.DrawQuad(5, 8, 4, 4, "Trust Contracts Not Yet Paid Since " + txtYear.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            DateTime date = DateTime.Now;
            string workDate = date.Month.ToString("D2") + "/" + date.Day.ToString("D2") + "/" + (date.Year % 100).ToString("D2");

            string str = "Report : " + workDate;

            Printer.SetQuadSize(24, 12);
            font = new Font("Ariel", 9, FontStyle.Regular);
            Printer.DrawQuad(19, 8, 5, 4, str, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(16, 8, 3, 4, lblPayment.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(19, 8, 3, 4, lblTrust85.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(22, 8, 3, 4, lblTrust100.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /***********************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            SetSpyGlass(gridMain);
        }
        /***********************************************************************************************/
        private void SetSpyGlass(DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView grid)
        {
            if (grid.OptionsFind.AlwaysVisible == true)
                grid.OptionsFind.AlwaysVisible = false;
            else
                grid.OptionsFind.AlwaysVisible = true;
        }
        /***********************************************************************************************/
        private void gridMain_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            ColumnView view = sender as ColumnView;
            if (e.Column.FieldName.ToUpper().IndexOf("DATE") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                if (e.DisplayText.IndexOf("0000") >= 0 || e.DisplayText.IndexOf("0001") >= 0)
                    e.DisplayText = "";
                else
                {
                    DateTime date = e.DisplayText.ObjToDateTime();
                    e.DisplayText = date.ToString("MM/dd/yyyy");
                    if (e.DisplayText.IndexOf("0000") >= 0 || e.DisplayText.IndexOf("0001") >= 0)
                        e.DisplayText = "";
                }
            }
        }
        /***********************************************************************************************/
        private void gridMain_DoubleClick_1(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string contract = dr["contractNumber"].ObjToString();
            this.Cursor = Cursors.WaitCursor;
            CustomerDetails clientForm = new CustomerDetails(contract);
            clientForm.Show();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
    }
}