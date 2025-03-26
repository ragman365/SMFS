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
using DevExpress.XtraReports.UI;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class ReinstateRequest : DevExpress.XtraEditors.XtraForm
    {
        private bool InsuranceContract = false;
        private string workContract = "";
        private DataRow paymentdRow = null;
        private double workPayments = 0D;
        private string reinstateRequestNumber = "";
        private string reinstateRequestRecord = "";
        private string nextReinstateNumber = "";
        private bool isSaved = false;
        private bool isSavedMaybe = false;
        private bool redo = false;
        private string redoRequestNumber = "";
        /***********************************************************************************************/
        public ReinstateRequest( string contract, bool isInsurance, double totalPayment, DataRow pDRow = null)
        {
            InitializeComponent();
            workContract = contract;
            InsuranceContract = isInsurance;
            workPayments = totalPayment;
            paymentdRow = pDRow;
            isSaved = false;
            redo = false;
        }
        /***********************************************************************************************/
        public ReinstateRequest(string contract, double totalPayment, DataRow pDRow = null)
        {
            InitializeComponent();
            workContract = contract;
            InsuranceContract = DailyHistory.isInsurance(contract);
            workPayments = totalPayment;
            paymentdRow = pDRow;
            isSaved = false;
            redo = true;
            redoRequestNumber = paymentdRow["requestNumber"].ObjToString();
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
        private void SetPositions()
        {
            ClearAllPositions(gridMain);
            G1.SetColumnPosition(gridMain, "requestDate", 1);
            G1.SetColumnPosition(gridMain, "accountNumber", 2);
            G1.SetColumnPosition(gridMain, "location", 3);
            G1.SetColumnPosition(gridMain, "fullName", 4);
            G1.SetColumnPosition(gridMain, "numMonths", 5);
            G1.SetColumnPosition(gridMain, "amtOfMonthlyPayt", 6);
            G1.SetColumnPosition(gridMain, "amountPaid", 7);
            G1.SetColumnPosition(gridMain, "dueDate8", 8);
            G1.SetColumnPosition(gridMain, "cpApproval", 9);
        }
        /***********************************************************************************************/
        private void ReinstateRequest_Load(object sender, EventArgs e)
        {
            if (InsuranceContract)
                LoadInsurance();
            else
                LoadTrust();
        }
        /***********************************************************************************************/
        private void LoadTrust()
        {
            string cmd = "Select * from `contracts` x JOIN `customers` c ON x.`contractNumber` = c.`contractNumber` WHERE x.`contractNumber` = '" + workContract + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return;

            reinstateRequestNumber = getNextRequestNumber( false );
            if (!String.IsNullOrWhiteSpace(redoRequestNumber))
                reinstateRequestNumber = redoRequestNumber;
            this.gridBand7.Caption = "Reinstate Sequence # : " + reinstateRequestNumber;

            this.Text = "Trust Reinstate Request";
            SetPositions();

            dt.Columns.Add("num");
            dt.Columns.Add("issueDate");
            dt.Columns.Add("requestDate");
            dt.Columns.Add("payDate");
            dt.Columns.Add("fullName");
            dt.Columns.Add("accountNumber");
            dt.Columns.Add("location");
            dt.Columns.Add("cpApproval");
            dt.Columns.Add("contractValue", Type.GetType("System.Double"));
            dt.Columns.Add("grossTrust", Type.GetType("System.Double"));
            dt.Columns.Add("numMonths", Type.GetType("System.Double"));


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

            dt.Rows[0]["fullname"] = dt.Rows[0]["firstName"].ObjToString().Trim() + " " + dt.Rows[0]["lastName"].ObjToString().Trim();
            dt.Rows[0]["accountNumber"] = workContract;
            dt.Rows[0]["requestDate"] = DateTime.Now.ToString("MM/dd/yyyy");

            string trust = "";
            string loc = "";
            string contract = "";

            contract = Trust85.decodeContractNumber(workContract, ref trust, ref loc);
            dt.Rows[0]["location"] = loc;

            double expected = dt.Rows[0]["amtOfMonthlyPayt"].ObjToDouble();

            if ( paymentdRow != null)
            {
                if (!redo)
                {
                    double payment = workPayments;
                    double months = 0;
                    if (expected > 0D)
                        months = payment / expected;
                    dt.Rows[0]["amountPaid"] = payment;
                    dt.Rows[0]["numMonths"] = months;
                    dt.Rows[0]["payDate"] = paymentdRow["date"].ObjToString();
                }
                else
                {
                    dt.Rows[0]["requestDate"] = paymentdRow["dor"].ObjToString();
                    string str = paymentdRow["dueDate8"].ObjToString();
                    dt.Rows[0]["dueDate8"] = G1.DTtoMySQLDT(str);
                    dt.Rows[0]["accountNumber"] = workContract;
                    dt.Rows[0]["location"] = paymentdRow["location"].ObjToString();
                    dt.Rows[0]["amountPaid"] = paymentdRow["paymentAmount"].ObjToDouble();
                    dt.Rows[0]["numMonths"] = paymentdRow["months"].ObjToDouble();
                    dt.Rows[0]["amtOfMonthlyPayt"] = paymentdRow["expected"].ObjToDouble();
                    date = paymentdRow["date_approved"].ObjToDateTime();
                    str = date.ToString("MM/dd/yyyy");
                    if (date.Year <= 100)
                        str = "";
                    dt.Rows[0]["cpApproval"] = str;
                }
            }
            else
            {
                dt.Rows[0]["numMonths"] = 0D;
                dt.Rows[0]["amountPaid"] = 0D;
            }

            //double amountDue = 0D;
            //double months = 0D;

            //GetAmountDue(dt, ref amountDue, ref months);

            //dt.Rows[0]["numMonths"] = months;
            //dt.Rows[0]["amountPaid"] = amountDue;

            G1.NumberDataTable(dt);
            dgv.DataSource = dt;

            // SaveRequest();
        }
        /***********************************************************************************************/
        private void LoadInsurance()
        {
            string cmd = "Select * from `icontracts` x JOIN `icustomers` c ON x.`contractNumber` = c.`contractNumber` WHERE x.`contractNumber` = '" + workContract + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return;

            reinstateRequestNumber = getNextRequestNumber( true );
            this.gridBand7.Caption = "Reinstate Sequence # : " + reinstateRequestNumber;

            this.Text = "Insurance Reinstate Request";
            SetPositions();

            dt.Columns.Add("num");
            dt.Columns.Add("issueDate");
            dt.Columns.Add("payDate");
            dt.Columns.Add("requestDate");
            dt.Columns.Add("fullName");
            dt.Columns.Add("accountNumber");
            dt.Columns.Add("location");
            dt.Columns.Add("cpApproval");
            dt.Columns.Add("contractValue", Type.GetType("System.Double"));
            dt.Columns.Add("grossTrust", Type.GetType("System.Double"));
            dt.Columns.Add("numMonths", Type.GetType("System.Double"));

            DateTime date = dt.Rows[0]["issueDate8"].ObjToDateTime();
            date = DailyHistory.GetIssueDate(date, workContract, dt);
            dt.Rows[0]["issueDate"] = date.ToString("MM/dd/yyyy");

            double contractValue = DailyHistory.GetContractValue(dt.Rows[0]);
            dt.Rows[0]["contractValue"] = contractValue;
            dt.Rows[0]["grossTrust"] = contractValue;

            double financedAmount = DailyHistory.GetFinanceValue(dt.Rows[0]);

            double apr = dt.Rows[0]["APR"].ObjToDouble();
            int numPayments = dt.Rows[0]["numberOfPayments"].ObjToInt32();

//            double amountPaid = GetAmountPaid(workContract, financedAmount, apr, numPayments, date);
            dt.Rows[0]["amountPaid"] = 0D;

            dt.Rows[0]["fullname"] = dt.Rows[0]["firstName"].ObjToString().Trim() + " " + dt.Rows[0]["lastName"].ObjToString().Trim();
            dt.Rows[0]["accountNumber"] = dt.Rows[0]["payer"].ObjToString();
            dt.Rows[0]["requestDate"] = DateTime.Now.ToString("MM/dd/yyyy");

            double expected = dt.Rows[0]["amtOfMonthlyPayt"].ObjToDouble();

            if (paymentdRow != null)
            {
                if (!redo)
                {
                    double payment = workPayments;
                    double months = 0;
                    if (expected > 0D)
                        months = payment / expected;
                    dt.Rows[0]["amountPaid"] = payment;
                    dt.Rows[0]["numMonths"] = months;
                    dt.Rows[0]["payDate"] = paymentdRow["date"].ObjToString();
                }
                else
                {
                    dt.Rows[0]["requestDate"] = paymentdRow["dor"].ObjToString();
                    string str = paymentdRow["dueDate8"].ObjToString();
                    dt.Rows[0]["dueDate8"] = G1.DTtoMySQLDT(str);
                    dt.Rows[0]["accountNumber"] = workContract;
                    dt.Rows[0]["location"] = paymentdRow["location"].ObjToString();
                    dt.Rows[0]["amountPaid"] = paymentdRow["paymentAmount"].ObjToDouble();
                    dt.Rows[0]["numMonths"] = paymentdRow["months"].ObjToDouble();
                    dt.Rows[0]["amtOfMonthlyPayt"] = paymentdRow["expected"].ObjToDouble();
                    date = paymentdRow["date_approved"].ObjToDateTime();
                    str = date.ToString("MM/dd/yyyy");
                    if (date.Year <= 100)
                        str = "";
                    dt.Rows[0]["cpApproval"] = str;
                }
            }
            else
            {
                dt.Rows[0]["numMonths"] = 0D;
                dt.Rows[0]["amountPaid"] = 0D;
            }
            //if (paymentdRow != null)
            //{
            //    numPayments = paymentdRow["monthsPaid"].ObjToInt32();
            //    double payment = paymentdRow["payment"].ObjToDouble();
            //    dt.Rows[0]["amountPaid"] = payment;
            //    dt.Rows[0]["payDate"] = paymentdRow["date"].ObjToString();
            //}
            //dt.Rows[0]["numMonths"] = numPayments;

            //double amountDue = 0D;
            //double months = 0D;

            //GetAmountDue(dt, ref amountDue, ref months);

            //dt.Rows[0]["numMonths"] = months;
            //dt.Rows[0]["amountPaid"] = amountDue;

            G1.NumberDataTable(dt);
            dgv.DataSource = dt;

            //SaveRequest();
        }
        /***********************************************************************************************/
        private void GetAmountDue(DataTable dt, ref double amountDue, ref double months)
        {
            amountDue = 0D;
            months = 0D;
            DateTime date2 = dt.Rows[0]["dueDate8"].ObjToDateTime();
            DateTime date1 = DateTime.Now;
            double expected = dt.Rows[0]["amtOfMonthlyPayt"].ObjToDouble();

            if (!InsuranceContract)
            {
                months = (date1.Year - date2.Year) * 12 + date1.Month - date2.Month + (date1.Day >= date2.Day ? 0 : -1);
                amountDue = months * expected;          
            }
            else
            {
                months = (date1.Year - date2.Year) * 12 + date1.Month - date2.Month + (date1.Day >= date2.Day ? 0 : -1);
                string workPayer = dt.Rows[0]["payer"].ObjToString();
                double premium = Policies.CalcMonthlyPremium(workPayer);
                amountDue = months * expected;
            }
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
            isSavedMaybe = true;
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
            SaveRequest();
            isSaved = true;
        }
        /***********************************************************************************************/
        private void printableComponentLink1_BeforeCreateAreas(object sender, EventArgs e)
        {
        }
        /***********************************************************************************************/
        private void previewPrint (object sender, EventArgs e)
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
            string title = "Trust Reinstate Request";
            if (InsuranceContract)
                title = "Insurance Reinstate Request";
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
                else
                {
                    DateTime date = e.DisplayText.ObjToDateTime();
                    if (date.Year < 100)
                        e.DisplayText = "";
                    else
                        e.DisplayText = date.ToString("MM/dd/yyyy");
                }
            }
        }
        /***********************************************************************************************/
        private string getNextRequestNumber ( bool insurance )
        {
            string request = "";
            string cmd = "Select * from `options` where `option` = 'Trust Reinstate Number';";
            if ( insurance )
                cmd = "Select * from `options` where `option` = 'Insurance Reinstate Number';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return "-1";
            int year = 0;
            int seq = 0;
            request = dt.Rows[0]["answer"].ObjToString();
            string[] Lines = request.Split('-');
            if ( Lines.Length > 1)
            {
                year = Lines[0].ObjToInt32();
                seq = Lines[1].ObjToInt32();
            }
            else
            {
                seq = Lines[0].ObjToInt32();
                if ( seq > 1900)
                {
                    year = DateTime.Now.Year;
                    seq = 0;
                }
            }
            reinstateRequestRecord = dt.Rows[0]["record"].ObjToString();
            if (year < DateTime.Now.Year)
            {
                year = DateTime.Now.Year;
                seq = 1;
            }
            else
                seq++;

            string str = year.ToString("D4") + "-" + seq.ToString();
            nextReinstateNumber = str;
            //G1.update_db_table("options", "record", reinstateRequestRecord, new string[] { "answer", str });
            return request;
        }
        /***********************************************************************************************/
        private void UpdateResuestNumber ()
        {
            string str = nextReinstateNumber;
            G1.update_db_table("options", "record", reinstateRequestRecord, new string[] { "answer", str });
        }
        /***********************************************************************************************/
        private void SaveRequest()
        {
            if (isSaved)
                return;
            DataTable dt = (DataTable)dgv.DataSource;
            string contractNumber = workContract;
            DateTime date = dt.Rows[0]["payDate"].ObjToDateTime();
            string record = "";
            //string cmd = "Select * from `reinstate_requests` where `contractNumber` = '" + workContract + "' and `requestNumber` = '" + reinstateRequestNumber + "';";
            //DataTable dx = G1.get_db_data(cmd);
            //if (dx.Rows.Count > 0)
            //    record = dx.Rows[0]["record"].ObjToString();
            //else
            record = G1.create_record("reinstate_requests", "requestNumber", "-1");
            if (G1.BadRecord("reinstate_request", record))
                return;
            string payDate8 = date.ToString("MM/dd/yyyy");
            date = dt.Rows[0]["dueDate8"].ObjToDateTime();
            string dueDate8 = date.ToString("MM/dd/yyyy");
            string dor = DateTime.Now.ToString("MM/dd/yyyy");
            double amountPaid = dt.Rows[0]["amountPaid"].ObjToDouble();
            string location = dt.Rows[0]["location"].ObjToString();
            double months = dt.Rows[0]["numMonths"].ObjToDouble();
            double expected = dt.Rows[0]["amtOfMonthlyPayt"].ObjToDouble();
            G1.update_db_table("reinstate_requests", "record", record, new string[] { "contractNumber", workContract, "requestNumber", reinstateRequestNumber, "payDate8", payDate8, "paymentAmount", amountPaid.ToString() });
            G1.update_db_table("reinstate_requests", "record", record, new string[] {"dor", dor, "location", location, "dueDate8", dueDate8, "months", months.ToString(), "expected", expected.ToString()});

            isSaved = true;
            UpdateResuestNumber();
        }
        /***********************************************************************************************/
        private void ReinstateRequest_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!redo)
            {
                if (!isSaved)
                {
                    if (isSavedMaybe)
                    {
                        DialogResult result = MessageBox.Show("It looks like you may have printed!\nDo you want to save this Reinstatement Request?", "Save Request Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (result == DialogResult.Yes)
                            SaveRequest();
                    }
                }
            }
            //SaveRequest();
        }
        /***********************************************************************************************/
        private void gridMain_CellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            if (e == null)
                return;
            int rowHandle = gridMain.FocusedRowHandle;
            DataRow dr = gridMain.GetFocusedDataRow();
            if (dr == null)
                return;
            DataTable dt = (DataTable)dgv.DataSource;
            double months = dr["numMonths"].ObjToDouble();
            double expected = dr["amtOfMonthlyPayt"].ObjToDouble();
            double paidAmount = dr["amountPaid"].ObjToDouble();
            if (e.Column.FieldName.Trim().ToUpper() == "NUMMONTHS")
                dr["amountPaid"] = expected * months;
            else if (e.Column.FieldName.Trim().ToUpper() == "AMOUNTPAID")
            {
                if (expected > 0D)
                {
                    double amount = paidAmount / expected;
                    amount = G1.RoundValue(amount);
                    dr["numMonths"] = amount;
                }
            }
        }
        /***********************************************************************************************/
    }
}
