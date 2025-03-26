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
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class ImportPreneedContacts : DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        /* Forethough - Import All Active Data. This code determines Pre or Post */
        /* FDLIC - Import All - FDLIC PB is included all together
        /***********************************************************************************************/
        private DataTable workDt = null;
        private string workWhat = "";
        private bool workDC = false;
        private string title = "";

        private DataTable problemDt = null;
        /***********************************************************************************************/
        public ImportPreneedContacts()
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
        private void ImportPreneedContacts_Load(object sender, EventArgs e)
        {
            btnSave.Hide();
            barImport.Hide();
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
            DataTable dt = (DataTable)dgv.DataSource;

            DataTable dx = null;
            string record = "";
            string cmd = "";


            string fields = "";
            string data = "";
            string str = "";
            string field = "";
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

                for (int i = 0; i < rows; i++)
                {
                    fields = "";
                    data = "";
                    lines = "";

                    barImport.Value = i;
                    barImport.Refresh();

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
                        workDt = ExcelWriter.ReadFile2(file);

                        workDt.TableName = actualFile;

                        workDt = ProcessTheData(workDt);
                    }
                    catch (Exception ex)
                    {
                    }
                    workDt.TableName = actualFile;

                    if (G1.get_column_number(workDt, "num") < 0)
                        workDt.Columns.Add("num").SetOrdinal(0);
                    G1.NumberDataTable(workDt);
                    dgv.DataSource = workDt;

                    btnSave.Show();
                    btnSave.Refresh();
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
    }
}