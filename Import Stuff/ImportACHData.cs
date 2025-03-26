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
using DevExpress.XtraGrid.Views.Base;
using DevExpress.Xpo.Helpers;
using System.IO;
using ExcelLibrary.BinaryFileFormat;
using System.Security.Cryptography;
using DevExpress.XtraPrinting;
using DevExpress.Utils;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class ImportACHData : DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        //private DataTable workDt = null;
        /***********************************************************************************************/
        public ImportACHData()
        {
            InitializeComponent();
            //workDt = dt;
        }
        /***********************************************************************************************/
        private void ImportACHData_Load(object sender, EventArgs e)
        {
            btnRun.Hide();
            //workDt.Columns.Add("frequency");
            //workDt.Columns.Add("status");

            //DataTable dxx = null;
            //string str = "";
            //string frequency = "";
            //string cnum = "";
            //string status = "";
            //string payer = "";
            //double payment = 0D;
            //double expected = 0D;
            //this.Cursor = Cursors.WaitCursor;
            //for (int i = 0; i < workDt.Rows.Count; i++)
            //{
            //    str = workDt.Rows[i]["COL 11"].ObjToString();
            //    frequency = "1";
            //    if (str.ToUpper() == "ONCE A YEAR")
            //        frequency = "12";
            //    workDt.Rows[i]["frequency"] = frequency;

            //    cnum = workDt.Rows[i]["Customer Number"].ObjToString();

            //    payer = "";

            //    cnum = cnum.TrimStart('0');
            //    cnum = cnum.Replace(" ", "");

            //    if (String.IsNullOrWhiteSpace(cnum))
            //    {
            //        workDt.Rows[i]["status"] = "NOT FOUND";
            //        continue;
            //    }

            //    string cmd = "Select * from `contracts` where `contractNumber` = '" + cnum + "';";
            //    dxx = G1.get_db_data(cmd);
            //    if (dxx.Rows.Count <= 0)
            //    {
            //        payer = cnum;
            //        string newPayer = "";
            //        bool isLapsed = false;
            //        str = workDt.Rows[i]["Amount"].ObjToString();
            //        payment = 0D;
            //        if (G1.validate_numeric(str))
            //            payment = str.ObjToDouble();

            //        cnum = ImportDailyDeposits.FindPayerContract(payer, payment.ObjToString(), ref newPayer, ref expected, ref isLapsed);
            //        if (!String.IsNullOrWhiteSpace(newPayer))
            //            payer = newPayer;
            //        if (String.IsNullOrWhiteSpace(cnum))
            //        {
            //            workDt.Rows[i]["status"] = "NOT FOUND";
            //            continue;
            //        }
            //    }
            //}
            //G1.NumberDataTable(workDt);
            //dgv.DataSource = workDt;
            //this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void btnRun_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            string origin = "";
            string str = "";
            string[] Lines = null;
            DateTime docp = DateTime.Now;
            string dom = "";
            double payment = 0D;
            string cnum = "";
            string originalPayer = "";
            string payer = "";
            string status = "";
            string code = "";
            string paymentsFile = "";
            bool insurance = false;
            string record = "";
            DataTable dxx = null;
            double expected = 0D;
            string routing = "";
            string checking = "";
            string type = "";
            string sPayment = "";
            string date = "";
            int created = 0;
            int updated = 0;
            int failed = 0;
            int frequency = 1;
            int leftPayments = 0;
            int numPayments = 0;
            string startDate = "";
            string originalCNum = "";
            int lastRow = dt.Rows.Count;
            DateTime sdate = DateTime.Now;
            //lastRow = 3;
            this.Cursor = Cursors.WaitCursor;
            for (int i = 0; i < lastRow; i++)
            {
                try
                {
                    frequency = 1;
                    str = dt.Rows[i]["COL 11"].ObjToString().ToUpper();
                    if (str == "ONCE A YEAR")
                        frequency = 12;
                    else if (str.ToUpper() == "ONCE A QUARTER")
                        frequency = 3;

                    type = dt.Rows[i]["type"].ObjToString();
                    if (type.ToUpper() != "CHECKING" && type.ToUpper() != "SAVINGS")
                        type = "Checking";
                    str = dt.Rows[i]["Start Date"].ObjToString();
                    Lines = str.Split(' ');
                    str = Lines[0].Trim();
                    docp = str.ObjToDateTime();
                    date = docp.ToString("MM/dd/yyyy");
                    dom = docp.Day.ObjToString();

                    str = dt.Rows[i]["Amount"].ObjToString();
                    payment = 0D;
                    if (G1.validate_numeric(str))
                        payment = str.ObjToDouble();
                    sPayment = G1.ReformatMoney(payment);

                    routing = dt.Rows[i]["Routing"].ObjToString();
                    checking = dt.Rows[i]["Account Number"].ObjToString();

                    sdate = dt.Rows[i]["startDate"].ObjToDateTime();
                    startDate = sdate.ToString("yyyy-MM-dd");

                    numPayments = dt.Rows[i]["numPayments"].ObjToInt32();
                    //leftPayments = dt.Rows[i]["leftPayments"].ObjToInt32();
                    leftPayments = dt.Rows[i]["difference"].ObjToInt32();

                    cnum = dt.Rows[i]["Customer Number"].ObjToString();
                    originalCNum = cnum;

                    if (String.IsNullOrWhiteSpace(cnum))
                        cnum = "EMPTY #";

                    code = "01";
                    payer = "";

                    paymentsFile = "payments";
                    insurance = false;

                    originalPayer = cnum;

                    cnum = cnum.TrimStart('0');
                    cnum = cnum.Replace(" ", "");

                    if (String.IsNullOrWhiteSpace(cnum))
                    {
                        MessageBox.Show("***ERROR*** Cannot locate anyone for Number " + originalCNum + "!");
                        failed++;
                        continue;
                    }

                    expected = 0D;

                    string cmd = "Select * from `contracts` where `contractNumber` = '" + cnum + "';";
                    dxx = G1.get_db_data(cmd);
                    if (dxx.Rows.Count <= 0)
                    {
                        payer = cnum;
                        paymentsFile = "ipayments";
                        string newPayer = "";
                        bool isLapsed = false;
                        cnum = ImportDailyDeposits.FindPayerContract(payer, payment.ObjToString(), ref newPayer, ref expected, ref isLapsed);
                        if (!String.IsNullOrWhiteSpace(newPayer))
                            payer = newPayer;
                        if (String.IsNullOrWhiteSpace(cnum))
                        {
                            MessageBox.Show("***ERROR*** Cannot locate anyone for Number " + originalCNum + "!");
                            failed++;
                            continue;
                        }
                        insurance = true;
                        code = "02";
                    }
                    cmd = "Select * from `ach` where `contractNumber` = '" + cnum + "';";
                    //if (insurance)
                    //    cmd = "Select * from `ach` where `payer` = '" + cnum + "';";
                    dxx = G1.get_db_data(cmd);
                    if (dxx.Rows.Count > 0)
                    {
                        record = dxx.Rows[0]["record"].ObjToString();
                        updated++;
                    }
                    else
                    {
                        record = G1.create_record("ach", "contractNumber", "-1");
                        created++;
                    }
                    if (G1.BadRecord("ach", record))
                    {
                        MessageBox.Show("***ERROR*** Creating ACH Record for Number " + originalCNum + "!");
                        failed++;
                        continue;
                    }

                    G1.update_db_table("ach", "record", record, new string[] { "contractNumber", cnum, "payer", payer, "code", code, "dayOfMonth", dom, "frequencyInMonths", "1", "routingNumber", routing, "accountNumber", checking, "acctType", type, "payment", sPayment, "dateBeginning", startDate, "numPayments", numPayments.ObjToString(), "leftPayments", leftPayments.ObjToString(), "legacy", "Y"});
                }
                catch (Exception ex)
                {
                    MessageBox.Show("***ERROR*** Creating ACH Record for Number " + originalCNum + "!");
                    failed++;
                }
            }
            MessageBox.Show("***INFO*** " + created.ToString() + " Created and " + updated.ToString() + " Updated and " + failed.ToString() + " Failed Customers", "Update ACH Customers Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (dgv.Visible)
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
        private void gridMain_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            ColumnView view = sender as ColumnView;
            if (e.Column.FieldName.ToUpper().IndexOf("DATE") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                if (e.DisplayText.IndexOf("0000") >= 0 || e.DisplayText.IndexOf("0001") >= 0)
                    e.DisplayText = "";
                else
                {
                    DateTime date = e.DisplayText.ObjToString().ObjToDateTime();
                    e.DisplayText = date.ToString("MM/dd/yyyy");
                    if (date.Year < 30)
                        e.DisplayText = "";
                }
            }
            else if (e.Column.FieldName.ToUpper().IndexOf("CODE") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                int row = e.ListSourceRowIndex;
                DataTable dt = (DataTable)dgv.DataSource;
                string payer = dt.Rows[row]["payer"].ObjToString();
                string contractNumber = dt.Rows[row]["contractNumber"].ObjToString();
                if (!String.IsNullOrWhiteSpace(payer))
                    e.DisplayText = "02";
                else if (contractNumber.ToUpper().Contains("ZZ"))
                    e.DisplayText = "02";
            }
            //else if (e.Column.FieldName.ToUpper().IndexOf("DIFFERENCE") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            //{
            //    int row = e.ListSourceRowIndex;
            //    DataTable dt = (DataTable)dgv.DataSource;
            //    string leftPayments = dt.Rows[row]["leftPayments"].ObjToString();
            //    string difference = dt.Rows[row]["difference"].ObjToString();
            //    if (difference != leftPayments)
            //        e.Column.AppearanceCell.BackColor = Color.Red;
            //    else
            //        e.Column.AppearanceCell.BackColor = Color.Transparent;
            //}
        }
        /***********************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string contract = dr["Customer Number"].ObjToString();
            if (!String.IsNullOrWhiteSpace(contract))
            {
                this.Cursor = Cursors.WaitCursor;
                string cmd = "Select * from `contracts` where `contractNumber` = '" + contract + "';";
                DataTable dx = G1.get_db_data(cmd);
                if ( dx.Rows.Count <= 0)
                {
                    string cnum = contract.TrimStart('0');
                    cnum = cnum.Replace(" ", "");

                    cmd = "Select * from `icustomers` where `payer` = '" + cnum + "';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                        contract = dx.Rows[0]["contractNumber"].ObjToString();
                }
                CustomerDetails clientForm = new CustomerDetails(contract);
                clientForm.Show();
                this.Cursor = Cursors.Default;
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

            printableComponentLink1.CreateDocument();
            printableComponentLink1.ShowPreview();
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

            font = new Font("Ariel", 12);
            string text = "ACH Data to Import";
            Printer.DrawQuad(6, 7, 4, 4, text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);

            font = new Font("Ariel", 10, FontStyle.Bold);

            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /****************************************************************************************/
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
            printableComponentLink1.PrintDlg();
            isPrinting = false;
        }
        /***********************************************************************************************/
        private void gridMain_CellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            DataTable dt = (DataTable)dgv.DataSource;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            if (gridMain.FocusedColumn == gridMain.Columns["Customer Number"])
            {
                string cnum = dt.Rows[row]["Customer Number"].ObjToString();
                cnum = cnum.TrimStart('0');
                cnum = cnum.Replace(" ", "");

                string cmd = "Select * from `contracts` where `contractNumber` = '" + cnum + "';";
                DataTable dxx = G1.get_db_data(cmd);
                if (dxx.Rows.Count <= 0)
                {
                    string payer = cnum;
                    string newPayer = "";
                    bool isLapsed = false;
                    string str = dt.Rows[row]["Amount"].ObjToString();
                    double expected = 0D;
                    double payment = 0D;
                    if (G1.validate_numeric(str))
                        payment = str.ObjToDouble();

                    cnum = ImportDailyDeposits.FindPayerContract(payer, payment.ObjToString(), ref newPayer, ref expected, ref isLapsed);
                    if (!String.IsNullOrWhiteSpace(newPayer))
                        payer = newPayer;
                    if (String.IsNullOrWhiteSpace(cnum))
                    {
                        dr["status"] = "STILL NOT FOUND";
                        return;
                    }
                }
                dr["status"] = "";
            }
        }
        /***********************************************************************************************/
        private string actualFile = "";
        private void btnSelectFile_Click(object sender, EventArgs e)
        {
            DataTable dt = null;
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
                    try
                    {
                        dt = Import.ImportCSVfile(file);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("***ERROR*** Reading File!");
                        dt = null;
                    }
                }
            }
            this.Cursor = Cursors.Default;
            if ( dt != null )
            {
                if ( dt.Rows.Count > 0 )
                    ImportForm_SelectDone(dt);
            }
        }
        /***********************************************************************************************/
        private void btnPullACH_Click(object sender, EventArgs e)
        {
            btnSelectFile_Click(null, null);
            //Import importForm = new Import("Import Existing ACH Customer Setup");
            //importForm.SelectDone += ImportForm_SelectDone;
            //importForm.Show();
        }
        /***********************************************************************************************/
        private void ImportForm_SelectDone(DataTable dt)
        {
            dt.Columns.Add("frequency");
            dt.Columns.Add("status");
            dt.Columns.Add("startDate");
            dt.Columns.Add("numPayments");
            dt.Columns.Add("leftPayments");
            dt.Columns.Add("difference");

            DataTable payDt = null;

            DataTable dxx = null;
            string str = "";
            string frequency = "";
            string cnum = "";
            string status = "";
            string payer = "";
            double payment = 0D;
            double expected = 0D;
            double amount = 0D;
            double debit = 0D;
            double credit = 0D;
            string startDate = "";
            string leftPayments = "";
            int paymentsLeft = 0;
            int paymentsMade = 0;
            int numPayments = 0;
            int before = 0;
            int totalPayments = 0;
            int difference = 0;

            double amtOfMonthlyPayt = 0D;
            double myPayments = 0D;
            bool insurance = false;
            DateTime date = DateTime.Now;
            DateTime payDate8 = DateTime.Now;
            this.Cursor = Cursors.WaitCursor;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                str = dt.Rows[i]["COL 11"].ObjToString();
                frequency = "1";
                if (str.ToUpper() == "ONCE A YEAR")
                    frequency = "12";
                else if (str.ToUpper() == "ONCE A QUARTER")
                    frequency = "3";
                dt.Rows[i]["frequency"] = frequency;

                cnum = dt.Rows[i]["Customer Number"].ObjToString();

                startDate = dt.Rows[i]["Start Date"].ObjToString();
                date = startDate.ObjToDateTime();

                str = dt.Rows[i]["Number of Payments"].ObjToString();
                numPayments = str.ObjToInt32();

                dt.Rows[i]["startDate"] = startDate;
                dt.Rows[i]["numPayments"] = numPayments.ObjToString();

                totalPayments = 0;
                myPayments = 0D;
                insurance = false;

                payer = "";

                cnum = cnum.TrimStart('0');
                cnum = cnum.Replace(" ", "");

                if (String.IsNullOrWhiteSpace(cnum))
                {
                    dt.Rows[i]["status"] = "NOT FOUND";
                    continue;
                }

                str = dt.Rows[i]["Amount"].ObjToString();
                amount = 0D;
                if (G1.validate_numeric(str))
                    amount = str.ObjToDouble();

                string cmd = "Select * from `contracts` where `contractNumber` = '" + cnum + "';";
                dxx = G1.get_db_data(cmd);
                if (dxx.Rows.Count <= 0)
                {
                    payer = cnum;
                    string newPayer = "";
                    bool isLapsed = false;

                    cnum = ImportDailyDeposits.FindPayerContract(payer, amount.ObjToString(), ref newPayer, ref expected, ref isLapsed);
                    if (!String.IsNullOrWhiteSpace(newPayer))
                        payer = newPayer;
                    if (String.IsNullOrWhiteSpace(cnum))
                    {
                        dt.Rows[i]["status"] = "NOT FOUND";
                        continue;
                    }
                    insurance = true;
                    cmd = "Select * from `ipayments` where `payer` = '" + payer + "' ";
                }
                else
                {
                    if ( cnum == "FF16010UI")
                    {
                    }
                    totalPayments = dxx.Rows[0]["numberOfPayments"].ObjToInt32();
                    amtOfMonthlyPayt = dxx.Rows[0]["amtOfMonthlyPayt"].ObjToDouble();
                    cmd = "Select * from `payments` where `contractNumber` = '" + cnum + "' ";
                }
                //if (date.Year > 100)
                //    cmd += " AND `payDate8` >= '" + date.ToString("yyyy-MM-dd") + "' ";
                cmd += "ORDER by `payDate8` DESC;";
                if ( numPayments == 999 )
                    continue;
                paymentsLeft = 0;
                paymentsMade = 0;
                before = 0;
                try
                {
                    if ( cnum == "WF18075LI")
                    {
                    }
                    payDt = G1.get_db_data(cmd);
                    for (int j = 0; j < payDt.Rows.Count; j++)
                    {
                        if (payDt.Rows[j]["fill"].ObjToString().ToUpper() == "D")
                            continue;
                        payDate8 = payDt.Rows[j]["payDate8"].ObjToDateTime();
                        payment = payDt.Rows[j]["paymentAmount"].ObjToDouble();
                        if ( !insurance)
                            myPayments += payment / amtOfMonthlyPayt;
                        debit = payDt.Rows[j]["debitAdjustment"].ObjToDouble();
                        credit = payDt.Rows[j]["creditAdjustment"].ObjToDouble();
                        if (payDate8 < date)
                        {
                            if (payment == amount)
                                before++;
                            else if (debit == amount)
                                before--;
                            else if (credit == amount)
                                before++;
                            continue;
                        }
                        if (payment == amount)
                            paymentsMade++;
                        else if (debit == amount)
                            paymentsMade--;
                        else if (credit == amount)
                            paymentsMade++;
                    }
                }
                catch ( Exception ex)
                {
                }
                paymentsLeft = numPayments - paymentsMade;
                dt.Rows[i]["leftPayments"] = paymentsLeft.ToString();
                difference = totalPayments - (before + paymentsMade);
                if (!insurance)
                {
                    myPayments = G1.RoundValue(myPayments);
                    difference = Convert.ToInt32(myPayments);
                    //dt.Rows[i]["leftPayments"] = totalPayments - difference;
                    difference = totalPayments - difference;
                }
                dt.Rows[i]["difference"] = difference.ToString();
            }
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            this.Cursor = Cursors.Default;
            btnRun.Show();
            this.Text = "Import ACH Bank Information for " + actualFile;
        }
        /***********************************************************************************************/
        private void gridMain_CustomDrawCell(object sender, RowCellCustomDrawEventArgs e)
        {
            if (e.RowHandle < 0)
                return;
            if (e.Column.FieldName.ToUpper().IndexOf("DIFFERENCE") >= 0)
            {
                DataTable dt = (DataTable)dgv.DataSource;
                int row = gridMain.GetDataSourceRowIndex(e.RowHandle);

                string leftPayments = dt.Rows[row]["leftPayments"].ObjToString();
                string difference = dt.Rows[row]["difference"].ObjToString();
                if (difference != leftPayments)
                    e.Appearance.BackColor = Color.Red;
                else
                    e.Appearance.BackColor = Color.Transparent;
            }
        }
        /***********************************************************************************************/
    }
}