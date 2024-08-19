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
using DevExpress.XtraGrid.Views.BandedGrid;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class PreneedContactHistory : DevExpress.XtraEditors.XtraForm
    {
        private bool loading = true;
        private bool modified = false;
        private string primaryName = "";
        private string workContactName = "";
        private string workContactType = "";
        private int defaultFrequency = 3;
        private string workFirstName = "";
        private string workMiddleName = "";
        private string workLastName = "";
        private string workLocation = "";
        private string mainTitle = "";
        private DataRow workDr = null;
        private GridView workGV = null;
        private DataTable workDt = null;
        private int workRow = -1;
        public bool isModified = false;
        private editDG editForm = null;
        private DataTable originalGV = null;
        /****************************************************************************************/
        public PreneedContactHistory(DevExpress.XtraGrid.Views.Grid.GridView gv, DataTable dt, int row, string lastName, string firstName, string middleName, string location, DataRow dr )
        {
            InitializeComponent();
            workGV = gv;
            workDt = dt;
            workRow = row;
            workFirstName = firstName;
            workLastName = lastName;
            workMiddleName = middleName;
            workLocation = location;
            workDr = dr;
        }
        /****************************************************************************************/
        private void SetupToolTips()
        {
            ToolTip tt = new ToolTip();
            tt.SetToolTip(this.pictureBox11, "Remove Contact");
        }
        /****************************************************************************************/
        private void PreneedContactHistory_Load(object sender, EventArgs e)
        {
            oldWhat = "";

            pictureBox11.Hide();
            btnAccept.Hide();
            chkExcludeCompleted.Hide();

            workContactName = workLastName + ", " + workFirstName + " " + workMiddleName;
            this.Text = "Contact History for " + workContactName;
            mainTitle = this.Text;

            SetupToolTips();

            loading = true;

            LoadDBTable("ref_relations", "relationship", this.repositoryItemComboBox3);
            LoadDBTable("ref_contact_status", "contact_status", this.repositoryItemComboBox4);
            LoadDBTable("ref_lead source", "lead source", this.repositoryItemComboBox5);

            LoadData();

            LoadContactInfo();

            LoadNotes();

            originalGV = new DataTable();
            originalGV.Columns.Add("field");
            originalGV.Columns.Add("what");

            DataRow dRow = null;
            for ( int i=0; i<workGV.Columns.Count; i++)
            {
                dRow = originalGV.NewRow();
                dRow["field"] = workGV.Columns[i].FieldName.ObjToString();
                if (workGV.Columns[i].Visible)
                    dRow["what"] = "Y";
                originalGV.Rows.Add(dRow);
            }

            GridColumn col = workGV.Columns["results"];
            //workGV.Columns.Remove(col);
            //col = workGV.Columns["lastTouchResult"];
            //workGV.Columns.Remove(col);
            //col = workGV.Columns["notes"];
            //col.Visible = false;

            gridMain.Columns["notes"].Visible = false;


            //workGV.Columns.Remove(col);

            string str = "";
            for ( int i=0; i<gridMain.Columns.Count; i++)
            {
                str = gridMain.Columns[i].FieldName.ObjToString();
                if (gridMain.Columns[i].Visible == false)
                    continue;
                if ( str.ToUpper() == "NOTES")
                {
                    continue;
                }
                if ( G1.get_column_number ( workGV, str) >= 0 )
                {
                    col = workGV.Columns[str];
                    col.Visible = false;
                    //workGV.Columns.Remove(col);
                }
            }

            string record = workDt.Rows[workRow]["record"].ObjToString();

            editForm = new editDG(workGV, workDt, workRow, record, true );
            editForm.editDone += EditForm_editDone;
            //editFunPayments.paymentClosing += EditFunPayments_paymentClosing;
            if (!this.LookAndFeel.UseDefaultLookAndFeel)
            {
                editForm.LookAndFeel.UseDefaultLookAndFeel = false;
                editForm.LookAndFeel.SetSkinStyle(this.LookAndFeel.SkinName);
            }

            G1.LoadFormInPanel(editForm, this.panelMiddle);


            //DataRow dr = gridMain.GetFocusedDataRow();
            //DataTable dt = (DataTable)dgv.DataSource;
            //int rowHandle = gridMain.FocusedRowHandle;
            //int row = gridMain.GetFocusedDataSourceRowIndex();
            //string record = dr["record"].ObjToString();

            //using (editDG editForm = new editDG(gridMain, dt, row, record))
            //{
            //    //editForm.editDone += EditForm_editDone;
            //    editForm.ShowDialog();
            //}

            gridMain.DestroyCustomization();
            G1.HideGridChooser(gridMain);


            modified = false;
            loading = false;
        }
        /****************************************************************************************/
        private void EditForm_editDone(DataTable dx, int row, string CancelStatus )
        {
            if ( CancelStatus == "YES")
            {
                this.Close();
                return;
            }
            PleaseWait pleaseForm = new PleaseWait("Please Wait!\nUpdating Contact!");
            pleaseForm.Show();
            pleaseForm.Refresh();

            DataTable dt = (DataTable)dgv.DataSource;

            string caption = "";
            string data = "";
            string field = "";
            string type = "";
            row = 0;
            string record = dt.Rows[row]["record"].ObjToString();
            dt.Rows[row]["mod"] = "Y";
            string modList = "";
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                caption = dx.Rows[i]["field"].ObjToString();
                field = dx.Rows[i]["actualField"].ObjToString();
                if (field.ToUpper() == "RESULTS")
                {
                }
                if (!workGV.Columns[field].Visible)
                    continue;
                data = dx.Rows[i]["data"].ObjToString();
                if (G1.get_column_number(dt, field) >= 0)
                {
                    try
                    {
                        type = dt.Columns[field].DataType.ToString().ToUpper();
                        if (type.IndexOf("MYSQLDATETIME") >= 0)
                            dt.Rows[row][field] = G1.DTtoMySQLDT(data);
                        else if (type.IndexOf("DOUBLE") >= 0)
                            dt.Rows[row][field] = data.ObjToDouble();
                        else if (type.IndexOf("DECIMAL") >= 0)
                            dt.Rows[row][field] = data.ObjToDecimal();
                        else if (type.IndexOf("INT32") >= 0)
                            dt.Rows[row][field] = data.ObjToInt32();
                        else if (type.IndexOf("INT64") >= 0)
                            dt.Rows[row][field] = data.ObjToInt64();
                        else
                        {
                            dt.Rows[row][field] = data.ToString();
                            if (data.IndexOf(",") >= 0)
                            {
                                G1.update_db_table("contacts_preneed", "record", record, new string[] { field, data });
                                continue;
                            }
                        }
                        if (String.IsNullOrWhiteSpace(data))
                            data = "NODATA";
                        modList += field + "," + data + ",";
                    }
                    catch (Exception ex)
                    {
                    }
                    //dt.Rows[row][field] = data;
                }
            }
            modList = modList.TrimEnd(',');
            G1.update_db_table("contacts_preneed", "record", record, modList);


            modList = "";
            for ( int i=0; i<gridMain.Columns.Count; i++)
            {
                if (!gridMain.Columns[i].Visible)
                    continue;
                field = gridMain.Columns[i].FieldName.ObjToString();
                if (field.ToUpper() == "NUM")
                    continue;
                if (field.ToUpper() == "NEXTCOMPLETED")
                    continue;
                data = dt.Rows[0][field].ObjToString();
                type = dt.Columns[field].DataType.ToString().ToUpper();
                if (type.IndexOf("MYSQLDATETIME") >= 0)
                    dt.Rows[row][field] = G1.DTtoMySQLDT(data);
                else if (type.IndexOf("DOUBLE") >= 0)
                    dt.Rows[row][field] = data.ObjToDouble();
                else if (type.IndexOf("DECIMAL") >= 0)
                    dt.Rows[row][field] = data.ObjToDecimal();
                else if (type.IndexOf("INT32") >= 0)
                    dt.Rows[row][field] = data.ObjToInt32();
                else if (type.IndexOf("INT64") >= 0)
                    dt.Rows[row][field] = data.ObjToInt64();
                else
                {
                    dt.Rows[row][field] = data.ToString();
                    if (data.IndexOf(",") >= 0)
                    {
                        G1.update_db_table("contacts_preneed", "record", record, new string[] { field, data });
                        continue;
                    }
                }
                if (String.IsNullOrWhiteSpace(data))
                    data = "NODATA";
                modList += field + "," + data + ",";
            }

            modList = modList.TrimEnd(',');
            G1.update_db_table("contacts_preneed", "record", record, modList);

            gridMain.RefreshData();
            gridMain.RefreshEditor(true);

            //PositionToRecord(dt, record);

            isModified = true;

            pleaseForm.FireEvent1();
            pleaseForm.Dispose();
            pleaseForm = null;

            //PutThingsBack();

            //for ( int i=0; i<originalGV.Rows.Count; i++)
            //{
            //    field = originalGV.Rows[i]["field"].ObjToString();
            //    if (originalGV.Rows[i]["what"].ObjToString() == "Y")
            //        workGV.Columns[field].Visible = true;
            //}

            this.Close();
        }
        /***********************************************************************************************/
        private void PutThingsBack ()
        {
            string field = "";
            for (int i = 0; i < originalGV.Rows.Count; i++)
            {
                field = originalGV.Rows[i]["field"].ObjToString();
                if (originalGV.Rows[i]["what"].ObjToString() == "Y")
                    workGV.Columns[field].Visible = true;
            }
        }
        /***********************************************************************************************/
        private void LoadNotes ()
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataTable newDt = new DataTable();
            newDt.Columns.Add("notes");
            DataRow dR = null;
            string notes = "";
            string majorNotes = "";
            DateTime date = DateTime.Now;

            string str = "";
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                date = dt.Rows[i]["prospectCreationDate"].ObjToDateTime();
                if (dt.Rows[i]["notes"] != null)
                {
                    str = dt.Rows[i]["notes"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(notes))
                        notes += "\n";
                    notes += str;
                }
            }
            if ( String.IsNullOrWhiteSpace ( notes ))
            {
                dgv2.DataSource = newDt;
                return;
            }
            string[] Lines = notes.Split('\n');
            for ( int i=Lines.Length-1; i>=0; i--)
            {
                str = Lines[i].Trim();
                if (String.IsNullOrWhiteSpace(str))
                    continue;
                dR = newDt.NewRow();
                dR["notes"] = str;
                newDt.Rows.Add(dR);
            }
            G1.NumberDataTable(newDt);
            dgv2.DataSource = newDt;
        }
        /***********************************************************************************************/
        private void LoadContactInfo()
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt.Rows.Count <= 0)
                return;
            string cmd = "";
            string workDetail = "";

            //lblContactName.Text = workLastName + ", " + workFirstName + " " + workMiddleName;
            //string str = dt.Rows[0]["address"].ObjToString();
            //lblContactAddress.Text = str;
            //string city = dt.Rows[0]["city"].ObjToString();
            //string state = dt.Rows[0]["state"].ObjToString();
            //string zip = dt.Rows[0]["zip"].ObjToString();
            //string email = dt.Rows[0]["email"].ObjToString();

            //str = city + ", " + state + "  " + zip;
            //if (String.IsNullOrWhiteSpace(city))
            //    str = state + "   " + zip;
            //lblContactCity.Text = str;

            string str = "";

            //if (workDr != null)
            //{
            //    string primaryPhone = workDr["primaryPhone"].ObjToString();

            //    string mobilePhone = workDr["mobilephone"].ObjToString();
            //    string workPhone = workDr["workphone"].ObjToString();
            //    string homePhone = workDr["homephone"].ObjToString();

            //    if (String.IsNullOrWhiteSpace(primaryPhone))
            //    {
            //        if (!String.IsNullOrWhiteSpace(mobilePhone))
            //            str = mobilePhone;
            //        else if (!String.IsNullOrWhiteSpace(workPhone))
            //            str = workPhone;
            //        else if (!String.IsNullOrWhiteSpace(homePhone))
            //            str = homePhone;
            //    }
            //    else
            //    {
            //        if (primaryPhone == "Mobile")
            //            str = mobilePhone;
            //        else if (primaryPhone == "Work")
            //            str = workPhone;
            //        else if (primaryPhone == "Home")
            //            str = homePhone;
            //    }
            //}

            //str = AgentProspectReport.reformatPhone(str, true);

            //lblContactPhone.Text = str;

            //lblEmail.Text = email;

            string age = dt.Rows[0]["age"].ObjToString();
            string gender = dt.Rows[0]["gender"].ObjToString();
            str = mainTitle + " (Age-" + age + ") (Gender-" + gender + ")";
            this.Text = str;
            

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
        private void LoadDBTable(string dbTable, string dbField, DevExpress.XtraEditors.Repository.RepositoryItemComboBox combo)
        {
            if (String.IsNullOrWhiteSpace(dbTable))
                return;
            if (dbTable.ToUpper() == "NONE")
            {
                combo.Items.Clear();
                return;
            }
            DataTable rx = G1.get_db_data("Select * from `" + dbTable + "`;");

            if (dbTable.ToUpper() == "REF_RELATIONS")
            {
                DataView tempview = rx.DefaultView;
                tempview.Sort = "relationship asc";
                rx = tempview.ToTable();
            }
            combo.Items.Clear();

            string name = "";
            for (int i = 0; i < rx.Rows.Count; i++)
            {
                name = rx.Rows[i][dbField].ToString().Trim();
                if (String.IsNullOrWhiteSpace(name))
                    continue;
                combo.Items.Add(name);
            }
        }
        /***********************************************************************************************/
        private void LoadData()
        {
            this.Cursor = Cursors.WaitCursor;


            string record = workDt.Rows[workRow]["oldRecord"].ObjToString();
            string cmd = "Select * from `contacts_preneed` WHERE `oldRecord` = '" + record + "' ";
            cmd += " ORDER by `record` desc, `oldRecord`, `nextScheduledTouchDate` DESC, `lastTouchDate` DESC";
            cmd += ";";
            DataTable dt = G1.get_db_data(cmd);

            if (G1.get_column_number(dt, "nextCompleted") < 0)
                dt.Columns.Add("nextCompleted");

            int touches = 1;
            for ( int i=dt.Rows.Count-1; i>=0; i--)
            {
                dt.Rows[i]["totalTouches"] = touches;
                touches++;
                if (i > 0)
                    dt.Rows[i]["nextCompleted"] = "1";
            }

            AddMod(dt, gridMain);

            SetupCompleted( dt );
            SetupNextCompleted(dt);
            //SetupFrequency ( dt );

            G1.NumberDataTable(dt);

            dgv.DataSource = dt;

            this.Cursor = Cursors.Default;
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
        /***********************************************************************************************/
        private void SetupNextCompleted(DataTable dt)
        {
            DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew = this.repositoryItemCheckEdit2;
            selectnew.NullText = "";
            selectnew.ValueChecked = "1";
            selectnew.ValueUnchecked = "0";
            selectnew.ValueGrayed = "";

            string completed = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                completed = dt.Rows[i]["nextCompleted"].ObjToString();
                if (completed == "1")
                    dt.Rows[i]["nextCompleted"] = "1";
                else
                    dt.Rows[i]["nextCompleted"] = "0";
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
            //if (gridMain.OptionsFind.AlwaysVisible == true)
            //    gridMain.OptionsFind.AlwaysVisible = false;
            //else
            //    gridMain.OptionsFind.AlwaysVisible = true;

            G1.SpyGlass(gridMain);
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

            if ( nextCompleted == "1") // xyzzy
            {
                e.Cancel = true;

                LoadData();

                LoadNotes();

                string record = workDt.Rows[workRow]["record"].ObjToString();

                editForm = new editDG(workGV, workDt, workRow, record, true);
                editForm.editDone += EditForm_editDone;
                if (!this.LookAndFeel.UseDefaultLookAndFeel)
                {
                    editForm.LookAndFeel.UseDefaultLookAndFeel = false;
                    editForm.LookAndFeel.SetSkinStyle(this.LookAndFeel.SkinName);
                }

                G1.LoadFormInPanel(editForm, this.panelMiddle);
                gridMain.DestroyCustomization();
            }
            else
                PutThingsBack();
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
            //Printer.DrawQuad(1, 8, 5, 2, "Contact Info :", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(2, 10, 3, 2, lblContactName.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(2, 12, 3, 2, lblContactAddress.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(2, 14, 3, 2, lblContactCity.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(2, 16, 3, 2, lblContactPhone.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(2, 18, 3, 2, lblEmail.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

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
            DataRow dr = gridMain.GetFocusedDataRow();
            GridColumn currCol = gridMain.FocusedColumn;
            string currentColumn = currCol.FieldName;
            int rowHandle = gridMain.FocusedRowHandle;
            if ( currentColumn == "nextCompleted" && rowHandle != 0 )
            {
                e.Valid = false;
                return;
            }

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
        public delegate string d_contactHistoryDone( DataTable dt, bool somethingDeleted );
        public event d_contactHistoryDone contactHistoryDone;
        private string nextCompleted = "";
        protected void OnDone()
        {
            nextCompleted = "";
            if (somethingDeleted)
                nextCompleted = "somethingDeleted";
            if (!isModified && !somethingDeleted )
                return;

            this.Validate();

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
            {
                nextCompleted = contactHistoryDone(dx, somethingDeleted);
            }
            isModified = false;
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
        private bool justSaved = false;
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            //if (e == null)
            //    return;


            DataTable dt = (DataTable)dgv.DataSource;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            DataRow dr = gridMain.GetFocusedDataRow();
            if (dr == null)
                return;

            dr["mod"] = "Y";
            //isModified = true;

            if (editForm != null)
                editForm.FireEventModified();
            if (1 == 1)
                return;


            GridColumn currCol = gridMain.FocusedColumn;
            string currentColumn = currCol.FieldName;
            if (currentColumn.ToUpper() == "NUM")
                return;
            if (currentColumn.ToUpper() == "NEXTCOMPLETED")
                return;

            if (G1.get_column_number(dt, currentColumn) < 0)
                return;
                
            string what = dr[currentColumn].ObjToString();
            string record = dr["record"].ObjToString();

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
        public void FireEventModified( DataTable dt )
        {
            workDt = dt;
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
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);

            GridColumn currCol = gridMain.FocusedColumn;
            DataRow dr = gridMain.GetFocusedDataRow();
            string name = currCol.FieldName;
            if ( name == "nextCompleted" && rowHandle > 0 )
            {
            }
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
                            dt.Rows[row][name] = G1.DTtoMySQLDT(myDate);
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
        private void gridMain2_CalcRowHeight(object sender, RowHeightEventArgs e)
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
                foreach (GridColumn column in gridMain2.Columns)
                {
                    name = column.FieldName.ToUpper();
                    if (name == "NOTES")
                        doit = true;
                    if (doit)
                    {
                        using (RepositoryItemMemoEdit edit = new RepositoryItemMemoEdit())
                        {
                            using (MemoEditViewInfo viewInfo = edit.CreateViewInfo() as MemoEditViewInfo)
                            {
                                str = gridMain2.GetRowCellValue(e.RowHandle, column.FieldName).ObjToString();
                                if (!String.IsNullOrWhiteSpace(str))
                                {
                                    Lines = str.Split('\n');
                                    count = Lines.Length;
                                }
                                viewInfo.EditValue = gridMain2.GetRowCellValue(e.RowHandle, column.FieldName);
                                viewInfo.Bounds = new Rectangle(0, 0, column.VisibleWidth, dgv2.Height);
                                using (Graphics graphics = dgv2.CreateGraphics())
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
                    e.RowHeight = maxHeight + 30;
            }
        }
        /****************************************************************************************/
        private void repositoryItemMemoEdit3_MouseDown(object sender, MouseEventArgs e)
        { // Notes
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowhandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowhandle);
            DataTable dt = (DataTable)dgv.DataSource;
            string data = dt.Rows[row]["notes"].ObjToString();
            string record = dt.Rows[row]["record"].ObjToString();
            //data = dr["notes"].ObjToString();

            using (EditTextData fmrmyform = new EditTextData("notes", data))
            {
                fmrmyform.Text = "";
                fmrmyform.ShowDialog();
                string p = fmrmyform.Answer.Trim();
                if (!String.IsNullOrWhiteSpace(p))
                {
                    dt.Rows[row]["notes"] = p;
                    dt.Rows[row]["mod"] = "Y";
                    dr["notes"] = p;
                    dr["mod"] = "Y";
                    gridMain.RefreshEditor(true);
                    try
                    {
                        Update_PreNeed(record, "notes", p);
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
        }
        /****************************************************************************************/
        private void gridMain_DragObjectOver(object sender, DevExpress.XtraGrid.Views.Base.DragObjectOverEventArgs e)
        {
            e.DropInfo.Valid = false;
            //if (e.DragObject is GridBand band)
            //{
            //    e.DropInfo.Valid = !(e.DropInfo.Index == -101); // when dragged to customization form
            //}
        }
        /****************************************************************************************/
        private void addNewNoteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            string data = dt.Rows[0]["notes"].ObjToString();
            data = data.TrimEnd('\n');
            string field = "Notes";

            DateTime today = DateTime.Now;
            if (!String.IsNullOrWhiteSpace(data))
                data += "\n";
            data += today.ToString("MM/dd/yyyy") + " ";

            using (EditTextData fmrmyform = new EditTextData(field, data))
            {
                fmrmyform.Text = "";
                fmrmyform.ShowDialog();
                if (fmrmyform.DialogResult == DialogResult.OK)
                {
                    string p = fmrmyform.Answer.Trim();
                    if (!String.IsNullOrWhiteSpace(p))
                    {
                        dt = (DataTable)dgv.DataSource;
                        dt.Rows[0]["notes"] = p;
                        dgv.DataSource = dt;

                        LoadNotes();

                        gridMain.RefreshEditor(true);
                        gridMain.RefreshData();
                        btnAccept.Show();
                        btnAccept.Refresh();
                    }
                }
            }
        }
        /****************************************************************************************/
        private void deleteCurrentNoteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv2.DataSource;
            DataRow dr = gridMain2.GetFocusedDataRow();
            if (dr == null)
                return;

            int row = gridMain2.FocusedRowHandle;
            row = gridMain2.GetDataSourceRowIndex(row);
            string note = dr["notes"].ObjToString();

            dt.Rows.Remove(dr);
            dt.AcceptChanges();

            string notes = "";
            string str = "";
            for ( int i=dt.Rows.Count-1; i>=0; i--)
            {
                str = dt.Rows[i]["notes"].ObjToString().Trim();
                if (String.IsNullOrWhiteSpace(str))
                    continue;
                str += "\n";
                notes += str;
            }
            dt = (DataTable)dgv.DataSource;
            dt.Rows[0]["notes"] = notes;
            dgv.DataSource = dt;

            LoadNotes();

            gridMain.RefreshEditor(true);
            gridMain.RefreshData();
            btnAccept.Show();
            btnAccept.Refresh();
        }
        /****************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            deleteCurrentNoteToolStripMenuItem_Click(sender, e);
        }
        /****************************************************************************************/
        private void gridMain2_DoubleClick(object sender, EventArgs e)
        {
            addNewNoteToolStripMenuItem_Click(sender, e);
        }
        /****************************************************************************************/
        private void pictureBox12_Click(object sender, EventArgs e)
        {
            addNewNoteToolStripMenuItem_Click(sender, e);
        }
        /****************************************************************************************/
        private void btnAccept_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            string record = dt.Rows[0]["record"].ObjToString();
            string notes = dt.Rows[0]["notes"].ObjToString();
            G1.update_db_table("contacts_preneed", "record", record, new string[] { "notes", notes });

            btnAccept.Hide();
            btnAccept.Refresh();
        }
        /****************************************************************************************/
        private void repositoryItemCheckEdit2_CheckedChanged(object sender, EventArgs e)
        {
            int rowHandle = gridMain.FocusedRowHandle;
            if ( rowHandle != 0 )
            {
                this.Validate();
                //MessageBox.Show("***ERROR*** Next Contact Completed can only be checked on the first row!", "Contact Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }
            if (editForm != null)
                editForm.FireEventModified();
        }
        /****************************************************************************************/
        private bool somethingDeleted = false;
        private void deleteCurrentRecordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();
            if (dr == null)
                return;

            int row = gridMain.FocusedRowHandle;
            row = gridMain.GetDataSourceRowIndex(row);

            string record = dr["record"].ObjToString();
            if ( !String.IsNullOrWhiteSpace ( record ))
            {
                string agent = dr["agent"].ObjToString();
                if (agent == primaryName || G1.isAdmin() || G1.isHR())
                {
                    DialogResult result = MessageBox.Show("Do you want to Delete This Preneed Contact?", "Delete Preneed Contact Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    if (result == DialogResult.Yes)
                    {
                        G1.delete_db_table("contacts_preneed", "record", record);
                        somethingDeleted = true;

                        try
                        {
                            dt.Rows.RemoveAt(row);
                            dt.AcceptChanges();
                            //gridMain.DeleteRow(gridMain.FocusedRowHandle);
                        }
                        catch (Exception ex)
                        {
                        }
                        //dt.Rows.Remove(dr);
                        //dt.AcceptChanges();

                        G1.NumberDataTable(dt);
                        dgv.DataSource = dt;
                        dgv.RefreshDataSource();
                        dgv.Refresh();
                    }
                }
                else
                {
                    MessageBox.Show("Do do not have permission to\ndelete this contact!", "Delete Contact Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                }
                this.Cursor = Cursors.Arrow;
            }
        }
        /****************************************************************************************/
        private void gridMain_CustomColumnDisplayText_1(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
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
        /****************************************************************************************/
    }
}