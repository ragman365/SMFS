using DevExpress.CodeParser;
using DevExpress.Pdf;
using DevExpress.Utils;
using DevExpress.Utils.Drawing;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraEditors.ViewInfo;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Controls;
using DevExpress.XtraGrid.Views.BandedGrid;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports.UI;
using GeneralLib;
using System;
using System.Data;
using System.Drawing;
using System.Web.UI.WebControls;
using System.Windows.Forms;
using GridView = DevExpress.XtraGrid.Views.Grid.GridView;
using iTextSharp.text.pdf;
using System.IO;
using System.Text.RegularExpressions;
using DevExpress.Data;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using MySql.Data.MySqlClient;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class FunArrangerTotals : DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        private Color menuBackColor = Color.Gray;
        private DataTable workDt = null;
        private DataTable originalDt = null;
        private string workReport = "";
        private string workManager = "";
        private string workLocation = "";
        private string workArranger = "";
        private string workWho = "";
        private string workArrangerFirstName = "";
        private string workArrangerLastName = "";
        private bool workingManagers = false;
        private bool workingArrangers = false;
        private DataTable oDt = null;
        private bool loading = false;
        private bool insurance = false;
        private bool foundLocalPreference = false;
        private GridView originalGridView = null;
        private bool showFooters = true;
        private string serviceLocs = "";
        private bool workPDF = false;
        private DataTable[] Alldbs = new DataTable [200];
        private int dbCount = 0;
        private bool byPass = false;
        private DataTable summaryDt = null;
        private DataTable funeralHomes = null;
        private bool historicBonus = false;
        private DataTable timDt = null;
        private DataTable badDt = null;
        /***********************************************************************************************/
        public FunArrangerTotals ()
        {
            InitializeComponent();
            workDt = null;
            workManager = "";
            workLocation = "";
            workWho = "";
        }
        /***********************************************************************************************/
        RepositoryItemDateEdit ri = null;
        private void FunArrangerTotals_Load(object sender, EventArgs e)
        {
            menuBackColor = menuStrip1.BackColor;

            loading = true;

            barImport.Hide();

            ri = new RepositoryItemDateEdit();
            ri.VistaEditTime = DevExpress.Utils.DefaultBoolean.True;
            ri.Mask.MaskType = DevExpress.XtraEditors.Mask.MaskType.DateTime;
            ri.Mask.UseMaskAsDisplayFormat = true;
            ri.Mask.EditMask = @"yyyy-MM-dd hh-mm";

            string name = G1.GetUserFullName();

            //string title = "Funeral Commission for ";
            //if (workDt == null)
            //{
            //    if (workWho.ToUpper() == "M")
            //    {
            //        title += " " + workManager + " as Manager";
            //        workingManagers = true;
            //    }
            //    else if (workWho.ToUpper() == "A")
            //    {
            //        workingArrangers = true;
            //        if (!String.IsNullOrWhiteSpace(workArranger))
            //            title += " " + workArranger + " as Arranger";
            //        else
            //            title += " " + workManager + " as Arranger";
            //    }
            //}

            //this.Text = title;

            showFooters = true;
            string preference = G1.getPreference(LoginForm.username, "Funerals CB Chooser", "Allow Access");
            if (preference != "YES")
                showFooters = false;

            string prefix = "";
            string suffix = "";
            string firstName = "";
            string lastName = "";
            string mi = "";

            G1.ParseOutName(workManager, ref prefix, ref firstName, ref lastName, ref mi, ref suffix);

            if (!String.IsNullOrWhiteSpace(firstName))
                workArrangerFirstName = firstName;
            if (!String.IsNullOrWhiteSpace(lastName))
                workArrangerLastName = lastName;

            workArranger = firstName + " " + lastName;

            loadLocatons();

            SetupTotalsSummary();

            loading = false;

            DateTime today = DateTime.Now;
            int days = DateTime.DaysInMonth(today.Year, today.Month);
            this.dateTimePicker2.Value = new DateTime(today.Year, today.Month, days);
            this.dateTimePicker1.Value = new DateTime(today.Year, today.Month, 1);

            this.Refresh();
            gridMain.RefreshEditor(true);

            //cmbSelectColumns_SelectedIndexChanged(cmbSelectColumns, null);

            gridMain.ShowCustomizationForm += GridMain_ShowCustomizationForm;

            SetupServiceLocs();
        }
        /****************************************************************************************/
        private void SetupServiceLocs ()
        {
            serviceLocs = "";
            string cmd = "Select * from `funeralhomes` where `manager` = '" + workManager + "' ";
            if (!String.IsNullOrWhiteSpace(workLocation))
                cmd += " AND `LocationCode` = '" + workLocation + "' ";
            cmd += " ;";

            DataTable funDt = G1.get_db_data( cmd );
            if (funDt.Rows.Count <= 0)
                return;

            string atNeedCode = "";
            for ( int i=0; i<funDt.Rows.Count; i++)
            {
                atNeedCode = funDt.Rows[i]["atneedcode"].ObjToString();
                if ( !String.IsNullOrWhiteSpace ( atNeedCode ))
                {
                    if (!String.IsNullOrWhiteSpace(serviceLocs))
                        serviceLocs += ",";
                    serviceLocs += "'" + atNeedCode + "'";
                }
            }
        }
        /****************************************************************************************/
        private void AddSummaryItem ( string fieldName )
        {
            bool found = false;
            string field = "";
            for (int i = 0; i < gridMain.GroupSummary.Count; i++)
            {
                field = gridMain.GroupSummary[i].FieldName;
                if ( field == fieldName )
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                if (G1.getGridColumnIndex(gridMain, fieldName) >= 0)
                {
                    GridGroupSummaryItem item = new GridGroupSummaryItem(DevExpress.Data.SummaryItemType.Sum, fieldName, gridMain.Columns[fieldName], "{0:N0}");
                    gridMain.GroupSummary.Add(item);
                }
            }
        }
        /****************************************************************************************/
        private void ClearAllPositions(DevExpress.XtraGrid.Views.BandedGrid.BandedGridView gMain = null)
        {
            if (gMain == null)
                gMain = gridMain;
            string name = "";
            for (int i = 0; i < gMain.Columns.Count; i++)
            {
                name = gMain.Columns[i].Name.ToUpper();
                if ( name != "NUM" )
                    gMain.Columns[i].Visible = false;
                else
                    gMain.Columns[i].Visible = true;
                gridMain.Columns[i].OptionsColumn.FixedWidth = true;
            }
        }
        /****************************************************************************************/
        private void ClearAllPositions(DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gMain = null)
        {
            if (gMain == null)
                gMain = (AdvBandedGridView) gridMain;
            string name = "";
            for (int i = 0; i < gMain.Columns.Count; i++)
            {
                name = gMain.Columns[i].Name.ToUpper();
                if (name != "NUM")
                    gMain.Columns[i].Visible = false;
                else
                    gMain.Columns[i].Visible = true;
                gridMain.Columns[i].OptionsColumn.FixedWidth = true;
            }
        }
        /***********************************************************************************************/
        private void GridMain_ShowCustomizationForm(object sender, EventArgs e)
        {
            if (!showFooters)
            {
                gridMain.DestroyCustomization();
            }
        }
        /***********************************************************************************************/
        private void loadLocatons()
        {
            string cmd = "Select * from `funeralhomes` order by `keycode`;";
            DataTable locDt = G1.get_db_data(cmd);

            DataTable newLocDt = locDt.Clone();

            string assignedLocations = "";

            cmd = "Select * from `users` where `username` = '" + LoginForm.username + "';";
            DataTable userDt = G1.get_db_data(cmd);
            if ( userDt.Rows.Count > 0 )
                assignedLocations = userDt.Rows[0]["assignedLocations"].ObjToString();

            string locationCode = "";
            string keyCode = "";
            string[] Lines = null;
            string locations = "";
            string location = "";

            for ( int i=locDt.Rows.Count-1; i>=0; i--)
            {
                keyCode = locDt.Rows[i]["keycode"].ObjToString();
                if (keyCode.IndexOf("-") > 0)
                    locDt.Rows.RemoveAt(i);
            }
            for (int i = 0; i < locDt.Rows.Count; i++)
            {
                locationCode = locDt.Rows[i]["locationCode"].ObjToString();
                if (String.IsNullOrWhiteSpace(locationCode))
                    continue;
                Lines = assignedLocations.Split('~');
                for (int j = 0; j < Lines.Length; j++)
                {
                    location = Lines[j].Trim();
                    if (String.IsNullOrWhiteSpace(location))
                        continue;
                    if (location.ToUpper() == locationCode.ToUpper())
                    {
                        location = locDt.Rows[i]["atNeedCode"].ObjToString();
                        locations += location + "|";
                        newLocDt.ImportRow(locDt.Rows[i]);
                    }
                }
            }
            if (!LoginForm.administrator)
                locDt = newLocDt;

            chkComboLocation.Properties.DataSource = locDt;

            locations = locations.TrimEnd('|');
            chkComboLocation.EditValue = locations;
            chkComboLocation.Text = locations;
        }
        /****************************************************************************************/
        private void SetupTotalsSummary()
        {
            if (!showFooters)
            {
                gridMain.GroupSummary.Clear();
                return;
            }
            //AddSummaryColumn("payment", null);
        }
        /****************************************************************************************/
        private void AddSummaryColumn(string columnName, DevExpress.XtraGrid.Views.Grid.GridView gMain = null, string summaryItemType = "", string format = "" )
        {
            if (gMain == null)
                gMain = gridMain;
            if (String.IsNullOrWhiteSpace(format))
                format = "{0:N2}";
            if (summaryItemType.ToUpper() == "CUSTOM")
            {
                gMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Custom;
                GridSummaryItem item = null;
                bool found = false;
                for ( int i=0; i<gMain.GroupSummary.Count; i++)
                {
                    item = gMain.GroupSummary[i];
                    if ( item.FieldName == columnName)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    gMain.GroupSummary.Add(new DevExpress.XtraGrid.GridGroupSummaryItem(DevExpress.Data.SummaryItemType.Custom, columnName, gMain.Columns[columnName], format));
                }
            }
            else
                gMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            gMain.Columns[columnName].SummaryItem.DisplayFormat = format;
        }
        /*******************************************************************************************/
        private string getLocationQuery()
        {
            DataRow[] dRows = null;
            DataTable locDt = (DataTable) this.chkComboLocation.Properties.DataSource;
            string procLoc = "";
            string jewelLoc = "";
            string[] locIDs = this.chkComboLocation.EditValue.ToString().Split('|');
            for (int i = 0; i < locIDs.Length; i++)
            {
                if (!String.IsNullOrWhiteSpace(locIDs[i]))
                {
                    if (procLoc.Trim().Length > 0)
                        procLoc += ",";
                    procLoc += "'" + locIDs[i].Trim() + "'";
                    dRows = locDt.Select("atneedcode='" + locIDs[i].Trim() + "'");
                    if ( dRows.Length > 0 )
                    {
                        jewelLoc = dRows[0]["merchandiseCode"].ObjToString();
                        if ( !String.IsNullOrWhiteSpace ( jewelLoc ))
                            procLoc += ",'" + jewelLoc.Trim() + "'";
                    }
                }
            }
            return procLoc.Length > 0 ? " serviceLoc IN (" + procLoc + ") " : "";
        }
        /***********************************************************************************************/
        private DataTable CombineLocations ( DataTable dt, string fromLoc, string toLoc )
        {
            if (dt == null)
                return dt;
            DataRow[] dRows = dt.Select("serviceLoc='" + fromLoc + "'");
            if ( dRows.Length > 0 )
            {
                for (int i = 0; i < dRows.Length; i++)
                    dRows[i]["serviceLoc"] = toLoc;
            }
            return dt;
        }
        /***********************************************************************************************/
        private void ProcessExcludes ( DataTable dt )
        {
            string classification = "";
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                classification = dt.Rows[i]["funeral_classification"].ObjToString();
                if (classification.ToUpper().IndexOf("PICKUP") >= 0)
                    dt.Rows[i]["Exclude"] = "Y";
                else 
                {
                    if (classification.ToUpper().IndexOf("OTHER -") >= 0)
                    {
                        if (classification.ToUpper().IndexOf("TENT AND CHAIR") >= 0)
                            dt.Rows[i]["Exclude"] = "Y";
                    }
                }
            }
        }
        /***********************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if ( dgv.Visible )
                G1.ShowHideFindPanel(gridMain);
        }
        /***********************************************************************************************/
        private int pageMarginLeft = 0;
        private int pageMarginRight = 0;
        private int pageMarginTop = 0;
        private int pageMarginBottom = 0;
        /***********************************************************************************************/
        private void printPreviewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            printPreview(false);
        }
        /***********************************************************************************************/
        private int printRow = 0;
        private void printPreview ( bool batch = true )
        {
            printRow = 0;

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

            //            Printer.setupPrinterMargins(50, 50, 80, 50);
            Printer.setupPrinterMargins(10, 5, 80, 50);

            pageMarginLeft = Printer.pageMarginLeft;
            pageMarginRight = Printer.pageMarginRight;
            pageMarginTop = Printer.pageMarginTop;
            pageMarginBottom = Printer.pageMarginBottom;

            printableComponentLink1.Margins.Left = pageMarginLeft;
            printableComponentLink1.Margins.Right = pageMarginRight;
            printableComponentLink1.Margins.Top = pageMarginTop;
            printableComponentLink1.Margins.Bottom = pageMarginBottom;

            printingSystem1.Document.AutoFitToPagesWidth = 1;

            printableComponentLink1.Landscape = true;

            printableComponentLink1.CreateDocument();

            if (workPDF && batch )
            {
                string filename = "";
                //string filename = path + @"\" + workReport + "_" + today.Year.ToString("D4") + today.Month.ToString("D2") + today.Day.ToString("D2") + ".pdf";

                filename = @"C:/rag/pdfDaily.pdf";
                //filename = workPDFfile;
                if (File.Exists(filename))
                {
                    File.SetAttributes(filename, FileAttributes.Normal);
                    File.Delete(filename);
                }
                printableComponentLink1.ExportToPdf(filename);
            }
            else
                printableComponentLink1.ShowPreview();

        }
        /***********************************************************************************************/
        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            printRow = 0;

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

            Printer.setupPrinterMargins(10, 5, 80, 50);

            pageMarginLeft = Printer.pageMarginLeft;
            pageMarginRight = Printer.pageMarginRight;
            pageMarginTop = Printer.pageMarginTop;
            pageMarginBottom = Printer.pageMarginBottom;

            printableComponentLink1.Margins.Left = pageMarginLeft;
            printableComponentLink1.Margins.Right = pageMarginRight;
            printableComponentLink1.Margins.Top = pageMarginTop;
            printableComponentLink1.Margins.Bottom = pageMarginBottom;
            printableComponentLink1.Landscape = true;

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
            string title = this.Text;

            Printer.DrawQuad(6, 8, 6, 4, title, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

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
        /***********************************************************************************************/
        private void SetupSelectedColumns(string procType, string group, DevExpress.XtraGrid.GridControl dgv)
        {
            if (String.IsNullOrWhiteSpace(group))
                return;
            if (String.IsNullOrWhiteSpace(procType))
                procType = "AR " + workReport;
            string user = LoginForm.username;
            if (group.ToUpper().IndexOf("(C)") >= 0)
            {
                user = "Common";
                group = group.Replace("(C) ", "");
            }
            string cmd = "Select * from procfiles where name = '" + group + "' and ProcType = '" + procType + "' AND `user` = '" + user + "' order by seq";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
            {
                cmd = "Select * from procfiles where name = '" + group + "' and ProcType = '" + procType + "' AND ( `user` = 'Common' OR `user` = '' ) order by seq";
                dt = G1.get_db_data(cmd);
            }
            if (dt.Rows.Count <= 0)
                return;
            DevExpress.XtraGrid.Views.Grid.GridView gridMain = (DevExpress.XtraGrid.Views.Grid.GridView)dgv.MainView;
            for (int i = 0; i < gridMain.Columns.Count; i++)
                gridMain.Columns[i].Visible = false;
            DataTable ddx = (DataTable)dgv.DataSource;
            int idx = 0;
            string name = "";
            int index = 0;
            for (int i = (dt.Rows.Count - 1); i >= 0; i--)
            {
                name = dt.Rows[i]["Description"].ToString();
                index = dt.Rows[i]["seq"].ObjToInt32();
                try
                {
                    idx = G1.get_column_number(gridMain, name);
                    if (idx >= 0)
                        gridMain.Columns[name].Visible = true;
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
            ComboBox combo = (ComboBox)sender;
            string comboName = combo.Text;
            string skinName = "";
            if (!String.IsNullOrWhiteSpace(comboName))
            {
                SetupSelectedColumns("AR", comboName, dgv);
                string name = "AR " + comboName;
                foundLocalPreference = G1.RestoreGridLayout(this, this.dgv, gridMain, LoginForm.username, name, ref skinName);
                SetupTotalsSummary();
                gridMain.OptionsView.ShowFooter = showFooters;
            }
            else
            {
                SetupSelectedColumns("AR", "Primary", dgv);
                string name = "AR Primary";
                foundLocalPreference = G1.RestoreGridLayout(this, this.dgv, gridMain, LoginForm.username, name, ref skinName);
                gridMain.OptionsView.ShowFooter = showFooters;
                SetupTotalsSummary();
            }

            string format = chkComboLocation.Text;
            if (!String.IsNullOrWhiteSpace(format))
                chkComboLocation_EditValueChanged(null, null);
        }
        /***********************************************************************************************/
        void sform_Done(DataTable dt)
        {
            string name = cmbSelectColumns.Text.Trim();
            if (String.IsNullOrWhiteSpace(name))
                name = "Primary";
            string saveName = "AR " + name;
            string skinName = "";
            SetupSelectedColumns("AR", name, dgv);
            foundLocalPreference = G1.RestoreGridLayout(this, this.dgv, gridMain, LoginForm.username, saveName, ref skinName);
            gridMain.OptionsView.ShowFooter = showFooters;
            SetupTotalsSummary();
            string field = "";
            string select = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                field = dt.Rows[i]["field"].ObjToString();
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
        private void btnSelectColumns_Click_1(object sender, EventArgs e)
        {
            string actualName = cmbSelectColumns.Text;
            if (actualName.ToUpper().IndexOf("(C)") >= 0 && !LoginForm.administrator)
            {
                MessageBox.Show("***Warning*** You do not have permission to modify a Common Display Format!", "Display Format Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            //            SelectColumns sform = new SelectColumns(dgv, "Funerals", "Primary", actualName);
            string user = LoginForm.username;
            SelectDisplayColumns sform = new SelectDisplayColumns(dgv, "AR " + workReport , "Primary", actualName, LoginForm.username);
            sform.Done += new SelectDisplayColumns.d_void_selectionDone(sform_Done);
            sform.Show();
        }
        /***********************************************************************************************/
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            string name = cmbSelectColumns.Text.Trim();
            if (String.IsNullOrWhiteSpace(name))
                name = "Primary";
            string saveName = "AR " + workReport + " " + name;
            G1.SaveLocalPreferences(this, gridMain, LoginForm.username, saveName);
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
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            this.gridMain.ExpandAllGroups();
            gridMain.OptionsView.ShowFooter = showFooters;
        }
        /***********************************************************************************************/
        private void toolStripRemoveFormat_Click(object sender, EventArgs e)
        {
            string comboName = cmbSelectColumns.Text;
            if (!String.IsNullOrWhiteSpace(comboName))
            {
                string name = "AR " + workReport + " " + comboName;
                G1.RemoveLocalPreferences(LoginForm.username, name);
                foundLocalPreference = false;
            }
        }
        /****************************************************************************************/
        private void txtScale_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string balance = txtScale.Text.Trim();
                if (!G1.validate_numeric(balance))
                {
                    MessageBox.Show("***ERROR*** Scale must be numeric!");
                    return;
                }
                double money = balance.ObjToDouble();
                balance = G1.ReformatMoney(money);
                txtScale.Text = balance;
                ScaleCells();
                return;
            }
            // Initialize the flag to false.
            bool nonNumberEntered = false;

            // Determine whether the keystroke is a number from the top of the keyboard.
            if (e.KeyCode < Keys.D0 || e.KeyCode > Keys.D9)
            {
                // Determine whether the keystroke is a number from the keypad.
                if (e.KeyCode < Keys.NumPad0 || e.KeyCode > Keys.NumPad9)
                {
                    // Determine whether the keystroke is a backspace.
                    if (e.KeyCode != Keys.Back)
                    {
                        // A non-numerical keystroke was pressed.
                        // Set the flag to true and evaluate in KeyPress event.
                        if (e.KeyCode != Keys.OemPeriod)
                            nonNumberEntered = true;
                    }
                }
            }
            //If shift key was pressed, it's not a number.
            if (Control.ModifierKeys == Keys.Shift)
            {
                nonNumberEntered = true;
            }
            if (nonNumberEntered)
            {
                MessageBox.Show("***ERROR*** Key entered must be a number!");
                e.Handled = true;
            }
        }
        /****************************************************************************************/
        private double originalSize = 0D;
        private Font mainFont = null;
        private Font newFont = null;
        private void ScaleCells()
        {
            try
            {
                if (originalSize == 0D)
                {
                    originalSize = gridMain.Columns["Funeral Arranger"].AppearanceCell.Font.Size;
                    mainFont = gridMain.Columns["Funeral Arranger"].AppearanceCell.Font;
                }
                double scale = txtScale.Text.ObjToDouble();
                double size = scale / 100D * originalSize;
                Font font = new Font(mainFont.Name, (float)size);
                for (int i = 0; i < gridMain.Columns.Count; i++)
                {
                    gridMain.Columns[i].AppearanceCell.Font = font;
                }

                gridMain.Appearance.GroupFooter.Font = font;
                gridMain.AppearancePrint.FooterPanel.Font = font;
                newFont = font;
            }
            catch (Exception ex)
            {
            }
            gridMain.RefreshData();
            gridMain.RefreshEditor(true);
            dgv.Refresh();
            this.Refresh();
        }
        /***********************************************************************************************/
        private void gridMain_CustomDrawFooterCell(object sender, FooterCellCustomDrawEventArgs e)
        {
            if (newFont != null)
                e.Appearance.Font = newFont;
        }
        /***********************************************************************************************/
        private void btnLeft_Click(object sender, EventArgs e)
        {
            DateTime now = this.dateTimePicker2.Value;
            DateTime date = new DateTime(now.Year, now.Month, 1);
            date = date.AddMonths(-1);
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            date = new DateTime(date.Year, date.Month, days);
            this.dateTimePicker2.Value = date;
            this.dateTimePicker1.Value = new DateTime(date.Year, date.Month, 1 );
        }
        /***********************************************************************************************/
        private void btnRight_Click(object sender, EventArgs e)
        {
            DateTime now = this.dateTimePicker2.Value;
            DateTime date = new DateTime(now.Year, now.Month, 1);
            date = date.AddMonths(1);
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            date = new DateTime(date.Year, date.Month, days);
            this.dateTimePicker2.Value = date;
            this.dateTimePicker1.Value = new DateTime(date.Year, date.Month, 1);
        }
        /***********************************************************************************************/
        private int footerCount = 0;
        private string majorLastLocation = "";
        private string lastLocation = "";
        private string majorLastDetail = "";
        private bool firstPrint = true;
        private bool gotFooter = true;
        /***********************************************************************************************/
        private void gridMain_BeforePrintRow(object sender, DevExpress.XtraGrid.Views.Printing.CancelPrintRowEventArgs e)
        {

            bool got = false;
            if (gotFooter)
            {
                gotFooter = false;
            }

            if (e.HasFooter)
            {
                got = true;
                footerCount++;
            }
        }
        /***********************************************************************************************/
        private void gridMain_AfterPrintRow(object sender, DevExpress.XtraGrid.Views.Printing.PrintRowEventArgs e)
        {
            if (e.HasFooter)
            {
                gotFooter = true;

                footerCount++;
                if (footerCount >= 1)
                {
                    if (!historicBonus)
                    {
                        string detail = FindLastLocation(e);
                        AddHeading((DevExpress.XtraGrid.Views.Printing.CancelPrintRowEventArgs)e, detail);
                    }

                    footerCount = 0;
                    //e.PS.InsertPageBreak(e.Y);
                    //printRow = 0;
                }
            }
        }
        /***********************************************************************************************/
        //private void AddHeading(DevExpress.XtraGrid.Views.Printing.PrintRowEventArgs e, string detail)
        //{
        //    TextBrick tb = e.PS.CreateTextBrick() as TextBrick;
        //    //tb.Text = majorLastDetail;
        //    tb.Text = detail;
        //    tb.Font = new Font(tb.Font, FontStyle.Bold);
        //    tb.HorzAlignment = DevExpress.Utils.HorzAlignment.Near;
        //    //tb.Padding = new PaddingInfo(5, 0, 0, 0);
        //    tb.BackColor = Color.LightGray;
        //    tb.ForeColor = Color.Black;
        //    // Get the client page width. 
        //    SizeF clientPageSize = (e.BrickGraphics as BrickGraphics).ClientPageSize;
        //    float textBrickHeight = e.Graphics.MeasureString(tb.Text, tb.Font).Height + 4;
        //    // Calculate a rectangle for the brick and draw the brick. 
        //    tb.Padding = new PaddingInfo(0, 0, 0, 0);

        //    int y = e.Y;

        //    RectangleF textBrickRect = new RectangleF(0, y, (int)clientPageSize.Width, textBrickHeight);
        //    e.BrickGraphics.DrawBrick(tb, textBrickRect);
        //    // Adjust the current Y position to print the following row below the brick. 
        //    //e.Y += (int)textBrickHeight;

        //    //if (printRow > 0)
        //    //    e.Y += (int)textBrickHeight * printRow;
        //    printRow++;
        //}
        /***********************************************************************************************/
        private void AddHeading(DevExpress.XtraGrid.Views.Printing.CancelPrintRowEventArgs e, string detail )
        {
            TextBrick tb = e.PS.CreateTextBrick() as TextBrick;
            //tb.Text = majorLastDetail;
            tb.Text = detail;
            //Font font = tb.Font;
            //font = new Font(font.Name, 16F, FontStyle.Bold);

            tb.Font = new Font(tb.Font.Name, 16F, FontStyle.Regular);
            tb.HorzAlignment = DevExpress.Utils.HorzAlignment.Near;
            //tb.Padding = new PaddingInfo(5, 0, 0, 0);
            tb.BackColor = Color.LightGray;
            tb.ForeColor = Color.Black;
            // Get the client page width. 
            SizeF clientPageSize = (e.BrickGraphics as BrickGraphics).ClientPageSize;
            float textBrickHeight = e.Graphics.MeasureString(tb.Text, tb.Font).Height + 4;
            float textBrickWidth = e.Graphics.MeasureString(tb.Text, tb.Font).Width + 4;
            textBrickHeight = 30f;
            // Calculate a rectangle for the brick and draw the brick. 
            tb.Padding = new PaddingInfo(0, 0, 0, 0);

            int y = e.Y;
            //if (printRow >= 2)
            //    y += 5;

            //RectangleF textBrickRect = new RectangleF(0, y, (int)clientPageSize.Width, textBrickHeight);
            RectangleF textBrickRect = new RectangleF(0, y, textBrickWidth, textBrickHeight);
            e.BrickGraphics.DrawBrick(tb, textBrickRect);
            // Adjust the current Y position to print the following row below the brick. 
            // e.Y += (int)textBrickHeight;

            //if (printRow > 0)
            e.Y += (int) textBrickHeight;
            printRow++;
        }
        /***********************************************************************************************/
        private string FindLastLocation(DevExpress.XtraGrid.Views.Printing.PrintRowEventArgs e)
        {
            majorLastLocation = majorLastDetail;
            lastLocation = "";

            try
            {
                DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gMain = gridMain;
                DataTable dt = (DataTable)dgv.DataSource;
                int rowHandle = e.RowHandle;
                int row = gMain.GetDataSourceRowIndex(rowHandle);
                lastLocation = dt.Rows[row]["serviceLoc"].ObjToString();
                if ( lastLocation == "BK")
                {
                }

                string manager = dt.Rows[row]["manager"].ObjToString();

                //row = printRow;
                //lastLocation = summaryDt.Rows[row]["serviceLoc"].ObjToString();
                //manager = summaryDt.Rows[row]["manager"].ObjToString();

                DataRow[] dRows = summaryDt.Select("serviceLoc='" + lastLocation + "'");
                if (dRows.Length > 0)
                    majorLastDetail = dRows[0]["detail"].ObjToString();
                //DataRow[] dRows = pre2002Dt.Select("locind='" + lastLocation + "'");
                //if (dRows.Length > 0)
                //{
                //    lastLocation = dRows[0]["name"].ObjToString();
                //    majorLastDetail = lastLocation;
                //}
                //}
            }
            catch ( Exception ex)
            {
            }
            return majorLastDetail;
        }
        /***********************************************************************************************/
        //private DataTable LoadArrangerData( DataTable localDt )
        //{
        //    string cmd = "Select * from `funcommissiondata` where `name` = '" + workManager + "' OR `name` = '" + workArranger + "';";
        //    if ( !String.IsNullOrWhiteSpace ( workLocation ))
        //        cmd = "Select * from `funcommissiondata` where (`name` = '" + workManager + "' OR `name` = '" + workArranger + "' ) AND `location` = '" + workLocation + "';";
        //    DataTable dt = G1.get_db_data(cmd);

        //    if (dt.Rows.Count <= 0)
        //    {
        //        string who = "";
        //        string option = "";
        //        string data = "";
        //        DataRow dRow = null;

        //        DataTable funDt = G1.get_db_data("Select * from `funcommoptions` ORDER by `order`;");
        //        for (int i = 0; i < funDt.Rows.Count; i++)
        //        {
        //            who = funDt.Rows[i]["who"].ObjToString();
        //            option = funDt.Rows[i]["option"].ObjToString();
        //            data = funDt.Rows[i]["defaults"].ObjToString();

        //            dRow = dt.NewRow();
        //            dRow["name"] = workManager;
        //            dRow["ma"] = who;
        //            dRow["option"] = option;
        //            dRow["answer"] = data;
        //            dt.Rows.Add(dRow);
        //        }
        //    }

        //    dt.Columns.Add("num");
        //    dt.Columns.Add("mod");
        //    G1.NumberDataTable(dt);

        //    string what = "";

        //    for (int i = (dt.Rows.Count - 1); i >= 0; i--)
        //    {
        //        what = dt.Rows[i]["ma"].ObjToString();
        //        if (what.ToUpper() != "A" )
        //            dt.Rows.RemoveAt(i);
        //    }

        //    LoadFuneralDetails(dt, localDt );

        //    return dt;
        //}
        ///***********************************************************************************************/
        //private void LoadFuneralDetails(DataTable dt, DataTable localDt )
        //{
        //    string option = "";
        //    string answer = "";
        //    string ma = "";
        //    double count = 0D;
        //    double detail = 0D;
        //    bool processOption = false;

        //    if (G1.get_column_number(dt, "count") < 0)
        //        dt.Columns.Add("count", Type.GetType("System.Double"));
        //    if (G1.get_column_number(dt, "detail") < 0)
        //        dt.Columns.Add("detail", Type.GetType("System.Double"));
        //    if (G1.get_column_number(dt, "commission") < 0)
        //        dt.Columns.Add("commission", Type.GetType("System.Double"));

        //    for (int i = 0; i < dt.Rows.Count; i++)
        //    {
        //        try
        //        {
        //            option = dt.Rows[i]["option"].ObjToString();
        //            answer = dt.Rows[i]["answer"].ObjToString();
        //            ma = dt.Rows[i]["ma"].ObjToString().ToUpper();

        //            processOption = true;
        //            //if (workWho.ToUpper() == "MA")
        //            //    processOption = true;
        //            //else if (workWho.ToUpper() == "M" && ma == "M")
        //            //    processOption = true;
        //            //else if (workWho.ToUpper() == "A" && ma == "A")
        //            //    processOption = true;

        //            if (!processOption)
        //                continue;

        //            ParseOutOption(localDt, option, answer, ma, ref count, ref detail);

        //            dt.Rows[i]["count"] = count;
        //            dt.Rows[i]["detail"] = detail;
        //            if (detail > 0D)
        //            {
        //                if (option != "Funeral Average")
        //                    dt.Rows[i]["commission"] = detail;
        //            }
        //        }
        //        catch ( Exception ex)
        //        {
        //        }
        //    }

        //    count = 0D;
        //    for (int i = 0; i < localDt.Rows.Count; i++)
        //    {
        //        answer = localDt.Rows[i]["funeralType"].ObjToString();
        //        if (answer.IndexOf("M") >= 0)
        //        {
        //            answer = localDt.Rows[i]["gotPackage"].ObjToString();
        //            if (String.IsNullOrWhiteSpace(answer))
        //            {
        //                answer = localDt.Rows[i]["urn"].ObjToString();
        //                detail = answer.ObjToDouble();
        //                if (detail == 0D)
        //                    count++;
        //            }
        //        }
        //    }

        //    double dollarsPerFuneral = 0D;
        //    double minimumFunerals = 0D;
        //    double funeralAverage = 0D;
        //    double averageMinimum = 0D;
        //    DataRow[] dRows = dt.Select("option='Funeral Average'");
        //    if (dRows.Length > 0)
        //    {
        //        funeralAverage = dRows[0]["detail"].ObjToDouble();
        //        averageMinimum = dRows[0]["answer"].ObjToDouble();
        //    }

        //    if (funeralAverage > averageMinimum)
        //    {
        //        dRows = dt.Select("option='Minimum Funerals'");
        //        if (dRows.Length > 0)
        //        {
        //            minimumFunerals = dRows[0]["answer"].ObjToDouble();
        //            dRows[0]["count"] = count;
        //            dRows[0]["detail"] = count - minimumFunerals;
        //        }

        //        dRows = dt.Select("option='Dollars per Funeral'");
        //        if (dRows.Length > 0)
        //            dollarsPerFuneral = dRows[0]["answer"].ObjToDouble();


        //        dRows = dt.Select("option='Dollars per Funeral'");
        //        if (dRows.Length > 0)
        //        {
        //            dRows[0]["count"] = (count - minimumFunerals);
        //            if (count > minimumFunerals)
        //            {
        //                dRows[0]["detail"] = (count - minimumFunerals) * dollarsPerFuneral;
        //                dRows[0]["commission"] = (count - minimumFunerals) * dollarsPerFuneral;
        //            }
        //        }
        //    }
        //}
        ///***********************************************************************************************/
        //private void ParseOutOption ( DataTable dt, string option, string answer, string ma, ref double count, ref double detail)
        //{
        //    count = 0D;
        //    detail = 0D;
        //    string who = "";
        //    string str = "";
        //    double dValue = 0D;
        //    double gauge = 0D;
        //    double totalDiscount = 0D;
        //    string[] Lines = null;
        //    if (option == "Funeral Average")
        //    {
        //        double netFuneral = 0D;
        //        double totalNet = 0D;
        //        for (int i = 0; i < dt.Rows.Count; i++)
        //        {
        //            totalDiscount = dt.Rows[i]["totalDiscount"].ObjToDouble();
        //            if (totalDiscount > 0D)
        //                continue;
        //            who = dt.Rows[i]["funeralType"].ObjToString();
        //            who = "A";
        //            if (who == ma || who == "MA" && ma == "M")
        //            {
        //                str = dt.Rows[i]["funeralType"].ObjToString();
        //                if (str.IndexOf("M") >= 0)
        //                {
        //                    str = dt.Rows[i]["gotPackage"].ObjToString();
        //                    if (String.IsNullOrWhiteSpace(str))
        //                    {
        //                        str = dt.Rows[i]["urn"].ObjToString();
        //                        dValue = str.ObjToDouble();
        //                        if (dValue == 0D)
        //                        {
        //                            count++;
        //                            netFuneral = dt.Rows[i]["netFuneral"].ObjToDouble();
        //                            totalNet += netFuneral;
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        if (count > 0D)
        //        {
        //            detail = totalNet / count;
        //            detail = G1.RoundValue(detail);
        //        }
        //    }
        //    else if (option == "Vault")
        //    {
        //        for (int i = 0; i < dt.Rows.Count; i++)
        //        {
        //            totalDiscount = dt.Rows[i]["totalDiscount"].ObjToDouble();
        //            if (totalDiscount > 0D)
        //                continue;
        //            who = dt.Rows[i]["funeralType"].ObjToString();
        //            who = "A";
        //            if (who == ma || who == "MA" && ma == "M")
        //            {
        //                str = dt.Rows[i]["vault"].ObjToString();
        //                if (!String.IsNullOrWhiteSpace(str))
        //                    count++;
        //            }
        //            detail = count * answer.ObjToDouble();
        //        }
        //    }
        //    else if (option == "Urn")
        //    {
        //        double totalUrn = 0D;
        //        answer = answer.Replace("%", "");
        //        double percent = 0D;
        //        if (G1.validate_numeric(answer))
        //            percent = answer.ObjToDouble() / 100D;
        //        if (percent > 0D)
        //        {
        //            count = 0;
        //            for (int i = 0; i < dt.Rows.Count; i++)
        //            {
        //                totalDiscount = dt.Rows[i]["totalDiscount"].ObjToDouble();
        //                if (totalDiscount > 0D)
        //                    continue;
        //                who = dt.Rows[i]["funeralType"].ObjToString();
        //                who = "A";
        //                if (who == ma || who == "MA" && ma == "M")
        //                {
        //                    str = dt.Rows[i]["urn"].ObjToString();
        //                    if (!String.IsNullOrWhiteSpace(str))
        //                    {
        //                        dValue = str.ObjToDouble();
        //                        if (dValue > 0D)
        //                        {
        //                            dValue = dValue * percent;
        //                            totalUrn += dValue;
        //                            count++;
        //                        }
        //                    }
        //                }
        //                if (count > 0D)
        //                    detail = totalUrn;
        //            }
        //        }
        //    }
        //    else if (option.ToUpper().IndexOf("CASKET GAUGE") == 0)
        //    {
        //        option = ParseOutEqual(option);
        //        option = option.ToUpper().Replace("GAUGE", "").Trim();
        //        gauge = option.ObjToDouble();
        //        count = 0;
        //        if (gauge > 0D)
        //        {
        //            for (int i = 0; i < dt.Rows.Count; i++)
        //            {
        //                totalDiscount = dt.Rows[i]["totalDiscount"].ObjToDouble();
        //                if (totalDiscount > 0D)
        //                    continue;
        //                who = dt.Rows[i]["funeralType"].ObjToString();
        //                who = "A";
        //                if (who == ma || who == "MA" && ma == "A")
        //                {
        //                    str = dt.Rows[i]["casketgauge"].ObjToString();
        //                    if (!String.IsNullOrWhiteSpace(str))
        //                    {
        //                        dValue = str.ObjToDouble();
        //                        if (dValue == gauge)
        //                            count++;
        //                    }
        //                }
        //            }
        //        }
        //        detail = count * answer.ObjToDouble();
        //    }
        //    else if (option.ToUpper().IndexOf("CASKET TYPE") == 0)
        //    {
        //        count = 0;
        //        option = ParseOutEqual(option);
        //        if (!String.IsNullOrWhiteSpace(option))
        //        {
        //            if (option.IndexOf("+") < 0)
        //            {
        //                for (int i = 0; i < dt.Rows.Count; i++)
        //                {
        //                    totalDiscount = dt.Rows[i]["totalDiscount"].ObjToDouble();
        //                    if (totalDiscount > 0D)
        //                        continue;
        //                    who = dt.Rows[i]["funeralType"].ObjToString();
        //                    who = "A";
        //                    if (who == ma || who == "MA" && ma == "A")
        //                    {
        //                        str = dt.Rows[i]["caskettype"].ObjToString();
        //                        if (str.ToUpper().Trim() == option.ToUpper().Trim())
        //                            count++;
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                Lines = option.Split('+');
        //                if (Lines.Length > 1)
        //                {
        //                    option = Lines[0].ToUpper().Trim();
        //                    Lines = Lines[1].Trim().Split(',');
        //                    for (int i = 0; i < dt.Rows.Count; i++)
        //                    {
        //                        totalDiscount = dt.Rows[i]["totalDiscount"].ObjToDouble();
        //                        if (totalDiscount > 0D)
        //                            continue;
        //                        who = dt.Rows[i]["funeralType"].ObjToString();
        //                        who = "A";
        //                        if (who == ma || who == "MA" && ma == "A")
        //                        {
        //                            str = dt.Rows[i]["caskettype"].ObjToString();
        //                            if (str.ToUpper().Trim() == option.ToUpper().Trim())
        //                            {
        //                                str = dt.Rows[i]["casketdesc"].ObjToString().Trim().ToUpper();
        //                                for (int j = 0; j < Lines.Length; j++)
        //                                {
        //                                    if (!String.IsNullOrWhiteSpace(Lines[j]))
        //                                    {
        //                                        if (str.IndexOf(Lines[j].Trim().ToUpper()) >= 0)
        //                                            count++;
        //                                    }
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        detail = count * answer.ObjToDouble();
        //    }
        //}
        ///***********************************************************************************************/
        //private string ParseOutEqual(string option)
        //{
        //    string[] Lines = option.Split('=');
        //    if (Lines.Length <= 1)
        //        return option;
        //    option = Lines[1].Trim();
        //    return option;
        //}
        /***********************************************************************************************/
        private string CleanServiceId ( string serviceId )
        {
            if (!String.IsNullOrWhiteSpace(serviceId ))
            {
                string c = serviceId.Substring(0, 1);
                if (c == "Y")
                    serviceId = serviceId.Substring(1);
            }
            return serviceId;
        }
        /***********************************************************************************************/
        private DataTable LoadData2( DateTime date1, DateTime date2 ) // Ramma Zamma
        {
            this.Cursor = Cursors.WaitCursor;

            DataTable dt = null;

            string contractsFile = "fcontracts";
            string customersFile = "fcustomers";
            insurance = false;

            DateTime date = date2;
            DateTime firstDate = new DateTime(date.Year, date.Month, 1);

            try
            {
                string cmd = "";
                //            string cmd = "SELECT * FROM `fcust_extended` e LEFT JOIN `fcontracts` p ON p.`contractNumber` = e.`contractNumber` left join `fcustomers` d ON e.`contractNumber` = d.`contractNumber` LEFT JOIN `icontracts` i ON i.`contractNumber` = e.`contractNumber` LEFT JOIN `icustomers` j ON j.`contractNumber` = e.`contractNumber` WHERE e.`ServiceID` <> '' ";
                cmd = "SELECT * FROM `fcust_extended` e LEFT JOIN `fcontracts` p ON p.`contractNumber` = e.`contractNumber` left join `fcustomers` d ON d.`contractNumber` = e.`contractNumber` LEFT JOIN `icontracts` i ON i.`contractNumber` = e.`contractNumber` LEFT JOIN `icustomers` j ON j.`contractNumber` = e.`contractNumber` ";
                //cmd += " LEFT JOIN `cust_payments` c ON c.`contractNumber` = e.`contractNumber` LEFT JOIN `cust_payment_details` x ON c.`record` = x.`paymentRecord` ";
                //cmd += " WHERE e.`ServiceID` <> '' AND ( e.`serviceLoc` IN (" + serviceLocs + ") OR e.`Funeral Arranger` LIKE '" + workArrangerFirstName + "%' AND e.`Funeral Arranger` LIKE  '%" + workArrangerLastName + "%' )";
                cmd += " WHERE e.`ServiceID` <> '' ";

                if (!String.IsNullOrWhiteSpace(serviceLocs))
                    cmd += " AND ( e.`serviceLoc` IN (" + serviceLocs + ") OR e.`Funeral Arranger` LIKE '" + workArrangerFirstName + "%' AND e.`Funeral Arranger` LIKE  '%" + workArrangerLastName + "%' )";
                else
                    cmd += " AND e.`Funeral Arranger` LIKE '" + workArrangerFirstName + "%' AND e.`Funeral Arranger` LIKE  '%" + workArrangerLastName + "%' ";

                //OR e.`Funeral Arranger` LIKE 'Arthur%' AND e.`Funeral Arranger` LIKE  '%Newman%' )
                    string sdate1 = date1.ToString("yyyy-MM-dd");
                    if (firstDate != date1)
                        sdate1 = firstDate.ToString("yyyy-MM-dd");

                    string sdate2 = date2.ToString("yyyy-MM-dd");
                        cmd += " AND ( (p.`deceasedDate` >= '" + sdate1 + "' AND p.`deceasedDate` <= '" + sdate2 + "' ) OR ( e.`bonusDate` >= '" + sdate1 + "' AND e.`bonusDate` <= '" + sdate2 + "' ) )";

                cmd += " ORDER BY e.`serviceDate` DESC ";
                cmd += ";";

                dt = G1.get_db_data(cmd);

                DataColumn Col1 = dt.Columns.Add("runDate");
                Col1.SetOrdinal(0);// to put the column in position 0;
                string str = date2.ToString("yyyyMMdd");
                for (int i = 0; i < dt.Rows.Count; i++)
                    dt.Rows[i]["runDate"] = str;


                if (!String.IsNullOrWhiteSpace(serviceLocs))
                {
                    //DataRow[] dRows = dt.Select("serviceLoc IN (" + serviceLocs + ")");
                    DataRow[] dRows = dt.Select("SRVLOC IN (" + serviceLocs + ")");
                    if (dRows.Length > 0)
                        dt = dRows.CopyToDataTable();
                }
                if (!String.IsNullOrWhiteSpace(workLocation))
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                        dt.Rows[i]["SRVLOC"] = workLocation;

                }

                dt.Columns.Add("netFuneral", Type.GetType("System.Double"));
                dt.Columns.Add("otherBonuses", Type.GetType("System.Double"));
                dt.Columns.Add("funeralType");
                dt.Columns.Add("bad");




                //this.Text = "Funerals for Manager " + workManager + " Location " + workLocation;

                //string title = "Funeral Commission for ";
                //if (workDt == null)
                //{
                //    if (workWho.ToUpper() == "M")
                //        title += " " + workManager + " as Manager for Location " + workLocation;
                //    else if (workWho.ToUpper() == "A")
                //    {
                //        if (!String.IsNullOrWhiteSpace(workArranger))
                //            title += " " + workArranger + " as Arranger";
                //        else
                //            title += " " + workManager + " as Arranger";
                //    }
                //    this.Text = title;
                //}

                ProcessExcludes(dt);

                //CombineLocations(dt, "HH", "HH-TY");
                //CombineLocations(dt, "TY", "HH-TY");

                //CombineLocations(dt, "LR", "LR-RA");
                //CombineLocations(dt, "RA", "LR-RA");

                //CombineLocations(dt, "MA", "MA-TV");
                //CombineLocations(dt, "TV", "MA-TV");

                //CombineLocations(dt, "WC", "WC-WR");
                //CombineLocations(dt, "WR", "WC-WR");

                G1.NumberDataTable(dt);
            }
            catch (Exception ex)
            {
            }

            this.Cursor = Cursors.Default;

            return dt;
        }
        /***********************************************************************************************/
        private void btnRefresh_Click (object sender, EventArgs e)
        {
            DateTime date1 = this.dateTimePicker1.Value;
            DateTime date2 = this.dateTimePicker2.Value;

            DateTime firstDate = new DateTime(date1.Year, date1.Month, 1);

            int numMonths = 1;

            DataTable dt = null;
            DataTable dx = null;
            DataTable finalDt = null;
            DataRow dRow = null;
            string loc = "";
            string oldArranger = "";
            string oldLoc = "";

            int count = 0;
            bool first = true;

            string arranger = "";
            bool got = false;

            for (; ; )
            {
                int days = DateTime.DaysInMonth(firstDate.Year, firstDate.Month);

                DateTime monthEnd = new DateTime(firstDate.Year, firstDate.Month, days);

                dt = LoadData2(firstDate, monthEnd);

                try
                {
                    DataView tempview = dt.DefaultView;
                    tempview.Sort = "serviceLoc,Funeral Arranger";
                    dt = tempview.ToTable();
                }
                catch (Exception ex)
                {
                }

                loc = "";
                oldArranger = "";
                oldLoc = "";
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    oldArranger = dt.Rows[i]["Funeral Arranger"].ObjToString();
                    if (String.IsNullOrWhiteSpace(oldArranger))
                        dt.Rows[i]["Funeral Arranger"] = "BLANK";
                    oldLoc = dt.Rows[i]["serviceLoc"].ObjToString();
                    if (String.IsNullOrWhiteSpace(oldLoc))
                        dt.Rows[i]["serviceLoc"] = "BLANK";
                }

                dx = dt.Clone();
                dx.Columns.Add("count", Type.GetType("System.Int32"));

                dRow = null;

                oldArranger = "";
                oldLoc = "";

                count = 0;

                arranger = "";
                got = false;

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    loc = dt.Rows[i]["serviceLoc"].ObjToString();
                    arranger = dt.Rows[i]["Funeral Arranger"].ObjToString();

                    if (String.IsNullOrWhiteSpace(oldLoc))
                        oldLoc = loc;
                    if (String.IsNullOrWhiteSpace(oldArranger))
                        oldArranger = arranger;

                    if (oldLoc != loc)
                    {
                        dRow = dx.NewRow();
                        dRow["serviceLoc"] = oldLoc;
                        dRow["Funeral Arranger"] = oldArranger;
                        dRow["count"] = count;
                        dx.Rows.Add(dRow);

                        count = 1;
                        oldLoc = loc;
                        oldArranger = arranger;
                        got = false;
                        continue;
                    }
                    else if (oldArranger != arranger)
                    {
                        dRow = dx.NewRow();
                        dRow["serviceLoc"] = oldLoc;
                        dRow["Funeral Arranger"] = oldArranger;
                        dRow["count"] = count;
                        dx.Rows.Add(dRow);

                        count = 1;
                        oldArranger = arranger;
                        got = false;
                        continue;
                    }
                    else
                    {
                        count++;
                    }
                }

                dRow = dx.NewRow();
                dRow["serviceLoc"] = oldLoc;
                dRow["Funeral Arranger"] = oldArranger; // Add Last One
                dRow["count"] = count;
                dx.Rows.Add(dRow);

                if (first)
                {
                    first = false;
                    finalDt = dx.Copy();
                    gridMain.Columns["count"].Caption = monthEnd.ToString("MM/yy");
                    AddSummaryColumn("count", gridMain, "Sum", "{0:N0}");
                }
                else
                {
                    MergeDataTables(monthEnd, finalDt, dx);
                    AddSummaryColumn(monthEnd.ToString("MM/dd/yyyy"), gridMain, "Sum", "{0:N0}");
                    AddSummaryItem(monthEnd.ToString("MM/dd/yyyy"));
                }
                gridMain.OptionsView.ShowFooter = true;

                firstDate = monthEnd.AddDays(1);
                if (firstDate > date2)
                    break;
            }

            int firstColumn = G1.get_column_number(finalDt, "count");
            int lastColumn = finalDt.Columns.Count;

            G1.AddNewColumn(gridMain, "Totals", "YTD Totals", "N0", FormatType.Numeric, 60, true);
            gridMain.Columns["Totals"].OptionsColumn.FixedWidth = true;
            finalDt.Columns.Add("Totals", Type.GetType("System.Int32"));
            AddSummaryColumn("Totals", gridMain, "Sum", "{0:N0}");
            AddSummaryItem( "Totals");

            int total = 0;

            for ( int i=0; i<finalDt.Rows.Count; i++)
            {
                total = 0;
                for ( int col=firstColumn; col<lastColumn; col++)
                    total += finalDt.Rows[i][col].ObjToInt32();
                finalDt.Rows[i]["Totals"] = total;
            }

            G1.ClearAllPositions(gridMain);
            for (int i = 0; i < finalDt.Columns.Count; i++)
                G1.SetColumnPosition(gridMain, finalDt.Columns[i].ColumnName, i);

            G1.NumberDataTable(finalDt);
            dgv.DataSource = finalDt;

            originalDt = finalDt;
            ScaleCells();
        }
        /***********************************************************************************************/
        private DataTable MergeDataTables ( DateTime date, DataTable finalDt, DataTable dx )
        {
            string caption = date.ToString("MM/dd/yyyy");
            string name = date.ToString("MM/yy");
            G1.AddNewColumn(gridMain, caption, name, "N0", FormatType.Numeric, 60, true);
            gridMain.Columns[caption].OptionsColumn.FixedWidth = true;
            finalDt.Columns.Add(caption, Type.GetType("System.Int32"));

            DataRow dRow = null;
            DataRow[] dRows = null;

            string arranger = "";
            string loc = "";
            for ( int i=0; i<dx.Rows.Count; i++)
            {
                arranger = dx.Rows[i]["Funeral Arranger"].ObjToString();
                loc = dx.Rows[i]["serviceLoc"].ObjToString();

                try
                {
                    dRows = finalDt.Select("`Funeral Arranger`='" + arranger + "' AND serviceLoc='" + loc + "'");
                    if (dRows.Length > 0)
                    {
                        dRows[0][caption] = dx.Rows[i]["count"].ObjToInt32();
                    }
                    else
                    {
                        dRow = finalDt.NewRow();
                        dRow["Funeral Arranger"] = arranger;
                        dRow["serviceLoc"] = loc;
                        dRow[caption] = dx.Rows[i]["count"].ObjToInt32();
                        finalDt.Rows.Add(dRow);
                    }
                }
                catch ( Exception ex )
                {
                }
            }
            return finalDt;
        }
        /***********************************************************************************************/
        private void chkArranger_CheckedChanged(object sender, EventArgs e)
        {
            if (byPass)
                return;
            if (dgv.DataSource == null)
                return;
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt.Rows.Count <= 0)
                return;
            try
            {
                if (chkArranger.Checked)
                {
                    DataView tempview = dt.DefaultView;
                    tempview.Sort = "Funeral Arranger";
                    dt = tempview.ToTable();
                    G1.NumberDataTable(dt);
                    dgv.DataSource = dt;

                    //AddSummaryColumn("count", gridMain, "Custom", "{0:N0}");

                    gridMain.OptionsView.ShowFooter = true;
                    byPass = true;
                    chkGroupByLocation.Checked = false;
                    byPass = false;
                    gridMain.Columns["serviceLoc"].GroupIndex = -1;
                    gridMain.Columns["Funeral Arranger"].GroupIndex = 0;
                    gridMain.ExpandAllGroups();
                }
                else
                {
                    byPass = true;
                    chkGroupByLocation.Checked = false;
                    chkArranger.Checked = false;
                    byPass = false;
                    gridMain.Columns["serviceLoc"].GroupIndex = -1;
                    gridMain.Columns["Funeral Arranger"].GroupIndex = -1;
                    //gridMain.OptionsView.ShowFooter = false;
                    gridMain.CollapseAllGroups();
                    //AddSummaryColumn("count", gridMain, "Sum", "{0:N0}");
                }
                gridMain.RefreshEditor(true);
            }
            catch (Exception ex)
            {
            }
            dgv.Refresh();
        }
        /***********************************************************************************************/
        private void chkGroupByLocation_CheckedChanged(object sender, EventArgs e)
        {
            if (byPass)
                return;
            if (dgv.DataSource == null)
                return;
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt.Rows.Count <= 0)
                return;
            try
            {
                if (chkGroupByLocation.Checked)
                {
                    DataView tempview = dt.DefaultView;
                    tempview.Sort = "serviceLoc";
                    dt = tempview.ToTable();
                    G1.NumberDataTable(dt);
                    dgv.DataSource = dt;

                    //AddSummaryColumn("count", gridMain, "Custom", "{0:N0}" );

                    gridMain.OptionsView.ShowFooter = true;
                    byPass = true;
                    chkArranger.Checked = false;
                    byPass = false;
                    gridMain.Columns["Funeral Arranger"].GroupIndex = -1;
                    gridMain.Columns["serviceLoc"].GroupIndex = 0;
                    gridMain.ExpandAllGroups();
                }
                else
                {
                    byPass = true;
                    chkArranger.Checked = false;
                    chkGroupByLocation.Checked = false;
                    byPass = false;
                    gridMain.Columns["serviceLoc"].GroupIndex = -1;
                    gridMain.Columns["Funeral Arranger"].GroupIndex = -1;
                    //gridMain.OptionsView.ShowFooter = false;
                    gridMain.CollapseAllGroups();
                    //AddSummaryColumn("count", gridMain, "Sum", "{0:N0}");
                }
                //gridMain5.RefreshData();
                gridMain.RefreshEditor(true);
            }
            catch (Exception ex)
            {
            }
            dgv.Refresh();
        }
        /***********************************************************************************************/
        private int groupCount = 0;
        private void gridMain5_CustomSummaryCalculate(object sender, DevExpress.Data.CustomSummaryEventArgs e)
        {
            //string field = (e.Item as GridSummaryItem).FieldName.ObjToString();

            //DataTable dt = null;
            //int row = 0;
            //double dValue = 0D;
            //double dTotal = 0D;
            //try
            //{
            //    dt = (DataTable)dgv.DataSource;
            //    if (dt.Rows.Count <= 0)
            //        return;

            //    row = e.RowHandle;
            //    row = gridMain.GetDataSourceRowIndex(row);
            //    if (row < 0)
            //        return;
            //}
            //catch (Exception ex)
            //{
            //}

            //try
            //{
            //    if (e.SummaryProcess == CustomSummaryProcess.Start)
            //        groupCount = 0;
            //    else if (e.SummaryProcess == CustomSummaryProcess.Calculate)
            //        groupCount += dt.Rows[row]["count"].ObjToInt32();
            //    //else if ( e.IsGroupSummary )
            //    //{
            //    //    e.TotalValue = groupCount;
            //    //    groupCount = 0;
            //    //}
            //    else if (e.SummaryProcess == CustomSummaryProcess.Finalize)
            //    {
            //        if (e.IsGroupSummary)
            //        {
            //            e.TotalValue = groupCount;
            //            groupCount = 0;
            //            return;
            //        }
            //        dTotal = 0D;
            //        for (int i = 0; i < dt.Rows.Count; i++)
            //        {
            //            dValue = dt.Rows[i][field].ObjToDouble();
            //            dTotal += dValue;

            //        }

            //        e.TotalValue = dTotal;

            //        return;
            //    }
            //}
            //catch (Exception ex)
            //{
            //}
        }
        /***********************************************************************************************/
        private void gridMain_CustomColumnDisplayText_1(object sender, CustomColumnDisplayTextEventArgs e)
        {
            ColumnView view = sender as ColumnView;

            string columnName = e.Column.FieldName.ToUpper();

            if (e.Column.FieldName.ToUpper() == "SERVICEID" && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                string str = e.DisplayText.Trim();
                if (!String.IsNullOrWhiteSpace(str))
                    e.DisplayText = CleanServiceId(str);
            }
        }
        /***********************************************************************************************/
        private void chkSummarize_CheckedChanged(object sender, EventArgs e)
        {
            if (chkSummarize.Checked)
            {
                gridMain.CollapseAllGroups();
                gridMain.OptionsPrint.ExpandAllGroups = false;
            }
            else
            {
                gridMain.OptionsPrint.ExpandAllGroups = true;
                gridMain.ExpandAllGroups();
            }
        }
        /***********************************************************************************************/
    }
}
