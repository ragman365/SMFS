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
using DevExpress.XtraPrinting;
using DevExpress.Utils;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class CompareInsPayments : DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        public CompareInsPayments()
        {
            InitializeComponent();
        }
        /***********************************************************************************************/
        private void CompareInsPayments_Load(object sender, EventArgs e)
        {
            btnCompare.Hide();
        }
        /***********************************************************************************************/
        private void btnSelect_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string file = ofd.FileName;
                    dgv.DataSource = null;
                    DataTable dt = Import.ImportCSVfile(file);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        dt.Columns.Add("num");
                        dt.Columns.Add("contractNumber");
                        dt.Columns.Add("status");
                        dt.Columns.Add("dueDate8");
                        dt.Columns.Add("numMonths");
                        dt.Columns.Add("lastPaidDate8");
                        dt.Columns.Add("offBy");
                        dt.Columns.Add("originalExpected", Type.GetType("System.Double"));
                        dt.Columns.Add("annual", Type.GetType("System.Double"));
                        DateTime dueDate8 = DateTime.Now;
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            dueDate8 = dt.Rows[i]["DDUE8"].ObjToDateTime();
                            dt.Rows[i]["DDUE8"] = dueDate8.ToString("MM/dd/yyyy");
                        }
                        dt.AcceptChanges();
                        G1.NumberDataTable(dt);
                        dgv.DataSource = dt;
                        btnCompare.Show();
                    }
                }
            }
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void gridMain_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            ColumnView view = sender as ColumnView;
            bool isDate = false;
            if (e.Column.FieldName.ToUpper().IndexOf("NUM") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                int row = e.GroupRowHandle;
            }
            if (e.Column.FieldName.ToUpper().IndexOf("DDUE8") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
                isDate = true;
            else if (e.Column.FieldName.ToUpper().IndexOf("DATE") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
                isDate = true;

            if (isDate)
            {
                if (e.DisplayText.IndexOf("0000") >= 0 || e.DisplayText.IndexOf("0001") >= 0)
                    e.DisplayText = "";
                else
                {
                    DateTime date = e.DisplayText.ObjToDateTime();
                    if (date.Year < 100)
                        e.DisplayText = "";
                    else
                        e.DisplayText = date.ToString("MM/dd/yyyy");
                }
            }
        }
        /***********************************************************************************************/
        private string FindPayerContract(string payer, string payment, ref string newPayer, ref double expected, ref bool isLapsed, ref string dueDate8, ref string lastPaidDate8, ref double originalExpected, ref double annual )
        {
            string contractNumber = "";
            string cmd = "";
            payer = payer.ToUpper().Replace("NEW", "");
            payer = payer.ToUpper().Replace("INSURANCE", "");
            payer = payer.Trim();
            if ( payer == "CC5132")
            {
            }
            DataTable ddx = null;
            try
            {
                cmd = "Select * from `icustomers` p JOIN `icontracts` d ON p.`contractNumber` = d.`contractNumber` where `payer` = '" + payer + "' ORDER BY p.`contractNumber` DESC;";
                ddx = G1.get_db_data(cmd);
                DateTime deceasedDate = DateTime.Now;
                DateTime dueDate = DateTime.Now;
                isLapsed = false;
                contractNumber = "";
                dueDate8 = "";
                lastPaidDate8 = "";
                string contract = "";
                originalExpected = 0D;
                annual = 0D;
                if (ddx.Rows.Count > 0)
                {
                    string lapsed = "";
                    double lastExpected = ddx.Rows[0]["amtOfMonthlyPayt"].ObjToDouble();
                    for (int i = 0; i < ddx.Rows.Count; i++)
                    {
                        deceasedDate = ddx.Rows[i]["deceasedDate"].ObjToDateTime();
                        if (deceasedDate.Year > 100)
                            continue;
                        contract = ddx.Rows[i]["contractNumber"].ObjToString();
                        if (contract.ToUpper().IndexOf("MM") >= 0)
                            continue;
                        if (contract.ToUpper().IndexOf("OO") >= 0)
                            continue;
                        dueDate = ddx.Rows[i]["dueDate8"].ObjToDateTime();
                        if (dueDate.Year < 100)
                            continue;
                        contractNumber = contract;
                        lastExpected = ddx.Rows[i]["amtOfMonthlyPayt"].ObjToDouble();
                        annual = ddx.Rows[i]["annualPremium"].ObjToDouble();
                        

                        lapsed = ddx.Rows[i]["lapsed"].ObjToString();
                        if (!String.IsNullOrWhiteSpace(lapsed))
                            isLapsed = true;
                        else
                        {
                            isLapsed = false;
                            break;
                        }
                    }
                    if (ddx.Rows.Count > 1)
                    {
                        DataTable ddd = null;
                        bool found = false;
                        string premium = "";
                        string originalContractNumber = contractNumber;
                        originalExpected = lastExpected;
                        for (int i = 0; i < ddx.Rows.Count; i++)
                        {
                            contractNumber = ddx.Rows[i]["contractNumber"].ObjToString();
                            if (originalContractNumber.ToUpper().IndexOf("OO") >= 0 && contractNumber.ToUpper().IndexOf("ZZ") >= 0)
                            {
                                originalContractNumber = contractNumber;
                                originalExpected = ddx.Rows[i]["amtOfMonthlyPayt"].ObjToDouble();
                            }
                            if (originalContractNumber.ToUpper().IndexOf("MM") >= 0 && contractNumber.ToUpper().IndexOf("ZZ") >= 0)
                            {
                                originalContractNumber = contractNumber;
                                originalExpected = ddx.Rows[i]["amtOfMonthlyPayt"].ObjToDouble();
                            }
                            cmd = "Select * from `icustomers` p JOIN `icontracts` d ON p.`contractNumber` = d.`contractNumber` WHERE p.`contractNumber` = '" + contractNumber + "';";
                            ddd = G1.get_db_data(cmd);
                            if (ddd.Rows.Count > 0)
                            {
                                premium = ddd.Rows[0]["amtOfMonthlyPayt"].ObjToString();
                                if (premium.ObjToDouble() > 0D)
                                    lastExpected = premium.ObjToDouble();
                                if (premium.ObjToDouble() == payment.ObjToDouble())
                                {
                                    expected = premium.ObjToDouble();
                                    found = true;
                                    break;
                                }
                            }
                        }
                        if (!found)
                        {
                            contractNumber = originalContractNumber;
                            expected = lastExpected;
                            if (originalExpected != 0D)
                                expected = originalExpected;
                            if ( String.IsNullOrWhiteSpace ( contractNumber))
                            {
                                newPayer = "";
                                contractNumber = ImportDailyDeposits.FindPayerContract(payer, payment, ref newPayer, ref expected, ref isLapsed);
                            }
                        }
                    }
                    else
                    {
                        expected = ddx.Rows[0]["amtOfMonthlyPayt"].ObjToDouble();
                    }
                }
            }
            catch (Exception ex)
            {
            }
            if (!String.IsNullOrWhiteSpace(contractNumber))
            {
                try
                {
                    cmd = "Select * from `icontracts` WHERE `contractNumber` = '" + contractNumber + "';";
                    ddx = G1.get_db_data(cmd);
                    if (ddx.Rows.Count > 0)
                    {
                        DateTime dueDate = ddx.Rows[0]["dueDate8"].ObjToDateTime();
                        dueDate8 = dueDate.ToString("MM/dd/yyyy");
                        dueDate = ddx.Rows[0]["lastDatePaid8"].ObjToDateTime();
                        lastPaidDate8 = dueDate.ToString("MM/dd/yyyy");
                    }
                }
                catch (Exception ex)
                {
                }
            }
            return contractNumber;
        }
        /***********************************************************************************************/
        private void btnCompare_Click(object sender, EventArgs e)
        {
            string payer = "";
            string newPayer = "";
            double expected = 0D;
            bool isLapsed = false;
            string payment = "";
            string contractNumber = "";
            string dueDate = "";

            double paid = 0D;
            double months = 0D;
            string numMonths = "";
            string lastPaidDate8 = "";

            DateTime as400DueDate = DateTime.Now;
            DateTime smfsDueDate = DateTime.Now;
            DateTime pDate = DateTime.Now;
            DateTime dDate = DateTime.Now;
            double originalExpected = 0D;
            double annual = 0D;

            DataTable dt = (DataTable)dgv.DataSource;
            if (dt == null)
                return;
            this.Cursor = Cursors.WaitCursor;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    payer = dt.Rows[i]["PAYER#"].ObjToString();
                    payer = payer.TrimStart('0');
                    payment = dt.Rows[i]["PAID"].ObjToString();
                    if ( payer == "EV-090408")
                    {

                    }
                    contractNumber = FindPayerContract(payer, payment, ref newPayer, ref expected, ref isLapsed, ref dueDate, ref lastPaidDate8, ref originalExpected, ref annual);
                    if (String.IsNullOrWhiteSpace(contractNumber))
                    {
                        dt.Rows[i]["status"] = "***BAD***";
                        continue;
                    }
                    dt.Rows[i]["contractNumber"] = contractNumber;
                    dt.Rows[i]["dueDate8"] = dueDate;
                    dt.Rows[i]["originalExpected"] = originalExpected;
                    dt.Rows[i]["annual"] = annual;
                    paid = dt.Rows[i]["PAID"].ObjToDouble();
                    pDate = lastPaidDate8.ObjToDateTime();
                    dDate = dueDate.ObjToDateTime();
                    months = DailyHistory.CheckMonthsForInsurance(contractNumber, payer, expected, paid, pDate, dDate );
                    months = G1.RoundValue(months);
                    numMonths = months.ToString();
                    dt.Rows[i]["numMonths"] = numMonths;
                    dt.Rows[i]["lastPaidDate8"] = lastPaidDate8;

                    as400DueDate = dt.Rows[i]["DDUE8"].ObjToDateTime();
                    smfsDueDate = dueDate.ObjToDateTime();
                    if (as400DueDate == smfsDueDate)
                        dt.Rows[i]["status"] = "MATCH";
                    else
                    {
                        months = G1.GetMonthsBetween(as400DueDate, smfsDueDate);
                        dt.Rows[i]["offBy"] = months.ToString();
                        dt.Rows[i]["status"] = "MISMATCH";
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("***ERROR*** " + ex.Message.ToString());
                }
            }
            dgv.DataSource = dt;
            dgv.RefreshDataSource();
            dgv.Refresh();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
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

            font = new Font("Ariel", 10, FontStyle.Bold);
            Printer.DrawQuad(5, 8, 5, 4, "Compare AS400 Insurance to SMFS Report", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);


            //            Printer.DrawQuadTicks();
            DateTime date = DateTime.Now;
            string workDate = date.Month.ToString("D2") + "/" + date.Year.ToString("D4");
            Printer.SetQuadSize(24, 12);
            font = new Font("Ariel", 9, FontStyle.Bold);
            //            Printer.DrawQuad(20, 8, 5, 4, "Report Month:" + workDate, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(16, 8, 3, 4, lblPayment.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(19, 8, 3, 4, lblTrust85.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(22, 8, 3, 4, lblTrust100.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /***********************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string contract = dr["contractNumber"].ObjToString();
            if (!String.IsNullOrWhiteSpace(contract))
            {
                this.Cursor = Cursors.WaitCursor;
                DataTable dt = (DataTable)dgv.DataSource;
                CustomerDetails clientForm = new CustomerDetails(contract);
                clientForm.Show();
                this.Cursor = Cursors.Default;
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
            else if (e.Column.FieldName.ToUpper() == "STATUS")
            {
                if (e.RowHandle >= 0)
                {
                    string status = e.DisplayText.Trim().ToUpper();
                    if (status != "MATCH")
                    {
                        e.Appearance.BackColor = Color.Red;
                        e.Appearance.ForeColor = Color.White;
                    }
                }
            }
        }
        /***********************************************************************************************/
        private void updateSMFSWithAS400DateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            string contractNumber = dr["contractNumber"].ObjToString();
            if (String.IsNullOrWhiteSpace(contractNumber))
                return;
            DialogResult result = MessageBox.Show("Are you sure you want to Set Due Date for customer (" + contractNumber + ") ?", "Set Due Date Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
            if (result == DialogResult.No)
                return;
            string dueDate = dr["DDUE8"].ObjToString();
            DateTime dueDate8 = dueDate.ObjToDateTime();
            if (dueDate8.Year < 100)
                return;
            dueDate = dueDate8.ToString("MM/dd/yyyy");
            string cmd = "Select * from `icontracts` where `contractNumber` = '" + contractNumber + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
                return;
            string record = dx.Rows[0]["record"].ObjToString();
            G1.update_db_table("icontracts", "record", record, new string[] { "dueDate8", dueDate });
            MessageBox.Show("***DONE***");
        }
        /***********************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.gridMain.OptionsFind.AlwaysVisible == true)
                    gridMain.OptionsFind.AlwaysVisible = false;
                else
                    gridMain.OptionsFind.AlwaysVisible = true;
            }
            catch (Exception ex)
            {
            }
        }
        /***********************************************************************************************/
        private void gridMain_CustomRowFilter(object sender, RowFilterEventArgs e)
        {
            if (!chkFilter.Checked)
                return;
            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv.DataSource;
            string status = dt.Rows[row]["status"].ObjToString().ToUpper();
            if (status.ToUpper() == "MATCH")
            {
                e.Visible = false;
                e.Handled = true;
            }
        }
        /***********************************************************************************************/
        private void chkFilter_CheckedChanged(object sender, EventArgs e)
        {
            gridMain.RefreshData();
            dgv.Refresh();
        }
        /***********************************************************************************************/
        private string oldPayerFile = "";
        private void verifyDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            string contractNumber = dr["contractNumber"].ObjToString();
            string payer = dr["PAYER#"].ObjToString();
            string lname = dr["PLNAME"].ObjToString();
            string fname = dr["PFNAME"].ObjToString();
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string file = ofd.FileName;
                    DataTable dx = Import.ImportCSVfile(file);
                    VerifyPayer verifyForm = new VerifyPayer(dx, contractNumber, payer, fname, lname);
                    verifyForm.Show();
                }
            }
        }
        /***********************************************************************************************/
    }
}