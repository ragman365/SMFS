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
    public partial class PayerDeceased : DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        public PayerDeceased()
        {
            InitializeComponent();
        }
        /***********************************************************************************************/
        private void PayerDeceased_Load(object sender, EventArgs e)
        {
            labelMaximum.Hide();
            lblTotal.Hide();
            barImport.Hide();
        }
        /***********************************************************************************************/
        Import importForm = null;
        private void btnSelectFile_Click(object sender, EventArgs e)
        {
            importForm = new Import();
            importForm.SelectDone += ImportForm_SelectDone;
            importForm.Show();
        }
        /***********************************************************************************************/
        private void ImportForm_SelectDone(DataTable dt)
        {
            if (dt == null)
                return;
            if (dt.Rows.Count <= 0)
                return;
            if (importForm != null)
                importForm.Close();
            barImport.Show();

            string payerNumber = "";
            string contractNumber = "";
            string cmd = "";
            DataTable dx = null;
            double ExpectedPayments = 0D;
            DateTime iDate = DateTime.Now;
            DateTime lastDate = DateTime.Now;
            DateTime bDate = DateTime.Now;
            DateTime dDate = DateTime.Now;
            DateTime issueDate = DateTime.Now;
            DateTime lapseDate = DateTime.Now;
            DateTime reinstateDate = DateTime.Now;
            double dPayments = 0D;
            int numberPayments = 0;
            string str = "";
            int lastrow = dt.Rows.Count;
            this.Cursor = Cursors.WaitCursor;
            dt.Columns.Add("contractNumber");
            dt.Columns.Add("lastDatePaid8");
            dt.Columns.Add("issueDate8");
            dt.Columns.Add("dueDate8");
            dt.Columns.Add("lapseDate8");
            dt.Columns.Add("reinstateDate8");
            dt.Columns.Add("bDate");
            dt.Columns.Add("dDate");
            dt.Columns.Add("lastPayment", Type.GetType("System.Double"));

            try
            {
                lblTotal.Show();

                lblTotal.Text = "of " + lastrow.ToString();
                lblTotal.Refresh();

                barImport.Minimum = 0;
                barImport.Maximum = lastrow;
                labelMaximum.Show();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    barImport.Value = i;
                    barImport.Refresh();
                    labelMaximum.Text = i.ToString();
                    labelMaximum.Refresh();

                    payerNumber = dt.Rows[i]["PAYER#"].ObjToString();
                    if (String.IsNullOrWhiteSpace(payerNumber))
                        continue;
                    payerNumber = payerNumber.TrimStart('0');
                    cmd = "Select * from `icustomers` where `payer` = '" + payerNumber + "';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                    {
                        contractNumber = dx.Rows[0]["contractNumber"].ObjToString();
                        dt.Rows[i]["contractNumber"] = contractNumber;
                        bDate = dx.Rows[0]["birthDate"].ObjToDateTime();
                        if (bDate.Year > 100)
                            dt.Rows[i]["bDate"] = bDate.ToString("MM/dd/yyyy");
                        dDate = dx.Rows[0]["deceasedDate"].ObjToDateTime();
                        if (dDate.Year > 100)
                            dt.Rows[i]["dDate"] = dDate.ToString("MM/dd/yyyy");
                        cmd = "Select * from `icontracts` where `contractNumber` = '" + contractNumber + "';";
                        dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count > 0)
                        {
                            if (dDate.Year < 100)
                            {
                                dDate = dx.Rows[0]["deceasedDate"].ObjToDateTime();
                                if (dDate.Year > 100)
                                    dt.Rows[i]["dDate"] = dDate.ToString("MM/dd/yyyy");
                            }
                            issueDate = dx.Rows[0]["issueDate8"].ObjToDateTime();
                            if (issueDate.Year > 100)
                                dt.Rows[i]["issueDate8"] = issueDate.ToString("MM/dd/yyyy");
                            lapseDate = dx.Rows[0]["lapseDate8"].ObjToDateTime();
                            if (lapseDate.Year > 100)
                                dt.Rows[i]["lapseDate8"] = lapseDate.ToString("MM/dd/yyyy");
                            reinstateDate = dx.Rows[0]["reinstateDate8"].ObjToDateTime();
                            if (reinstateDate.Year > 100)
                                dt.Rows[i]["reinstateDate8"] = reinstateDate.ToString("MM/dd/yyyy");
                            ExpectedPayments = dx.Rows[0]["amtOfMonthlyPayt"].ObjToDouble();
                            dx = LoadMainData(contractNumber, payerNumber, ExpectedPayments);
                            if (dx.Rows.Count > 0)
                            {
                                for (int j = 0; j < dx.Rows.Count; j++)
                                {
                                    if (dx.Rows[j]["fill"].ObjToString().ToUpper() == "D")
                                        continue;
                                    iDate = dx.Rows[j]["payDate8"].ObjToDateTime();
                                    if (iDate.Year > 100)
                                    {
                                        dt.Rows[i]["lastDatePaid8"] = iDate.ToString("MM/dd/yyyy");
                                        dPayments = dx.Rows[j]["NumPayments"].ObjToDouble();
                                        str = dPayments.ToString();
                                        str = G1.TrimDecimals(str);
                                        numberPayments = str.ObjToInt32();
                                        lastDate = dt.Rows[i]["dueDate8"].ObjToDateTime();
                                        iDate = lastDate.AddMonths(numberPayments);
                                        if ( iDate.Year > 200)
                                            dt.Rows[i]["dueDate8"] = iDate.ToString("MM/dd/yyyy");
                                        dPayments = dx.Rows[j]["paymentAmount"].ObjToDouble();
                                        dt.Rows[i]["lastPayment"] = dPayments;
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
                barImport.Value = lastrow;
                barImport.Refresh();
                labelMaximum.Text = lastrow.ToString();
            }
            catch ( Exception ex)
            {

            }
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private DataTable LoadMainData(string workContract, string workPayer, double ExpectedPayment)
        {
            string cmd = "Select * from `ipayments` where `contractNumber` = '" + workContract + "' order by `payDate8` DESC, `tmstamp` DESC;";
            if (!String.IsNullOrWhiteSpace(workPayer))
            {
                string ccd = "SELECT * from `icustomers` where `payer`= '" + workPayer + "';";
                DataTable ddx = G1.get_db_data(ccd);
                if (ddx.Rows.Count > 0)
                {
                    string list = "";
                    for (int i = 0; i < ddx.Rows.Count; i++)
                    {
                        string contract = ddx.Rows[i]["contractNumber"].ObjToString();
                        list += "'" + contract + "',";
                    }
                    list = list.TrimEnd(',');
                    list = "(" + list + ")";
                    cmd = "Select * from `ipayments` where `contractNumber` IN " + list + " order by `payDate8` DESC, `tmstamp` DESC;";
                }
            }
            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("num");
            dt.Columns.Add("balance", Type.GetType("System.Double"));
            dt.Columns.Add("debit", Type.GetType("System.Double"));
            dt.Columns.Add("credit", Type.GetType("System.Double"));
            dt.Columns.Add("prince", Type.GetType("System.Double"));
            dt.Columns.Add("nextDueDate");
            dt.Columns.Add("creditBalance", Type.GetType("System.Double"));
            dt.Columns.Add("reason");
            dt.Columns.Add("NumPayments", Type.GetType("System.Double"));
            DateTime dueDate = DateTime.Now;
            DateTime payDate = DateTime.Now;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dueDate = dt.Rows[i]["dueDate8"].ObjToDateTime();
                if (dueDate.Year.ToString("D4").IndexOf("000") >= 0)
                    dt.Rows[i]["dueDate8"] = dt.Rows[i]["payDate8"];
            }

            //            double sBalance = startBalance;
            string status = "";
            bool deleted = false;
            double NumPayments = 0D;
            double payment = 0D;
            double debit = 0D;
            double credit = 0D;
            double interest = 0D;
            double principal = 0D;
            double balance = 0D;
            string reason = "";
            string edited = "";
            DateTime pDate = DateTime.Now;
            DateTime dDate = DateTime.Now;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                payment = dt.Rows[i]["paymentAmount"].ObjToString().ObjToDouble();
                NumPayments = 0D;
                if (ExpectedPayment > 0D)
                {
                    NumPayments = payment / ExpectedPayment;
                    if (!String.IsNullOrWhiteSpace(workPayer))
                    {
                        pDate = dt.Rows[i]["payDate8"].ObjToDateTime();
                        dDate = dt.Rows[i]["dueDate8"].ObjToDateTime();
                        double months = DailyHistory.CheckMonthsForInsurance(workContract, workPayer, ExpectedPayment, payment, pDate, dDate );
                        NumPayments = months;
                        //nextDueDate = dueDate.ObjToDateTime();
                        //int imonths = (int)months;
                        //nextDueDate = nextDueDate.AddMonths(imonths);
                    }
                }
                dt.Rows[i]["NumPayments"] = NumPayments;
                debit = dt.Rows[i]["debitAdjustment"].ObjToString().ObjToDouble();
                credit = dt.Rows[i]["creditAdjustment"].ObjToString().ObjToDouble();
                status = dt.Rows[i]["fill"].ObjToString();
                if (status.ToUpper() == "D")
                    deleted = true;
                //if (payment == 0D && debit == 0D && credit == 0D)
                //    dt.Rows.RemoveAt(i);
                break;
            }
            return dt;
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
            Printer.setupPrinterQuads(e, 2, 3);
            Font font = new Font("Ariel", 16);
            Printer.DrawQuad(1, 1, Printer.xQuads, 2, "South Mississippi Funeral Services, LLC", Color.Black, BorderSide.Top, font, HorizontalAlignment.Center);

            Printer.SetQuadSize(12, 12);

            font = new Font("Ariel", 8);
            Printer.DrawGridDate(2, 3, 2, 3, Color.Black, BorderSide.None, font);
            Printer.DrawGridPage(11, 3, 2, 3, Color.Black, BorderSide.None, font);

            Printer.DrawQuad(1, 9, 2, 3, "User : " + LoginForm.username, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);

            font = new Font("Ariel", 10, FontStyle.Regular);
            Printer.DrawQuad(4, 8, 7, 4, "Payer/Deceased Report", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

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
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.gridMain.OptionsFind.AlwaysVisible == true)
                    gridMain.OptionsFind.AlwaysVisible = false;
                else
                    gridMain.OptionsFind.AlwaysVisible = true;
            }
            catch (Exception ex)
            {
            }
        }
        /***********************************************************************************************/
    }
}