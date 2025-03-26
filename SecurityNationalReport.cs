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
    public partial class SecurityNationalReport : DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        private DataTable originalDt = null;
        public SecurityNationalReport()
        {
            InitializeComponent();
        }
        /***********************************************************************************************/
        private void SecurityNationalReport_Load(object sender, EventArgs e)
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
            string cmd = "Select * from `policies` p LEFT OUTER JOIN `secnat` ON (p.companycode = secnat.cc) JOIN `icustomers` i ON p.`payer` = i.`payer`;";
            DataTable dt = G1.get_db_data(cmd);
            DataTable dx = filterSecNat(true, dt);
            DataTable dp = GetGroupData(dx);

            DataView tempview = dp.DefaultView;
            tempview.Sort = "payer asc, lastName asc, firstName asc";
            dp = tempview.ToTable();


            DataTable dd = dp.Clone();

            DataTable dj = null;
            string agent = "";
            string sdiCode = "";
            string oldloc = "";

            dd.Columns.Add("oldPremium", Type.GetType("System.Double"));
            dd.Columns.Add("newPremium", Type.GetType("System.Double"));
            dd.Columns.Add("sdiCode");
            dd.Columns.Add("location");
            dd.Columns.Add("sortcode", Type.GetType("System.Int32"));
            dd.Columns.Add("sortedby");

            string payer = "";
            double monthlyPremium = 0D;
            double historicPremium = 0D;
            double monthlySecNat = 0D;
            double monthly3rdParty = 0D;
            double newPremium = 0D;
            double oldPremium = 0D;
            double premium = 0D;
            string oldPayer = "";
            string location = "";
            string lastName = "";
            string firstName = "";
            int row = 0;
            string cc = "";
            DateTime deceasedDate = DateTime.Now;

            DataRow[] dRows = null;

            int lastRow = dp.Rows.Count;
            //lastRow = 2000;
            //if (dp.Rows.Count < 2000)
            //    lastRow = dp.Rows.Count;

            lblTotal.Show();
            barImport.Show();

            lblTotal.Text = "of " + lastRow.ToString();
            lblTotal.Refresh();

            barImport.Minimum = 0;
            barImport.Maximum = lastRow;
            labelMaximum.Show();
            int sortCode = 0;

            string mainSort = "";


            for (int i = 0; i < lastRow; i++)
            {
                try
                {
                    barImport.Value = i;
                    barImport.Refresh();
                    labelMaximum.Text = i.ToString();
                    labelMaximum.Refresh();

                    deceasedDate = dp.Rows[i]["deceasedDate"].ObjToDateTime();
                    if (deceasedDate.Year > 100)
                        continue;
                    payer = dp.Rows[i]["payer"].ObjToString();
                    oldloc = dp.Rows[i]["oldloc"].ObjToString();
                    if ( payer == "BB-4103")
                    {
                    }

                    sortCode++;

                    location = ImportDailyDeposits.FindLastPaymentLocation(payer, ref oldloc);

                    CustomerDetails.CalcMonthlyPremium(payer, ref monthlyPremium, ref historicPremium, ref monthlySecNat, ref monthly3rdParty);
                    dd.ImportRow(dp.Rows[i]);
                    row = dd.Rows.Count - 1;
                    dd.Rows[row]["oldPremium"] = monthlyPremium;
                    dd.Rows[row]["newPremium"] = monthlyPremium - monthlySecNat;
                    dd.Rows[row]["policyNumber"] = "";
                    dd.Rows[row]["location"] = location;
                    dd.Rows[row]["sortcode"] = sortCode;
                    dd.Rows[row]["liability"] = 0D;

                    if (!String.IsNullOrWhiteSpace(oldloc))
                    {
                        agent = dp.Rows[i]["agentCode"].ObjToString();
                        sdiCode = InsuranceCoupons.getSDICode(agent, oldloc);
                        if (!String.IsNullOrWhiteSpace(sdiCode) && String.IsNullOrWhiteSpace(location))
                        {
                            location = sdiCode;
                            dd.Rows[row]["location"] = location;
                        }
                    }

                    if (String.IsNullOrWhiteSpace(location))
                        dd.Rows[row]["location"] = "05";
                    location = dd.Rows[row]["location"].ObjToString();
                    location = getLocationText(location);
                    dd.Rows[row]["location"] = location;

                    lastName = dd.Rows[row]["lastName"].ObjToString();
                    firstName = dd.Rows[row]["firstName"].ObjToString();
                    mainSort = location + "~" + lastName + "~" + firstName;
                    dd.Rows[row]["sortedby"] = mainSort + "~" + sortCode.ToString("D09");
                    sortCode++;

                    cmd = "Select * from `policies` where `payer` = '" + payer + "' AND `deceasedDate` < '100-01-01';";
                    dj = G1.get_db_data(cmd);
                    if (dj.Rows.Count <= 0)
                        return;
                    for (int k = 0; k < dj.Rows.Count; k++)
                    {
                        sdiCode = "";
                        if (!String.IsNullOrWhiteSpace(oldloc))
                        {
                            agent = dj.Rows[k]["agentCode"].ObjToString();
                            sdiCode = InsuranceCoupons.getSDICode(agent, oldloc);
                        }
                        if (String.IsNullOrWhiteSpace(sdiCode))
                        {
                            if (!String.IsNullOrWhiteSpace(location))
                                sdiCode = location;
                            else
                            {
                                sdiCode = "05";
                                location = sdiCode;
                            }
                        }

                        if (k == 0)
                            dd.Rows[row]["sdiCode"] = sdiCode;
                        premium = dj.Rows[k]["premium"].ObjToDouble();
                        cc = dj.Rows[k]["companyCode"].ObjToString();

                        dd.ImportRow(dj.Rows[k]);
                        row = dd.Rows.Count - 1;
                        dd.Rows[row]["sortcode"] = sortCode;
                        //sortCode++;

                        lastName = dd.Rows[row]["policyLastName"].ObjToString();
                        firstName = dd.Rows[row]["policyFirstName"].ObjToString();
                        dd.Rows[row]["sortedby"] = mainSort + "~" + sortCode.ToString("D09");
                        //dd.Rows[row]["payer"] = dd.Rows[row]["payer"].ObjToString() + "~" + sortCode.ToString("D04");
//                        dd.Rows[row]["sortedby"] = location + "~" + lastName + "~" + firstName;


                        dd.Rows[row]["sdiCode"] = sdiCode;
                        dd.Rows[row]["location"] = location;
                        dd.Rows[row]["firstName"] = "   " + dd.Rows[row]["policyFirstName"].ObjToString();
                        dd.Rows[row]["lastName"] = "   " + dd.Rows[row]["policyLastName"].ObjToString();
                        if (String.IsNullOrWhiteSpace(cc))
                        {
                            dd.Rows[row]["oldPremium"] = premium;
                            dd.Rows[row]["newPremium"] = premium;
                            continue;
                        }
                        dRows = secNatDt.Select("cc='" + cc + "'");
                        if (dRows.Length > 0)
                        {
                            dd.Rows[row]["oldPremium"] = premium;
                            dd.Rows[row]["newPremium"] = 0D;
                        }
                        else
                        {
                            dd.Rows[row]["oldPremium"] = premium;
                            dd.Rows[row]["newPremium"] = premium;
                        }
                    }
                }
                catch (Exception ex)
                {
                }
            }

            //for (int i = (dd.Rows.Count - 1); i >= 0; i--)
            //{
            //    newPremium = dd.Rows[i]["newPremium"].ObjToDouble();
            //    oldPremium = dd.Rows[i]["oldPremium"].ObjToDouble();
            //    if (newPremium <= 0D && oldPremium <= 0D )
            //        dd.Rows.RemoveAt(i);
            //}

            barImport.Value = lastRow;
            barImport.Refresh();
            labelMaximum.Text = lastRow.ToString();
            labelMaximum.Refresh();

            G1.NumberDataTable(dd);
            dgv.DataSource = dd;
            originalDt = dd;
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
        private void chkSDI_CheckedChanged(object sender, EventArgs e)
        {
            if ( chkLocation.Checked )
                chkLocation.Checked = false;
            if (chkSDI.Checked)
            {
                gridMain.Columns["sdiCode"].GroupIndex = 0;
                gridMain.ExpandAllGroups();
            }
            else
            {
                gridMain.Columns["sdiCode"].GroupIndex = -1;
            }
            gridMain.RefreshData();
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
        private void chkLocation_CheckedChanged(object sender, EventArgs e)
        {
            if (chkSDI.Checked)
                chkSDI.Checked = false;
            if (chkLocation.Checked)
            {
                DataTable dt = (DataTable)dgv.DataSource;

                DataView tempview = dt.DefaultView;
                tempview.Sort = "sortedby asc";
                dt = tempview.ToTable();
                dgv.DataSource = dt;


                gridMain.Columns["location"].GroupIndex = 0;
                gridMain.Columns["num"].Visible = false;
                gridMain.Columns["contractNumber"].Visible = false;

                gridMain.ExpandAllGroups();
            }
            else
            {
                dgv.DataSource = originalDt;
                gridMain.Columns["location"].GroupIndex = -1;
                gridMain.Columns["num"].Visible = true;
                gridMain.Columns["contractNumber"].Visible = true;
            }
            gridMain.RefreshData();
        }
        /***********************************************************************************************/
    }
}