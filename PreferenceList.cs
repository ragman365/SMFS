using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.LookAndFeel;
using DevExpress.XtraEditors;
using DevExpress.XtraBars;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Base.ViewInfo;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraPrinting;
using GeneralLib;
using DevExpress.Utils;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class PreferenceList : DevExpress.XtraEditors.XtraForm
    {
        private DataTable preferenceDt = null;
        private bool modified = false;
        /***********************************************************************************************/
        public PreferenceList()
        {
            InitializeComponent();
        }
        /***********************************************************************************************/
        private void PreferenceList_Load(object sender, EventArgs e)
        {
            LoadData();
        }
        /***********************************************************************************************/
        private void LoadData()
        {
            string cmd = "Select * from `preferenceList`;";
            preferenceDt = G1.get_db_data(cmd);
            preferenceDt.Columns.Add("num");
            preferenceDt.Columns.Add("modified");
            G1.NumberDataTable(preferenceDt);
            dgv.DataSource = preferenceDt;
        }
        /***********************************************************************************************/
        private void btnAdd_Click(object sender, EventArgs e)
        {
            string record = G1.create_record("preferenceList", "module", "-1");
            if (string.IsNullOrWhiteSpace(record))
                MessageBox.Show("***ERROR*** Creating Preference List Record!");
            else if (record == "-1")
                MessageBox.Show("***ERROR*** Creating Preference List Record!");
            else
            {
                G1.update_db_table("preferenceList", "record", record, new string[] { "module", "new module " + record.ToString(), "preference", "new preference" });
            }
            LoadData();
        }
        /***********************************************************************************************/
        private void gridMain_CellValueChanging(object sender, CellValueChangedEventArgs e)
        {
            gridMain_KeyPress(null, null);
        }
        /***********************************************************************************************/
        private void gridMain_KeyPress(object sender, KeyPressEventArgs e)
        {
            modified = true;
            DataRow dr = gridMain.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            dr["modified"] = "YES";
        }
        /***********************************************************************************************/
        private void PreferenceList_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!modified)
                return;
            DialogResult result = MessageBox.Show("***Question***\nPreference List has been modified!\nWould you like to save your changes?", "Add/Edit Preferences Dialog", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (result == DialogResult.Cancel)
            {
                e.Cancel = true;
                return;
            }
            modified = false;
            if (result == DialogResult.No)
                return;
            UpdatePreferenceList();
        }
        /***********************************************************************************************/
        private void UpdatePreferenceList ()
        {
            string mod = "";
            string module = "";
            string preference = "";
            string defaultAnswer = "";
            string record = "";
            string admin = "";
            string superuser = "";
            string homeoffice = "";
            string field = "";
            DataTable dt = (DataTable)dgv.DataSource;
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                mod = dt.Rows[i]["modified"].ObjToString();
                if (mod.ToUpper() != "YES")
                    continue;
                record = dt.Rows[i]["record"].ObjToString();
                module = dt.Rows[i]["module"].ObjToString();
                preference = dt.Rows[i]["preference"].ObjToString();
                defaultAnswer = dt.Rows[i]["default"].ObjToString();
                admin = dt.Rows[i]["Admin"].ObjToString();
                superuser = dt.Rows[i]["SuperUser"].ObjToString();
                homeoffice = dt.Rows[i]["HomeOffice"].ObjToString();
                field = dt.Rows[i]["Field"].ObjToString();
                G1.update_db_table("preferenceList", "record", record, new string[] { "module", module, "preference", preference, "default", defaultAnswer, "Admin", admin, "SuperUser", superuser, "HomeOffice", homeoffice, "Field", field});
            }
        }
        /***********************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string module = dr["module"].ObjToString();
            string preference = dr["preference"].ObjToString();
            Preference prefForm = new Preference(module, preference);
            prefForm.ShowDialog();
        }
        /***********************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (gridMain.OptionsFind.AlwaysVisible == true)
                gridMain.OptionsFind.AlwaysVisible = false;
            else
                gridMain.OptionsFind.AlwaysVisible = true;
        }
        /***********************************************************************************************/
        private void btnDelete_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            string module = dr["module"].ObjToString();
            string preference = dr["preference"].ObjToString();
            DialogResult result = MessageBox.Show("***Question***\nAre you SURE you want to remove User Preference (" + module + "/" + preference + ") ?", "Delete Preferences Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;
            string cmd = "Delete from `preferenceusers` where `module` = '" + module + "' and `preference` = '" + preference + "';";
            G1.get_db_data(cmd);
            G1.delete_db_table("preferencelist", "record", record);
            LoadData();
        }
        /***********************************************************************************************/
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
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
            printableComponentLink1.Landscape = false;

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
            printableComponentLink1.Landscape = false;

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
            Printer.DrawQuad(6, 8, 4, 4, "Permissions List", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);


            //            Printer.DrawQuadTicks();
            DateTime date = DateTime.Now;
            string workDate = date.Month.ToString("D2") + "/" + date.Year.ToString("D4");
            Printer.SetQuadSize(24, 12);
            font = new Font("Ariel", 9, FontStyle.Bold);
            //            Printer.DrawQuad(20, 8, 5, 4, "Report Month:" + workDate, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(16, 8, 3, 4, lblPayment.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(19, 8, 3, 4, lblTrust85.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(22, 8, 3, 4, lblTrust100.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /***********************************************************************************************/
    }
}