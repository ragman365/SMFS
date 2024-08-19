using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Base;
using System.Globalization;
using System.IO;
using DevExpress.XtraPrinting;
using DevExpress.Utils;

using GeneralLib;
using DevExpress.XtraEditors;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class ImportRiles : DevExpress.XtraEditors.XtraForm
    {
        private string actualFile = "";
        private string workFix = "";
        /***********************************************************************************************/
        public ImportRiles( string fix = "" )
        {
            InitializeComponent();
            workFix = fix;
        }
        /***********************************************************************************************/
        private void ImportRiles_Load(object sender, EventArgs e)
        {
            btnImportRiles.Hide();
            picLoader.Hide();
            if (workFix.ToUpper() == "FIX")
                this.Text = "Fix Imported Riles Contract Data";
        }
        /***********************************************************************************************/
        private void btnSelectFile_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string file = ofd.FileName;
                    int idx = file.LastIndexOf("\\");
                    if (idx > 0)
                    {
                        actualFile = file.Substring(idx);
                        actualFile = actualFile.Replace("\\", "");
                    }
                    dgv.DataSource = null;
                    this.Cursor = Cursors.WaitCursor;
                    DataTable dt = Import.ImportCSVfile(file);
                    this.Cursor = Cursors.Default;
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        G1.NumberDataTable(dt);
                        dgv.DataSource = dt;
                        btnImportRiles.Show();
                    }
                }
            }
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void btnImportRiles_Click(object sender, EventArgs e)
        {
            picLoader.Show();
            string contractNumber = "";

            string firstName = "";
            string lastName = "";
            string cmd = "";
            string record = "";
            string customerRecord = "";
            string contractRecord = "";

            string merchandise = "";
            string cashAdvances = "";
            string services = "";

            string str = "";

            string balanceDue = "";
            string downPayment = "";
            DateTime dateOfDownPayment = DateTime.Now;

            string address = "";
            string city = "";
            string state = "";
            string zip = "";
            string sex = "";
            string ssn = "";

            DateTime dob = DateTime.Now;
            DateTime dod = DateTime.Now;
            DateTime issueDate8 = DateTime.Now;
            DateTime dolp = DateTime.Now;
            DateTime nullDate = new DateTime(1, 1, 1);

            DataTable dt = (DataTable)dgv.DataSource;
            DateTime date = DateTime.Now;
            DateTime oldDate = DateTime.Now;

            labelMaximum.Show();
            labelMaximum.Text = "0";
            barImport.Show();

            int lastrow = dt.Rows.Count;
            //lastrow = 10;

            lblTotal.Show();
            barImport.Minimum = 0;
            barImport.Maximum = lastrow;
            lblTotal.Text = "of " + lastrow.ToString();
            lblTotal.Refresh();

            DataRow[] dRows = null;

            string myFields = "";

            DataTable paymentDt = new DataTable();
            paymentDt.Columns.Add("payDate8");
            paymentDt.Columns.Add("payment");
            paymentDt.Columns.Add("downPayment");
            DataRow dR = null;

            string paymentDateCol = "";
            string paymentAmountCol = "";
            string datePaid8 = "";
            string paymentAmount = "";
            string amtOfMonthlyPayment = "";
            string fundingPCT = "";

            G1.CreateAudit("RilesImport");

            DataTable dx = null;
            bool newRecord = false;

            for (int i = 0; i < lastrow; i++)
            {
                Application.DoEvents();

                picLoader.Refresh();
                barImport.Value = i;
                barImport.Refresh();
                labelMaximum.Text = i.ToString();
                labelMaximum.Refresh();

                try
                {
                    contractRecord = "";
                    customerRecord = "";
                    newRecord = false;
                    contractNumber = "RF" + dt.Rows[i]["CONTRACT"].ObjToString();

                    firstName = dt.Rows[i]["First"].ObjToString();
                    lastName = dt.Rows[i]["Last"].ObjToString();

                    firstName = G1.protect_data(firstName);
                    lastName = G1.protect_data(lastName);

                    sex = dt.Rows[i]["sex"].ObjToString().ToUpper();
                    if (sex == "M")
                        sex = "male";
                    else if (sex == "F")
                        sex = "female";

                    ssn = dt.Rows[i]["SOCSEC"].ObjToString();
                    ssn = ssn.Replace("-", "");

                    address = dt.Rows[i]["address"].ObjToString();
                    address = G1.protect_data(address);
                    city = dt.Rows[i]["city"].ObjToString();
                    state = dt.Rows[i]["state"].ObjToString();
                    zip = dt.Rows[i]["zip"].ObjToString();

                    str = dt.Rows[i]["Date Of Birth"].ObjToString();
                    str = str.Replace("**", "01");
                    dob = str.ObjToDateTime();
                    if (dob.Year < 100)
                        dob = nullDate;

                    dod = dt.Rows[i]["Date Of Death"].ObjToDateTime();
                    if (dod.Year < 100)
                        dod = nullDate;

                    issueDate8 = dt.Rows[i]["Contract Date"].ObjToDateTime();
                    if (issueDate8.Year < 100)
                        issueDate8 = nullDate;

                    dolp = dt.Rows[i]["Date of Last Payment"].ObjToDateTime();
                    if (dolp.Year < 100)
                        dolp = nullDate;

                    merchandise = dt.Rows[i]["Merchandise"].ObjToString();
                    services = dt.Rows[i]["Services"].ObjToString();
                    cashAdvances = dt.Rows[i]["cash advances"].ObjToString();
                    balanceDue = dt.Rows[i]["amount due"].ObjToString();
                    downPayment = dt.Rows[i]["down payment"].ObjToString();

                    fundingPCT = dt.Rows[i]["FUNDING PCT"].ObjToString();

                    dateOfDownPayment = dt.Rows[i]["Down Payment Date"].ObjToDateTime();
                    if (dateOfDownPayment.Year < 100)
                        dateOfDownPayment = nullDate;

                    paymentDt.Rows.Clear();
                    amtOfMonthlyPayment = "";

                    try
                    {
                        dR = paymentDt.NewRow();
                        dR["payDate8"] = dateOfDownPayment.ToString("MM/dd/yyyy");
                        dR["downPayment"] = downPayment;
                        paymentDt.Rows.Add(dR);

                        for (int j = 1; j <= 57; j++)
                        {
                            paymentDateCol = "Installment Payment " + j.ToString() + " Date";
                            paymentAmountCol = "Installment Payment " + j.ToString() + " Amount";

                            datePaid8 = dt.Rows[i][paymentDateCol].ObjToString();
                            paymentAmount = dt.Rows[i][paymentAmountCol].ObjToString();
                            if (String.IsNullOrWhiteSpace(amtOfMonthlyPayment))
                                amtOfMonthlyPayment = paymentAmount;

                            dR = paymentDt.NewRow();
                            dR["payDate8"] = datePaid8;
                            dR["payment"] = paymentAmount;
                            paymentDt.Rows.Add(dR);
                        }
                    }
                    catch (Exception ex)
                    {
                    }

                    if ( workFix == "FIX" )
                    {
                        cmd = "Select * from `contracts` where `contractNumber` = '" + contractNumber + "';";
                        dx = G1.get_db_data(cmd);
                        if ( dx.Rows.Count <= 0 )
                        {
                            MessageBox.Show("***ERROR*** Riles Contract Not Found! : " + contractNumber, "Riles Contract Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                            continue;
                        }
                        contractRecord = dx.Rows[0]["record"].ObjToString();
                        G1.update_db_table("contracts", "record", contractRecord, new string[] { "trustPercent", fundingPCT });
                        continue;
                    }

                    cmd = "Select * from `contracts` where `contractNumber` = '" + contractNumber + "';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count <= 0)
                    {
                        contractRecord = G1.create_record("contracts", "notes", "-1");
                        newRecord = true;
                    }
                    else
                        contractRecord = dx.Rows[0]["record"].ObjToString();
                    if (G1.BadRecord("contracts", contractRecord))
                    {
                        G1.WriteAudit("***ERROR*** Row " + i.ToString() + " Record Problem with Contract " + contractNumber + " !");
                        continue;
                    }
                    if (newRecord)
                        G1.update_db_table("contracts", "record", contractRecord, new string[] { "contractNumber", contractNumber });

                    newRecord = false;
                    cmd = "Select * from `customers` where `contractNumber` = '" + contractNumber + "';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count <= 0)
                    {
                        customerRecord = G1.create_record("customers", "firstName", "-1");
                        newRecord = true;
                    }
                    else
                        customerRecord = dx.Rows[0]["record"].ObjToString();
                    if (G1.BadRecord("customers", customerRecord))
                    {
                        G1.WriteAudit("***ERROR*** Row " + i.ToString() + " Record Problem with Customer " + contractNumber + " !");
                        continue;
                    }
                    if (newRecord)
                        G1.update_db_table("customers", "record", customerRecord, new string[] { "contractNumber", contractNumber });

                    G1.update_db_table("contracts", "record", contractRecord, new string[] { "downPayment", downPayment, "serviceTotal", services, "merchandiseTotal", merchandise, "cashAdvance", cashAdvances, "balanceDue", balanceDue, "lastDatePaid8", dolp.ToString("MM/dd/yyyy"), "issueDate8", issueDate8.ToString("MM/dd/yyyy"), "deceasedDate", dod.ToString("MM/dd/yyyy"), "amtOfMonthlyPayt", amtOfMonthlyPayment });
                    G1.update_db_table("customers", "record", customerRecord, new string[] { "firstName", firstName, "lastName", lastName,"deceasedDate", dod.ToString("MM/dd/yyyy"), "address1", address, "city", city, "state", state, "zip1", zip, "sex", sex, "ssn", ssn, "birthDate", dob.ToString("MM/dd/yyyy"), "contractDate", issueDate8.ToString("MM/dd/yyyy") });

                    for ( int j=0; j<paymentDt.Rows.Count; j++)
                    {
                        str = paymentDt.Rows[j]["payDate8"].ObjToString();
                        if (String.IsNullOrWhiteSpace(str))
                            continue;
                        datePaid8 = paymentDt.Rows[j]["payDate8"].ObjToString();
                        paymentAmount = paymentDt.Rows[j]["payment"].ObjToString();
                        downPayment = paymentDt.Rows[j]["downPayment"].ObjToString();

                        date = datePaid8.ObjToDateTime();

                        newRecord = false;
                        record = "";
                        cmd = "Select * from `payments` where `contractNumber` = '" + contractNumber + "' AND `payDate8` = '" + date.ToString("yyyy-MM-dd") + "';";
                        dx = G1.get_db_data(cmd);
                        if ( dx.Rows.Count <= 0 )
                        {
                            record = G1.create_record("payments", "firstName", "-1");
                            newRecord = true;
                        }
                        else
                            record = dx.Rows[0]["record"].ObjToString();

                        if (G1.BadRecord("payments", record))
                        {
                            G1.WriteAudit("***ERROR*** Row " + i.ToString() + " Record Problem with Payments " + contractNumber + " !");
                            continue;
                        }
                        if (newRecord)
                            G1.update_db_table("payments", "record", record, new string[] { "contractNumber", contractNumber });

                        G1.update_db_table("payments", "record", record, new string[] { "firstName", firstName, "lastName", lastName, "downPayment", downPayment, "paymentAmount", paymentAmount, "payDate8", date.ToString("MM/dd/yyyy")  });

                    }
                }
                catch ( Exception ex)
                {
                }
            }
            G1.NumberDataTable(dt);
            barImport.Value = lastrow;
            picLoader.Hide();
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
            Printer.DrawQuad(4, 8, 7, 4, this.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            //            Printer.DrawQuadTicks();
            DateTime date = DateTime.Now;
            string workDate = date.Month.ToString("D2") + "/" + date.Year.ToString("D4");
            Printer.SetQuadSize(24, 12);
            font = new Font("Ariel", 9, FontStyle.Bold);

            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /***********************************************************************************************/
    }
}