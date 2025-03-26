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
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraPrinting;
using DevExpress.Utils;
using MyXtraGrid;
using DevExpress.XtraCharts;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class TrustTotals : DevExpress.XtraEditors.XtraForm
    {
        private bool foundLocalPreference = false;
        private DataTable originalDt = null;
        private string workReport = "MAIN";
        private bool previousDateRead = false;
        private bool loading = true;
        /****************************************************************************************/
        public TrustTotals()
        {
            InitializeComponent();
            SetupTotalsSummary();
        }
        /****************************************************************************************/
        private void SetupTotalsSummary()
        {
            AddSummaryColumn("dbr", null);
            AddSummaryColumn("debitAdjustment", null);
            AddSummaryColumn("creditAdjustment", null);
            AddSummaryColumn("totalInt", null);
            AddSummaryColumn("totalTrust15", null);
            AddSummaryColumn("totalTrust85P", null);
            AddSummaryColumn("totalTrust100P", null);
        }
        /****************************************************************************************/
        private void AddSummaryColumn(string columnName)
        {
            gridMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            //gridMain.Columns[columnName].SummaryItem.DisplayFormat = "{0:0,0.00}";
            gridMain.Columns[columnName].SummaryItem.DisplayFormat = "{0:N2}";
        }
        /****************************************************************************************/
        private void AddSummaryColumn(string columnName, DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gMain = null)
        {
            if (gMain == null)
                gMain = gridMain;
            //            gMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            gMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            //            gMain.Columns[columnName].SummaryItem.DisplayFormat = "${0:0,0.00}";
            gMain.Columns[columnName].SummaryItem.DisplayFormat = "{0:N2}";
        }
        /****************************************************************************************/
        private void TrustTotals_Load(object sender, EventArgs e)
        {
            if ( !G1.RobbyServer )
            {
                lblContract.Hide();
                txtContract.Hide();
            }

            previousDateRead = false;

            barImport.Hide();
            lblTotal.Hide();
            DateTime now = DateTime.Now;
            now = now.AddMonths(-1);
            now = new DateTime(now.Year, now.Month, 1);
            this.dateTimePicker1.Value = now;

            int days = DateTime.DaysInMonth(now.Year, now.Month);
            DateTime stop = new DateTime(now.Year, now.Month, days);
            this.dateTimePicker2.Value = stop;

            this.Cursor = Cursors.WaitCursor;

            string saveName = "TrustTotals " + workReport + " Primary";
            string skinName = "";

            foundLocalPreference = G1.RestoreGridLayout(this, this.dgv, gridMain, LoginForm.username, saveName, ref skinName);
            if (!String.IsNullOrWhiteSpace(skinName))
            {
                if (skinName != "DevExpress Style")
                    skinForm_SkinSelected("Skin : " + skinName);
            }

            loadLocations();

            ScaleCells();

            loading = false;

            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void btnRight_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            DateTime now = this.dateTimePicker1.Value;
            now = now.AddMonths(1);
            now = new DateTime(now.Year, now.Month, 1);
            this.dateTimePicker1.Value = now;
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            this.dateTimePicker2.Value = new DateTime(now.Year, now.Month, days);

            DateTime startDate = now;
            DateTime stopDate = this.dateTimePicker2.Value;

            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void btnLeft_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            DateTime now = this.dateTimePicker1.Value;
            now = now.AddMonths(-1);
            now = new DateTime(now.Year, now.Month, 1);
            this.dateTimePicker1.Value = now;
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            this.dateTimePicker2.Value = new DateTime(now.Year, now.Month, days);

            DateTime startDate = now;
            DateTime stopDate = this.dateTimePicker2.Value;

            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void gridMain_CustomDrawGroupRow(object sender, DevExpress.XtraGrid.Views.Base.RowObjectCustomDrawEventArgs e)
        {
            GridGroupRowInfo info = e.Info as GridGroupRowInfo;
            string location = info.GroupText;
            int idx = location.LastIndexOf(']');
            if (idx > 0)
            {
                location = location.Substring(idx + 1);
                location = location.Replace("#image", "ABC");
                //DataRow[] dRows = monthlyBankDt.Select("localDescription='" + location.Trim() + "'");
                //if (dRows.Length > 0)
                //{
                //    double beginningBalance = dRows[0]["beginningBalance"].ObjToDouble();
                //    string str = G1.ReformatMoney(beginningBalance);
                //    info.GroupText += " Beginning Balance $" + str;
                //}
            }
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
            printableComponentLink1.Landscape = false;

            Printer.setupPrinterMargins(10, 10, 80, 50);

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

            DataTable ddd = (DataTable)dgv.DataSource;

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
            printableComponentLink1.Landscape = false;

            Printer.setupPrinterMargins(10, 10, 80, 50);

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

            //            Printer.DrawQuad(1, 9, 2, 3, "User : " + LoginForm.username, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);

            font = new Font("Ariel", 10, FontStyle.Regular);
            string title = this.Text;
            string location = chkComboLocNames.Text.Trim();
            string trusts = chkTrusts.Text.Trim();

            if (!String.IsNullOrWhiteSpace(location))
                title += " " + location;
            if (!String.IsNullOrWhiteSpace(trusts))
                title += " (" + trusts + ")";

            Printer.DrawQuad(6, 7, 4, 3, title, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            Printer.DrawQuad(1, 9, 2, 3, "User : " + LoginForm.username, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);


            DateTime date = this.dateTimePicker1.Value;
            string workDate = date.Month.ToString("D2") + "/" + date.Year.ToString("D4");
            Printer.SetQuadSize(24, 12);
            font = new Font("Ariel", 9, FontStyle.Regular);
            Printer.DrawQuad(20, 8, 5, 4, "Month Closing - " + workDate, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            //Printer.DrawQuad(16, 8, 3, 4, lblPayment.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(19, 8, 3, 4, lblTrust85.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(22, 8, 3, 4, lblTrust100.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /****************************************************************************************/
        private int footerCount = 0;
        private void gridMain_BeforePrintRow(object sender, DevExpress.XtraGrid.Views.Printing.CancelPrintRowEventArgs e)
        {
            if (e.HasFooter)
            {
                footerCount++;
            }
        }
        /****************************************************************************************/
        private void gridMain_AfterPrintRow(object sender, DevExpress.XtraGrid.Views.Printing.PrintRowEventArgs e)
        {
            if (e.HasFooter)
            {
                footerCount++;
                if (footerCount >= 1)
                {
                    footerCount = 0;
                    e.PS.InsertPageBreak(e.Y);
                }
            }
        }
        /****************************************************************************************/
        private void gridMain_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            int row = e.ListSourceRow;
            if (row >= 0)
            {
                //if (gridMain.IsDataRow(row))
                //{
                //    e.Visible = false;
                //    e.Handled = true;
                //    return;
                //}
            }
        }
        /****************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        { // Spy Glass
            G1.SpyGlass(gridMain);
            //if (this.gridMain.OptionsFind.AlwaysVisible == true)
            //    gridMain.OptionsFind.AlwaysVisible = false;
            //else
            //    gridMain.OptionsFind.AlwaysVisible = true;
        }
        /****************************************************************************************/
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (e == null)
                return;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            DataRow dr = gridMain.GetFocusedDataRow();
            if (dr == null)
                return;

            DataTable dt = (DataTable)dgv.DataSource;
        }
        /****************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataTable ddd = (DataTable)dgv.DataSource;

            DataRow dr = gridMain.GetFocusedDataRow();
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);

            string contract = dr["contractNumber"].ObjToString();
            if (!String.IsNullOrWhiteSpace(contract))
            {
                this.Cursor = Cursors.WaitCursor;
                CustomerDetails clientForm = new CustomerDetails(contract);
                clientForm.ShowDialog();
                this.Cursor = Cursors.Default;
            }
        }
        /****************************************************************************************/
        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            //DateTime date = this.dateTimePicker1.Value;
            //int days = DateTime.DaysInMonth(date.Year, date.Month);
            //date = new DateTime(date.Year, date.Month, 1);
            //this.dateTimePicker1.Value = date;
        }
        /****************************************************************************************/
        private void gridMain_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
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
        /****************************************************************************************/
        private void gridMain_CustomSummaryCalculate(object sender, DevExpress.Data.CustomSummaryEventArgs e)
        {
            //double balance = 0D;
            //DataTable dt = (DataTable)dgv.DataSource;
            //if (dt.Rows.Count > 0)
            //{
            //    int lastRow = dt.Rows.Count - 1;
            //    balance = dt.Rows[lastRow]["balance"].ObjToDouble();
            //}
            //string str = G1.ReformatMoney(balance);
            //str = str.Replace("$", "");
            //e.TotalValue = str;
        }
        /***********************************************************************************************/
        private void loadTrusts( DataTable dt )
        {
            DataTable groupDt = GetGroupData(dt, "trust");
            chkTrusts.Properties.DataSource = groupDt;
        }
        /***********************************************************************************************/
        private void loadLocations()
        {
            string cmd = "Select * from `funeralhomes` order by `LocationCode`;";
            DataTable locDt = G1.get_db_data(cmd);
            chkComboLocNames.Properties.DataSource = locDt;
        }
        /****************************************************************************************/
        private void dateTimePicker1_CloseUp(object sender, EventArgs e)
        {
            DateTime date = this.dateTimePicker1.Value;
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            date = new DateTime(date.Year, date.Month, days);
//            this.dateTimePicker1.Value = date;
        }
        /****************************************************************************************/
        private void gridMain_RowCellStyle(object sender, RowCellStyleEventArgs e)
        {

            GridView View = sender as GridView;
            if (e.RowHandle >= 0)
            {
                string column = e.Column.FieldName.ToUpper();
                if (column == "NUM")
                {
                    DataTable dt = (DataTable)dgv.DataSource;
                    int row = gridMain.GetDataSourceRowIndex(e.RowHandle);
                }
            }
        }
        /****************************************************************************************/
        private string oldWhat = "";
        private string oldColumn = "";
        /****************************************************************************************/
        private void gridMain_ValidatingEditor(object sender, DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventArgs e)
        {
            GridView view = sender as GridView;
            DataRow dr = gridMain.GetFocusedDataRow();
            if (view.FocusedColumn.FieldName.ToUpper() == "DATE")
            {
                DataTable dt = (DataTable)dgv.DataSource;
                int rowhandle = gridMain.FocusedRowHandle;
                int row = gridMain.GetDataSourceRowIndex(rowhandle);
                //string manual = dr["manual"].ObjToString().ToUpper();
                //if (manual != "Y")
                //{
                //    e.Valid = false;
                //    return;
                //}
                oldWhat = e.Value.ObjToString();
                DateTime date = oldWhat.ObjToDateTime();
                dt.Rows[row]["date"] = G1.DTtoMySQLDT(date);
                e.Value = G1.DTtoMySQLDT(date);
            }
        }
        /****************************************************************************************/
        private double originalSize = 0D;
        private Font mainFont = null;
        private Font newFont = null;
        private Font HeaderFont = null;
        /****************************************************************************************/
        private void ScaleCells()
        {
            if (originalSize == 0D)
            {
                //                originalSize = gridMain.Columns["address1"].AppearanceCell.FontSizeDelta.ObjToDouble();
                originalSize = gridMain.Columns["contractNumber"].AppearanceCell.Font.Size;
                mainFont = gridMain.Columns["contractNumber"].AppearanceCell.Font;
                HeaderFont = gridMain.Appearance.HeaderPanel.Font;
            }
            double scale = txtScale.Text.ObjToDouble();
            double size = scale / 100D * originalSize;
            Font font = new Font(mainFont.Name, (float)size);
            gridMain.Appearance.GroupFooter.Font = font;
            gridMain.AppearancePrint.FooterPanel.Font = font;
            gridMain.AppearancePrint.HeaderPanel.Font = font;


            gridMain.Appearance.GroupFooter.Font = font;
            gridMain.Appearance.FooterPanel.Font = font;
            gridMain.AppearancePrint.FooterPanel.Font = font;
            gridMain.AppearancePrint.GroupFooter.Font = font;

            font = new Font(HeaderFont.Name, (float)size, FontStyle.Regular);
            for (int i = 0; i < gridMain.Columns.Count; i++)
            {
                gridMain.Columns[i].AppearanceCell.Font = font;
                gridMain.Columns[i].AppearanceHeader.Font = font;
            }

            newFont = font;
            gridMain.RefreshData();
            gridMain.RefreshEditor(true);
            dgv.Refresh();
            this.Refresh();
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
        private void btnSelectColumns_Click(object sender, EventArgs e)
        {
            string actualName = cmbSelectColumns.Text;
            SelectColumns sform = new SelectColumns(dgv, "TrustTotals " + workReport, "Primary", actualName);
            sform.Done += new SelectColumns.d_void_eventdone(sform_Done);
            sform.Show();
        }
        /***********************************************************************************************/
        void sform_Done(DataTable dt)
        {
            string name = cmbSelectColumns.Text.Trim();
            if (String.IsNullOrWhiteSpace(name))
                name = workReport + " Primary";
            string saveName = "TrustTotals " + workReport + " " + name;
            string skinName = "";
            SetupSelectedColumns("TrustTotals " + workReport, name, dgv);
            foundLocalPreference = G1.RestoreGridLayout(this, this.dgv, gridMain, LoginForm.username, saveName, ref skinName);
            gridMain.OptionsView.ShowFooter = true;
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
        void sform_Done()
        {
            dgv.Refresh();
            this.Refresh();
        }
        /***********************************************************************************************/
        private void SetupSelectedColumns(string procType, string group, DevExpress.XtraGrid.GridControl dgv)
        {
            if (String.IsNullOrWhiteSpace(group))
                return;
            if (String.IsNullOrWhiteSpace(procType))
                procType = "TrustTotals " + workReport;
            string cmd = "Select * from procfiles where name = '" + group + "' and ProcType = '" + procType + "' order by seq";
            DataTable dt = G1.get_db_data(cmd);
            DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gridMain = (DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView)dgv.MainView;
            for (int i = 0; i < gridMain.Columns.Count; i++)
                gridMain.Columns[i].Visible = false;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string name = dt.Rows[i]["Description"].ToString();
                int index = dt.Rows[i]["seq"].ObjToInt32();
                try
                {
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
            string name = cmbSelectColumns.Text.Trim();
            if (String.IsNullOrWhiteSpace(name))
                name = "Primary";
            string saveName = "TrustTotals " + workReport + " " + name;
            G1.SaveLocalPreferences(this, gridMain, LoginForm.username, saveName);

            foundLocalPreference = true;
        }
        /****************************************************************************************/
        private void unLockScreenDetailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string comboName = cmbSelectColumns.Text;
            if (!String.IsNullOrWhiteSpace(comboName))
            {
                string name = comboName;
                if (String.IsNullOrWhiteSpace(name))
                    name = "Primary";
                string saveName = "TrustTotals " + workReport + " " + name;
                G1.RemoveLocalPreferences(LoginForm.username, saveName);
                foundLocalPreference = false;
            }

            //G1.RemoveLocalPreferences(LoginForm.username, "DailyHistoryLayout");
            foundLocalPreference = false;
        }
        /***********************************************************************************************/
        void skinForm_SkinSelected(string s)
        {
            if (s.ToUpper().IndexOf("SKIN : ") >= 0)
            {
                string skin = s.Replace("Skin : ", "");
                if (skin.Trim().Length == 0)
                    skin = "Windows Default";
                if (skin == "Windows Default")
                {
                    this.LookAndFeel.SetSkinStyle(skin);
                    this.gridMain.Appearance.EvenRow.BackColor = System.Drawing.Color.LightGreen;
                    this.gridMain.Appearance.EvenRow.BackColor2 = System.Drawing.Color.LightGreen;
                    this.gridMain.Appearance.SelectedRow.BackColor = System.Drawing.Color.Yellow;
                    this.gridMain.Appearance.SelectedRow.ForeColor = System.Drawing.Color.Black;
                }
                else
                {
                    this.panelTop.BackColor = Color.Transparent;
                    this.menuStrip1.BackColor = Color.Transparent;
                    this.gridMain.PaintStyleName = "Skin";
                    DevExpress.Skins.SkinManager.EnableFormSkins();
                    this.LookAndFeel.UseDefaultLookAndFeel = true;
                    DevExpress.LookAndFeel.UserLookAndFeel.Default.SetSkinStyle(skin);
                    this.LookAndFeel.SetSkinStyle(skin);
                    this.dgv.LookAndFeel.SetSkinStyle(skin);
                    this.dgv.LookAndFeel.SkinName = skin;
                    gridMain.Appearance.EvenRow.Options.UseBackColor = false;
                    gridMain.Appearance.OddRow.Options.UseBackColor = false;
                    this.panelTop.Refresh();
                    OnSkinChange(skin);

                    //DevExpress.LookAndFeel.UserLookAndFeel.Default.SkinName = skin;
                    //this.LookAndFeel.SetSkinStyle(skin);
                    //this.dgv.LookAndFeel.SetSkinStyle(skin);
                }
            }
            else if (s.ToUpper().IndexOf("COLOR : ") >= 0)
            {
                string color = s.Replace("Color : ", "");
                this.gridMain.Appearance.EvenRow.BackColor = Color.FromName(color);
                this.gridMain.Appearance.EvenRow.BackColor2 = Color.FromName(color);
                this.gridMain.Appearance.SelectedRow.BackColor = System.Drawing.Color.Yellow;
                this.gridMain.Appearance.SelectedRow.ForeColor = System.Drawing.Color.Black;
                this.gridMain.Appearance.EvenRow.Options.UseBackColor = true;
                this.gridMain.Appearance.OddRow.Options.UseBackColor = true;
            }
            else if (s.ToUpper().IndexOf("NO COLOR ON") >= 0)
            {
                this.gridMain.Appearance.EvenRow.Options.UseBackColor = false;
                this.gridMain.Appearance.OddRow.Options.UseBackColor = false;
            }
            else if (s.ToUpper().IndexOf("NO COLOR OFF") >= 0)
            {
                this.gridMain.Appearance.EvenRow.Options.UseBackColor = true;
                this.gridMain.Appearance.OddRow.Options.UseBackColor = true;
            }
        }
        /***************************************************************************************/
        public delegate void d_void_eventdone_string(string s);
        public event d_void_eventdone_string SkinChange;
        protected void OnSkinChange(string done)
        {
            if (SkinChange != null)
                SkinChange.Invoke(done);
        }
        /****************************************************************************************/
        private void cmbSelectColumns_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            string comboName = cmbSelectColumns.Text;
            string skinName = "";
            if (!String.IsNullOrWhiteSpace(comboName))
            {
                SetupSelectedColumns("TrustTotals " + workReport, comboName, dgv);
                string name = "TrustTotals " + workReport + " " + comboName;
                foundLocalPreference = G1.RestoreGridLayout(this, this.dgv, gridMain, LoginForm.username, name, ref skinName);
                SetupTotalsSummary();
                gridMain.OptionsView.ShowFooter = true;
            }
            else
            {
                SetupSelectedColumns("TrustTotals " + workReport, "Primary", dgv);
                string name = "TrustTotals " + workReport + " Primary";
                foundLocalPreference = G1.RestoreGridLayout(this, this.dgv, gridMain, LoginForm.username, name, ref skinName);
                gridMain.OptionsView.ShowFooter = true;
                SetupTotalsSummary();
            }
        }
        /****************************************************************************************/
        private void gridMain_CustomDrawFooterCell(object sender, FooterCellCustomDrawEventArgs e)
        {
            if (newFont != null)
                e.Appearance.Font = newFont;
        }
        /****************************************************************************************/
        private void chkComboLocNames_EditValueChanged(object sender, EventArgs e)
        {
            string names = getLocationNameQuery();
            DataRow[] dRows = originalDt.Select(names);
            DataTable dt = originalDt.Clone();
            for (int i = 0; i < dRows.Length; i++)
                dt.ImportRow(dRows[i]);
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
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
                    if (procLoc.Trim().Length > 0)
                        procLoc += ",";
                    procLoc += "'" + locIDs[i].Trim() + "'";
                }
            }
            return procLoc.Length > 0 ? " `loc` IN (" + procLoc + ") " : "";
        }
        /****************************************************************************************/
        private void btnRun_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                DateTime date = dateTimePicker1.Value;
                string date1 = G1.DateTimeToSQLDateTime(date);
                date = dateTimePicker2.Value;
                string date2 = G1.DateTimeToSQLDateTime(date);

                string ddDate = "`payDate8` >= '" + date1 + "' and `payDate8` <= '" + date2 + "' AND (`depositNumber` LIKE 'T%' OR `depositNumber` LIKE 'A%') ";

                string cmd = "SELECT *,j.`lastName`, j.`firstName`, c.`dueDate8`, c.`lapseDate8`,j.`deceasedDate` FROM contracts c JOIN customers j ON c.`contractNumber` = j.`contractNumber` LEFT JOIN `payments` p ON p.`contractNumber` = c.`contractNumber` WHERE p.`payDate8` >= '" + date1 + "' and p.`payDate8` <= '" + date2 + "' ";

                string testContract = txtContract.Text.Trim();
                if (!String.IsNullOrWhiteSpace(testContract))
                    cmd += " AND c.`contractNumber` = '" + testContract + "' ";
                cmd += "GROUP BY c.`contractNumber` ";
                cmd += ";";

                DataTable dt = G1.get_db_data(cmd);

                FindNewContracts(dt, date1, date2);

                string runOn = cmbRunOn.Text.Trim().ToUpper();

                if (runOn.ToUpper() != "RILES")
                    dt = SMFS.FilterForRiles(dt);

                bool excludeDBR = chkExcludeDBR.Checked;

                dt = Trust85.FilterForCemetery(dt, runOn);

                dt = CleanupFutureReporting(dt, date1, date2);

                DataTable newDt = BuildData(dt, date1, date2, excludeDBR );

                //FindNewContracts(newDt, date1, date2);

                //newDt = LoadDBR(newDt, date1, date2);

                G1.NumberDataTable(newDt);
                dgv.DataSource = newDt;

                originalDt = dt;
                this.Cursor = Cursors.Default;
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** " + ex.Message.ToString());
            }
        }
        /****************************************************************************************/
        private void FindNewContracts(DataTable dt, string date1, string date2)
        {
            int lastRow = 0;
            double downPayment = 0D;
            if (G1.get_column_number(dt, "specialDP") < 0)
                dt.Columns.Add("specialDP");

            string cmd = "Select * from `contracts` x ";
            cmd += " JOIN `customers` c ON x.`contractNumber` = c.`contractNumber` ";
            cmd += " JOIN `payments` p on x.`contractNumber` = p.`contractNumber` ";
            cmd += " where p.`payDate8` >= '" + date1 + "' ";
            cmd += " and   p.`payDate8` <= '" + date2 + "' ";
            //cmd += " where p.`issueDate8` >= '" + date1 + "' ";
            //cmd += " and   p.`issueDate8` <= '" + date2 + "' ";
            cmd += " and p.`downPayment` > '0.00' ";
            cmd += " ORDER by p.`payDate8` ";
            //cmd += " ORDER by p.`issueDate8` ";
            cmd += ";";
            DataTable dx = G1.get_db_data(cmd);
            string contract = "";
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                contract = dx.Rows[i]["contractNumber"].ObjToString();
                if (contract == "P24035L")
                {
                }
                DataRow[] dRows = dt.Select("contractNumber='" + contract + "'");
                if (dRows.Length <= 0)
                {
                    dt.ImportRow(dx.Rows[i]);
                    lastRow = dt.Rows.Count - 1;
                    dt.Rows[lastRow]["payDate8"] = dt.Rows[lastRow]["issueDate8"];
                    downPayment = dt.Rows[lastRow]["downPayment"].ObjToDouble();
                    //if (!chkShowAll.Checked)
                        dt.Rows[lastRow]["paymentAmount"] = downPayment;
                    dt.Rows[lastRow]["interestPaid1"] = 0D;

                    //dt.Rows[i]["totalTrust85P"] = downPayment * 0.85D;
                    //dt.Rows[i]["totalTrust100P"] = downPayment;
                    //dt.Rows[i]["totalTrust15"] = downPayment - dt.Rows[i]["totalTrust85P"].ObjToDouble();

                    dt.Rows[lastRow]["trust100P"] = downPayment;
                    dt.Rows[lastRow]["trust85P"] = downPayment * 0.85D;
                    dt.Rows[lastRow]["debitAdjustment"] = 0D;
                    dt.Rows[lastRow]["creditAdjustment"] = 0D;
                    dt.Rows[lastRow]["specialDP"] = "Y";
                }
            }
        }
        /****************************************************************************************/
        private DataTable CleanupFutureReporting(DataTable dt, string date1, string date2)
        {
            DateTime lDate1 = date1.ObjToDateTime();
            DateTime lDate2 = date2.ObjToDateTime();

            int nextMonth = lDate2.Month;
            int issueMonth = 0;

            DateTime payDate = DateTime.Now;
            DateTime issueDate = DateTime.Now;

            double downPayment = 0D;

            string contractNumber = "";

            for (int i = dt.Rows.Count - 1; i >= 0; i--)
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                if ( contractNumber == "P24035L")
                {
                }
                payDate = dt.Rows[i]["payDate8"].ObjToDateTime();
                //issueDate = dt.Rows[i]["issueDate8"].ObjToDateTime();
                issueDate = dt.Rows[i]["payDate8"].ObjToDateTime();
                issueMonth = issueDate.Month;
                if (issueDate.Year > lDate2.Year)
                    issueMonth += 12;

                if (issueMonth > nextMonth)
                {
                    if ( previousDateRead )
                        downPayment = dt.Rows[i]["newBusiness"].ObjToDouble();
                    else
                        downPayment = dt.Rows[i]["downPayment1"].ObjToDouble();
                    if ( downPayment > 0D )
                        dt.Rows.RemoveAt(i);
                }
            }
            return dt;
        }
        /***********************************************************************************************/
        private DataTable GetGroupData(DataTable dt, string byColumn )
        {
            if (dt.Rows.Count <= 0)
                return dt;

            DataTable groupDt = dt.Clone();

            try
            {
                if (G1.get_column_number(dt, "Int32_id") < 0)
                    dt.Columns.Add("Int32_id", typeof(int), "num");

                groupDt = dt.AsEnumerable().GroupBy(r => new { Col1 = r[byColumn] }).Select(g => g.OrderBy(r => r["Int32_id"]).First()).CopyToDataTable();
                groupDt.Columns.Remove("Int32_id");
            }
            catch (Exception ex)
            {
            }
            return groupDt;
        }
        /****************************************************************************************/
        public static DataTable AddColumn ( DataTable dt, string columnName )
        {
            if (G1.get_column_number(dt, columnName) < 0)
                dt.Columns.Add(columnName);
            return dt;
        }
        /****************************************************************************************/
        private DataTable BuildData ( DataTable dt, string date1, string date2, bool excludeDBR )
        {
            dt = AddColumn(dt, "num");
            dt = AddColumn(dt,"Year");
            dt = AddColumn(dt,"sDate");
            dt = AddColumn(dt,"loc");
            dt = AddColumn(dt,"trust");

            dt.Columns.Add("dbr", Type.GetType("System.Double"));
            dt.Columns.Add("totalInt", Type.GetType("System.Double"));
            dt.Columns.Add("totalTrust15", Type.GetType("System.Double"));
            dt.Columns.Add("totalTrust85P", Type.GetType("System.Double"));
            dt.Columns.Add("totalTrust100P", Type.GetType("System.Double"));

            string contractNumber = "";
            string contract = "";
            string trust = "";
            string loc = "";

            NewByDetail.RemoveCemeteries(dt);

            barImport.Show();
            barImport.Refresh();
            lblTotal.Show();
            lblTotal.Text = dt.Rows.Count.ToString();
            lblTotal.Refresh();

            barImport.Minimum = 0;
            barImport.Maximum = dt.Rows.Count;
            barImport.Value = 0;

            DataTable funDt = (DataTable)(chkComboLocNames.Properties.DataSource);
            DataRow[] dRows = null;

            for ( int i=0; i<dt.Rows.Count; i++)
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                contract = Trust85.decodeContractNumber(contractNumber, ref trust, ref loc);
                dt.Rows[i]["trust"] = trust;

                dRows = funDt.Select("keycode='" + loc + "'");
                if ( dRows.Length > 0 )
                    loc = dRows[0]["LocationCode"].ObjToString();

                dt.Rows[i]["loc"] = loc;

                dt.Rows[i]["Year"] = dt.Rows[i]["payDate8"].ObjToDateTime().Year.ToString();
                dt.Rows[i]["sDate"] = dt.Rows[i]["payDate8"].ObjToDateTime().ToString("yyyyMMdd");
            }

            loadTrusts(dt);

            DateTime date = DateTime.Now;

            date = dt.Rows[0]["payDate8"].ObjToDateTime();
            int startYear = date.Year;
            int row = dt.Rows.Count - 1;
            date = dt.Rows[row]["payDate8"].ObjToDateTime();
            int stopYear = date.Year;
            int years = stopYear - startYear + 1;

            int numPayments = 0;
            DateTime lastDate = DateTime.Now;
            string apr = "";
            double dAPR = 0D;
            double startBalance = 0D;
            DataTable dp = null;
            string cmd = "";

            double totalInterest = 0D;
            double totalTrust15 = 0D;
            double totalTrust85P = 0D;
            double totalTrust100P = 0D;

            DateTime mDate1 = date1.ObjToDateTime();
            DateTime mDate2 = date2.ObjToDateTime();
            DateTime payDate = DateTime.Now;
            DateTime deceasedDate = DateTime.Now;
            DateTime deceasedDate1 = DateTime.Now;
            DateTime deceasedDate2 = DateTime.Now;
            int days = 0;
            double trust85 = 0D;

            cmd = "Select * from `dbrs` WHERE `cashRemitStartDate` >= '" + date1 + "' AND `cashRemitStopDate` <= '" + date2 + "';";
            DataTable dbrDt = G1.get_db_data(cmd);
            DataTable dbrContractDt = dbrDt.Clone();

            string special = "";
            double dbr = 0D;
            double debit = 0D;
            double credit = 0D;
            bool gotit = false;
            bool gotDBR = false;

            for ( int i=0; i<dt.Rows.Count; i++)
            {
                Application.DoEvents();

                barImport.Value = (i + 1);
                barImport.Refresh();

                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                if ( contractNumber == "WC23034L")
                {
                }
                if (contractNumber == "P24031LI")
                {
                }
                if ( !previousDateRead )
                    special = dt.Rows[i]["specialDP"].ObjToString().ToUpper();

                dRows = dbrDt.Select("contractNumber='" + contractNumber + "'");
                if (dRows.Length > 0)
                    dbrContractDt = dRows.CopyToDataTable();
                else
                    dbrContractDt.Rows.Clear();

                numPayments = dt.Rows[i]["numberOfPayments"].ObjToString().ObjToInt32();
                lastDate = dt.Rows[i]["issueDate8"].ObjToDateTime();

                apr = dt.Rows[i]["APR"].ObjToString();

                dAPR = apr.ObjToDouble() / 100.0D;

                deceasedDate = dt.Rows[i]["deceasedDate"].ObjToDateTime();
                if (deceasedDate.Year > 1000)
                {
                    deceasedDate1 = new DateTime(deceasedDate.Year, deceasedDate.Month, 1);
                    days = DateTime.DaysInMonth(deceasedDate.Year, deceasedDate.Month);
                }

                dbr = 0D;
                debit = 0D;
                credit = 0D;
                gotit = false;

                startBalance = DailyHistory.GetFinanceValue( dt.Rows[i] );

                cmd = "Select * from `payments` where `contractNumber` = '" + contractNumber + "' AND `payDate8` >= '" + date1 + "' AND `payDate8` <= '" + date2 + "';";
                cmd = "Select * from `payments` where `contractNumber` = '" + contractNumber + "';";
                dp = G1.get_db_data(cmd);

                try
                {
                    if (dp.Rows.Count > 0)
                    {
                        try
                        {
                            DailyHistory.CalculateNewStuff(dp, dAPR, numPayments, startBalance, lastDate);
                        }
                        catch (Exception ex)
                        {
                        }


                        totalInterest = 0D;
                        totalTrust85P = 0D;
                        totalTrust100P = 0D;
                        for (int j = 0; j < dp.Rows.Count; j++)
                        {
                            date = dp.Rows[j]["payDate8"].ObjToDateTime();
                            if (date >= mDate1 && date <= mDate2)
                            {
                                //trust85 = dp.Rows[j]["trust85P"].ObjToDouble();
                                if (deceasedDate.Year > 1000)
                                {
                                    if ( excludeDBR )
                                    {
                                        gotDBR = TrustInterestReport.isDBR(contractNumber, mDate1, mDate2, date, ref dbrDt);
                                        if (gotDBR)
                                        {
                                            totalInterest += dp.Rows[j]["interestPaid"].ObjToDouble();
                                            continue;
                                        }
                                    }
                                    //if (dbrContractDt.Rows.Count > 0)
                                    //{
                                    //    gotit = false;
                                    //    for (int k = 0; k < dbrContractDt.Rows.Count; k++)
                                    //    {
                                    //        if (trust85 == dbrContractDt.Rows[k]["dbr"].ObjToDouble())
                                    //        {
                                    //            dbr += dp.Rows[j]["trust85P"].ObjToDouble();
                                    //            totalInterest += dp.Rows[j]["interestPaid"].ObjToDouble();
                                    //            gotit = true;
                                    //        }
                                    //    }
                                    //    if (gotit)
                                    //        continue;
                                    //}
                                }
                                totalInterest += dp.Rows[j]["interestPaid"].ObjToDouble();
                                totalTrust85P += dp.Rows[j]["trust85P"].ObjToDouble();
                                totalTrust100P += dp.Rows[j]["trust100P"].ObjToDouble();
                                debit += dp.Rows[j]["debitAdjustment"].ObjToDouble();
                                credit += dp.Rows[j]["creditAdjustment"].ObjToDouble();
                            }
                            else if (special == "Y")
                            {
                                if (date.AddDays(7) >= mDate1 && date.AddDays(7) <= mDate2)
                                {
                                    totalInterest += dp.Rows[j]["interestPaid"].ObjToDouble();
                                    totalTrust85P += dp.Rows[j]["trust85P"].ObjToDouble();
                                    totalTrust100P += dp.Rows[j]["trust100P"].ObjToDouble();
                                }
                            }
                        }
                        dt.Rows[i]["dbr"] = dbr;
                        dt.Rows[i]["debitAdjustment"] = debit;
                        dt.Rows[i]["creditAdjustment"] = credit;
                        dt.Rows[i]["totalInt"] = totalInterest;
                        dt.Rows[i]["totalTrust85P"] = totalTrust85P;
                        dt.Rows[i]["totalTrust100P"] = totalTrust100P;
                        dt.Rows[i]["totalTrust15"] = totalTrust100P - totalTrust85P;
                    }
                }
                catch ( Exception ex )
                {
                }
            }

            DataTable newDt = dt.Clone();
            DataTable dt1 = dt.Clone();
            for (int i = 0; i < years; i++)
            {
                dRows = dt.Select("Year='" + (i + startYear).ToString() + "'");
                if ( dRows.Length > 0 )
                {
                    dt1 = dRows.CopyToDataTable();
                    newDt.Merge(dt1);
                }
            }

            return newDt;
        }
        /****************************************************************************************/
        private DataTable BuildPreviousData(DataTable dt, string date1, string date2)
        {
            dt = AddColumn(dt, "num");
            dt = AddColumn(dt, "Year");
            dt = AddColumn(dt, "sDate");
            dt = AddColumn(dt, "loc");
            dt = AddColumn(dt, "trust");

            dt.Columns.Add("totalInt", Type.GetType("System.Double"));
            dt.Columns.Add("totalTrust15", Type.GetType("System.Double"));
            dt.Columns.Add("totalTrust85P", Type.GetType("System.Double"));
            dt.Columns.Add("totalTrust100P", Type.GetType("System.Double"));

            string contractNumber = "";
            string contract = "";
            string trust = "";
            string loc = "";

            barImport.Show();
            barImport.Refresh();
            lblTotal.Show();
            lblTotal.Text = dt.Rows.Count.ToString();
            lblTotal.Refresh();

            barImport.Minimum = 0;
            barImport.Maximum = dt.Rows.Count;
            barImport.Value = 0;

            DataTable funDt = (DataTable)(chkComboLocNames.Properties.DataSource);
            DataRow[] dRows = null;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                contract = Trust85.decodeContractNumber(contractNumber, ref trust, ref loc);
                dt.Rows[i]["trust"] = trust;

                dRows = funDt.Select("keycode='" + loc + "'");
                if (dRows.Length > 0)
                    loc = dRows[0]["LocationCode"].ObjToString();

                dt.Rows[i]["loc"] = loc;

                dt.Rows[i]["Year"] = dt.Rows[i]["payDate8"].ObjToDateTime().Year.ToString();
                dt.Rows[i]["sDate"] = dt.Rows[i]["payDate8"].ObjToDateTime().ToString("yyyyMMdd");
            }

            loadTrusts(dt);

            DateTime date = DateTime.Now;

            date = dt.Rows[0]["payDate8"].ObjToDateTime();
            int startYear = date.Year;
            int row = dt.Rows.Count - 1;
            date = dt.Rows[row]["payDate8"].ObjToDateTime();
            int stopYear = date.Year;
            int years = stopYear - startYear + 1;

            int numPayments = 0;
            DateTime lastDate = DateTime.Now;
            string apr = "";
            double dAPR = 0D;
            double startBalance = 0D;
            DataTable dp = null;
            string cmd = "";

            double totalInterest = 0D;
            double totalTrust15 = 0D;
            double totalTrust85P = 0D;
            double totalTrust100P = 0D;

            DateTime mDate1 = date1.ObjToDateTime();
            DateTime mDate2 = date2.ObjToDateTime();
            DateTime payDate = DateTime.Now;

            string special = "";

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                Application.DoEvents();

                barImport.Value = (i + 1);
                barImport.Refresh();

                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();

                try
                {
                    totalInterest = dt.Rows[i]["interestPaid1"].ObjToDouble();
                    totalTrust85P = dt.Rows[i]["trust85P"].ObjToDouble();
                    totalTrust100P = dt.Rows[i]["trust100P"].ObjToDouble();
                    dt.Rows[i]["totalInt"] = totalInterest;
                    dt.Rows[i]["totalTrust85P"] = totalTrust85P;
                    dt.Rows[i]["totalTrust100P"] = totalTrust100P;
                    dt.Rows[i]["totalTrust15"] = totalTrust100P - totalTrust85P;
                }
                catch (Exception ex)
                {
                }
            }

            DataTable newDt = dt.Clone();
            DataTable dt1 = dt.Clone();
            for (int i = 0; i < years; i++)
            {
                dRows = dt.Select("Year='" + (i + startYear).ToString() + "'");
                if (dRows.Length > 0)
                {
                    dt1 = dRows.CopyToDataTable();
                    newDt.Merge(dt1);
                }
            }

            return newDt;
        }
        /****************************************************************************************/
        private void chkGroupYear_CheckedChanged(object sender, EventArgs e)
        {
            if ( chkGroupYear.Checked )
            {
                gridMain.Columns["Year"].GroupIndex = 0;
                string trusts = chkTrusts.Text.Trim();
                string locations = chkComboLocNames.Text.Trim();
                if (!String.IsNullOrWhiteSpace(locations))
                {
                    gridMain.Columns["loc"].GroupIndex = 1;
                    if (!String.IsNullOrWhiteSpace(trusts))
                        gridMain.Columns["trust"].GroupIndex = 2;
                }
                else if (!String.IsNullOrWhiteSpace(trusts))
                    gridMain.Columns["trust"].GroupIndex = 1;
                gridMain.ExpandAllGroups();
            }
            else
            {
                gridMain.Columns["Year"].GroupIndex = -1;
                gridMain.Columns["loc"].GroupIndex = -1;
                gridMain.Columns["trust"].GroupIndex = -1;
            }

            gridMain.RefreshData();
            gridMain.RefreshEditor(true);
            dgv.Refresh();
        }
        /****************************************************************************************/
        private void chkTrusts_EditValueChanged(object sender, EventArgs e)
        {
            string names = getTrustQuery();
            DataRow[] dRows = originalDt.Select(names);
            DataTable dt = originalDt.Clone();
            for (int i = 0; i < dRows.Length; i++)
                dt.ImportRow(dRows[i]);
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
        }
        /*******************************************************************************************/
        private string getTrustQuery()
        {
            string procLoc = "";
            string[] locIDs = this.chkTrusts.EditValue.ToString().Split('|');
            for (int i = 0; i < locIDs.Length; i++)
            {
                if (!String.IsNullOrWhiteSpace(locIDs[i]))
                {
                    if (procLoc.Trim().Length > 0)
                        procLoc += ",";
                    procLoc += "'" + locIDs[i].Trim() + "'";
                }
            }
            return procLoc.Length > 0 ? " `trust` IN (" + procLoc + ") " : "";
        }
        /****************************************************************************************/
        private void chkCollapse_CheckedChanged(object sender, EventArgs e)
        {
            if (chkCollapse.Checked)
            {
                int yearIdx = gridMain.Columns["Year"].GroupIndex;
                int locIdx = gridMain.Columns["loc"].GroupIndex;
                int trustIdx = gridMain.Columns["trust"].GroupIndex;
                if (yearIdx >= 0 && locIdx < 0 && trustIdx < 0)
                    gridMain.CollapseAllGroups();
                else if (yearIdx >= 0 && locIdx > 0 && trustIdx < 0)
                    gridMain.CollapseGroupLevel(1);
                else if (yearIdx >= 0 && locIdx <= 0 && trustIdx > 0)
                    gridMain.CollapseGroupLevel(1);
                else if (yearIdx >= 0 && locIdx > 0 && trustIdx > 0)
                    gridMain.CollapseGroupLevel(2);
                else if ( locIdx >= 0 )
                    gridMain.CollapseAllGroups();
            }
            else
            {
                gridMain.ExpandAllGroups();
            }
        }
        /****************************************************************************************/
        private void menuPreviousData_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            DateTime begin = this.dateTimePicker1.Value;
            DateTime end = this.dateTimePicker2.Value;

            int days = DateTime.DaysInMonth(begin.Year, begin.Month);
            DateTime last = new DateTime(begin.Year, begin.Month, days);
            if (last > end)
            {
                this.Cursor = Cursors.Default;
                return;
            }

            string runDate1 = begin.ToString("yyyy-MM-dd");
            string runDate2 = end.ToString("yyyy-MM-dd");
            string runWhat = cmbRunOn.Text.Trim();

            string cmd = "Select * from `cashRemitted` where `runDate1` = '" + runDate1 + "' AND `runDate2` = '" + runDate2 + "' AND `runWhat` = '" + runWhat + "';";
            DataTable dt = G1.get_db_data(cmd);

            previousDateRead = true;

            dt = ConsolidateTrustData(dt);

            dt = CleanupFutureReporting(dt, runDate1, runDate2);

            DataTable newDt = BuildPreviousData(dt, runDate1, runDate2);

            newDt = LoadDBR(newDt, runDate1, runDate2);

            G1.NumberDataTable(newDt);
            dgv.DataSource = newDt;

            originalDt = newDt;

            menuStrip1.BackColor = Color.LightGreen;

            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private DataTable LoadDBR ( DataTable dt, string runDate1, string runDate2 )
        {
            DateTime date1 = runDate1.ObjToDateTime();
            DateTime date2 = runDate2.ObjToDateTime();
            DateTime deceasedDate = DateTime.Now;
            double trust85 = 0D;
            double dbr = 0D;
            string contractNumber = "";
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                if ( contractNumber == "B19022LI")
                {
                }
                deceasedDate = dt.Rows[i]["deceasedDate"].ObjToDateTime();
                if ( deceasedDate >= date1 && deceasedDate <= date2 )
                {
                    trust85 = dt.Rows[i]["totalTrust85P"].ObjToDouble();
                    dbr = trust85;
                    dt.Rows[i]["dbr"] = dbr;
                }
                else
                    dt.Rows[i]["dbr"] = 0D;
            }
            return dt;
        }
        /****************************************************************************************/
        public static DataTable ConsolidateTrustData(DataTable dt)
        {
            try
            {
                DataView tempview = dt.DefaultView;
                tempview.Sort = "contractNumber asc";
                dt = tempview.ToTable();

                int oldrow = 0;

                string contractNumber = "";
                string oldContractNumber = "";

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                    if (String.IsNullOrWhiteSpace(oldContractNumber))
                    {
                        oldContractNumber = contractNumber;
                        oldrow = i;
                        continue;
                    }
                    if ( contractNumber != oldContractNumber )
                    {
                        oldrow = i;
                        oldContractNumber = contractNumber;
                        continue;
                    }
                    if (oldrow == i)
                        continue;

                    oldContractNumber = contractNumber;

                    Commission.CombineData(dt, oldrow, i, "interestPaid1");
                    Commission.CombineData(dt, oldrow, i, "trust85P");
                    Commission.CombineData(dt, oldrow, i, "trust100P");

                    dt.Rows[i]["contractNumber"] = "";
                }
                for (int i = (dt.Rows.Count - 1); i >= 0; i--)
                {
                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                    if (String.IsNullOrWhiteSpace(contractNumber))
                        dt.Rows.RemoveAt(i);
                }
            }
            catch (Exception ex)
            {
            }

            DataView tempv = dt.DefaultView;
            tempv.Sort = "payDate8 asc";
            dt = tempv.ToTable();

            G1.NumberDataTable(dt);
            return dt;
        }
        /****************************************************************************************/
        private void chkGroupLocations_CheckedChanged(object sender, EventArgs e)
        {
            if (chkGroupLocation.Checked)
            {
                gridMain.Columns["loc"].GroupIndex = 0;
                string locations = chkComboLocNames.Text.Trim();
                if (!String.IsNullOrWhiteSpace(locations))
                {
                    gridMain.Columns["loc"].GroupIndex = 1;
                }
                gridMain.ExpandAllGroups();
            }
            else
            {
                gridMain.Columns["loc"].GroupIndex = -1;
            }

            gridMain.RefreshData();
            gridMain.RefreshEditor(true);
            dgv.Refresh();
        }
        /****************************************************************************************/
    }
}