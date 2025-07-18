﻿using System;
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
using DevExpress.Office.Utils;
using DevExpress.XtraGrid;
using System.Drawing.Drawing2D;
using DevExpress.Utils.Drawing;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
//using System.Web.UI.WebControls;
using DevExpress.Data;
using DevExpress.XtraGrid.Views.Grid;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class TrustContractEOY: DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        DataTable funDt = null;
        DataTable preDt = null;
        DataTable cemDt = null;
        DataTable agentDt = null;
        DataTable originalDt = null;
        private bool loading = true;
        private bool foundLocalPreference = false;
        private string workFormat = "";
        /***********************************************************************************************/
        public TrustContractEOY()
        {
            InitializeComponent();
        }
        /***********************************************************************************************/
        private void TrustContractEOY_Load(object sender, EventArgs e)
        {
            footerCount = 0;

            DateTime now = DateTime.Now;
            now = now.AddMonths(-1);
            now = new DateTime(now.Year, now.Month, 1);
            //this.dateTimePicker1.Value = now;
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            DateTime stop = new DateTime(now.Year, now.Month, days);
            this.dateTimePicker2.Value = stop;

            loadLocatons();

            agentDt = G1.get_db_data("Select * from `agents`;");
            SetupTotalsSummary();

            SetupPrintView(gridMain); // <--- New as of 04/30/2025
            SetupPrintView(gridMain3); // <--- New as of 04/30/2025
            SetupPrintView(gridMain4); // <--- New as of 04/30/2025
            SetupPrintView(gridMain5); // <--- New as of 04/30/2025
            SetupPrintView(gridMain6); // <--- New as of 04/30/2025

            string saveName = "TrustEOY Primary";
            string skinName = "";

            SetupSelectedColumns("TrustEOY", "Primary", dgv);
            foundLocalPreference = G1.RestoreGridLayout(this, this.dgv, gridMain, LoginForm.username, saveName, ref skinName);

            workFormat = "Primary";
            loadGroupCombo(cmbSelectColumns, "TrustEOY", workFormat);
            cmbSelectColumns.Text = workFormat;

            chkRestoreDetail.Hide();

            loading = false;
        }
        /****************************************************************************************/
        private Font LucidaFont = new Font("Lucida Console", 7.8F); // <--- New as of 04/30/2025
        private void SetupPrintView (DevExpress.XtraGrid.Views.BandedGrid.BandedGridView gMain) // <--- New as of 04/30/2025
        {
            G1.AddNewColumn(gMain, "printLoc", "Location", "", FormatType.None, 100, false);

            gMain.Appearance.GroupRow.Font = LucidaFont;
            gMain.AppearancePrint.GroupRow.Font = LucidaFont;
            gMain.CustomDrawGroupRow += new DevExpress.XtraGrid.Views.Base.RowObjectCustomDrawEventHandler(this.gridMain_CustomDrawGroupRow);
        }
        /****************************************************************************************/
        private void SetupTotalsSummary()
        {
            gridMain.OptionsView.ShowFooter = true;
            gridMain2.OptionsView.ShowFooter = true;
            gridMain3.OptionsView.ShowFooter = true;
            gridMain4.OptionsView.ShowFooter = true;
            gridMain5.OptionsView.ShowFooter = true;
            gridMain6.OptionsView.ShowFooter = true;
            gridMain9.OptionsView.ShowFooter = true;
            //gridMain.Columns["value"].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Custom;

            AddSummaryColumn("beginningBalance", gridMain);
            AddSummaryColumn("trust85", gridMain);
            AddSummaryColumn("trust50", gridMain);
            AddSummaryColumn("contractValue", gridMain);
            AddSummaryColumn("allowInsurance", gridMain);
            AddSummaryColumn("annuity", gridMain);
            AddSummaryColumn("TandI", gridMain, "{0:N0}");
            AddSummaryColumn("IandA", gridMain, "{0:N0}");
            AddSummaryColumn("trustOnly", gridMain, "{0:N0}");
            AddSummaryColumn("insOnly", gridMain, "{0:N0}");
            AddSummaryColumn("annOnly", gridMain, "{0:N0}");

            AddSummaryColumn("L_contracts", gridMain2, "{0:N0}");
            AddSummaryColumn("L_contractValue", gridMain2);
            AddSummaryColumn("L_total", gridMain2);
            AddSummaryColumn("L_trust50", gridMain2);
            AddSummaryColumn("L_trust5085", gridMain2);
            AddSummaryColumn("total", gridMain2 );
            AddSummaryColumn("trust50", gridMain2);
            AddSummaryColumn("trust5085", gridMain2);
            AddSummaryColumn("contracts", gridMain2, "{0:N0}");
            AddSummaryColumn("contractValue", gridMain2);
            AddSummaryColumn("allowInsurance", gridMain2);
            AddSummaryColumn("annuity", gridMain2);
            AddSummaryColumn("totalLoc", gridMain2);
            AddSummaryColumn("balanceDue", gridMain2);
            AddSummaryColumn("TandI", gridMain2, "{0:N0}");
            AddSummaryColumn("IandA", gridMain2, "{0:N0}");
            AddSummaryColumn("trustOnly", gridMain2, "{0:N0}");
            AddSummaryColumn("insOnly", gridMain2, "{0:N0}");
            AddSummaryColumn("annOnly", gridMain2, "{0:N0}");
            AddSummaryColumn("A_contracts", gridMain2);
            AddSummaryColumn("A_remainingBal", gridMain2);

            AddSummaryColumn("beginningBalance", gridMain3);
            AddSummaryColumn("trust85", gridMain3);
            AddSummaryColumn("trust50", gridMain3);
            AddSummaryColumn("contractValue", gridMain3);
            AddSummaryColumn("allowInsurance", gridMain3);
            AddSummaryColumn("annuity", gridMain3);
            AddSummaryColumn("balanceDue", gridMain3);
            AddSummaryColumn("TandI", gridMain3, "{0:N0}");
            AddSummaryColumn("IandA", gridMain3, "{0:N0}");
            AddSummaryColumn("trustOnly", gridMain3, "{0:N0}");
            AddSummaryColumn("insOnly", gridMain3, "{0:N0}");
            AddSummaryColumn("annOnly", gridMain3, "{0:N0}");

            AddSummaryColumn("beginningBalance", gridMain4);
            AddSummaryColumn("trust85", gridMain4);
            AddSummaryColumn("trust50", gridMain4);
            AddSummaryColumn("contractValue", gridMain4);
            AddSummaryColumn("allowInsurance", gridMain4);
            AddSummaryColumn("annuity", gridMain4);
            AddSummaryColumn("balanceDue", gridMain4);
            AddSummaryColumn("TandI", gridMain4, "{0:N0}");
            AddSummaryColumn("IandA", gridMain4, "{0:N0}");
            AddSummaryColumn("trustOnly", gridMain4, "{0:N0}");
            AddSummaryColumn("insOnly", gridMain4, "{0:N0}");
            AddSummaryColumn("annOnly", gridMain4, "{0:N0}");

            AddSummaryColumn("beginningBalance", gridMain5);
            AddSummaryColumn("trust85", gridMain5);
            AddSummaryColumn("trust50", gridMain5);
            AddSummaryColumn("contractValue", gridMain5);
            AddSummaryColumn("allowInsurance", gridMain5);
            AddSummaryColumn("annuity", gridMain5);
            AddSummaryColumn("balanceDue", gridMain5);
            AddSummaryColumn("TandI", gridMain5, "{0:N0}");
            AddSummaryColumn("IandA", gridMain5, "{0:N0}");
            AddSummaryColumn("trustOnly", gridMain5, "{0:N0}");
            AddSummaryColumn("insOnly", gridMain5, "{0:N0}");
            AddSummaryColumn("annOnly", gridMain5, "{0:N0}");

            AddSummaryColumn("beginningBalance", gridMain6);
            AddSummaryColumn("trust85", gridMain6);
            AddSummaryColumn("trust50", gridMain6);
            AddSummaryColumn("contractValue", gridMain6);
            AddSummaryColumn("allowInsurance", gridMain6);
            AddSummaryColumn("annuity", gridMain6);
            AddSummaryColumn("balanceDue", gridMain6);
            AddSummaryColumn("TandI", gridMain6, "{0:N0}");
            AddSummaryColumn("IandA", gridMain6, "{0:N0}");
            AddSummaryColumn("trustOnly", gridMain6, "{0:N0}");
            AddSummaryColumn("insOnly", gridMain6, "{0:N0}");
            AddSummaryColumn("annOnly", gridMain6, "{0:N0}");

            AddSummaryColumn("beginningBalance", gridMain7);
            AddSummaryColumn("trust85", gridMain7);
            AddSummaryColumn("trust50", gridMain7);
            AddSummaryColumn("contractValue", gridMain7);
            AddSummaryColumn("allowInsurance", gridMain7);
            AddSummaryColumn("annuity", gridMain7);
            AddSummaryColumn("balanceDue", gridMain7);
            AddSummaryColumn("TandI", gridMain7, "{0:N0}");
            AddSummaryColumn("IandA", gridMain7, "{0:N0}");
            AddSummaryColumn("trustOnly", gridMain7, "{0:N0}");
            AddSummaryColumn("insOnly", gridMain7, "{0:N0}");
            AddSummaryColumn("annOnly", gridMain7, "{0:N0}");

            AddSummaryColumn("beginningBalance", gridMain8);
            AddSummaryColumn("trust85", gridMain8);
            AddSummaryColumn("trust50", gridMain8);
            AddSummaryColumn("contractValue", gridMain8);
            AddSummaryColumn("allowInsurance", gridMain8);
            AddSummaryColumn("annuity", gridMain8);
            AddSummaryColumn("balanceDue", gridMain8);
            AddSummaryColumn("TandI", gridMain8, "{0:N0}");
            AddSummaryColumn("IandA", gridMain8, "{0:N0}");
            AddSummaryColumn("trustOnly", gridMain8, "{0:N0}");
            AddSummaryColumn("insOnly", gridMain8, "{0:N0}");
            AddSummaryColumn("annOnly", gridMain8, "{0:N0}");

            AddSummaryColumn("total", gridMain9);
            AddSummaryColumn("trust50", gridMain9);
            AddSummaryColumn("contracts", gridMain9, "{0:N0}");
            AddSummaryColumn("contractValue", gridMain9);
            AddSummaryColumn("allowInsurance", gridMain9);
            AddSummaryColumn("annuity", gridMain9);
            AddSummaryColumn("totalLoc", gridMain9);
            AddSummaryColumn("balanceDue", gridMain9);
            AddSummaryColumn("TandI", gridMain9, "{0:N0}");
            AddSummaryColumn("IandA", gridMain9, "{0:N0}");
            AddSummaryColumn("trustOnly", gridMain9, "{0:N0}");
            AddSummaryColumn("insOnly", gridMain9, "{0:N0}");
            AddSummaryColumn("annOnly", gridMain9, "{0:N0}");
        }
        /****************************************************************************************/
        private void AddSummaryColumn(string columnName)
        {
            gridMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            //gridMain.Columns[columnName].SummaryItem.DisplayFormat = "{0:0,0.00}";
            gridMain.Columns[columnName].SummaryItem.DisplayFormat = "{0:N2}";
        }
        /****************************************************************************************/
        private void AddSummaryColumn(string columnName, DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gMain = null, string format = "" )
        {
            if (gMain == null)
                gMain = gridMain;
            if (String.IsNullOrWhiteSpace(format))
                format = "{0:N2}";
            //            gMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            gMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            //            gMain.Columns[columnName].SummaryItem.DisplayFormat = "${0:0,0.00}";
            gMain.Columns[columnName].SummaryItem.DisplayFormat = format;

            G1.AddSummaryItem(gMain, columnName);
        }
        /***********************************************************************************************/
        private void loadLocatons(DataTable dt = null)
        {
            string loc = "";
            string desc = "";

            DataRow dRow = null;
            DataRow[] dRows = null;

            if (dt != null) // <--- Added this section to fix selecting location
            {
                DataRow tempDrow = funDt.Rows[0];
                DataTable fDt = funDt.Clone();
                DataTable dx = G1.GetGroupBy(dt, "location");
                for (int i = 0; i < dx.Rows.Count; i++)
                {
                    loc = dx.Rows[i]["location"].ObjToString();
                    if (String.IsNullOrWhiteSpace(loc))
                        continue;
                    dRows = funDt.Select("keycode='" + loc + "'");
                    if (dRows.Length > 0)
                    {
                        fDt.ImportRow(dRows[0]);
                    }
                    else
                    {
                        tempDrow["keycode"] = loc;
                        tempDrow["LocationCode"] = loc;
                        fDt.ImportRow(tempDrow);
                    }
                }
                chkComboLocNames.Properties.DataSource = fDt;
                chkComboLocNames.Refresh();
                return;
            }

            string cmd = "Select * from `funeralhomes` order by `keycode`;";
            funDt = G1.get_db_data(cmd);

            cmd = "Select * from `cemeteries` order by `loc`;";
            cemDt = G1.get_db_data(cmd);
            
            for ( int i=0; i<cemDt.Rows.Count; i++)
            {
                loc = cemDt.Rows[i]["loc"].ObjToString();
                desc = cemDt.Rows[i]["description"].ObjToString();

                dRow = funDt.NewRow();
                dRow["keycode"] = loc;
                dRow["LocationCode"] = desc;
                dRow["name"] = loc + " " + desc;
                funDt.Rows.Add(dRow);
            }

            chkComboLocNames.Properties.DataSource = funDt;
        }
        /***********************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            // Makes the magnifying glass work on each tab.
            if (dgv.Visible)
                SetSpyGlass(gridMain);
            else if (dgv2.Visible)
                SetSpyGlass(gridMain2);
            else if (dgv3.Visible)
                SetSpyGlass(gridMain3);
            else if (dgv4.Visible)
                SetSpyGlass(gridMain4);
            else if (dgv5.Visible)
                SetSpyGlass(gridMain5);
            else if (dgv6.Visible)
                SetSpyGlass(gridMain6);
            else if (dgv7.Visible)
                SetSpyGlass(gridMain7);
            else if (dgv8.Visible)
                SetSpyGlass(gridMain8);
            else if (dgv9.Visible)
                SetSpyGlass(gridMain9);
        }
        /***********************************************************************************************/
        private void SetSpyGlass(DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView grid, string search = "" )
        {
            G1.ShowHideFindPanel(grid, search );
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
            if (e.IsForGroupRow ) // Use this for Group Headers ONLY
            {
                //if (e.Column.FieldName.ToUpper() == "LOCATION" )
                //    e.DisplayText = "<your text>";
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


            this.Cursor = Cursors.WaitCursor;
            printableComponentLink1.Component = dgv;
            if (dgv.Visible) // <--- New Stuff Here 
            {
                if (chkCollapes.Checked)
                    BuildPrintSummary( dgv );
            }
            if (dgv2.Visible) 
                printableComponentLink1.Component = dgv2;
            else if (dgv3.Visible) // <--- New Stuff Here 
            {
                printableComponentLink1.Component = dgv3;
                if (chkCollapes.Checked)
                    BuildPrintSummary(dgv3);
            }
            else if (dgv4.Visible) // <--- New Stuff Here 
            {
                printableComponentLink1.Component = dgv4;
                if (chkCollapes.Checked)
                    BuildPrintSummary(dgv4);
            }
            else if (dgv5.Visible) // <--- New Stuff Here 
            {
                printableComponentLink1.Component = dgv5;
                if (chkCollapes.Checked)
                    BuildPrintSummary(dgv5);
            }
            else if (dgv6.Visible) // <--- New Stuff Here 
            {
                printableComponentLink1.Component = dgv6;
                if (chkCollapes.Checked)
                    BuildPrintSummary(dgv6);
            }
            else if (dgv7.Visible) // <--- New Stuff Here 
            {
                printableComponentLink1.Component = dgv7;
                if (chkCollapes.Checked)
                    BuildPrintSummary(dgv7);
            }
            else if (dgv8.Visible) // <--- New Stuff Here 
            {
                printableComponentLink1.Component = dgv8;
                if (chkCollapes.Checked)
                    BuildPrintSummary(dgv8);
            }
            else if (dgv9.Visible)
                printableComponentLink1.Component = dgv9;

            this.Cursor = Cursors.Default;

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

            if (dgv.Visible && chkCollapes.Checked )
            {
                gridMain.Columns["printLoc"].GroupIndex = -1;
                gridMain.Columns["location"].GroupIndex = 0;
            }
        }
        /***********************************************************************************************/
        private void BuildPrintSummary ( GridControl dgv ) // New as of 4/30/2025
        {
            DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gMain = (DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView) dgv.MainView;
            DataTable dt = (DataTable)dgv.DataSource;

            if (G1.get_column_number(dt, "printLoc") < 0)
                dt.Columns.Add("printLoc");

            string location = "";
            string caption = "";
            double total = 0D;
            string str = "";

            int row = 0;
            int length = 0;
            string text = "";

            DataTable dx = G1.GetGroupBy(dt, "location");
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                location = dx.Rows[i]["location"].ObjToString();
                total = totalLocation( dt, location);
                total = G1.RoundValue(total);
                str = G1.ReformatMoney(total);

                caption = getLocationName(location);

                text = " (" + location + ") ";

                length = 40 - text.Length;
                if (length < 0)
                    length = 0;
                if ( str.Length < 13 )
                {
                    length += 13 - str.Length;
                }
                text += caption.PadRight(length);
                text += " $" + str;

                dt.Rows[i]["printLoc"] = text;
                row++;
            }

            gMain.Columns["location"].GroupIndex = -1;
            gMain.Columns["printLoc"].GroupIndex = 0;

            return;
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
            //string text = this.Text + " " + this.dateTimePicker1.Value.ToString("MM/dd/yyyy") + " - " + this.dateTimePicker2.Value.ToString("MM/dd/yyyy");
            string text = this.Text + " " +  this.dateTimePicker2.Value.ToString("MM/dd/yyyy");

            //Printer.DrawQuad(4, 7, 5, 4, text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);
            Printer.DrawQuad(4, 7, 6, 4, text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center); // <--- Had to increase width 5 to 6

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

            printingSystem1.Links.AddRange(new object[] {printableComponentLink1});
			
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
        private void gridMain_CustomRowFilter(object sender, RowFilterEventArgs e)
        {
            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv.DataSource;
//            string status = dt.Rows[row]["status"].ObjToString().Trim().ToUpper();
//            string trustType = dt.Rows[row]["trustType"].ObjToString().ToUpper();
            /*
            string showWhat = cmbType.Text.Trim().ToUpper();
            if ( showWhat == "TRUST")
            {
                if ( trustType != "TRUST")
                {
                    e.Visible = false;
                    e.Handled = true;
                    return;
                }
            }
            else if (showWhat == "INSURANCE")
            {
                if (trustType != "INSURANCE")
                {
                    e.Visible = false;
                    e.Handled = true;
                    return;
                }
            }
            else if (showWhat == "CEMETERY")
            {
                if (trustType != "CEMETERY")
                {
                    e.Visible = false;
                    e.Handled = true;
                    return;
                }
            }
            */
        }
        /***********************************************************************************************/
        private void gridMain_CustomDrawCell(object sender, RowCellCustomDrawEventArgs e)
        {
            if (e.Column.FieldName.ToUpper() == "NUM")
            {
                if (e.RowHandle >= 0)
                {
                    string num = (e.RowHandle + 1).ToString();
                    e.DisplayText = num;
                }
            }
        }
        /***********************************************************************************************/
        private void btnLeft_Click(object sender, EventArgs e)
        {
            DateTime now = this.dateTimePicker2.Value;
            DateTime date = new DateTime(now.Year, now.Month, 1);
            date = date.AddMonths(-1);
            //this.dateTimePicker1.Value = date;
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            date = new DateTime(date.Year, date.Month, days);
            this.dateTimePicker2.Value = date;
        }
        /***********************************************************************************************/
        private void btnRight_Click(object sender, EventArgs e)
        {
            DateTime now = this.dateTimePicker2.Value;
            DateTime date = new DateTime(now.Year, now.Month, 1);
            date = date.AddMonths(1);
            //this.dateTimePicker1.Value = date;
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            date = new DateTime(date.Year, date.Month, days);
            this.dateTimePicker2.Value = date;
        }
        /***********************************************************************************************/
        private void btnRun_Click(object sender, EventArgs e)
        {
            // This needs to pull from trust2013r
            this.Cursor = Cursors.WaitCursor;

            DateTime date = dateTimePicker2.Value;
            date = new DateTime(date.Year, date.Month, 1);
            DateTime saveDate1 = date;
            string date1 = G1.DateTimeToSQLDateTime(date);
            date = dateTimePicker2.Value;
            DateTime saveDate2 = date;
            string date2 = G1.DateTimeToSQLDateTime(date);

            string cmd = "SELECT * FROM `trust2013r` t LEFT JOIN `customers` c ON t.`contractNumber` = c.`contractNumber` LEFT JOIN `contracts` r ON t.`contractNumber` = r.`contractNumber`";
            cmd += " WHERE `payDate8` >= '" + date1 + "' AND `payDate8` <= '" + date2 + "' ";
            cmd += " ORDER by t.`payDate8`, t.`contractNumber`";
            cmd += ";";

            DataTable dt = G1.get_db_data(cmd);

            string is2002 = "";
            double balance = 0D;
            double interest = 0D;
            double removals = 0D;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                is2002 = dt.Rows[i]["is2002"].ObjToString();
                if (string.IsNullOrWhiteSpace(is2002))
                {
                    balance = dt.Rows[i]["endingBalance"].ObjToDouble();
                    interest = dt.Rows[i]["interest"].ObjToDouble();
                    removals = dt.Rows[i]["currentRemovals"].ObjToDouble();
                    if (removals == 0D)
                        dt.Rows[i]["endingBalance"] = balance + interest;
                }
            }

            DataRow[] dRows = dt.Select("endingBalance > '0.00' and currentRemovals = '0.00'");
            if (dRows.Length > 0)
                dt = dRows.CopyToDataTable();


            if (chkIncludeSMFS.Checked)
                dt = pullSMFSdata(dt);

            dt = processNewStuff(dt);

            dt = processTheData(dt);


            G1.NumberDataTable(dt);

            gridMain.Columns["location"].GroupIndex = 0;
            gridMain.OptionsView.ShowFooter = false;
            gridMain.OptionsView.GroupFooterShowMode = DevExpress.XtraGrid.Views.Grid.GroupFooterShowMode.VisibleIfExpanded;

            //gridMain9.Columns["location"].GroupIndex = 0;
            //gridMain9.OptionsView.ShowFooter = false;
            //gridMain9.OptionsView.GroupFooterShowMode = DevExpress.XtraGrid.Views.Grid.GroupFooterShowMode.VisibleIfExpanded;

            originalDt = dt;

            dt = processLocations(dt);

            originalDt = dt;
            
            loadLocatons(dt); // <--- Added this here to capture any location available in dt

            dt = RemoveNoContracts(dt, true);

            dgv.DataSource = dt;

            buildSummary(dt);

            buildFullSummary(dt);
            
            chkCollapes_CheckedChanged(null, null);
            //gridMain.ExpandAllGroups();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private DataTable RemoveNoContracts ( DataTable dt, bool load7 )
        {
            DataRow[] dRows = null;
            if (load7)
            {
                dRows = dt.Select("serviceTotal is null");
                if (dRows.Length > 0)
                {
                    DataTable xDt = dRows.CopyToDataTable();
                    Trust85.FindContract(xDt, "ANS980107");
                    G1.NumberDataTable(xDt);
                    dgv7.DataSource = xDt;
                }
            }

            dRows = dt.Select("serviceTotal is not null");
            if (dRows.Length > 0)
                dt = dRows.CopyToDataTable();
            return dt;
        }
        /***********************************************************************************************/
        private double TrustPayments ( string contractNumber )
        {
            double totalPayments = 0D;
            if (String.IsNullOrWhiteSpace(contractNumber))
                return totalPayments;

            string cmd = "Select * from `payments` where `contractNumber` = '" + contractNumber + "' order by `paydate8` DESC, `tmstamp` DESC;";
            DataTable dp = G1.get_db_data(cmd);
            if (dp.Rows.Count <= 0)
            {
                totalPayments = DailyHistory.GetDownPayment(contractNumber);
                return totalPayments;
            }

            double payment = 0D;
            double downPayment = 0D;
            double debit = 0D;
            double credit = 0D;
            bool gotDownPayment = false;
            for ( int i=0; i<dp.Rows.Count; i++)
            {

                payment = dp.Rows[i]["downPayment"].ObjToDouble();
                if ( payment > 0D )
                {
                    gotDownPayment = true;
                    downPayment += payment;
                }
                debit = dp.Rows[i]["debitAdjustment"].ObjToDouble();
                credit = dp.Rows[i]["creditAdjustment"].ObjToDouble();
                payment = dp.Rows[i]["paymentAmount"].ObjToDouble();
                totalPayments += payment + credit - debit;
            }

            if ( !gotDownPayment )
            {
                payment = DailyHistory.GetDownPayment(contractNumber);
                totalPayments += payment;
            }
            return totalPayments;
        }
        /***********************************************************************************************/
        private DataTable processNewStuff ( DataTable dt)
        {
            double endingBalance = 0D;
            string contractNumber = "";
            double contractValue = 0D;
            double allowInsurance = 0D;
            double annuity = 0D;
            if ( G1.get_column_number ( dt, "contractValue") < 0 )
                dt.Columns.Add("contractValue", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "annuity") < 0)
                dt.Columns.Add("annuity", Type.GetType("System.Double"));

            DateTime lapsedDate = DateTime.Now;
            DateTime reinstateDate = DateTime.Now;
            string lapsed = "";
            string str = "";

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                contractValue = DailyHistory.GetContractValueMinus(dt.Rows[i]);
                dt.Rows[i]["contractValue"] = contractValue;
                allowInsurance = dt.Rows[i]["allowInsurance"].ObjToDouble();
                //if ( allowInsurance > 0D )
                //{
                    if ( contractNumber.ToUpper().IndexOf ( "ANF") == 0 || contractNumber.ToUpper().IndexOf ( "ANS") == 0 )
                    {
                        //annuity = allowInsurance;
                        //dt.Rows[i]["allowInsurance"] = 0D;
                        dt.Rows[i]["annuity"] = TrustPayments(contractNumber);
                    }
                //}

                lapsed = dt.Rows[i]["lapsed"].ObjToString();
                str = dt.Rows[i]["lapseDate8"].ObjToString();
                if (str == "0/0/0000")
                    str = "";
                if (str.IndexOf("0001") > 0 )
                    str = "";

                if ( !String.IsNullOrWhiteSpace ( str ) )
                {
                    lapsedDate = dt.Rows[i]["lapseDate8"].ObjToDateTime();
                    reinstateDate = dt.Rows[i]["reinstateDate8"].ObjToDateTime();
                    if (reinstateDate.Year > 1000)
                    {
                        if ( lapsedDate > reinstateDate )
                            dt.Rows[i]["lapsed"] = lapsedDate.ToString("MM/dd/yyyy");
                    }
                    else
                        dt.Rows[i]["lapsed"] = lapsedDate.ToString("MM/dd/yyyy");
                }
                //dt.Rows[i]["currentRemovals"] = 0D;
            }
            return dt;
        }
        /***********************************************************************************************/
        private DataTable pullSMFSdata(DataTable dt)
        {
            DateTime date = dateTimePicker2.Value;
            date = new DateTime(date.Year, date.Month, 1);
            DateTime saveDate1 = date;
            string date1 = G1.DateTimeToSQLDateTime(date);
            date = dateTimePicker2.Value;
            DateTime saveDate2 = date;
            string date2 = G1.DateTimeToSQLDateTime(date);

            string contractNumber = "";
            string contract = "";
            string trust = "";
            string loc = "";
            int year = 0;

            try
            {
                string cmd = "SELECT t.tmstamp,t.contractNumber,t.firstName, t.lastname,t.address1 as address2013,t.city as city2013, t.state as state2013, t.zip1 as zip2013,t.ssn as ssn2013, ";
                cmd += "c.lastDatePaid8 as payDate8, c.balanceDue, c.ServiceId, c.serviceTotal, c.merchandiseTotal,c.allowMerchandise,c.allowInsurance,c.downPayment,c.cashAdvance ";
                cmd += " FROM `customers` t LEFT JOIN `contracts` c ON t.`contractNumber` = c.`contractNumber`";
                cmd += " WHERE (t.`deceasedDate` >= '" + date2 + "' OR t.`deceasedDate` < '19101231' ) AND t.`contractNumber` <> '' ";
                cmd += " ORDER by t.`contractNumber`";
                cmd += ";";

                DataTable dx = G1.get_db_data(cmd);

                if (dx.Rows.Count <= 0)
                    return dt;

                dx.Columns.Add("Is2002");

                for (int i = 0; i < dx.Rows.Count; i++)
                {
                    contractNumber = dx.Rows[i]["contractNumber"].ObjToString();
                    //if (contractNumber == "CO2012")
                    //{
                    //}
                    contract = Trust85.decodeContractNumber(contractNumber, ref trust, ref loc);
                    if (contract.Length > 2)
                    {
                        contract = contract.Substring(0, 2);
                        if (G1.validate_numeric(contract))
                        {
                            year = contract.ObjToInt32();
                            if (year > 2)
                                dx.Rows[i]["Is2002"] = "2002";
                        }
                    }

                }

                dt.Merge(dx);

                dt = G1.RemoveDuplicates(dt, "contractNumber");


            }
            catch ( Exception ex)
            {
            }
            return dt;
        }
        /***********************************************************************************************/
        private DataTable findDifferences ( DataTable dt2, DataTable dx2 )
        {
            var differences = dt2.AsEnumerable().Except(dx2.AsEnumerable(), DataRowComparer.Default);
            return differences.Any() ? differences.CopyToDataTable() : new DataTable();
        }
        /***********************************************************************************************/
        private void buildSummary(DataTable dx)
        {
            DataTable dt = dx.Clone();

            DataRow[] dRows = dx.Select("trustOnly='1' OR TandI='1'");
            if (dRows.Length > 0)
                dt = dRows.CopyToDataTable();

            string location = "";
            string oldLoc = "";
            string locInd = "";
            string oldLocInd = "";
            string is2002 = "";
            string oldIs2002 = "";
            string serviceLoc = "";
            string oldServiceLoc = "";

            DataRow dRow = null;
            
            double contracts = 0D;
            double total = 0D;
            double trust50 = 0D;
            double contractValue = 0D;
            double trust5085 = 0D;
            double allowInsurance = 0D;
            double annuity = 0D;
            double balanceDue = 0D;

            double tValue = 0D;
            double iValue = 0D;
            double aValue = 0D;
            double tiValue = 0D;
            double aiValue = 0D;

            double L_contracts = 0D;
            double L_total = 0D;
            double L_trust50 = 0D;
            double L_contractValue = 0D;
            double L_trust5085 = 0D;
            double L_allowInsurance = 0D;
            double L_annuity = 0D;
            double L_balanceDue = 0D;

            double L_tValue = 0D;
            double L_iValue = 0D;
            double L_aValue = 0D;
            double L_tiValue = 0D;
            double L_aiValue = 0D;

            double T_contracts = 0D;
            double T_total = 0D; // Trust 85
            double T_trust50 = 0D;
            double T_trust5085 = 0D;
            double T_contractValue = 0D;

            double I_contracts = 0D;  // Total number of contracts with a dueDate8 of 12-31-2039
            DateTime dueDate = DateTime.MinValue;
            
            //string formattedDateOnly = eventDate.ToString("yyyy-MM-dd");

            double A_contracts = 0D; // Total # of Actively Paid Contracts 
            double A_remainingBal = 0D; // Remaining Balance of Actively Paying Contracts

            double totals = 0D;
            double contractTotals = 0D;
            double contractValues = 0D;
            double allowInsurances = 0D;
            double annuitys = 0D;
            double balanceDues = 0D;

            string lapsed = "";
            string str_dueDate = "";

            DataView tempView = dt.DefaultView;
            tempView.Sort = "location";
            dt = tempView.ToTable();

            DataTable dt2 = new DataTable();
            dt2.Columns.Add("L_contracts", Type.GetType("System.Double"));
            dt2.Columns.Add("L_contractValue", Type.GetType("System.Double"));
            dt2.Columns.Add("L_trust50", Type.GetType("System.Double"));
            dt2.Columns.Add("L_total", Type.GetType("System.Double"));
            dt2.Columns.Add("L_trust5085", Type.GetType("System.Double"));
            dt2.Columns.Add("contracts", Type.GetType("System.Double"));
            dt2.Columns.Add("total", Type.GetType("System.Double"));
            dt2.Columns.Add("trust50", Type.GetType("System.Double"));
            dt2.Columns.Add("contractValue", Type.GetType("System.Double"));
            dt2.Columns.Add("trust5085", Type.GetType("System.Double"));
            dt2.Columns.Add("allowInsurance", Type.GetType("System.Double"));
            dt2.Columns.Add("annuity", Type.GetType("System.Double"));
            dt2.Columns.Add("balanceDue", Type.GetType("System.Double"));
            dt2.Columns.Add("totalLoc", Type.GetType("System.Double"));
            dt2.Columns.Add("location");
            dt2.Columns.Add("T_contracts", Type.GetType("System.Double"));
            dt2.Columns.Add("T_total", Type.GetType("System.Double"));
            dt2.Columns.Add("T_trust50", Type.GetType("System.Double"));
            dt2.Columns.Add("T_trust5085", Type.GetType("System.Double"));
            dt2.Columns.Add("T_contractValue", Type.GetType("System.Double"));
            dt2.Columns.Add("A_contracts", Type.GetType("System.Double"));
            dt2.Columns.Add("A_remainingBal", Type.GetType("System.Double"));
            dt2.Columns.Add("I_contracts", Type.GetType("System.Double")); // Total number of contracts that are inactive with a dueDate of 12-31-2039
            dt2.Columns.Add("dueDate", Type.GetType("System.DateTime"));

            dt2.Columns.Add("TandI", Type.GetType("System.Double"));
            dt2.Columns.Add("insOnly", Type.GetType("System.Double"));
            dt2.Columns.Add("trustOnly", Type.GetType("System.Double"));
            dt2.Columns.Add("annOnly", Type.GetType("System.Double"));
            dt2.Columns.Add("IandA", Type.GetType("System.Double"));

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    location = dt.Rows[i]["location"].ObjToString();
                    serviceLoc = dt.Rows[i]["serviceLoc"].ObjToString();
                    is2002 = dt.Rows[i]["is2002"].ObjToString();
                    locInd = dt.Rows[i]["locInd"].ObjToString();
                    lapsed = dt.Rows[i]["lapsed"].ObjToString();
                    dueDate = dt.Rows[i]["dueDate8"].ObjToDateTime();
                    str_dueDate = dueDate.ToString("yyyy-MM-dd");
                    if (str_dueDate == "2039-12-31")
                        I_contracts++; // It's an inactive contract. Increment the total number of inactive contracts to subtract from Total Contracts.
                    else
                    {
                        if (L_contracts != 0) // Not inactive and not lapsed.
                            A_remainingBal += dt.Rows[i]["balanceDue"].ObjToDouble();
                    }

                    if (string.IsNullOrWhiteSpace(oldLoc))
                    {
                        oldLoc = location;
                        oldIs2002 = is2002;
                        oldLocInd = locInd;
                        oldServiceLoc = serviceLoc;
                    }
                    
                    if (oldLoc != location)
                    {
                        dRow = dt2.NewRow();
                        dRow["location"] = getLocation(oldLoc);
                        dRow["location"] = filterServiceLoc(oldServiceLoc);
                        dRow["L_contracts"] = L_contracts;
                        dRow["L_contractValue"] = L_contractValue;
                        dRow["L_trust50"] = L_trust50;
                        dRow["L_total"] = L_total;
                        dRow["L_trust5085"] = L_trust5085;
                        dRow["contracts"] = contracts;
                        dRow["total"] = total;
                        dRow["trust50"] = trust50;
                        dRow["contractValue"] = contractValue;
                        dRow["trust5085"] = trust5085;
                        dRow["allowInsurance"] = allowInsurance;
                        dRow["annuity"] = annuity;
                        dRow["balanceDue"] = balanceDue;
                        dRow["T_contracts"] = T_contracts;
                        dRow["T_total"] = T_total;
                        dRow["T_trust50"] = T_trust50;
                        dRow["T_trust5085"] = T_trust5085;
                        dRow["T_contractValue"] = T_contractValue;
                        dRow["A_contracts"] = A_contracts;
                        dRow["A_remainingBal"] = A_remainingBal;

                        dRow["trustOnly"] = tValue;
                        dRow["insOnly"] = iValue;
                        dRow["annOnly"] = aValue;
                        dRow["TandI"] = tiValue;
                        dRow["IandA"] = aiValue;

                        dt2.Rows.Add(dRow);
                        /*
                         * 7-8-2025 - Adam Sloan - Removed extra row for lapsed content and created extra columns instead.
                        if (L_contracts != 0D)
                        {
                            dRow = dt2.NewRow();
                            dRow["location"] = getLocation(oldLoc);
                            dRow["location"] = filterServiceLoc(oldServiceLoc) + " Lapsed";
                            dRow["contracts"] = L_contracts;
                            dRow["total"] = L_total;
                            dRow["trust50"] = L_trust50;
                            dRow["contractValue"] = L_contractValue;
                            dRow["trust5085"] = L_trust5085;
                            dRow["allowInsurance"] = L_allowInsurance;
                            dRow["annuity"] = L_annuity;
                            dRow["balanceDue"] = L_balanceDue;

                            dRow["trustOnly"] = L_tValue;
                            dRow["insOnly"] = L_iValue;
                            dRow["annOnly"] = L_aValue;
                            dRow["TandI"] = L_tiValue;
                            dRow["IandA"] = L_aiValue;

                            dt2.Rows.Add(dRow);
                        }
                        */
                        totals += total;
                        contractValues += contractValue;
                        allowInsurances += allowInsurance;
                        annuitys += annuity;
                        contractTotals += contracts;

                        contracts = 0D;
                        total = 0D;
                        trust50 = 0D;
                        trust5085 = 0D;
                        contractValue = 0D;
                        allowInsurance = 0D;
                        annuity = 0D;
                        balanceDue = 0D;

                        tValue = 0D;
                        iValue = 0D;
                        aValue = 0D;
                        tiValue = 0D;
                        aiValue = 0D;

                        L_contracts = 0D;
                        L_total = 0D;
                        L_trust50 = 0D;
                        L_trust5085 = 0D;
                        L_contractValue = 0D;
                        L_allowInsurance = 0D;
                        L_annuity = 0D;
                        L_balanceDue = 0D;

                        L_tValue = 0D;
                        L_iValue = 0D;
                        L_aValue = 0D;
                        L_tiValue = 0D;
                        L_aiValue = 0D;

                        T_contracts = 0D;
                        T_total = 0D;
                        T_trust50 = 0D;
                        T_trust5085 = 0D;
                        T_contractValue = 0D;

                        A_contracts = 0D;
                        A_remainingBal = 0D;

                        oldLoc = location;
                        oldServiceLoc = serviceLoc;
                    }
                    if (!String.IsNullOrWhiteSpace(lapsed))
                    {
                        L_contracts++;
                        //L_total += dt.Rows[i]["endingBalance"].ObjToDouble();
                        L_total += dt.Rows[i]["trust85"].ObjToDouble();
                        L_trust50 += dt.Rows[i]["trust50"].ObjToDouble();
                        L_trust5085 = L_trust50 + L_total;
                        L_contractValue += dt.Rows[i]["contractValue"].ObjToDouble();
                        L_allowInsurance += dt.Rows[i]["allowInsurance"].ObjToDouble();
                        L_annuity += dt.Rows[i]["annuity"].ObjToDouble();
                        L_balanceDue += dt.Rows[i]["balanceDue"].ObjToDouble();

                        L_tValue += dt.Rows[i]["trustOnly"].ObjToDouble();
                        L_iValue += dt.Rows[i]["insOnly"].ObjToDouble();
                        L_aValue += dt.Rows[i]["annOnly"].ObjToDouble();
                        L_tiValue += dt.Rows[i]["TandI"].ObjToDouble();
                        L_aiValue += dt.Rows[i]["IandA"].ObjToDouble();
                    }
                    else
                    {
                        contracts++;
                        //total += dt.Rows[i]["endingBalance"].ObjToDouble();
                        total += dt.Rows[i]["trust85"].ObjToDouble();
                        trust50 += dt.Rows[i]["trust50"].ObjToDouble();
                        trust5085 = trust50 + total;
                        contractValue += dt.Rows[i]["contractValue"].ObjToDouble();
                        allowInsurance += dt.Rows[i]["allowInsurance"].ObjToDouble();
                        annuity += dt.Rows[i]["annuity"].ObjToDouble();
                        balanceDue += dt.Rows[i]["balanceDue"].ObjToDouble();

                        tValue += dt.Rows[i]["trustOnly"].ObjToDouble();
                        iValue += dt.Rows[i]["insOnly"].ObjToDouble();
                        aValue += dt.Rows[i]["annOnly"].ObjToDouble();
                        tiValue += dt.Rows[i]["TandI"].ObjToDouble();
                        aiValue += dt.Rows[i]["IandA"].ObjToDouble();
                    }

                    // Totals
                    T_contracts = L_contracts + contracts;
                    T_total = L_total + total;
                    T_trust50 = L_trust50 + trust50;
                    T_trust5085 = L_total + L_trust50;
                    T_contractValue = L_contractValue + contractValue;

                    // Calculate the remaining Active Balance
                    A_contracts = contracts - L_contracts - I_contracts;
//                    A_remainingBal = balanceDue - L_balanceDue;
                    I_contracts = 0D;
                }
                catch (Exception ex)
                { 
                }
            }

            if (contracts != 0D)
            {
                dRow = dt2.NewRow();
                dRow["location"] = getLocation(oldLoc);
				dRow["location"] = filterServiceLoc(oldServiceLoc);
                dRow["L_contracts"] = L_contracts;
                dRow["L_contractValue"] = L_contractValue;
                dRow["L_trust50"] = L_trust50;
                dRow["L_total"] = L_total;
                dRow["L_trust5085"] = L_trust5085;
                dRow["contracts"] = contracts;
                dRow["total"] = total;
                dRow["trust50"] = trust50;
                dRow["contractValue"] = contractValue;
                dRow["trust5085"] = trust5085;
                dRow["allowInsurance"] = allowInsurance;
                dRow["annuity"] = annuity;
                dRow["balanceDue"] = balanceDue;
                dt2.Rows.Add(dRow);
                /*
                if ( L_contracts != 0D )
                {
                    dRow = dt2.NewRow();
                    dRow["location"] = getLocation(oldLoc);
                    dRow["location"] = filterServiceLoc(oldServiceLoc) + " Lapsed";
                    dRow["contracts"] = L_contracts;
                    dRow["total"] = L_total;
                    dRow["trust50"] = L_trust50;
                    dRow["contractValue"] = L_contractValue;
                    dRow["trust5085"] = L_trust5085;
                    dRow["allowInsurance"] = L_allowInsurance;
                    dRow["annuity"] = L_annuity;
                    dRow["balanceDue"] = L_balanceDue;
                    dt2.Rows.Add(dRow);
                }
                */
                totals += total;
                contractTotals += contracts;
            }

            double trust85 = 0D;
            for ( int i=0; i<dt2.Rows.Count; i++)
            {
                trust50 = dt2.Rows[i]["trust50"].ObjToDouble();
                trust85 = dt2.Rows[i]["total"].ObjToDouble();
                allowInsurance = dt2.Rows[i]["allowInsurance"].ObjToDouble();
                annuity = dt2.Rows[i]["annuity"].ObjToDouble();

                total = trust50 + trust85 + allowInsurance + annuity;
                dt2.Rows[i]["totalLoc"] = total;
            }

            dgv2.DataSource = dt2;
        }
        /***********************************************************************************************/
        private void buildFullSummary(DataTable dt)
        {

            //DataTable dt = dx.Clone();

            //DataRow[] dRows = dx.Select("trustOnly='1' OR TandI='1'");
            //if (dRows.Length > 0)
            //    dt = dRows.CopyToDataTable();

            string location = "";
            string oldLoc = "";
            string locInd = "";
            string oldLocInd = "";
            string is2002 = "";
            string oldIs2002 = "";
            string serviceLoc = "";
            string oldServiceLoc = "";

            DataRow dRow = null;

            double contracts = 0D;
            double total = 0D;
            double trust50 = 0D;
            double contractValue = 0D;
            double trust5085 = 0D;
            double allowInsurance = 0D;
            double annuity = 0D;
            double balanceDue = 0D;

            double tValue = 0D;
            double iValue = 0D;
            double aValue = 0D;
            double tiValue = 0D;
            double aiValue = 0D;


            double L_contracts = 0D;
            double L_total = 0D;
            double L_trust50 = 0D;
            double L_contractValue = 0D;
            double L_trust5085 = 0D;
            double L_allowInsurance = 0D;
            double L_annuity = 0D;
            double L_balanceDue = 0D;

            double L_tValue = 0D;
            double L_iValue = 0D;
            double L_aValue = 0D;
            double L_tiValue = 0D;
            double L_aiValue = 0D;

            double totals = 0D;
            double contractTotals = 0D;
            double contractValues = 0D;
            double trust5085s = 0D;
            double allowInsurances = 0D;
            double annuitys = 0D;
            double balanceDues = 0D;

            string lapsed = "";

            DataView tempView = dt.DefaultView;
            tempView.Sort = "location";
            dt = tempView.ToTable();

            DataTable dt2 = new DataTable();
            dt2.Columns.Add("contracts", Type.GetType("System.Double"));
            dt2.Columns.Add("total", Type.GetType("System.Double"));
            dt2.Columns.Add("trust50", Type.GetType("System.Double"));
            dt2.Columns.Add("contractValue", Type.GetType("System.Double"));
            dt2.Columns.Add("trust5085", Type.GetType("System.Double"));
            dt2.Columns.Add("allowInsurance", Type.GetType("System.Double"));
            dt2.Columns.Add("annuity", Type.GetType("System.Double"));
            dt2.Columns.Add("balanceDue", Type.GetType("System.Double"));
            dt2.Columns.Add("totalLoc", Type.GetType("System.Double"));
            dt2.Columns.Add("location");

            dt2.Columns.Add("TandI", Type.GetType("System.Double"));
            dt2.Columns.Add("insOnly", Type.GetType("System.Double"));
            dt2.Columns.Add("trustOnly", Type.GetType("System.Double"));
            dt2.Columns.Add("annOnly", Type.GetType("System.Double"));
            dt2.Columns.Add("IandA", Type.GetType("System.Double"));

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    location = dt.Rows[i]["location"].ObjToString();
                    serviceLoc = dt.Rows[i]["serviceLoc"].ObjToString();
                    is2002 = dt.Rows[i]["is2002"].ObjToString();
                    locInd = dt.Rows[i]["locInd"].ObjToString();
                    lapsed = dt.Rows[i]["lapsed"].ObjToString();

                    if (string.IsNullOrWhiteSpace(oldLoc))
                    {
                        oldLoc = location;
                        oldIs2002 = is2002;
                        oldLocInd = locInd;
                        oldServiceLoc = serviceLoc;
                    }
                    
                    if (oldLoc != location)
                    {
                        dRow = dt2.NewRow();
                        dRow["location"] = getLocation(oldLoc);
                        dRow["location"] = filterServiceLoc(oldServiceLoc);
                        dRow["contracts"] = contracts;
                        dRow["total"] = total;
                        dRow["trust50"] = trust50;
                        dRow["contractValue"] = contractValue;
                        dRow["trust5085"] = trust5085;
                        dRow["allowInsurance"] = allowInsurance;
                        dRow["annuity"] = annuity;
                        dRow["balanceDue"] = balanceDue;

                        dRow["trustOnly"] = tValue;
                        dRow["insOnly"] = iValue;
                        dRow["annOnly"] = aValue;
                        dRow["TandI"] = tiValue;
                        dRow["IandA"] = aiValue;

                        dt2.Rows.Add(dRow);

                        if (L_contracts != 0D)
                        {
                            dRow = dt2.NewRow();
                            dRow["location"] = getLocation(oldLoc);
                            dRow["location"] = filterServiceLoc(oldServiceLoc) + " Lapsed";
                            dRow["contracts"] = L_contracts;
                            dRow["total"] = L_total;
                            dRow["trust50"] = L_trust50;
                            dRow["contractValue"] = L_contractValue;
                            dRow["trust5085"] = L_trust5085;
                            dRow["allowInsurance"] = L_allowInsurance;
                            dRow["annuity"] = L_annuity;
                            dRow["balanceDue"] = L_balanceDue;

                            dRow["trustOnly"] = L_tValue;
                            dRow["insOnly"] = L_iValue;
                            dRow["annOnly"] = L_aValue;
                            dRow["TandI"] = L_tiValue;
                            dRow["IandA"] = L_aiValue;

                            dt2.Rows.Add(dRow);
                        }

                        totals += total;
                        contractValues += contractValue;
                        trust5085s += trust5085;
                        allowInsurances += allowInsurance;
                        annuitys += annuity;
                        contractTotals += contracts;

                        contracts = 0D;
                        total = 0D;
                        trust50 = 0D;
                        contractValue = 0D;
                        trust5085 = 0D;
                        allowInsurance = 0D;
                        annuity = 0D;
                        balanceDue = 0D;

                        tValue = 0D;
                        iValue = 0D;
                        aValue = 0D;
                        tiValue = 0D;
                        aiValue = 0D;

                        L_contracts = 0D;
                        L_total = 0D;
                        L_trust50 = 0D;
                        L_contractValue = 0D;
                        L_trust5085 = 0D;
                        L_allowInsurance = 0D;
                        L_annuity = 0D;
                        L_balanceDue = 0D;

                        L_tValue = 0D;
                        L_iValue = 0D;
                        L_aValue = 0D;
                        L_tiValue = 0D;
                        L_aiValue = 0D;

                        oldLoc = location;
                        oldServiceLoc = serviceLoc;
                    }
                    if (!String.IsNullOrWhiteSpace(lapsed))
                    {
                        L_contracts++;
                        //L_total += dt.Rows[i]["endingBalance"].ObjToDouble();
                        L_total += dt.Rows[i]["trust85"].ObjToDouble();
                        L_trust50 += dt.Rows[i]["trust50"].ObjToDouble();
                        L_contractValue += dt.Rows[i]["contractValue"].ObjToDouble();
                        L_trust5085 = L_trust50 + L_total;
                        L_allowInsurance += dt.Rows[i]["allowInsurance"].ObjToDouble();
                        L_annuity += dt.Rows[i]["annuity"].ObjToDouble();
                        L_balanceDue += dt.Rows[i]["balanceDue"].ObjToDouble();

                        L_tValue += dt.Rows[i]["trustOnly"].ObjToDouble();
                        L_iValue += dt.Rows[i]["insOnly"].ObjToDouble();
                        L_aValue += dt.Rows[i]["annOnly"].ObjToDouble();
                        L_tiValue += dt.Rows[i]["TandI"].ObjToDouble();
                        L_aiValue += dt.Rows[i]["IandA"].ObjToDouble();
                    }
                    else
                    {
                        contracts++;
                        //total += dt.Rows[i]["endingBalance"].ObjToDouble();
                        total += dt.Rows[i]["trust85"].ObjToDouble();
                        trust50 += dt.Rows[i]["trust50"].ObjToDouble();
                        contractValue += dt.Rows[i]["contractValue"].ObjToDouble();
                        trust5085 = trust50 + total;
                        allowInsurance += dt.Rows[i]["allowInsurance"].ObjToDouble();
                        annuity += dt.Rows[i]["annuity"].ObjToDouble();
                        balanceDue += dt.Rows[i]["balanceDue"].ObjToDouble();

                        tValue += dt.Rows[i]["trustOnly"].ObjToDouble();
                        iValue += dt.Rows[i]["insOnly"].ObjToDouble();
                        aValue += dt.Rows[i]["annOnly"].ObjToDouble();
                        tiValue += dt.Rows[i]["TandI"].ObjToDouble();
                        aiValue += dt.Rows[i]["IandA"].ObjToDouble();
                    }
                }
                catch (Exception ex)
                {
                }
            }

            if (contracts != 0D)
            {
                dRow = dt2.NewRow();
                dRow["location"] = getLocation(oldLoc);
                dRow["location"] = filterServiceLoc(oldServiceLoc);
                dRow["contracts"] = contracts;
                dRow["total"] = total;
                dRow["trust50"] = trust50;
                dRow["contractValue"] = contractValue;
                dRow["trust5085"] = trust5085;
                dRow["allowInsurance"] = allowInsurance;
                dRow["annuity"] = annuity;
                dRow["balanceDue"] = balanceDue;
                dt2.Rows.Add(dRow);

                if (L_contracts != 0D)
                {
                    dRow = dt2.NewRow();
                    dRow["location"] = getLocation(oldLoc);
                    dRow["location"] = filterServiceLoc(oldServiceLoc) + " Lapsed";
                    dRow["contracts"] = L_contracts;
                    dRow["total"] = L_total;
                    dRow["trust50"] = L_trust50;
                    dRow["contractValue"] = L_contractValue;
                    dRow["trust5085"] = L_trust5085;
                    dRow["allowInsurance"] = L_allowInsurance;
                    dRow["annuity"] = L_annuity;
                    dRow["balanceDue"] = L_balanceDue;
                    dt2.Rows.Add(dRow);
                }

                totals += total;
                contractTotals += contracts;
                trust5085s += trust5085;
            }

            double trust85 = 0D;
            for (int i = 0; i < dt2.Rows.Count; i++)
            {
                trust50 = dt2.Rows[i]["trust50"].ObjToDouble();
                trust85 = dt2.Rows[i]["total"].ObjToDouble();
                allowInsurance = dt2.Rows[i]["allowInsurance"].ObjToDouble();
                annuity = dt2.Rows[i]["annuity"].ObjToDouble();

                total = trust50 + trust85 + allowInsurance + annuity;
                dt2.Rows[i]["totalLoc"] = total;
            }

            dgv9.DataSource = dt2;
        }
        /***********************************************************************************************/
        private string filterServiceLoc ( string serviceLoc )
        {
            string location = serviceLoc.Replace(" Pre", "").Trim();
            location = location.Replace(" Post", "").Trim();
            return location;
        }
        /***********************************************************************************************/
        private DataTable processLocations(DataTable dt)
        {
            string location = "";

            if (funDt == null)
                funDt = G1.get_db_data("SELECT * FROM `funeralHomes`;");

            if (preDt == null)
                preDt = G1.get_db_data("SELECT * FROM `pre2002`;");

            dt = LoadAnnuityLocations(dt);

            string is2002 = "";
            string locInd = "";
            string riles = "";
            string name = "";
            double balance = 0D;
            double interest = 0D;
            double removals = 0D;

            dt.Columns.Add("serviceLoc");
            DataRow[] dRows = null;
            for (int i = 0; i < dt.Rows.Count; i++) 
            {
                location = dt.Rows[i]["location"].ObjToString().ToUpper();
                if (location == "H")
                    location = "B";
                else if (location == "R")
                    location = "B";
                if (location == "J")
                    location = "C";
                else if (location == "CA")
                    location = "CT";
                else if (location == "JT")
                    location = "E";
                else if (location == "FFN")
                    location = "FF";
                else if (location == "FFO")
                    location = "FF";
                else if (location == "RF")
                    location = "FF";/*
                else if (location == "FO")
                    location = "F";*/
                else if (location == "F")
                    location = "FO";
                else if (location == "U")
                    location = "M";
                else if (location == "UM")
                    location = "M";
                else if (location == "NB")
                    location = "N";
                /*else if (location == "NC")
                    location = "NNM";
                else if (location == "NCOC")
                    location = "NNM";*/
                else if (location == "TY")
                    location = "T";
                else if (location == "W")
                    location = "WM";
                else if (location == "WW")
                    location = "WM";
                else if (location == "WT")
                    location = "N";
                
                is2002 = dt.Rows[i]["is2002"].ObjToString();
                locInd = dt.Rows[i]["locInd"].ObjToString();
                riles = dt.Rows[i]["riles"].ObjToString();

                dt.Rows[i]["location"] = location;
                name = location;
                if (location == "F")
                    name = "Forest";/*
                else if (location == "TY")
                    name = "Capps/Tylertown FH";
                else*/
                    dRows = funDt.Select("keyCode = '" + location + "'");

                if ( String.IsNullOrWhiteSpace ( locInd ) && !String.IsNullOrWhiteSpace ( location ))
                {
                    try
                    {
                        if (dRows.Length > 0 )
                        {
                            locInd = dRows[0]["locInd"].ObjToString();
                            dt.Rows[i]["locInd"] = locInd;
                        }
                    }
                    catch ( Exception ex)
                    {
                    }
                }

                if (!String.IsNullOrWhiteSpace(is2002))
                {
                    dRows = funDt.Select("keyCode = '" + location + "'");
                    if (dRows.Length > 0)
						name = dRows[0]["locationCode"].ObjToString();											  
					if (name == location)					   
                    {
						dRows = preDt.Select("locind = '" + location + "'");							
						if(dRows.Length > 0)
                        name = dRows[0]["name"].ObjToString();
                    }
                }
                else
                {
					dRows = funDt.Select("keyCode='" + location + "'");							   
					if (dRows.Length > 0)
						name = dRows[0]["locationCode"].ObjToString();
                }

                if (string.IsNullOrWhiteSpace(is2002))
                {
                    name += " Pre";
                    //balance = dt.Rows[i]["endingBalance"].ObjToDouble();
                    //interest = dt.Rows[i]["interest"].ObjToDouble();
                    //removals = dt.Rows[i]["currentRemovals"].ObjToDouble();
                    //if (removals == 0D)
                    //    dt.Rows[i]["endingBalance"] = balance + interest;
                }
                else
                {
                    name += " Post";
                }
                
                dt.Rows[i]["serviceLoc"] = name;
            }

            // An attempt at summing the totals in the cemetery tab dgv6
            //            gridMain6.Columns["Contract Amount"].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            //            gridMain6.Columns["Contract Amount"].SummaryItem.DisplayFormat = "";

            //int oldRows = dt.Rows.Count;
            //Trust85.FindContract(dt, "WT030");
            //dRows = dt.Select("endingBalance > '0.00' and currentRemovals = '0.00'");
            //if (dRows.Length > 0)
            //    dt = dRows.CopyToDataTable();
            //int newRow = dt.Rows.Count;
            //Trust85.FindContract(dt, "WT030");

            // HU Tab - dgv3 - gridMain3
            dRows = dt.Select("serviceLoc = 'Hartman Hughes Pre'");
            DataTable hudt = dt.Clone();
            if (dRows.Length > 0)
                hudt = dRows.CopyToDataTable();
            hudt = RemoveNoContracts(hudt, false );

            G1.NumberDataTable(hudt);
            dgv3.DataSource = hudt;

            // Remove Hartman Hughes Pre from Pre/Post/Riles
            dRows = dt.Select("serviceLoc <> 'Hartman Hughes Pre'");
            if (dRows.Length > 0)
                dt = dRows.CopyToDataTable();

            DataView tempView = dt.DefaultView;
            tempView.Sort = "serviceLoc";
            dt = tempView.ToTable();

            // JPN Tab - dgv4 - gridMain4
            //dRows = dt.Select("serviceLoc = 'Old Jones PN(Southland) Pre'");
            dRows = dt.Select("serviceLoc = 'JPN Pre'");
            DataTable jpndt = dt.Clone();
            if (dRows.Length > 0)
                jpndt = dRows.CopyToDataTable();

            G1.NumberDataTable(jpndt);
            dgv4.DataSource = jpndt;

            // Remove Old Jones PN(Southland) Pre from Pre/Post/Riles
            //dRows = dt.Select("serviceLoc <> 'Old Jones PN(Southland) Pre'");
            dRows = dt.Select("serviceLoc <> 'JPN Pre'");
            if (dRows.Length > 0)
                dt = dRows.CopyToDataTable();
            DataView tempView2 = dt.DefaultView;
            tempView2.Sort = "serviceLoc";
            dt = tempView2.ToTable();

            // NMOC Tab - dgv5 - gridMain5
            //dRows = dt.Select("serviceLoc = 'Newton Mem GRDN O/C Pre'");
            dRows = dt.Select("serviceLoc = 'NCOC Pre'");
            DataTable nmocdt = dt.Clone();
            if (dRows.Length > 0)
                nmocdt = dRows.CopyToDataTable();

            G1.NumberDataTable(nmocdt);
            dgv5.DataSource = nmocdt;

            // Remove Newton Mem GRDN O/C Pre from Pre/Post/Riles
            //dRows = dt.Select("serviceLoc <> 'Newton Mem GRDN O/C Pre'");
            dRows = dt.Select("serviceLoc <> 'NCOC Pre'");
            if (dRows.Length > 0)
                dt = dRows.CopyToDataTable();
            DataView tempView3 = dt.DefaultView;
            tempView3.Sort = "serviceLoc";
            dt = tempView3.ToTable();

            // Cemeteries Tab - dgv6 - gridMain6
            //dRows = dt.Select("serviceLoc = 'Hillcrest Cemetery Post' or serviceLoc = 'Hillcrest Cemetery Pre' or serviceLoc = 'Newton Memorial Gardens Pre' or serviceLoc = 'Newton Memorial Gardens Post'");
            dRows = dt.Select("serviceLoc = 'Hillcrest Cemetery Pre' or serviceLoc = 'Newton Memorial Gardens Pre'");
            DataTable hcdt = dt.Clone();
            if (dRows.Length > 0)
                hcdt = dRows.CopyToDataTable();

            G1.NumberDataTable(hcdt);

            // Group the different cemeteries and total them.
            gridMain6.Columns["location"].GroupIndex = 0;
            gridMain6.OptionsView.ShowFooter = false;
            gridMain6.OptionsView.GroupFooterShowMode = DevExpress.XtraGrid.Views.Grid.GroupFooterShowMode.VisibleIfExpanded;
            gridMain6.ExpandAllGroups();

            dgv6.DataSource = hcdt;

            // Remove Hillcrest POST from Pre/Post/Riles
            dRows = dt.Select("serviceLoc <> 'Hillcrest Cemetery Post'");
            if (dRows.Length > 0)
                dt = dRows.CopyToDataTable();
            DataView tempView4 = dt.DefaultView;
            tempView4.Sort = "serviceLoc";
            dt = tempView4.ToTable();

            // Remove Hillcrest PRE from Pre/Post/Riles
            dRows = dt.Select("serviceLoc <> 'Hillcrest Cemetery Pre'");
            if (dRows.Length > 0)
                dt = dRows.CopyToDataTable();
            DataView tempView5 = dt.DefaultView;
            tempView5.Sort = "serviceLoc";
            dt = tempView5.ToTable();

            // Remove NMG Pre from Pre/Post/Riles
            dRows = dt.Select("serviceLoc <> 'Newton Memorial Gardens Pre'");
            if (dRows.Length > 0)
                dt = dRows.CopyToDataTable();
            DataView tempView6 = dt.DefaultView;
            tempView6.Sort = "serviceLoc";
            dt = tempView6.ToTable();

            // Remove NMG Post from Pre/Post/Riles
            dRows = dt.Select("serviceLoc <> 'Newton Memorial Gardens Post'");
            if (dRows.Length > 0)
                dt = dRows.CopyToDataTable();
            DataView tempView7 = dt.DefaultView;
            tempView7.Sort = "serviceLoc";
            dt = tempView7.ToTable();

            // Remove AFA
            DataTable afaDt = dt.Clone();
            dRows = dt.Select("contractNumber LIKE 'AFA%'");
            if (dRows.Length > 0)
                afaDt = dRows.CopyToDataTable();
            DataView tempView8 = afaDt.DefaultView;
            tempView8.Sort = "serviceLoc";
            afaDt = tempView8.ToTable();
            dgv8.DataSource = afaDt;

            dRows = dt.Select("contractNumber NOT LIKE 'AFA%'");
            if (dRows.Length > 0)
                dt = dRows.CopyToDataTable();
            tempView = dt.DefaultView;
            tempView.Sort = "serviceLoc";
            dt = tempView.ToTable();

            DataTable summaryDt = dt.Copy();
            DataTable tempDt = (DataTable)dgv3.DataSource;
            summaryDt.Merge(tempDt);

            tempDt = (DataTable)dgv4.DataSource;
            summaryDt.Merge(tempDt);

            tempDt = (DataTable)dgv5.DataSource;
            summaryDt.Merge(tempDt);

            tempDt = (DataTable)dgv6.DataSource;
            summaryDt.Merge(tempDt);

            tempDt = (DataTable)dgv8.DataSource;
            summaryDt.Merge(tempDt);

            G1.NumberDataTable(summaryDt);
            //dgv9.DataSource = summaryDt;

            tempView = summaryDt.DefaultView;
            tempView.Sort = "serviceLoc";
            summaryDt = tempView.ToTable();

            dRows = summaryDt.Select("contractNumber='NNM68A'");
            if ( dRows.Length > 0 )
            {
            }

            return summaryDt;
        }
        /***********************************************************************************************/
        private string getLocation(string location)
        {
            try
            {
                if (funDt == null)
                    funDt = G1.get_db_data("SELECT * FROM `funeralHomes`;");

                if (preDt == null)
                    preDt = G1.get_db_data("SELECT * FROM `pre2002`;");

                DataRow[] dRows = funDt.Select("keyCode = '" + location + "'");

                if (dRows.Length > 0)
                    location = dRows[0]["locationCode"].ObjToString();
                
            }
            catch (Exception ex)
            { 
            
            }

            return location;
        }
        /***********************************************************************************************/
        private DataTable processTheData ( DataTable dt )
        {
            /*------------------------------------------------------------------------------------------------*/
            string contractNumber = "";
            string firstName = "";
            string lastName = "";
            string address2013 = "";
            string city2013 = "";
            string state2013 = "";
            string zip2013 = "";
            string ssn2013 = "";
            string payDate8 = "";
            double beginningBalance = 0D;
            double interest = 0D;
            double ytdPrevious = 0D;
            double paymentCurrMonth = 0D;
            double currentPayments = 0D;
            double deathRemYTDprevious = 0D;
            double refundRemYTDprevious = 0D;
            double refundRemCurrMonth = 0D;
            double currentRemovals = 0D;
            double endingBalance = 0D;
            string ServiceID = "";
            string Is2002 = "";
            string location = "";
            string filename = "";
            string riles = "";
            string locind = "";

            DataRow[] dRows = null;

            string contract = "";
            string trust = "";
            string loc = "";

            double trust85 = 0D;
            double trust50 = 0D;
            DateTime issueDate = DateTime.Now;
            DateTime cutDate = new DateTime(2006, 6, 1);

            dt.Columns.Add("trust50", Type.GetType("System.Double"));
            dt.Columns.Add("trust85", Type.GetType("System.Double"));
            dt.Columns.Add("MyIssueDate");
            dt.Columns.Add("TandI", Type.GetType("System.Double"));
            dt.Columns.Add("insOnly", Type.GetType("System.Double"));
            dt.Columns.Add("trustOnly", Type.GetType("System.Double"));
            dt.Columns.Add("annOnly", Type.GetType("System.Double"));
            dt.Columns.Add("IandA", Type.GetType("System.Double"));

            dRows = dt.Select("contractNumber NOT LIKE 'SX%'");
            if (dRows.Length > 0)
                dt = dRows.CopyToDataTable();

            dRows = dt.Select("contractNumber <> 'Test'");
            if (dRows.Length > 0)
                dt = dRows.CopyToDataTable();

            int oldCount = dRows.Length;
            double tValue = 0D;
            double iValue = 0D;
            double aValue = 0D;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                if (contractNumber == "NC0C35")
                {
                    dt.Rows[i]["contractNumber"] = "NCOC35";
                    dt.Rows[i]["location"] = "NCOC";
                }
                loc = dt.Rows[i]["location"].ObjToString();
                if (String.IsNullOrWhiteSpace(loc))
                {
                    contract = Trust85.decodeContractNumber(contractNumber, ref trust, ref loc);
                    dt.Rows[i]["location"] = loc;
                }

                issueDate = dt.Rows[i]["issueDate8"].ObjToDateTime();
                dt.Rows[i]["MyIssueDate"] = issueDate.ToString("yyyyMMdd");
                endingBalance = dt.Rows[i]["endingBalance"].ObjToDouble();
                tValue = endingBalance;
                iValue = dt.Rows[i]["allowInsurance"].ObjToDouble();
                aValue = dt.Rows[i]["annuity"].ObjToDouble();
                if (tValue != 0D && iValue == 0D && aValue == 0D)
                    dt = AddOne(dt, i, "trustOnly");
                else if (tValue == 0D && iValue != 0D && aValue == 0D)
                    dt = AddOne(dt, i, "insOnly");
                else if (tValue == 0D && iValue == 0D && aValue != 0D)
                    dt = AddOne(dt, i, "annOnly");
                else if (tValue != 0D && iValue != 0D)
                    dt = AddOne(dt, i, "TandI");
                else if (iValue != 0D && aValue != 0D)
                    dt = AddOne(dt, i, "IandA");
                if (endingBalance > 0D)
                {
                    if (issueDate < cutDate)
                        dt.Rows[i]["trust50"] = endingBalance;
                    else
                        dt.Rows[i]["trust85"] = endingBalance;
                }
                else
                    dt.Rows[i]["trust85"] = endingBalance;

            }

            dRows = dt.Select("location='B'");
            int newCount = dRows.Length;

            return dt;
        }
        /***********************************************************************************************/
        private DataTable AddOne ( DataTable dt, int i, string field )
        {
            try
            {
                double dValue = dt.Rows[i][field].ObjToDouble();
                dValue++;
                dt.Rows[i][field] = dValue;
            }
            catch ( Exception ex )
            {
            }
            return dt;
        }
        ///***********************************************************************************************/
        //private DataTable ProcessCustomData ( DataTable dt )
        //{
        //    DateTime date = dateTimePicker1.Value;
        //    DateTime saveDate1 = date;
        //    string date1 = G1.DateTimeToSQLDateTime(date);
        //    date = dateTimePicker2.Value;
        //    DateTime saveDate2 = date;
        //    string date2 = G1.DateTimeToSQLDateTime(date);

        //    string cmd = "Select * from `trust_log_data` WHERE `date` >= '" + date1 + "' AND `date` <= '" + date2 + "' ";
        //    cmd += " ORDER by `date`";
        //    cmd += ";";

        //    DataTable dx = G1.get_db_data(cmd);
        //    if (dx.Rows.Count <= 0)
        //        return dt;

        //    dt.Columns.Add("aIssueDate");

        //    for ( int i=0; i<dt.Rows.Count; i++)
        //    {
        //        date = dt.Rows[i]["issueDate8"].ObjToDateTime();
        //        date1 = date.ToString("yyyyMMdd");
        //        dt.Rows[i]["aIssueDate"] = date1;
        //    }

        //    DataRow[] dRows = null;
        //    string contractNumber = "";
        //    string column = "";
        //    string detail = "";

        //    for ( int i=0; i<dx.Rows.Count; i++)
        //    {
        //        date = dx.Rows[i]["date"].ObjToDateTime();
        //        date1 = date.ToString("yyyyMMdd");
        //        contractNumber = dx.Rows[i]["contractNumber"].ObjToString();
        //        column = dx.Rows[i]["what"].ObjToString();
        //        detail = dx.Rows[i]["detail"].ObjToString();
        //        dRows = dt.Select("contractNumber='" + contractNumber + "' AND `aIssueDate` = '" + date1 + "'");
        //        if (dRows.Length > 0)
        //        {
        //            if (column.ToUpper() == "ALLOWINSURANCE")
        //                dRows[0]["allowInsurance"] = detail.ObjToDouble();
        //            else
        //                dRows[0][column] = detail;
        //            detail = dRows[0]["dataedited"].ObjToString();
        //            detail += "," + column;
        //            dRows[0]["dataedited"] = detail;

        //        }
        //    }
        //    return dt;
        //}
        /***********************************************************************************************/
        private DataTable ProcessACH ( DataTable dt, string trustNumber = "" )
        {
            string contractNumber = "";
            string cmd = "Select * from `ach`;";
            if (!String.IsNullOrWhiteSpace(trustNumber))
                cmd = "Select * from `ach` where `contractNumber` = '" + trustNumber + "';";
            DataTable dx = G1.get_db_data(cmd);

            DataRow[] dRows = null;

            if ( !String.IsNullOrWhiteSpace ( trustNumber ))
            {
                dRows = dx.Select("contractNumber='" + trustNumber + "'");
                if (dRows.Length > 0)
                {
                    dRows = dt.Select("contractNumber='" + trustNumber + "'");
                    if (dRows.Length > 0)
                    {
                        dRows[0]["bookOrder"] = "DRAFT";
                        dt.AcceptChanges();
                    }
                }
                return dt;
            }

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                dRows = dx.Select("contractNumber='" + contractNumber + "'");
                if (dRows.Length > 0)
                    dt.Rows[i]["bookOrder"] = "DRAFT";
            }
            return dt;
        }
        /***********************************************************************************************/
        private DataTable ProcessDBR (DataTable dt, string trustNumber = "" )
        {
            string contractNumber = "";
            string cmd = "";
            DataTable dx = null;
            DataRow[] dRows = null;
            string str = "";

            if ( !String.IsNullOrWhiteSpace ( trustNumber ))
            {
                cmd = "Select * from `dbrs` where contractNumber = '" + trustNumber + "';";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    dRows = dt.Select("contractNumber='" + trustNumber + "'");
                    if (dRows.Length > 0)
                    {
                        str = dRows[0]["bookOrder"].ObjToString();
                        if (!String.IsNullOrWhiteSpace(str))
                            str += " / DBR";
                        else
                            str = "DBR";
                        dRows[0]["bookOrder"] = str;
                        dt.AcceptChanges();
                    }
                }
                return dt;
            }

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                cmd = "Select * from `dbrs` where contractNumber = '" + contractNumber + "';";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    str = dt.Rows[i]["bookOrder"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(str))
                        str += " / DBR";
                    else
                        str = "DBR";
                    dt.Rows[i]["bookOrder"] = str;
                }
            }
            return dt;
        }
        /***********************************************************************************************/
        private DataTable ProcessDownPayments ( DataTable dt )
        {
            //DateTime date = dateTimePicker1.Value;
            //date = date.AddMonths(-1);
            //DateTime saveDate1 = date;
            //string date1 = G1.DateTimeToSQLDateTime(date);
            //date = dateTimePicker2.Value;
            //date = date.AddMonths(1);
            //DateTime saveDate2 = date;
            //string date2 = G1.DateTimeToSQLDateTime(date);

            //try
            //{
            //    string cmd = "Select * from `downpayments` WHERE `date` >= '" + date1 + "' AND `date` <= '" + date2 + "' ";
            //    cmd += " ORDER by `date` ";
            //    cmd += ";";

            //    DataTable dx = G1.get_db_data(cmd);

            //    dx.Columns.Add("newDate");
            //    for (int i = 0; i < dx.Rows.Count; i++)
            //    {
            //        date = dx.Rows[i]["date"].ObjToDateTime();
            //        dx.Rows[i]["newDate"] = date.ToString("yyyyMMdd");
            //    }
            //    DataRow[] dRows = null;
            //    string contractNumber = "";
            //    string depNumber = "";
            //    string lName = "";
            //    string fName = "";
            //    DateTime depDate = DateTime.Now;
            //    double oldDownPayment = 0D;
            //    double downPayment = 0D;
            //    double ccFee = 0D;
            //    double lossRecoveryFee = 0D;
            //    double totalDownPayment = 0D;
            //    DataTable tempDt = null;
            //    string trust = "";
            //    string loc = "";
            //    string location = "";

            //    for (int i = 0; i < dt.Rows.Count; i++)
            //    {
            //        try
            //        {
            //            contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
            //            if (contractNumber == "CT24042LI")
            //            {
            //            }

            //            Trust85.decodeContractNumber(contractNumber, ref trust, ref loc);
            //            location = loc;
            //            if (!String.IsNullOrWhiteSpace(loc))
            //            {
            //                dRows = funDt.Select("keycode='" + loc + "'");
            //                if (dRows.Length > 0)
            //                    location = dRows[0]["LocationCode"].ObjToString();
            //            }

            //            depNumber = dt.Rows[i]["depositNumber"].ObjToString();
            //            lName = dt.Rows[i]["lastName"].ObjToString();
            //            fName = dt.Rows[i]["firstName"].ObjToString();
            //            oldDownPayment = dt.Rows[i]["downPayment"].ObjToDouble();
            //            //if (lName.Trim().ToUpper() != "BLACKLEDGE")
            //            //    continue;
            //            depDate = dt.Rows[i]["issueDate8"].ObjToDateTime();

            //            if ( !String.IsNullOrWhiteSpace ( location ))
            //                dRows = dx.Select("lastName='" + lName + "' AND `depositNumber` = '" + depNumber + "' AND `newDate` = '" + depDate.ToString("yyyyMMdd") + "' and `location` = '" + location + "'");
            //            else
            //                dRows = dx.Select("lastName='" + lName + "' AND `depositNumber` = '" + depNumber + "' AND `newDate` = '" + depDate.ToString("yyyyMMdd") + "'");
            //            if ( dRows.Length > 1 )
            //            {
            //                tempDt = dRows.CopyToDataTable();
            //                dRows = dx.Select("firstName='" + fName + "' AND lastName='" + lName + "' AND `depositNumber` = '" + depNumber + "' AND `newDate` = '" + depDate.ToString("yyyyMMdd") + "'");
            //            }
            //            if ( dRows.Length <= 0 )
            //                dRows = dx.Select("firstName='" + fName + "' AND `depositNumber` = '" + depNumber + "' AND `newDate` = '" + depDate.ToString("yyyyMMdd") + "'");
            //            if (dRows.Length > 0)
            //            {
            //                totalDownPayment = 0D;
            //                downPayment = 0D;
            //                lossRecoveryFee = 0D;
            //                ccFee = 0D;
            //                for (int j = 0; j < dRows.Length; j++)
            //                {
            //                    downPayment += dRows[j]["downPayment"].ObjToDouble();
            //                    lossRecoveryFee += dRows[j]["lossRecoveryFee"].ObjToDouble();
            //                    ccFee += dRows[j]["ccFee"].ObjToDouble();
            //                }
            //                if (downPayment < oldDownPayment)
            //                    downPayment = oldDownPayment;
            //                totalDownPayment += downPayment + lossRecoveryFee;
            //                totalDownPayment = G1.RoundValue(totalDownPayment);
            //                dt.Rows[i]["downpayment"] = totalDownPayment;
            //                dt.Rows[i]["ccFee"] = ccFee;
            //                if ( dRows.Length == 1 && lossRecoveryFee == 0D )
            //                {
            //                    if (!String.IsNullOrWhiteSpace(location))
            //                    {
            //                        if ( location.ToUpper() == "FLOWOOD" || location.ToUpper() == "CLINTON" )
            //                            dRows = dx.Select("lastName='" + lName + "' AND `depositNumber` <> '" + depNumber + "' AND `newDate` = '" + depDate.ToString("yyyyMMdd") + "' and `location` LIKE '%" + location + "%'");
            //                        else
            //                            dRows = dx.Select("lastName='" + lName + "' AND `depositNumber` <> '" + depNumber + "' AND `newDate` = '" + depDate.ToString("yyyyMMdd") + "' and `location` = '" + location + "'");
            //                    }
            //                    else
            //                        dRows = dx.Select("lastName='" + lName + "' AND `depositNumber` <> '" + depNumber + "' AND `newDate` = '" + depDate.ToString("yyyyMMdd") + "'");
            //                    if ( dRows.Length > 0 )
            //                    {
            //                        for ( int j=0; j<dRows.Length; j++)
            //                        {
            //                            downPayment = dRows[j]["downPayment"].ObjToDouble();
            //                            lossRecoveryFee = dRows[j]["lossRecoveryFee"].ObjToDouble();
            //                            if ( downPayment == 0D && lossRecoveryFee > 0D )
            //                            {
            //                                dt.Rows[i]["downpayment"] = totalDownPayment + lossRecoveryFee;
            //                                break;
            //                            }
            //                        }
            //                    }
            //                }
            //            }
            //        }
            //        catch (Exception ex)
            //        {
            //        }
            //    }
            //}
            //catch ( Exception ex)
            //{
            //}
            return dt;
        }
        /***********************************************************************************************/
        private void BuildGroupSummary ( DataTable dt )
        {
            if (summaryDt != null)
                summaryDt.Rows.Clear();
            else
            {
                summaryDt = new DataTable();
                summaryDt.Columns.Add("location");
                summaryDt.Columns.Add("L_contracts");
                summaryDt.Columns.Add("L_contractValue");
                summaryDt.Columns.Add("L_trust50");
                summaryDt.Columns.Add("L_total");
                summaryDt.Columns.Add("L_trust5085");
                summaryDt.Columns.Add("firstContract");
                summaryDt.Columns.Add("lastContract");
                summaryDt.Columns.Add("count", Type.GetType("System.Int32"));

                summaryDt.Columns.Add("firstIns");
                summaryDt.Columns.Add("lastIns");
                summaryDt.Columns.Add("countIns", Type.GetType("System.Int32"));

                summaryDt.Columns.Add("firstCem");
                summaryDt.Columns.Add("lastCem");
                summaryDt.Columns.Add("countCem", Type.GetType("System.Int32"));
            }

            DataRow dRow = null;
            string location = "";
            string lastLocation = "";
            string contractNumber = "";

            summaryFirstContract = "";
            summaryLastContract = "";
            int count = 0;

            summaryFirstIns = "";
            summaryLastIns = "";
            summaryInsCount = 0;

            summaryFirstCem = "";
            summaryLastCem = "";
            summaryCemCount = 0;

            string trustType = "";

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                location = dt.Rows[i]["location"].ObjToString();
                if (String.IsNullOrWhiteSpace(location))
                    location = "XXXX";
                trustType = dt.Rows[i]["trustType"].ObjToString().ToUpper();
                if (trustType != "TRUST")
                    continue;
                if (location != lastLocation)
                {
                    if (String.IsNullOrWhiteSpace(lastLocation))
                    {
                        contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                        summaryFirstContract = dt.Rows[i]["contractNumber"].ObjToString();
                        summaryLastContract = summaryFirstContract;
                        count = 1;
                        lastLocation = location;
                        continue;
                    }
                    dRow = summaryDt.NewRow();
                    dRow["location"] = lastLocation;
                    dRow["firstContract"] = summaryFirstContract;
                    dRow["lastContract"] = summaryLastContract;
                    dRow["count"] = count;
                    summaryDt.Rows.Add(dRow);

                    lastLocation = location;
                    summaryFirstContract = dt.Rows[i]["contractNumber"].ObjToString();
                    summaryLastContract = summaryFirstContract;
                    count = 1;
                    continue;
                }
                summaryLastContract = dt.Rows[i]["contractNumber"].ObjToString();
                count++;
            }
            dRow = summaryDt.NewRow();
            dRow["location"] = lastLocation;
            dRow["firstContract"] = summaryFirstContract;
            dRow["lastContract"] = summaryLastContract;
            dRow["count"] = count;
            summaryDt.Rows.Add(dRow);
            summaryDt.AcceptChanges();

            BuildInsuranceGroupSummary(dt);
            BuildCemeteryGroupSummary(dt);
        }
        /***********************************************************************************************/
        private void BuildInsuranceGroupSummary ( DataTable dx )
        {
            if (summaryDt == null)
                return;

            DataRow dRow = null;
            string location = "";
            string lastLocation = "";
            string contractNumber = "";

            summaryFirstContract = "";
            summaryLastContract = "";
            int count = 0;

            summaryFirstIns = "";
            summaryLastIns = "";
            summaryInsCount = 0;

            summaryFirstCem = "";
            summaryLastCem = "";
            summaryCemCount = 0;

            DataRow[] dRows = dx.Select("trustType='Insurance'");
            if (dRows.Length <= 0)
                return;

            DataTable dt = dRows.CopyToDataTable();

            string trustType = "";

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                location = dt.Rows[i]["location"].ObjToString();
                if (String.IsNullOrWhiteSpace(location))
                    location = "XXXX";
                trustType = dt.Rows[i]["trustType"].ObjToString().ToUpper();
                if (trustType != "INSURANCE")
                    continue;
                if (location != lastLocation)
                {
                    if (String.IsNullOrWhiteSpace(lastLocation))
                    {
                        contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                        summaryFirstContract = dt.Rows[i]["contractNumber"].ObjToString();
                        summaryLastContract = summaryFirstContract;
                        count = 1;
                        lastLocation = location;
                        continue;
                    }
                    dRows = summaryDt.Select("location='" + lastLocation + "'");
                    if (dRows.Length <= 0)
                    {
                        dRow = summaryDt.NewRow();
                        dRow["location"] = lastLocation;
                        dRow["firstIns"] = summaryFirstContract;
                        dRow["lastIns"] = summaryLastContract;
                        dRow["countIns"] = count;
                        summaryDt.Rows.Add(dRow);
                    }
                    else
                    {
                        dRows[0]["firstIns"] = summaryFirstContract;
                        dRows[0]["lastIns"] = summaryLastContract;
                        dRows[0]["countIns"] = count;
                    }

                    lastLocation = location;
                    summaryFirstContract = dt.Rows[i]["contractNumber"].ObjToString();
                    summaryLastContract = summaryFirstContract;
                    count = 1;
                    continue;
                }
                summaryLastContract = dt.Rows[i]["contractNumber"].ObjToString();
                count++;
            }
            dRows = summaryDt.Select("location='" + lastLocation + "'");
            if (dRows.Length <= 0)
            {
                dRow = summaryDt.NewRow();
                dRow["location"] = lastLocation;
                dRow["firstIns"] = summaryFirstContract;
                dRow["lastIns"] = summaryLastContract;
                dRow["countIns"] = count;
                summaryDt.Rows.Add(dRow);
            }
            else
            {
                dRows[0]["firstIns"] = summaryFirstContract;
                dRows[0]["lastIns"] = summaryLastContract;
                dRows[0]["countIns"] = count;
            }
        }
        /***********************************************************************************************/
        private void BuildCemeteryGroupSummary(DataTable dx)
        {
            if (summaryDt == null)
                return;

            DataRow dRow = null;
            string location = "";
            string lastLocation = "";
            string contractNumber = "";

            summaryFirstContract = "";
            summaryLastContract = "";
            int count = 0;

            summaryFirstIns = "";
            summaryLastIns = "";
            summaryInsCount = 0;

            summaryFirstCem = "";
            summaryLastCem = "";
            summaryCemCount = 0;

            DataRow[] dRows = dx.Select("trustType='Cemetery'");
            if (dRows.Length <= 0)
                return;

            DataTable dt = dRows.CopyToDataTable();

            string trustType = "";

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                location = dt.Rows[i]["location"].ObjToString();
                if (String.IsNullOrWhiteSpace(location))
                    location = "XXXX";
                trustType = dt.Rows[i]["trustType"].ObjToString().ToUpper();
                if (trustType != "CEMETERY")
                    continue;
                if (location != lastLocation)
                {
                    if (String.IsNullOrWhiteSpace(lastLocation))
                    {
                        contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                        summaryFirstContract = dt.Rows[i]["contractNumber"].ObjToString();
                        summaryLastContract = summaryFirstContract;
                        count = 1;
                        lastLocation = location;
                        continue;
                    }
                    dRows = summaryDt.Select("location='" + lastLocation + "'");
                    if (dRows.Length <= 0)
                    {
                        dRow = summaryDt.NewRow();
                        dRow["location"] = lastLocation;
                        dRow["firstCem"] = summaryFirstContract;
                        dRow["lastCem"] = summaryLastContract;
                        dRow["countCem"] = count;
                        summaryDt.Rows.Add(dRow);
                    }
                    else
                    {
                        dRows[0]["firstCem"] = summaryFirstContract;
                        dRows[0]["lastCem"] = summaryLastContract;
                        dRows[0]["countCem"] = count;
                    }

                    lastLocation = location;
                    summaryFirstContract = dt.Rows[i]["contractNumber"].ObjToString();
                    summaryLastContract = summaryFirstContract;
                    count = 1;
                    continue;
                }
                summaryLastContract = dt.Rows[i]["contractNumber"].ObjToString();
                count++;
            }
            dRows = summaryDt.Select("location='" + lastLocation + "'");
            if (dRows.Length <= 0)
            {
                dRow = summaryDt.NewRow();
                dRow["location"] = lastLocation;
                dRow["firstCem"] = summaryFirstContract;
                dRow["lastCem"] = summaryLastContract;
                dRow["countCem"] = count;
                summaryDt.Rows.Add(dRow);
            }
            else
            {
                dRows[0]["firstCem"] = summaryFirstContract;
                dRows[0]["lastCem"] = summaryLastContract;
                dRows[0]["countCem"] = count;
            }
        }
        /***********************************************************************************************/
        private void chkComboLocNames_EditValueChanged(object sender, EventArgs e)
        {
            if (originalDt == null)
                return;

            string names = getLocationNameQuery();
            DataRow[] dRows = originalDt.Select(names);
            DataTable dt = originalDt.Clone();
            for (int i = 0; i < dRows.Length; i++)
                dt.ImportRow(dRows[i]);
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;

            dgv.Refresh();
            gridMain.RefreshEditor(true);
            gridMain.ExpandAllGroups();
        }
        /*******************************************************************************************/
        private string getLocationNameQuery()
        {
            string procLoc = "";
            string[] locIDs = this.chkComboLocNames.EditValue.ToString().Split('|');
            for (int i = 0; i < locIDs.Length; i++)
            {
                if (!String.IsNullOrWhiteSpace(locIDs[i]))
                {
                    string cmd = "Select * from `funeralhomes` where `LocationCode` = '" + locIDs[i].Trim() + "';";
                    DataTable dt = G1.get_db_data(cmd);
                    if (dt.Rows.Count > 0)
                    {
                        if (procLoc.Trim().Length > 0)
                            procLoc += ",";
                        string id = dt.Rows[0]["keycode"].ObjToString();
                        procLoc += "'" + id.Trim() + "'";
                    }
                    else
                    {
                        if (procLoc.Trim().Length > 0)
                            procLoc += ",";
                        string id = locIDs[i].Trim();
                        procLoc += "'" + id.Trim() + "'";
                    }
                }
            }
            return procLoc.Length > 0 ? " `location` IN (" + procLoc + ") " : ""; // <--- Changed loc to location
        }
        /***********************************************************************************************/
        private void chkPageBreaks_CheckedChanged(object sender, EventArgs e)
        {
        }
        /***********************************************************************************************/
        private int footerCount = 0;
        private void gridMain_AfterPrintRow(object sender, DevExpress.XtraGrid.Views.Printing.PrintRowEventArgs e)
        {
            footerCount++;
            if (footerCount >= 1)
            {
                footerCount = 0;
//                AddFooter(e);
                //if (chkSort.Checked || autoRun)
                //    e.PS.InsertPageBreak(e.Y);
            }
            if (pageBreak)
            {
                e.PS.InsertPageBreak(e.Y);
            }
            pageBreak = false;
        }
        /***********************************************************************************************/
        private bool pageBreak = false;
		private bool allowPrint = true;
        private int printCount = 0;
        private bool justPrinted = true;
        private void gridMain_BeforePrintRow(object sender, DevExpress.XtraGrid.Views.Printing.CancelPrintRowEventArgs e)
        {
			string contract = "";					 
            int rowHandle = e.RowHandle;
			if (rowHandle >= 0)
            {
                int row = gridMain.GetDataSourceRowIndex(rowHandle);
                DataTable dt = (DataTable)dgv.DataSource;
                contract = dt.Rows[row]["contractNumber"].ObjToString();
            }
            if (e.HasFooter || rowHandle < 0)
            {
				allowPrint = true;
                printCount = 0;
                if (chkPageBreaks.Checked)
                    if ( !chkCollapes.Checked )
                        pageBreak = true;
                
                justPrinted = true;
                return;	   
            }
			else if (chkCollapes.Checked)
            {
                e.Cancel = true;
                if (1 == 1)
                    return;
                if (e.Level == 1)
                {
                    if (!gridMain.IsDataRow(rowHandle))
                        e.Cancel = false;
                    else
                    {
                        if ( justPrinted )
                        {
                            //allowPrint = false;
                        }
                        if (!allowPrint)
                            e.Cancel = true;
                        else
                            allowPrint = false;
                    }
                    if (1 == 1)
                        return;
                    if ( rowHandle >= 0 && !allowPrint)
                    {
                        e.Cancel = true;
                        printCount = 0;
                        return;
                    }
                    if (!allowPrint)
                    {
                        e.Cancel = true;
                        printCount = 0;
                    }
                    else
                    {
                        //if (printCount == 1 )
                        //    e.Cancel = false;
                        //if (gridMain.IsDataRow(rowHandle))
                        //    e.Cancel = true;
                        printCount++;
                    }
                }
                else
                    printCount++;
                allowPrint = false;
                if ( !e.Cancel )
                {
                }
            }
        }
        /***********************************************************************************************/
        private DataTable summaryDt = null;
        private string summaryLocation = "";
        private string summaryFirstContract = "";
        private string summaryLastContract = "";

        private string summaryFirstIns = "";
        private string summaryLastIns = "";
        private int summaryInsCount = 0;

        private string summaryFirstCem = "";
        private string summaryLastCem = "";
        private int summaryCemCount = 0;
        /***********************************************************************************************/
        private string location = "";
        private void gridMain_CustomSummaryCalculate(object sender, DevExpress.Data.CustomSummaryEventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            GridView view = sender as GridView;
            int groupRowHandle = e.GroupRowHandle;
            groupRowHandle = e.RowHandle;

            string field = (e.Item as GridSummaryItem).FieldName.ObjToString();

            int index = e.GroupLevel;

            if (e.SummaryProcess == CustomSummaryProcess.Start)
            {
            }
            else if ( e.SummaryProcess == CustomSummaryProcess.Calculate )
            {
                location = gridMain.GetRowCellValue(e.RowHandle, "location").ObjToString();
            }
            else if (e.SummaryProcess == CustomSummaryProcess.Finalize)
            {
                object value = CalculateTotal ( field, location );
                e.TotalValue = value;
            }
        }
        /***********************************************************************************************/
        private object CalculateTotal( string field, string loc )
        {
            string sum = "";

            if (summaryDt == null)
                return sum;
            if ( String.IsNullOrWhiteSpace ( loc ))
                return sum;

            try
            {
                DataRow[] dRows = summaryDt.Select("location='" + loc + "'");
                if (dRows.Length > 0)
                {
                    /*
                    string sumType = cmbType.Text.Trim();
                    if (sumType == "Trust")
                    {
                        if (field.ToUpper() == "AMOUNT")
                        {
                            string contract = dRows[0]["firstContract"].ObjToString() + " ";
                            sum = contract;
                        }
                        if (field.ToUpper() == "DOWNPAYMENT")
                        {
                            string contract = dRows[0]["lastContract"].ObjToString() + " ";
                            sum = contract;
                        }
                        if (field.ToUpper() == "TRUST")
                        {
                            string contract = "Total" + " " + dRows[0]["count"].ObjToString() + " ";
                            sum = contract;
                        }
                    }
                    else if (sumType == "Insurance")
                    {
                        if (field.ToUpper() == "AMOUNT")
                        {
                            string contract = dRows[0]["firstIns"].ObjToString() + " ";
                            sum = contract;
                        }
                        if (field.ToUpper() == "DOWNPAYMENT")
                        {
                            string contract = dRows[0]["lastIns"].ObjToString() + " ";
                            sum = contract;
                        }
                        if (field.ToUpper() == "TRUST")
                        {
                            string contract = "Total" + " " + dRows[0]["countIns"].ObjToString() + " ";
                            sum = contract;
                        }
                    }
                    else if (sumType == "Cemetery")
                    {
                        if (field.ToUpper() == "AMOUNT")
                        {
                            string contract = dRows[0]["firstCem"].ObjToString() + " ";
                            sum = contract;
                        }
                        if (field.ToUpper() == "DOWNPAYMENT")
                        {
                            string contract = dRows[0]["lastCem"].ObjToString() + " ";
                            sum = contract;
                        }
                        if (field.ToUpper() == "TRUST")
                        {
                            string contract = "Total" + " " + dRows[0]["countCem"].ObjToString() + " ";
                            sum = contract;
                        }
                    }
                    */
                }
            }
            catch ( Exception ex)
            {
            }
            return sum;
        }
        /***********************************************************************************************/
        private void gridMain_CellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            if (e == null)
                return;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            DataRow dr = gridMain.GetFocusedDataRow();
            if (dr == null)
                return;

            DataTable dt = (DataTable)dgv.DataSource;
            string record = dr["record"].ObjToString();
            if (record == "0")
                record = "";

            DateTime date = dr["issueDate8"].ObjToDateTime();
            string sDate = date.ToString("yyyyMMdd");
            string contractNumber = dr["contractNumber"].ObjToString();
            string column = gridMain.FocusedColumn.FieldName;
            string detail = dr[column].ObjToString();

            string cmd = "Select * from `trust_log_data` WHERE `contractNumber` = '" + contractNumber + "' AND `date` = '" + sDate + "' AND `what` = '" + column + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
                record = G1.create_record("trust_log_data", "what", column);
            else
                record = dx.Rows[0]["record"].ObjToString();
            if (G1.BadRecord("trust_log_data", record))
                return;
            G1.update_db_table("trust_log_data", "record", record, new string[] { "contractNumber", contractNumber, "date", date.ToString("yyyy-MM-dd"), "what", column, "detail", detail });

            if ( column.ToUpper() == "ALLOWINSURANCE")
            {
                cmd = "Select * from contracts where contractNumber = '" + contractNumber + "';";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    double contractValue = DailyHistory.GetContractValue(dx.Rows[0]);
                    double allowMerchandise = dx.Rows[0]["allowMerchandise"].ObjToDouble();
                    double allowInsurance = dx.Rows[0]["allowInsurance"].ObjToDouble();
                    double cashAdvance = dx.Rows[0]["cashAdvance"].ObjToDouble();
                    contractValue += allowMerchandise + allowInsurance + cashAdvance;
                    dr["amount"] = contractValue;
                    allowInsurance = detail.ObjToDouble();
                    double contractAmount = contractValue - cashAdvance - allowInsurance;
                    dr["trust"] = contractAmount;

                    if (allowInsurance == contractValue)
                        dr["status"] = "Y";

                    if (cashAdvance > 0D)
                    {
                        allowInsurance += cashAdvance;
                        dr["allowInsurance"] = allowInsurance;
                    }
                    else
                        dr["allowInsurance"] = allowInsurance;
                }
            }

            detail = dr["dataedited"].ObjToString();
            detail += "," + column;
            dr["dataedited"] = detail;

            gridMain.PostEditor();
            gridMain.UpdateTotalSummary();
        }
        /***********************************************************************************************/
        private void gridMain_RowCellStyle(object sender, RowCellStyleEventArgs e )
        {
            GridView View = sender as GridView;
            if (e.Column.FieldName.ToUpper() == "ALLOWINSURANCE")
            {
                string str = View.GetRowCellValue(e.RowHandle, "dataedited").ObjToString();
                if (str != null)
                {
                    if (str.ToUpper().Contains("ALLOWINSURANCE"))
                        e.Appearance.BackColor = Color.LightPink;
                }
            }
        }
        /***********************************************************************************************/
        private void clearInsuranceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //DataTable dt = (DataTable)dgv.DataSource;
            //DataRow dr = gridMain.GetFocusedDataRow();
            //int rowhandle = gridMain.FocusedRowHandle;
            //int row = gridMain.GetDataSourceRowIndex(rowhandle);

            //DateTime date = dr["issueDate8"].ObjToDateTime();
            //string sDate = date.ToString("yyyyMMdd");
            //string contractNumber = dr["contractNumber"].ObjToString();
            //string column = gridMain.FocusedColumn.FieldName;
            //string detail = dr[column].ObjToString();
            //string editData = dr["dataedited"].ObjToString();

            //string cmd = "Select * from `trust_log_data` WHERE `contractNumber` = '" + contractNumber + "' AND `date` = '" + sDate + "' AND `what` = 'allowInsurance';";
            //DataTable dx = G1.get_db_data(cmd);
            //if (dx.Rows.Count > 0)
            //{
            //    string record = dx.Rows[0]["record"].ObjToString();
            //    G1.delete_db_table("trust_log_data", "record", record);

            //    editData = editData.Replace("allowInsurance", "");
            //    dr["dataedited"] = editData;

            //    cmd = "Select * from contracts where contractNumber = '" + contractNumber + "';";
            //    dx = G1.get_db_data(cmd);
            //    if ( dx.Rows.Count > 0 )
            //    {
            //        double contractValue = DailyHistory.GetContractValue(dx.Rows[0]);
            //        double allowMerchandise = dx.Rows[0]["allowMerchandise"].ObjToDouble();
            //        double allowInsurance = dx.Rows[0]["allowInsurance"].ObjToDouble();
            //        double cashAdvance = dx.Rows[0]["cashAdvance"].ObjToDouble();
            //        contractValue += allowMerchandise + allowInsurance + cashAdvance;
            //        dr["amount"] = contractValue;
            //        double contractAmount = contractValue - cashAdvance - allowInsurance;
            //        dr["trust"] = contractAmount;

            //        if (allowInsurance == contractValue)
            //            dr["status"] = "Y";

            //        if (cashAdvance > 0D)
            //        {
            //            allowInsurance += cashAdvance;
            //            dr["allowInsurance"] = allowInsurance;
            //        }
            //        else
            //            dr["allowInsurance"] = allowInsurance;
            //        dt.AcceptChanges();

            //    }


            //    gridMain.PostEditor();
            //    gridMain.UpdateTotalSummary();
            //    gridMain.RefreshEditor(true);
            //    dgv.Refresh();
            //}
        }
        /***********************************************************************************************/
        private void cmbType_SelectedIndexChanged(object sender, EventArgs e)
        {
            gridMain.PostEditor();
            gridMain.UpdateTotalSummary();
            gridMain.RefreshEditor(true);
            dgv.RefreshDataSource();
            dgv.Refresh();
            gridMain.ExpandAllGroups();
        }
        /***********************************************************************************************/
        private void clearLossRecoveryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //DataTable dt = (DataTable)dgv.DataSource;
            //DataRow dr = gridMain.GetFocusedDataRow();
            //int rowhandle = gridMain.FocusedRowHandle;
            //int row = gridMain.GetDataSourceRowIndex(rowhandle);

            //DateTime date = dr["issueDate8"].ObjToDateTime();
            //string sDate = date.ToString("yyyyMMdd");
            //string contractNumber = dr["contractNumber"].ObjToString();
            //string column = gridMain.FocusedColumn.FieldName;
            //string detail = dr[column].ObjToString();
            //string editData = dr["dataedited"].ObjToString();

            //string cmd = "Select * from `trust_log_data` WHERE `contractNumber` = '" + contractNumber + "' AND `date` = '" + sDate + "' AND `what` = 'lossRecovery';";
            //DataTable dx = G1.get_db_data(cmd);
            //if (dx.Rows.Count > 0)
            //{
            //    string record = dx.Rows[0]["record"].ObjToString();
            //    G1.delete_db_table("trust_log_data", "record", record);

            //    dr["lossRecovery"] = "";
            //    dt.AcceptChanges();

            //    gridMain.PostEditor();
            //    gridMain.UpdateTotalSummary();
            //    gridMain.RefreshEditor(true);
            //    dgv.Refresh();
            //}
        }
        /***********************************************************************************************/
        private void clearBooksOrderedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //DataTable dt = (DataTable)dgv.DataSource;
            //DataRow dr = gridMain.GetFocusedDataRow();
            //int rowhandle = gridMain.FocusedRowHandle;
            //int row = gridMain.GetDataSourceRowIndex(rowhandle);

            //DateTime date = dr["issueDate8"].ObjToDateTime();
            //string sDate = date.ToString("yyyyMMdd");
            //string contractNumber = dr["contractNumber"].ObjToString();
            //string column = gridMain.FocusedColumn.FieldName;
            //string detail = dr[column].ObjToString();
            //string editData = dr["dataedited"].ObjToString();

            //string cmd = "Select * from `trust_log_data` WHERE `contractNumber` = '" + contractNumber + "' AND `date` = '" + sDate + "' AND `what` = 'bookOrder';";
            //DataTable dx = G1.get_db_data(cmd);
            //if (dx.Rows.Count > 0)
            //{
            //    string record = dx.Rows[0]["record"].ObjToString();
            //    G1.delete_db_table("trust_log_data", "record", record);

            //    dr["bookOrder"] = "";
            //    dt.AcceptChanges();

            //    ProcessACH ( dt, contractNumber );
            //    ProcessDBR( dt, contractNumber );

            //    gridMain.PostEditor();
            //    gridMain.UpdateTotalSummary();
            //    gridMain.RefreshEditor(true);
            //    dgv.Refresh();
            //}
        }
        /***********************************************************************************************/
        private void btnEditDownPayments_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowhandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowhandle);

            DateTime date = dr["issueDate8"].ObjToDateTime();

            string firstName = dr["firstName"].ObjToString();
            string lastName = dr["lastName"].ObjToString();
            string contractNumber = dr["contractNumber"].ObjToString();

            DownPayments downForm = new DownPayments(contractNumber, lastName, firstName, date, "Edit Deposits");
            downForm.Show();
        }
        /***********************************************************************************************/
		private void chkCollapes_CheckedChanged(object sender, EventArgs e)
        {
            ProcessGroupChange(chkCollapes.Checked, gridMain, "location");
            ProcessGroupChange(chkCollapes.Checked, gridMain3, "location");
            ProcessGroupChange(chkCollapes.Checked, gridMain4, "location");
            ProcessGroupChange(chkCollapes.Checked, gridMain5, "location");
            ProcessGroupChange(chkCollapes.Checked, gridMain6, "location");
            //ProcessGroupChange(chkCollapes.Checked, gridMain9, "location");
        }
        /***********************************************************************************************/
        private void ProcessGroupChange ( bool collape, DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gridMain, string column )
        {
            if ( collape )
            {
                gridMain.OptionsPrint.PrintGroupFooter = true;
                gridMain.OptionsView.ShowFooter = true;
                gridMain.OptionsView.ShowFooter = false;
                gridMain.Columns["location"].GroupIndex = -1;
                gridMain.ExpandAllGroups();
                gridMain.Columns["location"].GroupIndex = 0;
                gridMain.OptionsView.ShowGroupedColumns = false;
                gridMain.OptionsView.GroupFooterShowMode = GroupFooterShowMode.VisibleAlways;
                gridMain.OptionsView.GroupFooterShowMode = GroupFooterShowMode.Hidden; // New as of 4/30/2025
                SetupTotalsSummary();
            }
            else
            {
                gridMain.OptionsPrint.PrintGroupFooter = true;
                gridMain.OptionsView.ShowFooter = true;
                gridMain.Columns["location"].GroupIndex = 0;
                gridMain.OptionsView.ShowGroupedColumns = false;
                gridMain.OptionsView.GroupFooterShowMode = GroupFooterShowMode.VisibleAlways;
                SetupTotalsSummary();
                gridMain.ExpandAllGroups();
            }
        }
        /***********************************************************************************************/
        private string getLocationName(string location)
        {
            DataTable dx = (DataTable)chkComboLocNames.Properties.DataSource;
            DataRow[] dRows = dx.Select("keycode='" + location + "'");
            if (dRows.Length > 0)
                location = dRows[0]["LocationCode"].ObjToString();
            return location;
        }
        /***********************************************************************************************/
        private double totalLocation(DataTable dt, string location = "")
        {
            double total = 0D;
            if (String.IsNullOrWhiteSpace(location))
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                    total += dt.Rows[i]["endingBalance"].ObjToDouble();
            }
            else
            {
                DataRow[] dRows = dt.Select("location='" + location + "'");
                if (dRows.Length > 0)
                {
                    for (int i = 0; i < dRows.Length; i++)
                        total += dRows[i]["endingBalance"].ObjToDouble();
                }
            }
            return total;
        }
        /***********************************************************************************************/
        private void gridMain_CustomDrawGroupRow(object sender, RowObjectCustomDrawEventArgs e) // New as of 4/30/2025
        {
            GridGroupRowInfo info = e.Info as GridGroupRowInfo;
            string location = info.GroupText;
            location = info.GroupValueText.Trim();
            info.GroupText = location;

            var view = (GridView)sender;
            var caption = info.Column.Caption;
            if (info.Column.Caption == string.Empty)
            {
                caption = info.Column.ToString();
            }

            DataTable dt = (DataTable)dgv.DataSource;
            if (dgv3.Visible)
                dt = (DataTable)dgv3.DataSource;
            else if (dgv4.Visible)
                dt = (DataTable)dgv4.DataSource;
            else if (dgv5.Visible)
                dt = (DataTable)dgv5.DataSource;
            else if (dgv6.Visible)
                dt = (DataTable)dgv6.DataSource;

            double total = totalLocation(dt, location);
            total = G1.RoundValue(total);
            string str = G1.ReformatMoney(total);
            caption = getLocationName(location);

            info.GroupText = "Location : (" + location + ")";
            int length = 50 - info.GroupText.Length;
            if (length < 0)
                length = 0;
            if (str.Length < 13)
                length += 13 - str.Length;
            if (!chkCollapes.Checked)
                length = 10;
            info.GroupText += caption.PadRight(length);
            info.GroupText += "$" + str;
        }
        /***********************************************************************************************/
        private void gMain_DoubleClick(object sender, EventArgs e)
        {
            DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gMain = (DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView)sender;
            DataRow dr = gMain.GetFocusedDataRow();
            string contract = dr["contractNumber"].ObjToString();
            if (!String.IsNullOrWhiteSpace(contract))
            {
                this.Cursor = Cursors.WaitCursor;
                using (CustomerDetails clientForm = new CustomerDetails(contract))
                {
                    clientForm.ShowDialog();
                }
                this.Cursor = Cursors.Default;
            }
        }
        /***********************************************************************************************/
        private DataTable originalDt2 = null;
        private void gridMain2_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain2.GetFocusedDataRow();
            string location = dr["location"].ObjToString();
            bool lapsed = false;
            if (location.ToUpper().IndexOf("LAPSED") > 0)
                lapsed = true;

            location = location.Replace("Lapsed", "").Trim();

            DataTable dt = (DataTable)dgv.DataSource;
            if (originalDt2 == null)
                originalDt2 = dt;

            string lookup = "location='" + location + "'";
            if (lapsed)
                lookup += " AND lapsed <> ''";
            DataRow[] dRows = originalDt2.Select(lookup);
            if (dRows.Length <= 0)
            {
                lookup = "serviceLoc LIKE '" + location + "%'";
                if (lapsed)
                    lookup += " AND lapsed <> ''";
                dRows = originalDt2.Select(lookup);
            }
            if (dRows.Length > 0)
            {
                DataTable ddd = dRows.CopyToDataTable();
                dgv.DataSource = ddd;
                dgv.Refresh();
                tabControl1.SelectedIndex = 0;
                gridMain.ExpandAllGroups();

                chkRestoreDetail.Show();
                chkRestoreDetail.Refresh();
            }
        }
        /***********************************************************************************************/
        private void SetupSelectedColumns(string procType, string group, DevExpress.XtraGrid.GridControl dgv)
        {
            if (String.IsNullOrWhiteSpace(group))
                return;
            if (String.IsNullOrWhiteSpace(procType))
                procType = "PreNeed";
            string cmd = "Select * from procfiles where name = '" + group + "' and ProcType = '" + procType + "' order by seq";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return;
            DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gridMain = (DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView)dgv.MainView;
            for (int i = 0; i < gridMain.Columns.Count; i++)
                gridMain.Columns[i].Visible = false;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string name = dt.Rows[i]["Description"].ToString();
                int index = dt.Rows[i]["seq"].ObjToInt32();
                try
                {
                    if (G1.get_column_number((GridView)dgv.MainView, name) >= 0)
                        ((GridView)dgv.MainView).Columns[name].Visible = true;
                }
                catch
                {
                }
            }
        }
        /***********************************************************************************************/
        private void cmbSelectColumns_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            System.Windows.Forms.ComboBox combo = (System.Windows.Forms.ComboBox)sender;
            string comboName = combo.Text;
            string skinName = "";
            if (!String.IsNullOrWhiteSpace(comboName))
            {
                SetupSelectedColumns("TrustEOY", comboName, dgv);
                string name = "TrustEOY " + comboName;
                foundLocalPreference = G1.RestoreGridLayout(this, this.dgv, gridMain, LoginForm.username, name, ref skinName);
            }
            else
            {
                SetupSelectedColumns("TrustEOY", "Primary", dgv);
                string name = "TrustEOY Primary";
                foundLocalPreference = G1.RestoreGridLayout(this, this.dgv, gridMain, LoginForm.username, name, ref skinName);
            }
        }
        /***********************************************************************************************/
        private void btnSelectColumns_Click(object sender, EventArgs e)
        {
            string actualName = cmbSelectColumns.Text;

            SelectDisplayColumns sform = new SelectDisplayColumns(dgv, "TrustEOY", "Primary", actualName);
            sform.Done += new SelectDisplayColumns.d_void_selectionDone(sxform_Done);
            sform.Show();
        }
        /***********************************************************************************************/
        void sxform_Done(DataTable dt)
        {
            string name = cmbSelectColumns.Text.Trim();
            if (String.IsNullOrWhiteSpace(name))
                name = "Primary";
            string saveName = "TrustEOY";
            string skinName = "";
            SetupSelectedColumns("TrustEOY", name, dgv);
            foundLocalPreference = G1.RestoreGridLayout(this, this.dgv, gridMain, LoginForm.username, saveName, ref skinName);
            //gridMain.OptionsView.ShowFooter = showFooters;
            //SetupTotalsSummary();
            string field = "";
            string select = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                field = dt.Rows[i]["field"].ObjToString();
                if (String.IsNullOrWhiteSpace(field))
                    continue;
                select = dt.Rows[i]["select"].ObjToString();
                if (G1.get_column_number(gridMain, field) >= 0)
                {
                    if (select == "0")
                        gridMain.Columns[field].Visible = false;
                    else
                        gridMain.Columns[field].Visible = true;
                }
            }
            dgv.Refresh();
            this.Refresh();
        }
        /***********************************************************************************************/
        private void loadGroupCombo(System.Windows.Forms.ComboBox cmb, string key, string module)
        {
            string primaryName = "";
            cmb.Items.Clear();
            string cmd = "Select * from procfiles where ProcType = '" + key + "' AND `module` = '" + module + "' group by name;";
            DataTable dt = G1.get_db_data(cmd);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string name = dt.Rows[i]["Name"].ToString();
                if (name.Trim().ToUpper() == "PRIMARY")
                    primaryName = name;
                cmb.Items.Add(name);
            }
            if (!String.IsNullOrWhiteSpace(primaryName))
                cmb.Text = primaryName;
        }
        /***********************************************************************************************/
        private void lockScreenFormatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string name = cmbSelectColumns.Text.Trim();
            if (String.IsNullOrWhiteSpace(name))
                name = "Primary";
            string saveName = "TrustEOY " + name;
            G1.SaveLocalPreferences(this, gridMain, LoginForm.username, saveName);
        }
        /***********************************************************************************************/
        private void unlockScreenFormatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string comboName = cmbSelectColumns.Text;
            if (!String.IsNullOrWhiteSpace(comboName))
            {
                string name = "TrustEOY " + comboName;
                G1.RemoveLocalPreferences(LoginForm.username, name);
                foundLocalPreference = false;
            }
        }
        /***********************************************************************************************/
        private DataTable LoadAnnuityLocations( DataTable dt)
        {
            string file = @"C:\SMFSdata\Annuity Location Allocation.xlsx";
            DataTable excelDt = ExcelWriter.ReadFile2(file, 0, "Sheet1");
            if (excelDt == null)
                return dt;

            string contractNumber = "";
            string location = "";
            DataRow[] dRows = null;
            DataRow[] nRows = null;
            try
            {
                for (int i = 1; i < excelDt.Rows.Count; i++)
                {
                    contractNumber = excelDt.Rows[i][0].ObjToString();
                    if (String.IsNullOrWhiteSpace(contractNumber))
                        continue;
                    location = excelDt.Rows[i][1].ObjToString().ToUpper();
                    if (String.IsNullOrWhiteSpace(location))
                        continue;

                    dRows = dt.Select("contractNumber='" + contractNumber + "'");
                    if ( dRows.Length > 0 )
                    {
                        nRows = funDt.Select("LocationCode='" + location + "'");
                        if (nRows.Length > 0)
                            dRows[0]["location"] = nRows[0]["keyCode"].ObjToString();
                    }
                }
            }
            catch ( Exception ex)
            {
            }
            return dt;
        }
        /***********************************************************************************************/
        private void chkRestoreDetail_CheckedChanged(object sender, EventArgs e)
        {
            dgv.DataSource = originalDt;
            dgv.Refresh();
            tabControl1.SelectedIndex = 0;
            gridMain.ExpandAllGroups();
            chkRestoreDetail.Hide();
        }
        /***********************************************************************************************/
        private void gridMain9_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain9.GetFocusedDataRow();
            string location = dr["location"].ObjToString();
            bool lapsed = false;
            if (location.ToUpper().IndexOf("LAPSED") > 0)
                lapsed = true;

            location = location.Replace("Lapsed", "").Trim();

            DataTable dt = (DataTable)dgv.DataSource;
            if (originalDt2 == null)
                originalDt2 = dt;

            string lookup = "location='" + location + "'";
            if (lapsed)
                lookup += " AND lapsed <> ''";
            DataRow[] dRows = originalDt2.Select(lookup);
            if (dRows.Length <= 0)
            {
                lookup = "serviceLoc LIKE '" + location + "%'";
                if (lapsed)
                    lookup += " AND lapsed <> ''";
                dRows = originalDt2.Select(lookup);
            }
            if (dRows.Length > 0)
            {
                DataTable ddd = dRows.CopyToDataTable();
                dgv.DataSource = ddd;
                dgv.Refresh();
                tabControl1.SelectedIndex = 0;
                gridMain.ExpandAllGroups();

                chkRestoreDetail.Show();
                chkRestoreDetail.Refresh();
            }
        }
        /***********************************************************************************************/
    }
}