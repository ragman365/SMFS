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
using EMRControlLib;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.Utils.Drawing;
//using MinRowHeightXtraGrid;
using DevExpress.XtraGrid;
using MinRowHeightXtraGrid;
using DevExpress.XtraRichEdit.API.Native;
using DevExpress.XtraGrid.Views.BandedGrid;
using System.IO;
using DevExpress.XtraRichEdit;
using System.Drawing.Printing;
using DevExpress.Office.Utils;
using DevExpress.XtraReports.UI;
using DevExpress.XtraPrintingLinks;
using iTextSharp.text.pdf;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Threading;

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Text;
using System.Threading;
using static FileLockInfo.Win32Processes;
//using java.lang;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class Contract1 : DevExpress.XtraEditors.XtraForm
    {
        GridControl grid;

        public string myWorkContract = "";
        public static string workContractNumber = "";
        private string workContract = "";
        private string customerFile = "fcustomers";
        private DataTable workDt = null;
        private DataTable serviceDt = null;
        private DataTable paymentsDt = null;
        private bool gotPackage = false;
        private double PackageDiscount = 0D;
        private double PackagePrice = 0D;
        private double totalListedPrice = 0D;
        private string workName = "";
        private bool firstSummary = true;
        private int servicesLeftSide = 0;
        private int servicesRightSide = 0;
        public DataTable[] allDt = null;
        private Image[] Images = new Image[10];
        private int imageCount = 0;
        private bool workActuallyPrint = false;
        public static bool workJustViewing = false;
        public static DataTable signatureDt = null;
        private int printNumber = 0;
        //private int yOffset = 0;
        /****************************************************************************************/
        public Contract1(string contract, DataTable dt, DataTable payDt, bool separate = false, bool actuallyPrint = false, bool justViewing = false)
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {

            }
            workContract = contract;
            myWorkContract = contract;
            workContractNumber = contract;
            serviceDt = dt.Copy();
            CheckForPackageDiscount(serviceDt);
            paymentsDt = payDt.Copy();
            SetupTotalsSummary();
            if (separate)
                btnSeparate.Hide();
            workActuallyPrint = actuallyPrint;
            workJustViewing = justViewing;
        }
        /****************************************************************************************/
        private void CheckForPackageDiscount(DataTable dt)
        {
            PackagePrice = 0D;
            totalListedPrice = 0D;
            PackageDiscount = 0D;
            DataRow[] dR = dt.Select("service='Package Discount'");
            if (dR.Length <= 0)
                return;
            PackageDiscount = dR[0]["price"].ObjToDouble();

            dR = dt.Select("service='Package Price'");
            if (dR.Length > 0)
                PackagePrice = dR[0]["price"].ObjToDouble();

            dR = dt.Select("service='Total Listed Price'");
            if (dR.Length > 0)
                totalListedPrice = dR[0]["price"].ObjToDouble();
        }
        /****************************************************************************************/
        private void SetupTotalsSummary()
        {
            AddSummaryColumn("price", null);
            AddSummaryColumn("currentprice", null);
            AddSummaryColumn("difference", null);
        }
        /****************************************************************************************/
        private void AddSummaryColumn(string columnName, string format = "")
        {
            if (String.IsNullOrWhiteSpace(format))
                format = "${0:0,0.00}";
            gridMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            gridMain.Columns[columnName].SummaryItem.DisplayFormat = format;
        }
        /****************************************************************************************/
        private void LoadGeneralForm(string form, EMRControlLib.RichTextBoxEx rtb)
        {
            string cmd = "Select * from `arrangementforms` where `location` = 'General' AND `formName` = '" + form + "';";
            DataTable ddx = G1.get_db_data(cmd);
            if (ddx.Rows.Count <= 0)
                return;
            string record = ddx.Rows[0]["record"].ObjToString();
            string str = G1.get_db_blob("arrangementforms", record, "image");
            if (str.IndexOf("rtf1") > 0)
            {
                rtb.Rtf = str;
                //byte[] bytes = Encoding.ASCII.GetBytes(str);

                //MemoryStream stream = new MemoryStream(bytes);
                //rtb.LoadDocument(stream, DocumentFormat.Rtf);
            }
        }
        /****************************************************************************************/
        private int numSignatures = 0;
        private void Contract1_Load(object sender, EventArgs e)
        {
            //G1.WriteAudit("Generate GS Contract");

            dgv2.Visible = false;
            dgv2.Dock = DockStyle.Fill;
            dgv.Visible = true;
            dgv.Dock = DockStyle.Fill;
            this.panelAll.TabStop = false;
            this.panelAll.AutoScroll = false;
            this.panelAll.HorizontalScroll.Enabled = false;
            this.panelAll.HorizontalScroll.Visible = false;
            this.panelAll.HorizontalScroll.Maximum = 0;
            this.panelAll.AutoScroll = true;

            panelAll.VerticalScroll.Maximum = 0;

            if (DailyHistory.isInsurance(workContract))
                customerFile = "icustomers";

            string cmd = "Select * from `" + customerFile + "` where `contractNumber` = '" + workContract + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
            {
                cmd = "Select * from `customers` where `contractNumber` = '" + workContract + "';";
                dt = G1.get_db_data(cmd);
            }
            if (dt.Rows.Count <= 0)
                return;

            string fname = dt.Rows[0]["firstName"].ObjToString();
            string lname = dt.Rows[0]["lastName"].ObjToString();
            string address = dt.Rows[0]["address1"].ObjToString();
            string address2 = dt.Rows[0]["address2"].ObjToString();
            string city = dt.Rows[0]["city"].ObjToString();
            string state = dt.Rows[0]["state"].ObjToString();
            string zip = dt.Rows[0]["zip1"].ObjToString();

            EMRControlLib.RichTextBoxEx rtb = new EMRControlLib.RichTextBoxEx();
            LoadGeneralForm("FUNERALHOME", rtb);

            string filename = "";
            //string filename = @"C:\Users\Robby\Documents\SMFS\WordStuff\Cliff\FUNERALHOME.rtf";
            //rtb.LoadFile(filename);

            string activeFuneralHome = LoadFuneralHomeAddress(rtb);
            rtbAddress.AppendRtf(rtb.Rtf);

            filename = @"C:\Users\Robby\Documents\SMFS\WordStuff\Cliff\STATEMENT OF FUNERAL GOODS AND SERVICES SELECTED.rtf";
            EMRControlLib.RichTextBoxEx rtb2 = new EMRControlLib.RichTextBoxEx();
            LoadGeneralForm("STATEMENT OF FUNERAL GOODS AND SERVICES SELECTED", rtb2);
            //            rtb2.LoadFile(filename);
            string name = fname + " " + lname;

            RichEditControl rtbx = new RichEditControl();
            rtbx.Document.RtfText = rtb2.Rtf;
            RTF_Stuff.ProcessRTB(workContract, "", "STATEMENT OF FUNERAL GOODS AND SERVICES SELECTED", rtbx, true);
            rtb2.Rtf = rtbx.Document.RtfText;
            //            rtb2.Rtf = ArrangementForms.ReplaceField(rtb2.Rtf, "[%DEC%", name);

            //DataTable dx = Structures.ExtractFields(rtb2.Rtf);
            //DataTable ddt = Structures.LoadFields(dx, "", "STATEMENT OF FUNERAL GOODS AND SERVICES SELECTED");

            rtbStatementOfFuneral.AppendRtf(rtb2.Rtf);
            //            rtbStatementOfFuneral.LoadFile(filename);

            filename = @"C:\Users\Robby\Documents\SMFS\WordStuff\Cliff\DISCLAIMER OF WARRANTIES.rtf";
            LoadGeneralForm("DISCLAIMER OF WARRANTIES", rtbDisclaimer);
            //            rtbDisclaimer.LoadFile(filename);

            filename = @"C:\Users\Robby\Documents\SMFS\WordStuff\Cliff\ACKNOWLEDGEMENT AND AGREEMENT.rtf";
            LoadGeneralForm("ACKNOWLEDGEMENT AND AGREEMENT", rtback);
            //            rtback.LoadFile(filename);

            filename = @"C:\Users\Robby\Documents\SMFS\WordStuff\Cliff\DISCLOSURES.rtf";
            LoadGeneralForm("DISCLOSURES", rtbDisclosures);
            //            rtbDisclosures.LoadFile(filename);

            filename = @"C:\Users\Robby\Documents\SMFS\WordStuff\Cliff\SIGNATUREPAGE.rtf";
            LoadGeneralForm("SIGNATUREPAGE", rtbFinale);
            //            rtbFinale.LoadFile(filename);
            //rtbx.Document.RtfText = rtbFinale.Rtf;
            //RTF_Stuff.ProcessRTB(workContract, "", "SIGNATUREPAGE", rtbx, true);
            //rtbFinale.Rtf = rtbx.Document.RtfText;

            //enterSignatureOrPurchaserToolStripMenuItem_Click(null, null);


            //rtbFinale.Rtf = ArrangementForms.ReplaceField2(rtbFinale.Rtf, "[%SIG1%", "", signature );

            //rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%SIG2%", "");
            //rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%FUNSIG1%", "");
            //rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%FUNSIG2%", "");
            //rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "%FUNSIG1%", "");
            if (!String.IsNullOrWhiteSpace(activeFuneralHome))
                rtbFinale.Rtf = rtbFinale.Rtf.Replace("*FUNERALHOME*", activeFuneralHome);

            //LoadFuneralDirector();

            string fdName = getFuneralDirector();

            LoadServices();

            //LoadFamily();

            LoadFuneralDisclosures(rtbDisclosures);

            string tt = rtbDisclosures.Text;

            rtbx.Document.RtfText = rtbFinale.Rtf;

            LoadGeneralForm("SIGNATUREPAGE3", rtbFinale);
            rtbx.Document.RtfText = rtbFinale.Rtf;

            RTF_Stuff.ProcessRTB(workContract, "", "SIGNATUREPAGE3", rtbx);
            rtbFinale.Rtf = rtbx.Document.RtfText;

            //LoadFamily();

            bool generateRTF = false;

            Image fdSig = new Bitmap(1, 1);
            string who = "";

            numSignatures = 0;
            bool doingSignatures = false;
            bool allowSignatures = false;
            string preference = G1.getPreference(LoginForm.username, "GS Signatures", "Allow Access", false);
            if (preference == "YES")
                allowSignatures = true;
            if (workJustViewing)
                allowSignatures = false;

            if (allowSignatures)
            {
                DialogResult result = MessageBox.Show("Would you like to ADD Signatures?", "Add Signatures Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                if (result == DialogResult.Yes)
                {
                    using (ContractSignatures sigForm = new ContractSignatures(workContract, fdName, signatureDt))
                    {
                        if (sigForm.ShowDialog() == DialogResult.OK)
                        {
                            bool QuitAll = sigForm.ExitAll;
                            if (QuitAll)
                            {
                                this.Close();
                                return;
                            }
                            signatureDt = sigForm.SignatureResults;

                            if (signatureDt != null)
                            {
                                Image signature = null;
                                byte[] bytes = null;

                                for (int i = 0; i < signatureDt.Rows.Count; i++)
                                {
                                    Image signature2 = new Bitmap(1, 1);
                                    bytes = signatureDt.Rows[i]["signature"].ObjToBytes();
                                    if (bytes != null)
                                    {
                                        signature = G1.byteArrayToImage(bytes);
                                        if (signature != null)
                                        {
                                            doingSignatures = true;
                                            who = signatureDt.Rows[i]["relationType"].ObjToString();
                                            if (who.ToUpper() == "FD")
                                                fdSig = signature;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (signatureDt != null)
                    {
                        signatureDt.Rows.Clear();
                        signatureDt.Dispose();
                        signatureDt = null;
                    }
                    signatureDt = ContractSignatures.BuildSignatureTable(workContract, fdName, signatureDt);

                    preference = G1.getPreference(LoginForm.username, "GS Contracts", "Ask About RTF", false);
                    if (preference == "YES")
                    {
                        result = MessageBox.Show("Would you like to Generate an RTF Version?", "Generate RTF Version Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                        if (result == DialogResult.Yes)
                            generateRTF = true;
                    }
                }
            }
            else
            {
                if (signatureDt != null)
                {
                    signatureDt.Rows.Clear();
                    signatureDt.Dispose();
                    signatureDt = null;
                }
                signatureDt = ContractSignatures.BuildSignatureTable(workContract, fdName, signatureDt);
            }

            PleaseWait pleaseForm = G1.StartWait("Please Wait!\nGenerating G&&S Contract!");

            rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%SIG1%", "");
            rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%SIG2%", "");
            rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%FUNSIG5%", "");
            rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%TOD1%", "");
            rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%TOD2%", "");
            rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%FUNSIG2%", "");

            if (foundFirstPurchaser && !foundSecondPurchaser)
            {
                rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%ADDRESS2%", "");
                rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%CSZ2%", "");
                rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%PHONE2%", "");
            }
            else if (!foundFirstPurchaser)
            {
                rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%ADDRESS1%", "");
                rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%CSZ1%", "");
                rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%PHONE1%", "");
                rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%ADDRESS2%", "");
                rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%CSZ2%", "");
                rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%PHONE2%", "");
            }

            BuildSummary();

            dgv.Visible = false;
            dgv2.Visible = false;
            dgv3.Visible = true;

            DataTable dt3 = (DataTable)dgv3.DataSource;

            //ContractForm newForm = new ContractForm(allDt, rtbAddress, rtbStatementOfFuneral, dt3, rtbDisclaimer, rtbDisclosures, rtbFinale);
            //newForm.Show();

            //CreateReport3(dt3);

            cmd = "Select * from `relatives` where `contractNumber` = '" + workContract + "';";
            DataTable relativesDt = G1.get_db_data(cmd);
            relativesDt.Columns.Add("name");
            for (int i = 0; i < relativesDt.Rows.Count; i++)
            {
                name = GetFamilyName(relativesDt, i);
                relativesDt.Rows[i]["name"] = name;
            }


            string filePath = @"c:\SMFS_Other\" + LoginForm.username + "_" + workContract + "_";
            string mainPath = @"c:\SMFS_Other";
            string oldFile = "";
            string newFile = "";

            string relationType1 = "";
            string relationType2 = "";

            DataRow[] famDR = null;
            CompositeLink composLink = new CompositeLink(new PrintingSystem());

            int pageCount = 0;

            if (this.components == null)
                this.components = new System.ComponentModel.Container();

            DevExpress.XtraPrinting.PrintingSystem printingSystem1 = new DevExpress.XtraPrinting.PrintingSystem(this.components);
            printingSystem1.Document.AutoFitToPagesWidth = 1;

            composLink.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);

            //if (!doingSignatures || signatureDt.Rows.Count <= 0 )

            string doMultiPage = G1.getPreference(LoginForm.username, "MultiPage GS Contracts", "Allow", false);

            if (signatureDt.Rows.Count <= 1 || workActuallyPrint || workJustViewing || generateRTF)
            {
                LoadGeneralForm("SIGNATUREPAGE3", rtbFinale);
                rtbx.Document.RtfText = rtbFinale.Rtf;

                RTF_Stuff.ProcessRTB(workContract, "", "SIGNATUREPAGE3", rtbx);
                rtbFinale.Rtf = rtbx.Document.RtfText;

                if (!String.IsNullOrWhiteSpace(activeFuneralHome))
                    rtbFinale.Rtf = rtbFinale.Rtf.Replace("*FUNERALHOME*", activeFuneralHome);

                //if (!String.IsNullOrWhiteSpace(activeFuneralHome))
                //    rtbFinale.Rtf = rtbFinale.Rtf.Replace("*FUNERALHOME*", activeFuneralHome);

                LoadFuneralDirector();

                rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%Page%", "Page 1 of 1 ");
                if (signatureDt.Rows.Count == 1 && !String.IsNullOrWhiteSpace(fdName))
                {
                    Image signature4 = new Bitmap(1, 1);
                    Image image = MergeImages(fdSig, signature4, 1);
                    if (doingSignatures && image.Width > 3)
                        rtbFinale.Rtf = ArrangementForms.ReplaceField2(rtbFinale.Rtf, "[%FUNSIG5%", "", image, 925);
                }
                else if (signatureDt.Rows.Count == 1)
                {
                    name = signatureDt.Rows[0]["name"].ObjToString();
                    relationType1 = signatureDt.Rows[0]["relationType"].ToString();
                    if (relationType1.ToUpper() == "NOK")
                        relationType1 += "       ";
                    relationType1 = "Signature of " + relationType1;
                    rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%Signature of Purchaser%", relationType1);

                    Image signature1 = new Bitmap(1, 1);
                    Image signature2 = new Bitmap(1, 1);

                    byte[] bytes = null;
                    bytes = signatureDt.Rows[0]["signature"].ObjToBytes();
                    if (bytes != null)
                    {
                        signature1 = G1.byteArrayToImage(bytes);
                        numSignatures = 1;
                    }

                    if (signature1.Width <= 3)
                        signature1 = new Bitmap(365, 1);

                    Image image = MergeImages(signature1, signature2, 900);
                    if (doingSignatures)
                        rtbFinale.Rtf = ArrangementForms.ReplaceField2(rtbFinale.Rtf, "[%SIG1%", "", image, 0);
                }
                rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%SIG1%", "");
                rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%SIG2%", "");
                rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%FUNSIG5%", "");
                rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%TOD1%", "");
                rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%TOD2%", "");
                rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%FUNSIG2%", "");
                rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%Signature of Co-Purchaser%", "Signature of Co-Purchaser   ");
                rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%Signature of Purchaser%", "Signature of Purchaser   ");

                rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%ADDRESS2%", "");
                rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%CSZ2%", "");
                rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%PHONE2%", "");

                rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%ADDRESS1%", "");
                rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%CSZ1%", "");
                rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%PHONE1%", "");
                rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%ADDRESS2%", "");
                rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%CSZ2%", "");
                rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%PHONE2%", "");

                if (generateRTF)
                {
                    PrintableComponentLink p2 = printPreview(sender, e);
                    p2.PrintingSystemBase = printingSystem1;
                    printingSystem1.Links.Add(p2);
                    p2.CreateDocument();
                    p2.ShowPreviewDialog();
                    G1.StopWait(ref pleaseForm);
                    this.Close();
                    return;
                }

                PrintableComponentLink p1 = printPreview(sender, e);
                p1.PrintingSystemBase = printingSystem1;
                printingSystem1.Links.Add(p1);
                p1.CreateDocument();

                if (workActuallyPrint)
                {
                    p1.Print();
                    G1.StopWait(ref pleaseForm);
                    this.Close();
                    return;
                }
                else
                {
                    filename = filePath + "_" + pageCount.ToString() + "_GS.pdf";
                    //G1.WriteAudit("Generate GS Export to PDF");
                    p1.ExportToPdf(filename);
                    newFile = filename;
                }
            }
            else
            {
                int sigCount = 0;
                Image signature1 = new Bitmap(1, 1);
                Image signature2 = new Bitmap(1, 1);
                byte[] bytes = null;

                if (this.components == null)
                    this.components = new System.ComponentModel.Container();

                //DevExpress.XtraPrinting.PrintingSystem printingSystem1 = new DevExpress.XtraPrinting.PrintingSystem(this.components);

                int totalPages = signatureDt.Rows.Count / 2;
                if (totalPages <= 0)
                    totalPages = 1;

                int fdIndx = -1;
                for (int i = 0; i < signatureDt.Rows.Count; i++)
                {
                    who = signatureDt.Rows[i]["relationType"].ObjToString();
                    if (who.ToUpper() == "FD")
                    {
                        fdIndx = i;
                        break;
                    }
                }

                int page = 0;
                for (; ; )
                {
                    numSignatures = 0;
                    if (sigCount >= signatureDt.Rows.Count)
                        break;

                    LoadGeneralForm("SIGNATUREPAGE3", rtbFinale);
                    rtbx.Document.RtfText = rtbFinale.Rtf;

                    RTF_Stuff.ProcessRTB(workContract, "", "SIGNATUREPAGE3", rtbx);
                    rtbFinale.Rtf = rtbx.Document.RtfText;

                    page++;

                    rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%Page%", "Page " + page.ToString() + " of " + totalPages.ToString());

                    if (!String.IsNullOrWhiteSpace(activeFuneralHome))
                        rtbFinale.Rtf = rtbFinale.Rtf.Replace("*FUNERALHOME*", activeFuneralHome);

                    LoadFuneralDirector();

                    Image signature4 = new Bitmap(1, 1);
                    Image image = MergeImages(fdSig, signature4, 1);
                    if (doingSignatures)
                        rtbFinale.Rtf = ArrangementForms.ReplaceField2(rtbFinale.Rtf, "[%FUNSIG5%", "", image, 925);

                    signature1 = new Bitmap(1, 1);
                    bytes = signatureDt.Rows[sigCount]["signature"].ObjToBytes();
                    if (bytes != null)
                    {
                        signature1 = G1.byteArrayToImage(bytes);
                        numSignatures = 1;
                    }

                    who = signatureDt.Rows[sigCount]["relationType"].ObjToString();
                    if (who.ToUpper() == "FD")
                        sigCount++;

                    name = signatureDt.Rows[sigCount]["name"].ObjToString();
                    relationType1 = signatureDt.Rows[sigCount]["relationType"].ToString();
                    if (relationType1.ToUpper() == "NOK")
                        relationType1 += "       ";
                    relationType1 = "Signature of " + relationType1;

                    LoadSignatureDetails(relativesDt, name, 1);

                    sigCount++;
                    signature2 = new Bitmap(1, 1);

                    if (sigCount < signatureDt.Rows.Count)
                    {
                        who = signatureDt.Rows[sigCount]["relationType"].ObjToString();
                        if (who.ToUpper() == "FD")
                            sigCount++;

                        if (sigCount < signatureDt.Rows.Count)
                        {
                            bytes = signatureDt.Rows[sigCount]["signature"].ObjToBytes();
                            if (bytes != null)
                            {
                                signature2 = G1.byteArrayToImage(bytes);
                                numSignatures = 1;
                            }

                            name = signatureDt.Rows[sigCount]["name"].ObjToString();
                            relationType2 = signatureDt.Rows[sigCount]["relationType"].ToString();
                            relationType2 = "Signature of " + relationType2;
                            rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%Signature of Co-Purchaser%", relationType2);

                            LoadSignatureDetails(relativesDt, name, 2);
                        }
                    }

                    rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%Signature of Purchaser%", relationType1);

                    if (signature1.Width <= 3)
                        signature1 = new Bitmap(365, 1);

                    image = MergeImages(signature1, signature2, 900);
                    if (doingSignatures)
                        rtbFinale.Rtf = ArrangementForms.ReplaceField2(rtbFinale.Rtf, "[%SIG1%", "", image, 0);

                    //if (!String.IsNullOrWhiteSpace(activeFuneralHome) )
                    //    rtbFinale.Rtf = rtbFinale.Rtf.Replace("*FUNERALHOME*", activeFuneralHome);

                    //LoadFuneralDirector();

                    //Image signature4 = new Bitmap(1, 1);
                    //image = MergeImages(fdSig, signature4, 1);
                    //rtbFinale.Rtf = ArrangementForms.ReplaceField2(rtbFinale.Rtf, "[%FUNSIG5%", "", image, 945);


                    rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%SIG1%", "");
                    rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%SIG2%", "");
                    rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%FUNSIG5%", "");
                    rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%TOD1%", "");
                    rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%TOD2%", "");
                    rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%FUNSIG2%", "");
                    rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%Signature of Co-Purchaser%", "Signature of Co-Purchaser   ");

                    //if (foundFirstPurchaser && !foundSecondPurchaser)
                    //{
                    rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%ADDRESS2%", "");
                    rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%CSZ2%", "");
                    rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%PHONE2%", "");
                    //}
                    //else if (!foundFirstPurchaser)
                    //{
                    rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%ADDRESS1%", "");
                    rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%CSZ1%", "");
                    rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%PHONE1%", "");
                    rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%ADDRESS2%", "");
                    rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%CSZ2%", "");
                    rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%PHONE2%", "");
                    //}

                    PrintableComponentLink p1 = printPreview(sender, e);
                    p1.PrintingSystemBase = printingSystem1;
                    printingSystem1.Links.Add(p1);
                    p1.CreateDocument();

                    filename = filePath + "_" + pageCount.ToString() + "_GS.pdf";

                    //G1.WriteAudit("Generate GS Export to PDF 1");
                    p1.ExportToPdf(filename);

                    newFile = filename;
                    printableComponentLinks[pageCount] = p1;
                    //p1.ShowPreviewDialog();

                    try
                    {
                        composLink.Links.Add(p1);
                    }
                    catch (Exception ex)
                    {
                    }
                    //p1.Dispose();
                    //p1 = null;

                    pageCount++;

                    sigCount++;
                    if (sigCount >= signatureDt.Rows.Count)
                        break;
                    if (String.IsNullOrWhiteSpace(doMultiPage))
                        break;
                    //if (1 == 1)
                    //    break;
                }
            }

            if (pageCount > 0)
            {
                string outFile = "";
                filename = filename = filePath + "_" + "0_GS.pdf";
                newFile = filename;
                for (int i = 1; i < pageCount; i++)
                {
                    oldFile = filePath + "_" + i.ToString() + "_GS.pdf";
                    outFile = filePath + "_" + "X_GS.pdf";
                    if (File.Exists(outFile))
                        File.Delete(outFile);
                    //G1.WriteAudit("Generate GS Merge PDF");
                    outFile = MergePDF(newFile, oldFile, outFile);
                    if (File.Exists(newFile))
                        File.Delete(newFile);

                    File.Move(outFile, newFile);

                    GrantFileAccess(newFile);

                    FileAttributes attributes = File.GetAttributes(newFile);
                    if ((attributes & FileAttributes.Archive) == FileAttributes.Archive)
                    {
                        // Show the file.
                        attributes = RemoveAttribute(attributes, FileAttributes.Archive);
                        File.SetAttributes(newFile, attributes);
                        //Console.WriteLine("The {0} file is no longer hidden.", path);
                    }

                    //File.Copy(outFile, newFile, true);

                    //if (File.Exists(outFile))
                    //    File.Delete(outFile);
                }
            }
            else
            {
                //string outFile = "";
                //filename = filename = filePath + "_" + "0_GS.pdf";
                //newFile = filename;
                //oldFile = filePath + "_" + "0_GS.pdf";
                //outFile = filePath + "_" + "X_GS.pdf";
                //if (File.Exists(outFile))
                //    File.Delete(outFile);
                //outFile = MergePDF(newFile, oldFile, outFile);
                //if (File.Exists(newFile))
                //    File.Delete(newFile);

                //File.Move(outFile, newFile);

                //GrantFileAccess(newFile);

                //FileAttributes attributes = File.GetAttributes(newFile);
                //if ((attributes & FileAttributes.Archive) == FileAttributes.Archive)
                //{
                //    // Show the file.
                //    attributes = RemoveAttribute(attributes, FileAttributes.Archive);
                //    File.SetAttributes(newFile, attributes);
                //    //Console.WriteLine("The {0} file is no longer hidden.", path);
                //}

            }

            G1.StopWait(ref pleaseForm);

            File.SetAttributes(newFile, FileAttributes.Normal);

            //G1.WriteAudit("Generate GS ViewPDF");
            using (ViewPDF myView = new ViewPDF("GS Contract", newFile, true, workContract, doingSignatures, false))
            {
                myView.BringToFront();
                myView.TopLevel = true;
                myView.GSDone += MyView_GSDone;
                this.Hide();
                myView.ShowDialog();
            }


            //string filePath = @"c:\SMFS_Other\" + LoginForm.username + "_" + workContract + "_";

            string directory = @"c:\SMFS_Other";
            string wildCard = LoginForm.username + "_" + workContract + "_*";
            string[] files = Directory.GetFiles(directory, wildCard).Select(path => Path.GetFileName(path)).ToArray();

            Process p = Process.GetCurrentProcess();
            int processID = p.Id;

            foreach (string file in files)
            {
                filename = directory + "/" + file;
                if (File.Exists(filename))
                {
                    try
                    {
                        GrantFileAccess(filename);
                        File.SetAttributes(filename, FileAttributes.Normal);
                        GC.Collect();
                        GC.WaitForPendingFinalizers();

                        File.Delete(filename);
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }

            majorStartY = 0;
            majorHeaderY = 0;
            Printer.setupPrinterMargins(50, 50, 195, 0);

            pageMarginLeft = Printer.pageMarginLeft;
            pageMarginRight = Printer.pageMarginRight;
            pageMarginTop = Printer.pageMarginTop;
            pageMarginBottom = Printer.pageMarginBottom;

            composLink.Margins.Left = pageMarginLeft;
            composLink.Margins.Right = pageMarginRight;
            composLink.Margins.Top = pageMarginTop;
            composLink.Margins.Bottom = pageMarginBottom;

            composLink.Landscape = false;
            //composLink.ShowPreviewDialog();
            //composLink.ShowPreview();
            //composLink.ShowRibbonPreview(DevExpress.LookAndFeel.UserLookAndFeel.Default);
            //composLink.ShowRibbonPreviewDialog(DevExpress.LookAndFeel.UserLookAndFeel.Default);

            this.Close();
        }
        /***********************************************************************************************/
        private static FileAttributes RemoveAttribute(FileAttributes attributes, FileAttributes attributesToRemove)
        {
            return attributes & ~attributesToRemove;
        }
        /***********************************************************************************************/
        private void DeleteFile(string fileName)
        {
            //using (File.Create(@"c:\Temp\txt.txt")) ; // File.Create wrapped in a using() to ensure disposing the stream.

            try
            {
                File.Copy(fileName, fileName + "2.pdf");
                File.Delete(fileName);
                File.Delete(fileName + "2.txt");
            }
            catch (Exception ex)
            {
            }
        }
        /***********************************************************************************************/
        private void CheckArranger ()
        {
            if (String.IsNullOrWhiteSpace(workContract))
                return;
            string cmd = "Select * from `fcust_extended` WHERE `contractNumber` = '" + workContract + "';";
            DataTable dx = G1.get_db_data(cmd);
            if ( dx.Rows.Count > 0 )
            {
                string arranger = dx.Rows[0]["Funeral Arranger"].ObjToString();
                string serviceId = dx.Rows[0]["serviceId"].ObjToString();
                if ( String.IsNullOrWhiteSpace ( arranger ))
                    Messages.SendTheMessage(LoginForm.username, "cjenkins", "Blank Arranger for Service Id " + serviceId, "Funeral Service Id " + serviceId);
            }
        }
        /***********************************************************************************************/
        private void MyView_GSDone(bool printed)
        {
            if (printed)
                CheckArranger();
            if (GSDone != null)
                GSDone.Invoke(printed);
        }
        /***********************************************************************************************/
        private static string MergePDF(string File1, string File2, string outputPdfPath)
        {
            string[] fileArray = new string[3];
            fileArray[0] = File1;
            fileArray[1] = File2;

            PdfReader reader = null;
            iTextSharp.text.Document sourceDocument = null;
            PdfCopy pdfCopyProvider = null;
            PdfImportedPage importedPage;
            string inputPdfPath = File1;
            //string outputPdfPath = @"C:/Users/robby/downloads/pdfX.pdf";
            //GrantAccess(@"C:/Users/robby/downloads");
            //GrantAccess(@"C:/SMFS_Other/robby/downloads");

            if (File.Exists(outputPdfPath))
            {
                GrantFileAccess(outputPdfPath);
                File.Delete(outputPdfPath);
            }

            //try
            //{
            //    File.Copy(inputPdfPath, outputPdfPath);
            //}
            //catch ( Exception ex)
            //{
            //}


            sourceDocument = new iTextSharp.text.Document();
            pdfCopyProvider = new PdfCopy(sourceDocument, new System.IO.FileStream(outputPdfPath, System.IO.FileMode.Create));

            //output file Open  
            sourceDocument.Open();


            //files list wise Loop  
            for (int f = 0; f < fileArray.Length - 1; f++)
            {
                int pages = TotalPageCount(fileArray[f]);

                reader = new PdfReader(fileArray[f]);
                //Add pages in new file  
                for (int i = 1; i <= pages; i++)
                {
                    importedPage = pdfCopyProvider.GetImportedPage(reader, i);
                    pdfCopyProvider.AddPage(importedPage);
                }

                reader.Close();
                reader.Dispose();
                reader = null;
            }
            //save the output file  
            sourceDocument.Close();
            sourceDocument.Dispose();
            sourceDocument = null;

            pdfCopyProvider.Close();
            pdfCopyProvider.Dispose();
            pdfCopyProvider = null;

            //ViewPDF myView = new ViewPDF("TEST", outputPdfPath);
            //myView.ShowDialog();

            //if (File.Exists(outputPdfPath))
            //    File.Delete(outputPdfPath);

            return outputPdfPath;
        }
        /***********************************************************************************************/
        private static int TotalPageCount(string file)
        {
            if (File.Exists(file))
            {
                using (StreamReader sr = new StreamReader(System.IO.File.OpenRead(file)))
                {
                    Regex regex = new Regex(@"/Type\s*/Page[^s]");
                    MatchCollection matches = regex.Matches(sr.ReadToEnd());

                    return matches.Count;
                }
            }
            else
                return 0;
        }
        /***********************************************************************************************/
        private static void GrantFileAccess(string file)
        {
            try
            {
                DirectoryInfo dInfo = new DirectoryInfo(file);
                DirectorySecurity dSecurity = dInfo.GetAccessControl();
                dSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
                dInfo.SetAccessControl(dSecurity);
            }
            catch (Exception ex)
            {
            }
        }
        /***********************************************************************************************/
        private static void GrantAccess(string file)
        {
            bool exists = System.IO.Directory.Exists(file);
            if (!exists)
            {
                DirectoryInfo di = System.IO.Directory.CreateDirectory(file);
                //Console.WriteLine("The Folder is created Sucessfully");
            }
            else
            {
                //Console.WriteLine("The Folder already exists");
            }
            DirectoryInfo dInfo = new DirectoryInfo(file);
            DirectorySecurity dSecurity = dInfo.GetAccessControl();
            dSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
            dInfo.SetAccessControl(dSecurity);

        }
        /****************************************************************************************/
        private string GetFamilyName(DataTable dt, int i)
        {
            string prefix = dt.Rows[i]["depPrefix"].ObjToString();
            string firstName = dt.Rows[i]["depFirstName"].ObjToString();
            string middleName = dt.Rows[i]["depMI"].ObjToString();
            string lastName = dt.Rows[i]["depLastName"].ObjToString();
            string suffix = dt.Rows[i]["depSuffix"].ObjToString();
            string name = G1.BuildFullName(prefix, firstName, middleName, lastName, suffix);
            return name;
        }
        /***********************************************************************************************/
        private Image MergeImages(Image image1, Image image2, int space)
        {
            Bitmap bitmap = new Bitmap(image1.Width + image2.Width + space, Math.Max(image1.Height, image2.Height));
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.White);
                g.DrawImage(image1, 0, 0);
                g.DrawImage(image2, image1.Width + space, 0);
            }
            Image img = bitmap;
            return img;
        }
        /***********************************************************************************************/
        private string getFuneralDirector()
        {
            string cmd = "Select * from `fcust_extended` where `contractNumber` = '" + workContract + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return "";

            string licenseNumber = dt.Rows[0]["Funeral Director License"].ObjToString().Trim();
            string name = dt.Rows[0]["Funeral Director"].ObjToString().Trim();
            return name;
        }
        /***********************************************************************************************/
        private string LoadFuneralDirector()
        {
            //string agentCode = LoginForm.activeFuneralHomeAgent;
            //if (String.IsNullOrWhiteSpace(agentCode))
            //{
            //    rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%FUNDIR%", "");
            //    rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%FUNSIG1%", "");
            //    rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%FUNSIG2%", "");
            //    rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%LICE%", "");
            //    return;
            //}
            //string cmd = "Select * from `agents` where `agentCode` = '" + agentCode + "';";
            //DataTable dt = G1.get_db_data(cmd);
            //if ( dt.Rows.Count <= 0)
            //{
            //    rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%FUNDIR%", "");
            //    return;
            //}
            string cmd = "Select * from `fcust_extended` where `contractNumber` = '" + workContract + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
            {
                rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%FUNDIR%", "");
                rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%FUNSIG1%", "");
                rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%FUNSIG2%", "");
                rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%LICE%", "");
                return "";
            }

            //string licenseNumber = dt.Rows[0]["licenseNumber"].ObjToString();
            //string firstName = dt.Rows[0]["firstName"].ObjToString();
            //string lastName = dt.Rows[0]["lastName"].ObjToString();
            //string name = firstName + " " + lastName;

            string licenseNumber = dt.Rows[0]["Funeral Director License"].ObjToString().Trim();
            string name = dt.Rows[0]["Funeral Director"].ObjToString().Trim();
            string fdName = name;
            if (!String.IsNullOrWhiteSpace(name))
            {
                string[] Lines = name.Split('[');
                if (Lines.Length >= 1)
                {
                    name = Lines[0].Trim();
                    if (Lines.Length >= 2)
                    {
                        licenseNumber = Lines[1].Trim();
                        licenseNumber = licenseNumber.Replace("[", "");
                        licenseNumber = licenseNumber.Replace("]", "");
                    }
                    else
                        licenseNumber = "";
                }
            }

            rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%LICE%", licenseNumber);
            rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%FUNDIR%", name);
            //rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%FUNSIG2%", "");
            string tt = rtbFinale.Text;
            int start = rtbFinale.Text.IndexOf("%FUNSIG1%");
            rtbFinale.Rtf = rtbFinale.Rtf.Replace("%FUNSIG1%", "");
            //            rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%FUNSIG1%", "");
            //if (LoginForm.activeFuneralHomeSignature != null)
            //{
            //    Image myImage = G1.byteArrayToImage(LoginForm.activeFuneralHomeSignature);
            //    if (start >= 0)
            //    {
            //        myImage = ScaleImage(myImage, 0.15f, 0.15F);
            //        rtbFinale.SelectionStart = start;
            //        rtbFinale.ScrollToCaret();
            //        rtbFinale.InsertImage(myImage);
            //    }
            //}
            return fdName;
        }
        /***********************************************************************************************/
        public static Image ScaleImage(Image image, float xFactor, float yFactor)
        {
            Image myThumbnail = image;
            if (image == null)
                return null;
            try
            {
                Bitmap b = new Bitmap(image);
                int height = b.Height;
                int width = b.Width;
                height = (int)(((float)(height)) * yFactor);
                width = (int)(((float)(width)) * xFactor);
                myThumbnail = b.GetThumbnailImage(width, height, null, IntPtr.Zero);
            }
            catch (Exception ex)
            {
            }
            return myThumbnail;
        }
        /***********************************************************************************************/
        private bool foundFirstPurchaser = false;
        private bool foundSecondPurchaser = false;
        private void LoadFamily()
        {
            string cmd = "Select * from `relatives` where `contractNumber` = '" + workContract + "';";
            DataTable dt = G1.get_db_data(cmd);
            DateTime lastSigTime = DateTime.Now;
            bool gotPreviousContract = CheckLastContract();
            LoadSignatures(dt, gotPreviousContract);

            //enterSignatureOrPurchaserToolStripMenuItem_Click(null, null);
        }
        /***********************************************************************************************/
        private bool CheckLastContract()
        {
            string cmd = "Select * from `lapse_list` where `contractNumber` = '" + workContract + "' AND `detail` = 'Goods and Services';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return false;
            return true;
        }
        /***********************************************************************************************/
        public static bool noSigsFound = true;
        private void LoadSignatures(DataTable dt, bool gotPreviousContract)
        {
            noSigsFound = true;
            string text = "";
            string address = "";
            string city = "";
            string state = "";
            string zip = "";
            string phone = "";
            string phoneType = "";
            DateTime date = DateTime.Now;
            DateTime sigTime = DateTime.Now;
            int count = 1;
            Bitmap emptyImage = new Bitmap(1, 1);
            bool purchaser = false;

            G1.sortTable(dt, "LegOrder", "ASC");


            for (int i = 0; i < dt.Rows.Count; i++)
            {
                sigTime = dt.Rows[i]["sigTime"].ObjToDateTime();
                if (sigTime.Year > 100)
                    noSigsFound = false;
                text = dt.Rows[i]["purchaser"].ObjToString();
                purchaser = false;
                if (!String.IsNullOrWhiteSpace(text))
                {
                    if (text.Trim().ToUpper() == "1")
                        purchaser = true;
                }
                //purchaser = true;
                gotPreviousContract = true;
                if (purchaser)
                {
                    if (count == 1)
                    {
                        if (gotPreviousContract)
                        {
                            sigTime = dt.Rows[i]["sigTime"].ObjToDateTime();
                            if (sigTime.Year > 100 || 1 == 1)
                            {
                                //enterSignatureOrPurchaserToolStripMenuItem_Click(null, null);

                                Byte[] bytes = dt.Rows[i]["signature"].ObjToBytes();
                                Image myImage = emptyImage;
                                if (bytes != null)
                                {
                                    myImage = G1.byteArrayToImage(bytes);
                                    //myImage = ScaleImage(myImage, .5F, .5F);
                                }

                                //AddSignature(myImage, rtbSig1);
                                date = dt.Rows[i]["signatureDate"].ObjToDateTime();
                                //date = DateTime.Now;
                                if (date.Year > 1900)
                                    rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%TOD1%", date.ToString("MM/dd/yyyy"));
                                address = dt.Rows[i]["address"].ObjToString();
                                //                        rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%ADDRESS1%", address);
                                city = dt.Rows[i]["city"].ObjToString();
                                state = dt.Rows[i]["state"].ObjToString();
                                zip = dt.Rows[i]["zip"].ObjToString();
                                city += " ," + state + "  " + zip;
                                rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%CSZ1%", city);
                                rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%ADDRESS1%", address);
                                phone = dt.Rows[i]["phone"].ObjToString();
                                phoneType = dt.Rows[i]["phoneType"].ObjToString();
                                if (!String.IsNullOrWhiteSpace(phoneType))
                                    phone += "(" + phoneType + ")";
                                rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%PHONE1%", phone);
                                count++;
                                foundFirstPurchaser = true;
                            }
                        }
                    }
                    else if (count == 2)
                    {
                        if (gotPreviousContract)
                        {
                            sigTime = dt.Rows[i]["sigTime"].ObjToDateTime();
                            if (sigTime.Year > 100 || 1 == 1)
                            {
                                Byte[] bytes = dt.Rows[i]["signature"].ObjToBytes();
                                //if (bytes == null)
                                //    continue;
                                Image myImage = emptyImage;
                                if (bytes != null)
                                    myImage = G1.byteArrayToImage(bytes);

                                //AddSignature(myImage, rtbSig2);
                                date = dt.Rows[i]["signatureDate"].ObjToDateTime();
                                date = DateTime.Now;
                                if (date.Year > 1900)
                                    rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%TOD2%", date.ToString("MM/dd/yyyy"));
                                address = dt.Rows[i]["address"].ObjToString();
                                rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%ADDRESS2%", address);
                                city = dt.Rows[i]["city"].ObjToString();
                                state = dt.Rows[i]["state"].ObjToString();
                                zip = dt.Rows[i]["zip"].ObjToString();
                                city += " ," + state + "  " + zip;
                                rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%CSZ2%", city);
                                phone = dt.Rows[i]["phone"].ObjToString();
                                phoneType = dt.Rows[i]["phoneType"].ObjToString();
                                if (!String.IsNullOrWhiteSpace(phoneType))
                                    phone += "(" + phoneType + ")";
                                rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%PHONE2%", phone);
                                foundSecondPurchaser = true;
                                count++;
                            }
                        }
                    }
                    else if (count == 3)
                    {
                        if (gotPreviousContract)
                        {
                            sigTime = dt.Rows[i]["sigTime"].ObjToDateTime();
                            if (sigTime.Year > 100 || 1 == 1)
                            {
                                Byte[] bytes = dt.Rows[i]["signature"].ObjToBytes();
                                if (bytes != null)
                                {
                                    Image myImage = emptyImage;
                                    myImage = G1.byteArrayToImage(bytes);
                                    if (imageCount <= 9)
                                    {
                                        myImage = ScaleImage(myImage, 0.30F, 0.30F);
                                        Images[imageCount] = myImage;
                                        imageCount++;
                                    }
                                }
                            }
                        }

                        //AddSignature(myImage, rtbSig2);
                        //date = dt.Rows[i]["signatureDate"].ObjToDateTime();
                        //if (date.Year > 1900)
                        //    rtback.Rtf = ArrangementForms.ReplaceField(rtback.Rtf, "[%TOD3%", date.ToString("MM/dd/yyyy"));
                        //foundSecondPurchaser = true;
                    }
                    //Byte[] bytes = dt.Rows[i]["signature"].ObjToBytes();
                    //Image myImage = emptyImage;
                    //if (bytes != null)
                    //{
                    //    count++;
                    //    myImage = G1.byteArrayToImage(bytes);
                    //    if (count == 1)
                    //    {
                    //        //AddSignature(myImage, rtbSig1);
                    //        date = dt.Rows[i]["signatureDate"].ObjToDateTime();
                    //        if ( date.Year > 1900)
                    //            rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%TOD1%", date.ToString("MM/dd/yyyy"));
                    //        address = dt.Rows[i]["address"].ObjToString();
                    //        rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%ADDRESS1%", address);
                    //        city = dt.Rows[i]["city"].ObjToString();
                    //        state = dt.Rows[i]["state"].ObjToString();
                    //        zip = dt.Rows[i]["zip"].ObjToString();
                    //        city += " ," + state + "  " + zip;
                    //        rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%CSZ1%", city);
                    //        phone = dt.Rows[i]["phone"].ObjToString();
                    //        rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%PHONE1%", phone);
                    //    }
                    //    else if (count == 2)
                    //    {
                    //        //AddSignature(myImage, rtbSig2);
                    //        date = dt.Rows[i]["signatureDate"].ObjToDateTime();
                    //        if (date.Year > 1900)
                    //            rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%TOD2%", date.ToString("MM/dd/yyyy"));
                    //        address = dt.Rows[i]["address"].ObjToString();
                    //        rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%ADDRESS2%", address);
                    //        city = dt.Rows[i]["city"].ObjToString();
                    //        state = dt.Rows[i]["state"].ObjToString();
                    //        zip = dt.Rows[i]["zip"].ObjToString();
                    //        city += " ," + state + "  " + zip;
                    //        rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%CSZ2%", city);
                    //        phone = dt.Rows[i]["phone"].ObjToString();
                    //        rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%PHONE2%", phone);
                    //        break;
                    //    }
                    //}
                }
            }
        }
        /***********************************************************************************************/
        private void LoadSignatureDetails(DataTable dx, string name, int side)
        {
            string address = "";
            string city = "";
            string state = "";
            string zip = "";
            string phone = "";
            string phoneType = "";
            DateTime date = DateTime.Now;

            DataRow[] dRows = dx.Select("name='" + name + "'");
            if (dRows.Length <= 0)
                return;
            DataTable dt = dRows.CopyToDataTable();

            int row = 0;
            bool isTest = false;
            name = dt.Rows[row]["name"].ObjToString();
            string relationship = dt.Rows[row]["depRelationship"].ObjToString();

            if (side == 1)
            {
                rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%TOD1%", date.ToString("MM/dd/yyyy"));
                address = dt.Rows[row]["address"].ObjToString();
                if ( isTest )
                    address = name;
                city = dt.Rows[row]["city"].ObjToString();
                if (isTest)
                    city = relationship;
                 state = dt.Rows[row]["state"].ObjToString();
                zip = dt.Rows[row]["zip"].ObjToString();
                city += " ," + state + "  " + zip;
                rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%CSZ1%", city);
                rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%ADDRESS1%", address);
                phone = dt.Rows[row]["phone"].ObjToString();
                phoneType = dt.Rows[row]["phoneType"].ObjToString();
                if (!String.IsNullOrWhiteSpace(phoneType))
                    phone += "(" + phoneType + ")";
                rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%PHONE1%", phone);
            }
            else
            {
                date = DateTime.Now;
                rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%TOD2%", date.ToString("MM/dd/yyyy"));
                address = dt.Rows[row]["address"].ObjToString();
                if ( isTest)
                    address = name;
                rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%ADDRESS2%", address);
                city = dt.Rows[row]["city"].ObjToString();
                if (isTest)
                    city = relationship;
                state = dt.Rows[row]["state"].ObjToString();
                zip = dt.Rows[row]["zip"].ObjToString();
                city += " ," + state + "  " + zip;
                rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%CSZ2%", city);
                phone = dt.Rows[row]["phone"].ObjToString();
                phoneType = dt.Rows[row]["phoneType"].ObjToString();
                if (!String.IsNullOrWhiteSpace(phoneType))
                    phone += "(" + phoneType + ")";
                rtbFinale.Rtf = ArrangementForms.ReplaceField(rtbFinale.Rtf, "[%PHONE2%", phone);
            }
        }
        /***********************************************************************************************/
        //private Image ScaleImage(Image image, float xFactor, float yFactor)
        //{
        //    if (image == null)
        //        return null;
        //    Bitmap b = new Bitmap(image);
        //    int height = b.Height;
        //    int width = b.Width;
        //    height = (int)(((float)(height)) * yFactor);
        //    width = (int)(((float)(width)) * xFactor);
        //    Image myThumbnail = b.GetThumbnailImage(width, height, null, IntPtr.Zero);
        //    return myThumbnail;
        //}
        /***********************************************************************************************/
        public static void LoadFuneralDisclosures(EMRControlLib.RichTextBoxEx rtb)
        {
            string cmd = "Select * from `relatives` where `depRelationship` = 'Disclosures' AND `contractNumber` = '" + workContractNumber + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                string str = "";
                string what = "";
                string select = "";
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    what = dt.Rows[i]["depFirstName"].ObjToString();
                    str = dt.Rows[i]["depLastName"].ObjToString();
                    select = dt.Rows[i]["nextOfKin"].ObjToString();
                    if (select != "1")
                        str = "";
                    rtb.Rtf = rtb.Rtf.Replace("%" + what + "%", str);
                }
            }
            else
            {
                string what = "";
                string str = "";
                cmd = "Select * from `disclosures`;";
                dt = G1.get_db_data(cmd);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    what = dt.Rows[i]["disclosure"].ObjToString();
                    rtb.Rtf = rtb.Rtf.Replace("%" + what + "%", str);
                }
            }
        }
        /***********************************************************************************************/
        public static string LoadFuneralHomeAddress(EMRControlLib.RichTextBoxEx rtb)
        {
            string name = "name";
            string cmd = "Select * from `funeralhomes` where `keycode` = '" + LoginForm.activeFuneralHomeKeyCode + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
            {
                cmd = "Select * from `funeralhomes` where `atneedcode` = '" + LoginForm.activeFuneralHomeKeyCode + "';";
                dt = G1.get_db_data(cmd);
                if (dt.Rows.Count <= 0)
                {
                    cmd = "Select * from `cemeteries` WHERE `loc` = '" + LoginForm.activeFuneralHomeKeyCode + "';";
                    dt = G1.get_db_data(cmd);
                    if (dt.Rows.Count <= 0)
                    {
                        MessageBox.Show("***ERROR*** Invalid Funeral Home!");
                        //this.Close();
                        return "";
                    }
                    else
                        name = "description";
                }
            }
            string activeFuneralHomeName = dt.Rows[0][name].ObjToString();
            //if (String.IsNullOrWhiteSpace(activeFuneralHomeName))
            //    activeFuneralHomeName = dt.Rows[0]["name"].ObjToString();
            if (!String.IsNullOrWhiteSpace(activeFuneralHomeName))
            {
                //rtb.Rtf = rtb.Rtf.Replace("*FUNERALHOMEMY*", activeFuneralHomeName);
                rtb.Rtf = rtb.Rtf.Replace("*FUNERALHOME*", activeFuneralHomeName);
                rtb.Rtf = rtb.Rtf.Replace("[*BRANCH*]", activeFuneralHomeName);
                //                rtb.Rtf = ArrangementForms.ReplaceField(rtb.Rtf, "*FUNERALHOME*", activeFuneralHomeName);

            }

            string poBox = dt.Rows[0]["POBox"].ObjToString();
            string address = dt.Rows[0]["address"].ObjToString();
            if (!String.IsNullOrWhiteSpace(poBox))
                address = "P.O.Box  " + poBox + "     " + address;
            if (!String.IsNullOrWhiteSpace(address))
                rtb.Rtf = rtb.Rtf.Replace("%FUNERALADDRESS%", address);

            string city = dt.Rows[0]["city"].ObjToString();
            string state = dt.Rows[0]["state"].ObjToString();
            string zip = dt.Rows[0]["zip"].ObjToString();
            address = city + ", " + state + "  " + zip;
            if (!String.IsNullOrWhiteSpace(city) || !String.IsNullOrWhiteSpace(state))
            {
                rtb.Rtf = rtb.Rtf.Replace("%FUNERALLOCATION%", address);
                rtb.Rtf = rtb.Rtf.Replace("[*BRANCHCTYST*]", address);
            }

            string phone = dt.Rows[0]["phoneNumber"].ObjToString();
            if (!String.IsNullOrWhiteSpace(phone))
                rtb.Rtf = rtb.Rtf.Replace("%FUNERALPHONE%", phone);


            //% FUNERALHOME %
            //% FUNERALADDRESS %
            //% FUNERALLOCATION %
            //% FUNERALPHONE %
            return activeFuneralHomeName;
        }
        /***********************************************************************************************/
        private void SetupSelection(DataTable dt)
        {
            DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew = this.repositoryItemCheckEdit2;
            selectnew.NullText = "";
            selectnew.ValueChecked = "1";
            selectnew.ValueUnchecked = "0";
            selectnew.ValueGrayed = "";
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["select"] = "0";
        }
        /****************************************************************************************/
        private double TotalDiscounts = 0D;
        private double TotalPackage = 0D;
        private double urnCredit = 0D;
        private double alternateCredit = 0D;
        private double GetTotalCredits()
        {
            double totalCredits = 0D;
            if (paymentsDt == null)
                return totalCredits;
            string status = "";
            string type = "";
            string payment = "";
            double dValue = 0D;
            for (int i = 0; i < paymentsDt.Rows.Count; i++)
            {
                type = paymentsDt.Rows[i]["type"].ObjToString().ToUpper();
                //if (type == "DISCOUNT")
                //    continue;
                status = paymentsDt.Rows[i]["status"].ObjToString().ToUpper();
                payment = paymentsDt.Rows[i]["payment"].ObjToString();
                if (type.ToUpper() == "DISCOUNT")
                {
                    dValue = paymentsDt.Rows[i]["payment"].ObjToDouble();
                    dValue = Math.Abs(dValue);
                    paymentsDt.Rows[i]["payment"] = dValue;
                    payment = dValue.ObjToString();
                }
                if (status == "ACCEPT" || status == "DEPOSITED" || status == "FILED")
                {
                    if (G1.validate_numeric(payment))
                        totalCredits += payment.ObjToDouble();
                }
            }
            totalCredits += TotalDiscounts;
            return totalCredits;
        }
        /****************************************************************************************/
        private void LoadPayments(DataTable dt)
        {
            TotalDiscounts = 0D;
            TotalPackage = 0D;
            if (paymentsDt == null)
                return;
            if (!btnSeparate.Visible)
                return;
            string service = "";
            string status = "";
            string payment = "";
            string type = "";
            string names = "";
            string referenceNumber = "";
            string description = "";
            DateTime date = DateTime.Now;
            string line = "";
            RemovePreviousPayments(dt);
            double currentPrice = 0D;
            double discount = 0D;
            double dValue = 0D;
            double price = 0D;
            string upgrade = "";
            string deleted = "";
            double upgradeDifference = 0D;
            string trustOrPolicy = "";
            if (G1.get_column_number(dt, "DELETED") < 0)
                dt.Columns.Add("DELETED");

            double totalServices = 0D;
            double totalMerchandise = 0D;
            double totalCashAdvance = 0D;

            double packageDiscount = 0D;
            double packagePrice = 0D;
            double actualDiscount = 0D;
            double grandTotal = 0D;
            double urnCredit = 0D;
            double alternateCredit = 0D;

            bool myPackage = FunServices.GetPackageDetails(dt, ref totalListedPrice, ref packageDiscount, ref packagePrice, ref totalServices, ref totalMerchandise, ref totalCashAdvance, ref actualDiscount, ref grandTotal);

            DataRow[] ddRows = dt.Select("service='Package Discount'");
            if (ddRows.Length > 0)
            {
                ddRows[0]["price"] = G1.ReformatMoney(actualDiscount);
                packageDiscount = actualDiscount;
            }

            string ignore = "";
            double totalIgnore = 0D;
            bool AllIsPackage = true;
            double added = 0D;
            string select = "";

            string zeroData = "";

            string database = G1.conn1.Database.ObjToString();

            CalculateContractDifference(dt);


            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (dt.Rows[i]["isPackage"].ObjToString().ToUpper() != "P")
                    AllIsPackage = false;

                deleted = dt.Rows[i]["deleted"].ObjToString().ToUpper();
                if (deleted == "DELETED" || deleted == "D")
                    continue;
                service = dt.Rows[i]["service"].ObjToString().ToUpper();
                if (service == "TOTAL LISTED PRICE")
                    continue;
                else if (service == "PACKAGE PRICE")
                    continue;
                else if (service == "PACKAGE DISCOUNT")
                    continue;
                if (database.ToUpper() != "SMFS")
                    dt.Rows[i]["difference"] = 0D;
                ignore = dt.Rows[i]["ignore"].ObjToString();
                //currentPrice += dt.Rows[i]["currentprice"].ObjToDouble();
                price = dt.Rows[i]["price"].ObjToDouble();
                upgrade = dt.Rows[i]["upgrade"].ObjToString();
                zeroData = dt.Rows[i]["data"].ObjToString().ToUpper();

                if (!String.IsNullOrWhiteSpace(upgrade))
                {
                    upgradeDifference = upgrade.ObjToDouble();
                    if (upgradeDifference > 0D)
                        dt.Rows[i]["difference"] = upgradeDifference;
                }

                if (service.ToUpper().IndexOf("URN CREDIT") >= 0)
                {
                    if (myPackage)
                    {
                        string pSelect = dt.Rows[i]["pSelect"].ObjToString();
                        if (pSelect == "0")
                            urnCredit = dt.Rows[i]["difference"].ObjToDouble();
                    }
                }
                if (service.ToUpper().IndexOf("ALTERNATIVE CONTAINER CREDIT") >= 0)
                {
                    if (myPackage)
                    {
                        string pSelect = dt.Rows[i]["pSelect"].ObjToString();
                        if (pSelect == "0")
                            alternateCredit = dt.Rows[i]["price"].ObjToDouble();
                    }
                }
                if (service.ToUpper().IndexOf("CREMATION CASKET CREDIT") >= 0)
                {
                    if (myPackage)
                    {
                        string pSelect = dt.Rows[i]["pSelect"].ObjToString();
                        if (pSelect == "0")
                            alternateCredit = dt.Rows[i]["price"].ObjToDouble();
                    }
                }
                if (myPackage && price > 0D)
                {
                    if (dt.Rows[i]["isPackage"].ObjToString().ToUpper() != "P")
                        added += price;
                }

                if (price > 0D || upgradeDifference > 0D || database.ToUpper() != "SMFS")
                {
                    select = dt.Rows[i]["select"].ObjToString();
                    if (select == "1")
                    {
                        currentPrice += dt.Rows[i]["price"].ObjToDouble();
                        dValue = dt.Rows[i]["difference"].ObjToDouble();
                        if (dValue > 0D)
                            discount += dt.Rows[i]["difference"].ObjToDouble();
                        if (ignore == "Y")
                            totalIgnore += dt.Rows[i]["currentprice"].ObjToDouble();
                    }
                }
                else if (price < 0D)
                {
                    discount += dt.Rows[i]["difference"].ObjToDouble();
                }
                else if (zeroData == "ZERO")
                    discount += dt.Rows[i]["difference"].ObjToDouble();
            }

            currentPrice = G1.RoundValue(currentPrice);
            //currentPrice = 0D;
            //price = 0D;
            //discount = 0D;
            //for ( int i=0; i<dt.Rows.Count; i++)
            //{
            //    currentPrice = dt.Rows[i]["currentPrice"].ObjToDouble();
            //    price = dt.Rows[i]["price"].ObjToDouble();
            //    discount += dt.Rows[i]["difference"].ObjToDouble();
            //}
            //            discount = G1.RoundValue(discount - totalIgnore + urnCredit );
            discount = G1.RoundValue(discount - totalIgnore + urnCredit + alternateCredit - upgradeDifference);
            //discount -= upgradeDifference;


            date = DateTime.Now;

            DataRow dRow = null;
            //DataRow dRow = dt.NewRow();
            //dRow["type"] = "Payment";
            //dRow["price"] = G1.ReformatMoney(currentPrice);
            //dRow["service"] = date.ToString("MM/dd/yyyy") + " Current Price";
            //dt.Rows.Add(dRow);
            TotalPackage = currentPrice;
            if (PackageDiscount != 0D)
                discount = 0D;

            if (discount > 0D)
            {
                dRow = dt.NewRow();
                dRow["type"] = "Payment";
                dRow["price"] = G1.ReformatMoney(discount);
                dRow["service"] = date.ToString("MM/dd/yyyy") + " PreNeed Discount";
                dt.Rows.Add(dRow);
            }
            TotalDiscounts = discount;

            gotPackage = myPackage;
            if (myPackage)
                PackageDiscount = actualDiscount;

            //if (actualDiscount != 0D)
            //{
            //    if (actualDiscount < 0D)
            //        actualDiscount = actualDiscount * (-1D);
            //    dRow = dt.NewRow();
            //    dRow["type"] = "Payment";
            //    dRow["price"] = G1.ReformatMoney(actualDiscount);
            //    dRow["service"] = date.ToString("MM/dd/yyyy") + " Package Discount";
            //    dt.Rows.Add(dRow);
            //    TotalDiscounts += actualDiscount;
            //    gotPackage = true;
            //}

            if (PackageDiscount != 0D)
            {
                if (PackageDiscount < 0D)
                    PackageDiscount = PackageDiscount * (-1D);

                //                PackageDiscount = (totalServices + totalMerchandise + totalCashAdvance) - packagePrice + urnCredit;
                double newDiscount = (totalServices + totalMerchandise + totalCashAdvance) - packagePrice + urnCredit + alternateCredit;
                //if ( newDiscount <= PackageDiscount )
                //    PackageDiscount = (totalServices + totalMerchandise + totalCashAdvance) - packagePrice + urnCredit + alternateCredit;

                dRow = dt.NewRow();
                dRow["type"] = "Payment";
                dRow["price"] = G1.ReformatMoney(PackageDiscount);
                dRow["service"] = date.ToString("MM/dd/yyyy") + " Package Discount";
                dt.Rows.Add(dRow);
                TotalDiscounts += PackageDiscount;
                gotPackage = true;
            }

            string[] Lines = null;
            string money = "";
            for (int i = 0; i < paymentsDt.Rows.Count; i++)
            {
                status = paymentsDt.Rows[i]["status"].ObjToString().ToUpper();
                if (status != "ACCEPT" && status != "DEPOSITED" && status != "PENDING" && status != "FILED")
                    continue;
                payment = paymentsDt.Rows[i]["payment"].ObjToString();
                if (payment == "0")
                    continue;
                if (String.IsNullOrWhiteSpace(payment))
                    payment = "0.00";
                type = paymentsDt.Rows[i]["type"].ObjToString();
                if (type.ToUpper() == "REFUND")
                {
                }

                trustOrPolicy = paymentsDt.Rows[i]["trust_policy"].ObjToString();
                names = paymentsDt.Rows[i]["names"].ObjToString();
                referenceNumber = paymentsDt.Rows[i]["referenceNumber"].ObjToString();
                if (trustOrPolicy.IndexOf("/") > 0)
                {
                    Lines = trustOrPolicy.Split('/');
                    if (Lines.Length >= 1)
                        referenceNumber = Lines[0].Trim();
                }
                if (String.IsNullOrWhiteSpace(referenceNumber))
                    referenceNumber = trustOrPolicy;
                description = paymentsDt.Rows[i]["description"].ObjToString();
                Lines = description.Split('~');
                if (Lines.Length > 0)
                    description = Lines[0].Trim();
                if (!String.IsNullOrWhiteSpace(referenceNumber))
                    names += " " + referenceNumber;
                if (String.IsNullOrWhiteSpace(names))
                    names = trustOrPolicy;

                if (!String.IsNullOrWhiteSpace(names))
                    description = names + " " + description;

                //if (String.IsNullOrWhiteSpace(description))
                //    description = type;
                date = paymentsDt.Rows[i]["dateEntered"].ObjToDateTime();
                if (date.Year < 100)
                    date = paymentsDt.Rows[i]["dateModified"].ObjToDateTime();
                //                line = date.ToString("MM/dd/yyyy") + " " + status + " " + description;
                line = date.ToString("MM/dd/yyyy");
                if (type.ToUpper().IndexOf("CASH") >= 0)
                    line = date.ToString("MM/dd/yyyy") + " CASH";
                else if (type.ToUpper().IndexOf("CHECK") >= 0)
                    line = date.ToString("MM/dd/yyyy") + " CHECK";
                else if (type.ToUpper().IndexOf("CREDIT CARD") >= 0)
                    line = date.ToString("MM/dd/yyyy") + " CREDIT CARD";
                else if (type.ToUpper().IndexOf("DISCOUNT") >= 0)
                    line = date.ToString("MM/dd/yyyy") + " DISCOUNT ";
                else if (type.ToUpper().IndexOf("REFUND") >= 0)
                    line = date.ToString("MM/dd/yyyy") + " REFUND ";
                if (status.ToUpper() == "PENDING" || status.ToUpper() == "FILED")
                {
                    payment = payment.Replace("$", "");
                    payment = payment.Replace(",", "");
                    payment = "$" + G1.ReformatMoney(payment.ObjToDouble());
                    if (status.ToUpper() == "PENDING")
                    {
                        if (type.ToUpper().IndexOf("TRUST") >= 0)
                            line = date.ToString("MM/dd/yyyy") + " PENDING PRENEED TRUST " + " (" + payment + ")";
                        else if (type.ToUpper().IndexOf("INS") >= 0)
                            line = date.ToString("MM/dd/yyyy") + " PENDING INSURANCE " + " (" + payment + ")";
                        else if (type.ToUpper().IndexOf("CLASS A") >= 0)
                            line = date.ToString("MM/dd/yyyy") + " PENDING CLASS-A " + " (" + payment + ")";
                        else if (type.ToUpper().IndexOf("CHECK") >= 0)
                            line = date.ToString("MM/dd/yyyy") + " PENDING CHECK " + " (" + payment + ")";
                        else if (type.ToUpper().IndexOf("3RD PARTY") >= 0)
                            line = date.ToString("MM/dd/yyyy") + " PENDING CHECK " + " (" + payment + ")";
                        else if (type.ToUpper().IndexOf("OTHER") >= 0)
                            line = date.ToString("MM/dd/yyyy") + " PENDING OTHER " + " (" + payment + ")";
                    }
                    else if (status.ToUpper() == "FILED")
                    {
                        if (type.ToUpper().IndexOf("TRUST") >= 0)
                            line = date.ToString("MM/dd/yyyy") + " FILED PRENEED TRUST " + " (" + payment + ")";
                        else if (type.ToUpper().IndexOf("INS") >= 0)
                            line = date.ToString("MM/dd/yyyy") + " FILED INSURANCE " + " (" + payment + ")";
                        else if (type.ToUpper().IndexOf("CLASS A") >= 0)
                            line = date.ToString("MM/dd/yyyy") + " FILED CLASS-A " + " (" + payment + ")";
                        else if (type.ToUpper().IndexOf("CHECK") >= 0)
                            line = date.ToString("MM/dd/yyyy") + " FILED CHECK " + " (" + payment + ")";
                        else if (type.ToUpper().IndexOf("3RD PARTY") >= 0)
                            line = date.ToString("MM/dd/yyyy") + " FILED CHECK " + " (" + payment + ")";
                    }
                    payment = "0.00";
                }
                else if (status.ToUpper() == "DEPOSITED" && type.ToUpper() == "TRUST")
                {
                    //line = date.ToString("MM/dd/yyyy") + " TRUST " + paymentsDt.Rows[i]["trust_policy"].ObjToString() + " (" + payment + ")";
                    line = date.ToString("MM/dd/yyyy") + " TRUST";
                }
                else if (status.ToUpper() == "ACCEPT")
                {
                    money = payment;
                    money = money.Replace("$", "");
                    money = money.Replace(",", "");
                    money = "$" + G1.ReformatMoney(money.ObjToDouble());
                    line += " (" + money + ")";
                }

                if (!String.IsNullOrWhiteSpace(description))
                    line += " " + description;
                if (type.ToUpper() == "DISCOUNT")
                {
                    dValue = paymentsDt.Rows[i]["payment"].ObjToDouble();
                    dValue = Math.Abs(dValue);
                    payment = dValue.ObjToString();
                }

                dRow = dt.NewRow();
                dRow["type"] = "Payment";
                dRow["price"] = payment;
                dRow["service"] = line;
                dt.Rows.Add(dRow);
            }

            ProcessAllowances(dt);
        }
        /****************************************************************************************/
        private bool gotDiscounts = false;
        private bool gotPayments = false;
        private double totalAllowances = 0D;
        private double newTotalAllowances = 0D;
        private double newPayments = 0D;
        private void ProcessAllowances(DataTable dt)
        {
            float tinyFontSize = 2.5F;
            float smallFontSize = 7.5F;
            float largeFontSize = 8.5F;
            string service = "";
            string type = "";
            gotDiscounts = false;
            gotPayments = false;
            totalAllowances = 0D;
            newTotalAllowances = 0D;
            newPayments = 0D;
            double payment = 0D;
            double price = 0D;

            bool gotPD = false;
            double dValue = 0D;

            DataTable allowDt = dt.Clone();
            //AppendToTable(allowDt, "ALLOWANCES:", "", "Arial Black", largeFontSize);

            DataRow[] dRows = dt.Select("service LIKE '%Package Discount'");
            if (dRows.Length > 0)
            {
                for (int i = 0; i < dRows.Length; i++)
                    dRows[i]["price"] = allDiscount;
            }

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                service = dt.Rows[i]["service"].ObjToString();
                type = dt.Rows[i]["type"].ObjToString().ToUpper();
                if (type == "PAYMENT")
                {
                    price = dt.Rows[i]["price"].ObjToDouble();
                    if (service.ToUpper().IndexOf("DISCOUNT") >= 0)
                    {
                        //newTotalAllowances += Math.Abs(dt.Rows[i]["price"].ObjToDouble());
                        dValue = dt.Rows[i]["price"].ObjToDouble();
                        dValue = Math.Abs(dValue);
                        newTotalAllowances += dValue;
                    }
                    else
                        newPayments += dt.Rows[i]["price"].ObjToDouble();
                }
            }


            for (int i = 0; i < dt.Rows.Count; i++)
            {
                service = dt.Rows[i]["service"].ObjToString();
                if (service.ToUpper().IndexOf("DISCOUNT") >= 0)
                {
                    G1.copy_dt_row(dt, i, allowDt, allowDt.Rows.Count);
                    allowDt.Rows[allowDt.Rows.Count - 1]["type"] = "Allow";
                    dt.Rows[i]["type"] = "XXX";
                }
            }

            for (int i = 0; i < allowDt.Rows.Count; i++)
            {
                G1.copy_dt_row(allowDt, i, dt, dt.Rows.Count);
            }

            double packageDiscount = 0D;
            double totalAllow = 0D;
            totalAllowances = 0D;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                service = dt.Rows[i]["service"].ObjToString();
                type = dt.Rows[i]["type"].ObjToString().ToUpper();
                if (type == "PAYMENT")
                    gotPayments = true;
                else if (type == "ALLOW")
                {
                    gotDiscounts = true;
                    payment = dt.Rows[i]["price"].ObjToDouble();
                    totalAllow += payment;
                    if (service.ToUpper().IndexOf("PACKAGE DISCOUNT") >= 0)
                    {
                        //if ( !gotPD )
                        packageDiscount += payment;
                        gotPD = true;
                    }
                }
                else if (service.ToUpper().IndexOf("PACKAGE DISCOUNT") > 0)
                {
                    if (type != "XXX")
                    {
                        gotDiscounts = true;
                        payment = dt.Rows[i]["price"].ObjToDouble();
                        //if (!gotPD)
                        //{
                        totalAllowances += payment;
                        packageDiscount += payment;
                        //}
                        gotPD = true;
                    }
                }
            }
            totalAllowances += totalAllow;
            for (int i = (dt.Rows.Count - 1); i >= 0; i--)
            {
                type = dt.Rows[i]["type"].ObjToString().ToUpper();
                if (type == "XXX")
                    dt.Rows.RemoveAt(i);
            }
        }
        private void ProcessAllowancesx(DataTable dt)
        {
            float tinyFontSize = 2.5F;
            float smallFontSize = 7.5F;
            float largeFontSize = 8.5F;
            string service = "";
            string type = "";
            gotDiscounts = false;
            gotPayments = false;
            totalAllowances = 0D;
            newTotalAllowances = 0D;
            newPayments = 0D;
            double payment = 0D;
            double price = 0D;

            DataTable allowDt = dt.Clone();
            //AppendToTable(allowDt, "ALLOWANCES:", "", "Arial Black", largeFontSize);

            DataRow[] dRows = dt.Select("service LIKE '%Package Discount'");
            if (dRows.Length > 0)
            {
                for (int i = 0; i < dRows.Length; i++)
                    dRows[i]["price"] = allDiscount;
            }

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                service = dt.Rows[i]["service"].ObjToString();
                type = dt.Rows[i]["type"].ObjToString().ToUpper();
                if (type == "PAYMENT")
                {
                    price = dt.Rows[i]["price"].ObjToDouble();
                    if (service.ToUpper().IndexOf("DISCOUNT") >= 0)
                        newTotalAllowances += dt.Rows[i]["price"].ObjToDouble();
                    else
                        newPayments += dt.Rows[i]["price"].ObjToDouble();
                }
            }


            for (int i = 0; i < dt.Rows.Count; i++)
            {
                service = dt.Rows[i]["service"].ObjToString();
                if (service.ToUpper().IndexOf("DISCOUNT") >= 0)
                {
                    G1.copy_dt_row(dt, i, allowDt, allowDt.Rows.Count);
                    allowDt.Rows[allowDt.Rows.Count - 1]["type"] = "Allow";
                    dt.Rows[i]["type"] = "XXX";
                }
            }

            for (int i = 0; i < allowDt.Rows.Count; i++)
            {
                G1.copy_dt_row(allowDt, i, dt, dt.Rows.Count);
            }

            double packageDiscount = 0D;
            double totalAllow = 0D;
            totalAllowances = 0D;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                service = dt.Rows[i]["service"].ObjToString();
                type = dt.Rows[i]["type"].ObjToString().ToUpper();
                if (type == "PAYMENT")
                    gotPayments = true;
                else if (type == "ALLOW")
                {
                    gotDiscounts = true;
                    payment = dt.Rows[i]["price"].ObjToDouble();
                    totalAllow += payment;
                    if (service.ToUpper().IndexOf("PACKAGE DISCOUNT") >= 0)
                        packageDiscount += payment;
                }
                else if (service.ToUpper().IndexOf("PACKAGE DISCOUNT") > 0)
                {
                    if (type != "XXX")
                    {
                        gotDiscounts = true;
                        payment = dt.Rows[i]["price"].ObjToDouble();
                        totalAllowances += payment;
                        packageDiscount += payment;
                    }
                }
            }
            totalAllowances += totalAllow;
            for (int i = (dt.Rows.Count - 1); i >= 0; i--)
            {
                type = dt.Rows[i]["type"].ObjToString().ToUpper();
                if (type == "XXX")
                    dt.Rows.RemoveAt(i);
            }
        }
        /****************************************************************************************/
        private void RemovePreviousPayments(DataTable dt)
        {
            string type = "";
            for (int i = dt.Rows.Count - 1; i >= 0; i--)
            {
                type = dt.Rows[i]["type"].ObjToString().ToUpper();
                if (type == "PAYMENT")
                    dt.Rows.RemoveAt(i);
            }
        }
        /****************************************************************************************/
        private void LoadServices()
        {
            if (String.IsNullOrWhiteSpace(workContract))
                return;
            string cmd = "Select * from `fcust_services` c LEFT JOIN `funeral_master` s ON c.`service` = s.`service` where `contractNumber` = '" + workContract + "';";
            DataTable dt = G1.get_db_data(cmd);

            string casketCode = "";
            string type = "";
            bool foundPicture = false;
            dt.Columns.Add("num");
            dt.Columns.Add("mod");
            dt.Columns.Add("select");
            dt.Columns.Add("total", Type.GetType("System.Double"));
            dt.Columns.Add("currentprice", Type.GetType("System.Double"));

            SetupSelection(dt);

            if (G1.get_column_number(dt, "currentprice") < 0)
            {
                if (G1.get_column_number(dt, "price1") >= 0)
                {
                    dt.Columns["price1"].ColumnName = "currentprice";
                }
            }


            if (serviceDt != null)
                dt = serviceDt;

            cmd = "Select * from `fcust_services` where `data` LIKE 'CASKET:%' and `contractNumber` = '" + workContract + "';";
            DataTable dx = G1.get_db_data(cmd);
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                dt.ImportRow(dx.Rows[i]);
            }

            string str = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                str = dt.Rows[i]["type"].ObjToString();
                str = G1.force_lower_line(str);
                dt.Rows[i]["type"] = str;
            }

            if (dt.Rows.Count <= 0)
                ResolveImportedData(dt);

            CleanupOriginal(dt, "Casket");
            CleanupOriginal(dt, "Outer Container");
            CleanupOriginal(dt, "Alt Container");
            CleanupOriginal(dt, "URN");

            ReCalcTotalAll(dt, ref mainPackageDiscount, ref allServices, ref allMerchandise, ref allCashAdvance, ref allSalesTax, ref allDiscount, ref allSubTotal, ref allTotal);

            DetermineServices(dt);
            ReCalcTotal(dt);

            //if (serviceDt != null)
            //    dt = serviceDt;


            LoadPayments(dt);

            workDt = dt;
            urnCredit = FilterZeros(dt);
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
        }
        /***********************************************************************************************/
        private double mainPackageDiscount = 0D;
        private double allServices = 0D;
        private double allMerchandise = 0D;
        private double allCashAdvance = 0D;
        private double allSalesTax = 0D;
        private double allDiscount = 0D;
        private double allSubTotal = 0D;
        private double allTotal = 0D;
        /***********************************************************************************************/
        private DataTable ReorderMerchandise(DataTable funDt)
        {

            DataTable dt1 = funDt.Clone();
            DataTable dt2 = funDt.Clone();

            string data = "";

            for (int i = 0; i < funDt.Rows.Count; i++)
            {
                data = funDt.Rows[i]["data"].ObjToString();
                if (String.IsNullOrEmpty(data))
                    dt1.ImportRow(funDt.Rows[i]);
                else
                    dt2.ImportRow(funDt.Rows[i]);
            }
            DataView tempview = dt2.DefaultView;
            tempview.Sort = "service asc";
            dt2 = tempview.ToTable();

            funDt.Rows.Clear();
            for (int i = 0; i < dt1.Rows.Count; i++)
                funDt.ImportRow(dt1.Rows[i]);
            for (int i = 0; i < dt2.Rows.Count; i++)
                funDt.ImportRow(dt2.Rows[i]);

            string service = "";
            string type = "";
            string[] Lines = null;
            DataTable mDt = null;
            string casketCode = "";
            string cmd = "";
            string str = "";

            if (G1.get_column_number(funDt, "Order") < 0)
                funDt.Columns.Add("Order", Type.GetType("System.Int32"));

            for (int i = 0; i < funDt.Rows.Count; i++)
            {
                service = funDt.Rows[i]["service"].ObjToString().ToUpper().Trim();
                if (service.IndexOf("B.") == 0)
                {
                    funDt.Rows[i]["Order"] = -1;
                    continue;
                }
                funDt.Rows[i]["Order"] = 10;
                if (service.IndexOf("ACKNOW") < 0 && service.IndexOf("GRAVE MARKER") < 0 && service.ToUpper().IndexOf("REGISTER BOOK") < 0)
                {
                    cmd = "SELECT * FROM `casket_master` WHERE `casketDesc` = '" + service + "';";
                    mDt = G1.get_db_data(cmd);
                    if (mDt.Rows.Count <= 0)
                    {
                        Lines = service.Split(' ');
                        str = Lines[0].Trim();
                        cmd = "SELECT * FROM `casket_master` WHERE `casketcode` = '" + str + "';";
                        mDt = G1.get_db_data(cmd);
                        if (mDt.Rows.Count <= 0)
                        {
                            service = service.Replace(Lines[0].Trim(), "").Trim();
                            cmd = "SELECT * FROM `casket_master` WHERE `casketDesc` = '" + service + "';";
                            mDt = G1.get_db_data(cmd);
                        }
                    }
                    if (mDt.Rows.Count > 0)
                    {
                        casketCode = mDt.Rows[0]["casketcode"].ObjToString();
                        if (casketCode.ToUpper().IndexOf("V") == 0)
                            funDt.Rows[i]["Order"] = 2;
                        else if (casketCode.ToUpper().IndexOf("URN") == 0)
                            funDt.Rows[i]["Order"] = 3;
                        else if (casketCode.ToUpper().IndexOf("UV") == 0)
                            funDt.Rows[i]["Order"] = 3;
                        else
                            funDt.Rows[i]["Order"] = 1;
                    }
                }
                else
                    funDt.Rows[i]["Order"] = 5;
            }
            tempview = funDt.DefaultView;
            tempview.Sort = "Order asc";
            funDt = tempview.ToTable();

            funDt.Columns.Remove("Order");

            return funDt;
        }
        /***********************************************************************************************/
        private void ResolveImportedData(DataTable dt)
        {
            string cmd = "Select * from `fcustomers` where `contractNumber` = '" + workContract + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
                return;

            string group = EditCustomer.activeFuneralHomeCasketGroup;
            if (String.IsNullOrWhiteSpace(group))
                group = "Casket Group 3.3";

            string casketCode = dx.Rows[0]["extraItemAmtMI1"].ObjToString();
            string vaultCode = dx.Rows[0]["extraItemAmtMI2"].ObjToString();
            double casketPrice = dx.Rows[0]["extraItemAmtMR1"].ObjToDouble();
            double vaultPrice = dx.Rows[0]["extraItemAmtMR2"].ObjToDouble();

            ProcessImportedData(dt, casketCode, casketPrice);
            ProcessImportedData(dt, vaultCode, vaultPrice);
        }
        /***********************************************************************************************/
        private void ProcessImportedData(DataTable dt, string casketCode, double casketPrice)
        {
            if (String.IsNullOrWhiteSpace(casketCode))
                return;
            string cmd = "Select * from `casket_master` where `casketcode` = '" + casketCode + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
            {
                cmd = "Select * from `casket_master` where `casketcode` LIKE '" + casketCode + "%';";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count <= 0)
                    return;
            }

            string group = EditCustomer.activeFuneralHomeCasketGroup;
            if (String.IsNullOrWhiteSpace(group))
                group = "Casket Group 3.3";

            string masterRecord = dx.Rows[0]["record"].ObjToString();
            double cost = dx.Rows[0]["casketcost"].ObjToDouble();
            string service = dx.Rows[0]["casketdesc"].ObjToString();

            double currentPrice = 0D;
            DataRow dR = null;

            string chr = casketCode.Substring(0, 1).ToUpper();
            if (chr == "V" || casketCode.IndexOf("URN") == 0 || casketCode.ToUpper().IndexOf("UV") == 0)
            {
                //                currentPrice = dx.Rows[0]["casketprice"].ObjToDouble();
                if (service.IndexOf(casketCode) < 0)
                    service = casketCode + " " + service;
                dR = dt.NewRow();
                dR["service"] = service;
                dR["currentprice"] = cost;
                dR["price"] = casketPrice;
                dR["type"] = "Merchandise";
                dt.Rows.Add(dR);
                return;
            }

            cmd = "Select * from `casket_packages` where `!masterRecord` = '" + masterRecord + "' and `groupname` = '" + group + "';";
            dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
                return;
            double markup = dx.Rows[0]["markup"].ObjToDouble();
            currentPrice = cost * markup;
            dR = dt.NewRow();
            dR["service"] = service;
            dR["currentprice"] = currentPrice;
            dR["price"] = casketPrice;
            dR["type"] = "Merchandise";
            dt.Rows.Add(dR);
        }
        /***********************************************************************************************/
        private void CleanupOriginal(DataTable dt, string name)
        {
            string casketName = "";
            DataRow[] dR = dt.Select("service='" + name + " Name'");
            if (dR.Length <= 0)
                return;
            casketName = dR[0]["data"].ObjToString();
            if (String.IsNullOrWhiteSpace(casketName))
                return;
            dR = dt.Select("service='" + name + " Price'");
            if (dR.Length <= 0)
                return;
            dR[0]["service"] = casketName;
            string str = dR[0]["price"].ObjToString();
            double casketPrice = str.ObjToDouble();
            str = dR[0]["data"].ObjToString();
            if (casketPrice == 0D && G1.validate_numeric(str))
            {
                casketPrice = str.ObjToDouble();
                dR[0]["price"] = casketPrice;
            }


            string[] Lines = casketName.Split(' ');
            if (Lines.Length < 1)
                return;
            string code = Lines[0];
            string service = casketName.Replace(code, "").Trim();
            if (String.IsNullOrWhiteSpace(service))
                service = casketName;
            string cmd = "Select * from `casket_master` where `casketdesc` LIKE '%" + service + "%';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
                return;

            string group = EditCustomer.activeFuneralHomeCasketGroup;
            if (String.IsNullOrWhiteSpace(group))
                group = "Casket Group 3.3";

            string masterRecord = dx.Rows[0]["record"].ObjToString();
            double cost = dx.Rows[0]["casketcost"].ObjToDouble();
            cmd = "Select * from `casket_packages` where `!masterRecord` = '" + masterRecord + "' and `groupname` = '" + group + "';";
            dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
            {
                dR[0]["currentprice"] = cost;
                return;
            }
            double markup = dx.Rows[0]["markup"].ObjToDouble();
            double currentPrice = cost * markup;
            dR[0]["currentprice"] = currentPrice;
        }
        /***********************************************************************************************/
        private void DetermineServices(DataTable dt)
        {
            double data = 0D;
            double price = 0D;
            double pastPrice = 0D;
            string type = "";
            string type1 = "";
            string cmd = "";
            string service = "";
            string record = "";

            string group = EditCustomer.activeFuneralHomeGroup;
            if (String.IsNullOrWhiteSpace(group))
                group = "Group 3 GPL";

            DataTable dx = null;
            if (G1.get_column_number(dt, "currentprice") < 0)
                dt.Columns.Add("currentprice", Type.GetType("System.Double"));

            double totalServices = 0D;
            double totalMerchandise = 0D;
            double totalCashAdvance = 0D;

            double packageDiscount = 0D;
            double packagePrice = 0D;
            double actualDiscount = 0D;
            double grandTotal = 0D;
            double urnCredit = 0D;
            double alternateCredit = 0D;
            string pSelect = "";

            string packageName = "";

            bool myPackage = FunServices.GetPackageDetails(dt, ref totalListedPrice, ref packageDiscount, ref packagePrice, ref totalServices, ref totalMerchandise, ref totalCashAdvance, ref actualDiscount, ref grandTotal);


            double currentPrice = 0D;
            string database = G1.conn1.Database.ObjToString();

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                currentPrice = dt.Rows[i]["currentprice"].ObjToDouble();
                dt.Rows[i]["select"] = "1";
                if (myPackage)
                {
                    packageName = "";
                    if (G1.get_column_number(dt, "PackageName") >= 0)
                        packageName = dt.Rows[i]["PackageName"].ObjToString().Trim();
                    //if ( !String.IsNullOrWhiteSpace ( packageName ))
                    dt.Rows[i]["select"] = dt.Rows[i]["pSelect"].ObjToString();
                }

                record = dt.Rows[i]["record"].ObjToString();
                //if (!String.IsNullOrWhiteSpace(record))
                //    continue;

                type = dt.Rows[i]["type"].ObjToString();
                service = dt.Rows[i]["service"].ObjToString();
                if (service.ToUpper() == "ACKNOLEDGEMENT CARDS")
                    service = "ACKNOWLEDGEMENT CARDS";
                if (service.ToUpper() == "PACKAGE DISCOUNT")
                    continue;
                if (!String.IsNullOrWhiteSpace(service))
                {
                    //                        cmd = "Select * from `services` where `service` = '" + service + "';";
                    cmd = "Select * from `funeral_gplgroups` where `groupname` = '" + group + "' and `service` = '" + service + "';";
                    cmd = "Select * from `funeral_gplgroups` g LEFT JOIN `funeral_master` s on g.`service` = s.`service` where g.`groupname` = '" + group + "' AND g.`service` = '" + service + "';";

                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                    {
                        type = dx.Rows[0]["type"].ObjToString();
                        type1 = dx.Rows[0]["type1"].ObjToString();
                        if (!String.IsNullOrWhiteSpace(type1))
                            type = type1;
                        if (service.ToUpper().IndexOf("DVD") > 0)
                            type = "Merchandise";
                        dt.Rows[i]["type"] = type;
                        currentPrice = dx.Rows[0]["price"].ObjToDouble();
                    }
                }
                data = dt.Rows[i]["data"].ObjToDouble();
                price = dt.Rows[i]["price"].ObjToDouble();
                if (database.ToUpper() != "SMFS")
                {
                    if (price <= 0D)
                        dt.Rows[i]["price"] = data;
                }
                dt.Rows[i]["currentprice"] = currentPrice;

                pastPrice = dt.Rows[i]["pastPrice"].ObjToDouble();
                if (pastPrice > 0D)
                    dt.Rows[i]["currentprice"] = pastPrice;

                //if (service.ToUpper().IndexOf("URN CREDIT") >= 0)
                //{
                //    pSelect = dt.Rows[i]["pSelect"].ObjToString();
                //    if (pSelect == "0")
                //        dt.Rows[i]["price"] = 0D;
                //}

            }
            CalculateDifference(dt);
        }
        ///***********************************************************************************************/
        //public static bool GetPackageDetails(DataTable dx, ref double packageList, ref double packageDiscount, ref double packagePrice, ref double totalServices, ref double totalMerchandise, ref double cashAdvance, ref double actualDiscount, ref double grandTotal)
        //{
        //    packageList = 0D;
        //    packageDiscount = 0D;
        //    packagePrice = 0D;
        //    totalServices = 0D;
        //    totalMerchandise = 0D;
        //    cashAdvance = 0D;
        //    grandTotal = 0D;

        //    DataRow[] dRows = dx.Select("isPackage='P'");
        //    if (dRows.Length <= 0)
        //        return false;

        //    string deleted = "";
        //    string select = "";
        //    string service = "";
        //    string type = "";
        //    string currentPriceColumn = "currentprice";

        //    double price = 0D;
        //    double upgrade = 0D;
        //    double customerDiscount = 0D;

        //    double unServices = 0D;
        //    double unMerchandise = 0D;
        //    double unCashAdvance = 0D;

        //    string isPackage = "";

        //    DataTable dt = dRows.CopyToDataTable();

        //    if (G1.get_column_number(dt, "currentprice") < 0)
        //    {
        //        if (G1.get_column_number(dt, "price1") >= 0)
        //        {
        //            dt.Columns["price1"].ColumnName = "currentprice";
        //        }
        //        else
        //            currentPriceColumn = "price";
        //    }

        //    for (int i = 0; i < dt.Rows.Count; i++)
        //    {
        //        try
        //        {
        //            deleted = dt.Rows[i]["DELETED"].ObjToString().ToUpper();
        //            if (deleted == "DELETED" || deleted == "D")
        //                continue;

        //            select = dt.Rows[i]["select"].ObjToString();
        //            service = dt.Rows[i]["service"].ObjToString();
        //            type = dt.Rows[i]["type"].ObjToString().ToUpper();
        //            if (type.ToUpper() == "REFUND")
        //            {
        //            }

        //            if (service.ToUpper() == "TOTAL LISTED PRICE")
        //            {
        //                if (select == "0")
        //                    continue;
        //                packagePrice = dt.Rows[i]["price"].ObjToDouble();
        //                packageList = packagePrice;
        //                continue;
        //            }
        //            else if (service.ToUpper() == "PACKAGE PRICE")
        //            {
        //                if (select == "0")
        //                    continue;
        //                packagePrice = dt.Rows[i]["price"].ObjToDouble();
        //                continue;
        //            }
        //            else if (service.ToUpper() == "PACKAGE DISCOUNT")
        //            {
        //                if (select == "0")
        //                {
        //                    //mainPackageDiscount = 0D;
        //                    continue;
        //                }
        //                packageDiscount = dt.Rows[i]["price"].ObjToDouble();
        //                continue;
        //            }

        //            select = dt.Rows[i]["select"].ObjToString();
        //            if (select == "0")
        //            {
        //                price = dt.Rows[i]["price"].ObjToDouble();
        //                if (price < 0D)
        //                {
        //                    price = Math.Abs(price);
        //                    customerDiscount += dt.Rows[i]["difference"].ObjToDouble();
        //                }
        //                isPackage = dt.Rows[i]["isPackage"].ObjToString().ToUpper();
        //                if (type.ToUpper() == "SERVICE" && isPackage == "P")
        //                {
        //                    if (service.ToUpper().IndexOf("URN CREDIT") < 0)
        //                        unServices += price;
        //                }
        //                else if (type.ToUpper() == "MERCHANDISE" && isPackage == "P")
        //                    unMerchandise += price;
        //                else if (type.ToUpper() == "CASH ADVANCE" && isPackage == "P")
        //                    unCashAdvance += price;
        //                continue;
        //            }
        //            if (select == "1")
        //            {
        //                if ( type.ToUpper() == "REFUND")
        //                {
        //                }
        //                upgrade = dt.Rows[i]["upgrade"].ObjToDouble();
        //                price = dt.Rows[i]["price"].ObjToDouble();
        //                if (price <= 0D && upgrade <= 0D)
        //                    continue;
        //                price = dt.Rows[i][currentPriceColumn].ObjToDouble();
        //                price = dt.Rows[i]["price"].ObjToDouble();
        //                price = Math.Abs(price);
        //                customerDiscount += dt.Rows[i]["difference"].ObjToDouble();
        //                if (type.ToUpper() == "SERVICE")
        //                    totalServices += price;
        //                else if (type.ToUpper() == "MERCHANDISE")
        //                    totalMerchandise += price;
        //                else if (type.ToUpper() == "CASH ADVANCE")
        //                    cashAdvance += price;
        //            }
        //            else
        //            {
        //                type = dt.Rows[i]["type"].ObjToString().ToUpper();
        //                if (type != "CASH ADVANCE")
        //                {
        //                    upgrade = dt.Rows[i]["upgrade"].ObjToDouble();
        //                    price = dt.Rows[i]["price"].ObjToDouble();
        //                    price = Math.Abs(price);
        //                    if (type.ToUpper() == "SERVICE")
        //                        unServices += price;
        //                    else if (type.ToUpper() == "MERCHANDISE")
        //                        unMerchandise += price;
        //                    if (price <= 0D && upgrade <= 0D)
        //                        continue;
        //                    price = dt.Rows[i][currentPriceColumn].ObjToDouble();
        //                    customerDiscount += dt.Rows[i]["difference"].ObjToDouble();
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //        }
        //    }

        //    actualDiscount = Math.Abs(packageDiscount) - unServices - unMerchandise - unCashAdvance;
        //    actualDiscount = actualDiscount * -1D;
        //    grandTotal = packagePrice;

        //    return true;
        //}
        /***********************************************************************************************/
        private void CalculateContractDifference(DataTable dt)
        {
            double price = 0D;
            double currentprice = 0D;
            double difference = 0D;
            if (G1.get_column_number(dt, "difference") < 0)
                dt.Columns.Add("difference", Type.GetType("System.Double"));
            string select = "";
            string zero = "";

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                select = dt.Rows[i]["select"].ObjToString();
                if (select != "1")
                    continue;
                price = dt.Rows[i]["price"].ObjToDouble();
                zero = dt.Rows[i]["data"].ObjToString().ToUpper();
                if (zero == "ZERO")
                {
                }
                if (price > 0D || zero == "ZERO")
                {
                    currentprice = dt.Rows[i]["currentprice"].ObjToDouble();
                    difference = currentprice - price;
                    dt.Rows[i]["difference"] = difference;
                }
                else
                    dt.Rows[i]["difference"] = 0D;
            }
        }
        /***********************************************************************************************/
        private void CalculateDifference(DataTable dt)
        {
            double price = 0D;
            double currentprice = 0D;
            double difference = 0D;
            if (G1.get_column_number(dt, "difference") < 0)
                dt.Columns.Add("difference", Type.GetType("System.Double"));

            string zero = "";

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                price = dt.Rows[i]["price"].ObjToDouble();
                currentprice = dt.Rows[i]["currentprice"].ObjToDouble();
                zero = dt.Rows[i]["data"].ObjToString().ToUpper();
                if (price > currentprice)
                {
                    dt.Rows[i]["currentprice"] = price;
                    currentprice = price;
                }
                difference = currentprice - price;
                dt.Rows[i]["difference"] = difference;
            }
        }
        /***********************************************************************************************/
        private bool ReCalcTotalAll(DataTable dtt, ref double mainPackageDiscount, ref double allServices, ref double allMerchandise, ref double allCashAdvance, ref double salesTax, ref double allDiscount, ref double subtotal, ref double total)
        {
            if (dtt == null)
                return false;
            if (dtt.Rows.Count <= 0)
                return false;

            DataRow[] dRows = dtt.Select("type<>'Allow'");
            if (dRows.Length == 0)
                return false;

            DataTable dt = dRows.CopyToDataTable();



            string select = "";
            string ignore = "";
            string who = "";
            double price = 0D;
            double customerDiscount = 0D;
            string type = "";
            string deleted = "";
            double servicesTotal = 0D;
            double merchandiseTotal = 0D;
            double cashAdvanceTotal = 0D;

            double ignoreServices = 0D;
            double ignoreMerchandise = 0D;
            double ignoreCashAdvance = 0D;

            double totalListedPrice = 0D;
            double packagePrice = 0D;
            double packageDiscount = 0D;
            double totalUnselected = 0D;
            int packageDiscountRow = -1;

            //double salesTax = 0D;
            double tax = 0D;

            double grandTotal = 0D;
            double actualDiscount = 0D;
            string isPackage = "";

            if (G1.get_column_number(dt, "DELETED") < 0)
                dt.Columns.Add("DELETED");

            string currentPriceColumn = "currentprice";
            if (G1.get_column_number(dt, "currentprice") < 0)
            {
                if (G1.get_column_number(dt, "price1") >= 0)
                {
                    dt.Columns["price1"].ColumnName = "currentprice";
                }
                else
                    currentPriceColumn = "price";
            }
            if (G1.get_column_number(dt, "difference") < 0)
                dt.Columns.Add("difference", Type.GetType("System.Double"));

            gotPackage = FunServices.DoWeHavePackage(dt);
            string service = "";
            FunServices.PreProcessUrns(dt);
            double upgrade = 0D;

            FunServices.AddUpgrade(dt);

            double totalServices = 0D;
            double totalMerchandise = 0D;
            double totalCashAdvance = 0D;
            double difference = 0D;

            bool myPackage = FunServices.GetPackageDetails(dt, ref totalListedPrice, ref packageDiscount, ref packagePrice, ref totalServices, ref totalMerchandise, ref totalCashAdvance, ref actualDiscount, ref grandTotal);
            if (myPackage)
                currentPriceColumn = "price";

            string pSelect = "";
            double urnCredit = 0D;
            double alterCredit = 0D;
            double rentalCredit = 0D;

            bool allIsPackage = true;
            double added = 0D;
            double totalUpgrades = 0D;

            string zeroData = "";

            string database = G1.conn1.Database.ObjToString();

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    deleted = dt.Rows[i]["DELETED"].ObjToString().ToUpper();
                    if (deleted == "DELETED" || deleted == "D")
                        continue;

                    select = dt.Rows[i]["select"].ObjToString();
                    service = dt.Rows[i]["service"].ObjToString();
                    if (service == "Register Book And Pouch")
                    {
                    }
                    if (service == "The Alumina - Aluminum")
                    {
                    }
                    if (service == "Alternative Container")
                    {
                    }
                    if (service == "Basic Alternative Container - Cardboard")
                    {
                    }
                    if (service == "Cremation Casket Credit Or Rental Casket With Removable Insert")
                    {
                    }
                    type = dt.Rows[i]["type"].ObjToString().ToUpper();
                    ignore = dt.Rows[i]["ignore"].ObjToString();
                    who = dt.Rows[i]["who"].ObjToString();
                    price = dt.Rows[i]["price"].ObjToDouble();
                    upgrade = dt.Rows[i]["upgrade"].ObjToDouble();
                    if (upgrade > 0D)
                    {
                        totalUpgrades += upgrade;
                    }

                    if (type.ToUpper() == "CASH ADVANCE" && dt.Rows[i]["difference"].ObjToDouble() > 0D)
                    {
                    }
                    zeroData = dt.Rows[i]["data"].ObjToString();

                    //if (price > 0D)
                    //    zeroData = "";

                    if (myPackage)
                    {
                        isPackage = dt.Rows[i]["isPackage"].ObjToString().ToUpper();
                        if (isPackage == "P")
                        {
                            if (service.ToUpper().IndexOf("URN CREDIT") >= 0)
                            {
                                pSelect = dt.Rows[i]["pSelect"].ObjToString();
                                if (pSelect == "1")
                                    continue;
                                urnCredit = dt.Rows[i]["price"].ObjToDouble();
                                //dt.Rows[i]["price"] = 0D;
                            }
                            else if (service.ToUpper().IndexOf("ALTERNATIVE CONTAINER CREDIT") >= 0)
                            {
                                pSelect = dt.Rows[i]["pSelect"].ObjToString();
                                if (pSelect == "1")
                                    continue;
                                alterCredit = dt.Rows[i]["price"].ObjToDouble();
                                //dt.Rows[i]["price"] = 0D;
                            }
                            else if (service.ToUpper().IndexOf("CREMATION CASKET CREDIT") >= 0)
                            {
                                pSelect = dt.Rows[i]["pSelect"].ObjToString();
                                if (pSelect == "1")
                                    continue;
                                alterCredit = dt.Rows[i]["price"].ObjToDouble();
                                //dt.Rows[i]["price"] = 0D;
                            }
                            else if (service.ToUpper().IndexOf("CREMATION CASKET CREDIT OR RENTAL") >= 0)
                            {
                                pSelect = dt.Rows[i]["pSelect"].ObjToString();
                                if (pSelect == "1")
                                    continue;
                                rentalCredit = dt.Rows[i]["price"].ObjToDouble();
                                //dt.Rows[i]["price"] = 0D;
                            }
                            else
                            {
                                if (select == "1")
                                {
                                    price = dt.Rows[i]["price"].ObjToDouble();
                                    if (price <= 0D && upgrade <= 0D)
                                        continue;
                                    if (upgrade > 0D)
                                    {
                                        if (type.ToUpper() == "MERCHANDISE")
                                        {
                                            merchandiseTotal += upgrade;
                                            if (ignore == "Y")
                                                ignoreMerchandise += price;
                                        }
                                    }
                                    if (ignore == "Y")
                                    {
                                        if (type == "SERVICE")
                                            ignoreServices += price;
                                        else if (type == "MERCHANDISE" && upgrade <= 0D)
                                            ignoreMerchandise += price;
                                        else if (type == "CASH ADVANCE")
                                            ignoreCashAdvance += price;
                                    }
                                }
                                continue;
                            }
                        }
                        else
                        {
                            allIsPackage = false;
                            if (price > 0D)
                                added += price;
                        }
                    }
                    if (service.ToUpper() == "TOTAL LISTED PRICE")
                    {
                        if (select == "0")
                            continue;
                        packagePrice = dt.Rows[i]["price"].ObjToDouble();
                        totalListedPrice = packagePrice;
                        if (packagePrice > 0)
                            gotPackage = true;
                        continue;
                    }
                    else if (service.ToUpper() == "PACKAGE PRICE")
                    {
                        if (select == "0")
                            continue;
                        packagePrice = dt.Rows[i]["price"].ObjToDouble();
                        if (packagePrice > 0)
                            gotPackage = true;
                        continue;
                    }
                    else if (service.ToUpper() == "PACKAGE DISCOUNT")
                    {
                        if (select == "0")
                        {
                            mainPackageDiscount = 0D;
                            continue;
                        }
                        packageDiscount = dt.Rows[i]["price"].ObjToDouble();
                        packageDiscountRow = i;
                        customerDiscount = packageDiscount;
                        continue;
                    }

                    select = dt.Rows[i]["select"].ObjToString();
                    if (select == "0")
                    {
                        price = dt.Rows[i]["price"].ObjToDouble();
                        if (database.Trim().ToUpper() != "SMFS")
                        {
                            if (type.ToUpper() == "SERVICE")
                                servicesTotal += price;
                            else if (type.ToUpper() == "MERCHANDISE")
                                merchandiseTotal += price;
                            continue;
                        }
                        if (price < 0D)
                        {
                            price = Math.Abs(price);
                            difference = dt.Rows[i]["difference"].ObjToDouble();
                            customerDiscount += difference;
                            //if (type.ToUpper() == "SERVICE")
                            //    servicesTotal += price;
                            //else if (type.ToUpper() == "MERCHANDISE")
                            //    merchandiseTotal += price;
                            //else if (type.ToUpper() == "CASH ADVANCE")
                            //    cashAdvanceTotal += price;
                        }
                        else
                        {
                            price = dt.Rows[i]["difference"].ObjToDouble();
                            if (myPackage && price == 0D)
                                price = dt.Rows[i]["price"].ObjToDouble();
                            customerDiscount -= price;
                        }
                        continue;
                    }
                    if (select == "1")
                    {

                        tax = dt.Rows[i]["taxAmount"].ObjToDouble();
                        if (tax > 0D)
                        {
                            tax = G1.RoundValue(tax);
                            salesTax += tax;
                        }

                        type = dt.Rows[i]["type"].ObjToString();
                        upgrade = dt.Rows[i]["upgrade"].ObjToDouble();
                        price = dt.Rows[i]["price"].ObjToDouble();
                        if (service.IndexOf("(Refund)") > 0)
                        {
                            if (type.ToUpper() == "SERVICE")
                                servicesTotal += price;
                            else if (type.ToUpper() == "MERCHANDISE")
                                merchandiseTotal += price;
                            continue;
                        }
                        if (price <= 0D && upgrade <= 0D)
                        {
                            if (zeroData.ToUpper() != "ZERO")
                                continue;
                        }
                        price = dt.Rows[i][currentPriceColumn].ObjToDouble();
                        if (gotPackage)
                        {
                            price = dt.Rows[i]["price"].ObjToDouble();
                            price = Math.Abs(price);
                        }
                        customerDiscount += dt.Rows[i]["difference"].ObjToDouble();
                        if (type.ToUpper() == "SERVICE")
                        {
                            servicesTotal += price;
                            if (ignore == "Y")
                                ignoreServices += price;
                        }
                        else if (type.ToUpper() == "MERCHANDISE")
                        {
                            merchandiseTotal += price;
                            if (ignore == "Y")
                                ignoreMerchandise += price;
                        }
                        else if (type.ToUpper() == "CASH ADVANCE")
                        {
                            cashAdvanceTotal += price;
                            if (ignore == "Y")
                                ignoreCashAdvance += price;
                        }
                    }
                    else
                    {
                        type = dt.Rows[i]["type"].ObjToString().ToUpper();
                        if (gotPackage && type != "CASH ADVANCE")
                        {
                            upgrade = dt.Rows[i]["upgrade"].ObjToDouble();
                            price = dt.Rows[i]["price"].ObjToDouble();
                            if (price <= 0D && upgrade <= 0D)
                                continue;
                            price = dt.Rows[i][currentPriceColumn].ObjToDouble();
                            customerDiscount += dt.Rows[i]["difference"].ObjToDouble();
                            //if (type.ToUpper() == "SERVICE")
                            //    servicesTotal += price;
                            //else if (type.ToUpper() == "MERCHANDISE")
                            //    merchandiseTotal += price;
                            //else if (type.ToUpper() == "CASH ADVANCE")
                            //    cashAdvanceTotal += price;
                            totalUnselected += price;
                        }
                    }
                }
                catch (Exception ex)
                {
                }
            }
            try
            {
                if (!myPackage)
                {
                    customerDiscount = 0D;
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        deleted = dt.Rows[i]["DELETED"].ObjToString().ToUpper();
                        if (deleted == "DELETED" || deleted == "D")
                            continue;

                        select = dt.Rows[i]["select"].ObjToString();
                        price = dt.Rows[i]["price"].ObjToDouble();
                        upgrade = dt.Rows[i]["upgrade"].ObjToDouble();
                        zeroData = dt.Rows[i]["data"].ObjToString().ToUpper();
                        if (upgrade > 0D)
                        {

                        }
                        if (price <= 0D && upgrade > 0D)
                            price = upgrade;
                        if (price == 0D && zeroData != "ZERO")
                            continue;
                        price = dt.Rows[i]["difference"].ObjToDouble();
                        if (select == "1")
                            customerDiscount = customerDiscount + price;
                    }
                }
                double totalIgnore = ignoreServices + ignoreMerchandise + ignoreCashAdvance;

                string money = G1.ReformatMoney(servicesTotal + totalServices - ignoreServices);
                allServices = servicesTotal + totalServices - ignoreServices;
                //txtServices.Text = money;
                //txtServices.Refresh();

                money = G1.ReformatMoney(merchandiseTotal + totalMerchandise - ignoreMerchandise);
                allMerchandise = merchandiseTotal + totalMerchandise - ignoreMerchandise;
                //txtMerchandise.Text = money;
                //txtMerchandise.Refresh();

                money = G1.ReformatMoney(cashAdvanceTotal + totalCashAdvance - ignoreCashAdvance);
                allCashAdvance = cashAdvanceTotal + totalCashAdvance - ignoreCashAdvance;
                //txtCashAdvance.Text = money;
                //txtCashAdvance.Refresh();

                double actualCashAdvance = cashAdvanceTotal + totalCashAdvance - ignoreCashAdvance;

                //double subtotal = servicesTotal + merchandiseTotal + cashAdvanceTotal + totalCashAdvance + totalServices + totalMerchandise - totalIgnore;
                subtotal = servicesTotal + merchandiseTotal + cashAdvanceTotal + totalCashAdvance + totalServices + totalMerchandise - totalIgnore;
                money = G1.ReformatMoney(subtotal);
                //txtSubtotal.Text = money;
                //txtSubtotal.Refresh();

                //double total = subtotal;
                total = subtotal;
                if (gotPackage)
                {
                    //money = G1.ReformatMoney(actualDiscount + totalIgnore + urnCredit);
                    //txtDiscount.Text = money;
                    //txtDiscount.Refresh();
                    //total = packagePrice + cashAdvanceTotal + servicesTotal + merchandiseTotal - urnCredit;
                    //total = total + (actualDiscount + totalIgnore);

                    //total = subtotal + (actualDiscount + totalIgnore);

                    //total = packagePrice + added - urnCredit;
                    //money = G1.ReformatMoney(subtotal - total + urnCredit);
                    //txtDiscount.Text = money;
                    //txtDiscount.Refresh();
                    ////total = total + totalUpgrades - urnCredit;
                    //total = total - urnCredit;

                    money = G1.ReformatMoney(actualDiscount + totalIgnore);
                    allDiscount = actualDiscount + totalIgnore;
                    //txtDiscount.Text = money;
                    //txtDiscount.Refresh();
                    //total = packagePrice + cashAdvanceTotal + servicesTotal + merchandiseTotal - urnCredit;
                    //total = total + (actualDiscount + totalIgnore);

                    total = subtotal + (actualDiscount + totalIgnore);

                    total = packagePrice + added - urnCredit - alterCredit - rentalCredit;
                    //total = packagePrice + added;
                    money = G1.ReformatMoney(subtotal - total + urnCredit + alterCredit + rentalCredit);
                    money = G1.ReformatMoney(subtotal - total);
                    allDiscount = subtotal - total;
                    if (total < packagePrice)
                    {
                        double newDiscount = (subtotal - total) - (packagePrice - total);
                        allDiscount = newDiscount;
                        money = G1.ReformatMoney(newDiscount);
                        total = packagePrice;
                    }
                    else
                    {
                        double newDiscount = subtotal - total;
                        if (newDiscount > Math.Abs(actualDiscount))
                        {
                            allDiscount = actualDiscount;
                            money = G1.ReformatMoney(actualDiscount);
                            total = subtotal - Math.Abs(actualDiscount);
                        }
                    }
                    //txtDiscount.Text = money;
                    //txtDiscount.Refresh();
                    //total = total + totalUpgrades - urnCredit;
                    //total = total - urnCredit - alterCredit;

                }
                else
                {
                    if (customerDiscount > 0D)
                    {
                        double newDiscount = G1.RoundValue(customerDiscount - totalIgnore - totalUpgrades);
                        customerDiscount = newDiscount;
                    }
                    double discount = customerDiscount * -1D;
                    money = G1.ReformatMoney(discount);
                    allDiscount = discount;
                    //txtDiscount.Text = money;
                    //txtDiscount.Refresh();
                    total = total + discount;
                }
                total += salesTax;
                money = G1.ReformatMoney(total);
                //txtTotal.Text = money;
                //txtTotal.Refresh();

                money = G1.ReformatMoney(salesTax);
                //txtSalesTax.Text = money;
                //txtSalesTax.Refresh();
            }
            catch (Exception ex)
            {
            }

            //bool modified = FunServices.ProcessPackage(dt);
            return false;
        }
        /****************************************************************************************/
        private void ReCalcTotal(DataTable dt)
        {
            string select = "";
            double price = 0D;
            string type = "";
            double servicesTotal = 0D;
            double merchandiseTotal = 0D;
            double cashAdvanceTotal = 0D;
            //            DataTable dt = (DataTable)dgv.DataSource;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                select = dt.Rows[i]["select"].ObjToString();
                if (select == "1")
                {
                    type = dt.Rows[i]["type"].ObjToString();
                    price = dt.Rows[i]["price"].ObjToDouble();
                    if (type.ToUpper() == "SERVICE")
                        servicesTotal += price;
                    else if (type.ToUpper() == "MERCHANDISE")
                        merchandiseTotal += price;
                    else if (type.ToUpper() == "CASH ADVANCE")
                        cashAdvanceTotal += price;
                }
            }
            //string money = G1.ReformatMoney(servicesTotal);
            //txtServices.Text = money;
            //money = G1.ReformatMoney(merchandiseTotal);
            //txtMerchandise.Text = money;
            //money = G1.ReformatMoney(cashAdvanceTotal);
            //txtCashAdvance.Text = money;
            //double subtotal = servicesTotal + merchandiseTotal;
            //money = G1.ReformatMoney(subtotal);
            //txtSubtotal.Text = money;
            //double total = subtotal + cashAdvanceTotal;
            //money = G1.ReformatMoney(total);
            //txtTotal.Text = money;
        }
        /****************************************************************************************/
        private void Filter_CheckedChanged(object sender, EventArgs e)
        {
            string procLoc = "";
            if (chkAll.Checked)
            {
                procLoc = "";
            }
            else
            {
                if (chkService.Checked)
                {
                    if (procLoc.Trim().Length > 0)
                        procLoc += ",";
                    procLoc += "'Service'";
                }
                if (chkMerchandise.Checked)
                {
                    if (procLoc.Trim().Length > 0)
                        procLoc += ",";
                    procLoc += "'Merchandise'";
                }
                if (chkCashAdvance.Checked)
                {
                    if (procLoc.Trim().Length > 0)
                        procLoc += ",";
                    procLoc += "'Cash Advance'";
                }
            }
            if (String.IsNullOrWhiteSpace(procLoc))
            {
                DataTable dt = workDt.Copy();
                FilterZeros(dt);
                G1.NumberDataTable(dt);
                dgv.DataSource = dt;
            }
            else
            {
                string filter = " `type` IN (" + procLoc + ") ";
                DataRow[] dRows = workDt.Select(filter);
                DataTable dt = workDt.Clone();
                for (int i = 0; i < dRows.Length; i++)
                    dt.ImportRow(dRows[i]);
                FilterZeros(dt);
                G1.NumberDataTable(dt);
                dgv.DataSource = dt;
            }
        }
        /***********************************************************************************************/
        private double FilterZeros(DataTable dt)
        {
            if (!chkZeros.Checked)
                return 0D;
            double amount = 0D;
            string type = "";
            string deleted = "";
            double upgrade = 0D;
            string service = "";
            string pSelect = "";
            double urnCredit = 0D;
            double ac = 0D;
            string data = "";

            if (G1.get_column_number(dt, "DELETED") < 0)
                dt.Columns.Add("DELETED");

            string database = G1.conn1.Database.ObjToString();

            for (int i = dt.Rows.Count - 1; i >= 0; i--)
            {
                deleted = dt.Rows[i]["deleted"].ObjToString().ToUpper();
                if (deleted == "DELETED" || deleted == "D")
                {
                    dt.Rows.RemoveAt(i);
                    continue;
                }
                type = dt.Rows[i]["type"].ObjToString();
                if (type.ToUpper() == "PAYMENT")
                    continue;
                upgrade = dt.Rows[i]["upgrade"].ObjToDouble();
                amount = dt.Rows[i]["price"].ObjToDouble();
                if (database.ToUpper() != "SMFS")
                    continue;
                service = dt.Rows[i]["service"].ObjToString();
                if (service.ToUpper().IndexOf("PACKAGE DISCOUNT") >= 0)
                    continue;
                if (service.ToUpper().IndexOf("URN CREDIT") >= 0)
                {
                    pSelect = dt.Rows[i]["pSelect"].ObjToString();
                    if (pSelect == "0")
                        urnCredit = dt.Rows[i]["difference"].ObjToDouble();
                }

                if (service.ToUpper().IndexOf("ALTERNATIVE CONTAINER CREDIT") >= 0)
                {
                    pSelect = dt.Rows[i]["pSelect"].ObjToString();
                    if (pSelect == "0")
                        ac = dt.Rows[i]["difference"].ObjToDouble();
                }
                if (service.ToUpper().IndexOf("CREMATION CASKET CREDIT") >= 0)
                {
                    pSelect = dt.Rows[i]["pSelect"].ObjToString();
                    if (pSelect == "0")
                        ac = dt.Rows[i]["difference"].ObjToDouble();
                }

                if (amount <= 0D && upgrade <= 0D)
                {
                    data = dt.Rows[i]["data"].ObjToString().ToUpper();
                    if (data != "ZERO")
                        dt.Rows.RemoveAt(i);
                    else
                    {
                        //dt.Rows[i]["price"] = "0.00";
                        //dt.Rows[i]["price1"] = "0.00";
                        //dt.Rows[i]["pastPrice"] = "0.00";
                        //dt.Rows[i]["currentPrice"] = "0.00";
                    }
                }
            }
            alternateCredit = ac;
            return urnCredit;
        }
        /***********************************************************************************************/
        private int pageMarginLeft = 0;
        private int pageMarginRight = 0;
        private int pageMarginTop = 0;
        private int pageMarginBottom = 0;
        /***********************************************************************************************/
        //DevExpress.XtraPrinting.PrintableComponentLink printableComponentLink1 = null;
        DevExpress.XtraPrinting.PrintableComponentLink[] printableComponentLinks = new PrintableComponentLink[10];

        DevExpress.XtraPrinting.PrintingSystem printingSystem1 = null;
        DevExpress.XtraPrinting.PrintingSystem printingSystem2 = null;
        /***********************************************************************************************/
        private void printPreviewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.components = new System.ComponentModel.Container();
            DevExpress.XtraPrinting.PrintingSystem printingSystem3 = new DevExpress.XtraPrinting.PrintingSystem(this.components);

            DevExpress.XtraPrinting.PrintableComponentLink Link1 = null;
            Link1 = printPreview(sender, e);
        }
        /***********************************************************************************************/
        private DevExpress.XtraPrinting.PrintableComponentLink printPreview(object sender, EventArgs e)
        {
            //if (this.components == null)
            //    this.components = new System.ComponentModel.Container();

            //printingSystem1 = new DevExpress.XtraPrinting.PrintingSystem(this.components);

            //DevExpress.XtraPrinting.PrintableComponentLink printableComponentLink1 = null;
            //if (printableComponentLink1 != null)
            //{
            //    printableComponentLink1.ClearDocument();
            //    printableComponentLink1.Dispose();
            //}
            DevExpress.XtraPrinting.PrintableComponentLink printableComponentLink1 = new DevExpress.XtraPrinting.PrintableComponentLink(this.components);

            //printingSystem1.Links.AddRange(new object[] {
            //printableComponentLink1});

            if (dgv2.Visible)
                printableComponentLink1.Component = dgv2;
            else if (dgv3.Visible)
                printableComponentLink1.Component = dgv3;
            else if (grid.Visible)
                printableComponentLink1.Component = grid;
            else
                printableComponentLink1.Component = dgv;

            DataTable dt = (DataTable)dgv3.DataSource;
            DataTable testDt = dt.Clone();
            testDt.Rows.Clear();
            dgv2.DataSource = testDt;

            printableComponentLink1.Component = dgv2;

            Col2_TotalHeight = 0f;
            Col1_TotalHeight = 0f;

            //printableComponentLink1.PrintingSystemBase = printingSystem1;
            printableComponentLink1.CreateDetailHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateDetailHeaderArea);
            printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);
            printableComponentLink1.CreateMarginalFooterArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalFooterArea);
            printableComponentLink1.BeforeCreateAreas += new System.EventHandler(this.printableComponentLink1_BeforeCreateAreas);
            printableComponentLink1.AfterCreateAreas += new System.EventHandler(this.printableComponentLink1_AfterCreateAreas);
            printableComponentLink1.CreateReportFooterArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateReportFooterArea);

            printableComponentLink1.Landscape = false;

            //            Printer.setupPrinterMargins(50, 50, 195, 50); 
            Printer.setupPrinterMargins(50, 50, 195, 0);

            pageMarginLeft = Printer.pageMarginLeft;
            pageMarginRight = Printer.pageMarginRight;
            pageMarginTop = Printer.pageMarginTop;
            pageMarginBottom = Printer.pageMarginBottom;

            printableComponentLink1.Margins.Left = pageMarginLeft;
            printableComponentLink1.Margins.Right = pageMarginRight;
            printableComponentLink1.Margins.Top = pageMarginTop;
            printableComponentLink1.Margins.Bottom = pageMarginBottom;

            //printingSystem1.Document.AutoFitToPagesWidth = 1;

            //printableComponentLink1.CreateDocument();
            //printingSystem1.AddCommandHandler(new SaveDocumentCommandHandler());
            //printingSystem1.AddCommandHandler(new PrintDocumentCommandHandler());
            ////printingSystem1.AddCommandHandler(new ExportToImageCommandHandler());

            //if (printingSystem1.GetCommandVisibility(PrintingSystemCommand.Watermark) != CommandVisibility.None)
            //    printingSystem1.SetCommandVisibility(PrintingSystemCommand.Watermark, CommandVisibility.None);

            //if (printingSystem1.GetCommandVisibility(PrintingSystemCommand.PrintDirect) != CommandVisibility.None)
            //    printingSystem1.SetCommandVisibility(PrintingSystemCommand.PrintDirect, CommandVisibility.None);

            ////if (printingSystem1.CanExecCommand(PrintingSystemCommand.ViewWholePage))
            //printingSystem1.ExecCommand(PrintingSystemCommand.ViewWholePage);
            if (workActuallyPrint)
                printableComponentLink1.Print();
            else
                printableComponentLink1.ShowPreviewDialog();

            return printableComponentLink1;
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

            if (dgv2.Visible)
                printableComponentLink1.Component = dgv2;
            else if (dgv3.Visible)
                printableComponentLink1.Component = dgv3;
            else if (grid.Visible)
                printableComponentLink1.Component = grid;
            else
                printableComponentLink1.Component = dgv;

            printableComponentLink1.PrintingSystemBase = printingSystem1;
            printableComponentLink1.CreateDetailHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateDetailHeaderArea);
            printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);
            printableComponentLink1.BeforeCreateAreas += new System.EventHandler(this.printableComponentLink1_BeforeCreateAreas);
            printableComponentLink1.AfterCreateAreas += new System.EventHandler(this.printableComponentLink1_AfterCreateAreas);
            printableComponentLink1.CreateReportFooterArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateReportFooterArea);
            printableComponentLink1.Landscape = false;

            Printer.setupPrinterMargins(50, 50, 195, 50);

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
            //            GridView view = sender as GridView;
            DateTime date = DateTime.Now;
            string dateStr = "";
            DataTable dt = (DataTable)dgv.DataSource;
            //if (G1.get_column_number(dt, "dueDate8Print") < 0)
            //    dt.Columns.Add("dueDate8Print");
            //if (G1.get_column_number(dt, "payDate8Print") < 0)
            //    dt.Columns.Add("payDate8Print");
            //for (int i = 0; i < dt.Rows.Count; i++)
            //{
            //    date = dt.Rows[i]["dueDate8"].ObjToDateTime();
            //    dateStr = date.ToString("MM/dd/yyyy");
            //    dt.Rows[i]["dueDate8Print"] = dateStr;
            //    date = dt.Rows[i]["payDate8"].ObjToDateTime();
            //    dateStr = date.ToString("MM/dd/yyyy");
            //    dt.Rows[i]["payDate8Print"] = dateStr;
            //}
            //gridMain.Columns["dueDate8"].Visible = false;
            //gridMain.Columns["payDate8"].Visible = false;
            //gridMain.Columns["dueDate8Print"].Visible = true;
            //gridMain.Columns["payDate8Print"].Visible = true;
        }
        /***********************************************************************************************/
        private void printableComponentLink1_AfterCreateAreas(object sender, EventArgs e)
        {
            //gridMain.Columns["dueDate8"].Visible = true;
            //gridMain.Columns["payDate8"].Visible = true;
            //gridMain.Columns["dueDate8Print"].Visible = false;
            //gridMain.Columns["payDate8Print"].Visible = false;
        }
        /***********************************************************************************************/
        private void printableComponentLink1_CreateDetailHeaderArea(object sender, CreateAreaEventArgs e)
        {
        }
        /***********************************************************************************************/
        private int majorStartY = 0;
        private int majorHeaderY = 0;

        private CreateAreaEventArgs publicE = null;
        private bool printFirst = true;
        private bool printFirstToo = true;

        private void printableComponentLink1_CreateMarginalHeaderArea(object sender, CreateAreaEventArgs e)
        {
            if (printFirst)
                publicE = e;
            printFirst = false;

            e.Graph.DrawPageInfo(PageInfo.DateTime, "{0:hhhh:mmmm:ssss}", Color.Black, new RectangleF(0, 0, 200, 50), BorderSide.None);

            Printer.setupPrinterQuads(e, 50, 50);
            Font font = new Font("Ariel", 16);
            majorHeaderY = Printer.RichDrawQuad(1, 1, 49, 17, this.rtbAddress.Rtf, Color.Black, BorderSide.None, font, HorizontalAlignment.Left);
            Printer.DrawQuadBorder(1, 1, 50, 17, BorderSide.Bottom, 1, Color.Black);

            majorStartY = Printer.RichDrawQuad(2, 18, 49, 40, this.rtbStatementOfFuneral.Rtf, Color.Black, BorderSide.None, font, HorizontalAlignment.Left);
            Printer.DrawQuadBorder(1, 1, 50, 55, BorderSide.Bottom, 1, Color.Black);

            Printer.SetQuadSize(12, 12);
            //Printer.DrawQuadBorder(1, 1, 12, 6, BorderSide.All, 1, Color.Black);
            //Printer.DrawQuadBorder(12, 1, 1, 6, BorderSide.Right, 1, Color.Black);

            font = new Font("Ariel", 8);
            //            Printer.DrawGridDate(2, 1, 2, 3, Color.Black, BorderSide.None, font);
            //Printer.DrawGridPage(11, 1, 2, 3, Color.Black, BorderSide.None, font);

            //Printer.DrawQuad(1, 3, 2, 2, "User : " + LoginForm.username, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);

            //font = new Font("Ariel", 10, FontStyle.Bold);
            //Printer.DrawQuad(6, 3, 2, 3, "Daily Payment Report", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            //font = new Font("Ariel", 7, FontStyle.Regular);
            //Printer.DrawQuad(1, 5, 4, 1, "Contract :" + workContract, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(3, 5, 3, 1, "Name :" + workName, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            //Printer.DrawQuadBorder(1, 1, 12, 5, BorderSide.All, 1, Color.Black);
            //Printer.DrawQuadBorder(12, 1, 1, 6, BorderSide.Right, 1, Color.Black);

            //Printer.DrawQuad(1, 6, 3, 1, labelSerTot.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(1, 7, 3, 1, labelMerTot.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(1, 8, 3, 1, labDownPayment.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(1, 9, 3, 1, labRemainingBalance.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(1, 10, 3, 1, lblDueDate.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(1, 11, 3, 1, lblAPR.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            //Printer.DrawQuad(3, 6, 3, 1, lblPayment.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(3, 7, 3, 1, lblIssueDate.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(3, 8, 3, 1, lblNumPayments.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(3, 9, 3, 1, lblTotalPaid.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(3, 10, 3, 1, lblTrust85.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(3, 11, 3, 1, lblCalcTrust85.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            //Printer.DrawQuad(5, 6, 3, 1, labBalDue.Text + " " + labBalanceDue.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //if (!String.IsNullOrWhiteSpace(lblDeadDate.Text))
            //    Printer.DrawQuad(5, 7, 3, 1, lblDeadDate.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //if (!String.IsNullOrWhiteSpace(lblServiceId.Text))
            //    Printer.DrawQuad(5, 8, 3, 1, lblServiceId.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(5, 9, 3, 1, lblContractValue.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            //Printer.SetQuadSize(12, 12);
            //Printer.DrawQuadBorder(1, 1, 12, 11, BorderSide.All, 1, Color.Black);
            //Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);



            //Printer.DrawQuadBorder(1, 1, 12, 11, BorderSide.All, 1, Color.Black);
            ////            Printer.DrawQuadTicks();
        }
        /***********************************************************************************************/
        private void printableComponentLink1_CreateMarginalFooterArea(object sender, CreateAreaEventArgs e)
        {
            if (1 == 1)
                return;
            DevExpress.XtraPrinting.PrintableComponentLink link = (DevExpress.XtraPrinting.PrintableComponentLink)sender;
            link.Margins.Top = 10;
            link.Margins.Bottom = 500;
            Printer.setupPrinterMargins(100, 50, 300, 500); // Left, Right, Top, Bottom

            Printer.setupPrinterQuads(e, 80, 40); // X-Quads, Y-Quads
            Font font = new Font("Ariel", 16);
            DataTable dt = (DataTable)dgv3.DataSource;
            int maxRows = dt.Rows.Count;
            int rows = paymentsDt.Rows.Count;
            // X-Quad, Y-Quad, Width, Height
            int yOffset = 0;
            if (servicesLeftSide > servicesRightSide)
            {
                int diff = (servicesLeftSide - servicesRightSide) * 2;
                yOffset = (diff * -1) + 1;
            }
            yOffset = yOffset + 4;
            yOffset = 0;
            Printer.RichDrawQuad(41, 1 + yOffset, 40, 14, this.rtbDisclosures.Rtf, Color.Black, BorderSide.Top, font, HorizontalAlignment.Left);
            Printer.RichDrawQuad(41, 15 + yOffset, 40, 13, this.rtbDisclaimer.Rtf, Color.White, BorderSide.Top, font, HorizontalAlignment.Left);
            Printer.RichDrawQuad(41, 28 + yOffset, 40, 18, this.rtback.Rtf, Color.Black, BorderSide.Top, font, HorizontalAlignment.Left);
        }
        /***********************************************************************************************/
        private void printableComponentLink1_CreateReportFooterAreaxxx(object sender, CreateAreaEventArgs e)
        {
            if (1 == 1)
                return;
            //printableComponentLink1.Component.AcceptChanges();

            //DevExpress.XtraPrinting.PrintableComponentLink link = (DevExpress.XtraPrinting.PrintableComponentLink)sender;
            //link.Margins.Top = 10;
            //link.Margins.Bottom = 500;
            //Printer.setupPrinterMargins(100, 50, 300, 500); // Left, Right, Top, Bottom

            //Printer.setupPrinterQuads(e, 80, 40); // X-Quads, Y-Quads
            //Font font = new Font("Ariel", 16);
            //DataTable dt = (DataTable)dgv3.DataSource;
            //int maxRows = dt.Rows.Count;
            //int rows = paymentsDt.Rows.Count;
            //// X-Quad, Y-Quad, Width, Height
            //int yOffset = 0;
            ////if (servicesLeftSide > servicesRightSide)
            ////{
            ////    int diff = (servicesLeftSide - servicesRightSide) * 2;
            ////    yOffset = (diff * -1) + 1;
            ////}
            ////yOffset = yOffset + 4;

            ////yOffset = yOffset - servicesRightSide - 14;

            ////if (servicesRightSide < 16)
            ////    servicesRightSide = 16;
            ////yOffset = -69 + (servicesRightSide * 2) + 6;
            ////yOffset = -69;

            //int y = Printer.pageMarginTop - Printer.pageTopBorder;
            //y = 0;

            //int newY = Printer.RichDrawQuadLine(41, y + 1, 40, 14, this.rtbDisclosures.Rtf, Color.Black, BorderSide.Top, font, HorizontalAlignment.Left);
            //newY = Printer.RichDrawQuadLine(41, newY, 40, 13, this.rtbDisclaimer.Rtf, Color.White, BorderSide.Top, font, HorizontalAlignment.Left);
            //newY = Printer.RichDrawQuadLine(41, newY, 40, 18, this.rtback.Rtf, Color.Black, BorderSide.Top, font, HorizontalAlignment.Left);

            //if (servicesLeftSide > servicesRightSide)
            //{
            //    int diff = (servicesLeftSide - servicesRightSide) * 2;
            //    yOffset = (diff * -1) + 1;
            //}

            //int myOffset = 60;
            //if ( yOffset < 0 )
            //{
            //    myOffset = myOffset + yOffset + 6;
            //}
            ////yOffset = yOffset + 14 + 13 + 18;
            ////int totalOffset = yOffset;
            //yOffset = myOffset;

            //int irows = allDt[7].Rows.Count;

            ////yOffset = yOffset - 8 - irows + 5;

            //yOffset = yOffset - (irows*2) - 1;
            ////yOffset = -69 + 91;

            //newY += 10;

            ////Printer.RichDrawQuadPicture(1, yOffset, 40, 6, this.rtbSig1, Color.White, BorderSide.None, font, HorizontalAlignment.Left);
            ////Printer.RichDrawQuadPicture(41, yOffset, 40, 6, this.rtbSig2, Color.White, BorderSide.None, font, HorizontalAlignment.Left);

            ////yOffset = yOffset - 8;
            //newY = 675;
            //newY = Printer.RichDrawQuadLine(1, newY, 80, 25, this.rtbFinale.Rtf, Color.White, BorderSide.None, font, HorizontalAlignment.Left);
            ////Printer.MyRichDrawQuad(0, 349, 720, 175, this.rtbFinale.Rtf, Color.White, BorderSide.None, font, HorizontalAlignment.Left);

            ////for (int i = 0; i < imageCount; i++)
            ////{
            ////    yOffset -= 5;
            ////    Printer.DrawPagePicture(Images[i], 1, yOffset, 7, 4, Color.White, BorderSide.None, font, HorizontalAlignment.Left);
            ////}


            ////int totalWidth = (int)Printer.localE.Graph.ClientPageSize.Width;
            //int totalHeight = (int)Printer.localE.Graph.ClientPageSize.Height + 200;

            ////Printer.setupPrinterMargins(0, totalWidth, totalHeight, totalHeight); // Left, Right, Top, Bottom

            ////Printer.setupPrinterQuads(e, 80, 40); // X-Quads, Y-Quads

            ////Printer.DrawQuadBorder(10, 1, 50, 30, BorderSide.All, 1, Color.Red);

        }
        /***********************************************************************************************/
        private void printableComponentLink1_CreateReportFooterAreaOld(object sender, CreateAreaEventArgs e)
        {
            DevExpress.XtraPrinting.PrintableComponentLink link = (DevExpress.XtraPrinting.PrintableComponentLink)sender;
            link.Margins.Top = 10;
            link.Margins.Bottom = 500;
            Printer.setupPrinterMargins(100, 50, 300, 500); // Left, Right, Top, Bottom

            Font font = new Font("Ariel", 16);
            Printer.setupPrinterQuads(e, 80, 40); // X-Quads, Y-Quads

            DataTable dt = (DataTable)dgv3.DataSource;
            int maxRows = dt.Rows.Count;
            int rows = allDt[7].Rows.Count - 5;
            if (rows <= 3)
                rows = 4;
            else
                rows = 0;
            rows = SecondRows;
            // X-Quad, Y-Quad, Width, Height
            int yOffset = 0;
            majorStartY = 0;
            majorHeaderY = 0;

            //if (servicesLeftSide > servicesRightSide)
            //{
            //    int diff = (servicesLeftSide - servicesRightSide) * 2;
            //    yOffset = (diff * -1) + 1;
            //}
            int zeroMajor = 0;
            yOffset = yOffset + 4;
            int h = Convert.ToInt32(Col2_TotalHeight);
            if (Col2_TotalHeight < Col1_TotalHeight)
            {
                h = Convert.ToInt32(Col1_TotalHeight - Col2_TotalHeight);
                //h = Convert.ToInt32(Col2_TotalHeight) - majorHeaderY - 50;
                h -= (rows + 10) * 13;
                //if (rows <= 0)
                //    h -= 40;
                zeroMajor = h;
                //h -= rows * 13;
                //h -= Convert.ToInt32(Col2_TotalHeight);
                //h = Convert.ToInt32(Col1_TotalHeight) - majorStartY - Convert.ToInt32(Col2_TotalHeight);
                //zeroMajor = Convert.ToInt32(Col1_TotalHeight) - majorStartY;
                //zeroMajor = Convert.ToInt32(Col2_TotalHeight) + balanceDueY;
                //h = zeroMajor;
            }
            else
                h = Convert.ToInt32(Col2_TotalHeight);

            yOffset = 0;
            if (h > 0)
                yOffset = 0 - h;
            //yOffset = 0 - majorStartY + Convert.ToInt32(Col2_TotalHeight);
            //if (rows == 0)
            //    h += 40;
            //else if (h <= 70)
            //    h += 20;

            int diff = yOffset;

            //yOffset = yOffset;


            yOffset = Printer.RichDrawQuadLine(41, 1 + yOffset, 40, 14, this.rtbDisclosures.Rtf, Color.Black, BorderSide.Top, font, HorizontalAlignment.Left);
            yOffset = Printer.RichDrawQuadLine(41, yOffset, 40, 13, this.rtbDisclaimer.Rtf, Color.White, BorderSide.Top, font, HorizontalAlignment.Left);
            yOffset = Printer.RichDrawQuadLine(41, yOffset, 40, 18, this.rtback.Rtf, Color.Black, BorderSide.Top, font, HorizontalAlignment.Left);

            diff = yOffset - diff;

            //if (servicesLeftSide > servicesRightSide)
            //{
            //    int diff = (servicesLeftSide - servicesRightSide) * 2;
            //    yOffset = (diff * -1) + 1;
            //}

            //int myOffset = 60;
            //if (yOffset < 0)
            //{
            //    myOffset = myOffset + yOffset + 6;
            //}
            ////yOffset = yOffset + 14 + 13 + 18;
            ////int totalOffset = yOffset;
            //yOffset = myOffset;

            //int irows = allDt[7].Rows.Count;

            ////yOffset = yOffset - 8 - irows + 5;

            //yOffset = yOffset - (irows * 2) + 2;
            ////yOffset = -69 + 91;

            //Printer.RichDrawQuadPicture(1, yOffset, 40, 6, this.rtbSig1, Color.White, BorderSide.None, font, HorizontalAlignment.Left);
            //Printer.RichDrawQuadPicture(41, yOffset, 40, 6, this.rtbSig2, Color.White, BorderSide.None, font, HorizontalAlignment.Left);

            //yOffset = yOffset - 8;
            //yOffset = yOffset + 20;
            //if (yOffset > 160)
            //    yOffset = 160;

            Col2_TotalHeight += (float)diff;
            if (Col2_TotalHeight > Col1_TotalHeight)
                h = Convert.ToInt32(Col2_TotalHeight);
            else
                h = Convert.ToInt32(Col1_TotalHeight);
            if (yOffset < 0)
            {
                yOffset = 0 - h + majorStartY + majorHeaderY + 50 + 635;
                yOffset = 0 - h + majorStartY - majorHeaderY + 50 + 635;
            }
            //yOffset = 0 - zeroMajor;
            //yOffset = 0;
            if (yOffset < 0)
                yOffset = 0;
            if (Col1_TotalHeight >= 700F)
                yOffset = 0;

            yOffset = Printer.RichDrawQuadLine(1, yOffset + 4, 80, 25, this.rtbFinale.Rtf, Color.White, BorderSide.None, font, HorizontalAlignment.Left);




            //Printer.MyRichDrawQuad(0, 349, 720, 175, this.rtbFinale.Rtf, Color.White, BorderSide.None, font, HorizontalAlignment.Left);

            //for (int i = 0; i < imageCount; i++)
            //{
            //    yOffset -= 5;
            //    Printer.DrawPagePicture(Images[i], 1, yOffset, 7, 4, Color.White, BorderSide.None, font, HorizontalAlignment.Left);
            //}


            //int totalWidth = (int)Printer.localE.Graph.ClientPageSize.Width;
            int totalHeight = (int)Printer.localE.Graph.ClientPageSize.Height + 200;

        }
        /***********************************************************************************************/
        //private void printableComponentLink1_CreateReportFooterArea(object sender, CreateAreaEventArgs e)
        //{
        //    DevExpress.XtraPrinting.PrintableComponentLink link = (DevExpress.XtraPrinting.PrintableComponentLink)sender;
        //    link.Margins.Top = 10;
        //    link.Margins.Bottom = 500;
        //    Printer.setupPrinterMargins(100, 50, 300, 500); // Left, Right, Top, Bottom

        //    Printer.setupPrinterQuads(e, 80, 40); // X-Quads, Y-Quads
        //    Font font = new Font("Ariel", 16);
        //    // X-Quad, Y-Quad, Width, Height
        //    Printer.RichDrawQuad(41, 1, 40, 14, this.rtbDisclosures.Rtf, Color.Black, BorderSide.Top, font, HorizontalAlignment.Left);
        //    Printer.RichDrawQuad(41, 15, 40, 13, this.rtbDisclaimer.Rtf, Color.White, BorderSide.Top, font, HorizontalAlignment.Left);
        //    Printer.RichDrawQuad(41, 28, 40, 18, this.rtback.Rtf, Color.Black, BorderSide.Top, font, HorizontalAlignment.Left);


        //    Printer.RichDrawQuadPicture(1, 46, 40, 10, this.rtbSig1, Color.White, BorderSide.None, font, HorizontalAlignment.Left);
        //    Printer.RichDrawQuadPicture(41, 46, 40, 10, this.rtbSig2, Color.White, BorderSide.None, font, HorizontalAlignment.Left);


        //    Printer.RichDrawQuad(1, 56, 80, 40, this.rtbFinale.Rtf, Color.White, BorderSide.None, font, HorizontalAlignment.Left);
        //}
        /****************************************************************************************/
        private void chkZeros_CheckedChanged(object sender, EventArgs e)
        {
            dgv.RefreshDataSource();
            gridMain.RefreshData();
            dgv.Refresh();
            this.Refresh();
        }
        /****************************************************************************************/
        private void gridMain_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {

        }
        /****************************************************************************************/
        private Image GetSignature(string what)
        {
            Image signature = new Bitmap(1, 1);
            using (SignatureForm signatureForm = new SignatureForm(what, signature))
            {
                if (signatureForm.ShowDialog() == DialogResult.OK)
                {
                    signature = signatureForm.SignatureResult;
                }
                else
                    signature = null;
            }
            return signature;
        }
        /****************************************************************************************/
        private void enterSignatureOrPurchaserToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Image signature = new Bitmap(1, 1);
            using (SignatureForm signatureForm = new SignatureForm("Enter Signature of Purchaser", signature))
            {
                if (signatureForm.ShowDialog() == DialogResult.OK)
                {
                    signature = signatureForm.SignatureResult;
                    AddSignature(signature, rtbSig1);
                }
            }
        }
        /****************************************************************************************/
        private void enterSignatureOfCoPurchaserToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Image signature = new Bitmap(1, 1);
            using (SignatureForm signatureForm = new SignatureForm("Enter Signature of Co-Purchaser", signature))
            {
                if (signatureForm.ShowDialog() == DialogResult.OK)
                {
                    signature = signatureForm.SignatureResult;
                    AddSignature(signature, rtbSig2);
                }
            }
        }
        /****************************************************************************************/
        private void AddSignature(Image signature, RichTextBoxEx rtb)
        {
            string text = "[%SIG1%";
            //            RichTextBoxEx rtb = this.rtbFinale;
            //            rtb.ScrollToCaret();
            if (signature != null)
            {
                //Clipboard.SetImage(signature);
                rtb.SelectionStart = 0;
                //rtb.Paste();
                PictureBox pic = new PictureBox();
                pic.Image = signature;
                pic.Image = ScaleImage(pic.Image, 0.30F, 0.30F);
                pic.SizeMode = PictureBoxSizeMode.AutoSize;
                rtb.Controls.Add(pic);
                //                rtb.InsertImage(signature);
                //picPurchaser.Image = signature;
                //picCoPurchaser.Image = signature;
                //                pic.Image = signature;
            }
        }
        /****************************************************************************************/
        private void chkShowSummary_CheckedChanged(object sender, EventArgs e)
        {
            if (chkShowSummary.Checked)
            {
                //if (firstSummary)
                BuildSummary();
                dgv.Visible = false;
                dgv2.Visible = true;
            }
            else
            {
                dgv2.Visible = false;
                dgv.Visible = true;
            }
        }
        /****************************************************************************************/
        private void BuildSummary()
        {
            firstSummary = false;
            gridMain.OptionsView.AutoCalcPreviewLineCount = true;
            GridView View = gridMain2 as GridView;
            View.OptionsView.RowAutoHeight = true;

            DataTable dx = new DataTable();
            dx.Columns.Add("description");
            dx.Columns.Add("details");
            dx.Columns.Add("1font");
            dx.Columns.Add("1size", Type.GetType("System.Double"));
            dx.Columns.Add("1font2");
            dx.Columns.Add("1size2", Type.GetType("System.Double"));

            dx.Columns.Add("description2");
            dx.Columns.Add("details2");
            dx.Columns.Add("2font");
            dx.Columns.Add("2size", Type.GetType("System.Double"));
            dx.Columns.Add("2font2");
            dx.Columns.Add("2size2", Type.GetType("System.Double"));

            LoadServicesTable(dx);

            dgv2.DataSource = dx;
            dgv2.Visible = false;
            dgv3.DataSource = dx;
            dgv3.Dock = DockStyle.Fill;
            dgv3.Visible = true;

            panelDetailTop.Hide();

            rtbr.Dock = DockStyle.Fill;
            rtbr.Visible = false;

            //this.grid = new MinRowHeightGridControl();
            //grid.Dock = DockStyle.Fill;
            //grid.MainView = new MinRowHeightGridView(grid);
            //this.panelDetailBottom.Controls.Add(this.grid);
            //Controls.Add(grid);
            //grid.DataSource = dx;
            //grid.BringToFront();

        }
        /***********************************************************************************************/
        public static void SetColumnPosition(DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gridView, string name, int position)
        {
            try
            {
                BandedGridColumn column = gridView.Columns[name];
                column.Visible = true;
                gridView.SetColumnPosition(column, 0, position);
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Setting Column Position for Column " + name + " Position " + position.ToString() + "!");
            }
        }
        /***********************************************************************************************/
        private void LoadServicesTable(DataTable maindt)
        {
            DateTime start = DateTime.Now;
            float tinyFontSize = 2.5F;
            float smallFontSize = 7.5F;
            float largeFontSize = 8.5F;
            allDt = new DataTable[9];
            for (int i = 0; i < 9; i++)
            {
                allDt[i] = new DataTable();
                allDt[i].Columns.Add("service");
                allDt[i].Columns.Add("data");
                allDt[i].Columns.Add("font");
                allDt[i].Columns.Add("size", Type.GetType("System.Double"));
                allDt[i].Columns.Add("font2");
                allDt[i].Columns.Add("size2", Type.GetType("System.Double"));
                allDt[i].Columns.Add("ignore");
                if (i == 0)
                {
                    AppendToTable(allDt[i], "A. CHARGE FOR SERVICES:", "", "Arial Black", largeFontSize);
                    AppendToTable(allDt[i], "Professional Services:", "", "Arial Black", smallFontSize);
                }
                else if (i == 1)
                    AppendToTable(allDt[i], "Additional Services and Fees:", "", "Arial Black", smallFontSize);
                else if (i == 2)
                    AppendToTable(allDt[i], "Automotive Equipment:", "", "Arial Black", smallFontSize);
                else if (i == 3)
                    AppendToTable(allDt[i], "B. CHARGE FOR MERCHANDISE:", "", "Arial Black", smallFontSize);
                else if (i == 4)
                    AppendToTable(allDt[i], "C. SPECIAL CHARGES:", "", "Arial Black", smallFontSize);
                else if (i == 5)
                    AppendToTable(allDt[i], "D. CASH ADVANCE:", "", "Arial Black", smallFontSize);
                else if (i == 6)
                    AppendToTable(allDt[i], "SUMMARY OF CHARGES:", "", "Arial Black", largeFontSize);
                else if (i == 7)
                {
                    if (gotDiscounts)
                        AppendToTable(allDt[i], "ALLOWANCES:", "", "Arial Black", largeFontSize);
                }
                else if (i == 8)
                {
                    if (gotPayments)
                    {
                        if (gotDiscounts)
                            AppendToTable(allDt[i], "", "", "Dubai Medium", tinyFontSize);
                        AppendToTable(allDt[i], "PAYMENTS:", "", "Arial Black", largeFontSize);
                    }
                }
            }

            string cmd = "Select * from `fcust_services` where `contractNumber` = '" + workContract + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (serviceDt != null) // RAMMA ZAMMA
            {
                dt = serviceDt;
                string sel = "";
                string pSel = "";
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (dt.Rows[i]["mod"].ObjToString() == "1")
                    {
                        sel = dt.Rows[i]["select"].ObjToString();
                        pSel = dt.Rows[i]["pSelect"].ObjToString();
                        if (String.IsNullOrWhiteSpace(sel) && String.IsNullOrWhiteSpace(pSel))
                        {
                            dt.Rows[i]["select"] = "1";
                            dt.Rows[i]["pSelect"] = "1";
                        }
                    }
                }
            }

            //double mainPackageDiscount = 0D;
            //double allServices = 0D;
            //double allMerchandise = 0D;
            //double allCashAdvance = 0D;
            //double allSalesTax = 0D;
            //double allDiscount = 0D;
            //double allSubTotal = 0D;
            //double allTotal = 0D;


            FunServices.PreProcessUrns(dt);

            if (G1.get_column_number(dt, "currentprice") < 0)
            {
                if (G1.get_column_number(dt, "price1") >= 0)
                {
                    dt.Columns["price1"].ColumnName = "currentprice";
                }
            }


            int count = 0;
            string service = "";
            string data = "";
            string zeroData = "";
            double dValue = 0D;
            string localText = "";
            string type = "";
            string type1 = "";
            string modifiedType = "";
            double price = 0D;
            double currentPrice = 0D;
            string packageName = "";

            DataTable funDt = null;
            double total = 0D;
            string group = EditCustomer.activeFuneralHomeGroup;
            if (String.IsNullOrWhiteSpace(group))
                group = "GROUP 3 GPL";

            FunServices.PreProcessUrns(dt);

            DataRow[] dRows = dt.Select("isPackage='P'");
            bool isPackage = false;
            if (dRows.Length > 0)
                isPackage = true;
            bool gotPackage = FunServices.DoWeHavePackage(dt);

            string select = "";
            string pSelect = "";
            string ignore = "";
            double packageDiscount = 0D;
            double packagePrice = 0D;
            double totalListedPrice = 0D;
            double newPackageDiscount = 0D;
            double upgrade = 0D;
            double tax = 0D;
            double salesTax = 0D;

            string database = G1.conn1.Database.ObjToString();

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                funDt = null;
                service = dt.Rows[i]["service"].ObjToString();
                ignore = dt.Rows[i]["ignore"].ObjToString();
                modifiedType = dt.Rows[i]["type"].ObjToString();
                if (modifiedType.ToUpper() == "PAYMENT")
                {

                }
                if (String.IsNullOrWhiteSpace(service))
                    continue;
                if (service.ToUpper() == "ACKNOLEDGEMENT CARDS")
                    service = "ACKNOWLEDGEMENT CARDS";

                if (service.ToUpper() == "TOTAL LISTED PRICE")
                {
                    totalListedPrice = dt.Rows[i]["price"].ObjToDouble();
                    continue;
                }
                else if (service.ToUpper() == "PACKAGE PRICE")
                {
                    packagePrice = dt.Rows[i]["price"].ObjToDouble();
                    continue;
                }
                else if (service.ToUpper().IndexOf("PACKAGE DISCOUNT") > 0)
                {
                    if (service.ToUpper().IndexOf("CLASS A") < 0)
                        newPackageDiscount = dt.Rows[i]["price"].ObjToDouble();
                }
                else if (service.ToUpper() == "PACKAGE DISCOUNT")
                    continue;

                if (isPackage)
                {
                    if (modifiedType.ToUpper() != "ALLOW" && modifiedType.ToUpper() != "PAYMENT")
                    {
                        packageName = "";
                        if (G1.get_column_number(dt, "PackageName") >= 0)
                            packageName = dt.Rows[i]["PackageName"].ObjToString();
                        //if ( !String.IsNullOrWhiteSpace ( packageName ))
                        select = dt.Rows[i]["pSelect"].ObjToString();
                        if (select != "1")
                        {
                            if (modifiedType.ToUpper() == "MERCHANDISE" && !gotPackage)
                            {
                                select = dt.Rows[i]["select"].ObjToString();
                                if (select != "1")
                                    continue;
                            }
                            else
                                continue;
                        }
                    }
                }

                tax = dt.Rows[i]["taxAmount"].ObjToDouble();
                if (tax > 0D)
                {
                    tax = G1.RoundValue(tax);
                    salesTax += tax;
                }

                if (service.ToUpper().IndexOf("UTILITY") >= 0)
                {
                }

                cmd = "Select * from `services` where `service` = '" + service + "';";
                cmd = "Select * from `funeral_gplgroups` g LEFT JOIN `funeral_master` s on g.`service` = s.`service` where g.`groupname` = '" + group + "' AND g.`service` = '" + service + "';";

                DataTable dx = G1.get_db_data(cmd);
                if (dx.Rows.Count <= 0)
                {
                    dx = dt.Clone();
                    dx.ImportRow(dt.Rows[i]);
                }
                if (dx.Rows.Count > 0)
                {
                    type = dx.Rows[0]["type"].ObjToString();
                    type1 = type;
                    if (G1.get_column_number(dx, "type1") >= 0)
                        type1 = dx.Rows[0]["type1"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(type1))
                        type = type1;
                    if (!String.IsNullOrWhiteSpace(modifiedType))
                        type = modifiedType;

                    service = dx.Rows[0]["service"].ObjToString().ToUpper();
                    if (type.ToUpper() == "SERVICE")
                        type = QualifyServices(type, service);

                    if (type.ToUpper() == "SERVICE")
                        funDt = allDt[0];
                    else if (type.ToUpper() == "ADDITIONAL")
                        funDt = allDt[1];
                    else if (type.ToUpper() == "AUTOMOTIVE")
                        funDt = allDt[2];
                    else if (type.ToUpper() == "MERCHANDISE")
                        funDt = allDt[3];
                    else if (type.ToUpper() == "SPECIAL")
                        funDt = allDt[4];
                    else if (type.ToUpper() == "CASH ADVANCE")
                        funDt = allDt[5];
                    else if (type.ToUpper() == "DISCOUNT")
                        funDt = allDt[7];
                    else if (type.ToUpper() == "ALLOW")
                        funDt = allDt[7];
                    else if (type.ToUpper() == "PAYMENT")
                        funDt = allDt[8];
                }
                if (funDt == null)
                    funDt = allDt[0];
                data = dt.Rows[i]["data"].ObjToString();
                if (serviceDt != null)
                {
                    data = dt.Rows[i]["price"].ObjToString();
                    price = data.ObjToDouble();
                    upgrade = dt.Rows[i]["upgrade"].ObjToDouble();
                    //if ( upgrade > 0D )
                    //    price += upgrade;
                    if (type.ToUpper() != "PAYMENT" && type.ToUpper() != "ALLOW")
                    {
                        currentPrice = dt.Rows[i]["currentPrice"].ObjToDouble();
                        if (currentPrice > 0D)
                        {
                            if (currentPrice < price && price > 0D)
                                data = price.ObjToString();
                            else
                                data = dt.Rows[i]["currentPrice"].ObjToString();
                        }
                    }
                    if (database.ToUpper() != "SMFS")
                        data = dt.Rows[i]["price"].ObjToString();
                }
                if (!G1.validate_numeric(data))
                    continue;
                dValue = data.ObjToDouble();
                if (type.ToUpper() != "PAYMENT" && type.ToUpper() != "ALLOW")
                {
                    if (dValue == 0D)
                        continue;
                }
                //                total += dValue;

                //                AppendToTable(funDt, "   " + service, data, "Arial", 7f);
                zeroData = dt.Rows[i]["data"].ObjToString();
                //if (zeroData.ToUpper() == "ZERO")
                //    data = "Zero";
                AppendToTable(funDt, "   " + service, data, "Arial", smallFontSize, "Lucida Console", smallFontSize, ignore);
            }

            packageDiscount = totalListedPrice - packagePrice;
            double packageDifference = packageDiscount - newPackageDiscount;

            string str = "";
            SortServicesTable(allDt[0]);
            SortServicesTable(allDt[1]);

            allDt[3] = ReorderMerchandise(allDt[3]);

            double ignoreProcessions = 0D;
            double ignoreAdditional = 0D;
            double ignoreAuto = 0D;
            double ignoreMerchandise = 0D;
            double ignoreSpecial = 0D;
            double ignoreCashAdvance = 0D;

            double professionalServices = TotalUpTable(allDt[0], ref ignoreProcessions);
            double additionalServices = TotalUpTable(allDt[1], ref ignoreAdditional);
            double automotiveServices = TotalUpTable(allDt[2], ref ignoreAuto);
            double merchandice = TotalUpTable(allDt[3], ref ignoreMerchandise);
            double specialCharges = TotalUpTable(allDt[4], ref ignoreSpecial);
            double cashAdvance = TotalUpTable(allDt[5], ref ignoreCashAdvance);

            double totalIgnore = ignoreProcessions + ignoreAdditional + ignoreAuto + ignoreMerchandise + ignoreSpecial + ignoreCashAdvance;

            double totalServices = professionalServices + additionalServices + automotiveServices;
            double totalCredit = GetTotalCredits();

            double totalTotal = totalServices + merchandice + specialCharges + cashAdvance + salesTax;
            TotalPackage = totalTotal - totalIgnore;
            TotalPackage = totalTotal;

            data = G1.ReformatMoney(totalServices - (ignoreProcessions + ignoreAdditional + ignoreAuto));
            data = G1.ReformatMoney(totalServices);
            data = data.PadLeft(12);
            AppendToTable(allDt[6], "   A. CHARGES FOR SERVICES:", data, "Arial", smallFontSize, "Lucida Console", smallFontSize);
            data = G1.ReformatMoney(merchandice - ignoreMerchandise);
            data = G1.ReformatMoney(merchandice);
            data = data.PadLeft(12);
            AppendToTable(allDt[6], "   B. CHARGES FOR MERCHANDISE:", data, "Arial", smallFontSize, "Lucida Console", smallFontSize);
            data = G1.ReformatMoney(specialCharges - ignoreSpecial);
            data = G1.ReformatMoney(specialCharges);
            data = data.PadLeft(12);
            AppendToTable(allDt[6], "   C. SPECIAL CHARGES:", data, "Arial", smallFontSize, "Lucida Console", smallFontSize);
            data = G1.ReformatMoney(cashAdvance - ignoreCashAdvance);
            data = G1.ReformatMoney(cashAdvance);
            data = data.PadLeft(12);
            AppendToTable(allDt[6], "   D. CASH ADVANCES:", data, "Arial", smallFontSize, "Lucida Console", smallFontSize);
            data = G1.ReformatMoney(salesTax);
            data = data.PadLeft(12);
            AppendToTable(allDt[6], "   E. SALES TAX, IF APPLICABLE :", data, "Arial", smallFontSize, "Lucida Console", smallFontSize);
            AppendToTable(allDt[6], "", "", "Arial", smallFontSize, "Lucida Console", smallFontSize);
            data = G1.ReformatMoney(totalTotal - totalIgnore);
            data = G1.ReformatMoney(totalTotal);
            data = data.PadLeft(12);
            AppendToTable(allDt[6], "      TOTAL FUNERAL HOME CHARGES", data, "Arial Black", largeFontSize, "Lucida Console", smallFontSize);

            double balanceDue = TotalPackage - totalCredit;
            balanceDue = TotalPackage - Math.Abs(newTotalAllowances) - newPayments;
            //if (gotPackage && balanceDue < packagePrice) // Removed cuz of EV22042
            //{
            //    totalAllowances = totalAllowances - (packagePrice - balanceDue);
            //    balanceDue = packagePrice;
            //    dRows = allDt[7].Select ("service LIKE '%Package Discount%'");
            //    if (dRows.Length > 0)
            //        dRows[0]["data"] = totalAllowances.ToString();
            //}
            if (gotDiscounts)
            {
                data = G1.ReformatMoney(totalAllowances);
                data = G1.ReformatMoney(newTotalAllowances);
                data = data.PadLeft(12);
                AppendToTable(allDt[7], "                                  TOTAL ALLOWANCES ", data, "Arial", smallFontSize, "Lucida Console", smallFontSize);
            }

            if (gotPayments)
            {
                double aTotal = totalCredit - totalAllowances - packageDifference;
                aTotal = newPayments;
                data = G1.ReformatMoney(aTotal);
                data = data.PadLeft(12);
                AppendToTable(allDt[8], "                                  TOTAL CREDIT ", data, "Arial", smallFontSize, "Lucida Console", smallFontSize);
            }

            //double balanceDue = totalTotal - totalCredit;
            //balanceDue = TotalPackage - totalCredit;

            //if (gotPackage)
            //{
            //    double tempBalance = PackagePrice + (TotalPackage - totalListedPrice);
            //    //balanceDue = PackagePrice + (TotalPackage - totalListedPrice);
            //    if (tempBalance < PackagePrice)
            //        balanceDue = PackagePrice;
            //}

            if (gotDiscounts || gotPayments)
            {
                AppendToTable(allDt[8], "       ", " ", "Arial Black", tinyFontSize, "Lucida Console", tinyFontSize);
            }
            data = G1.ReformatMoney(balanceDue);
            data = data.PadLeft(12);
            AppendToTable(allDt[8], "        BALANCE DUE ", data, "Arial Black", largeFontSize, "Lucida Console", smallFontSize);

            DateTime afterStart = DateTime.Now;

            //Add Blank Lines

            for (int i = 0; i < 6; i++)
            {
                for (int j = (allDt[i].Rows.Count - 1); j >= 0; j--)
                {
                    str = allDt[i].Rows[j]["service"].ObjToString();
                    data = allDt[i].Rows[j]["data"].ObjToString();
                    //if (str.ToUpper().Trim() == "A. CHARGE FOR SERVICES:")
                    //    continue;
                    if (!String.IsNullOrWhiteSpace(str) || !String.IsNullOrWhiteSpace(data))
                    {
                        if ((j + 1) < allDt[i].Rows.Count)
                            InsertToTable(allDt[i], j + 1, " ", " ", "Dubai Medium", tinyFontSize, "Dubai Medium", tinyFontSize);
                    }
                }
            }

            InsertToTable(allDt[6], 1, " ", " ", "Dubai Medium", tinyFontSize, "Dubai Medium", tinyFontSize);
            //InsertToTable(allDt[7], 2, " ", " ", "Dubai Medium", tinyFontSize, "Dubai Medium", tinyFontSize);

            InsertToTable(allDt[0], 0, " ", " ", "Dubai Medium", tinyFontSize, "Dubai Medium", tinyFontSize);
            InsertToTable(allDt[6], 0, " ", " ", "Dubai Medium", tinyFontSize, "Dubai Medium", tinyFontSize);
            //InsertToTable(allDt[7], 0, " ", " ", "Dubai Medium", tinyFontSize, "Dubai Medium", tinyFontSize);

            int rowCount = 0;
            int leftSize = 0;
            int rightSize = 0;

            Col2_TotalHeight = 0;

            if (G1.get_column_number(maindt, "ignore") < 0)
                maindt.Columns.Add("ignore");

            rowCount = LoadTable(allDt[0], 1, maindt);
            rowCount = LoadTable(allDt[1], 1, maindt);
            rowCount = LoadTable(allDt[2], 1, maindt);
            rowCount = LoadTable(allDt[3], 1, maindt);
            rowCount = LoadTable(allDt[4], 1, maindt);
            rowCount = LoadTable(allDt[5], 1, maindt);
            for (int j = 0; j < 6; j++)
                leftSize += allDt[j].Rows.Count;
            rowCount = 0;
            rowCount += LoadTable(allDt[6], 2, maindt, 0);
            rowCount = LoadTable(allDt[7], 2, maindt, rowCount);
            rowCount = LoadTable(allDt[8], 2, maindt, rowCount);
            rightSize = allDt[6].Rows.Count + allDt[7].Rows.Count + allDt[8].Rows.Count;

            servicesLeftSide = leftSize;
            //if ( SecondRows > 0 )
            //        servicesRightSide = rightSize + SecondRows + 1;
            //else
            servicesRightSide = rightSize;


            DateTime afterLoad = DateTime.Now;

            //            LoadRtfTable(allDt, "");

            TimeSpan tsAfterStart = afterStart - start;
            TimeSpan tsAfterLoad = afterLoad - afterStart;
        }
        /***********************************************************************************************/
        private void SortServicesTable(DataTable dt)
        {
            DataTable dt1 = dt.Clone();
            DataTable dt2 = dt.Clone();

            string data = "";

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                data = dt.Rows[i]["data"].ObjToString();
                if (String.IsNullOrEmpty(data))
                    dt1.ImportRow(dt.Rows[i]);
                else
                    dt2.ImportRow(dt.Rows[i]);
            }
            DataView tempview = dt2.DefaultView;
            tempview.Sort = "service asc";
            dt2 = tempview.ToTable();

            dt.Rows.Clear();
            for (int i = 0; i < dt1.Rows.Count; i++)
                dt.ImportRow(dt1.Rows[i]);
            for (int i = 0; i < dt2.Rows.Count; i++)
                dt.ImportRow(dt2.Rows[i]);
        }
        /***********************************************************************************************/
        private string QualifyServices(string type, string service)
        {
            if (service == "Basic Services Of Funeral Director And Staff".ToUpper())
                type = "SERVICE";
            else if (service == "Embalming".ToUpper())
                type = "SERVICE";
            else if (service == "Embalming, Autopsy Or Donor".ToUpper())
                type = "SERVICE";
            else if (service == "Other Preparation Of The Body (Includes Cosmetology, Dressing And Casketing)".ToUpper())
                type = "SERVICE";
            else if (service == "HEARSE")
                type = "AUTOMOTIVE";
            else if (service.IndexOf("UTILITY VAN") >= 0)
                type = "AUTOMOTIVE";
            else if (service.IndexOf("SAFETY CAR") >= 0)
                type = "AUTOMOTIVE";
            else if (service.IndexOf("TRANSFER OF REMAINS") >= 0)
                type = "AUTOMOTIVE";
            else
                type = "ADDITIONAL";

            return type;
        }
        /***********************************************************************************************/
        private void LoadRtfTable(DataTable[] dts, string searchText)
        {
            DevExpress.XtraRichEdit.RichEditControl rtb = new DevExpress.XtraRichEdit.RichEditControl();
            rtb.Document.BeginUpdate();
            rtb.Text = "%PS1%";
            searchText = "%PS1%";

            int count = 0;
            for (int i = 0; i < 6; i++)
                count += dts[i].Rows.Count;

            DocumentRange[] ranges = rtb.Document.FindAll(searchText, SearchOptions.None);
            DocumentPosition myStart = ranges[0].Start;
            //Paragraph paragraph = rtb.Document.Paragraphs.Get(myStart);
            //paragraph.RightIndent = paragraph.LeftIndent;

            if (count < 45)
                count = 45;

            Table table = rtb.Document.Tables.Create(myStart, count, 4, AutoFitBehaviorType.AutoFitToContents);

            table.Rows[0].FirstCell.PreferredWidthType = WidthType.Fixed;
            table.Rows[0].FirstCell.PreferredWidth = DevExpress.Office.Utils.Units.InchesToDocumentsF(3.0f);
            table.Rows[0].FirstCell.HeightType = HeightType.Auto;
            table[0, 0].HeightType = HeightType.Auto;
            table[0, 0].Height = DevExpress.Office.Utils.Units.InchesToDocumentsF(0.5f);

            //Set the second column width and cell height
            table[0, 1].PreferredWidthType = WidthType.Fixed;
            table[0, 1].PreferredWidth = DevExpress.Office.Utils.Units.InchesToDocumentsF(0.8f);
            table[0, 1].HeightType = HeightType.AtLeast;
            table[0, 1].Height = DevExpress.Office.Utils.Units.InchesToDocumentsF(0.5f);

            table[0, 2].PreferredWidthType = WidthType.Fixed;
            table[0, 2].PreferredWidth = DevExpress.Office.Utils.Units.InchesToDocumentsF(2.5f);
            table[0, 2].HeightType = HeightType.Auto;
            table[0, 2].HeightType = HeightType.Auto;
            table[0, 2].Height = DevExpress.Office.Utils.Units.InchesToDocumentsF(0.5f);

            //Set the second column width and cell height
            table[0, 3].PreferredWidthType = WidthType.Fixed;
            table[0, 3].PreferredWidth = DevExpress.Office.Utils.Units.InchesToDocumentsF(0.8f);
            table[0, 3].HeightType = HeightType.AtLeast;
            table[0, 3].Height = DevExpress.Office.Utils.Units.InchesToDocumentsF(0.5f);

            float height = 0.5f;
            string service = "";
            string data = "";
            double dValue = 0D;
            int lastRow = 0;
            string font = "";
            float size = 0f;
            string font2 = "";
            float size2 = 0f;
            DataTable dt = null;
            int rows = 0;
            int row = 0;
            int col0 = 0;
            int col1 = 1;
            for (int j = 0; j < 8; j++)
            {
                dt = dts[j];
                if (j == 6)
                {
                    for (int k = rows; k < 45; k++)
                    {
                        table[k, 0].Borders.Bottom.LineStyle = TableBorderLineStyle.None;
                        table[k, 0].Borders.Top.LineStyle = TableBorderLineStyle.None;
                        table[k, 0].Borders.Left.LineStyle = TableBorderLineStyle.None;
                        table[k, 0].Borders.Right.LineStyle = TableBorderLineStyle.None;
                        table[k, 1].Borders.Bottom.LineStyle = TableBorderLineStyle.None;
                        table[k, 1].Borders.Top.LineStyle = TableBorderLineStyle.None;
                        table[k, 1].Borders.Left.LineStyle = TableBorderLineStyle.None;
                        table[k, 1].Borders.Right.LineStyle = TableBorderLineStyle.None;
                    }
                    col0 = 2;
                    col1 = 3;
                    rows = 0;
                }
                lastRow = dt.Rows.Count;
                for (int i = 0; i < lastRow; i++)
                {
                    font = dt.Rows[i]["font"].ObjToString();
                    size = dt.Rows[i]["size"].ObjToFloat();
                    font2 = dt.Rows[i]["font2"].ObjToString();
                    size2 = dt.Rows[i]["size2"].ObjToFloat();
                    service = dt.Rows[i]["service"].ObjToString();
                    if (service.ToUpper() == "ACKNOLEDGEMENT CARDS")
                        service = "ACKNOWLEDGEMENT CARDS";

                    data = dt.Rows[i]["data"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(data))
                    {
                        dValue = data.ObjToDouble();
                        data = "$" + G1.ReformatMoney(dValue);
                        if (j < 6)
                        {
                            if (service.ToUpper().IndexOf("TOTAL") < 0)
                            {
                                if (dValue == 0D)
                                    data = "";
                            }
                        }
                        data = data.PadLeft(12);
                    }
                    row = i + rows;
                    rtb.Document.InsertText(table[row, col0].Range.Start, service);
                    rtb.Document.InsertText(table[row, col1].Range.Start, data);
                    table.Rows[row].FirstCell.HeightType = HeightType.Auto;
                    ChangeCellFont(rtb, table[row, col0], font, size);
                    if (!String.IsNullOrWhiteSpace(font2) && size2 > 0f)
                        ChangeCellFont(rtb, table[row, col1], font2, size2);

                    table[row, col0].HeightType = HeightType.Auto;
                    table[row, col0].Height = DevExpress.Office.Utils.Units.InchesToDocumentsF(height);
                    table[row, col1].HeightType = HeightType.Auto;
                    table[row, col1].Height = DevExpress.Office.Utils.Units.InchesToDocumentsF(height);
                    table[row, col0].WordWrap = true;
                    table[row, col1].WordWrap = true;
                    //                table[i, 1].BackgroundColor = System.Drawing.Color.Red;
                    table[row, col0].HeightType = HeightType.Auto;
                    table[row, col1].HeightType = HeightType.Auto;
                    table[row, col0].Borders.Top.LineStyle = TableBorderLineStyle.None;
                    table[row, col0].Borders.Bottom.LineStyle = TableBorderLineStyle.None;
                    table[row, col0].Borders.Left.LineStyle = TableBorderLineStyle.None;
                    table[row, col0].Borders.Right.LineStyle = TableBorderLineStyle.None;
                    table[row, col1].Borders.Top.LineStyle = TableBorderLineStyle.None;
                    if (String.IsNullOrWhiteSpace(data))
                        table[row, col1].Borders.Bottom.LineStyle = TableBorderLineStyle.None;
                    table[row, col1].Borders.Left.LineStyle = TableBorderLineStyle.None;
                    table[row, col1].Borders.Right.LineStyle = TableBorderLineStyle.None;
                    table[row, col1].VerticalAlignment = TableCellVerticalAlignment.Bottom;
                }
                rows += lastRow;
            }
            string text = rtb.Document.RtfText;
            rtb.Document.RtfText = text.Replace(searchText, "");
            rtb.Document.EndUpdate();
            rtbr.InsertTextAsRtf(rtb.Document.RtfText);
        }
        /***********************************************************************************************/
        private void ChangeCellFont(DevExpress.XtraRichEdit.RichEditControl rtb, TableCell cell, string font, float size)
        {
            CharacterProperties cpCell = rtb.Document.BeginUpdateCharacters(cell.Range);
            //            cpCell.ForeColor = System.Drawing.Color.Green;
            //            cpCell.Bold = true;
            cpCell.FontName = font;
            cpCell.FontSize = size;
            rtb.Document.EndUpdateCharacters(cpCell);
        }
        /***********************************************************************************************/
        private int SecondRows = 0;
        private int LoadTable(DataTable inDt, int side, DataTable outDt, int outRow = -1)
        {
            string service = ""; // RAMMA ZAMMA
            string ignore = "";
            string data = "";
            string font = "";
            double size = 0D;
            string font2 = "";
            double size2 = 0D;
            double money = 0D;
            int wordBreak = 57;
            bool gotPayments = false;
            bool addrow = false;
            DataRow dRow = null;
            DataRow dR = null;
            string str = "";
            string str2 = "";
            for (int i = 0; i < inDt.Rows.Count; i++)
            {
                ignore = inDt.Rows[i]["ignore"].ObjToString();
                if (ignore == "Y")
                    continue;
                service = inDt.Rows[i]["service"].ObjToString();
                if (service.ToUpper() == "ACKNOLEDGEMENT CARDS")
                    service = "ACKNOWLEDGEMENT CARDS";
                if (service.ToUpper().IndexOf("URN CREDIT") >= 0)
                    continue;
                if (service.ToUpper().IndexOf("ALTERNATIVE CONTAINER CREDIT") >= 0)
                    continue;
                if (service.ToUpper().IndexOf("CASKET CREDIT") >= 0)
                    continue;
                //if (service.ToUpper().IndexOf("CREMATION CASKET CREDIT OR RENTAL CASKET WITH REMOVABLE INSERT") >= 0) // Comment Out Just in Case
                //    continue;

                if (service == "PAYMENTS:")
                {
                    gotPayments = true;
                    wordBreak = 47;
                    SecondRows = 0;
                }

                data = inDt.Rows[i]["data"].ObjToString();
                if (G1.validate_numeric(data))
                {
                    money = data.ObjToDouble();
                    data = G1.ReformatMoney(money);
                }
                font = inDt.Rows[i]["font"].ObjToString();
                size = inDt.Rows[i]["size"].ObjToDouble();
                font2 = inDt.Rows[i]["font2"].ObjToString();
                size2 = inDt.Rows[i]["size2"].ObjToDouble();
                addrow = false;
                dRow = null;
                if (outRow >= 0)
                {
                    if (outRow >= outDt.Rows.Count)
                    {
                        addrow = true;
                        dRow = outDt.NewRow();
                    }
                    else
                    {
                        addrow = false;
                        try
                        {
                            dRow = outDt.Rows[outRow];
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                    outRow++;
                }
                else
                {
                    addrow = true;
                    dRow = outDt.NewRow();
                }
                if (side == 1)
                {
                    dRow["description"] = service;
                    if (!String.IsNullOrWhiteSpace(data))
                    {
                        string text = data.Replace(",", "");
                        if (G1.validate_numeric(text))
                        {
                            data = "$" + data;
                            data = data.PadLeft(11);
                        }
                    }
                    dRow["details"] = data;
                    dRow["1font"] = font;
                    dRow["1size"] = size;
                    dRow["1font2"] = font2;
                    dRow["1size2"] = size2;
                }
                else
                {
                    dRow["description2"] = service;
                    if (!String.IsNullOrWhiteSpace(data))
                    {
                        string text = data.Replace(",", "");
                        if (G1.validate_numeric(text))
                        {
                            data = "$" + data;
                            data = data.PadLeft(11);
                        }
                    }
                    dRow["details2"] = data;
                    dRow["2font"] = font;
                    dRow["2size"] = size;
                    dRow["2font2"] = font2;
                    dRow["2size2"] = size2;
                    if (service.Length > wordBreak && !addrow)
                    {
                        WordBreak(service, wordBreak, ref str, ref str2);
                        dRow["description2"] = "  " + str;
                        //outRow++;
                        dRow = outDt.Rows[outRow];
                        if (gotPayments)
                            dRow["description2"] = "                     " + str2;
                        else
                            dRow["description2"] = "  " + str2;
                        dRow["details2"] = "";
                        dRow["2font"] = font;
                        dRow["2size"] = size;
                        dRow["2font2"] = font2;
                        dRow["2size2"] = size2;
                        outRow++;
                        SecondRows++;
                        outDt.AcceptChanges();
                    }
                }
                if (addrow)
                {
                    if (service.Length > wordBreak)
                    {
                        WordBreak(service, wordBreak, ref str, ref str2);
                        dR = outDt.NewRow();
                        dR["description"] = "  " + str;
                        dR["details"] = "";
                        dR["1font"] = font;
                        dR["1size"] = size;
                        dR["1font2"] = font2;
                        dR["1size2"] = size2;
                        outDt.Rows.Add(dR);

                        dRow["description"] = "       " + str2;
                        SecondRows++;
                    }
                    outDt.Rows.Add(dRow);
                }
            }
            return outRow;
        }
        /***********************************************************************************************/
        private void WordBreak(string str, int breakLength, ref string str1, ref string str2)
        {
            str1 = "";
            str2 = "";
            string word = "";
            string[] words = str.Split(' ');
            for (int i = 0; i < words.Length; i++)
            {
                word = words[i].Trim();
                if (String.IsNullOrWhiteSpace(word))
                    continue;
                if (!String.IsNullOrWhiteSpace(str2))
                    str2 += " " + word;
                else
                {
                    if ((str1.Length + (word.Length + 1)) > breakLength)
                        str2 = word;
                    else
                        str1 += " " + word;
                }
            }
        }
        /***********************************************************************************************/
        private void InsertToTable(DataTable dt, int row, string service, string data, string font, float size, string font2 = "", float size2 = 0f)
        {
            DataRow dR = dt.NewRow();
            dR["service"] = service;
            dR["data"] = data;
            dR["font"] = font;
            dR["size"] = size;
            dR["font2"] = font2;
            dR["size2"] = size2;
            dt.Rows.InsertAt(dR, row);
        }
        /***********************************************************************************************/
        private void AppendToTable(DataTable dt, string service, string data, string font, float size, string font2 = "", float size2 = 0f, string ignore = "")
        {
            DataRow dR = dt.NewRow();
            dR["service"] = service;
            dR["data"] = data;
            dR["font"] = font;
            dR["size"] = size;
            dR["font2"] = font2;
            dR["size2"] = size2;
            dR["ignore"] = ignore;
            dt.Rows.Add(dR);
        }
        /***********************************************************************************************/
        private double TotalUpTable(DataTable dt, ref double ignoreAmount)
        {
            double dValue = 0D;
            double total = 0D;
            ignoreAmount = 0D;
            string data = "";
            string ignore = "";
            bool found = false;
            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    data = dt.Rows[i]["data"].ObjToString();
                    ignore = dt.Rows[i]["ignore"].ObjToString();
                    //data = dt.Rows[i]["currentPrice"].ObjToString();
                    data = data.Replace("$", "");
                    if (!G1.validate_numeric(data))
                        continue;
                    dValue = data.ObjToDouble();
                    if (ignore == "Y")
                        ignoreAmount += dValue;
                    else
                        total += dValue;
                    found = true;
                }
                data = G1.ReformatMoney(total);
                //                AppendToTable(dt, "       TOTAL :", data, "Arial", 7f, "Lucida Console", 7f);
                if (data != "0.00")
                    AppendToTable(dt, "              ", "", "Arial", 7f, "Lucida Console", 7f);
                AppendToTable(dt, "              ", data, "Arial", 7f, "Lucida Console", 7f);
            }
            catch (Exception ex)
            {
            }
            return total;
        }
        /****************************************************************************************/
        private void gridMain2_RowCellStyle(object sender, DevExpress.XtraGrid.Views.Grid.RowCellStyleEventArgs e)
        {
            GridView View = sender as GridView;
            if (e.RowHandle >= 0)
            {
                //                string location = View.GetRowCellDisplayText(e.RowHandle, View.Columns["location"]);
                string fieldName = e.Column.FieldName;
                if (fieldName.ToUpper() == "DESCRIPTION")
                {
                    DataTable dt = (DataTable)dgv2.DataSource;
                    string name = dt.Rows[e.RowHandle]["1font"].ObjToString();
                    string ssize = dt.Rows[e.RowHandle]["1size"].ObjToString();
                    string data = dt.Rows[e.RowHandle]["description"].ObjToString();
                    Font f = e.Appearance.Font;
                    float size = ssize.ObjToFloat();
                    if (size <= 0F)
                        size = 7F;
                    //if (size <= 7F)
                    //    size = 8F;
                    Font font = new Font(name, size);
                    e.Appearance.Font = font;

                    Graphics g = dgv2.CreateGraphics();
                    StringFormat sf = new StringFormat(StringFormat.GenericTypographic);
                    SizeF fsize = g.MeasureString("XYZZY", font, 400, sf);
                    int h = Convert.ToInt32(fsize.Height);
                    int width = View.Columns["description"].Width;
                    int length = data.Length;
                }
                else if (fieldName.ToUpper() == "DESCRIPTION2")
                {
                    DataTable dt = (DataTable)dgv2.DataSource;
                    string name = dt.Rows[e.RowHandle]["2font"].ObjToString();
                    string ssize = dt.Rows[e.RowHandle]["2size"].ObjToString();
                    string data = dt.Rows[e.RowHandle]["description2"].ObjToString();
                    Font f = e.Appearance.Font;
                    float size = ssize.ObjToFloat();
                    if (size <= 0F)
                        size = 7F;
                    //if (size <= 7F)
                    //    size = 8F;
                    Font font = new Font(name, size);
                    e.Appearance.Font = font;

                    Graphics g = dgv2.CreateGraphics();
                    StringFormat sf = new StringFormat(StringFormat.GenericTypographic);
                    SizeF fsize = g.MeasureString("XYZZY", font, 400, sf);
                    int h = Convert.ToInt32(fsize.Height);
                    int width = View.Columns["description2"].Width;
                    int length = data.Length;
                }
                else if (fieldName.ToUpper() == "DETAILS")
                {
                    DataTable dt = (DataTable)dgv2.DataSource;
                    string name = dt.Rows[e.RowHandle]["1font2"].ObjToString();
                    string ssize = dt.Rows[e.RowHandle]["1size2"].ObjToString();
                    string data = dt.Rows[e.RowHandle]["details"].ObjToString();
                    string desc = dt.Rows[e.RowHandle]["description"].ObjToString();
                    //string name = View.GetRowCellDisplayText(e.RowHandle, View.Columns["2font"]);
                    //string ssize = View.GetRowCellDisplayText(e.RowHandle, View.Columns["2size"]);
                    if (!String.IsNullOrWhiteSpace(data))
                    {
                        if (G1.validate_numeric(data))
                        {
                            double money = data.ObjToDouble();
                            data = G1.ReformatMoney(money);
                        }
                        Font f = e.Appearance.Font;
                        float size = ssize.ObjToFloat();
                        if (size <= 0F)
                            size = 7F;
                        //if (size <= 7F)
                        //    size = 8F;
                        Font font = new Font(name, size);
                        if (desc.Trim().ToUpper().IndexOf("TOTAL") >= 0)
                            e.Appearance.FontStyleDelta = FontStyle.Bold;
                        e.Appearance.Font = font;
                    }
                }
                else if (fieldName.ToUpper() == "DETAILS2")
                {
                    DataTable dt = (DataTable)dgv2.DataSource;
                    string name = dt.Rows[e.RowHandle]["2font2"].ObjToString();
                    string ssize = dt.Rows[e.RowHandle]["2size2"].ObjToString();
                    string data = dt.Rows[e.RowHandle]["details2"].ObjToString();
                    string desc = dt.Rows[e.RowHandle]["description2"].ObjToString();
                    //string name = View.GetRowCellDisplayText(e.RowHandle, View.Columns["2font"]);
                    //string ssize = View.GetRowCellDisplayText(e.RowHandle, View.Columns["2size"]);
                    if (!String.IsNullOrWhiteSpace(data))
                    {
                        if (G1.validate_numeric(data))
                        {
                            double money = data.ObjToDouble();
                            data = G1.ReformatMoney(money);
                        }
                        Font f = e.Appearance.Font;
                        float size = ssize.ObjToFloat();
                        if (size <= 0F)
                            size = 7F;
                        //if (size <= 7F)
                        //    size = 8F;
                        Font font = new Font(name, size);
                        if (desc.Trim().ToUpper().IndexOf("TOTAL") >= 0)
                            e.Appearance.FontStyleDelta = FontStyle.Bold;
                        else if (desc.Trim().ToUpper().IndexOf("BALANCE DUE") >= 0)
                            e.Appearance.FontStyleDelta = FontStyle.Bold;
                        e.Appearance.Font = font;
                    }
                }
            }
        }
        /****************************************************************************************/
        Size sizeText;
        private void gridMain2_CalcRowHeight(object sender, RowHeightEventArgs e)
        {
            if (e.RowHandle < 0) return;

            int row = e.RowHandle;
            try
            {
                DataTable dt = (DataTable)dgv2.DataSource;
                Image chatFakeImage = new Bitmap(1, 1);
                Graphics chatGraphics = Graphics.FromImage(chatFakeImage);
                GraphicsCache graphicsCache = new GraphicsCache(chatGraphics);

                AppearanceObject appearance = new AppearanceObject();
                appearance.TextOptions.HAlignment = HorzAlignment.Near;
                appearance.TextOptions.VAlignment = VertAlignment.Top;
                appearance.TextOptions.WordWrap = WordWrap.Wrap;

                //SizeF sizeF = graphicsCache.CalcTextSize(dt.Rows[e.RowHandle]["description"] as string,
                //                                            appearance.Font,
                //                                            appearance.GetStringFormat(),
                //                                            100);

                //sizeText = new Size((int)Math.Ceiling(sizeF.Width), (int)Math.Ceiling(sizeF.Height));

                //e.RowHeight = sizeText.Height;
                int height = e.RowHeight;
                if (height == 20)
                    e.RowHeight = 10;
                gridMain2.BandPanelRowHeight = 5;
            }
            catch (Exception exception)
            {
            }

        }

        private void gridMain2_RowCellDefaultAlignment(object sender, DevExpress.XtraGrid.Views.Base.RowCellAlignmentEventArgs e)
        {
        }
        /****************************************************************************************/
        private void btnSeparate_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            Contract1 con1 = new Contract1(workContract, serviceDt, paymentsDt, true);
            con1.Show();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void gridMain3_RowCellStylex(object sender, RowCellStyleEventArgs e)
        {
            try
            {
                GridView View = sender as GridView;
                if (e.RowHandle >= 0)
                {
                    //                string location = View.GetRowCellDisplayText(e.RowHandle, View.Columns["location"]);
                    string fieldName = e.Column.FieldName;
                    if (fieldName.ToUpper() == "DESCRIPTION")
                    {
                        DataTable dt = (DataTable)dgv3.DataSource;
                        string name = dt.Rows[e.RowHandle]["1font"].ObjToString();
                        string ssize = dt.Rows[e.RowHandle]["1size"].ObjToString();
                        string data = dt.Rows[e.RowHandle]["description"].ObjToString();
                        Font f = e.Appearance.Font;
                        float size = ssize.ObjToFloat();
                        if (size <= 0F)
                            size = 7F;
                        //if (size <= 7F)
                        //    size = 8F;
                        Font font = new Font(name, size);
                        e.Appearance.Font = font;

                        Graphics g = dgv2.CreateGraphics();
                        StringFormat sf = new StringFormat(StringFormat.GenericTypographic);
                        SizeF fsize = g.MeasureString("XYZZY", font, 400, sf);
                        int h = Convert.ToInt32(fsize.Height);
                        int width = View.Columns["description"].Width;
                        int length = data.Length;
                    }
                    else if (fieldName.ToUpper() == "DESCRIPTION2")
                    {
                        DataTable dt = (DataTable)dgv3.DataSource;
                        string name = dt.Rows[e.RowHandle]["2font"].ObjToString();
                        string ssize = dt.Rows[e.RowHandle]["2size"].ObjToString();
                        string data = dt.Rows[e.RowHandle]["description2"].ObjToString();
                        Font f = e.Appearance.Font;
                        float size = ssize.ObjToFloat();
                        if (size <= 0F)
                            size = 7F;
                        //if (size <= 7F)
                        //    size = 8F;
                        Font font = new Font(name, size);
                        e.Appearance.Font = font;

                        Graphics g = dgv2.CreateGraphics();
                        StringFormat sf = new StringFormat(StringFormat.GenericTypographic);
                        SizeF fsize = g.MeasureString("XYZZY", font, 400, sf);
                        int h = Convert.ToInt32(fsize.Height);
                        int width = View.Columns["description2"].Width;
                        int length = data.Length;
                    }
                    else if (fieldName.ToUpper() == "DETAILS")
                    {
                        DataTable dt = (DataTable)dgv3.DataSource;
                        string name = dt.Rows[e.RowHandle]["1font2"].ObjToString();
                        string ssize = dt.Rows[e.RowHandle]["1size2"].ObjToString();
                        string data = dt.Rows[e.RowHandle]["details"].ObjToString();
                        string desc = dt.Rows[e.RowHandle]["description"].ObjToString();
                        bool gotMoney = false;
                        //string name = View.GetRowCellDisplayText(e.RowHandle, View.Columns["2font"]);
                        //string ssize = View.GetRowCellDisplayText(e.RowHandle, View.Columns["2size"]);
                        if (!String.IsNullOrWhiteSpace(data))
                        {
                            string text = data.Replace("$", "");
                            text = text.Replace(",", "");
                            text = text.Replace("-", "");
                            if (G1.validate_numeric(text))
                            {
                                double money = text.ObjToDouble();
                                data = G1.ReformatMoney(money);
                                data = "$" + data;
                                gotMoney = true;
                            }
                            Font f = e.Appearance.Font;
                            float size = ssize.ObjToFloat();
                            if (size <= 0F)
                                size = 7F;
                            Font font = new Font(name, size);
                            if (gotMoney)
                            {
                                if (desc.Trim().ToUpper().IndexOf("TOTAL") >= 0)
                                    font = new Font(name, size, FontStyle.Bold | FontStyle.Underline);
                                else
                                    font = new Font(name, size, FontStyle.Underline);
                            }
                            e.Appearance.Font = font;
                        }
                    }
                    else if (fieldName.ToUpper() == "DETAILS2")
                    {
                        DataTable dt = (DataTable)dgv3.DataSource;
                        string name = dt.Rows[e.RowHandle]["2font2"].ObjToString();
                        string ssize = dt.Rows[e.RowHandle]["2size2"].ObjToString();
                        string data = dt.Rows[e.RowHandle]["details2"].ObjToString();
                        string desc = dt.Rows[e.RowHandle]["description2"].ObjToString();
                        bool gotMoney = false;
                        if (!String.IsNullOrWhiteSpace(data))
                        {
                            string text = data.Replace("$", "");
                            text = text.Replace(",", "");
                            text = text.Replace("-", "");
                            if (G1.validate_numeric(text))
                            {
                                double money = text.ObjToDouble();
                                data = G1.ReformatMoney(money);
                                data = "$" + data;
                                gotMoney = true;
                            }
                            Font f = e.Appearance.Font;
                            float size = ssize.ObjToFloat();
                            if (size <= 0F)
                                size = 7F;
                            Font font = new Font(name, size);
                            if (desc.Trim().ToUpper().IndexOf("BALANCE") >= 0)
                            {

                            }
                            if (gotMoney)
                            {
                                if (desc.Trim().ToUpper().IndexOf("TOTAL") >= 0)
                                    font = new Font(name, size, FontStyle.Bold | FontStyle.Underline);
                                else if (desc.Trim().ToUpper().IndexOf("BALANCE") >= 0)
                                    font = new Font(name, size, FontStyle.Bold | FontStyle.Underline);
                                else
                                    font = new Font(name, size, FontStyle.Underline);
                            }
                            e.Appearance.Font = font;
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
        /****************************************************************************************/
        private float Col2_TotalHeight = 0f;
        private float Col1_TotalHeight = 0F;
        private bool stopCol2 = false;
        private void gridMain3_RowCellStyle(object sender, RowCellStyleEventArgs e)
        {
            try
            {
                float c1_height = 0F;
                float c2_height = 0F;
                GridView View = sender as GridView;
                if (e.RowHandle >= 0)
                {
                    //                string location = View.GetRowCellDisplayText(e.RowHandle, View.Columns["location"]);
                    string fieldName = e.Column.FieldName;
                    if (fieldName.ToUpper() == "DESCRIPTION")
                    {
                        DataTable dt = (DataTable)dgv3.DataSource;
                        string name = dt.Rows[e.RowHandle]["1font"].ObjToString();
                        string ssize = dt.Rows[e.RowHandle]["1size"].ObjToString();
                        string data = dt.Rows[e.RowHandle]["description"].ObjToString();
                        string data2 = dt.Rows[e.RowHandle]["details"].ObjToString();
                        Font f = e.Appearance.Font;
                        float size = ssize.ObjToFloat();
                        if (size <= 0F)
                            size = 7F;
                        //if (size <= 7F)
                        //    size = 8F;
                        Font font = new Font(name, size);
                        e.Appearance.Font = font;

                        Graphics g = dgv3.CreateGraphics();
                        StringFormat sf = new StringFormat(StringFormat.GenericTypographic);
                        string myString = "XYZZY";
                        SizeF fsize = g.MeasureString(myString, font, 40, sf);
                        SizeF hT = TextRenderer.MeasureText(myString, font);
                        float hh = font.GetHeight();
                        //if (!String.IsNullOrEmpty(data))
                        //{
                        //Col1_TotalHeight += fsize.Height;
                        Col1_TotalHeight += hT.Height;
                        c1_height = Col1_TotalHeight;
                        //}
                        int h = Convert.ToInt32(fsize.Height);
                        int width = View.Columns["description"].Width;
                        int length = data.Length;
                    }
                    else if (fieldName.ToUpper() == "DESCRIPTION2")
                    {
                        DataTable dt = (DataTable)dgv3.DataSource;
                        string name = dt.Rows[e.RowHandle]["2font"].ObjToString();
                        string ssize = dt.Rows[e.RowHandle]["2size"].ObjToString();
                        string data = dt.Rows[e.RowHandle]["description2"].ObjToString();
                        string data2 = dt.Rows[e.RowHandle]["details2"].ObjToString();
                        Font f = e.Appearance.Font;
                        float size = ssize.ObjToFloat();
                        if (size <= 0F)
                            size = 7F;
                        //if (size <= 7F)
                        //    size = 8F;
                        Font font = new Font(name, size);
                        e.Appearance.Font = font;

                        Graphics g = dgv3.CreateGraphics();
                        StringFormat sf = new StringFormat(StringFormat.GenericTypographic);
                        SizeF fsize = g.MeasureString("XYZZY", font, 40, sf);
                        SizeF hT = TextRenderer.MeasureText("XYZZY", font);
                        //if (!String.IsNullOrEmpty(data))
                        //{
                        //Col2_TotalHeight += fsize.Height;
                        if (!stopCol2)
                        {
                            Col2_TotalHeight += hT.Height;
                            c2_height = Col2_TotalHeight;
                        }
                        //}
                        int h = Convert.ToInt32(fsize.Height);
                        int width = View.Columns["description2"].Width;
                        int length = data.Length;
                        if (data.Trim().ToUpper() == "BALANCE DUE")
                        {
                            stopCol2 = true;
                        }
                    }
                    else if (fieldName.ToUpper() == "DETAILS")
                    {
                        DataTable dt = (DataTable)dgv3.DataSource;
                        string name = dt.Rows[e.RowHandle]["1font2"].ObjToString();
                        string ssize = dt.Rows[e.RowHandle]["1size2"].ObjToString();
                        string data = dt.Rows[e.RowHandle]["details"].ObjToString();
                        string desc = dt.Rows[e.RowHandle]["description"].ObjToString();
                        bool gotMoney = false;
                        //string name = View.GetRowCellDisplayText(e.RowHandle, View.Columns["2font"]);
                        //string ssize = View.GetRowCellDisplayText(e.RowHandle, View.Columns["2size"]);
                        if (!String.IsNullOrWhiteSpace(data))
                        {
                            string text = data.Replace("$", "");
                            text = text.Replace(",", "");
                            text = text.Replace("-", "");
                            if (G1.validate_numeric(text))
                            {
                                double money = text.ObjToDouble();
                                data = G1.ReformatMoney(money);
                                data = "$" + data;
                                gotMoney = true;
                            }
                            Font f = e.Appearance.Font;
                            float size = ssize.ObjToFloat();
                            if (size <= 0F)
                                size = 7F;
                            Font font = new Font(name, size);
                            if (gotMoney)
                            {
                                if (desc.Trim().ToUpper().IndexOf("TOTAL") >= 0)
                                    font = new Font(name, size, FontStyle.Bold | FontStyle.Underline);
                                else
                                    font = new Font(name, size, FontStyle.Underline);
                            }
                            Graphics g = dgv3.CreateGraphics();
                            StringFormat sf = new StringFormat(StringFormat.GenericTypographic);
                            SizeF fsize = g.MeasureString("XYZZY", font, 40, sf);
                            SizeF hT = TextRenderer.MeasureText("XYZZY", font);
                            //if (String.IsNullOrEmpty(desc))
                            //{
                            //    //Col1_TotalHeight += fsize.Height;
                            //    Col1_TotalHeight += hT.Height;
                            //    c1_height = Col1_TotalHeight;
                            //}

                            e.Appearance.Font = font;
                        }
                    }
                    else if (fieldName.ToUpper() == "DETAILS2")
                    {
                        DataTable dt = (DataTable)dgv3.DataSource;
                        string name = dt.Rows[e.RowHandle]["2font2"].ObjToString();
                        string ssize = dt.Rows[e.RowHandle]["2size2"].ObjToString();
                        string data = dt.Rows[e.RowHandle]["details2"].ObjToString();
                        string desc = dt.Rows[e.RowHandle]["description2"].ObjToString();
                        bool gotMoney = false;
                        if (!String.IsNullOrWhiteSpace(data))
                        {
                            string text = data.Replace("$", "");
                            text = text.Replace(",", "");
                            text = text.Replace("-", "");
                            if (G1.validate_numeric(text))
                            {
                                double money = text.ObjToDouble();
                                data = G1.ReformatMoney(money);
                                data = "$" + data;
                                gotMoney = true;
                            }
                            Font f = e.Appearance.Font;
                            float size = ssize.ObjToFloat();
                            if (size <= 0F)
                                size = 7F;
                            Font font = new Font(name, size);
                            if (desc.Trim().ToUpper().IndexOf("BALANCE") >= 0)
                            {

                            }
                            if (gotMoney)
                            {
                                if (desc.Trim().ToUpper().IndexOf("TOTAL") >= 0)
                                    font = new Font(name, size, FontStyle.Bold | FontStyle.Underline);
                                else if (desc.Trim().ToUpper().IndexOf("BALANCE") >= 0)
                                    font = new Font(name, size, FontStyle.Bold | FontStyle.Underline);
                                else
                                    font = new Font(name, size, FontStyle.Underline);
                            }
                            Graphics g = dgv3.CreateGraphics();
                            StringFormat sf = new StringFormat(StringFormat.GenericTypographic);
                            SizeF fsize = g.MeasureString("XYZZY", font, 40, sf);
                            SizeF hT = TextRenderer.MeasureText("XYZZY", font);
                            //if (String.IsNullOrEmpty(desc))
                            //{
                            //    //Col2_TotalHeight += fsize.Height;
                            //    Col2_TotalHeight += hT.Height;
                            //    c2_height = Col2_TotalHeight;
                            //}
                            e.Appearance.Font = font;
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
        /****************************************************************************************/
        private void gridMain3_CalcRowHeight(object sender, RowHeightEventArgs e)
        {
            if (e.RowHandle < 0) return;

            int row = e.RowHandle;
            try
            {
                DataTable dt = (DataTable)dgv3.DataSource;
                Image chatFakeImage = new Bitmap(1, 1);
                Graphics chatGraphics = Graphics.FromImage(chatFakeImage);
                GraphicsCache graphicsCache = new GraphicsCache(chatGraphics);

                AppearanceObject appearance = new AppearanceObject();
                appearance.TextOptions.HAlignment = HorzAlignment.Near;
                appearance.TextOptions.VAlignment = VertAlignment.Top;
                appearance.TextOptions.WordWrap = WordWrap.Wrap;

                SizeF sizeF = graphicsCache.CalcTextSize(dt.Rows[e.RowHandle]["description"] as string,
                                                            appearance.Font,
                                                            appearance.GetStringFormat(),
                                                            100);

                sizeText = new Size((int)Math.Ceiling(sizeF.Width), (int)Math.Ceiling(sizeF.Height));


                if (sizeText.Height < e.RowHeight)
                    e.RowHeight = sizeText.Height;
                int height = e.RowHeight;
                //                if (height == 20)
                //                    e.RowHeight = 10;
                //gridMain2.BandPanelRowHeight = 5;
            }
            catch (Exception exception)
            {
            }
        }
        /****************************************************************************************/
        private void btnRemoveEmpty_Click(object sender, EventArgs e)
        {
            string text = rtbStatementOfFuneral.Rtf;
            text = RemoveEmpties(text, "[%", "%]");
            rtbStatementOfFuneral.Rtf = text;
        }
        /****************************************************************************************/
        private string RemoveEmpties(string text, string start, string stop)
        {
            int idx = -1;
            string firstHalf = "";
            string lastHalf = "";
            for (; ; )
            {
                idx = text.IndexOf(start);
                if (idx < 0)
                    break;
                firstHalf = text.Substring(0, idx);
                lastHalf = text.Substring(idx + start.Length);
                idx = text.IndexOf(stop);
                if (idx < 0)
                    break;
                lastHalf = lastHalf.Substring(idx + stop.Length - 1);
                text = firstHalf + lastHalf;
            }
            return text;
        }
        /****************************************************************************************/
        public void FireEventFunServicesChanged(string contract, DataTable dt)
        {
            if (contract == workContract)
            {
                serviceDt = dt;
                //Contract1_Load(null,null);
            }
        }
        /****************************************************************************************/
        public void FireEventFunPaymentsChanged(string contract, DataTable dt)
        {
            if (contract == workContract)
            {
                paymentsDt = dt;
                //Contract1_Load(null, null);
            }
        }
        /****************************************************************************************/
        public class PrintDocumentCommandHandler : ICommandHandler
        {
            public virtual void HandleCommand(PrintingSystemCommand command, object[] args, IPrintControl printControl, ref bool handled)
            {
                if (!CanHandleCommand(command, printControl))
                    return;
                if (workJustViewing)
                    return;
                if ( noSigsFound )
                    MessageBox.Show("Remember to get Signatures for Contract!", "Signatures Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                if (MessageBox.Show("Contract Is Being Printed!!\nDo you want to save this as a permanent copy of this Contract in the customers file?", "Contract Printed Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly) == DialogResult.Yes)
                {
                    string contract = workContractNumber;
                    DateTime today = DateTime.Now;
                    string filename = @"c:\smfsdata\contract_" + contract + "_" + today.Year.ToString("D4") + today.Month.ToString("D2") + today.Day.ToString("D2") + ".pdf";
                    if (File.Exists(filename))
                        File.Delete(filename);
                    printControl.PrintingSystem.ExportToPdf(filename);
                    using (StreamReader sr = new StreamReader(filename))
                    {
                        byte[] result;
                        using (MemoryStream ms = new MemoryStream())
                        {
                            sr.BaseStream.CopyTo(ms);
                            result = ms.ToArray();
                        }
                        string sDate = today.ToString("MM/dd/yyyy");
                        string record = "";
                        string noticeRecord = SavePdfToDatabase(result, "trust");
                        if (!String.IsNullOrWhiteSpace(noticeRecord))
                        {
                            if (!String.IsNullOrWhiteSpace(contract))
                            {
                                record = G1.create_record("lapse_list", "type", "-1");
                                if (!G1.BadRecord("lapse_list", record))
                                {
                                    G1.update_db_table("lapse_list", "record", record, new string[] { "contractNumber", contract, "noticeDate", sDate, "type", "trust", "noticeRecord", noticeRecord, "detail", "Goods and Services" });
                                }
                            }
                        }
                    }
                    File.Delete(filename); // Now Remove the temp file
                }

            }
            public virtual bool CanHandleCommand(PrintingSystemCommand command, IPrintControl printControl)
            {
                return command == PrintingSystemCommand.Print;
            }
        }
        /****************************************************************************************/
        public static string SavePdfToDatabase( byte [] b, string type)
        {
            string record = G1.create_record("lapse_notices", "type", "-1");
            if (G1.BadRecord("lapse_notices", record))
                return "";

            G1.update_db_table("lapse_notices", "record", record, new string[] { "type", type });

            G1.update_blob("lapse_notices", "record", record, "image", b);
            return record;
        }
        /****************************************************************************************/
        public class SaveDocumentCommandHandler : ICommandHandler
        {
            public virtual void HandleCommand(PrintingSystemCommand command, object[] args, IPrintControl printControl, ref bool handled)
            {
                if (!CanHandleCommand(command, printControl))
                    return;
                handled = true;
            }
            public virtual bool CanHandleCommand(PrintingSystemCommand command, IPrintControl printControl)
            {
                return command == PrintingSystemCommand.Save;
            }
        }
        /****************************************************************************************/
        public class ExportToImageCommandHandler : ICommandHandler
        {
            public virtual void HandleCommand(PrintingSystemCommand command,
            object[] args, IPrintControl printControl, ref bool handled)
            {
                if (!CanHandleCommand(command, printControl)) 
                    return;

                // Export the document to PNG.
                printControl.PrintingSystem.ExportToImage("C:\\Report.png", System.Drawing.Imaging.ImageFormat.Png);

                // Prevent the default exporting procedure from being called.
                handled = true;
            }

            public virtual bool CanHandleCommand(PrintingSystemCommand command, IPrintControl printControl)
            {
                return command == PrintingSystemCommand.ExportGraphic;
            }
        }
        /****************************************************************************************/
        private bool gotBalanceDue = false;
        private int balanceDueY = 0;
        private int summaryY = 0;
        private void gridMain3_BeforePrintRow(object sender, DevExpress.XtraGrid.Views.Printing.CancelPrintRowEventArgs e)
        {
            int row = e.RowHandle;
            if (row < 0)
                return;
            row = gridMain3.GetDataSourceRowIndex(row);
            DataTable dt = (DataTable)dgv3.DataSource;
            string str = dt.Rows[row]["description2"].ObjToString();
            if (str.ToUpper().Trim() == "BALANCE DUE")
            {
                gotBalanceDue = true;
                balanceDueY = e.Y;
            }
            if (str.ToUpper().Trim() == "SUMMARY OF CHARGES:")
                summaryY = e.Y;
        }
        /***********************************************************************************************/
        DataTable casketDt = null;
        private void PrintDetail(CreateAreaEventArgs e, ref int leftY, ref int rightY)
        {
            Font font = new Font("Ariel", 16);
            Printer.setupPrinterQuads(e, 80, 40); // X-Quads, Y-Quads

            DataTable dt = (DataTable)dgv3.DataSource;

            if (casketDt == null)
                casketDt = G1.get_db_data("Select * from `casket_master`;");

            string description = "";
            string font1 = "";
            string size1 = "";

            string name = "";
            string ssize = "";
            string data = "";
            string data2 = "";
            float size = 0F;

            leftY = 0;
            rightY = 0;

            bool gotMoney = false;
            string desc = "";

            string[] Lines = null;
            string word = "";
            string casketCode = "";
            DataRow[] dRows = null;

            string database = G1.conn1.Database.ObjToString();

            Graphics g = dgv3.CreateGraphics();
            StringFormat sf = new StringFormat(StringFormat.GenericTypographic);
            string myString = "XYZZY";
            SizeF fsize = g.MeasureString(myString, font, 40, sf);
            SizeF hT = TextRenderer.MeasureText(myString, font);
            float hh = font.GetHeight();

            int y = 1;
            int originalY = 1;
            int yy = 1;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                name = dt.Rows[i]["1font"].ObjToString();
                ssize = dt.Rows[i]["1size"].ObjToString();
                data = dt.Rows[i]["description"].ObjToString();
                data2 = dt.Rows[i]["details"].ObjToString();


                hh = ssize.ObjToFloat();
                if (hh <= 0F)
                    hh = 7F;
                font = new Font(name, hh);

                sf = new StringFormat(StringFormat.GenericTypographic);
                myString = data;
                fsize = g.MeasureString("XYZZY", font, 40, sf);
                hT = TextRenderer.MeasureText(myString, font);
                hh = font.GetHeight();
                int h = Convert.ToInt32(fsize.Height);
                if (data.Trim().ToUpper() == "A. CHARGE FOR SERVICES:")
                    h = 16;
                originalY = y;
                data = G1.force_lower_line_special(data);
                Lines = data.Trim().Split(' ');
                if ( Lines.Length > 0 )
                {
                    casketCode = Lines[0];
                    if (casketCode == "A." || casketCode == "B." || casketCode == "C." || casketCode == "D.")
                        data = data.ToUpper();
                    if ( !String.IsNullOrWhiteSpace ( casketCode))
                    {
                        dRows = casketDt.Select("casketcode='" + casketCode + "'");
                        if (dRows.Length > 0)
                            data = data.Replace(casketCode, casketCode.ToUpper());
                    }
                }
                y = Printer.DrawQuadY(1, y, 35, h, data, Color.Black, BorderSide.None, font, HorizontalAlignment.Left);
                leftY = y;

                name = dt.Rows[i]["1font2"].ObjToString();
                ssize = dt.Rows[i]["1size2"].ObjToString();
                data = dt.Rows[i]["details"].ObjToString();
                desc = dt.Rows[i]["description"].ObjToString();
                gotMoney = false;
                if (!String.IsNullOrWhiteSpace(data))
                {
                    string text = data.Replace("$", "");
                    text = text.Replace(",", "");
                    if ( database.ToUpper() == "SMFS" )
                        text = text.Replace("-", "");
                    if (G1.validate_numeric(text))
                    {
                        double money = text.ObjToDouble();
                        data = G1.ReformatMoney(money);
                        data = "$" + data;
                        gotMoney = true;
                    }
                    size = ssize.ObjToFloat();
                    if (size <= 0F)
                        size = 7F;
                    font = new Font(name, size);
                    if (gotMoney)
                    {
                        if (desc.Trim().ToUpper().IndexOf("TOTAL") >= 0)
                            font = new Font(name, size, FontStyle.Bold | FontStyle.Underline);
                        else
                            font = new Font(name, size, FontStyle.Underline);
                    }
                    g = dgv3.CreateGraphics();
                    sf = new StringFormat(StringFormat.GenericTypographic);
                    fsize = g.MeasureString("XYZZY", font, 40, sf);
                    hT = TextRenderer.MeasureText("XYZZY", font);
                    data = data.PadLeft(10);
                    yy = Printer.DrawQuadY(32, originalY, 8, h, data, Color.Black, BorderSide.None, font, HorizontalAlignment.Right);
                }
            }

            y = 0;
            string detail2 = "";
            bool gotRefund = false;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                gotRefund = false;
                name = dt.Rows[i]["2font"].ObjToString();
                ssize = dt.Rows[i]["2size"].ObjToString();
                data = dt.Rows[i]["description2"].ObjToString();

                if (data.ToUpper().IndexOf("REFUND") >= 0)
                    gotRefund = true;
                else if (data.ToUpper().IndexOf("BALANCE DUE") >= 0)
                    gotRefund = true;

                data2 = dt.Rows[i]["details2"].ObjToString();

                hh = ssize.ObjToFloat();
                if (hh <= 0F)
                    hh = 7F;
                font = new Font(name, hh);

                sf = new StringFormat(StringFormat.GenericTypographic);
                myString = "XYZZY";
                fsize = g.MeasureString(myString, font, 40, sf);
                hT = TextRenderer.MeasureText(myString, font);
                hh = font.GetHeight();
                int h = Convert.ToInt32(fsize.Height);
                if (data.Trim().ToUpper() == "SUMMARY OF CHARGES:")
                    h = 16;
                else if (data.Trim().ToUpper() == "PAYMENTS:")
                    h = 18;
                else if (data.Trim().ToUpper() == "ALLOWANCES:")
                    h = 18;
                originalY = y;
                y = Printer.DrawQuadY(41, y, 35, h, data, Color.Black, BorderSide.None, font, HorizontalAlignment.Left);
                detail2 = data;

                name = dt.Rows[i]["2font2"].ObjToString();
                ssize = dt.Rows[i]["2size2"].ObjToString();
                data = dt.Rows[i]["details2"].ObjToString();
                desc = dt.Rows[i]["description2"].ObjToString();
                gotMoney = false;
                if (!String.IsNullOrWhiteSpace(data))
                {
                    string text = data.Replace("$", "");
                    text = text.Replace(",", "");
                    if (database.ToUpper() == "SMFS")
                    {
                        if (!gotRefund)
                            text = text.Replace("-", "");
                    }
                    if (G1.validate_numeric(text))
                    {
                        double money = text.ObjToDouble();
                        data = G1.ReformatMoney(money);
                        data = "$" + data;
                        gotMoney = true;
                    }
                    size = ssize.ObjToFloat();
                    if (size <= 0F)
                        size = 7F;
                    font = new Font(name, size);
                    if (gotMoney)
                    {
                        if (desc.Trim().ToUpper().IndexOf("TOTAL") >= 0)
                            font = new Font(name, size, FontStyle.Bold | FontStyle.Underline);
                        else if (desc.Trim().ToUpper().IndexOf("BALANCE DUE") >= 0)
                            font = new Font(name, size, FontStyle.Bold | FontStyle.Underline);
                        else
                            font = new Font(name, size, FontStyle.Underline);
                    }
                    g = dgv3.CreateGraphics();
                    sf = new StringFormat(StringFormat.GenericTypographic);
                    fsize = g.MeasureString("XYZZY", font, 40, sf);
                    hT = TextRenderer.MeasureText("XYZZY", font);
                    data = data.PadLeft(10);
                    yy = Printer.DrawQuadY(72, originalY, 8, h, data, Color.Black, BorderSide.None, font, HorizontalAlignment.Right);
                    if (detail2.ToUpper().Trim() == "BALANCE DUE")
                        break;
                }
            }
            rightY = y;
        }
        /***********************************************************************************************/
        private void printableComponentLink1_CreateReportFooterArea(object sender, CreateAreaEventArgs e)
        {
            DevExpress.XtraPrinting.PrintableComponentLink link = (DevExpress.XtraPrinting.PrintableComponentLink)sender;
            link.Margins.Top = 10;
            link.Margins.Bottom = 500;

            int topQuad = 300;
            int bottomQuad = 500;

            Printer.setupPrinterMargins(100, 50, topQuad, bottomQuad); // Left, Right, Top, Bottom

            int yOffset = 0;
            for (int copy = 0; copy < 1; copy++)
            {

                Font font = new Font("Ariel", 16);
                int yQuad = 40;
                //if (printNumber > 1)
                //    yQuad = yQuad * printNumber;
                Printer.setupPrinterQuads(e, 80, yQuad); // X-Quads, Y-Quads

                //int yOffset = 0;


                if (1 == 1)
                {
                    int leftY = 0;
                    int rightY = 0;

                    PrintDetail(e, ref leftY, ref rightY);

                    yOffset = rightY + 10;
                    yOffset = Printer.RichDrawQuadLine(41, 1 + yOffset, 40, 14, this.rtbDisclosures.Rtf, Color.Black, BorderSide.Top, font, HorizontalAlignment.Left);
                    yOffset = Printer.RichDrawQuadLine(41, yOffset, 40, 13, this.rtbDisclaimer.Rtf, Color.White, BorderSide.Top, font, HorizontalAlignment.Left);
                    yOffset = Printer.RichDrawQuadLine(41, yOffset, 40, 18, this.rtback.Rtf, Color.Black, BorderSide.Top, font, HorizontalAlignment.Left);

                    if (yOffset < leftY)
                        yOffset = leftY;

                    yOffset = 665; // Force to Bottom of Page
                    //if (printNumber > 1)
                    //    yOffset = yOffset * printNumber;
                    int yHeight = 25;
                    if (numSignatures > 0)
                    {
                        if (numSignatures == 1)
                        {
                            yOffset = yOffset - 40;
                            yHeight += 5;
                        }
                        else if (numSignatures == 2)
                        {
                            yOffset = yOffset - 80;
                            yHeight += 5;
                        }
                    }

                    yOffset = Printer.RichDrawQuadLine(1, yOffset + 4, 80, yHeight, this.rtbFinale.Rtf, Color.White, BorderSide.None, font, HorizontalAlignment.Left);
                }
            }
            if ( 1 == 1)
                return;


            //DataTable dt = (DataTable)dgv3.DataSource;
            //int maxRows = dt.Rows.Count;
            //int rows = allDt[7].Rows.Count - 5;
            //if (rows <= 3)
            //    rows = 4;
            //else
            //    rows = 0;
            //rows = SecondRows;
            //// X-Quad, Y-Quad, Width, Height
            ////int yOffset = 0;
            ////if (servicesLeftSide > servicesRightSide)
            ////{
            ////    int diff = (servicesLeftSide - servicesRightSide) * 2;
            ////    yOffset = (diff * -1) + 1;
            ////}
            //int zeroMajor = 0;
            //yOffset = yOffset + 4;
            //int h = Convert.ToInt32(Col2_TotalHeight);
            //if (Col2_TotalHeight < Col1_TotalHeight)
            //{
            //    h = Convert.ToInt32(Col1_TotalHeight - Col2_TotalHeight);
            //    //h = Convert.ToInt32(Col2_TotalHeight) - majorHeaderY - 50;
            //    h -= (rows + 10) * 13;
            //    //if (rows <= 0)
            //    //    h -= 40;
            //    zeroMajor = h;
            //    //h -= rows * 13;
            //    //h -= Convert.ToInt32(Col2_TotalHeight);
            //    //h = Convert.ToInt32(Col1_TotalHeight) - majorStartY - Convert.ToInt32(Col2_TotalHeight);
            //    //zeroMajor = Convert.ToInt32(Col1_TotalHeight) - majorStartY;
            //    //zeroMajor = Convert.ToInt32(Col2_TotalHeight) + balanceDueY;
            //    //h = zeroMajor;
            //}
            //else
            //    h = Convert.ToInt32(Col2_TotalHeight);

            //yOffset = 0;
            //if (h > 0)
            //    yOffset = 0 - h;
            ////yOffset = 0 - majorStartY + Convert.ToInt32(Col2_TotalHeight);
            ////if (rows == 0)
            ////    h += 40;
            ////else if (h <= 70)
            ////    h += 20;

            //int diff = yOffset;

            ////yOffset = yOffset;


            //yOffset = Printer.RichDrawQuadLine(41, 1 + yOffset, 40, 14, this.rtbDisclosures.Rtf, Color.Black, BorderSide.Top, font, HorizontalAlignment.Left);
            //yOffset = Printer.RichDrawQuadLine(41, yOffset, 40, 13, this.rtbDisclaimer.Rtf, Color.White, BorderSide.Top, font, HorizontalAlignment.Left);
            //yOffset = Printer.RichDrawQuadLine(41, yOffset, 40, 18, this.rtback.Rtf, Color.Black, BorderSide.Top, font, HorizontalAlignment.Left);

            //diff = yOffset - diff;

            ////if (servicesLeftSide > servicesRightSide)
            ////{
            ////    int diff = (servicesLeftSide - servicesRightSide) * 2;
            ////    yOffset = (diff * -1) + 1;
            ////}

            ////int myOffset = 60;
            ////if (yOffset < 0)
            ////{
            ////    myOffset = myOffset + yOffset + 6;
            ////}
            //////yOffset = yOffset + 14 + 13 + 18;
            //////int totalOffset = yOffset;
            ////yOffset = myOffset;

            ////int irows = allDt[7].Rows.Count;

            //////yOffset = yOffset - 8 - irows + 5;

            ////yOffset = yOffset - (irows * 2) + 2;
            //////yOffset = -69 + 91;

            ////Printer.RichDrawQuadPicture(1, yOffset, 40, 6, this.rtbSig1, Color.White, BorderSide.None, font, HorizontalAlignment.Left);
            ////Printer.RichDrawQuadPicture(41, yOffset, 40, 6, this.rtbSig2, Color.White, BorderSide.None, font, HorizontalAlignment.Left);

            ////yOffset = yOffset - 8;
            ////yOffset = yOffset + 20;
            ////if (yOffset > 160)
            ////    yOffset = 160;

            //Col2_TotalHeight += (float)diff;
            //if (Col2_TotalHeight > Col1_TotalHeight)
            //    h = Convert.ToInt32(Col2_TotalHeight);
            //else
            //    h = Convert.ToInt32(Col1_TotalHeight);
            //if (yOffset < 0)
            //{
            //    yOffset = 0 - h + majorStartY + majorHeaderY + 50 + 635;
            //    yOffset = 0 - h + majorStartY - majorHeaderY + 50 + 635;
            //}
            ////yOffset = 0 - zeroMajor;
            ////yOffset = 0;
            //if (yOffset < 0)
            //    yOffset = 0;
            //if (Col1_TotalHeight >= 700F)
            //    yOffset = 0;

            //yOffset = Printer.RichDrawQuadLine(1, yOffset + 4, 80, 25, this.rtbFinale.Rtf, Color.White, BorderSide.None, font, HorizontalAlignment.Left);




            ////Printer.MyRichDrawQuad(0, 349, 720, 175, this.rtbFinale.Rtf, Color.White, BorderSide.None, font, HorizontalAlignment.Left);

            ////for (int i = 0; i < imageCount; i++)
            ////{
            ////    yOffset -= 5;
            ////    Printer.DrawPagePicture(Images[i], 1, yOffset, 7, 4, Color.White, BorderSide.None, font, HorizontalAlignment.Left);
            ////}


            ////int totalWidth = (int)Printer.localE.Graph.ClientPageSize.Width;
            //int totalHeight = (int)Printer.localE.Graph.ClientPageSize.Height + 200;

        }
        /***************************************************************************************/
        public delegate void d_void_eventdone_gsContract(bool printed);
        public event d_void_eventdone_gsContract GSDone;
        protected void OnGSDone( bool fileGotPrinted )
        {
            if (GSDone != null)
                GSDone.Invoke(fileGotPrinted);
        }
        /****************************************************************************************/
    }
}