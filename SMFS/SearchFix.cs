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
using System.Text.RegularExpressions;
using ExcelLibrary.SpreadSheet;
using System.Xml;
using OfficeOpenXml;
using System.Drawing.Imaging;
using DevExpress.XtraReports.UI;
using DevExpress.XtraReports.ReportGeneration;
using DevExpress.XtraPrintingLinks;
using DevExpress.XtraGrid.Views.BandedGrid;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Base;
using System.Data.OleDb;
using Excel = Microsoft.Office.Interop.Excel;
using ClosedXML.Excel;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class SearchFix : DevExpress.XtraEditors.XtraForm
    {
        /****************************************************************************************/
        public SearchFix()
        {
            InitializeComponent();
        }
        /****************************************************************************************/
        private void SearchFix_Load(object sender, EventArgs e)
        {
            gridBand4.Visible = false;
        }
        /****************************************************************************************/
        private void btnRun_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            DataTable dt = new DataTable();
            dt.Columns.Add("num");
            dt.Columns.Add("payer");
            dt.Columns.Add("contractNumber");
            dt.Columns.Add("firstName");
            dt.Columns.Add("lastName");
            dt.Columns.Add("ssn");
            dt.Columns.Add("money");
            dt.Columns.Add("issueDate8");
            dt.Columns.Add("lastDatePaid8");
            dt.Columns.Add("lapseDate8");
            dt.Columns.Add("dueDate8");
            dt.Columns.Add("birthDate");
            dt.Columns.Add("deceasedDate");
            dt.Columns.Add("amtOfMonthlyPayt", Type.GetType("System.Double"));
            dt.Columns.Add("nowDue", Type.GetType("System.Double"));

            string directory = @"C:\Users\Robby\Downloads\Insurance\Insurance CSV Files\";
            DataTable importDt = Import.ImportCSVfile(directory + "Insurance Payer Active_20181214.csv");

            int lastRow = importDt.Rows.Count;
//            lastRow = 2;

            string payer = "";
            string cmd = "";
            string contractNumber = "";
            DataTable ddt = null;

            bool found1 = false;
            bool found2 = false;

            lblTotal.Show();

            lblTotal.Text = "of " + lastRow.ToString();
            lblTotal.Refresh();

            barImport.Minimum = 0;
            barImport.Maximum = lastRow;
            labelMaximum.Show();

            for ( int i=0; i<lastRow; i++)
            {
                barImport.Value = i;
                barImport.Refresh();
                labelMaximum.Text = i.ToString();
                labelMaximum.Refresh();

                payer = importDt.Rows[i]["payer#"].ObjToString();
//                payer = "100765";
                if (String.IsNullOrWhiteSpace(payer))
                    continue;
                if (G1.validate_numeric(payer))
                {
                    found1 = FindData(payer, dt);
                    found2 = FindData("0" + payer, dt, false);
                }
                //if (found1 || found2)
                //    break;
            }

            barImport.Value = lastRow;

            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private bool FindData ( string payer, DataTable dt, bool reportBad = true )
        {
            bool foundMoney = false;
            double money = 0D;
            DataTable payDt = null;
            string contractNumber = "";
            string cmd = "Select * from `icustomers` where `payer` = '" + payer + "';";
            DataTable ddt = G1.get_db_data(cmd);
            if (ddt.Rows.Count <= 0)
            {
                if (reportBad)
                {
                    DataRow dRow = dt.NewRow();
                    dRow["payer"] = payer;
                    dRow["ssn"] = "BAD PAYER";
                    dt.Rows.Add(dRow);
                }
                return false;
            }
            try
            {
                bool gotMoney = false;
                for (int j = 0; j < ddt.Rows.Count; j++)
                {
                    gotMoney = true;
                    contractNumber = ddt.Rows[j]["contractNumber"].ObjToString();
                    DataRow dRow = dt.NewRow();
                    dRow["payer"] = payer;
                    dRow["contractNumber"] = contractNumber;
                    cmd = "Select * from `ipayments` p JOIN `icontracts` c ON p.`contractNumber` = c.`contractNumber` ";
                    cmd += "JOIN `icustomers` i ON p.`contractNumber` = i.`contractNumber` ";
                    cmd += " where p.`contractNumber` = '" + contractNumber + "' ";
                    cmd += " LIMIT 1 ";
                    cmd += ";";
                    payDt = G1.get_db_data(cmd);
                    if ( payDt.Rows.Count <= 0 )
                    {
                        gotMoney = false;
                        cmd = "Select * from `icustomers` p JOIN `icontracts` c ON p.`contractNumber` = c.`contractNumber` ";
                        cmd += " where p.`contractNumber` = '" + contractNumber + "' ";
                        cmd += " LIMIT 1 ";
                        cmd += ";";
                        payDt = G1.get_db_data(cmd);
                    }
                    if (payDt.Rows.Count > 0)
                    {
                        dRow["firstName"] = payDt.Rows[0]["firstName"].ObjToString();
                        dRow["lastName"] = payDt.Rows[0]["lastName"].ObjToString();
                        dRow["issueDate8"] = payDt.Rows[0]["issueDate8"].ObjToDateTime().ToString("MM/dd/yyyy");
                        dRow["lastDatePaid8"] = payDt.Rows[0]["lastDatePaid8"].ObjToDateTime().ToString("MM/dd/yyyy");
                        dRow["lapseDate8"] = payDt.Rows[0]["lapseDate8"].ObjToDateTime().ToString("MM/dd/yyyy");
                        dRow["dueDate8"] = payDt.Rows[0]["dueDate8"].ObjToDateTime().ToString("MM/dd/yyyy");
                        dRow["birthDate"] = payDt.Rows[0]["birthDate"].ObjToDateTime().ToString("MM/dd/yyyy");
                        dRow["deceasedDate"] = payDt.Rows[0]["deceasedDate"].ObjToDateTime().ToString("MM/dd/yyyy");
                        money = payDt.Rows[0]["amtOfMonthlyPayt"].ObjToDouble();
                        dRow["amtOfMonthlyPayt"] = money;
                        money = payDt.Rows[0]["nowDue"].ObjToDouble();
                        dRow["nowDue"] = money;
                        if ( gotMoney )
                            dRow["money"] = "Got Money";
                        foundMoney = true;
                    }
                    dt.Rows.Add(dRow);
                }
            }
            catch ( Exception ex )
            {

            }
            return foundMoney;
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
                G1.UpdatePreviousCustomer(contract, LoginForm.username);
                bool insurance = false;
                if (contract.ToUpper().IndexOf("ZZ") == 0)
                    insurance = true;
                if (contract.ToUpper().IndexOf("MM") == 0)
                    insurance = true;
                if (contract.ToUpper().IndexOf("OO") == 0)
                    insurance = true;
                G1.UpdatePreviousCustomer(contract, LoginForm.username);
                if (insurance)
                {
                    string cmd = "Select * from `policies` p JOIN `icustomers` d ON p.`contractNumber` = d.`contractNumber` JOIN `icontracts` x ON p.`contractNumber` = x.`contractNumber` ";
                    cmd += " WHERE p.`contractNumber` = '" + contract + "' ";

                    DataTable dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                    {
                        Policies policyForm = new Policies(contract);
                        policyForm.Show();
                    }
                    else
                    {
                        CustomerDetails clientForm = new CustomerDetails(contract);
                        clientForm.Show();
                    }
                }
                else
                {
                    CustomerDetails clientForm = new CustomerDetails(contract);
                    clientForm.Show();
                }
                this.Cursor = Cursors.Default;
            }
        }
        /****************************************************************************************/
        private void gridMain_CustomDrawCell(object sender, RowCellCustomDrawEventArgs e)
        {
            if (e.Column.FieldName.ToUpper().IndexOf("DATE") >= 0)
            {
                if (e.RowHandle >= 0)
                {
                    DateTime date = e.DisplayText.ObjToDateTime();
                    e.DisplayText = date.Month.ToString("D2") + "/" + date.Day.ToString("D2") + "/" + date.Year.ToString("D4");
                    if (date.Year <= 1)
                        e.DisplayText = "";
                }
            }
        }
        /****************************************************************************************/
        private void gridMain2_CustomDrawCell(object sender, RowCellCustomDrawEventArgs e)
        {
            if (e.Column.FieldName.ToUpper().IndexOf("DATE") >= 0)
            {
                if (e.RowHandle >= 0)
                {
                    DateTime date = e.DisplayText.ObjToDateTime();
                    e.DisplayText = date.Month.ToString("D2") + "/" + date.Day.ToString("D2") + "/" + date.Year.ToString("D4");
                    if (date.Year <= 1)
                        e.DisplayText = "";
                }
            }
        }
        /****************************************************************************************/
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
        /****************************************************************************************/
        private void button1_Click(object sender, EventArgs e)
        {
            string payer = txtContract.Text.Trim();
            if ( String.IsNullOrWhiteSpace ( payer))
            {
                MessageBox.Show("***ERROR*** You Must Have a Payer Number");
                return;
            }

            DataTable dt = new DataTable();
            dt.Columns.Add("num");
            dt.Columns.Add("contractNumber");
            dt.Columns.Add("payer");
            dt.Columns.Add("firstName");
            dt.Columns.Add("lastName");
            dt.Columns.Add("ssn");
            dt.Columns.Add("money");

            string directory = @"C:\Users\Robby\Downloads\Insurance\Insurance CSV Files\";

            this.Cursor = Cursors.WaitCursor;

            DataTable importDt = Import.ImportCSVfile(directory + "01152019 DRAFTS.csv");
            LocatePayer(dt, payer, importDt, "Customer Number", "Name On Account", "", "ACH Draft");

            importDt = Import.ImportCSVfile(directory + "Insurance Payer Active_20181214.csv");
            LocatePayer(dt, payer, importDt, "payer#", "payer Last Name", "payer first name", "Active Payers");

            importDt = Import.ImportCSVfile(directory + "Insurance Payer Dead_20181214.csv");
            LocatePayer(dt, payer, importDt, "payer#", "payer Last Name", "payer first name", "Dead Payers");

            importDt = Import.ImportCSVfile(directory + "Insurance Payer Lapsed_20181214.csv");
            LocatePayer(dt, payer, importDt, "payer#", "payer Last Name", "payer first name", "Lapsed Payers");

            importDt = Import.ImportCSVfile(directory + "Insurance Policy Active_20181214.csv");
            LocatePayer(dt, payer, importDt, "payer#", "payer Last Name", "payer first name", "Active Policy Payers");

            importDt = Import.ImportCSVfile(directory + "Insurance Policy Dead_20181214.csv");
            LocatePayer(dt, payer, importDt, "payer#", "payer Last Name", "payer first name", "Dead Policy Payers");

            importDt = Import.ImportCSVfile(directory + "Insurance Policy Lapsed_20181214.csv");
            LocatePayer(dt, payer, importDt, "payer#", "payer Last Name", "payer first name", "Lapsed Policy Payers");

            importDt = Import.ImportCSVfile(directory + "Insurance payment History 20181031_file 1 of 3.csv");
            LocatePayer(dt, payer, importDt, "payer#", "payer Last Name", "payer first name", "Payments File 1");

            importDt = Import.ImportCSVfile(directory + "Insurance payment History 20181031_file 2 of 3.csv");
            LocatePayer(dt, payer, importDt, "payer#", "payer Last Name", "payer first name", "Payments File 2");

            importDt = Import.ImportCSVfile(directory + "Insurance payment History 20181031_file 3 of 3.csv");
            LocatePayer(dt, payer, importDt, "payer#", "payer Last Name", "payer first name", "Payments File 3");

            importDt = Import.ImportCSVfile(directory + "Insurance payment History 20181214_file 3 of 3.csv");
            LocatePayer(dt, payer, importDt, "payer#", "payer Last Name", "payer first name", "Payments File 4");

            G1.NumberDataTable(dt);
            dgv.DataSource = dt;

            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void LocatePayer ( DataTable dtOut, string findPayer, DataTable dt, string payerColumn, string payerLastNameColumn, string payerFirstNameColumn, string filename )
        {
            bool doPayer = false;
            bool doPayerLastName = false;
            bool doPayerFirstName = false;
            string cmd = "";
            DataTable cDt = null;

            int lastNameCol = -1;
            int firstNameCol = -1;
            if (G1.get_column_number(dt, payerColumn) >= 0)
                doPayer = true;
            else
            {
                MessageBox.Show("***ERROR*** Cannot find Column Payer Column!");
                return;
            }
            if (!String.IsNullOrWhiteSpace(payerLastNameColumn))
            {
                if (G1.get_column_number(dt, payerLastNameColumn) >= 0)
                {
                    lastNameCol = G1.get_column_number(dt, payerLastNameColumn);
                    doPayerLastName = true;
                }
            }
            if (!String.IsNullOrWhiteSpace(payerFirstNameColumn))
            {
                if (G1.get_column_number(dt, payerFirstNameColumn) >= 0)
                    doPayerFirstName = true;
            }
            string payer = "";
            string payerFirstName = "";
            string payerLastName = "";

            string contractNumber = "";
            string contractFirstName = "";
            string contractLastName = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                payer = dt.Rows[i][payerColumn].ObjToString();
                if (payer.IndexOf(findPayer) >= 0)
                {
                    payerFirstName = "";
                    payerLastName = "";
                    DataRow dRow = dtOut.NewRow();
                    dRow["payer"] = payer;
                    if (doPayerFirstName)
                    {
                        payerFirstName = dt.Rows[i][payerFirstNameColumn].ObjToString();
                        dRow["firstName"] = payerFirstName;
                    }
                    if (doPayerLastName)
                    {
                        payerLastName = dt.Rows[i][lastNameCol].ObjToString();
                        dRow["lastName"] = payerLastName;
                    }
                    dRow["ssn"] = filename;
                    if (filename.ToUpper() == "ACTIVE PAYERS" || filename.ToUpper() == "DEAD PAYERS" || filename.ToUpper() == "LAPSED PAYERS"  || filename == "ACH Draft")
                    {
                        bool foundMoney = findMoney(payer);
                        if (foundMoney)
                            dRow["money"] = "FOUND MONEY";
                    }
                    dtOut.Rows.Add(dRow);
                    cmd = "Select * from `icustomers` where `payer` = '" + payer + "';";
                    cDt = G1.get_db_data(cmd);
                    for ( int j=0; j<cDt.Rows.Count; j++)
                    {
                        contractNumber = cDt.Rows[j]["contractNumber"].ObjToString();
                        contractFirstName = cDt.Rows[j]["firstName"].ObjToString();
                        contractLastName = cDt.Rows[j]["lastName"].ObjToString();
                        dRow = dtOut.NewRow();
                        dRow["contractNumber"] = contractNumber;
                        dRow["firstName"] = contractFirstName;
                        dRow["lastName"] = contractLastName;
                        dtOut.Rows.Add(dRow);
                    }
                }
            }
        }
        /***********************************************************************************************/
        private bool findMoney ( string payer )
        {
            string cmd = "Select * from `icustomers` where `payer` = '" + payer + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return false;
            string contractNumber = dt.Rows[0]["contractNumber"].ObjToString();
            cmd = "Select * from `ipayments` where `contractNumber` = '" + contractNumber + "' LIMIT 1;";
            dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return false;
            return true;
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
            printableComponentLink1.Landscape = true;

            Printer.setupPrinterMargins(50, 50, 80, 50);

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
            printableComponentLink1.Landscape = true;

            Printer.setupPrinterMargins(50, 50, 80, 50);

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
            //            footerCount = 0;
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
            string report = "General Search Program";
            if ( dgv2.Visible )
                report = "Bad Payers Summary Report";
            Printer.DrawQuad(5, 8, 8, 4, report, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            //font = new Font("Ariel", 8, FontStyle.Regular);
            //report = cmbWhat.Text + " / " + cmbWho.Text + " / " + cmbDeposits.Text;
            //Printer.DrawQuad(10, 8, 2, 4, report, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            //            Printer.DrawQuadTicks();
        }
        /****************************************************************************************/
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        /****************************************************************************************/
        private void button2_Click(object sender, EventArgs e)
        {
            //ComparePayers(); // Done
            // CompareLapsedPayers(); // Done
            //CompareDeadPayers(); // Done
            //ComparePolicies(); //Done
            //CompareLapsedPolicies(); //Done
            CompareDeadPolicies();
        }
        /****************************************************************************************/
        private void ComparePolicies()
        {
            string badDirectory = @"C:\Users\Robby\Downloads\Insurance\Insurance CSV Files\";
            DataTable badDt = Import.ImportCSVfile(badDirectory + "Insurance Policy Active_20181214.csv");

            string goodDirectory = @"C:\Users\Robby\Downloads\Insurance\New Chance\";
            DataTable goodDt = Import.ImportCSVfile(goodDirectory + "Insurance Policy Active_20181214.csv");

            DataTable dt = compareBadPayers(badDt, goodDt);
            dgv2.DataSource = dt;
        }
        /****************************************************************************************/
        private void CompareLapsedPolicies()
        {
            string badDirectory = @"C:\Users\Robby\Downloads\Insurance\Insurance CSV Files\";
            DataTable badDt = Import.ImportCSVfile(badDirectory + "Insurance Policy Lapsed_20181214.csv");

            string goodDirectory = @"C:\Users\Robby\Downloads\Insurance\New Chance\";
            DataTable goodDt = Import.ImportCSVfile(goodDirectory + "Insurance Policy Lapsed_20181214.csv");

            DataTable dt = compareBadPayers(badDt, goodDt);
            dgv2.DataSource = dt;
        }
        /****************************************************************************************/
        private void CompareDeadPolicies()
        {
            string badDirectory = @"C:\Users\Robby\Downloads\Insurance\Insurance CSV Files\";
            DataTable badDt = Import.ImportCSVfile(badDirectory + "Insurance Policy Dead_20181214.csv");

            string goodDirectory = @"C:\Users\Robby\Downloads\Insurance\New Chance\";
            DataTable goodDt = Import.ImportCSVfile(goodDirectory + "Insurance Policy Dead_20181214.csv");

            DataTable dt = compareBadPayers(badDt, goodDt);
            dgv2.DataSource = dt;
        }
        /****************************************************************************************/
        private void CompareLapsedPayers()
        {
            string badDirectory = @"C:\Users\Robby\Downloads\Insurance\Insurance CSV Files\";
            DataTable badDt = Import.ImportCSVfile(badDirectory + "Insurance Payer Lapsed_20181214.csv");

            string goodDirectory = @"C:\Users\Robby\Downloads\Insurance\New Chance\";
            DataTable goodDt = Import.ImportCSVfile(goodDirectory + "Insurance Payer Lapsed_20181214.csv");

            DataTable dt = compareBadPayers(badDt, goodDt);
            dgv2.DataSource = dt;
        }
        /****************************************************************************************/
        private void CompareDeadPayers()
        {
            string badDirectory = @"C:\Users\Robby\Downloads\Insurance\Insurance CSV Files\";
            DataTable badDt = Import.ImportCSVfile(badDirectory + "Insurance Payer Dead_20181214.csv");

            string goodDirectory = @"C:\Users\Robby\Downloads\Insurance\New Chance\";
            DataTable goodDt = Import.ImportCSVfile(goodDirectory + "Insurance Payer Dead_20181214.csv");

            DataTable dt = compareBadPayers(badDt, goodDt);
            dgv2.DataSource = dt;
        }
        /****************************************************************************************/
        private void ComparePayers ()
        {
            string badDirectory = @"C:\Users\Robby\Downloads\Insurance\Insurance CSV Files\";
            DataTable badDt = Import.ImportCSVfile(badDirectory + "Insurance Payer Active_20181214.csv");

            string goodDirectory = @"C:\Users\Robby\Downloads\Insurance\New Chance\";
            DataTable goodDt = Import.ImportCSVfile(goodDirectory + "Insurance Payer Active_20181214.csv");

            DataTable dt = compareBadPayers(badDt, goodDt);
            dgv2.DataSource = dt;
        }
        /****************************************************************************************/
        private DataTable compareBadPayers ( DataTable badDt, DataTable goodDt )
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("num");
            dt.Columns.Add("Good Payers");
            dt.Columns.Add("Bad Payers");
            dt.Columns.Add("Good Contracts");
            dt.Columns.Add("Bad Contracts");
            dt.Columns.Add("Good Money");
            dt.Columns.Add("Bad Money");
            dt.Columns.Add("firstName");
            dt.Columns.Add("lastName");
            dt.Columns.Add("where");
            dt.Columns.Add("issueDate8");
            dt.Columns.Add("lastDatePaid8");
            dt.Columns.Add("lapseDate8");
            dt.Columns.Add("dueDate8");
            dt.Columns.Add("birthDate");
            dt.Columns.Add("deceasedDate");
            dt.Columns.Add("amtOfMonthlyPayt", Type.GetType("System.Double"));
            dt.Columns.Add("nowDue", Type.GetType("System.Double"));

            int dueDateCol = G1.get_column_number(goodDt, "DDUE8");
            int lastPaidCol = G1.get_column_number(goodDt, "LPAID8");

            int issueDateCol = G1.get_column_number(goodDt, "IDATE8");
            if (issueDateCol < 0)
                issueDateCol = G1.get_column_number(goodDt, "issue date");
            int birthDateCol = G1.get_column_number(goodDt, "BDATE8");
            if (birthDateCol < 0)
                birthDateCol = G1.get_column_number(goodDt, "birth date");

            int goodFirstNameCol = G1.get_column_number(goodDt, "pfname");
            if (goodFirstNameCol < 0)
                goodFirstNameCol = G1.get_column_number(goodDt, "payer first name");

            int goodLastNameCol = G1.get_column_number(goodDt, "plname");
            if (goodLastNameCol < 0)
                goodLastNameCol = G1.get_column_number(goodDt, "payer last name");

            int lastRow = badDt.Rows.Count;

            string badPayer = "";
            string goodPayer = "";
            string contracts = "";
            string firstName = "";
            string lastName = "";
            string contractNumber = "";
            string where = "";
            string cmd = "";
            DataTable cDt = null;
            bool GotMoney = false;
            DateTime date = DateTime.Now;
            string pulled = "";
            string oldBadPayer = "";
            string oldGoodPayer = "";

            lblTotal.Show();

            lblTotal.Text = "of " + lastRow.ToString();
            lblTotal.Refresh();

            barImport.Minimum = 0;
            barImport.Maximum = lastRow;

            labelMaximum.Show();

            int i = 0;

            try
            {
                //lastRow = 1;
                for (i = 0; i < lastRow; i++)
                {
                    barImport.Value = i;
                    barImport.Refresh();
                    labelMaximum.Text = i.ToString();
                    labelMaximum.Refresh();

                    badPayer = badDt.Rows[i]["payer#"].ObjToString();
                    badPayer = badPayer.Replace(",", "");
                    goodPayer = goodDt.Rows[i]["payer#"].ObjToString();
                    goodPayer = goodPayer.Replace(",", "");
                    badPayer = badPayer.TrimStart('0');
                    goodPayer = goodPayer.TrimStart('0');
                    //badPayer = "170013";
                    //goodPayer = "0170013";
                    if (badPayer == oldBadPayer && goodPayer == oldGoodPayer)
                        continue;
                    oldBadPayer = badPayer;
                    oldGoodPayer = goodPayer;
                    
                    if (badPayer != goodPayer)
                    {
                        DataRow dRow = dt.NewRow();
                        dRow["Good Payers"] = goodPayer;
                        dRow["Bad Payers"] = badPayer;
                        contracts = FindContracts(goodPayer, ref GotMoney);
                        dRow["Good Contracts"] = contracts;
                        if (GotMoney)
                            dRow["Good Money"] = "YES";
                        contracts = FindContracts(badPayer, ref GotMoney);
                        dRow["Bad Contracts"] = contracts;
                        if (GotMoney)
                            dRow["Bad Money"] = "YES";
                        firstName = "";
                        if (goodFirstNameCol >= 0)
                            firstName = goodDt.Rows[i][goodFirstNameCol].ObjToString();
                        lastName = "";
                        if (goodLastNameCol >= 0)
                            lastName = goodDt.Rows[i][goodLastNameCol].ObjToString();
                        if (!String.IsNullOrWhiteSpace(firstName) && !String.IsNullOrWhiteSpace(lastName))
                        {
                            firstName = G1.protect_data(firstName);
                            lastName = G1.protect_data(lastName);
                            dRow["firstName"] = firstName;
                            dRow["lastName"] = lastName;
                            cmd = "Select * from `icustomers` where `payer` = '" + badPayer + "' and `lastName` = '" + lastName + "' and `firstName` = '" + firstName + "';";
                            cDt = G1.get_db_data(cmd);
                            if (cDt.Rows.Count > 0)
                            {
                                contracts = "(BAD),";
                                for (int k = 0; k < cDt.Rows.Count; k++)
                                {
                                    contractNumber = cDt.Rows[k]["contractNumber"].ObjToString();
                                    pulled = cDt.Rows[k]["pulled"].ObjToString();
                                    if (pulled.Trim().ToUpper() == "BAD")
                                        continue;
                                    GetDetails(contractNumber, "Bad", dRow);
                                    contracts += contractNumber + ",";
                                }
                                contracts = contracts.TrimEnd(',');
                                dRow["Bad Contracts"] = contracts;
                            }
                            cmd = dRow["Bad Contracts"].ObjToString();
                            if ( cmd.ToUpper() == "(BAD)")
                            {

                            }
                            cmd = "Select * from `icustomers` where `payer` = '" + goodPayer + "' and `lastName` = '" + lastName + "' and `firstName` = '" + firstName + "';";
                            cDt = G1.get_db_data(cmd);
                            if (cDt.Rows.Count > 0)
                            {
                                contracts = "(GOOD),";
                                where = dRow["where"].ObjToString();
                                if (!String.IsNullOrWhiteSpace(where))
                                    where += "/Good";
                                for (int k = 0; k < cDt.Rows.Count; k++)
                                {
                                    pulled = cDt.Rows[k]["pulled"].ObjToString();
                                    if (pulled.Trim().ToUpper() == "BAD")
                                        continue;
                                    contractNumber = cDt.Rows[k]["contractNumber"].ObjToString();
                                    GetDetails(contractNumber, where, dRow);
                                    contracts += contractNumber + ",";
                                }
                                contracts = contracts.TrimEnd(',');
                                dRow["Good Contracts"] = contracts;
                            }
                        }
                        where = dRow["where"].ObjToString();
                        if (String.IsNullOrWhiteSpace(where))
                        {
                            where = "FILE";
                            if (dueDateCol >= 0)
                            {
                                date = goodDt.Rows[i][dueDateCol].ObjToDateTime();
                                if (date.Year > 1800)
                                    dRow["dueDate8"] = date.ToString("MM/dd/yyyy");
                            }
                            if (lastPaidCol >= 0)
                            {
                                date = goodDt.Rows[i][lastPaidCol].ObjToDateTime();
                                if (date.Year > 1800)
                                    dRow["lastDatePaid8"] = date.ToString("MM/dd/yyyy");
                            }
                            if ( issueDateCol >= 0 )
                            {
                                date = goodDt.Rows[i][issueDateCol].ObjToDateTime();
                                if (date.Year > 1800)
                                    dRow["issueDate8"] = date.ToString("MM/dd/yyyy");
                            }
                            if ( birthDateCol >= 0 )
                            {
                                date = goodDt.Rows[i][birthDateCol].ObjToDateTime();
                                if (date.Year > 1800)
                                    dRow["birthDate"] = date.ToString("MM/dd/yyyy");
                            }
                            dRow["where"] = "FILE";
                        }
                        dt.Rows.Add(dRow);
                    }
                }
            }
            catch ( Exception ex)
            {
                MessageBox.Show("***ERROR*** Ex " + ex.Message.ToString());
            }
            barImport.Value = lastRow;
            barImport.Refresh();
            labelMaximum.Text = lastRow.ToString();
            labelMaximum.Refresh();

            G1.NumberDataTable(dt);
            return dt;
        }
        /****************************************************************************************/
        private void GetDetails ( string contractNumber, string where, DataRow dRow )
        {
            try
            {
                string cmd = "Select * from `icustomers` p JOIN `icontracts` c ON p.`contractNumber` = c.`contractNumber` ";
                cmd += " where p.`contractNumber` = '" + contractNumber + "' ";
                cmd += " LIMIT 1 ";
                cmd += ";";
                DataTable payDt = G1.get_db_data(cmd);
                if (payDt.Rows.Count <= 0)
                    return;
                dRow["issueDate8"] = getLatestDate(payDt, 0, dRow, "issueDate8");
                dRow["lastDatePaid8"] = getLatestDate(payDt, 0, dRow, "lastDatePaid8");
                dRow["lapseDate8"] = getLatestDate(payDt, 0, dRow, "lapseDate8");
                dRow["dueDate8"] = getLatestDate(payDt, 0, dRow, "dueDate8");
                dRow["birthDate"] = getLatestDate(payDt, 0, dRow, "birthDate");
                dRow["deceasedDate"] = getLatestDate(payDt, 0, dRow, "deceasedDate");
                dRow["amtOfMonthlyPayt"] = getLatestMoney(payDt, 0, dRow, "amtOfMonthlyPayt");
                dRow["nowDue"] = getLatestMoney(payDt, 0, dRow, "nowDue");
                dRow["where"] = where;
                //DateTime date = payDt.Rows[0]["issueDate8"].ObjToDateTime();
                //if (date.Year > 1800)
                //{
                //    dRow["issueDate8"] = payDt.Rows[0]["issueDate8"].ObjToDateTime().ToString("MM/dd/yyyy");
                //    date = payDt.Rows[0]["lastDatePaid8"].ObjToDateTime();
                //    dRow["lastDatePaid8"] = payDt.Rows[0]["lastDatePaid8"].ObjToDateTime().ToString("MM/dd/yyyy");
                //    dRow["lapseDate8"] = payDt.Rows[0]["lapseDate8"].ObjToDateTime().ToString("MM/dd/yyyy");
                //    dRow["dueDate8"] = payDt.Rows[0]["dueDate8"].ObjToDateTime().ToString("MM/dd/yyyy");
                //    dRow["birthDate"] = payDt.Rows[0]["birthDate"].ObjToDateTime().ToString("MM/dd/yyyy");
                //    dRow["deceasedDate"] = payDt.Rows[0]["deceasedDate"].ObjToDateTime().ToString("MM/dd/yyyy");
                //    double money = payDt.Rows[0]["amtOfMonthlyPayt"].ObjToDouble();
                //    dRow["amtOfMonthlyPayt"] = money;
                //    money = payDt.Rows[0]["nowDue"].ObjToDouble();
                //    dRow["nowDue"] = money;
                //    dRow["where"] = where;
                //}
            }
            catch ( Exception ex )
            {

            }
        }
        /****************************************************************************************/
        private double getLatestMoney(DataTable dt, int row, DataRow dRow, string dateField)
        {
            double oldMoney = dRow[dateField].ObjToDouble();
            double tableMoney = dt.Rows[row][dateField].ObjToDouble();
            if (oldMoney == 0D)
                return tableMoney;
            return oldMoney;
        }
        /****************************************************************************************/
        private string getLatestDate ( DataTable dt, int row, DataRow dRow, string dateField )
        {
            DateTime oldDate = dRow[dateField].ObjToDateTime();
            DateTime tableDate = dt.Rows[row][dateField].ObjToDateTime();
            if (oldDate > tableDate)
                return oldDate.ToString("MM/dd/yyyy");
            return tableDate.ToString("MM/dd/yyyy");
        }
        /****************************************************************************************/
        private string FindContracts ( string payer, ref bool GotMoney )
        {
            GotMoney = false;
            DataTable mDt = null;
            string pulled = "";
            string contracts = "";
            string contractNumber = "";
            string cmd = "Select * from `icustomers` c JOIN `icontracts` i ON c.`contractNumber` = i.`contractNumber` where `payer` = '" + payer + "'";
            DataTable dt = G1.get_db_data(cmd);
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                //pulled = dt.Rows[i]["pulled"].ObjToString();
                //if (pulled.Trim().ToUpper() == "BAD")
                //    continue;
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                if (!String.IsNullOrWhiteSpace(contractNumber))
                    contracts += contractNumber + ",";
                cmd = "Select * from `ipayments` where `contractNumber` = '" + contractNumber + "';";
                mDt = G1.get_db_data(cmd);
                if (mDt.Rows.Count > 0)
                    GotMoney = true;
            }
            contracts = contracts.TrimEnd(',');
            return contracts;
        }
        /****************************************************************************************/
        private void GoToPayer(string payer)
        {
            if (String.IsNullOrWhiteSpace(payer))
                return;

            this.Cursor = Cursors.WaitCursor;
            string contract = "";
            string cmd = "Select * from `policies` p JOIN `icustomers` d ON p.`contractNumber` = d.`contractNumber` JOIN `icontracts` x ON p.`contractNumber` = x.`contractNumber` ";
            cmd += " WHERE p.`payer` = '" + payer + "' ";

            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                contract = dx.Rows[0]["contractNumber"].ObjToString();
                Policies policyForm = new Policies(contract);
                policyForm.Show();
            }
            else
            {
                cmd = "Select * from `icustomers` where `payer` = '" + payer + "';";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    contract = dx.Rows[0]["contractNumber"].ObjToString();
                    CustomerDetails clientForm = new CustomerDetails(contract);
                    clientForm.Show();
                }
            }
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void goToGoodPayerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain2.GetFocusedDataRow();
            string payer = dr["Good Payers"].ObjToString();
            GoToPayer(payer);
        }
        /****************************************************************************************/
        private void goToBadPayerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain2.GetFocusedDataRow();
            string payer = dr["Bad Payers"].ObjToString();
            GoToPayer(payer);
        }
        /****************************************************************************************/
        private void btnFix_Click(object sender, EventArgs e)
        {
            lblTotal.Show();

            create_audit();
            int i = 0;
            this.Cursor = Cursors.WaitCursor;
            try
            {
                DataTable dt = (DataTable)dgv2.DataSource;
                int lastRow = dt.Rows.Count;
                lblTotal.Text = "of " + lastRow.ToString();
                lblTotal.Refresh();

                barImport.Minimum = 0;
                barImport.Maximum = lastRow;
                labelMaximum.Show();


                for (i = 0; i < dt.Rows.Count; i++)
                {
                    barImport.Value = i;
                    barImport.Refresh();
                    labelMaximum.Text = i.ToString();
                    labelMaximum.Refresh();

                    FixData(dt.Rows[i]);
                }

                barImport.Value = lastRow;
                barImport.Refresh();
                labelMaximum.Text = lastRow.ToString();
                labelMaximum.Refresh();
            }
            catch ( Exception ex )
            {
                MessageBox.Show("***ERROR*** Ex=" + ex.Message.ToString());
            }
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void fixToolStripMenuItem_Click(object sender, EventArgs e)
        {
            create_audit();

            DataRow dr = gridMain2.GetFocusedDataRow();
            FixData(dr);
        }
        /****************************************************************************************/
        private void FixData ( DataRow dr)
        {
            rtb.Clear();

            bool debug = false;
            //debug = true;
            string cmd = "";
            string record = "";
            string contractNumber = "";
            string policy = "";
            bool foundBadPolicies = false;

            string badPayer = dr["Bad Payers"].ObjToString();
            string goodPayer = dr["Good Payers"].ObjToString();
            string badContracts = dr["Bad Contracts"].ObjToString();
            badContracts = badContracts.Replace("(BAD)", "");
            string[] Lines = badContracts.Split(',');
            for (int i = 0; i < Lines.Length; i++)
            {
                contractNumber = Lines[i].Trim();
                if ( !String.IsNullOrWhiteSpace(contractNumber))
                {
                    badContracts = contractNumber;
                    break;
                }
            }
            badContracts = badContracts.Replace(",", "");

            string goodContracts = dr["Good Contracts"].ObjToString();
            goodContracts = goodContracts.Replace("(GOOD)", "");
            Lines = goodContracts.Split(',');
            for ( int i=0; i<Lines.Length; i++)
            {
                contractNumber = Lines[i].Trim();
                if (contractNumber.IndexOf("ZZ") >= 0)
                {
                    goodContracts = contractNumber;
                    break;
                }
            }
            goodContracts = goodContracts.Replace(",", "");

            DateTime issueDate8 = dr["issueDate8"].ObjToDateTime();
            DateTime lastDatePaid8 = dr["lastDatePaid8"].ObjToDateTime();
            DateTime lapseDate8 = dr["lapseDate8"].ObjToDateTime();
            DateTime dueDate8 = dr["dueDate8"].ObjToDateTime();
            DateTime birthDate = dr["birthDate"].ObjToDateTime();
            DateTime deceasedDate = dr["deceasedDate"].ObjToDateTime();
            double amtOfMonthlyPayt = dr["amtOfMonthlyPayt"].ObjToDouble();
            double nowDue = dr["nowDue"].ObjToDouble();

            write_audit("Bad Payer = " + badPayer );
            write_audit("Bad Contract = " + badContracts );
            write_audit("Good Payer = " + goodPayer );
            write_audit("Good Contract = " + goodContracts);
            write_audit("");
            if (String.IsNullOrWhiteSpace(badContracts))
            {
                write_audit("*** No Bad Contract found . . .");
                write_audit("");
                if ( !String.IsNullOrWhiteSpace ( goodContracts ))
                { // Good Contract but Bad Payer may have Policies to change.
                    cmd = "Select * from `policies` where `payer` = '" + badPayer + "';";
                    DataTable ddx = G1.get_db_data(cmd);
                    if ( ddx.Rows.Count > 0 )
                    {
                        write_audit("   Changing Bad Payer Policies to Good Payer");
                        for ( int i=0; i<ddx.Rows.Count; i++)
                        {
                            record = ddx.Rows[i]["record"].ObjToString();
                            if ( !debug )
                                G1.update_db_table("policies", "record", record, new string[] { "contractNumber", goodContracts, "payer", goodPayer});
                        }
                    }
                }
                return;
            }
            if ( badContracts.IndexOf ( "OO") == 0 && goodContracts.IndexOf ("ZZ") == 0 )
            {
                write_audit("*** Reversing Bad Contract " + badContracts + " to Good Contract " + goodContracts);
                cmd = badContracts;
                badContracts = goodContracts;
                goodContracts = cmd;
            }
            //if (String.IsNullOrWhiteSpace(goodContracts))
            //{
            //    write_audit("*** No Good Contract found . . .");
            //    write_audit("");
            //    return;
            //}


            cmd = "Select * from `policies` p JOIN `icustomers` d ON p.`contractNumber` = d.`contractNumber` JOIN `icontracts` x ON p.`contractNumber` = x.`contractNumber` ";
            cmd += " WHERE p.`payer` = '" + badPayer + "';";

            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                write_audit("Change Bad Policy Payers Contracts");
                for (int i = 0; i < dx.Rows.Count; i++)
                {
                    foundBadPolicies = true;
                    record = dx.Rows[i]["record"].ObjToString();
                    contractNumber = dx.Rows[i]["contractNumber"].ObjToString();
                    policy = dx.Rows[i]["policyNumber"].ObjToString();
                    if ( contractNumber != badContracts )
                        write_audit ("Policy (" + policy + ") Record (" + record + ") ContractNumber (" + contractNumber + ") to (" + badContracts + ")");
                    write_audit ("    Change Bad Payer (" + badPayer + ") to (" + goodPayer + ")");
                    if ( !debug)
                        G1.update_db_table("policies", "record", record, new string[] { "contractNumber", badContracts, "payer", goodPayer });
                    if (contractNumber != badContracts)
                    {
                        write_audit("    Mark Contract (" + contractNumber + ") as BAD!");
                        cmd = "UPDATE `icontracts` SET `xtrust` = 'BAD' WHERE `contractNumber` = '" + contractNumber + "';";
                        G1.update_db_data(cmd);
                        cmd = "UPDATE `icustomers` SET `pulled` = 'BAD' WHERE `contractNumber` = '" + contractNumber + "';";
                        if ( !debug)
                            G1.update_db_data(cmd);
                    }
                }
            }

            cmd = "Select * from `policies` p JOIN `icustomers` d ON p.`contractNumber` = d.`contractNumber` JOIN `icontracts` x ON p.`contractNumber` = x.`contractNumber` ";
            cmd += " WHERE p.`payer` = '" + goodPayer + "';";

            dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                if (foundBadPolicies)
                    write_audit("");
                write_audit("Change Good Policy Payers Contracts");
                for (int i = 0; i < dx.Rows.Count; i++)
                {
                    record = dx.Rows[i]["record"].ObjToString();
                    contractNumber = dx.Rows[i]["contractNumber"].ObjToString();
                    policy = dx.Rows[i]["policyNumber"].ObjToString();
                    if (contractNumber != badContracts)
                    {
                        write_audit("Policy (" + policy + ") Record (" + record + ") ContractNumber (" + contractNumber + ") to (" + badContracts + ")");
                        if (!debug)
                            G1.update_db_table("policies", "record", record, new string[] { "contractNumber", badContracts });
                        cmd = "UPDATE `icontracts` SET `xtrust` = 'BAD' WHERE `contractNumber` = '" + contractNumber + "';";
                        if ( !debug )
                            G1.update_db_data(cmd);
                        cmd = "UPDATE `icustomers` SET `pulled` = 'BAD' WHERE `contractNumber` = '" + contractNumber + "';";
                        if ( !debug)
                            G1.update_db_data(cmd);
                    }
                }
            }
            write_audit("Update Customer Contract " + badContracts + " to Good Payer " + goodPayer);
            cmd = "UPDATE `icustomers` SET `payer` = '" + goodPayer + "' WHERE `contractNumber` = '" + badContracts + "';";
            if ( !debug )
                G1.update_db_data(cmd);

            if (!String.IsNullOrWhiteSpace(goodContracts))
            {
                cmd = "Select * from `ipayments` where `contractNumber` = '" + goodContracts + "' LIMIT 1;";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    write_audit("");
                    write_audit("Change Payment Contracts Row Count (" + dx.Rows.Count.ToString() + ") from " + goodContracts + " to " + badContracts);

                    cmd = "UPDATE `ipayments` SET `contractNumber` = '" + badContracts + "' WHERE `contractNumber` = '" + goodContracts + "';";
                    if ( !debug )
                        G1.update_db_data(cmd);
                }
            }

            string updateFields = "";
            if( issueDate8.Year > 1500 )
                updateFields += "issueDate8," + issueDate8.ToString("MM/dd/yyyy") + ",";
            if (lastDatePaid8.Year > 1500)
                updateFields += "lastDatePaid8," + lastDatePaid8.ToString("MM/dd/yyyy") + ",";
            if (lapseDate8.Year > 1500)
                updateFields += "lapseDate8," + lapseDate8.ToString("MM/dd/yyyy") + ",";
            if (dueDate8.Year > 1500)
                updateFields += "dueDate8," + dueDate8.ToString("MM/dd/yyyy") + ",";
            if (deceasedDate.Year > 1500)
                updateFields += "deceasedDate," + deceasedDate.ToString("MM/dd/yyyy") + ",";
            if (amtOfMonthlyPayt > 0D)
                updateFields += "amtOfMonthlyPayt," + amtOfMonthlyPayt.ToString() + ",";
            if (nowDue > 0D)
                updateFields += "nowDue," + nowDue.ToString() + ",";
            updateFields = updateFields.TrimEnd(',');

            if (!String.IsNullOrWhiteSpace(updateFields))
            {
                cmd = "Select * from `icontracts` where `contractNumber` = '" + badContracts + "';";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    record = dx.Rows[0]["record"].ObjToString();
                    if ( !debug )
                        G1.update_db_table("icontracts", "record", record, updateFields);
                }
            }
            if ( birthDate.Year > 1850)
            {
                cmd = "Select * from `icustomers` where `contractNumber` = '" + badContracts + "';";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    updateFields = "birthDate," + birthDate.ToString("MM/dd/yyyy");
                    record = dx.Rows[0]["record"].ObjToString();
                    if (!debug)
                        G1.update_db_table("icustomers", "record", record, updateFields);
                }
            }
        }
        /****************************************************************************************/
        private string auditFile = "fix.out";
        private void create_audit()
        {
            DateTime date = DateTime.Now;

            string filename = "c:/rag/Fix_" + date.ToString("yyyyMMdd_hhmmss") + ".out";
            if (File.Exists(filename))
                File.Delete(filename);
            auditFile = filename;
            StreamWriter sw = File.CreateText(filename);
            sw.WriteLine(date.ToString("MM/dd/yyyy hh:mm:ss") + " Start Audit Trail . . .");
            ((IDisposable)sw).Dispose();
            sw.Close();
        }
        /****************************************************************************************/
        private void write_audit(string str)
        {
            rtb.AppendText(str + "\n");

            string filename = auditFile;
            if (!File.Exists(filename))
                create_audit();
            using (StreamWriter sw = File.AppendText(filename))
            {
                DateTime date = DateTime.Now;
                sw.WriteLine(date.ToString("MM/dd/yyyy hh:mm:ss" ) + " " + str);
                sw.Flush();
                sw.Close();
            }
//            list_audit();
        }
        /****************************************************************************************/
        private void btnTrimCustomers_Click(object sender, EventArgs e)
        {
            lblTotal.Show();

            int i = 0;
            this.Cursor = Cursors.WaitCursor;
            try
            {
                string record = "";
                string payer = "";
                string newPayer = "";
                string cmd = "Select * from `icustomers`;";
                DataTable dt = G1.get_db_data(cmd);

                int lastRow = dt.Rows.Count;
                lblTotal.Text = "of " + lastRow.ToString();
                lblTotal.Refresh();

                barImport.Minimum = 0;
                barImport.Maximum = lastRow;
                labelMaximum.Show();

                string updateFields = "";


                for (i = 0; i < dt.Rows.Count; i++)
                {
                    barImport.Value = i;
                    barImport.Refresh();
                    labelMaximum.Text = i.ToString();
                    labelMaximum.Refresh();
                    payer = dt.Rows[i]["payer"].ObjToString();
                    newPayer = payer.TrimStart('0');
                    if (payer == newPayer)
                        continue;
                    record = dt.Rows[i]["record"].ObjToString();
                    updateFields = "payer," + newPayer;
                    G1.update_db_table("icustomers", "record", record, updateFields);
                }

                barImport.Value = lastRow;
                barImport.Refresh();
                labelMaximum.Text = lastRow.ToString();
                labelMaximum.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Ex=" + ex.Message.ToString());
            }
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void btnTrimPolicies_Click(object sender, EventArgs e)
        {
            lblTotal.Show();

            int i = 0;
            this.Cursor = Cursors.WaitCursor;
            try
            {
                string record = "";
                string payer = "";
                string newPayer = "";
                string cmd = "Select * from `policies`;";
                DataTable dt = G1.get_db_data(cmd);

                int lastRow = dt.Rows.Count;
                lblTotal.Text = "of " + lastRow.ToString();
                lblTotal.Refresh();

                barImport.Minimum = 0;
                barImport.Maximum = lastRow;
                labelMaximum.Show();

                string updateFields = "";


                for (i = 0; i < dt.Rows.Count; i++)
                {
                    barImport.Value = i;
                    barImport.Refresh();
                    labelMaximum.Text = i.ToString();
                    labelMaximum.Refresh();
                    payer = dt.Rows[i]["payer"].ObjToString();
                    newPayer = payer.TrimStart('0');
                    if (payer == newPayer)
                        continue;
                    record = dt.Rows[i]["record"].ObjToString();
                    updateFields = "payer," + newPayer;
                    G1.update_db_table("policies", "record", record, updateFields);
                }

                barImport.Value = lastRow;
                barImport.Refresh();
                labelMaximum.Text = lastRow.ToString();
                labelMaximum.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Ex=" + ex.Message.ToString());
            }
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
    }
}