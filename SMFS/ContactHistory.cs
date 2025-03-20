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
using System.IO;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraGrid.Views.Grid;
using System.Drawing.Drawing2D;
using DevExpress.Utils.Drawing;
using DevExpress.Utils;
using DevExpress.XtraEditors.Repository;
using System.Diagnostics.Contracts;
using DevExpress.XtraGrid.Columns;
using System.Configuration;
using DevExpress.XtraPrinting;
using DevExpress.XtraEditors.ViewInfo;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class ContactHistory : DevExpress.XtraEditors.XtraForm
    {
        private bool loading = true;
        private bool modified = false;
        private string primaryName = "";
        private string workContactName = "";
        private string workContactType = "";
        private int defaultFrequency = 3;
        /****************************************************************************************/
        public ContactHistory( string contactType, string contactName )
        {
            InitializeComponent();
            workContactName = contactName;
            workContactType = contactType;
        }
        /****************************************************************************************/
        private void SetupToolTips()
        {
            ToolTip tt = new ToolTip();
            tt.SetToolTip(this.pictureBox11, "Remove Contact");
        }
        /****************************************************************************************/
        private void ContactHistory_Load(object sender, EventArgs e)
        {
            if ( workContactType.Trim().ToUpper() != "CLERGY" && workContactType.Trim().ToUpper() != "CHURCH" && workContactType.Trim().ToUpper() != "HOSPICE")
            {
                gridMain.Columns["serviceId"].Visible = false;
                gridMain.Columns["dec"].Visible = false;
            }
            oldWhat = "";

            this.Text = "Contact History for " + workContactName;

            SetupToolTips();

            loading = true;

            LoadEmployees();

            LoadContactInfo();

            LoadData();

            modified = false;
            loading = false;
        }
        /***********************************************************************************************/
        private void LoadContactInfo ()
        {
            if (String.IsNullOrWhiteSpace(workContactName))
                return;
            DataRow[] dRows = null;
            DataTable dt = null;
            string cmd = "";
            string workDetail = "";

            if (!String.IsNullOrWhiteSpace(workContactType))
            {
                cmd = "Select * from `contacttypes` WHERE `contactType` = '" + workContactType + "';";
                dt = G1.get_db_data(cmd);
                if (dt.Rows.Count <= 0)
                    return;
                workDetail = dt.Rows[0]["detail"].ObjToString();
                defaultFrequency = dt.Rows[0]["frequency"].ObjToInt32();

                cmd = "Select * from `track` WHERE `contactType` = '" + workContactType + "';";
                dt = G1.get_db_data(cmd);
                if (dt.Rows.Count <= 0)
                    return;
                if (workDetail.ToUpper() == "PERSON")
                {
                    string name = "";
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        name = dt.Rows[i]["answer"].ObjToString();
                        if (!String.IsNullOrWhiteSpace(name))
                            continue;
                        name = Contacts.GetPerson(dt.Rows[i]);
                        dt.Rows[i]["answer"] = name;
                    }
                }
            }
            else
            {
                dt = G1.get_db_data("Select * from `track`;");
            }
            dRows = dt.Select("answer='" + workContactName + "'");
            if (dRows.Length > 0)
            {
                dt = dRows.CopyToDataTable();
                lblContactName.Text = workContactType;
                string str = dt.Rows[0]["address"].ObjToString();
                lblContactAddress.Text = str;
                string city = dt.Rows[0]["city"].ObjToString();
                string state = dt.Rows[0]["state"].ObjToString();
                string zip = dt.Rows[0]["zip"].ObjToString();

                str = city + " " + state + "  " + zip;
                lblContactCity.Text = str;

                str = dt.Rows[0]["phone"].ObjToString();
                lblContactPhone.Text = str;

                str = dt.Rows[0]["email"].ObjToString();
                lblContactEmail.Text = str;

                str = dt.Rows[0]["pocName"].ObjToString();
                lblpocName.Text = str;

                str = dt.Rows[0]["pocTitle"].ObjToString();
                lblpocTitle.Text = str;

                str = dt.Rows[0]["pocPhone"].ObjToString();
                lblpocPhone.Text = str;

                str = dt.Rows[0]["pocEmail"].ObjToString();
                lblpocEmail.Text = str;
            }
        }
        /***********************************************************************************************/
        private void LoadData()
        {
            this.Cursor = Cursors.WaitCursor;


            string cmd = "Select * from `contacts` WHERE `contactName` = '" + workContactName + "' ORDER by apptDate desc ";
            cmd += ";";
            DataTable dt = G1.get_db_data(cmd);

            AddMod(dt, gridMain);

            SetupCompleted ( dt );
            SetupFrequency ( dt );

            dgv.DataSource = dt;

            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void LoadEmployees()
        {
            repositoryItemComboBox2.Items.Clear();

            string cmd = "Select * from `tc_er` t JOIN `users` u ON t.`username` = u.`username` WHERE `empStatus` LIKE 'Full%';";
            DataTable dt = G1.get_db_data(cmd);

            DataRow[] dr = dt.Select("lastName<>''");
            if (dr.Length > 0)
                dt = dr.CopyToDataTable();

            string firstName = "";
            string middleName = "";
            string lastName = "";
            string name = "";

            DataView tempview = dt.DefaultView;
            tempview.Sort = "lastName,firstName,middleName";
            dt = tempview.ToTable();

            dt.Columns.Add("name");

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                firstName = dt.Rows[i]["firstName"].ObjToString();
                middleName = dt.Rows[i]["middleName"].ObjToString();
                lastName = dt.Rows[i]["lastName"].ObjToString();

                if (String.IsNullOrWhiteSpace(lastName))
                    continue;

                name = lastName;
                if (!String.IsNullOrWhiteSpace(firstName))
                    name += ", " + firstName;
                if (!String.IsNullOrWhiteSpace(middleName))
                    name += " " + middleName;

                //cmbEmployee.Items.Add(name);

                repositoryItemComboBox2.Items.Add(name);
                dt.Rows[i]["name"] = name;
            }

            DataRow[] dRows = dt.Select("username='" + LoginForm.username + "'");
            if (dRows.Length > 0)
            {
                firstName = dRows[0]["firstName"].ObjToString();
                middleName = dRows[0]["middleName"].ObjToString();
                lastName = dRows[0]["lastName"].ObjToString();

                name = lastName;
                if (!String.IsNullOrWhiteSpace(firstName))
                    name += ", " + firstName;
                if (!String.IsNullOrWhiteSpace(middleName))
                    name = " " + middleName;

                primaryName = name;
            }
        }
        /***********************************************************************************************/
        private void SetupFrequency(DataTable dt)
        {
            int frequency = 0;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                frequency = dt.Rows[i]["frequency"].ObjToInt32();
                if (frequency == 0)
                    dt.Rows[i]["frequency"] = defaultFrequency;
            }
        }
        /***********************************************************************************************/
        private void SetupCompleted(DataTable dt)
        {
            DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew = this.repositoryItemCheckEdit1;
            selectnew.NullText = "";
            selectnew.ValueChecked = "1";
            selectnew.ValueUnchecked = "0";
            selectnew.ValueGrayed = "";

            string completed = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                completed = dt.Rows[i]["completed"].ObjToString();
                if ( completed == "1")
                    dt.Rows[i]["completed"] = "1";
                else
                    dt.Rows[i]["completed"] = "0";
            }
        }
        /****************************************************************************************/
        private void gridMain_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            int row = e.ListSourceRow;
            if (dgv == null)
                return;
            if (dgv.DataSource == null)
                return;
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt.Rows.Count <= 0)
                return;
            string mod = dt.Rows[row]["mod"].ObjToString().ToUpper();
            if (mod.ToUpper() == "D" )
            {
                e.Visible = false;
                e.Handled = true;
            }

            if ( chkExcludeCompleted.Checked )
            {
                string completed = dt.Rows[row]["completed"].ObjToString().ToUpper();
                if ( completed == "1")
                {
                    e.Visible = false;
                    e.Handled = true;
                }
            }
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
                return;
            }
            int rowHandle = e.RowHandle;
            if (rowHandle < 0)
                return;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            if (dgv.DataSource == null)
                return;
            try
            {
                DataTable dt = (DataTable)dgv.DataSource;
                bool doDate = false;
                if (e.Column.FieldName == "apptDate")
                    doDate = true;
                //else if (e.Column.FieldName == "lastContactDate")
                //    doDate = true;

                if (doDate)
                {
                    if (!String.IsNullOrWhiteSpace(e.DisplayText.Trim()))
                    {
                        DateTime date = e.DisplayText.ObjToString().ObjToDateTime();
                        if (date.Year < 30)
                            e.DisplayText = "";
                        else
                        {
                            e.DisplayText = date.ToString("MM/dd/yyyy");
                        }
                    }
                }
            }
            catch ( Exception ex)
            {
            }
        }
        /****************************************************************************************/
        private bool justSaved = false;
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            //if (e == null)
            //    return;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            DataRow dr = gridMain.GetFocusedDataRow();
            if (dr == null)
                return;

            dr["mod"] = "Y";

            DataTable dt = (DataTable)dgv.DataSource;

            GridColumn currCol = gridMain.FocusedColumn;
            string currentColumn = currCol.FieldName;
            string what = dr[currentColumn].ObjToString();
            //if (currentColumn.ToUpper() == "contactName")
            //{
            //    what = dr[currentColumn].ObjToString();

            //    if (String.IsNullOrWhiteSpace(what))
            //        return;
            //    bool found = false;

            //    string contactType = dr["contactType"].ObjToString();
            //    if (!String.IsNullOrWhiteSpace(contactType))
            //    {

            //        DataTable cDt = null;
            //        string cmd = "Select * from `track` WHERE `contactType` = '" + contactType + "' AND `answer` LIKE '%" + what + "%' ;";
            //        cDt = G1.get_db_data(cmd);
            //        if ( cDt.Rows.Count > 0 )
            //        {
            //            what = cDt.Rows[0]["answer"].ObjToString();
            //            dr["contactName"] = what;
            //        }
            //    }
            //}
            if (currentColumn.ToUpper() == "NUM")
                return;
            string record = dr["record"].ObjToString();
            if (!String.IsNullOrWhiteSpace(record))
            {
                if ( currentColumn.ToUpper() == "APPTDATE")
                {
                    DateTime date = what.ObjToDateTime();
                    what = date.ToString("yyyy-MM-dd");
                }
                try
                {
                    G1.update_db_table("contacts", "record", record, new string[] { currentColumn, what });
                }
                catch ( Exception ex)
                {
                }
            }

            gridMain.RefreshData();
        }
        /****************************************************************************************/
        private void UpdateMod(DataRow dr)
        {
            dr["mod"] = "Y";
            modified = true;
        }
        /****************************************************************************************/
        private void pictureBox11_Click(object sender, EventArgs e)
        { // Remove Existing Payment
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();
            if (dr == null)
                return;

            int row = gridMain.FocusedRowHandle;
            row = gridMain.GetDataSourceRowIndex(row);
            string agent = dr["agent"].ObjToString();
            if (agent == primaryName || G1.isAdmin())
            {
                DialogResult result = MessageBox.Show("Permanently Delete This Contact?", "Delete Contact Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                if ( result == DialogResult.Yes )
                {
                    dr["mod"] = "D";
                    G1.NumberDataTable(dt);

                    gridMain.RefreshEditor(true);
                }
            }
            else
            {
                MessageBox.Show("Do do not have permission to\ndelete this contact!", "Delete Contact Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            }
            this.Cursor = Cursors.Arrow;
        }
        /***********************************************************************************************/
        private void AddMod(DataTable dt, DevExpress.XtraGrid.Views.Grid.GridView grid)
        {
            if (G1.get_column_number(dt, "mod") < 0)
                dt.Columns.Add("mod");
        }
        /****************************************************************************************/
        private void pictureBox4_Click(object sender, EventArgs e)
        {
            if (gridMain.OptionsFind.AlwaysVisible == true)
                gridMain.OptionsFind.AlwaysVisible = false;
            else
                gridMain.OptionsFind.AlwaysVisible = true;
        }
        /****************************************************************************************/
        private void gridMain_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            if (e.ListSourceRowIndex == DevExpress.XtraGrid.GridControl.InvalidRowHandle)
                return;
            string name = e.Column.FieldName;
            if ( name.ToUpper().IndexOf("DATE") >= 0 )
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
            bool doDate = false;
            bool doTime = false;
            if (name == "apptDate")
                doDate = true;
            else if (name == "lastContactDate")
                doDate = true;

            if (doDate)
            {
                DateTime date = e.DisplayText.ObjToString().ObjToDateTime();
                if (date.Year < 30)
                    e.DisplayText = "";
                else
                {
                    e.DisplayText = date.ToString("MM/dd/yyyy");
                }
            }

            if (doTime)
            {
                if (!String.IsNullOrWhiteSpace(e.DisplayText.Trim()))
                {
                    DateTime date = e.DisplayText.ObjToString().ObjToDateTime();
                    e.DisplayText = date.ToString("HH:mm");
                }
            }
        }
        /****************************************************************************************/
        private void FunPayments_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Validate();
            gridMain.RefreshEditor(true);

            OnDone();
        }
        /****************************************************************************************/
        private void gridMain_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();

            int focusedRow = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(focusedRow);

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

            Printer.setupPrinterMargins(5, 5, 180, 10);

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

            printableComponentLink1.Component = dgv;
            printableComponentLink1.PrintingSystemBase = printingSystem1;
            printableComponentLink1.CreateDetailHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateDetailHeaderArea);
            printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);
            printableComponentLink1.BeforeCreateAreas += new System.EventHandler(this.printableComponentLink1_BeforeCreateAreas);
            printableComponentLink1.AfterCreateAreas += new System.EventHandler(this.printableComponentLink1_AfterCreateAreas);
            printableComponentLink1.Landscape = true;

            Printer.setupPrinterMargins(5, 5, 180, 10);

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

            font = new Font("Ariel", 10, FontStyle.Regular);
            //            Printer.DrawQuad(6, 8, 4, 4, "Funeral Services Report", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            Printer.SetQuadSize(24, 48);
            Printer.DrawQuad(10, 11, 8, 6, this.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            //            Printer.DrawQuadTicks();
            DateTime date = DateTime.Now;
            string workDate = date.Month.ToString("D2") + "/" + date.Year.ToString("D4");
            //Printer.SetQuadSize(24, 24);
            font = new Font("Ariel", 9, FontStyle.Regular);
            Printer.SetQuadSize(24, 24);
            Printer.DrawQuad(1, 8, 5, 2, "Contact Info :", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            Printer.DrawQuad(2, 10, 3, 2, lblContactName.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            Printer.DrawQuad(2, 12, 3, 2, lblContactAddress.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            Printer.DrawQuad(2, 14, 3, 2, lblContactCity.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            Printer.DrawQuad(2, 16, 3, 2, lblContactPhone.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            Printer.DrawQuad(2, 18, 3, 2, lblContactEmail.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            Printer.DrawQuad(1, 20, 6, 3, "User : " + LoginForm.username, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);

            Printer.DrawQuad(9, 8, 5, 2, "Point of Contact :", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            Printer.DrawQuad(10, 10, 3, 2, lblpocName.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            Printer.DrawQuad(10, 12, 3, 2, lblpocTitle.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            Printer.DrawQuad(10, 14, 3, 2, lblpocPhone.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            Printer.DrawQuad(10, 16, 3, 2, lblpocEmail.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom); ;

            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
            //Printer.DrawQuadBorder(1, 11, 12, 1, BorderSide.Bottom, 1, Color.Black);
            Printer.SetQuadSize(12, 50);
            Printer.DrawQuadBorder(1, 49, 12, 4, BorderSide.Bottom, 1, Color.Black);
        }
        /****************************************************************************************/
        private void gridMain_ValidatingEditor(object sender, DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventArgs e)
        {
            GridView view = sender as GridView;
        }
        private string oldWhat = "";
        /****************************************************************************************/
        private string oldColumn = "";
        private DataTable trackingDt = null;
        private DataTable trackDt = null;
        RepositoryItemComboBox ciLookup = new RepositoryItemComboBox();
        /****************************************************************************************/
        private void gridMain_ShownEditor(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            int row = gridMain.FocusedRowHandle;

            GridColumn currCol = gridMain.FocusedColumn;
            DataRow dr = gridMain.GetFocusedDataRow();
            string name = currCol.FieldName;
            string record = "";
            string str = "";
            DateTime myDate = DateTime.Now;
            oldColumn = name;

            bool doDate = false;
            if (name == "apptDate")
                doDate = true;
            else if (name == "lastContactDate")
                doDate = true;

            if (doDate)
            {
                myDate = dr[name].ObjToDateTime();
                str = gridMain.Columns[name].Caption;
                using (GetDate dateForm = new GetDate(myDate, str))
                {
                    dateForm.ShowDialog();
                    if (dateForm.DialogResult == System.Windows.Forms.DialogResult.OK)
                    {
                        try
                        {
                            myDate = dateForm.myDateAnswer;
                            dr[name] = G1.DTtoMySQLDT(myDate);
                        }
                        catch (Exception ex)
                        {
                        }
                        //dr[name] = G1.DTtoMySQLDT(myDate);
                        UpdateMod(dr);
                        gridMain_CellValueChanged(null, null);
                    }
                }
            }
            gridMain.RefreshData();
            gridMain.RefreshEditor(true);
        }
        /****************************************************************************************/
        private string currentColumn = "";
        /***************************************************************************************/
        public delegate void d_contactHistoryDone( DataTable dt );
        public event d_contactHistoryDone contactHistoryDone;
        protected void OnDone()
        {
            DataTable dt = (DataTable)dgv.DataSource;

            DataTable dx = dt.Clone();

            string mod = "";

            for ( int i=0; i<dt.Rows.Count; i++)
            {
                mod = dt.Rows[i]["mod"].ObjToString().ToUpper();
                if (mod == "Y" || mod == "D" )
                    G1.copy_dt_row(dt, i, dx, dx.Rows.Count);
            }

            //DataRow [] dRows = dt.Select("mod='Y'");
            //if (dRows.Length > 0)
            //    dt = dRows.CopyToDataTable();
            //else
            //    dt.Rows.Clear();
            if (contactHistoryDone != null)
                contactHistoryDone ( dx );
        }
        /****************************************************************************************/
        private void repositoryItemCheckEdit1_CheckedChanged(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            if (dr["completed"].ObjToString() != "1")
                dr["completed"] = "1";
            else
                dr["completed"] = "0";
            dr["mod"] = "Y";

            gridMain_CellValueChanged(null, null);
        }
        /****************************************************************************************/
        private void gridMain_CustomRowCellEdit(object sender, CustomRowCellEditEventArgs e)
        {
            GridView view = sender as GridView;
            changedData = view.GetRowCellValue(e.RowHandle, e.Column).ObjToString().ToUpper();
        }
        /****************************************************************************************/
        private bool changing = false;
        private string changedData = "";
        private void gridMain_CellValueChanging(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            changing = true;
        }
        /****************************************************************************************/
        private void gridMain_CalcRowHeight(object sender, RowHeightEventArgs e)
        {
            GridView View = sender as GridView;
            if (e.RowHandle >= 0)
            {
                int maxHeight = 0;
                int newHeight = 0;
                bool doit = false;
                string name = "";
                string str = "";
                int count = 0;
                string[] Lines = null;
                foreach (GridColumn column in gridMain.Columns)
                {
                    name = column.FieldName.ToUpper();
                    if (name == "RESULTS")
                        doit = true;
                    if (doit)
                    {
                        using (RepositoryItemMemoEdit edit = new RepositoryItemMemoEdit())
                        {
                            using (MemoEditViewInfo viewInfo = edit.CreateViewInfo() as MemoEditViewInfo)
                            {
                                str = gridMain.GetRowCellValue(e.RowHandle, column.FieldName).ObjToString();
                                if (!String.IsNullOrWhiteSpace(str))
                                {
                                    Lines = str.Split('\n');
                                    count = Lines.Length + 1;
                                }
                                viewInfo.EditValue = gridMain.GetRowCellValue(e.RowHandle, column.FieldName);
                                viewInfo.Bounds = new Rectangle(0, 0, column.VisibleWidth, dgv.Height);
                                using (Graphics graphics = dgv.CreateGraphics())
                                using (GraphicsCache cache = new GraphicsCache(graphics))
                                {
                                    viewInfo.CalcViewInfo(graphics);
                                    var height = ((IHeightAdaptable)viewInfo).CalcHeight(cache, column.VisibleWidth);
                                    newHeight = Math.Max(height, maxHeight);
                                    if (newHeight > maxHeight)
                                    {
                                        maxHeight = newHeight * count;
                                    }
                                }
                            }
                        }
                    }
                }

                if (maxHeight > 0 && maxHeight > e.RowHeight)
                    e.RowHeight = maxHeight;
            }
        }
        /****************************************************************************************/
        private void chkExcludeCompleted_CheckedChanged(object sender, EventArgs e)
        {
            gridMain.RefreshData();
            gridMain.RefreshEditor(true);
            dgv.Refresh();
        }
        /****************************************************************************************/
    }
}