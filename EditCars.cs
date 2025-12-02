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
using DevExpress.XtraPrinting;
using DevExpress.Utils;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;
using GeneralLib;

/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class EditCars : DevExpress.XtraEditors.XtraForm
    {
        private bool modified = false;
        private bool modified_maint = false;
        private bool modified_vendor = false;
        private bool modified_service = false;
        private bool loading = true;
        private bool autoRun = false;
        private bool autoForce = false;
        private string workReport = "";
        private string sendTo = "";
        private string sendWhere = "";
        private string sendUsername = "";
        private string da = "";
        private string forceReportName = "";
        private string workReportIn = "";
        private string workModule = "";
        private string workSendTo = "";
        /****************************************************************************************/
        public EditCars()
        {
            InitializeComponent();
        }
        /****************************************************************************************/
        public EditCars(bool auto, bool force, string send, string sendTo, string username, string report, string ReportName = "")
        {
            InitializeComponent();
            autoRun = auto;
            autoForce = force;
            sendWhere = send;
            workSendTo = sendTo;
            sendUsername = username;
            workReportIn = report;
            forceReportName = ReportName;
            RunAutoReports();
            if (auto)
                this.Close();
        }
        /****************************************************************************************/
        private void EditCars_Load(object sender, EventArgs e)
        {
            btnSaveAll.Hide();
            btnSaveAllMaint.Hide();
            btnSaveAllVend.Hide();
            btnSaveAllService.Hide();
            btnSaveAllUpcoming.Hide();
            this.Text = "Edit Cars";

            string whereClause = "";
            string assignedLocations = "";
            string[] locations = null;
            string location = "";

            if (G1.isField())
            {
                // Field User View. Add a condition so that they only see data from their assigned locations
                string query = "SELECT `assignedLocations` FROM `users` WHERE `userName` = '" + LoginForm.username + "'";
                DataTable userDt = G1.get_db_data(query);
                if (userDt.Rows.Count > 0)
                    assignedLocations = userDt.Rows[0]["assignedLocations"].ObjToString();
                locations = assignedLocations.Split('~');
                if (locations.Length > 0)
                {
                    whereClause = "WHERE `location` = ";
                }
                for (int j = 0; j < locations.Length; j++)
                {
                    location = locations[j].Trim();
                    if (j == 0)
                    {
                        whereClause += "'" + location + "'";
                    }
                    else
                    {
                        whereClause += " OR `location` = '" + location + "' ";
                    }
                }

                tabControl1.TabPages.Remove(tabPage3);
                tabControl1.TabPages.Remove(tabPage4);
                tabControl1.TabPages.Remove(tabPage5);
                menuStrip1.Items.Remove(miscToolStripMenuItem);
                pictureBox6.Visible = false;
            }
            /* * The following else statement is for an admin view, but was used for developing the user view. Most, if not all of this code is for testing purposes only. *
            else
            {
                // Admin view
                // For Testing Purposes, log in as different testUsers
                string testUser = "";
                testUser = "Chanse";
                testUser = "WesleyM";
                string query = "SELECT `assignedLocations` FROM `users` WHERE `userName` = '" + testUser + "'";
                DataTable userDt = G1.get_db_data(query);
                if (userDt.Rows.Count > 0)
                    assignedLocations = userDt.Rows[0]["assignedLocations"].ObjToString();
                locations = assignedLocations.Split('~');
                if (locations.Length > 0)
                {
                    whereClause = "WHERE `location` = ";
                }
                for (int j = 0; j < locations.Length; j++)
                {
                    location = locations[j].Trim();
                    if (j == 0)
                    {
                        whereClause += "'"+location+"'";
                    }
                    else
                    {
                        whereClause += " OR `location` = '"+location+"' ";
                    }
                }

                tabControl1.TabPages.Remove(tabPage3);
                tabControl1.TabPages.Remove(tabPage4);
                tabControl1.TabPages.Remove(tabPage5);
                menuStrip1.Items.Remove(miscToolStripMenuItem);
                pictureBox6.Visible = false;
            }
            /**/
            string cmd = "Select * from `cars` "+whereClause.Trim()+" ORDER BY `tmstamp`;";
            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("num");
            dt.Columns.Add("mod");
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            whereClause = "";
            if(dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (i == 0)
                    {
                        whereClause = "WHERE `cars_record` = " + dt.Rows[i]["record"].ObjToString();
                    }
                    else 
                    {
                        whereClause += " OR `cars_record` = " + dt.Rows[i]["record"].ObjToString();
                    }
                }
            }
            
            cmd = "SELECT * FROM `cars_maint` "+whereClause.Trim()+" ORDER BY `tmstamp`;";
            DataTable dt2 = G1.get_db_data(cmd);
            dt2.Columns.Add("num");
            dt2.Columns.Add("mod");
            G1.NumberDataTable(dt2);
            dgv2.DataSource = dt2;

            cmd = "SELECT * FROM `cars_vendors` ORDER BY `record`;";
            DataTable dt3 = G1.get_db_data(cmd);
            dt3.Columns.Add("num");
            dt3.Columns.Add("mod");
            G1.NumberDataTable(dt3);
            dgv3.DataSource = dt3;

            cmd = "SELECT * FROM `cars_service_type` ORDER BY `record`;";
            DataTable dt4 = G1.get_db_data(cmd);
            dt4.Columns.Add("num");
            dt4.Columns.Add("mod");
            G1.NumberDataTable(dt4);
            dgv4.DataSource = dt4;

            string timeEnd = "";
//            timeEnd = "AND `service_sched_b_date` < DATE_ADD(CURDATE(), INTERVAL 7 DAY)";
            cmd = "SELECT * FROM `cars_maint` WHERE `service_sched_b_date` > CURDATE() "+timeEnd.Trim()+" ORDER BY `tmstamp`;";
            DataTable dt5 = G1.get_db_data(cmd);
            dt5.Columns.Add("num");
            dt5.Columns.Add("mod");
            G1.NumberDataTable(dt5);
            dgv5.DataSource = dt5;

            loadRepositories();

            loading = false;

            //int top = this.Top + 20;
            //int left = this.Left + 20;
            //this.SetBounds(left, top, this.Width, this.Height);
        }
        /***********************************************************************************************/
        private void loadRepositories()
        {
            loadRepositoryCars();
            loadRepositoryVendors();
            loadRepositoryServiceType();
            loadRepositoryCategory();
            loadRepositoryServCategory();
        }
        /***********************************************************************************************/
        private void loadRepositoryCars()
        {
            string whereClause = "";
            string assignedLocations = "";
            string[] locations = null;
            string location = "";

            if (G1.isField())
            {
                // Field User View. Add a condition so that they only see data from their assigned locations
                string query = "SELECT `assignedLocations` FROM `users` WHERE `userName` = '" + LoginForm.username + "'";
                DataTable userDt = G1.get_db_data(query);
                if (userDt.Rows.Count > 0)
                    assignedLocations = userDt.Rows[0]["assignedLocations"].ObjToString();
                locations = assignedLocations.Split('~');
                if (locations.Length > 0)
                {
                    whereClause = "WHERE `location` = ";
                }
                for (int j = 0; j < locations.Length; j++)
                {
                    location = locations[j].Trim();
                    if (j == 0)
                    {
                        whereClause += "'" + location + "'";
                    }
                    else
                    {
                        whereClause += " OR `location` = '" + location + "' ";
                    }
                }
            }
            /*  * The following else statement is for an admin view, but was used for developing the user view. Most, if not all of this code is for testing purposes only. 
            else
            {
                // Admin view
                // For Testing Purposes, log in as different testUsers
                string testUser = "";
                testUser = "Chanse";
                testUser = "WesleyM";
                string query = "SELECT `assignedLocations` FROM `users` WHERE `userName` = '" + testUser + "'";
                DataTable userDt = G1.get_db_data(query);
                if (userDt.Rows.Count > 0)
                    assignedLocations = userDt.Rows[0]["assignedLocations"].ObjToString();
                locations = assignedLocations.Split('~');
                if (locations.Length > 0)
                {
                    whereClause = "WHERE `location` = ";
                }
                for (int j = 0; j < locations.Length; j++)
                {
                    location = locations[j].Trim();
                    if (j == 0)
                    {
                        whereClause += "'" + location + "'";
                    }
                    else
                    {
                        whereClause += " OR `location` = '" + location + "' ";
                    }
                }
            }
            /**/
            string cmd = "Select * from `cars` " + whereClause.Trim() + " order by `tmstamp`;";
            DataTable carDt = G1.get_db_data(cmd);

            DataTable newCarDt = carDt.Clone();

            string assignedCars = "";

            DataView tempview = carDt.DefaultView;
            tempview.Sort = "record";
            carDt = tempview.ToTable();

            repositoryItemComboBox1.Items.Add("All");
            repositoryItemComboBox10.Items.Add("All");
            for (int i = 0; i < carDt.Rows.Count; i++)
            {
                repositoryItemComboBox1.Items.Add(carDt.Rows[i]["record"].ObjToString() + " - " + carDt.Rows[i]["location"].ObjToString() + " - " + carDt.Rows[i]["year"].ObjToString() + " - " + carDt.Rows[i]["make"].ObjToString() + " - " + carDt.Rows[i]["model"].ObjToString());
                repositoryItemComboBox10.Items.Add(carDt.Rows[i]["record"].ObjToString() + " - " + carDt.Rows[i]["location"].ObjToString() + " - " + carDt.Rows[i]["year"].ObjToString() + " - " + carDt.Rows[i]["make"].ObjToString() + " - " + carDt.Rows[i]["model"].ObjToString());
            }
            
            
        }
        /***********************************************************************************************/
        private void loadRepositoryVendors()
        {
            string cmd = "Select * from `cars_vendors` order by `record`;";
            DataTable vendorDt = G1.get_db_data(cmd);

            DataTable newVendorDt = vendorDt.Clone();

            DataView tempview = vendorDt.DefaultView;
            tempview.Sort = "record";
            vendorDt = tempview.ToTable();

            repositoryItemComboBox2.Items.Add("All");
            repositoryItemComboBox11.Items.Add("All");
            for (int i = 0; i < vendorDt.Rows.Count; i++)
            {
                repositoryItemComboBox2.Items.Add(vendorDt.Rows[i]["record"].ObjToString() + " - " + vendorDt.Rows[i]["name"].ObjToString());
                repositoryItemComboBox11.Items.Add(vendorDt.Rows[i]["record"].ObjToString() + " - " + vendorDt.Rows[i]["name"].ObjToString());
            }
        }
        /***********************************************************************************************/
        private void loadRepositoryServiceType()
        {
            string cmd = "Select * from `cars_service_type` order by `record`;";
            DataTable serviceDt = G1.get_db_data(cmd);

            DataTable newServiceDt = serviceDt.Clone();

            DataView tempview = serviceDt.DefaultView;
            tempview.Sort = "record";
            serviceDt = tempview.ToTable();

            repositoryItemComboBox3.Items.Add("All");
            repositoryItemComboBox12.Items.Add("All");
            for (int i = 0; i < serviceDt.Rows.Count; i++)
            {
                repositoryItemComboBox3.Items.Add(serviceDt.Rows[i]["record"].ObjToString() + " - " + serviceDt.Rows[i]["service_cat"].ObjToString());
                repositoryItemComboBox12.Items.Add(serviceDt.Rows[i]["record"].ObjToString() + " - " + serviceDt.Rows[i]["service_cat"].ObjToString());
            }
        }
        /***********************************************************************************************/
        private void loadRepositoryCategory()
        {
            string cmd = "Select * from `cars_maint_cat` order by `record`;";
            DataTable categoryDt = G1.get_db_data(cmd);

            DataTable newServiceDt = categoryDt.Clone();

            DataView tempview = categoryDt.DefaultView;
            tempview.Sort = "record";
            categoryDt = tempview.ToTable();

            repositoryItemComboBox4.Items.Add("All");
            repositoryItemComboBox13.Items.Add("All");
            for (int i = 0; i < categoryDt.Rows.Count; i++)
            {
                repositoryItemComboBox4.Items.Add(categoryDt.Rows[i]["record"].ObjToString() + " - " + categoryDt.Rows[i]["name"].ObjToString());
                repositoryItemComboBox13.Items.Add(categoryDt.Rows[i]["record"].ObjToString() + " - " + categoryDt.Rows[i]["name"].ObjToString());
            }
        }
        /***********************************************************************************************/
        private void loadRepositoryServCategory()
        {
            string cmd = "Select * from `cars_maint_cat` order by `record`;";
            DataTable categoryDt = G1.get_db_data(cmd);

            DataTable newServiceDt = categoryDt.Clone();

            DataView tempview = categoryDt.DefaultView;
            tempview.Sort = "record";
            categoryDt = tempview.ToTable();

            repositoryItemComboBox9.Items.Add("All");
            for (int i = 0; i < categoryDt.Rows.Count; i++)
                repositoryItemComboBox9.Items.Add(categoryDt.Rows[i]["record"].ObjToString() + " - " + categoryDt.Rows[i]["name"].ObjToString());
        }
        /***********************************************************************************************/
        private void pictureBox12_Click(object sender, EventArgs e)
        { // Add New Row
            DataTable dt = (DataTable) dgv.DataSource;
            DataRow dRow = dt.NewRow();
            dt.Rows.Add(dRow);
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            dgv.Refresh();
            gridMain.RefreshData();
            gridMain.RefreshEditor(true);
//            gridMain_CellValueChanged(null, null);
        }
        /****************************************************************************************/
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (loading)
                return;
            modified = true;
            btnSaveAll.Show();
            btnSaveAll.Refresh();
            DataRow dr = gridMain.GetFocusedDataRow();
            dr["mod"] = "Y";
        }
        /****************************************************************************************/
        private void gridMain2_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (loading)
                return;
            modified_maint = true;
            btnSaveAllMaint.Show();
            btnSaveAllMaint.Refresh();
            DataRow dr2 = gridMain2.GetFocusedDataRow();
            dr2["mod"] = "Y";
        }
        /****************************************************************************************/
        private void gridMain3_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (loading)
                return;
            modified_vendor = true;
            btnSaveAllVend.Show();
            btnSaveAllVend.Refresh();
            DataRow dr3 = gridMain3.GetFocusedDataRow();
            dr3["mod"] = "Y";
        }
        /****************************************************************************************/
        private void gridMain4_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (loading)
                return;
            modified_vendor = true;
            btnSaveAllService.Show();
            btnSaveAllService.Refresh();
            DataRow dr4 = gridMain4.GetFocusedDataRow();
            dr4["mod"] = "Y";
        }
        /****************************************************************************************/
        private void gridMain5_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (loading)
                return;
            modified_vendor = true;
            btnSaveAllUpcoming.Show();
            btnSaveAllUpcoming.Refresh();
            DataRow dr5 = gridMain5.GetFocusedDataRow();
            dr5["mod"] = "Y";
        }
        /****************************************************************************************/
        private void pictureBox11_Click(object sender, EventArgs e)
        { // Delete Current Row on Main Vehicles Tab
            DataRow dr = gridMain.GetFocusedDataRow();
            string data = "";
            data = dr["year"].ObjToString();
            data += " ";
            data += dr["make"].ObjToString();
            data += " ";
            data += dr["model"].ObjToString();
            DialogResult result = MessageBox.Show("***Question*** Are you sure you want to DELETE this Car (" + data + ") ?", "Delete Car Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt == null)
                return;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            dr["Mod"] = "D";
            dt.Rows[row]["mod"] = "D";
//            gridMain_CellValueChanged(null, null);
            if (loading)
                return;
            modified = true;
            btnSaveAll.Show();
            btnSaveAll.Refresh();
        }
        /****************************************************************************************/
        private void pictureBox13_Click(object sender, EventArgs e)
        { // Delete Current Row on Vendors Tab
            DataRow dr = gridMain3.GetFocusedDataRow();
            string data = dr["type"].ObjToString();
            DialogResult result = MessageBox.Show("***Question*** Are you sure you want to DELETE this Vendor (" + data + ") ?", "Delete Vendor Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;
            DataTable dt = (DataTable)dgv3.DataSource;
            if (dt == null)
                return;
            int rowHandle = gridMain3.FocusedRowHandle;
            int row = gridMain3.GetDataSourceRowIndex(rowHandle);
            dr["Mod"] = "D";
            dt.Rows[row]["mod"] = "D";
//            gridMain3_CellValueChanged(null, null);
            if (loading)
                return;
            modified = true;
            btnSaveAllVend.Show();
            btnSaveAllVend.Refresh();
        }
        /****************************************************************************************/
        private void gridMain_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt == null)
                return;
            string delete = dt.Rows[row]["mod"].ObjToString();
            if (delete.ToUpper() == "D")
            {
                e.Visible = false;
                e.Handled = true;
                return;
            }
        }
        /****************************************************************************************/
        private void btnSaveAll_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            string record = "";
            string mod = "";
            string location = "";
            string companyCode = "";
            string year = "";
            string make = "";
            string model = "";
            string color = "";
            string vin = "";
            string notes = "";
            string role = "";
            string county = "";
            string licensePlate = "";
            string expiration = "";
            string idNumber = "";

            string cmd = "DELETE from `cars` WHERE `model` = '-1'";
            G1.get_db_data(cmd);
            DataTable dx = G1.get_db_data ("select * from `cars`;");
            DataRow[] dRows = null;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                record = dt.Rows[i]["record"].ObjToString();
                location = dt.Rows[i]["location"].ObjToString();
                if (string.IsNullOrEmpty(location))
                {
                    continue;
                }
                mod = dt.Rows[i]["mod"].ObjToString();
                if (mod == "D")
                {
                    if (!String.IsNullOrWhiteSpace(record))
                        G1.delete_db_table("cars", "record", record);
                    continue;
                }
                if (mod != "Y")
                    continue;

                vin = dt.Rows[i]["vin"].ObjToString();
                dRows = dx.Select("vin = '" + vin + "'");
                if (dRows.Length > 0)
                    record = dRows[0]["record"].ObjToString();

                if ( String.IsNullOrWhiteSpace ( record ))
                    record = G1.create_record("cars", "model", "-1");
                if (G1.BadRecord("cars", record))
                    return;

                location = dt.Rows[i]["location"].ObjToString();
                companyCode = dt.Rows[i]["companyCode"].ObjToString();
                year = dt.Rows[i]["year"].ObjToString();
                make = dt.Rows[i]["make"].ObjToString();
                model = dt.Rows[i]["model"].ObjToString();
                color = dt.Rows[i]["color"].ObjToString();
                vin = dt.Rows[i]["vin"].ObjToString();
                notes = dt.Rows[i]["notes"].ObjToString();
                role = dt.Rows[i]["role"].ObjToString();
                idNumber = dt.Rows[i]["idNumber"].ObjToString();
                county = dt.Rows[i]["county"].ObjToString();
                expiration = dt.Rows[i]["expiration"].ObjToString();
                licensePlate = dt.Rows[i]["licensePlate"].ObjToString();
                G1.update_db_table("cars", "record", record, new string[] { "location", location, "companyCode", companyCode, "year", year, "make", make, "model", model, "color", color, "vin", vin, "notes", notes, "role", role, "idNumber", idNumber, "county", county, "expiration", expiration, "licensePlate", licensePlate });
            }
            modified = false;
            btnSaveAll.Hide();
        }
        /****************************************************************************************/
        private void btnSaveAllMaint_Click(object sender, EventArgs e)
        {
            DataTable dt2 = (DataTable)dgv2.DataSource;
            string record = "";
            string mod = "";

            string vehicle = ""; // Holds the entire string of the Vehicle drop down.
            string cars_record = ""; // Contains the record number of the car.
            string car = ""; // Contains the name of the car without the record number

            string vendor = "";
            string vendor_record = "";
            string vendor_name = "";

            string service_type = "";
            string service_type_record = "";
            string service = "";

            string category_field = ""; // Contains the data from the drop down.
            string category_record = ""; // record number
            string category = "";       // just the name of the category for the database.

            string beginDate = "";
            string endDate = "";
            string sched_b_date = "";
            string sched_e_date = "";
            string mileage = "";
            string cost = "";
            string notes = "";

            int indexNum = 0;

            string cmd = "DELETE from `cars_maint` WHERE `mileage` = '-1'";
            G1.get_db_data(cmd);
            DataTable dx2 = G1.get_db_data("select * from `cars_maint`;");
            DataRow[] dRows2 = null;

            for (int i = 0; i < dt2.Rows.Count; i++)
            {
                record = dt2.Rows[i]["record"].ObjToString();
                mod = dt2.Rows[i]["mod"].ObjToString();
                if (mod == "D")
                {
                    if (!String.IsNullOrWhiteSpace(record))
                        G1.delete_db_table("cars_maint", "record", record);
                    continue;
                }
                if (mod != "Y")
                    continue;

                // If there is no car record, then this is a new record and the number will be in the drop down. Else, it will be in its own column.
                vehicle = dt2.Rows[i]["car"].ObjToString();
                cars_record = dt2.Rows[i]["cars_record"].ObjToString();
                if (cars_record == "")
                {
                    indexNum = vehicle.IndexOf(" ");
                    cars_record = vehicle.Substring(0, indexNum);
                    car = vehicle.Substring(indexNum + 3);
                }
                else 
                {
                    car = vehicle;
                }

                // If there is no vendor record, then this is a new record and the number will be in the drop down. Else, it will be in its own column.
                vendor = dt2.Rows[i]["vendor_name"].ObjToString();
                vendor_record = dt2.Rows[i]["vendor_record"].ObjToString();
                if (vendor_record == "")
                {
                    indexNum = vendor.IndexOf(" ");
                    vendor_record = vendor.Substring(0, indexNum);
                    vendor_name = vendor.Substring(indexNum + 3);
                }
                else 
                {
                    vendor_name = vendor;
                }

                // If there is no service type record, then this is a new record and the number will be in the drop down. Else, it will be in its own column.
                service_type = dt2.Rows[i]["service"].ObjToString();
                service_type_record = dt2.Rows[i]["service_type_record"].ObjToString();
                if (service_type_record == "")
                {
                    indexNum = service_type.IndexOf(" ");
                    service_type_record = service_type.Substring(0, indexNum);
                    service = service_type.Substring(indexNum + 3);
                }
                else
                {
                    service = service_type;
                }

                // If there is no category record, then this is a new record and the number will be in the drop down. Else, it will be in its own column.
                category_field = dt2.Rows[i]["category"].ObjToString();
                category_record = dt2.Rows[i]["category_record"].ObjToString();
                if (category_record == "")
                {
                    indexNum = category_field.IndexOf(" ");
                    category_record = category_field.Substring(0, indexNum);
                    category = category_field.Substring(indexNum + 3);
                }
                else
                {
                    category = category_field;
                }

                beginDate = dt2.Rows[i]["service_begin_date"].ObjToString();
                endDate = dt2.Rows[i]["service_end_date"].ObjToString();

                sched_b_date = dt2.Rows[i]["service_sched_b_date"].ObjToString();
                sched_e_date = dt2.Rows[i]["service_sched_e_date"].ObjToString();
                
                mileage = dt2.Rows[i]["mileage"].ObjToString();
                cost = dt2.Rows[i]["cost"].ObjToString();
                notes = dt2.Rows[i]["notes"].ObjToString();

                if (String.IsNullOrWhiteSpace(record))
                    record = G1.create_record("cars_maint", "mileage", "0");
                if (G1.BadRecord("cars_maint", record))
                    return;
                
                G1.update_db_table("cars_maint", "record", record, new string[] { "cars_record", cars_record, "car", car, "service_type_record", service_type_record, "service", service, "category_record", category_record, "category", category, "service_sched_b_date", sched_b_date, "service_sched_e_date", sched_e_date, "service_begin_date", beginDate, "service_end_date", endDate, "mileage", mileage, "cost", cost, "vendor_record", vendor_record, "vendor_name", vendor_name, "notes", notes });
            }
            modified_maint = false;
            btnSaveAllMaint.Hide();

            // IF this is preventive maintenance, create a scheduled maintenance on the same vehicle with the same vendor based on the day frequency in cars_service_type
            if (category.ToUpper() == "PREVENTIVE" && endDate != "01/01/0001" && mod != "D")
            {
                // retrieve the frequency in days from cars_service_type
                string sql = "SELECT frequency_days, service_time_length, frequency_miles FROM cars_service_type WHERE record = " + service_type_record + ";";
                DataTable dx = G1.get_db_data(sql);
                if (dx.Rows.Count > 0)
                {
                    int frequency_days = dx.Rows[0]["frequency_days"].ObjToInt32();
                    int service_time_length = dx.Rows[0]["service_time_length"].ObjToInt32();
                    int frequency_miles = dx.Rows[0]["frequency_miles"].ObjToInt32();
                    DateTime schedule_begin_date = beginDate.ObjToDateTime();
                    
                    // increment end service date by frequency_days and store that in sched_b_date.
                    schedule_begin_date = schedule_begin_date.AddDays(frequency_days);
                    sched_b_date = schedule_begin_date.ObjToString();

                    // increment schedule start date by service_time_length and store it in sched_e_date.
                    DateTime schedule_end_date = schedule_begin_date.AddDays(service_time_length);
                    sched_e_date = schedule_end_date.ObjToString();

                    // Create a record# for the new maintenance record.
                    record = G1.create_record("cars_maint", "mileage", "0");
                    if (G1.BadRecord("cars_maint", record))
                        return;

                    // Clear the actual begin and end dates and notes so that those don't copy over into the new record.
                    beginDate = "";
                    endDate = "";
                    notes = "";

                    // Increase mileage so it isn't the same as last time.
                    int mileage_int = 0;
                    mileage_int = mileage.ObjToInt32();
                    mileage_int = mileage_int + frequency_miles;
                    mileage = mileage_int.ObjToString();

                    // Use the same data from before to create a new record with the new record number 
                    G1.update_db_table("cars_maint", "record", record, new string[] { "cars_record", cars_record, "car", car, "service_type_record", service_type_record, "service", service, "category_record", category_record, "category", category, "service_sched_b_date", sched_b_date, "service_sched_e_date", sched_e_date, "service_begin_date", beginDate, "service_end_date", endDate, "mileage", mileage, "cost", cost, "vendor_record", vendor_record, "vendor_name", vendor_name, "notes", notes });

                    // Refresh the table
                    dgv2.Refresh();
                }
            }
        }
        /****************************************************************************************/
        private void btnSaveAllVend_Click(object sender, EventArgs e)
        {
            DataTable dt3 = (DataTable)dgv3.DataSource;
            string record = "";
            string mod = "";
            string name = "";
            string type = "";
            string contact_name = "";
            string phone = "";
            string email = "";
            string mail_address = "";
            string physical_address = "";
            string city = "";
            string state = "";
            string zip = "";
            string active = "";
            string notes = "";

            string cmd = "DELETE from `cars_vendors` WHERE `type` = '-1'";
            G1.get_db_data(cmd);
            DataTable dx3 = G1.get_db_data("select * from `cars_vendors`;");
            DataRow[] dRows3 = null;

            for (int i = 0; i < dt3.Rows.Count; i++)
            {
                record = dt3.Rows[i]["record"].ObjToString();
                mod = dt3.Rows[i]["mod"].ObjToString();
                if (mod == "D")
                {
                    if (!String.IsNullOrWhiteSpace(record))
                        G1.delete_db_table("cars_vendors", "record", record);
                    continue;
                }
                if (mod != "Y")
                    continue;
                if (String.IsNullOrWhiteSpace(record))
                    record = G1.create_record("cars_vendors", "type", "-1");
                if (G1.BadRecord("cars_vendors", record))
                    return;

                name = dt3.Rows[i]["name"].ObjToString();
                type = dt3.Rows[i]["type"].ObjToString();
                contact_name = dt3.Rows[i]["contact_name"].ObjToString();
                phone = dt3.Rows[i]["phone"].ObjToString();
                email = dt3.Rows[i]["email"].ObjToString();
                mail_address = dt3.Rows[i]["mail_address"].ObjToString();
                physical_address = dt3.Rows[i]["physical_address"].ObjToString();
                city = dt3.Rows[i]["city"].ObjToString();
                state = dt3.Rows[i]["state"].ObjToString();
                zip = dt3.Rows[i]["zip"].ObjToString();
                active = dt3.Rows[i]["active"].ObjToString();
                notes = dt3.Rows[i]["notes"].ObjToString();
                G1.update_db_table("cars_vendors", "record", record, new string[] { "name", name, "type", type, "contact_name", contact_name, "phone", phone, "email", email, "mail_address", mail_address, "physical_address", physical_address, "city", city, "state", state, "zip", zip, "active", active, "notes", notes });
            }
            modified_maint = false;
            btnSaveAllVend.Hide();
        }
        /****************************************************************************************/
        private void btnSaveAllService_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv4.DataSource;
            string record = "";
            string mod = "";
            string service_cat = "";
            string service_name = "";
            string service_desc = "";
            string service_time_length = ""; // Length of time it takes for the service in hours.
            string category_field = ""; // Full text of the combo box.
            string cat_record = ""; // Record number to be taken from combo box and stored in cat_record
            string cat_desc = ""; // Description from combo box and stored in cat_desc
            string frequency_days = "";
            string frequency_miles = "";
            string active = "";

            int indexNum = 0;

            string cmd = "DELETE from `cars_service_type` WHERE `service_cat` = '-1'";
            G1.get_db_data(cmd);
            DataTable dx = G1.get_db_data("select * from `cars_service_type`;");
            DataRow[] dRows = null;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                record = dt.Rows[i]["record"].ObjToString();
                mod = dt.Rows[i]["mod"].ObjToString();
                if (mod == "D")
                {
                    if (!String.IsNullOrWhiteSpace(record))
                        G1.delete_db_table("cars_service_type", "record", record);
                    continue;
                }
                if (mod != "Y")
                    continue;
                
                service_name = dt.Rows[i]["service_name"].ObjToString();
                dRows = dx.Select("service_name = '" + service_name + "'");
                if (dRows.Length > 0)
                    record = dRows[0]["record"].ObjToString();

                if (String.IsNullOrWhiteSpace(record))
                    record = G1.create_record("cars_service_type", "service_time_length", "-1");
                if (G1.BadRecord("cars_service_type", record))
                    return;

                service_cat = dt.Rows[i]["service_cat"].ObjToString();
                service_desc = dt.Rows[i]["service_desc"].ObjToString();
                service_time_length = dt.Rows[i]["service_time_length"].ObjToString();

                category_field = dt.Rows[i]["cat_desc"].ObjToString();
                cat_record = dt.Rows[i]["cat_record"].ObjToString();
                // If there is no category record, then this is a new record and the number will be in the drop down. Else, it will be in its own column.
                if (cat_record == "")
                {
                    indexNum = category_field.IndexOf(" ");
                    cat_record = category_field.Substring(0, indexNum);
                    cat_desc = category_field.Substring(indexNum + 3);
                }
                else 
                {
                    cat_desc = category_field;
                }

                frequency_days = dt.Rows[i]["frequency_days"].ObjToString();
                frequency_miles = dt.Rows[i]["frequency_miles"].ObjToString();
                active = dt.Rows[i]["active"].ObjToString();
                G1.update_db_table("cars_service_type", "record", record, new string[] { "service_cat", service_cat, "service_name", service_name, "service_desc", service_desc, "service_time_length", service_time_length, "cat_record", cat_record, "cat_desc", cat_desc, "frequency_days", frequency_days, "frequency_miles", frequency_miles, "active", active });
            }
            modified = false;
            btnSaveAllService.Hide();
        }
        /****************************************************************************************/
        private void picRowUp_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt.Rows.Count <= 0)
                return;
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowHandle = gridMain.FocusedRowHandle;
            if (rowHandle == 0)
                return; // Already at the first row
            //MoveRowUp(dt, rowHandle);
            massRowsUp(dt, rowHandle);
            dt.AcceptChanges();
            dgv.DataSource = dt;
            gridMain.ClearSelection();
            gridMain.SelectRow(rowHandle - 1);
            gridMain.FocusedRowHandle = rowHandle - 1;
            gridMain.RefreshData();
            dgv.Refresh();
            gridMain_CellValueChanged(null, null);
        }
        /***********************************************************************************************/
        private void picRowUpMaint_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv2.DataSource;
            if (dt.Rows.Count <= 0)
                return;
            DataRow dr = gridMain2.GetFocusedDataRow();
            int rowHandle = gridMain2.FocusedRowHandle;
            if (rowHandle == 0)
                return; // Already at the first row
            //MoveRowUp(dt, rowHandle);
            massRowsUp(dt, rowHandle);
            dt.AcceptChanges();
            dgv2.DataSource = dt;
            gridMain2.ClearSelection();
            gridMain2.SelectRow(rowHandle - 1);
            gridMain2.FocusedRowHandle = rowHandle - 1;
            gridMain2.RefreshData();
            dgv2.Refresh();
            gridMain2_CellValueChanged(null, null);
        }

        /***********************************************************************************************/
        private void picRowUpVend_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv3.DataSource;
            if (dt.Rows.Count <= 0)
                return;
            DataRow dr = gridMain3.GetFocusedDataRow();
            int rowHandle = gridMain3.FocusedRowHandle;
            if (rowHandle == 0)
                return; // Already at the first row
            //MoveRowUp(dt, rowHandle);
            massRowsUp3(dt, rowHandle);
            dt.AcceptChanges();
            dgv3.DataSource = dt;
            gridMain3.ClearSelection();
            gridMain3.SelectRow(rowHandle - 1);
            gridMain3.FocusedRowHandle = rowHandle - 1;
            gridMain3.RefreshData();
            dgv3.Refresh();
            gridMain3_CellValueChanged(null, null);
        }
        /***********************************************************************************************/
        private void picRowUpServ_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt.Rows.Count <= 0)
                return;
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowHandle = gridMain.FocusedRowHandle;
            if (rowHandle == 0)
                return; // Already at the first row
            //MoveRowUp(dt, rowHandle);
            massRowsUp(dt, rowHandle);
            dt.AcceptChanges();
            dgv.DataSource = dt;
            gridMain.ClearSelection();
            gridMain.SelectRow(rowHandle - 1);
            gridMain.FocusedRowHandle = rowHandle - 1;
            gridMain.RefreshData();
            dgv.Refresh();
            gridMain_CellValueChanged(null, null);
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
                    dt.Rows[row]["mod"] = "M";
                    modified = true;
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
        /***********************************************************************************************/
        private void massRowsUp2(DataTable dt, int row)
        {
            int[] rows = gridMain2.GetSelectedRows();
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
                    dt.Rows[row]["mod"] = "M";
                    modified_maint = true;
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
            gridMain2.SelectRow(firstRow);
            dgv2.DataSource = dt;
        }
        /****************************************************************************************/
        private void massRowsUp3(DataTable dt, int row)
        {
            int[] rows = gridMain3.GetSelectedRows();
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
                    dt.Rows[row]["mod"] = "M";
                    modified_vendor = true;
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

            gridMain3.SelectRow(firstRow);
            dgv3.DataSource = dt;
        }
        /****************************************************************************************/
        private void massRowsUp4(DataTable dt, int row)
        {
            int[] rows = gridMain4.GetSelectedRows();
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
                    dt.Rows[row]["mod"] = "M";
                    modified_vendor = true;
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

            gridMain4.SelectRow(firstRow);
            dgv4.DataSource = dt;
        }
        /****************************************************************************************/
        private void massRowsUp5(DataTable dt, int row)
        {
            int[] rows = gridMain5.GetSelectedRows();
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
                    dt.Rows[row]["mod"] = "M";
                    modified_vendor = true;
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

            gridMain5.SelectRow(firstRow);
            dgv5.DataSource = dt;
        }
        /****************************************************************************************/
        private void picRowDown_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt.Rows.Count <= 0)
                return;
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowHandle = gridMain.FocusedRowHandle;
            if (rowHandle == (dt.Rows.Count - 1))
                return; // Already at the last row
            MoveRowDown(dt, rowHandle);
            dt.AcceptChanges();
            dgv.DataSource = dt;
            gridMain.ClearSelection();
            gridMain.SelectRow(rowHandle + 1);
            gridMain.FocusedRowHandle = rowHandle + 1;
            gridMain.RefreshData();
            dgv.Refresh();
            gridMain_CellValueChanged(null, null);
        }
        /***************************************************************************************/
        private void MoveRowDown(DataTable dt, int row)
        {
            dt.Columns.Add("Count", Type.GetType("System.Int32"));
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["Count"] = i.ToString();
            dt.Rows[row]["Count"] = (row + 1).ToString();
            dt.Rows[row + 1]["Count"] = row.ToString();
            G1.sortTable(dt, "Count", "asc");
            dt.Columns.Remove("Count");
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["Num"] = (i + 1).ToString();
        }
        /***************************************************************************************/
        private void MoveRowDown2(DataTable dt, int row)
        {
            dt.Columns.Add("Count", Type.GetType("System.Int32"));
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["Count"] = i.ToString();
            dt.Rows[row]["Count"] = (row + 1).ToString();
            dt.Rows[row + 1]["Count"] = row.ToString();
            G1.sortTable(dt, "Count", "asc");
            dt.Columns.Remove("Count");
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["Num"] = (i + 1).ToString();
        }
        /***********************************************************************************************/
        private void MoveRowDown3(DataTable dt, int row)
        {
            dt.Columns.Add("Count", Type.GetType("System.Int32"));
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["Count"] = i.ToString();
            dt.Rows[row]["Count"] = (row + 1).ToString();
            dt.Rows[row + 1]["Count"] = row.ToString();
            G1.sortTable(dt, "Count", "asc");
            dt.Columns.Remove("Count");
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["Num"] = (i + 1).ToString();
        }
        /***********************************************************************************************/
        private void MoveRowDown4(DataTable dt, int row)
        {
            dt.Columns.Add("Count", Type.GetType("System.Int32"));
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["Count"] = i.ToString();
            dt.Rows[row]["Count"] = (row + 1).ToString();
            dt.Rows[row + 1]["Count"] = row.ToString();
            G1.sortTable(dt, "Count", "asc");
            dt.Columns.Remove("Count");
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["Num"] = (i + 1).ToString();
        }
        /***********************************************************************************************/
        private void MoveRowDown5(DataTable dt, int row)
        {
            dt.Columns.Add("Count", Type.GetType("System.Int32"));
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["Count"] = i.ToString();
            dt.Rows[row]["Count"] = (row + 1).ToString();
            dt.Rows[row + 1]["Count"] = row.ToString();
            G1.sortTable(dt, "Count", "asc");
            dt.Columns.Remove("Count");
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["Num"] = (i + 1).ToString();
        }
        /***********************************************************************************************/
        private void btnInsert_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt.Rows.Count <= 0)
                return;
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowHandle = gridMain.FocusedRowHandle;
            int dtRow = gridMain.GetDataSourceRowIndex(rowHandle);
            if (dtRow < 0 || dtRow > (dt.Rows.Count - 1))
                return;
            //if (rowHandle == (dt.Rows.Count - 1))
            //    return; // Already at the last row
            DataRow dRow = dt.NewRow();
            dt.Rows.InsertAt(dRow, dtRow);
            G1.NumberDataTable(dt);
            dt.AcceptChanges();
            dgv.DataSource = dt;
            gridMain.ClearSelection();
            gridMain.RefreshData();
            gridMain.FocusedRowHandle = rowHandle + 1;
            gridMain.SelectRow(rowHandle + 1);
            dgv.Refresh();
            gridMain_CellValueChanged(null, null);
        }
        /****************************************************************************************/
        private void btnInsertVend_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv3.DataSource;
            if (dt.Rows.Count <= 0)
                return;
            DataRow dr = gridMain3.GetFocusedDataRow();
            int rowHandle = gridMain3.FocusedRowHandle;
            int dtRow = gridMain3.GetDataSourceRowIndex(rowHandle);
            if (dtRow < 0 || dtRow > (dt.Rows.Count - 1))
                return;
            //if (rowHandle == (dt.Rows.Count - 1))
            //    return; // Already at the last row
            DataRow dRow = dt.NewRow();
            dt.Rows.InsertAt(dRow, dtRow);
            G1.NumberDataTable(dt);
            dt.AcceptChanges();
            dgv3.DataSource = dt;
            gridMain3.ClearSelection();
            gridMain3.RefreshData();
            gridMain3.FocusedRowHandle = rowHandle + 1;
            gridMain3.SelectRow(rowHandle + 1);
            dgv3.Refresh();
            gridMain3_CellValueChanged(null, null);
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
        private void EditTable_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!btnSaveAll.Visible)
                return;
            if (!btnSaveAllMaint.Visible)
                return;
            if (!btnSaveAllVend.Visible)
                return;
            if (!btnSaveAllService.Visible)
                return;
            DialogResult result = MessageBox.Show("***Question*** Data has been modified.\nDo you really want to exit WITHOUT saving your data?", "Data Modified Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
                return;
            e.Cancel = true;
        }
        /****************************************************************************************/
        private void gridMain_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column.FieldName.ToUpper() == "YEAR" && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                string year = e.DisplayText;
                year = year.Replace(",", "");
                e.DisplayText = year;
            }
        }
        /****************************************************************************************/
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

            Printer.setupPrinterMargins(10, 5, 80, 50);

            pageMarginLeft = Printer.pageMarginLeft;
            pageMarginRight = Printer.pageMarginRight;
            pageMarginTop = Printer.pageMarginTop;
            pageMarginBottom = Printer.pageMarginBottom;

            printableComponentLink1.Margins.Left = pageMarginLeft;
            printableComponentLink1.Margins.Right = pageMarginRight;
            printableComponentLink1.Margins.Top = pageMarginTop;
            printableComponentLink1.Margins.Bottom = pageMarginBottom;

            printingSystem1.Document.AutoFitToPagesWidth = 1;

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
            if (dgv2.Visible)
                printableComponentLink1.Component = dgv2;
            else if (dgv3.Visible)
                printableComponentLink1.Component = dgv3;
            else if (dgv4.Visible)
                printableComponentLink1.Component = dgv4;
            else if (dgv5.Visible)
                printableComponentLink1.Component = dgv5;
            
            printableComponentLink1.PrintingSystemBase = printingSystem1;
            printableComponentLink1.CreateDetailHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateDetailHeaderArea);
            printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);
            printableComponentLink1.BeforeCreateAreas += new System.EventHandler(this.printableComponentLink1_BeforeCreateAreas);
            printableComponentLink1.AfterCreateAreas += new System.EventHandler(this.printableComponentLink1_AfterCreateAreas);
            printableComponentLink1.Landscape = true;

            Printer.setupPrinterMargins(10, 5, 80, 50);

            pageMarginLeft = Printer.pageMarginLeft;
            pageMarginRight = Printer.pageMarginRight;
            pageMarginTop = Printer.pageMarginTop;
            pageMarginBottom = Printer.pageMarginBottom;

            printableComponentLink1.Margins.Left = pageMarginLeft;
            printableComponentLink1.Margins.Right = pageMarginRight;
            printableComponentLink1.Margins.Top = pageMarginTop;
            printableComponentLink1.Margins.Bottom = pageMarginBottom;

            printingSystem1.Document.AutoFitToPagesWidth = 1;

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

            //            Printer.DrawQuad(1, 9, 2, 3, "User : " + LoginForm.username, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);

            font = new Font("Ariel", 10, FontStyle.Regular);
            string title = this.Text;
            //if (workReport == "ACH Detail Report")
            //    title = "The First Drafts";
            //else
            //    title = "The First (All)";
            Printer.DrawQuad(6, 7, 4, 3, title, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            Printer.DrawQuad(1, 9, 2, 3, "User : " + LoginForm.username, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);


            //DateTime date = this.dateTimePicker1.Value;
            //string workDate = date.Month.ToString("D2") + "/" + date.Year.ToString("D4");
            //Printer.SetQuadSize(24, 12);
            //font = new Font("Ariel", 9, FontStyle.Regular);
            //Printer.DrawQuad(20, 8, 5, 4, "Month Closing - " + workDate, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            //Printer.DrawQuad(16, 8, 3, 4, lblPayment.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(19, 8, 3, 4, lblTrust85.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(22, 8, 3, 4, lblTrust100.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /****************************************************************************************/
        string importedFile = "";
        string actualFile = "";

        private void importToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            DataTable workDt = null;
            string sheetName = "";
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string file = ofd.FileName;
                    importedFile = file;
                    int idx = file.LastIndexOf("\\");
                    if (idx > 0)
                    {
                        actualFile = file.Substring(idx);
                        actualFile = actualFile.Replace("\\", "");
                    }
                         
                    dgv.DataSource = null;
                    this.Cursor = Cursors.WaitCursor;
                    
                    try
                    {
                        workDt = ExcelWriter.ReadFile2(file, 0, sheetName);
                        if (workDt == null)
                            return;
                        if (workDt.Rows.Count < 2)
                            return;
                        workDt.TableName = actualFile;

                        workDt.Columns.Add("mod");
                        workDt.Columns.Add("record");

                        for (int col = 0; col < workDt.Columns.Count; col++)
                        {
                            string name = workDt.Rows[0][col].ObjToString();
                            if (string.IsNullOrEmpty(name))
                                continue;

                            workDt.Columns[col].ColumnName = name;
                        }

                        workDt.Rows.RemoveAt(0);
                        for (int i = 0; i < workDt.Rows.Count; i++)
                        {
                            workDt.Rows[i]["mod"] = "Y";
                        }

                        G1.NumberDataTable(workDt);
                        dgv.DataSource = workDt;
                        dgv.Refresh();

                        btnSaveAll.Show();
                        btnSaveAll.Refresh();
                        this.Cursor = Cursors.Default;
                        
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
        }
        private string oldWhat = "";
        /***********************************************************************************************/
        private void gridMain_ValidatingEditor(object sender, DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventArgs e)
        {
            GridView view = sender as GridView;
            if (view.FocusedColumn.FieldName.ToUpper() == "EXPIRATION")
            {
                DataTable dt = (DataTable)dgv.DataSource;
                DataRow dr = gridMain.GetFocusedDataRow();
                int rowhandle = gridMain.FocusedRowHandle;
                int row = gridMain.GetDataSourceRowIndex(rowhandle);
                oldWhat = e.Value.ObjToString();
                DateTime date = oldWhat.ObjToDateTime();
                dt.Rows[row]["expiration"] = G1.DTtoMySQLDT(date);
                e.Value = G1.DTtoMySQLDT(date);
            }
            /* -- Purchase Date
            else if (view.FocusedColumn.FieldName.ToUpper() == "EFFECTIVETODATE")
            {
                DataTable dt = (DataTable)dgv.DataSource;
                DataRow dr = gridMain.GetFocusedDataRow();
                int rowhandle = gridMain.FocusedRowHandle;
                int row = gridMain.GetDataSourceRowIndex(rowhandle);
                oldWhat = e.Value.ObjToString();
                DateTime date = oldWhat.ObjToDateTime();
                dt.Rows[row]["effectiveToDate"] = G1.DTtoMySQLDT(date);
                e.Value = G1.DTtoMySQLDT(date);
            }
            */
        }
        /***********************************************************************************************/
        private void gridMain_MouseDown(object sender, MouseEventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource; // Leave as a GetDate example

            var hitInfo = gridMain.CalcHitInfo(e.Location);
            if (hitInfo.InRowCell)
            {
                int rowHandle = hitInfo.RowHandle;
                gridMain.FocusedRowHandle = rowHandle;
                gridMain.SelectRow(rowHandle);
                gridMain.RefreshEditor(true);
                GridColumn column = hitInfo.Column;
                gridMain.FocusedColumn = column;
                string currentColumn = column.FieldName.Trim();
                if (currentColumn.ToUpper() == "EXPIRATION")
                {
                    DataRow dr = gridMain.GetFocusedDataRow();
                    DateTime date = dr["expiration"].ObjToDateTime();
                    if (date.Year < 1000)
                        date = DateTime.Now;
                    using (GetDate dateForm = new GetDate(date, "Expiration Date"))
                    {
                        dateForm.TopMost = true;
                        dateForm.ShowDialog();
                        if (dateForm.DialogResult == System.Windows.Forms.DialogResult.OK)
                        {
                            date = dateForm.myDateAnswer;
                            dr["expiration"] = G1.DTtoMySQLDT(date);
                            DataChanged();
                            dr["mod"] = "Y";
                            gridMain.ClearSelection();
                            gridMain.FocusedRowHandle = rowHandle;

                            gridMain.RefreshData();
                            gridMain.RefreshEditor(true);
                            gridMain.SelectRow(rowHandle);
                        }
                    }
                }
                /*
                else if (currentColumn.ToUpper() == "EFFECTIVETODATE")
                {
                    DataRow dr = gridMain.GetFocusedDataRow();
                    DateTime date = dr["effectiveToDate"].ObjToDateTime();
                    if (date.Year < 1000)
                        date = DateTime.Now;
                    using (GetDate dateForm = new GetDate(date, "Effective To Date"))
                    {
                        dateForm.TopMost = true;
                        dateForm.ShowDialog();
                        if (dateForm.DialogResult == System.Windows.Forms.DialogResult.OK)
                        {
                            date = dateForm.myDateAnswer;
                            dr["effectiveToDate"] = G1.DTtoMySQLDT(date);
                            DataChanged();
                            dr["mod"] = "Y";        
                            gridMain.ClearSelection();
                            gridMain.FocusedRowHandle = rowHandle;

                            gridMain.RefreshData();
                            gridMain.RefreshEditor(true);
                            gridMain.SelectRow(rowHandle);
                        }
                    }
                }
                */
            }
        }
        /***********************************************************************************************/
        private void gridMain3_MouseDown(object sender, MouseEventArgs e)
        {
            DataTable dt = (DataTable)dgv3.DataSource; // Leave as a GetDate example

            var hitInfo = gridMain3.CalcHitInfo(e.Location);
            if (hitInfo.InRowCell)
            {
                int rowHandle = hitInfo.RowHandle;
                gridMain3.FocusedRowHandle = rowHandle;
                gridMain3.SelectRow(rowHandle);
                gridMain3.RefreshEditor(true);
                GridColumn column = hitInfo.Column;
                gridMain3.FocusedColumn = column;
                string currentColumn = column.FieldName.Trim();
            }
        }
        /***********************************************************************************************/
        private void DataChanged()
        {
            if (loading)
                return;

            btnSaveAll.Show();
            btnSaveAll.Refresh();

            int rowHandle = gridMain.FocusedRowHandle;
            DataRow dr = gridMain.GetFocusedDataRow();
            if (dr == null)
                return;
            dr["mod"] = "Y";
        }
        /***********************************************************************************************/
        private void MaintDataChanged()
        {
            if (loading)
                return;

            btnSaveAllMaint.Show();
            btnSaveAllMaint.Refresh();

            int rowHandle = gridMain2.FocusedRowHandle;
            DataRow dr = gridMain2.GetFocusedDataRow();
            if (dr == null)
                return;
            dr["mod"] = "Y";
        }
        /***********************************************************************************************/
        private void UpcomingDataChanged()
        {
            if (loading)
                return;

            btnSaveAllUpcoming.Show();
            btnSaveAllUpcoming.Refresh();

            int rowHandle = gridMain5.FocusedRowHandle;
            DataRow dr = gridMain5.GetFocusedDataRow();
            if (dr == null)
                return;
            dr["mod"] = "Y";
        }
        /***********************************************************************************************/
        private void VendorDataChanged()
        {
            if (loading)
                return;

            btnSaveAllVend.Show();
            btnSaveAllVend.Refresh();

            int rowHandle = gridMain3.FocusedRowHandle;
            DataRow dr = gridMain3.GetFocusedDataRow();
            if (dr == null)
                return;
            dr["mod"] = "Y";
        }
        /***********************************************************************************************/
        private void ServiceDataChanged()
        {
            if (loading)
                return;

            btnSaveAllService.Show();
            btnSaveAllService.Refresh();

            int rowHandle = gridMain4.FocusedRowHandle;
            DataRow dr = gridMain4.GetFocusedDataRow();
            if (dr == null)
                return;
            dr["mod"] = "Y";
        }
        /***********************************************************************************************/
        private void gridMain2_MouseDown(object sender, MouseEventArgs e)
        {
            DataTable dt = (DataTable)dgv2.DataSource; // Leave as a GetDate example

            var hitInfo = gridMain2.CalcHitInfo(e.Location);
            if (hitInfo.InRowCell)
            {
                int rowHandle = hitInfo.RowHandle;
                gridMain2.FocusedRowHandle = rowHandle;
                gridMain2.SelectRow(rowHandle);
                gridMain2.RefreshEditor(true);
                GridColumn column = hitInfo.Column;
                gridMain2.FocusedColumn = column;
                string currentColumn = column.FieldName.Trim();
                if (currentColumn.ToUpper() == "SERVICE_BEGIN_DATE")
                {
                    DataRow dr = gridMain2.GetFocusedDataRow();
                    DateTime date = dr["service_begin_date"].ObjToDateTime();
                    if (date.Year < 1000)
                        date = DateTime.Now;
                    using (GetDate dateForm = new GetDate(date, "Begin Date"))
                    {
                        dateForm.TopMost = true;
                        dateForm.ShowDialog();
                        if (dateForm.DialogResult == System.Windows.Forms.DialogResult.OK)
                        {
                            date = dateForm.myDateAnswer;
                            try
                            {
                                string str = date.ToString("MM/dd/yyyy");
                                dr["service_begin_date"] = G1.DTtoMySQLDT(str);

                            }
                            catch (Exception ex)
                            {
                            }

                            MaintDataChanged();
                            dr["mod"] = "Y";
                            gridMain2.ClearSelection();
                            gridMain2.FocusedRowHandle = rowHandle;

                            gridMain2.RefreshData();
                            gridMain2.RefreshEditor(true);
                            gridMain2.SelectRow(rowHandle);
                        }
                    }
                }
                else if (currentColumn.ToUpper() == "SERVICE_END_DATE")
                {
                    DataRow dr = gridMain2.GetFocusedDataRow();
                    DateTime date = dr["service_end_date"].ObjToDateTime();
                    if (date.Year < 1000)
                        date = DateTime.Now;
                    using (GetDate dateForm = new GetDate(date, "End Date"))
                    {
                        dateForm.TopMost = true;
                        dateForm.ShowDialog();
                        if (dateForm.DialogResult == System.Windows.Forms.DialogResult.OK)
                        {
                            date = dateForm.myDateAnswer;
                            dr["service_end_date"] = G1.DTtoMySQLDT(date);

                            // Check to see if service end date is earlier than service begin date. If it is, notify the user and clear the field. Make the user select a different date.

                            MaintDataChanged();
                            dr["mod"] = "Y";
                            gridMain2.ClearSelection();
                            gridMain2.FocusedRowHandle = rowHandle;

                            gridMain2.RefreshData();
                            gridMain2.RefreshEditor(true);
                            gridMain2.SelectRow(rowHandle);
                        }
                    }
                }
                else if (currentColumn.ToUpper() == "CARS_RECORD")
                {
                    DataRow dr = gridMain2.GetFocusedDataRow();
                    DateTime date = dr["cars_record"].ObjToDateTime();
                    if (date.Year < 1000)
                        date = DateTime.Now;
                    DataGridViewComboBoxColumn comboBoxColumn = new DataGridViewComboBoxColumn();
                    comboBoxColumn.HeaderText = "Vehicle"; // Set the desired header text
                    comboBoxColumn.Name = "Vehicle"; // Set a unique name for the column
                    /*
                    using (GetDate dateForm = new GetDate(date, "End Date"))
                    {
                        dateForm.TopMost = true;
                        dateForm.ShowDialog();
                        if (dateForm.DialogResult == System.Windows.Forms.DialogResult.OK)
                        {
                            date = dateForm.myDateAnswer;
                            dr["service_end_date"] = G1.DTtoMySQLDT(date);
                            MaintDataChanged();
                            dr["mod"] = "Y";
                            gridMain2.ClearSelection();
                            gridMain2.FocusedRowHandle = rowHandle;

                            gridMain2.RefreshData();
                            gridMain2.RefreshEditor(true);
                            gridMain2.SelectRow(rowHandle);
                        }
                    }
                    */
                }
                else if (currentColumn.ToUpper() == "SERVICE_SCHED_B_DATE")
                {
                    DataRow dr = gridMain2.GetFocusedDataRow();
                    DateTime date = dr["service_sched_b_date"].ObjToDateTime();
                    if (date.Year < 1000)
                        date = DateTime.Now;
                    using (GetDate dateForm = new GetDate(date, "Scheduled Begin Date"))
                    {
                        dateForm.TopMost = true;
                        dateForm.ShowDialog();
                        if (dateForm.DialogResult == System.Windows.Forms.DialogResult.OK)
                        {
                            date = dateForm.myDateAnswer;
                            try
                            {
                                string str = date.ToString("MM/dd/yyyy");
                                dr["service_sched_b_date"] = G1.DTtoMySQLDT(str);

                            }
                            catch (Exception ex)
                            {
                            }

                            MaintDataChanged();
                            dr["mod"] = "Y";
                            gridMain2.ClearSelection();
                            gridMain2.FocusedRowHandle = rowHandle;

                            gridMain2.RefreshData();
                            gridMain2.RefreshEditor(true);
                            gridMain2.SelectRow(rowHandle);
                        }
                    }
                }
                else if (currentColumn.ToUpper() == "SERVICE_SCHED_E_DATE")
                {
                    DataRow dr = gridMain2.GetFocusedDataRow();
                    DateTime date = dr["service_sched_e_date"].ObjToDateTime();
                    if (date.Year < 1000)
                        date = DateTime.Now;
                    using (GetDate dateForm = new GetDate(date, "Scheduled End Date"))
                    {
                        dateForm.TopMost = true;
                        dateForm.ShowDialog();
                        if (dateForm.DialogResult == System.Windows.Forms.DialogResult.OK)
                        {
                            date = dateForm.myDateAnswer;
                            try
                            {
                                string str = date.ToString("MM/dd/yyyy");
                                dr["service_sched_e_date"] = G1.DTtoMySQLDT(str);

                            }
                            catch (Exception ex)
                            {
                            }

                            MaintDataChanged();
                            dr["mod"] = "Y";
                            gridMain2.ClearSelection();
                            gridMain2.FocusedRowHandle = rowHandle;

                            gridMain2.RefreshData();
                            gridMain2.RefreshEditor(true);
                            gridMain2.SelectRow(rowHandle);
                        }
                    }
                }
            }
        }
        /****************************************************************************************/
        private void gridMain5_MouseDown(object sender, MouseEventArgs e)
        {
            DataTable dt = (DataTable)dgv5.DataSource; // Leave as a GetDate example

            var hitInfo = gridMain5.CalcHitInfo(e.Location);
            if (hitInfo.InRowCell)
            {
                int rowHandle = hitInfo.RowHandle;
                gridMain5.FocusedRowHandle = rowHandle;
                gridMain5.SelectRow(rowHandle);
                gridMain5.RefreshEditor(true);
                GridColumn column = hitInfo.Column;
                gridMain5.FocusedColumn = column;
                string currentColumn = column.FieldName.Trim();
                if (currentColumn.ToUpper() == "SERVICE_BEGIN_DATE")
                {
                    DataRow dr = gridMain5.GetFocusedDataRow();
                    DateTime date = dr["service_begin_date"].ObjToDateTime();
                    if (date.Year < 1000)
                        date = DateTime.Now;
                    using (GetDate dateForm = new GetDate(date, "Begin Date"))
                    {
                        dateForm.TopMost = true;
                        dateForm.ShowDialog();
                        if (dateForm.DialogResult == System.Windows.Forms.DialogResult.OK)
                        {
                            date = dateForm.myDateAnswer;
                            try
                            {
                                string str = date.ToString("MM/dd/yyyy");
                                dr["service_begin_date"] = G1.DTtoMySQLDT(str);

                            }
                            catch (Exception ex)
                            {
                            }

                            UpcomingDataChanged();
                            dr["mod"] = "Y";
                            gridMain5.ClearSelection();
                            gridMain5.FocusedRowHandle = rowHandle;

                            gridMain5.RefreshData();
                            gridMain5.RefreshEditor(true);
                            gridMain5.SelectRow(rowHandle);
                        }
                    }
                }
                else if (currentColumn.ToUpper() == "SERVICE_END_DATE")
                {
                    DataRow dr = gridMain5.GetFocusedDataRow();
                    DateTime date = dr["service_end_date"].ObjToDateTime();
                    if (date.Year < 1000)
                        date = DateTime.Now;
                    using (GetDate dateForm = new GetDate(date, "End Date"))
                    {
                        dateForm.TopMost = true;
                        dateForm.ShowDialog();
                        if (dateForm.DialogResult == System.Windows.Forms.DialogResult.OK)
                        {
                            date = dateForm.myDateAnswer;
                            dr["service_end_date"] = G1.DTtoMySQLDT(date);

                            // Check to see if service end date is earlier than service begin date. If it is, notify the user and clear the field. Make the user select a different date.

                            UpcomingDataChanged();
                            dr["mod"] = "Y";
                            gridMain2.ClearSelection();
                            gridMain2.FocusedRowHandle = rowHandle;

                            gridMain5.RefreshData();
                            gridMain5.RefreshEditor(true);
                            gridMain5.SelectRow(rowHandle);
                        }
                    }
                }
                else if (currentColumn.ToUpper() == "CARS_RECORD")
                {
                    DataRow dr = gridMain5.GetFocusedDataRow();
                    DateTime date = dr["cars_record"].ObjToDateTime();
                    if (date.Year < 1000)
                        date = DateTime.Now;
                    DataGridViewComboBoxColumn comboBoxColumn = new DataGridViewComboBoxColumn();
                    comboBoxColumn.HeaderText = "Vehicle"; // Set the desired header text
                    comboBoxColumn.Name = "Vehicle"; // Set a unique name for the column
                    /*
                    using (GetDate dateForm = new GetDate(date, "End Date"))
                    {
                        dateForm.TopMost = true;
                        dateForm.ShowDialog();
                        if (dateForm.DialogResult == System.Windows.Forms.DialogResult.OK)
                        {
                            date = dateForm.myDateAnswer;
                            dr["service_end_date"] = G1.DTtoMySQLDT(date);
                            MaintDataChanged();
                            dr["mod"] = "Y";
                            gridMain2.ClearSelection();
                            gridMain2.FocusedRowHandle = rowHandle;

                            gridMain2.RefreshData();
                            gridMain2.RefreshEditor(true);
                            gridMain2.SelectRow(rowHandle);
                        }
                    }
                    */
                }
                else if (currentColumn.ToUpper() == "SERVICE_SCHED_B_DATE")
                {
                    DataRow dr = gridMain5.GetFocusedDataRow();
                    DateTime date = dr["service_sched_b_date"].ObjToDateTime();
                    if (date.Year < 1000)
                        date = DateTime.Now;
                    using (GetDate dateForm = new GetDate(date, "Scheduled Begin Date"))
                    {
                        dateForm.TopMost = true;
                        dateForm.ShowDialog();
                        if (dateForm.DialogResult == System.Windows.Forms.DialogResult.OK)
                        {
                            date = dateForm.myDateAnswer;
                            try
                            {
                                string str = date.ToString("MM/dd/yyyy");
                                dr["service_sched_b_date"] = G1.DTtoMySQLDT(str);

                            }
                            catch (Exception ex)
                            {
                            }

                            UpcomingDataChanged();
                            dr["mod"] = "Y";
                            gridMain5.ClearSelection();
                            gridMain5.FocusedRowHandle = rowHandle;

                            gridMain5.RefreshData();
                            gridMain5.RefreshEditor(true);
                            gridMain5.SelectRow(rowHandle);
                        }
                    }
                }
                else if (currentColumn.ToUpper() == "SERVICE_SCHED_E_DATE")
                {
                    DataRow dr = gridMain5.GetFocusedDataRow();
                    DateTime date = dr["service_sched_e_date"].ObjToDateTime();
                    if (date.Year < 1000)
                        date = DateTime.Now;
                    using (GetDate dateForm = new GetDate(date, "Scheduled End Date"))
                    {
                        dateForm.TopMost = true;
                        dateForm.ShowDialog();
                        if (dateForm.DialogResult == System.Windows.Forms.DialogResult.OK)
                        {
                            date = dateForm.myDateAnswer;
                            try
                            {
                                string str = date.ToString("MM/dd/yyyy");
                                dr["service_sched_e_date"] = G1.DTtoMySQLDT(str);

                            }
                            catch (Exception ex)
                            {
                            }

                            UpcomingDataChanged();
                            dr["mod"] = "Y";
                            gridMain5.ClearSelection();
                            gridMain5.FocusedRowHandle = rowHandle;

                            gridMain5.RefreshData();
                            gridMain5.RefreshEditor(true);
                            gridMain5.SelectRow(rowHandle);
                        }
                    }
                }
            }
        }
        /****************************************************************************************/
        private void pictureBox5_Click(object sender, EventArgs e)
        {
            DataTable dt2 = (DataTable)dgv2.DataSource;
            DataRow dRow = dt2.NewRow();
            dt2.Rows.Add(dRow);
            G1.NumberDataTable(dt2);
//            loadRepositories();
            dgv2.DataSource = dt2;
            dgv2.Refresh();
            gridMain2.RefreshData();
            gridMain2.RefreshEditor(true);
//            gridMain_CellValueChanged(null, null);
        }
        /****************************************************************************************/
        private void pictureBox10_Click(object sender, EventArgs e)
        {
            DataTable dt3 = (DataTable)dgv3.DataSource;
            DataRow dRow3 = dt3.NewRow();
            dt3.Rows.Add(dRow3);
            G1.NumberDataTable(dt3);
            dgv3.DataSource = dt3;
            dgv3.Refresh();
            gridMain3.RefreshData();
            gridMain3.RefreshEditor(true);
        }
        /****************************************************************************************/
        private void pictureBox6_Click(object sender, EventArgs e)
        {   // Delete Maintenance
            DataRow dr2 = gridMain2.GetFocusedDataRow();
            string data = dr2["car"].ObjToString();
            DialogResult result = MessageBox.Show("***Question*** Are you sure you want to DELETE this Maintenance (" + data + ") ?", "Delete Maintenance Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;
            DataTable dt2 = (DataTable)dgv2.DataSource;
            if (dt2 == null)
                return;
            int rowHandle = gridMain2.FocusedRowHandle;
            int row = gridMain2.GetDataSourceRowIndex(rowHandle);
            dr2["Mod"] = "D";
            dt2.Rows[row]["mod"] = "D";
//            gridMain2_CellValueChanged(null, null);
            if (loading)
                return;
            modified = true;
            btnSaveAllMaint.Show();
            btnSaveAllMaint.Refresh();
        }
        /****************************************************************************************/
        private void pictureBox3_Click(object sender, EventArgs e)
        {
            DataTable dt2 = (DataTable)dgv2.DataSource;
            if (dt2.Rows.Count <= 0)
                return;
            DataRow dr = gridMain2.GetFocusedDataRow();
            int rowHandle = gridMain2.FocusedRowHandle;
            if (rowHandle == 0)
                return; // Already at the first row
            //MoveRowUp(dt, rowHandle);
            massRowsUp2(dt2, rowHandle);
            dt2.AcceptChanges();
            dgv2.DataSource = dt2;
            gridMain2.ClearSelection();
            gridMain2.SelectRow(rowHandle - 1);
            gridMain2.FocusedRowHandle = rowHandle - 1;
            gridMain2.RefreshData();
            dgv2.Refresh();
            gridMain2_CellValueChanged(null, null);
        }
        /****************************************************************************************/
        private void pictureBox2_Click(object sender, EventArgs e)
        {
            DataTable dt2 = (DataTable)dgv2.DataSource;
            if (dt2.Rows.Count <= 0)
                return;
            DataRow dr2 = gridMain2.GetFocusedDataRow();
            int rowHandle = gridMain2.FocusedRowHandle;
            if (rowHandle == (dt2.Rows.Count - 1))
                return; // Already at the last row
            MoveRowDown2(dt2, rowHandle);
            dt2.AcceptChanges();
            dgv2.DataSource = dt2;
            gridMain2.ClearSelection();
            gridMain2.SelectRow(rowHandle + 1);
            gridMain2.FocusedRowHandle = rowHandle + 1;
            gridMain2.RefreshData();
            dgv2.Refresh();
            gridMain2_CellValueChanged(null, null);
        }
        /****************************************************************************************/
        private void pictureBox8_Click(object sender, EventArgs e)
        {
            DataTable dt3 = (DataTable)dgv3.DataSource;
            if (dt3.Rows.Count <= 0)
                return;
            DataRow dr = gridMain3.GetFocusedDataRow();
            int rowHandle = gridMain3.FocusedRowHandle;
            if (rowHandle == 0)
                return; // Already at the first row
            massRowsUp3(dt3, rowHandle);
            dt3.AcceptChanges();
            dgv3.DataSource = dt3;
            gridMain3.ClearSelection();
            gridMain3.SelectRow(rowHandle - 1);
            gridMain3.FocusedRowHandle = rowHandle - 1;
            gridMain3.RefreshData();
            dgv3.Refresh();
            gridMain3_CellValueChanged(null, null);
        }
        /****************************************************************************************/
        private void pictureBox7_Click(object sender, EventArgs e)
        {
            DataTable dt3 = (DataTable)dgv3.DataSource;
            if (dt3.Rows.Count <= 0)
                return;
            DataRow dr3 = gridMain3.GetFocusedDataRow();
            int rowHandle = gridMain3.FocusedRowHandle;
            if (rowHandle == (dt3.Rows.Count - 1))
                return; // Already at the last row
            MoveRowDown3(dt3, rowHandle);
            dt3.AcceptChanges();
            dgv3.DataSource = dt3;
            gridMain3.ClearSelection();
            gridMain3.SelectRow(rowHandle + 1);
            gridMain3.FocusedRowHandle = rowHandle + 1;
            gridMain3.RefreshData();
            dgv3.Refresh();
            gridMain3_CellValueChanged(null, null);
        }
        /****************************************************************************************/
        private void pictureBox15_Click(object sender, EventArgs e)
        {
            DataTable dt4 = (DataTable)dgv4.DataSource;
            if (dt4.Rows.Count <= 0)
                return;
            DataRow dr = gridMain4.GetFocusedDataRow();
            int rowHandle = gridMain4.FocusedRowHandle;
            if (rowHandle == 0)
                return; // Already at the first row
            massRowsUp4(dt4, rowHandle);
            dt4.AcceptChanges();
            dgv4.DataSource = dt4;
            gridMain4.ClearSelection();
            gridMain4.SelectRow(rowHandle - 1);
            gridMain4.FocusedRowHandle = rowHandle - 1;
            gridMain4.RefreshData();
            dgv4.Refresh();
            gridMain4_CellValueChanged(null, null);
        }
        /****************************************************************************************/
        private void button1_Click(object sender, EventArgs e)
        {
            DataTable dt2 = (DataTable)dgv2.DataSource;
            if (dt2.Rows.Count <= 0)
                return;
            DataRow dr2 = gridMain2.GetFocusedDataRow();
            int rowHandle = gridMain2.FocusedRowHandle;
            int dtRow2 = gridMain2.GetDataSourceRowIndex(rowHandle);
            if (dtRow2 < 0 || dtRow2 > (dt2.Rows.Count - 1))
                return;
            //if (rowHandle == (dt.Rows.Count - 1))
            //    return; // Already at the last row
            DataRow dRow2 = dt2.NewRow();
            dt2.Rows.InsertAt(dRow2, dtRow2);
            G1.NumberDataTable(dt2);
            dt2.AcceptChanges();
            dgv2.DataSource = dt2;
            gridMain2.ClearSelection();
            gridMain2.RefreshData();
            gridMain2.FocusedRowHandle = rowHandle + 1;
            gridMain2.SelectRow(rowHandle + 1);
            dgv2.Refresh();
            gridMain2_CellValueChanged(null, null);
        }
        
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            // Create a new maintenance record.
            DataRow dr = gridMain.GetFocusedDataRow();
            if (dr == null)
                return;

            string carRecord = dr["record"].ObjToString();
            if (!String.IsNullOrWhiteSpace(carRecord))
            {
                this.Cursor = Cursors.WaitCursor;
                DataTable dt = (DataTable)dgv.DataSource;
                bool car = false;

                // Open the form to input new maintenance

            }
            else
            {
                return;
            }
        }
        /****************************************************************************************/
        private void repositoryItemComboBox1_EditValueChanged(object sender, EventArgs e)
        {
            DataTable dt2 = (DataTable)dgv2.DataSource;
            DataRow dr = gridMain2.GetFocusedDataRow();
            ComboBoxEdit combo = (ComboBoxEdit)sender;
            string vehicle_text = combo.Text;
            int rowHandle = gridMain2.FocusedRowHandle;
            int row = gridMain2.GetDataSourceRowIndex(rowHandle);
            
            dr["mod"] = "Y";
            dr["car"] = vehicle_text;

            dt2.Rows[row]["car"] = vehicle_text;
            gridMain2.RefreshEditor(true);
        }
        /****************************************************************************************/
        private void repositoryItemComboBox1_SelectedValueChanged(object sender, EventArgs e)
        {
            ComboBoxEdit box = (ComboBoxEdit)sender;
            string vehicle_text = box.Text;

            btnSaveAllMaint.Show();
            DataTable dt2 = (DataTable)dgv2.DataSource;
            DataRow dr = gridMain2.GetFocusedDataRow();
            int rowHandle = gridMain2.FocusedRowHandle;
            int row = gridMain2.GetDataSourceRowIndex(rowHandle);

            dr["mod"] = "Y";
            dr["car"] = vehicle_text;

            dt2.Rows[row]["car"] = vehicle_text;
            gridMain2.RefreshEditor(true);
        }
        /****************************************************************************************/
        private void repositoryItemComboBox1_Validating(object sender, CancelEventArgs e)
        {
            if (1 == 1)
                return;
            DataTable dt2 = (DataTable)dgv2.DataSource;
            string vehicle = "";
            string record = "";
            string mod = "";
            int indexNum = 0;

            for (int i = 0; i < dt2.Rows.Count; i++)
            {
                record = dt2.Rows[i]["record"].ObjToString();
                vehicle = dt2.Rows[i]["cars_record"].ObjToString();
                indexNum = vehicle.IndexOf("-");
                vehicle = vehicle.Substring(0, indexNum);
            }
            /*
            DialogResult result = MessageBox.Show("***Notice*** You have changed this field to record number " + vehicle + "?", "Change Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;
            */
        }
        /****************************************************************************************/
        private void repositoryItemComboBox2_SelectedValueChanged(object sender, EventArgs e)
        {
            btnSaveAllMaint.Show();
            modified_maint = true;
            DataRow dr2 = gridMain2.GetFocusedDataRow();
            dr2["mod"] = "Y";
        }
        /****************************************************************************************/
        private void repositoryItemComboBox3_SelectedValueChanged(object sender, EventArgs e)
        {
            btnSaveAllMaint.Show();
            modified_maint = true;
            DataRow dr2 = gridMain2.GetFocusedDataRow();
            dr2["mod"] = "Y";
        }
        /****************************************************************************************/
        private void repositoryItemComboBox4_SelectedValueChanged(object sender, EventArgs e)
        {
            btnSaveAllMaint.Show(); DataTable dt2 = (DataTable)dgv2.DataSource;
            modified_maint = true;
            DataRow dr2 = gridMain2.GetFocusedDataRow();
            dr2["mod"] = "Y";
        }
        /****************************************************************************************/
        private void gridMain2_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            if (e.ListSourceRowIndex == DevExpress.XtraGrid.GridControl.InvalidRowHandle)
                return;
            string name = e.Column.FieldName;
            if (name.ToUpper().IndexOf("DATE") >= 0)
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
        /****************************************************************************************/
        private void repositoryItemComboBox2_EditValueChanged(object sender, EventArgs e)
        {
            DataTable dt2 = (DataTable)dgv2.DataSource;
            DataRow dr = gridMain2.GetFocusedDataRow();
            ComboBoxEdit combo = (ComboBoxEdit)sender;
            string vendor_text = combo.Text;
            int rowHandle = gridMain2.FocusedRowHandle;
            int row = gridMain2.GetDataSourceRowIndex(rowHandle);

            dr["mod"] = "Y";
            dr["vendor_name"] = vendor_text;

            dt2.Rows[row]["vendor_name"] = vendor_text;
            gridMain2.RefreshEditor(true);
        }
        /****************************************************************************************/
        private void repositoryItemComboBox3_EditValueChanged(object sender, EventArgs e)
        {
            DataTable dt2 = (DataTable)dgv2.DataSource;
            DataRow dr = gridMain2.GetFocusedDataRow();
            ComboBoxEdit combo = (ComboBoxEdit)sender;
            string service_text = combo.Text;
            int rowHandle = gridMain2.FocusedRowHandle;
            int row = gridMain2.GetDataSourceRowIndex(rowHandle);

            dr["mod"] = "Y";
            dr["service"] = service_text;

            dt2.Rows[row]["service"] = service_text;
            gridMain2.RefreshEditor(true);
        }
        /****************************************************************************************/
        private void repositoryItemComboBox4_EditValueChanged(object sender, EventArgs e)
        {
            DataTable dt2 = (DataTable)dgv2.DataSource;
            DataRow dr = gridMain2.GetFocusedDataRow();
            ComboBoxEdit combo = (ComboBoxEdit)sender;
            string category_text = combo.Text;
            int rowHandle = gridMain2.FocusedRowHandle;
            int row = gridMain2.GetDataSourceRowIndex(rowHandle);

            dr["mod"] = "Y";
            dr["category"] = category_text;

            dt2.Rows[row]["category"] = category_text;
            gridMain2.RefreshEditor(true);
        }
        /****************************************************************************************/
        private void pictureBox17_Click(object sender, EventArgs e)
        {
            // Adds new row to service type tab
            DataTable dt4 = (DataTable)dgv4.DataSource;
            DataRow dRow4 = dt4.NewRow();
            dt4.Rows.Add(dRow4);
            G1.NumberDataTable(dt4);
            dgv4.DataSource = dt4;
            dgv4.Refresh();
            gridMain4.RefreshData();
            gridMain4.RefreshEditor(true);
        }
        /****************************************************************************************/
        private void pictureBox18_Click(object sender, EventArgs e)
        {
            // Removes/Deletes row to service type tab
            DataRow dr4 = gridMain4.GetFocusedDataRow();
            string data = dr4["service_name"].ObjToString();
            DialogResult result = MessageBox.Show("***Question*** Are you sure you want to DELETE this service (" + data + ")?", "Delete Service Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;
            DataTable dt4 = (DataTable)dgv4.DataSource;
            if (dt4 == null)
                return;
            int rowHandle = gridMain4.FocusedRowHandle;
            int row = gridMain4.GetDataSourceRowIndex(rowHandle);
            dr4["Mod"] = "D";
            dt4.Rows[row]["mod"] = "D";
//            gridMain4_CellValueChanged(null, null);
            if (loading)
                return;
            modified = true;
            btnSaveAllService.Show();
            btnSaveAllService.Refresh();
        }
        /****************************************************************************************/
        private void repositoryItemComboBox9_EditValueChanged(object sender, EventArgs e)
        {
            DataTable dt4 = (DataTable)dgv4.DataSource;
            DataRow dr = gridMain4.GetFocusedDataRow();
            ComboBoxEdit combo = (ComboBoxEdit)sender;
            string cat_desc = combo.Text;
            int rowHandle = gridMain4.FocusedRowHandle;
            int row = gridMain4.GetDataSourceRowIndex(rowHandle);

            dr["mod"] = "Y";
            dr["cat_desc"] = cat_desc;

            dt4.Rows[row]["cat_desc"] = cat_desc;
            gridMain4.RefreshEditor(true);

            btnSaveAllService.Show();
        }
        /****************************************************************************************/
        private void pictureBox14_Click(object sender, EventArgs e)
        {
            DataTable dt4 = (DataTable)dgv4.DataSource;
            if (dt4.Rows.Count <= 0)
                return;
            DataRow dr4 = gridMain4.GetFocusedDataRow();
            int rowHandle = gridMain4.FocusedRowHandle;
            if (rowHandle == (dt4.Rows.Count - 1))
                return; // Already at the last row
            MoveRowDown4(dt4, rowHandle);
            dt4.AcceptChanges();
            dgv4.DataSource = dt4;
            gridMain4.ClearSelection();
            gridMain4.SelectRow(rowHandle + 1);
            gridMain4.FocusedRowHandle = rowHandle + 1;
            gridMain4.RefreshData();
            dgv4.Refresh();
            gridMain4_CellValueChanged(null, null);
        }
        /****************************************************************************************/
        private void btnInsertServ_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv4.DataSource;
            if (dt.Rows.Count <= 0)
                return;
            DataRow dr = gridMain4.GetFocusedDataRow();
            int rowHandle = gridMain4.FocusedRowHandle;
            int dtRow = gridMain4.GetDataSourceRowIndex(rowHandle);
            if (dtRow < 0 || dtRow > (dt.Rows.Count - 1))
                return;
            //if (rowHandle == (dt.Rows.Count - 1))
            //    return; // Already at the last row
            DataRow dRow = dt.NewRow();
            dt.Rows.InsertAt(dRow, dtRow);
            G1.NumberDataTable(dt);
            dt.AcceptChanges();
            dgv4.DataSource = dt;
            gridMain4.ClearSelection();
            gridMain4.RefreshData();
            gridMain4.FocusedRowHandle = rowHandle + 1;
            gridMain4.SelectRow(rowHandle + 1);
            dgv4.Refresh();
            gridMain4_CellValueChanged(null, null);
        }
        /****************************************************************************************/
        private void gridMain2_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv2.DataSource;
            if (dt == null)
                return;
            string delete = dt.Rows[row]["mod"].ObjToString();
            if (delete.ToUpper() == "D")
            {
                e.Visible = false;
                e.Handled = true;
                return;
            }
        }
        /****************************************************************************************/
        private void picSearchUpcoming_Click(object sender, EventArgs e)
        {
            G1.SpyGlass(gridMain5);
        }
        /****************************************************************************************/
        private void pictureBox16_Click(object sender, EventArgs e)
        {
            G1.SpyGlass(gridMain4);
        }
        /****************************************************************************************/
        private void pictureBox9_Click(object sender, EventArgs e)
        {
            G1.SpyGlass(gridMain3);
        }
        /****************************************************************************************/
        private void pictureBox4_Click(object sender, EventArgs e)
        {
            G1.SpyGlass(gridMain2);
        }
        /****************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        { // Spy Glass
            G1.SpyGlass(gridMain);
        }
        /****************************************************************************************/
        private void picAddUpcoming_Click(object sender, EventArgs e)
        {
            // Adds new row to service type tab
            DataTable dt5 = (DataTable)dgv5.DataSource;
            DataRow dRow5 = dt5.NewRow();
            dt5.Rows.Add(dRow5);
            G1.NumberDataTable(dt5);
            dgv5.DataSource = dt5;
            dgv5.Refresh();
            gridMain5.RefreshData();
            gridMain5.RefreshEditor(true);
        }
        /****************************************************************************************/
        private void picMoveUpcomingUp_Click(object sender, EventArgs e)
        {
            DataTable dt5 = (DataTable)dgv5.DataSource;
            if (dt5.Rows.Count <= 0)
                return;
            DataRow dr = gridMain5.GetFocusedDataRow();
            int rowHandle = gridMain5.FocusedRowHandle;
            if (rowHandle == 0)
                return; // Already at the first row
            massRowsUp5(dt5, rowHandle);
            dt5.AcceptChanges();
            dgv5.DataSource = dt5;
            gridMain5.ClearSelection();
            gridMain5.SelectRow(rowHandle - 1);
            gridMain5.FocusedRowHandle = rowHandle - 1;
            gridMain5.RefreshData();
            dgv5.Refresh();
            gridMain5_CellValueChanged(null, null);
        }
        /****************************************************************************************/
        private void picMoveUpcomingDown_Click(object sender, EventArgs e)
        {
            DataTable dt5 = (DataTable)dgv5.DataSource;
            if (dt5.Rows.Count <= 0)
                return;
            DataRow dr5 = gridMain5.GetFocusedDataRow();
            int rowHandle = gridMain5.FocusedRowHandle;
            if (rowHandle == (dt5.Rows.Count - 1))
                return; // Already at the last row
            MoveRowDown5(dt5, rowHandle);
            dt5.AcceptChanges();
            dgv5.DataSource = dt5;
            gridMain5.ClearSelection();
            gridMain5.SelectRow(rowHandle + 1);
            gridMain5.FocusedRowHandle = rowHandle + 1;
            gridMain5.RefreshData();
            dgv5.Refresh();
            gridMain5_CellValueChanged(null, null);
        }
        /****************************************************************************************/
        private void btnInsertUpcoming_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv5.DataSource;
            if (dt.Rows.Count <= 0)
                return;
            DataRow dr = gridMain5.GetFocusedDataRow();
            int rowHandle = gridMain5.FocusedRowHandle;
            int dtRow = gridMain5.GetDataSourceRowIndex(rowHandle);
            if (dtRow < 0 || dtRow > (dt.Rows.Count - 1))
                return;
            //if (rowHandle == (dt.Rows.Count - 1))
            //    return; // Already at the last row
            DataRow dRow = dt.NewRow();
            dt.Rows.InsertAt(dRow, dtRow);
            G1.NumberDataTable(dt);
            dt.AcceptChanges();
            dgv5.DataSource = dt;
            gridMain5.ClearSelection();
            gridMain5.RefreshData();
            gridMain5.FocusedRowHandle = rowHandle + 1;
            gridMain5.SelectRow(rowHandle + 1);
            dgv5.Refresh();
            gridMain5_CellValueChanged(null, null);
        }
        /****************************************************************************************/
        private void picDelUpcoming_Click(object sender, EventArgs e)
        {
            // Removes/Deletes row to service type tab
            DataRow dr5 = gridMain5.GetFocusedDataRow();
            string data = dr5["service_name"].ObjToString();
            DialogResult result = MessageBox.Show("***Question*** Are you sure you want to DELETE this service (" + data + ")?", "Delete Service Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;
            DataTable dt5 = (DataTable)dgv5.DataSource;
            if (dt5 == null)
                return;
            int rowHandle = gridMain5.FocusedRowHandle;
            int row = gridMain5.GetDataSourceRowIndex(rowHandle);
            dr5["Mod"] = "D";
            dt5.Rows[row]["mod"] = "D";
            if (loading)
                return;
            modified = true;
            btnSaveAllUpcoming.Show();
            btnSaveAllUpcoming.Refresh();
        }
        /****************************************************************************************/
        private void btnSaveAllUpcoming_Click(object sender, EventArgs e)
        {
            DataTable dt5 = (DataTable)dgv5.DataSource;
            string record = "";
            string mod = "";

            string vehicle = ""; // Holds the entire string of the Vehicle drop down.
            string cars_record = ""; // Contains the record number of the car.
            string car = ""; // Contains the name of the car without the record number

            string vendor = "";
            string vendor_record = "";
            string vendor_name = "";

            string service_type = "";
            string service_type_record = "";
            string service = "";

            string category_field = ""; // Contains the data from the drop down.
            string category_record = ""; // record number
            string category = "";       // just the name of the category for the database.

            string beginDate = "";
            string endDate = "";
            string sched_b_date = "";
            string sched_e_date = "";
            string mileage = "";
            string cost = "";
            string notes = "";

            int indexNum = 0;

            string cmd = "DELETE from `cars_maint` WHERE `mileage` = '-1'";
            G1.get_db_data(cmd);
            DataTable dx5 = G1.get_db_data("select * from `cars_maint`;");
            DataRow[] dRows2 = null;

            for (int i = 0; i < dt5.Rows.Count; i++)
            {
                record = dt5.Rows[i]["record"].ObjToString();
                mod = dt5.Rows[i]["mod"].ObjToString();
                if (mod == "D")
                {
                    if (!String.IsNullOrWhiteSpace(record))
                        G1.delete_db_table("cars_maint", "record", record);
                    continue;
                }
                if (mod != "Y")
                    continue;

                // If there is no car record, then this is a new record and the number will be in the drop down. Else, it will be in its own column.
                vehicle = dt5.Rows[i]["car"].ObjToString();
                cars_record = dt5.Rows[i]["cars_record"].ObjToString();
                if (cars_record == "")
                {
                    indexNum = vehicle.IndexOf(" ");
                    cars_record = vehicle.Substring(0, indexNum);
                    car = vehicle.Substring(indexNum + 3);
                }
                else
                {
                    car = vehicle;
                }

                // If there is no vendor record, then this is a new record and the number will be in the drop down. Else, it will be in its own column.
                vendor = dt5.Rows[i]["vendor_name"].ObjToString();
                vendor_record = dt5.Rows[i]["vendor_record"].ObjToString();
                if (vendor_record == "")
                {
                    indexNum = vendor.IndexOf(" ");
                    vendor_record = vendor.Substring(0, indexNum);
                    vendor_name = vendor.Substring(indexNum + 3);
                }
                else
                {
                    vendor_name = vendor;
                }

                // If there is no service type record, then this is a new record and the number will be in the drop down. Else, it will be in its own column.
                service_type = dt5.Rows[i]["service"].ObjToString();
                service_type_record = dt5.Rows[i]["service_type_record"].ObjToString();
                if (service_type_record == "")
                {
                    indexNum = service_type.IndexOf(" ");
                    service_type_record = service_type.Substring(0, indexNum);
                    service = service_type.Substring(indexNum + 3);
                }
                else
                {
                    service = service_type;
                }

                // If there is no category record, then this is a new record and the number will be in the drop down. Else, it will be in its own column.
                category_field = dt5.Rows[i]["category"].ObjToString();
                category_record = dt5.Rows[i]["category_record"].ObjToString();
                if (category_record == "")
                {
                    indexNum = category_field.IndexOf(" ");
                    category_record = category_field.Substring(0, indexNum);
                    category = category_field.Substring(indexNum + 3);
                }
                else
                {
                    category = category_field;
                }

                beginDate = dt5.Rows[i]["service_begin_date"].ObjToString();
                endDate = dt5.Rows[i]["service_end_date"].ObjToString();

                sched_b_date = dt5.Rows[i]["service_sched_b_date"].ObjToString();
                sched_e_date = dt5.Rows[i]["service_sched_e_date"].ObjToString();

                mileage = dt5.Rows[i]["mileage"].ObjToString();
                cost = dt5.Rows[i]["cost"].ObjToString();
                notes = dt5.Rows[i]["notes"].ObjToString();

                if (String.IsNullOrWhiteSpace(record))
                    record = G1.create_record("cars_maint", "mileage", "0");
                if (G1.BadRecord("cars_maint", record))
                    return;

                G1.update_db_table("cars_maint", "record", record, new string[] { "cars_record", cars_record, "car", car, "service_type_record", service_type_record, "service", service, "category_record", category_record, "category", category, "service_sched_b_date", sched_b_date, "service_sched_e_date", sched_e_date, "service_begin_date", beginDate, "service_end_date", endDate, "mileage", mileage, "cost", cost, "vendor_record", vendor_record, "vendor_name", vendor_name, "notes", notes });
            }
            modified_maint = false;
            btnSaveAllUpcoming.Hide();
            // IF this is preventive maintenance, create a scheduled maintenance on the same vehicle with the same vendor based on the day frequency in cars_service_type
            if (category.ToUpper() == "PREVENTIVE" && endDate != "01/01/0001" && mod != "D")
            {
                // retrieve the frequency in days from cars_service_type
                string sql = "SELECT frequency_days, service_time_length, frequency_miles FROM cars_service_type WHERE record = " + service_type_record + ";";
                DataTable dx = G1.get_db_data(sql);
                if (dx.Rows.Count > 0)
                {
                    int frequency_days = dx.Rows[0]["frequency_days"].ObjToInt32();
                    int service_time_length = dx.Rows[0]["service_time_length"].ObjToInt32();
                    int frequency_miles = dx.Rows[0]["frequency_miles"].ObjToInt32();
                    DateTime schedule_begin_date = beginDate.ObjToDateTime();

                    // increment end service date by frequency_days and store that in sched_b_date.
                    schedule_begin_date = schedule_begin_date.AddDays(frequency_days);
                    sched_b_date = schedule_begin_date.ObjToString();

                    // increment schedule start date by service_time_length and store it in sched_e_date.
                    DateTime schedule_end_date = schedule_begin_date.AddDays(service_time_length);
                    sched_e_date = schedule_end_date.ObjToString();

                    // Create a record# for the new maintenance record.
                    record = G1.create_record("cars_maint", "mileage", "0");
                    if (G1.BadRecord("cars_maint", record))
                        return;

                    // Clear the actual begin and end dates and notes so that those don't copy over into the new record.
                    beginDate = "";
                    endDate = "";
                    notes = "";

                    // Increase mileage so it isn't the same as last time.
                    int mileage_int = 0;
                    mileage_int = mileage.ObjToInt32();
                    mileage_int = mileage_int + frequency_miles;
                    mileage = mileage_int.ObjToString();

                    // Use the same data from before to create a new record with the new record number 
                    G1.update_db_table("cars_maint", "record", record, new string[] { "cars_record", cars_record, "car", car, "service_type_record", service_type_record, "service", service, "category_record", category_record, "category", category, "service_sched_b_date", sched_b_date, "service_sched_e_date", sched_e_date, "service_begin_date", beginDate, "service_end_date", endDate, "mileage", mileage, "cost", cost, "vendor_record", vendor_record, "vendor_name", vendor_name, "notes", notes });

                    // Refresh the table(s)
                    dgv5.Refresh();
                    dgv2.Refresh();
                }
            }
        }
        /****************************************************************************************/
        private void RunAutoReports()
        {
            //G1.AddToAudit("System", "AutoRun", "AT Funeral Activity Report", "Starting Funeral Autorun . . . . . . . ", "");
            workReport = "Upcoming Vehicle Maintenance Report for " + DateTime.Now.ToString("MM/dd/yyyy");
            string cmd = "Select * from `remote_processing`;";
            DataTable dt = G1.get_db_data(cmd);
            string report = "";
            DateTime date = DateTime.Now;
            int presentDay = date.Day;
            int dayToRun = 0;
            string status = "";
            string frequency = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                status = dt.Rows[i]["status"].ObjToString();
                if (status.ToUpper() == "INACTIVE")
                    continue;
                if (!autoForce)
                {
                    dayToRun = dt.Rows[i]["day_to_run"].ObjToInt32();
                    frequency = dt.Rows[i]["dateIncrement"].ObjToString();
                    if (!AutoRunSetup.CheckOkToRun(dayToRun, frequency))
                        return;
                }
                report = dt.Rows[i]["report"].ObjToString();
                sendTo = dt.Rows[i]["sendTo"].ObjToString();
                sendWhere = dt.Rows[i]["sendWhere"].ObjToString();
                da = dt.Rows[i]["da"].ObjToString();
                if (report.ToUpper() == "FUNERAL ACTIVITY REPORT")
                {
                    //G1.AddToAudit("System", "AutoRun", "Funeral Activity Report Load", "Starting Load . . . . . . . ", "");
                    Upcoming_Load(null, null);
                }
            }
        }
        /****************************************************************************************/
        private void Upcoming_Load(object sender, EventArgs e)
        {

            DateTime now = DateTime.Now;
//            this.dateTimePicker1.Value = now;
//            this.dateTimePicker2.Value = now;

            if (LoginForm.username.ToUpper() != "ROBBY")
            {
                miscToolStripMenuItem.DropDownItems.Clear();
                miscToolStripMenuItem.Dispose();
            }

            gridMain5.Columns["num"].Visible = true;
            gridMain5.OptionsPrint.PrintBandHeader = false;
            gridMain5.OptionsPrint.PrintFooter = false;

            getLocations();

            if (autoRun)
            {
                try
                {
//                    btnRun_Click(null, null);
                }
                catch (Exception ex)
                {
                }

                //G1.AddToAudit("System", "AutoRun", "Funeral Activity Print Preview", "Starting Report . . . . . . . ", "");
                printPreviewToolStripMenuItem_Click(null, null);
                this.Close();
            }
        }
        /***********************************************************************************************/
        private DataTable _LocationList;
        private void getLocations()
        {
            //string cmd = "SELECT `LocationCode` FROM `inventory` GROUP BY `LocationCode` ASC;";
            string cmd = "Select * from `funeralhomes` order by `atneedcode`;";
            _LocationList = G1.get_db_data(cmd);

            string str = "";

            for (int i = _LocationList.Rows.Count - 1; i >= 0; i--)
            {
                str = _LocationList.Rows[i]["atneedcode"].ObjToString();
                if (String.IsNullOrWhiteSpace(str))
                    _LocationList.Rows.RemoveAt(i);
            }

            //chkComboLocation.Properties.DataSource = _LocationList;
        }
        /*******************************************************************************************/
    }
}