using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Cryptography;
using DevExpress.LookAndFeel;
using DevExpress.XtraEditors;
using DevExpress.XtraBars;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Base.ViewInfo;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraPrinting;
using GeneralLib;
/***********************************************************************************************/
namespace SMFS
{
    public partial class addUser : DevExpress.XtraEditors.XtraForm
    {
        private bool firstUser = false;
        private string workRecord = "";
        private string dialogTitle = "Add User Dialog";
        bool modified = false;
        bool loading = true;
        /***********************************************************************************************/
        public addUser( bool first = false, string record = "" )
        {
            InitializeComponent();
            firstUser = first;
            workRecord = record;
        }
        /***********************************************************************************************/
        private void addUser_Load(object sender, EventArgs e)
        {
            modified = false;
            btnSave.Hide();
            if (!firstUser && !LoginForm.administrator)
                btnToggleAdmin.Hide();
            if ( !String.IsNullOrWhiteSpace(workRecord))
            {
                dialogTitle = "Edit User Dialog";
                string cmd = "Select * from `users` where `record` = '" + workRecord + "';";
                DataTable dt = G1.get_db_data(cmd);
                if ( dt.Rows.Count <= 0 )
                {
                    MessageBox.Show("***ERROR*** Trouble locating User Record " + workRecord + "!", dialogTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Close();
                }
                txtUserName.Text = dt.Rows[0]["userName"].ObjToString();
                txtFirstName.Text = dt.Rows[0]["firstName"].ObjToString();
                txtLastName.Text = dt.Rows[0]["lastName"].ObjToString();
                txtEmail.Text = dt.Rows[0]["email"].ObjToString();
                txtAgentCode.Text = dt.Rows[0]["agentCode"].ObjToString();
                string password = dt.Rows[0]["pwd"].ObjToString();
                if ( !String.IsNullOrWhiteSpace(password))
                {
                    textBox1.Text = "**************";
                    textBox2.Text = "**************";
                    btnShowPassword.Text = "Change Password";
                }
                string admin = dt.Rows[0]["admin"].ObjToString();
                if (admin.ToUpper() == "TRUE")
                {
                    btnToggleAdmin.BackColor = Color.Green;
                    btnToggleAdmin.ForeColor = Color.White;
                }
                else
                {
                    btnToggleAdmin.BackColor = Color.Red;
                    btnToggleAdmin.ForeColor = Color.White;
                }
            }
            loading = false;
        }
        /***********************************************************************************************/
        private bool showPWD = false;
        /***********************************************************************************************/
        private void btnShowPassword_Click(object sender, EventArgs e)
        {
            if ( btnShowPassword.Text.ToUpper() == "CHANGE PASSWORD")
            {
                textBox1.Text = "";
                textBox2.Text = "";
                btnShowPassword.Text = "Show Password";
                return;
            }
            if ( !showPWD )
            {
                showPWD = true;
                textBox1.PasswordChar = '\0';
                textBox2.PasswordChar = '\0';
            }
            else
            {
                showPWD = false;
                textBox1.PasswordChar = '*';
                textBox2.PasswordChar = '*';
            }
        }
        /***********************************************************************************************/
        private const string invalidchars = @"';:/?.>,<]}[{\|=+-_)(*&^%$#@!`~";
        /***********************************************************************************************/
        private void btnSave_Click(object sender, EventArgs e)
        {
            string userName = txtUserName.Text.Trim();
            if ( String.IsNullOrWhiteSpace ( userName))
            {
                MessageBox.Show("***ERROR*** You must provide a userName", "Add User Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (userName.IndexOfAny(invalidchars.ToCharArray()) > -1)
            {
                MessageBox.Show("***ERROR*** An invalid character was used for the Username!", "Add User Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            string firstName = txtFirstName.Text.Trim();
            if (String.IsNullOrWhiteSpace(firstName))
            {
                MessageBox.Show("***ERROR*** You must provide a firstName", "Add User Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            string lastName = txtLastName.Text.Trim();
            if (String.IsNullOrWhiteSpace(userName))
            {
                MessageBox.Show("***ERROR*** You must provide a lastName", "Add User Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string PasswordHash = "";
            if (btnShowPassword.Text.ToUpper() == "SHOW PASSWORD")
            {
                string p1 = textBox1.Text.Trim();
                if (String.IsNullOrWhiteSpace(userName))
                {
                    MessageBox.Show("***ERROR*** You must provide a Password", "Add User Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (p1.IndexOfAny(invalidchars.ToCharArray()) > -1)
                {
                    MessageBox.Show("***ERROR*** An invalid character was used for the Password!", "Add User Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                string p2 = textBox2.Text.Trim();
                if (String.IsNullOrWhiteSpace(userName))
                {
                    MessageBox.Show("***ERROR*** You must verify your Password", "Add User Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (p1 != p2)
                {
                    MessageBox.Show("***ERROR*** Your password and your verification password does not match!", "Add User Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                PasswordHash = LoginForm.Hash(p1);
            }

            firstName = G1.force_lower_line(firstName);
            lastName = G1.force_lower_line(lastName);
            string email = txtEmail.Text.Trim();
            string agentCode = txtAgentCode.Text.Trim();

            string record = workRecord;
            if ( String.IsNullOrWhiteSpace ( record ))
                record = G1.create_record("users", "firstName", "-1");
            G1.update_db_table("users", "record", record, new string[] { "userName", userName, "firstName", firstName, "lastName", lastName, "status", "active", "email", email, "agentCode", agentCode });
            if ( !String.IsNullOrWhiteSpace(PasswordHash ))
                G1.update_db_table("users", "record", record, new string[] { "pwd", PasswordHash });
            bool admin = false;
            if ( firstUser || btnToggleAdmin.BackColor == Color.Green)
                admin = true;
            if ( admin )
                G1.update_db_table("users", "record", record, new string[] { "admin", "1" });
            else
                G1.update_db_table("users", "record", record, new string[] { "admin", "0" });
            this.Close();
        }
        /***********************************************************************************************/
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
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
        public static bool CheckRecoveryPassword(string password)
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
        /***********************************************************************************************/
        private void btnToggleAdmin_Click(object sender, EventArgs e)
        {
            if ( btnToggleAdmin.BackColor == Color.Green )
            {
                btnToggleAdmin.BackColor = Color.Red;
                btnToggleAdmin.ForeColor = Color.White;
            }
            else
            {
                btnToggleAdmin.BackColor = Color.Green;
                btnToggleAdmin.ForeColor = Color.White;
            }
            something_TextChanged(null, null);
        }
        /***********************************************************************************************/
        private void something_TextChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            modified = true;
            btnSave.Show();
            btnSave.BackColor = Color.Green;
        }
        /***********************************************************************************************/
    }
}