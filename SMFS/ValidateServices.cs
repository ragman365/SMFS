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
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class ValidateServices : DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        public ValidateServices()
        {
            InitializeComponent();
        }
        /***********************************************************************************************/
        private void ValidateServices_Load(object sender, EventArgs e)
        {
        }
        /***********************************************************************************************/
        private void btnRun_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            string cmd = "Select * from `cust_services` c LEFT JOIN `customers` x ON c.`contractNumber` = x.`contractNumber` ";
            cmd += " GROUP BY c.`contractNumber` ;";
            DataTable dt = G1.get_db_data(cmd);

            DataView tempview = dt.DefaultView;
            tempview.Sort = "contractNumber";
            dt = tempview.ToTable();

            cmd = "Select * from `trust2013r` ORDER BY `payDate8` DESC LIMIT 1;";
            DataTable dx = G1.get_db_data(cmd);

            DateTime lastDate = dx.Rows[0]["payDate8"].ObjToDateTime();

            string date = lastDate.ToString("yyyy-MM-dd");

            this.Cursor = Cursors.WaitCursor;

            cmd = "Select * from `trust2013r` where `payDate8` = '" + date + "' ORDER BY `contractNumber`;";
            dx = G1.get_db_data(cmd);

            string what = cmbCheck.Text;
            this.Text = what;
            if (what == "Check Trusts are in Beginning Balances")
                CheckTrusts(dt, dx);
            else
                CheckBeginning(dt, dx);
            this.Cursor = Cursors.Default;

        }
        /***********************************************************************************************/
        private void CheckTrusts ( DataTable dt, DataTable dx )
        {
            string contractNumber = "";
            DataRow[] dRows = null;
            DataRow dR = null;

            DataTable newDt = new DataTable();
            newDt.Columns.Add("contractNumber");
            newDt.Columns.Add("name");

            int found = 0;
            int notFound = 0;

            DateTime issueDate8 = DateTime.Now;
            DateTime deceasedDate = DateTime.Now;

            string cmd = "Select * from `contracts`;";
            DataTable dd = G1.get_db_data(cmd);

            this.Cursor = Cursors.WaitCursor;

            for ( int i=0; i<dt.Rows.Count; i++)
            {
                try
                {
                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                    if (String.IsNullOrWhiteSpace(contractNumber))
                        continue;
                    dRows = dx.Select("contractNumber='" + contractNumber + "'");
                    if (dRows.Length > 0)
                    {
                        found++;
                        continue;
                    }

                    dRows = dd.Select("contractNumber='" + contractNumber + "'");
                    if (dRows.Length == 0)
                        continue;
                    if (contractNumber.IndexOf("AFA") == 0)
                        continue;
                    issueDate8 = dRows[0]["issueDate8"].ObjToDateTime();
                    if (issueDate8.Year < 1000)
                        continue;
                    deceasedDate = dRows[0]["deceasedDate"].ObjToDateTime();
                    if (deceasedDate.Year > 1000)
                        continue;
                    dR = newDt.NewRow();
                    dR["contractNumber"] = contractNumber;
                    dR["name"] = dt.Rows[i]["lastName"].ObjToString() + ", " + dt.Rows[i]["firstName"].ObjToString();
                    newDt.Rows.Add(dR);
                    notFound++;
                }
                catch (Exception ex)
                {
                }
            }
            MessageBox.Show("***INFO*** Found " + found.ToString() + " Matches and " + notFound.ToString() + " Not in Beginning Balances File", "Match Services Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information);
            G1.NumberDataTable(newDt);
            dgv.DataSource = newDt;
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void CheckBeginning(DataTable dt, DataTable dx)
        {
            string contractNumber = "";
            DataRow[] dRows = null;
            DataRow dR = null;

            DataTable newDt = new DataTable();
            newDt.Columns.Add("contractNumber");
            newDt.Columns.Add("name");

            int found = 0;
            int notFound = 0;

            this.Cursor = Cursors.WaitCursor;

            for (int i = 0; i < dx.Rows.Count; i++)
            {
                try
                {
                    contractNumber = dx.Rows[i]["contractNumber"].ObjToString();
                    if (String.IsNullOrWhiteSpace(contractNumber))
                        continue;
                    dRows = dt.Select("contractNumber='" + contractNumber + "'");
                    if (dRows.Length > 0)
                    {
                        found++;
                        continue;
                    }
                    dR = newDt.NewRow();
                    dR["contractNumber"] = contractNumber;
                    dR["name"] = dx.Rows[i]["lastName"].ObjToString() + ", " + dx.Rows[i]["firstName"].ObjToString();
                    newDt.Rows.Add(dR);
                    notFound++;
                }
                catch (Exception ex)
                {
                }
            }
            MessageBox.Show("***INFO*** Found " + found.ToString() + " Matches and " + notFound.ToString() + " Not in SMFS Trust Services File", "Match Beginning Balances Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information);
            G1.NumberDataTable(newDt);
            dgv.DataSource = newDt;
            this.Cursor = Cursors.Default;
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