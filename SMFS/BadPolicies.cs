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
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class BadPolicies : DevExpress.XtraEditors.XtraForm
    {
        private bool modified = false;
        private bool loading = true;
        private DataTable workDt = null;
        /****************************************************************************************/
        public BadPolicies( DataTable dt )
        {
            InitializeComponent();
            workDt = dt;
        }
        /****************************************************************************************/
        private void BadPolicies_Load(object sender, EventArgs e)
        {
            this.Text = "View Policy Differences";
            workDt.Columns.Add("num");
            workDt.Columns.Add("mod");
            G1.NumberDataTable(workDt);
            dgv.DataSource = workDt;

            loading = false;
        }
        /****************************************************************************************/
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (loading)
                return;
            modified = true;
            DataRow dr = gridMain.GetFocusedDataRow();
            dr["mod"] = "Y";
        }
        /****************************************************************************************/
        private void gridMain_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt == null)
                return;
            string delete = dt.Rows[row]["mod"].ObjToString();
            if (delete.ToUpper() == "D")
            {
                e.Visible = false;
                e.Handled = true;
            }
        }
        /****************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        { // Spy Glass
            G1.SpyGlass(gridMain);
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
            }
        }
        /****************************************************************************************/
        private void EditTable_FormClosing(object sender, FormClosingEventArgs e)
        {
        }
        /****************************************************************************************/
        private void gridMain_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
        }
        /****************************************************************************************/
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        /***********************************************************************************************/
        private int pageMarginLeft = 0;
        private int pageMarginRight = 0;
        private int pageMarginTop = 0;
        private int pageMarginBottom = 0;
        private int printCount = 0;
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

            //Printer.setupPrinterMargins(50, 100, 110, 50);

            Printer.setupPrinterMargins(30, 30, 90, 50);


            pageMarginLeft = Printer.pageMarginLeft;
            pageMarginRight = Printer.pageMarginRight;
            pageMarginTop = Printer.pageMarginTop;
            pageMarginBottom = Printer.pageMarginBottom;


            printableComponentLink1.Margins.Left = pageMarginLeft;
            printableComponentLink1.Margins.Right = pageMarginRight;
            printableComponentLink1.Margins.Top = pageMarginTop;
            printableComponentLink1.Margins.Bottom = pageMarginBottom;

            printingSystem1.Document.AutoFitToPagesWidth = 1;

            G1.AdjustColumnWidths(gridMain, 0.65D, true);

            printableComponentLink1.CreateDocument();
            printableComponentLink1.ShowPreview();

            G1.AdjustColumnWidths(gridMain, 0.65D, false);
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

            //Printer.setupPrinterMargins(50, 100, 110, 50);
            Printer.setupPrinterMargins(30, 30, 90, 50);

            pageMarginLeft = Printer.pageMarginLeft;
            pageMarginRight = Printer.pageMarginRight;
            pageMarginTop = Printer.pageMarginTop;
            pageMarginBottom = Printer.pageMarginBottom;

            printableComponentLink1.Margins.Left = pageMarginLeft;
            printableComponentLink1.Margins.Right = pageMarginRight;
            printableComponentLink1.Margins.Top = pageMarginTop;
            printableComponentLink1.Margins.Bottom = pageMarginBottom;

            printCount = 0;

            printableComponentLink1.CreateDocument();
            if (LoginForm.doLapseReport)
                printableComponentLink1.Print();
            else
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

            font = new Font("Ariel", 6);
            Printer.DrawGridDate(2, 3, 2, 3, Color.Black, BorderSide.None, font);
            Printer.DrawGridPage(11, 3, 2, 3, Color.Black, BorderSide.None, font);

            //            Printer.DrawQuad(1, 9, 2, 3, "User : " + LoginForm.username, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);

            font = new Font("Ariel", 12, FontStyle.Regular);
            string title = this.Text;
            int startX = 6;
            Printer.DrawQuad(startX, 8, 9, 4, title, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);


            //            Printer.DrawQuadTicks();
            Printer.SetQuadSize(24, 12);
            font = new Font("Ariel", 7, FontStyle.Regular);
            //Printer.DrawQuad(16, 7, 5, 2, lblBalance.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Top);
            //Printer.DrawQuad(16, 10, 5, 2, lblPayment.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            font = new Font("Ariel", 8);
            //            Printer.DrawQuad(1, 6, 6, 3, search, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Top);
            Printer.DrawQuad(1, 9, 6, 3, "User : " + LoginForm.username, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);


            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /****************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataRow dR = gridMain.GetFocusedDataRow();
            string InGreen = dR["InGreen"].ObjToString();
            string InOrange = dR["InOrange"].ObjToString();

            string payer = InGreen;
            if (String.IsNullOrWhiteSpace(payer))
                payer = InOrange;
            if (String.IsNullOrWhiteSpace(payer))
                return;

            DataTable dd = new DataTable();
            dd.Columns.Add("what");
            dd.Columns.Add("payer");
            dd.Columns.Add("contractNumber");
            dd.Columns.Add("policyNumber");
            dd.Columns.Add("deceasedDate");
            dd.Columns.Add("lapsedDate");
            dd.Columns.Add("lapsed");

            string contracts = "";
            string contractNumber = "";

            DataRow dr = null;
            string cmd = "";
            DataTable dx = null;

            try
            {
                cmd = "Select * from `payers` where `payer` = '" + payer + "';";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    for (int i = 0; i < dx.Rows.Count; i++)
                    {
                        dr = dd.NewRow();
                        dr["what"] = "Payer";
                        dr["payer"] = dx.Rows[i]["payer"].ObjToString();
                        contractNumber = dx.Rows[i]["contractNumber"].ObjToString();
                        dr["contractNumber"] = contractNumber;
                        dr["deceasedDate"] = dx.Rows[i]["deceasedDate"].ObjToDateTime().ToString("MM/dd/yyyy");
                        dr["lapsedDate"] = dx.Rows[i]["lapseDate8"].ObjToDateTime().ToString("MM/dd/yyyy");
                        dr["lapsed"] = dx.Rows[i]["lapsed"].ObjToString();
                        dd.Rows.Add(dr);

                        if (!contracts.Contains(contractNumber))
                            contracts += "'" + contractNumber + "',";
                    }
                }
            }
            catch ( Exception ex)
            {
            }

            try
            {
                cmd = "Select * from `icustomers` where `payer` = '" + payer + "';";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    for (int i = 0; i < dx.Rows.Count; i++)
                    {
                        dr = dd.NewRow();
                        dr["what"] = "Customers";
                        dr["payer"] = dx.Rows[i]["payer"].ObjToString();
                        contractNumber = dx.Rows[i]["contractNumber"].ObjToString();
                        dr["contractNumber"] = contractNumber;
                        dr["deceasedDate"] = dx.Rows[i]["deceasedDate"].ObjToDateTime().ToString("MM/dd/yyyy");
                        //dr["lapsedDate"] = dx.Rows[i]["lapseDate8"].ObjToDateTime().ToString("MM/dd/yyyy");
                        dr["lapsed"] = dx.Rows[i]["lapsed"].ObjToString();
                        dd.Rows.Add(dr);

                        if (!contracts.Contains(contractNumber))
                            contracts += "'" + contractNumber + "',";
                    }
                }
            }
            catch ( Exception ex)
            {
            }

            try
            {
                cmd = "Select * from `policies` where `payer` = '" + payer + "';";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    for (int i = 0; i < dx.Rows.Count; i++)
                    {
                        dr = dd.NewRow();
                        dr["what"] = "Policies";
                        dr["payer"] = dx.Rows[i]["payer"].ObjToString();
                        contractNumber = dx.Rows[i]["contractNumber"].ObjToString();
                        dr["contractNumber"] = contractNumber;
                        dr["policyNumber"] = dx.Rows[i]["policyNumber"].ObjToString();
                        dr["deceasedDate"] = dx.Rows[i]["deceasedDate"].ObjToDateTime().ToString("MM/dd/yyyy");
                        dr["lapsedDate"] = dx.Rows[i]["lapsedDate8"].ObjToDateTime().ToString("MM/dd/yyyy");
                        //dr["lapsed"] = dx.Rows[i]["lapsed"].ObjToString();
                        dd.Rows.Add(dr);

                        if (!contracts.Contains(contractNumber))
                            contracts += "'" + contractNumber + "',";
                    }
                }
            }
            catch ( Exception ex)
            {
            }

            try
            {
                contracts = contracts.TrimEnd(',');
                cmd = "Select * from `icontracts` WHERE `contractNumber` IN (" + contracts + ");";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    for (int i = 0; i < dx.Rows.Count; i++)
                    {
                        dr = dd.NewRow();
                        dr["what"] = "Contracts";
                        //dr["payer"] = dx.Rows[i]["payer"].ObjToString();
                        contractNumber = dx.Rows[i]["contractNumber"].ObjToString();
                        dr["contractNumber"] = contractNumber;
                        dr["deceasedDate"] = dx.Rows[i]["deceasedDate"].ObjToDateTime().ToString("MM/dd/yyyy");
                        dr["lapsedDate"] = dx.Rows[i]["lapseDate8"].ObjToDateTime().ToString("MM/dd/yyyy");
                        dr["lapsed"] = dx.Rows[i]["lapsed"].ObjToString();
                        dd.Rows.Add(dr);
                    }
                }
            }
            catch ( Exception ex )
            {
            }

            BadPolicyList listForm = new BadPolicyList(dd);
            listForm.Show();

            //ViewDataTable viewForm = new ViewDataTable(dd, "what,payer,contractNumber,deceasedDate,lapsedDate, lapsed, policyNumber");
            //viewForm.ManualDone += ViewForm_ManualDone;
            //viewForm.ShowDialog();
        }
        /****************************************************************************************/
        private void ViewForm_ManualDone(DataTable dd, DataRow dr)
        {
            string what = dr["what"].ObjToString();
            string contractNumber = dr["contractNumber"].ObjToString();
            if (!String.IsNullOrWhiteSpace(contractNumber))
            {
                this.Cursor = Cursors.WaitCursor;
                CustomerDetails clientForm = new CustomerDetails(contractNumber);
                clientForm.Show();
                this.Cursor = Cursors.Default;
            }
        }
        /****************************************************************************************/
    }
}