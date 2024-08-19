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
using DevExpress.XtraGrid.Views.Base;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class CheckBillingReport : DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        private DataTable workDt = null;
        public CheckBillingReport( DataTable dt )
        {
            InitializeComponent();
            workDt = dt;
        }
        /***********************************************************************************************/
        private void CheckBillingReport_Load(object sender, EventArgs e)
        {
        }
        /***********************************************************************************************/
        private void btnPull_Click(object sender, EventArgs e)
        {
            DataTable dt = null;
            this.Cursor = Cursors.WaitCursor;
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string file = ofd.FileName;
                    dgv.DataSource = null;
                    try
                    {
                        dt = Import.ImportCSVfile(file);
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
            this.Cursor = Cursors.Default;
            if (dt == null)
                return;
            DataTable tempDt = null;
            DataTable inventoryDt = null;
            DataTable dx = workDt.Clone();
            dx.Columns.Add("comment");
            dx.Columns.Add("bsid");
            dx.Columns.Add("sidResult");
            DataRow dRow = null;
            string serialNumber = "";
            string description = "";
            string serviceId = "";
            string desc2 = "";
            string cmd = "";
            int i = 0;
            this.Cursor = Cursors.WaitCursor;
            try
            {
                for (i = 0; i < dt.Rows.Count; i++)
                {
                    serialNumber = dt.Rows[i]["Serial #"].ObjToString();
                    if (String.IsNullOrWhiteSpace(serialNumber))
                        continue;
                    description = dt.Rows[i]["Desc Ln 1"].ObjToString();
                    serviceId = dt.Rows[i]["Customer PO"].ObjToString();
                    cmd = "Select * from `inventory` where `SerialNumber` = '" + serialNumber + "';";
                    tempDt = G1.get_db_data(cmd);
                    if (tempDt.Rows.Count > 0)
                    {
                        for (int j = 0; j < tempDt.Rows.Count; j++)
                        {
                            dx.ImportRow(tempDt.Rows[j]);
                            int row = dx.Rows.Count - 1;
                            dx.Rows[row]["bsid"] = serviceId;
                            if (j > 0)
                                dx.Rows[row]["comment"] = "Duplicate SS#";
                            desc2 = tempDt.Rows[j]["CasketDescription"].ObjToString();
                            cmd = "Select * from `inventorylist` where `casketdesc` = '" + desc2 + "';";
                            inventoryDt = G1.get_db_data(cmd);
                            if (inventoryDt.Rows.Count > 0)
                            {
                                dx.Rows[row]["casketguage"] = inventoryDt.Rows[0]["casketguage"].ObjToString();
                                dx.Rows[row]["caskettype"] = inventoryDt.Rows[0]["caskettype"].ObjToString();
                            }
                        }
                    }
                    else
                    {
                        dRow = dx.NewRow();
                        dRow["SerialNumber"] = serialNumber;
                        dRow["CasketDescription"] = description;
                        dRow["ServiceID"] = serviceId;
                        dRow["bsid"] = serviceId;
                        dRow["comment"] = "NOT IN INVENTORY";

                        cmd = "Select * from `inventorylist` where `casketdesc` = '" + description + "';";
                        inventoryDt = G1.get_db_data(cmd);
                        if (inventoryDt.Rows.Count > 0)
                        {
                            dRow["casketguage"] = inventoryDt.Rows[0]["casketguage"].ObjToString();
                            dRow["caskettype"] = inventoryDt.Rows[0]["caskettype"].ObjToString();
                        }
                        dx.Rows.Add(dRow);
                    }
                }
            }
            catch ( Exception ex)
            {
                MessageBox.Show("***ERROR*** " + ex.Message.ToString());
            }
            //string BSID = "";
            //int sidResult = 0;
            //this.Cursor = Cursors.WaitCursor;
            //for ( int i=0; i<dx.Rows.Count; i++)
            //{
            //    sidResult = 0;
            //    serviceId = dx.Rows[i]["serviceId"].ObjToString();
            //    if (!String.IsNullOrWhiteSpace(serviceId))
            //    {
            //        cmd = "Select * from `contracts` where `ServiceId` = '" + serviceId + "';";
            //        tempDt = G1.get_db_data(cmd);
            //        if (tempDt.Rows.Count > 0)
            //            sidResult += 1;
            //    }
            //    BSID = dx.Rows[i]["bsid"].ObjToString();
            //    if (!String.IsNullOrWhiteSpace(BSID))
            //    {
            //        cmd = "Select * from `contracts` where `ServiceId` = '" + BSID + "';";
            //        tempDt = G1.get_db_data(cmd);
            //        if (tempDt.Rows.Count > 0)
            //            sidResult += 2;
            //    }
            //    if (sidResult == 0)
            //        dx.Rows[i]["sidResult"] = "Not Found";
            //    else if (sidResult == 1)
            //        dx.Rows[i]["sidResult"] = "Trust";
            //    else if (sidResult == 2)
            //        dx.Rows[i]["sidResult"] = "Batesville";
            //    else if (sidResult == 3)
            //    {
            //        if ( serviceId == BSID)
            //            dx.Rows[i]["sidResult"] = "Match";
            //        else
            //            dx.Rows[i]["sidResult"] = "Both Found";
            //    }
            //}
            G1.NumberDataTable(dx);
            dgv.DataSource = dx;
            this.Cursor = Cursors.Default;
            //this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private string printDate = "";
        private string printDateReceived = "";
        private string printDateDeceased = "";
        private string printLocation = "";
        private string printType = "";
        private string printGuage = "";
        private string printUsed = "";
        private string printOwner = "";
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
            printableComponentLink1.CreateDetailHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateDetailHeaderArea);
            printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);
            printableComponentLink1.BeforeCreateAreas += new System.EventHandler(this.printableComponentLink1_BeforeCreateAreas);
            printableComponentLink1.AfterCreateAreas += new System.EventHandler(this.printableComponentLink1_AfterCreateAreas);
            printableComponentLink1.Landscape = true;

            Printer.setupPrinterMargins(50, 50, 70, 50);

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

            Printer.setupPrinterMargins(50, 50, 70, 50);

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
        private int pageMarginLeft = 0;
        private int pageMarginRight = 0;
        private int pageMarginTop = 0;
        private int pageMarginBottom = 0;
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
            Printer.DrawQuad(1, 1, Printer.xQuads, 1, "South Mississippi Funeral Services, LLC", Color.Black, BorderSide.None, font, HorizontalAlignment.Center);

            Printer.SetQuadSize(12, 12);

            font = new Font("Ariel", 8);
            Printer.DrawGridDate(2, 1, 2, 3, Color.Black, BorderSide.None, font);
            Printer.DrawGridPage(11, 2, 2, 3, Color.Black, BorderSide.None, font);
            Printer.DrawQuad(1, 7, 2, 3, "User : " + LoginForm.username, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);

            font = new Font("Ariel", 10, FontStyle.Italic);
            Printer.DrawQuad(5, 7, 4, 3, "Batesville Billing Report Reconciliation", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
        }
        /***********************************************************************************************/
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
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
            else if (e.Column.FieldName.ToUpper().IndexOf("BSID") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                int row = e.ListSourceRowIndex;
                DataTable dt = (DataTable)dgv.DataSource;
                string bid1 = dt.Rows[row]["ServiceId"].ObjToString();
                string bid2 = dt.Rows[row]["bsid"].ObjToString();
                if ( bid2 != bid1 )
                {
                    e.DisplayText = "(" + bid2 + ")";
                }
            }
        }
        /***********************************************************************************************/
        private void chkSort_CheckedChanged(object sender, EventArgs e)
        {
            if (chkSort.Checked)
            {
                gridMain.Columns["LocationCode"].GroupIndex = 0;
                gridMain.Columns["num"].Visible = false;
                this.gridMain.ExpandAllGroups();
            }
            else
            {
                gridMain.Columns["LocationCode"].GroupIndex = -1;
                gridMain.Columns["num"].Visible = true;
            }
            gridMain.RefreshData();
            dgv.Refresh();
        }
        /***********************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            SetSpyGlass(gridMain);
        }
        /***********************************************************************************************/
        private void SetSpyGlass(DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView grid)
        {
            if (grid.OptionsFind.AlwaysVisible == true)
                grid.OptionsFind.AlwaysVisible = false;
            else
                grid.OptionsFind.AlwaysVisible = true;
        }
        /***********************************************************************************************/
    }
}