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
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class FuneralDelays : DevExpress.XtraEditors.XtraForm
    {
        private bool loading = true;
        private bool modified = false;
        private string primaryName = "";
        private DataTable funDt = null;
        /****************************************************************************************/
        public FuneralDelays()
        {
            InitializeComponent();
        }
        /****************************************************************************************/
        private void SetupToolTips()
        {
            ToolTip tt = new ToolTip();
        }
        /****************************************************************************************/
        private void FuneralDelays_Load(object sender, EventArgs e)
        {
            oldWhat = "";

            SetupToolTips();

            loading = true;

            DateTime now = DateTime.Now;
//            now = now.AddMonths(-1);
            now = new DateTime(now.Year, now.Month, 1);
            this.dateTimePicker1.Value = now;
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            this.dateTimePicker2.Value = new DateTime(now.Year, now.Month, days);

            string cmd = "Select * from `funeralhomes` order by `keycode`;";
            funDt = G1.get_db_data(cmd);


            getLocations();

            modified = false;
            loading = false;
        }
        /****************************************************************************************/
        private void btnRight_Click(object sender, EventArgs e)
        {
            DateTime now = this.dateTimePicker2.Value;
            now = now.AddMonths(1);
            now = new DateTime(now.Year, now.Month, 1);
            this.dateTimePicker1.Value = now;
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            this.dateTimePicker2.Value = new DateTime(now.Year, now.Month, days);
        }
        /****************************************************************************************/
        private void btnLeft_Click(object sender, EventArgs e)
        {
            DateTime now = this.dateTimePicker1.Value;
            now = now.AddMonths(-1);
            now = new DateTime(now.Year, now.Month, 1);
            this.dateTimePicker1.Value = now;
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            this.dateTimePicker2.Value = new DateTime(now.Year, now.Month, days);
        }
        /***********************************************************************************************/
        private void LoadData()
        {
            this.Cursor = Cursors.WaitCursor;

            this.Cursor = Cursors.WaitCursor;
            DataTable dt = null;

            string contractsFile = "fcontracts";
            string customersFile = "fcustomers";

            string cmd = "SELECT * FROM `fcust_extended` e LEFT JOIN `fcontracts` p ON p.`contractNumber` = e.`contractNumber` left join `fcustomers` d ON e.`contractNumber` = d.`contractNumber` LEFT JOIN `icontracts` i ON i.`contractNumber` = e.`contractNumber` LEFT JOIN `icustomers` j ON j.`contractNumber` = e.`contractNumber` WHERE e.`ServiceID` <> '' ";

            string date1 = this.dateTimePicker1.Value.ToString("yyyy-MM-dd");
            string date2 = this.dateTimePicker2.Value.ToString("yyyy-MM-dd");

            cmd += " AND p.`deceasedDate` >= '" + date1 + "' AND p.`deceasedDate` <= '" + date2 + "' ";

            string names = getLocationNameQuery();
            if (!String.IsNullOrWhiteSpace(names))
                cmd += " AND " + names + " ";

            cmd += " ORDER by p.`deceasedDate` ";
            cmd += ";";

            dt = G1.get_db_data(cmd);

            dt = PreProcessData(dt);

            dgv.DataSource = dt;

            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private DataTable _LocationList;
        private void getLocations()
        {
            //string cmd = "SELECT `LocationCode` FROM `inventory` GROUP BY `LocationCode` ASC;";
            string cmd = "Select * from `funeralhomes` order by `atneedcode`;";
            _LocationList = G1.get_db_data(cmd);

            string str = "";

            for (int i = _LocationList.Rows.Count - 1; i >= 0; i--)
            {
                str = _LocationList.Rows[i]["atneedcode"].ObjToString();
                if (String.IsNullOrWhiteSpace(str))
                    _LocationList.Rows.RemoveAt(i);
            }

            chkComboLocation.Properties.DataSource = _LocationList;
        }
        /*******************************************************************************************/
        private string getLocationNameQuery()
        {
            string procLoc = "";
            string[] locIDs = this.chkComboLocation.EditValue.ToString().Split('|');
            DataRow[] dRows = null;
            DataTable dt = (DataTable)chkComboLocation.Properties.DataSource;
            for (int i = 0; i < locIDs.Length; i++)
            {
                if (!String.IsNullOrWhiteSpace(locIDs[i]))
                {
                    if (procLoc.Trim().Length > 0)
                        procLoc += ",";
                    dRows = dt.Select("LocationCode='" + locIDs[i].Trim() + "'");
                    if (dRows.Length > 0)
                    {
                        procLoc += "'" + dRows[0]["atneedcode"].ObjToString().Trim() + "'";
                        //procLoc += "'" + locIDs[i].Trim() + "'";
                    }
                }
            }
            return procLoc.Length > 0 ? " `serviceLoc` IN (" + procLoc + ") " : "";
        }
        /***********************************************************************************************/
        private DataTable PreProcessData(DataTable dt)
        {
            string contractNumber = "";
            string lastName = "";
            string firstName = "";
            string name = "";
            DateTime deceasedDate = DateTime.Now;
            DateTime serviceDate = DateTime.Now;
            int delay = 0;
            if (G1.get_column_number(dt, "deceasedName") < 0)
                dt.Columns.Add("deceasedName");
            if (G1.get_column_number(dt, "delay") < 0)
                dt.Columns.Add("delay", Type.GetType("System.Int32"));
            if (G1.get_column_number(dt, "Location Name") < 0)
                dt.Columns.Add("Location Name");

            string str = txtOutstanding.Text.Trim();
            if (String.IsNullOrWhiteSpace(str))
                str = "4";
            double outDays = str.ObjToDouble();
            TimeSpan ts;
            DataRow[] dRows = null;
            string serviceLoc = "";

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                    deceasedDate = dt.Rows[i]["deceasedDate"].ObjToDateTime();
                    serviceDate = dt.Rows[i]["serviceDate"].ObjToDateTime();
                    if (serviceDate.Year < 100)
                        serviceDate = DateTime.Now;
                    ts = serviceDate - deceasedDate;

                    delay = (int) ts.TotalDays;
                    if (delay <= outDays)
                        delay = 0;
                    dt.Rows[i]["delay"] = delay;

                    firstName = dt.Rows[i]["firstName"].ObjToString();
                    lastName = dt.Rows[i]["lastName"].ObjToString();
                    name = lastName + ", " + firstName;
                    dt.Rows[i]["deceasedName"] = name;

                    serviceLoc = dt.Rows[i]["serviceLoc"].ObjToString();

                    dRows = funDt.Select("atneedcode='" + serviceLoc + "'");
                    if (dRows.Length > 0)
                        dt.Rows[i]["Location Name"] = dRows[0]["LocationCode"].ObjToString();
                    else
                        dt.Rows[i]["Location Name"] = serviceLoc;

                    dRows = funDt.Select("merchandiseCode='" + serviceLoc + "'");
                    if (dRows.Length > 0)
                    {
                        //string lName = dRows[0]["LocationCode"].ObjToString();
                        //dt.Rows[i]["Location Name"] = dRows[0]["LocationCode"].ObjToString();
                        dt.Rows[i]["delay"] = 0;
                    }
                }
                catch ( Exception ex)
                {
                }
            }

            dRows = dt.Select("delay>'0'");
            if ( dRows.Length > 0)
                dt = dRows.CopyToDataTable();
            G1.NumberDataTable(dt);
            return dt;
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
            int outDays = txtOutstanding.Text.ObjToInt32();
            int days = dt.Rows[row]["delay"].ObjToInt32();
            if ( days <= outDays )
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
                if (e.Column.FieldName.ToUpper().IndexOf ( "DATE") >= 0 )
                    doDate = true;

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

            GridColumn currCol = gridMain.FocusedColumn;
            string currentColumn = currCol.FieldName;
            string what = dr[currentColumn].ObjToString();
            if (currentColumn.ToUpper() == "NUM")
                return;
            string record = dr["record"].ObjToString();
            if (!String.IsNullOrWhiteSpace(record))
            {
            }

            gridMain.RefreshData();
        }
        /****************************************************************************************/
        private void UpdateMod(DataRow dr)
        {
//            dr["mod"] = "Y";
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
        private void gridMain_KeyDown(object sender, KeyEventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            //AddMod(dt, gridMain);
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
                return;
            }
            //bool doDate = false;
            //bool doTime = false;
            //if (name == "apptDate")
            //    doDate = true;
            //else if (name == "lastContactDate")
            //    doDate = true;

            //if (doDate)
            //{
            //    DateTime date = e.DisplayText.ObjToString().ObjToDateTime();
            //    if (date.Year < 30)
            //        e.DisplayText = "";
            //    else
            //    {
            //        e.DisplayText = date.ToString("MM/dd/yyyy");
            //    }
            //}

            //if (doTime)
            //{
            //    if (!String.IsNullOrWhiteSpace(e.DisplayText.Trim()))
            //    {
            //        DateTime date = e.DisplayText.ObjToString().ObjToDateTime();
            //        e.DisplayText = date.ToString("HH:mm");
            //    }
            //}
        }
        /****************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            DataTable dt = (DataTable)dgv.DataSource;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetFocusedDataSourceRowIndex();

            string record = dr["record"].ObjToString();
            string contractNumber = dr["contractNumber"].ObjToString();
            if (!String.IsNullOrWhiteSpace(contractNumber))
            {
                this.Cursor = Cursors.WaitCursor;

                Form form = G1.IsFormOpen("EditCust", contractNumber);
                if (form != null)
                {
                    form.Show();
                    form.WindowState = FormWindowState.Maximized;
                    form.Visible = true;
                    form.BringToFront();
                }
                else
                {
                    EditCust custForm = new EditCust(contractNumber);
                    custForm.Tag = contractNumber;
                    custForm.Show();
                }
                this.Cursor = Cursors.Default;
            }
        }
        /****************************************************************************************/
        private void FunPayments_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Validate();
            gridMain.RefreshEditor(true);
        }
        /****************************************************************************************/
        private void gridMain_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();

            int focusedRow = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(focusedRow);

            //string initialized = dt.Rows[row]["initialized"].ObjToString();

            //string saveDescription = dr["localDescription"].ObjToString();
            //string saveBank = dr["bankAccount"].ObjToString();

            //try
            //{
            //    string type = dr["type"].ObjToString().ToUpper();
            //    string what = dr["status"].ObjToString().ToUpper();
            //    row = gridMain.GetDataSourceRowIndex(row);
            //    //if ( !loading )
            //    //    dt.Rows[row]["dateModified"] = G1.DTtoMySQLDT(DateTime.Now);
            //    if (what.ToUpper() == "DEPOSITED")
            //    {
            //        string bankAccount = GetDepositBankAccount(type);
            //        if (!String.IsNullOrWhiteSpace(bankAccount))
            //        {
            //            dr["bankAccount"] = bankAccount;
            //            dt.Rows[row]["bankAccount"] = bankAccount;
            //            gridMain.RefreshEditor(true);
            //            dgv.RefreshDataSource();
            //            dgv.Refresh();
            //        }
            //    }
            //    else
            //    {
            //        saveBank = "";
            //        saveDescription = "";
            //        dr["bankAccount"] = "";
            //        dr["localDescription"] = "";
            //        dt.Rows[row]["bankAccount"] = "";
            //        dt.Rows[row]["localDescription"] = "";
            //    }
            //    if (!String.IsNullOrWhiteSpace(saveDescription))
            //    {
            //        dr["bankAccount"] = saveBank;
            //        dr["localDescription"] = saveDescription;
            //        dt.Rows[row]["bankAccount"] = saveBank;
            //        dt.Rows[row]["localDescription"] = saveDescription;
            //    }
            //}
            //catch (Exception ex)
            //{
            //}
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

            string str = this.Text + " By More Than " + txtOutstanding.Text.Trim() + " Days";
            Printer.DrawQuad(5, 8, 8, 4, str, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

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
        private void gridMain_ValidatingEditor(object sender, DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventArgs e)
        {
            GridView view = sender as GridView;
            //if (view.FocusedColumn.FieldName.ToUpper() == "STATUS")
            //{
            //    DataTable dt = (DataTable)dgv.DataSource;
            //    DataRow dr = gridMain.GetFocusedDataRow();
            //    int rowhandle = gridMain.FocusedRowHandle;
            //    int row = gridMain.GetDataSourceRowIndex(rowhandle);
            //    oldWhat = e.Value.ObjToString();
            //    string status = dr["status"].ObjToString().ToUpper();
            //    if ( status == "CANCELLED")
            //    {
            //        string record = dr["record"].ObjToString();
            //        if (!String.IsNullOrWhiteSpace(record))
            //        {
            //            string cmd = "Select * from `cust_payment_details` WHERE `paymentRecord` = '" + record + "';";
            //            DataTable dx = G1.get_db_data(cmd);
            //            if (dx.Rows.Count > 0)
            //            {
            //                for (int i = 0; i < dx.Rows.Count; i++)
            //                {
            //                    record = dx.Rows[0]["record"].ObjToString();
            //                    G1.update_db_table("cust_payment_details", "record", record, new string[] { "status", "Cancelled" });

            //                    btnSavePayments_Click(null, null);
            //                    btnSavePayments.Hide();
            //                    btnSavePayments.Refresh();
            //                    justSaved = true;
            //                }
            //            }
            //        }
            //    }
            //}
            //else if (view.FocusedColumn.FieldName.ToUpper() == "DATEENTERED")
            //{
            //    DataTable dt = (DataTable)dgv.DataSource;
            //    DataRow dr = gridMain.GetFocusedDataRow();
            //    int rowhandle = gridMain.FocusedRowHandle;
            //    int row = gridMain.GetDataSourceRowIndex(rowhandle);
            //    oldWhat = e.Value.ObjToString();
            //    DateTime date = oldWhat.ObjToDateTime();
            //    dt.Rows[row]["dateEntered"] = G1.DTtoMySQLDT(date);
            //    e.Value = G1.DTtoMySQLDT(date);
            //}
            //else if (view.FocusedColumn.FieldName.ToUpper() == "TRUST_POLICY")
            //{
            //    DataTable dt = (DataTable)dgv.DataSource;
            //    DataRow dr = gridMain.GetFocusedDataRow();
            //    int rowhandle = gridMain.FocusedRowHandle;
            //    int row = gridMain.GetDataSourceRowIndex(rowhandle);
            //    oldWhat = e.Value.ObjToString();
            //}
            //else if (view.FocusedColumn.FieldName.ToUpper() == "PAYMENT")
            //{
            //    DataTable dt = (DataTable)dgv.DataSource;
            //    DataRow dr = gridMain.GetFocusedDataRow();
            //    int rowhandle = gridMain.FocusedRowHandle;
            //    int row = gridMain.GetDataSourceRowIndex(rowhandle);
            //    oldWhat = e.Value.ObjToString();

            //    string record = dr["record"].ObjToString();
            //    if (!String.IsNullOrWhiteSpace(record))
            //    {
            //        string cmd = "Select * from `cust_payment_details` WHERE `paymentRecord` = '" + record + "';";
            //        DataTable dx = G1.get_db_data(cmd);
            //        if ( dx.Rows.Count > 0 )
            //        {
            //            double payment = dr["payment"].ObjToDouble();
            //            payment = oldWhat.ObjToDouble();
            //            record = dx.Rows[0]["record"].ObjToString();
            //            G1.update_db_table("cust_payment_details", "record", record, new string[] {"paid", payment.ToString() });

            //            btnSavePayments_Click(null, null);
            //            btnSavePayments.Hide();
            //            btnSavePayments.Refresh();
            //            funModified = false;
            //            justSaved = true;
            //        }
            //    }
            //}
        }
        private string oldWhat = "";
        /****************************************************************************************/
        private void gridMain_CustomRowCellEdit(object sender, CustomRowCellEditEventArgs e)
        {
            GridView view = sender as GridView;
            //if (e.Column.FieldName.ToUpper() == "CHECKLIST")
            //{
            //    string type = view.GetRowCellValue(e.RowHandle, "type").ObjToString().ToUpper();
            //    if (type != "INSURANCE" && type != "POLICY" && type != "INSURANCE DIRECT" && type != "INSURANCE UNITY" && type != "3RD PARTY")
            //    {
            //        e.RepositoryItem = null;
            //        return;
            //    }
            //    string status = view.GetRowCellValue(e.RowHandle, "status").ObjToString();
            //    if (status.ToUpper() == "FILED")
            //        e.RepositoryItem = this.repositoryItemButtonEdit2;
            //    else if ( status.ToUpper() == "DEPOSITED")
            //        e.RepositoryItem = this.repositoryItemButtonEdit1;
            //    else
            //        e.RepositoryItem = this.repositoryItemButtonEdit2;
            //}
        }
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
        private string oldContactType = "";
        /****************************************************************************************/
        public static string GetPerson ( DataRow dRow )
        {
            string prefix = dRow["depPrefix"].ObjToString();
            string lastName = dRow["depLastName"].ObjToString();
            string firstName = dRow["depFirstName"].ObjToString();
            string middleName = dRow["depMI"].ObjToString();
            string suffix = dRow["depSuffix"].ObjToString();
            string name = prefix;
            name = BuildName(name, lastName);
            if (!String.IsNullOrWhiteSpace(name))
                name += ",";
            name = BuildName(name, firstName);
            name = BuildName(name, middleName);
            name = BuildName(name, suffix);
            return name;
        }
        /***********************************************************************************************/
        public static string BuildName(string name, string text)
        {
            if (!String.IsNullOrWhiteSpace(text))
            {
                if (!String.IsNullOrWhiteSpace(name))
                    name += " ";
                name += text;
            }
            return name;
        }
        /****************************************************************************************/
        private void comboBox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            System.Windows.Forms.ComboBox cbo = (System.Windows.Forms.ComboBox)sender;
            cbo.PreviewKeyDown -= comboBox_PreviewKeyDown;
            if (cbo.DroppedDown) cbo.Focus();
        }
        /****************************************************************************************/
        private void btnRun_Click(object sender, EventArgs e)
        {
            LoadData();
        }
        /****************************************************************************************/
        private void gridMain_CalcRowHeight(object sender, RowHeightEventArgs e)
        {
            //GridView View = sender as GridView;
            //if (e.RowHandle >= 0)
            //{
            //    int maxHeight = 0;
            //    int newHeight = 0;
            //    bool doit = false;
            //    string name = "";
            //    string str = "";
            //    int count = 0;
            //    string[] Lines = null;
            //    foreach (GridColumn column in gridMain.Columns)
            //    {
            //        name = column.FieldName.ToUpper();
            //        if (name == "RESULTS" )
            //            doit = true;
            //        if (doit)
            //        {
            //            using (RepositoryItemMemoEdit edit = new RepositoryItemMemoEdit())
            //            {
            //                using (MemoEditViewInfo viewInfo = edit.CreateViewInfo() as MemoEditViewInfo)
            //                {
            //                    str = gridMain.GetRowCellValue(e.RowHandle, column.FieldName).ObjToString();
            //                    if ( !String.IsNullOrWhiteSpace ( str ))
            //                    {
            //                        Lines = str.Split('\n');
            //                        count = Lines.Length + 1;
            //                    }
            //                    viewInfo.EditValue = gridMain.GetRowCellValue(e.RowHandle, column.FieldName);
            //                    viewInfo.Bounds = new Rectangle(0, 0, column.VisibleWidth, dgv.Height);
            //                    using (Graphics graphics = dgv.CreateGraphics())
            //                    using (GraphicsCache cache = new GraphicsCache(graphics))
            //                    {
            //                        viewInfo.CalcViewInfo(graphics);
            //                        var height = ((IHeightAdaptable)viewInfo).CalcHeight(cache, column.VisibleWidth);
            //                        newHeight = Math.Max(height, maxHeight);
            //                        if (newHeight > maxHeight)
            //                        {
            //                            maxHeight = newHeight * count;
            //                        }
            //                    }
            //                }
            //            }
            //        }
            //    }

            //    if (maxHeight > 0 && maxHeight > e.RowHeight )
            //        e.RowHeight = maxHeight;
            //}
        }
        /****************************************************************************************/
        private void chkGroup_CheckedChanged(object sender, EventArgs e)
        {
            if (chkGroup.Checked)
            {
                gridMain.Columns["Location Name"].GroupIndex = 0;

                gridMain.OptionsBehavior.AutoExpandAllGroups = true;
                gridMain.ExpandAllGroups();
                gridMain.OptionsPrint.ExpandAllGroups = true;
                //gridMain.OptionsPrint.PrintGroupFooter = true;
            }
            else
            {
                gridMain.Columns["Location Name"].GroupIndex = -1;
                gridMain.OptionsBehavior.AutoExpandAllGroups = false;
                gridMain.CollapseAllGroups();
                gridMain.OptionsPrint.ExpandAllGroups = false;
                //gridMain.OptionsPrint.PrintGroupFooter = true;
            }
            gridMain.RefreshData();
            dgv.Refresh();
        }
        /****************************************************************************************/
    }
}