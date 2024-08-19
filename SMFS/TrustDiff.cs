using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Drawing;

//using DevExpress.XtraEditors;
using DevExpress.XtraPrinting;

using GeneralLib;

using DevExpress.Utils;
//using DevExpress.Pdf;
using MySql.Data.MySqlClient;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using DevExpress.XtraGrid.Views.Base;
//using DocumentFormat.OpenXml.Drawing;
//using DevExpress.XtraRichEdit.Mouse;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class TrustDiff : DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        DataTable originalDt = null;
        public TrustDiff()
        {
            InitializeComponent();
        }
        /***********************************************************************************************/
        private void TrustDiff_Load(object sender, EventArgs e)
        {
            btnCalc.Hide();
            chkShowDiff.Hide();
            lblTotal.Hide();
            barImport.Hide();
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
            printableComponentLink1.ShowPreview();
            isPrinting = false;
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
            Printer.DrawQuad(6, 7, 4, 4, this.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);

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
        private void btnRun_Click(object sender, EventArgs e)
        {
            string cmd = "Select * from `customers` p JOIN `contracts` d ON p.`contractNumber` = d.`contractNumber` WHERE ";
            cmd += " p.`lapsed` <> 'Y' AND d.`lapsed` <> 'Y' ";
            cmd += " AND d.`deceasedDate` < '19101231' ";

            string what = cmbPre2002.Text;

            string Contract = txtContract.Text;
            if (String.IsNullOrWhiteSpace(Contract))
            {
                if (what.ToUpper() == "PRE2002 ONLY")
                    cmd += " AND `issueDate8` < '2002-01-01' ";
                else if (what.ToUpper() == "POST2002 ONLY")
                    cmd += " AND `issueDate8` >= '2002-01-01' ";
                if (chkPaidOff.Checked)
                    cmd += " AND `dueDate8` >= '2039-12-31' ";
                else if (chkBalance.Checked)
                    cmd += " AND `balanceDue` > '0.00' ";
            }
            else
                cmd += " AND p.`contractNumber` = '" + Contract + "' ";

            //cmd += " LIMIT 100 ";
            cmd += ";";

            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("what");
            dt.Columns.Add("smfsTrust85", Type.GetType("System.Double"));
            dt.Columns.Add("endingBalance", Type.GetType("System.Double"));
            dt.Columns.Add("currentRemovals", Type.GetType("System.Double"));


            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["what"] = what;

            G1.NumberDataTable(dt);
            dgv.DataSource = dt;

            originalDt = dt;

            btnCalc.Hide();
            chkShowDiff.Hide();
            if (dt.Rows.Count > 0)
            {
                btnCalc.Show();
                chkShowDiff.Show();
            }
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
        }
        /***********************************************************************************************/
        private void btnCalc_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;

            gridMain.Columns["smfsTrust85"].Visible = true;
            gridMain.Columns["endingBalance"].Visible = true;
            gridMain.Columns["currentRemovals"].Visible = true;

            string contractNumber = "";
            string cmd = "";
            DataTable dx = null;
            double trust85 = 0D;
            double endingBalance = 0D;
            double removals = 0D;
            double value = 0D;
            string fill = "";
            DateTime charlotteDate = DateTime.Now;

            lblTotal.Text = "";
            lblTotal.Show();
            barImport.Show();

            int lastRow = dt.Rows.Count;
            barImport.Minimum = 0;
            barImport.Maximum = lastRow;

            this.Cursor = Cursors.WaitCursor;
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                Application.DoEvents();

                barImport.Value = i + 1;
                barImport.Refresh();
                lblTotal.Text = (i + 1).ToString() + " of " + lastRow.ToString();
                lblTotal.Refresh();

                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                cmd = "Select * from `trust2013r` where `contractNumber` = '" + contractNumber + "' ORDER BY `payDate8` DESC limit 1;";
                dx = G1.get_db_data(cmd);
                if ( dx.Rows.Count > 0 )
                {
                    removals = dx.Rows[0]["currentRemovals"].ObjToDouble();
                    endingBalance = dx.Rows[0]["endingBalance"].ObjToDouble();
                    dt.Rows[i]["endingBalance"] = endingBalance;
                    if ( endingBalance <= 0D && removals > 0D)
                    {
                        dt.Rows[i]["smfsTrust85"] = endingBalance;
                        continue;
                    }

                    charlotteDate = dx.Rows[0]["payDate8"].ObjToDateTime();



                    //cmd = "Select * from `payments` where `contractNumber` = '" + contractNumber + "' AND `payDate8` <= '" + charlotteDate.ToString("yyyy-MM-dd") + "';";
                    //dx = G1.get_db_data(cmd);

                    dx = DailyHistory.GetPaymentData(contractNumber, charlotteDate, 0D);
                    trust85 = 0D;
                    for (int j = 0; j < dx.Rows.Count; j++)
                    {
                        fill = dx.Rows[j]["fill"].ObjToString();
                        if (fill.ToUpper() != "D")
                        {
                            value = Math.Round(dx.Rows[j]["trust85P"].ObjToDouble(), 2);
                            //                        value = (decimal) G1.RoundDown((double)value);
                            trust85 += value;
                        }
                    }
                    dt.Rows[i]["smfsTrust85"] = trust85.ObjToDouble();
                }
            }
            dgv.DataSource = dt;
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            string contract = "";
            DataRow dr = gridMain.GetFocusedDataRow();
            contract = dr["contractNumber"].ObjToString();
            this.Cursor = Cursors.WaitCursor;
            CustomerDetails clientForm = new CustomerDetails(contract);
            clientForm.Show();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void chkShowDiff_CheckedChanged(object sender, EventArgs e)
        {
            if (originalDt == null)
                return;
            if (chkShowDiff.Checked)
            {
                double trust85 = 0D;
                double endingBalance = 0D;
                double diff = 0D;
                DataRow[] dRows = originalDt.Select("smfsTrust85 <> endingBalance");
                if (dRows.Length > 0)
                {
                    DataTable dt = dRows.CopyToDataTable();
                    if (dt.Rows.Count > 0)
                    {
                        for (int i = (dt.Rows.Count - 1); i >= 0; i--)
                        {
                            trust85 = dt.Rows[i]["smfsTrust85"].ObjToDouble();
                            endingBalance = dt.Rows[i]["endingBalance"].ObjToDouble();
                            diff = endingBalance - trust85;
                            if (diff >= -0.02D && diff <= 0.02D)
                                dt.Rows.RemoveAt(i);
                        }
                    }
                    G1.NumberDataTable(dt);
                    dgv.DataSource = dt;
                }
                else
                {
                    DataTable dt = originalDt.Clone();
                    G1.NumberDataTable(dt);
                    dgv.DataSource = dt;
                }
            }
            else
                dgv.DataSource = originalDt;

            dgv.RefreshDataSource();
            dgv.Refresh();
        }
        /***********************************************************************************************/
    }
}