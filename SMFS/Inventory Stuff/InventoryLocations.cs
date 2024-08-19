using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;

using GeneralLib;
using DevExpress.XtraReports.UI;
using DevExpress.XtraReports.ReportGeneration;
using DevExpress.XtraPrinting;
using DevExpress.Utils;
using DevExpress.XtraGrid.Views.Grid;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class InventoryLocations : DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        private Bitmap emptyImage;
        private DataTable _LocationList;
        private DataTable _TypeList;
        private DataTable _OwnerList;
        private DataTable _GuageList;
        private DataTable _UsedList;

        private DataTable originalDt = null;

        /***********************************************************************************************/
        public InventoryLocations()
        {
            InitializeComponent();
            SetupTotalsSummary();
        }
        /****************************************************************************************/
        private void SetupTotalsSummary()
        {
            AddSummaryColumn("count", "{0}");
            gridMain.Columns["count"].Visible = false;
        }
        /****************************************************************************************/
        private void AddSummaryColumn(string columnName, string format = "")
        {
            if (String.IsNullOrWhiteSpace(format))
                format = "${0:0,0.00}";
            gridMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            gridMain.Columns[columnName].SummaryItem.DisplayFormat = format;
        }
        /***********************************************************************************************/
        private void InventoryLocations_Load(object sender, EventArgs e)
        {
            chkOld.Hide();
            txtLookup.Hide();
            txtSort.Hide();
            txtTotal.Hide();
            setupUsedUnused();
            SetupVisibleColumns();
            LoadData();
            loadGroupCombo(cmbSelectColumns, "Inventory", "Primary");
        }
        /***********************************************************************************************/
        private void loadGroupCombo(System.Windows.Forms.ComboBox cmb, string key, string module)
        {
            cmb.Items.Clear();
            string cmd = "Select * from procfiles where ProcType = '" + key + "' AND `module` = '" + module + "' group by name;";
            DataTable dt = G1.get_db_data(cmd);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string name = dt.Rows[i]["Name"].ToString();
                cmb.Items.Add(name);
            }
        }
        /***********************************************************************************************/
        private void doPreferences ()
        {
            labValue.Hide();
            labelValue.Hide();

            string preference = G1.getPreference(LoginForm.username, "InventoryLocation", "DisplayTotalValue");
            if (preference != "YES")
            {
                labValue.Hide();
                labelValue.Hide();
            }
            else
            {
                labValue.Show();
                labelValue.Show();
            }
            //labValue.Hide();
            //labelValue.Hide();
        }
        /***********************************************************************************************/
        private DataTable GetAllInventoryData ()
        {
            string cmd = "Select * from `inventory` i JOIN `inventorylist` l on i.`casketdescription` = l.`casketdesc`;";
            cmd = "Select * from `inventory`;";
            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("merchandise");
            dt.Columns.Add("removed");
            dt.Columns.Add("num");
            dt.Columns.Add("count", Type.GetType("System.Double"));
            dt.Columns.Add("desc");
            dt.Columns.Add("casketcost", Type.GetType("System.Double"));
            dt.Columns.Add("caskettype");
            dt.Columns.Add("casketguage");

            SetupImageIcon();

            cmd = "Select * from inventorylist;";
            DataTable dx = G1.get_db_data(cmd);

            string desc = "";
            DataRow[] dRows = null;
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                if (dt.Rows[i]["del"].ObjToString() == "1")
                    dt.Rows[i]["removed"] = "YES";
                else
                    dt.Rows[i]["removed"] = "NO";

                desc = dt.Rows[i]["CasketDescription"].ObjToString();
                if ( !String.IsNullOrWhiteSpace ( desc))
                {
                    dRows = dx.Select("casketdesc='" + desc + "'");
                    if ( dRows.Length > 0 )
                    {
                        dt.Rows[i]["casketcost"] = dRows[0]["casketcost"].ObjToDouble();
                        dt.Rows[i]["caskettype"] = dRows[0]["caskettype"].ObjToString();
                        dt.Rows[i]["casketguage"] = dRows[0]["casketguage"].ObjToString();
                        if (dRows[0]["picture"] != null)
                        {
                            try
                            {
                                Byte[] bytes = dRows[0]["picture"].ObjToBytes();
                                if (bytes != null)
                                    dt.Rows[i]["Merchandise"] = "1";
                            }
                            catch (Exception ex)
                            {
                                //                            MessageBox.Show("***ERROR*** Reading Image " + ex.Message.ToString());
                            }
                        }

                    }
                }
            }

            string removed = chkComboRemoved.Text.Trim();

            DataRow [] dR = dt.Select("removed='" + removed + "'");
            int count = dt.Rows.Count;
            DataTable extraDt = dt.Clone();
            G1.ConvertToTable(dR, extraDt);
            count = extraDt.Rows.Count;

            string used = chkComboUsed.Text.Trim();
            if ( !String.IsNullOrWhiteSpace(used))
            {
                if ( used.ToUpper() == "USED")
                    dR = extraDt.Select("ServiceID<>''");
                else
                    dR = extraDt.Select("ServiceID=''");
                DataTable newDt = extraDt.Clone();
                G1.ConvertToTable(dR, newDt);
                extraDt  = newDt;
            }

            dt = extraDt;

            return dt;
        }
        /***********************************************************************************************/
        private void LoadData ()
        {
            bool testing = false;
            this.Cursor = Cursors.WaitCursor;
            emptyImage = new Bitmap(1, 1);

            doPreferences();

            printDate = "";
            printDateReceived = "";
            printDateDeceased = "";
            printLocation = chkComboLocation.Text;
            printType = chkComboType.Text;
            printOwner = chkComboOwner.Text;
            printGuage = chkComboGuage.Text;
            printUsed = chkComboUsed.Text;

            string usedUnused = getUsedQuery();

            getLocations();
            getTypes();
            getOwnership();
            getGuages();

            DataTable dt = null;
            dgv.DataSource = null;

//            testing = chkTesting.Checked;

            if (testing)
                dt = GetAllInventoryData();
            else
            {

//                string cmd = "Select * from `inventory` i JOIN `inventorylist` l on i.`casketdescription` = l.`casketdesc` ";
                string cmd = "Select i.*, l.`casketdesc`, l.`casketcode`, l.`casketcost`, l.`caskettype`, l.`casketguage`,l.`record`  from `inventory` i JOIN `inventorylist` l on i.`casketdescription` = l.`casketdesc` ";
                string where = " where ";

                string removed = getRemovedQuery();
                if (!String.IsNullOrWhiteSpace(removed))
                {
                    cmd += " " + where + " " + removed;
                    where = "AND";
                }

                string dates = getDateQuery();
                if (!String.IsNullOrWhiteSpace(dates))
                {
                    cmd += " " + where + " " + dates;
                    where = "AND";
                }

                dates = getDateReceivedQuery();
                if (!String.IsNullOrWhiteSpace(dates))
                {
                    cmd += " " + where + " " + dates;
                    where = "AND";
                }

                dates = getDateDeceasedQuery();
                if (!String.IsNullOrWhiteSpace(dates))
                {
                    cmd += " " + where + " " + dates;
                    where = "AND";
                }

                string locations = getLocationQuery();
                if (!String.IsNullOrWhiteSpace(locations))
                {
                    cmd += " " + where + " " + locations;
                    where = "AND";
                }
                string types = getTypeQuery();
                if (!String.IsNullOrWhiteSpace(types))
                {
                    cmd += " " + where + " " + types;
                    where = "AND";
                }
                string owner = getOwnerQuery();
                if (!String.IsNullOrWhiteSpace(owner))
                {
                    cmd += " " + where + " " + owner;
                    where = "AND";
                }
                string guage = getGuageQuery();
                if (!String.IsNullOrWhiteSpace(guage))
                {
                    cmd += " " + where + " " + guage;
                    where = "AND";
                }

                if (!String.IsNullOrWhiteSpace(usedUnused))
                {
                    cmd += " " + where + " " + usedUnused;
                    where = "AND";
                }

                if (chkOld.Checked)
                    cmd += " ORDER BY `CasketDescription`, `LocationCode`, `SerialNumber` ";
                cmd += ";";

                dt = G1.get_db_data(cmd);

                dt.Columns.Add("merchandise");
                dt.Columns.Add("removed");
                dt.Columns.Add("num");
                dt.Columns.Add("count", Type.GetType("System.Double"));
                dt.Columns.Add("desc");

                //DataTable dx = GetAllInventoryData();
                //bool found = false;
                //for ( int i=0; i<dx.Rows.Count; i++)
                //{
                //    found = false;
                //    string rec = dx.Rows[i]["record"].ObjToString();
                //    for ( int j=0; j<dt.Rows.Count; j++)
                //    {
                //        if ( dt.Rows[j]["record"].ObjToString() == rec )
                //        {
                //            found = true;
                //            break;
                //        }
                //    }
                //    if ( !found)
                //    {

                //    }
                //}
            }

            DateTime start = DateTime.Now;
            DateTime stop = DateTime.Now;
            DateTime finished = stop;

            if (!chkOld.Checked)
            {
                DataView tempview = dt.DefaultView;
                tempview.Sort = "CasketDescription,LocationCode,SerialNumber";
                dt = tempview.ToTable();
                finished = DateTime.Now;
            }
            TimeSpan ts = stop - start;
            txtLookup.Text = ts.TotalSeconds.ToString();
            ts = finished - stop;
            txtSort.Text = ts.TotalSeconds.ToString();
            ts = finished - start;
            txtTotal.Text = ts.TotalSeconds.ToString();

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dt.Rows[i]["count"] = 1D;
                dt.Rows[i]["desc"] = dt.Rows[i]["CasketDescription"].ObjToString().ToUpper();
            }

            G1.NumberDataTable(dt);
            if ( !testing )
                LoadMerchandise(dt);

            double totValue = calcTotalValue(dt);
            labValue.Text = "$" + G1.ReformatMoney(totValue);

            if (chkUntied.Checked)
                dt = UnTied(dt);

            dgv.DataSource = dt;
            dgv.RefreshDataSource();
            this.Cursor = Cursors.Default;
            if (chkGroupDesc.Checked)
                gridMain.ExpandAllGroups();
        }
        /***********************************************************************************************/
        private DataTable UnTied(DataTable dt)
        {
            this.Cursor = Cursors.WaitCursor;
            string cmd = "Select * from `inventory`";
            DataTable dx = G1.get_db_data(cmd);
            DataTable dd = dx.Clone();
            cmd = "Select * from `inventorylist`;";
            DataTable dq = G1.get_db_data(cmd);
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                string desc = dx.Rows[i]["casketdescription"].ObjToString();
                DataRow[] dRows = dq.Select("casketdesc='" + desc + "'");
                if (dRows.Length <= 0)
                {
                    dd.ImportRow(dx.Rows[i]);
                }
            }
            this.Cursor = Cursors.Default;
            return dd;
        }
        /***********************************************************************************************/
        private DataTable UnTiedx( DataTable dt )
        {
            this.Cursor = Cursors.WaitCursor;
            string cmd = "Select * from `inventory`";
            DataTable dx = G1.get_db_data(cmd);
            DataTable dd = dx.Clone();
            for ( int i=0; i<dx.Rows.Count; i++)
            {
                string desc = dx.Rows[i]["casketdescription"].ObjToString();
                cmd = "Select * from `inventorylist` where `casketdesc` = '" + desc + "';";
                DataTable dq = G1.get_db_data(cmd);
                if ( dq.Rows.Count <= 0 )
                {
                    dd.ImportRow(dx.Rows[i]);
                }
            }
            this.Cursor = Cursors.Default;
            return dd;
        }
        /***********************************************************************************************/
        private double calcTotalValue ( DataTable dt )
        {
            double totValue = 0D;
            double cost = 0D;
            string str = "";
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                str = dt.Rows[i]["casketcost"].ObjToString();
                cost = str.ObjToDouble();
                totValue += cost;
            }
            return totValue;
        }
        /***********************************************************************************************/
        private void getLocations()
        {
            string cmd = "SELECT `LocationCode` FROM `inventory` GROUP BY `LocationCode` ASC;";
            _LocationList = G1.get_db_data(cmd);

            chkComboLocation.Properties.DataSource = _LocationList;
        }
        /***********************************************************************************************/
        private void getTypes()
        {
            string cmd = "SELECT `CasketType` FROM `inventorylist` GROUP BY `CasketType` ASC;";
            _TypeList = G1.get_db_data(cmd);
            chkComboType.Properties.DataSource = _TypeList;
        }
        /***********************************************************************************************/
        private void getOwnership()
        {
            string cmd = "SELECT `Ownership` FROM `inventory` GROUP BY `Ownership` ASC;";
            _OwnerList = G1.get_db_data(cmd);
            chkComboOwner.Properties.DataSource = _OwnerList;
        }
        /***********************************************************************************************/
        private void getGuages()
        {
            string cmd = "SELECT `CasketGuage` FROM `inventorylist` GROUP BY `CasketGuage` ASC;";
            _GuageList = G1.get_db_data(cmd);
            chkComboGuage.Properties.DataSource = _GuageList;
        }
        /***********************************************************************************************/
        private void setupUsedUnused()
        {
            _UsedList = new DataTable();
            _UsedList.Columns.Add("Used");
            _UsedList.Rows.Add("Used");
            _UsedList.Rows.Add("Unused");

            chkComboUsed.Properties.DataSource = _UsedList;
            chkComboUsed.Properties.DisplayMember = "Used";
            chkComboUsed.Properties.ValueMember = "Used";
            chkComboUsed.EditValue = "Unused";
        }
        /*******************************************************************************************/
        private string cleanComboItem ( string item )
        {
            string str = item.Replace("\r", "");
            str = item.Replace("\n", "");
            return str;
        }
        /*******************************************************************************************/
        private string getUsedQuery()
        {
            bool used = false;
            bool unused = false;
            string[] locIDs = this.chkComboUsed.EditValue.ToString().Split('|');
            for (int i = 0; i < locIDs.Length; i++)
            {
                if (!String.IsNullOrWhiteSpace(locIDs[i]))
                {
                    if (locIDs[i].Trim().ToUpper() == "USED")
                        used = true;
                    else if (locIDs[i].Trim().ToUpper() == "UNUSED")
                        unused = true;
                }
            }
            if (used && unused)
                return "";
            if (!used && !unused)
                return "";
            if (used)
                return "`ServiceID` <> ''";
            return "`ServiceID` = ''";
        }
        /*******************************************************************************************/
        private string getDateDeceasedQuery()
        {
            string dates = "";
            printDateDeceased = "";
            if (chkDateDeceased.Checked)
            {
                DateTime date = this.dateTimePicker5.Value;
                string date1 = date.Year.ToString("D4") + "-" + date.Month.ToString("D2") + "-" + date.Day.ToString("D2");

                date = this.dateTimePicker6.Value;
                string date2 = date.Year.ToString("D4") + "-" + date.Month.ToString("D2") + "-" + date.Day.ToString("D2");

                dates = " `deceasedDate` BETWEEN '" + date1 + "' AND '" + date2 + "' ";
                printDateDeceased = "Deceased Date : " + this.dateTimePicker5.Text;
                printDateDeceased += " -to- " + this.dateTimePicker6.Text;
            }
            return dates;
        }
        /*******************************************************************************************/
        private string getDateReceivedQuery()
        {
            string dates = "";
            printDateReceived = "";
            if (chkDateReceived.Checked)
            {
                DateTime date = this.dateTimePicker3.Value;
                string date1 = date.Year.ToString("D4") + "-" + date.Month.ToString("D2") + "-" + date.Day.ToString("D2");

                date = this.dateTimePicker4.Value;
                string date2 = date.Year.ToString("D4") + "-" + date.Month.ToString("D2") + "-" + date.Day.ToString("D2");

                dates = " `DateReceived` BETWEEN '" + date1 + "' AND '" + date2 + "' ";
                printDateReceived = "Received Date : " + this.dateTimePicker3.Text;
                printDateReceived += " -to- " + this.dateTimePicker4.Text;
            }
            return dates;
        }
        /*******************************************************************************************/
        private string getRemovedQuery()
        {
            string answer = chkComboRemoved.Text;
            if (answer.IndexOf("No") >= 0 && answer.IndexOf("Yes") >= 0)
                return "";
            string removed = " del = '' "; 
            if (chkComboRemoved.Text.Trim().ToUpper() == "YES")
                removed = " del = '1' ";
            return removed;
        }
        /*******************************************************************************************/
        private string getDateQuery()
        {
            string dates = "";
            printDate = "";
            if (chkDate.Checked)
            {
                DateTime date = this.dateTimePicker1.Value;
                string date1 = date.Year.ToString("D4") + "-" + date.Month.ToString("D2") + "-" + date.Day.ToString("D2");

                date = this.dateTimePicker2.Value;
                string date2 = date.Year.ToString("D4") + "-" + date.Month.ToString("D2") + "-" + date.Day.ToString("D2");

                dates = " `DateUsed` BETWEEN '" + date1 + "' AND '" + date2 + "' ";
                printDate = "Search Date : " + this.dateTimePicker1.Text;
                printDate += " -to- " + this.dateTimePicker2.Text;
            }
            return dates;
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
        /*******************************************************************************************/
        private string getTypeQuery()
        {
            string procLoc = "";
            string[] locIDs = this.chkComboType.EditValue.ToString().Split('|');
            for (int i = 0; i < locIDs.Length; i++)
            {
                if (!String.IsNullOrWhiteSpace(locIDs[i]))
                {
                    if (procLoc.Trim().Length > 0)
                        procLoc += ",";
                    procLoc += "'" + locIDs[i].Trim() + "'";
                }
            }
            return procLoc.Length > 0 ? " `CasketType` IN (" + procLoc + ") " : "";
        }
        /*******************************************************************************************/
        private string getOwnerQuery()
        {
            string procLoc = "";
            string[] locIDs = this.chkComboOwner.EditValue.ToString().Split('|');
            for (int i = 0; i < locIDs.Length; i++)
            {
                if (!String.IsNullOrWhiteSpace(locIDs[i]))
                {
                    if (procLoc.Trim().Length > 0)
                        procLoc += ",";
                    procLoc += "'" + locIDs[i].Trim() + "'";
                }
            }
            return procLoc.Length > 0 ? " `Ownership` IN (" + procLoc + ") " : "";
        }
        /*******************************************************************************************/
        private string getGuageQuery()
        {
            string procLoc = "";
            string[] locIDs = this.chkComboGuage.EditValue.ToString().Split('|');
            for (int i = 0; i < locIDs.Length; i++)
            {
                if (!String.IsNullOrWhiteSpace(locIDs[i]))
                {
                    if (procLoc.Trim().Length > 0)
                        procLoc += ",";
                    procLoc += "'" + locIDs[i] + "'";
                }
            }
            return procLoc.Length > 0 ? " `CasketGuage` IN (" + procLoc + ") " : "";
        }
        /***********************************************************************************************/
        private void LoadMerchandisex(DataTable dt)
        {
            SetupImageIcon();
            try
            {
                Image myImage = null;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (dt.Rows[i]["del"].ObjToString() == "1")
                        dt.Rows[i]["removed"] = "YES";
                    else
                        dt.Rows[i]["removed"] = "NO";
                    if (dt.Rows[i]["picture"] != null)
                    {
                        //                        Byte[] bytes = dt.Rows[i]["picture"].ObjToBytes();
                        try
                        {
                            if ( i == 9 )
                            {

                            }
                            Byte[] bytes = dt.Rows[i]["picture"].ObjToBytes();
                            if (bytes != null)
                            {
                                dt.Rows[i]["Merchandise"] = "1";
                                //myImage = G1.byteArrayToImage(bytes);
                                //if (myImage != null)
                                //{
                                //    if (myImage != emptyImage)
                                //        dt.Rows[i]["Merchandise"] = "1";
                                //    myImage.Dispose();
                                //    myImage = null;
                                //}
                            }
                        }
                        catch ( Exception ex)
                        {
//                            MessageBox.Show("***ERROR*** Reading Image " + ex.Message.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Reading Image " + ex.Message.ToString());
            }
        }
        /***********************************************************************************************/
        private void LoadMerchandise(DataTable dt)
        {
            SetupImageIcon();
            string cmd = "Select * from inventorylist;";
            DataTable dx = G1.get_db_data(cmd);

            string desc = "";
            DataRow[] dRows = null;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (dt.Rows[i]["del"].ObjToString() == "1")
                    dt.Rows[i]["removed"] = "YES";
                else
                    dt.Rows[i]["removed"] = "NO";

                desc = dt.Rows[i]["CasketDescription"].ObjToString();
                if (!String.IsNullOrWhiteSpace(desc))
                {
                    dRows = dx.Select("casketdesc='" + desc + "'");
                    if (dRows.Length > 0)
                    {
                        //dt.Rows[i]["casketcost"] = dRows[0]["casketcost"].ObjToDouble();
                        //dt.Rows[i]["caskettype"] = dRows[0]["caskettype"].ObjToString();
                        //dt.Rows[i]["casketguage"] = dRows[0]["casketguage"].ObjToString();
                        if (dRows[0]["picture"] != null)
                        {
                            try
                            {
                                Byte[] bytes = dRows[0]["picture"].ObjToBytes();
                                if (bytes != null)
                                    dt.Rows[i]["Merchandise"] = "1";
                            }
                            catch (Exception ex)
                            {
                                //                            MessageBox.Show("***ERROR*** Reading Image " + ex.Message.ToString());
                            }
                        }

                    }
                }
            }

            //try
            //{
            //    Image myImage = null;
            //    for (int i = 0; i < dt.Rows.Count; i++)
            //    {
            //        if (dt.Rows[i]["del"].ObjToString() == "1")
            //            dt.Rows[i]["removed"] = "YES";
            //        else
            //            dt.Rows[i]["removed"] = "NO";
            //        if (dt.Rows[i]["picture"] != null)
            //        {
            //            //                        Byte[] bytes = dt.Rows[i]["picture"].ObjToBytes();
            //            try
            //            {
            //                if (i == 9)
            //                {

            //                }
            //                Byte[] bytes = dt.Rows[i]["picture"].ObjToBytes();
            //                if (bytes != null)
            //                {
            //                    dt.Rows[i]["Merchandise"] = "1";
            //                    //myImage = G1.byteArrayToImage(bytes);
            //                    //if (myImage != null)
            //                    //{
            //                    //    if (myImage != emptyImage)
            //                    //        dt.Rows[i]["Merchandise"] = "1";
            //                    //    myImage.Dispose();
            //                    //    myImage = null;
            //                    //}
            //                }
            //            }
            //            catch (Exception ex)
            //            {
            //                //                            MessageBox.Show("***ERROR*** Reading Image " + ex.Message.ToString());
            //            }
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show("***ERROR*** Reading Image " + ex.Message.ToString());
            //}
        }
        /***********************************************************************************************/
        private void SetupImageIcon()
        {
            DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew = this.repositoryItemCheckEdit1;
            selectnew.NullText = "";
            selectnew.ValueChecked = "1";
            selectnew.ValueUnchecked = "0";
            selectnew.ValueGrayed = "";
        }
        /***********************************************************************************************/
        private void loadMerchandiseImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
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
                            ImageConverter converter = new ImageConverter();
                            var bytes = (byte[])converter.ConvertTo(myNewImage, typeof(byte[]));
                            G1.update_blob("inventorylist", "record", record, "picture", bytes);
                            LoadData();
                        }
                        catch ( Exception ex )
                        {
                            MessageBox.Show("***ERROR*** Storing Image " + ex.ToString());
                        }
                    }
                }
                dgv.Refresh();
                this.Refresh();
            }
        }
        /***********************************************************************************************/
        private void clearImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            DialogResult result = MessageBox.Show("Are you sure you want to this Image from this Merchandise?", "Clear Image Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;
            ImageConverter converter = new ImageConverter();
            var bytes = (byte[])converter.ConvertTo(emptyImage, typeof(byte[]));
            G1.update_blob("inventorylist", "record", record, "picture", bytes);
            LoadData();
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
        private void gridMain_MouseDown_1(object sender, MouseEventArgs e)
        {
            int rowHandle = gridMain.FocusedRowHandle;
            GridHitInfo hi = gridMain.CalcHitInfo(e.Location);
            if (hi.RowHandle < 0)
                return;
            if (hi.Column != null)
            {
                rowHandle = hi.RowHandle;
                DataRow dr = gridMain.GetDataRow(rowHandle);
                if (hi.Column.FieldName.ToUpper() == "MERCHANDISE")
                {
//                    DataTable dt = (DataTable)dgv.DataSource;
                    string desc = dr["casketdescription"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(desc))
                    {
                        string cmd = "Select * from `inventorylist` where `casketdesc` = '" + desc + "';";
                        DataTable dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count > 0)
                        {
                            string record = dx.Rows[0]["record"].ObjToString();
                            Merchandise mercForm = new Merchandise(record, "LOOK");
                            mercForm.Show();
                        }
                    }
                }
            }
        }
        /***********************************************************************************************/
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadData();
        }
        /***********************************************************************************************/
        void nmenu_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menu = (ToolStripMenuItem)sender;
            string name = menu.Name;
            int index = getGridColumnIndex(name);
            if (index < 0)
                return;
            if (menu.Checked)
            {
                menu.Checked = false;
                gridMain.Columns[index].Visible = false;
            }
            else
            {
                menu.Checked = true;
                gridMain.Columns[index].Visible = true;
            }
            gridMain.RefreshData();
            dgv.Refresh();
            ToolStripMenuItem xmenu = this.columnsToolStripMenuItem;
            xmenu.ShowDropDown();
        }
        /***********************************************************************************************/
        private int getGridColumnIndex(string columnName)
        {
            int index = -1;
            for (int i = 0; i < gridMain.Columns.Count; i++)
            {
                string name = gridMain.Columns[i].Name;
                if (name == columnName)
                {
                    index = i;
                    break;
                }
            }
            return index;
        }
        /***********************************************************************************************/
        private void SetupVisibleColumns()
        {
            ToolStripMenuItem menu = this.columnsToolStripMenuItem;
            for (int i = 0; i < gridMain.Columns.Count; i++)
            {
                string name = gridMain.Columns[i].Name;
                string caption = gridMain.Columns[i].Caption;
                ToolStripMenuItem nmenu = new ToolStripMenuItem();
                nmenu.Name = name;
                nmenu.Text = caption;
                nmenu.Checked = true;
                nmenu.Click += new EventHandler(nmenu_Click);
                menu.DropDownItems.Add(nmenu);
            }
        }
        /***********************************************************************************************/
        private string printDate = "";
        private string printDateReceived = "";
        private string printDateDeceased = "";
        private string printLocation = "";
        private string printType = "";
        private string printGuage = "";
        private string printUsed = "";
        private string printOwner = "";
        /***********************************************************************************************/
        private void printPreviewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if ( this.components == null)
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

            Printer.setupPrinterMargins(50, 50, 135, 50);

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

            Printer.setupPrinterMargins(50, 50, 135, 50);

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
        private int pageMarginLeft = 0;
        private int pageMarginRight = 0;
        private int pageMarginTop = 0;
        private int pageMarginBottom = 0;
        /***********************************************************************************************/
        private void printableComponentLink1_BeforeCreateAreas(object sender, EventArgs e)
        {

            //gridMain.Columns["num"].Visible = false;
            //gridMain.Columns["picture"].Visible = false;
            //if (workGroup)
            //    gridMain.Columns["notes"].Visible = false;
            //gridMain.Columns["cyclenotes"].Visible = false;
            //if (!workGroup)
            //    printUserName = true;
        }
        /***********************************************************************************************/
        private void printableComponentLink1_AfterCreateAreas(object sender, EventArgs e)
        {
            //gridMain.Columns["num"].Visible = true;
            //if (workGroup)
            //    gridMain.Columns["picture"].Visible = true;
            //gridMain.Columns["notes"].Visible = true;
            //if (workGroup)
            //    gridMain.Columns["cyclenotes"].Visible = true;
            //printUserName = false;
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
            Printer.DrawQuad(1, 1, Printer.xQuads, 1, "South Mississippi Funeral Services, LLC", Color.Black, BorderSide.Bottom, font, HorizontalAlignment.Center);

            Printer.SetQuadSize(12, 12);

            font = new Font("Ariel", 8);
            Printer.DrawGridDate(2, 1, 2, 2, Color.Black, BorderSide.None, font );
            Printer.DrawGridPage(11, 2, 2, 2, Color.Black, BorderSide.None, font);
            Printer.DrawQuad(1, 3, 2, 2, "User : " + LoginForm.username, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);

            string search = "Search Date : All";
            if (chkDate.Checked)
                search = printDate;
            Printer.DrawQuad(1, 6, 6, 2, search, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);

            font = new Font("Ariel", 8);
            search = "Location : All";
            if (!String.IsNullOrWhiteSpace(printLocation))
                search = "Location : " + printLocation;
            Printer.DrawQuad(1, 8, 6, 2, search, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);

            search = "Type : All";
            if (!String.IsNullOrWhiteSpace(printType))
                search = "Type : " + printType;
            Printer.DrawQuad(1, 10, 6, 2, search, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);

            search = "Guage : All";
            if (!String.IsNullOrWhiteSpace(printGuage))
                search = "Guage : " + printGuage;
            Printer.DrawQuad(7, 6, 6, 2, search, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);

            search = "Ownership : All";
            if (!String.IsNullOrWhiteSpace(printOwner))
                search = "Ownership : " + printOwner;
            Printer.DrawQuad(7, 8, 6, 2, search, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);

            search = "Used/Unused : All";
            if (!String.IsNullOrWhiteSpace(printUsed))
                search = "Used/Unused : " + printUsed;
            Printer.DrawQuad(7, 10, 6, 2, search, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);

            if ( labValue.Visible)
            {
                string str = labelValue.Text + " " + labValue.Text;
                Printer.DrawQuad(10, 9, 3, 3, str, Color.Black, BorderSide.Right, labValue.Font, HorizontalAlignment.Left, VertAlignment.Top);
            }

            font = new Font("Ariel", 10, FontStyle.Bold | FontStyle.Italic);
            Printer.DrawQuad(6, 3, 2, 2, "Inventory List", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorderRight(6, 5, 1, 12, BorderSide.Right, 1, Color.Black);
            Printer.DrawQuadBorderRight(12, 6, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /***********************************************************************************************/
        private void btnAdd_Click(object sender, EventArgs e)
        {
            InventoryList listForm = new InventoryList( true );
            listForm.ModuleDone += ListForm_ModuleDone;
            listForm.Show();
        }
        /***********************************************************************************************/
        private void ListForm_ModuleDone(string s)
        {
            string merchandiseRecord = s;
            if (String.IsNullOrWhiteSpace(merchandiseRecord))
                return;
            Merchandise mercForm = new Merchandise(merchandiseRecord, "", true);
            mercForm.ModuleDone += MercForm_ModuleDone;
            mercForm.Show();
        }
        /***********************************************************************************************/
        private void ReLoad(string s)
        {
            try
            {
                string[] Lines = s.Split(' ');
                if (Lines.Length >= 2)
                {
                    if (G1.validate_numeric(Lines[1]))
                    {
                        DataTable dt = (DataTable)dgv.DataSource;
                        string record = Lines[1].ObjToString();
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            if (dt.Rows[i]["record"].ObjToString() == record)
                            {
                                gridMain.FocusedRowHandle = i;
                                gridMain.SelectRow(i);
                                break;
                            }
                        }
                    }
                }
            }
            catch { }
        }
        /***********************************************************************************************/
        private void MercForm_ModuleDone(string s)
        {
            if (String.IsNullOrWhiteSpace(s))
                return;
            if (s.Trim().ToString().IndexOf("RELOAD") >= 0 )
            {
                LoadData();
                ReLoad(s);
                return;
            }
            string cmd = "Select * from `inventory` where `record` = '" + s + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return;

            string serialNumber = dt.Rows[0]["SerialNumber"].ObjToString();
            string locationCode = dt.Rows[0]["LocationCode"].ObjToString();
            string dateReceived = dt.Rows[0]["DateReceived"].ObjToString();
            DateTime date = dateReceived.ObjToDateTime();
            dateReceived = G1.DTtoSQLString(date);
            string ownership = dt.Rows[0]["Ownership"].ObjToString();
            string serviceID = dt.Rows[0]["ServiceID"].ObjToString();
            string dateUsed = dt.Rows[0]["DateUsed"].ObjToString();
            date = dateUsed.ObjToDateTime();
            dateUsed = G1.DTtoSQLString(date);

            string dateDeceased = dt.Rows[0]["deceasedDate"].ObjToString();
            date = dateDeceased.ObjToDateTime();
            dateDeceased = G1.DTtoSQLString(date);

            string localRecord = "";
            DataTable dt2 = (DataTable)dgv.DataSource;
            for (int i = 0; i < dt2.Rows.Count; i++)
            {
                localRecord = dt2.Rows[i]["record"].ObjToString();
                if (s == localRecord)
                {
//                    G1.copy_dt_row(dt, 0, dt2, i);
                    dt2.Rows[i]["SerialNumber"] = serialNumber;
                    dt2.Rows[i]["LocationCode"] = locationCode;
                    dt2.Rows[i]["DateReceived"] = dt.Rows[0]["DateReceived"];
                    dt2.Rows[i]["Ownership"] = ownership;
                    dt2.Rows[i]["ServiceId"] = serviceID;
                    dt2.Rows[i]["DateUsed"] = dt.Rows[0]["DateUsed"];
                    dt2.Rows[i]["deceasedDate"] = dt.Rows[0]["deceasedDate"];
                    dgv.RefreshDataSource();
                    dgv.Refresh();
                    gridMain.SelectRow(i);
                    this.Refresh();
                    break;
                }
            }
        }
        /***********************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataTable dx = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            string description = dr["CasketDescription"].ObjToString();
            if (String.IsNullOrWhiteSpace(description))
                return;
            string cmd = "Select * from `inventorylist` where `casketdesc` = '" + description + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                string mercRecord = dt.Rows[0]["record"].ObjToString();
                Merchandise mercForm = new Merchandise(mercRecord, record, false );
                mercForm.ModuleDone += MercForm_ModuleDone;
                mercForm.Show();
            }
            else
                MessageBox.Show("***ERROR*** No match on Description " + description);
        }
        /***********************************************************************************************/
        private void btnDelete_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            int[] rows = gridMain.GetSelectedRows();
            int row = 0;
            DataRow dr = gridMain.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            string desc = dr["casketdesc"].ObjToString();
            string question = "***Warning*** Are you SURE you want to DELETE " + desc + " from INVENTORY?";
            if ( rows.Length > 1 )
                question = "***Warning*** Are you SURE you want to DELETE this GROUP of Merchandise from INVENTORY?";
            DialogResult result = MessageBox.Show(question, "Delete Inventory Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
            {
                MessageBox.Show("***INFO*** Okay, Inventory not deleted!", "Delete Merchandise Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            try
            {
                string reason = "";
                using (Ask askForm = new Ask("Enter Delete Reason?"))
                {
                    askForm.Text = "";
                    askForm.ShowDialog();
                    if (askForm.DialogResult != DialogResult.OK)
                        return;
                    reason = askForm.Answer;
                }

                for ( int i=0; i<rows.Length; i++)
                {
                    row = rows[i];
                    dr = gridMain.GetDataRow(row);
                    string serial = dr["SerialNumber"].ObjToString();
                    string location = dr["LocationCode"].ObjToString();
                    string what = reason + " (" + serial + ") " + desc + " " + location;
                    G1.AddToAudit(LoginForm.username, "Inventory", "DELETE", what);
                    G1.update_db_table("inventory", "record", record, new string[] { "del", "1" });
                }
//                G1.delete_db_table("inventory", "record", record);
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Deleting Inventory " + desc + "!");
            }
        }
        /***********************************************************************************************/
        private void importInventoryLocationsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImportInventoryList importForm = new ImportInventoryList( "Import Inventory by Location");
            importForm.Show();
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
            else if (e.Column.FieldName.ToUpper() == "DATERECEIVED" ||
                     e.Column.FieldName.ToUpper() == "DATEUSED" ||
                     e.Column.FieldName.ToUpper() == "DECEASEDDATE")
            {
                if (e.RowHandle >= 0)
                {
                    string date = e.DisplayText.Trim();
                    e.DisplayText = G1.DTtoYMDString(date, "MM/dd/yyyy");
                    if (e.DisplayText == "01/01/0001")
                        e.DisplayText = "";
                }
            }
        }
        /***********************************************************************************************/
        private void importSalesReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImportInventoryList importForm = new ImportInventoryList( "Import Sales Report");
            importForm.Show();
        }
        /***********************************************************************************************/
        private void flatFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImportCasketUsage caskitForm = new ImportCasketUsage( "Flatfile");
            caskitForm.Show();
        }
        /***********************************************************************************************/
        private void delimitedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImportCasketUsage caskitForm = new ImportCasketUsage("Delimited");
            caskitForm.Show();
        }
        /***********************************************************************************************/
        private void chkGroupDesc_CheckedChanged(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (chkGroupDesc.Checked)
            {
                DataView tempview = dt.DefaultView;
//                tempview.Sort = "CasketDescription";
                tempview.Sort = "desc";
                dt = tempview.ToTable();
                G1.NumberDataTable(dt);
                dgv.DataSource = dt;

//                gridMain.Columns["CasketDescription"].GroupIndex = 0;
                gridMain.Columns["desc"].GroupIndex = 0;
                gridMain.Columns["count"].Visible = true;
                gridMain.OptionsView.ShowFooter = true;
                this.gridMain.ExpandAllGroups();
            }
            else
            {
                DataView tempview = dt.DefaultView;
//                tempview.Sort = "CasketDescription";
                tempview.Sort = "desc";
                dt = tempview.ToTable();
                G1.NumberDataTable(dt);
                dgv.DataSource = dt;

//                gridMain.Columns["CasketDescription"].GroupIndex = -1;
                gridMain.Columns["desc"].GroupIndex = -1;
                gridMain.Columns["count"].Visible = false;
                gridMain.OptionsView.ShowFooter = true;
                gridMain.CollapseAllGroups();
            }
            gridMain.RefreshData();
            dgv.Refresh();
        }
        /***********************************************************************************************/
        private void ownerShipToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menu = (ToolStripMenuItem)sender;
            string what = menu.Text;
            if (String.IsNullOrWhiteSpace(what))
                return;
            DataTable dt = (DataTable)dgv.DataSource;

            int row = 0;
            string record = "";
            int count = gridMain.SelectedRowsCount;
            int[] rows = gridMain.GetSelectedRows();
            this.Cursor = Cursors.WaitCursor;
            string str = "";
            string old = "";
            string desc = "";
            for (int i = 0; i < rows.Length; i++)
            {
                row = rows[i];
                DataRow dr = gridMain.GetDataRow(row);
                record = dr["record"].ToString();
                old = dr["Ownership"].ObjToString();
                dr["Ownership"] = what;
                desc = dr["CasketDescription"].ObjToString();
                G1.update_db_table("inventory", "record", record, new string[] { "Ownership", what });
                str = "Change Rec(" + record + ") " + desc + " from " + old + " to " + what;
                G1.AddToAudit(LoginForm.username, "Merchandise", "Ownership", str );
            }
            //            dgv.DataSource = dt;
            dgv.Refresh();
        }
        /***********************************************************************************************/
        private void btnSelectColumns_Click(object sender, EventArgs e)
        {
            string actualName = cmbSelectColumns.Text;
            SelectColumns sform = new SelectColumns(dgv, "Inventory", "Primary", actualName);
            sform.Done += new SelectColumns.d_void_eventdone(sform_Done);
            sform.Show();
        }
        /***********************************************************************************************/
        void sform_Done()
        {
            dgv.Refresh();
            this.Refresh();
        }
        /***********************************************************************************************/
        private void cmbSelectColumns_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetupSelectedColumns();
        }
        /***********************************************************************************************/
        private void SetupSelectedColumns()
        {
            string group = cmbSelectColumns.Text.Trim().ToUpper();
            if (group.Trim().Length == 0)
                return;
            string cmd = "Select * from procfiles where name = '" + group + "' and ProcType = 'Inventory' order by seq";
            DataTable dt = G1.get_db_data(cmd);
            for (int i = 0; i < gridMain.Columns.Count; i++)
                gridMain.Columns[i].Visible = false;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string name = dt.Rows[i]["Description"].ToString();
                int index = dt.Rows[i]["seq"].ObjToInt32();
                try
                {
                    ((GridView)dgv.MainView).Columns[name].Visible = true;
                }
                catch
                {
                }
            }
        }
        /***********************************************************************************************/
        private void SetupSelectedColumns(string procType, string group, DevExpress.XtraGrid.GridControl dgv)
        {
            if (String.IsNullOrWhiteSpace(group))
                return;
            if (String.IsNullOrWhiteSpace(procType))
                procType = "Inventory";
            string cmd = "Select * from procfiles where name = '" + group + "' and ProcType = '" + procType + "' order by seq";
            DataTable dt = G1.get_db_data(cmd);
            DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gridMain = (DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView)dgv.MainView;
            for (int i = 0; i < gridMain.Columns.Count; i++)
                gridMain.Columns[i].Visible = false;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string name = dt.Rows[i]["Description"].ToString();
                int index = dt.Rows[i]["seq"].ObjToInt32();
                try
                {
                    ((GridView)dgv.MainView).Columns[name].Visible = true;
                }
                catch
                {
                }
            }
        }
        /***********************************************************************************************/
        private void chkTesting_CheckedChanged(object sender, EventArgs e)
        {
            LoadData();
        }
        /***********************************************************************************************/
        private void checkBillingReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            CheckBillingReport billForm = new CheckBillingReport( dt );
            billForm.Show();
        }
        /***********************************************************************************************/
        private void barcodeModuleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BarCodeInventory barcodeForm = new BarCodeInventory();
            barcodeForm.Show();
        }
        /***********************************************************************************************/
        private void deleteInventoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menu = (ToolStripMenuItem)sender;
            string what = menu.Text;
            if (String.IsNullOrWhiteSpace(what))
                return;
            DataTable dt = (DataTable)dgv.DataSource;

            int row = 0;
            string record = "";
            int count = gridMain.SelectedRowsCount;
            int[] rows = gridMain.GetSelectedRows();
            this.Cursor = Cursors.WaitCursor;
            int rowHandle = -1;
            string str = "";
            string old = "";
            string desc = "";
            string location = "";
            for (int i = 0; i < rows.Length; i++)
            {
                row = rows[i];
                DataRow dr = gridMain.GetDataRow(row);
                rowHandle = gridMain.GetRowHandle(row);
                record = dr["record"].ToString();
                old = dr["Ownership"].ObjToString();
                desc = dr["CasketDescription"].ObjToString();
                location = dr["LocationCode"].ObjToString();
                string question = "***Warning*** Are you SURE you want to DELETE " + desc + " from INVENTORY?";
                DialogResult result = MessageBox.Show(question, "Delete Inventory Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                if (result == DialogResult.No)
                {
                    MessageBox.Show("***INFO*** Okay, Inventory not deleted!", "Delete Merchandise Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    this.Cursor = Cursors.Default;
                    return;
                }

                str = "Delete Rec(" + record + ") " + desc + " Location " + location;
                G1.AddToAudit(LoginForm.username, "Merchandise", "DELETE", str);
                G1.delete_db_table("inventory", "record", record);
                ((GridView)dgv.MainView).DeleteRow ( rowHandle );
            }
            dgv.Refresh();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void chkGroupLocation_CheckedChanged(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (chkGroupLocation.Checked)
            {
                DataView tempview = dt.DefaultView;
                //                tempview.Sort = "CasketDescription";
                tempview.Sort = "LocationCode,desc";
                dt = tempview.ToTable();
                G1.NumberDataTable(dt);
                dgv.DataSource = dt;

                //                gridMain.Columns["CasketDescription"].GroupIndex = 0;
                gridMain.Columns["LocationCode"].GroupIndex = 0;
                gridMain.Columns["count"].Visible = true;
                gridMain.OptionsView.ShowFooter = true;
                this.gridMain.ExpandAllGroups();
            }
            else
            {
                DataView tempview = dt.DefaultView;
                //                tempview.Sort = "CasketDescription";
                tempview.Sort = "desc";
                dt = tempview.ToTable();
                G1.NumberDataTable(dt);
                dgv.DataSource = dt;

                //                gridMain.Columns["CasketDescription"].GroupIndex = -1;
                gridMain.Columns["LocationCode"].GroupIndex = -1;
                gridMain.Columns["count"].Visible = false;
                gridMain.OptionsView.ShowFooter = true;
                gridMain.CollapseAllGroups();
            }
            gridMain.RefreshData();
            dgv.Refresh();
        }
        /***********************************************************************************************/
        private void setAsUnusedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            string record = dr["record"].ObjToString();
            G1.update_db_table("inventory", "record", record, new string[] { "DateUsed", "0000-00-00", "ServiceID", "", "deceasedDate", "0000-00-00"});

            GridView view = (GridView)gridMain;
            DeleteSelectedRows(view);
        }
        /***********************************************************************************************/
        private void DeleteSelectedRows(DevExpress.XtraGrid.Views.Grid.GridView view)
        { // Delete Grid Row
            if (view == null || view.SelectedRowsCount == 0) return;
            DataRow[] rows = new DataRow[view.SelectedRowsCount];
            for (int i = 0; i < view.SelectedRowsCount; i++)
                rows[i] = view.GetDataRow(view.GetSelectedRows()[i]);
            view.BeginSort();
            try
            {
                foreach (DataRow row in rows)
                    row.Delete();
            }
            finally
            {
                view.EndSort();
            }
        }
        /***********************************************************************************************/
    }
}