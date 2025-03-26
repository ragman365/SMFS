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
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class PotentialPayoffs : DevExpress.XtraEditors.XtraForm
    {
        DataTable workDt = null;
        /***********************************************************************************************/
        public PotentialPayoffs( DataTable dt )
        {
            InitializeComponent();
            workDt = dt;
        }
        /***********************************************************************************************/
        public PotentialPayoffs()
        {
            InitializeComponent();
        }
        /***********************************************************************************************/
        private void PotentialPayoffs_Load(object sender, EventArgs e)
        {
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

                gridMain.Columns["creditBalance"].Visible = false;
                gridMain.Columns["lastDatePaid8"].Visible = false;
                gridMain.Columns["dueDate8"].Visible = false;
                gridMain.Columns["issueDate8"].Visible = false;
                gridMain.Columns["paid"].Visible = false;
                gridMain.Columns["nowDue"].Visible = false;

                gridMain.Columns["contractValue"].Visible = true;
                gridMain.Columns["datePaid"].Visible = true;
                gridMain.Columns["maxTrust85"].Visible = true;
                gridMain.Columns["totalTrust85"].Visible = true;
                gridMain.Columns["difference"].Visible = true;
                gridMain.Columns["difference2"].Visible = true;

                SetupColumnView();
            }
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

            Printer.setupPrinterMargins(50, 100, 80, 50);

            pageMarginLeft = Printer.pageMarginLeft;
            pageMarginRight = Printer.pageMarginRight;
            pageMarginTop = Printer.pageMarginTop;
            pageMarginBottom = Printer.pageMarginBottom;

            printableComponentLink1.Margins.Left = pageMarginLeft;
            printableComponentLink1.Margins.Right = pageMarginRight;
            printableComponentLink1.Margins.Top = pageMarginTop;
            printableComponentLink1.Margins.Bottom = pageMarginBottom;

            printingSystem1.Document.AutoFitToPagesWidth = 1;

            G1.PrintPreview(printableComponentLink1, gridMain);

            //printableComponentLink1.CreateDocument();
            //printableComponentLink1.ShowPreview();

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

            Printer.setupPrinterMargins(50, 100, 80, 50);

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
            Font font = new Font("Ariel", 16);
            Printer.DrawQuad(1, 1, Printer.xQuads, 2, "South Mississippi Funeral Services, LLC", Color.Black, BorderSide.Top, font, HorizontalAlignment.Center);

            Printer.SetQuadSize(12, 12);

            font = new Font("Ariel", 8);
            Printer.DrawGridDate(2, 3, 2, 3, Color.Black, BorderSide.None, font);
            Printer.DrawGridPage(11, 3, 2, 3, Color.Black, BorderSide.None, font);

            Printer.DrawQuad(1, 9, 2, 3, "User : " + LoginForm.username, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);


            font = new Font("Ariel", 10, FontStyle.Bold);
            Printer.DrawQuad(5, 8, 4, 4, "Insurance Coupon Report", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

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

            string cmd = "Select * from `forced_paidoff` f JOIN `contracts` c ON f.`contractNumber` = c.`contractNumber` JOIN `customers` p ON f.`contractNumber` = p.`contractNumber` ";
            cmd += " WHERE `datePaid` >= '" + start.ToString("yyyy-MM-dd") + "' AND `datePaid` <= '" + stop.ToString("yyyy-MM-dd") + "';";
            DataTable dt = G1.get_db_data(cmd);

            FixRetained(dt);

            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
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

            for ( int i=0; i<dt.Rows.Count; i++)
            {
                difference = dt.Rows[i]["difference"].ObjToDouble();
                if (difference < 0D)
                {
                    dt.Rows[i]["difference"] = Math.Abs(difference);
                    dt.Rows[i]["difference2"] = 0D;
                }
                else
                {
                    dt.Rows[i]["difference2"] = difference;
                    dt.Rows[i]["difference"] = 0D;
                }
            }
        }
        /***********************************************************************************************/
    }
}