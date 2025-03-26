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
using System.IO;
using System.Web;
using System.Text.RegularExpressions;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraGrid.Views.Grid;
using System.Drawing.Drawing2D;
using DevExpress.Utils.Drawing;
using DevExpress.Utils;
using DevExpress.XtraEditors.Repository;
using System.Diagnostics.Contracts;
using DevExpress.XtraGrid.Columns;
using System.Configuration;
using DevExpress.XtraPrinting;
using DevExpress.XtraEditors.ViewInfo;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class PdfSignatures : DevExpress.XtraEditors.XtraForm
    {
        private bool loading = true;
        private bool modified = false;
        private string workContract = "";
        private string workFdName = "";
        private DataTable workDt = null;
        private DataTable tempDt = null;
        private bool QuitAll = false;
        /****************************************************************************************/
        public PdfSignatures(string contract, DataTable dt)
        {
            InitializeComponent();
            workContract = contract;
            workDt = dt;
            tempDt = workDt.Copy();
        }
        /****************************************************************************************/
        private void PdfSignatures_Load(object sender, EventArgs e)
        {
            oldWhat = "";

            loading = true;
            btnExit.Hide();

            LoadData();

            QuitAll = false;

            modified = false;
            loading = false;
        }
        /***********************************************************************************************/
        private void LoadData()
        {
            this.Cursor = Cursors.WaitCursor;

            G1.NumberDataTable(tempDt);

            dgv.DataSource = tempDt;

            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void gridMain_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            int row = e.ListSourceRow;
            if (dgv == null)
                return;
            if (dgv.DataSource == null)
                return;
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt.Rows.Count <= 0)
                return;
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
            int rowHandle = e.RowHandle;
            if (rowHandle < 0)
                return;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            if (dgv.DataSource == null)
                return;
        }
        /****************************************************************************************/
        private bool justSaved = false;
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            DataRow dr = gridMain.GetFocusedDataRow();
            if (dr == null)
                return;

            //dr["mod"] = "Y";
            //GridColumn currCol = gridMain.FocusedColumn;
            //string currentColumn = currCol.FieldName;
            //if (currentColumn.ToUpper() == "NUM")
            //    return;

            string what = dr[currentColumn].ObjToString();
            gridMain.RefreshData();
        }
        /****************************************************************************************/
        private void UpdateMod(DataRow dr)
        {
            dr["mod"] = "Y";
            modified = true;
            btnExit.Show();
            btnExit.Refresh();
        }
        /***********************************************************************************************/
        private void AddMod(DataTable dt, DevExpress.XtraGrid.Views.Grid.GridView grid)
        {
            if (G1.get_column_number(dt, "mod") < 0)
                dt.Columns.Add("mod");
        }
        /****************************************************************************************/
        private void pictureBox4_Click(object sender, EventArgs e)
        {
            if (gridMain.OptionsFind.AlwaysVisible == true)
                gridMain.OptionsFind.AlwaysVisible = false;
            else
                gridMain.OptionsFind.AlwaysVisible = true;
        }
        /****************************************************************************************/
        private void gridMain_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            if (e.ListSourceRowIndex == DevExpress.XtraGrid.GridControl.InvalidRowHandle)
                return;
            string name = e.Column.FieldName;
            if ( name.ToUpper().IndexOf("DATE") >= 0 )
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
        public DataTable SignatureResults
        {
            get
            {
                return tempDt;
            }
        }
        /****************************************************************************************/
        public bool ExitAll
        {
            get
            {
                return QuitAll;
            }
        }
        /****************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            DataTable dt = (DataTable)dgv.DataSource;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetFocusedDataSourceRowIndex();
            string what = dr["desc"].ObjToString();

            Image emptyImage = new Bitmap(1, 1);
            Byte[] bytes = dt.Rows[row]["signature"].ObjToBytes();
            Image myImage = emptyImage;
            if (bytes != null)
                myImage = G1.byteArrayToImage(bytes);

            Image signature = GetSignature("Enter Signature of " + what, myImage );
            if ( signature != null )
            {
                ImageConverter converter = new ImageConverter();
                bytes = (byte[])converter.ConvertTo(signature, typeof(byte[]));
                dr["signature"] = bytes;
                gridMain.RefreshData();
                gridMain.RefreshEditor(true);
                dgv.Refresh();
                modified = true;
                btnExit.Show();
                btnExit.Refresh();
            }
        }
        /****************************************************************************************/
        private Image GetSignature(string what, Image signature )
        {
            //Image signature = new Bitmap(1, 1);
            using (SignatureForm signatureForm = new SignatureForm(what, signature))
            {
                if (signatureForm.ShowDialog() == DialogResult.OK)
                {
                    signature = signatureForm.SignatureResult;
                }
                else
                    signature = null;
            }
            return signature;
        }
        /****************************************************************************************/
        private void gridMain_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();

            int focusedRow = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(focusedRow);

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

            printingSystem1.Document.AutoFitToPagesWidth = 1;

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
            //            Printer.DrawQuad(6, 8, 4, 4, "Funeral Services Report", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            Printer.DrawQuad(5, 8, 8, 4, this.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

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
        /****************************************************************************************/
        private void gridMain_ValidatingEditor(object sender, DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventArgs e)
        {
            GridView view = sender as GridView;
        }
        private string oldWhat = "";
        /****************************************************************************************/
        private void gridMain_CustomRowCellEdit(object sender, CustomRowCellEditEventArgs e)
        {
            GridView view = sender as GridView;
            //if (e.Column.FieldName.ToUpper() == "CHECKLIST")
            //{
            //    string type = view.GetRowCellValue(e.RowHandle, "type").ObjToString().ToUpper();
            //    if (type != "INSURANCE" && type != "POLICY" && type != "INSURANCE DIRECT" && type != "INSURANCE UNITY" && type != "3RD PARTY")
            //    {
            //        e.RepositoryItem = null;
            //        return;
            //    }
            //    string status = view.GetRowCellValue(e.RowHandle, "status").ObjToString();
            //    if (status.ToUpper() == "FILED")
            //        e.RepositoryItem = this.repositoryItemButtonEdit2;
            //    else if ( status.ToUpper() == "DEPOSITED")
            //        e.RepositoryItem = this.repositoryItemButtonEdit1;
            //    else
            //        e.RepositoryItem = this.repositoryItemButtonEdit2;
            //}
        }
        /****************************************************************************************/
        private string oldColumn = "";
        private DataTable trackingDt = null;
        private DataTable trackDt = null;
        RepositoryItemComboBox ciLookup = new RepositoryItemComboBox();
        /****************************************************************************************/
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

            bool doDate = false;
            //if (name == "apptDate")
            //    doDate = true;
            //else if (name == "lastContactDate")
            //    doDate = true;

            if (name.ToUpper().IndexOf("DATE") >= 0)
                doDate = true;

            if (doDate)
            {
                myDate = dr[name].ObjToDateTime();
                str = gridMain.Columns[name].Caption;
                using (GetDate dateForm = new GetDate(myDate, str))
                {
                    dateForm.ShowDialog();
                    if (dateForm.DialogResult == System.Windows.Forms.DialogResult.OK)
                    {
                        try
                        {
                            myDate = dateForm.myDateAnswer;
                            DateTime date = myDate.ObjToDateTime();

                            dr[name] = G1.DTtoMySQLDT(myDate);
                        }
                        catch (Exception ex)
                        {
                        }
                        //dr[name] = G1.DTtoMySQLDT(myDate);
                        UpdateMod(dr);
                        gridMain_CellValueChanged(null, null);
                    }
                }
            }
            gridMain.RefreshData();
            gridMain.RefreshEditor(true);
        }
        /****************************************************************************************/
        private string currentColumn = "";
        private string oldContactType = "";
        /****************************************************************************************/
        private void comboBox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            System.Windows.Forms.ComboBox cbo = (System.Windows.Forms.ComboBox)sender;
            cbo.PreviewKeyDown -= comboBox_PreviewKeyDown;
            if (cbo.DroppedDown) cbo.Focus();
        }
        /****************************************************************************************/
        private void gridMain_CalcRowHeight(object sender, RowHeightEventArgs e)
        {
            GridView View = sender as GridView;
            if (e.RowHandle >= 0)
            {
                int maxHeight = 0;
                int newHeight = 0;
                bool doit = false;
                string name = "";
                string str = "";
                int count = 0;
                string[] Lines = null;
                foreach (GridColumn column in gridMain.Columns)
                {
                    name = column.FieldName.ToUpper();
                    //if (name == "RESULTS" )
                    //    doit = true;
                    if (doit)
                    {
                        using (RepositoryItemMemoEdit edit = new RepositoryItemMemoEdit())
                        {
                            using (MemoEditViewInfo viewInfo = edit.CreateViewInfo() as MemoEditViewInfo)
                            {
                                str = gridMain.GetRowCellValue(e.RowHandle, column.FieldName).ObjToString();
                                if ( !String.IsNullOrWhiteSpace ( str ))
                                {
                                    Lines = str.Split('\n');
                                    count = Lines.Length + 1;
                                }
                                viewInfo.EditValue = gridMain.GetRowCellValue(e.RowHandle, column.FieldName);
                                viewInfo.Bounds = new Rectangle(0, 0, column.VisibleWidth, dgv.Height);
                                using (Graphics graphics = dgv.CreateGraphics())
                                using (GraphicsCache cache = new GraphicsCache(graphics))
                                {
                                    viewInfo.CalcViewInfo(graphics);
                                    var height = ((IHeightAdaptable)viewInfo).CalcHeight(cache, column.VisibleWidth);
                                    newHeight = Math.Max(height, maxHeight);
                                    if (newHeight > maxHeight)
                                    {
                                        maxHeight = newHeight * count;
                                    }
                                }
                            }
                        }
                    }
                }

                if (maxHeight > 0 && maxHeight > e.RowHeight )
                    e.RowHeight = maxHeight;
                e.RowHeight = 40;
            }
        }
        /****************************************************************************************/
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            modified = false;
            this.Close();
        }
        /****************************************************************************************/
        private void btnExit_Click(object sender, EventArgs e)
        {
            modified = false;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        /****************************************************************************************/
        private void PdfSignatures_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (modified)
            {
                DialogResult result = MessageBox.Show("***Question***\nData has been modified!\nWould you like a to save your changes?", "Data Modified Dialog", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
                if (result == DialogResult.Yes)
                    btnExit_Click(null, null);
                modified = false;
                QuitAll = true;
            }
            this.Validate();
            gridMain.RefreshEditor(true);
        }
        /****************************************************************************************/
        private void btnExitAll_Click(object sender, EventArgs e)
        {
            modified = false;
            QuitAll = true;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        /****************************************************************************************/
    }
}