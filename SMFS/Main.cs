using DevExpress.DataAccess.ConnectionParameters;
using DevExpress.DataAccess.Sql;
using DevExpress.Office.PInvoke;
using DevExpress.Xpo.Helpers;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports.Configuration;
using DevExpress.XtraReports.UI;
using DevExpress.XtraReports.UserDesigner;
using GeneralLib;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

using Microsoft.Win32;

using OfficeOpenXml.FormulaParsing.Excel.Functions.Information;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Xml;
using System.Threading;
using MySql.Data;
//using System.Windows;
//using iTextSharp.text.pdf;
//using iTextSharp.text.pdf.parser;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class SMFS : Form
    {
        internal static object pic_Loader;
        public static object Structures { get; internal set; }
        public static string activeSystem = "Main";
        private Color menuBackColor = Color.Gray;
        //private Timer mTimer;
        private int mDialogCount;
        public static Form SMFS_MainForm = null;
        public static DateTime lastTimer = DateTime.Now;
        public static int timerStartHour = 18;
        public static bool doNewFunFamily = true;
        public static double currentSalesTax = 0D;
        public static string capturedText = "";
        public static bool captureText = false;

        /****************************************************************************************/
        public SMFS()
        {
            //            ComponentInfo.SetLicense("FREE-LIMITED-KEY");
            InitializeComponent();
        }
        /****************************************************************************************/
        private void SMFS_Load(object sender, EventArgs e)
        {
            SMFS_MainForm = this;

            MessageFilter mobjMessageFilter = new MessageFilter();
            Application.AddMessageFilter(mobjMessageFilter);

            System.Diagnostics.Process.GetCurrentProcess().PriorityClass = System.Diagnostics.ProcessPriorityClass.RealTime;

            //HwndSource windowSpecificOSMessageListener = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            //windowSpecificOSMessageListener.AddHook(new HwndSourceHook(CallBackMethod));

            mainSystemToolStripMenuItem.Checked = true;
            menuBackColor = menuStrip1.BackColor;
            //DevExpress.UserSkins.BonusSkins.Register();
            labelMaximum.Hide();
            lblTotal.Hide();
            barImport.Hide();

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            System.Reflection.AssemblyName assemblyName = assembly.GetName();
            string location = assembly.Location;
            string lastPath = G1.LastPath(location);
            Version version = assemblyName.Version;

            DateTime lastMake = File.GetLastWriteTime(Assembly.GetExecutingAssembly().Location);

            string[] Lines = version.ObjToString().Split('.');
            try
            {
                this.Text += " " + Lines[0] + "." + Lines[1] + "." + Lines[3] + " " + lastPath;
            }
            catch
            {
            }

            //string record = "24082";
            //string date = "20080101 0000";
            //G1.update_db_table("customers", "record", record, new string[] { "birthDate", date });
            if (!LoginForm.administrator && !G1.isHR())
            {
                btnUsers.Dispose();
            }
            CheckMessages();
            CheckMajorOptions();
            CleanupMainMenu();

            SetupMenuPreferences();
            //SystemEvents.SessionSwitch += new SessionSwitchEventHandler(SystemEvents_SessionSwitch);

            //            Console.ReadLine();

            LoginForm.ReadTrust85ForcePayoffOptions();

            //G1.CreateAudit("Memory");

            //double myMen = G1.get_used_memory();
            //string str = G1.ReformatMoney(myMen);
            //str = str.Replace(".00", "");
            //G1.WriteAudit("Starting Memory=" + str);
        }
        //public static void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        //{
        //    switch (e.Reason)
        //    {
        //        // ...
        //        case SessionSwitchReason.SessionLock:
        //            // Do whatever you need to do for a lock
        //            // ...
        //            break;
        //        case SessionSwitchReason.SessionUnlock:
        //            // Do whatever you need to do for an unlock
        //            // ...
        //            break;
        //            // ...
        //    }
        //}
        /***********************************************************************************************/
        private void SetupMenuPreferences()
        {
            string preference = G1.getPreference(LoginForm.username, "Import Startup Data", "Allow Access");
            if (preference != "YES")
                importStartupDataToolStripMenuItem.Enabled = false;

            preference = G1.getPreference(LoginForm.username, "Import FDLIC Contract File", "Allow Access");
            if (preference != "YES")
                importContractFileToolStripMenuItem.Enabled = false;

            preference = G1.getPreference(LoginForm.username, "LockBox Imports", "Allow Access");
            if (preference != "YES")
                lockBoxImportsToolStripMenuItem.Enabled = false;

            preference = G1.getPreference(LoginForm.username, "Cremation Log", "Allow Access");
            if (preference != "YES")
                btnUrn.Visible = false;

            if (G1.isField())
            {
                btnFuneralHomes.Visible = false;
                btnInventory.Visible = false;
                //btnLocations.Visible = false;
                btnAdmin.Visible = false;
                btnUsers.Visible = false;
                btnContracts.Visible = false;
                btnPolicies.Visible = false;
                //btnAgents.Visible = false;

                miscToolStripMenuItem.Visible = false;
                editToolStripMenuItem.Visible = false;
                miscToolStripMenuItem1.Visible = false;
                activateToolStripMenuItem.Visible = false;
                reportsToolStripMenuItem.Visible = false;
                btnClock.Visible = true;

                int left = this.Left;
                int top = this.Top;
                int height = this.Height;
                int width = this.Width;
                width = width - 100;
                this.SetBounds(left, top, width, height);
            }
            else if (!G1.isAdmin() && !G1.isHR())
            {
                btnAdmin.Visible = false;
                btnUsers.Visible = false;
                btnClock.Visible = true;
            }
            //if (LoginForm.username.ToUpper() != "ROBBY" && LoginForm.username.ToUpper() != "CJENKINS")
            if (LoginForm.username.ToUpper() != "ROBBY")
            {
                btnClock.Visible = true;
                if (G1.isAdmin() || G1.isHR())
                {
                    int left = this.Left;
                    int top = this.Top;
                    int height = this.Height;
                    int width = this.Width;
                    width = width + btnClock.Width + btnUrn.Width;
                    this.SetBounds(left, top, width, height);
                    btnClock.Visible = true;

                    if (G1.RobbyServer)
                    {
                        left = this.Left;
                        top = this.Top;
                        if (G1.RobbyServer)
                            top += 60;
                        height = this.Height;
                        width = this.Width;
                        width = width + btnClock.Width + btnUrn.Width;
                        this.SetBounds(left, top, width, height);
                    }
                }
            }
            else
            {
                if (G1.isAdmin() || G1.RobbyServer)
                {
                    int left = this.Left;
                    int top = this.Top;
                    if (G1.RobbyServer)
                        top += 60;
                    int height = this.Height;
                    int width = this.Width;
                    width = width + btnClock.Width + btnUrn.Width;
                    this.SetBounds(left, top, width, height);
                }
            }

            SetupTimeSheetClock();

            SetupContacts();

            //if (G1.isAdmin())
            //    btnClock.Visible = true;

            //btnClock.Visible = false;
        }
        /****************************************************************************************/
        public static bool AmIaManager()
        {
            DataTable funDt = G1.get_db_data("Select * from `funeralhomes`;");
            DataTable userDt = G1.get_db_data("Select * from `users`;");
            string location = "";
            DataRow[] dRows = null;
            string manager = "";
            string[] Lines = null;
            bool isManager = false;
            dRows = userDt.Select("username='" + LoginForm.username + "'");
            if (dRows.Length <= 0)
                return false;
            string fName = dRows[0]["firstName"].ObjToString();
            string lName = dRows[0]["lastName"].ObjToString();

            string name = fName + " " + lName;

            dRows = funDt.Select("manager='" + name + "'");
            if (dRows.Length > 0)
                isManager = true;

            return isManager;
        }
        /****************************************************************************************/
        private void SetupContacts()
        {
            string user = LoginForm.username.ToUpper();
            if (user != "ROBBY" && user != "CJENKINS" && user != "TIM")
            {
                if (!G1.RobbyServer)
                    btnContacts.Visible = false;
            }
            btnContacts.Visible = false;
        }
        /****************************************************************************************/
        private bool isTimeKeeper = false;
        private bool isManager = false;
        private void SetupTimeSheetClock()
        {
            isTimeKeeper = false;
            isManager = AmIaManager();
            if (isManager)
            {
                btnClock.Visible = true;
                return;
            }

            string userName = LoginForm.username.ToUpper();
            DataTable userDt = G1.get_db_data("Select * from `users` WHERE `username` = '" + userName + "';");
            if (userDt.Rows.Count > 0)
            {
                if (!G1.isHR())
                {
                    if (userDt.Rows[0]["noTimeSheet"].ObjToString().ToUpper() == "Y")
                    {
                        btnClock.Visible = false;
                        return;
                    }
                }
            }

            //if ( 1 == 1)
            //{
            //    btnClock.Visible = true;
            //    return;
            //}
            btnClock.Visible = true;
            string cmd = "Select * from `tc_er` WHERE `username` = '" + userName + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
            {
                if (G1.isHR())
                {
                    isTimeKeeper = true;
                    btnClock.Visible = true;
                }
                return;
            }
            if (dx.Rows[0]["isManager"].ObjToString().ToUpper() == "Y")
            {
                isManager = true;
                btnClock.Visible = true;
            }
            if (dx.Rows[0]["isTimeKeeper"].ObjToString().ToUpper() == "Y")
            {
                isTimeKeeper = true;
                btnClock.Visible = true;
            }
            if (dx.Rows[0]["isSupervisor"].ObjToString().ToUpper() == "Y")
            {
                isTimeKeeper = true;
                btnClock.Visible = true;
            }
            if (G1.isHR())
            {
                isTimeKeeper = true;
                btnClock.Visible = true;
            }
            btnClock.Visible = true;
        }
        /****************************************************************************************/
        private void CleanupMainMenu()
        {
            if (G1.RobbyServer)
                return;

            for (int i = menuStrip1.Items.Count - 1; i >= 0; i--)
            {
                ToolStripItem menu = menuStrip1.Items[i];
                if (menu.Text.ToUpper() == "QUICKSTART")
                {
                    menuStrip1.Items.RemoveAt(i);
                }
            }
            for (int i = miscToolStripMenuItem1.DropDownItems.Count - 1; i >= 0; i--)
            {
                ToolStripItem menu = miscToolStripMenuItem1.DropDownItems[i];
                if (menu.Text.ToUpper() == "DISPLAY AUDIT")
                    continue;
                if (menu.Text.ToUpper() == "MESSAGES")
                    continue;
                if (menu.Text == "Load Bank ACH's")
                    continue;
                if (LoginForm.username.ToUpper() == "ROBBY" || LoginForm.username.ToUpper() == "CJENKINS" || LoginForm.username.ToUpper() == "TIM")
                {
                    if (menu.Text.ToUpper() == "TEST")
                        continue;
                }
                miscToolStripMenuItem1.DropDownItems.RemoveAt(i);
            }
        }
        /****************************************************************************************/
        private void CheckMajorOptions()
        {
            DailyHistory.majorSwitch = true;
            DataTable ddd = G1.get_db_data("Select * from `options`;");
            if (ddd.Rows.Count <= 0)
                return;
            DataRow[] dR = ddd.Select("option='Use New Interest/Balance Calculations'");
            if (dR.Length > 0)
            {
                string str = dR[0]["answer"].ObjToString().Trim().ToUpper();
                if (str.Length > 1)
                    str = str.Substring(0, 1);
                if (dR[0]["answer"].ObjToString().Trim().ToUpper() == "NO")
                    DailyHistory.majorSwitch = false;
            }

            dR = ddd.Select("option='Sales Tax'");
            if (dR.Length > 0)
                currentSalesTax = dR[0]["answer"].ObjToDouble();
        }
        /****************************************************************************************/
        private void importSMFSDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Import importForm = new Import();
            importForm.Show();
        }
        /****************************************************************************************/
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        /****************************************************************************************/
        private void btnUsers_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            if (G1.isAdmin() || G1.isHR())
            {
                AddEditUsers userForm = new AddEditUsers();
                userForm.Show();
            }
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void btnContracts_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            using (NewContract contractForm = new NewContract())
            {
                contractForm.SelectDone += ContractForm_SelectDone;
                contractForm.ShowDialog();
            }
            //Contracts fform = new Contracts();
            //fform.Show();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void ContractForm_SelectDone(string contract)
        {
            if (String.IsNullOrWhiteSpace(contract))
                return;
            this.Cursor = Cursors.WaitCursor;
            CustomerDetails customerForm = new CustomerDetails(contract);
            customerForm.Show();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void btnCustomer_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            Customers custForm = new Customers();
            custForm.Show();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void btnFuneralHomes_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            Form form = G1.IsFormOpen("FuneralHomes");
            if (form != null)
            {
                form.Show();
                form.WindowState = FormWindowState.Normal;
                form.Visible = true;
                form.Refresh();
                form.BringToFront();
            }
            else
            {
                FuneralHomes funeralForm = new FuneralHomes();
                funeralForm.Show();
            }
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void importDailyHistoryFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImportPaymentHistory payForm = new ImportPaymentHistory();
            payForm.Show();
        }
        /****************************************************************************************/
        private void inventorListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImportInventoryList inventForm = new ImportInventoryList("Import Main Inventory");
            inventForm.Show();
        }
        /****************************************************************************************/
        private void btnInventory_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            InventoryList inventForm = new InventoryList();
            inventForm.Show();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void btnAdmin_Click(object sender, EventArgs e)
        { // Interface to Admin Options
            if (!LoginForm.administrator)
            {
                MessageBox.Show("***ERROR*** You do not have permission for this!");
                return;
            }
            AdminOptions adminForm = new AdminOptions();
            adminForm.Show();
        }
        /****************************************************************************************/
        private void btnLocations_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            string preference = G1.getPreference(LoginForm.username, "Use New Inventory List", "Allow Access", false);
            if (preference != "YES")
            {
                InventoryLocations locationForm = new InventoryLocations();
                locationForm.Show();
            }
            else
            {
                InventoryLocationsNew locationFormNew = new InventoryLocationsNew();
                locationFormNew.Show();
            }
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void displayAuditToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Audit auditForm = new Audit();
            auditForm.Show();
        }
        /****************************************************************************************/
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            //ImportCasketUsage caskitForm = new ImportCasketUsage( "delimiter");
            //caskitForm.Show();
        }
        /****************************************************************************************/
        private void testImportLockBocToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImportDailyDeposits importForm = new ImportDailyDeposits(null);
            importForm.Show();
        }
        /****************************************************************************************/
        private void btnAgents_Click(object sender, EventArgs e)
        {
            //string preference = G1.getPreference(LoginForm.username, "Agents", "Allow Access", true);
            //if (preference != "YES")
            //    return;

            this.Cursor = Cursors.WaitCursor;
            Agents agentForm = new Agents();
            agentForm.Show();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            EditHelp helpForm = new EditHelp();
            helpForm.Show();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void importCustomerData2012AndLaterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Import importForm = new Import("New");
            importForm.Show();
        }
        /****************************************************************************************/
        private void importPaymentsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Import importForm = new Import("Payments");
            importForm.Show();
        }
        /****************************************************************************************/
        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            Import importForm = new Import("Deceased");
            importForm.Show();
        }
        /****************************************************************************************/
        private void importPreaccToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Import importForm = new Import("Preacc");
            importForm.Show();
        }
        /****************************************************************************************/
        private void iMportPremstToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Import importForm = new Import("Premst");
            importForm.Show();
        }
        /****************************************************************************************/
        private void importPreaccmstToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Import importForm = new Import("Preacclap");
            importForm.Show();
        }
        /****************************************************************************************/
        private void importPremstlapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Import importForm = new Import("Premstlap");
            importForm.Show();
        }
        /****************************************************************************************/
        private void importContractFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Import importForm = new Import("NewContracts");
            importForm.Show();
        }
        /****************************************************************************************/
        private void importAgentImcomingFDLICCodesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Import importForm = new Import("AgentIncoming");
            importForm.Show();
        }
        /****************************************************************************************/
        private void displayAuditToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Audit auditForm = new Audit();
            auditForm.Show();
        }
        /****************************************************************************************/
        private void messagesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            Messages messageForm = new Messages();
            messageForm.FormClosed += MessageForm_FormClosed;
            messageForm.Show();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void MessageForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            CheckMessages();
        }
        /***********************************************************************************************/
        private void CheckMessages()
        {
            int cnt = Messages.GetMessageCount(LoginForm.workUserRecord);
            if (cnt > 0)
            {
                btnMsg.Image = GetIcon("mail32");
                //if (btnMsg.Text != cnt.ToString())
                //    showMessagesTrayIcon();
                btnMsg.Text = cnt.ToString();
            }
            else
            {
                btnMsg.Image = GetIcon("mailnew32");
                btnMsg.Text = "";
                //                hideMessagesTrayIcon();
            }
        }
        /*******************************************************************************************/
        private Image GetIcon(string icon_name)
        {
            Image iconImage = null;
            try
            {
                if (icon_name.ToUpper() == "MAIL32")
                    iconImage = global::SMFS.Properties.Resources.mailclosed;
                else if (icon_name.ToUpper() == "MAILNEW32")
                    iconImage = global::SMFS.Properties.Resources.mailclosed;
            }
            catch
            { // Just forget it! Just don't die!
            }
            return iconImage;
        }
        /***********************************************************************************************/
        private void btnMsg_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            Messages messageForm = new Messages();
            messageForm.FormClosed += MessageForm_FormClosed;
            messageForm.Show();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        // Timeout Option
        private Thread t = null;
        public static PleaseWait pleaseWaitForm = null;
        private void timer1_Tick(object sender, EventArgs e)
        {
            CheckMessages();
            if (DateTime.Now.Hour < timerStartHour)
                return;
            TimeSpan ts = DateTime.Now - lastTimer;
            //            if (ts.TotalMinutes > 150D)
            //if (ts.TotalMinutes > 1500D)
            if (ts.TotalMinutes > 15D)
            {
                try
                {
                    System.Diagnostics.Process.GetCurrentProcess().Kill();
                    //if (pleaseWaitForm != null)
                    //    pleaseWaitForm.Close();
                    //pleaseWaitForm = null;
                }
                catch (Exception ex)
                {
                    this.Close();
                }
                finally
                {
                    this.Close();
                }
                //System.Diagnostics.Process.GetCurrentProcess().Kill();
                this.Close();
                return;
            }
            //if (ts.TotalMinutes > 120D)
            //{
            //    pleaseWaitForm = new PleaseWait("SMFS Inactivity! Press any key or move the mouse to continue!", true);
            //    pleaseWaitForm.BringToFront();
            //    pleaseWaitForm.TopMost = true;
            //    pleaseWaitForm.Show();
            //}
        }
        /****************************************************************************************/
        private void testToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textEdit textForm = new textEdit();
            textForm.Show();
        }
        /****************************************************************************************/
        private void checkBoxTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void gemBoxTestToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //ArrangementForms gemForm = new ArrangementForms( "", "" );
            //gemForm.Show();
        }
        /****************************************************************************************/
        private void btnArrangements_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            Form form = G1.IsFormOpen("Funerals");
            if (form != null)
            {
                form.Show();
                form.WindowState = FormWindowState.Normal;
                form.Visible = true;
                form.Refresh();
                form.BringToFront();
            }
            else
            {
                Funerals funeralForm = new Funerals();
                funeralForm.Show();
            }
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void Contract_Click(object sender, EventArgs e)
        {
            Calendar2 calendarForm = new Calendar2("", "", DateTime.Now, null);
            calendarForm.Show();


            //Calendar calendarForm = new Calendar("", "", DateTime.Now);
            //calendarForm.Show();
            //ToolStripMenuItem menu = (ToolStripMenuItem)sender;
            //string contract = menu.Text;
            ////EditCustomer custForm = new EditCustomer(contract);
            ////custForm.Show();
            //EditCust custForm = new EditCust(contract);
            //custForm.Show();
        }
        /****************************************************************************************/
        private void testToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            string contract = "B18018LI";
            //            contract = "L13098UI";
            contract = "L17035UI";
            //CustomerDetails custForm = new CustomerDetails(contract);
            //custForm.Show();
            //Trusts trustForm = new Trusts();
            //trustForm.Show();
            //EditCustomer custForm = new EditCustomer( contract);
            //custForm.Show();
            EditCustomer custForm = new EditCustomer(contract);
            custForm.Show();
        }

        private void testReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                XtraReport2 report = new XtraReport2();
                //                report.ShowPreview();

                // Bind the report to a data source.
                //                BindToData(report);

                //// Create a master report.
                //                CreateReportHeader(report, "Products by Categories");
                //                CreateDetail(report);

                //// Create a detail report.
                //SqlDataSource ds = report.DataSource as SqlDataSource;
                //                DataTable dt = (DataTable)report.DataSource;
                //                CreateDetailReport(report, "Users");

                //// Publish the report.
                //                PublishReport(report);


                //ReportDesignTool designTool = new ReportDesignTool(new XtraReport2());

                //// Access the standard or ribbon-based Designer form.
                //// IDesignForm designForm = designTool.DesignForm;
                //IDesignForm designForm = designTool.DesignRibbonForm;

                //// Create a new blank report to initialize a Design Panel.
                //designForm.DesignMdiController.CreateNewReport();

                //// Handle the Design Panel's Loaded event.
                //designForm.DesignMdiController.DesignPanelLoaded += DesignMdiController_DesignPanelLoaded;

                //// Load a Report Designer in a dialog window.
                //// designTool.ShowDesignerDialog();
                //designTool.ShowRibbonDesignerDialog();
            }
            catch (Exception ex)
            {

            }
        }

        private void DesignMdiController_DesignPanelLoaded(object sender, DesignerLoadedEventArgs e)
        {
            //            throw new NotImplementedException();
        }
        /****************************************************************************************/
        private void BindToData(XtraReport report)
        {

            Access97ConnectionParameters connectionParameters = new Access97ConnectionParameters("../../nwind.mdb", "", "");
            DevExpress.DataAccess.Sql.SqlDataSource ds = new DevExpress.DataAccess.Sql.SqlDataSource(connectionParameters);

            // Create an SQL query to access the master table.
            CustomSqlQuery queryCategories = new CustomSqlQuery();
            queryCategories.Name = "queryCategories";
            queryCategories.Sql = "SELECT * FROM Categories";

            // Create an SQL query to access the detail table.
            CustomSqlQuery queryProducts = new CustomSqlQuery();
            queryProducts.Name = "queryProducts";
            queryProducts.Sql = "SELECT * FROM Products";

            // Add the queries to the data source collection.
            ds.Queries.AddRange(new SqlQuery[] { queryCategories, queryProducts });

            // Create a master-detail relation between the queries.
            ds.Relations.Add("queryCategories", "queryProducts", "CategoryID", "CategoryID");

            // Assign the data source to the report.
            //report.DataSource = ds;
            //report.DataMember = "queryCategories";

            string cmd = "Select * from `users`;";
            DataTable dt = G1.get_db_data(cmd);
            //            Assign the data source to the report.
            //report.DataSource = dt;
            // report.DataMember = "userName";
        }
        /****************************************************************************************/
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
        }
        /****************************************************************************************/
        private void CreateDetail(XtraReport report)
        {
            // Create a new label with the required settings. bound to the CategoryName data field.
            XRLabel labelDetail = new XRLabel();
            labelDetail.Font = new Font("Tahoma", 10, System.Drawing.FontStyle.Bold);
            labelDetail.WidthF = 300F;

            // Bind the label to the CategoryName data field depending on the report's data binding mode.
            if (Settings.Default.UserDesignerOptions.DataBindingMode == DataBindingMode.Bindings)
                labelDetail.DataBindings.Add("Text", report.DataSource, "queryCategories.CategoryName", "Category: {0}");
            else labelDetail.ExpressionBindings.Add(
                new ExpressionBinding("BeforePrint", "Text", "'Category: ' + [CategoryName]"));

            // Create a detail band and display the category name in it.
            DetailBand detailBand = new DetailBand();
            detailBand.Height = labelDetail.Height;
            detailBand.KeepTogetherWithDetailReports = true;
            report.Bands.Add(detailBand);
            labelDetail.TopF = detailBand.LocationFloat.Y + 20F;
            detailBand.Controls.Add(labelDetail);
        }
        /****************************************************************************************/
        private void CreateDetailReport(XtraReport report, string dataMember)
        {
            // Create a detail report band and bind it to data.
            DetailReportBand detailReportBand = new DetailReportBand();
            report.Bands.Add(detailReportBand);
            detailReportBand.DataSource = report.DataSource;
            detailReportBand.DataMember = dataMember;

            // Add a header to the detail report.
            ReportHeaderBand detailReportHeader = new ReportHeaderBand();
            detailReportBand.Bands.Add(detailReportHeader);

            XRTable tableHeader = new XRTable();
            tableHeader.BeginInit();
            tableHeader.Rows.Add(new XRTableRow());
            tableHeader.Borders = BorderSide.All;
            tableHeader.BorderColor = Color.DarkGray;
            tableHeader.Font = new Font("Tahoma", 10, System.Drawing.FontStyle.Bold);
            tableHeader.Padding = 10;
            tableHeader.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft;

            XRTableCell cellHeader1 = new XRTableCell();
            cellHeader1.Text = "Product Name";
            XRTableCell cellHeader2 = new XRTableCell();
            cellHeader2.Text = "Unit Price";
            cellHeader2.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleRight;

            tableHeader.Rows[0].Cells.AddRange(new XRTableCell[] { cellHeader1, cellHeader2 });
            detailReportHeader.Height = tableHeader.Height;
            detailReportHeader.Controls.Add(tableHeader);

            // Adjust the table width.
            tableHeader.BeforePrint += tableHeader_BeforePrint;
            tableHeader.EndInit();

            // Create a detail band.
            XRTable tableDetail = new XRTable();
            tableDetail.BeginInit();
            tableDetail.Rows.Add(new XRTableRow());
            tableDetail.Borders = BorderSide.Left | BorderSide.Right | BorderSide.Bottom;
            tableDetail.BorderColor = Color.DarkGray;
            tableDetail.Font = new Font("Tahoma", 10);
            tableDetail.Padding = 10;
            tableDetail.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft;

            XRTableCell cellDetail1 = new XRTableCell();
            XRTableCell cellDetail2 = new XRTableCell();
            cellDetail2.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleRight;
            if (Settings.Default.UserDesignerOptions.DataBindingMode == DataBindingMode.Bindings)
            {
                cellDetail1.DataBindings.Add("Text", report.DataSource, dataMember + ".ProductName");
                cellDetail2.DataBindings.Add("Text", report.DataSource, dataMember + ".UnitPrice", "{0:$0.00}");
            }
            else
            {
                cellDetail1.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "[ProductName]"));
                cellDetail2.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text",
                    "FormatString('{0:$0.00}', [UnitPrice])"));
            }

            tableDetail.Rows[0].Cells.AddRange(new XRTableCell[] { cellDetail1, cellDetail2 });

            DetailBand detailBand = new DetailBand();
            detailBand.Height = tableDetail.Height;
            detailReportBand.Bands.Add(detailBand);
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
        /****************************************************************************************/

        private void AdjustTableWidth(XRTable table)
        {
            XtraReport report = table.RootReport;
            table.WidthF = report.PageWidth - report.Margins.Left - report.Margins.Right;
        }
        /****************************************************************************************/

        void tableHeader_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            AdjustTableWidth(sender as XRTable);
        }
        /****************************************************************************************/

        void tableDetail_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            AdjustTableWidth(sender as XRTable);
        }
        /****************************************************************************************/
        private void PublishReport(XtraReport report)
        {
            ReportPrintTool printTool = new ReportPrintTool(report);
            printTool.ShowPreviewDialog();
        }
        /****************************************************************************************/
        private void importTrust2013DataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Trusts2013 trustForm = new Trusts2013();
            trustForm.Show();
        }
        /****************************************************************************************/
        private void btnPolicies_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            Policies policyForm = new Policies();
            policyForm.Text = "All Policies";
            policyForm.Show();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void importDailyDepositFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable dt = new DataTable();
            this.Cursor = Cursors.WaitCursor;
            ImportDailyDeposits dailyForm = new ImportDailyDeposits(dt);
            dailyForm.Show();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void importACHFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable dt = new DataTable();
            this.Cursor = Cursors.WaitCursor;
            ImportDailyDeposits dailyForm = new ImportDailyDeposits(dt, true);
            dailyForm.Show();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void importCreditCardPaymentsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable dt = new DataTable();
            this.Cursor = Cursors.WaitCursor;
            ImportDailyDeposits dailyForm = new ImportDailyDeposits(dt, "CC");
            dailyForm.Show();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void importInsurancePayerFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Import importForm = new Import("Insurance Payer");
            importForm.Show();
        }
        /****************************************************************************************/
        private void importInsurancePaymentsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Import importForm = new Import("Insurance Payments");
            importForm.Show();
        }
        /****************************************************************************************/
        private void toolStripMenuItem12_Click(object sender, EventArgs e)
        {
            Import importForm = new Import("Insurance Payer Lapsed Data");
            importForm.Show();
        }
        /****************************************************************************************/
        private void customersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            Customers custForm = new Customers();
            custForm.Show();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void customersToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            Customers custForm = new Customers("INSURANCE");
            custForm.Show();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void lapseReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PastDueInsurance pastForm = new PastDueInsurance("Insurance Lapse Report");
            pastForm.Show();
        }
        /****************************************************************************************/
        private void toolStripMenuItem13_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            PaymentsReport paymentForm = new PaymentsReport("Insurance Week Totals");
            paymentForm.Show();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void trust85ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            Trust85 trustForm = new Trust85(null);
            trustForm.Show();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void toolStripMenuItem14_Click(object sender, EventArgs e)
        {
            Import importForm = new Import("Insurance Payer Deceased Data");
            importForm.Show();
        }
        /****************************************************************************************/
        private void importInsuracePolicyDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Import importForm = new Import("Insurance Policies");
            importForm.Show();
        }
        /****************************************************************************************/
        private void toolStripMenuItem16_Click(object sender, EventArgs e)
        {
            Import importForm = new Import("Insurance Policies Lapsed");
            importForm.Show();
        }
        /****************************************************************************************/
        private void toolStripMenuItem15_Click(object sender, EventArgs e)
        {
            Import importForm = new Import("Insurance Policies Deceased");
            importForm.Show();
        }
        /****************************************************************************************/
        private void reconcileOrphanPoliciesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string contractRecord = "";
            string customerRecord = "";
            string contractNumber = "";
            string lastContractNumber = "";
            string payer = "";
            string lastName = "";
            string firstName = "";
            string lastPayer = "";
            string lastFirstName = "";
            string lastLastName = "";
            string record = "";
            int errors = 0;
            DataTable dx = null;
            string cmd = "Select * from `policies` where `contractNumber` LIKE 'OO%' order by `payer`,`lastName`,`firstName`;";
            DataTable dt = G1.get_db_data(cmd);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                record = dt.Rows[i]["record"].ObjToString();
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                payer = dt.Rows[i]["payer"].ObjToString();
                lastName = dt.Rows[i]["lastName"].ObjToString();
                firstName = dt.Rows[i]["firstName"].ObjToString();
                if (String.IsNullOrWhiteSpace(lastName))
                    continue;

                if (payer == lastPayer && lastName == lastLastName && firstName == lastFirstName)
                {
                    if (!String.IsNullOrWhiteSpace(lastContractNumber))
                        G1.update_db_table("policies", "record", record, new string[] { "contractNumber", lastContractNumber });
                    continue;
                }

                if (String.IsNullOrWhiteSpace(lastContractNumber) && String.IsNullOrWhiteSpace(lastPayer) && String.IsNullOrWhiteSpace(lastLastName))
                {
                    lastContractNumber = contractNumber;
                    lastPayer = payer;
                    lastLastName = lastName;
                    lastFirstName = firstName;
                }

                cmd = "Select * from `icustomers` where `payer` = '" + payer + "' AND `lastName` = '" + lastName + "' and `firstName` = '" + firstName + "';";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    contractNumber = dx.Rows[0]["contractNumber"].ObjToString();
                    if (payer == lastPayer && lastName == lastLastName && firstName == lastFirstName)
                    {
                        if (!String.IsNullOrWhiteSpace(lastContractNumber))
                            G1.update_db_table("policies", "record", record, new string[] { "contractNumber", contractNumber });
                    }
                    lastPayer = payer;
                    lastFirstName = firstName;
                    lastLastName = lastName;
                    lastContractNumber = contractNumber;
                    continue;
                }
                int count = 0;
                for (; ; )
                {
                    cmd = "select * from `icustomers` where `contractNumber` = '" + contractNumber + "';";
                    DataTable ddx = G1.get_db_data(cmd);
                    if (ddx.Rows.Count <= 0)
                    {
                        if (count > 0)
                            G1.update_db_table("policies", "record", record, new string[] { "contractNumber", contractNumber });
                        break;
                    }
                    contractNumber += "x";
                    count++;
                }
                customerRecord = G1.create_record("icustomers", "contractNumber", "-1");
                if (G1.BadRecord("icustomers", customerRecord))
                {
                    errors++;
                    continue;
                }
                G1.update_db_table("icustomers", "record", customerRecord, new string[] { "payer", payer, "firstName", firstName, "lastName", lastName, "contractNumber", contractNumber });

                contractRecord = G1.create_record("icontracts", "contractNumber", "-1");
                if (G1.BadRecord("icontracts", contractRecord))
                    continue;
                G1.update_db_table("icontracts", "record", contractRecord, new string[] { "contractNumber", contractNumber });

                lastPayer = payer;
                lastFirstName = firstName;
                lastLastName = lastName;
                lastContractNumber = contractNumber;
            }
        }
        /****************************************************************************************/
        private void insuranceOrphansToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            Customers custForm = new Customers("INSURANCE", true);
            custForm.Show();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void importPayerDataFIXToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Import importForm = new Import("Insurance Payer FIX2");
            importForm.Show();
        }
        /****************************************************************************************/
        private void runDailyReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            DailyReport dailyForm = new DailyReport();
            dailyForm.Show();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void importAllLapsesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Import importForm = new Import("All_Lapses");
            importForm.Show();
        }
        /****************************************************************************************/
        private void importAllReinstatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Import importForm = new Import("All_Reinstates");
            importForm.Show();
        }
        /****************************************************************************************/
        private void importFDLICPhoneNUmbersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Import importForm = new Import("FDLIC Phone Numbers");
            importForm.Show();
        }
        /****************************************************************************************/
        private void importPolicyUCodesToolStripMenuItem_Click(object sender, EventArgs e)
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
            string payerNumber = "";
            string policyNumber = "";
            string ucode = "";
            string cmd = "";
            DataTable dx = null;
            string record = "";
            string updateString = "";
            this.Cursor = Cursors.WaitCursor;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                ucode = dt.Rows[i]["U-code"].ObjToString();
                if (String.IsNullOrWhiteSpace(ucode))
                    continue;
                payerNumber = dt.Rows[i]["PAYER#"].ObjToString();
                if (String.IsNullOrWhiteSpace(payerNumber))
                    continue;
                policyNumber = dt.Rows[i]["policy#"].ObjToString();
                if (String.IsNullOrWhiteSpace(policyNumber))
                    continue;
                payerNumber = payerNumber.TrimStart('0');
                policyNumber = policyNumber.TrimStart('0');
                cmd = "Select * from `policies` where `payer` = '" + payerNumber + "' AND `policyNumber` = '" + policyNumber + "';";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    for (int j = 0; j < dx.Rows.Count; j++)
                    {
                        record = dx.Rows[j]["record"].ObjToString();
                        updateString = "ucode," + ucode;
                        G1.update_db_table("policies", "record", record, updateString);
                    }
                }
            }
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void importThirdPartyLogicToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Import importForm = new Import();
            importForm.SelectDone += ImportForm_SelectDone1;
            importForm.Show();
        }
        /****************************************************************************************/
        private void ImportForm_SelectDone1(DataTable dt)
        {
            string cmd = "DELETE FROM `logic` where `record` > '0';";
            G1.get_db_data(cmd);
            string record = "";
            string location = "";
            string and_or = "";
            string Operator = "";
            string c1 = "";
            string c2 = "";
            string c3 = "";
            string c4 = "";
            string o1 = "";
            string o2 = "";
            string o3 = "";
            string a1 = "";
            string a2 = "";
            string a3 = "";
            string u1 = "";
            string str = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                location = dt.Rows[i]["Report Name (Header)"].ObjToString();
                and_or = dt.Rows[i]["And / Or"].ObjToString();
                Operator = dt.Rows[i]["Operator"].ObjToString();
                c1 = dt.Rows[i]["C1"].ObjToString();
                c2 = dt.Rows[i]["C2"].ObjToString();
                c3 = dt.Rows[i]["C3"].ObjToString();
                c4 = dt.Rows[i]["C4"].ObjToString();
                o1 = dt.Rows[i]["O1"].ObjToString();
                o2 = dt.Rows[i]["O2"].ObjToString();
                o3 = dt.Rows[i]["O3"].ObjToString();
                a1 = dt.Rows[i]["A1"].ObjToString();
                a2 = dt.Rows[i]["A2"].ObjToString();
                a3 = dt.Rows[i]["A3"].ObjToString();
                u1 = dt.Rows[i]["U1"].ObjToString();
                str = "location," + location;
                str += ",and_or," + and_or;
                str += ",operator," + Operator;
                str += ",c1," + c1;
                str += ",c2," + c2;
                str += ",c3," + c3;
                str += ",c4," + c4;
                str += ",o1," + o1;
                str += ",o2," + o2;
                str += ",o3," + o3;
                str += ",a1," + a1;
                str += ",a2," + a2;
                str += ",a3," + a3;
                str += ",u1," + u1;
                record = G1.create_record("logic", "location", "-1");
                if (G1.BadRecord("logic", record))
                    break;
                G1.update_db_table("logic", "record", record, str);
            }
        }
        /****************************************************************************************/
        private void pDFParcerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //            string text = P1.ReadPdfFile("C:/Users/Robby/downloads/SMFS Robby Graham Agreement.pdf");
            //            ParsePdf("C:/Users/Robby/downloads/SMFS Robby Graham Agreement.pdf");
            //            ParsePdf("C:/Users/Robby/downloads/Receipt for Rasberry Green Payment 1.pdf");
            //            ParsePdf("C:/Users/Robby/downloads/Test Scan.pdf");

            using (System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog())
            {
                ofd.Filter = "PDF Files|*.pdf";
                ofd.Title = "Select a PDF File";
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string file = ofd.FileName;
                    //ViewPDF pdfForm = new ViewPDF("Third Party", "", file);
                    //pdfForm.Show();
                    //ParsePdf(file);
                    string data = GetText(file);
                }
            }
        }
        public static string GetText(string filePath)
        {
            var sb = new StringBuilder();
            try
            {
                using (PdfReader reader = new PdfReader(filePath))
                {
                    string prevPage = "";
                    for (int page = 1; page <= reader.NumberOfPages; page++)
                    {
                        ITextExtractionStrategy its = new SimpleTextExtractionStrategy();
                        var s = PdfTextExtractor.GetTextFromPage(reader, page, its);
                        if (prevPage != s) sb.Append(s);
                        prevPage = s;
                    }
                    reader.Close();
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            return sb.ToString();
        }
        /****************************************************************************************/
        private void ParsePdf(string filename)
        {
            string data = "";
            using (PdfReader reader = new PdfReader(filename))
            {
                StringBuilder text = new StringBuilder();

                for (int i = 1; i <= reader.NumberOfPages; i++)
                {
                    text.Append(PdfTextExtractor.GetTextFromPage(reader, i));
                    data += text.ToString();
                }
                reader.Close();
            }

            if (String.IsNullOrWhiteSpace(data))
            {
                using (Stream newpdfStream = new FileStream(filename, FileMode.Open, FileAccess.ReadWrite))
                {
                    PdfReader pdfReader = new PdfReader(newpdfStream);
                    //                string text = PdfTextExtractor.GetTextFromPage(pdfReader, 1, new iTextSharp.text.pdf.parser.SimpleTextExtractionStrategy());
                    string text = PdfTextExtractor.GetTextFromPage(pdfReader, 1, new iTextSharp.text.pdf.parser.SimpleTextExtractionStrategy());
                    if (!String.IsNullOrWhiteSpace(text))
                    {
                        EMRControlLib.EMRRichTextBox rtb = new EMRControlLib.EMRRichTextBox();
                        rtb.RichTextBox.AppendText(text);
                        ViewRTF aForm = new ViewRTF(rtb.RichTextBox.Rtf);
                        aForm.Show();
                    }
                }
            }
        }
        /****************************************************************************************/
        private void import3rdPartyReportsPerPolicyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Import importForm = new Import("Import 3rd Party Report List for Policies");
            importForm.SelectDone += ImportForm_SelectDone2;
            importForm.Show();
        }
        /****************************************************************************************/
        private void ImportForm_SelectDone2(DataTable dt)
        { // Import 3rd Party List of Reports per Policy

            barImport.Show();

            string policy = "";
            string firstName = "";
            string lastName = "";
            string reportName = "";
            string oldReport = "";
            string cmd = "";
            string record = "";
            string listArgs = "";

            int i = 0;

            Rectangle rect = this.Bounds;

            int height = rect.Height + 132 - 103;
            this.SetBounds(rect.Left, rect.Top, rect.Width, height);

            G1.CreateAudit("ThirdParty");

            int lastRow = dt.Rows.Count;

            DataTable dx = null;
            try
            {
                lblTotal.Show();

                lblTotal.Text = "of " + lastRow.ToString();
                lblTotal.Refresh();

                barImport.Minimum = 0;
                barImport.Maximum = lastRow;
                labelMaximum.Show();

                for (i = 0; i < dt.Rows.Count; i++)
                {
                    barImport.Value = i;
                    barImport.Refresh();
                    labelMaximum.Text = i.ToString();
                    labelMaximum.Refresh();

                    policy = dt.Rows[i]["policy#"].ObjToString();
                    if (String.IsNullOrWhiteSpace(policy))
                    {
                        G1.WriteAudit("Policy Empty Row=" + i.ToString() + "!");
                        continue;
                    }
                    firstName = dt.Rows[i]["Policy First Name"].ObjToString();
                    //if (String.IsNullOrWhiteSpace(firstName))
                    //{
                    //    G1.WriteAudit("First Name Empty Row=" + i.ToString() + " Policy=" + policy + "!");
                    //    continue;
                    //}
                    lastName = dt.Rows[i]["Policy Last Name"].ObjToString();
                    //if (String.IsNullOrWhiteSpace(lastName))
                    //{
                    //    G1.WriteAudit("Last Name Empty Row=" + i.ToString() + " Policy=" + policy + "!");
                    //    continue;
                    //}
                    reportName = dt.Rows[i]["Report Name"].ObjToString();
                    if (String.IsNullOrWhiteSpace(reportName))
                    {
                        G1.WriteAudit("Report Name Blank Row=" + i.ToString() + " Policy=" + policy + "!");
                        continue;
                    }

                    firstName = G1.protect_data(firstName);
                    lastName = G1.protect_data(lastName);
                    reportName = G1.protect_data(reportName);

                    cmd = "Select * from `policies` where `policyNumber` = '" + policy + "' and `policyLastName` = '" + lastName + "' and `policyFirstName` = '" + firstName + "';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count <= 0)
                    {
                        G1.WriteAudit("No Policy Found Row=" + i.ToString() + " Policy=" + policy + " LastName=" + lastName + " FirstName=" + firstName + "!");
                        continue;
                    }
                    record = dx.Rows[0]["record"].ObjToString();
                    oldReport = dx.Rows[0]["report"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(oldReport))
                    {
                        if (oldReport != reportName)
                        {
                            G1.WriteAudit("Old Report != New Report for Duplicate " + i.ToString() + " Policy=" + policy + " Old=" + oldReport + " New=" + reportName + "!");
                            continue;
                        }
                    }

                    listArgs = "report," + reportName;
                    G1.update_db_table("policies", "record", record, listArgs);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Row" + ex.Message.ToString());
            }
            barImport.Value = lastRow;
            barImport.Refresh();
            labelMaximum.Text = lastRow.ToString();
            labelMaximum.Refresh();

            MessageBox.Show("Import Finished . . .");

            this.SetBounds(rect.Left, rect.Top, rect.Width, rect.Height);
        }
        /****************************************************************************************/
        private void toolStripMenuItem18_Click(object sender, EventArgs e)
        {
            Image emptyImage = new Bitmap(1, 1);
            Image myImage = emptyImage;
            if (LoginForm.activeFuneralHomeSignature != null)
                myImage = G1.byteArrayToImage(LoginForm.activeFuneralHomeSignature);

            using (SignatureForm signatureForm = new SignatureForm("Enter Signature for User " + LoginForm.username, myImage))
            {
                if (signatureForm.ShowDialog() == DialogResult.OK)
                {
                    Image signature = signatureForm.SignatureResult;
                    if (signature != null)
                    {
                        ImageConverter converter = new ImageConverter();
                        LoginForm.activeFuneralHomeSignature = (byte[])converter.ConvertTo(signature, typeof(byte[]));
                        G1.update_blob("users", "record", LoginForm.workUserRecord, "signature", LoginForm.activeFuneralHomeSignature);
                    }
                    else
                    {
                        ImageConverter converter = new ImageConverter();
                        LoginForm.activeFuneralHomeSignature = (byte[])converter.ConvertTo(signature, typeof(byte[]));
                        G1.update_blob("users", "record", LoginForm.workUserRecord, "signature", LoginForm.activeFuneralHomeSignature);
                    }
                }
            }
        }
        /****************************************************************************************/
        private void runPayerDeceasedReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PayerDeceased payForm = new PayerDeceased();
            payForm.Show();
        }
        /****************************************************************************************/
        private void importPDFContractsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImportPDFs importForm = new ImportPDFs();
            importForm.Show();
        }
        /****************************************************************************************/
        private void importFDLICSpreadsheetServiceDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Import importForm = new Import();
            importForm.SelectDone += ImportForm_FDLIC;
            importForm.Show();
        }
        /***********************************************************************************************/
        private void AddMapColumn(DataTable dt, string oldName, string newName)
        {
            DataRow dR = dt.NewRow();
            dR["SpreadName"] = oldName;
            dR["FDLICName"] = newName;
            dt.Rows.Add(dR);
        }
        /***********************************************************************************************/
        private string GetUnMappedFields(DataTable dt, int row, string firstColumn)
        {
            int col = G1.get_column_number(dt, firstColumn);
            if (col < 0)
                return "ALL";
            string answer = "";
            string data = "";
            for (int i = col; i < dt.Columns.Count; i++)
            {
                data = dt.Rows[row][i].ObjToString();
                if (data != "XXX")
                    answer += dt.Columns[i].ColumnName.ObjToString() + "\n";
            }
            return answer;
        }
        /***********************************************************************************************/
        private string GetMappedData(DataTable dt, int row, DataTable mapDt, string field)
        {
            string answer = "";
            if (String.IsNullOrWhiteSpace(field))
                return answer;
            try
            {
                DataRow[] dR = mapDt.Select("FDLICName='" + field + "'");
                if (dR.Length > 0)
                {
                    string mappedField = dR[0]["SpreadName"].ObjToString();
                    if (G1.get_column_number(dt, mappedField) >= 0)
                        answer = dt.Rows[row][mappedField].ObjToString();
                    //dt.Rows[row][mappedField] = "XXX";
                }
            }
            catch (Exception ex)
            {
            }
            return answer;
        }
        /***********************************************************************************************/
        private DataTable MapFDLIC()
        {
            DataTable mapDt = new DataTable();
            mapDt.Columns.Add("SpreadName");
            mapDt.Columns.Add("FDLICName");

            AddMapColumn(mapDt, "ANUM", "AGENT_NUMBER");
            AddMapColumn(mapDt, "MERTOT", "MERTOT");
            AddMapColumn(mapDt, "SERTOT", "SERTOT");
            AddMapColumn(mapDt, "BASIC", "SERVICES_BASIC_SERVICES");
            AddMapColumn(mapDt, "ISSDT8", "SIGNED_DATE");
            AddMapColumn(mapDt, "ANUM", "AGENT_NUMBER");
            AddMapColumn(mapDt, "EMBALMING", "SERVICES_EMBALMING");
            AddMapColumn(mapDt, "OTHER PREP", "SERVICES_BODY_PREP");
            AddMapColumn(mapDt, "VISITATION", "SERVICES_VISITATION");
            AddMapColumn(mapDt, "VISITATION- DAY OF SERVICE", "STAFF AND EQUIPMENT FOR VISITATION SAME DAY AS FUNERAL SERVICE");
            AddMapColumn(mapDt, "FUNERAL SERVICE", "FACILITY, STAFF AND EQUIPMENT FOR FUNERAL SERVICE");
            AddMapColumn(mapDt, "MEMORIAL SERVICE", "FACILITY, STAFF AND EQUIPMENT FOR MEMORIAL SERVICE");
            AddMapColumn(mapDt, "GRAVESIDE SERVICE", "SERVICES_GRAVESIDE_SERVICE");
            AddMapColumn(mapDt, "OTHER COMMITTAL EQUIP", "SERVICES_COMMITTAL_EQUIPMENT");
            AddMapColumn(mapDt, "TRANSFER OF REMAINS", "SERVICES_TRANSFER_REMAINS");
            AddMapColumn(mapDt, "HEARSE", "SERVICES_HEARSE");
            AddMapColumn(mapDt, "LEAD CAR", "SERVICES_LEAD_CAR");
            AddMapColumn(mapDt, "FLOWER VAN", "EQUIPMENT/UTILITY VAN");
            AddMapColumn(mapDt, "MILEAGE", "SERVICES_MILEAGE");
            AddMapColumn(mapDt, "FORWARDING REMAINS", "SERVICES_FORWARDING_REMAINS");
            AddMapColumn(mapDt, "RECEIVING REMAINS", "SERVICES_RECEIVING_REMAINS");
            AddMapColumn(mapDt, "DIRECT CREMATION", "SERVICES_DIRECT_CREMATION");
            AddMapColumn(mapDt, "IMMEDIATE BURIAL", "SERVICES_IMMEDIATE_BURIAL");
            AddMapColumn(mapDt, "TOTAL SERVICE", "TOTAL SERVICE");

            AddMapColumn(mapDt, "DEATH CERTIFICATE", "CASH_ADVANCE_DEATH_CERTIFICATES");
            AddMapColumn(mapDt, "OTHER", "CASH_ADVANCE_MSC");
            AddMapColumn(mapDt, "MUSIC", "CASH_ADVANCE_MUSIC");
            AddMapColumn(mapDt, "BEAUTICIAN", "CASH_ADVANCE_BEAUTICIAN");

            AddMapColumn(mapDt, "ACKNOWLEDGEMENT CARDS", "MERCH_ACKNOWLEDGEMENT_CARDS");
            AddMapColumn(mapDt, "GRAVE MARKER", "MERCH_GRAVE_MARKER");
            AddMapColumn(mapDt, "REGISTER BOOK", "MERCH_REGISTER_BOOK");

            return mapDt;
        }
        /***********************************************************************************************/
        private void ImportForm_FDLIC(DataTable dt)
        {
            if (dt == null)
                return;
            if (dt.Rows.Count <= 0)
                return;
            DataTable mapDt = MapFDLIC();

            labelMaximum.Show();
            labelMaximum.Text = "0";
            barImport.Show();

            Rectangle rect = this.Bounds;

            int height = rect.Height + 132 - 103;
            this.SetBounds(rect.Left, rect.Top, rect.Width, height);

            string contractNumber = "";
            int lastrow = 0;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                contractNumber = dt.Rows[i]["CNUM"].ObjToString();
                if (String.IsNullOrWhiteSpace(contractNumber))
                    continue;
                lastrow++;
            }
            G1.CreateAudit("Import_Services");

            double serviceTotal = 0D;
            double merchandiseTotal = 0D;
            double serTot = 0D;
            double merTot = 0D;
            double cashAdvance = 0D;
            double aptot = 0D;
            double downPayment = 0D;
            double trust85P = 0D;
            double trust100P = 0D;
            double balanceDue = 0D;
            double paymentAmount = 0D;
            double numberPayments = 0D;
            double contractAmount = 0D;

            string record = "";
            string contractRecord = "";
            string customerRecord = "";
            string cmd = "";
            string trustNumber = "";
            string CONTRACT_AMOUNT = "";
            string agentNumber = "";
            string apr = "";
            string dueDate = "";
            string issueDate = "";
            string agentCode = "";
            string dob = "";
            DateTime dtDOB;
            DateTime dtIssue;
            int ageAtIssue = 0;
            string instr = "";
            double totalInt = 0D;

            DateTime dateDPPaid = DateTime.Now;
            bool addDateDPPaid = false;
            string str = "";

            string DEPOSIT_NUMBER = "DIGI";
            string USER_ID = "DWNPA";

            string FUNDED_AMOUNT = "";
            string TOTAL_TO_PAY = "";
            string DOWN_PAYMENT = "";
            string PREMIUM = "";


            string SERVICES_BASIC_SERVICES = ""; // (1) BASIC SERVICES OF FUNERAL DIRECTOR AND STAFF
            string SERVICES_FUNERAL = "";
            string SERVICES_MEMORIAL = "";
            string SERVICES_DIRECT_CREMATION = ""; // (36) DIRECT CREMATION WITH CONTAINER PROVIDED BY PURCHASER
            string SERVICES_VISITATION = ""; // (4)(5)(17)(18)
            string SERVICES_VISITATION_SAME_DAY = "";
            string SERVICES_EMBALMING = ""; // (16) EMBALMING, AUTOPSY AND DONOR
            string SERVICES_TRANSFER_REMAINS = ""; // (9) TRANSFER OF REMAINS TO THE FUNERAL HOME
            string SERVICES_RECEIVING_REMAINS = ""; // (29) RECEIVING REMAINS FROM ANOTHER FUNERAL HOME
            string SERVICES_COMMITTAL_EQUIPMENT = "";
            string SERVICES_AUTOMOTIVE_EQUIPMENT = ""; // (11)(12)
            string SERVICES_HEARSE = "";
            string SERVICES_LEAD_CAR = "";
            string SERVICES_TRANSPORTATION = ""; // (10) HEARSE
            string SERVICES_MILEAGE = "";
            string SERVICES_IMMEDIATE_BURIAL = "";
            string SERVICES_GRAVESIDE_SERVICE = ""; // (8) STAFF AND EQUIPMENT FOR GRAVESIDE SERVICE
            string SERVICES_FORWARDING_REMAINS = ""; // (28) FORWARDING REMAINS TO ANOTHER FUNERAL HOME
            string SERVICES_BODY_PREP = ""; // (3) OTHER PREPARATION OF THE BODY
            string SERVICES_FACILITIES = ""; // (6)(7)
            string SERVICES_DISCOUNTS = "";
            string SERVICES_GUARANTEED = "";

            string MERCH_CASKET_NAME = "";
            string MERCH_CASKET_PRICE = "";
            string MERCH_CASKET_DESCRIPTION = "";
            string MERCH_URN_NAME = "";
            string MERCH_URN_PRICE = "";
            string MERCH_URN_DESCRIPTION = "";
            string MERCH_OUTER_CONTAINER_NAME = "";
            string MERCH_OUTER_CONTAINER_PRICE = "";
            string MERCH_OUTER_CONTAINER_DESCRIPTION = "";
            string MERCH_ALT_CONTAINER_NAME = "";
            string MERCH_ALT_CONTAINER_PRICE = "";
            string MERCH_ALT_CONTAINER_DESCRIPTION = "";
            string MERCH_REGISTER_BOOK = ""; // (15) REGISTER BOOK AND POUCH
            string MERCH_GRAVE_MARKER = ""; // (14) TEMPORARY GRAVE MARKER
            string MERCH_ACKNOWLEDGEMENT_CARDS = ""; // (13) ACKNOLEDGEMENT CARDS
            string MERCH_OTHER = "";
            string MERCH_DISCOUNTS = "";
            string MERCH_GUARANTEED = "";

            string INSURED_PREFIX = "";
            string INSURED_FIRST_NAME = "";
            string INSURED_MIDDLE_INITIAL = "";
            string INSURED_LAST_NAME = "";
            string INSURED_SUFFIX = "";
            string INSURED_GENDER = "";
            string INSURED_ADDRESS1 = "";
            string INSURED_ADDRESS2 = "";
            string INSURED_CITY = "";
            string INSURED_STATE = "";
            string INSURED_ZIP = "";
            string INSURED_DOB = "";
            string INSURED_SSN = "";

            string CASH_ADVANCE_DEATH_CERTIFICATES = "";
            string CASH_ADVANCE_BEAUTICIAN = "";
            string CASH_ADVANCE_MUSIC = "";
            string CASH_ADVANCE_MSC = "";
            string CASH_ADVANCE_DISCOUNTS = "";
            string AREA_CODE = "";
            string PHONE_NUMBER = "";

            double merTotal = 0D;
            double serTotal = 0D;
            double oldSerTotal = 0D;
            DataTable contractDt = null;
            string[] Lines = null;
            string unmapped = "";

            DataTable custDt = null;

            int tableRow = 0;
            try
            {
                barImport.Minimum = 0;
                barImport.Maximum = lastrow;
                lblTotal.Text = "of " + lastrow.ToString();
                lblTotal.Show();
                lblTotal.Refresh();
                lblTotal.Show();
                int notFound = 0;
                int passed = 0;
                int updated = 0;

                lastrow = 1; // Just for Debugging

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    Application.DoEvents();
                    barImport.Value = i;
                    barImport.Refresh();
                    labelMaximum.Text = i.ToString();
                    labelMaximum.Refresh();

                    tableRow = i;
                    record = "";
                    try
                    {
                        trustNumber = dt.Rows[i]["CNUM"].ObjToString();
                        if (String.IsNullOrWhiteSpace(trustNumber))
                            continue;

                        custDt = G1.get_db_data("Select * from `cust_services` where `contractNumber` = '" + trustNumber + "';");
                        //if ( custDt.Rows.Count > 0 )
                        //{
                        //    passed++;
                        //    continue;
                        //}

                        addDateDPPaid = false;

                        agentNumber = GetMappedData(dt, i, mapDt, "AGENT_NUMBER");

                        issueDate = GetMappedData(dt, i, mapDt, "SIGNED_DATE");
                        if (issueDate.Length >= 8)
                            issueDate = issueDate.Substring(0, 8);

                        merTotal = GetMappedData(dt, i, mapDt, "MERTOT").ObjToDouble();
                        serTotal = GetMappedData(dt, i, mapDt, "SERTOT").ObjToDouble();

                        SERVICES_BASIC_SERVICES = GetMappedData(dt, i, mapDt, "SERVICES_BASIC_SERVICES");
                        SERVICES_FUNERAL = GetMappedData(dt, i, mapDt, "FACILITY, STAFF AND EQUIPMENT FOR FUNERAL SERVICE");
                        SERVICES_MEMORIAL = GetMappedData(dt, i, mapDt, "FACILITY, STAFF AND EQUIPMENT FOR MEMORIAL SERVICE");
                        SERVICES_DIRECT_CREMATION = GetMappedData(dt, i, mapDt, "SERVICES_DIRECT_CREMATION");
                        SERVICES_VISITATION = GetMappedData(dt, i, mapDt, "SERVICES_VISITATION");
                        SERVICES_VISITATION_SAME_DAY = GetMappedData(dt, i, mapDt, "STAFF AND EQUIPMENT FOR VISITATION SAME DAY AS FUNERAL SERVICE");
                        SERVICES_EMBALMING = GetMappedData(dt, i, mapDt, "SERVICES_EMBALMING");
                        SERVICES_TRANSFER_REMAINS = GetMappedData(dt, i, mapDt, "SERVICES_TRANSFER_REMAINS");
                        SERVICES_RECEIVING_REMAINS = GetMappedData(dt, i, mapDt, "SERVICES_RECEIVING_REMAINS");
                        SERVICES_COMMITTAL_EQUIPMENT = GetMappedData(dt, i, mapDt, "SERVICES_COMMITTAL_EQUIPMENT");
                        SERVICES_AUTOMOTIVE_EQUIPMENT = GetMappedData(dt, i, mapDt, "SERVICES_AUTOMOTIVE_EQUIPMENT");
                        SERVICES_HEARSE = GetMappedData(dt, i, mapDt, "SERVICES_HEARSE");
                        SERVICES_LEAD_CAR = GetMappedData(dt, i, mapDt, "SERVICES_LEAD_CAR");
                        SERVICES_TRANSPORTATION = GetMappedData(dt, i, mapDt, "EQUIPMENT/UTILITY VAN");
                        SERVICES_MILEAGE = GetMappedData(dt, i, mapDt, "SERVICES_MILEAGE");
                        SERVICES_IMMEDIATE_BURIAL = GetMappedData(dt, i, mapDt, "SERVICES_IMMEDIATE_BURIAL");
                        SERVICES_GRAVESIDE_SERVICE = GetMappedData(dt, i, mapDt, "SERVICES_GRAVESIDE_SERVICE");
                        SERVICES_FORWARDING_REMAINS = GetMappedData(dt, i, mapDt, "SERVICES_FORWARDING_REMAINS");
                        SERVICES_BODY_PREP = GetMappedData(dt, i, mapDt, "SERVICES_BODY_PREP");
                        SERVICES_FACILITIES = GetMappedData(dt, i, mapDt, "SERVICES_FACILITIES");
                        SERVICES_DISCOUNTS = GetMappedData(dt, i, mapDt, "SERVICES_DISCOUNTS");
                        SERVICES_GUARANTEED = GetMappedData(dt, i, mapDt, "SERVICES_GUARANTEED");

                        serviceTotal = SERVICES_BASIC_SERVICES.ObjToDouble() + SERVICES_DIRECT_CREMATION.ObjToDouble();
                        serviceTotal += SERVICES_FUNERAL.ObjToDouble();
                        serviceTotal += SERVICES_VISITATION.ObjToDouble() + SERVICES_EMBALMING.ObjToDouble();

                        serviceTotal += SERVICES_VISITATION_SAME_DAY.ObjToDouble() + SERVICES_MEMORIAL.ObjToDouble();
                        serviceTotal += SERVICES_MILEAGE.ObjToDouble() + SERVICES_LEAD_CAR.ObjToDouble();
                        serviceTotal += SERVICES_HEARSE.ObjToDouble();

                        serviceTotal += SERVICES_TRANSFER_REMAINS.ObjToDouble() + SERVICES_RECEIVING_REMAINS.ObjToDouble();
                        serviceTotal += SERVICES_COMMITTAL_EQUIPMENT.ObjToDouble() + SERVICES_AUTOMOTIVE_EQUIPMENT.ObjToDouble();
                        serviceTotal += SERVICES_TRANSPORTATION.ObjToDouble() + SERVICES_IMMEDIATE_BURIAL.ObjToDouble();
                        serviceTotal += SERVICES_GRAVESIDE_SERVICE.ObjToDouble() + SERVICES_FORWARDING_REMAINS.ObjToDouble();
                        serviceTotal += SERVICES_BODY_PREP.ObjToDouble() + SERVICES_FACILITIES.ObjToDouble();

                        MERCH_REGISTER_BOOK = GetMappedData(dt, i, mapDt, "MERCH_REGISTER_BOOK");
                        MERCH_GRAVE_MARKER = GetMappedData(dt, i, mapDt, "MERCH_GRAVE_MARKER");
                        MERCH_ACKNOWLEDGEMENT_CARDS = GetMappedData(dt, i, mapDt, "MERCH_ACKNOWLEDGEMENT_CARDS");

                        serviceTotal += MERCH_REGISTER_BOOK.ObjToDouble() + MERCH_GRAVE_MARKER.ObjToDouble();
                        serviceTotal += MERCH_ACKNOWLEDGEMENT_CARDS.ObjToDouble();
                        serviceTotal = G1.RoundValue(serviceTotal);

                        if (serviceTotal != serTotal)
                            G1.WriteAudit("***WARNING Service Total " + serviceTotal.ToString() + " DOES NOT = SERTOT " + serTotal.ToString() + " Contract " + trustNumber + "!");

                        merchandiseTotal = MERCH_CASKET_PRICE.ObjToDouble() + MERCH_URN_PRICE.ObjToDouble();
                        merchandiseTotal += MERCH_OUTER_CONTAINER_PRICE.ObjToDouble() + MERCH_ALT_CONTAINER_PRICE.ObjToDouble();
                        merchandiseTotal += MERCH_REGISTER_BOOK.ObjToDouble() + MERCH_GRAVE_MARKER.ObjToDouble();
                        merchandiseTotal += MERCH_ACKNOWLEDGEMENT_CARDS.ObjToDouble() + MERCH_OTHER.ObjToDouble();
                        merchandiseTotal = G1.RoundValue(merchandiseTotal);

                        //if (merchandiseTotal != merTotal)
                        //    G1.WriteAudit("***WARNING Merchandise Total " + merchandiseTotal.ToString() + " DOES NOT = MERTOT " + merTotal.ToString() + "!");

                        CASH_ADVANCE_DEATH_CERTIFICATES = GetMappedData(dt, i, mapDt, "CASH_ADVANCE_DEATH_CERTIFICATES");
                        CASH_ADVANCE_BEAUTICIAN = GetMappedData(dt, i, mapDt, "CASH_ADVANCE_BEAUTICIAN");
                        CASH_ADVANCE_MUSIC = GetMappedData(dt, i, mapDt, "CASH_ADVANCE_MUSIC");
                        CASH_ADVANCE_MSC = GetMappedData(dt, i, mapDt, "CASH_ADVANCE_MSC");

                        cashAdvance = CASH_ADVANCE_DEATH_CERTIFICATES.ObjToDouble() + CASH_ADVANCE_BEAUTICIAN.ObjToDouble();
                        cashAdvance += CASH_ADVANCE_MUSIC.ObjToDouble() + CASH_ADVANCE_MSC.ObjToDouble();

                        INSURED_FIRST_NAME = GetMappedData(dt, i, mapDt, "FNAME");
                        INSURED_LAST_NAME = GetMappedData(dt, i, mapDt, "LNAME");

                        //unmapped = GetUnMappedFields(dt, i, "BASIC");
                        //Lines = unmapped.Split('\n');

                        cmd = "Select * from `contracts` where `contractNumber` = '" + trustNumber + "';";
                        contractDt = G1.get_db_data(cmd);
                        if (contractDt.Rows.Count > 0)
                            record = contractDt.Rows[0]["record"].ObjToString();
                        else
                        {
                            G1.WriteAudit("***ERROR*** Cannot Find Contract " + trustNumber + " in Database!");
                            notFound++;
                            continue;
                        }

                        oldSerTotal = contractDt.Rows[0]["serviceTotal"].ObjToDouble();
                        if (oldSerTotal != serTotal)
                            G1.WriteAudit("***WARNING Old Service Total " + oldSerTotal.ToString() + " DOES NOT = SERTOT " + serTotal.ToString() + " Contract " + trustNumber + "!");

                        AddServices(trustNumber, "BASIC SERVICES OF FUNERAL DIRECTOR AND STAFF", SERVICES_BASIC_SERVICES, "Service");
                        AddServices(trustNumber, "DIRECT CREMATION WITH CONTAINER PROVIDED BY PURCHASER", SERVICES_DIRECT_CREMATION, "Service");
                        AddServices(trustNumber, "FACILITY, STAFF AND EQUIPMENT FOR VISITATION EVENING BEFORE FUNERAL SERVICE", SERVICES_VISITATION, "Service");

                        AddServices(trustNumber, "STAFF AND EQUIPMENT FOR VISITATION SAME DAY AS FUNERAL SERVICE", SERVICES_VISITATION_SAME_DAY, "Service");
                        AddServices(trustNumber, "FACILITY, STAFF AND EQUIPMENT FOR MEMORIAL SERVICE", SERVICES_MEMORIAL, "Service");
                        AddServices(trustNumber, "TRANSPORATION MILEAGE", SERVICES_MILEAGE, "Service");

                        AddServices(trustNumber, "FACILITY, STAFF AND EQUIPMENT FOR FUNERAL SERVICE", SERVICES_FUNERAL, "Service");
                        AddServices(trustNumber, "EMBALMING, AUTOPSY AND DONOR", SERVICES_EMBALMING, "Service");
                        AddServices(trustNumber, "TRANSFER OF REMAINS TO THE FUNERAL HOME", SERVICES_TRANSFER_REMAINS, "Service");
                        AddServices(trustNumber, "RECEIVING REMAINS FROM ANOTHER FUNERAL HOME", SERVICES_RECEIVING_REMAINS, "Service");
                        AddServices(trustNumber, "Committal Equipment", SERVICES_COMMITTAL_EQUIPMENT, "Service");
                        AddServices(trustNumber, "LEAD/SAFETY CAR", SERVICES_LEAD_CAR, "Service");
                        AddServices(trustNumber, "HEARSE", SERVICES_HEARSE, "Service");
                        AddServices(trustNumber, "EQUIPMENT/UTILITY VAN", SERVICES_TRANSPORTATION, "Service");
                        AddServices(trustNumber, "Immediate Burial", SERVICES_IMMEDIATE_BURIAL, "Service");
                        AddServices(trustNumber, "STAFF AND EQUIPMENT FOR GRAVESIDE SERVICE", SERVICES_GRAVESIDE_SERVICE, "Service");
                        AddServices(trustNumber, "FORWARDING REMAINS TO ANOTHER FUNERAL HOME", SERVICES_FORWARDING_REMAINS, "Service");
                        AddServices(trustNumber, "OTHER PREPARATION OF THE BODY", SERVICES_BODY_PREP, "Service");
                        AddServices(trustNumber, "FACILITY, STAFF AND EQUIPMENT FOR FUNERAL SERVICE", SERVICES_FACILITIES, "Service");
                        AddServices(trustNumber, "Services Discounts", SERVICES_DISCOUNTS, "Service");
                        AddServices(trustNumber, "Services Guaranteed", SERVICES_GUARANTEED, "Service");

                        AddServices(trustNumber, "Casket Name", MERCH_CASKET_NAME, "Merchandise");
                        AddServices(trustNumber, "Casket Price", MERCH_CASKET_PRICE, "Merchandise");
                        AddServices(trustNumber, "Casket Description", MERCH_CASKET_DESCRIPTION, "Merchandise");
                        AddServices(trustNumber, "URN Name", MERCH_URN_NAME, "Merchandise");
                        AddServices(trustNumber, "URN Price", MERCH_URN_PRICE, "Merchandise");
                        AddServices(trustNumber, "URN Description", MERCH_URN_DESCRIPTION, "Merchandise");
                        AddServices(trustNumber, "Outer Container Name", MERCH_OUTER_CONTAINER_NAME, "Merchandise");
                        AddServices(trustNumber, "Outer Container Price", MERCH_OUTER_CONTAINER_PRICE, "Merchandise");
                        AddServices(trustNumber, "Outer Container Description", MERCH_OUTER_CONTAINER_DESCRIPTION, "Merchandise");
                        AddServices(trustNumber, "ALT Container Name", MERCH_ALT_CONTAINER_NAME, "Merchandise");
                        AddServices(trustNumber, "ALT Container Price", MERCH_ALT_CONTAINER_PRICE, "Merchandise");
                        AddServices(trustNumber, "ALT Container Description", MERCH_ALT_CONTAINER_DESCRIPTION, "Merchandise");
                        AddServices(trustNumber, "REGISTER BOOK AND POUCH", MERCH_REGISTER_BOOK, "Merchandise");
                        AddServices(trustNumber, "TEMPORARY GRAVE MARKER", MERCH_GRAVE_MARKER, "Merchandise");
                        AddServices(trustNumber, "ACKNOWLEDGEMENT CARDS", MERCH_ACKNOWLEDGEMENT_CARDS, "Merchandise");
                        AddServices(trustNumber, "Merchandise Other", MERCH_OTHER, "Merchandise");
                        AddServices(trustNumber, "Merchandise Discounts", MERCH_DISCOUNTS, "Merchandise");
                        AddServices(trustNumber, "Merchandise Guaranteed", MERCH_GUARANTEED, "Merchandise");

                        AddServices(trustNumber, "Cash Death Certificates", CASH_ADVANCE_DEATH_CERTIFICATES, "Cash Advance");
                        AddServices(trustNumber, "Cash Beautician", CASH_ADVANCE_BEAUTICIAN, "Cash Advance");
                        AddServices(trustNumber, "Cash Music", CASH_ADVANCE_MUSIC, "Cash Advance");
                        AddServices(trustNumber, "Cash Msc", CASH_ADVANCE_MSC, "Cash Advance");
                        AddServices(trustNumber, "Cash Discounts", CASH_ADVANCE_DISCOUNTS, "Cash Advance");
                        updated++;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("***ERROR*** " + ex.Message.ToString());
                    }
                }
                G1.WriteAudit("Finished Import");
                barImport.Value = lastrow;
                MessageBox.Show("Data Import of Services (Updated " + updated.ToString() + ") (Passed " + passed.ToString() + ") (Not Found " + notFound.ToString() + ") !");
            }
            catch (Exception ex)
            {
                MessageBox.Show("*** Add New Contracts ERROR *** " + ex.Message.ToString());
            }
            this.SetBounds(rect.Left, rect.Top, rect.Width, rect.Height);
        }
        /***********************************************************************************************/
        private bool ValidateFDLIC(DataTable dt)
        {
            bool rtn = true;
            string dueDate = "";
            string issueDate = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dueDate = dt.Rows[i]["DUE_DATE"].ObjToString();
                if (dueDate.Length >= 8)
                    dueDate = dueDate.Substring(0, 8);
                if (!G1.validate_date(dueDate))
                {
                    dt.Rows[i]["DUE_DATE"] = "**" + dueDate + "**";
                    rtn = false;
                }

                issueDate = dt.Rows[i]["SIGNED_DATE"].ObjToString();
                if (issueDate.Length >= 8)
                    issueDate = issueDate.Substring(0, 8);

                if (!G1.validate_date(issueDate))
                {
                    issueDate = dt.Rows[i]["TRUST_SEQ_DATE"].ObjToString();
                    if (issueDate.Length >= 8)
                        issueDate = issueDate.Substring(0, 8);
                    if (!G1.validate_date(issueDate))
                    {
                        dt.Rows[i]["TRUST_SEQ_DATE"] = "**" + issueDate + "**";
                        rtn = false;
                    }
                }
            }
            if (!rtn)
                MessageBox.Show("***ERROR*** There are some invalid DueDates or Trust_Seq_Dates in this File! You will not be able to import without fixing!");
            return rtn;
        }
        /***********************************************************************************************/
        private void AddServices(string contractNumber, string service, string data, string type, bool replace = false)
        {
            if (String.IsNullOrWhiteSpace(data))
                return;
            string record = "";
            string cmd = "Select * from `cust_services` where `contractNumber` = '" + contractNumber + "' ";
            cmd += " and `service` = '" + service + "';";
            DataTable customerDt = G1.get_db_data(cmd);
            if (customerDt.Rows.Count > 0)
            {
                if (!replace)
                    return;
                record = customerDt.Rows[0]["record"].ObjToString();
            }
            else
                record = G1.create_record("cust_services", "data", "-1");
            if (G1.BadRecord("cust_services", record))
                return;
            G1.update_db_table("cust_services", "record", record, new string[] { "service", service, "data", data, "type", type, "contractNumber", contractNumber });
        }
        /***********************************************************************************************/
        private string LookupAgentCode(string agentFDLIC, string contract)
        {
            string agentCode = "";
            string trust = "";
            string loc = "";
            string locCode = "";
            string miniContract = Trust85.decodeContractNumber(contract, ref trust, ref loc);
            for (; ; )
            {
                if (agentFDLIC.Length >= 6)
                    break;
                agentFDLIC = "0" + agentFDLIC;
            }

            string cmd = "Select * from `agents` where `agentIncoming` = '" + agentFDLIC + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
                agentCode = dx.Rows[0]["agentCode"].ObjToString();
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                locCode = dx.Rows[i]["locCode"].ObjToString();
                string[] Lines = locCode.Split(',');
                for (int j = 0; j < Lines.Length; j++)
                {
                    if (Lines[j].ToUpper() == loc.ToUpper())
                    {
                        agentCode = dx.Rows[i]["agentCode"].ObjToString();
                        break;
                    }
                }
            }
            return agentCode;
        }
        /***********************************************************************************************/
        private void matchFDLICToExcelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MatchFDLIC matchForm = new MatchFDLIC();
            matchForm.Show();
        }
        /***********************************************************************************************/
        private void importZipCodesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Import importForm = new Import();
            importForm.SelectDone += ImportForm_ZipDone;
            importForm.Show();
        }
        /***********************************************************************************************/
        private void ImportForm_ZipDone(DataTable dt)
        {
            if (dt == null)
                return;
            if (dt.Rows.Count <= 0)
                return;
            string zipCode = "";
            string city = "";
            string state = "";
            string county = "";
            string cmd = "";
            DataTable dx = null;
            string record = "";
            this.Cursor = Cursors.WaitCursor;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                zipCode = dt.Rows[i]["zip"].ObjToString();
                if (String.IsNullOrWhiteSpace(zipCode))
                    continue;
                city = dt.Rows[i]["city"].ObjToString();
                state = dt.Rows[i]["state_name"].ObjToString();
                county = dt.Rows[i]["county_name"].ObjToString();
                cmd = "Select * from `ref_zipcodes` where `zipcode` = '" + zipCode + "';";
                dx = G1.get_db_data(cmd);
                record = "";
                if (dx.Rows.Count > 0)
                    record = dx.Rows[0]["record"].ObjToString();
                if (String.IsNullOrWhiteSpace(record))
                    record = G1.create_record("ref_zipcodes", "county", "-1");
                if (G1.BadRecord("ref_zipcodes", record))
                    continue;
                G1.update_db_table("ref_zipcodes", "record", record, new string[] { "zipcode", zipCode, "city", city, "state", state, "county", county });
            }
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void toolStripMenuItem19_Click(object sender, EventArgs e)
        {
            Import importForm = new Import("Merchandise");
            importForm.SelectDone += ImportForm_Merchandise;
            importForm.Show();
        }
        /***********************************************************************************************/
        private void ImportForm_Merchandise(DataTable dt)
        {
            DataTable dx = null;
            string cmd = "";
            string record = "";
            string contract = "";
            string extraItemAmtMI1 = "";
            string extraItemAmtMI2 = "";
            string extraItemAmtMI3 = "";
            string extraItemAmtMI4 = "";
            string extraItemAmtMI5 = "";
            string extraItemAmtMI6 = "";
            string extraItemAmtMI7 = "";
            string extraItemAmtMI8 = "";
            string extraItemAmtMR1 = "";
            string extraItemAmtMR2 = "";
            string extraItemAmtMR3 = "";
            string extraItemAmtMR4 = "";
            string extraItemAmtMR5 = "";
            string extraItemAmtMR6 = "";
            string extraItemAmtMR7 = "";
            string extraItemAmtMR8 = "";
            double casketPrice = 0D;
            double vaultPrice = 0D;
            labelMaximum.Show();
            labelMaximum.Text = "0";
            barImport.Show();

            Rectangle rect = this.Bounds;

            int height = rect.Height + 132 - 103;
            this.SetBounds(rect.Left, rect.Top, rect.Width, height);

            G1.CreateAudit("Import_Merchandise");

            int lastrow = dt.Rows.Count;
            int tableRow = 0;
            // lastrow = 1; // For Debug Purposes
            try
            {
                lblTotal.Show();
                barImport.Minimum = 0;
                barImport.Maximum = lastrow;
                lblTotal.Text = "of " + lastrow.ToString();
                lblTotal.Refresh();
                int start = 0;
                //                start = 9000;

                for (int i = start; i < lastrow; i++)
                {
                    barImport.Value = i;
                    barImport.Refresh();
                    labelMaximum.Text = i.ToString();
                    labelMaximum.Refresh();

                    tableRow = i;
                    record = "";
                    try
                    {
                        contract = dt.Rows[i]["cnum"].ObjToString();
                        if (String.IsNullOrWhiteSpace(contract))
                            continue;
                        if (contract == "/")
                            contract = "X" + i.ToString();
                        else if (contract == "\\")
                            contract = "X" + i.ToString();

                        cmd = "Select * from `customers` where `contractNumber` = '" + contract + "';";
                        dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count <= 0)
                        {
                            G1.WriteAudit("Cannot Locate Contract " + contract + "!");
                            continue;
                        }
                        record = dx.Rows[0]["record"].ObjToString();
                        extraItemAmtMI1 = dt.Rows[i]["mi1"].ObjToString(); // Casket Code
                        extraItemAmtMI1 = extraItemAmtMI1.Replace("-", "");
                        extraItemAmtMI2 = dt.Rows[i]["mi2"].ObjToString(); // Vault Code
                        extraItemAmtMI3 = dt.Rows[i]["mi3"].ObjToString();
                        extraItemAmtMI4 = dt.Rows[i]["mi4"].ObjToString();
                        extraItemAmtMI5 = dt.Rows[i]["mi5"].ObjToString();
                        extraItemAmtMI6 = dt.Rows[i]["mi6"].ObjToString();
                        extraItemAmtMI7 = dt.Rows[i]["mi7"].ObjToString();
                        extraItemAmtMI8 = dt.Rows[i]["mi8"].ObjToString();

                        extraItemAmtMR1 = dt.Rows[i]["mr1"].ObjToString(); // Casket Price1
                        extraItemAmtMR2 = dt.Rows[i]["mr2"].ObjToString(); // Vault Price1
                        extraItemAmtMR3 = dt.Rows[i]["mr3"].ObjToString(); // Casket Price2, add to Casket Price1 for Total Price
                        extraItemAmtMR4 = dt.Rows[i]["mr4"].ObjToString(); // Vault Price2, add to Vault Price1 for Total Price
                        extraItemAmtMR5 = dt.Rows[i]["mr5"].ObjToString();
                        extraItemAmtMR6 = dt.Rows[i]["mr6"].ObjToString();
                        extraItemAmtMR7 = dt.Rows[i]["mr7"].ObjToString();
                        extraItemAmtMR8 = dt.Rows[i]["mr8"].ObjToString();

                        casketPrice = extraItemAmtMR1.ObjToDouble() + extraItemAmtMR3.ObjToDouble();
                        vaultPrice = extraItemAmtMR2.ObjToDouble() + extraItemAmtMR4.ObjToDouble();

                        G1.update_db_table("customers", "record", record, new string[] { "extraItemAmtMI1", extraItemAmtMI1, "extraItemAmtMI2", extraItemAmtMI2, "extraItemAmtMI3", extraItemAmtMI3, "extraItemAmtMI4", extraItemAmtMI4, "extraItemAmtMI5", extraItemAmtMI5, "extraItemAmtMI6", extraItemAmtMI6, "extraItemAmtMI7", extraItemAmtMI7, "extraItemAmtMI8", extraItemAmtMI8 });
                        G1.update_db_table("customers", "record", record, new string[] { "extraItemAmtMR1", extraItemAmtMR1, "extraItemAmtMR2", extraItemAmtMR2, "extraItemAmtMR3", extraItemAmtMR3, "extraItemAmtMR4", extraItemAmtMR4, "extraItemAmtMR5", extraItemAmtMR5, "extraItemAmtMR6", extraItemAmtMR6, "extraItemAmtMR7", extraItemAmtMR7, "extraItemAmtMR8", extraItemAmtMR8 });
                        if (!LocateMerchandise(extraItemAmtMI1))
                            G1.WriteAudit("***Warning*** Cannot Locate Casket Code " + extraItemAmtMI1 + " Contract " + contract);
                        if (!LocateMerchandise(extraItemAmtMI2))
                            G1.WriteAudit("***Warning*** Cannot Locate Vault Code " + extraItemAmtMI2 + " Contract " + contract);
                    }
                    catch (Exception ex)
                    {
                        G1.WriteAudit("***ERROR*** Exception Row =" + i.ToString() + " " + ex.Message.ToString());
                        dt.Rows[i]["num"] = "*ERROR*";
                    }
                }
                G1.WriteAudit("Finished Import");
                barImport.Value = lastrow;
                MessageBox.Show("Customer Merchandise Import of " + lastrow + " Rows Complete . . .");
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Creating Customer Record/Row! " + contract + "/" + tableRow.ToString() + " Stopping! " + ex.Message.ObjToString());
            }
            this.SetBounds(rect.Left, rect.Top, rect.Width, rect.Height);
        }
        /***********************************************************************************************/
        private bool LocateMerchandise(string casketCode)
        {
            if (String.IsNullOrWhiteSpace(casketCode))
                return true;
            string cmd = "Select * from `casket_master` where `casketcode` = '" + casketCode + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
            {
                cmd = "Select * from `casket_master` where `casketcode` LIKE '" + casketCode + "%';";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count <= 0)
                    return false;
            }
            return true;
        }
        /***********************************************************************************************/
        private void pastCommissionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ThePast pastForm = new ThePast();
            pastForm.Show();
            //Trust85.allAgentsDt = G1.get_db_data("Select * from `agents`");
            //double commission = 0D;
            //string agent = "N07";
            //string location = "M";
            //string splits = "";
            //double percent = 0.01D;
            //double goal = 65000D;
            //DateTime lapseDate8 = new DateTime(2019, 6, 6);
            //DateTime iDate = new DateTime(2015, 1, 1);
            //double totalContracts = 0D;
            //double recaps = 0D;
            //double dbrSales = 0D;
            //double recapContracts = 0D;
            //try
            //{
            //    commission = Trust85.CalculatePastCommissions("lapseDate8", agent, location, splits, percent, goal, lapseDate8, iDate, iDate, ref totalContracts, ref recaps, ref recapContracts, ref dbrSales);
            //}
            //catch ( Exception ex)
            //{

            //}
        }
        /***********************************************************************************************/
        private void processIssueDatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImportIssueDates importForm = new ImportIssueDates("PREACC");
            importForm.Show();
        }
        /***********************************************************************************************/
        private void pieChartToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }
        /***********************************************************************************************/
        private void comparePREACCDueDatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ComparePreacc compareForm = new ComparePreacc();
            compareForm.Show();
        }
        /***********************************************************************************************/
        private void fixTrust2013DataCopyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CopyTrust2013 copyForm = new CopyTrust2013();
            copyForm.Show();
        }
        /***********************************************************************************************/
        private void unityTrustPolicyImportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImportUnityPolicies unityForm = new ImportUnityPolicies();
            unityForm.Show();
        }
        /***********************************************************************************************/
        private void testExportImportDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OutIn outForm = new OutIn();
            outForm.Show();
        }
        /***********************************************************************************************/
        private void bankAccountInfomationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Import bankForm = new Import();
            bankForm.SelectDone += BankForm_SelectDone;
            bankForm.Show();
        }
        /***********************************************************************************************/
        private void BankForm_SelectDone(DataTable dt)
        {
            if (dt == null)
                return;
            if (dt.Rows.Count <= 0)
                return;
            string generalLedger = "";
            string accountTitle = "";
            string accountNumber = "";
            string location = "";
            string cmd = "";
            DataTable dx = null;
            string record = "";
            this.Cursor = Cursors.WaitCursor;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                generalLedger = dt.Rows[i]["General Ledger"].ObjToString();
                if (String.IsNullOrWhiteSpace(generalLedger))
                    continue;
                accountTitle = dt.Rows[i]["Account Title"].ObjToString();
                accountNumber = dt.Rows[i]["Account Number"].ObjToString();
                if (String.IsNullOrWhiteSpace(accountTitle))
                {
                    location = generalLedger;
                    continue;
                }
                cmd = "Select * from `bank_accounts` where `location` = '" + location + "' ";
                cmd += " AND `general_ledger_no` = '" + generalLedger + "' ";
                cmd += " AND `account_no` = '" + accountNumber + "' ";
                cmd += ";";
                dx = G1.get_db_data(cmd);
                record = "";
                if (dx.Rows.Count > 0)
                    record = dx.Rows[0]["record"].ObjToString();
                else
                    record = G1.create_record("bank_accounts", "location", "-1");
                if (G1.BadRecord("bank_accounts", record))
                    continue;
                G1.update_db_table("bank_accounts", "record", record, new string[] { "location", location, "general_ledger_no", generalLedger, "account_title", accountTitle, "account_no", accountNumber });
            }
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void testPREACCContractsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TestPreacc preForm = new TestPreacc();
            preForm.Show();
        }
        /***********************************************************************************************/
        private void toolStripMenuItem21_Click(object sender, EventArgs e)
        {
            TestDeaths testForm = new TestDeaths();
            testForm.Show();
        }
        /***********************************************************************************************/
        private void pastInsuranceMonthsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Import importForm = new Import();
            importForm.SelectDone += ImportForm_SelectDone3;
            importForm.Show();
        }
        /***********************************************************************************************/
        private void ImportForm_SelectDone3(DataTable dt)
        {
            InsuranceMonthscs insuranceForm = new InsuranceMonthscs(dt);
            insuranceForm.Show();
        }
        /***********************************************************************************************/
        private void testDecodeExponentialToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string data = "2.01912E+19";
            string date = Import.ConvertScientificDate(data);
        }
        /***********************************************************************************************/
        private void compareInsuranceLockBoxResultsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CompareInsPayments compareForm = new CompareInsPayments();
            compareForm.Show();
        }
        /***********************************************************************************************/
        private void compareAS400FileToSMFSImportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CompareInsPayments compareForm = new CompareInsPayments();
            compareForm.Show();
        }
        /***********************************************************************************************/
        private void toolStripMenuItem23_Click(object sender, EventArgs e)
        {
            //TestPrint testForm = new TestPrint();
            //testForm.Show();

            //FixPolicies();

            //FixBankAccounts();

            //FixManualPayments();
            XtraReport2 report = new XtraReport2();
            report.ShowPreview();


        }
        /***********************************************************************************************/
        private void FixManualPayments()
        {
            bool doit = false;
            DataTable dx = null;
            DateTime payDate8 = DateTime.Now;
            string record = "";
            string edited = "";
            string contractNumber = "";
            double downPayment = 0D;
            double trust85 = 0D;
            double trust100 = 0D;
            double trust85P = 0D;
            double trust100P = 0D;
            DateTime docp = DateTime.Now;
            double amtOfMonthlyPayt = 0D;
            double contractValue = 0D;
            double retained = 0D;
            double payment = 0D;
            double debit = 0D;
            double credit = 0D;
            double newprincipal = 0D;
            double interest = 0D;
            double financeMonths = 0D;
            string apr = "";
            double rate = 0D;
            double newTrust85 = 0D;
            double paymentCurrMonth = 0D;
            double difference = 0D;
            double originalDownPayment = 0D;
            string lockTrust85 = "";

            DataTable ddx = new DataTable();
            ddx.Columns.Add("num");
            ddx.Columns.Add("contractNumber");
            ddx.Columns.Add("record");
            ddx.Columns.Add("payDate8");
            ddx.Columns.Add("oldTrust85P", Type.GetType("System.Double"));
            ddx.Columns.Add("oldTrust100P", Type.GetType("System.Double"));
            ddx.Columns.Add("newTrust85P", Type.GetType("System.Double"));
            ddx.Columns.Add("newTrust100P", Type.GetType("System.Double"));
            ddx.Columns.Add("paymentCurrMonth", Type.GetType("System.Double"));
            ddx.Columns.Add("difference", Type.GetType("System.Double"));

            DateTime date = DateTime.Now;
            string issueDate = "";
            int method = 0;
            bool gotDebit = false;
            string fill = "";
            this.Cursor = Cursors.WaitCursor;
            //            string cmd = "SELECT * FROM `payments` c JOIN `contracts` d ON c.`contractNumber` = d.`contractNumber` WHERE c.`edited` = 'Manual' AND c.`payDate8` >= '2019-11-01';";
            string cmd = "SELECT * FROM `payments` c JOIN `contracts` d ON c.`contractNumber` = d.`contractNumber` WHERE c.`payDate8` >= '2019-11-01' AND `payDate8` <= '2019-11-30';";
            DataTable dt = G1.get_db_data(cmd);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                fill = dt.Rows[i]["fill"].ObjToString().ToUpper();
                if (fill == "D")
                    continue;
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                financeMonths = dt.Rows[i]["numberOfPayments"].ObjToDouble();
                amtOfMonthlyPayt = dt.Rows[i]["amtOfMonthlyPayt"].ObjToDouble();
                issueDate = dt.Rows[i]["issueDate8"].ObjToString();
                apr = dt.Rows[i]["APR"].ObjToString();
                rate = apr.ObjToDouble() / 100D;
                contractValue = DailyHistory.GetContractValue(dt.Rows[i]);
                contractValue = G1.RoundValue(contractValue);


                downPayment = dt.Rows[i]["downPayment1"].ObjToDouble();
                if (downPayment == 0D)
                    downPayment = DailyHistory.GetDownPaymentFromPayments(contractNumber);
                originalDownPayment = downPayment;

                record = dt.Rows[i]["record"].ObjToString();
                edited = dt.Rows[i]["edited"].ObjToString();
                docp = dt.Rows[i]["payDate8"].ObjToDateTime();
                downPayment = dt.Rows[i]["downPayment"].ObjToDouble();
                payment = dt.Rows[i]["paymentAmount"].ObjToDouble();
                interest = dt.Rows[i]["interestPaid"].ObjToDouble();
                debit = dt.Rows[i]["debitAdjustment"].ObjToDouble();
                credit = dt.Rows[i]["creditAdjustment"].ObjToDouble();
                trust85 = dt.Rows[i]["trust85P"].ObjToDouble();
                trust100 = dt.Rows[i]["trust100P"].ObjToDouble();
                if (payment == 0D && downPayment > 0D)
                    payment = downPayment;

                payment = G1.RoundValue(payment);
                interest = G1.RoundValue(interest);
                debit = G1.RoundValue(debit);
                credit = G1.RoundValue(credit);
                downPayment = G1.RoundValue(downPayment);


                lockTrust85 = dt.Rows[i]["lockTrust85"].ObjToString().ToUpper();

                newprincipal = payment - interest - debit + credit;
                newprincipal = G1.RoundDown(newprincipal);

                //gotDebit = false;
                //if (credit != 0D || debit != 0D)
                //    continue;
                //if (edited.ToUpper() == "TRUSTADJ")
                //    continue;

                try
                {
                    method = ImportDailyDeposits.CalcTrust85P(docp, amtOfMonthlyPayt, issueDate, contractValue, originalDownPayment, financeMonths, payment, newprincipal, debit, credit, rate, ref trust85P, ref trust100P, ref retained);

                    paymentCurrMonth = getTrust2013r(contractNumber, docp);

                    if (lockTrust85 == "Y")
                    {
                        trust85P = trust85;
                        trust100P = trust100;
                    }
                    else if (payment == 0D && credit == 1D && interest == 0D)
                    {
                        trust85P = trust85;
                        trust100P = trust100;
                    }
                    else
                    {
                        if (credit != 0D || debit != 0D || edited.ToUpper() == "TRUSTADJ")
                        {
                            trust85P = trust85;
                            trust100P = trust100;
                        }
                        else if (interest > 0D && payment <= interest)
                        {
                            if (credit == 0D && debit == 0D)
                            {
                                if (trust85 == paymentCurrMonth && trust85 < 0D)
                                {
                                    trust85P = trust85;
                                    trust100P = trust100;
                                }
                            }
                        }
                    }
                    difference = trust85 - trust85P;

                    DataRow dRow = ddx.NewRow();
                    dRow["record"] = record;
                    dRow["contractNumber"] = contractNumber;
                    dRow["payDate8"] = docp.ToString("yyyy-MM-dd");
                    dRow["oldTrust85P"] = trust85;
                    dRow["oldTrust100P"] = trust100;
                    dRow["newTrust85P"] = trust85P;
                    dRow["newTrust100P"] = trust100P;
                    dRow["paymentCurrMonth"] = paymentCurrMonth;
                    dRow["difference"] = difference;
                    ddx.Rows.Add(dRow);
                    //if ( difference != 0D )
                    //    G1.update_db_table("payments", "record", record, new string[] { "trust85P", trust85P.ToString(), "trust100P", trust100P.ToString() });


                    //if (trust85P != trust85 )
                    //{
                    //    paymentCurrMonth = getTrust2013r(contractNumber, docp);
                    //    if ( paymentCurrMonth != trust85P)
                    //    {
                    //    }
                    //    DataRow dRow = ddx.NewRow();
                    //    dRow["contractNumber"] = contractNumber;
                    //    dRow["payDate8"] = docp.ToString("yyyy-MM-dd");
                    //    dRow["oldTrust85P"] = trust85;
                    //    dRow["oldTrust100P"] = trust100;
                    //    dRow["newTrust85P"] = trust85P;
                    //    dRow["newTrust100P"] = trust100P;
                    //    dRow["paymentCurrMonth"] = paymentCurrMonth;
                    //    ddx.Rows.Add(dRow);
                    //}

                    //G1.update_db_table("payments", "record", record, new string[] { "bank_account", bankAccount });
                }
                catch (Exception ex)
                {
                }
            }
            this.Cursor = Cursors.Default;
            G1.NumberDataTable(ddx);
            FixManualEdits fixForm = new FixManualEdits(ddx);
            fixForm.Show();
            //MessageBox.Show("***INFO*** Finished!");
        }
        /***********************************************************************************************/
        private double getTrust2013r(string contractNumber, DateTime payDate8)
        {
            double paymentCurrMonth = 0D;
            int days = DateTime.DaysInMonth(payDate8.Year, payDate8.Month);
            DateTime date = new DateTime(payDate8.Year, payDate8.Month, days);
            string cmd = "Select * from `trust2013r` where `contractNumber` = '" + contractNumber + "' AND `payDate8` = '" + date.ToString("yyyy-MM-dd") + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                paymentCurrMonth = dt.Rows[0]["paymentCurrMonth"].ObjToDouble();
            }
            return paymentCurrMonth;
        }
        /***********************************************************************************************/
        private void FixBankAccounts()
        {
            string lapsed = "";
            string lapsed1 = "";
            string lapsed2 = "";
            DateTime deceasedDate = DateTime.Now;
            string policyNumber = "";
            string firstRecord = "";
            string secondRecord = "";
            string report = "";
            string report1 = "";
            string report2 = "";
            string agentCode1 = "";
            string agentCode2 = "";
            string agentCode = "";
            double hPremium1 = 0D;
            double hPremium2 = 0D;
            double Premium = 0D;
            string fname = "";
            string lname = "";
            bool doit = false;
            string lkbx_account = "";
            string ach_account = "";
            LoadBankAccounts(ref lkbx_account, ref ach_account);
            DataTable dx = null;
            DateTime payDate8 = DateTime.Now;
            string bankAccount = "";
            string record = "";
            string edited = "";
            string deposit = "";
            this.Cursor = Cursors.WaitCursor;
            string paymentFile = "payments";
            paymentFile = "ipayments";
            string cmd = "Select * from `" + paymentFile + "` where `payDate8` > '2020-07-01' and `bank_account` = '';";
            DataTable dt = G1.get_db_data(cmd);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    bankAccount = dt.Rows[i]["bank_account"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(bankAccount))
                        continue;
                    edited = dt.Rows[i]["edited"].ObjToString().ToUpper();
                    if (edited == "MANUAL" || edited == "TRUSTADJ")
                        continue;
                    deposit = dt.Rows[i]["depositNumber"].ObjToString();
                    if (String.IsNullOrWhiteSpace(deposit))
                        continue;
                    if (deposit.Length > 1)
                        deposit = deposit.Substring(0, 1).ToUpper();
                    if (deposit == "T")
                        bankAccount = lkbx_account;
                    else if (deposit == "A")
                        bankAccount = ach_account;
                    else
                        continue;
                    record = dt.Rows[i]["record"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(bankAccount))
                    {
                        G1.update_db_table(paymentFile, "record", record, new string[] { "bank_account", bankAccount });
                    }
                }
                catch (Exception ex)
                {
                }
            }
            this.Cursor = Cursors.Default;
            MessageBox.Show("***INFO*** Finished!");
        }
        /***************************************************************************************/
        private void LoadBankAccounts(ref string lkbx_ach_account, ref string ach_account)
        {
            string location = "";
            string bank_gl = "";
            string bankAccount = "";
            string cmd = "Select * from `bank_accounts` where `lkbx_ach` = '1';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                location = dx.Rows[0]["location"].ObjToString();
                bank_gl = dx.Rows[0]["general_ledger_no"].ObjToString();
                bankAccount = dx.Rows[0]["account_no"].ObjToString();
                lkbx_ach_account = location + "~" + bank_gl + "~" + bankAccount;
            }

            cmd = "Select * from `bank_accounts` where `ach` = '1';";
            dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                location = dx.Rows[0]["location"].ObjToString();
                bank_gl = dx.Rows[0]["general_ledger_no"].ObjToString();
                bankAccount = dx.Rows[0]["account_no"].ObjToString();
                ach_account = location + "~" + bank_gl + "~" + bankAccount;
            }
        }
        /***********************************************************************************************/
        private void FixPolicies()
        {
            string lapsed = "";
            string lapsed1 = "";
            string lapsed2 = "";
            DateTime deceasedDate = DateTime.Now;
            string policyNumber = "";
            string firstRecord = "";
            string secondRecord = "";
            string report = "";
            string report1 = "";
            string report2 = "";
            string agentCode1 = "";
            string agentCode2 = "";
            string agentCode = "";
            double hPremium1 = 0D;
            double hPremium2 = 0D;
            double Premium = 0D;
            string fname = "";
            string lname = "";
            bool doit = false;
            DataTable dx = null;
            this.Cursor = Cursors.WaitCursor;
            string cmd = "Select * from `policies` where `policyNumber` LIKE '0%';";
            //cmd = "Select * from `policies` where `policyNumber` LIKE '0%' AND `contractNumber` = 'ZZ0004624';";
            DataTable dt = G1.get_db_data(cmd);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    lapsed = dt.Rows[i]["lapsed"].ObjToString().ToUpper();
                    firstRecord = dt.Rows[i]["record"].ObjToString();
                    policyNumber = dt.Rows[i]["policyNumber"].ObjToString();
                    policyNumber = policyNumber.TrimStart('0');
                    policyNumber = policyNumber.Trim();
                    if (String.IsNullOrWhiteSpace(policyNumber))
                        continue;
                    lname = dt.Rows[i]["policyLastName"].ObjToString();
                    fname = dt.Rows[i]["policyFirstName"].ObjToString();
                    lapsed1 = dt.Rows[i]["lapsed"].ObjToString().ToUpper();
                    report1 = dt.Rows[i]["report"].ObjToString();
                    agentCode1 = dt.Rows[i]["agentCode"].ObjToString();
                    hPremium1 = dt.Rows[i]["historicPremium"].ObjToDouble();

                    cmd = "Select * from `policies` where `policyNumber` = '" + policyNumber + "' AND `policyFirstName` = '" + fname + "' AND `policyLastName` = '" + lname + "';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                    {
                        for (int j = 0; j < dx.Rows.Count; j++)
                        {
                            secondRecord = dx.Rows[j]["record"].ObjToString();
                            lapsed2 = dx.Rows[j]["lapsed"].ObjToString().ToUpper();
                            report2 = dx.Rows[j]["report"].ObjToString();
                            agentCode2 = dx.Rows[j]["agentCode"].ObjToString();
                            hPremium2 = dx.Rows[j]["historicPremium"].ObjToDouble();

                            report = report2;
                            if (String.IsNullOrWhiteSpace(report))
                                report = report1;

                            agentCode = agentCode2;
                            if (String.IsNullOrWhiteSpace(agentCode))
                                agentCode = agentCode1;

                            Premium = hPremium1;
                            if (Premium == 0D)
                                Premium = hPremium2;

                            G1.update_db_table("policies", "record", firstRecord, new string[] { "agentCode", agentCode, "report", report, "historicPremium", Premium.ToString(), "lapsed", "Y" });
                            G1.update_db_table("policies", "record", secondRecord, new string[] { "agentCode", agentCode, "report", report, "historicPremium", Premium.ToString(), "lapsed", "" });
                        }
                    }
                }
                catch (Exception ex)
                {
                }
            }
            this.Cursor = Cursors.Default;
            MessageBox.Show("***INFO*** Finished!");
        }
        /***********************************************************************************************/
        private void importPayersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Import importForm = new Import("Insurance Payer Test");
            importForm.Show();
        }
        /***********************************************************************************************/
        private void fixInsuranceContractsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FixInsurance fixForm = new FixInsurance();
            fixForm.Show();
        }
        /***********************************************************************************************/
        private void loadPayersTableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Import importForm = new Import("Load Payers");
            importForm.SelectDone += ImportForm_SelectDone4;
            importForm.Show();
        }
        /***********************************************************************************************/
        private void ImportForm_SelectDone4(DataTable dt)
        {
            string payer = "";
            string lastPayer = "";
            string address1 = "";
            string address2 = "";
            string address3 = "";
            string cmd = "";

            DataView tempview = dt.DefaultView;
            tempview.Sort = "PAYER# asc";
            dt = tempview.ToTable();

            DataTable dx = null;

            string lapsed = "";

            bool doLapsed = false;
            bool doDeceased = false;
            string actualFile = dt.TableName.ObjToString();
            if (actualFile.ToUpper().IndexOf("PAYER LAPSED") >= 0)
            {
                doLapsed = true;
                lapsed = "Y";
            }
            if (actualFile.ToUpper().IndexOf("PAYER DECEASED") >= 0)
            {
                doDeceased = true;
                ProcessPayerDeceased(dt);
            }

            double expected = 0D;
            string contractNumber = "";
            string newPayer = "";
            bool isLapsed = false;
            string firstName = "";
            string lastName = "";

            DateTime dueDate = DateTime.Now;
            DateTime DOLP = DateTime.Now;
            DateTime lapsedDate = DateTime.Now;
            DateTime reinstateDate = DateTime.Now;
            DateTime deceasedDate = DateTime.Now;
            DateTime newDueDate = DateTime.Now;
            DateTime newDOLP = DateTime.Now;
            DateTime newLapsedDate = DateTime.Now;
            DateTime newDeceasedDate = DateTime.Now;

            string myLapsed = "";

            DateTime nullDate = new DateTime(1, 1, 1);
            string record = "";

            double premium = 0D;
            double annual = 0D;

            Rectangle rect = this.Bounds;
            int lastRow = dt.Rows.Count;

            int height = rect.Height + 132 - 103;
            this.SetBounds(rect.Left, rect.Top, rect.Width, height);
            int created = 0;

            try
            {
                lblTotal.Show();

                lblTotal.Text = "of " + lastRow.ToString();
                lblTotal.Refresh();

                barImport.Visible = true;
                barImport.Minimum = 0;
                barImport.Maximum = lastRow;
                labelMaximum.Show();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    Application.DoEvents();

                    barImport.Value = i;
                    barImport.Refresh();
                    labelMaximum.Text = i.ToString() + "/" + created.ToString();
                    labelMaximum.Refresh();

                    address1 = dt.Rows[i]["address 1"].ObjToString();
                    address2 = dt.Rows[i]["address 2"].ObjToString();
                    address3 = dt.Rows[i]["address 3"].ObjToString();
                    if (address1.ToUpper().IndexOf("PAID") >= 0)
                        continue;
                    if (address2.ToUpper().IndexOf("PAID") >= 0)
                        continue;
                    if (address3.ToUpper().IndexOf("PAID") >= 0)
                        continue;
                    payer = dt.Rows[i]["payer#"].ObjToString();
                    if (String.IsNullOrWhiteSpace(payer))
                        continue;
                    payer = payer.TrimStart('0');
                    if (String.IsNullOrWhiteSpace(payer))
                        continue;
                    firstName = dt.Rows[i]["payer first name"].ObjToString();
                    lastName = dt.Rows[i]["payer last name"].ObjToString();

                    annual = 0D;
                    dueDate = nullDate;
                    DOLP = nullDate;
                    premium = 0D;
                    if (!doDeceased)
                    {
                        annual = dt.Rows[i]["annual amount due"].ObjToDouble();
                        dueDate = dt.Rows[i]["due date"].ObjToDateTime();
                        DOLP = dt.Rows[i]["last paid date"].ObjToDateTime();
                        premium = dt.Rows[i]["due each month"].ObjToDouble();
                        if (premium <= 0D || annual <= 0D)
                            continue;
                        if (dueDate.Year < 100)
                            continue;
                    }

                    cmd = "Select * from `payers` where `payer` = '" + payer + "';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                        continue;
                    record = G1.create_record("payers", "empty", "-1");
                    if (G1.BadRecord("payers", record))
                    {
                        continue;
                    }

                    lapsedDate = nullDate;
                    reinstateDate = nullDate;
                    deceasedDate = nullDate;
                    myLapsed = "";
                    if (doLapsed)
                    {
                        lapsedDate = dueDate;
                        myLapsed = lapsed;
                    }
                    if (doDeceased)
                        deceasedDate = dt.Rows[i]["deceasedDate"].ObjToDateTime();

                    contractNumber = ImportDailyDeposits.FindPayerContract(payer, premium.ToString(), ref newPayer, ref expected, ref isLapsed);
                    if (!String.IsNullOrWhiteSpace(contractNumber))
                    {

                        cmd = "Select * from `icontracts` i JOIN `icustomers` c ON i.`contractNumber` = c.`contractNumber` WHERE i.`contractNumber` = '" + contractNumber + "';";
                        dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count > 0)
                        {
                            firstName = dx.Rows[0]["firstName"].ObjToString();
                            lastName = dx.Rows[0]["lastName"].ObjToString();
                            if (doLapsed)
                            {
                                newLapsedDate = dx.Rows[0]["lapseDate8"].ObjToDateTime();
                                if (newLapsedDate > lapsedDate)
                                    lapsedDate = newLapsedDate;
                            }
                            reinstateDate = dx.Rows[0]["reinstateDate8"].ObjToDateTime();
                            if (reinstateDate.Year > 500 && lapsedDate.Year > 500 && reinstateDate > lapsedDate)
                            {
                                myLapsed = "";
                                lapsedDate = nullDate;
                            }
                            if (doDeceased)
                            {
                                newDeceasedDate = dx.Rows[0]["deceasedDate"].ObjToDateTime();
                                if (newDeceasedDate.Year < 500)
                                    deceasedDate = nullDate;
                                else if (newDeceasedDate > deceasedDate)
                                    deceasedDate = newDeceasedDate;
                            }
                            newDueDate = dx.Rows[0]["dueDate8"].ObjToDateTime();
                            if (newDueDate > dueDate)
                                dueDate = newDueDate;
                            newDOLP = dx.Rows[0]["lastDatePaid8"].ObjToDateTime();
                            if (newDOLP > DOLP)
                                DOLP = newDOLP;
                        }
                    }
                    created++;
                    G1.update_db_table("payers", "record", record, new string[] { "payer", payer, "amtOfMonthlyPayt", premium.ToString(), "annualPremium", annual.ToString(), "dueDate8", dueDate.ToString("MM/dd/yyyy"), "lastDatePaid8", DOLP.ToString("MM/dd/yyyy"), "lapsed", myLapsed, "empty", "", "lapseDate8", lapsedDate.ToString("MM/dd/yyyy"), "reinstateDate8", reinstateDate.ToString("MM/dd/yyyy"), "deceasedDate", deceasedDate.ToString("MM/dd/yyyy"), "firstName", firstName, "lastName", lastName, "contractNumber", contractNumber });
                }
            }
            catch (Exception ex)
            {
            }

            barImport.Value = lastRow;
            barImport.Refresh();
            labelMaximum.Text = lastRow.ToString() + "/" + created.ToString();
            labelMaximum.Refresh();

            MessageBox.Show("Import Payers Finished . . .");
        }
        /***********************************************************************************************/
        private void ProcessPayerDeceased(DataTable dt)
        {
            string str = "";
            dt.Columns.Add("deceasedDate");
            string address1 = "";
            string city = "";
            string month = "";
            string year = "";
            int imonth = 0;
            int iyear = 0;
            int iday = 0;
            string[] dates = null;
            DateTime deceasedDate = DateTime.Now;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                address1 = dt.Rows[i]["address 1"].ObjToString();
                city = dt.Rows[i]["city"].ObjToString().Trim();
                try
                {
                    if (address1.IndexOf("DEATH") >= 0)
                    {
                        address1 = address1.Replace("DEATH", "").Trim();
                        if (address1.Length == 4)
                        {
                            month = address1.Substring(0, 2);
                            year = address1.Substring(2, 2);
                            iday = 1;
                            iyear = year.ObjToInt32();
                            if (iyear >= 35 && iyear <= 99)
                                iyear += 1900;
                            else
                                iyear += 2000;
                            imonth = month.ObjToInt32();
                            if (imonth <= 0 || imonth > 12)
                                imonth = 1;
                            str = imonth.ToString("D2") + "/" + iday.ToString("D2") + "/" + iyear.ToString("D4");
                            dt.Rows[i]["deceasedDate"] = str;
                        }
                    }
                    if (city.Length == 6)
                    {
                        if (G1.validate_numeric(city))
                        {
                            month = city.Substring(0, 2);
                            str = city.Substring(2, 2);
                            year = city.Substring(4);
                            iday = str.ObjToInt32();
                            iyear = year.ObjToInt32();
                            if (iyear >= 35 && iyear <= 99)
                                iyear += 1900;
                            else
                                iyear += 2000;
                            imonth = month.ObjToInt32();
                            if (imonth <= 0 || imonth > 12)
                                imonth = 1;
                            str = imonth.ToString("D2") + "/" + iday.ToString("D2") + "/" + iyear.ToString("D4");
                            dt.Rows[i]["deceasedDate"] = str;
                        }
                        else
                        {
                            year = city.Substring(0, 2);
                            if (G1.validate_numeric(year))
                            {
                                if (iyear >= 35 && iyear <= 99)
                                    iyear += 1900;
                                else
                                    iyear += 2000;
                                iyear = year.ObjToInt32();
                                iday = 1;
                                city = city.Substring(2).Trim().ToUpper();
                                imonth = 1;
                                if (city == "JAN")
                                    imonth = 1;
                                else if (city == "FEB")
                                    imonth = 2;
                                else if (city == "MAR")
                                    imonth = 3;
                                else if (city == "APR")
                                    imonth = 4;
                                else if (city == "MAY")
                                    imonth = 5;
                                else if (city == "JUN")
                                    imonth = 6;
                                else if (city == "JUL")
                                    imonth = 7;
                                else if (city == "AUG")
                                    imonth = 8;
                                else if (city == "SEP")
                                    imonth = 9;
                                else if (city == "OCT")
                                    imonth = 10;
                                else if (city == "NOV")
                                    imonth = 11;
                                else if (city == "DEC")
                                    imonth = 12;
                                str = imonth.ToString("D2") + "/" + iday.ToString("D2") + "/" + iyear.ToString("D4");
                                dt.Rows[i]["deceasedDate"] = str;
                            }
                        }
                        //110113
                    }
                    else if (city.Length == 8)
                    {
                        if (G1.validate_numeric(city))
                        {
                            month = city.Substring(0, 2);
                            imonth = month.ObjToInt32();
                            if (imonth <= 0 || imonth > 12)
                                imonth = 1;
                            city = city.Substring(2);
                            str = city.Substring(0, 2);
                            iday = str.ObjToInt32();
                            if (iday <= 0)
                                iday = 1;
                            else if (iday > 31)
                                iday = 28;
                            city = city.Substring(2);
                            iyear = city.ObjToInt32();
                            str = imonth.ToString("D2") + "/" + iday.ToString("D2") + "/" + iyear.ToString("D4");
                            dt.Rows[i]["deceasedDate"] = str;
                        }
                        else if (city.IndexOf("/") > 0)
                        {
                            deceasedDate = city.ObjToDateTime();
                            if (deceasedDate.Year <= 1850)
                            {
                                dates = city.Split('/');
                                if (dates.Length >= 3)
                                {
                                    iyear = dates[0].ObjToInt32();
                                    if (iyear > 21)
                                        iyear = 1900 + iyear;
                                    else
                                        iyear = 2000 + iyear;
                                    imonth = dates[1].ObjToInt32();
                                    iday = dates[2].ObjToInt32();
                                    city = imonth.ToString("D2") + "/" + iday.ToString("D2") + "/" + iyear.ToString("D4");
                                }
                            }
                            else
                                city = deceasedDate.ToString("MM/dd/yyyy");
                            dt.Rows[i]["deceasedDate"] = city;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("***ERROR*** " + ex.Message.ToString());
                }
                deceasedDate = dt.Rows[i]["deceasedDate"].ObjToDateTime();
                if (deceasedDate.Year < 1850)
                    dt.Rows[i]["deceasedDate"] = "12/31/1910";
            }
        }
        /***********************************************************************************************/
        private void menuBankDeposits_Click(object sender, EventArgs e)
        {
            BankDeposits bankForm = new BankDeposits();
            bankForm.Show();
        }
        /***********************************************************************************************/
        private void mergeTwoSpreadsheetsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MergeData mergeForm = new MergeData();
            mergeForm.Show();
        }
        /***********************************************************************************************/
        private void mainSystemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            activeSystem = "Main";
            mainSystemToolStripMenuItem.Checked = true;
            rilesToolStripMenuItem.Checked = false;
            menuStrip1.BackColor = menuBackColor;
            G1.oldCopy = false;
        }
        /***********************************************************************************************/
        private void rilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            activeSystem = "Riles";
            mainSystemToolStripMenuItem.Checked = false;
            rilesToolStripMenuItem.Checked = true;
            menuStrip1.BackColor = Color.Pink;
            G1.oldCopy = false;
        }
        /***********************************************************************************************/
        public static DataTable FilterForRiles(DataTable dt)
        {
            if (G1.get_column_number(dt, "contractNumber") < 0)
                return dt;
            DataTable dx = null;
            try
            {
                dx = dt.Clone();
                DataRow[] dRows = null;
                string cmd = "contractNumber LIKE 'RF%'";
                if (activeSystem.ToUpper() == "MAIN")
                    cmd = "contractNumber NOT LIKE 'RF%'";
                else if ( String.IsNullOrWhiteSpace ( activeSystem))
                    cmd = "contractNumber NOT LIKE 'RF%'";
                dRows = dt.Select(cmd);
                G1.ConvertToTable(dRows, dx);
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Trying to Filter Riles Data!");
            }
            return dx;
        }
        /***********************************************************************************************/
        private void securityNationalMatchReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SecurityNational securityForm = new SecurityNational();
            securityForm.Show();
        }
        /***********************************************************************************************/
        private void securityNationalACHReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SecurityNationalACH securityForm = new SecurityNationalACH();
            securityForm.Show();
        }
        /***********************************************************************************************/
        private void importInsuranceLocationsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Import importForm = new Import("Insurance Payer Data");
            importForm.SelectDone += ImportForm_SelectDone6;
            importForm.Show();
        }
        /***********************************************************************************************/
        private void ImportForm_SelectDone6(DataTable dt)
        {
            barImport.Show();
            DataTable dx = null;
            string cmd = "";
            string record = "";
            string contractRecord = "";
            string payer = "";
            string contract = "";
            string firstName = "";
            string lastName = "";
            string address1 = "";
            string address2 = "";
            string address3 = "";
            string city = "";
            string state = "";
            string zip1 = "";

            string ssn = "";
            string agentCode = "";

            string deleteFlag = "";
            string areaCode = "";
            string phoneNumber = "";
            string lapsed = "";
            string dueDate8 = "";
            string lastDatePaid8 = "";
            string amtOfMonthlyPayt = "";
            double badAmount = 0D;
            double annualPremium = 0D;
            string dueNow = "";
            string deceasedDate = "";
            DateTime date = DateTime.Now;
            double creditBalance = 0D;

            Rectangle rect = this.Bounds;

            int height = rect.Height + 132 - 103;
            this.SetBounds(rect.Left, rect.Top, rect.Width, height);

            G1.CreateAudit("Payer Location Import");

            string workwhat = "";


            DateTime date1 = DateTime.Now;
            DateTime date2 = DateTime.Now;
            DateTime nullDeceasedDate = new DateTime(1, 1, 1);


            int plnameCol = G1.get_column_number(dt, "PAYER LAST NAME");
            if (plnameCol < 0)
                plnameCol = G1.get_column_number(dt, "PLNAME");

            int pfnameCol = G1.get_column_number(dt, "PAYER FIRST NAME");
            if (pfnameCol < 0)
                pfnameCol = G1.get_column_number(dt, "PFNAME");

            cmd = "Select COUNT(*) from `icustomers`;";
            dx = G1.get_db_data(cmd);
            int totalCustomers = dx.Rows[0][0].ObjToInt32();

            int lastRow = dt.Rows.Count;

            int tableRow = 0;
            //            lastrow = 10; // Just for Testing
            try
            {
                int created = 0;
                int possible = 0;
                int errors = 0;
                bool justCreated = false;
                int start = 0;
                string oldloc = "";
                //                lastrow = 1;
                //                start = 9000;
                lblTotal.Show();

                lblTotal.Text = "of " + lastRow.ToString();
                lblTotal.Refresh();

                barImport.Minimum = 0;
                barImport.Maximum = lastRow;
                labelMaximum.Show();

                for (int i = start; i < lastRow; i++)
                {
                    Application.DoEvents();
                    tableRow = i;
                    record = "";
                    contractRecord = "";

                    barImport.Value = i;
                    barImport.Refresh();
                    labelMaximum.Text = i.ToString();
                    labelMaximum.Refresh();

                    try
                    {
                        justCreated = false;
                        deceasedDate = "";

                        payer = dt.Rows[i]["PAYER#"].ObjToString();
                        if (String.IsNullOrWhiteSpace(payer))
                            continue;
                        payer = payer.TrimStart('0');
                        firstName = dt.Rows[i][pfnameCol].ObjToString();
                        lastName = dt.Rows[i][plnameCol].ObjToString();
                        firstName = G1.protect_data(firstName);
                        lastName = G1.protect_data(lastName);
                        oldloc = dt.Rows[i]["OLDLOC"].ObjToString();
                        if (String.IsNullOrWhiteSpace(oldloc))
                            continue;

                        cmd = "Select * from `icustomers` where `payer` = '" + payer + "' AND `lastName` = '" + lastName + "' and `firstName` = '" + firstName + "' ORDER BY `contractNumber` DESC;";
                        dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count <= 0)
                        {
                            G1.WriteAudit("No Payer Customer Row " + i.ToString() + " payer " + payer + " " + lastName + " " + firstName + "!");
                            continue;
                        }

                        record = dx.Rows[0]["record"].ObjToString();
                        contract = dx.Rows[0]["contractNumber"].ObjToString();

                        G1.update_db_table("icustomers", "record", record, new string[] { "oldloc", oldloc });

                        cmd = "Select * from `payers` where `contractNumber` = '" + contract + "';";
                        dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count <= 0)
                        {
                            //G1.WriteAudit("No Payer Row " + i.ToString() + " payer " + payer + " " + lastName + " " + firstName + "!");
                            continue;
                        }
                        record = dx.Rows[0]["record"].ObjToString();
                        G1.update_db_table("payers", "record", record, new string[] { "oldloc", oldloc });

                        if ((i % 500) == 0)
                            GC.Collect();
                    }
                    catch (Exception ex)
                    {
                        dt.Rows[i]["num"] = "*ERROR*";
                        errors++;
                    }
                    //                    picLoader.Refresh();
                }

                barImport.Value = lastRow;
                barImport.Refresh();
                labelMaximum.Text = lastRow.ToString();
                labelMaximum.Refresh();

                MessageBox.Show("Payer Data Import of " + lastRow + " Rows Complete Created = " + created.ToString() + " Errors = " + errors.ToString() + " Possible = " + possible.ToString() + " . . .");
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Creating Payer Record/Row! " + contract + "/" + tableRow.ToString() + " Stopping! " + ex.Message.ObjToString());
            }

            this.SetBounds(rect.Left, rect.Top, rect.Width, rect.Height);
        }
        /***********************************************************************************************/
        private void importHistoricPolicyPremiumsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Import importForm = new Import("Insurance Policy Premiums");
            importForm.SelectDone += ImportForm_SelectDone7;
            importForm.Show();
        }
        /***********************************************************************************************/
        private void ImportForm_SelectDone7(DataTable dt)
        {
            barImport.Show();
            DataTable dx = null;
            string cmd = "";
            string record = "";
            string contractRecord = "";
            string payer = "";
            string contract = "";
            string firstName = "";
            string lastName = "";
            string address1 = "";
            string address2 = "";
            string address3 = "";
            string city = "";
            string state = "";
            string zip1 = "";

            string ssn = "";
            string agentCode = "";

            string deleteFlag = "";
            string areaCode = "";
            string phoneNumber = "";
            string lapsed = "";
            string dueDate8 = "";
            string lastDatePaid8 = "";
            string amtOfMonthlyPayt = "";
            double badAmount = 0D;
            double annualPremium = 0D;
            string dueNow = "";
            string deceasedDate = "";
            DateTime date = DateTime.Now;
            double creditBalance = 0D;

            Rectangle rect = this.Bounds;

            int height = rect.Height + 132 - 103;
            this.SetBounds(rect.Left, rect.Top, rect.Width, height);

            G1.CreateAudit("Policy Historic Premium Import");

            string workwhat = "";


            DateTime date1 = DateTime.Now;
            DateTime date2 = DateTime.Now;
            DateTime nullDeceasedDate = new DateTime(1, 1, 1);


            int plnameCol = G1.get_column_number(dt, "PAYER LAST NAME");
            if (plnameCol < 0)
                plnameCol = G1.get_column_number(dt, "PLNAME");

            int pfnameCol = G1.get_column_number(dt, "PAYER FIRST NAME");
            if (pfnameCol < 0)
                pfnameCol = G1.get_column_number(dt, "PFNAME");

            cmd = "Select COUNT(*) from `icustomers`;";
            dx = G1.get_db_data(cmd);
            int totalCustomers = dx.Rows[0][0].ObjToInt32();

            int lastRow = dt.Rows.Count;

            int tableRow = 0;
            string policyNumber = "";
            string policyFirstName = "";
            string policyLastName = "";
            string premium = "";
            //            lastrow = 10; // Just for Testing
            try
            {
                int created = 0;
                int possible = 0;
                int errors = 0;
                bool justCreated = false;
                int start = 0;
                string oldloc = "";
                //                lastrow = 1;
                //                start = 9000;
                //lastRow = 1000;
                lblTotal.Show();

                lblTotal.Text = "of " + lastRow.ToString();
                lblTotal.Refresh();

                barImport.Minimum = 0;
                barImport.Maximum = lastRow;
                labelMaximum.Show();

                for (int i = start; i < lastRow; i++)
                {
                    Application.DoEvents();
                    tableRow = i;
                    record = "";
                    contractRecord = "";

                    barImport.Value = i;
                    barImport.Refresh();
                    labelMaximum.Text = i.ToString();
                    labelMaximum.Refresh();

                    try
                    {
                        policyNumber = dt.Rows[i]["INV#"].ObjToString();
                        policyNumber = policyNumber.TrimStart('0');
                        if (String.IsNullOrWhiteSpace(policyNumber))
                            continue;

                        payer = dt.Rows[i]["PAYER#"].ObjToString();
                        payer = payer.TrimStart('0');
                        if (String.IsNullOrWhiteSpace(payer))
                        {
                            dt.Rows[i]["BAD"] = "Y";
                            dt.Rows[i]["BADDETAIL"] = "EMPTY PAYER";
                            continue;
                        }

                        premium = dt.Rows[i]["prem"].ObjToString();
                        if (String.IsNullOrWhiteSpace(premium))
                            premium = "0.00";
                        else if (!G1.validate_numeric(premium))
                            premium = "0.00";

                        agentCode = dt.Rows[i]["agent"].ObjToString();

                        firstName = dt.Rows[i]["FNAME"].ObjToString();
                        lastName = dt.Rows[i]["LNAME"].ObjToString();
                        firstName = G1.protect_data(firstName);
                        lastName = G1.protect_data(lastName);

                        //cmd = "Select * from `icustomers` where `payer` = '" + payer + "' AND `lastName` = '" + lastName + "' and `firstName` = '" + firstName + "';";
                        //dx = G1.get_db_data(cmd);
                        //if ( dx.Rows.Count <= 0 )
                        //{
                        //    dt.Rows[i]["num"] = "*ERROR*";
                        //    errors++;
                        //    continue;
                        //}
                        //record = dx.Rows[0]["record"].ObjToString();
                        //contract = dx.Rows[0]["contractNumber"].ObjToString();

                        policyFirstName = dt.Rows[i]["pfname"].ObjToString();
                        policyFirstName = G1.protect_data(policyFirstName);
                        policyLastName = dt.Rows[i]["plname"].ObjToString();
                        policyLastName = G1.protect_data(policyLastName);

                        //                        cmd = "Select * from `policies` where `payer` = '" + payer + "' AND `policyNumber` = '" + policyNumber + "' AND `policyLastName` = '" + policyLastName + "' and `policyFirstName` = '" + policyFirstName + "';";
                        cmd = "Select * from `policies` where `policyNumber` = '" + policyNumber + "' AND `policyLastName` = '" + lastName + "' and `policyFirstName` = '" + firstName + "';";
                        dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count <= 0)
                        {
                            G1.WriteAudit("BAD POLICY - " + policyNumber + " Last Name " + lastName + " First Name " + firstName);
                            dt.Rows[i]["num"] = "*ERROR*";
                            errors++;
                            continue;
                        }

                        record = dx.Rows[0]["record"].ObjToString();
                        G1.update_db_table("policies", "record", record, new string[] { "historicPremium", premium, "agentCode", agentCode });

                        if ((i % 500) == 0)
                            GC.Collect();
                    }
                    catch (Exception ex)
                    {
                        dt.Rows[i]["num"] = "*ERROR*";
                        errors++;
                    }
                }

                barImport.Value = lastRow;
                barImport.Refresh();
                labelMaximum.Text = lastRow.ToString();
                labelMaximum.Refresh();

                MessageBox.Show("Policy Data Import of " + lastRow + " Rows Complete Created = " + created.ToString() + " Errors = " + errors.ToString() + " Possible = " + possible.ToString() + " . . .");
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Creating Policy Record/Row! " + contract + "/" + tableRow.ToString() + " Stopping! " + ex.Message.ObjToString());
            }

            this.SetBounds(rect.Left, rect.Top, rect.Width, rect.Height);
        }
        /***********************************************************************************************/
        private void fixImportInsurancePolicyActiveDeceasedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Import importForm = new Import("Fix Insurance Policy Active/Deceased");
            importForm.SelectDone += ImportForm_SelectDone8;
            importForm.Show();
        }
        /***********************************************************************************************/
        private void ImportForm_SelectDone8(DataTable dt)
        {
            barImport.Show();
            DataTable dx = null;
            string cmd = "";
            string record = "";
            string contractRecord = "";
            string payer = "";
            string contract = "";
            string firstName = "";
            string lastName = "";
            string address1 = "";
            string address2 = "";
            string address3 = "";
            string city = "";
            string state = "";
            string zip1 = "";

            string ssn = "";
            string agentCode = "";

            string deleteFlag = "";
            string areaCode = "";
            string phoneNumber = "";
            string lapsed = "";
            string dueDate8 = "";
            string lastDatePaid8 = "";
            string amtOfMonthlyPayt = "";
            double badAmount = 0D;
            double annualPremium = 0D;
            string dueNow = "";
            string deceasedDate = "";
            DateTime date = DateTime.Now;
            double creditBalance = 0D;

            Rectangle rect = this.Bounds;

            int height = rect.Height + 132 - 103;
            this.SetBounds(rect.Left, rect.Top, rect.Width, height);

            G1.CreateAudit("Fix Import Policy Active_Deceased");

            string workwhat = "";


            DateTime date1 = DateTime.Now;
            DateTime date2 = DateTime.Now;
            DateTime nullDeceasedDate = new DateTime(1, 1, 1);


            int plnameCol = G1.get_column_number(dt, "PAYER LAST NAME");
            if (plnameCol < 0)
                plnameCol = G1.get_column_number(dt, "PLNAME");

            int pfnameCol = G1.get_column_number(dt, "PAYER FIRST NAME");
            if (pfnameCol < 0)
                pfnameCol = G1.get_column_number(dt, "PFNAME");

            cmd = "Select COUNT(*) from `icustomers`;";
            dx = G1.get_db_data(cmd);
            int totalCustomers = dx.Rows[0][0].ObjToInt32();

            int lastRow = dt.Rows.Count;

            int tableRow = 0;
            string policyNumber = "";
            string policyFirstName = "";
            string policyLastName = "";
            string premium = "";
            //            lastrow = 10; // Just for Testing
            try
            {
                int created = 0;
                int possible = 0;
                int errors = 0;
                bool justCreated = false;
                int start = 0;
                string oldloc = "";
                //                lastrow = 1;
                //                start = 9000;
                //lastRow = 1000;
                lblTotal.Show();

                lblTotal.Text = "of " + lastRow.ToString();
                lblTotal.Refresh();

                barImport.Minimum = 0;
                barImport.Maximum = lastRow;
                labelMaximum.Show();

                for (int i = start; i < lastRow; i++)
                {
                    Application.DoEvents();
                    tableRow = i;
                    record = "";
                    contractRecord = "";

                    barImport.Value = i;
                    barImport.Refresh();
                    labelMaximum.Text = i.ToString();
                    labelMaximum.Refresh();

                    try
                    {
                        policyNumber = dt.Rows[i]["INV#"].ObjToString();
                        policyNumber = policyNumber.TrimStart('0');
                        if (String.IsNullOrWhiteSpace(policyNumber))
                            continue;

                        payer = dt.Rows[i]["PAYER#"].ObjToString();
                        payer = payer.TrimStart('0');
                        if (String.IsNullOrWhiteSpace(payer))
                        {
                            dt.Rows[i]["BAD"] = "Y";
                            dt.Rows[i]["BADDETAIL"] = "EMPTY PAYER";
                            continue;
                        }

                        firstName = dt.Rows[i]["FNAME"].ObjToString();
                        lastName = dt.Rows[i]["LNAME"].ObjToString();
                        firstName = G1.protect_data(firstName);
                        lastName = G1.protect_data(lastName);


                        policyFirstName = dt.Rows[i]["pfname"].ObjToString();
                        policyFirstName = G1.protect_data(policyFirstName);
                        policyLastName = dt.Rows[i]["plname"].ObjToString();
                        policyLastName = G1.protect_data(policyLastName);

                        cmd = "Select * from `policies` where `policyNumber` = '" + policyNumber + "' AND `policyLastName` = '" + lastName + "' and `policyFirstName` = '" + firstName + "';";
                        dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count <= 0)
                        {
                            G1.WriteAudit("BAD POLICY - " + policyNumber + " Last Name " + lastName + " First Name " + firstName);
                            dt.Rows[i]["num"] = "*ERROR*";
                            errors++;
                            continue;
                        }

                        record = dx.Rows[0]["record"].ObjToString();
                        G1.update_db_table("policies", "record", record, new string[] { "deceasedDate", "0000-00-00" });

                        if ((i % 500) == 0)
                            GC.Collect();
                    }
                    catch (Exception ex)
                    {
                        dt.Rows[i]["num"] = "*ERROR*";
                        errors++;
                    }
                }

                barImport.Value = lastRow;
                barImport.Refresh();
                labelMaximum.Text = lastRow.ToString();
                labelMaximum.Refresh();

                MessageBox.Show("Policy Data Import of " + lastRow + " Rows Complete Created = " + created.ToString() + " Errors = " + errors.ToString() + " Possible = " + possible.ToString() + " . . .");
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Creating Policy Record/Row! " + contract + "/" + tableRow.ToString() + " Stopping! " + ex.Message.ObjToString());
            }

            this.SetBounds(rect.Left, rect.Top, rect.Width, rect.Height);
        }
        /***********************************************************************************************/
        private void fixImportInsurancePayerDueDatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Import importForm = new Import("Fix Insurance Payer Due Dates");
            importForm.SelectDone += ImportForm_SelectDone9;
            importForm.Show();
        }
        /***********************************************************************************************/
        private void ImportForm_SelectDone9(DataTable dt)
        {
            barImport.Show();
            DataTable dx = null;
            string cmd = "";
            string record = "";
            string contractRecord = "";
            string payer = "";
            string contract = "";
            string firstName = "";
            string lastName = "";
            string address1 = "";
            string address2 = "";
            string address3 = "";
            string city = "";
            string state = "";
            string zip1 = "";

            string ssn = "";
            string agentCode = "";

            string deleteFlag = "";
            string areaCode = "";
            string phoneNumber = "";
            string lapsed = "";
            string dueDate8 = "";
            string lastDatePaid8 = "";
            string amtOfMonthlyPayt = "";
            double badAmount = 0D;
            double annualPremium = 0D;
            string dueNow = "";
            string deceasedDate = "";
            DateTime date = DateTime.Now;
            double creditBalance = 0D;

            Rectangle rect = this.Bounds;

            int height = rect.Height + 132 - 103;
            this.SetBounds(rect.Left, rect.Top, rect.Width, height);

            G1.CreateAudit("Fix Insurance Payer Due Dates");

            string workwhat = "";


            DateTime date1 = DateTime.Now;
            DateTime date2 = DateTime.Now;
            DateTime nullDeceasedDate = new DateTime(1, 1, 1);


            int plnameCol = G1.get_column_number(dt, "PAYER LAST NAME");
            if (plnameCol < 0)
                plnameCol = G1.get_column_number(dt, "PLNAME");

            int pfnameCol = G1.get_column_number(dt, "PAYER FIRST NAME");
            if (pfnameCol < 0)
                pfnameCol = G1.get_column_number(dt, "PFNAME");

            cmd = "Select COUNT(*) from `icustomers`;";
            dx = G1.get_db_data(cmd);
            int totalCustomers = dx.Rows[0][0].ObjToInt32();

            int lastRow = dt.Rows.Count;

            int tableRow = 0;
            //            lastrow = 10; // Just for Testing
            try
            {
                int j = 0;
                int created = 0;
                int possible = 0;
                int errors = 0;
                bool justCreated = false;
                int start = 0;
                string oldloc = "";
                //                lastrow = 1;
                //                start = 9000;
                lblTotal.Show();

                lblTotal.Text = "of " + lastRow.ToString();
                lblTotal.Refresh();

                barImport.Minimum = 0;
                barImport.Maximum = lastRow;
                labelMaximum.Show();

                for (int i = start; i < lastRow; i++)
                {
                    Application.DoEvents();
                    tableRow = i;
                    record = "";
                    contractRecord = "";

                    barImport.Value = i;
                    barImport.Refresh();
                    labelMaximum.Text = i.ToString();
                    labelMaximum.Refresh();

                    try
                    {
                        justCreated = false;
                        deceasedDate = "";

                        payer = dt.Rows[i]["PAYER#"].ObjToString();
                        if (String.IsNullOrWhiteSpace(payer))
                            continue;
                        payer = payer.TrimStart('0');
                        //if (payer != "BB-0392")
                        //    continue;
                        firstName = dt.Rows[i][pfnameCol].ObjToString();
                        lastName = dt.Rows[i][plnameCol].ObjToString();
                        firstName = G1.protect_data(firstName);
                        lastName = G1.protect_data(lastName);

                        date1 = dt.Rows[i]["due date"].ObjToDateTime();

                        cmd = "Select * from `icontracts` p JOIN `icustomers` c ON p.`contractNumber` = c.`contractNumber` where c.`payer` = '" + payer + "' GROUP by c.`contractNumber`;";
                        dx = G1.get_db_data(cmd);

                        for (j = 0; j < dx.Rows.Count; j++)
                        {
                            date2 = dx.Rows[j]["dueDate8"].ObjToDateTime();
                            if (date1 > date2)
                            {
                                record = dx.Rows[j]["record"].ObjToString();
                                G1.update_db_table("icontracts", "record", record, new string[] { "dueDate8", date1.ObjToDateTime().ToString("MM/dd/yyyy") });
                            }
                        }
                        cmd = "Select * from `payers` where `payer` = '" + payer + "' GROUP by `contractNumber`;";
                        dx = G1.get_db_data(cmd);

                        for (j = 0; j < dx.Rows.Count; j++)
                        {
                            date2 = dx.Rows[j]["dueDate8"].ObjToDateTime();
                            if (date1 > date2)
                            {
                                record = dx.Rows[j]["record"].ObjToString();
                                G1.update_db_table("payers", "record", record, new string[] { "dueDate8", date1.ObjToDateTime().ToString("MM/dd/yyyy") });
                            }
                        }

                        if ((i % 500) == 0)
                            GC.Collect();
                    }
                    catch (Exception ex)
                    {
                        dt.Rows[i]["num"] = "*ERROR*";
                        errors++;
                    }
                    //                    picLoader.Refresh();
                }

                barImport.Value = lastRow;
                barImport.Refresh();
                labelMaximum.Text = lastRow.ToString();
                labelMaximum.Refresh();

                MessageBox.Show("Payer Data Import of " + lastRow + " Rows Complete Created = " + created.ToString() + " Errors = " + errors.ToString() + " Possible = " + possible.ToString() + " . . .");
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Creating Payer Record/Row! " + contract + "/" + tableRow.ToString() + " Stopping! " + ex.Message.ObjToString());
            }

            this.SetBounds(rect.Left, rect.Top, rect.Width, rect.Height);
        }
        /***********************************************************************************************/
        private void runACHReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GenerateACH achForm = new GenerateACH("The First");
            achForm.Show();
        }
        /***********************************************************************************************/
        private void importSMFSDrafts_Click(object sender, EventArgs e)
        {
            DataTable dt = new DataTable();
            this.Cursor = Cursors.WaitCursor;
            ImportDailyDeposits dailyForm = new ImportDailyDeposits(dt, "SMFS Drafts");
            dailyForm.Show();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void importExistingACHCustomerSetupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImportACHData achForm = new ImportACHData();
            achForm.Show();

            //Import importForm = new Import("Import Existing ACH Customer Setup");
            //importForm.SelectDone += ImportForm_SelectDone10;
            //importForm.Show();
        }
        /***********************************************************************************************/
        private void ImportForm_SelectDone10(DataTable dt)
        {
            if (1 == 1)
            {
                ImportACHData achForm = new ImportACHData();
                achForm.Show();
                return;
            }
            string origin = "";
            string str = "";
            string[] Lines = null;
            DateTime docp = DateTime.Now;
            string dom = "";
            double payment = 0D;
            string cnum = "";
            string originalPayer = "";
            string payer = "";
            string status = "";
            string code = "";
            string paymentsFile = "";
            bool insurance = false;
            string record = "";
            DataTable dxx = null;
            double expected = 0D;
            string routing = "";
            string checking = "";
            string type = "";
            string sPayment = "";
            string date = "";
            int created = 0;
            int updated = 0;
            int failed = 0;
            int frequency = 1;
            string originalCNum = "";
            int lastRow = dt.Rows.Count;
            //lastRow = 3;
            for (int i = 0; i < lastRow; i++)
            {
                try
                {
                    frequency = 1;
                    str = dt.Rows[i]["COL 11"].ObjToString().ToUpper();
                    if (str == "ONCE A YEAR")
                        frequency = 12;
                    type = dt.Rows[i]["type"].ObjToString();
                    if (type.ToUpper() != "CHECKING" && type.ToUpper() != "SAVINGS")
                        type = "Checking";
                    str = dt.Rows[i]["Start Date"].ObjToString();
                    Lines = str.Split(' ');
                    str = Lines[0].Trim();
                    docp = str.ObjToDateTime();
                    date = docp.ToString("MM/dd/yyyy");
                    dom = docp.Day.ObjToString();

                    str = dt.Rows[i]["Amount"].ObjToString();
                    payment = 0D;
                    if (G1.validate_numeric(str))
                        payment = str.ObjToDouble();
                    sPayment = G1.ReformatMoney(payment);

                    routing = dt.Rows[i]["Routing"].ObjToString();
                    checking = dt.Rows[i]["Account Number"].ObjToString();

                    cnum = dt.Rows[i]["Customer Number"].ObjToString();
                    originalCNum = cnum;

                    if (String.IsNullOrWhiteSpace(cnum))
                        cnum = "EMPTY #";

                    code = "01";
                    payer = "";

                    paymentsFile = "payments";
                    insurance = false;

                    originalPayer = cnum;

                    cnum = cnum.TrimStart('0');
                    cnum = cnum.Replace(" ", "");

                    if (String.IsNullOrWhiteSpace(cnum))
                    {
                        MessageBox.Show("***ERROR*** Cannot locate anyone for Number " + originalCNum + "!");
                        failed++;
                        continue;
                    }

                    expected = 0D;

                    string cmd = "Select * from `contracts` where `contractNumber` = '" + cnum + "';";
                    dxx = G1.get_db_data(cmd);
                    if (dxx.Rows.Count <= 0)
                    {
                        payer = cnum;
                        paymentsFile = "ipayments";
                        string newPayer = "";
                        bool isLapsed = false;
                        cnum = ImportDailyDeposits.FindPayerContract(payer, payment.ObjToString(), ref newPayer, ref expected, ref isLapsed);
                        if (!String.IsNullOrWhiteSpace(newPayer))
                            payer = newPayer;
                        if (String.IsNullOrWhiteSpace(cnum))
                        {
                            MessageBox.Show("***ERROR*** Cannot locate anyone for Number " + originalCNum + "!");
                            failed++;
                            continue;
                        }
                        insurance = true;
                        code = "02";
                    }
                    cmd = "Select * from `ach` where `contractNumber` = '" + cnum + "';";
                    if (insurance)
                        cmd = "Select * from `ach` where `payer` = '" + cnum + "';";
                    dxx = G1.get_db_data(cmd);
                    if (dxx.Rows.Count > 0)
                    {
                        record = dxx.Rows[0]["record"].ObjToString();
                        updated++;
                    }
                    else
                    {
                        record = G1.create_record("ach", "contractNumber", "-1");
                        created++;
                    }
                    if (G1.BadRecord("ach", record))
                    {
                        MessageBox.Show("***ERROR*** Creating ACH Record for Number " + originalCNum + "!");
                        failed++;
                        continue;
                    }

                    G1.update_db_table("ach", "record", record, new string[] { "contractNumber", cnum, "payer", payer, "code", code, "dayOfMonth", dom, "frequencyInMonths", "1", "routingNumber", routing, "accountNumber", checking, "acctType", type, "payment", sPayment });
                }
                catch (Exception ex)
                {
                    MessageBox.Show("***ERROR*** Creating ACH Record for Number " + originalCNum + "!");
                    failed++;
                }
            }
            MessageBox.Show("***INFO*** " + created.ToString() + " Created and " + updated.ToString() + " Updated and " + failed.ToString() + " Failed Customers", "Update ACH Customers Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        /***********************************************************************************************/
        private void ImportForm_SelectDone10OLD(DataTable dt)
        {
            string origin = "";
            string str = "";
            string[] Lines = null;
            DateTime docp = DateTime.Now;
            string dom = "";
            double payment = 0D;
            string cnum = "";
            string originalPayer = "";
            string payer = "";
            string status = "";
            string code = "";
            string paymentsFile = "";
            bool insurance = false;
            string record = "";
            DataTable dxx = null;
            double expected = 0D;
            string date = "";
            int created = 0;
            int updated = 0;
            int failed = 0;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    origin = dt.Rows[i]["Payment Origin"].ObjToString();
                    if (origin.ToUpper() != "ORIGINAL SIGNATURE")
                        continue;

                    str = dt.Rows[i]["Transaction Date"].ObjToString();
                    Lines = str.Split(' ');
                    str = Lines[0].Trim();
                    docp = str.ObjToDateTime();
                    date = docp.ToString("MM/dd/yyyy");
                    dom = docp.Day.ObjToString();

                    str = dt.Rows[i]["amount"].ObjToString();
                    payment = 0D;
                    if (G1.validate_numeric(str))
                        payment = str.ObjToDouble();

                    cnum = dt.Rows[i]["Customer Number"].ObjToString();

                    if (String.IsNullOrWhiteSpace(cnum))
                        cnum = "EMPTY #";

                    status = dt.Rows[i]["status"].ObjToString();
                    code = "01";
                    payer = "";

                    paymentsFile = "payments";
                    insurance = false;

                    originalPayer = cnum;

                    cnum = cnum.TrimStart('0');
                    cnum = cnum.Replace("NEW", "");
                    cnum = cnum.ToUpper().Replace("INSURANCE", "").Trim();
                    cnum = cnum.Replace(" ", "");

                    if (String.IsNullOrWhiteSpace(cnum))
                        continue;

                    expected = 0D;

                    string cmd = "Select * from `contracts` where `contractNumber` = '" + cnum + "';";
                    dxx = G1.get_db_data(cmd);
                    if (dxx.Rows.Count <= 0)
                    {
                        payer = cnum;
                        paymentsFile = "ipayments";
                        string newPayer = "";
                        bool isLapsed = false;
                        cnum = ImportDailyDeposits.FindPayerContract(payer, payment.ObjToString(), ref newPayer, ref expected, ref isLapsed);
                        if (!String.IsNullOrWhiteSpace(newPayer))
                            payer = newPayer;
                        if (String.IsNullOrWhiteSpace(cnum))
                            continue;
                        insurance = true;
                        code = "02";
                    }
                    if (status.Trim().ToUpper() == "PROCESSED")
                    {
                        cmd = "Select * from `ach` where `contractNumber` = '" + cnum + "';";
                        if (insurance)
                            cmd = "Select * from `ach` where `payer` = '" + cnum + "';";
                        dxx = G1.get_db_data(cmd);
                        if (dxx.Rows.Count > 0)
                        {
                            record = dxx.Rows[0]["record"].ObjToString();
                            updated++;
                        }
                        else
                        {
                            record = G1.create_record("ach", "contractNumber", "-1");
                            created++;
                        }
                        if (G1.BadRecord("ach", record))
                            continue;

                        G1.update_db_table("ach", "record", record, new string[] { "contractNumber", cnum, "payer", payer, "code", code, "dayOfMonth", dom, "frequencyInMonths", "1", "routingNumber", "New Route", "accountNumber", "New Account" });
                    }
                }
                catch (Exception ex)
                {
                }
            }
            MessageBox.Show("***INFO*** " + created.ToString() + " Created and " + updated.ToString() + " Updated and " + failed.ToString() + " Failed Customers", "Update ACH Customers Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        /***********************************************************************************************/
        private void validateServicesAndBeginningBalancesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ValidateServices ValidForm = new ValidateServices();
            ValidForm.Show();
        }
        /***********************************************************************************************/
        private void importInsurancePaymentsToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            Import importForm = new Import("Import Final Insurance Payments File");
            importForm.SelectDone += ImportForm_SelectDone11;
            importForm.Show();
        }
        /***********************************************************************************************/
        private void ImportForm_SelectDone11(DataTable dt)
        {
            barImport.Show();

            G1.CreateAudit("Import Insurance Payments");
            G1.WriteAudit("New Version");


            DataTable dx = null;

            DataView tempview = dt.DefaultView;
            tempview.Sort = "Payer#";
            dt = tempview.ToTable();


            string cmd = "";
            string record = "";
            string oldPayer = "";
            string payer = "";
            string contract = "";
            string oldContract = "";
            string firstName = "";
            string lastName = "";

            string policyFirstName = "";
            string policyLastName = "";
            string policyNumber = "";

            string amountKeyed = "";
            string payment = "";
            string debit = "";
            string credit = "";
            string totalMonths = "";
            string transactionCode = "";
            string batchNumber = "";
            string agent = "";
            string depositNumber = "";
            string user = "";
            string dateKeyed = "";

            Rectangle rect = this.Bounds;

            int height = rect.Height + 132 - 103;
            this.SetBounds(rect.Left, rect.Top, rect.Width, height);

            labelMaximum.Show();
            labelMaximum.Text = "0";
            barImport.Show();

            int lastrow = dt.Rows.Count;

            int tableRow = 0;
            //            lastrow = 1; // Just for Testing
            try
            {
                lblTotal.Show();
                barImport.Minimum = 0;
                barImport.Maximum = lastrow;
                lblTotal.Text = "of " + lastrow.ToString();
                lblTotal.Refresh();
                int created = 0;
                int errors = 0;
                int majorError = 0;
                int start = 0;
                oldPayer = "";
                oldContract = "";
                //                start = 200771; // Don't forget to reset this

                for (int i = start; i < lastrow; i++)
                {
                    Application.DoEvents();

                    barImport.Value = i;
                    barImport.Refresh();
                    labelMaximum.Text = i.ToString();
                    labelMaximum.Refresh();

                    tableRow = i;
                    record = "";
                    try
                    {
                        payer = dt.Rows[i]["PAYER#"].ObjToString();
                        payer = payer.TrimStart('0');
                        if (String.IsNullOrWhiteSpace(payer))
                            continue;
                        //if (payer.Trim() != "CC-843")
                        //    continue;
                        firstName = dt.Rows[i]["PAYER FIRST NAME"].ObjToString();
                        lastName = dt.Rows[i]["PAYER LAST NAME"].ObjToString();
                        firstName = G1.protect_data(firstName);
                        lastName = G1.protect_data(lastName);

                        if (payer != oldPayer)
                        {
                            cmd = "Select * from `payers` where `payer` = '" + payer + "';";
                            dx = G1.get_db_data(cmd);
                            if (dx.Rows.Count <= 0)
                            {
                                //                                cmd = "Select * from `icustomers` where `payer` = '" + payer + "' AND `lastName` = '" + lastName + "' and `firstName` = '" + firstName + "';";
                                cmd = "Select * from `icustomers` where `payer` = '" + payer + "';";
                                dx = G1.get_db_data(cmd);
                                if (dx.Rows.Count > 0)
                                {
                                    for (int k = 0; k < dx.Rows.Count; k++)
                                    {
                                        record = dx.Rows[k]["record"].ObjToString();
                                        contract = dx.Rows[k]["contractNumber"].ObjToString();
                                        if (contract.ToUpper().IndexOf("ZZ") >= 0)
                                            break;
                                    }
                                }
                                else
                                {
                                    dt.Rows[i]["num"] = "*ERROR*";
                                    errors++;
                                    contract = Import.CreateNewContract(payer, lastName, firstName, i);
                                    if (String.IsNullOrWhiteSpace(contract))
                                    {
                                        majorError++;
                                        continue;
                                    }
                                }
                            }
                            else
                                contract = dx.Rows[0]["contractNumber"].ObjToString();
                            oldPayer = payer;
                            oldContract = contract;
                        }

                        batchNumber = dt.Rows[i]["batch number"].ObjToString();
                        transactionCode = dt.Rows[i]["transaction code"].ObjToString().Trim();
                        amountKeyed = dt.Rows[i]["amount keyed"].ObjToString();
                        agent = dt.Rows[i]["agent at time keyed"].ObjToString();
                        totalMonths = dt.Rows[i]["total months"].ObjToString();
                        depositNumber = dt.Rows[i]["deposit number"].ObjToString();
                        user = dt.Rows[i]["user"].ObjToString();
                        dateKeyed = dt.Rows[i]["date keyed"].ObjToString();
                        if (!G1.validate_numeric(dateKeyed))
                            dateKeyed = "2019-11-29";

                        record = G1.create_record("ipayments", "contractNumber", "-1");
                        if (G1.BadRecord("ipayments", record))
                            continue;
                        created++;
                        //                        }
                        payment = "";
                        debit = "";
                        credit = "";
                        if (transactionCode == "01")
                            payment = amountKeyed;
                        else if (transactionCode == "1")
                            payment = amountKeyed;
                        else if (transactionCode == "98")
                            debit = amountKeyed;
                        else if (transactionCode == "99")
                            credit = amountKeyed;

                        G1.update_db_table("ipayments", "record", record, new string[] { "contractNumber", contract, "payDate8", dateKeyed, "agentNumber", agent, "paymentAmount", payment, "debitAdjustment", debit, "creditAdjustment", credit, "userId", user, "depositNumber", depositNumber, "new", batchNumber, "firstName", firstName, "lastName", lastName, "numMonthPaid", totalMonths, "payer", payer });
                        if ((i % 500) == 0)
                            GC.Collect();
                    }
                    catch (Exception ex)
                    {
                        dt.Rows[i]["num"] = "*ERROR*";
                    }
                }
                barImport.Value = lastrow;

                MessageBox.Show("Payment Data Import of " + lastrow + " Rows Complete Created = " + created.ToString() + " Errors = " + errors.ToString() + " . . .");
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Creating Payment Record/Row! " + contract + "/" + tableRow.ToString() + " Stopping! " + ex.Message.ObjToString());
            }
            this.SetBounds(rect.Left, rect.Top, rect.Width, rect.Height);
        }
        /***********************************************************************************************/
        private void listACHCustomersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GenerateACH achForm = new GenerateACH("", true, true);
            achForm.Show();
        }
        /***********************************************************************************************/
        private void fixInsuranceMonthlyPaymentsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string cmd = "Select * from `icontracts` i JOIN `icustomers` c ON i.`contractNumber` = c.`contractNumber` WHERE `amtOfMonthlyPayt` > '500';";
            DataTable dt = G1.get_db_data(cmd);
            string payer = "";
            double payment = 0D;
            string record = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                payer = dt.Rows[i]["payer"].ObjToString();
                if (String.IsNullOrWhiteSpace(payer))
                    continue;
                payment = Policies.CalcMonthlyPremium(payer, DateTime.Now);
                record = dt.Rows[i]["record"].ObjToString();
                G1.update_db_table("icontracts", "record", record, new string[] { "amtOfMonthlyPayt", payment.ToString() });
            }
        }
        /***********************************************************************************************/
        private void fixInsuranceACHLocationsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string cmd = "SELECT * FROM `ipayments` WHERE `payDate8` >= '2020-07-01' AND `depositNumber` LIKE 'A%';";
            DataTable dt = G1.get_db_data(cmd);
            string payer = "";
            double payment = 0D;
            string record = "";
            string location = "";
            string oldloc = "";
            DataTable dx = null;
            string contractNumber = "";

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                cmd = "Select * from `icustomers` where `contractNumber` = '" + contractNumber + "';";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    payer = dx.Rows[0]["payer"].ObjToString();
                    if (String.IsNullOrWhiteSpace(payer))
                        continue;
                    location = ImportDailyDeposits.FindLastPaymentLocation(payer, ref oldloc);
                    record = dt.Rows[i]["record"].ObjToString();
                    G1.update_db_table("ipayments", "record", record, new string[] { "location", location });
                }
            }
        }
        /***********************************************************************************************/
        private void menuShowCharlotteDiff_Click(object sender, EventArgs e)
        {
            TrustDiff trustForm = new TrustDiff();
            trustForm.Show();
        }
        /***********************************************************************************************/
        private void generateACHPaymentsForBankPlusToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GenerateACH achForm = new GenerateACH("Bank Plus");
            achForm.Show();
        }
        /***********************************************************************************************/
        private void toolStripMenuItem20_Click(object sender, EventArgs e)
        {
            DataTable dt = new DataTable();
            this.Cursor = Cursors.WaitCursor;
            ImportDailyDeposits dailyForm = new ImportDailyDeposits(dt, false, true);
            dailyForm.Show();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void rilesImportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImportRiles rilesForm = new ImportRiles();
            rilesForm.Show();
        }
        /***********************************************************************************************/
        private void SMFS_FormClosing(object sender, FormClosingEventArgs e)
        {

            //double myMen = G1.get_used_memory();
            //string str = G1.ReformatMoney(myMen);
            //str = str.Replace(".00", "");
            //G1.WriteAudit("Ending Memory=" + str);
        }
        /***********************************************************************************************/
        private void matchBankDepositsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BankMatch matchForm = new BankMatch();
            matchForm.Show();
        }
        /***********************************************************************************************/
        private void pDFSharpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            G1.ReadMyPDF();
        }
        /***********************************************************************************************/
        private void editBankAccountsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            EditBankAccounts bankForm = new EditBankAccounts();
            bankForm.Show();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void editAllACHCustomersToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            GenerateACH achForm = new GenerateACH("", true);
            achForm.Show();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void editServicesToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            Services serviceForm = new Services(false);
            serviceForm.Show();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void editCasketMasterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            Caskets casketForm = new Caskets(false);
            casketForm.Show();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void editPricesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            PriceLists priceForm = new PriceLists();
            priceForm.Show();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void editFuneralHomeGroupsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            FuneralGroups funForm = new FuneralGroups();
            funForm.Show();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void editCemeteriesToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            Cemeteries cemeForm = new Cemeteries();
            cemeForm.Show();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void editAutoRunMasterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            AutoRunSetup autoForm = new AutoRunSetup();
            autoForm.Show();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void editArrangementFormsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            Arrangements arrangeform = new Arrangements();
            arrangeform.Show();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void editExtraFuneralLayoutInformationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            CustomerExtra extraForm = new CustomerExtra();
            extraForm.Show();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void editGeneralReferenceTablesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            EditTables editForm = new EditTables();
            editForm.Show();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void editDirectorsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            EditDirectors directorForm = new EditDirectors();
            directorForm.Show();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void editCrematoryOperatorMenu_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            EditCrematoryOperators operatorsForm = new EditCrematoryOperators();
            operatorsForm.Show();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void editFuneralTranslatorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditTranslator editTranslator = new EditTranslator();
            editTranslator.Show();
        }
        /***********************************************************************************************/
        private void matchInventoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MatchInventory matchForm = new MatchInventory();
            matchForm.Show();
        }
        /***********************************************************************************************/
        private void checkBadServiceIDsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BadServiceIds badForm = new BadServiceIds();
            badForm.Show();
        }
        /***********************************************************************************************/
        private void aRReportsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            arReport reportForm = new arReport();
            reportForm.Show();
        }
        /***********************************************************************************************/
        private void menuTDP_Click(object sender, EventArgs e)
        {
            BankTDPReport bankForm = new BankTDPReport();
            bankForm.Show();
        }
        /***********************************************************************************************/
        public class MessageFilter : IMessageFilter
        {
            private enum WindowsMessages : int
            {
                WM_KEYDOWN = 0x0100,
                WM_MOUSEMOVE = 0x0200,
                WM_SYSCOMMAND = 0x0112,
                SC_MONITORPOWER = 0xf170,
                //WM_WTSSESSION_CHANGE = 0x02B1,
                WTS_SESSION_LOCK = 0x07,
                WTS_SESSION_UNLOCK = 0x08
            }
            private static string userRoot = "HKEY_CURRENT_USER";
            private static string subkey = @"Software\Microsoft\Windows\CurrentVersion\Policies\System";
            private string keyName = userRoot + "\\" + subkey;
            private string valueName = "DisableLockWorkstation";
            private bool gotCTRL = false;
            private bool gotALT = false;
            private bool gotDEL = false;
            public bool PreFilterMessage(ref Message objMessage)
            {
                if (objMessage.Msg == Convert.ToInt32(WindowsMessages.WM_KEYDOWN))
                {
                    Keys key = (Keys)objMessage.WParam;

                    if (captureText)
                    {
                        string newkey = new System.Windows.Forms.KeysConverter().ConvertToString(key).ToUpper();
                        if ( key >= Keys.A && key <= Keys.Z)
                        {
                            capturedText += key;
                        }
                        else if ( key == Keys.Space )
                        {
                            capturedText += " ";
                        }
                        else if (key == Keys.Oemcomma )
                        {
                            capturedText += ",";
                        }
                    }
                    if (key == Keys.LWin)
                    {
                        try
                        {
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                    else if (key == Keys.ControlKey)
                    {
                        gotCTRL = true;
                    }
                    else if (key == Keys.Delete)
                    {
                        if (gotCTRL)
                        {
                        }
                    }
                    else
                    {
                        gotCTRL = false;
                        gotALT = false;
                        gotDEL = false;
                    }
                }
                if (objMessage.Msg == Convert.ToInt32(WindowsMessages.WM_KEYDOWN) ||
                    objMessage.Msg == Convert.ToInt32(WindowsMessages.WM_MOUSEMOVE))
                {
                    if (pleaseWaitForm != null)
                    {
                        pleaseWaitForm.Close();
                        pleaseWaitForm = null;
                    }
                    lastTimer = DateTime.Now;
                }
                return false;
            }
        }
        /***********************************************************************************************/
        private void editEffectiveDatesMenu_Click(object sender, EventArgs e)
        {
            EditEffectiveDates datesForm = new EditEffectiveDates();
            datesForm.Show();
        }
        /***********************************************************************************************/
        private void menuArrangers_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            EditArrangers arrangerForm = new EditArrangers();
            arrangerForm.Show();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void yearEndCasketUsageImportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Import importForm = new Import();
            importForm.SelectDone += ImportForm_SelectDone5;
            importForm.Show();
        }
        /***********************************************************************************************/
        private void ImportForm_SelectDone5(DataTable dt)
        {
            if (dt == null)
                return;
            DateTime date = DateTime.Now;
            string serialNumber = "";
            string location = "";
            string casket = "";
            string amount = "";
            string serviceId = "";
            string ownership = "";
            string count = "";
            string record = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    date = dt.Rows[i]["date"].ObjToDateTime();
                    if (date.Year <= 100)
                        continue;
                    location = dt.Rows[i]["location"].ObjToString();
                    casket = dt.Rows[i]["casket"].ObjToString();
                    serialNumber = dt.Rows[i]["Serial Number"].ObjToString();
                    amount = dt.Rows[i]["amount"].ObjToString();
                    amount = amount.Replace(",", "");
                    serviceId = dt.Rows[i]["Funeral No Used"].ObjToString();
                    ownership = dt.Rows[i]["ownership"].ObjToString();
                    count = dt.Rows[i]["count"].ObjToString();

                    record = G1.create_record("invoices", "location", "-1");
                    if (G1.BadRecord("invoices", record))
                        continue;
                    G1.update_db_table("invoices", "record", record, new string[] { "date", date.ToString("MM/dd/yyyy"), "location", location, "casket", casket, "SerialNumber", serialNumber, "amount", amount, "ServiceId", serviceId, "Ownership", ownership, "count", count });
                }
                catch (Exception ex)
                {
                }
            }
        }
        /***********************************************************************************************/
        private void checkBadCharactersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog())
            {
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string file = ofd.FileName;
                    try
                    {
                        string line = "";
                        int row = 0;
                        string str = "";
                        FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        using (StreamReader sr = new StreamReader(fs))

                        {
                            while ((line = sr.ReadLine()) != null)
                            {
                                Application.DoEvents();
                                row++;
                                if (row == 6223)
                                {
                                    string auditFile = "c:/rag/fix_sql" + DateTime.Now.ToString("yyyyMMdd_hhmmss") + ".sql";
                                    string filename = auditFile;
                                    if (File.Exists(filename))
                                        File.Delete(filename);
                                    StreamWriter sw = File.CreateText(filename);
                                    sw.WriteLine(line);
                                    sw.Close();
                                    break;
                                }
                            }
                            sr.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        DevExpress.XtraEditors.XtraMessageBox.Show(ex.Message, "Error Occurred");
                    }
                }
            }
        }
        /***********************************************************************************************/
        private void menuFixDueDates_Click(object sender, EventArgs e)
        {
            FixFDLICDates fixForm = new FixFDLICDates();
            fixForm.Show();
        }
        /***********************************************************************************************/
        private void compareCashRemittedToTrustReportExcelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CashToTrustReport cashForm = new CashToTrustReport();
            cashForm.Show();
        }
        /***********************************************************************************************/
        private void toolStripMenuItem26_Click(object sender, EventArgs e)
        {
            FixAS400Debits fixForm = new FixAS400Debits();
            fixForm.Show();
        }
        /***********************************************************************************************/
        private void trustReportsSetupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Import importForm = new Import();
            importForm.SelectDone += ImportForm_TrustImportDone;
            importForm.Show();
        }
        /****************************************************************************************/
        private void ImportForm_TrustImportDone(DataTable dt)
        {
            string report = "";
            string locations = "";
            string format = "";
            string filename = "";
            string saveDirectory = "";
            string holdReport = "";
            string record = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    report = dt.Rows[i]["Report"].ObjToString();
                    if (String.IsNullOrWhiteSpace(report))
                        continue;
                    locations = dt.Rows[i]["Locations To Include"].ObjToString();
                    if (String.IsNullOrWhiteSpace(locations))
                    {
                        holdReport = report;
                        continue;
                    }
                    format = dt.Rows[i]["format"].ObjToString();
                    filename = dt.Rows[i]["File Name"].ObjToString();
                    saveDirectory = dt.Rows[i]["Directory To Save"].ObjToString();
                    saveDirectory = saveDirectory.Replace("\\", "/");
                    record = G1.create_record("mass_reports", "format", "xxx");
                    if (G1.BadRecord("mass_reports", record))
                        break;
                    G1.update_db_table("mass_reports", "record", record, new string[] { "mainReport", holdReport, "report", report, "locations", locations, "format", format, "outputFilename", filename, "outputDirectory", saveDirectory, "options", "" });
                }
                catch (Exception ex)
                {
                    MessageBox.Show("***ERROR*** " + ex.Message.ToString());
                }
            }
        }
        /***********************************************************************************************/
        private void formButtonsMenu_Click(object sender, EventArgs e)
        {
            if (!LoginForm.administrator)
            {
                MessageBox.Show("*** Sorry *** You do not have permission to run this module!", "Edit Buttons Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }
            EditFormButtons formButtons = new EditFormButtons();
            formButtons.Show();
        }
        /***********************************************************************************************/
        private void toolMenuTitlesScriptures_Click(object sender, EventArgs e)
        {
            EditTitleScripture titleForm = new EditTitleScripture();
            titleForm.Show();
        }
        /***********************************************************************************************/
        private void importFuneralBCSACHPaymentsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Import importForm = new Import("Funeral BCS ACH Payments", false);
            importForm.SelectDone += ImportForm_SelectDone12;
            importForm.Show();
        }
        /***********************************************************************************************/
        private void ImportForm_SelectDone12(DataTable dt)
        {
            if (dt == null)
                return;
            if (dt.Rows.Count <= 0)
                return;

            DataTable dx = new DataTable();
            dx.Columns.Add("Status");
            dx.Columns.Add("receivedFrom");
            dx.Columns.Add("customer");
            dx.Columns.Add("contractNumber");
            dx.Columns.Add("principal");
            dx.Columns.Add("income");
            dx.Columns.Add("total");
            dx.Columns.Add("comment");

            DataRow dRow = null;

            DateTime date = DateTime.Now;
            DateTime depositDate = DateTime.MinValue;

            bool gotFirst = false;
            bool gotDate = false;
            bool gotData = false;

            string receivedFrom = "";
            string customer = "";
            string contractNumber = "";
            string principal = "";
            string income = "";
            string total = "";
            string comment = "";

            string str = "";
            DataTable ddt = null;
            DataTable ddx = null;

            string localDescription = "BCS - Trust Death Claims";

            string cmd = "Select * from `bank_accounts` where `localDescription` = '" + localDescription + "';";
            ddt = G1.get_db_data(cmd);
            if (ddt.Rows.Count <= 0)
            {
                MessageBox.Show("*** ERROR *** Cannot locate BCS - Trust Death Claims Bank Account", "Bank Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }


            string bankAccount = ddt.Rows[0]["account_no"].ObjToString();

            bool gotError = false;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    str = dt.Rows[i][1].ObjToString();
                    if (String.IsNullOrWhiteSpace(str))
                        continue;
                    if (!gotFirst)
                    {
                        if (str.ToUpper().IndexOf("ACCT#") >= 0)
                        {
                            gotFirst = true;
                            continue;
                        }
                    }
                    if (!gotDate)
                    {
                        date = str.ObjToDateTime();
                        if (date.Year >= 100)
                        {
                            gotDate = true;
                            depositDate = date;
                        }
                        continue;
                    }
                    if (!gotData)
                    {
                        if (str.ToUpper() == "RECEIVED FROM")
                            gotData = true;
                        continue;
                    }
                    receivedFrom = str;
                    customer = dt.Rows[i][2].ObjToString();
                    contractNumber = dt.Rows[i][3].ObjToString();
                    principal = dt.Rows[i][4].ObjToString();
                    income = dt.Rows[i][5].ObjToString();
                    total = dt.Rows[i][6].ObjToString();
                    comment = dt.Rows[i][7].ObjToString();

                    total = total.Replace("$", "");
                    total = total.Replace(",", "");

                    income = income.Replace("$", "");
                    income = income.Replace(",", "");

                    principal = principal.Replace("$", "");
                    principal = principal.Replace(",", "");

                    dRow = dx.NewRow();
                    dRow["receivedFrom"] = receivedFrom;
                    dRow["customer"] = customer;
                    dRow["contractNumber"] = contractNumber;
                    dRow["principal"] = principal;
                    dRow["income"] = income;
                    dRow["total"] = total;
                    dRow["comment"] = comment;
                    dx.Rows.Add(dRow);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("*** ERROR *** Pulling in Row (" + (i + 1).ToString() + "!\n" + ex.Message.ToString(), "Import Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    gotError = true;
                }
            }

            dx.AcceptChanges();
            if (!gotError)
            {
                ShowBCSImport bcsForm = new ShowBCSImport(dx, depositDate);
                bcsForm.Show();
            }
        }
        /***********************************************************************************************/
        private void trustAndInsuranceToolStripMenuItem_Click(object sender, EventArgs e)
        { // Credit Card Trust and Insurance Payments
            if (!G1.isHomeOffice() && !G1.isAdminOrSuper())
            {
                MessageBox.Show("*** Sorry *** You do not have permission to run this module!", "Bank Report Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }
            this.Cursor = Cursors.WaitCursor;
            CCBankDeposits ccBankForm = new CCBankDeposits("CC Trust and Insurance");
            ccBankForm.Show();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void funeralPaymentsToolStripMenuItem_Click(object sender, EventArgs e)
        { // Credit Card Funeral Payments
            if (!G1.isHomeOffice() && !G1.isAdminOrSuper())
            {
                MessageBox.Show("*** Sorry *** You do not have permission to run this module!", "Bank Report Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }
            this.Cursor = Cursors.WaitCursor;
            CCBankDeposits ccBankForm = new CCBankDeposits("CC Funerals");
            ccBankForm.Show();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void menuCashLocalReport_Click(object sender, EventArgs e)
        { // Cash Local Deposit Repoert
            if (!G1.isHomeOffice() && !G1.isAdminOrSuper())
            {
                MessageBox.Show("*** Sorry *** You do not have permission to run this module!", "Bank Report Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }
            this.Cursor = Cursors.WaitCursor;
            CashDeposits cashForm = new CashDeposits();
            cashForm.Show();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void editFirstRemoteFuneralDetailReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!G1.isHomeOffice() && !G1.isAdminOrSuper())
            {
                MessageBox.Show("*** Sorry *** You do not have permission to run this module!", "Bank Report Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }
            this.Cursor = Cursors.WaitCursor;
            EditFuneralReports reportForm = new EditFuneralReports();
            reportForm.Show();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void coverReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!G1.isHomeOffice() && !G1.isAdminOrSuper())
            {
                MessageBox.Show("*** Sorry *** You do not have permission to run this module!", "Bank Report Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }
            this.Cursor = Cursors.WaitCursor;
            AchBankDeposits achBankForm = new AchBankDeposits("Cover Report");
            achBankForm.Show();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void aCHDetailsReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!G1.isHomeOffice() && !G1.isAdminOrSuper())
            {
                MessageBox.Show("*** Sorry *** You do not have permission to run this module!", "Bank Report Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }
            this.Cursor = Cursors.WaitCursor;
            AchBankDeposits achBankForm = new AchBankDeposits("ACH Detail Report");
            achBankForm.Show();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void funeralDetailReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!G1.isHomeOffice() && !G1.isAdminOrSuper())
            {
                MessageBox.Show("*** Sorry *** You do not have permission to run this module!", "Bank Report Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }
            this.Cursor = Cursors.WaitCursor;
            AchBankDeposits achBankForm = new AchBankDeposits("Funeral Detail Report");
            achBankForm.Show();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void bankPlusLockboxDepositReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!G1.isHomeOffice() && !G1.isAdminOrSuper())
            {
                MessageBox.Show("*** Sorry *** You do not have permission to run this module!", "Bank Report Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }
            this.Cursor = Cursors.WaitCursor;
            LockBoxDeposits lockBoxForm = new LockBoxDeposits("LKBX");
            lockBoxForm.Show();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void theFirstLockboxDepositReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!G1.isHomeOffice() && !G1.isAdminOrSuper())
            {
                MessageBox.Show("*** Sorry *** You do not have permission to run this module!", "Bank Report Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }
            this.Cursor = Cursors.WaitCursor;
            LockBoxDeposits lockBoxForm = new LockBoxDeposits("TFBX");
            lockBoxForm.Show();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void fixInsurancePayerAddress1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Import importForm = new Import("Import Combined Insurance Addresses");
            importForm.SelectDone += ImportForm_SelectDone13;
            importForm.Show();
        }
        /***********************************************************************************************/
        private void ImportForm_SelectDone13(DataTable dt)
        {
            if (dt == null)
                return;
            if (dt.Rows.Count <= 0)
                return;
            string payer = "";
            string lastName = "";
            string firstName = "";
            string address = "";
            string cmd = "";
            string record = "";
            string contractNumber = "";
            int count = 0;

            string myFields = "";
            DataTable dx = null;

            dt.Columns.Add("NumCustomers");

            this.Cursor = Cursors.WaitCursor;
            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    Application.DoEvents();

                    payer = dt.Rows[i]["payer"].ObjToString();
                    if (String.IsNullOrWhiteSpace(payer))
                        continue;
                    lastName = dt.Rows[i]["Last Name"].ObjToString();
                    if (String.IsNullOrWhiteSpace(lastName))
                        continue;
                    firstName = dt.Rows[i]["FirstName"].ObjToString();
                    if (String.IsNullOrWhiteSpace(firstName))
                        continue;

                    cmd = "Select * from `icustomers` where `payer` = '" + payer + "' AND `firstName` = '" + firstName + "' AND `lastName` = '" + lastName + "';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count <= 0)
                    {

                        cmd = "Select * from `payers` where `payer` = '" + payer + "';";
                        dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count <= 0)
                        {
                            dt.Rows[i]["NumCustomers"] = "BAD";
                            continue;
                        }
                        contractNumber = dx.Rows[0]["contractNumber"].ObjToString();
                        cmd = "Select * from `icustomers` where `contractNumber` = '" + contractNumber + "';";
                        dx = G1.get_db_data(cmd);
                    }

                    if (dx.Rows.Count <= 0)
                    {
                        dt.Rows[i]["NumCustomers"] = "BAD Customer";
                        continue;
                    }
                    dt.Rows[i]["NumCustomers"] = dx.Rows.Count.ObjToString();
                    record = dx.Rows[0]["record"].ObjToString();
                    address = dt.Rows[i]["address"].ObjToString();
                    if (String.IsNullOrWhiteSpace(address))
                        continue;

                    myFields = "address1" + "," + address + ",address2," + " ";

                    G1.update_db_table("icustomers", "record", record, myFields);
                    count++;
                }
            }
            catch (Exception ex)
            {
            }

            this.Cursor = Cursors.Default;

            MessageBox.Show("*** INFO *** " + count.ToString() + " Insurance Customer Addresses Updated!", "Insurance Customers Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);

            DataRow[] dRows = dt.Select("NumCustomers<>'1'");
            if (dRows.Length > 0)
            {
                MessageBox.Show("*** INFO *** Somce Insurance Customers Had More than One Payer!", "Insurance Customers Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            }
        }
        /***********************************************************************************************/
        private void fixInsurancePayerAddress2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Import importForm = new Import("Import C/O Insurance Addresses");
            importForm.SelectDone += ImportForm_SelectDone14;
            importForm.Show();
        }
        /***********************************************************************************************/
        private void ImportForm_SelectDone14(DataTable dt)
        {
            if (dt == null)
                return;
            if (dt.Rows.Count <= 0)
                return;
            string payer = "";
            string lastName = "";
            string firstName = "";
            string address1 = "";
            string address2 = "";
            string cmd = "";
            string record = "";
            string contractNumber = "";
            int count = 0;

            string myFields = "";
            DataTable dx = null;

            dt.Columns.Add("NumCustomers");

            this.Cursor = Cursors.WaitCursor;
            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    Application.DoEvents();

                    payer = dt.Rows[i]["payer"].ObjToString();
                    if (String.IsNullOrWhiteSpace(payer))
                        continue;
                    lastName = dt.Rows[i]["LastName"].ObjToString();
                    if (String.IsNullOrWhiteSpace(lastName))
                        continue;
                    firstName = dt.Rows[i]["FirstName"].ObjToString();
                    if (String.IsNullOrWhiteSpace(firstName))
                        continue;

                    cmd = "Select * from `icustomers` where `payer` = '" + payer + "' AND `firstName` = '" + firstName + "' AND `lastName` = '" + lastName + "';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count <= 0)
                    {

                        cmd = "Select * from `payers` where `payer` = '" + payer + "';";
                        dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count <= 0)
                        {
                            dt.Rows[i]["NumCustomers"] = "BAD";
                            continue;
                        }
                        contractNumber = dx.Rows[0]["contractNumber"].ObjToString();
                        cmd = "Select * from `icustomers` where `contractNumber` = '" + contractNumber + "';";
                        dx = G1.get_db_data(cmd);
                    }

                    if (dx.Rows.Count <= 0)
                    {
                        dt.Rows[i]["NumCustomers"] = "BAD Customer";
                        continue;
                    }
                    dt.Rows[i]["NumCustomers"] = dx.Rows.Count.ObjToString();
                    record = dx.Rows[0]["record"].ObjToString();
                    address1 = dt.Rows[i]["address1"].ObjToString();
                    address2 = dt.Rows[i]["address2"].ObjToString();

                    myFields = "address1," + address1 + ",address2," + address2;

                    G1.update_db_table("icustomers", "record", record, myFields);
                    count++;
                }
            }
            catch (Exception ex)
            {
            }

            this.Cursor = Cursors.Default;

            MessageBox.Show("*** INFO *** " + count.ToString() + " Insurance Customer Addresses Updated!", "Insurance Customers Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);

            DataRow[] dRows = dt.Select("NumCustomers<>'1'");
            if (dRows.Length > 0)
            {
                MessageBox.Show("*** INFO *** Somce Insurance Customers Had More than One Payer!", "Insurance Customers Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            }
        }
        /***********************************************************************************************/
        private void toolStripMenuItem28_Click(object sender, EventArgs e)
        {
            EditManagers managerForm = new EditManagers();
            managerForm.Show();
        }
        /***********************************************************************************************/
        private void oldPassareSystemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            activeSystem = "Other";

            mainSystemToolStripMenuItem.Checked = false;
            menuStrip1.BackColor = Color.LightBlue;

            G1.conn1.Close();
            //G1.CloseConnection();
            G1.oldCopy = true;
            G1.conn1.Open();

            string database = G1.conn1.Database.ObjToString();

            G1.oldCopy = true;
            G1.OpenConnection(G1.conn1);
            database = G1.conn1.Database.ObjToString();

        }
        /***********************************************************************************************/
        private void trustInformationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Passare.Import_Cases();
        }
        /***********************************************************************************************/
        private void goodsAndServicesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Passare.Import_Services();
        }
        /***********************************************************************************************/
        private void paymentsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Passare.Import_Payments();
        }
        /***********************************************************************************************/
        private void acquaintancesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Passare.Import_Acquaintances();
        }
        /***********************************************************************************************/
        private void eventsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Passare.Import_Events();
        }
        /***********************************************************************************************/
        private void militaryInformationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Passare.Import_Veteran();
        }
        /***********************************************************************************************/

        private void caseInformationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Passare.Import_Cases("SMART");
        }
        /***********************************************************************************************/
        private void goodsAndServicesToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Passare.Import_Services("SMART");
        }
        /***********************************************************************************************/
        private void paymentsToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            Passare.Import_Payments("SMART");
        }
        /***********************************************************************************************/
        private void aquantToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Passare.Import_Acquaintances("SMART");
        }
        /***********************************************************************************************/
        private void eventsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Passare.Import_Events("SMART");
        }
        /***********************************************************************************************/
        private void veteranInformationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Passare.Import_Veteran("SMART");
        }
        /***********************************************************************************************/
        private void fixBadContractNumbersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Passare.FixBadContractNumbers("fcustomers", true );
            //Passare.FixBadContractNumbers("fcontracts", true );
            //Passare.FixBadContractNumbers("fcust_extended", true );
            //Passare.FixBadContractNumbers("fcust_services", false );
            //Passare.FixBadContractNumbers("cust_payments", false );
            //Passare.FixBadContractNumbers("relatives", false );
        }
        /***********************************************************************************************/
        private void editOtherInventoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditInventoryOther otherForm = new EditInventoryOther();
            otherForm.Show();
        }
        /***********************************************************************************************/
        private void toolStripMenuItem29_Click(object sender, EventArgs e) // Edit Batesville Surcharges
        {
            EditSurcharges editForm = new EditSurcharges();
            editForm.Show();
        }
        /***********************************************************************************************/
        private void doubleCheckDueDatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DoubleCheckPayments doubleForm = new DoubleCheckPayments();
            doubleForm.Show();
        }
        /***********************************************************************************************/
        private void magnoliaStateBankUnityACHReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!G1.isHomeOffice() && !G1.isAdminOrSuper())
            {
                MessageBox.Show("*** Sorry *** You do not have permission to run this module!", "Bank Report Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }
            this.Cursor = Cursors.WaitCursor;
            MSBankAchReport achForm = new MSBankAchReport("Magnolia State Bank Unity ACH Report");
            achForm.Show();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void magnoliaStateBankUnityACHDeathClaimsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!G1.isHomeOffice() && !G1.isAdminOrSuper())
            {
                MessageBox.Show("*** Sorry *** You do not have permission to run this module!", "Bank Report Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }
            this.Cursor = Cursors.WaitCursor;
            MSBankDeathReport achForm = new MSBankDeathReport("Magnolia State Bank Unity ACH Death Claims");
            achForm.Show();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void bancorpSouthDeathClaimsReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BCSDeathClaims bcsForm = new BCSDeathClaims("BancorpSkouth Bank Death Claims");
            bcsForm.Show();
        }
        /***********************************************************************************************/
        private void HolidayMenu_Click(object sender, EventArgs e)
        {
            EditHolidays holidayForm = new EditHolidays();
            holidayForm.Show();
        }
        /***********************************************************************************************/
        private void cCDetailImportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Import ccForm = new Import(" Credit Card Customer Information");
            ccForm.SelectDone += CcForm_SelectDone;
            ccForm.Show();
        }
        /***********************************************************************************************/
        private void CcForm_SelectDone(DataTable dt)
        {
            string cmd = "";
            string record = "";
            string contractNumber = "";
            string payer = "";
            string cardNumber = "";
            string insFirstName = "";
            string insMiddleName = "";
            string insLastName = "";
            string cardFirstName = "";
            string cardMiddleName = "";
            string cardLastName = "";
            string expirationDate = "";
            string billingZip = "";
            double draftAmount = 0D;
            DateTime draftStartDate = DateTime.Now;
            int draftStartDay = 0;
            int numPayments = 0;
            int remainingPayments = 0;
            int count = 0;
            string str = "";

            DataTable dx = null;

            this.Cursor = Cursors.WaitCursor;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    payer = "";
                    contractNumber = dt.Rows[i]["Contract #"].ObjToString();
                    if (String.IsNullOrWhiteSpace(contractNumber))
                        continue;

                    cmd = "Select * from `customers` WHERE `contractNumber` = '" + contractNumber + "';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count <= 0)
                    {
                        cmd = "Select * from `payers` where `payer` = '" + contractNumber + "';";
                        dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count <= 0)
                        {
                            MessageBox.Show("*** ERROR *** Number " + contractNumber + " is not a Contract or Payer!!", "Bad Contract/Payer Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                            continue;
                        }
                        payer = contractNumber;
                    }
                    cardNumber = dt.Rows[i]["Credit Card #"].ObjToString();
                    insFirstName = dt.Rows[i]["Insured First Name"].ObjToString();
                    insMiddleName = dt.Rows[i]["Insured Middle Name"].ObjToString();
                    insLastName = dt.Rows[i]["Insured Last Name"].ObjToString();

                    cardFirstName = dt.Rows[i]["Card First Name"].ObjToString();
                    cardMiddleName = dt.Rows[i]["Card Middle Name"].ObjToString();
                    cardLastName = dt.Rows[i]["Card Last Name"].ObjToString();

                    expirationDate = dt.Rows[i]["Expiration Date"].ObjToString();
                    billingZip = dt.Rows[i]["Billing Zip"].ObjToString();

                    str = dt.Rows[i]["Amount Of Draft"].ObjToString();
                    str = str.Replace("$", "");
                    draftAmount = str.ObjToDouble();
                    draftStartDate = dt.Rows[i]["Draft Start Date"].ObjToDateTime();
                    draftStartDay = dt.Rows[i]["Day Of Month To Draft"].ObjToInt32();
                    numPayments = dt.Rows[i]["# Of Payments"].ObjToInt32();
                    remainingPayments = dt.Rows[i]["Payments Remaining"].ObjToInt32();

                    cmd = "Select * from `creditcards` WHERE `contractNumber` = '" + contractNumber + "';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                        record = dx.Rows[0]["record"].ObjToString();
                    else
                        record = G1.create_record("creditcards", "cardMiddleName", "-1");
                    if (G1.BadRecord("creditcards", record))
                        return;
                    G1.update_db_table("creditcards", "record", record, new string[] { "contractNumber", contractNumber, "payer", payer, "insFirstName", insFirstName, "insMiddleName", insMiddleName, "insLastName", insLastName, "cardFirstName", cardFirstName, "cardMiddleName", cardMiddleName, "cardLastName", cardLastName, "billingZip", billingZip });
                    G1.update_db_table("creditcards", "record", record, new string[] { "draftAmount", draftAmount.ToString(), "ccNumber", cardNumber, "expirationDate", expirationDate, "draftStartDate", draftStartDate.ToString("yyyy-MM-dd"), "draftStartDay", draftStartDay.ToString(), "numPayments", numPayments.ToString(), "remainingPayments", remainingPayments.ToString() });

                    count++;
                }
                catch (Exception ex)
                {
                }
            }
            this.Cursor = Cursors.Default;

            MessageBox.Show("*** Info *** " + count.ToString() + " Credit Card Customers Imported!", "Credit Card Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
        }
        /***********************************************************************************************/
        private void toolStripMenuItem30_Click(object sender, EventArgs e)
        {
            EditCC ccForm = new EditCC();
            ccForm.Show();
        }
        /***********************************************************************************************/
        private void toolEditCCMenu_Click(object sender, EventArgs e)
        {
            EditCCPercent ccForm = new EditCCPercent();
            ccForm.Show();
        }
        /***********************************************************************************************/
        private void menuCreditCardsFromBank_Click(object sender, EventArgs e)
        {
            CCImport ccForm = new CCImport();
            ccForm.Show();
        }
        /***********************************************************************************************/
        private void bankCCCoversheetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            CCBankReport bankform = new CCBankReport();
            bankform.Show();

            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void importBankDepositDetailToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImportBankDetails bankForm = new ImportBankDetails();
            bankForm.Show();
        }
        /***********************************************************************************************/
        private void dailyDepositReportFromAllSourcesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DailyPayments dailyForm = new DailyPayments();
            dailyForm.Show();
        }
        /***********************************************************************************************/
        private void rilesUpdateTrustPercentageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImportRiles rilesForm = new ImportRiles("FIX");
            rilesForm.Show();
        }
        /***********************************************************************************************/
        private void fixRilesTrust2013rDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FixRilesTrustData fixForm = new FixRilesTrustData();
            fixForm.Show();
        }
        /***********************************************************************************************/
        private void menuEditMassReports_Click(object sender, EventArgs e)
        {
            EditMassReports editForm = new EditMassReports();
            editForm.Show();
        }
        /***********************************************************************************************/
        private void editVehiclesMenu_Click(object sender, EventArgs e)
        {
            EditCars carsForm = new EditCars();
            carsForm.Show();
        }
        /***********************************************************************************************/
        private void btnClock_Click(object sender, EventArgs e)
        {
            //if (!isTimeKeeper && !G1.isAdmin() && !G1.isHR() && !isManager && !G1.RobbyServer )
            //{
            //    MessageBox.Show("*** Sorry *** This Function is not available at this time!!", "Time Sheet Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            //    return;
            //}
            string user = LoginForm.username.ToUpper();

            string name = "";
            string cmd = "Select * from `users` where `userName` = '" + LoginForm.username + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
            {
                if ( !LoginForm.isRobby )
                    return;
                name = "Robby Graham";
            }
            else
                name = dx.Rows[0]["firstName"].ObjToString() + " " + dx.Rows[0]["lastName"].ObjToString();
            if (!isTimeKeeper && !G1.isAdmin() && !G1.isHR() && !isManager)
            {
                this.Cursor = Cursors.WaitCursor;
                string empno = LoginForm.workUserRecord;
                Form form = G1.IsFormOpen("TimeClock");
                if (form != null)
                {
                    form.Show();
                    form.WindowState = FormWindowState.Normal;
                    form.Visible = true;
                    form.Refresh();
                    form.BringToFront();
                }
                else
                {
                    TimeClock timeForm = new TimeClock(empno, LoginForm.username, name);
                    timeForm.Show();
                }
                this.Cursor = Cursors.Default;
            }
            else
            {
                this.Cursor = Cursors.WaitCursor;
                string empno = LoginForm.workUserRecord;
                Form form = G1.IsFormOpen("Employees");
                if (form != null)
                {
                    form.Show();
                    form.WindowState = FormWindowState.Normal;
                    form.Visible = true;
                    form.Refresh();
                    form.BringToFront();
                }
                else
                {
                    Employees empForm = new Employees(empno, name);
                    empForm.Show();
                }
                this.Cursor = Cursors.Default;
            }
        }
        /****************************************************************************************/
        private void menuEditLocationCombos_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            EditLocationCombos comboForm = new EditLocationCombos("ACH Detail Report");
            comboForm.Show();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void editFirstReportACHCoverReportCombinationsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            EditLocationCombos comboForm = new EditLocationCombos("Cover Report");
            comboForm.Show();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void menuInsuranceCompanies_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            EditInsuranceCompanies editForm = new EditInsuranceCompanies();
            editForm.Show();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void menuEditFuneralHomeGroups_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            EditHomeGroups homeForm = new EditHomeGroups();
            homeForm.Show();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void btnUrn_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            EditUrnLog urnForm = new EditUrnLog();
            urnForm.Show();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void editHRGroupsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            EditHrGroups groupForm = new EditHrGroups();
            groupForm.Show();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void btnContacts_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            Contacts contactForm = new Contacts();
            contactForm.Show();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void editContactTypesMenu_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            EditContactTypes contactForm = new EditContactTypes();
            contactForm.Show();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void runPolicyToTrustModuleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            EditPolicyTrusts policyForm = new EditPolicyTrusts();
            policyForm.Show();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void importSecurityNationalFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            ImportTrustFile trustForm = new ImportTrustFile( "Security National");
            trustForm.Show();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void importFDLICFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            ImportTrustFile trustForm = new ImportTrustFile("FDLIC");
            trustForm.Show();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void importUnityFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            ImportTrustFile trustForm = new ImportTrustFile("Unity");
            trustForm.Show();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void importForethoughtFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            ImportTrustFile trustForm = new ImportTrustFile("Forethought");
            trustForm.Show();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void runPolicyVerifyModuleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            VerifyPolicyTrusts policyForm = new VerifyPolicyTrusts();
            policyForm.Show();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void findMismatchedDueDatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            FindMismatches mismatchForm = new FindMismatches();
            mismatchForm.Show();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void importCDFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            ImportTrustFile trustForm = new ImportTrustFile("CD");
            trustForm.Show();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void editRelationCategoriesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            EditRelationCategory editForm = new EditRelationCategory();
            editForm.Show();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void editRelationAgeRangesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditRelationAges editForm = new EditRelationAges();
            editForm.Show();
        }
        /****************************************************************************************/
        private void editAllTrackingDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditTracking trackForm = new EditTracking("All");
            trackForm.Show();
        }
        /****************************************************************************************/
        private void preneedContactsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            ImportPreneedContacts importForm = new ImportPreneedContacts();
            importForm.Show();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private string importWhat = "Import";
        private void cashRemitDBRsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            importWhat = "Import";
            Import importForm = new Import();
            importForm.SelectDone += ImportForm_DBRDone;
            importForm.Show();
        }
        /***********************************************************************************************/
        private void ImportForm_DBRDone(DataTable dt)
        {
            if (dt == null)
                return;
            if (dt.Rows.Count <= 0)
                return;
            string payerNumber = "";
            string policyNumber = "";
            string ucode = "";
            string cmd = "";
            DataTable dx = null;
            string record = "";
            string updateString = "";
            string actualFile = dt.TableName.ObjToString();

            EditDBRs dbrForm = new EditDBRs(importWhat, dt, actualFile);
            dbrForm.Show();
        }
        /****************************************************************************************/
        private void editDBRTableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            EditDBRs dbrForm = new EditDBRs();
            dbrForm.Show();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void cashRemittedDPsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            importWhat = "Import DPs";
            Import importForm = new Import();
            importForm.SelectDone += ImportForm_DBRDone;
            importForm.Show();
        }
        /****************************************************************************************/
        private void cashRemittedPaymentsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            importWhat = "Import Payments";
            Import importForm = new Import();
            importForm.SelectDone += ImportForm_DBRDone;
            importForm.Show();
        }
        /****************************************************************************************/
    }
}
