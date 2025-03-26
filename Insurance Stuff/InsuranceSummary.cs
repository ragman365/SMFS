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
using System.Linq;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class InsuranceSummary : DevExpress.XtraEditors.XtraForm
    {
        private bool autoRun = false;
        private bool autoForce = false;
        private string sendTo = "";
        private string sendWhere = "";
        private string emailLocations = "";
        /****************************************************************************************/
        DataTable originalDt = null;
        /***********************************************************************************************/
        public InsuranceSummary()
        {
            InitializeComponent();
        }
        /****************************************************************************************/
        public InsuranceSummary(bool auto, bool force)
        {
            autoRun = auto;
            autoForce = force;
            InitializeComponent();
            RunAutoReports();
        }
        /****************************************************************************************/
        private void RunAutoReports()
        {
            string cmd = "Select * from `remote_processing`;";
            DataTable dt = G1.get_db_data(cmd);
            string report = "";
            DateTime date = DateTime.Now;
            long currentDay = G1.date_to_days(date.ToString("MM/dd/yyyy"));
            int presentDay = date.Day;
            int dayToRun = 0;
            string status = "";
            string frequency = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                report = dt.Rows[i]["report"].ObjToString();
                if (report.ToUpper() != "INSURANCE SUMMARY")
                    continue;
                status = dt.Rows[i]["status"].ObjToString();
                if (status.ToUpper() == "INACTIVE")
                    continue;
                if (!autoForce)
                {
                    dayToRun = dt.Rows[i]["day_to_run"].ObjToInt32();
                    frequency = dt.Rows[i]["dateIncrement"].ObjToString();
                    if (!AutoRunSetup.CheckOkToRun(dayToRun, frequency))
                        return;
                }
                report = dt.Rows[i]["report"].ObjToString();
                sendTo = dt.Rows[i]["sendTo"].ObjToString();
                sendWhere = dt.Rows[i]["sendWhere"].ObjToString();
                InsuranceSummary_Load(null, null);
            }
        }
        /***********************************************************************************************/
        private void InsuranceSummary_Load(object sender, EventArgs e)
        {
            if (autoRun)
            {
                btnRun_Click(null, null);
                DataTable dt = (DataTable)dgv.DataSource;
                emailLocations = DailyHistory.ParseOutLocations(dt);

                printPreviewToolStripMenuItem_Click(null, null);
                this.Close();
            }
        }
        /***********************************************************************************************/
        private void btnRun_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            string search = "ZZ0001476";
            string cmd = "Select * from `icontracts` x JOIN `icustomers` c ON x.`contractNumber` = c.`contractNumber` JOIN `policies` p ON x.`contractNumber` = p.`contractNumber` WHERE x.`deceasedDate` < '1805-01-01' AND x.`lapsed` <> 'Y' AND x.`contractNumber` LIKE 'ZZ%' ORDER BY c.`lastName`,c.`firstName`;";
            //cmd = "Select * from `icontracts` x JOIN `icustomers` c ON x.`contractNumber` = c.`contractNumber` WHERE x.`deceasedDate` < '1805-01-01' AND x.`lapsed` <> 'Y' AND x.`contractNumber` LIKE 'ZZ%' AND x.`contractNumber` = 'ZZ0001476' ORDER BY c.`lastName`,c.`firstName`;";
            cmd = "Select * from `icontracts` x JOIN `icustomers` c ON x.`contractNumber` = c.`contractNumber` WHERE x.`deceasedDate` < '1805-01-01' AND x.`lapsed` <> 'Y' AND x.`contractNumber` LIKE 'ZZ%' ORDER BY c.`lastName`,c.`firstName`;";
            DataTable dt = G1.get_db_data(cmd);

            DataView tempview = dt.DefaultView;
            tempview.Sort = "lastName asc, firstName asc, payer asc, dueDate8 desc";
            dt = tempview.ToTable();


            int fCount = dt.Rows.Count;

            Trust85.FindContract ( dt, "ZZ0000540");

            //dt = CustomerDetails.filterSecNat(true, dt);
            //Trust85.FindContract(dt, "ZZ0002111");

            int lCount = dt.Rows.Count;

            dt.Columns.Add("num");
            dt.Columns.Add("issueDate");
            dt.Columns.Add("nnx");
            dt.Columns.Add("phone");
            dt.Columns.Add("ddate");
            dt.Columns.Add("idate");
            dt.Columns.Add("ldate");

            DateTime date = DateTime.Now;
            DateTime deceasedDate = DateTime.Now;
            string contractNumber = "";
            string payer = "";
            string phone = "";
            string nnx = "";
            string firstName = "";
            string lastName = "";
            int idx = 0;

            int lastRow = dt.Rows.Count;
            //lastRow = 50;

            string oldLast = "";
            string oldFirst = "";
            string oldPayer = "";
            DateTime oldDueDate = DateTime.Now;
            DateTime dueDate8 = DateTime.Now;

            for (int i = 0; i < lastRow; i++)
            {
                payer = dt.Rows[i]["payer"].ObjToString();
                if ( payer == "CC-3015")
                {
                }
                lastName = dt.Rows[i]["lastName"].ObjToString();
                firstName = dt.Rows[i]["firstName"].ObjToString();
                dueDate8 = dt.Rows[i]["dueDate8"].ObjToDateTime();
                try
                {
                    if (String.IsNullOrWhiteSpace(payer))
                    {
                        oldPayer = payer;
                        oldLast = lastName;
                        oldFirst = firstName;
                        oldDueDate = dueDate8;
                    }
                    if (payer == oldPayer)
                    {
                        if (oldLast == lastName && oldFirst == firstName)
                        {
                            if (dueDate8 <= oldDueDate)
                                dt.Rows[i]["payer"] = "";
                        }
                    }
                    else
                    {
                        oldPayer = payer;
                        oldFirst = firstName;
                        oldLast = lastName;
                        oldDueDate = dueDate8;
                    }
                }
                catch ( Exception ex)
                {
                }
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                phone = dt.Rows[i]["phoneNumber"].ObjToString();
                idx = phone.IndexOf("0");
                if (idx > 0)
                    phone = phone.Substring(idx);
                phone = phone.Replace(")", "");
                phone = phone.Trim();
                if (phone.Length == 7)
                {
                    nnx = phone.Substring(0, 3);
                    phone = phone.Substring(3);
                    dt.Rows[i]["nnx"] = nnx;
                    dt.Rows[i]["phone"] = phone;
                }
            }

            try
            {
                for (int i = (lastRow-1); i >= 0; i--)
                {
                    payer = dt.Rows[i]["payer"].ObjToString();
                    if (String.IsNullOrWhiteSpace(payer))
                        dt.Rows.RemoveAt(i);
                }
            }
            catch ( Exception ex)
            {
            }
            Trust85.FindContract(dt, "ZZ0000540");

            DataTable newDt = CleanupSecNat(dt);

            Trust85.FindContract(dt, "ZZ0000540");

            G1.NumberDataTable(newDt);
            dgv.DataSource = newDt;
            originalDt = newDt;
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void btnRun_Click2(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            string search = "ZZ0001476";
            string cmd = "Select * from `icontracts` x JOIN `icustomers` c ON x.`contractNumber` = c.`contractNumber` JOIN `policies` p ON x.`contractNumber` = p.`contractNumber` WHERE x.`deceasedDate` < '1805-01-01' AND x.`lapsed` <> 'Y' AND x.`contractNumber` LIKE 'ZZ%' ORDER BY c.`lastName`,c.`firstName`;";
            cmd = "Select * from `policies` p LEFT JOIN `icontracts` x ON p.`contractNumber` = x.`contractNumber` LEFT JOIN `icustomers` c ON p.`contractNumber` = c.`contractNumber` WHERE p.`report` = 'NOT THIRD PARTY' AND x.`deceasedDate` < '1805-01-01' AND x.`lapsed` <> 'Y' AND x.`contractNumber` LIKE 'ZZ%' ORDER by p.`contractNumber` ;";
            DataTable dt = G1.get_db_data(cmd);

            DataView tempview = dt.DefaultView;
            tempview.Sort = "lastName asc, firstName asc, payer asc, dueDate8 desc";
            dt = tempview.ToTable();


            int fCount = dt.Rows.Count;

            dt = GetGroupData(dt, "payer");

            Trust85.FindContract(dt, "ZZ0000540");

            tempview = dt.DefaultView;
            tempview.Sort = "lastName1 asc, firstName1 asc, payer asc, dueDate8 desc";
            dt = tempview.ToTable();


            //dt = CustomerDetails.filterSecNat(true, dt);
            //Trust85.FindContract(dt, "ZZ0002111");

            int lCount = dt.Rows.Count;

            dt.Columns.Add("num");
            dt.Columns.Add("issueDate");
            dt.Columns.Add("nnx");
            dt.Columns.Add("phone");
            dt.Columns.Add("ddate");
            dt.Columns.Add("idate");
            dt.Columns.Add("ldate");

            DateTime date = DateTime.Now;
            DateTime deceasedDate = DateTime.Now;
            string contractNumber = "";
            string payer = "";
            string phone = "";
            string nnx = "";
            string firstName = "";
            string lastName = "";
            int idx = 0;

            int lastRow = dt.Rows.Count;
            //lastRow = 50;

            string oldLast = "";
            string oldFirst = "";
            string oldPayer = "";
            DateTime oldDueDate = DateTime.Now;
            DateTime dueDate8 = DateTime.Now;

            for (int i = 0; i < lastRow; i++)
            {
                payer = dt.Rows[i]["payer"].ObjToString();
                if (payer == "CC-3015")
                {
                }
                lastName = dt.Rows[i]["lastName1"].ObjToString();
                firstName = dt.Rows[i]["firstName1"].ObjToString();
                dueDate8 = dt.Rows[i]["dueDate8"].ObjToDateTime();
                try
                {
                    if (String.IsNullOrWhiteSpace(payer))
                    {
                        oldPayer = payer;
                        oldLast = lastName;
                        oldFirst = firstName;
                        oldDueDate = dueDate8;
                    }
                    if (payer == oldPayer)
                    {
                        if (oldLast == lastName && oldFirst == firstName)
                        {
                            if (dueDate8 <= oldDueDate)
                                dt.Rows[i]["payer"] = "";
                        }
                    }
                    else
                    {
                        oldPayer = payer;
                        oldFirst = firstName;
                        oldLast = lastName;
                        oldDueDate = dueDate8;
                    }
                }
                catch (Exception ex)
                {
                }
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                phone = dt.Rows[i]["phoneNumber"].ObjToString();
                idx = phone.IndexOf("0");
                if (idx > 0)
                    phone = phone.Substring(idx);
                phone = phone.Replace(")", "");
                phone = phone.Trim();
                if (phone.Length == 7)
                {
                    nnx = phone.Substring(0, 3);
                    phone = phone.Substring(3);
                    dt.Rows[i]["nnx"] = nnx;
                    dt.Rows[i]["phone"] = phone;
                }
            }

            try
            {
                for (int i = (lastRow - 1); i >= 0; i--)
                {
                    payer = dt.Rows[i]["payer"].ObjToString();
                    if (String.IsNullOrWhiteSpace(payer))
                        dt.Rows.RemoveAt(i);
                }
            }
            catch (Exception ex)
            {
            }
            Trust85.FindContract(dt, "ZZ0000540");

            DataTable newDt = CleanupSecNat(dt, true );

            Trust85.FindContract(dt, "ZZ0000540");

            G1.NumberDataTable(newDt);
            dgv.DataSource = newDt;
            originalDt = newDt;
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private DataTable CleanupSecNat ( DataTable dt, bool small = false )
        {
            int fCount = dt.Rows.Count;

            DataTable newDt = dt.Clone();
            string cmd = "";
            string contractNumber = "";
            string payer = "";
            string firstName = "";
            string lastName = "";
            DataTable dx = null;
            DataTable testDt = null;
            DataRow[] dRows = null;
            if (!small)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    try
                    {
                        contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                        payer = dt.Rows[i]["payer"].ObjToString();
                        firstName = dt.Rows[i]["firstName"].ObjToString();
                        lastName = dt.Rows[i]["lastName"].ObjToString();
                        if (payer == "CC-3015")
                        {
                        }

                        //                    cmd = "Select * from `policies` p where `payer` = '" + payer + "' and `firstName` = '" + firstName + "' AND `lastName` = '" + lastName + "' AND `tmstamp` > '2020-01-01'";
                        cmd = "Select * from `policies` p where `payer` = '" + payer + "' AND `tmstamp` > '2020-01-01'";
                        cmd += ";";

                        dx = G1.get_db_data(cmd);

                        if (dx.Rows.Count <= 0)
                        {
                            dt.Rows[i]["payer"] = "";
                            continue;
                        }
                        dRows = dx.Select("report='NOT THIRD PARTY'");
                        if (dRows.Length <= 0)
                            dt.Rows[i]["payer"] = "";


                        //testDt = CustomerDetails.filterSecNat(false, dx);
                        //if (testDt.Rows.Count <= 0)
                        //    dt.Rows[i]["payer"] = "";
                        //G1.HardCopyDtRow(dt, i, newDt, newDt.Rows.Count);
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
            double premium = 0D;
            for ( int i=(dt.Rows.Count-1); i>=0; i--)
            {
                try
                {
                    payer = dt.Rows[i]["payer"].ObjToString();
                    if (String.IsNullOrWhiteSpace(payer))
                        dt.Rows.RemoveAt(i);
                    else
                    {
                        premium = Policies.CalcMonthlyPremium(payer, DateTime.Now);
                        dt.Rows[i]["amtOfMonthlyPayt"] = premium;
                    }
                }
                catch ( Exception ex)
                {
                }
            }
            return dt;
        }
        /***********************************************************************************************/
        private DataTable GetGroupData(DataTable dt, string group)
        {
            if (dt.Rows.Count <= 0)
                return dt;
            if (G1.get_column_number(dt, "Int32_id") < 0)
                dt.Columns.Add("Int32_id", typeof(int), "record");

            DataTable groupDt = dt.AsEnumerable().GroupBy(r => new { Col1 = r[group] }).Select(g => g.OrderBy(r => r["Int32_id"]).First()).CopyToDataTable();
            groupDt.Columns.Remove("Int32_id");
            return groupDt;
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
                cmd = "Select * from `icustomers` where `contractNumber` = '" + contract + "';";
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
        }
        /***********************************************************************************************/
        private void btnRight_Click(object sender, EventArgs e)
        {
        }
        /***********************************************************************************************/
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
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

            G1.AdjustColumnWidths(gridMain, 0.65D, true);

            printableComponentLink1.CreateDocument();
            if (autoRun)
            {
                string path = G1.GetReportPath();
                DateTime today = DateTime.Now;

                string filename = path + @"\Insurance_Summary_" + today.Year.ToString("D4") + today.Month.ToString("D2") + today.Day.ToString("D2") + ".pdf";
                if (File.Exists(filename))
                    File.Delete(filename);
                printableComponentLink1.ExportToPdf(filename);
                RemoteProcessing.AutoRunSendTo("Insurance Summary Report", filename, sendTo, sendWhere, emailLocations);
            }
            else
                printableComponentLink1.ShowPreview();

            G1.AdjustColumnWidths(gridMain, 0.65D, false );

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
            if (!chkIncludeHeader.Checked)
                return;
            Printer.setupPrinterQuads(e, 2, 3);
            Font font = new Font("Ariel", 16);
            Printer.DrawQuad(1, 1, Printer.xQuads, 2, "South Mississippi Funeral Services, LLC", Color.Black, BorderSide.Top, font, HorizontalAlignment.Center);

            Printer.SetQuadSize(12, 12);

            font = new Font("Ariel", 8);
            Printer.DrawGridDate(2, 3, 2, 3, Color.Black, BorderSide.None, font);
            Printer.DrawGridPage(11, 3, 2, 3, Color.Black, BorderSide.None, font);

            Printer.DrawQuad(1, 9, 2, 3, "User : " + LoginForm.username, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);


            font = new Font("Ariel", 10, FontStyle.Bold);
            Printer.DrawQuad(5, 8, 4, 4, "Insurance Summary Report", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

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
        private void btnCompareDates_Click(object sender, EventArgs e)
        {
            string oldPayer = "";
            string oldPayerFirstName = "";
            string oldPayerLastName = "";
            string oldDueDate = "";
            string oldIssueDate = "";
            string oldLastDatePaid8 = "";
            DataRow[] dR = null;
            DataTable dx = (DataTable)dgv.DataSource;
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string file = ofd.FileName;
                    this.Cursor = Cursors.WaitCursor;
                    DataTable dt = Import.ImportCSVfile(file);
                    this.Cursor = Cursors.WaitCursor;
                    try
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            oldPayer = dt.Rows[i]["PAYER#"].ObjToString();
                            if (String.IsNullOrWhiteSpace(oldPayer))
                                continue;
                            oldPayerFirstName = dt.Rows[i]["payer first Name"].ObjToString();
                            oldPayerFirstName = G1.protect_data(oldPayerFirstName);
                            oldPayerLastName = dt.Rows[i]["payer last Name"].ObjToString();
                            oldPayerLastName = G1.protect_data(oldPayerLastName);
                            oldDueDate = dt.Rows[i]["due date"].ObjToString();
                            oldLastDatePaid8 = dt.Rows[i]["last paid date"].ObjToString();
                            oldIssueDate = oldLastDatePaid8;
                            try
                            {
                                dR = dx.Select("payer='" + oldPayer + "' AND firstname='" + oldPayerFirstName + "' AND lastname='" + oldPayerLastName + "'");
                            }
                            catch ( Exception ex)
                            {
                            }
                            if (dR.Length > 0)
                            {
                                dR[0]["ddate"] = oldDueDate;
                                dR[0]["idate"] = oldLastDatePaid8;
                                dR[0]["ldate"] = oldLastDatePaid8;
                            }
                            else
                            {

                            }
                        }
                        gridMain.Columns["ddate"].Visible = true;
                        gridMain.Columns["idate"].Visible = true;
                        gridMain.Columns["ldate"].Visible = true;
                        gridMain.Columns["issueDate8"].Visible = true;
                        gridMain.Columns["lastDatePaid8"].Visible = true;
                    }
                    catch ( Exception ex)
                    {
                        MessageBox.Show("***ERROR*** " + ex.Message.ToString());
                    }
                    this.Cursor = Cursors.Default;
                }
            }
        }
        /***********************************************************************************************/
    }
}