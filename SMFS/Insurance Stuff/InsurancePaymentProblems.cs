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
using DevExpress.Charts.Native;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.Data;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using java.awt.print;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class InsurancePaymentProblems : DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        private DataTable originalDt = null;
        private bool loading = true;
        public InsurancePaymentProblems()
        {
            InitializeComponent();
        }
        /***********************************************************************************************/
        private void InsurancePaymentProblems_Load(object sender, EventArgs e)
        {
            labelMaximum.Hide();
            lblTotal.Hide();
            barImport.Hide();

            chkSecNat.Hide();
            chkHonor.Hide();

            DateTime now = DateTime.Now;
            now = now.AddMonths(-1);
            now = new DateTime(now.Year, now.Month, 1);
            this.dateTimePicker1.Value = now;
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            this.dateTimePicker2.Value = new DateTime(now.Year, now.Month, days);
            loading = false;
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
            mainPrintRow = 0;

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

            printingSystem1.Document.AutoFitToPagesWidth = 1;

            G1.PrintPreview(printableComponentLink1, gridMain);

            //printableComponentLink1.CreateDocument();
            //printableComponentLink1.ShowPreview();

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
            mainPrintRow = 0;

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

            printingSystem1.Document.AutoFitToPagesWidth = 1;

            printableComponentLink1.CreateDocument();
            printableComponentLink1.PrintDlg();
            isPrinting = false;
        }
        /***********************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string contract = dr["contractNumber"].ObjToString();
            if (!String.IsNullOrWhiteSpace(contract))
            {
                this.Cursor = Cursors.WaitCursor;
                CustomerDetails clientForm = new CustomerDetails(contract);
                clientForm.Show();
                this.Cursor = Cursors.Default;
            }
        }
        /****************************************************************************************/
        private DataTable secNatDt = null;
        private DataTable filterSecNat(bool include, DataTable dt)
        {
            if (secNatDt == null)
                secNatDt = G1.get_db_data("Select * from `secnat`;");

            DataTable newDt = dt.Clone();
            try
            {
                if (!include)
                {
                    var result = dt.AsEnumerable()
                           .Where(row => !secNatDt.AsEnumerable()
                                                 .Select(r => r.Field<string>("cc"))
                                                 .Any(x => x == row.Field<string>("companyCode"))
                          ).CopyToDataTable();
                    newDt = result.Copy();
                }
                else
                {
                    var result = dt.AsEnumerable()
                           .Where(row => secNatDt.AsEnumerable()
                                                 .Select(r => r.Field<string>("cc"))
                                                 .Any(x => x == row.Field<string>("companyCode"))
                          ).CopyToDataTable();
                    newDt = result.Copy();
                }
            }
            catch (Exception ex)
            {
            }
            return newDt;
        }
        /***********************************************************************************************/
        private DataTable funDt = null;
        private string getLocationText ( string location)
        {
            if (funDt == null)
                funDt = G1.get_db_data("Select * from `funeralhomes`;");

            if (location == "0")
                location = "05";

            DataRow[] dRows = funDt.Select("SDICode='" + location + "'");
            if (dRows.Length > 0)
                location = dRows[0]["LocationCode"].ObjToString();
            return location;
        }
        /***********************************************************************************************/
        private void btnRun_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            DateTime date = this.dateTimePicker1.Value;
            string date1 = date.ToString("yyyy-MM-dd");
            date = this.dateTimePicker2.Value;
            string date2 = date.ToString("yyyy-MM-dd");

            string cmd = "Select * from `ipayments` p LEFT JOIN `icustomers` c ON p.`contractNumber` = c.`contractNumber` ";
            cmd += " where `payDate8` >= '" + date1 + "' AND `payDate8` <= '" + date2 + "' ";
            cmd += " ORDER BY p.`contractNumber`,`payDate8`;";
            //cmd = "Select * from `ipayments` where `dueDate8` >= '2020-06-01' AND `payer` = 'CC-5633' ORDER BY `payer`,`contractNumber`;";
            DataTable dt = G1.get_db_data(cmd);

            dt.Columns.Add("premium", Type.GetType("System.Double"));
            dt.Columns.Add("secNatPremium", Type.GetType("System.Double"));
            dt.Columns.Add("thirdPartyPremium", Type.GetType("System.Double"));

            //if (chkHonor.Checked)
            //{
            //    DataTable testDt = filterSecNat(chkSecNat.Checked, dt);
            //    dt = testDt.Copy();
            //}

            DataTable dx = dt.Clone();
            string name = "";
            string oldName = "";
            string oldPayer = "";
            string payer = "";
            bool oldDeceased = false;
            bool deceased = false;
            DateTime deceasedDate = DateTime.Now;

            string contractNumber = "";
            string oldContractNumber = "";
            DateTime dueDate8 = DateTime.Now;
            DateTime oldDueDate8 = DateTime.Now;
            double monthlyPremium = 0D;
            double historicPremium = 0D;
            double monthlySecNat = 0D;
            double monthly3rdParty = 0D;
            double premium = 0D;

            bool first = true;

            DataRow dR = null;
            DataTable dxx = null;

            int lastRow = dt.Rows.Count;
            lblTotal.Show();
            barImport.Show();

            lblTotal.Text = "of " + lastRow.ToString();
            lblTotal.Refresh();

            barImport.Minimum = 0;
            barImport.Maximum = lastRow;
            barImport.Value = 0;
            labelMaximum.Show();
            loading = true;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    Application.DoEvents();

                    barImport.Value = i;
                    barImport.Refresh();
                    labelMaximum.Text = i.ToString();
                    labelMaximum.Refresh();

                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                    //if ( contractNumber == "ZZ0000004")
                    //{
                    //}
                    payer = dt.Rows[i]["payer"].ObjToString();
                    CustomerDetails.CalcMonthlyPremium(payer, ref monthlyPremium, ref historicPremium, ref monthlySecNat, ref monthly3rdParty);

                    premium = monthlyPremium - monthlySecNat - monthly3rdParty;
                    premium = G1.RoundValue(premium);

                    dt.Rows[i]["premium"] = premium;
                    dt.Rows[i]["secNatPremium"] = monthlySecNat;
                    dt.Rows[i]["thirdPartyPremium"] = monthly3rdParty;

                    //dueDate8 = dt.Rows[i]["dueDate8"].ObjToDateTime();
                    //if ( String.IsNullOrWhiteSpace ( oldContractNumber))
                    //{
                    //    oldContractNumber = contractNumber;
                    //    oldDueDate8 = dueDate8;
                    //    first = true;
                    //    continue;
                    //}
                    //if ( oldContractNumber != contractNumber )
                    //{
                    //    oldContractNumber = contractNumber;
                    //    oldDueDate8 = dueDate8;
                    //    first = false;
                    //    continue;
                    //}
                    //if (dueDate8 != oldDueDate8)
                    //{
                    //    oldDueDate8 = dueDate8;
                    //    continue;
                    //}
                    //else
                    //{
                    //    if ( String.IsNullOrWhiteSpace ( payer))
                    //    {
                    //        cmd = "Select * from `icustomers` where `contractNumber` = '" + contractNumber + "';";
                    //        dxx = G1.get_db_data(cmd);
                    //        if (dxx.Rows.Count > 0)
                    //            payer = dxx.Rows[0]["payer"].ObjToString();
                    //    }
                    //    dR = dx.NewRow();
                    //    dR["payer"] = payer;
                    //    dR["contractNumber"] = contractNumber;
                    //    dR["firstName"] = dt.Rows[i]["firstName"].ObjToString();
                    //    dR["lastName"] = dt.Rows[i]["lastName"].ObjToString();
                    //    dx.Rows.Add(dR);
                    //    continue;
                    //}
                }
                catch ( Exception ex)
                {
                    MessageBox.Show("***ERROR*** " + ex.Message.ToString());
                }
            }

            barImport.Value = lastRow;
            barImport.Refresh();
            labelMaximum.Text = lastRow.ToString();
            labelMaximum.Refresh();

            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            originalDt = dt;
            this.Cursor = Cursors.Default;
            loading = false;
        }
        /***********************************************************************************************/
        private DataTable GetGroupData(DataTable dt)
        {
            if (dt.Rows.Count <= 0)
                return dt;

            DataTable groupDt = dt.AsEnumerable().GroupBy(r => new { Col1 = r["payer"] }).Select(g => g.OrderBy(r => r["payer"]).First()).CopyToDataTable();
            return groupDt;
        }
        /***********************************************************************************************/
        private void gridMain_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column.FieldName.ToUpper().IndexOf("NUM") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                e.DisplayText = e.GroupRowHandle.ObjToString();
            }
        }
        /***********************************************************************************************/
        private void gridMain_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            //int row = e.ListSourceRow;
            //DataTable dt = (DataTable)dgv.DataSource;
            //double newPremium = dt.Rows[row]["newPremium"].ObjToDouble();
            //double oldPremium = dt.Rows[row]["oldPremium"].ObjToDouble();
            //if (newPremium == 0D && oldPremium == 0D )
            //{
            //    e.Visible = false;
            //    e.Handled = true;
            //    return;
            //}
        }
        /***********************************************************************************************/
        private void gridMain_RowCellStyle(object sender, DevExpress.XtraGrid.Views.Grid.RowCellStyleEventArgs e)
        {
            //GridView View = sender as GridView;
            //if (e.RowHandle >= 0)
            //{
            //    double payment = View.GetRowCellDisplayText(e.RowHandle, View.Columns["paymentAmount"]).ObjToDouble();
            //    double expected = View.GetRowCellDisplayText(e.RowHandle, View.Columns["premium"]).ObjToDouble();
            //    double secNat = View.GetRowCellDisplayText(e.RowHandle, View.Columns["secNatPremium"]).ObjToDouble();
            //    double thirdParty = View.GetRowCellDisplayText(e.RowHandle, View.Columns["thirdPartyPremium"]).ObjToDouble();

            //    double total = expected + secNat;
            //    if ( payment == total )
            //    {
            //        e.Appearance.BackColor = Color.Red;
            //    }
            //}
        }
        /***********************************************************************************************/
        private void chkHonor_CheckedChanged(object sender, EventArgs e)
        {
            //btnRun_Click(null, null);
        }
        /***********************************************************************************************/
        private void chkSecNat_CheckedChanged(object sender, EventArgs e)
        {
            //btnRun_Click(null, null);
        }
        /***********************************************************************************************/
        private void btnLeft_Click(object sender, EventArgs e)
        {
            DateTime date = this.dateTimePicker1.Value;
            date = date.AddMonths(-1);
            this.dateTimePicker1.Value = date;
            this.dateTimePicker1.Refresh();

            date = this.dateTimePicker2.Value;
            date = date.AddMonths(-1);
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            this.dateTimePicker2.Value = new DateTime(date.Year, date.Month, days);
            this.dateTimePicker2.Refresh();
        }
        /***********************************************************************************************/
        private void btnRight_Click(object sender, EventArgs e)
        {
            DateTime date = this.dateTimePicker1.Value;
            date = date.AddMonths(1);
            this.dateTimePicker1.Value = date;
            this.dateTimePicker1.Refresh();

            date = this.dateTimePicker2.Value;
            date = date.AddMonths(1);
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            this.dateTimePicker2.Value = new DateTime(date.Year, date.Month, days);
            this.dateTimePicker2.Refresh();
        }
        /***********************************************************************************************/
        private void gridMain_ColumnFilterChanged(object sender, EventArgs e)
        {
            //DataTable dt = (DataTable)dgv.DataSource;
            //int count = dt.Rows.Count;
            //gridMain.SelectAll();
            //int[] rows = gridMain.GetSelectedRows();
            //int row = 0;
            //for (int i = 0; i < rows.Length; i++)
            //{
            //    row = rows[i];
            //    var dRow = gridMain.GetDataRow(row);
            //    if (dRow != null)
            //        dRow["num"] = (i + 1).ToString();
            //}
            //gridMain.ClearSelection();
        }
        /***********************************************************************************************/
        private void RenumberRows ()
        {

            //int row = 0;

            //DataTable dt = (DataTable)dgv.DataSource;
            //string num = "";
            //DataRow dRow = null;
            //int iRow = 0;
            //for (int i = 0; i != gridMain.RowCount; i++)
            //{
            //    if (gridMain.IsRowVisible(i) == RowVisibleState.Visible)
            //    {
            //        iRow = gridMain.GetVisibleRowHandle(i);
            //        iRow = gridMain.GetDataSourceRowIndex(iRow);

            //        dt.Rows[iRow]["num"] = (row + 1).ToString();
            //        row++;
            //    }
            //    else
            //    {
            //        num = dt.Rows[i]["num"].ObjToString();
            //    }
            //}
            //dgv.DataSource = dt;

            //row = 0;
            //int count = dt.Rows.Count;
            //gridMain.SelectAll();
            //int[] rows = gridMain.GetSelectedRows();
            //for (int i = 0; i < rows.Length; i++)
            //{
            //    row = rows[i];
            //    var dRow = gridMain.GetDataRow(row);
            //    if (dRow != null)
            //        dRow["num"] = (i + 1).ToString();
            //}
            //gridMain.ClearSelection();
        }
        /***********************************************************************************************/
        private void gridMain_CustomRowFilter_1(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv.DataSource;
            double secNatPremium= dt.Rows[row]["secNatPremium"].ObjToDouble();
            double thirdPartyPremium = dt.Rows[row]["thirdPartyPremium"].ObjToDouble();
            if (secNatPremium == 0D && thirdPartyPremium == 0D)
            {
                e.Visible = false;
                e.Handled = true;
                return;
            }
            double payment = dt.Rows[row]["paymentAmount"].ObjToDouble();
            double expected = dt.Rows[row]["premium"].ObjToDouble();
            if ( payment == expected)
            {
                e.Visible = false;
                e.Handled = true;
                return;
            }
            //double difference = payment / expected;
            //difference = G1.RoundValue(difference);
            //difference = difference % 1D;
            //difference = G1.RoundValue(difference);
            //if ( difference == 0D)
            //{
            //    e.Visible = false;
            //    e.Handled = true;
            //    return;
            //}
        }
        /***********************************************************************************************/
        private void gridMain_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
        {
            if (loading)
                return;
            if (e.Column.FieldName.ToUpper() == "NUM")
            {
                if (e.RowHandle >= 0)
                {
                    string num = (e.RowHandle + 1).ToString();
                    e.DisplayText = num;
                    //int row = gridMain.GetDataSourceRowIndex(e.RowHandle);
                    //DataTable dt = (DataTable)dgv.DataSource;
                    //dt.Rows[row]["num"] = num;
                }
            }
        }
        /***********************************************************************************************/
        private int mainPrintRow = 0;
        private void gridMain_BeforePrintRow(object sender, DevExpress.XtraGrid.Views.Printing.CancelPrintRowEventArgs e)
        {
            int row = e.RowHandle;
            if (row < 0)
                return;
            row = gridMain.GetDataSourceRowIndex(row);
            DataTable dt = (DataTable)dgv.DataSource;
            dt.Rows[row]["num"] = (mainPrintRow + 1).ToString();
            mainPrintRow++;
        }
        /***********************************************************************************************/
    }
}