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
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.BandedGrid;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using Org.BouncyCastle.Bcpg.OpenPgp;
using DevExpress.XtraReports.UI;
using static DevExpress.Utils.Drawing.Helpers.NativeMethods;
using System.Drawing.Printing;
using DevExpress.XtraExport.Implementation;
using DevExpress.CodeParser;
using DevExpress.Printing.Core.PdfExport.Metafile;
using DevExpress.Utils.Extensions;
using DevExpress.XtraReports.UI.CrossTab;
using DevExpress.DirectX.Common.Direct3D;
//using System.Web.UI.WebControls;

/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class Caskets : DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        private bool alreadyLoaded = false;
        private bool modified = false;
        private bool casketsModified = false;
        private bool vaultsModified = false;
        private bool urnsModified = false;
        private bool miscModified = false;
        private bool Selecting = false;
        private DataTable workDt = null;
        private string workContract = "";
        private bool loading = false;
        private string loadedPackage = "";
        private string loadededLocation = "";
        private string workFrom = "";
        private bool workingLocation = false;
        private string workCasket = "";
        /***********************************************************************************************/
        public Caskets(bool selecting, DataTable dt = null, string contract = "")
        {
            InitializeComponent();
            Selecting = selecting;
            workDt = dt;
            workContract = contract;
        }
        /***********************************************************************************************/
        public Caskets(string from, bool selecting, DataTable dt = null, string contract = "")
        {
            InitializeComponent();
            Selecting = selecting;
            workDt = dt;
            workContract = contract;
            workFrom = from;
        }
        /***********************************************************************************************/
        public Caskets(bool selecting, bool locations, string casketGroup, DataTable dt = null)
        {
            InitializeComponent();
            Selecting = selecting;
            workCasket = casketGroup;
            workDt = dt;
            workContract = "";
            workingLocation = locations;
        }
        /***********************************************************************************************/
        private void Caskets_Load(object sender, EventArgs e)
        {
            btnSave.Hide();
            loading = true;
            if (!Selecting)
            {
                LoadGroupCombo();
                LoadGPLGroupCombo();
                labBalanceDue.Hide();
                labBalDue.Hide();
                gridMain.OptionsBehavior.ReadOnly = false;
                gridMain.Columns["total"].Visible = false;
                btnEdit.Hide();
                btnDeleteLocation.Hide();
            }
            else
            {
                cmbGroups.Hide();
                cmbPackage.Hide();
                lblLocation.Hide();
                lblPackage.Hide();
                btnAddPackage.Hide();
                btnDeletePackage.Hide();
                gridMain.OptionsView.ShowFooter = true;
                gridMain.Columns["total"].Visible = false;
            }
            LoadData();
            if (Selecting)
            {
                pictureAdd.Hide();
                pictureDelete.Hide();
                btnInsert.Hide();
                picRowDown.Hide();
                picRowUp.Hide();
                gridMain.Columns["select"].Visible = true;
                gridMain.Columns["SameAsMaster"].Visible = true;
                this.Text = "Merchandise for Contract (" + workContract + ")";
                if (!workingLocation)
                    ReSelectServices();
                else
                {
                    cmbLocations.Hide();
                    cmbGPLGroup.Hide();
                    label1.Hide();
                    labBalanceDue.Hide();
                    labBalDue.Hide();
                    btnEdit.Hide();
                    btnAddLocation.Hide();
                    btnDeleteLocation.Hide();
                    picMoveFuture.Hide();
                    picMovePast.Hide();
                    lblLocations.Hide();
                    int left = panelTop.Left;
                    int top = panelTop.Top;
                    int height = panelTop.Height;
                    int width = panelTop.Width;
                    height = height / 2;
                    panelTop.SetBounds(left, top, width, height);
                    if (workingLocation)
                        this.Text = "Select Merchandise to Customize Prices for " + workCasket;
                }
            }
            loading = false;
        }
        /***********************************************************************************************/
        private void ReSelectServices()
        {
            if (workDt == null)
                return;
            DataTable dt = (DataTable)dgv.DataSource;
            string availableService = "";
            string service = "";
            double price = 0D;
            double price1 = 0D;
            double cost = 0D;
            string data = "";
            string type = "";
            string same = "";
            bool found = false;
            bool serviceRecord = false;
            string record = "";
            if (G1.get_column_number(workDt, "!serviceRecord") < 0)
            {
                serviceRecord = true;
            }
            for (int i = 0; i < workDt.Rows.Count; i++)
            {
                service = workDt.Rows[i]["casketdesc"].ObjToString();
                price = workDt.Rows[i]["price"].ObjToDouble();
                //data = workDt.Rows[i]["data"].ObjToString();
                //price1 = workDt.Rows[i]["price1"].ObjToDouble();
                //cost = workDt.Rows[i]["cost"].ObjToDouble();
                //type = workDt.Rows[i]["type"].ObjToString();
                //same = workDt.Rows[i]["SameAsMaster"].ObjToString();
                found = false;
                for (int j = 0; j < dt.Rows.Count; j++)
                {
                    availableService = dt.Rows[j]["casketdesc"].ObjToString();
                    if (availableService == service)
                    {
                        dt.Rows[j]["select"] = "1";
                        dt.Rows[j]["casketcost"] = price;
                        //                        dt.Rows[j]["data"] = cost;
                        //if (same == "1")
                        //{
                        //    dt.Rows[j]["price"] = price1;
                        //    dt.Rows[j]["data"] = data;
                        //    dt.Rows[j]["SameAsMaster"] = "1";
                        //}
                        //if (!String.IsNullOrWhiteSpace(type))
                        //    dt.Rows[j]["type"] = type;
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    DataRow dRow = dt.NewRow();
                    dRow["service"] = service;
                    dRow["data"] = data;
                    dRow["type"] = type;
                    dRow["select"] = "1";
                    dt.Rows.Add(dRow);
                }
            }
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            ReCalcTotal();
        }
        /***********************************************************************************************/
        private void LoadMasterVaults()
        {
            string cmd = "Select * from `casket_master` where `casketcode` like 'V%';";
            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("num");
            dt.Columns.Add("mod");
            dt.Columns.Add("select");
            dt.Columns.Add("casket", Type.GetType("System.Double"));
            dt.Columns.Add("package", Type.GetType("System.Double"));

            //            dt.Columns.Add("SameAsMaster");
            dt.Columns.Add("total", Type.GetType("System.Double"));
            SetupSelection(dt);
            //            SetupSameAsMaster(dt);
            //            CalculateCosts(dt);
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void LoadPackage(bool fromBackup = false)
        {
            string serviceRecord = "";
            string group = GetGroup();
            if (String.IsNullOrWhiteSpace(group))
            {
                MessageBox.Show("***ERROR*** Empty group or package!");
                return;
            }

            LoadPackagesCombo();

            loadededLocation = group;
            string list = "";
            string casketPackageTable = "casket_packages";
            if (fromBackup)
                casketPackageTable = "casket_packages_old";
            string cmd = "Select * from `" + casketPackageTable + "` where `groupname` = '" + group + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0 && group.ToUpper() == "MASTER VAULT")
            {
                LoadMasterVaults();
                return;
            }
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
                cmd = "Select * from `" + casketPackageTable + "` p RIGHT JOIN `casket_master` s ON p.`!masterRecord` = s.`record` where s.`record` IN (" + list + ") ";
                cmd += " and `groupname` = '" + group + "' ";
                cmd += ";";
            }
            else
                cmd = "Select * from `" + casketPackageTable + "` p JOIN `casket_master` s ON p.`!masterRecord` = s.`record` where `groupname` = 'xyzzyxxxx';";
            DataTable dt = G1.get_db_data(cmd);

            DataTable dt2 = dt.Clone();
            DataTable dt3 = dt.Clone();
            DataTable dt4 = dt.Clone();

            if (group.ToUpper().IndexOf("CASKET") >= 0)
                dt = MatchToMaster(dt, group);

            dt.Columns.Add("num");
            dt.Columns.Add("mod");
            dt.Columns.Add("select");
            dt.Columns.Add("casket", Type.GetType("System.Double"));
            dt.Columns.Add("package", Type.GetType("System.Double"));

            //            dt.Columns.Add("SameAsMaster");
            dt.Columns.Add("total", Type.GetType("System.Double"));
            SetupSelection(dt);
            if (group.ToUpper().IndexOf("CASKET") >= 0)
            {
                SetupMaster(dt);
                dt2 = (DataTable)dgv2.DataSource;
                dt3 = (DataTable)dgv3.DataSource;
                dt4 = (DataTable)dgv4.DataSource;
                dt2 = MatchToMaster(dt2, "VAULT");
                dt3 = MatchToMaster(dt3, "URN");
                dt4 = MatchToMaster(dt4, "MISC");
            }
            //            SetupSameAsMaster(dt);
            CalculateCosts(dt);

            G1.NumberDataTable(dt);
            G1.NumberDataTable(dt2);
            G1.NumberDataTable(dt3);
            G1.NumberDataTable(dt4);

            dgv.DataSource = dt;
            dgv2.DataSource = dt2;
            dgv3.DataSource = dt3;
            dgv4.DataSource = dt4;
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        public static double RoundTo(double price, double rounding)
        {
            double newPrice = price;
            if (rounding < 0D)
                return newPrice;
            if (rounding == 1000D)
                newPrice = G1.Round(price, -3);
            else if (rounding == 500D)
            {
                double count = price / 500D;
                count = G1.RoundValue(count);
                int icount = Convert.ToInt32(count);
                count = (double)icount;
                newPrice = count * 500D;
            }
            else if (rounding == 100D)
            {
                newPrice = G1.Round(price, -2);
            }
            else if (rounding == 95D)
            {
                newPrice = G1.Round(price, -2);
                newPrice = newPrice - 5D;
            }
            else if (rounding == 5D)
            {
                double count = price / 5D;
                count = G1.RoundValue(count);
                int icount = Convert.ToInt32(count);
                count = (double)icount;
                newPrice = count * 5D;
            }
            else
            {
                double count = price / rounding;
                count = G1.RoundValue(count);
                int icount = Convert.ToInt32(count);
                count = (double)icount;
                newPrice = count * rounding;
            }
            return newPrice;
        }
        /***********************************************************************************************/
        private void CalculateCosts(DataTable dt)
        {
            double cost = 0D;
            double markup = 0D;
            double casketCost = 0D;
            double packageCost = 0D;
            double rounding = 0D;
            string cmd = "";
            string gplGroup = "";
            string masterRecord = "";
            DataTable dx = null;
            string basicRecord = "";
            //string basics = "BASIC SERVICES OF FUNERAL DIRECTOR AND STAFF";
            //cmd = "Select * from `services` where `service` = '" + basics + "';";
            //dx = G1.get_db_data(cmd);
            //if (dx.Rows.Count > 0)
            //    basicRecord = dx.Rows[0]["record"].ObjToString();
            double basicPackage = GetGPGBasicPackage();
            bool gotMarkup = false;
            if (G1.get_column_number(dt, "markup") >= 0)
                gotMarkup = true;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    markup = 1D;
                    if (gotMarkup)
                        markup = dt.Rows[i]["markup"].ObjToDouble();
                    masterRecord = dt.Rows[i]["!masterRecord"].ObjToString();
                    cmd = "Select * from `casket_master` where `record` = '" + masterRecord + "';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                    {
                        casketCost = dx.Rows[0]["casketcost"].ObjToDouble();
                        casketCost = casketCost * markup;
                        rounding = dx.Rows[0]["round"].ObjToDouble();
                        if (rounding > 0D)
                            casketCost = RoundTo(casketCost, rounding);
                        dt.Rows[i]["casket"] = casketCost;
                        packageCost = casketCost + basicPackage;
                        dt.Rows[i]["package"] = packageCost;
                        //if ( !String.IsNullOrWhiteSpace ( basicRecord))
                        //{
                        //    gplGroup = dt.Rows[i]["GPL_Group"].ObjToString();
                        //    if ( !String.IsNullOrWhiteSpace ( gplGroup ))
                        //    {
                        //        cmd = "Select * from `packages` where `groupname` = '" + gplGroup + "' and `!serviceRecord` = '" + basicRecord + "';";
                        //        dx = G1.get_db_data(cmd);
                        //        if ( dx.Rows.Count > 0 )
                        //        {
                        //            packageCost = dx.Rows[0]["price"].ObjToDouble();
                        //            packageCost += casketCost;
                        //            dt.Rows[i]["package"] = packageCost;
                        //        }
                        //    }
                        //}
                    }
                }
                catch (Exception ex)
                {
                }
            }
        }
        /***********************************************************************************************/
        private void LoadData()
        {
            btnSave.Hide();
            modified = false;
            string location = GetGroup();
            if (location.ToUpper() == "ALL BATESVILLE CASKETS")
                return;
            if (location.ToUpper() == "CRECENT URNS")
                return;
            if (location.ToUpper() == "MASTER VAULT")
                return;
            this.Cursor = Cursors.WaitCursor;
            gridMain.Columns["round"].Visible = true;
            if (location.ToUpper() != "MASTER")
            {
                //picMoveFuture.Hide();
                //picMovePast.Hide();
                gridMain.Columns["markup"].Visible = true;
                gridMain.Columns["round"].Visible = false;
            }
            if (!Selecting)
            {
                if (location != "Master")
                {
                    LoadPackage();
                    LoadCasketLocations();
                    //tabControl1.TabPages.Remove(tabVaults);
                    //tabControl1.TabPages.Remove(tabUrns);
                    //tabControl1.TabPages.Remove(tabMisc);
                    pictureAdd.Hide();
                    pictureDelete.Hide();
                    btnInsert.Hide();
                    picRowDown.Hide();
                    picRowUp.Hide();
                    gridMain.Columns["markup"].Visible = true;
                    gridMain.Columns["round"].Visible = false;
                    gridMain2.Columns["markup"].Visible = true;
                    gridMain2.Columns["round"].Visible = false;
                    gridMain3.Columns["markup"].Visible = true;
                    gridMain3.Columns["round"].Visible = false;
                    gridMain4.Columns["markup"].Visible = true;
                    gridMain4.Columns["round"].Visible = false;
                    return;
                }
            }
            picMoveFuture.Show();
            picMovePast.Show();
            gridMain.Columns["markup"].Visible = false;
            //string cmd = "Select * from `casket_master` order by `record`;";
            string cmd = "Select * from `casket_master` order by `order`;";
            DataTable dt = null;
            if (Selecting && workingLocation && workDt != null)
                dt = workDt;
            else
            {
                dt = G1.get_db_data(cmd);
                dt.Columns.Add("num");
                dt.Columns.Add("mod");
                dt.Columns.Add("select");
                dt.Columns.Add("casket", Type.GetType("System.Double"));
                dt.Columns.Add("package", Type.GetType("System.Double"));
                dt.Columns.Add("total", Type.GetType("System.Double"));
            }
            if (G1.get_column_number(dt, "SameAsMaster") < 0)
                dt.Columns.Add("SameAsMaster");
            SetupSelection(dt);
            SetupSameAsMaster(dt);

            double basicPackage = GetGPGBasicPackage();

            double pastPrice = 0D;
            double rounding = 0D;
            double packagePrice = 0D;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                pastPrice = dt.Rows[i]["casketCost"].ObjToDouble();
                rounding = dt.Rows[i]["round"].ObjToDouble();
                if (rounding > 0D)
                    pastPrice = Caskets.RoundTo(pastPrice, rounding);
                dt.Rows[i]["casket"] = pastPrice;
                packagePrice = pastPrice + basicPackage;
                dt.Rows[i]["package"] = packagePrice;
            }

            LoadCasketLocations();

            if (gridMain.Columns["round"].Visible)
            {
                SetupMaster(dt);
                if (tabControl1.TabPages.Count <= 1)
                {
                    tabControl1.TabPages.Add(tabVaults);
                    tabControl1.TabPages.Add(tabUrns);
                    tabControl1.TabPages.Add(tabMisc);
                }
            }
            else
            {
                SetupMaster(dt);
                if (tabControl1.TabPages.Count <= 1)
                {
                    tabControl1.TabPages.Add(tabVaults);
                    tabControl1.TabPages.Add(tabUrns);
                    tabControl1.TabPages.Add(tabMisc);
                }
                gridMain2.Columns["select"].Visible = true;
                gridMain3.Columns["select"].Visible = true;
                gridMain4.Columns["select"].Visible = true;
            }

            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private DataTable MatchToMaster(DataTable dt, string type = "CASKET")
        {
            string service = "";
            DataRow[] dR = null;
            string cmd = "Select * from `casket_master` order by `order`, `record`;";
            DataTable dx = G1.get_db_data(cmd);

            if (G1.get_column_number(dt, "BAD") >= 0)
                dt.Columns.Remove("BAD");
            dt.Columns.Add("BAD");
            dx.Columns.Add("BAD");

            bool modified = false;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dt.Rows[i]["BAD"] = "BAD";
                service = dt.Rows[i]["casketdesc"].ObjToString();
                if (!String.IsNullOrWhiteSpace(service))
                {
                    dR = dx.Select("casketdesc='" + service + "'");
                    if (dR.Length > 0)
                    {
                        dt.Rows[i]["BAD"] = "";
                        dt.Rows[i]["order"] = dR[0]["order"].ObjToInt32();
                        dt.Rows[i]["!masterRecord"] = dR[0]["record"].ObjToString();
                        dR[0]["BAD"] = "GOOD";
                    }
                }
            }
            for (int i = dt.Rows.Count - 1; i >= 0; i--)
            {
                service = dt.Rows[i]["BAD"].ObjToString();
                if (service.ToUpper() == "BAD")
                {
                    dt.Rows.RemoveAt(i);
                    modified = true;
                }
            }
            string str = "";
            string casketCode = "";
            string classCode = "";
            int row = 0;
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                service = dx.Rows[i]["BAD"].ObjToString();
                if (service.ToUpper() == "GOOD")
                    continue;
                casketCode = dx.Rows[i]["casketcode"].ObjToString();
                classCode = Services.ClassifyCode(casketCode);

                service = dx.Rows[i]["casketdesc"].ObjToString();
                if (!String.IsNullOrWhiteSpace(casketCode))
                {
                    str = casketCode.Substring(0, 1);
                    if (type.ToUpper().IndexOf("CASKET") >= 0)
                    {
                        if (classCode != "URN" && classCode != "Vault" && classCode != "MISC")
                        {
                            dt.ImportRow(dx.Rows[i]);
                            row = dt.Rows.Count - 1;
                            dt.Rows[row]["!masterRecord"] = dx.Rows[i]["record"].ObjToString();
                            modified = true;
                        }
                    }
                    else if (type.ToUpper().IndexOf("VAULT") >= 0)
                    {
                        if (classCode.ToUpper() == "VAULT")
                        {
                            dt.ImportRow(dx.Rows[i]);
                            row = dt.Rows.Count - 1;
                            dt.Rows[row]["!masterRecord"] = dx.Rows[i]["record"].ObjToString();
                            modified = true;
                        }
                    }
                    else if (type.ToUpper().IndexOf("URN") >= 0)
                    {
                        if (classCode == "URN")
                        {
                            dt.ImportRow(dx.Rows[i]);
                            row = dt.Rows.Count - 1;
                            dt.Rows[row]["!masterRecord"] = dx.Rows[i]["record"].ObjToString();
                            modified = true;
                        }
                    }
                    else if (type.ToUpper().IndexOf("MISC") >= 0)
                    {
                        if (classCode.ToUpper().IndexOf("MISC") == 0)
                        {
                            dt.ImportRow(dx.Rows[i]);
                            row = dt.Rows.Count - 1;
                            dt.Rows[row]["!masterRecord"] = dx.Rows[i]["record"].ObjToString();
                            modified = true;
                        }
                    }
                }
            }

            DataView tempview = dt.DefaultView;
            tempview.Sort = "order asc";
            dt = tempview.ToTable();

            if (modified)
            {
                btnSave.Show();
                modified = true;
            }

            CleanupMarkups(dt);

            return dt;
        }
        /***********************************************************************************************/
        private void CleanupMarkups(DataTable dt)
        {
            if (G1.get_column_number(dt, "markup") < 0)
                return;
            bool gotCasket = false;
            if (G1.get_column_number(dt, "casket") >= 0)
                gotCasket = true;
            double markup = 0D;
            double cost = 0D;
            double futureCost = 0D;
            double price = 0D;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                markup = dt.Rows[i]["markup"].ObjToDouble();
                if (markup <= 0D)
                {
                    markup = 1D;
                    dt.Rows[i]["markup"] = 1D;
                }
                cost = dt.Rows[i]["casketcost"].ObjToDouble();
                price = cost * markup;
                price = G1.RoundValue(price);
                dt.Rows[i]["casketprice"] = price;
                if (gotCasket)
                    dt.Rows[i]["casket"] = price;
            }
        }
        /***********************************************************************************************/
        private void SetupMaster(DataTable dt)
        {
            DataTable dt2 = dt.Clone();
            DataTable dt3 = dt.Clone();
            DataTable dt4 = dt.Clone();

            string casketCode = "";
            string str = "";

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                casketCode = dt.Rows[i]["casketcode"].ObjToString();
                if (String.IsNullOrWhiteSpace(casketCode))
                    continue;
                if (casketCode.ToUpper().IndexOf("MISC") == 0)
                {
                    dt4.ImportRow(dt.Rows[i]);
                    continue;
                }
                str = casketCode.Substring(0, 1).ToUpper();
                if (str == "V")
                {
                    str = casketCode.Substring(1);
                    if (G1.validate_numeric(str))
                    {
                        dt2.ImportRow(dt.Rows[i]);
                        continue;
                    }
                }
                if (casketCode.Contains("URN"))
                {
                    dt3.ImportRow(dt.Rows[i]);
                    continue;
                }
                if (casketCode.Length >= 2)
                {
                    str = casketCode.Substring(0, 2).ToUpper();
                    if (str == "UV")
                    {
                        dt3.ImportRow(dt.Rows[i]);
                        continue;
                    }
                }
            }
            for (int i = (dt.Rows.Count - 1); i >= 0; i--)
            {
                casketCode = dt.Rows[i]["casketcode"].ObjToString();
                if (String.IsNullOrWhiteSpace(casketCode))
                    continue;
                if (casketCode.ToUpper().IndexOf("MISC") == 0)
                {
                    dt.Rows.RemoveAt(i);
                    continue;
                }
                str = casketCode.Substring(0, 1).ToUpper();
                if (str == "V" && casketCode.Length >= 3)
                {
                    dt.Rows.RemoveAt(i);
                    continue;
                }
                if (casketCode.Contains("URN"))
                {
                    dt.Rows.RemoveAt(i);
                    continue;
                }
                if (casketCode.Length >= 2)
                {
                    str = casketCode.Substring(0, 2).ToUpper();
                    if (str == "UV")
                    {
                        dt.Rows.RemoveAt(i);
                        continue;
                    }
                }
            }
            gridMain2.Columns["markup"].Visible = false;
            gridMain2.Columns["round"].Visible = true;
            gridMain2.Columns["type"].Visible = false;
            gridMain2.Columns["package"].Visible = false;
            gridMain2.Columns["total"].Visible = false;
            gridMain2.OptionsBehavior.ReadOnly = false;
            gridMain2.OptionsBehavior.Editable = true;
            gridMain2.Columns["casketdesc"].Caption = "Vault Description";
            gridMain2.Columns["casket"].Caption = "Vault $";

            gridMain3.Columns["markup"].Visible = false;
            gridMain3.Columns["round"].Visible = true;
            gridMain3.Columns["type"].Visible = false;
            gridMain3.Columns["package"].Visible = false;
            gridMain3.Columns["total"].Visible = false;
            gridMain3.OptionsBehavior.ReadOnly = false;
            gridMain3.OptionsBehavior.Editable = true;
            gridMain3.Columns["casketdesc"].Caption = "Urn Description";
            gridMain3.Columns["casket"].Caption = "Urn $";

            gridMain4.Columns["markup"].Visible = false;
            gridMain4.Columns["round"].Visible = true;
            gridMain4.Columns["type"].Visible = false;
            gridMain4.Columns["package"].Visible = false;
            gridMain4.Columns["total"].Visible = false;
            gridMain4.OptionsBehavior.ReadOnly = false;
            gridMain4.OptionsBehavior.Editable = true;
            gridMain4.Columns["casketdesc"].Caption = "Miscellaneous Description";
            gridMain4.Columns["casket"].Caption = "Misc $";

            G1.NumberDataTable(dt2);
            G1.NumberDataTable(dt3);
            G1.NumberDataTable(dt4);

            SetupSelection(dt2, this.repositoryItemCheckEdit2);
            SetupSelection(dt3, this.repositoryItemCheckEdit5);
            SetupSelection(dt4, this.repositoryItemCheckEdit7);

            dgv2.DataSource = dt2;
            dgv3.DataSource = dt3;
            dgv4.DataSource = dt4;
        }
        /***********************************************************************************************/
        private void FixArrowPosition(AdvBandedGridView gridMain)
        {
            int width = 0;
            int totalWidth = 0;
            for (int i = 0; i < gridMain.Columns.Count; i++)
            {
                if (gridMain.Columns[i].FieldName.ToUpper() == "FUTURECASKETCOST")
                {
                    width = gridMain.Columns[i].Width;
                    totalWidth += width / 2;
                    break;
                }
                if (gridMain.Columns[i].Visible)
                {
                    width = gridMain.Columns[i].Width;
                    totalWidth += width;
                }
            }
            int top = picMoveFuture.Top;
            int left = picMoveFuture.Left;
            int height = picMoveFuture.Height;
            width = picMoveFuture.Width;
            picMoveFuture.SetBounds(totalWidth, top, width, height);

            top = picMovePast.Top;
            left = totalWidth;
            height = picMovePast.Height;
            left += (gridMain.Columns["futureCasketCost"].Width / 2);
            left += (gridMain.Columns["pastCasketCost"].Width / 2);
            width = picMovePast.Width;
            picMovePast.SetBounds(left, top, width, height);

        }
        /***********************************************************************************************/
        private void SetupSelection(DataTable dt, DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew)
        {
            //DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew = this.repositoryItemCheckEdit3;
            selectnew.NullText = "";
            selectnew.ValueChecked = "1";
            selectnew.ValueUnchecked = "0";
            selectnew.ValueGrayed = "";
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["select"] = "0";
        }
        /***********************************************************************************************/
        private void SetupSelection(DataTable dt)
        {
            DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew = this.repositoryItemCheckEdit3;
            selectnew.NullText = "";
            selectnew.ValueChecked = "1";
            selectnew.ValueUnchecked = "0";
            selectnew.ValueGrayed = "";
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["select"] = "0";
        }
        /***********************************************************************************************/
        private void SetupSameAsMaster(DataTable dt)
        {
            DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew = this.repositoryItemCheckEdit1;
            selectnew.NullText = "";
            selectnew.ValueChecked = "1";
            selectnew.ValueUnchecked = "0";
            selectnew.ValueGrayed = "";
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["SameAsMaster"] = "0";
        }
        /***********************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (dgv.Visible)
                SetSpyGlass(gridMain);
            else if (dgv2.Visible)
                SetSpyGlass(gridMain2);
            else if (dgv3.Visible)
                SetSpyGlass(gridMain3);
            else if (dgv4.Visible)
                SetSpyGlass(gridMain4);
        }
        /***********************************************************************************************/
        private void SetSpyGlass(DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView grid)
        {
            G1.ShowHideFindPanel(grid);

            //if (grid.OptionsFind.AlwaysVisible == true)
            //    grid.OptionsFind.AlwaysVisible = false;
            //else
            //    grid.OptionsFind.AlwaysVisible = true;
        }
        /***********************************************************************************************/
        private void repositoryItemCheckEdit3_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            int rowHandle = gridMain.FocusedRowHandle;
            try
            {
                string select = dr["select"].ObjToString();
                string doit = "1";
                if (select == "1")
                    doit = "0";
                //string doit = "0";
                //if (select == "1")
                //    doit = "1";
                dr["select"] = doit;
            }
            catch (Exception ex)
            {

            }
            modified = true;
            if (!Selecting)
                btnSave.Show();
            ReCalcTotal();
            gridMain.RefreshData();
            gridMain.RefreshRow(rowHandle);
            dgv.Refresh();
        }
        /***************************************************************************************/
        public delegate void d_void_eventdone_dt(DataTable dt);
        public event d_void_eventdone_dt SelectDone;
        protected void OnSelectDone(DataTable dt)
        {
            SelectDone?.Invoke(dt);
        }
        /***********************************************************************************************/
        private void ReCalcTotal()
        {
            string select = "";
            double price = 0D;
            double total = 0D;
            string data = "";
            DataTable dt = (DataTable)dgv.DataSource;
            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    select = dt.Rows[i]["select"].ObjToString();
                    if (select == "1")
                    {
                        price = dt.Rows[i]["casketcost"].ObjToDouble();
                        total += price;
                        //data = dt.Rows[i]["data"].ObjToString();
                        //if (G1.validate_numeric(data))
                        //{
                        //    price = data.ObjToDouble();
                        //    total = total + price;
                        //    dt.Rows[i]["total"] = total;
                        //}
                    }
                    else
                        dt.Rows[i]["total"] = 0D;
                }
            }
            catch (Exception ex)
            {

            }
            //labBalanceDue.Text = "$" + G1.ReformatMoney(total);
            labBalanceDue.Hide();
            labBalDue.Hide();
        }
        /***********************************************************************************************/
        private bool CheckForSaving()
        {
            if (!modified)
                return true;
            DialogResult result = MessageBox.Show("***Question***\nMerchandise has been modified!\nWould you like to save your changes?", "Select Merchandise Dialog", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (result == DialogResult.Cancel)
                return false;
            modified = false;
            if (result == DialogResult.No)
                return true;
            if (workingLocation)
                return true;
            DataTable dt = (DataTable)dgv.DataSource;
            if (String.IsNullOrWhiteSpace(loadededLocation) || String.IsNullOrWhiteSpace(loadedPackage))
                return true;
            if (loadededLocation == "MASTER" && loadedPackage == "MASTER")
                SaveServices();
            else
                SaveServices(dt);
            return true;
        }
        /***********************************************************************************************/
        private void Services_FormClosing(object sender, FormClosingEventArgs e)
        {
            loadededLocation = cmbGroups.Text.ToUpper();
            loadedPackage = cmbPackage.Text.ToUpper();
            if (String.IsNullOrWhiteSpace(loadedPackage))
                loadedPackage = "MASTER";
            if (loadededLocation == "MASTER" && loadedPackage == "MASTER" && !Selecting)
            {
                modified = false;
                if (casketsModified)
                    modified = true;
                else if (vaultsModified)
                    modified = true;
                else if (urnsModified)
                    modified = true;
                else if (miscModified)
                    modified = true;
            }
            else
            {
                if (btnSave.Visible)
                    modified = true;
            }

            if (!modified)
                return;
            DialogResult result = MessageBox.Show("***Question***\nMerchandise has been modified!\nWould you like to save your changes?", "Select Merchandise Dialog", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (result == DialogResult.Cancel)
            {
                e.Cancel = true;
                return;
            }
            modified = false;
            if (result == DialogResult.No)
                return;
            DataTable dt = (DataTable)dgv.DataSource;
            if (!Selecting)
            {
                loadededLocation = cmbGroups.Text.ToUpper();
                loadedPackage = cmbPackage.Text.ToUpper();
                if (String.IsNullOrWhiteSpace(loadedPackage))
                    loadedPackage = "MASTER";
                if (loadededLocation == "MASTER" && loadedPackage == "MASTER")
                {
                    if (casketsModified)
                    {
                        dt = (DataTable)dgv.DataSource;
                        SaveCasketMaster(dt);
                    }
                    if (vaultsModified)
                    {
                        dt = (DataTable)dgv2.DataSource;
                        SaveCasketMaster(dt);
                    }
                    if (urnsModified)
                    {
                        dt = (DataTable)dgv3.DataSource;
                        SaveCasketMaster(dt);
                    }
                    if (miscModified)
                    {
                        dt = (DataTable)dgv4.DataSource;
                        SaveCasketMaster(dt);
                    }
                    casketsModified = false;
                    vaultsModified = false;
                    urnsModified = false;
                    miscModified = false;
                }
                else
                {
                    SaveServices(dt);
                    modified = false;
                }
                return;
            }
            DataTable dx = dt.Clone();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (dt.Rows[i]["select"].ObjToString() == "1")
                    G1.copy_dt_row(dt, i, dx, dx.Rows.Count);
            }
            if (dgv2.Visible)
            {
                dt = (DataTable)dgv2.DataSource;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (dt.Rows[i]["select"].ObjToString() == "1")
                        G1.copy_dt_row(dt, i, dx, dx.Rows.Count);
                }
            }
            if (dgv3.Visible)
            {
                dt = (DataTable)dgv3.DataSource;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (dt.Rows[i]["select"].ObjToString() == "1")
                        G1.copy_dt_row(dt, i, dx, dx.Rows.Count);
                }
            }
            if (dgv4.Visible)
            {
                dt = (DataTable)dgv4.DataSource;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (dt.Rows[i]["select"].ObjToString() == "1")
                        G1.copy_dt_row(dt, i, dx, dx.Rows.Count);
                }
            }
            OnSelectDone(dx);
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
            printableComponentLink1.Landscape = false;

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
            printableComponentLink1.Landscape = false;

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
            if (Selecting)
                Printer.DrawQuad(6, 8, 4, 4, this.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            else
                Printer.DrawQuad(6, 8, 4, 4, "Services Report", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);


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
            Printer.DrawQuadBorder(1, 1, 12, 11, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /***********************************************************************************************/
        private void pictureAdd_Click(object sender, EventArgs e)
        {
            string group = GetGroup();
            if (group.ToUpper() != "MASTER")
            {
                //                LoadPackage(cmbPackage.Text.Trim());
                LoadPackage(group.Trim());
                return;
            }
            if (dgv.Visible)
                AddNewRow(gridMain, dgv);
            else if (dgv2.Visible)
                AddNewRow(gridMain2, dgv2);
            else if (dgv3.Visible)
                AddNewRow(gridMain3, dgv3);
            else if (dgv4.Visible)
                AddNewRow(gridMain4, dgv4);
            //DataTable dt = (DataTable)dgv.DataSource;
            //int lines = 1;
            //for (int i = 0; i < lines; i++)
            //{
            //    DataRow dRow = dt.NewRow();
            //    dRow["num"] = dt.Rows.Count.ObjToInt32();
            //    dt.Rows.Add(dRow);
            //}
            //dgv.DataSource = dt;

            //int row = dt.Rows.Count - 1;
            //gridMain.SelectRow(row);
            //gridMain.FocusedRowHandle = row;
            //dgv.RefreshDataSource();
            //dgv.Refresh();
        }
        /***********************************************************************************************/
        private void AddNewRow(AdvBandedGridView gridMain, GridControl dgv)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            int lines = 1;
            for (int i = 0; i < lines; i++)
            {
                DataRow dRow = dt.NewRow();
                dRow["num"] = dt.Rows.Count.ObjToInt32();
                if (dgv == dgv4)
                    dRow["casketcode"] = "misc";
                dt.Rows.Add(dRow);
            }
            dgv.DataSource = dt;

            int row = dt.Rows.Count - 1;
            gridMain.SelectRow(row);
            gridMain.FocusedRowHandle = row;
            dgv.RefreshDataSource();
            dgv.Refresh();
        }
        /***********************************************************************************************/
        private void pictureDelete_Click(object sender, EventArgs e)
        {
            if (dgv.Visible)
                DeleteRow(gridMain, dgv);
            else if (dgv2.Visible)
                DeleteRow(gridMain2, dgv2);
            else if (dgv3.Visible)
                DeleteRow(gridMain3, dgv3);
            else if (dgv4.Visible)
                DeleteRow(gridMain4, dgv4);

            //DataTable dt = (DataTable)dgv.DataSource;
            //DataRow dr = gridMain.GetFocusedDataRow();
            //string record = dr["record"].ObjToString();
            //string service = dr["casketdesc"].ObjToString();
            //DialogResult result = MessageBox.Show("***Question*** Are you sure you want to delete (" + service + ") ?", "Delete Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            //if (result == DialogResult.No)
            //    return;

            //int[] rows = gridMain.GetSelectedRows();
            //int dtRow = 0;
            //int firstRow = 0;
            //if (rows.Length > 0)
            //    firstRow = rows[0];
            //int row = 0;
            //try
            //{
            //    loading = true;
            //    for (int i = 0; i < rows.Length; i++)
            //    {
            //        row = rows[i];
            //        dtRow = gridMain.GetDataSourceRowIndex(row);
            //        if (dtRow < 0 || dtRow > (dt.Rows.Count - 1))
            //        {
            //            continue;
            //        }
            //        var dRow = gridMain.GetDataRow(row);
            //        if (dRow != null)
            //            dRow["mod"] = "D";
            //        dt.Rows[dtRow]["mod"] = "D";
            //        modified = true;
            //        if ( !Selecting )
            //            btnSave.Show();
            //    }
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show("*ERROR*** " + ex.Message.ToString());
            //}

            ////            gridMain.FocusedRowHandle = firstRow;
            //loading = false;
            //if (firstRow > (dt.Rows.Count - 1))
            //    firstRow = (dt.Rows.Count - 1);
            //dgv.DataSource = dt;
            //gridMain.RefreshData();
            //dgv.Refresh();

            //gridMain.FocusedRowHandle = firstRow;
            //gridMain.SelectRow(firstRow);
        }
        /***********************************************************************************************/
        private void DeleteRow(AdvBandedGridView gridMain, GridControl dgv)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            string service = dr["casketdesc"].ObjToString();
            DialogResult result = MessageBox.Show("***Question*** Are you sure you want to delete (" + service + ") ?", "Delete Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;

            int[] rows = gridMain.GetSelectedRows();
            int dtRow = 0;
            int firstRow = 0;
            if (rows.Length > 0)
                firstRow = rows[0];
            int row = 0;
            try
            {
                loading = true;
                for (int i = 0; i < rows.Length; i++)
                {
                    row = rows[i];
                    dtRow = gridMain.GetDataSourceRowIndex(row);
                    if (dtRow < 0 || dtRow > (dt.Rows.Count - 1))
                    {
                        continue;
                    }
                    var dRow = gridMain.GetDataRow(row);
                    if (dRow != null)
                        dRow["mod"] = "D";
                    dt.Rows[dtRow]["mod"] = "D";
                    modified = true;
                    if (!Selecting)
                        btnSave.Show();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("*ERROR*** " + ex.Message.ToString());
            }

            //            gridMain.FocusedRowHandle = firstRow;
            loading = false;
            if (firstRow > (dt.Rows.Count - 1))
                firstRow = (dt.Rows.Count - 1);
            dgv.DataSource = dt;
            gridMain.RefreshData();
            dgv.Refresh();

            gridMain.FocusedRowHandle = firstRow;
            gridMain.SelectRow(firstRow);
        }
        /***********************************************************************************************/
        private void picRowUp_Click(object sender, EventArgs e)
        {
            if (dgv.Visible)
            {
                MoveRowUp(gridMain, dgv);
                casketsModified = true;
            }
            else if (dgv2.Visible)
            {
                MoveRowUp(gridMain2, dgv2);
                vaultsModified = true;
            }
            else if (dgv3.Visible)
            {
                MoveRowUp(gridMain3, dgv3);
                urnsModified = true;
            }
            else if (dgv4.Visible)
            {
                MoveRowUp(gridMain4, dgv4);
                miscModified = true;
            }

            //DataTable dt = (DataTable)dgv.DataSource;
            //if (dt.Rows.Count <= 0)
            //    return;
            //DataRow dr = gridMain.GetFocusedDataRow();
            //int rowHandle = gridMain.FocusedRowHandle;
            //if (rowHandle == 0)
            //    return; // Already at the first row
            ////MoveRowUp(dt, rowHandle);
            //massRowsUp(dt, rowHandle);
            //dt.AcceptChanges();
            //dgv.DataSource = dt;
            //gridMain.ClearSelection();
            //gridMain.SelectRow(rowHandle - 1);
            //gridMain.FocusedRowHandle = rowHandle - 1;
            //gridMain.RefreshData();
            //dgv.Refresh();
        }
        /***********************************************************************************************/
        private void MoveRowUp(AdvBandedGridView gridMain, GridControl dgv)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt.Rows.Count <= 0)
                return;
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowHandle = gridMain.FocusedRowHandle;
            if (rowHandle == 0)
                return; // Already at the first row

            massRowsUp(gridMain, dgv, dt, rowHandle);

            dt.AcceptChanges();
            dgv.DataSource = dt;
            gridMain.ClearSelection();
            gridMain.SelectRow(rowHandle - 1);
            gridMain.FocusedRowHandle = rowHandle - 1;
            gridMain.RefreshData();
            dgv.Refresh();
        }
        /***********************************************************************************************/
        private void massRowsUp(AdvBandedGridView gridMain, GridControl dgv, DataTable dt, int row)
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
                    if (!Selecting)
                        btnSave.Show();
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
        private void picRowDown_Click(object sender, EventArgs e)
        {
            if (dgv.Visible)
                PicRowDown(gridMain, dgv);
            else if (dgv2.Visible)
                PicRowDown(gridMain2, dgv2);
            else if (dgv3.Visible)
                PicRowDown(gridMain3, dgv3);
            else if (dgv4.Visible)
                PicRowDown(gridMain4, dgv4);

            //DataTable dt = (DataTable)dgv.DataSource;
            //if (dt.Rows.Count <= 0)
            //    return;
            //DataRow dr = gridMain.GetFocusedDataRow();
            //int rowHandle = gridMain.FocusedRowHandle;
            //if (rowHandle == (dt.Rows.Count - 1))
            //    return; // Already at the last row
            //MoveRowDown(dt, rowHandle);
            //dt.AcceptChanges();
            //dgv.DataSource = dt;
            //gridMain.ClearSelection();
            //gridMain.SelectRow(rowHandle + 1);
            //gridMain.FocusedRowHandle = rowHandle + 1;
            //gridMain.RefreshData();
            //dgv.Refresh();
        }
        /***************************************************************************************/
        private void PicRowDown(AdvBandedGridView gridMain, GridControl dgv)
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
        /***********************************************************************************************/
        private void btnInsert_Click(object sender, EventArgs e)
        {
            if (dgv.Visible)
                InsertRow(gridMain, dgv);
            else if (dgv2.Visible)
                InsertRow(gridMain2, dgv2);
            else if (dgv3.Visible)
                InsertRow(gridMain3, dgv3);
            else if (dgv4.Visible)
                InsertRow(gridMain4, dgv4);

            //DataTable dt = (DataTable)dgv.DataSource;
            //if (dt.Rows.Count <= 0)
            //    return;
            //DataRow dr = gridMain.GetFocusedDataRow();
            //int rowHandle = gridMain.FocusedRowHandle;
            //int dtRow = gridMain.GetDataSourceRowIndex(rowHandle);
            //if (dtRow < 0 || dtRow > (dt.Rows.Count - 1))
            //    return;
            //if (rowHandle == (dt.Rows.Count - 1))
            //    return; // Already at the last row
            //DataRow dRow = dt.NewRow();
            //dt.Rows.InsertAt(dRow, dtRow);
            //G1.NumberDataTable(dt);
            //dt.AcceptChanges();
            //dgv.DataSource = dt;
            //gridMain.ClearSelection();
            //gridMain.RefreshData();
            //gridMain.FocusedRowHandle = rowHandle + 1;
            //gridMain.SelectRow(rowHandle + 1);
            //dgv.Refresh();
        }
        /***********************************************************************************************/
        private void InsertRow(AdvBandedGridView gridMain, GridControl dgv)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt.Rows.Count <= 0)
                return;
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowHandle = gridMain.FocusedRowHandle;
            int dtRow = gridMain.GetDataSourceRowIndex(rowHandle);
            if (dtRow < 0 || dtRow > (dt.Rows.Count - 1))
                return;
            if (rowHandle == (dt.Rows.Count - 1))
                return; // Already at the last row
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
        }
        /***********************************************************************************************/
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        /***********************************************************************************************/
        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditHelp helpForm = new EditHelp("customers");
            helpForm.Show();
        }
        /***********************************************************************************************/
        private void SaveServices()
        {
            if (1 == 1)
                return; // Don't want to mess up services here
            string record = "";
            string service = "";
            string data = "";
            string type = "";
            string price = "";
            string mod = "";

            DataTable dt = (DataTable)dgv.DataSource;
            this.Cursor = Cursors.WaitCursor;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                record = dt.Rows[i]["record"].ObjToString();
                mod = dt.Rows[i]["mod"].ObjToString();
                if (mod.ToUpper() == "D")
                {
                    if (!String.IsNullOrWhiteSpace(record))
                        G1.delete_db_table("services", "record", record);
                    continue;
                }
                if (String.IsNullOrWhiteSpace(record))
                    record = G1.create_record("services", "service", "-1");
                if (G1.BadRecord("services", record))
                    continue;
                service = dt.Rows[i]["service"].ObjToString();
                price = dt.Rows[i]["price"].ObjToString();
                data = dt.Rows[i]["data"].ObjToString();
                type = dt.Rows[i]["type"].ObjToString();
                G1.update_db_table("services", "record", record, new string[] { "service", service, "price", price, "data", data, "type", type, "order", i.ToString() });
            }
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void CellModified(AdvBandedGridView gridMain, GridControl dgv, ref bool modified)
        {
            modified = true;
            if (!Selecting)
            {
                btnSave.Show();
                DataTable dt = (DataTable)dgv.DataSource;
                bool gotMarkup = false;
                if (G1.get_column_number(dt, "markup") >= 0)
                    gotMarkup = true;
                double casketCost = 0D;
                double price = 0D;
                double markup = 0D;
                double rounding = 0D;
                double basicPackage = GetGPGBasicPackage();
                DataRow dr = gridMain.GetFocusedDataRow();
                GridColumn currCol = gridMain.FocusedColumn;
                string currentColumn = currCol.FieldName;
                if (currentColumn.ToUpper() == "MARKUP" || currentColumn.ToUpper() == "ROUND")
                {
                    if (gotMarkup)
                        markup = dr["markup"].ObjToDouble();
                    casketCost = dr["casketcost"].ObjToDouble();
                    rounding = dr["round"].ObjToDouble();
                    //if (rounding > 0D)
                    //    casketCost = RoundTo(casketCost, rounding);
                    if (gotMarkup)
                        casketCost = casketCost * markup;
                    dr["casket"] = casketCost;
                    dr["package"] = casketCost + basicPackage;
                }
                else if ( currentColumn.ToUpper() == "CASKET" && gotMarkup)
                {
                    casketCost = dr["casketcost"].ObjToDouble();
                    rounding = dr["round"].ObjToDouble();
                    price = dr["casket"].ObjToDouble();
                    if (casketCost > 0D)
                    {
                        markup = price / casketCost;
                        markup = G1.RoundValue(markup);
                        dr["markup"] = markup;
                    }
                }
            }
        }
        /***********************************************************************************************/
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (dgv.Visible)
                CellModified(gridMain, dgv, ref casketsModified);
            else if (dgv2.Visible)
                CellModified(gridMain2, dgv2, ref vaultsModified);
            else if (dgv3.Visible)
                CellModified(gridMain3, dgv3, ref urnsModified);
            else if (dgv4.Visible)
                CellModified(gridMain4, dgv4, ref miscModified);

            if (casketsModified || vaultsModified || urnsModified || miscModified)
                btnSave.Show();

            //modified = true;
            //if (!Selecting)
            //{
            //    btnSave.Show();
            //    DataTable dt = (DataTable)dgv.DataSource;
            //    double casketCost = 0D;
            //    double markup = 0D;
            //    double basicPackage = GetGPGBasicPackage();               
            //    DataRow dr = gridMain.GetFocusedDataRow();
            //    if (e.Column.FieldName.Trim().ToUpper() == "MARKUP")
            //    {
            //        markup = dr["markup"].ObjToDouble();
            //        casketCost = dr["casketcost"].ObjToDouble();
            //        casketCost = casketCost * markup;
            //        dr["casket"] = casketCost;
            //        dr["package"] = casketCost + basicPackage;
            //    }
            //}
        }
        /***********************************************************************************************/
        private void gridMain_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            int row = e.ListSourceRow;
            DataTable dt = null;
            if ( dgv.Visible )
                dt = (DataTable)dgv.DataSource;
            else if (dgv2.Visible)
                dt = (DataTable)dgv2.DataSource;
            else if (dgv3.Visible)
                dt = (DataTable)dgv3.DataSource;
            else if (dgv4.Visible)
                dt = (DataTable)dgv4.DataSource;
            if (dt == null)
                return;
            if (G1.get_column_number(dt, "mod") < 0)
                return;
            if (dt.Rows.Count <= row)
                return;
            string delete = dt.Rows[row]["mod"].ObjToString();
            if (delete.ToUpper() == "D")
            {
                e.Visible = false;
                e.Handled = true;
            }
        }
        /***********************************************************************************************/
        private void gridMain_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
        {
            if (e.Column.FieldName.ToUpper() == "DATA")
            {
                if (e.RowHandle >= 0)
                {
                    string data = e.DisplayText.ObjToString();
                    if (G1.validate_numeric(data))
                    {
                        double dvalue = data.ObjToDouble();
                        e.DisplayText = G1.ReformatMoney(dvalue);
                    }
                }
            }
            else if (e.Column.FieldName.ToUpper() == "TYPE")
            {
                if (e.RowHandle >= 0)
                {
                    string data = e.DisplayText.ObjToString();
                    if ( data.ToUpper() == "CUSTOM")
                    {
                        e.Appearance.BackColor = Color.Yellow;
                        e.Appearance.ForeColor = Color.Black;
                    }
                }
            }
        }
        /***********************************************************************************************/
        private void gridMain_RowCellStyle(object sender, DevExpress.XtraGrid.Views.Grid.RowCellStyleEventArgs e)
        {
            GridView View = sender as GridView;
            if (e.Column.FieldName.ToUpper() == "DATA")
            {
                string str = View.GetRowCellValue(e.RowHandle, "data").ObjToString();
                if (str != null)
                {
                    if (G1.validate_numeric(str))
                    {
                        e.Appearance.TextOptions.HAlignment = HorzAlignment.Far;
                    }
                }
            }
            //else if (e.Column.FieldName.ToUpper() == "CASKETDESC")
            //{
            //    e.Appearance.Font = new Font("Times New Roman", 24);
            //}
        }
        /***********************************************************************************************/
        private void LoadGPLGroupCombo()
        {
            string cmd = "Select * from `funeral_groups` ORDER BY `order`,`record`;";
            DataTable dt = G1.get_db_data(cmd);
            string groupname = "";
            cmbGPLGroup.Items.Clear();
            string firstGroup = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                groupname = dt.Rows[i]["shortname"].ObjToString();
                if (String.IsNullOrWhiteSpace(firstGroup))
                    firstGroup = groupname;
                cmbGPLGroup.Items.Add(groupname);
            }
            cmbGPLGroup.Text = firstGroup;
        }
        /***********************************************************************************************/
        private void LoadGroupCombo()
        {
            string cmd = "Select * from `casket_groups` ORDER BY `order`,`record`;";
            DataTable dt = G1.get_db_data(cmd);
            string groupname = "";
            string name = "";
            string locationCode = "";
            string str = "";
            cmbGroups.Items.Clear();
            cmbGroups.Items.Add("Master");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                groupname = dt.Rows[i]["shortname"].ObjToString();
                cmbGroups.Items.Add(groupname);
            }
            cmbGroups.Text = "Master";
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
            loading = true;
            cmbPackage.Items.Clear();
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
            cmbPackage.Text = "Master";
            loading = false;
        }
        /***********************************************************************************************/
        private string GetGroup()
        {
            string location = cmbGroups.Text;
            if (location.ToUpper() == "MASTER")
                return location;
            return location;
        }
        /***********************************************************************************************/
        private void cmbLocation_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            if (!CheckForSaving())
                return;
            LoadData();
        }
        /***********************************************************************************************/
        private void btnAddPackage_Click(object sender, EventArgs e)
        {
            string packName = "";
            using (Ask askForm = new Ask("Enter New Package Name?"))
            {
                askForm.Text = "";
                askForm.ShowDialog();
                if (askForm.DialogResult != System.Windows.Forms.DialogResult.OK)
                    return;
                packName = askForm.Answer;
                if (String.IsNullOrWhiteSpace(packName))
                    return;
            }
            LoadPackage(packName);
        }
        /***********************************************************************************************/
        private void LoadPackage(string packName)
        {
            loadedPackage = packName;
            string group = GetGroup();
            loadededLocation = group;
            string cmd = "Select * from `casket_packages` p JOIN `casket_master` s ON p.`!masterRecord` = s.`record` where p.`groupname` = '" + group + "';";
            DataTable dt = G1.get_db_data(cmd);
            Caskets serviceForm = new Caskets("CasketPackages", true, dt, "");
            serviceForm.SelectDone += ServiceForm_SelectDone;
            serviceForm.Show();
        }
        ///***********************************************************************************************/
        //private void LoadPackage(string packName)
        //{
        //    loadedPackage = packName;
        //    string group = GetGroup();
        //    loadededLocation = group;
        //    string cmd = "Select * from `packages` p JOIN `services` s ON p.`!serviceRecord` = s.`record` where p.`groupname` = '" + group + "' and p.`PackageName` = '" + packName + "';";
        //    DataTable dt = G1.get_db_data(cmd);
        //    Services serviceForm = new Services("Packages", true, dt, "");
        //    serviceForm.SelectDone += ServiceForm_SelectDone;
        //    serviceForm.Show();
        //}
        /***********************************************************************************************/
        private void ServiceForm_SelectDone(DataTable dt)
        {
            SaveServices(dt);
            if (G1.get_column_number(dt, "num") < 0)
                dt.Columns.Add("num");
            G1.NumberDataTable(dt);
            if ( !alreadyLoaded )
                dgv.DataSource = dt;
            alreadyLoaded = false;
        }
        /***********************************************************************************************/
        private DataTable CombineMerchandise ()
        {
            DataTable cDt = (DataTable)dgv.DataSource;
            DataTable dt = (DataTable)dgv2.DataSource;
            for (int i = 0; i < dt.Rows.Count; i++)
                cDt.ImportRow(dt.Rows[i]);

            dt = (DataTable)dgv3.DataSource;
            for (int i = 0; i < dt.Rows.Count; i++)
                cDt.ImportRow(dt.Rows[i]);

            dt = (DataTable)dgv4.DataSource;
            for (int i = 0; i < dt.Rows.Count; i++)
                cDt.ImportRow(dt.Rows[i]);

            return cDt;
        }
        /***********************************************************************************************/
        private void SaveServices(DataTable dt)
        {
            DataTable cDt = CombineMerchandise();
            SaveGroupServices(cDt);

            casketsModified = false;
            vaultsModified = false;
            urnsModified = false;
            miscModified = false;

            btnSave.Hide();
        }
        /***********************************************************************************************/
        private void SaveGroupServices(DataTable dt)
        {
            if (workingLocation)
                return;
            this.Cursor = Cursors.WaitCursor;
            string service = "";
            string serviceRecord = "";
            string data = "";
            double price = 0D;
            double cost = 0D;
            string type = "";
            string record = "";
            string mod = "";
            string order = "";
            int recordCol = G1.get_column_number(dt, "record");
            if (G1.get_column_number(dt, "!masterRecord") >= 0)
                recordCol = G1.get_column_number(dt, "!masterRecord");
            if (String.IsNullOrWhiteSpace(loadededLocation))
            {
                MessageBox.Show("***ERROR*** Empty Location");
                return;
            }

            string cmd = "";

            try
            {
                cmd = "Delete from `casket_packages_old` where `groupname` = '" + loadededLocation + "';";
                G1.get_db_data(cmd);
            }
            catch ( Exception ex)
            {
                DevExpress.XtraEditors.XtraMessageBox.Show("***ERROR*** " + ex.Message.ToString(), "Deleting Casket Backup Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            double markup = 0D;
            DataRow[] dR = null;
            bool gotType = false;
            DataTable tDt = null;
            string gplGroup = cmbGPLGroup.Text;

            try
            {
                cmd = "Select * from `casket_packages` where `groupname` = '" + loadededLocation + "';"; // Get Old Data
                tDt = G1.get_db_data(cmd);

                recordCol = G1.get_column_number(tDt, "record");
                if (G1.get_column_number(tDt, "!masterRecord") >= 0)
                    recordCol = G1.get_column_number(tDt, "!masterRecord");

                for (int i = 0; i < tDt.Rows.Count; i++)
                {
                    serviceRecord = tDt.Rows[i][recordCol].ObjToString();
                    price = tDt.Rows[i]["price"].ObjToDouble();
                    markup = tDt.Rows[i]["markup"].ObjToDouble();
                    order = tDt.Rows[i]["order"].ObjToString();
                    data = tDt.Rows[i]["data"].ObjToString();
                    record = G1.create_record("casket_packages_old", "groupname", "-1");
                    if (G1.BadRecord("casket_packages_old", record))
                        continue;
                    G1.update_db_table("casket_packages_old", "record", record, new string[] { "groupname", loadededLocation, "!masterRecord", serviceRecord, "price", price.ToString(), "GPL_Group", gplGroup, "markup", markup.ToString(), "data", data, "order", order });
                }
            }
            catch ( Exception ex)
            {
                DevExpress.XtraEditors.XtraMessageBox.Show("***ERROR*** " + ex.Message.ToString(), "Creating Casket Backup Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            tDt = (DataTable)dgv.DataSource;

            recordCol = G1.get_column_number(dt, "record");
            if (G1.get_column_number(dt, "!masterRecord") >= 0)
                recordCol = G1.get_column_number(dt, "!masterRecord");

            cmd = "Delete from `casket_packages` where `groupname` = '" + loadededLocation + "';"; // Delete Real Current Data
            G1.get_db_data(cmd);

            markup = 0D;
            dR = null;
            gotType = false;
            if (G1.get_column_number(dt, "type") >= 0)
                gotType = true;
            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    mod = dt.Rows[i]["mod"].ObjToString().ToUpper();
                    if (mod == "D")
                        continue;
                    serviceRecord = dt.Rows[i][recordCol].ObjToString();
                    service = dt.Rows[i]["casketdesc"].ObjToString();
                    if ( service.ToUpper().IndexOf ( "RAMMA") >= 0 )
                    {
                    }
                    //                data = dt.Rows[i]["data"].ObjToString();
                    if (gotType)
                    {
                        type = dt.Rows[i]["type"].ObjToString();
                        if (type.Trim().ToUpper() == "CUSTOM")
                            continue;
                    }
                    price = dt.Rows[i]["casketcost"].ObjToDouble();
                    if ( !String.IsNullOrWhiteSpace ( serviceRecord))
                        dR = tDt.Select("!masterRecord='" + serviceRecord + "'");
                    markup = 1.0D;
                    if (dR.Length > 0)
                        markup = dR[0]["markup"].ObjToDouble();

                    record = G1.create_record("casket_packages", "groupname", "-1");
                    if (G1.BadRecord("casket_packages", record))
                        continue;
                    G1.update_db_table("casket_packages", "record", record, new string[] { "groupname", loadededLocation, "!masterRecord", serviceRecord, "price", price.ToString(), "GPL_Group", gplGroup, "markup", markup.ToString() });
                }
            }
            catch ( Exception ex)
            {
                MessageBox.Show("***ERROR*** " + ex.Message.ToString());
                this.Cursor = Cursors.Default;
                return;
            }
            if (G1.get_column_number(dt, "num") < 0)
                dt.Columns.Add("num");
            G1.NumberDataTable(dt);

            cmd = "Select * from `casket_packages` p LEFT JOIN `casket_master` s ON p.`!masterRecord` = s.`record` where ";
            cmd += " `groupname` = '" + loadededLocation + "' ";
            cmd += ";";

            dt = G1.get_db_data(cmd);
            dt.Columns.Add("num");
            dt.Columns.Add("mod");
            dt.Columns.Add("select");
            dt.Columns.Add("casket", Type.GetType("System.Double"));
            dt.Columns.Add("package", Type.GetType("System.Double"));
            dt.Columns.Add("total", Type.GetType("System.Double"));

            SetupSelection(dt);


            //cmd = "Select * from `casket_packages` where `groupname` = '" + loadededLocation + "';";
            //dt = G1.get_db_data(cmd);

            CalculateCosts(dt);
            dgv.DataSource = dt;
            alreadyLoaded = true;
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void cmbPackage_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            if (!CheckForSaving())
                return;
            LoadData();
        }
        /***********************************************************************************************/
        private void repositoryItemCheckEdit1_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string service = dr["service"].ObjToString();
            string record = dr["record"].ObjToString();
            int rowHandle = gridMain.FocusedRowHandle;
            string select = dr["SameAsMaster"].ObjToString();
            string doit = "1";
            if (select == "1")
                doit = "0";
            dr["SameAsMaster"] = doit;
            DataTable dt = (DataTable)dgv.DataSource;
            if (doit == "1")
            {
                string cmd = "Select * from `services` where `record` = '" + record + "';";
                DataTable dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    dr["price"] = dx.Rows[0]["price"].ObjToDouble();
                    dr["data"] = dx.Rows[0]["data"].ObjToDouble();
                }
            }
            else
            {
                DataRow[] dRow = workDt.Select("service='" + service + "'");
                if (dRow.Length > 0)
                {
                    dr["price"] = dRow[0]["price"].ObjToDouble();
                    dr["data"] = dRow[0]["cost"].ObjToDouble();
                }
            }
            modified = true;
            if ( !Selecting )
                btnSave.Show();
            ReCalcTotal();
            gridMain.RefreshData();
            gridMain.RefreshRow(rowHandle);
            dgv.Refresh();
        }
        /***********************************************************************************************/
        private void importCasketMasterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Import importForm = new Import();
            importForm.SelectDone += ImportForm_SelectDone;
            importForm.Show();
        }
        /***********************************************************************************************/
        private void ImportForm_SelectDone(DataTable dt)
        {
            if (dt == null)
                return;
            if (dt.Rows.Count <= 0)
                return;
            DialogResult result = MessageBox.Show("***Question***\nWould you like to first CLEAR the Casket Database before Import?", "Clear Casket Database Dialog", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if ( result == DialogResult.Yes )
            {
                string cmd = "DELETE FROM `casket_master` where `record` > '0';";
                G1.get_db_data(cmd);
            }

            string casketCode = "";
            string casketDesc = "";
            string price = "";
            string casketPrice = "";
            string record = "";
            string chr = "";
            string str = "";
            bool vault = false;
            bool urn = false;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                casketCode = dt.Rows[i][1].ObjToString();
                if (String.IsNullOrWhiteSpace(casketCode))
                    continue;

                vault = false;
                urn = false;

                chr = casketCode.Substring(0, 1).ToUpper();
                if (chr == "V" && casketCode.Length == 3)
                    vault = true;
                if (chr == "V" )
                    vault = true;

                if (casketCode.IndexOf("URN") >= 0)
                    urn = true;
                else if (casketCode.Length >= 2)
                {
                    str = casketCode.Substring(0, 2).ToUpper();
                    if (str == "UV")
                        urn = true;
                }

                if (urn)
                    vault = false;
                if (!vault)
                    casketDesc = dt.Rows[i][3].ObjToString();
                else
                    casketDesc = dt.Rows[i][2].ObjToString();
                if (String.IsNullOrWhiteSpace(casketDesc))
                    continue;

                if (String.IsNullOrWhiteSpace(casketDesc))
                    continue;
                price = dt.Rows[i][4].ObjToString();
                if (String.IsNullOrWhiteSpace(price))
                    continue;
                price = price.Replace("$", "");
                price = price.Replace(",", "");
                if (!G1.validate_numeric(price))
                    continue;
                casketDesc = G1.protect_data(casketDesc);
                casketPrice = "";
                if ( vault )
                {
                    casketPrice = dt.Rows[i][6].ObjToString();
                    if ( !String.IsNullOrWhiteSpace ( casketPrice ))
                    {
                        if ( urn)
                        {
                            string[] Lines = casketPrice.Split(',');
                            if (Lines.Length > 1)
                                casketPrice = Lines[1];
                        }
                        casketPrice = casketPrice.Replace("$", "");
                        casketPrice = casketPrice.Replace(",", "");
                        if (!G1.validate_numeric(casketPrice))
                            casketPrice = "";
                    }
                }
                record = G1.create_record("casket_master", "casketdesc", "-1");
                if (G1.BadRecord("casket_master", record))
                    continue;
                G1.update_db_table("casket_master", "record", record, new string[] { "casketdesc", casketDesc, "casketcode", casketCode, "casketcost", price, "casketprice", casketPrice });
            }
        }
        /***********************************************************************************************/
        private void SaveCasketMaster( DataTable dt )
        {
            this.Cursor = Cursors.WaitCursor;
            if (dgv.Visible)
                dt = (DataTable)dgv.DataSource;
            else if (dgv2.Visible)
                dt = (DataTable)dgv2.DataSource;
            else if (dgv3.Visible)
                dt = (DataTable)dgv3.DataSource;
            else if (dgv4.Visible)
                dt = (DataTable)dgv4.DataSource;
            string casketCode = "";
            string casketDesc = "";
            string price = "";
            string futurePrice = "";
            string pastPrice = "";
            string casketPrice = "";
            string markup = "";
            string record = "";
            string mod = "";
            string chr = "";
            string round = "";
            bool gotMarkup = false;
            if (G1.get_column_number(dt, "markup") >= 0)
                gotMarkup = true;
            bool gotMaster = false;
            if (G1.get_column_number(dt, "") >= 0)
                gotMaster = true;
            bool vault = false;
            bool urn = false;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    mod = dt.Rows[i]["mod"].ObjToString();
                    if (mod == "D")
                    {
                        record = dt.Rows[i]["record"].ObjToString();
                        if (!String.IsNullOrWhiteSpace(record))
                            G1.delete_db_table("casket_master", "record", record);
                        continue;
                    }
                    casketCode = dt.Rows[i]["casketcode"].ObjToString();
                    if (String.IsNullOrWhiteSpace(casketCode))
                        continue;

                    vault = false;
                    chr = casketCode.Substring(0, 1).ToUpper();
                    if (chr == "V")
                        vault = true;
                    if (casketCode.IndexOf("URN") >= 0)
                    {
                        urn = true;
                        vault = true;
                    }
                    casketDesc = dt.Rows[i]["casketdesc"].ObjToString();
                    if (String.IsNullOrWhiteSpace(casketDesc))
                        continue;

                    futurePrice = dt.Rows[i]["futureCasketCost"].ObjToString();
                    futurePrice = futurePrice.Replace("$", "");
                    futurePrice = futurePrice.Replace(",", "");

                    pastPrice = dt.Rows[i]["pastCasketCost"].ObjToString();
                    pastPrice = pastPrice.Replace("$", "");
                    pastPrice = pastPrice.Replace(",", "");

                    price = dt.Rows[i]["casketcost"].ObjToString();
                    if (String.IsNullOrWhiteSpace(price))
                        continue;
                    price = price.Replace("$", "");
                    price = price.Replace(",", "");
                    if (!G1.validate_numeric(price))
                        continue;
                    casketDesc = G1.protect_data(casketDesc);
                    casketPrice = "";
                    if (vault)
                    {
                        casketPrice = dt.Rows[i]["casketprice"].ObjToString();
                        if (!String.IsNullOrWhiteSpace(casketPrice))
                        {
                            if (urn)
                            {
                                string[] Lines = casketPrice.Split(',');
                                if (Lines.Length > 1)
                                    casketPrice = Lines[1];
                            }
                            casketPrice = casketPrice.Replace("$", "");
                            casketPrice = casketPrice.Replace(",", "");
                            if (!G1.validate_numeric(casketPrice))
                                casketPrice = "";
                        }
                    }
                    //                record = dt.Rows[i]["!masterRecord"].ObjToString();
                    round = dt.Rows[i]["round"].ObjToString();
                    record = dt.Rows[i]["record"].ObjToString();
                    if (String.IsNullOrWhiteSpace(record))
                        record = G1.create_record("casket_master", "casketdesc", "-1");
                    if (G1.BadRecord("casket_master", record))
                        continue;
                    G1.update_db_table("casket_master", "record", record, new string[] { "casketdesc", casketDesc, "casketcode", casketCode, "casketcost", price, "casketprice", casketPrice, "futureCasketCost", futurePrice, "pastCasketCost", pastPrice, "round", round, "order", i.ToString() });
                    if (gotMarkup)
                    {
                        record = dt.Rows[i]["record"].ObjToString();
                        markup = dt.Rows[i]["markup"].ObjToString();
                        markup = markup.Replace("$", "");
                        markup = markup.Replace(",", "");
                        G1.update_db_table("casket_packages", "record", record, new string[] { "markup", markup });
                    }
                }
                catch ( Exception ex)
                {
                }
            }
            if (dgv.Visible)
                casketsModified = false;
            else if (dgv2.Visible)
                vaultsModified = false;
            else if (dgv3.Visible)
                urnsModified = false;
            else if (dgv4.Visible)
                miscModified = false;
            btnSave.Hide();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void importCasketGroupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Import importForm = new Import();
            importForm.SelectDone += ImportForm_SelectDone1;
            importForm.Show();
        }
        /***********************************************************************************************/
        private void ImportForm_SelectDone1(DataTable dt)
        {
            if (dt == null)
                return;
            if (dt.Rows.Count <= 0)
                return;
            string casketCode = "";
            string casketDesc = "";
            string price = "";
            string record = "";
            string masterRecord = "";
            string cmd = "";
            int goodCount = 0;
            int badCount = 0;
            DataTable dx = null;
            double cost = 0D;
            double markup = 0D;
            string str = "";
            string groupName = cmbGroups.Text.Trim();
            if (String.IsNullOrWhiteSpace(groupName))
                return;
            if ( groupName.Trim().ToUpper() == "MASTER")
            {
                MessageBox.Show("***ERROR*** Cannot import Groups as Master");
                return;
            }
            string gplGroup = cmbGPLGroup.Text.Trim();
            if (String.IsNullOrWhiteSpace(gplGroup))
            {
                MessageBox.Show("***ERROR*** You MUST assign a GPL Group to Assign to Casket Group!");
                return;
            }
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                casketCode = dt.Rows[i][1].ObjToString();
                if (String.IsNullOrWhiteSpace(casketCode))
                    continue;
                casketDesc = dt.Rows[i][3].ObjToString();
                if (String.IsNullOrWhiteSpace(casketDesc))
                    continue;
                price = dt.Rows[i][4].ObjToString();
                if (String.IsNullOrWhiteSpace(price))
                    continue;
                price = price.Replace("$", "");
                price = price.Replace(",", "");
                if (!G1.validate_numeric(price))
                    continue;
                casketDesc = G1.protect_data(casketDesc);
                cmd = "Select * from `casket_master` where `casketdesc` = '" + casketDesc + "';";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    goodCount++;
                    cost = dx.Rows[0]["casketcost"].ObjToDouble();
                    masterRecord = dx.Rows[0]["record"].ObjToString();
                    str = dt.Rows[i][7].ObjToString();
                    str = str.Replace("%", "");
                    markup = str.ObjToDouble() / 100D;
                    record = G1.create_record("casket_packages", "groupname", "-1");
                    if (G1.BadRecord("casket_packages", record))
                        continue;
                    G1.update_db_table("casket_packages", "record", record, new string[] { "!masterRecord", masterRecord, "markup", markup.ToString(), "GPL_Group", gplGroup, "groupname", groupName });
                }
                else
                    badCount++;
            }
        }
        /***********************************************************************************************/
        private void btnSave_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if ( dgv2.Visible )
                dt = (DataTable)dgv2.DataSource;
            else if (dgv3.Visible)
                dt = (DataTable)dgv3.DataSource;
            else if (dgv4.Visible)
                dt = (DataTable)dgv4.DataSource;
            loadededLocation = cmbGroups.Text.Trim().ToUpper();
            loadedPackage = cmbPackage.Text.Trim().ToUpper();
            if (String.IsNullOrWhiteSpace(loadedPackage))
                loadedPackage = "MASTER";
            if (loadededLocation == "MASTER" && loadedPackage == "MASTER")
            {
                SaveCasketMaster(dt);
                btnSave.Hide();
                modified = false;
                if (dgv.Visible)
                    casketsModified = false;
                else if (dgv2.Visible)
                    vaultsModified = false;
                else if (dgv3.Visible)
                    urnsModified = false;
                else if (dgv4.Visible)
                    miscModified = false;
                return;
            }
            if (loadededLocation == "MASTER" && loadedPackage == "MASTER")
                SaveCasketMaster(dt);
            else
                SaveServices(dt);
            btnSave.Hide();
            modified = false;
        }
        /***********************************************************************************************/
        private double GetGPGBasicPackageXX ()
        {
            double basicPackage = 0D;
            string gplGroup = cmbGPLGroup.Text.Trim();
            if (String.IsNullOrWhiteSpace(gplGroup))
                return basicPackage;
            string basics = "BASIC SERVICES OF FUNERAL DIRECTOR AND STAFF";
            string cmd = "Select * from `funeral_gplgroups` where `groupname` = '" + gplGroup + "' and `service` = '" + basics + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
                basicPackage = dx.Rows[0]["price"].ObjToDouble();
            return basicPackage;
        }
        /***********************************************************************************************/
        private double GetGPGBasicPackage()
        {
            double basicPackage = 0D;
            string gplGroup = cmbGPLGroup.Text.Trim();
            if (String.IsNullOrWhiteSpace(gplGroup))
                return basicPackage;
            string basics = "BASIC SERVICES OF FUNERAL DIRECTOR AND STAFF";
            string cmd = "Select * from `funeral_gplgroups` where `groupname` = '" + gplGroup + "';";
            DataTable dx = G1.get_db_data(cmd);
            basicPackage = GetBasicServices(dx);
            return basicPackage;
        }
        /***********************************************************************************************/
        private double GetBasicServices(DataTable gDt)
        {
            double basicServicesPrice = 0D;
            double price = 0D;
            string service = "";
            DataRow[] dR = null;
            DataTable dt = G1.get_db_data("Select * from `funeral_master` where `basicService` = '1';");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                service = dt.Rows[i]["service"].ObjToString();
                if (String.IsNullOrWhiteSpace(service))
                    continue;
                dR = gDt.Select("service='" + service + "'");
                if (dR.Length > 0)
                {
                    price = dR[0]["price"].ObjToDouble();
                    basicServicesPrice += price;
                }

            }
            return basicServicesPrice;
        }
        /***********************************************************************************************/
        private void cmbGPLGroup_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            double basicPackage = GetGPGBasicPackage();

            LoadCasketLocations();

            DataTable dt = (DataTable)dgv.DataSource;
            if (dt == null)
                return;

            double casketPrice = 0D;
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                casketPrice = dt.Rows[i]["casket"].ObjToDouble();
                dt.Rows[i]["package"] = casketPrice + basicPackage;
            }
            dgv.DataSource = dt;
            dgv.Refresh();
        }
        /***********************************************************************************************/
        private void picMoveFuture_Click(object sender, EventArgs e)
        {
            if (loading)
                return;

            if (dgv.Visible)
            {
                MoveFuture(gridMain, dgv);
                casketsModified = true;
                btnSave.Show();
            }
            else if (dgv2.Visible)
            {
                MoveFuture(gridMain2, dgv2);
                vaultsModified = true;
                btnSave.Show();
            }
            else if (dgv3.Visible)
            {
                MoveFuture(gridMain3, dgv3);
                urnsModified = true;
                btnSave.Show();
            }
            else if (dgv4.Visible)
            {
                MoveFuture(gridMain4, dgv4);
                miscModified = true;
                btnSave.Show();
            }

            //DataTable dt = (DataTable)dgv.DataSource;
            //if (dt == null)
            //    return;

            //double basicPackage = GetGPGBasicPackage();

            //double futurePrice = 0D;
            //double currentPrice = 0D;
            //for (int i = 0; i < dt.Rows.Count; i++)
            //{
            //    currentPrice = dt.Rows[i]["casketCost"].ObjToDouble();
            //    dt.Rows[i]["pastCasketCost"] = currentPrice;
            //    futurePrice = dt.Rows[i]["futureCasketCost"].ObjToDouble();
            //    if (futurePrice <= 0D)
            //        futurePrice = currentPrice;
            //    dt.Rows[i]["casketCost"] = futurePrice;
            //    dt.Rows[i]["package"] = futurePrice + basicPackage;
            //    dt.Rows[i]["casket"] = futurePrice;
            //}

            //dgv.DataSource = dt;
            //dgv.Refresh();
            //modified = true;
            //btnSave.Show();
            //picMoveFuture.Hide();
            //picMovePast.Show();
        }
        /***********************************************************************************************/
        private void MoveFuture ( AdvBandedGridView gridMain, GridControl dgv )
        {
            if (loading)
                return;
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt == null)
                return;

            double basicPackage = GetGPGBasicPackage();

            double futurePrice = 0D;
            double currentPrice = 0D;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                currentPrice = dt.Rows[i]["casketCost"].ObjToDouble();
                dt.Rows[i]["pastCasketCost"] = currentPrice;
                futurePrice = dt.Rows[i]["futureCasketCost"].ObjToDouble();
                if (futurePrice <= 0D)
                    futurePrice = currentPrice;
                dt.Rows[i]["casketCost"] = futurePrice;
                dt.Rows[i]["package"] = futurePrice + basicPackage;
                dt.Rows[i]["casket"] = futurePrice;
            }

            dgv.DataSource = dt;
            dgv.Refresh();
            dgv.Tag = "HIDEFUTURE";
            //modified = true;
            //btnSave.Show();
            picMoveFuture.Hide();
            picMovePast.Show();
        }
        /***********************************************************************************************/
        private void picMovePast_Click(object sender, EventArgs e)
        {
            if (loading)
                return;

            if (dgv == null)
                return;

            DataTable dt = null;
            GridControl mdgv = null;

            if (dgv.Visible)
            {
                casketsModified = true;
                mdgv = this.dgv;
                dt = (DataTable)dgv.DataSource;
            }
            else if (dgv2.Visible)
            {
                vaultsModified = true;
                mdgv = this.dgv2;
                dt = (DataTable)dgv2.DataSource;
            }
            else if (dgv3.Visible)
            {
                urnsModified = true;
                mdgv = this.dgv3;
                dt = (DataTable)dgv3.DataSource;
            }
            else if (dgv4.Visible)
            {
                miscModified = true;
                mdgv = this.dgv4;
                dt = (DataTable)dgv4.DataSource;
            }

            if (mdgv == null)
                return;

            if (dt == null)
                return;

            double basicPackage = GetGPGBasicPackage();

            double pastPrice = 0D;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                pastPrice = dt.Rows[i]["pastCasketCost"].ObjToDouble();
                dt.Rows[i]["casketCost"] = pastPrice;
                dt.Rows[i]["casket"] = pastPrice;
                dt.Rows[i]["package"] = pastPrice + basicPackage;
            }

            mdgv.DataSource = dt;
            mdgv.Refresh();

            modified = true;
            btnSave.Show();
            picMoveFuture.Show();
            picMovePast.Hide();
            mdgv.Tag = "HIDEPAST";
        }
        /***********************************************************************************************/
        private void LoadSelection()
        {
            btnSave.Hide();
            string group = GetGroup();
            if (workingLocation && !String.IsNullOrWhiteSpace(workCasket))
                group = workCasket;
            LoadCasketLocations();
        }
        /***********************************************************************************************/
        private void LoadCasketLocations()
        {
            string casketGroup = cmbGroups.Text.Trim();
            cmbLocations.Items.Clear();
            cmbLocations.Text = "";
            if (casketGroup.ToUpper() == "MASTER")
                return;

            lblLocations.Text = "Locations (0)";

            string cmd = "Select * from `casket_locations` where `casketGroup` = '" + casketGroup + "' GROUP BY `location`;";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
            {
                btnEdit.Hide();
                btnDeleteLocation.Hide();
                return;
            }
            bool saveLoading = loading;
            loading = true;
            string location = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                location = dt.Rows[i]["location"].ObjToString();
                cmbLocations.Items.Add(location);
            }
            lblLocations.Text = "Locations (" + dt.Rows.Count.ToString() + ")";

            loading = saveLoading;
        }
        /***********************************************************************************************/
        private void PullLocationCaskets(DataTable dx)
        {
            string casketGroup = cmbGroups.Text.Trim();
            if (String.IsNullOrWhiteSpace(casketGroup))
                return;
            if (casketGroup.Trim().ToUpper() == "MASTER")
                return;

            string location = cmbLocations.Text.Trim();
            if (String.IsNullOrWhiteSpace(location))
                return;

            string cmd = "Select * from `casket_locations` where `casketGroup` = '" + casketGroup + "' and `location` = '" + location + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return;
            if (dx != null)
            {
                DataRow[] dRows = null;
                string casketcode = "";
                string casketdesc = "";
                bool gotData = true;
                if (G1.get_column_number(dx, "type") < 0)
                    dx.Columns.Add("type");
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    casketcode = dt.Rows[i]["casketcode"].ObjToString();
                    casketdesc = dt.Rows[i]["casketdesc"].ObjToString();
                    if ( !String.IsNullOrWhiteSpace ( casketcode))
                        dRows = dx.Select("casketcode='" + casketcode + "'");
                    else
                        dRows = dx.Select("casketdesc='" + casketdesc + "'");
                    if (dRows.Length > 0)
                    {
                        dRows[0]["price"] = dt.Rows[i]["price"].ObjToString();
                        dRows[0]["futureCasketCost"] = dt.Rows[i]["futureCasketCost"].ObjToString();
                        dRows[0]["pastCasketCost"] = dt.Rows[i]["pastCasketCost"].ObjToString();
                        dRows[0]["casket"] = dt.Rows[i]["casket"].ObjToString();
                        dRows[0]["casketcost"] = dt.Rows[i]["casketcost"].ObjToString();
                        dRows[0]["markup"] = dt.Rows[i]["markup"].ObjToString();
                        if (gotData)
                            dRows[0]["type"] = "CUSTOM";
                    }
                }
            }
        }
        /***********************************************************************************************/
        private void btnAddLocation_Click(object sender, EventArgs e)
        {
            string gplGroup = cmbGroups.Text.Trim();
            if (gplGroup.ToUpper() == "MASTER")
            {
                MessageBox.Show("***ERROR*** You cannot customize the Master Casket Group!\nChoose another Casket Group!");
                return;
            }
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
            //using (ListSelect listForm = new ListSelect(lines, true))
            //{
            //    listForm.Text = "Select Location for Casket Group " + cmbGroups.Text;
            //    listForm.ListDone += ListForm_LocationDone;
            //    listForm.Show();
            //}
            ListSelect listForm = new ListSelect(lines, true);
            listForm.Text = "Select Location for Casket Group " + cmbGroups.Text;
            listForm.ListDone += ListForm_LocationDone;
            listForm.Show();
        }
        /***********************************************************************************************/
        private void ListForm_LocationDone(string s)
        {
            if (String.IsNullOrWhiteSpace(s))
                return;
            string gplGroup = cmbGroups.Text.Trim();
            if (gplGroup.ToUpper() == "MASTER")
                return;
            string location = s;
            string[] Lines = s.Split('\n');
            if (Lines.Length <= 0)
                return;
            location = Lines[0];

            DataTable dt = (DataTable)dgv.DataSource;

            CasketsLocations servicesLocationForm = new CasketsLocations(gplGroup, location, false, dt);
            servicesLocationForm.ShowDialog();
            ServicesLocationForm_SelectDone(location);
        }
        /***********************************************************************************************/
        private void ServicesLocationForm_SelectDone(string location)
        {
            LoadSelection();
            cmbLocations.Text = location;
        }
        /***********************************************************************************************/
        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (loading)
                return;
            string gplGroup = cmbGroups.Text.Trim();
            if (String.IsNullOrWhiteSpace(gplGroup))
                return;
            string location = cmbLocations.Text.Trim();
            if (String.IsNullOrWhiteSpace(location))
                return;
            if (gplGroup.ToUpper() == "MASTER")
                return;
            DataTable dt = (DataTable)dgv.DataSource;
            CasketsLocations servicesForm = new CasketsLocations(gplGroup, location, false, dt);
            servicesForm.ShowDialog();
            ServicesLocationForm_SelectDone(location);
        }
        /***********************************************************************************************/
        private void cmbLocations_SelectedIndexChanged(object sender, EventArgs e)
        {
            DataTable dx = (DataTable)dgv.DataSource;
            PullLocationCaskets(dx);
            string location = cmbLocations.Text.Trim();
            btnEdit.Hide();
            btnDeleteLocation.Hide();
            if ( !String.IsNullOrWhiteSpace ( location))
            {
                btnEdit.Show();
                btnDeleteLocation.Show();
            }
        }
        /***********************************************************************************************/
        private void btnDeleteLocation_Click(object sender, EventArgs e)
        {
            if (loading)
                return;
            string gplGroup = cmbGroups.Text.Trim();
            if (String.IsNullOrWhiteSpace(gplGroup))
                return;
            string location = cmbLocations.Text.Trim();
            if (String.IsNullOrWhiteSpace(location))
                return;
            if (gplGroup.ToUpper() == "MASTER")
                return;

            string cmd = "Select * from `casket_locations` where `casketGroup` = '" + gplGroup + "' AND `location` = '" + location + "';";
            DataTable dt = G1.get_db_data(cmd);
            if ( dt.Rows.Count > 0 )
            {
                string record = "";
                for ( int i=0; i<dt.Rows.Count; i++)
                {
                    record = dt.Rows[i]["record"].ObjToString();
                    G1.delete_db_table("casket_locations", "record", record);
                }
            }
            LoadCasketLocations();
            LoadData();
        }
        /***********************************************************************************************/
        private void importBatevilleCasketPriceUpdateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Import importForm = new Import();
            importForm.Text = "Select Batesville Import File to obtain future costs";
            importForm.SelectDone += ImportForm_ImportCasketMaster;
            importForm.Show();
        }
        /***********************************************************************************************/
        private void ImportForm_ImportCasketMaster(DataTable dt)
        {
            if (dt == null)
                return;
            if (dt.Rows.Count <= 0)
                return;

            DataTable ddt = (DataTable)dgv.DataSource;

            DataTable badDt = ddt.Clone();

            string casketCode = "";
            string casketDesc = "";
            string price = "";
            string oldPrice = "";
            string casketPrice = "";
            string record = "";
            string chr = "";
            string cmd = "";
            string str = "";
            bool vault = false;
            bool urn = false;
            DataTable dx = null;
            DataTable casketDt = null;
            bool gotit = false;

            DataRow[] dR = null;

            DialogResult result = MessageBox.Show("***Question***\nWould you like to first set all Future Prices to Zero?", "Clear Casket Future Prices Dialog", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                for (int i = 0; i < ddt.Rows.Count; i++)
                {
                    casketCode = ddt.Rows[i]["casketcode"].ObjToString();
                    if (String.IsNullOrWhiteSpace(casketCode))
                        continue;
                    if (casketCode.ToUpper() == "MISC")
                        continue;
                    str = casketCode.Substring(0, 1).ToUpper();
                    if (str == "V" && casketCode.Length == 3)
                        continue;
                    if (casketCode.Contains("URN"))
                        continue;
                    if ( casketCode.Length >= 2 )
                    {
                        str = casketCode.Substring(0, 2).ToUpper();
                        if (str == "UV")
                            continue;
                    }
                    ddt.Rows[i]["futureCasketCost"] = 0;
                }
            }

            dt.Columns["Col 4"].ColumnName = "COL4";
            dt.Columns["Col 1"].ColumnName = "COL1";

            int found = 0;
            int notfound = 0;
            for (int i = 0; i < ddt.Rows.Count; i++)
            {
                casketCode = ddt.Rows[i]["casketcode"].ObjToString();
                if (String.IsNullOrWhiteSpace(casketCode))
                    continue;
                if (casketCode.ToUpper() == "MISC")
                    continue;
                str = casketCode.Substring(0, 1).ToUpper();
                if (str == "V" && casketCode.Length == 3)
                    continue;
                if (casketCode.Contains("URN"))
                    continue;
                if (casketCode.Length >= 2)
                {
                    str = casketCode.Substring(0, 2).ToUpper();
                    if (str == "UV")
                        continue;
                }
                if (casketCode.Length > 3)
                    casketCode = casketCode.Substring(0, 3);
                if ( casketCode == "CMY")
                {
                }
                dR = dt.Select("COL4 LIKE '" + casketCode + "%' ");
                if (dR.Length <= 0)
                {
                    dR = dt.Select("COL1 LIKE '" + casketCode + "%' ");
                    if (dR.Length <= 0)
                    {
                        notfound++;
                        badDt.ImportRow(ddt.Rows[i]);
                        continue;
                    }
                }
                found++;
                if (dR.Length == 1)
                {
                    price = dR[0]["COL 17"].ObjToString();
                    if (String.IsNullOrWhiteSpace(price))
                        continue;
                    price = price.Replace("$", "");
                    price = price.Replace(",", "");
                    if (!G1.validate_numeric(price))
                        continue;
                    ddt.Rows[i]["futureCasketCost"] = price.ObjToDecimal();
                }
                else
                {
                    casketDt = dR.CopyToDataTable();
                    oldPrice = "";
                    gotit = true;
                    for (int j = 0; j < casketDt.Rows.Count; j++)
                    {
                        price = casketDt.Rows[j]["Col 17"].ObjToString();
                        if (String.IsNullOrWhiteSpace(oldPrice))
                            oldPrice = price;
                        if (price != oldPrice)
                        {
                            gotit = false;
                            break;
                        }
                    }
                    if (!gotit)
                    {
                        using (SelectCasket listForm = new SelectCasket(casketDt))
                        {
                            casketDesc = ddt.Rows[i]["casketdesc"].ObjToString();
                            casketPrice = ddt.Rows[i]["casketcost"].ObjToString();
                            listForm.Text = "Select Casket for " + casketCode + " " + casketDesc + " Current Price " + casketPrice;
                            listForm.ShowDialog();
                            if (listForm.DialogResult == System.Windows.Forms.DialogResult.OK)
                            {
                                price = listForm.Answer;
                                if (String.IsNullOrWhiteSpace(price))
                                    continue;
                                price = price.Replace("$", "");
                                price = price.Replace(",", "");
                                if (!G1.validate_numeric(price))
                                    continue;
                                ddt.Rows[i]["futureCasketCost"] = price.ObjToDecimal();
                            }
                        }
                    }
                    else
                    {
                        price = dR[0]["COL 17"].ObjToString();
                        if (String.IsNullOrWhiteSpace(price))
                            continue;
                        price = price.Replace("$", "");
                        price = price.Replace(",", "");
                        if (!G1.validate_numeric(price))
                            continue;
                        ddt.Rows[i]["futureCasketCost"] = price.ObjToDecimal();
                    }
                }

                //record = G1.create_record("casket_master", "casketdesc", "-1");
                //if (G1.BadRecord("casket_master", record))
                //    continue;
                //G1.update_db_table("casket_master", "record", record, new string[] { "casketdesc", casketDesc, "casketcode", casketCode, "casketcost", price, "casketprice", casketPrice });
            }

            MessageBox.Show("Caskets Found = " + found.ToString() + " Caskets Not Found = " + notfound.ToString(), "Casket Price Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information);

            if (badDt.Rows.Count > 0)
            {
                DataTable listDt = dt.Clone();
                DataRow dRow = null;
                privateDt = null;
                for (int i = 0; i < badDt.Rows.Count; i++)
                {
                    dRow = listDt.NewRow();
                    dRow["COL4"] = badDt.Rows[i]["casketdesc"].ObjToString();
                    dRow["Col 17"] = badDt.Rows[i]["casketcost"].ObjToString();
                    listDt.Rows.Add(dRow);
                }
                using (SelectCasket listForm = new SelectCasket(listDt, dt ))
                {
                    listForm.Text = "FYI - Caskets Not Found!";
                    listForm.ListDone += ListForm_ListDone;
                    listForm.ShowDialog();
                }
            }

            if ( privateDt !=  null)
            {
                for ( int i=0; i<privateDt.Rows.Count; i++)
                {
                    casketDesc = privateDt.Rows[i]["casket"].ObjToString();
                    price = privateDt.Rows[i]["newcost"].ObjToString();
                    if ( !String.IsNullOrWhiteSpace ( price))
                    {
                        price = price.Replace("$", "");
                        price = price.Replace(",", "");
                        if (!G1.validate_numeric(price))
                            price = "0.00";
                        dR = ddt.Select("casketdesc='" + casketDesc + "'");
                        if ( dR.Length > 0 )
                        {
                            dR[0]["futureCasketCost"] = price.ObjToDecimal();
                        }
                    }
                }
            }
            dgv.DataSource = ddt;
            dgv.RefreshDataSource();
            dgv.Refresh();
            btnSave.Show();
            modified = true;
        }
        /***********************************************************************************************/
        private DataTable privateDt = null;
        private void ListForm_ListDone(DataTable dt)
        {
            privateDt = dt;
        }
        /***********************************************************************************************/
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            TabPage current = (sender as TabControl).SelectedTab;
            if (current.Name.Trim().ToUpper() == "TABCASKETS")
            {
                if (casketsModified)
                    btnSave.Show();
                else
                    btnSave.Hide();
                FixArrowPosition(gridMain);
                picMoveFuture.Show();
                picMovePast.Show();
                string tag = this.dgv.Tag.ObjToString().ToUpper();
                if (tag == "HIDEFUTURE")
                    this.picMoveFuture.Hide();
                else if (tag == "HIDEPAST")
                    this.picMovePast.Hide();
            }
            else if (current.Name.Trim().ToUpper() == "TABVAULTS")
            {
                if (vaultsModified)
                    btnSave.Show();
                else
                    btnSave.Hide();
                FixArrowPosition(gridMain2);
                picMoveFuture.Show();
                picMovePast.Show();
                string tag = dgv2.Tag.ObjToString().ToUpper();
                if (tag == "HIDEFUTURE")
                    this.picMoveFuture.Hide();
                else if (tag == "HIDEPAST")
                    this.picMovePast.Hide();
            }
            else if (current.Name.Trim().ToUpper() == "TABURNS")
            {
                if (urnsModified)
                    btnSave.Show();
                else
                    btnSave.Hide();
                FixArrowPosition(gridMain3);
                picMoveFuture.Show();
                picMovePast.Show();
                string tag = dgv3.Tag.ObjToString().ToUpper();
                if (tag == "HIDEFUTURE")
                    this.picMoveFuture.Hide();
                else if (tag == "HIDEPAST")
                    this.picMovePast.Hide();
            }
            else if (current.Name.Trim().ToUpper() == "TABMISC")
            {
                if (miscModified)
                    btnSave.Show();
                else
                    btnSave.Hide();
                FixArrowPosition(gridMain4);
                picMoveFuture.Show();
                picMovePast.Show();
                string tag = dgv4.Tag.ObjToString().ToUpper();
                if (tag == "HIDEFUTURE")
                    this.picMoveFuture.Hide();
                else if (tag == "HIDEPAST")
                    this.picMovePast.Hide();
            }
            btnSave.Hide();
            if (casketsModified || vaultsModified || urnsModified || miscModified)
                btnSave.Show();
        }
        /***********************************************************************************************/
        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            loadededLocation = cmbGroups.Text.Trim().ToUpper();
            if ( loadededLocation.ToUpper() == "MASTER")
            {
                DevExpress.XtraEditors.XtraMessageBox.Show("***ERROR*** You cannot pull in a backup Master!", "Pulling Backup Casket Group Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            LoadPackage( true );
            LoadCasketLocations();
            tabControl1.TabPages.Remove(tabVaults);
            tabControl1.TabPages.Remove(tabUrns);
            tabControl1.TabPages.Remove(tabMisc);
            return;
        }
        /***********************************************************************************************/
        private void repositoryItemCheckEdit2_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain2.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            int rowHandle = gridMain2.FocusedRowHandle;
            try
            {
                string select = dr["select"].ObjToString();
                string doit = "1";
                if (select == "1")
                    doit = "0";
                //string doit = "0";
                //if (select == "1")
                //    doit = "1";
                dr["select"] = doit;
            }
            catch (Exception ex)
            {
            }
            modified = true;
            if (!Selecting)
                btnSave.Show();
            //ReCalcTotal();
            gridMain2.RefreshData();
            gridMain2.RefreshRow(rowHandle);
            dgv2.Refresh();
        }
        /***********************************************************************************************/
        private void repositoryItemCheckEdit5_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain3.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            int rowHandle = gridMain3.FocusedRowHandle;
            try
            {
                string select = dr["select"].ObjToString();
                string doit = "1";
                if (select == "1")
                    doit = "0";
                //string doit = "0";
                //if (select == "1")
                //    doit = "1";
                dr["select"] = doit;
            }
            catch (Exception ex)
            {
            }
            modified = true;
            if (!Selecting)
                btnSave.Show();
            //ReCalcTotal();
            gridMain3.RefreshData();
            gridMain3.RefreshRow(rowHandle);
            dgv3.Refresh();
        }
        /***********************************************************************************************/
        private void repositoryItemCheckEdit7_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain4.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            int rowHandle = gridMain4.FocusedRowHandle;
            try
            {
                string select = dr["select"].ObjToString();
                string doit = "1";
                if (select == "1")
                    doit = "0";
                //string doit = "0";
                //if (select == "1")
                //    doit = "1";
                dr["select"] = doit;
            }
            catch (Exception ex)
            {
            }
            modified = true;
            if (!Selecting)
                btnSave.Show();
            //ReCalcTotal();
            gridMain4.RefreshData();
            gridMain4.RefreshRow(rowHandle);
            dgv4.Refresh();
        }

        /***********************************************************************************************/
        private void testToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;

            //XtraReport1 report = new XtraReport1( dt, dgv );
            //report.ShowPreview();
            CreateReport3( dt );
            //report.ShowPreview();
        }
        /***********************************************************************************************/
        private DevExpress.XtraReports.UI.PageHeaderBand PageHeader = null;
        private DevExpress.XtraReports.UI.DetailBand detailBand = null;
        private XtraReport report = null;
        public void CreateReport3( DataTable dt )
        {
            // Create an empty report.
            try
            {
                report = new XtraReport();
                report.Margins.Top = 50;
                report.Margins.Bottom = 50;
                report.BeforePrint += Report_BeforePrint;
                //report.ReportUnit = ReportUnit.Pixels;

                PageHeader = new DevExpress.XtraReports.UI.PageHeaderBand();
                PageHeader.HeightF = 0F;
                PageHeader.BeforePrint += PageHeader_BeforePrint;

                // Bind the report to a data source.
                BindToData(report, dt);


                // Create a master report.
                try
                {
                    //CreateReportHeader(report, "Products by Categories");
                }
                catch ( Exception ex)
                {
                }
                try
                {
                    CreateDetail(report);
                }
                catch ( Exception ex)
                {
                }

                try
                {
                    CreateDetailReport(report, "casketdesc", dt);
                    CreateDetailReport(report, "casketdesc", dt);
                }
                catch ( Exception ex)
                {
                }

                report.Bands.Add(PageHeader);

                // Publish the report.
                PublishReport(report);
            }
            catch ( Exception ex)
            {
            }
        }

        private void Report_BeforePrint(object sender, PrintEventArgs e)
        {
        }

        private void PageHeader_BeforePrint(object sender, PrintEventArgs e)
        {
        }

        private void BindToData(XtraReport report, DataTable dt )
        {
            //// Create a data source.
            //Access97ConnectionParameters connectionParameters = new Access97ConnectionParameters("../../nwind.mdb", "", "");
            //DevExpress.DataAccess.Sql.SqlDataSource ds = new DevExpress.DataAccess.Sql.SqlDataSource(connectionParameters);

            //// Create an SQL query to access the master table.
            //CustomSqlQuery queryCategories = new CustomSqlQuery();
            //queryCategories.Name = "queryCategories";
            //queryCategories.Sql = "SELECT * FROM Categories";

            //// Create an SQL query to access the detail table.
            //CustomSqlQuery queryProducts = new CustomSqlQuery();
            //queryProducts.Name = "queryProducts";
            //queryProducts.Sql = "SELECT * FROM Products";

            //// Add the queries to the data source collection.
            //ds.Queries.AddRange(new SqlQuery[] { queryCategories, queryProducts });

            //// Create a master-detail relation between the queries.
            //ds.Relations.Add("queryCategories", "queryProducts", "CategoryID", "CategoryID");

            //// Assign the data source to the report.
            ///
            //string dataMember = "casketdesc";
            //DataSet ds = new DataSet("casketdesc");
            //ds.Tables.Add(dt);

            //report.DataSource = ds;
            //report.DataMember = "casketdesc";
        }
        private void CreateReportHeader(XtraReport report, string caption)
        {
            // Create a report title.
            XRLabel label = new XRLabel();
            label.Font = new Font("Tahoma", 12, System.Drawing.FontStyle.Bold);
            label.Text = caption;
            label.WidthF = 300F;

            // Create a report header and add the title to it.
            ReportHeaderBand reportHeader = new ReportHeaderBand();
            report.Bands.Add(reportHeader);
            reportHeader.Controls.Add(label);
            reportHeader.HeightF = label.HeightF;

            PageHeader.Controls.Add(label);
        }

        private void CreateDetail(XtraReport report)
        {
            // Create a new label with the required settings. bound to the CategoryName data field.
            XRLabel labelDetail = new XRLabel();
            labelDetail.Font = new Font("Tahoma", 10, System.Drawing.FontStyle.Bold);
            labelDetail.WidthF = 300F;

            // Bind the label to the CategoryName data field.
            labelDetail.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "'Category: ' + [CategoryName]"));

            // Create a detail band and display the category name in it.
            detailBand = new DetailBand();
            detailBand.Height = labelDetail.Height;
            detailBand.Height = 0;
            detailBand.KeepTogetherWithDetailReports = true;
            report.Bands.Add(detailBand);
        }
        private XRTable tableDetail = null;
        private XRPageBreak xrPageBreak1 = null;
        private void CreateDetailReport(XtraReport report, string dataMember, DataTable dt)
        {
            // Create a detail report band and bind it to data.
            DetailReportBand detailReportBand = new DetailReportBand();
            detailReportBand.HeightF = 0F;
            detailReportBand.WidthF = 650F;
            report.Bands.Add(detailReportBand);
            detailReportBand.DataSource = report.DataSource;
            detailReportBand.DataMember = dataMember;

            GroupFooterBand footer = new GroupFooterBand();

            // Add a header to the detail report.
            ReportHeaderBand detailReportHeader = new ReportHeaderBand();
            detailReportHeader.HeightF = 0F;
            detailReportHeader.WidthF = 650F;
            detailReportBand.Bands.Add(detailReportHeader);

            XRTable titleHeader = new XRTable();
            titleHeader.BeginInit();
            titleHeader.Rows.Add(new XRTableRow());
            titleHeader.Borders = BorderSide.All;
            titleHeader.BorderColor = Color.Black;
            titleHeader.Font = new Font("Times New Roman", 18, System.Drawing.FontStyle.Bold);
            titleHeader.Padding = 0;
            titleHeader.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
            titleHeader.WidthF = 650F;
            XRTableCell titleCellHeader1 = new XRTableCell();
            titleCellHeader1.Text = "General Services";
            titleCellHeader1.WidthF = 650F;
            titleHeader.Rows[0].Cells.AddRange(new XRTableCell[] { titleCellHeader1 });
            titleHeader.EndInit();

            XRTable tableHeader = new XRTable();
            tableHeader.BeginInit();
            tableHeader.Rows.Add(new XRTableRow());
            tableHeader.Borders = BorderSide.All;
            tableHeader.BorderColor = Color.DarkGray;
            tableHeader.Font = new Font("Tahoma", 10, System.Drawing.FontStyle.Bold);
            tableHeader.Padding = 0;
            tableHeader.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft;
            tableHeader.HeightF = 0F;
            tableHeader.LocationF = new PointF(tableHeader.LeftF, tableHeader.TopF + titleHeader.HeightF);
            tableHeader.WidthF = 650F;

            XRTableCell cellHeader1 = new XRTableCell();
            cellHeader1.Text = "Casket Name";
            //cellHeader1.WidthF = 300F;
            cellHeader1.WidthF = 550F;
            XRTableCell cellHeader2 = new XRTableCell();
            cellHeader2.Text = "Unit Price";
            cellHeader2.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
            cellHeader2.WidthF = 100F;

            xrPageBreak1 = new XRPageBreak();
            xrPageBreak1.Visible = false;

            tableHeader.Rows[0].Cells.AddRange(new XRTableCell[] { cellHeader1, cellHeader2 });
            detailReportHeader.Height = tableHeader.Height + titleHeader.Height;
            detailReportHeader.HeightF = 0F;
            //detailReportHeader.Controls.Add(titleHeader);
            //detailReportHeader.Controls.Add(tableHeader);

            PageHeader.Controls.Add(titleHeader);
            PageHeader.Controls.Add(tableHeader);

            // Adjust the table width.
            tableHeader.BeforePrint += tableHeader_BeforePrint;
            tableHeader.EndInit();

            // Create a detail band.
            tableDetail = new XRTable();
            tableDetail.BeginInit();
            tableDetail.WidthF = 650F;

            //float[] cellWidth = { XlCell.WidthF, xrCell2.WidthF, xrCell3.WidthF, xrCell4.WidthF, xrCell5.WidthF };

            string casketdesc = "";
            string price = "";
            DataTable dx = dt.Clone();
            int lastRow = dt.Rows.Count;
            for ( int i=0; i<lastRow; i++)
            {
                dx.ImportRow(dt.Rows[i]);
            }
            for (int i = 0; i < lastRow; i++)
            {
                dx.ImportRow(dt.Rows[i]);
            }
            lastRow = dx.Rows.Count;
            lastRow = 10;
            for (int i = 0; i < lastRow; i++) 
            {
                XRTableRow xrRow = new XRTableRow();
                if ((i % 20) == 0 && i > 0 )
                {
                    XRTableCell cell = new XRTableCell();
                    cell.Text = "{Break}";
                    cell.WidthF = 550F;
                    xrRow.Cells.Add(cell);
                    tableDetail.Rows.Add(xrRow);
                    continue;
                }
                if ( i == 2)
                {
                    xrRow.WidthF = 650F;
                    XRTableCell rCell = new XRTableCell();
                    XRRichText richtext = new XRRichText();
                    RichTextBox rtf = new RichTextBox();
                    rtf.LoadFile(@"C:\Users\robby\Downloads\Payment Policy.rtf");
                    richtext.Text = rtf.Rtf;
                    //richtext.Text = "{\rtf1\deff0{\fonttbl{\f0 Calibri;}{\f1 Courier New;}}{\colortbl ;\red0\green0\blue255 ;}{\*\defchp \fs22}{\stylesheet {\ql\fs22 Normal;}{\*\cs1\fs22 Default Paragraph Font;}{\*\cs2\sbasedon1\fs22 Line Number;}{\*\cs3\ul\fs22\cf1 Hyperlink;}{\*\ts4\tsrowd\fs22\ql\tscellpaddfl3\tscellpaddl108\tscellpaddfb3\tscellpaddfr3\tscellpaddr108\tscellpaddft3\tsvertalt\cltxlrtb Normal Table;}{\*\ts5\tsrowd\sbasedon4\fs22\ql\trbrdrt\brdrs\brdrw10\trbrdrl\brdrs\brdrw10\trbrdrb\brdrs\brdrw10\trbrdrr\brdrs\brdrw10\trbrdrh\brdrs\brdrw10\trbrdrv\brdrs\brdrw10\tscellpaddfl3\tscellpaddl108\tscellpaddfr3\tscellpaddr108\tsvertalt\cltxlrtb Table Simple 1;}}{\*\listoverridetable}{\info{\creatim\yr2016\mo8\dy25\hr8\min34}{\version1}}\nouicompat\splytwnine\htmautsp\sectd\pard\plain\ql{\langnp1033\langfenp1033\noproof\f1\fs20\cf1 TEST}\fs22\par}"

                    richtext.Location = new Point(0, 0);
                    richtext.CanGrow = true;
                    richtext.Size = rCell.Size;
                    richtext.WidthF = 650F;
                    rCell.Controls.Add(richtext);
                    //xrRow.InsertCell(rCell, 2);
                    xrRow.Cells.Add(rCell);
                    tableDetail.Rows.Add(xrRow);
                    continue;
                }
                xrRow.WidthF = 650F;
                casketdesc = dx.Rows[i]["casketdesc"].ObjToString();
                price = dx.Rows[i]["casketcost"].ObjToString();
                //xrRow.HeightF = 10F;

                try
                {
                    XRTableCell cell = new XRTableCell();
                    cell.Text = casketdesc;
                    //cell.Padding = 1;
                    //Unit width = new Unit(cellWidth[0], UnitType.UnitTypePixelPixel);

                    //cell.Width = cellWidth[0];
                    cell.WidthF = 550F;
                    xrRow.Cells.Add(cell);

                    XRTableCell cell2 = new XRTableCell();
                    //cell.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "FormatString('{0:$0.00}', [UnitPrice])"));
                    cell2.Text = price;
                    //cell.Width = cellWidth[1];
                    cell2.WidthF = 100F;
                    cell2.TextAlignment = TextAlignment.MiddleRight;
                    //xrRow.Cells.AddRange(new XRTableCell[] { cell2 });
                    xrRow.Cells.Add(cell2);
                }
                catch ( Exception ex)
                {
                }

                tableDetail.Rows.Add(xrRow);
                int pageHeight = report.PageHeight;
                int myHeight = i * 23;
            }

            tableDetail.Borders = BorderSide.Left | BorderSide.Right | BorderSide.Bottom;
            tableDetail.BorderColor = Color.DarkGray;
            tableDetail.Font = new Font("Tahoma", 10);
            tableDetail.Padding = 0;
            tableDetail.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft;
            tableDetail.WidthF = 650F;

            detailBand = new DetailBand();
            detailBand.Height = tableDetail.Height;
            detailBand.WidthF = 500F;
            detailReportBand.Bands.Add(detailBand);
            tableDetail.WidthF = 650F;
            detailBand.Controls.Add(xrPageBreak1);
            detailBand.Controls.Add(tableDetail);

            // Adjust the table width.
            tableDetail.BeforePrint += tableDetail_BeforePrint;
            tableDetail.EndInit();

            // Create and assign different odd and even styles.
            XRControlStyle oddStyle = new XRControlStyle();
            XRControlStyle evenStyle = new XRControlStyle();

            oddStyle.BackColor = Color.WhiteSmoke;
            oddStyle.StyleUsing.UseBackColor = true;
            oddStyle.Name = "OddStyle";

            evenStyle.BackColor = Color.White;
            evenStyle.StyleUsing.UseBackColor = true;
            evenStyle.Name = "EvenStyle";

            report.StyleSheet.AddRange(new XRControlStyle[] { oddStyle, evenStyle });

            tableDetail.OddStyleName = "OddStyle";
            tableDetail.EvenStyleName = "EvenStyle";
        }

        private void AdjustTableWidth(XRTable table)
        {
            XtraReport report = table.RootReport;
            table.WidthF = report.PageWidth - report.Margins.Left - report.Margins.Right - 10;
        }

        void tableHeader_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            //AdjustTableWidth(sender as XRTable);
            try
            {
                string ItemName = report.GetCurrentColumnValue("casketdesc").ObjToString();
                if (!String.IsNullOrWhiteSpace(ItemName))
                {
                    if (ItemName == "{Break}")
                    {
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        void tableDetail_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            //AdjustTableWidth(sender as XRTable);
            XRTable table = (XRTable)sender;
            try
            {
                string detail = "";
                XRTableCell cell = null;
                foreach (XRTableRow row in table.Rows)
                {
                    cell = row.Cells[0];
                    detail = cell.Text.ObjToString();

                    if (!String.IsNullOrWhiteSpace(detail))
                    {
                        if (detail == "{Break}")
                        {
                            xrPageBreak1.Visible = true;
                        }
                    }

                }
            }
            catch ( Exception ex)
            {
            }
        }
        private void SetBreaks(object sender)
        {
            XRPageBreak control = sender as XRPageBreak;

            var ItemName = report.GetCurrentColumnValue("casketdesc").ToString();
            if (ItemName == "{Break}")
                control.Visible = true;
        }
        private void PublishReport(XtraReport report)
        {
            ReportPrintTool printTool = new ReportPrintTool(report);
            printTool.ShowPreviewDialog();
        }
        /***********************************************************************************************/
    }
}