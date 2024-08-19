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
using DevExpress.XtraPrinting;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Columns;
using System.Globalization;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class HistoricEmployeeCommissions : DevExpress.XtraEditors.XtraForm
    {
        private string workTable = "";
        private string workColumns = "";
        private bool modified = false;
        private DataTable workDt = null;
        private string workAgent = "";
        private string workAgentCodes = "";
        private DateTime workStartDate = DateTime.Now;
        private DateTime workStopDate = DateTime.Now;
        private string commissionType = "";
        private string workEmpNo = "";
        private string workLastName = "";
        private string workFirstName = "";
        private string workPreferredFirstName = "";
        private string workSuffix = "";

        private DataTable agentDt = null;

        private int SecondSet = 0;
        /****************************************************************************************/
        public HistoricEmployeeCommissions(DataTable dt )
        {
            InitializeComponent();
            workDt = dt;
            workFirstName = "";
            workLastName = "";
            workPreferredFirstName = "";
        }
        /****************************************************************************************/
        public HistoricEmployeeCommissions( string lastName, string firstName, string preferredFirstName = "" )
        {
            InitializeComponent();
            workDt = null;
            workLastName = lastName;
            workFirstName = firstName;
            workPreferredFirstName = preferredFirstName;
        }
        /****************************************************************************************/
        private void HistoricEmployeeCommissions_Load(object sender, EventArgs e)
        {
            barImport.Hide();

            btnSave.Hide();
            modified = false;

            DateTime now = DateTime.Now;
            //            now = now.AddMonths(-1);
            now = new DateTime(now.Year, 1, 1);
            this.dateTimePicker1.Value = now;
            now = DateTime.Now;
            now = now.AddMonths(-1);
            if (now < this.dateTimePicker1.Value)
                now = new DateTime(now.Year, 2, 1);
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            this.dateTimePicker2.Value = new DateTime(now.Year, now.Month, days);


            DataTable dt = LoadFormat();

            LoadMonthColumns(dt);

            //LoadAgentCommission(dt);

            //LoadAgentCustom(workAgent, dt);

            DataTable myDataTable = dt.Clone();

            if (workDt == null)
            {
                DataRow dR = myDataTable.NewRow();
                dR = myDataTable.NewRow();
                dR["title"] = "[" + workLastName + ", " + workFirstName + "] " + workAgentCodes;
                if ( !String.IsNullOrWhiteSpace ( workPreferredFirstName ))
                    dR["title"] = "[" + workLastName + ", " + workFirstName + "], " + workPreferredFirstName + " " + workAgentCodes;

                myDataTable.Rows.Add(dR);
            }

            for (int k = 0; k < dt.Rows.Count; k++)
                myDataTable.ImportRow(dt.Rows[k]);

            dgv.DataSource = myDataTable;

            gridMain.RefreshEditor(true);
        }
        /****************************************************************************************/
        private DataTable LoadFormat ()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("title");

            DataRow dr = null;
            string title = "";
            string cmd = "Select * from `historic_commission_format` order by `order`;";
            DataTable fDt = G1.get_db_data(cmd);
            for ( int i=0; i<fDt.Rows.Count; i++)
            {
                title = fDt.Rows[i]["title"].ObjToString();
                dr = dt.NewRow();
                dr["title"] = title;
                dt.Rows.Add(dr);
            }

            dr = dt.NewRow();
            dt.Rows.Add(dr);

            SecondSet = dt.Rows.Count;

            for (int i = 0; i < fDt.Rows.Count; i++)
            {
                title = fDt.Rows[i]["title"].ObjToString();
                dr = dt.NewRow();
                dr["title"] = title;
                dt.Rows.Add(dr);
            }
            return dt;
        }
        /****************************************************************************************/
        private void LoadMonthColumns ( DataTable dt )
        {
            dt.Columns.Add("month1");
            dt.Columns.Add("month2");
            dt.Columns.Add("month3");
            dt.Columns.Add("month4");
            dt.Columns.Add("month5");
            dt.Columns.Add("month6");

            dt.Rows[0]["month1"] = "January";
            dt.Rows[0]["month2"] = "February";
            dt.Rows[0]["month3"] = "March";
            dt.Rows[0]["month4"] = "April";
            dt.Rows[0]["month5"] = "May";
            dt.Rows[0]["month6"] = "June";

            dt.Rows[SecondSet]["month1"] = "July";
            dt.Rows[SecondSet]["month2"] = "August";
            dt.Rows[SecondSet]["month3"] = "September";
            dt.Rows[SecondSet]["month4"] = "October";
            dt.Rows[SecondSet]["month5"] = "November";
            dt.Rows[SecondSet]["month6"] = "December";
        }
        /****************************************************************************************/
        private void decodeAgentName ( string name, ref string firstName, ref string lastName )
        {
            firstName = "";
            lastName = "";
            string prefix = "";
            string suffix = "";
            string mi = "";

            G1.ParseOutName(name, ref prefix, ref firstName, ref lastName, ref mi, ref suffix);
        }
        /****************************************************************************************/
        private void LoadAgentCommission ( DataTable dt )
        {
            DateTime last = DateTime.Now;
            int days = 0;

            int monthCol = 0;

            string cmd = "";
            string splits = "";
            DataTable dx = null;

            DateTime begin = workStartDate.AddMonths(-1); //ramma zamma
            DateTime end = workStopDate;

            string runNumber = "";

            string firstName = "";
            string lastName = "";

            double splitCommission = 0D;
            double splitBaseCommission = 0D;

            decodeAgentName(workAgent, ref firstName, ref lastName);

            DataRow[] dRows = null;

            for (; ; )
            {
                begin = begin.AddMonths(1);
                days = DateTime.DaysInMonth(begin.Year, begin.Month);
                last = new DateTime(begin.Year, begin.Month, days);
                if (last > end)
                    break;

                cmd = "Select * from `lapse_reinstates` where `startDate` = '" + begin.ToString("yyyy-MM-dd") + "' AND `endDate` = '" + last.ToString("yyyy-MM-dd") + "';";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count <= 0)
                    continue;
                runNumber = dx.Rows[0]["record"].ObjToString();

                cmd = "Select * from `historic_commissions` where `runNumber` = '" + runNumber + "';";
                dx = G1.get_db_data(cmd);

                if (dx.Rows.Count <= 0)
                    continue;

                if (commissionType.ToUpper().IndexOf("STANDARD") >= 0)
                {
                    //dRows = dx.Select("firstName='" + firstName + "' AND lastName='" + lastName + "' AND type='Standard'");
                    //dRows = dx.Select("firstName='" + firstName + "' AND lastName='" + lastName + "' ");
                    dRows = dx.Select("customer='" + workAgent + "' ");

                }
                else
                {
//                    dRows = dx.Select("firstName='" + firstName + "' AND lastName='" + lastName + "' AND type='Goal'");
                    dRows = dx.Select("customer='" + workAgent + "' AND type='Goal'");
                }
                if (dRows.Length <= 0)
                    continue;
                dx = dRows.CopyToDataTable();

                double commission = 0D;
                string commType = "";

                for ( int j=0; j<dx.Rows.Count; j++)
                {
                    commType = dx.Rows[j]["type"].ObjToString().ToUpper();
                    splits = dx.Rows[j]["splits"].ObjToString();

                    splitBaseCommission = dx.Rows[j]["splitBaseCommission"].ObjToDouble();
                    splitCommission = dx.Rows[j]["splitCommission"].ObjToDouble();

                    if (commissionType.ToUpper().IndexOf("STANDARD") >= 0)
                    {
                        if (commType == "GOAL")
                        {
                            if (splitBaseCommission > 0D)
                                commission += splitBaseCommission;
                            else
                                commission += dx.Rows[j]["totalCommission"].ObjToDouble() - dx.Rows[j]["contractCommission"].ObjToDouble();
                        }
                        else if (!String.IsNullOrWhiteSpace(splits))
                            commission += dx.Rows[j]["splitBaseCommission"].ObjToDouble();
                        else
                            commission += dx.Rows[j]["totalCommission"].ObjToDouble();
                    }
                    else
                    {
                        commission += dx.Rows[j]["totalCommission"].ObjToDouble();
                    }
                }

                commission = G1.RoundValue(commission);

                dRows = dt.Select("option='Trust Commission'");
                if ( dRows.Length > 0 )
                {
                    monthCol = begin.Month;
                    if (begin.Month >= 7)
                        monthCol -= 6;
                    if (begin.Month <= 6)
                        dRows[0][monthCol] = G1.ReformatMoney(commission);
                    else
                        dRows[1][monthCol] = G1.ReformatMoney(commission);
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
        }
        /****************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        { // Spy Glass
            G1.ShowHideFindPanel(gridMain);
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
            if (e.Column.FieldName.ToUpper() == "TITLE")
            {
                if (e.RowHandle >= 0)
                {
                    if (e.DisplayText.ToUpper() == "BREAK")
                        e.DisplayText = "";
                }
            }
        }
        /****************************************************************************************/
        private void EditTable_FormClosing(object sender, FormClosingEventArgs e)
        {
            //if (!modified)
            //    return;
            //DialogResult result = MessageBox.Show("***Question***\nData has been modified!\nWould you like to save your changes?", "Options Changed Dialog", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            //if (result == DialogResult.Cancel)
            //{
            //    e.Cancel = true;
            //    return;
            //}
            //modified = false;
            //if (result == DialogResult.No)
            //    return;
            //btnSave_Click(null, null);
        }
        /****************************************************************************************/
        private void repositoryItemComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            modified = true;
        }
        /****************************************************************************************/
        private void btnSave_Click(object sender, EventArgs e)
        {
            //string mod = "";
            //string name = "";
            //string who = "";
            //string option = "";
            //string answer = "";
            //string record = "";
            //DataTable dx = null;
            //DataRow[] dRows = null;
            //string cmd = "";
            //DataTable dt = (DataTable)dgv.DataSource;

            //int startColumn = G1.get_column_number(dt, "name");
            //startColumn = startColumn + 1;

            //this.Cursor = Cursors.WaitCursor;

            //for ( int i=0; i<dt.Rows.Count; i++)
            //{
            //    mod = dt.Rows[i]["mod"].ObjToString();
            //    if (mod != "Y")
            //        continue;
            //    name = dt.Rows[i]["name"].ObjToString();
            //    who = dt.Rows[i]["ma"].ObjToString();
            //    cmd = "Select * from `funcommissiondata` where `name` = '" + name + "' AND `ma` = '" + who + "';";
            //    dx = G1.get_db_data(cmd);

            //    for ( int j=startColumn; j<dt.Columns.Count; j++)
            //    {
            //        option = dt.Columns[j].ColumnName.ObjToString().Trim();
            //        if (option.ToUpper() == "MOD") // Don't Save This as an option
            //            continue;
            //        dRows = optionDt.Select("option='" + option + "'");
            //        if (dRows.Length > 0)
            //            who = dRows[0]["who"].ObjToString();
            //        answer = dt.Rows[i][j].ObjToString();
            //        dRows = dx.Select("option='" + option + "'");
            //        try
            //        {
            //            if (dRows.Length > 0)
            //            {
            //                record = dRows[0]["record"].ObjToString();
            //                G1.update_db_table("funcommissiondata", "record", record, new string[] { "name", name, "ma", who, "option", option, "answer", answer });
            //            }
            //            else
            //            {
            //                record = G1.create_record("funcommissiondata", "option", "-1");
            //                if (G1.BadRecord("funcommissiondata", record))
            //                    continue;
            //                G1.update_db_table("funcommissiondata", "record", record, new string[] { "name", name, "ma", who, "option", option, "answer", answer });
            //            }
            //        }
            //        catch ( Exception ex)
            //        {
            //        }
            //    }
            //}

            //btnSave.Hide();
            //btnSave.Refresh();
            //modified = false;
            //this.Cursor = Cursors.Default;
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

            //            Printer.setupPrinterMargins(50, 50, 80, 50);
            Printer.setupPrinterMargins(5, 5, 80, 50);

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
            string title = this.Text;
            Printer.DrawQuad(5, 8, 6, 4, title, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            Printer.DrawQuad(6, 5, 6, 4, commissionType, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

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
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        /****************************************************************************************/
        private void gridMain_RowCellStyle(object sender, DevExpress.XtraGrid.Views.Grid.RowCellStyleEventArgs e)
        {
            GridView View = sender as GridView;
            if (e.RowHandle >= 0)
            {
                string column = e.Column.FieldName.ToUpper();
                DataTable dt = (DataTable)dgv.DataSource;
                int row = gridMain.GetDataSourceRowIndex(e.RowHandle);

                string data = dt.Rows[row][column].ObjToString();
                if (!String.IsNullOrWhiteSpace(data))
                {
                    if (G1.validate_numeric(data))
                        e.Appearance.TextOptions.HAlignment = HorzAlignment.Far;
                }
            }
        }
        /****************************************************************************************/
        private void LoadAll ()
        {
            btnSave.Hide();
            modified = false;

            this.Text = "All Agents";

            this.Cursor = Cursors.WaitCursor;

            DataTable myDataTable = null;

            for (int i = 0; i < workDt.Rows.Count; i++)
            {
                workAgent = workDt.Rows[i]["name"].ObjToString();
                workAgentCodes = workDt.Rows[i]["agentCodes"].ObjToString();

                DataTable dt = LoadFormat();

                LoadMonthColumns(dt);

                LoadAgentCommission(dt);

                LoadAgentCustom(workAgent, dt);

                if (myDataTable == null)
                    myDataTable = dt.Clone();

                DataRow dR = myDataTable.NewRow();
                if (i > 0)
                {
                    dR["option"] = "BREAK";
                    myDataTable.Rows.Add(dR);
                }
                dR = myDataTable.NewRow();
                dR["option"] = "[" + workAgent + "] " + workAgentCodes;
                myDataTable.Rows.Add(dR);

                for (int k = 0; k < dt.Rows.Count; k++)
                    myDataTable.ImportRow(dt.Rows[k]);

            }

            dgv.DataSource = myDataTable;

            gridMain.RefreshEditor(true);
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void LoadAgentCustom ( string agent, DataTable dt )
        {
            DateTime date = workStartDate;
            date = new DateTime(workStartDate.Year, 1, 1);
            DateTime stopDate = new DateTime(date.Year, 12, 31);
            string title = this.Text;
            try
            {
                string cmd = "Select * from `historic_commission_custom` where `agentName` = '" + agent + "' and `date` >= '" + date.ToString("yyyy-MM-dd") + "' and `date` <= '" + stopDate.ToString("yyyy-MM-dd") + "' AND `commissionType` = '" + commissionType + "';";
                DataTable dx = G1.get_db_data(cmd);
                if (dx.Rows.Count <= 0)
                    return;
                int row = 0;
                string data = "";
                int iMonth = 0;
                for (int i = 0; i < dx.Rows.Count; i++)
                {
                    try
                    {
                        date = dx.Rows[i]["date"].ObjToDateTime();
                        row = dx.Rows[i]["row"].ObjToInt32();
                        if (row > 0)
                            row--;
                        data = dx.Rows[i]["data"].ObjToString();
                        iMonth = date.Month;
                        if (iMonth > 6)
                            iMonth = iMonth - 6;
                        if (date.Month == 1 && date.Day == 1)
                            iMonth = 0;
                        dt.Rows[row][iMonth + 0] = data;
                    }
                    catch ( Exception ex )
                    {
                    }
                }
            }
            catch ( Exception ex )
            {
            }
        }
        /****************************************************************************************/
        private bool pageBreak = false;
        /****************************************************************************************/
        private void gridMain_BeforePrintRow(object sender, DevExpress.XtraGrid.Views.Printing.CancelPrintRowEventArgs e)
        {
            int rowHandle = e.RowHandle;
            if (gridMain.IsDataRow(rowHandle))
            {
                try
                {
                    DataTable dt = (DataTable)dgv.DataSource;
                    int row = gridMain.GetDataSourceRowIndex(rowHandle);

                    string newPage = dt.Rows[row]["title"].ObjToString();
                    if (newPage.ToUpper() == "BREAK")
                    {
                        pageBreak = true;
                        e.Cancel = true;
                    }
                }
                catch (Exception ex)
                {
                }
            }
        }
        /****************************************************************************************/
        private void gridMain_AfterPrintRow(object sender, DevExpress.XtraGrid.Views.Printing.PrintRowEventArgs e)
        {
            if (pageBreak)
                e.PS.InsertPageBreak(e.Y);
            pageBreak = false;
        }
        /****************************************************************************************/
        private string oldWhat = "";
        private void gridMain_ValidatingEditor(object sender, DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventArgs e)
        {
            GridView view = sender as GridView;
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowhandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowhandle);
            string field = gridMain.FocusedColumn.FieldName;
            oldWhat = dt.Rows[row][field].ObjToString();
        }
        /****************************************************************************************/
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string option = dr["title"].ObjToString();
            if (String.IsNullOrWhiteSpace(option))
                return;
            DataTable dt = (DataTable)dgv.DataSource;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            string fieldname = gridMain.FocusedColumn.FieldName;
            if (fieldname.ToUpper() == "TITLE" && oldWhat.ToUpper() == "MONTH")
            {
                MessageBox.Show("***ERROR***\nYou Cannot Change the Month Info Here!", "Change Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                dt.Rows[row][fieldname] = oldWhat;
                dr[fieldname] = oldWhat;
                gridMain.RefreshEditor(true);
                return;
            }
            if ( fieldname == "MONTH")
            {
                MessageBox.Show("***ERROR***\nYou Cannot Change the Month Name Here!", "Change Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                dt.Rows[row][fieldname] = oldWhat;
                dr[fieldname] = oldWhat;
                gridMain.RefreshEditor(true);
                return;
            }
            //if ( !String.IsNullOrWhiteSpace ( oldWhat ))
            //{
            //    if ( G1.validate_numeric ( oldWhat ))
            //    {
            //        double dValue = oldWhat.ObjToDouble();
            //        if (dValue != 0D)
            //        {
            //            if (!G1.ValidateOverridePassword("Enter Password To Override Non-Zero Amount > "))
            //            {
            //                MessageBox.Show("***ERROR***\nYou Cannot Change a Value that already exists!", "Change Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            //                dt.Rows[row][fieldname] = oldWhat;
            //                dr[fieldname] = oldWhat;
            //                gridMain.RefreshEditor(true);
            //                return;
            //            }
            //        }
            //    }
            //}
            string prefix = "";
            string data = dr[fieldname].ObjToString();
            int idx = data.IndexOf("/");
            if ( idx > 0 )
            {
                prefix = data.Substring(0, idx) + "/";
                data = data.Substring(idx);
                data = data.Replace("/", "").Trim();
            }
            if (G1.validate_numeric(data))
            {
                double dValue = data.ObjToDouble();
                data = G1.ReformatMoney(dValue);
                dr[fieldname] = prefix + data;
            }

            data = dr[fieldname].ObjToString();
            data = G1.try_protect_data(data);

            int relativeRow = 0;
            string agent = findAgent(dt, row, ref relativeRow );
            string month = findMonth(dt, row, fieldname);
            int monthNumber = getMonthNumber(month);

            DateTime beginningDate = new DateTime(workStartDate.Year, 1, 31);
            DateTime date = beginningDate;
            if (month.ToUpper() == "MONTH" && fieldname.ToUpper() == "TITLE")
            {
                date = new DateTime(date.Year, 1, 1);
            }
            else
            {
                date = beginningDate.AddMonths(monthNumber - 1);
                int days = DateTime.DaysInMonth(date.Year, date.Month);
                date = new DateTime(date.Year, date.Month, days);
            }

            string cmd = "Select * from `historic_employee_custom` where `agentName` = '" + agent + "' AND `date` = '" + date.ToString("yyyy-MM-dd") + "' AND `row` = '" + relativeRow + "';";
            DataTable ddd = G1.get_db_data(cmd);
            string record = "";
            if (ddd.Rows.Count > 0)
                record = ddd.Rows[0]["record"].ObjToString();
            if (String.IsNullOrWhiteSpace(record) || record == "-1")
                record = G1.create_record("historic_employee_custom", "agentName", "-1");
            if (String.IsNullOrWhiteSpace(data) && record != "-1")
                G1.delete_db_table("historic_employee_custom", "record", record);
            else
                G1.update_db_table("historic_employee_custom", "record", record, new string[] { "date", date.ToString("yyyy-MM-dd"), "agentName", agent, "row", relativeRow.ToString(), "data", data, "commissionType", option });

            TotalUpCommission(dt, row, fieldname);
        }
        /****************************************************************************************/
        private void TotalUpCommission( DataTable dt, int row, string fieldname )
        {
            int relativeRow = 0;
            string agent = findAgent(dt, row, ref relativeRow);
            string month = findMonth(dt, row, fieldname);
            int monthNumber = getMonthNumber(month);

            double total = 0D;
            double dValue = 0D;
            string str = "";
            int idx = 0;
            relativeRow = row - relativeRow;
            for ( int i=relativeRow; i<dt.Rows.Count; i++)
            {
                dValue = 0D;
                str = dt.Rows[i]["title"].ObjToString();
                if ( str.ToUpper() == "TOTAL")
                {
                    str = G1.ReformatMoney(total);
                    dt.Rows[i][fieldname] = str;
                    break;
                }
                str = dt.Rows[i][fieldname].ObjToString();
                idx = str.IndexOf("/");
                if (idx > 0)
                {
                    str = str.Substring(idx);
                    str = str.Replace("/", "").Trim();
                }
                if ( G1.validate_numeric ( str ))
                    dValue = str.ObjToDouble();
                total += dValue;
            }
        }
        /****************************************************************************************/
        private string findAgent(DataTable dt, int row, ref int relativeRow)
        {
            string agent = "";
            string option = "";
            int idx = 0;
            relativeRow = row;
            string title = this.Text.Trim();
            for (int i = row; i >= 0; i--)
            {
                option = dt.Rows[i]["title"].ObjToString();
                if (option.IndexOf("[") >= 0)
                {
                    idx = option.IndexOf("]");
                    agent = option.Substring(1, idx);
                    agent = agent.Replace("]", "");
                    relativeRow = row - i;
                    break;
                }
            }
            return agent;
        }
        /****************************************************************************************/
        private string findMonth ( DataTable dt, int row, string columnName )
        {
            string option = "";
            string month = "";
            for (int i = row; i >= 0; i--)
            {
                option = dt.Rows[i]["title"].ObjToString();
                if (option == "Month ->" )
                {
                    month = dt.Rows[i][columnName].ObjToString();
                    break;
                }
            }
            return month;
        }
        /****************************************************************************************/
        private int getMonthNumber ( string monthName )
        {
            int month = 0;
            try
            {
                month = DateTime.ParseExact(monthName, "MMMM", CultureInfo.InvariantCulture).Month;
            }
            catch ( Exception ex )
            {
            }
            return month;
        }
        /****************************************************************************************/
        private void button1_Click(object sender, EventArgs e)
        {
            if ( workDt == null )
            {
                this.Cursor = Cursors.WaitCursor;
                string what = "";
                DataTable dt = (DataTable)dgv.DataSource;
                for (int i = 2; i < dt.Rows.Count; i++)
                {
                    what = dt.Rows[i]["title"].ObjToString();
                    if (String.IsNullOrWhiteSpace(what))
                        continue;
                    //what = "Standard Commission(5 %)";
                    if (what == "Month ->")
                        break;
                    dt = LoadCommissions(workLastName, workFirstName, what, dt, workPreferredFirstName );
                }
                dgv.DataSource = dt;
                this.Cursor = Cursors.Default;
            }
            else
            {
                string what = "";
                string firstName = "";
                string lastName = "";
                string preferredName = "";

                DataTable mainDt = LoadFormat();
                LoadMonthColumns(mainDt);

                DataTable copyDt = mainDt.Copy();
                DataTable totalDt = mainDt.Clone();

                this.Cursor = Cursors.WaitCursor;

                barImport.Show();
                barImport.Minimum = 0;
                barImport.Maximum = workDt.Rows.Count;
                barImport.Value = 0;

                for ( int j=0; j<workDt.Rows.Count; j++)
                {
                    Application.DoEvents();

                    barImport.Value = j;
                    barImport.Refresh();

                    firstName = workDt.Rows[j]["firstName"].ObjToString();
                    lastName = workDt.Rows[j]["lastName"].ObjToString();
                    preferredName = workDt.Rows[j]["preferredName"].ObjToString();

                    workFirstName = firstName;
                    workLastName = lastName;
                    workPreferredFirstName = preferredName;

                    copyDt = mainDt.Copy();
                    for (int i = 2; i < copyDt.Rows.Count; i++)
                    {
                        what = copyDt.Rows[i]["title"].ObjToString();
                        if (String.IsNullOrWhiteSpace(what))
                            continue;
                        if (what == "Month ->")
                            break;
                        copyDt = LoadCommissions(workLastName, workFirstName, what, copyDt, preferredName );
                    }
                    DataRow dR = totalDt.NewRow();
                    if (j > 0)
                    {
                        dR["title"] = "BREAK";
                        totalDt.Rows.Add(dR);
                    }

                    dR = totalDt.NewRow();
                    dR = totalDt.NewRow();
                    dR["title"] = "[" + workLastName + ", " + workFirstName + "] ";
                    if (!String.IsNullOrWhiteSpace(workPreferredFirstName))
                        dR["title"] = "[" + workLastName + ", " + workFirstName + "], " + workPreferredFirstName;
                    totalDt.Rows.Add(dR);

                    for (int k = 0; k < copyDt.Rows.Count; k++)
                        totalDt.ImportRow(copyDt.Rows[k]);
                }
                dgv.DataSource = totalDt;

                barImport.Value = workDt.Rows.Count;
                barImport.Refresh();

                this.Cursor = Cursors.Default;
            }
        }
        /****************************************************************************************/
        private DataTable LoadCommissions(string lastName, string firstName, string commissionType, DataTable dt, string preferredName = "" )
        {
            DateTime last = DateTime.Now;
            int days = 0;

            int monthCol = 0;

            string cmd = "";
            string splits = "";
            DataTable dx = null;

            //Standard Commission(5 %)
            //Trust Commission(1 %)

            DataRow[] dRows = null;


            string runNumber = "";

            string workAgentCodes = "";
            string workAgent = lastName + ", " + firstName;
            string workPreferred = "";
            if (!String.IsNullOrWhiteSpace(preferredName))
                workPreferred = lastName + ", " + preferredName;
            double totalCommission = 0D;
            double editedCommission = 0D;
            double customCommission = 0D;
            double payroll = 0D;

            double splitCommission = 0D;
            double splitBaseCommission = 0D;

            double reins = 0D;
            double recap = 0D;


            DateTime begin = this.dateTimePicker1.Value;
            DateTime end = this.dateTimePicker2.Value;
            begin = begin.AddMonths(-1);

            DataTable dtTrust = null;

            totalCommission = 0D;
            double contractCommission = 0D;

            for (; ; )
            {
                begin = begin.AddMonths(1);
                days = DateTime.DaysInMonth(begin.Year, begin.Month);
                last = new DateTime(begin.Year, begin.Month, days);
                if (last > end)
                    break;

                cmd = "Select * from `lapse_reinstates` where `startDate` = '" + begin.ToString("yyyy-MM-dd") + "' AND `endDate` = '" + last.ToString("yyyy-MM-dd") + "';";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count <= 0)
                    continue;
                runNumber = dx.Rows[0]["record"].ObjToString();

                cmd = "Select * from `historic_commissions` where `runNumber` = '" + runNumber + "';";
                dx = G1.get_db_data(cmd);

                if (dx.Rows.Count <= 0)
                    continue;
                recap = 0D;
                reins = 0D;
                if (commissionType.ToUpper().IndexOf("1 %") >= 0)
                {

                    cmd = "Select * from `lapsetable` where `runNumber` = '" + runNumber + "';";
                    dtTrust = G1.get_db_data(cmd);
                    recap = 0D;
                    dRows = dtTrust.Select("agentName='" + firstName + " " + lastName + "'");
                    if (dRows.Length <= 0 && !String.IsNullOrWhiteSpace(workPreferred))
                        dRows = dtTrust.Select("agentName='" + preferredName + " " + lastName + "'");
                    if (dRows.Length > 0)
                    {
                        for (int j = 0; j < dRows.Length; j++)
                            recap += dRows[j]["recap"].ObjToDouble();
                    }
                    cmd = "Select * from `reinstatetable` where `runNumber` = '" + runNumber + "';";
                    dtTrust = G1.get_db_data(cmd);
                    reins = 0D;
                    dRows = dtTrust.Select("agentName='" + firstName + " " + lastName + "'");
                    if (dRows.Length <= 0 && !String.IsNullOrWhiteSpace(workPreferred))
                        dRows = dtTrust.Select("agentName='" + preferredName + " " + lastName + "'");

                    if (dRows.Length > 0)
                    {
                        for (int j = 0; j < dRows.Length; j++)
                            reins += dRows[j]["reins"].ObjToDouble();
                    }
                }

                dRows = null;
                if (commissionType.ToUpper().IndexOf("STANDARD") >= 0)
                {
                    //dRows = dx.Select("firstName='" + firstName + "' AND lastName='" + lastName + "' AND type='Standard'");
                    dRows = dx.Select("firstName='" + firstName + "' AND lastName='" + lastName + "' ");
                    if (dRows.Length <= 0 && !String.IsNullOrWhiteSpace(workPreferred))
                        dRows = dx.Select("firstName='" + preferredName + "' AND lastName='" + lastName + "' ");
                }
                else if (commissionType.ToUpper().IndexOf("GOAL") >= 0)
                {
                    dRows = dx.Select("firstName='" + firstName + "' AND lastName='" + lastName + "' AND type='Goal'");
                    if (dRows.Length <= 0 && !String.IsNullOrWhiteSpace(workPreferred))
                        dRows = dx.Select("firstName='" + preferredName + "' AND lastName='" + lastName + "' ");
                }

                if (dRows == null)
                    dx.Rows.Clear();
                else if (dRows.Length > 0)
                    dx = dRows.CopyToDataTable();
                else
                    dx.Rows.Clear();

                double commission = 0D;
                string commType = "";

                if (commissionType.ToUpper().IndexOf("1 %") >= 0)
                {
                    double splitGoalCommission = 0D;
                    double totalGoalCommission = 0D;
                    double totalPastFailures = 0D;
                    double totalContract = 0D;
                    for (int j = 0; j < dx.Rows.Count; j++ )
                    {
                        splitGoalCommission += dx.Rows[j]["splitGoalCommission"].ObjToDouble();
                        totalGoalCommission += dx.Rows[j]["goalCommission"].ObjToDouble();
                        totalPastFailures += dx.Rows[j]["pastFailures"].ObjToDouble();
                    }
                    totalContract = splitGoalCommission + totalGoalCommission - totalPastFailures + reins - recap;
                    if (totalContract < 0D)
                        totalContract = 0D;
                    commission = totalContract;
                }

                for (int j = 0; j < dx.Rows.Count; j++)
                {
                    commType = dx.Rows[j]["type"].ObjToString().ToUpper();
                    splits = dx.Rows[j]["splits"].ObjToString();

                    splitBaseCommission = dx.Rows[j]["splitBaseCommission"].ObjToDouble();
                    splitCommission = dx.Rows[j]["splitCommission"].ObjToDouble();

                    if (commissionType.ToUpper().IndexOf("STANDARD") >= 0)
                    {
                        if (commType == "GOAL")
                        {
                            if (splitBaseCommission > 0D)
                                commission = splitBaseCommission;
                            else
                            {
                                //commission = dx.Rows[j]["totalCommission"].ObjToDouble() - dx.Rows[j]["contractCommission"].ObjToDouble();
                                commission += dx.Rows[j]["commission"].ObjToDouble();
                            }
                        }
                        else if (!String.IsNullOrWhiteSpace(splits))
                            commission += dx.Rows[j]["splitBaseCommission"].ObjToDouble();
                        else
                            commission += dx.Rows[j]["totalCommission"].ObjToDouble();
                    }
                    else
                    {
                        contractCommission += dx.Rows[j]["totalCommission"].ObjToDouble();
                        //if (commType == "GOAL")
                        //{
                        //    //commission += dx.Rows[j]["contractCommission"].ObjToDouble();
                        //    if (splitBaseCommission > 0D)
                        //        commission += splitBaseCommission;
                        //    else
                        //        commission += dx.Rows[j]["goalCommission"].ObjToDouble();
                        //}
                        //else if (!String.IsNullOrWhiteSpace(splits))
                        //    commission += dx.Rows[j]["splitBaseCommission"].ObjToDouble();
                        //else
                        //    commission += dx.Rows[j]["totalCommission"].ObjToDouble();
                    }
                }

                //if (commissionType.ToUpper().IndexOf("1 %") >= 0)
                //{
                //    commission = commission + reins - recap;
                //}
                payroll = 0D;
                if (commissionType.ToUpper().IndexOf("PAYROLL") >= 0)
                {
                    if (LoadEmployeePayroll(firstName, lastName, begin, last, ref payroll))
                        commission = payroll;
                }


                editedCommission = 0D;
                if (LoadAgentCustom(workAgent, last, commissionType, ref editedCommission))
                    commission = editedCommission;

                customCommission = 0D;
                string prefix = "";
                if (LoadEmployeeCustom(workAgent, last, commissionType, ref customCommission, ref prefix))
                {
                    if (commissionType.IndexOf("5 %") > 0 || commissionType.IndexOf("1 %") > 0)
                    {
                        commission = customCommission;
                        prefix = "M/ ";
                    }
                    else
                        commission += customCommission;
                }
                if ( !String.IsNullOrWhiteSpace ( prefix))
                {
                }

                commission = G1.RoundValue(commission);

                if ( commissionType.Trim().ToUpper() == "TOTAL")
                {
                    int beginRow = 2;
                    monthCol = begin.Month;
                    if (begin.Month >= 7)
                    {
                        monthCol -= 6;
                        beginRow = FindRow(dt, 2, "Month ->");
                    }
                    string str = "";
                    commission = 0D;
                    double dValue = 0D;
                    int idx = 0;

                    for ( int j=beginRow; j<dt.Rows.Count; j++)
                    {
                        str = dt.Rows[j]["title"].ObjToString();
                        if (str.Trim().ToUpper() == "TOTAL")
                            break;
                        str = dt.Rows[j][monthCol].ObjToString();
                        idx = str.IndexOf("/");
                        if ( idx > 0 )
                        {
                            str = str.Substring(idx);
                            str = str.Replace("/", "").Trim();
                        }
                        if ( G1.validate_numeric ( str ))
                        {
                            dValue = str.ObjToDouble();
                            commission += dValue;
                        }
                    }
                }

                dRows = dt.Select("title='" + commissionType + "'");
                if (dRows.Length > 0)
                {
                    int beginRow = 2;
                    monthCol = begin.Month;
                    if (begin.Month >= 7)
                        monthCol -= 6;
                    if (begin.Month <= 6)
                        dRows[0][monthCol] = prefix + G1.ReformatMoney(commission);
                    else
                        dRows[1][monthCol] = prefix + G1.ReformatMoney(commission);
                }

                totalCommission += commission;

            }
            return dt;
        }
        /****************************************************************************************/
        private int FindRow ( DataTable dt, int startRow, string what )
        {
            int row = -1;
            string str = "";
            for ( int i=startRow; i<dt.Rows.Count; i++)
            {
                str = dt.Rows[i]["title"].ObjToString();
                if ( str == what )
                {
                    row = i;
                    break;
                }
            }
            return row;
        }
        /****************************************************************************************/
        private bool LoadAgentCustom(string agent, DateTime date, string commissionType, ref double editedCommission)
        {
            bool rtn = false;
            editedCommission = 0D;
            try
            {
                string cmd = "Select * from `historic_commission_custom` where `agentName` = '" + agent + "' and `date` = '" + date.ToString("yyyy-MM-dd") + "' AND `commissionType` = '" + commissionType + "' AND `row` = '2';";
                DataTable dx = G1.get_db_data(cmd);
                if (dx.Rows.Count <= 0)
                    return rtn;
                string data = dx.Rows[0]["data"].ObjToString();
                if (G1.validate_numeric(data))
                {
                    editedCommission = data.ObjToDouble();
                    rtn = true;
                }
            }
            catch (Exception ex)
            {
            }
            return rtn;
        }
        /****************************************************************************************/
        private bool LoadEmployeeCustom(string agent, DateTime date, string commissionType, ref double editedCommission, ref string prefix )
        {
            bool rtn = false;
            editedCommission = 0D;
            prefix = "";
            int idx = 0;
            try
            {
                string cmd = "Select * from `historic_employee_custom` where `agentName` = '" + agent + "' and `date` = '" + date.ToString("yyyy-MM-dd") + "' AND `commissionType` = '" + commissionType + "';";
                DataTable dx = G1.get_db_data(cmd);
                if (dx.Rows.Count <= 0)
                    return rtn;
                string data = dx.Rows[0]["data"].ObjToString();
                idx = data.IndexOf("/");
                if ( idx > 0 )
                {
                    prefix = data.Substring(0, idx) + "/";
                    data = data.Substring(idx);
                    data = data.Replace("/", "").Trim();
                }
                if (G1.validate_numeric(data))
                {
                    editedCommission = data.ObjToDouble();
                    rtn = true;
                }
            }
            catch (Exception ex)
            {
            }
            return rtn;
        }
        /***********************************************************************************************/
        private void DetermineRealDateRange(DateTime date1, DateTime date2, ref DateTime start, ref DateTime stop)
        {
            start = new DateTime(2022, 12, 23);
            stop = start;
            bool gotStart = false;
            bool gotStop = false;
            for (; ; )
            {
                if (!gotStart)
                {
                    if (start.AddDays(13) >= date1)
                    {
                        //start = date1;
                        gotStart = true;
                    }
                    else
                        start = start.AddDays(14);
                }
                if (!gotStop)
                {
                    if (stop.AddDays(13) >= date2)
                    {
                        stop = stop.AddDays(14);
                        gotStop = true;
                        break;
                    }
                    else
                        stop = stop.AddDays(14);
                }
            }
        }
        /****************************************************************************************/
        private bool LoadEmployeePayroll(string firstName, string lastName, DateTime beginDate, DateTime lastDate, ref double payroll )
        {
            bool rv = false;
            payroll = 0D;

            DateTime date1 = beginDate;
            string startDate = date1.ToString("yyyyMMdd");
            DateTime date2 = lastDate;
            string endDate = date2.ToString("yyyyMMdd");

            DateTime originalEndDate = new DateTime(date2.Year, date2.Month, date2.Day);

            DataTable dx = null;
            string cmd = "";

            DateTime realStartDate = DateTime.Now;
            DateTime realStopDate = DateTime.Now;

            DetermineRealDateRange(date1, date2, ref realStartDate, ref realStopDate);
            if (realStartDate != date1)
            {
                date1 = realStartDate;
                startDate = date1.ToString("yyyyMMdd");
            }
            if (realStopDate != date2)
            {
                //date2 = date1.AddDays(14);
                date2 = realStopDate;
                endDate = date2.ToString("yyyyMMdd");
            }

            cmd = "Select DISTINCT * from `users` j RIGHT JOIN `tc_pay` p ON j.`username` = p.`username` LEFT JOIN `tc_er` r ON p.`username` = r.`username` WHERE `startDate` >= '" + startDate + "' AND `endDate` <= '" + endDate + "' AND j.`lastName` = '" + lastName + "' AND j.`firstName` = '" + firstName + "';";
            dx = G1.get_db_data(cmd);

            double daysInPeriod = 0D;
            double payDays = 0D;
            double payPerDay = 0D;
            TimeSpan ts;
            double gross = 0D;
            double totalPay = 0D;

            for ( int i=0; i<dx.Rows.Count; i++)
            {
                gross = dx.Rows[0]["totalPay"].ObjToDouble();
                if (gross <= 0D)
                    continue;
                if ( dx.Rows[i]["startDate"].ObjToDateTime() < beginDate )
                {
                    ts = dx.Rows[i]["endDate"].ObjToDateTime() - dx.Rows[i]["startDate"].ObjToDateTime();
                    daysInPeriod = ts.TotalDays;
                    payPerDay = gross / (double)daysInPeriod;
                    ts = beginDate - dx.Rows[i]["startDate"].ObjToDateTime();
                    daysInPeriod = ts.TotalDays;
                    totalPay = payPerDay * daysInPeriod;
                    payroll += gross - totalPay;
                }
                else if ( dx.Rows[i]["startDate"].ObjToDateTime() >= beginDate && dx.Rows[i]["endDate"].ObjToDateTime() <= lastDate )
                {
                    payroll += gross;
                }
                else if ( dx.Rows[i]["startDate"].ObjToDateTime() <= lastDate && dx.Rows[i]["endDate"].ObjToDateTime() > lastDate )
                {
                    ts = dx.Rows[i]["endDate"].ObjToDateTime() - dx.Rows[i]["startDate"].ObjToDateTime();
                    daysInPeriod = ts.TotalDays;
                    payPerDay = gross / (double)daysInPeriod;
                    ts = dx.Rows[i]["endDate"].ObjToDateTime() - lastDate;
                    daysInPeriod = ts.TotalDays;
                    totalPay = payPerDay * daysInPeriod;
                    payroll += gross - totalPay;
                }
            }

            rv = true;

            return rv;
        }
        /****************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();
            string field = gridMain.FocusedColumn.FieldName.ToUpper();
            if (field == "TITLE")
                return;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetFocusedDataSourceRowIndex();
            int col = gridMain.FocusedColumn.VisibleIndex;
            string data = dt.Rows[row][field].ObjToString();
            string detail = dt.Rows[row]["title"].ObjToString();
            string str = "";
            string agent = "";
            int agentRow = -1;
            int monthCount = 0;
            string month = "";
            for (int i = row; i >= 0; i--)
            {
                str = dt.Rows[i]["title"].ObjToString();
                if (str.IndexOf("[") >= 0)
                {
                    str = str.Replace("[", "");
                    str = str.Substring(0, str.IndexOf("]")).Trim();
                    str = str.Replace("]", "");
                    agent = str;
                    agentRow = i;
                    break;
                }
                if (str.ToUpper() == "MONTH ->")
                {
                    monthCount++;
                    month = dt.Rows[i][col].ObjToString();
                }
            }
            //MessageBox.Show("Agent=" + agent + " Month=" + month);
            if (String.IsNullOrWhiteSpace(agent))
                return;

            string[] Lines = agent.Split(',');
            string firstName = "";
            string lastName = "";
            if (Lines.Length > 0)
            {
                lastName = Lines[0].Trim();
                if (Lines.Length > 1)
                    firstName = Lines[1].Trim();
            }

            string preferredName = "";
            string cmd = "Select * from `users` u JOIN `tc_er` t ON u.`userName` = t.`username` WHERE `firstName` = '" + firstName + "' AND `lastName` = '" + lastName + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                preferredName = dx.Rows[0]["preferredName"].ObjToString();
            }

            int otherRow = row - agentRow;

            int monthIndex = G1.ConvertMonthToIndex(month);

            DateTime startDate = this.dateTimePicker1.Value;
            int startMonth = startDate.Month.ObjToInt32();
            monthIndex = monthIndex - startMonth + 1;
            if (monthCount > 1)
                monthIndex += 6;
            DateTime commissionDate = startDate.AddMonths(monthIndex - 1);
            int days = DateTime.DaysInMonth(commissionDate.Year, commissionDate.Month);
            commissionDate = new DateTime(commissionDate.Year, commissionDate.Month, days);
            DateTime firstDate = new DateTime(commissionDate.Year, commissionDate.Month, 1);

            //MessageBox.Show("FirstDate=" + firstDate.ToString("MM/dd/yyyy") + " EndDate=" + commissionDate.ToString("MM/dd/yyyy"));

            cmd = "Select * from `lapse_reinstates` where `startDate` = '" + firstDate.ToString("yyyy-MM-dd") + "' AND `endDate` = '" + commissionDate.ToString("yyyy-MM-dd") + "';";
            dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
            {
                MessageBox.Show("PROBLEM FINDING lapse_reinstate date");
                return;
            }
            string runNumber = dx.Rows[0]["record"].ObjToString();

            string cmbShow = "1%";
            if (detail.ToUpper().IndexOf("STANDARD") >= 0 )
                cmbShow = "5%";

            cmbShow = "All";

            cmd = "Select * from `trustdetail` WHERE `runNumber` = '" + runNumber + "';";
            DataTable dtTrust = G1.get_db_data(cmd);

            int iRun = runNumber.ObjToInt32();
            iRun = iRun - 1;
            cmd = "Select * from `lapsetable` WHERE `runNumber` = '" + iRun.ToString() + "';";
            DataTable dt8 = G1.get_db_data(cmd);

            cmd = "Select * from `reinstatetable` WHERE `runNumber` = '" + runNumber + "';";
            DataTable dt9 = G1.get_db_data(cmd);

            cmd = "Select * from `historic_commissions` WHERE `runNumber` = '" + runNumber + "';";
            DataTable dt10 = G1.get_db_data(cmd);

            string agentName = "";

            cmd = "Select * from `agents` WHERE `firstName` = '" + firstName + "' AND `lastName` = '" + lastName + "';";
            DataTable agentDt = G1.get_db_data(cmd);
            if (agentDt.Rows.Count <= 0)
            {
                if (!String.IsNullOrWhiteSpace(preferredName))
                {
                    cmd = "Select * from `agents` WHERE `firstName` = '" + preferredName + "' AND `lastName` = '" + lastName + "';";
                    agentDt = G1.get_db_data(cmd);
                    if ( agentDt.Rows.Count <= 0 )
                    {
                        MessageBox.Show("PROBLEM FINDING Agent " + firstName + " " + lastName + " " + preferredName);
                        return;
                    }
                    agentName = preferredName + " " + lastName;
                }
            }
            else
                agentName = firstName + " " + lastName;

            string agentNumber = agentDt.Rows[0]["agentCode"].ObjToString();

            this.Cursor = Cursors.WaitCursor;
            CommissionDetail commForm = new CommissionDetail(firstDate, commissionDate, agentNumber, agentName, dtTrust, dt8, dt9, dt10, false, cmbShow);
            commForm.TopMost = true;
            commForm.Show();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void clearValueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!G1.isAdmin() && !G1.isHR())
                return;

            DataRow dr = gridMain.GetFocusedDataRow();
            string option = dr["title"].ObjToString();
            if (String.IsNullOrWhiteSpace(option))
                return;
            DataTable dt = (DataTable)dgv.DataSource;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            string fieldname = gridMain.FocusedColumn.FieldName;
            if (fieldname.ToUpper() == "TITLE" && oldWhat.ToUpper() == "MONTH")
            {
                MessageBox.Show("***ERROR***\nYou Cannot Change the Month Info Here!", "Change Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                dt.Rows[row][fieldname] = oldWhat;
                dr[fieldname] = oldWhat;
                gridMain.RefreshEditor(true);
                return;
            }
            if (fieldname == "MONTH")
            {
                MessageBox.Show("***ERROR***\nYou Cannot Change the Month Name Here!", "Change Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                dt.Rows[row][fieldname] = oldWhat;
                dr[fieldname] = oldWhat;
                gridMain.RefreshEditor(true);
                return;
            }
            //if ( !String.IsNullOrWhiteSpace ( oldWhat ))
            //{
            //    if ( G1.validate_numeric ( oldWhat ))
            //    {
            //        double dValue = oldWhat.ObjToDouble();
            //        if (dValue != 0D)
            //        {
            //            if (!G1.ValidateOverridePassword("Enter Password To Override Non-Zero Amount > "))
            //            {
            //                MessageBox.Show("***ERROR***\nYou Cannot Change a Value that already exists!", "Change Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            //                dt.Rows[row][fieldname] = oldWhat;
            //                dr[fieldname] = oldWhat;
            //                gridMain.RefreshEditor(true);
            //                return;
            //            }
            //        }
            //    }
            //}
            string data = "0.00";
            if (G1.validate_numeric(data))
            {
                double dValue = data.ObjToDouble();
                data = G1.ReformatMoney(dValue);
                dr[fieldname] = data;
            }

            data = G1.try_protect_data(data);

            int relativeRow = 0;
            string agent = findAgent(dt, row, ref relativeRow);
            string month = findMonth(dt, row, fieldname);
            int monthNumber = getMonthNumber(month);
            string title = dt.Rows[row]["title"].ObjToString();

            DateTime beginningDate = new DateTime(workStartDate.Year, 1, 31);
            DateTime date = beginningDate;
            if (month.ToUpper() == "MONTH" && fieldname.ToUpper() == "TITLE")
            {
                date = new DateTime(date.Year, 1, 1);
            }
            else
            {
                date = beginningDate.AddMonths(monthNumber - 1);
                int days = DateTime.DaysInMonth(date.Year, date.Month);
                date = new DateTime(date.Year, date.Month, days);
            }

            string cmd = "Select * from `historic_employee_custom` where `agentName` = '" + agent + "' AND `date` = '" + date.ToString("yyyy-MM-dd") + "' AND `commissionType` = '" + title + "';";
            DataTable ddd = G1.get_db_data(cmd);
            string record = "";
            if (ddd.Rows.Count > 0)
            {
                record = ddd.Rows[0]["record"].ObjToString();
                G1.delete_db_table("historic_employee_custom", "record", record);
            }

            TotalUpCommission(dt, row, fieldname);
        }
        /****************************************************************************************/
    }
}