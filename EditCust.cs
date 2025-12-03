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
using DevExpress.XtraRichEdit.UI;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using System.Runtime.Remoting.Messaging;
using DevExpress.XtraCharts.Native;
using MySql.Data.MySqlClient;
using java.security;
using DevExpress.XtraPrinting;
using DevExpress.Utils;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class EditCust : DevExpress.XtraEditors.XtraForm
    {
        private bool loading = true;
        private string workContract = "";
        private string workPayer = "";
        private string originalTitle = "";
        private DataTable workDt = null;
        public static string activeFuneralHomeName = "";
        public static string activeFuneralHomeGroup = "";
        public static string activeFuneralHomeCasketGroup = "";
        public string myWorkContract = "";
        private DataTable workServicesDt = null;
        private DataTable workPaymentsDt = null;
        private int originalPanelVitalsHeight = 0;
        private string contractFile = "fcustomers";
        private string customerFile = "fcustomers";
        private bool workContractOnly = false;

        private string workDatabase = "SMFS";
        private bool OpenCloseFuneral = false;
        /****************************************************************************************/
        public string[] liveContracts = new string[10];
        public int liveContractsCount = 0;

        public DataTable masterTable = null;
        public DataTable pallTable = null;

        private FormWindowState mLastState;

        /****************************************************************************************/
        public EditCust(string contract = "")
        {
            InitializeComponent();
            workContract = contract;
            myWorkContract = contract;
            workContractOnly = false;
        }
        /****************************************************************************************/
        public EditCust(bool contractOnly, string contract = "" )
        {
            InitializeComponent();
            workContract = contract;
            myWorkContract = contract;
            workContractOnly = contractOnly;
            this.Cursor = Cursors.WaitCursor;
            EditCust_Load(null, null);
        }
        /****************************************************************************************/
        private void EditCust_Load(object sender, EventArgs e)
        {
            //FunFamilyNew.saveMembersDt = null;
            //FunFamilyNew.preprocessDone = false;

            if (!G1.isAdmin())
                selectToolStripMenuItem.Dispose();

            Contract1.signatureDt = null;

            workDatabase = G1.conn1.Database.ObjToString();

            if (G1.oldCopy)
                menuStrip1.BackColor = Color.LightBlue;

            savedContractsMenu.Visible = false;
            if (!G1.isAdmin())
                changeLogToolStripMenuItem.Visible = false;

            //string funeralDirectory = G1.GetFuneralFileDirectory(workContract);

            originalTitle = "";
            ClearAllPanels();
            string serviceId = "";
            string title = "";
            bool isInsurance = false;
            if (!String.IsNullOrWhiteSpace(workContract))
            {
                string cmd = "Select * from `" + contractFile + "` c JOIN `fcustomers` z ON c.`contractNumber` = z.`contractNumber` where c.`contractNumber` = '" + workContract + "';";
                workDt = G1.get_db_data(cmd);
                if ( workDt.Rows.Count <= 0 )
                {
                    cmd = "Select * from `fcustomers` c where c.`payer` = '" + workContract + "';";
                    if (DailyHistory.isInsurance(workContract))
                        cmd = "Select * from `fcustomers` c where c.`contractNumber` = '" + workContract + "';";
                    workDt = G1.get_db_data(cmd);
                    if ( workDt.Rows.Count > 0 )
                    {
                        if (workDt.Rows.Count > 0 && DailyHistory.isInsurance(workContract))
                            workPayer = workDt.Rows[0]["payer"].ObjToString();

                        workContract = workDt.Rows[0]["contractNumber"].ObjToString();
//                        cmd = "Select * from `" + contractFile + "` c JOIN `customers` z ON c.`contractNumber` = z.`contractNumber` where c.`contractNumber` = '" + workContract + "';";
                        cmd = "Select * from `" + contractFile + "` c where c.`contractNumber` = '" + workContract + "';";
                        if (DailyHistory.isInsurance(workContract))
                        {
//                            cmd = "Select * from `" + contractFile + "` c JOIN `icustomers` z ON c.`contractNumber` = z.`contractNumber` where c.`contractNumber` = '" + workContract + "';";
                            cmd = "Select * from `" + contractFile + "` c where c.`contractNumber` = '" + workContract + "';";
                        }
                        workDt = G1.get_db_data(cmd);
                        if ( contractFile.ToUpper() == "FCUSTOMERS" && DailyHistory.isInsurance(workContract))
                            workPayer = workDt.Rows[0]["payer"].ObjToString();
                    }
                    else
                    {
                        cmd = "Select * from `fcustomers` c where c.`contractNumber` = '" + workContract + "';";
                        workDt = G1.get_db_data(cmd);
                        if ( workDt.Rows.Count <= 0)
                        {
                            cmd = "Select * from `icustomers` c where c.`payer` = '" + workContract + "';";
                            workDt = G1.get_db_data(cmd);
                            if ( workDt.Rows.Count > 0 )
                            {
                                workPayer = workContract;
                                workContract = workDt.Rows[0]["contractNumber"].ObjToString();
                            }
                        }
                    }
                }
                if (workDt.Rows.Count > 0)
                {
                    ConfirmServiceId(workDt.Rows[0]);
                    serviceId = workDt.Rows[0]["serviceId"].ObjToString();
                    title = CustomerDetails.BuildClientTitle(workDt.Rows[0]);
                    this.Text = title;
                    originalTitle = title;
                }
            }

            //DetermineWorkingFuneralHome( serviceId );
            DetermineWorkingFuneralHome(workContract, serviceId);

            if (String.IsNullOrWhiteSpace(activeFuneralHomeName) && !OpenCloseFuneral )
            {
                return;
            }
            this.Text += " [" + activeFuneralHomeName + "]  [Group: " + activeFuneralHomeGroup + "]  [CasketGroup: " + activeFuneralHomeCasketGroup + "]";

            txtFuneralHome.Text = activeFuneralHomeName;
            txtGPLGroup.Text = activeFuneralHomeGroup;
            txtCasketGroup.Text = activeFuneralHomeCasketGroup;

            EditCustomer.activeFuneralHomeGroup = activeFuneralHomeGroup;
            EditCustomer.activeFuneralHomeCasketGroup = activeFuneralHomeCasketGroup;
            EditCustomer.activeFuneralHomeName = activeFuneralHomeName;

            this.panelAll.BorderStyle = BorderStyle.FixedSingle;
            this.panelDesign.BorderStyle = BorderStyle.FixedSingle;

            this.panelDesign.Controls.Clear();
            this.panelDesign.VerticalScroll.Maximum = 0;
            this.panelDesign.VerticalScroll.Value = 0;
            this.panelDesign.VerticalScroll.Enabled = false;

            //            this.panelAll.Controls.Clear();

            this.panelAll.TabStop = false;
            this.panelAll.AutoScroll = false;
            this.panelAll.HorizontalScroll.Enabled = false;
            this.panelAll.HorizontalScroll.Visible = false;

            this.panelAll.VerticalScroll.Enabled = true;
            this.panelAll.VerticalScroll.Visible = true;
            this.panelAll.VerticalScroll.Value = 0;

            this.panelAll.HorizontalScroll.Maximum = 0;
            this.panelAll.AutoScroll = true;

            panelAll.VerticalScroll.Maximum = 0;
            panelAll.VerticalScroll.Value = 0;
            // panelAll.Controls.Clear();

            InitializeCustomerPanel();
            //InitializeFamilyPanel();
            //InitializeServicePanel();
            //InitializePaymentsPanel();
            //InitializeLegalPanel();
            //InitializeFormsPanel();

            panelAll.VerticalScroll.Maximum = 0;
            panelAll.VerticalScroll.Value = 0;

            panelAll.VerticalScroll.Maximum = 0;
            this.LostFocus += EditCustomer_LostFocus;
            this.GotFocus += EditCustomer_GotFocus;

            int left = panelAll.Left;
            int top = panelAll.Top;
            int width = panelAll.Width;
            int height = FindMainWindowHeight();
            panelDesign.SetBounds(left, top, width, height);
            panelDesign.Dock = DockStyle.Top;
            //this.panelDesign.Dock = DockStyle.Fill;
            panelDesign.Show();

            positionAll();
            LoadFuneralInfo();
            CheckForAgreement(workContract);
            CheckForContract();
            CleanupMenu();
            loading = false;
            this.WindowState = FormWindowState.Maximized;
            if (SMFS.SMFS_MainForm != null)
                SMFS.SMFS_MainForm.WindowState = FormWindowState.Minimized;
            if ( workContractOnly )
            {
                this.Cursor = Cursors.WaitCursor;
                toolStripButton1_Click(null, null);
                if (SMFS.SMFS_MainForm != null)
                    SMFS.SMFS_MainForm.WindowState = FormWindowState.Normal;
                this.Close();
            }
            this.Refresh();
            mainRect = this.Bounds;
            mLastState = this.WindowState;
        }
        /****************************************************************************************/
        private Rectangle mainRect;
        /****************************************************************************************/
        private void CleanupMenu ()
        {
            if (LoginForm.administrator)
                return;
            selectToolStripMenuItem.Enabled = false;
        }
        /****************************************************************************************/
        private void CheckForAgreement(string contractNumber)
        {
            this.btnContracts.Enabled = false;
            this.btnContracts.Tag = "";
            this.btnContracts.Text = "";
            string cmd = "Select * from `customers` where `contractNumber` = '" + contractNumber + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return;
            string ssn = dt.Rows[0]["ssn"].ObjToString();
            cmd = "Select * from `customers` where `ssn` = '" + ssn + "';";
            dt = G1.get_db_data(cmd);
            DataTable picDt = null;
            int count = 0;
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                cmd = "Select * from `pdfimages` where `contractNumber` = '" + contractNumber + "';";
                picDt = G1.get_db_data(cmd);
                if (picDt.Rows.Count > 0)
                {
                    count++;
                    this.btnContracts.Tag = picDt.Rows[0]["record"].ObjToString();
                    this.btnContracts.Enabled = true;
                }
            }
            if (count > 0)
            {
                if ( count == 1)
                    btnContracts.ToolTipText = count.ToString() + " Pre-Need Contract Available";
                else
                    btnContracts.ToolTipText = count.ToString() + " Pre-Need Contracts Available";
            }
        }
        /****************************************************************************************/
        private void DetermineWorkingFuneralHome( string contractNumber, string serviceId = "" )
        {
            activeFuneralHomeCasketGroup = "";
            activeFuneralHomeGroup = "";
            activeFuneralHomeName = "";
            if (DailyHistory.isInsurance(contractNumber))
                contractNumber = serviceId;
            string trust = "";
            string loc = "";
            string junk = "";
            if ( !String.IsNullOrWhiteSpace ( contractNumber ))
            {
                if ( !String.IsNullOrWhiteSpace ( serviceId))
                    junk = Trust85.decodeContractNumber(serviceId, true, ref trust, ref loc);
                else
                    junk = Trust85.decodeContractNumber(contractNumber, true, ref trust, ref loc);
                if (!String.IsNullOrWhiteSpace(loc))
                    LoginForm.activeFuneralHomeKeyCode = loc;
            }

            bool gotCemetery = DailyHistory.gotCemetery(serviceId);
            if ( gotCemetery )
            {
                DetermineCemetery(serviceId);
                return;
            }

            if (String.IsNullOrWhiteSpace(LoginForm.activeFuneralHomeKeyCode) )
            {
                for (;;)
                {
                    using (FuneralHomeSelect funSelect = new FuneralHomeSelect())
                    {
                        funSelect.ShowDialog();
                    }
                    if (String.IsNullOrWhiteSpace(LoginForm.activeFuneralHomeKeyCode))
                    {
                        DialogResult result = MessageBox.Show("***Warning*** Are you sure you DO NOT WANT to select an Active Funeral Home?", "Active Funeral Home Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                        if (result == DialogResult.Yes)
                        {
                            this.Close();
                            return;
                        }
                    }
                    break;
                }
            }
            //string cmd = "select * from `funeralHomes` where `assignedAgents` LIKE ('%" + LoginForm.activeFuneralHomeAgent + "%');";

            string cmd = "Select * from `funeralhomes` where `atneedcode` = '" + LoginForm.activeFuneralHomeKeyCode + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0  )
            {
                if ( !String.IsNullOrWhiteSpace (LoginForm.activeFuneralHomeKeyCode))
                {
                    DataTable funDt = G1.get_db_data("Select * from `funeralHomes` WHERE `keycode` = '" + LoginForm.activeFuneralHomeKeyCode + "';");
                    if ( funDt.Rows.Count <= 0 )
                    {
                        funDt = G1.get_db_data("Select * from `funeralHomes` WHERE `merchandisecode` = '" + LoginForm.activeFuneralHomeKeyCode + "';");
                        if (funDt.Rows.Count <= 0)
                        {
                            bool doit = true;
                            cmd = "Select * from `fcust_extended` WHERE `serviceId` = '" + serviceId + "';";
                            funDt = G1.get_db_data(cmd);
                            if ( funDt.Rows.Count > 0 )
                            {
                                if (funDt.Rows[0]["OpenCloseFuneral"].ObjToString().ToUpper() == "Y")
                                {
                                    doit = false;
                                    junk = Trust85.decodeContractNumber(serviceId, true, ref trust, ref loc);
                                    LoginForm.activeFuneralHomeKeyCode = loc;
                                    OpenCloseFuneral = true;
                                }
                            }
                            if (doit)
                            {
                                using (FuneralHomeSelect funSelect = new FuneralHomeSelect())
                                {
                                    funSelect.ShowDialog();
                                }
                            }
                        }
                        else
                        {
                            LoginForm.activeFuneralHomeKeyCode = funDt.Rows[0]["keycode"].ObjToString();
                        }
                    }
                }
                else
                {
                    using (FuneralHomeSelect funSelect = new FuneralHomeSelect())
                    {
                        funSelect.ShowDialog();
                    }
                }
                if (!OpenCloseFuneral)
                {
                    if (String.IsNullOrWhiteSpace(LoginForm.activeFuneralHomeKeyCode))
                    {
                        MessageBox.Show("***ERROR*** Invalid Funeral Home!");
                        this.Close();
                        return;
                    }
                    cmd = "Select * from `funeralhomes` where `keycode` = '" + LoginForm.activeFuneralHomeKeyCode + "';";
                    dt = G1.get_db_data(cmd);
                    if (dt.Rows.Count <= 0)
                    {
                        if ( gotCemetery )
                        {
                        }
                        MessageBox.Show("***ERROR*** Invalid Funeral Home!");
                        this.Close();
                        return;
                    }
                }
            }
            if (!OpenCloseFuneral  )
            {
                activeFuneralHomeName = dt.Rows[0]["LocationCode"].ObjToString();
                if (String.IsNullOrWhiteSpace(activeFuneralHomeName))
                    activeFuneralHomeName = dt.Rows[0]["name"].ObjToString();
                activeFuneralHomeGroup = dt.Rows[0]["groupname"].ObjToString();
                activeFuneralHomeCasketGroup = dt.Rows[0]["casketgroup"].ObjToString();
            }
        }
        /****************************************************************************************/
        private void DetermineCemetery ( string serviceId )
        {
            string loc = "";
            string trust = "";
            Trust85.decodeContractNumber(serviceId, ref trust, ref loc);
            if (String.IsNullOrWhiteSpace(loc))
                return;
            string cmd  = "Select * from `cemeteries` where `loc` = '" + loc + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
                return;
            activeFuneralHomeName = dx.Rows[0]["loc"].ObjToString();
        }
        /****************************************************************************************/
        public static void DetermineActiveGroups(string contractNumber, string serviceId = "")
        {
            string trust = "";
            string loc = "";
            string junk = "";
            activeFuneralHomeCasketGroup = "";
            activeFuneralHomeGroup = "";
            activeFuneralHomeName = "";
            if (DailyHistory.isInsurance(contractNumber))
                contractNumber = serviceId;
            if (!String.IsNullOrWhiteSpace(contractNumber))
            {
                if (!String.IsNullOrWhiteSpace(serviceId))
                    junk = Trust85.decodeContractNumber(serviceId, true, ref trust, ref loc);
                else
                    junk = Trust85.decodeContractNumber(contractNumber, true, ref trust, ref loc);
                if (!String.IsNullOrWhiteSpace(loc))
                    LoginForm.activeFuneralHomeKeyCode = loc;
            }
            if (String.IsNullOrWhiteSpace(LoginForm.activeFuneralHomeKeyCode))
                return;

            bool OpenCloseFuneral = false;
            string cmd = "Select * from `funeralhomes` where `atneedcode` = '" + LoginForm.activeFuneralHomeKeyCode + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
            {
                if (!String.IsNullOrWhiteSpace(LoginForm.activeFuneralHomeKeyCode))
                {
                    DataTable funDt = G1.get_db_data("Select * from `funeralHomes` WHERE (`keycode` = '" + LoginForm.activeFuneralHomeKeyCode + "' OR `merchandiseCode` = '" + LoginForm.activeFuneralHomeKeyCode + "');");
                    if (funDt.Rows.Count <= 0)
                    {
                        if ( !String.IsNullOrWhiteSpace ( serviceId ))
                        {
                            cmd = "Select * from `fcust_extended` WHERE `serviceId` = '" + serviceId + "';";
                            funDt = G1.get_db_data(cmd);
                            if ( funDt.Rows.Count > 0 )
                            {
                                if (funDt.Rows[0]["OpenCloseFuneral"].ObjToString().ToUpper() == "Y")
                                    OpenCloseFuneral = true;
                            }
                        }
                        if (!OpenCloseFuneral)
                        {
                            using (FuneralHomeSelect funSelect = new FuneralHomeSelect())
                            {
                                funSelect.ShowDialog();
                            }
                        }
                    }
                    else
                    {
                        LoginForm.activeFuneralHomeKeyCode = funDt.Rows[0]["keycode"].ObjToString();
                    }
                }
                else
                {
                    using (FuneralHomeSelect funSelect = new FuneralHomeSelect())
                    {
                        funSelect.ShowDialog();
                    }
                }
                if (String.IsNullOrWhiteSpace(LoginForm.activeFuneralHomeKeyCode))
                {
                    MessageBox.Show("***ERROR*** Invalid Funeral Home!");
                    return;
                }
                if (OpenCloseFuneral)
                    return;
                cmd = "Select * from `funeralhomes` where `keycode` = '" + LoginForm.activeFuneralHomeKeyCode + "';";
                dt = G1.get_db_data(cmd);
                if (dt.Rows.Count <= 0)
                {
                    MessageBox.Show("***ERROR*** Invalid Funeral Home!", "Funeral Home Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    return;
                }
            }
            activeFuneralHomeName = dt.Rows[0]["LocationCode"].ObjToString();
            if (String.IsNullOrWhiteSpace(activeFuneralHomeName))
                activeFuneralHomeName = dt.Rows[0]["name"].ObjToString();
            activeFuneralHomeGroup = dt.Rows[0]["groupname"].ObjToString();
            activeFuneralHomeCasketGroup = dt.Rows[0]["casketgroup"].ObjToString();
        }
        /****************************************************************************************/
        private void SetDesignHeight()
        {
            int left = panelAll.Left;
            int top = panelAll.Top;
            int width = panelAll.Width;
            int height = FindMainWindowHeight();
            panelDesign.SetBounds(left, top, width, height);
            //panelDesign.Hide();
            //panelTest.SetBounds(left, top, width, height);
            //panelTest.Show();
            //panelTest.Dock = DockStyle.Fill;
        }
        /****************************************************************************************/
        private void positionAll()
        {
            this.tabControl1.Hide();

            //AddDesign(this.panelForms, DockStyle.Bottom, ref panelFormsMoved);
            //AddDesign(this.panelLegal, DockStyle.Top, ref panelLegalMoved);
            //AddDesign(this.panelPayments, DockStyle.Top, ref panelPaymentsMoved);
            //AddDesign(this.panelServices, DockStyle.Top, ref panelServicesMoved);
            //AddDesign(this.panelFamily, DockStyle.Top, ref panelFamilyMoved);
            //AddDesign(this.panelCustomer, DockStyle.Top, ref panelCustomerMoved);

            //AddDesign(this.panelForms, DockStyle.Fill, ref panelFormsMoved);
            //AddDesign(this.panelLegal, DockStyle.Fill, ref panelLegalMoved);
            //AddDesign(this.panelPayments, DockStyle.Fill, ref panelPaymentsMoved);
            //AddDesign(this.panelServices, DockStyle.Fill, ref panelServicesMoved);
            //AddDesign(this.panelFamily, DockStyle.Fill, ref panelFamilyMoved);
            //AddDesign(this.panelCustomer, DockStyle.Fill, ref panelCustomerMoved);
        }
        /****************************************************************************************/
        //        private Control lastControl = null;
        //        private Point lastPoint = new Point();
        private void ResetPanels(Panel myPanel)
        {
            SetDesignHeight();
            //if (1 == 1)
            //    return;
            try
            {
                Control control = null;
                int controlCount = 0;
                string name = "";
                for (int i = 0; i < this.panelDesign.Controls.Count; i++)
                {
                    control = this.panelDesign.Controls[i];
                    if (control.Visible)
                    {
                        name = control.Tag.ObjToString().ToUpper();
                        if (name == "MYPANEL")
                            controlCount++;
                    }
                }
                int actualHeight = FindMainWindowHeight();
                for (int i = 0; i < this.panelDesign.Controls.Count; i++)
                {
                    control = this.panelDesign.Controls[i];
                    if (control.Visible)
                    {
                        name = control.Tag.ObjToString().ToUpper();
                        if (name == "MYPANEL")
                        {
                            if ((Panel)control == myPanel)
                            {
                                control.Focus();
                                //if ( controlCount == 1 )
                                //    control.SetBounds(control.Left, control.Top, control.Width, this.Height);
                                int left = this.panelDesign.Left;
                                int top = this.panelDesign.Top;
                                int width = this.panelDesign.Width;
                                int height = this.panelDesign.Height;
                                control.SetBounds(left, top, width, height);

                                G1.LoadFormInPanel(editFunCustomer, this.panelCustomer);
                                control.SetBounds(left, top, width, height);

                                //this.panelCustomer.Show();

                                control.Dock = DockStyle.Fill;


                                panelAll.ScrollControlIntoView(control);
                                control.Focus();
                                //control.Dock = DockStyle.Top;
                            }
                        }
                        else if ( name == "PANELVITALS")
                        {
                        }
                    }
                }
                this.panelAll.TabStop = false;
            }
            catch (Exception ex)
            {
            }
        }
        /****************************************************************************************/
        private int FindMainWindowHeight()
        {
            Control control = null;
            int totalHeight = 0;
            string name = "";
            for (int i = 0; i < this.panelDesign.Controls.Count; i++)
            {
                control = this.panelDesign.Controls[i];
                if (control.Visible)
                {
                    name = control.Tag.ObjToString().ToUpper();
                    if (name == "MYPANEL")
                        totalHeight += control.Height + 5;
                }
            }
            return totalHeight;
        }
        /****************************************************************************************/
        private void AddDesign(Panel myPanel, DockStyle style, ref bool panelMoved)
        {
            this.panelDesign.Hide();
            this.panelDesign.SuspendLayout();
            myPanel.SuspendLayout();
            this.panelDesign.Controls.Add(myPanel);
            myPanel.Dock = style;
            myPanel.ResumeLayout();
            myPanel.Hide();
            this.panelDesign.ResumeLayout();
            this.panelDesign.PerformLayout();
            this.panelDesign.Show();
            panelMoved = true;
        }
        /****************************************************************************************/
        private void btnCustomer_Click(object sender, EventArgs e)
        {
            FastLookup fastForm = new FastLookup(workContract);
            fastForm.ListDone += FastForm_ListDone;
            fastForm.Show();
        }
        /****************************************************************************************/
        private void FastForm_ListDone(string s)
        {
            if (String.IsNullOrWhiteSpace(s))
                return;

            if (s.ToUpper().IndexOf("GET OUT!") == 0 )
            {
                OnCustRename( s );
                return;
            }

            string contractNumber = s;
            string contracts = "contracts";
            string customers = "customers";
            if (DailyHistory.isInsurance(s))
            {
                contracts = "icontracts";
                customers = "icustomers";
            }

            string[] Lines = contractNumber.Split(':');
            if (Lines.Length < 2)
                return;
            contractNumber = Lines[1].Trim();
            string cmd = "Select * from `" + contracts + "` c JOIN `" + customers + "` b ON c.`contractNumber` = b.`contractNumber` where c.`contractNumber` = '" + contractNumber + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return;
            workContract = contractNumber;
            string firstName = dt.Rows[0]["firstName"].ObjToString();
            string lastName = dt.Rows[0]["lastName"].ObjToString();
            ConfirmServiceId(dt.Rows[0]);
            string title = CustomerDetails.BuildClientTitle(dt.Rows[0]);
            this.Text = title;
            InitializeCustomerPanel();
            InitializeFamilyPanel();
            InitializeServicePanel();
            InitializePaymentsPanel();
            InitializeFormsPanel();
            toolCustomerClick(true);
        }
        /****************************************************************************************/
        public static void ConfirmServiceId ( DataRow dR)
        {
            string serviceId = dR["serviceId"].ObjToString();
            if ( String.IsNullOrWhiteSpace ( serviceId))
            {
                string contractNumber = dR["contractNumber"].ObjToString();
                if ( !String.IsNullOrWhiteSpace ( contractNumber))
                {
                    string cmd = "Select * from `fcust_extended` where `contractNumber` = '" + contractNumber + "';";
                    DataTable dt = G1.get_db_data(cmd);
                    if ( dt.Rows.Count > 0 )
                    {
                        serviceId = dt.Rows[0]["serviceId"].ObjToString();
                        dR["serviceId"] = serviceId;
                    }
                }
            }
        }
        /****************************************************************************************/
        public static string ConfirmServiceId(string contractNumber)
        {
            string serviceId = "";
            if (!String.IsNullOrWhiteSpace(contractNumber))
            {
                string cmd = "Select * from `fcust_extended` where `contractNumber` = '" + contractNumber + "';";
                DataTable dt = G1.get_db_data(cmd);
                if (dt.Rows.Count > 0)
                {
                    serviceId = dt.Rows[0]["serviceId"].ObjToString();
                }
            }
            return serviceId;
        }
        /****************************************************************************************/
        private bool panelCustomerActive = false;
        private bool panelCustomerMoved = false;
        private void toolCustomerDemographics_Click(object sender, EventArgs e)
        { // Bring PanelCustomer into PanelAll
            toolCustomerClick();
        }
        /****************************************************************************************/
        private void toolCustomerClick(bool force = false)
        { // Bring PanelCustomer into PanelAll
            try
            {
                ClearAllPanels();

                if (panelCustomerActive && !force)
                {
                    toolCustomerDemographics.BackColor = Color.Transparent;
                    this.panelCustomer.Hide();
                    panelCustomerActive = false;
                }
                else
                {
                    if (panelCustomerMoved)
                    {
                        toolCustomerDemographics.BackColor = Color.Yellow;
                        panelCustomer.Show();
                        panelCustomer.Focus();
                        panelCustomerActive = true;
                        panelCustomer.Dock = DockStyle.Fill;
                    }
                    else
                    {
                        this.panelAll.SuspendLayout();
                        this.panelAll.Controls.Add(this.panelCustomer);
                        this.panelAll.ResumeLayout(false);
                        this.panelAll.PerformLayout();
                        this.panelCustomer.Dock = DockStyle.Fill;
                        panelCustomerActive = true;
                        panelCustomerMoved = true;
                        panelCustomer.Show();
                        toolCustomerDemographics.BackColor = Color.Yellow;
                    }
                }
                ResetPanels(panelCustomer);
            }
            catch (Exception ex)
            {

            }
        }
        /****************************************************************************************/
        private bool panelFamilyActive = false;
        private bool panelFamilyMoved = false;
        /****************************************************************************************/
        private void toolFamily_Click(object sender, EventArgs e)
        { //Bring PanelFamily into PanelAll
            try
            {
                ClearAllPanels();

                if (SMFS.doNewFunFamily)
                {
                    if (editFunFamilyNew == null)
                    {
                        this.Cursor = Cursors.WaitCursor;
                        InitializeFamilyPanelNew();
                        this.Cursor = Cursors.Default;
                    }
                }
                else
                {
                    if (editFunFamily == null)
                    {

                        this.Cursor = Cursors.WaitCursor;
                        InitializeFamilyPanel();
                        this.Cursor = Cursors.Default;
                    }
                }
                if (panelFamilyActive)
                {
                    toolFamily.BackColor = Color.Transparent;
                    this.panelFamily.Hide();
                    panelFamilyActive = false;
                }
                else
                {
                    if (panelFamilyMoved)
                    {
                        toolFamily.BackColor = Color.Yellow;
                        panelFamily.Show();
                        panelFamily.Focus();
                        panelFamilyActive = true;
                        panelFamily.Dock = DockStyle.Fill;
                    }
                    else
                    {
                        this.panelAll.SuspendLayout();
                        this.panelAll.Controls.Add(this.panelFamily);
                        this.panelAll.ResumeLayout(false);
                        this.panelAll.PerformLayout();
                        this.panelFamily.Dock = DockStyle.Fill;
                        this.panelFamily.Show();
                        panelFamilyActive = true;
                        panelFamilyMoved = true;
                        toolFamily.BackColor = Color.Yellow;
                    }
                }
                ResetPanels(panelFamily);
                editFunFamilyNew.FireEventFunShowMain();
                G1.LoadFormInPanel(editFunFamilyNew, this.panelFamily);
                //editFunFamilyNew.FireEventFunShowTab("MAIN");
            }
            catch (Exception ex)
            {

            }
        }
        /****************************************************************************************/
        private string lastFamilyTab = "";
        private string lastLegalTab = "";
        private void ClearAllPanels()
        {
            if (panelFamilyActive)
            {
                lastFamilyTab = tabControl1.SelectedTab.Name;
                toolFamily.BackColor = Color.Transparent;
                this.panelFamily.Hide();
                panelFamilyActive = false;
            }
            if (panelFormsActive)
            {
                toolForms.BackColor = Color.Transparent;
                this.panelForms.Hide();
                panelFormsActive = false;
            }

            if (panelLegalActive)
            {
                lastLegalTab = tabControl1.SelectedTab.Name;
                toolLegal.BackColor = Color.Transparent;
                this.panelLegal.Hide();
                panelLegalActive = false;
            }

            if (panelServicesActive)
            {
                toolServices.BackColor = Color.Transparent;
                this.panelServices.Hide();
                panelServicesActive = false;
            }

            if (panelPaymentsActive)
            {
                toolPayments.BackColor = Color.Transparent;
                this.panelPayments.Hide();
                panelPaymentsActive = false;
            }

            if (panelCustomerActive)
            {
                toolCustomerDemographics.BackColor = Color.Transparent;
                this.panelCustomer.Hide();
                panelCustomerActive = false;
            }

            if (this.panelForms.Visible)
                this.panelForms.Hide();
            if (this.panelLegal.Visible)
                this.panelLegal.Hide();
            if (this.panelPayments.Visible)
                this.panelPayments.Hide();
            if (this.panelServices.Visible)
                this.panelServices.Hide();
            if (this.panelFamily.Visible)
                this.panelFamily.Hide();
            if (this.panelCustomer.Visible)
                this.panelCustomer.Hide();
        }
        /****************************************************************************************/
        private bool panelServicesActive = false;
        private bool panelServicesMoved = false;
        /****************************************************************************************/
        private void toolServices_Click(object sender, EventArgs e)
        {
            try
            {
                ClearAllPanels();

                if (editFunServices == null)
                {
                    this.Cursor = Cursors.WaitCursor;
                    InitializeServicePanel();
                    this.Cursor = Cursors.Default;
                    int width = this.Width;
                    int height = this.Height;
                    //this.Size = new Size(width - 50, height - 50);
                    this.Refresh();
                    this.Update();
                    Application.DoEvents();
                    G1.RefreshAllControls(editFunServices.Controls);
                }
                if (panelServicesActive)
                {
                    toolServices.BackColor = Color.Transparent;
                    this.panelServices.Hide();
                    panelServicesActive = false;
                    this.Refresh();
                    this.Update();
                    Application.DoEvents();
                }
                else
                {
                    if (panelServicesMoved)
                    {
                        toolServices.BackColor = Color.Yellow;
                        panelServices.Show();
                        panelServices.Focus();
                        panelServicesActive = true;
                        panelServices.Dock = DockStyle.Fill;
                        if (editFunServices != null)
                            editFunServices.FireEventFunReloadServices();
                        this.Refresh();
                        this.Update();
                        Application.DoEvents();
                    }
                    else
                    {
                        this.panelAll.SuspendLayout();
                        this.panelAll.Controls.Add(this.panelServices);
                        this.panelAll.ResumeLayout(false);
                        this.panelAll.PerformLayout();
                        this.panelServices.Dock = DockStyle.Bottom;
                        this.panelServices.Dock = DockStyle.Fill;
                        this.panelServices.Show();
                        panelServicesActive = true;
                        panelServicesMoved = true;
                        toolServices.BackColor = Color.Yellow;
                        this.Refresh();
                        this.Update();
                        Application.DoEvents();
                    }
                }
                ResetPanels(panelServices);
                //toolServices.BackColor = Color.Yellow;
            }
            catch (Exception ex)
            {
            }
        }
        /****************************************************************************************/
        private bool panelPaymentsActive = false;
        private bool panelPaymentsMoved = false;
        /****************************************************************************************/
        private void toolPayments_Click(object sender, EventArgs e)
        {

            try
            {
                if ( editFunServices != null)
                {
                    bool modified = editFunServices.FireEventFunServicesModified();
                    if ( modified && workDatabase.ToUpper() == "SMFS" )
                    {
                        MessageBox.Show("***ERROR***\nYou cannot go to payments while Services have been modified and not saved!\nPlease SAVE your changes before moving on.", "Services Data Modified Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                        return;
                    }
                }
                ClearAllPanels();

                if (editFunPayments == null)
                {
                    this.Cursor = Cursors.WaitCursor;
                    InitializePaymentsPanel();
                    this.Cursor = Cursors.Default;
                }
                if (panelPaymentsActive)
                {
                    toolPayments.BackColor = Color.Transparent;
                    this.panelPayments.Hide();
                    panelPaymentsActive = false;
                }
                else
                {
                    if (panelPaymentsMoved)
                    {
                        toolPayments.BackColor = Color.Yellow;
                        panelPayments.Show();
                        panelPayments.Focus();
                        panelPaymentsActive = true;
                        panelPayments.Dock = DockStyle.Fill;
                        if ( editFunPayments != null)
                            editFunPayments.FireEventFunReloadPayments();
                    }
                    else
                    {
                        this.panelAll.SuspendLayout();
                        this.panelAll.Controls.Add(this.panelPayments);
                        this.panelAll.ResumeLayout(false);
                        this.panelAll.PerformLayout();
                        this.panelPayments.Dock = DockStyle.Fill;
                        this.panelPayments.Show();
                        panelPaymentsActive = true;
                        panelPaymentsMoved = true;
                        toolPayments.BackColor = Color.Yellow;
                    }
                }
                ResetPanels(panelPayments);
            }
            catch (Exception ex)
            {
            }
        }
        /****************************************************************************************/
        private bool panelLegalActive = false;
        private bool panelLegalMoved = false;
        /****************************************************************************************/
        private void toolLegal_Click(object sender, EventArgs e)
        {
            try
            {
                ClearAllPanels();

                if (SMFS.doNewFunFamily)
                {
                    if (editFunLegalNew == null)
                    {
                        if (editFunFamilyNew != null )
                        {
                            if ( editFunFamilyNew.FireEventFunServicesModified() )
                            {
                                editFunFamilyNew.FireEventSaveFunServices( true, true );
                            }
                        }
                        this.Cursor = Cursors.WaitCursor;
                        InitializeLegalPanelNew();
                        this.Cursor = Cursors.Default;
                    }
                }
                else
                {
                    if (editFunLegal == null)
                    {
                        this.Cursor = Cursors.WaitCursor;
                        InitializeLegalPanel();
                        this.Cursor = Cursors.Default;
                    }
                }
                if (panelLegalActive)
                {
                    toolLegal.BackColor = Color.Transparent;
                    this.panelLegal.Hide();
                    panelLegalActive = false;
                }
                else
                {
                    if (panelLegalMoved)
                    {
                        toolLegal.BackColor = Color.Yellow;
                        panelLegal.Show();
                        panelLegal.Focus();
                        panelLegalActive = true;
                        panelLegal.Dock = DockStyle.Fill;
                    }
                    else
                    {
                        this.panelAll.SuspendLayout();
                        this.panelAll.Controls.Add(this.panelLegal);
                        this.panelAll.ResumeLayout(false);
                        this.panelAll.PerformLayout();
                        this.panelLegal.Dock = DockStyle.Fill;
                        this.panelLegal.Show();
                        panelLegalActive = true;
                        panelLegalMoved = true;
                        toolLegal.BackColor = Color.Yellow;
                    }
                }
                ResetPanels(panelLegal);
                G1.LoadFormInPanel(editFunLegalNew, this.panelLegal);
                editFunFamilyNew.FireEventFunShowOthers();
                //editFunFamilyNew.FireEventFunShowTab("OTHER");
            }
            catch (Exception ex)
            {
            }
        }
        /****************************************************************************************/
        private bool panelFormsActive = false;
        private bool panelFormsMoved = false;
        /****************************************************************************************/
        private void toolForms_Click(object sender, EventArgs e)
        {
            try
            {
                ClearAllPanels();

                if (editFunForms == null)
                {
                    this.Cursor = Cursors.WaitCursor;
                    InitializeFormsPanel();
                    this.Cursor = Cursors.Default;
                }
                if (panelFormsActive)
                {
                    toolForms.BackColor = Color.Transparent;
                    this.panelForms.Hide();
                    panelFormsActive = false;
                }
                else
                {
                    if (panelFormsMoved)
                    {
                        toolForms.BackColor = Color.Yellow;
                        panelForms.Show();
                        panelForms.Focus();
                        panelFormsActive = true;
                        panelForms.Dock = DockStyle.Fill;
                    }
                    else
                    {
                        this.panelAll.SuspendLayout();
                        this.panelAll.Controls.Add(this.panelForms);
                        this.panelAll.ResumeLayout(false);
                        this.panelAll.PerformLayout();
                        this.panelForms.Dock = DockStyle.Fill;
                        this.panelForms.Show();
                        panelFormsActive = true;
                        panelFormsMoved = true;
                        toolForms.BackColor = Color.Yellow;
                    }
                }
                ResetPanels(panelForms);
            }
            catch (Exception ex)
            {
            }
        }
        /***********************************************************************************************/
        private FunCustomer editFunCustomer = null;
        private bool custModified = false;
        private bool customerModified = false;
        private void InitializeCustomerPanel()
        {
            if (editFunCustomer != null)
                editFunCustomer.Close();
            editFunCustomer = null;
            custModified = false;
            customerModified = false;
            G1.ClearPanelControls(this.panelCustomer);

            editFunCustomer = new FunCustomer(workContract, true );
            editFunCustomer.SomethingChanged += EditFunCustomer_SomethingChanged;
            if (!this.LookAndFeel.UseDefaultLookAndFeel)
            {
                editFunCustomer.LookAndFeel.UseDefaultLookAndFeel = false;
                editFunCustomer.LookAndFeel.SetSkinStyle(this.LookAndFeel.SkinName);
            }
            int left = panelDesign.Left;
            int top = panelDesign.Top;
            int width = panelDesign.Width;
            int height = panelDesign.Height;
            editFunCustomer.SetBounds(left, top, width, height);
            G1.LoadFormInPanel(editFunCustomer, this.panelCustomer);
            this.panelCustomer.SetBounds(left, top, width, height);

            this.panelCustomer.Show();
            toolCustomerDemographics.BackColor = Color.Yellow;
            panelCustomerActive = true;

            this.panelCustomer.Dock = DockStyle.Fill;
            FixVitals();
        }
        /****************************************************************************************/
        private void EditFunCustomer_SomethingChanged(string what)
        {
            if ( what.ToUpper() == "SRVDATE")
            {
                if ( editFunFamilyNew != null )
                    editFunFamilyNew.FireEventServiceDateChanged();
            }
        }
        /****************************************************************************************/
        public DataTable FireEventGetMaster()
        {
            if ( masterTable == null )
            {
                string cmd = "Select * from `relatives` where `contractNumber` = '" + workContract + "';";
                masterTable = G1.get_db_data(cmd);
                masterTable.Columns.Add("num");
                masterTable.Columns.Add("mod");
                masterTable.Columns.Add("sigs", typeof(Bitmap));
                masterTable.Columns.Add("add");
                masterTable.Columns.Add("edit");
            }
            return masterTable;
        }
        /***********************************************************************************************/
        private FunFamilyNew editFunFamilyNew = null;
        private bool familyModifiedNew = false;
        private void InitializeFamilyPanelNew()
        {
            if (masterTable == null)
            {
                string cmd = "Select * from `relatives` where `contractNumber` = '" + workContract + "';";
                masterTable = G1.get_db_data(cmd);
                masterTable.Columns.Add("num");
                masterTable.Columns.Add("mod");
                masterTable.Columns.Add("sigs", typeof(Bitmap));
                masterTable.Columns.Add("add");
                masterTable.Columns.Add("edit");
            }

            if (editFunFamilyNew != null)
                editFunFamilyNew.Close();
            editFunFamilyNew = null;
            familyModifiedNew = false;
            G1.ClearPanelControls(this.panelFamily);

            editFunFamilyNew = new FunFamilyNew(this, workContract, true, false );
            editFunFamilyNew.FamModified += EditFunFamilyNew_FamModified;
            editFunFamilyNew.SomethingChanged += EditFunFamilyNew_SomethingChanged;
            editFunFamilyNew.funFamilyPrint += EditFunFamilyNew_funFamilyPrint;


            if (!this.LookAndFeel.UseDefaultLookAndFeel)
            {
                editFunFamilyNew.LookAndFeel.UseDefaultLookAndFeel = false;
                editFunFamilyNew.LookAndFeel.SetSkinStyle(this.LookAndFeel.SkinName);
            }

            int left = panelDesign.Left;
            int top = panelDesign.Top;
            int width = panelDesign.Width;
            int height = panelDesign.Height;
            editFunFamilyNew.SetBounds(left, top, width, height);

            G1.LoadFormInPanel(editFunFamilyNew, this.panelFamily);

            this.panelFamily.SetBounds(left, top, width, height);

            if (editFunFamilyNew.Visible)
                this.panelFamily.Show();

            this.panelFamily.Dock = DockStyle.Fill;
        }
        /***********************************************************************************************/
        private bool pbChanged = false;
        private bool hpbChanged = false;
        private void EditFunFamilyNew_SomethingChanged(string what)
        {
            if (what.ToUpper() == "PB")
                pbChanged = true;
            else if (what.ToUpper() == "HPB")
                hpbChanged = true;
            if ( editFunLegalNew != null )
            {
                if ( pbChanged || hpbChanged )
                    editFunLegalNew.FireEventFamModified(masterTable);
                pbChanged = false;
                hpbChanged = false;
            }
            if ( what.ToUpper() == "SRVDATE")
            {
                if (editFunCustomer != null)
                    editFunCustomer.FireEventServiceDateChanged();
            }
        }
        /***********************************************************************************************/
        private void EditFunFamilyNew_FamModified(DataTable dt)
        {
            if (editFunFamilyNew != null )
            {
                masterTable = dt;
                editFunFamilyNew.FireEventFamModified(dt);
            }
        }
        /***********************************************************************************************/
        private FunFamily editFunFamily = null;
        private bool familyModified = false;
        private void InitializeFamilyPanel()
        {
            if ( SMFS.doNewFunFamily )
            {
                InitializeFamilyPanelNew();
                return;
            }
            if (editFunFamily != null)
                editFunFamily.Close();
            editFunFamily = null;
            familyModified = false;
            G1.ClearPanelControls(this.panelFamily);

            editFunFamily = new FunFamily(workContract, true );
            if (!this.LookAndFeel.UseDefaultLookAndFeel)
            {
                editFunFamily.LookAndFeel.UseDefaultLookAndFeel = false;
                editFunFamily.LookAndFeel.SetSkinStyle(this.LookAndFeel.SkinName);
            }

            int left = panelDesign.Left;
            int top = panelDesign.Top;
            int width = panelDesign.Width;
            int height = panelDesign.Height;
            editFunFamily.SetBounds(left, top, width, height);

            G1.LoadFormInPanel(editFunFamily, this.panelFamily);

            this.panelFamily.SetBounds(left, top, width, height);

            if (editFunFamily.Visible)
                this.panelFamily.Show();

            this.panelFamily.Dock = DockStyle.Fill;
        }
        /***********************************************************************************************/
        private FunServices editFunServices = null;
        
        private bool funModified = false;
        private bool serialModified = false;
        private void InitializeServicePanel( bool justLoading = false )
        {
            if (editFunServices != null)
                editFunServices.Close();
            editFunServices = null;
            funModified = false;
            serialModified = false;
            G1.ClearPanelControls(this.panelServices);

            editFunServices = new FunServices(this, workContract, true );
            editFunServices.servicesClosing += EditFunServices_servicesClosing;
            editFunServices.serialReleasedClosing += EditFunServices_serialReleasedClosing;
            editFunServices.servicesSizeChanged += EditFunServices_servicesSizeChanged;
            editFunServices.funServicesPrint += EditFunServices_funServicesPrint;
            if (!this.LookAndFeel.UseDefaultLookAndFeel)
            {
                editFunServices.LookAndFeel.UseDefaultLookAndFeel = false;
                editFunServices.LookAndFeel.SetSkinStyle(this.LookAndFeel.SkinName);
            }
            int left = panelDesign.Left;
            int top = panelDesign.Top;
            int width = panelDesign.Width;
            int height = panelDesign.Height;
            editFunServices.SetBounds(left, top, width, height);

            G1.LoadFormInPanel(editFunServices, this.panelServices);

            this.panelServices.SetBounds(left, top, width, height);

            if ( !justLoading )
            {
                if (editFunServices.Visible)
                    this.panelServices.Show();
            }

            this.panelServices.Dock = DockStyle.Fill;


            //int left = panelDesign.Left;
            //int top = panelDesign.Top;
            //int width = panelDesign.Width;
            //int height = editFunServices.Height;
            //this.panelServices.SetBounds(left, top, width, height);
            ////this.panelServices.Dock = DockStyle.Top;
            //this.panelServices.Dock = DockStyle.Fill;

            //editFunServices.Dock = DockStyle.Fill;
            //this.panelDesign.Dock = DockStyle.Fill;
            ////panelServices.TopMost = true;
            //if (!justLoading)
                //panelServices.Visible = true;
        }
        /***********************************************************************************************/
        private void EditFunServices_funServicesPrint(string who, DevExpress.XtraGrid.GridControl dgv1)
        {
            string myTitle = BuildTitle();

            DataTable dt = (DataTable)dgv1.DataSource;
            if (dt.Rows.Count > 0)
            {
                DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView mainGrid = (DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView)dgv1.MainView;
                title = who + " " + myTitle;

                printPreviewMenuItem(mySender, myEventArgs, dgv1, false );
            }
        }
        /***********************************************************************************************/
        private void EditFunServices_servicesSizeChanged()
        {
            this.panelServices.Refresh();
        }
        /***********************************************************************************************/
        private void EditFunServices_serialReleasedClosing(string record, double amountFiled, double amountReceived)
        {
            serialModified = true;
            custExtendedRecord = record;
        }
        /***********************************************************************************************/
        private bool serviceModified = false;
        private void EditFunServices_servicesClosing(string record, double amountFiled, double amountReceived)
        {
            serviceModified = true;
            custExtendedRecord = record;
        }
        /***********************************************************************************************/
        private FunPayments editFunPayments = null;
        private bool payModified = false;
        private void InitializePaymentsPanel( bool justLoading = false )
        {
            if (editFunPayments != null)
                editFunPayments.Close();
            editFunPayments = null;
            payModified = false;
            G1.ClearPanelControls(this.panelPayments);

            editFunPayments = new FunPayments(this, workContract, workPayer );
            editFunPayments.paymentClosing += EditFunPayments_paymentClosing;
            if (!this.LookAndFeel.UseDefaultLookAndFeel)
            {
                editFunPayments.LookAndFeel.UseDefaultLookAndFeel = false;
                editFunPayments.LookAndFeel.SetSkinStyle(this.LookAndFeel.SkinName);
            }

            int left = panelDesign.Left;
            int top = panelDesign.Top;
            int width = panelDesign.Width;
            int height = panelDesign.Height;
            editFunPayments.SetBounds(left, top, width, height);

            G1.LoadFormInPanel(editFunPayments, this.panelPayments);

            this.panelPayments.SetBounds(left, top, width, height);

            if (!justLoading)
            {
                if (editFunPayments.Visible)
                    this.panelPayments.Show();
            }

            this.panelPayments.Dock = DockStyle.Fill;
        }
        /***********************************************************************************************/
        private double totalFiled = 0D;
        private double totalReceived = 0D;
        private double totalDiscount = 0D;
        private double totalGrowth = 0D;
        private string custExtendedRecord = "";
        private bool paymentModified = false;
        private void EditFunPayments_paymentClosing(string record, double amountFiled, double amountReceived, double amountDiscount, double amountGrowth )
        {
            paymentModified = true;
            custExtendedRecord = record;
            totalFiled = amountFiled;
            totalReceived = amountReceived;
            totalDiscount = amountDiscount;
            totalGrowth = amountGrowth;
        }
        /***********************************************************************************************/
        private FunFamilyNew editFunLegalNew = null;
        private bool legalModifiedNew = false;
        private void InitializeLegalPanelNew()
        {
            if (masterTable == null)
            {
                string cmd = "Select * from `relatives` where `contractNumber` = '" + workContract + "';";
                masterTable = G1.get_db_data(cmd);
                masterTable.Columns.Add("num");
                masterTable.Columns.Add("mod");
                masterTable.Columns.Add("sigs", typeof(Bitmap));
                masterTable.Columns.Add("add");
                masterTable.Columns.Add("edit");
            }

            if (editFunFamilyNew == null)
            {
                InitializeFamilyPanelNew();
            }

            if (editFunLegalNew != null)
                editFunLegalNew.Close();
            editFunLegalNew = null;
            legalModifiedNew = false;
            G1.ClearPanelControls(this.panelLegal);

            editFunLegalNew = editFunFamilyNew;

            //editFunLegalNew = new FunFamilyNew(this, workContract, true, true);
            editFunLegalNew.PallModified += EditFunLegalNew_PallModified;
            editFunLegalNew.HPallModified += EditFunLegalNew_HPallModified;

            if (!this.LookAndFeel.UseDefaultLookAndFeel)
            {
                editFunLegalNew.LookAndFeel.UseDefaultLookAndFeel = false;
                editFunLegalNew.LookAndFeel.SetSkinStyle(this.LookAndFeel.SkinName);
            }
            G1.LoadFormInPanel(editFunLegalNew, this.panelLegal);

            editFunFamilyNew.FireEventFunShowOthers();

            G1.LoadFormInPanel(editFunLegalNew, this.panelLegal);
            this.panelLegal.Dock = DockStyle.Top;
        }
        /***********************************************************************************************/
        private void EditFunLegalNew_PallModified(DataTable dt)
        {
            if ( editFunFamilyNew != null)
            {
                //masterTable = dt;
                editFunFamilyNew.FireEventPallModified(dt);
            }
        }
        /***********************************************************************************************/
        private void EditFunLegalNew_HPallModified(DataTable dt)
        {
            if (editFunFamilyNew != null)
            {
                //masterTable = dt;
                editFunFamilyNew.FireEventHPallModified(dt);
            }
        }
        /***********************************************************************************************/
        private FunFamily editFunLegal = null;
        private bool legalModified = false;
        private void InitializeLegalPanel()
        {
            if (editFunLegal != null)
                editFunLegal.Close();
            editFunLegal = null;
            legalModified = false;
            G1.ClearPanelControls(this.panelLegal);

            editFunLegal = new FunFamily(workContract, true, true );
            if (!this.LookAndFeel.UseDefaultLookAndFeel)
            {
                editFunLegal.LookAndFeel.UseDefaultLookAndFeel = false;
                editFunLegal.LookAndFeel.SetSkinStyle(this.LookAndFeel.SkinName);
            }
            G1.LoadFormInPanel(editFunLegal, this.panelLegal);
            this.panelLegal.Dock = DockStyle.Top;
        }
        /***********************************************************************************************/
        private FunForms editFunForms = null;
        private bool formsModified = false;
        private void InitializeFormsPanel()
        {
            if (editFunForms != null)
                editFunForms.Close();
            editFunForms = null;
            formsModified = false;
            G1.ClearPanelControls(this.panelForms);

            Rectangle rect = panelForms.Bounds;
            int height = rect.Height * 3;
            panelForms.SetBounds(rect.Left, rect.Top, rect.Width, height);

            if (editFunServices == null)
            {
                PleaseWait pleaseForm = new PleaseWait("Please Wait!\nPreparing Customer Services!");
                pleaseForm.Show();
                pleaseForm.Refresh();

                InitializeServicePanel(true);

                pleaseForm.FireEvent1();
                pleaseForm.Dispose();
                pleaseForm = null;
            }
            if (editFunPayments == null)
            {
                InitializePaymentsPanel(true); // This should grab the services from previous load panel
            }

            workServicesDt = editFunServices.FireEventFunServicesReturn();
            workPaymentsDt = editFunPayments.FireEventFunPaymentsReturn();

            editFunForms = new FunForms(this, workContract, workServicesDt, workPaymentsDt);
            if (!this.LookAndFeel.UseDefaultLookAndFeel)
            {
                editFunForms.LookAndFeel.UseDefaultLookAndFeel = false;
                editFunForms.LookAndFeel.SetSkinStyle(this.LookAndFeel.SkinName);
            }

            int left = panelDesign.Left;
            int top = panelDesign.Top;
            int width = panelDesign.Width;
            height = panelDesign.Height;
            editFunForms.SetBounds(left, top, width, height);

            G1.LoadFormInPanel(editFunForms, this.panelForms);

            this.panelForms.SetBounds(left, top, width, height);

            if (editFunForms.Visible)
                this.panelForms.Show();

            this.panelForms.Dock = DockStyle.Fill;
        }
        /****************************************************************************************/
        private void EditCustomer_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (editFunPayments != null)
            {
                bool okay = editFunPayments.FireEventFunServicesOkayToClose();
                if (!okay)
                {
                    e.Cancel = true;
                    return;
                }
            }

            bool doSave = true;
            if (checkForModified())
            {
                DialogResult result = MessageBox.Show("***Question***\nData has been modified!\nWould you like a chance to save your changes?", "Data Modified Dialog", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                if (result == DialogResult.Cancel)
                {
                    if (editFunForms != null)
                        editFunForms.FireEventFunFormsBringToFront();
                    e.Cancel = true;
                    return;
                }
                if ( result == DialogResult.No )
                {
                    G1.AddToAudit(LoginForm.username, "Funerals", "Exiting Funeral","Changes Made, Did Not Save Changes!", workContract );
                    checkForModified(true);
                }
                if (result == DialogResult.Yes || serialModified)
                    SaveAllData();
                doSave = false;
            }
            else
                doSave = false;
            OnCustClosing( doSave );

            if (SMFS.SMFS_MainForm != null)
                SMFS.SMFS_MainForm.WindowState = FormWindowState.Normal;
        }
        /***************************************************************************************/
        public delegate void d_void_CustClosing(string contractNumber, double amountFiled, double amountReceived);
        public event d_void_CustClosing custClosing;
        protected void OnCustClosing( bool doSave = false )
        {
            ClosePartialForms();
            if (doSave)
            {
                if (paymentModified || serviceModified || customerModified || serialModified)
                {
                    if (!String.IsNullOrWhiteSpace(custExtendedRecord))
                        custClosing?.Invoke(workContract, totalFiled, totalReceived);
                    else if (customerModified)
                        custClosing?.Invoke(workContract, totalFiled, totalReceived);
                    else if (serialModified)
                        custClosing?.Invoke(workContract, totalFiled, totalReceived);
                }
            }
            else
            {
                if (custClosing != null)
                    custClosing?.Invoke(workContract, -1D, -1D);
            }
        }
        /***************************************************************************************/
        public delegate void d_void_CustRename(string contractNumber );
        public event d_void_CustRename custRename;
        protected void OnCustRename( string s )
        {
            ClosePartialForms();
            if (custRename != null)
            {
                string contract = workContract;
                string[] Lines = s.Split(' ');
                if (Lines.Length >= 3)
                    contract = Lines[2].Trim();
                custRename?.Invoke( contract );
            }
            this.Close();
        }
        /****************************************************************************************/
        private void ClosePartialForms ()
        {
            if (editFunPayments != null)
                editFunPayments.Close();
            if (editFunServices != null)
                editFunServices.Close();
            if (editFunForms != null)
                editFunForms.Close();
            if (editFunFamilyNew != null)
                editFunFamilyNew.Close();
            editFunPayments = null;
            editFunServices = null;
            editFunForms = null;
            editFunFamilyNew = null;
        }
        /****************************************************************************************/
        private void CloseAllForms()
        {
            if (editFunForms != null)
                editFunForms.Close();
            if (editFunPayments != null)
                editFunPayments.Close();
            if (editFunFamily != null)
                editFunFamily.Close();
            if (editFunFamilyNew != null)
                editFunFamilyNew.Close();
            if (editFunServices != null)
                editFunServices.Close();
            if (editFunCustomer != null)
                editFunCustomer.Close();

            editFunForms = null;
            editFunPayments = null;
            editFunFamily = null;
            editFunFamilyNew = null;
            editFunServices = null;
            editFunCustomer = null;
        }
        /****************************************************************************************/
        private void SaveAllData()
        {
            if (familyModified)
            {
                if ( editFunFamily != null )
                    editFunFamily.FireEventSaveFunServices(true);
            }
            if (familyModifiedNew)
            {
                if (editFunFamilyNew != null)
                    editFunFamilyNew.FireEventSaveFunServices(true);
            }
            if (funModified || serialModified )
                editFunServices.FireEventSaveFunServices(true);
            if (custModified)
                editFunCustomer.FireEventSaveFunServices(true);
            if (payModified)
                editFunPayments.FireEventSaveFunServices(true);
            if (legalModified)
                editFunLegal.FireEventSaveFunServices(true);
            if (formsModified)
                editFunForms.FireEventSaveFunServices(true);
        }
        /****************************************************************************************/
        private bool checkForModified( bool showAudit = false )
        {
            bool modified = false;
            if (editFunFamily != null)
            {
                familyModified = editFunFamily.FireEventFunServicesModified();
                if (familyModified)
                {
                    modified = true;
                    if ( showAudit )
                        G1.AddToAudit(LoginForm.username, "Funerals", "Exiting Funeral", "Family Changes Made, Did Not Save Changes!", workContract);
                }
            }
            if (editFunFamilyNew != null)
            {
                familyModifiedNew = editFunFamilyNew.FireEventFunServicesModified();
                if (familyModifiedNew)
                {
                    modified = true;
                    if (showAudit)
                        G1.AddToAudit(LoginForm.username, "Funerals", "Exiting Funeral", "Family Changes Made, Did Not Save Changes!", workContract);
                }
            }
            if (editFunServices != null)
            {
                funModified = editFunServices.FireEventFunServicesModified();
                if (funModified)
                {
                    modified = true;
                    if (showAudit)
                        G1.AddToAudit(LoginForm.username, "Funerals", "Exiting Funeral", "Service Changes Made, Did Not Save Changes!", workContract);
                }
                else if (serialModified)
                {
                    modified = true;
                    if (showAudit)
                        G1.AddToAudit(LoginForm.username, "Funerals", "Exiting Funeral", "Serial Number Changes Made, Did Not Save Changes!", workContract);
                }
            }
            if (editFunCustomer != null)
            {
                custModified = editFunCustomer.FireEventFunServicesModified();
                if (custModified)
                {
                    modified = true;
                    customerModified = true;
                    if (showAudit)
                        G1.AddToAudit(LoginForm.username, "Funerals", "Exiting Funeral", "Demographics Changes Made, Did Not Save Changes!", workContract);
                }
            }
            if (editFunPayments != null)
            {
                payModified = editFunPayments.FireEventFunServicesModified();
                if (payModified)
                {
                    modified = true;
                    if (showAudit)
                        G1.AddToAudit(LoginForm.username, "Funerals", "Exiting Funeral", "Payment Changes Made, Did Not Save Changes!", workContract);
                }
            }
            if (editFunLegal != null)
            {
                legalModified = editFunLegal.FireEventFunServicesModified();
                if (legalModified)
                {
                    modified = true;
                    if (showAudit)
                        G1.AddToAudit(LoginForm.username, "Funerals", "Exiting Funeral", "Legal Member Changes Made, Did Not Save Changes!", workContract);
                }
            }
            if (editFunForms != null)
            {
                formsModified = editFunForms.FireEventFunServicesModified();
                if (formsModified)
                {
                    modified = true;
                    if (showAudit)
                        G1.AddToAudit(LoginForm.username, "Funerals", "Exiting Funeral", "Form Changes Made, Did Not Save Changes!", workContract);
                }
            }
            return modified;
        }
        /****************************************************************************************/
        private void btnContracts_Click(object sender, EventArgs e)
        {
            string cmd = "Select * from `customers` where `contractNumber` = '" + workContract + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return;
            string ssn = dt.Rows[0]["ssn"].ObjToString();
            cmd = "Select * from `customers` where `ssn` = '" + ssn + "';";
            dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return;
            if ( dt.Rows.Count == 1)
            {
                string filename = dt.Rows[0]["agreementFile"].ObjToString();
                string firstName = dt.Rows[0]["firstName"].ObjToString();
                string lastName = dt.Rows[0]["lastName"].ObjToString();
                string title = "Agreement for (" + workContract + ") " + firstName + " " + lastName;
                string record = this.btnContracts.Tag.ObjToString();
                if (record != "-1")
                    Customers.ShowPDfImage(record, title, filename);
                return;
            }

            string contractNumber = "";
            DataTable picDt = null;
            DataTable preneedDt = new DataTable();
            preneedDt.Columns.Add("record");
            preneedDt.Columns.Add("contractNumber");
            DataRow dRow = null;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                cmd = "Select * from `pdfimages` where `contractNumber` = '" + contractNumber + "';";
                picDt = G1.get_db_data(cmd);
                if (picDt.Rows.Count > 0)
                {
                    dRow = preneedDt.NewRow();
                    dRow["record"] = picDt.Rows[0]["record"].ObjToString();
                    dRow["contractNumber"] = picDt.Rows[0]["contractNumber"].ObjToString();
                    preneedDt.Rows.Add(dRow);
                }
            }
            ContractList contractForm = new ContractList(preneedDt);
            contractForm.Show();
        }
        /****************************************************************************************/
        public class CustomPanel : System.Windows.Forms.Panel
        {
            protected override System.Drawing.Point ScrollToControl(System.Windows.Forms.Control activeControl)
            {
                // Returning the current location prevents the panel from
                // scrolling to the active control when the panel loses and regains focus
                return this.DisplayRectangle.Location;
            }
        }
        /****************************************************************************************/
        private void EditCustomer_GotFocus(object sender, EventArgs e)
        { // None of this worked

            //            this.panelAll.VerticalScroll.Value = 0;
            //this.panelAll.VerticalScroll.Maximum = 0;

            //if (MainControl == null)
            //    return;
            //ResetPanels((Panel)MainControl);
            //this.panelAll.VerticalScroll.Value = ScrollPosition;
            //MainControl = null;
        }
        /****************************************************************************************/
        private Control MainControl = null;
        private int ScrollPosition = 0;
        private void EditCustomer_LostFocus(object sender, EventArgs e)
        {
            //Control control = null;
            //string name = "";
            //for (int i = 0; i < this.panelAll.Controls.Count; i++)
            //{
            //    control = this.panelAll.Controls[i];
            //    if (control.Visible)
            //    {
            //        name = control.Tag.ObjToString().ToUpper();
            //        if (name == "MYPANEL")
            //        {
            //            if (control.Focused)
            //            {
            //                MainControl = control;
            //                ScrollPosition = panelAll.VerticalScroll.Value;
            //                break;
            //            }
            //        }
            //    }
            //}
        }
        /****************************************************************************************/
        //Point scrolledPoint = new Point();
        //private void panelAll_MouseMove(object sender, MouseEventArgs e)
        //{
        //    scrolledPoint = new Point(e.X - panelAll.AutoScrollPosition.X, e.Y - panelAll.AutoScrollPosition.Y);
        //    int f = panelAll.VerticalScroll.Value;
        //    textBox2.Text = f.ToString();
        //    textBox2.Refresh();
        //}
        /****************************************************************************************/
        //[System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
        //protected override void WndProc(ref Message m)
        //{
        //    // 0x210 is WM_PARENTNOTIFY
        //    // 513 is WM_LBUTTONCLICK
        //    if (m.Msg == 0x210 && m.WParam.ToInt32() == 513)
        //    {
        //        //                int actualHeight = FindMainWindowHeight();

        //        // get the clicked position
        //        var x = (int)(m.LParam.ToInt32() & 0xFFFF);
        //        var y = (int)(m.LParam.ToInt32() >> 16);

        //        // get the clicked control
        //        var childControl = this.GetChildAtPoint(new Point(x, y));
        //        childControl.Focus();

        //        // call onClick (which fires Click event)
        //        //Point point = panelAll.AutoScrollPosition;
        //        //panelAll.AutoScroll = false;
        //        //panelAll.VerticalScroll.Enabled = false;
        //        //                OnClick(EventArgs.Empty);
        //        //                panelAll.AutoScrollPosition = lastPoint;
        //        //panelAll.VerticalScroll.Enabled = true;
        //        //panelAll.AutoScroll = true;
        //        //if (lastControl != null)
        //        //{
        //        //    ResetPanels((Panel)lastControl);
        //        //    panelAll.AutoScrollPosition = lastPoint;
        //        //    panelAll.AutoScroll = true;
        //        //}

        //        // do something else...
        //    }
        //    base.WndProc(ref m);
        //}
        /****************************************************************************************/
        private void panelAll_Paint(object sender, PaintEventArgs e)
        {
        }
        protected override Point ScrollToControl(Control activeControl)
        {
            //           return this.AutoScrollPosition;
            return this.DisplayRectangle.Location;
        }
        /****************************************************************************************/
        private void LoadFuneralInfo()
        {
            try
            {
                string cmd = "Select * from `fcust_extended` where `contractNumber` = '" + workContract + "';";
                DataTable dt = G1.get_db_data(cmd);
                if (dt.Rows.Count > 0)
                {
                    string director = dt.Rows[0]["Funeral Director"].ObjToString();
                    string arranger = dt.Rows[0]["Funeral Arranger"].ObjToString();
                    RTF_Stuff.activeFuneralHomeDirector = director;
                    RTF_Stuff.activeFuneralHomeArranger = arranger;
                    if (!String.IsNullOrWhiteSpace(director))
                        lblFuneralDirector.Text = "Funeral Director : " + director;
                    if (!String.IsNullOrWhiteSpace(arranger))
                        lblFuneralArranger.Text = "Funeral Arranger : " + arranger;
                }
            }
            catch (Exception ex)
            {

            }
        }
        /****************************************************************************************/
        private void funeralDirectorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string director = "";
                using (Ask askForm = new Ask("Enter Name of Funeral Director?"))
                {
                    askForm.Text = "";
                    askForm.ShowDialog();
                    if (askForm.DialogResult != System.Windows.Forms.DialogResult.OK)
                        return;
                    director = askForm.Answer;
                    if (String.IsNullOrWhiteSpace(director))
                        return;
                }
                lblFuneralDirector.Text = "Funeral Director : " + director;
                string cmd = "Select * from `fcust_extended` where `contractNumber` = '" + workContract + "';";
                DataTable dt = G1.get_db_data(cmd);
                if (dt.Rows.Count > 0)
                {
                    string record = dt.Rows[0]["record"].ObjToString();
                    G1.update_db_table("fcust_extended", "record", record, new string[] { "Funeral Director", director });
                }
            }
            catch (Exception ex)
            {

            }
        }
        /****************************************************************************************/
        private void funeralArrangerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string director = "";
                using (Ask askForm = new Ask("Enter Name of Funeral Arranger?"))
                {
                    askForm.Text = "";
                    askForm.ShowDialog();
                    if (askForm.DialogResult != System.Windows.Forms.DialogResult.OK)
                        return;
                    director = askForm.Answer;
                    if (String.IsNullOrWhiteSpace(director))
                        return;
                }
                lblFuneralArranger.Text = "Funeral Arranger : " + director;
                string cmd = "Select * from `fcust_extended` where `contractNumber` = '" + workContract + "';";
                DataTable dt = G1.get_db_data(cmd);
                if (dt.Rows.Count > 0)
                {
                    string record = dt.Rows[0]["record"].ObjToString();
                    G1.update_db_table("fcust_extended", "record", record, new string[] { "Funeral Arranger", director });
                }
            }
            catch (Exception ex)
            {

            }
        }
        /****************************************************************************************/
        private void funeralHomeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (;;)
            {
                using (FuneralHomeSelect funSelect = new FuneralHomeSelect())
                {
                    funSelect.ShowDialog();
                }
                if (String.IsNullOrWhiteSpace(LoginForm.activeFuneralHomeKeyCode))
                {
                    DialogResult result = MessageBox.Show("***Warning*** Are you sure you DO NOT WANT to select an Active Funeral Home?", "Active Funeral Home Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (result == DialogResult.Yes)
                        return;
                }
                break;
            }
            string cmd = "Select * from `funeralhomes` where `keycode` = '" + LoginForm.activeFuneralHomeKeyCode + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
            {
                MessageBox.Show("***ERROR*** Invalid Funeral Home!");
                this.Close();
                return;
            }

            string oldFuneralHome = activeFuneralHomeName;

            activeFuneralHomeName = dt.Rows[0]["LocationCode"].ObjToString();
            if (String.IsNullOrWhiteSpace(activeFuneralHomeName))
                activeFuneralHomeName = dt.Rows[0]["name"].ObjToString();
            activeFuneralHomeGroup = dt.Rows[0]["groupname"].ObjToString();
            activeFuneralHomeCasketGroup = dt.Rows[0]["casketgroup"].ObjToString();

            if ( activeFuneralHomeName != oldFuneralHome )
            {
                G1.AddToAudit(LoginForm.username, "Funerals", "Active Funeral Home", "Changed from " + oldFuneralHome + " to " + activeFuneralHomeName );
            }

            this.Text = originalTitle;
            this.Text += " [" + activeFuneralHomeName + "]  [Group: " + activeFuneralHomeGroup + "]  [CasketGroup: " + activeFuneralHomeCasketGroup + "]";

            txtFuneralHome.Text = activeFuneralHomeName;
            txtGPLGroup.Text = activeFuneralHomeGroup;
            txtCasketGroup.Text = activeFuneralHomeCasketGroup;

            EditCustomer.activeFuneralHomeGroup = activeFuneralHomeGroup;
            EditCustomer.activeFuneralHomeCasketGroup = activeFuneralHomeCasketGroup;
            EditCustomer.activeFuneralHomeName = activeFuneralHomeName;

            if (editFunFamily != null)
                familyModified = editFunFamily.FireEventFunServicesModified();
            if (editFunFamilyNew != null)
                familyModifiedNew = editFunFamilyNew.FireEventFunServicesModified();

            if (editFunCustomer != null)
                custModified = editFunCustomer.FireEventFunServicesModified();

            ClosePartialForms();
            //InitializeCustomerPanel();

            toolServices.BackColor = Color.Transparent;
            toolPayments.BackColor = Color.Transparent;
            if (editFunFamily != null && familyModified )
                editFunFamily.FireEventFunServicesSetModified();
            if (editFunFamilyNew != null && familyModifiedNew)
                editFunFamilyNew.FireEventFunServicesSetModified();
            if ( editFunCustomer != null && custModified )
                editFunCustomer.FireEventFunServicesSetModified();

        }
        /****************************************************************************************/
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            if (editFunServices != null)
            {
                bool modified = editFunServices.FireEventFunServicesModified();
                if (modified && workDatabase.ToUpper() == "SMFS")
                {
                    if (!G1.RobbyServer)
                    {
                        MessageBox.Show("***ERROR***\nYou cannot go to G&S Contract while Services have been modified and not saved!\nPlease SAVE your changes before moving on.", "Services Data Modified Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                        return;
                    }
                }
            }

            this.Cursor = Cursors.WaitCursor;
            if (editFunServices == null)
                InitializeServicePanel( true );
            //if (editFunPayments == null)
            //    InitializePaymentsPanel( true );

            workServicesDt = editFunServices.FireEventFunServicesReturn();
            if ( editFunPayments != null)
                workPaymentsDt = editFunPayments.FireEventFunPaymentsReturn();
            else
            {
                string cmd = "Select * from `cust_payments` where `contractNumber` = '" + workContract + "';";
                workPaymentsDt = G1.get_db_data(cmd);

                workPaymentsDt.Columns.Add("contractValue", Type.GetType("System.Double"));
            }

            //if (G1.RobbyServer)
            //{
            //    Contract2 conActive = new Contract2(workContract, workServicesDt, workPaymentsDt);
            //    conActive.Show();
            //}
            //else
            //{

            PleaseWait pleaseForm = G1.StartWait("Please Wait!\nGenerating G&&S Contract!!!");

            //this.Hide();
            this.Visible = false;
                Contract1 conActive = new Contract1(workContract, workServicesDt, workPaymentsDt);
                conActive.GSDone += ConActive_GSDone;
                conActive.ShowDialog();
            this.Visible = true;
            this.Invalidate();
            this.Refresh();
            //this.Show();
            G1.StopWait(ref pleaseForm);

            //}
            //Contract2 conActive = new Contract2(workContract, workServicesDt, workPaymentsDt);
            //conActive.Show();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void ConActive_GSDone(bool printed)
        {
            tblLeft.Refresh();
            tblLeft.Invalidate();

            toolFamily.Invalidate();
            toolServices.Invalidate();
            toolPayments.Invalidate();

            this.Invalidate();
            int left = this.Left;
            int top = this.Top;
            int width = this.Width;
            int height = this.Height;

            this.SetBounds(left-10, top, width, height);
            this.Invalidate();
            this.Refresh();
            //this.SetBounds(left, top, width, height);
            //this.Invalidate();
            //this.Refresh();
            if (editFunServices != null)
            {
                editFunServices.Invalidate();
                editFunServices.FireEventFunReloadServices();
            }
            if (!printed)
                return;
            savedContractsMenu.Visible = true;
            this.Refresh();
        }
        /****************************************************************************************/
        private void EditCust_SizeChanged(object sender, EventArgs e)
        {
//            ResetPanels((Panel)MainControl);
            if (editFunCustomer != null)
            {
                int left = panelDesign.Left;
                int top = panelDesign.Top;
                int width = panelDesign.Width;
                int height = panelDesign.Height;
                editFunCustomer.SetBounds(left, top, width, height);
                //G1.LoadFormInPanel(editFunCustomer, this.panelCustomer);
                this.panelCustomer.SetBounds(left, top, width, height);

                if ( editFunCustomer.Visible )
                    this.panelCustomer.Show();

                this.panelCustomer.Dock = DockStyle.Fill;
                FixVitals();
            }
            if ( editFunServices != null)
            {
                int left = panelDesign.Left;
                int top = panelDesign.Top;
                int width = panelDesign.Width;
                int height = panelDesign.Height;
                //editFunServices.SetBounds(left, top, width, height);
                ////G1.LoadFormInPanel(editFunCustomer, this.panelCustomer);
                //this.panelServices.SetBounds(left, top, width, height);

                if (editFunServices.Visible)
                {
                    this.panelServices.Show();
                    editFunServices.Refresh();
                }
                tblLeft.Refresh();
                tlbMain.Refresh();
                this.panelServices.Dock = DockStyle.Fill;
                this.panelServices.Refresh();
                this.Refresh();
                panelAll.Refresh();
                if (this.WindowState != mLastState)
                {
                    mLastState = this.WindowState;
                    if (this.WindowState == FormWindowState.Normal)
                    {
                        this.SetBounds(left, top, width + 10, height); // ramma zamma bamma
                        this.Refresh();
                    }
                }
            }
        }
        /****************************************************************************************/
        private void FixVitals ()
        {
            if (editFunCustomer == null)
                return;
            string name = "";
            Control control = null;
            Control control2 = null;
            bool found = false;
            for ( int i=0; i<editFunCustomer.Controls.Count; i++ )
            {
                control = editFunCustomer.Controls[i];
                name = control.Name.ToUpper();
                if ( name == "PANELALL")
                {
                    for ( int j=0; j<control.Controls.Count; j++)
                    {
                        control2 = control.Controls[j];
                        name = control2.Name.ToUpper();
                        if ( name == "PANELVITALS")
                        {
                            int left = control2.Left;
                            int top = control2.Top;
                            int width = control2.Width;
                            if (originalPanelVitalsHeight <= 0)
                                originalPanelVitalsHeight = control2.Height;
                            //int height = panelCustomer.Height - originalPanelVitalsHeight;
                            int height = this.Height - originalPanelVitalsHeight;
                            if (height <= 0)
                                height = originalPanelVitalsHeight;
                            control2.SetBounds(left, top, width, height);
                            found = true;
                            break;
                        }
                        if (found)
                            break;
                    }
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
            savedContractsMenu.Visible = true;
            return true;
        }
        /****************************************************************************************/
        private void savedContractsMenu_Click(object sender, EventArgs e)
        {
            string cmd = "Select * from `lapse_list` where `contractNumber` = '" + workContract + "' AND `detail` = 'Goods and Services';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return;
            string lines = "";
            DateTime date = DateTime.Now;
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                date = dt.Rows[i]["noticeDate"].ObjToDateTime();
                lines += date.ToString("MM/dd/yyyy") + "\n";
            }
            using (SelectFromList listForm = new SelectFromList(lines, false, false, true ))
            {
                listForm.Text = "Select Goods and Services Contract to Display";
                listForm.ListDone += ListForm_ListDone;
                listForm.ShowDialog();
            }
        }
        /****************************************************************************************/
        private void ListForm_ListDone(string s)
        {
            if (String.IsNullOrWhiteSpace(s))
                return;
            bool delete = false;
            if ( s.IndexOf ( "~") > 0 )
            {
                string[] Lines = s.Split('~');
                if ( Lines.Length > 1 )
                {
                    s = Lines[0].Trim();
                    if (Lines[1].Trim().ToUpper() == "DELETE")
                        delete = true;
                }
            }
            DateTime date = s.ObjToDateTime();
            string noticeDate = date.ToString("yyyy-MM-dd");
            string cmd = "Select * from `lapse_list` where `contractNumber` = '" + workContract + "' AND `detail` = 'Goods and Services' AND `noticeDate` = '" + noticeDate + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return;
            string record = dt.Rows[0]["noticeRecord"].ObjToString();
            if ( delete )
            {
                G1.delete_db_table("lapse_notices", "record", record);
                record = dt.Rows[0]["record"].ObjToString();
                G1.delete_db_table("lapse_list", "record", record);
                return;
            }
            string detail = dt.Rows[0]["detail"].ObjToString();

            string str = G1.get_db_blob("lapse_notices", record, "image");
            if (str.IndexOf("PDF") > 0)
            {
                try
                {
                    string command = "Select `image` from `lapse_notices` where `record` = '" + record + "';";
                    MySqlCommand cmd1 = new MySqlCommand(command, G1.conn1);
                    cmd1.Connection.Open();
                    try
                    {
                        using (MySqlDataReader dR = cmd1.ExecuteReader(System.Data.CommandBehavior.Default))
                        {
                            if (dR.Read())
                            {
                                byte[] fileData = (byte[])dR.GetValue(0);
                                this.Cursor = Cursors.WaitCursor;
                                ViewPDF viewForm = new ViewPDF(detail, record, workContract, fileData);
                                viewForm.TopMost = true;
//                                viewForm.PdfDone += ViewForm_PdfDone;
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
                catch (Exception ex)
                {
                }
            }
        }
        /****************************************************************************************/
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        /****************************************************************************************/
        private void changeLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            string contract = workContract;
            FuneralsChanges fForm = new FuneralsChanges( contract );
            fForm.Show();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private int pageMarginLeft = 0;
        private int pageMarginRight = 0;
        private int pageMarginTop = 0;
        private int pageMarginBottom = 0;
        /***********************************************************************************************/
        private void printPreviewMenuItem(object sender, EventArgs e, DevExpress.XtraGrid.GridControl dgv, bool landscape = false )
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
            printableComponentLink1.Landscape = landscape;

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
        private void printMenuItem(object sender, EventArgs e, DevExpress.XtraGrid.GridControl dgv, bool landscape = false )
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
            printableComponentLink1.Landscape = landscape;

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
            if (!String.IsNullOrWhiteSpace(title))
                Printer.DrawQuad(4, 8, 6, 4, title, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            else
                Printer.DrawQuad(6, 8, 4, 4, "Print Details", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);


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
        /****************************************************************************************/
        private string BuildTitle()
        {
            string title = "";
            string contract = "";
            string serviceId = "";

            string str = this.Text;
            string temp = "";
            string[] Lines = str.Split(')');
            if (Lines.Length > 0)
            {
                temp = Lines[0].Trim();
                temp = temp.Replace("(", "");
                temp = temp.Replace(")", "");
                contract = temp;
            }

            int idx = str.IndexOf(":");
            if (idx > 0)
            {
                str = str.Substring(idx);
                idx = str.IndexOf("[");
                if (idx > 0)
                    str = str.Substring(0, idx);
                serviceId = str;
                serviceId = serviceId.Replace(":", "");
                serviceId = serviceId.Trim();
            }

            title = contract + " " + serviceId;
            return title;
        }
        /****************************************************************************************/
        private string title = "";
        private object mySender = null;
        private EventArgs myEventArgs = null;
        private void printPreviewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string myTitle = BuildTitle();
            mySender = sender;
            myEventArgs = e;

            if (panelCustomerActive)
            {
                DevExpress.XtraGrid.GridControl dgv = editFunCustomer.FireEventPrintPreview();
                if (dgv != null)
                {
                    DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView mainGrid = (DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView)dgv.MainView;
                    mainGrid.Columns["help"].Visible = false;
                    mainGrid.Columns["data"].OptionsColumn.FixedWidth = true;
                    title = "Customer Demographics " + myTitle;
                    printPreviewMenuItem(sender, e, dgv);
                    mainGrid.Columns["help"].Visible = true;
                    mainGrid.Columns["data"].OptionsColumn.FixedWidth = false;
                }
            }
            else if (panelFamilyActive)
                editFunFamilyNew.FireEventPrintPreview("Family");
            else if (panelLegalActive)
                editFunFamilyNew.FireEventPrintPreview("Legal");
            else if (panelServicesActive)
                editFunServices.FireEventPrintPreview();
            else if ( panelPaymentsActive )
            {
                DevExpress.XtraGrid.GridControl dgv = editFunPayments.FireEventPrintPreview();
                if (dgv != null)
                {
                    DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView mainGrid = (DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView)dgv.MainView;
                    title = "Funeral Payments " + myTitle;
                    printPreviewMenuItem(sender, e, dgv, true );
                }
            }
        }
        /****************************************************************************************/
        private void EditFunFamilyNew_funFamilyPrint( string who, DevExpress.XtraGrid.GridControl dgv1 )
        {
            string myTitle = BuildTitle();

            DataTable dt = (DataTable) dgv1.DataSource;
            if (dt.Rows.Count > 0)
            {
                DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView mainGrid = (DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView)dgv1.MainView;
                title = who + " " + myTitle;
                bool landscape = true;
                if (who.ToUpper() == "FUNERAL DETAILS")
                {
                    mainGrid.Columns["help"].Visible = false;
                    mainGrid.Columns["data"].OptionsColumn.FixedWidth = true;
                    landscape = false;
                }

                printPreviewMenuItem(mySender, myEventArgs, dgv1, landscape);

                if (who.ToUpper() == "FUNERAL DETAILS")
                {
                    mainGrid.Columns["help"].Visible = true;
                    mainGrid.Columns["data"].OptionsColumn.FixedWidth = false;
                }
            }
        }
        /****************************************************************************************/
    }
}