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
using System.IO;
using System.Text.RegularExpressions;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraGrid.Views.Grid;
using System.Drawing.Drawing2D;
using DevExpress.Utils.Drawing;
using DevExpress.Utils;
using MySql.Data.MySqlClient;
using DevExpress.XtraRichEdit;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.security;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Pkcs;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class FunForms : DevExpress.XtraEditors.XtraForm
    {
        public Contract1 conActive = null;
        private string workContract = "";
        private bool funModified = false;
        private bool loading = true;
        private Color serviceColor = Color.Transparent;
        private DataTable workServicesDt = null;
        private DataTable workPaymentsDt = null;
        private DevExpress.XtraEditors.XtraForm workControl = null;
        private string temporaryFileName = "";
        public static int pictureBorder = 6;
        /****************************************************************************************/
        public FunForms(DevExpress.XtraEditors.XtraForm mainControl, string contract, DataTable serviceDt, DataTable payDt)
        {
            InitializeComponent();
            workContract = contract;
            workControl = mainControl;
            workServicesDt = serviceDt;
            workPaymentsDt = payDt;
        }
        /****************************************************************************************/
        private void FunForms_Load(object sender, EventArgs e)
        {
            if (!LoginForm.RobbyLocal)
                btnProcess.Hide();
            gridMain.OptionsView.AllowHtmlDrawGroups = true;
            funModified = false;
            loading = true;

            LoadData();

            LoadButtons();

            loading = false;
            btnForms_Click(null, null);
        }
        /***********************************************************************************************/
        private void LoadButtons ()
        {

            string formButton = "";
            string formName = "";
            string cmd = "Select * from `form_buttons`;";
            DataTable dx = G1.get_db_data(cmd);

            this.panelBottomLeft.SuspendLayout();

            DataTable dt = new DataTable();
            dt.Columns.Add("buttons");
            DataRow dR = null;

            dR = dt.NewRow();
            dR["buttons"] = "All Forms";
            dt.Rows.Add(dR);


            for ( int i=0; i<dx.Rows.Count; i++)
            {
                formButton = dx.Rows[i]["formButton"].ObjToString();
                formName = dx.Rows[i]["formName"].ObjToString();

                System.Windows.Forms.Button btnContract = new System.Windows.Forms.Button();
                btnContract.Dock = System.Windows.Forms.DockStyle.Bottom;
                btnContract.Location = new System.Drawing.Point(0, 196);
                btnContract.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
                btnContract.Name = formButton;
                btnContract.Size = new System.Drawing.Size(189, 28);
                btnContract.TabIndex = 1;
                btnContract.Text = formButton;
                btnContract.UseVisualStyleBackColor = true;
                btnContract.Click += BtnContract_Click;
                panelBottomLeft.Controls.Add(btnContract);

                dR = dt.NewRow();
                dR["buttons"] = formButton;
                dt.Rows.Add(dR);
            }

            dgv2.DataSource = dt;
            this.panelBottomLeft.ResumeLayout();

            this.panelBottomLeft.Refresh();

            this.panelBottomLeft.Hide();

            dgv2.Refresh();
        }
        /***********************************************************************************************/
        private void BtnContract_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;

            string formButton = button.Name;
            if (String.IsNullOrWhiteSpace(formButton))
                return;
            string formName = "";
            string cmd = "Select * from `form_buttons`;";
            DataTable dx = G1.get_db_data(cmd);

            DataRow[] dRows = dx.Select("formButton='" + formButton + "'");
            if (dRows.Length <= 0)
                return;

            formName = dRows[0]["formName"].ObjToString();
            if (String.IsNullOrWhiteSpace(formName))
                return;
            btnOther(formName);
        }
        /****************************************************************************************/
        private ArrangementForms editFunOther = null;
        private bool otherModified = false;
        private bool showOther = false;
        private void btnOther( string what, bool fromForm = false ) // Ramma Zamma
        {
            btnBiography.Hide();
            btnProcess.Hide();
            this.Cursor = Cursors.WaitCursor;
            pictureBorder = cmbBorder.Text.ObjToInt32();
            ClearColor();
            panelBottomRightForms.Visible = false;
            dgv.Visible = false;
            InitializeSomethingPanel(ref editFunOther, ref otherModified, what, fromForm );
            panelBottomRightRTF.Dock = DockStyle.Fill;
            panelBottomRightRTF.Visible = true;
            //btnMarque.BackColor = Color.Yellow;
            showOther = true;
            btnForms_Click(null, null);
            gridMain.RefreshData();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void LoadData()
        {
            this.Cursor = Cursors.WaitCursor;
            string cmd = "Select * from `agreements` where `contractNumber` = '" + workContract + "';";
            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("num");
            dt.Columns.Add("mod");
            dt.Columns.Add("custom");
            string modified = "";
            string str = null;
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                modified = dt.Rows[i]["modified"].ObjToString();
                str = dt.Rows[i]["image"].ObjToString();
                if ( !String.IsNullOrWhiteSpace ( str ))
                    dt.Rows[i]["custom"] = "Modified!";
                else if ( modified.ToUpper() =="Y")
                    dt.Rows[i]["custom"] = "Modified!";
            }
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            funModified = false;
            this.Cursor = Cursors.Default;
        }
        /***************************************************************************************/
        public bool FireEventFunServicesModified()
        {
            if (funModified)
                return true;
            if ( viewForm != null )
            {
                if (viewForm.FireEventModified())
                    return true;
            }
            return false;
        }
        /***************************************************************************************/
        public void FireEventSaveFunServices(bool save = false)
        {
            if (save && funModified)
            {
                if ( editFunObit != null)
                    editFunObit.FireEventSaveFunForms(true);
                if ( editFunProgram != null)
                    editFunProgram.FireEventSaveFunForms(true);
                if ( editFunMarque != null)
                    editFunMarque.FireEventSaveFunForms(true);
                if (editFunTube != null)
                    editFunTube.FireEventSaveFunForms(true);
                if (editFunDirectorsReport != null)
                    editFunDirectorsReport.FireEventSaveFunForms(true);
                if (editFunOther != null)
                    editFunOther.FireEventSaveFunForms(true);

                obitModified = false;
                programModified = false;
                marqueModified = false;
                tubeModified = false;
                directorsReportModified = false;
                otherModified = false;
            }
            this.Close();
        }
        /****************************************************************************************/
        private void panelClaimTop_Paint(object sender, PaintEventArgs e)
        {
            Rectangle rect = panelClaimTop.Bounds;
            Graphics g = panelClaimTop.CreateGraphics();
            Pen pen = new Pen(Brushes.Black);
            int left = rect.Left;
            int top = rect.Top;
            int width = rect.Width - 1;
            int high = rect.Height - 1;
            g.DrawRectangle(pen, left, top, width, high);
        }
        /****************************************************************************************/
        private void panelBottom_Paint(object sender, PaintEventArgs e)
        {
            Rectangle rect = panelBottom.Bounds;
            Graphics g = panelBottom.CreateGraphics();
            Pen pen = new Pen(Brushes.Black);
            int left = rect.Left;
            int top = rect.Top;
            int width = rect.Width - 1;
            int high = rect.Height - 1;
            g.DrawRectangle(pen, left, top, width, high);
        }
        /***********************************************************************************************/
        private void CheckForSaving()
        {
            if (String.IsNullOrWhiteSpace(workContract))
                return;
            if (!funModified)
                return;
            DialogResult result = MessageBox.Show("***Question***\nPayments have been modified!\nWould you like to SAVE your Payments?", "Payments Modified Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;
        }
        /****************************************************************************************/
        private void gridMain_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv.DataSource;
            string type = dt.Rows[row]["type"].ObjToString().ToUpper();
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
        }
        /****************************************************************************************/
        private void btnShowPDF_Click(object sender, EventArgs e)
        {
        }
        /****************************************************************************************/
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            funModified = true;
            gridMain.RefreshData();
        }
        /***********************************************************************************************/
        private void AddMod(DataTable dt, DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView grid)
        {
            if (G1.get_column_number(dt, "mod") < 0)
                dt.Columns.Add("mod");
        }
        /****************************************************************************************/
        private void pictureBox12_Click(object sender, EventArgs e)
        { // Add New Form
            using (Arrangements arrForm = new Arrangements(true))
            {
                arrForm.Text = "Arrangement List";
                arrForm.ListDone += ArrForm_ListDone;
                arrForm.ShowDialog();
            }
        }
        /***********************************************************************************************/
        private void ArrForm_ListDone(string s)
        {
            if (String.IsNullOrWhiteSpace(s))
                return;
            string arrangementRecord = s;
            string cmd = "Select * from `arrangementForms` where `record` = '" + arrangementRecord + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                string type = dt.Rows[0]["type"].ObjToString();
                string form = dt.Rows[0]["formName"].ObjToString();
                string location = dt.Rows[0]["location"].ObjToString();
                string record = G1.create_record("agreements", "formName", "-1");
                if (G1.BadRecord("agreements", record))
                    return;
                G1.update_db_table("agreements", "record", record, new string[] { "contractNumber", workContract, "formName", form , "location", location });
                LoadData();
            }
        }
        ///****************************************************************************************/
        //private void gridMain_CustomDrawFooterCell(object sender, FooterCellCustomDrawEventArgs e)
        //{
        //    if (e.Column.FieldName != "payment")
        //        return;
        //    int dx = e.Bounds.Height;
        //    Brush brush = e.Cache.GetGradientBrush(e.Bounds, Color.Wheat, Color.FloralWhite, LinearGradientMode.Vertical);
        //    Rectangle r = e.Bounds;
        //    //Draw a 3D border 
        //    BorderPainter painter = BorderHelper.GetPainter(DevExpress.XtraEditors.Controls.BorderStyles.Style3D);
        //    AppearanceObject borderAppearance = new AppearanceObject(e.Appearance);
        //    borderAppearance.BorderColor = Color.DarkGray;
        //    painter.DrawObject(new BorderObjectInfoArgs(e.Cache, borderAppearance, r));
        //    //Fill the inner region of the cell 
        //    r.Inflate(-1, -1);
        //    e.Cache.FillRectangle(brush, r);
        //    //Draw a summary value 
        //    r.Inflate(-2, 0);
        //    double total = calculateTotalPayments();
        //    string text = G1.ReformatMoney(total);
        //    e.Appearance.DrawString(e.Cache, text, r);
        //    //Prevent default drawing of the cell 
        //    e.Handled = true;
        //}
        ///****************************************************************************************/
        //private double calculateTotalPayments ()
        //{
        //    DataTable dt = (DataTable)dgv.DataSource;
        //    double price = 0D;
        //    double total = 0D;
        //    string status = "";
        //    for (int i = 0; i < dt.Rows.Count; i++)
        //    {
        //        status = dt.Rows[i]["status"].ObjToString().Trim().ToUpper();
        //        if (status != "CANCELLED" && status != "REJECTED")
        //        {
        //            price = dt.Rows[i]["payment"].ObjToDouble();
        //            total += price;
        //        }
        //    }
        //    status = G1.ReformatMoney(total);
        //    return total;
        //}
        /****************************************************************************************/
        private void pictureBox11_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            string form = dr["formName"].ObjToString();

            DialogResult result = MessageBox.Show("Are you sure you want to delete form (" + form + ")?", "Delete Form Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.No)
                return;

            G1.delete_db_table("agreements", "record", record);
            LoadData();
        }
        /****************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            string form = dr["formName"].ObjToString();
            string location = dr["location"].ObjToString();
            if ( String.IsNullOrWhiteSpace ( location ))
                location = EditCust.activeFuneralHomeName;
            //LoadUpForm(form, location, record);
            btnOther(form, true );
        }
        /***********************************************************************************************/
        private void LoadUpForm(string form, string location, string record, bool reload = false)
        {
            string record1 = "";
            string str = G1.get_db_blob("agreements", record, "image");
            if (String.IsNullOrWhiteSpace(str))
            {
                string cmd = "Select * from `arrangementforms` where `formName` = '" + form + "' AND `location` = '" + location + "';";
                DataTable dt = G1.get_db_data(cmd);
                if (dt.Rows.Count > 0)
                {
                    record1 = dt.Rows[0]["record"].ObjToString();
                    str = G1.get_db_blob("arrangementforms", record1, "image");
                }
                else
                {
                    cmd = "Select * from `arrangementforms` where `formName` = '" + form + "';";
                    dt = G1.get_db_data(cmd);
                    if (dt.Rows.Count > 0)
                    {
                        record1 = dt.Rows[0]["record"].ObjToString();
                        str = G1.get_db_blob("arrangementforms", record1, "image");
                    }
                }
            }
            if (str.IndexOf("rtf1") > 0)
            {
                if (reload)
                {
                    string cmd = "Select * from `arrangementforms` where `formName` = '" + form + "';";
                    DataTable dt = G1.get_db_data(cmd);
                    if (dt.Rows.Count > 0)
                    {
                        record1 = dt.Rows[0]["record"].ObjToString();
                        str = G1.get_db_blob("arrangementforms", record1, "image");
                        byte[] b = Encoding.UTF8.GetBytes(str);
                        G1.update_blob("agreements", "record", record, "image", b);
                        this.Cursor = Cursors.WaitCursor;
                        ArrangementForms arrangementForm = new ArrangementForms(form, location, record, workContract, b);
                        arrangementForm.RtfFinished += ArrangementForm_RtfDone;
                        arrangementForm.RtfModified += ArrangementForm_RtfModified;
                        arrangementForm.Show();
                        this.Cursor = Cursors.Default;
                    }
                }
                else
                {
                    byte[] b = Encoding.UTF8.GetBytes(str);
                    this.Cursor = Cursors.WaitCursor;
                    ArrangementForms arrangementForm = new ArrangementForms(form, location, record, workContract, b);
                    arrangementForm.RtfFinished += ArrangementForm_RtfDone;
                    arrangementForm.RtfModified += ArrangementForm_RtfModified;
                    arrangementForm.Show();
                    this.Cursor = Cursors.Default;

                }
            }
            else if (str.IndexOf("PDF") > 0)
            {
                bool custom = true;
                string command = "Select `image` from `agreements` where `record` = '" + record + "';";
                if (!String.IsNullOrWhiteSpace(record1))
                {
                    command = "Select `image` from `arrangementforms` where `record` = '" + record1 + "';";
                    custom = false;
                }
                MySqlCommand cmd1 = new MySqlCommand(command, G1.conn1);
                cmd1.Connection.Open();
                try
                {
                    using (MySqlDataReader dR = cmd1.ExecuteReader(System.Data.CommandBehavior.Default))
                    {
                        if (dR.Read())
                        {
                            byte[] fileData = (byte[])dR.GetValue(0);
                            byte[] results = ReplaceFields(fileData, form);
                            this.Cursor = Cursors.WaitCursor;
                            if (!custom)
                            {
                                ViewPDF viewForm = new ViewPDF(form, record, workContract, results, fileData );
                                viewForm.PdfDone += ViewForm_PdfDone;
                                viewForm.Show();
                            }
                            else
                            {
                                ViewPDF viewForm = new ViewPDF(form, record, workContract, fileData, fileData );
                                viewForm.PdfDone += ViewForm_PdfDone;
                                viewForm.Show();
                            }
                            this.Cursor = Cursors.Default;
                            //btnDeathCert.BackColor = Color.Transparent;
                            //btnDeathCert.Refresh();
                            btnForms_Click(null, null);
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
        private void ArrangementForm_RtfModified()
        {
            funModified = true;
        }
        /***********************************************************************************************/
        private void ArrangementForm_RtfDone(string workFormName, string record, string contractNumber, string rtfText, bool dontAsk, bool forceSave = false )
        {
            if (String.IsNullOrWhiteSpace(record))
            {
                return;
            }
            if (!funModified)
            {
                if ( !forceSave )
                    return;
            }

            if (!dontAsk)
            {
                DialogResult result = MessageBox.Show("***Question***\nForm " + workFormName + " has been modified!\nDo you want to save your changes?", "Form Modified Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2, MessageBoxOptions.DefaultDesktopOnly);
                if (result == DialogResult.No)
                {
                    funModified = false;
                    return;
                }
            }

            string form = workFormName;
            byte[] b = Encoding.UTF8.GetBytes(rtfText);
            if (!String.IsNullOrWhiteSpace(contractNumber))
            {
                string cmd = "Select * from `agreements` where `contractNumber` = '" + contractNumber + "' AND `formName` = '" + workFormName + "';";
                DataTable ddx = G1.get_db_data(cmd);
                if (ddx.Rows.Count <= 0)
                {
                    record = G1.create_record("agreements", "formName", "-1");
                    if (G1.BadRecord("agreements", record))
                        return;
                    G1.update_db_table("agreements", "record", record, new string[] { "contractNumber", contractNumber, "formName", form });
                }
                else
                {
                    record = ddx.Rows[0]["record"].ObjToString();
                    G1.update_db_table("agreements", "record", record, new string[] { "modified", "Y" });
                }
            }
            G1.update_blob("agreements", "record", record, "image", b);
            LoadData();
            funModified = false;
        }
        /***********************************************************************************************/
        private byte[] ReplaceFields(byte[] bytes, string form)
        {
            byte[] result = null;
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                //creating a sample Document
                iTextSharp.text.Document doc = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4, 30f, 30f, 30f, 30f);

                System.IO.MemoryStream mo = new System.IO.MemoryStream();
                iTextSharp.text.pdf.PdfStamper pdfStamper = null;

                iTextSharp.text.pdf.PdfReader reader = new iTextSharp.text.pdf.PdfReader(bytes);
                try
                {
                    //string pfxFilePath = "D:\\Uday Dodiya\\Digital_Sign\\Uday Dodiya.pfx";
                    //string pfxPassword = "uday1234";
                    //Pkcs12Store pfxKeyStore = new Pkcs12Store(new FileStream(pfxFilePath, FileMode.Open, FileAccess.Read), pfxPassword.ToCharArray());

                    pdfStamper = new iTextSharp.text.pdf.PdfStamper(reader, mo);

                    //PdfStamper pdfStamperX = PdfStamper.CreateSignature(reader, new FileStream("C:\\Users\\Admin\\Documents\\MyPDF_Signed.pdf", FileMode.Create), '\0', null, true);
                    //PdfSignatureAppearance signatureAppearance = pdfStamperX.SignatureAppearance;
                    //signatureAppearance.Reason = "Digital Signature Reason";
                    //signatureAppearance.Location = "Digital Signature Location";
                    //// Set the signature appearance location (in points)
                    //float x = 360;
                    //float y = 130;
                    //signatureAppearance.Acro6Layers = false;
                    //signatureAppearance.Layer4Text = PdfSignatureAppearance.questionMark;
                    //signatureAppearance.SetVisibleSignature(new iTextSharp.text.Rectangle(x, y, x + 150, y + 50), 1, "signature");
                    //string alias = pfxKeyStore.Aliases.Cast<string>().FirstOrDefault(entryAlias => pfxKeyStore.IsKeyEntry(entryAlias));

                    //if (alias != null)
                    //{
                    //    ICipherParameters privateKey = pfxKeyStore.GetKey(alias).Key;
                    //    IExternalSignature pks = new PrivateKeySignature(privateKey, DigestAlgorithms.SHA256);
                    //    MakeSignature.SignDetached(signatureAppearance, pks, new
                    //    Org.BouncyCastle.X509.X509Certificate[] { pfxKeyStore.GetCertificate(alias).Certificate },
                    //    null, null, null, 0, CryptoStandard.CMS);
                    //}
                    //else
                    //{
                    //    Console.WriteLine("Private key not found in the PFX certificate.");
                    //}
                }
                catch (Exception ex)
                {
                }


                iTextSharp.text.pdf.AcroFields fields = pdfStamper.AcroFields;
                //fields.SetField("First Name", "Robby");
                string str = fields.GetField("First Name");

                DataTable dt = setupData(form); // This is where we lookup the data

                //StringBuilder sb = new StringBuilder();
                //foreach (var de in reader.AcroFields.Fields)
                //{
                //    sb.Append(de.Key.ToString() + Environment.NewLine);
                //}
                //str = sb.ToString();
                //str = str.Replace("\r", "");
                //string[] Lines = str.Split('\n');
                //string field = "";
                //string data = "";

                //string cmd = "Select * from `structures` where `form` = '" + form + "' order by `order`;";
                //DataTable dt = G1.get_db_data(cmd);
                //dt.Columns.Add("num");
                //dt.Columns.Add("mod");
                //dt.Columns.Add("F1");
                //dt.Columns.Add("F2");

                //for (int i = 0; i < Lines.Length; i++)
                //{
                //    field = Lines[i].Trim();
                //    if (!String.IsNullOrWhiteSpace(field))
                //    {
                //        DataRow dRow = dt.NewRow();
                //        dRow["field"] = field;
                //        dt.Rows.Add(dRow);
                //    }
                //}
                //string returnData = "";
                //LoadDbFields(dt);
                string field = "";
                string data = "";
                string returnData = "";
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    field = dt.Rows[i]["dbfield"].ObjToString();
                    if (field.ToUpper() == "POD")
                    {
                    }
                    field = dt.Rows[i]["field"].ObjToString();
                    if ( field == "Surviving Spouse Name")
                    {
                    }
                    if ( field.ToUpper().IndexOf ("14A") >= 0 )
                    {
                    }
                    data = dt.Rows[i]["F2"].ObjToString();
                    if (data.IndexOf("`") > 0)
                        data = data.Replace("`", "'");
                    int field_type = fields.GetFieldType(field);
                    //if (field == "Decedent Marital Status")
                    //{
                    //    // CP_1 is the name of a check box field
                    //    String[] values = fields.GetAppearanceStates(field);
                    //    string f = fields.GetField(field);
                    //    //data = "/" + data;
                    //    data = values[1];
                    //}
                    if (!String.IsNullOrWhiteSpace(data))
                    {
                        fields.SetField(field, data);
                    }
                    else
                        fields.SetField(field, data);
                    if ( field == "Surviving Spouse Name")
                    {
                    }
                    returnData = fields.GetField(field);
                }
                //fields.SetField("tot_amt", "56.00");
                //fields.SetField("tot_qty", "7");
                //fields.SetField("add_amt", "36.00");
                //fields.SetField("add_qty", "6.00");
                pdfStamper.Close();
                reader.Close();
                result = mo.ToArray();
            }
            return result;
        }
        /***********************************************************************************************/
        private DataTable setupData(string form)
        {
            string cmd = "Select * from `structures` where `form` = '" + form + "' order by `order`;";
            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("num");
            dt.Columns.Add("mod");
            dt.Columns.Add("F1");
            dt.Columns.Add("F2");
            LoadDbFields(dt);
            return dt;
        }
        /***********************************************************************************************/
        private DataTable LoadDbFields(DataTable dt)
        {
            if (String.IsNullOrWhiteSpace(workContract))
                return dt;
            string table = "";
            string dbfield = "";
            string qualify = "";
            string data = "";
            string cmd = "";
            string field = "";
            string form = "";
            bool found = false;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                table = dt.Rows[i]["table"].ObjToString();
                dbfield = dt.Rows[i]["dbfield"].ObjToString();
                if ( dbfield.ToUpper().IndexOf ("PLACE OF DEATH") >= 0 )
                {
                }
                if (String.IsNullOrWhiteSpace(table))
                    continue;
                if (String.IsNullOrWhiteSpace(dbfield))
                    continue;
                field = dt.Rows[i]["field"].ObjToString();
                if (field == "SIGNATURE_DATE")
                {
                }
                qualify = dt.Rows[i]["qualify"].ObjToString();
                if ( dbfield == "EducationLevel")
                {
                }
                if (qualify.ToUpper() == "INFORMANT=1")
                {
                }
                field = dt.Rows[i]["field"].ObjToString();
                if ( field.ToUpper().IndexOf ( "14A") >= 0 )
                {
                }
                found = GetDbField(table, field, dbfield, qualify, workContract, ref data);
                if ( dbfield.ToUpper() == "SSN")
                    data = FunCustomer.FixSSN(data);
                if ( data.IndexOf ( "her Residence") >= 0 || data.IndexOf ( "his Residence" ) >= 0 )
                {
                    form = dt.Rows[i]["form"].ObjToString();
                    if ( form.ToUpper() == "MS_DEATH_CERTIFICATE")
                    {
                        string address1 = "";
                        string address2 = "";
                        string city = "";
                        string state = "";
                        string zip = "";
                        found = GetDbField("FCUSTOMERS", "", "address1", "", workContract, ref address1 );
                        found = GetDbField("FCUSTOMERS", "", "address2", "", workContract, ref address2 );
                        found = GetDbField("FCUSTOMERS", "", "city", "", workContract, ref city);
                        found = GetDbField("FCUSTOMERS", "", "state", "", workContract, ref state);
                        found = GetDbField("FCUSTOMERS", "", "zip1", "", workContract, ref zip );
                        if (!string.IsNullOrWhiteSpace(address2))
                            address1 += " " + address2;
                        //if (!String.IsNullOrWhiteSpace(city))
                        //    address1 += ", " + city;
                        //if (!String.IsNullOrWhiteSpace(state))
                        //    address1 += ", " + state;
                        //if (!String.IsNullOrWhiteSpace(zip))
                        //    address1 += "  " + zip;
                        data = address1;
                    }
                }
                if (field.ToUpper().IndexOf("PLACE OF DEATH CHECKBOX") >= 0)
                {
                    if ( String.IsNullOrWhiteSpace ( data ))
                        data = "Other";
                }
                field = data.Replace(",", "");
                if (String.IsNullOrWhiteSpace(field))
                    data = "";
                dt.Rows[i]["F2"] = data;
                if ( dbfield.ToUpper() == "AGE")
                    dt.Rows[i]["F2"] = RTF_Stuff.GetDecAge(workContract);
                else if ( dbfield == "Maiden Name")
                {
                    string gender = RTF_Stuff.GetDecGender(workContract);
                    if (gender.ToUpper() == "F")
                    {
                        found = GetDbField("FCUSTOMERS", "", "firstName", qualify, workContract, ref data);
                        dt.Rows[i]["F2"] = data.Trim() + " ";
                        found = GetDbField("FCUSTOMERS", "", "middleName", qualify, workContract, ref data);
                        dt.Rows[i]["F2"] += data.Trim() + " ";
                        found = GetDbField("FCUSTOMERS", "", "maidenName", qualify, workContract, ref data);
                        if ( String.IsNullOrWhiteSpace ( data ))
                        {
                            found = GetDbField("FCUSTOMERS", "", "lastName", qualify, workContract, ref data);
                            dt.Rows[i]["F2"] += data.Trim();
                        }
                        else
                            dt.Rows[i]["F2"] += data.Trim();
                    }
                }
            }

            FinalizeData(dt);

            return dt;
        }
        /***********************************************************************************************/
        private void FinalizeData ( DataTable dt )
        {
            string table = "";
            string dbfield = "";
            string qualify = "";
            string data = "";
            string cmd = "";
            string field = "";
            string dispositionType = "";
            bool found = false;
            DataRow[] dRows = null;
            string location = "";
            string address = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                table = dt.Rows[i]["table"].ObjToString();
                dbfield = dt.Rows[i]["dbfield"].ObjToString();
                if (String.IsNullOrWhiteSpace(table))
                    continue;
                if (String.IsNullOrWhiteSpace(dbfield))
                    continue;
                field = dt.Rows[i]["field"].ObjToString();
                if (field == "Disposition Location Name")
                {
                    dRows = dt.Select("field='Disposition Type'");
                    if (dRows.Length > 0)
                    {
                        dispositionType = dRows[0]["F2"].ObjToString();
                        //GetDispositionInfo(dispositionType, ref location, ref address );
                        RTF_Stuff.GetDispositionInfo(workContract, dispositionType, ref location, ref address);
                        dt.Rows[i]["F2"] = location;
                        dRows = dt.Select("field='Disposition City State'");
                        if (dRows.Length > 0)
                            dRows[0]["F2"] = address;
                    }
                }
            }
        }
        /***********************************************************************************************/
        private void GetDispositionInfo(string dispositionType, ref string location, ref string address )
        {
            location = "";
            address = "";
            if (String.IsNullOrWhiteSpace(dispositionType))
                return;

            string cmd = "Select * from `fcust_extended` where `contractNumber` = '" + workContract + "' ";
            cmd += ";";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
                return;
            if (dispositionType.ToUpper() == "CREMATION")
            {
                location = dx.Rows[0]["crematorium"].ObjToString();
                address = dx.Rows[0]["CremCityStateZip"].ObjToString();
            }
            else
            {
                location = dx.Rows[0]["cem"].ObjToString();
                address = dx.Rows[0]["cemctyst"].ObjToString();
            }
        }
        /***********************************************************************************************/
        private bool GetDbField(string table, string formField, string field, string qualify, string contractNumber, ref string data )
        {
            data = "";
            string cmd = "";
            string str = "";
            string additional = "";
            bool gotDate = false;
            if (qualify.ToUpper().IndexOf("$DATE") == 0)
                gotDate = true;
            bool found = true;
            if (table.ToUpper() == "CUSTOMERS")
                table = "FCUSTOMERS";
            else if (table.ToUpper() == "CONTRACTS")
                table = "FCONTRACTS";
            else if (table.ToUpper() == "CUST_EXTENDED")
                table = "FCUST_EXTENDED";

            //if (table.ToUpper() == "RELATIVES")
            //    return "";
            if ( !String.IsNullOrWhiteSpace ( qualify))
            {
            }

            int idx = 0;
            if (!String.IsNullOrWhiteSpace(qualify) && !gotDate )
            {
                if (!String.IsNullOrWhiteSpace(field))
                {
                    string[] Lines = qualify.Split('=');
                    if (Lines.Length == 2)
                    {
                        additional = "`" + Lines[0] + "` = '" + Lines[1] + "'";
                        if (additional.ToUpper().IndexOf("`NEXTOFKIN") == 0)
                            additional += " AND `depRelationship` <> 'DISCLOSURES'";
                        if ( Lines[1].ToUpper() == "SPOUSE" && Lines[0].ToUpper() == "DEPRELATIONSHIP")
                        {
                            additional = "(`" + Lines[0] + "` = '" + Lines[1] + "' ";
                            additional += " OR `" + Lines[0] + "` = 'Wife' ";
                            additional += " OR `" + Lines[0] + "` = 'Husband' ) ";
                            if ( formField.ToUpper().IndexOf ( "SURVIVING") >= 0 )
                                additional += " AND `deceased` <> '1' ";
                        }
                        idx = G1.StripNumeric(field);
                    }
                }
            }

            try
            {
                cmd = "Select * from `" + table + "` where `contractNumber` = '" + contractNumber + "' ";
                if (table.ToUpper() == "FUNERALHOMES")
                    cmd = "Select * from `funeralhomes` where `atneedcode` = '" + LoginForm.activeFuneralHomeKeyCode + "' ";
                if (!String.IsNullOrWhiteSpace(additional))
                    cmd += " AND " + additional;
                cmd += ";";

                DataTable dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    string strDelimitor = " +";
                    string[] Lines = field.Split(new[] { strDelimitor }, StringSplitOptions.None);
                    for (int i = 0; i < Lines.Length; i++)
                    {
                        field = Lines[i].Trim();
                        if (String.IsNullOrWhiteSpace(field))
                            continue;
                        try
                        {
                            if (field.ToUpper() == "DECAGE")
                            {
                                DateTime birthDate = dx.Rows[0]["birthDate"].ObjToDateTime();
                                DateTime deathDate = dx.Rows[0]["deceasedDate"].ObjToDateTime();
                                data = G1.GetAge(birthDate, deathDate).ToString();
                            }
                            else if (field.Trim() != "+")
                            {
                                //str = dx.Rows[0][field].ObjToString();
                                //data += str;
                                if ( G1.get_column_number ( dx, field) < 0 )
                                {
                                }
                                if (idx > 0)
                                    str = dx.Rows[idx - 1][field].ObjToString().Trim();
                                else
                                    str = dx.Rows[0][field].ObjToString().Trim();

                                if (str.ToUpper() == "FEMALE")
                                    str = "Female";
                                else if (str.ToUpper() == "MALE")
                                    str = "Male";

                                data = data.Trim();
                                if ( formField == "Surviving Spouse Name" && field.ToUpper() == "MAIDENNAME" && table.ToUpper() == "RELATIVES")
                                {
                                    if ( String.IsNullOrEmpty ( str ))
                                        str = dx.Rows[0]["depLastName"].ObjToString().Trim();
                                }

                                if (qualify.ToUpper() == "$MONTH" && G1.validate_date(str))
                                    str = str.ObjToDateTime().ToString("MMMMMMMMMMMMM");
                                else if (qualify.ToUpper() == "$DAY" && G1.validate_date(str))
                                    str = str.ObjToDateTime().Day.ToString();
                                else if (qualify.ToUpper() == "$YEAR" && G1.validate_date(str))
                                    str = str.ObjToDateTime().Year.ToString();
                                else if (qualify.ToUpper() == "$MM,DD,YYYY" && G1.validate_date(str))
                                    str = str.ObjToDateTime().ToString("MM,dd,yyyy");
                                else if (qualify.ToUpper() == "$NOW")
                                    str = DateTime.Now.ToString("MM/dd/yyyy");
                                data += " " + str;
                            }
                        }
                        catch ( Exception ex)
                        {
                            found = false;
                            if (field == ",")
                                data += field + " ";
                            else
                                data += " " + field;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Looking up Table " + table + " Field " + field + " For ContractNumber " + contractNumber + "!!");
            }
            data = data.Trim();
            if (field.ToUpper() == "POD")
            {
                if (String.IsNullOrWhiteSpace(data))
                {
                    string address = RTF_Stuff.GetDeathPlaceInfo(workContract);
                    if (!String.IsNullOrWhiteSpace(address))
                        data += address;
                    //cmd = "Select * from `fcustomers` where `contractNumber` = '" + workContract + "';";
                    //DataTable dx = G1.get_db_data(cmd);
                    //if (dx.Rows.Count > 0)
                    //{
                    //    data = dx.Rows[0]["address1"].ObjToString();
                    //    string address2 = dx.Rows[0]["address2"].ObjToString();
                    //    if (!String.IsNullOrWhiteSpace(address2))
                    //        data += address2;
                    //}
                }
            }
            return found;
        }
        /***********************************************************************************************/
        private void ViewForm_PdfDone(string filename, string record, string contractNumber, byte[] b)
        {
            string formname = G1.DecodeFilename(filename);
            byte[] result = null; // Save Customer PDF Form
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

                string data = "";
                for (int i = 0; i < Lines.Length; i++)
                {
                    string name = Lines[i].Trim();
                    str = fields.GetField(name);
                    data += name + "\n" + str + "\n";
                }
                pdfStamper.Close();
                reader.Close();
                result = mo.ToArray();
                string cmd = "Select * from `agreements` where `formName` = '" + filename + "' AND `contractNumber` = '" + workContract + "' ";
                DataTable dt = G1.get_db_data(cmd);
                if (dt.Rows.Count > 0)
                {
                    record = dt.Rows[0]["record"].ObjToString();
                    G1.update_blob("agreements", "record", record, "image", b);
                    G1.update_db_table("agreements", "record", record, new string[] {"modified", "Y" });

                    LoadData();
                }
                else
                {
                    record = G1.create_record("agreements", "formName", "-1");
                    if (G1.BadRecord("agreements", record))
                        return;
                    G1.update_blob("agreements", "record", record, "image", b);
                    G1.update_db_table("agreements", "record", record, new string[] {"contractNumber", workContract, "formName", filename });
                    G1.update_db_table("agreements", "record", record, new string[] { "modified", "Y" });

                    LoadData();

                    //cmd = "Select * from `agreements` where `contractNumber` = '" + workContract + "';";
                    //dt = G1.get_db_data(cmd);
                    //dt.Columns.Add("num");
                    //dt.Columns.Add("mod");
                    //G1.NumberDataTable(dt);
                    //dgv.DataSource = dt;
                    //dgv.RefreshDataSource();
                    //dgv.Refresh();
                }
            }
            return;
        }
        /****************************************************************************************/
        private void editDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            string form = dr["formName"].ObjToString();
            string location = dr["location"].ObjToString();
            EditFormData editForm = new EditFormData(workContract, form, location, record);
            editForm.Show();
        }
        /****************************************************************************************/
        private void reloadOriginalDocumentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            string form = dr["formName"].ObjToString();
            string location = dr["location"].ObjToString();
            LoadUpForm(form, location, record, true);
        }
        /****************************************************************************************/
        private bool showForms = false;
        private bool showContract = false;
        private bool showMarque = false;
        private bool showProgram = false;
        private bool showTube = false;
        private bool showObit = false;
        private bool showDirectorsReport = false;
        private bool show_MS_DeathCert = false;
        private void ClearColor()
        {
            if (showContract || editFunContract != null)
                editFunContract.Close();
            if (showMarque || editFunMarque != null)
            {
                editFunMarque.FireEventSaveFunForms(true);
                editFunMarque.Close();
            }
            if (showProgram || editFunProgram != null)
            {
                editFunProgram.FireEventSaveFunForms(true);
                editFunProgram.Close();
            }
            if (showTube || editFunTube != null)
            {
                editFunTube.FireEventSaveFunForms(true);
                editFunTube.Close();
            }
            if (showObit || editFunObit != null)
            {
                if (editFunObit != null)
                {
                    editFunObit.FireEventSaveFunForms(true);
                    editFunObit.Close();
                }
            }
            if (showDirectorsReport || editFunDirectorsReport != null)
            {
                if (editFunDirectorsReport != null)
                {
                    editFunDirectorsReport.FireEventSaveFunForms(true);
                    editFunDirectorsReport.Close();
                }
            }
            if (show_MS_DeathCert || editMsDeathCertProgram != null)
            {
                if (editMsDeathCertProgram != null)
                {
                    editMsDeathCertProgram.FireEventSaveFunForms(true);
                    editMsDeathCertProgram.Close();
                }
            }
            editFunContract = null;
            editFunMarque = null;
            editFunProgram = null;
            editFunTube = null;
            editFunObit = null;
            editFunDirectorsReport = null;
            editMsDeathCertProgram = null;

            showForms = false;
            showContract = false;
            showMarque = false;
            showProgram = false;
            showTube = false;
            showObit = false;
            showDirectorsReport = false;
            show_MS_DeathCert = false;

            btnForms.BackColor = Color.Transparent;
            //btnMarque.BackColor = Color.Transparent;
            //btnProgram.BackColor = Color.Transparent;
            //btnTube.BackColor = Color.Transparent;
            //btnContract.BackColor = Color.Transparent;
            //btnObit.BackColor = Color.Transparent;
            //btnDirectorsReport.BackColor = Color.Transparent;
            //btnDeathCert.BackColor = Color.Transparent;
        }
        /****************************************************************************************/
        private void btnForms_Click(object sender, EventArgs e)
        {
            btnBiography.Show();
            btnProcess.Show();
            //ClearColor();
            dgv.Dock = DockStyle.Fill;
            dgv.Visible = true;
            panelBottomRightForms.Dock = DockStyle.Fill;
            panelBottomRightForms.Visible = true;
            btnForms.BackColor = Color.Yellow;
            showForms = true;
            gridMain.RefreshData();
        }
        /****************************************************************************************/
        //private void btnMarque_Click(object sender, EventArgs e)
        //{
        //    btnBiography.Hide();
        //    btnProcess.Hide();
        //    this.Cursor = Cursors.WaitCursor;
        //    ClearColor();
        //    panelBottomRightForms.Visible = false;
        //    dgv.Visible = false;
        //    //InitializeMarquePanel();
        //    InitializeSomethingPanel(ref editFunMarque, ref marqueModified, "Marque");
        //    panelBottomRightRTF.Dock = DockStyle.Fill;
        //    panelBottomRightRTF.Visible = true;
        //    //btnMarque.BackColor = Color.Yellow;
        //    showMarque = true;
        //    btnForms_Click(null, null);
        //    gridMain.RefreshData();
        //    this.Cursor = Cursors.Default;
        //}
        /***********************************************************************************************/
        private ArrangementForms editFunMarque = null;
        private bool marqueModified = false;
        private void InitializeMarquePanel()
        {
            if (editFunMarque != null)
                editFunMarque.Close();
            editFunMarque = null;
            marqueModified = false;
            G1.ClearPanelControls(this.panelBottomRightRTF);

            string record = "";
            string str = "";
            string location = EditCust.activeFuneralHomeName;
            string cmd = "Select * from `arrangementforms` where `formName` = 'Marque' ";
            if (!String.IsNullOrWhiteSpace(location))
                cmd += " AND `location` = '" + location + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
            {
                cmd = "Select * from `arrangementforms` where `formName` = 'Marque' ";
                dt = G1.get_db_data(cmd);
            }
            if (dt.Rows.Count > 0)
            {
                record = dt.Rows[0]["record"].ObjToString();
                str = G1.get_db_blob("arrangementforms", record, "image");
                location = dt.Rows[0]["location"].ObjToString();
            }
            if (str.IndexOf("rtf1") < 0)
                return;

            byte[] b = Encoding.UTF8.GetBytes(str);
            editFunMarque = new ArrangementForms("Marque", location, record, workContract, b, true);
            editFunMarque.RtfFinished += ArrangementForm_RtfDone;
            editFunMarque.RtfModified += ArrangementForm_RtfModified;
            if (!this.LookAndFeel.UseDefaultLookAndFeel)
            {
                editFunMarque.LookAndFeel.UseDefaultLookAndFeel = false;
                editFunMarque.LookAndFeel.SetSkinStyle(this.LookAndFeel.SkinName);
            }
            G1.LoadFormInPanel(editFunMarque, this.panelBottomRightRTF);
            this.panelBottomRightRTF.Dock = DockStyle.Fill;
        }
        /***********************************************************************************************/
        private ArrangementForms editFunDirectorsReport = null;
        private bool directorsReportModified = false;
        private void InitializeDirectorsReportPanel()
        {
            if (editFunDirectorsReport != null)
                editFunDirectorsReport.Close();
            editFunDirectorsReport = null;
            directorsReportModified = false;
            G1.ClearPanelControls(this.panelBottomRightRTF);

            string record = "";
            string str = "";
            string location = EditCust.activeFuneralHomeName;
            string cmd = "Select * from `arrangementforms` where `formName` = 'Directors Report' ";
            if (!String.IsNullOrWhiteSpace(location))
                cmd += " AND `location` = '" + location + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
            {
                cmd = "Select * from `arrangementforms` where `formName` = 'Directors Report' ";
                dt = G1.get_db_data(cmd);
            }
            if (dt.Rows.Count > 0)
            {
                record = dt.Rows[0]["record"].ObjToString();
                str = G1.get_db_blob("arrangementforms", record, "image");
                location = dt.Rows[0]["location"].ObjToString();
            }
            if (str.IndexOf("rtf1") < 0)
                return;

            byte[] b = Encoding.UTF8.GetBytes(str);
            editFunDirectorsReport = new ArrangementForms("Directors Report", location, record, workContract, b, true);
            editFunDirectorsReport.RtfFinished += ArrangementForm_RtfDone;
            editFunDirectorsReport.RtfModified += ArrangementForm_RtfModified;
            if (!this.LookAndFeel.UseDefaultLookAndFeel)
            {
                editFunDirectorsReport.LookAndFeel.UseDefaultLookAndFeel = false;
                editFunDirectorsReport.LookAndFeel.SetSkinStyle(this.LookAndFeel.SkinName);
            }
            G1.LoadFormInPanel(editFunDirectorsReport, this.panelBottomRightRTF);
            this.panelBottomRightRTF.Dock = DockStyle.Fill;
        }
        ///****************************************************************************************/
        //private void btnContract_Click(object sender, EventArgs e)
        //{
        //    if ( conActive != null)
        //    {
        //        try
        //        {
        //            conActive.Close();
        //            conActive = null;
        //        }
        //        catch ( Exception ex)
        //        {
        //        }
        //    }
        //    this.Cursor = Cursors.WaitCursor;
        //    conActive = new Contract1(workContract, workServicesDt, workPaymentsDt);
        //    conActive.Show();
        //    this.Cursor = Cursors.Default;
        //}
        /***********************************************************************************************/
        private Contract1 editFunContract = null;
        private bool contractModified = false;
        private void InitializeContractPanel()
        {
            if (editFunContract != null)
                editFunContract.Close();
            editFunContract = null;
            contractModified = false;
            G1.ClearPanelControls(this.panelBottomRightRTF);
            editFunContract = new Contract1(workContract, workServicesDt, workPaymentsDt);
            editFunContract.Show();
            //            editFunContract.RtfFinished += ArrangementForm_RtfDone;
            if (!this.LookAndFeel.UseDefaultLookAndFeel)
            {
                editFunContract.LookAndFeel.UseDefaultLookAndFeel = false;
                editFunContract.LookAndFeel.SetSkinStyle(this.LookAndFeel.SkinName);
            }
            G1.LoadFormInPanel(editFunContract, this.panelBottomRightRTF);
            this.panelBottomRightRTF.Dock = DockStyle.Fill;
        }
        /***********************************************************************************************/
        private void InitializeSomethingPanel(ref ArrangementForms editForm, ref bool formModified, string formName, bool fromForm )
        {
            if (editForm != null)
                editForm.Close();
            editForm = null;
            funModified = false;
            formModified = false;
            G1.ClearPanelControls(this.panelBottomRightRTF);

            if (formName.ToUpper() == "CASKET TUBE")
                formName = "SMFS_Casket_Tube_Record";

            string record = "";
            string record1 = "";
            string str = "";
            string table = "agreements";
            string location = EditCust.activeFuneralHomeName;
            string formType = "Custom";
            string cmd = "Select * from `agreements` where `formName` = '" + formName + "' AND `contractNumber` = '" + workContract + "' ";
            if (!fromForm)
            {
                if (!String.IsNullOrWhiteSpace(location))
                    cmd += " AND `location` = '" + location + "' ";
            }
            cmd += ";";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0 ) // RAmma
            {
                cmd = "Select * from `arrangementforms` where `formName` = '" + formName + "' ";
                if (!String.IsNullOrWhiteSpace(location))
                    cmd += " AND `location` = '" + location + "' ";
                else
                    cmd += " AND `location` = '' ";
                cmd += ";";
                formType = location;
                dt = G1.get_db_data(cmd);
                if ( dt.Rows.Count <= 0 && !String.IsNullOrWhiteSpace ( location ))
                {
                    cmd = "Select * from `arrangementforms` where `formName` = '" + formName + "' ";
                    cmd += " AND `location` = '' ";
                    cmd += ";";
                    dt = G1.get_db_data(cmd);
                    if (dt.Rows.Count > 0)
                        formType = "Generic";
                }
                table = "arrangementforms";
                if (dt.Rows.Count > 0)
                    record1 = dt.Rows[0]["record"].ObjToString();
            }
            bool useForm = false;
            if (dt.Rows.Count > 0)
            {
                record = dt.Rows[0]["record"].ObjToString();
                str = G1.get_db_blob(table, record, "image");
                location = dt.Rows[0]["location"].ObjToString();
                if (String.IsNullOrWhiteSpace(str))
                {
                    cmd = "Select * from `arrangementforms` where `formName` = '" + formName + "';";
                    dt = G1.get_db_data(cmd);
                    if (dt.Rows.Count > 0)
                    {
                        record = dt.Rows[0]["record"].ObjToString();
                        record1 = record;
                        str = G1.get_db_blob("arrangementforms", record, "image");
                        useForm = true;
                    }
                }
            }
            if (str.IndexOf("rtf1") < 0)
            {
                if (str.IndexOf("PDF") > 0)
                {
                    bool custom = true;
                    string command = "Select `image` from `agreements` where `record` = '" + record + "';";
                    if (!String.IsNullOrWhiteSpace(record1))
                    {
                        command = "Select `image` from `arrangementforms` where `record` = '" + record1 + "';";
                        custom = false;
                    }
                    MySqlCommand cmd1 = new MySqlCommand(command, G1.conn1);
                    cmd1.Connection.Open();
                    try
                    {
                        using (MySqlDataReader dR = cmd1.ExecuteReader(System.Data.CommandBehavior.Default))
                        {
                            if (dR.Read())
                            {
                                byte[] fileData = (byte[])dR.GetValue(0);
                                byte[] results = ReplaceFields(fileData, formName);
                                this.Cursor = Cursors.WaitCursor;
                                if (!custom)
                                {
                                    ViewPDF viewForm = new ViewPDF(formName, record, workContract, results, fileData);
                                    viewForm.PdfDone += ViewForm_PdfDone;
                                    viewForm.Show();
                                }
                                else
                                {
                                    ViewPDF viewForm = new ViewPDF(formName, record, workContract, fileData, fileData);
                                    viewForm.PdfDone += ViewForm_PdfDone;
                                    viewForm.Show();
                                }
                                this.Cursor = Cursors.Default;
                                //btnDeathCert.BackColor = Color.Transparent;
                                //btnDeathCert.Refresh();
                                btnForms_Click(null, null);
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
                return;
            }

            byte[] b = Encoding.UTF8.GetBytes(str);
            //location = formType;
            editForm = new ArrangementForms(formName, location, record, workContract, b, true, false, "", fromForm );
            editForm.RtfFinished += ArrangementForm_RtfDone;
            editForm.RtfModified += ArrangementForm_RtfModified;
            if (!this.LookAndFeel.UseDefaultLookAndFeel)
            {
                editForm.LookAndFeel.UseDefaultLookAndFeel = false;
                editForm.LookAndFeel.SetSkinStyle(this.LookAndFeel.SkinName);
            }
            editForm.Show();
            btnForms_Click(null, null);
            editForm.BringToFront();
            //G1.LoadFormInPanel(editForm, this.panelBottomRightRTF);
            //this.panelBottomRightRTF.Dock = DockStyle.Fill;
        }
        /***********************************************************************************************/
        private void InitializeSomethingPanelOld(ref ArrangementForms editForm, ref bool formModified, string formName)
        {
            if (editForm != null)
                editForm.Close();
            editForm = null;
            funModified = false;
            formModified = false;
            G1.ClearPanelControls(this.panelBottomRightRTF);

            string record = "";
            string record1 = "";
            string str = "";
            string table = "agreements";
            string location = EditCust.activeFuneralHomeName;
            string cmd = "Select * from `agreements` where `formName` = '" + formName + "' AND `contractNumber` = '" + workContract + "' ";
            if (String.IsNullOrWhiteSpace(location))
                cmd += " AND `location` = '" + location + "' ";
            cmd += ";";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0 || 1 == 1)
            {
                cmd = "Select * from `arrangementforms` where `formName` = '" + formName + "';";
                dt = G1.get_db_data(cmd);
                table = "arrangementforms";
                if (dt.Rows.Count > 0)
                    record1 = dt.Rows[0]["record"].ObjToString();
            }
            bool useForm = false;
            if (dt.Rows.Count > 0)
            {
                record = dt.Rows[0]["record"].ObjToString();
                str = G1.get_db_blob(table, record, "image");
                location = dt.Rows[0]["location"].ObjToString();
                if (String.IsNullOrWhiteSpace(str))
                {
                    cmd = "Select * from `arrangementforms` where `formName` = '" + formName + "';";
                    dt = G1.get_db_data(cmd);
                    if (dt.Rows.Count > 0)
                    {
                        record = dt.Rows[0]["record"].ObjToString();
                        record1 = record;
                        str = G1.get_db_blob("arrangementforms", record, "image");
                        useForm = true;
                    }
                }
            }
            if (str.IndexOf("rtf1") < 0)
            {
                if (str.IndexOf("PDF") > 0)
                {
                    bool custom = true;
                    string command = "Select `image` from `agreements` where `record` = '" + record + "';";
                    if (!String.IsNullOrWhiteSpace(record1))
                    {
                        command = "Select `image` from `arrangementforms` where `record` = '" + record1 + "';";
                        custom = false;
                    }
                    MySqlCommand cmd1 = new MySqlCommand(command, G1.conn1);
                    cmd1.Connection.Open();
                    try
                    {
                        using (MySqlDataReader dR = cmd1.ExecuteReader(System.Data.CommandBehavior.Default))
                        {
                            if (dR.Read())
                            {
                                byte[] fileData = (byte[])dR.GetValue(0);
                                byte[] results = ReplaceFields(fileData, formName);
                                this.Cursor = Cursors.WaitCursor;
                                if (!custom)
                                {
                                    ViewPDF viewForm = new ViewPDF(formName, record, workContract, results, fileData);
                                    viewForm.PdfDone += ViewForm_PdfDone;
                                    viewForm.Show();
                                }
                                else
                                {
                                    ViewPDF viewForm = new ViewPDF(formName, record, workContract, fileData, fileData);
                                    viewForm.PdfDone += ViewForm_PdfDone;
                                    viewForm.Show();
                                }
                                this.Cursor = Cursors.Default;
                                //btnDeathCert.BackColor = Color.Transparent;
                                //btnDeathCert.Refresh();
                                btnForms_Click(null, null);
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
                return;
            }

            byte[] b = Encoding.UTF8.GetBytes(str);
            editForm = new ArrangementForms(formName, location, record, workContract, b, true);
            editForm.RtfFinished += ArrangementForm_RtfDone;
            editForm.RtfModified += ArrangementForm_RtfModified;
            if (!this.LookAndFeel.UseDefaultLookAndFeel)
            {
                editForm.LookAndFeel.UseDefaultLookAndFeel = false;
                editForm.LookAndFeel.SetSkinStyle(this.LookAndFeel.SkinName);
            }
            editForm.Show();
            btnForms_Click(null, null);
            editForm.BringToFront();
            //G1.LoadFormInPanel(editForm, this.panelBottomRightRTF);
            //this.panelBottomRightRTF.Dock = DockStyle.Fill;
        }
        /***********************************************************************************************/
        private ViewPDF viewForm = null;
        private void InitializeSomethingPanelx( ref ArrangementForms editForm, ref bool formModified, string formName )
        {
            if (editForm != null)
                editForm.Close();
            editForm = null;

            if (viewForm != null)
                viewForm.Close();
            viewForm = null;

            funModified = false;
            formModified = false;
            G1.ClearPanelControls(this.panelBottomRightRTF);

            string record = "";
            string record1 = "";
            string str = "";
            string table = "agreements";
            string location = EditCust.activeFuneralHomeName;
            string cmd = "Select * from `agreements` where `formName` = '" + formName + "' AND `contractNumber` = '" + workContract + "' ";
            if (String.IsNullOrWhiteSpace(location))
                cmd += " AND `location` = '" + location + "' ";
            cmd += ";";
            DataTable dt = G1.get_db_data(cmd);
            if ( dt.Rows.Count <= 0 )
            {
                cmd = "Select * from `arrangementforms` where `formName` = '" + formName + "';";
                dt = G1.get_db_data(cmd);
            }
            //if (dt.Rows.Count <= 0)
            //{
            //    cmd = "Select * from `agreements` where `formName` = '" + formName + "' AND `contractNumber` = '" + workContract + "' ";
            //    if (!String.IsNullOrWhiteSpace(location))
            //        cmd = "Select * from `agreements` where `formName` = '" + location + " " + formName + "' AND `contractNumber` = '" + workContract + "' ";
            //    cmd += ";";
            //    dt = G1.get_db_data(cmd);
            //    if ( dt.Rows.Count <= 0 )
            //    {
            //        cmd = "Select * from `agreements` where `formName` = '" + formName + "' ";
            //        if (!String.IsNullOrWhiteSpace(location))
            //            cmd = "Select * from `agreements` where `formName` = '" + location + " " + formName + "' ";
            //        cmd += ";";
            //        dt = G1.get_db_data(cmd);
            //        if ( dt.Rows.Count <= 0 )
            //        {
            //            cmd = "Select * from `arrangementforms` where `formName` = '" + formName + "';";
            //            if (!String.IsNullOrWhiteSpace(location))
            //                cmd = "Select * from `arrangementforms` where `formName` = '" + location + " " + formName + "' ";
            //            dt = G1.get_db_data(cmd);
            //            if ( dt.Rows.Count <= 0 )
            //            {
            //                cmd = "Select * from `arrangementforms` where `formName` = '" + formName + "';";
            //                dt = G1.get_db_data(cmd);
            //            }
            //        }
            //    }
            //    //cmd = "Select * from `arrangementforms` where `formName` = '" + formName + "';";
            //    //dt = G1.get_db_data(cmd);
            //    if ( dt.Rows.Count <= 0 )
            //    {
            //        MessageBox.Show("A critical exception has occurred while attempting to read Form : " + formName + "!!!", "Read Form Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            //        return;

            //    }
            //    table = "arrangementforms";
            //    if ( dt.Rows.Count > 0 )
            //        record1 = dt.Rows[0]["record"].ObjToString();
            //}
            table = "arrangementforms";
            if (dt.Rows.Count > 0)
                record1 = dt.Rows[0]["record"].ObjToString();
            bool useForm = false;
            if (dt.Rows.Count > 0)
            {
                record = dt.Rows[0]["record"].ObjToString();
                str = G1.get_db_blob(table, record, "image");
                location = dt.Rows[0]["location"].ObjToString();
                if (String.IsNullOrWhiteSpace(str))
                {
                    cmd = "Select * from `arrangementforms` where `formName` = '" + formName + "';";
                    dt = G1.get_db_data(cmd);
                    if (dt.Rows.Count > 0)
                    {
                        record = dt.Rows[0]["record"].ObjToString();
                        record1 = record;
                        str = G1.get_db_blob("arrangementforms", record, "image");
                        useForm = true;
                    }
                }
            }
            if (str.IndexOf("rtf1") < 0)
            {
                if (str.IndexOf("PDF") > 0)
                {
                    bool custom = true;
                    string command = "Select `image` from `agreements` where `record` = '" + record + "';";
                    if (!String.IsNullOrWhiteSpace(record1))
                    {
                        command = "Select `image` from `arrangementforms` where `record` = '" + record1 + "';";
                        custom = false;
                    }
                    MySqlCommand cmd1 = new MySqlCommand(command, G1.conn1);
                    cmd1.Connection.Open();
                    try
                    {
                        using (MySqlDataReader dR = cmd1.ExecuteReader(System.Data.CommandBehavior.Default))
                        {
                            if (dR.Read())
                            {
                                byte[] fileData = (byte[])dR.GetValue(0);
                                byte[] results = ReplaceFields(fileData, formName);
                                this.Cursor = Cursors.WaitCursor;
                                if (!custom)
                                {
                                    viewForm = new ViewPDF(formName, record, workContract, results, fileData );
                                    viewForm.PdfDone += ViewForm_PdfDone;
                                    viewForm.Show();
                                }
                                else
                                {
                                    viewForm = new ViewPDF(formName, record, workContract, fileData, fileData );
                                    viewForm.PdfDone += ViewForm_PdfDone;
                                    viewForm.Show();
                                }
                                this.Cursor = Cursors.Default;
                                //btnDeathCert.BackColor = Color.Transparent;
                                //btnDeathCert.Refresh();
                                btnForms_Click(null, null);
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
                return;
            }

            byte[] b = Encoding.UTF8.GetBytes(str);
            editForm = new ArrangementForms(formName, location, record, workContract, b, true);
            editForm.RtfFinished += ArrangementForm_RtfDone;
            editForm.RtfModified += ArrangementForm_RtfModified;
            if (!this.LookAndFeel.UseDefaultLookAndFeel)
            {
                editForm.LookAndFeel.UseDefaultLookAndFeel = false;
                editForm.LookAndFeel.SetSkinStyle(this.LookAndFeel.SkinName);
            }
            editForm.Show();
            btnForms_Click(null, null);
            editForm.BringToFront();
            //G1.LoadFormInPanel(editForm, this.panelBottomRightRTF);
            //this.panelBottomRightRTF.Dock = DockStyle.Fill;
        }
        /***********************************************************************************************/
        private void ViewForm_RtfModified()
        {
            funModified = true;
        }
        /***********************************************************************************************/
        private ArrangementForms editFunProgram = null;
        private bool programModified = false;
        /***********************************************************************************************/
        //private ArrangementForms editFunObit = null;
        //private bool obitModified = false;
        //private void InitializeProgramPanel()
        //{
        //    if (editFunProgram != null)
        //        editFunProgram.Close();
        //    editFunProgram = null;
        //    programModified = false;
        //    G1.ClearPanelControls(this.panelBottomRightRTF);

        //    string record = "";
        //    string str = "";
        //    string table = "agreements";
        //    string location = EditCust.activeFuneralHomeName;
        //    string cmd = "Select * from `agreements` where `formName` = 'Collins Program Template' AND `contractNumber` = '" + workContract + "' ";
        //    if (String.IsNullOrWhiteSpace(location))
        //        cmd += " AND `location` = '" + location + "' ";
        //    cmd += ";";
        //    DataTable dt = G1.get_db_data(cmd);
        //    if (dt.Rows.Count <= 0)
        //    {
        //        cmd = "Select * from `arrangementforms` where `formName` = 'Collins Program Template';";
        //        dt = G1.get_db_data(cmd);
        //        table = "arrangementforms";
        //    }
        //    if (dt.Rows.Count > 0)
        //    {
        //        record = dt.Rows[0]["record"].ObjToString();
        //        str = G1.get_db_blob(table, record, "image");
        //        location = dt.Rows[0]["location"].ObjToString();
        //    }
        //    if (str.IndexOf("rtf1") < 0)
        //        return;

        //    byte[] b = Encoding.UTF8.GetBytes(str);
        //    editFunProgram = new ArrangementForms("Collins Program Template", location, record, workContract, b, true);
        //    editFunProgram.RtfFinished += ArrangementForm_RtfDone;
        //    editFunProgram.RtfModified += ArrangementForm_RtfModified;
        //    if (!this.LookAndFeel.UseDefaultLookAndFeel)
        //    {
        //        editFunProgram.LookAndFeel.UseDefaultLookAndFeel = false;
        //        editFunProgram.LookAndFeel.SetSkinStyle(this.LookAndFeel.SkinName);
        //    }
        //    G1.LoadFormInPanel(editFunProgram, this.panelBottomRightRTF);
        //    this.panelBottomRightRTF.Dock = DockStyle.Fill;
        //}

        //private void InitializeProgramPanel()
        //{
        //    if (editFunProgram != null)
        //        editFunProgram.Close();
        //    editFunProgram = null;
        //    programModified = false;
        //    G1.ClearPanelControls(this.panelBottomRightRTF);

        //    string record = "";
        //    string str = "";
        //    string location = EditCust.activeFuneralHomeName;
        //    string cmd = "Select * from `arrangementforms` where `formName` = 'Collins Program Template' ";
        //    if (!String.IsNullOrWhiteSpace(location))
        //        cmd += " AND `location` = '" + location + "' ";
        //    cmd += ";";
        //    DataTable dt = G1.get_db_data(cmd);
        //    if (dt.Rows.Count <= 0)
        //    {
        //        cmd = "Select * from `arrangementforms` where `formName` = 'Collins Program Template' ";
        //        cmd += ";";
        //        dt = G1.get_db_data(cmd);
        //    }
        //    if (dt.Rows.Count > 0)
        //    {
        //        record = dt.Rows[0]["record"].ObjToString();
        //        str = G1.get_db_blob("arrangementforms", record, "image");
        //        location = dt.Rows[0]["location"].ObjToString();
        //    }
        //    if (str.IndexOf("rtf1") < 0)
        //        return;

        //    byte[] b = Encoding.UTF8.GetBytes(str);
        //    editFunProgram = new ArrangementForms("Collins Program Template", location, record, workContract, b, true);
        //    editFunProgram.RtfFinished += ArrangementForm_RtfDone;
        //    editFunProgram.RtfModified += ArrangementForm_RtfModified;
        //    if (!this.LookAndFeel.UseDefaultLookAndFeel)
        //    {
        //        editFunProgram.LookAndFeel.UseDefaultLookAndFeel = false;
        //        editFunProgram.LookAndFeel.SetSkinStyle(this.LookAndFeel.SkinName);
        //    }
        //    G1.LoadFormInPanel(editFunProgram, this.panelBottomRightRTF);
        //    this.panelBottomRightRTF.Dock = DockStyle.Fill;
        //}
        /****************************************************************************************/
        //private void btnProgram_Click(object sender, EventArgs e)
        //{
        //    btnBiography.Hide();
        //    btnProcess.Hide();
        //    this.Cursor = Cursors.WaitCursor;
        //    ClearColor();
        //    panelBottomRightForms.Visible = false;
        //    dgv.Visible = false;
        //    //InitializeProgramPanel();
        //    InitializeSomethingPanel(ref editFunProgram, ref programModified, "SMFS Program Template");
        //    panelBottomRightRTF.Dock = DockStyle.Fill;
        //    panelBottomRightRTF.Visible = true;
        //    //btnProgram.BackColor = Color.Yellow;
        //    showProgram = true;
        //    gridMain.RefreshData();
        //    this.Cursor = Cursors.Default;
        //    editFunProgram.BringToFront();
        //}
        /***********************************************************************************************/
        private ArrangementForms editFunTube = null;
        private bool tubeModified = false;
        private void InitializeTubePanel()
        {
            if (editFunTube != null)
                editFunTube.Close();
            editFunTube = null;
            tubeModified = false;
            G1.ClearPanelControls(this.panelBottomRightRTF);

            string record = "";
            string str = "";
            string location = EditCust.activeFuneralHomeName;
            string cmd = "Select * from `arrangementforms` where `formName` = 'SMFS_Casket_Tube_Record' ";
            if (String.IsNullOrWhiteSpace(location))
                cmd += " AND `location` = '" + location + "' ";
            cmd += ";";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
            {
                cmd = "Select * from `arrangementforms` where `formName` = 'SMFS_Casket_Tube_Record';";
                dt = G1.get_db_data(cmd);
            }
            if (dt.Rows.Count > 0)
            {
                record = dt.Rows[0]["record"].ObjToString();
                str = G1.get_db_blob("arrangementforms", record, "image");
                location = dt.Rows[0]["location"].ObjToString();
            }
            if (str.IndexOf("rtf1") < 0)
                return;

            byte[] b = Encoding.UTF8.GetBytes(str);
            editFunTube = new ArrangementForms("SMFS_Casket_Tube_Record", location, record, workContract, b, true);
            editFunTube.RtfFinished += ArrangementForm_RtfDone;
            editFunTube.RtfModified += ArrangementForm_RtfModified; 
            if (!this.LookAndFeel.UseDefaultLookAndFeel)
            {
                editFunTube.LookAndFeel.UseDefaultLookAndFeel = false;
                editFunTube.LookAndFeel.SetSkinStyle(this.LookAndFeel.SkinName);
            }
            G1.LoadFormInPanel(editFunTube, this.panelBottomRightRTF);
            this.panelBottomRightRTF.Dock = DockStyle.Fill;
            editFunTube.BringToFront();
        }
        /***********************************************************************************************/
        private ArrangementForms editFunObit = null;
        private bool obitModified = false;
        private void InitializeObitPanel()
        {
            if (editFunObit != null)
                editFunObit.Close();
            editFunObit = null;
            obitModified = false;
            G1.ClearPanelControls(this.panelBottomRightRTF);

            string record = "";
            string str = "";
            string table = "agreements";
            string location = EditCust.activeFuneralHomeName;
            string cmd = "Select * from `agreements` where `formName` = 'Generic Obit' AND `contractNumber` = '" + workContract + "' ";
            if (String.IsNullOrWhiteSpace(location))
                cmd += " AND `location` = '" + location + "' ";
            cmd += ";";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
            {
                cmd = "Select * from `arrangementforms` where `formName` = 'Generic Obit';";
                dt = G1.get_db_data(cmd);
                table = "arrangementforms";
            }
            if (dt.Rows.Count > 0)
            {
                record = dt.Rows[0]["record"].ObjToString();
                str = G1.get_db_blob(table, record, "image");
                location = dt.Rows[0]["location"].ObjToString();
            }
            if (str.IndexOf("rtf1") < 0)
                return;

            byte[] b = Encoding.UTF8.GetBytes(str);
            editFunObit = new ArrangementForms("Generic Obit", location, record, workContract, b, true);
            editFunObit.RtfFinished += ArrangementForm_RtfDone;
            editFunObit.RtfModified += ArrangementForm_RtfModified; 
            if (!this.LookAndFeel.UseDefaultLookAndFeel)
            {
                editFunObit.LookAndFeel.UseDefaultLookAndFeel = false;
                editFunObit.LookAndFeel.SetSkinStyle(this.LookAndFeel.SkinName);
            }
            G1.LoadFormInPanel(editFunObit, this.panelBottomRightRTF);
            this.panelBottomRightRTF.Dock = DockStyle.Fill;
            editFunObit.BringToFront();
        }
        /****************************************************************************************/
        //private void btnTube_Click(object sender, EventArgs e)
        //{
        //    btnBiography.Hide();
        //    btnProcess.Hide();
        //    this.Cursor = Cursors.WaitCursor;
        //    ClearColor();
        //    panelBottomRightForms.Visible = false;
        //    dgv.Visible = false;
        //    //InitializeTubePanel();
        //    InitializeSomethingPanel(ref editFunTube, ref tubeModified, "SMFS_Casket_Tube_Record");
        //    panelBottomRightRTF.Dock = DockStyle.Fill;
        //    panelBottomRightRTF.Visible = true;
        //    //btnTube.BackColor = Color.Yellow;
        //    showTube = true;
        //    gridMain.RefreshData();
        //    this.Cursor = Cursors.Default;
        //    //editFunTube.BringToFront();
        //}
        /****************************************************************************************/
        private void btnProcess_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            EditFormData editForm = new EditFormData(workContract, "ALL", "ALL", "");
            editForm.Show();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private string LookupFile()
        {
            string foundFile = "";
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string file = G1.DecodeFilename(ofd.FileName);
                    foundFile = ofd.FileName;
                }
            }
            return foundFile;
        }
        /****************************************************************************************/
        private void btnBiography_Click(object sender, EventArgs e)
        {
            string filename = LookupFile();
            if (String.IsNullOrWhiteSpace(filename))
                return;
            string file = G1.DecodeFilename(filename);
            string cmd = "Select * from `agreements` where `formName` = 'Biography' AND `contractNumber` = '" + workContract + "';";
            DataTable dt = G1.get_db_data(cmd);
            try
            {
                if (filename.ToUpper().IndexOf(".DOC") > 0 || filename.ToUpper().IndexOf("DOCX") > 0)
                {
                    this.Hide();
                    this.Cursor = Cursors.WaitCursor;
                    ArrangementForms aForm = new ArrangementForms(filename, "", workContract, true);
                    aForm.RtfFinished += AForm_RtfDone;
                    aForm.Show();
                    this.Cursor = Cursors.Default;
                    temporaryFileName = "Biography";
                }
                else if (filename.ToUpper().IndexOf(".RTF") > 0)
                {
                    this.Hide();
                    this.Cursor = Cursors.WaitCursor;
                    ArrangementForms aForm = new ArrangementForms(filename, "", workContract, true);
                    aForm.RtfFinished += AForm_RtfDone;
                    aForm.Show();
                    this.Cursor = Cursors.Default;
                    temporaryFileName = "Biography";
                }
            }
            catch (Exception ex)
            {
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

            if (String.IsNullOrWhiteSpace(record))
            {
                record = G1.create_record("agreements", "formName", "-1");
                if (G1.BadRecord("agreements", record))
                    return;
                string temp = temporaryFileName;
                if (String.IsNullOrWhiteSpace(temp))
                    temp = "Unknown";
                G1.update_db_table("agreements", "record", record, new string[] { "contractNumber", contractNumber, "formName", temp });
            }

            G1.update_blob("agreements", "record", record, "image", b);
            temporaryFileName = "";
            LoadData();
            this.Show();
            this.Refresh();
        }
        /****************************************************************************************/
        private void gridMain_MouseDown(object sender, MouseEventArgs e)
        {
            this.BringToFront();
            this.Refresh();
        }

        private void dgv_MouseDown(object sender, MouseEventArgs e)
        {

        }
        /****************************************************************************************/
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string formName = dr["formName"].ObjToString();
            if (String.IsNullOrWhiteSpace(formName))
                return;
            string location = dr["location"].ObjToString();
            string record = dr["record"].ObjToString();
            string str = "";
            string cmd = "Select * from `arrangementforms` where `formName` = '" + formName + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                record = dt.Rows[0]["record"].ObjToString();
                str = G1.get_db_blob("arrangementforms", record, "image");
            }
            else
                return;
            string type = "PDF";
            if (str.ToUpper().IndexOf("RTF1") >= 0)
                type = "RTF";
            if (type == "RTF")
            {
                byte[] b = Encoding.ASCII.GetBytes(str);

                this.Cursor = Cursors.WaitCursor;
                ArrangementForms aForm = new ArrangementForms(formName, location, record, "", b);
                aForm.RtfFinished += XForm_RtfDone;
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
                            byte[] results = Arrangements.ReplaceFields(fileData);
                            this.Cursor = Cursors.WaitCursor;
                            ViewPDF viewForm = new ViewPDF(formName, record, "", results);
                            viewForm.PdfDone += ViewForm_PdfDone;
                            viewForm.Show();
                            this.Cursor = Cursors.Default;
                            //btnDeathCert.BackColor = Color.Transparent;
                            //btnDeathCert.Refresh();
                            btnForms_Click(null, null);
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
        private void XForm_RtfDone(string workFormName, string record, string contractNumber, string rtfText, bool dontASk, bool force )
        {
            string file = G1.DecodeFilename(workFormName);
            file = file.Replace(".docx", "");
            file = file.Replace(".doc", "");
            file = file.Replace(".rtf", "");
            byte[] b = Encoding.UTF8.GetBytes(rtfText);
            string location = "";
            if (String.IsNullOrWhiteSpace(record))
            {
                record = G1.create_record("arrangementforms", "type", "-1");
                if (G1.BadRecord("arrangementforms", record))
                    return;
                G1.update_db_table("arrangementforms", "record", record, new string[] { "type", "", "formName", file, "location", location });
            }

            G1.update_blob("arrangementforms", "record", record, "image", b);
            //LoadData();
            //this.Show();
        }
        /****************************************************************************************/
//        private void btnObit_Click(object sender, EventArgs e)
//        {
//            btnBiography.Hide();
//            btnProcess.Hide();
//            this.Cursor = Cursors.WaitCursor;
//            ClearColor();
//            panelBottomRightForms.Visible = false;
//            dgv.Visible = false;
//            //InitializeObitPanel();
//            //InitializeSomethingPanel(ref editFunObit, ref obitModified, "Generic Obit");
////            InitializeSomethingPanel(ref editFunObit, ref obitModified, "Hathorn Obit Template");
//            InitializeSomethingPanel(ref editFunObit, ref obitModified, "SMFS Obit");
//            panelBottomRightRTF.Dock = DockStyle.Fill;
//            panelBottomRightRTF.Visible = true;
//            //btnObit.BackColor = Color.Yellow;
//            showObit = true;
//            gridMain.RefreshData();
//            this.Cursor = Cursors.Default;
//        }
        /****************************************************************************************/
        private void pictureBox4_Click(object sender, EventArgs e)
        {
            G1.ShowHideFindPanel(gridMain);
        }
        /****************************************************************************************/
        //private void btnDirectorsReport_Click(object sender, EventArgs e)
        //{
        //    btnBiography.Hide();
        //    btnProcess.Hide();
        //    this.Cursor = Cursors.WaitCursor;
        //    ClearColor();
        //    panelBottomRightForms.Visible = false;
        //    dgv.Visible = false;
        //    //InitializeMarquePanel();
        //    InitializeSomethingPanel(ref editFunDirectorsReport, ref directorsReportModified, "Directors Report");
        //    panelBottomRightRTF.Dock = DockStyle.Fill;
        //    panelBottomRightRTF.Visible = true;
        //    //btnDirectorsReport.BackColor = Color.Yellow;
        //    showDirectorsReport = true;
        //    gridMain.RefreshData();
        //    this.Cursor = Cursors.Default;
        //}
        /***********************************************************************************************/
        private ArrangementForms editMsDeathCertProgram = null;
        private bool ms_DC_Modified = false;
        /****************************************************************************************/
        //private void btnDeathCert_Click(object sender, EventArgs e)
        //{
        //    btnBiography.Hide();
        //    btnProcess.Hide();
        //    this.Cursor = Cursors.WaitCursor;
        //    ClearColor();
        //    panelBottomRightForms.Visible = false;
        //    dgv.Visible = false;
        //    //InitializeProgramPanel();
        //    InitializeSomethingPanel(ref editMsDeathCertProgram, ref ms_DC_Modified, "MS_Death_Certificate");
        //    panelBottomRightRTF.Dock = DockStyle.Fill;
        //    panelBottomRightRTF.Visible = true;
        //    show_MS_DeathCert = true;
        //    gridMain.RefreshData();
        //    this.Cursor = Cursors.Default;
        //}
        /***************************************************************************************/
        public void FireEventFunFormsBringToFront()
        {
            if (editFunMarque != null)
                editFunMarque.BringToFront();
            if (editMsDeathCertProgram != null)
                editMsDeathCertProgram.BringToFront();
            if (editFunDirectorsReport != null)
                editFunDirectorsReport.BringToFront();
            if (editFunObit != null)
                editFunObit.BringToFront();
            if (editFunProgram != null)
                editFunProgram.BringToFront();
            if (editFunTube != null)
                editFunTube.BringToFront();
            if (editFunOther != null)
                editFunOther.BringToFront();
            if (viewForm != null)
                viewForm.BringToFront();
        }
        /***************************************************************************************/
        public bool FireEventFunServicesOkayToClose()
        {
            if (funModified)
            {
                if (editFunMarque != null)
                    editFunMarque.BringToFront();
                if (editMsDeathCertProgram != null)
                    editMsDeathCertProgram.BringToFront();
                if (editFunDirectorsReport != null)
                    editFunDirectorsReport.BringToFront();
                if (editFunObit != null)
                    editFunObit.BringToFront();
                if (editFunProgram != null)
                    editFunProgram.BringToFront();
                if (editFunTube != null)
                    editFunTube.BringToFront();
                if (editFunOther != null)
                    editFunOther.BringToFront();
                if (viewForm != null)
                    viewForm.BringToFront();
                return false;
            }
            if (editFunMarque != null)
                editFunMarque.Close();
            if (editMsDeathCertProgram != null)
                editMsDeathCertProgram.Close();
            if (editFunDirectorsReport != null)
                editFunDirectorsReport.Close();
            if (editFunObit != null)
                editFunObit.Close();
            if (editFunProgram != null)
                editFunProgram.Close();
            if (editFunTube != null)
                editFunTube.Close();
            if (editFunOther != null)
                editFunOther.Close();

            if (viewForm != null)
                viewForm = null;

            editFunMarque = null;
            editMsDeathCertProgram = null;
            editFunDirectorsReport = null;
            editFunObit = null;
            editFunProgram = null;
            editFunTube = null;
            editFunOther = null;
            viewForm = null;
            return true;
        }
        /****************************************************************************************/
        private void FunForms_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (editFunMarque != null)
                editFunMarque.Close();
            if (editMsDeathCertProgram != null)
                editMsDeathCertProgram.Close();
            if (editFunDirectorsReport != null)
                editFunDirectorsReport.Close();
            if (editFunObit != null)
                editFunObit.Close();
            if (editFunProgram != null)
                editFunProgram.Close();
            if (editFunTube != null)
                editFunTube.Close();
            if (editFunOther != null)
                editFunOther.Close();

            if (viewForm != null)
                viewForm.Close();

            editFunMarque = null;
            editMsDeathCertProgram = null;
            editFunDirectorsReport = null;
            editFunObit = null;
            editFunProgram = null;
            editFunTube = null;
            editFunOther = null;
            viewForm = null;
        }
        /****************************************************************************************/
        private void gridMain2_Click(object sender, EventArgs e)
        {
            try
            {
                DataRow dr = gridMain2.GetFocusedDataRow();
                if (dr == null)
                    return;

                string formButton = dr["buttons"].ObjToString();
                string formName = "";
                string cmd = "Select * from `form_buttons`;";
                DataTable dx = G1.get_db_data(cmd);

                DataRow[] dRows = dx.Select("formButton='" + formButton + "'");
                if (dRows.Length <= 0)
                    return;

                formName = dRows[0]["formName"].ObjToString();
                if (String.IsNullOrWhiteSpace(formName))
                    return;
                btnOther(formName);
            }
            catch ( Exception ex )
            {
            }
        }
        /****************************************************************************************/
        private void gridMain2_CalcRowHeight(object sender, RowHeightEventArgs e)
        {
            if (e.RowHandle >= 0)
                e.RowHeight = 28;
        }
        /****************************************************************************************/
        private void gridMain2_RowCellStyle(object sender, RowCellStyleEventArgs e)
        {
            if (e.RowHandle >= 0)
            {
                if (e.RowHandle == gridMain2.FocusedRowHandle)
                    e.Appearance.BackColor = Color.Yellow;
                else
                    e.Appearance.BackColor = Color.Transparent;
            }
        }
        /****************************************************************************************/
    }
}