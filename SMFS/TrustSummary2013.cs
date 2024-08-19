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
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class TrustSummary2013 : DevExpress.XtraEditors.XtraForm
    {
        private bool autoRun = false;
        private bool autoForce = false;
        private string sendTo = "";
        private string sendWhere = "";
        private string emailLocations = "";
        /****************************************************************************************/
        DataTable originalDt = null;
        /***********************************************************************************************/
        public TrustSummary2013()
        {
            InitializeComponent();
        }
        /***********************************************************************************************/
        private void TrustSummary2013_Load(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            now = now.AddMonths(-1);
            now = new DateTime(now.Year, now.Month, 1);
            this.dateTimePicker1.Value = now;
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            now = new DateTime(now.Year, now.Month, days);
            this.dateTimePicker2.Value = now;
        }
        /***********************************************************************************************/
        private void btnRun_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            DateTime date = dateTimePicker1.Value;
            string date1 = G1.DateTimeToSQLDateTime(date);
            date = dateTimePicker2.Value;
            string date2 = G1.DateTimeToSQLDateTime(date);

            DataTable dt = new DataTable();

            dt.Columns.Add("num");
            dt.Columns.Add("date");
            dt.Columns.Add("beginningTBB", Type.GetType("System.Double"));
            dt.Columns.Add("ytdPrevious", Type.GetType("System.Double"));
            dt.Columns.Add("payments", Type.GetType("System.Double"));
            dt.Columns.Add("ytdPrevRemovals", Type.GetType("System.Double"));
            dt.Columns.Add("removals", Type.GetType("System.Double"));
            dt.Columns.Add("endingTBB", Type.GetType("System.Double"));
            dt.Columns.Add("newTBB", Type.GetType("System.Double"));
            dt.Columns.Add("newEndingTBB", Type.GetType("System.Double"));

            string contractNumber = "";
            string str = "";

            DataRow dRow = null;

            DateTime beginDate = dateTimePicker1.Value;
            DateTime endDate = dateTimePicker2.Value;

            date = beginDate;
            double beginningTBB = 0D;
            double ytdPrevious = 0D;
            double endingTBB = 0D;
            double payments = 0D;
            double ytdPrevRemovals = 0D;
            double removals = 0D;
            double newEndingTBB = 0D;
            double newTBB = 0D;

            for (; ;)
            {
                try
                {
                    PullTheData(date, ref beginningTBB, ref ytdPrevious, ref payments, ref ytdPrevRemovals, ref removals, ref endingTBB);

                    dRow = dt.NewRow();
                    str = date.ObjToDateTime().ToString("MM/dd/yyyy");
                    dRow["date"] = str;
                    dRow["beginningTBB"] = beginningTBB;
                    dRow["ytdPrevious"] = ytdPrevious;
                    dRow["payments"] = payments;
                    dRow["ytdPrevRemovals"] = ytdPrevRemovals;
                    dRow["removals"] = removals;
                    dRow["endingTBB"] = endingTBB;
                    dRow["newTBB"] = newTBB;

                    newEndingTBB = beginningTBB + ytdPrevious + payments - ytdPrevRemovals - removals;
                    dRow["newEndingTBB"] = newEndingTBB;

                    newTBB = newEndingTBB;

                    dt.Rows.Add(dRow);
                }
                catch ( Exception ex)
                {
                }

                date = date.AddMonths(1);
                if (date > endDate)
                    break;
            }


            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            originalDt = dt;
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void PullTheData( DateTime pullDate, ref double beginningTBB, ref double ytdPrevious, ref double monthlyPayments, ref double ytdPrevRemovals, ref double monthlyRemovals, ref double endingTBB )
        {
            beginningTBB = 0D;
            ytdPrevious = 0D;
            monthlyPayments = 0D;
            ytdPrevRemovals = 0D;
            monthlyRemovals = 0D;
            endingTBB = 0D;

            this.Cursor = Cursors.WaitCursor;
            string Y2002 = "";
            if (chk2002.Checked)
                Y2002 = "2002";

            string cmd = "Select * from `trust2013r` a JOIN `customers` c ON a.`contractNumber` = c.`contractNumber` ";
            cmd += " JOIN `contracts` x ON a.`contractNumber` = x.`contractNumber` ";
            if (chk2002.Checked)
                cmd += " where `Is2002` = '2002' ";
            else
                cmd += " where `Is2002` <> '2002' ";

            DateTime date = pullDate;

            string date1 = date.Year.ToString("D4") + "-" + date.Month.ToString("D2") + "-01 00:00:00";
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            string date2 = date.Year.ToString("D4") + "-" + date.Month.ToString("D2") + "-" + days.ToString("D2") + " 23:59:59";

            cmd += " AND `payDate8` >= '" + date1 + "' AND `payDate8` <= '" + date2 + "' ";

            if (SMFS.activeSystem.ToUpper() == "RILES")
                cmd += " AND a.`riles` = 'Y' ";
            else
                cmd += " AND a.`riles` <> 'Y' ";

            cmd += ";";

            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
            {
                return;
            }

            cmd = "Select * from `trust2013r` a ";
            if (chk2002.Checked)
                cmd += " where `Is2002` = '2002' ";
            else
                cmd += " where `Is2002` <> '2002' ";

            cmd += " AND `payDate8` >= '" + date1 + "' AND `payDate8` <= '" + date2 + "' ";

            if (SMFS.activeSystem.ToUpper() == "RILES")
                cmd += " AND a.`riles` = 'Y' ";
            else
                cmd += " AND a.`riles` <> 'Y' ";

            cmd += ";";
            //if (1 == 1)
            //    return dt;

            DataTable dx = G1.get_db_data(cmd); // This is because not all that is in trust2013 is in the SMFS database

            //dx = SMFS.FilterForRiles(dx);

            string contractNumber = "";
            DataRow[] dRows = null;
            DataRow dR = null;
            double removed = 0D;
            double refunded = 0D;

            try
            {
                this.Cursor = Cursors.WaitCursor;
                for (int i = 0; i < dx.Rows.Count; i++)
                {
                    Application.DoEvents();
                    contractNumber = dx.Rows[i]["contractNumber"].ObjToString();
                    dRows = dt.Select("contractNumber = '" + contractNumber + "'"); // Avoid Duplicate Records
                    if (dRows.Length <= 0)
                    {
                        dR = dt.NewRow();
                        dR["contractNumber"] = contractNumber;
                        dR["firstName"] = dx.Rows[i]["firstName"].ObjToString();
                        dR["lastName"] = dx.Rows[i]["lastName"].ObjToString();
                        dR["address1"] = dx.Rows[i]["address2013"].ObjToString();
                        dR["address2"] = "";
                        dR["city"] = dx.Rows[i]["city2013"].ObjToString();
                        dR["state"] = dx.Rows[i]["state2013"].ObjToString();
                        dR["zip1"] = dx.Rows[i]["zip2013"].ObjToString();
                        dR["zip2"] = "";
                        dR["ssn"] = dx.Rows[i]["ssn2013"].ObjToString();
                        dR["payDate8"] = dx.Rows[i]["payDate8"];
                        dR["beginningBalance"] = dx.Rows[i]["beginningBalance"].ObjToDouble();
                        dR["paymentCurrMonth"] = dx.Rows[i]["paymentCurrMonth"].ObjToDouble();
                        dR["ytdPrevious"] = dx.Rows[i]["ytdPrevious"].ObjToDouble();
                        dR["currentPayments"] = dx.Rows[i]["currentPayments"].ObjToDouble();
                        dR["interest"] = dx.Rows[i]["interest"].ObjToDouble();
                        dR["locind"] = dx.Rows[i]["locind"].ObjToString();
                        dR["deathRemYTDPrevious"] = dx.Rows[i]["deathRemYTDPrevious"].ObjToDouble();
                        dR["deathRemCurrMonth"] = dx.Rows[i]["deathRemCurrMonth"].ObjToDouble();
                        dR["refundRemYTDPrevious"] = dx.Rows[i]["refundRemYTDPrevious"].ObjToDouble();
                        dR["refundRemCurrMonth"] = dx.Rows[i]["refundRemCurrMonth"].ObjToDouble();
                        dR["currentRemovals"] = dx.Rows[i]["currentRemovals"].ObjToDouble();
                        dR["endingBalance"] = dx.Rows[i]["endingBalance"].ObjToDouble();
                        dR["location"] = dx.Rows[i]["location"].ObjToString();

                        removed = dx.Rows[i]["deathRemYTDPrevious"].ObjToDouble() + dx.Rows[i]["deathRemCurrMonth"].ObjToDouble();
                        refunded = dx.Rows[i]["refundRemYTDPrevious"].ObjToDouble() + dx.Rows[i]["refundRemCurrMonth"].ObjToDouble();

                        if (removed > 0D)
                            dR["trustRemoved"] = "Y";
                        if (refunded > 0D)
                            dR["trustRefunded"] = "Y";
                        //dR["dateRemoved"] = G1.DTtoMySQLDT(dx.Rows[i]["dateRemoved"].ObjToDateTime());
                        dR["ServiceID"] = dx.Rows[i]["ServiceID"].ObjToString();
                        if (chk2002.Checked)
                            dR["Is2002"] = "2002";
                        dt.Rows.Add(dR);

                    }
                    //dt.ImportRow(dx.Rows[i]);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** " + ex.Message.ToString());
            }

            double tbb = 0D;
            double ebb = 0D;
            double payments = 0D;
            double ytdPrevR = 0D;
            double removals = 0D;

            double ytdP = 0;

            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    tbb = dt.Rows[i]["beginningBalance"].ObjToDouble();
                    ytdP = dt.Rows[i]["ytdPrevious"].ObjToDouble();
                    payments = dt.Rows[i]["paymentCurrMonth"].ObjToDouble();
                    removals = dt.Rows[i]["deathRemCurrMonth"].ObjToDouble() + dt.Rows[i]["refundRemCurrMonth"].ObjToDouble();
                    ytdPrevR = dt.Rows[i]["deathRemYTDPrevious"].ObjToDouble() + dt.Rows[i]["refundRemYTDPrevious"].ObjToDouble();
                    ebb = dt.Rows[i]["endingBalance"].ObjToDouble();

                    beginningTBB += tbb;
                    ytdPrevious += ytdP;
                    monthlyPayments += payments;
                    monthlyRemovals += removals;
                    ytdPrevRemovals += ytdPrevR;
                    endingTBB += ebb;

                }
            }
            catch ( Exception ex )
            {
            }

            //if (PerformYearEnd)
            //    CleanupYearEnd(dt);
            return;
        }
        /***********************************************************************************************/
        private void CleanupYearEnd(DataTable dt)
        {
            if (this.dateTimePicker2.Value.Month != 1)
                return;
            string str = "";
            double originalBeginningBalance = 0D;
            double beginningBalance = 0D;
            double endingBalance = 0D;
            double currentPayments = 0D;
            double currentRemovals = 0D;
            double interest = 0D;
            DateTime runDate = this.dateTimePicker2.Value;
            DateTime date = DateTime.Now;
            DateTime thisMonth = new DateTime(runDate.Year, runDate.Month, 1);
            int removeCount = 0;
            double totalRemovals = 0D;
            bool remove = false;
            string contractNumber = "";

            //DataTable rDt = dt.Clone();

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                if (contractNumber == "C17007U")
                {
                }
                remove = false;
                beginningBalance = dt.Rows[i]["beginningBalance"].ObjToDouble();
                originalBeginningBalance = beginningBalance;
                endingBalance = dt.Rows[i]["endingBalance"].ObjToDouble();
                endingBalance = G1.RoundValue(endingBalance);
                currentPayments = dt.Rows[i]["currentPayments"].ObjToDouble();
                currentRemovals = dt.Rows[i]["deathRemYTDPrevious"].ObjToDouble();
                currentRemovals += dt.Rows[i]["refundRemYTDPrevious"].ObjToDouble();
                currentRemovals += dt.Rows[i]["deathRemCurrMonth"].ObjToDouble();
                currentRemovals += dt.Rows[i]["refundRemCurrMonth"].ObjToDouble();
                if (currentRemovals > 0D)
                {
                    totalRemovals += beginningBalance;
                    remove = true;
                    //G1.copy_dt_row(dt, i, rDt, rDt.Rows.Count);
                }
                interest = dt.Rows[i]["interest"].ObjToDouble();
                currentRemovals = currentRemovals - interest;
                beginningBalance = beginningBalance + currentPayments;
                beginningBalance = G1.RoundValue(beginningBalance);
                if (remove && endingBalance != 0D)
                {
                    beginningBalance = endingBalance;
                    remove = false;
                }
                str = dt.Rows[i]["trustRemoved"].ObjToString().Trim().ToUpper();
                if (str == "YES")
                {
                    date = dt.Rows[i]["dateRemoved"].ObjToDateTime();
                    if (date < thisMonth.AddMonths(-1))
                    {
                        if (currentRemovals != 0D)
                            beginningBalance = 0D;
                        if (endingBalance != 0D)
                        {
                            remove = false;
                            beginningBalance = endingBalance;
                        }
                    }
                }
                str = dt.Rows[i]["trustRefunded"].ObjToString().Trim().ToUpper();
                if (str == "YES")
                {
                    date = dt.Rows[i]["dateRemoved"].ObjToDateTime();
                    if (date < thisMonth.AddMonths(-1))
                    {
                        if (currentRemovals != 0D)
                            beginningBalance = 0D;
                        if (endingBalance != 0D)
                        {
                            remove = false;
                            beginningBalance = endingBalance;
                        }
                    }
                }
                if (remove)
                {
                    beginningBalance = 0D;
                    dt.Rows[i]["interest"] = 0D;
                }
                dt.Rows[i]["beginningBalance"] = beginningBalance;
                dt.Rows[i]["deathRemYTDPrevious"] = 0D;
                dt.Rows[i]["deathRemCurrMonth"] = 0D;
                dt.Rows[i]["refundRemYTDPrevious"] = 0D;
                dt.Rows[i]["refundRemCurrMonth"] = 0D;
                dt.Rows[i]["ytdPrevious"] = 0D;
                dt.Rows[i]["endingBalance"] = endingBalance;
            }
        }
        /***********************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
        }
        /***********************************************************************************************/
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
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

                string filename = path + @"\Trust_Summary_" + today.Year.ToString("D4") + today.Month.ToString("D2") + today.Day.ToString("D2") + ".pdf";
                if (File.Exists(filename))
                    File.Delete(filename);
                printableComponentLink1.ExportToPdf(filename);
                RemoteProcessing.AutoRunSendTo("Trust Summary Report", filename, sendTo, sendWhere, emailLocations);
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
            Printer.DrawQuad(5, 8, 4, 4, "Trust TBB Summary Report", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            DateTime date = this.dateTimePicker1.Value;
            string workDate = date.Month.ToString("D2") + "/" + date.Day.ToString("D2") + "/" + (date.Year % 100).ToString("D2");

            date = this.dateTimePicker2.Value;
            workDate += " - " + date.Month.ToString("D2") + "/" + date.Day.ToString("D2") + "/" + (date.Year % 100).ToString("D2");

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
                //if (e.DisplayText.IndexOf("0000") >= 0 || e.DisplayText.IndexOf("0001") >= 0)
                //    e.DisplayText = "";
                //else
                //{
                //    DateTime date = e.DisplayText.ObjToDateTime();
                //    if (date.Year < 500)
                //        e.DisplayText = "";
                //    else
                //        e.DisplayText = date.Year.ToString("yyyy") + date.Month.ToString("MM") + date.Day.ToString("dd");
                //}
            }
            else if (e.DisplayText == "0.00")
                e.DisplayText = "-    ";
        }
        /***********************************************************************************************/
        private void btnLeft_Click(object sender, EventArgs e)
        {
            DateTime now = this.dateTimePicker1.Value;
            now = now.AddMonths(-1);
            now = new DateTime(now.Year, now.Month, 1);
            this.dateTimePicker1.Value = now;

            now = this.dateTimePicker2.Value;
            now = now.AddMonths(-1);
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            now = new DateTime(now.Year, now.Month, days);
            this.dateTimePicker2.Value = now;
        }
        /***********************************************************************************************/
        private void btnRight_Click(object sender, EventArgs e)
        {
            DateTime now = this.dateTimePicker1.Value;
            now = now.AddMonths(1);
            now = new DateTime(now.Year, now.Month, 1);
            this.dateTimePicker1.Value = now;

            now = this.dateTimePicker2.Value;
            now = now.AddMonths(1);
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            now = new DateTime(now.Year, now.Month, days);
            this.dateTimePicker2.Value = now;
        }
        /***********************************************************************************************/
    }
}