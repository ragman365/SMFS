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
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class SecNatPayments : DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        public SecNatPayments()
        {
            InitializeComponent();
        }
        /***********************************************************************************************/
        private void SecNatPayments_Load(object sender, EventArgs e)
        {
            DateTime date = new DateTime(2020, 6, 30);
            this.dateTimePicker2.Value = date;
            date = new DateTime(2020, 6, 22);
            this.dateTimePicker1.Value = date;
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
            if (!chkIncludeHeader.Checked)
                return;
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
        private void btnRun_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            DateTime date = dateTimePicker1.Value;
            string date1 = G1.DateTimeToSQLDateTime(date);
            date = dateTimePicker2.Value;
            string date2 = G1.DateTimeToSQLDateTime(date);

            DateTime dueDate8 = DateTime.Now;

            string cmd = "Select * from `policies` p LEFT OUTER JOIN `secnat` ON (p.companycode = secnat.cc) JOIN `icustomers` i ON p.`payer` = i.`payer` ;";
            DataTable dt = G1.get_db_data(cmd);
            DataTable dx = filterSecNat(true, dt);
            DataTable dp = GetGroupData(dx);

            cmd = "Select * from `ipayments` where `payDate8` >= '" + date1 + "' and `payDate8` <= '" + date2 + "';";
            DataTable ddp = G1.get_db_data(cmd);

            DataTable dd = dp.Clone();

            dd.Columns.Add("paymentAmount", Type.GetType("System.Double"));
            dd.Columns.Add("payDate8");
            dd.Columns.Add("oldPremium", Type.GetType("System.Double"));
            dd.Columns.Add("newPremium", Type.GetType("System.Double"));
            dd.Columns.Add("oldMonths", Type.GetType("System.Double"));
            dd.Columns.Add("newMonths", Type.GetType("System.Double"));
            dd.Columns.Add("dueDate8");

            string payer = "";
            double monthlyPremium = 0D;
            double historicPremium = 0D;
            double monthlySecNat = 0D;
            double monthly3rdParty = 0D;
            double newPremium = 0D;
            double oldPremium = 0D;
            double paymentAmount = 0D;
            double premium = 0D;
            double oldMonths = 0D;
            double newMonths = 0D;
            int row = 0;
            string cc = "";
            string contractNumber = "";

            DataRow[] dRows = null;

            for (int i = 0; i < ddp.Rows.Count; i++)
            {
                try
                {
                    contractNumber = ddp.Rows[i]["contractNumber"].ObjToString();
                    cmd = "Select * from `icustomers` c JOIN `icontracts` i ON c.`contractNumber` = i.`contractNumber` where c.`contractNumber` = '" + contractNumber + "';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count <= 0)
                        continue;
                    payer = dx.Rows[0]["payer"].ObjToString();
                    dRows = dp.Select("payer='" + payer + "'");
                    if (dRows.Length <= 0)
                        continue;

                    dueDate8 = dx.Rows[0]["dueDate8"].ObjToDateTime();
                    //if (dueDate8 <= DailyHistory.killSecNatDate)
                    //    continue;

                    CustomerDetails.CalcMonthlyPremium(payer, ref monthlyPremium, ref historicPremium, ref monthlySecNat, ref monthly3rdParty );

                    paymentAmount = ddp.Rows[i]["paymentAmount"].ObjToDouble();
                    if (paymentAmount == monthlyPremium)
                        continue;

                    dd.ImportRow(dRows[0]);
                    row = dd.Rows.Count - 1;
                    dd.Rows[row]["oldPremium"] = monthlyPremium;
                    newPremium = monthlyPremium - monthlySecNat;
                    dd.Rows[row]["newPremium"] = monthlyPremium - monthlySecNat;
                    dd.Rows[row]["payDate8"] = ddp.Rows[i]["payDate8"].ObjToDateTime().ToString("MM/dd/yyyy");
                    dd.Rows[row]["paymentAmount"] = paymentAmount;
                    dd.Rows[row]["dueDate8"] = dueDate8.ToString("MM/dd/yyyy");

                    oldMonths = paymentAmount / monthlyPremium;
                    dd.Rows[row]["oldMonths"] = oldMonths;

                    newMonths = 0D;
                    if (newPremium > 0D)
                        newMonths = paymentAmount / newPremium;
                    dd.Rows[row]["newMonths"] = newMonths;
                }
                catch ( Exception ex)
                {
                }
            }

            for (int i = (dd.Rows.Count - 1); i >= 0; i--)
            {
                newPremium = dd.Rows[i]["newPremium"].ObjToDouble();
                oldPremium = dd.Rows[i]["oldPremium"].ObjToDouble();
                if (newPremium <= 0D )
                    dd.Rows.RemoveAt(i);
            }


            G1.NumberDataTable(dd);
            dgv.DataSource = dd;
            this.Cursor = Cursors.Default;
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
            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv.DataSource;
            double newPremium = dt.Rows[row]["newPremium"].ObjToDouble();
            if (newPremium == 0D )
            {
                e.Visible = false;
                e.Handled = true;
                return;
            }
        }
        /***********************************************************************************************/
        private void fixAsIfNewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);

            string contractNumber = dr["contractNumber"].ObjToString();
            string payer = dr["payer"].ObjToString();
            DateTime date = dr["payDate8"].ObjToDateTime();
            double newMonths = dr["newMonths"].ObjToDouble();
            double newPremium = dr["newPremium"].ObjToDouble();

            string cmd = "Select * from `ipayments` where `contractNumber` = '" + contractNumber + "' AND `payDate8` = '" + date.ToString("yyyy-MM-dd") + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
            {
                MessageBox.Show("***ERROR*** Payer " + payer + " Payment Could Not Be Found!", "Early Sec Nat Payment Dialog", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            DateTime dueDate8 = dx.Rows[0]["dueDate8"].ObjToDateTime();
            DateTime newDueDate = dueDate8.AddMonths(Convert.ToInt32(newMonths));

            string record = dx.Rows[0]["record"].ObjToString();
            G1.update_db_table("ipayments", "record", record, new string[] { "numMonthPaid", newMonths.ToString()});

            cmd = "Select * from `icontracts` where `contractNumber` = '" + contractNumber + "';";
            dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
            {
                MessageBox.Show("***ERROR*** Payer " + payer + " Contract Could Not Be Found!", "Early Sec Nat Payment Dialog", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            record = dt.Rows[0]["record"].ObjToString();

            G1.update_db_table("icontracts", "record", record, new string[] {"dueDate8", newDueDate.ToString("yyyy-MM-dd"), "creditBalance", "0.00" });
            DialogResult result = MessageBox.Show("Payer " + payer + " Payment Has Been Fixed!", "Early Sec Nat Payment Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        /***********************************************************************************************/
    }
}