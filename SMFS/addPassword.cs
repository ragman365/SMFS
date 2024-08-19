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
    public partial class addPassword : DevExpress.XtraEditors.XtraForm
    {
        private bool modified = false;
        private bool loading = true;
        private string oldPWD = "";

        private string _answer = "";

        public string Answer { get { return _answer; } }
        /***********************************************************************************************/
        public addPassword ( string pwd )
        {
            InitializeComponent();
            oldPWD = pwd;
        }
        /***********************************************************************************************/
        private void addPassword_Load(object sender, EventArgs e)
        {
            modified = false;
            btnSave.Hide();
            if (!String.IsNullOrWhiteSpace(oldPWD))
            {
                textBox1.Text = "**************";
                textBox2.Text = "**************";
                btnShowPassword.Text = "Change Password";
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
            string PasswordHash = "";
            if (btnShowPassword.Text.ToUpper() == "SHOW PASSWORD")
            {
                string p1 = textBox1.Text.Trim();

                if (p1.IndexOfAny(invalidchars.ToCharArray()) > -1)
                {
                    MessageBox.Show("***ERROR*** An invalid character was used for the Password!", "Add User Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                string p2 = textBox2.Text.Trim();
                if (p1 != p2)
                {
                    MessageBox.Show("***ERROR*** Your password and your verification password does not match!", "Add User Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                PasswordHash = LoginForm.Hash(p1);
            }

            _answer = textBox1.Text;
            this.DialogResult = DialogResult.OK;

            //string record = workRecord;
            //if ( String.IsNullOrWhiteSpace ( record ))
            //    record = G1.create_record("users", "firstName", "-1");
            //G1.update_db_table("users", "record", record, new string[] { "userName", userName, "firstName", firstName, "lastName", lastName, "status", "active", "email", email, "agentCode", agentCode });
            //if ( !String.IsNullOrWhiteSpace(PasswordHash ))
            //    G1.update_db_table("users", "record", record, new string[] { "pwd", PasswordHash });
            //bool admin = false;
            //if ( firstUser || btnToggleAdmin.BackColor == Color.Green)
            //    admin = true;
            //if ( admin )
            //    G1.update_db_table("users", "record", record, new string[] { "admin", "1" });
            //else
            //    G1.update_db_table("users", "record", record, new string[] { "admin", "0" });
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