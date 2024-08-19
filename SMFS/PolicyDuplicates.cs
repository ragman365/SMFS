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
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class PolicyDuplicates : DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        private DataTable originalDt = null;
        public PolicyDuplicates()
        {
            InitializeComponent();
        }
        /***********************************************************************************************/
        private void PolicyDuplicates_Load(object sender, EventArgs e)
        {
            labelMaximum.Hide();
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

            string cmd = "Select * from `policies` ORDER BY `payer`,`policyLastName`, `policyFirstName`;";
            //string cmd = "Select * from `policies` WHERE `payer` = '221115' ORDER BY `policyLastName`, `policyFirstName`;";
            //string cmd = "Select * from `policies` WHERE `payer` = 'UC-4368' ORDER BY `policyLastName`, `policyFirstName`;";
            DataTable dt = G1.get_db_data(cmd);

            if (chkHonor.Checked)
            {
                DataTable testDt = filterSecNat(chkSecNat.Checked, dt);
                dt = testDt.Copy();
            }

            DataTable dx = dt.Clone();
            string name = "";
            string oldName = "";
            string oldPayer = "";
            string payer = "";
            bool oldDeceased = false;
            bool deceased = false;
            DateTime deceasedDate = DateTime.Now;

            int lastRow = dt.Rows.Count;
            lblTotal.Show();
            barImport.Show();

            lblTotal.Text = "of " + lastRow.ToString();
            lblTotal.Refresh();

            barImport.Minimum = 0;
            barImport.Maximum = lastRow;
            labelMaximum.Show();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    Application.DoEvents();

                    barImport.Value = i;
                    barImport.Refresh();
                    labelMaximum.Text = i.ToString();
                    labelMaximum.Refresh();

                    deceasedDate = dt.Rows[i]["deceasedDate"].ObjToDateTime();
                    deceased = false;
                    if (deceasedDate.Year > 1000)
                        deceased = true;
                    payer = dt.Rows[i]["payer"].ObjToString();
                    name = dt.Rows[i]["policyFirstName"].ObjToString() + " " + dt.Rows[i]["policyLastName"].ObjToString();
                    if ( name.ToUpper() == "JAMES E ADAMS")
                    {
                    }
                    if (i == 0)
                    {
                        oldPayer = payer;
                        oldName = name;
                        oldDeceased = deceased;
                        continue;
                    }
                    if (String.IsNullOrWhiteSpace(name))
                        continue;
                    if ( oldPayer != payer )
                    {
                        oldPayer = payer;
                        oldName = name;
                        oldDeceased = deceased;
                        continue;
                    }
                    if (oldName != name)
                    {
                        oldName = name;
                        oldDeceased = deceased;
                        continue;
                    }
                    if (deceased != oldDeceased)
                    {
                        oldDeceased = deceased;
                        dx.ImportRow(dt.Rows[i]);
                    }
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

            G1.NumberDataTable(dx);
            dgv.DataSource = dx;
            originalDt = dx;
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
            GridView View = sender as GridView;
            if (e.RowHandle >= 0)
            {
                string policyNumber = View.GetRowCellDisplayText(e.RowHandle, View.Columns["policyNumber"]);
                if (String.IsNullOrWhiteSpace ( policyNumber ))
                {
                    //e.Appearance.BackColor = Color.Salmon;
                    //e.Appearance.BackColor2 = Color.SeaShell;
                    Font f = e.Appearance.Font;
                    string name = f.Name.ObjToString();
                    Font font = new Font(name, e.Appearance.Font.Size, FontStyle.Bold);
                    e.Appearance.Font = font;
                }
            }
        }
        /***********************************************************************************************/
        private void chkHonor_CheckedChanged(object sender, EventArgs e)
        {
            btnRun_Click(null, null);
        }
        /***********************************************************************************************/
        private void chkSecNat_CheckedChanged(object sender, EventArgs e)
        {
            btnRun_Click(null, null);
        }
        /***********************************************************************************************/
    }
}