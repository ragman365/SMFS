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
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class Tickle : DevExpress.XtraEditors.XtraForm
    {
        private string workFromUser = "";
        private string workMessage = "";
        private string workSubject = "";
        public bool TickleSent = false;
        /***********************************************************************************************/
        public Tickle( string fromUser, string subject, string message )
        {
            workFromUser = fromUser;
            workSubject = subject;
            workMessage = message;
            InitializeComponent();
        }
        /***********************************************************************************************/
        private void Tickle_Load(object sender, EventArgs e)
        {
            this.Text = "Tickle from " + workFromUser;
            txtSubject.Text = workSubject;
            rtb.Text = workMessage;

            string cmd = "Select * from `users` order by lastName, firstName";
            DataTable dx = G1.get_db_data(cmd);
            dx.Columns.Add("name");
            string mainUser = "";
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                dx.Rows[i]["name"] = dx.Rows[i]["firstName"] + " " + dx.Rows[i]["lastName"].ObjToString();
                if (dx.Rows[i]["record"].ObjToString() == LoginForm.workUserRecord)
                    mainUser = dx.Rows[i]["name"].ObjToString();
            }
            chkComboLocNames.Properties.DataSource = dx;
            chkComboLocNames.Text = mainUser;
            chkComboLocNames.EditValue = mainUser;
        }
        /***********************************************************************************************/
        private void btnSend_Click(object sender, EventArgs e)
        {
            TickleSent = true;
            string message = "Tickle Reminder :\n\n" + rtb.Text;
            string from = workFromUser;
            string subject = "T: " + txtSubject.Text;
            string to = from;
            string sendTo = from;
            string record = "";

            DateTime tdate = this.dateTimePicker1.Value;

            string tickleDate = tdate.Year.ToString("D4") + "-" + tdate.Month.ToString("D2") + "-" + tdate.Day.ToString("D2") + " ";
            tdate = this.dateTimePicker2.Value;
            tickleDate += tdate.Hour.ToString("D2") + ":" + tdate.Minute.ToString("D2") + ":00";

            string toRecord = LoginForm.workUserRecord;
            string fromRecord = LoginForm.workUserRecord;

            DataTable dx = (DataTable)chkComboLocNames.Properties.DataSource;

            to = chkComboLocNames.Text;
            string[] Lines = to.Split('|');

            for (int i = 0; i < Lines.Length; i++)
            {
                sendTo = Lines[i].ToUpper();
                DataRow [] dRow = dx.Select("name='" + sendTo + "'");
                if (dRow.Length > 0)
                {
                    toRecord = dRow[0]["record"].ObjToString();
                    record = G1.create_record("messages", "fromUser", "-1");
                    if (G1.BadRecord("messages", record))
                        continue;
                    G1.update_db_table("messages", "record", record, new string[] { "fromUser", from, "toUser", sendTo, "subject", subject, "message", message, "senddate", tickleDate, "fromRecord", fromRecord, "toRecord", toRecord, "type", "tickle" });
                }
            }


            //record = G1.create_record("messages", "fromUser", "-1");
            //if (!G1.BadRecord("messages", record))
            //    G1.update_db_table("messages", "record", record, new string[] { "fromUser", from, "toUser", sendTo, "subject", subject, "message", message, "senddate", tickleDate, "fromRecord", fromRecord, "toRecord", toRecord, "type", "tickle" });
            this.Close();
        }
        /***********************************************************************************************/
        private void Tickle_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!TickleSent )
            {
                DialogResult result = MessageBox.Show("Do you want to exit without sending Tickle?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if ( result == DialogResult.No )
                    e.Cancel = true;
            }
        }

        private void panelTop_Paint(object sender, PaintEventArgs e)
        {

        }
        /***********************************************************************************************/
    }
}