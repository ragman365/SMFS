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
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class ShowBCSImport : DevExpress.XtraEditors.XtraForm
    {
        private string workTable = "";
        private string workColumns = "";
        private bool modified = false;
        private DataTable workDt = null;
        private DateTime importDate = DateTime.Now;
        /****************************************************************************************/
        public ShowBCSImport( DataTable dt, DateTime date )
        {
            InitializeComponent();
            workDt = dt;
            importDate = date;
        }
        /****************************************************************************************/
        private void ShowBCSImport_Load(object sender, EventArgs e)
        {
            this.Text = "BCS Import Bank Deposits for Date " + importDate.ToString("MM/dd/yyyy");

            G1.NumberDataTable(workDt);
            dgv.DataSource = workDt;
        }
        /****************************************************************************************/
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            modified = true;
        }
        /****************************************************************************************/
        private void gridMain_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt == null)
                return;
            //string delete = dt.Rows[row]["mod"].ObjToString();
            //if (delete.ToUpper() == "D")
            //{
            //    e.Visible = false;
            //    e.Handled = true;
            //}
        }
        /****************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        { // Spy Glass
            if (gridMain.OptionsFind.AlwaysVisible == true)
                gridMain.OptionsFind.AlwaysVisible = false;
            else
                gridMain.OptionsFind.AlwaysVisible = true;
        }
        /****************************************************************************************/
        private void gridMain_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
        {
            if (e.Column.FieldName.ToUpper() == "NUM")
            {
                if (e.RowHandle >= 0)
                {
                    string num = (e.RowHandle + 1).ToString();
                    e.DisplayText = num;
                }
            }
        }
        /****************************************************************************************/
        private void EditTable_FormClosing(object sender, FormClosingEventArgs e)
        {
        }
        /***********************************************************************************************/
        private int pageMarginLeft = 0;
        private int pageMarginRight = 0;
        private int pageMarginTop = 0;
        private int pageMarginBottom = 0;
        /***********************************************************************************************/
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

            printableComponentLink1.EnablePageDialog = true;

            printableComponentLink1.CreateDetailHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateDetailHeaderArea);
            printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);
            printableComponentLink1.BeforeCreateAreas += new System.EventHandler(this.printableComponentLink1_BeforeCreateAreas);
            printableComponentLink1.AfterCreateAreas += new System.EventHandler(this.printableComponentLink1_AfterCreateAreas);
            printableComponentLink1.Landscape = false;

            Printer.setupPrinterMargins(50, 50, 80, 50);

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
            printableComponentLink1.ShowPreview();
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
            printableComponentLink1.Landscape = false;

            Printer.setupPrinterMargins(50, 50, 80, 50);

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
            Printer.DrawQuad(5, 8, 6, 4, this.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);


            //            Printer.DrawQuadTicks();
            DateTime date = DateTime.Now;
            string workDate = date.Month.ToString("D2") + "/" + date.Year.ToString("D4");
            Printer.SetQuadSize(24, 12);
            font = new Font("Ariel", 9, FontStyle.Regular);

            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /****************************************************************************************/
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        /****************************************************************************************/
        private void btnImport_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt == null)
                return;
            if (dt.Rows.Count <= 0)
                return;

            DataRow dRow = null;

            DateTime date = DateTime.Now;
            DateTime depositDate = DateTime.MinValue;

            string receivedFrom = "";
            string customer = "";
            string contractNumber = "";
            string principal = "";
            string income = "";
            string total = "";
            string comment = "";

            string str = "";
            DataTable ddt = null;
            DataTable ddx = null;

            string localDescription = "BCS - Trust Death Claims";

            string cmd = "Select * from `bank_accounts` where `localDescription` = '" + localDescription + "';";
            ddt = G1.get_db_data(cmd);
            if (ddt.Rows.Count <= 0)
            {
                MessageBox.Show("*** ERROR *** Cannot locate BCS - Trust Death Claims Bank Account", "Bank Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }

            string bankAccount = ddt.Rows[0]["account_no"].ObjToString();

            string trust = "";
            string paymentRecord = "";
            string record = "";

            double growth = 0D;
            double discount = 0D;
            double money = 0D;

            bool badData = false;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                    total = dt.Rows[i]["total"].ObjToString();
                    comment = dt.Rows[i]["comment"].ObjToString();
                    receivedFrom = dt.Rows[i]["receivedFrom"].ObjToString();
                    str = dt.Rows[i]["income"].ObjToString();

                    growth = 0D;
                    discount = 0D;
                    money = str.ObjToDouble();
                    if (money > 0D)
                        growth = money;
                    else
                        discount = money;

                    cmd = "Select * from `cust_payments` where `contractNumber` = '" + contractNumber + "' AND `trust_policy` = '" + contractNumber + "';";
                    ddt = G1.get_db_data(cmd);
                    if (ddt.Rows.Count <= 0)
                    {
                        dt.Rows[i]["status"] = "NOT FOUND";
                        badData = true;
                        continue;
                    }
                    record = ddt.Rows[0]["record"].ObjToString();
                    cmd = "Select * from `cust_payment_details` where `paymentRecord` = '" + record + "';";
                    ddx = G1.get_db_data(cmd);
                    if (ddx.Rows.Count <= 0)
                    {
                        dt.Rows[i]["status"] = "Payment Detail Not Found";
                        badData = true;
                        continue;
                    }
                    paymentRecord = ddx.Rows[0]["record"].ObjToString();
                    G1.update_db_table("cust_payment_details", "record", paymentRecord, new string[] { "dateReceived", date.ToString("yyyy-MM-dd"), "status", "Deposited", "amtActuallyReceived", total, "depositNumber", "TR" + date.ToString("yyyyMMdd") + "_" + (i + 1).ToString(), "paidFrom", receivedFrom, "bankAccount", bankAccount, "localDescription", localDescription, "growth", growth.ToString(), "discount", discount.ToString() });

                    G1.update_db_table("cust_payments", "record", record, new string[] { "status", "Deposited", "depositNumber", "TR" + date.ToString("yyyyMMdd") + "_" + (i + 1).ToString(), "amountReceived", total, "dateModified", DateTime.Now.ToString("yyyy-MM-dd"), "amountGrowth", growth.ToString(), "amountDiscount", discount.ToString(), "grossAmountReceived", total });

                    RecalcExtended(contractNumber);

                    dt.Rows[i]["status"] = "IMPORTED";
                }
                catch ( Exception ex )
                {
                    dt.Rows[i]["status"] = "BAD IMPORT";
                    MessageBox.Show("*** ERROR *** Importing Row (" + (i + 1).ToString() + "!\n" + ex.Message.ToString(), "Import Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    break;
                }
            }

            dgv.DataSource = dt;
            gridMain.RefreshData();
            gridMain.RefreshEditor(true);
            dgv.Refresh();
            this.Refresh();
        }
        /***********************************************************************************************/
        private void RecalcExtended(string contractNumber)
        {
            string extendedRecord = "";
            string record = "";

            string cmd = "SELECT *, (SELECT `pdfimages`.`record` FROM `pdfimages` WHERE `pdfimages`.`contractNumber` = e.`contractNumber` ) AS `picRecord` FROM `fcust_extended` e LEFT JOIN `fcontracts` p ON p.`contractNumber` = e.`contractNumber` left join `fcustomers` d ON e.`contractNumber` = d.`contractNumber` WHERE e.`contractNumber` = '" + contractNumber + "';";
            //cmd += " AND e.`record` = '" + record + "';";

            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                extendedRecord = dx.Rows[0]["record"].ObjToString();
                record = extendedRecord;
                Funerals.CalculateCustomerDetails(contractNumber, record, dx.Rows[0]);
            }
        }
        /****************************************************************************************/
    }
}