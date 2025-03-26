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
    public partial class ReinstateReport : DevExpress.XtraEditors.XtraForm
    {
        private string workContract = "";
        private string workPayer = "";
        private bool insurance = false;
        private bool workLapsed = false;
        /***********************************************************************************************/
        public ReinstateReport( string contract, bool lapsed = false )
        {
            InitializeComponent();
            workContract = contract;
            workLapsed = lapsed;
        }
        /***********************************************************************************************/
        private void ReinstateReport_Load(object sender, EventArgs e)
        {
            dgv2.Dock = DockStyle.Fill;
            dgv.Dock = DockStyle.Fill;
            if (DailyHistory.isInsurance(workContract))
                FormatForInsurance();
            else
                FormatForTrusts();
        }
        /***********************************************************************************************/
        private void FormatForInsurance()
        {
            dgv2.Visible = true;
            dgv.Visible = false;
            insurance = true;
            DataTable mainDt = new DataTable();
            mainDt.Columns.Add("payer");
            mainDt.Columns.Add("policy");
            mainDt.Columns.Add("lastname");
            mainDt.Columns.Add("firstname");
            mainDt.Columns.Add("premium", Type.GetType("System.Double"));
            mainDt.Columns.Add("lDate");
            mainDt.Columns.Add("bDate");

            string cmd = "Select * from `icustomers` where `contractNumber` = '" + workContract + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return;

            workPayer = dt.Rows[0]["payer"].ObjToString();
            string name = dt.Rows[0]["firstName"].ObjToString() + " " + dt.Rows[0]["lastName"].ObjToString();
            this.Text = "Reinstate Report for (" + workPayer + ") " + name;
            if (workLapsed)
                this.Text = "Lapse Report for (" + workPayer + ") " + name;

            string message = "Payer (" + workPayer + ") has been Reinstated.\n\n";
//            message = "Payer Name : " + name + ".\n";

            DateTime birthDate = dt.Rows[0]["birthDate"].ObjToDateTime();
            message = workPayer + "  " + dt.Rows[0]["lastName"].ObjToString() + "  " + dt.Rows[0]["firstName"].ObjToString() + birthDate.ToString("MM/dd/yyyy") + "\n";

            DataRow dR = mainDt.NewRow();
            dR["payer"] = workPayer;
            dR["policy"] = dt.Rows[0]["lastName"].ObjToString();
            dR["lastname"] = dt.Rows[0]["firstName"].ObjToString();
            dR["firstname"] = dt.Rows[0]["agentCode"].ObjToString();
//            dR["bDate"] = birthDate.ToString("MM/dd/yyyy");
            mainDt.Rows.Add(dR);

            double monthlyPremium = 0D;
            cmd = "Select * from `policies` where `payer` = '" + workPayer + "';";
            dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return;

            DateTime deceasedDate = DateTime.Now;
            DateTime lapseDate8 = DateTime.Now;
            DateTime reinstateDate8 = DateTime.Now;
            double premium = 0D;
            string firstName = "";
            string lastName = "";
            string policy = "";
            string str = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                deceasedDate = dt.Rows[i]["deceasedDate"].ObjToDateTime();
                if (deceasedDate.Year > 1800)
                    continue;
                lapseDate8 = dt.Rows[i]["lapsedDate8"].ObjToDateTime();
                if (lapseDate8.Year > 1800)
                {
                    if ( !workLapsed)
                        continue;
                }
                premium = dt.Rows[i]["premium"].ObjToDouble();
                policy = dt.Rows[i]["policyNumber"].ObjToString();
                firstName = dt.Rows[i]["policyFirstName"].ObjToString();
                lastName = dt.Rows[i]["policyLastName"].ObjToString();
                birthDate = dt.Rows[i]["birthDate"].ObjToDateTime();
                str = G1.ReformatMoney(premium);
                message += "                " + policy + "   " + lastName + "  " + firstName + "  " + str + "   " + birthDate.ToString("MM/dd/yyyy") + "\n";
                if ( lapseDate8.Year < 1800 )
                    monthlyPremium += premium;
                dR = mainDt.NewRow();
                dR["policy"] = policy;
                dR["lastname"] = lastName;
                dR["firstname"] = firstName;
                dR["premium"] = premium;
                if ( birthDate.Year > 1800)
                    dR["bDate"] = birthDate.ToString("MM/dd/yyyy");
                if ( lapseDate8.Year > 1800)
                    dR["lDate"] = lapseDate8.ToString("MM/dd/yyyy");
                mainDt.Rows.Add(dR);
            }
            dR = mainDt.NewRow();
            dR["premium"] = monthlyPremium;
            mainDt.Rows.Add(dR);
            monthlyPremium = G1.RoundDown(monthlyPremium);
            str = G1.ReformatMoney(monthlyPremium);
            message += "                                                 " + str + "\n";
            if (!workLapsed)
                gridMain2.Columns["lDate"].Visible = false;
            //dt = new DataTable();
            //dt.Columns.Add("desc");
            //string[] Lines = message.Split('\n');
            //for (int i = 0; i < Lines.Length; i++)
            //{
            //    message = Lines[i];
            //    DataRow dRow = dt.NewRow();
            //    dRow["desc"] = message;
            //    dt.Rows.Add(dRow);
            //}
            dgv2.DataSource = mainDt;

        }
        /***********************************************************************************************/
        private void FormatForTrusts()
        {
            dgv2.Visible = false;
            dgv.Visible = true;

            string message = "Contract (" + workContract + ") has been Reinstated.\n";
            if ( workLapsed )
                message = "Contract (" + workContract + ") has Lapsed.\n";
            string name = "";

            string cmd = "Select * from `customers` where `contractNumber` = '" + workContract + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                name = dx.Rows[0]["firstName"].ObjToString() + " " + dx.Rows[0]["lastName"].ObjToString();
                message = "Customer Name : " + name + ".\n\n";
            }

            this.Text = "Reinstate Report for (" + workContract + ") " + name;
            if ( workLapsed )
                this.Text = "Lapse Report for (" + workContract + ") " + name;

            string paymentsFile = "payments";

            cmd = "Select * from `contracts` where `contractNumber` = '" + workContract + "';";
            dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                DateTime dueDate = dx.Rows[0]["dueDate8"].ObjToDateTime();
                double payment = dx.Rows[0]["amtOfMonthlyPayt"].ObjToDouble();
                double balance = dx.Rows[0]["balanceDue"].ObjToDouble();
                DateTime fromDate = dx.Rows[0]["lastDatePaid8"].ObjToDateTime();
                DateTime lapseDate = dx.Rows[0]["lapseDate8"].ObjToDateTime();
                DateTime reinstateDate = dx.Rows[0]["reinstateDate8"].ObjToDateTime();
                string lapse = dx.Rows[0]["lapsed"].ObjToString().ToUpper();
//                DateTime toDate = dr["date"].ObjToDateTime();
                message += "Current balance is $" + G1.ReformatMoney(balance) + " and their last payment was made on " + fromDate.ToString("MM/dd/yyyy") + ".\n\n";
                double contractValue = DailyHistory.GetContractValue(workContract);
                double downPayment = dx.Rows[0]["downPayment"].ObjToDouble();
                message += "Original Contract Value was $" + G1.ReformatMoney(contractValue) + " and their down payment was $" + G1.ReformatMoney(downPayment) + ".\n\n";
                dx = G1.get_db_data("Select * from `" + paymentsFile + "` where `contractNumber` = '" + workContract + "' order by `payDate8` DESC;");
                ManualPayment.CleanupWork(dx);
                int payments = dx.Rows.Count;
                string word = " payments";
                if (payments == 1)
                    word = " payment";

                message += "Customer has made " + payments.ToString() + word + " with $" + G1.ReformatMoney(payment) + " as the monthly payment.\n\n";
                message += "Current Due Date is " + dueDate.ToString("MM/dd/yyyy") + "\n\n";

                if (reinstateDate.Year > 500)
                    message += "Customer reinstated contract on " + reinstateDate.ToString("MM/dd/yyyy") + ".\n\n";
                if ((lapse == "Y" || workLapsed) && lapseDate.Year > 500 )
                    message += "Customer lapsed contract on " + lapseDate.ToString("MM/dd/yyyy") + ".\n\n";
            }
            dx = new DataTable();
            dx.Columns.Add("desc");
            string[] Lines = message.Split('\n');
            for ( int i=0; i<Lines.Length; i++)
            {
                message = Lines[i];
                DataRow dRow = dx.NewRow();
                dRow["desc"] = message;
                dx.Rows.Add(dRow);
            }
            dgv.DataSource = dx;
        }
        /***********************************************************************************************/
        private int pageMarginLeft = 0;
        private int pageMarginRight = 0;
        private int pageMarginTop = 0;
        private int pageMarginBottom = 0;
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
            if ( dgv2.Visible )
                printableComponentLink1.Component = dgv2;

            printableComponentLink1.PrintingSystemBase = printingSystem1;
            printableComponentLink1.CreateDetailHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateDetailHeaderArea);
            printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);
            printableComponentLink1.BeforeCreateAreas += new System.EventHandler(this.printableComponentLink1_BeforeCreateAreas);
            printableComponentLink1.AfterCreateAreas += new System.EventHandler(this.printableComponentLink1_AfterCreateAreas);
            this.gridMain.BeforePrintRow += new DevExpress.XtraGrid.Views.Base.BeforePrintRowEventHandler(this.gridMain_BeforePrintRow);
            this.gridMain.AfterPrintRow += new DevExpress.XtraGrid.Views.Base.AfterPrintRowEventHandler(this.gridMain_AfterPrintRow);

            printableComponentLink1.Landscape = false;

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
        }
        /****************************************************************************************/
        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.components == null)
                    this.components = new System.ComponentModel.Container();
                DevExpress.XtraPrinting.PrintingSystem printingSystem1 = new DevExpress.XtraPrinting.PrintingSystem(this.components);
                DevExpress.XtraPrinting.PrintableComponentLink printableComponentLink1 = new DevExpress.XtraPrinting.PrintableComponentLink(this.components);

                printingSystem1.Links.AddRange(new object[] {
            printableComponentLink1});

                printableComponentLink1.Component = dgv;
                if (dgv2.Visible)
                    printableComponentLink1.Component = dgv2;

                printableComponentLink1.PrintingSystemBase = printingSystem1;
                printableComponentLink1.CreateDetailHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateDetailHeaderArea);
                printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);
                printableComponentLink1.BeforeCreateAreas += new System.EventHandler(this.printableComponentLink1_BeforeCreateAreas);
                printableComponentLink1.AfterCreateAreas += new System.EventHandler(this.printableComponentLink1_AfterCreateAreas);
                this.gridMain.BeforePrintRow += new DevExpress.XtraGrid.Views.Base.BeforePrintRowEventHandler(this.gridMain_BeforePrintRow);
                this.gridMain.AfterPrintRow += new DevExpress.XtraGrid.Views.Base.AfterPrintRowEventHandler(this.gridMain_AfterPrintRow);

                printableComponentLink1.Landscape = false;

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
                printableComponentLink1.PrintDlg();
            }
            catch (Exception ex)
            {
            }
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

            font = new Font("Ariel", 6);
            Printer.DrawGridDate(2, 3, 2, 3, Color.Black, BorderSide.None, font);
            Printer.DrawGridPage(11, 3, 2, 3, Color.Black, BorderSide.None, font);

            //            Printer.DrawQuad(1, 9, 2, 3, "User : " + LoginForm.username, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);

            font = new Font("Ariel", 10, FontStyle.Bold);

            string report = "Reinstatement Report";
            if (workLapsed)
                report = "Lapse Report";
            if (insurance)
            {
                report = "Policy Reinstatement Report";
                if ( workLapsed )
                    report = "Policy Lapse Report";
            }
            Printer.DrawQuad(5, 8, 5, 4, report, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);


            //            Printer.DrawQuadTicks();
            Printer.SetQuadSize(24, 12);
            font = new Font("Ariel", 7, FontStyle.Regular);
            //Printer.DrawQuad(16, 7, 5, 2, lblBalance.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Top);
            //Printer.DrawQuad(16, 10, 5, 2, lblPayment.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            font = new Font("Ariel", 8);
            string search = "Contract : " + workContract;
            if (insurance)
                search = "Payer : " + workPayer;
            Printer.DrawQuad(1, 6, 6, 3, search, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Top);
            Printer.DrawQuad(1, 9, 6, 3, "User : " + LoginForm.username, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);

            //DateTime date = this.dateTimePicker2.Value;
            //date = date.AddDays(1);
            //string workDate = date.ToString("MM/dd/yyyy");
            //Printer.SetQuadSize(24, 12);
            //font = new Font("Ariel", 9, FontStyle.Bold);
            //Printer.DrawQuad(20, 8, 5, 4, "Report Ending: " + workDate, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /****************************************************************************************/
        private int footerCount = 0;
        private void gridMain_BeforePrintRow(object sender, DevExpress.XtraGrid.Views.Printing.CancelPrintRowEventArgs e)
        {
            if (e.HasFooter)
            {
                footerCount++;
            }
        }
        /****************************************************************************************/
        private void gridMain_AfterPrintRow(object sender, DevExpress.XtraGrid.Views.Printing.PrintRowEventArgs e)
        {
            if (e.HasFooter)
            {
                footerCount++;
                if (footerCount >= 1)
                {
                    footerCount = 0;
                }
            }
        }
        /****************************************************************************************/
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        /***********************************************************************************************/
    }
}