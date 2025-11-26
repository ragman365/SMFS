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
using DevExpress.XtraGrid.Views.Base;
using DevExpress.Xpo.Helpers;
using System.IO;
using ExcelLibrary.BinaryFileFormat;
using System.Security.Cryptography;
using DevExpress.XtraPrinting;
using DevExpress.Utils;
using MySql.Data.MySqlClient;
using System.Web.UI.WebControls;
using DevExpress.XtraGrid.Columns;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class ImportMuseContacts : DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        /* Forethough - Import All Active Data. This code determines Pre or Post */
        /* FDLIC - Import All - FDLIC PB is included all together
        /***********************************************************************************************/
        private string workTable = "contacts_preneed_mapping";
        private string workMap = "preneedMuse";
        private DataTable workTableDt = null;
        private DataTable existingDt = null;

        private DataTable workDt = null;
        private string workWhat = "";
        private bool workDC = false;
        private string title = "";

        private DataTable problemDt = null;
        /***********************************************************************************************/
        public ImportMuseContacts()
        {
            InitializeComponent();
            SetupTotalsSummary();
        }
        /****************************************************************************************/
        private void SetupTotalsSummary()
        {
        }
        /****************************************************************************************/
        private void AddSummaryColumn(string columnName, DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gMain = null, string format = "")
        {
            if (gMain == null)
                gMain = gridMain;

            if (String.IsNullOrWhiteSpace(format))
                format = "{0:N2}";
            gMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            gMain.Columns[columnName].SummaryItem.DisplayFormat = format;
        }
        /****************************************************************************************/
        private void AddSummaryColumn(string columnName, string format = "")
        {
            if (String.IsNullOrWhiteSpace(format))
                format = "{0:0,0.00}";
            gridMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            gridMain.Columns[columnName].SummaryItem.DisplayFormat = format;
        }
        /***********************************************************************************************/
        private void ImportMuseContacts_Load(object sender, EventArgs e)
        {
            menuStrip1.Items["editToolStripMenuItem"].Dispose();
            btnSave.Hide();
            barImport.Hide();

            LoadAgents();

            LoadLocations();

            tabControl1.TabPages.Remove(tabPage2);
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
                if (completed == "1")
                    dt.Rows[i]["completed"] = "1";
                else
                    dt.Rows[i]["completed"] = "0";
            }
        }
        /***********************************************************************************************/
        private void LoadLocations()
        {
            string location = "";
            DataTable locDt = null;

            string cmd = "Select * from `users` where `username` = '" + LoginForm.username + "';";
            DataTable usersDt = G1.get_db_data(cmd);
            if (usersDt.Rows.Count > 0 )
            {
                cmbLocation.Items.Add("All");
                string assignedLocations = usersDt.Rows[0]["assignedLocations"].ObjToString();
                if (!String.IsNullOrWhiteSpace(assignedLocations))
                {
                    string[] Lines = assignedLocations.Split('~');
                    for (int i = 0; i < Lines.Length; i++)
                    {
                        location = Lines[i].Trim();
                        if (!String.IsNullOrWhiteSpace(location))
                            cmbLocation.Items.Add(location);
                    }
                }
                else
                {
                    cmd = "Select * from `funeralhomes` order by `LocationCode`;";
                    locDt = G1.get_db_data(cmd);
                    for (int i = 0; i < locDt.Rows.Count; i++)
                    {
                        cmbLocation.Items.Add(locDt.Rows[i]["locationCode"].ObjToString());
                    }
                }
            }
            else
            {
                cmbLocation.Items.Add("All");
                cmd = "Select * from `funeralhomes` order by `LocationCode`;";
                locDt = G1.get_db_data(cmd);
                for (int i = 0; i < locDt.Rows.Count; i++)
                {
                    cmbLocation.Items.Add(locDt.Rows[i]["locationCode"].ObjToString());
                }
                //DataRow dRow = locDt.NewRow();
                //dRow["LocationCode"] = "All";
                //locDt.Rows.InsertAt(dRow, 0);
                //cmbLocation.DataSource = locDt;
            }
            cmbLocation.Text = "All";

            repositoryItemComboBox6.Items.Clear();
            for (int i = 0; i < cmbLocation.Items.Count; i++)
                repositoryItemComboBox6.Items.Add(cmbLocation.Items[i].ObjToString());

        }
        /***********************************************************************************************/
        private void loadRepositoryLocatons()
        {
            string cmd = "Select * from `funeralhomes` order by `keycode`;";
            DataTable locDt = G1.get_db_data(cmd);

            DataTable newLocDt = locDt.Clone();

            string assignedLocations = "";

            string[] Lines = null;

            string newUser = cmbEmployee.Text;

            cmd = "Select * from `users` where `username` = '" + LoginForm.username + "';";
            if (!String.IsNullOrWhiteSpace(newUser))
            {
                DataTable uDt = (DataTable)cmbEmployee.DataSource;
                if (uDt.Rows.Count > 0)
                    newUser = uDt.Rows[0]["username"].ObjToString();
                if (String.IsNullOrWhiteSpace(newUser))
                {
                    Lines = cmbEmployee.Text.Trim().Split(',');
                    if (Lines.Length > 1)
                    {
                        string lastName = Lines[0].Trim();
                        string firstName = Lines[1].Trim();
                        uDt = G1.get_db_data("Select * from `users` WHERE `firstName` = '" + firstName + "' AND `lastName` = '" + lastName + "'");
                        if (uDt.Rows.Count > 0)
                            newUser = uDt.Rows[0]["username"].ObjToString();
                    }
                }
                cmd = "Select * from `users` where `username` = '" + newUser + "';";
            }

            newUser = "";

            DataTable userDt = G1.get_db_data(cmd);
            if (userDt.Rows.Count > 0)
                assignedLocations = userDt.Rows[0]["assignedLocations"].ObjToString();

            string locationCode = "";
            string keyCode = "";
            Lines = null;
            string locations = "";
            string location = "";

            for (int i = locDt.Rows.Count - 1; i >= 0; i--)
            {
                keyCode = locDt.Rows[i]["keycode"].ObjToString();
                if (keyCode.IndexOf("-") > 0)
                    locDt.Rows.RemoveAt(i);
            }
            for (int i = 0; i < locDt.Rows.Count; i++)
            {
                locationCode = locDt.Rows[i]["locationCode"].ObjToString();
                if (String.IsNullOrWhiteSpace(locationCode))
                    continue;
                Lines = assignedLocations.Split('~');
                for (int j = 0; j < Lines.Length; j++)
                {
                    location = Lines[j].Trim();
                    if (String.IsNullOrWhiteSpace(location))
                        continue;
                    if (location.ToUpper() == locationCode.ToUpper())
                    {
                        location = locDt.Rows[i]["atNeedCode"].ObjToString();
                        location = locDt.Rows[i]["LocationCode"].ObjToString();
                        locations += location + "|";
                        newLocDt.ImportRow(locDt.Rows[i]);
                    }
                }
            }
            if (!LoginForm.administrator)
                locDt = newLocDt;

            DataView tempview = locDt.DefaultView;
            //tempview.Sort = "atneedcode";
            tempview.Sort = "LocationCode";
            locDt = tempview.ToTable();

            repositoryItemComboBox6.Items.Add("All");
            for (int i = 0; i < locDt.Rows.Count; i++)
                repositoryItemComboBox6.Items.Add(locDt.Rows[i]["locationCode"].ObjToString());
        }
        /***********************************************************************************************/
        private void LoadAgents()
        {
            string firstName = "";
            string middleName = "";
            string lastName = "";
            string name = "";

            repositoryItemComboBox2.Items.Clear();

            string cmd = "Select * from `agents` WHERE `agentCode` = 'XYZZY'";
            cmd += ";";
            DataTable dt = G1.get_db_data(cmd);

            AddOtherAgents(dt);

            DataView tempview = dt.DefaultView;
            tempview.Sort = "lastName,firstName";
            dt = tempview.ToTable();

            dt.Columns.Add("name");
            dt.Columns.Add("username");

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                firstName = dt.Rows[i]["firstName"].ObjToString();
                //middleName = dt.Rows[i]["middleName"].ObjToString();
                lastName = dt.Rows[i]["lastName"].ObjToString();

                if (String.IsNullOrWhiteSpace(lastName))
                    continue;

                name = lastName;
                if (!String.IsNullOrWhiteSpace(firstName))
                    name += ", " + firstName;
                //if (!String.IsNullOrWhiteSpace(middleName))
                //    name += " " + middleName;

                //cmbEmployee.Items.Add(name);

                repositoryItemComboBox2.Items.Add(name);
                dt.Rows[i]["name"] = name;
            }

            DataRow dR = dt.NewRow();
            dR["name"] = "All";
            dt.Rows.InsertAt(dR, 0);

            cmbEmployee.DataSource = dt;

            cmd = "Select * from `users` where `username` = '" + LoginForm.username + "';";
            DataTable dx = G1.get_db_data(cmd);

            DataRow[] dRows = dx.Select("username='" + LoginForm.username + "'");
            if (dRows.Length > 0 && !G1.isAdminOrSuper())
            {
                firstName = dRows[0]["firstName"].ObjToString();
                //middleName = dRows[0]["middleName"].ObjToString();
                lastName = dRows[0]["lastName"].ObjToString();

                name = lastName;
                if (!String.IsNullOrWhiteSpace(firstName))
                    name += ", " + firstName;
                //if (!String.IsNullOrWhiteSpace(middleName))
                //    name = " " + middleName;

                cmbEmployee.Text = name;
                //primaryName = name;
                //gridMain.Columns["agent"].Visible = false;
                dt.Rows.Clear();
                repositoryItemComboBox2.Items.Clear();
                dR = dt.NewRow();
                dR["name"] = name;
                dR["username"] = dRows[0]["username"].ObjToString();
                dt.Rows.Add(dR);
                cmbEmployee.DataSource = dt;
//                gridMain.Columns["agent"].Visible = false;
                //showAgent = false;
            }
            cmbEmployee.Text = "Muse, Vernon";

            cmd = "Select * from `contacts_preneed` WHERE `agent` = 'Muse, Vernon'";
            cmd += ";";
            existingDt = G1.get_db_data(cmd);
        }
        /***********************************************************************************************/
        private DataTable AddOtherAgents(DataTable dt)
        {
            string cmd = "Select * from `agents`;";
            DataTable agentDt = G1.get_db_data(cmd);

            string firstName = "";
            string lastName = "";
            DataRow[] dRows = null;
            DataRow dRow = null;
            bool found = false;
            for (int i = 0; i < agentDt.Rows.Count; i++)
            {
                firstName = agentDt.Rows[i]["firstName"].ObjToString();
                lastName = agentDt.Rows[i]["lastName"].ObjToString();

                dRows = dt.Select("firstName='" + firstName + "' AND `lastName` = '" + lastName + "'");
                if (dRows.Length <= 0)
                {
                    dRow = dt.NewRow();
                    dRow["firstName"] = firstName;
                    dRow["lastName"] = lastName;
                    dt.Rows.Add(dRow);
                    found = true;
                }
            }

            if (found)
            {
                DataView tempview = dt.DefaultView;
                tempview.Sort = "lastName asc, firstName asc";
                dt = tempview.ToTable();
            }
            return dt;
        }
        /***********************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (dgv.Visible)
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
        private void gridMain_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            ColumnView view = sender as ColumnView;
            if (e.Column.FieldName.ToUpper().IndexOf("DATE") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
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
        }
        /***********************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string contract = dr["contractNumber"].ObjToString();
            //if (!String.IsNullOrWhiteSpace(contract))
            //{
            //    this.Cursor = Cursors.WaitCursor;
            //    string cmd = "Select * from `contracts` where `contractNumber` = '" + contract + "';";
            //    DataTable dx = G1.get_db_data(cmd);
            //    if (dx.Rows.Count <= 0)
            //    {
            //        string cnum = contract.TrimStart('0');
            //        cnum = cnum.Replace(" ", "");

            //        cmd = "Select * from `icustomers` where `payer` = '" + cnum + "';";
            //        dx = G1.get_db_data(cmd);
            //        if (dx.Rows.Count > 0)
            //            contract = dx.Rows[0]["contractNumber"].ObjToString();
            //    }
            //    cmd = "Select * from `customers` where `contractNumber` = '" + contract + "';";
            //    dx = G1.get_db_data(cmd);
            //    if ( dx.Rows.Count <= 0 )
            //    {
            //        MessageBox.Show("***ERROR*** Contract " + contract + "\nDoes Not Have a Customer File!\nBe sure to edit all Demographics", "Customer File Record Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            //    }
            //    CustomerDetails clientForm = new CustomerDetails(contract);
            //    clientForm.Show();
            //    this.Cursor = Cursors.Default;
            //}
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

            font = new Font("Ariel", 12);
            string text = this.Text;
            Printer.DrawQuad(5, 7, 5, 4, text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);

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
        private void gridMain_CellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            DataTable dt = (DataTable)dgv.DataSource;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
        }
        /***********************************************************************************************/
        private void gridMain_CustomDrawCell(object sender, RowCellCustomDrawEventArgs e)
        {
            if (e.RowHandle < 0)
                return;
        }
        /***********************************************************************************************/
        private void btnSave_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv2.DataSource;

            DataTable dx = null;
            string record = "";
            string cmd = "";


            string fields = "";
            string data = "";
            string str = "";
            string field = "";
            string duplicate = "";
            bool addDuplicates = chkDuplicates.Checked;
            string allow = "";

            DateTime date = DateTime.Now;

            string lines = "";

            try
            {
                this.Cursor = Cursors.WaitCursor;

                int rows = dt.Rows.Count;
                //rows = 1;

                barImport.Show();
                barImport.Minimum = 0;
                barImport.Maximum = rows;
                barImport.Value = 0;

                string firstName = "";
                string lastName = "";

                for (int i = 0; i < rows; i++)
                {
                    fields = "";
                    data = "";
                    lines = "";

                    barImport.Value = i;
                    barImport.Refresh();

                    duplicate = dt.Rows[i]["duplicate"].ObjToString();
                    allow = dt.Rows[i]["allow"].ObjToString().ToUpper();

                    if (!addDuplicates)
                    {
                        if (!String.IsNullOrWhiteSpace(duplicate))
                            continue;
                        if (allow == "EXCLUDE")
                            continue;
                    }

                    firstName = dt.Rows[i]["firstName"].ObjToString();
                    lastName = dt.Rows[i]["lastName"].ObjToString();
                    if ( firstName == "Tanya" && lastName == "Wynn")
                    {

                    }

                    if (allow == "REPLACE")
                        record = dt.Rows[i]["duplicateRecord"].ObjToString();
                    else
                        record = G1.create_record("contacts_preneed", "POC", "-1");

                    if (G1.BadRecord("contacts_preneed", record))
                        break;
                    for ( int j=1; j<dt.Columns.Count; j++ )
                    {
                        field = dt.Columns[j].ColumnName.Trim();
                        str = dt.Rows[i][j].ObjToString();
                        if (String.IsNullOrWhiteSpace(str))
                            continue;
                        if (field.ToUpper() == "AGE" && String.IsNullOrWhiteSpace(str))
                            continue;
                        if (field.ToUpper() == "DUPLICATERECORD" || field == "ALLOW")
                            continue;
                        if ( field.ToUpper() == "RECORD")
                        {
                            if ( allow == "REPLACE")
                                record = dt.Rows[i]["duplicateRecord"].ObjToString();
                        }
                        if ( field.ToUpper().IndexOf ( "DATE") >= 0 )
                        {
                            date = str.ObjToDateTime();
                            if (date.Year < 1000)
                                continue;
                        }

                        if (str.IndexOf(",") > 0)
                        {
                            G1.update_db_table("contacts_preneed", "record", record, new string[] { field, str });
                        }
                        else
                        {
                            str = G1.try_protect_data(str);
                            lines += field + "," + str + ",";
                        }
                    }
                    lines += "oldRecord," + record + ",";
                    lines += "POC,";
                    G1.update_db_table("contacts_preneed", "record", record, lines );
                }
                this.Cursor = Cursors.Default;
            }
            catch (Exception ex)
            {
            }

            barImport.Hide();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void CleanupCommas ( DataTable dt, string column )
        {
            string str = "";
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                str = dt.Rows[i][column].ObjToString();
                if ( str.IndexOf ( "$") >= 0 )
                {
                    str = str.Replace("$", "");
                    dt.Rows[i][column] = str;
                }
                if (String.IsNullOrWhiteSpace(str))
                    dt.Rows[i][column] = "0";
                else if ( str.IndexOf ( ",") > 0 )
                {
                    str = str.Replace(",", "");
                    dt.Rows[i][column] = str;
                }
            }
        }
        /***********************************************************************************************/
        private string determineWorkWhat ( string filename, ref string sheetName )
        {
            string search = "";
            sheetName = "";
            filename = filename.ToUpper();
            if (filename.IndexOf(workWhat.ToUpper()) < 0)
                filename = workWhat.ToUpper() + " " + filename;
            if ( filename.IndexOf ( "UNITY") >= 0 )
            {
                search = "FH Name";
                workWhat = "Unity";
                sheetName = "List of all policies";
            }
            else if (filename.IndexOf("FORETHOUGHT") >= 0)
            {
                search = "Insured Last Name";
                workWhat = "Forethought";
            }
            else if (filename.IndexOf("FDLIC") >= 0)
            {
                search = "FH No.";
                search = "Funeral Home";
                workWhat = "FDLIC";
            }
            else if (filename.IndexOf("SECURITY NATIONAL") >= 0)
            {
                search = "TRUST#";
                workWhat = "Security National";
            }
            else if (filename.IndexOf(" CD") >= 0)
            {
                search = "FIRST NAME";
                workWhat = "CD";
            }
            return search;
        }
        /***********************************************************************************************/
        private string actualFile = "";
        private string importedFile = "";
        private void btnImport_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            string sheetName = "";
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string file = ofd.FileName;
                    importedFile = file;
                    int idx = file.LastIndexOf("\\");
                    if (idx > 0)
                    {
                        actualFile = file.Substring(idx);
                        actualFile = actualFile.Replace("\\", "");
                    }

                    dgv.DataSource = null;
                    this.Cursor = Cursors.WaitCursor;
                    workDt = null;

                    try
                    {
                        workDt = Import.ImportCSVfile(file);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("***ERROR*** Reading File!");
                        workDt = null;
                    }

                    workDt.TableName = actualFile;

                    if (G1.get_column_number(workDt, "num") < 0)
                        workDt.Columns.Add("num").SetOrdinal(0);
                    G1.NumberDataTable(workDt);
                    dgv.DataSource = workDt;

                    menuStrip1.Items.Add(editToolStripMenuItem);
                    editToolStripMenuItem.DropDownItems.Add(museInputFileMappingToolStripMenuItem);

                    tabControl1.TabPages.Add (tabPage2);

                }
            }
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private DataTable ProcessTheData ( DataTable dt )
        {
            int firstRow = -1;
            string search = "SMFS AGENT";
            string str = "";
            DataTable newDt = dt.Clone();

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    str = dt.Rows[i][j].ObjToString().ToUpper();
                    if (str == search)
                    {
                        firstRow = i;
                        break;
                    }
                }
                if (firstRow >= 0)
                    break;
            }
            if (firstRow < 0)
                return newDt;
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                str = dt.Rows[firstRow][i].ObjToString();
                if (String.IsNullOrWhiteSpace(str))
                    continue;
                if (G1.get_column_number(dt, str) >= 0)
                {
                    for (; ; )
                    {
                        str = str + "2";
                        if (G1.get_column_number(dt, str) < 0)
                            break;
                    }
                }
                newDt.Columns[i].ColumnName = str;
                newDt.Columns[i].Caption = str;

                dt.Columns[i].ColumnName = str;
                dt.Columns[i].Caption = str;
            }
            for (int i = (firstRow + 1); i < dt.Rows.Count; i++)
            {
                str = dt.Rows[i]["SMFS AGENT"].ObjToString().ToUpper();
                if (String.IsNullOrWhiteSpace(str))
                    continue;
                newDt.ImportRow(dt.Rows[i]);
            }

            newDt = mapTheColumns(newDt);

            newDt = processImportedData(newDt);

            if (G1.get_column_number(newDt, "num") < 0)
                newDt.Columns.Add("num").SetOrdinal(0);

            G1.NumberDataTable(newDt);
            return newDt;
        }
        /***********************************************************************************************/
        private DataTable processImportedData ( DataTable dt )
        {
            ConvertExcelDate(dt, "prospectCreationDate");
            ConvertExcelDate(dt, "lastTouchDate");
            ConvertExcelDate(dt, "nextScheduledTouchDate");

            string str = "";
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                str = dt.Rows[i]["contactStatus"].ObjToString().Trim();
                if (str.ToUpper().IndexOf("SOLD") >= 0)
                    dt.Rows[i]["completed"] = "1";
                dt.Rows[i]["contactStatus"] = str;
            }
            return dt;
        }
        /***********************************************************************************************/
        private void ConvertExcelDate ( DataTable dt, string column )
        {
            if (G1.get_column_number(dt, column) < 0)
                return;
            DateTime date = DateTime.Now;
            double str = 0;
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                str = dt.Rows[i][column].ObjToDouble();
                date = DateTime.FromOADate(str);

                if ( date.Year > 1900 )
                    dt.Rows[i][column] = date.ToString("MM/dd/yyyy");
            }
        }
        /***********************************************************************************************/
        private DataTable mapTheColumns ( DataTable dt )
        {
            dt = MapColumn(dt, "Funeral Home", "funeralHome");
            dt = MapColumn(dt, "SMFS Agent", "agent");
            dt = MapColumn(dt, "Title", "prefix");
            dt = MapColumn(dt, "First Name", "firstName");
            dt = MapColumn(dt, "Last Name", "lastName");
            dt = MapColumn(dt, "Middle Name / Initial", "middleName");
            dt = MapColumn(dt, "Mobile Phone Number", "mobilePhone");
            dt = MapColumn(dt, "Home Phone Number", "homePhone");
            dt = MapColumn(dt, "Work Phone Number", "workPhone");
            dt = MapColumn(dt, "Address 2", "address2");
            dt = MapColumn(dt, "Lead Source", "leadSource");
            dt = MapColumn(dt, "Prospect Creation Date", "prospectCreationDate");
            dt = MapColumn(dt, "Contact Status / Interest Level", "contactStatus");
            dt = MapColumn(dt, "Last Touch Date", "lastTouchDate");
            dt = MapColumn(dt, "Last Touch Time", "lastTouchTime");
            dt = MapColumn(dt, "Last Touch Activity", "lastTouchActivity");
            dt = MapColumn(dt, "Last Touch Result", "lastTouchResult");
            dt = MapColumn(dt, "Next Scheduled Touch Date", "nextScheduledTouchDate");
            dt = MapColumn(dt, "Next Scheduled Touch Time", "nextScheduledTouchTime");
            dt = MapColumn(dt, "Next Touch Result", "nextTouchResult");
            dt = MapColumn(dt, "Scheduled Activity", "scheduledActivity");
            dt = MapColumn(dt, "Total # Touches Made", "totalTouches");
            dt = MapColumn(dt, "Reference Funeral #", "referenceFuneral");
            dt = MapColumn(dt, "Reference Deceased Title", "refDeceasedPrefix");
            dt = MapColumn(dt, "Reference Deceased First Name", "refDeceasedFirstName");
            dt = MapColumn(dt, "Reference Deceased Middle Name", "refDeceasedMiddleName");
            dt = MapColumn(dt, "Reference Deceased Last Name", "refDeceasedLastName");
            dt = MapColumn(dt, "Reference Deceased Suffix", "refDeceasedSuffix");
            dt = MapColumn(dt, "Prospect Relationship to Reference Funeral", "funeralRelationship");
            dt = MapColumn(dt, "Reference Trust #", "referenceTrust");
            dt = MapColumn(dt, "Special Meeting", "specialMeeting");
            return dt;
        }
        /***********************************************************************************************/
        private DataTable MapColumn ( DataTable dt, string fromCol, string toCol )
        {
            try
            {
                if (G1.get_column_number(dt, fromCol) >= 0)
                {
                    if (G1.get_column_number(dt, toCol) < 0)
                    {
                        dt.Columns[fromCol].ColumnName = toCol;
                        dt.Columns[toCol].Caption = dt.Columns[toCol].ColumnName.ObjToString().Trim();
                    }
                    else
                    {
                    }
                }
                else
                {
                }
            }
            catch ( Exception ex )
            {
            }
            return dt;
        }
        /***********************************************************************************************/
        private bool SaveData(DataTable dt, DateTime saveDate )
        {
            this.Cursor = Cursors.WaitCursor;

            double dValue = 0D;
            string str = "";

            DateTime date = saveDate;

            this.Cursor = Cursors.Default;
            return true;
        }
        /***********************************************************************************************/
        private void museInputFileMappingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            this.Cursor = Cursors.WaitCursor;
            EditMapping mapField = new EditMapping(workMap, dt);
            mapField.Show();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void btnProcess_Click(object sender, EventArgs e)
        {
            string cmd = "Select * from `" + workTable + "` WHERE `map` = '" + workMap + "';";
            DataTable mapDt = G1.get_db_data(cmd);

            DataTable importDt = (DataTable)dgv.DataSource;

            string command = "select column_name,data_type,column_key,character_maximum_length,column_default from information_schema.`COLUMNS` where table_schema = 'smfs'";
            command += " and table_name = 'contacts_preneed';";
            workTableDt = G1.get_db_data(command);
            if (workTableDt.Rows.Count <= 0)
                return;


            cmd = "Select * from `contacts_preneed` WHERE `funeralHome` = 'XyzzyHome'";
            DataTable contactsDt = G1.get_db_data(cmd);
            contactsDt.Columns.Add("duplicate");
            contactsDt.Columns.Add("duplicateRecord");
            contactsDt.Columns.Add("allow");

            this.Cursor = Cursors.WaitCursor;
            for ( int i=0; i<importDt.Rows.Count; i++)
            {
                BuildContact(importDt, i, mapDt, contactsDt);
            }

            this.Cursor = Cursors.Default;

            SetupCompleted( contactsDt );

            //gridMain2.Columns["completed"].Visible = false;
            G1.NumberDataTable(contactsDt);
            dgv2.DataSource = contactsDt;

            btnSave.Show();
            btnSave.Refresh();
        }
        /***********************************************************************************************/
        private DataTable BuildContact ( DataTable importDt, int row, DataTable mapDt, DataTable contactsDt )
        {
            DataRow dRow = null;
            DataRow [] xRows = null;
            string data_field = "";
            string map_field = "";
            string type = "";
            bool good = true;
            string dbData = null;

            int pass = 1;
            bool gotOther = false;
            string[] Lines = null;
            DateTime date = DateTime.Now;
            string data = "";
            string phoneType = "";
            string firstName = importDt.Rows[row]["FirstName"].ObjToString();
            string lastName = importDt.Rows[row]["LastName"].ObjToString();

            string otherFirstName = importDt.Rows[row]["OtherFirstName"].ObjToString();
            string otherLastName = importDt.Rows[row]["OtherFirstName"].ObjToString();

            DataRow [] dRows = existingDt.Select("firstName='" + firstName + "' AND lastName='" + lastName + "'");

            DateTime dob = DateTime.Now;
            int age = 0;

            for (; ;)
            {
                dRow = contactsDt.NewRow();

                if (dRows.Length > 0)
                {
                    dRow["duplicate"] = "Duplicate";
                    dRow["allow"] = "Exclude";
                    dRow["funeralHome"] = dRows[0]["funeralHome"].ObjToString();
                    dRow["duplicateRecord"] = dRows[0]["record"].ObjToString();
                }

                for ( int i=0; i<mapDt.Rows.Count; i++)
                {
                    good = true;
                    try
                    {
                        dbData = null;
                        data_field = mapDt.Rows[i]["data_field"].ObjToString();

                        if (data_field.ToUpper() == "NOTES" && pass == 2)
                            continue;

                        if (dRows.Length > 0)
                            dbData = dRows[0][data_field].ObjToString();

                        map_field = mapDt.Rows[i]["map_field"].ObjToString();
                        if (!String.IsNullOrWhiteSpace(map_field))
                        {
                            Lines = map_field.Split(',');
                            if (Lines.Length > 1)
                                gotOther = true;
                            if (Lines.Length > 1)
                            {
                                if (pass == 2)
                                    map_field = Lines[1].Trim();
                                else
                                    map_field = Lines[0].Trim();

                            }
                            xRows = workTableDt.Select("COLUMN_NAME='" + data_field + "'");
                            if (xRows.Length <= 0)
                                continue;

                            data = importDt.Rows[row][map_field].ObjToString();
                            if (map_field.ToUpper().IndexOf("LASTNAME") >= 0)
                            {
                                if (String.IsNullOrWhiteSpace(data))
                                {
                                    good = false;
                                    break; // Get Out if blank
                                }
                            }
                        }
                        else
                        {
                            xRows = workTableDt.Select("COLUMN_NAME='" + data_field + "'");
                            if (xRows.Length <= 0)
                                continue;

                            data = dbData;
                            if (data == null)
                                continue;
                        }

                        type = xRows[0]["DATA_TYPE"].ObjToString();
                        if (type.ToUpper() == "DATE" || type.ToUpper() == "TIMESTAMP" )
                        {
                            if (String.IsNullOrWhiteSpace(data) && !String.IsNullOrWhiteSpace(dbData))
                                data = dbData;
                            date = data.ObjToDateTime();
                            dRow[data_field] = G1.DTtoMySQLDT(date);
                        }
                        else
                        {
                            if (map_field.ToUpper().IndexOf("PHONE") == 0 && pass == 1 )
                            {
                                for (int k = 0; k < Lines.Length; k++)
                                {
                                    map_field = "Phone" + (k + 1).ToString();
                                    data = importDt.Rows[row][map_field].ObjToString().Trim();
                                    data = AgentProspectReport.reformatPhone(data, true);
                                    phoneType = importDt.Rows[row][map_field + "Note"].ObjToString();
                                    if (phoneType.ToUpper() == "CELL")
                                        dRow["mobilePhone"] = data;
                                    else if (phoneType.ToUpper() == "WORK")
                                        dRow["workPhone"] = data;
                                    else if (phoneType.ToUpper() == "HOME")
                                        dRow["homePhone"] = data;
                                    else
                                        dRow["primaryPhone"] = data;
                                }
                            }
                            else
                            {
                                if (String.IsNullOrWhiteSpace(data) && !String.IsNullOrWhiteSpace(dbData))
                                    data = dbData;
                                dRow[data_field] = data;
                            }
                        }
                    }
                    catch ( Exception ex)
                    {
                    }
                }

                if (good)
                {
                    try
                    {
                        dRow["agent"] = cmbEmployee.Text.Trim();
                        dRow["funeralHome"] = cmbLocation.Text.Trim();
                        firstName = dRow["firstName"].ObjToString();
                        lastName = dRow["lastName"].ObjToString();

                        //dRows = existingDt.Select("firstName='" + firstName + "' AND lastName='" + lastName + "'");
                        if (dRows.Length > 0)
                        {
                            dRow["duplicate"] = "Duplicate";
                            dRow["allow"] = "Exclude";
                            dRow["funeralHome"] = dRows[0]["funeralHome"].ObjToString();
                            dRow["duplicateRecord"] = dRows[0]["record"].ObjToString();
                        }

                        dob = dRow["dob"].ObjToDateTime();
                        if (dob.Year > 1000)
                            dRow["age"] = G1.CalculateAgeCorrect(dob, DateTime.Now);
                    }
                    catch (Exception ex)
                    {
                    }

                    contactsDt.Rows.Add(dRow);
                }

                if ( pass == 1 && gotOther )
                {
                    pass = 2;
                    if (String.IsNullOrWhiteSpace(otherFirstName) && String.IsNullOrWhiteSpace(otherLastName))
                        break;
                    dRows = existingDt.Select("firstName='" + otherFirstName + "' AND lastName='" + otherLastName + "'");
                    continue;
                }
                break;
            }

            return contactsDt;
        }
        /***********************************************************************************************/
        private void gridMain2_CustomColumnDisplayText(object sender, CustomColumnDisplayTextEventArgs e)
        {
            if (e.ListSourceRowIndex == DevExpress.XtraGrid.GridControl.InvalidRowHandle)
                return;
            string name = e.Column.FieldName;
            if (name.ToUpper().IndexOf("DATE") >= 0)
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
        }
        /***********************************************************************************************/
        private string oldWhat = "";

        private void repositoryItemComboBox6_Validating(object sender, CancelEventArgs e)
        {
            GridView view = sender as GridView;
            if (gridMain.FocusedColumn.FieldName == "funeralHome")
            {
                DataRow dr = gridMain.GetFocusedDataRow();
                int rowhandle = gridMain.FocusedRowHandle;
                int row = gridMain.GetDataSourceRowIndex(rowhandle);
                string funeralHome = dr["funeralHome"].ObjToString();
                if (!ValidateFuneralHome(funeralHome))
                    e.Cancel = false;
                else
                    oldWhat = funeralHome;
            }
        }
        /****************************************************************************************/
        private bool ValidateFuneralHome(string data)
        {
            bool found = false;
            string funeralHome = "";
            for (int i = 0; i < cmbLocation.Items.Count; i++)
            {
                funeralHome = cmbLocation.Items[i].ObjToString().Trim();
                if (funeralHome == data)
                {
                    found = true;
                    break;
                }
            }
            return found;
        }
        /***********************************************************************************************/
        private void repositoryItemComboBox7_SelectedValueChanged(object sender, EventArgs e)
        {
            DevExpress.XtraEditors.ComboBoxEdit combo = (DevExpress.XtraEditors.ComboBoxEdit)sender;
            string primary = combo.Text;
            if (!String.IsNullOrWhiteSpace(primary))
            {
                try
                {
                    DataRow dr = gridMain2.GetFocusedDataRow();
                    string phone = dr["primaryPhone"].ObjToString();
                    //dr["primaryPhone"] = primary;
                    int rowhandle = gridMain.FocusedRowHandle;
                    int row = gridMain.GetDataSourceRowIndex(rowhandle);
                    if (primary.ToUpper() == "MOBILE PHONE")
                    {
                        dr["mobilePhone"] = phone;
                        dr["primaryPhone"] = "";
                        gridMain2.RefreshEditor(true);
                        gridMain2.PostEditor();
                    }
                    else if (primary.ToUpper() == "WORK PHONE")
                    {
                        dr["workPhone"] = phone;
                        dr["primaryPhone"] = "";
                        gridMain2.RefreshEditor(true);
                        gridMain2.PostEditor();
                    }
                    else if (primary.ToUpper() == "HOME PHONE")
                    {
                        dr["homePhone"] = phone;
                        dr["primaryPhone"] = "";
                        gridMain2.RefreshEditor(true);
                        gridMain2.PostEditor();
                    }
                }
                catch (Exception ex)
                {
                }
            }
        }
        /***********************************************************************************************/
    }
}