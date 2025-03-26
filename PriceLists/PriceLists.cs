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
using System.Threading;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class PriceLists : DevExpress.XtraEditors.XtraForm
    {
        private bool modified = false;
        private DataTable workDt = null;
        private bool loading = false;
        private string workFunRec = "";
        private string workPrices = "";
        private string workGroupName = "";
        private string workCasketName = "";
        private string workAsOfDate = "";
        private string funeralHome = "";
        /***********************************************************************************************/
        public PriceLists( string whatPrices = "" , string funeralHomeRecord = "" )
        {
            InitializeComponent();
            workFunRec = funeralHomeRecord;
            workPrices = whatPrices;
        }
        /***********************************************************************************************/
        public PriceLists( string whatPrices, DataTable dt, string asOfDate )
        {
            InitializeComponent();
            workDt = dt;
            workPrices = whatPrices;
            workAsOfDate = asOfDate;

            this.Text += " for " + workPrices + " Prices";
            if (!String.IsNullOrWhiteSpace(workAsOfDate))
                this.Text += " as of " + workAsOfDate;
        }
        /***********************************************************************************************/
        private void PriceLists_Load(object sender, EventArgs e)
        {
            loading = true;
            btnMassPrint.Hide();

            LoadData();

            loading = false;
            btnSave.Hide();
        }
        /***********************************************************************************************/
        private void LoadData()
        {
            this.Cursor = Cursors.WaitCursor;

            SetupFuneralHome();

            string cmd = "Select * from `pricelists` order by `order`, `record`;";
            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("num");
            dt.Columns.Add("mod");
            dt.Columns.Add("select");

            SetupSelection(dt);

            G1.NumberDataTable(dt);
            if (workDt != null)
            {
                gridMain.Columns["select"].Visible = true;
                btnSave.Hide();
                btnSave.Refresh();
            }

            dgv.DataSource = dt;
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void SetupFuneralHome ()
        {
            if (String.IsNullOrWhiteSpace(workFunRec))
                return;
            string cmd = "Select * from `funeralHomes` where `record` = '" + workFunRec + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return;
            workGroupName = dt.Rows[0]["groupname"].ObjToString();
            workCasketName = dt.Rows[0]["casketgroup"].ObjToString();
            funeralHome = dt.Rows[0]["LocationCode"].ObjToString();

            this.Text = "Price Lists for " + funeralHome + "-" + workGroupName + "-" + workCasketName;
        }
        /***********************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            G1.ShowHideFindPanel(gridMain);

            //if (gridMain.OptionsFind.AlwaysVisible == true)
            //    gridMain.OptionsFind.AlwaysVisible = false;
            //else
            //    gridMain.OptionsFind.AlwaysVisible = true;
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
                dt.Rows.Add(dRow);
            }
            dgv.DataSource = dt;

            int row = dt.Rows.Count - 1;
            gridMain.SelectRow(row);
            gridMain.FocusedRowHandle = row;
            dgv.RefreshDataSource();
            dgv.Refresh();
            modified = true;
            btnSave.Show();
        }
        /***********************************************************************************************/
        private void pictureDelete_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            string service = dr["description"].ObjToString();
            DialogResult result = MessageBox.Show("***Question*** Are you sure you want to delete this entire Price List (" + service + ") ?", "Delete Price List Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
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
                    if (dRow != null)
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
            modified = true;
            btnSave.Show();
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
            dt.Rows.InsertAt(dRow, dtRow);
            G1.NumberDataTable(dt);
            dt.AcceptChanges();
            dgv.DataSource = dt;
            gridMain.ClearSelection();
            gridMain.RefreshData();
            gridMain.FocusedRowHandle = rowHandle + 1;
            gridMain.SelectRow(rowHandle + 1);
            dgv.Refresh();
            modified = true;
            btnSave.Show();
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
            printableComponentLink1.Landscape = false;

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

            font = new Font("Ariel", 10, FontStyle.Bold);
            Printer.DrawQuad(6, 8, 4, 4, "Price Lists Report", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);


            //            Printer.DrawQuadTicks();
            DateTime date = DateTime.Now;
            string workDate = date.Month.ToString("D2") + "/" + date.Year.ToString("D4");
            Printer.SetQuadSize(24, 12);
            font = new Font("Ariel", 9, FontStyle.Bold);
            //            Printer.DrawQuad(20, 8, 5, 4, "Report Month:" + workDate, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(16, 8, 3, 4, lblPayment.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(19, 8, 3, 4, lblTrust85.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(22, 8, 3, 4, lblTrust100.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /***********************************************************************************************/
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (gridMain.FocusedColumn.FieldName == "select")
                return;
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
        private void PriceLists_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!modified)
                return;
            DialogResult result = MessageBox.Show("***Question***\nPrice Lists have been modified!\nWould you like to save your changes?", "Price Lists Dialog", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (result == DialogResult.Cancel)
            {
                e.Cancel = true;
                return;
            }
            modified = false;
            if (result == DialogResult.No)
                return;
            SavePriceLists();
        }
        /***********************************************************************************************/
        private void SavePriceLists()
        {
            string record = "";
            string description = "";
            string title = "";
            string mod = "";
            string abrev = "";

            DataTable dt = (DataTable)dgv.DataSource;
            this.Cursor = Cursors.WaitCursor;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                record = dt.Rows[i]["record"].ObjToString();
                mod = dt.Rows[i]["mod"].ObjToString();
                if (mod.ToUpper() == "D")
                {
                    if (!String.IsNullOrWhiteSpace(record))
                        G1.delete_db_table("pricelists", "record", record);
                    continue;
                }
                if (String.IsNullOrWhiteSpace(record))
                    record = G1.create_record("pricelists", "description", "-1");
                if (G1.BadRecord("pricelists", record))
                    continue;
                description = dt.Rows[i]["description"].ObjToString();
                abrev = dt.Rows[i]["abrev"].ObjToString();
                title = dt.Rows[i]["title"].ObjToString();
                G1.update_db_table("pricelists", "record", record, new string[] { "description", description, "abrev", abrev, "title", title, "order", i.ToString() });
            }
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();

            string description = dr["description"].ObjToString();
            string title = dr["title"].ObjToString();
            if ( !String.IsNullOrWhiteSpace ( description))
            {
                this.Cursor = Cursors.WaitCursor;
                PriceList priceForm = new PriceList( description, title, workPrices, workFunRec );
                priceForm.Show();
                this.Cursor = Cursors.Default;
            }
        }
        /***********************************************************************************************/
        private void btnSave_Click(object sender, EventArgs e)
        {
            SavePriceLists();
            modified = false;
            btnSave.Visible = false;
        }
        /***********************************************************************************************/
        private void repositoryItemCheckEdit1_CheckedChanged(object sender, EventArgs e)
        {
            DevExpress.XtraEditors.CheckEdit check = (DevExpress.XtraEditors.CheckEdit)sender;
            bool isChecked = true;
            if (!check.Checked)
                isChecked = false;

            DataTable dt = (DataTable)dgv.DataSource;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetFocusedDataSourceRowIndex();

            DataRow dr = gridMain.GetFocusedDataRow();
            string select = dr["select"].ObjToString();
            if (isChecked)
            {
                dr["select"] = "1";
                btnMassPrint.Show();
                btnMassPrint.Refresh();
            }
            else
            {
                dr["select"] = "0";
                dt.Rows[row]["select"] = "0";

                DataRow[] dRows = dt.Select("select='1'");
                if (dRows.Length <= 0)
                {
                    btnMassPrint.Hide();
                    btnMassPrint.Refresh();
                }
            }
        }
        /***********************************************************************************************/
        private void SetupSelection(DataTable dt)
        {
            if (G1.get_column_number(dt, "select") < 0)
                dt.Columns.Add("select");

            DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew = this.repositoryItemCheckEdit1;
            selectnew.NullText = "";
            selectnew.ValueChecked = "1";
            selectnew.ValueUnchecked = "0";
            selectnew.ValueGrayed = "";
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["select"] = "0";
        }
        /***********************************************************************************************/
        static string description = "";
        static string title = "";
        private void btnMassPrint_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;

            string select = "";
            description = "";
            title = "";
            string record = "";
            string locationCode = "";

            PleaseWait waitForm = G1.StartWait();

            for ( int i=0; i<workDt.Rows.Count; i++)
            {
                Application.DoEvents();

                select = workDt.Rows[i]["select"].ObjToString();
                if (select != "1")
                    continue;
                record = workDt.Rows[i]["record"].ObjToString();
                locationCode = workDt.Rows[i]["locationCode"].ObjToString();

                for ( int j=0; j<dt.Rows.Count; j++)
                {
                    select = dt.Rows[j]["select"].ObjToString();
                    if (select != "1")
                        continue;
                    description = dt.Rows[j]["description"].ObjToString();
                    title = dt.Rows[j]["title"].ObjToString();

                    if (!String.IsNullOrWhiteSpace(description))
                    {
                        waitForm.FireEvent2("Please Wait!\nPrinting Price List for " + locationCode + " / " + description);

                        this.Cursor = Cursors.WaitCursor;
                        PriceList priceForm = new PriceList(description, title, workPrices, record, true, workAsOfDate );
                        //priceForm.ShowDialog();
                        this.Cursor = Cursors.Default;
                    }
                }
            }
            G1.StopWait(ref waitForm);

            MessageBox.Show("***INFO*** Mass Report Location Is - C:/SMFS_Reports/PriceLists", "Mass Report Info Dialog", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
        }
        /***********************************************************************************************/
        static void MessageThread()
        {
            MessageBox.Show(" Please Wait!\nBuilding Price List Report for " + description);
        }
        /***********************************************************************************************/
    }
}