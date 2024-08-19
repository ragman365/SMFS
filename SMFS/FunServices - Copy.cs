using DevExpress.Utils;
using DevExpress.Xpo.Helpers;
using DevExpress.XtraCharts;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.BandedGrid;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports.UI;
using DocumentFormat.OpenXml.Bibliography;
using GeneralLib;
using Microsoft.Ink;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Forms;
using Cursors = System.Windows.Forms.Cursors;
using GridView = DevExpress.XtraGrid.Views.Grid.GridView;
using Image = System.Drawing.Image;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class FunServices : DevExpress.XtraEditors.XtraForm
    {
        private bool serviceOnly = false;
        public DataTable funServicesDT = null;
        private DataTable _funServicesDt;
        private string workContract = "";
        private bool funModified = false;
        private bool loading = true;
        private Color serviceColor = Color.Transparent;
        private bool showServices = true;
        private bool showMerchandise = false;
        private bool showCashAdvanced = false;
        private bool showAll = false;
        private DataTable workDt = null;
        private Bitmap emptyImage;
        private DevExpress.XtraEditors.XtraForm workControl = null;
        private bool workDetached = false;
        private string deceasedDate = "";
        private string serviceId = "";
        private string primaryContract = "";

        private string contractFile = "contracts";
        private string customerFile = "customers";
        private string extendedFile = "cust_extended";
        private string custServicesFile = "cust_services";

        private bool gotPackage = false;

        private string custExtendedRecord = "";
        private bool totalModified = false;
        public DataTable Answer { get { return _funServicesDt; } }

        private bool workFuneral = false;
        private string selectWhat = "";
        private bool matchedSSNs = false;
        /****************************************************************************************/
        public FunServices(DevExpress.XtraEditors.XtraForm mainControl, string contract, bool funeral)
        {
            InitializeComponent();
            workContract = contract;
            workControl = mainControl;
            SetupTotalsSummary();
            workFuneral = funeral;
        }
        /****************************************************************************************/
        public FunServices(string contract, bool funeral)
        {
            InitializeComponent();
            workContract = contract;
            workControl = null;
            SetupTotalsSummary();
            workFuneral = funeral;
            FunServices_Load(null, null);

            string group = EditCustomer.activeFuneralHomeGroup;
            string casketGroup = EditCustomer.activeFuneralHomeCasketGroup;

        }
        /****************************************************************************************/
        public FunServices(DevExpress.XtraEditors.XtraForm mainControl, string contract, bool funeral, bool detached)
        {
            InitializeComponent();
            workContract = contract;
            workControl = mainControl;
            workDetached = detached;
            SetupTotalsSummary();
            workFuneral = funeral;
        }
        /****************************************************************************************/
        public FunServices(string contract)
        {
            InitializeComponent();
            serviceOnly = true;
            workFuneral = true;
            workContract = contract;
            FunServices_Load(null, null);
            this.Close();
        }
        /****************************************************************************************/
        private void FunServices_Load(object sender, EventArgs e)
        {
            //if (!LoginForm.administrator)
            //{
            btnSelectMerchandise.Hide();
            btnDetach.Hide();
            btnRemoveAll.Hide();
            //}
            if (workFuneral)
            {
                customerFile = "fcustomers";
                contractFile = "fcontracts";
                extendedFile = "fcust_extended";
                custServicesFile = "fcust_services";
            }

            if ( !String.IsNullOrWhiteSpace ( workContract ))
            {
                if ( String.IsNullOrWhiteSpace (EditCustomer.activeFuneralHomeGroup) )
                {
                    string cmd = "Select * from `contracts` where `contractNumber` = '" + workContract + "';";
                    DataTable dt = G1.get_db_data(cmd);
                    if (dt.Rows.Count > 0)
                    {
                        string serviceId = dt.Rows[0]["serviceId"].ObjToString();
                        EditCust.DetermineActiveGroups(workContract, serviceId );
                        EditCustomer.activeFuneralHomeGroup = EditCust.activeFuneralHomeGroup;
                        EditCustomer.activeFuneralHomeCasketGroup = EditCust.activeFuneralHomeCasketGroup;
                        EditCustomer.activeFuneralHomeName = EditCust.activeFuneralHomeName;
                    }
                }
            }

            btnMultiSSN.Hide();
            emptyImage = new Bitmap(1, 1);
            this.picCasket.Hide();
            btnSaveServices.Hide();
            funModified = false;
            loading = true;

            LoadFuneralClass();
            LoadGroupCombo();
            LoadPackagesCombo();
            LoadCasketGroupCombo();

            if (workFuneral)
                PresentDuplicateSSNs();
            btnServices_Click(null, null);
            LoadData();
            if (!String.IsNullOrWhiteSpace(workContract))
                LoadServices();

            loading = false;
            gridMain.Columns["SerialNumber"].Visible = false;
            if (workDetached)
            {
                btnDetach.Text = "Print";
            }
            if (serviceOnly)
            {
                funServicesDT = workDt.Copy();
                _funServicesDt = workDt.Copy();
                this.Close();
            }
            totalModified = false;
            if (!CheckForEditing())
            {
                pictureBox3.Hide();
                gridMain.OptionsBehavior.Editable = false;
                gridMain.OptionsBehavior.ReadOnly = true;
                btnSelectMerchandise.Enabled = false;
            }
            if ( !workFuneral && G1.isField() )
            {
                pictureBox3.Hide();
                pictureDelete.Hide();
            }
            this.Focus();
            this.BringToFront();
            btnServices.Focus();
            btnServices_Click(null, null);

            if ( workFuneral )
            {
                cmbCasketGroup.Enabled = false;
                cmbGroups.Enabled = false;
                cmbPackage.Enabled = false;
            }

            //this.ForceRefresh();
            //Application.DoEvents();
            //this.ForceRefresh();
            //this.Update();

            //int width = this.Width;
            //int height = this.Height;
            //this.Size = new Size(width - 50, height - 50);
        }
        /***************************************************************************************/
        public void FireEventFunReloadServices()
        {
            this.Refresh();
        }
        /****************************************************************************************/
        private void LoadFuneralClass()
        {
            string cmd = "Select * from `ref_funeral_classification`;";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                cmbFunClass.DataSource = dt;
            }
            cmbFunClass.Text = "";

            cmd = "Select * from `" + extendedFile + "` where `contractNumber` = '" + workContract + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                string funClass = dx.Rows[0]["funeral_classification"].ObjToString();
                cmbFunClass.Text = funClass;
            }
        }
        /****************************************************************************************/
        private bool CheckForEditing()
        {
            if (workFuneral)
                return true;
            string cmd = "Select * from `contracts` where `contractNumber` = '" + workContract + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return false;
            string serviceId = dt.Rows[0]["serviceId"].ObjToString();
            DateTime deceasedDate = dt.Rows[0]["deceasedDate"].ObjToDateTime();
            if (deceasedDate.Year > 1000 && !String.IsNullOrWhiteSpace(serviceId))
                return false; // Even Administrators cannot change services once deceased and Service ID has been entered.
            if (!LoginForm.administrator)
                return false;
            return true;
        }
        /****************************************************************************************/
        private void PresentDuplicateSSNs()
        {
            string cmd = "Select * from `customers` where `contractNumber` = '" + workContract + "';";
            if (workFuneral)
                cmd = "Select * from `fcustomers` where `contractNumber` = '" + workContract + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
                return;

            string ssn = dx.Rows[0]["ssn"].ObjToString();
            if (String.IsNullOrWhiteSpace(ssn))
                return;
            if (ssn == "0")
                return;

            cmd = "Select * from `fcustomers` where `ssn` = '" + ssn + "';";
            dx = G1.get_db_data(cmd);

            string contractNumber = "";
            DataRow[] dRows = null;
            cmd = "Select * from `customers` where `ssn` = '" + ssn + "';";
            DataTable ddx = G1.get_db_data(cmd);
            for ( int i=0; i<ddx.Rows.Count; i++)
            {
                contractNumber = ddx.Rows[i]["contractNumber"].ObjToString();
                dRows = dx.Select("contractNumber='" + contractNumber + "'");
                if (dRows.Length <= 0)
                    G1.copy_dt_row(ddx, i, dx, dx.Rows.Count);
            }
            if (dx.Rows.Count <= 1)
                return;
            btnMultiSSN.Show();
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
        /***********************************************************************************************/
        private void LoadData()
        {
            //            this.Cursor = Cursors.WaitCursor;
            string location = GetGroup();
            string package = cmbPackage.Text;

            gridMain.Columns["price"].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Custom;
            gridMain.Columns["price"].SummaryItem.DisplayFormat = "{0:C2}";

            gridMain.Columns["currentprice"].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Custom;
            gridMain.Columns["currentprice"].SummaryItem.DisplayFormat = "{0:C2}";

            gridMain.Columns["difference"].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Custom;
            gridMain.Columns["difference"].SummaryItem.DisplayFormat = "{0:C2}";

            LoadPackage();
            funModified = false;
            //            this.Cursor = Cursors.Default;
            //btnServices_Click(null, null);
        }
        /***********************************************************************************************/
        private void LoadPackage()
        {
            string serviceRecord = "";
            string group = GetGroup();
            string package = cmbPackage.Text;
            if (String.IsNullOrWhiteSpace(package))
                package = "Master";
            string list = "";
            string cmd = "Select * from `packages` where `groupname` = '" + group + "' and `PackageName` = '" + package + "';";
            DataTable dx = G1.get_db_data(cmd);
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                serviceRecord = dx.Rows[i]["!serviceRecord"].ObjToString();
                if (String.IsNullOrWhiteSpace(serviceRecord))
                    continue;
                list += "'" + serviceRecord + "',";
            }
            list = list.TrimEnd(',');
            if (!String.IsNullOrWhiteSpace(list))
            {
                cmd = "Select * from `packages` p RIGHT JOIN `services` s ON p.`!serviceRecord` = s.`record` where s.`record` IN (" + list + ") ";
                cmd += " and `groupname` = '" + group + "' and `PackageName` = '" + package + "' ";
                cmd += ";";
                //                cmd = "Select * from `services` where `record` IN (" + list + ");";
            }
            else
                cmd = "Select * from `packages` p JOIN `services` s ON p.`!serviceRecord` = s.`record` where `service` = 'xyzzyxxxx';";
            DataTable dt = G1.get_db_data(cmd);

            dt.Columns.Add("num");
            dt.Columns.Add("mod");
            dt.Columns.Add("select");
            //            dt.Columns.Add("SameAsMaster");
            dt.Columns.Add("total", Type.GetType("System.Double"));
            SetupSelection(dt);
            MatchServices(dt);
            ProcessPackage(dt);

            DetermineDiscount(dt);

            ReCalcTotal(dt);
            G1.NumberDataTable(dt);


            dgv.DataSource = dt;
            //            this.Cursor = Cursors.Default;
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
        /***********************************************************************************************/
        public static void RunServiceTranslator(DataTable dt)
        {
            string fromService = "";
            DataRow[] dR = null;
            string cmd = "Select * from `funeral_translator`;";
            DataTable dx = G1.get_db_data(cmd);
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                fromService = dx.Rows[i]["fromService"].ObjToString();
                dR = dt.Select("service='" + fromService + "'");
                if (dR.Length > 0)
                    dR[0]["service"] = dx.Rows[i]["toService"].ObjToString();
            }
        }
        /***********************************************************************************************/
        private void LoadServices()
        {
            if (String.IsNullOrWhiteSpace(workContract))
                return;
            //            string cmd = "Select * from `cust_services` c JOIN `services` s ON c.`service` = s.`service` where `contractNumber` = '" + workContract + "';";
            string cmd = "Select * from `" + custServicesFile + "` c LEFT JOIN `funeral_master` s ON c.`service` = s.`service` where `contractNumber` = '" + workContract + "';";
            DataTable dt = G1.get_db_data(cmd);
            Services.FixAllData(dt);
            RunServiceTranslator(dt);

            string casketCode = "";
            string casketDesc = "";
            string type = "";
            bool foundPicture = false;
            DataRow[] dR = null;
            cmd = "Select * from `" + custServicesFile + "` where `data` LIKE 'CASKET:%' and `contractNumber` = '" + workContract + "';";
            DataTable dx = G1.get_db_data(cmd);
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                casketDesc = dx.Rows[i]["service"].ObjToString();
                dR = dt.Select("service='" + casketDesc + "'");
                if (dR.Length <= 0)
                {
                    dt.ImportRow(dx.Rows[i]);

                    casketCode = dx.Rows[i]["data"].ObjToString();
                    if (ShowCasket(casketCode))
                        foundPicture = true;
                }
            }
            if (!foundPicture)
            {
                string[] Lines = null;
                cmd = "Select * from `" + custServicesFile + "` where `service` LIKE 'CASKET NAME%' and `contractNumber` = '" + workContract + "';";
                dx = G1.get_db_data(cmd);
                for (int i = 0; i < dx.Rows.Count; i++)
                {
                    type = dx.Rows[i]["type"].ObjToString().ToUpper();
                    if (type == "MERCHANDISE")
                    {
                        casketCode = dx.Rows[i]["service"].ObjToString();
                        if (casketCode.ToUpper().IndexOf("CASKET NAME") >= 0)
                            casketCode = dx.Rows[i]["data"].ObjToString();
                        Lines = casketCode.Split(' ');
                        if (Lines.Length > 1)
                        {
                            casketCode = Lines[0].Trim();
                            if (ShowCasketPicture(casketCode))
                            {
                                foundPicture = true;
                                break;
                            }
                            else
                            {
                                casketDesc = dx.Rows[i]["data"].ObjToString();
                                casketDesc = casketDesc.Replace(casketCode, "").Trim();
                                cmd = "Select * from `inventorylist` where `casketdesc` LIKE '%" + casketDesc + "%';";
                                DataTable ddx = G1.get_db_data(cmd);
                                if (ddx.Rows.Count > 0)
                                {
                                    casketCode = ddx.Rows[0]["casketcode"].ObjToString();
                                    if (ShowCasketPicture(casketCode))
                                    {
                                        foundPicture = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (!foundPicture)
            {
                string[] Lines = null;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    type = dt.Rows[i]["type"].ObjToString().ToUpper();
                    if (type == "MERCHANDISE")
                    {
                        casketCode = dt.Rows[i]["service"].ObjToString();
                        Lines = casketCode.Split(' ');
                        if (Lines.Length > 1)
                        {
                            casketCode = Lines[0].Trim();
                            if (ShowCasketPicture(casketCode))
                                break;
                        }
                    }
                }
            }
            dt.Columns.Add("num");
            dt.Columns.Add("mod");
            dt.Columns.Add("select");
            dt.Columns.Add("status");
            dt.Columns.Add("total", Type.GetType("System.Double"));
            dt.Columns.Add("currentprice", Type.GetType("System.Double"));

            //            if (dt.Rows.Count <= 0)
            ResolveImportedData(dt);

            CleanupOriginal(dt, "Casket");
            CleanupOriginal(dt, "Outer Container");
            CleanupOriginal(dt, "Alt Container");
            CleanupOriginal(dt, "URN");

            SetupSelection(dt);
            DetermineServices(dt);

            DetermineDiscount(dt);

            Services.FixAllData(dt);

            ReCalcTotal(dt);
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            workDt = dt;
        }
        /***********************************************************************************************/
        private void ResolveImportedData(DataTable dt)
        {
            string cmd = "Select * from `" + customerFile + "` where `contractNumber` = '" + workContract + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
                return;

            string group = EditCustomer.activeFuneralHomeCasketGroup;
            if (String.IsNullOrWhiteSpace(group))
                group = "Casket Group 3.3";

            string casketCode = dx.Rows[0]["extraItemAmtMI1"].ObjToString();
            if (casketCode.ToUpper().IndexOf("-BAD") >= 0)
                casketCode = "";
            string vaultCode = dx.Rows[0]["extraItemAmtMI2"].ObjToString();
            if (vaultCode.ToUpper().IndexOf("-BAD") >= 0)
                vaultCode = "";
            double casketPrice = dx.Rows[0]["extraItemAmtMR1"].ObjToDouble();
            double vaultPrice = dx.Rows[0]["extraItemAmtMR2"].ObjToDouble();

            ProcessImportedData(dt, casketCode, casketPrice, "Casket");
            ProcessImportedData(dt, vaultCode, vaultPrice, "Vault");
        }
        /***********************************************************************************************/
        private void AddDefaultMerchandise(DataTable dt, string casketCode, double casketPrice, string type)
        {
            DataRow dR = dt.NewRow();
            string what = "Casket";
            if (casketCode.Length > 0)
            {
                string dd = casketCode.Substring(0, 1).ToUpper();
                if (dd == "V")
                    what = "VAULT";
                else if (dd == "U")
                    what = "URN";
                else
                    what = type;
            }
            dR["service"] = casketCode + " " + what;
            dR["currentprice"] = casketPrice;
            dR["price"] = casketPrice;
            dR["type"] = "Merchandise";
            dR["status"] = "Imported";
            if ( G1.get_column_number ( dt, "mod") >= 0 )
                dR["mod"] = "1";
            dt.Rows.Add(dR);
        }
        /***********************************************************************************************/
        private void ProcessImportedData(DataTable dt, string casketCode, double casketPrice, string type)
        {
            if (String.IsNullOrWhiteSpace(casketCode))
                return;
            if (G1.get_column_number(dt, "data") >= 0)
            {
                DataRow[] dRows = dt.Select("data='" + casketCode + "'");
                if (dRows.Length > 0)
                {
                    if (dRows[0]["service"].ObjToString().ToUpper() == "CASKET NAME")
                    {
                        dRows = dt.Select("service='Casket Price'");
                        if (dRows.Length > 0)
                            return;
                    }
                    else if (dRows[0]["service"].ObjToString().ToUpper() == "OUTER CONTAINER NAME")
                    {
                        dRows = dt.Select("service='Outer Container Price'");
                        if (dRows.Length > 0)
                            return;
                    }
                }
            }
            string cmd = "Select * from `casket_master` where `casketcode` = '" + casketCode + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
            {
                cmd = "Select * from `casket_master` where `casketcode` LIKE '" + casketCode + "%';";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count <= 0)
                {
                    AddDefaultMerchandise(dt, casketCode, casketPrice, type);
                    return;
                }
            }

            string group = EditCustomer.activeFuneralHomeCasketGroup;
            if (String.IsNullOrWhiteSpace(group))
                group = "Casket Group 3.3";

            string masterRecord = dx.Rows[0]["record"].ObjToString();
            double cost = dx.Rows[0]["casketcost"].ObjToDouble();
            double rounding = dx.Rows[0]["round"].ObjToDouble();
            string service = dx.Rows[0]["casketdesc"].ObjToString();

            bool gotVault = false;

            double currentPrice = 0D;
            DataRow dR = null;
            DataRow[] ddR = null;

            string chr = casketCode.Substring(0, 1).ToUpper();
            if (chr == "V" || casketCode.IndexOf("URN") >= 0)
            {
                //                currentPrice = dx.Rows[0]["casketprice"].ObjToDouble();
                if (service.IndexOf(casketCode) < 0)
                    service = casketCode + " " + service;
                ddR = dt.Select("service LIKE'" + casketCode + "%'");
                if (ddR.Length > 0)
                    gotVault = true;
                else
                    ddR = dt.Select("service='" + service + "'");
                if (ddR.Length > 0)
                {
                    if (currentPrice <= 0D)
                    {
                        cmd = "Select * from `casket_master` where `casketCode` = '" + casketCode + "';";
                        dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count > 0)
                        {
                            currentPrice = dx.Rows[0]["casketcost"].ObjToDouble();
                            if (currentPrice <= 0D)
                                currentPrice = dx.Rows[0]["casketprice"].ObjToDouble();
                        }
                    }
                    ddR[0]["currentprice"] = currentPrice;
                    //                ddR[0]["price"] = casketPrice;
                    ddR[0]["type"] = "Merchandise";
                    ddR[0]["status"] = "Imported";
                }
                else
                {
                    dR = dt.NewRow();
                    dR["service"] = service;
                    dR["currentprice"] = cost;
                    dR["price"] = casketPrice;
                    dR["type"] = "Merchandise";
                    dR["status"] = "Imported";
                    if (G1.get_column_number(dt, "mod") >= 0)
                        dR["mod"] = "1";
                    dt.Rows.Add(dR);
                }
                return;
            }

            cmd = "Select * from `casket_packages` where `!masterRecord` = '" + masterRecord + "' and `groupname` = '" + group + "';";
            dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
                return;

            if (rounding > 0D)
                cost = Caskets.RoundTo(cost, rounding);

            double markup = dx.Rows[0]["markup"].ObjToDouble();
            currentPrice = cost * markup;
            currentPrice = G1.RoundValue(currentPrice);
            ddR = dt.Select("service='" + service + "'");
            if (ddR.Length > 0)
            {
                ddR[0]["currentprice"] = currentPrice;
                //                ddR[0]["price"] = casketPrice;
                ddR[0]["type"] = "Merchandise";
                ddR[0]["status"] = "Imported";
            }
            else
            {
                dR = dt.NewRow();
                dR["service"] = service;
                dR["currentprice"] = currentPrice;
                dR["price"] = casketPrice;
                dR["type"] = "Merchandise";
                dR["status"] = "Imported";
                if (G1.get_column_number(dt, "mod") >= 0)
                    dR["mod"] = "1";
                dt.Rows.Add(dR);
            }
            ShowCasketPicture(casketCode);
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
            dR[0]["mod"] = "1";

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
                dR[0]["mod"] = "1";
                return;
            }
            double markup = dx.Rows[0]["markup"].ObjToDouble();
            double currentPrice = cost * markup;
            dR[0]["currentprice"] = currentPrice;
            dR[0]["mod"] = "1";
        }
        /***********************************************************************************************/
        private bool ShowCasket(string casketCode)
        {
            if (casketCode.IndexOf("Casket:") < 0)
                return false;
            casketCode = casketCode.Replace("Casket:", "").Trim();
            if (String.IsNullOrWhiteSpace(casketCode))
                return false;
            bool found = false;
            string cmd = "Select * from `inventorylist` where `casketcode` = '" + casketCode + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                Byte[] bytes = dx.Rows[0]["picture"].ObjToBytes();
                Image myImage = emptyImage;
                if (bytes != null)
                {
                    myImage = G1.byteArrayToImage(bytes);
                    this.picCasket.Image = (Bitmap)myImage;
                    this.picCasket.Show();
                    found = true;
                }
            }
            return found;
        }
        /***********************************************************************************************/
        private bool ShowCasketPicture(string casketCode)
        {
            casketCode = casketCode.Replace("Casket:", "").Trim();
            if (String.IsNullOrWhiteSpace(casketCode))
                return false;
            bool found = false;
            string cmd = "Select * from `inventorylist` where `casketcode` = '" + casketCode + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                Byte[] bytes = dx.Rows[0]["picture"].ObjToBytes();
                Image myImage = emptyImage;
                if (bytes != null)
                {
                    myImage = G1.byteArrayToImage(bytes);
                    this.picCasket.Image = (Bitmap)myImage;
                    this.picCasket.Show();
                    found = true;
                }
            }
            return found;
        }
        /***********************************************************************************************/
        private string getSerialNumber ( string casketCode )
        {
            if (!workFuneral)
                return "";
            string serialNumber = "";
            string cmd = "Select * from `fcust_extended` where `contractNumber` = '" + workContract + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
                return "";
            string serviceId = dx.Rows[0]["serviceId"].ObjToString();
            if (String.IsNullOrWhiteSpace(serviceId))
                return "";
            cmd = "SELECT* FROM `inventory` WHERE `casketDescription` LIKE '" + casketCode + "%' AND serviceId = '" + serviceId + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return "";
            serialNumber = dt.Rows[0]["SerialNumber"].ObjToString();
            return serialNumber;
        }
        /***********************************************************************************************/
        private void DetermineServices(DataTable dt)
        {
            double data = 0D;
            double price = 0D;
            string type = "";
            string cmd = "";
            string service = "";
            try
            {

                string group = EditCustomer.activeFuneralHomeGroup;
                if (String.IsNullOrWhiteSpace(group))
                    group = "Group 3 GPL";

                string casketGroup = EditCustomer.activeFuneralHomeCasketGroup;

                DataTable dx = null;
                if (G1.get_column_number(dt, "currentprice") < 0)
                    dt.Columns.Add("currentprice", Type.GetType("System.Double"));
                if (G1.get_column_number(dt, "data") < 0)
                    dt.Columns.Add("data");

                double currentPrice = 0D;
                string mainDescription = "";

                DataRow[] dRows = dt.Select("isPackage='P'");
                bool isPackage = false;
                if (dRows.Length > 0)
                    isPackage = true;

                string package = "";
                bool gotNewCode = false;

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    try
                    {
                        gotNewCode = false;
                        currentPrice = dt.Rows[i]["currentprice"].ObjToDouble();
                        dt.Rows[i]["select"] = "1";
                        if (isPackage)
                        {
                            package = dt.Rows[i]["isPackage"].ObjToString();
                            if ( package.ToUpper() == "P")
                                dt.Rows[i]["select"] = dt.Rows[i]["pSelect"].ObjToString();
                        }
                        type = dt.Rows[i]["type"].ObjToString();
                        service = dt.Rows[i]["service"].ObjToString().Trim();
                        if (service.ToUpper().IndexOf("D-") == 0)
                        {
                            dt.Rows[i]["currentPrice"] = dt.Rows[i]["price"];
                            continue;
                        }
                        if (String.IsNullOrWhiteSpace(service))
                            continue;
                        if (service.ToUpper() == "ACKNOLEDGEMENT CARDS")
                        {
                            service = "ACKNOWLEDGEMENT CARDS";
                        }
                        if (service.ToUpper() == "OTHER PREPARATION OF THE BODY")
                            service = "OTHER PREPERATION OF THE BODY";

                        if (type.ToUpper() == "SERVICE")
                        {
                            if (!String.IsNullOrWhiteSpace(service))
                            {
                                if ( service.ToUpper().IndexOf ( "UTILITY") > 0 )
                                {
                                }
                                //                        cmd = "Select * from `services` where `service` = '" + service + "';";
                                cmd = "Select * from `funeral_gplgroups` where `groupname` = '" + group + "' and `service` = '" + service + "';";
                                dx = G1.get_db_data(cmd);
                                if (dx.Rows.Count > 0)
                                {
                                    //                        dt.Rows[i]["type"] = dx.Rows[0]["type"].ObjToString();
                                    currentPrice = dx.Rows[0]["price"].ObjToDouble();
                                }
                                else
                                {
                                    if (G1.get_column_number(dt, "data") >= 0)
                                    {
                                        currentPrice = dt.Rows[i]["data"].ObjToDouble();
                                        if (currentPrice == 0D && G1.get_column_number(dt, "price") >= 0)
                                            currentPrice = dt.Rows[i]["price"].ObjToDouble();
                                    }
                                }
                            }
                        }
                        else if (type.ToUpper() == "MERCHANDISE")
                        {
                            if (!String.IsNullOrWhiteSpace(casketGroup))
                            {
                                string[] Lines = service.Split(' ');
                                string casketCode = service;
                                if (Lines.Length > 1)
                                    casketCode = Lines[0].Trim();
                                if (service.ToUpper() == "URN CREDIT")
                                    casketCode = "URN1";
                                cmd = "Select * from `casket_master` where `casketcode` = '" + casketCode + "';";
                                dx = G1.get_db_data(cmd);
                                //                        if (dx.Rows.Count <= 0 && casketCode.Length > 3)
                                if (dx.Rows.Count <= 0 && casketCode.Length > 1 && casketCode.Length <= 6 )
                                {
                                    string newcode = casketCode.Substring(0, 3);
                                    cmd = "Select * from `casket_master` where `casketcode` LIKE '" + newcode + "%';";
                                    dx = G1.get_db_data(cmd);
                                    if (dx.Rows.Count > 0)
                                    {
                                        casketCode = newcode;
                                        gotNewCode = true;
                                    }
                                }
                                if ( dx.Rows.Count <= 0 )
                                {
                                    cmd = "Select * from `casket_master` where `casketdesc` = '" + service + "';";
                                    dx = G1.get_db_data(cmd);
                                }
                                if (dx.Rows.Count > 0)
                                {
                                    mainDescription = dx.Rows[0]["casketdesc"].ObjToString();
                                    if (mainDescription.IndexOf(casketCode) >= 0)
                                    {
                                        dt.Rows[i]["service"] = mainDescription;
                                        string serialNumber = dt.Rows[i]["serialNumber"].ObjToString();
                                        if (String.IsNullOrWhiteSpace(serialNumber))
                                        {
                                            serialNumber = getSerialNumber(casketCode);
                                            if (!String.IsNullOrWhiteSpace(serialNumber))
                                                dt.Rows[i]["serialNumber"] = serialNumber;
                                        }
                                    }
                                    else
                                    {
                                        if (!mainDescription.ToUpper().Contains(casketCode.ToUpper()))
                                            dt.Rows[i]["service"] = casketCode + " " + mainDescription;
                                    }
                                    if (gotNewCode)
                                        dt.Rows[i]["mod"] = "1";
                                    double rounding = dx.Rows[0]["round"].ObjToDouble();
                                    double casketCost = dx.Rows[0]["casketcost"].ObjToDouble();
                                    currentPrice = casketCost;
                                    string masterRecord = dx.Rows[0]["record"].ObjToString();
                                    cmd = "Select * from `casket_packages` where `groupname` = '" + casketGroup + "' AND `!masterRecord` = '" + masterRecord + "';";
                                    dx = G1.get_db_data(cmd);
                                    if (dx.Rows.Count > 0)
                                    {
                                        double markup = dx.Rows[0]["markup"].ObjToDouble();
                                        currentPrice = casketCost * markup;
                                        if (rounding > 0D)
                                            currentPrice = Caskets.RoundTo(currentPrice, rounding);
                                    }
                                }
                                else
                                {
                                    if (service.ToUpper() == "REGISTER BOOK AND POUCH" || service.ToUpper() == "TEMPORARY GRAVE MARKER" || service.ToUpper() == "ACKNOWLEDGEMENT CARDS")
                                    {
                                        cmd = "Select * from `funeral_gplgroups` where `groupname` = '" + group + "' and `service` = '" + service + "';";
                                        dx = G1.get_db_data(cmd);
                                        if (dx.Rows.Count > 0)
                                        {
                                            //                        dt.Rows[i]["type"] = dx.Rows[0]["type"].ObjToString();
                                            currentPrice = dx.Rows[0]["price"].ObjToDouble();
                                        }
                                    }
                                    else
                                    {
                                        if (G1.get_column_number(dt, "data") >= 0)
                                        {
                                            currentPrice = dt.Rows[i]["data"].ObjToDouble();
                                            if (currentPrice == 0D && G1.get_column_number(dt, "price") >= 0)
                                                currentPrice = dt.Rows[i]["price"].ObjToDouble();
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (service.ToUpper() == "REGISTER BOOK AND POUCH" || service.ToUpper() == "TEMPORARY GRAVE MARKER" || service.ToUpper() == "ACKNOWLEDGEMENT CARDS")
                                {
                                    cmd = "Select * from `funeral_gplgroups` where `groupname` = '" + group + "' and `service` = '" + service + "';";
                                    dx = G1.get_db_data(cmd);
                                    if (dx.Rows.Count > 0)
                                    {
                                        //                        dt.Rows[i]["type"] = dx.Rows[0]["type"].ObjToString();
                                        currentPrice = dx.Rows[0]["price"].ObjToDouble();
                                    }
                                }
                                else
                                {
                                    if (G1.get_column_number(dt, "data") >= 0)
                                    {
                                        currentPrice = dt.Rows[i]["data"].ObjToDouble();
                                        if (currentPrice == 0D && G1.get_column_number(dt, "price") >= 0)
                                            currentPrice = dt.Rows[i]["price"].ObjToDouble();
                                    }
                                }
                            }
                        }
                        data = dt.Rows[i]["data"].ObjToDouble();
                        price = dt.Rows[i]["price"].ObjToDouble();
                        if (price <= 0D )
                        {
                            if (service.ToUpper().IndexOf("DISCOUNT") < 0)
                                dt.Rows[i]["price"] = data;
                            if ( data == 0D && price < 0D)
                            {
                                //dt.Rows[i]["price"] = Math.Abs(price);
                                if ( service.ToUpper() != "PACKAGE DISCOUNT")
                                    dt.Rows[i]["select"] = "0";
                            }
                        }
                        dt.Rows[i]["currentprice"] = currentPrice;
                    }
                    catch (Exception ex)
                    {
                    }
                }
                CalculateDifference(dt);
            }
            catch (Exception ex)
            {
            }
        }
        /***********************************************************************************************/
        private void CalculateDifference(DataTable dt)
        {
            string type = "";
            double price = 0D;
            double currentprice = 0D;
            double difference = 0D;
            string service = "";
            string upgrade = "";
            double upgradeDifference = 0D;
            double totalDifference = 0D;
            double data2 = 0D;
            if (G1.get_column_number(dt, "difference") < 0)
                dt.Columns.Add("difference", Type.GetType("System.Double"));

            AddUpgrade(dt);

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    dt.Rows[i]["difference"] = 0D;
                    service = dt.Rows[i]["service"].ObjToString().ToUpper();
                    if (service == "TOTAL LISTED PRICE")
                        continue;
                    else if (service == "PACKAGE PRICE")
                        continue;
                    else if (service == "PACKAGE DISCOUNT")
                        continue;
                    type = dt.Rows[i]["type"].ObjToString().ToUpper();
                    price = dt.Rows[i]["price"].ObjToDouble();
                    data2 = dt.Rows[i]["data"].ObjToDouble();
                    if ( type == "SERVICE")
                    {
                        if (price == 0D )
                            continue;
                    }
                    currentprice = dt.Rows[i]["currentprice"].ObjToDouble();
                    if (price > currentprice)
                    {
                        dt.Rows[i]["currentprice"] = price;
                        currentprice = price;
                    }
                    if ( price < 0D )
                    {
                        price = Math.Abs(price);
                        dt.Rows[i]["select"] = "0";
                    }
                    difference = currentprice - price;
                    upgrade = dt.Rows[i]["upgrade"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(upgrade))
                    {
                        upgradeDifference = upgrade.ObjToDouble();
                        if (upgradeDifference > 0D)
                        {
                            difference = upgradeDifference;
                            dt.Rows[i]["price"] = upgradeDifference;
                            difference = 0D;
                        }
                    }
                    if (type == "CASH ADVANCE")
                        difference = 0D;
                    dt.Rows[i]["difference"] = difference;
                    totalDifference += difference;
                }
                catch (Exception ex)
                {
                }
            }
        }
        /***********************************************************************************************/
        public static void AddUpgrade(DataTable dt)
        {
            if (G1.get_column_number(dt, "upgrade") >= 0)
                return;
            dt.Columns.Add("upgrade");
        }
        /***********************************************************************************************/
        private void MatchServices(DataTable dt, bool retain = false, bool protectExisting = true)
        {
            if (workDt == null)
                return;
            if (String.IsNullOrWhiteSpace(workContract))
                return;
            double data = 0D;
            double price = 0D;
            string service = "";
            string select = "";
            string ignore = "";
            string who = "";
            string deleted = "";
            string isPackage = "";
            DataRow[] dR = null;
            if (workDt.Rows.Count > 0)
            {
                if (!retain)
                {
                    for (int i = 0; i < workDt.Rows.Count; i++)
                        workDt.Rows[i]["select"] = "0";
                }
                try
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        isPackage = dt.Rows[i]["isPackage"].ObjToString();
                        service = dt.Rows[i]["service"].ObjToString();
                        ignore = dt.Rows[i]["ignore"].ObjToString();
                        who = dt.Rows[i]["who"].ObjToString();
                        deleted = dt.Rows[i]["deleted"].ObjToString();
                        if (service.Trim().ToUpper() == "URN CREDIT")
                        {
                        }
                        dR = workDt.Select("service='" + service + "'");
                        if (dR.Length > 0)
                        {
                            select = dt.Rows[i]["select"].ObjToString();
                            dt.Rows[i]["select"] = select;
                            data = dR[0]["data"].ObjToDouble();
                            price = dR[0]["price"].ObjToDouble();
                            if (select == "1")
                                price = dt.Rows[i]["price"].ObjToDouble();
                            if (price <= 0D)
                            {
                                if (service.ToUpper() != "PACKAGE DISCOUNT")
                                    price = data;
                            }
                            //                        dt.Rows[i]["cost"] = data;
                            dt.Rows[i]["price"] = price;
                            dt.Rows[i]["data"] = data;
                            dt.Rows[i]["price1"] = price;
                            dt.Rows[i]["DELETED"] = deleted;
                            dt.Rows[i]["ignore"] = ignore;
                            dt.Rows[i]["who"] = who;
                        }
                    }
                }
                catch (Exception ex)
                {

                }
            }

            if (retain)
            {
                try
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        isPackage = dt.Rows[i]["isPackage"].ObjToString();
                        select = dt.Rows[i]["select"].ObjToString();
                        if (select == "1")
                        {
                            service = dt.Rows[i]["service"].ObjToString();
                            dR = workDt.Select("service='" + service + "'");
                            if (dR.Length <= 0)
                            {
                                DataRow newRow = workDt.NewRow();
                                newRow["select"] = "1";
                                newRow["service"] = service;
                                newRow["isPackage"] = isPackage;
                                data = dt.Rows[i]["data"].ObjToDouble();
                                price = dt.Rows[i]["price"].ObjToDouble();
                                if (price <= 0D)
                                    price = data;
                                newRow["type"] = dt.Rows[i]["type"].ObjToString();
                                newRow["record1"] = dt.Rows[i]["record"].ObjToInt64();
                                //                            newRow["cost"] = data;
                                newRow["price"] = price;
                                newRow["data"] = data;
                                newRow["price1"] = price;
                                workDt.Rows.Add(newRow);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {

                }
            }
            if (!protectExisting)
                return;
            bool loadRow = false;
            try
            {
                for (int i = 0; i < workDt.Rows.Count; i++)
                {
                    service = workDt.Rows[i]["service"].ObjToString();
                    isPackage = workDt.Rows[i]["isPackage"].ObjToString();
                    loadRow = false;
                    if (dt.Rows.Count > 0)
                    {
                        dR = dt.Select("service='" + service + "'");
                        if (dR.Length <= 0)
                            loadRow = true;
                    }
                    else
                        loadRow = true;
                    if (loadRow)
                    {
                        DataRow newRow = dt.NewRow();
                        newRow["select"] = "1";
                        newRow["service"] = service;
                        newRow["isPackage"] = isPackage;
                        data = workDt.Rows[i]["data"].ObjToDouble();
                        price = workDt.Rows[i]["price"].ObjToDouble();
                        if (price <= 0D)
                            price = data;
                        newRow["type"] = workDt.Rows[i]["type"].ObjToString();
                        newRow["record1"] = workDt.Rows[i]["record"].ObjToInt64();
                        //                    newRow["cost"] = data;
                        newRow["price"] = price;
                        newRow["data"] = data;
                        newRow["price1"] = price;
                        dt.Rows.Add(newRow);
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
        /****************************************************************************************/
        private DataTable saveDt = null;
        private void pictureBox3_Click(object sender, EventArgs e)
        {
            //if ( showMerchandise )
            //{
            //    SelectMerchandise();
            //    return;
            //}
            if (showCashAdvanced)
            {
                DataTable dx = (DataTable)dgv.DataSource;
                DataRow dRow = dx.NewRow();
                try
                {
                    dRow["type"] = "Cash Advance";
                    dRow["price"] = 0D;
                    dRow["currentprice"] = 0D;
                    dRow["difference"] = 0D;
                    dRow["service"] = "Cash Advance";
                    dRow["select"] = "1";
                    dRow["mod"] = "1";
                    if (G1.get_column_number(dx, "contractNumber") >= 0)
                        dRow["contractNumber"] = workContract;
                    dx.Rows.Add(dRow);
                    btnSaveServices.Show();
                    btnSaveServices.Refresh();
                }
                catch (Exception ex)
                {
                }
                dgv.DataSource = dx;
                dgv.Refresh();
                return;
            }
            string group = cmbGroups.Text.Trim();
            string package = cmbPackage.Text.Trim();
            string casketGroup = cmbCasketGroup.Text.Trim();
            DataTable dt = (DataTable)dgv.DataSource;
            BackupRemoved(dt);
            selectWhat = "Service";
            if (showMerchandise)
                selectWhat = "Merchandise";
            //            Services serviceForm = new Services(group, package, true, dt, "", selectWhat );
            Services serviceForm = new Services(group, casketGroup, true, dt, "", selectWhat);
            serviceForm.SelectDone += ServiceForm_SelectDone;
            serviceForm.Show();
        }
        /***************************************************************************************/
        private DataTable removedServices = null;
        private void BackupRemoved(DataTable dt)
        {
            removedServices = null;
            DataRow[] dRows = dt.Select("mod='1' AND select='0'");
            if (dRows.Length > 0)
                removedServices = dRows.CopyToDataTable();
        }
        /***************************************************************************************/
        private void ServiceForm_SelectDone(DataTable dt, string what )
        {
            workDt = (DataTable)dgv.DataSource;

            if (G1.get_column_number(workDt, "isPackage") < 0)
                workDt.Columns.Add("isPackage");
            if (G1.get_column_number(dt, "isPackage") < 0)
                dt.Columns.Add("isPackage");
            if (G1.get_column_number(dt, "pSelect") < 0)
                dt.Columns.Add("pSelect");
            if (G1.get_column_number(dt, "ignore") < 0)
                dt.Columns.Add("ignore");
            if (G1.get_column_number(dt, "who") < 0)
                dt.Columns.Add("who");
            if (G1.get_column_number(dt, "DELETED") < 0)
                dt.Columns.Add("DELETED");

            if ( what.ToUpper() != "MASTER")
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i]["isPackage"] = "P";
                    dt.Rows[i]["pSelect"] = dt.Rows[i]["select"].ObjToString();
                }
            }
            if ( what.ToUpper() == "DELETE ALL")
            {
                for ( int i=0; i<workDt.Rows.Count; i++)
                {
                    workDt.Rows[i]["DELETED"] = "D";
                }
                dt.Rows.Clear();
            }

            if ( dt.Rows.Count <= 0 )
            {
                double price = 0D;
                for ( int i=0; i<workDt.Rows.Count; i++)
                {
                    workDt.Rows[i]["select"] = "0";
                    workDt.Rows[i]["mod"] = "1";
                    price = workDt.Rows[i]["price"].ObjToDouble();
                    if (price < 0D)
                        workDt.Rows[i]["price"] = Math.Abs(price);
                }
                dgv.DataSource = workDt;
                dgv.Refresh();

                ReCalcTotal(workDt);

                btnSaveServices.Show();
                btnSaveServices.Refresh();
                funModified = true;
                return;
            }


            MatchServices(dt, false, false);
            ShowPicture(dt);
            ProcessPackage(dt);

            DetermineDiscount(dt);

            ReCalcTotal(dt);

            if (G1.get_column_number(dt, "status") < 0)
                dt.Columns.Add("status");
            if (G1.get_column_number(dt, "SerialNumber") < 0)
                dt.Columns.Add("SerialNumber");

            DetermineServices(dt);

            if (removedServices != null)
            {
                try
                {
                    for (int i = 0; i < removedServices.Rows.Count; i++)
                    {
                        dt.ImportRow(removedServices.Rows[i]);
                    }
                }
                catch (Exception ex)
                {
                }
                removedServices = null;
            }

            CleanupDuplicateCasketCodes(dt);

            ReCalcTotal(dt);
            dgv.DataSource = dt;

            ProcessPackage(dt);

            //ProtectMerchandise(workDt);

            dgv.Refresh();
            btnSaveServices.Show();
            btnSaveServices.Refresh();
            funModified = true;
        }
        private double mainPackageDiscount = 0D;
        /***********************************************************************************************/
        private void ProtectMerchandise( DataTable workDt )
        {
            if (selectWhat.ToUpper() != "SERVICE")
                return;
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow[] dRows = workDt.Select("type='Merchandise'");
            if (dRows.Length <= 0)
                return;
            DataTable workMerchandiseDt = dRows.CopyToDataTable();

            DataRow[] ddRows = null;
            string service = "";
            for (int i = 0; i < workMerchandiseDt.Rows.Count; i++)
            {
                service = workMerchandiseDt.Rows[i]["service"].ObjToString();
                ddRows = dt.Select("service='" + service + "'");
                if (ddRows.Length <= 0)
                {
                    G1.copy_dt_row(workMerchandiseDt, i, dt, dt.Rows.Count);
                }
            }
            dgv.DataSource = dt;
        }
        /***************************************************************************************/
        public static bool DoWeHavePackage(DataTable dt)
        {
            bool havePackage = false;
            string service = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                service = dt.Rows[i]["service"].ObjToString();
                if (service.Trim().ToUpper() == "TOTAL LISTED PRICE")
                {
                    havePackage = true;
                    break;
                }
            }
            return havePackage;
        }
        /***************************************************************************************/
        private void DetermineDiscount(DataTable dt)
        {
            gotPackage = false;
            string service = "";
            mainPackageDiscount = 0D;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                service = dt.Rows[i]["service"].ObjToString();
                if (service.Trim().ToUpper() == "TOTAL LISTED PRICE")
                    gotPackage = true;
                if (service.Trim().ToUpper() == "PACKAGE DISCOUNT" && gotPackage)
                {
                    mainPackageDiscount = dt.Rows[i]["price"].ObjToDouble();
                    break;
                }
            }
        }
        /***************************************************************************************/
        private void ProcessPackage(DataTable dt, bool findPackageDiscount = false)
        {
            gotPackage = false;
            string service = "";
            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    service = dt.Rows[i]["service"].ObjToString();
                    if (service.Trim().ToUpper() == "TOTAL LISTED PRICE")
                    {
                        gotPackage = true;
                        //if (G1.get_column_number(dt, "package") < 0)
                        //{
                        //    dt.Columns.Add("package");
                        //    for (int j = 0; j < dt.Rows.Count; j++)
                        //        dt.Rows[j]["package"] = "p";
                        //}
                        break;
                    }
                }
                if (gridMain.Columns.Count <= 0)
                    return;
                bool gotCurrentPrice = false;
                if (G1.get_column_number(dt, "currentprice") >= 0)
                    gotCurrentPrice = true;
                if (gotPackage)
                {
                    if (gotCurrentPrice)
                        gridMain.Columns["currentprice"].Caption = "Package Price";
                    gridMain.Columns["price"].Visible = true;
                }
                else
                {
                    if (gotCurrentPrice)
                        gridMain.Columns["currentprice"].Caption = "Current Price";
                    gridMain.Columns["price"].Visible = true;
                }
                bool gotAtneed = false;
                string cmd = "Select * from `contracts` where `contractNumber` = '" + workContract + "';";
                DataTable dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                    gotAtneed = false;
                if (workContract.ToUpper().IndexOf("SX") >= 0)
                    gotAtneed = true;
                if (gotAtneed && !gotPackage)
                {
                    if (gotCurrentPrice)
                        gridMain.Columns["currentprice"].Caption = "AtNeed Price";
                    gridMain.Columns["price"].Visible = false;
                    if (DoWeHavePackage(dt))
                        gridMain.Columns["price"].Visible = true;
                }
                else
                {
                    //if ( gotPackage )
                    //    gridMain.Columns["price"].Visible = false;
                }
            }
            catch ( Exception ex)
            {
            }
        }
        /***************************************************************************************/
        public DataTable FireEventFunServicesReturn()
        {
            DataTable dt = (DataTable)dgv.DataSource;
            return dt;
        }
        /***************************************************************************************/
        public bool FireEventFunServicesModified()
        {
            if (funModified)
                return true;
            return false;
        }
        /***************************************************************************************/
        public void FireEventSaveFunServices(bool save = false)
        {
            if (save && funModified)
            {
                DataTable dt = (DataTable)dgv.DataSource;
                SaveCustomerServices(dt);
            }
            this.Close();
        }
        /****************************************************************************************/
        public void FireEventFunServicesProtection(string contract)
        {
            string cmd = "Select * from `contracts` where `contractNumber` = '" + contract + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                string serviceId = dt.Rows[0]["serviceId"].ObjToString();
                DateTime deceasedDate = dt.Rows[0]["deceasedDate"].ObjToDateTime();
                if (deceasedDate.Year > 1000 && !String.IsNullOrWhiteSpace(serviceId))
                {
                    pictureBox3.Hide();
                    if (funModified)
                        btnSaveServices.Hide();
                    gridMain.OptionsBehavior.Editable = false;
                    gridMain.OptionsBehavior.ReadOnly = true;
                    btnSelectMerchandise.Enabled = false;
                }
                else
                {
                    if (LoginForm.administrator)
                    {
                        pictureBox3.Show();
                        if (funModified)
                            btnSaveServices.Show();
                        gridMain.OptionsBehavior.Editable = true;
                        gridMain.OptionsBehavior.ReadOnly = false;
                        btnSelectMerchandise.Enabled = true;
                    }
                }
            }
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
        /****************************************************************************************/
        private void repositoryItemCheckEdit2_Click(object sender, EventArgs e)
        {
            if (loading)
                return;
            //gridMain.RefreshData();
            //gridMain.RefreshEditor(true);
            //gridMain.PostEditor();
            if (1 == 1)
                return;
            DataRow dr = null;
            string select = "";
            int rowHandle = 0;
            int row = 0;
            DataTable dt = null;
            //gridMain.PostEditor();
            if (1 == 1)
                return;
            if ( 1 == 1)
            {
                //dt = (DataTable)dgv.DataSource;
                //DataTable dx = dt.Copy();
                //rowHandle = gridMain.FocusedRowHandle;
                //row = gridMain.GetDataSourceRowIndex(rowHandle);
                dr = gridMain.GetFocusedDataRow();
                select = dr["select"].ObjToString();
                if ( select == "1")
                {
                    //dx.Rows[row]["select"] = "0";
                    //dt.AcceptChanges();
                    //ReCalcTotal(dx);
                    //dr["select"] = "0";
                }
                else if ( select == "0")
                {
                    //dx.Rows[row]["select"] = "1";
                    //dt.AcceptChanges();
                    //ReCalcTotal(dx);
                    //dr["select"] = "1";
                }
                //dgv.DataSource = dt;
                //gridMain.PostEditor();
                return;
            }
            dt = (DataTable)dgv.DataSource;
            rowHandle = gridMain.FocusedRowHandle;
            row = gridMain.GetDataSourceRowIndex(rowHandle);
            select = dt.Rows[row]["select"].ObjToString();
            bool isChecked = true;
            if (select == "0")
                isChecked = false;

            dr = gridMain.GetFocusedDataRow();
            //select = dr["select"].ObjToString();
            if (isChecked)
            {
                dt.Rows[row]["select"] = "0";
                //dr["select"] = "0";
                //dr["mod"] = "1";
            }
            else
            {
                dt.Rows[row]["select"] = "1";
                //dr["select"] = "1";
                //dr["mod"] = "1";
            }
            gridMain.PostEditor();
            dt.Rows[row]["mod"] = "1";
            dt.AcceptChanges();
            //dgv.DataSource = dt;
            //dgv.RefreshDataSource();
            //dgv.Refresh();


            //loading = true;
            //DataRow dr = gridMain.GetFocusedDataRow();
            //string x = dr["select"].ObjToString();
            //if (x == "1")
            //    x = "0";
            //else
            //    x = "1";
            //dr["select"] = x;
            //dr["mod"] = "1";
            //DataTable dt = (DataTable)dgv.DataSource;
            //int rowHandle = gridMain.FocusedRowHandle;
            //int row = gridMain.GetDataSourceRowIndex(rowHandle);
            //dt.Rows[row]["select"] = x;
            //dt.Rows[row]["mod"] = "1";
            //dt.AcceptChanges();

            dt = (DataTable)dgv.DataSource;
            ReCalcTotal(dt);
            loading = false;
            //gridMain.RefreshEditor(true);
            //gridMain.RefreshData();
            //dgv.DataSource = dt;
            //dgv.Refresh();
            funModified = true;
            btnSaveServices.Show();
            btnSaveServices.Refresh();
        }
        /****************************************************************************************/
        private void repositoryItemCheckEdit2_Clickx(object sender, EventArgs e)
        {
            if (loading)
                return;
            loading = true;
            DataRow dr = gridMain.GetFocusedDataRow();
            string x = dr["select"].ObjToString();
            if (x == "1")
                x = "0";
            else
                x = "1";
            dr["select"] = x;
            dr["mod"] = "1";
            DataTable dt = (DataTable)dgv.DataSource;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            dt.Rows[row]["select"] = x;
            dt.Rows[row]["mod"] = "1";
            dt.AcceptChanges();
            ReCalcTotal(dt);
            loading = false;
            gridMain.RefreshEditor(true);
            gridMain.RefreshData();
            dgv.DataSource = dt;
            dgv.Refresh();
            funModified = true;
            btnSaveServices.Show();
            btnSaveServices.Refresh();
        }
        /***********************************************************************************************/
        private void ReCalcTotal(DataTable dt)
        {
            if (dt == null)
                return;
            if (dt.Rows.Count <= 0)
                return;
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

            gotPackage = DoWeHavePackage(dt);
            string service = "";
            PreProcessUrns(dt);
            double upgrade = 0D;

            AddUpgrade(dt);

            double totalServices = 0D;
            double totalMerchandise = 0D;
            double totalCashAdvance = 0D;

            bool myPackage = GetPackageDetails(dt, ref totalListedPrice, ref packageDiscount, ref packagePrice, ref totalServices, ref totalMerchandise, ref totalCashAdvance, ref actualDiscount, ref grandTotal);
            if (myPackage)
                currentPriceColumn = "price";

            string pSelect = "";
            double urnCredit = 0D;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    deleted = dt.Rows[i]["DELETED"].ObjToString().ToUpper();
                    if (deleted == "DELETED" || deleted == "D")
                        continue;

                    select = dt.Rows[i]["select"].ObjToString();
                    service = dt.Rows[i]["service"].ObjToString();
                    type = dt.Rows[i]["type"].ObjToString().ToUpper();
                    ignore = dt.Rows[i]["ignore"].ObjToString();
                    who = dt.Rows[i]["who"].ObjToString();

                    if (myPackage)
                    {
                        isPackage = dt.Rows[i]["isPackage"].ObjToString().ToUpper();
                        if (isPackage == "P")
                        {
                            if ( service.ToUpper().IndexOf ( "URN CREDIT") >= 0 )
                            {
                                pSelect = dt.Rows[i]["pSelect"].ObjToString();
                                if (pSelect == "1")
                                    continue;
                                urnCredit = dt.Rows[i]["price"].ObjToDouble();
                            }
                            else
                                continue;
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
                        if (price < 0D)
                        {
                            price = Math.Abs(price);
                            customerDiscount += dt.Rows[i]["difference"].ObjToDouble();
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
                            customerDiscount -= price;
                        }
                        continue;
                    }
                    if (select == "1")
                    {
                        type = dt.Rows[i]["type"].ObjToString();
                        upgrade = dt.Rows[i]["upgrade"].ObjToDouble();
                        price = dt.Rows[i]["price"].ObjToDouble();
                        if (price <= 0D && upgrade <= 0D)
                            continue;
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
                if ( !myPackage )
                {
                    customerDiscount = 0D;
                    for ( int i=0; i<dt.Rows.Count; i++)
                    {
                        deleted = dt.Rows[i]["DELETED"].ObjToString().ToUpper();
                        if (deleted == "DELETED" || deleted == "D")
                            continue;

                        select = dt.Rows[i]["select"].ObjToString();
                        price = dt.Rows[i]["price"].ObjToDouble();
                        upgrade = dt.Rows[i]["upgrade"].ObjToDouble();
                        if (price <= 0D && upgrade > 0D)
                            price = upgrade;
                        if (price == 0D)
                            continue;
                        price = dt.Rows[i]["difference"].ObjToDouble();
                        if (select == "1")
                            customerDiscount = customerDiscount + price;
                    }
                }
                double totalIgnore = ignoreServices + ignoreMerchandise + ignoreCashAdvance;

                string money = G1.ReformatMoney(servicesTotal + totalServices - ignoreServices);
                txtServices.Text = money;
                txtServices.Refresh();

                money = G1.ReformatMoney(merchandiseTotal + totalMerchandise - ignoreMerchandise );
                txtMerchandise.Text = money;
                txtMerchandise.Refresh();

                money = G1.ReformatMoney(cashAdvanceTotal + totalCashAdvance - ignoreCashAdvance );
                txtCashAdvance.Text = money;
                txtCashAdvance.Refresh();

                double subtotal = servicesTotal + merchandiseTotal + cashAdvanceTotal + totalCashAdvance + totalServices + totalMerchandise - totalIgnore;
                money = G1.ReformatMoney(subtotal);
                txtSubtotal.Text = money;
                txtSubtotal.Refresh();

                double total = subtotal;
                if (gotPackage)
                {
                    money = G1.ReformatMoney(actualDiscount + totalIgnore );
                    txtDiscount.Text = money;
                    txtDiscount.Refresh();
                    total = packagePrice + cashAdvanceTotal + servicesTotal + merchandiseTotal - urnCredit;
                }
                else
                {
                    customerDiscount = G1.RoundValue(customerDiscount - totalIgnore);
                    double discount = customerDiscount * -1D;
                    money = G1.ReformatMoney(discount);
                    txtDiscount.Text = money;
                    txtDiscount.Refresh();
                    total = total + discount;
                }
                money = G1.ReformatMoney(total);
                txtTotal.Text = money;
                txtTotal.Refresh();

            }
            catch (Exception ex)
            {
            }

            ProcessPackage(dt);
        }
        /***********************************************************************************************/
        public static bool GetPackageDetails ( DataTable dx, ref double packageList, ref double packageDiscount, ref double packagePrice, ref double totalServices, ref double totalMerchandise, ref double cashAdvance, ref double actualDiscount, ref double grandTotal )
        {
            packageList = 0D;
            packageDiscount = 0D;
            packagePrice = 0D;
            totalServices = 0D;
            totalMerchandise = 0D;
            cashAdvance = 0D;
            grandTotal = 0D;

            DataRow [] dRows = dx.Select("service='Package Price'");
            if (dRows.Length <= 0)
                return false;

            dRows = dx.Select("isPackage='P'");
            if (dRows.Length <= 0)
                return false;

            string deleted = "";
            string select = "";
            string service = "";
            string type = "";
            string currentPriceColumn = "currentprice";

            double price = 0D;
            double upgrade = 0D;
            double customerDiscount = 0D;

            double unServices = 0D;
            double unMerchandise = 0D;
            double unCashAdvance = 0D;

            string isPackage = "";

            DataTable dt = dRows.CopyToDataTable();

            if (G1.get_column_number(dt, "currentprice") < 0)
            {
                if (G1.get_column_number(dt, "price1") >= 0)
                {
                    dt.Columns["price1"].ColumnName = "currentprice";
                }
                else
                    currentPriceColumn = "price";
            }

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    deleted = dt.Rows[i]["DELETED"].ObjToString().ToUpper();
                    if (deleted == "DELETED" || deleted == "D")
                        continue;

                    select = dt.Rows[i]["select"].ObjToString();
                    service = dt.Rows[i]["service"].ObjToString();
                    type = dt.Rows[i]["type"].ObjToString().ToUpper();

                    if (service.ToUpper() == "TOTAL LISTED PRICE")
                    {
                        if (select == "0")
                            continue;
                        packagePrice = dt.Rows[i]["price"].ObjToDouble();
                        packageList = packagePrice;
                        continue;
                    }
                    else if (service.ToUpper() == "PACKAGE PRICE")
                    {
                        if (select == "0")
                            continue;
                        packagePrice = dt.Rows[i]["price"].ObjToDouble();
                        continue;
                    }
                    else if (service.ToUpper() == "PACKAGE DISCOUNT")
                    {
                        if (select == "0")
                        {
                            //mainPackageDiscount = 0D;
                            continue;
                        }
                        packageDiscount = dt.Rows[i]["price"].ObjToDouble();
                        continue;
                    }

                    select = dt.Rows[i]["select"].ObjToString();
                    if (select == "0")
                    {
                        price = dt.Rows[i]["price"].ObjToDouble();
                        if (price < 0D)
                        {
                            price = Math.Abs(price);
                            customerDiscount += dt.Rows[i]["difference"].ObjToDouble();
                        }
                        isPackage = dt.Rows[i]["isPackage"].ObjToString().ToUpper();
                        if (type.ToUpper() == "SERVICE" && isPackage == "P")
                        {
                            if ( service.ToUpper().IndexOf ( "URN CREDIT") < 0 )
                                unServices += price;
                        }
                        else if (type.ToUpper() == "MERCHANDISE" && isPackage == "P")
                            unMerchandise += price;
                        else if (type.ToUpper() == "CASH ADVANCE" && isPackage == "P")
                            unCashAdvance += price;
                        continue;
                    }
                    if (select == "1")
                    {
                        type = dt.Rows[i]["type"].ObjToString();
                        upgrade = dt.Rows[i]["upgrade"].ObjToDouble();
                        price = dt.Rows[i]["price"].ObjToDouble();
                        if (price <= 0D && upgrade <= 0D)
                            continue;
                        price = dt.Rows[i][currentPriceColumn].ObjToDouble();
                        price = dt.Rows[i]["price"].ObjToDouble();
                        price = Math.Abs(price);
                        customerDiscount += dt.Rows[i]["difference"].ObjToDouble();
                        if (type.ToUpper() == "SERVICE")
                            totalServices += price;
                        else if (type.ToUpper() == "MERCHANDISE")
                            totalMerchandise += price;
                        else if (type.ToUpper() == "CASH ADVANCE")
                            cashAdvance += price;
                    }
                    else
                    {
                        type = dt.Rows[i]["type"].ObjToString().ToUpper();
                        if (type != "CASH ADVANCE")
                        {
                            upgrade = dt.Rows[i]["upgrade"].ObjToDouble();
                            price = dt.Rows[i]["price"].ObjToDouble();
                            price = Math.Abs(price);
                            if (type.ToUpper() == "SERVICE")
                                unServices += price;
                            else if (type.ToUpper() == "MERCHANDISE")
                                unMerchandise += price;
                            if (price <= 0D && upgrade <= 0D)
                                continue;
                            price = dt.Rows[i][currentPriceColumn].ObjToDouble();
                            customerDiscount += dt.Rows[i]["difference"].ObjToDouble();
                        }
                    }
                }
                catch (Exception ex)
                {
                }
            }

            actualDiscount = Math.Abs(packageDiscount) - unServices - unMerchandise - unCashAdvance;
            actualDiscount = actualDiscount * -1D;
            grandTotal = packagePrice;

            return true;
        }
        /***********************************************************************************************/
        private void ReCalcTotalxx(DataTable dt)
        {
            if (dt.Rows.Count <= 0)
                return;
            string select = "";
            double price = 0D;
            double customerDiscount = 0D;
            string type = "";
            string deleted = "";
            double servicesTotal = 0D;
            double merchandiseTotal = 0D;
            double cashAdvanceTotal = 0D;

            double totalServices = 0D;
            double totalMerchandise = 0D;
            double totalCashAdvance = 0D;

            double totalListedPrice = 0D;
            double packagePrice = 0D;
            double packageDiscount = 0D;
            double totalUnselected = 0D;
            int packageDiscountRow = -1;

            double amountUnchecked = 0D;

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

            gotPackage = DoWeHavePackage(dt);
            string service = "";
            PreProcessUrns(dt);
            double upgrade = 0D;

            AddUpgrade(dt);

            //            DataTable dt = (DataTable)dgv.DataSource;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                deleted = dt.Rows[i]["DELETED"].ObjToString().ToUpper();
                if (deleted == "DELETED" || deleted == "D")
                    continue;
                select = dt.Rows[i]["select"].ObjToString();
                if (select == "0")
                    continue;
                service = dt.Rows[i]["service"].ObjToString();
                if (service.ToUpper() == "TOTAL LISTED PRICE")
                    continue;
                else if (service.ToUpper() == "PACKAGE PRICE")
                    continue;
                else if (service.ToUpper() == "PACKAGE DISCOUNT")
                    continue;
                type = dt.Rows[i]["type"].ObjToString().ToUpper();
                price = dt.Rows[i]["price"].ObjToDouble();
                price = Math.Abs(price);
                if (type.ToUpper() == "SERVICE")
                    totalServices += price;
                else if (type.ToUpper() == "MERCHANDISE")
                    totalMerchandise += price;
                else if (type.ToUpper() == "CASH ADVANCE")
                    totalCashAdvance += price;
            }
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    deleted = dt.Rows[i]["DELETED"].ObjToString().ToUpper();
                    if (deleted == "DELETED" || deleted == "D")
                        continue;

                    select = dt.Rows[i]["select"].ObjToString();
                    service = dt.Rows[i]["service"].ObjToString();

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
                    if ( select == "0")
                    {
                        type = dt.Rows[i]["type"].ObjToString().ToUpper();
                        price = dt.Rows[i]["price"].ObjToDouble();
                        if (price < 0D)
                        {
                            price = Math.Abs(price);
                            amountUnchecked += price;
                            customerDiscount += dt.Rows[i]["difference"].ObjToDouble();
                            if (type.ToUpper() == "SERVICE")
                                servicesTotal += price;
                            else if (type.ToUpper() == "MERCHANDISE")
                                merchandiseTotal += price;
                            else if (type.ToUpper() == "CASH ADVANCE")
                                cashAdvanceTotal += price;
                        }
                        continue;
                    }
                    if (select == "1")
                    {
                        type = dt.Rows[i]["type"].ObjToString();
                        upgrade = dt.Rows[i]["upgrade"].ObjToDouble();
                        price = dt.Rows[i]["price"].ObjToDouble();
                        if (price <= 0D && upgrade <= 0D )
                            continue;
                        price = dt.Rows[i][currentPriceColumn].ObjToDouble();
                        if ( gotPackage )
                            price = dt.Rows[i]["price"].ObjToDouble();
                        customerDiscount += dt.Rows[i]["difference"].ObjToDouble();
                        if (type.ToUpper() == "SERVICE")
                            servicesTotal += price;
                        else if (type.ToUpper() == "MERCHANDISE")
                            merchandiseTotal += price;
                        else if (type.ToUpper() == "CASH ADVANCE")
                            cashAdvanceTotal += price;
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
                            if (type.ToUpper() == "SERVICE")
                                servicesTotal += price;
                            else if (type.ToUpper() == "MERCHANDISE")
                                merchandiseTotal += price;
                            else if (type.ToUpper() == "CASH ADVANCE")
                                cashAdvanceTotal += price;
                            totalUnselected += price;
                        }
                    }
                }
                catch ( Exception ex)
                {
                }
            }
            try
            {
                servicesTotal = totalServices;
                merchandiseTotal = totalMerchandise;
                cashAdvanceTotal = totalCashAdvance;

                string money = G1.ReformatMoney(servicesTotal);
                txtServices.Text = money;
                txtServices.Refresh();
                money = G1.ReformatMoney(merchandiseTotal);
                txtMerchandise.Text = money;
                txtMerchandise.Refresh();
                money = G1.ReformatMoney(cashAdvanceTotal);
                txtCashAdvance.Text = money;
                txtCashAdvance.Refresh();
                double subtotal = servicesTotal + merchandiseTotal + cashAdvanceTotal;
                money = G1.ReformatMoney(subtotal);
                txtSubtotal.Text = money;
                txtSubtotal.Refresh();
                double total = subtotal;
                if (gotPackage)
                    total = packagePrice;
                total = totalServices + totalMerchandise + totalCashAdvance + packageDiscount;
                money = G1.ReformatMoney(total);
                txtTotal.Text = money;
                txtTotal.Refresh();
                if (gotPackage)
                {
                    if (amountUnchecked <= Math.Abs(mainPackageDiscount))
                    {
                        //customerDiscount = mainPackageDiscount + totalUnselected;
                        money = G1.ReformatMoney(mainPackageDiscount + amountUnchecked);
                        txtDiscount.Text = money;
                        if (packageDiscountRow >= 0)
                            dt.Rows[packageDiscountRow]["price"] = mainPackageDiscount + amountUnchecked;
                        UpdatePackageDiscount(dt, mainPackageDiscount + amountUnchecked);
                        int row = gridMain.FocusedRowHandle;
                        try
                        {
                            //if (G1.get_column_number(dt, "currentprice") < 0)
                            //    dt.Columns.Add("currentprice");
                            dgv.DataSource = dt;
                        }
                        catch (Exception ex)
                        {
                        }
                        gridMain.FocusedRowHandle = row;
                    }
                    else if (totalUnselected <= Math.Abs(mainPackageDiscount))
                    {
                        //customerDiscount = mainPackageDiscount + totalUnselected;
                        money = G1.ReformatMoney(mainPackageDiscount + totalUnselected);
                        txtDiscount.Text = money;
                        if (packageDiscountRow >= 0)
                            dt.Rows[packageDiscountRow]["price"] = mainPackageDiscount + totalUnselected;
                        UpdatePackageDiscount(dt, mainPackageDiscount + totalUnselected);
                        int row = gridMain.FocusedRowHandle;
                        try
                        {
                            //if (G1.get_column_number(dt, "currentprice") < 0)
                            //    dt.Columns.Add("currentprice");
                            dgv.DataSource = dt;
                        }
                        catch ( Exception ex)
                        {
                        }
                        gridMain.FocusedRowHandle = row;
                    }
                    if (amountUnchecked <= Math.Abs(mainPackageDiscount))
                    {
                        //customerDiscount = mainPackageDiscount + totalUnselected;
                        money = G1.ReformatMoney(mainPackageDiscount + totalUnselected);
                        txtDiscount.Text = money;
                        if (packageDiscountRow >= 0)
                            dt.Rows[packageDiscountRow]["price"] = mainPackageDiscount + totalUnselected;
                        UpdatePackageDiscount(dt, mainPackageDiscount + totalUnselected);
                        int row = gridMain.FocusedRowHandle;
                        try
                        {
                            //if (G1.get_column_number(dt, "currentprice") < 0)
                            //    dt.Columns.Add("currentprice");
                            dgv.DataSource = dt;
                        }
                        catch (Exception ex)
                        {
                        }
                        gridMain.FocusedRowHandle = row;
                    }
                    else
                    {
                        customerDiscount = 0D;
                        money = G1.ReformatMoney(customerDiscount);
                        txtDiscount.Text = money;
                        UpdatePackageDiscount(dt, customerDiscount);
                        int row = gridMain.FocusedRowHandle;
                        try
                        {
                            dgv.DataSource = dt;
                        }
                        catch ( Exception ex)
                        {
                        }
                        gridMain.FocusedRowHandle = row;
                    }
                }
                else
                {
                    money = G1.ReformatMoney(customerDiscount);
                    txtDiscount.Text = money;
                }
                txtDiscount.Refresh();

                customerDiscount = Math.Abs(customerDiscount);
                if (gotPackage)
                    customerDiscount = customerDiscount - totalUnselected;
                money = G1.ReformatMoney(subtotal - customerDiscount);

                total = txtSubtotal.Text.ObjToDouble();
                customerDiscount = txtDiscount.Text.ObjToDouble();
                customerDiscount = Math.Abs(customerDiscount);
                total = total - customerDiscount;
                if ( gotPackage )
                {
                    total = packagePrice + (subtotal - totalListedPrice);
                    if (total < packagePrice)
                        total = packagePrice;
                    total = totalServices + totalMerchandise + totalCashAdvance + packageDiscount;
                }
                money = G1.ReformatMoney(total);
                txtTotal.Text = money;
                txtTotal.Refresh();
            }
            catch (Exception ex)
            {
            }

            ProcessPackage(dt);
        }
        private void ReCalcTotalx(DataTable dt)
        {
            if (dt.Rows.Count <= 0)
                return;
            string select = "";
            double price = 0D;
            double customerDiscount = 0D;
            string type = "";
            string deleted = "";
            double servicesTotal = 0D;
            double merchandiseTotal = 0D;
            double cashAdvanceTotal = 0D;

            double totalServices = 0D;
            double totalMerchandise = 0D;
            double totalCashAdvance = 0D;

            double totalListedPrice = 0D;
            double packagePrice = 0D;
            double packageDiscount = 0D;
            double totalUnselected = 0D;
            int packageDiscountRow = -1;

            double amountUnchecked = 0D;

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

            gotPackage = DoWeHavePackage(dt);
            string service = "";
            PreProcessUrns(dt);
            double upgrade = 0D;

            AddUpgrade(dt);

            //            DataTable dt = (DataTable)dgv.DataSource;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                deleted = dt.Rows[i]["DELETED"].ObjToString().ToUpper();
                if (deleted == "DELETED" || deleted == "D")
                    continue;
                select = dt.Rows[i]["select"].ObjToString();
                if (select == "0")
                    continue;
                service = dt.Rows[i]["service"].ObjToString();
                if (service.ToUpper() == "TOTAL LISTED PRICE")
                    continue;
                else if (service.ToUpper() == "PACKAGE PRICE")
                    continue;
                else if (service.ToUpper() == "PACKAGE DISCOUNT")
                    continue;
                type = dt.Rows[i]["type"].ObjToString().ToUpper();
                price = dt.Rows[i]["price"].ObjToDouble();
                if (type.ToUpper() == "SERVICE")
                    totalServices += price;
                else if (type.ToUpper() == "MERCHANDISE")
                    totalMerchandise += price;
                else if (type.ToUpper() == "CASH ADVANCE")
                    totalCashAdvance += price;
            }
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    deleted = dt.Rows[i]["DELETED"].ObjToString().ToUpper();
                    if (deleted == "DELETED" || deleted == "D")
                        continue;

                    select = dt.Rows[i]["select"].ObjToString();
                    service = dt.Rows[i]["service"].ObjToString();

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
                        type = dt.Rows[i]["type"].ObjToString().ToUpper();
                        price = dt.Rows[i]["price"].ObjToDouble();
                        if (price < 0D)
                        {
                            price = Math.Abs(price);
                            amountUnchecked += price;
                            customerDiscount += dt.Rows[i]["difference"].ObjToDouble();
                            if (type.ToUpper() == "SERVICE")
                                servicesTotal += price;
                            else if (type.ToUpper() == "MERCHANDISE")
                                merchandiseTotal += price;
                            else if (type.ToUpper() == "CASH ADVANCE")
                                cashAdvanceTotal += price;
                        }
                        continue;
                    }
                    if (select == "1")
                    {
                        type = dt.Rows[i]["type"].ObjToString();
                        upgrade = dt.Rows[i]["upgrade"].ObjToDouble();
                        price = dt.Rows[i]["price"].ObjToDouble();
                        if (price <= 0D && upgrade <= 0D)
                            continue;
                        price = dt.Rows[i][currentPriceColumn].ObjToDouble();
                        if (gotPackage)
                            price = dt.Rows[i]["price"].ObjToDouble();
                        customerDiscount += dt.Rows[i]["difference"].ObjToDouble();
                        if (type.ToUpper() == "SERVICE")
                            servicesTotal += price;
                        else if (type.ToUpper() == "MERCHANDISE")
                            merchandiseTotal += price;
                        else if (type.ToUpper() == "CASH ADVANCE")
                            cashAdvanceTotal += price;
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
                            if (type.ToUpper() == "SERVICE")
                                servicesTotal += price;
                            else if (type.ToUpper() == "MERCHANDISE")
                                merchandiseTotal += price;
                            else if (type.ToUpper() == "CASH ADVANCE")
                                cashAdvanceTotal += price;
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
                string money = G1.ReformatMoney(servicesTotal);
                txtServices.Text = money;
                txtServices.Refresh();
                money = G1.ReformatMoney(merchandiseTotal);
                txtMerchandise.Text = money;
                txtMerchandise.Refresh();
                money = G1.ReformatMoney(cashAdvanceTotal);
                txtCashAdvance.Text = money;
                txtCashAdvance.Refresh();
                double subtotal = servicesTotal + merchandiseTotal + cashAdvanceTotal;
                money = G1.ReformatMoney(subtotal);
                txtSubtotal.Text = money;
                txtSubtotal.Refresh();
                double total = subtotal;
                if (gotPackage)
                    total = packagePrice;
                money = G1.ReformatMoney(total);
                txtTotal.Text = money;
                txtTotal.Refresh();
                if (gotPackage)
                {
                    if (amountUnchecked <= Math.Abs(mainPackageDiscount))
                    {
                        //customerDiscount = mainPackageDiscount + totalUnselected;
                        money = G1.ReformatMoney(mainPackageDiscount + amountUnchecked);
                        txtDiscount.Text = money;
                        if (packageDiscountRow >= 0)
                            dt.Rows[packageDiscountRow]["price"] = mainPackageDiscount + amountUnchecked;
                        UpdatePackageDiscount(dt, mainPackageDiscount + amountUnchecked);
                        int row = gridMain.FocusedRowHandle;
                        try
                        {
                            //if (G1.get_column_number(dt, "currentprice") < 0)
                            //    dt.Columns.Add("currentprice");
                            dgv.DataSource = dt;
                        }
                        catch (Exception ex)
                        {
                        }
                        gridMain.FocusedRowHandle = row;
                    }
                    else if (totalUnselected <= Math.Abs(mainPackageDiscount))
                    {
                        //customerDiscount = mainPackageDiscount + totalUnselected;
                        money = G1.ReformatMoney(mainPackageDiscount + totalUnselected);
                        txtDiscount.Text = money;
                        if (packageDiscountRow >= 0)
                            dt.Rows[packageDiscountRow]["price"] = mainPackageDiscount + totalUnselected;
                        UpdatePackageDiscount(dt, mainPackageDiscount + totalUnselected);
                        int row = gridMain.FocusedRowHandle;
                        try
                        {
                            //if (G1.get_column_number(dt, "currentprice") < 0)
                            //    dt.Columns.Add("currentprice");
                            dgv.DataSource = dt;
                        }
                        catch (Exception ex)
                        {
                        }
                        gridMain.FocusedRowHandle = row;
                    }
                    if (amountUnchecked <= Math.Abs(mainPackageDiscount))
                    {
                        //customerDiscount = mainPackageDiscount + totalUnselected;
                        money = G1.ReformatMoney(mainPackageDiscount + totalUnselected);
                        txtDiscount.Text = money;
                        if (packageDiscountRow >= 0)
                            dt.Rows[packageDiscountRow]["price"] = mainPackageDiscount + totalUnselected;
                        UpdatePackageDiscount(dt, mainPackageDiscount + totalUnselected);
                        int row = gridMain.FocusedRowHandle;
                        try
                        {
                            //if (G1.get_column_number(dt, "currentprice") < 0)
                            //    dt.Columns.Add("currentprice");
                            dgv.DataSource = dt;
                        }
                        catch (Exception ex)
                        {
                        }
                        gridMain.FocusedRowHandle = row;
                    }
                    else
                    {
                        customerDiscount = 0D;
                        money = G1.ReformatMoney(customerDiscount);
                        txtDiscount.Text = money;
                        UpdatePackageDiscount(dt, customerDiscount);
                        int row = gridMain.FocusedRowHandle;
                        try
                        {
                            dgv.DataSource = dt;
                        }
                        catch (Exception ex)
                        {
                        }
                        gridMain.FocusedRowHandle = row;
                    }
                }
                else
                {
                    money = G1.ReformatMoney(customerDiscount);
                    txtDiscount.Text = money;
                }
                txtDiscount.Refresh();

                customerDiscount = Math.Abs(customerDiscount);
                if (gotPackage)
                    customerDiscount = customerDiscount - totalUnselected;
                money = G1.ReformatMoney(subtotal - customerDiscount);

                total = txtSubtotal.Text.ObjToDouble();
                customerDiscount = txtDiscount.Text.ObjToDouble();
                customerDiscount = Math.Abs(customerDiscount);
                total = total - customerDiscount;
                if (gotPackage)
                {
                    total = packagePrice + (subtotal - totalListedPrice);
                    if (total < packagePrice)
                        total = packagePrice;
                }
                money = G1.ReformatMoney(total);
                txtTotal.Text = money;
                txtTotal.Refresh();
            }
            catch (Exception ex)
            {
            }

            ProcessPackage(dt);
        }
        /***********************************************************************************************/
        private void UpdatePackageDiscount( DataTable dt, double discount )
        {
            DataRow[] dRows = dt.Select("service='Package Discount'");
            if (dRows.Length > 0)
                dRows[0]["price"] = discount;
        }
        /***********************************************************************************************/
        public static void CleanupDuplicateCasketCodes(DataTable dt)
        {
            string service = "";
            string newService = "";
            string str = "";
            string type = "";
            bool gotit = false;
            string[] Lines = null;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    service = dt.Rows[i]["service"].ObjToString();
                    if (String.IsNullOrWhiteSpace(service))
                        continue;
                    type = dt.Rows[i]["type"].ObjToString().ToUpper();
                    if (type == "MERCHANDISE")
                    {
                        Lines = service.Split(' ');
                        if (Lines.Length > 0)
                        {
                            newService = "";
                            gotit = false;
                            string firstWord = "";
                            for (int j = 0; j < Lines.Length; j++)
                            {
                                str = Lines[j].Trim();
                                if (String.IsNullOrWhiteSpace(str))
                                    continue;
                                if (String.IsNullOrWhiteSpace(firstWord))
                                {
                                    firstWord = str;
                                    continue;
                                }
                                if (str.IndexOf(firstWord) >= 0)
                                    gotit = true;
                                newService += str + " ";
                            }
                            if (!gotit)
                                service = firstWord + " " + newService.Trim();
                            else
                                service = newService.Trim();
                        }
                    }
                    dt.Rows[i]["service"] = service;
                }
                catch (Exception ex)
                {
                }
            }
        }
        /***********************************************************************************************/
        public static void PreProcessUrns(DataTable dt)
        {
            DataTable dx = G1.get_db_data("Select * from `casket_master`;");

            if ( G1.get_column_number ( dt, "select") < 0 )
                dt.Columns.Add ( "select");

            bool gotUrnCredit = false;
            bool gotUrn = false;

            DataRow[] dRow = null;
            string service = "";
            string type = "";
            string casketCode = "";
            string classCode = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                type = dt.Rows[i]["type"].ObjToString();
                if (type.ToUpper() == "MERCHANDISE")
                {
                    service = dt.Rows[i]["service"].ObjToString();
                    if (service.ToUpper().IndexOf ("URN CREDIT") >= 0 )
                    {
                        if (G1.get_column_number(dt, "difference") >= 0)
                            dt.Rows[i]["difference"] = "0.00";
                        gotUrnCredit = true;
                    }
                    dRow = dx.Select("casketdesc='" + service + "'");
                    if (dRow.Length <= 0)
                        continue;
                    casketCode = dRow[0]["casketcode"].ObjToString();
                    classCode = Services.ClassifyCode(casketCode);
                    if (classCode == "URN")
                    {
                        gotUrn = true;
                        break;
                    }
                }
                else
                {
                    service = dt.Rows[i]["service"].ObjToString();
                    if (service.ToUpper().IndexOf("URN CREDIT") >= 0)
                    {
                        if (G1.get_column_number(dt, "difference") >= 0)
                            dt.Rows[i]["difference"] = "0.00";
                        gotUrnCredit = true;
                    }
                }
            }
            if (gotUrn && gotUrnCredit)
            {
                dRow = dt.Select("service LIKE '%URN CREDIT'");
                if (dRow.Length > 0)
                {
                    //dRow[0]["price"] = "0.00";
                    //dRow[0]["DELETED"] = "D";
                    dRow[0]["select"] = "0";
                }
            }
        }
        /***********************************************************************************************/
        private void LoadGroupCombo()
        {
            string cmd = "Select * from `funeral_groups` ORDER BY `order`,`record`;";
            DataTable dt = G1.get_db_data(cmd);
            string groupname = "";
            string firstGroup = "";
            cmbGroups.Items.Clear();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                groupname = dt.Rows[i]["shortname"].ObjToString();
                cmbGroups.Items.Add(groupname);
                if (String.IsNullOrWhiteSpace(firstGroup))
                    firstGroup = groupname;
            }
            cmbGroups.Text = firstGroup;
            if (!String.IsNullOrWhiteSpace(EditCust.activeFuneralHomeGroup))
                cmbGroups.Text = EditCust.activeFuneralHomeGroup;
        }
        /***********************************************************************************************/
        private void LoadPackagesCombo()
        {
            string group = GetGroup();
            if (String.IsNullOrWhiteSpace(group))
                return;
            string cmd = "Select * from `packages` where `groupname` = '" + group + "' GROUP BY `PackageName`;";
            DataTable dt = G1.get_db_data(cmd);
            string firstPackage = "";
            string package = "";
            cmbPackage.Items.Clear();
            if (group.ToUpper() != "MASTER")
                cmbPackage.Items.Add("Master");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                package = dt.Rows[i]["PackageName"].ObjToString();
                if (String.IsNullOrWhiteSpace(package))
                    continue;
                if (String.IsNullOrWhiteSpace(firstPackage))
                    firstPackage = package;
                cmbPackage.Items.Add(package);
            }
            cmbPackage.Text = firstPackage;
            cmbPackage.Text = "Master";
        }
        /***********************************************************************************************/
        private void LoadCasketGroupCombo()
        {
            string cmd = "Select * from `casket_groups` ORDER BY `order`,`record`;";
            DataTable dt = G1.get_db_data(cmd);
            string groupname = "";
            string name = "";
            string locationCode = "";
            string str = "";
            cmbCasketGroup.Items.Clear();
            string firstGroup = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                groupname = dt.Rows[i]["shortname"].ObjToString();
                if (String.IsNullOrWhiteSpace(firstGroup))
                    firstGroup = groupname;
                cmbCasketGroup.Items.Add(groupname);
            }
            cmbCasketGroup.Text = firstGroup;
            if (!String.IsNullOrWhiteSpace(EditCust.activeFuneralHomeCasketGroup))
                cmbCasketGroup.Text = EditCust.activeFuneralHomeCasketGroup;
        }
        /***********************************************************************************************/
        private string GetGroup()
        {
            string location = cmbGroups.Text;
            if (location.ToUpper() == "MASTER")
                return location;
            return location;
        }
        /****************************************************************************************/
        private void cmbGroups_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loading)
                return;

            CheckForSaving();

            string group = GetGroup();
            string cmd = "Select * from `packages` where `groupname` = '" + group + "' GROUP BY `PackageName`;";
            DataTable dt = G1.get_db_data(cmd);
            cmbPackage.Items.Clear();
            string packageName = "";
            string firstPackage = "";
            loading = true;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                packageName = dt.Rows[i]["PackageName"].ObjToString();
                if (String.IsNullOrWhiteSpace(firstPackage))
                    firstPackage = packageName;
                cmbPackage.Items.Add(packageName);
            }
            if (!String.IsNullOrWhiteSpace(firstPackage))
            {
                cmbPackage.Text = firstPackage;
                LoadData();
                //cmd = "Select * from `packages` where `PackageName` = '" + firstPackage + "';";
                //dt = G1.get_db_data(cmd);
                //dgv.DataSource = dt;
            }
            else
            {
                cmd = "Select * from `packages` p JOIN `services` s ON p.`!serviceRecord` = s.`record` where `service` = 'xyzzyxxxx';";
                //                cmd = "Select * from `packages` where `PackageName` = 'xyzzyxxx';";
                dt = G1.get_db_data(cmd);
                dt.Columns.Add("num");
                dt.Columns.Add("mod");
                dt.Columns.Add("select");
                dt.Columns.Add("total", Type.GetType("System.Double"));
                MatchServices(dt);
                ReCalcTotal(dt);
                dgv.DataSource = dt;
            }
            loading = false;
        }
        /***********************************************************************************************/
        private void CheckForSaving()
        {
            if (String.IsNullOrWhiteSpace(workContract))
                return;
            if (!funModified)
                return;
            bool selected = false;
            string select = "";
            DataTable dt = (DataTable)dgv.DataSource;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                select = dt.Rows[i]["select"].ObjToString();
                if (select == "1")
                {
                    selected = true;
                    break;
                }
            }
            if (!selected)
                return;

            DialogResult result = DevExpress.XtraEditors.XtraMessageBox.Show("***Question***\nMerchandise has been Selected!\nWould you like to RETAIN your Selected Items?", "Select Merchandise Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            //DialogResult result = MessageBox.Show("***Question***\nMerchandise has been Selected!\nWould you like to RETAIN your Selected Items?", "Select Merchandise Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);

            //MessageBox.Show("***Question***\nMerchandise has been Selected!\nWould you like to RETAIN your Selected Items?", "Select Merchandise Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;
            MatchServices(dt, true);
        }
        /****************************************************************************************/
        private void cmbPackage_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            CheckForSaving();
            LoadData();
        }
        /****************************************************************************************/
        private void ClearColor()
        {
            showServices = false;
            showMerchandise = false;
            showCashAdvanced = false;
            showAll = false;
            btnCashAdvance.BackColor = Color.Transparent;
            btnMerchandise.BackColor = Color.Transparent;
            btnServices.BackColor = Color.Transparent;
            btnShowAll.BackColor = Color.Transparent;
            btnShowAll.Refresh();
            btnServices.Refresh();
            btnMerchandise.Refresh();
            btnCashAdvance.Refresh();
        }
        /****************************************************************************************/
        private void btnServices_Click(object sender, EventArgs e)
        {
            //if (!workFuneral)
            //    return;
            try
            {
                DataTable dt = (DataTable)dgv.DataSource;

                BandedGridView bandedView = new BandedGridView(dgv);
                BandedGridColumn column = (BandedGridColumn)bandedView.Columns.AddField("SerialNumber");

                if ( column.Visible)
                    gridMain.Columns["SerialNumber"].Visible = false;
                ClearColor();
                btnServices.BackColor = Color.Yellow;
                btnServices.Refresh();
                showServices = true;
                gridMain.RefreshData();
                dt = (DataTable)dgv.DataSource;
                ReCalcTotal(dt);
            }
            catch ( Exception ex)
            {
            }
        }
        /****************************************************************************************/
        private void btnMerchandise_Click(object sender, EventArgs e)
        {
            gridMain.Columns["SerialNumber"].Visible = true;
            ClearColor();
            btnMerchandise.BackColor = Color.Yellow;
            btnMerchandise.Refresh();
            showMerchandise = true;
            gridMain.RefreshData();
            DataTable dt = (DataTable)dgv.DataSource;
            ReCalcTotal(dt);
        }
        /****************************************************************************************/
        private void btnCashAdvance_Click(object sender, EventArgs e)
        {
            gridMain.Columns["SerialNumber"].Visible = false;
            ClearColor();
            btnCashAdvance.BackColor = Color.Yellow;
            btnCashAdvance.Refresh();
            showCashAdvanced = true;
            gridMain.RefreshData();
            DataTable dt = (DataTable)dgv.DataSource;
            ReCalcTotal(dt);
        }
        /****************************************************************************************/
        private void btnShowAll_Click(object sender, EventArgs e)
        {
            gridMain.Columns["SerialNumber"].Visible = false;
            ClearColor();
            btnShowAll.BackColor = Color.Yellow;
            btnShowAll.Refresh();
            showAll = true;
            gridMain.RefreshData();
            DataTable dt = (DataTable)dgv.DataSource;
            if (G1.get_column_number(dt, "myOrder") < 0)
                dt.Columns.Add("myOrder");
            string type = "";
            string myOrder = "1";
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                myOrder = "1";
                type = dt.Rows[i]["type"].ObjToString().ToUpper();
                if (type == "MERCHANDISE")
                    myOrder = "2";
                else if (type == "CASH ADVANCE")
                    myOrder = "3";
                dt.Rows[i]["myOrder"] = myOrder;
            }

            DataView tempview = dt.DefaultView;
            tempview.Sort = "myOrder asc";
            dt = tempview.ToTable();

            dt.Columns.Remove ("myOrder");

            dgv.DataSource = dt;
            dgv.Refresh();

            ReCalcTotal(dt);
        }
        /****************************************************************************************/
        private void gridMain_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv.DataSource;
            string type = "";
            try
            {
                type = dt.Rows[row]["type"].ObjToString().ToUpper();
            }
            catch ( Exception ex)
            {
                return;
            }
            string service = dt.Rows[row]["service"].ObjToString();
            //if (G1.get_column_number(dt, "select") >= 0)
            //{
            //    string select = dt.Rows[row]["select"].ObjToString();
            //    if (select != "1")
            //    {
            //        e.Visible = false;
            //        e.Handled = true;
            //        return;
            //    }
            //}
            if (service.ToUpper() == "PACKAGE DISCOUNT")
            {
            }
            if (G1.get_column_number(dt, "DELETED") >= 0)
            {
                if (dt.Rows[row]["DELETED"].ObjToString().ToUpper() == "D" || dt.Rows[row]["DELETED"].ObjToString().ToUpper() == "DELETED")
                {
                    e.Visible = false;
                    e.Handled = true;
                }
            }
            if (type == "CASH ADVANCE")
            {
            }
            if (showServices && type != "SERVICE")
            {
                e.Visible = false;
                e.Handled = true;
            }
            else if (showMerchandise && type != "MERCHANDISE")
            {
                e.Visible = false;
                e.Handled = true;
            }
            else if (showCashAdvanced && type != "CASH ADVANCE")
            {
                e.Visible = false;
                e.Handled = true;
                return;
            }
            double price = dt.Rows[row]["price"].ObjToDouble();
            double upgrade = dt.Rows[row]["upgrade"].ObjToDouble();
            if (showAll && price == 0D && upgrade <= 0D)
            {
                e.Visible = false;
                e.Handled = true;
            }

            if (price == 0D && type != "CASH ADVANCE")
            {
                if (gotPackage && service.ToUpper() == "PACKAGE DISCOUNT")
                    return;
                if (upgrade <= 0D)
                {
                    e.Visible = false;
                    e.Handled = true;
                }
            }
        }
        /***********************************************************************************************/
        private bool CheckForContract()
        {
            string cmd = "Select * from `lapse_list` where `contractNumber` = '" + workContract + "' AND `detail` = 'Goods and Services';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return false;
            return true;
        }
        /****************************************************************************************/
        private void btnSaveServices_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;

            SaveCustomerServices(dt);

            btnSaveServices.Hide();
            string select = "";
            string isPackage = "";
            double price = 0D;
            for ( int i=(dt.Rows.Count-1); i>=0; i--)
            {
                isPackage = dt.Rows[i]["isPackage"].ObjToString().ToUpper();
                if (isPackage == "P")
                    continue;
                select = dt.Rows[i]["select"].ObjToString();
                if (select != "1")
                {
                    price = dt.Rows[i]["price"].ObjToDouble();
                    if (price < 0D)
                        continue;
                    dt.Rows.RemoveAt(i);
                }
            }
            dgv.DataSource = dt;
        }
        /***********************************************************************************************/
        private void CleanOutSigTimes()
        {
            string cmd = "Select * from `relatives` where `contractNumber` = '" + workContract + "';";
            DataTable dt = G1.get_db_data(cmd);
            string sqlDate = "0000-00-00 00:00:00";
            string record = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                record = dt.Rows[i]["record"].ObjToString();
                G1.update_db_table("relatives", "record", record, new string[] { "sigTime", sqlDate });
            }
        }
        /***********************************************************************************************/
        private void SaveCustomerServices(DataTable dt)
        {
            if (custServicesFile.ToUpper() == "FCUST_SERVICES")
            {
                bool savedContracts = CheckForContract();
                if (savedContracts)
                {
                    DialogResult result = MessageBox.Show("Are you creating an Addendum to this already signed Goods and Services Agreement?", "Goods and Services Addendum Dialog", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    if (result == DialogResult.No)
                    {
                        MessageBox.Show("Existing Signed Contracts cannot be saved unless it is an Addendum!", "Goods and Services Addendum Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                        return;
                    }
                    else if (result == DialogResult.Cancel)
                        return;
                    MessageBox.Show("Okay, then you must obtain new signatures for the Goods and Services Agreement?", "Goods and Services Addendum Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    CleanOutSigTimes();
                }
                SaveFuneralServices(dt, savedContracts);
                return;
            }
            string service = "";
            string data = "";
            string price = "";
            string type = "";
            string record = "";
            string select = "";
            string status = "";
            string serialNumber = "";
            string casketCode = "";
            string cmd = "";
            string thisContract = workContract;
            if (String.IsNullOrWhiteSpace(primaryContract))
                primaryContract = workContract;
            try
            {
                if (primaryContract != workContract)
                {
                    cmd = "Select * from `fcontracts` where `contractNumber` = '" + workContract + "';";
                    DataTable oldDt = G1.get_db_data(cmd);
                    if (oldDt.Rows.Count > 0)
                    {
                        record = oldDt.Rows[0]["record"].ObjToString();
                        G1.delete_db_table("fcontracts", "record", record);
                    }
                    cmd = "Select * from `fcustomers` where `contractNumber` = '" + workContract + "';";
                    oldDt = G1.get_db_data(cmd);
                    if (oldDt.Rows.Count > 0)
                    {
                        record = oldDt.Rows[0]["record"].ObjToString();
                        G1.delete_db_table("fcustomers", "record", record);
                    }

                    thisContract = primaryContract;
                    CustomerDetails.CopyFromTableToTable("contracts", "fcontracts", primaryContract);
                    CustomerDetails.CopyFromTableToTable("customers", "fcustomers", primaryContract);
                    CustomerDetails.CopyFromTableToTable("contracts_extended", "fcontracts_extended", primaryContract);
                    CustomerDetails.CopyFromTableToTable("cust_extended", "fcust_extended", primaryContract);
                    cmd = "Delete from `fcust_services` where `contractNumber` = '" + primaryContract + "';";
                    G1.get_db_data(cmd);
                }
                cmd = "Delete from `" + custServicesFile + "` where `contractNumber` = '" + workContract + "';";
                G1.get_db_data(cmd);
                if (G1.get_column_number(dt, "status") < 0)
                    dt.Columns.Add("status");
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    try
                    {
                        service = dt.Rows[i]["service"].ObjToString();
                        status = dt.Rows[i]["status"].ObjToString();
                        select = dt.Rows[i]["select"].ObjToString();
                        if (select != "1")
                        {
                            if (status.ToUpper() == "IMPORTED")
                            {
                                cmd = "Select * from `" + customerFile + "` where `contractNumber` = '" + primaryContract + "';";
                                DataTable dd = G1.get_db_data(cmd);
                                if (dd.Rows.Count > 0)
                                {
                                    record = dd.Rows[0]["record"].ObjToString();
                                    casketCode = parseOutCasketCode(service);
                                    if (casketCode == dd.Rows[0]["extraItemAmtMI1"].ObjToString())
                                        G1.update_db_table(customerFile, "record", record, new string[] { "extraItemAmtMI1", casketCode + "-BAD" });
                                    else if (casketCode == dd.Rows[0]["extraItemAmtMI2"].ObjToString())
                                        G1.update_db_table(customerFile, "record", record, new string[] { "extraItemAmtMI2", casketCode + "-BAD" });
                                }
                            }
                            continue;
                        }
                        dt.Rows[i]["record"] = "0";
                        service = dt.Rows[i]["service"].ObjToString();
                        data = dt.Rows[i]["data"].ObjToString();
                        price = dt.Rows[i]["price"].ObjToString();
                        type = dt.Rows[i]["type"].ObjToString();
                        serialNumber = dt.Rows[i]["SerialNumber"].ObjToString();
                        //                double price = dt.Rows[i]["price"].ObjToDouble();
                        record = G1.create_record(custServicesFile, "service", "-1");
                        if (G1.BadRecord(custServicesFile, record))
                            continue;
                        dt.Rows[i]["record"] = record;
                        G1.update_db_table(custServicesFile, "record", record, new string[] { "service", service, "data", data, "type", type, "contractNumber", primaryContract, "price", price, "data", data, "SerialNumber", serialNumber });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("***ERROR*** On Adding a Service ! " + ex.Message.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Adding Serives " + ex.Message.ToString());
            }
            SaveCustExtended(dt);
            if (G1.get_column_number(dt, "num") < 0)
                dt.Columns.Add("num");
            G1.NumberDataTable(dt);
            funModified = false;
            if (!String.IsNullOrWhiteSpace(primaryContract))
            {
                if (primaryContract != workContract)
                {
                    //cmd = "Select * from `fcontracts` where `contractNumber` = '" + workContract + "';";
                    //DataTable oldDt = G1.get_db_data(cmd);
                    //if ( oldDt.Rows.Count > 0 )
                    //{
                    //    record = oldDt.Rows[0]["record"].ObjToString();
                    //    G1.delete_db_table("fcontracts", "record", record);
                    //}
                    //cmd = "Select * from `fcustomers` where `contractNumber` = '" + workContract + "';";
                    //oldDt = G1.get_db_data(cmd);
                    //if (oldDt.Rows.Count > 0)
                    //{
                    //    record = oldDt.Rows[0]["record"].ObjToString();
                    //    G1.delete_db_table("fcustomers", "record", record);
                    //}
                    workContract = primaryContract;
                }
            }
            NotifyContract(dt);
        }
        /***********************************************************************************************/
        private void SaveFuneralServices(DataTable dt, bool savedContracts)
        {
            string service = "";
            string data = "";
            string price = "";
            string upgrade = "";
            string type = "";
            string record = "";
            string select = "";
            string status = "";
            string serialNumber = "";
            string casketCode = "";
            string oldCasketCode1 = "";
            string oldCasketCode2 = "";
            string cmd = "";
            string mod = "";
            string changeRecord = "";
            string thisContract = workContract;
            double services = 0D;
            double merchandise = 0D;
            double cashAdvance = 0D;
            double dValue = 0D;
            string deleted = "";
            bool gotDeleted = false;
            bool removedCasketName = false;
            string isPackage = "";
            string ignore = "";
            string who = "";
            bool rv = false;
            if (G1.get_column_number(dt, "DELETED") < 0)
                dt.Columns.Add("DELETED");

            if (String.IsNullOrWhiteSpace(primaryContract))
                primaryContract = workContract;
            try
            {
                if (primaryContract != workContract)
                {
                    cmd = "Select * from `fcontracts` where `contractNumber` = '" + workContract + "';";
                    DataTable oldDt = G1.get_db_data(cmd);
                    if (oldDt.Rows.Count > 0)
                    {
                        record = oldDt.Rows[0]["record"].ObjToString();
                        G1.delete_db_table("fcontracts", "record", record);
                    }
                    cmd = "Select * from `fcustomers` where `contractNumber` = '" + workContract + "';";
                    oldDt = G1.get_db_data(cmd);
                    if (oldDt.Rows.Count > 0)
                    {
                        record = oldDt.Rows[0]["record"].ObjToString();
                        G1.delete_db_table("fcustomers", "record", record);
                    }

                    thisContract = primaryContract;
                    CustomerDetails.CopyFromTableToTable("contracts", "fcontracts", primaryContract);
                    CustomerDetails.CopyFromTableToTable("customers", "fcustomers", primaryContract);
                    CustomerDetails.CopyFromTableToTable("contracts_extended", "fcontracts_extended", primaryContract);
                    CustomerDetails.CopyFromTableToTable("cust_extended", "fcust_extended", primaryContract);
                    cmd = "Delete from `fcust_services` where `contractNumber` = '" + primaryContract + "';";
                    G1.get_db_data(cmd);
                }
                //cmd = "Delete from `" + custServicesFile + "` where `contractNumber` = '" + workContract + "';";
                //G1.get_db_data(cmd);
                if (G1.get_column_number(dt, "status") < 0)
                    dt.Columns.Add("status");
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    try
                    {
                        mod = dt.Rows[i]["mod"].ObjToString();
                        service = dt.Rows[i]["service"].ObjToString();
                        if (service.ToUpper().IndexOf("CONTRACT DATE") >= 0)
                            continue;
                        isPackage = dt.Rows[i]["isPackage"].ObjToString();
                        status = dt.Rows[i]["status"].ObjToString();
                        select = dt.Rows[i]["select"].ObjToString();
                        deleted = dt.Rows[i]["DELETED"].ObjToString();
                        ignore = dt.Rows[i]["ignore"].ObjToString();
                        who = dt.Rows[i]["who"].ObjToString();

                        if ( isPackage.ToUpper() == "P" && select == "0")
                        {
                        }

                        if (deleted.ToUpper() == "DELETED" || deleted == "D")
                        {
                            record = dt.Rows[i]["record"].ObjToString();
                            if (!String.IsNullOrWhiteSpace(record))
                            {
                                if (record != "0")
                                    G1.delete_db_table(custServicesFile, "record", record);
                            }
                            if (status.ToUpper() == "IMPORTED")
                            {
                                cmd = "Select * from `" + customerFile + "` where `contractNumber` = '" + primaryContract + "';";
                                DataTable dd = G1.get_db_data(cmd);
                                if (dd.Rows.Count > 0)
                                {
                                    record = dd.Rows[0]["record"].ObjToString();
                                    casketCode = parseOutCasketCode(service);
                                    oldCasketCode1 = dd.Rows[0]["extraItemAmtMI1"].ObjToString().ToUpper();
                                    oldCasketCode2 = dd.Rows[0]["extraItemAmtMI2"].ObjToString().ToUpper();
                                    if (casketCode.ToUpper().Contains(oldCasketCode1))
                                    {
                                        //G1.update_db_table(customerFile, "record", record, new string[] { "extraItemAmtMI1", casketCode + "-BAD" });
                                        G1.update_db_table(customerFile, "record", record, new string[] { "extraItemAmtMI1", "" });
                                    }
                                    else if (casketCode.ToUpper().Contains(oldCasketCode2))
                                    {
                                        //G1.update_db_table(customerFile, "record", record, new string[] { "extraItemAmtMI2", casketCode + "-BAD" });
                                        G1.update_db_table(customerFile, "record", record, new string[] { "extraItemAmtMI2", "" });
                                    }
                                }
                            }
                            continue;

                        }
                        if (select != "1")
                        {
                            dValue = dt.Rows[i]["price"].ObjToDouble();
                            if (dValue < 0D)
                                select = "1";
                        }
                        if (select != "1" && isPackage.ToUpper() != "P" )
                        {
                            if (status.ToUpper() == "IMPORTED")
                            {
                                cmd = "Select * from `" + customerFile + "` where `contractNumber` = '" + primaryContract + "';";
                                DataTable dd = G1.get_db_data(cmd);
                                if (dd.Rows.Count > 0)
                                {
                                    record = dd.Rows[0]["record"].ObjToString();
                                    casketCode = parseOutCasketCode(service);
                                    if (casketCode == dd.Rows[0]["extraItemAmtMI1"].ObjToString())
                                    {
                                        //G1.update_db_table(customerFile, "record", record, new string[] { "extraItemAmtMI1", casketCode + "-BAD" });
                                        G1.update_db_table(customerFile, "record", record, new string[] { "extraItemAmtMI1", "" });
                                    }
                                    else if (casketCode == dd.Rows[0]["extraItemAmtMI2"].ObjToString())
                                    {
                                        //G1.update_db_table(customerFile, "record", record, new string[] { "extraItemAmtMI2", casketCode + "-BAD" });
                                        G1.update_db_table(customerFile, "record", record, new string[] { "extraItemAmtMI2", "" });
                                    }
                                }
                            }
                            changeRecord = G1.create_record("fcust_changes", "what", "-1");
                            if (G1.BadRecord("fcust_changes", changeRecord))
                                continue;

                            if (savedContracts)
                            {
                                service = dt.Rows[i]["service"].ObjToString();
                                type = dt.Rows[i]["type"].ObjToString();
                                G1.update_db_table("fcust_changes", "record", changeRecord, new string[] { "contractNumber", workContract, "action", "Removed", "type", type, "what", service, "user", LoginForm.username, "date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") });
                            }
                            record = dt.Rows[i]["record"].ObjToString();
                            if (!String.IsNullOrWhiteSpace(record))
                                G1.delete_db_table(custServicesFile, "record", record);
                            continue;
                        }
                        //dt.Rows[i]["record"] = "0";
                        record = "";
                        service = dt.Rows[i]["service"].ObjToString();
                        type = dt.Rows[i]["type"].ObjToString();
                        if (type.ToUpper() == "MERCHANDISE")
                        {
                            if ( service.ToUpper() == "CASKET NAME" )
                            {
                                price = dt.Rows[i]["price"].ObjToString();
                                if (price == "0")
                                {
                                    record = dt.Rows[i]["record"].ObjToString();
                                    if (!String.IsNullOrWhiteSpace(record) && record != "0")
                                    {
                                        G1.delete_db_table("fcust_services", "record", record);
                                        removedCasketName = true;
                                        cmd = "Select * from `fcust_services` where `contractNumber` = '" + workContract + "' and `service` = 'Casket Price';";
                                        DataTable ddm = G1.get_db_data(cmd);
                                        if ( ddm.Rows.Count > 0 )
                                        {
                                            record = ddm.Rows[0]["record"].ObjToString();
                                            if (!String.IsNullOrWhiteSpace(record) && record != "0")
                                                G1.delete_db_table("fcust_services", "record", record);
                                        }
                                    }
                                    continue;
                                }
                            }
                        }
                        if (service.ToUpper() == "PACKAGE DISCOUNT")
                        {
                        }
                        cmd = "Select * from `fcust_services` WHERE `contractNumber` = '" + workContract + "' AND `service` LIKE '%" + service + "';";
                        DataTable ddd = G1.get_db_data(cmd);
                        if (ddd.Rows.Count > 0)
                            record = ddd.Rows[0]["record"].ObjToString();
                        data = dt.Rows[i]["data"].ObjToString();
                        price = dt.Rows[i]["price"].ObjToString();
                        upgrade = dt.Rows[i]["upgrade"].ObjToString();
                        type = dt.Rows[i]["type"].ObjToString();
                        if (type.ToUpper() == "SERVICE")
                            services += price.ObjToDouble();
                        else if (type.ToUpper() == "MERCHANDISE")
                            merchandise += price.ObjToDouble();
                        else if (type.ToUpper() == "CASH ADVANCE")
                            cashAdvance += price.ObjToDouble();

                        if (mod != "1")
                            continue;

                        serialNumber = dt.Rows[i]["SerialNumber"].ObjToString();
                        if (!String.IsNullOrWhiteSpace(serialNumber))
                        {
                            rv = updateInventory(serialNumber);
                            if (!rv)
                                serialNumber = "";
                        }
                        //                double price = dt.Rows[i]["price"].ObjToDouble();
                        if (String.IsNullOrWhiteSpace(record))
                        {
                            record = G1.create_record(custServicesFile, "service", "-1");
                            if (G1.BadRecord(custServicesFile, record))
                                continue;
                            dt.Rows[i]["record"] = record;
                        }
                        //dt.Rows[i]["record"] = record;
                        G1.update_db_table(custServicesFile, "record", record, new string[] { "service", service, "data", data, "type", type, "contractNumber", primaryContract, "price", price, "data", data, "SerialNumber", serialNumber, "upgrade", upgrade, "isPackage", isPackage, "pSelect", select, "ignore", ignore, "who", LoginForm.username });

                        if (savedContracts)
                        {
                            changeRecord = G1.create_record("fcust_changes", "what", "-1");
                            if (G1.BadRecord("fcust_changes", changeRecord))
                                continue;
                            G1.update_db_table("fcust_changes", "record", changeRecord, new string[] { "contractNumber", workContract, "action", "Added", "type", type, "what", service, "user", LoginForm.username, "date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") });
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("***ERROR*** On Adding a Service ! " + ex.Message.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Adding Serives " + ex.Message.ToString());
            }
            SaveCustExtended(dt);
            if (G1.get_column_number(dt, "num") < 0)
                dt.Columns.Add("num");

            dt = CleanupDeleted(dt);

            G1.NumberDataTable(dt);
            funModified = false;
            if (!String.IsNullOrWhiteSpace(primaryContract))
            {
                if (primaryContract != workContract)
                {
                    //cmd = "Select * from `fcontracts` where `contractNumber` = '" + workContract + "';";
                    //DataTable oldDt = G1.get_db_data(cmd);
                    //if ( oldDt.Rows.Count > 0 )
                    //{
                    //    record = oldDt.Rows[0]["record"].ObjToString();
                    //    G1.delete_db_table("fcontracts", "record", record);
                    //}
                    //cmd = "Select * from `fcustomers` where `contractNumber` = '" + workContract + "';";
                    //oldDt = G1.get_db_data(cmd);
                    //if (oldDt.Rows.Count > 0)
                    //{
                    //    record = oldDt.Rows[0]["record"].ObjToString();
                    //    G1.delete_db_table("fcustomers", "record", record);
                    //}
                    workContract = primaryContract;
                }
            }

            //if (matchedSSNs)
            CleanupOldMerchandise (dt);
            bool gotOne = CleanupDeletedServices(dt);
            cmd = "Select * from `fcontracts` where `contractNumber` = '" + workContract + "';";
            DataTable newDt = G1.get_db_data(cmd);
            if (newDt.Rows.Count > 0)
            {
                services = G1.RoundValue(services);
                merchandise = G1.RoundValue(merchandise);
                cashAdvance = G1.RoundValue(cashAdvance);
                record = newDt.Rows[0]["record"].ObjToString();
                G1.update_db_table("fcontracts", "record", record, new string[] { "serviceTotal", services.ToString(), "merchandiseTotal", merchandise.ToString(), "cashAdvance", cashAdvance.ToString() });
            }
            NotifyContract(dt);
            dgv.DataSource = dt;
        }
        /****************************************************************************************/
        private bool CleanupDeletedServices ( DataTable dt)
        {
            bool gotOne = false;
            string deleted = "";
            for ( int i=(dt.Rows.Count-1); i>=0; i--)
            {
                deleted = dt.Rows[i]["DELETED"].ObjToString().ToUpper();
                if ( deleted == "DELETED" || deleted == "D" )
                {
                    gotOne = true;
                    dt.Rows.RemoveAt(i);
                }
            }
            return gotOne;
        }
        /****************************************************************************************/
        private void CleanupOldMerchandise( DataTable dt )
        {
            string cmd = "Select * from `fcustomers` WHERE `contractNumber` = '" + workContract + "';";
            DataTable custDt = G1.get_db_data(cmd);
            if (custDt.Rows.Count > 0)
            {
                string record = custDt.Rows[0]["record"].ObjToString();
                if (!String.IsNullOrWhiteSpace(record))
                {
                    string casketName = custDt.Rows[0]["extraItemAmtMI1"] + "-BAD";
                    string vaultName = custDt.Rows[0]["extraItemAmtMI2"] + "-BAD";
                    G1.update_db_table("fcustomers", "record", record, new string[] { "extraItemAmtMI1", casketName, "extraItemAmtMI2", vaultName });
                }

                CleanupOldCasketInfo(dt, workContract, "Casket Name");
                CleanupOldCasketInfo(dt, workContract, "Casket Price");
                CleanupOldCasketInfo(dt, workContract, "Outer Container Name");
                CleanupOldCasketInfo(dt, workContract, "Outer Container Price");
            }
        }
        /***********************************************************************************************/
        private void CleanupOldCasketInfo(DataTable dt, string contractNumber, string what)
        {
            bool gotit = false;
            string record = "";
            try
            {
                DataRow[] dRows = dt.Select("service='" + what + "'");
                if (dRows.Length > 0)
                {
                    record = dRows[0]["record"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(record) && record != "0" && record != "-1")
                    {
                        G1.delete_db_table("fcust_services", "record", record);
                        gotit = true;
                    }
                }
                if (!gotit)
                {
                    string cmd = "Select * from `fcust_services` where `contractNumber` = '" + contractNumber + "' and `service` = '" + what + "';";
                    DataTable dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                    {
                        record = dx.Rows[0]["record"].ObjToString();
                        G1.delete_db_table("fcust_services", "record", record);
                    }
                }
            }
            catch ( Exception ex)
            {
            }
        }
        /***********************************************************************************************/
        private void CleanupImported ( string contractNumber )
        {
            string cmd = "Select * from `fcustomers` where `contractNumber` = '" + contractNumber + "';";
            DataTable dd = G1.get_db_data(cmd);
            if (dd.Rows.Count > 0)
            {
                string record = dd.Rows[0]["record"].ObjToString();
                G1.update_db_table(customerFile, "record", record, new string[] { "extraItemAmtMI1", "" });
                G1.update_db_table(customerFile, "record", record, new string[] { "extraItemAmtMI2", "" });
            }
        }
    /***********************************************************************************************/
    private DataTable CleanupDeleted ( DataTable dt)
        {
            string deleted = "";
            for (int i = (dt.Rows.Count-1); i >= 0; i--)
            {
                try
                {
                    deleted = dt.Rows[i]["DELETED"].ObjToString();
                    if (deleted.ToUpper() == "DELETED" || deleted == "D")
                        dt.Rows.RemoveAt(i);
                }
                catch (Exception ex)
                {
                }
            }
            return dt;
        }
        /***********************************************************************************************/
        private string parseOutCasketCode(string service)
        {
            if (String.IsNullOrWhiteSpace(service))
                return "";
            string[] Lines = service.Split(' ');
            if (Lines.Length > 0)
                service = Lines[0].Trim();
            return service;
        }
        /***********************************************************************************************/
        private void SaveCustExtended(DataTable dx)
        {
            double totalMerchandise = 0D;
            double totalServices = 0D;
            double totalCashAdvance = 0D;
            double merchandiseDifference = 0D;
            double serviceDifference = 0D;
            double price = 0D;
            double diff = 0D;
            string record = "";
            string thisContract = workContract;
            if (!String.IsNullOrWhiteSpace(primaryContract))
                thisContract = primaryContract;

            string str = this.txtDiscount.Text;
            str = str.Replace("$", "");
            str = str.Replace(",", "");
            double discount = str.ObjToDouble();
            double preneedDiscount = 0D;
            double packageDiscount = 0D;
            if (gotPackage)
                packageDiscount = discount;
            else
                preneedDiscount = discount;

            string cmd = "Select * from `" + extendedFile + "` where `contractNumber` = '" + primaryContract + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
            {
                record = G1.create_record(extendedFile, "pendingComment", "-1");
                if (G1.BadRecord(extendedFile, record))
                    return;
                G1.update_db_table(extendedFile, "record", record, new string[] { "contractNumber", primaryContract, "pendingComment", "" });
            }
            else
                record = dt.Rows[0]["record"].ObjToString();
            custExtendedRecord = record;
            string type = "";
            string select = "";
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                select = dx.Rows[i]["select"].ObjToString();
                if (select != "1")
                    continue;
                type = dx.Rows[i]["type"].ObjToString().ToUpper();
                price = dx.Rows[i]["price"].ObjToDouble();
                if (price == 0D)
                    continue;
                price = dx.Rows[i]["currentPrice"].ObjToDouble();
                diff = dx.Rows[i]["difference"].ObjToDouble();
                if (type == "MERCHANDISE")
                {
                    totalMerchandise += price;
                    merchandiseDifference += diff;
                }
                else if (type == "SERVICE")
                {
                    totalServices += price;
                    serviceDifference += diff;
                }
                else if (type == "CASH ADVANCE")
                    totalCashAdvance += price;
            }
            double custMerchandise = totalMerchandise - merchandiseDifference;
            double custServices = totalServices - serviceDifference;
            double custPrice = custMerchandise + custServices;
            double currentPrice = totalMerchandise + totalServices;
            double totalDiscount = merchandiseDifference + serviceDifference;

            G1.update_db_table(extendedFile, "record", record, new string[] { "custPrice", custPrice.ToString(), "custMerchandise", custMerchandise.ToString(), "custServices", custServices.ToString(), "cashAdvance", totalCashAdvance.ToString() });
            G1.update_db_table(extendedFile, "record", record, new string[] { "currentPrice", currentPrice.ToString(), "currentMerchandise", totalMerchandise.ToString(), "currentServices", totalServices.ToString() });
            G1.update_db_table(extendedFile, "record", record, new string[] { "totalDiscount", totalDiscount.ToString(), "merchandiseDiscount", merchandiseDifference.ToString(), "servicesDiscount", serviceDifference.ToString() });

            G1.update_db_table(extendedFile, "record", record, new string[] { "preneedDiscount", preneedDiscount.ToString(), "packageDiscount", packageDiscount.ToString() });

            totalModified = true;
        }
        /***********************************************************************************************/
        private void NotifyContract(DataTable dt)
        {
            for (int i = 0; i < Application.OpenForms.Count; i++)
            {
                var form = Application.OpenForms[i];
                if (form.Visible)
                {
                    string text = form.Name.ObjToString();
                    if (text.ToUpper().IndexOf("CONTRACT1") >= 0)
                    {
                        text = form.Text;
                        Contract1 editForm = (Contract1)form;
                        string contract = editForm.myWorkContract;
                        if (contract == workContract)
                        {
                            editForm.FireEventFunServicesChanged(contract, dt);
                        }
                    }
                    else if (text.ToUpper().IndexOf("FUNPAYMENTS") >= 0)
                    {
                        text = form.Text;
                        FunPayments editForm = (FunPayments)form;
                        string contract = editForm.myWorkContract;
                        if (contract == workContract)
                        {
                            editForm.FireEventFunServicesChanged(contract, dt);
                        }
                    }
                }
            }
        }
        /***********************************************************************************************/
        private DataTable LoadMasterVaults()
        {
            string casketCode = "";
            string list = "";
            string cmd = "Select * from `casket_master` where `casketcode` like 'V%';";
            DataTable dx = G1.get_db_data(cmd);
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                casketCode = dx.Rows[i]["casketcode"].ObjToString();
                if (String.IsNullOrWhiteSpace(casketCode))
                    continue;
                list += "'" + casketCode + "',";
            }
            list = list.TrimEnd(',');
            if (!String.IsNullOrWhiteSpace(list))
            {
                cmd = "Select * from `inventorylist` i where `casketcode` IN (" + list + ") ";
                cmd += ";";
            }
            else
            {
                cmd = "Select * from `inventorylist` where `casketcode` lIKE 'V%';";
            }
            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("num");
            dt.Columns.Add("mod");
            dt.Columns.Add("select");
            dt.Columns.Add("casket", Type.GetType("System.Double"));
            dt.Columns.Add("package", Type.GetType("System.Double"));
            dt.Columns.Add("total", Type.GetType("System.Double"));
            SetupSelection(dt);
            G1.NumberDataTable(dt);
            return dt;
        }
        /***********************************************************************************************/
        private DataTable LoadCasketPackage(string group)
        {
            string serviceRecord = "";
            if (String.IsNullOrWhiteSpace(group))
            {
                MessageBox.Show("***ERROR*** Empty group or package!");
                return null;
            }
            if (group.ToUpper() == "MASTER VAULT")
            {
                DataTable ddt = LoadMasterVaults();
                return ddt;
            }
            string list = "";
            string cmd = "Select * from `casket_packages` where `groupname` = '" + group + "';";
            DataTable dx = G1.get_db_data(cmd);
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                serviceRecord = dx.Rows[i]["!masterRecord"].ObjToString();
                if (String.IsNullOrWhiteSpace(serviceRecord))
                    continue;
                list += "'" + serviceRecord + "',";
            }
            list = list.TrimEnd(',');
            if (!String.IsNullOrWhiteSpace(list))
            {
                cmd = "Select * from `casket_packages` p RIGHT JOIN `casket_master` s ON p.`!masterRecord` = s.`record` JOIN `inventorylist` i ON s.`casketcode` = i.`casketcode` where s.`record` IN (" + list + ") ";
                cmd += " and `groupname` = '" + group + "' ";
                cmd += " AND i.`present` = 'Y' ";
                cmd += " ORDER BY i.`order` ";
                cmd += ";";
            }
            else
                cmd = "Select * from `casket_packages` p JOIN `casket_master` s ON p.`!masterRecord` = s.`record` where `groupname` = 'xyzzyxxxx';";
            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("num");
            dt.Columns.Add("mod");
            dt.Columns.Add("select");
            dt.Columns.Add("casket", Type.GetType("System.Double"));
            dt.Columns.Add("package", Type.GetType("System.Double"));
            CalculateCosts(dt);
            G1.NumberDataTable(dt);
            return dt;
        }
        /***********************************************************************************************/
        private void CalculateCosts(DataTable dt)
        {
            double markup = 0D;
            double casketCost = 0D;
            double packageCost = 0D;
            string cmd = "";
            string gplGroup = "";
            string masterRecord = "";
            DataTable dx = null;
            string basicRecord = "";
            string basics = "BASIC SERVICES OF FUNERAL DIRECTOR AND STAFF";
            cmd = "Select * from `services` where `service` = '" + basics + "';";
            dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
                basicRecord = dx.Rows[0]["record"].ObjToString();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                markup = dt.Rows[i]["markup"].ObjToDouble();
                masterRecord = dt.Rows[i]["!masterRecord"].ObjToString();
                cmd = "Select * from `casket_master` where `record` = '" + masterRecord + "';";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    casketCost = dx.Rows[0]["casketcost"].ObjToDouble();
                    casketCost = casketCost * markup;
                    dt.Rows[i]["casket"] = casketCost;
                    if (!String.IsNullOrWhiteSpace(basicRecord))
                    {
                        gplGroup = dt.Rows[i]["GPL_Group"].ObjToString();
                        if (!String.IsNullOrWhiteSpace(gplGroup))
                        {
                            cmd = "Select * from `packages` where `groupname` = '" + gplGroup + "' and `!serviceRecord` = '" + basicRecord + "';";
                            dx = G1.get_db_data(cmd);
                            if (dx.Rows.Count > 0)
                            {
                                packageCost = dx.Rows[0]["price"].ObjToDouble();
                                packageCost += casketCost;
                                dt.Rows[i]["package"] = packageCost;
                            }
                        }
                    }
                }
            }
        }
        /****************************************************************************************/
        private void SelectMerchandise()
        {
            string casketGroup = cmbCasketGroup.Text;
            if (String.IsNullOrWhiteSpace(casketGroup))
                return;
            this.Cursor = Cursors.WaitCursor;
            DataTable packageDt = LoadCasketPackage(casketGroup);
            if (packageDt == null)
            {
                this.Cursor = Cursors.Default;
                return;
            }
            if (packageDt.Rows.Count <= 0)
            {
                MessageBox.Show("***WARNING*** There are no items in this list!");
                this.Cursor = Cursors.Default;
                return;
            }
            workControl.WindowState = FormWindowState.Minimized;
            if (workDetached)
                this.WindowState = FormWindowState.Minimized;
            this.Cursor = Cursors.WaitCursor;
            CasketPackageView casketForm = new CasketPackageView(casketGroup, packageDt);
            casketForm.SelectDone += CasketForm_SelectDone;
            casketForm.SelectDoneAnyway += CasketForm_SelectDoneAnyway;
            casketForm.Show();
            this.Cursor = Cursors.Arrow;
        }
        /****************************************************************************************/
        private void CasketForm_SelectDoneAnyway()
        {
            workControl.WindowState = FormWindowState.Normal;
            if (workDetached)
                this.WindowState = FormWindowState.Normal;
        }
        /****************************************************************************************/
        private void SelectedVault(int position, DataTable packageDt)
        {
            string casketCode = packageDt.Rows[position]["casketcode"].ObjToString();
            string casketDesc = packageDt.Rows[position]["casketdesc"].ObjToString();
            double casketCost = packageDt.Rows[position]["casketcost"].ObjToDouble();
            string cmd = "Select * from `casket_master` where `casketcode` = '" + casketCode + "';";
            DataTable dx = G1.get_db_data(cmd);
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dRow = dt.NewRow();
            dRow["type"] = "Merchandise";
            dRow["service"] = casketDesc;
            dRow["select"] = "1";
            dRow["price"] = casketCost.ToString();
            dRow["data"] = "Casket:" + casketCode;
            dt.Rows.Add(dRow);

            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            dgv.Refresh();
            ReCalcTotal(dt);
            btnMerchandise_Click(null, null);
            funModified = true;
            btnSaveServices.Show();
            this.Refresh();
        }
        /****************************************************************************************/
        private void CasketForm_SelectDone(int position, DataTable packageDt)
        {
            string casketGroup = cmbCasketGroup.Text;
            if (String.IsNullOrWhiteSpace(casketGroup))
                return;
            if (casketGroup.ToUpper() == "MASTER VAULT")
            {
                SelectedVault(position, packageDt);
                return;
            }
            string masterRecord = packageDt.Rows[position]["!masterRecord"].ObjToString();
            double casketCost = packageDt.Rows[position]["casket"].ObjToDouble();
            double packageCost = packageDt.Rows[position]["package"].ObjToDouble();
            //double markup = packageDt.Rows[position]["markup"].ObjToDouble();
            //if (markup > 0D)
            //    casketCost = casketCost * markup;
            string str = "";

            if (String.IsNullOrWhiteSpace(masterRecord))
                return;
            DataTable dt = (DataTable)dgv.DataSource;
            string cmd = "Select * from `casket_master` where `record` = '" + masterRecord + "';";
            DataTable dx = G1.get_db_data(cmd);
            //for (int i = 0; i < dt.Rows.Count; i++)
            //{
            //    str = dt.Rows[i]["data"].ObjToString();
            //    if (str.IndexOf("Casket:") >= 0)
            //        dt.Rows[i]["select"] = "0";
            //}
            if (dx.Rows.Count > 0)
            {
                string casketCode = dx.Rows[0]["casketcode"].ObjToString();
                string casketDesc = dx.Rows[0]["casketdesc"].ObjToString();
                DataRow dRow = dt.NewRow();
                dRow["type"] = "Merchandise";
                dRow["service"] = casketDesc;
                dRow["select"] = "1";
                dRow["price"] = casketCost.ToString();
                dRow["currentprice"] = casketCost.ToString();
                dRow["difference"] = "0.00";
                dRow["data"] = "Casket:" + casketCode;
                dt.Rows.Add(dRow);

                //dRow = dt.NewRow();
                //dRow["type"] = "Merchandise";
                //dRow["service"] = "Traditional Service " + casketDesc;
                //dRow["select"] = "1";
                //dRow["price"] = (packageCost - casketCost).ToString();
                //dRow["data"] = "Casket:";
                //dt.Rows.Add(dRow);

                cmd = "Select * from `inventorylist` where `casketcode` = '" + casketCode + "';";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    Byte[] bytes = dx.Rows[0]["picture"].ObjToBytes();
                    Image myImage = emptyImage;
                    if (bytes != null)
                    {
                        myImage = G1.byteArrayToImage(bytes);
                        this.picCasket.Image = (Bitmap)myImage;
                        this.picCasket.Show();
                    }
                }

                G1.NumberDataTable(dt);
                dgv.DataSource = dt;
                dgv.Refresh();
                ReCalcTotal(dt);
                btnMerchandise_Click(null, null);
                funModified = true;
                btnSaveServices.Show();
                this.Refresh();
            }
        }
        /****************************************************************************************/
        private void ShowPicture(DataTable dx)
        {
            string casketCode = "";
            string select = "";
            bool found = false;
            if (G1.get_column_number(dx, "data") >= 0)
            {

                for (int i = 0; i < dx.Rows.Count; i++)
                {
                    select = dx.Rows[i]["select"].ObjToString();
                    if (select == "1")
                    {
                        casketCode = dx.Rows[i]["data"].ObjToString();
                        if (ShowCasket(casketCode))
                            found = true;
                    }
                }
            }
            if (!found)
                this.picCasket.Hide();
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
            //System.Diagnostics.Process.Start("https://selections.batesville.com/#/burial-solutions/caskets?selectionRoomId=32611"); // Batesville Caskets

            string showType = cmbShowType.Text.Trim();

            string title = "Casket Presentation";
            if (showType.ToUpper().IndexOf("VAULT") >= 0)
                title = "Vault Presentation";
            else if (showType.ToUpper().IndexOf("URN") >= 0)
                title = "Urn Presentation";
            string filename = "";
            string directory = "C:/SMFSdata/PDF Casket Presentations";
            string[] files = Directory.GetFiles(directory);

            DataTable dt = new DataTable("MyDataTable");
            dt.Columns.Add("MyColumn");
            string str = "";
            foreach (string value in files)
            {
                str = value;
                str = str.Replace(directory, "");
                str = str.Replace("\\", "");
                dt.Rows.Add(str);
            }

            if (dt.Rows.Count <= 0)
                return;

            string loc = "";
            DataRow[] dR = null;
            try
            {
                if (title == "Vault Presentation")
                    dR = dt.Select("MyColumn LIKE 'Vault%'");
                else if (title == "Urn Presentation")
                    dR = dt.Select("MyColumn LIKE 'Urn%'");
                else
                {
                    string cmd = "Select * from `fcust_extended` where `contractNumber` = '" + workContract + "';";
                    DataTable dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count <= 0)
                        return;
                    string serviceId = dx.Rows[0]["serviceId"].ObjToString();
                    if (String.IsNullOrWhiteSpace(serviceId))
                        return;
                    string trust = "";
                    loc = "";
                    string junk = "";
                    junk = Trust85.decodeContractNumber(serviceId, true, ref trust, ref loc);

                    dR = dt.Select("MyColumn LIKE '%" + loc + "%'");
                }

                if (dR.Length <= 0)
                {
                    MessageBox.Show("***ERROR*** Locating Presentation File for Location " + loc, "Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    return;
                }
                filename = dR[0]["MyColumn"].ObjToString();
            }
            catch (Exception ex)
            {
            }

            //string filename = "C:/Users/Robby/downloads/Casket Presentation Main Draft.pdf";
            string newFilename = directory + "/" + filename;
            if (File.Exists(newFilename))
            {
                title += " " + filename;
                ViewPDF viewForm = new ViewPDF(title, "", newFilename, true);
                viewForm.Show();
            }
        }
        /****************************************************************************************/
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            DataTable dt = (DataTable)dgv.DataSource;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            string field = e.Column.FieldName.Trim();
            if (field.ToUpper() == "SELECT")
            {
                string select = dt.Rows[row]["select"].ObjToString();
                select = e.Value.ObjToString();
                //dt.Rows[row]["select"] = select;
                //dt.AcceptChanges();
                //ReCalcTotal(dt);
                return;
            }
            if (field.ToUpper() == "PRICE" && showCashAdvanced)
            {
                double price = dr["price"].ObjToDouble();
                dr["currentprice"] = price;
                dr["difference"] = 0D;
                dt.Rows[row]["currentPrice"] = price;
                ReCalcTotal(dt);
            }
            //else if (field.ToUpper() == "SERIALNUMBER")
            //{
            //    bool foundIt = false;
            //    StackTrace stackTrace = new StackTrace();           // get call stack
            //    StackFrame[] stackFrames = stackTrace.GetFrames();
            //    int frameCount = stackTrace.FrameCount;
            //    for (int n = 0; n < frameCount; ++n)
            //    {
            //        StackFrame frame = stackTrace.GetFrame(n);
            //        if ( frame.GetMethod().ObjToString().ToUpper().Contains ( "ONKEYDOWN") )
            //        {
            //            foundIt = true;
            //            break;
            //        }
            //    }
            //    if (!foundIt)
            //        return;

            //    string serialNumber = dr[field].ObjToString();
            //    //if (String.IsNullOrWhiteSpace(serialNumber))
            //    //{
            //    //    MessageBox.Show("***ERROR*** You cannot blank out a Serial Number!", "Merchandie Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            //    //    return;
            //    //}
            //    if (serialNumber.Length < 8)
            //        return;
                
            //    string cmd = "Select * from `inventory` where `SerialNumber` = '" + serialNumber + "';";
            //    DataTable dx = G1.get_db_data(cmd);
            //    if (dx.Rows.Count <= 0)
            //    {
            //        DialogResult result = MessageBox.Show("***ERROR*** Serial Number " + serialNumber + " is not in current inventory!", "Invalid Serial Number", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            //        dr[field] = "";
            //        return;
            //    }
            //    string serviceID = dx.Rows[0]["ServiceID"].ObjToString();
            //    DateTime date = dx.Rows[0]["DateUsed"].ObjToDateTime();
            //    string location = dx.Rows[0]["LocationCode"].ObjToString();
            //    if (!String.IsNullOrWhiteSpace(serviceID) && date.Year > 100)
            //    {
            //        MessageBox.Show("***ERROR*** Serial Number (" + serialNumber + ") already in use on " + date.ToString("MM/dd/yyyy") + " at " + location + " ServiceID " + serviceID + "!", "Merchandie Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            //        dr[field] = "";
            //        return;
            //    }
            //    if (!String.IsNullOrWhiteSpace(serviceID))
            //    {
            //        MessageBox.Show("***ERROR*** Serial Number (" + serialNumber + ") already in use at " + location + " ServiceID " + serviceID + "!", "Merchandie Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            //        dr[field] = "";
            //        return;
            //    }
            //    if (date.Year > 100)
            //    {
            //        MessageBox.Show("***ERROR*** Serial Number (" + serialNumber + ") already in use on " + date.ToString("MM/dd/yyyy") + " at " + location + "!", "Merchandie Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            //        dr[field] = "";
            //        return;
            //    }

            //    deceasedDate = GetDeceasedDate(workContract);
            //    if (String.IsNullOrWhiteSpace(deceasedDate))
            //    {
            //        MessageBox.Show("***ERROR*** Current Customer is not DECEASED!\nYou cannot assign a serial number here!", "Merchandie Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            //        dr[field] = "";
            //        return;
            //    }
            //    if (String.IsNullOrWhiteSpace(serviceId))
            //    {
            //        MessageBox.Show("***ERROR*** Current Customer does not have a Service ID!\nYou cannot assign a serial number here!", "Merchandie Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            //        dr[field] = "";
            //        return;
            //    }
            //    dr["mod"] = "1";
            //    funModified = true;
            //    btnSaveServices.Show();
            //    //gridMain.RefreshData();
            //    dt = (DataTable)dgv.DataSource;
            //    ReCalcTotal(dt);
            //    return;
            //}
            funModified = true;
            btnSaveServices.Show();
            gridMain.RefreshData();
        }
        /****************************************************************************************/
        private bool updateInventory(string serialNumber)
        {
            string cmd = "Select * from `fcontracts` where `contractNumber` = '" + workContract + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return false;

            string serviceId = dt.Rows[0]["serviceId"].ObjToString();
            DateTime deceasedDate = dt.Rows[0]["deceasedDate"].ObjToDateTime();
            if (deceasedDate.Year < 100 || String.IsNullOrWhiteSpace(serviceId))
                return false;

            bool rtn = MarkInventoryAsUsed(serialNumber, DateTime.Now, deceasedDate, serviceId);
            return rtn;
        }
        /****************************************************************************************/
        private bool MarkInventoryAsUsed ( string serialNumber, DateTime dateUsed, DateTime deceasedDate, string serviceId )
        {
            string cmd = "Select * from `inventory` where `SerialNumber` = '" + serialNumber + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
                return false;
            string record = dx.Rows[0]["record"].ObjToString();
            G1.update_db_table("inventory", "record", record, new string[] { "DateUsed", dateUsed.ToString("MM/dd/yyyy"), "deceasedDate", deceasedDate.ToString("MM/dd/yyyy"), "ServiceId", serviceId });
            return true;
        }
        /****************************************************************************************/
        private void MarkInventoryAsNotUsed ( string serialNumber )
        {
            string cmd = "Select * from `inventory` where `SerialNumber` = '" + serialNumber + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
                return;
            string record = dx.Rows[0]["record"].ObjToString();
            G1.update_db_table("inventory", "record", record, new string[] { "DateUsed", "0000-00-00", "deceasedDate", "0000-00-00", "ServiceId", ""});
        }
        /****************************************************************************************/
        private bool AddNewMerchandise ( string contractNumber, string serialNumber, string service, string type )
        {
            string location = GetLocation();
            if (String.IsNullOrWhiteSpace(location))
                return false;
            string[] Locations = location.Split(' ');
            if (Locations.Length > 1)
                location = Locations[1];
            string cmd = "Select * from `casket_master` where `casketdesc` = '" + service + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
            {
                MessageBox.Show("***ERROR*** Inventory Casket Desc (" + service + ") is not available in the Casket Master!", "Merchandie Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return false;
            }
            string casketCode = dt.Rows[0]["casketcode"].ObjToString();
            if ( String.IsNullOrWhiteSpace ( casketCode))
            {
                MessageBox.Show("***ERROR*** Inventory Casket Code (" + casketCode + ") is BLANK!", "Merchandie Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return false;
            }

            cmd = "Select * from `inventorylist` where `casketcode` = '" + casketCode + "';";
            dt = G1.get_db_data(cmd);
            if ( dt.Rows.Count <= 0)
            {
                MessageBox.Show("***ERROR*** Inventory Casket Code (" + casketCode + ") not available in the Inventory List!", "Merchandie Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return false;
            }

            string casketDesc = dt.Rows[0]["casketdesc"].ObjToString();

            string record = G1.create_record("inventory", "Ownership", "-1");
            if (G1.BadRecord("inventory", record))
                return false;
            G1.update_db_table("inventory", "record", record, new string[] {"SerialNumber", serialNumber, "LocationCode", location, "CasketDescription", casketDesc, "Ownership", "Consigned" });

            DateTime now = DateTime.Now;
            string dateReceived = now.ToString("MM/dd/yyyy");
            //G1.update_db_table("inventory", "record", record, new string[] { "dateReceived", dateReceived, "DateUsed", dateReceived, "deceasedDate", deceasedDate, "ServiceId", serviceId});
            G1.update_db_table("inventory", "record", record, new string[] { "dateReceived", dateReceived });
            return true;
        }
        /***********************************************************************************************/
        private string GetDeceasedDate ( string contractNumber )
        {
            deceasedDate = "";
            serviceId = "";
            string contractFile = "contracts";
            if (DailyHistory.isInsurance(workContract))
                contractFile = "icontracts";
            if (workFuneral)
                contractFile = "fcontracts";
            string cmd = "Select * from `" + contractFile + "` where `contractNumber` = '" + contractNumber + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return "";
            DateTime date = dt.Rows[0]["deceasedDate"].ObjToDateTime();
            if (date.Year < 100)
                return "";
            deceasedDate = date.ToString("MM/dd/yyyy");
            serviceId = dt.Rows[0]["ServiceID"].ObjToString();
            return deceasedDate;
        }
        /***********************************************************************************************/
        private string GetLocation ()
        {
            string cmd = "Select * from `funeralhomes` ORDER BY `atneedcode`;";
            DataTable dt = G1.get_db_data(cmd);
            string lines = "";
            string atNeedCode = "";
            string location = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                atNeedCode = dt.Rows[i]["atneedcode"].ObjToString();
                location = dt.Rows[i]["LocationCode"].ObjToString();
                lines += "(" + atNeedCode + ")" + " " + location + "\n";
            }
            string casketLocation = "";
            using (ListSelect listForm = new ListSelect(lines, true))
            {
                listForm.ShowDialog();
                string junk = ListSelect.list_detail.Trim();
                if (!String.IsNullOrWhiteSpace(junk))
                    casketLocation = junk;
            }
            return (casketLocation);
        }
        /****************************************************************************************/
        private void btnDetach_Click(object sender, EventArgs e)
        {
            if (btnDetach.Text.ToUpper() == "PRINT")
            {
                printPreviewToolStripMenuItem_Click(null, null);
            }
            else
            {
                this.Cursor = Cursors.WaitCursor;
                FunServices funForm = new FunServices(workControl, workContract, true, workFuneral );
                funForm.Show();
                this.Cursor = Cursors.Default;
            }
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
            if (btnServices.BackColor == Color.Yellow )
                Printer.DrawQuad(6, 8, 4, 4, "Services Detail Report", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            else if ( btnMerchandise.BackColor == Color.Yellow )
                Printer.DrawQuad(6, 8, 4, 4, "Merchandise Detail Report", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            else if ( btnCashAdvance.BackColor == Color.Yellow)
                Printer.DrawQuad(6, 8, 4, 4, "Cash Advance Detail Report", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            font = new Font("Ariel", 10, FontStyle.Regular);
            Printer.DrawQuad(11, 9, 2, 3, "Contract : " + workContract, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);

            DateTime date = DateTime.Now;
            string workDate = date.Month.ToString("D2") + "/" + date.Day.ToString("D2") + "/" + (date.Year % 100).ToString("D2");

            //string str = "Report : " + workDate;

            //Printer.SetQuadSize(24, 12);
            //font = new Font("Ariel", 9, FontStyle.Regular);
            //Printer.DrawQuad(19, 8, 5, 4, str, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(16, 8, 3, 4, lblPayment.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(19, 8, 3, 4, lblTrust85.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(22, 8, 3, 4, lblTrust100.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 11, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /****************************************************************************************/
        private void gridMain_ShowingEditor(object sender, CancelEventArgs e)
        {
            GridView view = sender as GridView;
            if ( gridMain.Columns["SerialNumber"].Visible )
            {
                if (view.FocusedColumn.FieldName.ToUpper() != "SELECT" && view.FocusedColumn.FieldName.ToUpper() != "SERIALNUMBER")
                    e.Cancel = true;
                else
                    return;
            }
            if (!showCashAdvanced)
            {
                if (view.FocusedColumn.FieldName.ToUpper() != "SELECT" )
                    e.Cancel = true;
                //if (view.FocusedColumn.FieldName.ToUpper() != "SELECT" && view.FocusedColumn.FieldName.ToUpper() != "PRICE")
                //    e.Cancel = true;
            }
            else
            {
                if (view.FocusedColumn.FieldName.ToUpper() == "SELECT")
                {
                    //funModified = true;
                    //btnSaveServices.Show();
                    //btnSaveServices.Visible = true;
                    //btnSaveServices.Refresh();
                    return;
                }
                if (view.FocusedColumn.FieldName.ToUpper() != "SERVICE" && view.FocusedColumn.FieldName.ToUpper() != "PRICE")
                    e.Cancel = true;
                else
                {
                    funModified = true;
                    btnSaveServices.Show();
                    btnSaveServices.Visible = true;
                }
            }
        }
        /****************************************************************************************/
        private void gridMain_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column.FieldName.ToUpper() == "CURRENTPRICE" && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                int row = e.ListSourceRowIndex;
                DataTable dt = (DataTable)dgv.DataSource;
                string service = dt.Rows[row]["type"].ObjToString().ToUpper();
                if (service == "CASH ADVANCE")
                {
                    dt.Rows[row]["currentprice"] = dt.Rows[row]["price"].ObjToDouble();
                    dt.Rows[row]["difference"] = 0D;
                    e.DisplayText = G1.ReformatMoney(dt.Rows[row]["price"].ObjToDouble());
                }
            }
            else if (e.Column.FieldName.ToUpper() == "DIFFERENCE" && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                int row = e.ListSourceRowIndex;
                DataTable dt = (DataTable)dgv.DataSource;
                string service = dt.Rows[row]["type"].ObjToString().ToUpper();
                if (service == "CASH ADVANCE")
                {
                    dt.Rows[row]["currentprice"] = dt.Rows[row]["price"].ObjToDouble();
                    dt.Rows[row]["difference"] = 0D;
                    e.DisplayText = G1.ReformatMoney(dt.Rows[row]["difference"].ObjToDouble());
                }
            }
            else if (e.Column.FieldName.ToUpper() == "PRICE" && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                int row = e.ListSourceRowIndex;
                DataTable dt = (DataTable)dgv.DataSource;
                string type = dt.Rows[row]["type"].ObjToString().ToUpper();
                string service = dt.Rows[row]["service"].ObjToString().ToUpper();
                double price = dt.Rows[row]["price"].ObjToDouble();
                if (price < 0D && service.ToUpper() != "PACKAGE DISCOUNT")
                {
                    price = Math.Abs(price);
                    e.DisplayText = G1.ReformatMoney(price);
                }
            }
            //if (e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            //{
            //    int row = e.ListSourceRowIndex;
            //    DataTable dt = (DataTable)dgv.DataSource;
            //    if (G1.get_column_number(dt, "ignore") >= 0)
            //    {
            //        string service = dt.Rows[row]["service"].ObjToString().ToUpper();
            //        string ignore = dt.Rows[row]["ignore"].ObjToString().ToUpper();
            //        e.Column.
            //        if (ignore == "Y")
            //            e.Column.AppearanceCell.BackColor = Color.LightGray;
            //        else
            //            e.Column.AppearanceCell.BackColor = Color.Transparent;
            //    }
            //}
        }
        /***************************************************************************************/
        public delegate void d_void_ServicesClosing(string record, double amountFiled, double amountReceived);
        public event d_void_ServicesClosing servicesClosing;
        protected void OnPaymentClosing()
        {
            if (totalModified)
                servicesClosing?.Invoke(custExtendedRecord, 0D, 0D);
        }
        /****************************************************************************************/
        private void FunServices_FormClosing(object sender, FormClosingEventArgs e)
        {
            OnPaymentClosing();
            G1.CleanupDataGrid(ref dgv);
            GC.Collect();
            if ( this.picCasket.Image != null)
            {
                this.picCasket.Image.Dispose();
                this.picCasket.Image = null;
                GC.Collect();
            }
        }
        /****************************************************************************************/
        private void btnSelectMerchandise_Click(object sender, EventArgs e)
        {
            SelectMerchandise();
        }
        /****************************************************************************************/
        private bool didSummary = false;
        private bool sizeChange = false;
        private bool avoidUpdate = false;
        private void gridMain_CustomSummaryCalculate(object sender, DevExpress.Data.CustomSummaryEventArgs e)
        {
            if (avoidUpdate)
                return;
            if (sizeChange && didSummary == true)
            {
                this.Cursor = Cursors.Default;
                UnsubscribeSystemEvents();
                return;
            }
            try
            {
                didSummary = true;
                double value = e.TotalValue.ObjToDouble();
                string field = (e.Item as GridSummaryItem).FieldName.ObjToString();
                if (field.ToUpper() == "SERIALNUMBER")
                {
                    didSummary = false;
                    UnsubscribeSystemEvents();
                    return;
                }
                double price = 0D;
                double totalPrice = 0D;
                double currentPrice = 0D;
                double totalCurrentPrice = 0D;
                double difference = 0D;
                double totalDifference = 0D;
                double totalPackagePrice = 0D;
                double packagePrice = 0D;
                double packageDiscount = 0D;
                double cashAdvance = 0D;
                double upgrade = 0D;
                gotPackage = false;
                string type = "";
                string select = "";
                string service = "";
                string deleted = "";

                double unServices = 0D;
                double unMerchandise = 0D;

                DataTable dt = (DataTable)dgv.DataSource;

                if (G1.get_column_number(dt, "DELETED") < 0)
                    dt.Columns.Add("DELETED");

                double servicesTotal = 0D;
                double merchandiseTotal = 0D;

                double totalServices = 0D;
                double totalMerchandise = 0D;
                double totalCashAdvance = 0D;
                double actualDiscount = 0D;
                double totalListedPrice = 0D;
                double grandTotal = 0D;

                string isPackage = "";

                //if (dt.Rows.Count <= 0)
                //    return;

                if ( dt.Rows.Count > 0 )
                {
                }

                if (G1.get_column_number(dt, "isPackage") < 0)
                    dt.Columns.Add("isPackage");

                double urnCredit = 0D;
                string pSelect = "";


                bool myPackage = GetPackageDetails(dt, ref totalListedPrice, ref packageDiscount, ref packagePrice, ref totalServices, ref totalMerchandise, ref totalCashAdvance, ref actualDiscount, ref grandTotal);

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    deleted = dt.Rows[i]["DELETED"].ObjToString();
                    if (deleted.ToUpper() == "DELETED" || deleted.ToUpper() == "D")
                        continue;
                    type = dt.Rows[i]["type"].ObjToString().ToUpper();
                    service = dt.Rows[i]["service"].ObjToString().ToUpper();
                    if (showServices && type != "SERVICE")
                        continue;
                    if (showMerchandise && type != "MERCHANDISE")
                        continue;
                    if (showCashAdvanced && type != "CASH ADVANCE")
                        continue;
                    select = dt.Rows[i]["select"].ObjToString();
                    if (select != "1")
                        continue;
                    price = dt.Rows[i]["price"].ObjToDouble();
                    upgrade = dt.Rows[i]["upgrade"].ObjToDouble();
                    if (price <= 0D && upgrade > 0D)
                        price = upgrade;
                    if (price == 0D)
                        continue;
                    currentPrice = dt.Rows[i]["currentprice"].ObjToDouble();
                    totalCurrentPrice += currentPrice;
                    upgrade = dt.Rows[i]["upgrade"].ObjToDouble();
                    if (upgrade > 0D)
                        dt.Rows[i]["difference"] = upgrade;
                    else
                        dt.Rows[i]["difference"] = currentPrice - price;

                    if (service == "TOTAL LISTED PRICE")
                        dt.Rows[i]["difference"] = 0D;
                    else if (service == "PACKAGE PRICE")
                        dt.Rows[i]["difference"] = 0D;
                    else if (service == "PACKAGE DISCOUNT")
                        dt.Rows[i]["difference"] = 0D;

                    totalDifference += dt.Rows[i]["difference"].ObjToDouble();
                }

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    deleted = dt.Rows[i]["DELETED"].ObjToString();
                    if (deleted.ToUpper() == "DELETED" || deleted.ToUpper() == "D")
                        continue;
                    type = dt.Rows[i]["type"].ObjToString().ToUpper();
                    service = dt.Rows[i]["service"].ObjToString().ToUpper();
                    select = dt.Rows[i]["select"].ObjToString();

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
                            }
                            else
                                continue;
                        }
                    }

                    if (type.ToUpper() == "CASH ADVANCE")
                    {
                        if (select == "1")
                            cashAdvance += dt.Rows[i]["price"].ObjToDouble();
                    }
                    if (service == "TOTAL LISTED PRICE")
                    {
                        totalPackagePrice = dt.Rows[i]["price"].ObjToDouble();
                        if (packagePrice > 0)
                            gotPackage = true;
                        continue;
                    }
                    else if (service == "PACKAGE PRICE")
                    {
                        packagePrice = dt.Rows[i]["price"].ObjToDouble();
                        if (packagePrice > 0)
                            gotPackage = true;
                        continue;
                    }
                    else if (service == "PACKAGE DISCOUNT")
                    {
                        packageDiscount = dt.Rows[i]["price"].ObjToDouble();
                        packageDiscount = Math.Abs(packageDiscount);
                        dt.Rows[i]["difference"] = packageDiscount;
                        continue;
                    }

                    type = dt.Rows[i]["type"].ObjToString().ToUpper();
                    if (!showAll)
                    {
                        if (showServices && type != "SERVICE")
                            continue;
                        if (showMerchandise && type != "MERCHANDISE")
                            continue;
                        if (showCashAdvanced && type != "CASH ADVANCE")
                            continue;
                    }
                    if (select == "1")
                    {
                        upgrade = dt.Rows[i]["upgrade"].ObjToDouble();
                        price = dt.Rows[i]["price"].ObjToDouble();
                        if (price != 0D || upgrade > 0D)
                        {
                            totalPrice += dt.Rows[i]["price"].ObjToDouble();
                            if ( type == "SERVICE" && price > 0D)
                                servicesTotal += Math.Abs(price);
                            else if ( type == "MERCHANDISE" && price > 0D )
                                merchandiseTotal += Math.Abs(price);
                            //totalCurrentPrice += dt.Rows[i]["currentprice"].ObjToDouble();
                            //totalDifference += dt.Rows[i]["difference"].ObjToDouble();
                        }
                    }
                    else
                    {
                        price = dt.Rows[i]["price"].ObjToDouble();
                        if (price < 0D)
                        {
                            price = Math.Abs(price);
                            if (type == "SERVICE")
                                unServices += Math.Abs(price);
                            else if (type == "MERCHANDISE")
                                unMerchandise += Math.Abs(price);
                            totalPrice += price;
                            //totalCurrentPrice += dt.Rows[i]["currentprice"].ObjToDouble();
                            //totalDifference += dt.Rows[i]["difference"].ObjToDouble();
                        }
                    }
                }

                if (field.ToUpper() == "PRICE")
                {
                    e.TotalValueReady = true;
                    e.TotalValue = totalPrice;
                    gridMain.PostEditor();
                    if (myPackage)
                    {
                        if (showServices)
                            e.TotalValue = totalServices + servicesTotal;
                        else if (showMerchandise)
                            e.TotalValue = totalMerchandise + merchandiseTotal;
                        else if (showCashAdvanced)
                            e.TotalValue = cashAdvance + totalCashAdvance;
                        else
                        {
                            double total = packagePrice + cashAdvance + servicesTotal + merchandiseTotal - urnCredit;
                            e.TotalValue = total;

                            //e.TotalValue = packagePrice + cashAdvance;
                            //e.TotalValue = totalServices + totalMerchandise + cashAdvance - packageDiscount;
                        }
                        gridMain.PostEditor();
                        gridMain.RefreshEditor(true);
                    }
                    gridMain.PostEditor();
                    gridMain.RefreshEditor(true);
                }
                if (field.ToUpper() == "CURRENTPRICE")
                {
                    e.TotalValueReady = true;
                    e.TotalValue = totalCurrentPrice;
                    if (gotPackage)
                        e.TotalValue = totalPackagePrice + cashAdvance;
                    gridMain.PostEditor();
                    gridMain.RefreshEditor(true);
                }
                else if (field.ToUpper() == "DIFFERENCE")
                {
                    e.TotalValueReady = true;
                    e.TotalValue = totalDifference;
                    if (gotPackage)
                    {
                        e.TotalValue = packageDiscount;
                        gridMain.PostEditor();
                        gridMain.RefreshEditor(true);
                    }
                }
                e.TotalValueReady = true;
                gridMain.PostEditor();
                gridMain.RefreshEditor(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show("A critical exception has occurred while attempting to Display Totals\n" + ex.Message + "\n", "Funeral Services Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            }
            gridMain.PostEditor();
            gridMain.RefreshEditor(true);
        }
        private void gridMain_CustomSummaryCalculatexx(object sender, DevExpress.Data.CustomSummaryEventArgs e)
        {
            if (sizeChange && didSummary == true )
            {
                this.Cursor = Cursors.Default;
                UnsubscribeSystemEvents();
                return;
            }
            try
            {
                didSummary = true;
                double value = e.TotalValue.ObjToDouble();
                string field = (e.Item as GridSummaryItem).FieldName.ObjToString();
                double price = 0D;
                double totalPrice = 0D;
                double currentPrice = 0D;
                double totalCurrentPrice = 0D;
                double difference = 0D;
                double totalDifference = 0D;
                double totalPackagePrice = 0D;
                double packagePrice = 0D;
                double packageDiscount = 0D;
                double cashAdvance = 0D;
                double upgrade = 0D;
                gotPackage = false;
                string type = "";
                string select = "";
                string service = "";
                string deleted = "";

                double totalServices = 0D;
                double totalMerchandise = 0D;

                DataTable dt = (DataTable)dgv.DataSource;

                if (G1.get_column_number(dt, "DELETED") < 0)
                    dt.Columns.Add("DELETED");

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    deleted = dt.Rows[i]["DELETED"].ObjToString();
                    if (deleted.ToUpper() == "DELETED" || deleted.ToUpper() == "D")
                        continue;
                    service = dt.Rows[i]["service"].ObjToString().ToUpper();
                    select = dt.Rows[i]["select"].ObjToString();
                    type = dt.Rows[i]["type"].ObjToString().ToUpper();
                    if (type.ToUpper() == "CASH ADVANCE")
                    {
                        if (select == "1")
                            cashAdvance += dt.Rows[i]["price"].ObjToDouble();
                    }
                    if (service == "TOTAL LISTED PRICE")
                    {
                        totalPackagePrice = dt.Rows[i]["price"].ObjToDouble();
                        if (packagePrice > 0)
                            gotPackage = true;
                        continue;
                    }
                    else if (service == "PACKAGE PRICE")
                    {
                        packagePrice = dt.Rows[i]["price"].ObjToDouble();
                        if (packagePrice > 0)
                            gotPackage = true;
                        continue;
                    }
                    else if (service == "PACKAGE DISCOUNT")
                    {
                        packageDiscount = dt.Rows[i]["price"].ObjToDouble();
                        packageDiscount = Math.Abs(packageDiscount);
                        dt.Rows[i]["difference"] = packageDiscount;
                        continue;
                    }

                    type = dt.Rows[i]["type"].ObjToString().ToUpper();
                    if (!showAll)
                    {
                        if (showServices && type != "SERVICE")
                            continue;
                        if (showMerchandise && type != "MERCHANDISE")
                            continue;
                        if (showCashAdvanced && type != "CASH ADVANCE")
                            continue;
                    }
                    if (select == "1")
                    {
                        upgrade = dt.Rows[i]["upgrade"].ObjToDouble();
                        price = dt.Rows[i]["price"].ObjToDouble();
                        price = Math.Abs(price);
                        if (price != 0D || upgrade > 0D)
                        {
                            if (type == "SERVICE")
                                totalServices += price;
                            else if (type == "MERCHANDISE")
                                totalMerchandise += price;
                            totalPrice += dt.Rows[i]["price"].ObjToDouble();
                            totalCurrentPrice += dt.Rows[i]["currentprice"].ObjToDouble();
                            totalDifference += dt.Rows[i]["difference"].ObjToDouble();
                        }
                    }
                    else
                    {
                        price = dt.Rows[i]["price"].ObjToDouble();
                        if (price < 0D)
                        {
                            price = Math.Abs(price);
                            totalPrice += price;
                            totalCurrentPrice += dt.Rows[i]["currentprice"].ObjToDouble();
                            totalDifference += dt.Rows[i]["difference"].ObjToDouble();
                        }
                    }
                }

                if (field.ToUpper() == "PRICE")
                {
                    e.TotalValueReady = true;
                    e.TotalValue = totalPrice;
                    if (gotPackage)
                    {
                        e.TotalValue = packagePrice + cashAdvance;
                        if (showServices)
                            e.TotalValue = totalServices;
                        else if (showMerchandise)
                            e.TotalValue = totalMerchandise;
                        else if (showCashAdvanced)
                            e.TotalValue = cashAdvance;
                        else
                        {
                            //e.TotalValue = totalPackagePrice + cashAdvance;
                            e.TotalValue = totalServices + totalMerchandise + cashAdvance - packageDiscount;
                        }
                    }
                }
                if (field.ToUpper() == "CURRENTPRICE")
                {
                    e.TotalValueReady = true;
                    e.TotalValue = totalCurrentPrice;
                    if (gotPackage)
                    {
                        e.TotalValue = totalPackagePrice + cashAdvance;
                    }
                }
                else if (field.ToUpper() == "DIFFERENCE")
                {
                    e.TotalValueReady = true;
                    e.TotalValue = totalDifference;
                    if (gotPackage)
                    {
                        e.TotalValue = packageDiscount;
                        gridMain.PostEditor();
                    }
                }
            }
            catch ( Exception ex)
            {
            }
            gridMain.PostEditor();
            gridMain.RefreshEditor(true);
        }
        /****************************************************************************************/
        private void btnMultiSSN_Click(object sender, EventArgs e)
        {
            string cmd = "Select * from `fcustomers` where `contractNumber` = '" + workContract + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
            {
                cmd = "Select * from `customers` where `contractNumber` = '" + workContract + "';";
                dx = G1.get_db_data(cmd);
                if ( dx.Rows.Count <= 0 )
                    return;
            }
            string ssn = dx.Rows[0]["ssn"].ObjToString();

            this.Cursor = Cursors.WaitCursor;
            Duplicate_SSN dupForm = new Duplicate_SSN(workContract, ssn);
            dupForm.SelectDone += DupForm_SelectDone;
            dupForm.Show();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void DupForm_SelectDone(DataTable dt)
        {
            bool gotSomething = false;
            string name = "";
            string str = "";
            primaryContract = workContract;
            string contractNumber = "";
            string service = "";
            string sName = "";

            DataTable oldDt = (DataTable)dgv.DataSource;

            string cmd = "Select * from `fcustomers` where `contractNumber` = '" + workContract + "';";
            DataTable ddx = G1.get_db_data(cmd);
            if (ddx.Rows.Count <= 0)
                return;
            DateTime deceasedDate = ddx.Rows[0]["deceasedDate"].ObjToDateTime();
            string serviceId = ddx.Rows[0]["serviceId"].ObjToString();

            for ( int i=0; i<4; i++)
            {
                name = "C" + (i + 1).ToString();
                if (G1.get_column_number(dt, name) < 0)
                    continue;
                sName = "S" + (i + 1).ToString();
                if ( G1.get_column_number ( dt, sName ) < 0 )
                        continue;
                contractNumber = dt.Columns[sName].Caption.Trim();

                UpdateAllContractsInfo(contractNumber, deceasedDate, serviceId);

                for ( int j=0; j<dt.Rows.Count; j++)
                {
                    if ( dt.Rows[j][name].ObjToString() == "1")
                    {
                        gotSomething = true;
                        str = "S" + (i + 1).ToString();
                        service = dt.Rows[j][str].ObjToString();
                        if ( service.IndexOf("Primary-") == 0)
                        {
                            service = service.Replace("Primary-", "");
                            //primaryContract = service;
                        }
                    }
                }
            }
            if (!gotSomething)
                return;
            DataTable dx = (DataTable)dgv.DataSource;

            //if (String.IsNullOrWhiteSpace(primaryContract))
            //    primaryContract = workContract;

            DataRow dRow = null;

            cmd = "Select * from `contracts` WHERE `contractNumber` = '" + primaryContract + "';";
            DataTable newDt = G1.get_db_data(cmd); // Some Contracts actually don't exist
            if (newDt.Rows.Count <= 0)
            {
                MessageBox.Show("This Contract Number does not have a Valid Contract!", "Invalid Contract Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }

            newDt = dx.Clone();
            int lastRow = 0;
            string lastType = "";
            string lastService = "";
            string type = "";
            string price = "";
            int idx = 0;
            int count = 0;
            for ( int i=0; i<dt.Columns.Count; i = i+2)
            {
                count++;
                name = "C" + count.ToString();
                if (G1.get_column_number(dt, name) < 0)
                    continue;
                for (int j = 0; j < dt.Rows.Count; j++)
                {
                    if (dt.Rows[j][name].ObjToString() == "1")
                    {
                        try
                        {
                            str = "S" + count.ToString();
                            service = dt.Rows[j][str].ObjToString();
                            if (service.IndexOf("Primary-") == 0)
                                continue;
                            if (service.IndexOf("Select ") >= 0)
                                continue;
                            type = "";
                            price = "0";
                            if (service.IndexOf("(M") == 0)
                                type = "Merchandise";
                            else if (service.IndexOf("(S") == 0)
                                type = "Service";
                            else if (service.IndexOf("(C") == 0)
                                type = "Cash Advance";
                            idx = service.IndexOf("$");
                            if (idx > 0)
                            {
                                str = service.Substring(0, idx);
                                service = service.Replace(str, "");
                                idx = service.IndexOf(")");
                                if ( idx > 0)
                                {
                                    price = service.Substring(0, idx);
                                    price = price.Replace(")", "");
                                    price = price.Replace(",", "");
                                    price = price.Replace("$", "");
                                    if (!G1.validate_numeric(price))
                                        price = "0";
                                    service = service.Substring(idx+1);
                                }
                            }
                            service = service.Trim();
                            if ( service == "Casket Price" && newDt.Rows.Count > 0 )
                            {
                                if (lastType == "Merchandise")
                                {
                                    lastRow = newDt.Rows.Count - 1;
                                    newDt.Rows[lastRow]["data"] = price;
                                    newDt.Rows[lastRow]["price"] = price;
                                    continue;
                                }
                            }
                            dRow = newDt.NewRow();
                            dRow["service"] = service;
                            dRow["price"] = price;
                            dRow["data"] = price;
                            dRow["type"] = type;
                            dRow["select"] = "1";
                            newDt.Rows.Add(dRow);
                            lastType = type;
                            lastService = service;
                        }
                        catch ( Exception ex)
                        {
                        }
                    }
                }
            }

            for ( int i=0; i<oldDt.Rows.Count; i++)
                oldDt.Rows[i]["DELETED"] = "D";
            for (int i = 0; i < newDt.Rows.Count; i++)
                G1.copy_dt_row(newDt, i, oldDt, oldDt.Rows.Count);
            for (int i = 0; i < oldDt.Rows.Count; i++)
                oldDt.Rows[i]["mod"] = "1";

            newDt = oldDt.Copy();

            matchedSSNs = true;

            DetermineServices(newDt);
            //MatchServices(newDt);
            ReCalcTotal(newDt);
            Services.FixAllData(newDt);
            G1.NumberDataTable(newDt);
            dgv.DataSource = newDt;
            dgv.RefreshDataSource();
            dgv.Refresh();
            gridMain.RefreshData();
            btnSaveServices.Show();
            funModified = true;
            //if ( workContract != primaryContract )
            //{
            //    CustomerDetails.CopyAllContractInfo(primaryContract);
            //    Funerals.DeleteFuneralContract(workContract);
            //    workContract = primaryContract;
            //    FunServices_Load(null, null);
            //}
            gridMain.RefreshData();
            this.Refresh();
        }
        /****************************************************************************************/
        private void UpdateAllContractsInfo ( string contractNumber, DateTime deceasedDate, string serviceId )
        {
            if (String.IsNullOrWhiteSpace(contractNumber))
                return;
            try
            {
                string dDate = deceasedDate.ToString("yyyy-MM-dd");
                string record = "";
                string cmd = "Select * from `customers` where `contractNumber` = '" + contractNumber + "';";
                DataTable dt = G1.get_db_data(cmd);
                if ( dt.Rows.Count > 0 )
                {
                    record = dt.Rows[0]["record"].ObjToString();
                    G1.update_db_table("customers", "record", record, new string[] { "deceasedDate", dDate, "serviceId", serviceId });
                }

                cmd = "Select * from `contracts` where `contractNumber` = '" + contractNumber + "';";
                dt = G1.get_db_data(cmd);
                if (dt.Rows.Count > 0)
                {
                    record = dt.Rows[0]["record"].ObjToString();
                    G1.update_db_table("contracts", "record", record, new string[] { "deceasedDate", dDate, "serviceId", serviceId });
                }
                cmd = "Select * from `cust_extended` where `contractNumber` = '" + contractNumber + "';";
                dt = G1.get_db_data(cmd);
                if (dt.Rows.Count > 0)
                {
                    record = dt.Rows[0]["record"].ObjToString();
                    G1.update_db_table("cust_extended", "record", record, new string[] { "serviceId", serviceId });
                }
            }
            catch ( Exception ex)
            {
                MessageBox.Show("***ERROR*** Updating Contract (" + contractNumber + " ServiceId Info !", "Updating Contract Info Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            }

        }
        /****************************************************************************************/
        private void changeToMerchandiseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            dr["type"] = "Merchandise";
            funModified = true;
            btnSaveServices.Show();
        }
        /****************************************************************************************/
        private void changeToServiceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            dr["type"] = "service";
            funModified = true;
            btnSaveServices.Show();
        }
        /****************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            if (dr == null)
                return;
            if (!workFuneral && G1.isField())
                return;

            DataTable dt = (DataTable)dgv.DataSource;

            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            string type = dr["type"].ObjToString();
            string service = dr["service"].ObjToString();
            string oldService = service;
            double price = dr["price"].ObjToDouble();
            double currentPrice = dr["currentPrice"].ObjToDouble();
            bool discretionary = false;
            //if (type.ToUpper() == "MERCHANDISE")
            //    discretionary = true;
            if (String.IsNullOrWhiteSpace(service))
            {
                discretionary = true;
                service = "D-";
            }
            else if (service.ToUpper().IndexOf("D-") == 0)
                discretionary = true;

            using (ManuallyEditService askForm = new ManuallyEditService(type, service, price, currentPrice ) )
            {
                askForm.ShowDialog();
                if (askForm.DialogResult == System.Windows.Forms.DialogResult.OK)
                {
                    string newService = askForm.wService;
                    if (newService != service && !discretionary )
                    {
                        G1.copy_dt_row(dt, row, dt, dt.Rows.Count);
                        dr["DELETED"] = "DELETED";
                        row = dt.Rows.Count - 1;
                        dt.Rows[row]["service"] = newService;
                        dt.Rows[row]["price"] = askForm.wPrice;
                        dt.Rows[row]["currentPrice"] = askForm.wCurrentPrice;
                        dt.Rows[row]["difference"] = askForm.wCurrentPrice - askForm.wPrice;
                        dt.Rows[row]["mod"] = "1";

                        DataRow[] dRows = dt.Select("service='Outer Container Name' AND data='" + oldService + "'");
                        if (dRows.Length > 0)
                            dRows[0]["DELETED"] = "D";
                    }
                    else
                    {
                        dr["type"] = askForm.wType;
                        dr["service"] = askForm.wService;
                        dr["price"] = askForm.wPrice;
                        dr["currentPrice"] = askForm.wCurrentPrice;

                        service = askForm.wService.Trim();
                        if (discretionary)
                        {
                            if (service.ToUpper().IndexOf("D-") < 0)
                                service = "D-" + service;
                        }
                        dt.Rows[row]["service"] = service;
                        dt.Rows[row]["price"] = askForm.wPrice;
                        dt.Rows[row]["currentPrice"] = askForm.wCurrentPrice;
                        dt.Rows[row]["difference"] = askForm.wCurrentPrice - askForm.wPrice;
                        dt.Rows[row]["mod"] = "1";
                    }

                    funModified = true;
                    btnSaveServices.Show();
                    btnSaveServices.Refresh();

                    ReCalcTotal(dt);
                    dgv.DataSource = dt;
                }
            }
        }
        /****************************************************************************************/
        private void pictureDelete_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt.Rows.Count <= 0)
                return;
            DataRow dr = gridMain.GetFocusedDataRow();
            if (dr == null)
                return;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            string service = dr["service"].ObjToString();
            string record = dr["record"].ObjToString();
            string type = dr["type"].ObjToString().ToUpper();

            string serialNumber = dr["serialNumber"].ObjToString();

            DialogResult result = DevExpress.XtraEditors.XtraMessageBox.Show("***Question***\nDo you really want to REMOVE " + service + "?", "Remove Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;

            bool isCasket = false;
            bool isVault = false;
            bool isUrn = false;

            bool gotPackage = false;

            DataRow[] dRows = dt.Select("isPackage='P'");
            if (dRows.Length > 0)
                gotPackage = true;

            if (type == "MERCHANDISE")
            {
                string[] Lines = service.Split(' ');
                if (Lines.Length >= 1)
                {
                    string casketCode = Lines[0].Trim();
                    string cmd = "Select * from `casket_master` where `casketCode` = '" + casketCode + "';";
                    DataTable ddt = G1.get_db_data(cmd);
                    if (ddt.Rows.Count > 0)
                    {
                        string str = casketCode.Substring(0, 1);
                        if (str == "V")
                            isVault = true;
                        else
                            isCasket = true;
                        cmd = "Select * from `fcustomers` where `contractNumber` = '" + workContract + "';";
                        ddt = G1.get_db_data(cmd);
                        if (ddt.Rows.Count > 0)
                        {
                            string custRec = ddt.Rows[0]["record"].ObjToString();
                            if (isCasket)
                            {
                                G1.update_db_table("fcustomers", "record", custRec, new string[] { "extraItemAmtMI1", "" });
                                G1.update_db_table("fcustomers", "record", custRec, new string[] { "extraItemAmtMR1", "" });
                            }
                            else
                            {
                                G1.update_db_table("fcustomers", "record", custRec, new string[] { "extraItemAmtMI2", "" });
                                G1.update_db_table("fcustomers", "record", custRec, new string[] { "extraItemAmtMR2", "" });
                            }
                        }
                    }
                    else
                    {
                        cmd = "Select * from `casket_master` where `casketdesc` = '" + service + "';";
                        ddt = G1.get_db_data(cmd);
                        if (ddt.Rows.Count > 0)
                        {
                            casketCode = ddt.Rows[0]["casketCode"].ObjToString();
                            string str = casketCode.Substring(0, 1);
                            if (str == "V")
                                isVault = true;
                            else if (casketCode.ToUpper().IndexOf("URN") == 0)
                                isUrn = true;
                            else if (casketCode.ToUpper().IndexOf("UV") == 0)
                                isUrn = true;
                        }
                    }
                }
                else
                {
                    string cmd = "Select * from `casket_master` where `casketdesc` = '" + service + "';";
                    DataTable ddt = G1.get_db_data(cmd);
                    if (ddt.Rows.Count > 0)
                    {
                        string casketCode = ddt.Rows[0]["casketCode"].ObjToString();
                        string str = casketCode.Substring(0, 1);
                        if (str == "V")
                            isVault = true;
                        else if (casketCode.ToUpper().IndexOf("URN") == 0)
                            isUrn = true;
                        else if (casketCode.ToUpper().IndexOf("UV") == 0)
                            isUrn = true;
                    }
                }
            }

            if (isUrn)
            {
                dRows = dt.Select("service like '%URN CREDIT%'");
                if (dRows.Length > 0)
                    dRows[0]["select"] = "1";
            }

            if (G1.get_column_number(dt, "DELETED") < 0)
                dt.Columns.Add("DELETED");
            if (!String.IsNullOrWhiteSpace(record))
            {
                if (record != "0")
                {
                    if (workFuneral)
                        G1.delete_db_table("fcust_services", "record", record);
                    else
                        G1.delete_db_table("cust_services", "record", record);
                }
            }
            try
            {
                avoidUpdate = true;
                dt.Rows.Remove(dr);
                avoidUpdate = false;
            }
            catch ( Exception ex)
            {
            }

            //dt.Rows[row]["DELETED"] = "D"; // Save this for Tracking later

            //dr["select"] = "0";
            //dr["mod"] = "1";
            //dt.Rows[row]["select"] = "0";
            //dt.Rows[row]["mod"] = "1";
            if (type == "MERCHANDISE")
                RemoveOldMerchandise(service, dt );
            funModified = true;
            btnSaveServices.Show();
            btnSaveServices.Refresh();

            dgv.DataSource = dt;

            ReCalcTotal(dt);

            if (!String.IsNullOrWhiteSpace(serialNumber))
                MarkInventoryAsNotUsed(serialNumber);

            btnSaveServices_Click(null, null);
        }
        /****************************************************************************************/
        private void RemoveOldMerchandise ( string service, DataTable dt )
        {
            string cmd = "Select * from `fcustomers` where `contractNumber` = '" + workContract + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
                return;
            bool cleanupCasket = false;
            bool cleanupVault = false;
            string record = dx.Rows[0]["record"].ObjToString();
            string oldCasket = dx.Rows[0]["extraItemAmtMI1"].ObjToString().ToUpper();
            if (!String.IsNullOrWhiteSpace(oldCasket))
            {
                if (service.ToUpper().IndexOf(oldCasket) >= 0)
                {
                    cleanupCasket = true;
                    oldCasket = "";
                }
            }
            string oldVault = dx.Rows[0]["extraItemAmtMI2"].ObjToString().ToUpper();
            if (!String.IsNullOrWhiteSpace(oldVault))
            {
                if (service.ToUpper().IndexOf(oldVault) >= 0)
                {
                    cleanupVault = true;
                    oldVault = "";
                }
            }
            if (cleanupCasket && cleanupVault && record != "0" )
                G1.update_db_table("fcustomers", "record", record, new string[] { "extraItemAmtMI1", oldCasket, "extraItemAmtMI2", oldVault});
            else if ( cleanupCasket && record != "0" )
                G1.update_db_table("fcustomers", "record", record, new string[] { "extraItemAmtMI1", oldCasket });
            else if (cleanupVault && record != "0" )
                G1.update_db_table("fcustomers", "record", record, new string[] { "extraItemAmtMI2", oldVault });

            if (cleanupCasket)
            {
                DataRow[] dRows = dt.Select("data='" + service + "'");
                if (dRows.Length > 0)
                {
                    record = dRows[0]["record"].ObjToString();
                    if (record != "0")
                        G1.delete_db_table("fcust_services", "record", record);
                }
                dRows = dt.Select("service='" + service + "'");
                if (dRows.Length > 0)
                {
                    record = dRows[0]["record"].ObjToString();
                    if (record != "0")
                        G1.delete_db_table("fcust_services", "record", record);
                }
            }
        }
        /****************************************************************************************/
        private void btnRemoveAll_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt.Rows.Count <= 0)
                return;

            DialogResult result = DevExpress.XtraEditors.XtraMessageBox.Show("***Question***\nDo you really want to REMOVE ALL SERVICES AND MERCHANDISE?", "Remove ALL Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;
            if (String.IsNullOrWhiteSpace(custServicesFile))
                return;
            string record = "";
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                record = dt.Rows[i]["record"].ObjToString();
                if (!String.IsNullOrWhiteSpace(record))
                    G1.delete_db_table(custServicesFile, "record", record);
            }
            dt.Rows.Clear();
            dgv.DataSource = dt;
        }
        /****************************************************************************************/
        private void cmbFunClass_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            string funClass = cmbFunClass.Text.Trim();

            string cmd = "Select * from `" + extendedFile + "` where `contractNumber` = '" + workContract + "';";
            DataTable dt = G1.get_db_data(cmd);
            if ( dt.Rows.Count > 0 )
            {
                string record = dt.Rows[0]["record"].ObjToString();
                G1.update_db_table(extendedFile, "record", record, new string[] { "funeral_classification", funClass });
            }
        }
        /****************************************************************************************/
        private int subRow = -1;
        private bool forceUpgrade = false;
        private string subMerchandise = "";
        private void menuSubstitute_Click(object sender, EventArgs e)
        {
            forceUpgrade = false;
            DataTable dt = (DataTable)dgv.DataSource;
            subRow = gridMain.FocusedRowHandle;
            subRow = gridMain.GetDataSourceRowIndex(subRow);
            subMerchandise = dt.Rows[subRow]["service"].ObjToString();
            string group = cmbGroups.Text.Trim();
            string package = cmbPackage.Text.Trim();
            string casketGroup = cmbCasketGroup.Text.Trim();

            string what = isWhatMerchansdise(subMerchandise);

            Services serviceForm = new Services(group, casketGroup, true, dt, "", "Merchandise", what );
            serviceForm.SelectDone += ServiceForm_SelectDoneSubstitute;
            serviceForm.Show();
        }
        /***************************************************************************************/
        private string isWhatMerchansdise ( string service )
        {
            string what = "";
            string casketCode = "";
            string cmd = "Select * from `casket_master` where `casketdesc` = '" + service + "';";
            DataTable ddt = G1.get_db_data(cmd);
            if (ddt.Rows.Count > 0)
            {
                casketCode = ddt.Rows[0]["casketCode"].ObjToString();
                string str = casketCode.Substring(0, 1);
                if (str == "V")
                    what = "Vault";
                else if (casketCode.ToUpper().IndexOf("URN") == 0)
                    what = "Urn";
                else if (casketCode.ToUpper().IndexOf("UV") == 0)
                    what = "Urn";
            }
            return what;
        }
        /***************************************************************************************/
        private DataTable CompareDT(DataTable dt1, DataTable dt2)
        {
            DataTable dt3 = dt1.Clone();
            try
            {
                dt3 = dt1.AsEnumerable().Where(ra => !dt2.AsEnumerable().Any(rb => rb.Field<string>("service") == ra.Field<string>("service"))).CopyToDataTable();
            }
            catch ( Exception ex )
            {
            }
            return dt3;
        }        
        /***************************************************************************************/
        private void ServiceForm_SelectDoneSubstitute(DataTable dt, string what )
        {
            workDt = (DataTable)dgv.DataSource;

            DataTable dx = CompareDT(dt, workDt);

            if ( dx.Rows.Count > 0 )
            {
                if (G1.get_column_number(workDt, "DELETED") < 0)
                    workDt.Columns.Add("DELETED");

                string service = dx.Rows[0]["service"].ObjToString();
                if (String.IsNullOrWhiteSpace(service))
                    return;

                string casketCode = "";
                bool isVault = false;
                bool isUrn = false;

                string cmd = "Select * from `casket_master` where `casketdesc` = '" + service + "';";
                DataTable ddt = G1.get_db_data(cmd);
                if (ddt.Rows.Count > 0)
                {
                    casketCode = ddt.Rows[0]["casketCode"].ObjToString();
                    string str = casketCode.Substring(0, 1);
                    if (str == "V")
                        isVault = true;
                    else if (casketCode.ToUpper().IndexOf("URN") == 0)
                        isUrn = true;
                    else if (casketCode.ToUpper().IndexOf("UV") == 0)
                        isUrn = true;

                    if (!isVault && !isUrn)
                    {
                        if (service.IndexOf(casketCode) < 0)
                            service = casketCode + " " + service;
                    }
                }

                double originalDifference = workDt.Rows[subRow]["difference"].ObjToDouble();
                //if ( originalDifference <= 0D)
                //{
                //    MessageBox.Show("***ERROR*** This merchandise CANNOT be upgraded\nbecause there isn't already a Pre-Need Discount!", "Merchandise Upgrade Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                //    return;
                //}
                double oldPrice = workDt.Rows[subRow]["currentPrice"].ObjToDouble();
                double customerPrice = workDt.Rows[subRow]["price"].ObjToDouble();
                double oldCustomerPrice = customerPrice;
                double currentPrice = dx.Rows[0]["price"].ObjToDouble();
                double difference = currentPrice - customerPrice;
                if ( forceUpgrade )
                {
                    if ( currentPrice < oldPrice )
                    {
                        MessageBox.Show("***ERROR*** This merchandise CANNOT be upgraded\nbecause price is less than original price!", "Merchandise Upgrade Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                        return;
                    }
                }
                else
                {
                    //if ( oldPrice != currentPrice )
                    //{
                    //    MessageBox.Show("***ERROR*** This merchandise CANNOT be stubstituted\nbecause price is not equal to original price!", "Merchandise Substitute Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    //    return;
                    //}
                }
                if (forceUpgrade && originalDifference > 0D )
                {
                    difference = originalDifference;
                    customerPrice = currentPrice - difference;
                }
                else if ( originalDifference == 0D )
                {
                    customerPrice = customerPrice + difference;
                }
                else if ( forceUpgrade )
                {
                    MessageBox.Show("***ERROR*** This merchandise CANNOT be upgraded\nbecause there isn't already a Pre-Need Discount!", "Merchandise Upgrade Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    return;
                }
                G1.copy_dt_row(workDt, subRow, workDt, workDt.Rows.Count);

                workDt.Rows[subRow]["mod"] = "1";
                workDt.Rows[subRow]["DELETED"] = "D";

                subRow = workDt.Rows.Count - 1;
                workDt.Rows[subRow]["service"] = service;
                if (forceUpgrade && originalDifference > 0D )
                {
                    workDt.Rows[subRow]["price"] = 0D;
                    workDt.Rows[subRow]["data"] = "0";
                    workDt.Rows[subRow]["upgrade"] = difference.ToString();
                }
                else
                {
                    if (!forceUpgrade)
                        oldPrice = oldCustomerPrice;
                    workDt.Rows[subRow]["price"] = oldPrice;
                    workDt.Rows[subRow]["data"] = oldPrice.ObjToString();
                    workDt.Rows[subRow]["upgrade"] = difference.ToString();
                }

                workDt.Rows[subRow]["currentPrice"] = currentPrice;
                workDt.Rows[subRow]["difference"] = difference;
                workDt.Rows[subRow]["mod"] = "1";
                workDt.Rows[subRow]["record"] = "0";

                cmd = "Select * from `fcustomers` where `contractNumber` = '" + workContract + "';";
                ddt = G1.get_db_data(cmd);

                DataRow[] dRows = workDt.Select("service='Casket Name'");
                if ( dRows.Length > 0 )
                {
                    dRows[0]["service"] = "";
                    dRows[0]["data"] = "";
                    dRows[0]["select"] = "1";
                    dRows[0]["mod"] = "1";
                    dRows[0]["DELETED"] = "D";
                }
                dRows = workDt.Select("service='Casket Price'");
                if (dRows.Length > 0)
                {
                    dRows[0]["data"] = "";
                    dRows[0]["select"] = "1";
                    dRows[0]["mod"] = "1";
                    dRows[0]["DELETED"] = "D";
                }
                if ( ddt.Rows.Count > 0 )
                {
                    string record = ddt.Rows[0]["record"].ObjToString();
                    if (!isVault)
                    {
                        G1.update_db_table("fcustomers", "record", record, new string[] { "extraItemAmtMI1", "" });
                        G1.update_db_table("fcustomers", "record", record, new string[] { "extraItemAmtMR1", "" });
                    }
                    else if ( isVault )
                    {
                        G1.update_db_table("fcustomers", "record", record, new string[] { "extraItemAmtMI2", "" });
                        G1.update_db_table("fcustomers", "record", record, new string[] { "extraItemAmtMR2", "" });
                    }
                }
                dgv.DataSource = workDt;
                dgv.Refresh();
                btnSaveServices.Show();
                btnSaveServices.Refresh();
                funModified = true;
                ReCalcTotal(workDt);
                btnSaveServices_Click(null, null);
            }
        }
        /****************************************************************************************/
        private void menuUpgrade_Click(object sender, EventArgs e)
        {
            forceUpgrade = true;
            DataTable dt = (DataTable)dgv.DataSource;
            subRow = gridMain.FocusedRowHandle;
            subRow = gridMain.GetDataSourceRowIndex(subRow);
            subMerchandise = dt.Rows[subRow]["service"].ObjToString();

            string group = cmbGroups.Text.Trim();
            string package = cmbPackage.Text.Trim();
            string casketGroup = cmbCasketGroup.Text.Trim();

            string what = isWhatMerchansdise(subMerchandise);

            Services serviceForm = new Services(group, casketGroup, true, dt, "", "Merchandise", what );
            serviceForm.SelectDone += ServiceForm_SelectDoneSubstitute;
            serviceForm.Show();
        }
        /****************************************************************************************/
        private void releaseInventoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!workFuneral)
                return;

            DataTable dt = (DataTable)dgv.DataSource;
            if (dt.Rows.Count <= 0)
                return;
            DataRow dr = gridMain.GetFocusedDataRow();
            if (dr == null)
                return;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            string serialNumber = dr["serialNumber"].ObjToString();
            if (String.IsNullOrWhiteSpace(serialNumber))
                return;

            MarkInventoryAsNotUsed(serialNumber);

            dr["serialNumber"] = "";
            dr["mod"] = "1";

            string record = dr["record"].ObjToString();
            if ( !String.IsNullOrWhiteSpace ( record ))
            {
                if (record != "0" && record != "-1")
                    G1.update_db_table("fcust_services", "record", record, new string[] {"SerialNumber", "" });
            }    
        }
        /****************************************************************************************/
        private void menuCorrectPreNeed_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            DataTable dt = (DataTable)dgv.DataSource;

            if (G1.get_column_number(dt, "DELETED") < 0)
                dt.Columns.Add("DELETED");

            string type = dr["type"].ObjToString();
            string service = dr["service"].ObjToString();
            double price = dr["price"].ObjToDouble();
            double currentPrice = dr["currentPrice"].ObjToDouble();

            string status = dr["status"].ObjToString();

            using (ManuallyEditService askForm = new ManuallyEditService(type, service, price, currentPrice))
            {
                askForm.ShowDialog();
                if (askForm.DialogResult == System.Windows.Forms.DialogResult.OK)
                {
                    //dr["type"] = askForm.wType;
                    //dr["service"] = askForm.wService;
                    string newService = askForm.wService;
                    if (newService != service)
                    {
                        G1.copy_dt_row(dt, row, dt, dt.Rows.Count);
                        dr["DELETED"] = "DELETED";
                        row = dt.Rows.Count - 1;
                        dt.Rows[row]["service"] = newService;
                        dt.Rows[row]["price"] = askForm.wPrice;
                        dt.Rows[row]["currentPrice"] = askForm.wCurrentPrice;
                        dt.Rows[row]["difference"] = askForm.wCurrentPrice - askForm.wPrice;
                        dt.Rows[row]["mod"] = "1";
                    }
                    else
                    {
                        dr["price"] = askForm.wPrice;
                        dr["currentPrice"] = askForm.wCurrentPrice;
                        dt.Rows[row]["price"] = askForm.wPrice;
                        dt.Rows[row]["currentPrice"] = askForm.wCurrentPrice;
                        dt.Rows[row]["difference"] = askForm.wCurrentPrice - askForm.wPrice;
                        dt.Rows[row]["data"] = askForm.wCurrentPrice.ToString();
                        dt.Rows[row]["mod"] = "1";
                    }

                    DataRow[] dRows = dt.Select("service='Outer Container Name' AND data='" + service + "'");
                    if (dRows.Length > 0)
                        dRows[0]["DELETED"] = "D";
                    ReCalcTotal(dt);
                    dgv.DataSource = dt;
                    funModified = true;
                    btnSaveServices.Show();
                    btnSaveServices.Refresh();
                }
            }
        }
        /****************************************************************************************/
        private void repositoryItemCheckEdit2_CheckStateChanged(object sender, EventArgs e)
        {
            DevExpress.XtraEditors.CheckEdit check = (DevExpress.XtraEditors.CheckEdit)sender;
            bool isChecked = true;
            if (!check.Checked)
                isChecked = false;
            DataTable dt = (DataTable)dgv.DataSource;
            bool isPackage = false;
            if (G1.get_column_number(dt, "isPackage") >= 0)
            {
                DataRow[] dRows = dt.Select("isPackage='P'");
                if (dRows.Length > 0)
                    isPackage = true;
            }
            bool keepDiscount = false;
            DataRow dr = gridMain.GetFocusedDataRow();
            if (isChecked)
            {
                dr["select"] = "1";
                dr["mod"] = "1";
            }
            else
            {
                dr["select"] = "0";
                dr["mod"] = "1";
                    gridMain.PostEditor();
                    gridMain.RefreshData();
                    gridMain.RefreshEditor(true);

                if (isPackage)
                {
                    //DialogResult result = MessageBox.Show("***Question***\nMaintain Discount?", "Maintain Discount Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    //if (result == DialogResult.Yes)
                    //    keepDiscount = true;
                    keepDiscount = true;
                }
            }
            //dt = (DataTable)dgv.DataSource;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            dt.Rows[row]["select"] = dr["select"].ObjToString();
            dt.Rows[row]["mod"] = "1";
            if ( keepDiscount )
            {
                double price = dr["price"].ObjToDouble();
//                price = Math.Abs(price) * -1D;
                price = Math.Abs(price);
                dr["price"] = price;
                dt.Rows[row]["price"] = price;
            }
            //dt.AcceptChanges();
            //gridMain.PostEditor();
            gridMain.RefreshData();
            gridMain.EndInit();

            DataTable dx = dt.Copy();
            dgv.DataSource = dt;
            dgv.RefreshDataSource();
            dgv.Refresh();
            ReCalcTotal(dt);
            funModified = true;
            btnSaveServices.Show();
            btnSaveServices.Refresh();
        }
        /****************************************************************************************/
        private void FunServices_MaximumSizeChanged(object sender, EventArgs e)
        {
            if (sizeChange)
                return;
            try
            {
                this.Refresh();
                this.Update();
                Application.DoEvents();
            }
            catch ( Exception ex)
            {
            }
        }
        /****************************************************************************************/
        private void FunServices_SizeChanged(object sender, EventArgs e)
        {
            try
            {
                if (sizeChange)
                    return;
                sizeChange = true;
                didSummary = false;
                this.Refresh();
                this.Update();
                Application.DoEvents();
            }
            catch (Exception ex)
            {
            }
            sizeChange = false;
        }
        /****************************************************************************************/
        public static void UnsubscribeSystemEvents()
        {
            try
            {
                var handlers = typeof(SystemEvents).GetField("_handlers", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
                var handlersValues = handlers.GetType().GetProperty("Values").GetValue(handlers);
                foreach (var invokeInfos in (handlersValues as IEnumerable).OfType<object>().ToArray())
                    foreach (var invokeInfo in (invokeInfos as IEnumerable).OfType<object>().ToArray())
                    {
                        var syncContext = invokeInfo.GetType().GetField("_syncContext", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(invokeInfo);
                        if (syncContext == null)
                            throw new Exception("syncContext missing");
                        if (!(syncContext is WindowsFormsSynchronizationContext))
                            continue;
                        var threadRef = (WeakReference)syncContext.GetType().GetField("destinationThreadRef", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(syncContext);
                        if (!threadRef.IsAlive)
                            continue;
                        var thread = (System.Threading.Thread)threadRef.Target;
                        if (thread.ManagedThreadId == 1)
                            continue;  // Change here if you have more valid UI threads to ignore
                        var dlg = (Delegate)invokeInfo.GetType().GetField("_delegate", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(invokeInfo);
                        var handler = (UserPreferenceChangedEventHandler)Delegate.CreateDelegate(typeof(UserPreferenceChangedEventHandler), dlg.Target, dlg.Method.Name);
                        SystemEvents.UserPreferenceChanged -= handler;
                    }
            }
            catch ( Exception ex)
            {
                //trace here your errors
            }
        }
        /****************************************************************************************/
        private void repositoryItemCheckEdit2_EditValueChanged(object sender, EventArgs e)
        {
            gridMain.PostEditor();
        }
        /****************************************************************************************/
        private void menuHideOnContract_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();

            string type = dr["type"].ObjToString();
            string service = dr["service"].ObjToString();
            if ( type.ToUpper() != "SERVICE")
            {
                string cmd = "Select * from `services` where `service` = '" + service + "';";
                DataTable dx = G1.get_db_data(cmd);
                if (dx.Rows.Count <= 0)
                {
                    MessageBox.Show("Only SERVICES can be EXCLUDED from a Funeral!", "Exclude Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    return;
                }
            }

            DialogResult result = MessageBox.Show("***Question*** \nDo you really want to Exclude this Service (" + service + ") ?", "Exclude Service Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2, MessageBoxOptions.DefaultDesktopOnly);
            if (result == DialogResult.No)
                return;

            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            DataTable dt = (DataTable)dgv.DataSource;

            if (G1.get_column_number(dt, "ignore") < 0)
                dt.Columns.Add("ignore");

            string ignore = dr["ignore"].ObjToString();
            if ( ignore == "Y" )
            {
                dr["ignore"] = "";
                dt.Rows[row]["ignore"] = "";
            }
            else
            {
                dr["ignore"] = "Y";
                dt.Rows[row]["ignore"] = "Y";
            }

            dr["mod"] = "1";
            dt.Rows[row]["mod"] = "1";

            ReCalcTotal(dt);
            dgv.DataSource = dt;
            funModified = true;
            btnSaveServices.Show();
            btnSaveServices.Refresh();
        }
        /****************************************************************************************/
        private void gridMain_RowCellStyle(object sender, RowCellStyleEventArgs e)
        {
            GridView View = sender as GridView;
            string str = View.GetRowCellValue(e.RowHandle, "ignore").ObjToString();
            if (str != null)
            {
                if (str == "Y")
                    e.Appearance.BackColor = Color.LightGray;
            }
        }
        /****************************************************************************************/
        private void gridMain_ShownEditor(object sender, EventArgs e)
        {
            GridView view = sender as GridView;
            DataRow dr = gridMain.GetFocusedDataRow();
            int row = gridMain.FocusedRowHandle;
            row = gridMain.GetDataSourceRowIndex(row);
            string field = view.FocusedColumn.FieldName.ToUpper();
            if (field.ToUpper() == "SERIALNUMBER")
            {
                string serialNumber = dr["SerialNumber"].ObjToString();
                DataTable dt = (DataTable)dgv.DataSource;
                string oldSerialNumber = dt.Rows[row]["SerialNumber"].ObjToString();
            }
        }
        /****************************************************************************************/
        private void gridMain_ValidatingEditor(object sender, DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventArgs e)
        {
            GridView view = sender as GridView;
            DataRow dr = gridMain.GetFocusedDataRow();
            int row = gridMain.FocusedRowHandle;
            row = gridMain.GetDataSourceRowIndex(row);
            string field = view.FocusedColumn.FieldName.ToUpper();
            if ( field.ToUpper() == "SERIALNUMBER")
            {
                if (didSummary)
                {
                    didSummary = false;
                    return;
                }
                string serialNumber = dr["SerialNumber"].ObjToString();
                DataTable dt = (DataTable)dgv.DataSource;
                string oldSerialNumber = dt.Rows[row]["SerialNumber"].ObjToString();
            }
            //string edit = dt.Rows[row]["edit"].ObjToString();
            //if (edit != "Y")
            //{
            //    e.Valid = false;
            //    e.ErrorText = "This Row May Not Be Edited!";
            //    return;
            //}
        }
        /****************************************************************************************/
        private void gridMain_KeyDown(object sender, KeyEventArgs e)
        {
            bool gotKey = false;
            if (e.KeyData == Keys.Enter)
                gotKey = true;
            else if (e.KeyData == Keys.Up || e.KeyData == Keys.Down || e.KeyData == Keys.Left || e.KeyData == Keys.Right || e.KeyData == Keys.Home)
                gotKey = true;
            else if (e.KeyData == Keys.Tab || e.KeyData == Keys.PageUp || e.KeyData == Keys.PageDown || e.KeyData == Keys.End)
                gotKey = true;
            if ( gotKey )
                ChangeInventory();
        }
        /****************************************************************************************/
        private void gridMain_MouseDown(object sender, MouseEventArgs e)
        {
            ChangeInventory();
        }
        /****************************************************************************************/
        private void ChangeInventory ()
        {
            if (!workFuneral)
                return;

            DataTable dt = (DataTable)dgv.DataSource;

            DataRow dr = gridMain.GetFocusedDataRow();
            if (dr == null)
                return;
            string type = dr["type"].ObjToString().ToUpper();
            string field = gridMain.FocusedColumn.FieldName.ToUpper();
            if (field.ToUpper() != "SERIALNUMBER")
                return;

            string oldSerialNumber = dr[field].ObjToString();

            dt.AcceptChanges();
            dr = gridMain.GetFocusedDataRow();

            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);



            string serialNumber = dt.Rows[row]["serialNumber"].ObjToString();
            if (String.IsNullOrWhiteSpace(serialNumber) && !String.IsNullOrWhiteSpace ( oldSerialNumber ))
            {
                MessageBox.Show("***ERROR*** You cannot blank out a Serial Number!\nUse (Right-Click) Release Inventory!", "Merchandie Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                dr[field] = oldSerialNumber;
                dt.Rows[row]["serialNumber"] = oldSerialNumber;
                gridMain.RefreshData();
                return;
            }
            //if (serialNumber.Length < 8)
            //    return;

            if (String.IsNullOrWhiteSpace(serialNumber))
                return;

            if (serialNumber == oldSerialNumber)
                return;

            string cmd = "Select * from `inventory` where `SerialNumber` = '" + serialNumber + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
            {
                DialogResult result = MessageBox.Show("***ERROR*** Serial Number " + serialNumber + " is not in current inventory!", "Invalid Serial Number", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                if (!String.IsNullOrWhiteSpace(oldSerialNumber))
                {
                    dr[field] = oldSerialNumber;
                    dt.Rows[row]["serialNumber"] = oldSerialNumber;
                    gridMain.RefreshData();
                }
                else
                    dr[field] = "";
                return;
            }
            string serviceID = dx.Rows[0]["ServiceID"].ObjToString();
            DateTime date = dx.Rows[0]["DateUsed"].ObjToDateTime();
            string location = dx.Rows[0]["LocationCode"].ObjToString();
            if (!String.IsNullOrWhiteSpace(serviceID) && date.Year > 100)
            {
                MessageBox.Show("***ERROR*** Serial Number (" + serialNumber + ") already in use on " + date.ToString("MM/dd/yyyy") + " at " + location + " ServiceID " + serviceID + "!", "Merchandie Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                if (!String.IsNullOrWhiteSpace(oldSerialNumber))
                {
                    dr[field] = oldSerialNumber;
                    dt.Rows[row]["serialNumber"] = oldSerialNumber;
                    gridMain.RefreshData();
                }
                else
                    dr[field] = "";
                return;
            }
            if (!String.IsNullOrWhiteSpace(serviceID))
            {
                MessageBox.Show("***ERROR*** Serial Number (" + serialNumber + ") already in use at " + location + " ServiceID " + serviceID + "!", "Merchandie Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                if (!String.IsNullOrWhiteSpace(oldSerialNumber))
                {
                    dr[field] = oldSerialNumber;
                    dt.Rows[row]["serialNumber"] = oldSerialNumber;
                    gridMain.RefreshData();
                }
                else
                    dr[field] = "";
                return;
            }
            if (date.Year > 100)
            {
                MessageBox.Show("***ERROR*** Serial Number (" + serialNumber + ") already in use on " + date.ToString("MM/dd/yyyy") + " at " + location + "!", "Merchandie Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                if (!String.IsNullOrWhiteSpace(oldSerialNumber))
                {
                    dr[field] = oldSerialNumber;
                    dt.Rows[row]["serialNumber"] = oldSerialNumber;
                    gridMain.RefreshData();
                }
                else
                    dr[field] = "";
                return;
            }

            deceasedDate = GetDeceasedDate(workContract);
            if (String.IsNullOrWhiteSpace(deceasedDate))
            {
                MessageBox.Show("***ERROR*** Current Customer is not DECEASED!\nYou cannot assign a serial number here!", "Merchandie Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                if (!String.IsNullOrWhiteSpace(oldSerialNumber))
                {
                    dr[field] = oldSerialNumber;
                    dt.Rows[row]["serialNumber"] = oldSerialNumber;
                    gridMain.RefreshData();
                }
                else
                    dr[field] = "";
                return;
            }
            if (String.IsNullOrWhiteSpace(serviceId))
            {
                MessageBox.Show("***ERROR*** Current Customer does not have a Service ID!\nYou cannot assign a serial number here!", "Merchandie Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                if (!String.IsNullOrWhiteSpace(oldSerialNumber))
                {
                    dr[field] = oldSerialNumber;
                    dt.Rows[row]["serialNumber"] = oldSerialNumber;
                    gridMain.RefreshData();
                }
                else
                    dr[field] = "";
                return;
            }
            dr["mod"] = "1";
            funModified = true;
            btnSaveServices.Show();
            gridMain.RefreshData();
        }
        /****************************************************************************************/
    }
}