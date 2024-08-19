using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using GeneralLib;
/***********************************************************************************************/

namespace SMFS
{
    /***********************************************************************************************/
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        /***********************************************************************************************/
        static void Main(string[] args)
        {
            //MessageBox.Show("***HERE***");
            if (args.Length > 0)
            {
                //MessageBox.Show("*** Arg=" + args[0].ObjToString());
                //G1.AddToAudit("System", "AutoRun", "AutoRun", "Starting Reports . . . . . . . ", "");
            }
            if (1 != 1)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                using (LoginForm logForm = new LoginForm())
                {
                    DialogResult result = logForm.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        Application.Run(new SMFS());
                    }
                }
            }
            else
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                string user = "";
                string str = "";
                if ( 1 != 1 )
                {
                    user = "RGRAHAM1952";
                    LoginForm.RobbyLocal = true;
                    string filename = @"C:\SMFS_Reports\LapseReport_20190904.pdf";
                    string sendTo = "(Robby) Robby Graham";
                    string sendWhere = "Local";
                    //RemoteProcessing.AutoRunSendTo( "Lapse Report", filename, sendTo, sendWhere );
                    RemoteProcessing pForm = new RemoteProcessing();
                }
                else if (args.Length > 0)
                {
                    str = args[0].ObjToString();
                    if (str.Trim().ToUpper() == "-AUTORUN")
                    {
                        user = "RGRAHAM1952";
                        LoginForm.RobbyLocal = true;
                        RemoteProcessing pForm = new RemoteProcessing();
                    }
                    //if (str.Trim().ToUpper() == "-LAPSE")
                    //{
                    //    LoginForm.doLapseReport = true;
                    //    user = "RGRAHAM1952";
                    //    LoginForm.RobbyLocal = true;
                    //}
                }
                else
                {
                    if (LoginForm.doLapseReport)
                    {
                        //Trust85 trustForm = new Trust85();
                        //trustForm.Show();
                        PastDue pastForm = new PastDue();
                        pastForm.Show();
                        return;
                    }
                    try
                    {
                        //string local_user = Environment.GetEnvironmentVariable("USERNAME").ToUpper();
                        //if (local_user.Trim().ToUpper() == "ROBBY")
                        //{
                        //    string platform = Environment.GetEnvironmentVariable("PLATFORMCODE").ToUpper();
                        //    if (platform.ToUpper() == "KV")
                        //    {
                        //        user = "RGRAHAM1952";
                        //        LoginForm.RobbyLocal = true;
                        //    }
                        //}
                        if (String.IsNullOrWhiteSpace(user))
                        {
                            using (LoginForm logForm = new LoginForm())
                            {
                                DialogResult result = logForm.ShowDialog();
                                if (result == DialogResult.OK)
                                {
                                    Application.Run(new SMFS());
                                }
                            }
                        }
                        else
                        {
                            using (LoginForm logForm = new LoginForm(user))
                            {
                                DialogResult result = logForm.ShowDialog();
                                if (result == DialogResult.OK)
                                {
                                    if (user == "RGRAHAM1952")
                                        LoginForm.username = "ROBBY";
                                    Application.Run(new SMFS());
                                }
                            }
                        }
                    }
                    catch ( Exception ex)
                    {
                        using (LoginForm logForm = new LoginForm())
                        {
                            DialogResult result = logForm.ShowDialog();
                            if (result == DialogResult.OK)
                            {
                                Application.Run(new SMFS());
                            }
                        }
                    }
                }
            }
        }
    }
        /***********************************************************************************************/
}
