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
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraEditors;
using ComboBox = System.Windows.Forms.ComboBox;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class InventoryLocationsNew : DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        private Bitmap emptyImage;
        private DataTable _LocationList;
        private DataTable _TypeList;
        private DataTable _OwnerList;
        private DataTable _GuageList;
        private DataTable _UsedList;

        private DataTable originalDt = null;
        private DataTable originalDtNew = null;
        private DataTable vaultDt = null;
        private DataTable infantDt = null;
        private DataTable miscDt = null;
        private bool loading = true;
        private bool workAsField = false;
        private bool first = true;
        private bool runPressed = false;

        /***********************************************************************************************/
        public InventoryLocationsNew(bool asField = false)
        {
            InitializeComponent();
            SetupTotalsSummary();
            workAsField = asField;
            if (G1.isField())
                workAsField = true;
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
        private void InventoryLocationsNew_Load(object sender, EventArgs e)
        {
            //label6.Hide();
            label3.Hide();
            //chkComboRemoved.Hide();
            chkComboOwner.Hide();

            btnSaveVaults.Hide();
            btnSaveInfant.Hide();
            btnSaveMisc.Hide();

            if (!G1.isAdminOrSuper())
                fixDateUsedToolStripMenuItem.Visible = false;

            LoadStatus();

            if (workAsField)
            {
                pictureBox3.Hide();
                pictureDelete.Hide();
            }

            if (!G1.isAdmin())
            {
                gridMain.Columns["gross"].Visible = false;
                gridMain.Columns["discount"].Visible = false;
                gridMain.Columns["net"].Visible = false;
                gridMain.Columns["surcharge"].Visible = false;
            }

            setupUsedUnused();

            tabControl1.DrawMode = TabDrawMode.OwnerDrawFixed;

            LoadData();

            loadGroupCombo(cmbSelectColumns, "Inventory", "Primary");
            loadLocatons();
            loadVaultOptions();
            loadInfanttOptions();
            loadMiscOptions();

            if (workAsField)
            {
                Rectangle rect = this.Bounds;
                int width = rect.Width - 500;
                this.SetBounds(rect.Left + 250, rect.Top, width, rect.Height);
            }
        }
        /***********************************************************************************************/
        private void LoadStatus()
        {
            repositoryItemComboBox7.Items.Add("None");

            DataTable dt = G1.get_db_data("Select * from `ref_inv_status`;");
            for (int i = 0; i < dt.Rows.Count; i++)
                repositoryItemComboBox7.Items.Add(dt.Rows[i]["inv_status"].ObjToString());
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
        private void loadVaultOptions()
        {
            string cmd = "Select * from `casket_master` where `casketcode` LIKE 'V%';";
            DataTable locDt = G1.get_db_data(cmd);
            for (int i = 0; i < locDt.Rows.Count; i++)
                repositoryItemComboBox2.Items.Add(locDt.Rows[i]["casketdesc"].ObjToString());
        }
        /***********************************************************************************************/
        private void loadInfanttOptions()
        {
            string cmd = "Select * from `casket_master` where `casketdesc` LIKE '%Infant%';";
            DataTable locDt = G1.get_db_data(cmd);

            for (int i = 0; i < locDt.Rows.Count; i++)
                repositoryItemComboBox4.Items.Add(locDt.Rows[i]["casketdesc"].ObjToString());
        }
        /***********************************************************************************************/
        private void loadMiscOptions()
        {
            string cmd = "Select * from `casket_master` where `casketcode` = 'Misc';";
            DataTable locDt = G1.get_db_data(cmd);
            for (int i = 0; i < locDt.Rows.Count; i++)
                repositoryItemComboBox6.Items.Add(locDt.Rows[i]["casketdesc"].ObjToString());
        }
        /***********************************************************************************************/
        private void loadLocatons()
        {
            string cmd = "Select * from `funeralhomes` order by `keycode`;";
            DataTable locDt = G1.get_db_data(cmd);
            for (int i = 0; i < locDt.Rows.Count; i++)
            {
                repositoryItemComboBox1.Items.Add(locDt.Rows[i]["LocationCode"].ObjToString());
                repositoryItemComboBox3.Items.Add(locDt.Rows[i]["LocationCode"].ObjToString());
                repositoryItemComboBox5.Items.Add(locDt.Rows[i]["LocationCode"].ObjToString());
            }
        }
        /***********************************************************************************************/
        private DataTable GetAllInventoryData()
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

            DataRow[] dR = dt.Select("removed='" + removed + "'");
            int count = dt.Rows.Count;
            DataTable extraDt = dt.Clone();
            G1.ConvertToTable(dR, extraDt);
            count = extraDt.Rows.Count;

            string used = chkComboUsed.Text.Trim();
            if (!String.IsNullOrWhiteSpace(used))
            {
                if (used.ToUpper() == "USED")
                    dR = extraDt.Select("ServiceID<>''");
                else
                    dR = extraDt.Select("ServiceID=''");
                DataTable newDt = extraDt.Clone();
                G1.ConvertToTable(dR, newDt);
                extraDt = newDt;
            }

            dt = extraDt;

            return dt;
        }
        /***********************************************************************************************/
        private void LoadData()
        {
            bool testing = false;
            this.Cursor = Cursors.WaitCursor;
            emptyImage = new Bitmap(1, 1);

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

            bool filterAfter = false;


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
                string types = getTypeQuery(this.chkComboType);
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
                string guage = getGuageQuery(this.chkComboGuage);
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

                if (!chkUseSearch.Checked)
                {
                    cmd = "Select i.*, l.`casketdesc`, l.`casketcode`, l.`casketcost`, l.`caskettype`, l.`casketguage`,l.`record`  from `inventory` i JOIN `inventorylist` l on i.`casketdescription` = l.`casketdesc` ";

                    //where = " WHERE ";
                    //if (!first || runPressed)
                    //{
                    //    removed = getRemovedQuery();
                    //    if (!String.IsNullOrWhiteSpace(removed))
                    //    {
                    //        cmd += " " + where + " " + removed;
                    //        where = "AND";
                    //    }

                    //    locations = getLocationQuery();
                    //    if (!String.IsNullOrWhiteSpace(locations))
                    //    {
                    //        cmd += " " + where + " " + locations;
                    //        where = "AND";
                    //    }
                    //    types = getTypeQuery(this.chkComboType);
                    //    if (!String.IsNullOrWhiteSpace(types))
                    //    {
                    //        cmd += " " + where + " " + types;
                    //        where = "AND";
                    //    }
                    //    owner = getOwnerQuery();
                    //    if (!String.IsNullOrWhiteSpace(owner))
                    //    {
                    //        cmd += " " + where + " " + owner;
                    //        where = "AND";
                    //    }
                    //    guage = getGuageQuery(this.chkComboGuage);
                    //    if (!String.IsNullOrWhiteSpace(guage))
                    //    {
                    //        cmd += " " + where + " " + guage;
                    //        where = "AND";
                    //    }
                    //    if (!String.IsNullOrWhiteSpace(usedUnused))
                    //    {
                    //        cmd += " " + where + " " + usedUnused;
                    //        where = "AND";
                    //    }
                    //}
                    cmd += " ORDER BY `CasketDescription` ASC, `LocationCode` ASC";

                    filterAfter = true;
                }
                cmd += ";";

                dt = G1.get_db_data(cmd); // Ramma Zamma

                dt.Columns.Add("merchandise");
                dt.Columns.Add("removed");
                dt.Columns.Add("num");
                dt.Columns.Add("count", Type.GetType("System.Double"));
                dt.Columns.Add("desc");
            }

            DateTime start = DateTime.Now;
            DateTime stop = DateTime.Now;
            DateTime finished = stop;

            TimeSpan ts = stop - start;
            ts = finished - stop;
            ts = finished - start;

            double dValue = 0D;
            string str = "";

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dt.Rows[i]["count"] = 1D;
                dt.Rows[i]["desc"] = dt.Rows[i]["CasketDescription"].ObjToString().ToUpper();
                str = dt.Rows[i]["casketguage"].ObjToString();
                str = str.Replace("\r", "");
                if (!String.IsNullOrWhiteSpace(str))
                {
                    dValue = str.ObjToDouble();
                    str = G1.ReformatMoney(dValue);
                    dt.Rows[i]["casketguage"] = str;
                }
            }

            G1.NumberDataTable(dt);
            if (!testing)
                LoadMerchandise(dt);

            //double totValue = calcTotalValue(dt);

            if (chkUseSearch.Checked)
            {
                string what = cmbWhat.Text.Trim().ToUpper();
                if (what == "UNTIED")
                    dt = UnTied(dt);
            }

            originalDt = dt;

            if (first)
            {
                DataRow[] dR = dt.Select("ServiceID='' AND del<>'1'");
                if (dR.Length > 0)
                    dt = dR.CopyToDataTable();

            }
            else
            {
                dgv.DataSource = dt;
                dgv.RefreshDataSource();
                chkComboLocation_EditValueChanged(null, null);
                dt = (DataTable)dgv.DataSource;
            }

            first = false;

            if (workAsField)
            {
                DataView tempview = dt.DefaultView;
                tempview.Sort = "LocationCode ASC, CasketDescription ASC, DateReceived ASC";
                dt = tempview.ToTable();
            }

            dgv.DataSource = dt;
            dgv.RefreshDataSource();

            dgv5.DataSource = dt;

            this.Cursor = Cursors.Default;
            if (chkGroupDesc.Checked)
                gridMain.ExpandAllGroups();

            originalDtNew = dt;
            dgv.RefreshDataSource();

            LoadVaults();
            LoadInfants();
            LoadMisc();

            FixFieldUsers();

            if (chkUseSearch.Checked)
                chkComboLocationNew_EditValueChanged(null, null);

            loading = false;
        }
        /***********************************************************************************************/
        private void FixFieldUsers ()
        {
            if ( !G1.isAdmin() )
            { // Only make these columns available to Admin and no one else
                gridMain.Columns["gross"].OptionsColumn.ShowInCustomizationForm = false;
                gridMain.Columns["discount"].OptionsColumn.ShowInCustomizationForm = false;
                gridMain.Columns["net"].OptionsColumn.ShowInCustomizationForm = false;
                gridMain.Columns["surcharge"].OptionsColumn.ShowInCustomizationForm = false;
            }
            if (!workAsField)
            {
                tabControl1.TabPages.Remove(tabFieldCaskets);
                tabControl1.TabPages.Remove(tabFieldVaults);
                tabControl1.TabPages.Remove(tabFieldInfants);
                tabControl1.TabPages.Remove(tabFieldMisc);
                return;
            }

            tabControl1.TabPages.Remove(tabCaskets);
            tabControl1.TabPages.Remove(tabVaults);
            tabControl1.TabPages.Remove(tabInfants);
            tabControl1.TabPages.Remove(tabMiscMerchandise);

            HideGridChooser(gridMain5);
            HideGridChooser(gridMain6);
            //HideGridChooser(gridMain7);
            //HideGridChooser(gridMain8);

            checkBillingReportToolStripMenuItem.Visible = false;
            menuShowOrdersOnHand.Visible = false;
            miscToolStripMenuItem.Visible = false;
        }
        /***********************************************************************************************/
        private void HideGridChooser (DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gridMain )
        {
            for ( int i=0; i<gridMain.Columns.Count; i++)
            {
                if (!gridMain.Columns[i].Visible)
                    gridMain.Columns[i].OptionsColumn.ShowInCustomizationForm = false;
            }
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
            //string cmd = "SELECT `LocationCode` FROM `inventory` GROUP BY `LocationCode` ASC;";
            string cmd = "Select `LocationCode` from `funeralhomes` order by `LocationCode`;";
            _LocationList = G1.get_db_data(cmd);

            chkComboLocation.Properties.DataSource = _LocationList;
            chkComboLocationNew.Properties.DataSource = _LocationList;
            chkComboLocationNewVault.Properties.DataSource = _LocationList;
            chkComboLocationNewInfant.Properties.DataSource = _LocationList;
            chkComboLocationNewMisc.Properties.DataSource = _LocationList;
        }
        /***********************************************************************************************/
        private void getTypes()
        {
            string cmd = "SELECT `CasketType` FROM `inventorylist` GROUP BY `CasketType` ASC;";
            _TypeList = G1.get_db_data(cmd);
            chkComboType.Properties.DataSource = _TypeList;
            chkComboTypeNew.Properties.DataSource = _TypeList;
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



            DataTable dt = _GuageList.Copy();
            _GuageList.Rows.Clear();
            string str = "";
            double guage = 0D;
            DataRow [] dRows = null;
            DataRow dR = null;

            for ( int i=0; i<dt.Rows.Count; i++)
            {
                str = dt.Rows[i]["casketGuage"].ObjToString().Trim();
                str = str.Replace("\r", "");
                guage = str.ObjToDouble();
                if ( guage > 0D)
                {
                    str = G1.ReformatMoney(guage);
                    dRows = _GuageList.Select("casketGuage='" + str + "'");
                    if ( dRows.Length <= 0 )
                    {
                        dR = _GuageList.NewRow();
                        dR["CasketGuage"] = G1.ReformatMoney(guage);
                        _GuageList.Rows.Add(dR);
                    }
                }
            }

            DataView tempview = _GuageList.DefaultView;
            tempview.Sort = "casketGuage asc";
            _GuageList = tempview.ToTable();

            chkComboGuage.Properties.DataSource = _GuageList;
            chkComboGuageNew
                .Properties.DataSource = _GuageList;
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
            string what = this.cmbWhat.Text.Trim().ToUpper();
            if (chkUseSearch.Checked && what == "DATE DECEASED")
            {
                DateTime date = this.dateTimePicker1.Value;
                string date1 = date.Year.ToString("D4") + "-" + date.Month.ToString("D2") + "-" + date.Day.ToString("D2");

                date = this.dateTimePicker2.Value;
                string date2 = date.Year.ToString("D4") + "-" + date.Month.ToString("D2") + "-" + date.Day.ToString("D2");

                dates = " `deceasedDate` BETWEEN '" + date1 + "' AND '" + date2 + "' ";
                printDateDeceased = "Deceased Date : " + this.dateTimePicker1.Text;
                printDateDeceased += " -to- " + this.dateTimePicker2.Text;
            }
            return dates;
        }
        /*******************************************************************************************/
        private string getDateReceivedQuery()
        {
            string dates = "";
            printDateReceived = "";
            string what = this.cmbWhat.Text.Trim().ToUpper();
            if (chkUseSearch.Checked && what == "DATE RECEIVED")
            {
                DateTime date = this.dateTimePicker1.Value;
                string date1 = date.Year.ToString("D4") + "-" + date.Month.ToString("D2") + "-" + date.Day.ToString("D2");

                date = this.dateTimePicker2.Value;
                string date2 = date.Year.ToString("D4") + "-" + date.Month.ToString("D2") + "-" + date.Day.ToString("D2");

                dates = " `DateReceived` BETWEEN '" + date1 + "' AND '" + date2 + "' ";
                printDateReceived = "Received Date : " + this.dateTimePicker1.Text;
                printDateReceived += " -to- " + this.dateTimePicker2.Text;
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
            string what = this.cmbWhat.Text.Trim().ToUpper();
            if (chkUseSearch.Checked && what == "DATE USED" )
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
        private string getTypeQuery(DevExpress.XtraEditors.CheckedComboBoxEdit combo )
        {
            string procLoc = "";
            //string[] locIDs = this.chkComboType.EditValue.ToString().Split('|');
            string[] locIDs = combo.EditValue.ToString().Split('|');
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
        private string getGuageQuery(DevExpress.XtraEditors.CheckedComboBoxEdit combo )
        {
            string procLoc = "";
            string str = "";
            //string[] locIDs = this.chkComboGuage.EditValue.ToString().Split('|');
            string[] locIDs = combo.EditValue.ToString().Split('|');

            for (int i = 0; i < locIDs.Length; i++)
            {
                str = locIDs[i].Trim();
                if (string.IsNullOrWhiteSpace(str))
                    continue;
                if (procLoc.Length > 0)
                    procLoc += " OR ";
                if (!String.IsNullOrWhiteSpace(locIDs[i]))
                    procLoc += "`casketGuage` LIKE '" + locIDs[i].Trim() + "%'";
            }


            //for (int i = 0; i < locIDs.Length; i++)
            //{
            //    if (!String.IsNullOrWhiteSpace(locIDs[i]))
            //    {
            //        if (procLoc.Trim().Length > 0)
            //            procLoc += ",";
            //        procLoc += "'" + locIDs[i] + "'";
            //    }
            //}
            //return procLoc.Length > 0 ? " `CasketGuage` IN (" + procLoc + ") " : "";
            return procLoc.Length > 0 ? " (" + procLoc + ") " : "";
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
            runPressed = true;
            LoadData();
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

            if ( dgv5.Visible )
                printableComponentLink1.Component = dgv5;
            else
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

            if (dgv5.Visible)
                printableComponentLink1.Component = dgv5;
            else
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

            printLocation = chkComboLocation.Text;
            printType = chkComboType.Text;
            printOwner = chkComboOwner.Text;
            printGuage = chkComboGuage.Text;
            printUsed = chkComboUsed.Text;

            if ( dgv5.Visible )
            {
                printLocation = chkComboLocationNew.Text;
                printType = chkComboTypeNew.Text;
                printOwner = chkComboOwner.Text;
                printGuage = chkComboGuageNew.Text;
                printUsed = chkComboUsed.Text;
           }

            if (chkUseSearch.Checked)
            {
                if (!String.IsNullOrWhiteSpace(printDate))
                    search = printDate;
                else if (String.IsNullOrWhiteSpace(printDateDeceased))
                    search = printDateDeceased;
                else if (String.IsNullOrWhiteSpace(printDateReceived))
                    search = printDateReceived;
            }
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

            font = new Font("Ariel", 10, FontStyle.Bold | FontStyle.Italic);
            Printer.DrawQuad(6, 3, 2, 2, "Inventory List", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorderRight(6, 5, 1, 12, BorderSide.Right, 1, Color.Black);
            Printer.DrawQuadBorderRight(12, 6, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /***********************************************************************************************/
        private void btnAdd_Click(object sender, EventArgs e)
        {
            InventoryList listForm = new InventoryList(true);
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
            if (workAsField)
                return;
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
        /***************************************************************************************/
        public void FireEventReloadInventory( string what )
        {
            if (what.ToUpper() == "VAULT")
            {
                LoadVaults();
                if (G1.isAdminOrSuper())
                    dgv2.Refresh();
                else
                    dgv6.Refresh();
            }
            else if (what.ToUpper() == "INFANT")
            {
                LoadInfants();
                if (G1.isAdminOrSuper())
                    dgv3.Refresh();
                else
                    dgv7.Refresh();
            }
            else if (what.ToUpper() == "MISC")
            {
                LoadMisc();
                if (G1.isAdminOrSuper())
                    dgv4.Refresh();
                else
                    dgv8.Refresh();
            }
        }
        /***********************************************************************************************/
        private void LoadVaults()
        {
            string cmd = "Select * from `inventory_other` where `type` = 'vault'";
            DataTable dt = G1.get_db_data(cmd);

            DataView tempview = dt.DefaultView;
            //tempview.Sort = "LocationCode asc, description asc, DateReceived desc";
            tempview.Sort = "LocationCode asc, description asc, usage asc";
            dt = tempview.ToTable();

            G1.NumberDataTable(dt);
            dgv2.DataSource = dt;
            dgv6.DataSource = dt;

            vaultDt = dt;
        }
        /***********************************************************************************************/
        private void LoadInfants()
        {
            string cmd = "Select * from `inventory_other` where `type` = 'infant'";
            DataTable dt = G1.get_db_data(cmd);

            DataView tempview = dt.DefaultView;
            //tempview.Sort = "LocationCode asc, description asc, DateReceived desc";
            tempview.Sort = "LocationCode asc, description asc, usage asc";
            dt = tempview.ToTable();

            G1.NumberDataTable(dt);
            dgv3.DataSource = dt;
            dgv7.DataSource = dt;

            infantDt = dt;
        }
        /***********************************************************************************************/
        private void LoadMisc()
        {
            string cmd = "Select * from `inventory_other` where `type` = 'misc'";
            DataTable dt = G1.get_db_data(cmd);

            DataView tempview = dt.DefaultView;
            //tempview.Sort = "LocationCode asc, description asc, DateReceived desc";
            tempview.Sort = "LocationCode asc, description asc, usage asc";
            dt = tempview.ToTable();

            G1.NumberDataTable(dt);
            dgv4.DataSource = dt;
            dgv8.DataSource = dt;

            miscDt = dt;
        }
        /***********************************************************************************************/
        private void SortVaults ( DataTable dt )
        {
            DataView tempview = dt.DefaultView;
            tempview.Sort = "LocationCode asc, description asc, DateReceived asc";
            dt = tempview.ToTable();

            string loc = "";
            string name = "";
            string oldStr = "";
            string str = "";
            int usage = 0;

            for ( int i=(dt.Rows.Count-1); i>=0; i--)
            {
                loc = dt.Rows[i]["LocationCode"].ObjToString();
                name = dt.Rows[i]["description"].ObjToString();
                str = loc + "|" + name;
                if (String.IsNullOrWhiteSpace(oldStr))
                    oldStr = str;
                if ( oldStr == str )
                {
                    usage++;
                    dt.Rows[i]["usage"] = usage;
                }
                else
                {
                    usage = 1;
                    dt.Rows[i]["usage"] = usage;
                    oldStr = str;
                }
            }

            G1.NumberDataTable(dt);

            dgv2.DataSource = dt;
            gridMain2.RefreshData();
            gridMain2.RefreshEditor(true);
            dgv2.Refresh();
            dgv6.DataSource = dt;
        }
        /***********************************************************************************************/
        private void picAddVault_Click(object sender, EventArgs e)
        {
            //DataTable dt = (DataTable)dgv2.DataSource;
            //DataRow dRow = dt.NewRow();
            //dRow["DateReceived"] = G1.DTtoMySQLDT(DateTime.Now);
            //dt.Rows.Add(dRow);

            string cmd = "Select * from `fcust_services` where `contractNumber` = '12345';";
            DataTable dx = G1.get_db_data(cmd);
            dx.Rows.Clear();

            Services serviceForm = new Services("GROUP 1 GPL", "CASKET GROUP 1", true, dx, "", "Merchandise", "Vault" );
            serviceForm.SelectDone += ServiceForm_SelectDone;
            serviceForm.Show();

        }
        /***********************************************************************************************/
        private void ServiceForm_SelectDone(DataTable dx, string what)
        {
            string service = "";

            DataTable dt = (DataTable)dgv2.DataSource;

            for ( int i=0; i<dx.Rows.Count; i++)
            {
                service = dx.Rows[i]["service"].ObjToString();

                DataRow dRow = dt.NewRow();
                dRow["DateReceived"] = G1.DTtoMySQLDT(DateTime.Now);
                dRow["type"] = "Vault";
                dRow["description"] = service;
                dt.Rows.Add(dRow);
            }

            SortVaults(dt);

            btnSaveVaults.Show();
        }
        /***********************************************************************************************/
        private void chkComboLocationNew_EditValueChanged(object sender, EventArgs e)
        {
            try
            {
                DataTable dt = null;
                string names = getLocationNameQuery ( this.chkComboLocationNew );
                DataRow[] dRows = originalDtNew.Select(names);
                if (dRows.Length > 0)
                {
                    dt = originalDtNew.Clone();
                    for (int i = 0; i < dRows.Length; i++)
                        dt.ImportRow(dRows[i]);
                }
                else
                    dt = originalDtNew.Copy();

                string type = chkComboTypeNew.Text;
                if (!String.IsNullOrWhiteSpace(type) && dt.Rows.Count > 0)
                {
                    names = getTypeQuery ( this.chkComboTypeNew );
                    dRows = dt.Select(names);
                    DataTable dt2 = originalDtNew.Clone();
                    for (int i = 0; i < dRows.Length; i++)
                        dt2.ImportRow(dRows[i]);
                    if (dt2.Rows.Count > 0)
                        dt = dt2.Copy();
                }

                type = chkComboGuageNew.Text;
                if (!String.IsNullOrWhiteSpace(type) && dt.Rows.Count > 0)
                {
                    names = getGuageQuery ( this.chkComboGuageNew );
                    dRows = dt.Select(names);
                    DataTable dt2 = dt.Clone();
                    for (int i = 0; i < dRows.Length; i++)
                        dt2.ImportRow(dRows[i]);
                    if (dt2.Rows.Count > 0)
                        dt = dt2.Copy();
                }
                G1.NumberDataTable(dt);
                dgv5.DataSource = dt;
            }
            catch ( Exception ex)
            {
            }
        }
        /*******************************************************************************************/
        private string getLocationNameQueryNew()
        {
            string procLoc = "";
            string[] locIDs = this.chkComboLocationNew.EditValue.ToString().Split('|');
            for (int i = 0; i < locIDs.Length; i++)
            {
                if (!String.IsNullOrWhiteSpace(locIDs[i]))
                {
                    string cmd = "Select * from `funeralhomes` where `LocationCode` = '" + locIDs[i].Trim() + "';";
                    DataTable dt = G1.get_db_data(cmd);
                    if (dt.Rows.Count > 0)
                    {
                        string id = dt.Rows[0]["locationCode"].ObjToString();
                        procLoc += "'" + id.Trim() + "',";
                    }
                }
            }
            procLoc = procLoc.TrimEnd(',');
            return procLoc.Length > 0 ? " `locationCode` IN (" + procLoc + ") " : "";
        }
        /***********************************************************************************************/
        private void chkComboTypeNew_EditValueChanged(object sender, EventArgs e)
        {
            chkComboLocationNew_EditValueChanged(null, null);
            //string names = getTypeQueryNew();
            //DataRow[] dRows = originalDtNew.Select(names);
            //DataTable dt = originalDtNew.Clone();
            //for (int i = 0; i < dRows.Length; i++)
            //    dt.ImportRow(dRows[i]);
            //string type = chkComboLocationNew.Text;
            //if (!String.IsNullOrWhiteSpace(type) && dt.Rows.Count > 0)
            //{
            //    names = getLocationNameQueryNew();
            //    dRows = dt.Select(names);
            //    DataTable dt2 = dt.Clone();
            //    for (int i = 0; i < dRows.Length; i++)
            //        dt2.ImportRow(dRows[i]);
            //    if (dt2.Rows.Count > 0)
            //        dt = dt2.Copy();
            //}
            //type = chkComboGuageNew.Text;
            //if (!String.IsNullOrWhiteSpace(type) && dt.Rows.Count > 0)
            //{
            //    names = getGuageQueryNew();
            //    dRows = dt.Select(names);
            //    DataTable dt2 = dt.Clone();
            //    for (int i = 0; i < dRows.Length; i++)
            //        dt2.ImportRow(dRows[i]);
            //    if (dt2.Rows.Count > 0)
            //        dt = dt2.Copy();
            //}
            //G1.NumberDataTable(dt);
            //dgv5.DataSource = dt;
        }
        /*******************************************************************************************/
        private string getTypeQueryNew()
        {
            string procLoc = "";
            string[] locIDs = this.chkComboTypeNew.EditValue.ToString().Split('|');
            for (int i = 0; i < locIDs.Length; i++)
            {
                if (!String.IsNullOrWhiteSpace(locIDs[i]))
                    procLoc += "'" + locIDs[i].Trim() + "',";
            }
            procLoc = procLoc.TrimEnd(',');
            return procLoc.Length > 0 ? " `casketType` IN (" + procLoc + ") " : "";
        }
        /***********************************************************************************************/
        private void chkComboGuageNew_EditValueChanged(object sender, EventArgs e)
        {
            chkComboLocationNew_EditValueChanged(null, null);
            //string names = getGuageQueryNew();
            //DataRow[] dRows = originalDtNew.Select(names);
            //DataTable dt = originalDtNew.Clone();
            //for (int i = 0; i < dRows.Length; i++)
            //    dt.ImportRow(dRows[i]);
            //string type = chkComboLocationNew.Text;
            //if (!String.IsNullOrWhiteSpace(type) && dt.Rows.Count > 0)
            //{
            //    names = getLocationNameQueryNew();
            //    dRows = dt.Select(names);
            //    DataTable dt2 = dt.Clone();
            //    for (int i = 0; i < dRows.Length; i++)
            //        dt2.ImportRow(dRows[i]);
            //    if (dt2.Rows.Count > 0)
            //        dt = dt2.Copy();
            //}
            //type = chkComboTypeNew.Text;
            //if (!String.IsNullOrWhiteSpace(type) && dt.Rows.Count > 0)
            //{
            //    names = getTypeQueryNew();
            //    dRows = dt.Select(names);
            //    DataTable dt2 = originalDtNew.Clone();
            //    for (int i = 0; i < dRows.Length; i++)
            //        dt2.ImportRow(dRows[i]);
            //    if (dt2.Rows.Count > 0)
            //        dt = dt2.Copy();
            //}
            //G1.NumberDataTable(dt);
            //dgv5.DataSource = dt;
            //G1.NumberDataTable(dt);
            //dgv5.DataSource = dt;
        }
        /*******************************************************************************************/
        private string getGuageQueryNew()
        {
            string procLoc = "";
            string str = "";
            string[] locIDs = this.chkComboGuageNew.EditValue.ToString().Split('|');
            for (int i = 0; i < locIDs.Length; i++)
            {
                str = locIDs[i].Trim();
                if (string.IsNullOrWhiteSpace(str))
                    continue;
                if (procLoc.Length > 0)
                    procLoc += " OR ";
                if (!String.IsNullOrWhiteSpace(locIDs[i]))
                    procLoc += "`casketGuage` LIKE '" + locIDs[i].Trim() + "%'";
            }
            //return procLoc.Length > 0 ? " `casketguage` IN (" + procLoc + ") " : "";
            return procLoc.Length > 0 ? " (" + procLoc + ") " : "";
        }
        /***********************************************************************************************/
        private void chkComboLocation_EditValueChanged(object sender, EventArgs e)
        {
            try
            {
                DataRow[] dRows = null;
                DataTable dt = null;
                string names = getLocationNameQuery ( this.chkComboLocation );
                if (!String.IsNullOrWhiteSpace(names))
                {
                    dRows = originalDt.Select(names);
                    dt = originalDt.Clone();
                    for (int i = 0; i < dRows.Length; i++)
                        dt.ImportRow(dRows[i]);
                }
                else
                    dt = originalDt;

                string type = chkComboType.Text;
                if (!String.IsNullOrWhiteSpace(type) && dt.Rows.Count > 0)
                {
                    names = getTypeQuery (this.chkComboType );
                    dRows = dt.Select(names);
                    if (dRows.Length > 0)
                    {
                        DataTable dt2 = originalDt.Clone();
                        for (int i = 0; i < dRows.Length; i++)
                            dt2.ImportRow(dRows[i]);
                        if (dt2.Rows.Count > 0)
                            dt = dt2.Copy();
                    }
                    else
                        dt.Rows.Clear();
                }

                type = chkComboGuage.Text;
                if (!String.IsNullOrWhiteSpace(type) && dt.Rows.Count > 0)
                {
                    names = getGuageQuery (this.chkComboGuage );
                    dRows = dt.Select(names);
                    if (dRows.Length > 0)
                    {
                        DataTable dt2 = dt.Clone();
                        for (int i = 0; i < dRows.Length; i++)
                            dt2.ImportRow(dRows[i]);
                        if (dt2.Rows.Count > 0)
                            dt = dt2.Copy();
                    }
                    else
                        dt.Rows.Clear();
                }

                DataRow[] dR = null;

                string removed = chkComboRemoved.Text.Trim();
                if (!String.IsNullOrWhiteSpace(removed))
                {
                    if ( removed.ToUpper() == "NO" )
                        dR = dt.Select("del=''");
                    else if ( removed.ToUpper() == "YES" )
                        dR = dt.Select("del='1'");
                    if (dR != null)
                    {
                        if (dR.Length > 0)
                            dt = dR.CopyToDataTable();
                        else
                            dt.Rows.Clear();
                    }
                }

                string used = chkComboUsed.Text.Trim();
                if (!String.IsNullOrWhiteSpace(used))
                {
                    dR = null;
                    if (used.ToUpper() == "USED")
                        dR = dt.Select("ServiceID<>''");
                    else
                        dR = dt.Select("ServiceID=''");
                    if (dR.Length > 0)
                        dt = dR.CopyToDataTable();
                    else
                        dt.Rows.Clear();
                }
                G1.NumberDataTable(dt);
                dgv.DataSource = dt;
            }
            catch (Exception ex)
            {
            }
        }
        /*******************************************************************************************/
        private string getLocationNameQuery( DevExpress.XtraEditors.CheckedComboBoxEdit combo )
        {
            string procLoc = "";
            string[] locIDs = combo.EditValue.ToString().Split('|');
            for (int i = 0; i < locIDs.Length; i++)
            {
                if (!String.IsNullOrWhiteSpace(locIDs[i]))
                {
                    string cmd = "Select * from `funeralhomes` where `LocationCode` = '" + locIDs[i].Trim() + "';";
                    DataTable dt = G1.get_db_data(cmd);
                    if (dt.Rows.Count > 0)
                    {
                        string id = dt.Rows[0]["locationCode"].ObjToString();
                        procLoc += "'" + id.Trim() + "',";
                    }
                }
            }
            procLoc = procLoc.TrimEnd(',');
            return procLoc.Length > 0 ? " `locationCode` IN (" + procLoc + ") " : "";
        }
        /***********************************************************************************************/
        private void chkComboType_EditValueChanged(object sender, EventArgs e)
        {
             chkComboLocation_EditValueChanged(null, null);
            //string names = getTypeQuery ( this.chkComboType );
            //DataRow[] dRows = originalDt.Select(names);
            //DataTable dt = originalDtNew.Clone();
            //for (int i = 0; i < dRows.Length; i++)
            //    dt.ImportRow(dRows[i]);
            //string type = chkComboLocationNew.Text;
            //if (!String.IsNullOrWhiteSpace(type) && dt.Rows.Count > 0)
            //{
            //    names = getLocationNameQuery ( this.chkComboLocation );
            //    dRows = dt.Select(names);
            //    DataTable dt2 = dt.Clone();
            //    for (int i = 0; i < dRows.Length; i++)
            //        dt2.ImportRow(dRows[i]);
            //    if (dt2.Rows.Count > 0)
            //        dt = dt2.Copy();
            //}
            //type = chkComboGuageNew.Text;
            //if (!String.IsNullOrWhiteSpace(type) && dt.Rows.Count > 0)
            //{
            //    names = getGuageQuery ( this.chkComboGuage );
            //    dRows = dt.Select(names);
            //    DataTable dt2 = dt.Clone();
            //    for (int i = 0; i < dRows.Length; i++)
            //        dt2.ImportRow(dRows[i]);
            //    if (dt2.Rows.Count > 0)
            //        dt = dt2.Copy();
            //}
            //string removed = chkComboRemoved.Text.Trim();
            //DataRow[] dR = dt.Select("removed='" + removed + "'");
            //if (dR.Length > 0)
            //    dt = dR.CopyToDataTable();


            //string used = chkComboUsed.Text.Trim();
            //if (!String.IsNullOrWhiteSpace(used))
            //{
            //    dR = null;
            //    if (used.ToUpper() == "USED")
            //        dR = dt.Select("ServiceID<>''");
            //    else
            //        dR = dt.Select("ServiceID=''");
            //    if (dR.Length > 0)
            //        dt = dR.CopyToDataTable();
            //}
            //G1.NumberDataTable(dt);
            //dgv.DataSource = dt;
        }
        /***********************************************************************************************/
        private void chkComboGuage_EditValueChanged(object sender, EventArgs e)
        {
            chkComboLocation_EditValueChanged(null, null);
            //string names = getGuageQuery(this.chkComboGuage);
            //DataRow[] dRows = originalDt.Select(names);
            //DataTable dt = originalDtNew.Clone();
            //for (int i = 0; i < dRows.Length; i++)
            //    dt.ImportRow(dRows[i]);
            //string type = chkComboLocationNew.Text;
            //if (!String.IsNullOrWhiteSpace(type) && dt.Rows.Count > 0)
            //{
            //    names = getLocationNameQuery(this.chkComboLocation);
            //    dRows = dt.Select(names);
            //    DataTable dt2 = dt.Clone();
            //    for (int i = 0; i < dRows.Length; i++)
            //        dt2.ImportRow(dRows[i]);
            //    if (dt2.Rows.Count > 0)
            //        dt = dt2.Copy();
            //}
            //type = chkComboTypeNew.Text;
            //if (!String.IsNullOrWhiteSpace(type) && dt.Rows.Count > 0)
            //{
            //    names = getTypeQuery(this.chkComboType);
            //    dRows = dt.Select(names);
            //    DataTable dt2 = originalDt.Clone();
            //    for (int i = 0; i < dRows.Length; i++)
            //        dt2.ImportRow(dRows[i]);
            //    if (dt2.Rows.Count > 0)
            //        dt = dt2.Copy();
            //}

            //string removed = chkComboRemoved.Text.Trim();
            //DataRow[] dR = dt.Select("removed='" + removed + "'");
            //if (dR.Length > 0)
            //    dt = dR.CopyToDataTable();

            //string used = chkComboUsed.Text.Trim();
            //if (!String.IsNullOrWhiteSpace(used))
            //{
            //    dR = null;
            //    if (used.ToUpper() == "USED")
            //        dR = dt.Select("ServiceID<>''");
            //    else
            //        dR = dt.Select("ServiceID=''");
            //    if (dR.Length > 0)
            //        dt = dR.CopyToDataTable();
            //}
            //G1.NumberDataTable(dt);
            //dgv.DataSource = dt;
        }
        /***********************************************************************************************/
        private void chkComboUsed_EditValueChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            if (dgv == null)
                return;
            chkComboLocation_EditValueChanged ( null, null );
        }
        /***********************************************************************************************/
        private void tabControl1_DrawItem(object sender, DrawItemEventArgs e)
        {
            int x = 1;
            try
            {
                Font font = null;
                TabPage tabNow = tabControl1.SelectedTab;
                if (tabControl1.TabPages[e.Index] == tabNow)
                    font = new Font("Tahoma", 8F, FontStyle.Bold | FontStyle.Italic);
                else
                    font = new Font("Tahoma", 8F, FontStyle.Bold);
                using (Brush br = new SolidBrush(Color.Transparent))
                {
                    Rectangle rect = e.Bounds;

                    //rect.Width += 10;

                    e.Graphics.FillRectangle(br, rect);
                    SizeF sz = e.Graphics.MeasureString(tabControl1.TabPages[e.Index].Text, font);
                    if (tabControl1.TabPages[e.Index] == tabNow)
                        e.Graphics.DrawString(tabControl1.TabPages[e.Index].Text, font, Brushes.Red, rect.Left + (rect.Width - sz.Width) / 2, rect.Top + (rect.Height - sz.Height) / 2 + 1);
                    else
                        e.Graphics.DrawString(tabControl1.TabPages[e.Index].Text, font, Brushes.Black, rect.Left + (rect.Width - sz.Width) / 2, rect.Top + (rect.Height - sz.Height) / 2 + 1);

                    e.DrawFocusRectangle();
                }
            }
            catch ( Exception ex)
            {
            }
        }
        /***********************************************************************************************/
        private void repositoryItemComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv2.DataSource;
            DataRow dr = gridMain2.GetFocusedDataRow();
            int rowhandle = gridMain2.FocusedRowHandle;
            int row = gridMain2.GetDataSourceRowIndex(rowhandle);

            ComboBoxEdit combo = (ComboBoxEdit)sender;
            string what = combo.Text.Trim();

            try
            {
                dr["LocationCode"] = what;
                dt.Rows[row]["LocationCode"] = what;
            }
            catch ( Exception ex)
            {
            }

            SortVaults(dt);

            gridMain2.RefreshEditor(true);

            btnSaveVaults.Show();
            btnSaveVaults.Refresh();

            //string record = dr["record"].ObjToString();
            //if (!String.IsNullOrWhiteSpace(record))
            //{
            //    string groupname = dr["groupname"].ObjToString();
            //    if (what.ToUpper() == "CLEAR")
            //        what = "";
            //    G1.update_db_table("funeralhomes", "record", record, new string[] { "groupname", what });
            //}
        }
        /***********************************************************************************************/
        private void btnSaveVaults_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv2.DataSource;

            string record = "";
            string desc = "";
            string location = "";
            string qty = "";
            string dateReceived = "";
            string cost = "";
            string retail = "";
            string usage = "";
            DateTime date = DateTime.Now;

            for ( int i=0; i<dt.Rows.Count; i++)
            {
                try
                {
                    record = dt.Rows[i]["record"].ObjToString();
                    desc = dt.Rows[i]["description"].ObjToString();
                    location = dt.Rows[i]["LocationCode"].ObjToString();
                    qty = dt.Rows[i]["qty"].ObjToString();
                    date = dt.Rows[i]["DateReceived"].ObjToDateTime();
                    cost = dt.Rows[i]["cost"].ObjToString();
                    retail = dt.Rows[i]["retail"].ObjToString();
                    usage = dt.Rows[i]["usage"].ObjToString();
                    if (String.IsNullOrWhiteSpace(record))
                        record = G1.create_record("inventory_other", "description", "-1");
                    if (G1.BadRecord("inventory_other", record))
                        break;
                    G1.update_db_table("inventory_other", "record", record, new string[] { "type", "vault", "description", desc, "LocationCode", location, "qty", qty, "DateReceived", date.ToString("MM/dd/yyyy"), "cost", cost, "retail", retail, "usage", usage });
                }
                catch ( Exception ex )
                {
                }
            }
            btnSaveVaults.Hide();
            btnSaveVaults.Refresh();

            dgv6.DataSource = dt;
        }
        /***********************************************************************************************/
        private void gridMain2_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            //ColumnView view = sender as ColumnView;
            if (e.Column.FieldName.ToUpper().IndexOf("DATE") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                if (e.DisplayText.IndexOf("0000") >= 0 || e.DisplayText.IndexOf("0001") >= 0)
                    e.DisplayText = "";
                else
                {
                    DateTime date = e.DisplayText.ObjToString().ObjToDateTime();
                    if (date.Year < 10)
                        e.DisplayText = "";
                    else
                        e.DisplayText = date.ToString("yyyy-MM-dd");
                    //e.DisplayText = date.ToString("MM/dd/yyyy");
                }
            }
        }
        /***********************************************************************************************/
        private void repositoryItemComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv2.DataSource;
            DataRow dr = gridMain2.GetFocusedDataRow();
            int rowhandle = gridMain2.FocusedRowHandle;
            int row = gridMain2.GetDataSourceRowIndex(rowhandle);

            ComboBoxEdit combo = (ComboBoxEdit)sender;
            string what = combo.Text.Trim();

            try
            {
                dr["description"] = what;
                dt.Rows[row]["description"] = what;
            }
            catch (Exception ex)
            {
            }

            SortVaults(dt);

            gridMain2.RefreshEditor(true);

            btnSaveVaults.Show();
            btnSaveVaults.Refresh();
        }
        /***********************************************************************************************/
        private void gridMain2_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            btnSaveVaults.Show();
            btnSaveVaults.Refresh();
        }
        /***********************************************************************************************/
        private void SortInfants(DataTable dt)
        {
            DataView tempview = dt.DefaultView;
            tempview.Sort = "LocationCode asc, description asc, DateReceived asc";
            dt = tempview.ToTable();

            string loc = "";
            string name = "";
            string oldStr = "";
            string str = "";
            int usage = 0;

            for (int i = (dt.Rows.Count - 1); i >= 0; i--)
            {
                loc = dt.Rows[i]["LocationCode"].ObjToString();
                name = dt.Rows[i]["description"].ObjToString();
                str = loc + "|" + name;
                if (String.IsNullOrWhiteSpace(oldStr))
                    oldStr = str;
                if (oldStr == str)
                {
                    usage++;
                    dt.Rows[i]["usage"] = usage;
                }
                else
                {
                    usage = 1;
                    dt.Rows[i]["usage"] = usage;
                    oldStr = str;
                }
            }

            G1.NumberDataTable(dt);

            dgv3.DataSource = dt;
            gridMain3.RefreshData();
            gridMain3.RefreshEditor(true);
            dgv3.Refresh();
            dgv7.DataSource = dt;
        }
        /***********************************************************************************************/
        private void picAddInfant_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv3.DataSource;
            DataRow dRow = dt.NewRow();
            dRow["DateReceived"] = G1.DTtoMySQLDT(DateTime.Now);
            dt.Rows.Add(dRow);

            SortInfants(dt);

            btnSaveInfant.Show();
            btnSaveInfant.Refresh();
        }
        /***********************************************************************************************/
        private void gridMain3_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            btnSaveInfant.Show();
            btnSaveInfant.Refresh();
        }
        /***********************************************************************************************/
        private void gridMain3_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            //ColumnView view = sender as ColumnView;
            if (e.Column.FieldName.ToUpper().IndexOf("DATE") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                if (e.DisplayText.IndexOf("0000") >= 0 || e.DisplayText.IndexOf("0001") >= 0)
                    e.DisplayText = "";
                else
                {
                    DateTime date = e.DisplayText.ObjToString().ObjToDateTime();
                    if (date.Year < 10)
                        e.DisplayText = "";
                    else
                        e.DisplayText = date.ToString("yyyy-MM-dd");
                    //e.DisplayText = date.ToString("MM/dd/yyyy");
                }
            }
        }
        /***********************************************************************************************/
        private void btnSaveInfant_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv3.DataSource;

            string record = "";
            string desc = "";
            string location = "";
            string qty = "";
            string dateReceived = "";
            string cost = "";
            string retail = "";
            string usage = "";
            DateTime date = DateTime.Now;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    record = dt.Rows[i]["record"].ObjToString();
                    desc = dt.Rows[i]["description"].ObjToString();
                    location = dt.Rows[i]["LocationCode"].ObjToString();
                    qty = dt.Rows[i]["qty"].ObjToString();
                    date = dt.Rows[i]["DateReceived"].ObjToDateTime();
                    cost = dt.Rows[i]["cost"].ObjToString();
                    retail = dt.Rows[i]["retail"].ObjToString();
                    usage = dt.Rows[i]["usage"].ObjToString();
                    if (String.IsNullOrWhiteSpace(record))
                        record = G1.create_record("inventory_other", "description", "-1");
                    if (G1.BadRecord("inventory_other", record))
                        break;
                    G1.update_db_table("inventory_other", "record", record, new string[] { "type", "infant", "description", desc, "LocationCode", location, "qty", qty, "DateReceived", date.ToString("MM/dd/yyyy"), "cost", cost, "retail", retail, "usage", usage });
                }
                catch (Exception ex)
                {
                }
            }
            btnSaveInfant.Hide();
            btnSaveInfant.Refresh();
            dgv7.DataSource = dt;
        }
        /***********************************************************************************************/
        private void chkComboLocationNewVault_EditValueChanged(object sender, EventArgs e)
        {
            try
            {
                DataRow[] dRows = null;
                DataTable dt = vaultDt.Clone();

                string names = getLocationNameQuery(this.chkComboLocationNewVault);
                if (!String.IsNullOrWhiteSpace(names))
                {
                    dRows = vaultDt.Select(names);
                    dt = vaultDt.Clone();
                    for (int i = 0; i < dRows.Length; i++)
                        dt.ImportRow(dRows[i]);
                }
                else
                    dt = vaultDt;

                G1.NumberDataTable(dt);
                dgv6.DataSource = dt;
            }
            catch (Exception ex)
            {
            }
        }
        /***********************************************************************************************/
        private void chkComboLocationNewInfant_EditValueChanged(object sender, EventArgs e)
        {
            try
            {
                DataRow[] dRows = null;
                DataTable dt = infantDt.Clone();

                string names = getLocationNameQuery(this.chkComboLocationNewInfant);
                if (!String.IsNullOrWhiteSpace(names))
                {
                    dRows = infantDt.Select(names);
                    dt = infantDt.Clone();
                    for (int i = 0; i < dRows.Length; i++)
                        dt.ImportRow(dRows[i]);
                }
                else
                    dt = infantDt;

                G1.NumberDataTable(dt);
                dgv7.DataSource = dt;
            }
            catch (Exception ex)
            {
            }
        }
        /***********************************************************************************************/
        private void repositoryItemComboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv3.DataSource;
            DataRow dr = gridMain3.GetFocusedDataRow();
            int rowhandle = gridMain3.FocusedRowHandle;
            int row = gridMain3.GetDataSourceRowIndex(rowhandle);

            ComboBoxEdit combo = (ComboBoxEdit)sender;
            string what = combo.Text.Trim();

            try
            {
                dr["LocationCode"] = what;
                dt.Rows[row]["LocationCode"] = what;
            }
            catch (Exception ex)
            {
            }

            SortInfants(dt);

            gridMain3.RefreshEditor(true);

            btnSaveInfant.Show();
            btnSaveInfant.Refresh();
        }
        /***********************************************************************************************/
        private void repositoryItemComboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv3.DataSource;
            DataRow dr = gridMain3.GetFocusedDataRow();
            int rowhandle = gridMain3.FocusedRowHandle;
            int row = gridMain3.GetDataSourceRowIndex(rowhandle);

            ComboBoxEdit combo = (ComboBoxEdit)sender;
            string what = combo.Text.Trim();

            try
            {
                dr["description"] = what;
                dt.Rows[row]["description"] = what;
            }
            catch (Exception ex)
            {
            }

            SortInfants(dt);

            gridMain3.RefreshEditor(true);

            btnSaveInfant.Show();
            btnSaveInfant.Refresh();
        }
        /***********************************************************************************************/
        private void gridMain4_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            btnSaveMisc.Show();
            btnSaveMisc.Refresh();
        }
        /***********************************************************************************************/
        private void SortMisc(DataTable dt)
        {
            DataView tempview = dt.DefaultView;
            tempview.Sort = "LocationCode asc, description asc, DateReceived asc";
            dt = tempview.ToTable();

            string loc = "";
            string name = "";
            string oldStr = "";
            string str = "";
            int usage = 0;

            for (int i = (dt.Rows.Count - 1); i >= 0; i--)
            {
                loc = dt.Rows[i]["LocationCode"].ObjToString();
                name = dt.Rows[i]["description"].ObjToString();
                str = loc + "|" + name;
                if (String.IsNullOrWhiteSpace(oldStr))
                    oldStr = str;
                if (oldStr == str)
                {
                    usage++;
                    dt.Rows[i]["usage"] = usage;
                }
                else
                {
                    usage = 1;
                    dt.Rows[i]["usage"] = usage;
                    oldStr = str;
                }
            }

            G1.NumberDataTable(dt);

            dgv4.DataSource = dt;
            gridMain4.RefreshData();
            gridMain4.RefreshEditor(true);
            dgv4.Refresh();
            dgv8.DataSource = dt;
        }
        /***********************************************************************************************/
        private void picAddMerc_Click(object sender, EventArgs e)
        {
            string cmd = "Select * from `fcust_services` where `contractNumber` = '12345';";
            DataTable dx = G1.get_db_data(cmd);
            dx.Rows.Clear();

            Services serviceForm = new Services("GROUP 1 GPL", "CASKET GROUP 1", true, dx, "", "Merchandise", "Miscellaneous");
            serviceForm.SelectDone += ServiceForm_SelectDone1;
            serviceForm.Show();
        }
        /***********************************************************************************************/
        private void ServiceForm_SelectDone1(DataTable dx, string what)
        {
            string service = "";

            DataTable dt = (DataTable)dgv4.DataSource;

            for (int i = 0; i < dx.Rows.Count; i++)
            {
                service = dx.Rows[i]["service"].ObjToString();

                DataRow dRow = dt.NewRow();
                dRow["DateReceived"] = G1.DTtoMySQLDT(DateTime.Now);
                dRow["type"] = "Misc";
                dRow["description"] = service;
                dt.Rows.Add(dRow);
            }

            SortMisc(dt);

            btnSaveMisc.Show();
        }
        /***********************************************************************************************/
        private void gridMain4_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            //ColumnView view = sender as ColumnView;
            if (e.Column.FieldName.ToUpper().IndexOf("DATE") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                if (e.DisplayText.IndexOf("0000") >= 0 || e.DisplayText.IndexOf("0001") >= 0)
                    e.DisplayText = "";
                else
                {
                    DateTime date = e.DisplayText.ObjToString().ObjToDateTime();
                    if (date.Year < 10)
                        e.DisplayText = "";
                    else
                        e.DisplayText = date.ToString("yyyy-MM-dd");
                    //e.DisplayText = date.ToString("MM/dd/yyyy");
                }
            }
        }
        /***********************************************************************************************/
        private void btnSaveMisc_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv4.DataSource;

            string record = "";
            string desc = "";
            string location = "";
            string qty = "";
            string dateReceived = "";
            string cost = "";
            string retail = "";
            string usage = "";
            DateTime date = DateTime.Now;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    record = dt.Rows[i]["record"].ObjToString();
                    desc = dt.Rows[i]["description"].ObjToString();
                    location = dt.Rows[i]["LocationCode"].ObjToString();
                    qty = dt.Rows[i]["qty"].ObjToString();
                    date = dt.Rows[i]["DateReceived"].ObjToDateTime();
                    cost = dt.Rows[i]["cost"].ObjToString();
                    retail = dt.Rows[i]["retail"].ObjToString();
                    usage = dt.Rows[i]["usage"].ObjToString();
                    if (String.IsNullOrWhiteSpace(record))
                        record = G1.create_record("inventory_other", "description", "-1");
                    if (G1.BadRecord("inventory_other", record))
                        break;
                    G1.update_db_table("inventory_other", "record", record, new string[] { "type", "misc", "description", desc, "LocationCode", location, "qty", qty, "DateReceived", date.ToString("MM/dd/yyyy"), "cost", cost, "retail", retail, "usage", usage });
                }
                catch (Exception ex)
                {
                }
            }
            btnSaveMisc.Hide();
            btnSaveMisc.Refresh();
            dgv7.DataSource = dt;
        }
        /***********************************************************************************************/
        private void repositoryItemComboBox5_SelectedIndexChanged(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv4.DataSource;
            DataRow dr = gridMain4.GetFocusedDataRow();
            int rowhandle = gridMain4.FocusedRowHandle;
            int row = gridMain4.GetDataSourceRowIndex(rowhandle);

            ComboBoxEdit combo = (ComboBoxEdit)sender;
            string what = combo.Text.Trim();

            try
            {
                dr["LocationCode"] = what;
                dt.Rows[row]["LocationCode"] = what;
            }
            catch (Exception ex)
            {
            }

            SortMisc(dt);

            gridMain4.RefreshEditor(true);

            btnSaveMisc.Show();
            btnSaveMisc.Refresh();
        }
        /***********************************************************************************************/
        private void repositoryItemComboBox6_SelectedIndexChanged(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv4.DataSource;
            DataRow dr = gridMain4.GetFocusedDataRow();
            int rowhandle = gridMain4.FocusedRowHandle;
            int row = gridMain4.GetDataSourceRowIndex(rowhandle);

            ComboBoxEdit combo = (ComboBoxEdit)sender;
            string what = combo.Text.Trim();

            try
            {
                dr["description"] = what;
                dt.Rows[row]["description"] = what;
            }
            catch (Exception ex)
            {
            }

            SortMisc(dt);

            gridMain4.RefreshEditor(true);

            btnSaveInfant.Show();
            btnSaveInfant.Refresh();
        }
        /***********************************************************************************************/
        private void chkComboLocationNewMisc_EditValueChanged(object sender, EventArgs e)
        {
            try
            {
                DataRow[] dRows = null;
                DataTable dt = miscDt.Clone();

                string names = getLocationNameQuery(this.chkComboLocationNewMisc);
                if (!String.IsNullOrWhiteSpace(names))
                {
                    dRows = miscDt.Select(names);
                    dt = miscDt.Clone();
                    for (int i = 0; i < dRows.Length; i++)
                        dt.ImportRow(dRows[i]);
                }
                else
                    dt = miscDt;

                G1.NumberDataTable(dt);
                dgv8.DataSource = dt;
            }
            catch (Exception ex)
            {
            }
        }
        /***********************************************************************************************/
        private void enterReceivedMerchandiseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OrdersMatch matchForm = new OrdersMatch();
            matchForm.Show();
        }
        /***********************************************************************************************/
        private void showOrdersOnHandToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Orders ordersForm = new Orders();
            ordersForm.Show();
        }
        /***********************************************************************************************/
        private void menuShowOrdersOnHand_Click(object sender, EventArgs e)
        {
            string locations = chkComboLocation.Text;
            Orders ordersForm = new Orders( locations );
            ordersForm.Show();
        }
        /***********************************************************************************************/
        private void repositoryItemComboBox7_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            DevExpress.XtraEditors.ComboBoxEdit combo = (DevExpress.XtraEditors.ComboBoxEdit) sender;
            //ComboBox combo = (ComboBox)sender;
            string status = combo.Text;
            if (String.IsNullOrWhiteSpace(status))
                return;


            DataRow dr = gridMain.GetFocusedDataRow();
            string record = dr["record"].ObjToString();

            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);

            if (String.IsNullOrWhiteSpace(record))
                return;
            if (record == "0" || record == "-1")
                return;

            if (status.ToUpper() == "NONE")
            {
                status = "";
                dr["status"] = "";
            }
            DataTable dt = (DataTable)dgv.DataSource;
            dt.Rows[row]["status"] = status;

            dgv.DataSource = dt;

            G1.update_db_table("inventory", "record", record, new string[] { "status", status });

            gridMain.RefreshEditor(true);
            gridMain.RefreshData();
            dgv.Refresh();
        }
        /***********************************************************************************************/
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (loading)
                return;

            string field = gridMain.FocusedColumn.FieldName.Trim().ToUpper();
            if (field != "NOTES")
                return;

            DataRow dr = gridMain.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            string notes = dr[field].ObjToString();

            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);

            if (String.IsNullOrWhiteSpace(record))
                return;
            if (record == "0" || record == "-1")
                return;

            DataTable dt = (DataTable)dgv.DataSource;
            dt.Rows[row][field] = notes;

            dgv.DataSource = dt;

            G1.update_db_table("inventory", "record", record, new string[] { "notes", notes });

            gridMain.RefreshEditor(true);
            gridMain.RefreshData();
            dgv.Refresh();
        }
        /***********************************************************************************************/
        private void chkComboRemoved_EditValueChanged(object sender, EventArgs e)
        {
            chkComboLocation_EditValueChanged(null, null);
        }
        /***********************************************************************************************/
        private void fixDateUsedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            InventoryDateUsed usedForm = new InventoryDateUsed();
            usedForm.Show();

            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void pictureBox2_Click(object sender, EventArgs e)
        {
            G1.ShowHideFindPanel(gridMain5);
        }
        /***********************************************************************************************/
        private void gridMain5_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
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
        private void picDeleteVault_Click(object sender, EventArgs e)
        {
            DataTable dx = (DataTable)dgv2.DataSource;
            DataRow dr = gridMain2.GetFocusedDataRow();

            int rowHandle = gridMain2.FocusedRowHandle;
            int row = gridMain2.GetDataSourceRowIndex(rowHandle);

            string record = dr["record"].ObjToString();
            string description = dr["description"].ObjToString();
            string question = "***Warning*** Are you SURE you want to DELETE (" + description + ") from Vault INVENTORY?";
            DialogResult result = MessageBox.Show(question, "Delete Vault Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            if (result == DialogResult.No)
            {
                MessageBox.Show("***INFO*** Okay, Vault not deleted!", "Delete Vault Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                this.Cursor = Cursors.Default;
                return;
            }

            G1.delete_db_table("inventory_other", "record", record);

            dx.Rows.RemoveAt(row);

            G1.NumberDataTable(dx);

            dgv2.DataSource = dx;
            dgv2.RefreshDataSource();
            dgv2.Refresh();

            gridMain2.RefreshEditor(true);
        }
        /***********************************************************************************************/
        private void picDeleteInfant_Click(object sender, EventArgs e)
        {
            DataTable dx = (DataTable)dgv3.DataSource;
            DataRow dr = gridMain3.GetFocusedDataRow();

            int rowHandle = gridMain3.FocusedRowHandle;
            int row = gridMain3.GetDataSourceRowIndex(rowHandle);

            string record = dr["record"].ObjToString();
            string description = dr["description"].ObjToString();
            string question = "***Warning*** Are you SURE you want to DELETE (" + description + ") from Infant Caskets INVENTORY?";
            DialogResult result = MessageBox.Show(question, "Delete Infant Casket Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            if (result == DialogResult.No)
            {
                MessageBox.Show("***INFO*** Okay, Infant Casket not deleted!", "Delete Infant Casket Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                this.Cursor = Cursors.Default;
                return;
            }

            G1.delete_db_table("inventory_other", "record", record);

            dx.Rows.RemoveAt(row);

            G1.NumberDataTable(dx);

            dgv3.DataSource = dx;
            dgv3.RefreshDataSource();
            dgv3.Refresh();

            gridMain3.RefreshEditor(true);
        }
        /***********************************************************************************************/
        private void picDeleteMerc_Click(object sender, EventArgs e)
        {
            DataTable dx = (DataTable)dgv4.DataSource;
            DataRow dr = gridMain4.GetFocusedDataRow();

            int rowHandle = gridMain4.FocusedRowHandle;
            int row = gridMain4.GetDataSourceRowIndex(rowHandle);

            string record = dr["record"].ObjToString();
            string description = dr["description"].ObjToString();
            string question = "***Warning*** Are you SURE you want to DELETE (" + description + ") from Miscellaneous Merchandise INVENTORY?";
            DialogResult result = MessageBox.Show(question, "Delete Miscellaneous Merchandise Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            if (result == DialogResult.No)
            {
                MessageBox.Show("***INFO*** Okay, Miscellaneous Merchandise not deleted!", "Delete Miscellaneous Merchandise Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                this.Cursor = Cursors.Default;
                return;
            }

            G1.delete_db_table("inventory_other", "record", record);

            dx.Rows.RemoveAt(row);

            G1.NumberDataTable(dx);

            dgv4.DataSource = dx;
            dgv4.RefreshDataSource();
            dgv4.Refresh();

            gridMain4.RefreshEditor(true);
        }
        /***********************************************************************************************/
        private void batchImportSalesReportsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BatchSalesReport batchForm = new BatchSalesReport();
            batchForm.Show();
        }
        /***********************************************************************************************/
    }
}