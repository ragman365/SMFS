using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraBars;
using System.Linq;
using System.Diagnostics;
using System.IO;
using DevExpress.XtraGrid;
using DevExpress.XtraPrinting;
using System.Data.OleDb;
using GeneralLib;

using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Base.ViewInfo;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraGrid.Views.BandedGrid;
using DevExpress.XtraGrid.Views.BandedGrid.ViewInfo;
using DevExpress.XtraGrid.Columns;

using System.Drawing.Printing;
using System.Drawing.Imaging;
using System.Collections;
using System.CodeDom.Compiler;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Collections.Specialized;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using DevExpress.Utils;

using MySql.Data.MySqlClient;
using System.Configuration;
using System.Threading;
using MySql.Data.Types;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class TimeOffRequest : Form
    {
        private string work_empno = "";
        private string work_name = "";
        private string work_supers = "";
        private double work_december = 0D;
        private double work_ptonow = 0D;
        private DateTime workDate = DateTime.Now;
        private DataTable workDt = null;
        /***********************************************************************************************/
        public TimeOffRequest( string empno, string name, string supers, double ptonow, double december, DateTime date, DataTable dt )
        {
            InitializeComponent();
            work_empno = empno;
            work_name = name;
            work_supers = supers;
            work_ptonow = ptonow;
            work_december = december;
            workDate = date;
            workDt = dt;
        }
        /***********************************************************************************************/
        private void LoadSuperCombo()
        {
            DataTable dx = null;
            string[] Lines = work_supers.Split('\n');
            for ( int i=0; i<Lines.Length; i++ )
            {
                string super = "";
                string supno = Lines[i].Trim();
                if (!String.IsNullOrWhiteSpace(supno))
                {
                    string cmd = "Select * from `tc_er` where `empno` = '" + supno + "';";
                    DataTable dt = G1.get_db_data(cmd);
                    if (dt.Rows.Count > 0)
                        super = dt.Rows[0]["firstname"].ObjToString();
                    super += " (" + supno + ")";
                    comboBox1.Items.Add(super);
                }
            }
            if (dx != null)
                dx.Dispose();
            dx = null;
        }
        /***********************************************************************************************/
        private void TimeOffRequest_Load(object sender, EventArgs e)
        {
            dgv.Hide();
            dgv.Dock = DockStyle.Fill;

            rtb.Dock = DockStyle.Fill;

            LoadSuperCombo();

            btnMakeRequest.Enabled = false;

            DateTime now = DateTime.Now;
            txtDateRequested.Text = now.Month.ToString("D2") + "/" + now.Day.ToString("D2") + "/" + now.Year.ToString("D4");
            DataTable dx = G1.get_db_data("Select * from `tc_er` where `username` = '" + work_empno + "';");
            if (dx.Rows.Count <= 0)
                return;
            //string jobcode = dx.Rows[0]["jobcode"].ObjToString();
            string super = dx.Rows[0]["TimeKeeper"].ObjToString();
            //double pto_now = dx.Rows[0]["pto_now"].ObjToDouble();
            //double december = dx.Rows[0]["december"].ObjToDouble();
            //if (String.IsNullOrWhiteSpace(super))
            //{
            //    dx = G1.get_db_data("Select * from `jobs` where `jobcode` = '" + jobcode + "';");
            //    if ( dx.Rows.Count > 0 )
            //    {
            //        string sup = dx.Rows[0]["super"].ObjToString();
            //        dx = G1.get_db_data("Select * from `er` where `empno` = '" + sup + "';");
            //        if (dx.Rows.Count > 0)
            //            super = dx.Rows[0]["firstname"].ObjToString() + " (" + sup + ")";
            //    }
            //}
            //this.txtDecemberPTO.Text = "  " + work_december.ToString("###,###.00");
            //this.txtAvailablePTO.Text = "  " + work_ptonow.ToString("###,###.00");
            comboBox1.Text = super;
            lblBy.Visible = false;
            btnApproved.Visible = false;
            txtApprovedBy.Visible = false;
            this.Text = "Time Off Request for " + work_name;
            //CalcTimeOff();
        }
        /***********************************************************************************************/
        private void CalcTimeOff()
        {
            string cmd = "Select * from `tc_er` where `username` = '" + work_empno + "';";
            DataTable dt = G1.get_db_data(cmd);
            int hoursExpected = 8;
            int hours = 0;
            //DateTime date = dateTimePicker1.Value;
            //date = new DateTime(date.Year, date.Month, date.Day);
            //date = date.AddDays(-1);

            //DateTime date2 = dateTimePicker2.Value;
            //date2 = new DateTime(date2.Year, date2.Month, date2.Day);
            //for (;;)
            //{
            //    date = date.AddDays(1);
            //    if (date > date2)
            //    {
            //        if (hours <= 0)
            //            hours = hoursExpected;
            //        break;
            //    }
            //    DayOfWeek dow = date.DayOfWeek;
            //    hours = hours + hoursExpected;
            //}
            //if (hours > 0)
            //{
            //    txtRequested.Text = hours.ToString();
            //    txtRequested.Refresh();
            //}
        }
        /***********************************************************************************************/
        private void btnMakeRequest_Click(object sender, EventArgs e)
        {
            string supervisor = this.comboBox1.Text;
            if ( String.IsNullOrWhiteSpace ( supervisor ))
            {
                MessageBox.Show("***ERROR*** Must have supervisor assigned!");
                return;
            }
            int idx = supervisor.IndexOf("(");
            if ( idx > 0 )
            {
                supervisor = supervisor.Substring((idx + 1));
                supervisor = supervisor.Replace(")", "");
            }

            DateTime startDate = DateTime.Now;
            DateTime stopDate = DateTime.Now;
            DateTime newDate = DateTime.Now;
            DateTime date = DateTime.Now;
            DateTime dateRequested = DateTime.Now;

            string cDate1 = "";
            string cDate2 = "";

            string str = "";
            string record = "";
            bool gotStart = false;
            double hours = 0D;
            double days = 0D;

            string rtbTest = rtb.Text;
            string[] Lines = rtbTest.Split('\n');
            string[] lines = null;
            for ( int i=0; i<Lines.Length; i++)
            {
                str = Lines[i].Trim();
                if (String.IsNullOrWhiteSpace(str))
                    continue;
                if ( str.ToUpper().IndexOf("START VACATION") >= 0 )
                {
                    gotStart = true;
                    lines = str.Split(' ');
                    date = lines[0].ObjToDateTime();
                    startDate = date;
                }
                else if (str.ToUpper().IndexOf("STOP VACATION") >= 0)
                {
                    lines = str.Split(' ');
                    date = lines[0].ObjToDateTime();
                    stopDate = date;
                    if ( str.ToUpper().IndexOf ( "NOT AVAILABLE") < 0 )
                    {
                        record = G1.create_record("tc_timerequest", "user", "-1");
                        if (!String.IsNullOrWhiteSpace(record) && record != "-1")
                        {
                            TimeSpan ts = stopDate - startDate;
                            days = ts.TotalDays + 1;
                            hours = days * 8D;

                            cDate1 = startDate.ToString("MM/dd/yyyy");
                            cDate2 = stopDate.ToString("MM/dd/yyyy");
                            G1.update_db_table("tc_timerequest", "record", record, new string[] { "empno", work_empno, "supervisor", supervisor, "fromdate", cDate1, "todate", cDate2, "requested_hours", hours.ToString(), "OtherInformation", "", "date_requested", dateRequested.ToString("MM/dd/yyyy"), "name", work_name });
                            //G1.update_db_table("tc_timerequest", "record", record, new string[] { "name", name, "pto_now", ptonow.ToString(), "december", december.ToString(), "date_requested", date_requested, "user", LoginForm.username });
                        }
                    }
                    gotStart = false;
                }
                else
                {
                    lines = str.Split(' ');
                    date = lines[0].ObjToDateTime();
                    hours = lines[1].ObjToDouble();
                    if (str.ToUpper().IndexOf("NOT AVAILABLE") < 0)
                    {
                        record = G1.create_record("tc_timerequest", "user", "-1");
                        if (!String.IsNullOrWhiteSpace(record) && record != "-1")
                        {
                            cDate1 = date.ToString("MM/dd/yyyy");
                            cDate2 = date.ToString("MM/dd/yyyy");
                            G1.update_db_table("tc_timerequest", "record", record, new string[] { "empno", work_empno, "supervisor", supervisor, "fromdate", cDate1, "todate", cDate2, "requested_hours", hours.ToString(), "OtherInformation", "", "date_requested", dateRequested.ToString("MM/dd/yyyy"), "name", work_name });
                            //G1.update_db_table("tc_timerequest", "record", record, new string[] { "name", name, "pto_now", ptonow.ToString(), "december", december.ToString(), "date_requested", date_requested, "user", LoginForm.username });
                        }
                    }
                }
            }
            //DateTime fromDate = this.dateTimePicker1.Value;
            //DateTime toDate = this.dateTimePicker2.Value;
            //string cdate1 = fromDate.Year.ToString("D4") + "-" + fromDate.Month.ToString("D2") + "-" + fromDate.Day.ToString("D2");
            //string cdate2 = toDate.Year.ToString("D4") + "-" + toDate.Month.ToString("D2") + "-" + toDate.Day.ToString("D2");
            //string hours = this.txtRequested.Text;
            //string otherInfo = this.rtb.Text;

            //DateTime now = DateTime.Now;

            //string date_requested = now.Year.ToString("D4") + "-" + now.Month.ToString("D2") + "-" + now.Day.ToString("D2");

            //string name = "";
            //double ptonow = work_ptonow;
            //double december = work_december;

            //DataTable dt = G1.get_db_data("Select * from `users` u JOIN `tc_er` t ON u.`username` = t.`username` where u.`username` = '" + work_empno + "';");
            //if ( dt.Rows.Count > 0 )
            //{
            //    name = dt.Rows[0]["lastName"].ObjToString() + ", " + dt.Rows[0]["firstName"].ObjToString();
            //    //ptonow = dt.Rows[0]["pto_now"].ObjToDouble();
            //    //december = dt.Rows[0]["december"].ObjToDouble();
            //}

            //string record = "";
            //record = G1.create_record("tc_timerequest", "user", "-1");
            //if ( !String.IsNullOrWhiteSpace ( record) && record != "-1" )
            //{
            //    G1.update_db_table("tc_timerequest", "record", record, new string[] { "empno", work_empno, "supervisor", supervisor, "fromdate", cdate1, "todate", cdate2, "requested_hours", hours, "OtherInformation", otherInfo });
            //    G1.update_db_table("tc_timerequest", "record", record, new string[] { "name", name, "pto_now", ptonow.ToString(), "december", december.ToString(), "date_requested", date_requested, "user", LoginForm.username });
            //}
            this.Close();
        }
        /***********************************************************************************************/
        private void btnCalendar_Click(object sender, EventArgs e)
        {
            Calendar2 calendarForm = new Calendar2(work_empno, work_name, workDate, workDt );
            calendarForm.CalendarDone += CalendarForm_CalendarDone;
            calendarForm.Show();
        }
        /***********************************************************************************************/
        private void CalendarForm_CalendarDone(DataTable dt)
        {
            if (dt == null)
                return;
            if (dt.Rows.Count <= 0)
                return;

            btnMakeRequest.Enabled = false;

            string[] Lines = null;
            double hours = 0D;
            double totalHours = 0D;
            double days = 0D;
            DateTime startDate = DateTime.Now;
            DateTime stopDate = DateTime.Now;
            DateTime testDate = DateTime.Now;
            DateTime date = DateTime.Now;
            string what = "";
            bool gotStart = false;
            bool somethingIsGood = false;
            rtb.Clear();
            string str = "";
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                str = dt.Rows[i]["what"].ObjToString();
                Lines = str.Split('~');

                date = Lines[0].ObjToDateTime();
                what = Lines[1].ObjToString();

                str = str.Replace("~", " ");

                if (what.ToUpper().IndexOf("REQUESTED") >= 0)
                    continue;

                if ( what.ToUpper().IndexOf ( "START VACATION" ) >= 0 )
                {
                    gotStart = true;
                    startDate = date;
                    rtb.AppendText(str + "\n");
                }
                else if ( what.ToUpper().IndexOf ( "STOP VACATION") >= 0 )
                {
                    stopDate = date;
                    bool good = VerifyVacation(startDate, stopDate);
                    if (good)
                    {
                        somethingIsGood = true;
                        gotStart = false;
                        stopDate = date;
                        TimeSpan ts = stopDate - startDate;
                        days = ts.TotalDays + 1D;
                        hours = days * 8D;
                        totalHours += hours;
                        rtb.AppendText(str + " Available\n");
                    }
                    else
                    {
                        rtb.AppendText(str + " Not Available\n");
                        //btnMakeRequest.Enabled = false;
                    }
                }
                else
                {
                    bool good = VerifyVacation(date, date);
                    if (good)
                    {
                        somethingIsGood = true;
                        Lines = str.Split(' ');
                        hours = Lines[1].ObjToDouble();
                        totalHours += hours;
                        rtb.AppendText(str + " Available\n");
                    }
                    else
                    {
                        rtb.AppendText(str + " Not Available\n");
                        //btnMakeRequest.Enabled = false;
                    }
                }
            }
            if (somethingIsGood)
                btnMakeRequest.Enabled = true;

            txtRequested.Text = G1.ReformatMoney(totalHours);
        }
        /***********************************************************************************************/
        private bool VerifyVacation ( DateTime startDate, DateTime stopDate )
        {
            bool good = false;

            string date1 = startDate.ToString("yyyy-MM-dd");
            string date2 = stopDate.ToString("yyyy-MM-dd");

            string cmd = "Select * from `tc_er` WHERE `username` = '" + work_empno + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return false;

            string myLocation = dt.Rows[0]["location"].ObjToString();
            if (String.IsNullOrWhiteSpace(myLocation))
                return false;

            cmd = "Select * from `tc_hr_groups` WHERE `locations` LIKE '%" + myLocation + "%';";
            dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return false;

            string locations = dt.Rows[0]["locations"].ObjToString();
            string[] Lines = locations.Split('~');
            string query = "";
            string location = "";
            for ( int i=0; i<Lines.Length; i++)
            {
                location = Lines[i].Trim();
                query += "'" + location + "',";
            }
            query = query.TrimEnd(',');

            try
            {
                cmd = "Select * from `tc_timerequest` r JOIN `tc_er` e ON r.`empno` = e.`username` JOIN `users` u ON r.`empno` = u.`username` WHERE '" + date1 + "' >= `fromDate` AND '" + date1 + "' <= `toDate` AND e.`location` IN (" + query + ");";
                dt = G1.get_db_data(cmd);
                if (dt.Rows.Count > 0)
                {
                    string otheremp = dt.Rows[0]["firstName"].ObjToString() + " " + dt.Rows[0]["lastName"].ObjToString();
                    string xdate1 = dt.Rows[0]["fromDate"].ObjToDateTime().ToString("yyyy-MM-dd");
                    string xdate2 = dt.Rows[0]["toDate"].ObjToDateTime().ToString("yyyy-MM-dd");
                    MessageBox.Show("*** PROBLEM *** One of your Vacation dates overlaps with\n" + otheremp + "!\nDates are " + xdate1 + " to " + xdate2 + "\nYou must try to resolve!", "Conflicting Vacation Dates Dialog", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    return false;
                }
                cmd = "Select * from `tc_timerequest` r JOIN `tc_er` e ON r.`empno` = e.`username` JOIN `users` u ON r.`empno` = u.`username` WHERE '" + date2 + "' >= `fromDate` AND '" + date2 + "' <= `toDate` AND e.`location` IN (" + query + ");";
                dt = G1.get_db_data(cmd);
                if (dt.Rows.Count > 0)
                {
                    string otheremp = dt.Rows[0]["firstName"].ObjToString() + " " + dt.Rows[0]["lastName"].ObjToString();
                    string xdate1 = dt.Rows[0]["fromDate"].ObjToDateTime().ToString("yyyy-MM-dd");
                    string xdate2 = dt.Rows[0]["toDate"].ObjToDateTime().ToString("yyyy-MM-dd");
                    MessageBox.Show("*** PROBLEM *** One of your Vacation dates overlaps with\n" + otheremp + "!\nDates are " + xdate1 + " to " + xdate2 + "\nYou must try to resolve!", "Conflicting Vacation Dates Dialog", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    return false;
                }
            }
            catch ( Exception ex)
            {
            }

            return true;
        }
        /***********************************************************************************************/
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

            BuildRequestDetails();

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
        private void BuildRequestDetails ()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("detail");

            DataRow dR = null;

            string text = rtb.Text;
            string[] Lines = text.Split('\n');
            for ( int i=0; i<Lines.Length; i++)
            {
                dR = dt.NewRow();
                dR["detail"] = Lines[i].Trim();
                dt.Rows.Add(dR);
            }
            dgv.DataSource = dt;
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

            BuildRequestDetails();

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
        /***********************************************************************************************/
    }
}
