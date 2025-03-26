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
using DevExpress.Office.Utils;
using DevExpress.XtraGrid;
//using java.awt;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class EditCC : DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        private string workWho = "";
        private bool workEdit = false;
        private bool workReport = false;
        private string lastFileCreated = "";
        /***********************************************************************************************/
        public EditCC()
        {
            InitializeComponent();
        }
        /***********************************************************************************************/
        private void EditCC_Load(object sender, EventArgs e)
        {
            gridMain.Columns["draftAmount"].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Custom;
            gridMain.Columns["draftAmount"].SummaryItem.DisplayFormat = "{0:C2}";

            this.Text = "Credit Card Customers";

            LoadData();
        }
        /***********************************************************************************************/
        private void LoadData()
        {
            string cmd = "Select * from `creditcards` ORDER by `draftStartDay`;";
            DataTable dt = G1.get_db_data(cmd);

            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
        }
        /***********************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            G1.ShowHideFindPanel(gridMain);
        }
        /***********************************************************************************************/
        private void gridMain_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            ColumnView view = sender as ColumnView;

            if (e.Column.FieldName.ToUpper().IndexOf("EXPIRATIONDATE") >= 0 )
            {
                return;
            }
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
            string payer = dr["payer"].ObjToString();
            if ( !String.IsNullOrWhiteSpace ( payer))
            {
                string cmd = "Select * from `payers` where `payer` = '" + payer + "';";
                DataTable dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                    contract = dx.Rows[0]["contractNumber"].ObjToString();
            }
            if (!String.IsNullOrWhiteSpace(contract))
            {
                this.Cursor = Cursors.WaitCursor;
                using (CustomerDetails clientForm = new CustomerDetails(contract))
                {
                    clientForm.ShowDialog();
                }
                string cmd = "Select * from `creditcards` where `contractNumber` = '" + contract + "';";
                DataTable dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    string accountNumber = dx.Rows[0]["ccNumber"].ObjToString();
                    string dayOfMonth = dx.Rows[0]["draftStartDay"].ObjToString();

                    string dom = dx.Rows[0]["draftStartDay"].ObjToString();
                    string spayment = dx.Rows[0]["draftAmount"].ObjToString();
                    double ccMonthlyPayment = spayment.ObjToDouble();
                    spayment = G1.ReformatMoney(ccMonthlyPayment);

                    string numPayments = dx.Rows[0]["numPayments"].ObjToString();
                    string leftPayments = dx.Rows[0]["remainingPayments"].ObjToString();
                    DateTime dateBeginning = dx.Rows[0]["draftStartDate"].ObjToDateTime();
                    string expirationDate = dx.Rows[0]["expirationDate"].ObjToString();

                    string insFirstName = dx.Rows[0]["insFirstName"].ObjToString();
                    string insMiddleName = dx.Rows[0]["insMiddleName"].ObjToString();
                    string insLastName = dx.Rows[0]["insLastName"].ObjToString();

                    string cardFirstName = dx.Rows[0]["cardFirstName"].ObjToString();
                    string cardMiddleName = dx.Rows[0]["cardMiddleName"].ObjToString();
                    string cardLastName = dx.Rows[0]["cardLastName"].ObjToString();

                    string billingZip = dx.Rows[0]["billingZip"].ObjToString();



                    dr["draftStartDay"] = dom.ObjToInt32();
                    dr["remainingPayments"] = leftPayments.ObjToInt32();
                    dr["numPayments"] = numPayments.ObjToInt32();
                    dr["draftAmount"] = ccMonthlyPayment;
                    dr["ccNumber"] = accountNumber;
                    dr["draftStartDay"] = dayOfMonth;
                    dr["insFirstName"] = insFirstName;
                    dr["insMiddleName"] = insMiddleName;
                    dr["insLastName"] = insLastName;
                    dr["cardFirstname"] = cardFirstName;
                    dr["cardMiddlename"] = cardMiddleName;
                    dr["cardLastname"] = cardLastName;
                    dr["draftStartDate"] = G1.DTtoMySQLDT(dateBeginning);
                    dr["expirationDate"] = expirationDate;
                    dr["billingZip"] = billingZip;
                    gridMain.RefreshData();
                }
                this.Cursor = Cursors.Default;
            }
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
            string text = "Credit Card Customers for " + DateTime.Now.ToString("MM/dd/yyyy");
            Printer.DrawQuad(6, 7, 4, 4, text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);

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
        private void gridMain_CustomRowFilter(object sender, RowFilterEventArgs e)
        {
            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv.DataSource;
            string status = dt.Rows[row]["status"].ObjToString().Trim().ToUpper();

            //if (String.IsNullOrWhiteSpace(status))
            //{
            //    e.Visible = false;
            //    e.Handled = true;
            //}
        }
        /***********************************************************************************************/
        private double CalculateTotalPayments ( DataTable dt )
        {
            double totalPayments = 0D;
            string str = "";
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                str = dt.Rows[i]["draftAmount"].ObjToString();
                str = str.Replace("$", "");
                str = str.Replace(",", "");
                totalPayments += str.ObjToDouble();
            }
            return totalPayments;
        }
        /***********************************************************************************************/
        private void gridMain_CustomSummaryCalculate(object sender, DevExpress.Data.CustomSummaryEventArgs e)
        {
            string field = (e.Item as GridSummaryItem).FieldName.ObjToString();

            if (field.ToUpper() == "DRAFTAMOUNT")
            {
                DataTable dt = (DataTable)dgv.DataSource;
                e.TotalValueReady = true;
                e.TotalValue = CalculateTotalPayments(dt);
            }
        }
        /***********************************************************************************************/
        private void gridMain_CustomDrawCell(object sender, RowCellCustomDrawEventArgs e)
        {
            if (e.Column.FieldName.ToUpper() == "NUM")
            {
                if (e.RowHandle >= 0)
                {
                    string num = (e.RowHandle + 1).ToString();
                    e.DisplayText = num;
                }
            }
        }
        /***********************************************************************************************/
        private void pictureDelete_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt.Rows.Count <= 0)
                return;
            DataRow dr = gridMain.GetFocusedDataRow();
            if (dr == null)
                return;

            bool gotCredit = false;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            string record = dr["record"].ObjToString();
            string contractNumber = dr["contractNumber"].ObjToString().ToUpper();

            DialogResult result = DevExpress.XtraEditors.XtraMessageBox.Show("***Question***\nDo you really want to REMOVE Credit Card for (" + contractNumber + ")?", "Remove Credit Card Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;
            if (String.IsNullOrWhiteSpace(record))
                return;
            if (record == "-1" || record == "0")
                return;

            G1.delete_db_table("creditcards", "record", record);

            LoadData();

            //try // Can not get this to work when deleting the first row
            //{
            //    gridMain.DeleteRow(gridMain.FocusedRowHandle);
            //    dt.Rows.RemoveAt(row);
            //    dt.AcceptChanges();
            //}
            //catch (Exception ex)
            //{
            //}
            //G1.NumberDataTable(dt);
            //dgv.DataSource = dt;
            //dgv.RefreshDataSource();
            //dgv.Refresh();
        }
        /***********************************************************************************************/
    }
}