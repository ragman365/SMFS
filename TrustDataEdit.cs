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
using DevExpress.XtraGrid.Columns;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class TrustDataEdit : DevExpress.XtraEditors.XtraForm
    {
        private bool foundLocalPreference = false;
        private DataTable originalDt = null;
        private string workReport = "MAIN";
        private bool loading = true;
        private string workCompany = "";
        private string workMonth = "";
        private DateTime workDate = DateTime.Now;
        /****************************************************************************************/
        public TrustDataEdit( string trustCompany, string month )
        {
            InitializeComponent();

            workCompany = trustCompany;
            workMonth = month;

            SetupTotalsSummary();
        }
        /****************************************************************************************/
        private void SetupTotalsSummary()
        {
            AddSummaryColumn("beginningPaymentBalance", null);
            AddSummaryColumn("beginningDeathBenefit", null);
            AddSummaryColumn("endingPaymentBalance", null);
            AddSummaryColumn("endingDeathBenefit", null);
            AddSummaryColumn("downPayments", null);
            AddSummaryColumn("payments", null);
            AddSummaryColumn("growth", null);
            AddSummaryColumn("priorUnappliedCash", null);
            AddSummaryColumn("currentUnappliedCash", null);
            AddSummaryColumn("deathClaimAmount", null);
            //AddSummaryColumn("endingBalance", null);
            //AddSummaryColumn("overshort", null);
            //AddSummaryColumn("tSurrender", null);
            //AddSummaryColumn("refund", null);
            //AddSummaryColumn("surfacdiff", null);
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
        private void TrustDataEdit_Load(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            string title = "Edit Trust Data for " + workCompany + " " + workMonth;
            this.Text = title;

            string[] Lines = workMonth.Split(' ');
            if (Lines.Length < 2)
                return;

            string str = Lines[0].Trim();
            int iMonth = G1.ConvertMonthToIndex(str);
            string year = Lines[1].Trim();
            int iYear = year.ObjToInt32();

            int days = DateTime.DaysInMonth(iYear, iMonth);

            DateTime date1 = new DateTime(iYear, iMonth, 1);
            DateTime date2 = new DateTime(iYear, iMonth, days);

            workDate = date2;

            string startDate = date1.ToString("yyyy-MM-dd");
            string stopDate = date2.ToString("yyyy-MM-dd");

            this.Cursor = Cursors.WaitCursor;

            gridMain.Columns["date"].Visible = true;

            string newCompany = workCompany;
            if (workCompany == "Unity Old Barham")
                newCompany = "Unity";
            if (workCompany == "Unity PB")
                newCompany = "Unity";


            string cmd = "Select * from `trust_data` WHERE `date` >= '" + startDate + "' AND `date` <= '" + stopDate + "' ";
            if ( workCompany == "SNFT")
                cmd += " AND `trustCompany` IN ('Security National', 'FORETHOUGHT') ";
            else
                cmd += " AND `trustCompany` LIKE '" + newCompany + "%' ";
            cmd += " ORDER by `date`, `trustCompany`;  ";

            DataTable dt = G1.get_db_data(cmd);

            cmd = "Select * from `trust_data_edits` WHERE `date` >= '" + startDate + "' AND `date` <= '" + stopDate + "' ";
            if (workCompany == "SNFT")
                cmd += " AND `trustCompany` IN ('Security National', 'FORETHOUGHT') ";
            else
                cmd += " AND `trustCompany` LIKE '" + workCompany + "%' ";
            cmd += " ORDER by `date`, `trustCompany`;  ";

            DataTable dx = G1.get_db_data(cmd);
            dx.Columns.Add("manual");
            for (int i = 0; i < dx.Rows.Count; i++)
                dx.Rows[i]["manual"] = "Y";

            dt.Merge(dx);

            string saveName = "TrustDataEdit Primary";
            string skinName = "";
            if (!String.IsNullOrWhiteSpace(workCompany))
                saveName = "TrustDataEdit " + workCompany;

            foundLocalPreference = G1.RestoreGridLayoutExact(this, this.dgv, gridMain, LoginForm.username, saveName, ref skinName);
            if ( !String.IsNullOrWhiteSpace ( workCompany ))
                loadGroupCombo(cmbSelectColumns, "TrustDataEdit", workCompany);
            else
                loadGroupCombo(cmbSelectColumns, "TrustDataEdit", "Primary");

            if ( !String.IsNullOrWhiteSpace ( workCompany ))
                cmbSelectColumns.Text = workCompany;
            else
                cmbSelectColumns.Text = "Primary";

            ScaleCells();

            G1.NumberDataTable(dt);

            dgv.DataSource = dt;

            btnSave.Hide();
            btnSave.Refresh();

            loading = false;

            gridMain.Columns["date"].Visible = true;

            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void loadGroupCombo(System.Windows.Forms.ComboBox cmb, string key, string module)
        {
            string primaryName = "";
            cmb.Items.Clear();
            string cmd = "Select * from procfiles where ProcType = '" + key + "' AND `module` = '" + module + "' group by name;";
            cmd = "Select * from procfiles where ProcType = '" + key + "' group by name;";
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
            printableComponentLink1.Landscape = true;

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
            string location = "";
            string trusts = "";

            if (!String.IsNullOrWhiteSpace(location))
                title += " " + location;
            if (!String.IsNullOrWhiteSpace(trusts))
                title += " (" + trusts + ")";

            string user = LoginForm.username;
            string format = cmbSelectColumns.Text.Trim();
            //if (!String.IsNullOrWhiteSpace(format))
            //    user += " Format " + format;
            Printer.DrawQuad(6, 7, 4, 3, title, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            Printer.DrawQuad(1, 9, 4, 3, "User : " + user, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            Printer.DrawQuad(3, 9, 4, 3, "Format : " + format, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);


            string workDate = workMonth;
            Printer.SetQuadSize(24, 12);
            font = new Font("Ariel", 9, FontStyle.Regular);
            Printer.DrawQuad(20, 7, 5, 5, "Month Closing - " + workDate, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

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
                DataTable dt = (DataTable)dgv.DataSource;
                if (dt == null)
                    return;
                string str = dt.Rows[row]["status"].ObjToString().ToUpper();
                if ( str == "DELETE")
                {
                    e.Visible = false;
                    e.Handled = true;
                    return;
                }
            }
        }
        /****************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        { // Spy Glass
            G1.SpyGlass(gridMain);
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

            SelectColumns sform = new SelectColumns(dgv, "TrustDataEdit", "Primary", actualName);
            sform.Done += new SelectColumns.d_void_eventdone(sform_Done);
            sform.Show();
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
                procType = "TrustDataEdit";
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
            System.Windows.Forms.ComboBox combo = (System.Windows.Forms.ComboBox)sender;
            string comboName = combo.Text;
            string skinName = "";
            if (!String.IsNullOrWhiteSpace(comboName))
            {
                SetupSelectedColumns("TrustDataEdit", comboName, dgv);
                string name = "TrustDataEdit " + comboName;
                foundLocalPreference = G1.RestoreGridLayout(this, this.dgv, gridMain, LoginForm.username, name, ref skinName);
                SetupTotalsSummary();
                gridMain.OptionsView.ShowFooter = true;
            }
            else
            {
                SetupSelectedColumns("TrustDataEdit", "Primary", dgv);
                string name = "TrustDataEdit Primary";
                foundLocalPreference = G1.RestoreGridLayout(this, this.dgv, gridMain, LoginForm.username, name, ref skinName);
                SetupTotalsSummary();
                gridMain.OptionsView.ShowFooter = true;
            }
        }
        /****************************************************************************************/
        private void btnAdd_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;

            DataRow dRow = dt.NewRow();
            dRow["date"] = G1.DTtoMySQLDT(workDate);
            dRow["trustCompany"] = workCompany;
            dRow["manual"] = "Y";
            dt.Rows.Add(dRow);

            G1.NumberDataTable(dt);
            dt.AcceptChanges();
            dgv.DataSource = dt;

            G1.GoToLastRow(gridMain);

            btnSave.Show();
            btnSave.Refresh();

            //dgv.Refresh();
        }
        /****************************************************************************************/
        private void gridMain_ValidatingEditor(object sender, DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventArgs e)
        {
            GridView view = sender as GridView;
            DataRow dr = gridMain.GetFocusedDataRow();
            string manual = dr["manual"].ObjToString().ToUpper();
            if (manual != "Y")
            {
                e.Valid = false;
                return;
            }
        }
        /****************************************************************************************/
        private void btnSave_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (G1.get_column_number(dt, "manual") < 0)
                return;

            string data = "";
            string type = "";
            string field = "";
            string record = "";
            string modList = "";

            DataTable dx = null;
            DataRow[] dRows = dt.Select("status='DELETE' ");
            if (dRows.Length <= 0)
                dx = dt.Clone();
            else
                dx = dRows.CopyToDataTable();
            for ( int i=0; i<dRows.Length; i++)
            {
                record = dRows[i]["record"].ObjToString();
                if (record == "-1")
                    continue;
                if ( !String.IsNullOrWhiteSpace ( record ))
                {
                    G1.delete_db_table("trust_data_edits", "record", record);
                    dRows[i]["record"] = -1;
                }
            }

            dRows = dt.Select("manual='Y' ");
            if (dRows.Length <= 0)
                return;
            dx = dRows.CopyToDataTable();

            for (int i = 0; i < dRows.Length; i++)
            {
                record = dRows[i]["record"].ObjToString();
                if (record == "-1")
                    continue;
                if (String.IsNullOrWhiteSpace(record))
                {
                    record = G1.create_record("trust_data_edits", "status", "-1");
                    dRows[i]["record"] = record.ObjToInt32();
                    G1.update_db_table("trust_data_edits", "record", record, new string[] {"status", "" });
                }
                if (G1.BadRecord("trust_data_edits", record))
                    return;

                modList = "";

                for (int j = 0; j < dx.Columns.Count; j++)
                {
                    field = dx.Columns[j].ColumnName;
                    if (field.ToUpper() == "NUM")
                        continue;
                    if (field.ToUpper() == "RECORD")
                        continue;
                    if (field.ToUpper() == "MANUAL")
                        continue;

                    data = dRows[i][j].ObjToString();
                    if (G1.get_column_number(dt, field) >= 0)
                    {
                        try
                        {
                            type = dt.Columns[field].DataType.ToString().ToUpper();
                            if (data.IndexOf(",") >= 0)
                            {
                                G1.update_db_table("trust_data_edits", "record", record, new string[] { field, data });
                                continue;
                            }
                            if (String.IsNullOrWhiteSpace(data))
                                data = "NODATA";
                            modList += field + "," + data + ",";
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }

                modList = modList.TrimEnd(',');
                G1.update_db_table("trust_data_edits", "record", record, modList);
            }

            btnSave.Hide();
            btnSave.Refresh();
        }
        /****************************************************************************************/
        private void btnDelete_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt.Rows.Count <= 0)
                return;

            if (G1.get_column_number(dt, "manual") < 0)
                return;

            DataRow dr = gridMain.GetFocusedDataRow();
            string manual = dr["manual"].ObjToString();
            if (String.IsNullOrWhiteSpace(manual))
                return;
            dr["status"] = "DELETE";

            G1.NumberDataTable(dt);
            dt.AcceptChanges();
            dgv.DataSource = dt;
            dgv.Refresh();

            btnSave.Show();
            btnSave.Refresh();
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

                    string manual = dt.Rows[row]["manual"].ObjToString();
                    if (manual.Trim().ToUpper() == "Y")
                        e.Appearance.BackColor = Color.Red;
                    else
                        e.Appearance.BackColor = Color.Transparent;
                }
            }
        }
        /****************************************************************************************/
        private void gridMain_CellValueChanged_1(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            btnSave.Show();
            btnSave.Refresh();
        }
        /****************************************************************************************/
        private void copyToNextMonthToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;

            int[] rows = gridMain.GetSelectedRows();
            if (rows.Length <= 0)
                return;

            int months = -1;
            using (Ask fmrmyform = new Ask("Enter Number of Months > "))
            {
                fmrmyform.Text = "";
                fmrmyform.ShowDialog();
                string p = fmrmyform.Answer.Trim();
                if (!String.IsNullOrWhiteSpace(p))
                {
                    if (!G1.validate_numeric(p))
                        return;
                    months = p.ObjToInt32();
                    if (months <= 0)
                        return;
                }
                else
                    return;
            }

            this.Cursor = Cursors.WaitCursor;
            int row = 0;
            int irow = 0;
            string record = "";
            DateTime date = DateTime.Now;
            DateTime newDate = DateTime.Now;
            int days = 0;
            string cmd = "";
            string modList = "";
            string field = "";

            for (int k = 0; k < months; k++)
            {
                date = workDate;
                newDate = new DateTime(date.Year, date.Month, 1);
                newDate = newDate.AddMonths(k + 1);
                days = DateTime.DaysInMonth(newDate.Year, newDate.Month);
                newDate = new DateTime(newDate.Year, newDate.Month, days);

                for (int i = 0; i < rows.Length; i++)
                {
                    modList = "";
                    row = rows[i];
                    irow = gridMain.GetDataSourceRowIndex(row);
                    record = dt.Rows[irow]["record"].ObjToString();
                    cmd = "INSERT INTO trust_data_edits (";
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        field = dt.Columns[j].ColumnName;
                        if (field.ToUpper() == "NUM")
                            continue;
                        if (field.ToUpper() == "RECORD")
                            continue;
                        if (field.ToUpper() == "MANUAL")
                            continue;
                        modList += field + ",";
                    }
                    modList = modList.TrimEnd(',');
                    cmd += modList + ") Select ";
                    modList = "";
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        field = dt.Columns[j].ColumnName;
                        if (field.ToUpper() == "NUM")
                            continue;
                        if (field.ToUpper() == "RECORD")
                            continue;
                        if (field.ToUpper() == "MANUAL")
                            continue;

                        if (field.ToUpper() == "DATE")
                            field = newDate.ToString("yyyyMMdd");

                        modList += field + ",";
                    }
                    modList = modList.TrimEnd(',');
                    cmd += modList;
                    cmd += " FROM `trust_data_edits` ";
                    cmd += " WHERE ";
                    cmd += " `record` = '" + record + "' ";
                    cmd += ";";

                    try
                    {
                        G1.get_db_data(cmd);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("***ERROR*** " + ex.Message.ToString(), "Copy Data Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    }
                }
            }
            dgv.DataSource = dt;
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void lockSceenFormatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string name = cmbSelectColumns.Text.Trim();
            if (String.IsNullOrWhiteSpace(name))
                name = "Primary";
            string saveName = "TrustDataEdit " + name;
            G1.SaveLocalPreferences(this, gridMain, LoginForm.username, saveName);
        }
        /****************************************************************************************/
        private void unlockScreenFormatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string comboName = cmbSelectColumns.Text;
            if (!String.IsNullOrWhiteSpace(comboName))
            {
                string name = "TrustDataEdit " + comboName;
                G1.RemoveLocalPreferences(LoginForm.username, name);
                //foundLocalPreference = false;
            }
        }
        /****************************************************************************************/
        private void copySelectedRowsToPreviousMonthToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;

            int[] rows = gridMain.GetSelectedRows();
            if (rows.Length <= 0)
                return;

            int months = -1;
            using (Ask fmrmyform = new Ask("Enter Number of Months > "))
            {
                fmrmyform.Text = "";
                fmrmyform.ShowDialog();
                string p = fmrmyform.Answer.Trim();
                if (!String.IsNullOrWhiteSpace(p))
                {
                    if (!G1.validate_numeric(p))
                        return;
                    months = p.ObjToInt32();
                    if (months <= 0)
                        return;
                }
                else
                    return;
            }

            this.Cursor = Cursors.WaitCursor;
            int row = 0;
            int irow = 0;
            string record = "";
            DateTime date = DateTime.Now;
            DateTime newDate = DateTime.Now;
            int days = 0;
            string cmd = "";
            string modList = "";
            string field = "";

            for (int k = 0; k < months; k++)
            {
                date = workDate;
                newDate = new DateTime(date.Year, date.Month, 1);
                newDate = newDate.AddMonths(k - 1);
                days = DateTime.DaysInMonth(newDate.Year, newDate.Month);
                newDate = new DateTime(newDate.Year, newDate.Month, days);

                for (int i = 0; i < rows.Length; i++)
                {
                    modList = "";
                    row = rows[i];
                    irow = gridMain.GetDataSourceRowIndex(row);
                    record = dt.Rows[irow]["record"].ObjToString();
                    cmd = "INSERT INTO trust_data_edits (";
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        field = dt.Columns[j].ColumnName;
                        if (field.ToUpper() == "NUM")
                            continue;
                        if (field.ToUpper() == "RECORD")
                            continue;
                        if (field.ToUpper() == "MANUAL")
                            continue;
                        modList += field + ",";
                    }
                    modList = modList.TrimEnd(',');
                    cmd += modList + ") Select ";
                    modList = "";
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        field = dt.Columns[j].ColumnName;
                        if (field.ToUpper() == "NUM")
                            continue;
                        if (field.ToUpper() == "RECORD")
                            continue;
                        if (field.ToUpper() == "MANUAL")
                            continue;

                        if (field.ToUpper() == "DATE")
                            field = newDate.ToString("yyyyMMdd");

                        modList += field + ",";
                    }
                    modList = modList.TrimEnd(',');
                    cmd += modList;
                    cmd += " FROM `trust_data_edits` ";
                    cmd += " WHERE ";
                    cmd += " `record` = '" + record + "' ";
                    cmd += ";";

                    try
                    {
                        G1.get_db_data(cmd);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("***ERROR*** " + ex.Message.ToString(), "Copy Data Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    }
                }
            }
            dgv.DataSource = dt;
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void repositoryItemComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowhandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowhandle);

            ComboBoxEdit combo = (ComboBoxEdit)sender;
            string what = combo.Text.Trim().ToUpper();
            dt.Rows[row]["position"] = what;
            dr["position"] = what;
            DateTime date = dr["date"].ObjToDateTime();
            if ( what == "TOP")
            {
                DateTime newDate = new DateTime(date.Year, date.Month, 1);
                dr["deathPaidDate"] = G1.DTtoMySQLDT(newDate.ToString("yyyy-MM-dd"));
            }
            else if (what == "BOTTOM")
                dr["deathPaidDate"] = G1.DTtoMySQLDT(date.ToString("yyyy-MM-dd"));
            gridMain.RefreshEditor(true);

            btnSave.Show();
            btnSave.Refresh();
        }
        /***********************************************************************************************/
        private void DataChanged()
        {
            if (loading)
                return;

            btnSave.Show();
            btnSave.Refresh();

            int rowHandle = gridMain.FocusedRowHandle;
            DataRow dr = gridMain.GetFocusedDataRow();
            if (dr == null)
                return;
//            dr["mod"] = "Y";
        }
        /****************************************************************************************/
        private void gridMain_MouseDown(object sender, MouseEventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource; // Leave as a GetDate example

            var hitInfo = gridMain.CalcHitInfo(e.Location);
            if (hitInfo.InRowCell)
            {
                int rowHandle = hitInfo.RowHandle;
                gridMain.FocusedRowHandle = rowHandle;
                gridMain.SelectRow(rowHandle);
                gridMain.RefreshEditor(true);
                GridColumn column = hitInfo.Column;
                gridMain.FocusedColumn = column;
                string currentColumn = column.FieldName.Trim();
                if (currentColumn.ToUpper() == "DEATHPAIDDATE")
                {
                    DataRow dr = gridMain.GetFocusedDataRow();
                    DateTime date = dr["deathPaidDate"].ObjToDateTime();
                    if (date.Year < 1000)
                    {
                        date = dr["date"].ObjToDateTime();
                        if ( date.Year < 1000 )
                            date = DateTime.Now;
                    }
                    using (GetDate dateForm = new GetDate(date, "Enter Death Paid Date"))
                    {
                        dateForm.TopMost = true;
                        dateForm.ShowDialog();
                        if (dateForm.DialogResult == System.Windows.Forms.DialogResult.OK)
                        {
                            date = dateForm.myDateAnswer;
                            dr["deathPaidDate"] = G1.DTtoMySQLDT(date);
                            DataChanged();
                            gridMain.ClearSelection();
                            gridMain.FocusedRowHandle = rowHandle;

                            gridMain.RefreshData();
                            gridMain.RefreshEditor(true);
                            gridMain.SelectRow(rowHandle);
                        }
                    }
                }
            }
        }
        /****************************************************************************************/
    }
}