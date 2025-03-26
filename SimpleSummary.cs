using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraPrinting;
using DevExpress.Utils;
using MySql.Data.MySqlClient;
using GeneralLib;
using DevExpress.XtraRichEdit.API.Native;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class SimpleSummary : DevExpress.XtraEditors.XtraForm
    {
        /****************************************************************************************/
        private bool first = true;
        private bool continuousPrint = false;
        private DevExpress.XtraRichEdit.RichEditControl rtb2 = new DevExpress.XtraRichEdit.RichEditControl();
        private string workContract = "";
        /****************************************************************************************/
        public SimpleSummary( string cnum)
        {
            workContract = cnum;
            InitializeComponent();
        }
        /****************************************************************************************/
        private void SimpleSummary_Load(object sender, EventArgs e)
        {
            gridMain.OptionsView.ShowFooter = false;
            LoadData();
        }
        /****************************************************************************************/
        private void LoadData()
        {
            G1.SetupVisibleColumns(gridMain, this.columnsToolStripMenuItem, nmenu_Click);
            RunLoad();
        }
        /***********************************************************************************************/
        void nmenu_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menu = (ToolStripMenuItem)sender;
            string name = menu.Name;
            int index = getGridColumnIndex(name);
            if (index < 0)
                return;
            if (menu.Checked)
            {
                menu.Checked = false;
                gridMain.Columns[index].Visible = false;
            }
            else
            {
                menu.Checked = true;
                gridMain.Columns[index].Visible = true;
            }
            gridMain.RefreshData();
            dgv.Refresh();
            ToolStripMenuItem xmenu = this.columnsToolStripMenuItem;
            xmenu.ShowDropDown();
        }
        /***********************************************************************************************/
        private int getGridColumnIndex(string columnName)
        {
            int index = -1;
            for (int i = 0; i < gridMain.Columns.Count; i++)
            {
                string name = gridMain.Columns[i].Name;
                if (name == columnName)
                {
                    index = i;
                    break;
                }
            }
            return index;
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
            if (continuousPrint)
            {
                SectionMargins margins = rtb2.Document.Sections[0].Margins;
                margins.Left = 0;
                margins.Right = 0;
                margins.Top = 0;
                margins.Bottom = 0;
                printableComponentLink1.Component = rtb2;
            }

            printableComponentLink1.PrintingSystemBase = printingSystem1;
            printableComponentLink1.CreateDetailHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateDetailHeaderArea);
            printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);
            printableComponentLink1.BeforeCreateAreas += new System.EventHandler(this.printableComponentLink1_BeforeCreateAreas);
            printableComponentLink1.AfterCreateAreas += new System.EventHandler(this.printableComponentLink1_AfterCreateAreas);
            this.gridMain.BeforePrintRow += new DevExpress.XtraGrid.Views.Base.BeforePrintRowEventHandler(this.gridMain_BeforePrintRow);
            this.gridMain.AfterPrintRow += new DevExpress.XtraGrid.Views.Base.AfterPrintRowEventHandler(this.gridMain_AfterPrintRow);

            printableComponentLink1.Landscape = true;

            Printer.setupPrinterMargins(50, 100, 80, 50);
            if ( continuousPrint )
                Printer.setupPrinterMargins(0, 0, 0, 0);

            pageMarginLeft = Printer.pageMarginLeft;
            pageMarginRight = Printer.pageMarginRight;
            pageMarginTop = Printer.pageMarginTop;
            pageMarginBottom = Printer.pageMarginBottom;


            printableComponentLink1.Margins.Left = pageMarginLeft;
            printableComponentLink1.Margins.Right = pageMarginRight;
            printableComponentLink1.Margins.Top = pageMarginTop;
            printableComponentLink1.Margins.Bottom = pageMarginBottom;
            if ( continuousPrint )
            {
                printableComponentLink1.MinMargins.Left = pageMarginLeft;
                printableComponentLink1.MinMargins.Right = pageMarginRight;
                printableComponentLink1.MinMargins.Top = pageMarginTop;
                printableComponentLink1.MinMargins.Bottom = pageMarginBottom;
            }

            G1.PrintPreview(printableComponentLink1, gridMain);

            //printableComponentLink1.CreateDocument();
            //printableComponentLink1.ShowPreview();
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
            this.gridMain.BeforePrintRow += new DevExpress.XtraGrid.Views.Base.BeforePrintRowEventHandler(this.gridMain_BeforePrintRow);
            this.gridMain.AfterPrintRow += new DevExpress.XtraGrid.Views.Base.AfterPrintRowEventHandler(this.gridMain_AfterPrintRow);

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
            Printer.DrawQuad(6, 8, 4, 4, "Trust Contracts", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);


            //            Printer.DrawQuadTicks();
            Printer.SetQuadSize(24, 12);
            font = new Font("Ariel", 7, FontStyle.Regular);
            //Printer.DrawQuad(16, 7, 5, 2, lblBalance.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Top);
            //Printer.DrawQuad(16, 10, 5, 2, lblPayment.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            font = new Font("Ariel", 8);
            //string search = "Agents : All";
            //if (!String.IsNullOrWhiteSpace(chkComboAgent.Text))
            //    search = "Agents : " + chkComboAgent.Text;
            //Printer.DrawQuad(1, 6, 6, 3, search, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Top);
            Printer.DrawQuad(1, 9, 6, 3, "User : " + LoginForm.username, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);


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
                    //if ( chkSort.Checked )
                    //    e.PS.InsertPageBreak(e.Y);
                }
            }
        }
        /****************************************************************************************/
        private void gridMain_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
        {
            if (e.Column.FieldName.ToUpper() == "DUEDATE8")
            {
                if (e.RowHandle >= 0)
                {
                    DateTime date = e.DisplayText.ObjToDateTime();
                    e.DisplayText = date.Month.ToString("D2") + "/" + date.Day.ToString("D2") + "/" + date.Year.ToString("D4");
                }
            }
            else if (e.Column.FieldName.ToUpper() == "PAYDATE8")
            {
                if (e.RowHandle >= 0)
                {
                    DateTime date = e.DisplayText.ObjToDateTime();
                    e.DisplayText = date.Month.ToString("D2") + "/" + date.Day.ToString("D2") + "/" + date.Year.ToString("D4");
                }
            }
            else if (e.Column.FieldName.ToUpper() == "ISSUEDATE8")
            {
                if (e.RowHandle >= 0)
                {
                    DateTime date = e.DisplayText.ObjToDateTime();
                    e.DisplayText = date.Month.ToString("D2") + "/" + date.Day.ToString("D2") + "/" + date.Year.ToString("D4");
                }
            }
        }
        /****************************************************************************************/
        private void RunLoad()
        {
            this.Cursor = Cursors.WaitCursor;
            string cmd = "Select * from `contracts` p ";
            cmd += " JOIN `payments` d ON p.`contractNumber` = d.`contractNumber` ";
            cmd += " JOIN `customers` c ON p.`contractNumber` = c.`contractNumber` ";
            cmd += " where p.`contractNumber` = '" + workContract + "' ";

            cmd += " LIMIT 1;";

            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("num");
            dt.Columns.Add("customer");
            dt.Columns.Add("netTrust", Type.GetType("System.Double"));
            dt.Columns.Add("grossTrust", Type.GetType("System.Double"));
            dt.Columns.Add("dueNow", Type.GetType("System.Double"));
            //            dt.Columns.Add("amountPaid", Type.GetType("System.Double"));
            string fname = "";
            string lname = "";
            string name = "";
            string area = "";
            string phone = "";
            string address = "";
            string address2 = "";
            string zip = "";
            string zip2 = "";
            string contractNumber = "";
            string miniContract = "";
            string loc = "";
            string trust = "";
            string locationName = "";
            double serviceTotal = 0D;
            double merchandiseTotal = 0D;
            double allowMerchandise = 0D;
            double allowInsurance = 0D;
            double downpayment = 0D;
            double totalContract = 0D;

            double totalPayments = 0D;
            double totalBalance = 0D;

            double payment = 0D;
            double balance = 0D;
            double netTrust = 0D;
            double grossTrust = 0D;
            double amountPaid = 0D;
            double dueNow = 0D;

            DateTime dueDate = DateTime.Now;
            DateTime issueDate = DateTime.Now;

            for ( int i=0; i<dt.Rows.Count; i++)
            {
                try
                {
                    fname = dt.Rows[i]["firstName1"].ObjToString();
                    lname = dt.Rows[i]["lastName1"].ObjToString();
                    name = fname + " " + lname;
                    dt.Rows[i]["customer"] = name.ToUpper();
                    payment = dt.Rows[i]["amtOfMonthlyPayt"].ObjToDouble();
                    balance = dt.Rows[i]["balanceDue"].ObjToDouble();
                    dueDate = dt.Rows[i]["dueDate8"].ObjToDateTime();
                    issueDate = dt.Rows[i]["issueDate8"].ObjToDateTime();
                    netTrust = DailyHistory.GetContractValue(dt.Rows[i]);
                    dt.Rows[i]["netTrust"] = netTrust;
                    dt.Rows[i]["amountPaid"] = GetTotals();
                    dt.Rows[i]["grossTrust"] = DailyHistory.GetContractValue(dt.Rows[i]);
                    if (DailyHistory.CalculateDueNow(workContract, ref dueNow))
                        dt.Rows[i]["dueNow"] = dueNow;
                    else
                        dt.Rows[i]["dueNow"] = 0D;
                }
                catch ( Exception ex )
                {
                    MessageBox.Show("***ERROR*** Loading Contract! " + ex.Message.ToString());
                }
            }
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private double GetTotals()
        {
            DataTable dt = G1.get_db_data("Select * from `payments` where `contractNumber` = '" + workContract + "' ORDER BY `paydate8`");
            double payment = 0D;
            double interest = 0D;
            double debit = 0D;
            double credit = 0D;
            double downPayment = 0D;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                payment += dt.Rows[i]["paymentAmount"].ObjToDouble();
                interest += dt.Rows[i]["interestPaid"].ObjToDouble();
                debit += dt.Rows[i]["debitAdjustment"].ObjToDouble();
                credit += dt.Rows[i]["creditAdjustment"].ObjToDouble();
                downPayment += dt.Rows[i]["downPayment"].ObjToDouble();
            }
            double paid = payment + downPayment - interest - credit + debit;
            paid = G1.RoundValue(paid);
            return paid;
        }
        /****************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (gridMain.OptionsFind.AlwaysVisible == true)
                gridMain.OptionsFind.AlwaysVisible = false;
            else
                gridMain.OptionsFind.AlwaysVisible = true;
        }
        /****************************************************************************************/
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        /****************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string contract = dr["contractNumber"].ObjToString();
            if (!String.IsNullOrWhiteSpace(contract))
            {
                this.Cursor = Cursors.WaitCursor;
                DataTable dt = (DataTable)dgv.DataSource;
                CustomerDetails clientForm = new CustomerDetails(contract);
                clientForm.Show();
                //DailyHistory dailyForm = new DailyHistory(contract);
                //dailyForm.Show();
                this.Cursor = Cursors.Default;
            }
        }
        /****************************************************************************************/
    }
}