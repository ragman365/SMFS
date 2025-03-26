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
using iTextSharp.text.pdf;
using System.IO;
//using iTextSharp.text;

/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class EditBankAccounts : DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        private bool modified = false;
        private bool Selecting = false;
        private bool loading = true;
        private bool foundLocalPreference = false;
        /***********************************************************************************************/
        public EditBankAccounts()
        {
            InitializeComponent();
        }
        /***********************************************************************************************/
        private void EditBankAccounts_Load(object sender, EventArgs e)
        {
            loading = true;

            loadGroupCombo(cmbSelectColumns, "EditBankAccounts", "Primary");

            LoadData();
            loading = false;
            btnSave.Hide();
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
        private void LoadData()
        {
            gridMain.Columns["deposits"].Visible = false;
            gridMain.Columns["insuranceDeposits"].Visible = false;
            gridMain.Columns["funeralDeposits"].Visible = false;
            gridMain.Columns["totalDeposits"].Visible = false;

            this.Cursor = Cursors.WaitCursor;

            string cmd = "Select * from `bank_accounts` ORDER by `order`;";
            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("num");
            dt.Columns.Add("mod");
            dt.Columns.Add("deposits", Type.GetType("System.Double"));
            dt.Columns.Add("insuranceDeposits", Type.GetType("System.Double"));
            dt.Columns.Add("funeralDeposits", Type.GetType("System.Double"));
            dt.Columns.Add("totalDeposits", Type.GetType("System.Double"));

            SetupTFBX(dt);
            SetupLkbxAch(dt);
            SetupCC(dt);
            SetupManual(dt);
            SetupACH(dt);
            SetupFuneral(dt);

            SetupSelection(dt, repositoryItemCheckEdit7, "insUnity");
            SetupSelection(dt, repositoryItemCheckEdit8, "checkRemote");
            SetupSelection(dt, repositoryItemCheckEdit9, "ccInsTrusts");
            SetupSelection(dt, repositoryItemCheckEdit10, "trustDeathClaims");
            SetupSelection(dt, repositoryItemCheckEdit11, "discresionaryACH");
            SetupSelection(dt, repositoryItemCheckEdit12, "checkACH");
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            this.Cursor = Cursors.Default;
            modified = false;
        }
        /****************************************************************************************/
        private void SetupTotalsSummary()
        {
            AddSummaryColumn("deposits", null);
            AddSummaryColumn("insuranceDeposits", null);
            AddSummaryColumn("funeralDeposits", null);
            AddSummaryColumn("totalDeposits", null);
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
        /***********************************************************************************************/
        private void SetupSelection(DataTable dt, DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew, string column)
        {
            bool saveLoad = loading;
            loading = true;
            selectnew.NullText = "";
            selectnew.ValueChecked = "1";
            selectnew.ValueUnchecked = "0";
            selectnew.ValueGrayed = "";
            string set = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                set = dt.Rows[i][column].ObjToString();
                if (set != "1")
                    dt.Rows[i][column] = "0";
            }
            loading = saveLoad;
        }
        /***********************************************************************************************/
        private void SetupTFBX(DataTable dt)
        {
            DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew = this.repositoryItemCheckEdit6;
            selectnew.NullText = "";
            selectnew.ValueChecked = "1";
            selectnew.ValueUnchecked = "0";
            selectnew.ValueGrayed = "";
            string set = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                set = dt.Rows[i]["tfbx"].ObjToString();
                if (set != "1")
                    dt.Rows[i]["tfbx"] = "0";
            }
        }
        /***********************************************************************************************/
        private void SetupLkbxAch(DataTable dt)
        {
            DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew = this.repositoryItemCheckEdit1;
            selectnew.NullText = "";
            selectnew.ValueChecked = "1";
            selectnew.ValueUnchecked = "0";
            selectnew.ValueGrayed = "";
            string set = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                set = dt.Rows[i]["lkbx_ach"].ObjToString();
                if ( set != "1")
                    dt.Rows[i]["lkbx_ach"] = "0";
            }
        }
        /***********************************************************************************************/
        private void SetupCC(DataTable dt)
        {
            DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew = this.repositoryItemCheckEdit2;
            selectnew.NullText = "";
            selectnew.ValueChecked = "1";
            selectnew.ValueUnchecked = "0";
            selectnew.ValueGrayed = "";
            string set = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                set = dt.Rows[i]["cc"].ObjToString();
                if (set != "1")
                    dt.Rows[i]["cc"] = "0";
            }
        }
        /***********************************************************************************************/
        private void SetupManual(DataTable dt)
        {
            DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew = this.repositoryItemCheckEdit3;
            selectnew.NullText = "";
            selectnew.ValueChecked = "1";
            selectnew.ValueUnchecked = "0";
            selectnew.ValueGrayed = "";
            string set = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                set = dt.Rows[i]["show_dropdown"].ObjToString();
                if (set != "1")
                    dt.Rows[i]["show_dropdown"] = "0";
            }
        }
        /***********************************************************************************************/
        private void SetupACH(DataTable dt)
        {
            DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew = this.repositoryItemCheckEdit4;
            selectnew.NullText = "";
            selectnew.ValueChecked = "1";
            selectnew.ValueUnchecked = "0";
            selectnew.ValueGrayed = "";
            string set = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                set = dt.Rows[i]["ach"].ObjToString();
                if (set != "1")
                    dt.Rows[i]["ach"] = "0";
            }
        }
        /***********************************************************************************************/
        private void SetupFuneral(DataTable dt)
        {
            DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew = this.repositoryItemCheckEdit5;
            selectnew.NullText = "";
            selectnew.ValueChecked = "1";
            selectnew.ValueUnchecked = "0";
            selectnew.ValueGrayed = "";
            string set = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                set = dt.Rows[i]["funeral"].ObjToString();
                if (set != "1")
                    dt.Rows[i]["funeral"] = "0";
            }
        }
        /***********************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (gridMain.OptionsFind.AlwaysVisible == true)
                gridMain.OptionsFind.AlwaysVisible = false;
            else
                gridMain.OptionsFind.AlwaysVisible = true;
        }
        /***********************************************************************************************/
        private void EditBankAccounts_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!modified)
                return;
            DialogResult result = MessageBox.Show("***Question***\nSetup has been modified!\nWould you like to save your changes?", "Bank Accounts Setup Dialog", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (result == DialogResult.Cancel)
            {
                e.Cancel = true;
                return;
            }
            modified = false;
            if (result == DialogResult.No)
                return;
            SaveSetup();
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
            if ( Selecting )
                Printer.DrawQuad(6, 8, 4, 4, this.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            else
                Printer.DrawQuad(6, 8, 4, 4, "Bank Account Report", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);


            //            Printer.DrawQuadTicks();
            DateTime date = DateTime.Now;
            string workDate = date.Month.ToString("D2") + "/" + date.Year.ToString("D4");
            Printer.SetQuadSize(24, 12);
            font = new Font("Ariel", 9, FontStyle.Regular);
//            Printer.DrawQuad(20, 8, 5, 4, "Report Month:" + workDate, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(16, 8, 3, 4, lblPayment.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(19, 8, 3, 4, lblTrust85.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(22, 8, 3, 4, lblTrust100.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /***********************************************************************************************/
        private void pictureAdd_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            int lines = 1;
            for (int i = 0; i < lines; i++)
            {
                DataRow dRow = dt.NewRow();
                dRow["num"] = dt.Rows.Count.ObjToInt32() + 1;
                dRow["lkbx_ach"] = "0";
                dRow["tfbx"] = "0";
                dRow["cc"] = "0";
                dRow["show_dropdown"] = "0";
                dRow["ach"] = "0";
                dRow["funeral"] = "0";
                dRow["insUnity"] = "0";
                dRow["checkRemote"] = "0";
                dRow["ccInsTrusts"] = "0";
                dRow["trustDeathClaims"] = "0";
                dRow["discresionaryACH"] = "0";
                dRow["checkACH"] = "0";
                dRow["beginningBalance"] = 0D;

                dt.Rows.Add(dRow);
            }
            dgv.DataSource = dt;

            int row = dt.Rows.Count - 1;
            gridMain.SelectRow(row);
            gridMain.FocusedRowHandle = row;
            dgv.RefreshDataSource();
            dgv.Refresh();
        }
        /***********************************************************************************************/
        private void pictureDelete_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            string service = dr["account_title"].ObjToString();
            DialogResult result = MessageBox.Show("***Question*** Are you sure you want to delete this bank account (" + service + ") ?", "Delete Bank Account Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;

            DataTable dt = (DataTable)dgv.DataSource;
            int[] rows = gridMain.GetSelectedRows();
            int dtRow = 0;
            int firstRow = 0;
            if (rows.Length > 0)
                firstRow = rows[0];
            int row = 0;
            try
            {
                loading = true;
                for (int i = 0; i < rows.Length; i++)
                {
                    row = rows[i];
                    dtRow = gridMain.GetDataSourceRowIndex(row);
                    if (dtRow < 0 || dtRow > (dt.Rows.Count - 1))
                    {
                        continue;
                    }
                    var dRow = gridMain.GetDataRow(row);
                    if ( dRow != null)
                        dRow["mod"] = "D";
                    dt.Rows[dtRow]["mod"] = "D";
                    modified = true;
                    btnSave.Show();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("*ERROR*** " + ex.Message.ToString());
            }

            //            gridMain.FocusedRowHandle = firstRow;
            loading = false;
            if (firstRow > (dt.Rows.Count - 1))
                firstRow = (dt.Rows.Count - 1);
            dgv.DataSource = dt;
            gridMain.RefreshData();
            dgv.Refresh();

            gridMain.FocusedRowHandle = firstRow;
            gridMain.SelectRow(firstRow);
        }
        /***********************************************************************************************/
        private void picRowUp_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt.Rows.Count <= 0)
                return;
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowHandle = gridMain.FocusedRowHandle;
            if (rowHandle == 0)
                return; // Already at the first row
            //MoveRowUp(dt, rowHandle);
            massRowsUp(dt, rowHandle);
            dt.AcceptChanges();
            dgv.DataSource = dt;
            gridMain.ClearSelection();
            gridMain.SelectRow(rowHandle - 1);
            gridMain.FocusedRowHandle = rowHandle - 1;
            gridMain.RefreshData();
            dgv.Refresh();
        }
        /***********************************************************************************************/
        private void massRowsUp(DataTable dt, int row)
        {
            int[] rows = gridMain.GetSelectedRows();
            int firstRow = 0;
            if (rows.Length > 0)
                firstRow = rows[0];
            try
            {
                G1.NumberDataTable(dt);
                dt.Columns.Add("Count", Type.GetType("System.Int32"));
                for (int i = 0; i < dt.Rows.Count; i++)
                    dt.Rows[i]["Count"] = i.ToString();
                int moverow = rows[0];
                for (int i = 0; i < rows.Length; i++)
                {
                    row = rows[i];
                    dt.Rows[row]["Count"] = (row - 1).ToString();
                    //dt.Rows[row - 1]["Count"] = row.ToString();
                    //var dRow = gridMain.GetDataRow(row);
                    dt.Rows[row]["mod"] = "M";
                    modified = true;
                    btnSave.Show();
                }
                dt.Rows[moverow - 1]["Count"] = (moverow + (rows.Length - 1)).ToString();
                G1.sortTable(dt, "Count", "asc");
                dt.Columns.Remove("Count");
                G1.NumberDataTable(dt);
            }
            catch (Exception ex)
            {
                MessageBox.Show("*ERROR*** " + ex.Message.ToString());
            }

            //            gridMain.FocusedRowHandle = firstRow;
            gridMain.SelectRow(firstRow);
            dgv.DataSource = dt;
        }
        /***********************************************************************************************/
        private void picRowDown_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt.Rows.Count <= 0)
                return;
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowHandle = gridMain.FocusedRowHandle;
            if (rowHandle == (dt.Rows.Count - 1))
                return; // Already at the last row
            MoveRowDown(dt, rowHandle);
            dt.AcceptChanges();
            dgv.DataSource = dt;
            gridMain.ClearSelection();
            gridMain.SelectRow(rowHandle + 1);
            gridMain.FocusedRowHandle = rowHandle + 1;
            gridMain.RefreshData();
            dgv.Refresh();
        }
        /***************************************************************************************/
        private void MoveRowDown(DataTable dt, int row)
        {
            dt.Columns.Add("Count", Type.GetType("System.Int32"));
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["Count"] = i.ToString();
            dt.Rows[row]["Count"] = (row + 1).ToString();
            dt.Rows[row + 1]["Count"] = row.ToString();
            G1.sortTable(dt, "Count", "asc");
            dt.Columns.Remove("Count");
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["Num"] = (i + 1).ToString();
        }
        /***********************************************************************************************/
        private void btnInsert_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt.Rows.Count <= 0)
                return;
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowHandle = gridMain.FocusedRowHandle;
            int dtRow = gridMain.GetDataSourceRowIndex(rowHandle);
            if (dtRow < 0 || dtRow > (dt.Rows.Count - 1))
                return;
            if (rowHandle == (dt.Rows.Count - 1))
                return; // Already at the last row

            DataRow dRow = dt.NewRow();
            dRow["lkbx_ach"] = "0";
            dRow["tfbx"] = "0";
            dRow["cc"] = "0";
            dRow["show_dropdown"] = "0";
            dRow["ach"] = "0";
            dRow["funeral"] = "0";
            dRow["insUnity"] = "0";
            dRow["checkRemote"] = "0";
            dRow["ccInsTrusts"] = "0";
            dRow["trustDeathClaims"] = "0";
            dRow["discresionaryACH"] = "0";
            dRow["checkACH"] = "0";
            dRow["beginningBalance"] = 0D;

            dt.Rows.InsertAt(dRow, dtRow);
            G1.NumberDataTable(dt);
            dt.AcceptChanges();
            dgv.DataSource = dt;
            gridMain.ClearSelection();
            gridMain.RefreshData();
            gridMain.FocusedRowHandle = rowHandle + 1;
            gridMain.SelectRow(rowHandle + 1);
            dgv.Refresh();
        }
        /***********************************************************************************************/
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        /***********************************************************************************************/
        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditHelp helpForm = new EditHelp( "customers" );
            helpForm.Show();
        }
        /***********************************************************************************************/
        private void SaveSetup()
        {
            string record = "";
            string generalLedger = "";
            string accountTitle = "";
            string accountNumber = "";
            string location = "";
            string localDescription = "";
            string lkbx_ach = "";
            string tfbx = "";
            string cc = "";
            string show_dropdown = "";
            string ach = "";
            string funeral = "";
            string insUnity = "";
            string checkRemote = "";
            string ccInsTrusts = "";
            string trustDeathClaims = "";
            string discresionaryACH = "";
            string checkACH = "";
            double beginningBalance = 0D;
            DateTime asOfDate = DateTime.Now;
            string mod = "";

            DataTable dt = (DataTable)dgv.DataSource;
            this.Cursor = Cursors.WaitCursor;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    record = dt.Rows[i]["record"].ObjToString();
                    if (record == "-1")
                        continue;
                    mod = dt.Rows[i]["mod"].ObjToString();
                    if (mod.ToUpper() == "D")
                    {
                        if (!String.IsNullOrWhiteSpace(record))
                            G1.delete_db_table("bank_accounts", "record", record);
                        dt.Rows[i]["record"] = -1;
                        continue;
                    }
                    if (String.IsNullOrWhiteSpace(record))
                        record = G1.create_record("bank_accounts", "location", "-1");
                    if (G1.BadRecord("bank_accounts", record))
                        continue;
                    location = dt.Rows[i]["location"].ObjToString();
                    generalLedger = dt.Rows[i]["general_ledger_no"].ObjToString();
                    accountTitle = dt.Rows[i]["account_title"].ObjToString();
                    accountNumber = dt.Rows[i]["account_no"].ObjToString();
                    lkbx_ach = dt.Rows[i]["lkbx_ach"].ObjToString();
                    tfbx = dt.Rows[i]["tfbx"].ObjToString();
                    cc = dt.Rows[i]["cc"].ObjToString();
                    show_dropdown = dt.Rows[i]["show_dropdown"].ObjToString();
                    ach = dt.Rows[i]["ach"].ObjToString();
                    funeral = dt.Rows[i]["funeral"].ObjToString();
                    localDescription = dt.Rows[i]["localDescription"].ObjToString();
                    insUnity = dt.Rows[i]["insUnity"].ObjToString();
                    checkRemote = dt.Rows[i]["checkRemote"].ObjToString();
                    ccInsTrusts = dt.Rows[i]["ccInsTrusts"].ObjToString();
                    trustDeathClaims = dt.Rows[i]["trustDeathClaims"].ObjToString();
                    discresionaryACH = dt.Rows[i]["discresionaryACH"].ObjToString();
                    checkACH = dt.Rows[i]["checkACH"].ObjToString();
                    beginningBalance = dt.Rows[i]["beginningBalance"].ObjToDouble();
                    asOfDate = dt.Rows[i]["asOfDate"].ObjToDateTime();
                    G1.update_db_table("bank_accounts", "record", record, new string[] { "location", location, "general_ledger_no", generalLedger, "account_title", accountTitle, "account_no", accountNumber, "lkbx_ach", lkbx_ach, "cc", cc, "show_dropdown", show_dropdown, "ach", ach, "funeral", funeral, "tfbx", tfbx, "localDescription", localDescription, "insUnity", insUnity, "checkRemote", checkRemote, "ccInsTrusts", ccInsTrusts, "trustDeathClaims", trustDeathClaims, "discresionaryACH", discresionaryACH, "checkACH", checkACH, "beginningBalance", beginningBalance.ToString(), "asOfDate", asOfDate.ToString("yyyy-MM-dd"), "order", i.ToString() });
                }
                catch ( Exception ex)
                {
                    MessageBox.Show("***ERROR*** Updating Bank Accounts " + ex.Message.ToString());
                }
            }
            modified = false;
            btnSave.Hide();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            modified = true;
            btnSave.Show();
        }
        /***********************************************************************************************/
        private void gridMain_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv.DataSource;
            if (G1.get_column_number(dt, "mod") < 0)
                return;
            string delete = dt.Rows[row]["mod"].ObjToString();
            if (delete.ToUpper() == "D")
            {
                e.Visible = false;
                e.Handled = true;
            }
        }
        /***********************************************************************************************/
        private void gridMain_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
        {
            if (e.Column.FieldName.ToUpper() == "DATA")
            {
                if (e.RowHandle >= 0)
                {
                    string data = e.DisplayText.ObjToString();
                    if ( G1.validate_numeric ( data ))
                    {
                        double dvalue = data.ObjToDouble();
                        e.DisplayText = G1.ReformatMoney(dvalue);
                    }
                }
            }
            else if (e.Column.FieldName.ToUpper().IndexOf("DATE") >= 0)
            {
                if (e.RowHandle >= 0)
                {
                    DateTime date = e.DisplayText.ObjToDateTime();
                    e.DisplayText = date.Month.ToString("D2") + "/" + date.Day.ToString("D2") + "/" + date.Year.ToString("D4");
                    if (date.Year == 1)
                        e.DisplayText = "";
                }
            }
        }
        /***********************************************************************************************/
        private void gridMain_RowCellStyle(object sender, DevExpress.XtraGrid.Views.Grid.RowCellStyleEventArgs e)
        {
            GridView View = sender as GridView;
            if (e.Column.FieldName.ToUpper() == "DATA")
            {
                string str = View.GetRowCellValue(e.RowHandle, "data").ObjToString();
                if (str != null)
                {
                    if (G1.validate_numeric(str))
                        e.Appearance.TextOptions.HAlignment = HorzAlignment.Far;
                }
            }
        }
        /***********************************************************************************************/
        private void repositoryItemCheckEdit1_CheckedChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            loading = true;
            DataTable dt = (DataTable)dgv.DataSource;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            DataRow dr = gridMain.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            if (!String.IsNullOrWhiteSpace(record))
            {
                string set = dr["lkbx_ach"].ObjToString();
                ClearSelection(dt, "lkbx_ach");
                dr["lkbx_ach"] = set;
                dt.Rows[row]["lkbx_ach"] = set;
            }
            loading = false;
            modified = true;
            btnSave.Show();
        }
        /***********************************************************************************************/
        private void ClearSelection ( DataTable dt, string column)
        {
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i][column] = "0";
        }
        /***********************************************************************************************/
        private void repositoryItemCheckEdit2_CheckedChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            loading = true;
            DataTable dt = (DataTable)dgv.DataSource;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            DataRow dr = gridMain.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            if (!String.IsNullOrWhiteSpace(record))
            {
                string set = dr["cc"].ObjToString();
                ClearSelection(dt, "cc");
                dr["cc"] = set;
                dt.Rows[row]["cc"] = set;
            }
            loading = false;
            modified = true;
            btnSave.Show();
        }
        /***********************************************************************************************/
        private void repositoryItemCheckEdit3_CheckedChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            modified = true;
            btnSave.Show();
        }
        /***********************************************************************************************/
        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveSetup();
        }
        /***********************************************************************************************/
        private void repositoryItemCheckEdit4_CheckedChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            loading = true;
            DataTable dt = (DataTable)dgv.DataSource;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            DataRow dr = gridMain.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            if (!String.IsNullOrWhiteSpace(record))
            {
                string set = dr["ach"].ObjToString();
                ClearSelection(dt, "ach");
                dr["ach"] = set;
                dt.Rows[row]["ach"] = set;
            }
            loading = false;
            modified = true;
            btnSave.Show();
        }
        /***********************************************************************************************/
        private void repositoryItemCheckEdit5_CheckedChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            loading = true;
            DataTable dt = (DataTable)dgv.DataSource;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            DataRow dr = gridMain.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            if (!String.IsNullOrWhiteSpace(record))
            {
                string set = dr["funeral"].ObjToString();
                //ClearSelection(dt, "funeral");
                dr["funeral"] = set;
                dt.Rows[row]["funeral"] = set;
            }
            loading = false;
            modified = true;
            btnSave.Show();
        }
        /***********************************************************************************************/
        private void repositoryItemCheckEdit6_CheckedChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            loading = true;
            DataTable dt = (DataTable)dgv.DataSource;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            DataRow dr = gridMain.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            if (!String.IsNullOrWhiteSpace(record))
            {
                string set = dr["tfbx"].ObjToString();
                ClearSelection(dt, "tfbx");
                dr["tfbx"] = set;
                dt.Rows[row]["tfbx"] = set;
            }
            loading = false;
            modified = true;
            btnSave.Show();
        }
        /***********************************************************************************************/
        private void repositoryItemCheckEdit7_CheckedChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            loading = true;
            DataTable dt = (DataTable)dgv.DataSource;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            DataRow dr = gridMain.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            if (!String.IsNullOrWhiteSpace(record))
            {
                string set = dr["insUnity"].ObjToString();
                dr["insUnity"] = set;
                dt.Rows[row]["insUnity"] = set;
            }
            loading = false;
            modified = true;
            btnSave.Show();
        }
        /***********************************************************************************************/
        private void repositoryItemCheckEdit8_CheckedChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            loading = true;
            DataTable dt = (DataTable)dgv.DataSource;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            DataRow dr = gridMain.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            if (!String.IsNullOrWhiteSpace(record))
            {
                string set = dr["checkRemote"].ObjToString();
                dr["checkRemote"] = set;
                dt.Rows[row]["checkRemote"] = set;
            }
            loading = false;
            modified = true;
            btnSave.Show();
        }
        /***********************************************************************************************/
        private void repositoryItemCheckEdit9_CheckedChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            loading = true;
            DataTable dt = (DataTable)dgv.DataSource;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            DataRow dr = gridMain.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            if (!String.IsNullOrWhiteSpace(record))
            {
                string set = dr["ccInsTrusts"].ObjToString();
                dr["ccInsTrusts"] = set;
                dt.Rows[row]["ccInsTrusts"] = set;
            }
            loading = false;
            modified = true;
            btnSave.Show();
        }
        /***********************************************************************************************/
        private void repositoryItemCheckEdit10_CheckedChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            loading = true;
            DataTable dt = (DataTable)dgv.DataSource;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            DataRow dr = gridMain.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            if (!String.IsNullOrWhiteSpace(record))
            {
                string set = dr["trustDeathClaims"].ObjToString();
                dr["trustDeathClaims"] = set;
                dt.Rows[row]["trustDeathClaims"] = set;
            }
            loading = false;
            modified = true;
            btnSave.Show();
        }
        /***********************************************************************************************/
        private void repositoryItemCheckEdit11_CheckedChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            loading = true;
            DataTable dt = (DataTable)dgv.DataSource;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            DataRow dr = gridMain.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            if (!String.IsNullOrWhiteSpace(record))
            {
                string set = dr["discresionaryACH"].ObjToString();
                dr["discresionaryACH"] = set;
                dt.Rows[row]["discresionaryACH"] = set;
            }
            loading = false;
            modified = true;
            btnSave.Show();
        }
        /***********************************************************************************************/
        private void repositoryItemCheckEdit12_CheckedChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            loading = true;
            DataTable dt = (DataTable)dgv.DataSource;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            DataRow dr = gridMain.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            if (!String.IsNullOrWhiteSpace(record))
            {
                string set = dr["checkACH"].ObjToString();
                dr["checkACH"] = set;
                dt.Rows[row]["checkACH"] = set;
            }
            loading = false;
            modified = true;
            btnSave.Show();
        }
        /****************************************************************************************/
        private string oldWhat = "";
        /***********************************************************************************************/
        private void gridMain_ValidatingEditor(object sender, DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventArgs e)
        {
            GridView view = sender as GridView;
            if (view.FocusedColumn.FieldName.ToUpper() == "ASOFDATE")
            {
                DataTable dt = (DataTable)dgv.DataSource;
                DataRow dr = gridMain.GetFocusedDataRow();
                int rowhandle = gridMain.FocusedRowHandle;
                int row = gridMain.GetDataSourceRowIndex(rowhandle);
                oldWhat = e.Value.ObjToString();
                DateTime date = oldWhat.ObjToDateTime();
                dt.Rows[row]["ASOFDATE"] = G1.DTtoMySQLDT(date);
                e.Value = G1.DTtoMySQLDT(date);
            }
        }
        /***********************************************************************************************/
        private void btnLeft_Click(object sender, EventArgs e)
        {
            DateTime now = this.dateTimePicker1.Value;
            now = now.AddMonths(-1);
            now = new DateTime(now.Year, now.Month, 1);
            this.dateTimePicker1.Value = now;
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            this.dateTimePicker2.Value = new DateTime(now.Year, now.Month, days);
        }
        /***********************************************************************************************/
        private void btnRight_Click(object sender, EventArgs e)
        {
            DateTime now = this.dateTimePicker2.Value;
            now = now.AddMonths(1);
            now = new DateTime(now.Year, now.Month, 1);
            this.dateTimePicker1.Value = now;
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            this.dateTimePicker2.Value = new DateTime(now.Year, now.Month, days);
        }
        /***********************************************************************************************/
        private void btnRun_Click(object sender, EventArgs e)
        {

            DataTable dt = (DataTable)dgv.DataSource;

            try
            {
                if (G1.get_column_number(dt, "deposits") < 0)
                    dt.Columns.Add("deposits", Type.GetType("System.Double"));
                if (G1.get_column_number(dt, "insuranceDeposits") < 0)
                    dt.Columns.Add("insuranceDeposits", Type.GetType("System.Double"));
                if (G1.get_column_number(dt, "funeralDeposits") < 0)
                    dt.Columns.Add("funeralDeposits", Type.GetType("System.Double"));
                if (G1.get_column_number(dt, "totalDeposits") < 0)
                    dt.Columns.Add("totalDeposits", Type.GetType("System.Double"));

                gridMain.Columns["deposits"].Visible = true;
                gridMain.Columns["insuranceDeposits"].Visible = true;
                gridMain.Columns["funeralDeposits"].Visible = true;
                gridMain.Columns["totalDeposits"].Visible = true;

                SetupTotalsSummary();

                DateTime date1 = this.dateTimePicker1.Value;
                DateTime date2 = this.dateTimePicker2.Value;

                string sDate = date1.ToString("yyyy-MM-dd");
                string eDate = date2.ToString("yyyy-MM-dd");

                barImport.Show();
                barImport.Maximum = dt.Rows.Count;
                barImport.Minimum = 0;
                barImport.Value = 0;
                barImport.Refresh();

                string cmd = "";
                DataTable dx = null;

                int length = 0;
                int start = 0;
                string search = "";

                string bankAccount = "";

                double totalDeposits = 0D;
                double payment = 0D;
                double downPayment = 0D;

                double totalInsurance = 0D;
                double totalFuneral = 0D;
                double totalTotal = 0D;
                string depositNumber = "";

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    Application.DoEvents();

                    barImport.Value = i + 1;
                    barImport.Refresh();

                    bankAccount = dt.Rows[i]["account_no"].ObjToString();
                    length = bankAccount.Length;
                    start = length - 4;
                    if (start < 0)
                        continue;
                    search = bankAccount.Substring(start, 4);

                    cmd = "Select * from `payments` where `payDate8` >= '" + sDate + "' AND `payDate8` <= '" + eDate + "' AND `bank_account` LIKE '%" + search + "';";
                    dx = G1.get_db_data(cmd);
                    totalDeposits = 0D;
                    for (int j = 0; j < dx.Rows.Count; j++)
                    {
                        payment = DailyHistory.getPayment(dx, j);
                        //downPayment = DailyHistory.getDownPayment(dx, j);
                        totalDeposits += payment;
                    }

                    cmd = "Select * from `downpayments` where `date` >= '" + sDate + "' AND `date` <= '" + eDate + "' AND `bankAccount` LIKE '%" + search + "';";
                    dx = G1.get_db_data(cmd);
                    //totalDeposits = 0D;
                    for (int j = 0; j < dx.Rows.Count; j++)
                    {
                        payment = dx.Rows[j]["totalDeposit"].ObjToDouble();
                        totalDeposits += payment;
                    }
                    totalDeposits = G1.RoundValue(totalDeposits);
                    dt.Rows[i]["deposits"] = totalDeposits;

                    totalDeposits = G1.RoundValue(totalDeposits);
                    dt.Rows[i]["deposits"] = totalDeposits;

                    cmd = "Select * from `ipayments` where `payDate8` >= '" + sDate + "' AND `payDate8` <= '" + eDate + "' AND `bank_account` LIKE '%" + search + "';";
                    dx = G1.get_db_data(cmd);
                    totalInsurance = 0D;
                    for (int j = 0; j < dx.Rows.Count; j++)
                    {
                        payment = DailyHistory.getPayment(dx, j);
                        totalInsurance += payment;
                    }
                    totalInsurance = G1.RoundValue(totalInsurance);
                    dt.Rows[i]["insuranceDeposits"] = totalInsurance;

                    cmd = "Select * from `cust_payment_details` where `dateReceived` >= '" + sDate + "' AND `dateReceived` <= '" + eDate + "' AND `bankAccount` LIKE '%" + search + "';";
                    dx = G1.get_db_data(cmd);
                    totalFuneral = 0D;
                    for (int j = 0; j < dx.Rows.Count; j++)
                    {
                        depositNumber = dx.Rows[j]["depositNumber"].ObjToString().ToUpper();
                        if ( depositNumber == "TD039")
                        {

                        }
                        if (depositNumber.IndexOf("TD") < 0 && depositNumber.IndexOf("CCT") < 0)
                        {
                            payment = dx.Rows[j]["paid"].ObjToDouble();
                            totalFuneral += payment;
                        }
                    }
                    totalFuneral = G1.RoundValue(totalFuneral);
                    dt.Rows[i]["funeralDeposits"] = totalFuneral;

                    totalTotal = totalDeposits + totalInsurance + totalFuneral;
                    dt.Rows[i]["totalDeposits"] = totalTotal;
                }
            }
            catch ( Exception ex)
            {
            }
            dgv.DataSource = dt;
            dgv.Refresh();
        }
        /****************************************************************************************/
        private void btnSelectColumns_Click(object sender, EventArgs e)
        {
            string actualName = cmbSelectColumns.Text;
            SelectColumns sform = new SelectColumns(dgv, "EditBankAccounts", "Primary", actualName);
            sform.Done += new SelectColumns.d_void_eventdone(sform_Done);
            sform.Show();
        }
        /***********************************************************************************************/
        void sform_Done(DataTable dt)
        {
            string name = cmbSelectColumns.Text.Trim();
            if (String.IsNullOrWhiteSpace(name))
                name = "Primary";
            string saveName = "EditBankAccounts " + name;
            string skinName = "";
            SetupSelectedColumns("EditBankAccounts", name, dgv);
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
                procType = "";
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
        /***********************************************************************************************/
        private void cmbSelectColumns_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            //ComboBox combo = (ComboBox)sender;
            //string comboName = combo.Text;
            string comboName = cmbSelectColumns.Text;
            string skinName = "";
            if (!String.IsNullOrWhiteSpace(comboName))
            {
                SetupSelectedColumns("EditBankAccounts", comboName, dgv);
                string name = "EditBankAccounts " + comboName;
                foundLocalPreference = G1.RestoreGridLayout(this, this.dgv, gridMain, LoginForm.username, name, ref skinName);
                SetupTotalsSummary();
                gridMain.OptionsView.ShowFooter = true;
            }
            else
            {
                SetupSelectedColumns("EditBankAccounts", "Primary", dgv);
                string name = "EditBankAccounts Primary";
                foundLocalPreference = G1.RestoreGridLayout(this, this.dgv, gridMain, LoginForm.username, name, ref skinName);
                gridMain.OptionsView.ShowFooter = true;
                SetupTotalsSummary();
            }
        }
        /***********************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            DataTable dt = (DataTable)dgv.DataSource;
            int rowhandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowhandle);


            string bankAccount = dr["account_no"].ObjToString();
            string account_no = dr["account_no"].ObjToString();
            string accountTitle = dr["account_title"].ObjToString();

            int length = bankAccount.Length;
            int start = length - 4;
            if (start < 0)
            {
                MessageBox.Show("*** ERROR *** Invalid Bank Account!", "Bank Account Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }
            string search = bankAccount.Substring(start, 4);
            DateTime date1 = this.dateTimePicker1.Value;
            DateTime date2 = this.dateTimePicker2.Value;

            EditBankDeposits editForm = new EditBankDeposits( accountTitle, account_no, date1, date2, search );
            editForm.Show();
        }
        /***********************************************************************************************/
    }
}