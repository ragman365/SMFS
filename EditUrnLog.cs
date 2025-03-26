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
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Columns;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class EditUrnLog : DevExpress.XtraEditors.XtraForm
    {
        private bool modified = false;
        private bool loading = true;
        private DataGridView dataGridView1 = new DataGridView();
        private double CrematoryCharge = 0D;
        private bool foundLocalPreference = false;
        /****************************************************************************************/
        public EditUrnLog()
        {
            InitializeComponent();
        }
        /****************************************************************************************/
        private void EditUrnLog_Load(object sender, EventArgs e)
        {
            //string cmd = "Select * from `cremation_log` ORDER BY `order`;";
            //DataTable dt = G1.get_db_data(cmd);

            DateTime date = DateTime.Now;
            this.dateTimePicker2.Value = date;

            date = date.AddDays(-90);
            this.dateTimePicker1.Value = date;

            LoadFuneralHomes();
            LoadContainerTypes();
            loadCrematoryOperators();

            dgv.Dock = DockStyle.Fill;
            dgv2.Hide();
            dgv2.Dock = DockStyle.Fill;

            if (G1.isField())
                reportsToolStripMenuItem.Dispose();

            GetCrematoryCharge();

            //bool modified = LoadData ( this.dateTimePicker1.Value, this.dateTimePicker2.Value );

            btnRun_Click(null, null);

            //if ( !modified )
            //    btnSaveAll.Hide();

            string saveName = "UrnLog Primary";
            string skinName = "";

            foundLocalPreference = G1.RestoreGridLayout(this, this.dgv, gridMain, LoginForm.username, saveName, ref skinName);
            if (!String.IsNullOrWhiteSpace(skinName))
            {
                //if (skinName != "DevExpress Style")
                //    skinForm_SkinSelected("Skin : " + skinName);
            }

            loadGroupCombo(cmbSelectColumns, "UrnLog", "Primary");
            cmbSelectColumns.Text = "Primary";

            loading = false;

            SetButtons(true);
        }
        /****************************************************************************************/
        private void LoadCreamtoryLocations ( string location )
        {
            string cmd = "Select * from `crematories` order by `keycode`;";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return;

            chkComboLocation.Properties.DataSource = dt;

            chkComboLocation.EditValue = location;
            chkComboLocation.Text = location;

        }
        /****************************************************************************************/
        private void loadCrematoryOperators()
        {
            this.repositoryItemComboBox7.Items.Clear();
            this.repositoryItemComboBox8.Items.Clear();

            string location = chkComboLocation.Text;
            string[] Lines = location.Split('|');
            if (Lines.Length > 0)
                location = Lines[0].Trim();

            string cmd = "";

            cmd = "Select * from `crematory_operators`;";
            DataTable locDt = G1.get_db_data(cmd);

            if (locDt.Rows.Count <= 0)
                return;

            DataRow[] dRows = null;

            string firstName = "";
            string lastName = "";
            string middleName = "";
            string name = "";

            if (String.IsNullOrWhiteSpace(location))
            {
                cmd = "Select * from `users` u JOIN `tc_er` t ON u.`username` = t.`username` WHERE u.`username` = '" + LoginForm.username + "';";
                DataTable dt = G1.get_db_data(cmd);
                if (dt.Rows.Count <= 0)
                    return;
                firstName = dt.Rows[0]["firstName"].ObjToString();
                lastName = dt.Rows[0]["lastName"].ObjToString();
                middleName = dt.Rows[0]["middleName"].ObjToString();

                name = "";
                dRows = locDt.Select("firstName='" + firstName + "' AND lastName='" + lastName + "' AND middleName='" + middleName + "'");
                if (dRows.Length > 0)
                {
                    location = dRows[0]["location"].ObjToString();
                    name = lastName;
                    if (!String.IsNullOrWhiteSpace(firstName))
                        name += ", " + firstName;
                    if (!String.IsNullOrWhiteSpace(middleName))
                        name += " " + middleName;
                    this.repositoryItemComboBox7.Items.Add(name);
                    this.repositoryItemComboBox8.Items.Add(name);
                    locDt.Rows.Remove(dRows[0]);
                }
            }

            if (!String.IsNullOrWhiteSpace(location))
            {
                DataView tempview = locDt.DefaultView;
                tempview.Sort = "location";
                locDt = tempview.ToTable();
                dRows = locDt.Select("location='" + location + "'");
                if ( dRows.Length > 0 )
                {
                    for (int j = 0; j < dRows.Length; j++)
                    {
                        firstName = dRows[j]["firstName"].ObjToString();
                        lastName = dRows[j]["lastName"].ObjToString();
                        middleName = dRows[j]["middleName"].ObjToString();
                        name = lastName;
                        if (!String.IsNullOrWhiteSpace(firstName))
                            name += ", " + firstName;
                        if (!String.IsNullOrWhiteSpace(middleName))
                            name += " " + middleName;
                        this.repositoryItemComboBox7.Items.Add(name);
                        this.repositoryItemComboBox8.Items.Add(name);
                        locDt.Rows.Remove(dRows[j]);
                    }
                }
            }

            for (int i = 0; i < locDt.Rows.Count; i++)
            {
                firstName = locDt.Rows[i]["firstName"].ObjToString();
                lastName = locDt.Rows[i]["lastName"].ObjToString();
                middleName = locDt.Rows[i]["middleName"].ObjToString();
                name = lastName;
                if (!String.IsNullOrWhiteSpace(firstName))
                    name += ", " + firstName;
                if (!String.IsNullOrWhiteSpace(middleName))
                    name += " " + middleName;
                this.repositoryItemComboBox7.Items.Add(name);
                this.repositoryItemComboBox8.Items.Add(name);
            }

            if ( loading )
                LoadCreamtoryLocations(location);
        }
        /****************************************************************************************/
        private void GetCrematoryCharge()
        {
            string cmd = "Select * from `options` where `option` = 'SMFS Crematory Charge';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return;
            string str = dt.Rows[0]["answer"].ObjToString();
            str = str.Replace(",", "");
            if (G1.validate_numeric(str))
                CrematoryCharge = str.ObjToDouble();
        }
        /****************************************************************************************/
        private void LoadContainerTypes()
        {
            string cmd = "Select * from `ref_container_type` order by `container_type`;";
            DataTable locDt = G1.get_db_data(cmd);

            repositoryItemComboBox3.Items.Clear();

            for (int i = 0; i < locDt.Rows.Count; i++)
                repositoryItemComboBox3.Items.Add(locDt.Rows[i]["container_type"].ObjToString());
        }
        /****************************************************************************************/
        private void LoadFuneralHomes ()
        {
            string cmd = "Select * from `funeralhomes` order by `keycode`;";
            DataTable locDt = G1.get_db_data(cmd);

            repositoryItemComboBox2.Items.Clear();
            repositoryItemComboBox4.Items.Clear();

            for (int i = 0; i < locDt.Rows.Count; i++)
            {
                repositoryItemComboBox2.Items.Add(locDt.Rows[i]["LocationCode"].ObjToString());
                repositoryItemComboBox4.Items.Add(locDt.Rows[i]["LocationCode"].ObjToString());
            }
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
            else if (delete.ToUpper() == "X")
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
                return;
            }
            bool doDate = false;
            if (e.Column.FieldName == "date_placed_in_cooler")
                doDate = true;
            else if (e.Column.FieldName == "date_received")
                doDate = true;
            else if (e.Column.FieldName == "date_cremation_started")
                doDate = true;
            else if (e.Column.FieldName == "date_cremation_completed")
                doDate = true;

            bool doTime = false;
            if (e.Column.FieldName == "time_placed_in_cooler")
                doTime = true;
            else if (e.Column.FieldName == "time_received")
                doTime = true;
            else if (e.Column.FieldName == "time_cremation_started")
                doTime = true;
            else if (e.Column.FieldName == "time_cremation_completed")
                doTime = true;

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
        /****************************************************************************************/
        private bool CheckDataModified ()
        {
            if (!btnSaveAll.Visible)
                return true;
            DialogResult result = MessageBox.Show("***Question*** Data has been modified.\nDo you really want to change Locations WITHOUT saving your data?", "Data Modified Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
                return true;
            return false;
        }
        /****************************************************************************************/
        private void EditTable_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!btnSaveAll.Visible)
                return;
            DialogResult result = MessageBox.Show("***Question*** Data has been modified.\nDo you really want to exit WITHOUT saving your data?", "Data Modified Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
                return;
            e.Cancel = true;
        }
        /****************************************************************************************/
        private void gridMain_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            if (e.ListSourceRowIndex == DevExpress.XtraGrid.GridControl.InvalidRowHandle)
                return;
            string name = e.Column.FieldName;
            bool doDate = false;
            bool doTime = false;
            if (name == "date_placed_in_cooler")
                doDate = true;
            else if (name == "date_received")
                doDate = true;
            else if (name == "date_cremation_started")
                doDate = true;
            else if (name == "date_cremation_completed")
                doDate = true;
            else if (name == "deceasedDate")
                doDate = true;

            if (name == "time_placed_in_cooler")
                doTime = true;
            else if (name == "time_received")
                doTime = true;
            else if (name == "time_cremation_started")
                doTime = true;
            else if (name == "time_cremation_completed")
                doTime = true;

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
        private void btnRight_Click(object sender, EventArgs e)
        {
            DateTime date = this.dateTimePicker1.Value;
            date = date.AddDays(60);
            this.dateTimePicker1.Value = date;

            date = this.dateTimePicker2.Value;
            date = date.AddDays(60);
            this.dateTimePicker2.Value = date;
        }
        /****************************************************************************************/
        private void btnLeft_Click(object sender, EventArgs e)
        {
            DateTime date = this.dateTimePicker1.Value;
            date = date.AddDays(-60);
            this.dateTimePicker1.Value = date;

            date = this.dateTimePicker2.Value;
            date = date.AddDays(-60);
            this.dateTimePicker2.Value = date;
        }
        /****************************************************************************************/
        private void btnRun_Click(object sender, EventArgs e)
        {
            if ( btnRun.Text == "Go Back")
            {
                SetButtons(true);
                dgv2.Hide();
                dgv.Show();
                dgv.Refresh();
                btnRun.Text = "Run";
                btnRun.BackColor = Color.Transparent;
                lblTitle.Text = "Cremation Log";
                return;
            }
            this.Cursor = Cursors.WaitCursor;

            string what = cmbSearchBy.Text.Trim().ToUpper();
            string search = "";

            if (what == "DATE PLACED IN COOLER")
                search = "date_placed_in_cooler";
            else if (what == "DATE RECEIVED")
                search = "date_received";
            else if (what == "DATE CREMATION STARTED")
                search = "date_cremation_started";
            else if (what == "DATE CREMATION COMPLETED")
                search = "date_cremation_completed";
            else if (what == "DECEASED DATE")
                search = "deceasedDate";

            if (String.IsNullOrWhiteSpace(search))
                return;

            DateTime start = this.dateTimePicker1.Value;
            DateTime stop = this.dateTimePicker2.Value;

            string date1 = start.ToString("yyyyMMdd") + "000000";
            string date2 = stop.ToString("yyyyMMdd") + "235959";

            string cmd = "Select * from `cremation_log` c ";
            cmd += " JOIN `fcustomers` f ON c.`contractNumber` = f.`contractNumber` ";

            //cmd += " WHERE `date_placed_in_cooler` >= '" + date1 + "' AND `date_placed_in_cooler` <= '" + date2 + "' ";
            if (search == "deceasedDate")
            {
                date1 = start.ToString("yyyy-MM-dd 00:00:00");
                date2 = stop.ToString("yyyy-MM-dd 23:59:59");
                cmd += " WHERE f.`" + search + "` >= '" + date1 + "' AND `" + search + "` <= '" + date2 + "' ";
            }
            else
                cmd += " WHERE `" + search + "` >= '" + date1 + "' AND `" + search + "` <= '" + date2 + "' ";

            string searchLocation = GetCrematorySearchLocations();

            if (!String.IsNullOrWhiteSpace(searchLocation))
                cmd += " AND " + searchLocation + " ";

            cmd += " ORDER by `order` ";
            cmd += ";";

            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("num");
            dt.Columns.Add("mod");

            bool modified = ReprocessData(dt);

            dgv.DataSource = dt;

            if ( !modified )
            {
                btnSaveAll.Hide();
                btnSaveAll.Refresh();
            }
            else
            {
                btnSaveAll.Show();
                btnSaveAll.Refresh();
            }

            SetButtons(true);

            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private string GetCrematorySearchLocations ()
        {
            DataRow[] dRows = null;
            DataTable locDt = (DataTable)this.chkComboLocation.Properties.DataSource;
            string procLoc = "";
            string jewelLoc = "";
            string[] locIDs = this.chkComboLocation.EditValue.ToString().Split('|');
            string keycode = "";
            string where = "";
            for (int i = 0; i < locIDs.Length; i++)
            {
                if (!String.IsNullOrWhiteSpace(locIDs[i]))
                {
                    where = locIDs[i].Trim();
                    dRows = locDt.Select("LocationCode='" + where + "'");
                    if (dRows.Length > 0)
                    {
                        if (procLoc.Trim().Length > 0)
                            procLoc += " OR ";
                        keycode = dRows[0]["keycode"].ObjToString();
                        procLoc += " `smc_cremation_number` LIKE '___" + keycode + "%' ";
                    }
                }
            }
            if (!String.IsNullOrWhiteSpace(procLoc))
                procLoc = "( " + procLoc + " ) ";

            return procLoc;
        }
        /****************************************************************************************/
        private bool LoadData(DateTime startTime, DateTime stopTime)
        {
            this.Cursor = Cursors.WaitCursor;

            string date1 = startTime.ToString("yyyyMMdd") + "000000";
            string date2 = stopTime.ToString("yyyyMMdd") + "235959";

            string search = GetCrematorySearchLocations();

            string cmd = "Select * from `cremation_log` ";
            cmd += " WHERE `date_received` >= '" + date1 + "' AND `date_received` <= '" + date2 + "' ";
            if (!String.IsNullOrWhiteSpace(search))
                cmd += " AND " + search + " ";
            cmd += " ORDER by `order` ";
            cmd += ";";

            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("mod");
            dt.Columns.Add("num");

            modified = ReprocessData(dt);

            dgv.DataSource = dt;

            this.Cursor = Cursors.Default;
            return modified;
        }
        /****************************************************************************************/
        private bool ReprocessData ( DataTable dt )
        {
            string record = "";
            string str = "";
            string dateStr = "";
            string strDate = "";
            string[] Lines = null;
            DateTime date = DateTime.Now;
            bool doDate = false;
            string urn = "";
            double price = 0D;
            string contractNumber = "";
            string cmd = "";
            DataTable dx = null;
            string cname = "";
            string fName = "";
            string lName = "";
            string middleName = "";
            string gender = "";
            string oldUrn = "";
            double oldPrice = 0D;
            bool modified = false;

            for ( int i=0; i<dt.Rows.Count; i++)
            {
                str = dt.Rows[i]["funeral_home_number"].ObjToString();
                if (String.IsNullOrWhiteSpace(str))
                    continue;

                cmd = "Select * from `fcust_extended` e JOIN `fcustomers` c ON e.`contractNumber` = c.`contractNumber` WHERE e.`serviceId` = '" + str + "';";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    cname = "";
                    fName = dx.Rows[0]["firstName"].ObjToString();
                    lName = dx.Rows[0]["lastName"].ObjToString();
                    cname = lName.Trim() + ", ";
                    middleName = dx.Rows[0]["middleName"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(middleName))
                        cname += middleName.Substring(0, 1) + ". ";
                    cname += fName;
                    //dr["deceasedName"] = cname;

                    gender = dx.Rows[0]["sex"].ObjToString();
                    gender = G1.force_lower_line(gender);
                    //dr["gender"] = gender;

                    contractNumber = dx.Rows[0]["contractNumber"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(contractNumber))
                    {

                        //dr["contractNumber"] = contractNumber;

                        oldUrn = dt.Rows[i]["urn"].ObjToString();

                        GetUrn(contractNumber, ref urn, ref price);

                        if (oldUrn != urn)
                        {
                            dt.Rows[i]["urn"] = urn;
                            dt.Rows[i]["mod"] = "Y";
                            modified = true;
                            //dr["price"] = price;
                            //dr["price"] = CrematoryCharge;
                        }
                    }
                }
            }
            if ( modified )
            {
                btnSaveAll.Show();
                btnSaveAll.Refresh();
            }
            return modified;
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
            if ( dgv2.Visible )
                printableComponentLink1.Component = dgv2;

            printableComponentLink1.PrintingSystemBase = printingSystem1;
            printableComponentLink1.CreateDetailHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateDetailHeaderArea);
            printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);
            printableComponentLink1.BeforeCreateAreas += new System.EventHandler(this.printableComponentLink1_BeforeCreateAreas);
            printableComponentLink1.AfterCreateAreas += new System.EventHandler(this.printableComponentLink1_AfterCreateAreas);
            printableComponentLink1.Landscape = true;

            //Printer.setupPrinterMargins(50, 100, 110, 50);

            Printer.setupPrinterMargins(10, 10, 90, 50);


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
            if (dgv2.Visible)
                printableComponentLink1.Component = dgv2;

            printableComponentLink1.PrintingSystemBase = printingSystem1;
            printableComponentLink1.CreateDetailHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateDetailHeaderArea);
            printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);
            printableComponentLink1.BeforeCreateAreas += new System.EventHandler(this.printableComponentLink1_BeforeCreateAreas);
            printableComponentLink1.AfterCreateAreas += new System.EventHandler(this.printableComponentLink1_AfterCreateAreas);
            printableComponentLink1.Landscape = true;

            //Printer.setupPrinterMargins(50, 100, 110, 50);
            Printer.setupPrinterMargins(10, 10, 90, 50);

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
            Printer.DrawQuad(1, 1, Printer.xQuads, 2, "South Mississippi Cremations, LLC", Color.Black, BorderSide.Top, font, HorizontalAlignment.Center);

            Printer.SetQuadSize(12, 12);

            font = new Font("Ariel", 6);
            Printer.DrawGridDate(2, 3, 2, 3, Color.Black, BorderSide.None, font);
            Printer.DrawGridPage(11, 3, 2, 3, Color.Black, BorderSide.None, font);

            //            Printer.DrawQuad(1, 9, 2, 3, "User : " + LoginForm.username, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);

            font = new Font("Ariel", 12, FontStyle.Regular);
            string title = lblTitle.Text.Trim();
            if (String.IsNullOrWhiteSpace(title))
                title = this.Text;
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

            DateTime date = this.dateTimePicker1.Value;
            string workDate = date.ToString("MM/dd/yyyy") + " - ";
            date = this.dateTimePicker2.Value;
            workDate += date.ToString("MM/dd/yyyy");

            Printer.SetQuadSize(24, 12);
            font = new Font("Ariel", 9, FontStyle.Regular);
            Printer.DrawQuad(18, 7, 10, 4, "Log Dates :" + workDate, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);


            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /***********************************************************************************************/
        private void pictureBox12_Click(object sender, EventArgs e)
        { // Add New Row
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dRow = dt.NewRow();
            dRow["crematory_charge"] = CrematoryCharge;
            dt.Rows.Add(dRow);
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            gridMain.RefreshData();
            gridMain_CellValueChanged(null, null);
        }
        /****************************************************************************************/
        private void picRowUp_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt.Rows.Count <= 0)
                return;
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowHandle = gridMain.FocusedRowHandle;
            if (rowHandle == 0)
                return; // Already at the first row
            //MoveRowUp(dt, rowHandle);
            massRowsUp(dt, rowHandle);
            dt.AcceptChanges();
            dgv.DataSource = dt;
            gridMain.ClearSelection();
            gridMain.SelectRow(rowHandle - 1);
            gridMain.FocusedRowHandle = rowHandle - 1;
            gridMain.RefreshData();
            dgv.Refresh();
            gridMain_CellValueChanged(null, null);
        }
        /***********************************************************************************************/
        private void massRowsUp(DataTable dt, int row)
        {
            int[] rows = gridMain.GetSelectedRows();
            int firstRow = 0;
            if (rows.Length > 0)
                firstRow = rows[0];
            try
            {
                G1.NumberDataTable(dt);
                dt.Columns.Add("Count", Type.GetType("System.Int32"));
                for (int i = 0; i < dt.Rows.Count; i++)
                    dt.Rows[i]["Count"] = i.ToString();
                int moverow = rows[0];
                for (int i = 0; i < rows.Length; i++)
                {
                    row = rows[i];
                    dt.Rows[row]["Count"] = (row - 1).ToString();
                    //dt.Rows[row - 1]["Count"] = row.ToString();
                    //var dRow = gridMain.GetDataRow(row);
                    dt.Rows[row]["mod"] = "Y";
                    modified = true;
                }
                dt.Rows[moverow - 1]["Count"] = (moverow + (rows.Length - 1)).ToString();
                dt.Rows[moverow - 1]["mod"] = "Y";
                G1.sortTable(dt, "Count", "asc");
                dt.Columns.Remove("Count");
                G1.NumberDataTable(dt);
            }
            catch (Exception ex)
            {
                MessageBox.Show("*ERROR*** " + ex.Message.ToString());
            }

            //            gridMain.FocusedRowHandle = firstRow;
            gridMain.SelectRow(firstRow);
            dgv.DataSource = dt;
        }
        /***********************************************************************************************/
        private void picRowDown_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt.Rows.Count <= 0)
                return;
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowHandle = gridMain.FocusedRowHandle;
            if (rowHandle == (dt.Rows.Count - 1))
                return; // Already at the last row
            MoveRowDown(dt, rowHandle);
            dt.AcceptChanges();
            dgv.DataSource = dt;
            gridMain.ClearSelection();
            gridMain.SelectRow(rowHandle + 1);
            gridMain.FocusedRowHandle = rowHandle + 1;
            gridMain.RefreshData();
            dgv.Refresh();
            gridMain_CellValueChanged(null, null);
        }
        /***************************************************************************************/
        private void MoveRowDown(DataTable dt, int row)
        {
            dt.Columns.Add("Count", Type.GetType("System.Int32"));
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["Count"] = i.ToString();
            dt.Rows[row]["Count"] = (row + 1).ToString();
            dt.Rows[row]["mod"] = "Y";
            dt.Rows[row + 1]["Count"] = row.ToString();
            dt.Rows[row + 1]["mod"] = "Y";
            G1.sortTable(dt, "Count", "asc");
            dt.Columns.Remove("Count");
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["Num"] = (i + 1).ToString();
        }
        /***********************************************************************************************/
        private void btnInsert_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt.Rows.Count <= 0)
                return;
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowHandle = gridMain.FocusedRowHandle;
            int dtRow = gridMain.GetDataSourceRowIndex(rowHandle);
            if (dtRow < 0 || dtRow > (dt.Rows.Count - 1))
                return;
            //if (rowHandle == (dt.Rows.Count - 1))
            //    return; // Already at the last row
            DataRow dRow = dt.NewRow();
            dt.Rows.InsertAt(dRow, dtRow);
            G1.NumberDataTable(dt);
            dt.AcceptChanges();
            dgv.DataSource = dt;
            gridMain.ClearSelection();
            gridMain.RefreshData();
            gridMain.FocusedRowHandle = rowHandle + 1;
            gridMain.SelectRow(rowHandle + 1);
            dgv.Refresh();
            gridMain_CellValueChanged(null, null);
        }
        /****************************************************************************************/
        private void pictureBox11_Click(object sender, EventArgs e)
        { // Delete Current Row
            DataRow dr = gridMain.GetFocusedDataRow();
            string data = dr["date_received"].ObjToString();
            if ( !String.IsNullOrWhiteSpace ( data ))
                data = DetermineSaveDate(dr["date_received"].ObjToString().ObjToDateTime());

            DialogResult result;
            if ( String.IsNullOrWhiteSpace ( data ) )
                result = MessageBox.Show("***Question*** Are you sure you want to DELETE this Cremation Row ?", "Delete Cremation Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            else
                result = MessageBox.Show("***Question*** Are you sure you want to DELETE this Cremation Recevied (" + data + ") ?", "Delete Cremation Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.No)
                return;
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt == null)
                return;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            dr["Mod"] = "D";
            dt.Rows[row]["mod"] = "D";
            gridMain_CellValueChanged(null, null);
        }
        /****************************************************************************************/
        private void UpdateMod ( DataRow dr )
        {
            dr["mod"] = "Y";
            modified = true;
            btnSaveAll.Show();
            btnSaveAll.Refresh();
        }
        /****************************************************************************************/
        private bool ValidateAllData ( DataTable dt )
        {
            bool valid = true;
            string mod = "";
            DateTime dateReceived = DateTime.Now;

            for ( int i=0; i<dt.Rows.Count; i++)
            {
                mod = dt.Rows[i]["mod"].ObjToString();
                if (mod == "X") // Already deleted
                    continue;
                if (mod == "D")
                    continue;
                if (mod != "Y")
                    continue;
                dateReceived = dt.Rows[i]["date_received"].ObjToDateTime();
                if ( dateReceived.Year < 30 )
                {
                    MessageBox.Show("*** ERROR *** At least one of the rows has an invalid Date Received!", "Cremation Log Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    valid = false;
                    break;
                }
            }
            return valid;
        }
        /****************************************************************************************/
        private void btnSaveAll_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            string record = "";
            string mod = "";
            string dateStr = "";
            string contractNumber = "";
            string crematory_operator = "";
            string smc_cremation_number = "";
            string funeral_home_name = "";
            string funeral_home_number = "";
            string deceasedName = "";
            string date_received = "";
            string time_received = "";
            string date_placed_in_cooler = "";
            string time_placed_in_cooler = "";
            string container_type = "";
            string date_cremation_started = "";
            string time_cremation_started = "";
            string date_cremation_completed = "";
            string time_cremation_completed = "";
            string gender = "";
            string weight = "";
            string urn = "";
            double crematory_charge = 0D;
            double price = 0D;
            DateTime date = DateTime.Now;

            if (!ValidateAllData(dt))
                return;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                record = dt.Rows[i]["record"].ObjToString();
                mod = dt.Rows[i]["mod"].ObjToString();
                if (mod == "X") // Already deleted
                    continue;
                if (mod == "D")
                {
                    if (!String.IsNullOrWhiteSpace(record))
                        G1.delete_db_table("cremation_log", "record", record);
                    dt.Rows[i]["mod"] = "X";
                    continue;
                }
                if (mod != "Y")
                    continue;
                if (String.IsNullOrWhiteSpace(record))
                    record = G1.create_record("cremation_log", "gender", "-1");
                if (G1.BadRecord("cremation_log", record))
                    continue;

                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();

                crematory_operator = dt.Rows[i]["crematory_operator"].ObjToString();
                smc_cremation_number = dt.Rows[i]["smc_cremation_number"].ObjToString();
                funeral_home_name = dt.Rows[i]["funeral_home_name"].ObjToString();
                funeral_home_number = dt.Rows[i]["funeral_home_number"].ObjToString();
                deceasedName = dt.Rows[i]["deceasedName"].ObjToString();
                date_received = DetermineSaveDate(dt.Rows[i]["date_received"].ObjToString().ObjToDateTime());
                time_received = dt.Rows[i]["time_received"].ObjToString();
                date_placed_in_cooler = DetermineSaveDate( dt.Rows[i]["date_placed_in_cooler"].ObjToString().ObjToDateTime());
                time_placed_in_cooler = dt.Rows[i]["time_placed_in_cooler"].ObjToString();
                container_type = dt.Rows[i]["container_type"].ObjToString();
                date_cremation_started = DetermineSaveDate(dt.Rows[i]["date_cremation_started"].ObjToString().ObjToDateTime());
                time_cremation_started = dt.Rows[i]["time_cremation_started"].ObjToString();
                date_cremation_completed = DetermineSaveDate(dt.Rows[i]["date_cremation_completed"].ObjToString().ObjToDateTime());
                time_cremation_completed = dt.Rows[i]["time_cremation_completed"].ObjToString();
                gender = dt.Rows[i]["gender"].ObjToString();
                weight = dt.Rows[i]["weight"].ObjToString();

                price = dt.Rows[i]["price"].ObjToDouble();
                crematory_charge = dt.Rows[i]["crematory_charge"].ObjToDouble();
                price = crematory_charge;
                urn = dt.Rows[i]["urn"].ObjToString();

                G1.update_db_table("cremation_log", "record", record, new string[] { "contractNumber", contractNumber, "crematory_operator", crematory_operator, "smc_cremation_number", smc_cremation_number, "funeral_home_name", funeral_home_name, "funeral_home_number", funeral_home_number, "deceasedName", deceasedName, "container_type", container_type, "gender", gender, "weight", weight });
                G1.update_db_table("cremation_log", "record", record, new string[] { "date_received", date_received, "date_placed_in_cooler", date_placed_in_cooler, "date_cremation_started", date_cremation_started, "date_cremation_completed", date_cremation_completed, "crematory_charge", crematory_charge.ToString(), "price", price.ToString(), "urn", urn, "order", i.ToString() });
                G1.update_db_table("cremation_log", "record", record, new string[] { "time_received", time_received, "time_placed_in_cooler", time_placed_in_cooler, "time_cremation_started", time_cremation_started, "time_cremation_completed", time_cremation_completed });
            }
            modified = false;
            btnSaveAll.Hide();
        }
        /****************************************************************************************/
        private string DetermineSaveDate ( DateTime date )
        {
            string saveDate = "";
            if (date.Year < 30)
                return "";
            saveDate = date.ToString("yyyyMMddHHmmss");
            return saveDate;
        }
        /****************************************************************************************/
        private void gridMain_ValidatingEditor(object sender, DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventArgs e)
        {
            GridView gridView = sender as GridView;
            string[] Lines = null;
            DateTime date = DateTime.Now;
            string name = gridView.FocusedColumn.FieldName;
            oldColumn = name;

            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridView.GetFocusedDataRow();
            int rowhandle = gridView.FocusedRowHandle;
            int row = gridView.GetDataSourceRowIndex(rowhandle);
            string data = dr[name].ObjToString();
            if (!String.IsNullOrWhiteSpace(data))
            {
                if (name == "date_placed_in_cooler")
                {
                    oldWhat = data;
                }
            }
        }
        /****************************************************************************************/
        private string oldWhat = "";
        private string oldColumn = "";
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
            if (name == "date_placed_in_cooler")
                doDate = true;
            else if (name == "date_received")
                doDate = true;
            else if (name == "date_cremation_started")
                doDate = true;
            else if (name == "date_cremation_completed")
                doDate = true;

            if ( doDate )
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
                        catch ( Exception ex)
                        {
                        }
                        //dr[name] = G1.DTtoMySQLDT(myDate);
                        UpdateMod(dr);
                    }
                }
            }
            gridMain.RefreshData();
            gridMain.RefreshEditor(true);
        }
        /****************************************************************************************/
        private void gridMain_CellValueChanging(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            GridColumn currCol = gridMain.FocusedColumn;
            DataRow dr = gridMain.GetFocusedDataRow();
            string name = currCol.FieldName;
            string record = "";
            string str = "";
            DateTime date = DateTime.Now;
            oldColumn = name;
        }
        /****************************************************************************************/
        private bool decodeDateTime(string dateStr, ref DateTime dateOut, ref string strDate )
        {
            string str = "";
            string[] Lines = null;
            DateTime date = DateTime.Now;
            Lines = dateStr.Split(' ');
            if (Lines.Length <= 1)
                return false;
            if (!G1.validate_date(Lines[0]))
                return false;
            date = Lines[0].ObjToDateTime();
            str = Lines[1].Trim();
            if (String.IsNullOrWhiteSpace(str))
                return false;
            int hour = 0;
            int min = 0;
            int sec = 0;
            strDate = "";
            Lines = str.Split(':');
            if ( Lines.Length > 0 )
            {
                bool addHours = false;
                if (Lines[0].Trim().ToUpper().IndexOf("AM") > 0)
                    Lines[0] = Lines[0].ToUpper().Replace("AM", "");

                if ( Lines[0].Trim().ToUpper().IndexOf ( "PM") > 0 )
                {
                    Lines[0] = Lines[0].ToUpper().Replace("PM", "");
                    addHours = true;
                }
                hour = Lines[0].ObjToInt32();
                if (addHours)
                    hour += 12;
                if (hour < 0 || hour > 23)
                    return false;
                if ( Lines.Length > 1 )
                {
                    min = Lines[1].ObjToInt32();
                    if (min < 0 || min > 59)
                        return false;
                    if ( Lines.Length > 2 )
                    {
                        sec = Lines[2].ObjToInt32();
                        if (sec < 0 || sec > 59)
                            return false;
                    }
                }
            }

            dateOut = new DateTime(date.Year, date.Month, date.Day, hour, min, sec);
            if (sec > 0)
                strDate = dateOut.ToString("MM/dd/yyyy HH:mm:ss");
            else
                strDate = dateOut.ToString("MM/dd/yyyy HH:mm");
            return true;
        }
        /****************************************************************************************/
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (loading)
                return;

            GridColumn currCol = gridMain.FocusedColumn;
            DataRow dr = gridMain.GetFocusedDataRow();
            string name = currCol.FieldName;
            string record = "";
            string str = "";
            string dateStr = "";
            string strDate = "";
            string[] Lines = null;
            DateTime date = DateTime.Now;
            str = dr[name].ObjToString();
            bool doDate = false;
            string urn = "";
            double price = 0D;
            string contractNumber = "";

            if ( name == "funeral_home_number" && !String.IsNullOrWhiteSpace ( str ) )
            {
                string cmd = "Select * from `fcust_extended` e JOIN `fcustomers` c ON e.`contractNumber` = c.`contractNumber` WHERE e.`serviceId` = '" + str + "';";
                DataTable dx = G1.get_db_data(cmd);
                if ( dx.Rows.Count > 0 )
                {
                    string cname = "";
                    string fName = dx.Rows[0]["firstName"].ObjToString();
                    string lName = dx.Rows[0]["lastName"].ObjToString();
                    cname = lName.Trim() + ", ";
                    string middleName = dx.Rows[0]["middleName"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(middleName))
                        cname += middleName.Substring(0, 1) + ". ";
                    cname += fName;
                    dr["deceasedName"] = cname;

                    string gender = dx.Rows[0]["sex"].ObjToString();
                    gender = G1.force_lower_line ( gender );
                    dr["gender"] = gender;

                    contractNumber = dx.Rows[0]["contractNumber"].ObjToString();

                    dr["contractNumber"] = contractNumber;

                    GetUrn(contractNumber, ref urn, ref price);

                    dr["urn"] = urn;
                    //dr["price"] = price;
                    dr["price"] = CrematoryCharge;
                    dr["mod"] = "Y";
                    DateTime deceasedDate = dx.Rows[0]["deceasedDate"].ObjToDateTime();
                    dr["deceasedDate"] = G1.DTtoMySQLDT(deceasedDate);
                    btnSaveAll.Show();
                    btnSaveAll.Refresh();
                }
            }

            modified = true;
            dr["mod"] = "Y";
            btnSaveAll.Show();
            btnSaveAll.Refresh();
        }
        /****************************************************************************************/
        private void Main_CellValueChanged( int row, int col, string str )
        {
            GridColumn currCol = gridMain.Columns[col];
            DataRow dr = gridMain.GetDataRow(row);
            string name = currCol.FieldName;
            string record = "";
            string dateStr = "";
            string strDate = "";
            string[] Lines = null;
            DateTime date = DateTime.Now;
            dr[name] = str;
            //str = dr[name].ObjToString();
            bool doDate = false;
            string urn = "";
            double price = 0D;
            string contractNumber = "";

            if (name == "funeral_home_number" && !String.IsNullOrWhiteSpace(str))
            {
                string cmd = "Select * from `fcust_extended` e JOIN `fcustomers` c ON e.`contractNumber` = c.`contractNumber` WHERE e.`serviceId` = '" + str + "';";
                DataTable dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    string cname = "";
                    string fName = dx.Rows[0]["firstName"].ObjToString();
                    string lName = dx.Rows[0]["lastName"].ObjToString();
                    cname = lName.Trim() + ", ";
                    string middleName = dx.Rows[0]["middleName"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(middleName))
                        cname += middleName.Substring(0, 1) + ". ";
                    cname += fName;
                    dr["deceasedName"] = cname;

                    string gender = dx.Rows[0]["sex"].ObjToString();
                    gender = G1.force_lower_line(gender);
                    dr["gender"] = gender;

                    contractNumber = dx.Rows[0]["contractNumber"].ObjToString();

                    dr["contractNumber"] = contractNumber;

                    GetUrn(contractNumber, ref urn, ref price);

                    dr["urn"] = urn;
                    //dr["price"] = price;
                    dr["price"] = CrematoryCharge;
                    dr["mod"] = "Y";
                    btnSaveAll.Show();
                    btnSaveAll.Refresh();
                }
            }

            modified = true;
            dr["mod"] = "Y";
            btnSaveAll.Show();
            btnSaveAll.Refresh();
        }
        /****************************************************************************************/
        private void GetUrn ( string contractNumber, ref string urn, ref double price )
        {
            urn = "";
            price = 0D;
            string service = "";
            string what = "";
            string type = "";
            string cmd = "Select * from `fcust_services` where `contractNumber` = '" + contractNumber + "';";
            DataTable dt = G1.get_db_data(cmd);
            if ( dt.Rows.Count > 0 )
            {
                for ( int i=0; i<dt.Rows.Count; i++)
                {
                    type = dt.Rows[i]["type"].ObjToString();
                    service = dt.Rows[i]["service"].ObjToString();
                    what = FunServices.isWhatMerchandise(service);
                    if (what == "Urn")
                    {
                        urn = service;
                        price = dt.Rows[i]["price"].ObjToDouble();
                    }
                    if (!String.IsNullOrWhiteSpace(urn) && price > 0D)
                        break;
                }
            }
        }
        /****************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string serviceId = dr["funeral_home_number"].ObjToString();
            string cmd = "Select * from `fcust_extended` e JOIN `fcustomers` c ON e.`contractNumber` = c.`contractNumber` WHERE e.`serviceId` = '" + serviceId + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                string contractNumber = dx.Rows[0]["contractNumber"].ObjToString();
                EditCust custForm = new EditCust(contractNumber);
                custForm.Tag = contractNumber;
                custForm.Show();
            }
        }
        /****************************************************************************************/
        private void southMississippiCremationsReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataTable dt2 = dt.Copy();
            dgv2.DataSource = dt;

            dgv.Hide();
            dgv.Refresh();
            dgv2.Show();
            dgv2.Refresh();

            gridMain2.Columns["date_cremation_completed"].Visible = false;
            gridMain2.Columns["days"].Visible = false;

            btnRun.Text = "Go Back";
            btnRun.BackColor = Color.LightGreen;

            ToolStripMenuItem menu = (ToolStripMenuItem)sender;
            lblTitle.Text = menu.Text;

            SetButtons(false);
        }
        /****************************************************************************************/
        private void repositoryItemComboBox7_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnSaveAll.Show();
            btnSaveAll.Refresh();
        }
        /****************************************************************************************/
        private void chkComboLocation_EditValueChanged(object sender, EventArgs e)
        {
            if (loading)
                return;

            loadCrematoryOperators();

            if (!CheckDataModified())
                return;

            //bool modified = LoadData(this.dateTimePicker1.Value, this.dateTimePicker2.Value);

            btnRun_Click(null, null);

            loading = false;

            SetButtons(true);


            //if (!modified)
            //{
            //    btnSaveAll.Hide();
            //    btnSaveAll.Refresh();
            //}
        }
        /****************************************************************************************/
        private void outstandingCremationsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menu = (ToolStripMenuItem)sender;
            lblTitle.Text = menu.Text;


            DataTable dt = (DataTable)dgv.DataSource;

            if ( G1.get_column_number ( dt, "days") < 0 )
                dt.Columns.Add("days", Type.GetType("System.Double"));

            DataRow dR = null;

            btnRun.Text = "Go Back";
            btnRun.BackColor = Color.LightGreen;

            DateTime deceasedDate = DateTime.Now;
            DateTime finishDate = DateTime.Now;
            TimeSpan ts;

            string str = txtOutstanding.Text.Trim();
            if (String.IsNullOrWhiteSpace(str))
                str = "4";
            double outDays = str.ObjToDouble();

            for ( int i=0; i<dt.Rows.Count; i++)
            {
                deceasedDate = dt.Rows[i]["deceasedDate"].ObjToDateTime();
                if (deceasedDate.Year < 100)
                    deceasedDate = dt.Rows[i]["date_received"].ObjToDateTime();
                finishDate = dt.Rows[i]["date_cremation_completed"].ObjToDateTime();
                if (finishDate.Year < 100)
                    finishDate = DateTime.Now;
                ts = finishDate - deceasedDate;
                if ( ts.TotalDays > outDays )
                {
                    //G1.copy_dt_row(dt, i, dt2, dt2.Rows.Count);
                    dt.Rows[dt.Rows.Count - 1]["days"] = ts.TotalDays;
                }
            }

            gridMain2.Columns["date_cremation_completed"].Visible = true;
            gridMain2.Columns["days"].Visible = true;

            dgv2.DataSource = dt;

            dgv.Hide();
            dgv.Refresh();
            dgv2.Show();
            dgv2.Refresh();

            SetButtons(false);
        }
        /****************************************************************************************/
        private void gridMain2_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            if (e.ListSourceRowIndex == DevExpress.XtraGrid.GridControl.InvalidRowHandle)
                return;
            string name = e.Column.FieldName;
            bool doDate = false;
            bool doTime = false;
            if (name == "date_placed_in_cooler")
                doDate = true;
            else if (name == "date_received")
                doDate = true;
            else if (name == "date_cremation_started")
                doDate = true;
            else if (name == "date_cremation_completed")
                doDate = true;
            else if (name == "deceasedDate")
                doDate = true;

            if (name == "time_placed_in_cooler")
                doTime = true;
            else if (name == "time_received")
                doTime = true;
            else if (name == "time_cremation_started")
                doTime = true;
            else if (name == "time_cremation_completed")
                doTime = true;

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
        private void gridMain2_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv2.DataSource;
            if (dt == null)
                return;
            string delete = dt.Rows[row]["mod"].ObjToString();
            if (delete.ToUpper() == "D")
            {
                e.Visible = false;
                e.Handled = true;
            }
            else if (delete.ToUpper() == "X")
            {
                e.Visible = false;
                e.Handled = true;
            }
        }
        /****************************************************************************************/
        private void SetButtons ( bool visible )
        {
            pictureBox12.Visible = visible;
            pictureBox12.Refresh();
            picRowUp.Visible = visible;
            picRowUp.Refresh();
            picRowDown.Visible = visible;
            picRowDown.Refresh();
            btnInsert.Visible = visible;
            btnInsert.Refresh();
            pictureBox11.Visible = visible;
            pictureBox11.Refresh();
        }
        /****************************************************************************************/
        private void gridMain2_CellValueChanging(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            GridColumn currCol = gridMain2.FocusedColumn;
            DataRow dr = gridMain2.GetFocusedDataRow();
            string name = currCol.FieldName;
            string record = "";
            string str = "";
            DateTime date = DateTime.Now;
            oldColumn = name;
        }
        /****************************************************************************************/

        private void gridMain2_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (loading)
                return;

            GridColumn currCol = gridMain2.FocusedColumn;
            DataRow dr = gridMain2.GetFocusedDataRow();
            DataTable dt = (DataTable)dgv2.DataSource;
            string name = currCol.FieldName;
            string record = "";
            string str = "";
            string dateStr = "";
            string strDate = "";
            string[] Lines = null;
            DateTime date = DateTime.Now;
            str = dr[name].ObjToString();
            bool doDate = false;
            string urn = "";
            double price = 0D;
            string contractNumber = "";

            if (name == "funeral_home_number" && !String.IsNullOrWhiteSpace(str))
            {
                string cmd = "Select * from `fcust_extended` e JOIN `fcustomers` c ON e.`contractNumber` = c.`contractNumber` WHERE e.`serviceId` = '" + str + "';";
                DataTable dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    string cname = "";
                    string fName = dx.Rows[0]["firstName"].ObjToString();
                    string lName = dx.Rows[0]["lastName"].ObjToString();
                    cname = lName.Trim() + ", ";
                    string middleName = dx.Rows[0]["middleName"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(middleName))
                        cname += middleName.Substring(0, 1) + ". ";
                    cname += fName;
                    dr["deceasedName"] = cname;

                    string gender = dx.Rows[0]["sex"].ObjToString();
                    gender = G1.force_lower_line(gender);
                    dr["gender"] = gender;

                    contractNumber = dx.Rows[0]["contractNumber"].ObjToString();

                    dr["contractNumber"] = contractNumber;

                    GetUrn(contractNumber, ref urn, ref price);

                    dr["urn"] = urn;
                    //dr["price"] = price;
                    dr["price"] = CrematoryCharge;
                    dr["mod"] = "Y";
                    DateTime deceasedDate = dx.Rows[0]["deceasedDate"].ObjToDateTime();
                    dr["deceasedDate"] = G1.DTtoMySQLDT(deceasedDate);
                    btnSaveAll.Show();
                    btnSaveAll.Refresh();
                }
            }

            modified = true;
            dr["mod"] = "Y";
            dt.AcceptChanges();

            gridMain2.RefreshEditor(true);
            dgv2.RefreshDataSource();
            btnSaveAll.Show();
            btnSaveAll.Refresh();

            oldColumn = name;
            dgv.DataSource = (DataTable) dgv2.DataSource;
        }
        /****************************************************************************************/
        private void gridMain2_ShownEditor(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv2.DataSource;
            int row = gridMain2.FocusedRowHandle;

            GridColumn currCol = gridMain2.FocusedColumn;
            DataRow dr = gridMain2.GetFocusedDataRow();
            string name = currCol.FieldName;
            string record = "";
            string str = "";
            DateTime myDate = DateTime.Now;
            oldColumn = name;

            bool doDate = false;
            if (name == "date_placed_in_cooler")
                doDate = true;
            else if (name == "date_received")
                doDate = true;
            else if (name == "date_cremation_started")
                doDate = true;
            else if (name == "date_cremation_completed")
                doDate = true;

            if (doDate)
            {
                myDate = dr[name].ObjToDateTime();
                str = gridMain2.Columns[name].Caption;
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
                    }
                }
            }

            dgv.DataSource = (DataTable) dgv2.DataSource;
            gridMain2.RefreshData();
            gridMain2.RefreshEditor(true);
        }
        /****************************************************************************************/
        private void gridMain2_ValidatingEditor(object sender, DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventArgs e)
        {
            GridView gridView = sender as GridView;
            string[] Lines = null;
            DateTime date = DateTime.Now;
            string name = gridView.FocusedColumn.FieldName;
            oldColumn = name;

            DataTable dt = (DataTable)dgv2.DataSource;
            DataRow dr = gridView.GetFocusedDataRow();
            int rowhandle = gridView.FocusedRowHandle;
            int row = gridView.GetDataSourceRowIndex(rowhandle);
            string data = dr[name].ObjToString();
            if (!String.IsNullOrWhiteSpace(data))
            {
                if (name == "date_placed_in_cooler")
                {
                    oldWhat = data;
                }
            }
        }
        /****************************************************************************************/
        private void repositoryItemComboBox8_SelectedIndexChanged(object sender, EventArgs e)
        {
        }
        /****************************************************************************************/
        private void cmbSelectColumns_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            System.Windows.Forms.ComboBox combo = (System.Windows.Forms.ComboBox)sender;
            string comboName = combo.Text;
            string skinName = "";
            if (!String.IsNullOrWhiteSpace(comboName))
            {
                SetupSelectedColumns("UrnLog", comboName, dgv);
                string name = "UrnLog " + comboName;
                foundLocalPreference = G1.RestoreGridLayout(this, this.dgv, gridMain, LoginForm.username, name, ref skinName);
            }
            else
            {
                SetupSelectedColumns("UrnLog", "Primary", dgv);
                string name = "UrnLog Primary";
                foundLocalPreference = G1.RestoreGridLayout(this, this.dgv, gridMain, LoginForm.username, name, ref skinName);
            }
        }
        /***********************************************************************************************/
        private void SetupSelectedColumns()
        {
            string group = cmbSelectColumns.Text.Trim().ToUpper();
            if (group.Trim().Length == 0)
                return;
            string cmd = "Select * from procfiles where name = '" + group + "' and ProcType = 'UrnLog' order by seq";
            DataTable dt = G1.get_db_data(cmd);
            for (int i = 0; i < gridMain.Columns.Count; i++)
                gridMain.Columns[i].Visible = false;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string name = dt.Rows[i]["Description"].ToString();
                int index = dt.Rows[i]["seq"].ObjToInt32();
                try
                {
                    ((GridView)dgv.MainView).Columns[name].Visible = true;
                }
                catch
                {
                }
            }
        }
        /***********************************************************************************************/
        private void SetupSelectedColumns(string procType, string group, DevExpress.XtraGrid.GridControl dgv)
        {
            if (String.IsNullOrWhiteSpace(group))
                return;
            if (String.IsNullOrWhiteSpace(procType))
                procType = "UrnLog";
            string cmd = "Select * from procfiles where name = '" + group + "' and ProcType = '" + procType + "' order by seq";
            DataTable dt = G1.get_db_data(cmd);
            DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gridMain = (DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView)dgv.MainView;
            for (int i = 0; i < gridMain.Columns.Count; i++)
                gridMain.Columns[i].Visible = false;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string name = dt.Rows[i]["Description"].ToString();
                int index = dt.Rows[i]["seq"].ObjToInt32();
                try
                {
                    ((GridView)dgv.MainView).Columns[name].Visible = true;
                }
                catch
                {
                }
            }
        }
        /***********************************************************************************************/
        private void loadGroupCombo(System.Windows.Forms.ComboBox cmb, string key, string module)
        {
            string primaryName = "";
            cmb.Items.Clear();
            string cmd = "Select * from procfiles where ProcType = '" + key + "' AND `module` = '" + module + "' group by name;";
            DataTable dt = G1.get_db_data(cmd);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string name = dt.Rows[i]["Name"].ToString();
                if (name.Trim().ToUpper() == "PRIMARY")
                    primaryName = name;
                cmb.Items.Add(name);
            }
            if (!String.IsNullOrWhiteSpace(primaryName))
                cmb.Text = primaryName;
        }
        /****************************************************************************************/
        private void lockScreenFormatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string name = cmbSelectColumns.Text.Trim();
            if (String.IsNullOrWhiteSpace(name))
                name = "Primary";
            string saveName = "UrnLog " + name;
            G1.SaveLocalPreferences(this, gridMain, LoginForm.username, saveName);
        }
        /****************************************************************************************/
        private void unlockScreenFormatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string comboName = cmbSelectColumns.Text;
            if (!String.IsNullOrWhiteSpace(comboName))
            {
                string name = "UrnLog " + comboName;
                G1.RemoveLocalPreferences(LoginForm.username, name);
                foundLocalPreference = false;
            }
        }
        /****************************************************************************************/
        private void btnSelectColumns_Click(object sender, EventArgs e)
        {
            string actualName = cmbSelectColumns.Text;
            SelectColumns sform = new SelectColumns(dgv, "UrnLog", "Primary", actualName);
            sform.Done += new SelectColumns.d_void_eventdone(sform_Done);
            sform.Show();
        }
        /***********************************************************************************************/
        void sform_Done()
        {
            dgv.Refresh();
            this.Refresh();
        }
        /****************************************************************************************/
    }
}