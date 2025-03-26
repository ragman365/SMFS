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
using DevExpress.XtraPrinting;
using DevExpress.Utils;
using System.IO;
using GeneralLib;
using DevExpress.XtraGrid.Views.BandedGrid;
using DevExpress.XtraGrid.Views.Grid;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class CompareCommissions : DevExpress.XtraEditors.XtraForm
    {
        private DataTable workDt = null;
        private DataTable allAgentsDt = null;
        /****************************************************************************************/
        public CompareCommissions( DataTable dt )
        {
            InitializeComponent();
            AddSummaryColumn("commission", gridMain);
            AddSummaryColumn("foundCommission", gridMain);

            workDt = dt.Copy();
        }
        /****************************************************************************************/
        private void AddSummaryColumn(string columnName, DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gMain = null)
        {
            if (gMain == null)
                gMain = gridMain;
            //            gMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            gMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            //            gMain.Columns[columnName].SummaryItem.DisplayFormat = "${0:0,0.00}";
            gMain.Columns[columnName].SummaryItem.DisplayFormat = "{0:N2}";
        }
        /****************************************************************************************/
        private void CompareCommissions_Load(object sender, EventArgs e)
        {
            chkFilter.Hide();
            chkFoundComm.Hide();
            chkRed.Hide();
            if ( G1.get_column_number ( workDt,"found") < 0 )
                workDt.Columns.Add("found");
            if (G1.get_column_number(workDt, "foundCommission") < 0)
                workDt.Columns.Add("foundCommission");
            if (G1.get_column_number(workDt, "foundPayment") < 0)
                workDt.Columns.Add("foundPayment");
            if (G1.get_column_number(workDt, "date") < 0)
                workDt.Columns.Add("date");
            //if (G1.get_column_number(workDt, "foundPayment") < 0)
            //    workDt.Columns.Add("foundPayment");
            //if (G1.get_column_number(workDt, "foundDebit") < 0)
            //    workDt.Columns.Add("foundDebit");
            //if (G1.get_column_number(workDt, "foundCredit") < 0)
            //    workDt.Columns.Add("foundCredit");
            //if (G1.get_column_number(workDt, "foundTrust85") < 0)
            //    workDt.Columns.Add("foundTrust85");
            //if (G1.get_column_number(workDt, "foundTrust100") < 0)
            //    workDt.Columns.Add("foundTrust100");

            DateTime date = DateTime.Now;
            for ( int i=0; i<workDt.Rows.Count; i++)
            {
                date = workDt.Rows[i]["payDate8"].ObjToDateTime();
                workDt.Rows[i]["date"] = date.ToString("MM/dd/yyyy");
            }
            int count = workDt.Rows.Count;
            G1.NumberDataTable(workDt);
            dgv.DataSource = workDt;
            loadGroupCombo(cmbSelectColumns, "CommCompare", "Primary");
        }
        /***********************************************************************************************/
        private void loadGroupCombo(System.Windows.Forms.ComboBox cmb, string key, string module)
        {
            cmb.Items.Clear();
            string cmd = "Select * from procfiles where ProcType = '" + key + "' AND `module` = '" + module + "' group by name;";
            DataTable dt = G1.get_db_data(cmd);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string name = dt.Rows[i]["Name"].ToString();
                cmb.Items.Add(name);
            }
        }
        /****************************************************************************************/
        private void btnCompare_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            OpenFileDialog openFileDialog1 = new OpenFileDialog
            {
                InitialDirectory = @"C:\users\robby\downloads",
                Title = "Browse TXT Files",

                CheckFileExists = true,
                CheckPathExists = true,

                DefaultExt = "txt",
                Filter = "txt files (*.txt)|*.txt",
                FilterIndex = 2,
                RestoreDirectory = true,

                ReadOnlyChecked = true,
                ShowReadOnly = true
            };

            if (openFileDialog1.ShowDialog() != DialogResult.OK)
            {
                this.Cursor = Cursors.Default;
                return;
            }
            this.Cursor = Cursors.WaitCursor;
            string filename = openFileDialog1.FileName;

            DataTable testDt = new DataTable();
            testDt.Columns.Add("found");
            testDt.Columns.Add("foundCommission");
            testDt.Columns.Add("foundPayment");
            testDt.Columns.Add("agentCode");
            testDt.Columns.Add("contractNumber");
            testDt.Columns.Add("commission", Type.GetType("System.Double"));
            testDt.Columns.Add("payment", Type.GetType("System.Double"));
            testDt.Columns.Add("firstName");
            testDt.Columns.Add("lastName");

            allAgentsDt = G1.get_db_data("Select * from `agents`;");

            DataRow[] dRows = null;

            try
            {
                bool first = true;
                string line = "";
                int row = 0;
                string str = "";
                string agentCode = "";
                string contractNumber = "";
                string commission = "";
                string payment = "";
                string downPayment = "";
                double money = 0D;
                string firstName = "";
                string lastName = "";

                FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using (StreamReader sr = new StreamReader(fs))

                {
                    while ((line = sr.ReadLine()) != null)
                    {
                        Application.DoEvents();
                        if (String.IsNullOrWhiteSpace(line))
                            continue;
                        if (line.Length < 147)
                            continue;
                        agentCode = line.Substring(0, 5).Trim();
                        if (String.IsNullOrWhiteSpace(agentCode))
                            continue;
                        dRows = allAgentsDt.Select("agentCode='" + agentCode + "'");
                        if (dRows.Length <= 0)
                            continue;

                        contractNumber = line.Substring(6, 10).Trim();
                        commission = line.Substring(136, 11).Trim();
                        payment = line.Substring(100, 12).Trim();
                        money = payment.ObjToDouble();
                        downPayment = line.Substring(50, 11).Trim();
                        if (money == 0D)
                            payment = downPayment;
                        lastName = line.Substring(17, 15).Trim();
                        firstName = line.Substring(33, 15).Trim();

                        if (contractNumber == "WF18165LI")
                        {

                        }

                        DataRow dRow = testDt.NewRow();

                        dRow["agentCode"] = agentCode;
                        dRow["contractNumber"] = contractNumber;
                        dRow["firstName"] = firstName;
                        dRow["lastName"] = lastName;
                        dRow["commission"] = commission;
                        dRow["payment"] = payment;
                        testDt.Rows.Add(dRow);
                        row++;
                    }
                    sr.Close();

                    CompareData(testDt);

                }
            }
            catch (Exception ex)
            {
                DevExpress.XtraEditors.XtraMessageBox.Show(ex.Message, "Error Occurred");
            }
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void CompareData (DataTable dt)
        {
            if (dt == null)
                return;
            if (dt.Rows.Count <= 0)
                return;
            dt.Columns.Add("BAD");
            string contractNumber = "";
            string agentCode = "";
            string commission = "";
            string payment = "";
            string firstName = "";
            string lastName = "";
            string cmd = "";
            DataTable dx = null;
            bool found = false;
            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                    agentCode = dt.Rows[i]["agentCode"].ObjToString();
                    commission = dt.Rows[i]["commission"].ObjToString();
                    payment = dt.Rows[i]["payment"].ObjToString();
                    if (contractNumber == "L14140UI")
                    {

                    }

                    found = FindData(contractNumber, agentCode, commission, payment );
                    if (!found)
                    {
                        dt.Rows[i]["BAD"] = "BAD";
                    }
                    else
                        dt.Rows[i]["BAD"] = "FOUND";
                }
            }
            catch ( Exception ex )
            {
                MessageBox.Show("***ERROR 1 *** " + ex.Message.ToString());
            }

            try
            {
                DataRow[] dRows = dt.Select("BAD='BAD'");
                for (int i = 0; i < dRows.Length; i++)
                {
                    contractNumber = dRows[i]["contractNumber"].ObjToString();
                    cmd = "Select * from `contracts` where `contractNumber` = '" + contractNumber + "';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count <= 0)
                        contractNumber += " (BAD)";
                    agentCode = dRows[i]["agentCode"].ObjToString();
                    commission = dRows[i]["commission"].ObjToString();
                    firstName = dRows[i]["firstName"].ObjToString();
                    lastName = dRows[i]["lastName"].ObjToString();

                    DataRow nRow = workDt.NewRow();
                    nRow["contractNumber"] = contractNumber;
                    nRow["agentNumber"] = agentCode;
                    nRow["foundCommission"] = commission;
                    nRow["firstName"] = firstName;
                    nRow["lastName"] = lastName;
                    nRow["FOUND"] = "BAD";
                    workDt.Rows.Add(nRow);
                }
            }
            catch ( Exception ex)
            {
                MessageBox.Show("***ERROR 2*** " + ex.Message.ToString());
            }
            chkFilter.Show();
            chkFoundComm.Show();
            chkRed.Show();
            G1.NumberDataTable(workDt);
            dgv.DataSource = workDt;
        }
        /****************************************************************************************/
        private bool FindData(string foundContract, string foundAgentCode, string foundCommission, string foundPayment )
        {
            bool found = false;
            DataTable dt = workDt;

            double commission = 0D;
            double calcCommission = foundCommission.ObjToDouble();
            string str = "";

            DataRow[] dRows = null;

            try
            {
//                dRows = dt.Select("contractNumber='" + foundContract + "' AND agentNumber ='" + foundAgentCode + "'");
                dRows = dt.Select("contractNumber='" + foundContract + "'");
                for (int j = 0; j < dRows.Length; j++)
                {
                    str = dRows[j]["FOUND"].ObjToString();
                    if (str.ToUpper() == "FOUND")
                        continue;
                    commission = dRows[j]["commission"].ObjToDouble();
                    if (commission == calcCommission)
                    {
                        dRows[j]["FOUND"] = "FOUND";
                        dRows[j]["foundCommission"] = commission;
                        dRows[j]["foundPayment"] = "FOUND";
                        found = true;
                        break;
                    }
                }
                if ( !found )
                {
                    for (int j = 0; j < dRows.Length; j++)
                    {
                        str = dRows[j]["FOUND"].ObjToString();
                        if (str.ToUpper() == "FOUND")
                            continue;
                        dRows[j]["FOUND"] = "FOUND";
                        dRows[j]["foundCommission"] = foundCommission;
                        dRows[j]["foundPayment"] = foundPayment;
                        found = true;
                        break;
                    }
                }
            }
            catch ( Exception ex )
            {
                MessageBox.Show("***ERROR 0*** " + ex.Message.ToString());
            }
            return found;
        }
        /****************************************************************************************/
        private int compressData ( string [] Compressed )
        {
            int compressedAnswers = 0;
            int count = G1.of_ans_count;

            for ( int i=0; i<count; i++)
            {
                if ( !String.IsNullOrWhiteSpace ( G1.of_answer[i]))
                {
                    Compressed[compressedAnswers] = G1.of_answer[i];
                    compressedAnswers++;
                    if (compressedAnswers >= 10)
                        break;
                }
            }
            return compressedAnswers;
        }
        /****************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string contractNumber = dr["contractNumber"].ObjToString();
            if (String.IsNullOrWhiteSpace(contractNumber))
                return;
            string cmd = "Select * from `contracts` where `contractNumber` = '" + contractNumber + "';";
            DataTable dt = G1.get_db_data(cmd);
            if ( dt.Rows.Count <= 0)
            {
                MessageBox.Show("***ERROR*** Bad Contract Number " + contractNumber + "!");
                return;
            }
            this.Cursor = Cursors.WaitCursor;
            CustomerDetails clientForm = new CustomerDetails(contractNumber);
            clientForm.Show();
            this.Cursor = Cursors.Default;
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
            Printer.DrawQuad(1, 1, Printer.xQuads, 2, "South Mississippi Funeral Services, LLC", Color.Black, BorderSide.Top, font, HorizontalAlignment.Center);

            Printer.SetQuadSize(12, 12);

            font = new Font("Ariel", 8);
            Printer.DrawGridDate(2, 3, 2, 3, Color.Black, BorderSide.None, font);
            Printer.DrawGridPage(11, 3, 2, 3, Color.Black, BorderSide.None, font);

            Printer.DrawQuad(1, 9, 2, 3, "User : " + LoginForm.username, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);

            font = new Font("Ariel", 10, FontStyle.Bold);
            string title = "Compare Commission Data to AS400";
            Printer.DrawQuad(6, 8, 4, 4, title, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);


//            Printer.DrawQuad(20, 8, 5, 4, title + workDate, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(16, 8, 3, 4, lblPayment.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(19, 8, 3, 4, lblTrust85.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(22, 8, 3, 4, lblTrust100.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /****************************************************************************************/
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
        /****************************************************************************************/
        private void chkFilter_CheckedChanged(object sender, EventArgs e)
        {
            gridMain.RefreshData();
            dgv.Refresh();
        }
        /****************************************************************************************/
        private void gridMain_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            if (!chkFilter.Checked && !chkFoundComm.Checked && !chkRed.Checked)
                return;
            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv.DataSource;
            string found = dt.Rows[row]["FOUND"].ObjToString();
            double foundCommission = dt.Rows[row]["foundCommission"].ObjToDouble();
            double commission = dt.Rows[row]["commission"].ObjToDouble();
            if (chkFilter.Checked) // Filter Green
            {
                if (commission == foundCommission)
                    return;
                else
                {
                    e.Visible = false;
                    e.Handled = true;
                    return;
                }
            }
            if (chkRed.Checked) // Filter Red
            {
                if (commission != foundCommission)
                    return;
                else
                {
                    e.Visible = false;
                    e.Handled = true;
                    return;
                }
            }
            if (chkFoundComm.Checked && found == "FOUND")
                e.Visible = false;
            else
                e.Visible = true;
            e.Handled = true;
            return;
        }
        /****************************************************************************************/
        private void gridMain_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
        {
            if (e.RowHandle < 0)
                return;
            if (e.Column.FieldName.ToUpper().IndexOf("FOUNDCOMMISSION") >= 0)
            {
                DataTable dt = (DataTable)dgv.DataSource;
                int row = gridMain.GetDataSourceRowIndex(e.RowHandle);

                double foundCommission = dt.Rows[row]["foundCommission"].ObjToDouble();
                double commission = dt.Rows[row]["commission"].ObjToDouble();
                if ( commission != foundCommission)
                    e.Appearance.BackColor = Color.Red;
                else
                    e.Appearance.BackColor = Color.Green;
            }
            else if (e.Column.FieldName.ToUpper() == "NUM")
            {
                if (e.RowHandle >= 0)
                {
                    string num = (e.RowHandle + 1).ToString();
                    e.DisplayText = num;
                }
            }
        }
        /****************************************************************************************/
        private void gridMain_DoubleClick_1(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string contractNumber = dr["contractNumber"].ObjToString();
            if (String.IsNullOrWhiteSpace(contractNumber))
                return;
            string cmd = "Select * from `contracts` where `contractNumber` = '" + contractNumber + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
            {
                MessageBox.Show("***ERROR*** Bad Contract Number " + contractNumber + "!");
                return;
            }
            this.Cursor = Cursors.WaitCursor;
            CustomerDetails clientForm = new CustomerDetails(contractNumber);
            clientForm.Show();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void btnSelectColumns_Click(object sender, EventArgs e)
        {
            string actualName = cmbSelectColumns.Text;
            SelectColumns sform = new SelectColumns(dgv, "CommCompare", "Primary", actualName);
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
        private void cmbSelectColumns_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetupSelectedColumns();
        }
        /***********************************************************************************************/
        private void SetupSelectedColumns()
        {
            string group = cmbSelectColumns.Text.Trim().ToUpper();
            if (group.Trim().Length == 0)
                return;
            string cmd = "Select * from procfiles where name = '" + group + "' and ProcType = 'CommCompare' order by seq";
            DataTable dt = G1.get_db_data(cmd);
            for (int i = 0; i < gridMain.Columns.Count; i++)
                gridMain.Columns[i].Visible = false;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string name = dt.Rows[i]["Description"].ToString();
                int index = dt.Rows[i]["seq"].ObjToInt32();
                try
                {
                    ((AdvBandedGridView)dgv.MainView).Columns[name].Visible = true;
                }
                catch
                {
                }
            }
        }
        /****************************************************************************************/
        private void chkFoundComm_CheckedChanged(object sender, EventArgs e)
        {
            gridMain.RefreshData();
            dgv.Refresh();
        }
        /****************************************************************************************/
        private void chkRed_CheckedChanged(object sender, EventArgs e)
        {
            gridMain.RefreshData();
            dgv.Refresh();
        }
        /****************************************************************************************/
        private void gridMain_RowCellStyle(object sender, DevExpress.XtraGrid.Views.Grid.RowCellStyleEventArgs e)
        {
            if (e.RowHandle < 0)
                return;
            GridView View = sender as GridView;
            if (e.Column.FieldName.ToUpper() == "FOUNDCOMMISSION")
            {
                string str = View.GetRowCellDisplayText(e.RowHandle, View.Columns["foundCommission"]);
                double foundCommission = str.ObjToDouble();
                str = View.GetRowCellDisplayText(e.RowHandle, View.Columns["commission"]);
                double commission = str.ObjToDouble();

                if (commission == foundCommission)
                    e.Appearance.BackColor = Color.Green;
                else
                    e.Appearance.BackColor = Color.Red;
            }
        }
        /****************************************************************************************/
    }
}