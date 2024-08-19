using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraPrinting;
using DevExpress.Utils;
using MySql.Data.MySqlClient;
using GeneralLib;
using DevExpress.XtraRichEdit.API.Native;
using DevExpress.XtraGrid.Views.Grid;
using MySql.Data.Types;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraEditors;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class InsuranceChecklist : DevExpress.XtraEditors.XtraForm
    {
        /****************************************************************************************/
        private string workContract = "";
        private string workServiceId = "";
        private string workPolicyNumber = "";
        private DateTime workDateReceived = DateTime.Now;
        private double workPayment = 0D;
        private bool modified = false;
        private bool loading = true;
        private bool hold = false;
        private string direction = "DOWN";
        private string workRecord = "";
        /****************************************************************************************/
        public InsuranceChecklist( string contractNumber, string serviceId, string policyNumber, DateTime dateReceived, double payment )
        {
            InitializeComponent();
            workContract = contractNumber;
            workServiceId = serviceId;
            workPolicyNumber = policyNumber;
            workDateReceived = dateReceived;
            workPayment = payment;
        }
        /****************************************************************************************/
        private void InsuranceChecklist_Load(object sender, EventArgs e)
        {
            if (workDateReceived.Year > 100)
                txtDatePaid.Text = workDateReceived.ToString("MM/dd/yyyy");
            string money = G1.ReformatMoney(workPayment);
            txtAmountPaid.Text = money;

            string cmd = "Select * from `fcustomers` where `contractNumber` = '" + workContract + "';";
            DataTable dx = G1.get_db_data(cmd);
            if ( dx.Rows.Count > 0 )
            {
                string name = dx.Rows[0]["lastname"].ObjToString() + ", " + dx.Rows[0]["firstName"].ObjToString() + " " + dx.Rows[0]["middleName"].ObjToString();
                this.Text = "Insurance Claim Checklist for " + name;
            }
            DataTable dt = new DataTable();
            dt.Columns.Add("description1");
            dt.Columns.Add("data1");
            dt.Columns.Add("description2");
            dt.Columns.Add("data2");

            AddNewRow(dt, "Funeral #", "Deceased Name: ");
            AddNewRow(dt, "Date of Birth:", "Deceased SS #:");
            AddNewRow(dt, "Date of Death:", "Policy #:");
            AddNewRow(dt, "Insurance Co:", "Date Policy Verified:");
            AddNewRow(dt, "Family Present for Verification: (Yes/No)", "Time Policy Verified:");
            AddNewRow(dt, "Agent at Ins. Company:", "Ins. Co. Phone #:");
            AddNewRow(dt, "Type Of Policy: (Ins. Policy/Annuity/Trust", "Policy Issue Date:");
            AddNewRow(dt, "Past Contestability: (Yes/No)", "Face Value of Policy:");
            AddNewRow(dt, "Loans Against Policy: (Yes/No)", "Death Benefit Of Policy");
            AddNewRow(dt, "If Yes, How Much:", "File With (Desk Copy/Certified D.C.)");
            AddNewRow(dt, "Beneficiary:", "Date Desk Copy / Cert D.C. Filed:");
            AddNewRow(dt, "Beneficiary Deceased: (Yes/No)", "Date Assignment Filed:");
            AddNewRow(dt, "D.C. Insuing Agent (Hospital/Coroner)", "Date Claim Forms Filed:");

            ReadCheckList(dt);

            dgv.DataSource = dt;

            gridMain.FocusedColumn = gridMain.Columns["data1"];

            loading = false;
            modified = false;
            btnSave.Hide();
        }
        /****************************************************************************************/
        private void AddNewRow ( DataTable dt, string desc1, string desc2 )
        {
            DataRow dRow = dt.NewRow();
            dRow["description1"] = desc1;
            dRow["description2"] = desc2;
            dt.Rows.Add(dRow);
        }
        /****************************************************************************************/
        private void gridMain_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                if ( hold )
                {
                    hold = false;
                    return;
                }
                changeData();
                //DataTable dt = (DataTable)dgv.DataSource;
                //int rowHandle = gridMain.FocusedRowHandle;

                //int row = gridMain.FocusedRowHandle;
                //GridColumn currCol = gridMain.FocusedColumn;
                //string currentColumn = currCol.FieldName;
                //string direction = cmbDirection.Text.Trim().ToUpper();
                //if (direction == "ACROSS")
                //{
                //    if (currentColumn.ToUpper() == "DESCRIPTION1")
                //        gridMain.FocusedColumn = gridMain.Columns["data1"];
                //    else if (currentColumn.ToUpper() == "DESCRIPTION2")
                //        gridMain.FocusedColumn = gridMain.Columns["data2"];
                //    else if (currentColumn.ToUpper() == "DATA1")
                //        gridMain.FocusedColumn = gridMain.Columns["data2"];
                //    else if (currentColumn.ToUpper() == "DATA2")
                //    {
                //        if (rowHandle < (dt.Rows.Count - 1))
                //        {
                //            gridMain.FocusedColumn = gridMain.Columns["data1"];
                //            rowHandle++;
                //            gridMain.SelectRow(rowHandle);
                //            gridMain.FocusedRowHandle = rowHandle;
                //        }
                //    }
                //}
                //else
                //{
                //    rowHandle++;
                //    if (currentColumn.ToUpper() == "DATA1")
                //    {
                //        if (rowHandle > (dt.Rows.Count - 1))
                //        {
                //            gridMain.FocusedColumn = gridMain.Columns["data2"];
                //            rowHandle = 0;
                //        }
                //    }
                //    else if (currentColumn.ToUpper() == "DATA2")
                //    {
                //        if (rowHandle > (dt.Rows.Count - 1))
                //        {
                //            gridMain.FocusedColumn = gridMain.Columns["data1"];
                //            rowHandle = 0;
                //        }
                //    }
                //    gridMain.SelectRow(rowHandle);
                //    gridMain.FocusedRowHandle = rowHandle;
                //}
                e.Handled = true;
            }
        }
        /****************************************************************************************/
        private void changeData()
        {
            DataTable dt = (DataTable)dgv.DataSource;
            int rowHandle = gridMain.FocusedRowHandle;

            int row = gridMain.FocusedRowHandle;
            GridColumn currCol = gridMain.FocusedColumn;
            string currentColumn = currCol.FieldName;
            string direction = cmbDirection.Text.Trim().ToUpper();
            if (direction == "ACROSS")
            {
                if (currentColumn.ToUpper() == "DESCRIPTION1")
                    gridMain.FocusedColumn = gridMain.Columns["data1"];
                else if (currentColumn.ToUpper() == "DESCRIPTION2")
                    gridMain.FocusedColumn = gridMain.Columns["data2"];
                else if (currentColumn.ToUpper() == "DATA1")
                    gridMain.FocusedColumn = gridMain.Columns["data2"];
                else if (currentColumn.ToUpper() == "DATA2")
                {
                    if (rowHandle < (dt.Rows.Count - 1))
                    {
                        gridMain.FocusedColumn = gridMain.Columns["data1"];
                        rowHandle++;
                        gridMain.SelectRow(rowHandle);
                        gridMain.FocusedRowHandle = rowHandle;
                    }
                }
            }
            else
            {
                rowHandle++;
                if (currentColumn.ToUpper() == "DATA1")
                {
                    if (rowHandle > (dt.Rows.Count - 1))
                    {
                        gridMain.FocusedColumn = gridMain.Columns["data2"];
                        rowHandle = 0;
                    }
                }
                else if (currentColumn.ToUpper() == "DATA2")
                {
                    if (rowHandle > (dt.Rows.Count - 1))
                    {
                        gridMain.FocusedColumn = gridMain.Columns["data1"];
                        rowHandle = 0;
                    }
                }
                gridMain.SelectRow(rowHandle);
                gridMain.FocusedRowHandle = rowHandle;
            }
        }
        /****************************************************************************************/
        private void gridMain_MouseDown(object sender, MouseEventArgs e)
        {
            int row = gridMain.FocusedRowHandle;
            GridColumn currCol = gridMain.FocusedColumn;
            string currentColumn = currCol.FieldName;
            if (currentColumn.ToUpper() == "DESCRIPTION1")
                gridMain.FocusedColumn = gridMain.Columns["data1"];
            else if (currentColumn.ToUpper() == "DESCRIPTION2")
                gridMain.FocusedColumn = gridMain.Columns["data2"];
            changeData();
            gridMain.RefreshEditor(true);
        }
        /****************************************************************************************/
        private void gridMain_MouseUp(object sender, MouseEventArgs e)
        {
            int row = gridMain.FocusedRowHandle;
            GridColumn currCol = gridMain.FocusedColumn;
            string currentColumn = currCol.FieldName;
            if (currentColumn.ToUpper() == "DESCRIPTION1")
                gridMain.FocusedColumn = gridMain.Columns["data1"];
            else if (currentColumn.ToUpper() == "DESCRIPTION2")
                gridMain.FocusedColumn = gridMain.Columns["data2"];
            gridMain.RefreshEditor(true);
        }
        /****************************************************************************************/
        private string WhatChanged(ref string data)
        {
            data = "";

            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowhandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowhandle);

            GridColumn currCol = gridMain.FocusedColumn;
            string currentColumn = currCol.FieldName.ToUpper();

            string what = dt.Rows[row]["description1"].ObjToString().Trim();
            data = dt.Rows[row]["data1"].ObjToString();
            if (currentColumn == "DATA2")
            {
                what = dt.Rows[row]["description2"].ObjToString().Trim();
                data = dt.Rows[row]["data2"].ObjToString();
            }
            return what;
        }
        /****************************************************************************************/
        private string lastWhat = "";
        private void gridMain_ShownEditor(object sender, EventArgs e)
        {
            GridView view = sender as GridView;

            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowhandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowhandle);

            GridColumn currCol = gridMain.FocusedColumn;
            string currentColumn = currCol.FieldName.ToUpper();

            if (currentColumn != "DATA1" && currentColumn != "DATA2")
            {
                this.bandedGridData1.ColumnEdit = null;
                this.bandedGridData2.ColumnEdit = null;
                lastWhat = "";
                return;
            }

            string what = dt.Rows[row]["description1"].ObjToString().Trim();
            if ( currentColumn == "DATA2")
                what = dt.Rows[row]["description2"].ObjToString().Trim();

            if (what == lastWhat)
                return;
            lastWhat = what;

            if (what.IndexOf("(Yes/No)") > 0)
            {
                if (currentColumn == "DATA1")
                    this.bandedGridData1.ColumnEdit = this.repositoryItemComboBox1;
                else if (currentColumn == "DATA2")
                    this.bandedGridData2.ColumnEdit = this.repositoryItemComboBox1;
            }
            else if (what.IndexOf("(Hospital/Coroner)") > 0)
            {
                if (currentColumn == "DATA1")
                    this.bandedGridData1.ColumnEdit = this.repositoryItemComboBox2;
            }
            else if (what.IndexOf("Type Of Policy") >= 0)
            {
                if (currentColumn == "DATA1")
                    this.bandedGridData1.ColumnEdit = this.repositoryItemComboBox3;
            }
            else if (what.IndexOf("File With") >= 0)
            {
                if (currentColumn == "DATA2")
                    this.bandedGridData2.ColumnEdit = this.repositoryItemComboBox4;
            }
            else
            {
                this.bandedGridData1.ColumnEdit = null;
                this.bandedGridData2.ColumnEdit = null;
                lastWhat = "";
                return;
            }
        }
        /****************************************************************************************/
        private void repositoryItemComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowhandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowhandle);

            ComboBoxEdit combo = (ComboBoxEdit)sender;
            string what = combo.Text.Trim();

            try
            {
                GridColumn currCol = gridMain.FocusedColumn;
                string currentColumn = currCol.FieldName.ToUpper();

                dr[currentColumn] = what;
                dt.Rows[row][currentColumn] = what;
                dt.AcceptChanges();
                SetModified();
            }
            catch ( Exception ex )
            {
            }
        }
        /****************************************************************************************/
        private void gridMain_CellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            if (loading)
                return;
            string column = e.Column.FieldName.Trim();
            string data = "";
            string what = WhatChanged( ref data );
            DataRow dr = gridMain.GetFocusedDataRow();
            bool processDate = false;
            if ( what.IndexOf ( "Date") == 0 )
            {
                processDate = true;
                if (what == "Date Policy Verified:")
                    processDate = false;
            }
            if ( processDate )
            {
                if (String.IsNullOrWhiteSpace(data))
                    return;
                DateTime date = data.ObjToDateTime();
                if ( date.Year < 5 )
                {
                    DialogResult result = MessageBox.Show("***ERROR*** Date Entered is Invalid!", "Bad Date Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    dr[column] = "";
                    hold = true;
                    return;
                }
                dr[column] = date.ToString("MM/dd/yyyy");
            }
            SetModified();
        }
        /****************************************************************************************/
        private void InsuranceChecklist_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!modified)
                return;
            DialogResult result = MessageBox.Show("***Question***\nData has been modified!\nWould you like to save your changes now?", "Data Modified Dialog", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            if (result == DialogResult.Cancel)
            {
                e.Cancel = true;
                return;
            }
            modified = false;
            if (result == DialogResult.No)
                return;
        }
        /****************************************************************************************/
        private void txtDatePaid_Enter(object sender, EventArgs e)
        {
            string date = txtDatePaid.Text;
            if (G1.validate_date(date))
            {
                DateTime ddate = date.ObjToDateTime();
                txtDatePaid.Text = ddate.ToString("MM/dd/yyyy");
            }
            else
                txtDatePaid.Text = "";
        }
        private void txtDatePaid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                txtDatePaid_Leave(sender, e);
            else if (e.KeyCode == Keys.Tab)
                txtDatePaid_Leave(sender, e);
        }
        private void txtDatePaid_Leave(object sender, EventArgs e)
        {
            string date = txtDatePaid.Text;
            if (String.IsNullOrWhiteSpace(date))
                return;
            if (G1.validate_date(date))
            {
                DateTime ddate = txtDatePaid.Text.ObjToDateTime();
                if (ddate.Year < 1800)
                {
                    MessageBox.Show("***ERROR*** Date Entered Year is less than 1800!");
                    return;
                }
                if (ddate.Year > 100)
                {
                    txtDatePaid.Text = ddate.ToString("MM/dd/yyyy");
                    SetModified();
                }
            }
            else
            {
                MessageBox.Show("***ERROR*** Invalid Date!");
            }
        }
        /****************************************************************************************/
        private void SetModified ()
        {
            modified = true;
            btnSave.Show();
            btnSave.Refresh();
        }
        /****************************************************************************************/
        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            SetModified();
        }
        /****************************************************************************************/
        private void txtAmountPaid_TextChanged(object sender, EventArgs e)
        {
            string data = txtAmountPaid.Text.Trim();
            data = data.Replace(",", "");
            data = data.Replace("$", "");
            if ( !G1.validate_numeric ( data))
            {
                DialogResult result = MessageBox.Show("***ERROR*** Amount Paid Entered is Invalid!", "Bad Data Value Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                txtAmountPaid.Text = "";
                return;
            }
            SetModified();
        }
        /****************************************************************************************/
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            SetModified();
        }
        /****************************************************************************************/
        private void cmbDirection_SelectedIndexChanged(object sender, EventArgs e)
        {
            string direction = cmbDirection.Text.Trim();
            if ( direction.ToUpper() == "ACROSS")
                gridMain.OptionsNavigation.EnterMoveNextColumn = true;
            else
                gridMain.OptionsNavigation.EnterMoveNextColumn = false;
        }
        /****************************************************************************************/
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
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
            printableComponentLink1.CreateReportFooterArea += PrintableComponentLink1_CreateReportFooterArea;
            printableComponentLink1.BeforeCreateAreas += new System.EventHandler(this.printableComponentLink1_BeforeCreateAreas);
            printableComponentLink1.AfterCreateAreas += new System.EventHandler(this.printableComponentLink1_AfterCreateAreas);
            printableComponentLink1.Landscape = true;

            Printer.setupPrinterMargins(50, 50, 120, 50);

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
        private void PrintableComponentLink1_CreateReportFooterArea(object sender, CreateAreaEventArgs e)
        {
            string text = "Notes : \n" + rtb.Text + "\n\n\n";
            text += "Employee Who Verified Insurance : " + txtVerifyEmployee.Text;
            TextBrick brick = e.Graph.DrawString(text, Color.Black, new RectangleF(0, 10, 400, 300), BorderSide.None);
            brick.BackColor = Color.White;
            brick.Font = new Font("Ariel", 10, FontStyle.Regular);
            brick.StringFormat = new DevExpress.XtraPrinting.BrickStringFormat(StringAlignment.Near);
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
            printableComponentLink1.CreateReportFooterArea += PrintableComponentLink1_CreateReportFooterArea;
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

            font = new Font("Ariel", 10, FontStyle.Regular);
            Printer.DrawGridDate(2, 2, 2, 3, Color.Black, BorderSide.None, font);
            Printer.DrawGridPage(11, 2, 2, 3, Color.Black, BorderSide.None, font);

            Printer.DrawQuad(1, 4, 2, 3, "User : " + LoginForm.username, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);

            //font = new Font("Ariel", 10, FontStyle.Regular);
            Printer.DrawQuad(6, 4, 2, 3, "Insurance Checklist", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);


            //            Printer.DrawQuadTicks();
            DateTime date = DateTime.Now;
            string workDate = date.ToString("MM/dd/yyyy");
            //Printer.SetQuadSize(24, 12);
            //font = new Font("Ariel", 9, FontStyle.Regular);
            Printer.DrawQuad(11, 4, 2, 3, "Report Date:" + workDate, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            Printer.DrawQuad(1, 7, 2, 3, "Date Paid : " + txtDatePaid.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);
            Printer.DrawQuad(1, 9, 2, 3, "Amt Paid : " + txtAmountPaid.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);


            //Printer.DrawQuad(16, 8, 3, 4, lblPayment.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(19, 8, 3, 4, lblTrust85.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(22, 8, 3, 4, lblTrust100.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /****************************************************************************************/
        private string GetFieldData(DataTable dt, string column, string field )
        {
            if (String.IsNullOrWhiteSpace(field))
                return "";
            if (dt.Rows.Count <= 0)
                return "";
            if (String.IsNullOrWhiteSpace(column))
                return "";

            string answer = "";
            try
            {
                string cmd = "description1='Funeral #";
                string cmx = column + "='" + field + "'";
                DataRow[] dRows = dt.Select(cmx);

                if (dRows.Length > 0)
                {
                    if (column == "description1")
                        answer = dRows[0]["data1"].ObjToString();
                    else
                        answer = dRows[0]["data2"].ObjToString();
                }
            }
            catch (Exception ex)
            {
            }
            return answer;
        }
        /****************************************************************************************/
        private void AddFieldData ( DataTable dt, string column, string field, string answer )
        {
            if (String.IsNullOrWhiteSpace(field))
                return;
            if (dt.Rows.Count <= 0)
                return;
            if (String.IsNullOrWhiteSpace(column))
                return;
            try
            {
                string cmd = "description1='Funeral #";
                string cmx = column + "='" + field +"'";
                DataRow[] dRows = dt.Select(cmx);

                if (dRows.Length > 0)
                {
                    if ( column == "description1")
                        dRows[0]["data1"] = answer;
                    else
                        dRows[0]["data2"] = answer;
                }
            }
            catch ( Exception ex)
            {
            }
        }
        /****************************************************************************************/
        private void ReadCheckList ( DataTable dt )
        {
            //AddNewRow(dt, "Funeral #", "Deceased Name: ");
            //AddNewRow(dt, "Date of Birth:", "Deceased SS #:");
            //AddNewRow(dt, "Date of Death:", "Policy #:");

            //AddNewRow(dt, "Insurance Co:", "Date Policy Verified:");
            //AddNewRow(dt, "Family Present for Verification: (Yes/No)", "Time Policy Verified:");
            //AddNewRow(dt, "Agent at Ins. Company:", "Ins. Co. Phone #:");
            //AddNewRow(dt, "Type Of Policy: (Ins. Policy/Annuity/Trust", "Policy Issue Date:");
            //AddNewRow(dt, "Past Contestability: (Yes/No)", "Face Value of Policy:");
            //AddNewRow(dt, "Loans Against Policy: (Yes/No)", "Death Benefit Of Policy");
            //AddNewRow(dt, "If Yes, How Much:", "File With (Desk Copy/Certified D.C.)");
            //AddNewRow(dt, "Beneficiary:", "Date Desk Copy / Cert D.C. Filed:");
            //AddNewRow(dt, "Beneficiary Deceased: (Yes/No)", "Date Assignment Filed:");
            //AddNewRow(dt, "D.C. Insuing Agent (Hospital/Coroner)", "Date Claim Forms Filed:");

            try
            {
                workRecord = "";

                AddFieldData(dt, "description1", "Funeral #", workServiceId);
                AddFieldData(dt, "description2", "Policy #:", workPolicyNumber);

                string cmd = "Select * from `fcustomers` where `contractNumber` = '" + workContract + "';";
                DataTable dx = G1.get_db_data(cmd);
                if (dx.Rows.Count <= 0)
                    return;
                string ssn = dx.Rows[0]["ssn"].ObjToString();
                AddFieldData(dt, "description2", "Deceased SS #:", ssn);

                string name = dx.Rows[0]["lastName"].ObjToString() + ", " + dx.Rows[0]["firstName"].ObjToString() + " " + dx.Rows[0]["middleName"].ObjToString();
                AddFieldData(dt, "description2", "Deceased Name:", name);

                DateTime date = dx.Rows[0]["birthDate"].ObjToDateTime();
                AddFieldData(dt, "description1", "Date of Birth:", date.ToString("MM/dd/yyyy"));

                date = dx.Rows[0]["deceasedDate"].ObjToDateTime();
                AddFieldData(dt, "description1", "Date of Death:", date.ToString("MM/dd/yyyy"));


                cmd = "Select * from `cust_payment_ins_checklist` where `contractNumber` = '" + workContract + "' and `serviceId` = '" + workServiceId + "';";
                if ( !String.IsNullOrWhiteSpace ( workPolicyNumber ))
                    cmd = "Select * from `cust_payment_ins_checklist` where `contractNumber` = '" + workContract + "' and `serviceId` = '" + workServiceId + "' AND `policyNumber` = '" + workPolicyNumber + "';";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count <= 0)
                    return;
                workRecord = dx.Rows[0]["record"].ObjToString();

                string notes = dx.Rows[0]["notes"].ObjToString();
                rtb.Text = notes;
                txtVerifyEmployee.Text = dx.Rows[0]["verifyEmployee"].ObjToString();

                string insCompany = dx.Rows[0]["insCompany"].ObjToString();
                AddFieldData(dt, "description1", "Insurance Co:", insCompany);
                string familyPresent = dx.Rows[0]["familyPresent"].ObjToString();
                AddFieldData(dt, "description1", "Family Present for Verification: (Yes/No)", familyPresent);
                string agentAtInsCo = dx.Rows[0]["agentAtInsCo"].ObjToString();
                AddFieldData(dt, "description1", "Agent at Ins. Company:", agentAtInsCo);
                string typeOfPolicy = dx.Rows[0]["typeOfPolicy"].ObjToString();
                AddFieldData(dt, "description1", "Type Of Policy: (Ins. Policy/Annuity/Trust", typeOfPolicy);
                string pastContest = dx.Rows[0]["pastContest"].ObjToString();
                AddFieldData(dt, "description1", "Past Contestability: (Yes/No)", pastContest);
                string loans = dx.Rows[0]["loans"].ObjToString();
                AddFieldData(dt, "description1", "Loans Against Policy: (Yes/No)", loans);
                string howMuch = dx.Rows[0]["howMuch"].ObjToString();
                AddFieldData(dt, "description1", "If Yes, How Much:", howMuch);
                string beneficiary = dx.Rows[0]["beneficiary"].ObjToString();
                AddFieldData(dt, "description1", "Beneficiary:", beneficiary);
                string beneficiaryDeceased = dx.Rows[0]["beneficiaryDeceased"].ObjToString();
                AddFieldData(dt, "description1", "Beneficiary Deceased: (Yes/No)", beneficiaryDeceased);
                string dcInsuringAgent = dx.Rows[0]["dcInsuringAgent"].ObjToString();
                AddFieldData(dt, "description1", "D.C. Insuing Agent (Hospital/Coroner)", dcInsuringAgent);

                string datePolicyVerified = dx.Rows[0]["datePolicyVerified"].ObjToString();
                AddFieldData(dt, "description2", "Date Policy Verified:", datePolicyVerified);
                string timePolicyVerified = dx.Rows[0]["timePolicyVerified"].ObjToString();
                AddFieldData(dt, "description2", "Time Policy Verified:", timePolicyVerified);
                string insCoPhone = dx.Rows[0]["insCoPhone"].ObjToString();
                AddFieldData(dt, "description2", "Ins. Co. Phone #:", insCoPhone);

                string policyIssueDate = dx.Rows[0]["policyIssueDate"].ObjToString();
                AddFieldData(dt, "description2", "Policy Issue Date:", policyIssueDate);

                string deathBenefit = dx.Rows[0]["deathBenefit"].ObjToString();
                AddFieldData(dt, "description2", "Death Benefit Of Policy", deathBenefit);

                string faceValue = dx.Rows[0]["faceValue"].ObjToString();
                AddFieldData(dt, "description2", "Face Value of Policy:", faceValue);

                string fileWith = dx.Rows[0]["fileWith"].ObjToString();
                AddFieldData(dt, "description2", "File With (Desk Copy/Certified D.C.)", fileWith);
                string dateDeskCopy = dx.Rows[0]["dateDeskCopy"].ObjToString();
                AddFieldData(dt, "description2", "Date Desk Copy / Cert D.C. Filed:", dateDeskCopy);
                string dateAssignmentFiled = dx.Rows[0]["dateAssignmentFiled"].ObjToString();
                AddFieldData(dt, "description2", "Date Assignment Filed:", dateAssignmentFiled);
                string dateClaimFormsFiled = dx.Rows[0]["dateClaimFormsFiled"].ObjToString();
                AddFieldData(dt, "description2", "Date Claim Forms Filed:", dateClaimFormsFiled);
            }
            catch ( Exception ex)
            {
            }
        }
        /****************************************************************************************/
        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(workRecord))
                    workRecord = G1.create_record("cust_payment_ins_checklist", "verifyEmployee", "-1");
                if (G1.BadRecord("cust_payment_ins_checklist", workRecord))
                    return;
                string verifyEmployee = txtVerifyEmployee.Text;
                string notes = rtb.Text.Trim();
                DateTime datePaid = txtDatePaid.Text.ObjToDateTime();
                double amtPaid = txtAmountPaid.Text.ObjToDouble();
                amtPaid = G1.RoundValue(amtPaid);

                G1.update_db_table("cust_payment_ins_checklist", "record", workRecord, new string[] { "contractNumber", workContract, "serviceId", workServiceId, "policyNumber", workPolicyNumber, "verifyEmployee", verifyEmployee, "notes", notes, "datePaid", datePaid.ToString("MM/dd/yyyy"), "amtPaid", amtPaid.ToString() });

                DataTable dt = (DataTable)dgv.DataSource;

                //AddNewRow(dt, "Insurance Co:", "Date Policy Verified:");
                //AddNewRow(dt, "Family Present for Verification: (Yes/No)", "Time Policy Verified:");
                //AddNewRow(dt, "Agent at Ins. Company:", "Ins. Co. Phone #:");
                //AddNewRow(dt, "Type Of Policy: (Ins. Policy/Annuity/Trust", "Policy Issue Date:");
                //AddNewRow(dt, "Past Contestability: (Yes/No)", "Face Value of Policy:");
                //AddNewRow(dt, "Loans Against Policy: (Yes/No)", "Death Benefit Of Policy");
                //AddNewRow(dt, "If Yes, How Much:", "File With (Desk Copy/Certified D.C.)");
                //AddNewRow(dt, "Beneficiary:", "Date Desk Copy / Cert D.C. Filed:");
                //AddNewRow(dt, "Beneficiary Deceased: (Yes/No)", "Date Assignment Filed:");
                //AddNewRow(dt, "D.C. Insuing Agent (Hospital/Coroner)", "Date Claim Forms Filed:");

                string insCompany = GetFieldData(dt, "description1", "Insurance Co:");
                string familyPresent = GetFieldData(dt, "description1", "Family Present for Verification: (Yes/No)");
                string agentAtInsCo = GetFieldData(dt, "description1", "Agent at Ins. Company:");
                string typeOfPolicy = GetFieldData(dt, "description1", "Type Of Policy: (Ins. Policy/Annuity/Trust");
                string pastContest = GetFieldData(dt, "description1", "Past Contestability: (Yes/No)");
                string loans = GetFieldData(dt, "description1", "Loans Against Policy: (Yes/No)");
                string howMuch = GetFieldData(dt, "description1", "If Yes, How Much:");
                string beneficiary = GetFieldData(dt, "description1", "Beneficiary:");
                string beneficiaryDeceased = GetFieldData(dt, "description1", "Beneficiary Deceased: (Yes/No)");
                string dcInsuringAgent = GetFieldData(dt, "description1", "D.C. Insuing Agent (Hospital/Coroner)");


                string datePolicyVerified = GetFieldData(dt, "description2", "Date Policy Verified:");
                string timePolicyVerified = GetFieldData(dt, "description2", "Time Policy Verified:");
                string issueDate = GetFieldData(dt, "description2", "Policy Issue Date:");
                string insCoPhone = GetFieldData(dt, "description2", "Ins. Co. Phone #:");
                string deathBenefit = GetFieldData(dt, "description2", "Death Benefit Of Policy");
                string faceValue = GetFieldData(dt, "description2", "Face Value of Policy:");
                string fileWith = GetFieldData(dt, "description2", "File With (Desk Copy/Certified D.C.)");
                string dateDeskCopy = GetFieldData(dt, "description2", "Date Desk Copy / Cert D.C. Filed:");
                string dateAssignmentFiled = GetFieldData(dt, "description2", "Date Assignment Filed:");
                string dateClaimFormsFiled = GetFieldData(dt, "description2", "Date Claim Forms Filed:");

                G1.update_db_table("cust_payment_ins_checklist", "record", workRecord, new string[] { "insCompany", insCompany, "familyPresent", familyPresent, "agentAtInsCo", agentAtInsCo, "typeOfPolicy", typeOfPolicy, "pastContest", pastContest});
                G1.update_db_table("cust_payment_ins_checklist", "record", workRecord, new string[] {"loans", loans, "howMuch", howMuch, "dcInsuringAgent", dcInsuringAgent, "beneficiary", beneficiary, "beneficiaryDeceased", beneficiaryDeceased, "datePolicyVerified", datePolicyVerified, "timePolicyVerified", timePolicyVerified, "policyIssueDate", issueDate });
                G1.update_db_table("cust_payment_ins_checklist", "record", workRecord, new string[] { "insCoPhone", insCoPhone, "deathBenefit", deathBenefit, "faceValue", faceValue, "fileWith", fileWith, "dateDeskCopy", dateDeskCopy, "dateAssignmentFiled", dateAssignmentFiled, "dateClaimFormsFiled", dateClaimFormsFiled });

                modified = false;
                btnSave.Hide();
                btnSave.Refresh();
            }
            catch ( Exception ex)
            {
            }
        }
        /****************************************************************************************/
    }
}