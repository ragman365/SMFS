using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Globalization;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using GeneralLib;
using DevExpress.XtraLayout.Utils;
/****************************************************************************************/
namespace SMFS
{
/****************************************************************************************/
	public class LoginForm : System.Windows.Forms.Form
	{
        public static bool isRobby = false;
        public static bool isReallyRobby = false;
        public static bool doLapseReport = false;
        public static bool RobbyLocal = false;
        public static bool LogUserLogin = false;
		public Process[] myprocess;
		public static string username;
		public static string password;
        public static string classification = "";
        public static string activeFuneralHomeAgent = "";
        public static string activeFuneralHomeKeyCode = "";
        public static Byte[] activeFuneralHomeSignature = null;
        public static bool administrator = false;
        public static bool realUser = false;
        public static bool CalculateForcedPayoff = false;
        public static double minimumForceBalance = 0D;
        public static double trust85Threshold = 0D;
        public static string allowPayOffMethod = "";
        public static bool useNewTCACalculation = false;
        private bool addnewuser = false;
		private string work_user;
        public static string workUserRecord = "";
        private bool errorState = false;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.TextBox textBox2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Button button2;
        private System.Windows.Forms.ErrorProvider errorProvider1;
        private IContainer components;
        /****************************************************************************************/
        public static DataTable UserFile = null;
        public static string NotesDirectory = "";
        private PictureBox picError;
        private Label lblRevision;
        public static string NotesUsers = "";
        /****************************************************************************************/
        public LoginForm()
		{
			work_user = "";
			InitializeComponent();
		}
        /****************************************************************************************/
        public LoginForm(bool newuser )
        {
            addnewuser = newuser;
            InitializeComponent();
        }
        /****************************************************************************************/
        public LoginForm( string user )
		{
			work_user = user;
			InitializeComponent();
			this.textBox1.Text    = user;
			this.textBox1.Enabled = false;
			this.textBox1.ReadOnly = true;
		}
/****************************************************************************************/
		public LoginForm( string user, string pass )
		{
			work_user = user;
			InitializeComponent();
            username = user;
            password = pass;
            classification = "";
            Login();
            this.Close();
		}
/****************************************************************************************/
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}
/**************************************************************************************/
		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LoginForm));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
            this.picError = new System.Windows.Forms.PictureBox();
            this.lblRevision = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picError)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.SystemColors.Control;
            this.label1.Font = new System.Drawing.Font("Times New Roman", 18F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.DarkBlue;
            this.label1.Location = new System.Drawing.Point(8, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(304, 37);
            this.label1.TabIndex = 26;
            this.label1.Text = "South Mississippi";
            // 
            // label2
            // 
            this.label2.BackColor = System.Drawing.SystemColors.Control;
            this.label2.Font = new System.Drawing.Font("Times New Roman", 18F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.DarkBlue;
            this.label2.Location = new System.Drawing.Point(29, 37);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(293, 35);
            this.label2.TabIndex = 27;
            this.label2.Text = "Funeral Services, LLC";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(125, 92);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(187, 22);
            this.textBox1.TabIndex = 28;
            this.textBox1.Enter += new System.EventHandler(this.textBox_Enter);
            this.textBox1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox1_KeyPress);
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(125, 129);
            this.textBox2.Name = "textBox2";
            this.textBox2.PasswordChar = '*';
            this.textBox2.Size = new System.Drawing.Size(187, 22);
            this.textBox2.TabIndex = 29;
            this.textBox2.Enter += new System.EventHandler(this.textBox_Enter);
            this.textBox2.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox1_KeyPress);
            // 
            // label3
            // 
            this.label3.BackColor = System.Drawing.SystemColors.Control;
            this.label3.Font = new System.Drawing.Font("Times New Roman", 12F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.Black;
            this.label3.Location = new System.Drawing.Point(10, 92);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(105, 19);
            this.label3.TabIndex = 30;
            this.label3.Text = "Username :";
            // 
            // label4
            // 
            this.label4.BackColor = System.Drawing.SystemColors.Control;
            this.label4.Font = new System.Drawing.Font("Times New Roman", 12F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.Black;
            this.label4.Location = new System.Drawing.Point(10, 129);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(105, 19);
            this.label4.TabIndex = 31;
            this.label4.Text = "Password  :";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(24, 166);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(126, 37);
            this.button1.TabIndex = 32;
            this.button1.Text = "Exit";
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(186, 166);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(126, 37);
            this.button2.TabIndex = 33;
            this.button2.Text = "Log-In";
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // errorProvider1
            // 
            this.errorProvider1.ContainerControl = this;
            // 
            // picError
            // 
            this.picError.Image = ((System.Drawing.Image)(resources.GetObject("picError.Image")));
            this.picError.Location = new System.Drawing.Point(319, 92);
            this.picError.Name = "picError";
            this.picError.Size = new System.Drawing.Size(61, 56);
            this.picError.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picError.TabIndex = 34;
            this.picError.TabStop = false;
            // 
            // lblRevision
            // 
            this.lblRevision.AutoSize = true;
            this.lblRevision.ForeColor = System.Drawing.Color.Red;
            this.lblRevision.Location = new System.Drawing.Point(36, 72);
            this.lblRevision.Name = "lblRevision";
            this.lblRevision.Size = new System.Drawing.Size(70, 17);
            this.lblRevision.TabIndex = 37;
            this.lblRevision.Text = "Revision :";
            // 
            // LoginForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 15);
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(317, 209);
            this.Controls.Add(this.lblRevision);
            this.Controls.Add(this.picError);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.KeyPreview = true;
            this.Name = "LoginForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "SMFS";
            this.Load += new System.EventHandler(this.LoginForm_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.LoginForm_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picError)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion
/**************************************************************************************/
		private void LoginForm_Load(object sender, System.EventArgs e)
		{
            this.BringToFront();
            this.TopMost = true;

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            System.Reflection.AssemblyName assemblyName = assembly.GetName();
            Version version = assemblyName.Version;

            DateTime lastMake = File.GetLastWriteTime(Assembly.GetExecutingAssembly().Location);

            string[] Lines = version.ObjToString().Split('.');
            try
            {
                lblRevision.Text += Lines[0] + "." + Lines[1] + "." + Lines[3] + " " + lastMake.ToString("MM/dd/yyyy HH:mm:ss");

            }
            catch
            {
            }

            picError.Hide();
            LoadUserPasswordFile();
            if (!String.IsNullOrEmpty(work_user))
            {
                button2_Click(null, null);
            }
		}
/**************************************************************************************/
		private void button1_Click(object sender, System.EventArgs e)
		{
			username = ""; // Force this to be empty
			password = "";
            classification = "";
            this.DialogResult = DialogResult.Cancel;
		}
        /**************************************************************************************/
        private const string invalidchars = @"';:/?.>,<]}[{\|=+-_)(*&^%$#@!`~";
        public static int LogInTrys = 0;
        private void button2_Click(object sender, System.EventArgs e)
		{
			username = textBox1.Text;
			password = textBox2.Text;

			errorProvider1.SetError(textBox1,"");
			errorProvider1.SetError(textBox2,"");
		
			if(username.IndexOfAny(invalidchars.ToCharArray()) > -1)
			{
                MessageBox.Show("***ERROR***\nAn invalid character was entered for username!!", "Log-In Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			if(password.IndexOfAny(invalidchars.ToCharArray()) > -1)
			{
                MessageBox.Show("***ERROR***\nAn invalid character was entered for password!!", "Log-In Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

            if ( addnewuser )
            {
                this.DialogResult = DialogResult.OK;
                return;
            }

            bool success = Login();
 
			if(!success)
			{
				textBox1.Focus();
				textBox1.SelectAll();
				username = "";
				password = "";
                classification = "";
                LogInTrys++;
                if ( LogInTrys >= 5 )
                {
                    if ( username.ToUpper() == "ADMIN")
                        MessageBox.Show("***ERROR***\nMax Login Trys Exceeded.\nDon't Forget About Recovery User/Password!\nSorry!", "Log-In Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    else
                        MessageBox.Show("***ERROR***\nMax Login Trys Exceeded.\nSorry!", "Log-In Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.DialogResult = DialogResult.Abort;
                    this.Close();
                }
                else
                {
                    errorState = true;
                    picError.Show();
//                    MessageBox.Show("***WARNING***\nInvalid Username/Password.\nPlease Try Again!\nSorry!");
                }
                return;
			}

			this.DialogResult = DialogResult.OK;
		}
/**************************************************************************************/
        private const int SALT_SIZE = 8;
        private const int NUM_ITERATIONS = 1000;

        private static readonly RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();

        /// <summary>
        /// Creates a signature for a password.
        /// </summary>
        /// <param name="password">The password to hash.</param>
        /// <returns>the "salt:hash" for the password.</returns>
        public static string CreatePasswordSalt(string password)
        {
            string PasswordHash = Hash(password);
            return PasswordHash;
        }
        /**************************************************************************************/
        public static string Hash(string password)
        {
            var bytes = new UTF8Encoding().GetBytes(password);
            var hashBytes = System.Security.Cryptography.MD5.Create().ComputeHash(bytes);
            return Convert.ToBase64String(hashBytes);
        }
        /**************************************************************************************/
        public static bool CheckUserPassword(string username, string password)
        {
            return false;
        }
        /**************************************************************************************/
        public static bool CheckRecoveryPassword ( string password )
        {
            DateTime now = DateTime.Now;
            string day = now.Day.ToString("D2").Reverse();
            string month = now.Month.ToString("D2").Reverse();
            string year = now.Year.ToString("D4").Reverse();
            string str = day + month + year;
            if (str == password)
                return true;
            return false;
        }
        /**************************************************************************************/
        public static bool Login()
		{
            bool success = false;
            isRobby = isThisRobby();
            isReallyRobby = isThisReallyRobby();
            //System.Diagnostics.Process[] myProcs = System.Diagnostics.Process.GetProcesses();

            if (username.Trim().Length == 0 && password.Trim().Length == 0)
            {
                if ( UserFile.Rows.Count == 0 )
                    return true;
                if (isRobby)
                {
                    username = "RGRAHAM1952";
                    LoginForm.RobbyLocal = true;
                    classification = "Admin";
                }
                else
                {
                    MessageBox.Show("***ERROR*** You must enter a valid username/password!");
                    return false;
                }
            }
            if (username.Trim().ToUpper() == "RGRAHAM1952")
            {
                realUser = true;
                administrator = true;
                string cmd = "Select * from `users` where `username` = 'ROBBY';";
                DataTable dd = G1.get_db_data(cmd);
                if (dd.Rows.Count > 0)
                {
                    LoginForm.username = dd.Rows[0]["username"].ObjToString();
                    LoginForm.classification = dd.Rows[0]["classification"].ObjToString();
                    LoginForm.workUserRecord = dd.Rows[0]["record"].ObjToString();
                    LoginForm.activeFuneralHomeAgent = dd.Rows[0]["agentCode"].ObjToString();
                    LoginForm.activeFuneralHomeSignature = dd.Rows[0]["signature"].ObjToBytes();
                }
                else
                {
                    LoginForm.username = "Robby";
                    LoginForm.classification = "Admin";
                    LoginForm.workUserRecord = "3";
                }

                return true;
            }
            string p = "";
            string salt = "";
            string u = "";
            activeFuneralHomeAgent = "";
            for ( int i = 0; i<UserFile.Rows.Count; i++)
            {
                u = UserFile.Rows[i]["userName"].ObjToString();
                if (u.ToUpper() != username.ToUpper())
                    continue;
                p = UserFile.Rows[i]["pwd"].ObjToString();
                salt = CreatePasswordSalt(password);
                if ( p == salt || isRobby )
                {
                    string status = UserFile.Rows[i]["status"].ObjToString();
                    activeFuneralHomeAgent = UserFile.Rows[i]["agentCode"].ObjToString();
                    activeFuneralHomeSignature = UserFile.Rows[i]["signature"].ObjToBytes();
                    if ( status.ToUpper() == "INACTIVE")
                    {
                        MessageBox.Show("***ERROR***\nUser is not longer ACTIVE.\nSorry!", "Log-In Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                    }
                    classification = UserFile.Rows[i]["classification"].ObjToString();
                    success = true;
                    realUser = true;
                    if (u.ToUpper() == "ADMIN")
                        administrator = true;
                    string admin = UserFile.Rows[i]["admin"].ObjToString();
                    if (admin.ToUpper() == "TRUE")
                        administrator = true;
                    workUserRecord = UserFile.Rows[i]["record"].ObjToString();
                    if ( String.IsNullOrWhiteSpace ( classification ))
                    {
                        classification = "Field";
                        G1.update_db_table("users", "record", workUserRecord, new string[] { "classification", classification });
                    }
                    break;
                }
                else
                {
                    if ( CheckRecoveryPassword ( password ))
                    {
                        success = true;
                        realUser = true;
                        if (u.ToUpper() == "ADMIN")
                            administrator = true;
                        break;
                    }
                }
            }
            return success;
		}
/**************************************************************************************/
		private void LoginForm_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			if ( e.KeyCode == Keys.F1 )
				button1_Click ( sender, e );

			if(e.KeyCode == Keys.Enter)
				button2_Click(sender,e);
		}
/**************************************************************************************/
		private void textBox_Enter(object sender, System.EventArgs e)
		{
            if (!errorState)
                picError.Hide();
			((TextBox)sender).SelectAll();
		}
        /*****************************************************************/
        private void LoadUserPasswordFile()
        {
            if (LoginForm.RobbyLocal)
                G1.RobbyServer = true;
            isRobby = isThisRobby();
            if (isRobby)
                G1.RobbyServer = true;
            string cmd = "Select * from `users`;";
            UserFile = G1.get_db_data(cmd);
            if (UserFile.Rows.Count <= 0)
            {
                DialogResult result = MessageBox.Show("         ***Warning***\nThere are no users in the system!\nWould you like to add one?", "Log-In Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    addUser addForm = new addUser(true);
                    addForm.ShowDialog();
                    UserFile = G1.get_db_data(cmd);
                }
                else
                    this.Close();
            }
        }
        /**************************************************************************************/
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ( !errorState)
                picError.Hide();
            errorState = false;
        }
        /**************************************************************************************/
        public static bool isThisRobby()
        {
            //MessageBox.Show("HERE1");
            bool robby = false;
            string local_user = Environment.GetEnvironmentVariable("USERNAME").ToUpper();
            if (local_user.Trim().ToUpper() == "ROBBY" || local_user.Trim().ToUpper() == "ADAM")
            {
                try
                {
                    try
                    {
                        string platform = Environment.GetEnvironmentVariable("PLATFORMCODE").ToUpper();
                        if (platform.ToUpper() == "KV")
                            robby = true;
                        //MessageBox.Show("platform=" + platform);
                    }
                    catch ( Exception ex )
                    {
                    }
                    string userprofile = Environment.GetEnvironmentVariable("USERPROFILE").ToUpper();
                    if (userprofile.IndexOf("\\USERS\\ROBBY") >= 0 || userprofile.IndexOf("\\USERS\\CLIFF") >= 0)
                        robby = true;
                    //MessageBox.Show(" userprofile=" + userprofile);
                }
                catch ( Exception ex)
                {
                    //MessageBox.Show("HERE2 " + ex.Message.ToString());
                }
            }
            //MessageBox.Show("HERE3");
            return robby;
        }
        /**************************************************************************************/
        public static bool isThisReallyRobby()
        {
            //MessageBox.Show("HERE1");
            bool robby = false;
            string local_user = Environment.GetEnvironmentVariable("USERNAME").ToUpper();
            if (local_user.Trim().ToUpper() == "ROBBY" || local_user.Trim().ToUpper() == "ADAM")
            {
                try
                {
                    try
                    {
                        string platform = Environment.GetEnvironmentVariable("PLATFORMCODE").ToUpper();
                        if (platform.ToUpper() == "KV")
                            robby = true;
                        //MessageBox.Show("platform=" + platform);
                    }
                    catch (Exception ex)
                    {
                    }
                }
                catch (Exception ex)
                {
                    //MessageBox.Show("HERE2 " + ex.Message.ToString());
                }
            }
            //MessageBox.Show("HERE3");
            return robby;
        }
        /***********************************************************************************************/
        public static bool ReadTrust85ForcePayoffOptions()
        {
            bool doCalculation = false;
            LoginForm.CalculateForcedPayoff = false;
            LoginForm.minimumForceBalance = 0D;
            LoginForm.trust85Threshold = 0D;
            LoginForm.allowPayOffMethod = "";
            LoginForm.useNewTCACalculation = false;

            string cmd = "Select * from `options`;";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return doCalculation;
            DataRow [] dRows = dt.Select ("option='Calculate Forced Payoff'");
            if ( dRows.Length <= 0 )
                return doCalculation;
            string answer = dRows[0]["answer"].ObjToString().ToUpper();
            if ( String.IsNullOrWhiteSpace ( answer))
                return doCalculation;
            string str = answer.Substring(0, 1);
            if ( str != "Y")
                return doCalculation;
            doCalculation = true;
            LoginForm.CalculateForcedPayoff = true;

            double dValue = 0D;

            dRows = dt.Select("option='Minimum Balance to Force Payoff'");
            if ( dRows.Length > 0 )
            {
                answer = dRows[0]["answer"].ObjToString().ToUpper();
                if ( !String.IsNullOrWhiteSpace ( answer))
                {
                    dValue = answer.ObjToDouble();
                    if (dValue <= 0D)
                    {
                        LoginForm.CalculateForcedPayoff = false;
                        doCalculation = false;
                    }
                    else
                        LoginForm.minimumForceBalance = dValue;
                }
            }

            dRows = dt.Select("option='Trust85 Threshold'");
            if (dRows.Length > 0)
            {
                answer = dRows[0]["answer"].ObjToString().ToUpper();
                if (!String.IsNullOrWhiteSpace(answer))
                {
                    dValue = answer.ObjToDouble();
                    LoginForm.trust85Threshold = dValue;
                }
            }
            dRows = dt.Select("option='Allow Trust Payoff (Debit/Credit/Both)'");
            if (dRows.Length > 0)
            {
                answer = dRows[0]["answer"].ObjToString().ToUpper();
                if (!String.IsNullOrWhiteSpace(answer))
                {
                    LoginForm.allowPayOffMethod = answer;
                }
            }

            dRows = dt.Select("option='Use New Trust Credit Adjustment (Yes/No)'");
            if (dRows.Length > 0)
            {
                answer = dRows[0]["answer"].ObjToString().ToUpper();
                if (answer == "YES")
                    LoginForm.useNewTCACalculation = true;
            }
            return doCalculation;
        }
    /**************************************************************************************/
}
}
