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
using System.IO;
using System.Text.RegularExpressions;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraGrid.Views.Grid;
using System.Drawing.Drawing2D;
using DevExpress.Utils.Drawing;
using DevExpress.Utils;
using DevExpress.XtraEditors.Repository;
using System.Diagnostics.Contracts;
using DevExpress.XtraGrid.Columns;
using System.Configuration;
using DevExpress.XtraPrinting;
using DevExpress.XtraEditors.ViewInfo;
using DevExpress.XtraBars;
using DevExpress.XtraGrid.Views.BandedGrid;
using DevExpress.XtraGrid;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class PayrollReport : DevExpress.XtraEditors.XtraForm
    {
        private bool loading = true;
        private bool modified = false;
        private string primaryName = "";
        /****************************************************************************************/
        public PayrollReport()
        {
            InitializeComponent();

            SetupTotalsSummary();
        }
        /****************************************************************************************/
        private void SetupToolTips()
        {
            ToolTip tt = new ToolTip();
        }
        /****************************************************************************************/
        private void SetupTotalsSummary()
        {
            AddSummaryColumn("gross", gridMain);
        }
        /****************************************************************************************/
        private void AddSummaryColumn(string columnName, DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gMain = null, string format = "")
        {
            if (gMain == null)
                gMain = gridMain;
            //if (String.IsNullOrWhiteSpace(format))
            //    format = "${0:0,0.00}";
            if (String.IsNullOrWhiteSpace(format))
                format = "{0:0,0.00}";
            gMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            gMain.Columns[columnName].SummaryItem.DisplayFormat = format;
        }
        /****************************************************************************************/
        private void AddSummaryColumn(string columnName, string format = "")
        {
            //if (String.IsNullOrWhiteSpace(format))
            //    format = "${0:0,0.00}";
            if (String.IsNullOrWhiteSpace(format))
                format = "{0:0,0.00}";
            gridMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            gridMain.Columns[columnName].SummaryItem.DisplayFormat = format;
        }
        /***********************************************************************************************/
        private void LoadPayPeriods()
        {

            DataTable locDt = new DataTable();
            locDt.Columns.Add("payPeriods");

            DateTime date1 = dateTimePicker1.Value;
            DateTime date2 = dateTimePicker2.Value;

            DateTime startDate = DateTime.Now;
            DateTime stopDate = DateTime.Now;
            DateTime now = DateTime.Now;
            now = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);

            DateTime newDate = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
            DateTime testDate = DateTime.Now;
            DateTime beginDate = new DateTime(2022, 12, 23);
            try
            {
                for (; ; )
                {
                    newDate = beginDate;
                    testDate = newDate.AddDays(13);
                    if (now >= newDate && now <= newDate.AddDays(13))
                    {
                        startDate = newDate;
                        break;
                    }
                    beginDate = beginDate.AddDays(14);
                }
            }
            catch (Exception ex)
            {
                startDate = DateTime.Now;
            }


            DateTime last = beginDate;
            DateTime date = last.AddDays(14); // Move (2) weeks ahead to start
            DateTime workTime = DateTime.Now;
            for (; ; )
            {
                if (workTime > date)
                {
                    date1 = date;
                    date2 = date.AddDays(14);
                    break;
                }
                date = date.AddDays(-14); // Go back in time
            }

            DataRow dR = null;
            string payPeriod = "";

            for (int i = 26; i >= 1; i--)
            {
                payPeriod = date2.AddDays(-14).Month.ToString("D2") + "/" + date2.AddDays(-14).Day.ToString("D2") + "/" + date2.AddDays(-14).Year.ToString("D4");
                payPeriod += " - " + date2.Month.ToString("D2") + "/" + date2.Day.ToString("D2") + "/" + date2.Year.ToString("D4");
                dR = locDt.NewRow();
                dR["payPeriods"] = payPeriod;
                locDt.Rows.Add(dR);

                date1 = date1.AddDays(-14);
                date2 = date2.AddDays(-14);
            }
            chkComboLocation.Properties.DataSource = locDt;
        }
        /****************************************************************************************/
        private void SetupEmployeeTimes()
        {
            DateTime startDate = DateTime.Now;
            DateTime stopDate = DateTime.Now;
            DateTime now = DateTime.Now;
            now = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);

            DateTime newDate = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
            DateTime testDate = DateTime.Now;
            DateTime beginDate = new DateTime(2022, 12, 23);
            try
            {
                for (; ; )
                {
                    newDate = beginDate;
                    testDate = newDate.AddDays(13);
                    if (now >= newDate && now <= newDate.AddDays(13))
                    {
                        startDate = newDate;
                        break;
                    }
                    beginDate = beginDate.AddDays(14);
                }
            }
            catch (Exception ex)
            {
                startDate = DateTime.Now;
            }
            //int count = 0;
            //for (; ; )
            //{
            //    DayOfWeek dow = now.DayOfWeek;
            //    if (dow == DayOfWeek.Friday)
            //    {
            //        count++;
            //        if (count >= 2)
            //        {
            //            startDate = now;
            //            break;
            //        }
            //        now = now.AddDays(-1);
            //        continue;
            //    }
            //    now = now.AddDays(-1);
            //}

            newDate = new DateTime(startDate.Year, startDate.Month, startDate.Day, 0, 0, 0);
            startDate = newDate;
            stopDate = startDate.AddDays(14);
            newDate = new DateTime(stopDate.Year, stopDate.Month, stopDate.Day, 23, 59, 59);
            stopDate = newDate;

            this.dateTimePicker1.Value = startDate;
            this.dateTimePicker2.Value = stopDate;

            stopDate = this.dateTimePicker2.Value;
            DateTime checkDate = stopDate.AddDays(-14);
            DateTime date1 = new DateTime(checkDate.Year, checkDate.Month, checkDate.Day, 17, 0, 0);
            if (DateTime.Now <= date1)
            {
                this.dateTimePicker1.Value = this.dateTimePicker1.Value.AddDays(-14);
                this.dateTimePicker2.Value = this.dateTimePicker2.Value.AddDays(-14);
            }
        }
        /****************************************************************************************/
        private void PayrollReport_Load(object sender, EventArgs e)
        {
            oldWhat = "";

            SetupToolTips();

            LoadPayPeriods();

            loading = true;

            DateTime now = DateTime.Now;
//            now = now.AddMonths(-1);
            now = new DateTime(now.Year, now.Month, 1);
            this.dateTimePicker1.Value = now;
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            this.dateTimePicker2.Value = new DateTime(now.Year, now.Month, days);

            SetupEmployeeTimes();

            LoadLocations();

            //LoadData();

            modified = false;
            loading = false;
        }
        /****************************************************************************************/
        private void btnRight_Click(object sender, EventArgs e)
        {
            DateTime startDate = dateTimePicker1.Value;
            startDate = startDate.AddDays(14);

            DateTime stopDate = dateTimePicker2.Value;
            stopDate = stopDate.AddDays(14);

            this.dateTimePicker1.Value = startDate;
            this.dateTimePicker2.Value = stopDate;
        }
        /***********************************************************************************************/
        private void btnLeft_Click(object sender, EventArgs e)
        {
            DateTime startDate = dateTimePicker1.Value;
            startDate = startDate.AddDays(-14);

            DateTime stopDate = dateTimePicker2.Value;
            stopDate = stopDate.AddDays(-14);

            this.dateTimePicker1.Value = startDate;
            this.dateTimePicker2.Value = stopDate;
        }
        /***********************************************************************************************/
        private void SetupCompleted(DataTable dt)
        {
            DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew = this.repositoryItemCheckEdit1;
            selectnew.NullText = "";
            selectnew.ValueChecked = "1";
            selectnew.ValueUnchecked = "0";
            selectnew.ValueGrayed = "";

            string completed = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                completed = dt.Rows[i]["completed"].ObjToString();
                if ( completed == "1")
                    dt.Rows[i]["completed"] = "1";
                else
                    dt.Rows[i]["completed"] = "0";
            }
        }
        /***********************************************************************************************/
        private void LoadLocations()
        {
            string cmd = "Select * from `funeralhomes` order by `LocationCode`;";
            DataTable locDt = G1.get_db_data(cmd);
            DataRow dRow = locDt.NewRow();
            dRow["LocationCode"] = "Home Office";
            locDt.Rows.InsertAt(dRow, 0);
            dRow = locDt.NewRow();
            dRow["LocationCode"] = "All";
            locDt.Rows.InsertAt(dRow, 0);
            cmbLocation.DataSource = locDt;
        }
        /****************************************************************************************/
        private void gridMain_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            int row = e.ListSourceRow;
            if (dgv == null)
                return;
            if (dgv.DataSource == null)
                return;
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt.Rows.Count <= 0)
                return;

            //string contactType = dt.Rows[row]["contactType"].ObjToString().ToUpper();
            //if ( contactType != cType )
            //{
            //    e.Visible = false;
            //    e.Handled = true;
            //}
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
                return;
            }
            int rowHandle = e.RowHandle;
            if (rowHandle < 0)
                return;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            if (dgv.DataSource == null)
                return;
            try
            {
                DataTable dt = (DataTable)dgv.DataSource;
                bool doDate = false;
                if (e.Column.FieldName == "apptDate")
                    doDate = true;
                //else if (e.Column.FieldName == "lastContactDate")
                //    doDate = true;

                if (doDate)
                {
                    if (!String.IsNullOrWhiteSpace(e.DisplayText.Trim()))
                    {
                        DateTime date = e.DisplayText.ObjToString().ObjToDateTime();
                        if (date.Year < 30)
                            e.DisplayText = "";
                        else
                        {
                            e.DisplayText = date.ToString("MM/dd/yyyy");
                        }
                    }
                }
            }
            catch ( Exception ex)
            {
            }
        }
        /****************************************************************************************/
        private bool justSaved = false;
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            //if (e == null)
            //    return;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            DataRow dr = gridMain.GetFocusedDataRow();
            if (dr == null)
                return;

            dr["mod"] = "Y";


            GridColumn currCol = gridMain.FocusedColumn;
            string currentColumn = currCol.FieldName;
            string what = dr[currentColumn].ObjToString();
            if (currentColumn.ToUpper() == "contactName")
            {
                what = dr[currentColumn].ObjToString();

                if (String.IsNullOrWhiteSpace(what))
                    return;
                bool found = false;

                string contactType = dr["contactType"].ObjToString();
                if (!String.IsNullOrWhiteSpace(contactType))
                {

                    DataTable cDt = null;
                    string cmd = "Select * from `track` WHERE `contactType` = '" + contactType + "' AND `answer` LIKE '%" + what + "%' ;";
                    cDt = G1.get_db_data(cmd);
                    if ( cDt.Rows.Count > 0 )
                    {
                        what = cDt.Rows[0]["answer"].ObjToString();
                        dr["contactName"] = what;
                    }
                }
            }
            if (currentColumn.ToUpper() == "NUM")
                return;
            string record = dr["record"].ObjToString();
            if (!String.IsNullOrWhiteSpace(record))
            {
                if ( currentColumn.ToUpper() == "APPTDATE")
                {
                    DateTime date = what.ObjToDateTime();
                    what = date.ToString("yyyy-MM-dd");
                }
                try
                {
                    G1.update_db_table("contacts", "record", record, new string[] { currentColumn, what });
                }
                catch ( Exception ex)
                {
                }
            }

            gridMain.RefreshData();
        }
        /****************************************************************************************/
        private void UpdateMod(DataRow dr)
        {
            dr["mod"] = "Y";
            modified = true;
        }
        /***********************************************************************************************/
        private void AddMod(DataTable dt, DevExpress.XtraGrid.Views.Grid.GridView grid)
        {
            if (G1.get_column_number(dt, "mod") < 0)
                dt.Columns.Add("mod");
        }
        /****************************************************************************************/
        private void GoToLastRow (DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gridMain )
        {
            if (gridMain == null)
                return;
            if (gridMain.GridControl == null)
                return;
            DevExpress.XtraGrid.GridControl dgv = gridMain.GridControl;
            if (dgv == null)
                return;
            if (dgv.DataSource == null)
                return;

            try
            {
                DataTable dt = (DataTable)dgv.DataSource;
                int row = dt.Rows.Count - 1;
                gridMain.SelectRow(row);
                gridMain.FocusedRowHandle = row;
                gridMain.RefreshData();
                dgv.RefreshDataSource();
                dgv.Refresh();
            }
            catch ( Exception ex )
            {
            }
        }
        /****************************************************************************************/
        private void pictureBox4_Click(object sender, EventArgs e)
        {
            G1.SpyGlass(gridMain);
            //if (gridMain.OptionsFind.AlwaysVisible == true)
            //    gridMain.OptionsFind.AlwaysVisible = false;
            //else
            //    gridMain.OptionsFind.AlwaysVisible = true;
        }
        /****************************************************************************************/
        private void gridMain_KeyDown(object sender, KeyEventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            //AddMod(dt, gridMain);
        }
        /****************************************************************************************/
        private void gridMain_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            if (e.ListSourceRowIndex == DevExpress.XtraGrid.GridControl.InvalidRowHandle)
                return;
            string name = e.Column.FieldName;
            if ( name.ToUpper().IndexOf("DATE") >= 0 )
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
            bool doDate = false;
            bool doTime = false;
            if (name == "apptDate")
                doDate = true;
            else if (name == "lastContactDate")
                doDate = true;

            if (doDate)
            {
                DateTime date = e.DisplayText.ObjToString().ObjToDateTime();
                if (date.Year < 30)
                    e.DisplayText = "";
                else
                {
                    e.DisplayText = date.ToString("MM/dd/yyyy");
                }
            }

            if (doTime)
            {
                if (!String.IsNullOrWhiteSpace(e.DisplayText.Trim()))
                {
                    DateTime date = e.DisplayText.ObjToString().ObjToDateTime();
                    e.DisplayText = date.ToString("HH:mm");
                }
            }
        }
        /****************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            DataTable dt = (DataTable)dgv.DataSource;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetFocusedDataSourceRowIndex();
            string record = dr["record"].ObjToString();

            string contactName = dr["contactName"].ObjToString();
            string contactType = dr["contactType"].ObjToString();
            if (String.IsNullOrWhiteSpace(contactName))
                return;
            using ( ContactHistory historyForm = new ContactHistory ( gridMain, dt, row, record, contactType, contactName, null ))
            {
                historyForm.contactHistoryDone += HistoryForm_contactHistoryDone;
                historyForm.ShowDialog();
            }
        }
        /****************************************************************************************/
        private string HistoryForm_contactHistoryDone(DataTable dt, bool somethingDeleted )
        {
            if (dt.Rows.Count <= 0)
                return "";

            DataTable dx = (DataTable)dgv.DataSource;

            bool found = false;
            string record = "";
            string results = "";
            string completed = "";
            string mod = "";
            bool foundDelete = false;
            DataRow[] dRows = null;
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                record = dt.Rows[i]["record"].ObjToString();
                results = dt.Rows[i]["results"].ObjToString();
                completed = dt.Rows[i]["completed"].ObjToString();
                mod = dt.Rows[i]["mod"].ObjToString();

                dRows = dx.Select("record='" + record + "'");
                if ( dRows.Length > 0 )
                {
                    found = true;
                    if (mod == "D")
                    {
                        G1.delete_db_table("contacts", "record", record);

                        dx.Rows.Remove(dRows[0]);
                        G1.NumberDataTable(dx);
                        foundDelete = true;
                    }
                    else
                    {
                        G1.copy_dr_row(dt.Rows[i], dRows[0] );
                        //dRows[0]["results"] = results;
                        //dRows[0]["completed"] = completed;
                        //dRows[0]["mod"] = mod;
                    }
                }
            }

            if ( found )
            {
                gridMain.RefreshData();
                gridMain.RefreshEditor(true);
                dgv.Refresh();
            }
            return "";
        }
        /****************************************************************************************/
        private void Contacts_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Validate();
            gridMain.RefreshEditor(true);
        }
        /****************************************************************************************/
        private void gridMain_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();

            int focusedRow = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(focusedRow);

            //string initialized = dt.Rows[row]["initialized"].ObjToString();

            //string saveDescription = dr["localDescription"].ObjToString();
            //string saveBank = dr["bankAccount"].ObjToString();

            //try
            //{
            //    string type = dr["type"].ObjToString().ToUpper();
            //    string what = dr["status"].ObjToString().ToUpper();
            //    row = gridMain.GetDataSourceRowIndex(row);
            //    //if ( !loading )
            //    //    dt.Rows[row]["dateModified"] = G1.DTtoMySQLDT(DateTime.Now);
            //    if (what.ToUpper() == "DEPOSITED")
            //    {
            //        string bankAccount = GetDepositBankAccount(type);
            //        if (!String.IsNullOrWhiteSpace(bankAccount))
            //        {
            //            dr["bankAccount"] = bankAccount;
            //            dt.Rows[row]["bankAccount"] = bankAccount;
            //            gridMain.RefreshEditor(true);
            //            dgv.RefreshDataSource();
            //            dgv.Refresh();
            //        }
            //    }
            //    else
            //    {
            //        saveBank = "";
            //        saveDescription = "";
            //        dr["bankAccount"] = "";
            //        dr["localDescription"] = "";
            //        dt.Rows[row]["bankAccount"] = "";
            //        dt.Rows[row]["localDescription"] = "";
            //    }
            //    if (!String.IsNullOrWhiteSpace(saveDescription))
            //    {
            //        dr["bankAccount"] = saveBank;
            //        dr["localDescription"] = saveDescription;
            //        dt.Rows[row]["bankAccount"] = saveBank;
            //        dt.Rows[row]["localDescription"] = saveDescription;
            //    }
            //}
            //catch (Exception ex)
            //{
            //}
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
            //            Printer.DrawQuad(6, 8, 4, 4, "Funeral Services Report", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(5, 8, 8, 4, this.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            font = new Font("Ariel", 10, FontStyle.Regular);
            string reportName = this.Text;
            string report = reportName + " for " + this.dateTimePicker1.Value.ToString("MM/dd/yyyy") + " through " + this.dateTimePicker2.Value.ToString("MM/dd/yyyy");
            Printer.DrawQuad(5, 8, 8, 4, report, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);


            //            Printer.DrawQuadTicks();
            DateTime date = DateTime.Now;
            string workDate = date.Month.ToString("D2") + "/" + date.Year.ToString("D4");
            Printer.SetQuadSize(24, 12);
            font = new Font("Ariel", 9, FontStyle.Regular);
            //Printer.DrawQuad(20, 8, 5, 4, "Report Month:" + workDate, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(16, 8, 3, 4, lblPayment.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(19, 8, 3, 4, lblTrust85.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(22, 8, 3, 4, lblTrust100.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);



            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /****************************************************************************************/
        private void gridMain_ValidatingEditor(object sender, DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventArgs e)
        {
            GridView view = sender as GridView;
            //if (view.FocusedColumn.FieldName.ToUpper() == "STATUS")
            //{
            //    DataTable dt = (DataTable)dgv.DataSource;
            //    DataRow dr = gridMain.GetFocusedDataRow();
            //    int rowhandle = gridMain.FocusedRowHandle;
            //    int row = gridMain.GetDataSourceRowIndex(rowhandle);
            //    oldWhat = e.Value.ObjToString();
            //    string status = dr["status"].ObjToString().ToUpper();
            //    if ( status == "CANCELLED")
            //    {
            //        string record = dr["record"].ObjToString();
            //        if (!String.IsNullOrWhiteSpace(record))
            //        {
            //            string cmd = "Select * from `cust_payment_details` WHERE `paymentRecord` = '" + record + "';";
            //            DataTable dx = G1.get_db_data(cmd);
            //            if (dx.Rows.Count > 0)
            //            {
            //                for (int i = 0; i < dx.Rows.Count; i++)
            //                {
            //                    record = dx.Rows[0]["record"].ObjToString();
            //                    G1.update_db_table("cust_payment_details", "record", record, new string[] { "status", "Cancelled" });

            //                    btnSavePayments_Click(null, null);
            //                    btnSavePayments.Hide();
            //                    btnSavePayments.Refresh();
            //                    justSaved = true;
            //                }
            //            }
            //        }
            //    }
            //}
            //else if (view.FocusedColumn.FieldName.ToUpper() == "DATEENTERED")
            //{
            //    DataTable dt = (DataTable)dgv.DataSource;
            //    DataRow dr = gridMain.GetFocusedDataRow();
            //    int rowhandle = gridMain.FocusedRowHandle;
            //    int row = gridMain.GetDataSourceRowIndex(rowhandle);
            //    oldWhat = e.Value.ObjToString();
            //    DateTime date = oldWhat.ObjToDateTime();
            //    dt.Rows[row]["dateEntered"] = G1.DTtoMySQLDT(date);
            //    e.Value = G1.DTtoMySQLDT(date);
            //}
            //else if (view.FocusedColumn.FieldName.ToUpper() == "TRUST_POLICY")
            //{
            //    DataTable dt = (DataTable)dgv.DataSource;
            //    DataRow dr = gridMain.GetFocusedDataRow();
            //    int rowhandle = gridMain.FocusedRowHandle;
            //    int row = gridMain.GetDataSourceRowIndex(rowhandle);
            //    oldWhat = e.Value.ObjToString();
            //}
            //else if (view.FocusedColumn.FieldName.ToUpper() == "PAYMENT")
            //{
            //    DataTable dt = (DataTable)dgv.DataSource;
            //    DataRow dr = gridMain.GetFocusedDataRow();
            //    int rowhandle = gridMain.FocusedRowHandle;
            //    int row = gridMain.GetDataSourceRowIndex(rowhandle);
            //    oldWhat = e.Value.ObjToString();

            //    string record = dr["record"].ObjToString();
            //    if (!String.IsNullOrWhiteSpace(record))
            //    {
            //        string cmd = "Select * from `cust_payment_details` WHERE `paymentRecord` = '" + record + "';";
            //        DataTable dx = G1.get_db_data(cmd);
            //        if ( dx.Rows.Count > 0 )
            //        {
            //            double payment = dr["payment"].ObjToDouble();
            //            payment = oldWhat.ObjToDouble();
            //            record = dx.Rows[0]["record"].ObjToString();
            //            G1.update_db_table("cust_payment_details", "record", record, new string[] {"paid", payment.ToString() });

            //            btnSavePayments_Click(null, null);
            //            btnSavePayments.Hide();
            //            btnSavePayments.Refresh();
            //            funModified = false;
            //            justSaved = true;
            //        }
            //    }
            //}
        }
        private string oldWhat = "";
        /****************************************************************************************/
        private void gridMain_CustomRowCellEdit(object sender, CustomRowCellEditEventArgs e)
        {
            GridView view = sender as GridView;
            //if (e.Column.FieldName.ToUpper() == "CHECKLIST")
            //{
            //    string type = view.GetRowCellValue(e.RowHandle, "type").ObjToString().ToUpper();
            //    if (type != "INSURANCE" && type != "POLICY" && type != "INSURANCE DIRECT" && type != "INSURANCE UNITY" && type != "3RD PARTY")
            //    {
            //        e.RepositoryItem = null;
            //        return;
            //    }
            //    string status = view.GetRowCellValue(e.RowHandle, "status").ObjToString();
            //    if (status.ToUpper() == "FILED")
            //        e.RepositoryItem = this.repositoryItemButtonEdit2;
            //    else if ( status.ToUpper() == "DEPOSITED")
            //        e.RepositoryItem = this.repositoryItemButtonEdit1;
            //    else
            //        e.RepositoryItem = this.repositoryItemButtonEdit2;
            //}
        }
        /****************************************************************************************/
        private string oldColumn = "";
        private DataTable trackingDt = null;
        private DataTable trackDt = null;
        RepositoryItemComboBox ciLookup = new RepositoryItemComboBox();
        /****************************************************************************************/
        private void gridMain_ShownEditor(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            int row = gridMain.FocusedRowHandle;

            GridColumn currCol = gridMain.FocusedColumn;
            DataRow dr = gridMain.GetFocusedDataRow();
            string name = currCol.FieldName;
            string record = "";
            string str = "";
            DateTime myDate = DateTime.Now;
            oldColumn = name;

            //bool doDate = false;
            //if (name == "apptDate")
            //    doDate = true;
            //else if (name == "lastContactDate")
            //    doDate = true;

            //if (doDate)
            //{
            //    myDate = dr[name].ObjToDateTime();
            //    str = gridMain.Columns[name].Caption;
            //    using (GetDate dateForm = new GetDate(myDate, str))
            //    {
            //        dateForm.ShowDialog();
            //        if (dateForm.DialogResult == System.Windows.Forms.DialogResult.OK)
            //        {
            //            try
            //            {
            //                myDate = dateForm.myDateAnswer;
            //                dr[name] = G1.DTtoMySQLDT(myDate);
            //            }
            //            catch (Exception ex)
            //            {
            //            }
            //            //dr[name] = G1.DTtoMySQLDT(myDate);
            //            UpdateMod(dr);
            //            gridMain_CellValueChanged(null, null);
            //        }
            //    }
            //}
            //gridMain.RefreshData();
            //gridMain.RefreshEditor(true);
        }
        /****************************************************************************************/
        private string currentColumn = "";
        private string oldContactType = "";
        private void gridMain_MouseDown(object sender, MouseEventArgs e)
        {
            var hitInfo = gridMain.CalcHitInfo(e.Location);
            if (hitInfo.InRowCell)
            {
                int rowHandle = hitInfo.RowHandle;
                gridMain.SelectRow(rowHandle);
                dgv.RefreshDataSource();
                DataTable dt = (DataTable)dgv.DataSource;

            //    GridColumn column = hitInfo.Column;
            //    currentColumn = column.FieldName.Trim();
            //    string data = dt.Rows[rowHandle][currentColumn].ObjToString();
            //    DataRow dr = gridMain.GetFocusedDataRow();

            //    if ( currentColumn == "contactName")
            //    {
            //        this.Validate();
            //        string contactType = dr["contactType"].ObjToString();
            //        if (String.IsNullOrWhiteSpace(contactType))
            //            return;
            //        if (contactType == oldContactType)
            //            return;
            //        oldContactType = contactType;

            //        string viewDetail = DetermineView(contactType);

            //        string answer = "";
            //        ciLookup.Items.Clear();
            //        if (myDt == null)
            //        {
            //            myDt = new DataTable();
            //            myDt.Columns.Add("stuff");
            //        }
            //        myDt.Rows.Clear();
            //        string cmd = "Select * from `track` where `contactType` = '" + contactType + "';";
            //        DataTable dx = G1.get_db_data(cmd);
            //        for ( int i=0; i<dx.Rows.Count; i++)
            //        {
            //            answer = dx.Rows[i]["answer"].ObjToString();
            //            if ( String.IsNullOrWhiteSpace ( answer))
            //            {
            //                if ( viewDetail.ToUpper() == "PERSON")
            //                {
            //                    answer = GetPerson(dx.Rows[i]);
            //                }
            //            }
            //            if ( !String.IsNullOrWhiteSpace ( answer ))
            //                AddToMyDt(answer);
            //        }

            //        ciLookup.Items.Clear();
            //        for (int i = 0; i < myDt.Rows.Count; i++)
            //            ciLookup.Items.Add(myDt.Rows[i]["stuff"].ObjToString());

            //        gridMain.Columns[currentColumn].ColumnEdit = ciLookup;
            //        gridMain.RefreshData();
            //        gridMain.RefreshEditor(true);
            //    }
            }
        }
        /****************************************************************************************/
        private void comboBox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            //System.Windows.Forms.ComboBox cbo = (System.Windows.Forms.ComboBox)sender;
            //cbo.PreviewKeyDown -= comboBox_PreviewKeyDown;
            //if (cbo.DroppedDown) cbo.Focus();
        }
        /****************************************************************************************/
        private void btnRun_Click(object sender, EventArgs e)
        {
            LoadData();
        }
        /****************************************************************************************/
        private void gridMain_CalcRowHeight(object sender, RowHeightEventArgs e)
        {
            //GridView View = sender as GridView;
            //if (e.RowHandle >= 0)
            //{
            //    int maxHeight = 0;
            //    int newHeight = 0;
            //    bool doit = false;
            //    string name = "";
            //    string str = "";
            //    int count = 0;
            //    string[] Lines = null;
            //    foreach (GridColumn column in gridMain.Columns)
            //    {
            //        name = column.FieldName.ToUpper();
            //        if (name == "RESULTS" )
            //            doit = true;
            //        if (doit)
            //        {
            //            using (RepositoryItemMemoEdit edit = new RepositoryItemMemoEdit())
            //            {
            //                using (MemoEditViewInfo viewInfo = edit.CreateViewInfo() as MemoEditViewInfo)
            //                {
            //                    str = gridMain.GetRowCellValue(e.RowHandle, column.FieldName).ObjToString();
            //                    if ( !String.IsNullOrWhiteSpace ( str ))
            //                    {
            //                        Lines = str.Split('\n');
            //                        count = Lines.Length + 1;
            //                    }
            //                    viewInfo.EditValue = gridMain.GetRowCellValue(e.RowHandle, column.FieldName);
            //                    viewInfo.Bounds = new Rectangle(0, 0, column.VisibleWidth, dgv.Height);
            //                    using (Graphics graphics = dgv.CreateGraphics())
            //                    using (GraphicsCache cache = new GraphicsCache(graphics))
            //                    {
            //                        viewInfo.CalcViewInfo(graphics);
            //                        var height = ((IHeightAdaptable)viewInfo).CalcHeight(cache, column.VisibleWidth);
            //                        newHeight = Math.Max(height, maxHeight);
            //                        if (newHeight > maxHeight)
            //                        {
            //                            maxHeight = newHeight * count;
            //                        }
            //                    }
            //                }
            //            }
            //        }
            //    }

            //    if (maxHeight > 0 && maxHeight > e.RowHeight )
            //        e.RowHeight = maxHeight;
            //}
        }
        /***********************************************************************************************/
        private void LoadData()
        {
            this.Cursor = Cursors.WaitCursor;

            DateTime beginDate = this.dateTimePicker1.Value;
            DateTime finalDate = this.dateTimePicker2.Value;

            DateTime date1 = this.dateTimePicker1.Value;
            string startDate = date1.ToString("yyyyMMdd");
            DateTime date2 = this.dateTimePicker2.Value;
            date2 = new DateTime(date2.Year, date2.Month, date2.Day);
            finalDate = date2;
            string endDate = date2.ToString("yyyyMMdd");

            DateTime originalEndDate = new DateTime(date2.Year, date2.Month, date2.Day);

            DataTable dx = null;
            string cmd = "";

            DateTime realStartDate = DateTime.Now;
            DateTime realStopDate = DateTime.Now;

            if ( chkPayPeriods.Checked )
            {
                date2 = date1.AddDays(14);
                endDate = date2.ToString("yyyyMMdd");
            }
            else
            {
                DetermineRealDateRange(date1, date2, ref realStartDate, ref realStopDate);
                if (realStartDate != date1)
                {
                    date1 = realStartDate;
                    startDate = date1.ToString("yyyyMMdd");
                }
                if ( realStopDate != date2 )
                {
                    TimeSpan ts = realStopDate - date1;
                    if (ts.TotalDays != 14)
                        date2 = date1.AddDays(14);
                    else
                        date2 = realStopDate;
                    endDate = date2.ToString("yyyyMMdd");
                }
            }

            string BPM = cmbBPM.Text;

            cmd = "Select DISTINCT * from `users` j RIGHT JOIN `tc_pay` p ON j.`username` = p.`username` LEFT JOIN `tc_er` r ON p.`username` = r.`username` WHERE `startDate` >= '" + startDate + "' AND `endDate` <= '" + endDate + "' ";
            if ( BPM == "BPM Only")
                cmd += " AND r.`isBPM` = 'Y' ";
            else if (BPM == "All W/O BPM")
                cmd += " AND ( r.`isBPM` <> 'Y' OR r.`splitBPM` = 'Y' ) ";
            //cmd += " AND j.`noTimeSheet` <> 'Y' ";
            cmd += ";";
            dx = G1.get_db_data(cmd);

            //dx = RemoveTerminated(dx, startDate.ObjToDateTime());

            DataTable dt = new DataTable();
            dt.Columns.Add("num");
            dt.Columns.Add("location");
            dt.Columns.Add("EmpStatus");
            dt.Columns.Add("name");
            dt.Columns.Add("gross", Type.GetType("System.Double"));

            DataView tempview = dx.DefaultView;
            tempview.Sort = "location asc, username asc, lastName asc, firstName asc, middleName asc";
            dx = tempview.ToTable();

            string firstName = "";
            string lastName = "";
            string middleName = "";
            string name = "";
            string splitBPM = "";
            double otherPay = 0D;

            dx.Columns.Add("name");

            string status = "";
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                firstName = dx.Rows[i]["firstName"].ObjToString();
                middleName = dx.Rows[i]["middleName"].ObjToString();
                lastName = dx.Rows[i]["lastName"].ObjToString();

                name = lastName + ", " + firstName;
                if (!String.IsNullOrWhiteSpace(middleName))
                    name = name + " " + middleName;
                dx.Rows[i]["name"] = name;

                status = dx.Rows[i]["EmpStatus"].ObjToString();
                if (status.Trim().ToUpper().IndexOf("FULL") == 0)
                    status = "FullTime";
                else if (status.Trim().ToUpper().IndexOf("PART") == 0)
                    status = "PartTime";
                dx.Rows[i]["EmpStatus"] = status;
            }

            DataTable locDt = (DataTable)cmbLocation.DataSource;
            string location = "";

            DataTable ddd = null;
            string oldEmp = "";
            string employee = "";
            string oldEmployee = "";
            string oldStatus = "";
            status = "";
            double gross = 0D;
            double totalPay = 0D;
            DateTime termDate = DateTime.Now;
            string excludePay = "";
            string userName = "";

            DataRow dR = null;

            for (int i = 0; i < locDt.Rows.Count; i++)
            {
                location = locDt.Rows[i]["LocationCode"].ObjToString();
                if (String.IsNullOrWhiteSpace(location))
                    continue;

                DataRow[] dRows = dx.Select("Location='" + location + "'");
                if (dRows.Length <= 0)
                    continue;

                ddd = dRows.CopyToDataTable();
                tempview = ddd.DefaultView;
                tempview.Sort = "EmpStatus asc";
                ddd = tempview.ToTable();

                oldEmp = "";
                gross = 0D;
                for (int j = 0; j < ddd.Rows.Count; j++)
                {
                    userName = ddd.Rows[j]["username"].ObjToString();
                    if ( userName.ToUpper() == "SGRAY")
                    {
                    }
                    employee = ddd.Rows[j]["name"].ObjToString();
                    status = ddd.Rows[j]["EmpStatus"].ObjToString();
                    if (String.IsNullOrWhiteSpace(oldEmp))
                    {
                        oldEmployee = employee;
                        oldEmp = userName;
                        oldStatus = status;
                    }

                    if (userName != oldEmp)
                    {
                        dR = dt.NewRow();
                        dR["name"] = oldEmployee;
                        dR["location"] = location;
                        dR["EmpStatus"] = oldStatus;
                        dR["gross"] = gross;
                        dt.Rows.Add(dR);
                        oldEmployee = employee;
                        oldEmp = userName;
                        oldStatus = status;
                        gross = 0D;
                    }
                    termDate = ddd.Rows[j]["termDate"].ObjToDateTime();
                    if ( termDate.Year > 1900 )
                    {
                        //if (termDate < startDate.ObjToDateTime())
                        //    continue;
                    }
                    if (ddd.Rows[j]["excludePayroll"].ObjToString().ToUpper() == "Y")
                        continue;

                    totalPay = ddd.Rows[j]["totalPay"].ObjToDouble();
                    if (BPM != "All")
                    {
                        splitBPM = ddd.Rows[j]["splitBPM"].ObjToString();
                        if (splitBPM == "Y")
                        {
                            otherPay = ddd.Rows[j]["otherPay"].ObjToDouble();
                            if (BPM == "BPM Only")
                                totalPay = totalPay - otherPay;
                            else
                                totalPay = otherPay;
                        }
                    }
                    totalPay = G1.RoundValue(totalPay);
                    gross += totalPay;
                }
                //if (gross > 0D)
                //{
                    dR = dt.NewRow();
                    dR["name"] = employee;
                    dR["location"] = location;
                    dR["EmpStatus"] = oldStatus;
                    dR["gross"] = gross;
                    dt.Rows.Add(dR);
                //}
            }

            int count = 1;

            if ( chkPayPeriods.Checked )
            {
                string title = date1.ToString("MM/dd/yyyy") + " to " + date2.ToString("MM/dd/yyyy");
                gridMain.Columns["gross"].Caption = title;
                int grossCol = G1.get_column_number(gridMain, "gross");
                for ( int i=gridMain.Columns.Count-1; i>grossCol;  i--)
                {
                    string junk = gridMain.Columns[i].FieldName.Trim();
                    if (!String.IsNullOrWhiteSpace(junk))
                        gridMain.Columns.RemoveAt(i);
                }

                int junk1 = 0;

                for (; ; )
                {
                    dt = LoadAnotherDataSet(dt, date1, date2, count);
                    date1 = date1.AddDays(14);
                    date2 = date1.AddDays(14);
                    count++;
                    if (date2 >= originalEndDate)
                    {
                        break;
                    }
                }
            }
            else
            {
                string title = this.dateTimePicker1.Value.ToString("MM/dd/yyyy") + " to " + this.dateTimePicker2.Value.ToString("MM/dd/yyyy");
                gridMain.Columns["gross"].Caption = title;
                int grossCol = G1.get_column_number(gridMain, "gross");
                for (int i = gridMain.Columns.Count - 1; i > grossCol; i--)
                {
                    string junk = gridMain.Columns[i].FieldName.Trim();
                    if (!String.IsNullOrWhiteSpace(junk))
                        gridMain.Columns.RemoveAt(i);
                }

                double daysInPeriod = 0D;
                double payDays = 0D;
                double payPerDay = 0D;
                TimeSpan ts;

                if ( date1 < beginDate )
                {
                    ts = date2 - date1;
                    daysInPeriod = ts.TotalDays;
                    ts = date2 - beginDate;
                    payDays = ts.TotalDays;
                    for ( int i=0; i<dt.Rows.Count; i++)
                    {
                        gross = dt.Rows[i]["gross"].ObjToDouble();
                        if (gross > 0D)
                        {
                            payPerDay = gross / (double)daysInPeriod;
                            totalPay = payPerDay * payDays;
                            dt.Rows[i]["gross"] = totalPay;
                        }
                    }
                }
                for (; ; )
                {
                    dt = AddAnotherDataSet(dt, date1, date2, finalDate );
                    date1 = date1.AddDays(14);
                    date2 = date1.AddDays(14);
                    count++;
                    if (date2 >= originalEndDate)
                    {
                        break;
                    }
                }
            }

            bool startNewWidth = false;
            int col = 0;
            int width = gridMain.Columns["gross"].VisibleWidth;

            for (int i = 0; i < dt.Columns.Count; i++)
            {
                name = dt.Columns[i].ColumnName.ObjToString().Trim();
                G1.SetColumnPosition(gridMain, name, i);
                if (startNewWidth)
                {
                    col = G1.get_column_number(gridMain, name);
                    if (col > 0)
                    {
                        gridMain.Columns[i].OptionsColumn.FixedWidth = true;
                        G1.SetColumnWidth(gridMain, name, width);
                    }
                }
                if (name.Trim().ToUpper() == "GROSS")
                    startNewWidth = true;
            }

            dgv.DataSource = dt;

            if (chkGroup.Checked)
                gridMain.ExpandAllGroups();

            gridMain.RefreshData();
            gridMain.RefreshEditor(true);
            dgv.Refresh();
            this.Refresh();

            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private DataTable RemoveTerminated ( DataTable dt, DateTime date )
        {
            DateTime termDate = DateTime.Now;
            for ( int i=dt.Rows.Count-1; i>=0; i--)
            {
                termDate = dt.Rows[i]["termDate"].ObjToDateTime();
                if (termDate.Year > 1900)
                {
                    if (termDate < date)
                        dt.Rows.RemoveAt(i);
                }
            }
            return dt;
        }
        /***********************************************************************************************/
        private void DetermineRealDateRange ( DateTime date1, DateTime date2, ref DateTime start, ref DateTime stop )
        {
            start = new DateTime(2022, 12, 23);
            stop = start;
            bool gotStart = false;
            bool gotStop = false;
            date2 = new DateTime(date2.Year, date2.Month, date2.Day);
            for (; ; )
            {
                if (!gotStart)
                {
                    if (start.AddDays (13)  >= date1)
                    {
                        //start = date1;
                        gotStart = true;
                    }
                    else
                        start = start.AddDays(14);
                }
                if (!gotStop)
                {
                    if ( stop.AddDays(13) >= date2)
                    {
                        //stop = stop.AddDays(14);
                        gotStop = true;
                        break;
                    }
                    else
                        stop = stop.AddDays(14);
                }
            }
        }
        /***********************************************************************************************/
        private DataTable AddAnotherDataSet(DataTable dt, DateTime date1, DateTime date2, DateTime finalDate )
        {
            this.Cursor = Cursors.WaitCursor;

            date1 = date1.AddDays(14);
            date2 = date1.AddDays(14);

            string startDate = date1.ToString("yyyyMMdd");
            string endDate = date2.ToString("yyyyMMdd");

            DataTable dx = null;
            string cmd = "";

            cmd = "Select DISTINCT * from `users` j RIGHT JOIN `tc_pay` p ON j.`username` = p.`username` LEFT JOIN `tc_er` r ON p.`username` = r.`username` WHERE `startDate` >= '" + startDate + "' AND `endDate` <= '" + endDate + "' ";
            //cmd = "Select * from `tc_pay` p LEFT JOIN `tc_er` r ON p.`username` = r.`username` LEFT JOIN `users` u ON p.`username` = u.`username` WHERE `startDate` = '" + startDate + "' AND `endDate` = '" + endDate + "';";
            dx = G1.get_db_data(cmd);

            DataView tempview = dx.DefaultView;
            tempview.Sort = "location asc,lastName asc, firstName asc, middleName asc";
            dx = tempview.ToTable();

            string firstName = "";
            string lastName = "";
            string middleName = "";
            string name = "";
            string title = date1.ToString("MM/dd/yyyy") + " to " + date2.ToString("MM/dd/yyyy");

            double daysInPeriod = 0D;
            double payDays = 0D;
            double payPerDay = 0D;
            TimeSpan ts;

            bool doRatio = false;

            if (date2 > finalDate)
            {
                ts = date2 - date1;
                daysInPeriod = ts.TotalDays;
                ts = date2 - finalDate;
                payDays = ts.TotalDays;
                doRatio = true;
            }


            try
            {
                dx.Columns.Add("name");

                string status = "";
                for (int i = 0; i < dx.Rows.Count; i++)
                {
                    firstName = dx.Rows[i]["firstName"].ObjToString();
                    middleName = dx.Rows[i]["middleName"].ObjToString();
                    lastName = dx.Rows[i]["lastName"].ObjToString();

                    name = lastName + ", " + firstName;
                    if (!String.IsNullOrWhiteSpace(middleName))
                        name = name + " " + middleName;
                    dx.Rows[i]["name"] = name;

                    status = dx.Rows[i]["EmpStatus"].ObjToString();
                    if (status.Trim().ToUpper().IndexOf("FULL") == 0)
                        status = "FullTime";
                    else if (status.Trim().ToUpper().IndexOf("PART") == 0)
                        status = "PartTime";
                    dx.Rows[i]["EmpStatus"] = status;
                }

                DataTable locDt = (DataTable)cmbLocation.DataSource;
                string location = "";

                DataTable ddd = null;
                string oldEmp = "";
                string employee = "";
                string oldStatus = "";
                status = "";
                double gross = 0D;
                double totalPay = 0D;

                DataRow dR = null;
                DataRow[] dRows = null;

                for (int i = 0; i < dx.Rows.Count; i++)
                {
                    employee = dx.Rows[i]["name"].ObjToString();
                    gross = dx.Rows[i]["totalPay"].ObjToDouble();
                    if ( doRatio && gross > 0D )
                    {
                        payPerDay = gross / (double)daysInPeriod;
                        totalPay = payPerDay * payDays;
                        gross = gross - totalPay;
                    }
                    dRows = dt.Select("name='" + employee + "'");
                    if (dRows.Length > 0)
                    {
                        totalPay = dRows[0]["gross"].ObjToDouble();
                        totalPay += gross;
                        dRows[0]["gross"] = totalPay;
                    }
                }
            }
            catch (Exception ex)
            {
            }

            return dt;
        }
        /***********************************************************************************************/
        private DataTable LoadAnotherDataSet( DataTable dt, DateTime date1, DateTime date2, int count )
        {
            this.Cursor = Cursors.WaitCursor;

            date1 = date1.AddDays(14);
            date2 = date1.AddDays(14);

            string startDate = date1.ToString("yyyyMMdd");
            string endDate = date2.ToString("yyyyMMdd");

            DataTable dx = null;
            string cmd = "";

            cmd = "Select DISTINCT * from `users` j RIGHT JOIN `tc_pay` p ON j.`username` = p.`username` LEFT JOIN `tc_er` r ON p.`username` = r.`username` WHERE `startDate` >= '" + startDate + "' AND `endDate` <= '" + endDate + "' ";
            //cmd = "Select * from `tc_pay` p LEFT JOIN `tc_er` r ON p.`username` = r.`username` LEFT JOIN `users` u ON p.`username` = u.`username` WHERE `startDate` = '" + startDate + "' AND `endDate` = '" + endDate + "';";
            dx = G1.get_db_data(cmd);

            DataView tempview = dx.DefaultView;
            tempview.Sort = "location asc,lastName asc, firstName asc, middleName asc";
            dx = tempview.ToTable();

            string firstName = "";
            string lastName = "";
            string middleName = "";
            string name = "";
            string title = date1.ToString("MM/dd/yyyy") + " to " + date2.ToString("MM/dd/yyyy");

            try
            {
                int width = gridMain.Columns["gross"].VisibleWidth;
                G1.AddNewColumn(gridMain, "gross" + count.ToString(), title, "N2", FormatType.Numeric, width, true);
                G1.SetColumnWidth(gridMain, "gross" + count.ToString(), width);
                dt.Columns.Add("gross" + count.ToString(), Type.GetType("System.Double"));


                BandedGridColumn column = gridMain.Columns["gross" + count.ToString() ];
                this.gridMain.GroupSummary.AddRange(new DevExpress.XtraGrid.GridSummaryItem[] {new DevExpress.XtraGrid.GridGroupSummaryItem(DevExpress.Data.SummaryItemType.Sum, "gross" + count.ToString(), column, "{0:0,0.00}"),});
                AddSummaryColumn("gross" + count.ToString(), gridMain);

                for (int i = 0; i < dt.Rows.Count; i++)
                    dt.Rows[i]["gross" + count.ToString()] = 0D;



                dx.Columns.Add("name");

                string status = "";
                for (int i = 0; i < dx.Rows.Count; i++)
                {
                    firstName = dx.Rows[i]["firstName"].ObjToString();
                    middleName = dx.Rows[i]["middleName"].ObjToString();
                    lastName = dx.Rows[i]["lastName"].ObjToString();

                    name = lastName + ", " + firstName;
                    if (!String.IsNullOrWhiteSpace(middleName))
                        name = name + " " + middleName;
                    dx.Rows[i]["name"] = name;

                    status = dx.Rows[i]["EmpStatus"].ObjToString();
                    if (status.Trim().ToUpper().IndexOf("FULL") == 0)
                        status = "FullTime";
                    else if (status.Trim().ToUpper().IndexOf("PART") == 0)
                        status = "PartTime";
                    dx.Rows[i]["EmpStatus"] = status;
                }

                DataTable locDt = (DataTable)cmbLocation.DataSource;
                string location = "";

                DataTable ddd = null;
                string oldEmp = "";
                string employee = "";
                string oldStatus = "";
                status = "";
                double gross = 0D;
                double totalPay = 0D;

                DataRow dR = null;
                DataRow[] dRows = null;

                for (int i = 0; i < dx.Rows.Count; i++)
                {
                    employee = dx.Rows[i]["name"].ObjToString();
                    gross = dx.Rows[i]["totalPay"].ObjToDouble();
                    dRows = dt.Select("name='" + employee + "'");
                    if (dRows.Length > 0)
                    {
                        totalPay = dRows[0]["gross" + count.ToString()].ObjToDouble();
                        totalPay += gross;
                        dRows[0]["gross" + count.ToString()] = totalPay;
                    }
                }
            }
            catch ( Exception ex)
            {
            }

            return dt;
        }
        /****************************************************************************************/
        private void chkGroup_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox box = (CheckBox)sender;
            if ( box.Checked )
            {
                gridMain.Columns["location"].GroupIndex = 0;
                gridMain.Columns["EmpStatus"].GroupIndex = 1;
                gridMain.ExpandAllGroups();
            }
            else
            {
                gridMain.Columns["location"].GroupIndex = -1;
                gridMain.Columns["EmpStatus"].GroupIndex = -1;
                gridMain.CollapseAllDetails();
            }
        }
        /****************************************************************************************/
        private void chkComboLocation_EditValueChanged(object sender, EventArgs e)
        {
            DataTable dx = (DataTable)chkComboLocation.Properties.DataSource;

            string what = chkComboLocation.Text;
            if (String.IsNullOrWhiteSpace(what))
                return;

            string payPeriod = "";
            DateTime firstDate = DateTime.MaxValue;
            DateTime secondDate = DateTime.MinValue;

            DateTime date1 = DateTime.Now;
            DateTime date2 = DateTime.Now;

            string[] Lines = what.Split('|');
            string[] lines = null;

            for ( int i=0; i<Lines.Length; i++)
            {
                payPeriod = Lines[i].Trim();
                if (String.IsNullOrWhiteSpace(payPeriod))
                    continue;
                lines = payPeriod.Split('-');
                if (lines.Length <= 0)
                    continue;
                date1 = lines[0].Trim().ObjToDateTime();
                date2 = lines[1].Trim().ObjToDateTime();
                if (date1 < firstDate)
                    firstDate = date1;
                if (date2 > secondDate)
                    secondDate = date2;
            }
            this.dateTimePicker2.Value = secondDate;
            this.dateTimePicker1.Value = firstDate;
            this.dateTimePicker1.Refresh();
            this.dateTimePicker2.Refresh();
        }
        /****************************************************************************************/
        private void cmbBPM_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadData();
        }
        /****************************************************************************************/
    }
}