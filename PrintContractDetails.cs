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
    public partial class PrintContractDetails : DevExpress.XtraEditors.XtraForm
    {
        private bool LapsedContract = false;
        private string workContract = "";
        /***********************************************************************************************/
        public PrintContractDetails( string contract )
        {
            InitializeComponent();
            workContract = contract;
        }
        /****************************************************************************************/
        private void ClearAllPositions(DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gMain = null)
        {
            if (gMain == null)
                gMain = gridMain;
            for (int i = 0; i < gMain.Columns.Count; i++)
            {
                gMain.Columns[i].Visible = false;
            }
        }
        /****************************************************************************************/
        private void SetTrustPositions()
        {
            ClearAllPositions(gridMain);
            G1.SetColumnPosition(gridMain, "contractNumber", 1);
            G1.SetColumnPosition(gridMain, "lastName", 2);
            G1.SetColumnPosition(gridMain, "firstName", 3);
            G1.SetColumnPosition(gridMain, "contractValue", 4);
            G1.SetColumnPosition(gridMain, "amountPaid", 5);
            G1.SetColumnPosition(gridMain, "issueDate", 6);
            G1.SetColumnPosition(gridMain, "grossTrust", 7);
            G1.SetColumnPosition(gridMain, "dueDate8", 8);
            G1.SetColumnPosition(gridMain, "amtOfMonthlyPayt", 9);
        }
        /****************************************************************************************/
        private void SetLapsedTrustPositions()
        {
            ClearAllPositions(gridMain);
            G1.SetColumnPosition(gridMain, "contractNumber", 1);
            G1.SetColumnPosition(gridMain, "dueDate8", 2);
            G1.SetColumnPosition(gridMain, "lastName", 3);
            G1.SetColumnPosition(gridMain, "firstName", 4);
            G1.SetColumnPosition(gridMain, "amountPaid", 5);
            G1.SetColumnPosition(gridMain, "contractValue", 6);
            G1.SetColumnPosition(gridMain, "amtOfMonthlyPayt", 7);
            G1.SetColumnPosition(gridMain, "issueDate", 8);
            G1.SetColumnPosition(gridMain, "address1", 9);
        }
        /***********************************************************************************************/
        private void PrintContractDetails_Load(object sender, EventArgs e)
        {
            string cmd = "Select * from `contracts` x JOIN `customers` c ON x.`contractNumber` = c.`contractNumber` WHERE x.`contractNumber` = '" + workContract + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return;

            this.Text = "Trust Contract";
            LapsedContract = false;
            if (dt.Rows[0]["lapsed"].ObjToString() == "Y")
            {
                this.Text = "Lapsed Trust Contract";
                LapsedContract = true;
                SetLapsedTrustPositions();
            }
            else
                SetTrustPositions();

            dt.Columns.Add("num");
            dt.Columns.Add("issueDate");
            dt.Columns.Add("contractValue", Type.GetType("System.Double"));
            dt.Columns.Add("grossTrust", Type.GetType("System.Double"));

            DateTime date = dt.Rows[0]["issueDate8"].ObjToDateTime();
            date = DailyHistory.GetIssueDate(date, workContract, dt);
            dt.Rows[0]["issueDate"] = date.ToString("MM/dd/yyyy");

            double contractValue = DailyHistory.GetContractValue(dt.Rows[0]);
            dt.Rows[0]["contractValue"] = contractValue;
            dt.Rows[0]["grossTrust"] = contractValue;

            double financedAmount = DailyHistory.GetFinanceValue(dt.Rows[0]);

            double apr = dt.Rows[0]["APR"].ObjToDouble();
            int numPayments = dt.Rows[0]["numberOfPayments"].ObjToInt32();

            double amountPaid = GetAmountPaid(workContract, financedAmount, apr, numPayments, date);
            dt.Rows[0]["amountPaid"] = amountPaid;

            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
        }
        /***********************************************************************************************/
        private double GetAmountPaid ( string contractNumber, double financedAmount, double apr, int numPayments, DateTime issueDate )
        {
            double amountPaid = 0D;

            DataTable dt = G1.get_db_data("Select * from `payments` where `contractNumber` = '" + contractNumber + "' order by `paydate8` DESC, `tmstamp` DESC;");
            if (dt.Rows.Count <= 0)
                return amountPaid;

            DailyHistory.CalculateNewStuff(dt, apr, numPayments, financedAmount, issueDate);

            double payment = 0D;
            double debit = 0D;
            double credit = 0D;
            double originalDownPayment = DailyHistory.GetDownPayment(workContract);

            DataView tempview = dt.DefaultView;
            tempview.Sort = "payDate8 asc";
            dt = tempview.ToTable();


            double principal = 0D;
            double interest = 0D;
            double newBalance = financedAmount;
            string status = "";
            DateTime payDate8 = DateTime.Now;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                status = dt.Rows[i]["fill"].ObjToString();
                if (status.ToUpper() == "D")
                    continue;
                payDate8 = dt.Rows[i]["payDate8"].ObjToDateTime();

                payment = dt.Rows[i]["paymentAmount"].ObjToDouble();
                debit = dt.Rows[i]["debitAdjustment"].ObjToDouble();
                credit = dt.Rows[i]["creditAdjustment"].ObjToDouble();
                interest = dt.Rows[i]["interestPaid"].ObjToDouble();
                if (payDate8 >= DailyHistory.majorDate)
                {
                    interest = dt.Rows[i]["int"].ObjToDouble();
                }

                principal = payment - interest + credit - debit;
                newBalance = newBalance - payment + interest - credit + debit;
                amountPaid += principal;
            }
            amountPaid += originalDownPayment;
            return amountPaid;
        }
        /***********************************************************************************************/
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
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

            printableComponentLink1.CreateDocument();
            printableComponentLink1.ShowPreview();
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


            font = new Font("Ariel", 10, FontStyle.Bold);
            string title = "Trust Contract";
            if (LapsedContract)
                title = "Lapsed " + title;
            Printer.DrawQuad(6, 8, 4, 4, title, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

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
        private void gridMain_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            ColumnView view = sender as ColumnView;
            if (e.Column.FieldName.ToUpper().IndexOf("DATE") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                if (e.DisplayText.IndexOf("0000") >= 0 || e.DisplayText.IndexOf("0001") >= 0)
                    e.DisplayText = "";
            }
        }
        /***********************************************************************************************/
    }
}