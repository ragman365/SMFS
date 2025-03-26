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


using System.Net;
using System.Net.Sockets;
using System.IO.Compression;

using iTextSharp.text.pdf;
using System.IO;
using System.Text.RegularExpressions;

using System.Windows.Forms.VisualStyles;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraEditors.ViewInfo;
using DevExpress.Utils.Drawing;
using System.Drawing.Drawing2D;
using Excel = Microsoft.Office.Interop.Excel;
using System.Reflection;
using ClosedXML.Excel;
using DocumentFormat.OpenXml;
using OfficeOpenXml.FormulaParsing;
//using Microsoft.Office.Interop.Excel;

/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class Unity : DevExpress.XtraEditors.XtraForm
    {
        string workReport = "UnityImport";
        private bool foundLocalPreference = false;
        private string work_empno = "";
        private string work_myName = "";
        private string work_username = "";
        private bool loading = true;
        private bool justTimeKeeper = false;
        private bool justManager = false;
        private DataTable mainDt = null;
        private DataTable funDt = null;
        public static bool showRates = true;
        private string workGroupName = "";
        /***********************************************************************************************/
        public Unity()
        {
            InitializeComponent();
        }
        /***********************************************************************************************/
        private void Unity_Load(object sender, EventArgs e)
        {
            btnLoadOthers.Hide();
            btnExportToExcel.Hide();
            chkExcludeHeader.Hide();
            btnSave.Hide();
            chkHonorPrevious.Hide();
            btnImportData.Hide();

            HideOrShowTabs(false);

            Rectangle rect = this.Bounds;

            int width = rect.Width - 100;
            int left = rect.Left;

            this.SetBounds(left, rect.Top, width, rect.Height);

            loading = false;
        }
        /***********************************************************************************************/
        private void HideOrShowTabs(bool showTabs)
        {
            if (!showTabs)
            {
                tabControl1.TabPages.Remove(tabPageUnityActive);
                tabControl1.TabPages.Remove(tabPageUnityLapsed);
                tabControl1.TabPages.Remove(tabPageUnityQuestioned);
                tabControl1.TabPages.Remove(tabPageUnityDeceased);
                tabControl1.TabPages.Remove(tabPageUnityCancelled);
                tabControl1.TabPages.Remove(tabPagePBUnityAC);
                tabControl1.TabPages.Remove(tabPagePBUnityDEC);
                tabControl1.TabPages.Remove(tabPageBarham);
                tabControl1.TabPages.Remove(tabPageWebb);
                tabControl1.TabPages.Remove(tabPageBarhamWebbDEC);
                tabControl1.TabPages.Remove(tabPagePBDirectIssue);
                tabControl1.TabPages.Remove(tabPageNotFound);
                tabControl1.TabPages.Remove(tabPageSummary);
            }
            else
            {
                tabControl1.TabPages.Add(tabPageUnityActive);
                tabControl1.TabPages.Add(tabPageUnityLapsed);
                tabControl1.TabPages.Add(tabPageUnityQuestioned);
                tabControl1.TabPages.Add(tabPageUnityDeceased);
                tabControl1.TabPages.Add(tabPageUnityCancelled);
                tabControl1.TabPages.Add(tabPagePBUnityAC);
                tabControl1.TabPages.Add(tabPagePBUnityDEC);
                tabControl1.TabPages.Add(tabPageBarham);
                tabControl1.TabPages.Add(tabPageWebb);
                tabControl1.TabPages.Add(tabPageBarhamWebbDEC);
                tabControl1.TabPages.Add(tabPagePBDirectIssue);
                tabControl1.TabPages.Add(tabPageNotFound);
                tabControl1.TabPages.Add(tabPageSummary);
            }
        }
        /***********************************************************************************************/
        private void HideGridChooser(DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gridMain)
        {
            for (int i = 0; i < gridMain.Columns.Count; i++)
            {
                if (!gridMain.Columns[i].Visible)
                    gridMain.Columns[i].OptionsColumn.ShowInCustomizationForm = false;
            }
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
        private void gridMain_CustomDrawCell(object sender, RowCellCustomDrawEventArgs e)
        {
            GridView view = sender as GridView;
            DataTable dt = (DataTable)(dgv.DataSource);
            int row = e.RowHandle;
            if (e.Column.FieldName.ToUpper() == "NUM")
                e.DisplayText = (row + 1).ToString();
        }
        /***********************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
        }
        /***********************************************************************************************/
        private void LoadTimeKeepers()
        {
        }
        /***********************************************************************************************/
        DataTable SavedSuperDt = null;
        int SavedSuperRow = -1;
        string SavedSupervisor = "";
        string savedJobCodes = "";
        /***********************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (dgv.Visible)
                G1.SpyGlass(gridMain);
            else if (dgv2.Visible)
                G1.SpyGlass(gridMain2);
            else if (dgv3.Visible)
                G1.SpyGlass(gridMain3);
            else if (dgv4.Visible)
                G1.SpyGlass(gridMain4);
            else if (dgv5.Visible)
                G1.SpyGlass(gridMain5);
            else if (dgv6.Visible)
                G1.SpyGlass(gridMain6);
            else if (dgv7.Visible)
                G1.SpyGlass(gridMain7);
            else if (dgv8.Visible)
                G1.SpyGlass(gridMain8);
            else if (dgv9.Visible)
                G1.SpyGlass(gridMain9);
            else if (dgv10.Visible)
                G1.SpyGlass(gridMain10);
            else if (dgv11.Visible)
                G1.SpyGlass(gridMain11);
            else if (dgv12.Visible)
                G1.SpyGlass(gridMain12);
            else if (dgv13.Visible)
                G1.SpyGlass(gridMain13);
            else if (dgv14.Visible)
                G1.SpyGlass(gridMain14);
        }
        /***********************************************************************************************/
        private bool printToExcel = false;
        private void printPreviewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetupPrintPage();

            if (printToExcel)
            {
                TabPage current = tabControl1.SelectedTab;
                tabTitle = current.Text.Trim();
                tabName = current.Name;

                string outputDirectory = @"C:\SMFSData\Unity Reports";
                G1.verify_path(outputDirectory);
                string fullPath = outputDirectory + "/" + this.Text + " " + tabTitle + ".xls";
                if (File.Exists(fullPath))
                    File.Delete(fullPath);
                printableComponentLink1.ExportToXls(fullPath);
            }
            else
                printableComponentLink1.ShowPreview();
        }
        /***********************************************************************************************/
        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetupPrintPage();
            printableComponentLink1.PrintDlg();
        }
        /***********************************************************************************************/
        private int pageMarginLeft = 0;
        private int pageMarginRight = 0;
        private int pageMarginTop = 0;
        private int pageMarginBottom = 0;
        /***********************************************************************************************/
        private bool SetVisiblePrintTab(DevExpress.XtraGrid.GridControl dgv)
        {
            bool done = false;
            if (dgv.Visible)
            {
                printableComponentLink1.Component = dgv;
                printableComponentLink1.Landscape = true;
                done = true;
            }
            return done;
        }
        /***********************************************************************************************/
        private void SetupPrintPage()
        {
            bool done = SetVisiblePrintTab(dgv);
            if (!done)
                done = SetVisiblePrintTab(dgv2);
            if (!done)
                done = SetVisiblePrintTab(dgv3);
            if (!done)
                done = SetVisiblePrintTab(dgv4);
            if (!done)
                done = SetVisiblePrintTab(dgv5);
            if (!done)
                done = SetVisiblePrintTab(dgv6);
            if (!done)
                done = SetVisiblePrintTab(dgv7);
            if (!done)
                done = SetVisiblePrintTab(dgv8);
            if (!done)
                done = SetVisiblePrintTab(dgv9);
            if (!done)
                done = SetVisiblePrintTab(dgv10);
            if (!done)
                done = SetVisiblePrintTab(dgv11);
            if (!done)
                done = SetVisiblePrintTab(dgv12);
            if (!done)
                done = SetVisiblePrintTab(dgv13);
            if (!done)
                done = SetVisiblePrintTab(dgv14);

            int top = 80;
            if (chkExcludeHeader.Checked)
                top = 10;

            Printer.setupPrinterMargins(10, 10, top, 50);

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
        private void printableComponentLink1_CreateMarginalHeaderArea(object sender, CreateAreaEventArgs e)
        {
            if (chkExcludeHeader.Checked)
                return;

            Printer.setupPrinterQuads(e, 2, 3);
            Font font = new Font("Ariel", 16);
            Printer.DrawQuad(1, 1, Printer.xQuads, 2, "South Mississippi Funeral Services, LLC", Color.Black, BorderSide.Top, font, HorizontalAlignment.Center);

            Printer.SetQuadSize(12, 12);

            font = new Font("Ariel", 8);
            Printer.DrawGridDate(2, 3, 2, 3, Color.Black, BorderSide.None, font);
            Printer.DrawGridPage(11, 3, 2, 3, Color.Black, BorderSide.None, font);

            Printer.DrawQuad(1, 9, 2, 3, "User : " + LoginForm.username, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);

            font = new Font("Ariel", 10, FontStyle.Regular);

            Printer.DrawQuad(6, 8, 4, 4, tabTitle, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            //            Printer.DrawQuadTicks();
            //DateTime date = this.dateTimePicker1.Value;
            //string workDate = date.ToString("MM/dd/yyyy") + " - ";
            //date = this.dateTimePicker2.Value;
            //workDate += date.ToString("MM/dd/yyyy");

            Printer.SetQuadSize(24, 12);
            font = new Font("Ariel", 9, FontStyle.Regular);
            //if (dgv2.Visible)
            //    Printer.DrawQuad(18, 8, 10, 4, "Pay Period:" + workDate, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(16, 8, 3, 4, lblPayment.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(19, 8, 3, 4, lblTrust85.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(22, 8, 3, 4, lblTrust100.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
        }

        /***********************************************************************************************/
        private void printableComponentLink1_CreateMarginalHeaderAreax(object sender, CreateAreaEventArgs e)
        {
            // Date in upper left corner
            PageInfoBrick printDate = e.Graph.DrawPageInfo(PageInfo.DateTime, "{0:MM/dd/yyyy HH:mm}", Color.DarkBlue, new RectangleF(0, 0, 200, 18), BorderSide.None);

            // Create and Draw the Report Title, Include Thick bottom border
            DateTime date1 = DateTime.Now;
            string title = "";

            TextBrick textBrick = e.Graph.DrawString(title, Color.Black, new RectangleF(0, 18, e.Graph.ClientPageSize.Width, 24), DevExpress.XtraPrinting.BorderSide.Bottom);
            textBrick.BorderWidth = 2;
            textBrick.Font = new Font("Arial", 16);
            textBrick.HorzAlignment = HorzAlignment.Center;
            textBrick.VertAlignment = VertAlignment.Top;

            // RightTopPanel
            // Page Number Brick
            TextBrick pageNumberLabel = new TextBrick(BorderSide.None, 0, Color.Black, Color.Transparent, Color.DarkBlue);
            pageNumberLabel.Text = "PAGE NO.";
            pageNumberLabel.Rect = new RectangleF(0, 0, 144, 18);
            PageInfoBrick pageNumberInfo = new PageInfoBrick(BorderSide.None, 0, Color.Black, Color.Transparent, Color.DarkBlue);
            pageNumberInfo.PageInfo = PageInfo.Number;
            pageNumberInfo.Rect = new RectangleF(100, 0, 84, 18);
            pageNumberInfo.HorzAlignment = HorzAlignment.Far;

            bool doit = true;

            // UserName Brick
            string str = "";
            TextBrick userIDLabel = new TextBrick(BorderSide.None, 0, Color.Black, Color.Transparent, Color.DarkBlue);
            //            userIDLabel.Text = "USERID";
            userIDLabel.Text = "";
            userIDLabel.Rect = new RectangleF(0, 18, 250, 18);
            //PageInfoBrick userIDInfo = new PageInfoBrick(BorderSide.None, 0, Color.Black, Color.Transparent, Color.DarkBlue);
            //userIDInfo.PageInfo = PageInfo.UserName;
            //userIDInfo.Rect = new RectangleF(60, 18, 84, 18);

            // Create RightTopPanel and Paint
            PanelBrick rightTopPanel = new PanelBrick();
            rightTopPanel.BorderWidth = 0;
            rightTopPanel.Bricks.Add(pageNumberLabel);
            rightTopPanel.Bricks.Add(pageNumberInfo);
            if (doit)
                rightTopPanel.Bricks.Add(userIDLabel);
            //            rightTopPanel.Bricks.Add(userIDInfo);
            //            e.Graph.DrawBrick(rightTopPanel, new RectangleF(816, 0, 144, 36));
            e.Graph.DrawBrick(rightTopPanel, new RectangleF(0, 45, 250, 36));

            // File Date Brick
            TextBrick fileIDLabel = new TextBrick(BorderSide.None, 0, Color.Black, Color.Transparent, Color.DarkBlue);
            fileIDLabel.Text = "TimeSheet Date";
            fileIDLabel.Rect = new RectangleF(0, 80, 144, 18);
            //title = this.dateTimePicker1.Text;
            //title += " -to- " + this.dateTimePicker2.Text;
            if (SavedSuperRow >= 0)
            {
                title = "Job Codes (" + savedJobCodes + ")";
                TextBrick fileBrick = e.Graph.DrawString(title, Color.Navy, new RectangleF(0, 80, e.Graph.ClientPageSize.Width, 24), DevExpress.XtraPrinting.BorderSide.Bottom);
                fileBrick.BorderWidth = 2;
                fileBrick.Font = new Font("Arial", 16);
                fileBrick.HorzAlignment = HorzAlignment.Center;
                fileBrick.VertAlignment = VertAlignment.Top;
            }
        }
        /***********************************************************************************************/
        private void gridMain_CellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            if (e == null)
                return;
            DataChanged();
        }
        /***********************************************************************************************/
        private void DataChanged()
        {
            if (loading)
                return;

            int rowHandle = gridMain.FocusedRowHandle;
            DataRow dr = gridMain.GetFocusedDataRow();
            if (dr == null)
                return;
            dr["mod"] = "1";
        }
        /***********************************************************************************************/
        private void gridMain_MouseDown(object sender, MouseEventArgs e)
        {

            DataTable dt = (DataTable)dgv.DataSource; // Leave as a GetDate example

            var hitInfo = gridMain.CalcHitInfo(e.Location);
            if (hitInfo.InRowCell)
            {
                int rowHandle = hitInfo.RowHandle;
                GridColumn column = hitInfo.Column;
                rowHandle = hitInfo.RowHandle;
                string currentColumn = column.FieldName.Trim();
                if (currentColumn.ToUpper().IndexOf("DATE") >= 0)
                {
                    //DataRow dr = gridMain.GetFocusedDataRow();
                    DataRow dr = gridMain.GetDataRow(rowHandle);
                    DateTime date = dr[currentColumn].ObjToDateTime();
                    using (GetDate dateForm = new GetDate(date, currentColumn))
                    {
                        dateForm.TopMost = true;
                        dateForm.ShowDialog();
                        if (dateForm.DialogResult == System.Windows.Forms.DialogResult.OK)
                        {
                            date = dateForm.myDateAnswer;
                            dr[currentColumn] = G1.DTtoMySQLDT(date);
                            gridMain.RefreshEditor(true);
                            dgv.RefreshDataSource();
                            dgv.Refresh();
                            DataChanged();
                        }
                    }
                }
            }
        }
        /***********************************************************************************************/
        private void gridMain_CustomColumnDisplayText(object sender, CustomColumnDisplayTextEventArgs e)
        {
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
        }
        /***********************************************************************************************/
        string oldWhat = "";
        /***********************************************************************************************/
        private void gridMain_ValidatingEditor(object sender, DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventArgs e)
        {
            GridView view = sender as GridView;
            if (view.FocusedColumn.FieldName.ToUpper().IndexOf("DATE") >= 0)
            {
                string column = view.FocusedColumn.FieldName;
                DataTable dt = (DataTable)dgv.DataSource;
                DataRow dr = gridMain.GetFocusedDataRow();
                int rowhandle = gridMain.FocusedRowHandle;
                int row = gridMain.GetDataSourceRowIndex(rowhandle);
                oldWhat = e.Value.ObjToString();
                DateTime date = oldWhat.ObjToDateTime();
                dt.Rows[row][column] = G1.DTtoMySQLDT(date);
                e.Value = G1.DTtoMySQLDT(date);
                //dt.Rows[row]["mod"] = "Y";
            }
        }
        /***********************************************************************************************/
        private int saveRow = -1;
        private int saveTopRow = -1;
        private string tabTitle = "All Policies";
        private string tabName = "";
        /***********************************************************************************************/
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            TabPage current = (sender as TabControl).SelectedTab;
            tabTitle = current.Text.Trim();
            tabName = current.Name;
            if (tabTitle == "Unity Active")
                SetupActivePolicies(gridMain2);
            else if (tabTitle == "Unity Lapsed")
                SetupLapsedPolicies(gridMain3);
            else if (tabTitle == "Unity Lapsed Questioned")
                SetupLapsedQuestioned(gridMain4);
            else if (tabTitle == "Unity Deceased")
                SetupDeceasedPolicies(gridMain5);
            else if (tabTitle == "Unity Cancelled")
                SetupCancelledPolicies(gridMain6);
            else if (tabTitle == "PB Unity AC")
                SetupPBUnityAC(gridMain7);
            else if (tabTitle == "PB Unity DEC")
                SetupPBUnityDEC(gridMain8);
            else if (tabTitle == "Barham")
                SetupBarham(gridMain9);
            else if (tabTitle == "Webb")
                SetupWebb(gridMain10);
            else if (tabTitle == "Barham & Webb DEC")
                SetupBarhamWebbDEC(gridMain11);
            else if (tabTitle == "PB Direct Issue")
                SetupPBDirectIssue(gridMain12);

            SetupTabFormat(tabName, tabTitle);
        }
        /****************************************************************************************/
        private void ClearAllPositions(DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gMain = null)
        {
            if (gMain == null)
                gMain = gridMain;
            for (int i = 0; i < gMain.Columns.Count; i++)
                gMain.Columns[i].Visible = false;
        }
        /***********************************************************************************************/
        private void SetupActivePolicies(DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gMain = null)
        {
            ClearAllPositions(gMain);
            int i = 1;
            G1.SetColumnPosition(gMain, "Num", i++);
            G1.SetColumnPosition(gMain, "FH Name", i++);
            G1.SetColumnPosition(gMain, "Policy Extract_Policy Status", i++);
            G1.SetColumnPosition(gMain, "Policy Number", i++);
            G1.SetColumnPosition(gMain, "Insured Last Name", i++);
            G1.SetColumnPosition(gMain, "Insured First Name", i++);
            G1.SetColumnPosition(gMain, "contractNumber", i++);
            G1.SetColumnPosition(gMain, "Death Benefit", i++);
            G1.SetColumnPosition(gMain, "Face Amount", i++);
            G1.SetColumnPosition(gMain, "Prior Premiums Paid", i++);
            G1.SetColumnPosition(gMain, "Prior Unapplied Cash", i++);
            G1.SetColumnPosition(gMain, "Prior IBA", i++);
            G1.SetColumnPosition(gMain, "Prior Cash Received", i++);
            G1.SetColumnPosition(gMain, "Current Monthly Premium", i++);
            G1.SetColumnPosition(gMain, "Current New Business", i++);
            G1.SetColumnPosition(gMain, "Policy Extract_Premiums Paid", i++);
            G1.SetColumnPosition(gMain, "Current Unapplied Cash", i++);
            G1.SetColumnPosition(gMain, "Current IBA", i++);
            G1.SetColumnPosition(gMain, "Current Cash Received", i++);
            G1.SetColumnPosition(gMain, "Balancing", i++);
            G1.SetColumnPosition(gMain, "O/S under $2", i++);
            G1.SetColumnPosition(gMain, "Paid Up Policies Refunded", i++);
            G1.SetColumnPosition(gMain, "Charlotte spreadsheet shows reversal", i++);
            G1.SetColumnPosition(gMain, "Other Reversal on Charlotte Spreadsheet", i++);
            G1.SetColumnPosition(gMain, "IBA/Unapplied Cash paid out at death claim", i++);
            G1.SetColumnPosition(gMain, "Change in status from Terminated to Reinstated cash value reapplied at reinstatement on lapsed policies", i++);
            G1.SetColumnPosition(gMain, "Diff in cash value in lapsed policy", i++);
            G1.SetColumnPosition(gMain, "Reconciling", i++);
            G1.SetColumnPosition(gMain, "explanaition", i++);
            G1.SetColumnPosition(gMain, "Date Claim Processed", i++);

            GridView gridView = (GridView)gMain;
            bool autoWidth = false, columnAutoWidth = false;
            Dictionary<GridColumn, int> widthByColumn = null;
            if (gridView != null)
            {
                autoWidth = gridView.OptionsPrint.AutoWidth;
                columnAutoWidth = gridView.OptionsView.ColumnAutoWidth;
                widthByColumn = gridView.Columns.ToDictionary(x => x, x => x.Width);

                gridView.OptionsPrint.AutoWidth = false;
                gridView.OptionsView.ColumnAutoWidth = false;
                string str = "";
                int width = 0;

                foreach (var item in widthByColumn)
                {
                    str = (item.Key).ObjToString();
                    width = (item.Value).ObjToInt32();

                    G1.SetColumnWidth ( gMain, str, width);
                }

                //gridView.OptionsPrint.AutoWidth = false;
                //gridView.OptionsView.ColumnAutoWidth = false;
                //gridView.BestFitColumns();
            }
        }
        /***********************************************************************************************/
        private void SetupLapsedPolicies(DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gMain = null)
        {
            ClearAllPositions(gMain);
            int i = 1;
            G1.SetColumnPosition(gMain, "Num", i++);
            G1.SetColumnPosition(gMain, "contractNumber", i++);
            G1.SetColumnPosition(gMain, "FH Name", i++);
            G1.SetColumnPosition(gMain, "Policy Extract_Policy Status", i++);
            G1.SetColumnPosition(gMain, "Policy Extract_Status Reason", i++);
            G1.SetColumnPosition(gMain, "Policy Number", i++);
            G1.SetColumnPosition(gMain, "Insured Last Name", i++);
            G1.SetColumnPosition(gMain, "Insured First Name", i++);
            G1.SetColumnPosition(gMain, "Death Benefit", i++);
            G1.SetColumnPosition(gMain, "Face Amount", i++);
            G1.SetColumnPosition(gMain, "Prior Premiums Paid", i++);
            G1.SetColumnPosition(gMain, "Prior Unapplied Cash", i++);
            G1.SetColumnPosition(gMain, "Prior IBA", i++);
            G1.SetColumnPosition(gMain, "Prior Cash Received", i++);
            G1.SetColumnPosition(gMain, "Current Monthly Premium", i++);
            G1.SetColumnPosition(gMain, "Current New Business", i++);
            G1.SetColumnPosition(gMain, "Policy Extract_Premiums Paid", i++);
            G1.SetColumnPosition(gMain, "Current Unapplied Cash", i++);
            G1.SetColumnPosition(gMain, "Current IBA", i++);
            G1.SetColumnPosition(gMain, "Current Cash Received", i++);
            G1.SetColumnPosition(gMain, "Balancing", i++);
            G1.SetColumnPosition(gMain, "O/S under $2", i++);
            G1.SetColumnPosition(gMain, "Paid Up Policies Refunded", i++);
            G1.SetColumnPosition(gMain, "Charlotte spreadsheet shows reversal", i++);
            G1.SetColumnPosition(gMain, "Other Reversal on Charlotte Spreadsheet", i++);
            G1.SetColumnPosition(gMain, "IBA/Unapplied Cash paid out at death claim", i++);
            G1.SetColumnPosition(gMain, "Change in status from Terminated to Reinstated cash value reapplied at reinstatement on lapsed policies", i++);
            G1.SetColumnPosition(gMain, "Diff in cash value in lapsed policy", i++);
            G1.SetColumnPosition(gMain, "Reconciling", i++);
            G1.SetColumnPosition(gMain, "explanaition", i++);
            G1.SetColumnPosition(gMain, "Date Claim Processed", i++);
        }
        /***********************************************************************************************/
        private void SetupLapsedQuestioned(DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gMain = null)
        {
            ClearAllPositions(gMain);
            int i = 1;
            G1.SetColumnPosition(gMain, "Num", i++);
            G1.SetColumnPosition(gMain, "contractNumber", i++);
            G1.SetColumnPosition(gMain, "FH Name", i++);
            G1.SetColumnPosition(gMain, "Policy Extract_Policy Status", i++);
            G1.SetColumnPosition(gMain, "Policy Extract_Status Reason", i++);
            G1.SetColumnPosition(gMain, "Policy Extract_Billing Reason", i++);
            G1.SetColumnPosition(gMain, "Policy Number", i++);
            G1.SetColumnPosition(gMain, "Insured Last Name", i++);
            G1.SetColumnPosition(gMain, "Insured First Name", i++);
            G1.SetColumnPosition(gMain, "Death Benefit", i++);
            G1.SetColumnPosition(gMain, "Face Amount", i++);
            G1.SetColumnPosition(gMain, "Prior Premiums Paid", i++);
            G1.SetColumnPosition(gMain, "Prior Unapplied Cash", i++);
            G1.SetColumnPosition(gMain, "Prior IBA", i++);
            G1.SetColumnPosition(gMain, "Prior Cash Received", i++);
            G1.SetColumnPosition(gMain, "Current Monthly Premium", i++);
            G1.SetColumnPosition(gMain, "Current New Business", i++);
            G1.SetColumnPosition(gMain, "Policy Extract_Premiums Paid", i++);
            G1.SetColumnPosition(gMain, "Current Unapplied Cash", i++);
            G1.SetColumnPosition(gMain, "Current IBA", i++);
            G1.SetColumnPosition(gMain, "Current Cash Received", i++);
            G1.SetColumnPosition(gMain, "Balancing", i++);
            G1.SetColumnPosition(gMain, "O/S under $2", i++);
            G1.SetColumnPosition(gMain, "Paid Up Policies Refunded", i++);
            G1.SetColumnPosition(gMain, "Charlotte spreadsheet shows reversal", i++);
            G1.SetColumnPosition(gMain, "Other Reversal on Charlotte Spreadsheet", i++);
            G1.SetColumnPosition(gMain, "IBA/Unapplied Cash paid out at death claim", i++);
            G1.SetColumnPosition(gMain, "Change in status from Terminated to Reinstated cash value reapplied at reinstatement on lapsed policies", i++);
            G1.SetColumnPosition(gMain, "Diff in cash value in lapsed policy", i++);
            G1.SetColumnPosition(gMain, "Reconciling", i++);
            G1.SetColumnPosition(gMain, "explanaition", i++);
            G1.SetColumnPosition(gMain, "Date Claim Processed", i++);
        }
        /***********************************************************************************************/
        private void SetupDeceasedPolicies(DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gMain = null)
        {
            ClearAllPositions(gMain);
            int i = 1;
            G1.SetColumnPosition(gMain, "Num", i++);
            G1.SetColumnPosition(gMain, "contractNumber", i++);
            G1.SetColumnPosition(gMain, "FH Name", i++);
            G1.SetColumnPosition(gMain, "Policy Extract_Policy Status", i++);
            G1.SetColumnPosition(gMain, "Policy Extract_Status Reason", i++);
            G1.SetColumnPosition(gMain, "Policy Number", i++);
            G1.SetColumnPosition(gMain, "Insured Last Name", i++);
            G1.SetColumnPosition(gMain, "Insured First Name", i++);
            G1.SetColumnPosition(gMain, "Death Benefit", i++);
            G1.SetColumnPosition(gMain, "Face Amount", i++);
            G1.SetColumnPosition(gMain, "Prior Premiums Paid", i++);
            G1.SetColumnPosition(gMain, "Prior Unapplied Cash", i++);
            G1.SetColumnPosition(gMain, "Prior IBA", i++);
            G1.SetColumnPosition(gMain, "Prior Cash Received", i++);
            G1.SetColumnPosition(gMain, "Current Monthly Premium", i++);
            G1.SetColumnPosition(gMain, "Current New Business", i++);
            G1.SetColumnPosition(gMain, "Policy Extract_Premiums Paid", i++);
            G1.SetColumnPosition(gMain, "Current Unapplied Cash", i++);
            G1.SetColumnPosition(gMain, "Current IBA", i++);
            G1.SetColumnPosition(gMain, "Current Cash Received", i++);
            G1.SetColumnPosition(gMain, "Balancing", i++);
            G1.SetColumnPosition(gMain, "O/S under $2", i++);
            G1.SetColumnPosition(gMain, "Paid Up Policies Refunded", i++);
            G1.SetColumnPosition(gMain, "Charlotte spreadsheet shows reversal", i++);
            G1.SetColumnPosition(gMain, "Other Reversal on Charlotte Spreadsheet", i++);
            G1.SetColumnPosition(gMain, "IBA/Unapplied Cash paid out at death claim", i++);
            G1.SetColumnPosition(gMain, "Change in status from Terminated to Reinstated cash value reapplied at reinstatement on lapsed policies", i++);
            G1.SetColumnPosition(gMain, "Diff in cash value in lapsed policy", i++);
            G1.SetColumnPosition(gMain, "Reconciling", i++);
            G1.SetColumnPosition(gMain, "explanaition", i++);
            G1.SetColumnPosition(gMain, "Date Claim Processed", i++);
        }
        /***********************************************************************************************/
        private void SetupCancelledPolicies(DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gMain = null)
        {
            ClearAllPositions(gMain);
            int i = 1;
            G1.SetColumnPosition(gMain, "Num", i++);
            G1.SetColumnPosition(gMain, "contractNumber", i++);
            G1.SetColumnPosition(gMain, "FH Name", i++);
            G1.SetColumnPosition(gMain, "Policy Extract_Policy Status", i++);
            G1.SetColumnPosition(gMain, "Policy Extract_Status Reason", i++);
            G1.SetColumnPosition(gMain, "Policy Number", i++);
            G1.SetColumnPosition(gMain, "Insured Last Name", i++);
            G1.SetColumnPosition(gMain, "Insured First Name", i++);
            G1.SetColumnPosition(gMain, "Death Benefit", i++);
            G1.SetColumnPosition(gMain, "Face Amount", i++);
            G1.SetColumnPosition(gMain, "Prior Premiums Paid", i++);
            G1.SetColumnPosition(gMain, "Prior Unapplied Cash", i++);
            G1.SetColumnPosition(gMain, "Prior IBA", i++);
            G1.SetColumnPosition(gMain, "Prior Cash Received", i++);
            G1.SetColumnPosition(gMain, "Current Monthly Premium", i++);
            G1.SetColumnPosition(gMain, "Current New Business", i++);
            G1.SetColumnPosition(gMain, "Policy Extract_Premiums Paid", i++);
            G1.SetColumnPosition(gMain, "Current Unapplied Cash", i++);
            G1.SetColumnPosition(gMain, "Current IBA", i++);
            G1.SetColumnPosition(gMain, "Current Cash Received", i++);
            G1.SetColumnPosition(gMain, "Balancing", i++);
            G1.SetColumnPosition(gMain, "O/S under $2", i++);
            G1.SetColumnPosition(gMain, "Paid Up Policies Refunded", i++);
            G1.SetColumnPosition(gMain, "Charlotte spreadsheet shows reversal", i++);
            G1.SetColumnPosition(gMain, "Other Reversal on Charlotte Spreadsheet", i++);
            G1.SetColumnPosition(gMain, "IBA/Unapplied Cash paid out at death claim", i++);
            G1.SetColumnPosition(gMain, "Change in status from Terminated to Reinstated cash value reapplied at reinstatement on lapsed policies", i++);
            G1.SetColumnPosition(gMain, "Diff in cash value in lapsed policy", i++);
            G1.SetColumnPosition(gMain, "Reconciling", i++);
            G1.SetColumnPosition(gMain, "explanaition", i++);
            G1.SetColumnPosition(gMain, "Date Claim Processed", i++);
        }
        /***********************************************************************************************/
        private void SetupPBUnityAC(DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gMain = null)
        {
            ClearAllPositions(gMain);
            int i = 1;
            G1.SetColumnPosition(gMain, "Num", i++);
            G1.SetColumnPosition(gMain, "contractNumber", i++);
            G1.SetColumnPosition(gMain, "FH Name", i++);
            G1.SetColumnPosition(gMain, "Policy Extract_Policy Status", i++);
            G1.SetColumnPosition(gMain, "Policy Number", i++);
            G1.SetColumnPosition(gMain, "Insured Last Name", i++);
            G1.SetColumnPosition(gMain, "Insured First Name", i++);
            G1.SetColumnPosition(gMain, "Death Benefit", i++);
            G1.SetColumnPosition(gMain, "Face Amount", i++);
        }
        /***********************************************************************************************/
        private void SetupPBUnityDEC(DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gMain = null)
        {
            ClearAllPositions(gMain);
            int i = 1;
            G1.SetColumnPosition(gMain, "Num", i++);
            G1.SetColumnPosition(gMain, "contractNumber", i++);
            G1.SetColumnPosition(gMain, "FH Name", i++);
            G1.SetColumnPosition(gMain, "Policy Extract_Policy Status", i++);
            G1.SetColumnPosition(gMain, "Policy Extract_Status Reason", i++);
            G1.SetColumnPosition(gMain, "Policy Number", i++);
            G1.SetColumnPosition(gMain, "Insured Last Name", i++);
            G1.SetColumnPosition(gMain, "Insured First Name", i++);
            G1.SetColumnPosition(gMain, "Death Benefit", i++);
            G1.SetColumnPosition(gMain, "Face Amount", i++);
        }
        /***********************************************************************************************/
        private void SetupBarham(DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gMain = null)
        {
            ClearAllPositions(gMain);
            int i = 1;
            G1.SetColumnPosition(gMain, "Num", i++);
            G1.SetColumnPosition(gMain, "contractNumber", i++);
            G1.SetColumnPosition(gMain, "FH Name", i++);
            G1.SetColumnPosition(gMain, "Policy Extract_Policy Status", i++);
            G1.SetColumnPosition(gMain, "Policy Number", i++);
            G1.SetColumnPosition(gMain, "Insured Last Name", i++);
            G1.SetColumnPosition(gMain, "Insured First Name", i++);
            G1.SetColumnPosition(gMain, "Death Benefit", i++);
            G1.SetColumnPosition(gMain, "Face Amount", i++);
        }
        /***********************************************************************************************/
        private void SetupWebb(DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gMain = null)
        {
            ClearAllPositions(gMain);
            int i = 1;
            G1.SetColumnPosition(gMain, "Num", i++);
            G1.SetColumnPosition(gMain, "contractNumber", i++);
            G1.SetColumnPosition(gMain, "FH Name", i++);
            G1.SetColumnPosition(gMain, "Policy Extract_Policy Status", i++);
            G1.SetColumnPosition(gMain, "Policy Number", i++);
            G1.SetColumnPosition(gMain, "Insured Last Name", i++);
            G1.SetColumnPosition(gMain, "Insured First Name", i++);
            G1.SetColumnPosition(gMain, "Death Benefit", i++);
            G1.SetColumnPosition(gMain, "Face Amount", i++);
        }
        /***********************************************************************************************/
        private void SetupBarhamWebbDEC(DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gMain = null)
        {
            ClearAllPositions(gMain);
            int i = 1;
            G1.SetColumnPosition(gMain, "Num", i++);
            G1.SetColumnPosition(gMain, "contractNumber", i++);
            G1.SetColumnPosition(gMain, "FH Name", i++);
            G1.SetColumnPosition(gMain, "Policy Extract_Policy Status", i++);
            G1.SetColumnPosition(gMain, "Policy Extract_Status Reason", i++);
            G1.SetColumnPosition(gMain, "Policy Number", i++);
            G1.SetColumnPosition(gMain, "Insured Last Name", i++);
            G1.SetColumnPosition(gMain, "Insured First Name", i++);
            G1.SetColumnPosition(gMain, "Death Benefit", i++);
            G1.SetColumnPosition(gMain, "Face Amount", i++);
        }
        /***********************************************************************************************/
        private void SetupPBDirectIssue(DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gMain = null)
        {
            ClearAllPositions(gMain);
            int i = 1;
            G1.SetColumnPosition(gMain, "Num", i++);
            G1.SetColumnPosition(gMain, "contractNumber", i++);
            G1.SetColumnPosition(gMain, "FH Name", i++);
            G1.SetColumnPosition(gMain, "Policy Extract_Policy Status", i++);
            G1.SetColumnPosition(gMain, "Policy Extract_Status Reason", i++);
            G1.SetColumnPosition(gMain, "Policy Number", i++);
            G1.SetColumnPosition(gMain, "Insured Last Name", i++);
            G1.SetColumnPosition(gMain, "Insured First Name", i++);
            G1.SetColumnPosition(gMain, "Death Benefit", i++);
            G1.SetColumnPosition(gMain, "Face Amount", i++);
        }
        /***********************************************************************************************/
        private void btnPrintAll_Click(object sender, EventArgs e)
        {
            //DataTable dt = (DataTable)dgv2.DataSource;

            //int[] rows = gridMain2.GetSelectedRows();
            //int lastRow = dt.Rows.Count;
            //if (rows.Length > 0)
            //    lastRow = rows.Length;

            //string empno = "";
            //string name = "";
            //string userName = "";
            //int row = 0;
            //DataRow dr = null;

            //iTextSharp.text.Document sourceDocument = null;
            //PdfCopy pdfCopyProvider = null;
            //PdfImportedPage importedPage;
            //string outputPdfPath = @"C:/rag/pdfAllTime.pdf";
            //string timeFile = @"C:/rag/pdfTime.pdf";
            //string contractFile = @"C:/rag/pdfContract.pdf";
            //string otherFile = @"C:/rag/pdfOther.pdf";

            //try
            //{
            //    if (File.Exists(outputPdfPath))
            //    {
            //        File.SetAttributes(outputPdfPath, FileAttributes.Normal);
            //        File.Delete(outputPdfPath);
            //    }
            //    if (File.Exists(timeFile))
            //    {
            //        File.SetAttributes(timeFile, FileAttributes.Normal);
            //        File.Delete(timeFile);
            //    }
            //    if (File.Exists(contractFile))
            //    {
            //        File.SetAttributes(contractFile, FileAttributes.Normal);
            //        File.Delete(contractFile);
            //    }
            //    if (File.Exists(otherFile))
            //    {
            //        File.SetAttributes(otherFile, FileAttributes.Normal);
            //        File.Delete(otherFile);
            //    }
            //}
            //catch (Exception ex)
            //{
            //}

            //sourceDocument = new iTextSharp.text.Document();
            //pdfCopyProvider = new PdfCopy(sourceDocument, new System.IO.FileStream(outputPdfPath, System.IO.FileMode.Create));

            ////output file Open  
            //sourceDocument.Open();

            //for (int i = 0; i < lastRow; i++)
            //{
            //    Application.DoEvents();

            //    row = rows[i];
            //    row = gridMain2.GetDataSourceRowIndex(row);

            //    dr = dt.Rows[row];

            //    empno = dr["record"].ObjToString();
            //    name = dr["firstName"].ObjToString() + " " + dr["lastName"].ObjToString();
            //    userName = dr["userName"].ObjToString();

            //    DateTime startTime = this.dateTimePicker1.Value;
            //    DateTime stopTime = this.dateTimePicker2.Value;


            //    using (TimeClock timeForm = new TimeClock(startTime, stopTime, empno, userName, name, true))
            //    {
            //        this.Cursor = Cursors.WaitCursor;
            //        try
            //        {
            //            timeForm.ShowDialog();
            //        }
            //        catch (Exception ex)
            //        {
            //        }

            //        try
            //        {
            //            MergeAllPDF(pdfCopyProvider, timeFile, contractFile, otherFile );
            //            File.SetAttributes(outputPdfPath, FileAttributes.Normal);
            //        }
            //        catch (Exception ex)
            //        {
            //        }

            //        if (File.Exists(timeFile))
            //        {
            //            File.SetAttributes(timeFile, FileAttributes.Normal);
            //            File.Delete(timeFile);
            //        }

            //        if (File.Exists(contractFile))
            //        {
            //            File.SetAttributes(contractFile, FileAttributes.Normal);
            //            File.Delete(contractFile);
            //        }
            //        if (File.Exists(otherFile))
            //        {
            //            File.SetAttributes(otherFile, FileAttributes.Normal);
            //            File.Delete(otherFile);
            //        }
            //        this.Cursor = Cursors.Default;
            //    }
            //}
            //sourceDocument.Close();

            //ViewPDF myView = new ViewPDF("SMFS Employee Timesheets", outputPdfPath);
            //myView.ShowDialog();
        }
        /***********************************************************************************************/
        private static int TotalPageCount(string file)
        {
            using (StreamReader sr = new StreamReader(System.IO.File.OpenRead(file)))
            {
                Regex regex = new Regex(@"/Type\s*/Page[^s]");
                MatchCollection matches = regex.Matches(sr.ReadToEnd());

                return matches.Count;
            }
        }
        /***********************************************************************************************/
        private static void MergeAllPDF(PdfCopy pdfCopyProvider, string File1, string File2, string File3)
        {
            string[] fileArray = new string[4];
            fileArray[0] = File1;
            fileArray[1] = File2;
            fileArray[2] = File3;

            PdfReader reader = null;
            PdfImportedPage importedPage;


            //files list wise Loop  
            try
            {
                for (int f = 0; f < fileArray.Length - 1; f++)
                {
                    try
                    {
                        if (!File.Exists(fileArray[f]))
                            continue;
                        int pages = TotalPageCount(fileArray[f]);

                        reader = new PdfReader(fileArray[f]);
                        //Add pages in new file  
                        for (int i = 1; i <= pages; i++)
                        {
                            try
                            {
                                importedPage = pdfCopyProvider.GetImportedPage(reader, i);
                                pdfCopyProvider.AddPage(importedPage);
                            }
                            catch (Exception ex)
                            {
                            }
                        }

                        reader.Close();
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
        /***********************************************************************************************/
        private void printEmployeeDetailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }
        /***********************************************************************************************/
        //private bool showRates = false;
        private void dgv_ProcessGridKey(object sender, KeyEventArgs e)
        {
            SetControlKey(sender, e);
        }
        /***********************************************************************************************/
        private void SetControlKey(object sender, KeyEventArgs e)
        {
            if (loading)
                return;
            //if (e.Control && e.KeyCode == Keys.R)
            //{
            //    loading = true;
            //    if (showRates)
            //        showRates = false;
            //    else
            //        showRates = true;
            //    if (dgv2.Visible)
            //    {
            //        gridMain2.FocusedColumn = gridMain2.Columns["empStatus"];
            //        gridMain2.RefreshData();
            //        gridMain2.FocusedColumn = gridMain2.Columns["empStatus"];
            //        gridMain2.RefreshData();
            //        //if (!showRates)
            //        //    gridMain2.OptionsView.ShowFooter = false;
            //        //else
            //        //    gridMain2.OptionsView.ShowFooter = true;
            //        gridMain2.FocusedColumn = gridMain2.Columns["empStatus"];
            //        gridMain2.RefreshData();
            //    }
            //    else
            //    {
            //        gridMain.RefreshData();
            //        dgv.Refresh();
            //        int rowHandle = gridMain.FocusedRowHandle;
            //        gridMain.SelectRow(rowHandle);
            //        gridMain.FocusedRowHandle = rowHandle;
            //        gridMain.FocusedColumn = gridMain.Columns["lastName"];
            //    }
            //    loading = false;
            //}
        }
        /***********************************************************************************************/
        private void gridMain2_CustomDrawCell(object sender, RowCellCustomDrawEventArgs e)
        {
            GridView view = sender as GridView;
            DataTable dt = (DataTable)(dgv.DataSource);
            int row = e.RowHandle;
            if (e.Column.FieldName.ToUpper() == "NUM")
                e.DisplayText = (row + 1).ToString();
        }
        /***********************************************************************************************/
        private void gridMain2_CustomColumnDisplayText(object sender, CustomColumnDisplayTextEventArgs e)
        {
            GridView view = sender as GridView;
            if (e.ListSourceRowIndex == DevExpress.XtraGrid.GridControl.InvalidRowHandle)
                return;

            bool doit = false;
            string column = e.Column.FieldName.ToUpper();
        }
        /***********************************************************************************************/
        private void gridMain_CustomRowFilter(object sender, RowFilterEventArgs e)
        {
        }
        /****************************************************************************************/
        private double originalSize = 0D;
        private Font mainFont = null;
        private Font newFont = null;
        private void ScaleCells()
        {
            //if (originalSize == 0D)
            //{
            //    //                originalSize = gridMain.Columns["address1"].AppearanceCell.FontSizeDelta.ObjToDouble();
            //    originalSize = gridMain2.Columns["lastName"].AppearanceCell.Font.Size;
            //    mainFont = gridMain2.Columns["lastName"].AppearanceCell.Font;
            //}
            //double scale = txtScale.Text.ObjToDouble();
            //double size = scale / 100D * originalSize;
            //Font font = new Font(mainFont.Name, (float)size);
            //for (int i = 0; i < gridMain2.Columns.Count; i++)
            //{
            //    gridMain2.Columns[i].AppearanceCell.Font = font;
            //    gridMain2.Columns[i].AppearanceHeader.Font = font;
            //}
            //gridMain2.Appearance.GroupFooter.Font = font;
            //gridMain2.AppearancePrint.FooterPanel.Font = font;
            //gridMain2.Appearance.FocusedRow.Font = font;
            //newFont = font;
            //gridMain2.RefreshData();
            //gridMain2.RefreshEditor(true);
            //dgv2.Refresh();
            //this.Refresh();
        }
        /****************************************************************************************/
        private void txtScale_KeyUp(object sender, KeyEventArgs e)
        {
        }
        /***********************************************************************************************/
        private void pictureBox3_Click(object sender, EventArgs e)
        {
            //G1.SpyGlass(gridView6);
        }
        /***********************************************************************************************/
        private void btnImportData_Click(object sender, EventArgs e)
        {
            Import importForm = new Import("Import Unity File", false, true);
            importForm.SelectDone += ImportForm_SelectDone;
            importForm.Show();
        }
        /***********************************************************************************************/
        private void ImportForm_SelectDone(DataTable dt)
        {
            string name = dt.TableName.Trim();
            name = name.Replace(".csv", "");
            name = name.Replace(".xlsx", "");
            this.Text = name;

            actualFile = name;

            importMonth = DetermineMonth();
            if (String.IsNullOrWhiteSpace(importMonth))
            {
                MessageBox.Show("*** ERROR *** I'm having trouble determing the Month from the Filename!\nIt should be the first word!", "Unity Filename Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }
            importYear = DetermineYear();
            if (String.IsNullOrWhiteSpace(importYear))
            {
                MessageBox.Show("*** ERROR *** I'm having trouble determing the Year from the Filename!\nIt should be the second word!", "Unity Filename Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }

            dt = PreprocessData(dt);

            if (G1.get_column_number(dt, "num") < 0)
                dt.Columns.Add("num", typeof(string)).SetOrdinal(0);
            if (G1.get_column_number(dt, "status") < 0)
                dt.Columns.Add("status");
            if (G1.get_column_number(dt, "insuredName") < 0)
                dt.Columns.Add("insuredName");
            if (G1.get_column_number(dt, "tab") < 0)
                dt.Columns.Add("tab");



            dt = SortBy(dt);
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;

            btnLoadOthers.Show();
            btnLoadOthers.Refresh();

            chkExcludeHeader.Show();
            chkExcludeHeader.Refresh();

            //chkHonorPrevious.Show();
            //chkHonorPrevious.Refresh();
        }
        /***********************************************************************************************/
        private DataTable PreprocessData(DataTable dt)
        {
            string cName1 = dt.Columns[1].ColumnName.Trim();
            string cName2 = dt.Columns[2].ColumnName.Trim();

            string c1 = "";
            string c2 = "";

            for (int i = (dt.Rows.Count - 1); i >= 0; i--)
            {
                c1 = dt.Rows[i][1].ObjToString();
                if (String.IsNullOrWhiteSpace(c1))
                {
                    dt.Rows.RemoveAt(i);
                    continue;
                }
                c2 = dt.Rows[i][2].ObjToString();
                if (c1 == cName1 && c2 == cName2)
                    dt.Rows.RemoveAt(i);
            }
            return dt;
        }
        /***********************************************************************************************/
        private DataTable RemoveFromMain(DataTable bDt, DataRow[] rDt)
        {
            int i = 0;
            try
            {
                for (i = (rDt.Length - 1); i >= 0; i--)
                    bDt.Rows.Remove(rDt[i]);
            }
            catch (Exception ex)
            {
            }
            return bDt;
        }
        /***********************************************************************************************/
        private double GetFaceAmount(DataTable dt)
        {
            double faceAmount = 0D;
            for (int i = 0; i < dt.Rows.Count; i++)
                faceAmount += dt.Rows[i]["Face Amount"].ObjToDouble();
            return faceAmount;
        }
        /***********************************************************************************************/
        private DataTable reportDt = null;
        private void AddReportCount(string title, int count, double faceAmount)
        {
            DataRow dr = reportDt.NewRow();
            dr["Tab"] = title;
            dr["Count"] = count;
            dr["faceAmount"] = faceAmount;
            reportDt.Rows.Add(dr);
        }
        /***********************************************************************************************/
        private void btnLoadOthers_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            HideOrShowTabs(true);

            DataTable bDt = (DataTable)dgv.DataSource;

            DataTable dt = bDt.Copy();

            DataTable dx = dt.Clone();
            DataTable backupDt = dt.Clone();

            DataRow[] dRows = null;

            reportDt = new DataTable();
            reportDt.Columns.Add("Tab");
            reportDt.Columns.Add("totalRows", Type.GetType("System.Int32"));
            reportDt.Columns.Add("Count", Type.GetType("System.Int32"));

            reportDt.Columns.Add("totalFaceAmount", Type.GetType("System.Double"));
            reportDt.Columns.Add("faceAmount", Type.GetType("System.Double"));

            DataRow dr = reportDt.NewRow();
            dr["Tab"] = "All Policies";
            dr["totalRows"] = dt.Rows.Count;
            dr["totalFaceAmount"] = GetFaceAmount(dt);
            dr["faceAmount"] = 0D;
            reportDt.Rows.Add(dr);

            double faceAmount = 0D;
            string str = "";
            bool isExcel = false;
            if (importedFile.ToUpper().IndexOf(".XLSX") > 0)
            {
                isExcel = true;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    str = dt.Rows[i]["Death Benefit"].ObjToString().Trim();
                    if (str == "0")
                        dt.Rows[i]["Death Benefit"] = "0.00";
                }
            }

            try
            {
                dRows = dt.Select("`Death Benefit` <> '0.00' AND `Policy Extract_Policy Status` = 'A' AND `Policy Number` LIKE '77%'");
                if (dRows.Length > 0)
                {
                    dx = dRows.CopyToDataTable();
                    dx = SortBy(dx);
                    G1.NumberDataTable(dx);
                    dgv2.DataSource = dx;

                    faceAmount = GetFaceAmount(dx);

                    AddReportCount(tabControl1.TabPages[1].Text, dx.Rows.Count, faceAmount);

                    dt = RemoveFromMain(dt, dRows);
                }
                else
                    dgv2.DataSource = backupDt;
            }
            catch (Exception ex)
            {
            }

            try
            {
                dRows = dt.Select("`Death Benefit` = '0.00' AND `Policy Extract_Policy Status` = 'T' AND `Policy Number` LIKE '77%' AND `Policy Extract_Status Reason` IN ('LP','NI','NN','NT','SR')");
                if (dRows.Length > 0)
                {
                    dx = dRows.CopyToDataTable();
                    dx = SortBy(dx);
                    G1.NumberDataTable(dx);
                    dgv3.DataSource = dx;

                    faceAmount = GetFaceAmount(dx);

                    AddReportCount(tabControl1.TabPages[2].Text, dx.Rows.Count, faceAmount);

                    dt = RemoveFromMain(dt, dRows);
                }
                else
                    dgv3.DataSource = backupDt;
            }
            catch (Exception ex)
            {
            }

            try
            {
                dRows = dt.Select("`Death Benefit` = '0.00' AND `Policy Extract_Policy Status` = 'S' AND `Policy Number` LIKE '77%' AND `Policy Extract_Status Reason` = 'AN'");
                if (dRows.Length > 0)
                {
                    dx = dRows.CopyToDataTable();
                    dx = SortBy(dx);
                    G1.NumberDataTable(dx);
                    dgv4.DataSource = dx;

                    faceAmount = GetFaceAmount(dx);

                    AddReportCount(tabControl1.TabPages[3].Text, dx.Rows.Count, faceAmount);

                    dt = RemoveFromMain(dt, dRows);
                }
                else
                    dgv4.DataSource = backupDt;
            }
            catch (Exception ex)
            {
            }

            try
            {
                dRows = dt.Select("`Death Benefit` = '0.00' AND `Policy Extract_Policy Status` = 'T' AND `Policy Number` LIKE '77%' AND `Policy Extract_Status Reason` = 'DC'");
                if (dRows.Length > 0)
                {
                    dx = dRows.CopyToDataTable();
                    dx = SortBy(dx);
                    G1.NumberDataTable(dx);
                    dgv5.DataSource = dx;

                    faceAmount = GetFaceAmount(dx);

                    AddReportCount(tabControl1.TabPages[4].Text, dx.Rows.Count, faceAmount);

                    dt = RemoveFromMain(dt, dRows);
                }
                else
                    dgv5.DataSource = backupDt;
            }
            catch (Exception ex)
            {
            }

            try
            {
                dRows = dt.Select("`Death Benefit` = '0.00' AND `Policy Extract_Policy Status` = 'T' AND `Policy Number` LIKE '77%' AND `Policy Extract_Status Reason` = 'CA'");
                if (dRows.Length > 0)
                {
                    dx = dRows.CopyToDataTable();
                    dx = SortBy(dx);
                    G1.NumberDataTable(dx);
                    dgv6.DataSource = dx;

                    faceAmount = GetFaceAmount(dx);

                    AddReportCount(tabControl1.TabPages[5].Text, dx.Rows.Count, faceAmount);

                    dt = RemoveFromMain(dt, dRows);
                }
                else
                    dgv6.DataSource = backupDt;
            }
            catch (Exception ex)
            {
            }

            try
            {
                dRows = dt.Select("`Death Benefit` <> '0.00' AND `Policy Extract_Policy Status` = 'A' AND `Policy Number` LIKE 'P%' AND `Policy Number` NOT LIKE 'PB%' AND `Policy Number` NOT LIKE 'PSPNB%' AND `Policy Number` NOT LIKE 'PSPWT%'");
                if (dRows.Length > 0)
                {
                    dx = dRows.CopyToDataTable();
                    dx = SortBy(dx);
                    G1.NumberDataTable(dx);
                    dgv7.DataSource = dx;

                    faceAmount = GetFaceAmount(dx);

                    AddReportCount(tabControl1.TabPages[6].Text, dx.Rows.Count, faceAmount);

                    dt = RemoveFromMain(dt, dRows);
                }
                else
                    dgv7.DataSource = backupDt;
            }
            catch (Exception ex)
            {
            }

            try
            {
                dRows = dt.Select("`Death Benefit` = '0.00' AND `Policy Extract_Policy Status` = 'T' AND `Policy Number` LIKE 'P%' AND `Policy Number` NOT LIKE 'PB%' AND `Policy Number` NOT LIKE 'PSPNB%' AND `Policy Number` NOT LIKE 'PSPWT%' AND `Policy Extract_Status Reason` = 'DC'");
                if (dRows.Length > 0)
                {
                    dx = dRows.CopyToDataTable();
                    dx = SortBy(dx);
                    G1.NumberDataTable(dx);
                    dgv8.DataSource = dx;

                    faceAmount = GetFaceAmount(dx);

                    AddReportCount(tabControl1.TabPages[7].Text, dx.Rows.Count, faceAmount);

                    dt = RemoveFromMain(dt, dRows);
                }
                else
                    dgv8.DataSource = backupDt;
            }
            catch (Exception ex)
            {
            }

            try
            {
                dRows = dt.Select("( `Death Benefit` <> '0.00' AND `Policy Extract_Policy Status` = 'A' AND `Policy Number` LIKE 'PSPNB%' ) OR `Policy Number` = 'PSPNB08002'");
                if (dRows.Length > 0)
                {
                    dx = dRows.CopyToDataTable();
                    dx = SortBy(dx);
                    G1.NumberDataTable(dx);
                    dgv9.DataSource = dx;

                    faceAmount = GetFaceAmount(dx);

                    AddReportCount(tabControl1.TabPages[8].Text, dx.Rows.Count, faceAmount);

                    dt = RemoveFromMain(dt, dRows);
                }
                else
                    dgv9.DataSource = backupDt;
            }
            catch (Exception ex)
            {
            }

            try
            {
                dRows = dt.Select("`Death Benefit` <> '0.00' AND `Policy Extract_Policy Status` = 'A' AND `Policy Number` LIKE 'PSPWT%'");
                if (dRows.Length > 0)
                {
                    dx = dRows.CopyToDataTable();
                    dx = SortBy(dx);
                    G1.NumberDataTable(dx);
                    dgv10.DataSource = dx;

                    faceAmount = GetFaceAmount(dx);

                    AddReportCount(tabControl1.TabPages[9].Text, dx.Rows.Count, faceAmount);

                    dt = RemoveFromMain(dt, dRows);
                }
                else
                    dgv10.DataSource = backupDt;
            }
            catch (Exception ex)
            {
            }

            try
            {
                dRows = dt.Select("`Death Benefit` = '0.00' AND `Policy Extract_Policy Status` = 'T' AND ( `Policy Number` LIKE 'PSPNB%' OR `Policy Number` LIKE 'PSPWT%' ) AND `Policy Extract_Status Reason` = 'DC'");
                if (dRows.Length > 0)
                {
                    dx = dRows.CopyToDataTable();
                    dx = SortBy(dx);
                    G1.NumberDataTable(dx);
                    dgv11.DataSource = dx;

                    faceAmount = GetFaceAmount(dx);

                    AddReportCount(tabControl1.TabPages[10].Text, dx.Rows.Count, faceAmount);

                    dt = RemoveFromMain(dt, dRows);
                }
                else
                    dgv11.DataSource = backupDt;
            }
            catch (Exception ex)
            {
            }

            try
            {
                dRows = dt.Select("`Policy Number` LIKE 'PB%'");
                if (dRows.Length > 0)
                {
                    dx = dRows.CopyToDataTable();
                    dx = SortBy(dx);
                    G1.NumberDataTable(dx);
                    dgv12.DataSource = dx;

                    faceAmount = GetFaceAmount(dx);

                    AddReportCount(tabControl1.TabPages[11].Text, dx.Rows.Count, faceAmount);

                    dt = RemoveFromMain(dt, dRows);
                }
                else
                    dgv12.DataSource = backupDt;
            }
            catch (Exception ex)
            {
            }

            if (dt.Rows.Count > 0)
            {
                dt = SortBy(dt);
                G1.NumberDataTable(dt);
            }
            dgv13.DataSource = dt;

            faceAmount = GetFaceAmount(dt);

            AddReportCount(tabControl1.TabPages[12].Text, dt.Rows.Count, faceAmount);

            G1.NumberDataTable(reportDt);
            dgv14.DataSource = reportDt;

            SetupTotalsSummary();

            BuildContextMenu();

            btnExportToExcel.Show();
            btnExportToExcel.Refresh();

            btnSave.Show();
            btnSave.Refresh();

            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private DataTable SortBy(DataTable dx)
        {
            try
            {
                DataView tempview = dx.DefaultView;
                tempview.Sort = "Insured Last Name, Insured First Name";
                dx = tempview.ToTable();
            }
            catch (Exception ex)
            {
            }
            return dx;
        }
        /****************************************************************************************/
        private void SetupTotalsSummary()
        {
            SetGridTotal("Death Benefit", gridMain);
            SetGridTotal("Face Amount", gridMain);
            SetGridTotal("Prior Premiums Paid", gridMain);
            SetGridTotal("Prior Unapplied Cash", gridMain);
            SetGridTotal("Prior IBA", gridMain);
            SetGridTotal("Prior Cash Received", gridMain);
            SetGridTotal("Current Monthly Premium", gridMain);
            SetGridTotal("Current New Business", gridMain);
            SetGridTotal("Policy Extract_Premiums Paid", gridMain);
            SetGridTotal("Current Unapplied Cash", gridMain);
            SetGridTotal("Current IBA", gridMain);
            SetGridTotal("Current Cash Received", gridMain);
            SetGridTotal("Balancing", gridMain);

            SetGridTotal("Death Benefit", gridMain2);
            SetGridTotal("Face Amount", gridMain2);
            SetGridTotal("Prior Premiums Paid", gridMain2);
            SetGridTotal("Prior Unapplied Cash", gridMain2);
            SetGridTotal("Prior IBA", gridMain2);
            SetGridTotal("Prior Cash Received", gridMain2);
            SetGridTotal("Current Monthly Premium", gridMain2);
            SetGridTotal("Current New Business", gridMain2);
            SetGridTotal("Policy Extract_Premiums Paid", gridMain2);
            SetGridTotal("Current Unapplied Cash", gridMain2);
            SetGridTotal("Current IBA", gridMain2);
            SetGridTotal("Current Cash Received", gridMain2);
            SetGridTotal("Balancing", gridMain2);

            SetGridTotal("Death Benefit", gridMain3);
            SetGridTotal("Face Amount", gridMain3);
            SetGridTotal("Prior Premiums Paid", gridMain3);
            SetGridTotal("Prior Unapplied Cash", gridMain3);
            SetGridTotal("Prior IBA", gridMain3);
            SetGridTotal("Prior Cash Received", gridMain3);
            SetGridTotal("Current Monthly Premium", gridMain3);
            SetGridTotal("Current New Business", gridMain3);
            SetGridTotal("Policy Extract_Premiums Paid", gridMain3);
            SetGridTotal("Current Unapplied Cash", gridMain3);
            SetGridTotal("Current IBA", gridMain3);
            SetGridTotal("Current Cash Received", gridMain3);
            SetGridTotal("Balancing", gridMain3);

            SetGridTotal("Death Benefit", gridMain4);
            SetGridTotal("Face Amount", gridMain4);
            SetGridTotal("Prior Premiums Paid", gridMain4);
            SetGridTotal("Prior Unapplied Cash", gridMain4);
            SetGridTotal("Prior IBA", gridMain4);
            SetGridTotal("Prior Cash Received", gridMain4);
            SetGridTotal("Current Monthly Premium", gridMain4);
            SetGridTotal("Current New Business", gridMain4);
            SetGridTotal("Policy Extract_Premiums Paid", gridMain4);
            SetGridTotal("Current Unapplied Cash", gridMain4);
            SetGridTotal("Current IBA", gridMain4);
            SetGridTotal("Current Cash Received", gridMain4);
            SetGridTotal("Balancing", gridMain4);

            SetGridTotal("Death Benefit", gridMain5);
            SetGridTotal("Face Amount", gridMain5);
            SetGridTotal("Prior Premiums Paid", gridMain5);
            SetGridTotal("Prior Unapplied Cash", gridMain5);
            SetGridTotal("Prior IBA", gridMain5);
            SetGridTotal("Prior Cash Received", gridMain5);
            SetGridTotal("Current Monthly Premium", gridMain5);
            SetGridTotal("Current New Business", gridMain5);
            SetGridTotal("Policy Extract_Premiums Paid", gridMain5);
            SetGridTotal("Current Unapplied Cash", gridMain5);
            SetGridTotal("Current IBA", gridMain5);
            SetGridTotal("Current Cash Received", gridMain5);
            SetGridTotal("Balancing", gridMain5);

            SetGridTotal("Death Benefit", gridMain6);
            SetGridTotal("Face Amount", gridMain6);
            SetGridTotal("Prior Premiums Paid", gridMain6);
            SetGridTotal("Prior Unapplied Cash", gridMain6);
            SetGridTotal("Prior IBA", gridMain6);
            SetGridTotal("Prior Cash Received", gridMain6);
            SetGridTotal("Current Monthly Premium", gridMain6);
            SetGridTotal("Current New Business", gridMain6);
            SetGridTotal("Policy Extract_Premiums Paid", gridMain6);
            SetGridTotal("Current Unapplied Cash", gridMain6);
            SetGridTotal("Current IBA", gridMain6);
            SetGridTotal("Current Cash Received", gridMain6);
            SetGridTotal("Balancing", gridMain6);

            SetGridTotal("Death Benefit", gridMain7);
            SetGridTotal("Face Amount", gridMain7);
            SetGridTotal("Prior Premiums Paid", gridMain7);
            SetGridTotal("Prior Unapplied Cash", gridMain7);
            SetGridTotal("Prior IBA", gridMain7);
            SetGridTotal("Prior Cash Received", gridMain7);
            SetGridTotal("Current Monthly Premium", gridMain7);
            SetGridTotal("Current New Business", gridMain7);
            SetGridTotal("Policy Extract_Premiums Paid", gridMain7);
            SetGridTotal("Current Unapplied Cash", gridMain7);
            SetGridTotal("Current IBA", gridMain7);
            SetGridTotal("Current Cash Received", gridMain7);
            SetGridTotal("Balancing", gridMain7);

            SetGridTotal("Death Benefit", gridMain8);
            SetGridTotal("Face Amount", gridMain8);
            SetGridTotal("Prior Premiums Paid", gridMain8);
            SetGridTotal("Prior Unapplied Cash", gridMain8);
            SetGridTotal("Prior IBA", gridMain8);
            SetGridTotal("Prior Cash Received", gridMain8);
            SetGridTotal("Current Monthly Premium", gridMain8);
            SetGridTotal("Current New Business", gridMain8);
            SetGridTotal("Policy Extract_Premiums Paid", gridMain8);
            SetGridTotal("Current Unapplied Cash", gridMain8);
            SetGridTotal("Current IBA", gridMain8);
            SetGridTotal("Current Cash Received", gridMain8);
            SetGridTotal("Balancing", gridMain8);

            SetGridTotal("Death Benefit", gridMain9);
            SetGridTotal("Face Amount", gridMain9);
            SetGridTotal("Prior Premiums Paid", gridMain9);
            SetGridTotal("Prior Unapplied Cash", gridMain9);
            SetGridTotal("Prior IBA", gridMain9);
            SetGridTotal("Prior Cash Received", gridMain9);
            SetGridTotal("Current Monthly Premium", gridMain9);
            SetGridTotal("Current New Business", gridMain9);
            SetGridTotal("Policy Extract_Premiums Paid", gridMain9);
            SetGridTotal("Current Unapplied Cash", gridMain9);
            SetGridTotal("Current IBA", gridMain9);
            SetGridTotal("Current Cash Received", gridMain9);
            SetGridTotal("Balancing", gridMain9);

            SetGridTotal("Death Benefit", gridMain10);
            SetGridTotal("Face Amount", gridMain10);
            SetGridTotal("Prior Premiums Paid", gridMain10);
            SetGridTotal("Prior Unapplied Cash", gridMain10);
            SetGridTotal("Prior IBA", gridMain10);
            SetGridTotal("Prior Cash Received", gridMain10);
            SetGridTotal("Current Monthly Premium", gridMain10);
            SetGridTotal("Current New Business", gridMain10);
            SetGridTotal("Policy Extract_Premiums Paid", gridMain10);
            SetGridTotal("Current Unapplied Cash", gridMain10);
            SetGridTotal("Current IBA", gridMain10);
            SetGridTotal("Current Cash Received", gridMain10);
            SetGridTotal("Balancing", gridMain10);

            SetGridTotal("Death Benefit", gridMain11);
            SetGridTotal("Face Amount", gridMain11);
            SetGridTotal("Prior Premiums Paid", gridMain11);
            SetGridTotal("Prior Unapplied Cash", gridMain11);
            SetGridTotal("Prior IBA", gridMain11);
            SetGridTotal("Prior Cash Received", gridMain11);
            SetGridTotal("Current Monthly Premium", gridMain11);
            SetGridTotal("Current New Business", gridMain11);
            SetGridTotal("Policy Extract_Premiums Paid", gridMain11);
            SetGridTotal("Current Unapplied Cash", gridMain11);
            SetGridTotal("Current IBA", gridMain11);
            SetGridTotal("Current Cash Received", gridMain11);
            SetGridTotal("Balancing", gridMain11);

            SetGridTotal("Death Benefit", gridMain12);
            SetGridTotal("Face Amount", gridMain12);
            SetGridTotal("Prior Premiums Paid", gridMain12);
            SetGridTotal("Prior Unapplied Cash", gridMain12);
            SetGridTotal("Prior IBA", gridMain12);
            SetGridTotal("Prior Cash Received", gridMain12);
            SetGridTotal("Current Monthly Premium", gridMain12);
            SetGridTotal("Current New Business", gridMain12);
            SetGridTotal("Policy Extract_Premiums Paid", gridMain12);
            SetGridTotal("Current Unapplied Cash", gridMain12);
            SetGridTotal("Current IBA", gridMain12);
            SetGridTotal("Current Cash Received", gridMain12);
            SetGridTotal("Balancing", gridMain12);

            SetGridTotal("Death Benefit", gridMain13);
            SetGridTotal("Face Amount", gridMain13);
            SetGridTotal("Prior Premiums Paid", gridMain13);
            SetGridTotal("Prior Unapplied Cash", gridMain13);
            SetGridTotal("Prior IBA", gridMain13);
            SetGridTotal("Prior Cash Received", gridMain13);
            SetGridTotal("Current Monthly Premium", gridMain13);
            SetGridTotal("Current New Business", gridMain13);
            SetGridTotal("Policy Extract_Premiums Paid", gridMain13);
            SetGridTotal("Current Unapplied Cash", gridMain13);
            SetGridTotal("Current IBA", gridMain13);
            SetGridTotal("Current Cash Received", gridMain13);
            SetGridTotal("Balancing", gridMain13);

            AddSummaryColumn("Count", gridMain14, "{0}");
            AddSummaryColumn("totalRows", gridMain14, "{0}");
            AddSummaryColumn("faceAmount", gridMain14);
            AddSummaryColumn("totalFaceAmount", gridMain14);

            HideGridColumns(gridMain);
            HideGridColumns(gridMain2);
            HideGridColumns(gridMain3);
            HideGridColumns(gridMain4);
            HideGridColumns(gridMain5);
            HideGridColumns(gridMain6);
            HideGridColumns(gridMain7);
            HideGridColumns(gridMain8);
            HideGridColumns(gridMain9);
            HideGridColumns(gridMain10);
            HideGridColumns(gridMain11);
            HideGridColumns(gridMain12);
            HideGridColumns(gridMain13);
        }
        /***********************************************************************************************/
        private void SetGridTotal(string columnName, DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gMain = null)
        {
            if (gMain == null)
                return;

            if (G1.getGridColumnIndex(gMain, columnName) < 0)
                return;

            gMain.OptionsView.ShowFooter = true;
            AddSummaryColumn(columnName, gMain);
            gMain.Columns[columnName].Width = 100;
        }
        /***********************************************************************************************/
        private void HideGridColumns(DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gMain = null)
        {
            if (gMain == null)
                return;
            if (G1.getGridColumnIndex(gMain, "Policy Number2") >= 0)
                gMain.Columns["Policy Number2"].Visible = false;
            if (G1.getGridColumnIndex(gMain, "Policy Number7") >= 0)
                gMain.Columns["Policy Number7"].Visible = false;
        }
        /***********************************************************************************************/
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        /***********************************************************************************************/
        private void BuildContextMenu()
        {
            ToolStripMenuItem menu = null;
            for (int i = 0; i < contextMenuStrip1.Items.Count; i++)
            {
                menu = (ToolStripMenuItem)contextMenuStrip1.Items[i];
                menu.Dispose();
                menu = null;
            }

            contextMenuStrip1.Items.Clear();

            for (int i = 1; i < tabControl1.TabPages.Count; i++)
            {
                menu = new ToolStripMenuItem();
                menu.Name = tabControl1.TabPages[i].Name;
                menu.Text = "Move to " + tabControl1.TabPages[i].Text;
                menu.Click += Menu_Click;
                contextMenuStrip1.Items.Add(menu);
            }
        }
        /***********************************************************************************************/
        private void Menu_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menu = (ToolStripMenuItem)sender;
            string name = menu.Name;

            int selected = tabControl1.SelectedIndex;
            TabPage currentPage = tabControl1.TabPages[selected];
            string fromName = currentPage.Text;
            fromName = fromName.Replace("Move to ", "");

            TabPage tabPage = tabControl1.TabPages[name];
            string toName = tabPage.Text;
            toName = toName.Replace("Move to ", "");

            DataTable fromDt = null;
            DataTable toDt = null;

            GridControl fromDGV = null;
            GridControl toDGV = null;

            DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView fromGmain = GetGridView(fromName, ref fromDt, ref fromDGV);
            DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView toGmain = GetGridView(toName, ref toDt, ref toDGV);

            if (fromGmain == null)
                return;
            if (toGmain == null)
                return;

            DataRow dr = fromGmain.GetFocusedDataRow();
            int rowHandle = fromGmain.FocusedRowHandle;
            int row = fromGmain.GetDataSourceRowIndex(rowHandle);
            string policyNumber = dr["Policy Number"].ObjToString();

            G1.copy_dt_row(fromDt, row, toDt, toDt.Rows.Count);

            toDt = SortBy(toDt);

            G1.NumberDataTable(toDt);
            toDGV.DataSource = toDt;
            toDGV.RefreshDataSource();
            toDGV.Refresh();
            //toGmain.RefreshData();

            fromDt.Rows.Remove(dr);

            fromGmain.RefreshData();
            G1.NumberDataTable(fromDt);
            fromDGV.DataSource = fromDt;

            DataTable dx = (DataTable)dgv14.DataSource;

            double faceAmount = GetFaceAmount(fromDt);

            DataRow[] dRows = dx.Select("Tab='" + fromName + "'");
            if (dRows.Length > 0)
            {
                dRows[0]["faceAmount"] = faceAmount;
                double count = dRows[0]["Count"].ObjToDouble();
                dRows[0]["Count"] = count - 1D;
            }

            dRows = dx.Select("Tab='" + toName + "'");
            if (dRows.Length > 0)
            {
                faceAmount = GetFaceAmount(toDt);
                dRows[0]["faceAmount"] = faceAmount;
                double count = dRows[0]["Count"].ObjToDouble();
                dRows[0]["Count"] = count + 1D;
            }

            DataTable dt = (DataTable)dgv.DataSource;
            dRows = dt.Select("`Policy Number`='" + policyNumber + "'");
            if ( dRows.Length > 0 )
            {
                dRows[0]["tab"] = toName;
            }

            //string cmd = "Select * from `unity_moves` where `policyNumber` = '" + policyNumber + "';";
            //DataTable ddt = G1.get_db_data(cmd);
            //string record = "";
            //if ( ddt.Rows.Count > 0 )
            //{
            //    record = ddt.Rows[0]["record"].ObjToString();
            //    G1.delete_db_table("unity_moves", "record", record);
            //}

            //record = G1.create_record("unity_moves", "toTab", "-1");
            //if (G1.BadRecord("unity_moves", record))
            //    return;
            //G1.update_db_table("unity_moves", "record", record, new string[] { "policyNumber", policyNumber, "fromTab", fromName, "toTab", toName });
        }
        /***********************************************************************************************/
        private void HonorPreviousMoves()
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt.Rows.Count <= 0)
                return;

            DataRow[] dRows = null;
            DataRow[] drs = null;

            string cmd = "Select * from `unity_moves`;";
            DataTable ddt = G1.get_db_data(cmd);
            if (ddt.Rows.Count <= 0)
                return;
            string policyNumber = "";
            string fromName = "";
            string toName = "";
            string record = "";

            DataTable fromDt = null;
            GridControl fromDGV = null;
            AdvBandedGridView fromGmain = null;
            DataTable toDt = null;
            GridControl toDGV = null;
            AdvBandedGridView toGmain = null;


            for (int i = 0; i < ddt.Rows.Count; i++)
            {
                record = ddt.Rows[i]["record"].ObjToString();
                policyNumber = ddt.Rows[i]["policyNumber"].ObjToString();
                fromName = ddt.Rows[i]["fromTab"].ObjToString();
                toName = ddt.Rows[i]["toTab"].ObjToString();

                fromGmain = GetGridView(fromName, ref fromDt, ref fromDGV);
                toGmain = GetGridView(toName, ref toDt, ref toDGV);


                try
                {
                    dRows = fromDt.Select("`Policy Number`='" + policyNumber + "'");
                }
                catch (Exception ex)
                {
                }
                if (dRows.Length > 0)
                {
                    DataRow dr = dRows[0];
                    int rowHandle = fromDt.Rows.IndexOf(dr);
                    int row = fromGmain.GetRowHandle(rowHandle);
                    //int rowHandle = fromGmain.FocusedRowHandle;
                    //int row = fromGmain.GetDataSourceRowIndex(rowHandle);
                    //string policyNumber = dr["Policy Number"].ObjToString();

                    G1.copy_dt_row(fromDt, row, toDt, toDt.Rows.Count);

                    toDt = SortBy(toDt);

                    G1.NumberDataTable(toDt);
                    toDGV.DataSource = toDt;
                    toDGV.RefreshDataSource();
                    toDGV.Refresh();

                    fromDt.Rows.Remove(dr);

                    fromGmain.RefreshData();
                    G1.NumberDataTable(fromDt);
                    fromDGV.DataSource = fromDt;

                    DataTable dx = (DataTable)dgv14.DataSource;

                    double faceAmount = GetFaceAmount(fromDt);

                    drs = dx.Select("Tab='" + fromName + "'");
                    if (drs.Length > 0)
                    {
                        drs[0]["faceAmount"] = faceAmount;
                        double count = drs[0]["Count"].ObjToDouble();
                        drs[0]["Count"] = count - 1D;
                    }

                    drs = dx.Select("Tab='" + toName + "'");
                    if (drs.Length > 0)
                    {
                        faceAmount = GetFaceAmount(toDt);
                        drs[0]["faceAmount"] = faceAmount;
                        double count = drs[0]["Count"].ObjToDouble();
                        drs[0]["Count"] = count + 1D;
                    }
                }
            }
        }
        /***********************************************************************************************/
        private AdvBandedGridView GetGridView(string name, ref DataTable dt, ref GridControl dgv)
        {
            AdvBandedGridView gMain = null;
            if (name == "All Policies")
            {
                gMain = gridMain;
                if (dgv == null)
                    return null;
                dt = (DataTable)dgv.DataSource;
                dgv = dgv;
            }
            else if (name == "Unity Active")
            {
                gMain = gridMain2;
                dt = (DataTable)dgv2.DataSource;
                dgv = dgv2;
            }
            else if (name == "Unity Lapsed")
            {
                gMain = gridMain3;
                dt = (DataTable)dgv3.DataSource;
                dgv = dgv3;
            }
            else if (name == "Unity Lapsed Questioned")
            {
                gMain = gridMain4;
                dt = (DataTable)dgv4.DataSource;
                dgv = dgv4;
            }
            else if (name == "Unity Deceased")
            {
                gMain = gridMain5;
                dt = (DataTable)dgv5.DataSource;
                dgv = dgv5;
            }
            else if (name == "Unity Cancelled")
            {
                gMain = gridMain6;
                dt = (DataTable)dgv6.DataSource;
                dgv = dgv6;
            }
            else if (name == "PB Unity AC")
            {
                gMain = gridMain7;
                dt = (DataTable)dgv7.DataSource;
                dgv = dgv7;
            }
            else if (name == "PB Unity DEC")
            {
                gMain = gridMain8;
                dt = (DataTable)dgv8.DataSource;
                dgv = dgv8;
            }
            else if (name == "Barham")
            {
                gMain = gridMain9;
                dt = (DataTable)dgv9.DataSource;
                dgv = dgv9;
            }
            else if (name == "Webb")
            {
                gMain = gridMain10;
                dt = (DataTable)dgv10.DataSource;
                dgv = dgv10;
            }
            else if (name == "Barham & Webb DEC")
            {
                gMain = gridMain11;
                dt = (DataTable)dgv11.DataSource;
                dgv = dgv11;
            }
            else if (name == "PB Direct Issue")
            {
                gMain = gridMain12;
                dt = (DataTable)dgv12.DataSource;
                dgv = dgv12;
            }
            else if (name == "Not Found")
            {
                gMain = gridMain13;
                dt = (DataTable)dgv13.DataSource;
                dgv = dgv13;
            }

            return gMain;
        }
        /***********************************************************************************************/
        private void btnExportToExcel_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Do you REALLY want to SAVE these tabs to Excel Files?", "Save Unity Tabs to Excel Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            if (result == DialogResult.No)
                return;

            PleaseWait pleaseForm = new PleaseWait("Please Wait!\nExporting Unity to Excel Files!");
            pleaseForm.TopMost = true;
            pleaseForm.Show();
            pleaseForm.Refresh();

            string directory = G1.DecodePath(importedFile);

            ExportUnityActive(directory);
            ExportUnityCancelled(directory);
            ExportUnityDeceased(directory);
            ExportUnityLapsed(directory);
            ExportUnityOldBarhamAndWEbbDeceased(directory);

            ExportTab(gridMain9, tabPageBarham, directory, "Unity Old Barham", "num,contractNumber");
            ExportTab(gridMain10, tabPageWebb, directory, "Unity Old Webb", "num,contractNumber" );
            ExportTab(gridMain7, tabPagePBUnityAC, directory, "Unity PB Active", "num,contractNumber");
            ExportTab(gridMain8, tabPagePBUnityDEC, directory, "Unity PB Deceased", "num,contractNumber");
            ExportTab(gridMain12, tabPagePBDirectIssue, directory, "Unity Pine Belp Direct Issue", "num,contractNumber");
            ExportTab(gridMain14, tabPageSummary, directory, "Unity Summary Counts" );

            this.tabControl1.SelectedTab = tabPage1;

            pleaseForm.FireEvent1();
            pleaseForm.Dispose();
            pleaseForm = null;
        }
        /***********************************************************************************************/
        private void ExportUnityActive ( string directory )
        {
            G1.SetColumnWidth(gridMain2, "Policy Extract_Policy Status", 50);
            G1.SetColumnWidth(gridMain2, "Policy Number", 100);
            G1.SetColumnWidth(gridMain2, "contract Number", 75);
            G1.SetColumnWidth(gridMain2, "Prior Unapplied Cash", 50);
            G1.SetColumnWidth(gridMain2, "Prior IBA", 50);
            G1.SetColumnWidth(gridMain2, "Prior Cash Received", 75);
            G1.SetColumnWidth(gridMain2, "Prior Premiums Paid", 75);
            G1.SetColumnWidth(gridMain2, "Current Monthly Premium", 75);
            G1.SetColumnWidth(gridMain2, "Current New Business", 75);
            G1.SetColumnWidth(gridMain2, "Policy Extract_Premiums Paid", 75);
            G1.SetColumnWidth(gridMain2, "Current Unapplied Cash", 75);
            G1.SetColumnWidth(gridMain2, "Current IBA", 75);
            G1.SetColumnWidth(gridMain2, "Current Cash Received", 75);
            ExportTab(gridMain2, tabPageUnityActive, directory, "Unity Active", "num,Balancing, O/S under $2,Paid Up Policies Refunded,Charlotte spreadsheet shows reversal,Other Reversal on Charlotte Spreadsheet,IBA/Unapplied Cash paid out at death claim,Change in status from Terminated to Reinstated cash value reapplied at reinstatement on lapsed policies,Diff in cash value in lapsed policy,Reconciling,explanaition,Date Claim Processed");
        }
        /***********************************************************************************************/
        private void ExportUnityCancelled(string directory)
        {
            G1.SetColumnWidth(gridMain6, "Policy Extract_Policy Status", 50);
            G1.SetColumnWidth(gridMain6, "Policy Number", 100);
            G1.SetColumnWidth(gridMain6, "contract Number", 75);
            G1.SetColumnWidth(gridMain6, "Prior Unapplied Cash", 50);
            G1.SetColumnWidth(gridMain6, "Prior IBA", 50);
            G1.SetColumnWidth(gridMain6, "Prior Cash Received", 100);
            G1.SetColumnWidth(gridMain6, "Prior Premiums Paid", 75);
            G1.SetColumnWidth(gridMain6, "Current Monthly Premium", 75);
            G1.SetColumnWidth(gridMain6, "Current New Business", 75);
            G1.SetColumnWidth(gridMain6, "Policy Extract_Premiums Paid", 75);
            G1.SetColumnWidth(gridMain6, "Current Unapplied Cash", 75);
            G1.SetColumnWidth(gridMain6, "Current IBA", 75);
            G1.SetColumnWidth(gridMain6, "Current Cash Received", 75);

            G1.SetColumnWidth(gridMain6, "FH Name", 200);
            G1.SetColumnWidth(gridMain6, "Insured First Name", 200);
            G1.SetColumnWidth(gridMain6, "Insured Last Name", 200);
            ExportTab(gridMain6, tabPageUnityCancelled, directory, "Unity Cancelled", "num,Balancing, O/S under $2,Paid Up Policies Refunded,Charlotte spreadsheet shows reversal,Other Reversal on Charlotte Spreadsheet,IBA/Unapplied Cash paid out at death claim,Change in status from Terminated to Reinstated cash value reapplied at reinstatement on lapsed policies,Diff in cash value in lapsed policy,Reconciling,explanaition,Date Claim Processed");
        }
        /***********************************************************************************************/
        private void ExportUnityDeceased(string directory)
        {
            G1.SetColumnWidth(gridMain5, "Policy Extract_Policy Status", 50);
            G1.SetColumnWidth(gridMain5, "Policy Number", 100);
            G1.SetColumnWidth(gridMain5, "contract Number", 75);
            G1.SetColumnWidth(gridMain5, "Prior Unapplied Cash", 50);
            G1.SetColumnWidth(gridMain5, "Prior IBA", 50);
            G1.SetColumnWidth(gridMain5, "Prior Cash Received", 100);
            G1.SetColumnWidth(gridMain5, "Prior Premiums Paid", 75);
            G1.SetColumnWidth(gridMain5, "Current Monthly Premium", 75);
            G1.SetColumnWidth(gridMain5, "Current New Business", 75);
            G1.SetColumnWidth(gridMain5, "Policy Extract_Premiums Paid", 75);
            G1.SetColumnWidth(gridMain5, "Current Unapplied Cash", 75);
            G1.SetColumnWidth(gridMain5, "Current IBA", 75);
            G1.SetColumnWidth(gridMain5, "Current Cash Received", 75);

            G1.SetColumnWidth(gridMain5, "FH Name", 200);
            G1.SetColumnWidth(gridMain5, "Insured First Name", 200);
            G1.SetColumnWidth(gridMain5, "Insured Last Name", 200);
            ExportTab(gridMain5, tabPageUnityDeceased, directory, "Unity Deceased", "num,Balancing, O/S under $2,Paid Up Policies Refunded,Charlotte spreadsheet shows reversal,Other Reversal on Charlotte Spreadsheet,IBA/Unapplied Cash paid out at death claim,Change in status from Terminated to Reinstated cash value reapplied at reinstatement on lapsed policies,Diff in cash value in lapsed policy,Reconciling,explanaition,Date Claim Processed");
        }
        /***********************************************************************************************/
        private void ExportUnityLapsed(string directory)
        {
            G1.SetColumnWidth(gridMain3, "Policy Extract_Policy Status", 50);
            G1.SetColumnWidth(gridMain3, "Policy Number", 100);
            G1.SetColumnWidth(gridMain3, "contract Number", 75);
            G1.SetColumnWidth(gridMain3, "Prior Unapplied Cash", 50);
            G1.SetColumnWidth(gridMain3, "Prior IBA", 50);
            G1.SetColumnWidth(gridMain3, "Prior Cash Received", 100);
            G1.SetColumnWidth(gridMain3, "Prior Premiums Paid", 75);
            G1.SetColumnWidth(gridMain3, "Current Monthly Premium", 75);
            G1.SetColumnWidth(gridMain3, "Current New Business", 75);
            G1.SetColumnWidth(gridMain3, "Policy Extract_Premiums Paid", 75);
            G1.SetColumnWidth(gridMain3, "Current Unapplied Cash", 75);
            G1.SetColumnWidth(gridMain3, "Current IBA", 75);
            G1.SetColumnWidth(gridMain3, "Current Cash Received", 75);

            G1.SetColumnWidth(gridMain3, "FH Name", 200);
            G1.SetColumnWidth(gridMain3, "Insured First Name", 200);
            G1.SetColumnWidth(gridMain3, "Insured Last Name", 200);
            ExportTab(gridMain3, tabPageUnityLapsed, directory, "Unity Lapsed", "num,Balancing, O/S under $2,Paid Up Policies Refunded,Charlotte spreadsheet shows reversal,Other Reversal on Charlotte Spreadsheet,IBA/Unapplied Cash paid out at death claim,Change in status from Terminated to Reinstated cash value reapplied at reinstatement on lapsed policies,Diff in cash value in lapsed policy,Reconciling,explanaition,Date Claim Processed");
        }
        /***********************************************************************************************/
        private void ExportUnityOldBarhamAndWEbbDeceased(string directory)
        {
            G1.SetColumnWidth(gridMain11, "Policy Extract_Policy Status", 50);
            G1.SetColumnWidth(gridMain11, "Policy Number", 100);
            G1.SetColumnWidth(gridMain11, "contract Number", 75);
            G1.SetColumnWidth(gridMain11, "Prior Unapplied Cash", 50);
            G1.SetColumnWidth(gridMain11, "Prior IBA", 50);
            G1.SetColumnWidth(gridMain11, "Prior Cash Received", 100);
            G1.SetColumnWidth(gridMain11, "Prior Premiums Paid", 75);
            G1.SetColumnWidth(gridMain11, "Current Monthly Premium", 75);
            G1.SetColumnWidth(gridMain11, "Current New Business", 75);
            G1.SetColumnWidth(gridMain11, "Policy Extract_Premiums Paid", 75);
            G1.SetColumnWidth(gridMain11, "Current Unapplied Cash", 75);
            G1.SetColumnWidth(gridMain11, "Current IBA", 75);
            G1.SetColumnWidth(gridMain11, "Current Cash Received", 75);

            G1.SetColumnWidth(gridMain11, "FH Name", 200);
            G1.SetColumnWidth(gridMain11, "Insured First Name", 200);
            G1.SetColumnWidth(gridMain11, "Insured Last Name", 200);
            ExportTab(gridMain11, tabPageBarhamWebbDEC, directory, "UNITY OLD BARHAM AND WEBB DEC", "num,Balancing, O/S under $2,Paid Up Policies Refunded,Charlotte spreadsheet shows reversal,Other Reversal on Charlotte Spreadsheet,IBA/Unapplied Cash paid out at death claim,Change in status from Terminated to Reinstated cash value reapplied at reinstatement on lapsed policies,Diff in cash value in lapsed policy,Reconciling,explanaition,Date Claim Processed");
        }
        /***********************************************************************************************/
        private void ExportTab (DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gridMain, TabPage tabPage, string directory, string title, string hideColumns = "" )
        {
            try
            {
                this.tabControl1.SelectedTab = tabPage;

                //GridView gridView = (GridView)gridMain;
                //bool autoWidth = false, columnAutoWidth = false;
                //Dictionary<GridColumn, int> widthByColumn = null;
                //if (gridView != null)
                //{
                //    autoWidth = gridView.OptionsPrint.AutoWidth;
                //    columnAutoWidth = gridView.OptionsView.ColumnAutoWidth;
                //    widthByColumn = gridView.Columns.ToDictionary(x => x, x => x.Width);
                //    gridView.OptionsPrint.AutoWidth = false;
                //    gridView.OptionsView.ColumnAutoWidth = false;
                //    gridView.BestFitColumns();
                //}

                string toName = tabPage.Text;
                if (!String.IsNullOrWhiteSpace(title))
                    toName = title;

                string outputFile = directory + "/" + "X" + toName + ".xlsx";
                string fullPath = outputFile;
                if (File.Exists(fullPath))
                    File.Delete(fullPath);

                gridMain.AppearancePrint.Row.Font = new Font("Calibri", 12F, FontStyle.Bold);
                gridMain.AppearancePrint.HeaderPanel.Font = new Font("Calibri", 12F, FontStyle.Bold);
                gridMain.AppearancePrint.BandPanel.Font = new Font("Calibri", 12F, FontStyle.Bold);
                gridMain.OptionsPrint.AllowMultilineHeaders = true;
                gridMain.OptionsView.ColumnHeaderAutoHeight = DefaultBoolean.True;
                gridMain.AppearancePrint.HeaderPanel.TextOptions.WordWrap = WordWrap.Wrap;
                gridMain.ColumnPanelRowHeight = 100;


                DevExpress.XtraGrid.GridControl dgv = (DevExpress.XtraGrid.GridControl)gridMain.GridControl;

                string[] Lines = hideColumns.Split(',');
                string str = "";
                for (int i = 0; i < Lines.Length; i++)
                {
                    str = Lines[i].Trim();
                    if (String.IsNullOrWhiteSpace(str))
                        continue;
                    if (G1.get_column_number(gridMain, str) < 0)
                        continue;
                    gridMain.Columns[str].Visible = false;
                }

                gridMain.RefreshEditor(true);
                gridMain.RefreshData();
                dgv.Refresh();

                XlsxExportOptions options = new XlsxExportOptions();
                options.FitToPrintedPageWidth = true;
                options.ShowGridLines = true;

                printableComponentLink1.Component = dgv;
                printableComponentLink1.Landscape = true;
                printableComponentLink1.Margins.Bottom = 0;
                printableComponentLink1.Margins.Top = 0;
                printableComponentLink1.Margins.Left = 0;
                printableComponentLink1.Margins.Right = 0;

                printingSystem1.Document.AutoFitToPagesWidth = 2; //Does not work


                printableComponentLink1.CreateDocument();
                printableComponentLink1.ExportToXlsx(fullPath, options );

                gridMain.ColumnPanelRowHeight = -1;


                for (int i = 0; i < Lines.Length; i++)
                {
                    str = Lines[i].Trim();
                    if (String.IsNullOrWhiteSpace(str))
                        continue;
                    if (G1.get_column_number(gridMain, str) < 0)
                        continue;
                    gridMain.Columns[str].Visible = true;
                }
            }
            catch ( Exception ex)
            {
            }
        }
        /***********************************************************************************************/
        private void btnExportToExcel_Clickx(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Do you REALLY want to SAVE these tabs to Excel Files?", "Save Unity Tabs to Excel Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            if (result == DialogResult.No)
                return;
            printToExcel = true;


            string iFile = importedFile;
            string aFile = actualFile;
            string directory = G1.DecodePath(importedFile);
            TabPage tabPage = null;
            string toName = "";
            string outputFile = "";
            string fullPath = "";

            XlsxExportOptions junk = new XlsxExportOptions();
            junk.ShowGridLines = false;
            junk.FitToPrintedPageWidth = false;
            junk.FitToPrintedPageHeight = false;
            junk.ExportHyperlinks = false;
            junk.ExportMode = XlsxExportMode.SingleFile;

            GridView gridView = (GridView) gridMain9;
            bool autoWidth = false, columnAutoWidth = false;
            Dictionary<GridColumn, int> widthByColumn = null;
            //if (bestFitColumns && gridView != null)
            if (gridView != null)
            {
                autoWidth = gridView.OptionsPrint.AutoWidth;
                columnAutoWidth = gridView.OptionsView.ColumnAutoWidth;
                widthByColumn = gridView.Columns.ToDictionary(x => x, x => x.Width);
                gridView.OptionsPrint.AutoWidth = false;
                gridView.OptionsView.ColumnAutoWidth = false;
                gridView.BestFitColumns();
            }

            DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView workGridMain = null;

            for (int i = 1; i < tabControl1.TabPages.Count; i++)
            {
                if (i >= 2)
                    break;

                tabPage = tabControl1.TabPages[i];
                toName = tabPage.Text;

                PleaseWait pleaseForm = new PleaseWait("Please Wait!\nExporting " + toName + "!");
                pleaseForm.Show();
                pleaseForm.Refresh();

                if (i == 0)
                    printableComponentLink1.Component = dgv;
                else if (i == 1)
                {
                    workGridMain = gridMain2;
                    outputFile = directory + "/" + "X" + toName + ".xlsx";
                    fullPath = outputFile;
                    if (File.Exists(fullPath))
                        File.Delete(fullPath);

                    //gridMain2.Columns["num"].Visible = false;

                    gridMain9.AppearancePrint.Row.Font = new Font("Calibri", 13.8F, FontStyle.Bold);
                    gridMain9.AppearancePrint.HeaderPanel.Font = new Font("Calibri", 13.8F, FontStyle.Bold);
                    //gridMain2.OptionsPrint.AllowMultilineHeaders = true;


                    //printingSystem1.Document.AutoFitToPagesWidth = 1;
                    gridMain9.Columns["num"].Visible = false;
                    gridMain9.Columns["contractNumber"].Visible = false;

                    printableComponentLink1.Component = dgv9;
                    DataTable dx = (DataTable) dgv9.DataSource;

                    printableComponentLink1.CreateDocument();
                    printableComponentLink1.ExportToXlsx(fullPath, junk );

                    //gridMain2.Columns["num"].Visible = true;
                    //gridMain2.Appearance.Row.Font = new Font("Tahoma", 7.8F, FontStyle.Regular);
                    //gridMain2.Appearance.HeaderPanel.Font = new Font("Tahoma", 7.8F, FontStyle.Regular);

                }
                else if (i == 2)
                    printableComponentLink1.Component = dgv3;
                else if (i == 3)
                    printableComponentLink1.Component = dgv4;
                else if (i == 4)
                    printableComponentLink1.Component = dgv5;
                else if (i == 5)
                    printableComponentLink1.Component = dgv6;
                else if (i == 6)
                    printableComponentLink1.Component = dgv7;
                else if (i == 7)
                    printableComponentLink1.Component = dgv8;
                else if (i == 8)
                    printableComponentLink1.Component = dgv9;
                else if (i == 9)
                    printableComponentLink1.Component = dgv10;
                else if (i == 10)
                    printableComponentLink1.Component = dgv11;
                else if (i == 11)
                    printableComponentLink1.Component = dgv12;
                else if (i == 12)
                    printableComponentLink1.Component = dgv13;
                else if (i == 13)
                    printableComponentLink1.Component = dgv14;

                printableComponentLink1.Landscape = true;

                tabTitle = tabPage.Text.Trim();
                tabName = tabPage.Name;

                //string outputDirectory = @"C:\SMFSData\Unity Reports";
                //G1.verify_path(outputDirectory);
                //outputDirectory += "/" + importYear + " " + importMonth;
                //G1.verify_path(outputDirectory);

                //string fullPath = outputDirectory + "/" + importYear + " " + importMonth + " " + tabTitle + ".xls";
                fullPath = outputFile;
                if (File.Exists(fullPath))
                    File.Delete(fullPath);

                //SetNewFont(workGridMain, "Calibri", 13.8F); // FT - SN

                printableComponentLink1.CreateDocument();
                printableComponentLink1.ExportToXlsx(fullPath);

                //ResetOldFont(workGridMain);

                pleaseForm.FireEvent1();
                pleaseForm.Dispose();
                pleaseForm = null;
            }
            printToExcel = false;
        }
        /****************************************************************************************/
        private void SetNewFont(DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gridMain, string font, float size)
        {
            try
            {
                gridMain.Columns["num"].Visible = false;

                //gridMain.Appearance.Row.Font = new Font(font, size, FontStyle.Bold);
                gridMain.AppearancePrint.Row.Font = new Font(font, size, FontStyle.Bold);
                gridMain.AppearancePrint.HeaderPanel.Font = new Font(font, size, FontStyle.Bold);
                //gridMain.RefreshEditor(true);
                //gridMain.RefreshData();
            }
            catch ( Exception ex )
            {
            }
        }
        /****************************************************************************************/
        private void ResetOldFont(DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gridMain )
        {
            try
            {
                gridMain.Columns["num"].Visible = true;

                gridMain.Appearance.Row.Font = new Font("Tahoma", 7.8F, FontStyle.Regular);
                gridMain.Appearance.HeaderPanel.Font = new Font("Tahoma", 7.8F, FontStyle.Regular);
            }
            catch (Exception ex)
            {
            }
        }
        /***********************************************************************************************/
        private void SetupTabFormat(string name, string caption)
        {
            //string name = cmbSelectColumns.Text.Trim();

            TabPage tabPage = tabControl1.TabPages[name];
            string toName = tabPage.Text;
            toName = toName.Replace("Move to ", "");

            GridControl toDGV = null;
            DataTable dt = null;

            DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView toGmain = GetGridView(toName, ref dt, ref toDGV);

            if (toGmain == null)
                return;

            if (String.IsNullOrWhiteSpace(name))
                name = workReport + " Primary";
            string saveName = "Unity " + workReport + " " + toName;
            string skinName = "";

            SetupSelectedColumns("Unity " + workReport, name, toDGV, toGmain);

            for (int i = 0; i < toGmain.Bands.Count; i++)
                toGmain.Bands[i].Caption = this.Text + " " + toName;

            foundLocalPreference = G1.RestoreGridLayout(this, toDGV, toGmain, LoginForm.username, saveName, ref skinName);
            if (!foundLocalPreference)
                return;

            toGmain.OptionsView.ShowFooter = true;

            SetupTotalsSummary();

            //string field = "";
            //string select = "";
            //for (int i = 0; i < dt.Rows.Count; i++)
            //{
            //    field = dt.Rows[i]["field"].ObjToString();
            //    select = dt.Rows[i]["select"].ObjToString();
            //    if (G1.get_column_number(toGmain, field) >= 0)
            //    {
            //        if (select == "0")
            //            toGmain.Columns[field].Visible = false;
            //        else
            //            toGmain.Columns[field].Visible = true;
            //    }
            //}
            toDGV.Refresh();
            this.Refresh();
        }
        /***********************************************************************************************/
        private void SetupSelectedColumns(string procType, string group, DevExpress.XtraGrid.GridControl dgv, AdvBandedGridView gMain)
        {
            if (String.IsNullOrWhiteSpace(group))
                return;
            if (String.IsNullOrWhiteSpace(procType))
                procType = "Unity " + workReport;

            string cmd = "Select * from procfiles where name = '" + group + "' and ProcType = '" + procType + "' order by seq";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return;

            for (int i = 0; i < gMain.Columns.Count; i++)
                gMain.Columns[i].Visible = false;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string name = dt.Rows[i]["Description"].ToString();
                int index = dt.Rows[i]["seq"].ObjToInt32();
                try
                {
                    if (G1.DoesGridViewColumnExist(gMain, name))
                        ((GridView)dgv.MainView).Columns[name].Visible = true;
                }
                catch
                {
                }
            }
        }
        /****************************************************************************************/
        private void lockScreenDetailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TabPage current = tabControl1.SelectedTab;
            string name = current.Text.Trim();

            GridControl toDGV = null;
            DataTable dt = null;

            DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView toGmain = GetGridView(name, ref dt, ref toDGV);

            if (toGmain == null)
                return;


            //string name = cmbSelectColumns.Text.Trim();
            if (String.IsNullOrWhiteSpace(name))
                name = "Primary";
            string saveName = "Unity " + workReport + " " + name;
            G1.SaveLocalPreferences(this, toGmain, LoginForm.username, saveName);

            foundLocalPreference = true;
        }
        /****************************************************************************************/
        private void unLockScreenDetailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TabPage current = tabControl1.SelectedTab;
            string comboName = current.Text.Trim();
            //string comboName = cmbSelectColumns.Text;
            if (!String.IsNullOrWhiteSpace(comboName))
            {
                string name = comboName;
                if (String.IsNullOrWhiteSpace(name))
                    name = "Primary";
                string saveName = "Unity " + workReport + " " + name;
                G1.RemoveLocalPreferences(LoginForm.username, saveName);
                foundLocalPreference = false;
            }

            foundLocalPreference = false;
        }
        /***********************************************************************************************/
        private string importedFile = "";
        private string actualFile = "";
        private string importMonth = "";
        private string importYear = "";
        /***********************************************************************************************/
        private void button1_Click(object sender, EventArgs e)
        {
            DataTable workDt = null;
            this.Cursor = Cursors.WaitCursor;
            string sheetName = "List of all policies";
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string file = ofd.FileName;
                    importedFile = file;
                    int idx = file.LastIndexOf("\\");
                    if (idx > 0)
                    {
                        actualFile = file.Substring(idx);
                        actualFile = actualFile.Replace("\\", "");
                    }

                    importMonth = DetermineMonth();
                    if (String.IsNullOrWhiteSpace(importMonth))
                    {
                        MessageBox.Show("*** ERROR *** I'm having trouble determing the Month from the Filename!\nIt should be the first word!", "Unity Filename Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                        return;
                    }
                    importYear = DetermineYear();
                    if (String.IsNullOrWhiteSpace(importYear))
                    {
                        MessageBox.Show("*** ERROR *** I'm having trouble determing the Year from the Filename!\nIt should be the second word!", "Unity Filename Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                        return;
                    }

                    dgv.DataSource = null;
                    this.Cursor = Cursors.WaitCursor;
                    workDt = null;
                    try
                    {
                        workDt = ExcelWriter.ReadFile2(file, 0, sheetName);

                        workDt.TableName = actualFile;
                    }
                    catch (Exception ex)
                    {
                    }
                    if (workDt != null)
                    {
                        workDt.TableName = actualFile;

                        ClearGridView();
                        //chkExcludeHeader.Show();
                        //chkExcludeHeader.Refresh();
                        //btnLoadOthers.Hide();
                        //btnLoadOthers.Refresh();
                        //btnSave.Hide();
                        //btnSave.Refresh();
                        //btnExportToExcel.Hide();
                        //btnExportToExcel.Refresh();


                        workDt = PreProcessUnity(workDt);

                        workDt = LookupTrusts(workDt);

                        if (G1.get_column_number(workDt, "num") < 0)
                            workDt.Columns.Add("num", typeof(string)).SetOrdinal(0);
                        if (G1.get_column_number(workDt, "status") < 0)
                            workDt.Columns.Add("status");
                        if (G1.get_column_number(workDt, "insuredName") < 0)
                            workDt.Columns.Add("insuredName");
                        if (G1.get_column_number(workDt, "tab") < 0)
                            workDt.Columns.Add("tab");

                        ImportForm_SelectDone(workDt);
                    }
                }
            }
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private DataTable PreProcessUnity(DataTable dt)
        {
            int firstRow = -1;
            string search = "POLICY NUMBER";
            search = "FH NAME";
            string str = "";
            DataTable newDt = dt.Clone();

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    str = dt.Rows[i][j].ObjToString().ToUpper();
                    if (str == search)
                    {
                        firstRow = i;
                        break;
                    }
                }
                if (firstRow >= 0)
                    break;
            }
            if (firstRow < 0)
                return newDt;
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                str = dt.Rows[firstRow][i].ObjToString();
                if (String.IsNullOrWhiteSpace(str))
                    continue;
                if (G1.get_column_number(dt, str) >= 0)
                {
                    for (; ; )
                    {
                        str = str + "2";
                        if (G1.get_column_number(dt, str) < 0)
                            break;
                    }
                }
                newDt.Columns[i].ColumnName = str;
                newDt.Columns[i].Caption = str;

                dt.Columns[i].ColumnName = str;
                dt.Columns[i].Caption = str;
            }
            string policyNumber = "";
            for (int i = (firstRow + 1); i < dt.Rows.Count; i++)
            {
                str = dt.Rows[i]["POLICY NUMBER"].ObjToString().ToUpper();
                if (String.IsNullOrWhiteSpace(str))
                    continue;
                policyNumber = str;
                str = dt.Rows[i]["FH NAME"].ObjToString();
                if (String.IsNullOrWhiteSpace(str))
                    continue;
                if (str.ToUpper() == "FH NAME")
                {
                    if (!G1.validate_numeric(policyNumber))
                        continue;
                }
                //if (!G1.validate_numeric(str))
                //    continue;
                newDt.ImportRow(dt.Rows[i]);
            }
            return newDt;
        }
        /***********************************************************************************************/
        private DataTable LookupTrusts(DataTable dt)
        {
            string policy = "";
            string contractNumber = "";
            DataRow[] dRows = null;

            dt.Columns.Add("contractNumber").SetOrdinal(0);

            string cmd = "Select * from `policytrusts`;";
            DataTable dx = G1.get_db_data(cmd);

            for ( int i=0; i<dt.Rows.Count; i++)
            {
                policy = dt.Rows[i]["Policy Number"].ObjToString();
                if (String.IsNullOrWhiteSpace(policy))
                    continue;
                dRows = dx.Select("policyNumber='" + policy + "'");
                if ( dRows.Length > 0 )
                {
                    contractNumber = dRows[0]["contractNumber"].ObjToString();
                    dt.Rows[i]["contractNumber"] = contractNumber;
                }
            }
            return dt;
        }
        /***********************************************************************************************/
        private DataTable mapUnity(DataTable dt)
        {
            string str = "";
            if (G1.get_column_number(dt, "lastName") < 0)
            {
                try
                {
                    dt.Columns.Add("middleName");
                }
                catch (Exception ex)
                {
                }
            }
            if (G1.get_column_number(dt, "preOrPost") < 0)
                dt.Columns.Add("preOrPost");
            if (G1.get_column_number(dt, "trustCompany") < 0)
                dt.Columns.Add("trustCompany");
            if (G1.get_column_number(dt, "growth") < 0)
                dt.Columns.Add("growth", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "payments") < 0)
                dt.Columns.Add("payments", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "downPayments") < 0)
                dt.Columns.Add("downPayments", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "contractNumber") < 0)
                dt.Columns.Add("contractNumber");

            if (G1.get_column_number(dt, "deathClaimAmount") < 0)
                dt.Columns.Add("deathClaimAmount", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "reducedPaidUpAmount") < 0)
                dt.Columns.Add("reducedPaidUpAmount", Type.GetType("System.Double"));
            //if (G1.get_column_number(dt, "deathPaidDate") < 0)
            //    dt.Columns.Add("deathPaidDate");

            string[] Lines = null;

            string cName = "";
            bool found = false;

            for (int i = 0; i < dt.Columns.Count; i++)
            {
                try
                {
                    str = dt.Columns[i].ColumnName.ObjToString().Trim();
                    if (str == "Policy Number")
                    {
                        dt.Columns[i].ColumnName = "policyNumber";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "FH Name")
                    {
                        dt.Columns[i].ColumnName = "trustName";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "Num")
                    {
                        dt.Columns[i].ColumnName = "num";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "Prior Cash Received")
                    {
                        dt.Columns[i].ColumnName = "beginningPaymentBalance";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "Current Cash Received")
                    {
                        dt.Columns[i].ColumnName = "beginningDeathBenefit";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "Prior Unapplied Cash")
                    {
                        dt.Columns[i].ColumnName = "priorUnappliedCash";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "Current Unapplied Cash")
                    {
                        dt.Columns[i].ColumnName = "currentUnappliedCash";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "Death Benefit")
                    {
                        dt.Columns[i].ColumnName = "endingDeathBenefit";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "Face Amount")
                    {
                        dt.Columns[i].ColumnName = "endingPaymentBalance";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "Down Payments")
                    {
                        dt.Columns[i].ColumnName = "downPayments";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "Payments")
                    {
                        dt.Columns[i].ColumnName = "payments";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "Insured Name")
                    {
                        dt.Columns[i].ColumnName = "insuredName";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "Insured First Name")
                    {
                        dt.Columns[i].ColumnName = "firstName";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "Insured Last Name")
                    {
                        dt.Columns[i].ColumnName = "lastName";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "Policy Extract_Policy Status")
                    {
                        dt.Columns[i].ColumnName = "policyStatus";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "Policy Extract_Status Reason")
                    {
                        dt.Columns[i].ColumnName = "statusReason";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    else if (str == "Policy Extract_Billing Reason")
                    {
                        dt.Columns[i].ColumnName = "billingReason";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                    //else if (str == "Paid-to-Date")
                    //{
                    //    dt.Columns[i].ColumnName = "deathPaidDate";
                    //    dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    //}
                    else if (str == "Date Claim Processed")
                    {
                        dt.Columns[i].ColumnName = "deathPaidDate";
                        dt.Columns[i].Caption = dt.Columns[i].ColumnName.ObjToString().Trim();
                    }
                }
                catch (Exception ex)
                {
                }
            }

            try
            {
                if (G1.get_column_number(dt, "trustName") < 0)
                    dt.Columns.Add("trustName");
                if (G1.get_column_number(dt, "statusReason") < 0)
                    dt.Columns.Add("statusReason");
                if (G1.get_column_number(dt, "billingReason") < 0)
                    dt.Columns.Add("billingReason");
                if (G1.get_column_number(dt, "policyStatus") < 0)
                    dt.Columns.Add("policyStatus");
            }
            catch ( Exception )
            {
            }
            string name = "";
            string firstName = "";
            string middleName = "";
            string lastName = "";
            string preOrPost = "";
            string trustName = "";

            double payments = 0D;
            double downPayments = 0D;
            double beginningDeathBenefit = 0D;
            double endingDeathBenefit = 0D;
            double reducedPaidUpAmount = 0D;
            double growth = 0D;

            try
            {
                string cmd = "";
                DataTable dx = null;
                string contractNumber = "";
                string policyNumber = "";
                string trustCompany = "";
                string billingReason = "";

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    trustCompany = "Unity";
                    policyNumber = dt.Rows[i]["policyNumber"].ObjToString();
                    if (policyNumber == "770080466")
                    {
                    }
                    if (policyNumber.ToUpper().IndexOf("PB") == 0)
                        trustCompany = "Unity DI";
                    else if (policyNumber.ToUpper().IndexOf("PI") == 0)
                        trustCompany = "Unity PB";
                    else if (policyNumber.ToUpper().IndexOf("PS") == 0)
                        trustCompany = "Unity PB";

                    dt.Rows[i]["trustCompany"] = trustCompany;

                    reducedPaidUpAmount = 0D;

                    billingReason = dt.Rows[i]["billingReason"].ObjToString();

                    trustName = dt.Rows[i]["trustName"].ObjToString();

                    preOrPost = "post";
                    dt.Rows[i]["preOrPost"] = preOrPost;
                    if (policyNumber == "770080466")
                    {
                    }

                    beginningDeathBenefit = dt.Rows[i]["beginningDeathBenefit"].ObjToDouble();
                    endingDeathBenefit = dt.Rows[i]["endingDeathBenefit"].ObjToDouble();
                    //if ( billingReason.ToUpper() == "RU")
                    //{
                    //    reducedPaidUpAmount = endingDeathBenefit;
                    //    endingDeathBenefit = reducedPaidUpAmount;
                    //    dt.Rows[i]["endingDeathBenefit"] = 0D;
                    //    //dt.Rows[i]["endingPaymentBalance"] = reducedPaidUpAmount;
                    //}
                    payments = dt.Rows[i]["payments"].ObjToDouble();
                    downPayments = dt.Rows[i]["downPayments"].ObjToDouble();
                    if (endingDeathBenefit > 0D)
                    {
                        growth = endingDeathBenefit - beginningDeathBenefit - payments - downPayments;
                        growth = G1.RoundValue(growth);
                    }
                    else
                        growth = 0D;
                    dt.Rows[i]["growth"] = growth;

                    policyNumber = dt.Rows[i]["policyNumber"].ObjToString().Trim();
                    cmd = "Select * from `policytrusts` where `policyNumber` = '" + policyNumber + "';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                    {
                        contractNumber = dx.Rows[0]["contractNumber"].ObjToString();
                        dt.Rows[i]["contractNumber"] = contractNumber;
                    }
                    else
                    {
                        if (policyNumber.ToUpper().IndexOf("PBI") == 0)
                        {
                            policyNumber = policyNumber.ToUpper().Replace("PBI", "");
                            cmd = "Select * from `contracts` where `contractNumber` = '" + policyNumber + "';";
                            dx = G1.get_db_data(cmd);
                            if (dx.Rows.Count > 0)
                            {
                                contractNumber = dx.Rows[0]["contractNumber"].ObjToString();
                                dt.Rows[i]["contractNumber"] = contractNumber;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return dt;
        }
        /***********************************************************************************************/
        private string DetermineMonth()
        {
            string[] Months = new string[12];

            Months[0] = "January";
            Months[1] = "February";
            Months[2] = "March";
            Months[3] = "April";
            Months[4] = "May";
            Months[5] = "June";
            Months[6] = "July";
            Months[7] = "August";
            Months[8] = "September";
            Months[9] = "October";
            Months[10] = "November";
            Months[11] = "December";

            string month = "";

            try
            {
                string name = actualFile.Trim().ToUpper();
                name = name.Trim();

                string str = "";
                string year = "";
                string[] Lines = name.Split(' ');
                month = Lines[0].Trim();
                month = G1.force_lower_line(month);
                for (int i = 0; i < Months.Length; i++)
                {
                    str = Months[i].ObjToString().Trim();
                    if (month.IndexOf(str) == 0)
                    {
                        month = month.Replace(str, "");
                        if (!String.IsNullOrWhiteSpace(month))
                        {
                            year = month.Trim();
                            month = str;
                            break;
                        }
                        else
                        {
                            month = str;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return month;
        }
        /***********************************************************************************************/
        private string DetermineYear()
        {
            string year = "";
            try
            {
                string name = actualFile.Trim().ToUpper();
                name = name.Trim();

                name = name.Replace(".xlsx", "");
                name = name.Replace(".xls", "");
                name = name.Replace(".XLSX", "");
                name = name.Replace(".XLS", "");
                string[] Lines = name.Split(' ');
                string str = Lines[1].Trim();
                if (G1.validate_numeric(str))
                {
                    int iyear = str.ObjToInt32();
                    if (iyear < 100)
                        iyear += 2000;
                    year = iyear.ToString();
                }
                else
                    str = year;
            }
            catch (Exception ex)
            {
            }
            return year;
        }
        /***********************************************************************************************/
        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveUnityData();
        }
        /***********************************************************************************************/
        private DataTable CleanupUnityData ( DataTable dt )
        {
            string colName = "";
            DataTable dx = null;
            try
            {
                string cmd = "Select * from `trust_data` WHERE `trustCompany` = 'xxxx';";
                dx = G1.get_db_data(cmd);
                for (int i = dt.Columns.Count - 1; i >= 0; i--)
                {
                    colName = dt.Columns[i].ColumnName.Trim();
                    if (colName.ToUpper() == "NUM")
                        continue;
                    else if (colName.ToUpper() == "TMSTAMP")
                        continue;
                    else if (colName.ToUpper() == "DATE")
                        continue;
                    if (G1.get_column_number(dx, colName) < 0)
                        dt.Columns.RemoveAt(i);
                }

                string[] columns = new string[dx.Columns.Count];
                for (int i = 0; i <dx.Columns.Count; i++)
                {
                    colName = dx.Columns[i].ColumnName.Trim().ToUpper();
                    if (colName == "TMSTAMP")
                        continue;
                    else if (colName == "NUM")
                        continue;
                    else if (colName == "RECORD")
                        continue;
                    else if (colName == "DATE")
                        continue;
                    columns[i] = dx.Columns[i].ColumnName.Trim();
                }
                dt = SetColumnsOrder(dt, columns);
            }
            catch ( Exception ex)
            {
            }


            return dt;
        }
        /***********************************************************************************************/
        private DataTable SetColumnsOrder(DataTable table, params String[] columnNames)
        {
            int columnIndex = 0;
            foreach (var columnName in columnNames)
            {
                if (!String.IsNullOrWhiteSpace(columnName))
                {
                    table.Columns[columnName].SetOrdinal(columnIndex);
                    columnIndex++;
                }
            }
            return table;
        } 
        /***********************************************************************************************/
        private void SaveUnityData()
        {
            DataTable dt = (DataTable)dgv.DataSource;

            if (G1.get_column_number(dt, "reducedPaidUpAmount") < 0)
                dt.Columns.Add("reducedPaidUpAmount", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "billingReason") < 0)
                dt.Columns.Add("billingReason");
            if (G1.get_column_number(dt, "statusReason") < 0)
                dt.Columns.Add("statusReason");
            if (G1.get_column_number(dt, "policyStatus") < 0)
                dt.Columns.Add("policyStatus");
            if (G1.get_column_number(dt, "insuredName") < 0)
                dt.Columns.Add("insuredName");

            DataTable dx = null;
            string record = "";
            string cmd = "";

            string company = "Unity";
            string trustName = "";
            string policyNumber = "";

            string contractNumber = "";
            double premium = 0D;
            double surrender = 0D;
            double faceAmount = 0D;
            double deathBenefit = 0D;
            double downPayments = 0D;
            double payments = 0D;
            double growth = 0D;
            string preOrPost = "";
            double deathClaimAmount = 0D;
            string deathPaidDate = "";

            string insuredName = "";
            string lastName = "";
            string firstName = "";
            string middleName = "";

            DataTable myDt = new DataTable();
            string cName = "";
            string type = "";
            DataRow dRow = null;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dRow = myDt.NewRow();
                myDt.Rows.Add();
            }

            for (int i = 0; i < gridMain.Columns.Count; i++)
            {
                cName = gridMain.Columns[i].FieldName.Trim();
                if (G1.get_column_number(dt, cName) < 0)
                    dt.Columns.Add(cName);

                type = dt.Columns[cName].DataType.ObjToString();
                if (type == "System.String")
                    myDt.Columns.Add(cName);
                else if (type == "System.Double")
                    myDt.Columns.Add(cName, Type.GetType("System.Double"));
                else
                {
                }

                G1.copy_dt_column(dt, cName, myDt, cName);
            }

            myDt = mapUnity(myDt);

            myDt = CleanupUnityData(myDt);

            CleanupCommas(myDt, "beginningPaymentBalance");
            CleanupCommas(myDt, "beginningDeathBenefit");
            CleanupCommas(myDt, "endingPaymentBalance");
            CleanupCommas(myDt, "endingDeathBenefit");
            CleanupCommas(myDt, "downPayments");
            CleanupCommas(myDt, "payments");
            CleanupCommas(myDt, "growth");
            CleanupCommas(myDt, "priorUnappliedCash");
            CleanupCommas(myDt, "currentUnappliedCash");
            CleanupCommas(myDt, "deathClaimAmount");
            CleanupCommas(myDt, "reducedPaidUpAmount");

            try
            {
                DateTime date = DateTime.Now;
                string date1 = "";
                string month = DetermineMonth();
                if (String.IsNullOrWhiteSpace(month))
                {
                    MessageBox.Show("*** ERROR *** I'm having trouble determing the Month from the Filename!\nIt should be the first word!", "Unity Filename Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    return;
                }
                string s_year = DetermineYear();
                if (String.IsNullOrWhiteSpace(s_year))
                {
                    MessageBox.Show("*** ERROR *** I'm having trouble determing the Year from the Filename!\nIt should be the second word!", "Unity Filename Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    return;
                }

                int year = s_year.ObjToInt32();
                if (month.ToUpper() == "JANUARY")
                    date = new DateTime(year, 1, 31);
                else if (month.ToUpper() == "FEBRUARY")
                    date = new DateTime(year, 2, 28);
                else if (month.ToUpper() == "MARCH")
                    date = new DateTime(year, 3, 31);
                else if (month.ToUpper() == "APRIL")
                    date = new DateTime(year, 4, 30);
                else if (month.ToUpper() == "MAY")
                    date = new DateTime(year, 5, 31);
                else if (month.ToUpper() == "JUNE")
                    date = new DateTime(year, 6, 30);
                else if (month.ToUpper() == "JULY")
                    date = new DateTime(year, 7, 31);
                else if (month.ToUpper() == "AUGUST")
                    date = new DateTime(year, 8, 31);
                else if (month.ToUpper() == "SEPTEMBER")
                    date = new DateTime(year, 9, 30);
                else if (month.ToUpper() == "OCTOBER")
                    date = new DateTime(year, 10, 31);
                else if (month.ToUpper() == "NOVEMBER")
                    date = new DateTime(year, 11, 30);
                else if (month.ToUpper() == "DECEMBER")
                    date = new DateTime(year, 12, 31);

                this.Cursor = Cursors.WaitCursor;

                SaveData ( myDt, date );

                btnSave.Hide();
                btnSave.Refresh();
                this.Cursor = Cursors.Default;
            }
            catch (Exception ex)
            {
            }
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void CleanupCommas(DataTable dt, string column)
        {
            string str = "";
            int i = 0;
            try
            {
                for (i = 0; i < dt.Rows.Count; i++)
                {
                    str = dt.Rows[i][column].ObjToString();
                    if (str.IndexOf("$") >= 0)
                    {
                        str = str.Replace("$", "");
                        dt.Rows[i][column] = str;
                    }
                    if (String.IsNullOrWhiteSpace(str))
                        dt.Rows[i][column] = "0";
                    else if (str.IndexOf(",") > 0)
                    {
                        str = str.Replace(",", "");
                        dt.Rows[i][column] = str;
                    }
                }
            }
            catch ( Exception ex )
            {
            }
        }
        /***********************************************************************************************/
        private bool SaveData(DataTable dt, DateTime saveDate)
        {
            this.Cursor = Cursors.WaitCursor;

            DataTable saveDt = dt.Copy();

            if (G1.get_column_number(saveDt, "date") < 0)
                saveDt.Columns.Add("date");
            if (G1.get_column_number(saveDt, "trustCompany") < 0)
                saveDt.Columns.Add("trustCompany");

            if (G1.get_column_number(saveDt, "tmstamp") >= 0)
                saveDt.Columns.Remove("tmstamp");
            if (G1.get_column_number(saveDt, "record") >= 0)
                saveDt.Columns.Remove("record");
            if (G1.get_column_number(saveDt, "num") >= 0)
                saveDt.Columns.Remove("num");
            if (G1.get_column_number(saveDt, "found") >= 0)
                saveDt.Columns.Remove("found");

            saveDt.Columns["date"].SetOrdinal(0);
            saveDt.Columns["status"].SetOrdinal(0);


            DataColumn Col = saveDt.Columns.Add("record", System.Type.GetType("System.String"));
            Col.SetOrdinal(0);// to put the column in position 0;
            DataColumn Col1 = saveDt.Columns.Add("tmstamp", System.Type.GetType("System.String"));
            Col1.SetOrdinal(0);// to put the column in position 0;

            double dValue = 0D;
            string str = "";

            for (int i = 0; i < saveDt.Rows.Count; i++)
            {
                //saveDt.Rows[i]["tmstamp"] = "0000-00-00";
                saveDt.Rows[i]["tmstamp"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                saveDt.Rows[i]["record"] = "0";
                saveDt.Rows[i]["trustName"] = G1.try_protect_data(saveDt.Rows[i]["trustName"].ObjToString());
                saveDt.Rows[i]["insuredName"] = G1.try_protect_data(saveDt.Rows[i]["insuredName"].ObjToString());
                saveDt.Rows[i]["firstName"] = G1.try_protect_data(saveDt.Rows[i]["firstName"].ObjToString());
                saveDt.Rows[i]["middleName"] = G1.try_protect_data(saveDt.Rows[i]["middleName"].ObjToString());
                saveDt.Rows[i]["lastName"] = G1.try_protect_data(saveDt.Rows[i]["lastName"].ObjToString());
                saveDt.Rows[i]["firstName"] = G1.Truncate(saveDt.Rows[i]["firstName"].ObjToString(), 80);
                saveDt.Rows[i]["middleName"] = G1.Truncate(saveDt.Rows[i]["middleName"].ObjToString(), 80);
                saveDt.Rows[i]["lastName"] = G1.Truncate(saveDt.Rows[i]["lastName"].ObjToString(), 80);

                //saveDt.Rows[i]["trustCompany"] = workWhat;
                saveDt.Rows[i]["date"] = saveDate.ToString("yyyyMMdd");
            }

            DateTime date = saveDate;

            DeletePreviousData(saveDate, "Unity");

            string strFile = "/TrustData/TrustData_P_" + date.ToString("yyyyMMdd") + ".csv";
            string Server = "C:/rag";
            //Create directory if not exist... Make sure directory has required rights..
            if (!Directory.Exists(Server + "/TrustData/"))
                Directory.CreateDirectory(Server + "/TrustData/");

            //If file does not exist then create it and right data into it..
            if (!File.Exists(Server + strFile))
            {
                FileStream fs = new FileStream(Server + strFile, FileMode.Create, FileAccess.Write);
                fs.Close();
                fs.Dispose();
            }

            //Generate csv file from where data read

            try
            {
                //DateTime saveDate = this.dateTimePicker2.Value;
                int days = DateTime.DaysInMonth(saveDate.Year, saveDate.Month);
                //                string mySaveDate = saveDate.Year.ToString("D4") + "-" + saveDate.Month.ToString("D2") + "-" + days.ToString("D2") + " 00:00:00";

                var mySaveDate = G1.DTtoMySQLDT(saveDate);

                //for ( int i=0; i<saveDt.Rows.Count; i++)
                //    saveDt.Rows[i]["payDate8"] = mySaveDate;

                MySQL.CreateCSVfile(saveDt, Server + strFile, false, "~");
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Creating CSV File to load into Database " + ex.Message.ToString());
                this.Cursor = Cursors.Default;
                return false;
            }
            try
            {
                Structures.TieDbTable("trust_data", saveDt);
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Tieing trust_data to DataTable " + ex.Message.ToString());
                this.Cursor = Cursors.Default;
                return false;
            }
            try
            {
                G1.conn1.Open();
                MySqlBulkLoader bcp1 = new MySqlBulkLoader(G1.conn1);
                bcp1.TableName = "trust_data"; //Create table into MYSQL database...
                bcp1.FieldTerminator = ",";

                bcp1.LineTerminator = "\r\n";
                bcp1.FileName = Server + strFile;
                bcp1.NumberOfLinesToSkip = 0;
                bcp1.FieldTerminator = "~";
                bcp1.Load();
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Bulk Loading trust_data to DataTable " + ex.Message.ToString());
            }

            saveDt.Dispose();
            saveDt = null;

            this.Cursor = Cursors.Default;
            return true;
        }
        /***********************************************************************************************/
        private void DeletePreviousData(DateTime saveDate, string trustCompany)
        {
            string date1 = saveDate.ToString("yyyy-MM-dd");

            string cmd = "DELETE from `trust_data` where `date` = '" + date1 + "' AND `trustCompany` = '" + trustCompany + "' ";
            if (trustCompany.ToUpper() == "UNITY")
            {
                cmd = "DELETE from `trust_data` where `date` = '" + date1 + "' AND `trustCompany` LIKE '" + trustCompany + "%' ";
            }
            else if (trustCompany.ToUpper() == "FDLIC")
            {
                cmd = "DELETE from `trust_data` where `date` = '" + date1 + "' AND `trustCompany` LIKE '" + trustCompany + "%' ";
            }
            cmd += ";";
            try
            {
                G1.get_db_data(cmd);
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Delete Previous Data " + ex.Message.ToString());
            }
        }
        /***********************************************************************************************/
        private void menuReadOldData_Click(object sender, EventArgs e)
        {
            string cmd = "Select * from `trust_data` where `trustCompany` = 'Unity' Group by `date` ORDER by `date` DESC;";
            DataTable dx = G1.get_db_data(cmd);

            string selection = "";
            string lines = "";
            for ( int i=0; i<dx.Rows.Count; i++)
            {
                lines += dx.Rows[i]["date"].ObjToDateTime().ToString("yyyy-MM-dd");
                lines += "\n";
            }
            lines = lines.TrimEnd('\n');
            using (SelectFromList listForm = new SelectFromList(lines))
            {
                listForm.Text = "Select Date to Read";
                listForm.ShowDialog();

                selection = SelectFromList.theseSelections.Trim();
            }

            if (String.IsNullOrWhiteSpace(selection))
                return;

            ClearGridView();
            HideOrShowTabs(false);

            cmd = "Select * from `trust_data` where `trustCompany` LIKE 'Unity%' AND `date` = '" + selection + "';";
            dx = G1.get_db_data(cmd);

            G1.NumberDataTable(dx);
            dgv.DataSource = dx;

            if ( G1.get_column_number ( gridMain, "record") >= 0 )
                gridMain.Columns["record"].Visible = false;

            chkExcludeHeader.Hide();
            chkExcludeHeader.Refresh();
            btnLoadOthers.Hide();
            btnLoadOthers.Refresh();
            btnSave.Hide();
            btnSave.Refresh();
            btnExportToExcel.Hide();
            btnExportToExcel.Refresh();
        }
        /***********************************************************************************************/
        private void ClearGridView()
        {
            gridMain.BeginUpdate();

            dgv.DataSource = null;
            //gridMain.Bands.Clear();
            gridMain.Columns.Clear();

            gridMain.EndUpdate();
        }
        /***********************************************************************************************/
    }
}

