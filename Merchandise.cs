using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using GeneralLib;
using System.IO;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class Merchandise : DevExpress.XtraEditors.XtraForm
    {
        private bool modified = false;
        private bool modifiedPicture = false;
        private bool loading = true;
        private string workRecord = "";
        private string workInventoryRecord = "";
        private Bitmap emptyImage;
        private bool workAdding = false;
        private bool workAddingInventory = false;
        private string workWhat = "";
        private bool showing = false;
        private DataTable _LocationList;
        private DataTable _OwnerList;
        private DataTable workDt = null;
        private int mainRow = -1;
        /***********************************************************************************************/
        private DataTable originalLocationDt = null;
        protected bool validData;

        string path;
        protected Image image;
        protected Thread getImageThread;
        /***********************************************************************************************/
        public Merchandise( string record, string what, DataTable dt = null )
        {
            workRecord = record;
            workAdding = false;
            workAddingInventory = false;
            showing = true;
            workWhat = what;
            workDt = dt;
            InitializeComponent();
            if (workDt == null)
            {
                btnLeft.Hide();
                btnRight.Hide();
            }
            else
            {
                mainRow = LocateMerchandiseRow(workRecord);
            }
        }
        /***********************************************************************************************/
        public Merchandise( string record, bool adding = false )
        {
            workRecord = record;
            workAdding = adding;
            InitializeComponent();
        }
        /***********************************************************************************************/
        public Merchandise(string record, string inventoryRecord, bool adding, bool editing = false )
        {
            workRecord = record;
            workInventoryRecord = inventoryRecord;
            workAddingInventory = adding;
            if (editing)
                workWhat = "SPECIAL";
            InitializeComponent();
        }
        /***********************************************************************************************/
        private void getLocations()
        {
            string cmd = "SELECT `LocationCode` FROM `inventory` GROUP BY `LocationCode` ASC;";
            _LocationList = G1.get_db_data(cmd);

            string location = "";

            for ( int i=_LocationList.Rows.Count-1; i>=0; i--)
            {
                location = _LocationList.Rows[i]["LocationCode"].ObjToString();
                if (String.IsNullOrWhiteSpace(location))
                    _LocationList.Rows.RemoveAt(i);
            }

            cmbLocation.DataSource = _LocationList;
        }
        /***********************************************************************************************/
        private void getOwnership()
        {
            string cmd = "SELECT `Ownership` FROM `inventory` GROUP BY `Ownership` ASC;";
            _OwnerList = G1.get_db_data(cmd);
            cmbOwner.DataSource = _OwnerList;
        }
        /***********************************************************************************************/
        private int LocateMerchandiseRow ( string record )
        {
            int row = -1;
            for ( int i=0; i<workDt.Rows.Count; i++)
            {
                if ( workDt.Rows[i]["record"].ObjToString() == record )
                {
                    row = i;
                    break;
                }
            }
            return row;
        }
        /***********************************************************************************************/
        private void Merchandise_Load(object sender, EventArgs e)
        {
            this.AllowDrop = true;
            btnSave.Hide();
            if (showing)
            {
                rtbDesc.Enabled = false;
                rtbPrice.Enabled = false;
                txtCode.Enabled = false;
                txtType.Enabled = false;
                txtGuage.Enabled = false;
                cmbLocation.Enabled = false;
                receivedDate.Enabled = false;
                txtSerialNumber.Enabled = false;
                usedDate.Enabled = false;
                deceasedDate.Enabled = false;
                cmbOwner.Enabled = false;
                btnAttach.Enabled = false;
                btnDetach.Enabled = false;
            }
            if (workAddingInventory || !String.IsNullOrWhiteSpace(workInventoryRecord))
            {
                rtbDesc.Enabled = false;
                rtbPrice.Enabled = false;
                txtCode.Enabled = false;
                txtType.Enabled = false;
                txtGuage.Enabled = false;
                btnAttach.Enabled = false;
                btnDetach.Enabled = false;
                getLocations();
                getOwnership();
            }
            else if (workAdding)
            {
                panelAssign.Hide();
                panelMiddle.Hide();
                btnSave.Show();
                loading = false;
                return;
            }
            if (workWhat.ToUpper() == "EDIT" || workWhat.ToUpper() == "SPECIAL")
            {
                rtbDesc.Enabled = true;
                rtbPrice.Enabled = true;
                txtCode.Enabled = true;
                txtType.Enabled = true;
                txtGuage.Enabled = true;
                cmbLocation.Enabled = false;
                receivedDate.Enabled = false;
                txtSerialNumber.Enabled = false;
                usedDate.Enabled = false;
                deceasedDate.Enabled = false;
                cmbOwner.Enabled = false;
                btnAttach.Enabled = true;
                btnDetach.Enabled = true;
            }
            emptyImage = new Bitmap(1, 1);
            this.pictureBox1.Image = emptyImage;
            string cmd = "Select * from `inventorylist` where `record` = '" + workRecord + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
            {
                MessageBox.Show("***ERROR*** Locating Inventory Record " + workRecord + "!");
                this.Close();
            }
            Byte[] bytes = dt.Rows[0]["picture"].ObjToBytes();
            Image myImage = emptyImage;
            if (bytes != null)
            {
                myImage = G1.byteArrayToImage(bytes);
                this.pictureBox1.Image = (Bitmap)myImage;
            }
            double money = dt.Rows[0]["casketcost"].ObjToString().ObjToDouble();
            money = G1.RoundValue(money);
            long pennys = (long)(money * 100D);
            string cPennys = G1.commatize(pennys);
            this.rtbDesc.Text = dt.Rows[0]["casketdesc"].ObjToString();
            this.rtbPrice.Text = "$" + cPennys;
            this.txtCode.Text = dt.Rows[0]["casketcode"].ObjToString();
            this.txtType.Text = dt.Rows[0]["caskettype"].ObjToString();
            this.txtGuage.Text = dt.Rows[0]["casketguage"].ObjToString();
            this.txtBatesville.Text = dt.Rows[0]["itemnumber"].ObjToString();

            if (workWhat.ToUpper() == "LOOK" || workWhat == "EDIT" || workWhat == "SPECIAL")
            {
                panelMiddle.Hide();
                panelAssign.Hide();
                if (workWhat == "EDIT" || workWhat == "SPECIAL" )
                    showing = false;
            }
            else
                LoadWorkInventory();
            this.BringToFront();
            loading = false;
        }
        /***********************************************************************************************/
        private void LoadWorkInventory()
        {
            //if (!workAddingInventory)
            //    return;

            txtCode.ReadOnly = true;
            rtbDesc.ReadOnly = true;
            rtbPrice.ReadOnly = true;
            txtType.ReadOnly = true;
            txtGuage.ReadOnly = true;

            btnSave.Hide();

            getLocations();
            getOwnership();

            if ( String.IsNullOrWhiteSpace( workInventoryRecord))
            {
                panelMiddle.Show();
                panelAssign.Hide();
                return;
            }

            string cmd = "Select * from `inventory` where `record` = '" + workInventoryRecord + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return;
            string location = dt.Rows[0]["LocationCode"].ObjToString();
            string serialNumber = dt.Rows[0]["SerialNumber"].ObjToString();
            string ownership = dt.Rows[0]["Ownership"].ObjToString();
            string dateReceived = dt.Rows[0]["DateReceived"].ObjToString();

            originalLocationDt = dt.Copy();

            cmbLocation.Text = location;
            txtSerialNumber.Text = serialNumber;
            cmbOwner.Text = ownership;
            receivedDate.EditValue = dateReceived;

            panelMiddle.Show();
            panelAssign.Show();

            string serviceID = dt.Rows[0]["ServiceID"].ObjToString();
            //if (String.IsNullOrWhiteSpace(serviceID))
            //{
            //    panelAssign.Hide();
            //    return;
            //}

            txtServiceID.Text = serviceID;
            string DateUsed = dt.Rows[0]["DateUsed"].ObjToString();
            usedDate.EditValue = DateUsed;
            string DateDeceased = dt.Rows[0]["deceasedDate"].ObjToString();
            deceasedDate.EditValue = DateDeceased;
            if (DateDeceased.IndexOf("0000") >= 0)
                btnSelectCustomer.Hide();
            panelAssign.Show();
        }
        /***********************************************************************************************/
        private void WriteAuditInventory ( string newRecord , string[] fields)
        {
            if (originalLocationDt == null)
                return;

            DataTable newDt = G1.get_db_data("Select * from `inventory` where `record` = '" + newRecord + "';");

            for (int i = 0; i < fields.Length; i++)
            {
                string field = fields[i].ToString();
                Audit.CheckAuditField("Merchandise", field, originalLocationDt.Rows[0], newDt.Rows[0]);
            }
        }
        /***********************************************************************************************/
        private void SaveWorkRecord()
        {
            string serialNumber = txtSerialNumber.Text;
            string location = cmbLocation.Text;
            string owner = cmbOwner.Text;
            string dateReceived = receivedDate.Text;
            string desc = rtbDesc.Text;

            dateReceived = G1.date_to_sql(dateReceived);

            string record = workInventoryRecord;
            if (String.IsNullOrWhiteSpace(record))
            {
                CheckForBadRecord();
                record = G1.create_record("inventory", "LocationCode", "-1");
//                record = G1.create_record("inventory", "Added", "2");
            }
            if (String.IsNullOrWhiteSpace(record))
                record = "-1";
            if ( record == "-1" || record == "0")
            {
                MessageBox.Show("***ERROR*** Adding new Inventory Record!!");
                return;
            }

            if (workWhat.ToUpper() == "SPECIAL")
            {
                G1.update_db_table("inventory", "record", record, new string[] { "CasketDescription", desc });
            }
            else
            {
                G1.update_db_table("inventory", "record", record, new string[] { "CasketDescription", desc, "DateReceived", dateReceived, "LocationCode", location, "SerialNumber", serialNumber, "Ownership", owner, "ServiceID", "" });

                WriteAuditInventory(record, new string[] { "LocationCode", "Ownership" });

                if (panelAssign.Visible)
                {
                    string dateUsed = usedDate.Text;
                    dateUsed = G1.date_to_sql(dateUsed);
                    string dateDeceased = deceasedDate.Text;
                    dateDeceased = G1.date_to_sql(dateDeceased);
                    string serviceID = txtServiceID.Text;

                    G1.update_db_table("inventory", "record", record, new string[] { "DateUsed", dateUsed, "ServiceID", serviceID, "deceasedDate", dateDeceased });
                    WriteAuditInventory(record, new string[] { "DateUsed", "ServiceID" });
                }
            }
            if (ModuleDone != null)
            {
                if (workAddingInventory)
                    ModuleDone.Invoke("RELOAD " + record);
                else
                {
                    if ( workWhat.ToUpper() == "SPECIAL")
                        ModuleDone.Invoke("RELOAD " + record);
                    else
                        ModuleDone.Invoke(record);
                }
            }
        }
        /***********************************************************************************************/
        private void CheckForBadRecord ()
        {
            string cmd = "Select * from `inventory` where `LocationCode` = '-1';";
            DataTable dt = G1.get_db_data(cmd);
            if ( dt.Rows.Count > 0 )
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string record = dt.Rows[i]["record"].ObjToString();
                    G1.delete_db_table("inventory", "record", record);
                }
            }
        }
        /***********************************************************************************************/
        private void btnSave_Click(object sender, EventArgs e)
        {
            if (showing)
                return;
            if (!modified)
                return;
            if (workAddingInventory || !String.IsNullOrWhiteSpace(workInventoryRecord))
            {
                SaveWorkRecord();
                this.Close();
                return;
            }
            if ( rtbDesc.Text.Length == 0)
            {
                MessageBox.Show("***ERROR*** Description is empty! You must enter a medchandise description to save!");
                return;
            }

            string record = workRecord;
            if ( String.IsNullOrWhiteSpace(record ))
                record = G1.create_record("inventorylist", "casketdesc", "-1");
            if (record == "" || record == "0" || record == "-1")
            {
                MessageBox.Show("***ERROR*** Creating database entry for this merchandise!");
                return;
            }
            string desc = rtbDesc.Text;
            string price = rtbPrice.Text;
            string code = txtCode.Text;
            string type = txtType.Text;
            string guage = txtGuage.Text;
            string itemnumber = txtBatesville.Text;

            string money = price.Replace("$", "");
            money = money.Replace(",", "");

            G1.update_db_table("inventorylist", "record", record, new string[] { "casketdesc", desc, "casketcode", code, "caskettype", type, "casketguage", guage });
            G1.update_db_table("inventorylist", "record", record, new string[] { "casketcost", money, "itemnumber", itemnumber });
            if ( modifiedPicture)
            {
                Image myNewImage = this.pictureBox1.Image;
                ImageConverter converter = new ImageConverter();
                var bytes = (byte[])converter.ConvertTo(myNewImage, typeof(byte[]));
                G1.update_blob("inventorylist", "record", record, "picture", bytes);
            }
            if (ModuleDone != null)
            {
                if ( workAdding )
                    ModuleDone.Invoke("RELOAD " + record);
                else
                    ModuleDone.Invoke(record);
            }
            if (this.pictureBox1.Image != null)
                this.pictureBox1.Image.Dispose();
            this.pictureBox1.Image = null;
            this.Close();
        }
        /***********************************************************************************************/
        private void something_Changed(object sender, EventArgs e)
        {
            if (showing)
                return;
            if (!loading)
            {
                modified = true;
                btnSave.Show();
            }
        }
        /***********************************************************************************************/
        private void btnAttach_Clickx(object sender, EventArgs e)
        {
            this.pictureBox1.Focus();
        }
        /***********************************************************************************************/
        private void btnAttach_Click(object sender, EventArgs e)
        {
            string record = workRecord;
            using (OpenFileDialog ofdImage = new OpenFileDialog())
            {
                ofdImage.Multiselect = false;

                if (ofdImage.ShowDialog() == DialogResult.OK)
                {
                    string filename = ofdImage.FileName;
                    filename = filename.Replace('\\', '/');
                    if (!String.IsNullOrWhiteSpace(filename))
                    {
                        try
                        {
                            //                        string filename = @"C:\Users\Robby\Documents\SMFS\Inventory\Caskets\Y33_833_TDH_Finch.jpg";
                            Bitmap myNewImage = new Bitmap(filename);
                            this.pictureBox1.Image = (Bitmap)myNewImage;
                            modifiedPicture = true;
                            modified = true;
                            btnSave.Show();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("***ERROR*** Storing Image " + ex.ToString());
                        }
                    }
                }
                this.Refresh();
            }
        }
        /***********************************************************************************************/
        private void btnDetach_Click(object sender, EventArgs e)
        {
            this.pictureBox1.Image = emptyImage;
            modifiedPicture = true;
            modified = true;
            btnSave.Show();
        }
        /***************************************************************************************/
        public delegate void d_void_eventdone_string(string s);
        public event d_void_eventdone_string ModuleDone;
        /***********************************************************************************************/
        private void deceasedDate_EditValueChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            btnSelectCustomer.Show();
        }
        /***********************************************************************************************/
        private void btnSelectCustomer_Click(object sender, EventArgs e)
        {
            this.Hide();
            Customers custForm = new Customers(true);
            custForm.ModuleDone += CustForm_ModuleDone;
            custForm.ShowDialog();
            this.Show();
        }
        /***********************************************************************************************/
        private void CustForm_ModuleDone(string s)
        {
            if (String.IsNullOrWhiteSpace(s))
                return;
            if (String.IsNullOrWhiteSpace(workRecord))
                return;
            string customerRecord = s;
            string dateDeceased = deceasedDate.Text;
            dateDeceased = G1.date_to_sql(dateDeceased);
            string serviceID = txtServiceID.Text;
            try
            {
                G1.update_db_table("customers", "record", customerRecord, new string[] { "deceasedDate", dateDeceased, "ServiceId", serviceID, "!InventoryLocRecord", workRecord});
            }
            catch ( Exception ex)
            {
                MessageBox.Show("***ERROR*** Updating Customer Table with Merchandise and Deceased Date!");
            }
        }
        /***********************************************************************************************/
        private void renameCasketDescriptionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string newDescription = "";
            using (Ask askForm = new Ask("Enter New Casket Description?"))
            {
                askForm.Text = "";
                askForm.ShowDialog();
                if (askForm.DialogResult != DialogResult.OK)
                    return;
                newDescription = askForm.Answer;
            }
            string oldDesc = rtbDesc.Text;
            string mess = "Change Old (" + oldDesc + ") to New (" + newDescription + ") Plus all Matching Inventory?";

            DialogResult result = MessageBox.Show(mess, "Change Casket Description Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;
            string record = "";
            string cmd = "Select * from `inventory` where `casketdescription` = '" + oldDesc + "';";
            DataTable dt = G1.get_db_data(cmd);
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                record = dt.Rows[i]["record"].ObjToString();
                G1.update_db_table("inventory", "record", record, new string[] { "CasketDescription", newDescription });
            }
            if (!String.IsNullOrWhiteSpace(workRecord))
            {
                G1.update_db_table("inventorylist", "record", workRecord, new string[] { "CasketDesc", newDescription });
                rtbDesc.Text = newDescription;
            }
            MessageBox.Show("Okay, " + dt.Rows.Count + " Records Changed in Inventory plus Main Casket Description!");
        }
        /***********************************************************************************************/
        private void btnLeft_Click(object sender, EventArgs e)
        {
            int row = mainRow - 1;
            if (row < 0)
                return;
            workRecord = workDt.Rows[row]["record"].ObjToString();
            mainRow = row;
            Merchandise_Load(null, null);
        }
        /***********************************************************************************************/
        private void btnRight_Click(object sender, EventArgs e)
        {
            int row = mainRow + 1;
            if (row >= (workDt.Rows.Count - 1))
                return;
            workRecord = workDt.Rows[row]["record"].ObjToString();
            mainRow = row;
            Merchandise_Load(null, null);
        }
        /***********************************************************************************************/
        private bool GetFilename(out string filename, DragEventArgs e)
        {
            bool ret = false;
            filename = String.Empty;
            if ((e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy)
            {
                Array data = ((IDataObject)e.Data).GetData("FileDrop") as Array;
                if (data != null)
                {
                    if ((data.Length == 1) && (data.GetValue(0) is String))
                    {
                        filename = ((string[])data)[0];
                        string ext = Path.GetExtension(filename).ToLower();
                        if ((ext == ".jpg") || (ext == ".png") || (ext == ".bmp"))
                        {
                            ret = true;
                        }
                    }
                }
            }
            return ret;
        }
        /***********************************************************************************************/
        protected void LoadImage()

        {
            image = new Bitmap(path);
        }
        /***********************************************************************************************/
        private void PictureBox1_DragEnter(object sender, DragEventArgs e)
        {
            string filename = "";
            validData = GetFilename(out filename, e);
            if (validData)
            {
                path = filename;
                getImageThread = new Thread(new ThreadStart(LoadImage));
                getImageThread.Start();
                e.Effect = DragDropEffects.Copy;
            }
            else
                e.Effect = DragDropEffects.None;
        }
        /***********************************************************************************************/
        private void PictureBox1_DragDrop(object sender, DragEventArgs e)
        {
            if (validData)
            {
                while (getImageThread.IsAlive)
                {
                    Application.DoEvents();
                    Thread.Sleep(0);
                }
                pictureBox1.Image = image;
            }
        }

        private void pictureBox1_DragLeave(object sender, EventArgs e)
        {

        }

        private void pictureBox1_DragOver(object sender, DragEventArgs e)
        {

        }
        /***********************************************************************************************/
        private void btnChange_Click(object sender, EventArgs e)
        {
            if (pictureBox1.SizeMode == PictureBoxSizeMode.StretchImage)
                pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            else if (pictureBox1.SizeMode == PictureBoxSizeMode.Zoom)
                pictureBox1.SizeMode = PictureBoxSizeMode.AutoSize;
            else if (pictureBox1.SizeMode == PictureBoxSizeMode.AutoSize)
                pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;
            else if (pictureBox1.SizeMode == PictureBoxSizeMode.CenterImage)
                pictureBox1.SizeMode = PictureBoxSizeMode.Normal;
            else if (pictureBox1.SizeMode == PictureBoxSizeMode.Normal)
                pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
        }

        private void Merchandise_DragDrop(object sender, DragEventArgs e)
        {
            if (validData)
            {
                while (getImageThread.IsAlive)
                {
                    Application.DoEvents();
                    Thread.Sleep(0);
                }
                pictureBox1.Image = image;
            }

        }

        private void Merchandise_DragEnter(object sender, DragEventArgs e)
        {
            string filename = "";
            validData = GetFilename(out filename, e);
            if (validData)
            {
                path = filename;
                getImageThread = new Thread(new ThreadStart(LoadImage));
                getImageThread.Start();
                e.Effect = DragDropEffects.Copy;
            }
            else
                e.Effect = DragDropEffects.None;

        }
        /***********************************************************************************************/
        private void importClipBoardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Clipboard.ContainsImage())
            {

                pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;

                pictureBox1.Image = Clipboard.GetImage();
                modifiedPicture = true;
                modified = true;
                btnSave.Show();
            }
            else
            {
                MessageBox.Show("Clipboard is empty. Please Copy Image.");
            }
         }
    }
    /***********************************************************************************************/
}