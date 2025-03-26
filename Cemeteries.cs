using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using GeneralLib;
using DevExpress.XtraPrinting;
using DevExpress.Utils;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class Cemeteries : DevExpress.XtraEditors.XtraForm
    {
        private bool modified = false;
        /****************************************************************************************/
        public Cemeteries()
        {
            InitializeComponent();
        }
        /****************************************************************************************/
        private void Cemeteries_Load(object sender, EventArgs e)
        {
            btnSave.Hide();
            string cmd = "Select * from `cemeteries`;";
            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("mod");
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;

            LoadBankAccounts();
        }
        /***************************************************************************************/
        DataTable bankAccounts = null;
        private void LoadBankAccounts()
        {
            string cmd = "Select * from `bank_accounts` where `show_dropdown` = '1';";
            bankAccounts = G1.get_db_data(cmd);
            this.repositoryItemComboBox1.Items.Clear();
            string account_no = "";
            for (int i = 0; i < bankAccounts.Rows.Count; i++)
            {
                account_no = bankAccounts.Rows[i]["account_no"].ObjToString();
                this.repositoryItemComboBox1.Items.Add(account_no);
            }
        }
        /****************************************************************************************/
        private void pictureAdd_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dRow = dt.NewRow();
            dt.Rows.Add(dRow);
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
        }
        /****************************************************************************************/
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            if (e.Column.FieldName.Trim().ToUpper() == "LOCATION")
            {
                string location = dr["location"].ObjToString();
                dr["location"] = location.ToUpper();
            }
            dr["mod"] = "Y";
            modified = true;
            btnSave.Show();
        }
        /****************************************************************************************/
        private void pictureDelete_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            dr["mod"] = "D";
            modified = true;
            btnSave.Show();
            gridMain.RefreshData();
            dgv.Refresh();
        }
        /****************************************************************************************/
        private void btnSave_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;

            string location = "";
            string address = "";
            string city = "";
            string state = "";
            string zip = "";
            string POBox = "";
            string POCity = "";
            string POState = "";
            string POZip = "";
            string phoneNumber = "";
            string desc = "";
            string mod = "";
            string record = "";

            for ( int i=0; i<dt.Rows.Count; i++)
            {
                try
                {
                    mod = dt.Rows[i]["mod"].ObjToString();
                    record = dt.Rows[i]["record"].ObjToString();
                    if (mod == "D")
                    {
                        if (!String.IsNullOrWhiteSpace(record))
                            G1.delete_db_table("cemeteries", "record", record);
                        continue;
                    }
                    if (String.IsNullOrWhiteSpace(record))
                        record = G1.create_record("cemeteries", "description", "-1");
                    if (G1.BadRecord("cemeteries", record))
                        continue;
                    dt.Rows[i]["record"] = record;
                    dt.Rows[i]["mod"] = "";
                    location = dt.Rows[i]["loc"].ObjToString().ToUpper();
                    desc = dt.Rows[i]["description"].ObjToString();
                    address = dt.Rows[i]["address"].ObjToString();
                    city = dt.Rows[i]["city"].ObjToString();
                    state = dt.Rows[i]["state"].ObjToString();
                    zip = dt.Rows[i]["zip"].ObjToString();
                    POBox = dt.Rows[i]["POBox"].ObjToString();
                    POState = dt.Rows[i]["POState"].ObjToString();
                    POZip = dt.Rows[i]["POZip"].ObjToString();
                    phoneNumber = dt.Rows[i]["phoneNumber"].ObjToString();
                    G1.update_db_table("cemeteries", "record", record, new string[] { "loc", location, "description", desc, "address", address, "city", city, "state", state, "zip", zip, "POBox", POBox, "POCity", POCity, "POState", POState, "POZip", POZip, "phoneNumber", phoneNumber });
                }
                catch ( Exception ex)
                {
                }
            }
            for ( int i=dt.Rows.Count-1; i>=0; i--)
            {
                try
                {
                    mod = dt.Rows[i]["mod"].ObjToString();
                    if (mod.ToUpper() == "D")
                        dt.Rows.RemoveAt(i);
                }
                catch ( Exception ex)
                {
                }
            }
            dgv.DataSource = dt;
            gridMain.RefreshData();
            dgv.Refresh();
            modified = false;
            btnSave.Hide();
        }
        /****************************************************************************************/
        private void Cemeteries_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!modified)
                return;
            DialogResult result = MessageBox.Show("***Question***\nCemeteries have been modified!\nWould you like to save your changes?", "Cemeteries Dialog", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (result == DialogResult.Cancel)
            {
                e.Cancel = true;
                return;
            }
            modified = false;
            if (result == DialogResult.No)
                return;
            DataTable dt = (DataTable)dgv.DataSource;
            btnSave_Click(null, null);
        }
        /****************************************************************************************/
        private void gridMain_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv.DataSource;
            if (G1.get_column_number(dt, "mod") < 0)
                return;
            string delete = dt.Rows[row]["mod"].ObjToString();
            if (delete.ToUpper() == "D")
            {
                e.Visible = false;
                e.Handled = true;
            }
        }
        /****************************************************************************************/
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            modified = false;
            this.Close();
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
            string title = "Cemetery List Report";
            Printer.DrawQuad(6, 8, 4, 4, title, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);


            //DateTime date = DateTime.Now;
            //string workDate = reportDate.ToString("MMMMMMMMMMMMM") + "  " + reportDate.Year.ToString("D4");
            //Printer.SetQuadSize(24, 12);
            //font = new Font("Ariel", 9, FontStyle.Regular);
            //Printer.DrawQuad(20, 8, 5, 4, " " + workDate, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            //Printer.DrawQuad(16, 8, 3, 4, lblPayment.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(19, 8, 3, 4, lblTrust85.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(22, 8, 3, 4, lblTrust100.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /****************************************************************************************/
        private void gridMain_BeforePrintRow(object sender, DevExpress.XtraGrid.Views.Printing.CancelPrintRowEventArgs e)
        {
            int rowHandle = e.RowHandle;
            if (!gridMain.IsDataRow(rowHandle))
            {
                DevExpress.XtraPrinting.BrickGraphics brick = (DevExpress.XtraPrinting.BrickGraphics)e.BrickGraphics;
            }
        }
        /****************************************************************************************/
        private void gridMain_AfterPrintRow(object sender, DevExpress.XtraGrid.Views.Printing.PrintRowEventArgs e)
        {
        }
        /****************************************************************************************/
        private void btnBanks_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            string loc = dr["loc"].ObjToString();
            string desc = dr["description"].ObjToString();
            SelectFuneralHomeBanks bankForm = new SelectFuneralHomeBanks(desc, record, true );
            bankForm.ShowDialog();

            string cmd = "Select * from `cemeteries` where `record` = '" + record + "';";
            DataTable dx = G1.get_db_data(cmd);
            if ( dx.Rows.Count > 0 )
            {
                string cashLocal = dx.Rows[0]["cashLocal"].ObjToString();
                dr["cashLocal"] = cashLocal;
                string checkLocal = dx.Rows[0]["checkLocal"].ObjToString();
                dr["checkLocal"] = dx.Rows[0]["checkLocal"].ObjToString();
            }
        }
        /****************************************************************************************/
    }
}