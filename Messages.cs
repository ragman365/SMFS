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
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class Messages : DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        private string fromName = "";
        private string fromNameRecord = "";
        private string workMessage = "";
        private string workSubject = "";
        private string workFilename = "";
        private string attachFile = "";
        bool loading = true;
        /***********************************************************************************************/
        public Messages( string subject = "", string message = "", string filename = "" )
        {
            workMessage = message;
            workSubject = subject;
            workFilename = filename;
            InitializeComponent();
        }
        /***********************************************************************************************/
        private void Messages_Load(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            btnSend.Hide();
            btnDiscard.Hide();
            rtb.Dock = DockStyle.Fill;
            string cmd = "Select * from `users` order by lastName, firstName";
            DataTable dx = G1.get_db_data(cmd);
            dx.Columns.Add("name");
            string mainUser = "";
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                dx.Rows[i]["name"] = dx.Rows[i]["firstName"] + " " + dx.Rows[i]["lastName"].ObjToString();
                if (dx.Rows[i]["record"].ObjToString() == LoginForm.workUserRecord)
                    mainUser = dx.Rows[i]["name"].ObjToString();
            }
            chkComboLocNames.Properties.DataSource = dx;
            if (LoginForm.administrator)
            {
                cmbUsers.DataSource = dx;
            }
            else
            {
                cmbUsers.Items.Add(mainUser);
                cmbUsers.Text = mainUser;
            }
            string record = LoginForm.workUserRecord;

            DataTable dt = G1.get_db_data("Select * from `users` where `record` = '" + record + "';");
            if ( dt.Rows.Count > 0 )
            {
                string name = dt.Rows[0]["firstName"] + " " + dt.Rows[0]["lastName"].ObjToString();
                fromName = name;
                fromNameRecord = dt.Rows[0]["record"].ObjToString();
                cmbUsers.Text = name;
                LoadMyMessages();
            }
            EnableTop();
            loading = false;
            if ( !String.IsNullOrWhiteSpace ( workMessage))
            {
                tsbNewMessage_Click(null, null);
                rtb.Text = workMessage;
                txtSubject.Text = workSubject;
            }
        }
        /***********************************************************************************************/
        public static int GetMessageCount( string userRecord )
        {
            int rv = 0;
            if (String.IsNullOrWhiteSpace(userRecord))
                return rv;
            string cmd = "Select * from `messages` where `toRecord` = '" + userRecord + "' and `deleted` <> 'Y' and `type` <> 'tickle' and `read` <> 'Y' order by `senddate` desc;";
            DataTable dt = G1.get_db_data(cmd);
            rv = dt.Rows.Count;

            DateTime tdate = DateTime.Now;
            string tickleDate = tdate.Year.ToString("D4") + "-" + tdate.Month.ToString("D2") + "-" + tdate.Day.ToString("D2") + " ";
            tickleDate += tdate.Hour.ToString("D2") + ":" + tdate.Minute.ToString("D2") + ":00";
            cmd = "Select * from `messages` where `toRecord` = '" + userRecord + "' and `deleted` <> 'Y' and `type` = 'tickle' and `senddate` <= '" + tickleDate + "' and `read` <> 'Y' order by `senddate` desc;";
            dt = G1.get_db_data(cmd);
            rv += dt.Rows.Count;
            dt.Dispose();
            return rv;
        }
        /***********************************************************************************************/
        public static void SendLatePayment ( string contractNumber )
        {
            DataTable dt = G1.getPreferenceUsers("Long Payment", "Send 60 Day Deposit");
            string username = "";
            string message = "Contract (" + contractNumber + ")";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                username = dt.Rows[i]["userName"].ObjToString();

                Messages.SendAMessage(LoginForm.username, username, "TRUST PAYMENT MORE THAN 6 MONTHS OLD (" + contractNumber + ")", message);
            }
        }
        /***********************************************************************************************/
        public static void SendAMessage(string from, string to, string subject, string message)
        {
            string[] Lines = to.Split('|');
            string sendTo = "";
            string record = "";
            DateTime today = DateTime.Now;
            string sendDate = today.ToString("yyyy-MM-dd");
            string toRecord = "";
            string cmd = "";
            string userName = "";
            DataTable dt = null;

            if (String.IsNullOrWhiteSpace(from))
                from = LoginForm.username;

            cmd = "Select * from `users` where `userName` = '" + from + "';";
            dt = G1.get_db_data(cmd);
            if (dt.Rows.Count < 0)
                return;

            string fromRecord = dt.Rows[0]["record"].ObjToString();

            cmd = "Select * from `users`;";
            dt = G1.get_db_data(cmd);

            DataRow[] dRows = null;
            string lastName = "";
            string firstName = "";
            for (int i = 0; i < Lines.Length; i++)
            {
                userName = Lines[i].ToUpper().Trim();
                dRows = dt.Select("userName='" + userName + "'");
                if (dRows.Length > 0)
                {
                    record = G1.create_record("messages", "fromUser", "-1");
                    if (G1.BadRecord("messages", record))
                        continue;
                    toRecord = dRows[0]["record"].ObjToString();

                    lastName = dRows[0]["lastName"].ObjToString();
                    firstName = dRows[0]["firstName"].ObjToString();
                    sendTo = firstName + " " + lastName;

                    G1.update_db_table("messages", "record", record, new string[] { "fromUser", from, "toUser", sendTo, "subject", subject, "message", message, "senddate", sendDate, "fromRecord", fromRecord, "toRecord", toRecord });
                }
            }
        }
        /***********************************************************************************************/
        public static void SendTheMessage(string from, string to, string subject, string message)
        {
            string[] Lines = to.Split('|');
            string sendTo = "";
            string record = "";
            DateTime today = DateTime.Now;
            string sendDate = today.ToString("yyyy-MM-dd");
            string toRecord = "";
            string cmd = "";
            string userName = "";
            DataTable dt = null;

            if (String.IsNullOrWhiteSpace(from))
                from = LoginForm.username;

            cmd = "Select * from `users` where `userName` = '" + from + "';";
            dt = G1.get_db_data(cmd);
            if (dt.Rows.Count < 0)
                return;

            string fromRecord = dt.Rows[0]["record"].ObjToString();

            for (int i = 0; i < Lines.Length; i++)
            {
                userName = Lines[i].ToUpper().Trim();
                cmd = "Select * from `users` where `userName` = '" + userName + "';";
                dt = G1.get_db_data(cmd);
                if (dt.Rows.Count > 0)
                {
                    record = G1.create_record("messages", "fromUser", "-1");
                    if (G1.BadRecord("messages", record))
                        continue;
                    toRecord = dt.Rows[0]["record"].ObjToString();

                    G1.update_db_table("messages", "record", record, new string[] { "fromUser", from, "toUser", sendTo, "subject", subject, "message", message, "senddate", sendDate, "fromRecord", fromRecord, "toRecord", toRecord });
                }
            }
        }
        /***********************************************************************************************/
        private void LoadMyMessages()
        {
            string where = cmbBoxes.Text.Trim().ToUpper();
            string cmd = "Select * from `messages` where `toRecord` = '" + fromNameRecord + "' and `deleted` <> 'Y' and `type` <> 'tickle' order by `senddate` desc;";
            if ( where == "SENT MESSAGES")
                cmd = "Select * from `messages` where `fromRecord` = '" + fromNameRecord + "' and `deleted` <> 'Y' and `type` <> 'tickle' order by `senddate` desc;";
            if (where == "DELETED MESSAGES")
                cmd = "Select * from `messages` where `toRecord` = '" + fromNameRecord + "' and `deleted` = 'Y' order by `senddate` desc;";
            if (where == "TICKLES")
                cmd = "Select * from `messages` where `toRecord` = '" + fromNameRecord + "' and `deleted` <> 'Y' and `type` = 'tickle' order by `senddate` desc;";

            DataTable dt = G1.get_db_data(cmd);
            if ( where == "MESSAGES RECEIVED")
            {
                DateTime tdate = DateTime.Now;
                string tickleDate = tdate.Year.ToString("D4") + "-" + tdate.Month.ToString("D2") + "-" + tdate.Day.ToString("D2") + " ";
                tickleDate += tdate.Hour.ToString("D2") + ":" + tdate.Minute.ToString("D2") + ":00";
                cmd = "Select * from `messages` where `toRecord` = '" + fromNameRecord + "' and `deleted` <> 'Y' and `type` = 'tickle' and `senddate` <= '" + tickleDate + "' order by `senddate` desc;";
                DataTable dx = G1.get_db_data(cmd);
                for (int i = 0; i < dx.Rows.Count; i++)
                    dt.ImportRow(dx.Rows[i]);
            }
            dt.Columns.Add("num");
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            ShowMessage(0);
        }
        /***********************************************************************************************/
        private void ShowMessage( int row )
        {
            if (row < 0)
                return;
            picPaperClip.Hide();
            DataTable dt = (DataTable)dgv.DataSource;

            ClearAll();
            if (dt.Rows.Count <= 0)
                return;

            try
            {
                loading = true;
                string read = dt.Rows[row]["read"].ObjToString();
                if (String.IsNullOrWhiteSpace(read))
                {
                    string toRecord = dt.Rows[row]["toRecord"].ObjToString();
                    if (toRecord == LoginForm.workUserRecord)
                    {
                        string record = dt.Rows[row]["record"].ObjToString();
                        G1.update_db_table("messages", "record", record, new string[] { "read", "Y" });
                        dt.Rows[row]["read"] = "Y";
                    }
                }
                rtb.Text = dt.Rows[row]["message"].ObjToString();
                lblFrom.Text = dt.Rows[row]["fromUser"].ObjToString();
                lblFrom.Refresh();
                chkComboLocNames.Text = dt.Rows[row]["toUser"].ObjToString();
                chkComboLocNames.EditValue = dt.Rows[row]["toUser"].ObjToString();
                txtSubject.Text = dt.Rows[row]["subject"].ObjToString();
                attachFile = dt.Rows[row]["filename"].ObjToString();
                if (!String.IsNullOrWhiteSpace(attachFile))
                    picPaperClip.Show();
                txtSubject.Refresh();
            }
            catch ( Exception ex)
            {
                rtb.Text = "";
                lblFrom.Text = "";
                lblFrom.Refresh();
                chkComboLocNames.Text = "";
                chkComboLocNames.EditValue = "";
                txtSubject.Text = "";
                attachFile = "";
                if (!String.IsNullOrWhiteSpace(attachFile))
                    picPaperClip.Show();
                txtSubject.Refresh();
            }
            loading = false;
        }
        /***********************************************************************************************/
        private void ClearAll ()
        {
            txtSubject.Text = "";
            lblFrom.Text = fromName;
            chkComboLocNames.Text = "";
            chkComboLocNames.EditValue = "";
            rtb.Clear();
            picPaperClip.Hide();
            attachFile = "";
        }
        /***********************************************************************************************/
        private void chkComboLocNames_EditValueChanged(object sender, EventArgs e)
        {
            //if (loading)
            //    return;
            //txtSubject.Text = "";
            //lblFrom.Text = fromName;
            //btnSend.Enabled = true;
        }
        /***********************************************************************************************/
        private void btnSend_Click(object sender, EventArgs e)
        {
            int rowHandle = gridMain.FocusedRowHandle;
            string message = rtb.Text;
            string from = lblFrom.Text;
            string subject = txtSubject.Text;
            string to = chkComboLocNames.Text;
            string[] Lines = to.Split('|');
            string sendTo = "";
            string record = "";
            DateTime today = DateTime.Now;
            string sendDate = today.ToString("yyyy-MM-dd");
            string toRecord = "";
            string fromRecord = "";
            DataTable dx = (DataTable)chkComboLocNames.Properties.DataSource;
            DataRow[] dRow = dx.Select("name='" + from + "'");
            if ( dRow.Length <= 0 )
            {
                MessageBox.Show("***ERROR***");
                return;
            }
            fromRecord = dRow[0]["record"].ObjToString();

            for ( int i=0; i<Lines.Length; i++)
            {
                sendTo = Lines[i].ToUpper();
                dRow = dx.Select("name='" + sendTo + "'");
                if (dRow.Length > 0)
                {
                    toRecord = dRow[0]["record"].ObjToString();
                    record = G1.create_record("messages", "fromUser", "-1");
                    if (G1.BadRecord("messages", record))
                        continue;
                    G1.update_db_table("messages", "record", record, new string[] { "fromUser", from, "toUser", sendTo, "subject", subject, "message", message, "senddate", sendDate, "fromRecord", fromRecord, "toRecord", toRecord });
                }
            }
            EnableTop();
            rtb.Clear();
            ShowMessage(rowHandle);
        }
        /***********************************************************************************************/
        private void EnableTop()
        {
            tsbDelete.Enabled = true;
            tsbNewMessage.Enabled = true;
            tsbForward.Enabled = true;
            tsbReply.Enabled = true;
            tsbPrint.Enabled = true;
            tsbTickle.Enabled = true;
            dgv.Enabled = true;
            btnSend.Enabled = false;
            btnDiscard.Enabled = false;
            btnSend.Hide();
            btnDiscard.Hide();
        }
        /***********************************************************************************************/
        private void DisableTop ()
        {
            tsbDelete.Enabled = false;
            tsbNewMessage.Enabled = false;
            tsbForward.Enabled = false;
            tsbReply.Enabled = false;
            tsbPrint.Enabled = true;
            tsbTickle.Enabled = true;
            dgv.Enabled = false;
            btnSend.Enabled = true;
            btnDiscard.Enabled = true;
            btnSend.Show();
            btnDiscard.Show();
        }
        /***********************************************************************************************/
        private void tsbNewMessage_Click(object sender, EventArgs e)
        {
            txtSubject.Text = "";
            lblFrom.Text = fromName;
            rtb.Clear();
            DisableTop();
        }
        /***********************************************************************************************/
        private void btnDiscard_Click(object sender, EventArgs e)
        {
            int rowHandle = gridMain.FocusedRowHandle;
            txtSubject.Text = "";
            chkComboLocNames.Text = "";
            chkComboLocNames.EditValue = "";
            rtb.Clear();
            EnableTop();
            ShowMessage(rowHandle);
        }
        /***********************************************************************************************/
        private void cmbBoxes_SelectedIndexChanged(object sender, EventArgs e)
        {
            string record = LoginForm.workUserRecord;
            LoadMyMessages();
        }
        /***********************************************************************************************/
        private void cmbUsers_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            string user = cmbUsers.Text;
            string name = "";
            DataTable dx = (DataTable)cmbUsers.DataSource;
            for ( int i=0; i<dx.Rows.Count; i++)
            {
                name = dx.Rows[i]["name"].ObjToString();
                if ( name == user )
                {
                    string record = dx.Rows[i]["record"].ObjToString();
                    fromName = name;
                    fromNameRecord = record;
                    LoadMyMessages();
                }
            }
        }
        /***********************************************************************************************/
        private void rtb_DoubleClick(object sender, EventArgs e)
        {
            string contractNumber = "";
            int pos = rtb.SelectionStart;
            string text = rtb.Text;
            string c = "";
            for (int i = pos; i >= 0; i--)
            {
                c = text.Substring(i, 1);
                if ( c == "(")
                {
                    string str = text.Substring(i);
                    int idx = str.IndexOf(')');
                    string cc = str.Substring(0, idx);
                    str = cc.Replace("(", "");
                    str = str.Replace(")", "");
                    contractNumber = str;
                }
            }
            if ( !String.IsNullOrWhiteSpace ( contractNumber))
            {
                string cmd = "Select * from `contracts` where `contractNumber` = '" + contractNumber + "';";
                DataTable dt = G1.get_db_data(cmd);
                if (dt.Rows.Count > 0)
                {
                    this.Cursor = Cursors.WaitCursor;
                    CustomerDetails clientForm = new CustomerDetails(contractNumber);
                    clientForm.Show();
                    this.Cursor = Cursors.Default;
                }
                else
                {
                    MessageBox.Show("***ERROR*** Could not locate contract number " + contractNumber + "!!");
                }
            }
        }
        /***********************************************************************************************/
        private void tsbReply_Click(object sender, EventArgs e)
        {
            string fromUser = lblFrom.Text;
            string toUser = fromName;
            lblFrom.Text = fromName;
            chkComboLocNames.Text = fromUser;
            DisableTop();
            DateTime today = DateTime.Now;
            rtb.Text += "\nReply from fromName on " + today.ToString("MM/dd/yyyy") + ".\n";
            string subject = "Re: " + txtSubject.Text;
            txtSubject.Text = subject;
        }
        /***********************************************************************************************/
        private void gridMain_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
        {
            if (e.Column.FieldName.ToUpper() == "SUBJECT")
            {
                if (e.RowHandle >= 0)
                {
                    DataTable dt = (DataTable)dgv.DataSource;
                    int row = gridMain.GetDataSourceRowIndex(e.RowHandle);
                    string read = dt.Rows[row]["read"].ObjToString();
                    if (read.ToUpper() != "Y")
                        e.Appearance.FontStyleDelta = FontStyle.Bold;
                }
            }
        }
        /***********************************************************************************************/
        private void gridMain_RowClick(object sender, DevExpress.XtraGrid.Views.Grid.RowClickEventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowHandle = gridMain.FocusedRowHandle;
            ShowMessage(rowHandle);
        }
        /***********************************************************************************************/
        private void gridMain_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowHandle = gridMain.FocusedRowHandle;
            ShowMessage(rowHandle);
        }
        /***********************************************************************************************/
        private void tsbDelete_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("***Warning*** Are you SURE you want to DELETE this MESSAGE?", "Delete Message Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
            {
                MessageBox.Show("***INFO*** Okay, Message not deleted!", "Delete Message Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowHandle = gridMain.FocusedRowHandle;
            string record = dr["record"].ObjToString();
            string where = cmbBoxes.Text.Trim().ToUpper();
            if ( where.ToUpper() == "DELETED MESSAGES")
            {
                if ( !LoginForm.administrator )
                    return;
                G1.delete_db_table("messages", "record", record);
            }
            else
                G1.update_db_table("messages", "record", record, new string[] { "deleted", "Y" });
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            DataTable dt = (DataTable)dgv.DataSource;
            dt.Rows.RemoveAt(row);
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            dgv.Refresh();
        }
        /***********************************************************************************************/
        private void tsbPrint_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            DateTime date = dr["senddate"].ObjToDateTime();

            int rowHandle = gridMain.FocusedRowHandle;
            rtb2.Dock = DockStyle.Fill;
            RichTextBox rtb3 = new RichTextBox();
            rtb3.AppendText("Date :" + date.ToString("MM/dd/yyyy") + "\n\n");
            rtb3.AppendText("From : " + lblFrom.Text + "\n\n");
            rtb3.AppendText("To   : " + chkComboLocNames.Text + "\n\n");
            rtb3.AppendText("Subject : " + txtSubject.Text + "\n");
            rtb3.AppendText("______________________________________________________________________________________________________\n");
            rtb3.AppendText("Message :\n\n");

            rtb3.AppendText(rtb.Text);

            rtb2.Document.RtfText = rtb3.Rtf;
            printPreviewToolStripMenuItem_Click(null, null);
        }
        /***********************************************************************************************/
        private void printPreviewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.components == null)
                this.components = new System.ComponentModel.Container();

            DevExpress.XtraPrinting.PrintingSystem printingSystem1 = new PrintingSystem(this.components);
            DevExpress.XtraPrinting.PrintableComponentLink printableComponentLink1 = new DevExpress.XtraPrinting.PrintableComponentLink(this.components);

            printingSystem1.Links.AddRange(new object[] {
            printableComponentLink1});


            printableComponentLink1.Component = rtb2;
            printableComponentLink1.PrintingSystemBase = printingSystem1;
            printableComponentLink1.CreateDetailHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateDetailHeaderArea);
            printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);
            printableComponentLink1.CreateMarginalFooterArea += PrintableComponentLink1_CreateMarginalFooterArea;
            printableComponentLink1.BeforeCreateAreas += new System.EventHandler(this.printableComponentLink1_BeforeCreateAreas);
            printableComponentLink1.AfterCreateAreas += new System.EventHandler(this.printableComponentLink1_AfterCreateAreas);
            printableComponentLink1.Landscape = true;

            string leftColumn = "Pages: ";
            string middleColumn = "User: ";
            string rightColumn = "Date: ";

            // Create a PageHeaderFooter object and initializing it with
            // the link's PageHeaderFooter.
            PageHeaderFooter phf = printableComponentLink1.PageHeaderFooter as PageHeaderFooter;

            // Clear the PageHeaderFooter's contents.
            phf.Header.Content.Clear();

            // Add custom information to the link's header.
            phf.Header.Content.AddRange(new string[] { leftColumn, middleColumn, rightColumn });
            phf.Header.LineAlignment = BrickAlignment.Far;



            Printer.setupPrinterMargins(0, 0, 0, 150);

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
        private void PrintableComponentLink1_CreateMarginalFooterArea(object sender, CreateAreaEventArgs e)
        {
            //Printer.setupPrinterQuads(e, 4, 3, true);

            //// Printer.DrawQuadGrid();

            //Font font = new Font("Times New Roman", 10);
            //Printer.DrawQuad(1, 2, 1, 1, "", Color.Black, BorderSide.Bottom, font, HorizontalAlignment.Left);
            //Printer.DrawQuad(1, 3, 1, 1, printName, Color.Black, BorderSide.None, font, HorizontalAlignment.Left);
            //Printer.DrawQuad(1, 4, 1, 1, printSSN, Color.Black, BorderSide.None, font, HorizontalAlignment.Left);

            //font = new Font("Ariel", 8);
            //Printer.setupPrinterQuads(e, 20, 3, true);
            //Printer.DrawGridPage(18, 2, 2, 2, Color.Black, BorderSide.None, font);
        }
        /***********************************************************************************************/
        private int pageMarginLeft = 0;
        private int pageMarginRight = 0;
        private int pageMarginTop = 0;
        private int pageMarginBottom = 0;
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
        }
        /***********************************************************************************************/
        private void tsbTickle_Click(object sender, EventArgs e)
        {
            string fromUser = lblFrom.Text;
            string subject = txtSubject.Text;
            string message = rtb.Text;
            timer1.Enabled = false;
            Tickle tickleForm = new Tickle(fromUser, subject, message);
            tickleForm.ShowDialog();
            timer1.Enabled = false;
        }
        /***********************************************************************************************/
        private void timer1_Tick(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowHandle = gridMain.FocusedRowHandle;
            string record = dr["record"].ObjToString();
            LoadMyMessages();
            DataTable dt = (DataTable)dgv.DataSource;
            string rec = "";
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                rec = dt.Rows[i]["record"].ObjToString();
                if ( rec == record )
                {
                    rowHandle = i;
                    break;
                }
            }
            ShowMessage(rowHandle);
            gridMain.FocusedRowHandle = rowHandle;
        }
        /***********************************************************************************************/
        void TickleFormFormClosed(object sender, FormClosedEventArgs e)
        {
            if (((Tickle)sender).TickleSent)
            {
                int rowHandle = gridMain.FocusedRowHandle;
                LoadMyMessages();
                ShowMessage(rowHandle);
                gridMain.FocusedRowHandle = rowHandle;
            }
        }
        /***********************************************************************************************/
        private void tsbRefresh_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt == null)
                return;
            if (dt.Rows.Count < 0)
                return;
            DataRow dr = gridMain.GetFocusedDataRow();
            if (dr == null)
                return;
            int rowHandle = gridMain.FocusedRowHandle;
            string record = dr["record"].ObjToString();
            LoadMyMessages();
            dt = (DataTable)dgv.DataSource;
            string rec = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                rec = dt.Rows[i]["record"].ObjToString();
                if (rec == record)
                {
                    rowHandle = i;
                    break;
                }
            }
            ShowMessage(rowHandle);
            gridMain.FocusedRowHandle = rowHandle;
        }
        /***********************************************************************************************/
        private void picPaperClip_Click(object sender, EventArgs e)
        {
            string filename = attachFile;
            try
            {
                ViewPDF viewForm = new ViewPDF(txtSubject.Text, filename);
                viewForm.Show();
            }
            catch (Exception ex)
            {
            }
        }
        /***********************************************************************************************/
    }
}