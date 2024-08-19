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
using DevExpress.XtraPrinting;
using DevExpress.Utils;
using MySql.Data.MySqlClient;
using GeneralLib;
using DevExpress.XtraGrid.Views.Grid;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class ExplainTrust85 : DevExpress.XtraEditors.XtraForm
    {
        private DateTime workDate = DateTime.Now;
        private DataTable workDt = null;
        private int workRow = 0;
        private DataTable contractDt = null;
        private string workContract = "";
        private string workName = "";
        private int workMethod = 0;
        /****************************************************************************************/
        public ExplainTrust85(string contract, string name, DataTable dt, int row )
        {
            InitializeComponent();
            workContract = contract;
            workName = name;
            workDt = dt;
            workRow = row;
        }
        /****************************************************************************************/
        private void ExplainTrust85_Load(object sender, EventArgs e)
        {
            contractDt = G1.get_db_data("Select * from `contracts` where `contractNumber` = '" + workContract + "';");
            if ( contractDt.Rows.Count <= 0 )
            {
                MessageBox.Show("***ERROR*** Cannot Find Contract + " + workContract);
                this.Close();
            }
            DataTable dt = null;
            workMethod = workDt.Rows[workRow]["method"].ObjToInt32();
            if (workMethod == 3)
                dt = LoadMethod3();
            else if (workMethod == 2)
                dt = LoadMethod2();
            else
                dt = LoadMethod1();
            dgv.DataSource = dt;
        }
        /****************************************************************************************/
        private DataTable LoadMethod3 ()
        {
            double contractValue = DailyHistory.GetContractValue(contractDt.Rows[0]);
            double originalDownPayment = DailyHistory.GetOriginalDownPayment(contractDt.Rows[0]);
            DateTime issueDate = contractDt.Rows[0]["issueDate8"].ObjToDateTime();
            issueDate = DailyHistory.GetIssueDate(issueDate, workContract, null);
            workDate = workDt.Rows[workRow]["payDate8"].ObjToDateTime();

            double numPayments = contractDt.Rows[0]["numberOfPayments"].ObjToDouble();
            double amtOfMonthlyPayt = contractDt.Rows[0]["amtOfMonthlyPayt"].ObjToDouble();
            double rate = contractDt.Rows[0]["APR"].ObjToDouble();

            double payment = workDt.Rows[workRow]["paymentAmount"].ObjToDouble();
            double debit = workDt.Rows[workRow]["debitAdjustment"].ObjToDouble();
            double credit = workDt.Rows[workRow]["creditAdjustment"].ObjToDouble();

            double interest = workDt.Rows[workRow]["interestPaid"].ObjToDouble();
            double principal = payment - interest + credit - debit;

            double trust85P = 0D;
            double trust100P = 0D;
            double retained = 0D;
            DateTime docp = new DateTime(2020, 1, 1);

            int method = ImportDailyDeposits.CalcTrust85P(docp, amtOfMonthlyPayt, issueDate.ToString("MM/dd/yyyy"), contractValue, originalDownPayment, numPayments, payment, principal, debit, credit, rate, ref trust85P, ref trust100P, ref retained);

            DataTable dt = new DataTable();
            dt.Columns.Add("field");
            dt.Columns.Add("data");

            LoadUpRow(dt, "Contract Number", workContract);
            LoadUpRow(dt, "Issue Date", issueDate.ToString("MM/dd/yyyy"));
            LoadUpRow(dt, "Contract Value", G1.ReformatMoney(contractValue));
            LoadUpRow(dt, "Down Payment", G1.ReformatMoney(originalDownPayment));
            LoadUpRow(dt, "Finance Months", numPayments.ToString());
            LoadUpRow(dt, "APR", G1.ReformatMoney ( rate));
            LoadUpRow(dt, "", "");

            LoadUpRow(dt, "Method", method.ToString());
            LoadUpRow(dt, "Definition", "Contracts BEFORE 12/01/2017");
            LoadUpRow(dt, "Trust100 Formula", "(ContractValue - DownPayment) / FinanceMonths");
            string str = "(" + G1.ReformatMoney ( contractValue).Trim() + " - " + G1.ReformatMoney(originalDownPayment).Trim() + ") / " + numPayments.ToString();
            LoadUpRow(dt, "Trust100", str);
            LoadUpRow(dt, "Trust100 Value", G1.ReformatMoney(trust100P));

            LoadUpRow(dt, "Trust85 Formula", "Trust100 * .85");
            LoadUpRow(dt, "Trust85 Value", G1.ReformatMoney(trust85P));

            return (dt);
        }
        /****************************************************************************************/
        private DataTable LoadMethod2()
        {
            double contractValue = DailyHistory.GetContractValue(contractDt.Rows[0]);
            double originalDownPayment = DailyHistory.GetOriginalDownPayment(contractDt.Rows[0]);
            DateTime issueDate = contractDt.Rows[0]["issueDate8"].ObjToDateTime();
            issueDate = DailyHistory.GetIssueDate(issueDate, workContract, null);
            workDate = workDt.Rows[workRow]["payDate8"].ObjToDateTime();

            double numPayments = contractDt.Rows[0]["numberOfPayments"].ObjToDouble();
            double amtOfMonthlyPayt = contractDt.Rows[0]["amtOfMonthlyPayt"].ObjToDouble();
            double rate = contractDt.Rows[0]["APR"].ObjToDouble();

            double payment = workDt.Rows[workRow]["paymentAmount"].ObjToDouble();
            double debit = workDt.Rows[workRow]["debitAdjustment"].ObjToDouble();
            double credit = workDt.Rows[workRow]["creditAdjustment"].ObjToDouble();

            double interest = workDt.Rows[workRow]["interestPaid"].ObjToDouble();
            double principal = payment - interest + credit - debit;

            double trust85P = 0D;
            double trust100P = 0D;
            double retained = 0D;
            DateTime docp = new DateTime(2020, 1, 1);


            int method = ImportDailyDeposits.CalcTrust85P(docp, amtOfMonthlyPayt, issueDate.ToString("MM/dd/yyyy"), contractValue, originalDownPayment, numPayments, payment, principal, debit, credit, rate, ref trust85P, ref trust100P, ref retained);

            DataTable dt = new DataTable();
            dt.Columns.Add("field");
            dt.Columns.Add("data");

            LoadUpRow(dt, "Contract Number", workContract);
            LoadUpRow(dt, "Issue Date", issueDate.ToString("MM/dd/yyyy"));
            LoadUpRow(dt, "Contract Value", G1.ReformatMoney(contractValue));
            LoadUpRow(dt, "Down Payment", G1.ReformatMoney(originalDownPayment));
            LoadUpRow(dt, "Finance Months", numPayments.ToString());
            LoadUpRow(dt, "APR", G1.ReformatMoney(rate));
            LoadUpRow(dt, "", "");

            //else if (contractDate >= testDate)
            //{ // Contracts after 12/1/2017
            //    principal = G1.RoundValue(principal);
            //    trust85P = principal * .85D;
            //    trust85P = G1.RoundDown(trust85P);
            //    trust100P = principal;
            //    method = 2;
            //    return method;
            //}

            LoadUpRow(dt, "Method", method.ToString());
            LoadUpRow(dt, "Definition", "Contracts AFTER 12/01/2017");
            LoadUpRow(dt, "Trust100 = Principal of ", G1.ReformatMoney(principal));
            string str = G1.ReformatMoney(principal).Trim() + " * 0.85";
            LoadUpRow(dt, "Trust85 Formula", "Trust100 * .85");
            LoadUpRow(dt, "Trust85 Value", G1.ReformatMoney(trust85P));

            return (dt);
        }
        /****************************************************************************************/
        private DataTable LoadMethod1()
        {
            double contractValue = DailyHistory.GetContractValue(contractDt.Rows[0]);
            double originalDownPayment = DailyHistory.GetOriginalDownPayment(contractDt.Rows[0]);
            DateTime issueDate = contractDt.Rows[0]["issueDate8"].ObjToDateTime();
            issueDate = DailyHistory.GetIssueDate(issueDate, workContract, null);
            workDate = workDt.Rows[workRow]["payDate8"].ObjToDateTime();

            double numPayments = contractDt.Rows[0]["numberOfPayments"].ObjToDouble();
            double amtOfMonthlyPayt = contractDt.Rows[0]["amtOfMonthlyPayt"].ObjToDouble();
            double rate = contractDt.Rows[0]["APR"].ObjToDouble();

            double payment = workDt.Rows[workRow]["paymentAmount"].ObjToDouble();
            double debit = workDt.Rows[workRow]["debitAdjustment"].ObjToDouble();
            double credit = workDt.Rows[workRow]["creditAdjustment"].ObjToDouble();

            double interest = workDt.Rows[workRow]["interestPaid"].ObjToDouble();
            double principal = payment - interest + credit - debit;

            double trust85P = 0D;
            double trust100P = 0D;
            double retained = 0D;
            DateTime docp = new DateTime(2020, 1, 1);


            int method = ImportDailyDeposits.CalcTrust85P(docp, amtOfMonthlyPayt, issueDate.ToString("MM/dd/yyyy"), contractValue, originalDownPayment, numPayments, payment, principal, debit, credit, rate, ref trust85P, ref trust100P, ref retained);

            DataTable dt = new DataTable();
            dt.Columns.Add("field");
            dt.Columns.Add("data");

            LoadUpRow(dt, "Contract Number", workContract);
            LoadUpRow(dt, "Issue Date", issueDate.ToString("MM/dd/yyyy"));
            LoadUpRow(dt, "Contract Value", G1.ReformatMoney(contractValue));
            LoadUpRow(dt, "Down Payment", G1.ReformatMoney(originalDownPayment));
            LoadUpRow(dt, "Finance Months", numPayments.ToString());
            LoadUpRow(dt, "APR", G1.ReformatMoney(rate));
            LoadUpRow(dt, "", "");

            //else if (contractDate >= testDate)
            //{ // Contracts after 12/1/2017
            //    principal = G1.RoundValue(principal);
            //    trust85P = principal * .85D;
            //    trust85P = G1.RoundDown(trust85P);
            //    trust100P = principal;
            //    method = 2;
            //    return method;
            //}

            LoadUpRow(dt, "Method", method.ToString());
            if ( rate == 0D)
                LoadUpRow(dt, "Definition", "Contracts with APR = 0");
            if ( credit != 0D)
                LoadUpRow(dt, "Definition", "Credit");
            else if (debit != 0D)
                LoadUpRow(dt, "Definition", "Debit");
            else
                LoadUpRow(dt, "Definition", "??");


            LoadUpRow(dt, "Trust100 = Principal of ", G1.ReformatMoney(principal));
            string str = G1.ReformatMoney(principal).Trim() + " * 0.85";
            LoadUpRow(dt, "Trust85 Formula", "Trust100 * .85");
            LoadUpRow(dt, "Trust85 Value", G1.ReformatMoney(trust85P));

            return (dt);
        }
        /****************************************************************************************/
        private void LoadUpRow ( DataTable dt, string field, string data )
        {
            DataRow dR = dt.NewRow();
            dR["field"] = field; ;
            dR["data"] = data;
            dt.Rows.Add(dR);
        }
        /***********************************************************************************************/
        private int pageMarginLeft = 0;
        private int pageMarginRight = 0;
        private int pageMarginTop = 0;
        private int pageMarginBottom = 0;
        /***********************************************************************************************/
        private void printableComponentLink1_CreateDetailHeaderArea(object sender, CreateAreaEventArgs e)
        {
        }
        /***********************************************************************************************/
        private void printableComponentLink1_CreateMarginalHeader(object sender, CreateAreaEventArgs e)
        {
            Printer.setupPrinterQuads(e, 2, 3);
            Font font = new Font("Ariel", 16);
            Printer.DrawQuad(1, 1, Printer.xQuads, 2, "South Mississippi Funeral Services, LLC", Color.Black, BorderSide.Top, font, HorizontalAlignment.Center);

            Printer.SetQuadSize(12, 12);

            font = new Font("Ariel", 8);
            Printer.DrawGridDate(2, 1, 2, 3, Color.Black, BorderSide.None, font);
            Printer.DrawGridPage(11, 1, 2, 3, Color.Black, BorderSide.None, font);

            Printer.DrawQuad(1, 3, 2, 2, "User : " + LoginForm.username, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);

            font = new Font("Ariel", 10, FontStyle.Bold);
            Printer.DrawQuad(6, 10, 4, 2, "Trust Calculation Explained", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            //font = new Font("Ariel", 7, FontStyle.Regular);
            //Printer.DrawQuad(1, 11, 4, 1, "Contract :" + workContract, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(3, 11, 3, 1, "Name :" + workName, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            Printer.DrawQuadBorder(1, 1, 12, 5, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 6, BorderSide.Right, 1, Color.Black);

            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /****************************************************************************************/
        private void printPreviewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.components == null)
                this.components = new System.ComponentModel.Container();
            DevExpress.XtraPrinting.PrintingSystem printingSystem1 = new DevExpress.XtraPrinting.PrintingSystem(this.components);
            DevExpress.XtraPrinting.PrintableComponentLink printableComponentLink1 = new DevExpress.XtraPrinting.PrintableComponentLink(this.components);

            printingSystem1.Links.AddRange(new object[] {
            printableComponentLink1});

            printableComponentLink1.Component = dgv;
            printableComponentLink1.PrintingSystemBase = printingSystem1;
            printableComponentLink1.CreateDetailHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateDetailHeaderArea);
            printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeader);
            printableComponentLink1.Landscape = false;

            Printer.setupPrinterMargins(50, 50, 150, 50);

            pageMarginLeft = Printer.pageMarginLeft;
            pageMarginRight = Printer.pageMarginRight;
            pageMarginTop = Printer.pageMarginTop;
            pageMarginBottom = Printer.pageMarginBottom;

            printableComponentLink1.Margins.Left = pageMarginLeft;
            printableComponentLink1.Margins.Right = pageMarginRight;
            printableComponentLink1.Margins.Top = pageMarginTop;
            printableComponentLink1.Margins.Bottom = pageMarginBottom;

            printableComponentLink1.CreateDocument();
            printableComponentLink1.ShowPreviewDialog();
        }
        /****************************************************************************************/
        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.components == null)
                this.components = new System.ComponentModel.Container();
            DevExpress.XtraPrinting.PrintingSystem printingSystem1 = new DevExpress.XtraPrinting.PrintingSystem(this.components);
            DevExpress.XtraPrinting.PrintableComponentLink printableComponentLink1 = new DevExpress.XtraPrinting.PrintableComponentLink(this.components);

            printingSystem1.Links.AddRange(new object[] {
            printableComponentLink1});

            printableComponentLink1.Component = dgv;
            printableComponentLink1.PrintingSystemBase = printingSystem1;
            printableComponentLink1.CreateDetailHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateDetailHeaderArea);
            printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeader);

            printableComponentLink1.Landscape = false;

            Printer.setupPrinterMargins(50, 100, 150, 50);

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
        /****************************************************************************************/
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        /****************************************************************************************/
    }
}