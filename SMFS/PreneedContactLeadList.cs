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
    public partial class PreneedContactLeadList : DevExpress.XtraEditors.XtraForm
    {
        private bool loading = true;
        private bool modified = false;
        private string workContractNumber = "";
        private string workLastName = "";
        private string workMiddleName = "";
        private string workFirstName = "";
        private string workContactName = "";
        private string mainTitle = "";
        private string workAgent = "";
        private DateTime workDOB = DateTime.Now;
        private DateTime workDOD = DateTime.Now;
        private DataTable workDt = null;
        /****************************************************************************************/
        public PreneedContactLeadList( string contractNumber, string agent, DataTable dt )
        {
            InitializeComponent();
            workContractNumber = contractNumber;
            workAgent = agent;
            workDt = dt;
        }
        /****************************************************************************************/
        private void SetupToolTips()
        {
            ToolTip tt = new ToolTip();
            //tt.SetToolTip(this.pictureBox11, "Remove Contact");
        }
        /****************************************************************************************/
        private void PreneedContactLeadList_Load(object sender, EventArgs e)
        {
            oldWhat = "";

            string cmd = "Select * from `fcustomers` WHERE `contractNumber` = '" + workContractNumber + "';";
            DataTable dt = G1.get_db_data(cmd);

            if ( dt.Rows.Count <= 0 )
            {
                this.Close();
                return;
            }

            workLastName = dt.Rows[0]["lastName"].ObjToString();
            workFirstName = dt.Rows[0]["firstName"].ObjToString();
            workMiddleName = dt.Rows[0]["middleName"].ObjToString();
            workDOB = dt.Rows[0]["birthDate"].ObjToDateTime();
            workDOD = dt.Rows[0]["deceasedDate"].ObjToDateTime();

            workContactName = workLastName + ", " + workFirstName + " " + workMiddleName;
            this.Text = "Contact Possibilities for " + workContactName;
            mainTitle = this.Text;

            SetupToolTips();

            loading = true;

            LoadData();

            LoadContactInfo();

            modified = false;
            loading = false;
        }
        /***********************************************************************************************/
        private void LoadContactInfo()
        {
            string cmd = "";
            string workDetail = "";

            lblContactName.Text = workLastName + ", " + workFirstName + " " + workMiddleName;
            cmd = "Select * from `fcustomers` where `contractNumber` = '" + workContractNumber + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return;

            string str = "";
            string address1 = dt.Rows[0]["address1"].ObjToString();
            string address2 = dt.Rows[0]["address2"].ObjToString();
            lblContactAddress.Text = address1 + " " + address2;
            string city = dt.Rows[0]["city"].ObjToString();
            string state = dt.Rows[0]["state"].ObjToString();
            string zip = dt.Rows[0]["zip1"].ObjToString();
            string zip2 = dt.Rows[0]["zip2"].ObjToString();
            //string email = dt.Rows[0]["email"].ObjToString();

            str = city + ", " + state + "  " + zip;
            if (String.IsNullOrWhiteSpace(city))
                str = state + "   " + zip;
            lblContactCity.Text = str;

            str = dt.Rows[0]["phoneNumber1"].ObjToString();
            lblContactPhone.Text = str;

            lblEmail.Text = "";

            DateTime dob = dt.Rows[0]["birthDate"].ObjToDateTime();
            if (dob.Year > 100)
            {
                int myAge = G1.CalculateAgeCorrect(dob, DateTime.Now);
                string age = myAge.ObjToString();
                string gender = dt.Rows[0]["sex"].ObjToString();
                str = mainTitle + " (Age-" + age + ") (Gender-" + gender + ")";
                this.Text = str;
            }

            str = "Current Agent :" + workAgent;
            lblCurrentAgent.Text = str;
            

            //str = dt.Rows[0]["email"].ObjToString();
            //lblContactEmail.Text = str;

            //str = dt.Rows[0]["pocName"].ObjToString();
            //lblpocName.Text = str;

            //str = dt.Rows[0]["pocTitle"].ObjToString();
            //lblpocTitle.Text = str;

            //str = dt.Rows[0]["pocPhone"].ObjToString();
            //lblpocPhone.Text = str;

            //str = dt.Rows[0]["pocEmail"].ObjToString();
            //lblpocEmail.Text = str;
        }
        /***********************************************************************************************/
        private void LoadData()
        {
            this.Cursor = Cursors.WaitCursor;



            string cmd = "Select * from `relatives` WHERE `contractNumber` = '" + workContractNumber + "' ";
            cmd += " AND `depRelationship` <> 'DISCLOSURES' ";
            cmd += " AND `depRelationship` <> 'CLERGY' ";
            cmd += " AND `depRelationship` <> 'PB' ";
            cmd += " AND `depRelationship` <> 'HPB' ";
            cmd += " AND `depRelationship` <> 'MUSICIAN' ";
            cmd += " AND `depRelationship` <> 'FUNERAL DIRECTOR' ";
            cmd += " AND `depRelationship` <> 'PALLBEARER' ";
            cmd += " AND `deceased` <> '1' ";
            cmd += " ORDER by `depLastName`,`depFirstName`,`depMI` ";

            DataTable dt = G1.get_db_data(cmd);

            dt.Columns.Add("addContact");
            dt.Columns.Add("pp");
            dt.Columns.Add("agent");

            string firstName = "";
            string lastName = "";
            string middleName = "";
            string prefix = "";
            string suffix = "";
            string contactStatus = "";
            DataTable dx = null;

            AddMod(dt, gridMain);

            if ( !showAll )
                dt = filterResults(dt);

            SetupAddContact ( dt );

            string saveName = "";

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                firstName = dt.Rows[i]["depFirstName"].ObjToString();
                saveName = firstName;
                if (firstName.Length > 1)
                    firstName = firstName.Substring(0, 1);
                lastName = dt.Rows[i]["depLastName"].ObjToString().Trim();
                middleName = dt.Rows[i]["depMI"].ObjToString().Trim();
                prefix = dt.Rows[i]["depPrefix"].ObjToString().Trim();
                suffix = dt.Rows[i]["depSuffix"].ObjToString().Trim();
                if (!String.IsNullOrWhiteSpace(lastName))
                {
                    cmd = "Select * from `customers` WHERE `firstName` LIKE '" + firstName + "%' AND `lastName` = '" + lastName + "';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                        dt.Rows[i]["pp"] = "Y";
                    cmd = "Select * from `contacts_preneed` WHERE `firstName` = '" + saveName + "' AND `lastName` = '" + lastName + "' ";
                    cmd += " AND `middleName` = '" + middleName + "' AND `prefix` = '" + prefix + "' AND `suffix` = '" + suffix + "';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                    {
                        contactStatus = dx.Rows[0]["contactStatus"].ObjToString();
                        if (contactStatus.Trim().ToUpper() == "RELEASED")
                        {
                            dt.Rows[i]["addContact"] = "0";
                            dt.Rows[i]["mod"] = "Y";
                            dt.Rows[i]["agent"] = workAgent;
                            modified = true;
                        }
                        else
                            dt.Rows[i]["addContact"] = "1";
                        dt.Rows[i]["agent"] = dx.Rows[i]["agent"].ObjToString();
                    }
                }
            }

            G1.NumberDataTable(dt);

            dgv.DataSource = dt;

            this.Cursor = Cursors.Default;

            showAll = false;

        }
        /***********************************************************************************************/
        private DataTable filterResults ( DataTable dt )
        {
            string cmd = "Select * from `relation_categories`;";
            DataTable catDt = G1.get_db_data(cmd);

            cmd = "Select * from `relation_age_ranges`;";
            DataTable ageDt = G1.get_db_data(cmd);

            string ageRange = "";
            string category = "";
            DateTime dob = DateTime.Now;
            DateTime dod = DateTime.Now;
            DateTime now = DateTime.Now;
            int age = 0;

            for ( int i=dt.Rows.Count-1; i>=0; i--)
            {
                dod = dt.Rows[i]["depDOD"].ObjToDateTime();
                if ( dod.Year > 100 )
                    dt.Rows.RemoveAt(i);
            }

            int minAge = 0;
            int maxAge = 0;
            string[] Lines = null;
            DataRow[] dRows = null;
            string relationships = "";
            string relationship = "";
            bool found = false;

            for (int i = dt.Rows.Count - 1; i >= 0; i--)
            {
                //dob = dt.Rows[i]["depDOB"].ObjToDateTime();
                //if (dob.Year < 100)
                //{
                //    dt.Rows.RemoveAt(i);
                //    continue;
                //}
                dob = workDOB;
                //dob = workDOD;
                age = G1.CalculateAgeCorrect(dob, now);
                relationship = dt.Rows[i]["depRelationship"].ObjToString();
                if ( relationship.ToUpper() == "DAUGHTER")
                {

                }
                found = false;
                for (int j = 0; j < ageDt.Rows.Count; j++)
                {
                    ageRange = ageDt.Rows[j]["ageRange"].ObjToString();
                    if (String.IsNullOrWhiteSpace(ageRange))
                        continue;
                    Lines = ageRange.Split('-');
                    if (Lines.Length < 2)
                        continue;
                    minAge = Lines[0].ObjToInt32();
                    maxAge = Lines[1].ObjToInt32();
                    if (age < minAge || age > maxAge)
                        continue;

                    category = ageDt.Rows[j]["relation_category"].ObjToString();
                    if (String.IsNullOrWhiteSpace(category))
                        continue;

                    dRows = catDt.Select("relation_category='" + category + "'");
                    if (dRows.Length <= 0)
                        continue;
                    relationships = dRows[0]["relationships"].ObjToString();
                    if (String.IsNullOrWhiteSpace(relationships))
                        continue;
                    Lines = relationships.Split('~');
                    for ( int k=0; k<Lines.Length; k++)
                    {
                        if ( Lines[k].Trim() == relationship )
                        {
                            found = true;
                            break;
                        }
                    }
                    if (found)
                        break;
                }
                if (!found)
                    dt.Rows.RemoveAt(i);
            }

            return dt;
        }
        /***********************************************************************************************/
        private void SetupAddContact(DataTable dt)
        {
            DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew = this.repositoryItemCheckEdit1;
            selectnew.NullText = "";
            selectnew.ValueChecked = "1";
            selectnew.ValueUnchecked = "0";
            selectnew.ValueGrayed = "";

            string completed = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                completed = dt.Rows[i]["addContact"].ObjToString();
                if (completed == "1")
                    dt.Rows[i]["addContact"] = "1";
                else
                    dt.Rows[i]["addContact"] = "0";
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
        private void UpdateMod(DataRow dr)
        {
            dr["mod"] = "Y";
            modified = true;
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
            Printer.DrawQuad(2, 18, 3, 2, lblEmail.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            Printer.DrawQuad(1, 20, 6, 3, "User : " + LoginForm.username, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);

            //Printer.DrawQuad(9, 8, 5, 2, "Point of Contact :", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(10, 10, 3, 2, lblpocName.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(10, 12, 3, 2, lblpocTitle.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(10, 14, 3, 2, lblpocPhone.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(10, 16, 3, 2, lblpocEmail.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom); ;

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
        private string currentColumn = "";
        /***************************************************************************************/
        public delegate void d_contactLeadDone( DataTable dt );
        public event d_contactLeadDone contactLeadDone;
        protected void OnDone()
        {
            DataTable dt = (DataTable)dgv.DataSource;

            DataTable dx = dt.Clone();

            string mod = "";
            string addContact = "";

            for ( int i=0; i<dt.Rows.Count; i++)
            {
                mod = dt.Rows[i]["mod"].ObjToString().ToUpper();
                addContact = dt.Rows[i]["addContact"].ObjToString();
                if ( addContact == "1" )
                    G1.copy_dt_row(dt, i, dx, dx.Rows.Count);
            }

            if (contactLeadDone != null)
                contactLeadDone ( dx );
        }
        /****************************************************************************************/
        private void repositoryItemCheckEdit1_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                DataRow dr = gridMain.GetFocusedDataRow();
                DataTable dt = (DataTable)dgv.DataSource;
                string agent = dr["agent"].ObjToString();
                string addContact = dr["addContact"].ObjToString();
                if (!String.IsNullOrWhiteSpace(agent))
                {
                    if (workAgent != agent)
                    {
                        MessageBox.Show("This contact is already on another contact List!", "Contact Issue Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                        if (addContact == "1")
                        {
                            dr["addContact"] = "1";
                            gridMain.RefreshData();
                            gridMain.RefreshEditor(true);
                        }
                        return;
                    }
                }
                if (dr["addContact"].ObjToString() != "1")
                    dr["addContact"] = "1";
                else
                    dr["addContact"] = "0";
                dr["mod"] = "Y";
            }
            catch ( Exception ex)
            {
            }

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

            GridColumn currCol = gridMain.FocusedColumn;
            string currentColumn = currCol.FieldName;
            if (currentColumn.ToUpper() == "NUM")
                return;

            string what = dr[currentColumn].ObjToString();
            string record = dr["record"].ObjToString();
            string agent = dr["agent"].ObjToString();
            //if (!String.IsNullOrWhiteSpace(agent))
            //{
            //    MessageBox.Show("This contact is already on a contact List!", "Contact Issue Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            //    return;
            //}

            try
            {
                Update_PreNeed(record, currentColumn, what);
            }
            catch (Exception ex)
            {
            }
            gridMain.RefreshData();
        }
        /****************************************************************************************/
        private void Update_PreNeed(string record, string field, string data)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(record))
                    return;
                if (!String.IsNullOrWhiteSpace(record))
                {
                    G1.update_db_table("contacts_preneed", "record", record, new string[] { field, data });
                }
            }
            catch (Exception ex)
            {
            }
        }
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
            //if (name == "apptDate")
            //    doDate = true;
            //else if (name == "lastContactDate")
            //    doDate = true;

            if (name.ToUpper().IndexOf("DATE") >= 0)
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
                            DateTime date = myDate.ObjToDateTime();

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
        private bool showAll = false;
        private void btnShowAll_Click(object sender, EventArgs e)
        {
            showAll = true;
            LoadData();
        }
        /****************************************************************************************/
        private void searchForPreNeedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            DataTable dt = (DataTable)dgv.DataSource;

            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            string firstName = dr["depFirstName"].ObjToString();
            string saveName = firstName;
            if ( firstName.Length > 1 )
            {
                firstName = firstName.Substring(0, 1);
                firstName += "%";
            }
            string lastName = dr["depLastName"].ObjToString();

            using (FastLookup fastForm = new FastLookup(firstName, lastName))
            {
                fastForm.Text = "Possible Preneeds for " + saveName + " " + lastName;
                fastForm.ShowDialog();
            }
        }
        /****************************************************************************************/
    }
}