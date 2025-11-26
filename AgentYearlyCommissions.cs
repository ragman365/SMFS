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
    public partial class AgentYearlyCommissions : DevExpress.XtraEditors.XtraForm
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

        private int SecondSet = 0;
        /****************************************************************************************/
        public AgentYearlyCommissions(DataTable dt, string type, string agent, string agentCodes, DateTime start, DateTime stop )
        {
            InitializeComponent();
            workDt = dt;
            commissionType = type;
            workAgent = agent;
            workAgentCodes = agentCodes;
            workStartDate = start;
            workStopDate = stop;
        }
        /****************************************************************************************/
        private void AgentYearlyCommissions_Load(object sender, EventArgs e)
        {
            btnSave.Hide();
            modified = false;

            if ( String.IsNullOrWhiteSpace ( workAgent ) && String.IsNullOrWhiteSpace ( workAgentCodes ))
            {
                LoadAll();
                return;
            }

            this.Text = "Agent Name and Number [" + workAgent + "] " + workAgentCodes;

            DataTable dt = LoadFormat();

            LoadMonthColumns(dt);

            LoadAgentCommission(dt);

            LoadAgentCustom(workAgent, dt);

            DataTable myDataTable = dt.Clone();

            DataRow dR = myDataTable.NewRow();
            dR = myDataTable.NewRow();
            dR["option"] = "[" + workAgent + "] " + workAgentCodes;
            myDataTable.Rows.Add(dR);

            for (int k = 0; k < dt.Rows.Count; k++)
                myDataTable.ImportRow(dt.Rows[k]);

            dgv.DataSource = myDataTable;

            gridMain.RefreshEditor(true);
        }
        /****************************************************************************************/
        private DataTable LoadFormat ()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("option");

            DataRow dr = null;
            string option = "";
            string cmd = "Select * from `agentcommissionformat`;";
            DataTable fDt = G1.get_db_data(cmd);
            for ( int i=0; i<fDt.Rows.Count; i++)
            {
                option = fDt.Rows[i]["option"].ObjToString();
                if (option.ToUpper() == "EMPTY")
                    option = "";
                dr = dt.NewRow();
                dr["option"] = option;
                dt.Rows.Add(dr);
            }

            dr = dt.NewRow();
            dt.Rows.Add(dr);

            SecondSet = dt.Rows.Count;

            for (int i = 0; i < fDt.Rows.Count; i++)
            {
                option = fDt.Rows[i]["option"].ObjToString();
                if (option.ToUpper() == "EMPTY")
                    option = "";
                dr = dt.NewRow();
                dr["option"] = option;
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
                    //dRows = dx.Select("customer='" + workAgent + "' AND type='Goal'");
                    dRows = dx.Select("customer='" + workAgent + "' ");
                }
                if (dRows.Length <= 0)
                    continue;
                dx = dRows.CopyToDataTable();

                double commission = 0D;
                string commType = "";
                double contractCommission = 0D;
                double total_Commission = 0D;
                double splitGoalCommission = 0D;
                double totalGoalCommission = 0D;
                double totalPastFailures = 0D;
                double totalReins = 0D;
                double totalRecap = 0D;
                double goalCommission = 0D;

                for ( int j=0; j<dx.Rows.Count; j++)
                {
                    commType = dx.Rows[j]["type"].ObjToString().ToUpper();
                    splits = dx.Rows[j]["splits"].ObjToString();

                    splitBaseCommission = dx.Rows[j]["splitBaseCommission"].ObjToDouble();
                    splitCommission = dx.Rows[j]["splitCommission"].ObjToDouble();
                    contractCommission = dx.Rows[j]["contractCommission"].ObjToDouble();
                    total_Commission = dx.Rows[j]["totalCommission"].ObjToDouble();
                    splitGoalCommission = dx.Rows[j]["splitGoalCommission"].ObjToDouble();
                    totalRecap = dx.Rows[j]["Recap"].ObjToDouble();
                    totalReins = dx.Rows[j]["Reins"].ObjToDouble();
                    totalPastFailures = dx.Rows[j]["pastFailures"].ObjToDouble();
                    goalCommission = dx.Rows[j]["goalCommission"].ObjToDouble();

                    if (commissionType.ToUpper().IndexOf("STANDARD") >= 0)
                    {
                        if (commType == "GOAL")
                        {
                            if (splitBaseCommission > 0D)
                                commission += splitBaseCommission;
                            else
                            {
                                //commission += dx.Rows[j]["totalCommission"].ObjToDouble() - dx.Rows[j]["contractCommission"].ObjToDouble();
                                commission += dx.Rows[j]["TotalCommission"].ObjToDouble() - contractCommission;
                            }
                        }
                        else if (!String.IsNullOrWhiteSpace(splits))
                            commission += dx.Rows[j]["splitBaseCommission"].ObjToDouble();
                        else
                        {
                            commission += dx.Rows[j]["TotalCommission"].ObjToDouble() - contractCommission;
                            //commission += dx.Rows[j]["totalCommission"].ObjToDouble();
                            //commission += dx.Rows[j]["commission"].ObjToDouble();
                        }
                    }
                    else
                    {
                        //commission += dx.Rows[j]["totalCommission"].ObjToDouble();
                        //commission += goalCommission + splitGoalCommission - totalPastFailures + totalReins - totalRecap;
                        commission += goalCommission - totalRecap + totalReins - totalPastFailures;
                    }

                    //if (commissionType.ToUpper().IndexOf("STANDARD") >= 0)
                    //{
                    //    if (commType == "GOAL")
                    //    {
                    //        if (splitBaseCommission > 0D)
                    //            commission += splitBaseCommission;
                    //        else
                    //            commission += dx.Rows[j]["totalCommission"].ObjToDouble() - dx.Rows[j]["contractCommission"].ObjToDouble();
                    //    }
                    //    else if (!String.IsNullOrWhiteSpace(splits))
                    //        commission += dx.Rows[j]["splitBaseCommission"].ObjToDouble();
                    //    else
                    //        commission += dx.Rows[j]["totalCommission"].ObjToDouble();
                    //}
                    //else
                    //{
                    //    commission += dx.Rows[j]["totalCommission"].ObjToDouble();
                    //}
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
            if (e.Column.FieldName.ToUpper() == "OPTION")
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
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            //DataTable dt = (DataTable)dgv.DataSource;
            //DataRow dr = gridMain.GetFocusedDataRow();
            //string manager = dr["name"].ObjToString();
            //if (String.IsNullOrWhiteSpace(manager))
            //    return;
            //string ma = dr["ma"].ObjToString();
            //this.Cursor = Cursors.WaitCursor;
            //FunManager funForm = new FunManager(manager, ma);
            //funForm.Show();
            //this.Cursor = Cursors.Default;
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

                string comment = dt.Rows[row]["option"].ObjToString();
                if (comment.Trim().ToUpper() == "TRUST COMMISSION")
                {
                    if (column.ToUpper().IndexOf("MONTH") >= 0)
                        e.Appearance.TextOptions.HAlignment = HorzAlignment.Far;
                }
                else if (comment.Trim().ToUpper() == "MONTH")
                {
                    if (column.ToUpper().IndexOf("MONTH") >= 0)
                        e.Appearance.TextOptions.HAlignment = HorzAlignment.Center;
                }
                else
                {
                    string data = dt.Rows[row][column].ObjToString();
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

                    string newPage = dt.Rows[row]["option"].ObjToString();
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
            string option = dr["option"].ObjToString();
            DataTable dt = (DataTable)dgv.DataSource;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            string fieldname = gridMain.FocusedColumn.FieldName;
            if (fieldname.ToUpper() == "OPTION" && oldWhat.ToUpper() == "MONTH")
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
            string data = dr[fieldname].ObjToString();
            if (G1.validate_numeric(data))
            {
                double dValue = data.ObjToDouble();
                data = G1.ReformatMoney(dValue);
                dr[fieldname] = data;
            }

            data = G1.try_protect_data(data);

            int relativeRow = 0;
            string agent = findAgent(dt, row, ref relativeRow );
            string month = findMonth(dt, row, fieldname);
            int monthNumber = getMonthNumber(month);

            DateTime beginningDate = new DateTime(workStartDate.Year, 1, 31);
            DateTime date = beginningDate;
            if (month.ToUpper() == "MONTH" && fieldname.ToUpper() == "OPTION")
            {
                date = new DateTime(date.Year, 1, 1);
            }
            else
            {
                date = beginningDate.AddMonths(monthNumber - 1);
                int days = DateTime.DaysInMonth(date.Year, date.Month);
                date = new DateTime(date.Year, date.Month, days);
            }

            string cmd = "Select * from `historic_commission_custom` where `agentName` = '" + agent + "' AND `date` = '" + date.ToString("yyyy-MM-dd") + "' AND `row` = '" + relativeRow + "';";
            DataTable ddd = G1.get_db_data(cmd);
            string record = "";
            if (ddd.Rows.Count > 0)
                record = ddd.Rows[0]["record"].ObjToString();
            if (String.IsNullOrWhiteSpace(record) || record == "-1")
                record = G1.create_record("historic_commission_custom", "agentName", "-1");
            if (String.IsNullOrWhiteSpace(data) && record != "-1")
                G1.delete_db_table("historic_commission_custom", "record", record);
            else
                G1.update_db_table("historic_commission_custom", "record", record, new string[] { "date", date.ToString("yyyy-MM-dd"), "agentName", agent, "row", relativeRow.ToString(), "data", data, "commissionType", commissionType });
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
                option = dt.Rows[i]["option"].ObjToString();
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
                option = dt.Rows[i]["option"].ObjToString();
                if (option == "Month" )
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
    }
}