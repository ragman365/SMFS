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
using DevExpress.XtraPrinting;
using DevExpress.Utils;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class TrustCoupons : DevExpress.XtraEditors.XtraForm
    {
        private DataTable workDt = null;
        /***********************************************************************************************/
        public TrustCoupons( DataTable dt )
        {
            InitializeComponent();
            workDt = dt;
        }
        /***********************************************************************************************/
        private void TrustCoupons_Load(object sender, EventArgs e)
        {
            string paymentType = "";
            string contractNumber = "";
            string contract = "";
            string trust = "";
            string loc = "";
            bool copy = false;
            DataTable localDt = workDt.Clone();
            for ( int i=(workDt.Rows.Count-1); i>=0; i--)
            {
                contractNumber = workDt.Rows[i]["TRUST_NUMBER"].ObjToString();
                contract = Trust85.decodeContractNumber(contract, ref trust, ref loc);
                copy = false;
                if (trust.ToUpper() == "LI")
                    copy = true;
                else
                {
                    paymentType = workDt.Rows[i]["PAYMENT_PREMIUM_TYPE"].ObjToString();
                    if (paymentType.ToUpper() == "COUPON BOOK")
                        copy = true;
                }
                if (copy)
                    G1.copy_dt_row(workDt, i, localDt, localDt.Rows.Count);
            }

            DataTable dx = generateCouponList(localDt);
            dgv.DataSource = dx;
        }
        /***********************************************************************************************/
        private DataTable generateCouponList ( DataTable dt )
        {
            DataTable dx = new DataTable();
            dx.Columns.Add("Number");
            dx.Columns.Add("Contract");
            dx.Columns.Add("Name");
            dx.Columns.Add("Empty");
            dx.Columns.Add("Re");
            dx.Columns.Add("Address");
            dx.Columns.Add("CityStateZip");
            dx.Columns.Add("ALWAYSM");
            dx.Columns.Add("Zero");
            dx.Columns.Add("NumPayments");
            dx.Columns.Add("Payment");
            dx.Columns.Add("IssueDate");
            dx.Columns.Add("ALWAYSM2");

            string contract = "";
            string firstName = "";
            string middleName = "";
            string lastName = "";
            string suffix = "";
            string name = "";
            string re = "";
            string address = "";
            string address1 = "";
            string address2 = "";
            string city = "";
            string state = "";
            string zip = "";
            string gender = "";
            string payment = "";
            string issueDate = "";
            string gender2 = "";
            string str = "";
            DateTime date = DateTime.Now;

            DataRow dRow = null;

            string SDICode = "";
            DataTable sdiDt = null;
            string cnum = "";
            string trust = "";
            string loc = "";
            string cmd = "";
            double numberPayments = 0D;
            string TOTAL_TO_PAY = "";

            for ( int i=0; i<dt.Rows.Count; i++)
            {
                contract = dt.Rows[i]["TRUST_NUMBER"].ObjToString();
                dRow = dx.NewRow();
                str = contract.PadLeft(10, '0');
                dRow["contract"] = str;

                dRow["re"] = "RE: Contract " + contract;

                SDICode = "";
                cnum = Trust85.decodeContractNumber(contract, ref trust, ref loc);
                if (!String.IsNullOrWhiteSpace(loc))
                {
                    cmd = "Select * from `funeralHomes` where `keycode` = '" + loc + "';";
                    sdiDt = G1.get_db_data(cmd);
                    if (sdiDt.Rows.Count > 0)
                        SDICode = sdiDt.Rows[0]["SDICode"].ObjToString();
                }
                if (String.IsNullOrWhiteSpace(SDICode))
                    SDICode = "XX";
                dRow["Number"] = "947401" + SDICode;

                address1 = dt.Rows[i]["PURCHASER_ADDRESS1"].ObjToString();
                address2 = dt.Rows[i]["PURCHASER_ADDRESS2"].ObjToString();
                address = address1;
                if ( !String.IsNullOrWhiteSpace ( address2 ))
                {
                    if (!String.IsNullOrWhiteSpace(address))
                        address += " ";
                    address += address2;
                }
                dRow["address"] = address;

                city = dt.Rows[i]["PURCHASER_CITY"].ObjToString();
                state = dt.Rows[i]["PURCHASER_STATE"].ObjToString();
                zip = dt.Rows[i]["PURCHASER_ZIP"].ObjToString();

                name = "";
                name = BuildName(name, city);
                if (!String.IsNullOrWhiteSpace(name))
                    name += ", ";
                name = BuildName(name, state);
                name = BuildName(name, zip);
                dRow["CityStateZip"] = name;

                firstName = dt.Rows[i]["PURCHASER_FIRST_NAME"].ObjToString();
                middleName = dt.Rows[i]["PURCHASER_MIDDLE_INITIAL"].ObjToString();
                lastName = dt.Rows[i]["PURCHASER_LAST_NAME"].ObjToString();
                //suffix = dt.Rows[i]["PURCHASER_SUFFIX"].ObjToString();
                name = "";
                name = BuildName(name, firstName);
                name = BuildName(name, middleName);
                name = BuildName(name, lastName);
                //name = BuildName(name, suffix);
                dRow["name"] = name;

                gender = dt.Rows[i]["PURCHASER_GENDER"].ObjToString();
                //dRow["gender"] = gender;
                //dRow["gender2"] = gender;
                dRow["ALWAYSM"] = "M";
                dRow["ALWAYSM2"] = "M";

                TOTAL_TO_PAY = dt.Rows[i]["TOTAL_TO_PAY"].ObjToString();
                payment = dt.Rows[i]["PREMIUM"].ObjToString();
                dRow["payment"] = payment;
                numberPayments = TOTAL_TO_PAY.ObjToDouble() / payment.ObjToDouble();
                dRow["NumPayments"] = numberPayments.ObjToString();

                dRow["zero"] = "0";

                issueDate = dt.Rows[i]["TRUST_SEQ_DATE"].ObjToString();
                if (issueDate.Length >= 8)
                {
                    if (issueDate.IndexOf("/") >= 0)
                    {
                        date = issueDate.ObjToDateTime();
                        issueDate = date.Year.ToString("D4") + date.Month.ToString("D2") + date.Day.ToString("D2");
                    }
                    else
                        issueDate = issueDate.Substring(0, 8);
                }

                if (!G1.validate_date(issueDate))
                {
                    issueDate = dt.Rows[i]["SIGNED_DATE"].ObjToString();
                    if (issueDate.Length >= 8)
                    {
                        if (issueDate.IndexOf("/") >= 0)
                        {
                            date = issueDate.ObjToDateTime();
                            issueDate = date.Year.ToString("D4") + date.Month.ToString("D2") + date.Day.ToString("D2");
                        }
                        else
                            issueDate = issueDate.Substring(0, 8);
                    }
                }
                date = issueDate.ObjToDateTime();
                dRow["IssueDate"] = date.ToString("MM/dd/yyyy");
                dx.Rows.Add(dRow);
            }
            return dx;
        }
        /***********************************************************************************************/
        private string BuildName ( string name, string text )
        {
            if ( !String.IsNullOrWhiteSpace ( text ))
            {
                if (!String.IsNullOrWhiteSpace(name))
                    name += " ";
                name += text;
            }
            return name;
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
            if ( !chkIncludeHeader.Checked )
                gridMain.OptionsPrint.PrintHeader = false;
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
            gridMain.OptionsPrint.PrintHeader = true;
        }
        /***********************************************************************************************/
        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!chkIncludeHeader.Checked)
                gridMain.OptionsPrint.PrintHeader = false;
            isPrinting = true;
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
            isPrinting = false;
            gridMain.OptionsPrint.PrintHeader = true;
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
            if (!chkIncludeHeader.Checked)
                return;
            Printer.setupPrinterQuads(e, 2, 3);
            Font font = new Font("Ariel", 16);
            Printer.DrawQuad(1, 1, Printer.xQuads, 2, "South Mississippi Funeral Services, LLC", Color.Black, BorderSide.Top, font, HorizontalAlignment.Center);

            Printer.SetQuadSize(12, 12);

            font = new Font("Ariel", 8);
            Printer.DrawGridDate(2, 3, 2, 3, Color.Black, BorderSide.None, font);
            Printer.DrawGridPage(11, 3, 2, 3, Color.Black, BorderSide.None, font);

            Printer.DrawQuad(1, 9, 2, 3, "User : " + LoginForm.username, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);


            font = new Font("Ariel", 10, FontStyle.Bold);
            Printer.DrawQuad(5, 8, 4, 4, "Trust Coupon Report", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            DateTime date = DateTime.Now;
            string workDate = date.Month.ToString("D2") + "/" + date.Day.ToString("D2") + "/" + (date.Year % 100).ToString("D2");

            string str = "Report : " + workDate;

            Printer.SetQuadSize(24, 12);
            font = new Font("Ariel", 9, FontStyle.Regular);
            Printer.DrawQuad(19, 8, 5, 4, str, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(16, 8, 3, 4, lblPayment.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(19, 8, 3, 4, lblTrust85.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(22, 8, 3, 4, lblTrust100.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /***********************************************************************************************/
    }
}