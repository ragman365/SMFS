using System;
using System.Data;
using System.Windows.Forms;
using GeneralLib;

using System.Drawing;
using DevExpress.XtraPrinting;
using DevExpress.Utils;
using DevExpress.Pdf;
using MySql.Data.MySqlClient;
using System.Text;
using iTextSharp;
using System.IO;
using System.Runtime.InteropServices;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class Arrangements : DevExpress.XtraEditors.XtraForm
    {
        private bool workSelect = false;
        private bool workLetters = false;
        private Bitmap emptyImage;
        private DataTable masterDt = null;
        private string workLocation = "";
        private string currentTab = "";
        private bool loading = true;
        /***********************************************************************************************/
        public Arrangements()
        {
            InitializeComponent();
            workSelect = false;
            workLetters = false;
        }
        /***********************************************************************************************/
        public Arrangements( bool select = false)
        {
            InitializeComponent();
            workSelect = select;
        }
        /***********************************************************************************************/
        public Arrangements(bool select = false, bool letters = false )
        {
            InitializeComponent();
            workSelect = select;
            workLetters = letters;
        }
        /***********************************************************************************************/
        private void Arrangements_Load(object sender, EventArgs e)
        {
            this.AutoSize = false;
            loading = true;

            if (!workLetters)
            {
                loadLocatons();
                LoadData();
                LoadMainLetters();
            }
            else
            {
                label1.Hide();
                chkComboLocation.Hide();
                cmbLocations.Hide();
                btnSave.Hide();
                gridMain.Columns["location"].Visible = false;
                gridMain.Columns["pic"].Visible = false;
                tabControl1.TabPages.Remove(tabPageGeneral);
                LoadLetters();
            }

            if ( workSelect )
            {
                pictureAdd.Hide();
                pictureDelete.Hide();
                btnSelect.Show();
            }
            else
            {
                btnSelect.Hide();
            }
            loading = false;
        }
        /***********************************************************************************************/
        private void LoadIcons(DataTable dt)
        {
            try
            {
                if (!gridMain.Columns["pic"].Visible)
                    return;
                if (G1.get_column_number(dt, "pic") < 0)
                    dt.Columns.Add("pic", typeof(Bitmap));

                Icon pdfIcon = new Icon("Resources/file_pdf.ico");
                Image pdfImage = pdfIcon.ToBitmap();
                Icon rtfIcon = new Icon("Resources/file_rtf.ico");
                Image rtfImage = rtfIcon.ToBitmap();

                emptyImage = new Bitmap(1, 1);
                string type = "";
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i]["pic"] = (Bitmap)(emptyImage);
                    type = dt.Rows[i]["type"].ObjToString();
                    if ( type.ToUpper() == "PDF")
                    {
                        dt.Rows[i]["pic"] = (Bitmap)pdfImage;
                    }
                    else
                    {
                        dt.Rows[i]["pic"] = (Bitmap)rtfImage;
                    }
                    //Byte[] bytes = dt.Rows[i]["picture"].ObjToBytes();
                    //Image myImage = emptyImage;
                    //if (bytes != null)
                    //{
                    //    myImage = G1.byteArrayToImage(bytes);
                    //    dt.Rows[i]["merchandise"] = (Bitmap)myImage;
                    //}
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Reading Image " + ex.Message.ToString());
            }
        }
        /***********************************************************************************************/
        private void LoadLetters()
        {
            this.Cursor = Cursors.WaitCursor;
            DataTable dt = null;

            string cmd = "Select * from `arrangementforms` where `location` = 'letter' order by `order`";

            cmd += ";";
            dt = G1.get_db_data(cmd);

            dt.Columns.Add("num");
            dt.Columns.Add("agreement");

            G1.NumberDataTable(dt);

            LoadIcons(dt);
            masterDt = dt.Copy();
            masterDt.Columns.Add("LocationCode");
            string name = "";
            for (int i = 0; i < masterDt.Rows.Count; i++)
            {
                masterDt.Rows[i]["LocationCode"] = "letter";
            }

            dgv.DataSource = dt;
            dgv2.DataSource = dt;
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void LoadMainLetters()
        {
            this.Cursor = Cursors.WaitCursor;
            DataTable dt = null;

            string cmd = "Select * from `arrangementforms` where `location` = 'letter' order by `order`";

            cmd += ";";
            dt = G1.get_db_data(cmd);

            dt.Columns.Add("num");
            dt.Columns.Add("agreement");

            G1.NumberDataTable(dt);

            LoadIcons(dt);
            //masterDt = dt.Copy();
            dt.Columns.Add("LocationCode");
            //string name = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dt.Rows[i]["LocationCode"] = "letter";
            }

            dgv3.DataSource = dt;
            //dgv2.DataSource = dt;
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void LoadData()
        {
            this.Cursor = Cursors.WaitCursor;
            DataTable dt = null;

            string cmd = "Select * from `arrangementforms` where `location` <> 'Letter' order by `order`";

            cmd += ";";
            dt = G1.get_db_data(cmd);

            dt.Columns.Add("num");
            dt.Columns.Add("agreement");

            G1.NumberDataTable(dt);
            //            SetupAgreementIcon(dt);
            LoadIcons(dt);
            masterDt = dt.Copy();
            masterDt.Columns.Add("LocationCode");
            string name = "";
            for ( int i=0; i<masterDt.Rows.Count; i++)
            {
                name = masterDt.Rows[i]["location"].ObjToString();
                if (String.IsNullOrWhiteSpace(name))
                    name = "Generic";
                masterDt.Rows[i]["LocationCode"] = name;
            }

            dgv.DataSource = dt;
            dgv2.DataSource = dt;
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            G1.ShowHideFindPanel(gridMain);
            //if (gridMain.OptionsFind.AlwaysVisible == true)
            //    gridMain.OptionsFind.AlwaysVisible = false;
            //else
            //    gridMain.OptionsFind.AlwaysVisible = true;
        }
        /***********************************************************************************************/
        //private void gridMain_DoubleClick(object sender, EventArgs e)
        //{
        //}
        /***********************************************************************************************/
        private void SetupAgreementIcon(DataTable dt)
        {
            DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew = this.repCheckEdit1;
            selectnew.NullText = "";
            selectnew.ValueChecked = "1";
            selectnew.ValueUnchecked = "0";
            selectnew.ValueGrayed = "";
            string filename = "";
            for (int i = 0; i < 1; i++)
            {
                filename = dt.Rows[i]["agreementFile"].ObjToString();
                if (!String.IsNullOrWhiteSpace(filename))
                    dt.Rows[i]["agreement"] = "1";
            }
        }
        /***********************************************************************************************/
        private void repCheckEdit1_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)(dgv.DataSource);
            DataRow dr = gridMain.GetFocusedDataRow();
            string contract = dr["contractNumber"].ObjToString();
            string value = dr["agreement"].ObjToString();
            if (value == "1")
            {
                string filename = "";
                string title = "Agreement for (" + contract + ") ";
                string cmd = "Select * from `customers` where `contractNumber` = '" + contract + "';";
                DataTable dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    filename = dx.Rows[0]["agreementFile"].ObjToString();
                    string firstName = dx.Rows[0]["firstName"].ObjToString();
                    string lastName = dx.Rows[0]["lastName"].ObjToString();
                    title = "Agreement for (" + contract + ") " + firstName + " " + lastName;
                    if (!String.IsNullOrWhiteSpace(filename))
                    {
                        string record = dr["!imagesRecord"].ObjToString();
                        if (record != "-1")
                            Customers.ShowPDfImage(record, title, filename);
                    }
                }
            }
        }
        ///***********************************************************************************************/
        //private void gridMain_DoubleClick_1(object sender, EventArgs e)
        //{
        //    if (workSelect)
        //    {
        //        btnSelect_Click(null, null);
        //        return;
        //    }
        //    //if (!G1.checkUserPreference(LoginForm.username, "DailyHistory", "Change"))
        //    //    return;
        //    DataRow dr = gridMain.GetFocusedDataRow();
        //    string contract = dr["contractNumber"].ObjToString();
        //    if (!String.IsNullOrWhiteSpace(contract))
        //    {
        //        DailyHistory dailyForm = new DailyHistory(contract, null, null);
        //        dailyForm.Show();
        //    }
        //}
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
            Printer.DrawQuad(6, 8, 4, 4, "Arrangement Forms", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);


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
        private void pictureAdd_Click(object sender, EventArgs e)
        {
            string lines = "New Form\nExisting Form\n";
            using (SelectFromList listForm = new SelectFromList(lines))
            {
                listForm.Text = "Select Type of Form";
                listForm.ListDone += ListForm_ListDone;
                listForm.ShowDialog();
            }
            //using (ListSelect listForm = new ListSelect(lines, false))
            //{
            //    listForm.Text = "Select Type of Form";
            //    listForm.ListDone += ListForm_ListDone;
            //    listForm.ShowDialog();
            //}
        }
        /***********************************************************************************************/
        private void ListForm_ListDone(string s)
        {
            string[] Lines = s.Split('\n');
            int Count = Lines.Length;
            if (Count == 0)
                return;
            if ( Lines[0].Trim().ToUpper() == "EXISTING FORM")
            {
                string filename = LookupFile();
                if (String.IsNullOrWhiteSpace(filename))
                    return;
                if ( filename.ToUpper().IndexOf( ".PDF") > 0 )
                {
                    string file = G1.DecodeFilename(filename);
                    string cmd = "Select * from `arrangementForms` where `formName` = '" + file + "';";
                    DataTable dt = G1.get_db_data(cmd);
                    if (dt.Rows.Count > 0)
                        MessageBox.Show("***ERROR*** Form Name has already been used!");
                    else
                    {
                        try
                        {
                            this.Hide();
                            ViewPDF viewForm = new ViewPDF(file, "", filename);
                            viewForm.PdfDone += ViewForm_PdfDone;
                            viewForm.Show();
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }
                else
                {
                    if ( filename.ToUpper().IndexOf( ".DOC") > 0 || filename.ToUpper().IndexOf( "DOCX") > 0 )
                    {
                        this.Hide();
                        this.Cursor = Cursors.WaitCursor;
                        ArrangementForms aForm = new ArrangementForms(filename, "", "", true);
                        aForm.RtfFinished += AForm_RtfDone;
                        aForm.Show();
                        this.Cursor = Cursors.Default;
                    }
                    else if ( filename.ToUpper().IndexOf( ".RTF") > 0 )
                    {
                        this.Hide();
                        this.Cursor = Cursors.WaitCursor;
                        ArrangementForms aForm = new ArrangementForms(filename, "", "", true);
                        aForm.RtfFinished += AForm_RtfDone;
                        aForm.Show();
                        this.Cursor = Cursors.Default;
                    }
                }
            }
            else
            {
                string formName = "";
                using (Ask askForm = new Ask("Enter New Form Name?"))
                {
                    askForm.Text = "";
                    askForm.ShowDialog();
                    if (askForm.DialogResult != System.Windows.Forms.DialogResult.OK)
                        return;
                    formName = askForm.Answer;
                    if (String.IsNullOrWhiteSpace(formName))
                        return;
                }
                string cmd = "Select * from `arrangementForms` where `formName` = '" + formName + "';";
                DataTable dt = G1.get_db_data(cmd);
                if (dt.Rows.Count > 0)
                    MessageBox.Show("***ERROR*** Form Name has already been used!", "Form Name Already Used Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);

                else
                {
                    string record = G1.create_record("arrangementforms", "formName", "-1");
                    if (G1.BadRecord("arrangementforms", record))
                        return;
                    if (workLetters)
                    {
                        G1.update_db_table("arrangementForms", "record", record, new string[] { "formName", formName, "location", "Letter" });

                        LoadLetters();
                    }
                    else
                    {
                        G1.update_db_table("arrangementForms", "record", record, new string[] { "formName", formName });

                        LoadData();
                    }
                }
            }
        }
        /***********************************************************************************************/
        private void AForm_RtfDone(string workFormName, string record, string contractNumber, string rtfText, bool dontAsk, bool force )
        {
            string file = G1.DecodeFilename(workFormName);
            file = file.Replace(".docx", "");
            file = file.Replace(".doc", "");
            file = file.Replace(".rtf", "");
            byte[] b = Encoding.UTF8.GetBytes(rtfText);
            string location = "";
            if (currentTab == "GENERAL")
                location = "General";
            if (workLetters)
                location = "Letter";

            bool creating = false;

            if ( String.IsNullOrWhiteSpace ( record ))
            {
                creating = true;
                record = G1.create_record("arrangementforms", "type", "-1");
                if (G1.BadRecord("arrangementforms", record))
                    return;
                G1.update_db_table("arrangementforms", "record", record, new string[] { "type", "", "formName", file, "location", location });
            }

            G1.update_blob("arrangementforms", "record", record, "image", b);
            if ( creating )
                LoadData();
            this.Show();

            if (1 == 1)
                return;

            if (String.IsNullOrWhiteSpace(record))
                record = G1.create_record("pdfimages", "filename", "-1");
            if (String.IsNullOrWhiteSpace(record))
            {
                MessageBox.Show("***ERROR*** Creating Image Record!");
                return;
            }
            if (record == "0" || record == "-1")
            {
                MessageBox.Show("***ERROR*** Creating Image Record!");
                return;
            }

            string form = file;

            G1.update_db_table("pdfimages", "record", record, new string[] { "filename", form });

            //MemoryStream s = new MemoryStream();
            //rtb.Document.SaveDocument(s, DocumentFormat.Rtf);

            //byte[] b = G1.GetBytesFromStream(s);

            byte[] bb = Encoding.UTF8.GetBytes(rtfText);

            G1.update_blob("pdfimages", "record", record, "image", bb);
        }
        /***********************************************************************************************/
        private void ContractForm_SelectDone(string contract)
        {
            if (String.IsNullOrWhiteSpace(contract))
                return;
            this.Cursor = Cursors.WaitCursor;
            CustomerDetails custForm = new CustomerDetails(contract);
            custForm.Show();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void gridMain_DoubleClick_2(object sender, EventArgs e)
        {
            if ( workSelect )
            {
                OnListDone();
                return;
            }
            Rectangle resolution = Screen.PrimaryScreen.Bounds;
            DataRow dr = gridMain.GetFocusedDataRow();
            string formName = dr["formName"].ObjToString();
            string location = dr["location"].ObjToString();
            string record = dr["record"].ObjToString();
            string type = dr["type"].ObjToString();
            if (!String.IsNullOrWhiteSpace(formName) && String.IsNullOrWhiteSpace (type))
            {
                string str = G1.get_db_blob("arrangementforms", record, "image");
                byte[] b = Encoding.ASCII.GetBytes(str);

                this.Cursor = Cursors.WaitCursor;
                ArrangementForms aForm = new ArrangementForms(formName, location, record, "", b);
                aForm.RtfFinished += AForm_RtfDone;
                aForm.Show();
                this.Cursor = Cursors.Default;
            }
            else if ( type.ToUpper() == "PDF")
            {
                string command = "Select `image` from `arrangementforms` where `record` = '" + record + "';";
                MySqlCommand cmd1 = new MySqlCommand(command, G1.conn1);
                cmd1.Connection.Open();
                try
                {
                    using (MySqlDataReader dR = cmd1.ExecuteReader(System.Data.CommandBehavior.Default))
                    {
                        if (dR.Read())
                        {
                            byte[] fileData = (byte[])dR.GetValue(0);
                            byte[] results = ReplaceFields(fileData);
                            this.Cursor = Cursors.WaitCursor;
                            ViewPDF viewForm = new ViewPDF(formName, record, "", results);
                            viewForm.AutoSize = false;
                            viewForm.PdfDone += ViewForm_PdfDone;
                            viewForm.Show();
                            this.Cursor = Cursors.Default;
                        }
                        dR.Close();
                    }
                }
                catch (Exception ex)
                {

                }
                finally
                {
                    if (cmd1.Connection.State == ConnectionState.Open)
                        cmd1.Connection.Close();
                }
            }
        }
        /***********************************************************************************************/
        private void ViewForm_PdfDone(string filename, string record, string contractNumber, byte[] b)
        {
            string formName = G1.DecodeFilename(filename);
            formName = formName.Replace(".pdf", "");
            byte[] result = null; // Save Default System PDF Form
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                //creating a sample Document
                iTextSharp.text.Document doc = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4, 30f, 30f, 30f, 30f);

                System.IO.MemoryStream mo = new System.IO.MemoryStream();

                iTextSharp.text.pdf.PdfReader reader = new iTextSharp.text.pdf.PdfReader(b);

                StringBuilder sb = new StringBuilder();
                foreach (var de in reader.AcroFields.Fields)
                {
                    sb.Append(de.Key.ToString() + Environment.NewLine);
                }
                string str = sb.ToString();
                str = str.Replace("\r", "");
                string[] Lines = str.Split('\n');

                iTextSharp.text.pdf.PdfStamper pdfStamper = new iTextSharp.text.pdf.PdfStamper(reader, mo);
                iTextSharp.text.pdf.AcroFields fields = pdfStamper.AcroFields;

                //string data = "";
                //for ( int i=0; i<Lines.Length; i++)
                //{
                //    string name = Lines[i].Trim();
                //    str = fields.GetField(name);
                //    data += name + "\n" + str + "\n";
                //}
                pdfStamper.Close();
                reader.Close();
                result = mo.ToArray();

                if (String.IsNullOrWhiteSpace(formName))
                    return;

                if ( 1 == 1)
                {
                    if (!String.IsNullOrWhiteSpace(record))
                        G1.update_blob("arrangementforms", "record", record, "image", result);
                    else
                    {
                        bool creating = false;
                        string cmd = "Select * from `arrangementForms` where `formName` = '" + formName + "';";
                        DataTable dt = G1.get_db_data(cmd);
                        if (dt.Rows.Count > 0)
                            record = dt.Rows[0]["record"].ObjToString();
                        if (String.IsNullOrWhiteSpace(record))
                        {
                            record = G1.create_record("arrangementforms", "formName", "-1");
                            creating = true;
                        }
                        if (G1.BadRecord("arrangementforms", record))
                            return;
                        if (creating)
                        {
                            G1.update_db_table("arrangementforms", "record", record, new string[] { "formName", formName, "type", "PDF" });
                            G1.update_blob("arrangementforms", "record", record, "image", result);
                        }
                    }
                }


                //    string record = "";
                //    bool creating = false;
                //    string cmd = "Select * from `arrangementForms` where `formName` = '" + formName + "';";
                //    DataTable dt = G1.get_db_data(cmd);
                //    if (dt.Rows.Count > 0)
                //        record = dt.Rows[0]["record"].ObjToString();
                //    if (String.IsNullOrWhiteSpace(record))
                //    {
                //        record = G1.create_record("arrangementforms", "formName", "-1");
                //        creating = true;
                //    }
                //    if (G1.BadRecord("arrangementforms", record))
                //        return;
                //    if (creating)
                //    {
                //        G1.update_db_table("arrangementForms", "record", record, new string[] { "formName", formName, "type", "PDF" });
                //        string record1 = G1.create_record("pdfimages", "filename", "-1");
                //        G1.update_db_table("pdfimages", "record", record1, new string[] { "filename", formName });
                //        G1.update_blob("pdfimages", "record", record1, "image", result);
                //    }
                //    else
                //    {
                //        cmd = "Select * from `pdfimages` where `filename` = '" + formName + "';";
                //        dt = G1.get_db_data(cmd);
                //        if ( dt.Rows.Count > 0 )
                //        {
                //            string record1 = dt.Rows[0]["record"].ObjToString();
                //            G1.update_blob("pdfimages", "record", record1, "image", result);
                //        }
                //        else
                //        {
                //            string record1 = G1.create_record("pdfimages", "filename", "-1");
                //            G1.update_db_table("pdfimages", "record", record1, new string[] { "filename", formName });
                //            G1.update_blob("pdfimages", "record", record1, "image", result);
                //        }
                //    }
                LoadData();
            }
            return;
        }
        /***********************************************************************************************/
        public static byte [] ReplaceFields ( byte [] bytes )
        {
            byte[] result = null;
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                //creating a sample Document
                iTextSharp.text.Document doc = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4, 30f, 30f, 30f, 30f);

                System.IO.MemoryStream mo = new System.IO.MemoryStream();

                iTextSharp.text.pdf.PdfReader reader = new iTextSharp.text.pdf.PdfReader(bytes);
                iTextSharp.text.pdf.PdfStamper pdfStamper = new iTextSharp.text.pdf.PdfStamper(reader, mo);

                iTextSharp.text.pdf.AcroFields fields = pdfStamper.AcroFields;
                //fields.SetField("First Name", "Robby");
                //string str = fields.GetField("First Name");
                pdfStamper.Close();
                reader.Close();
                result = mo.ToArray();
            }
            return result;
        }
        /***********************************************************************************************/
        private void pictureDelete_Click(object sender, EventArgs e)
        {
            if (!LoginForm.administrator)
            {
                MessageBox.Show("***Warning*** You do not have permission to delete a FORM!");
                return;
            }

            DataRow dr = gridMain.GetFocusedDataRow();
            if (dgv2.Visible)
                dr = gridMain2.GetFocusedDataRow();

            string record = dr["record"].ObjToString();
            string location = dr["location"].ObjToString();
            string formName = dr["formName"].ObjToString();
            if (String.IsNullOrWhiteSpace(formName))
                formName = "Empty Form";

            string text = "***Question*** Are you sure you want to DELETE this Form (" + formName + ") ?";
            if ( !String.IsNullOrWhiteSpace ( location))
                text = "***Question*** Are you sure you want to DELETE this Form (" + location + " " + formName + ") ?";

            DialogResult result = MessageBox.Show(text, "Delete Agreement Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;
            if ( !String.IsNullOrWhiteSpace ( record ))
                G1.delete_db_table("arrangementforms", "record", record);

            string cmd = "DELETE FROM `structures` WHERE `form` = '" + formName + "' AND `location` = '" + location + "';";
            G1.get_db_data(cmd);

            LoadData();
        }
        /***********************************************************************************************/
        private void renameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            if (dgv2.Visible)
                dr = gridMain2.GetFocusedDataRow();
            else if (dgv3.Visible)
                dr = gridMain3.GetFocusedDataRow();

            string record = dr["record"].ObjToString();
            string formName = dr["formName"].ObjToString();
            string oldName = formName;
            string type = dr["type"].ObjToString();
            using (Ask askForm = new Ask("Enter New Form Name?", formName))
            {
                askForm.Text = "";
                askForm.ShowDialog();
                if (askForm.DialogResult != System.Windows.Forms.DialogResult.OK)
                    return;
                formName = askForm.Answer;
                if (String.IsNullOrWhiteSpace(formName))
                    return;
            }
            dr["formName"] = formName;
            G1.update_db_table("arrangementForms", "record", record, new string[] { "formName", formName });
            if (dgv2.Visible)
                dgv2.Refresh();
            else if (dgv3.Visible)
                dgv2.Refresh();
            else
                dgv.Refresh();

            string cmd = "Select * from `structures` where `form` = '" + oldName + "';";
            DataTable dt = G1.get_db_data(cmd);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                record = dt.Rows[i]["record"].ObjToString();
                G1.update_db_table("structures", "record", record, new string[] { "form", formName });
            }
        }
        /***************************************************************************************/
        public delegate void d_void_eventdone_string(string s);
        public event d_void_eventdone_string ListDone;
        protected void OnListDone()
        {
            if (ListDone != null)
            {
                if (dgv.Visible)
                {
                    DataRow dr = gridMain.GetFocusedDataRow();
                    string record = dr["record"].ObjToString();
                    string formName = dr["formName"].ObjToString();
                    if (!string.IsNullOrWhiteSpace(formName))
                    {
                        //                    ListDone.Invoke(formName);
                        ListDone.Invoke(record);
                        this.Close();
                    }
                }
            }
        }
        /***********************************************************************************************/
        private void btnSelect_Click(object sender, EventArgs e)
        {
            OnListDone();
        }
        /***********************************************************************************************/
        private void editDictionaryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRow dr = null;
            if ( dgv.Visible )
                dr = gridMain.GetFocusedDataRow();
            else if ( dgv2.Visible )
                dr = gridMain2.GetFocusedDataRow();
            else if (dgv3.Visible)
                dr = gridMain3.GetFocusedDataRow();
            string formName = dr["formName"].ObjToString();
            string record = dr["record"].ObjToString();
            string type = dr["type"].ObjToString();
            string location = dr["location"].ObjToString();
            if (!String.IsNullOrWhiteSpace(formName))
            {
                this.Cursor = Cursors.WaitCursor;
                Structures structForm = new Structures (formName, type, record );
                structForm.Show();
                this.Cursor = Cursors.Default;
            }
        }
        /***********************************************************************************************/
        private string LookupFile ()
        {
            string foundFile = "";
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.ShowHelp = true;
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string file = G1.DecodeFilename(ofd.FileName);
                    string cmd = "Select * from `arrangementForms` where `formName` = '" + file + "';";
                    DataTable dt = G1.get_db_data(cmd);
                    if (dt.Rows.Count > 0)
                        MessageBox.Show("***ERROR*** Form Name has already been used!");
                    else
                        foundFile = ofd.FileName;
                }
            }
            return foundFile;
        }
        /***********************************************************************************************/
        private void pDFToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string filename = LookupFile();
            if (String.IsNullOrWhiteSpace(filename))
                return;
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
//                    string file = ofd.FileName;
                    string file = G1.DecodeFilename(ofd.FileName);
                    string cmd = "Select * from `arrangementForms` where `formName` = '" + file + "';";
                    DataTable dt = G1.get_db_data(cmd);
                    if (dt.Rows.Count > 0)
                        MessageBox.Show("***ERROR*** Form Name has already been used!");
                    else
                    {
                        try
                        {
//                            string filename = G1.DecodeFilename(file);
                            ViewPDF viewForm = new ViewPDF(file, "", ofd.FileName);
                            viewForm.PdfDone += ViewForm_PdfDone;
                            viewForm.Show();
                        }
                        catch ( Exception ex)
                        {
                        }
                    }
                }
            }
        }
        /***********************************************************************************************/
        private void Arrangements_Enter(object sender, EventArgs e)
        {
            this.Visible = true;
            this.Show();
        }
        /***********************************************************************************************/
        private void loadLocatons()
        {
            string cmd = "Select * from `funeralhomes` order by `keycode`;";
            DataTable dt = G1.get_db_data(cmd);
            string locationCode = "";
            string name = "";
            cmbLocations.Items.Add("ALL");
            cmbLocations.Items.Add("General");
            repositoryItemComboBox1.Items.Add("ALL");
            repositoryItemComboBox1.Items.Add("General");
            repositoryItemComboBox2.Items.Add("ALL");
            repositoryItemComboBox2.Items.Add("General");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                locationCode = dt.Rows[i]["LocationCode"].ObjToString();
                name = dt.Rows[i]["name"].ObjToString();
                if (String.IsNullOrWhiteSpace(locationCode))
                {
                    locationCode = name;
                    dt.Rows[i]["LocationCode"] = locationCode;
                }
                repositoryItemComboBox1.Items.Add(locationCode);
                repositoryItemComboBox2.Items.Add(locationCode);
                cmbLocations.Items.Add(locationCode);
            }
            cmbLocations.Text = "ALL";

            G1.sortTable(dt, "LocationCode", "ASC");
            DataRow dR = dt.NewRow();
            dR["LocationCode"] = "Generic";
            dt.Rows.InsertAt(dR, 0);

            chkComboLocation.Properties.DataSource = dt;
            chkComboLocation.Text = "All";
        }
        /***********************************************************************************************/
        private void repositoryItemComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            DevExpress.XtraEditors.ComboBoxEdit combo = (DevExpress.XtraEditors.ComboBoxEdit)(sender);
            DataRow dr = gridMain.GetFocusedDataRow();
            string oldLocation = dr["location"].ObjToString();
            string newLocation = combo.Text;
            string formName = dr["formName"].ObjToString();
            if (String.IsNullOrWhiteSpace(oldLocation))
                oldLocation = "Nothing";
            DialogResult result = MessageBox.Show("***Question*** Do you want to change Form (" + formName + ") Location\nfrom " + oldLocation + "\nto " + newLocation + "\nand in ALL it's locations?", "Change Form Location Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;
            string record = dr["record"].ObjToString();
//            string myFields = "location," + newLocation;
            string field = newLocation;
            if (newLocation.ToUpper() == "ALL")
            {
                field = "";
                dr["location"] = "";
                loading = true;
                combo.Text = "";
                loading = false;
            }
            G1.update_db_table("arrangementforms", "record", record, new string[] { "location", field});
            if (oldLocation == "Nothing")
                oldLocation = "";
            string cmd = "Select * from `agreements` where `location` = '" + oldLocation + "' AND `formName` = '" + formName + "';";
            DataTable dt = G1.get_db_data(cmd);
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                record = dt.Rows[i]["record"].ObjToString();
//                myFields = "location," + newLocation;
                G1.update_db_table("agreements", "record", record, new string[] { "location", field });
            }
            cmd = "Select * from `structures` where `location` = '" + oldLocation + "' AND `form` = '" + formName + "';";
            dt = G1.get_db_data(cmd);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                record = dt.Rows[i]["record"].ObjToString();
//                myFields = "location," + newLocation;
                G1.update_db_table("structures", "record", record, new string[] { "location", field });
            }

            int rowHandle = gridMain.FocusedRowHandle;
            //if (rowHandle == 0)
            //    rowHandle = 1;
            gridMain.ClearSelection();
            gridMain.SelectRow(rowHandle);
            gridMain.FocusedRowHandle = rowHandle;
            gridMain.RefreshData();
            dgv.Refresh();
        }
        /***********************************************************************************************/
        private void cmbLocations_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            workLocation = cmbLocations.Text;
            if (workLocation.ToUpper() == "ALL")
                workLocation = "";
            gridMain.RefreshData();
            dgv.Refresh();
            this.Refresh();
        }
        /***********************************************************************************************/
        private void gridMain_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            if (workLocation.Trim().ToUpper() == "ALL")
                return;
            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv.DataSource;
            string location = dt.Rows[row]["location"].ObjToString().Trim().ToUpper();
            if ( location == "GENERAL")
            {
                e.Visible = false;
                e.Handled = true;
                return;
            }
            if (String.IsNullOrWhiteSpace(workLocation))
                return;
            if ( location.ToUpper() != workLocation .ToUpper())
            {
                e.Visible = false;
                e.Handled = true;
                return;
            }
        }
        /***********************************************************************************************/
        private void gridMain2_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv.DataSource;
            string location = dt.Rows[row]["location"].ObjToString().Trim().ToUpper();
            if (location != "GENERAL")
            {
                e.Visible = false;
                e.Handled = true;
                return;
            }
        }
        /***********************************************************************************************/
        private void repositoryItemComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            DevExpress.XtraEditors.ComboBoxEdit combo = (DevExpress.XtraEditors.ComboBoxEdit)(sender);
            DataRow dr = gridMain2.GetFocusedDataRow();
            string oldLocation = dr["location"].ObjToString();
            string newLocation = combo.Text;
            string formName = dr["formName"].ObjToString();
            if (String.IsNullOrWhiteSpace(oldLocation))
                oldLocation = "Nothing";
            DialogResult result = MessageBox.Show("***Question*** Do you want to change Form (" + formName + ") Location\nfrom " + oldLocation + "\nto " + newLocation + "\nand in ALL it's locations?", "Change Form Location Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;
            string record = dr["record"].ObjToString();
            string myFields = "location," + newLocation;
            G1.update_db_table("arrangementforms", "record", record, myFields);
            if (oldLocation == "Nothing")
                oldLocation = "";
            string cmd = "Select * from `agreements` where `location` = '" + oldLocation + "' AND `formName` = '" + formName + "';";
            DataTable dt = G1.get_db_data(cmd);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                record = dt.Rows[i]["record"].ObjToString();
                myFields = "location," + newLocation;
                G1.update_db_table("agreements", "record", record, myFields);
            }
            cmd = "Select * from `structures` where `location` = '" + oldLocation + "' AND `form` = '" + formName + "';";
            dt = G1.get_db_data(cmd);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                record = dt.Rows[i]["record"].ObjToString();
                myFields = "location," + newLocation;
                G1.update_db_table("structures", "record", record, myFields);
            }
            int rowHandle = gridMain2.FocusedRowHandle;
            if (rowHandle == 0)
                rowHandle = 1;
            gridMain2.ClearSelection();
            gridMain2.SelectRow(rowHandle - 1);
            gridMain2.FocusedRowHandle = rowHandle - 1;
            gridMain2.RefreshData();
            dgv2.Refresh();
        }
        /***********************************************************************************************/
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
        }
        /***********************************************************************************************/
        private void gridMain2_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
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
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            TabPage current = (sender as TabControl).SelectedTab;
            currentTab = current.Text.Trim().ToUpper();
        }
        /*******************************************************************************************/
        private string getLocationQuery()
        {
            string procLoc = "";
            string[] locIDs = this.chkComboLocation.EditValue.ToString().Split('|');
            for (int i = 0; i < locIDs.Length; i++)
            {
                if (!String.IsNullOrWhiteSpace(locIDs[i]))
                {
                    if (procLoc.Trim().Length > 0)
                        procLoc += ",";
                    procLoc += "'" + locIDs[i].Trim() + "'";
                }
            }
            return procLoc.Length > 0 ? " `LocationCode` IN (" + procLoc + ") " : "";
        }
        /***********************************************************************************************/
        private void chkComboLocation_EditValueChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            string names = getLocationQuery();
            DataRow[] dRows = masterDt.Select(names);
            DataTable dt = masterDt.Clone();
            for (int i = 0; i < dRows.Length; i++)
                dt.ImportRow(dRows[i]);
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
        }
        /***********************************************************************************************/
        private void gridMain2_DoubleClick(object sender, EventArgs e)
        {
            if (workSelect)
            {
                OnListDone();
                return;
            }
            Rectangle resolution = Screen.PrimaryScreen.Bounds;

            DataRow dr = gridMain2.GetFocusedDataRow();
            string formName = dr["formName"].ObjToString();
            string location = dr["location"].ObjToString();
            string record = dr["record"].ObjToString();
            string type = dr["type"].ObjToString();
            if (!String.IsNullOrWhiteSpace(formName) && String.IsNullOrWhiteSpace(type))
            {
                string str = G1.get_db_blob("arrangementforms", record, "image");
                byte[] b = Encoding.ASCII.GetBytes(str);

                this.Cursor = Cursors.WaitCursor;
                ArrangementForms aForm = new ArrangementForms(formName, location, record, "", b);
                aForm.RtfFinished += AForm_RtfDone;
                aForm.Show();
                this.Cursor = Cursors.Default;
            }
            else if (type.ToUpper() == "PDF")
            {
                string command = "Select `image` from `arrangementforms` where `record` = '" + record + "';";
                MySqlCommand cmd1 = new MySqlCommand(command, G1.conn1);
                cmd1.Connection.Open();
                try
                {
                    using (MySqlDataReader dR = cmd1.ExecuteReader(System.Data.CommandBehavior.Default))
                    {
                        if (dR.Read())
                        {
                            byte[] fileData = (byte[])dR.GetValue(0);
                            byte[] results = ReplaceFields(fileData);
                            this.Cursor = Cursors.WaitCursor;
                            Rectangle resolution2 = Screen.PrimaryScreen.Bounds;
                            ViewPDF viewForm = new ViewPDF(formName, record, "", results);
                            viewForm.PdfDone += ViewForm_PdfDone;
                            viewForm.Show();
                            this.Cursor = Cursors.Default;
                        }
                        dR.Close();
                    }
                }
                catch (Exception ex)
                {

                }
                finally
                {
                    if (cmd1.Connection.State == ConnectionState.Open)
                        cmd1.Connection.Close();
                }
            }
        }
        /***********************************************************************************************/
        private void picRowUp_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgv.Visible)
                {
                    DataTable dt = (DataTable)dgv.DataSource;
                    if (dt.Rows.Count <= 0)
                        return;
                    DataRow dr = gridMain.GetFocusedDataRow();
                    int rowHandle = gridMain.FocusedRowHandle;
                    int row = gridMain.GetDataSourceRowIndex(rowHandle);
                    if (row == 0)
                        return; // Already at the first row
                                //MoveRowUp(dt, rowHandle);
                    massRowsUp(dt, row);
                    dt.AcceptChanges();
                    dgv.DataSource = dt;
                    gridMain.ClearSelection();
                    gridMain.SelectRow(rowHandle - 1);
                    gridMain.FocusedRowHandle = rowHandle - 1;
                    gridMain.RefreshData();
                    dgv.Refresh();
                    btnSave.Show();
                }
                else if ( dgv2.Visible )
                {
                    DataTable dt = (DataTable)dgv2.DataSource;
                    if (dt.Rows.Count <= 0)
                        return;
                    DataRow dr = gridMain2.GetFocusedDataRow();
                    int rowHandle = gridMain2.FocusedRowHandle;
                    int row = gridMain2.GetDataSourceRowIndex(rowHandle);
                    if (row == 0)
                        return; // Already at the first row
                                //MoveRowUp(dt, rowHandle);
                    massRowsUp(dt, row);
                    dt.AcceptChanges();
                    dgv2.DataSource = dt;
                    gridMain2.ClearSelection();
                    gridMain2.SelectRow(rowHandle - 1);
                    gridMain2.FocusedRowHandle = rowHandle - 1;
                    gridMain2.RefreshData();
                    dgv2.Refresh();
                    btnSave.Show();
                }
            }
            catch (Exception ex)
            {
            }
        }
        /***********************************************************************************************/
        private void massRowsUp(DataTable dt, int row)
        {
            int[] rows = gridMain.GetSelectedRows();
            int firstRow = 0;
            if (rows.Length > 0)
                firstRow = rows[0];
            try
            {
                G1.NumberDataTable(dt);
                dt.Columns.Add("Count", Type.GetType("System.Int32"));
                for (int i = 0; i < dt.Rows.Count; i++)
                    dt.Rows[i]["Count"] = i.ToString();
                int moverow = rows[0];
                for (int i = 0; i < rows.Length; i++)
                {
                    row = rows[i];
                    dt.Rows[row]["Count"] = (row - 1).ToString();
                    //dt.Rows[row - 1]["Count"] = row.ToString();
                    //var dRow = gridMain.GetDataRow(row);
                    //dt.Rows[row]["mod"] = "M";
                    //modified = true;
                }
                dt.Rows[moverow - 1]["Count"] = (moverow + (rows.Length - 1)).ToString();
                G1.sortTable(dt, "Count", "asc");
                dt.Columns.Remove("Count");
                G1.NumberDataTable(dt);
            }
            catch (Exception ex)
            {
                MessageBox.Show("*ERROR*** " + ex.Message.ToString());
            }

            //            gridMain.FocusedRowHandle = firstRow;
            gridMain.SelectRow(firstRow);
            dgv.DataSource = dt;
        }
        /***************************************************************************************/
        private void massRowsUpxx(DataTable dt, int row)
        {
            dt.AcceptChanges();
            string type = "";
            if (G1.get_column_number(dt, "count") < 0)
                dt.Columns.Add("Count", Type.GetType("System.Int32"));
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["Count"] = i.ToString();
            type = dt.Rows[row]["location"].ObjToString();
            int mainRow = row;
            for (; ; )
            {
                dt.Rows[mainRow]["Count"] = (row - 1).ToString();
                dt.Rows[row - 1]["Count"] = row.ToString();
                type = dt.Rows[row - 1]["location"].ObjToString();
                if (String.IsNullOrWhiteSpace ( type))
                    break;
                //if (String.IsNullOrWhiteSpace(type))
                //    break;
                row--;
            }

            G1.sortTable(dt, "Count", "asc");
            dt.Columns.Remove("Count");
            G1.NumberDataTable(dt);
        }
        /***************************************************************************************/
        private void massRowsUpx(DataTable dt, int row)
        {
            string type = "";
            if (G1.get_column_number(dt, "count") < 0)
                dt.Columns.Add("Count", Type.GetType("System.Int32"));
            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                    dt.Rows[i]["Count"] = i.ToString();
            }
            catch ( Exception ex)
            {
            }
            try
            {
                int mainRow = row;
                for (; ; )
                {
                    dt.Rows[mainRow]["Count"] = (row - 1).ToString();
                    dt.Rows[row - 1]["Count"] = row.ToString();
                    row--;
                    if (row <= 0)
                        break;
                }
            }
            catch ( Exception ex)
            {
            }

            G1.sortTable(dt, "Count", "asc");
            dt.Columns.Remove("Count");
            G1.NumberDataTable(dt);
        }
        /***************************************************************************************/
        private void MoveRowUp(DataTable dt, int row)
        {
            if (G1.get_column_number(dt, "count") < 0)
                dt.Columns.Add("Count", Type.GetType("System.Int32"));
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["Count"] = i.ToString();
            dt.Rows[row]["Count"] = (row - 1).ToString();
            dt.Rows[row - 1]["Count"] = row.ToString();

            DataView tempview = dt.DefaultView;
            tempview.Sort = "count asc";
            dt = tempview.ToTable();

            dt.Columns.Remove("Count");
            G1.NumberDataTable(dt);
        }
        /***************************************************************************************/
        private void MoveRowDowxn(DataTable dt, int row)
        {
            if (G1.get_column_number(dt, "count") < 0)
                dt.Columns.Add("Count", Type.GetType("System.Int32"));
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["Count"] = i.ToString();
            string type = "";
            int mainRow = row;
            for (; ; )
            {
                dt.Rows[mainRow]["Count"] = (row + 1).ToString();
                dt.Rows[row + 1]["Count"] = row.ToString();
                row++;
            }
            G1.sortTable(dt, "Count", "asc");
            dt.Columns.Remove("Count");
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["Num"] = (i + 1).ToString();
        }
        /***************************************************************************************/
        private void MoveRowDown(DataTable dt, int row)
        {
            if (G1.get_column_number(dt, "count") < 0)
                dt.Columns.Add("Count", Type.GetType("System.Int32"));
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["Count"] = i.ToString();
            string type = "";
            int mainRow = row;
            for (; ; )
            {
                dt.Rows[mainRow]["Count"] = (row + 1).ToString();
                dt.Rows[row + 1]["Count"] = row.ToString();
                type = dt.Rows[row + 1]["location"].ObjToString();
                if (String.IsNullOrWhiteSpace(type))
                    break;
                row++;
            }
            G1.sortTable(dt, "Count", "asc");
            dt.Columns.Remove("Count");
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["Num"] = (i + 1).ToString();
        }
        /***********************************************************************************************/
        private void picRowDown_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgv.Visible)
                {
                    DataTable dt = (DataTable)dgv.DataSource;
                    if (dt.Rows.Count <= 0)
                        return;
                    DataRow dr = gridMain.GetFocusedDataRow();
                    int rowHandle = gridMain.FocusedRowHandle;
                    int row = gridMain.GetDataSourceRowIndex(rowHandle);
                    if (row == (dt.Rows.Count - 1))
                        return; // Already at the last row
                    MoveRowDown(dt, row);
                    dt.AcceptChanges();
                    dgv.DataSource = dt;
                    gridMain.ClearSelection();
                    gridMain.SelectRow(rowHandle + 1);
                    gridMain.FocusedRowHandle = rowHandle + 1;
                    gridMain.RefreshData();
                    dgv.Refresh();
                    btnSave.Show();
                }
                else if ( dgv2.Visible )
                {
                    DataTable dt = (DataTable)dgv2.DataSource;
                    if (dt.Rows.Count <= 0)
                        return;
                    DataRow dr = gridMain2.GetFocusedDataRow();
                    int rowHandle = gridMain2.FocusedRowHandle;
                    int row = gridMain2.GetDataSourceRowIndex(rowHandle);
                    if (row == (dt.Rows.Count - 1))
                        return; // Already at the last row
                    MoveRowDown(dt, row);
                    dt.AcceptChanges();
                    dgv2.DataSource = dt;
                    gridMain2.ClearSelection();
                    gridMain2.SelectRow(rowHandle + 1);
                    gridMain2.FocusedRowHandle = rowHandle + 1;
                    gridMain2.RefreshData();
                    dgv2.Refresh();
                    btnSave.Show();
                }
            }
            catch (Exception ex)
            {
            }
        }
        /***********************************************************************************************/
        private string copyRecord = "";
        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            copyRecord = "";
            DataRow dr = null;
            if (dgv.Visible)
                dr = gridMain.GetFocusedDataRow();
            else if (dgv2.Visible)
                dr = gridMain2.GetFocusedDataRow();
            else if (dgv3.Visible)
                dr = gridMain3.GetFocusedDataRow();
            string formName = dr["formName"].ObjToString();
            string record = dr["record"].ObjToString();
            string type = dr["type"].ObjToString();
            copyRecord = record;
        }
        /***********************************************************************************************/
        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrWhiteSpace(copyRecord))
                return;
            string formName = "";
            string record = "";
            string type = "";
            string location = "";
            string cmd = "Select * from `arrangementforms` where `record` = '" + copyRecord + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                formName = dt.Rows[0]["formName"].ObjToString() + " - Copy";
                record = G1.create_record("arrangementforms", "formName", "-1");
                if (G1.BadRecord("arrangementforms", record))
                    return;
                type = dt.Rows[0]["type"].ObjToString();
                location = dt.Rows[0]["location"].ObjToString();
                G1.update_db_table("arrangementforms", "record", record, new string[] {"type", type, "location", location, "formName", formName });

                string str = G1.get_db_blob("arrangementforms", copyRecord, "image");
                byte[] b = Encoding.ASCII.GetBytes(str);
                G1.update_blob("arrangementforms", "record", record, "image", b);
                copyRecord = "";
                loading = true;
                if ( dgv3.Visible )
                {
                    LoadMainLetters();
                    loading = false;
                    dgv3.Refresh();
                }
                else
                {
                    LoadData();
                    loading = false;
                    dgv.Refresh();
                }
            }
        }
        /***********************************************************************************************/
        private void btnSave_Click(object sender, EventArgs e)
        {
            string record = "";
            DataTable dt = (DataTable)dgv.DataSource;
            if ( dgv2.Visible )
                dt = (DataTable)dgv2.DataSource;
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                record = dt.Rows[i]["record"].ObjToString();
                G1.update_db_table("arrangementforms", "record", record, new string[] {"order", i.ToString() });
            }
            btnSave.Hide();
        }
        /***********************************************************************************************/
        private void gridMain3_DoubleClick(object sender, EventArgs e)
        {
            if (workSelect)
            {
                OnListDone();
                return;
            }
            Rectangle resolution = Screen.PrimaryScreen.Bounds;

            DataRow dr = gridMain3.GetFocusedDataRow();
            string formName = dr["formName"].ObjToString();
            string location = dr["location"].ObjToString();
            string record = dr["record"].ObjToString();
            string type = dr["type"].ObjToString();
            if (!String.IsNullOrWhiteSpace(formName) && String.IsNullOrWhiteSpace(type))
            {
                string str = G1.get_db_blob("arrangementforms", record, "image");
                byte[] b = Encoding.ASCII.GetBytes(str);

                this.Cursor = Cursors.WaitCursor;
                ArrangementForms aForm = new ArrangementForms(formName, location, record, "", b);
                aForm.RtfFinished += AForm_RtfDone;
                aForm.Show();
                this.Cursor = Cursors.Default;
            }
            else if (type.ToUpper() == "PDF")
            {
                string command = "Select `image` from `arrangementforms` where `record` = '" + record + "';";
                MySqlCommand cmd1 = new MySqlCommand(command, G1.conn1);
                cmd1.Connection.Open();
                try
                {
                    using (MySqlDataReader dR = cmd1.ExecuteReader(System.Data.CommandBehavior.Default))
                    {
                        if (dR.Read())
                        {
                            byte[] fileData = (byte[])dR.GetValue(0);
                            byte[] results = ReplaceFields(fileData);
                            this.Cursor = Cursors.WaitCursor;
                            Rectangle resolution2 = Screen.PrimaryScreen.Bounds;
                            ViewPDF viewForm = new ViewPDF(formName, record, "", results);
                            //viewForm.PdfDone += ViewForm_PdfDone;
                            viewForm.Show();
                            this.Cursor = Cursors.Default;
                        }
                        dR.Close();
                    }
                }
                catch (Exception ex)
                {

                }
                finally
                {
                    if (cmd1.Connection.State == ConnectionState.Open)
                        cmd1.Connection.Close();
                }
            }
        }
        /***********************************************************************************************/
    }
}