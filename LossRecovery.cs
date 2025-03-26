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
using DevExpress.XtraRichEdit;
using System.Collections.Generic;
//using GemBox.Document;
//using DocumentFormat.OpenXml.Packaging;
using Word = Microsoft.Office.Interop.Word;
using System.Text;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class LossRecovery : DevExpress.XtraEditors.XtraForm
    {
        DataTable originalDt = null;
        private string wordTemplateName = "10PN002";
        /***********************************************************************************************/
        public LossRecovery()
        {
            InitializeComponent();
        }
        /***********************************************************************************************/
        private void LossRecovery_Load(object sender, EventArgs e)
        {
            button1.Hide();
            barImport.Hide();
            labelAll.Hide();

            DateTime now = DateTime.Now;
            now = now.AddMonths(-1);
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            now = new DateTime(now.Year, now.Month, days);
            this.dateTimePicker2.Value = now;
//            now = now.AddMonths(-2);
            now = new DateTime(now.Year, now.Month, 1);
            this.dateTimePicker1.Value = now;

            LoadLocations();
        }
        /***********************************************************************************************/
        private void LoadLocations()
        {
            string cmd = "Select * from `funeralhomes` order by `keycode`;";
            DataTable locDt = G1.get_db_data(cmd);
//            chkComboLocNames.Properties.DataSource = locDt;
            chkComboLocation.Properties.DataSource = locDt;
        }
        /***********************************************************************************************/
        private void btnRun_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            DateTime date = dateTimePicker1.Value;
            string date1 = G1.DateTimeToSQLDateTime(date);
            date = dateTimePicker2.Value;
            string date2 = G1.DateTimeToSQLDateTime(date);

            DateTime startDate = new DateTime(2015, 1, 1);
            //string cmd = "Select * from `contracts` x JOIN `customers` c ON x.`contractNumber` = c.`contractNumber` WHERE `issueDate8` >= '" + date1 + "' and `issueDate8` <= '" + date2 + "' AND x.`deceasedDate` < '1850-01-01' ORDER BY x.`contractNumber`;";
            string cmd = "Select * from `contracts` x LEFT JOIN `customers` c ON x.`contractNumber` = c.`contractNumber` WHERE `issueDate8` >= '" + date1 + "' and `issueDate8` <= '" + date2 + "' ORDER BY x.`contractNumber`;";
            DataTable dt = G1.get_db_data(cmd);

            dt.Columns.Add("num");
            dt.Columns.Add("loc");
            dt.Columns.Add("fullname");
            dt.Columns.Add("contractValue", Type.GetType("System.Double"));
            dt.Columns.Add("issueDate");
            dt.Columns.Add("bdate");

            string contractNumber = "";
            string contract = "";
            string trust = "";
            string loc = "";
            string ssn = "";
            double contractValue = 0D;
            double allowInsurance = 0D;
            double downPayment = 0D;
            try
            {
                dt.Columns.Add("IFX"); // Insurance Funded
                dt.Columns.Add("TF"); // Trust Funded
                dt.Columns.Add("CAX"); // Class A Insurance

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i]["fullname"] = dt.Rows[i]["firstName"].ObjToString().Trim() + " " + dt.Rows[i]["lastName"].ObjToString().Trim();
                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                    if ( contractNumber == "P24002L")
                    {
                    }
                    if (DailyHistory.gotCemetery(contractNumber))
                        continue;
                    contract = Trust85.decodeContractNumber(contractNumber, ref trust, ref loc);
                    if ( String.IsNullOrWhiteSpace ( trust ))
                    {
                        contract = contract.Substring(2);
                        if (contract.Substring(0, 1) == "8") // Tim told me to exclude the 800 series contracts.
                            continue;
                    }
                    dt.Rows[i]["loc"] = loc;
                    date = dt.Rows[i]["issueDate8"].ObjToDateTime();
                    date = DailyHistory.GetIssueDate(date, contractNumber, dt);
                    dt.Rows[i]["issueDate"] = date.ToString("MM/dd/yyyy");
                    date = dt.Rows[i]["birthDate"].ObjToDateTime();
                    dt.Rows[i]["bdate"] = date.ToString("MM/dd/yyyy");
                    contractValue = DailyHistory.GetContractValuePlus(dt.Rows[i], true );
                    if (DailyHistory.IsFundedByInsurance(dt.Rows[i]))
                        contractValue = 0D;
                    dt.Rows[i]["contractValue"] = contractValue;
                    dt.Rows[i]["TF"] = "X"; // Trust Funded
                    allowInsurance = dt.Rows[i]["allowInsurance"].ObjToDouble();
                    ssn = dt.Rows[i]["ssn"].ObjToString();
                    ssn = FunCustomer.FixSSN(ssn);
                    dt.Rows[i]["ssn"] = ssn;

                    downPayment = dt.Rows[i]["downPayment"].ObjToDouble(); // Several Time this down payment is zero, if so, get it from the Daily History
                    if ( downPayment == 0D)
                    {
                        downPayment = DailyHistory.GetDownPaymentFromPayments(contractNumber);
                        if (downPayment > 0D)
                            dt.Rows[i]["downPayment"] = downPayment;
                    }
                    if (allowInsurance > 0D)
                    {
                        if ((allowInsurance % 150) == 0)
                        {
                            if (allowInsurance == 750D || allowInsurance == 1500D || allowInsurance == 2250D )
                                dt.Rows[i]["IFX"] = "X"; // Insurance Funded
                            else
                                dt.Rows[i]["CAX"] = "X"; // Class A Insurance
                        }
                        else
                            dt.Rows[i]["IFX"] = "X"; // Insurance Funded
                    }
                }
            }
            catch ( Exception ex)
            {
            }

            for ( int i=(dt.Rows.Count-1); i>=0; i--)
            {
                contractValue = dt.Rows[i]["contractValue"].ObjToDouble();
                if (contractValue == 0D)
                    dt.Rows.RemoveAt(i);
            }

            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            originalDt = dt;
            this.Cursor = Cursors.Default;
            button1.Show();
        }
        /***********************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string contract = dr["contractNumber"].ObjToString();
            string cmd = "Select * from `contracts` where `contractNumber` = '" + contract + "';";
            DataTable ddx = G1.get_db_data(cmd);
            if (ddx.Rows.Count <= 0) // Maybe Insurance
            {
                cmd = "Select * from `icustomers` where `payer` = '" + contract + "';";
                ddx = G1.get_db_data(cmd);
                if (ddx.Rows.Count <= 0)
                {
                    MessageBox.Show("***ERROR*** Cannot find Contract or Payer!");
                    return;
                }
                contract = ddx.Rows[0]["contractNumber"].ObjToString();
            }
            if (!String.IsNullOrWhiteSpace(contract))
            {
                this.Cursor = Cursors.WaitCursor;
                DataTable dt = (DataTable)dgv.DataSource;
                bool insurance = false;
                if (contract.ToUpper().IndexOf("ZZ") == 0)
                    insurance = true;
                if (contract.ToUpper().IndexOf("MM") == 0)
                    insurance = true;
                if (contract.ToUpper().IndexOf("OO") == 0)
                    insurance = true;
                if (insurance)
                {
                    cmd = "Select * from `policies` p JOIN `icustomers` d ON p.`contractNumber` = d.`contractNumber` JOIN `icontracts` x ON p.`contractNumber` = x.`contractNumber` ";
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
        /***********************************************************************************************/
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        /***********************************************************************************************/
        private void btnLeft_Click(object sender, EventArgs e)
        {
            string period = cmbDateType.Text.ToUpper();
            if (period == "MONTHLY")
            {
                DateTime now = this.dateTimePicker2.Value;
                now = now.AddMonths(-1);
                int days = DateTime.DaysInMonth(now.Year, now.Month);
                now = new DateTime(now.Year, now.Month, days);
                this.dateTimePicker2.Value = now;
                now = new DateTime(now.Year, now.Month, 1);
                this.dateTimePicker1.Value = now;
            }
            else // must be quarterly
            {
                DateTime now = this.dateTimePicker2.Value;
                now = now.AddMonths(-1);
                int days = DateTime.DaysInMonth(now.Year, now.Month);
                now = new DateTime(now.Year, now.Month, days);
                this.dateTimePicker2.Value = now;
                now = now.AddMonths(-2);
                now = new DateTime(now.Year, now.Month, 1);
                this.dateTimePicker1.Value = now;
            }
        }
        /***********************************************************************************************/
        private void btnRight_Click(object sender, EventArgs e)
        {
            string period = cmbDateType.Text.ToUpper();
            if (period == "MONTHLY")
            {
                DateTime now = this.dateTimePicker2.Value;
                now = now.AddMonths(1);
                int days = DateTime.DaysInMonth(now.Year, now.Month);
                now = new DateTime(now.Year, now.Month, days);
                this.dateTimePicker2.Value = now;
                now = new DateTime(now.Year, now.Month, 1);
                this.dateTimePicker1.Value = now;
            }
            else // must be quarterly
            {
                DateTime now = this.dateTimePicker2.Value;
                now = now.AddMonths(1);
                int days = DateTime.DaysInMonth(now.Year, now.Month);
                now = new DateTime(now.Year, now.Month, days);
                this.dateTimePicker2.Value = now;
                now = now.AddMonths(-2);
                now = new DateTime(now.Year, now.Month, 1);
                this.dateTimePicker1.Value = now;
            }
        }
        /***********************************************************************************************/
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            if (e.Column.FieldName.Trim().ToUpper() == "IF")
            {
                string record = dr["record"].ObjToString();
                string ifc = dr["IF"].ObjToString().ToUpper();
                dr["IF"] = ifc;
                G1.update_db_table("contracts", "record", record, new string[] { "IF", ifc});
            }
            else if (e.Column.FieldName.Trim().ToUpper() == "CA")
            {
                string record = dr["record"].ObjToString();
                string ca = dr["CA"].ObjToString().ToUpper();
                dr["CA"] = ca;
                G1.update_db_table("contracts", "record", record, new string[] { "CA", ca });
            }
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

            printingSystem1.Document.AutoFitToPagesWidth = 1;

            G1.PrintPreview(printableComponentLink1, gridMain);

            //printableComponentLink1.CreateDocument();
            //printableComponentLink1.ShowPreview();

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

            printingSystem1.Document.AutoFitToPagesWidth = 1;

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
            Printer.DrawQuad(5, 8, 4, 4, "Stop/Loss Trust Data", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            DateTime date = this.dateTimePicker1.Value;
            string workDate = date.Month.ToString("D2") + "/" + date.Day.ToString("D2") + "/" + (date.Year % 100).ToString("D2");

            date = this.dateTimePicker2.Value;
            string workDate1 = date.Month.ToString("D2") + "/" + date.Day.ToString("D2") + "/" + (date.Year % 100).ToString("D2");

            string str = "Report : " + workDate + " - " + workDate1;

            Printer.SetQuadSize(24, 12);
            font = new Font("Ariel", 9, FontStyle.Bold);
            Printer.DrawQuad(19, 8, 5, 4, str, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(16, 8, 3, 4, lblPayment.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(19, 8, 3, 4, lblTrust85.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(22, 8, 3, 4, lblTrust100.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /***********************************************************************************************/
        private void chkComboLocation_EditValueChanged(object sender, EventArgs e)
        {
            if (originalDt == null)
                return;
            string names = getLocationQuery();
            DataRow[] dRows = originalDt.Select(names);
            DataTable dt = originalDt.Clone();
            for (int i = 0; i < dRows.Length; i++)
                dt.ImportRow(dRows[i]);
            DataView tempview = dt.DefaultView;
            tempview.Sort = "contractNumber asc";
            dt = tempview.ToTable();
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
        }
        /*******************************************************************************************/
        private string getLocationQuery()
        {
            string procLoc = "";
            string[] locIDs = this.chkComboLocation.EditValue.ToString().Split('|');
            for (int i = 0; i < locIDs.Length; i++)
            {
                if (!String.IsNullOrWhiteSpace(locIDs[i]))
                {
                    if (procLoc.Trim().Length > 0)
                        procLoc += ",";
                    procLoc += "'" + locIDs[i].Trim() + "'";
                }
            }
            return procLoc.Length > 0 ? " `loc` IN (" + procLoc + ") " : "";
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
        private void button1_Click(object sender, EventArgs e)
        {
            if (dgv == null)
                return;
            if (dgv.DataSource == null)
                return;


            DataTable dt = (DataTable)dgv.DataSource;

            Dictionary<string, string> MailMerge = new Dictionary<string, string>()
                    {
                        { "INSURED", "ROBBY GRAHAM" },
                        { "B_DATE\\@ \"MM/DD/YYYY\"", "11/08/1952" },
                        { "SS\\# \"000'-'00'-'0000\"", "425-98-4073" },
                        { "TRUST_NO", "L17035UI" },
                        { "CT_AMT\\# $,#0.00", "$6,500.51" },
                        { "ADDRESS", "351 Service Road" },
                        { "CITY", "Laurel" },
                        { "ST", "MS." },
                        { "ZIP", "39443" },
                        //{ "CT_DATE \\@\"MM/DD/YYYY\"", "07/15/2017" },
                        { "INL_PAYMENT\\#$,#0.00", "$600.00" },
                        { "TF", "X" },
                        { "C_A", "" },
                        { "IF", "" },
                    };

            //{ "CT_DATE \\@\"MM/DD/YYYY\"", "07/15/2017" },
            this.Cursor = Cursors.WaitCursor;

            string detailFile = "C:/SMFSdata/SECRETARY OF STATE RECOVERY FEE SHEET.docx";

            byte[] b = null;
            var tmpFile = "C:/SMFSdata/10PN002_Default_File.docx";

            string cmd = "Select * from `arrangementforms` where `formName` = '" + wordTemplateName + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                string record = dx.Rows[0]["record"].ObjToString();

                b = dx.Rows[0]["image"].ObjToBytes();
                if (File.Exists(tmpFile))
                    File.Delete(tmpFile);
                File.WriteAllBytes(tmpFile, b);
                detailFile = tmpFile;
            }

            Object oMissing = System.Reflection.Missing.Value;
            Object oTrue = false;
            Object oFalse = false;
            Microsoft.Office.Interop.Word.Application oWord = new Microsoft.Office.Interop.Word.Application();
            Microsoft.Office.Interop.Word.Document oWordDoc = new Microsoft.Office.Interop.Word.Document();
            Microsoft.Office.Interop.Word.Document tWordDoc = new Microsoft.Office.Interop.Word.Document();
            oWord.Visible = false;
            Object oTemplatePath = detailFile;
            bool visible = false;
            //oWordDoc = oWord.Documents.Add(ref oTemplatePath, ref oMissing, ref oMissing, false);

            //Microsoft.Office.Interop.Word.Application WordApp = new Microsoft.Office.Interop.Word.Application();
            Microsoft.Office.Interop.Word.Document wDoc = oWord.Documents.Add(ref oMissing, ref oMissing, ref oMissing, ref oMissing);

            string insured = "";
            string address = "";
            string city = "";
            string state = "";
            string zip = "";
            string contractNumber = "";
            DateTime date = DateTime.Now;
            string bDate = "";
            string cDate = "";
            string downPayment = "";
            string contractValue = "";
            string ssn = "";
            double dValue = 0D;
            string ifx = "";
            string tf = "";
            string cax = "";
            int count = 0;

            int lastRow = dt.Rows.Count;
            //lastRow = 16;

            barImport.Minimum = 0;
            barImport.Maximum = lastRow;
            barImport.Show();
            labelAll.Show();

            for (int i = 0; i < lastRow; i++)
            {
                try
                {
                    if (i % 100 == 0)
                        GC.Collect();
                    barImport.Value = (i+1);
                    barImport.Refresh();
                    labelAll.Text = (i+1).ToString() + " of " + lastRow.ToString();
                    labelAll.Refresh();

                    oWordDoc = oWord.Documents.Add(ref oTemplatePath, ref oMissing, ref oMissing, false);

                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                    insured = dt.Rows[i]["fullname"].ObjToString();
                    address = dt.Rows[i]["address1"].ObjToString();
                    city = dt.Rows[i]["city"].ObjToString();
                    state = dt.Rows[i]["state"].ObjToString();
                    zip = dt.Rows[i]["zip1"].ObjToString();
                    date = dt.Rows[i]["bdate"].ObjToDateTime();
                    bDate = date.ToString("MM/dd/yyyy");
                    date = dt.Rows[i]["issueDate"].ObjToDateTime();
                    cDate = date.ToString("MM/dd/yyyy");
                    ssn = dt.Rows[i]["ssn"].ObjToString();
                    ssn = ssn.Replace("-", "");
                    ssn = FunCustomer.FixSSN(ssn);
                    dValue = dt.Rows[i]["downPayment"].ObjToDouble();
                    downPayment = "$" + G1.ReformatMoney(dValue);
                    dValue = dt.Rows[i]["contractValue"].ObjToDouble();
                    contractValue = "$" + G1.ReformatMoney(dValue);
                    ifx = dt.Rows[i]["IFX"].ObjToString();
                    tf = dt.Rows[i]["TF"].ObjToString();
                    cax = dt.Rows[i]["CAX"].ObjToString();

                    MailMerge["INSURED"] = insured;
                    MailMerge["B_DATE\\@ \"MM/DD/YYYY\""] = bDate;
                    MailMerge["SS\\# \"000'-'00'-'0000\""] = ssn;
                    MailMerge["TRUST_NO"] = contractNumber;
                    MailMerge["CT_AMT\\# $,#0.00"] = contractValue;
                    MailMerge["ADDRESS"] = address;
                    MailMerge["CITY"] = city;
                    MailMerge["ST"] = state;
                    MailMerge["ZIP"] = zip;
                    MailMerge["CT_DATE\\@ \"MM/DD/YYYY\""] = cDate;
                    MailMerge["INL_PAYMENT\\#$,#0.00"] = downPayment;
                    MailMerge["TF"] = tf;
                    MailMerge["IF"] = ifx;
                    MailMerge["C_A"] = cax;

                    TextToWord(oWordDoc, oWord, MailMerge);


                    //Clipboard.Clear();
                    oWordDoc.Activate();
                    oWordDoc.ActiveWindow.Selection.WholeStory();
                    oWordDoc.ActiveWindow.Selection.Copy();

                    wDoc.ActiveWindow.Selection.Paste();
                    count++;
                    if ( (count%4) == 0)
                    {
                        Object objBreak = Word.WdBreakType.wdPageBreak;
                        Object objUnit = Word.WdUnits.wdStory;

                        wDoc.ActiveWindow.Selection.EndKey(ref objUnit, ref oMissing );
                        wDoc.ActiveWindow.Selection.InsertBreak(ref objBreak);
                    }

                    oWordDoc.Close(false);
                    oWordDoc = null;
                }
                catch ( Exception ex)
                {
                }
            }

            string filter = "Word files (*.docx)|*.docx";
            saveFileDialog1.Filter += filter;
            saveFileDialog1.FilterIndex = 0;
            saveFileDialog1.FileName = "";
            if (saveFileDialog1.ShowDialog() != DialogResult.OK)
                return;

            string fileName = saveFileDialog1.FileName;

            //string outputFile = "C:/Users/Robby/Downloads/TestFile.docx";
            string outputFile = fileName;
            if (File.Exists(outputFile))
                File.Delete(outputFile);
            wDoc.SaveAs2(outputFile);
            wDoc.Saved = true;
            wDoc.Close();
            wDoc = null;
            oWord.Quit();
            oWord = null;
            //WordApp.Quit();
            //WordApp = null;
            this.Cursor = Cursors.Default;
            DialogResult result = MessageBox.Show("Do you want to VIEW the results?", "Loss Recovery Results Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            if (result == DialogResult.Yes)
            {
                this.Cursor = Cursors.WaitCursor;
                ArrangementForms aForm = new ArrangementForms(fileName, "", "", true);
                aForm.Show();
                this.Cursor = Cursors.Default;
            }
        }
        /***********************************************************************************************/
        public static void TextToWord(Microsoft.Office.Interop.Word.Document oWordDoc, Microsoft.Office.Interop.Word.Application oWord, Dictionary<string, string> pDictionaryMerge)
        {
            bool gotit = false;
            foreach (Microsoft.Office.Interop.Word.Field myMergeField in oWordDoc.Fields)
            {
                gotit = false;
                Microsoft.Office.Interop.Word.Range rngFieldCode = myMergeField.Code;
                String fieldText = rngFieldCode.Text;
                if (fieldText.StartsWith(" MERGEFIELD"))
                {
                    try
                    {
                        String fieldName = fieldText.Replace("MERGEFIELD", "").Trim();
                        if (fieldName.ToUpper().IndexOf("CT_DATE") >= 0)
                        {
                        }

                        foreach (var item in pDictionaryMerge)
                        {
                            if (fieldName.ToUpper().IndexOf("CT_DATE") >= 0)
                            {
                                if (item.Key.ObjToString().ToUpper().IndexOf("CT_DATE") >= 0)
                                    gotit = true;
                            }
                            if (fieldName == item.Key || gotit )
                            {
                                myMergeField.Select();
                                oWord.Selection.TypeText(item.Value);
                            }
                        }
                    }
                    catch ( Exception ex)
                    {
                    }
                }
            }
        }
        /***********************************************************************************************/
        private void selectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog
            {
                InitialDirectory = @"C:\",
                Title = "Browse Word Files",

                CheckFileExists = true,
                CheckPathExists = true,

                DefaultExt = "docs",
                Filter = "Word files (*.docx)|*.docx",
                FilterIndex = 2,
                RestoreDirectory = true,

                ReadOnlyChecked = true,
                ShowReadOnly = true
            };

            if (openFileDialog1.ShowDialog() != DialogResult.OK)
                return;
            string filename = openFileDialog1.FileName;

            byte[] bte = File.ReadAllBytes(filename); // Put the Reading file

            string record = "";
            string cmd = "Select * from `arrangementforms` where `formName` = '" + wordTemplateName + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
                record = dt.Rows[0]["record"].ObjToString();

            if (String.IsNullOrWhiteSpace(record))
            {
                record = G1.create_record("arrangementforms", "type", "-1");
                if (G1.BadRecord("arrangementforms", record))
                    return;
                G1.update_db_table("arrangementforms", "record", record, new string[] { "type", "", "formName", wordTemplateName, "location", "Other" });
            }

            G1.update_blob("arrangementforms", "record", record, "image", bte);

            cmd = "Select * from `arrangementforms` where `formName` = '" + wordTemplateName + "';";
            dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                record = dt.Rows[0]["record"].ObjToString();

                byte[] b = dt.Rows[0]["image"].ObjToBytes();

                var tmpFile = "C:/SMFSdata/10PN002_Default_File.docx";
                try
                {
                    File.WriteAllBytes(tmpFile, b);
                }
                catch ( Exception ex)
                {
                }
            }
        }
        /***********************************************************************************************/
    }
}