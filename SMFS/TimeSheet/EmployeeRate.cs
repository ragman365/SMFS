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
    public partial class EmployeeRate : DevExpress.XtraEditors.XtraForm
    {
        private bool loading = true;
        private bool modified = false;
        private string workUserName = "";
        private string workId = "";
        /****************************************************************************************/
        public EmployeeRate( string empId, string userName )
        {
            InitializeComponent();
            workId = empId;
            workUserName = userName;
        }
        /****************************************************************************************/
        private void EmployeeRate_Load(object sender, EventArgs e)
        {
            btnSaveAll.Hide();

            loading = true;

            string cmd = "select * from `tc_rates` WHERE `userName` = '" + workUserName + "' order by `effectiveDate` DESC;";
            DataTable dt = G1.get_db_data(cmd);

            dt.Columns.Add("mod");
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;

            SetupSelection(dt, this.repositoryItemCheckEdit1, "isBPM");
            SetupSelection(dt, this.repositoryItemCheckEdit1, "splitBPM");
            SetupSelection(dt, this.repositoryItemCheckEdit1, "salaried");
            SetupSelection(dt, this.repositoryItemCheckEdit1, "flux");
            SetupSelection(dt, this.repositoryItemCheckEdit1, "excludePayroll");


            //this.TopMost = true;
            //this.FormBorderStyle = FormBorderStyle.SizableToolWindow;

            modified = false;
            loading = false;
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
            string data = dr["rate"].ObjToString();
            DialogResult result = MessageBox.Show("***Question*** Are you sure you want to DELETE this Historic Rate (" + data + ") ?", "Delete Rate Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
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

            LoadNewRow(dRow);

            dt.Rows.Add(dRow);

            SetupSelection(dt, this.repositoryItemCheckEdit1, "isBPM");
            SetupSelection(dt, this.repositoryItemCheckEdit1, "splitBPM");
            SetupSelection(dt, this.repositoryItemCheckEdit1, "salaried");
            SetupSelection(dt, this.repositoryItemCheckEdit1, "flux");
            SetupSelection(dt, this.repositoryItemCheckEdit1, "excludePayroll");

            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            gridMain.RefreshData();
            gridMain_CellValueChanged(null, null);

            G1.GoToLastRow(gridMain);
        }
        /****************************************************************************************/
        private void LoadNewRow ( DataRow dr )
        {
            try
            {
                string cmd = "Select * from `tc_er` WHERE `userName` = '" + workUserName + "';";
                DataTable dt = G1.get_db_data(cmd);
                if (dt.Rows.Count <= 0)
                    return;
                dr["EmpStatus"] = dt.Rows[0]["EmpStatus"].ObjToString();
                dr["EmpType"] = dt.Rows[0]["EmpType"].ObjToString();
                dr["rate"] = dt.Rows[0]["rate"].ObjToDouble();
                dr["futureRate"] = dt.Rows[0]["futureRate"].ObjToDouble();
                dr["biWeekly"] = dt.Rows[0]["biWeekly"].ObjToDouble();
                dr["futureBiWeekly"] = dt.Rows[0]["futureBiWeekly"].ObjToDouble();
                dr["salary"] = dt.Rows[0]["salary"].ObjToDouble();
                dr["futureSalary"] = dt.Rows[0]["futureSalary"].ObjToDouble();
                dr["vacationOverride"] = dt.Rows[0]["vacationOverride"].ObjToDouble();

                dr["futureEffectiveDate"] = dt.Rows[0]["effectiveDate"];

                dr["isBPM"] = dt.Rows[0]["isBPM"].ObjToString();
                dr["splitBPM"] = dt.Rows[0]["splitBPM"].ObjToString();
                dr["salaried"] = dt.Rows[0]["salaried"].ObjToString();
                dr["flux"] = dt.Rows[0]["flux"].ObjToString();
                dr["excludePayroll"] = dt.Rows[0]["excludePayroll"].ObjToString();
            }
            catch ( Exception ex)
            {
            }
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
            string excludePayroll = "";
            DateTime effectiveDate = DateTime.Now;
            DateTime futureEffectiveDate = DateTime.Now;
            string salaried = "";
            string flux = "";
            string EmpStatus = "";
            string EmpType = "";
            double rate = 0D;
            double futureRate = 0D;
            double salary = 0D;
            double futureSalary = 0D;
            double biWeekly = 0D;
            double futureBiWeekly = 0D;
            string isBPM = "";
            string splitBPM = "";
            double vacationOverride = 0D;
            double fullTimeHours = 0D;

            this.Cursor = Cursors.WaitCursor;

            string cmd = "DELETE from `tc_rates` WHERE `excludePayroll` = '-1'";
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
                            G1.delete_db_table("tc_rates", "record", record);
                        continue;
                    }
                    if (mod != "Y")
                        continue;

                    excludePayroll = dt.Rows[i]["excludePayroll"].ObjToString();
                    effectiveDate = dt.Rows[i]["effectiveDate"].ObjToDateTime();
                    futureEffectiveDate = dt.Rows[i]["futureEffectiveDate"].ObjToDateTime();
                    salaried = dt.Rows[i]["salaried"].ObjToString();
                    flux = dt.Rows[i]["flux"].ObjToString();
                    EmpStatus = dt.Rows[i]["EmpStatus"].ObjToString();
                    EmpType = dt.Rows[i]["EmpType"].ObjToString();
                    rate = dt.Rows[i]["rate"].ObjToDouble();
                    futureRate = dt.Rows[i]["futureRate"].ObjToDouble();
                    salary = dt.Rows[i]["salary"].ObjToDouble();
                    futureSalary = dt.Rows[i]["futureSalary"].ObjToDouble();
                    biWeekly = dt.Rows[i]["biWeekly"].ObjToDouble();
                    futureBiWeekly = dt.Rows[i]["biWeekly"].ObjToDouble();
                    isBPM = dt.Rows[i]["isBPM"].ObjToString();
                    splitBPM = dt.Rows[i]["splitBPM"].ObjToString();
                    vacationOverride = dt.Rows[i]["vacationOverride"].ObjToDouble();
                    fullTimeHours = dt.Rows[i]["fullTimeHours"].ObjToDouble();

                    if (String.IsNullOrWhiteSpace(record))
                        record = G1.create_record("tc_rates", "excludePayroll", "-1");
                    if (G1.BadRecord("tc_rates", record))
                        return;

                    G1.update_db_table("tc_rates", "record", record, new string[] { "excludePayroll", excludePayroll, "userName", workUserName, "userId", workId, "effectiveDate", effectiveDate.ToString("yyyyMMdd"), "futureEffectiveDate", futureEffectiveDate.ToString("yyyyMMdd") });


                    G1.update_db_table("tc_rates", "record", record, new string[] { "salaried", salaried, "flux", flux, "EmpStatus", EmpStatus, "EmpType", EmpType, "rate", rate.ToString(), "futureRate", futureRate.ToString(), "isBPM", isBPM, "splitBPM", splitBPM }) ;
                    G1.update_db_table("tc_rates", "record", record, new string[] { "salary", salary.ToString(), "futureSalary", futureSalary.ToString(), "biWeekly", biWeekly.ToString(), "futureBiWeekly", futureBiWeekly.ToString(), "vacationOverride", vacationOverride.ToString(), "fullTimeHours", fullTimeHours.ToString() });

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
                GridColumn column = hitInfo.Column;
                string currentColumn = column.FieldName.Trim();
                if (currentColumn.ToUpper() == "EFFECTIVEDATE")
                {
                    DataRow dr = gridMain.GetFocusedDataRow();
                    DateTime date = dr["effectiveDate"].ObjToDateTime();
                    if (date.Year < 1000)
                        date = DateTime.Now;
                    using (GetDate dateForm = new GetDate(date, "Effective Date"))
                    {
                        dateForm.TopMost = true;
                        dateForm.ShowDialog();
                        if (dateForm.DialogResult == System.Windows.Forms.DialogResult.OK)
                        {
                            date = dateForm.myDateAnswer;
                            dr["effectiveDate"] = G1.DTtoMySQLDT(date);
                            DataChanged();
                        }
                    }
                }
                else if (currentColumn.ToUpper() == "FUTUREEFFECTIVEDATE")
                {
                    DataRow dr = gridMain.GetFocusedDataRow();
                    DateTime date = dr["effectiveDate"].ObjToDateTime();
                    if (date.Year < 1000)
                        date = DateTime.Now;
                    using (GetDate dateForm = new GetDate(date, "Future Effective Date"))
                    {
                        dateForm.TopMost = true;
                        dateForm.ShowDialog();
                        if (dateForm.DialogResult == System.Windows.Forms.DialogResult.OK)
                        {
                            date = dateForm.myDateAnswer;
                            dr["futureEffectiveDate"] = G1.DTtoMySQLDT(date);
                            DataChanged();
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
    }
}