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

using DevExpress.Utils;
using DevExpress.Utils.Drawing;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraEditors.ViewInfo;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Controls;
using DevExpress.XtraGrid.Views.BandedGrid;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports.UI;


//using DevExpress.XtraEditors.Controls;
//using DevExpress.Utils;
//using DevExpress.XtraPrinting;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class EditManagerCommissions : DevExpress.XtraEditors.XtraForm
    {
        private string workTable = "";
        private string workColumns = "";
        private bool modified = false;
        private string workAs = "";
        private DataTable funServices = null;
        /****************************************************************************************/
        public EditManagerCommissions( string workingAs = "" )
        {
            InitializeComponent();
            workAs = workingAs;
            btnSaveServices.Hide();
        }
        /****************************************************************************************/
        private void EditManagerCommissions_Load(object sender, EventArgs e)
        {
            LoadData();
        }
        /****************************************************************************************/
        private void LoadData ()
        {
            string manager = "";
            string location = "";
            string atneedcode = "";
            DataTable manDt = new DataTable();
            manDt.Columns.Add("num");
            manDt.Columns.Add("atneedcode");
            manDt.Columns.Add("location");
            manDt.Columns.Add("ma");
            manDt.Columns.Add("name");
            manDt.Columns.Add("order", Type.GetType("System.Int32"));

            if (String.IsNullOrWhiteSpace(workAs))
            {
                btnRun.Hide();
                workAs = "M";
            }


            DataRow[] dRows = null;
            DataRow dR = null;

            string cmd = "Select * from `funeralhomes`;";
            DataTable funDt = G1.get_db_data(cmd);
            if (workAs == "M")
            {
                for (int i = 0; i < funDt.Rows.Count; i++)
                {
                    manager = funDt.Rows[i]["manager"].ObjToString();
                    if (String.IsNullOrWhiteSpace(manager))
                        continue;

                    atneedcode = funDt.Rows[i]["atneedcode"].ObjToString();

                    location = funDt.Rows[i]["LocationCode"].ObjToString();

                    //dRows = manDt.Select("name='" + manager + "'");
                    //if (dRows.Length <= 0)
                    //{
                    dR = manDt.NewRow();
                    dR["atneedcode"] = atneedcode;
                    dR["location"] = location;
                    dR["name"] = manager;
                    dR["ma"] = "M";
                    dR["order"] = 1;
                    manDt.Rows.Add(dR);
                    //}
                }
            }

            string firstName = "";
            string lastName = "";
            string middleName = "";
            string name = "";

            if (workAs == "A")
            {
                cmd = "Select * from `arrangers`;";
                funDt = G1.get_db_data(cmd);
                for (int i = 0; i < funDt.Rows.Count; i++)
                {
                    firstName = funDt.Rows[i]["firstName"].ObjToString().Trim();
                    lastName = funDt.Rows[i]["lastName"].ObjToString().Trim();
                    middleName = funDt.Rows[i]["middleName"].ObjToString().Trim();
                    name = firstName + " " + lastName;
                    if (String.IsNullOrWhiteSpace(name))
                        continue;

                    dRows = manDt.Select("name='" + name + "'");
                    if (dRows.Length <= 0)
                    {
                        dR = manDt.NewRow();
                        dR["ma"] = "A";
                        dR["name"] = name;
                        dR["order"] = 3;
                        manDt.Rows.Add(dR);
                    }
                }
            }

            DataView tempview = manDt.DefaultView;
            tempview.Sort = "order asc, name asc";
            manDt = tempview.ToTable();

            dgv.DataSource = manDt;
        }
        /****************************************************************************************/
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            modified = true;
        }
        /****************************************************************************************/
        private void gridMain_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt == null)
                return;
            //string delete = dt.Rows[row]["mod"].ObjToString();
            //if (delete.ToUpper() == "D")
            //{
            //    e.Visible = false;
            //    e.Handled = true;
            //}
        }
        /****************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        { // Spy Glass
            if (dgv.Visible)
                G1.SpyGlass(gridMain);
            else if (dgv2.Visible)
                G1.SpyGlass(gridMain2);

            //if (gridMain.OptionsFind.AlwaysVisible == true)
            //    gridMain.OptionsFind.AlwaysVisible = false;
            //else
            //    gridMain.OptionsFind.AlwaysVisible = true;
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
            //DialogResult result = MessageBox.Show("***Question*** Data has been modified.\nDo you really want to exit WITHOUT saving your data?", "Data Modified Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            //if (result == DialogResult.Yes)
            //    return;
            //e.Cancel = true;
        }
        /****************************************************************************************/
        private void repositoryItemComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            modified = true;
        }
        /****************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();
            string manager = dr["name"].ObjToString();
            if (String.IsNullOrWhiteSpace(manager))
                return;
            string who = dr["ma"].ObjToString();
            string location = dr["location"].ObjToString();

            this.Cursor = Cursors.WaitCursor;
            FunManager funForm = new FunManager(null, manager, who, location );
            funForm.Show();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void btnRun_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();
            string manager = "";
            string who = "";
            string location = "";
            this.Cursor = Cursors.WaitCursor;
            FunManager funForm = new FunManager(dt, manager, who, location );
            funForm.Show();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private int pageMarginLeft = 0;
        private int pageMarginRight = 0;
        private int pageMarginTop = 0;
        private int pageMarginBottom = 0;
        /***********************************************************************************************/
        private void printPreviewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            printPreview(false);
        }
        /***********************************************************************************************/
        private void printPreview(bool batch = true)
        {
            if (this.components == null)
                this.components = new System.ComponentModel.Container();

            DevExpress.XtraPrinting.PrintingSystem printingSystem1 = new DevExpress.XtraPrinting.PrintingSystem(this.components);
            DevExpress.XtraPrinting.PrintableComponentLink printableComponentLink1 = new DevExpress.XtraPrinting.PrintableComponentLink(this.components);

            printingSystem1.Links.AddRange(new object[] {
            printableComponentLink1});


            if (dgv.Visible)
                printableComponentLink1.Component = dgv;
            else if (dgv2.Visible)
                printableComponentLink1.Component = dgv2;
            else
                return;

            printableComponentLink1.PrintingSystemBase = printingSystem1;

            printableComponentLink1.EnablePageDialog = true;

            printableComponentLink1.CreateDetailHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateDetailHeaderArea);
            printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);
            printableComponentLink1.BeforeCreateAreas += new System.EventHandler(this.printableComponentLink1_BeforeCreateAreas);
            printableComponentLink1.AfterCreateAreas += new System.EventHandler(this.printableComponentLink1_AfterCreateAreas);
            printableComponentLink1.Landscape = true;

            //            Printer.setupPrinterMargins(50, 50, 80, 50);
            Printer.setupPrinterMargins(50, 50, 80, 50);

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

            if (dgv.Visible)
                printableComponentLink1.Component = dgv;
            else if (dgv2.Visible)
                printableComponentLink1.Component = dgv2;
            else
                return;

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

            printingSystem1.Document.AutoFitToPagesWidth = 1;

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
            string title = this.Text;
            if (dgv2.Visible)
                title = "Funeral Commission Service Exceptions";

            Printer.DrawQuad(5, 8, 6, 4, title, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            //            Printer.DrawQuadTicks();
            DateTime date = DateTime.Now;
            string workDate = date.Month.ToString("D2") + "/" + date.Year.ToString("D4");
            Printer.SetQuadSize(24, 12);
            font = new Font("Ariel", 9, FontStyle.Regular);
            //Printer.DrawQuad(20, 8, 5, 4, "Report Month:" + workDate, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(16, 8, 3, 4, lblPayment.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(19, 8, 3, 4, lblTrust85.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(22, 8, 3, 4, lblTrust100.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /****************************************************************************************/
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            TabPage current = (sender as TabControl).SelectedTab;
            if (current == null)
                return;
            if (current.Text.ToUpper() == "EXCEPTIONS")
            {
                if (funServices == null)
                {
                    string cmd = "Select * from `funeral_master`;";
                    funServices = G1.get_db_data(cmd);

                    funServices.Columns.Add("mod");

                    funServices = CleanupServices(funServices);

                    funServices = SetupSelection(funServices, this.repositoryItemCheckEdit2, "asService");
                    funServices = SetupSelection(funServices, this.repositoryItemCheckEdit1, "asCash");
                    funServices = SetupSelection(funServices, this.repositoryItemCheckEdit3, "asNothing");
                    funServices = SetupSelection(funServices, this.repositoryItemCheckEdit4, "asMerc");
                    funServices = SetupSelection(funServices, this.repositoryItemCheckEdit5, "fromService");
                    funServices = SetupSelection(funServices, this.repositoryItemCheckEdit6, "fromMerc");
                    G1.NumberDataTable(funServices);
                    dgv2.DataSource = funServices;
                    dgv2.Refresh();
                }
            }
        }
        /***********************************************************************************************/
        DataTable CleanupServices ( DataTable dt )
        {
            string type = "";
            string service = "";
            for ( int i=(dt.Rows.Count-1); i>=0; i--)
            {
                service = dt.Rows[i]["service"].ObjToString();
                type = dt.Rows[i]["type"].ObjToString();
                if (String.IsNullOrWhiteSpace(type))
                    dt.Rows.RemoveAt(i);
                else if (String.IsNullOrWhiteSpace(service))
                    dt.Rows.RemoveAt(i);
            }

            DataRow [] dRows = dt.Select("service='LOWEST CASKET PRICE'");
            if (dRows.Length > 0 )
                dt.Rows.Remove(dRows[0]);
            dRows = dt.Select("service='HIGHEST CASKET PRICE'");
            if (dRows.Length > 0)
                dt.Rows.Remove(dRows[0]);

            dRows = dt.Select("service='LOWEST CONTAINER PRICE'");
            if (dRows.Length > 0)
                dt.Rows.Remove(dRows[0]);
            dRows = dt.Select("service='HIGHEST CONTAINER PRICE'");
            if (dRows.Length > 0)
                dt.Rows.Remove(dRows[0]);

            return dt;
        }
        /***********************************************************************************************/
        private DataTable SetupSelection(DataTable dt, RepositoryItemCheckEdit repository, string columnName )
        {
            if (G1.get_column_number(dt, columnName) < 0)
                dt.Columns.Add(columnName);
            repository.NullText = "";
            repository.ValueChecked = "1";
            repository.ValueUnchecked = "0";
            repository.ValueGrayed = "";

            string data = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                data = dt.Rows[i][columnName].ObjToString();
                if ( data == "1")
                    dt.Rows[i][columnName] = "1";
                else
                    dt.Rows[i][columnName] = "0";
            }
            return dt;
        }
        /****************************************************************************************/
        private void repositoryItemCheckEdit2_CheckedChanged(object sender, EventArgs e)
        {
            DataRow dr = gridMain2.GetFocusedDataRow();
            if (dr == null)
                return;
            dr["mod"] = "1";
            btnSaveServices.Show();
            btnSaveServices.Refresh();
            gridMain2.RefreshData();
        }
        /****************************************************************************************/
        private void repositoryItemCheckEdit1_CheckedChanged(object sender, EventArgs e)
        {
            DataRow dr = gridMain2.GetFocusedDataRow();
            if (dr == null)
                return;
            dr["mod"] = "1";
            btnSaveServices.Show();
            btnSaveServices.Refresh();
            gridMain2.RefreshData();
        }
        /****************************************************************************************/
        private void btnSaveServices_Click(object sender, EventArgs e)
        {
            string mod = "";
            string asService = "";
            string asCash = "";
            string asNothing = "";
            string asMerc = "";
            string fromService = "";
            string fromMerc = "";
            string record = "";

            DataTable dt = (DataTable)dgv2.DataSource;
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                mod = dt.Rows[i]["mod"].ObjToString();
                if (mod != "1")
                    continue;
                asService = dt.Rows[i]["asService"].ObjToString();
                asCash = dt.Rows[i]["asCash"].ObjToString();
                asNothing = dt.Rows[i]["asNothing"].ObjToString();
                asMerc = dt.Rows[i]["asMerc"].ObjToString();
                fromService = dt.Rows[i]["fromService"].ObjToString();
                fromMerc = dt.Rows[i]["fromMerc"].ObjToString();
                record = dt.Rows[i]["record"].ObjToString();

                G1.update_db_table("funeral_master", "record", record, new string[] {"asService", asService, "asCash", asCash, "asNothing", asNothing, "asMerc", asMerc, "fromService", fromService, "fromMerc", fromMerc });

                dt.Rows[i]["mod"] = "";
            }

            btnSaveServices.Hide();
            btnSaveServices.Refresh();
        }
        /****************************************************************************************/
        private void repositoryItemCheckEdit3_CheckedChanged(object sender, EventArgs e)
        {
            DataRow dr = gridMain2.GetFocusedDataRow();
            if (dr == null)
                return;
            dr["mod"] = "1";
            btnSaveServices.Show();
            btnSaveServices.Refresh();
            gridMain2.RefreshData();
        }
        /****************************************************************************************/
        private void repositoryItemCheckEdit4_CheckedChanged(object sender, EventArgs e)
        {
            DataRow dr = gridMain2.GetFocusedDataRow();
            if (dr == null)
                return;
            dr["mod"] = "1";
            btnSaveServices.Show();
            btnSaveServices.Refresh();
            gridMain2.RefreshData();
        }
        /****************************************************************************************/
        private void repositoryItemCheckEdit5_CheckedChanged(object sender, EventArgs e)
        {
            DataRow dr = gridMain2.GetFocusedDataRow();
            if (dr == null)
                return;
            dr["mod"] = "1";
            btnSaveServices.Show();
            btnSaveServices.Refresh();
            gridMain2.RefreshData();
        }
        /****************************************************************************************/
        private void repositoryItemCheckEdit6_CheckedChanged(object sender, EventArgs e)
        {
            DataRow dr = gridMain2.GetFocusedDataRow();
            if (dr == null)
                return;
            dr["mod"] = "1";
            btnSaveServices.Show();
            btnSaveServices.Refresh();
            gridMain2.RefreshData();
        }
        /****************************************************************************************/
    }
}