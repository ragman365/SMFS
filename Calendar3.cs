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
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraEditors.ViewInfo;
using DevExpress.Utils.Drawing;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class Calendar3 : DevExpress.XtraEditors.XtraForm
    {
        private bool modified = false;
        private bool loading = true;
        private string workUsername = "";
        private string workSupervisor = "";
        private DateTime workDate = DateTime.Now;
        private DataTable vacationDt = null;
        private DataTable colorDt = null;
        private DataTable workDt = null;
        private bool workDepartment = false;
        private string workGroupName = "";
        public DataTable CalendarDt = null;
        private string workSearch = "";

        /****************************************************************************************/
        public Calendar3 ( string username, string supervisor, DateTime new_date, DataTable dt )
        {
            InitializeComponent();
            workUsername = username;
            workSupervisor = supervisor;
            workDate = new_date;
            workDt = dt;
        }
        /****************************************************************************************/
        public Calendar3 ( DataTable dt, string username, string supervisor, DateTime new_date, string groupName )
        {
            InitializeComponent();
            workUsername = username;
            workSupervisor = supervisor;
            workDate = new_date;
            workDt = dt;
            workDepartment = true;
            workGroupName = groupName;
        }
        /****************************************************************************************/
        public Calendar3( DateTime new_date, string searchBy )
        {
            InitializeComponent();
            workDate = new_date;
            workSearch = searchBy;
            workDt = GoogleCalendarManager.GetCalendarEvents();
        }
        /****************************************************************************************/
        private void Calendar3_Load(object sender, EventArgs e)
        {
            this.dateTimePicker1.Value = workDate;
            if ( !G1.RobbyServer )
                btnRefresh.Hide();

            DateTime date = this.dateTimePicker1.Value;
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            date = new DateTime(date.Year, date.Month, days);
            this.dateTimePicker1.Value = date;

            vacationDt = new DataTable();
            vacationDt.Columns.Add("what");

            LoadData();

            //LoadVacationTable();

            int left = this.Left - 50;
            int top = this.Top;
            int width = this.Width;
            int height = this.Height;

            width = width + 100;
            this.SetBounds(left, top, width, height);

            loading = false;
        }
        /****************************************************************************************/
        private void LoadVacationTable ()
        {
            if (workDt == null)
                return;
            DateTime startDate = DateTime.Now;
            DateTime stopDate = DateTime.Now;
            DateTime newDate = DateTime.Now;

            double hours = 0D;

            DataTable dt = (DataTable)dgv.DataSource;
            string name = "";
            string[] Lines = null;

            DataRow dR = null;
            string approved = "";

            DataRow[] dRows = null;
            string what = "";
            bool added = false;

            for (int i = 0; i < workDt.Rows.Count; i++)
            {
                startDate = workDt.Rows[i]["start"].ObjToDateTime();
                stopDate = workDt.Rows[i]["stop"].ObjToDateTime();
                //approved = workDt.Rows[i]["approved"].ObjToString().ToUpper();
                name = workDt.Rows[i]["who"].ObjToString();
                name = workDt.Rows[i]["Details"].ObjToString();

                what = startDate.ToString("yyyy-MM-dd") + "~" + name;

                added = AddVacationRow(what);
            }

            //PreProcessNew(dt);

            SetColors();
        }
        /****************************************************************************************/
        private bool AddVacationRow ( string what )
        {
            bool added = false;
            DataRow [] dRows = vacationDt.Select("what='" + what + "'");
            if (dRows.Length <= 0)
            {
                DataRow dR = vacationDt.NewRow();
                dR["what"] = what;
                vacationDt.Rows.Add(dR);
                added = true;
            }
            return added;
        }
        /****************************************************************************************/
        private void LoadPreviousVacation ( DataTable dt, string what, DateTime startDate, DateTime stopDate, string approved )
        {
            if (String.IsNullOrWhiteSpace(what))
                return;
            bool gotStop = false;
            bool gotHours = false;
            int idx = 0;

            string hours1 = "";
            string hours2 = "";

            string[] Lines = what.Split('~');
            string[] lines = null;
            if (what.ToUpper().IndexOf("STOP VACATION") >= 0)
                gotStop = true;
            else if (what.ToUpper().IndexOf("HOURS") >= 0)
            {
                gotHours = true;
                idx = Lines[1].IndexOf("Approved");
                if ( idx > 0 )
                   Lines[1] = Lines[1].Substring(idx + 9);
                else
                {
                    idx = Lines[1].IndexOf("Requested");
                    Lines[1] = Lines[1].Substring(idx + 10);
                }
                hours1 = Lines[0];
                hours2 = Lines[1];
            }

            DateTime newDate = Lines[0].ObjToDateTime();
            DateTime testDate = DateTime.Now;

            int iday = 0;

            testDate = this.dateTimePicker1.Value;
            int months = G1.GetMonthsBetween ( testDate, stopDate );
            if ( months == -1 )
            {
                iday = DateTime.DaysInMonth(startDate.Year, startDate.Month);
                stopDate = new DateTime(startDate.Year, startDate.Month, iday);
                if ( gotStop )
                    newDate = stopDate;
            }
            else
            {
                months = G1.GetMonthsBetween(testDate, startDate);
                if ( months == 1 )
                {
                    startDate = new DateTime(stopDate.Year, stopDate.Month, 1);
                    if (!gotStop)
                        newDate = startDate;
                }
            }

            int firstColumn = G1.get_column_number(dt, "sunday");
            int lastColumn = G1.get_column_number(dt, "saturday");

            string str = "";

            for ( int i=0; i<dt.Rows.Count; i++)
            {
                for ( int k=firstColumn; k<=lastColumn; k++)
                {
                    str = dt.Rows[i][k].ObjToString();
                    if (String.IsNullOrWhiteSpace(str))
                        continue;
                    lines = str.Split('\n');
                    if (lines.Length <= 0)
                        continue;
                    if (!G1.validate_numeric(lines[0].Trim()))
                        continue;
                    iday = lines[0].ObjToInt32();
                    testDate = new DateTime(workDate.Year, workDate.Month, iday);
                    if ( testDate == newDate )
                    {
                        //if (Lines[1].Trim().IndexOf("Start Vacation Requested") == 0)
                        //{
                        //    Lines[1] = Lines[1].Replace("Start Vacation Requested", "");
                        //    Lines[1] += " Requested";
                        //}
                        str += "\n" + Lines[1].ObjToString();
                        dt.Rows[i][k] = str;
                        if (gotStop)
                            ProcessVacationStop(dt, what, startDate, stopDate, approved );
                        else if ( gotHours )
                        {
                            if (approved.ToUpper() == "Y")
                                str += " Approved";
                            else
                                str += " Requested";
                            dt.Rows[i][k] = str;
                        }
                        break;
                    }
                }
            }
        }
        /****************************************************************************************/
        private void ProcessVacationStop ( DataTable dt, string what, DateTime startDate, DateTime stopDate, string approved )
        {
            string newWhat = "";
            string cName = "";
            bool finished = false;

            string[] Lines = what.Split('~');
            if (Lines.Length < 2)
                return;

            string vacationType = "Requested";
            if ( approved.ToUpper() == "Y" ) 
                vacationType = "Approved";

            string str = Lines[1].Trim();
            str = str.Replace("Stop Vacation Requested", "");
            str = str.Replace("Stop Vacation Approved", "");
            string name = str.Trim();

            Lines = what.Split('~');
            if (Lines.Length < 2)
                return;
            string replacement = Lines[1].Trim();
            string stopReplacement = replacement;
            replacement = replacement.Replace("Stop", "Start");



            string[] lines = null;
            int day = 0;

            int firstColumn = G1.get_column_number(dt, "sunday");
            int lastColumn = G1.get_column_number(dt, "saturday");

            DateTime testDate = DateTime.Now;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                for (int k = firstColumn; k <= lastColumn; k++)
                {
                    str = dt.Rows[i][k].ObjToString();
                    if (String.IsNullOrWhiteSpace(str))
                        continue;
                    lines = str.Split('\n');
                    if (lines.Length <= 0)
                        continue;
                    if (!G1.validate_numeric(lines[0].Trim()))
                        continue;
                    day = lines[0].ObjToInt32();
                    testDate = new DateTime(workDate.Year, workDate.Month, day);
                    if (testDate > startDate && testDate < stopDate )
                    {
                        if (name != "Stop Vacation" && !String.IsNullOrWhiteSpace ( name ))
                            str += "\n" + name;
                        if (G1.validate_numeric(str))
                            str += "\n";
                        dt.Rows[i][k] = str + " " + vacationType;
                    }
                    else if ( testDate == startDate )
                    {
                        str = str.Replace(replacement, name + " " + vacationType);
                        if (G1.validate_numeric(str))
                            str += "\n" + vacationType;
                        dt.Rows[i][k] = str;
                    }
                    else if (testDate == stopDate)
                    {
                        str = str.Replace(stopReplacement, name + " " + vacationType);
                        if (G1.validate_numeric(str))
                            str += "\n" + vacationType;
                        dt.Rows[i][k] = str;
                    }
                }
            }
        }
        /****************************************************************************************/
        private void gridMain_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt == null)
                return;
            string delete = dt.Rows[row]["mod"].ObjToString();
        }
        /****************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        { // Spy Glass
            G1.SpyGlass(gridMain);
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
        }
        /****************************************************************************************/
        private void EditTable_FormClosing(object sender, FormClosingEventArgs e)
        {
            //if (!btnSaveAll.Visible)
            //    return;
            //DialogResult result = MessageBox.Show("***Question*** Data has been modified.\nDo you really want to exit WITHOUT saving your data?", "Data Modified Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            //if (result == DialogResult.Yes)
            //    return;
            //e.Cancel = true;
        }
        /****************************************************************************************/
        private void gridMain_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            if (e.ListSourceRowIndex == DevExpress.XtraGrid.GridControl.InvalidRowHandle)
                return;
            string name = e.Column.FieldName;
            int row = e.ListSourceRowIndex;
            DataTable dt = (DataTable)dgv.DataSource;
            string str = dt.Rows[row][name].ObjToString();
        }
        /****************************************************************************************/
        private void LoadData()
        {
            loading = true;

            this.Cursor = Cursors.WaitCursor;

            DataTable dt = new DataTable();

            dt.Columns.Add("mod");
            dt.Columns.Add("num");

            dt.Columns.Add("sunday");
            dt.Columns.Add("monday");
            dt.Columns.Add("tuesday");
            dt.Columns.Add("wednesday");
            dt.Columns.Add("thursday");
            dt.Columns.Add("friday");
            dt.Columns.Add("saturday");

            DataRow dR = null;

            for ( int i=0; i<7; i++)
            {
                dR = dt.NewRow();
                dt.Rows.Add(dR);
            }

            dgv.DataSource = dt;

            FillCalendar(workDate);

            RemoveSlots();

            dt = (DataTable)dgv.DataSource;
            colorDt = dt.Copy();

            LoadVacationTable();

            gridBand4.Caption = workDate.ToString("MMMM, yyyy");

            this.Cursor = Cursors.Default;

            loading = false;
        }
        /***************************************************************************************/
        private void RemoveSlots()
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataRowCollection rc = dt.Rows;
            if (dt.Columns.Count <= 0)
                return;
            int maxrow = rc.Count;
            bool found = false;
            string str = "";

            for ( int i=dt.Rows.Count - 1; i>=0; i--)
            {
                found = false;
                for ( int j=0; j<7; j++)
                {
                    str = dt.Rows[i][j].ObjToString();
                    if ( !String.IsNullOrWhiteSpace ( str ))
                    {
                        found = true;
                        break;
                    }
                }
                if (found)
                    break;
                dt.Rows.RemoveAt(i);

            }

            dgv.DataSource = dt;
            dgv.Refresh();
        }
        /****************************************************************************************/
        private void FillCalendar(DateTime new_date)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataRowCollection rc = dt.Rows;
            int count = rc.Count;
            if (count <= 0)
                return;
            int mm = new_date.Month;
            int dd = new_date.Day;
            int yy = new_date.Year;
            string sdate = mm.ToString("D2") + "/" + dd.ToString("D2") + "/" + yy.ToString("D4");
            long cdate = G1.date_to_days(sdate);
            //int days             = G1.days_in_month ( yy, mm );
            int days = DateTime.DaysInMonth(yy, mm);
            DateTime ldate = new_date;
            ldate = ldate.AddDays((double)(-(dd) + 1));
            try
            {
                for (int j = 0; j < 6; j++)
                {
                    for (int i = 0; i < 7; i++)
                        rc[j][i + 1] = "";
                }
                dt.AcceptChanges();

                DateTime firstDate = new DateTime(new_date.Year, new_date.Month, 1);
                DateTime lastDate = new DateTime(new_date.Year, new_date.Month, days);

                //string cmd = "Select * from `tc_timerequest` where `supervisor` = '" + workSupervisor + "' AND (( `fromdate` >= '" + firstDate.ToString("yyyyMMdd") + "' AND `fromdate` <= '" + lastDate.ToString("yyyyMMdd") + "' ) ";

                string cmd = "Select * from `tc_timerequest` where `supervisor` = '" + workUsername + "' AND (( `fromdate` >= '" + firstDate.ToString("yyyyMMdd") + "' AND `fromdate` <= '" + lastDate.ToString("yyyyMMdd") + "' ) ";
                cmd += " OR ( `todate` >= '" + lastDate.ToString("yyyyMMdd") + "' AND `todate` <= '" + lastDate.ToString("yyyyMMdd") + "') ) ";
                //if (cmbMyProc.Text.ToUpper() == "APPROVED")
                //	cmd += " and `approved` = 'Y' ";
                //else if (cmbMyProc.Text.ToUpper() == "UNAPPROVED")
                //	cmd += " and `approved` <> 'Y' ";
                cmd += " order by `fromdate` DESC; ";
                DataTable dx = G1.get_db_data(cmd);

                dx = workDt;

                if ( G1.get_column_number ( dx, "sFrom") < 0 )
                    dx.Columns.Add("sFrom");
                if (G1.get_column_number(dx, "sTo") < 0)
                    dx.Columns.Add("sTo");
                DateTime date = DateTime.Now;

                string sfDate = "";
                string slDate = "";
                for (int i = 0; i < dx.Rows.Count; i++)
                {
                    date = dx.Rows[i]["start"].ObjToDateTime();
                    sfDate = date.ToString("yyyyMMdd");
                    date = dx.Rows[i]["stop"].ObjToDateTime();
                    slDate = date.ToString("yyyyMMdd");
                    dx.Rows[i]["sFrom"] = sfDate;
                    dx.Rows[i]["sTo"] = slDate;
                }

                DataRow[] dRows = null;

                DataTable timeOffDt = null;

                int row = 0;
                slDate = "";
                string details = "";
                string[] Lines = null;
                for (int i = 1; i <= days; i++)
                {
                    int dow = (int)ldate.DayOfWeek;
                    rc[row][dow+2] = i.ToString();

                    slDate = ldate.ToString("yyyyMMdd");

                    dRows = dx.Select("'" + slDate + "' >= sFrom AND '" + slDate + "'<= sTo");

                    //                dRows = dx.Select("sFrom >='" + slDate + "'");
                    if (dRows.Length > 0)
                    {
                        timeOffDt = dRows.CopyToDataTable();
                        dRows = timeOffDt.Select("sTo >='" + slDate + "'");
                        if (dRows.Length > 0)
                        {
                            slDate = rc[row][dow + 2].ObjToString();
                            for (int j = 0; j < dRows.Length; j++)
                            {
                                timeOffDt = dRows.CopyToDataTable();
                                details = dRows[j]["details"].ObjToString();
                                details = details.Replace("Creation Date", "(CD)");
                                details = details.Replace("Last Touch Date", "(LTD)");
                                details = details.Replace("Next Touch Date", "(NTD)");

                                date = dRows[j]["start"].ObjToDateTime();
                                Lines = date.ObjToString().Split(' ');
                                if (Lines.Length > 1)
                                {
                                    if (workSearch == "Last Touch Date")
                                    {
                                        DateTime last = date.ObjToDateTime();
                                        details += " " + last.ToString("hh:mm");
                                        if (Lines.Length > 2)
                                            details += " " + Lines[2].Trim();
                                        //details += " " + Lines[1].ObjToString();
                                    }
                                }
                                slDate += "\n" + details;
                            }
                            rc[row][dow + 2] = slDate;
                        }
                    }

                    if (dow == 6)
                        row = row + 1;
                    ldate = ldate.AddDays((double)1.0);
                }
            }
            catch ( Exception ex)
            {
            }

            dgv.RefreshDataSource();
            dgv.Refresh();
        }
        /****************************************************************************************/
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        /***********************************************************************************************/
        private int pageMarginLeft = 0;
        private int pageMarginRight = 0;
        private int pageMarginTop = 0;
        private int pageMarginBottom = 0;
        private int printCount = 0;
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
            printableComponentLink1.CreateDetailHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateDetailHeaderArea);
            printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);
            printableComponentLink1.BeforeCreateAreas += new System.EventHandler(this.printableComponentLink1_BeforeCreateAreas);
            printableComponentLink1.AfterCreateAreas += new System.EventHandler(this.printableComponentLink1_AfterCreateAreas);
            printableComponentLink1.Landscape = true;

            //Printer.setupPrinterMargins(50, 100, 110, 50);

            Printer.setupPrinterMargins(10, 10, 90, 50);


            pageMarginLeft = Printer.pageMarginLeft;
            pageMarginRight = Printer.pageMarginRight;
            pageMarginTop = Printer.pageMarginTop;
            pageMarginBottom = Printer.pageMarginBottom;


            printableComponentLink1.Margins.Left = pageMarginLeft;
            printableComponentLink1.Margins.Right = pageMarginRight;
            printableComponentLink1.Margins.Top = pageMarginTop;
            printableComponentLink1.Margins.Bottom = pageMarginBottom;

            printingSystem1.Document.AutoFitToPagesWidth = 1;

            G1.AdjustColumnWidths(gridMain, 0.65D, true);

            printableComponentLink1.CreateDocument();
            printableComponentLink1.ShowPreview();

            G1.AdjustColumnWidths(gridMain, 0.65D, false);
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

            //Printer.setupPrinterMargins(50, 100, 110, 50);
            Printer.setupPrinterMargins(10, 10, 90, 50);

            pageMarginLeft = Printer.pageMarginLeft;
            pageMarginRight = Printer.pageMarginRight;
            pageMarginTop = Printer.pageMarginTop;
            pageMarginBottom = Printer.pageMarginBottom;

            printableComponentLink1.Margins.Left = pageMarginLeft;
            printableComponentLink1.Margins.Right = pageMarginRight;
            printableComponentLink1.Margins.Top = pageMarginTop;
            printableComponentLink1.Margins.Bottom = pageMarginBottom;

            printCount = 0;

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

            font = new Font("Ariel", 12, FontStyle.Regular);
            string title = this.Text;
            title = workGroupName + " Calendar";
            int startX = 6;
            Printer.DrawQuad(startX, 8, 9, 4, title, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);


            //            Printer.DrawQuadTicks();
            Printer.SetQuadSize(24, 12);
            font = new Font("Ariel", 7, FontStyle.Regular);
            //Printer.DrawQuad(16, 7, 5, 2, lblBalance.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Top);
            //Printer.DrawQuad(16, 10, 5, 2, lblPayment.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            font = new Font("Ariel", 8);
            //            Printer.DrawQuad(1, 6, 6, 3, search, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Top);
            Printer.DrawQuad(1, 9, 6, 3, "User : " + LoginForm.username, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);

            //DateTime date = this.dateTimePicker1.Value;
            //string workDate = date.ToString("MM/dd/yyyy") + " - ";
            //date = this.dateTimePicker2.Value;
            //workDate += date.ToString("MM/dd/yyyy");

            //Printer.SetQuadSize(24, 12);
            //font = new Font("Ariel", 9, FontStyle.Regular);
            //Printer.DrawQuad(18, 7, 10, 4, "Log Dates :" + workDate, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);


            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /****************************************************************************************/
        private void UpdateMod ( DataRow dr )
        {
            dr["mod"] = "Y";
            modified = true;
        }
        /****************************************************************************************/
        private void gridMain_ValidatingEditor(object sender, DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventArgs e)
        {
            GridView gridView = sender as GridView;
            string[] Lines = null;
            DateTime date = DateTime.Now;
            string name = gridView.FocusedColumn.FieldName;
            oldColumn = name;

            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridView.GetFocusedDataRow();
            int rowhandle = gridView.FocusedRowHandle;
            int row = gridView.GetDataSourceRowIndex(rowhandle);
            string data = dr[name].ObjToString();
        }
        /****************************************************************************************/
        private string oldWhat = "";
        private string oldColumn = "";
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
        }
        /****************************************************************************************/
        private void gridMain_CellValueChanging(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            GridColumn currCol = gridMain.FocusedColumn;
            DataRow dr = gridMain.GetFocusedDataRow();
            string name = currCol.FieldName;
            string record = "";
            string str = "";
            DateTime date = DateTime.Now;
            oldColumn = name;
        }
        /****************************************************************************************/
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (loading)
                return;

            GridColumn currCol = gridMain.FocusedColumn;
            DataRow dr = gridMain.GetFocusedDataRow();
            string name = currCol.FieldName;
            string record = "";
            string str = "";
            string dateStr = "";
            string strDate = "";
            string[] Lines = null;
            DateTime date = DateTime.Now;
            str = dr[name].ObjToString();
            bool doDate = false;

            modified = true;
            dr["mod"] = "Y";
        }
        /****************************************************************************************/
        private void gridMain_CalcRowHeight(object sender, RowHeightEventArgs e)
        {
            e.RowHeight = 100;
            GridView View = sender as GridView;
            if (e.RowHandle >= 0)
            {
                int maxHeight = 0;
                int newHeight = 0;
                bool doit = false;
                string name = "";
                foreach (GridColumn column in gridMain.Columns)
                {
                    name = column.FieldName.ToUpper();
                    //if (name == "CASH" || name == "DEPOSITNUMBER" || name == "CREDIT CARD" || name == "CCDEPOSITNUMBER")
                        doit = true;
                    if (doit)
                    {
                        using (RepositoryItemMemoEdit edit = new RepositoryItemMemoEdit())
                        {
                            using (MemoEditViewInfo viewInfo = edit.CreateViewInfo() as MemoEditViewInfo)
                            {
                                viewInfo.EditValue = gridMain.GetRowCellValue(e.RowHandle, column.FieldName);
                                viewInfo.Bounds = new Rectangle(0, 0, column.VisibleWidth, dgv.Height);
                                using (Graphics graphics = dgv.CreateGraphics())
                                using (GraphicsCache cache = new GraphicsCache(graphics))
                                {
                                    viewInfo.CalcViewInfo(graphics);
                                    var height = ((IHeightAdaptable)viewInfo).CalcHeight(cache, column.VisibleWidth);
                                    newHeight = Math.Max(height, maxHeight);
                                    if (newHeight > maxHeight)
                                        maxHeight = newHeight;
                                }
                            }
                        }
                    }
                }

                if (maxHeight > 0)
                {
                    if (maxHeight < 100)
                        maxHeight = 100;
                    else
                        e.RowHeight = maxHeight;
                }
                else
                    e.RowHeight = 100;
            }
        }
        /****************************************************************************************/
        private void gridMain_RowCellStyle(object sender, RowCellStyleEventArgs e)
        {
            if (loading)
                return;
            GridView View = sender as GridView;
            if (e.RowHandle >= 0)
            {
                string column = e.Column.FieldName.ToUpper();
                DataTable dt = (DataTable)dgv.DataSource;
                int row = gridMain.GetDataSourceRowIndex(e.RowHandle);

                string str = dt.Rows[row][column].ObjToString();
                str = colorDt.Rows[row][column].ObjToString();
                if (str.Trim().ToUpper() == "COLOR")
                    e.Appearance.BackColor = Color.Yellow;
            }
        }
        /****************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            int saveRowHandle = gridMain.FocusedRowHandle;
            DataRow dr = gridMain.GetFocusedDataRow();
            string name = gridMain.FocusedColumn.FieldName.Trim();

            string data = dr[name].ObjToString();


            string[] Lines = data.Split('\n');
            if (Lines.Length < 2)
                return;

            int day = Lines[0].Trim().ObjToInt32();

            DateTime actualDate = this.dateTimePicker1.Value;
            DateTime newDate = new DateTime(actualDate.Year, actualDate.Month, day);

            string searchDate = newDate.ToString("yyyyMMdd");
            DataRow[] dRows = workDt.Select("sFrom='" + searchDate + "'");
            if (dRows.Length <= 0)
                return;
            DataTable myDt = dRows.CopyToDataTable();

            DataTable dayDt = new DataTable();
            dayDt.Columns.Add("hours");
            dayDt.Columns.Add("details");

            DataRow dRow = null;

            for ( int i=0; i<24; i++ )
            {
                dRow = dayDt.NewRow();
                dRow["hours"] = i.ToString() + ":00";
                if ( i == 12 )
                {
                    dRow["hours"] = "12:00 PM";
                }
                else if ( i > 12 )
                {
                    dRow["hours"] = (i-12).ToString() + ":00 PM";
                }
                dRow["details"] = "";
                dayDt.Rows.Add(dRow);
            }

            DateTime date = DateTime.Now;
            string details = "";
            string results = "";
            string location = "";

            for ( int i=0; i<myDt.Rows.Count; i++)
            {
                date = myDt.Rows[i]["start"].ObjToDateTime();
                location = myDt.Rows[i]["location"].ObjToString();
                details = myDt.Rows[i]["details"].ObjToString();
                results = myDt.Rows[i]["result"].ObjToString();

                details = details.Replace("Creation Date", "(CD)");
                details = details.Replace("Last Touch Date", "(LTD)");
                details = details.Replace("Next Touch Date", "(NTD)");


                if ( date.Hour == 0 )
                {
                    dayDt.Rows[i]["details"] = details + " " + results;
                }
                else
                {
                    dayDt.Rows[date.Hour]["details"] = details + " " + results;
                }
            }

            dgv2.DataSource = dayDt;

            tabControl1.SelectedTab = tabDay;
            dgv2.Refresh();

            string selection = "";
        }
        /***************************************************************************************/
        private void AddVacationRow(DateTime date, string day, string what )
        {
            int iday = day.ObjToInt32();
            string str = date.Year.ToString("D4") + "-" + date.Month.ToString("D2") + "-" + iday.ToString("D2") + "~" + what;
            DataRow dRow = vacationDt.NewRow();
            dRow["what"] = str;
            vacationDt.Rows.Add(dRow);

            PreProcessNew();

            SetColors();
        }
        /****************************************************************************************/
        private void PreProcessNew( DataTable dt = null )
        {
            bool gotDt = true;
            if (dt == null)
            {
                dt = (DataTable)dgv.DataSource;
                gotDt = false;
            }

            DateTime startDate = DateTime.Now;
            DateTime stopDate = DateTime.Now;
            DateTime date = DateTime.Now;

            bool gotStart = false;
            bool gotStop = false;

            string what = "";

            string[] Lines = null;

            for (int i = 0; i < vacationDt.Rows.Count; i++)
            {
                what = vacationDt.Rows[i]["what"].ObjToString();
                if (what.ToUpper().IndexOf("REQUESTED") > 0)
                    continue;
                Lines = what.Split('~');
                if (Lines.Length < 2)
                    continue;
                date = Lines[0].ObjToDateTime();
                if (Lines[1].Trim().IndexOf("Start") >= 0)
                {
                    startDate = date;
                    gotStart = true;
                }
                else if (Lines[1].Trim().IndexOf("Stop") >= 0)
                {
                    if (!gotStart)
                        continue;
                    stopDate = date;
                    ProcessVacationStop(dt, what, startDate, stopDate, "");
                    gotStart = false;
                }
            }
            if (!gotDt)
            {
                dgv.DataSource = dt;
                dgv.Refresh();
            }
        }
        /***************************************************************************************/
        private void SetColors()
        {
            string data = "";
            string[] Lines = null;

            int calStartDay = -1;
            int calStopDay = -1;
            int testRow = -1;
            int col = 0;

            DateTime startDate = DateTime.Now;
            DateTime stopDate = DateTime.Now;
            DateTime newDate = DateTime.Now;

            bool gotStart = false;
            bool doit = false;

            DataTable dt = (DataTable)dgv.DataSource;

            for (int i = 0; i < vacationDt.Rows.Count; i++)
            {
                try
                {
                    data = vacationDt.Rows[i]["what"].ObjToString();
                    if (data.ToUpper().IndexOf("START VACATION") > 0)
                    {
                        Lines = data.Split('~');
                        //calStartDay = Lines[0].ObjToInt32();
                        startDate = Lines[0].ObjToDateTime();
                        gotStart = true;
                    }
                    else if (data.ToUpper().IndexOf("STOP VACATION") > 0)
                    {
                        if ( !gotStart )
                            continue;
                        Lines = data.Split('~');
                        //calStopDay = Lines[0].ObjToInt32();
                        stopDate = Lines[0].ObjToDateTime();

                        gotStart = false;

                        for (int j = 0; j < dt.Columns.Count; j++)
                        {
                            for (int k = 0; k < dt.Rows.Count; k++)
                            {
                                data = dt.Rows[k][j].ObjToString();
                                if (String.IsNullOrWhiteSpace(data))
                                    continue;
                                Lines = data.Split('\n');

                                data = workDate.Year.ToString() + "-" + workDate.Month.ToString() + "-" + Lines[0];
                                newDate = data.ObjToDateTime();
                                if ( newDate >= startDate && newDate <= stopDate )
                                {
                                    colorDt.Rows[k][j] = "COLOR";
                                }
                            }
                        }
                    }
                    else
                    {
                        doit = false;
                        if (data.ToUpper().IndexOf("8 HOURS") >= 0)
                            doit = true;
                        else if (data.ToUpper().IndexOf("6 HOURS") >= 0)
                            doit = true;
                        else if (data.ToUpper().IndexOf("4 HOURS") >= 0)
                            doit = true;
                        else if (data.ToUpper().IndexOf("2 HOURS") >= 0)
                            doit = true;
                        if ( doit )
                        {
                            Lines = data.Split('~');
                            startDate = Lines[0].ObjToDateTime();
                            for (int j = 0; j < dt.Columns.Count; j++)
                            {
                                for (int k = 0; k < dt.Rows.Count; k++)
                                {
                                    data = dt.Rows[k][j].ObjToString();
                                    if (String.IsNullOrWhiteSpace(data))
                                        continue;
                                    Lines = data.Split('\n');

                                    data = workDate.Year.ToString() + "-" + workDate.Month.ToString() + "-" + Lines[0];
                                    newDate = data.ObjToDateTime();
                                    if (newDate == startDate )
                                    {
                                        colorDt.Rows[k][j] = "COLOR";
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                }
            }
        }
        /****************************************************************************************/
        private void btnRight_Click(object sender, EventArgs e)
        {
            DateTime date = this.dateTimePicker1.Value;
            date = date.AddMonths(1);
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            date = new DateTime(date.Year, date.Month, days);
            this.dateTimePicker1.Value = date;

            workDate = date;

            LoadData();
        }
        /****************************************************************************************/
        private void btnLeft_Click(object sender, EventArgs e)
        {
            DateTime date = this.dateTimePicker1.Value;
            date = date.AddMonths(-1);
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            date = new DateTime(date.Year, date.Month, days);
            this.dateTimePicker1.Value = date;

            workDate = date;

            LoadData();
        }
        /***************************************************************************************/
        public delegate void d_string_eventdone_string(DataTable dt);
        public event d_string_eventdone_string CalendarDone;
        /***************************************************************************************/
        protected void OnCalendarDone()
        {
            if (CalendarDone != null)
                CalendarDone(vacationDt);
        }
        /****************************************************************************************/
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadData();
        }
        /****************************************************************************************/
        private void Menu_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menu = (ToolStripMenuItem)sender;
            string hrGroup = menu.Text.ObjToString();
        }
        /***************************************************************************************/
        public void InitCalander(string title)
        {
            if (CalendarDt == null)
            {
                CalendarDt = new DataTable();
                CalendarDt.Columns.Add("Title");
                CalendarDt.Columns.Add("Who");
                CalendarDt.Columns.Add("Start");
                CalendarDt.Columns.Add("Stop");
                CalendarDt.Columns.Add("Location");
                CalendarDt.Columns.Add("Details");
            }
            if (CalendarDt != null)
            {
                CalendarDt.Rows.Clear();
            }
        }
        /****************************************************************************************/
        public void AddCalanderEvent(string title, string who, string details, string location, DateTime start, DateTime stop)
        {
            if (CalendarDt == null)
                InitCalander(title);
            DataRow dRow = CalendarDt.NewRow();
            dRow["Title"] = title;
            dRow["Who"] = who;
            dRow["Details"] = details;
            dRow["Location"] = location;
            dRow["Start"] = start;
            dRow["Stop"] = stop;
        }
        /****************************************************************************************/
        public DataTable GetCalendarEvents()
        {
            return CalendarDt;
        }
        /****************************************************************************************/
    }
}