﻿using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GeneralLib;
using DevExpress.XtraPrinting;
using DevExpress.Utils;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class ViewDataTable : DevExpress.XtraEditors.XtraForm
    {
        private DataTable workDt = null;
        private string workFields = "";
        private string workTotals = "";
        private bool workMulti = false;
        /***********************************************************************************************/
        public ViewDataTable( DataTable dt, string fields = "", string totals = "" )
        {
            InitializeComponent();
            workDt = dt;
            workFields = fields;
            workTotals = totals;
        }
        /***********************************************************************************************/
        public ViewDataTable(DataTable dt, bool multiSelect = false, string fields = "")
        {
            InitializeComponent();
            workDt = dt;
            workFields = fields;
            workMulti = multiSelect;
            btnFinished.Hide();
        }
        /***********************************************************************************************/
        private void ViewDataTable_Load(object sender, EventArgs e)
        {
            if (ManualDone == null)
                btnFinished.Hide();
            if (String.IsNullOrWhiteSpace(workFields))
            {
                dgv.DataSource = workDt;
                return;
            }

            //if (workMulti)
            //    dgv.ContextMenuStrip = this.contextMenuStrip1;

            string[] Lines = workFields.Split(',');

            string field = "";
            string toType = "";
            DataRow dR = null;
            DataTable dt = new DataTable();

            if ( workMulti )
            {
                dt.Columns.Add("Select");
            }

            for ( int i=0; i<workDt.Rows.Count; i++)
            {
                dR = dt.NewRow();
                dt.Rows.Add(dR);
            }

            G1.NumberDataTable(dt);

            for (int i = 0; i < Lines.Length; i++)
            {
                try
                {
                    field = Lines[i].Trim();
                    field = field.Replace("(", "");
                    field = field.Replace(")", "");
                    if (G1.get_column_number(workDt, field) < 0)
                        continue;
                    toType = workDt.Columns[field].DataType.ToString().ToUpper();

                    if (toType.IndexOf("MYSQLDATETIME") >= 0)
                        dt.Columns.Add(field, Type.GetType("System.DateTime"));
                    else if (toType.IndexOf("DOUBLE") >= 0)
                        dt.Columns.Add(field, Type.GetType("System.Double"));
                    else if (toType.IndexOf("DECIMAL") >= 0)
                        dt.Columns.Add(field, Type.GetType("System.Decimal"));
                    else if (toType.IndexOf("INT32") >= 0)
                        dt.Columns.Add(field, Type.GetType("System.Int32"));
                    else if (toType.IndexOf("INT64") >= 0)
                        dt.Columns.Add(field, Type.GetType("System.Double"));
                    else if (toType.ToUpper() == "SYSTEM.BYTE[]")
                        continue;
                    else
                        dt.Columns.Add(field, Type.GetType("System.String"));

                    G1.copy_dt_column(workDt, field, dt, field);
                }
                catch ( Exception ex)
                {
                }
            }

            dgv.DataSource = dt;

            for (int i = 0; i < Lines.Length; i++)
            {
                try
                {
                    field = Lines[i].Trim();
                    if ( field.IndexOf( "(") >= 0 )
                    {
                        field = field.Replace("(", "");
                        field = field.Replace(")", "");
                        gridMain.Columns[field].Visible = false;
                    }
                }
                catch (Exception ex)
                {
                }
            }
            if (workMulti)
            {
                gridMain.Columns["Select"].ColumnEdit = this.repositoryItemCheckEdit4;
                SetupSelection(dt);
                btnFinished.Show();
                if (G1.get_column_number(dt, "SelectedRow") < 0)
                    dt.Columns.Add("SelectedRow");

                //gridMain.OptionsSelection.MultiSelect = true;
            }
            if (!String.IsNullOrWhiteSpace(workTotals))
            {
                Lines = workTotals.Split(',');
                for (int i = 0; i < Lines.Length; i++)
                {
                    field = Lines[i].Trim();
                    if (!String.IsNullOrWhiteSpace(field))
                    {
                        AddSummaryColumn(field, gridMain);
                        gridMain.OptionsView.ShowFooter = true;
                    }
                }
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
        /***********************************************************************************************/
        private void SetupSelection(DataTable dt)
        {
            DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew = this.repositoryItemCheckEdit4;
            selectnew.NullText = "";
            selectnew.ValueChecked = "1";
            selectnew.ValueUnchecked = "0";
            selectnew.ValueGrayed = "";
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["select"] = "0";
        }
        /***************************************************************************************/
        public delegate void d_void_eventdone_datarow(DataTable dd, DataRow dx );
        public event d_void_eventdone_datarow ManualDone;
        protected void OnManualDone(DataTable dd, DataRow dx )
        {
            if (ManualDone != null)
            {
                this.Cursor = Cursors.WaitCursor;
                ManualDone.Invoke(dd, dx);
                this.Cursor = Cursors.Default;
            }
        }
        /***********************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();
            if (dr != null)
                OnManualDone(dt, dr);
        }
        /***********************************************************************************************/
        private void selectRowsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (G1.get_column_number(dt, "SelectedRow") < 0)
                dt.Columns.Add("SelectedRow");
            int row = 0;
            int rowIndex = 0;
            int[] rows = gridMain.GetSelectedRows();
            try
            {
                for (int i = 0; i < rows.Length; i++)
                {
                    row = rows[i];
                    rowIndex = gridMain.GetDataSourceRowIndex(row);
                    dt.Rows[rowIndex]["SelectedRow"] = "Y";
                }
                OnManualDone(dt, null );
            }
            catch (Exception ex)
            {
                MessageBox.Show("*ERROR*** " + ex.Message.ToString());
            }

        }
        /***********************************************************************************************/
        private void btnFinished_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            string select = "";
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                select = dt.Rows[i]["select"].ObjToString();
                if (select == "1")
                    dt.Rows[i]["SelectedRow"] = "Y";
            }
            DataRow dr = null;
            OnManualDone(dt, dr);
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
            //Printer.DrawQuad(5, 8, 8, 4, this.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            font = new Font("Ariel", 10, FontStyle.Regular);
            string reportName = this.Text;
            string report = reportName;
            Printer.DrawQuad(5, 8, 8, 4, report, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);


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
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            G1.SpyGlass(gridMain);
        }
        /***********************************************************************************************/
    }
}