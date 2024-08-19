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
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class EditCustomer : DevExpress.XtraEditors.XtraForm
    {
        private bool loading = true;
        private string workContract = "";
        private string originalTitle = "";
        private DataTable workDt = null;
        public static string activeFuneralHomeName = "";
        public static string activeFuneralHomeGroup = "";
        public static string activeFuneralHomeCasketGroup = "";
        /****************************************************************************************/
        public EditCustomer(string contract = "")
        {
            InitializeComponent();
            workContract = contract;
        }
        /****************************************************************************************/
        private void EditCustomer_Load(object sender, EventArgs e)
        {
            originalTitle = "";
            if (!String.IsNullOrWhiteSpace(workContract))
            {
                string cmd = "Select * from `contracts` c JOIN `customers` z ON c.`contractNumber` = z.`contractNumber` where c.`contractNumber` = '" + workContract + "';";
                workDt = G1.get_db_data(cmd);
                if (workDt.Rows.Count > 0)
                {
                    string title = CustomerDetails.BuildClientTitle(workDt.Rows[0]);
                    this.Text = title;
                    originalTitle = title;
                }
            }

            DetermineWorkingFuneralHome();
            if (String.IsNullOrWhiteSpace(activeFuneralHomeName))
            {
                return;
            }
            this.Text += " [" + activeFuneralHomeName + "]  [Group: " + activeFuneralHomeGroup + "]  [CasketGroup: " + activeFuneralHomeCasketGroup + "]";

            txtFuneralHome.Text = activeFuneralHomeName;
            txtGPLGroup.Text = activeFuneralHomeGroup;
            txtCasketGroup.Text = activeFuneralHomeCasketGroup;

            EditCust.activeFuneralHomeGroup = activeFuneralHomeGroup;
            EditCust.activeFuneralHomeCasketGroup = activeFuneralHomeCasketGroup;
            EditCust.activeFuneralHomeName = activeFuneralHomeName;


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
            panelDesign.Show();

            positionAll();
            LoadFuneralInfo();
            loading = false;
        }
        /****************************************************************************************/
        private void DetermineWorkingFuneralHome()
        {
            activeFuneralHomeCasketGroup = "";
            activeFuneralHomeGroup = "";
            activeFuneralHomeName = "";
            if (String.IsNullOrWhiteSpace(LoginForm.activeFuneralHomeKeyCode))
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
            string cmd = "Select * from `funeralhomes` where `keycode` = '" + LoginForm.activeFuneralHomeKeyCode + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
            {
                MessageBox.Show("***ERROR*** Invalid Funeral Home!");
                this.Close();
                return;
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

            AddDesign(this.panelForms, DockStyle.Bottom, ref panelFormsMoved);
            AddDesign(this.panelLegal, DockStyle.Top, ref panelLegalMoved);
            AddDesign(this.panelPayments, DockStyle.Top, ref panelPaymentsMoved);
            AddDesign(this.panelServices, DockStyle.Top, ref panelServicesMoved);
            AddDesign(this.panelFamily, DockStyle.Top, ref panelFamilyMoved);
            AddDesign(this.panelCustomer, DockStyle.Top, ref panelCustomerMoved);

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
                                panelAll.ScrollControlIntoView(control);
                                control.Focus();
                                //control.Dock = DockStyle.Top;
                            }
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

            string contractNumber = s;
            string contracts = "contracts";
            string customers = "customers";
            if (DailyHistory.isInsurance(s))
            {
                contracts = "icontracts";
                customers = "icustomers";
            }

            string cmd = "Select * from `" + contracts + "` c JOIN `" + customers + "` b ON c.`contractNumber` = b.`contractNumber` where c.`contractNumber` = '" + contractNumber + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return;
            workContract = contractNumber;
            string firstName = dt.Rows[0]["firstName"].ObjToString();
            string lastName = dt.Rows[0]["lastName"].ObjToString();
            string title = CustomerDetails.BuildClientTitle(dt.Rows[0]);
            this.Text = title;
            InitializeCustomerPanel();
            InitializeServicePanel();
            InitializeFamilyPanel();
            InitializePaymentsPanel();
            InitializeFormsPanel();
            toolCustomerClick(true);
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
                    }
                    else
                    {
                        this.panelAll.SuspendLayout();
                        this.panelAll.Controls.Add(this.panelCustomer);
                        this.panelAll.ResumeLayout(false);
                        this.panelAll.PerformLayout();
                        this.panelCustomer.Dock = DockStyle.Top;
                        panelCustomerActive = true;
                        panelCustomerMoved = true;
                        panelCustomer.Show();
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
                if (editFunFamily == null)
                {
                    this.Cursor = Cursors.WaitCursor;
                    InitializeFamilyPanel();
                    this.Cursor = Cursors.Default;
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
                    }
                    else
                    {
                        this.panelAll.SuspendLayout();
                        this.panelAll.Controls.Add(this.panelFamily);
                        this.panelAll.ResumeLayout(false);
                        this.panelAll.PerformLayout();
                        this.panelFamily.Dock = DockStyle.Bottom;
                        panelFamilyActive = true;
                        panelFamilyMoved = true;
                    }
                }
                ResetPanels(panelFamily);
            }
            catch (Exception ex)
            {

            }
        }
        /****************************************************************************************/
        private void ClearAllPanels()
        {
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
                //ClearAllPanels();
                //FunServices fService = new FunServices(this, workContract);
                //fService.Show();

                if (editFunServices == null)
                {
                    this.Cursor = Cursors.WaitCursor;
                    InitializeServicePanel();
                    this.Cursor = Cursors.Default;
                }
                if (panelServicesActive)
                {
                    toolServices.BackColor = Color.Transparent;
                    this.panelServices.Hide();
                    panelServicesActive = false;
                }
                else
                {
                    if (panelServicesMoved)
                    {
                        toolServices.BackColor = Color.Yellow;
                        panelServices.Show();
                        panelServices.Focus();
                        panelServicesActive = true;
                    }
                    else
                    {
                        this.panelAll.SuspendLayout();
                        this.panelAll.Controls.Add(this.panelServices);
                        this.panelAll.ResumeLayout(false);
                        this.panelAll.PerformLayout();
                        this.panelServices.Dock = DockStyle.Bottom;
                        panelServicesActive = true;
                        panelServicesMoved = true;
                    }
                }
                ResetPanels(panelServices);
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
                    }
                    else
                    {
                        this.panelAll.SuspendLayout();
                        this.panelAll.Controls.Add(this.panelPayments);
                        this.panelAll.ResumeLayout(false);
                        this.panelAll.PerformLayout();
                        this.panelPayments.Dock = DockStyle.Bottom;
                        panelPaymentsActive = true;
                        panelPaymentsMoved = true;
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
                if (editFunLegal == null)
                {
                    this.Cursor = Cursors.WaitCursor;
                    InitializeLegalPanel();
                    this.Cursor = Cursors.Default;
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
                    }
                    else
                    {
                        this.panelAll.SuspendLayout();
                        this.panelAll.Controls.Add(this.panelLegal);
                        this.panelAll.ResumeLayout(false);
                        this.panelAll.PerformLayout();
                        this.panelLegal.Dock = DockStyle.Bottom;
                        panelLegalActive = true;
                        panelLegalMoved = true;
                    }
                }
                ResetPanels(panelLegal);
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
                    }
                    else
                    {
                        this.panelAll.SuspendLayout();
                        this.panelAll.Controls.Add(this.panelForms);
                        this.panelAll.ResumeLayout(false);
                        this.panelAll.PerformLayout();
                        this.panelForms.Dock = DockStyle.Bottom;
                        panelFormsActive = true;
                        panelFormsMoved = true;
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
        private void InitializeCustomerPanel()
        {
            if (editFunCustomer != null)
                editFunCustomer.Close();
            editFunCustomer = null;
            custModified = false;
            G1.ClearPanelControls(this.panelCustomer);

            editFunCustomer = new FunCustomer(workContract, "", true );
            if (!this.LookAndFeel.UseDefaultLookAndFeel)
            {
                editFunCustomer.LookAndFeel.UseDefaultLookAndFeel = false;
                editFunCustomer.LookAndFeel.SetSkinStyle(this.LookAndFeel.SkinName);
            }
            G1.LoadFormInPanel(editFunCustomer, this.panelCustomer);
            //            this.panelCustomer.Dock = DockStyle.Top;
        }
        /***********************************************************************************************/
        private FunFamily editFunFamily = null;
        private bool familyModified = false;
        private void InitializeFamilyPanel()
        {
            if (editFunFamily != null)
                editFunFamily.Close();
            editFunFamily = null;
            familyModified = false;
            G1.ClearPanelControls(this.panelFamily);

            editFunFamily = new FunFamily(workContract, true);
            if (!this.LookAndFeel.UseDefaultLookAndFeel)
            {
                editFunFamily.LookAndFeel.UseDefaultLookAndFeel = false;
                editFunFamily.LookAndFeel.SetSkinStyle(this.LookAndFeel.SkinName);
            }
            G1.LoadFormInPanel(editFunFamily, this.panelFamily);
            this.panelFamily.Dock = DockStyle.Top;
        }
        /***********************************************************************************************/
        private FunServices editFunServices = null;
        private bool funModified = false;
        private void InitializeServicePanel()
        {
            if (editFunServices != null)
                editFunServices.Close();
            editFunServices = null;
            funModified = false;
            G1.ClearPanelControls(this.panelServices);

            editFunServices = new FunServices(this, workContract, true );
            if (!this.LookAndFeel.UseDefaultLookAndFeel)
            {
                editFunServices.LookAndFeel.UseDefaultLookAndFeel = false;
                editFunServices.LookAndFeel.SetSkinStyle(this.LookAndFeel.SkinName);
            }
            G1.LoadFormInPanel(editFunServices, this.panelServices);
            int left = panelDesign.Left;
            int top = panelDesign.Top;
            int width = panelDesign.Width;
            int height = editFunServices.Height;
            this.panelServices.SetBounds(left, top, width, height);
            this.panelServices.Dock = DockStyle.Top;
        }
        /***********************************************************************************************/
        private FunPayments editFunPayments = null;
        private bool payModified = false;
        private void InitializePaymentsPanel()
        {
            if (editFunPayments != null)
                editFunPayments.Close();
            editFunPayments = null;
            payModified = false;
            G1.ClearPanelControls(this.panelPayments);

            editFunPayments = new FunPayments(this, workContract);
            if (!this.LookAndFeel.UseDefaultLookAndFeel)
            {
                editFunPayments.LookAndFeel.UseDefaultLookAndFeel = false;
                editFunPayments.LookAndFeel.SetSkinStyle(this.LookAndFeel.SkinName);
            }
            G1.LoadFormInPanel(editFunPayments, this.panelPayments);
            this.panelPayments.Dock = DockStyle.Top;
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

            editFunLegal = new FunFamily(workContract, true);
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
                InitializeServicePanel();
            if (editFunPayments == null)
                InitializePaymentsPanel();

            DataTable servicesDt = editFunServices.FireEventFunServicesReturn();
            DataTable paymentsDt = editFunPayments.FireEventFunPaymentsReturn();

            editFunForms = new FunForms(this, workContract, servicesDt, paymentsDt);
            if (!this.LookAndFeel.UseDefaultLookAndFeel)
            {
                editFunForms.LookAndFeel.UseDefaultLookAndFeel = false;
                editFunForms.LookAndFeel.SetSkinStyle(this.LookAndFeel.SkinName);
            }
            G1.LoadFormInPanel(editFunForms, this.panelForms);
            this.panelForms.Dock = DockStyle.Top;
        }
        /****************************************************************************************/
        private void EditCustomer_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (checkForModified())
            {
                DialogResult result = MessageBox.Show("***Question***\nData has been modified!\nWould you like to save your changes?", "Data Modified Dialog", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
                if (result == DialogResult.Yes)
                    SaveAllData();
            }
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
            if (editFunServices != null)
                editFunServices.Close();
            if (editFunCustomer != null)
                editFunCustomer.Close();
            editFunForms = null;
            editFunPayments = null;
            editFunFamily = null;
            editFunServices = null;
            editFunCustomer = null;
        }
        /****************************************************************************************/
        private void SaveAllData()
        {
            if (familyModified)
                editFunFamily.FireEventSaveFunServices(true);
            if (funModified)
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
        private bool checkForModified()
        {
            bool modified = false;
            if (editFunFamily != null)
            {
                familyModified = editFunFamily.FireEventFunServicesModified();
                if (familyModified)
                    modified = true;
            }
            if (editFunServices != null)
            {
                funModified = editFunServices.FireEventFunServicesModified();
                if (funModified)
                    modified = true;
            }
            if (editFunCustomer != null)
            {
                custModified = editFunCustomer.FireEventFunServicesModified();
                if (custModified)
                    modified = true;
            }
            if (editFunPayments != null)
            {
                payModified = editFunPayments.FireEventFunServicesModified();
                if (payModified)
                    modified = true;
            }
            if (editFunLegal != null)
            {
                legalModified = editFunLegal.FireEventFunServicesModified();
                if (legalModified)
                    modified = true;
            }
            if (editFunForms != null)
            {
                formsModified = editFunForms.FireEventFunServicesModified();
                if (formsModified)
                    modified = true;
            }
            return modified;
        }
        /****************************************************************************************/
        private void btnContracts_Click(object sender, EventArgs e)
        {
            using (NewContract contractForm = new NewContract())
            {
                contractForm.SelectDone += ContractForm_SelectDone;
                contractForm.ShowDialog();
            }
        }
        /***********************************************************************************************/
        private void ContractForm_SelectDone(string contract)
        {
            if (String.IsNullOrWhiteSpace(contract))
                return;
            this.Cursor = Cursors.WaitCursor;
            workContract = contract;
            FastForm_ListDone(contract);
            this.Cursor = Cursors.Default;
        }
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
        [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
        protected override void WndProc(ref Message m)
        {
            // 0x210 is WM_PARENTNOTIFY
            // 513 is WM_LBUTTONCLICK
            if (m.Msg == 0x210 && m.WParam.ToInt32() == 513)
            {
                //                int actualHeight = FindMainWindowHeight();

                // get the clicked position
                var x = (int)(m.LParam.ToInt32() & 0xFFFF);
                var y = (int)(m.LParam.ToInt32() >> 16);

                // get the clicked control
                var childControl = this.GetChildAtPoint(new Point(x, y));
                childControl.Focus();

                // call onClick (which fires Click event)
                //Point point = panelAll.AutoScrollPosition;
                //panelAll.AutoScroll = false;
                //panelAll.VerticalScroll.Enabled = false;
                //                OnClick(EventArgs.Empty);
                //                panelAll.AutoScrollPosition = lastPoint;
                //panelAll.VerticalScroll.Enabled = true;
                //panelAll.AutoScroll = true;
                //if (lastControl != null)
                //{
                //    ResetPanels((Panel)lastControl);
                //    panelAll.AutoScrollPosition = lastPoint;
                //    panelAll.AutoScroll = true;
                //}

                // do something else...
            }
            base.WndProc(ref m);
        }
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
                string cmd = "Select * from `cust_extended` where `contractNumber` = '" + workContract + "';";
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
                string cmd = "Select * from `cust_extended` where `contractNumber` = '" + workContract + "';";
                DataTable dt = G1.get_db_data(cmd);
                if (dt.Rows.Count > 0)
                {
                    string record = dt.Rows[0]["record"].ObjToString();
                    G1.update_db_table("cust_extended", "record", record, new string[] { "Funeral Director", director });
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
                string cmd = "Select * from `cust_extended` where `contractNumber` = '" + workContract + "';";
                DataTable dt = G1.get_db_data(cmd);
                if (dt.Rows.Count > 0)
                {
                    string record = dt.Rows[0]["record"].ObjToString();
                    G1.update_db_table("cust_extended", "record", record, new string[] { "Funeral Arranger", director });
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
            activeFuneralHomeName = dt.Rows[0]["LocationCode"].ObjToString();
            if (String.IsNullOrWhiteSpace(activeFuneralHomeName))
                activeFuneralHomeName = dt.Rows[0]["name"].ObjToString();
            activeFuneralHomeGroup = dt.Rows[0]["groupname"].ObjToString();
            activeFuneralHomeCasketGroup = dt.Rows[0]["casketgroup"].ObjToString();

            this.Text = originalTitle;
            this.Text += " [" + activeFuneralHomeName + "]  [Group: " + activeFuneralHomeGroup + "]  [CasketGroup: " + activeFuneralHomeCasketGroup + "]";

            txtFuneralHome.Text = activeFuneralHomeName;
            txtGPLGroup.Text = activeFuneralHomeGroup;
            txtCasketGroup.Text = activeFuneralHomeCasketGroup;
        }
        /****************************************************************************************/
    }
}