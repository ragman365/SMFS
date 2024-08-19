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
using System.Text.RegularExpressions;
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
using DevExpress.XtraBars;
using DevExpress.XtraGrid.Views.BandedGrid;
using DevExpress.XtraGrid;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class AgentMeetings : DevExpress.XtraEditors.XtraForm
    {
        private bool loading = true;
        private bool modified = false;
        /****************************************************************************************/
        public AgentMeetings()
        {
            InitializeComponent();
        }
        /****************************************************************************************/
        private void AgentMeetings_Load(object sender, EventArgs e)
        {
            btnSaveAll.Hide();

            loading = true;

            LoadAgents();

            string cmd = "select * from `agent_meetings` order by `meetingNumber`;";
            DataTable dt = G1.get_db_data(cmd);

            dt.Columns.Add("mod");
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;

            //this.TopMost = true;
            //this.FormBorderStyle = FormBorderStyle.SizableToolWindow;

            modified = false;
            loading = false;
        }
        /***********************************************************************************************/
        DataTable agentDt = null;
        private void LoadAgents ()
        {
//            string cmd = "Select DISTINCT `lastname`,`firstName` from `agents` WHERE `employeeStatus` = 'Full Time' AND `activeStatus` = 'Active' ORDER by `lastName`,`firstName`;";
            string cmd = "Select DISTINCT `lastname`,`firstName` from `agents` WHERE `employeeStatus` = 'Full Time' ORDER by `lastName`,`firstName`;";
            agentDt = G1.get_db_data(cmd);

            agentDt.Columns.Add("name");

            string name = "";
            string lastName = "";
            string firstName = "";
            for ( int i=0; i<agentDt.Rows.Count; i++)
            {
                firstName = agentDt.Rows[i]["firstName"].ObjToString();
                lastName = agentDt.Rows[i]["lastName"].ObjToString();

                name = lastName + ", " + firstName;
                agentDt.Rows[i]["name"] = name;
            }

            this.repositoryItemComboBox2.Items.Clear();
            for ( int i=0; i<agentDt.Rows.Count; i++)
            {
                name = agentDt.Rows[i]["name"].ObjToString();
                this.repositoryItemComboBox2.Items.Add(name);
            }
        }
        /***********************************************************************************************/
        private void SetupSelection(DataTable dt, DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew = null, string columnName = "")
        {
            if (selectnew == null)
                selectnew = this.repositoryItemCheckEdit1;
            selectnew.NullText = null;
            selectnew.ValueChecked = "Y";
            selectnew.ValueUnchecked = "N";
            selectnew.ValueGrayed = null;
            if (G1.get_column_number(dt, columnName) < 0)
                dt.Columns.Add(columnName);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (dt.Rows[i][columnName].ObjToString().ToUpper() != "Y")
                    dt.Rows[i][columnName] = "N";
            }
        }
        /****************************************************************************************/
        private void Contacts_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Validate();
            gridMain.RefreshEditor(true);
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
            //            Printer.DrawQuad(6, 8, 4, 4, "Funeral Services Report", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(5, 8, 8, 4, this.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            font = new Font("Ariel", 10, FontStyle.Regular);
            string reportName = this.Text;
            string report = reportName;
            Printer.DrawQuad(5, 8, 8, 4, report, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);


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
        private void pictureDelete_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string data = dr["agent"].ObjToString();
            DialogResult result = MessageBox.Show("***Question*** Are you sure you want to DELETE this\nAgent Meeting for (" + data + ") ?", "Delete Agent Meeting Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt == null)
                return;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            dr["Mod"] = "D";
            dt.Rows[row]["mod"] = "D";

            btnSaveAll.Show();
            btnSaveAll.Refresh();
        }
        /****************************************************************************************/
        private void pictureAdd_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dRow = dt.NewRow();

            //LoadNewRow(dRow);

            dt.Rows.Add(dRow);

            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            gridMain.RefreshData();
            gridMain_CellValueChanged(null, null);

            G1.GoToLastRow(gridMain);
        }
        /****************************************************************************************/
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (loading)
                return;

            if (e == null)
                return;

            DataChanged();

            modified = true;
            btnSaveAll.Show();
            btnSaveAll.Refresh();
            DataRow dr = gridMain.GetFocusedDataRow();
            dr["mod"] = "Y";
        }
        /****************************************************************************************/
        private void pictureBox4_Click(object sender, EventArgs e)
        {
            G1.SpyGlass(gridMain);
        }
        /****************************************************************************************/
        private void btnSaveAll_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            string record = "";
            string mod = "";
            DateTime effectiveFromDate = DateTime.Now;
            DateTime effectiveToDate = DateTime.Now;
            string meetingNumber = "";
            string agent = "";
            string agentLastName = "";
            string agentFirstName = "";
            string location = "";
            double attendees = 0;
            double commissionPercent = 0D;
            double splitCommissionPercent = 0D;

            this.Cursor = Cursors.WaitCursor;

            string cmd = "DELETE from `agent_meetings` WHERE `agent` = '-1'";
            G1.get_db_data(cmd);

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    record = dt.Rows[i]["record"].ObjToString();
                    mod = dt.Rows[i]["mod"].ObjToString();
                    if (mod == "D")
                    {
                        if (!String.IsNullOrWhiteSpace(record))
                            G1.delete_db_table("agent_meetings", "record", record);
                        continue;
                    }
                    if (mod != "Y")
                        continue;

                    meetingNumber = dt.Rows[i]["meetingNumber"].ObjToString();
                    effectiveFromDate = dt.Rows[i]["effectiveFromDate"].ObjToDateTime();
                    effectiveToDate = dt.Rows[i]["effectiveToDate"].ObjToDateTime();
                    location = dt.Rows[i]["location"].ObjToString();
                    attendees = dt.Rows[i]["attendees"].ObjToDouble();
                    agent = dt.Rows[i]["agent"].ObjToString();
                    agentLastName = dt.Rows[i]["agentLastName"].ObjToString();
                    agentFirstName = dt.Rows[i]["agentFirstName"].ObjToString();
                    commissionPercent = dt.Rows[i]["commissionPercent"].ObjToDouble();
                    splitCommissionPercent = dt.Rows[i]["splitCommissionPercent"].ObjToDouble();

                    if (String.IsNullOrWhiteSpace(record))
                        record = G1.create_record("agent_meetings", "agent", "-1");
                    if (G1.BadRecord("agent_meetings", record))
                        return;

                    G1.update_db_table("agent_meetings", "record", record, new string[] { "meetingNumber", meetingNumber.ToString(), "agent", agent, "agentLastName", agentLastName, "agentFirstName", agentFirstName, "effectiveFromDate", effectiveFromDate.ToString("yyyyMMdd"), "effectiveToDate", effectiveToDate.ToString("yyyyMMdd"), "commissionPercent", commissionPercent.ToString(), "splitCommissionPercent", splitCommissionPercent.ToString(), "location", location, "attendees", attendees.ToString() });
                }
                catch ( Exception ex)
                {
                }
            }
            modified = false;
            btnSaveAll.Hide();

            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void repositoryItemCheckEdit1_CheckedChanged(object sender, EventArgs e)
        {
            int rowHandle = gridMain.FocusedRowHandle;
            string column = gridMain.FocusedColumn.FieldName.Trim();
            DataRow dr = gridMain.GetFocusedDataRow();
            if (dr == null)
                return;
            string isOn = dr[column].ObjToString();
            if (isOn.ToUpper() == "Y")
                dr[column] = "N";
            else
                dr[column] = "Y";
            DataChanged();
        }
        /***********************************************************************************************/
        private void DataChanged()
        {
            if (loading)
                return;

            btnSaveAll.Show();
            btnSaveAll.Refresh();

            int rowHandle = gridMain.FocusedRowHandle;
            DataRow dr = gridMain.GetFocusedDataRow();
            if (dr == null)
                return;
            dr["mod"] = "Y";
        }
        /****************************************************************************************/
        private void gridMain_MouseDown(object sender, MouseEventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource; // Leave as a GetDate example

            var hitInfo = gridMain.CalcHitInfo(e.Location);
            if (hitInfo.InRowCell)
            {
                int rowHandle = hitInfo.RowHandle;
                gridMain.FocusedRowHandle = rowHandle;
                gridMain.SelectRow(rowHandle);
                gridMain.RefreshEditor(true);
                GridColumn column = hitInfo.Column;
                gridMain.FocusedColumn = column;
                string currentColumn = column.FieldName.Trim();
                if (currentColumn.ToUpper() == "EFFECTIVEFROMDATE")
                {
                    DataRow dr = gridMain.GetFocusedDataRow();
                    DateTime date = dr["effectiveFromDate"].ObjToDateTime();
                    if (date.Year < 1000)
                        date = DateTime.Now;
                    using (GetDate dateForm = new GetDate(date, "Effective From Date"))
                    {
                        dateForm.TopMost = true;
                        dateForm.ShowDialog();
                        if (dateForm.DialogResult == System.Windows.Forms.DialogResult.OK)
                        {
                            date = dateForm.myDateAnswer;
                            dr["effectiveFromDate"] = G1.DTtoMySQLDT(date);
                            DataChanged();
                            gridMain.ClearSelection();
                            gridMain.FocusedRowHandle = rowHandle;

                            gridMain.RefreshData();
                            gridMain.RefreshEditor(true);
                            gridMain.SelectRow(rowHandle);
                        }
                    }
                }
                else if (currentColumn.ToUpper() == "EFFECTIVETODATE")
                {
                    DataRow dr = gridMain.GetFocusedDataRow();
                    DateTime date = dr["effectiveToDate"].ObjToDateTime();
                    if (date.Year < 1000)
                        date = DateTime.Now;
                    using (GetDate dateForm = new GetDate(date, "Effective To Date"))
                    {
                        dateForm.TopMost = true;
                        dateForm.ShowDialog();
                        if (dateForm.DialogResult == System.Windows.Forms.DialogResult.OK)
                        {
                            date = dateForm.myDateAnswer;
                            dr["effectiveToDate"] = G1.DTtoMySQLDT(date);
                            DataChanged();
                            gridMain.ClearSelection();
                            gridMain.FocusedRowHandle = rowHandle;

                            gridMain.RefreshData();
                            gridMain.RefreshEditor(true);
                            gridMain.SelectRow(rowHandle);
                        }
                    }
                }
            }
        }
        /****************************************************************************************/
        private void gridMain_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
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
        private void repositoryItemComboBox2_EditValueChanged(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            ComboBoxEdit combo = (ComboBoxEdit)sender;
            string agent = combo.Text;
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);

            DataRow [] dRows = agentDt.Select("name='" + agent + "'");
            if ( dRows.Length > 0 )
            {
                dr["agent"] = agent;
                dr["agentFirstName"] = dRows[0]["firstName"].ObjToString();
                dr["agentLastName"] = dRows[0]["lastName"].ObjToString();

                dt.Rows[row]["agent"] = agent;
                dt.Rows[row]["agentFirstName"] = dRows[0]["firstName"].ObjToString();
                dt.Rows[row]["agentLastName"] = dRows[0]["lastName"].ObjToString();

                gridMain.RefreshData();
                gridMain.RefreshEditor(true);
                dgv.Refresh();
            }
            DataChanged();
        }
        /****************************************************************************************/
        private void reportMeetingContractsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string meetingNumber = dr["meetingNumber"].ObjToString();

            string cmd = "Select * from `customers` WHERE `meetingNumber` = '" + meetingNumber.ToString() + "';";
            DataTable dt = G1.get_db_data(cmd);

            using (ViewDataTable viewForm = new ViewDataTable(dt, "contractNumber,firstName, lastName "))
            {
                viewForm.Text = "Contracts Assigned Meeting Number " + meetingNumber.ToString();
                viewForm.ShowDialog();
            }

        }
        /****************************************************************************************/
        private string oldWhat = "";
        private void gridMain_ValidatingEditor(object sender, DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventArgs e)
        {
            GridView view = sender as GridView;
            if (view.FocusedColumn.FieldName.ToUpper() == "EFFECTIVEFROMDATE")
            {
                DataTable dt = (DataTable)dgv.DataSource;
                DataRow dr = gridMain.GetFocusedDataRow();
                int rowhandle = gridMain.FocusedRowHandle;
                int row = gridMain.GetDataSourceRowIndex(rowhandle);
                oldWhat = e.Value.ObjToString();
                DateTime date = oldWhat.ObjToDateTime();
                dt.Rows[row]["effectiveFromDate"] = G1.DTtoMySQLDT(date);
                e.Value = G1.DTtoMySQLDT(date);
            }
            else if (view.FocusedColumn.FieldName.ToUpper() == "EFFECTIVETODATE")
            {
                DataTable dt = (DataTable)dgv.DataSource;
                DataRow dr = gridMain.GetFocusedDataRow();
                int rowhandle = gridMain.FocusedRowHandle;
                int row = gridMain.GetDataSourceRowIndex(rowhandle);
                oldWhat = e.Value.ObjToString();
                DateTime date = oldWhat.ObjToDateTime();
                dt.Rows[row]["effectiveToDate"] = G1.DTtoMySQLDT(date);
                e.Value = G1.DTtoMySQLDT(date);
            }
        }
        /****************************************************************************************/
    }
}