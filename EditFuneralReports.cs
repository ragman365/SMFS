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
using DevExpress.XtraEditors.Repository;
//using iTextSharp.text;

/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class EditFuneralReports : DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        private bool modified = false;
        private bool Selecting = false;
        private bool loading = true;
        /***********************************************************************************************/
        public EditFuneralReports()
        {
            InitializeComponent();
        }
        /***********************************************************************************************/
        private void EditFuneralReports_Load(object sender, EventArgs e)
        {
            loading = true;
            LoadData();
            loading = false;
            btnSave.Hide();
        }
        /***********************************************************************************************/
        private void LoadData()
        {
            this.Cursor = Cursors.WaitCursor;
            string cmd = "Select * from `funeralReports` ORDER by `order`;";
            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("num");
            dt.Columns.Add("mod");

            G1.NumberDataTable(dt);
            dgv.DataSource = dt;

            LoadFuneralHomeData();
            this.Cursor = Cursors.Default;
            modified = false;
        }
        /***********************************************************************************************/
        private void LoadFuneralHomeData()
        {
            string command = "Select * FROM `funeralHomes` order by `record`;";
            DataTable rx = G1.get_db_data(command);
            if (rx == null || rx.Rows == null || rx.Rows.Count == 0)
                return; // Somehow the table does not exist
            repositoryItemCheckedComboBoxEdit1.Items.Clear();
            repositoryItemCheckedComboBoxEdit2.Items.Clear();

            DataTable mercDt = new DataTable();
            mercDt.Columns.Add("location");
            DataRow[] dRows = null;
            DataRow dR = null;

            string atNeedCode = "";
            for (int i = 0; i < rx.Rows.Count; i++)
            {
                atNeedCode = rx.Rows[i]["atNeedCode"].ObjToString();
                if (String.IsNullOrWhiteSpace(atNeedCode))
                    continue;
                dRows = mercDt.Select("location='" + atNeedCode + "'");
                if ( dRows.Length <= 0 )
                {
                    dR = mercDt.NewRow();
                    dR["location"] = atNeedCode;
                    mercDt.Rows.Add(dR);
                }
            }

            DataView tempview = mercDt.DefaultView;
            tempview.Sort = "location asc";
            mercDt = tempview.ToTable();

            for (int i = 0; i < mercDt.Rows.Count; i++)
            {
                atNeedCode = mercDt.Rows[i]["location"].ObjToString();
                if (!String.IsNullOrWhiteSpace(atNeedCode))
                    repositoryItemCheckedComboBoxEdit1.Items.Add(atNeedCode);
            }

            string mercCode = "";
            mercDt.Rows.Clear();

            for (int i = 0; i < rx.Rows.Count; i++)
            {
                mercCode = rx.Rows[i]["merchandiseCode"].ObjToString();
                if (String.IsNullOrWhiteSpace(mercCode))
                    continue;
                dRows = mercDt.Select("location='" + mercCode + "'");
                if ( dRows.Length <= 0 )
                {
                    dR = mercDt.NewRow();
                    dR["location"] = mercCode;
                    mercDt.Rows.Add(dR);
                }
            }

            tempview = mercDt.DefaultView;
            tempview.Sort = "location asc";
            mercDt = tempview.ToTable();

            for (int i = 0; i < mercDt.Rows.Count; i++)
            {
                mercCode = mercDt.Rows[i]["location"].ObjToString();
                if (!String.IsNullOrWhiteSpace(mercCode))
                    repositoryItemCheckedComboBoxEdit2.Items.Add(mercCode);
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
            DialogResult result = MessageBox.Show("***Question***\nSetup has been modified!\nWould you like to save your changes?", "Funeral Reports Setup Dialog", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
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

            font = new Font("Ariel", 10, FontStyle.Regular);
                Printer.DrawQuad(6, 8, 4, 4, this.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

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
            string service = dr["reportName"].ObjToString();
            DialogResult result = MessageBox.Show("***Question*** Are you sure you want to delete this Funeral Report (" + service + ") ?", "Delete Funeral Report Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
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
            string reportName = "";
            string funeralLocations = "";
            string merchandiseLocations = "";
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
                            G1.delete_db_table("funeralReports", "record", record);
                        dt.Rows[i]["record"] = -1;
                        continue;
                    }
                    if (String.IsNullOrWhiteSpace(record))
                        record = G1.create_record("funeralReports", "merchandiseLocations", "-1");
                    if (G1.BadRecord("funeralReports", record))
                        continue;

                    reportName = dt.Rows[i]["reportName"].ObjToString();
                    funeralLocations = dt.Rows[i]["funeralLocations"].ObjToString();
                    merchandiseLocations = dt.Rows[i]["merchandiseLocations"].ObjToString();
                    G1.update_db_table("funeralReports", "record", record, new string[] { "reportName", reportName, "funeralLocations", funeralLocations, "merchandiseLocations", merchandiseLocations, "order", i.ToString() });
                }
                catch ( Exception ex)
                {
                    MessageBox.Show("***ERROR*** Updating Funeral Report Table " + ex.Message.ToString());
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
        private void ClearSelection ( DataTable dt, string column)
        {
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i][column] = "0";
        }
        /***********************************************************************************************/
        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveSetup();
        }
        /****************************************************************************************/
        private string oldWhat = "";
        /***********************************************************************************************/
        private void gridMain_ValidatingEditor(object sender, DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventArgs e)
        {
            GridView view = sender as GridView;
            //if (view.FocusedColumn.FieldName.ToUpper() == "ASOFDATE")
            //{
            //    DataTable dt = (DataTable)dgv.DataSource;
            //    DataRow dr = gridMain.GetFocusedDataRow();
            //    int rowhandle = gridMain.FocusedRowHandle;
            //    int row = gridMain.GetDataSourceRowIndex(rowhandle);
            //    oldWhat = e.Value.ObjToString();
            //    DateTime date = oldWhat.ObjToDateTime();
            //    dt.Rows[row]["ASOFDATE"] = G1.DTtoMySQLDT(date);
            //    e.Value = G1.DTtoMySQLDT(date);
            //}
        }
    }
    /***********************************************************************************************/
}